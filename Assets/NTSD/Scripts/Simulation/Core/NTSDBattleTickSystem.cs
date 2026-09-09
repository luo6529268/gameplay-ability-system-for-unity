using System;
using System.Diagnostics;
using NTSD.Simulation.Presentation;
using Unity.Profiling;

namespace NTSD.Simulation
{
    public enum BattleTickPhase
    {
        BattleFlow = 0,
        Cooldown = 1,
        HumanInput = 2,
        RuntimeMaintenance = 3,
        InputClear = 4,
        CharacterInput = 5,
        NativeTeleport = 6,
        FrameMotion = 7,
        FrameAdvance = 8,
        Revival = 9,
        StageBounds = 10,
        PreInteraction = 11,
        HeldProcess = 12,
        CollisionSnapshot = 13,
        PairVRest = 14,
        CandidateCollect = 15,
        CharacterHitConsumePostInteraction = 16,
        RandomWeaponDrop = 17,
        ObjectHitConsume = 18,
        CandidateConsumptionEnd = 19,
        PreFrameBounds = 20,
        Stage = 21,
        RenderDispatch = 22,
        FramePostProcess = 23,
        LateEntityUpdate = 24,
        RandomWeaponDropTail = 25,
        EntityPostFrameTail = 26,
        BattleResults = 27,
        NativeSparkAdvance = 28,
        NestedPhysics = 29,
        ActiveWeaponCount = 30,
        NativeResourceTick = 31,
        NativeFrameTick = 32,
        Count = 33,
    }

    internal enum BattleTickCompletion : byte
    {
        NotCompleted = 0,
        FullReturn = 1,
    }

    public sealed class BattleTickPhaseDiagnostics
    {
        private const int PhaseSequenceCapacity = 64;

        private static class PhaseProfilerMarkers
        {
            internal static readonly ProfilerMarker[] All = Create();

            private static ProfilerMarker[] Create()
            {
                var markers = new ProfilerMarker[(int)BattleTickPhase.Count];
                for (int index = 0; index < markers.Length; index++)
                {
                    BattleTickPhase phase = (BattleTickPhase)index;
                    markers[index] = new ProfilerMarker(
                        "NTSD.BattleTick." + GetPhaseName(phase));
                }
                return markers;
            }
        }

        private readonly long[] elapsedTimestampTicks = new long[(int)BattleTickPhase.Count];
        private readonly BattleTickPhase[] phaseSequence =
            new BattleTickPhase[PhaseSequenceCapacity];
        private BattleTickPhase activePhase = BattleTickPhase.Count;
        private long activePhaseTimestamp;
        private int phaseSequenceCount;
        private bool phaseSequenceOverflowed;

        public static int PhaseCount => (int)BattleTickPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastTickIndex { get; private set; } = -1;
        public bool HasActivePhaseForDiagnostics =>
            activePhase != BattleTickPhase.Count;
        public BattleTickPhase ActivePhaseForDiagnostics => activePhase;
        public int LastPhaseSequenceCount => phaseSequenceCount;
        public bool LastPhaseSequenceOverflowed => phaseSequenceOverflowed;

        internal static void PrepareProfilerMarkers()
        {
            _ = PhaseProfilerMarkers.All.Length;
        }

        public void SetEnabled(bool enabled)
        {
            if (Enabled)
                EndActivePhase();
            Enabled = enabled;
            activePhase = BattleTickPhase.Count;
            activePhaseTimestamp = 0;
            LastTickIndex = -1;
            phaseSequenceCount = 0;
            phaseSequenceOverflowed = false;
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
        }

        public void BeginTick(int tickIndex)
        {
            if (!Enabled)
                return;

            EndActivePhase();
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            LastTickIndex = tickIndex;
            phaseSequenceCount = 0;
            phaseSequenceOverflowed = false;
        }

