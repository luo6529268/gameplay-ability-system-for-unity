using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Input
{
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

        public bool Right => _right;
        public bool Left => _left;
        public bool Up => _up;
        public bool Down => _down;
        public bool Attack => _attack;
        public bool Jump => _jump;
        public bool Defend => _defend;
        public bool PreviousRight => _prevRight;
        public bool PreviousLeft => _prevLeft;
        public bool PreviousJump => _prevJump;
        public int RightCooldown => _cdRight;
        public int LeftCooldown => _cdLeft;
        public int UpCooldown => _cdUp;
        public int DownCooldown => _cdDown;
        public int AttackCooldown => _cdAttack;
        public int JumpCooldown => _cdJump;
        public int DefendCooldown => _cdDefend;

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
        }

        public void UpdateFromBuffer(SimInputBuffer inputBuffer, int tickIndex, LF2Entity owner)
        {
            List<SimInputEvent> events = null;
            bool hasEvents = inputBuffer != null &&
                             inputBuffer.TryDequeueAll(tickIndex, out events);
            bool hasCompletePacket = false;
            if (hasEvents)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    if (!events[i].completePacket)
                        continue;

                    hasCompletePacket = true;
                    break;
                }
            }

            if (hasCompletePacket)
                SetHeldStateFromRuntime(owner?.Runtime);

            _prevRight = _right;
            _prevLeft = _left;
            _prevUp = _up;
            _prevDown = _down;
            _prevAttack = _attack;
            _prevJump = _jump;
            _prevDefend = _defend;

            if (hasEvents)
            {
                for (int i = 0; i < events.Count; i++)
                {
                    SimInputEvent evt = events[i];
                    if (hasCompletePacket && !evt.completePacket)
                        continue;
                    ApplyEvent(evt.key, evt.down);
                }
            }

            DecrementCooldowns();
            ApplyNewPressEdges(owner);
        }

        internal void PollFromBuffer(SimInputBuffer inputBuffer, int tickIndex, LF2Entity owner)
        {
            UpdateFromBuffer(inputBuffer, tickIndex, owner);
            SyncToRuntime(owner?.Runtime);
        }

        private void SetHeldStateFromRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            _right = runtime.KeyRight != 0;
            _left = runtime.KeyLeft != 0;
            _up = runtime.KeyUp != 0;
            _down = runtime.KeyDown != 0;
            _attack = runtime.KeyAttack != 0;
            _jump = runtime.KeyJump != 0;
            _defend = runtime.KeyDefend != 0;
        }

        public bool ApplyFrameInput(LF2Entity character)
        {
            if (character == null || character.Frame?.D == null)
                return false;

            bool result = ApplyComboFrameInput(character);
            result |= ApplyDirectFrameInput(character);
            if (character is LF2Character realCharacter)
                result |= realCharacter.ProcessReleaseInput();
            SyncToRuntime(character.Runtime);
            return result;
        }

        internal void SyncFromRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            _right = runtime.KeyRight != 0;
            _left = runtime.KeyLeft != 0;
            _up = runtime.KeyUp != 0;
            _down = runtime.KeyDown != 0;
            _attack = runtime.KeyAttack != 0;
            _jump = runtime.KeyJump != 0;
            _defend = runtime.KeyDefend != 0;

            _prevRight = runtime.PrevRight != 0;
            _prevLeft = runtime.PrevLeft != 0;
            _prevUp = runtime.PrevUp != 0;
            _prevDown = runtime.PrevDown != 0;
            _prevAttack = runtime.PrevAttack != 0;
            _prevJump = runtime.PrevJump != 0;
            _prevDefend = runtime.PrevDefend != 0;

            SyncProgressFromRuntime(runtime);
        }

        internal void SyncProgressFromRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            _cdRight = runtime.CdRight;
            _cdLeft = runtime.CdLeft;
            _cdUp = runtime.CdUp;
            _cdDown = runtime.CdDown;
            _cdAttack = runtime.CdAttack;
            _cdJump = runtime.CdJump;
            _cdDefend = runtime.CdDefend;
            _cdDefendLock = runtime.CdDefendLock;

            _comboDRA = runtime.ComboDra;
            _comboDLA = runtime.ComboDla;
            _comboDUA = runtime.ComboDua;
            _comboDDA = runtime.ComboDda;
            _comboDRJ = runtime.ComboDrj;
            _comboDLJ = runtime.ComboDlj;
            _comboDUJ = runtime.ComboDuj;
            _comboDDJ = runtime.ComboDdj;
            _comboDJA = runtime.ComboDja;
        }

        public void OnStateExit()
        {
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

        private void ApplyNewPressEdges(LF2Entity owner)
        {
            ApplyNewPressEdge(_prevRight, _right, ref _cdRight, owner, FuncKeyMask.right);
            ApplyNewPressEdge(_prevLeft, _left, ref _cdLeft, owner, FuncKeyMask.left);
            ApplyNewPressEdge(_prevUp, _up, ref _cdUp, owner, FuncKeyMask.up);
            ApplyNewPressEdge(_prevDown, _down, ref _cdDown, owner, FuncKeyMask.down);
            ApplyNewPressEdge(_prevAttack, _attack, ref _cdDefend, owner, FuncKeyMask.att);
            ApplyNewPressEdge(_prevDefend, _defend, ref _cdJump, owner, FuncKeyMask.def);
            ApplyNewPressEdge(_prevJump, _jump, ref _cdAttack, owner, FuncKeyMask.jump);
        }

        private static void ApplyNewPressEdge(
            bool previous,
            bool held,
            ref byte cooldown,
            LF2Entity owner,
            FuncKeyMask key)
        {
            if (previous || !held)
                return;

            cooldown = 5;
            PushInputHistory(owner, key);
        }

        private bool ApplyComboFrameInput(LF2Entity character)
        {
            if (_cdDefend != 5 &&
                _comboDRA == 0 &&
                _comboDLA == 0 &&
                _comboDUA == 0 &&
                _comboDDA == 0 &&
                _comboDRJ == 0 &&
                _comboDLJ == 0 &&
                _comboDUJ == 0 &&
                _comboDDJ == 0 &&
                _comboDJA == 0)
            {
                return false;
            }

            // C# authority RunComboWrappers advances all nine combo values as one local
            // transaction. Only the final DJA fallthrough commits them; every earlier
            // return keeps frame/facing/cooldown side effects but discards local progress.
            byte comboDRA = _comboDRA;
            byte comboDLA = _comboDLA;
            byte comboDUA = _comboDUA;
            byte comboDDA = _comboDDA;
            byte comboDRJ = _comboDRJ;
            byte comboDLJ = _comboDLJ;
            byte comboDUJ = _comboDUJ;
            byte comboDDJ = _comboDDJ;
            byte comboDJA = _comboDJA;
            bool result = false;

            result |= RunCombo(character, ref comboDRA, _cdRight, ComboMode.Right, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Fa, "right");
            result |= RunCombo(character, ref comboDLA, _cdLeft, ComboMode.Left, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Fa, "left");
            result |= RunCombo(character, ref comboDUA, _cdUp, ComboMode.Up, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Ua, null);
            result |= RunCombo(character, ref comboDDA, _cdDown, ComboMode.Down, _cdAttack, ComboMode.Attack, character.Frame.D.hit_Da, null);
            result |= RunCombo(character, ref comboDRJ, _cdRight, ComboMode.Right, _cdJump, ComboMode.Jump, character.Frame.D.hit_Fj, "right");
            result |= RunCombo(character, ref comboDLJ, _cdLeft, ComboMode.Left, _cdJump, ComboMode.Jump, character.Frame.D.hit_Fj, "left");
            result |= RunCombo(character, ref comboDUJ, _cdUp, ComboMode.Up, _cdJump, ComboMode.Jump, character.Frame.D.hit_Uj, null);
            result |= RunCombo(character, ref comboDDJ, _cdDown, ComboMode.Down, _cdJump, ComboMode.Jump, character.Frame.D.hit_Dj, null);

            bool djaAdvanced = false;
            AdvanceCombo(ref comboDJA, _cdJump, ComboMode.Jump, _cdAttack, ref djaAdvanced);
            if (character.Frame?.D == null || comboDJA != 3)
                return result;

            int targetFrame = character.Frame.D.hit_ja;
            if (character.ShouldHoldCharacterDatDjaInputGuard(targetFrame))
                return result;

            if (targetFrame != 0 && character.CanEnterCharacterDatInputFrameJump())
            {
                bool jumped = character.TryCharacterDatInputFrameJump(targetFrame);
                comboDJA = 0;
                if (jumped)
                    ClearActionAndDirectionCooldowns();
                return true;
            }

            if (character.Runtime?.Unk328 == 1)
            {
                character.Runtime.Unk338 = 0;
                return result;
            }

            if (ComboInterrupted(ComboMode.Attack, djaAdvanced))
                comboDJA = 0;

            CommitComboProgress(
                comboDRA,
                comboDLA,
                comboDUA,
                comboDDA,
                comboDRJ,
                comboDLJ,
                comboDUJ,
                comboDDJ,
                comboDJA);

            return result;
        }

        private void CommitComboProgress(
            byte comboDRA,
            byte comboDLA,
            byte comboDUA,
            byte comboDDA,
            byte comboDRJ,
            byte comboDLJ,
            byte comboDUJ,
            byte comboDDJ,
            byte comboDJA)
        {
            _comboDRA = comboDRA;
            _comboDLA = comboDLA;
            _comboDUA = comboDUA;
            _comboDDA = comboDDA;
            _comboDRJ = comboDRJ;
            _comboDLJ = comboDLJ;
            _comboDUJ = comboDUJ;
            _comboDDJ = comboDDJ;
            _comboDJA = comboDJA;
        }

        private bool RunCombo(
            LF2Entity character,
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

            if (targetFrame != 0 && character.Runtime.LinkState != 2)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(targetFrame);
                if (!string.IsNullOrEmpty(facing))
                    character.SwitchDir(facing);
                comboState = 0;
                if (jumped) ClearActionAndDirectionCooldowns();
                return true;
            }

            if (ComboInterrupted(finalMode, advanced))
                comboState = 0;
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

        private bool ApplyDirectFrameInput(LF2Entity character)
        {
            if (character.Frame?.D == null)
                return false;

            if (character.Frame.D.hit_a != 0 && _cdAttack > _cdDefend && _cdAttack > _cdJump)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(character.Frame.D.hit_a);
                if (jumped) ClearActionAndDirectionCooldowns();
                _cdAttack = 0;
                return true;
            }

            if (character.Frame.D.hit_d != 0 && _cdDefend > _cdAttack && _cdDefend > _cdJump)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(character.Frame.D.hit_d);
                if (jumped) ClearActionAndDirectionCooldowns();
                _cdDefend = 0;
                return true;
            }

            if (character.Frame.D.hit_j != 0 && _cdJump > _cdAttack && _cdJump > _cdDefend)
            {
                bool jumped = character.TryCharacterDatInputFrameJump(character.Frame.D.hit_j);
                if (jumped) ClearActionAndDirectionCooldowns();
                _cdJump = 0;
                return true;
            }

            return false;
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

        private void ClearActionAndDirectionCooldowns()
        {
            _cdRight = _cdLeft = _cdUp = _cdDown = 0;
            _cdAttack = _cdJump = _cdDefend = 0;
        }

        private static int FuncKeyMaskToNtsdCode(FuncKeyMask key)
        {
            return key switch
            {
                // C# authority input_history 编码：right=6,left=4,up=8,down=2,attack=9,defend=0,jump=5。
                FuncKeyMask.att => 9,
                FuncKeyMask.jump => 5,
                FuncKeyMask.down => 2,
                FuncKeyMask.def => 0,
                FuncKeyMask.left => 4,
                FuncKeyMask.right => 6,
                FuncKeyMask.up => 8,
                _ => -1,
            };
        }

        private static void PushInputHistory(LF2Entity owner, FuncKeyMask key)
        {
            if (owner?.Runtime?.InputHistory == null || owner.Runtime.InputHistory.Length < 6)
                return;

            int code = FuncKeyMaskToNtsdCode(key);
            if (code < 0)
                return;

            owner.Runtime.PushInputHistory(code);
        }

        private void SyncToRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            runtime.KeyRight = _right ? (byte)1 : (byte)0;
            runtime.KeyLeft = _left ? (byte)1 : (byte)0;
            runtime.KeyUp = _up ? (byte)1 : (byte)0;
            runtime.KeyDown = _down ? (byte)1 : (byte)0;
            runtime.KeyAttack = _attack ? (byte)1 : (byte)0;
            runtime.KeyJump = _jump ? (byte)1 : (byte)0;
            runtime.KeyDefend = _defend ? (byte)1 : (byte)0;

            runtime.PrevRight = _prevRight ? (byte)1 : (byte)0;
            runtime.PrevLeft = _prevLeft ? (byte)1 : (byte)0;
            runtime.PrevUp = _prevUp ? (byte)1 : (byte)0;
            runtime.PrevDown = _prevDown ? (byte)1 : (byte)0;
            runtime.PrevAttack = _prevAttack ? (byte)1 : (byte)0;
            runtime.PrevJump = _prevJump ? (byte)1 : (byte)0;
            runtime.PrevDefend = _prevDefend ? (byte)1 : (byte)0;

            runtime.CdRight = _cdRight;
            runtime.CdLeft = _cdLeft;
            runtime.CdUp = _cdUp;
            runtime.CdDown = _cdDown;
            runtime.CdAttack = _cdAttack;
            runtime.CdJump = _cdJump;
            runtime.CdDefend = _cdDefend;
            runtime.CdDefendLock = _cdDefendLock;

            runtime.ComboDra = _comboDRA;
            runtime.ComboDla = _comboDLA;
            runtime.ComboDua = _comboDUA;
            runtime.ComboDda = _comboDDA;
            runtime.ComboDrj = _comboDRJ;
            runtime.ComboDlj = _comboDLJ;
            runtime.ComboDuj = _comboDUJ;
            runtime.ComboDdj = _comboDDJ;
            runtime.ComboDja = _comboDJA;
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
