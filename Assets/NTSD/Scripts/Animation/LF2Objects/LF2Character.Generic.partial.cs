using BeatEmUpTemplate2D;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character : LF2LivingObject
    {
        #region Generic State Handlers

        /// <summary>
        /// 通用时间单元更新。
        /// 负责角色逐 tick 状态逻辑；复刻基准以 C++ release 的实体 tick 流程为准。
        /// </summary>
        private bool Generic_TU()
        {
            int tickIndex = SimulationTickDriver.Instance != null
                ? SimulationTickDriver.Instance.CurrentTickIndex
                : 0;

            // post_interaction 已移至 SimPostInteraction 阶段（对齐 C++ release GameMode_Process 碰撞循环）
            // 原位置：Generic_TU 开头；新位置：所有对象 SerialTickAll 完成后统一执行

            // C++ release Entity_InputProcess 0x41507E-0x415114:
            // state==301 (DeepSpecific) 或 state==19 (FirenSpecific) 时，根据左右输入更新 vz
            // 条件：(int)PS.z == 0（整数 z 坐标为 0，即在地面中心线）
            {
                int curStateForVz = Frame.D?.state ?? -1;
                if (curStateForVz == LF2States.DeepSpecific || curStateForVz == LF2States.FirenSpecific)
                {
                    bool isLeft  = Controller?.IsLeft  ?? false;
                    bool isRight = Controller?.IsRight ?? false;
                    float dvz = Frame.D?.dvz ?? 0f;
                    if (dvz != 0f && (int)PS.z == 0)
                    {
                        if (isLeft && !isRight)
                            PS.vz = -dvz;
                        else if (isRight && !isLeft)
                            PS.vz = dvz;
                    }
                }
            }

            // 死亡闪烁流程：0 开始闪烁，1~29 维持，30 后隐藏并从模拟世界注销。
            if (_deadBlinkCount == 0)
            {
                Effect.Blink = true;
                _deadBlinkCount = 1;
            }
            else if (_deadBlinkCount > 0 && _deadBlinkCount < 30)
            {
                _deadBlinkCount++;
            }
            else if (_deadBlinkCount >= 30)
            {
                Effect.Blink = false;
                Sprite?.Hide();
                _deadBlinkCount = -1;
                Match?.Unregister(this);
            }

            // fall/bdefend 每 tick 递减，字段通过 HitCounters 绑定到正式运行时。
            HitCounters.RecoverFall(NTSDGlobal.Gameplay.RecoverFall);
            HitCounters.RecoverBdefend(NTSDGlobal.Gameplay.RecoverBdefend);

            return false;
        }

        /// <summary>
        /// C++ release regenerate_pre_collision_stats。
        /// 在全局碰撞结算前处理角色 HP/PP 的自然恢复。
        /// </summary>
        public void RegeneratePreCollisionStats(int tickIndex)
        {
            if (Health == null) return;

            bool period12 = tickIndex % NTSDGlobal.Gameplay.HpRecoverPeriod == 0;
            if (Health.HP > 0 && Health.HP < Health.HPBound && period12)
            {
                Health.HP++;
            }

            if (WeaponCount < 0 && period12)
            {
                int injury = NTSDGlobal.Gameplay.NegativeWeaponCountInjury;
                if (FallDamageDiv > 0)
                    injury = NTSDGlobal.Gameplay.NegativeWeaponCountScaledInjury / FallDamageDiv;

                Health.HP -= injury;
                Health.HPBound -= injury / NTSDGlobal.Gameplay.NegativeWeaponCountHpBoundDivisor;
                if (Health.HP < 0) Health.HP = 0;
                if (Health.HPBound < 0) Health.HPBound = 0;
            }

            bool period3 = tickIndex % NTSDGlobal.Gameplay.PpRecoverPeriod == 0;
            if (!period3) return;
            if (Health.PP >= NTSDGlobal.Gameplay.PpRecoverCap) return;
            if (KillCount != -1 && Health.PP >= NTSDGlobal.Gameplay.PpRecoverLowLimit) return;
            if (HitStun < 0) return;

            int hpForRate = Health.HP;
            if (hpForRate > NTSDGlobal.Gameplay.PpRecoverCap)
                hpForRate = NTSDGlobal.Gameplay.PpRecoverCap;

            int oid = ObjectId;
            if (oid == 51 || oid == 52)
                hpForRate /= 2;

            int ppGain = ((NTSDGlobal.Gameplay.PpRecoverCap - hpForRate) /
                          NTSDGlobal.Gameplay.PpRecoverHpRateDivisor) + 1;
            Health.PP = System.Math.Min(Health.PP + ppGain, NTSDGlobal.Gameplay.PpRecoverCap);
        }

        /// <summary>
        /// 通用物理转换。
        /// C++ release 的击退帧后处理已移到 SimulationWorld.FramePostProcessAll。
        /// </summary>
        private bool Generic_Transit()
        {
            // C++ release 0x416254-0x41627C：FrameDelay 非零时跳过物理（hit_stop 冻结）
            // FrameDelay 衰减已在 base.Transit() 中完成，此处为衰减后的值
            if (FrameDelay != 0) return false;

            // Frame_PostProcess（C++ release 0x0041BF00）的 Knockback→vx/vy/vz 逻辑
            // 不在此处执行——C++ release 中该函数在所有 entity SerialTick 完成后才调用，
            // 对应 SimulationTickDriver 中 SerialTickAll 之后的独立 pass。
            // 见 SimulationWorld.FramePostProcessAll()

            // kind=14 的方向阻挡标记由 CharacterMechanics.Step() 消耗。
            // C++ release 物理只跳过本 tick 被阻挡轴的位移，并保留 vx/vz。

            // dynamics: position, friction, gravity
            ApplyDynamics();
            return false;
        }

        /// <summary>
        /// 通用帧逻辑。
        /// 处理当前帧资源消耗和 opoint 请求。
        /// </summary>
        private bool Generic_Frame()
        {
            // 角色帧的 mp 资源消耗不在进入帧时统一处理。
            // C++ release 的输入/连招跳帧由 sub_414C30 使用目标帧 mp 检查并扣除 PP/HP；
            // Unity 对应入口是 TryInputFrameJump()。
            return false;
        }

        /// <summary>
        /// PreInteraction 阶段。
        /// </summary>
        private bool Generic_PreInteraction()
        {
            LF2FrameData frame = FrameCache.GetFrameDataById(Frame.N);
            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (frame == null || sceneQuery == null) return false;
            if (PS == null) return false;

            var itrs = frame.itrs;
            if (itrs == null || itrs.Count == 0) return false;

            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return false;

            var preItrs = ListPool<InteractionArea>.Get();
            preItrs.Capacity = 4;

            for (int i = 0; i < itrs.Count; i++)
            {
                var itr = itrs[i];
                if (itr == null) continue;
                if (!kindService.IsPreInteractionKind(itr.kind)) continue;
                preItrs.Add(itr);
            }

            if (preItrs.Count == 0)
            {
                ListPool<InteractionArea>.Release(preItrs);
                return false;
            }


            var itrVolumes = PS.GetItrVolumes(preItrs, frame.centerx, frame.centery, spriteWidthPx, itrZWidthPx: NTSDGlobal.Default.Itr.ZWidth);
            int count = Mathf.Min(preItrs.Count, itrVolumes.Count);
            for (int i = 0; i < count; i++)
            {
                var itr = preItrs[i];
                var vol = itrVolumes[i];

                var candidates = sceneQuery.QueryBodies(vol, this);
                if (candidates == null || candidates.Count == 0) continue;

                for (int c = 0; c < candidates.Count; c++)
                {
                    var target = candidates[c];
                    if (!CanPreInteractTarget(kindService, itr, target)) continue;

                    if (!DispatchPreInteractionByKind(kindService, itr, target)) continue;

                    //target.ItrVrestUpdate(StableId, itr);
                    ListPool<InteractionArea>.Release(preItrs);
                    return true;
                }
            }

            ListPool<InteractionArea>.Release(preItrs);
            return false;
        }

        /// <summary>
        /// 角色攻击命中判定。
        /// 在全局 PostInteraction pass 中处理正式攻击类 itr。
        /// </summary>
        private void Generic_PostInteraction()
        {
            var frame = Frame?.D;
            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (frame == null || sceneQuery == null) return;
            if (PS == null) return;

            var itrs = frame.itrs;
            if (itrs == null || itrs.Count == 0) return;

            if (!ItrArestTest()) return;

            // 攻击方碰撞豁免守卫（C++ release 对齐 0x419E3B：[esi+0ECh] > 0 跳过整体碰撞检测）
            if (HitCounters?.AttackExempt > 0) return;

            // Falling 状态下不执行 kind=0 攻击判定
            // C++ release 中 Falling 帧（180-183）实际无 itr，等价于此过滤
            if (GetState() == LF2States.Falling) return;

            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return;

            // 普通拳脚攻击的 z 宽由目标 bdy 自身决定。
            var itrVolumes = PS.GetItrVolumes(itrs, frame.centerx, frame.centery, spriteWidthPx, itrZWidthPx: 0f);

            for (int i = 0; i < Mathf.Min(itrs.Count, itrVolumes.Count); i++)
            {
                var itr = itrs[i];
                if (itr == null) continue;
                bool isAttackKind = kindService?.IsAttackKind(itr.kind) ?? false;
                if (!isAttackKind) continue;

                var candidates = sceneQuery.QueryBodies(itrVolumes[i], this);
                if (candidates == null || candidates.Count == 0) continue;

                for (int c = 0; c < candidates.Count; c++)
                {
                    var target = candidates[c];
                    if (!CanPostInteractTarget(itr, target)) continue;
                    if (target is not LF2LivingObject living) continue;

                    var attackerPos = new UnityEngine.Vector3(PS.x, PS.y, PS.z);
                    CurrentItrIndex = i;
                    bool hit = living.Hit(itr, this, attackerPos, itrVolumes[i]);
                    if (!hit) continue;

                    ItrArestUpdate(itr);

                    if (itr.arest > 0) return;
                    break;
                }
            }

            // kind=6：受伤硬直帧向外发出命中确认标记
            // C++ release 对齐 EXE 0x0042E6F4：[victim+0EAh] = 3
            // 自身 itr kind=6 碰到附近角色 body → 目标.HitConfirmEa = 3
            for (int i = 0; i < Mathf.Min(itrs.Count, itrVolumes.Count); i++)
            {
                var itr = itrs[i];
                if (itr == null || itr.kind != 6) continue;

                var candidates = sceneQuery.QueryBodies(itrVolumes[i], this);
                if (candidates == null || candidates.Count == 0) continue;

                for (int c = 0; c < candidates.Count; c++)
                {
                    var target = candidates[c];
                    if (target == null || target == this) continue;
                    if (target.PS == null) continue;
                    if (Team != 0 && target.Team == Team) continue;
                    if (!target.ItrVrestTest(StableId)) continue;
                    if (target is not LF2LivingObject living6) continue;

                    var attackerPos = new UnityEngine.Vector3(PS.x, PS.y, PS.z);
                    living6.Hit(itr, this, attackerPos, itrVolumes[i]);
                }
            }
        }

        private bool CanPostInteractTarget(InteractionArea itr, LF2Entity target)
        {
            if (target == null || target == this)
            {
                return false;
            }
            if (target.PS == null || target.Frame?.D == null)
            {
                return false;
            }
            if (target.Health != null && target.Health.HP <= 0)
            {
                return false;
            }
            if (!target.ItrVrestTest(StableId))
            {
                return false;
            }
            // C++ release 在攻击候选过滤阶段按 kind 组排除同队角色，不只看 effect 0/1。
            if (target is LF2Character targetCharacter && ShouldRejectSameTeamPostTarget(itr, targetCharacter.Team))
            {
                return false;
            }

            // effect 4：只命中非角色且 state==3000。
            if (itr.effect == 4)
            {
                if (target is LF2Character)
                {
                    return false;
                }
                if (target.GetState() != LF2States.ProjectileFlying)
                {
                    return false;
                }
            }

            // effect 20/21/22：只命中角色。
            if (itr.effect == 20 || itr.effect == 21 || itr.effect == 22)
            {
                if (target is not LF2Character)
                {
                    return false;
                }
            }

            // kind=4：不能命中自己的 attacker。
            if (itr.kind == 4 && Attacker == target)
            {
                return false;
            }

            return true;
        }

        private bool ShouldRejectSameTeamPostTarget(InteractionArea itr, int targetTeam)
        {
            if (itr == null || Team == 0 || targetTeam != Team)
            {
                return false;
            }

            bool sameTeamFilteredKind =
                itr.kind < 4 ||
                itr.kind == 6 ||
                itr.kind == 9 ||
                itr.kind == 10 ||
                itr.kind == 11 ||
                itr.kind == 15 ||
                itr.kind == 16;

            if (!sameTeamFilteredKind || itr.kind == 8)
            {
                return false;
            }

            int attackerState = Frame?.D?.state ?? 0;
            return !(attackerState == LF2States.Burning && itr.effect != 21 && itr.effect != 22);
        }

        private bool CanPreInteractTarget(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (Team != 0 && target.Team != 0 && Team == target.Team) return false;
            if (kindService == null) return false;

            return true;
        }

        private bool DispatchPreInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (kindService == null) return false;

            switch (itr.kind)
            {
                case 1:
                case 3:
                    return target is LF2LivingObject lo1 && HandlePreInteractionKind(itr, lo1);
                case 2:
                    return HandlePreInteractionKind2(itr, target);
                case 7:
                    return HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 处理抓取类型的预交互（itr kind 1/3）。
        /// </summary>
        private bool HandlePreInteractionKind(InteractionArea itr, LF2LivingObject target)
        {
            // 只处理角色类型
            if (target.Type != LF2ObjectType.Character)
                return false;

            // 检查抓取条件：(kind==1 && 目标处于 Injured2) || kind==3
            bool canCatch = (itr.kind == 1 && target.GetState() == LF2States.Injured2) || itr.kind == 3;
            if (!canCatch)
                return false;

            // 检查 itr arest（防止重复抓取）
            if (!ItrArestTest())
                return false;

            // 转换为 LF2Character 以调用 CaughtA
            var targetChar = target as LF2Character;
            if (targetChar == null)
                return false;

            // 调用被抓者的 CaughtA，获取抓取方向
            string dir = targetChar.CaughtA(itr, this, new Vector3(PS.x, PS.y, PS.z));
            if (dir == null)
                return false;

            // 抓取成功，更新 itr arest
            ItrArestUpdate(itr);

            // C++ release kind=1/3 抓取成功时无条件直接写双方 frame。
            int catchFrame = itr.effect != 0 ? itr.effect : LF2StandardFrames.Catching;
            ImmediateFrame(catchFrame);

            // 设置抓取目标
            Catching = target;

            // C++ release 对齐 0x0042D786/0x0042D796：抓取成功时抓取者 FrameDelay=3，被抓者 FrameDelay=-3
            FrameDelay = 3;
            targetChar.FrameDelay = -3;

            return true;
        }

        private bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            return PickupWeapon(itr, target, playAnimation: true);
        }

        // 武器拾取共享逻辑。
        // playAnimation：kind=2 时播放拾取帧，kind=7 时不播。
        private bool PickupWeapon(InteractionArea itr, LF2Entity target, bool playAnimation, bool skipGroundCheck = false)
        {
            if (_heldWeapon != null)
                return false;

            if (target.Type != LF2ObjectType.LightWeapon && target.Type != LF2ObjectType.HeavyWeapon && target.Type != LF2ObjectType.ThrowWeapon && target.Type != LF2ObjectType.Drink)
                return false;

            // kind=2 只允许拾取地面上的武器；kind=7 只检查 picker==0，不检查地面状态。
            if (!skipGroundCheck)
            {
                int wstate = target.GetState();
                bool isOnGround = wstate == LF2States.WeaponOnGround
                               || wstate == LF2States.WeaponJustOnGround
                               || wstate == LF2States.HeavyWeaponOnGround;
                if (!isOnGround)
                    return false;
            }

            var weapon = target as LF2WeaponBase;
            if (weapon == null || !weapon.Pick(this))
                return false;

            ItrArestUpdate(itr);

            if (playAnimation)
            {
                if (target.Type == LF2ObjectType.LightWeapon || target.Type == LF2ObjectType.ThrowWeapon || target.Type == LF2ObjectType.Drink)
                    ImmediateFrame(LF2StandardFrames.PickingLight);
                else if (target.Type == LF2ObjectType.HeavyWeapon)
                    ImmediateFrame(LF2StandardFrames.PickingHeavy);
            }

            HoldWeapon(weapon);
            return true;
        }

        private bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            // C++ release 0x0042E97B/0x0042E984：kind=7 近身拾取
            // 条件：target.picker==0（武器未被持有），无 att 键守卫，无重武器排除
            // 与 kind=2 相同逻辑，但不播放拾取动画帧
            return PickupWeapon(itr, target, playAnimation: false, skipGroundCheck: true);
        }

        /// <summary>
        /// 通用状态退出清理。
        /// </summary>
        private bool Generic_StateExit()
        {
            InputState?.OnStateExit();
            return false;
        }

        /// <summary>
        /// 被抓取处理。
        /// 由抓取者调用，在被抓目标身上执行。
        /// </summary>
        /// <param name="itr">抓取者的 itr 数据</param>
        /// <param name="attacker">抓取者</param>
        /// <param name="attackerPos">抓取者位置</param>
        /// <returns>"front"/"back" 表示抓取方向，null 表示抓取失败</returns>
        public string CaughtA(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos)
        {
            // 再次验证抓取条件。
            if (!((itr.kind == 1 && GetState() == LF2States.Injured2) || itr.kind == 3))
                return null;

            // 判断正面/背面。
            bool isFront = (attackerPos.x > PS.x) == (PS.dir == "right");
            CaughtFront = isFront;

            // C++ release kind=1/3 抓取成功时直接写 victim.frame = itr.catchingact。
            int caughtFrame = itr.catchingact != null && itr.catchingact.Length > 0
                ? itr.catchingact[0]
                : LF2StandardFrames.PickedCaught;
            ImmediateFrame(caughtFrame);

            // C++ release 里抓取会重置/改写部分受击计数；此处后续按正式流程校正。
            //if (Health != null) Health.Fall = 0;

            // 记录抓取者。
            Catching = attacker;

            // 被抓时丢弃当前武器。
            DropWeapon();

            return isFront ? "front" : "back";
        }

        /// <summary>
        /// PostInteraction 阶段（C++ release 对齐 GameMode_Process 碰撞双层循环）
        /// 在所有对象 SerialTickAll 完成后统一执行，处理 kind=0/4 普通攻击碰撞。
        /// </summary>
        public override void SimPostInteraction(int tickIndex)
        {
            Generic_PostInteraction();
        }

        #endregion
    }
}