        public void BeginPhase(BattleTickPhase phase)
        {
            if (!Enabled || (uint)phase >= (uint)BattleTickPhase.Count)
                return;

            EndActivePhase();
            if (phaseSequenceCount < phaseSequence.Length)
            {
                phaseSequence[phaseSequenceCount++] = phase;
            }
            else
            {
                phaseSequenceOverflowed = true;
            }
            activePhase = phase;
            activePhaseTimestamp = Stopwatch.GetTimestamp();
            PhaseProfilerMarkers.All[(int)phase].Begin();
        }

        public void EndPhase(BattleTickPhase phase)
        {
            if (!Enabled || activePhase != phase)
                return;

            EndActivePhase();
        }

        public void EndTick()
        {
            if (!Enabled)
                return;

            EndActivePhase();
        }

        public long GetLastElapsedTimestampTicks(BattleTickPhase phase)
        {
            return (uint)phase < (uint)BattleTickPhase.Count
                ? elapsedTimestampTicks[(int)phase]
                : 0;
        }

        public long GetLastPhaseSumTimestampTicks()
        {
            long sum = 0;
            for (int i = 0; i < elapsedTimestampTicks.Length; i++)
                sum += elapsedTimestampTicks[i];
            return sum;
        }

        public bool TryGetLastPhaseAt(
            int index,
            out BattleTickPhase phase)
        {
            if ((uint)index >= (uint)phaseSequenceCount)
            {
                phase = BattleTickPhase.Count;
                return false;
            }

            phase = phaseSequence[index];
            return true;
        }

        public static string GetPhaseName(BattleTickPhase phase)
        {
            switch (phase)
            {
                case BattleTickPhase.BattleFlow: return "BattleFlow";
                case BattleTickPhase.Cooldown: return "Cooldown";
                case BattleTickPhase.HumanInput: return "HumanInput";
                case BattleTickPhase.RuntimeMaintenance: return "RuntimeMaintenance";
                case BattleTickPhase.InputClear: return "InputClear";
                case BattleTickPhase.CharacterInput: return "CharacterInput";
                case BattleTickPhase.NativeTeleport: return "NativeTeleport";
                case BattleTickPhase.FrameMotion: return "FrameMotion";
                case BattleTickPhase.FrameAdvance: return "FrameAdvance";
                case BattleTickPhase.Revival: return "Revival";
                case BattleTickPhase.StageBounds: return "StageBounds";
                case BattleTickPhase.PreInteraction: return "PreInteraction";
                case BattleTickPhase.HeldProcess: return "HeldProcess";
                case BattleTickPhase.CollisionSnapshot: return "CollisionSnapshot";
                case BattleTickPhase.PairVRest: return "PairVRest";
                case BattleTickPhase.CandidateCollect: return "CandidateCollect";
                case BattleTickPhase.CharacterHitConsumePostInteraction:
                    return "CharacterHitConsumePostInteraction";
                case BattleTickPhase.RandomWeaponDrop: return "RandomWeaponDrop";
                case BattleTickPhase.ObjectHitConsume: return "ObjectHitConsume";
                case BattleTickPhase.CandidateConsumptionEnd: return "CandidateConsumptionEnd";
                case BattleTickPhase.PreFrameBounds: return "PreFrameBounds";
                case BattleTickPhase.Stage: return "Stage";
                case BattleTickPhase.RenderDispatch: return "RenderDispatch";
                case BattleTickPhase.FramePostProcess: return "FramePostProcess";
                case BattleTickPhase.LateEntityUpdate: return "LateEntityUpdate";
                case BattleTickPhase.RandomWeaponDropTail: return "RandomWeaponDropTail";
                case BattleTickPhase.EntityPostFrameTail: return "EntityPostFrameTail";
                case BattleTickPhase.BattleResults: return "BattleResults";
                case BattleTickPhase.NativeSparkAdvance: return "NativeSparkAdvance";
                case BattleTickPhase.NestedPhysics: return "NestedPhysics";
                case BattleTickPhase.ActiveWeaponCount: return "ActiveWeaponCount";
                case BattleTickPhase.NativeResourceTick: return "NativeResourceTick";
                case BattleTickPhase.NativeFrameTick: return "NativeFrameTick";
                default: return string.Empty;
            }
        }

