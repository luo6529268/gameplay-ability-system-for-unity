using BeatEmUpTemplate2D;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
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
                else
                {
                    // 默认分支：PS.vy=0 + 落地瞬间摩擦
                    PS.vy = 0;
                    float fricX = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, PS.vx);
                    float fricZ = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FrictionFell, PS.vz);
                    CharacterMechanics.LinearFriction(PS, fricX, fricZ);
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
                else
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

            if (tickIndex % 12 == 0 && Health.HP >= 0 && Health.HP < Health.HPBound)
            {
                Health.HP++;
                if (CharacterStats != null)
                {
                    CharacterStats.CurrentHP = Health.HP;
                }
            }

            if (Health.HP >= 0 && Effect.Heal is int healAmount && healAmount > 0 && tickIndex % 8 == 0)
            {
                const int healSpeed = 8;
                int appliedHeal = Mathf.Min(healSpeed, healAmount);
                Health.HP = Mathf.Min(Health.HP + appliedHeal, Health.HPBound);
                Effect.Heal = healAmount - appliedHeal;
                if (CharacterStats != null)
                {
                    CharacterStats.CurrentHP = Health.HP;
                }
            }

            int maxMp = CharacterStats != null && CharacterStats.MaxMP > 0
                ? CharacterStats.MaxMP
                : NTSDGlobal.Default.Health.MpFull;
            if (tickIndex % 3 == 0 && Health.MP < maxMp)
            {
                int clampedHp = CharacterStats != null && CharacterStats.MaxHP > 0
                    ? Mathf.Min(Health.HP, CharacterStats.MaxHP)
                    : Mathf.Min(Health.HP, NTSDGlobal.Default.Health.HpFull);
                int hpFull = CharacterStats != null && CharacterStats.MaxHP > 0
                    ? CharacterStats.MaxHP
                    : NTSDGlobal.Default.Health.HpFull;
                int mpRecover = 1 + Mathf.FloorToInt((hpFull - clampedHp) / 100f);
                Health.MP = Mathf.Min(Health.MP + mpRecover, maxMp);
                if (CharacterStats != null)
                {
                    CharacterStats.CurrentMP = Health.MP;
                }
            }

            ComboBuffer?.ReduceTimeout();

            return false;
        }

        /// <summary>
        /// 通用物理转换 (Transit)
        /// 对应 FLF character.js:185-190
        /// </summary>
        private bool Generic_Transit()
        {
            // dynamics: position, friction, gravity
            // 更新动态物理效果（位置、摩擦力、重力）
            // 任何位置变更将在下一个时间单位(TU)更新到屏幕
            // 更新武器位置，使其跟随角色移动
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
                LF2FrameData previousFrame = FrameCache.GetFrameDataById(Frame.PN);
                bool isNextTriggered = previousFrame != null && previousFrame.next == Frame.N;

                if (isNextTriggered)
                {
                    if (Frame.D.mp < 0)
                    {
                        Health.MP += Frame.D.mp;
                        if (CharacterStats != null)
                        {
                            CharacterStats.CurrentMP = Health.MP;
                        }

                        if (Health.MP < 0)
                        {
                            Health.MP = 0;
                            if (CharacterStats != null)
                            {
                                CharacterStats.CurrentMP = 0;
                            }

                            if (Frame.D.hit_d > 0)
                            {
                                TransitionToFrame(Frame.D.hit_d);
                            }
                        }
                    }
                }
                else
                {
                    int dmp = Frame.D.mp % 1000;
                    int dhp = Mathf.FloorToInt(Frame.D.mp / 1000f) * 10;

                    Health.MP -= dmp;
                    if (Health.MP < 0)
                    {
                        Health.MP = 0;
                    }

                    if (CharacterStats != null)
                    {
                        CharacterStats.CurrentMP = Health.MP;
                    }

                    Injury(dhp);
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
                    // 特殊处理: Rudolf 的 DJA 变身
                    if (combo == "DJA")
                    {
                        // TODO: Rudolf 变身检查逻辑
                        // if (character.transform_character != null && character.transform_character.is_rudolf_transform) { ... }
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

            // 检查连招是否有效
            // Step 3: 尝试调用角色特定逻辑 (id_update) 进行拦截
            // 对应 FLF: if (!$.id_update('generic_combo', K, tag))
            //if (character._Character != null && character._Character._IdUpdate != null)
            //{
            //    if (character._Character._IdUpdate.TryInvokeGenericCombo(combo, tag, targetFrame))
            //    {
            //        return true;  // 角色特定逻辑已处理，不再执行默认跳转
            //    }
            //}

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

        private bool CanPreInteractTarget(INTSDItrKindService kindService, InteractionArea itr, LF2LivingObject target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.PS == null || target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (Team != 0 && target.Team != 0 && Team == target.Team) return false;
            if (kindService == null) return false;

            return true;
        }

        private bool DispatchPreInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2LivingObject target)
        {
            if (kindService == null) return false;

            switch (itr.kind)
            {
                case 1:
                case 3:
                    return HandlePreInteractionKind(itr, target);
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

            // 原版 LF2：抓取者固定切换到帧 120（正面/背面相同）
            TransitionToFrame(LF2StandardFrames.Catching, 10);

            // 设置抓取目标
            Catching = target;

            return true;
        }

        private bool HandlePreInteractionKind2(InteractionArea itr, LF2LivingObject target)
        {
            if (_heldWeapon != null)
                return false;

            if (target.Type != LF2ObjectType.LightWeapon && target.Type != LF2ObjectType.HeavyWeapon)
                return false;

            var weapon = target as LF2WeaponBase;
            if (weapon == null || !weapon.Pick(this))
                return false;

            ItrArestUpdate(itr);

            if (target.Type == LF2ObjectType.LightWeapon)
                TransitionToFrame(LF2StandardFrames.PickingLight, 10);
            else if (target.Type == LF2ObjectType.HeavyWeapon)
                TransitionToFrame(LF2StandardFrames.PickingHeavy, 10);

            HoldWeapon(weapon);
            return true;
        }

        private bool HandlePreInteractionKind7(InteractionArea itr, LF2LivingObject target)
        {
            // 检查是否处于攻击状态
            if (Controller == null || !Controller.IsAttack)
            {
                return false;
            }
            return false;
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

        #endregion
    }
}
