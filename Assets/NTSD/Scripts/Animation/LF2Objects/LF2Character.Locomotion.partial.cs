using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character
    {
        private bool State_Standing(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    if (IsHeavyWeapon())
                        TransitionToFrame(LF2StandardFrames.HeavyObjWalk0);
                    break;

                case "combo":
                    return ProcessStandingInputCommand(eventData as string);
            }

            return false;
        }

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
                    {
                        FrameAniOscillate(LF2StandardFrames.WalkingStart, LF2StandardFrames.WalkingEnd);
                    }

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
                    return false;
                }

                case "state_entry":
                    Trans.SetWait(0);
                    return false;

                case "combo":
                    return ProcessWalkingInputCommand(eventData as string);

                default:
                    return false;
            }
        }

        private bool State_Running(string eventType, object eventData)
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
                    return false;
                }

                case "combo":
                    return ProcessRunningInputCommand(eventData as string);

                default:
                    return false;
            }
        }

        private bool State_Jump(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    _jumpFrameTU = true;
                    if (Frame.PN == LF2StandardFrames.JumpAttack ||
                        Frame.PN == LF2StandardFrames.JumpAttack + 1)
                    {
                        _jumpAttackLock = 2;
                    }
                    return false;

                case "TU":
                    if (_jumpFrameTU)
                    {
                        _jumpFrameTU = false;
                        if (Frame.N == LF2StandardFrames.JumpingAir &&
                            Frame.PN == LF2StandardFrames.JumpingUp)
                        {
                            var (dx, dz) = Controller.GetMoveInput();
                            var characterData = _FrameDataWrapper?.characterData;
                            if (characterData == null) return false;

                            PS.vx = dx * characterData.jump_distance;
                            PS.vz = Dirv() * characterData.jump_distancez;
                            PS.vy = characterData.jump_height;
                        }
                    }

                    if (_jumpAttackLock > 0)
                        _jumpAttackLock--;

                    return false;

                case "combo":
                    return ProcessJumpInputCommand(eventData as string);
            }

            return false;
        }

        private bool State_Dash(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "state_entry":
                    if ((Frame.PN >= LF2StandardFrames.RunningStart &&
                         Frame.PN <= LF2StandardFrames.RunningEnd) ||
                        Frame.PN == LF2StandardFrames.Crouch)
                    {
                        var characterData = _FrameDataWrapper?.characterData;
                        if (characterData == null) return false;

                        PS.vx = Dirh() * characterData.dash_distance * (Frame.N == LF2StandardFrames.DashForward ? 1 : -1);
                        PS.vz = Dirv() * characterData.dash_distancez;
                        PS.vy = characterData.dash_height;
                    }
                    return false;

                case "combo":
                    return ProcessDashInputCommand(eventData as string);
            }

            return false;
        }

        private bool State_StopRunning(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    Log.Info("[State {0}:{1}] Event={2}", 15, "Mixed", eventType);
                    int frameId = Frame.N;

                    if (frameId == LF2StandardFrames.TreeJump2)
                    {
                        if (IsHeavyWeapon())
                            Trans.SetNext(LF2StandardFrames.HeavyObjWalk0);
                        break;
                    }
                    else if (frameId == LF2StandardFrames.Crouch)
                    {
                        Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧215 蹲下 → 减少等待时间");
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
                            Log.Info("[State {0}:{1}] -> Branch: {2}", 15, "Mixed", "帧54 空中轻武器投掷结束 → 返回跳跃");
                            Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.JumpingAir, "空中投掷完成");
                            Trans.SetNext(LF2StandardFrames.JumpingAir);
                        }
                    }
                    else if (frameId == LF2StandardFrames.Disappear)
                    {
                    }
                    break;

                case "combo":
                    return ProcessStopRunningInputCommand(eventData as string);
            }

            return false;
        }
    }
}
