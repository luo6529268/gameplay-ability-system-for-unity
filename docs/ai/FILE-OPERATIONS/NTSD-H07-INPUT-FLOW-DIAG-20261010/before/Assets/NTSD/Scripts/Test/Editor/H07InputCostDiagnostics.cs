#if UNITY_EDITOR
using System;
using NTSD.Animation;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    internal sealed class H07InputCostDiagnostics : IDisposable
    {
        internal const string OutputRoot = "artifacts/diagnostics/NTSD-H07-INPUT-COST-DIAG-20261010/windows";
        private static Tick[] baseline;
        private readonly ProductionEntityStressRunner runner;
        private readonly SimulationTickDriver driver;
        private readonly SimulationWorld world;
        private readonly BruteForceSceneQuery query;
        private readonly BattleLogicTickGcObserverEditor gc;
        private readonly BattleAiInputDetailDiagnostics detail;
        private readonly bool previousBoundary;
        private readonly int sequence;
        private bool attached, disposed;
        internal readonly Evidence evidence = new Evidence();

        [Serializable]
        internal sealed class Tick
        {
            public int tick, playerInputCount, progressCommits, progressSkips, producerRefreshes, producerSkips;
            public bool steady, actualFlagsValid;
            public ulong fullRuntimeChecksum, rngCalls, crtCalls, synchronizedCalls, synchronizedTableHash;
            public uint rngState, crtState, tableSeed, lastSynchronizedCallSite;
            public int synchronizedCounter, synchronizedIndex, fallbackAttack, fallbackBody, inert, fallbackTotal;
            public long driverAllocationEvents;
            public long[] elapsed = new long[42];
            public long[] calls = new long[42];
        }

        [Serializable]
        internal sealed class Evidence
        {
            public string scope = "Per completed Driver tick; checksum and counter capture after Driver timing/GC scope. Warm1-120; steady121-300. Boundary toggle only; other AI fine timers remain equal and ON.";
            public long stopwatchFrequency = System.Diagnostics.Stopwatch.Frequency;
            public bool boundaryEnabled, settingsRestored, aborted;
            public int sequenceIndex, ticks, parityMismatches, actualFlagFailures;
            public Tick[] observations = new Tick[300];
        }

        internal static void ResetBaseline() { baseline = null; }

        internal static ProductionEntityStressRequest BuildRequest(int index)
        {
            if (index < 0 || index >= 4) throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(3);
            request.outputPath = OutputRoot + "/" + index.ToString("D2") +
                (index == 1 || index == 2 ? "-combat1000-on/report.json" : "-combat1000-off/report.json");
            return request;
        }

        internal H07InputCostDiagnostics(ProductionEntityStressRunner runner, SimulationTickDriver driver,
            BruteForceSceneQuery query, BattleLogicTickGcObserverEditor gc, int sequence)
        {
            this.runner = runner;
            this.driver = driver;
            this.query = query;
            this.gc = gc;
            this.sequence = sequence;
            world = driver.World;
            detail = world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (detail == null || !detail.Enabled || BattleAiInputDetailDiagnostics.PhaseCount != 42 ||
                runner.DriverTickObserverAttachedForDiagnostics || runner.Report.logicTicksExecuted != 0 ||
                world.EnableCharacterInputBoundaryTimingForDiagnostics)
                throw new InvalidOperationException("Input observation requires an unowned first-tick boundary and fixed AI timing schema.");
            evidence.sequenceIndex = sequence;
            evidence.boundaryEnabled = sequence == 1 || sequence == 2;
            for (int i = 0; i < 300; i++) evidence.observations[i] = new Tick();
            previousBoundary = world.EnableCharacterInputBoundaryTimingForDiagnostics;
            world.EnableCharacterInputBoundaryTimingForDiagnostics = evidence.boundaryEnabled;
            try
            {
                runner.ConfigureDriverTickObserverForDiagnostics(gc.BeginDriverStep, gc.EndDriverStep, Accept);
                attached = true;
            }
            catch { Dispose(); throw; }
        }

        private void Accept(int tick, bool steady)
        {
            gc.AcceptCompletedTick(tick, steady);
            if (tick != evidence.ticks + 1 || tick > 300)
                throw new InvalidOperationException("Input observation lost the fixed 300-tick sequence.");
            Tick row = evidence.observations[tick - 1];
            row.tick = tick;
            row.steady = steady;
            row.playerInputCount = driver.LastAppliedFrameInput.Players?.Count ?? 0;
            row.fullRuntimeChecksum = world.CaptureRuntimeChecksum64(driver.CurrentTickIndex, driver.LastAppliedFrameInput);
            row.rngState = world.Rng.State;
            row.rngCalls = world.Rng.CallCount;
            var native = world.NativeRandom.CaptureScalarState();
            row.crtState = native.CrtState;
            row.crtCalls = native.CrtCalls;
            row.tableSeed = native.TableSeed;
            row.synchronizedCounter = native.SynchronizedCounter;
            row.synchronizedIndex = native.SynchronizedIndex;
            row.synchronizedCalls = native.SynchronizedCalls;
            row.lastSynchronizedCallSite = native.LastSynchronizedCallSite;
            row.synchronizedTableHash = native.SynchronizedTableHash;
            row.progressCommits = world.LastCharacterInputProgressCommitCountForDiagnostics;
            row.progressSkips = world.LastCharacterInputProgressCommitSkipCountForDiagnostics;
            row.producerRefreshes = world.LastCharacterInputProducerSnapshotRefreshCountForDiagnostics;
            row.producerSkips = world.LastCharacterInputProducerSnapshotReuseSkipCountForDiagnostics;
            row.driverAllocationEvents = gc.LastCompletedScopeForDiagnostics.allocationEvents;
            query.GetLastRoleAwareFallbackOrdinalCountsForSelfCheck(out _, out row.fallbackAttack, out _, out row.fallbackBody);
            row.inert = query.LastRoleAwareInertParticipantCountForDiagnostics;
            row.fallbackTotal = query.LastFormalFallbackParticipantCountForDiagnostics;
            row.actualFlagsValid = detail.Enabled &&
                world.EnableCharacterInputBoundaryTimingForDiagnostics == evidence.boundaryEnabled &&
                !world.EnableCharacterInputProducerSnapshotReuseForDiagnostics &&
                !world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics &&
                !query.EnableRoleRecordCostDiagnostics && !query.EnableRoleBranchTimingForDiagnostics &&
                !query.EnableRoleCapacityRecordGuardForDiagnostics && !query.EnableRoleSnapshotFrameReuseForDiagnostics &&
                !query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics && !query.EnableRoleEndpointSnapshotReuseForDiagnostics &&
                query.LastFormalCollectorModeForDiagnostics == CollisionFormalCollectorMode.ForceRoleAware &&
                !query.LastFormalCollectionAbortedForDiagnostics && row.fallbackAttack == 0 && row.fallbackBody == 0 &&
                row.inert >= 0 && row.fallbackTotal == row.inert && steady == (tick > 120);
            for (int phase = 0; phase < 42; phase++)
            {
                row.elapsed[phase] = detail.GetLastElapsedTimestampTicks((BattleAiInputDetailPhase)phase);
                row.calls[phase] = detail.GetLastCallCount((BattleAiInputDetailPhase)phase);
                if (!evidence.boundaryEnabled && phase >= 30) row.actualFlagsValid &= row.elapsed[phase] == 0;
            }
            if (!row.actualFlagsValid) evidence.actualFlagFailures++;
            if (sequence != 0 && (baseline == null || !EqualState(baseline[tick - 1], row))) evidence.parityMismatches++;
            evidence.ticks++;
        }

        internal bool Finish(bool aborted)
        {
            evidence.aborted = aborted;
            Dispose();
            bool valid = !aborted && evidence.ticks == 300 && evidence.actualFlagFailures == 0 &&
                evidence.parityMismatches == 0 && evidence.settingsRestored;
            if (sequence == 0 && valid) baseline = evidence.observations;
            return valid;
        }

        private static bool EqualState(Tick a, Tick b) => a != null && a.tick == b.tick && a.steady == b.steady &&
            a.playerInputCount == b.playerInputCount && a.fullRuntimeChecksum == b.fullRuntimeChecksum &&
            a.rngState == b.rngState && a.rngCalls == b.rngCalls && a.crtState == b.crtState && a.crtCalls == b.crtCalls &&
            a.tableSeed == b.tableSeed && a.synchronizedCounter == b.synchronizedCounter &&
            a.synchronizedIndex == b.synchronizedIndex && a.synchronizedCalls == b.synchronizedCalls &&
            a.lastSynchronizedCallSite == b.lastSynchronizedCallSite && a.synchronizedTableHash == b.synchronizedTableHash;

        public void Dispose()
        {
            if (disposed) return;
            try { if (attached) runner.ConfigureDriverTickObserverForDiagnostics(null, null, null); }
            finally
            {
                world.EnableCharacterInputBoundaryTimingForDiagnostics = previousBoundary;
                evidence.settingsRestored = world.EnableCharacterInputBoundaryTimingForDiagnostics == previousBoundary;
                disposed = true;
            }
        }
    }
}
#endif
