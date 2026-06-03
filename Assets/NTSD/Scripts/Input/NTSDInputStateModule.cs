using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Input
{
    public enum NTSDInputCommand
    {
        None,
        Left,
        Right,
        Up,
        Down,
        Defend,
        Jump,
        Attack,
        RunLeft,
        RunRight,
    }

    public sealed class NTSDInputStateModule
    {
        private bool _right;
        private bool _left;
        private bool _up;
        private bool _down;
        private bool _attack;
        private bool _jump;
        private bool _defend;

        private bool _prevRight;
        private bool _prevLeft;
        private bool _prevUp;
        private bool _prevDown;
        private bool _prevAttack;
        private bool _prevJump;
        private bool _prevDefend;

        private byte _cdRight;
        private byte _cdLeft;
        private byte _cdUp;
        private byte _cdDown;
        private byte _cdAttack;
        private byte _cdJump;
        private byte _cdDefend;
        private byte _cdDefendLock;

        private byte _comboDRA;
        private byte _comboDLA;
        private byte _comboDUA;
        private byte _comboDDA;
        private byte _comboDRJ;
        private byte _comboDLJ;
        private byte _comboDUJ;
        private byte _comboDDJ;
        private byte _comboDJA;

        private FuncKeyMask _lastDirectionTap;
        private int _lastDirectionTapTick = -1;
        private NTSDInputCommand _pendingCommand;

        public bool Right => _right;
        public bool Left => _left;
        public bool Up => _up;
        public bool Down => _down;
        public bool Attack => _attack;
        public bool Jump => _jump;
        public bool Defend => _defend;
        public bool DefendLockActive => _cdDefendLock > 0;

        public void Reset()
        {
            _right = _left = _up = _down = false;
            _attack = _jump = _defend = false;
            _prevRight = _prevLeft = _prevUp = _prevDown = false;
            _prevAttack = _prevJump = _prevDefend = false;
            _cdRight = _cdLeft = _cdUp = _cdDown = 0;
            _cdAttack = _cdJump = _cdDefend = _cdDefendLock = 0;
            _comboDRA = _comboDLA = _comboDUA = _comboDDA = 0;
            _comboDRJ = _comboDLJ = _comboDUJ = _comboDDJ = _comboDJA = 0;
            _lastDirectionTap = FuncKeyMask.None;
            _lastDirectionTapTick = -1;
            _pendingCommand = NTSDInputCommand.None;
        }

        public void UpdateFromBuffer(SimInputBuffer inputBuffer, int tickIndex, LF2Character owner)
        {
            _prevRight = _right;
            _prevLeft = _left;
            _prevUp = _up;
            _prevDown = _down;
            _prevAttack = _attack;
            _prevJump = _jump;
            _prevDefend = _defend;
            _pendingCommand = NTSDInputCommand.None;

            DecrementCooldowns();

            if (inputBuffer == null || !inputBuffer.TryDequeueAll(tickIndex, out List<SimInputEvent> events))
                return;

            for (int i = 0; i < events.Count; i++)
            {
                var evt = events[i];
                ApplyEvent(evt.key, evt.down);
                if (!evt.down) continue;

                owner?.RecordInputKey(FuncKeyMaskToNtsdCode(evt.key));
                SetEdgeCooldown(evt.key);
                UpdatePendingCommand(evt.key, tickIndex);
            }
        }

        public bool ApplyFrameInput(LF2Character character)
        {
            if (character == null || character.Frame?.D == null)
                return false;

            bool result = ApplyComboFrameInput(character);
            if (!result)
            {
                var command = ConsumeDirectFrameCommand(character, out bool handledByFrameHit);
                if (handledByFrameHit)
                {
                    result = true;
                }
                else if (command != NTSDInputCommand.None)
                {
                    result = character.ProcessReleaseInputCommand(command);
                }
            }

            return result;
        }

        public void OnStateExit()
        {
            _pendingCommand = NTSDInputCommand.None;
        }

        public void SetDefendLock(byte value)
        {
            _cdDefendLock = value;
        }

        private void DecrementCooldowns()
        {
            if (_cdRight > 0) _cdRight--;
            if (_cdLeft > 0) _cdLeft--;
            if (_cdUp > 0) _cdUp--;
            if (_cdDown > 0) _cdDown--;
            if (_cdAttack > 0) _cdAttack--;
            if (_cdJump > 0) _cdJump--;
            if (_cdDefend > 0) _cdDefend--;
            if (_cdDefendLock > 0) _cdDefendLock--;
        }

        private void ApplyEvent(FuncKeyMask key, bool down)
        {
            switch (key)
            {
                case FuncKeyMask.right: _right = down; break;
                case FuncKeyMask.left: _left = down; break;
                case FuncKeyMask.up: _up = down; break;
                case FuncKeyMask.down: _down = down; break;
                case FuncKeyMask.att: _attack = down; break;
                case FuncKeyMask.jump: _jump = down; break;
                case FuncKeyMask.def: _defend = down; break;
            }
        }

        private void SetEdgeCooldown(FuncKeyMask key)
        {
            switch (key)
            {
                case FuncKeyMask.right: if (!_prevRight) _cdRight = 5; break;
                case FuncKeyMask.left: if (!_prevLeft) _cdLeft = 5; break;
                case FuncKeyMask.up: if (!_prevUp) _cdUp = 5; break;
                case FuncKeyMask.down: if (!_prevDown) _cdDown = 5; break;
                case FuncKeyMask.att: if (!_prevAttack) _cdAttack = 5; break;
                case FuncKeyMask.jump: if (!_prevJump) _cdJump = 5; break;
                case FuncKeyMask.def: if (!_prevDefend) _cdDefend = 5; break;
            }
        }

        private void UpdatePendingCommand(FuncKeyMask key, int tickIndex)
        {
            switch (key)
            {
                case FuncKeyMask.left:
                case FuncKeyMask.right:
                    var runCommand = key == FuncKeyMask.left ? NTSDInputCommand.RunLeft : NTSDInputCommand.RunRight;
                    var walkCommand = key == FuncKeyMask.left ? NTSDInputCommand.Left : NTSDInputCommand.Right;
                    _pendingCommand = _lastDirectionTap == key && tickIndex - _lastDirectionTapTick <= 9
                        ? runCommand
                        : walkCommand;
                    _lastDirectionTap = key;
                    _lastDirectionTapTick = tickIndex;
                    break;
                case FuncKeyMask.up:
                    _pendingCommand = NTSDInputCommand.Up;
                    break;
                case FuncKeyMask.down:
                    _pendingCommand = NTSDInputCommand.Down;
                    break;
                case FuncKeyMask.att:
                    _pendingCommand = NTSDInputCommand.Attack;
                    break;
                case FuncKeyMask.jump:
                    _pendingCommand = NTSDInputCommand.Jump;
                    break;
                case FuncKeyMask.def:
                    _pendingCommand = NTSDInputCommand.Defend;
                    break;
            }
        }

        private bool ApplyComboFrameInput(LF2Character character)
        {
            bool result = false;

            result |= RunCombo(character, ref _comboDRA, _cdRight, ComboMode.Right, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Fa, "right");
            result |= RunCombo(character, ref _comboDLA, _cdLeft, ComboMode.Left, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Fa, "left");
            result |= RunCombo(character, ref _comboDUA, _cdUp, ComboMode.Up, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Ua, null);
            result |= RunCombo(character, ref _comboDDA, _cdDown, ComboMode.Down, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Da, null);
            result |= RunCombo(character, ref _comboDRJ, _cdRight, ComboMode.Right, _cdJump, ComboMode.Jump, character.Frame.D.hit_Fj, "right");
            result |= RunCombo(character, ref _comboDLJ, _cdLeft, ComboMode.Left, _cdJump, ComboMode.Jump, character.Frame.D.hit_Fj, "left");
            result |= RunCombo(character, ref _comboDUJ, _cdUp, ComboMode.Up, _cdJump, ComboMode.Jump, character.Frame.D.hit_Uj, null);
            result |= RunCombo(character, ref _comboDDJ, _cdDown, ComboMode.Down, _cdJump, ComboMode.Jump, character.Frame.D.hit_Dj, null);
            result |= RunDjaCombo(character);

            return result;
        }

        private bool RunCombo(
            LF2Character character,
            ref byte comboState,
            byte step2Cooldown,
            ComboMode step2Mode,
            byte step3Cooldown,
            ComboMode finalMode,
            int targetFrame,
            string facing)
        {
            bool advanced = false;
            AdvanceCombo(ref comboState, step2Cooldown, step2Mode, step3Cooldown, ref advanced);
            if (comboState != 3) return false;

            if (targetFrame != 0 && character.CanTriggerReleaseInputFrame())
            {
                if (!string.IsNullOrEmpty(facing))
                    character.SwitchDir(facing);
                character.TransitionToFrame(targetFrame, 10);
                character.MarkInputFrameConsumed();
                comboState = 0;
                return true;
            }

            if (ComboInterrupted(finalMode, advanced))
                comboState = 0;
            return false;
        }

        private bool RunDjaCombo(LF2Character character)
        {
            bool advanced = false;
            AdvanceCombo(ref _comboDJA, _cdJump, ComboMode.Jump, _cdAttack, ref advanced);
            if (_comboDJA != 3) return false;

            int targetFrame = character.Frame.D.hit_ja;
            if (targetFrame != 0 && character.CanTriggerReleaseInputFrame())
            {
                character.TransitionToFrame(targetFrame, 10);
                character.MarkInputFrameConsumed();
                _comboDJA = 0;
                return true;
            }

            if (ComboInterrupted(ComboMode.Attack, advanced))
                _comboDJA = 0;
            return false;
        }

        private void AdvanceCombo(ref byte comboState, byte step2Cooldown, ComboMode step2Mode, byte step3Cooldown, ref bool advanced)
        {
            advanced = false;
            if (comboState == 0 && _cdDefend == 5)
            {
                comboState = 1;
                advanced = true;
            }

            if (comboState == 1)
            {
                if (step2Cooldown == 5)
                {
                    comboState = 2;
                    advanced = true;
                }
                else if (ComboInterrupted(ComboMode.Defend, advanced))
                {
                    comboState = 0;
                }
            }

            if (comboState == 2)
            {
                if (step3Cooldown == 5)
                {
                    comboState = 3;
                    advanced = true;
                }
                else if (ComboInterrupted(step2Mode, advanced))
                {
                    comboState = 0;
                }
            }
        }

        private NTSDInputCommand ConsumeDirectFrameCommand(LF2Character character, out bool handledByFrameHit)
        {
            handledByFrameHit = false;
            if (character.Frame?.D == null)
                return ConsumePendingCommand();

            if (character.Frame.D.hit_a != 0 && _cdAttack > _cdDefend && _cdAttack > _cdJump)
            {
                _cdAttack = 0;
                character.TransitionToFrame(character.Frame.D.hit_a, 10);
                character.MarkInputFrameConsumed();
                handledByFrameHit = true;
                return NTSDInputCommand.None;
            }

            if (character.Frame.D.hit_d != 0 && _cdDefend > _cdAttack && _cdDefend > _cdJump)
            {
                _cdDefend = 0;
                character.TransitionToFrame(character.Frame.D.hit_d, 10);
                character.MarkInputFrameConsumed();
                handledByFrameHit = true;
                return NTSDInputCommand.None;
            }

            if (character.Frame.D.hit_j != 0 && _cdJump > _cdAttack && _cdJump > _cdDefend)
            {
                _cdJump = 0;
                character.TransitionToFrame(character.Frame.D.hit_j, 10);
                character.MarkInputFrameConsumed();
                handledByFrameHit = true;
                return NTSDInputCommand.None;
            }

            return ConsumePendingCommand();
        }

        private NTSDInputCommand ConsumePendingCommand()
        {
            var command = _pendingCommand;
            _pendingCommand = NTSDInputCommand.None;
            return command;
        }

        private bool ComboInterrupted(ComboMode mode, bool advancedThisWrapper)
        {
            if (!advancedThisWrapper)
            {
                return IsFreshAttackOrJumpOrDefend() || IsFreshDirection();
            }

            return mode switch
            {
                ComboMode.Up => IsFreshAttackOrJumpOrDefend() || _cdLeft == 5 || _cdDown == 5 || _cdRight == 5,
                ComboMode.Down => IsFreshAttackOrJumpOrDefend() || _cdLeft == 5 || _cdUp == 5 || _cdRight == 5,
                ComboMode.Left => IsFreshAttackOrJumpOrDefend() || _cdUp == 5 || _cdDown == 5 || _cdRight == 5,
                ComboMode.Right => IsFreshAttackOrJumpOrDefend() || _cdLeft == 5 || _cdUp == 5 || _cdDown == 5,
                ComboMode.Defend => _cdAttack == 5 || _cdJump == 5 || IsFreshDirection(),
                ComboMode.Jump => _cdAttack == 5 || _cdDefend == 5 || IsFreshDirection(),
                ComboMode.Attack => _cdJump == 5 || _cdDefend == 5 || IsFreshDirection(),
                _ => false,
            };
        }

        private bool IsFreshAttackOrJumpOrDefend() => _cdAttack == 5 || _cdJump == 5 || _cdDefend == 5;
        private bool IsFreshDirection() => _cdLeft == 5 || _cdUp == 5 || _cdDown == 5 || _cdRight == 5;

        private static int FuncKeyMaskToNtsdCode(FuncKeyMask key)
        {
            return key switch
            {
                FuncKeyMask.att => 9,
                FuncKeyMask.jump => 6,
                FuncKeyMask.down => 5,
                FuncKeyMask.def => 0,
                FuncKeyMask.left => 8,
                FuncKeyMask.right => 2,
                FuncKeyMask.up => 4,
                _ => -1,
            };
        }

        private enum ComboMode
        {
            Up,
            Down,
            Left,
            Right,
            Defend,
            Jump,
            Attack,
        }
    }
}
