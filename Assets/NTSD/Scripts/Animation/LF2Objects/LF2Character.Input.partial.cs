using NTSD.Animation;
using NTSD.Input;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    public partial class LF2Character : LF2LivingObject
    {
        internal bool ProcessReleaseInput()
        {
            if (Frame?.D == null || Controller == null || PS == null)
                return false;

            if (Frame.N == LF2StandardFrames.Defend)
            {
                if (Controller.IsRight) SwitchDir("right");
                if (Controller.IsLeft) SwitchDir("left");
            }

            ApplyVerticalInputForSpecialStates();

            int state = Frame.D.state;
            bool handled = false;

            if (IsHeavyWeapon() && (state == LF2States.Standing || state == LF2States.Walking))
            {
                ProcessHeavyWalkInput();
                return true;
            }

            if (Frame.N == LF2StandardFrames.Crouch)
                return ProcessCrouchInput();

            if (ProcessDefensiveRecoveryInput())
                return true;

            switch (state)
            {
                case LF2States.Standing:
                case LF2States.Walking:
                    ApplyWalkRunFrame(heavy: false);
                    handled = ProcessStandingActions();
                    break;

                case LF2States.Running:
                    handled = ProcessRunningInput();
                    break;

                case LF2States.Jump:
                    if (PS.y < 0f)
                        handled = ProcessJumpingInput();
                    break;

                case LF2States.Dash:
                    handled = ProcessDashInput();
                    break;

                case LF2States.Defending:
                    handled = ProcessDefendingInput();
                    break;

                case LF2States.Catching:
                    handled = ProcessCatchingInput();
                    break;
            }

            return handled;
        }

        private bool ProcessStandingActions()
        {
            bool handled = false;
            int linkState = Runtime.LinkState;

            if (Controller.IsAttack && (InputState?.AttackCooldown ?? 0) > 0)
            {
                handled = true;
                AnimSub = 0;
                AttackingCounter = 0;

                if (!HasHeldObject() || linkState == 0)
                {
                    if (HitConfirmEa > 0 && FrameCache.GetFrameDataById(LF2StandardFrames.SuperPunch) != null)
                    {
                        HitConfirmEa = 0;
                        ImmediateFrame(LF2StandardFrames.SuperPunch);
                    }
                    else
                    {
                        int punchFrame = RandInt(0, 2) == 0 ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                        TrySpendFramePpCost(punchFrame, clampOnOverdraw: true);
                        ImmediateFrame(punchFrame);
                    }
                }
                else if (linkState == 101)
                {
                    ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.LightWeaponThw : RandomWeaponAttackFrame());
                }
                else if (linkState % 100 == 1)
                {
                    ImmediateFrame(RandomWeaponAttackFrame());
                }
                else if (linkState == 4)
                {
                    ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                }
                else if (linkState == 6)
                {
                    ImmediateFrame(LF2StandardFrames.SkyLgtWpThw);
                }
                else
                {
                    ApplyHeldWeaponStandingAttack();
                }
            }

            if (Controller.IsJump && (InputState?.JumpCooldown ?? 0) > 0)
            {
                handled = true;
                ImmediateFrame(LF2StandardFrames.Jumping);
                AttackingCounter = 0;
                AnimSub = 0;
            }

            if (Controller.IsDefend && (InputState?.DefendCooldown ?? 0) > 0 && InputState.DefendLockActive == false)
            {
                handled = true;
                ImmediateFrame(LF2StandardFrames.Defend);
                AnimSub = 0;
                AttackingCounter = 0;
            }

            return handled;
        }

        private void ApplyHeldWeaponStandingAttack()
        {
            if (!HasHeldObject())
                return;

            if (IsHeldHeavyWeapon())
            {
                ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
                return;
            }

            if (CanHeldObjectStandThrow())
            {
                ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                return;
            }

            if (IsHeldObjectAttackable())
                ImmediateFrame(RandomWeaponAttackFrame());
        }

        private bool ProcessRunningInput()
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            if (IsHeavyWeapon())
            {
                ProcessHeavyRunningInput(characterData);
                return true;
            }

            AttackingCounter = 0;
            StepRunningFrame(LF2StandardFrames.RunningStart, LF2StandardFrames.Running1);

            if (PS.dir == "right")
            {
                PS.vx = characterData.running_speed;
                if (Controller.IsLeft)
                    SetMoveFrameDirect(LF2StandardFrames.StopRunning);
            }
            else
            {
                PS.vx = -characterData.running_speed;
                if (Controller.IsRight)
                    SetMoveFrameDirect(LF2StandardFrames.StopRunning);
            }

            ApplyRunLane(characterData.running_speedz);

            bool handled = false;
            int linkState = Runtime.LinkState;
            if (Controller.IsAttack && (InputState?.AttackCooldown ?? 0) > 0)
            {
                handled = true;
                if (!HasHeldObject() || linkState == 0)
                {
                    if (TrySpendFramePpCost(LF2StandardFrames.RunAttack))
                        ImmediateFrame(LF2StandardFrames.RunAttack);
                }
                else if (linkState % 100 == 1)
                {
                    ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.RunWeaponAtck);
                }
                else if (linkState == 4)
                {
                    ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                }
                else if (linkState == 6)
                {
                    ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.SkyLgtWpThw);
                }
                else
                {
                    ApplyHeldWeaponRunningAttack();
                }
            }

            if (Controller.IsDefend && (InputState?.DefendCooldown ?? 0) > 0)
            {
                handled = true;
                ImmediateFrame(LF2StandardFrames.Rowing2);
            }

            if (Controller.IsJump && (InputState?.JumpCooldown ?? 0) > 0)
            {
                handled = true;
                ImmediateFrame(LF2StandardFrames.DashForward);
                ApplyDashStartVelocity(forward: true);
            }

            return handled;
        }

        private void ApplyHeldWeaponRunningAttack()
        {
            if (!HasHeldObject())
                return;

            if (IsHeldHeavyWeapon())
            {
                ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
                return;
            }

            if (HasHorizontalInput() && CanHeldObjectRunThrow())
            {
                ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                return;
            }

            if (IsHeldObjectAttackable())
                ImmediateFrame(LF2StandardFrames.RunWeaponAtck);
        }

        private bool ProcessJumpingInput()
        {
            if (Controller.IsRight && !Controller.IsLeft) SwitchDir("right");
            else if (Controller.IsLeft && !Controller.IsRight) SwitchDir("left");

            if (!Controller.IsAttack || JumpAttackLock > 0)
                return false;

            int linkState = Runtime.LinkState;
            if (!HasHeldObject() || linkState == 0)
            {
                AttackingCounter = 0;
                TrySpendFramePpCost(LF2StandardFrames.JumpAttack, clampOnOverdraw: true);
                ImmediateFrame(LF2StandardFrames.JumpAttack);
            }
            else if (linkState % 100 == 1)
            {
                AttackingCounter = 0;
                ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
            }
            else if (linkState == 4 || linkState == 6)
            {
                ImmediateFrame(LF2StandardFrames.SkyLgtWpThw);
            }
            else if (IsHeldObjectAttackable())
            {
                ImmediateFrame(HasHorizontalInput() ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
            }

            return true;
        }

        private bool ProcessDashInput()
        {
            ApplyDashFrame();

            bool dashForward = (PS.dir == "right" && PS.vx > 0f) || (PS.dir == "left" && PS.vx < 0f);
            if (!dashForward || !Controller.IsAttack)
                return false;

            int linkState = Runtime.LinkState;
            if (!HasHeldObject() || linkState == 0)
            {
                if (TrySpendFramePpCost(LF2StandardFrames.DashAttack))
                    ImmediateFrame(LF2StandardFrames.DashAttack);
            }
            else if (linkState % 100 == 1)
            {
                ImmediateFrame(LF2StandardFrames.DashWeaponAtck);
                PS.vy -= 1f;
                AttackingCounter = 0;
            }
            else if (linkState == 4 || linkState == 6)
            {
                if (HasAnyDirectionInput())
                {
                    ImmediateFrame(LF2StandardFrames.SkyLgtWpThw);
                    PS.vy -= 1f;
                    AttackingCounter = 0;
                }
            }
            else if (IsHeldObjectAttackable())
            {
                ImmediateFrame(LF2StandardFrames.DashWeaponAtck);
            }

            return true;
        }

        private bool ProcessDefendingInput()
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            float previousVx = PS.vx;
            PS.vx = 0f;
            PS.vz = 0f;

            if (!Controller.IsDefend)
            {
                ImmediateFrame(LF2StandardFrames.Standing);
                return true;
            }

            if ((Controller.IsRight || previousVx > 0f) && (InputState?.JumpCooldown ?? 0) > 0)
            {
                ImmediateFrame(PS.dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                PS.vx = characterData.walking_speed;
                PS.vy = 0f;
                InputState?.SetDefendLock(5);
                return true;
            }

            if ((Controller.IsLeft || previousVx < 0f) && (InputState?.JumpCooldown ?? 0) > 0)
            {
                ImmediateFrame(PS.dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
                PS.vx = -characterData.walking_speed;
                PS.vy = 0f;
                InputState?.SetDefendLock(5);
                return true;
            }

            InputState?.SetDefendLock(5);
            if (Frame.N != LF2StandardFrames.DashForward && Frame.N != LF2StandardFrames.DashForward2)
                ImmediateFrame(LF2StandardFrames.Defend);
            return false;
        }

        private bool ProcessCrouchInput()
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            bool handled = false;
            if (Controller.IsDefend && (InputState?.DefendCooldown ?? 0) > 0)
            {
                ImmediateFrame(LF2StandardFrames.Rowing2);
                handled = true;
            }

            if (Controller.IsJump)
            {
                if ((Controller.IsRight || PS.vx > 0.001f) && (InputState?.JumpCooldown ?? 0) > 0)
                {
                    ImmediateFrame(PS.dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                    PS.vx = characterData.dash_distance;
                    PS.vy = characterData.dash_height;
                    ApplyDashLane(characterData.dash_distancez);
                    AnimSub = 0;
                    handled = true;
                }
                else if ((Controller.IsLeft || PS.vx < -0.001f) && (InputState?.JumpCooldown ?? 0) > 0)
                {
                    ImmediateFrame(PS.dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
                    PS.vx = -characterData.dash_distance;
                    PS.vy = characterData.dash_height;
                    ApplyDashLane(characterData.dash_distancez);
                    AnimSub = 0;
                    handled = true;
                }
            }

            ApplyDashLane(characterData.dash_distancez);
            return handled;
        }

        private bool ProcessDefensiveRecoveryInput()
        {
            int frameId = Frame.N;
            if (frameId != LF2StandardFrames.FallingFront2 && frameId != LF2StandardFrames.FallingBack2)
                return false;

            if (WeaponCount < 0 || !Controller.IsJump || (InputState?.JumpCooldown ?? 0) <= 0 || Health.HP <= 0)
                return false;

            bool backward = PS.dir == "right" ? PS.vx <= 0f : PS.vx >= 0f;
            ImmediateFrame(backward ? LF2StandardFrames.Rowing : LF2StandardFrames.RowingBack);
            AttackingCounter = 0;

            var characterData = _FrameDataWrapper?.characterData;
            if (characterData != null)
            {
                if (PS.vy > characterData.rowing_height)
                    PS.vy = characterData.rowing_height;

                float rowingDistance = characterData.rowing_distance;
                if (PS.vx > -1f && PS.vx < 1f)
                    PS.vx = PS.dir == "left" ? rowingDistance : -rowingDistance;
                else
                    PS.vx = PS.vx > 0f ? rowingDistance : -rowingDistance;
            }

            return true;
        }

        private void ProcessHeavyWalkInput()
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null)
                return;

            if (Frame.N < LF2StandardFrames.HeavyObjWalk0)
                SetMoveFrameDirect(LF2StandardFrames.HeavyObjWalk0);

            ApplyWalkRunFrame(heavy: true);

            if (Controller.IsAttack && (InputState?.AttackCooldown ?? 0) > 0)
            {
                ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
                AnimSub = 0;
                AttackingCounter = 0;
            }
        }

        private void ProcessHeavyRunningInput(LF2CharacterData characterData)
        {
            AttackingCounter = 0;
            StepRunningFrame(LF2StandardFrames.HeavyObjRun, LF2StandardFrames.TreeJump0);

            if (PS.dir == "right")
            {
                PS.vx = characterData.heavy_running_speed;
                if (Controller.IsLeft) SetMoveFrameDirect(LF2StandardFrames.TreeJump2);
            }
            else
            {
                PS.vx = -characterData.heavy_running_speed;
                if (Controller.IsRight) SetMoveFrameDirect(LF2StandardFrames.TreeJump2);
            }

            ApplyRunLane(characterData.heavy_running_speedz);

            if (Controller.IsAttack && (InputState?.AttackCooldown ?? 0) > 0)
                ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
        }

        private void StepRunningFrame(int frameBase, int loopFrame)
        {
            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null)
                return;

            int rate = characterData.running_frame_rate;
            if (rate < 1) rate = 1;

            AnimCounter = (AnimCounter + 1) % (rate * 4);
            int fi = AnimCounter / rate;
            SetMoveFrameDirect(fi < 3 ? frameBase + fi : loopFrame);
        }

        private void ApplyVerticalInputForSpecialStates()
        {
            int state = Frame?.D?.state ?? 0;
            if ((state != LF2States.DeepSpecific && state != LF2States.FirenSpecific) || PS.y != 0f)
                return;

            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null)
                return;

            if (Controller.IsUp && !Controller.IsDown)
                PS.vz = -characterData.running_speedz;
            else if (Controller.IsDown && !Controller.IsUp)
                PS.vz = characterData.running_speedz;
        }

        private void ApplyDashLane(float dashDistanceZ)
        {
            if (Controller.IsUp && !Controller.IsDown)
                PS.vz = -dashDistanceZ;
            else if (Controller.IsDown && !Controller.IsUp)
                PS.vz = dashDistanceZ;
        }

        private bool HasAnyDirectionInput()
        {
            return Controller.IsLeft || Controller.IsRight || Controller.IsUp || Controller.IsDown;
        }

        private bool HasHorizontalInput()
        {
            return Controller.IsLeft != Controller.IsRight;
        }

        private int RandomWeaponAttackFrame()
        {
            return RandInt(0, 2) == 0 ? LF2StandardFrames.NormalWeaponAtck : LF2StandardFrames.NormalWeaponAtck2;
        }
    }
}
