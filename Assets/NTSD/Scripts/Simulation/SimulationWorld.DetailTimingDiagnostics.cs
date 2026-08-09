using System;
using System.Diagnostics;
using System.Threading;
using Unity.Profiling;

namespace NTSD.Simulation
{
    public enum BattleLateRuntimeSnapshotMode
    {
        LegacyThree = 0,
        ConsolidatedFinal = 1,
    }

    public enum BattleTickDetailPhase
    {
        CharacterInputSnapshotBuild = 0,
        CharacterInputEntityInputPass = 1,
        CharacterInputSnapshotClear = 2,
        LateEntityStateSpecial = 3,
        LateEntityRecovery = 4,
        LateEntityFrameTick = 5,
        [Obsolete("Reserved historical phase id 6; production never samples this removed ghost pass.")]
        LateEntityCollision = 6,
        LateEntityFrameExit = 7,
        LateEntityDeathOpoint = 8,
        LateEntityOpointProcess = 9,
        LateEntityCleanup = 10,
        LateEntityTailAndQueuedFlush = 11,
        LateEntityPrevFrameMirror = 12,
        LateEntityFinalPendingFlush = 13,
        RenderPresentationOrder = 14,
        RenderBeginFrame = 15,
        RenderPrepareFrameAndLegacyCapacityGuard = 16,
        RenderLateRendererUpdate = 17,
        RenderBeginFrameSortEntities = 18,
        RenderBeginFrameCaptureHitRecords = 19,
        RenderBeginFrameCaptureEntities = 20,
        RenderBeginFrameBuildCommands = 21,
        RenderPrepareFrameFrozenFrameCopy = 22,
        RenderPrepareFrameResolveCommands = 23,
        RenderPrepareFrameWriteQuads = 24,
        RenderPrepareFrameSetVertexBufferData = 25,
        RenderPrepareFrameSetSubMeshes = 26,
        RenderExecuteCommandBuffer = 27,
        CandidateCollectCacheSetup = 28,
        CandidateCollectParticipantBodyItrBuild = 29,
        CandidateCollectInputValidation = 30,
        CandidateCollectDirectBroadphase = 31,
        CandidateCollectTreeBroadphase = 32,
        CandidateCollectFallbackPairAdd = 33,
        CandidateCollectSortDeduplicate = 34,
        CandidateCollectPairExactLoop = 35,
        RenderBuildCommandsShadow = 36,
        RenderBuildCommandsEntity = 37,
        RenderBuildCommandsOverlay = 38,
        RenderBuildCommandsHitRecord = 39,
        Count = 40,
    }

    /// <summary>
    /// Stable LateEntityUpdate call-site markers for the runtime snapshot diagnostic.
    /// These name pass positions rather than source-code line numbers so reports remain
    /// comparable when the implementation moves.
    /// </summary>
    public enum BattleLateRuntimeSnapshotStage
    {
        StateSpecial = 0,
        Recovery = 1,
        FrameTickSuppressed = 2,
        FrameTick = 3,
        FrameExit = 4,
        DeathOpoint = 5,
        CleanupCompleted = 6,
        TailAndQueuedFlush = 7,
        PrevFrameMirror = 8,
        TransitionInternal = 9,
        Count = 10,
    }

    public sealed class BattleTickDetailPhaseDiagnostics
    {
        private const string ProfilerMarkerNamePrefix = "NTSD.BattleTick.Detail.";

        private static class PhaseProfilerMarkers
        {
            internal static readonly ProfilerMarker[] All = Create();

            private static ProfilerMarker[] Create()
            {
                var markers = new ProfilerMarker[(int)BattleTickDetailPhase.Count];
                for (int index = 0; index < markers.Length; index++)
                {
                    markers[index] = new ProfilerMarker(
                        GetProfilerMarkerNameForDiagnostics(
                            (BattleTickDetailPhase)index));
                }
                return markers;
            }
        }