        private void EndActivePhase()
        {
            BattleTickPhase phase = activePhase;
            if ((uint)phase >= (uint)BattleTickPhase.Count)
                return;

            elapsedTimestampTicks[(int)phase] +=
                Stopwatch.GetTimestamp() - activePhaseTimestamp;
            PhaseProfilerMarkers.All[(int)phase].End();
            activePhase = BattleTickPhase.Count;
            activePhaseTimestamp = 0;
        }
    }

    /// <summary>
    /// Unity NTSD 战斗 tick 调度器。
    /// 目标顺序以 NTSD 2.8-Logan playable 的 GameSession28::step() 和
    /// SimulationTickDriver28::step(...) 为 authority。当前生产调度仍在 B3 迁移中；
    /// <see cref="NTSD28BattlePassOrder"/> 只定义 expected contract，不表示本类已经对齐。
    /// </summary>
    public sealed class NTSDBattleTickSystem
    {
        private readonly SimulationWorld world;

        public NTSDBattleTickSystem(SimulationWorld world)
        {
            this.world = world;
        }

        public void RunReleaseTick(int tickIndex)
        {
            RunReleaseTick(tickIndex, buildPresentation: true);
        }

        public void RunReleaseTick(int tickIndex, bool buildPresentation)
        {
            RunReleaseTick(
                tickIndex,
                buildPresentation,
                world?.CurrentAppliedFrameInputForResults);
        }

        public void RunReleaseTick(
            int tickIndex,
            bool buildPresentation,
            FrameInputSet frameInput)
        {
            RunTick(
                tickIndex,
                buildPresentation,
                simulationWorker: false,
                frameInput);
        }

        internal BattleTickCompletion RunSimulationWorkerTick(
            int tickIndex,
            bool buildPresentation)
        {
            return RunTick(
                tickIndex,
                buildPresentation,
                simulationWorker: true,
                world?.CurrentAppliedFrameInputForResults);
        }

        private BattleTickCompletion RunTick(
            int tickIndex,
            bool buildPresentation,
            bool simulationWorker,
            FrameInputSet frameInput)
        {
            if (world == null)
                return BattleTickCompletion.NotCompleted;

            BattleTickPhaseDiagnostics diagnostics =
                world.ActiveBattleTickPhaseDiagnosticsForDiagnostics;
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            diagnostics?.BeginTick(tickIndex);
            detailDiagnostics?.BeginTick(tickIndex);
            world.BeginDataObjectTypeTickCache(tickIndex);
            try
            {
                diagnostics?.BeginPhase(BattleTickPhase.BattleFlow);
                if (world.Runtime?.Flow != null)
                    world.Runtime.Flow.HumanInputPolledExternally = false;
                world.PendingSounds.Clear();
                world.AdvanceBattleFlowTick(tickIndex);
                diagnostics?.EndPhase(BattleTickPhase.BattleFlow);
                diagnostics?.BeginPhase(BattleTickPhase.NativeSparkAdvance);
                AdvanceNativeSparks();
                diagnostics?.EndPhase(BattleTickPhase.NativeSparkAdvance);
                // Alignment contract: CLIENT-CPP-RESULTS-SCENE-HOST-TICK-ALIGNMENT-001.
                // C++ runs the complete world tick before processing Results host input.
                bool resultsActiveAtTickStart =
                    world.Runtime?.Results?.IsActive == true;

                bool stepWaitGate = PrepareBattleStepGateForTick();
                if (!resultsActiveAtTickStart &&
                    (!stepWaitGate || world.NeedClearInput))
                {
                    diagnostics?.BeginPhase(BattleTickPhase.HumanInput);
                    PollHumanInput(tickIndex);
                    diagnostics?.EndPhase(BattleTickPhase.HumanInput);
                }
                if (!stepWaitGate &&
                    (resultsActiveAtTickStart || !world.NeedClearInput))
                {
                    diagnostics?.BeginPhase(BattleTickPhase.CharacterInput);
                    NativeProducerSampleAndInputRoute(tickIndex);
                    diagnostics?.EndPhase(BattleTickPhase.CharacterInput);
                }
                if (!RunFrameAdvancePhase(
                        tickIndex,
                        diagnostics,
                        allowBattleEntryInputClear: !resultsActiveAtTickStart))
                {
                    return BattleTickCompletion.NotCompleted;
                }
                RunInteractionPhase(tickIndex, diagnostics);
                return RunPresentationAndCleanupPhase(
                    tickIndex,
                    buildPresentation,
                    simulationWorker,
                    stepWaitGate,
                    diagnostics,
                    resultsActiveAtTickStart,
                    frameInput != null && frameInput.TickIndex == tickIndex
                        ? frameInput
                        : null)
                    ? BattleTickCompletion.FullReturn
                    : BattleTickCompletion.NotCompleted;
            }
            finally
            {
                world.EndDataObjectTypeTickCache();
                detailDiagnostics?.EndTick();
                diagnostics?.EndTick();
                world.RefreshBattleEcsShadowAfterTick(tickIndex);
            }
        }

