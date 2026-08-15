using System;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleCharacterInputAiProjection
    {
        internal BattleCharacterInputAiProjection(
            bool inputHistoryGate,
            int cachedTargetSlot,
            int coordinateTargetX,
            int decisionBoundaryFlags)
        {
            InputHistoryGate = inputHistoryGate;
            CachedTargetSlot = cachedTargetSlot;
            CoordinateTargetX = coordinateTargetX;
            DecisionBoundaryFlags = decisionBoundaryFlags;
        }

        internal bool InputHistoryGate { get; }
        internal int CachedTargetSlot { get; }
        internal int CoordinateTargetX { get; }
        internal int DecisionBoundaryFlags { get; }
    }

    /// <summary>
    /// Persistent slot/generation-owned storage for character input state.
    /// The AI kernel consumes the complete row together, so rows are stored as
    /// contiguous value types rather than reconstructed from scattered arrays.
    /// Runtime objects remain compatibility mirrors during U6.
    /// </summary>
    internal sealed class BattleCharacterInputStore
    {
        private readonly BattleAiUnifiedRowPublisher unifiedRowPublisher;
        private NTSDEntityRuntime[] owners;
        private uint[] generations;
        private AiDecisionInputState[] inputs;

        internal long LastAiProjectionPublicationCountForDiagnostics { get; private set; }
        internal long LastAiProjectionPublicationSkipCountForDiagnostics { get; private set; }

        internal BattleCharacterInputStore(
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            this.unifiedRowPublisher = unifiedRowPublisher ??
                throw new ArgumentNullException(nameof(unifiedRowPublisher));
            owners = new NTSDEntityRuntime[capacity];
            generations = new uint[capacity];
            inputs = new AiDecisionInputState[capacity];
        }

        internal int Capacity => owners.Length;

        internal void Bind(
            NTSDEntityRuntime runtime,
            RuntimeEntityHandle handle)
        {
            if (runtime == null ||
                !handle.IsValid ||
                handle.Slot >= owners.Length ||
                runtime.SlotIndex != handle.Slot)
            {
                throw new InvalidOperationException(
                    "Character input store requires a current runtime handle.");
            }

            int slot = handle.Slot;
            owners[slot] = runtime;
            generations[slot] = handle.Generation;
            inputs[slot] = Capture(runtime);
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            if (!handle.IsValid ||
                handle.Slot >= owners.Length ||
                generations[handle.Slot] != handle.Generation)
            {
                return;
            }

            int slot = handle.Slot;
            owners[slot] = null;
            generations[slot] = 0;
            inputs[slot] = default;
        }

        internal void Reset()
        {
            Array.Clear(owners, 0, owners.Length);
            Array.Clear(generations, 0, generations.Length);
            Array.Clear(inputs, 0, inputs.Length);
        }

        internal void GrowTo(int capacity)
        {
            if (capacity <= owners.Length)
                return;

            Array.Resize(ref owners, capacity);
            Array.Resize(ref generations, capacity);
            Array.Resize(ref inputs, capacity);
        }

        internal bool TryCaptureCommon(
            NTSDEntityRuntime runtime,
            out AiDecisionInputState input)
        {
            if (!TryResolve(runtime, out int slot))
            {
                input = default;
                return false;
            }

            input = inputs[slot];
            return true;
        }

        internal bool CanEvaluateCanonicalDecision(NTSDEntityRuntime runtime)
        {
            return TryResolve(runtime, out _);
        }

        internal bool TryEvaluateCanonicalDecision(
            NTSDEntityRuntime runtime,
            AiDecisionSnapshot snapshot,
            AiDecisionEvaluationPolicy policy,
            bool captureRngTrace,
            BattleAiInputDetailDiagnostics diagnostics,
            ref AiDecisionWitness witness)
        {
            if (!TryResolve(runtime, out int slot))
            {
                witness = default;
                witness.Availability = AiDecisionAvailability.SnapshotMissing;
                return false;
            }

            return AiDecisionKernel.TryEvaluateCanonicalInput(
                snapshot,
                in inputs[slot],
                policy,
                captureRngTrace,
                diagnostics,
                ref witness);
        }

        internal bool TryCaptureAiProjection(
            NTSDEntityRuntime runtime,
            out BattleCharacterInputAiProjection projection)
        {
            if (!TryResolve(runtime, out int slot))
            {
                projection = default;
                return false;
            }

            ref AiDecisionInputState row = ref inputs[slot];
            projection = new BattleCharacterInputAiProjection(
                row.History0 != 0,
                row.Unk360,
                row.Unk3FC,
                row.BoundaryFlags);
            return true;
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleCharacterInputAiProjection projection)
        {
            if (!TryResolve(handle, out int slot))
            {
                projection = default;
                return false;
            }

            ref AiDecisionInputState row = ref inputs[slot];
            projection = new BattleCharacterInputAiProjection(
                row.History0 != 0,
                row.Unk360,
                row.Unk3FC,
                row.BoundaryFlags);
            return true;
        }

        internal bool TryCaptureProgress(
            NTSDEntityRuntime runtime,
            out BattleCharacterInputActionState input)
        {
            input = default;
            if (!TryResolve(runtime, out int slot))
                return false;

            ref AiDecisionInputState row = ref inputs[slot];
            input.CdAttack = row.CdAttack;
            input.CdJump = row.CdJump;
            input.CdDefend = row.CdDefend;
            input.CdDefendLock = row.CdDefendLock;
            input.CdRight = row.CdRight;
            input.CdLeft = row.CdLeft;
            input.CdUp = row.CdUp;
            input.CdDown = row.CdDown;
            input.ComboDra = row.ComboDra;
            input.ComboDla = row.ComboDla;
            input.ComboDua = row.ComboDua;
            input.ComboDda = row.ComboDda;
            input.ComboDrj = row.ComboDrj;
            input.ComboDlj = row.ComboDlj;
            input.ComboDuj = row.ComboDuj;
            input.ComboDdj = row.ComboDdj;
            input.ComboDja = row.ComboDja;
            return true;
        }

        internal void CommitFull(
            NTSDEntityRuntime runtime,
            in AiDecisionInputState input,
            bool includeHistory)
        {
            if (!TryResolve(runtime, out int slot))
                return;

            if (includeHistory)
            {
                ref AiDecisionInputState current = ref inputs[slot];
                bool previousHistoryGate = current.History0 != 0;
                int previousCachedTargetSlot = current.Unk360;
                int previousCoordinateTargetX = current.Unk3FC;
                inputs[slot] = input;
                PublishAiProjectionIfChanged(
                    slot,
                    previousHistoryGate,
                    previousCachedTargetSlot,
                    previousCoordinateTargetX);
                return;
            }

            ref AiDecisionInputState row = ref inputs[slot];
            CopyHeldAndPrevious(ref row, input);
            CopyProgress(ref row, input);
        }

        internal void CommitProgress(
            NTSDEntityRuntime runtime,
            in AiDecisionInputState input)
        {
            if (TryResolve(runtime, out int slot))
                CopyProgress(ref inputs[slot], input);
        }

        internal void CommitProgress(
            NTSDEntityRuntime runtime,
            in BattleCharacterInputActionState input)
        {
            if (!TryResolve(runtime, out int slot))
                return;

            ref AiDecisionInputState row = ref inputs[slot];
            row.CdAttack = input.CdAttack;
            row.CdJump = input.CdJump;
            row.CdDefend = input.CdDefend;
            row.CdDefendLock = input.CdDefendLock;
            row.CdRight = input.CdRight;
            row.CdLeft = input.CdLeft;
            row.CdUp = input.CdUp;
            row.CdDown = input.CdDown;
            row.ComboDra = input.ComboDra;
            row.ComboDla = input.ComboDla;
            row.ComboDua = input.ComboDua;
            row.ComboDda = input.ComboDda;
            row.ComboDrj = input.ComboDrj;
            row.ComboDlj = input.ComboDlj;
            row.ComboDuj = input.ComboDuj;
            row.ComboDdj = input.ComboDdj;
            row.ComboDja = input.ComboDja;
        }

        internal void RollAndClearCurrentKeys(NTSDEntityRuntime runtime)
        {
            if (!TryResolve(runtime, out int slot))
                return;

            ref AiDecisionInputState row = ref inputs[slot];
            row.PrevUp = row.KeyUp;
            row.PrevDown = row.KeyDown;
            row.PrevLeft = row.KeyLeft;
            row.PrevRight = row.KeyRight;
            row.PrevJump = row.KeyJump;
            row.PrevDefend = row.KeyDefend;
            row.PrevAttack = row.KeyAttack;
            ClearCurrentKeys(ref row);
        }

        internal void ClearCurrentKeys(NTSDEntityRuntime runtime)
        {
            if (TryResolve(runtime, out int slot))
                ClearCurrentKeys(ref inputs[slot]);
        }

        internal void ApplyInputEdges(NTSDEntityRuntime runtime)
        {
            if (!TryResolve(runtime, out int slot))
                return;

            ref AiDecisionInputState row = ref inputs[slot];
            if (row.PrevRight == 0 && row.KeyRight == 1)
            {
                row.CdRight = 5;
                PushHistory(ref row, 6);
            }
            if (row.PrevLeft == 0 && row.KeyLeft == 1)
            {
                row.CdLeft = 5;
                PushHistory(ref row, 4);
            }
            if (row.PrevUp == 0 && row.KeyUp == 1)
            {
                row.CdUp = 5;
                PushHistory(ref row, 8);
            }
            if (row.PrevDown == 0 && row.KeyDown == 1)
            {
                row.CdDown = 5;
                PushHistory(ref row, 2);
            }
            if (row.PrevAttack == 0 && row.KeyAttack == 1)
            {
                row.CdDefend = 5;
                PushHistory(ref row, 9);
            }
            if (row.PrevDefend == 0 && row.KeyDefend == 1)
            {
                row.CdJump = 5;
                PushHistory(ref row, 0);
            }
            if (row.PrevJump == 0 && row.KeyJump == 1)
            {
                row.CdAttack = 5;
                PushHistory(ref row, 5);
            }
        }

        internal void PushInputHistory(NTSDEntityRuntime runtime, int keyCode)
        {
            if (TryResolve(runtime, out int slot))
                PushHistory(ref inputs[slot], keyCode);
        }

        internal void SetInputHistoryGate(NTSDEntityRuntime runtime, bool enabled)
        {
            if (TryResolve(runtime, out int slot))
            {
                ref AiDecisionInputState row = ref inputs[slot];
                bool previousHistoryGate = row.History0 != 0;
                int previousCachedTargetSlot = row.Unk360;
                int previousCoordinateTargetX = row.Unk3FC;
                inputs[slot].History0 = enabled ? 1 : 0;
                PublishAiProjectionIfChanged(
                    slot,
                    previousHistoryGate,
                    previousCachedTargetSlot,
                    previousCoordinateTargetX);
            }
        }

        internal void ClearInputHistoryTail(NTSDEntityRuntime runtime)
        {
            if (!TryResolve(runtime, out int slot))
                return;

            ref AiDecisionInputState row = ref inputs[slot];
            row.History1 = 0;
            row.History2 = 0;
            row.History3 = 0;
            row.History4 = 0;
            row.History5 = 0;
        }

        internal void SetDefendLock(NTSDEntityRuntime runtime, byte value)
        {
            if (TryResolve(runtime, out int slot))
                inputs[slot].CdDefendLock = value;
        }

        internal void ResetInputState(NTSDEntityRuntime runtime)
        {
            if (!TryResolve(runtime, out int slot))
                return;

            ref AiDecisionInputState row = ref inputs[slot];
            bool previousHistoryGate = row.History0 != 0;
            int previousCachedTargetSlot = row.Unk360;
            int previousCoordinateTargetX = row.Unk3FC;
            int unk360 = row.Unk360;
            int unk3FC = row.Unk3FC;
            int unk400 = row.Unk400;
            int boundaryFlags = row.BoundaryFlags;
            row = default;
            row.Unk360 = unk360;
            row.Unk3FC = unk3FC;
            row.Unk400 = unk400;
            row.BoundaryFlags = boundaryFlags;
            PublishAiProjectionIfChanged(
                slot,
                previousHistoryGate,
                previousCachedTargetSlot,
                previousCoordinateTargetX);
        }

        internal void SetCoordinateTarget(
            NTSDEntityRuntime runtime,
            int x,
            int z)
        {
            if (!TryResolve(runtime, out int slot))
                return;

            ref AiDecisionInputState row = ref inputs[slot];
            bool previousHistoryGate = row.History0 != 0;
            int previousCachedTargetSlot = row.Unk360;
            int previousCoordinateTargetX = row.Unk3FC;
            inputs[slot].Unk3FC = x;
            inputs[slot].Unk400 = z;
            PublishAiProjectionIfChanged(
                slot,
                previousHistoryGate,
                previousCachedTargetSlot,
                previousCoordinateTargetX);
        }

        internal void ResetAiProjectionPublicationDiagnostics()
        {
            LastAiProjectionPublicationCountForDiagnostics = 0;
            LastAiProjectionPublicationSkipCountForDiagnostics = 0;
        }

        internal void SetBoundaryFlags(
            NTSDEntityRuntime runtime,
            int flags)
        {
            if (!TryResolve(runtime, out int slot) ||
                inputs[slot].BoundaryFlags == flags)
            {
                return;
            }

            inputs[slot].BoundaryFlags = flags;
            unifiedRowPublisher.PublishDecisionBoundaryFlags(
                slot,
                generations[slot],
                flags);
        }

        private bool TryResolve(NTSDEntityRuntime runtime, out int slot)
        {
            slot = runtime?.SlotIndex ?? -1;
            return (uint)slot < (uint)owners.Length &&
                   generations[slot] != 0 &&
                   ReferenceEquals(owners[slot], runtime);
        }

        private bool TryResolve(RuntimeEntityHandle handle, out int slot)
        {
            slot = handle.Slot;
            return handle.IsValid &&
                   (uint)slot < (uint)owners.Length &&
                   generations[slot] == handle.Generation;
        }

        private void PublishAiProjection(int slot)
        {
            ref AiDecisionInputState row = ref inputs[slot];
            unifiedRowPublisher.PublishInputProjection(
                slot,
                generations[slot],
                row.History0 != 0,
                row.Unk360,
                row.Unk3FC);
        }

        private void PublishAiProjectionIfChanged(
            int slot,
            bool previousHistoryGate,
            int previousCachedTargetSlot,
            int previousCoordinateTargetX)
        {
            if (!unifiedRowPublisher.Active)
                return;

            ref AiDecisionInputState row = ref inputs[slot];
            bool changed = previousHistoryGate != (row.History0 != 0) ||
                           previousCachedTargetSlot != row.Unk360 ||
                           previousCoordinateTargetX != row.Unk3FC;
            if (!changed)
            {
                LastAiProjectionPublicationSkipCountForDiagnostics++;
                return;
            }

            LastAiProjectionPublicationCountForDiagnostics++;
            PublishAiProjection(slot);
        }

        private static AiDecisionInputState Capture(NTSDEntityRuntime runtime)
        {
            int[] history = runtime.InputHistory;
            return new AiDecisionInputState
            {
                History0 = ReadHistory(history, 0),
                History1 = ReadHistory(history, 1),
                History2 = ReadHistory(history, 2),
                History3 = ReadHistory(history, 3),
                History4 = ReadHistory(history, 4),
                History5 = ReadHistory(history, 5),
                CdAttack = runtime.CdAttack,
                CdJump = runtime.CdJump,
                CdDefend = runtime.CdDefend,
                CdDefendLock = runtime.CdDefendLock,
                CdRight = runtime.CdRight,
                CdLeft = runtime.CdLeft,
                CdUp = runtime.CdUp,
                CdDown = runtime.CdDown,
                ComboDra = runtime.ComboDra,
                ComboDla = runtime.ComboDla,
                ComboDua = runtime.ComboDua,
                ComboDda = runtime.ComboDda,
                ComboDrj = runtime.ComboDrj,
                ComboDlj = runtime.ComboDlj,
                ComboDuj = runtime.ComboDuj,
                ComboDdj = runtime.ComboDdj,
                ComboDja = runtime.ComboDja,
                PrevUp = runtime.PrevUp,
                PrevDown = runtime.PrevDown,
                PrevLeft = runtime.PrevLeft,
                PrevRight = runtime.PrevRight,
                PrevJump = runtime.PrevJump,
                PrevDefend = runtime.PrevDefend,
                PrevAttack = runtime.PrevAttack,
                KeyUp = runtime.KeyUp,
                KeyDown = runtime.KeyDown,
                KeyLeft = runtime.KeyLeft,
                KeyRight = runtime.KeyRight,
                KeyAttack = runtime.KeyAttack,
                KeyJump = runtime.KeyJump,
                KeyDefend = runtime.KeyDefend,
                Unk360 = runtime.Unk360,
                Unk3FC = runtime.Unk3FC,
                Unk400 = runtime.Unk400,
                BoundaryFlags = CaptureBoundaryFlags(runtime),
            };
        }

        private static void CopyHeldAndPrevious(
            ref AiDecisionInputState destination,
            in AiDecisionInputState source)
        {
            destination.PrevUp = source.PrevUp;
            destination.PrevDown = source.PrevDown;
            destination.PrevLeft = source.PrevLeft;
            destination.PrevRight = source.PrevRight;
            destination.PrevJump = source.PrevJump;
            destination.PrevDefend = source.PrevDefend;
            destination.PrevAttack = source.PrevAttack;
            destination.KeyUp = source.KeyUp;
            destination.KeyDown = source.KeyDown;
            destination.KeyLeft = source.KeyLeft;
            destination.KeyRight = source.KeyRight;
            destination.KeyAttack = source.KeyAttack;
            destination.KeyJump = source.KeyJump;
            destination.KeyDefend = source.KeyDefend;
        }

        private static void CopyProgress(
            ref AiDecisionInputState destination,
            in AiDecisionInputState source)
        {
            destination.CdAttack = source.CdAttack;
            destination.CdJump = source.CdJump;
            destination.CdDefend = source.CdDefend;
            destination.CdDefendLock = source.CdDefendLock;
            destination.CdRight = source.CdRight;
            destination.CdLeft = source.CdLeft;
            destination.CdUp = source.CdUp;
            destination.CdDown = source.CdDown;
            destination.ComboDra = source.ComboDra;
            destination.ComboDla = source.ComboDla;
            destination.ComboDua = source.ComboDua;
            destination.ComboDda = source.ComboDda;
            destination.ComboDrj = source.ComboDrj;
            destination.ComboDlj = source.ComboDlj;
            destination.ComboDuj = source.ComboDuj;
            destination.ComboDdj = source.ComboDdj;
            destination.ComboDja = source.ComboDja;
        }

        private static void ClearCurrentKeys(ref AiDecisionInputState input)
        {
            input.KeyUp = 0;
            input.KeyDown = 0;
            input.KeyLeft = 0;
            input.KeyRight = 0;
            input.KeyAttack = 0;
            input.KeyJump = 0;
            input.KeyDefend = 0;
        }

        private static void PushHistory(
            ref AiDecisionInputState input,
            int keyCode)
        {
            input.History1 = input.History2;
            input.History2 = input.History3;
            input.History3 = input.History4;
            input.History4 = input.History5;
            input.History5 = keyCode;
        }

        private static int CaptureBoundaryFlags(NTSDEntityRuntime runtime)
        {
            int flags = 0;
            if (runtime.XBoundPositive)
                flags |= 1;
            if (runtime.XBoundNegative)
                flags |= 2;
            if (runtime.ZBoundPositive)
                flags |= 4;
            if (runtime.ZBoundNegative)
                flags |= 8;
            return flags;
        }

        private static int ReadHistory(int[] history, int index)
        {
            return history != null && (uint)index < (uint)history.Length
                ? history[index]
                : 0;
        }
    }
}
