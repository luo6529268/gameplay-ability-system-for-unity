using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;

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
            SimInputEventBatch events = default;
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
            SyncToRuntime(owner);
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

            bool result = ApplyFrameInputCore(character);
            SyncToRuntime(character);
            return result;
        }

        internal bool ApplyFrameInputFromSynchronizedRuntimeProgress(
            LF2Entity character)
        {
            if (character == null || character.Frame?.D == null)
                return false;

            bool result = ApplyFrameInputCore(character);
            SyncProgressToRuntime(character);
            return result;
        }

        private bool ApplyFrameInputCore(LF2Entity character)
        {
            BattleCharacterInputActionState input = CaptureActionState();
            BattleCharacterInputActionResolver resolver =
                character.RegisteredWorldForSimulation?.CharacterInputActionResolver ?? default;
            bool result = resolver.ApplyFrameInput(character, ref input);
            ApplyProgressFromActionState(in input);
            return result;
        }

        private void ApplyProgressFromActionState(
            in BattleCharacterInputActionState input)
        {
            _cdAttack = input.CdAttack;
            _cdJump = input.CdJump;
            _cdDefend = input.CdDefend;
            _cdDefendLock = input.CdDefendLock;
            _cdRight = input.CdRight;
            _cdLeft = input.CdLeft;
            _cdUp = input.CdUp;
            _cdDown = input.CdDown;
            _comboDRA = input.ComboDra;
            _comboDLA = input.ComboDla;
            _comboDUA = input.ComboDua;
            _comboDDA = input.ComboDda;
            _comboDRJ = input.ComboDrj;
            _comboDLJ = input.ComboDlj;
            _comboDUJ = input.ComboDuj;
            _comboDDJ = input.ComboDdj;
            _comboDJA = input.ComboDja;
        }

        private BattleCharacterInputActionState CaptureActionState()
        {
            return new BattleCharacterInputActionState
            {
                CdAttack = _cdAttack,
                CdJump = _cdJump,
                CdDefend = _cdDefend,
                CdDefendLock = _cdDefendLock,
                CdRight = _cdRight,
                CdLeft = _cdLeft,
                CdUp = _cdUp,
                CdDown = _cdDown,
                ComboDra = _comboDRA,
                ComboDla = _comboDLA,
                ComboDua = _comboDUA,
                ComboDda = _comboDDA,
                ComboDrj = _comboDRJ,
                ComboDlj = _comboDLJ,
                ComboDuj = _comboDUJ,
                ComboDdj = _comboDDJ,
                ComboDja = _comboDJA,
            };
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

        private void SyncProgressToRuntime(LF2Entity owner)
        {
            NTSDEntityRuntime runtime = owner?.Runtime;
            if (runtime == null)
                return;

            AiDecisionInputState input = CaptureRuntimeWriteState();
            BattleCharacterInputWriter writer =
                owner.RegisteredWorldForSimulation?.CharacterInputWriter;
            if (writer != null)
            {
                writer.CommitProgressState(runtime, input);
                return;
            }

            CommitProgressCompatibility(runtime, input);
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

            BattleCharacterInputWriter writer =
                owner.RegisteredWorldForSimulation?.CharacterInputWriter;
            if (writer != null)
                writer.PushInputHistory(owner.Runtime, code);
            else
                owner.Runtime.PushInputHistory(code);
        }

        private void SyncToRuntime(LF2Entity owner)
        {
            NTSDEntityRuntime runtime = owner?.Runtime;
            if (runtime == null)
                return;

            AiDecisionInputState input = CaptureRuntimeWriteState();
            BattleCharacterInputWriter writer =
                owner.RegisteredWorldForSimulation?.CharacterInputWriter;
            if (writer != null)
            {
                writer.CommitFullState(runtime, input);
                return;
            }

            CommitFullCompatibility(runtime, input);
        }

        private AiDecisionInputState CaptureRuntimeWriteState()
        {
            return new AiDecisionInputState
            {
                CdAttack = _cdAttack,
                CdJump = _cdJump,
                CdDefend = _cdDefend,
                CdDefendLock = _cdDefendLock,
                CdRight = _cdRight,
                CdLeft = _cdLeft,
                CdUp = _cdUp,
                CdDown = _cdDown,
                ComboDra = _comboDRA,
                ComboDla = _comboDLA,
                ComboDua = _comboDUA,
                ComboDda = _comboDDA,
                ComboDrj = _comboDRJ,
                ComboDlj = _comboDLJ,
                ComboDuj = _comboDUJ,
                ComboDdj = _comboDDJ,
                ComboDja = _comboDJA,
                PrevUp = _prevUp ? (byte)1 : (byte)0,
                PrevDown = _prevDown ? (byte)1 : (byte)0,
                PrevLeft = _prevLeft ? (byte)1 : (byte)0,
                PrevRight = _prevRight ? (byte)1 : (byte)0,
                PrevJump = _prevJump ? (byte)1 : (byte)0,
                PrevDefend = _prevDefend ? (byte)1 : (byte)0,
                PrevAttack = _prevAttack ? (byte)1 : (byte)0,
                KeyUp = _up ? (byte)1 : (byte)0,
                KeyDown = _down ? (byte)1 : (byte)0,
                KeyLeft = _left ? (byte)1 : (byte)0,
                KeyRight = _right ? (byte)1 : (byte)0,
                KeyAttack = _attack ? (byte)1 : (byte)0,
                KeyJump = _jump ? (byte)1 : (byte)0,
                KeyDefend = _defend ? (byte)1 : (byte)0,
            };
        }

        private void CommitFullCompatibility(
            NTSDEntityRuntime runtime,
            in AiDecisionInputState input)
        {
            runtime.PrevUp = input.PrevUp;
            runtime.PrevDown = input.PrevDown;
            runtime.PrevLeft = input.PrevLeft;
            runtime.PrevRight = input.PrevRight;
            runtime.PrevJump = input.PrevJump;
            runtime.PrevDefend = input.PrevDefend;
            runtime.PrevAttack = input.PrevAttack;
            runtime.KeyUp = input.KeyUp;
            runtime.KeyDown = input.KeyDown;
            runtime.KeyLeft = input.KeyLeft;
            runtime.KeyRight = input.KeyRight;
            runtime.KeyAttack = input.KeyAttack;
            runtime.KeyJump = input.KeyJump;
            runtime.KeyDefend = input.KeyDefend;
            CommitProgressCompatibility(runtime, input);
        }

        private void CommitProgressCompatibility(
            NTSDEntityRuntime runtime,
            in AiDecisionInputState input)
        {
            runtime.CdAttack = input.CdAttack;
            runtime.CdJump = input.CdJump;
            runtime.CdDefend = input.CdDefend;
            runtime.CdDefendLock = input.CdDefendLock;
            runtime.CdRight = input.CdRight;
            runtime.CdLeft = input.CdLeft;
            runtime.CdUp = input.CdUp;
            runtime.CdDown = input.CdDown;
            runtime.ComboDra = input.ComboDra;
            runtime.ComboDla = input.ComboDla;
            runtime.ComboDua = input.ComboDua;
            runtime.ComboDda = input.ComboDda;
            runtime.ComboDrj = input.ComboDrj;
            runtime.ComboDlj = input.ComboDlj;
            runtime.ComboDuj = input.ComboDuj;
            runtime.ComboDdj = input.ComboDdj;
            runtime.ComboDja = input.ComboDja;
        }

    }
}