        private bool RunFrameAdvancePhase(
            int tickIndex,
            BattleTickPhaseDiagnostics diagnostics,
            bool allowBattleEntryInputClear)
        {
            if (allowBattleEntryInputClear && world.NeedClearInput)
            {
                diagnostics?.BeginPhase(BattleTickPhase.InputClear);
                world.SetNeedClearInput(false);
                world.ClearBattleEntryInputAll();
                diagnostics?.EndPhase(BattleTickPhase.InputClear);
                return false;
            }

            diagnostics?.BeginPhase(BattleTickPhase.FrameMotion);
            NativeFrameMotion();
            diagnostics?.EndPhase(BattleTickPhase.FrameMotion);
            diagnostics?.BeginPhase(BattleTickPhase.NativeTeleport);
            NativeTeleport();
            diagnostics?.EndPhase(BattleTickPhase.NativeTeleport);
            diagnostics?.BeginPhase(BattleTickPhase.NestedPhysics);
            NativePhysicsAndDeadCharacterResourceNormalize(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.NestedPhysics);
            diagnostics?.BeginPhase(BattleTickPhase.Revival);
            RunRevival(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.Revival);
            diagnostics?.BeginPhase(BattleTickPhase.StageBounds);
            ClampCharacterZToStageBounds();
            diagnostics?.EndPhase(BattleTickPhase.StageBounds);

            // Alignment contract: R2-SCHED-001. C++ game_tick T09 runs the first
            // negative-link held scan after the first Z clamp and before candidate build.
            diagnostics?.BeginPhase(BattleTickPhase.HeldProcess);
            ProcessNegativeHeldObjectsFirstPass(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.HeldProcess);
            diagnostics?.BeginPhase(BattleTickPhase.CollisionSnapshot);
            CaptureCollisionActionSnapshotsOnly();
            diagnostics?.EndPhase(BattleTickPhase.CollisionSnapshot);
            PrepareAttackerRestForCandidate();
            diagnostics?.BeginPhase(BattleTickPhase.PairVRest);
            TickCollisionPairVRest();
            diagnostics?.EndPhase(BattleTickPhase.PairVRest);
            diagnostics?.BeginPhase(BattleTickPhase.CandidateCollect);
            CollectCollisionCandidates();
            diagnostics?.EndPhase(BattleTickPhase.CandidateCollect);
            diagnostics?.BeginPhase(BattleTickPhase.RuntimeMaintenance);
            Oid5152FusionScan(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.RuntimeMaintenance);
            diagnostics?.BeginPhase(BattleTickPhase.ActiveWeaponCount);
            CaptureActiveWeaponObjectCountBeforeHits(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.ActiveWeaponCount);
            diagnostics?.BeginPhase(
                BattleTickPhase.CharacterHitConsumePostInteraction);
            ResolvePostInteractions(tickIndex);
            diagnostics?.EndPhase(
                BattleTickPhase.CharacterHitConsumePostInteraction);
            return true;
        }

