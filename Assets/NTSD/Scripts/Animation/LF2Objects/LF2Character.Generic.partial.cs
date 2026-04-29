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

        /// 通用时间单元更新 (TU)
        /// 对应 FLF character.js:54-183
        /// 负责处理周期性的逻辑，如 Buff 消失、状态恢复、物理 Tick
        /// </summary>
        private bool Generic_TU()
        {
            int tickIndex = SimulationTickDriver.Instance != null
                ? SimulationTickDriver.Instance.CurrentTickIndex
                : 0;

            // post_interaction 已移至 SimPostInteraction 阶段（对齐反汇编 GameMode_Process 碰撞循环）
            // 原位置：Generic_TU 开头；新位置：所有对象 SerialTickAll 完成后统一执行

            if (PS.y == 0 && PS.vy == 0 && Frame.N == LF2StandardFrames.JumpingAir && Frame.PN != LF2StandardFrames.JumpingUp)
            {
                TransitionToFrame(999);
            }
            // A) fell_onto_ground（PS.y==0 && PS.vy>0）- 对齐 FLF js:115-126
            else if (PS.y == 0 && PS.vy > 0)
            {
                var res = StateUpdate("fell_onto_ground", out int frameId);
                if (res && frameId > 0)
                {
                    TransitionToFrame(frameId, 15);
                }
                else if (!res)
                {
                    // 默认分支：PS.vy=0 + 落地瞬间摩擦 + 着地帧跳转（对齐 Case B 逻辑）
                    PS.vy = 0;
                    float fricX = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, PS.vx);
                    float fricZ = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, PS.vz);
                    CharacterMechanics.LinearFriction(PS, fricX, fricZ);
                    //if (Frame.D.state != LF2States.Frozen)
                    //{
                    //    if (Frame.N == LF2StandardFrames.JumpingAir)
                    //        TransitionToFrame(LF2StandardFrames.Crouch, 15);
                    //    else
                    //        TransitionToFrame(LF2StandardFrames.Crouch2, 15);
                    //}
                }
            }
            // B) fall_onto_ground（PS.y+PS.vy>=0 && PS.vy>0）- 对齐 FLF js:127-141
            else if ((PS.y + PS.vy) >= 0 && PS.vy > 0)
            {
                var res = StateUpdate("fall_onto_ground", out int frameId);
                if (res && frameId > 0)
                {
                    TransitionToFrame(frameId, 15);
                }
                else if (!res)
                {
                    // 默认分支：Frozen 不动；JumpingAir→Crouch；其它→Crouch2
                    if (Frame.D.state == LF2States.Frozen)
                    {
                        // 冰冻状态不处理
                    }
                    else if (Frame.N == LF2StandardFrames.JumpingAir)
                    {
                        TransitionToFrame(LF2StandardFrames.Crouch, 15);
                    }
                    else
                    {
                        TransitionToFrame(LF2StandardFrames.Crouch2, 15);
                    }
                }
            }

            // kind=8 分帧回血（反汇编验证存在：heal_timer = throwvz + 1000，每8帧回8HP）
            if (Health.HP >= 0 && Effect.Heal is int healAmount && healAmount > 0 && tickIndex % 8 == 0)
            {
                const int healSpeed = 8;
                int appliedHeal = Mathf.Min(healSpeed, healAmount);
                Health.HP = Mathf.Min(Health.HP + appliedHeal, Health.HPBound);
                Effect.Heal = healAmount - appliedHeal;
                if (CharacterStats != null)
                    CharacterStats.CurrentHP = Health.HP;
            }
            // 注：反汇编验证无 HP/MP 自然恢复，已移除 tickIndex%12 HP++ 和 tickIndex%3 MP++ 逻辑

            ComboBuffer?.ReduceTimeout();

            // FLF character.js:104-122
            // switch(true) {
            //   case dead_blink_count < 0: break  (不执行)
            //   case dead_blink_count == 0: effect.blink=true; count++
            //   case count>0 && count<30: count++
            //   case count>=30: effect.blink=false; sp.hide(); shadow.hide(); count=-1; match.destroy_object($)
            // }
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

            // FLF character.js:174-175  fall/bdefend 自然恢复（每 TU）
            HitCounters.RecoverFall(NTSDGlobal.Gameplay.RecoverFall);
            HitCounters.RecoverBdefend(NTSDGlobal.Gameplay.RecoverBdefend);

            return false;
        }

        /// <summary>
        /// 通用物理转换 (Transit)
        /// 对应 FLF character.js:185-190
        /// </summary>
        private bool Generic_Transit()
        {
            // 反汇编 0x416254-0x41627C：FrameDelay 非零时跳过物理（hit_stop 冻结）
            // FrameDelay 衰减已在 base.Transit() 中完成，此处为衰减后的值
            if (FrameDelay != 0) return false;

            // Frame_PostProcess（反汇编 0x0041BF00）的 Knockback→vx/vy/vz 逻辑
            // 不在此处执行——反汇编中该函数在所有 entity SerialTick 完成后才调用，
            // 对应 SimulationTickDriver 中 SerialTickAll 之后的独立 pass。
            // 见 SimulationWorld.FramePostProcessAll()

            // kind=14 方向阻挡（反汇编 entity+3F0h/3F4h/3E8h/3ECh 边界标志）
            // xBound/zBound 由 Hit() kind=14 分支每帧写入，此处在位移前强制阻止对应方向移动
            if (PS.xBoundPositive && PS.vx > 0f) PS.vx = 0f;
            if (PS.xBoundNegative && PS.vx < 0f) PS.vx = 0f;
            if (PS.zBoundPositive && PS.vz > 0f) PS.vz = 0f;
            if (PS.zBoundNegative && PS.vz < 0f) PS.vz = 0f;
            // 边界标志由 CharacterMechanics.WeaponDynamics 清零；
            // 字符物理走 Step() 路径，这里手动清零
            PS.xBoundPositive = PS.xBoundNegative = false;
            PS.zBoundPositive = PS.zBoundNegative = false;

            // dynamics: position, friction, gravity
            ApplyDynamics();
            WPointUpdate();
            return false;
        }

        /// <summary>
        /// 通用帧逻辑 (Frame)
        /// 对应 FLF character.js:14-52
        /// </summary>
        private bool Generic_Frame()
        {
            if (Frame.D.mp != 0)
            {
                // 对齐 NTSD 2.4 反汇编 sub_414C30 @ 0x00414C85
                // dmp = mp % 1000, dhp = (mp / 1000) * 10（整数截断除法）
                int mp  = Frame.D.mp;
                int dmp = mp % 1000;
                int dhp = (mp / 1000) * 10;

                // 0x00414C99: jl loc_414D6D — MP 不足则跳过整段
                if (Health.MP >= dmp)
                {
                    // 0x00414CC3: jle loc_414D6D — HP 不足则跳过整段
                    if (Health.HP > dhp)
                    {
                        Health.HP -= dhp;
                        Health.MP -= dmp;

                        if (CharacterStats != null)
                        {
                            CharacterStats.CurrentHP = Health.HP;
                            CharacterStats.CurrentMP = Health.MP;
                        }
                    }
                }
            }

            ObjectPointModule?.ProcessFrame(this);
            return false;
        }

        /// <summary>
        /// 通用连招处理器 (Generic Combo)
        /// 对应 FLF character.js line 191-215 的 generic case 'combo'
        /// 
        /// <para>工作流程：</para>
        /// <list type="number">
        /// <item>1. 处理单键输入 (硬编码)：如 left, right, jump 等基础移动逻辑。</item>
        /// <item>2. 处理多键连招：通过 Tag 映射 (如 D>A 映射到 Tag "Fa")。</item>
        /// <item>3. 调用 id_update：允许角色脚本覆盖通用逻辑 (id_update('generic_combo'))。</item>
        /// <item>4. 处理方向切换：如输入 D>A 强制角色转向右侧。</item>
        /// <item>5. 执行跳转：根据 Frame Data 中的 Tag 跳转到目标帧。</item>
        /// </list>
        /// </summary>
        private bool Generic_Combo(string combo)
        {
            if (string.IsNullOrEmpty(combo))
                return false;

            // === 1. 处理单键连招 (硬编码逻辑) ===
            // 对应 FLF character.js:239-338 State 0 的 case 'combo' 部分逻辑
            switch (combo)
            {
                case "left":
                case "right":
                case "left-left":
                case "right-right":
                    // 这些基础移动指令通常由 Standing/Walking 状态自行处理，通用逻辑直接返回
                    return false;

                default:
                    // 对应 FLF character.js:226-228: DJA + transform_character.is_rudolf_transform → revert_transform
                    if (combo == "DJA" && _idUpdate != null)
                    {
                        var ctx = new IdUpdateContext(this, PS, combo, null, 0, 0);
                        if (_idUpdate.TryInvoke(IdUpdateHooks.RevertTransform, in ctx))
                            return true;
                    }
                    break;
            }

            // === 2. 处理多键连招 (Tag 映射机制) ===
            // 对应 FLF character.js:191-215

            // Step 1: 将输入序列 (如 "D>A") 映射为内部 Tag (如 "Fa")
            string tag = ComboConfig.GetComboTag(combo);
            if (string.IsNullOrEmpty(tag))
                return false;

            // Step 2: 检查当前帧的数据中是否定义了该 Tag 的跳转目标 (hit_Fa: 123)
            int targetFrame = Frame.D.Hit[tag];
            Log.LogState(Name, "Combo", $"combo='{combo}' → tag='{tag}' → Hit['{tag}']={targetFrame}");

            if (targetFrame <= 0)   // 0 是缺省值（未定义），与 FLF JS 的 falsy 判断对齐
            {
                Log.LogState(Name, "Combo", $"BLOCKED: Hit['{tag}']={targetFrame} ≤ 0", Log.StateLogLevel.Warn);
                return false;
            }

            // Step 3: 调用角色特定逻辑 id_update('generic_combo', K, tag)
            // 对应 FLF character.js:233: if (!$.id_update('generic_combo', K, tag))
            if (_idUpdate != null)
            {
                if (_idUpdate.TryInvokeGenericCombo(combo, tag, targetFrame))
                    return true;  // 角色特定逻辑已处理，阻止默认跳转
            }

            // 如果不是通用连招
            // 获取连招方向
            // Step 4: 处理连招的方向要求 (如 D>A 要求必须朝右)
            string dir = ComboConfig.GetComboDirection(combo);
            if (!string.IsNullOrEmpty(dir))
            {
                // 切换方向
                SwitchDir(dir);
            }

            // 执行连招动画
            // 返回成功状态
            Log.LogState(Name, "Combo", $"→ TransitionToFrame({targetFrame})");
            TransitionToFrame(targetFrame, LF2StateConstants.GenericComboWait);
            StateReturnFrame = 1;
            return true;
        }

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
        /// 角色拳脚攻击命中判定
        /// 对应 FLF character.js:2291-2360 character.prototype.post_interaction
        /// 处理 itr kind=0（普通攻击）和 kind=4（倒地攻击）
        /// </summary>
        private void Generic_PostInteraction()
        {
            var frame = Frame?.D;
            var sceneQuery = Match?.SceneQuery;
            if (frame == null || sceneQuery == null) return;
            if (PS == null) return;

            var itrs = frame.itrs;
            if (itrs == null || itrs.Count == 0) return;

            if (!ItrArestTest()) return;

            // 攻击方碰撞豁免守卫（对应反汇编 0x419E3B：[esi+0ECh] > 0 跳过整体碰撞检测）
            if (HitCounters?.AttackExempt > 0) return;

            // Falling 状态下不执行 kind=0 攻击判定
            // 反汇编中 Falling 帧（180-183）实际无 itr，等价于此过滤
            if (GetState() == LF2States.Falling) return;

            float spriteWidthPx = GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return;

            // FLF: vol.zwidth = 0（由目标 bdy 自身的 zwidth 决定范围）
            var itrVolumes = PS.GetItrVolumes(itrs, frame.centerx, frame.centery, spriteWidthPx, itrZWidthPx: 0f);

            for (int i = 0; i < Mathf.Min(itrs.Count, itrVolumes.Count); i++)
            {
                var itr = itrs[i];
                if (itr == null) continue;
                if (itr.kind != 0 && itr.kind != 4) continue;

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
                    StateUpdate("hit_stop", out _);

                    if (itr.arest > 0) return;
                    break;
                }
            }

            // kind=6：受伤硬直帧向外发出命中确认标记
            // 对应反汇编 EXE 0x0042E6F4：[victim+0EAh] = 3
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
            if (target == null || target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (!target.ItrVrestTest(StableId)) return false;
            // effect 0/1：同队角色不可命中（FLF:2302-2306）
            if ((itr.effect == 0 || itr.effect == 1) &&
                target is LF2Character && Team != 0 && target.Team == Team)
                return false;

            // effect 4：只命中非角色且 state==3000（FLF:2307-2310）
            if (itr.effect == 4)
            {
                if (target is LF2Character) return false;
                if (target.GetState() != LF2States.ProjectileFlying) return false;
            }

            // effect 20/21/22：只命中角色（FLF:2311-2320）
            if (itr.effect == 20 || itr.effect == 21 || itr.effect == 22)
            {
                if (target is not LF2Character) return false;
            }

            // kind=4：不能命中自己的 attacker（FLF:2322-2330）
            if (itr.kind == 4 && Attacker == target) return false;

            return true;
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
        /// 处理抓取类型的预交互（itr kind 1/3）
        /// 对应 FLF character.js:2216-2246 pre_interaction
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

            // 对应 FLF character.js:2234-2237: 根据 dir 选择 catchingact[0](正面) 或 catchingact[1](背面)
            int catchFrame;
            if (itr.catchingact != null && itr.catchingact.Length >= 2)
                catchFrame = (dir == "front") ? itr.catchingact[0] : itr.catchingact[1];
            else
                catchFrame = LF2StandardFrames.Catching;
            TransitionToFrame(catchFrame, 10);

            // 设置抓取目标
            Catching = target;

            // 对应反汇编 0x0042D786/0x0042D796：抓取成功时抓取者 FrameDelay=3，被抓者 FrameDelay=-3
            FrameDelay = 3;
            targetChar.FrameDelay = -3;

            return true;
        }

        private bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            return PickupWeapon(itr, target, playAnimation: true);
        }

        // 对应 FLF character.js:2250-2270 武器拾取共享逻辑
        // playAnimation: kind=2 时播放拾取帧; kind=7 时不播
        private bool PickupWeapon(InteractionArea itr, LF2Entity target, bool playAnimation)
        {
            if (_heldWeapon != null)
                return false;

            if (target.Type != LF2ObjectType.LightWeapon && target.Type != LF2ObjectType.HeavyWeapon && target.Type != LF2ObjectType.ThrowWeapon && target.Type != LF2ObjectType.Drink)
                return false;

            // 只允许拾取地面上的武器（FLF: light=1004/1003, heavy=2004）
            int wstate = target.GetState();
            bool isOnGround = wstate == LF2States.WeaponOnGround
                           || wstate == LF2States.WeaponJustOnGround
                           || wstate == LF2States.HeavyWeaponOnGround;
            if (!isOnGround)
                return false;

            var weapon = target as LF2WeaponBase;
            if (weapon == null || !weapon.Pick(this))
                return false;

            ItrArestUpdate(itr);

            if (playAnimation)
            {
                if (target.Type == LF2ObjectType.LightWeapon || target.Type == LF2ObjectType.ThrowWeapon || target.Type == LF2ObjectType.Drink)
                    TransitionToFrame(LF2StandardFrames.PickingLight, 10);
                else if (target.Type == LF2ObjectType.HeavyWeapon)
                    TransitionToFrame(LF2StandardFrames.PickingHeavy, 10);
            }

            HoldWeapon(weapon);
            return true;
        }

        private bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            // 对应 FLF character.js:2247-2249: kind=7 需要 att 键按下才触发，否则跳出
            // FLF: if (!$.con.state.att) { break }
            if (Controller == null || !Controller.IsAttack)
                return false;

            // att 已按下：fall-through 到 kind=2 武器拾取逻辑
            // 注意：kind=7 不切换拾取动画帧（FLF 原文 kind==2 才切帧）
            // kind=7 也不允许拾取重型武器（FLF: if (!(ITR.kind===7 && hit[t].type==='heavyweapon'))）
            if (target.Type == LF2ObjectType.HeavyWeapon)
                return false;

            return PickupWeapon(itr, target, playAnimation: false);
        }

        /// <summary>
        /// 通用状态退出清理
        /// 对应 FLF character.js:221-228
        /// </summary>
        private bool Generic_StateExit()
        {
            // 清除双击指令缓存 (防止状态切换后误触发跑动)
            // 对应 FLF:222-227
            switch (ComboBuffer?.Combo)
            {
                case "left-left":
                case "right-right":
                    ComboBuffer?.OnClearCombo();
                    break;
            }
            return false;
        }

        /// <summary>
        /// 被抓取处理（对应 FLF character.prototype.caught_a）
        /// 由抓取者调用，在被抓目标身上执行。
        /// </summary>
        /// <param name="itr">抓取者的 itr 数据</param>
        /// <param name="attacker">抓取者</param>
        /// <param name="attackerPos">抓取者位置</param>
        /// <returns>"front"/"back" 表示抓取方向，null 表示抓取失败</returns>
        public string CaughtA(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos)
        {
            // FLF:2457 - 再次验证抓取条件
            if (!((itr.kind == 1 && GetState() == LF2States.Injured2) || itr.kind == 3))
                return null;

            // FLF:2459 - 判断正面/背面
            bool isFront = (attackerPos.x > PS.x) == (PS.dir == "right");

            // 原版 LF2：被抓者固定切换到帧 130（PickedCaught）
            // 正面/背面的区分由 cpoint 的 fronthurtact/backhurtact 控制
            TransitionToFrame(LF2StandardFrames.PickedCaught, 22);

            // FLF:2464 - 重置倒地值
            //if (Health != null) Health.Fall = 0;

            // FLF:2465-2466 - 记录抓取者
            Catching = attacker;

            // FLF:2467 - 丢弃武器
            DropWeapon();

            return isFront ? "front" : "back";
        }

        /// <summary>
        /// PostInteraction 阶段（对应反汇编 GameMode_Process 碰撞双层循环）
        /// 在所有对象 SerialTickAll 完成后统一执行，处理 kind=0/4 普通攻击碰撞。
        /// </summary>
        public override void SimPostInteraction(int tickIndex)
        {
            if (!StateUpdate("post_interaction", out _))
                Generic_PostInteraction();
        }

        #endregion
    }
}
