using System;
using System.Diagnostics;
using NTSD.Simulation.Presentation;

namespace NTSD.Simulation.Presentation
{
    public enum BattlePresentationPhase
    {
        RenderOrderCollectAndSort = 0,
        RenderOrderRankMapFill = 1,
        BeginFrameSortEntities = 2,
        BeginFrameCaptureHitRecords = 3,
        BeginFrameCaptureEntities = 4,
        BeginFrameBuildCommands = 5,
        RequiresPublicationBinding = 6,
        PublishSwapAndRelease = 7,
        QueueLatestPublishedFrame = 8,
        PresentationPublishTotal = 9,
        BeginFrameTotal = 10,
        ResolveDeferredSpriteCaptures = 11,
        BuildCommandsCore = 12,
        CaptureSubmissionFrame = 13,
        MaterializePresentationOrder = 14,
        MaterializeCommands = 15,
        ConfigureResolver = 16,
        MeshResolveAndWriteCommands = 17,
        MeshUploadChunks = 18,
        PublishSubmission = 19,
        MeshResolveCommands = 20,
        MeshWriteQuads = 21,
        BuildCommandsShadow = 22,
        BuildCommandsEntity = 23,
        BuildCommandsOverlay = 24,
        BuildCommandsHitRecord = 25,
        Count = 26,
    }

    /// <summary>
    /// Opt-in coarse timing for the presentation publication path. The world keeps
    /// this recorder unallocated until explicitly enabled.
    /// </summary>
    public sealed class BattlePresentationPhaseDiagnostics
    {
        private readonly long[] elapsedTimestampTicks =
            new long[(int)BattlePresentationPhase.Count];
        private readonly long[] completedElapsedTimestampTicks =
            new long[(int)BattlePresentationPhase.Count];
        private readonly BattlePresentationPhase[] activePhases =
            new BattlePresentationPhase[4];
        private readonly long[] activePhaseTimestamps = new long[4];
        private int activePhaseDepth;
        private int activeTickIndex = -1;

        public static int PhaseCount => (int)BattlePresentationPhase.Count;
        public static long TimestampFrequency => Stopwatch.Frequency;
        public bool Enabled { get; private set; }
        public int LastCompletedTickIndex { get; private set; } = -1;
        public long CompletedSampleSequence { get; private set; }