        private void RunInteractionPhase(
            int tickIndex,
            BattleTickPhaseDiagnostics diagnostics)
        {
            diagnostics?.BeginPhase(BattleTickPhase.RandomWeaponDrop);
            RandomWeaponDrop(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.RandomWeaponDrop);
            diagnostics?.BeginPhase(BattleTickPhase.ObjectHitConsume);
            ResolveObjectInteractions(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.ObjectHitConsume);
            diagnostics?.BeginPhase(BattleTickPhase.CandidateConsumptionEnd);
            EndCollisionCandidateConsumption();
            diagnostics?.EndPhase(BattleTickPhase.CandidateConsumptionEnd);

            // Alignment contract: R2-SCHED-001. C++ game_tick T14/T15 run after
            // both collision consume loops, then T16 repeats the held scan.
            diagnostics?.BeginPhase(BattleTickPhase.PreInteraction);
            ResolveCpointAndWeaponSync(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.PreInteraction);
            diagnostics?.BeginPhase(BattleTickPhase.StageBounds);
            ClampCharacterZToStageBounds();
            diagnostics?.EndPhase(BattleTickPhase.StageBounds);
            diagnostics?.BeginPhase(BattleTickPhase.HeldProcess);
            ProcessNegativeHeldObjectsSecondPass(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.HeldProcess);

            // Alignment contract: NTSD28-B3-C21-C22-PLACEMENT-001.
            // Keep the existing boundary and impulse algorithms intact while restoring
            // their authority order before C23/C24 and the C25 production entry.
            diagnostics?.BeginPhase(BattleTickPhase.PreFrameBounds);
            PreFrameBounds();
            diagnostics?.EndPhase(BattleTickPhase.PreFrameBounds);
            diagnostics?.BeginPhase(BattleTickPhase.FramePostProcess);
            FramePostProcess();
            diagnostics?.EndPhase(BattleTickPhase.FramePostProcess);

            // Alignment contract: NTSD28-B3-C23-C24-WORLD-CLOCK-001.
            diagnostics?.BeginPhase(BattleTickPhase.NativeResourceTick);
            world.BeginNativeResourceTick();
            diagnostics?.EndPhase(BattleTickPhase.NativeResourceTick);
            diagnostics?.BeginPhase(BattleTickPhase.NativeFrameTick);
            world.BeginNativeFrameTick();
            diagnostics?.EndPhase(BattleTickPhase.NativeFrameTick);

        }

