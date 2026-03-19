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
        #region Specific State Handlers

        public override bool GetStatesSwitchDir(int stateId)
        {
            switch (stateId)
            {
                case LF2States.Standing:       // 站立：允许转身
                case LF2States.Walking:        // 行走：允许转身
                case LF2States.Jump:         // 跳跃：允许空中转身 (LF2机制)
                case LF2States.Defending:     // 防御：允许转身
                    return true;

                case LF2States.Attack: // 攻击：锁定方向
                case LF2States.Running: // 奔跑：锁定方向
                case LF2States.Dash:// 冲刺：锁定方向
                case LF2States.Rowing: // 划船：锁定方向
                case LF2States.BrokenDefend:  // 防破：锁定方向
                case LF2States.Catching:  // 抓人：锁定方向
                case LF2States.BeingCaught:  // 被抓：锁定方向
                case LF2States.Injured: // 受伤：锁定方向
                case LF2States.Falling: // 跌倒：锁定方向
                case LF2States.Frozen:// 冰冻：锁定方向
                case LF2States.Lying: // 倒地：锁定方向
                case LF2States.StopRunning: // 停跑：锁定方向
                case LF2States.Injured2: // 受伤2：锁定方向
                    break;

                default:
                    break;
            }

            return false;
        }


        /// <summary>
        /// 站立状态处理器 (State 0)
        /// 对应 FLF character.js:244-338
        /// 处理角色的静止、基础按键响应
        /// </summary>
        private bool State_Standing(string eventType, object eventData)
        {
            {
                switch (eventType)
                {
                    case "frame":
                        if(IsHeavyWeapon())
                            TransitionToFrame(LF2StandardFrames.HeavyObjWalk0);

                        break;

                    case "combo":
                        // 站立状态的输入响应 (对应 FLF Line 250-338)
                        string comboKey = eventData as string;
                        Log.Info("[State {0}] Event={1}", "ComboKey = {2}", "Standing", eventType,comboKey);
                        // === 方向键与跳跃键处理 (FLF Line 253-272) ===
                        switch (comboKey)
                        {
                            case "left":
                            case "right":
                            case "up":
                            case "down":
                            case "jump":
                            case "":
                            case null:
                                // 检查是否有实际方向输入
                                {
                                    bool hasDx = Controller.IsLeft != Controller.IsRight;
                                    bool hasDz = Controller.IsUp != Controller.IsDown;
                                    if (hasDx || hasDz)
                                    {
                                        if (IsHeavyWeapon())
                                        {
                                            if (hasDx) PS.vx = Dirh() * _FrameDataWrapper.characterData.heavy_walking_speed;
                                            PS.vz = Dirv() * _FrameDataWrapper.characterData.heavy_walking_speedz;
                                        }
                                        else
                                        {
                                            // 除非按下的是跳跃键，否则切换到行走状态
                                            if (comboKey != "jump")
                                            {
                                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.WalkingStart, "方向键按下 -> 行走");
                                                TransitionToFrame(LF2StandardFrames.WalkingStart, 5);
                                            }

                                            // 设置速度 (对应 FLF Line 265-270)
                                            // 注意: FLF 在 Standing 状态不使用 xFactor (斜向减速)，只有 Walking 状态使用
                                            var characterData = _FrameDataWrapper?.characterData;
                                            if (characterData == null) return false;

                                            if (hasDx) PS.vx = Dirh() * characterData.walking_speed;
                                            PS.vz = Dirv() * characterData.walking_speedz;
                                        }

                                    }
                                }
                                break;
                        }

                        // === 动作键处理 ===
                        switch (comboKey)
                        {
                            case "left-left":
                            case "right-right":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.RunningStart, "双击方向键 -> 奔跑");
                                if (IsHeavyWeapon())
                                    TransitionToFrame(LF2StandardFrames.HeavyObjRun, LF2StateConstants.ComboTransitionWait);
                                else
                                    TransitionToFrame(LF2StandardFrames.RunningStart, LF2StateConstants.ComboTransitionWait);

                                StateReturnFrame = 1;
                                return true;

                            case "def":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Defend, "防御键 -> 防御");
                                if (IsHeavyWeapon()) 
                                {
                                    StateReturnFrame = 1;
                                    return true;
                                }

                                TransitionToFrame(LF2StandardFrames.Defend, LF2StateConstants.ComboTransitionWait);
                                StateReturnFrame = 1;
                                return true;

                            case "jump":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Jumping, "跳跃键 -> 跳跃");
                                if (IsHeavyWeapon())
                                {
                                    if ((bool)Proper("heavy_weapon_jump"))
                                    {
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else
                                    {
                                        TransitionToFrame((int)Proper("heavy_weapon_jump"), LF2StateConstants.ComboTransitionWait);
                                        return true;
                                    }
                                }

                                TransitionToFrame(LF2StandardFrames.Jumping, LF2StateConstants.ComboTransitionWait);
                                StateReturnFrame = 1;
                                return true;

                            case "att":
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Punch, "攻击键 -> 挥拳");
                                if (_heldWeapon != null)
                                {
                                    bool hasDx = Controller.IsLeft != Controller.IsRight;

                                    if (IsHeavyWeapon())
                                    {
                                        TransitionToFrame(LF2StandardFrames.HeavyWeaponThw, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else if ((bool)Proper(_heldWeapon.ObjectId, "just_throw")) 
                                    {
                                        TransitionToFrame(LF2StandardFrames.LightWeaponThw, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else if ((bool)Proper(_heldWeapon.ObjectId, "stand_throw"))
                                    {
                                        TransitionToFrame(LF2StandardFrames.LightWeaponThw, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else if ((bool)Proper(_heldWeapon.ObjectId, "attackable"))
                                    {
                                        // FLF character.js:303 — $.match.random() < 0.5 选择武器攻击帧
                                        int NormalWeaponAtck = Match.Rng.Next() < 0.5f ? LF2StandardFrames.NormalWeaponAtck : LF2StandardFrames.NormalWeaponAtck2;
                                        TransitionToFrame(NormalWeaponAtck, LF2StateConstants.ComboTransitionWait);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                }

                                // 重拳检测：用帧72/73的itr范围检测前方是否有近身敌人（itr kind:6）
                                var sceneQuery = Match?.SceneQuery;
                                if (sceneQuery != null)
                                {
                                    var superPunchFrame = FrameCache.GetFrameDataById(LF2StandardFrames.SuperPunch2)
                                                       ?? FrameCache.GetFrameDataById(LF2StandardFrames.SuperPunch2 + 1);
                                    if (superPunchFrame?.itrs != null && superPunchFrame.itrs.Count > 0)
                                    {
                                        float spriteW = GetSpriteWidthPxForCollision();
                                        var itrVol = PS.GetItrVolume(superPunchFrame.itrs[0], superPunchFrame.centerx, superPunchFrame.centery, spriteW);
                                        var hits = sceneQuery.QueryItrs(itrVol, this, 6, Team);
                                        if (hits != null && hits.Count > 0)
                                        {
                                            TransitionToFrame(LF2StandardFrames.SuperPunch, LF2StateConstants.ComboTransitionWait);
                                            StateReturnFrame = 1;
                                            return true;
                                        }
                                    }
                                }

                                // FLF character.js:361 — $.match.random() < 0.5 选择挥拳帧 (60 或 65)
                                int punchFrame = Match.Rng.Next() < 0.5f ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                                TransitionToFrame(punchFrame, LF2StateConstants.ComboTransitionWait);
                                return true;
                        }

                        break;
                }
                return false;
            }
        }


        /// <summary>
        /// 行走状态处理器 (State 1)
        /// 对应 FLF character.js:341-400
        /// 
        /// <para>特性：</para>
        /// <list type="bullet">
        /// <item>在函数开头计算输入 (dx, dz)。</item>
        /// <item>TU 事件中更新速度，包含斜向移动减速 (xFactor)。</item>
        /// <item>Combo 事件处理转向和停止。</item>
        /// </list>
        /// </summary>
        private bool State_Walking(string eventType, object eventData)
        {

             (int dx, int dz) = Controller.GetMoveInput();

            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}] Event={1}", "ComboKey = {2}", "walking", eventType, eventData is string);
                    if (IsHeavyWeapon())
                    {
                        if (dx != 0 || dz != 0)
                            FrameAniOscillate(LF2StandardFrames.HeavyObjWalk0, LF2StandardFrames.HeavyObjWalk3);
                        else
                            Trans.SetNext(Frame.N);
                    }
                    else
                        FrameAniOscillate(LF2StandardFrames.WalkingStart, LF2StandardFrames.WalkingEnd);

                    Trans.SetWait(_FrameDataWrapper.characterData.walking_frame_rate - 1);
                    return false;

                case "TU":
                    {
                        var characterData = _FrameDataWrapper?.characterData;
                        if (characterData == null) return false;

                        var xfactor = 1 - (Dirv() != 0 ? 1 : 0) * (2f / 7f);

                        if (IsHeavyWeapon())
                        {
                            if (dx != 0) PS.vx = Dirh() * characterData.heavy_walking_speed * xfactor;
                            PS.vz = Dirv() * characterData.heavy_walking_speedz;
                        }
                        else
                        {
                            if (dx != 0) PS.vx = Dirh() * characterData.walking_speed * xfactor;
                            PS.vz = Dirv() * characterData.walking_speedz;
                        }

                        if (dx == 0 && dz == 0 && Trans.Next != LF2StandardFrames.LoopToStart)
                        {
                            Trans.SetNext(LF2StandardFrames.LoopToStart);
                            Trans.SetWait(1, 1, 2);
                        }
                    }
                    return false;

                case "state_entry":
                    Trans.SetWait(0);
                    return false;

                case "combo":
                    // 行走中的输入处理
                    string comboKey = eventData as string;

                    // 1. 处理转向
                    if (dx != 0 && dx != Dirh())
                    {
                        SwitchDir(PS.dir == "right" ? "left" : "right");
                    }

                    // 2. 停止移动时应用一次性减速 (Friction)
                    if (dx == 0 && dz == 0 && !StateMem.ContainsKey("released"))
                    {
                        StateMem["released"] = true;
                        // Step 2: 移除 unitActions.ApplyUnitFriction，摩擦力由 PS 系统处理
                    }

                    // 3. 按键处理委托给 StandingStateHandler (如跳跃、攻击逻辑相同)
                    if (!string.IsNullOrEmpty(comboKey))
                    {
                        return State_Standing("combo", comboKey);
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 奔跑状态处理器 (State 2)
        /// 对应 FLF character.js:403-486
        /// <para>注意：Frame 事件没有 break，会穿透执行 TU 逻辑 (模拟 switch fallthrough)。</para>
        /// </summary>
        private bool State_Running(string eventType, object eventData)
        {
            {
                switch (eventType)
                {
                    case "frame":
                        Log.Info("[State {0}] Event={1}", "ComboKey = {2}", "running", eventType, eventData is string);
                        if (IsHeavyWeapon())
                            FrameAniOscillate(LF2StandardFrames.HeavyObjRun, LF2StandardFrames.TreeJump1);
                        else
                            FrameAniOscillate(LF2StandardFrames.RunningStart, LF2StandardFrames.RunningEnd);
                        if (_FrameDataWrapper?.characterData == null) return false;
                        Trans.SetWait(_FrameDataWrapper.characterData.running_frame_rate);
                        goto case "TU";

                    case "TU":
                        {
                            var xfactor = 1 - (Dirv() != 0 ? 1 : 0) * (1f / 7f);
                            var characterData = _FrameDataWrapper?.characterData;
                            if (characterData == null) return false;

                            if (IsHeavyWeapon())
                            {
                                PS.vx = xfactor * Dirh() * characterData.heavy_running_speed;
                                PS.vz = Dirv() * characterData.heavy_running_speedz;
                            }
                            else
                            {
                                PS.vx = xfactor * Dirh() * characterData.running_speed;
                                PS.vz = Dirv() * characterData.running_speedz;
                            }
                        }
                        return false;

                    case "combo":
                        string comboKey = eventData as string;

                        if (!string.IsNullOrEmpty(comboKey))
                        {
                            // 1. 反向输入检测 -> 停止奔跑 (急停)
                            if (comboKey == "left" || comboKey == "right" || comboKey == "left-left" || comboKey == "right-right")
                            {
                                string inputDir = comboKey.Split('-')[0];

                                if (inputDir != PS.dir)
                                {
                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.StopRunning, "反向输入 -> 急停");
                                    if (IsHeavyWeapon())
                                        TransitionToFrame(LF2StandardFrames.TreeJump2, 10);
                                    else
                                        TransitionToFrame(LF2StandardFrames.StopRunning, 10);

                                    StateReturnFrame = 1;
                                    return true;
                                }
                            }
                            // 2. 奔跑防御
                            else if (comboKey == "def")
                            {
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", 102, "防御 -> 奔跑防御");
                                if (IsHeavyWeapon())
                                {
                                    StateReturnFrame = 1;
                                    return true;
                                }
                                TransitionToFrame(102, 10);
                                return true;
                            }
                            // 3. 奔跑跳跃 -> 冲刺 (Dash)
                            else if (comboKey == "jump")
                            {
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.DashForward, "跳跃 -> 冲刺");
                                if (IsHeavyWeapon())
                                {
                                    if ((bool)Proper("heavy_weapon_dash"))
                                    {
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else
                                    {
                                        TransitionToFrame((int)Proper("heavy_weapon_dash"), 10);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                }

                                TransitionToFrame(LF2StandardFrames.DashForward, 10);
                                StateReturnFrame = 1;
                                return true;
                            }
                            // 4. 奔跑攻击
                            else if (comboKey == "att")
                            {
                                if (_heldWeapon != null)
                                {

                                    if (_heldWeapon is LF2HeavyWeapon)
                                    {
                                        TransitionToFrame(LF2StandardFrames.HeavyWeaponThw, 10);
                                        StateReturnFrame = 1;
                                        return true;
                                    }
                                    else
                                    {
                                        bool hasDx = Controller.IsLeft != Controller.IsRight;
                                        if (hasDx && (bool)Proper(_heldWeapon.ObjectId, "run_throw"))
                                        {
                                            TransitionToFrame(LF2StandardFrames.LightWeaponThw, 10);
                                            StateReturnFrame = 1;
                                            return true;
                                        }
                                        else if ((bool)Proper(_heldWeapon.ObjectId, "attackable"))
                                        {
                                            TransitionToFrame(LF2StandardFrames.RunWeaponAtck, 10);
                                            StateReturnFrame = 1;
                                            return true;
                                        }
                                    }
                                }

                                TransitionToFrame(LF2StandardFrames.RunAttack, 10);
                                StateReturnFrame = 1;
                                return true;
                            }
                        }
                        return false;

                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// 攻击状态处理器 (State 3)
        /// 对应 FLF character.js:489-549
        /// 处理所有攻击动作 (普通、跳跃、冲刺攻击) 的通用逻辑
        /// </summary>
        // 状态 3: 攻击
        private bool State_Attack(string eventType, object eventData)
        {

            switch (eventType)
            {
                case "frame":
                    // 空中攻击保持逻辑: 如果攻击结束时还在空中，强制切回跳跃状态
                    var D = Frame.D;
                    if (D.next == LF2StandardFrames.LoopToStart && PS.vy < 0)
                    {
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 3, "Attack", LF2StandardFrames.JumpingAir, "空中攻击结束 -> 返回跳跃");
                        Trans.SetNext(LF2StandardFrames.JumpingAir);
                    }
                    return false;

                case "hit_stop":
                    // 命中停顿 (卡肉) 效果
                    // 部分攻击帧 (如 86, 87, 91) 在命中时会延长当前帧时间
                    if (CurrentFrameId == 86 || CurrentFrameId == 87 || CurrentFrameId == 91)
                    {
                        Trans.IncWait(1, 10);
                        return true;
                    }
                    return false;

                case "TU":
                    // FLF character.js:516-548
                    // if (frame.D.itr && (kind==10||11) && match.time.t % 2 === 0)
                    //   椭圆范围 x²+4z²<150² 内所有目标执行 hit(frame[251].itr[0])
                    //   target.ps.y<0 || type=='character' || random()<0.15 才攻击
                    int tickTU = SimulationTickDriver.Instance != null
                        ? SimulationTickDriver.Instance.CurrentTickIndex
                        : 0;
                    var frameDataTU = Frame.D;
                    if (frameDataTU.itrs != null)
                    {
                        foreach (var itr in frameDataTU.itrs)
                        {
                            if ((itr.kind == 10 || itr.kind == 11) && tickTU % 2 == 0)
                            {
                                var sceneQueryTU = Match?.SceneQuery;
                                if (sceneQueryTU == null) break;

                                var frame251 = FrameCache.GetFrameDataById(LF2StandardFrames.FluteAttackDamage);
                                if (frame251?.itrs == null || frame251.itrs.Count == 0) break;

                                var itr251 = frame251.itrs[0];
                                float spriteWTU = GetSpriteWidthPxForCollision();
                                var vol251 = PS.GetItrVolume(itr251, frame251.centerx, frame251.centery, spriteWTU);

                                List<LF2LivingObject> allObjects = new List<LF2LivingObject>();
                                Match.GetAllLivingObjects(allObjects);

                                for (int i = 0; i < allObjects.Count; i++)
                                {
                                    var target = allObjects[i];
                                    if (target == this) continue;
                                    if (target.PS == null) continue;

                                    float zDiff = Mathf.Abs(target.PS.z - PS.z);
                                    float xDiff = Mathf.Abs(target.PS.x - PS.x);
                                    if (xDiff * xDiff + 4 * zDiff * zDiff < 150 * 150)
                                    {
                                        // FLF character.js:556 — $.match.random() < 0.15 随机攻击地面对象
                                        bool randHit = Match.Rng.Next() < 0.15f;
                                        if (target.PS.y < 0 ||
                                            target.Type == LF2ObjectType.Character ||
                                            (target.PS.y >= 0 && randHit))
                                        {
                                            if (target is LF2Character targetChar && targetChar.GetHeldWeapon() != null)
                                                targetChar.DropWeapon(0, 0);

                                            if (target.Hit(itr251, this, new Vector3(PS.x, PS.y, PS.z), vol251))
                                            {
                                                target.Attacked(itr251, this);
                                                target.ItrArestUpdate(itr251);
                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        }
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 跳跃状态处理器 (State 4)
        /// 对应 FLF js:552-602
        /// </summary>
        private bool State_Jump(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    // 标记 frameTU，用于 TU 事件中处理起跳物理
                    SetStateMemory("frameTU", true);

                    // 攻击锁定: 防止连续跳跃攻击 (Jump Attack 后有 2 帧锁定)
                    if (Frame.PN == LF2StandardFrames.JumpAttack ||
                        Frame.PN == LF2StandardFrames.JumpAttack + 1)
                    {
                        SetStateMemory("attlock", 2);
                    }
                    return false;

                case "TU":
                    // 1. 起跳速度设置 (Frame 211 -> 212)
                    if (GetStateMemory("frameTU", out bool frameTUValue) && frameTUValue)
                    {
                        SetStateMemory("frameTU", false);
                        if (Frame.N == LF2StandardFrames.JumpingAir &&
                            Frame.PN == LF2StandardFrames.JumpingUp)
                        {
                            var (dx, dz) = Controller.GetMoveInput();
                            var characterData = _FrameDataWrapper?.characterData;
                            if (characterData == null) return false;

                            // 应用跳跃速度
                            PS.vx = dx * (characterData.jump_distance - 1);
                            PS.vz = Dirv() * (characterData.jump_distancez - 1);
                            PS.vy = characterData.jump_height;
                        }
                    }

                    // 2. 更新攻击锁定计时器
                    if (GetStateMemory("attlock", out int lockVal))
                    {
                        StateMem["attlock"] = lockVal - 1;
                    }
                    return false;

                case "combo":
                    string comboKey = eventData as string;
                    if ((comboKey == "att" || Controller.IsAttack) && !GetStateMemory("attlock", out int attlockValue))
                    {
                        if (Frame.N == LF2StandardFrames.JumpingAir)
                        {
                            if (_heldWeapon != null)
                            {
                                bool Hasdx = Controller.IsLeft != Controller.IsRight;
                                if (Hasdx && (bool)Proper(_heldWeapon.ObjectId, "attackable"))
                                {
                                    TransitionToFrame(LF2StandardFrames.SkyLgtWpThw, 10);
                                    // 空中投掷轻型武器
                                }
                                else if ((bool)(Proper(_heldWeapon.ObjectId, "attackable")))
                                {
                                    TransitionToFrame(LF2StandardFrames.JumpWeaponAtck, 10);
                                }
                            }
                            else
                            {
                                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 4, "Jump", LF2StandardFrames.JumpAttack, "跳跃攻击");
                                TransitionToFrame(LF2StandardFrames.JumpAttack, 10);
                            }

                            StateReturnFrame = 1;
                            return true;
                        }
                    }
                    return false;
            }
            return false;

        }

        /// <summary>
        /// 冲刺状态处理器 (State 5)
        /// 对应 FLF js:605-651
        /// </summary>
        private bool State_Dash(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    // 从奔跑或蹲下进入冲刺时，设置初始冲刺速度
                    if ((Frame.PN >= LF2StandardFrames.RunningStart &&
                         Frame.PN <= LF2StandardFrames.RunningEnd) ||
                        Frame.PN == LF2StandardFrames.Crouch)
                    {
                        var characterData = _FrameDataWrapper?.characterData;
                        if (characterData == null) return false;

                        PS.vx = Dirh() * (characterData.dash_distance - 1) * (Frame.N == LF2StandardFrames.DashForward ? 1 : -1);
                        PS.vz = Dirv() * (characterData.dash_distancez - 1);
                        PS.vy = characterData.dash_height;
                    }
                    return false;

                case "combo":
                    string comboKey = eventData as string;
                    // 1. 冲刺攻击
                    if (comboKey == "att" || Controller.IsAttack)
                    {
                        if (Dirh() == (PS.vx > 0 ? 1 : -1))
                        {
                            if (_heldWeapon != null && (bool)Proper(_heldWeapon.ObjectId, "attackable"))
                            {
                                TransitionToFrame(LF2StandardFrames.DashWeaponAtck, 10);
                            }
                            else
                            {
                                TransitionToFrame(LF2StandardFrames.DashAttack, 10);
                            }
                        }
                        AllowSwitchDir = false;
                        if (comboKey == "att")
                        {
                            StateReturnFrame = 1;
                            return true;
                        }
                    }
                    // 2. 冲刺转身
                    if (comboKey == "left" || comboKey == "right")
                    {
                        if (comboKey != PS.dir)
                        {
                            if (Dirh() == (PS.vx > 0 ? 1 : -1))
                            {
                                // 转身
                                if (Frame.N == LF2StandardFrames.DashForward)
                                    TransitionToFrame(LF2StandardFrames.DashForward2, 0);

                                if (Frame.N == LF2StandardFrames.DashBack)
                                    TransitionToFrame(LF2StandardFrames.DashBack2, 0);

                                SwitchDir(comboKey);
                            }
                            else
                            {
                                // 转向
                                if (Frame.N == LF2StandardFrames.DashForward2)
                                    TransitionToFrame(LF2StandardFrames.DashForward, 0);

                                if (Frame.N == LF2StandardFrames.DashBack2)
                                    TransitionToFrame(LF2StandardFrames.DashBack, 0);

                                SwitchDir(comboKey);
                            }
                            return true;
                        }

                    }
                    break;
            }
            return false;

        }

        /// <summary>
        /// 爬起状态处理器 (state = 6)
        /// 对应 FLF js:656-681
        ///
        /// 功能：处理被击飞后的爬起动作（减速下落）
        /// 关键帧：
        /// - 100: 正面爬起暂停
        /// - 101: 正面爬起结束
        /// - 108: 背面爬起暂停
        /// - 109: 背面爬起结束
        /// </summary>
        private bool State_Rowing(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "TU":
                    Log.Info("[State {0}:TU] ", eventType);

                    // ✓ 垂直速度重置（对应 FLF Line 660-664）
                    // 特定帧的重置垂直速度，使角色停止在空中
                    if (CurrentFrameId == LF2StandardFrames.Rowing ||      // 100: 正面爬起
                        CurrentFrameId == LF2StandardFrames.RowingBack)    // 108: 背面爬起
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", $"爬起暂停 Frame={CurrentFrameId}");
                        PS.vy = 0;
                    }
                    return false;

                case "frame":
                    Log.Info("[State {0}:frame] ", eventType);

                    // ✓ 等待时间设置（对应 FLF Line 667-671）
                    // 延长爬起动作的持续时间
                    if (CurrentFrameId == LF2StandardFrames.Rowing ||      // 100
                        CurrentFrameId == LF2StandardFrames.RowingBack)    // 108
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 6, "Rowing", "设置爬起等待时间");
                        Trans.SetWait(LF2StateConstants.RowingWaitTime);  // 1 帧
                        return true;
                    }
                    return false;

                case "fall_onto_ground":
                    Log.Info("[State {0}:fall_onto_ground] ", eventType);

                    // ✓ 落地处理（对应 FLF Line 674-679）
                    // 落地时的状态转换：爬起结束 → 蹲姿
                    if (CurrentFrameId == LF2StandardFrames.Rowing1 ||     // 101: 正面爬起结束
                        CurrentFrameId == LF2StandardFrames.RowingBack1)   // 109: 背面爬起结束
                    {
                        Log.Info("爬起结束落地");
                        Log.Info("TransitionTo: Frame {0} ({1})", LF2StandardFrames.Crouch, "落地 → 蹲姿");
                        TransitionToFrame(LF2StandardFrames.Crouch, 0);  // 215: 蹲姿帧
                        return true;
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 防御状态处理器 (state = 7)
        /// 对应 FLF js:684-695
        /// 
        /// 功能：处理防御相关逻辑
        /// 关键帧：
        /// - 110: 防御起始帧
        /// - 111: 防御成功（受击时转入）
        /// - 112: 防御被破（defend超过上限时转入）
        /// </summary>
        private bool State_Defending(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}", 7, "Defending", eventType);

                    // ✓ 防御等待时间延长（对应 FLF Line 688-693）
                    // 给予视觉反馈，让玩家感知到成功防御
                    if (Frame.N == LF2StandardFrames.Defend1)  // 111: 防御成功帧
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 7, "Defending", "防御成功 → 延长等待时间");
                        // 增加4帧等待时间（延长防御状态）
                        Trans.IncWait(LF2StateConstants.DefendSuccessWaitBonus);
                    }
                    break;
            }

            return false;
        }


        /// <summary>
        /// 防御被破状态处理器 (state = 8)
        /// 对应 FLF js:698-719
        ///
        /// 功能：处理防御被破后的特殊移动逻辑
        /// 关键机制：修复弱击倒移动时的方向问题
        /// 问题：防御被破时，角色被击退方向可能与朝向方向相反
        /// 解决：在空中或速度不足时，强制按帧定义的dvx设置速度
        /// </summary>
        private bool State_BrokenDefend(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame_force":
                case "TU_force":
                    Log.Info("[State {0}:{1}] Event={2}", 8, "BrokenDefend", eventType);

                    // ✓ 强制移动方向修正（对应 FLF Line 702-717）

                    var D = Frame.D;
                    if (D.dvx != 0)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 8, "BrokenDefend", $"防御被破 dvx={D.dvx} → 修正移动方向");
                        if ((PS.vx > 0 ? 1 : -1) != Dirh())
                        {
                            float avx = PS.vx > 0 ? PS.vx : -PS.vx;
                            float dirx = 2 * (PS.vx > 0 ? 1 : -1);
                            if (PS.y < 0 || avx < D.dvx)
                                PS.vx = dirx * D.dvx;

                            if (D.dvx < 0)
                                PS.vx -= dirx;

                        }
                    }
                    break;
            }

            return false;
        }


        /// <summary>
        /// 抓取状态处理器 (state = 9)
        /// 对应 FLF js:722-853
        ///
        /// 功能：处理抓取敌人和投掷动作
        /// 关键特性：
        /// 1. 抓取计数器系统（counter 从43递减到0）
        /// 2. 攻击次数记录（每次成功攻击延长抓取时间）
        /// 3. 位置同步（每帧更新被抓对象位置到cpoint）
        /// 4. 伤害处理（通过cpoint.injury）
        /// 5. Z轴层级控制（cover参数）
        /// 6. 方向控制（dircontrol参数）
        /// 7. 投掷/攻击/跳跃动作（taction/aaction/jaction）
        /// </summary>
        private bool State_Catching(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    // ✓ 初始化抓取状态（对应 FLF Line 570-573）
                    StateMem["stateTU"] = true;
                    StateMem["counter"] = 43;    // 初始计数43帧
                    StateMem["attacks"] = 0;     // 攻击次数计数
                    caught_decrease_counter = 99; // 默认值99（从反汇编确认）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "初始化抓取状态 counter=43, attacks=0, decrease=99");
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "Clear catching state");
                    Catching = null;
                    PS.zz = 0;
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    // ✓ 抓取帧处理（对应 FLF Line 584-614）
                    int frameId = CurrentFrameId;
                    var D = Frame.D;

                    // ==================== 特殊帧处理 ====================

                    // 帧123（成功攻击）：增加attacks计数器，延长抓取时间3帧
                    if (frameId == 123)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "帧123 成功攻击 → 延长抓取时间");
                        StateMem["attacks"] = (int)StateMem["attacks"] + 1;
                        StateMem["counter"] = (int)StateMem["counter"] + 3;
                        Trans.SetWait(Trans.Wait + 1);
                        return true;
                    }

                    // 帧233/234：减少等待时间（1帧）
                    if (frameId == 233 || frameId == 234)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", $"Frame {frameId} -> decrease wait");
                        Trans.SetWait(Trans.Wait - 1);
                        return true;
                    }

                    // 帧240：Rudolf特殊变身
                    if (frameId == 240)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "帧240 Rudolf特殊变身");
                        // TODO: 需要实现id_update机制
                        // CallIdUpdate("rudolf_transform");
                        // return true;
                    }

                    // 位置同步
                    if (Catching is LF2Character caughtChar && D.cpoint != null)
                    {
                        // 从 cpoint.decrease 初始化计数器（仅首次）
                        if (D.cpoint.decrease > 0 && caught_decrease_counter == 99)
                        {
                            caught_decrease_counter = D.cpoint.decrease;
                        }
                        
                        int adir = (PS.dir == "right") ? 1 : -1;
                        int vdir = 1;
                        Vector3 holdpoint = new Vector3(
                            PS.x + D.cpoint.x * adir,
                            PS.y + D.cpoint.y,
                            PS.z
                        );
                        caughtChar.caught_b(holdpoint, D.cpoint, adir, vdir);
                    }

                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    if (Catching != null &&
                        caught_cpointkind() == 1 &&
                        ((LF2Character)Catching).caught_cpointkind() == 2)
                    {
                        if (StateMem.ContainsKey("stateTU") && (bool)StateMem["stateTU"])
                        {
                            StateMem["stateTU"] = false;

                            var cpoint = Frame.D.cpoint;

                            if (cpoint.injury != 0)
                            {
                                NTSDDamageCalculator.ApplyDamage(Catching, cpoint.injury);
                            }

                            int cover = cpoint.cover;
                            if (cover == 0 || cover == 10)
                            {
                                PS.zz = 1;
                            }
                            else
                            {
                                PS.zz = -1;
                            }

                            if (cpoint.dircontrol == 1 && Controller != null)
                            {
                                if (Controller.IsLeft)
                                {
                                    SwitchDir("left");
                                }
                                else if (Controller.IsRight)
                                {
                                    SwitchDir("right");
                                }
                            }
                        }
                    }

                    return false;

                case "post_combo":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 9, "Catching", eventType, CurrentFrameId);

                    // 抓取计数器递减（被抓者按方向键时递减）
                    if (Catching != null && Catching.Controller != null)
                    {
                        var caughtCtrl = Catching.Controller;
                        if (caughtCtrl.IsLeft || caughtCtrl.IsRight || caughtCtrl.IsUp || caughtCtrl.IsDown)
                        {
                            caught_decrease_counter--;
                            Log.Info("[State {0}:{1}] -> Branch: {2}, Counter={3}", 9, "Catching", "被抓者按键，计数器递减", caught_decrease_counter);
                            
                            if (caught_decrease_counter <= 0)
                            {
                                Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "计数器归零，释放被抓者");
                                if (Catching is LF2Character victim)
                                {
                                    victim.caught_release();
                                }
                                Catching = null;
                                TransitionToFrame(LF2StandardFrames.Standing, 22);
                                return true;
                            }
                        }
                    }
                    return false;

                case "combo":
                    string comboKey = eventData as string;
                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 9, "Catching", eventType, comboKey, CurrentFrameId);

                    if (string.IsNullOrEmpty(comboKey))
                        return false;

                    var comboCpoint = Frame.D?.cpoint;
                    if (comboCpoint == null)
                        return false;

                    if (comboKey == "att")
                    {
                        // 投掷动作优先于攻击动作
                        if (comboCpoint.taction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "taction 投掷");
                            if (Catching is LF2Character throwTarget)
                            {
                                int vdir = (PS.dir == "right") ? 1 : -1;
                                throwTarget.caught_throw(comboCpoint, vdir);
                                
                                // 设置被抓者速度
                                throwTarget.PS.vx = comboCpoint.throwvx * vdir;
                                throwTarget.PS.vy = comboCpoint.throwvy;
                                throwTarget.PS.vz = comboCpoint.throwvz;
                                
                                // 设置投掷伤害（落地时生效）
                                if (comboCpoint.throwinjury != 0)
                                {
                                    throwTarget.caught_throwinjury = comboCpoint.throwinjury;
                                }
                            }
                            TransitionToFrame(comboCpoint.taction, 22);
                            Catching = null;
                            return true;
                        }
                        else if (comboCpoint.aaction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "aaction 攻击");
                            TransitionToFrame(comboCpoint.aaction, 22);
                            return true;
                        }
                        return false;
                    }

                    if (comboKey == "jump")
                    {
                        if (comboCpoint.jaction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "jaction 跳跃");
                            TransitionToFrame(comboCpoint.jaction, 22);
                            return true;
                        }
                        return false;
                    }

                    if (comboKey == "def")
                    {
                        if (comboCpoint.daction != 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 9, "Catching", "daction 防御");
                            TransitionToFrame(comboCpoint.daction, 22);
                            return true;
                        }
                        return false;
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 被抓取状态处理器 (state = 10)
        /// 对应 FLF js:856-939
        ///
        /// 功能：处理被敌人抓取时的表现
        /// 关键特性：
        /// 1. 位置同步到抓取者的 cpoint
        /// 2. 被投掷时的速度设置（throwvx/vy/vz）
        /// 3. 方向处理（cover 参数控制）
        /// 4. 投掷伤害记录（落地时生效）
        /// 5. 抓取状态验证（双向检查）
        /// </summary>
        private bool State_BeingCaught(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "Clear being-caught state");
                    // 清理被抓状态（FLF:781-787）
                    Catching = null;
                    caught_b_holdpoint = Vector3.zero;
                    caught_b_cpoint = null;
                    caught_b_adir = 0;
                    caught_b_vdir = 0;
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);

                    // ✓ 被抓帧处理（对应 FLF Line 792-794）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "设置长时间等待（由抓取者控制）");
                    StateMem["frameTU"] = true;
                    Trans.SetWait(99);  // 长时间等待（由抓取者控制）
                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 10, "BeingCaught", eventType, CurrentFrameId);

                    // ✓ 被抓时的处理（对应 FLF Line 803-880）

                    // ==================== 帧135时消除重力 ====================
                    // 对应 FLF Line 804-807
                    if (CurrentFrameId == 135)
                    {
                        // Step 2: 使用 PS.vy 替代 unitActions.yForce
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 10, "BeingCaught", "帧135 暂停（消除重力）");
                        PS.vy = 0;  // 暂停
                    }

                    // NTSD 2.4 速度处理（基于反汇编 loc_404E78）
                    
                    // vx 摩擦（向0靠近）
                    if (PS.vx < 0)
                        PS.vx += 1.1f;
                    else if (PS.vx > 0)
                        PS.vx -= 1.1f;

                    // vx 边界（±30）
                    PS.vx = Mathf.Clamp(PS.vx, -30f, 30f);

                    // 位置追踪
                    if (Catching != null)
                    {
                        if (Catching.PS.x > PS.x)
                            PS.vx += 0.85f;
                        else if (Catching.PS.x < PS.x)
                            PS.vx -= 0.85f;

                        if (Catching.PS.z > PS.z + 7)
                            PS.vz += 0.3f;
                        else if (Catching.PS.z < PS.z - 7)
                            PS.vz -= 0.3f;

                        PS.vy *= 0.714f;
                    }

                    // vx 边界（±13）
                    PS.vx = Mathf.Clamp(PS.vx, -13f, 13f);
                    // vz 边界（±2）
                    PS.vz = Mathf.Clamp(PS.vz, -2f, 2f);

                    // 根据 vx 设置朝向
                    if (PS.vx > 0)
                        PS.dir = "right";
                    else if (PS.vx < 0)
                        PS.dir = "left";

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 受伤状态处理器 (state = 11)
        /// 对应 FLF js:942-960
        /// 
        /// 功能：处理硬直受伤的表现（不倒地）
        /// 关键特性：
        /// 1. 延长受伤动作的持续时间（给予视觉反馈）
        /// 2. 受伤等级帧自动返回站姿
        /// 3. 受伤等级：220/221（轻度）、222/223（中度）、224/225（重度）、226（超重）
        /// </summary>
        private bool State_Injured(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    // ✓ 增加等待时间（对应 FLF Line 946-948）
                    int currentWait = Trans.Wait;
                    Trans.SetWait(Mathf.Min(currentWait + 1, 20));  // 上限20帧
                    return false;

                case "frame":
                    // ✓ 受伤动画结束处理（对应 FLF Line 949-958）
                    // 受伤结束帧（奇数帧 221/223/225）返回站姿
                    int frameId = CurrentFrameId;
                    if (frameId == LF2StandardFrames.Injured1 ||       // 221
                        frameId == LF2StandardFrames.Injured3 ||       // 223
                        frameId == LF2StandardFrames.Injured5)         // 225
                    {
                        Trans.SetNext(LF2StandardFrames.LoopToStart);  // 999
                        return true;
                    }

                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 倒地状态处理器 (state = 12)
        /// 对应 FLF js:963-1089
        /// 
        /// 这是一个高优先级状态，包含复杂的倒地逻辑：
        /// 1. 基于垂直速度的动画状态机（上浮/下落不同帧序列）
        /// 2. 爬起/直接躺地的判定（基于总速度）
        /// 3. 倒地无敌时间管理（fall值减少）
        /// 4. 按键起身逻辑（帧182/188 + fall<KO + hp>0）
        /// 5. 摔落伤害结算（落地时生效）
        /// 
        /// 关键帧序列：
        /// - 正面：180 → 181 → 182 → 183 / 185（上浮/下落）
        /// - 背面：186 → 187 → 188 → 189 / 191（上浮/下落）
        /// - 爬起/躺地判定在 fell_onto_ground 事件
        /// </summary>
        private bool State_Falling(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    // FLF character.js:969-1020
                    // if ($.effect.dvy <= 0) {
                    //   case 180: $.trans.set_next(181); $.trans.set_wait(lookup_abs(GC.fall.wait180, $.effect.dvy))
                    //   case 181: $.trans.set_next(182); vy=abs(ps.vy)||5; wait= vy<=4?2 : vy<7?3 : 4
                    //   case 182: $.trans.set_next(183)
                    //   case 186: ps.vy||=5; $.trans.set_next(187)
                    //   case 187: $.trans.set_next(188)
                    //   case 188: $.trans.set_next(189)
                    // } else {
                    //   case 180: $.trans.set_next(185); $.trans.set_wait(1)
                    //   case 186: $.trans.set_next(191)
                    // }
                    int fn = Frame.N;
                    if (Effect.Dvy <= 0f)
                    {
                        switch (fn)
                        {
                            case LF2StandardFrames.FallingFront:
                                Trans.SetNext(LF2StandardFrames.FallingFront1);
                                Trans.SetWait((int)NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.FallWait180, Effect.Dvy));
                                break;
                            case LF2StandardFrames.FallingFront1:
                                Trans.SetNext(LF2StandardFrames.FallingFront2);
                                float vy181 = PS.vy == 0f ? 5f : Mathf.Abs(PS.vy);
                                if (PS.vy == 0f) PS.vy = 5f;
                                if      (vy181 <= 4f) Trans.SetWait(2);
                                else if (vy181 <  7f) Trans.SetWait(3);
                                else                  Trans.SetWait(4);
                                break;
                            case LF2StandardFrames.FallingFront2: Trans.SetNext(LF2StandardFrames.FallingFront3); break;
                            case LF2StandardFrames.FallingBack:
                                if (PS.vy == 0f) PS.vy = 5f;
                                Trans.SetNext(LF2StandardFrames.FallingBack1);
                                break;
                            case LF2StandardFrames.FallingBack1: Trans.SetNext(LF2StandardFrames.FallingBack2); break;
                            case LF2StandardFrames.FallingBack2: Trans.SetNext(LF2StandardFrames.FallingBack3); break;
                        }
                    }
                    else
                    {
                        switch (fn)
                        {
                            case LF2StandardFrames.FallingFront: Trans.SetNext(LF2StandardFrames.FallingFront5); Trans.SetWait(1); break;
                            case LF2StandardFrames.FallingBack: Trans.SetNext(LF2StandardFrames.FallingBack5); break;
                        }
                    }
                    return false;

                case "TU":
                    // FLF character.js:1038-1057
                    // $.health.fall > 0 → $.health.fall--
                    if (HitCounters.Fall > 0)
                        HitCounters.AddFall(-1);
                    return false;

                case "combo":
                    // FLF character.js:1059-1082
                    // if (frame.N===182||188) && K==='jump'
                    //   if (health.fall < GC.fall.KO && health.hp > 0)
                    //     trans.frame(182→100, 188→108)
                    //     if ps.vx: ps.vx = 5*sign(vx)
                    //     if ps.vy==0: ps.vy = 5
                    //     if ps.vz: ps.vz = 2*sign(vz)
                    // return 1  (屏蔽所有输入)
                    string comboKey = eventData as string;
                    int frameId = Frame.N;
                    if ((frameId == LF2StandardFrames.FallingFront2 || frameId == LF2StandardFrames.FallingBack2) && comboKey == "jump")
                    {
                        if (HitCounters.Fall < NTSDGlobal.Gameplay.FallKO && Health.HP > 0)
                        {
                            int rowingFrame = (frameId == LF2StandardFrames.FallingFront2)
                                ? LF2StandardFrames.Rowing
                                : LF2StandardFrames.RowingBack;
                            TransitionToFrame(rowingFrame, 10);

                            if (PS.vx != 0f) PS.vx = 5f * (PS.vx > 0f ? 1f : -1f);
                            if (PS.vy == 0f) PS.vy = 5f;
                            if (PS.vz != 0f) PS.vz = 2f * (PS.vz > 0f ? 1f : -1f);

                            return true;
                        }
                    }
                    return true;

                case "fell_onto_ground":
                case "fall_onto_ground":
                    // FLF character.js:1022-1036
                    // if (caught_throwinjury > 0) injury(caught_throwinjury); caught_throwinjury=null
                    // sound.play('1/016')
                    // if (mech.speed() > GC.character.bounceup.limit.xy || ps.vy > GC.character.bounceup.limit.y)
                    //   mech.linear_friction(lookup_abs(bounceup.absorb,vx), lookup_abs(bounceup.absorb,vz))
                    //   ps.vy = -GC.character.bounceup.y
                    //   203-206→185, 180-185→185, 186-191→191
                    // else
                    //   203-206→230, 180-185→230, 186-191→231
                    if (caught_throwinjury.HasValue && caught_throwinjury.Value > 0)
                    {
                        Injury(caught_throwinjury.Value);
                        caught_throwinjury = null;
                    }

                    float speed = CharacterMechanics.SpeedXY(PS);
                    int curFn = Frame.N;

                    if (speed > NTSDGlobal.Gameplay.CharBounceupLimitXY ||
                        PS.vy > NTSDGlobal.Gameplay.CharBounceupLimitY)
                    {
                        float absorbX = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.CharBounceupAbsorb, PS.vx);
                        float absorbZ = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.CharBounceupAbsorb, PS.vz);
                        CharacterMechanics.LinearFriction(PS, absorbX, absorbZ);
                        PS.vy = -NTSDGlobal.Gameplay.CharBounceupY;

                        if (curFn >= LF2StandardFrames.Fire && curFn <= LF2StandardFrames.Fire3) { StateReturnFrame = LF2StandardFrames.FallingFront5; return true; }
                        if (curFn >= LF2StandardFrames.FallingFront && curFn <= LF2StandardFrames.FallingFront5) { StateReturnFrame = LF2StandardFrames.FallingFront5; return true; }
                        if (curFn >= LF2StandardFrames.FallingBack && curFn <= LF2StandardFrames.FallingBack5) { StateReturnFrame = LF2StandardFrames.FallingBack5; return true; }
                    }
                    else
                    {
                        if (curFn >= LF2StandardFrames.Fire && curFn <= LF2StandardFrames.Fire3) { StateReturnFrame = LF2StandardFrames.Lying; return true; }
                        if (curFn >= LF2StandardFrames.FallingFront && curFn <= LF2StandardFrames.FallingFront5) { StateReturnFrame = LF2StandardFrames.Lying; return true; }
                        if (curFn >= LF2StandardFrames.FallingBack && curFn <= LF2StandardFrames.FallingBack5) { StateReturnFrame = LF2StandardFrames.LyingBack; return true; }
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 冰冻状态处理器 (state = 13)
        /// 对应 FLF js:1097-1106
        ///
        /// 功能：处理冰冻效果
        /// 关键特性：
        /// 1. 离开冰冻状态时创建冰块碎裂效果
        /// 2. 冰冻状态期间：
        ///    - 角色完全停止（无法移动、攻击、连招）
        ///    - 受到攻击时会碎裂（转入倒地状态）
        /// 3. 关键帧：200（冰冻帧）
        ///
        /// 冰冻机制（来自 hit 函数）：
        /// - effectnum = 3/30: 冰冻攻击
        /// - 未冰冻 → 转到帧200（冰冻）
        /// - 已冰冻 → 碎裂倒地（转到帧182）
        /// - 强制丢弃武器
        /// </summary>
        private bool State_Frozen(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 13, "Frozen", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 13, "Frozen", "冰冻结束 → 创建碎裂效果");
                    // 创建冰块碎裂效果（FLF:1101-1104）
                    // TODO: 实现特效系统（ID 212 碎裂效果，音效 1/066）
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 躺地状态处理器 (state = 14)
        /// 对应 FLF js:1113-1138
        ///
        /// 功能：处理落地后的躺地状态和死亡判定
        /// 关键特性：
        /// 1. state_entry：
        ///    - 重置 fall 和 bdefend 值（清空临时状态）
        ///    - 检测死亡（hp ≤ 0）并触发 die()
        ///    - NPC 死亡时启动玩家闪烁计数（30帧后销毁）
        /// 2. state_exit：
        ///    - 爬起后获得 30 帧无敌时间
        ///    - 启用透明效果提示玩家无敌状态
        ///    - 设置 super 状态（超级护甲）
        /// 3. 关键帧：
        ///    - 230: 正面躺地
        ///    - 231: 背面躺地
        ///
        /// 死亡流程（来自 generic 状态 TU 事件）：
        /// - 死亡闪烁到 4 阶段：
        ///   1. dead_blink_count = 0: 开启闪烁
        ///   2. 0 < count < 30: 持续闪烁
        ///   3. count >= 30: 关闭闪烁，隐藏精灵和影子
        ///   4. count = -1: 销毁对象
        /// </summary>
        private bool State_Lying(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    // FLF character.js:1117-1128
                    // $.health.fall = 0
                    // $.health.bdefend = 0
                    // if ($.health.hp <= 0)
                    //   $.die()
                    //   if ($.is_npc) $.counter.dead_blink_count = 0
                    HitCounters.ResetFall();
                    HitCounters.ResetBdefend();

                    if (Health.HP <= 0)
                    {
                        Dead = true;
                        if (_deadBlinkCount < 0)
                            _deadBlinkCount = 0;
                    }
                    return false;

                case "state_exit":
                    // FLF character.js:1130-1137
                    // $.effect.timein = 0
                    // $.effect.timeout = 30
                    // $.effect.blink = true
                    // $.effect.super = true
                    Effect.TimeIn  = 0;
                    Effect.TimeOut = 30;
                    Effect.Blink   = true;
                    Effect.Super   = true;
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 综合状态处理器 (state = 15)
        /// 对应 FLF js:1145-1223
        ///
        /// 功能：处理多种复杂状态（停止奔跑、蹲下、冲刺攻击、武器投掷等）
        /// 关键特性：
        /// 1. frame 事件：处理多种帧的特殊逻辑
        ///    - 帧9: 重武器停止奔跑 → 检查重武器，转到帧12
        ///    - 帧215: 蹲下 → 减少等待时间 1 帧
        ///    - 帧219: 蹲下 → 调用 id_update 或根据前帧应用冲刺力
        ///    - 帧54: 空中轻武器投掷结束 → 在空中时返回跳跃状态
        ///    - 帧257: Rudolf 消失帧 → 调用变身逻辑
        /// 2. combo 事件：蹲下二段跳（仅帧215）：
        ///    - 防御键 → 转到帧102（奔跑防御）
        ///    - 跳跃键 → 根据方向和速度决定跳跃类型：
        ///      * 有方向输入 → 该方向跳跃（帧213）
        ///      * 静止不动 → 垂直跳跃（帧210）
        ///      * 有速度同向 → 前冲刺（帧213）
        ///      * 有速度反向 → 后冲刺（帧214）
        ///
        /// 覆盖的状态类型：
        /// - 停止奔跑（stop_running）
        /// - 蹲下（crouch） 帧215
        /// - 蹲下2（crouch2） 帧219
        /// - 冲刺攻击（dash_attack）
        /// - 轻武器投掷（light_weapon_thw）
        /// - 重武器投掷（heavy_weapon_thw）
        /// - 重武器停止奔跑（heavy_stop_run） 帧9
        /// - 空中轻武器投掷（sky_lgt_wp_thw） 帧54
        /// - 消失（disappear） 帧257（Rudolf 特有）
        /// </summary>
        private bool State_StopRunning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}", 15, "Mixed", eventType);
                    // 多帧特殊处理（FLF:1149-1188）
                    int frameId = Frame.N;

                    if (frameId == LF2StandardFrames.TreeJump2)
                    {
                        if (IsHeavyWeapon())
                            Trans.SetNext(LF2StandardFrames.HeavyObjWalk0);
                        break;
                    }
                    else if (frameId == LF2StandardFrames.Crouch)  // 215
                    {
                        // 帧215: 蹲下 → 减少等待时间
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧215 蹲下 → 减少等待时间");
                        Trans.IncWait(-1);
                        break;
                    }
                    else if (frameId == LF2StandardFrames.Crouch2)
                    {
                        // 蹲下
                        if (!_CharacterHub._IdUpdate.TryInvokeGeneric(IdUpdateHooks.State15_Crouch))
                        {
                            switch (Frame.PN) // 上一帧编号
                            {
                                case LF2StandardFrames.Rowing5:
                                    // 划船后
                                    // 应用摩擦力
                                    CharacterMechanics.UnitFriction(PS);
                                    break;

                                case LF2StandardFrames.DashBack: // 冲刺后
                                case LF2StandardFrames.DashAttack:
                                case LF2StandardFrames.DashAttack + 1:
                                case LF2StandardFrames.DashAttack + 2: // 冲刺攻击
                                                                       // 减少等待时间
                                    Trans.IncWait(-1);
                                    break;
                            }
                        }
                    }
                    else if (frameId == LF2StandardFrames.SkyLgtWpThw3)
                    {
                        // 帧54: 空中轻武器投掷结束 → 在空中时返回跳跃状态
                        var D = Frame.D;
                        if (D.next == LF2StandardFrames.LoopToStart && PS.y < 0)
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧54 空中轻武器投掷结束 → 返回跳跃");
                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.JumpingAir, "空中投掷完成");
                            Trans.SetNext(LF2StandardFrames.JumpingAir);  // 212
                        }
                    }
                    else if (frameId == LF2StandardFrames.Disappear)
                    {
                        // 帧257: Rudolf 消失帧 → 调用变身逻辑

                        // 其他特殊帧需要武器系统
                    }
                    break;
                case "combo":
                    // ✓ 蹲下二段跳（对应 FLF Line 1190-1221）
                    string comboKey = eventData as string;
                    Log.Info("[State {0}:{1}] Event={2}, Key={3}", 15, "Mixed", eventType, comboKey);

                    // 只在蹲下帧215响应
                    if (Frame.N == LF2StandardFrames.Crouch)  // 215
                    {
                        if (string.IsNullOrEmpty(comboKey))
                            break;

                        // 防御键 → 奔跑防御
                        if (comboKey == "def")
                        {
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "蹲下 + 防御 → 奔跑防御");
                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", 102, "奔跑防御");
                            TransitionToFrame(LF2StandardFrames.Rowing2, 10);
                            return true;
                        }

                        // 跳跃键 → 4种跳跃类型
                        if (comboKey == "jump")
                        {
                            var (dx, dz) = Controller.GetMoveInput();
                            {
                                // 1. 有方向输入 → 该方向跳跃
                                if (dx != 0)
                                {
                                    Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", $"蹲下二段跳 dx={dx} → 方向跳跃");
                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward, "方向跳跃");
                                    TransitionToFrame(LF2StandardFrames.DashForward, 10);  // 213
                                    SwitchDir(dx == 1 ? DIRECTION.RIGHT : DIRECTION.LEFT);
                                }
                                else if (PS.vx == 0)
                                {
                                    Trans.IncWait(2, 10, 99);
                                    Trans.SetNext(LF2StandardFrames.Jumping, 10);
                                }
                                else if ((PS.vx > 0 ? 1 : -1) == Dirh())
                                {
                                    TransitionToFrame(LF2StandardFrames.DashForward, 10);  // 213
                                }
                                else
                                {
                                    Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "蹲下二段跳 → 前冲刺2");
                                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward2, "前冲刺2");
                                    // 检查角色是否静止（无水平速度）
                                    // 简化实现：直接跳到垂直跳跃
                                    TransitionToFrame(LF2StandardFrames.DashForward2, 10);  // 214
                                }
                            }

                            return true;
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// 受伤2状态处理器 (state = 16)
        /// 对应 FLF js:1230-1235
        ///
        /// 功能：痛苦之舞（Dance of Pain）状态
        /// 关键特性：
        /// 1. 空实现：无任何特殊逻辑
        /// 2. 所有行为由帧数据驱动（动画自动播放）
        /// 3. 可能是预留状态或由角色特定逻辑覆盖
        ///
        /// 推测用途：
        /// - 被抓取前的准备状态
        /// - 或某些特殊受击动作的状态标记
        /// - FLF 中也是空实现，表示所有逻辑都在帧数据中
        /// </summary>
        private bool State_Injured2(string eventType, object eventData)
        {
            // ✓ 无特殊事件处理（对应 FLF Line 1230-1235）
            // FLF 中也是空实现，所有逻辑由帧数据驱动
            return false;
        }

        /// <summary>
        /// 蓄力状态处理器 (state = 17)
        /// 用途：角色进行技能蓄力时的状态
        /// 出现次数：16
        /// </summary>
        private bool State_Charging(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 初始化蓄力状态
                    StateMem["chargeTime"] = 0;
                    StateMem["maxChargeTime"] = 60;  // 60帧 = 2秒（30fps）
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", "初始化蓄力 chargeTime=0, maxChargeTime=60");
                    return false;

                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 蓄力状态的帧处理
                    // 蓄力等级判定和特效播放由外部系统处理
                    return false;

                case "TU":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 蓄力时间更新
                    if (StateMem.ContainsKey("chargeTime"))
                    {
                        int chargeTime = (int)StateMem["chargeTime"];
                        int maxChargeTime = (int)StateMem["maxChargeTime"];

                        // 递增蓄力时间，但不超过上限
                        if (chargeTime < maxChargeTime)
                        {
                            StateMem["chargeTime"] = chargeTime + 1;
                            if (chargeTime % 10 == 0)  // 每10帧输出一次日志
                            {
                                Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", $"蓄力中 chargeTime={chargeTime}/{maxChargeTime}");
                            }
                        }
                    }
                    return false;

                case "combo":
                    // ✓ 蓄力中的输入处理
                    string comboKey = eventData as string;
                    Log.Info("[State {0}:{1}] Event={2}, Key={3}, Frame.D={4}", 17, "Charging", eventType, comboKey, CurrentFrameId);

                    // 任何按键输入都会结束蓄力状态
                    // 具体的技能释放逻辑由技能系统处理
                    if (!string.IsNullOrEmpty(comboKey))
                    {
                        int chargeTime = StateMem.ContainsKey("chargeTime") ? (int)StateMem["chargeTime"] : 0;
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", $"蓄力中断 按键={comboKey}, 蓄力时间={chargeTime}");
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 17, "Charging", LF2StandardFrames.Standing, "蓄力中断");
                        // 返回站立状态，让技能系统接管
                        TransitionToFrame(LF2StandardFrames.Standing, 10);
                        return true;
                    }
                    return false;

                case "state_exit":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 17, "Charging", eventType, CurrentFrameId);

                    // ✓ 清理蓄力状态内存
                    Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", "Clear charging state mem");
                    StateMem.Remove("chargeTime");
                    StateMem.Remove("maxChargeTime");
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// 燃烧状态处理器 (state = 18)
        /// 对应 FLF js:1242-1258
        ///
        /// 功能：处理燃烧效果
        /// 关键特性：
        /// 1. frame 事件：每帧创建燃烧特效（持续燃烧视觉效果）
        /// 2. fall_onto_ground 事件：落地瞬间创建燃烧效果
        /// 3. fell_onto_ground 事件：复用 State 12 的落地逻辑（弹起/躺地判定）
        /// 4. 关键帧：203-206（燃烧落地帧）
        ///
        /// 燃烧机制（来自 hit 函数）：
        /// - effectnum = 2/20/21/22/23: 火焰攻击
        /// - 转到帧203（燃烧状态）
        /// - 高级火焰（21/22/23）弱化投掷判定器
        /// - 燃烧状态防止急火击中（effectnum=20/21）
        /// - 燃烧状态21/22不会伤害队友
        /// </summary>
        private bool State_Burning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "持续燃烧 → 每帧创建燃烧特效");
                    // 每帧创建燃烧特效（FLF:1246-1249）
                    // TODO: 实现特效系统（ID 302，持续模式）
                    return false;

                case "fall_onto_ground":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "燃烧落地 → 创建落地燃烧特效");
                    // 落地时创建燃烧特效（FLF:1250-1252）
                    // TODO: 实现特效系统（ID 302，一次性模式）
                    return false;

                case "fell_onto_ground":
                    Log.Info("[State {0}:{1}] Event={2}, Frame.D={3}", 18, "Burning", eventType, CurrentFrameId);

                    Log.Info("[State {0}:{1}] -> Branch: {2}", 18, "Burning", "燃烧倒地 → 复用State 12落地逻辑");
                    // 复用State 12落地逻辑（FLF:1253-1256）
                    return State_Falling("fell_onto_ground", eventData);

                default:
                    return false;
            }
        }

        #endregion
    }
}