        public void SetEnabled(bool enabled)
        {
            Enabled = enabled;
            activePhaseDepth = 0;
            activeTickIndex = -1;
            LastCompletedTickIndex = -1;
            CompletedSampleSequence = 0;
            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(
                completedElapsedTimestampTicks,
                0,
                completedElapsedTimestampTicks.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
        }

        public void BeginTick(int tickIndex)
        {
            if (!Enabled)
                return;

            Array.Clear(elapsedTimestampTicks, 0, elapsedTimestampTicks.Length);
            Array.Clear(activePhaseTimestamps, 0, activePhaseTimestamps.Length);
            activePhaseDepth = 0;
            activeTickIndex = tickIndex;
        }

        public void BeginPhase(BattlePresentationPhase phase)
        {
            if (!Enabled || activeTickIndex < 0 ||
                (uint)phase >= (uint)BattlePresentationPhase.Count ||
                activePhaseDepth >= activePhases.Length)
            {
                return;
            }

            activePhases[activePhaseDepth] = phase;
            activePhaseTimestamps[activePhaseDepth] = Stopwatch.GetTimestamp();
            activePhaseDepth++;
        }

        public void EndPhase(BattlePresentationPhase phase)
        {
            if (!Enabled || activeTickIndex < 0 || activePhaseDepth == 0 ||
                activePhases[activePhaseDepth - 1] != phase)
            {
                return;
            }

            activePhaseDepth--;
            elapsedTimestampTicks[(int)phase] +=
                Stopwatch.GetTimestamp() - activePhaseTimestamps[activePhaseDepth];
            activePhaseTimestamps[activePhaseDepth] = 0;
        }

        public void CompleteTick(int tickIndex)
        {
            if (!Enabled || activeTickIndex != tickIndex || activePhaseDepth != 0)
                return;

            LastCompletedTickIndex = tickIndex;
            Array.Copy(
                elapsedTimestampTicks,
                completedElapsedTimestampTicks,
                elapsedTimestampTicks.Length);
            CompletedSampleSequence++;
            activeTickIndex = -1;
        }

        public long GetLastElapsedTimestampTicks(BattlePresentationPhase phase)
        {
            return (uint)phase < (uint)BattlePresentationPhase.Count
                ? completedElapsedTimestampTicks[(int)phase]
                : 0;
        }

        public void RecordPhaseElapsed(
            BattlePresentationPhase phase,
            long elapsedTicks)
        {
            if (!Enabled || activeTickIndex < 0 || elapsedTicks <= 0 ||
                (uint)phase >= (uint)BattlePresentationPhase.Count)
            {
                return;
            }

            elapsedTimestampTicks[(int)phase] += elapsedTicks;
        }

        public static string GetPhaseName(BattlePresentationPhase phase)
        {
            switch (phase)
            {
                case BattlePresentationPhase.RenderOrderCollectAndSort:
                    return "BuildPresentationRenderOrder/CollectAndSort";
                case BattlePresentationPhase.RenderOrderRankMapFill:
                    return "BuildPresentationRenderOrder/RankMapFill";
                case BattlePresentationPhase.BeginFrameSortEntities:
                    return "BeginFrame/SortEntities";
                case BattlePresentationPhase.BeginFrameCaptureHitRecords:
                    return "BeginFrame/CaptureHitRecords";
                case BattlePresentationPhase.BeginFrameCaptureEntities:
                    return "BeginFrame/CaptureEntities";
                case BattlePresentationPhase.BeginFrameBuildCommands:
                    return "BeginFrame/BuildCommands";
                case BattlePresentationPhase.RequiresPublicationBinding:
                    return "CaptureBuildAndPublishFrame/RequiresPublicationBinding";
                case BattlePresentationPhase.PublishSwapAndRelease:
                    return "CaptureBuildAndPublishFrame/PublishSwapAndRelease";
                case BattlePresentationPhase.QueueLatestPublishedFrame:
                    return "RenderDispatch/QueueLatestPublishedFrame";
                case BattlePresentationPhase.PresentationPublishTotal:
                    return "RenderDispatch/PresentationPublishTotal";
                case BattlePresentationPhase.BeginFrameTotal:
                    return "RenderDispatch/BeginFrameTotal";
                case BattlePresentationPhase.ResolveDeferredSpriteCaptures:
                    return "BeginFrame/BuildCommands/ResolveDeferredSpriteCaptures";
                case BattlePresentationPhase.BuildCommandsCore:
                    return "BeginFrame/BuildCommands/Core";
                case BattlePresentationPhase.CaptureSubmissionFrame:
                    return "Materialize/CaptureSubmissionFrame";
                case BattlePresentationPhase.MaterializePresentationOrder:
                    return "Materialize/PresentationOrder";
                case BattlePresentationPhase.MaterializeCommands:
                    return "Materialize/BuildCommands";
                case BattlePresentationPhase.ConfigureResolver:
                    return "Materialize/ConfigureResolver";
                case BattlePresentationPhase.MeshResolveAndWriteCommands:
                    return "Materialize/Mesh/ResolveAndWriteCommands";
                case BattlePresentationPhase.MeshUploadChunks:
                    return "Materialize/Mesh/UploadChunks";
                case BattlePresentationPhase.PublishSubmission:
                    return "Materialize/PublishSubmission";
                case BattlePresentationPhase.MeshResolveCommands:
                    return "Materialize/Mesh/ResolveCommands";
                case BattlePresentationPhase.MeshWriteQuads:
                    return "Materialize/Mesh/WriteQuads";
                case BattlePresentationPhase.BuildCommandsShadow:
                    return "BeginFrame/BuildCommands/Core/Shadow";
                case BattlePresentationPhase.BuildCommandsEntity:
                    return "BeginFrame/BuildCommands/Core/Entity";
                case BattlePresentationPhase.BuildCommandsOverlay:
                    return "BeginFrame/BuildCommands/Core/Overlay";
                case BattlePresentationPhase.BuildCommandsHitRecord:
                    return "BeginFrame/BuildCommands/Core/HitRecord";
                default:
                    return string.Empty;
            }
        }
    }
}
