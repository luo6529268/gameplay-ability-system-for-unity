using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

namespace NTSD.Animation.Rendering
{
    public enum BattleRenderingBenchmarkComparison : byte
    {
        Single = 0,
        CentralLegacyAB = 1,
    }

    public enum BattleRenderingBenchmarkScenarioKind : byte
    {
        CurrentScene = 0,
        Entities100 = 1,
        Entities300 = 2,
        Entities500 = 3,
        Entities1000 = 4,
    }

    public enum BattleRenderingBenchmarkVerdict : byte
    {
        Pass = 0,
        Fail = 1,
        Incomplete = 2,
        Unsupported = 3,
    }

    public enum BattleBenchmarkMetricApplicability : byte
    {
        Applicable = 0,
        NotApplicable = 1,
    }

    public enum BattleBenchmarkMetricStatus : byte
    {
        Available = 0,
        Missing = 1,
        NotApplicable = 2,
        Unsupported = 3,
        Passed = 4,
        Failed = 5,
    }

    public readonly struct BattleRenderingBenchmarkScenario
    {
        private BattleRenderingBenchmarkScenario(
            BattleRenderingBenchmarkScenarioKind kind,
            int requestedEntityCount,
            string name)
        {
            Kind = kind;
            RequestedEntityCount = requestedEntityCount;
            Name = name;
        }

        public BattleRenderingBenchmarkScenarioKind Kind { get; }
        public int RequestedEntityCount { get; }
        public string Name { get; }
        public bool UsesCurrentScene => Kind == BattleRenderingBenchmarkScenarioKind.CurrentScene;

        public static BattleRenderingBenchmarkScenario Parse(string value)
        {
            string normalized = string.IsNullOrWhiteSpace(value)
                ? "current-scene"
                : value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "current-scene":
                    return new BattleRenderingBenchmarkScenario(
                        BattleRenderingBenchmarkScenarioKind.CurrentScene,
                        -1,
                        "current-scene");
                case "100":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities100, 100);
                case "300":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities300, 300);
                case "500":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities500, 500);
                case "1000":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities1000, 1000);
                default:
                    throw new ArgumentException(
                        $"Unknown benchmark scenario '{value}'. Expected current-scene, 100, 300, 500, or 1000.",
                        nameof(value));
            }
        }

        private static BattleRenderingBenchmarkScenario Fixed(
            BattleRenderingBenchmarkScenarioKind kind,
            int count)
        {
            return new BattleRenderingBenchmarkScenario(kind, count, count.ToString());
        }
    }

    [Serializable]
    public sealed class BattleRenderingBenchmarkRequest
    {
        public string backend = nameof(BattlePresentationBackendMode.CentralOnly);
        public string comparison = "single";
        public int warmupFrames = 30;
        public int sampleFrames = 120;
        public int leakCheckFrames = 600;
        public long maxManagedGrowthBytes = 1048576L;
        public long maxGraphicsGrowthBytes = 4194304L;
        public string targetActiveEntities = "current-scene";
        public string outputPath = "Temp/NTSD_BattleRenderingBenchmark.json";
    }

    public readonly struct BattleRenderingBenchmarkConfig
    {
        public BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode backend,
            int warmupFrames,
            int sampleFrames,
            string targetActiveEntities,
            string outputPath)
            : this(
                backend,
                BattleRenderingBenchmarkComparison.Single,
                warmupFrames,
                sampleFrames,
                0,
                1048576L,
                4194304L,
                targetActiveEntities,
                outputPath)
        {
        }

        public BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode backend,
            BattleRenderingBenchmarkComparison comparison,
            int warmupFrames,
            int sampleFrames,
            int leakCheckFrames,
            long maxManagedGrowthBytes,
            long maxGraphicsGrowthBytes,
            string targetActiveEntities,
            string outputPath)
        {
            BattlePresentationBackendResolver.ValidateAvailable(backend);
            if (backend == BattlePresentationBackendMode.CentralShadowBuild)
            {
                throw new ArgumentException(
                    "CentralShadowBuild fixes pixel ownership to Legacy and is not a valid P8-D A/B backend.",
                    nameof(backend));
            }
            if (comparison != BattleRenderingBenchmarkComparison.Single &&
                comparison != BattleRenderingBenchmarkComparison.CentralLegacyAB)
            {
                throw new ArgumentOutOfRangeException(nameof(comparison));
            }
            if (warmupFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(warmupFrames));
            if (sampleFrames <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleFrames));
            if (leakCheckFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(leakCheckFrames));
            if (maxManagedGrowthBytes < 0L)
                throw new ArgumentOutOfRangeException(nameof(maxManagedGrowthBytes));
            if (maxGraphicsGrowthBytes < 0L)
                throw new ArgumentOutOfRangeException(nameof(maxGraphicsGrowthBytes));

            Scenario = BattleRenderingBenchmarkScenario.Parse(targetActiveEntities);
            Backend = backend;
            Comparison = comparison;
            WarmupFrames = warmupFrames;
            SampleFrames = sampleFrames;
            LeakCheckFrames = leakCheckFrames;
            MaxManagedGrowthBytes = maxManagedGrowthBytes;
            MaxGraphicsGrowthBytes = maxGraphicsGrowthBytes;
            OutputPath = outputPath ?? string.Empty;
        }

        public BattlePresentationBackendMode Backend { get; }
        public BattleRenderingBenchmarkComparison Comparison { get; }
        public int WarmupFrames { get; }
        public int SampleFrames { get; }
        public int LeakCheckFrames { get; }
        public long MaxManagedGrowthBytes { get; }
        public long MaxGraphicsGrowthBytes { get; }
        public BattleRenderingBenchmarkScenario Scenario { get; }
        public string TargetActiveEntities => Scenario.Name;
        public string OutputPath { get; }

        public static BattleRenderingBenchmarkConfig Default => new BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode.CentralOnly,
            BattleRenderingBenchmarkComparison.CentralLegacyAB,
            30,
            120,
            600,
            1048576L,
            4194304L,
            "current-scene",
            "Temp/NTSD_BattleRenderingBenchmark.json");

        public BattleRenderingBenchmarkConfig ForBackend(BattlePresentationBackendMode backend)
        {
            return new BattleRenderingBenchmarkConfig(
                backend,
                BattleRenderingBenchmarkComparison.Single,
                WarmupFrames,
                SampleFrames,
                LeakCheckFrames,
                MaxManagedGrowthBytes,
                MaxGraphicsGrowthBytes,
                Scenario.Name,
                OutputPath);
        }

        public static BattleRenderingBenchmarkConfig FromRequest(BattleRenderingBenchmarkRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            string backendText = string.IsNullOrWhiteSpace(request.backend)
                ? nameof(BattlePresentationBackendMode.CentralOnly)
                : request.backend;
            if (!BattlePresentationBackendResolver.TryParse(backendText, out BattlePresentationBackendMode backend))
                throw new ArgumentException($"Unknown battle presentation backend '{backendText}'.", nameof(request));
            BattleRenderingBenchmarkComparison comparison = ParseComparison(request.comparison);
            return new BattleRenderingBenchmarkConfig(
                backend,
                comparison,
                request.warmupFrames,
                request.sampleFrames,
                request.leakCheckFrames,
                request.maxManagedGrowthBytes,
                request.maxGraphicsGrowthBytes,
                request.targetActiveEntities,
                request.outputPath);
        }

        private static BattleRenderingBenchmarkComparison ParseComparison(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                string.Equals(value.Trim(), "single", StringComparison.OrdinalIgnoreCase))
            {
                return BattleRenderingBenchmarkComparison.Single;
            }
            if (string.Equals(value.Trim(), "ab", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value.Trim(), "central-legacy-ab", StringComparison.OrdinalIgnoreCase))
            {
                return BattleRenderingBenchmarkComparison.CentralLegacyAB;
            }
            throw new ArgumentException(
                $"Unknown benchmark comparison '{value}'. Expected single or ab.",
                nameof(value));
        }
    }

    public readonly struct BattleBenchmarkMetric
    {
        private BattleBenchmarkMetric(bool available, double value, string unit)
        {
            Available = available;
            Value = value;
            Unit = unit ?? string.Empty;
        }

        public bool Available { get; }
        public double Value { get; }
        public string Unit { get; }

        public static BattleBenchmarkMetric Unavailable(string unit = "") =>
            new BattleBenchmarkMetric(false, 0d, unit);

        public static BattleBenchmarkMetric FromValue(double value, string unit = "") =>
            new BattleBenchmarkMetric(true, value, unit);

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["unit"] = Unit,
                ["value"] = Available ? (object)Value : null,
            };
        }
    }

    public static class BattleRenderingBenchmarkSubmissionPolicy
    {
        public const int Unavailable = -1;

        public static int FromGraphicsDrawMeshCalls(bool callsIssued, int actualCallCount)
        {
            if (!callsIssued)
                return Unavailable;
            if (actualCallCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(actualCallCount),
                    "An available Graphics.DrawMesh submission count must contain at least one actual call.");
            }
            return actualCallCount;
        }
    }

    public static class BattleRenderingBenchmarkPassPolicy
    {
        public static bool Evaluate(
            bool countValidated,
            bool runtimeAdmissionValidated,
            bool logicTickMetricsValidated,
            bool determinismValidated,
            bool rendererWorkloadValidated,
            bool leakRequested,
            bool leakPassed)
        {
            return countValidated &&
                   runtimeAdmissionValidated &&
                   logicTickMetricsValidated &&
                   determinismValidated &&
                   rendererWorkloadValidated &&
                   (!leakRequested || leakPassed);
        }
    }

    public static class BattleRenderingBenchmarkEvidencePolicy
    {
        public static BattleBenchmarkMetricStatus ValidationStatus(bool? observedResult)
        {
            if (!observedResult.HasValue)
                return BattleBenchmarkMetricStatus.Missing;
            return observedResult.Value
                ? BattleBenchmarkMetricStatus.Passed
                : BattleBenchmarkMetricStatus.Failed;
        }
    }

    public static class BattleBenchmarkDrawCallPolicy
    {
        public static BattleBenchmarkMetric RequirePositiveForNonEmptyWorkload(
            BattleBenchmarkMetric metric)
        {
            return metric.Available && metric.Value <= 0d
                ? BattleBenchmarkMetric.Unavailable(metric.Unit)
                : metric;
        }
    }

    public readonly struct BattleRenderingBenchmarkLogicTickSample
    {
        internal BattleRenderingBenchmarkLogicTickSample(
            int tickIndex,
            BattleBenchmarkMetric elapsedMilliseconds,
            BattleBenchmarkMetric allocatedBytes,
            string checksum)
        {
            TickIndex = tickIndex;
            ElapsedMilliseconds = elapsedMilliseconds;
            AllocatedBytes = allocatedBytes;
            Checksum = checksum ?? string.Empty;
        }

        public int TickIndex { get; }
        public BattleBenchmarkMetric ElapsedMilliseconds { get; }
        public BattleBenchmarkMetric AllocatedBytes { get; }
        public string Checksum { get; }

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["allocatedBytes"] = AllocatedBytes.ToProjection(),
                ["checksum"] = Checksum,
                ["elapsedMilliseconds"] = ElapsedMilliseconds.ToProjection(),
                ["tickIndex"] = TickIndex,
            };
        }
    }

    public sealed class BattleBenchmarkMetricAvailability
    {
        internal BattleBenchmarkMetricAvailability(
            string metric,
            string source,
            bool available,
            string reason)
            : this(
                metric,
                required: false,
                BattleBenchmarkMetricApplicability.Applicable,
                available ? BattleBenchmarkMetricStatus.Available : BattleBenchmarkMetricStatus.Missing,
                "completed-frame",
                available ? 1 : 0,
                1,
                source,
                reason)
        {
        }

        public BattleBenchmarkMetricAvailability(
            string metric,
            bool required,
            BattleBenchmarkMetricApplicability applicability,
            BattleBenchmarkMetricStatus status,
            string scope,
            int sampleCount,
            int expectedSampleCount,
            string source,
            string reason)
        {
            Metric = metric ?? string.Empty;
            Required = required;
            Applicability = applicability;
            Status = status;
            Scope = scope ?? string.Empty;
            SampleCount = Math.Max(0, sampleCount);
            ExpectedSampleCount = Math.Max(0, expectedSampleCount);
            Source = source ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public string Metric { get; }
        public bool Required { get; }
        public BattleBenchmarkMetricApplicability Applicability { get; }
        public BattleBenchmarkMetricStatus Status { get; }
        public string Scope { get; }
        public int SampleCount { get; }
        public int ExpectedSampleCount { get; }
        public string Source { get; }
        public bool Available =>
            Status == BattleBenchmarkMetricStatus.Available ||
            Status == BattleBenchmarkMetricStatus.Passed;
        public string Reason { get; }

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["applicability"] = Applicability.ToString(),
                ["expectedSampleCount"] = ExpectedSampleCount,
                ["metric"] = Metric,
                ["reason"] = Reason,
                ["required"] = Required,
                ["sampleCount"] = SampleCount,
                ["scope"] = Scope,
                ["source"] = Source,
                ["status"] = Status.ToString(),
            };
        }
    }

    public readonly struct BattleRenderingBenchmarkPolicyContext
    {
        public BattleRenderingBenchmarkPolicyContext(
            bool isPlaying,
            bool isEditor,
            RuntimePlatform platform,
            bool graphicsMultiThreaded,
            bool frameTimingStatsEnabled)
        {
            IsPlaying = isPlaying;
            IsEditor = isEditor;
            Platform = platform;
            GraphicsMultiThreaded = graphicsMultiThreaded;
            FrameTimingStatsEnabled = frameTimingStatsEnabled;
        }

        public bool IsPlaying { get; }
        public bool IsEditor { get; }
        public RuntimePlatform Platform { get; }
        public bool GraphicsMultiThreaded { get; }
        public bool FrameTimingStatsEnabled { get; }
        public bool IsWindowsStandalone =>
            Platform == RuntimePlatform.WindowsPlayer;
        public bool IsSupportedExecutionScope =>
            IsPlaying && (IsEditor || IsWindowsStandalone);
        public string Scope => !IsPlaying
            ? "EditMode"
            : IsEditor
                ? "PlayModeEditor"
                : IsWindowsStandalone
                    ? "WindowsStandalone"
                    : Platform.ToString();

        internal static BattleRenderingBenchmarkPolicyContext Capture()
        {
            return new BattleRenderingBenchmarkPolicyContext(
                Application.isPlaying,
                Application.isEditor,
                Application.platform,
                SystemInfo.graphicsMultiThreaded,
                FrameTimingManager.IsFeatureEnabled());
        }
    }

    public static class BattleRenderingBenchmarkVerdictPolicy
    {
        public const string PolicyId = "ntsd-battle-rendering-benchmark-policy-v5";

        private static readonly string[] MandatoryMetricNames =
        {
            "frameTimeMs",
            "mainThreadTimeMs",
            "renderThreadTimeMs",
            "gpuFrameTimeMs",
            "managedAllocationBytes",
            "drawCalls",
            "totalAllocatedMemoryBytes",
            "graphicsMemoryBytes",
            "benchmarkOwnedTextureMemoryBytes",
            "logicTickTimeMs",
            "logicTickAllocatedBytes",
            "presentationBuildTimeMs",
            "presenterSubmittedRenderItems",
            "resourceSegments",
            "benchmarkOwnedMemoryBytes",
            "presenterSubmissionDrawCalls",
            "meshChunks",
            "exactSampleCount",
            "countValidated",
            "runtimeAdmissionValidated",
            "determinismValidated",
            "rendererWorkloadValidated",
            "leakCheck",
        };
        private static readonly IReadOnlyList<string> MandatoryMetricRegistry =
            Array.AsReadOnly(MandatoryMetricNames);

        public static IReadOnlyList<string> RequiredMetricNames => MandatoryMetricRegistry;

        public static BattleRenderingBenchmarkVerdict Evaluate(
            BattleRenderingBenchmarkPolicyContext context,
            IReadOnlyList<BattleBenchmarkMetricAvailability> metrics,
            out string reason,
            out string[] missingRequiredMetrics)
        {
            var missing = new List<string>();
            var failed = new List<string>();
            var metricCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var mandatoryNames = new HashSet<string>(MandatoryMetricNames, StringComparer.Ordinal);
            for (int index = 0; index < metrics.Count; index++)
            {
                BattleBenchmarkMetricAvailability metric = metrics[index];
                metricCounts.TryGetValue(metric.Metric, out int count);
                metricCounts[metric.Metric] = count + 1;
                if (!mandatoryNames.Contains(metric.Metric))
                    missing.Add(metric.Metric + " (unknown schema entry)");
                if (!metric.Required ||
                    metric.Applicability == BattleBenchmarkMetricApplicability.NotApplicable)
                {
                    continue;
                }

                if (metric.Status == BattleBenchmarkMetricStatus.Failed)
                    failed.Add(metric.Metric);
                else if (!metric.Available)
                    missing.Add(metric.Metric);
            }

            for (int index = 0; index < MandatoryMetricNames.Length; index++)
            {
                string metricName = MandatoryMetricNames[index];
                metricCounts.TryGetValue(metricName, out int count);
                if (count == 0)
                    missing.Add(metricName);
                else if (count > 1)
                    missing.Add(metricName + " (duplicate schema entry)");
            }
            missingRequiredMetrics = missing.ToArray();

            if (!context.IsPlaying)
            {
                reason = "EditMode does not provide a completed rendered-frame benchmark scope.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (!context.IsSupportedExecutionScope)
            {
                reason = "The v5 policy supports Play Mode Editor and Windows Standalone only.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (!context.FrameTimingStatsEnabled)
            {
                reason = "FrameTimingManager is disabled; completed-frame CPU/GPU metrics are unsupported.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (failed.Count > 0)
            {
                reason = "Required validation gates failed: " + string.Join(", ", failed) + ".";
                return BattleRenderingBenchmarkVerdict.Fail;
            }
            if (missing.Count > 0)
            {
                reason = "Required metrics are missing or incomplete: " + string.Join(", ", missing) + ".";
                return BattleRenderingBenchmarkVerdict.Incomplete;
            }

            reason = "All required v5 metrics and validation gates passed.";
            return BattleRenderingBenchmarkVerdict.Pass;
        }
    }

    public sealed class BattleRenderingBenchmarkFrame
    {
        internal BattleRenderingBenchmarkFrame(int frameIndex, int presentationEntityCount, int commandCount)
        {
            FrameIndex = frameIndex;
            PresentationEntityCount = presentationEntityCount;
            CommandCount = commandCount;
        }

        public int FrameIndex { get; }
        public int PresentationEntityCount { get; }
        public int CommandCount { get; }
        public BattleBenchmarkMetric FrameTimeMs { get; internal set; }
        public BattleBenchmarkMetric MainThreadTimeMs { get; internal set; }
        public BattleBenchmarkMetric RenderThreadTimeMs { get; internal set; }
        public BattleBenchmarkMetric GpuFrameTimeMs { get; internal set; }
        public BattleBenchmarkMetric LogicTickTimeMs { get; internal set; }
        public BattleBenchmarkMetric LogicTickAllocatedBytes { get; internal set; }
        public string LogicTickChecksum { get; internal set; } = string.Empty;
        public BattleBenchmarkMetric PresentationBuildTimeMs { get; internal set; }
        public BattleBenchmarkMetric ManagedAllocationBytes { get; internal set; }
        public BattleBenchmarkMetric DrawCalls { get; internal set; }
        public BattleBenchmarkMetric PresenterSubmittedRenderItems { get; internal set; }
        public BattleBenchmarkMetric PresenterSubmissionDrawCalls { get; internal set; }
        public BattleBenchmarkMetric TotalAllocatedMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric GraphicsMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric BenchmarkOwnedTextureMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric BenchmarkOwnedMemoryBytes { get; internal set; }
        public int BenchmarkResourceGeneration { get; internal set; }
        public BattleBenchmarkMetric SourceCommands { get; internal set; }
        public BattleBenchmarkMetric ResolvedCommands { get; internal set; }
        public BattleBenchmarkMetric UnresolvedCommands { get; internal set; }
        public BattleBenchmarkMetric ResourceSegments { get; internal set; }
        public BattleBenchmarkMetric MeshChunks { get; internal set; }
        public string RequestedBackend { get; internal set; } = string.Empty;
        public string EffectiveBackend { get; internal set; } = string.Empty;

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["presentationEntityCount"] = PresentationEntityCount,
                ["benchmarkOwnedMemoryBytes"] = BenchmarkOwnedMemoryBytes.ToProjection(),
                ["benchmarkOwnedTextureMemoryBytes"] = BenchmarkOwnedTextureMemoryBytes.ToProjection(),
                ["benchmarkResourceGeneration"] = BenchmarkResourceGeneration,
                ["commandCount"] = CommandCount,
                ["drawCalls"] = DrawCalls.ToProjection(),
                ["effectiveBackend"] = EffectiveBackend,
                ["frameIndex"] = FrameIndex,
                ["frameTimeMs"] = FrameTimeMs.ToProjection(),
                ["gpuFrameTimeMs"] = GpuFrameTimeMs.ToProjection(),
                ["graphicsMemoryBytes"] = GraphicsMemoryBytes.ToProjection(),
                ["logicTickTimeMs"] = LogicTickTimeMs.ToProjection(),
                ["logicTickAllocatedBytes"] = LogicTickAllocatedBytes.ToProjection(),
                ["logicTickChecksum"] = LogicTickChecksum,
                ["mainThreadTimeMs"] = MainThreadTimeMs.ToProjection(),
                ["managedAllocationBytes"] = ManagedAllocationBytes.ToProjection(),
                ["meshChunks"] = MeshChunks.ToProjection(),
                ["presentationBuildTimeMs"] = PresentationBuildTimeMs.ToProjection(),
                ["presenterSubmittedRenderItems"] = PresenterSubmittedRenderItems.ToProjection(),
                ["presenterSubmissionDrawCalls"] = PresenterSubmissionDrawCalls.ToProjection(),
                ["renderThreadTimeMs"] = RenderThreadTimeMs.ToProjection(),
                ["requestedBackend"] = RequestedBackend,
                ["resolvedCommands"] = ResolvedCommands.ToProjection(),
                ["resourceSegments"] = ResourceSegments.ToProjection(),
                ["sourceCommands"] = SourceCommands.ToProjection(),
                ["totalAllocatedMemoryBytes"] = TotalAllocatedMemoryBytes.ToProjection(),
                ["unresolvedCommands"] = UnresolvedCommands.ToProjection(),
            };
        }
    }

    public sealed class BattleRenderingBenchmarkLeakReport
    {
        internal BattleRenderingBenchmarkLeakReport(
            bool available,
            bool passed,
            int soakFrames,
            long prePresenterManaged,
            long prePresenterGraphics,
            bool prePresenterGraphicsAvailable,
            long managedStart,
            long managedEnd,
            long graphicsStart,
            long graphicsEnd,
            bool graphicsAvailable,
            long ownedStart,
            long ownedEnd,
            int resourcesStart,
            int resourcesEnd,
            long maxManagedGrowth,
            long maxGraphicsGrowth,
            int teardownFrames,
            long teardownManagedEnd,
            long teardownGraphicsEnd,
            bool teardownGraphicsAvailable,
            long teardownOwnedEnd,
            int teardownResourcesEnd,
            BattleBenchmarkMetricStatus teardownStatus,
            string teardownReason,
            string measurementMode,
            string reason)
        {
            Available = available;
            Passed = passed;
            SoakFrames = soakFrames;
            PrePresenterManagedBytes = prePresenterManaged;
            PrePresenterGraphicsBytes = prePresenterGraphics;
            PrePresenterGraphicsAvailable = prePresenterGraphicsAvailable;
            ManagedStartBytes = managedStart;
            ManagedEndBytes = managedEnd;
            GraphicsStartBytes = graphicsStart;
            GraphicsEndBytes = graphicsEnd;
            GraphicsAvailable = graphicsAvailable;
            OwnedStartBytes = ownedStart;
            OwnedEndBytes = ownedEnd;
            ResourcesStart = resourcesStart;
            ResourcesEnd = resourcesEnd;
            MaxManagedGrowthBytes = maxManagedGrowth;
            MaxGraphicsGrowthBytes = maxGraphicsGrowth;
            TeardownFrames = teardownFrames;
            TeardownManagedEndBytes = teardownManagedEnd;
            TeardownGraphicsEndBytes = teardownGraphicsEnd;
            TeardownGraphicsAvailable = teardownGraphicsAvailable;
            TeardownOwnedEndBytes = teardownOwnedEnd;
            TeardownResourcesEnd = teardownResourcesEnd;
            TeardownStatus = teardownStatus;
            TeardownReason = teardownReason ?? string.Empty;
            MeasurementMode = measurementMode ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public bool Available { get; }
        public bool Passed { get; }
        public int SoakFrames { get; }
        public long PrePresenterManagedBytes { get; }
        public long PrePresenterGraphicsBytes { get; }
        public bool PrePresenterGraphicsAvailable { get; }
        public long ManagedStartBytes { get; }
        public long ManagedEndBytes { get; }
        public long GraphicsStartBytes { get; }
        public long GraphicsEndBytes { get; }
        public bool GraphicsAvailable { get; }
        public long OwnedStartBytes { get; }
        public long OwnedEndBytes { get; }
        public int ResourcesStart { get; }
        public int ResourcesEnd { get; }
        public long MaxManagedGrowthBytes { get; }
        public long MaxGraphicsGrowthBytes { get; }
        public int TeardownFrames { get; }
        public long TeardownManagedEndBytes { get; }
        public long TeardownGraphicsEndBytes { get; }
        public bool TeardownGraphicsAvailable { get; }
        public long TeardownOwnedEndBytes { get; }
        public int TeardownResourcesEnd { get; }
        public BattleBenchmarkMetricStatus TeardownStatus { get; }
        public string TeardownReason { get; }
        public string MeasurementMode { get; }
        public string Reason { get; }
        public long ManagedGrowthBytes => ManagedEndBytes - ManagedStartBytes;
        public long GraphicsGrowthBytes => GraphicsEndBytes - GraphicsStartBytes;
        public long OwnedGrowthBytes => OwnedEndBytes - OwnedStartBytes;
        public long TeardownManagedGrowthBytes => TeardownManagedEndBytes - ManagedStartBytes;
        public long TeardownGraphicsGrowthBytes => TeardownGraphicsEndBytes - GraphicsStartBytes;

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["graphicsAvailable"] = GraphicsAvailable,
                ["graphicsEndBytes"] = GraphicsAvailable ? (object)GraphicsEndBytes : null,
                ["graphicsGrowthBytes"] = GraphicsAvailable ? (object)GraphicsGrowthBytes : null,
                ["graphicsStartBytes"] = GraphicsAvailable ? (object)GraphicsStartBytes : null,
                ["prePresenterGraphicsAvailable"] = PrePresenterGraphicsAvailable,
                ["prePresenterGraphicsBytes"] = PrePresenterGraphicsAvailable
                    ? (object)PrePresenterGraphicsBytes
                    : null,
                ["prePresenterManagedBytes"] = Available ? (object)PrePresenterManagedBytes : null,
                ["managedEndBytes"] = Available ? (object)ManagedEndBytes : null,
                ["managedGrowthBytes"] = Available ? (object)ManagedGrowthBytes : null,
                ["managedStartBytes"] = Available ? (object)ManagedStartBytes : null,
                ["maxGraphicsGrowthBytes"] = MaxGraphicsGrowthBytes,
                ["maxManagedGrowthBytes"] = MaxManagedGrowthBytes,
                ["measurementMode"] = MeasurementMode,
                ["ownedEndBytes"] = Available ? (object)OwnedEndBytes : null,
                ["ownedGrowthBytes"] = Available ? (object)OwnedGrowthBytes : null,
                ["ownedStartBytes"] = Available ? (object)OwnedStartBytes : null,
                ["passed"] = Available ? (object)Passed : null,
                ["reason"] = Reason,
                ["resourcesEnd"] = Available ? (object)ResourcesEnd : null,
                ["resourcesStart"] = Available ? (object)ResourcesStart : null,
                ["soakFrames"] = SoakFrames,
                ["teardownFrames"] = TeardownFrames,
                ["teardownGraphicsAvailable"] = TeardownGraphicsAvailable,
                ["teardownGraphicsEndBytes"] = TeardownGraphicsAvailable
                    ? (object)TeardownGraphicsEndBytes
                    : null,
                ["teardownGraphicsGrowthBytes"] = TeardownGraphicsAvailable
                    ? (object)TeardownGraphicsGrowthBytes
                    : null,
                ["teardownManagedEndBytes"] = Available ? (object)TeardownManagedEndBytes : null,
                ["teardownManagedGrowthBytes"] = Available ? (object)TeardownManagedGrowthBytes : null,
                ["teardownMemoryBaseline"] =
                    "steady-state soak baseline; pre-presenter fields are initialization diagnostics only",
                ["teardownOwnedEndBytes"] = Available ? (object)TeardownOwnedEndBytes : null,
                ["teardownReason"] = TeardownReason,
                ["teardownResourcesEnd"] = Available ? (object)TeardownResourcesEnd : null,
                ["teardownStatus"] = TeardownStatus.ToString(),
            };
        }

        internal static BattleRenderingBenchmarkLeakReport NotRequested()
        {
            return NotRun("Leak/long-run soak was not requested.", "not-requested", BattleBenchmarkMetricStatus.NotApplicable);
        }

        internal static BattleRenderingBenchmarkLeakReport NotRun(string reason)
        {
            return NotRun(reason, "not-run", BattleBenchmarkMetricStatus.Missing);
        }

        private static BattleRenderingBenchmarkLeakReport NotRun(
            string reason,
            string measurementMode,
            BattleBenchmarkMetricStatus teardownStatus)
        {
            return new BattleRenderingBenchmarkLeakReport(
                false,
                false,
                0,
                0L,
                0L,
                false,
                0L,
                0L,
                0L,
                0L,
                false,
                0L,
                0L,
                0,
                0,
                0L,
                0L,
                0,
                0L,
                0L,
                false,
                0L,
                0,
                teardownStatus,
                reason,
                measurementMode,
                reason);
        }
    }

        public sealed class BattleRenderingBenchmarkReport
    {
        internal BattleRenderingBenchmarkReport(
            BattleRenderingBenchmarkConfig config,
            BattleRenderingBenchmarkFrame[] frames,
            BattleBenchmarkMetricAvailability[] metricAvailability,
            BattleRenderingBenchmarkPolicyContext policyContext,
            int requestedPresentationEntityCount,
            int actualPresentationEntityCount,
            int commandCount,
            string workloadFingerprint,
            string presenterImplementation,
            string resourceMode,
            string drawMode,
            int benchmarkRenderTargetWidth,
            int benchmarkRenderTargetHeight,
            bool countValidated,
            bool runtimeAdmissionValidated,
            bool logicTickMetricsValidated,
            bool determinismValidated,
            bool rendererWorkloadValidated,
            BattleRenderingBenchmarkLeakReport leakReport)
        {
            Config = config;
            Frames = frames ?? Array.Empty<BattleRenderingBenchmarkFrame>();
            MetricAvailability = metricAvailability ?? Array.Empty<BattleBenchmarkMetricAvailability>();
            PolicyContext = policyContext;
            RequestedPresentationEntityCount = requestedPresentationEntityCount;
            ActualPresentationEntityCount = actualPresentationEntityCount;
            CommandCount = commandCount;
            WorkloadFingerprint = workloadFingerprint ?? string.Empty;
            PresenterImplementation = presenterImplementation ?? string.Empty;
            ResourceMode = resourceMode ?? string.Empty;
            DrawMode = drawMode ?? string.Empty;
            BenchmarkRenderTargetWidth = benchmarkRenderTargetWidth;
            BenchmarkRenderTargetHeight = benchmarkRenderTargetHeight;
            CountValidated = countValidated;
            RuntimeAdmissionValidated = runtimeAdmissionValidated;
            LogicTickMetricsValidated = logicTickMetricsValidated;
            DeterminismValidated = determinismValidated;
            RendererWorkloadValidated = rendererWorkloadValidated;
            LeakReport = leakReport ?? BattleRenderingBenchmarkLeakReport.NotRequested();
            Verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                PolicyContext,
                MetricAvailability,
                out string verdictReason,
                out string[] missingRequiredMetrics);
            VerdictReason = verdictReason;
            MissingRequiredMetrics = missingRequiredMetrics;
        }

        public BattleRenderingBenchmarkConfig Config { get; }
        public IReadOnlyList<BattleRenderingBenchmarkFrame> Frames { get; }
        public IReadOnlyList<BattleBenchmarkMetricAvailability> MetricAvailability { get; }
        internal BattleRenderingBenchmarkPolicyContext PolicyContext { get; }
        public int RequestedPresentationEntityCount { get; }
        public int ActualPresentationEntityCount { get; }
        public int CommandCount { get; }
        public string WorkloadFingerprint { get; }
        public string PresenterImplementation { get; }
        public string ResourceMode { get; }
        public string DrawMode { get; }
        public int BenchmarkRenderTargetWidth { get; }
        public int BenchmarkRenderTargetHeight { get; }
        public bool CountValidated { get; }
        public bool RuntimeAdmissionValidated { get; }
        public bool LogicTickMetricsValidated { get; }
        public bool DeterminismValidated { get; }
        public bool RendererWorkloadValidated { get; }
        public int RuntimeObjectCount { get; internal set; }
        public int RuntimeSlotCapacity { get; internal set; }
        public string RuntimeProfile { get; internal set; } = string.Empty;
        public int WarmupLogicTickCount { get; internal set; }
        public int SampleLogicTickCount { get; internal set; }
        public string InputFingerprint { get; internal set; } = string.Empty;
        public string InitialRuntimeChecksum { get; internal set; } = string.Empty;
        public string FinalRuntimeChecksum { get; internal set; } = string.Empty;
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> WarmupLogicTickSamples
        {
            get;
            internal set;
        } = Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> SampleLogicTickSamples
        {
            get;
            internal set;
        } = Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
        public BattleRenderingBenchmarkLeakReport LeakReport { get; }
        public BattleRenderingBenchmarkVerdict Verdict { get; }
        public string VerdictReason { get; }
        public IReadOnlyList<string> MissingRequiredMetrics { get; }
        public int CompletedFrameRejectedAttemptCount { get; internal set; }
        public int MaxCompletedFrameSampleAttempts { get; internal set; }
        public string CompletedFrameSamplingFailureReason { get; internal set; } = string.Empty;
        public bool Passed => Verdict == BattleRenderingBenchmarkVerdict.Pass;

        public string ToJson()
        {
            return BattleCanonicalJson.Serialize(ToProjection(true));
        }

        internal Dictionary<string, object> ToProjection(bool includeEnvironment)
        {
            var frameProjection = new List<object>(Frames.Count);
            for (int i = 0; i < Frames.Count; i++)
                frameProjection.Add(Frames[i].ToProjection());
            var availability = new List<object>(MetricAvailability.Count);
            var unavailable = new List<object>();
            for (int i = 0; i < MetricAvailability.Count; i++)
            {
                BattleBenchmarkMetricAvailability item = MetricAvailability[i];
                availability.Add(item.ToProjection());
                if (!item.Available)
                    unavailable.Add(item.Metric);
            }
            var missingRequired = new List<object>(MissingRequiredMetrics.Count);
            for (int index = 0; index < MissingRequiredMetrics.Count; index++)
                missingRequired.Add(MissingRequiredMetrics[index]);

            var config = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["backend"] = Config.Backend.ToString(),
                ["leakCheckFrames"] = Config.LeakCheckFrames,
                ["maxCompletedFrameSampleAttempts"] = BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts,
                ["sampleFrames"] = Config.SampleFrames,
                ["targetActiveEntities"] = Config.TargetActiveEntities,
                ["warmupFrames"] = Config.WarmupFrames,
            };
            var workload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["actualPresentationEntityCount"] = ActualPresentationEntityCount,
                ["commandCount"] = CommandCount,
                ["countValidated"] = CountValidated,
                ["runtimeAdmissionValidated"] = RuntimeAdmissionValidated,
                ["logicTickMetricsValidated"] = LogicTickMetricsValidated,
                ["determinismValidated"] = DeterminismValidated,
                ["fingerprint"] = WorkloadFingerprint,
                ["frozenPresentationFrame"] = true,
                ["gameplayRuntimeMutated"] = false,
                ["requestedPresentationEntityCount"] = RequestedPresentationEntityCount,
                ["rendererWorkloadValidated"] = RendererWorkloadValidated,
                ["runtimeObjectCount"] = RuntimeObjectCount,
                ["runtimeProfile"] = RuntimeProfile,
                ["runtimeSlotCapacity"] = RuntimeSlotCapacity,
                ["warmupLogicTickCount"] = WarmupLogicTickCount,
                ["sampleLogicTickCount"] = SampleLogicTickCount,
                ["inputFingerprint"] = InputFingerprint,
                ["initialRuntimeChecksum"] = InitialRuntimeChecksum,
                ["finalRuntimeChecksum"] = FinalRuntimeChecksum,
                ["scenario"] = Config.Scenario.Name,
                ["source"] = Config.Scenario.UsesCurrentScene
                    ? "current-scene-frozen-presentation-frame"
                    : "deterministic-mobileextended-runtime-fixture-v1",
                ["workloadKind"] = Config.Scenario.UsesCurrentScene
                    ? "frozen-current-scene-presentation"
                    : "frozen-real-runtime-presentation",
            };
            var warmupLogicTicks = new List<object>(WarmupLogicTickSamples.Count);
            for (int index = 0; index < WarmupLogicTickSamples.Count; index++)
                warmupLogicTicks.Add(WarmupLogicTickSamples[index].ToProjection());
            var sampleLogicTicks = new List<object>(SampleLogicTickSamples.Count);
            for (int index = 0; index < SampleLogicTickSamples.Count; index++)
                sampleLogicTicks.Add(SampleLogicTickSamples[index].ToProjection());
            var runtimeTrace = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["finalChecksum"] = FinalRuntimeChecksum,
                ["initialChecksum"] = InitialRuntimeChecksum,
                ["inputFingerprint"] = InputFingerprint,
                ["profile"] = RuntimeProfile,
                ["fixtureInput"] = "FrameInputSet.Empty for every logic tick",
                ["fixtureInteraction"] = Config.Scenario.UsesCurrentScene
                    ? "production current-scene runtime"
                    : "non-interacting LF2Entity fixtures with collision candidates explicitly suppressed",
                ["rngInitialSeed"] = Config.Scenario.UsesCurrentScene
                    ? "captured production state"
                    : "0x4E545344",
                ["runtimeObjectCount"] = RuntimeObjectCount,
                ["runtimeSlotCapacity"] = RuntimeSlotCapacity,
                ["sampleTicks"] = sampleLogicTicks,
                ["warmupTicks"] = warmupLogicTicks,
            };
            var limitations = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["drawAndGpuCounterScope"] =
                    "ProfilerRecorder frame counters include the complete Editor/Player frame; presenter-specific work is separately reported.",
                ["renderTargetScope"] =
                    $"Screen resolution describes the Editor/Player window; the benchmark presentation workload renders to {BenchmarkRenderTargetWidth}x{BenchmarkRenderTargetHeight}.",
                ["legacyPresenterScope"] =
                    "LegacyOnly uses a benchmark-only SpriteRenderer compatibility presenter because production battle prefabs are rendererless.",
                ["legacyVisualParityClaimed"] = false,
                ["logicTickMeasured"] = LogicTickMetricsValidated,
                ["logicTickReason"] = LogicTickMetricsValidated
                    ? "Full NTSDBattleTickSystem ticks were measured locally with Stopwatch and thread allocation counters."
                    : "No reliable full logic-tick sample was observed for this current-scene capture.",
                ["runtimeActiveEntityCapacityClaimed"] = RuntimeAdmissionValidated,
                ["runtimeActiveEntityLimitation"] = Config.Scenario.UsesCurrentScene
                    ? "The scene frame was frozen at benchmark start; runtime admission reflects the active production world at capture time."
                    : "Fixed scenarios register exactly the requested LF2Entity fixtures in a MobileExtended(1050) SimulationWorld.",
                ["productionAtlasPerformanceClaimed"] = false,
                ["productionAtlasLimitation"] =
                    "The deterministic A/B resolver uses one shared SourceTexture2D so both presenters consume identical drawable resources; production atlas modes require a separate current production-scene sample.",
                ["benchmarkOwnedTextureMemoryScope"] =
                    "benchmarkOwnedTextureMemoryBytes sums Profiler.GetRuntimeMemorySizeLong for the Texture2D and RenderTexture objects owned by the reported benchmarkResourceGeneration. It excludes global Editor/Player textures, production atlas resources, and non-texture presenter resources.",
            };
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["config"] = config,
                ["benchmarkRenderTargetHeight"] = BenchmarkRenderTargetHeight,
                ["benchmarkRenderTargetWidth"] = BenchmarkRenderTargetWidth,
                ["completedFrameSampling"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["acceptedSampleCount"] = Frames.Count,
                    ["maxAttemptsPerSample"] = MaxCompletedFrameSampleAttempts,
                    ["rejectedAttemptCount"] = CompletedFrameRejectedAttemptCount,
                    ["terminalFailureReason"] = string.IsNullOrEmpty(CompletedFrameSamplingFailureReason)
                        ? null
                        : (object)CompletedFrameSamplingFailureReason,
                },
                ["frames"] = frameProjection,
                ["leakCheck"] = LeakReport.ToProjection(),
                ["limitations"] = limitations,
                ["metricAvailability"] = availability,
                ["missingRequiredMetrics"] = missingRequired,
                ["policyId"] = BattleRenderingBenchmarkVerdictPolicy.PolicyId,
                ["drawMode"] = DrawMode,
                ["presenterImplementation"] = PresenterImplementation,
                ["resourceMode"] = ResourceMode,
                ["runtimeTrace"] = runtimeTrace,
                ["passed"] = Passed,
                ["reason"] = VerdictReason,
                ["schema"] = "ntsd-battle-rendering-benchmark-run-v5",
                ["summary"] = BuildSummary(),
                ["unavailableMetrics"] = unavailable,
                ["verdict"] = Verdict.ToString(),
                ["workload"] = workload,
            };
            if (includeEnvironment)
                root["environment"] = BattleRenderingBenchmarkEnvironment.Capture();
            return root;
        }

        private Dictionary<string, object> BuildSummary()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["benchmarkOwnedMemoryBytes"] = Summarize(frame => frame.BenchmarkOwnedMemoryBytes),
                ["benchmarkOwnedTextureMemoryBytes"] = Summarize(frame => frame.BenchmarkOwnedTextureMemoryBytes),
                ["drawCalls"] = Summarize(frame => frame.DrawCalls),
                ["frameTimeMs"] = Summarize(frame => frame.FrameTimeMs),
                ["gpuFrameTimeMs"] = Summarize(frame => frame.GpuFrameTimeMs),
                ["graphicsMemoryBytes"] = Summarize(frame => frame.GraphicsMemoryBytes),
                ["logicTickTimeMs"] = Summarize(frame => frame.LogicTickTimeMs),
                ["logicTickAllocatedBytes"] = Summarize(frame => frame.LogicTickAllocatedBytes),
                ["mainThreadTimeMs"] = Summarize(frame => frame.MainThreadTimeMs),
                ["managedAllocationBytes"] = Summarize(frame => frame.ManagedAllocationBytes),
                ["presentationBuildTimeMs"] = Summarize(frame => frame.PresentationBuildTimeMs),
                ["presenterSubmittedRenderItems"] = Summarize(frame => frame.PresenterSubmittedRenderItems),
                ["presenterSubmissionDrawCalls"] = Summarize(frame => frame.PresenterSubmissionDrawCalls),
                ["renderThreadTimeMs"] = Summarize(frame => frame.RenderThreadTimeMs),
                ["resourceSegments"] = Summarize(frame => frame.ResourceSegments),
                ["totalAllocatedMemoryBytes"] = Summarize(frame => frame.TotalAllocatedMemoryBytes),
            };
        }

        private Dictionary<string, object> Summarize(
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector)
        {
            int count = 0;
            double sum = 0d;
            double min = double.MaxValue;
            double max = double.MinValue;
            string unit = string.Empty;
            for (int index = 0; index < Frames.Count; index++)
            {
                BattleBenchmarkMetric metric = selector(Frames[index]);
                if (!metric.Available)
                    continue;
                count++;
                sum += metric.Value;
                min = Math.Min(min, metric.Value);
                max = Math.Max(max, metric.Value);
                unit = metric.Unit;
            }
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = count > 0,
                ["average"] = count > 0 ? (object)(sum / count) : null,
                ["maximum"] = count > 0 ? (object)max : null,
                ["minimum"] = count > 0 ? (object)min : null,
                ["sampleCount"] = count,
                ["unit"] = unit,
            };
        }

        public void WriteJson(string path)
        {
            BattleRenderingBenchmarkEnvironment.WriteJson(path, ToJson());
        }
    }

    public sealed class BattleRenderingBenchmarkSuiteReport
    {
        internal BattleRenderingBenchmarkSuiteReport(
            BattleRenderingBenchmarkConfig config,
            BattleRenderingBenchmarkReport[] runs,
            string workloadFingerprint)
        {
            Config = config;
            Runs = runs ?? Array.Empty<BattleRenderingBenchmarkReport>();
            WorkloadFingerprint = workloadFingerprint ?? string.Empty;
        }

        public BattleRenderingBenchmarkConfig Config { get; }
        public IReadOnlyList<BattleRenderingBenchmarkReport> Runs { get; }
        public string WorkloadFingerprint { get; }
        public BattleRenderingBenchmarkVerdict Verdict
        {
            get
            {
                if (Runs.Count == 0)
                    return BattleRenderingBenchmarkVerdict.Incomplete;
                bool unsupported = false;
                for (int index = 0; index < Runs.Count; index++)
                {
                    if (Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Fail)
                        return BattleRenderingBenchmarkVerdict.Fail;
                    if (Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Incomplete)
                        return BattleRenderingBenchmarkVerdict.Incomplete;
                    unsupported |= Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Unsupported;
                }
                return unsupported
                    ? BattleRenderingBenchmarkVerdict.Unsupported
                    : BattleRenderingBenchmarkVerdict.Pass;
            }
        }
        public bool Passed => Verdict == BattleRenderingBenchmarkVerdict.Pass;
        public string VerdictReason
        {
            get
            {
                if (Runs.Count == 0)
                    return "The suite contains no completed runs.";
                if (Passed)
                    return "All suite runs passed the v5 policy.";
                for (int index = 0; index < Runs.Count; index++)
                {
                    if (Runs[index].Verdict == Verdict)
                        return Runs[index].VerdictReason;
                }
                return "One or more suite runs did not pass.";
            }
        }

        public string ToJson()
        {
            var runProjection = new List<object>(Runs.Count);
            for (int index = 0; index < Runs.Count; index++)
                runProjection.Add(Runs[index].ToProjection(false));
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["comparison"] = Config.Comparison.ToString(),
                ["environment"] = BattleRenderingBenchmarkEnvironment.Capture(),
                ["missingRequiredMetrics"] = BuildMissingRequiredMetrics(),
                ["passed"] = Passed,
                ["policyId"] = BattleRenderingBenchmarkVerdictPolicy.PolicyId,
                ["reason"] = VerdictReason,
                ["runs"] = runProjection,
                ["schema"] = "ntsd-battle-rendering-benchmark-suite-v5",
                ["verdict"] = Verdict.ToString(),
                ["workloadFingerprint"] = WorkloadFingerprint,
            };
            return BattleCanonicalJson.Serialize(root);
        }

        private List<object> BuildMissingRequiredMetrics()
        {
            var result = new List<object>();
            var unique = new HashSet<string>(StringComparer.Ordinal);
            for (int runIndex = 0; runIndex < Runs.Count; runIndex++)
            {
                IReadOnlyList<string> missing = Runs[runIndex].MissingRequiredMetrics;
                for (int metricIndex = 0; metricIndex < missing.Count; metricIndex++)
                {
                    string qualified = Runs[runIndex].Config.Backend + ":" + missing[metricIndex];
                    if (unique.Add(qualified))
                        result.Add(qualified);
                }
            }
            return result;
        }

        public void WriteJson(string path)
        {
            BattleRenderingBenchmarkEnvironment.WriteJson(path, ToJson());
        }
    }

    public sealed class BattleRenderingBenchmarkWorkload
    {
        private BattleRenderingBenchmarkWorkload(
            BattlePresentationFrame frozenFrame,
            int requestedEntityCount,
            int actualEntityCount,
            string fingerprint,
            string source,
            int runtimeObjectCount,
            int runtimeSlotCapacity,
            string runtimeProfile,
            BattleRenderingBenchmarkLogicTickSample[] warmupLogicTickSamples,
            BattleRenderingBenchmarkLogicTickSample[] logicTickSamples,
            string inputFingerprint,
            string initialRuntimeChecksum,
            string finalRuntimeChecksum,
            bool runtimeAdmissionValidated,
            bool runtimeStateDeterministic)
        {
            FrozenFrame = frozenFrame ?? throw new ArgumentNullException(nameof(frozenFrame));
            RequestedEntityCount = requestedEntityCount;
            ActualEntityCount = actualEntityCount;
            Fingerprint = fingerprint ?? string.Empty;
            Source = source ?? string.Empty;
            RuntimeObjectCount = runtimeObjectCount;
            RuntimeSlotCapacity = runtimeSlotCapacity;
            RuntimeProfile = runtimeProfile ?? string.Empty;
            WarmupLogicTickSamples = warmupLogicTickSamples ??
                                     Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
            LogicTickSamples = logicTickSamples ?? Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
            InputFingerprint = inputFingerprint ?? string.Empty;
            InitialRuntimeChecksum = initialRuntimeChecksum ?? string.Empty;
            FinalRuntimeChecksum = finalRuntimeChecksum ?? string.Empty;
            RuntimeAdmissionValidated = runtimeAdmissionValidated;
            RuntimeStateDeterministic = runtimeStateDeterministic;
        }

        public BattlePresentationFrame FrozenFrame { get; }
        public int RequestedEntityCount { get; }
        public int ActualEntityCount { get; }
        public int CommandCount => FrozenFrame.CommandCount;
        public string Fingerprint { get; }
        public string Source { get; }
        public int RuntimeObjectCount { get; }
        public int RuntimeSlotCapacity { get; }
        public string RuntimeProfile { get; }
        public int WarmupTickCount => WarmupLogicTickSamples.Count;
        public int SampleTickCount => LogicTickSamples.Count;
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> WarmupLogicTickSamples { get; }
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> LogicTickSamples { get; }
        public string InputFingerprint { get; }
        public string InitialRuntimeChecksum { get; }
        public string FinalRuntimeChecksum { get; }
        public bool RuntimeAdmissionValidated { get; }
        public bool RuntimeStateDeterministic { get; }
        public bool LogicTickMetricsAvailable
        {
            get
            {
                if (LogicTickSamples.Count <= 0)
                    return false;
                return ValidateLogicSamples(WarmupLogicTickSamples) &&
                       ValidateLogicSamples(LogicTickSamples);
            }
        }

        private static bool ValidateLogicSamples(
            IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> samples)
        {
            for (int index = 0; index < samples.Count; index++)
            {
                if (!samples[index].ElapsedMilliseconds.Available ||
                    !samples[index].AllocatedBytes.Available ||
                    string.IsNullOrEmpty(samples[index].Checksum))
                {
                    return false;
                }
            }
            return true;
        }

        public static BattleRenderingBenchmarkWorkload Create(
            BattleRenderingBenchmarkScenario scenario,
            SimulationWorld world)
        {
            return Create(scenario, world, 0, 1);
        }

        public static BattleRenderingBenchmarkWorkload Create(
            BattleRenderingBenchmarkScenario scenario,
            SimulationWorld world,
            int warmupTickCount,
            int sampleTickCount)
        {
            if (warmupTickCount < 0)
                throw new ArgumentOutOfRangeException(nameof(warmupTickCount));
            if (sampleTickCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleTickCount));

            BattlePresentationFrame frame;
            int requested;
            string source;
            if (scenario.UsesCurrentScene)
            {
                BattlePresentationFrame published = world?.BattlePresentation?.PublishedFrame;
                if (published == null)
                {
                    throw new InvalidOperationException(
                        "The current-scene benchmark requires an active immutable presentation frame.");
                }
                if (published.EntityCount <= 0 || published.CommandCount <= 0)
                {
                    throw new InvalidOperationException(
                        "The current-scene benchmark refuses an empty entity or render-command workload.");
                }
                frame = CloneFrame(published);
                requested = published.EntityCount;
                source = "current-scene-frozen-presentation-frame";
                int runtimeObjectCount = world.ObjectCount;
                string checksum = CaptureRuntimeChecksum(world, published.TickIndex);
                string inputFingerprint = ComputeInputFingerprint(0, 0);
                string fingerprint = ComputeFingerprint(frame, requested, source);
                return new BattleRenderingBenchmarkWorkload(
                    frame,
                    requested,
                    frame.EntityCount,
                    fingerprint,
                    source,
                    runtimeObjectCount,
                    world.RuntimeSlotCapacity,
                    world.RuntimeProfileForServices.ToString(),
                    Array.Empty<BattleRenderingBenchmarkLogicTickSample>(),
                    Array.Empty<BattleRenderingBenchmarkLogicTickSample>(),
                    inputFingerprint,
                    checksum,
                    checksum,
                    runtimeObjectCount > 0,
                    false);
            }

            requested = scenario.RequestedEntityCount;
            return BuildRuntimeWorkload(requested, warmupTickCount, sampleTickCount);
        }

        private static BattleRenderingBenchmarkWorkload BuildRuntimeWorkload(
            int requested,
            int warmupTickCount,
            int sampleTickCount)
        {
            const int runtimeCapacity = BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity;
            var primaryWorld = new SimulationWorld(BattleRuntimeProfile.MobileExtended, runtimeCapacity);
            var mirrorWorld = new SimulationWorld(BattleRuntimeProfile.MobileExtended, runtimeCapacity);
            BattleRenderingBenchmarkEntity[] primaryEntities = RegisterRuntimeEntities(primaryWorld, requested);
            RegisterRuntimeEntities(mirrorWorld, requested);

            string initialPrimary = CaptureRuntimeChecksum(primaryWorld, 0);
            string initialMirror = CaptureRuntimeChecksum(mirrorWorld, 0);
            int totalTicks = checked(warmupTickCount + sampleTickCount);
            string inputFingerprint = ComputeInputFingerprint(warmupTickCount, sampleTickCount);
            var warmupSamples = new BattleRenderingBenchmarkLogicTickSample[warmupTickCount];
            var samples = new BattleRenderingBenchmarkLogicTickSample[sampleTickCount];
            var primaryTickSystem = new NTSDBattleTickSystem(primaryWorld);
            var mirrorTickSystem = new NTSDBattleTickSystem(mirrorWorld);
            int sampleIndex = 0;
            for (int tickIndex = 1; tickIndex <= totalTicks; tickIndex++)
            {
                FrameInputSet primaryInput = FrameInputSet.Empty(tickIndex);
                FrameInputSet mirrorInput = FrameInputSet.Empty(tickIndex);
                primaryWorld.ApplyFrameInputSet(primaryInput);
                mirrorWorld.ApplyFrameInputSet(mirrorInput);

                long allocationStart = GC.GetAllocatedBytesForCurrentThread();
                long started = Stopwatch.GetTimestamp();
                primaryTickSystem.RunReleaseTick(tickIndex);
                double elapsedMilliseconds = BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
                long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
                mirrorTickSystem.RunReleaseTick(tickIndex);

                string checksum = CaptureRuntimeChecksum(primaryWorld, tickIndex);
                var tickSample = new BattleRenderingBenchmarkLogicTickSample(
                    tickIndex,
                    BattleBenchmarkMetric.FromValue(elapsedMilliseconds, "ms"),
                    BattleBenchmarkMetric.FromValue(allocatedBytes, "bytes"),
                    checksum);
                if (tickIndex <= warmupTickCount)
                    warmupSamples[tickIndex - 1] = tickSample;
                else
                    samples[sampleIndex++] = tickSample;
            }

            string finalPrimary = CaptureRuntimeChecksum(primaryWorld, totalTicks);
            string finalMirror = CaptureRuntimeChecksum(mirrorWorld, totalTicks);
            bool admissionValidated = primaryWorld.ObjectCount == requested &&
                                      mirrorWorld.ObjectCount == requested;
            bool deterministic = admissionValidated &&
                                 string.Equals(initialPrimary, initialMirror, StringComparison.Ordinal) &&
                                 string.Equals(finalPrimary, finalMirror, StringComparison.Ordinal);
            if (!admissionValidated)
            {
                throw new InvalidOperationException(
                    $"Runtime benchmark admission mismatch: requested={requested}, " +
                    $"primary={primaryWorld.ObjectCount}, mirror={mirrorWorld.ObjectCount}.");
            }
            if (!deterministic)
            {
                throw new InvalidOperationException(
                    "The deterministic empty-input runtime fixture produced different checksums in its mirror world.");
            }

            BattlePresentationFrame frame = BuildRuntimeDerivedFrame(primaryWorld, primaryEntities, totalTicks);
            string source = "deterministic-mobileextended-runtime-fixture-v1";

            int actual = frame.EntityCount;
            if (actual != requested)
            {
                throw new InvalidOperationException(
                    $"Benchmark workload count mismatch: requested {requested}, built {actual}.");
            }
            if (frame.CommandCount <= 0)
                throw new InvalidOperationException("Benchmark workload contains no render commands.");
            string fingerprint = ComputeFingerprint(frame, requested, source);
            return new BattleRenderingBenchmarkWorkload(
                frame,
                requested,
                actual,
                fingerprint,
                source,
                primaryWorld.ObjectCount,
                primaryWorld.RuntimeSlotCapacity,
                primaryWorld.RuntimeProfileForServices.ToString(),
                warmupSamples,
                samples,
                inputFingerprint,
                initialPrimary,
                finalPrimary,
                admissionValidated,
                deterministic);
        }

        private static BattlePresentationFrame CloneFrame(BattlePresentationFrame source)
        {
            var frame = new BattlePresentationFrame();
            frame.Reset(source.TickIndex);
            frame.EnsureEntityCapacity(source.EntityCount);
            frame.EnsureHitRecordCapacity(source.HitRecordCount);
            frame.EnsureCommandCapacity(source.CommandCount);
            for (int index = 0; index < source.EntityCount; index++)
                frame.AddEntity(source.GetEntity(index));
            for (int index = 0; index < source.HitRecordCount; index++)
                frame.AddHitRecord(source.GetHitRecord(index));
            for (int index = 0; index < source.CommandCount; index++)
                frame.AddCommand(source.GetCommand(index));
            frame.OverlayUnsupportedCount = source.OverlayUnsupportedCount;
            return frame;
        }

        private static BattleRenderingBenchmarkEntity[] RegisterRuntimeEntities(
            SimulationWorld world,
            int entityCount)
        {
            var entities = new BattleRenderingBenchmarkEntity[entityCount];
            for (int index = 0; index < entityCount; index++)
            {
                int column = index % 40;
                int row = index / 40;
                var entity = new BattleRenderingBenchmarkEntity(
                    index + 1,
                    40 + column * 16,
                    200 + row * 4);
                world.Register(entity);
                if (entity.Runtime.SlotIndex < 50)
                {
                    throw new InvalidOperationException(
                        $"Runtime benchmark fixture {index} was not assigned a valid dynamic slot.");
                }
                entities[index] = entity;
            }
            return entities;
        }

        private static BattlePresentationFrame BuildRuntimeDerivedFrame(
            SimulationWorld world,
            BattleRenderingBenchmarkEntity[] entities,
            int tickIndex)
        {
            var frame = new BattlePresentationFrame();
            frame.Reset(tickIndex);
            frame.EnsureEntityCapacity(entities.Length);
            frame.EnsureCommandCapacity(checked(entities.Length * 2));
            for (int index = 0; index < entities.Length; index++)
            {
                BattleRenderingBenchmarkEntity entity = entities[index];
                if (!world.TryGetCurrentRuntimeHandle(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    throw new InvalidOperationException(
                        $"Runtime benchmark fixture lost its generation-aware handle at index {index}.");
                }
                int stableId = entity.Runtime.StableId;
                int runtimeSlot = entity.Runtime.SlotIndex;
                int logicalZ = entity.Runtime.ZInt;
                Vector3 position = NTSDRenderSpace.ScreenPixelToWorld(
                    entity.Runtime.XInt,
                    logicalZ,
                    logicalZ * 0.001f);
                int baseOrder = checked(index * 4);
                frame.AddEntity(new BattlePresentationEntitySnapshot(
                    handle,
                    stableId,
                    entity.ObjectId,
                    entity.GetCurrentDataObjectTypeForSimulation(),
                    0,
                    logicalZ,
                    runtimeSlot,
                    baseOrder,
                    0,
                    true,
                    0,
                    0,
                    0,
                    0,
                    0,
                    entity.Runtime.XInt,
                    logicalZ,
                    position.z,
                    0f,
                    0,
                    0,
                    8f,
                    8f,
                    16f,
                    16f,
                    Vector2.zero,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    (index & 1) != 0,
                    false,
                    default,
                    0,
                    0));
                AddSyntheticCommand(
                    frame,
                    BattleRenderCommandType.Shadow,
                    handle,
                    stableId,
                    runtimeSlot,
                    baseOrder,
                    position + new Vector3(0f, -0.04f, 0f),
                    0,
                    false);
                AddSyntheticCommand(
                    frame,
                    BattleRenderCommandType.Entity,
                    handle,
                    stableId,
                    runtimeSlot,
                    baseOrder,
                    position,
                    1,
                    (index & 1) != 0);
            }
            return frame;
        }

        private static string CaptureRuntimeChecksum(SimulationWorld world, int tickIndex)
        {
            if (world == null)
                return string.Empty;
            FrameInputSet input = FrameInputSet.Empty(tickIndex);
            if (world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended)
            {
                return world.CaptureExtendedChecksumSnapshot(tickIndex, input).OverallChecksum;
            }
            return world.CaptureParityFrameSnapshot(tickIndex, input).OverallChecksum;
        }

        private static string ComputeInputFingerprint(int warmupTickCount, int sampleTickCount)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                Hash(ref hash, warmupTickCount);
                Hash(ref hash, sampleTickCount);
                for (int tickIndex = 1; tickIndex <= warmupTickCount + sampleTickCount; tickIndex++)
                {
                    Hash(ref hash, tickIndex);
                    Hash(ref hash, 0);
                }
                return hash.ToString("x16");
            }
        }

        private static void AddSyntheticCommand(
            BattlePresentationFrame frame,
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int runtimeSlot,
            int baseOrder,
            Vector3 position,
            int localSequence,
            bool flipX)
        {
            frame.AddCommand(new BattleRenderCommand(
                type,
                handle,
                stableId,
                1,
                0,
                runtimeSlot / 40,
                runtimeSlot,
                baseOrder + localSequence,
                0,
                localSequence,
                position,
                new Vector2(16f, 16f),
                new Vector2(0.5f, 0.5f),
                new Rect(0f, 0f, 1f, 1f),
                BattleSpriteRenderState.Default(flipX),
                default));
        }

        private static string ComputeFingerprint(
            BattlePresentationFrame frame,
            int requested,
            string source)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                Hash(ref hash, requested);
                Hash(ref hash, frame.EntityCount);
                Hash(ref hash, frame.CommandCount);
                for (int index = 0; index < source.Length; index++)
                    Hash(ref hash, source[index]);
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    Hash(ref hash, (int)command.Type);
                    Hash(ref hash, command.Handle.Slot);
                    Hash(ref hash, unchecked((int)command.Handle.Generation));
                    Hash(ref hash, command.StableId);
                    Hash(ref hash, command.RuntimeSlot);
                    Hash(ref hash, command.SortOrder);
                    Hash(ref hash, command.LocalSequence);
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.x));
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.y));
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.z));
                    Hash(ref hash, command.FlipX ? 1 : 0);
                }
                return hash.ToString("x16");
            }
        }

        private static void Hash(ref ulong hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 1099511628211UL;
            }
        }
    }

    internal sealed class BattleRenderingBenchmarkEntity : LF2Entity
    {
        public BattleRenderingBenchmarkEntity(int stableId, int x, int z)
        {
            StableId = stableId;
            ObjectId = 10000 + stableId;
            Team = 0;
            Health = new LF2Health();
            Health.BindRuntime(Runtime);
            Health.HP = 500;
            Health.HPBound = 500;
            ItrRest = new LF2ItrRestTracker();
            PS.BindRuntime(Runtime);
            Trans = new FrameTransistor(this);
            Frame.D = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 999,
                wait = 1000000,
                next = 0,
                centerx = 8,
                centery = 8,
            };
            Frame.N = 0;
            Frame.PN = 0;
            Frame.Prev = 0;
            Runtime.X = x;
            Runtime.Y = 0;
            Runtime.Z = z;
            Runtime.SuppressCollisionCandidateUntilTick = int.MaxValue;
            Runtime.SyncIntegerPosition();
            RefreshRuntimeSnapshot();
        }

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

        internal override bool UsesDynamicRuntimeSlot() => true;

        public override int GetCurrentDataObjectTypeForSimulation() => (int)LF2ObjectType.Other;

        public override void RunFrameLogicBeforeAdvance()
        {
        }

        public override void SimTransit(int tickIndex)
        {
        }

        public override void SimTU(int tickIndex)
        {
        }

        public override void SimPostInteraction(int tickIndex)
        {
        }

        public override void SimObjectInteraction(int tickIndex)
        {
        }

        public override void SimPreInteraction(int tickIndex)
        {
        }

        public override void SimEntityCollision(int tickIndex)
        {
        }

        public override void SimFrameTick(int tickIndex)
        {
        }

        public override void SimLateTick(int tickIndex)
        {
        }

        public override void Reset()
        {
        }

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
        }
    }

    public readonly struct BattleBenchmarkCompletedFrameMetrics
    {
        public BattleBenchmarkCompletedFrameMetrics(
            BattleBenchmarkMetric frameTimeMs,
            BattleBenchmarkMetric mainThreadTimeMs,
            BattleBenchmarkMetric renderThreadTimeMs,
            BattleBenchmarkMetric gpuFrameTimeMs,
            BattleBenchmarkMetric managedAllocationBytes,
            BattleBenchmarkMetric drawCalls,
            BattleBenchmarkMetric totalAllocatedMemoryBytes,
            BattleBenchmarkMetric graphicsMemoryBytes)
        {
            FrameTimeMs = frameTimeMs;
            MainThreadTimeMs = mainThreadTimeMs;
            RenderThreadTimeMs = renderThreadTimeMs;
            GpuFrameTimeMs = gpuFrameTimeMs;
            ManagedAllocationBytes = managedAllocationBytes;
            DrawCalls = drawCalls;
            TotalAllocatedMemoryBytes = totalAllocatedMemoryBytes;
            GraphicsMemoryBytes = graphicsMemoryBytes;
        }

        public BattleBenchmarkMetric FrameTimeMs { get; }
        public BattleBenchmarkMetric MainThreadTimeMs { get; }
        public BattleBenchmarkMetric RenderThreadTimeMs { get; }
        public BattleBenchmarkMetric GpuFrameTimeMs { get; }
        public BattleBenchmarkMetric ManagedAllocationBytes { get; }
        public BattleBenchmarkMetric DrawCalls { get; }
        public BattleBenchmarkMetric TotalAllocatedMemoryBytes { get; }
        public BattleBenchmarkMetric GraphicsMemoryBytes { get; }

        internal static BattleBenchmarkCompletedFrameMetrics Unavailable()
        {
            return new BattleBenchmarkCompletedFrameMetrics(
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("bytes"),
                BattleBenchmarkMetric.Unavailable("count"),
                BattleBenchmarkMetric.Unavailable("bytes"),
                BattleBenchmarkMetric.Unavailable("bytes"));
        }
    }

    public interface IBattleBenchmarkCompletedFrameCollector : IDisposable
    {
        bool IsSupported { get; }
        string UnsupportedReason { get; }
        void Request(int generation);
        bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics);
        string Source(BattleBenchmarkRecorderKind kind);
        string Reason(BattleBenchmarkRecorderKind kind);
        void Reset();
    }

    public sealed class BattleBenchmarkInjectedCompletedFrameCollector :
        IBattleBenchmarkCompletedFrameCollector
    {
        private readonly BattleBenchmarkCompletedFrameMetrics metrics;
        private int pendingGeneration;

        public BattleBenchmarkInjectedCompletedFrameCollector(
            BattleBenchmarkCompletedFrameMetrics completedFrameMetrics)
        {
            metrics = completedFrameMetrics;
        }

        public bool IsSupported => true;
        public string UnsupportedReason => string.Empty;

        public void Request(int generation)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
        }

        public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics result)
        {
            if (pendingGeneration != generation)
            {
                result = default;
                return false;
            }
            pendingGeneration = 0;
            result = metrics;
            return true;
        }

        public string Source(BattleBenchmarkRecorderKind kind) => "injected-completed-frame-test-sample";
        public string Reason(BattleBenchmarkRecorderKind kind) => string.Empty;
        public void Reset() => pendingGeneration = 0;
        public void Dispose() => Reset();
    }

    public interface IBattleRenderingBenchmarkRunSession : IDisposable
    {
        bool CaptureFrame();
        BattleRenderingBenchmarkReport Report { get; }
    }

    public interface IBattleBenchmarkLeakProbe
    {
        long CaptureRetainedManagedHeapBytes();
        BattleBenchmarkMetric CaptureGraphicsMemory();
        int CurrentUnityFrame { get; }
        bool RequiresDeferredDestructionWait { get; }
        void BeginPostDisposeCleanup();
        bool IsPostDisposeCleanupComplete { get; }
        void CompletePostDisposeCleanup();
    }

    internal sealed class BattleBenchmarkUnityLeakProbe : IBattleBenchmarkLeakProbe
    {
        private AsyncOperation postDisposeUnload;

        public int CurrentUnityFrame => Time.frameCount;
        public bool RequiresDeferredDestructionWait => Application.isPlaying;
        public bool IsPostDisposeCleanupComplete =>
            postDisposeUnload == null || postDisposeUnload.isDone;

        public long CaptureRetainedManagedHeapBytes()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return GC.GetTotalMemory(false);
        }

        public BattleBenchmarkMetric CaptureGraphicsMemory()
        {
            return BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
        }

        public void BeginPostDisposeCleanup()
        {
            if (!Application.isPlaying)
                return;
            GL.Flush();
            postDisposeUnload = Resources.UnloadUnusedAssets();
        }

        public void CompletePostDisposeCleanup()
        {
            if (Application.isPlaying)
                GL.Flush();
        }
    }

    public sealed class BattleRenderingBenchmarkSession : IBattleRenderingBenchmarkRunSession
    {
        private readonly BattleRenderingBenchmarkConfig config;
        private readonly SimulationWorld world;
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleRenderingBenchmarkPolicyContext policyContext;
        private readonly IBattleBenchmarkCompletedFrameCollector completedFrameCollector;
        private readonly IBattleRenderingBenchmarkPresenter presenter;
        private readonly IBattleBenchmarkLeakProbe leakProbe;
        private readonly List<BattleRenderingBenchmarkFrame> frames;
        private readonly string presenterImplementation;
        private readonly string presenterResourceMode;
        private readonly string presenterDrawMode;
        private readonly string presenterSubmissionDrawMetricSource;
        private readonly string presenterSubmissionDrawUnavailableReason;
        private readonly int presenterRenderTargetWidth;
        private readonly int presenterRenderTargetHeight;
        private readonly int presenterResourceGeneration;
        private readonly int presenterOwnedTextureResourceCount;
        private static int nextGeneration;
        private BattleRenderingBenchmarkReport report;
        private BattleRenderingBenchmarkLeakReport leakReport;
        private bool disposed;
        private int frameIndex;
        private int pendingGeneration;
        private int pendingSampleIndex;
        private bool pendingWarmup;
        private bool pendingLeakFrame;
        private int pendingSampleAttempt;
        private double pendingPresentationMs;
        private int completedFrameRejectedAttemptCount;
        private string completedFrameSamplingFailureReason = string.Empty;
        private int leakFramesCaptured;
        private bool leakBaselineCaptured;
        private bool presenterDisposed;
        private bool teardownPending;
        private int teardownStartedFrame;
        private int teardownFramesWaited;
        private bool teardownCleanupRequested;
        private bool teardownCleanupCompleted;
        private int teardownCleanupCompletedFrame;
        private long leakPrePresenterManaged;
        private long leakPrePresenterGraphics;
        private bool leakPrePresenterGraphicsAvailable;
        private long leakManagedStart;
        private long leakGraphicsStart;
        private bool leakGraphicsAvailable;
        private long leakOwnedStart;
        private int leakResourcesStart;
        private long leakManagedEnd;
        private long leakGraphicsEnd;
        private bool leakSoakGraphicsAvailable;
        private long leakOwnedEnd;
        private int leakResourcesEnd;

        public const string RetainedManagedHeapMeasurementMode =
            "full-gc-retained-managed-heap-outside-performance-sample-window-v1";
        public const int DeferredDestructionPlayFrames = 2;
        public const int PostDisposeCleanupPlayFrames = 2;
        public const int MaxPostDisposeCleanupPlayFrames = 120;
        public const int MaxCompletedFrameSampleAttempts = 16;

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world)
            : this(
                config,
                world,
                BattleRenderingBenchmarkWorkload.Create(
                    config.Scenario,
                    world,
                    config.WarmupFrames,
                    config.SampleFrames))
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload)
            : this(
                config,
                world,
                workload,
                BattleRenderingBenchmarkPolicyContext.Capture(),
                null,
                null)
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload,
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext,
            IBattleBenchmarkCompletedFrameCollector collector,
            IBattleRenderingBenchmarkPresenter benchmarkPresenter)
            : this(
                config,
                world,
                workload,
                benchmarkPolicyContext,
                collector,
                benchmarkPresenter,
                null)
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload,
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext,
            IBattleBenchmarkCompletedFrameCollector collector,
            IBattleRenderingBenchmarkPresenter benchmarkPresenter,
            IBattleBenchmarkLeakProbe benchmarkLeakProbe)
        {
            if (config.Comparison != BattleRenderingBenchmarkComparison.Single)
                throw new ArgumentException("A single run session requires Single comparison mode.", nameof(config));
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            policyContext = benchmarkPolicyContext;
            leakProbe = benchmarkLeakProbe ?? new BattleBenchmarkUnityLeakProbe();
            ValidateCount();
            frames = new List<BattleRenderingBenchmarkFrame>(config.SampleFrames);
            completedFrameCollector = collector ??
                new BattleBenchmarkUnityCompletedFrameCollector(policyContext);
            IBattleRenderingBenchmarkPresenter presenterCandidate = benchmarkPresenter;
            try
            {
                if (config.LeakCheckFrames > 0)
                    CapturePrePresenterLeakBaseline();
                presenterCandidate = presenterCandidate ??
                                     BattleRenderingBenchmarkPresenterFactory.Create(config.Backend, workload);
                presenter = presenterCandidate;
                ValidatePresenterWorkload();
                presenterImplementation = presenter.Implementation;
                presenterResourceMode = presenter.ResourceMode;
                presenterDrawMode = presenter.DrawMode;
                presenterSubmissionDrawMetricSource = presenter.SubmissionDrawMetricSource;
                presenterSubmissionDrawUnavailableReason = presenter.SubmissionDrawUnavailableReason;
                presenterRenderTargetWidth = presenter.RenderTargetWidth;
                presenterRenderTargetHeight = presenter.RenderTargetHeight;
                presenterResourceGeneration = presenter.ResourceGeneration;
                presenterOwnedTextureResourceCount = presenter.OwnedTextureResourceCount;
            }
            catch
            {
                try
                {
                    presenterCandidate?.Dispose();
                }
                catch (Exception cleanupException)
                {
                    UnityEngine.Debug.LogException(cleanupException);
                }
                finally
                {
                    completedFrameCollector.Dispose();
                }
                throw;
            }
        }

        public BattleRenderingBenchmarkConfig Config => config;
        public bool IsComplete => report != null;
        public BattleRenderingBenchmarkReport Report => report;
        public bool IsDisposed => disposed;
        public int WarmupFramesCaptured => Math.Min(frameIndex, config.WarmupFrames);
        public int SampleFramesCaptured => frames.Count;
        public int LeakFramesCaptured => leakFramesCaptured;
        public BattleRenderingBenchmarkWorkload Workload => workload;

        public bool CaptureFrame()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleRenderingBenchmarkSession));
            if (report != null)
                return true;

            if (pendingGeneration != 0)
            {
                if (!completedFrameCollector.TryDrain(
                        pendingGeneration,
                        out BattleBenchmarkCompletedFrameMetrics completedMetrics))
                {
                    return false;
                }

                bool completedLeakFrame = pendingLeakFrame;
                if (completedLeakFrame)
                    leakFramesCaptured++;
                else if (!pendingWarmup)
                {
                    BattleRenderingBenchmarkFrame sample =
                        CaptureSample(pendingSampleIndex, pendingPresentationMs, completedMetrics);
                    if (!HasAllApplicableFormalSampleMetrics(sample, out string rejectionReason))
                    {
                        int rejectedGeneration = pendingGeneration;
                        completedFrameRejectedAttemptCount++;
                        pendingGeneration = 0;
                        pendingWarmup = false;
                        pendingLeakFrame = false;
                        if (pendingSampleAttempt < MaxCompletedFrameSampleAttempts)
                        {
                            pendingSampleAttempt++;
                            BeginCompletedFrameRequest();
                            return false;
                        }

                        completedFrameSamplingFailureReason =
                            $"Formal sample {pendingSampleIndex} exhausted {MaxCompletedFrameSampleAttempts} " +
                            $"completed-frame attempts; last generation {rejectedGeneration}: {rejectionReason}";
                        leakReport = BattleRenderingBenchmarkLeakReport.NotRun(
                            "Leak/long-run soak was not run because " + completedFrameSamplingFailureReason);
                        FinalizeReport();
                        return true;
                    }
                    frames.Add(sample);
                    pendingSampleAttempt = 0;
                }
                pendingGeneration = 0;
                pendingWarmup = false;
                pendingLeakFrame = false;

                if (completedLeakFrame)
                {
                    if (leakFramesCaptured < config.LeakCheckFrames)
                        return false;
                    BeginLeakTeardown();
                    return false;
                }

                if (frames.Count < config.SampleFrames)
                    return false;
                if (config.LeakCheckFrames <= 0)
                {
                    leakReport = BattleRenderingBenchmarkLeakReport.NotRequested();
                    FinalizeReport();
                    return true;
                }
                CaptureLeakBaseline();
                return false;
            }

            if (teardownPending)
            {
                teardownFramesWaited = Math.Max(0, leakProbe.CurrentUnityFrame - teardownStartedFrame);
                if (leakProbe.RequiresDeferredDestructionWait &&
                    teardownFramesWaited < DeferredDestructionPlayFrames)
                {
                    return false;
                }

                if (!teardownCleanupRequested)
                {
                    teardownCleanupRequested = true;
                    leakProbe.BeginPostDisposeCleanup();
                    return false;
                }

                if (!leakProbe.IsPostDisposeCleanupComplete)
                {
                    if (leakProbe.RequiresDeferredDestructionWait &&
                        teardownFramesWaited >= MaxPostDisposeCleanupPlayFrames)
                    {
                        FinalizeLeakReport(
                            "Post-Dispose Unity cleanup did not complete within " +
                            MaxPostDisposeCleanupPlayFrames + " Play frames.");
                        FinalizeReport();
                        return true;
                    }
                    return false;
                }

                if (!teardownCleanupCompleted)
                {
                    teardownCleanupCompleted = true;
                    teardownCleanupCompletedFrame = leakProbe.CurrentUnityFrame;
                    leakProbe.CompletePostDisposeCleanup();
                    return false;
                }

                if (leakProbe.RequiresDeferredDestructionWait &&
                    leakProbe.CurrentUnityFrame - teardownCleanupCompletedFrame <
                    PostDisposeCleanupPlayFrames)
                {
                    return false;
                }

                FinalizeLeakReport();
                FinalizeReport();
                return true;
            }

            if (frames.Count < config.SampleFrames)
            {
                ValidateCount();
                int currentFrame = frameIndex;
                pendingWarmup = currentFrame < config.WarmupFrames;
                pendingSampleIndex = frames.Count;
                pendingLeakFrame = false;
                pendingSampleAttempt = 1;
                BeginCompletedFrameRequest();
                frameIndex++;
                return false;
            }

            if (!leakBaselineCaptured)
                CaptureLeakBaseline();
            ValidateCount();
            pendingWarmup = false;
            pendingLeakFrame = true;
            pendingSampleAttempt = 0;
            BeginCompletedFrameRequest();
            return false;
        }

        private void BeginCompletedFrameRequest()
        {
            pendingGeneration = Interlocked.Increment(ref nextGeneration);
            if (pendingGeneration == 0)
                pendingGeneration = Interlocked.Increment(ref nextGeneration);
            try
            {
                completedFrameCollector.Request(pendingGeneration);
                pendingPresentationMs = presenter.Present();
                ValidatePresenterWorkload();
            }
            catch
            {
                completedFrameCollector.Reset();
                pendingGeneration = 0;
                pendingWarmup = false;
                pendingLeakFrame = false;
                pendingSampleAttempt = 0;
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            try
            {
                DisposePresenter();
            }
            finally
            {
                completedFrameCollector.Dispose();
            }
        }

        private BattleRenderingBenchmarkFrame CaptureSample(
            int index,
            double presentationMs,
            BattleBenchmarkCompletedFrameMetrics completedMetrics)
        {
            BattleCentralBuildDiagnostics diagnostics = presenter.Diagnostics;
            BattleBenchmarkMetric logicTickTime;
            BattleBenchmarkMetric logicTickAllocatedBytes;
            if (index < workload.LogicTickSamples.Count)
            {
                BattleRenderingBenchmarkLogicTickSample logicSample = workload.LogicTickSamples[index];
                logicTickTime = logicSample.ElapsedMilliseconds;
                logicTickAllocatedBytes = logicSample.AllocatedBytes;
            }
            else
            {
                logicTickTime = BattleBenchmarkMetric.Unavailable("ms");
                logicTickAllocatedBytes = BattleBenchmarkMetric.Unavailable("bytes");
            }
            var frame = new BattleRenderingBenchmarkFrame(
                index,
                workload.ActualEntityCount,
                workload.CommandCount)
            {
                FrameTimeMs = completedMetrics.FrameTimeMs,
                MainThreadTimeMs = completedMetrics.MainThreadTimeMs,
                RenderThreadTimeMs = completedMetrics.RenderThreadTimeMs,
                GpuFrameTimeMs = completedMetrics.GpuFrameTimeMs,
                LogicTickTimeMs = logicTickTime,
                LogicTickAllocatedBytes = logicTickAllocatedBytes,
                LogicTickChecksum = index < workload.LogicTickSamples.Count
                    ? workload.LogicTickSamples[index].Checksum
                    : string.Empty,
                PresentationBuildTimeMs = BattleBenchmarkMetric.FromValue(presentationMs, "ms"),
                ManagedAllocationBytes = completedMetrics.ManagedAllocationBytes,
                DrawCalls = BattleBenchmarkDrawCallPolicy.RequirePositiveForNonEmptyWorkload(
                    completedMetrics.DrawCalls),
                PresenterSubmittedRenderItems = BattleBenchmarkMetric.FromValue(
                    presenter.MaterializedRenderItemCount,
                    "count"),
                PresenterSubmissionDrawCalls = presenter.SubmissionDrawCount >= 0
                    ? BattleBenchmarkMetric.FromValue(presenter.SubmissionDrawCount, "count")
                    : BattleBenchmarkMetric.Unavailable("count"),
                TotalAllocatedMemoryBytes = completedMetrics.TotalAllocatedMemoryBytes,
                GraphicsMemoryBytes = completedMetrics.GraphicsMemoryBytes,
                BenchmarkOwnedTextureMemoryBytes = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                    presenterResourceGeneration,
                    presenterOwnedTextureResourceCount,
                    presenter.MeasureOwnedTextureMemoryBytes(),
                    out _),
                BenchmarkOwnedMemoryBytes = BattleBenchmarkMetric.FromValue(
                    presenter.CachedOwnedResourceMemoryBytes,
                    "bytes"),
                BenchmarkResourceGeneration = presenterResourceGeneration,
                SourceCommands = BattleBenchmarkMetric.FromValue(workload.CommandCount, "count"),
                ResolvedCommands = BattleBenchmarkMetric.FromValue(presenter.ResolvedCommandCount, "count"),
                UnresolvedCommands = BattleBenchmarkMetric.FromValue(
                    workload.CommandCount - presenter.ResolvedCommandCount,
                    "count"),
                ResourceSegments = BattleBenchmarkMetric.FromValue(presenter.ResourceSegmentCount, "count"),
                MeshChunks = diagnostics == null
                    ? BattleBenchmarkMetric.Unavailable("count")
                    : BattleBenchmarkMetric.FromValue(diagnostics.ActiveChunkCount, "count"),
                RequestedBackend = config.Backend.ToString(),
                EffectiveBackend = presenter.EffectiveBackend,
            };
            return frame;
        }

        private bool HasAllApplicableFormalSampleMetrics(
            BattleRenderingBenchmarkFrame frame,
            out string reason)
        {
            if (!completedFrameCollector.IsSupported)
            {
                reason = string.Empty;
                return true;
            }
            var missing = new List<string>();
            AddMissingFormalSampleMetric(missing, "frameTimeMs", frame.FrameTimeMs);
            AddMissingFormalSampleMetric(missing, "mainThreadTimeMs", frame.MainThreadTimeMs);
            if (policyContext.GraphicsMultiThreaded)
                AddMissingFormalSampleMetric(missing, "renderThreadTimeMs", frame.RenderThreadTimeMs);
            AddMissingFormalSampleMetric(missing, "gpuFrameTimeMs", frame.GpuFrameTimeMs);
            AddMissingFormalSampleMetric(missing, "managedAllocationBytes", frame.ManagedAllocationBytes);
            AddMissingFormalSampleMetric(missing, "drawCalls", frame.DrawCalls);
            AddMissingFormalSampleMetric(missing, "totalAllocatedMemoryBytes", frame.TotalAllocatedMemoryBytes);
            AddMissingFormalSampleMetric(missing, "graphicsMemoryBytes", frame.GraphicsMemoryBytes);
            AddMissingFormalSampleMetric(
                missing,
                "benchmarkOwnedTextureMemoryBytes",
                frame.BenchmarkOwnedTextureMemoryBytes);
            AddMissingFormalSampleMetric(missing, "presentationBuildTimeMs", frame.PresentationBuildTimeMs);
            AddMissingFormalSampleMetric(
                missing,
                "presenterSubmittedRenderItems",
                frame.PresenterSubmittedRenderItems);
            AddMissingFormalSampleMetric(missing, "resourceSegments", frame.ResourceSegments);
            AddMissingFormalSampleMetric(missing, "benchmarkOwnedMemoryBytes", frame.BenchmarkOwnedMemoryBytes);
            if (config.Backend == BattlePresentationBackendMode.CentralOnly)
            {
                AddMissingFormalSampleMetric(
                    missing,
                    "presenterSubmissionDrawCalls",
                    frame.PresenterSubmissionDrawCalls);
                AddMissingFormalSampleMetric(missing, "meshChunks", frame.MeshChunks);
            }

            if (missing.Count == 0)
            {
                reason = string.Empty;
                return true;
            }

            reason = "required applicable metrics unavailable: " + string.Join(", ", missing) + ".";
            return false;
        }

        private static void AddMissingFormalSampleMetric(
            List<string> missing,
            string name,
            BattleBenchmarkMetric metric)
        {
            if (!metric.Available)
                missing.Add(name);
        }

        private void CaptureLeakBaseline()
        {
            leakBaselineCaptured = true;
            leakManagedStart = leakProbe.CaptureRetainedManagedHeapBytes();
            leakOwnedStart = presenter.MeasureOwnedResourceMemoryBytes();
            leakResourcesStart = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakGraphicsAvailable = graphics.Available;
            leakGraphicsStart = graphics.Available ? (long)graphics.Value : 0L;
        }

        private void CapturePrePresenterLeakBaseline()
        {
            leakPrePresenterManaged = leakProbe.CaptureRetainedManagedHeapBytes();
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakPrePresenterGraphicsAvailable = graphics.Available;
            leakPrePresenterGraphics = graphics.Available ? (long)graphics.Value : 0L;
        }

        private void BeginLeakTeardown()
        {
            leakManagedEnd = leakProbe.CaptureRetainedManagedHeapBytes();
            leakOwnedEnd = presenter.MeasureOwnedResourceMemoryBytes();
            leakResourcesEnd = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakSoakGraphicsAvailable = leakGraphicsAvailable && graphics.Available;
            leakGraphicsEnd = leakSoakGraphicsAvailable ? (long)graphics.Value : 0L;
            DisposePresenter();
            teardownPending = true;
            teardownStartedFrame = leakProbe.CurrentUnityFrame;
            teardownCleanupRequested = false;
            teardownCleanupCompleted = false;
            teardownCleanupCompletedFrame = 0;
        }

        private void FinalizeLeakReport(string teardownCleanupFailureReason = null)
        {
            long teardownManagedEnd = leakProbe.CaptureRetainedManagedHeapBytes();
            BattleBenchmarkMetric teardownGraphics = leakProbe.CaptureGraphicsMemory();
            bool teardownGraphicsAvailable = leakPrePresenterGraphicsAvailable &&
                                             teardownGraphics.Available;
            long teardownGraphicsEnd = teardownGraphicsAvailable
                ? (long)teardownGraphics.Value
                : 0L;
            long teardownOwnedEnd = presenter.MeasureOwnedResourceMemoryBytes();
            int teardownResourcesEnd = presenter.OwnedResourceCount;

            long managedGrowth = leakManagedEnd - leakManagedStart;
            long graphicsGrowth = leakGraphicsEnd - leakGraphicsStart;
            bool soakPassed = leakSoakGraphicsAvailable &&
                              managedGrowth <= config.MaxManagedGrowthBytes &&
                              graphicsGrowth <= config.MaxGraphicsGrowthBytes &&
                              leakOwnedEnd <= leakOwnedStart &&
                              leakResourcesEnd <= leakResourcesStart;
            // The pre-presenter values expose one-time pipeline initialization, while the
            // post-Dispose gate detects retained growth relative to the steady-state soak baseline.
            long teardownManagedGrowth = teardownManagedEnd - leakManagedStart;
            long teardownGraphicsGrowth = teardownGraphicsEnd - leakGraphicsStart;
            bool teardownPassed = string.IsNullOrEmpty(teardownCleanupFailureReason) &&
                                  teardownGraphicsAvailable &&
                                  teardownManagedGrowth <= config.MaxManagedGrowthBytes &&
                                  teardownGraphicsGrowth <= config.MaxGraphicsGrowthBytes &&
                                  teardownOwnedEnd == 0L &&
                                  teardownResourcesEnd == 0;
            BattleBenchmarkMetricStatus teardownStatus = !teardownGraphicsAvailable
                ? BattleBenchmarkMetricStatus.Missing
                : teardownPassed
                    ? BattleBenchmarkMetricStatus.Passed
                    : BattleBenchmarkMetricStatus.Failed;
            string teardownReason = !string.IsNullOrEmpty(teardownCleanupFailureReason)
                ? teardownCleanupFailureReason
                : !teardownGraphicsAvailable
                ? "Post-Dispose graphics memory evidence is required but unavailable."
                : teardownPassed
                    ? "Post-Dispose ownership returned to zero and retained managed/graphics memory returned within steady-state thresholds after bounded Unity cleanup."
                    : "Post-Dispose ownership remained nonzero or retained managed/graphics memory exceeded a steady-state threshold after bounded Unity cleanup.";
            bool passed = soakPassed && teardownPassed;
            string reason = !leakSoakGraphicsAvailable
                ? "Steady-state graphics memory evidence is required but unavailable."
                : passed
                    ? "Steady-state soak and post-Dispose teardown both passed."
                    : "Steady-state soak or post-Dispose teardown failed: " + teardownReason;
            leakReport = new BattleRenderingBenchmarkLeakReport(
                true,
                passed,
                leakFramesCaptured,
                leakPrePresenterManaged,
                leakPrePresenterGraphics,
                leakPrePresenterGraphicsAvailable,
                leakManagedStart,
                leakManagedEnd,
                leakGraphicsStart,
                leakGraphicsEnd,
                leakSoakGraphicsAvailable,
                leakOwnedStart,
                leakOwnedEnd,
                leakResourcesStart,
                leakResourcesEnd,
                config.MaxManagedGrowthBytes,
                config.MaxGraphicsGrowthBytes,
                teardownFramesWaited,
                teardownManagedEnd,
                teardownGraphicsEnd,
                teardownGraphicsAvailable,
                teardownOwnedEnd,
                teardownResourcesEnd,
                teardownStatus,
                teardownReason,
                RetainedManagedHeapMeasurementMode,
                reason);
            teardownPending = false;
        }

        private void DisposePresenter()
        {
            if (presenterDisposed)
                return;
            presenterDisposed = true;
            presenter.Dispose();
        }

        private void FinalizeReport()
        {
            bool logicTickMetricsValidated = ValidateLogicTickMetrics();
            BattleBenchmarkMetricAvailability[] metricAvailability = BuildMetricAvailability();
            report = new BattleRenderingBenchmarkReport(
                config,
                frames.ToArray(),
                metricAvailability,
                policyContext,
                workload.RequestedEntityCount,
                workload.ActualEntityCount,
                workload.CommandCount,
                workload.Fingerprint,
                presenterImplementation,
                presenterResourceMode,
                presenterDrawMode,
                presenterRenderTargetWidth,
                presenterRenderTargetHeight,
                true,
                workload.RuntimeAdmissionValidated,
                logicTickMetricsValidated,
                workload.RuntimeStateDeterministic,
                true,
                leakReport);
            report.RuntimeObjectCount = workload.RuntimeObjectCount;
            report.RuntimeSlotCapacity = workload.RuntimeSlotCapacity;
            report.RuntimeProfile = workload.RuntimeProfile;
            report.WarmupLogicTickCount = workload.WarmupTickCount;
            report.SampleLogicTickCount = workload.SampleTickCount;
            report.InputFingerprint = workload.InputFingerprint;
            report.InitialRuntimeChecksum = workload.InitialRuntimeChecksum;
            report.FinalRuntimeChecksum = workload.FinalRuntimeChecksum;
            report.WarmupLogicTickSamples = workload.WarmupLogicTickSamples;
            report.SampleLogicTickSamples = workload.LogicTickSamples;
            report.CompletedFrameRejectedAttemptCount = completedFrameRejectedAttemptCount;
            report.MaxCompletedFrameSampleAttempts = MaxCompletedFrameSampleAttempts;
            report.CompletedFrameSamplingFailureReason = completedFrameSamplingFailureReason;
        }

        private BattleBenchmarkMetricAvailability[] BuildMetricAvailability()
        {
            var result = new List<BattleBenchmarkMetricAvailability>(24);
            AddFrameMetric(result, "frameTimeMs", BattleBenchmarkRecorderKind.FrameTime, frame => frame.FrameTimeMs);
            AddFrameMetric(result, "mainThreadTimeMs", BattleBenchmarkRecorderKind.MainThread, frame => frame.MainThreadTimeMs);
            AddFrameMetric(
                result,
                "renderThreadTimeMs",
                BattleBenchmarkRecorderKind.RenderThread,
                frame => frame.RenderThreadTimeMs,
                policyContext.GraphicsMultiThreaded);
            AddFrameMetric(result, "gpuFrameTimeMs", BattleBenchmarkRecorderKind.GpuFrame, frame => frame.GpuFrameTimeMs);
            AddFrameMetric(result, "managedAllocationBytes", BattleBenchmarkRecorderKind.ManagedAllocation, frame => frame.ManagedAllocationBytes);
            AddFrameMetric(
                result,
                "drawCalls",
                BattleBenchmarkRecorderKind.DrawCalls,
                frame => frame.DrawCalls,
                unavailableReason: "A positive completed-frame draw-call count is required for this non-empty benchmark render workload.");
            AddFrameMetric(result, "totalAllocatedMemoryBytes", BattleBenchmarkRecorderKind.TotalMemory, frame => frame.TotalAllocatedMemoryBytes);
            AddFrameMetric(result, "graphicsMemoryBytes", BattleBenchmarkRecorderKind.GraphicsMemory, frame => frame.GraphicsMemoryBytes);
            AddLocalMetric(
                result,
                "benchmarkOwnedTextureMemoryBytes",
                "benchmark-owned-textures",
                frame => frame.BenchmarkOwnedTextureMemoryBytes,
                BenchmarkOwnedTextureMemorySource(),
                unavailableReason: BenchmarkOwnedTextureMemoryUnavailableReason());
            AddLocalMetric(result, "logicTickTimeMs", "logic-tick", frame => frame.LogicTickTimeMs,
                "Stopwatch around full NTSDBattleTickSystem.RunReleaseTick");
            AddLocalMetric(result, "logicTickAllocatedBytes", "logic-tick", frame => frame.LogicTickAllocatedBytes,
                "GC.GetAllocatedBytesForCurrentThread around full NTSDBattleTickSystem.RunReleaseTick");
            AddLocalMetric(result, "presentationBuildTimeMs", "presenter-local", frame => frame.PresentationBuildTimeMs,
                "Stopwatch around benchmark presenter update/build");
            AddLocalMetric(result, "presenterSubmittedRenderItems", "presenter-local", frame => frame.PresenterSubmittedRenderItems,
                "Validated frozen render-command/materializer count");
            AddLocalMetric(result, "resourceSegments", "presenter-local", frame => frame.ResourceSegments,
                "Presenter resource compatibility grouping");
            AddLocalMetric(result, "benchmarkOwnedMemoryBytes", "presenter-local", frame => frame.BenchmarkOwnedMemoryBytes,
                "Profiler.GetRuntimeMemorySizeLong over benchmark-owned resources");

            bool central = config.Backend == BattlePresentationBackendMode.CentralOnly;
            AddLocalMetric(
                result,
                "presenterSubmissionDrawCalls",
                "presenter-local",
                frame => frame.PresenterSubmissionDrawCalls,
                presenterSubmissionDrawMetricSource,
                central,
                presenterSubmissionDrawUnavailableReason);
            AddLocalMetric(
                result,
                "meshChunks",
                "presenter-local",
                frame => frame.MeshChunks,
                central ? "BattleDynamicMeshBackend diagnostics" : "not applicable",
                central,
                "Legacy compatibility presentation does not build central mesh chunks.");

            bool? exactSampleCount = string.IsNullOrEmpty(completedFrameSamplingFailureReason)
                ? frames.Count == config.SampleFrames
                : (bool?)null;
            AddGate(result, "exactSampleCount", exactSampleCount,
                frames.Count,
                config.SampleFrames,
                "completed-frame collector");
            AddGate(result, "countValidated", workload.ActualEntityCount == workload.RequestedEntityCount,
                1, 1, "frozen workload entity counts");
            AddGate(result, "runtimeAdmissionValidated", workload.RuntimeAdmissionValidated,
                1, 1, "SimulationWorld runtime admission");
            AddGate(result, "determinismValidated",
                config.Scenario.UsesCurrentScene ? (bool?)null : workload.RuntimeStateDeterministic,
                1, 1, "runtime checksum replay");
            AddGate(result, "rendererWorkloadValidated", true,
                1, 1, "presenter materialization validation");
            if (config.LeakCheckFrames > 0)
            {
                BattleBenchmarkMetricStatus leakStatus = !leakReport.GraphicsAvailable ||
                                                         leakReport.TeardownStatus == BattleBenchmarkMetricStatus.Missing
                    ? BattleBenchmarkMetricStatus.Missing
                    : leakReport.Passed
                        ? BattleBenchmarkMetricStatus.Passed
                        : BattleBenchmarkMetricStatus.Failed;
                result.Add(new BattleBenchmarkMetricAvailability(
                    "leakCheck",
                    true,
                    BattleBenchmarkMetricApplicability.Applicable,
                    leakStatus,
                    "long-run",
                    leakReport.Available && leakReport.GraphicsAvailable &&
                    leakReport.TeardownStatus != BattleBenchmarkMetricStatus.Missing ? 1 : 0,
                    1,
                    RetainedManagedHeapMeasurementMode,
                    leakReport.Reason));
            }
            else
            {
                result.Add(new BattleBenchmarkMetricAvailability(
                    "leakCheck",
                    false,
                    BattleBenchmarkMetricApplicability.NotApplicable,
                    BattleBenchmarkMetricStatus.NotApplicable,
                    "long-run",
                    0,
                    0,
                    "not requested",
                    "Leak/long-run soak and teardown were not requested."));
            }
            return result.ToArray();
        }

        private string BenchmarkOwnedTextureMemorySource()
        {
            return "Profiler.GetRuntimeMemorySizeLong summed over " +
                   presenterOwnedTextureResourceCount +
                   " Texture2D/RenderTexture objects owned by benchmark resource generation " +
                   presenterResourceGeneration + ".";
        }

        private string BenchmarkOwnedTextureMemoryUnavailableReason()
        {
            return "No positive runtime-memory sample was observed for the " +
                   presenterOwnedTextureResourceCount +
                   " Texture2D/RenderTexture objects owned by benchmark resource generation " +
                   presenterResourceGeneration + ".";
        }

        private void AddFrameMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            BattleBenchmarkRecorderKind kind,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            bool applicable = true,
            string unavailableReason = "")
        {
            AddMetric(
                result,
                name,
                "completed-frame",
                selector,
                completedFrameCollector.Source(kind),
                required: applicable,
                applicable,
                string.IsNullOrWhiteSpace(completedFrameCollector.Reason(kind))
                    ? unavailableReason
                    : completedFrameCollector.Reason(kind));
        }

        private void AddLocalMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            string scope,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            string source,
            bool applicable = true,
            string unavailableReason = "")
        {
            AddMetric(result, name, scope, selector, source, applicable, applicable, unavailableReason);
        }

        private void AddMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            string scope,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            string source,
            bool required,
            bool applicable,
            string unavailableReason)
        {
            if (!applicable)
            {
                result.Add(new BattleBenchmarkMetricAvailability(
                    name,
                    false,
                    BattleBenchmarkMetricApplicability.NotApplicable,
                    BattleBenchmarkMetricStatus.NotApplicable,
                    scope,
                    0,
                    0,
                    source,
                    unavailableReason));
                return;
            }

            int sampleCount = 0;
            for (int index = 0; index < frames.Count; index++)
            {
                if (selector(frames[index]).Available)
                    sampleCount++;
            }
            BattleBenchmarkMetricStatus status = sampleCount == config.SampleFrames
                ? BattleBenchmarkMetricStatus.Available
                : completedFrameCollector.IsSupported
                    ? BattleBenchmarkMetricStatus.Missing
                    : BattleBenchmarkMetricStatus.Unsupported;
            string reason = status == BattleBenchmarkMetricStatus.Available
                ? string.Empty
                : !string.IsNullOrEmpty(completedFrameSamplingFailureReason)
                    ? completedFrameSamplingFailureReason
                    : string.IsNullOrWhiteSpace(unavailableReason)
                        ? string.IsNullOrWhiteSpace(completedFrameCollector.UnsupportedReason)
                            ? $"Captured {sampleCount} of {config.SampleFrames} required samples."
                            : completedFrameCollector.UnsupportedReason
                        : unavailableReason;
            result.Add(new BattleBenchmarkMetricAvailability(
                name,
                required,
                BattleBenchmarkMetricApplicability.Applicable,
                status,
                scope,
                sampleCount,
                config.SampleFrames,
                source,
                reason));
        }

        private static void AddGate(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            bool? passed,
            int sampleCount,
            int expectedSampleCount,
            string source)
        {
            BattleBenchmarkMetricStatus status =
                BattleRenderingBenchmarkEvidencePolicy.ValidationStatus(passed);
            result.Add(new BattleBenchmarkMetricAvailability(
                name,
                true,
                BattleBenchmarkMetricApplicability.Applicable,
                status,
                "validation-gate",
                passed.HasValue ? sampleCount : 0,
                expectedSampleCount,
                source,
                !passed.HasValue
                    ? "The current-scene workload did not measure this validation gate."
                    : passed.Value
                        ? string.Empty
                        : "The required validation gate failed."));
        }

        private bool ValidateLogicTickMetrics()
        {
            if (frames.Count <= 0)
                return false;
            for (int index = 0; index < frames.Count; index++)
            {
                if (!frames[index].LogicTickTimeMs.Available)
                    return false;
                if (!frames[index].LogicTickAllocatedBytes.Available)
                    return false;
            }
            return true;
        }

        private void ValidateCount()
        {
            if (workload.ActualEntityCount != workload.RequestedEntityCount ||
                workload.FrozenFrame.EntityCount != workload.ActualEntityCount)
            {
                throw new InvalidOperationException(
                    $"Benchmark presentation entity count changed or mismatched: requested={workload.RequestedEntityCount}, " +
                    $"actual={workload.ActualEntityCount}, frame={workload.FrozenFrame.EntityCount}.");
            }
        }

        private void ValidatePresenterWorkload()
        {
            if (presenter.ResolvedCommandCount != workload.CommandCount ||
                presenter.MaterializedRenderItemCount != workload.CommandCount)
            {
                throw new InvalidOperationException(
                    $"{presenter.Implementation} did not materialize the complete workload: " +
                    $"commands={workload.CommandCount}, resolved={presenter.ResolvedCommandCount}, " +
                    $"materializedItems={presenter.MaterializedRenderItemCount}.");
            }
        }
    }

    public sealed class BattleRenderingBenchmarkSuiteSession : IDisposable
    {
        private readonly BattleRenderingBenchmarkConfig config;
        private readonly SimulationWorld world;
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly List<BattleRenderingBenchmarkReport> runs =
            new List<BattleRenderingBenchmarkReport>(2);
        private readonly BattlePresentationBackendMode previousBackend;
        private readonly Func<BattleRenderingBenchmarkConfig, SimulationWorld,
            BattleRenderingBenchmarkWorkload, IBattleRenderingBenchmarkRunSession> sessionFactory;
        private IBattleRenderingBenchmarkRunSession activeSession;
        private BattleRenderingBenchmarkSuiteReport report;
        private int nextBackendIndex;
        private bool disposed;
        private bool backendRestored;

        public BattleRenderingBenchmarkSuiteSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world)
            : this(config, world, null)
        {
        }

        public BattleRenderingBenchmarkSuiteSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            Func<BattleRenderingBenchmarkConfig, SimulationWorld,
                BattleRenderingBenchmarkWorkload, IBattleRenderingBenchmarkRunSession> benchmarkSessionFactory)
        {
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            previousBackend = this.world.BattlePresentation.Mode;
            sessionFactory = benchmarkSessionFactory ??
                ((runConfig, runWorld, runWorkload) =>
                    new BattleRenderingBenchmarkSession(runConfig, runWorld, runWorkload));
            workload = BattleRenderingBenchmarkWorkload.Create(
                config.Scenario,
                this.world,
                config.WarmupFrames,
                config.SampleFrames);
            try
            {
                StartNextRun();
            }
            catch
            {
                RestoreBackend();
                throw;
            }
        }

        public bool IsComplete => report != null;
        public BattleRenderingBenchmarkSuiteReport Report => report;
        public BattleRenderingBenchmarkWorkload Workload => workload;

        public bool CaptureFrame()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleRenderingBenchmarkSuiteSession));
            if (report != null)
                return true;
            try
            {
                if (!activeSession.CaptureFrame())
                    return false;

                BattleRenderingBenchmarkReport completed = activeSession.Report;
                runs.Add(completed);
                activeSession.Dispose();
                activeSession = null;
                if (StartNextRun())
                    return false;

                ValidateABWorkload();
                report = new BattleRenderingBenchmarkSuiteReport(config, runs.ToArray(), workload.Fingerprint);
                RestoreBackend();
                return true;
            }
            catch
            {
                try
                {
                    DisposeAfterFailure();
                }
                catch (Exception cleanupException)
                {
                    UnityEngine.Debug.LogException(cleanupException);
                }
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            try
            {
                activeSession?.Dispose();
            }
            finally
            {
                activeSession = null;
                RestoreBackend();
            }
        }

        private bool StartNextRun()
        {
            BattlePresentationBackendMode backend;
            if (config.Comparison == BattleRenderingBenchmarkComparison.Single)
            {
                if (nextBackendIndex > 0)
                    return false;
                backend = config.Backend;
            }
            else
            {
                if (nextBackendIndex == 0)
                    backend = BattlePresentationBackendMode.CentralOnly;
                else if (nextBackendIndex == 1)
                    backend = BattlePresentationBackendMode.LegacyOnly;
                else
                    return false;
            }
            nextBackendIndex++;
            world.SetBattlePresentationBackend(backend);
            activeSession = sessionFactory(
                config.ForBackend(backend),
                world,
                workload);
            return true;
        }

        private void ValidateABWorkload()
        {
            if (config.Comparison != BattleRenderingBenchmarkComparison.CentralLegacyAB)
                return;
            if (runs.Count != 2 ||
                runs[0].Config.Backend != BattlePresentationBackendMode.CentralOnly ||
                runs[1].Config.Backend != BattlePresentationBackendMode.LegacyOnly ||
                runs[0].WorkloadFingerprint != workload.Fingerprint ||
                runs[1].WorkloadFingerprint != workload.Fingerprint ||
                runs[0].InputFingerprint != runs[1].InputFingerprint ||
                runs[0].FinalRuntimeChecksum != runs[1].FinalRuntimeChecksum ||
                runs[0].BenchmarkRenderTargetWidth != runs[1].BenchmarkRenderTargetWidth ||
                runs[0].BenchmarkRenderTargetHeight != runs[1].BenchmarkRenderTargetHeight ||
                !runs[0].RendererWorkloadValidated ||
                !runs[1].RendererWorkloadValidated)
            {
                throw new InvalidOperationException(
                    "Central/Legacy A/B did not consume the same validated frozen workload.");
            }
        }

        private void RestoreBackend()
        {
            if (backendRestored)
                return;
            backendRestored = true;
            world.SetBattlePresentationBackend(previousBackend);
        }

        private void DisposeAfterFailure()
        {
            disposed = true;
            try
            {
                activeSession?.Dispose();
            }
            finally
            {
                activeSession = null;
                RestoreBackend();
            }
        }
    }

    public sealed class BattleRenderingBenchmarkRunner : MonoBehaviour
    {
        private BattleRenderingBenchmarkSuiteSession session;
        private string outputPath;
        private Action<BattleRenderingBenchmarkRunner, string> completion;
        private bool stopping;

        public static BattleRenderingBenchmarkRunner Start(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            string outputPath,
            Action<BattleRenderingBenchmarkRunner, string> completion = null)
        {
            var host = new GameObject("NTSD Battle Rendering Benchmark Runner")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            DontDestroyOnLoad(host);
            BattleRenderingBenchmarkRunner runner = host.AddComponent<BattleRenderingBenchmarkRunner>();
            try
            {
                runner.Initialize(config, world, outputPath, completion);
                return runner;
            }
            catch
            {
                DisposeHost(host);
                throw;
            }
        }

        public void Abort(string reason)
        {
            if (stopping)
                return;
            stopping = true;
            session?.Dispose();
            session = null;
            Notify("FAIL\n" + (reason ?? "Benchmark aborted."));
            DisposeHost(gameObject);
        }

        private void Initialize(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            string path,
            Action<BattleRenderingBenchmarkRunner, string> callback)
        {
            outputPath = path;
            completion = callback;
            session = new BattleRenderingBenchmarkSuiteSession(config, world);
        }

        private void Update()
        {
            if (stopping || session == null)
                return;
            try
            {
                if (!session.CaptureFrame())
                    return;
                session.Report.WriteJson(outputPath);
                StopWithResult(session.Report.Verdict.ToString().ToUpperInvariant() + "\n" + outputPath);
            }
            catch (Exception ex)
            {
                Abort(ex.ToString());
            }
        }

        private void StopWithResult(string result)
        {
            if (stopping)
                return;
            stopping = true;
            session.Dispose();
            session = null;
            Notify(result);
            DisposeHost(gameObject);
        }

        private void Notify(string result)
        {
            Action<BattleRenderingBenchmarkRunner, string> callback = completion;
            completion = null;
            callback?.Invoke(this, result);
        }

        private void OnDestroy()
        {
            if (!stopping && session != null)
            {
                session.Dispose();
                session = null;
                Notify("FAIL\nBenchmark runner was destroyed before completion.");
            }
        }

        private static void DisposeHost(UnityEngine.Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }

    public static class BattleRenderingBenchmarkPlayerArguments
    {
        public const string EnableArgument = "-ntsdBattleRenderingBenchmark";
        public const string ScenarioArgument = "-ntsdBattleRenderingBenchmarkScenario";
        public const string BackendArgument = "-ntsdBattleRenderingBenchmarkBackend";
        public const string ComparisonArgument = "-ntsdBattleRenderingBenchmarkComparison";
        public const string WarmupArgument = "-ntsdBattleRenderingBenchmarkWarmup";
        public const string SampleArgument = "-ntsdBattleRenderingBenchmarkSamples";
        public const string LeakArgument = "-ntsdBattleRenderingBenchmarkLeakFrames";
        public const string OutputArgument = "-ntsdBattleRenderingBenchmarkOutput";

        public static bool TryParse(
            string[] arguments,
            out BattleRenderingBenchmarkRequest request,
            out string error)
        {
            request = null;
            error = string.Empty;
            if (!ContainsFlag(arguments, EnableArgument))
                return false;

            var parsed = new BattleRenderingBenchmarkRequest();
            try
            {
                parsed.targetActiveEntities = FindValue(arguments, ScenarioArgument) ?? "1000";
                parsed.backend = FindValue(arguments, BackendArgument) ??
                                 nameof(BattlePresentationBackendMode.CentralOnly);
                parsed.comparison = FindValue(arguments, ComparisonArgument) ?? "ab";
                parsed.warmupFrames = ParseInt(arguments, WarmupArgument, parsed.warmupFrames);
                parsed.sampleFrames = ParseInt(arguments, SampleArgument, parsed.sampleFrames);
                parsed.leakCheckFrames = ParseInt(arguments, LeakArgument, parsed.leakCheckFrames);
                parsed.outputPath = FindValue(arguments, OutputArgument) ??
                                    "NTSD_BattleRenderingBenchmark-Player.json";
                BattleRenderingBenchmarkConfig.FromRequest(parsed);
                request = parsed;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static int ParseInt(string[] arguments, string name, int fallback)
        {
            string value = FindValue(arguments, name);
            if (string.IsNullOrWhiteSpace(value))
                return fallback;
            if (!int.TryParse(value, out int parsed))
                throw new ArgumentException($"Argument {name} requires an integer value.");
            return parsed;
        }

        private static bool ContainsFlag(string[] arguments, string name)
        {
            if (arguments == null)
                return false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arguments[index], name + "=true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static string FindValue(string[] arguments, string name)
        {
            if (arguments == null)
                return null;
            string prefix = name + "=";
            for (int index = 0; index < arguments.Length; index++)
            {
                string argument = arguments[index];
                if (argument != null &&
                    argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return argument.Substring(prefix.Length);
                }
                if (string.Equals(argument, name, StringComparison.OrdinalIgnoreCase) &&
                    index + 1 < arguments.Length)
                {
                    return arguments[index + 1];
                }
            }
            return null;
        }
    }

    internal static class BattleRenderingBenchmarkPlayerBootstrap
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TryStart()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            bool explicitlyRequested = false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(
                        arguments[index],
                        BattleRenderingBenchmarkPlayerArguments.EnableArgument,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        arguments[index],
                        BattleRenderingBenchmarkPlayerArguments.EnableArgument + "=true",
                        StringComparison.OrdinalIgnoreCase))
                {
                    explicitlyRequested = true;
                    break;
                }
            }
            if (!explicitlyRequested)
                return;

            if (!BattleRenderingBenchmarkPlayerArguments.TryParse(
                    arguments,
                    out BattleRenderingBenchmarkRequest request,
                    out string error))
            {
                UnityEngine.Debug.LogError("[BattleRenderingBenchmark] Invalid Player request: " + error);
                Application.Quit(2);
                return;
            }

            try
            {
                BattleRenderingBenchmarkConfig config =
                    BattleRenderingBenchmarkConfig.FromRequest(request);
                SimulationWorld world = config.Scenario.UsesCurrentScene
                    ? SimulationTickDriver.Instance?.World
                    : null;
                if (config.Scenario.UsesCurrentScene && world == null)
                    throw new InvalidOperationException("Current-scene Player benchmark has no active SimulationWorld.");
                BattleRenderingBenchmarkRunner.Start(
                    config,
                    world,
                    config.OutputPath,
                    (_, result) =>
                    {
                        bool passed = result != null && result.StartsWith("PASS", StringComparison.Ordinal);
                        UnityEngine.Debug.Log("[BattleRenderingBenchmark] Player result: " + result);
                        Application.Quit(passed ? 0 : 1);
                    });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[BattleRenderingBenchmark] Player start failed: " + ex);
                Application.Quit(2);
            }
        }
#endif
    }

    public enum BattleBenchmarkRecorderKind : byte
    {
        FrameTime = 0,
        MainThread = 1,
        RenderThread = 2,
        GpuFrame = 3,
        LogicTick = 4,
        ManagedAllocation = 5,
        DrawCalls = 6,
        TotalMemory = 7,
        GraphicsMemory = 8,
    }

    internal static class BattleRenderingBenchmarkMemory
    {
        internal static BattleBenchmarkMetric CaptureTotalAllocatedMemory()
        {
            return BattleBenchmarkMetric.FromValue(Profiler.GetTotalAllocatedMemoryLong(), "bytes");
        }

        internal static BattleBenchmarkMetric CaptureGraphicsMemory()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return BattleBenchmarkMetric.FromValue(
                Profiler.GetAllocatedMemoryForGraphicsDriver(),
                "bytes");
#else
            return BattleBenchmarkMetric.Unavailable("bytes");
#endif
        }

    }

    public sealed class BattleBenchmarkCompletedFrameAttribution
    {
        private int pendingGeneration;
        private int requestedUnityFrame;
        private ulong timingWatermark;
        private ulong lastAcceptedTimingTimestamp;
        private bool countersSnapshotted;

        public bool CountersSnapshotted => countersSnapshotted;

        public void Request(int generation, int unityFrame, ulong latestTimingTimestamp)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
            requestedUnityFrame = unityFrame;
            timingWatermark = Math.Max(latestTimingTimestamp, lastAcceptedTimingTimestamp);
            countersSnapshotted = false;
        }

        public bool ShouldSnapshotCounters(int generation, int unityFrame)
        {
            if (generation != pendingGeneration || countersSnapshotted || unityFrame <= requestedUnityFrame)
                return false;
            countersSnapshotted = true;
            return true;
        }

        public bool TryAcceptTiming(int generation, ulong frameStartTimestamp)
        {
            if (generation != pendingGeneration || !countersSnapshotted ||
                frameStartTimestamp == 0UL || frameStartTimestamp <= timingWatermark)
            {
                return false;
            }
            lastAcceptedTimingTimestamp = frameStartTimestamp;
            pendingGeneration = 0;
            return true;
        }

        public void CompleteWithoutTiming(int generation)
        {
            if (generation == pendingGeneration)
                pendingGeneration = 0;
        }

        public void ResetPending()
        {
            pendingGeneration = 0;
            requestedUnityFrame = 0;
            timingWatermark = lastAcceptedTimingTimestamp;
            countersSnapshotted = false;
        }
    }

    internal sealed class BattleBenchmarkUnityCompletedFrameCollector :
        IBattleBenchmarkCompletedFrameCollector
    {
        private const int MaxFrameTimingDrainAttempts = 4;
        private readonly BattleRenderingBenchmarkPolicyContext context;
        private readonly BattleBenchmarkCounterRecorder managedAllocation;
        private readonly BattleBenchmarkCounterRecorder drawCalls;
        private readonly BattleBenchmarkCompletedFrameAttribution attribution =
            new BattleBenchmarkCompletedFrameAttribution();
        private readonly FrameTiming[] timings = new FrameTiming[1];
        private int pendingGeneration;
        private int drainAttempts;
        private int lastDrainUnityFrame = -1;
        private string frameTimingReason = string.Empty;
        private BattleBenchmarkMetric managedAllocationSnapshot;
        private BattleBenchmarkMetric drawCallsSnapshot;
        private BattleBenchmarkMetric totalMemorySnapshot;
        private BattleBenchmarkMetric graphicsMemorySnapshot;

        internal BattleBenchmarkUnityCompletedFrameCollector(
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext)
        {
            context = benchmarkPolicyContext;
            managedAllocation = new BattleBenchmarkCounterRecorder(
                ProfilerCategory.Memory,
                "GC Allocated In Frame",
                "bytes");
            drawCalls = new BattleBenchmarkCounterRecorder(
                ProfilerCategory.Render,
                "Draw Calls Count",
                "count");
        }

        public bool IsSupported =>
            context.IsSupportedExecutionScope && context.FrameTimingStatsEnabled;

        public string UnsupportedReason
        {
            get
            {
                if (!context.IsPlaying)
                    return "EditMode has no completed rendered-frame collection scope.";
                if (!context.IsSupportedExecutionScope)
                    return "Completed-frame collection is supported only in Play Mode Editor and Windows Standalone.";
                if (!context.FrameTimingStatsEnabled)
                    return "FrameTimingManager.IsFeatureEnabled returned false.";
                return frameTimingReason;
            }
        }

        public void Request(int generation)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
            drainAttempts = 0;
            lastDrainUnityFrame = -1;
            frameTimingReason = string.Empty;
            managedAllocationSnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            drawCallsSnapshot = BattleBenchmarkMetric.Unavailable("count");
            totalMemorySnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            graphicsMemorySnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            managedAllocation.Restart();
            drawCalls.Restart();
            if (IsSupported)
            {
                attribution.Request(generation, Time.frameCount, LatestTimingTimestamp());
                FrameTimingManager.CaptureFrameTimings();
            }
        }

        public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics)
        {
            if (pendingGeneration != generation)
            {
                metrics = default;
                return false;
            }
            if (!IsSupported)
            {
                pendingGeneration = 0;
                managedAllocation.Abort();
                drawCalls.Abort();
                metrics = BattleBenchmarkCompletedFrameMetrics.Unavailable();
                return true;
            }
            if (!attribution.CountersSnapshotted &&
                !attribution.ShouldSnapshotCounters(generation, Time.frameCount))
            {
                metrics = default;
                return false;
            }
            if (attribution.CountersSnapshotted && drainAttempts == 0)
                SnapshotAndStopCounters();
            if (lastDrainUnityFrame == Time.frameCount)
            {
                metrics = default;
                return false;
            }
            lastDrainUnityFrame = Time.frameCount;

            drainAttempts++;
            uint count = FrameTimingManager.GetLatestTimings(1, timings);
            bool timingAccepted = count > 0 &&
                                  attribution.TryAcceptTiming(
                                      generation,
                                      timings[0].frameStartTimestamp);
            if (!timingAccepted && drainAttempts < MaxFrameTimingDrainAttempts)
            {
                metrics = default;
                return false;
            }

            pendingGeneration = 0;
            if (!timingAccepted)
            {
                frameTimingReason =
                    count == 0
                        ? "FrameTimingManager returned no completed timing after the bounded drain window."
                        : "FrameTimingManager returned only stale timing generations after the bounded drain window.";
                attribution.CompleteWithoutTiming(generation);
                metrics = new BattleBenchmarkCompletedFrameMetrics(
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    managedAllocationSnapshot,
                    drawCallsSnapshot,
                    totalMemorySnapshot,
                    graphicsMemorySnapshot);
                return true;
            }

            FrameTiming timing = timings[0];
            metrics = new BattleBenchmarkCompletedFrameMetrics(
                PositiveMilliseconds(timing.cpuFrameTime),
                PositiveMilliseconds(timing.cpuMainThreadFrameTime),
                context.GraphicsMultiThreaded
                    ? PositiveMilliseconds(timing.cpuRenderThreadFrameTime)
                    : BattleBenchmarkMetric.Unavailable("ms"),
                PositiveMilliseconds(timing.gpuFrameTime),
                managedAllocationSnapshot,
                drawCallsSnapshot,
                totalMemorySnapshot,
                graphicsMemorySnapshot);
            return true;
        }

        public string Source(BattleBenchmarkRecorderKind kind)
        {
            switch (kind)
            {
                case BattleBenchmarkRecorderKind.FrameTime:
                case BattleBenchmarkRecorderKind.MainThread:
                case BattleBenchmarkRecorderKind.RenderThread:
                case BattleBenchmarkRecorderKind.GpuFrame:
                    return "FrameTimingManager completed frame";
                case BattleBenchmarkRecorderKind.ManagedAllocation:
                    return managedAllocation.Source;
                case BattleBenchmarkRecorderKind.DrawCalls:
                    return drawCalls.Source;
                case BattleBenchmarkRecorderKind.TotalMemory:
                    return "Profiler.GetTotalAllocatedMemoryLong";
                case BattleBenchmarkRecorderKind.GraphicsMemory:
                    return "Profiler.GetAllocatedMemoryForGraphicsDriver";
                default:
                    return string.Empty;
            }
        }

        public string Reason(BattleBenchmarkRecorderKind kind)
        {
            switch (kind)
            {
                case BattleBenchmarkRecorderKind.ManagedAllocation:
                    return managedAllocation.Reason;
                case BattleBenchmarkRecorderKind.DrawCalls:
                    return drawCalls.Reason;
                case BattleBenchmarkRecorderKind.GraphicsMemory:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    return string.Empty;
#else
                    return "Profiler.GetAllocatedMemoryForGraphicsDriver is available only in Editor or Development Player.";
#endif
                default:
                    return frameTimingReason;
            }
        }

        public void Reset()
        {
            pendingGeneration = 0;
            drainAttempts = 0;
            lastDrainUnityFrame = -1;
            frameTimingReason = string.Empty;
            attribution.ResetPending();
            managedAllocation.Abort();
            drawCalls.Abort();
        }

        public void Dispose()
        {
            Reset();
            managedAllocation.Dispose();
            drawCalls.Dispose();
        }

        private void SnapshotAndStopCounters()
        {
            managedAllocationSnapshot = managedAllocation.SnapshotAndStop();
            drawCallsSnapshot = drawCalls.SnapshotAndStop(requirePositive: true);
            totalMemorySnapshot = BattleRenderingBenchmarkMemory.CaptureTotalAllocatedMemory();
            graphicsMemorySnapshot = BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
        }

        private ulong LatestTimingTimestamp()
        {
            uint count = FrameTimingManager.GetLatestTimings(1, timings);
            return count > 0 ? timings[0].frameStartTimestamp : 0UL;
        }

        private static BattleBenchmarkMetric PositiveMilliseconds(double value)
        {
            return value > 0d
                ? BattleBenchmarkMetric.FromValue(value, "ms")
                : BattleBenchmarkMetric.Unavailable("ms");
        }
    }

    internal sealed class BattleBenchmarkCounterRecorder : IDisposable
    {
        private readonly string marker;
        private readonly string unit;
        private ProfilerRecorder recorder;
        private bool valid;

        internal BattleBenchmarkCounterRecorder(
            ProfilerCategory category,
            string profilerMarker,
            string metricUnit)
        {
            marker = profilerMarker;
            unit = metricUnit;
            Source = "ProfilerRecorderHandle:" + marker;
            var handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);
            for (int index = 0; index < handles.Count; index++)
            {
                ProfilerRecorderDescription description =
                    ProfilerRecorderHandle.GetDescription(handles[index]);
                if (description.Category != category ||
                    !string.Equals(description.Name, marker, StringComparison.Ordinal))
                {
                    continue;
                }
                try
                {
                    recorder = new ProfilerRecorder(
                        handles[index],
                        1,
                        ProfilerRecorderOptions.Default);
                    valid = recorder.Valid;
                }
                catch (Exception ex)
                {
                    Reason = "ProfilerRecorder start failed: " + ex.GetType().Name;
                }
                break;
            }
            if (!valid && string.IsNullOrEmpty(Reason))
                Reason = "The exact profiler counter was not discovered on this platform.";
        }

        internal string Source { get; }
        internal string Reason { get; private set; } = string.Empty;

        internal void Restart()
        {
            if (!valid)
                return;
            recorder.Reset();
            Reason = string.Empty;
            recorder.Start();
        }

        internal BattleBenchmarkMetric SnapshotAndStop(bool requirePositive = false)
        {
            if (valid)
                recorder.Stop();
            BattleBenchmarkMetric metric = BattleBenchmarkCounterSamplePolicy.Capture(
                valid,
                valid ? recorder.Count : 0,
                valid && recorder.Count > 0 ? recorder.LastValue : 0L,
                unit,
                requirePositive,
                out string reason);
            if (!string.IsNullOrEmpty(reason))
                Reason = reason;
            return metric;
        }

        internal void Abort()
        {
            if (valid)
                recorder.Reset();
        }

        public void Dispose()
        {
            recorder.Dispose();
            valid = false;
        }
    }

    public static class BattleBenchmarkCounterSamplePolicy
    {
        public static BattleBenchmarkMetric Capture(
            bool recorderValid,
            int sampleCount,
            long value,
            string unit,
            bool requirePositive,
            out string reason)
        {
            if (!recorderValid)
            {
                reason = "The exact profiler counter was not discovered on this platform.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            if (sampleCount <= 0)
            {
                reason = "The profiler counter produced no completed-frame sample.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            if (requirePositive && value <= 0L)
            {
                reason = "The profiler counter returned zero for a non-empty benchmark render workload.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            reason = string.Empty;
            return BattleBenchmarkMetric.FromValue(value, unit);
        }
    }

    public static class BattleBenchmarkOwnedTextureMemoryPolicy
    {
        public static BattleBenchmarkMetric Capture(
            int resourceGeneration,
            int ownedTextureResourceCount,
            long measuredBytes,
            out string reason)
        {
            if (resourceGeneration <= 0)
            {
                reason = "The benchmark-owned texture evidence has no valid resource generation.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }
            if (ownedTextureResourceCount <= 0)
            {
                reason = "The benchmark resource generation owns no Texture2D or RenderTexture objects.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }
            if (measuredBytes <= 0L)
            {
                reason = "Profiler.GetRuntimeMemorySizeLong returned no positive bytes for the benchmark-owned texture resources.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }

            reason = string.Empty;
            return BattleBenchmarkMetric.FromValue(measuredBytes, "bytes");
        }
    }

    public interface IBattleRenderingBenchmarkPresenter : IDisposable
    {
        string Implementation { get; }
        string EffectiveBackend { get; }
        string ResourceMode { get; }
        string DrawMode { get; }
        int RenderTargetWidth { get; }
        int RenderTargetHeight { get; }
        int ResolvedCommandCount { get; }
        int MaterializedRenderItemCount { get; }
        int ResourceSegmentCount { get; }
        int SubmissionDrawCount { get; }
        string SubmissionDrawMetricSource { get; }
        string SubmissionDrawUnavailableReason { get; }
        int ResourceGeneration { get; }
        int OwnedTextureResourceCount { get; }
        int OwnedResourceCount { get; }
        long CachedOwnedResourceMemoryBytes { get; }
        long MeasureOwnedResourceMemoryBytes();
        long MeasureOwnedTextureMemoryBytes();
        BattleCentralBuildDiagnostics Diagnostics { get; }
        double Present();
    }

    internal static class BattleRenderingBenchmarkPresenterFactory
    {
        internal static IBattleRenderingBenchmarkPresenter Create(
            BattlePresentationBackendMode backend,
            BattleRenderingBenchmarkWorkload workload)
        {
            switch (backend)
            {
                case BattlePresentationBackendMode.CentralOnly:
                    return new BattleBenchmarkCentralPresenter(workload);
                case BattlePresentationBackendMode.LegacyOnly:
                    return new BattleBenchmarkLegacyPresenter(workload);
                default:
                    throw new ArgumentOutOfRangeException(nameof(backend));
            }
        }
    }

    internal sealed class BattleBenchmarkCentralPresenter : IBattleRenderingBenchmarkPresenter
    {
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleBenchmarkResourceSet resources;
        private readonly BattleDynamicMeshBackend backend = new BattleDynamicMeshBackend();
        private readonly long cachedOwnedResourceMemoryBytes;
        private int lastSubmissionDrawCount = BattleRenderingBenchmarkSubmissionPolicy.Unavailable;
        private bool disposed;

        internal BattleBenchmarkCentralPresenter(BattleRenderingBenchmarkWorkload workload)
        {
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            resources = new BattleBenchmarkResourceSet("Central");
            Present();
            cachedOwnedResourceMemoryBytes = MeasureOwnedResourceMemoryBytes();
        }

        public string Implementation => "BenchmarkCentralPersistentDynamicMesh";
        public string EffectiveBackend => BattlePresentationBackendMode.CentralOnly.ToString();
        public string ResourceMode => BattleSpriteCentralBindingMode.SourceTexture2D.ToString();
        public string DrawMode => BattleCentralDrawMode.OrderedChunks.ToString();
        public int RenderTargetWidth => resources.RenderTargetWidth;
        public int RenderTargetHeight => resources.RenderTargetHeight;
        public int ResolvedCommandCount => backend.Diagnostics.ResolvedCommandCount;
        public int MaterializedRenderItemCount => backend.Diagnostics.ResolvedCommandCount;
        public int ResourceSegmentCount => backend.Diagnostics.SegmentCount;
        public int SubmissionDrawCount => lastSubmissionDrawCount;
        public string SubmissionDrawMetricSource => "Graphics.DrawMesh calls issued by the central presenter";
        public string SubmissionDrawUnavailableReason =>
            "Application is not in Play Mode; the central presenter built mesh segments but did not call Graphics.DrawMesh.";
        public int ResourceGeneration => resources.ResourceGeneration;
        public int OwnedTextureResourceCount => resources.OwnedTextureResourceCount;
        public int OwnedResourceCount => disposed
            ? 0
            : resources.OwnedResourceCount + backend.AllocatedChunkCount;
        public long CachedOwnedResourceMemoryBytes => disposed ? 0L : cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
            if (disposed)
                return 0L;
            long bytes = resources.OwnedResourceMemoryBytes;
            for (int index = 0; index < backend.AllocatedChunkCount; index++)
            {
                Mesh mesh = backend.GetChunkMesh(index);
                if (mesh != null)
                    bytes += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            return bytes;
        }
        public long MeasureOwnedTextureMemoryBytes() => resources.OwnedTextureMemoryBytes;
        public BattleCentralBuildDiagnostics Diagnostics => backend.Diagnostics;

        public double Present()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleBenchmarkCentralPresenter));
            long started = Stopwatch.GetTimestamp();
            backend.Build(
                workload.FrozenFrame,
                resources,
                BattleCentralDrawMode.OrderedChunks);
            double elapsed = BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
            int issuedDrawCalls = 0;
            if (Application.isPlaying)
            {
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    Graphics.DrawMesh(
                        backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity,
                        segment.Material,
                        BattleBenchmarkResourceSet.BenchmarkLayer,
                        resources.Camera,
                        segment.SubMeshIndex,
                        null,
                        false,
                        false,
                        false);
                    issuedDrawCalls++;
                }
            }
            lastSubmissionDrawCount = BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(
                issuedDrawCalls > 0,
                issuedDrawCalls);
            return elapsed;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            backend.Dispose();
            resources.Dispose();
        }
    }

    internal sealed class BattleBenchmarkLegacyPresenter : IBattleRenderingBenchmarkPresenter
    {
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleBenchmarkResourceSet resources;
        private readonly GameObject root;
        private readonly Transform[] transforms;
        private readonly SpriteRenderer[] renderers;
        private readonly long cachedOwnedResourceMemoryBytes;
        private bool disposed;

        internal BattleBenchmarkLegacyPresenter(BattleRenderingBenchmarkWorkload workload)
        {
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            resources = new BattleBenchmarkResourceSet("Legacy");
            root = new GameObject("NTSD Benchmark Legacy Presenter")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = BattleBenchmarkResourceSet.BenchmarkLayer,
            };
            transforms = new Transform[workload.CommandCount];
            renderers = new SpriteRenderer[workload.CommandCount];
            for (int index = 0; index < workload.CommandCount; index++)
            {
                var child = new GameObject("LegacyCommand" + index)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    layer = BattleBenchmarkResourceSet.BenchmarkLayer,
                };
                child.transform.SetParent(root.transform, false);
                child.transform.localScale = NTSDRenderSpace.RenderScale;
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                renderer.sprite = resources.Sprite;
                renderer.sharedMaterial = resources.Material;
                transforms[index] = child.transform;
                renderers[index] = renderer;
            }
            Present();
            cachedOwnedResourceMemoryBytes = MeasureOwnedResourceMemoryBytes();
        }

        public string Implementation => "BenchmarkRendererlessLegacyCompatibilityPresenter";
        public string EffectiveBackend => BattlePresentationBackendMode.LegacyOnly.ToString();
        public string ResourceMode => "SharedSourceTexture2D";
        public string DrawMode => "UnitySpriteRendererTransparentSortAndBatch";
        public int RenderTargetWidth => resources.RenderTargetWidth;
        public int RenderTargetHeight => resources.RenderTargetHeight;
        public int ResolvedCommandCount => renderers.Length;
        public int MaterializedRenderItemCount => renderers.Length;
        public int ResourceSegmentCount => renderers.Length > 0 ? 1 : 0;
        public int SubmissionDrawCount => -1;
        public string SubmissionDrawMetricSource =>
            "Unity SpriteRenderer batching is represented by the frame draw-call counter";
        public string SubmissionDrawUnavailableReason =>
            "Legacy SpriteRenderer batching has no reliable presenter-local draw count; use drawCalls when its ProfilerRecorder counter is available.";
        public int ResourceGeneration => resources.ResourceGeneration;
        public int OwnedTextureResourceCount => resources.OwnedTextureResourceCount;
        public int OwnedResourceCount => disposed
            ? 0
            : resources.OwnedResourceCount + 1 + renderers.Length * 3;
        public long CachedOwnedResourceMemoryBytes => disposed ? 0L : cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
            if (disposed)
                return 0L;
            long bytes = resources.OwnedResourceMemoryBytes;
            if (root != null)
                bytes += Profiler.GetRuntimeMemorySizeLong(root);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                    bytes += Profiler.GetRuntimeMemorySizeLong(renderers[index]);
                if (transforms[index] != null)
                {
                    bytes += Profiler.GetRuntimeMemorySizeLong(transforms[index]);
                    bytes += Profiler.GetRuntimeMemorySizeLong(transforms[index].gameObject);
                }
            }
            return bytes;
        }
        public long MeasureOwnedTextureMemoryBytes() => resources.OwnedTextureMemoryBytes;
        public BattleCentralBuildDiagnostics Diagnostics => null;

        public double Present()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleBenchmarkLegacyPresenter));
            long started = Stopwatch.GetTimestamp();
            for (int index = 0; index < workload.CommandCount; index++)
            {
                BattleRenderCommand command = workload.FrozenFrame.GetCommand(index);
                Transform target = transforms[index];
                SpriteRenderer renderer = renderers[index];
                target.localPosition = command.Position;
                renderer.flipX = command.FlipX;
                renderer.flipY = command.FlipY;
                renderer.color = command.Color;
                renderer.sortingOrder = command.SortOrder;
                renderer.enabled = true;
            }
            return BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            BattleRenderingBenchmarkEnvironment.DestroyObject(root);
            resources.Dispose();
        }
    }

    internal sealed class BattleBenchmarkResourceSet : IBattleCentralResourceResolver, IDisposable
    {
        internal const int BenchmarkLayer = 31;
        internal const int BenchmarkRenderTargetWidth = 256;
        internal const int BenchmarkRenderTargetHeight = 256;
        private readonly Texture2D texture;
        private readonly Material material;
        private readonly Sprite sprite;
        private readonly GameObject cameraObject;
        private readonly Camera camera;
        private readonly RenderTexture renderTexture;
        private static int nextResourceGeneration;
        private bool disposed;

        internal BattleBenchmarkResourceSet(string suffix)
        {
            ResourceGeneration = Interlocked.Increment(ref nextResourceGeneration);
            if (ResourceGeneration <= 0)
                throw new InvalidOperationException("Benchmark resource generation overflowed.");
            Shader shader = Shader.Find(BattleSpriteMaterialContract.BuiltInSpriteShaderName);
            if (shader == null)
                throw new InvalidOperationException("Sprites/Default shader is unavailable for the benchmark harness.");
            texture = new Texture2D(16, 16, TextureFormat.RGBA32, false, true)
            {
                name = "NTSD Benchmark Texture " + suffix,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[16 * 16];
            for (int index = 0; index < pixels.Length; index++)
            {
                byte shade = (byte)(((index / 16 + index % 16) & 1) == 0 ? 255 : 192);
                pixels[index] = new Color32(shade, shade, shade, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 16f, 16f),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "NTSD Benchmark Sprite " + suffix;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            material = new Material(shader)
            {
                name = "NTSD Benchmark Material " + suffix,
                mainTexture = texture,
                hideFlags = HideFlags.HideAndDontSave,
            };
            cameraObject = new GameObject("NTSD Benchmark Camera " + suffix)
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = BenchmarkLayer,
            };
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.transform.position = new Vector3(4f, 4f, -10f);
            camera.cullingMask = 1 << BenchmarkLayer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            renderTexture = new RenderTexture(
                BenchmarkRenderTargetWidth,
                BenchmarkRenderTargetHeight,
                16,
                RenderTextureFormat.ARGB32)
            {
                name = "NTSD Benchmark Target " + suffix,
                hideFlags = HideFlags.HideAndDontSave,
            };
            renderTexture.Create();
            camera.targetTexture = renderTexture;
            camera.enabled = Application.isPlaying;
        }

        internal Camera Camera => camera;
        internal int ResourceGeneration { get; }
        internal int RenderTargetWidth => disposed ? 0 : renderTexture.width;
        internal int RenderTargetHeight => disposed ? 0 : renderTexture.height;
        internal Material Material => material;
        internal Sprite Sprite => sprite;
        internal int OwnedResourceCount => disposed ? 0 : 6;
        internal int OwnedTextureResourceCount => disposed ? 0 : 2;
        internal long OwnedTextureMemoryBytes =>
            disposed
                ? 0L
                : BattleRenderingBenchmarkEnvironment.RuntimeMemory(texture) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(renderTexture);
        internal long OwnedResourceMemoryBytes =>
            disposed
                ? 0L
                : BattleRenderingBenchmarkEnvironment.RuntimeMemory(texture) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(material) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(sprite) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(cameraObject) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(camera) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(renderTexture);

        public BattleCentralResourceStatus Resolve(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = new BattleCentralResolvedResource(
                texture,
                material,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(16f, 16f),
                new Vector2(0.5f, 0.5f),
                command.Color,
                0,
                0,
                BattleSpriteCentralBindingMode.SourceTexture2D);
            return BattleCentralResourceStatus.Resolved;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            if (camera != null)
                camera.targetTexture = null;
            if (renderTexture != null)
                renderTexture.Release();
            BattleRenderingBenchmarkEnvironment.DestroyObject(cameraObject);
            BattleRenderingBenchmarkEnvironment.DestroyObject(renderTexture);
            BattleRenderingBenchmarkEnvironment.DestroyObject(material);
            BattleRenderingBenchmarkEnvironment.DestroyObject(sprite);
            BattleRenderingBenchmarkEnvironment.DestroyObject(texture);
        }
    }

    internal static class BattleRenderingBenchmarkEnvironment
    {
        internal static Dictionary<string, object> Capture()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["applicationPlatform"] = Application.platform.ToString(),
                ["deviceModel"] = SystemInfo.deviceModel,
                ["editor"] = Application.isEditor,
                ["graphicsApi"] = SystemInfo.graphicsDeviceType.ToString(),
                ["gpu"] = SystemInfo.graphicsDeviceName,
                ["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion,
                ["graphicsMemoryCapacityMB"] = SystemInfo.graphicsMemorySize,
                ["resolutionHeight"] = Screen.height,
                ["resolutionWidth"] = Screen.width,
                ["runtime"] = Application.isEditor ? "Editor" : "Player",
            };
        }

        internal static double ElapsedMilliseconds(long startedTimestamp)
        {
            long elapsed = Stopwatch.GetTimestamp() - startedTimestamp;
            return elapsed * 1000d / Stopwatch.Frequency;
        }

        internal static long RuntimeMemory(UnityEngine.Object target)
        {
            return target == null ? 0L : Profiler.GetRuntimeMemorySizeLong(target);
        }

        internal static void WriteJson(string path, string json)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An output path is required.", nameof(path));
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, json, new System.Text.UTF8Encoding(false));
        }

        internal static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }
    }
}