        private readonly long[] elapsedTimestampTicks =
            new long[(int)BattleTickDetailPhase.Count];
        private readonly long[] deferredRenderElapsedTimestampTicks =
            new long[(int)BattleTickDetailPhase.Count];
        private readonly BattleTickDetailPhase[] activePhases =
            new BattleTickDetailPhase[8];
        private readonly long[] activePhaseTimestamps = new long[8];
        private readonly long[] lateRuntimeSnapshotElapsedTimestampTicks =
            new long[(int)BattleLateRuntimeSnapshotStage.Count];
        private readonly long[] lateRuntimeSnapshotCallCounts =
            new long[(int)BattleLateRuntimeSnapshotStage.Count];
        private readonly BattleLateRuntimeSnapshotStage[] activeLateRuntimeSnapshotStages =
            new BattleLateRuntimeSnapshotStage[4];
        private readonly long[] activeLateRuntimeSnapshotTimestamps = new long[4];
        private int activePhaseDepth;
        private int activeLateRuntimeSnapshotDepth;
        private int deferredRenderMaterializationDepth;

        public static int PhaseCount => (int)BattleTickDetailPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastTickIndex { get; private set; } = -1;
        public int ActivePhaseDepthForDiagnostics => activePhaseDepth;
        public BattleTickDetailPhase ActivePhaseForDiagnostics =>
            activePhaseDepth > 0
                ? activePhases[activePhaseDepth - 1]
                : BattleTickDetailPhase.Count;

        public void SetEnabled(bool enabled)
        {
            if (Enabled)
                EndTick();
            Enabled = enabled;
            activePhaseDepth = 0;
            LastTickIndex = -1;
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
            Array.Clear(
                lateRuntimeSnapshotElapsedTimestampTicks,
                0,
                lateRuntimeSnapshotElapsedTimestampTicks.Length);
            Array.Clear(lateRuntimeSnapshotCallCounts, 0, lateRuntimeSnapshotCallCounts.Length);
            Array.Clear(
                activeLateRuntimeSnapshotTimestamps,
                0,
                activeLateRuntimeSnapshotTimestamps.Length);
            deferredRenderMaterializationDepth = 0;
            activeLateRuntimeSnapshotDepth = 0;
            for (int index = 0; index < deferredRenderElapsedTimestampTicks.Length; index++)
                Interlocked.Exchange(ref deferredRenderElapsedTimestampTicks[index], 0);
        }

        public void BeginTick(int tickIndex)
        {
            if (!Enabled)
                return;

            EndTick();
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
            Array.Clear(
                lateRuntimeSnapshotElapsedTimestampTicks,
                0,
                lateRuntimeSnapshotElapsedTimestampTicks.Length);
            Array.Clear(lateRuntimeSnapshotCallCounts, 0, lateRuntimeSnapshotCallCounts.Length);
            Array.Clear(
                activeLateRuntimeSnapshotTimestamps,
                0,
                activeLateRuntimeSnapshotTimestamps.Length);
            activePhaseDepth = 0;
            activeLateRuntimeSnapshotDepth = 0;
            deferredRenderMaterializationDepth = 0;
            for (int index = 0; index < deferredRenderElapsedTimestampTicks.Length; index++)
            {
                elapsedTimestampTicks[index] =
                    Interlocked.Exchange(ref deferredRenderElapsedTimestampTicks[index], 0);
            }
            LastTickIndex = tickIndex;
        }

        public bool BeginDeferredRenderMaterialization()
        {
            if (!Enabled)
                return false;

            deferredRenderMaterializationDepth++;
            return true;
        }

        public void EndDeferredRenderMaterialization()
        {
            if (!Enabled || deferredRenderMaterializationDepth <= 0)
                return;

            deferredRenderMaterializationDepth--;
        }

        public void BeginPhase(BattleTickDetailPhase phase)
        {
            if (!Enabled || (uint)phase >= (uint)BattleTickDetailPhase.Count ||
                activePhaseDepth >= activePhases.Length)
            {
                return;
            }

            activePhases[activePhaseDepth] = phase;
            activePhaseTimestamps[activePhaseDepth] = Stopwatch.GetTimestamp();
            PhaseProfilerMarkers.All[(int)phase].Begin();
            activePhaseDepth++;
        }

        public void EndPhase(BattleTickDetailPhase phase)
        {
            if (!Enabled || activePhaseDepth == 0 ||
                activePhases[activePhaseDepth - 1] != phase)
            {
                return;
            }

            EndActivePhase();
        }

