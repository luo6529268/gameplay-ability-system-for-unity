using LinkedField = NTSD.Simulation.BattleNativeLinkedWeaponActionField;
using LinkedActions = NTSD.Simulation.BattleNativeLinkedWeaponActionResolver;
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
        private LF2Character _character;

        public bool ProcessReleaseInput(LF2Character character)
        {
            if (character == null)
                return false;
            if (_character != null)
                throw new System.InvalidOperationException(
                    "The world-owned character action resolver cannot be re-entered.");

            _character = character;
            try
            {
                return ProcessReleaseInputCore();
            }
            finally
            {
                _character = null;
            }
        }

        private bool ProcessReleaseInputCore()
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

            if (_character.Runtime.LinkState == 2 && (state == LF2States.Standing || state == LF2States.Walking))
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
                    if (_character.Runtime.LinkState == 0 ? _character.Runtime.YInt < 0 :
                        _character.Runtime.Y < _character.PS.groundY)
                        handled = ProcessJumpingInput();
                    break;

                case LF2States.Dash:
                    handled = ProcessDashInput();
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

                if (linkState == 0)
                {
                    if (_character.HitConfirmEa > 0 && _character.FrameCache.HasFrame(LF2StandardFrames.SuperPunch))
                    {
                        _character.SetInputFrameDirectInternal(LF2StandardFrames.SuperPunch);
                    }
                    else
                    {
                        int punchFrame = (NativeAttackSelection(0x82u) + 12) * 5;
                        _character.TrySpendFramePpCost(punchFrame, clampOnOverdraw: true);
                        _character.SetInputFrameDirectInternal(punchFrame);
                    }
                }
                else if (linkState == 101)
                {
                    WriteLinkedAction(HasAnyDirectionInput()
                        ? LinkedAction(LinkedField.LightThrow, LinkedActions.LightThrowFallback)
                        : RandomWeaponAttackFrame());
                }
                else if (linkState % 100 == 1)
                {
                    WriteLinkedAction(RandomWeaponAttackFrame());
                }
                else if (linkState == 4)
                {
                    WriteLinkedAction(LinkedAction(LinkedField.LightThrow, LinkedActions.LightThrowFallback));
                }
                else if (linkState == 6)
                {
                    WriteLinkedAction(HasAnyDirectionInput()
                        ? LinkedAction(LinkedField.LightThrow, LinkedActions.LightThrowFallback)
                        : LinkedAction(LinkedField.WeaponDrink, LinkedActions.WeaponDrinkFallback));
                }
            }

            if (_character.IsJumpActionInputReadyInternal())
            {
                handled = true;
                _character.SetInputFrameDirectInternal(LF2StandardFrames.Jumping);
                _character.AttackingCounter = 0;
                _character.SetAnimSubInternal(0);
            }

            if (_character.IsDefendActionInputReadyInternal(requireDefendLockOpen: true))
            {
                handled = true;
                _character.SetInputFrameDirectInternal(LF2StandardFrames.Defend);
                _character.SetAnimSubInternal(0);
                _character.AttackingCounter = 0;
            }

            return handled;
        }

        private bool ProcessRunningInput()
        {
            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            if (_character.Runtime.LinkState == 2)
            {
                ProcessHeavyRunningInput(characterData);
                return true;
            }

            if (_character.Runtime.LinkState == 0)
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
            if (linkState == 0 ? _character.IsAttackActionInputReadyInternal() :
                _character.ReadAttackCooldownInternal() > 0)
            {
                handled = true;
                if (linkState == 0)
                {
                    if (_character.TrySpendFramePpCost(LF2StandardFrames.RunAttack))
                        _character.SetInputFrameDirectInternal(LF2StandardFrames.RunAttack);
                }
                else if (linkState % 100 == 1)
                {
                    WriteLinkedAction(HasAnyDirectionInput()
                        ? LinkedAction(LinkedField.LightThrow, LinkedActions.LightThrowFallback)
                        : LinkedAction(LinkedField.RunAttack, LinkedActions.RunAttackFallback));
                }
                else if (linkState == 4)
                {
                    WriteLinkedAction(LinkedAction(LinkedField.LightThrow, LinkedActions.LightThrowFallback));
                }
                else if (linkState == 6)
                {
                    WriteLinkedAction(HasAnyDirectionInput()
                        ? LinkedAction(LinkedField.LightThrow, LinkedActions.LightThrowFallback)
                        : LinkedAction(LinkedField.WeaponDrink, LinkedActions.WeaponDrinkFallback));
                }
                else
                {
                    WriteLinkedAction(85);
                }
            }

            if (_character.IsDefendActionInputReadyInternal())
            {
                handled = true;
                _character.SetInputFrameDirectInternal(LF2StandardFrames.Rowing2);
            }

            if (_character.IsJumpActionInputReadyInternal())
            {
                handled = true;
                _character.SetInputFrameDirectInternal(LF2StandardFrames.DashForward);
                _character.ApplyDashStartVelocityInternal(forward: true);
            }

            return handled;
        }

        private bool ProcessJumpingInput()
        {
            if (_character.IsCurrentRightPressedInternal() && !_character.IsCurrentLeftPressedInternal()) _character.SwitchDir("right");
            else if (_character.IsCurrentLeftPressedInternal() && !_character.IsCurrentRightPressedInternal()) _character.SwitchDir("left");

            int linkState = _character.Runtime.LinkState;
            bool ready = linkState == 0
                ? _character.IsCurrentJumpPressedInternal() && _character.Runtime.JumpAttackLock <= 0
                : _character.ReadAttackCooldownInternal() > 0;
            if (!ready)
                return false;
            if (linkState == 0)
            {
                _character.AttackingCounter = 0;
                _character.TrySpendFramePpCost(LF2StandardFrames.JumpAttack, clampOnOverdraw: true);
                _character.SetInputFrameDirectInternal(LF2StandardFrames.JumpAttack);
            }
            else if (linkState % 100 == 1 && !HasAnyDirectionInput())
            {
                WriteLinkedAction(LinkedAction(LinkedField.JumpAttack, LinkedActions.AirJumpAttackFallback));
                _character.AttackingCounter = 0;
            }
            else if (linkState % 100 == 1 || linkState == 4 || linkState == 6)
            {
                WriteLinkedAction(LinkedAction(LinkedField.SkyLightThrow, LinkedActions.SkyLightThrowFallback));
                _character.AttackingCounter = 0;
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
            if (linkState == 0)
            {
                if (_character.TrySpendFramePpCost(LF2StandardFrames.DashAttack))
                    _character.SetInputFrameDirectInternal(LF2StandardFrames.DashAttack);
            }
            else if (linkState % 100 == 1)
            {
                WriteLinkedAction(LinkedAction(LinkedField.JumpAttack, LinkedActions.DashJumpAttackFallback));
                _character.Runtime.Vy -= 1.0;
                _character.AttackingCounter = 0;
            }
            else if ((linkState == 4 && HasAnyDirectionInput() && !HasAllDirectionInput()) ||
                     (linkState == 6 && HasAnyDirectionInput()))
            {
                WriteLinkedAction(LinkedAction(LinkedField.SkyLightThrow, LinkedActions.SkyLightThrowFallback));
                _character.Runtime.Vy -= 1.0;
                _character.AttackingCounter = 0;
            }
            return true;
        }

        private bool ProcessCrouchInput()
        {
            var characterData = _character._FrameDataWrapper?.characterData;
            if (characterData == null)
                return false;

            bool handled = false;
            if (_character.IsDefendActionInputReadyInternal())
            {
                _character.SetInputFrameDirectInternal(LF2StandardFrames.Rowing2);
                handled = true;
            }

            if (_character.IsCurrentDefendPressedInternal())
            {
                if ((_character.IsCurrentRightPressedInternal() || _character.PS.vx > 0.001f) && _character.IsJumpActionInputReadyInternal())
                {
                    _character.SetInputFrameDirectInternal(_character.PS.dir == "right" ? LF2StandardFrames.DashForward : LF2StandardFrames.DashForward2);
                    _character.PS.vx = characterData.dash_distance;
                    _character.PS.vy = characterData.dash_height;
                    ApplyDashLane(characterData.dash_distancez);
                    _character.SetAnimSubInternal(0);
                    handled = true;
                }
                else if ((_character.IsCurrentLeftPressedInternal() || _character.PS.vx < -0.001f) && _character.IsJumpActionInputReadyInternal())
                {
                    _character.SetInputFrameDirectInternal(_character.PS.dir == "right" ? LF2StandardFrames.DashForward2 : LF2StandardFrames.DashForward);
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
            _character.SetInputFrameDirectInternal(backward ? LF2StandardFrames.Rowing : LF2StandardFrames.RowingBack);
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
            LF2CharacterData data = _character.FrameCache?.Wrapper?.characterData;
            NTSDEntityRuntime runtime = _character.Runtime;
            bool left = _character.IsCurrentLeftPressedInternal();
            bool right = _character.IsCurrentRightPressedInternal();
            bool up = _character.IsCurrentUpPressedInternal();
            bool down = _character.IsCurrentDownPressedInternal();
            double walkX = data?.heavy_walking_speed ?? 0.0;
            double walkZ = data?.heavy_walking_speedz ?? 0.0;
            int walkRate = data?.heavy_walking_frames?.Count > 0
                ? data.heavy_walking_frames.Count
                : data?.walking_frame_rate ?? 1;
            if (_character.Frame.N < 12)
                AssignHeavyAction(12, false);

            bool walking = false;
            if (right && !left)
            {
                if (runtime.Dir == "left")
                    runtime.AnimSub = 0;
                _character.SwitchDir("right");
                runtime.Vx = walkX;
                if (!_character.WasRightPressedPreviousFrameInternal())
                    runtime.AnimSub += 10;
                walking = true;
            }
            else if (left && !right)
            {
                if (runtime.Dir != "left")
                    runtime.AnimSub = 0;
                _character.SwitchDir("left");
                runtime.Vx = -walkX;
                if (!_character.WasLeftPressedPreviousFrameInternal())
                    runtime.AnimSub -= 10;
                walking = true;
            }
            if (up && !down)
            {
                runtime.Vz = -walkZ;
                walking = true;
            }
            else if (down && !up)
            {
                runtime.Vz = walkZ;
                walking = true;
            }
            ScaleNativeDiagonalX(runtime, up != down, 1.4);

            if (walking)
            {
                bool running = runtime.AnimSub > 10 || runtime.AnimSub < -10;
                if (running)
                {
                    runtime.AnimSub = 0;
                    runtime.AnimCounter = 0;
                    AssignHeavyAction(16, false);
                    if (_character.Frame.N == 16)
                    {
                        ProcessRunningInput();
                        return;
                    }
                }
                else
                {
                    AssignHeavyAction(
                        NativeMovementAction(
                            data?.heavy_walking_frames,
                            12,
                            6,
                            walkRate,
                            runtime),
                        false);
                }
            }

            if (_character.ReadAttackCooldownInternal() > 0)
            {
                runtime.AnimCounter = 0;
                AssignHeavyAction(
                    LinkedAction(LinkedField.HeavyThrow, LinkedActions.HeavyThrowFallback),
                    true);
            }
            if (_character.ReadJumpCooldownInternal() > 0)
            {
                runtime.AnimCounter = 0;
                AssignHeavyAction(210, true);
            }
            if (_character.ReadDefendCooldownInternal() > 0 &&
                !_character.IsDefendLockActiveInternal())
            {
                runtime.AnimCounter = 0;
                AssignHeavyAction(110, true);
            }
        }

        private void ProcessHeavyRunningInput(LF2CharacterData data)
        {
            NTSDEntityRuntime runtime = _character.Runtime;
            bool left = _character.IsCurrentLeftPressedInternal();
            bool right = _character.IsCurrentRightPressedInternal();
            bool up = _character.IsCurrentUpPressedInternal();
            bool down = _character.IsCurrentDownPressedInternal();
            double runX = data?.heavy_running_speed ?? 0.0;
            double runZ = data?.heavy_running_speedz ?? 0.0;
            int runRate = data?.heavy_running_frames?.Count > 0
                ? data.heavy_running_frames.Count
                : data?.running_frame_rate ?? 1;
            if (right && !left)
            {
                _character.SwitchDir("right");
                runtime.Vx = runX;
            }
            else if (left && !right)
            {
                _character.SwitchDir("left");
                runtime.Vx = -runX;
            }
            if (up && !down)
                runtime.Vz = -runZ;
            else if (down && !up)
                runtime.Vz = runZ;
            ScaleNativeDiagonalX(runtime, up != down, 1.2);
            if ((left != right) || (up != down))
            {
                AssignHeavyAction(
                    NativeMovementAction(
                        data?.heavy_running_frames,
                        16,
                        4,
                        runRate,
                        runtime),
                    false);
            }

            if (_character.ReadAttackCooldownInternal() > 0)
            {
                AssignHeavyAction(
                    LinkedAction(LinkedField.RunHeavyThrow, LinkedActions.RunHeavyThrowFallback),
                    false);
            }
            if (_character.ReadJumpCooldownInternal() > 0)
            {
                _character.ApplyDashStartVelocityInternal(forward: true);
                runtime.AnimSub = 0;
                AssignHeavyAction(213, false);
            }
            if (_character.ReadDefendCooldownInternal() > 0)
                AssignHeavyAction(102, false);
        }

        private void AssignHeavyAction(int action, bool resetCounter)
        {
            WriteLinkedAction(action);
            if (resetCounter) _character.AttackingCounter = 0;
        }

        private static void ScaleNativeDiagonalX(NTSDEntityRuntime runtime, bool diagonal, double divisor)
        {
            if (diagonal) runtime.Vx /= divisor;
        }

        private static int NativeMovementAction(System.Collections.Generic.List<int> sequence,
            int fallbackBase, int fallbackPhases, int rate, NTSDEntityRuntime runtime)
        {
            if (rate < 1)
                rate = 1;
            int count = sequence?.Count ?? 0;
            int phases = count > 0 ? count : fallbackPhases;
            int period = phases * rate;
            runtime.AnimCounter = (runtime.AnimCounter + 1) % period;
            int phase = runtime.AnimCounter / rate;
            if (count > 0)
                return sequence[phase];
            if (fallbackPhases == 6 && phase > 3)
                phase = 6 - phase;
            if (fallbackPhases == 4 && phase > 2)
                phase -= 2;
            return fallbackBase + phase;
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
            if ((state != LF2States.DeepSpecific && state != LF2States.FirenSpecific) || _character.Runtime.YInt != 0)
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
            return NativeAttackSelection(_character.Runtime.LinkState == 101 ? 0x83u : 0x84u) == 0
                ? LinkedAction(LinkedField.NormalAttack1, LinkedActions.NormalAttack1Fallback)
                : LinkedAction(LinkedField.NormalAttack2, LinkedActions.NormalAttack2Fallback);
        }

        private int NativeAttackSelection(uint callSite)
        {
            SimulationWorld world = _character.RegisteredWorldForSimulation;
            return world != null ? world.NativeRandom.SynchronizedNext(callSite, 2)
                : _character.RandIntInternal(0, 2);
        }

        private int LinkedAction(LinkedField field, int fallback)
        {
            LF2Entity linked = _character.RegisteredWorldForSimulation?
                .FindEntityByRuntimeSlotForQuery(_character.Runtime.TargetSlotIndex);
            return LinkedActions.Resolve(linked?.FrameCache?.Wrapper?.characterData, field, fallback);
        }

        private void WriteLinkedAction(int action)
        {
            _character.WriteNativeInputActionUnchecked(action);
        }

        private bool HasAllDirectionInput()
        {
            return _character.IsCurrentLeftPressedInternal() && _character.IsCurrentRightPressedInternal() &&
                   _character.IsCurrentUpPressedInternal() && _character.IsCurrentDownPressedInternal();
        }
    }
}
