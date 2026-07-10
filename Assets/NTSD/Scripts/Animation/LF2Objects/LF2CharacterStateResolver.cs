using NTSD.Animation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 角色状态机处理器。
    /// 
    /// 当角色处于站立、行走、跑动、跳跃、冲刺等“常规动作状态”时，
    /// 对应的状态事件大多都由这个类处理。
    /// 
    /// 如果以后你想查“为什么角色这一帧会切到某个移动帧、为什么跑起来/跳起来”，
    /// 这里通常就是第一站。
    /// </summary>
    internal sealed class LF2CharacterStateResolver
    {
        private readonly LF2Character _character;

        public LF2CharacterStateResolver(LF2Character character)
        {
            _character = character;
        }

        public bool StateStanding(string eventType, object eventData)
        {
            // 站立状态比较简单，主要在 frame 阶段决定是否切到重武器专用移动帧。
            switch (eventType)
            {
                case "frame":
                    if (_character.IsHeavyWeapon())
                        SetMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);
                    break;
            }

            return false;
        }

        public bool StateWalking(string eventType, object eventData)
        {
            // 行走状态最重要的事情是：
            // 根据方向输入、重武器状态、动画计数，决定当前该显示哪一张走路帧，并写入移动速度。
            switch (eventType)
            {
                case "frame":
                    ApplyWalkRunFrame(_character.IsHeavyWeapon());
                    return false;

                case "TU":
                    return false;

                case "state_entry":
                    _character.SetAnimCounterInternal(0);
                    _character.SetAnimSubInternal(0);
                    return false;

                default:
                    return false;
            }
        }

        public bool StateRunning(string eventType, object eventData)
        {
            // 跑动状态和行走类似，但速度和动画切换规则不同。
            switch (eventType)
            {
                case "frame":
                    ApplyRunningFrame();
                    return false;

                case "TU":
                    return false;

                default:
                    return false;
            }
        }

        public bool StateJump(string eventType, object eventData)
        {
            // 跳跃状态主要负责两类事情：
            // 1. 起跳瞬间给速度。
            // 2. 维护空中运动本身。
            switch (eventType)
            {
                case "frame":
                    if (_character.Frame.N == LF2StandardFrames.JumpingAir &&
                        _character.Frame.PN == LF2StandardFrames.JumpingUp)
                    {
                        ApplyJumpStartVelocity();
                    }
                    return false;

                default:
                    return false;
            }
        }

        public bool StateDash(string eventType, object eventData)
        {
            // 冲刺状态主要在 frame 阶段决定冲刺方向和对应帧。
            switch (eventType)
            {
                case "frame":
                    ApplyDashFrame();
                    return false;

                case "state_entry":
                    return false;

                default:
                    return false;
            }
        }

        public bool StateStopRunning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    int frameId = _character.Frame.N;

                    if (frameId == LF2StandardFrames.TreeJump2)
                    {
                        if (_character.IsHeavyWeapon())
                            SetMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);
                        break;
                    }

                    if (frameId == LF2StandardFrames.Crouch)
                    {
                        _character.Trans.IncWait(-1);
                        break;
                    }

                    if (frameId == LF2StandardFrames.Crouch2)
                    {
                        switch (_character.Frame.PN)
                        {
                            case LF2StandardFrames.Rowing5:
                                CharacterMechanics.UnitFriction(_character.Runtime);
                                break;

                            case LF2StandardFrames.DashBack:
                            case LF2StandardFrames.DashAttack:
                            case LF2StandardFrames.DashAttack + 1:
                            case LF2StandardFrames.DashAttack + 2:
                                _character.Trans.IncWait(-1);
                                break;
                        }
                    }
                    else if (frameId == LF2StandardFrames.SkyLgtWpThw3)
                    {
                        LF2FrameData frameData = _character.Frame.D;
                        if (frameData.next == LF2StandardFrames.LoopToStart && _character.Runtime.Y < 0)
                            _character.Trans.SetNext(LF2StandardFrames.JumpingAir);
                    }
                    break;
            }

            return false;
        }

        public void ApplyWalkRunFrame(bool heavy)
        {
            // 这是行走/慢跑动画与速度写入的核心函数。
            // 它会同时处理：
            // 1. 面朝方向切换
            // 2. 左右/上下输入带来的速度
            // 3. 行走动画帧循环
            // 4. 是否从走路衔接到跑动
            LF2CharacterData characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null || !_character.HasBoundControllerInternal())
                return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1)
                rate = 1;

            int animSub = _character.Runtime.AnimSub;
            if (animSub > 0)
                _character.Runtime.AnimSub--;
            else if (animSub < 0)
                _character.Runtime.AnimSub++;

            bool handled = false;
            bool vxSet = false;
            int frameBase = heavy ? LF2StandardFrames.HeavyObjWalk0 : LF2StandardFrames.WalkingStart;
            int turnFrame = heavy ? LF2StandardFrames.HeavyObjRun : LF2StandardFrames.RunningStart;
            float walkSpeed = heavy ? characterData.heavy_walking_speed : characterData.walking_speed;
            float walkSpeedZ = heavy ? characterData.heavy_walking_speedz : characterData.walking_speedz;

            if (_character.IsCurrentRightPressedInternal() && !_character.IsCurrentLeftPressedInternal() && _character.Runtime.Y == 0f)
            {
                handled = true;
                if (_character.Runtime.Dir == "left")
                    _character.Runtime.AnimSub = 0;
                _character.SwitchDir("right");
                StepWalkAnimation(rate, frameBase);
                _character.Runtime.Vx = walkSpeed;
                vxSet = true;
                if (!_character.WasRightPressedPreviousFrameInternal())
                    _character.Runtime.AnimSub += 10;
                if (_character.Runtime.AnimSub >= 11)
                {
                    SetMoveFrameDirect(turnFrame);
                    _character.SetAnimCounterInternal(0);
                    _character.SetAnimSubInternal(0);
                }
            }

            if (!handled && _character.IsCurrentLeftPressedInternal() && !_character.IsCurrentRightPressedInternal() && _character.Runtime.Y == 0f)
            {
                handled = true;
                if (_character.Runtime.Dir == "right")
                    _character.Runtime.AnimSub = 0;
                _character.SwitchDir("left");
                StepWalkAnimation(rate, frameBase);
                _character.Runtime.Vx = -walkSpeed;
                vxSet = true;
                if (!_character.WasLeftPressedPreviousFrameInternal())
                    _character.Runtime.AnimSub -= 10;
                if (_character.Runtime.AnimSub <= -11)
                {
                    SetMoveFrameDirect(turnFrame);
                    _character.SetAnimCounterInternal(0);
                    _character.SetAnimSubInternal(0);
                }
            }

            if (_character.IsCurrentUpPressedInternal() && !_character.IsCurrentDownPressedInternal() && _character.Runtime.Y == 0f)
            {
                if (!vxSet)
                    StepWalkAnimation(rate, frameBase);
                _character.Runtime.Vz = -walkSpeedZ;
                _character.Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }

            if (_character.IsCurrentDownPressedInternal() && !_character.IsCurrentUpPressedInternal() && _character.Runtime.Y == 0f)
            {
                if (!vxSet)
                    StepWalkAnimation(rate, frameBase);
                _character.Runtime.Vz = walkSpeedZ;
                _character.Runtime.Vx *= 0.7142857142857143; // P0-f-2b B2-3b: VALUE-BUG 5f/7f→0.7142857142857143 (baseline InputRuntime.cs Vx*=5.0/7.0)
            }
        }

        public void ApplyRunLane(float speedZ)
        {
            if (_character.IsCurrentUpPressedInternal() && !_character.IsCurrentDownPressedInternal())
            {
                _character.Runtime.Vz = -speedZ;
                _character.Runtime.Vx *= 5f / 6f;
            }
            else if (_character.IsCurrentDownPressedInternal() && !_character.IsCurrentUpPressedInternal())
            {
                _character.Runtime.Vz = speedZ;
                _character.Runtime.Vx *= 5f / 6f;
            }
        }

        public void ApplyDashFrame()
        {
            if (!_character.HasBoundControllerInternal())
                return;

            if (_character.IsCurrentRightPressedInternal() && !_character.IsCurrentLeftPressedInternal())
                _character.SwitchDir("right");
            else if (_character.IsCurrentLeftPressedInternal() && !_character.IsCurrentRightPressedInternal())
                _character.SwitchDir("left");

            bool facingRight = _character.Runtime.Dir == "right";
            if (facingRight)
            {
                if (_character.Frame.N != LF2StandardFrames.DashBack2 && _character.Runtime.Vx < 0f)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (_character.Runtime.Vx > 0f && _character.Frame.N != LF2StandardFrames.DashBack)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward);
            }
            else
            {
                if (_character.Runtime.Vx > 0f && _character.Frame.N != LF2StandardFrames.DashBack2)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (_character.Runtime.Vx < 0f && _character.Frame.N != LF2StandardFrames.DashBack)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward);
            }
        }

        public void ApplyJumpStartVelocity()
        {
            LF2CharacterData characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null || !_character.HasBoundControllerInternal())
                return;

            _character.Runtime.Vy = characterData.jump_height;
            if (_character.IsCurrentRightPressedInternal() && !_character.IsCurrentLeftPressedInternal())
            {
                _character.Runtime.Vx = characterData.jump_distance;
                _character.SwitchDir("right");
            }
            else if (_character.IsCurrentLeftPressedInternal() && !_character.IsCurrentRightPressedInternal())
            {
                _character.Runtime.Vx = -characterData.jump_distance;
                _character.SwitchDir("left");
            }

            if (_character.IsCurrentUpPressedInternal() && !_character.IsCurrentDownPressedInternal())
                _character.Runtime.Vz = -characterData.jump_distancez;
            else if (_character.IsCurrentDownPressedInternal() && !_character.IsCurrentUpPressedInternal())
                _character.Runtime.Vz = characterData.jump_distancez;
        }

        public void ApplyDashStartVelocity(bool forward)
        {
            LF2CharacterData characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null || !_character.HasBoundControllerInternal())
                return;

            float sign = _character.Runtime.Dir == "right" ? 1f : -1f;
            _character.Runtime.Vx = sign * characterData.dash_distance * (forward ? 1f : -1f);
            _character.Runtime.Vy = characterData.dash_height;
            if (_character.IsCurrentUpPressedInternal() && !_character.IsCurrentDownPressedInternal())
                _character.Runtime.Vz = -characterData.dash_distancez;
            else if (_character.IsCurrentDownPressedInternal() && !_character.IsCurrentUpPressedInternal())
                _character.Runtime.Vz = characterData.dash_distancez;
            _character.SetAnimSubInternal(0);
        }

        public void SetMoveFrameDirect(int frameId)
        {
            LF2FrameData targetFrame = _character.FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null || _character.Frame == null)
                return;

            _character.Frame.PN = _character.Frame.N;
            _character.Frame.N = frameId;
            _character.Frame.D = targetFrame;
            _character.Trans?.SyncDirectFrameData(_character.Frame.D.wait, _character.Frame.D.next);
            _character.Runtime.NextFrame = _character.Frame.D.next;
        }

        private void StepWalkAnimation(int rate, int frameBase)
        {
            int animCounter = _character.GetAnimCounterInternal();
            animCounter = (animCounter + 1) % (rate * 6);
            _character.SetAnimCounterInternal(animCounter);
            int fi = animCounter / rate;
            int frameId = fi < 4 ? frameBase + fi : frameBase + (6 - fi);
            SetMoveFrameDirect(frameId);
        }

        private void ApplyRunningFrame()
        {
            LF2CharacterData characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null || !_character.HasBoundControllerInternal())
                return;

            int runRate = characterData.running_frame_rate;
            if (runRate < 1)
                runRate = 1;

            _character.AttackingCounter = 0;
            int animCounter = _character.GetAnimCounterInternal();
            animCounter = (animCounter + 1) % (runRate * 4);
            _character.SetAnimCounterInternal(animCounter);
            int fi = animCounter / runRate;

            bool heavy = _character.IsHeavyWeapon();
            if (heavy)
            {
                SetMoveFrameDirect(fi < 3 ? LF2StandardFrames.HeavyObjRun + fi : LF2StandardFrames.TreeJump0);
                _character.Runtime.Vx = _character.Runtime.Dir == "right" ? characterData.heavy_running_speed : -characterData.heavy_running_speed;
                if ((_character.Runtime.Dir == "right" && _character.IsCurrentLeftPressedInternal()) ||
                    (_character.Runtime.Dir == "left" && _character.IsCurrentRightPressedInternal()))
                {
                    SetMoveFrameDirect(LF2StandardFrames.TreeJump2);
                }

                ApplyRunLane(characterData.heavy_running_speedz);
                return;
            }

            SetMoveFrameDirect(fi < 3 ? LF2StandardFrames.RunningStart + fi : LF2StandardFrames.Running1);
            _character.Runtime.Vx = _character.Runtime.Dir == "right" ? characterData.running_speed : -characterData.running_speed;
            if ((_character.Runtime.Dir == "right" && _character.IsCurrentLeftPressedInternal()) ||
                (_character.Runtime.Dir == "left" && _character.IsCurrentRightPressedInternal()))
            {
                SetMoveFrameDirect(LF2StandardFrames.StopRunning);
            }

            ApplyRunLane(characterData.running_speedz);
        }
    }
}
