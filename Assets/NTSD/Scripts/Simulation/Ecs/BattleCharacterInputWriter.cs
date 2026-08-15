using System;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns the common held, previous, cooldown and combo publication shared by
    /// human input adapters and canonical AI decisions. AI-only history, target,
    /// flow and RNG state remain owned by <see cref="BattleAiInputWriter"/>.
    /// </summary>
    internal sealed class BattleCharacterInputWriter
    {
        private readonly SimulationWorld world;
        private readonly BattleCharacterInputStore store;

        internal BattleCharacterInputWriter(
            SimulationWorld world,
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            store = new BattleCharacterInputStore(capacity, unifiedRowPublisher);
        }

        internal void Bind(
            NTSDEntityRuntime runtime,
            RuntimeEntityHandle handle)
        {
            store.Bind(runtime, handle);
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            store.Release(handle);
        }

        internal void Reset()
        {
            store.Reset();
        }

        internal void GrowTo(int capacity)
        {
            store.GrowTo(capacity);
        }

        internal long LastAiProjectionPublicationCountForDiagnostics =>
            store.LastAiProjectionPublicationCountForDiagnostics;

        internal long LastAiProjectionPublicationSkipCountForDiagnostics =>
            store.LastAiProjectionPublicationSkipCountForDiagnostics;

        internal void ResetAiProjectionPublicationDiagnostics()
        {
            store.ResetAiProjectionPublicationDiagnostics();
        }

        internal bool TryCaptureCanonicalState(
            NTSDEntityRuntime runtime,
            out AiDecisionInputState input)
        {
            if (!store.TryCaptureCommon(runtime, out input))
                return false;

            return true;
        }

        internal bool CanEvaluateCanonicalDecision(NTSDEntityRuntime runtime)
        {
            return store.CanEvaluateCanonicalDecision(runtime);
        }

        internal bool TryEvaluateCanonicalDecision(
            NTSDEntityRuntime runtime,
            AiDecisionSnapshot snapshot,
            AiDecisionEvaluationPolicy policy,
            bool captureRngTrace,
            BattleAiInputDetailDiagnostics diagnostics,
            ref AiDecisionWitness witness)
        {
            return store.TryEvaluateCanonicalDecision(
                runtime,
                snapshot,
                policy,
                captureRngTrace,
                diagnostics,
                ref witness);
        }

        internal bool TryCaptureAiProjection(
            NTSDEntityRuntime runtime,
            out BattleCharacterInputAiProjection projection)
        {
            return store.TryCaptureAiProjection(runtime, out projection);
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleCharacterInputAiProjection projection)
        {
            return store.TryCaptureAiProjection(handle, out projection);
        }

        internal void SetCoordinateTarget(
            NTSDEntityRuntime runtime,
            int x,
            int z)
        {
            store.SetCoordinateTarget(runtime, x, z);
        }

        internal void SyncBoundaryFlagsFromRuntime(NTSDEntityRuntime runtime)
        {
            if (runtime != null)
                store.SetBoundaryFlags(runtime, CaptureBoundaryFlags(runtime));
        }

        internal bool TryCaptureProgressState(
            NTSDEntityRuntime runtime,
            out BattleCharacterInputActionState input)
        {
            if (world.AiExecutionProfile !=
                BattleAiExecutionProfile.DataOrientedCanonical)
            {
                input = default;
                return false;
            }

            return store.TryCaptureProgress(runtime, out input);
        }

        internal void CommitFullState(
            NTSDEntityRuntime runtime,
            in AiDecisionInputState input)
        {
            if (runtime == null)
                return;

            store.CommitFull(runtime, input, false);
            CommitFullRuntimeMirror(runtime, input);
        }

        internal void CommitAiDecisionState(
            NTSDEntityRuntime runtime,
            in AiDecisionInputState input)
        {
            if (runtime == null)
                return;

            store.CommitFull(runtime, input, true);
            int[] history = EnsureInputHistory(runtime);
            history[0] = input.History0;
            history[1] = input.History1;
            history[2] = input.History2;
            history[3] = input.History3;
            history[4] = input.History4;
            history[5] = input.History5;
            CommitFullRuntimeMirror(runtime, input);
        }

        private static void CommitFullRuntimeMirror(
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
            CommitProgressRuntimeMirror(runtime, input);
        }

        internal void CommitProgressState(
            NTSDEntityRuntime runtime,
            in AiDecisionInputState input)
        {
            if (runtime == null)
                return;

            store.CommitProgress(runtime, input);
            CommitProgressRuntimeMirror(runtime, input);
        }

        private static void CommitProgressRuntimeMirror(
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

        internal void CommitProgressState(
            NTSDEntityRuntime runtime,
            in BattleCharacterInputActionState input)
        {
            if (runtime == null)
                return;

            store.CommitProgress(runtime, input);
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

        internal void CommitResolvedProgressState(
            NTSDEntityRuntime runtime,
            in BattleCharacterInputActionState original,
            in BattleCharacterInputActionState resolved,
            bool capturedCanonical)
        {
            bool unchanged = capturedCanonical && resolved.ContentEquals(original);
            if (unchanged)
            {
                world.RecordCharacterInputProgressCommitForDiagnostics(false);
                return;
            }

            CommitProgressState(runtime, resolved);
            world.RecordCharacterInputProgressCommitForDiagnostics(true);
        }

        internal void RollAndClearCurrentKeys(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            store.RollAndClearCurrentKeys(runtime);
            runtime.PrevUp = runtime.KeyUp;
            runtime.PrevDown = runtime.KeyDown;
            runtime.PrevLeft = runtime.KeyLeft;
            runtime.PrevRight = runtime.KeyRight;
            runtime.PrevJump = runtime.KeyJump;
            runtime.PrevDefend = runtime.KeyDefend;
            runtime.PrevAttack = runtime.KeyAttack;
            ClearCurrentKeys(runtime);
        }

        internal void ClearCurrentKeys(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            store.ClearCurrentKeys(runtime);
            runtime.KeyUp = 0;
            runtime.KeyDown = 0;
            runtime.KeyLeft = 0;
            runtime.KeyRight = 0;
            runtime.KeyAttack = 0;
            runtime.KeyJump = 0;
            runtime.KeyDefend = 0;
        }

        internal void ApplyInputEdges(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            store.ApplyInputEdges(runtime);
            if (runtime.PrevRight == 0 && runtime.KeyRight == 1)
            {
                runtime.CdRight = 5;
                PushInputHistoryRuntimeMirror(runtime, 6);
            }
            if (runtime.PrevLeft == 0 && runtime.KeyLeft == 1)
            {
                runtime.CdLeft = 5;
                PushInputHistoryRuntimeMirror(runtime, 4);
            }
            if (runtime.PrevUp == 0 && runtime.KeyUp == 1)
            {
                runtime.CdUp = 5;
                PushInputHistoryRuntimeMirror(runtime, 8);
            }
            if (runtime.PrevDown == 0 && runtime.KeyDown == 1)
            {
                runtime.CdDown = 5;
                PushInputHistoryRuntimeMirror(runtime, 2);
            }
            if (runtime.PrevAttack == 0 && runtime.KeyAttack == 1)
            {
                runtime.CdDefend = 5;
                PushInputHistoryRuntimeMirror(runtime, 9);
            }
            if (runtime.PrevDefend == 0 && runtime.KeyDefend == 1)
            {
                runtime.CdJump = 5;
                PushInputHistoryRuntimeMirror(runtime, 0);
            }
            if (runtime.PrevJump == 0 && runtime.KeyJump == 1)
            {
                runtime.CdAttack = 5;
                PushInputHistoryRuntimeMirror(runtime, 5);
            }
        }

        internal void PushInputHistory(NTSDEntityRuntime runtime, int keyCode)
        {
            store.PushInputHistory(runtime, keyCode);
            PushInputHistoryRuntimeMirror(runtime, keyCode);
        }

        private void PushInputHistoryRuntimeMirror(
            NTSDEntityRuntime runtime,
            int keyCode)
        {
            int[] history = EnsureInputHistory(runtime);
            if (history == null)
                return;

            history[1] = history[2];
            history[2] = history[3];
            history[3] = history[4];
            history[4] = history[5];
            history[5] = keyCode;
        }

        internal void SetInputHistoryGate(NTSDEntityRuntime runtime, bool enabled)
        {
            store.SetInputHistoryGate(runtime, enabled);
            int[] history = EnsureInputHistory(runtime);
            if (history != null)
                history[0] = enabled ? 1 : 0;
        }

        internal void ClearInputHistoryTail(NTSDEntityRuntime runtime)
        {
            store.ClearInputHistoryTail(runtime);
            int[] history = EnsureInputHistory(runtime);
            if (history != null)
                Array.Clear(history, 1, history.Length - 1);
        }

        internal void SetDefendLock(NTSDEntityRuntime runtime, byte value)
        {
            store.SetDefendLock(runtime, value);
            if (runtime != null)
                runtime.CdDefendLock = value;
        }

        internal void ResetInputState(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            store.ResetInputState(runtime);
            runtime.CdAttack = 0;
            runtime.CdJump = 0;
            runtime.CdDefend = 0;
            runtime.CdDefendLock = 0;
            runtime.CdRight = 0;
            runtime.CdLeft = 0;
            runtime.CdUp = 0;
            runtime.CdDown = 0;
            runtime.ComboDra = 0;
            runtime.ComboDla = 0;
            runtime.ComboDua = 0;
            runtime.ComboDda = 0;
            runtime.ComboDrj = 0;
            runtime.ComboDlj = 0;
            runtime.ComboDuj = 0;
            runtime.ComboDdj = 0;
            runtime.ComboDja = 0;
            runtime.PrevUp = 0;
            runtime.PrevDown = 0;
            runtime.PrevLeft = 0;
            runtime.PrevRight = 0;
            runtime.PrevJump = 0;
            runtime.PrevDefend = 0;
            runtime.PrevAttack = 0;
            ClearCurrentKeys(runtime);

            int[] history = EnsureInputHistory(runtime);
            if (history != null)
                Array.Clear(history, 0, history.Length);
        }

        private int[] EnsureInputHistory(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return null;

            if (runtime.InputHistory == null || runtime.InputHistory.Length != 6)
                runtime.InputHistory = new int[6];
            return runtime.InputHistory;
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
    }
}
