#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Spatial;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace NTSD.Animation.Rendering.Editor
{
    public enum ProductionEntityStressMode
    {
        Smoke50,
        Dispersed1000,
        Concentrated1000,
    }

    internal enum ProductionEntityStressInputMode
    {
        Ai,
        None,
    }

    [Serializable]
    public sealed class ProductionEntityStressRequest
    {
        public string action = "dispersed";
        public string inputMode = "ai";
        public int warmupTicks = 30;
        public int sampleTicks = 300;
        public int spawnBatchSize = 25;
        public int maxCatchUpTicksPerFrame = 4;
        public int maxBacklogTicks = 8;
        public bool enableDetailPhaseTiming;
        public string formalCollectorMode = "configured";
        public string outputPath = "Temp/NTSD_ProductionEntityStress.dispersed.json";
    }

    internal readonly struct ProductionEntityStressConfig
    {
        internal ProductionEntityStressConfig(
            ProductionEntityStressMode mode,
            ProductionEntityStressInputMode inputMode,
            int warmupTicks,
            int sampleTicks,
            int spawnBatchSize,
            int maxCatchUpTicksPerFrame,
            int maxBacklogTicks,
            bool enableDetailPhaseTiming,
            CollisionFormalCollectorMode formalCollectorMode,
            string outputPath)
        {
            Mode = mode;
            InputMode = inputMode;
            EntityCount = mode == ProductionEntityStressMode.Smoke50 ? 50 : 1000;
            WarmupTicks = Math.Max(0, warmupTicks);
            SampleTicks = Math.Max(1, sampleTicks);
            SpawnBatchSize = Math.Max(1, Math.Min(100, spawnBatchSize));
            MaxCatchUpTicksPerFrame = Math.Max(1, maxCatchUpTicksPerFrame);
            MaxBacklogTicks = Math.Max(MaxCatchUpTicksPerFrame, maxBacklogTicks);
            EnableDetailPhaseTiming = enableDetailPhaseTiming;
            FormalCollectorMode = formalCollectorMode;
            OutputPath = outputPath ?? string.Empty;
        }

        internal ProductionEntityStressMode Mode { get; }
        internal ProductionEntityStressInputMode InputMode { get; }
        internal int EntityCount { get; }
        internal int WarmupTicks { get; }
        internal int SampleTicks { get; }
        internal int SpawnBatchSize { get; }
        internal int MaxCatchUpTicksPerFrame { get; }
        internal int MaxBacklogTicks { get; }
        internal bool EnableDetailPhaseTiming { get; }
        internal CollisionFormalCollectorMode FormalCollectorMode { get; }
        internal string OutputPath { get; }
        internal bool AutoCleanup => Mode == ProductionEntityStressMode.Smoke50;

        internal static ProductionEntityStressConfig FromRequest(
            ProductionEntityStressRequest request,
            string projectRoot)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            ProductionEntityStressMode mode;
            switch (action)
            {
                case "smoke":
                case "smoke50":
                    mode = ProductionEntityStressMode.Smoke50;
                    break;
                case "dispersed":
                case "dispersed1000":
                    mode = ProductionEntityStressMode.Dispersed1000;
                    break;
                case "concentrated":
                case "concentrated1000":
                    mode = ProductionEntityStressMode.Concentrated1000;
                    break;
                default:
                    throw new ArgumentException(
                        $"Unknown production stress action '{request.action}'.",
                        nameof(request));
            }

            string outputPath = string.IsNullOrWhiteSpace(request.outputPath)
                ? $"Temp/NTSD_ProductionEntityStress.{action}.json"
                : request.outputPath;
            outputPath = Path.IsPathRooted(outputPath)
                ? Path.GetFullPath(outputPath)
                : Path.GetFullPath(Path.Combine(projectRoot, outputPath));

            int warmupTicks = mode == ProductionEntityStressMode.Smoke50
                ? Math.Min(Math.Max(0, request.warmupTicks), 5)
                : request.warmupTicks;
            int sampleTicks = mode == ProductionEntityStressMode.Smoke50
                ? Math.Min(Math.Max(1, request.sampleTicks), 30)
                : request.sampleTicks;
            CollisionFormalCollectorMode formalCollectorMode =
                ParseFormalCollectorMode(request.formalCollectorMode);
            return new ProductionEntityStressConfig(
                mode,
                ParseInputMode(request.inputMode),
                warmupTicks,
                sampleTicks,
                request.spawnBatchSize,
                request.maxCatchUpTicksPerFrame,
                request.maxBacklogTicks,
                request.enableDetailPhaseTiming,
                formalCollectorMode,
                outputPath);
        }

        internal static ProductionEntityStressInputMode ParseInputMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                case "ai":
                    return ProductionEntityStressInputMode.Ai;
                case "none":
                    return ProductionEntityStressInputMode.None;
                default:
                    throw new ArgumentException(
                        $"Unknown production stress input mode '{value}'. Expected ai or none.",
                        nameof(value));
            }
        }

        internal static string FormatInputMode(ProductionEntityStressInputMode mode)
        {
            switch (mode)
            {
                case ProductionEntityStressInputMode.Ai:
                    return "ai";
                case ProductionEntityStressInputMode.None:
                    return "none";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        internal static CollisionFormalCollectorMode ParseFormalCollectorMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "configured":
                    return CollisionFormalCollectorMode.Configured;
                case "legacy":
                    return CollisionFormalCollectorMode.ForceLegacyUnionAabb;
                case "role":
                    return CollisionFormalCollectorMode.ForceRoleAware;
                case "brute":
                    return CollisionFormalCollectorMode.ForceBruteForce;
                default:
                    throw new ArgumentException(
                        $"Unknown formal collector mode '{value}'. " +
                        "Expected configured, legacy, role, or brute.",
                        nameof(value));
            }
        }

        internal static string FormatFormalCollectorMode(CollisionFormalCollectorMode mode)
        {
            switch (mode)
            {
                case CollisionFormalCollectorMode.Configured:
                    return "configured";
                case CollisionFormalCollectorMode.ForceLegacyUnionAabb:
                    return "legacy";
                case CollisionFormalCollectorMode.ForceRoleAware:
                    return "role";
                case CollisionFormalCollectorMode.ForceBruteForce:
                    return "brute";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }

    [Serializable]
    public sealed class ProductionEntityStressMetricSummary
    {
        public bool available;
        public string unit;
        public string source;
        public string unavailableReason;
        public int sampleCount;
        public double average;
        public double maximum;
        public double p95;
        public double p99;
    }

    [Serializable]
    public sealed class ProductionEntityStressPhaseTimingSummary
    {
        public string phase;
        public ProductionEntityStressMetricSummary timing;
    }

    [Serializable]
    public sealed class ProductionEntityStressLateRuntimeSnapshotTimingSummary
    {
        public string stage;
        public long callCount;
        public ProductionEntityStressMetricSummary timing;
    }

    [Serializable]
    public sealed class ProductionEntityStressAiInputDetailCounters
    {
        public bool available;
        public string unavailableReason;
        public long aiCount;
        public long spatialQueryCount;
        public long queriedHandleVisits;
        public long candidateVisits;
        public long radiusExpansions;
        public long bruteFallbackCount;
        public long bruteSlotVisits;
        public long phase1ListVisits;
        public long refreshCount;
        public long[] radiusHistogram =
            new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
    }

    [Serializable]
    public sealed class ProductionEntityStressTeardownReport
    {
        public bool attempted;
        public bool restored;
        public bool activeStateRestored;
        public bool driverStateRestored;
        public bool loggingStateRestored;
        public int activeGameObjectsBefore;
        public int activeGameObjectsAfter;
        public int worldObjectsBefore;
        public int worldObjectsAfter;
        public int worldEntitiesBefore;
        public int worldEntitiesAfter;
        public int claimedSlotsBefore;
        public int claimedSlotsAfter;
        public int objectPoolActiveBeforeRun;
        public int objectPoolActiveAfter;
        public int objectPoolAvailableBeforeRun;
        public int objectPoolAvailableAfter;
        public int retainedInactiveObjectPoolCapacityBeforeRun;
        public int retainedInactiveObjectPoolCapacityAfter;
        public int retainedInactiveObjectPoolCapacityDelta;
        public string retainedInactiveObjectPoolCapacityPolicy;
        public int referencePoolActiveBeforeRun;
        public int referencePoolActiveAfter;
        public int cleanupExceptionCount;
        public string cleanupExceptions;
        public string evidence;
    }

    [Serializable]
    public sealed class ProductionEntityStressLoggingPolicyReport
    {
        public string originalFilterLogType;
        public string runningFilterLogType;
        public string policy;
        public bool applied;
        public bool restored;
    }

    [Serializable]
    public sealed class ProductionEntityStressReport
    {
        public string schema = "ntsd-production-entity-stress/v1";
        public string status;
        public string mode;
        public string inputMode;
        public string startedUtc;
        public string updatedUtc;
        public string unityVersion;
        public string platform;
        public string scene;
        public string stressRootName;
        public string outputPath;
        public string failure;
        public bool harnessValidity;
        public string performanceVerdict = "EvidenceOnlyNoThreshold";
        public int requestedEntityCount;
        public int selectedCharacterOid;
        public int totalEntitiesCreated;
        public int lifecycleReplacements;
        public int activeGameObjectCount;
        public int stressRootChildCount;
        public int worldObjectCount;
        public int worldEntityCount;
        public int peakWorldEntityCount;
        public int claimedRuntimeSlotCount;
        public string runtimeProfile;
        public int runtimeSlotCapacity;
        public string broadphaseBackend;
        public string formalCollectorRequestedMode;
        public string formalCollectorMode;
        public int formalCollectorBodyEntries;
        public int formalCollectorItrQueries;
        public int logicTicksExecuted;
        public int warmupTicksCompleted;
        public int sampledLogicTicks;
        public int sampledUnityFrames;
        public int framesWithCatchUp;
        public int maximumCatchUpTicksInFrame;
        public int currentBacklogTicks;
        public int maximumBacklogTicks;
        public int droppedBacklogTicks;
        public long aiControlledEntityTicks;
        public long collisionCandidateCountSum;
        public int collisionCandidateCountPeak;
        public long broadphasePairCountSum;
        public int broadphasePairCountPeak;
        public int broadphaseFallbackParticipantPeak;
        public int broadphaseAbortedTicks;
        public int broadphaseLastIndexedCount;
        public int damageStatTotal;
        public int killStatTotal;
        public bool opointCounterAvailable;
        public int observedOpointCreates;
        public string opointCounterReason;
        public ProductionEntityStressMetricSummary logicTickMilliseconds;
        public ProductionEntityStressMetricSummary unityFrameMilliseconds;
        public ProductionEntityStressMetricSummary logicTickAllocatedBytes;
        public bool phaseTimingEnabled;
        public string phaseTimingSource;
        public List<ProductionEntityStressPhaseTimingSummary> phaseTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public ProductionEntityStressMetricSummary phaseTimingUnattributedMilliseconds;
        public bool detailPhaseTimingEnabled;
        public string detailPhaseTimingSource;
        public string detailPhaseTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> detailPhaseTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public string aiInputDetailTimingSource;
        public string aiInputDetailTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> aiInputDetailTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public string lateRuntimeSnapshotTimingSource;
        public string lateRuntimeSnapshotTimingUnavailableReason;
        public List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>
            lateRuntimeSnapshotTimings =
                new List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>();
        public ProductionEntityStressAiInputDetailCounters aiInputDetailCounters =
            new ProductionEntityStressAiInputDetailCounters();
        public ProductionEntityStressLoggingPolicyReport loggingPolicy =
            new ProductionEntityStressLoggingPolicyReport();
        public ProductionEntityStressTeardownReport teardown =
            new ProductionEntityStressTeardownReport();
    }

    internal sealed class ProductionEntityStressLoggingPolicy
    {
        private readonly Func<LogType> getFilterLogType;
        private readonly Action<LogType> setFilterLogType;
        private bool applied;
        private LogType originalFilterLogType;

        internal ProductionEntityStressLoggingPolicy()
            : this(
                () => Debug.unityLogger.filterLogType,
                value => Debug.unityLogger.filterLogType = value)
        {
        }

        internal ProductionEntityStressLoggingPolicy(
            Func<LogType> getFilterLogType,
            Action<LogType> setFilterLogType)
        {
            this.getFilterLogType = getFilterLogType ?? throw new ArgumentNullException(nameof(getFilterLogType));
            this.setFilterLogType = setFilterLogType ?? throw new ArgumentNullException(nameof(setFilterLogType));
        }

        internal void Apply(ProductionEntityStressLoggingPolicyReport report)
        {
            if (applied)
                return;

            originalFilterLogType = getFilterLogType();
            setFilterLogType(LogType.Error);
            applied = true;
            UpdateReport(report);
        }

        internal void Restore(ProductionEntityStressLoggingPolicyReport report)
        {
            if (applied)
            {
                setFilterLogType(originalFilterLogType);
                applied = false;
            }
            UpdateReport(report);
            if (report != null)
                report.restored = getFilterLogType() == originalFilterLogType;
        }

        private void UpdateReport(ProductionEntityStressLoggingPolicyReport report)
        {
            if (report == null)
                return;

            report.originalFilterLogType = originalFilterLogType.ToString();
            report.runningFilterLogType = LogType.Error.ToString();
            report.policy = "Suppress Log and Warning during the stress run while retaining Error.";
            report.applied = applied;
        }
    }

    internal static class ProductionEntityStressStatistics
    {
        internal static ProductionEntityStressMetricSummary Summarize(
            IReadOnlyList<double> values,
            string unit,
            string source)
        {
            var result = new ProductionEntityStressMetricSummary
            {
                available = values != null && values.Count > 0,
                unit = unit ?? string.Empty,
                source = source ?? string.Empty,
                unavailableReason = values == null || values.Count == 0
                    ? "No completed samples."
                    : string.Empty,
                sampleCount = values?.Count ?? 0,
            };
            if (!result.available)
                return result;

            var sorted = new double[values.Count];
            double sum = 0d;
            double maximum = double.MinValue;
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i];
                sorted[i] = value;
                sum += value;
                maximum = Math.Max(maximum, value);
            }
            Array.Sort(sorted);
            result.average = sum / values.Count;
            result.maximum = maximum;
            result.p95 = Percentile(sorted, 0.95d);
            result.p99 = Percentile(sorted, 0.99d);
            return result;
        }

        internal static double Percentile(IReadOnlyList<double> sortedValues, double percentile)
        {
            if (sortedValues == null || sortedValues.Count == 0)
                return 0d;
            double rank = Math.Max(0d, Math.Min(1d, percentile)) * (sortedValues.Count - 1);
            int lower = (int)Math.Floor(rank);
            int upper = Math.Min(sortedValues.Count - 1, lower + 1);
            double blend = rank - lower;
            return sortedValues[lower] + (sortedValues[upper] - sortedValues[lower]) * blend;
        }
    }

    internal sealed class ProductionEntityStressPhaseTimingCollector
    {
        internal const string Source =
            "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; " +
            "diagnostic evidence only, with no performance threshold.";

        private readonly List<double>[] phaseSamples;
        private readonly List<double> unattributedSamples = new List<double>(512);

        internal ProductionEntityStressPhaseTimingCollector()
        {
            phaseSamples = new List<double>[BattleTickPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }

        internal void CaptureAfterTick(
            BattleTickPhaseDiagnostics diagnostics,
            double outerMilliseconds,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled ||
                completedTickCount <= warmupTickCount)
            {
                return;
            }

            double timestampToMilliseconds =
                1000d / BattleTickPhaseDiagnostics.TimestampFrequency;
            double phaseSumMilliseconds = 0d;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double phaseMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattleTickPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], phaseMilliseconds);
                phaseSumMilliseconds += phaseMilliseconds;
            }

            AddRollingSample(
                unattributedSamples,
                Math.Max(0d, outerMilliseconds - phaseSumMilliseconds));
            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.phaseTimings.Clear();
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattleTickPhase phase = (BattleTickPhase)i;
                report.phaseTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleTickPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }

            report.phaseTimingUnattributedMilliseconds =
                ProductionEntityStressStatistics.Summarize(
                    unattributedSamples,
                    "ms",
                    "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings");
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal sealed class ProductionEntityStressDetailPhaseTimingCollector
    {
        internal const string Source =
            "Independent Stopwatch timestamps accumulated inside CharacterInput and " +
            "LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried " +
            "from the preceding render pass into the next logic-tick sample. Nested diagnostic " +
            "evidence only, with no performance threshold.";
        internal const string AiInputSource =
            "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; " +
            "nested diagnostic evidence only, with no performance threshold.";
        internal const string LateRuntimeSnapshotSource =
            "Independent Stopwatch timestamps around individual LateEntityUpdate " +
            "RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, " +
            "not source-code line numbers; nested diagnostic evidence only, with no " +
            "performance threshold.";

        private readonly List<double>[] phaseSamples;
        private List<double>[] aiInputPhaseSamples;
        private List<double>[] lateRuntimeSnapshotSamples;
        private readonly long[] radiusHistogram =
            new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
        private readonly long[] lateRuntimeSnapshotCallCounts =
            new long[(int)BattleLateRuntimeSnapshotStage.Count];
        private long aiCount;
        private long spatialQueryCount;
        private long queriedHandleVisits;
        private long candidateVisits;
        private long radiusExpansions;
        private long bruteFallbackCount;
        private long bruteSlotVisits;
        private long phase1ListVisits;
        private long refreshCount;

        internal ProductionEntityStressDetailPhaseTimingCollector()
        {
            phaseSamples = new List<double>[BattleTickDetailPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }
        internal int AiInputSampleCount { get; private set; }
        internal bool AiInputPhaseSamplesAllocatedForDiagnostics =>
            aiInputPhaseSamples != null;
        internal int LateRuntimeSnapshotSampleCount { get; private set; }
        internal bool LateRuntimeSnapshotSamplesAllocatedForDiagnostics =>
            lateRuntimeSnapshotSamples != null;

        internal void CaptureAfterTick(
            BattleTickDetailPhaseDiagnostics diagnostics,
            BattleAiInputDetailDiagnostics aiDiagnostics,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled ||
                completedTickCount <= warmupTickCount)
            {
                return;
            }

            double timestampToMilliseconds =
                1000d / BattleTickDetailPhaseDiagnostics.TimestampFrequency;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double phaseMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattleTickDetailPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], phaseMilliseconds);
            }

            if (aiDiagnostics != null && aiDiagnostics.Enabled)
            {
                EnsureAiInputPhaseSamples();
                double aiTimestampToMilliseconds =
                    1000d / BattleAiInputDetailDiagnostics.TimestampFrequency;
                for (int i = 0; i < aiInputPhaseSamples.Length; i++)
                {
                    double phaseMilliseconds = aiDiagnostics.GetLastElapsedTimestampTicks(
                        (BattleAiInputDetailPhase)i) * aiTimestampToMilliseconds;
                    AddRollingSample(aiInputPhaseSamples[i], phaseMilliseconds);
                }

                aiCount += aiDiagnostics.AiCount;
                spatialQueryCount += aiDiagnostics.SpatialQueryCount;
                queriedHandleVisits += aiDiagnostics.QueriedHandleVisits;
                candidateVisits += aiDiagnostics.CandidateVisits;
                radiusExpansions += aiDiagnostics.RadiusExpansions;
                bruteFallbackCount += aiDiagnostics.BruteFallbackCount;
                bruteSlotVisits += aiDiagnostics.BruteSlotVisits;
                phase1ListVisits += aiDiagnostics.Phase1ListVisits;
                refreshCount += aiDiagnostics.RefreshCount;
                for (int i = 0; i < radiusHistogram.Length; i++)
                    radiusHistogram[i] += aiDiagnostics.GetRadiusHistogramValue(i);
                AiInputSampleCount++;
            }

            EnsureLateRuntimeSnapshotSamples();
            for (int i = 0; i < lateRuntimeSnapshotSamples.Length; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                double elapsedMilliseconds = diagnostics
                    .GetLastLateRuntimeSnapshotElapsedTimestampTicks(stage) *
                    timestampToMilliseconds;
                AddRollingSample(lateRuntimeSnapshotSamples[i], elapsedMilliseconds);
                lateRuntimeSnapshotCallCounts[i] +=
                    diagnostics.GetLastLateRuntimeSnapshotCallCount(stage);
            }
            LateRuntimeSnapshotSampleCount++;

            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            if (report.detailPhaseTimings == null)
            {
                report.detailPhaseTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.detailPhaseTimings.Clear();
            }
            if (report.aiInputDetailTimings == null)
            {
                report.aiInputDetailTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.aiInputDetailTimings.Clear();
            }
            if (report.lateRuntimeSnapshotTimings == null)
            {
                report.lateRuntimeSnapshotTimings =
                    new List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>();
            }
            else
            {
                report.lateRuntimeSnapshotTimings.Clear();
            }
            if (report.aiInputDetailCounters == null)
            {
                report.aiInputDetailCounters =
                    new ProductionEntityStressAiInputDetailCounters();
            }
            else if (report.aiInputDetailCounters.radiusHistogram == null ||
                     report.aiInputDetailCounters.radiusHistogram.Length !=
                     BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount)
            {
                report.aiInputDetailCounters.radiusHistogram =
                    new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
            }
            if (!report.detailPhaseTimingEnabled)
            {
                report.detailPhaseTimingSource = string.Empty;
                report.detailPhaseTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.";
                report.aiInputDetailCounters.available = false;
                report.aiInputDetailCounters.unavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail counters.";
                report.aiInputDetailTimingSource = string.Empty;
                report.aiInputDetailTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail timings.";
                report.lateRuntimeSnapshotTimingSource = string.Empty;
                report.lateRuntimeSnapshotTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect " +
                    "LateEntityUpdate RefreshRuntimeSnapshot timings.";
                return;
            }

            report.detailPhaseTimingSource = Source;
            report.detailPhaseTimingUnavailableReason = string.Empty;
            report.aiInputDetailTimingSource = AiInputSource;
            report.aiInputDetailTimingUnavailableReason = AiInputSampleCount > 0
                ? string.Empty
                : "No completed AI input detail timing samples.";
            report.aiInputDetailCounters.available = AiInputSampleCount > 0;
            report.aiInputDetailCounters.unavailableReason = AiInputSampleCount > 0
                ? string.Empty
                : "No completed AI input detail timing samples.";
            report.aiInputDetailCounters.aiCount = aiCount;
            report.aiInputDetailCounters.spatialQueryCount = spatialQueryCount;
            report.aiInputDetailCounters.queriedHandleVisits = queriedHandleVisits;
            report.aiInputDetailCounters.candidateVisits = candidateVisits;
            report.aiInputDetailCounters.radiusExpansions = radiusExpansions;
            report.aiInputDetailCounters.bruteFallbackCount = bruteFallbackCount;
            report.aiInputDetailCounters.bruteSlotVisits = bruteSlotVisits;
            report.aiInputDetailCounters.phase1ListVisits = phase1ListVisits;
            report.aiInputDetailCounters.refreshCount = refreshCount;
            Array.Copy(radiusHistogram, report.aiInputDetailCounters.radiusHistogram, radiusHistogram.Length);
            report.lateRuntimeSnapshotTimingSource = LateRuntimeSnapshotSource;
            report.lateRuntimeSnapshotTimingUnavailableReason =
                LateRuntimeSnapshotSampleCount > 0
                    ? string.Empty
                    : "No completed LateEntityUpdate RefreshRuntimeSnapshot timing samples.";
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattleTickDetailPhase phase = (BattleTickDetailPhase)i;
                report.detailPhaseTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleTickDetailPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }

            for (int i = 0; i < BattleAiInputDetailDiagnostics.PhaseCount; i++)
            {
                BattleAiInputDetailPhase phase = (BattleAiInputDetailPhase)i;
                report.aiInputDetailTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleAiInputDetailDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        aiInputPhaseSamples?[i],
                        "ms",
                        AiInputSource),
                });
            }

            for (int i = 0; i < (int)BattleLateRuntimeSnapshotStage.Count; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                report.lateRuntimeSnapshotTimings.Add(
                    new ProductionEntityStressLateRuntimeSnapshotTimingSummary
                    {
                        stage = BattleTickDetailPhaseDiagnostics
                            .GetLateRuntimeSnapshotStageName(stage),
                        callCount = lateRuntimeSnapshotCallCounts[i],
                        timing = ProductionEntityStressStatistics.Summarize(
                            lateRuntimeSnapshotSamples?[i],
                            "ms",
                            LateRuntimeSnapshotSource),
                    });
            }
        }

        private void EnsureAiInputPhaseSamples()
        {
            if (aiInputPhaseSamples != null)
                return;

            aiInputPhaseSamples =
                new List<double>[BattleAiInputDetailDiagnostics.PhaseCount];
            for (int i = 0; i < aiInputPhaseSamples.Length; i++)
                aiInputPhaseSamples[i] = new List<double>(512);
        }

        private void EnsureLateRuntimeSnapshotSamples()
        {
            if (lateRuntimeSnapshotSamples != null)
                return;

            lateRuntimeSnapshotSamples =
                new List<double>[(int)BattleLateRuntimeSnapshotStage.Count];
            for (int i = 0; i < lateRuntimeSnapshotSamples.Length; i++)
                lateRuntimeSnapshotSamples[i] = new List<double>(512);
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal static class ProductionEntityStressPhaseTimingLifecycle
    {
        internal static void Disable(SimulationWorld world)
        {
            world?.DisableBattleTickPhaseDiagnosticsForDiagnostics();
            world?.DisableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            world?.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
        }
    }

    internal static class ProductionEntityStressRunStatusPolicy
    {
        internal static string ResolveCleanupStatus(
            string currentStatus,
            string reason,
            bool restored)
        {
            if (!string.Equals(reason, "runner-destroyed", StringComparison.Ordinal))
                return currentStatus;

            return restored ? "InterruptedCleanly" : "InterruptedWithResidue";
        }
    }

    internal static class ProductionEntityStressPopulationPolicy
    {
        internal static bool Evaluate(
            int requestedEntityCount,
            int activeGameObjectCount,
            int rootChildCount,
            int worldObjectCount,
            int worldEntityCount,
            int claimedRuntimeSlotCount)
        {
            return requestedEntityCount > 0 &&
                   activeGameObjectCount == requestedEntityCount &&
                   rootChildCount == requestedEntityCount &&
                   worldObjectCount == requestedEntityCount * 2 &&
                   worldEntityCount == requestedEntityCount &&
                   claimedRuntimeSlotCount == requestedEntityCount;
        }
    }

    internal static class ProductionEntityStressTeardownPolicy
    {
        internal static bool IsRestored(
            int activeGameObjects,
            int worldObjects,
            int worldEntities,
            int claimedSlots,
            int objectPoolActive,
            int objectPoolAvailable,
            int referencePoolActive,
            int objectPoolActiveBaseline,
            int objectPoolAvailableBaseline,
            int referencePoolActiveBaseline)
        {
            _ = objectPoolAvailable;
            _ = objectPoolAvailableBaseline;
            return activeGameObjects == 0 &&
                   worldObjects == 0 &&
                   worldEntities == 0 &&
                   claimedSlots == 0 &&
                   objectPoolActive == objectPoolActiveBaseline &&
                   referencePoolActive == referencePoolActiveBaseline;
        }

        internal static int CountActiveStressRootGameObjects(Transform stressRoot)
        {
            if (stressRoot == null)
                return 0;

            int count = 0;
            for (int i = 0; i < stressRoot.childCount; i++)
            {
                GameObject child = stressRoot.GetChild(i)?.gameObject;
                if (child != null && child.activeInHierarchy)
                    count++;
            }
            return count;
        }

        internal static string BuildEvidence(
            string reason,
            ProductionEntityStressTeardownReport teardown)
        {
            if (teardown == null)
                return string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "reason={0}; restored={1}; activeCleanupRestored={2}; driverRestored={3}; " +
                "loggerRestored={4}; cleanupExceptions={5}; activeGO={6}->{7}; " +
                "worldObjects={8}->{9}; worldEntities={10}->{11}; claimed={12}->{13}; " +
                "objectPoolActive={14}->{15}; referencePoolActive={16}->{17}; " +
                "retainedInactiveObjectPoolCapacity={18}->{19} (delta={20}; " +
                "doesNotAffectRestored=True)",
                reason,
                teardown.restored,
                teardown.activeStateRestored,
                teardown.driverStateRestored,
                teardown.loggingStateRestored,
                teardown.cleanupExceptionCount,
                teardown.activeGameObjectsBefore,
                teardown.activeGameObjectsAfter,
                teardown.worldObjectsBefore,
                teardown.worldObjectsAfter,
                teardown.worldEntitiesBefore,
                teardown.worldEntitiesAfter,
                teardown.claimedSlotsBefore,
                teardown.claimedSlotsAfter,
                teardown.objectPoolActiveBeforeRun,
                teardown.objectPoolActiveAfter,
                teardown.referencePoolActiveBeforeRun,
                teardown.referencePoolActiveAfter,
                teardown.retainedInactiveObjectPoolCapacityBeforeRun,
                teardown.retainedInactiveObjectPoolCapacityAfter,
                teardown.retainedInactiveObjectPoolCapacityDelta);
        }
    }

    internal sealed class ProductionEntityStressCleanupJournal
    {
        private readonly List<string> failures = new List<string>();

        internal int FailureCount => failures.Count;

        internal bool Attempt(string phase, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
                return true;
            }
            catch (Exception exception)
            {
                failures.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1}: {2}",
                    phase ?? "cleanup",
                    exception.GetType().Name,
                    exception.Message));
                return false;
            }
        }

        internal string FormatFailures()
        {
            return string.Join(" | ", failures);
        }
    }

    internal static class ProductionEntityStressDerivedObservationPolicy
    {
        internal static bool TryRecord(
            RuntimeEntityHandle handle,
            ISet<RuntimeEntityHandle> harnessOwned,
            ISet<RuntimeEntityHandle> observedDerived)
        {
            return handle.IsValid &&
                   (harnessOwned == null || !harnessOwned.Contains(handle)) &&
                   observedDerived != null &&
                   observedDerived.Add(handle);
        }
    }

    public sealed class ProductionEntityStressRunner : MonoBehaviour
    {
        internal const int MaximumRetainedSamples = 4096;
        private const int MaximumCleanupPasses = 8;

        private readonly List<LF2Character> entities = new List<LF2Character>(1000);
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(1050);
        private readonly HashSet<RuntimeEntityHandle> harnessOwnedHandles =
            new HashSet<RuntimeEntityHandle>();
        private readonly HashSet<RuntimeEntityHandle> observedDerivedHandles =
            new HashSet<RuntimeEntityHandle>();
        private readonly List<double> logicTickSamples = new List<double>(512);
        private readonly List<double> frameSamples = new List<double>(512);
        private readonly List<double> allocationSamples = new List<double>(512);
        private readonly ProductionEntityStressLoggingPolicy loggingPolicy =
            new ProductionEntityStressLoggingPolicy();
        private readonly ProductionEntityStressPhaseTimingCollector phaseTimingCollector =
            new ProductionEntityStressPhaseTimingCollector();
        private readonly ProductionEntityStressDetailPhaseTimingCollector detailPhaseTimingCollector =
            new ProductionEntityStressDetailPhaseTimingCollector();

        private ProductionEntityStressConfig config;
        private ProductionEntityStressReport report;
        private SimulationTickDriver driver;
        private SimulationWorld world;
        private LF2ObjectPool objectPool;
        private LF2ReferencePool referencePool;
        private LF2ObjectPointFactory objectPointFactory;
        private BattleTickPhaseDiagnostics phaseTimingDiagnostics;
        private BattleTickDetailPhaseDiagnostics detailPhaseTimingDiagnostics;
        private BattleAiInputDetailDiagnostics aiInputDetailDiagnostics;
        private LockstepSimulationSettings previousSettings;
        private bool previousPaused;
        private int objectPoolActiveBaseline;
        private int objectPoolAvailableBaseline;
        private int referencePoolActiveBaseline;
        private int selectedCharacterOid;
        private int frameCounter;
        private float accumulator;
        private bool configured;
        private bool cleaned;
        private bool cleanupInProgress;
        private bool driverConfigurationChanged;

        public static ProductionEntityStressRunner Active { get; private set; }
        public ProductionEntityStressReport Report => report;

        internal static ProductionEntityStressRunner StartRun(ProductionEntityStressConfig runConfig)
        {
            if (!Application.isPlaying)
                throw new InvalidOperationException("Production entity stress requires Play Mode.");
            if (Active != null)
                throw new InvalidOperationException("A production entity stress run is already active.");

            var root = new GameObject($"NTSD Production Entity Stress [{runConfig.Mode}]");
            var runner = root.AddComponent<ProductionEntityStressRunner>();
            Active = runner;
            try
            {
                runner.Configure(runConfig);
                return runner;
            }
            catch
            {
                runner.CleanupInternal("start-failed", true);
                UnityEngine.Object.Destroy(root);
                throw;
            }
        }

        internal static bool AreProductionServicesReady()
        {
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            return SimulationTickDriver.Instance?.World != null &&
                   LF2ObjectPool.Instance != null &&
                   LF2ReferencePool.Instance != null &&
                   LF2ObjectPointFactory.Instance != null &&
                   manager != null &&
                   dataManager != null &&
                   dataManager.IsLoaded() &&
                   TrySelectLoadedCharacter(manager, dataManager, out _);
        }

        internal static Vector3 BuildSpawnPosition(
            ProductionEntityStressMode mode,
            int index,
            int entityCount)
        {
            if (mode == ProductionEntityStressMode.Concentrated1000)
            {
                int column = index % 20;
                int row = (index / 20) % 10;
                return new Vector3(385f + column * 1.5f, 0f, 242f + row * 1.5f);
            }

            int columns = mode == ProductionEntityStressMode.Smoke50 ? 10 : 40;
            int rows = Math.Max(1, (entityCount + columns - 1) / columns);
            int xIndex = index % columns;
            int zIndex = index / columns;
            float x = 20f + xIndex * (760f / Math.Max(1, columns - 1));
            float z = 185f + zIndex * (160f / Math.Max(1, rows - 1));
            return new Vector3(x, 0f, z);
        }

        private void Configure(ProductionEntityStressConfig runConfig)
        {
            config = runConfig;
            report = new ProductionEntityStressReport
            {
                status = "Starting",
                mode = config.Mode.ToString(),
                inputMode = ProductionEntityStressConfig.FormatInputMode(config.InputMode),
                startedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                updatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                scene = gameObject.scene.name,
                stressRootName = gameObject.name,
                outputPath = config.OutputPath,
                requestedEntityCount = config.EntityCount,
                opointCounterAvailable = true,
                opointCounterReason =
                    "Runtime-derived observable proxy: unique active non-harness runtime handles observed " +
                    "after each logic tick. It is not a production opoint creation counter.",
                phaseTimingSource = ProductionEntityStressPhaseTimingCollector.Source,
                detailPhaseTimingEnabled = config.EnableDetailPhaseTiming,
                detailPhaseTimingSource = config.EnableDetailPhaseTiming
                    ? ProductionEntityStressDetailPhaseTimingCollector.Source
                    : string.Empty,
                detailPhaseTimingUnavailableReason = config.EnableDetailPhaseTiming
                    ? string.Empty
                    : "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.",
            };
            loggingPolicy.Apply(report.loggingPolicy);

            driver = SimulationTickDriver.Instance;
            objectPool = LF2ObjectPool.Instance;
            referencePool = LF2ReferencePool.Instance;
            objectPointFactory = LF2ObjectPointFactory.Instance;
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            if (driver == null || objectPool == null || referencePool == null ||
                objectPointFactory == null || manager == null || dataManager == null)
            {
                throw new InvalidOperationException(
                    "Required production services are missing. Load a battle scene before starting the stress harness.");
            }
            if (driver.World == null || driver.World.ObjectCount != 0 ||
                driver.World.ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new InvalidOperationException(
                    "The production stress harness requires an empty SimulationWorld.");
            }
            if (objectPool.ActiveObjectCountForAcceptance != 0 || referencePool.ActiveCount != 0)
            {
                throw new InvalidOperationException(
                    "The production stress harness requires both production pools to have zero active objects.");
            }
            if (!TrySelectLoadedCharacter(manager, dataManager, out selectedCharacterOid))
            {
                throw new InvalidOperationException(
                    "No loaded type-0 character with DAT frames and visible sprites is available.");
            }

            previousSettings = CloneSettings(driver.Settings);
            previousPaused = driver.IsPaused;
            objectPoolActiveBaseline = objectPool.ActiveObjectCountForAcceptance;
            objectPoolAvailableBaseline = objectPool.AvailableObjectCountForAcceptance;
            referencePoolActiveBaseline = referencePool.ActiveCount;

            var settings = new BattleRuntimeWorldSettings(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities,
                CollisionBroadphaseBackend.LooseQuadtree);
            driverConfigurationChanged = true;
            if (!driver.TryConfigureEmptyDiagnosticWorld(settings, out string failureReason))
                throw new InvalidOperationException(failureReason);

            world = driver.World;
            if (world.RuntimeProfileForDiagnostics != BattleRuntimeProfile.MobileExtended ||
                world.RuntimeSlotCapacityForDiagnostics != BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity ||
                world.CollisionBroadphaseForDiagnostics != CollisionBroadphaseBackend.LooseQuadtree)
            {
                throw new InvalidOperationException(
                    "Diagnostic world did not apply MobileExtended(1050) + LooseQuadtree before registration.");
            }
            BruteForceSceneQuery stressSceneQuery =
                ApplyFormalCollectorModeForDiagnostics(world, config.FormalCollectorMode);
            report.formalCollectorRequestedMode =
                ProductionEntityStressConfig.FormatFormalCollectorMode(config.FormalCollectorMode);
            report.formalCollectorMode =
                ProductionEntityStressConfig.FormatFormalCollectorMode(
                    ResolveAppliedFormalCollectorModeForDiagnostics(
                        world,
                        stressSceneQuery));
            report.formalCollectorBodyEntries =
                stressSceneQuery.LastRoleAwareBodyEntryCountForDiagnostics;
            report.formalCollectorItrQueries =
                stressSceneQuery.LastRoleAwareItrQueryCountForDiagnostics;
            phaseTimingDiagnostics = world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            if (config.EnableDetailPhaseTiming)
            {
                detailPhaseTimingDiagnostics =
                    world.EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
                aiInputDetailDiagnostics =
                    world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            }
            report.phaseTimingEnabled = true;
            report.phaseTimingSource = ProductionEntityStressPhaseTimingCollector.Source;

            driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                useUnscaledTime = true,
                maxCatchUpTicksPerFrame = config.MaxCatchUpTicksPerFrame,
                maxBacklogTicks = config.MaxBacklogTicks,
                inputDelayTicks = 0,
                requireInputFrameReady = false,
                enableFrameChecksum = false,
            });
            driver.SetPaused(true);
            configured = true;
            report.selectedCharacterOid = selectedCharacterOid;
            RefreshReportCounts();
            WriteReport();
        }

        private void Update()
        {
            if (!configured || cleaned)
                return;

            try
            {
                double frameMs = Time.unscaledDeltaTime * 1000d;
                if (entities.Count < config.EntityCount)
                {
                    SpawnBatch(config.SpawnBatchSize);
                    if (entities.Count == config.EntityCount)
                    {
                        accumulator = 0f;
                        report.status = "Running";
                        ValidatePeakPopulation();
                        WriteReport();
                    }
                    return;
                }

                RemoveReleasedEntities();
                if (entities.Count < config.EntityCount)
                {
                    SpawnBatch(Math.Min(config.SpawnBatchSize, config.EntityCount - entities.Count));
                    return;
                }

                AddRollingSample(frameSamples, frameMs);
                report.sampledUnityFrames++;
                accumulator += Time.unscaledDeltaTime;
                float maximumAccumulator = SimulationConstants.SIM_DT * config.MaxBacklogTicks;
                if (accumulator > maximumAccumulator)
                {
                    report.droppedBacklogTicks += Mathf.FloorToInt(
                        (accumulator - maximumAccumulator) / SimulationConstants.SIM_DT);
                    accumulator = maximumAccumulator;
                }

                int ticksThisFrame = 0;
                while (accumulator >= SimulationConstants.SIM_DT &&
                       ticksThisFrame < config.MaxCatchUpTicksPerFrame)
                {
                    accumulator -= SimulationConstants.SIM_DT;
                    bool buildPresentation = SimulationTickDriver.IsFinalCatchUpTick(
                        accumulator,
                        ticksThisFrame,
                        config.MaxCatchUpTicksPerFrame);
                    StepMeasuredTick(buildPresentation);
                    ticksThisFrame++;
                }

                if (ticksThisFrame > 1)
                    report.framesWithCatchUp++;
                report.maximumCatchUpTicksInFrame = Math.Max(
                    report.maximumCatchUpTicksInFrame,
                    ticksThisFrame);
                report.currentBacklogTicks = Mathf.FloorToInt(
                    accumulator / SimulationConstants.SIM_DT);
                report.maximumBacklogTicks = Math.Max(
                    report.maximumBacklogTicks,
                    report.currentBacklogTicks);

                frameCounter++;
                if (frameCounter % 30 == 0)
                    WriteReport();

                if (config.AutoCleanup && report.sampledLogicTicks >= config.SampleTicks)
                    StopAndCleanup("smoke-complete");
            }
            catch (Exception exception)
            {
                report.status = "Failed";
                report.failure = exception.ToString();
                CleanupInternal("exception", true);
                ProductionEntityStressPaths.WriteTerminalResult(false, config.OutputPath, exception.Message);
                Debug.LogError($"[ProductionEntityStress] Failed: {exception}");
            }
        }

        internal void StopAndCleanup(string reason)
        {
            if (cleaned)
                return;
            bool smokeCompleted = config.AutoCleanup &&
                                  report.sampledLogicTicks >= config.SampleTicks &&
                                  string.IsNullOrEmpty(report.failure);
            CleanupInternal(reason, true);
            bool success = report.harnessValidity && report.teardown.restored &&
                           (!config.AutoCleanup || smokeCompleted);
            report.status = config.AutoCleanup
                ? (success ? "SmokePassed" : "SmokeFailed")
                : (success ? "StoppedCleanly" : "StoppedWithResidue");
            WriteReport();
            ProductionEntityStressPaths.WriteTerminalResult(
                success,
                config.OutputPath,
                report.teardown.evidence);
            Debug.Log($"[ProductionEntityStress] {report.status}: {config.OutputPath}");
            Destroy(gameObject);
        }

        private void StepMeasuredTick(bool buildPresentation = true)
        {
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            long timestamp = Stopwatch.GetTimestamp();
            bool stepped = driver.StepOneTick(
                ignorePaused: true,
                buildPresentation: buildPresentation);
            double elapsedMs = (Stopwatch.GetTimestamp() - timestamp) * 1000d / Stopwatch.Frequency;
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            if (!stepped)
                throw new InvalidOperationException("Production SimulationTickDriver refused a manual tick.");

            report.logicTicksExecuted++;
            phaseTimingCollector.CaptureAfterTick(
                phaseTimingDiagnostics,
                elapsedMs,
                report.logicTicksExecuted,
                config.WarmupTicks);
            if (config.EnableDetailPhaseTiming)
            {
                detailPhaseTimingCollector.CaptureAfterTick(
                    detailPhaseTimingDiagnostics,
                    aiInputDetailDiagnostics,
                    report.logicTicksExecuted,
                    config.WarmupTicks);
            }
            if (report.logicTicksExecuted <= config.WarmupTicks)
            {
                report.warmupTicksCompleted = report.logicTicksExecuted;
            }
            else
            {
                AddRollingSample(logicTickSamples, elapsedMs);
                AddRollingSample(allocationSamples, allocatedBytes);
                report.sampledLogicTicks++;
            }

            CaptureProductionCounters();
        }

        private void CaptureProductionCounters()
        {
            long candidateCount = 0;
            int aiCount = 0;
            for (int i = 0; i < entities.Count; i++)
            {
                LF2Character entity = entities[i];
                if (!IsActive(entity))
                    continue;
                candidateCount += Math.Max(0, entity.Runtime.HitCandidateCount);
                if (entity.AiControlled)
                    aiCount++;
            }
            report.aiControlledEntityTicks += aiCount;
            report.collisionCandidateCountSum += candidateCount;
            report.collisionCandidateCountPeak = Math.Max(
                report.collisionCandidateCountPeak,
                candidateCount > int.MaxValue ? int.MaxValue : (int)candidateCount);

            if (world.SceneQuery is BruteForceSceneQuery sceneQuery)
            {
                report.formalCollectorMode =
                    ProductionEntityStressConfig.FormatFormalCollectorMode(
                        sceneQuery.LastFormalCollectorModeForDiagnostics);
                report.formalCollectorBodyEntries =
                    sceneQuery.LastRoleAwareBodyEntryCountForDiagnostics;
                report.formalCollectorItrQueries =
                    sceneQuery.LastRoleAwareItrQueryCountForDiagnostics;
                int pairCount = sceneQuery.LastFormalPairCountForDiagnostics;
                report.broadphasePairCountSum += pairCount;
                report.broadphasePairCountPeak = Math.Max(report.broadphasePairCountPeak, pairCount);
                report.broadphaseFallbackParticipantPeak = Math.Max(
                    report.broadphaseFallbackParticipantPeak,
                    sceneQuery.LastFormalFallbackParticipantCountForDiagnostics);
                if (sceneQuery.LastFormalCollectionAbortedForDiagnostics)
                    report.broadphaseAbortedTicks++;
                SpatialSynchronizeResult sync = sceneQuery.LastFormalSynchronizeResultForDiagnostics;
                report.broadphaseLastIndexedCount = sync.IndexedCount;
            }

            report.damageStatTotal = Sum(world.DamageStats);
            report.killStatTotal = Sum(world.KillStats);
            ObserveRuntimeEntitySnapshot();
        }

        internal static BruteForceSceneQuery ApplyFormalCollectorModeForDiagnostics(
            SimulationWorld targetWorld,
            CollisionFormalCollectorMode mode)
        {
            if (targetWorld?.SceneQuery is not BruteForceSceneQuery sceneQuery)
            {
                throw new InvalidOperationException(
                    "Production stress requires BruteForceSceneQuery diagnostics.");
            }

            sceneQuery.FormalCollectorMode = mode;
            return sceneQuery;
        }

        internal static CollisionFormalCollectorMode
            ResolveAppliedFormalCollectorModeForDiagnostics(
                SimulationWorld targetWorld,
                BruteForceSceneQuery sceneQuery)
        {
            if (targetWorld == null)
                throw new ArgumentNullException(nameof(targetWorld));
            if (sceneQuery == null)
                throw new ArgumentNullException(nameof(sceneQuery));
            if (!ReferenceEquals(targetWorld.SceneQuery, sceneQuery))
            {
                throw new ArgumentException(
                    "The scene query does not belong to the supplied stress world.",
                    nameof(sceneQuery));
            }

            if (sceneQuery.FormalCollectorMode != CollisionFormalCollectorMode.Configured)
                return sceneQuery.FormalCollectorMode;

            return targetWorld.CollisionBroadphaseForDiagnostics ==
                   CollisionBroadphaseBackend.LooseQuadtree
                ? CollisionFormalCollectorMode.ForceRoleAware
                : CollisionFormalCollectorMode.ForceBruteForce;
        }

        private void SpawnBatch(int count)
        {
            int remaining = config.EntityCount - entities.Count;
            int spawnCount = Math.Min(Math.Max(0, count), remaining);
            for (int i = 0; i < spawnCount; i++)
            {
                int placementIndex = entities.Count;
                LF2Character entity = SpawnCharacter(placementIndex);
                if (entity == null)
                    throw new InvalidOperationException(
                        $"Production creation chain failed at entity {placementIndex}/{config.EntityCount}.");
                entities.Add(entity);
                report.totalEntitiesCreated++;
            }
            RefreshReportCounts();
        }

        private LF2Character SpawnCharacter(int placementIndex)
        {
            Vector3 position = BuildSpawnPosition(config.Mode, placementIndex, config.EntityCount);
            int team = (placementIndex & 1) + 1;
            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                kind = 1,
                oid = selectedCharacterOid,
                action = 0,
                facing = placementIndex & 1,
            };
            task.team = team;
            task.relationTeam = team;
            task.useExplicitRelationIdentity = true;
            task.dir = (placementIndex & 1) == 0 ? "right" : "left";
            task.useDirectRuntimePosition = true;
            task.directX = position.x;
            task.directY = position.y;
            task.directZ = position.z;
            task.useDirectVelocity = true;
            task.directVx = 0d;
            task.directVy = 0d;
            task.directVz = 0d;
            task.preserveActionZero = true;
            task.skipPostInitZOffset = true;

            LF2Entity spawned;
            try
            {
                spawned = objectPointFactory.CreateObjectImmediate(task);
            }
            finally
            {
                referencePool.Recycle(task);
            }

            if (spawned is not LF2Character character)
            {
                spawned?.OnTransitDestroy();
                return null;
            }
            if (!IsActive(character))
            {
                character.OnTransitDestroy();
                return null;
            }
            GameObject entityRoot = character.Renderer?.transform.parent?.gameObject;
            if (entityRoot == null)
            {
                character.OnTransitDestroy();
                return null;
            }
            entityRoot.name = string.Format(
                CultureInfo.InvariantCulture,
                "StressEntity_{0:D4}_Slot_{1:D4}_Team_{2}",
                report.totalEntitiesCreated,
                character.Runtime.SlotIndex,
                team);
            entityRoot.transform.SetParent(transform, true);
            character.AiControlled = config.InputMode == ProductionEntityStressInputMode.Ai;
            character.Renderer.ForceRefreshPresentation();
            if (world.TryGetCurrentRuntimeHandleForDiagnostics(
                    character.Runtime.SlotIndex,
                    character,
                    out RuntimeEntityHandle handle))
            {
                harnessOwnedHandles.Add(handle);
            }
            return character;
        }

        private void RemoveReleasedEntities()
        {
            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (IsActive(entities[i]))
                    continue;
                entities.RemoveAt(i);
                report.lifecycleReplacements++;
            }
        }

        private void ValidatePeakPopulation()
        {
            RefreshReportCounts();
            report.harnessValidity =
                ProductionEntityStressPopulationPolicy.Evaluate(
                    config.EntityCount,
                    report.activeGameObjectCount,
                    report.stressRootChildCount,
                    report.worldObjectCount,
                    report.worldEntityCount,
                    report.claimedRuntimeSlotCount) &&
                world.RuntimeProfileForDiagnostics == BattleRuntimeProfile.MobileExtended &&
                world.RuntimeSlotCapacityForDiagnostics == BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity &&
                world.CollisionBroadphaseForDiagnostics == CollisionBroadphaseBackend.LooseQuadtree;
            if (!report.harnessValidity)
            {
                throw new InvalidOperationException(
                    "Peak population validation failed; see structured count fields in the report.");
            }
        }

        private void CleanupInternal(string reason, bool restoreDriver)
        {
            if (cleaned || cleanupInProgress)
                return;

            cleanupInProgress = true;
            report ??= new ProductionEntityStressReport();
            var journal = new ProductionEntityStressCleanupJournal();
            try
            {
                ProductionEntityStressPhaseTimingLifecycle.Disable(world);
                report.teardown.attempted = true;
                journal.Attempt("capture-before-state", () =>
                {
                    RefreshReportCounts();
                    report.teardown.activeGameObjectsBefore = report.activeGameObjectCount;
                    report.teardown.worldObjectsBefore = world?.ObjectCount ?? 0;
                    report.teardown.worldEntitiesBefore = CountWorldEntities();
                    report.teardown.claimedSlotsBefore =
                        world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
                    report.teardown.objectPoolActiveBeforeRun = objectPoolActiveBaseline;
                    report.teardown.objectPoolAvailableBeforeRun = objectPoolAvailableBaseline;
                    report.teardown.referencePoolActiveBeforeRun = referencePoolActiveBaseline;
                });

                CleanupActiveRuntimeEntities(journal);
                journal.Attempt("clear-entity-tracking", entities.Clear);

                bool afterStateCaptured = journal.Attempt("capture-after-state", () =>
                {
                    report.teardown.activeGameObjectsAfter = CountActiveEntityGameObjects();
                    report.teardown.worldObjectsAfter = world?.ObjectCount ?? 0;
                    report.teardown.worldEntitiesAfter = CountWorldEntities();
                    report.teardown.claimedSlotsAfter =
                        world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
                    report.teardown.objectPoolActiveAfter =
                        objectPool?.ActiveObjectCountForAcceptance ?? -1;
                    report.teardown.objectPoolAvailableAfter =
                        objectPool?.AvailableObjectCountForAcceptance ?? -1;
                    report.teardown.referencePoolActiveAfter = referencePool?.ActiveCount ?? -1;
                });
                report.teardown.retainedInactiveObjectPoolCapacityBeforeRun =
                    report.teardown.objectPoolAvailableBeforeRun;
                report.teardown.retainedInactiveObjectPoolCapacityAfter =
                    report.teardown.objectPoolAvailableAfter;
                report.teardown.retainedInactiveObjectPoolCapacityDelta =
                    report.teardown.retainedInactiveObjectPoolCapacityAfter -
                    report.teardown.retainedInactiveObjectPoolCapacityBeforeRun;
                report.teardown.retainedInactiveObjectPoolCapacityPolicy =
                    "Informational inactive cache capacity only; it is not active cleanup residue " +
                    "and the stress harness does not trim it.";
                report.teardown.activeStateRestored = afterStateCaptured &&
                    ProductionEntityStressTeardownPolicy.IsRestored(
                        report.teardown.activeGameObjectsAfter,
                        report.teardown.worldObjectsAfter,
                        report.teardown.worldEntitiesAfter,
                        report.teardown.claimedSlotsAfter,
                        report.teardown.objectPoolActiveAfter,
                        report.teardown.objectPoolAvailableAfter,
                        report.teardown.referencePoolActiveAfter,
                        objectPoolActiveBaseline,
                        objectPoolAvailableBaseline,
                        referencePoolActiveBaseline);

                report.teardown.driverStateRestored = !driverConfigurationChanged;
                if (driverConfigurationChanged && restoreDriver && driver != null)
                {
                    bool recreated = journal.Attempt("restore-driver-world", () =>
                    {
                        driver.RecreateWorld();
                        world = driver.World;
                    });
                    bool settingsApplied = previousSettings == null ||
                                           journal.Attempt(
                                               "restore-driver-settings",
                                               () => driver.ApplySettings(previousSettings));
                    bool pauseRestored = journal.Attempt(
                        "restore-driver-pause",
                        () => driver.SetPaused(previousPaused));
                    bool stateVerified = journal.Attempt("verify-driver-state", () =>
                    {
                        if (driver.World == null || driver.IsPaused != previousPaused ||
                            !SettingsMatch(driver.Settings, previousSettings))
                        {
                            throw new InvalidOperationException(
                                "SimulationTickDriver did not match its captured pre-run state.");
                        }
                    });
                    report.teardown.driverStateRestored =
                        recreated && settingsApplied && pauseRestored && stateVerified;
                }
                else if (driverConfigurationChanged)
                {
                    journal.Attempt("restore-driver-state", () =>
                    {
                        throw new InvalidOperationException(
                            restoreDriver
                                ? "SimulationTickDriver was unavailable during cleanup."
                                : "SimulationTickDriver restoration was not permitted in the current lifecycle state.");
                    });
                }

                journal.Attempt("restore-logger", () => loggingPolicy.Restore(report.loggingPolicy));
                report.teardown.loggingStateRestored = report.loggingPolicy.restored;
                report.teardown.cleanupExceptionCount = journal.FailureCount;
                report.teardown.cleanupExceptions = journal.FormatFailures();
                report.teardown.restored = report.teardown.activeStateRestored &&
                                           report.teardown.driverStateRestored &&
                                           report.teardown.loggingStateRestored &&
                                           report.teardown.cleanupExceptionCount == 0;
                report.teardown.evidence =
                    ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                journal.Attempt("refresh-report-metrics", RefreshReportMetrics);

                if (journal.FailureCount != report.teardown.cleanupExceptionCount)
                {
                    report.teardown.cleanupExceptionCount = journal.FailureCount;
                    report.teardown.cleanupExceptions = journal.FormatFailures();
                    report.teardown.restored = false;
                    report.teardown.evidence =
                        ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                }

                report.status = ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    report.status,
                    reason,
                    report.teardown.restored);
                journal.Attempt("write-report", WriteReport);
                if (journal.FailureCount != report.teardown.cleanupExceptionCount)
                {
                    report.teardown.cleanupExceptionCount = journal.FailureCount;
                    report.teardown.cleanupExceptions = journal.FormatFailures();
                    report.teardown.restored = false;
                    report.teardown.evidence =
                        ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                }
            }
            finally
            {
                ProductionEntityStressPhaseTimingLifecycle.Disable(world);
                cleaned = true;
                cleanupInProgress = false;
                if (ReferenceEquals(Active, this))
                    Active = null;
            }
        }

        private void OnDestroy()
        {
            if (!cleaned)
                CleanupInternal("runner-destroyed", restoreDriver: Application.isPlaying);
            if (ReferenceEquals(Active, this))
                Active = null;
        }

        private void RefreshReportCounts()
        {
            if (report == null)
                return;
            report.activeGameObjectCount = CountActiveEntityGameObjects();
            report.stressRootChildCount = transform != null ? transform.childCount : 0;
            report.worldObjectCount = world?.ObjectCount ?? 0;
            report.worldEntityCount = CountWorldEntities();
            report.claimedRuntimeSlotCount = world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
            if (!report.teardown.attempted)
            {
                report.runtimeProfile = world?.RuntimeProfileForDiagnostics.ToString() ?? string.Empty;
                report.runtimeSlotCapacity = world?.RuntimeSlotCapacityForDiagnostics ?? 0;
                report.broadphaseBackend = world?.CollisionBroadphaseForDiagnostics.ToString() ?? string.Empty;
            }
        }

        private void RefreshReportMetrics()
        {
            RefreshReportCounts();
            report.logicTickMilliseconds = ProductionEntityStressStatistics.Summarize(
                logicTickSamples,
                "ms",
                "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick");
            report.unityFrameMilliseconds = ProductionEntityStressStatistics.Summarize(
                frameSamples,
                "ms",
                "Time.unscaledDeltaTime for visible Play Mode frames");
            report.logicTickAllocatedBytes = ProductionEntityStressStatistics.Summarize(
                allocationSamples,
                "bytes",
                "GC.GetAllocatedBytesForCurrentThread around production logic tick");
            phaseTimingCollector.PopulateReport(report);
            detailPhaseTimingCollector.PopulateReport(report);
            report.updatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }

        private void WriteReport()
        {
            if (report == null || string.IsNullOrWhiteSpace(config.OutputPath))
                return;
            RefreshReportMetrics();
            string directory = Path.GetDirectoryName(config.OutputPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(
                config.OutputPath,
                JsonUtility.ToJson(report, true),
                new UTF8Encoding(false));
        }

        private int CountActiveEntityGameObjects()
        {
            return ProductionEntityStressTeardownPolicy.CountActiveStressRootGameObjects(transform);
        }

        private int CountWorldEntities()
        {
            if (world == null)
                return 0;
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch);
            return entityScratch.Count;
        }

        private void ObserveRuntimeEntitySnapshot()
        {
            if (world == null)
                return;

            world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch);
            report.peakWorldEntityCount = Math.Max(report.peakWorldEntityCount, entityScratch.Count);
            for (int i = 0; i < entityScratch.Count; i++)
            {
                LF2Entity entity = entityScratch[i];
                if (entity?.Runtime == null ||
                    !world.TryGetCurrentRuntimeHandleForDiagnostics(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    continue;
                }

                if (ProductionEntityStressDerivedObservationPolicy.TryRecord(
                        handle,
                        harnessOwnedHandles,
                        observedDerivedHandles))
                {
                    report.observedOpointCreates++;
                }
            }
        }

        private void CleanupActiveRuntimeEntities(ProductionEntityStressCleanupJournal journal)
        {
            if (world == null)
                return;

            int previousRemaining = -1;
            for (int pass = 0; pass < MaximumCleanupPasses; pass++)
            {
                bool captured = journal.Attempt(
                    $"capture-world-entities-pass-{pass}",
                    () => world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch));
                if (!captured)
                    break;

                int remaining = entityScratch.Count;
                if (remaining == 0 || remaining >= previousRemaining && previousRemaining >= 0)
                    break;

                previousRemaining = remaining;
                for (int i = 0; i < entityScratch.Count; i++)
                {
                    LF2Entity entity = entityScratch[i];
                    if (entity == null)
                        continue;
                    journal.Attempt(
                        $"release-world-entity-pass-{pass}-index-{i}",
                        entity.OnTransitDestroy);
                }

                journal.Attempt(
                    $"flush-world-destroy-pass-{pass}",
                    world.FlushPendingDestroyForDiagnostics);
            }

            journal.Attempt("flush-world-destroy-final", world.FlushPendingDestroyForDiagnostics);
        }

        private static bool SettingsMatch(
            LockstepSimulationSettings current,
            LockstepSimulationSettings expected)
        {
            if (current == null || expected == null)
                return current == expected;

            return current.driveMode == expected.driveMode &&
                   current.useUnscaledTime == expected.useUnscaledTime &&
                   current.maxCatchUpTicksPerFrame == expected.maxCatchUpTicksPerFrame &&
                   current.maxBacklogTicks == expected.maxBacklogTicks &&
                   current.inputDelayTicks == expected.inputDelayTicks &&
                   current.requireInputFrameReady == expected.requireInputFrameReady &&
                   current.enableFrameChecksum == expected.enableFrameChecksum;
        }

        private static bool TrySelectLoadedCharacter(
            CharacterAnimtorManager manager,
            GameDataManager dataManager,
            out int oid)
        {
            List<int> ids = manager.GetAllLoadedCharacterIds();
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
            {
                int candidate = ids[i];
                ObjectDefinition definition = dataManager.GetObjectById(candidate);
                LF2CharacterDataWrapper config = manager.GetCharacterConfig(candidate);
                if (definition == null || definition.type != (int)LF2ObjectType.Character ||
                    config?.characterData?.frames == null || config.characterData.frames.Count == 0 ||
                    !manager.TryGetSprites(candidate, out List<Sprite> sprites) || sprites == null ||
                    sprites.Count == 0)
                {
                    continue;
                }
                oid = candidate;
                return true;
            }
            oid = -1;
            return false;
        }

        private static LockstepSimulationSettings CloneSettings(LockstepSimulationSettings source)
        {
            if (source == null)
                return new LockstepSimulationSettings();
            return new LockstepSimulationSettings
            {
                driveMode = source.driveMode,
                useUnscaledTime = source.useUnscaledTime,
                maxCatchUpTicksPerFrame = source.maxCatchUpTicksPerFrame,
                maxBacklogTicks = source.maxBacklogTicks,
                inputDelayTicks = source.inputDelayTicks,
                requireInputFrameReady = source.requireInputFrameReady,
                enableFrameChecksum = source.enableFrameChecksum,
            };
        }

        private static bool IsActive(LF2Character entity)
        {
            return entity != null && entity.Renderer != null && entity.Runtime != null &&
                   entity.Runtime.SlotIndex >= 0;
        }

        private static int Sum(int[] values)
        {
            if (values == null)
                return 0;
            long sum = 0;
            for (int i = 0; i < values.Length; i++)
                sum += values[i];
            return sum > int.MaxValue ? int.MaxValue : (int)sum;
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal static class ProductionEntityStressPaths
    {
        internal const string RequestFile = "Temp/NTSD_ProductionEntityStress.request.json";
        internal const string ResultFile = "Temp/NTSD_ProductionEntityStress.result";

        internal static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        internal static string ProjectPath(string path)
        {
            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(ProjectRoot, path));
        }

        internal static void WriteTerminalResult(bool success, string reportPath, string evidence)
        {
            string path = ProjectPath(ResultFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            string content = (success ? "PASS" : "FAIL") + "\n" +
                             (reportPath ?? string.Empty) + "\n" +
                             (evidence ?? string.Empty);
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
    }
}
#endif
