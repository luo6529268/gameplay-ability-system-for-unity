using NTSD.Animation;
using NTSD.Input;
using NTSD.Simulation;
using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character : LF2LivingObject
    {
        internal bool ProcessReleaseInputCommand(NTSDInputCommand command)
        {
            int state = Frame?.D?.state ?? LF2States.Standing;
            switch (state)
            {
                case LF2States.Standing:
                    return ProcessStandingInputCommand(command);
                case LF2States.Walking:
                    return ProcessWalkingInputCommand(command);
                case LF2States.Running:
                    return ProcessRunningInputCommand(command);
                case LF2States.Jump:
                    return ProcessJumpInputCommand(command);
                case LF2States.Dash:
                    return ProcessDashInputCommand(command);
                case LF2States.Defending:
                    return ProcessDefendingInputCommand(command);
                case LF2States.Catching:
                    return ProcessCatchingInputCommand(ToLegacyInputCommand(command));
                case LF2States.Falling:
                    return ProcessFallingInputCommand(command);
                case LF2States.StopRunning:
                    return ProcessStopRunningInputCommand(command);
                case LF2States.Charging:
                    return ProcessChargingInputCommand(command);
                default:
                    return false;
            }
        }

        private bool ProcessReleaseInputCommand(string command)
        {
            return ProcessReleaseInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessStandingInputCommand(NTSDInputCommand command)
        {
            bool hasDx = Controller.IsLeft != Controller.IsRight;
            bool hasDz = Controller.IsUp != Controller.IsDown;
            if (hasDx || hasDz)
            {
                var characterData = _FrameDataWrapper?.characterData;
                if (characterData == null) return false;

                if (IsHeavyWeapon())
                {
                    if (hasDx) PS.vx = Dirh() * characterData.heavy_walking_speed;
                    PS.vz = Dirv() * characterData.heavy_walking_speedz;
                }
                else
                {
                    if (command != NTSDInputCommand.Jump)
                    {
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.WalkingStart, "direction input -> walking");
                        TransitionToFrame(LF2StandardFrames.WalkingStart, 5);
                    }

                    if (hasDx) PS.vx = Dirh() * characterData.walking_speed;
                    PS.vz = Dirv() * characterData.walking_speedz;
                }
            }

            switch (command)
            {
                case NTSDInputCommand.RunLeft:
                case NTSDInputCommand.RunRight:
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.RunningStart, "double direction -> running");
                    TransitionToFrame(IsHeavyWeapon() ? LF2StandardFrames.HeavyObjRun : LF2StandardFrames.RunningStart, LF2StateConstants.ComboTransitionWait);
                    StateReturnFrame = 1;
                    return true;

                case NTSDInputCommand.Defend:
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Defend, "defend input");
                    if (InputState != null && InputState.DefendLockActive) break;
                    if (IsHeavyWeapon())
                    {
                        StateReturnFrame = 1;
                        return true;
                    }

                    TransitionToFrame(LF2StandardFrames.Defend, LF2StateConstants.ComboTransitionWait);
                    StateReturnFrame = 1;
                    return true;

                case NTSDInputCommand.Jump:
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Jumping, "jump input");
                    if (IsHeavyWeapon())
                    {
                        if (!NTSDSpec.CanHeavyWeaponJump(ObjectId))
                        {
                            StateReturnFrame = 1;
                            return true;
                        }

                        TransitionToFrame(LF2StandardFrames.Jumping, LF2StateConstants.ComboTransitionWait);
                        return true;
                    }

                    TransitionToFrame(LF2StandardFrames.Jumping, LF2StateConstants.ComboTransitionWait);
                    StateReturnFrame = 1;
                    return true;

                case NTSDInputCommand.Attack:
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 0, "Standing", LF2StandardFrames.Punch, "attack input");
                    if (InputState != null && InputState.DefendLockActive) break;

                    if (HitConfirmEa > 0 && FrameCache.GetFrameDataById(LF2StandardFrames.SuperPunch) != null)
                    {
                        HitConfirmEa = 0;
                        TransitionToFrame(LF2StandardFrames.SuperPunch, LF2StateConstants.ComboTransitionWait);
                        StateReturnFrame = 1;
                        return true;
                    }

                    if (_heldWeapon != null)
                    {
                        if (IsHeavyWeapon())
                        {
                            TransitionToFrame(LF2StandardFrames.HeavyWeaponThw, LF2StateConstants.ComboTransitionWait);
                            StateReturnFrame = 1;
                            return true;
                        }

                        if (NTSDSpec.CanJustThrowWeapon(_heldWeapon.ObjectId))
                        {
                            TransitionToFrame(LF2StandardFrames.LightWeaponThw, LF2StateConstants.ComboTransitionWait);
                            StateReturnFrame = 1;
                            return true;
                        }

                        if (NTSDSpec.CanStandThrowWeapon(_heldWeapon.ObjectId))
                        {
                            TransitionToFrame(LF2StandardFrames.LightWeaponThw, LF2StateConstants.ComboTransitionWait);
                            StateReturnFrame = 1;
                            return true;
                        }

                        if (NTSDSpec.IsWeaponAttackable(_heldWeapon.ObjectId))
                        {
                            int normalWeaponAttack = Match.Rng.Next() < 0.5f ? LF2StandardFrames.NormalWeaponAtck : LF2StandardFrames.NormalWeaponAtck2;
                            TransitionToFrame(normalWeaponAttack, LF2StateConstants.ComboTransitionWait);
                            StateReturnFrame = 1;
                            return true;
                        }
                    }

                    int punchFrame = Match.Rng.Next() < 0.5f ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                    TransitionToFrame(punchFrame, LF2StateConstants.ComboTransitionWait);
                    return true;
            }

            return false;
        }

        private bool ProcessStandingInputCommand(string command)
        {
            return ProcessStandingInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessWalkingInputCommand(NTSDInputCommand command)
        {
            (int dx, int dz) = Controller.GetMoveInput();

            if (dx != 0 && dx != Dirh())
            {
                SwitchDir(PS.dir == "right" ? "left" : "right");
            }

            return command != NTSDInputCommand.None && ProcessStandingInputCommand(command);
        }

        private bool ProcessWalkingInputCommand(string command)
        {
            return ProcessWalkingInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessRunningInputCommand(NTSDInputCommand command)
        {
            if (command == NTSDInputCommand.None)
                return false;

            if (IsHorizontalCommand(command))
            {
                string inputDir = HorizontalCommandDirection(command);
                if (inputDir != PS.dir)
                {
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.StopRunning, "opposite direction -> stop running");
                    TransitionToFrame(IsHeavyWeapon() ? LF2StandardFrames.TreeJump2 : LF2StandardFrames.StopRunning, 10);
                    StateReturnFrame = 1;
                    return true;
                }
            }
            else if (command == NTSDInputCommand.Defend)
            {
                if (IsHeavyWeapon())
                {
                    StateReturnFrame = 1;
                    return true;
                }

                if (Catching != null && GetState() == LF2States.BeingCaught)
                {
                    TransitionToFrame(_caughtFront ? 45 : 55, 10);
                }
                else
                {
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", 102, "defend while running");
                    TransitionToFrame(102, 10);
                }

                return true;
            }
            else if (command == NTSDInputCommand.Jump)
            {
                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 2, "Running", LF2StandardFrames.DashForward, "jump while running");
                if (IsHeavyWeapon())
                {
                    if (!NTSDSpec.CanHeavyWeaponDash(ObjectId))
                    {
                        StateReturnFrame = 1;
                        return true;
                    }

                    TransitionToFrame(LF2StandardFrames.DashForward, 10);
                    StateReturnFrame = 1;
                    return true;
                }

                TransitionToFrame(LF2StandardFrames.DashForward, 10);
                StateReturnFrame = 1;
                return true;
            }
            else if (command == NTSDInputCommand.Attack)
            {
                if (_heldWeapon != null)
                {
                    if ((_heldWeapon as LF2WeaponBase)?.IsHeavy == true)
                    {
                        TransitionToFrame(LF2StandardFrames.HeavyWeaponThw, 10);
                        StateReturnFrame = 1;
                        return true;
                    }

                    bool hasDx = Controller.IsLeft != Controller.IsRight;
                    if (hasDx && NTSDSpec.CanRunThrowWeapon(_heldWeapon.ObjectId))
                    {
                        TransitionToFrame(LF2StandardFrames.LightWeaponThw, 10);
                        StateReturnFrame = 1;
                        return true;
                    }

                    if (NTSDSpec.IsWeaponAttackable(_heldWeapon.ObjectId))
                    {
                        TransitionToFrame(LF2StandardFrames.RunWeaponAtck, 10);
                        StateReturnFrame = 1;
                        return true;
                    }
                }

                TransitionToFrame(LF2StandardFrames.RunAttack, 10);
                StateReturnFrame = 1;
                return true;
            }

            return false;
        }

        private bool ProcessRunningInputCommand(string command)
        {
            return ProcessRunningInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessJumpInputCommand(NTSDInputCommand command)
        {
            if (command == NTSDInputCommand.Defend)
            {
                if (Catching != null && GetState() == LF2States.BeingCaught)
                {
                    TransitionToFrame(_caughtFront ? 30 : 52, 10);
                }
                else
                {
                    TransitionToFrame(LF2StandardFrames.JumpAttack, 10);
                }

                StateReturnFrame = 1;
                return true;
            }

            if ((command == NTSDInputCommand.Attack || Controller.IsAttack) && _jumpAttackLock <= 0)
            {
                if (Frame.N == LF2StandardFrames.JumpingAir)
                {
                    if (_heldWeapon != null)
                    {
                        bool hasDx = Controller.IsLeft != Controller.IsRight;
                        bool attackable = NTSDSpec.IsWeaponAttackable(_heldWeapon.ObjectId);
                        if (hasDx && attackable)
                        {
                            TransitionToFrame(LF2StandardFrames.SkyLgtWpThw, 10);
                        }
                        else if (attackable)
                        {
                            TransitionToFrame(LF2StandardFrames.JumpWeaponAtck, 10);
                        }
                    }
                    else
                    {
                        Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 4, "Jump", LF2StandardFrames.JumpAttack, "jump attack");
                        TransitionToFrame(LF2StandardFrames.JumpAttack, 10);
                    }

                    StateReturnFrame = 1;
                    return true;
                }
            }

            return false;
        }

        private bool ProcessJumpInputCommand(string command)
        {
            return ProcessJumpInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessDashInputCommand(NTSDInputCommand command)
        {
            if (command == NTSDInputCommand.Attack || Controller.IsAttack)
            {
                if (Dirh() == (PS.vx > 0 ? 1 : -1))
                {
                    if (_heldWeapon != null && NTSDSpec.IsWeaponAttackable(_heldWeapon.ObjectId))
                    {
                        TransitionToFrame(LF2StandardFrames.DashWeaponAtck, 10);
                    }
                    else
                    {
                        TransitionToFrame(LF2StandardFrames.DashAttack, 10);
                    }
                }

                if (command == NTSDInputCommand.Attack)
                {
                    StateReturnFrame = 1;
                    return true;
                }
            }

            if (command == NTSDInputCommand.Defend)
            {
                TransitionToFrame(LF2StandardFrames.DashAttack, 10);
                StateReturnFrame = 1;
                return true;
            }

            if (command == NTSDInputCommand.Up || command == NTSDInputCommand.Jump)
            {
                var (dx, _) = Controller.GetMoveInput();
                int upFrame = (dx != 0 && (dx > 0 ? 1 : -1) == Dirh())
                    ? LF2StandardFrames.DashForward
                    : LF2StandardFrames.DashForward2;
                TransitionToFrame(upFrame, 10);
                StateReturnFrame = 1;
                return true;
            }

            if (IsHorizontalCommand(command))
            {
                string inputDir = HorizontalCommandDirection(command);
                if (inputDir != PS.dir)
                {
                    if (Dirh() == (PS.vx > 0 ? 1 : -1))
                    {
                        if (Frame.N == LF2StandardFrames.DashForward)
                            TransitionToFrame(LF2StandardFrames.DashForward2, 0);

                        if (Frame.N == LF2StandardFrames.DashBack)
                            TransitionToFrame(LF2StandardFrames.DashBack2, 0);

                        SwitchDir(inputDir);
                    }
                    else
                    {
                        if (Frame.N == LF2StandardFrames.DashForward2)
                            TransitionToFrame(LF2StandardFrames.DashForward, 0);

                        if (Frame.N == LF2StandardFrames.DashBack2)
                            TransitionToFrame(LF2StandardFrames.DashBack, 0);

                        SwitchDir(inputDir);
                    }

                    return true;
                }
            }

            return false;
        }

        private bool ProcessDashInputCommand(string command)
        {
            return ProcessDashInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessDefendingInputCommand(NTSDInputCommand command)
        {
            if (Frame.N == LF2StandardFrames.Defend)
            {
                if (command == NTSDInputCommand.Left || command == NTSDInputCommand.RunLeft) SwitchDir("left");
                else if (command == NTSDInputCommand.Right || command == NTSDInputCommand.RunRight) SwitchDir("right");
            }

            return false;
        }

        private bool ProcessDefendingInputCommand(string command)
        {
            return ProcessDefendingInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessFallingInputCommand(NTSDInputCommand command)
        {
            int frameId = Frame.N;
            if ((frameId == LF2StandardFrames.FallingFront2 || frameId == LF2StandardFrames.FallingBack2) && command == NTSDInputCommand.Jump)
            {
                if (HitCounters.Fall >= 0 && Health.HP > 0)
                {
                    int rowingFrame = frameId == LF2StandardFrames.FallingFront2
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
        }

        private bool ProcessFallingInputCommand(string command)
        {
            return ProcessFallingInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessStopRunningInputCommand(NTSDInputCommand command)
        {
            if (Frame.N != LF2StandardFrames.Crouch)
                return false;

            if (command == NTSDInputCommand.None)
                return false;

            if (command == NTSDInputCommand.Attack)
            {
                TransitionToFrame(LF2StandardFrames.Rowing2, 10);
                return true;
            }

            if (command == NTSDInputCommand.Defend)
            {
                Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", 102, "defend from crouch");
                TransitionToFrame(LF2StandardFrames.Rowing2, 10);
                return true;
            }

            if (command == NTSDInputCommand.Jump)
            {
                var (dx, _) = Controller.GetMoveInput();
                if (dx != 0)
                {
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward, "directional crouch jump");
                    TransitionToFrame(LF2StandardFrames.DashForward, 10);
                    SwitchDir(dx == 1 ? DIRECTION.RIGHT : DIRECTION.LEFT);
                }
                else if (PS.vx == 0)
                {
                    Trans.IncWait(2, 10, 99);
                    Trans.SetNext(LF2StandardFrames.Jumping, 10);
                }
                else if ((PS.vx > 0 ? 1 : -1) == Dirh())
                {
                    TransitionToFrame(LF2StandardFrames.DashForward, 10);
                }
                else
                {
                    Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 15, "Mixed", LF2StandardFrames.DashForward2, "reverse crouch jump");
                    TransitionToFrame(LF2StandardFrames.DashForward2, 10);
                }

                return true;
            }

            return false;
        }

        private bool ProcessStopRunningInputCommand(string command)
        {
            return ProcessStopRunningInputCommand(ParseLegacyInputCommand(command));
        }

        private bool ProcessChargingInputCommand(NTSDInputCommand command)
        {
            if (command == NTSDInputCommand.None)
                return false;

            Log.Info("[State {0}:{1}] -> Branch: {2}", 17, "Charging", $"charge interrupted key={command}");
            TransitionToFrame(LF2StandardFrames.Standing, 10);
            return true;
        }

        private bool ProcessChargingInputCommand(string command)
        {
            return ProcessChargingInputCommand(ParseLegacyInputCommand(command));
        }

        internal bool CanTriggerReleaseInputFrame()
        {
            return true;
        }

        internal void MarkInputFrameConsumed()
        {
            StateReturnFrame = 1;
        }

        private static bool IsHorizontalCommand(NTSDInputCommand command)
        {
            return command == NTSDInputCommand.Left
                || command == NTSDInputCommand.Right
                || command == NTSDInputCommand.RunLeft
                || command == NTSDInputCommand.RunRight;
        }

        private static string HorizontalCommandDirection(NTSDInputCommand command)
        {
            return command == NTSDInputCommand.Left || command == NTSDInputCommand.RunLeft
                ? "left"
                : "right";
        }

        private static NTSDInputCommand ParseLegacyInputCommand(string command)
        {
            return command switch
            {
                "left" => NTSDInputCommand.Left,
                "right" => NTSDInputCommand.Right,
                "up" => NTSDInputCommand.Up,
                "down" => NTSDInputCommand.Down,
                "def" => NTSDInputCommand.Defend,
                "jump" => NTSDInputCommand.Jump,
                "att" => NTSDInputCommand.Attack,
                "left-left" => NTSDInputCommand.RunLeft,
                "right-right" => NTSDInputCommand.RunRight,
                _ => NTSDInputCommand.None,
            };
        }

        private static string ToLegacyInputCommand(NTSDInputCommand command)
        {
            return command switch
            {
                NTSDInputCommand.Left => "left",
                NTSDInputCommand.Right => "right",
                NTSDInputCommand.Up => "up",
                NTSDInputCommand.Down => "down",
                NTSDInputCommand.Defend => "def",
                NTSDInputCommand.Jump => "jump",
                NTSDInputCommand.Attack => "att",
                NTSDInputCommand.RunLeft => "left-left",
                NTSDInputCommand.RunRight => "right-right",
                _ => null,
            };
        }
    }
}