        public void EndTick()
        {
            if (!Enabled)
                return;

            while (activePhaseDepth > 0)
                EndActivePhase();
            while (activeLateRuntimeSnapshotDepth > 0)
                EndActiveLateRuntimeSnapshot();
            deferredRenderMaterializationDepth = 0;
        }

        public void RecordDeferredPhaseElapsed(
            BattleTickDetailPhase phase,
            long elapsedTicks)
        {
            if (!Enabled || phase != BattleTickDetailPhase.RenderExecuteCommandBuffer ||
                elapsedTicks <= 0)
            {
                return;
            }

            Interlocked.Add(
                ref deferredRenderElapsedTimestampTicks[(int)phase],
                elapsedTicks);
        }

        public void RecordPhaseElapsed(
            BattleTickDetailPhase phase,
            long elapsedTicks)
        {
            if (!Enabled || (uint)phase >= (uint)BattleTickDetailPhase.Count ||
                elapsedTicks <= 0)
            {
                return;
            }

            elapsedTimestampTicks[(int)phase] += elapsedTicks;
        }

        public long GetLastElapsedTimestampTicks(BattleTickDetailPhase phase)
        {
            return (uint)phase < (uint)BattleTickDetailPhase.Count
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

        public void BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage stage)
        {
            if (!Enabled || (uint)stage >= (uint)BattleLateRuntimeSnapshotStage.Count ||
                activeLateRuntimeSnapshotDepth >= activeLateRuntimeSnapshotStages.Length)
            {
                return;
            }

            lateRuntimeSnapshotCallCounts[(int)stage]++;
            activeLateRuntimeSnapshotStages[activeLateRuntimeSnapshotDepth] = stage;
            activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth] =
                Stopwatch.GetTimestamp();
            activeLateRuntimeSnapshotDepth++;
        }