        private bool RunPresentationAndCleanupPhase(
            int tickIndex,
            bool buildPresentation,
            bool simulationWorker,
            bool stepWaitGate,
            BattleTickPhaseDiagnostics diagnostics,
            bool resultsActiveAtTickStart,
            FrameInputSet frameInput)
        {
            if (stepWaitGate)
            {
                // This legacy step-wait route is intentionally preserved by the C25
                // placement package. Its native Host ownership remains a later audit.
                diagnostics?.BeginPhase(BattleTickPhase.FrameAdvance);
                FrameAdvanceAll(tickIndex);
                diagnostics?.EndPhase(BattleTickPhase.FrameAdvance);
                diagnostics?.BeginPhase(BattleTickPhase.Stage);
                CurrentWaveStage(tickIndex);
                diagnostics?.EndPhase(BattleTickPhase.Stage);
                diagnostics?.BeginPhase(BattleTickPhase.RenderDispatch);
                RenderDispatch(tickIndex, buildPresentation, simulationWorker);
                diagnostics?.EndPhase(BattleTickPhase.RenderDispatch);
                return false;
            }

            // Alignment contract: NTSD28-B3-C25-NESTED-TAIL-SKELETON-001.
            // C25 is the first normal-path phase after C24. The existing live-slot
            // loop is now its only production entry; C25a-p behavior is migrated by
            // subsequent packages rather than by retaining split global scans here.
            diagnostics?.BeginPhase(BattleTickPhase.LateEntityUpdate);
            LateEntityUpdate(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.LateEntityUpdate);
            ExpireNativeComboEntries();

            // The surviving old type-3/state9998 serial body has no proven C25 slot
            // owner yet. Keep it visible and strictly after the C25 skeleton until its
            // current-authority behavior is either rehomed or removed.
            diagnostics?.BeginPhase(BattleTickPhase.FrameAdvance);
            FrameAdvanceAll(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.FrameAdvance);
            diagnostics?.BeginPhase(BattleTickPhase.Stage);
            CurrentWaveStage(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.Stage);
            diagnostics?.BeginPhase(BattleTickPhase.RandomWeaponDropTail);
            Mode2RandomWeaponDropTail(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.RandomWeaponDropTail);
            diagnostics?.BeginPhase(BattleTickPhase.EntityPostFrameTail);
            EntityPostFrameTail(tickIndex);
            diagnostics?.EndPhase(BattleTickPhase.EntityPostFrameTail);
            // Alignment contract: R2-SCHED-002. C++ clears g_game_mode2 only after
            // the mode2 and entity post-frame tails have both consumed it.
            ClearFunctionKeyRequestsAfterPostFrameTail();
            diagnostics?.BeginPhase(BattleTickPhase.BattleResults);
            BattleResultsFlow(resultsActiveAtTickStart, frameInput);
            diagnostics?.EndPhase(BattleTickPhase.BattleResults);
            diagnostics?.BeginPhase(BattleTickPhase.RenderDispatch);
            RenderDispatch(tickIndex, buildPresentation, simulationWorker);
            diagnostics?.EndPhase(BattleTickPhase.RenderDispatch);
            return true;
        }

        private void AdvanceNativeSparks()
        {
            world.AdvanceNativeSparkLifecycleAll();
        }

        private void ExpireNativeComboEntries()
        {
            // Alignment contract: NTSD28-B5-NATIVE-COMBO-EXPIRY-001.
            BattleNativeComboExpiryModule.Expire(world);
        }

        private bool PrepareBattleStepGateForTick()
        {
            BattleFlowRuntimeState flow = world.Runtime?.Flow;
            if (flow == null)
                return false;

            flow.BattleStepGate = 0;
            if (flow.BattleStepMode == 2)
            {
                flow.BattleStepGate = 1;
                flow.BattleStepMode = 1;
            }

            return flow.BattleStepMode == 1 && flow.BattleStepGate != 1;
        }

        private void PollHumanInput(int tickIndex)
        {
            world.PostCooldownHumanInputAll(tickIndex);
            if (world.Runtime?.Flow != null)
                world.Runtime.Flow.HumanInputPolledExternally = true;
        }

        private void NativeProducerSampleAndInputRoute(int tickIndex)
        {
            world.NativeProducerSampleAndInputRouteAll(tickIndex);
        }

        private void ProcessNegativeHeldObjectsFirstPass(int tickIndex)
        {
            world.HeldObjectProcessAll(tickIndex);
        }

        private void ProcessNegativeHeldObjectsSecondPass(int tickIndex)
        {
            world.HeldObjectProcessAll(tickIndex);
        }

        private void Oid5152FusionScan(int tickIndex)
        {
            world.Oid5152FusionScanAll(tickIndex);
        }

