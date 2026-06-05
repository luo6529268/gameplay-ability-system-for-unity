namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        private int AnimCounter { get => Runtime.AnimCounter; set => Runtime.AnimCounter = value; }
        private int AnimSub { get => Runtime.AnimSub; set => Runtime.AnimSub = value; }

        private bool State_Standing(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    if (IsHeavyWeapon())
                        SetMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);
                    break;

            }

            return false;
        }

        private bool State_Walking(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    ApplyWalkRunFrame(heavy: IsHeavyWeapon());
                    return false;

                case "TU":
                {
                    return false;
                }

                case "state_entry":
                    AnimCounter = 0;
                    AnimSub = 0;
                    return false;

                default:
                    return false;
            }
        }

        private bool State_Running(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    ApplyRunningFrame();
                    return false;

                case "TU":
                {
                    return false;
                }

                default:
                    return false;
            }
        }

        private bool State_Jump(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    if (Frame.N == LF2StandardFrames.JumpingAir &&
                        Frame.PN == LF2StandardFrames.JumpingUp)
                    {
                        ApplyJumpStartVelocity();
                    }

                    if (Frame.PN == LF2StandardFrames.JumpAttack ||
                        Frame.PN == LF2StandardFrames.JumpAttack + 1)
                    {
                        JumpAttackLock = 2;
                    }
                    return false;

                case "TU":
                    if (JumpAttackLock > 0)
                        JumpAttackLock--;

                    return false;

            }

            return false;
        }

        private bool State_Dash(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    ApplyDashFrame();
                    return false;

                case "state_entry":
                    return false;

            }

            return false;
        }

        private bool State_StopRunning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    int frameId = Frame.N;

                    if (frameId == LF2StandardFrames.TreeJump2)
                    {
                        if (IsHeavyWeapon())
                            SetMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);
                        break;
                    }
                    else if (frameId == LF2StandardFrames.Crouch)
                    {
                        Trans.IncWait(-1);
                        break;
                    }
                    else if (frameId == LF2StandardFrames.Crouch2)
                    {
                        switch (Frame.PN)
                        {
                            case LF2StandardFrames.Rowing5:
                                CharacterMechanics.UnitFriction(PS);
                                break;

                            case LF2StandardFrames.DashBack:
                            case LF2StandardFrames.DashAttack:
                            case LF2StandardFrames.DashAttack + 1:
                            case LF2StandardFrames.DashAttack + 2:
                                Trans.IncWait(-1);
                                break;
                        }
                    }
                    else if (frameId == LF2StandardFrames.SkyLgtWpThw3)
                    {
                        var D = Frame.D;
                        if (D.next == LF2StandardFrames.LoopToStart && PS.y < 0)
                        {
                            Trans.SetNext(LF2StandardFrames.JumpingAir);
                        }
                    }
                    else if (frameId == LF2StandardFrames.Disappear)
                    {
                    }
                    break;

            }

            return false;
        }

        private void ApplyWalkRunFrame(bool heavy)
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null || PS == null || Controller == null) return;

            int rate = characterData.walking_frame_rate;
            if (rate < 1) rate = 1;

            if (AnimSub > 0) AnimSub--;
            else if (AnimSub < 0) AnimSub++;

            bool handled = false;
            bool vxSet = false;
            int frameBase = heavy ? LF2StandardFrames.HeavyObjWalk0 : LF2StandardFrames.WalkingStart;
            int turnFrame = heavy ? LF2StandardFrames.HeavyObjRun : LF2StandardFrames.RunningStart;
            float walkSpeed = heavy ? characterData.heavy_walking_speed : characterData.walking_speed;
            float walkSpeedZ = heavy ? characterData.heavy_walking_speedz : characterData.walking_speedz;

            if (Controller.IsRight && !Controller.IsLeft && PS.y == 0f)
            {
                handled = true;
                if (PS.dir == "left") AnimSub = 0;
                SwitchDir("right");
                StepWalkAnimation(rate, frameBase);
                PS.vx = walkSpeed;
                vxSet = true;
                if (InputState?.PreviousRight == false) AnimSub += 10;
                if (AnimSub >= 11)
                {
                    SetMoveFrameDirect(turnFrame);
                    AnimCounter = 0;
                    AnimSub = 0;
                }
            }

            if (!handled && Controller.IsLeft && !Controller.IsRight && PS.y == 0f)
            {
                handled = true;
                if (PS.dir == "right") AnimSub = 0;
                SwitchDir("left");
                StepWalkAnimation(rate, frameBase);
                PS.vx = -walkSpeed;
                vxSet = true;
                if (InputState?.PreviousLeft == false) AnimSub -= 10;
                if (AnimSub <= -11)
                {
                    SetMoveFrameDirect(turnFrame);
                    AnimCounter = 0;
                    AnimSub = 0;
                }
            }

            if (Controller.IsUp && !Controller.IsDown && PS.y == 0f)
            {
                if (!vxSet)
                    StepWalkAnimation(rate, frameBase);
                PS.vz = -walkSpeedZ;
                PS.vx *= 5f / 7f;
            }

            if (Controller.IsDown && !Controller.IsUp && PS.y == 0f)
            {
                if (!vxSet)
                    StepWalkAnimation(rate, frameBase);
                PS.vz = walkSpeedZ;
                PS.vx *= 5f / 7f;
            }
        }

        private void StepWalkAnimation(int rate, int frameBase)
        {
            AnimCounter = (AnimCounter + 1) % (rate * 6);
            int fi = AnimCounter / rate;
            int frameId = fi < 4 ? frameBase + fi : frameBase + (6 - fi);
            SetMoveFrameDirect(frameId);
        }

        private void ApplyRunningFrame()
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null || PS == null || Controller == null) return;

            int runRate = characterData.running_frame_rate;
            if (runRate < 1) runRate = 1;

            AttackingCounter = 0;
            AnimCounter = (AnimCounter + 1) % (runRate * 4);
            int fi = AnimCounter / runRate;

            bool heavy = IsHeavyWeapon();
            if (heavy)
            {
                SetMoveFrameDirect(fi < 3 ? LF2StandardFrames.HeavyObjRun + fi : LF2StandardFrames.TreeJump0);
                PS.vx = PS.dir == "right" ? characterData.heavy_running_speed : -characterData.heavy_running_speed;
                if ((PS.dir == "right" && Controller.IsLeft) || (PS.dir == "left" && Controller.IsRight))
                    SetMoveFrameDirect(LF2StandardFrames.TreeJump2);
                ApplyRunLane(characterData.heavy_running_speedz);
                return;
            }

            SetMoveFrameDirect(fi < 3 ? LF2StandardFrames.RunningStart + fi : LF2StandardFrames.Running1);
            PS.vx = PS.dir == "right" ? characterData.running_speed : -characterData.running_speed;
            if ((PS.dir == "right" && Controller.IsLeft) || (PS.dir == "left" && Controller.IsRight))
                SetMoveFrameDirect(LF2StandardFrames.StopRunning);
            ApplyRunLane(characterData.running_speedz);
        }

        private void ApplyRunLane(float speedZ)
        {
            if (Controller.IsUp && !Controller.IsDown)
            {
                PS.vz = -speedZ;
                PS.vx *= 5f / 6f;
            }
            else if (Controller.IsDown && !Controller.IsUp)
            {
                PS.vz = speedZ;
                PS.vx *= 5f / 6f;
            }
        }

        private void ApplyDashFrame()
        {
            if (PS == null || Controller == null) return;

            if (Controller.IsRight && !Controller.IsLeft) SwitchDir("right");
            else if (Controller.IsLeft && !Controller.IsRight) SwitchDir("left");

            bool facingRight = PS.dir == "right";
            if (facingRight)
            {
                if (Frame.N != LF2StandardFrames.DashBack2 && PS.vx < 0f)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (PS.vx > 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward);
            }
            else
            {
                if (PS.vx > 0f && Frame.N != LF2StandardFrames.DashBack2)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward2);
                else if (PS.vx < 0f && Frame.N != LF2StandardFrames.DashBack)
                    SetMoveFrameDirect(LF2StandardFrames.DashForward);
            }
        }

        private void ApplyJumpStartVelocity()
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null || PS == null || Controller == null) return;

            PS.vy = characterData.jump_height;
            if (Controller.IsRight && !Controller.IsLeft)
            {
                PS.vx = characterData.jump_distance;
                SwitchDir("right");
            }
            else if (Controller.IsLeft && !Controller.IsRight)
            {
                PS.vx = -characterData.jump_distance;
                SwitchDir("left");
            }

            if (Controller.IsUp && !Controller.IsDown)
                PS.vz = -characterData.jump_distancez;
            else if (Controller.IsDown && !Controller.IsUp)
                PS.vz = characterData.jump_distancez;
        }

        private void ApplyDashStartVelocity(bool forward)
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null || PS == null || Controller == null) return;

            float sign = PS.dir == "right" ? 1f : -1f;
            PS.vx = sign * characterData.dash_distance * (forward ? 1f : -1f);
            PS.vy = characterData.dash_height;
            if (Controller.IsUp && !Controller.IsDown)
                PS.vz = -characterData.dash_distancez;
            else if (Controller.IsDown && !Controller.IsUp)
                PS.vz = characterData.dash_distancez;
            AnimSub = 0;
        }

        private void SetMoveFrameDirect(int frameId)
        {
            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null || Frame == null) return;

            Frame.PN = Frame.N;
            Frame.N = frameId;
            Frame.D = targetFrame;
            if (Frame.D.pic >= 0)
                Sprite?.ShowPic(Frame.D.pic);
            Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            Runtime.NextFrame = Frame.D.next;
        }
    }
}
