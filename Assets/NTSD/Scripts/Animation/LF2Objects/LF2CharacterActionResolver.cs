using NTSD.Animation;
using NTSD.Input;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 角色“松开输入 / 动作输入”处理器。
    ///
    /// 当角色处于站立、行走、跑动、跳跃、冲刺、防御、抓取等状态时，
    /// 每一拍从输入快照推导出“该切到哪一张动作帧、要不要起跳/攻击/防御”
    /// 都由这个类负责（对应 C++ release 的 release-input / a_rest 动作选择阶段）。
    ///
    /// 这里只做“输入 → 动作帧”的选择；具体的移动帧写入、速度写入
    /// 仍然复用 LF2Character 暴露的 State/Locomotion 桥接入口。
    /// </summary>
    internal sealed class LF2CharacterActionResolver
    {
        private readonly LF2Character _character;

        public LF2CharacterActionResolver(LF2Character character)
        {
            _character = character;
        }

        public bool ProcessReleaseInput()
        {
            if (_character.Frame?.D == null || _character.PS == null)
                return false;

            if (_character.Frame.N == LF2StandardFrames.Defend)
            {
                if (_character.IsCurrentRightPressedInternal()) _character.SwitchDir("right");
                if (_character.IsCurrentLeftPressedInternal()) _character.SwitchDir("left");
            }

            ApplyVerticalInputForSpecialStates();

            int state = _character.Frame.D.state;
            bool handled = false;

            if (_character.IsHeavyWeapon() && (state == LF2States.Standing || state == LF2States.Walking))
            {
                ProcessHeavyWalkInput();
                return true;
            }

            if (_character.Frame.N == LF2StandardFrames.Crouch)
                return ProcessCrouchInput();

            if (ProcessDefensiveRecoveryInput())
                return true;

            switch (state)
            {
                case LF2States.Standing:
                case LF2States.Walking:
                    _character.ApplyWalkRunFrameInternal(heavy: false);
                    handled = ProcessStandingActions();
                    break;

                case LF2States.Running:
                    handled = ProcessRunningInput();
                    break;

                case LF2States.Jump:
                    if (_character.PS.y < 0f)
                        handled = ProcessJumpingInput();
                    break;

                case LF2States.Dash:
                    handled = ProcessDashInput();
                    break;

                case LF2States.Defending:
                    handled = ProcessDefendingInput();
                    break;

                case LF2States.Catching:
                    handled = _character.ProcessCatchingInputInternal();
                    break;
            }

            return handled;
        }

        private bool ProcessStandingActions()
        {
            bool handled = false;
            int linkState = _character.Runtime.LinkState;

            if (_character.IsAttackActionInputReadyInternal())
            {
                handled = true;
                _character.SetAnimSubInternal(0);
                _character.AttackingCounter = 0;

                if (!_character.HasHeldObjectInternal() || linkState == 0)
                {
                    if (_character.HitConfirmEa > 0 && _character.FrameCache.GetFrameDataById(LF2StandardFrames.SuperPunch) != null)
                    {
                        _character.HitConfirmEa = 0;
                        _character.ImmediateFrame(LF2StandardFrames.SuperPunch);
                    }
                    else
                    {
                        int punchFrame = _character.RandIntInternal(0, 2) == 0 ? LF2StandardFrames.Punch : LF2StandardFrames.Punch4;
                        _character.TrySpendFramePpCost(punchFrame, clampOnOverdraw: true);
                        _character.ImmediateFrame(punchFrame);
                    }
                }
                else if (linkState == 101)
                {
                    _character.ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.LightWeaponThw : RandomWeaponAttackFrame());
                }
                else if (linkState % 100 == 1)
                {
                    _character.ImmediateFrame(RandomWeaponAttackFrame());
                }
                else if (linkState == 4)
                {
                    _character.ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                }
                else if (linkState == 6)
                {
                    _character.ImmediateFrame(LF2StandardFrames.SkyLgtWpThw);
                }
                else
                {
                    ApplyHeldWeaponStandingAttack();
                }
            }

            if (_character.IsJumpActionInputReadyInternal())
            {
                handled = true;
                _character.ImmediateFrame(LF2StandardFrames.Jumping);
                _character.AttackingCounter = 0;
                _character.SetAnimSubInternal(0);
            }

            if (_character.IsDefendActionInputReadyInternal(requireDefendLockOpen: true))
            {
                handled = true;
                _character.ImmediateFrame(LF2StandardFrames.Defend);
                _character.SetAnimSubInternal(0);
                _character.AttackingCounter = 0;
            }

            return handled;
        }

        private void ApplyHeldWeaponStandingAttack()
        {
            if (!_character.HasHeldObjectInternal())
                return;

            if (_character.IsHeldHeavyWeaponInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
                return;
            }

            if (_character.CanHeldObjectStandThrowInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                return;
            }

            if (_character.IsHeldObjectAttackableInternal())
                _character.ImmediateFrame(RandomWeaponAttackFrame());
        }

        private bool ProcessRunningInput()
        {
            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            if (_character.IsHeavyWeapon())
            {
                ProcessHeavyRunningInput(characterData);
                return true;
            }

            _character.AttackingCounter = 0;
            StepRunningFrame(LF2StandardFrames.RunningStart, LF2StandardFrames.Running1);

            if (_character.PS.dir == "right")
            {
                _character.PS.vx = characterData.running_speed;
                if (_character.IsCurrentLeftPressedInternal())
                    _character.SetMoveFrameDirectInternal(LF2StandardFrames.StopRunning);
            }
            else
            {
                _character.PS.vx = -characterData.running_speed;
                if (_character.IsCurrentRightPressedInternal())
                    _character.SetMoveFrameDirectInternal(LF2StandardFrames.StopRunning);
            }

            _character.ApplyRunLaneInternal(characterData.running_speedz);

            bool handled = false;
            int linkState = _character.Runtime.LinkState;
            if (_character.IsAttackActionInputReadyInternal())
            {
                handled = true;
                if (!_character.HasHeldObjectInternal() || linkState == 0)
                {
                    if (_character.TrySpendFramePpCost(LF2StandardFrames.RunAttack))
                        _character.ImmediateFrame(LF2StandardFrames.RunAttack);
                }
                else if (linkState % 100 == 1)
                {
                    _character.ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.RunWeaponAtck);
                }
                else if (linkState == 4)
                {
                    _character.ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                }
                else if (linkState == 6)
                {
                    _character.ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.LightWeaponThw : LF2StandardFrames.SkyLgtWpThw);
                }
                else
                {
                    ApplyHeldWeaponRunningAttack();
                }
            }

            if (_character.IsDefendActionInputReadyInternal())
            {
                handled = true;
                _character.ImmediateFrame(LF2StandardFrames.Rowing2);
            }

            if (_character.IsJumpActionInputReadyInternal())
            {
                handled = true;
                _character.ImmediateFrame(LF2StandardFrames.DashForward);
                _character.ApplyDashStartVelocityInternal(forward: true);
            }

            return handled;
        }

        private void ApplyHeldWeaponRunningAttack()
        {
            if (!_character.HasHeldObjectInternal())
                return;

            if (_character.IsHeldHeavyWeaponInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
                return;
            }

            if (HasHorizontalInput() && _character.CanHeldObjectRunThrowInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.LightWeaponThw);
                return;
            }

            if (_character.IsHeldObjectAttackableInternal())
                _character.ImmediateFrame(LF2StandardFrames.RunWeaponAtck);
        }

        private bool ProcessJumpingInput()
        {
            if (_character.IsCurrentRightPressedInternal() && !_character.IsCurrentLeftPressedInternal()) _character.SwitchDir("right");
            else if (_character.IsCurrentLeftPressedInternal() && !_character.IsCurrentRightPressedInternal()) _character.SwitchDir("left");

            if (!_character.IsCurrentJumpPressedInternal() || _character.Runtime.JumpAttackLock > 0)
                return false;

            int linkState = _character.Runtime.LinkState;
            if (!_character.HasHeldObjectInternal() || linkState == 0)
            {
                _character.AttackingCounter = 0;
                _character.TrySpendFramePpCost(LF2StandardFrames.JumpAttack, clampOnOverdraw: true);
                _character.ImmediateFrame(LF2StandardFrames.JumpAttack);
            }
            else if (linkState % 100 == 1)
            {
                _character.AttackingCounter = 0;
                _character.ImmediateFrame(HasAnyDirectionInput() ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
            }
            else if (linkState == 4 || linkState == 6)
            {
                _character.ImmediateFrame(LF2StandardFrames.SkyLgtWpThw);
            }
            else if (_character.IsHeldObjectAttackableInternal())
            {
                _character.ImmediateFrame(HasHorizontalInput() ? LF2StandardFrames.SkyLgtWpThw : LF2StandardFrames.JumpWeaponAtck);
            }

            return true;
        }

        private bool ProcessDashInput()
        {
            _character.ApplyDashFrameInternal();

            bool dashForward = (_character.PS.dir == "right" && _character.PS.vx > 0f) || (_character.PS.dir == "left" && _character.PS.vx < 0f);
            if (!dashForward || !_character.IsCurrentJumpPressedInternal())
                return false;

            int linkState = _character.Runtime.LinkState;
            if (!_character.HasHeldObjectInternal() || linkState == 0)
            {
                if (_character.TrySpendFramePpCost(LF2StandardFrames.DashAttack))
                    _character.ImmediateFrame(LF2StandardFrames.DashAttack);
            }
            else if (linkState % 100 == 1)
            {
                _character.ImmediateFrame(LF2StandardFrames.DashWeaponAtck);
                _character.PS.vy -= 1f;
                _character.AttackingCounter = 0;
            }
            else if (linkState == 4 || linkState == 6)
            {
                if (HasAnyDirectionInput())
                {
                    _character.ImmediateFrame(LF2StandardFrames.SkyLgtWpThw);
                    _character.PS.vy -= 1f;
                    _character.AttackingCounter = 0;
                }
            }
            else if (_character.IsHeldObjectAttackableInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.DashWeaponAtck);
            }

            return true;
        }

        private bool ProcessDefendingInput()
        {
            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            double previousVx = _character.PS.vx;
            _character.PS.vx = 0f;
            _character.PS.vz = 0f;

            if (!_character.IsCurrentAttackPressedInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.Standing);
                return true;
            }

            if ((_character.IsCurrentRightPressedInternal() || previousVx > 0f) && _character.IsJumpActionInputReadyInternal())
            {
                _character.ImmediateFrame(_character.PS.dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                _character.PS.vx = characterData.walking_speed;
                _character.PS.vy = 0f;
                _character.SetDefendLockInternal(5);
                return true;
            }

            if ((_character.IsCurrentLeftPressedInternal() || previousVx < 0f) && _character.IsJumpActionInputReadyInternal())
            {
                _character.ImmediateFrame(_character.PS.dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
                _character.PS.vx = -characterData.walking_speed;
                _character.PS.vy = 0f;
                _character.SetDefendLockInternal(5);
                return true;
            }

            _character.SetDefendLockInternal(5);
            if (_character.Frame.N != LF2StandardFrames.DashForward && _character.Frame.N != LF2StandardFrames.DashForward2)
                _character.ImmediateFrame(LF2StandardFrames.Defend);
            return false;
        }

        private bool ProcessCrouchInput()
        {
            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            bool handled = false;
            if (_character.IsDefendActionInputReadyInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.Rowing2);
                handled = true;
            }

            if (_character.IsCurrentDefendPressedInternal())
            {
                if ((_character.IsCurrentRightPressedInternal() || _character.PS.vx > 0.001f) && _character.IsJumpActionInputReadyInternal())
                {
                    _character.ImmediateFrame(_character.PS.dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                    _character.PS.vx = characterData.dash_distance;
                    _character.PS.vy = characterData.dash_height;
                    ApplyDashLane(characterData.dash_distancez);
                    _character.SetAnimSubInternal(0);
                    handled = true;
                }
                else if ((_character.IsCurrentLeftPressedInternal() || _character.PS.vx < -0.001f) && _character.IsJumpActionInputReadyInternal())
                {
                    _character.ImmediateFrame(_character.PS.dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
                    _character.PS.vx = -characterData.dash_distance;
                    _character.PS.vy = characterData.dash_height;
                    ApplyDashLane(characterData.dash_distancez);
                    _character.SetAnimSubInternal(0);
                    handled = true;
                }
            }

            ApplyDashLane(characterData.dash_distancez);
            return handled;
        }

        private bool ProcessDefensiveRecoveryInput()
        {
            int frameId = _character.Frame.N;
            if (frameId != LF2StandardFrames.FallingFront2 && frameId != LF2StandardFrames.FallingBack2)
                return false;

            if (_character.WeaponCount < 0 || !_character.IsJumpActionInputReadyInternal() || _character.Health.HP <= 0)
                return false;

            bool backward = _character.PS.dir == "right" ? _character.PS.vx <= 0f : _character.PS.vx >= 0f;
            _character.ImmediateFrame(backward ? LF2StandardFrames.Rowing : LF2StandardFrames.RowingBack);
            _character.AttackingCounter = 0;

            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData != null)
            {
                if (_character.PS.vy > characterData.rowing_height)
                    _character.PS.vy = characterData.rowing_height;

                float rowingDistance = characterData.rowing_distance;
                if (_character.PS.vx > -1f && _character.PS.vx < 1f)
                    _character.PS.vx = _character.PS.dir == "left" ? rowingDistance : -rowingDistance;
                else
                    _character.PS.vx = _character.PS.vx > 0f ? rowingDistance : -rowingDistance;
            }

            return true;
        }

        private void ProcessHeavyWalkInput()
        {
            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return;

            if (_character.Frame.N < LF2StandardFrames.HeavyObjWalk0)
                _character.SetMoveFrameDirectInternal(LF2StandardFrames.HeavyObjWalk0);

            _character.ApplyWalkRunFrameInternal(heavy: true);

            if (_character.IsAttackActionInputReadyInternal())
            {
                _character.ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
                _character.SetAnimSubInternal(0);
                _character.AttackingCounter = 0;
            }
        }

        private void ProcessHeavyRunningInput(LF2CharacterData characterData)
        {
            _character.AttackingCounter = 0;
            StepRunningFrame(LF2StandardFrames.HeavyObjRun, LF2StandardFrames.TreeJump0);

            if (_character.PS.dir == "right")
            {
                _character.PS.vx = characterData.heavy_running_speed;
                if (_character.IsCurrentLeftPressedInternal()) _character.SetMoveFrameDirectInternal(LF2StandardFrames.TreeJump2);
            }
            else
            {
                _character.PS.vx = -characterData.heavy_running_speed;
                if (_character.IsCurrentRightPressedInternal()) _character.SetMoveFrameDirectInternal(LF2StandardFrames.TreeJump2);
            }

            _character.ApplyRunLaneInternal(characterData.heavy_running_speedz);

            if (_character.IsAttackActionInputReadyInternal())
                _character.ImmediateFrame(LF2StandardFrames.HeavyWeaponThw);
        }

        private void StepRunningFrame(int frameBase, int loopFrame)
        {
            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return;

            int rate = characterData.running_frame_rate;
            if (rate < 1) rate = 1;

            int animCounter = _character.GetAnimCounterInternal();
            animCounter = (animCounter + 1) % (rate * 4);
            _character.SetAnimCounterInternal(animCounter);
            int fi = animCounter / rate;
            _character.SetMoveFrameDirectInternal(fi < 3 ? frameBase + fi : loopFrame);
        }

        private void ApplyVerticalInputForSpecialStates()
        {
            int state = _character.Frame?.D?.state ?? 0;
            if ((state != LF2States.DeepSpecific && state != LF2States.FirenSpecific) || _character.PS.y != 0f)
                return;

            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return;

            if (_character.IsCurrentUpPressedInternal() && !_character.IsCurrentDownPressedInternal())
                _character.PS.vz = -characterData.running_speedz;
            else if (_character.IsCurrentDownPressedInternal() && !_character.IsCurrentUpPressedInternal())
                _character.PS.vz = characterData.running_speedz;
        }

        private void ApplyDashLane(float dashDistanceZ)
        {
            if (_character.IsCurrentUpPressedInternal() && !_character.IsCurrentDownPressedInternal())
                _character.PS.vz = -dashDistanceZ;
            else if (_character.IsCurrentDownPressedInternal() && !_character.IsCurrentUpPressedInternal())
                _character.PS.vz = dashDistanceZ;
        }

        private bool HasAnyDirectionInput()
        {
            return _character.IsCurrentLeftPressedInternal() ||
                   _character.IsCurrentRightPressedInternal() ||
                   _character.IsCurrentUpPressedInternal() ||
                   _character.IsCurrentDownPressedInternal();
        }

        private bool HasHorizontalInput()
        {
            return _character.IsCurrentLeftPressedInternal() != _character.IsCurrentRightPressedInternal();
        }

        private int RandomWeaponAttackFrame()
        {
            return _character.RandIntInternal(0, 2) == 0 ? LF2StandardFrames.NormalWeaponAtck : LF2StandardFrames.NormalWeaponAtck2;
        }
    }
}