        private void CaptureCollisionActionSnapshotsOnly()
        {
            world.CaptureCollisionActionSnapshotsOnlyAll();
        }

        private void PrepareAttackerRestForCandidate()
        {
            world.PrepareAttackerRestForCandidateAll();
        }

        private void CollectCollisionCandidates()
        {
            world.CollectCollisionCandidatesAll();
        }

        private void TickCollisionPairVRest()
        {
            world.TickCollisionPairVRestAll();
        }

        private void EndCollisionCandidateConsumption()
        {
            world.EndCollisionCandidateConsumption();
        }

        private void NativeTeleport()
        {
            world.NativeTeleportAll();
        }

        private void ResolveCpointAndWeaponSync(int tickIndex)
        {
            world.PreInteractionTickAll(tickIndex);
        }

        private void FrameAdvanceAll(int tickIndex)
        {
            world.SerialTickAll(
                tickIndex,
                nativeFrameMotionAlreadyApplied: true,
                nativePhysicsAlreadyApplied: true);
        }

        private void NativeFrameMotion()
        {
            world.NativeFrameMotionAll();
        }

        private void NativePhysicsAndDeadCharacterResourceNormalize(int tickIndex)
        {
            world.NativePhysicsAndDeadCharacterResourceNormalizeAll(tickIndex);
        }

        private void RunRevival(int tickIndex)
        {
            world.PostFrameAdvanceDeathCleanupAll(tickIndex);
        }

        private void RandomWeaponDrop(int tickIndex)
        {
            world.RandomWeaponDropTickAll(tickIndex);
        }

        private void CaptureActiveWeaponObjectCountBeforeHits(int tickIndex)
        {
            world.CaptureActiveWeaponObjectCountBeforeHits(tickIndex);
        }

        private void ResolvePostInteractions(int tickIndex)
        {
            world.PostInteractionTickAll(tickIndex);
        }

        private void ResolveObjectInteractions(int tickIndex)
        {
            world.ObjectInteractionTickAll(tickIndex);
        }

        private void ClampCharacterZToStageBounds()
        {
            world.ClampCharacterZToStageBoundsAll();
        }

        private void FramePostProcess()
        {
            world.RunBattleEcsFramePostProcessPass();
        }

        private void CurrentWaveStage(int tickIndex)
        {
            world.CurrentWaveStageTickAll();
        }

        private void RenderDispatch(
            int tickIndex,
            bool buildPresentation,
            bool simulationWorker)
        {
            if (simulationWorker)
            {
                if (buildPresentation)
                    world.CaptureSimulationWorkerPresentationFrame(tickIndex);
                return;
            }

            world.RenderDispatchAll(tickIndex, buildPresentation);
            // Alignment contract: NTSD28-B3-NATIVE-SPARK-C01-INTEGRATION-001.
            // RenderDispatch only freezes/publishes. Logical native spark age and
            // tail ownership now runs once at C01, before any current-tick hit.
        }

        private void PreFrameBounds()
        {
            world.ApplyPreFrameBoundsAll();
        }

        private void LateEntityUpdate(int tickIndex)
        {
            world.LateEntityUpdateAll(tickIndex);
        }

        private void Mode2RandomWeaponDropTail(int tickIndex)
        {
            world.Mode2RandomWeaponDropTailAll(tickIndex);
        }

        private void EntityPostFrameTail(int tickIndex)
        {
            world.EntityPostFrameTailAll(tickIndex);
        }

        private void ClearFunctionKeyRequestsAfterPostFrameTail()
        {
            world.ClearFunctionKeyRequestsAfterPostFrameTail();
        }

        private void BattleResultsFlow(
            bool resultsActiveAtTickStart,
            FrameInputSet frameInput)
        {
            if (resultsActiveAtTickStart)
            {
                world.RunActiveBattleResultsTick(frameInput);
                return;
            }

            world.UpdateBattleResultsFlow();
        }
    }
}