        public void EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage stage)
        {
            if (!Enabled || activeLateRuntimeSnapshotDepth == 0 ||
                activeLateRuntimeSnapshotStages[activeLateRuntimeSnapshotDepth - 1] != stage)
            {
                return;
            }

            activeLateRuntimeSnapshotDepth--;
            lateRuntimeSnapshotElapsedTimestampTicks[(int)stage] += Stopwatch.GetTimestamp() -
                activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth];
            activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth] = 0;
        }

        public long GetLastLateRuntimeSnapshotElapsedTimestampTicks(
            BattleLateRuntimeSnapshotStage stage)
        {
            return (uint)stage < (uint)BattleLateRuntimeSnapshotStage.Count
                ? lateRuntimeSnapshotElapsedTimestampTicks[(int)stage]
                : 0;
        }

        public long GetLastLateRuntimeSnapshotCallCount(BattleLateRuntimeSnapshotStage stage)
        {
            return (uint)stage < (uint)BattleLateRuntimeSnapshotStage.Count
                ? lateRuntimeSnapshotCallCounts[(int)stage]
                : 0;
        }

        public static string GetLateRuntimeSnapshotStageName(
            BattleLateRuntimeSnapshotStage stage)
        {
            switch (stage)
            {
                case BattleLateRuntimeSnapshotStage.StateSpecial:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/StateSpecial";
                case BattleLateRuntimeSnapshotStage.Recovery:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/Recovery";
                case BattleLateRuntimeSnapshotStage.FrameTickSuppressed:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/FrameTickSuppressed";
                case BattleLateRuntimeSnapshotStage.FrameTick:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/FrameTick";
                case BattleLateRuntimeSnapshotStage.FrameExit:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/FrameExit";
                case BattleLateRuntimeSnapshotStage.DeathOpoint:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/DeathOpoint";
                case BattleLateRuntimeSnapshotStage.CleanupCompleted:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/CleanupCompleted";
                case BattleLateRuntimeSnapshotStage.TailAndQueuedFlush:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/TailAndQueuedFlush";
                case BattleLateRuntimeSnapshotStage.PrevFrameMirror:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/PrevFrameMirror";
                case BattleLateRuntimeSnapshotStage.TransitionInternal:
                    return "LateEntityUpdate/RefreshRuntimeSnapshot/TransitionInternal";
                default:
                    return string.Empty;
            }
        }

        public static string GetPhaseName(BattleTickDetailPhase phase)
        {
            switch (phase)
            {
                case BattleTickDetailPhase.CharacterInputSnapshotBuild:
                    return "CharacterInput/SnapshotBuild";
                case BattleTickDetailPhase.CharacterInputEntityInputPass:
                    return "CharacterInput/EntityInputPass";
                case BattleTickDetailPhase.CharacterInputSnapshotClear:
                    return "CharacterInput/SnapshotClear";
                case BattleTickDetailPhase.LateEntityStateSpecial:
                    return "LateEntityUpdate/StateSpecial";
                case BattleTickDetailPhase.LateEntityRecovery:
                    return "LateEntityUpdate/Recovery";
                case BattleTickDetailPhase.LateEntityFrameTick:
                    return "LateEntityUpdate/FrameTick";
                case (BattleTickDetailPhase)6:
                    return "Reserved/RemovedLateEntityCollision";
                case BattleTickDetailPhase.LateEntityFrameExit:
                    return "LateEntityUpdate/FrameExit";
                case BattleTickDetailPhase.LateEntityDeathOpoint:
                    return "LateEntityUpdate/DeathOpoint";
                case BattleTickDetailPhase.LateEntityOpointProcess:
                    return "LateEntityUpdate/OpointProcess";
                case BattleTickDetailPhase.LateEntityCleanup:
                    return "LateEntityUpdate/Cleanup";
                case BattleTickDetailPhase.LateEntityTailAndQueuedFlush:
                    return "LateEntityUpdate/TailAndQueuedFlush";
                case BattleTickDetailPhase.LateEntityPrevFrameMirror:
                    return "LateEntityUpdate/PrevFrameMirror";
                case BattleTickDetailPhase.LateEntityFinalPendingFlush:
                    return "LateEntityUpdate/FinalPendingFlush";
                case BattleTickDetailPhase.RenderPresentationOrder:
                    return "Render/PresentationOrder";
                case BattleTickDetailPhase.RenderBeginFrame:
                    return "Render/BeginFrame";
                case BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard:
                    return "Render/PrepareFrame/LegacyCapacityGuard";
                case BattleTickDetailPhase.RenderLateRendererUpdate:
                    return "Render/LateRendererUpdate";
                case BattleTickDetailPhase.RenderBeginFrameSortEntities:
                    return "Render/BeginFrame/SortEntities";
                case BattleTickDetailPhase.RenderBeginFrameCaptureHitRecords:
                    return "Render/BeginFrame/CaptureHitRecords";
                case BattleTickDetailPhase.RenderBeginFrameCaptureEntities:
                    return "Render/BeginFrame/CaptureEntities";
                case BattleTickDetailPhase.RenderBeginFrameBuildCommands:
                    return "Render/BeginFrame/BuildCommands";
                case BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy:
                    return "Render/PrepareFrame/FrozenFrameCopy";
                case BattleTickDetailPhase.RenderPrepareFrameResolveCommands:
                    return "Render/PrepareFrame/ResolveCommands";
                case BattleTickDetailPhase.RenderPrepareFrameWriteQuads:
                    return "Render/PrepareFrame/WriteQuads";
                case BattleTickDetailPhase.RenderPrepareFrameSetVertexBufferData:
                    return "Render/PrepareFrame/SetVertexBufferData";
                case BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes:
                    return "Render/PrepareFrame/SetSubMeshes";
                case BattleTickDetailPhase.RenderExecuteCommandBuffer:
                    return "Render/ExecuteCommandBuffer";
                case BattleTickDetailPhase.CandidateCollectCacheSetup:
                    return "CandidateCollect/CacheSetup";
                case BattleTickDetailPhase.CandidateCollectParticipantBodyItrBuild:
                    return "CandidateCollect/ParticipantBodyItrBuild";
                case BattleTickDetailPhase.CandidateCollectInputValidation:
                    return "CandidateCollect/InputValidation";
                case BattleTickDetailPhase.CandidateCollectDirectBroadphase:
                    return "CandidateCollect/DirectBroadphase";
                case BattleTickDetailPhase.CandidateCollectTreeBroadphase:
                    return "CandidateCollect/TreeBroadphase";
                case BattleTickDetailPhase.CandidateCollectFallbackPairAdd:
                    return "CandidateCollect/FallbackPairAdd";
                case BattleTickDetailPhase.CandidateCollectSortDeduplicate:
                    return "CandidateCollect/SortDeduplicate";
                case BattleTickDetailPhase.CandidateCollectPairExactLoop:
                    return "CandidateCollect/PairExactLoop";
                case BattleTickDetailPhase.RenderBuildCommandsShadow:
                    return "Render/BeginFrame/BuildCommands/Shadow";
                case BattleTickDetailPhase.RenderBuildCommandsEntity:
                    return "Render/BeginFrame/BuildCommands/Entity";
                case BattleTickDetailPhase.RenderBuildCommandsOverlay:
                    return "Render/BeginFrame/BuildCommands/Overlay";
                case BattleTickDetailPhase.RenderBuildCommandsHitRecord:
                    return "Render/BeginFrame/BuildCommands/HitRecord";
                default:
                    return string.Empty;
            }
        }

        public static string GetProfilerMarkerNameForDiagnostics(
            BattleTickDetailPhase phase)
        {
            string phaseName = GetPhaseName(phase);
            return phaseName.Length == 0
                ? string.Empty
                : ProfilerMarkerNamePrefix + phaseName;
        }

        private void EndActivePhase()
        {
            activePhaseDepth--;
            BattleTickDetailPhase phase = activePhases[activePhaseDepth];
            long elapsed = Stopwatch.GetTimestamp() -
                           activePhaseTimestamps[activePhaseDepth];
            PhaseProfilerMarkers.All[(int)phase].End();
            if (deferredRenderMaterializationDepth > 0 &&
                IsDeferredMaterializationPhase(phase))
            {
                Interlocked.Add(
                    ref deferredRenderElapsedTimestampTicks[(int)phase],
                    elapsed);
            }
            else
            {
                elapsedTimestampTicks[(int)phase] += elapsed;
            }
            activePhaseTimestamps[activePhaseDepth] = 0;
        }

        private void EndActiveLateRuntimeSnapshot()
        {
            activeLateRuntimeSnapshotDepth--;
            BattleLateRuntimeSnapshotStage stage =
                activeLateRuntimeSnapshotStages[activeLateRuntimeSnapshotDepth];
            lateRuntimeSnapshotElapsedTimestampTicks[(int)stage] +=
                Stopwatch.GetTimestamp() -
                activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth];
            activeLateRuntimeSnapshotTimestamps[activeLateRuntimeSnapshotDepth] = 0;
        }

        private static bool IsDeferredMaterializationPhase(BattleTickDetailPhase phase)
        {
            return phase == BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard ||
                   phase >= BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy &&
                   phase <= BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes;
        }
    }

    public enum BattleAiInputDetailPhase
    {
        SnapshotSlotSnapshot = 0,
        SnapshotIndexBuild = 1,
        SnapshotQuadtreeSync = 2,
        FindNearestGround = 3,
        FindNearestAir = 4,
        RemainingAiDecision = 5,
        InputStateSyncFromRuntime = 6,
        ComboUpdate = 7,
        RefreshRuntimeSnapshot = 8,
        CandidateNearest = 9,
        CandidateSpecial = 10,
        ContextMoveMode = 11,
        CachedTargetRetention = 12,
        PostSpecialMainDecision = 13,
        Teammate20Scan = 14,
        Held20Scan = 15,
        InputEdges = 16,
        SnapshotUnifiedDuplicateCapture = 17,
        SnapshotUnifiedDuplicateIndexBuild = 18,
        UnifiedSnapshotExecutionRowRefresh = 19,
        IndexedCanonicalCapture = 20,
        IndexedCanonicalKernel = 21,
        IndexedCanonicalCommitValidation = 22,
        IndexedCanonicalCommitApply = 23,
        Count = 24,
    }

    /// <summary>
    /// Per-CharacterInput diagnostic recorder. It is independent from the top-level
    /// detail phase recorder so AI sub-phases can nest without replacing its active phase.
    /// </summary>
    public sealed class BattleAiInputDetailDiagnostics
    {
        public const int RadiusHistogramBucketCount = 9;

        private readonly long[] elapsedTimestampTicks =
            new long[(int)BattleAiInputDetailPhase.Count];
        private readonly long[] phaseCallCounts =
            new long[(int)BattleAiInputDetailPhase.Count];
        private readonly long[] phaseSlotVisitCounts =
            new long[(int)BattleAiInputDetailPhase.Count];
        private readonly long[] phaseRngCallCounts =
            new long[(int)BattleAiInputDetailPhase.Count];
        private readonly BattleAiInputDetailPhase[] activePhases =
            new BattleAiInputDetailPhase[4];
        private readonly long[] activePhaseTimestamps = new long[4];
        private readonly long[] radiusHistogram = new long[RadiusHistogramBucketCount];
        private int activePhaseDepth;

        public static int PhaseCount => (int)BattleAiInputDetailPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastTickIndex { get; private set; } = -1;
        public long AiCount { get; private set; }
        public long SpatialQueryCount { get; private set; }
        public long QueriedHandleVisits { get; private set; }
        public long CandidateVisits { get; private set; }
        public long RadiusExpansions { get; private set; }
        public long BruteFallbackCount { get; private set; }
        public long BruteSlotVisits { get; private set; }
        public long Phase1ListVisits { get; private set; }
        public long RefreshCount { get; private set; }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            Reset(-1);
        }

        public void BeginTick(int tickIndex)
        {
            if (Enabled)
                Reset(tickIndex);
        }

        public void BeginPhase(BattleAiInputDetailPhase phase)
        {
            if (!Enabled || (uint)phase >= (uint)BattleAiInputDetailPhase.Count ||
                activePhaseDepth >= activePhases.Length)
            {
                return;
            }

            activePhases[activePhaseDepth] = phase;
            activePhaseTimestamps[activePhaseDepth] = Stopwatch.GetTimestamp();
            activePhaseDepth++;
        }

        public void EndPhase(BattleAiInputDetailPhase phase)
        {
            if (!Enabled || activePhaseDepth == 0 ||
                activePhases[activePhaseDepth - 1] != phase)
            {
                return;
            }

            activePhaseDepth--;
            elapsedTimestampTicks[(int)phase] +=
                Stopwatch.GetTimestamp() - activePhaseTimestamps[activePhaseDepth];
            activePhaseTimestamps[activePhaseDepth] = 0;
        }

        public long GetLastElapsedTimestampTicks(BattleAiInputDetailPhase phase)
        {
            return (uint)phase < (uint)BattleAiInputDetailPhase.Count
                ? elapsedTimestampTicks[(int)phase]
                : 0;
        }

        public long GetLastCallCount(BattleAiInputDetailPhase phase)
        {
            return (uint)phase < (uint)BattleAiInputDetailPhase.Count
                ? phaseCallCounts[(int)phase]
                : 0;
        }

        public long GetLastSlotVisitCount(BattleAiInputDetailPhase phase)
        {
            return (uint)phase < (uint)BattleAiInputDetailPhase.Count
                ? phaseSlotVisitCounts[(int)phase]
                : 0;
        }

        public long GetLastRngCallCount(BattleAiInputDetailPhase phase)
        {
            return (uint)phase < (uint)BattleAiInputDetailPhase.Count
                ? phaseRngCallCounts[(int)phase]
                : 0;
        }

        public long GetRadiusHistogramValue(int index)
        {
            return (uint)index < (uint)radiusHistogram.Length ? radiusHistogram[index] : 0;
        }

        public static string GetPhaseName(BattleAiInputDetailPhase phase)
        {
            switch (phase)
            {
                case BattleAiInputDetailPhase.SnapshotSlotSnapshot:
                    return "CharacterInput/AI/SnapshotSlotSnapshot";
                case BattleAiInputDetailPhase.SnapshotIndexBuild:
                    return "CharacterInput/AI/SnapshotIndexBuild";
                case BattleAiInputDetailPhase.SnapshotQuadtreeSync:
                    return "CharacterInput/AI/SnapshotQuadtreeSync";
                case BattleAiInputDetailPhase.FindNearestGround:
                    return "CharacterInput/AI/FindNearestGround";
                case BattleAiInputDetailPhase.FindNearestAir:
                    return "CharacterInput/AI/FindNearestAir";
                case BattleAiInputDetailPhase.RemainingAiDecision:
                    return "CharacterInput/AI/RemainingAiDecision";
                case BattleAiInputDetailPhase.InputStateSyncFromRuntime:
                    return "CharacterInput/AI/InputStateSyncFromRuntime";
                case BattleAiInputDetailPhase.ComboUpdate:
                    return "CharacterInput/AI/ComboUpdate";
                case BattleAiInputDetailPhase.RefreshRuntimeSnapshot:
                    return "CharacterInput/AI/RefreshRuntimeSnapshot";
                case BattleAiInputDetailPhase.CandidateNearest:
                    return "CharacterInput/AI/RemainingAiDecision/CandidateNearest";
                case BattleAiInputDetailPhase.CandidateSpecial:
                    return "CharacterInput/AI/RemainingAiDecision/CandidateSpecial";
                case BattleAiInputDetailPhase.ContextMoveMode:
                    return "CharacterInput/AI/RemainingAiDecision/ContextMoveMode";
                case BattleAiInputDetailPhase.CachedTargetRetention:
                    return "CharacterInput/AI/RemainingAiDecision/CachedTargetRetention";
                case BattleAiInputDetailPhase.PostSpecialMainDecision:
                    return "CharacterInput/AI/RemainingAiDecision/PostSpecialMainDecision";
                case BattleAiInputDetailPhase.Teammate20Scan:
                    return "CharacterInput/AI/RemainingAiDecision/PostSpecialMainDecision/Teammate20Scan";
                case BattleAiInputDetailPhase.Held20Scan:
                    return "CharacterInput/AI/RemainingAiDecision/PostSpecialMainDecision/Held20Scan";
                case BattleAiInputDetailPhase.InputEdges:
                    return "CharacterInput/AI/RemainingAiDecision/InputEdges";
                case BattleAiInputDetailPhase.SnapshotUnifiedDuplicateCapture:
                    return "CharacterInput/AI/SnapshotUnifiedDuplicateCapture";
                case BattleAiInputDetailPhase.SnapshotUnifiedDuplicateIndexBuild:
                    return "CharacterInput/AI/SnapshotUnifiedDuplicateIndexBuild";
                case BattleAiInputDetailPhase.UnifiedSnapshotExecutionRowRefresh:
                    return "CharacterInput/AI/UnifiedSnapshotExecutionRowRefresh";
                case BattleAiInputDetailPhase.IndexedCanonicalCapture:
                    return "CharacterInput/AI/RemainingAiDecision/IndexedCanonicalCapture";
                case BattleAiInputDetailPhase.IndexedCanonicalKernel:
                    return "CharacterInput/AI/RemainingAiDecision/IndexedCanonicalKernel";
                case BattleAiInputDetailPhase.IndexedCanonicalCommitValidation:
                    return "CharacterInput/AI/RemainingAiDecision/IndexedCanonicalCommitValidation";
                case BattleAiInputDetailPhase.IndexedCanonicalCommitApply:
                    return "CharacterInput/AI/RemainingAiDecision/IndexedCanonicalCommitApply";
                default:
                    return string.Empty;
            }
        }

        public void RecordAi() { if (Enabled) AiCount++; }
        public void RecordSpatialQuery() { if (Enabled) SpatialQueryCount++; }
        public void RecordQueriedHandleVisits(int count) { if (Enabled) QueriedHandleVisits += count; }
        public void RecordCandidateVisits(int count) { if (Enabled) CandidateVisits += count; }
        public void RecordRadius(int radius)
        {
            if (!Enabled)
                return;
            int bucket = radius <= 64 ? 0 : radius <= 128 ? 1 : radius <= 256 ? 2 :
                radius <= 512 ? 3 : radius <= 1024 ? 4 : radius <= 2048 ? 5 :
                radius <= 4096 ? 6 : radius <= 8192 ? 7 : 8;
            radiusHistogram[bucket]++;
        }
        public void RecordRadiusExpansion() { if (Enabled) RadiusExpansions++; }
        public void RecordBruteFallback() { if (Enabled) BruteFallbackCount++; }
        public void RecordBruteSlotVisits(int count) { if (Enabled) BruteSlotVisits += count; }
        public void RecordPhase1ListVisits(int count) { if (Enabled) Phase1ListVisits += count; }
        public void RecordRefresh() { if (Enabled) RefreshCount++; }
        public void RecordPhaseCall(BattleAiInputDetailPhase phase)
        {
            if (Enabled && (uint)phase < (uint)BattleAiInputDetailPhase.Count)
                phaseCallCounts[(int)phase]++;
        }
        public void RecordPhaseSlotVisits(BattleAiInputDetailPhase phase, int count)
        {
            if (Enabled && count > 0 &&
                (uint)phase < (uint)BattleAiInputDetailPhase.Count)
            {
                phaseSlotVisitCounts[(int)phase] += count;
            }
        }
        public void RecordPhaseRngCalls(BattleAiInputDetailPhase phase, ulong count)
        {
            if (Enabled && count > 0 &&
                (uint)phase < (uint)BattleAiInputDetailPhase.Count)
            {
                phaseRngCallCounts[(int)phase] += count > long.MaxValue
                    ? long.MaxValue
                    : (long)count;
            }
        }

        private void Reset(int tickIndex)
        {
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(phaseCallCounts, 0, phaseCallCounts.Length);
            Array.Clear(phaseSlotVisitCounts, 0, phaseSlotVisitCounts.Length);
            Array.Clear(phaseRngCallCounts, 0, phaseRngCallCounts.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
            Array.Clear(radiusHistogram, 0, radiusHistogram.Length);
            activePhaseDepth = 0;
            LastTickIndex = tickIndex;
            AiCount = 0;
            SpatialQueryCount = 0;
            QueriedHandleVisits = 0;
            CandidateVisits = 0;
            RadiusExpansions = 0;
            BruteFallbackCount = 0;
            BruteSlotVisits = 0;
            Phase1ListVisits = 0;
            RefreshCount = 0;
        }
    }

    public partial class SimulationWorld
    {
        private BattleTickDetailPhaseDiagnostics battleTickDetailPhaseDiagnostics;
        private BattleAiInputDetailDiagnostics battleAiInputDetailDiagnostics;

        public bool BattleTickDetailPhaseDiagnosticsAllocatedForDiagnostics =>
            battleTickDetailPhaseDiagnostics != null;

        public BattleTickDetailPhaseDiagnostics ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics =>
            battleTickDetailPhaseDiagnostics != null &&
            battleTickDetailPhaseDiagnostics.Enabled
                ? battleTickDetailPhaseDiagnostics
                : null;

        public BattleTickDetailPhaseDiagnostics EnableBattleTickDetailPhaseDiagnosticsForDiagnostics()
        {
            if (battleTickDetailPhaseDiagnostics == null)
                battleTickDetailPhaseDiagnostics = new BattleTickDetailPhaseDiagnostics();
            battleTickDetailPhaseDiagnostics.SetEnabled(true);
            return battleTickDetailPhaseDiagnostics;
        }

        public void DisableBattleTickDetailPhaseDiagnosticsForDiagnostics()
        {
            battleTickDetailPhaseDiagnostics?.SetEnabled(false);
        }

        public bool BattleAiInputDetailDiagnosticsAllocatedForDiagnostics =>
            battleAiInputDetailDiagnostics != null;

        public BattleAiInputDetailDiagnostics ActiveBattleAiInputDetailDiagnosticsForDiagnostics =>
            battleAiInputDetailDiagnostics != null && battleAiInputDetailDiagnostics.Enabled
                ? battleAiInputDetailDiagnostics
                : null;

        public BattleAiInputDetailDiagnostics EnableBattleAiInputDetailDiagnosticsForDiagnostics()
        {
            if (battleAiInputDetailDiagnostics == null)
                battleAiInputDetailDiagnostics = new BattleAiInputDetailDiagnostics();
            battleAiInputDetailDiagnostics.SetEnabled(true);
            return battleAiInputDetailDiagnostics;
        }

        public void DisableBattleAiInputDetailDiagnosticsForDiagnostics()
        {
            battleAiInputDetailDiagnostics?.SetEnabled(false);
        }
    }
}
