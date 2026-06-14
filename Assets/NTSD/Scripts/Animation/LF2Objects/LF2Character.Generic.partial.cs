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

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character : LF2LivingObject
    {
        #region Generic State Handlers

        /// <summary>
        /// 通用时间单元更新。
        /// 负责角色逐 tick 状态逻辑；复刻基准以 C++ release 的实体 tick 流程为准。
        /// </summary>
        private bool RunTUPhase()
        {
            // post_interaction 已移至 SimPostInteraction 阶段（对齐 C++ release GameMode_Process 碰撞循环）
            // 原位置：TU phase 开头；新位置：所有对象 SerialTickAll 完成后统一执行

            ApplyLegacySpecialStateVzInput();
            UpdateLegacyDeathBlinkLifecycle();
            RecoverLegacyHitCounters();

            return false;
        }

        private void ApplyLegacySpecialStateVzInput()
        {
            // C++ release Entity_InputProcess 0x41507E-0x415114:
            // state==301 (DeepSpecific) 或 state==19 (FirenSpecific) 时，根据左右输入更新 vz
            // 条件：(int)PS.z == 0（整数 z 坐标为 0，即在地面中心线）
            int curStateForVz = Frame.D?.state ?? -1;
            if (curStateForVz != LF2States.DeepSpecific && curStateForVz != LF2States.FirenSpecific)
                return;

            bool isLeft = Controller?.IsLeft ?? false;
            bool isRight = Controller?.IsRight ?? false;
            float dvz = Frame.D?.dvz ?? 0f;
            if (dvz == 0f || (int)PS.z != 0)
                return;

            if (isLeft && !isRight)
                PS.vz = -dvz;
            else if (isRight && !isLeft)
                PS.vz = dvz;
        }

        private void UpdateLegacyDeathBlinkLifecycle()
        {
            // 死亡闪烁流程：0 开始闪烁，1~29 维持，30 后隐藏并从模拟世界注销。
            if (_deadBlinkCount == 0)
            {
                Effect.Blink = true;
                _deadBlinkCount = 1;
                return;
            }

            if (_deadBlinkCount > 0 && _deadBlinkCount < 30)
            {
                _deadBlinkCount++;
                return;
            }

            if (_deadBlinkCount < 30)
                return;

            Effect.Blink = false;
            Sprite?.Hide();
            _deadBlinkCount = -1;
            Match?.Unregister(this);
        }

        private void RecoverLegacyHitCounters()
        {
            // fall/bdefend 每 tick 递减，字段通过 HitCounters 绑定到正式运行时。
            HitCounters.RecoverFall(NTSDGlobal.Gameplay.RecoverFall);
            HitCounters.RecoverBdefend(NTSDGlobal.Gameplay.RecoverBdefend);
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

        internal override bool SupportsPostInteractionPhase() => true;
        internal override bool IsStageBoundedCharacter() => true;
        internal override bool ShouldContributeToReleaseCamera() => Health != null && Health.HP > 0;
        internal override void ApplyPreFrameZBounds(float zMin, float zMax)
        {
            if (PS == null)
                return;

            if (PS.z < zMin) PS.z = zMin;
            if (PS.z > zMax) PS.z = zMax;
        }

        internal override bool ApplyPreFrameXBounds(float stageWidth)
        {
            if (PS == null)
                return false;

            int slotIndex = Runtime?.SlotIndex ?? StableId;
            if (slotIndex >= 20)
            {
                if (PS.x < -100f) PS.x = -100f;
                if (PS.x > stageWidth + 100f) PS.x = stageWidth + 100f;
            }
            else
            {
                if (PS.x < 0f) PS.x = 0f;
                if (PS.x > stageWidth) PS.x = stageWidth;
            }

            return false;
        }

        internal override void RunPreCollisionRecoveryPhase(int tickIndex)
        {
            RegeneratePreCollisionStats(tickIndex);
        }

        /// <summary>
        /// 通用物理转换。
        /// C++ release 的击退帧后处理已移到 SimulationWorld.FramePostProcessAll。
        /// </summary>
        private bool RunTransitPhase()
        {
            // C++ release 0x416254-0x41627C：FrameDelay 非零时跳过物理（hit_stop 冻结）
            // 当前正式战斗模拟由 SimTransit() 先衰减 FrameDelay，再进入这里。
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
        private bool RunFramePhase()
        {
            // 角色帧的 mp 资源消耗不在进入帧时统一处理。
            // C++ release 的输入/连招跳帧由 sub_414C30 使用目标帧 mp 检查并扣除 PP/HP；
            // Unity 对应入口是 TryInputFrameJump()。
            return false;
        }

        private static bool IsReleaseInvalidCandidateItrIndex(int itrIndex, LF2FrameData collisionFrame)
        {
            return collisionFrame?.itrs == null || itrIndex < 0 || itrIndex >= collisionFrame.itrs.Count;
        }

        /// <summary>
        /// C++ release step7/step9 的角色候选消费主路径。
        /// 按 step6 记录下来的 candidate 顺序逐条消费，并按 itr.kind 当场分派，
        /// 避免 Unity 侧再拆成先攻击、后抓取/拾取两段，造成同 tick 顺序偏差。
        /// </summary>
        private bool TryConsumeUnifiedStep7CandidateSequence()
        {
            LF2FrameData frame = GetCollisionFrameData();
            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (frame?.itrs == null || sceneQuery == null || kindService == null)
                return false;

            if (!sceneQuery.TryGetCollisionCandidateSequence(this, out var candidates) || candidates == null)
                return false;

            bool allowAttackKinds =
                ItrArestTest() &&
                (HitCounters?.AttackExempt ?? 0) <= 0 &&
                GetState() != LF2States.Falling;

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < candidateLimit; candidateIndex++)
            {
                int liveCandidateCount = Runtime?.HitCandidateCount ?? candidates.Count;
                if (liveCandidateCount < 0)
                    liveCandidateCount = 0;
                if (liveCandidateCount > candidates.Count)
                    liveCandidateCount = candidates.Count;
                if (candidateIndex >= liveCandidateCount)
                    break;

                SceneQueryHit hitInfo = candidates[candidateIndex];
                int itrIndex = hitInfo.ItrIndex;
                if (IsReleaseInvalidCandidateItrIndex(itrIndex, frame))
                    continue;

                bool zeroAttackerHpOnConsume;
                bool releaseHeavyHeldTargetOnConsume;
                InteractionArea itr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                    this,
                    hitInfo.Target,
                    frame,
                    frame.itrs[itrIndex],
                    out zeroAttackerHpOnConsume,
                    out releaseHeavyHeldTargetOnConsume);
                if (itr == null)
                    continue;

                hitInfo = new SceneQueryHit(
                    hitInfo.Target,
                    hitInfo.BodyX,
                    hitInfo.ItrIndex,
                    itr,
                    zeroAttackerHpOnConsume,
                    releaseHeavyHeldTargetOnConsume);

                LF2Entity target = hitInfo.Target;
                if (ShouldAbortReleaseConsume(itr, target))
                    return true;

                if (kindService.IsPreInteractionKind(itr.kind))
                {
                    if (!CanPreInteractTarget(kindService, itr, target))
                        continue;

                    DispatchPreInteractionByKind(kindService, itr, target);
                    continue;
                }

                if (itr.kind == 6)
                {
                    if (target == null || target == this)
                        continue;
                    if (target.PS == null)
                        continue;
                    if (RelationTeam != 0 && target.RelationTeam == RelationTeam)
                        continue;
                    int selfSlot = Runtime?.SlotIndex ?? -1;
                    if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true))
                        continue;
                    if (target is not LF2LivingObject living6)
                        continue;

                    var attackerPos6 = new Vector3(PS.x, PS.y, PS.z);
                    living6.Hit(itr, this, attackerPos6, default);
                    continue;
                }

                if (!kindService.IsAttackKind(itr.kind))
                    continue;
                if (!allowAttackKinds)
                    continue;
                if (!CanPostInteractTarget(itr, target, hitInfo.BodyX))
                    continue;
                if (target is not LF2LivingObject living)
                    continue;
                ApplyReleaseSceneQueryConsumeEffects(hitInfo);
                var attackerPos = new Vector3(PS.x, PS.y, PS.z);
                CurrentItrIndex = itrIndex;
                bool hit = living.Hit(itr, this, attackerPos, default);
                if (!hit)
                    continue;
                if (ShouldAbortAfterSuccessfulReleaseHit(itr, target))
                    return true;

                ItrArestUpdate(itr);
                if ((Runtime?.HitCandidateCount ?? 0) <= 0)
                    return true;
                if (ItrRest != null && ItrRest.Arest > 0)
                    return true;
            }

            return true;
        }

        private bool ShouldAbortReleaseConsume(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null)
                return false;

            // C++ release collision.cpp:
            // 1. attacker.hit_confirm2!=0 且当前 pair 的 victim 是 character -> next_attacker
            // 2. kind0/effect21 且 victim 当前 frame.state==18/19 -> next_attacker
            if (HitConfirm2 != 0 && GetDataObjectTypeForReleaseConsume(target) == (int)LF2ObjectType.Character)
                return true;

            if (itr.kind == 0 &&
                itr.effect == 21 &&
                (target.GetState() == LF2States.Burning || target.GetState() == LF2States.FirenSpecific))
            {
                return true;
            }

            return false;
        }

        private static int GetDataObjectTypeForReleaseConsume(LF2Entity entity)
        {
            if (entity == null)
                return -1;

            int wrapperOid = entity.FrameCache?.Wrapper?.characterId ?? entity.ObjectId;
            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(wrapperOid);
            return definition?.type ?? entity.ReleaseEntityType;
        }

        private static bool ShouldAbortAfterSuccessfulReleaseHit(InteractionArea itr, LF2Entity target)
        {
            return itr != null &&
                   target != null &&
                   itr.kind == 0 &&
                   target.ObjectId == 300;
        }

        private bool CanPostInteractTarget(InteractionArea itr, LF2Entity target, int hitBodyX = 0)
        {
            if (itr == null)
            {
                return false;
            }
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
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr))
            {
                return false;
            }
            // C++ release collision.cpp 在 consume 路径仍会做一次 pair blocked 检查。
            // step6 先收候选、step7/step9 再消费；期间抓取关系可能已经被前面的 candidate 改写，
            // 所以这里不能只依赖 collect 时的快照过滤。
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(this, target))
            {
                return false;
            }
            if (!BruteForceSceneQuery.RuntimeConsumeItrAllowed(this, itr, target))
            {
                return false;
            }
            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true))
            {
                return false;
            }

            return true;
        }

        private bool CanPreInteractTarget(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr)) return false;
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(this, target)) return false;
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
            if (GetDataObjectTypeForReleaseConsume(target) != (int)LF2ObjectType.Character)
                return false;

            // 转换为 LF2Character 以调用 CaughtA
            var targetChar = target as LF2Character;
            if (targetChar == null)
                return false;

            // 调用被抓者的 CaughtA，获取抓取方向
            string dir = targetChar.CaughtA(itr, this, new Vector3(PS.x, PS.y, PS.z));
            if (dir == null)
                return false;

            // C++ release kind=1/3：抓取者写 itr.catchingact[0]，不是 effect。
            int catchFrame = itr.catchingact != null && itr.catchingact.Length > 0
                ? itr.catchingact[0]
                : LF2StandardFrames.Catching;
            ImmediateFrame(catchFrame);

            // 设置抓取目标
            Catching = target;
            CaughtSlotIndex = target.Runtime?.SlotIndex ?? -1;
            target.CatcherSlotIndex = Runtime?.SlotIndex ?? -1;

            ApplyImmediateCatchPairState(targetChar);

            return true;
        }

        /// <summary>
        /// C++ release kind=1/3 在 step7 成功后会立刻同步一组抓取状态：
        /// 双方 vx 清零、按 x 关系改朝向、写 caught_duration、victim fall=0，并立刻对位。
        /// 这些字段如果等到 step10 才补，会导致抓取建立后的当帧表现偏掉。
        /// </summary>
        private void ApplyImmediateCatchPairState(LF2Character targetChar)
        {
            if (targetChar == null || PS == null || targetChar.PS == null)
                return;

            PS.vx = 0f;
            targetChar.PS.vx = 0f;
            KnockbackVx = 0f;
            targetChar.KnockbackVx = 0f;

            bool attackerFacesLeft = Mathf.RoundToInt(PS.x) > Mathf.RoundToInt(targetChar.PS.x);
            SwitchDir(attackerFacesLeft ? "left" : "right");
            targetChar.SwitchDir(attackerFacesLeft ? "right" : "left");

            CaughtDuration = 300;
            targetChar.FallCounter = 0;
            if (targetChar.HitCounters != null)
                targetChar.HitCounters.ResetFall();

            LF2FrameData attackerFrame = Frame?.D;
            LF2FrameData victimFrame = targetChar.Frame?.D;
            if (attackerFrame == null || victimFrame == null)
                return;

            int attackerWact = attackerFrame.cpoint?.x ?? 0;
            int victimWact = victimFrame.cpoint?.x ?? 0;
            int attackerCx = attackerFrame.centerx;
            int attackerCy = attackerFrame.centery;
            int victimCx = victimFrame.centerx;
            int victimCy = victimFrame.centery;

            int attackerXInt = Mathf.RoundToInt(PS.x);
            int attackerYInt = Mathf.RoundToInt(PS.y);
            int victimXInt = Mathf.RoundToInt(targetChar.PS.x);

            float victimNewX;
            if (PS.dir == "right")
                victimNewX = attackerXInt - attackerCx - victimCx + attackerWact + victimWact;
            else
                victimNewX = attackerCx + victimCx + attackerXInt - attackerWact - victimWact;

            float victimNewY = victimCy - attackerCy + attackerYInt;
            targetChar.PS.x = victimNewX;
            targetChar.PS.y = victimNewY;

            float lerp = (victimXInt - victimNewX) * 0.5f;
            targetChar.PS.x += lerp;
            PS.x += lerp;
        }

        private bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            return PickupWeapon(itr, target, playAnimation: true);
        }

        // 武器拾取共享逻辑。
        // playAnimation：kind=2 时播放拾取帧，kind=7 时不播。
        private bool PickupWeapon(InteractionArea itr, LF2Entity target, bool playAnimation, bool skipGroundCheck = false)
        {
            if (HasHeldObject())
                return false;

            int targetType = GetDataObjectTypeForReleaseConsume(target);
            if (targetType != (int)LF2ObjectType.LightWeapon &&
                targetType != (int)LF2ObjectType.HeavyWeapon &&
                targetType != (int)LF2ObjectType.ThrowWeapon &&
                targetType != (int)LF2ObjectType.Drink)
                return false;

            // kind=2 只允许拾取地面上的武器；kind=7 只检查 picker==0，不检查地面状态。
            if (!skipGroundCheck)
            {
                int wstate = target is LF2WeaponBase targetWeapon
                    ? targetWeapon.GetResolvedWeaponStateForExternalUse()
                    : target.GetState();
                bool isOnGround = wstate == LF2States.WeaponOnGround
                               || wstate == LF2States.WeaponJustOnGround
                               || wstate == LF2States.HeavyWeaponOnGround;
                if (!isOnGround)
                    return false;
            }

            LF2WeaponBase weapon = AsWeaponEntity(target);
            if (weapon == null || !weapon.Pick(this))
                return false;

            if (playAnimation)
            {
                if (targetType == (int)LF2ObjectType.LightWeapon ||
                    targetType == (int)LF2ObjectType.ThrowWeapon ||
                    targetType == (int)LF2ObjectType.Drink)
                    ImmediateFrame(LF2StandardFrames.PickingLight);
                else if (targetType == (int)LF2ObjectType.HeavyWeapon)
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
        private bool RunStateExitPhase()
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

            // C++ release kind=1/3：被抓者写 itr.caughtact[0]，不是 catchingact。
            int caughtFrame = itr.caughtact != null && itr.caughtact.Length > 0
                ? itr.caughtact[0]
                : LF2StandardFrames.PickedCaught;
            ImmediateFrame(caughtFrame);

            // C++ release 里抓取会重置/改写部分受击计数；此处后续按正式流程校正。
            //if (Health != null) Health.Fall = 0;

            // 记录抓取者。
            Catching = attacker;
            CatcherSlotIndex = attacker?.Runtime?.SlotIndex ?? -1;
            if (attacker != null)
                attacker.CaughtSlotIndex = Runtime?.SlotIndex ?? -1;

            // 被抓时丢弃当前武器。
            DropWeapon();

            return isFront ? "front" : "back";
        }

        /// <summary>
        /// C++ release step7 collision_check_loop1：角色作为攻击方时处理攻击、抓取和拾取类 itr。
        /// 当前统一按 candidate 快照顺序消费，不再拆回旧的分组式查询路径。
        /// </summary>
        public override void SimPostInteraction(int tickIndex)
        {
            TryConsumeUnifiedStep7CandidateSequence();
        }

        #endregion
    }
}
