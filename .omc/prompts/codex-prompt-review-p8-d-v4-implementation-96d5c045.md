---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\BattleRenderingBenchmark.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\Editor\\BattleRenderingBenchmarkEditorTests.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Animation\\Rendering\\Editor\\BattleRenderingBenchmarkWindow.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Docs\\central-battle-render-system-plan.md"
timestamp: "2026-07-23T13:43:30.884Z"
---

--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\BattleRenderingBenchmark.cs ---
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
        public const string PolicyId = "ntsd-battle-rendering-benchmark-policy-v4";

        public static BattleRenderingBenchmarkVerdict Evaluate(
            BattleRenderingBenchmarkPolicyContext context,
            IReadOnlyList<BattleBenchmarkMetricAvailability> metrics,
            out string reason,
            out string[] missingRequiredMetrics)
        {
            var missing = new List<string>();
            var failed = new List<string>();
            for (int index = 0; index < metrics.Count; index++)
            {
                BattleBenchmarkMetricAvailability metric = metrics[index];
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
            missingRequiredMetrics = missing.ToArray();

            if (!context.IsPlaying)
            {
                reason = "EditMode does not provide a completed rendered-frame benchmark scope.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (!context.IsSupportedExecutionScope)
            {
                reason = "The v4 policy supports Play Mode Editor and Windows Standalone only.";
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

            reason = "All required v4 metrics and validation gates passed.";
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
        public BattleBenchmarkMetric TextureMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric BenchmarkOwnedMemoryBytes { get; internal set; }
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
                ["textureMemoryBytes"] = TextureMemoryBytes.ToProjection(),
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
            string measurementMode,
            string reason)
        {
            Available = available;
            Passed = passed;
            SoakFrames = soakFrames;
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
            MeasurementMode = measurementMode ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public bool Available { get; }
        public bool Passed { get; }
        public int SoakFrames { get; }
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
        public string MeasurementMode { get; }
        public string Reason { get; }
        public long ManagedGrowthBytes => ManagedEndBytes - ManagedStartBytes;
        public long GraphicsGrowthBytes => GraphicsEndBytes - GraphicsStartBytes;
        public long OwnedGrowthBytes => OwnedEndBytes - OwnedStartBytes;

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["graphicsAvailable"] = GraphicsAvailable,
                ["graphicsEndBytes"] = GraphicsAvailable ? (object)GraphicsEndBytes : null,
                ["graphicsGrowthBytes"] = GraphicsAvailable ? (object)GraphicsGrowthBytes : null,
                ["graphicsStartBytes"] = GraphicsAvailable ? (object)GraphicsStartBytes : null,
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
            };
        }

        internal static BattleRenderingBenchmarkLeakReport NotRequested()
        {
            return new BattleRenderingBenchmarkLeakReport(
                false,
                false,
                0,
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
                "not-requested",
                "Leak/long-run soak was not requested.");
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
            };
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["config"] = config,
                ["benchmarkRenderTargetHeight"] = BenchmarkRenderTargetHeight,
                ["benchmarkRenderTargetWidth"] = BenchmarkRenderTargetWidth,
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
                ["schema"] = "ntsd-battle-rendering-benchmark-run-v4",
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
                ["textureMemoryBytes"] = Summarize(frame => frame.TextureMemoryBytes),
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
                    return "All suite runs passed the v4 policy.";
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
                ["schema"] = "ntsd-battle-rendering-benchmark-suite-v4",
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
            BattleBenchmarkMetric graphicsMemoryBytes,
            BattleBenchmarkMetric textureMemoryBytes)
        {
            FrameTimeMs = frameTimeMs;
            MainThreadTimeMs = mainThreadTimeMs;
            RenderThreadTimeMs = renderThreadTimeMs;
            GpuFrameTimeMs = gpuFrameTimeMs;
            ManagedAllocationBytes = managedAllocationBytes;
            DrawCalls = drawCalls;
            TotalAllocatedMemoryBytes = totalAllocatedMemoryBytes;
            GraphicsMemoryBytes = graphicsMemoryBytes;
            TextureMemoryBytes = textureMemoryBytes;
        }

        public BattleBenchmarkMetric FrameTimeMs { get; }
        public BattleBenchmarkMetric MainThreadTimeMs { get; }
        public BattleBenchmarkMetric RenderThreadTimeMs { get; }
        public BattleBenchmarkMetric GpuFrameTimeMs { get; }
        public BattleBenchmarkMetric ManagedAllocationBytes { get; }
        public BattleBenchmarkMetric DrawCalls { get; }
        public BattleBenchmarkMetric TotalAllocatedMemoryBytes { get; }
        public BattleBenchmarkMetric GraphicsMemoryBytes { get; }
        public BattleBenchmarkMetric TextureMemoryBytes { get; }

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

    public sealed class BattleRenderingBenchmarkSession : IDisposable
    {
        private readonly BattleRenderingBenchmarkConfig config;
        private readonly SimulationWorld world;
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleRenderingBenchmarkPolicyContext policyContext;
        private readonly IBattleBenchmarkCompletedFrameCollector completedFrameCollector;
        private readonly IBattleRenderingBenchmarkPresenter presenter;
        private readonly List<BattleRenderingBenchmarkFrame> frames;
        private static int nextGeneration;
        private BattleRenderingBenchmarkReport report;
        private BattleRenderingBenchmarkLeakReport leakReport;
        private bool disposed;
        private int frameIndex;
        private int pendingGeneration;
        private int pendingSampleIndex;
        private bool pendingWarmup;
        private bool pendingLeakFrame;
        private double pendingPresentationMs;
        private int leakFramesCaptured;
        private bool leakBaselineCaptured;
        private long leakManagedStart;
        private long leakGraphicsStart;
        private bool leakGraphicsAvailable;
        private long leakOwnedStart;
        private int leakResourcesStart;

        public const string RetainedManagedHeapMeasurementMode =
            "full-gc-retained-managed-heap-outside-performance-sample-window-v1";

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
        {
            if (config.Comparison != BattleRenderingBenchmarkComparison.Single)
                throw new ArgumentException("A single run session requires Single comparison mode.", nameof(config));
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            policyContext = benchmarkPolicyContext;
            ValidateCount();
            frames = new List<BattleRenderingBenchmarkFrame>(config.SampleFrames);
            completedFrameCollector = collector ??
                new BattleBenchmarkUnityCompletedFrameCollector(policyContext);
            try
            {
                presenter = benchmarkPresenter ??
                    BattleRenderingBenchmarkPresenterFactory.Create(config.Backend, workload);
                ValidatePresenterWorkload();
            }
            catch
            {
                completedFrameCollector.Dispose();
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
                    frames.Add(CaptureSample(pendingSampleIndex, pendingPresentationMs, completedMetrics));
                pendingGeneration = 0;
                pendingWarmup = false;
                pendingLeakFrame = false;

                if (completedLeakFrame)
                {
                    if (leakFramesCaptured < config.LeakCheckFrames)
                        return false;
                    FinalizeLeakReport();
                    FinalizeReport();
                    return true;
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

            if (frames.Count < config.SampleFrames)
            {
                ValidateCount();
                pendingPresentationMs = presenter.Present();
                ValidatePresenterWorkload();
                int currentFrame = frameIndex++;
                pendingWarmup = currentFrame < config.WarmupFrames;
                pendingSampleIndex = frames.Count;
                pendingGeneration = Interlocked.Increment(ref nextGeneration);
                if (pendingGeneration == 0)
                    pendingGeneration = Interlocked.Increment(ref nextGeneration);
                completedFrameCollector.Request(pendingGeneration);
                return false;
            }

            if (!leakBaselineCaptured)
                CaptureLeakBaseline();
            ValidateCount();
            pendingPresentationMs = presenter.Present();
            ValidatePresenterWorkload();
            pendingWarmup = false;
            pendingLeakFrame = true;
            pendingGeneration = Interlocked.Increment(ref nextGeneration);
            if (pendingGeneration == 0)
                pendingGeneration = Interlocked.Increment(ref nextGeneration);
            completedFrameCollector.Request(pendingGeneration);
            return false;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            presenter.Dispose();
            completedFrameCollector.Dispose();
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
                DrawCalls = completedMetrics.DrawCalls,
                PresenterSubmittedRenderItems = BattleBenchmarkMetric.FromValue(
                    presenter.MaterializedRenderItemCount,
                    "count"),
                PresenterSubmissionDrawCalls = presenter.SubmissionDrawCount >= 0
                    ? BattleBenchmarkMetric.FromValue(presenter.SubmissionDrawCount, "count")
                    : BattleBenchmarkMetric.Unavailable("count"),
                TotalAllocatedMemoryBytes = completedMetrics.TotalAllocatedMemoryBytes,
                GraphicsMemoryBytes = completedMetrics.GraphicsMemoryBytes,
                TextureMemoryBytes = completedMetrics.TextureMemoryBytes,
                BenchmarkOwnedMemoryBytes = BattleBenchmarkMetric.FromValue(
                    presenter.CachedOwnedResourceMemoryBytes,
                    "bytes"),
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

        private void CaptureLeakBaseline()
        {
            leakBaselineCaptured = true;
            leakManagedStart = CaptureRetainedManagedHeapBytes();
            leakOwnedStart = presenter.MeasureOwnedResourceMemoryBytes();
            leakResourcesStart = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
            leakGraphicsAvailable = graphics.Available;
            leakGraphicsStart = graphics.Available ? (long)graphics.Value : 0L;
        }

        private void FinalizeLeakReport()
        {
            long managedEnd = CaptureRetainedManagedHeapBytes();
            long ownedEnd = presenter.MeasureOwnedResourceMemoryBytes();
            int resourcesEnd = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
            bool graphicsAvailable = leakGraphicsAvailable && graphics.Available;
            long graphicsEnd = graphicsAvailable ? (long)graphics.Value : 0L;
            long managedGrowth = managedEnd - leakManagedStart;
            long graphicsGrowth = graphicsEnd - leakGraphicsStart;
            bool passed = managedGrowth <= config.MaxManagedGrowthBytes &&
                          graphicsAvailable &&
                          graphicsGrowth <= config.MaxGraphicsGrowthBytes &&
                          ownedEnd <= leakOwnedStart &&
                          resourcesEnd <= leakResourcesStart;
            string reason = !graphicsAvailable
                ? "Graphics memory is required for a requested leak check but was unavailable."
                : passed
                ? "Full-GC retained managed heap, benchmark-owned resources, and configured graphics growth thresholds were respected."
                : "The soak exceeded a retained-heap or graphics threshold, or retained additional benchmark-owned resources.";
            leakReport = new BattleRenderingBenchmarkLeakReport(
                true,
                passed,
                leakFramesCaptured,
                leakManagedStart,
                managedEnd,
                leakGraphicsStart,
                graphicsEnd,
                graphicsAvailable,
                leakOwnedStart,
                ownedEnd,
                leakResourcesStart,
                resourcesEnd,
                config.MaxManagedGrowthBytes,
                config.MaxGraphicsGrowthBytes,
                RetainedManagedHeapMeasurementMode,
                reason);
        }

        private static long CaptureRetainedManagedHeapBytes()
        {
            // These collections run only after the performance sample window has closed.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return GC.GetTotalMemory(false);
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
                presenter.Implementation,
                presenter.ResourceMode,
                presenter.DrawMode,
                presenter.RenderTargetWidth,
                presenter.RenderTargetHeight,
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
            AddFrameMetric(result, "drawCalls", BattleBenchmarkRecorderKind.DrawCalls, frame => frame.DrawCalls);
            AddFrameMetric(result, "totalAllocatedMemoryBytes", BattleBenchmarkRecorderKind.TotalMemory, frame => frame.TotalAllocatedMemoryBytes);
            AddFrameMetric(result, "graphicsMemoryBytes", BattleBenchmarkRecorderKind.GraphicsMemory, frame => frame.GraphicsMemoryBytes);
            AddFrameMetric(result, "textureMemoryBytes", BattleBenchmarkRecorderKind.TextureMemory, frame => frame.TextureMemoryBytes);
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
                presenter.SubmissionDrawMetricSource,
                central,
                presenter.SubmissionDrawUnavailableReason);
            AddLocalMetric(
                result,
                "meshChunks",
                "presenter-local",
                frame => frame.MeshChunks,
                central ? "BattleDynamicMeshBackend diagnostics" : "not applicable",
                central,
                "Legacy compatibility presentation does not build central mesh chunks.");

            AddGate(result, "exactSampleCount", frames.Count == config.SampleFrames,
                frames.Count,
                config.SampleFrames,
                "completed-frame collector");
            AddGate(result, "countValidated", workload.ActualEntityCount == workload.RequestedEntityCount,
                1, 1, "frozen workload entity counts");
            AddGate(result, "runtimeAdmissionValidated", workload.RuntimeAdmissionValidated,
                1, 1, "SimulationWorld runtime admission");
            AddGate(result, "determinismValidated", workload.RuntimeStateDeterministic,
                1, 1, "runtime checksum replay");
            AddGate(result, "rendererWorkloadValidated", true,
                1, 1, "presenter materialization validation");
            if (config.LeakCheckFrames > 0)
            {
                BattleBenchmarkMetricStatus leakStatus = !leakReport.GraphicsAvailable
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
                    leakReport.Available && leakReport.GraphicsAvailable ? 1 : 0,
                    1,
                    RetainedManagedHeapMeasurementMode,
                    leakReport.Reason));
            }
            return result.ToArray();
        }

        private void AddFrameMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            BattleBenchmarkRecorderKind kind,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            bool applicable = true)
        {
            AddMetric(
                result,
                name,
                "completed-frame",
                selector,
                completedFrameCollector.Source(kind),
                required: applicable,
                applicable,
                completedFrameCollector.Reason(kind));
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
            bool passed,
            int sampleCount,
            int expectedSampleCount,
            string source)
        {
            result.Add(new BattleBenchmarkMetricAvailability(
                name,
                true,
                BattleBenchmarkMetricApplicability.Applicable,
                passed ? BattleBenchmarkMetricStatus.Passed : BattleBenchmarkMetricStatus.Failed,
                "validation-gate",
                sampleCount,
                expectedSampleCount,
                source,
                passed ? string.Empty : "The required validation gate failed."));
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
        private BattleRenderingBenchmarkSession activeSession;
        private BattleRenderingBenchmarkSuiteReport report;
        private int nextBackendIndex;
        private bool disposed;
        private bool backendRestored;

        public BattleRenderingBenchmarkSuiteSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world)
        {
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            previousBackend = this.world.BattlePresentation.Mode;
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

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            activeSession?.Dispose();
            activeSession = null;
            RestoreBackend();
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
            activeSession = new BattleRenderingBenchmarkSession(
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
        TextureMemory = 9,
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

        internal static BattleBenchmarkMetric CaptureTextureMemory()
        {
            return BattleBenchmarkMetric.FromValue(Texture.totalTextureMemory, "bytes");
        }
    }

    internal sealed class BattleBenchmarkUnityCompletedFrameCollector :
        IBattleBenchmarkCompletedFrameCollector
    {
        private const int MaxFrameTimingDrainAttempts = 4;
        private readonly BattleRenderingBenchmarkPolicyContext context;
        private readonly BattleBenchmarkCounterRecorder managedAllocation;
        private readonly BattleBenchmarkCounterRecorder drawCalls;
        private readonly FrameTiming[] timings = new FrameTiming[1];
        private int pendingGeneration;
        private int requestedUnityFrame;
        private int drainAttempts;
        private string frameTimingReason = string.Empty;

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
            requestedUnityFrame = Time.frameCount;
            drainAttempts = 0;
            frameTimingReason = string.Empty;
            if (IsSupported)
                FrameTimingManager.CaptureFrameTimings();
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
                metrics = BattleBenchmarkCompletedFrameMetrics.Unavailable();
                return true;
            }
            if (Time.frameCount <= requestedUnityFrame)
            {
                metrics = default;
                return false;
            }

            drainAttempts++;
            uint count = FrameTimingManager.GetLatestTimings(1, timings);
            if (count == 0 && drainAttempts < MaxFrameTimingDrainAttempts)
            {
                metrics = default;
                return false;
            }

            pendingGeneration = 0;
            if (count == 0)
            {
                frameTimingReason =
                    "FrameTimingManager returned no completed timing after the bounded drain window.";
                metrics = new BattleBenchmarkCompletedFrameMetrics(
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    managedAllocation.Read(),
                    drawCalls.Read(),
                    BattleRenderingBenchmarkMemory.CaptureTotalAllocatedMemory(),
                    BattleRenderingBenchmarkMemory.CaptureGraphicsMemory(),
                    BattleRenderingBenchmarkMemory.CaptureTextureMemory());
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
                managedAllocation.Read(),
                drawCalls.Read(),
                BattleRenderingBenchmarkMemory.CaptureTotalAllocatedMemory(),
                BattleRenderingBenchmarkMemory.CaptureGraphicsMemory(),
                BattleRenderingBenchmarkMemory.CaptureTextureMemory());
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
                case BattleBenchmarkRecorderKind.TextureMemory:
                    return "Texture.totalTextureMemory (Texture2D/Cubemap scope)";
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
            requestedUnityFrame = 0;
            drainAttempts = 0;
            frameTimingReason = string.Empty;
        }

        public void Dispose()
        {
            Reset();
            managedAllocation.Dispose();
            drawCalls.Dispose();
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
                    recorder = ProfilerRecorder.StartNew(
                        category,
                        marker,
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

        internal BattleBenchmarkMetric Read()
        {
            if (!valid || recorder.Count <= 0)
            {
                if (valid)
                    Reason = "The profiler counter produced no completed-frame sample.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            return BattleBenchmarkMetric.FromValue(recorder.LastValue, unit);
        }

        public void Dispose()
        {
            recorder.Dispose();
            valid = false;
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
        int OwnedResourceCount { get; }
        long CachedOwnedResourceMemoryBytes { get; }
        long MeasureOwnedResourceMemoryBytes();
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
        public int OwnedResourceCount => resources.OwnedResourceCount + backend.AllocatedChunkCount;
        public long CachedOwnedResourceMemoryBytes => cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
            long bytes = resources.OwnedResourceMemoryBytes;
            for (int index = 0; index < backend.AllocatedChunkCount; index++)
            {
                Mesh mesh = backend.GetChunkMesh(index);
                if (mesh != null)
                    bytes += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            return bytes;
        }
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
        public int OwnedResourceCount => resources.OwnedResourceCount + 1 + renderers.Length * 3;
        public long CachedOwnedResourceMemoryBytes => cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
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
        private bool disposed;

        internal BattleBenchmarkResourceSet(string suffix)
        {
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
        internal int RenderTargetWidth => renderTexture.width;
        internal int RenderTargetHeight => renderTexture.height;
        internal Material Material => material;
        internal Sprite Sprite => sprite;
        internal int OwnedResourceCount => 6;
        internal long OwnedResourceMemoryBytes =>
            BattleRenderingBenchmarkEnvironment.RuntimeMemory(texture) +
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


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\Editor\BattleRenderingBenchmarkEditorTests.cs ---
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingBenchmarkEditorTests
    {
        [Test]
        public void RequestConfig_ParsesOnlyExplicitCapacityScenarios()
        {
            var request = new BattleRenderingBenchmarkRequest
            {
                backend = "centralonly",
                comparison = "ab",
                warmupFrames = 2,
                sampleFrames = 3,
                leakCheckFrames = 4,
                maxManagedGrowthBytes = 1024,
                maxGraphicsGrowthBytes = 2048,
                targetActiveEntities = " 300 ",
                outputPath = "Temp/test-benchmark.json",
            };

            BattleRenderingBenchmarkConfig config =
                BattleRenderingBenchmarkConfig.FromRequest(request);

            Assert.That(config.Backend, Is.EqualTo(BattlePresentationBackendMode.CentralOnly));
            Assert.That(config.Comparison, Is.EqualTo(BattleRenderingBenchmarkComparison.CentralLegacyAB));
            Assert.That(config.WarmupFrames, Is.EqualTo(2));
            Assert.That(config.SampleFrames, Is.EqualTo(3));
            Assert.That(config.LeakCheckFrames, Is.EqualTo(4));
            Assert.That(config.TargetActiveEntities, Is.EqualTo("300"));
            Assert.That(config.Scenario.RequestedEntityCount, Is.EqualTo(300));
            Assert.That(config.OutputPath, Is.EqualTo("Temp/test-benchmark.json"));
            Assert.Throws<System.ArgumentException>(() =>
                BattleRenderingBenchmarkScenario.Parse("24 actors"));
            Assert.Throws<System.ArgumentException>(() =>
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralShadowBuild,
                    0,
                    1,
                    "100",
                    string.Empty));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    0,
                    "100",
                    string.Empty));
        }

        [TestCase("100", 100, 200)]
        [TestCase("300", 300, 600)]
        [TestCase("500", 500, 1000)]
        [TestCase("1000", 1000, 2000)]
        public void DeterministicRuntimeWorkload_BuildsExactRequestedCount(
            string scenarioText,
            int expectedEntities,
            int expectedCommands)
        {
            BattleRenderingBenchmarkScenario scenario =
                BattleRenderingBenchmarkScenario.Parse(scenarioText);

            BattleRenderingBenchmarkWorkload first =
                BattleRenderingBenchmarkWorkload.Create(scenario, null, 1, 2);
            BattleRenderingBenchmarkWorkload second =
                BattleRenderingBenchmarkWorkload.Create(scenario, null, 1, 2);

            Assert.That(first.RequestedEntityCount, Is.EqualTo(expectedEntities));
            Assert.That(first.ActualEntityCount, Is.EqualTo(expectedEntities));
            Assert.That(first.FrozenFrame.EntityCount, Is.EqualTo(expectedEntities));
            Assert.That(first.CommandCount, Is.EqualTo(expectedCommands));
            Assert.That(first.RuntimeObjectCount, Is.EqualTo(expectedEntities));
            Assert.That(first.RuntimeProfile, Is.EqualTo(BattleRuntimeProfile.MobileExtended.ToString()));
            Assert.That(first.RuntimeSlotCapacity, Is.EqualTo(1050));
            Assert.That(first.WarmupTickCount, Is.EqualTo(1));
            Assert.That(first.WarmupLogicTickSamples.Count, Is.EqualTo(1));
            Assert.That(first.WarmupLogicTickSamples[0].ElapsedMilliseconds.Available, Is.True);
            Assert.That(first.WarmupLogicTickSamples[0].AllocatedBytes.Available, Is.True);
            Assert.That(first.WarmupLogicTickSamples[0].Checksum, Is.Not.Empty);
            Assert.That(first.SampleTickCount, Is.EqualTo(2));
            Assert.That(first.LogicTickSamples.Count, Is.EqualTo(2));
            Assert.That(first.LogicTickSamples[0].ElapsedMilliseconds.Available, Is.True);
            Assert.That(first.LogicTickSamples[0].AllocatedBytes.Available, Is.True);
            Assert.That(first.InputFingerprint, Is.Not.Empty);
            Assert.That(first.InitialRuntimeChecksum, Is.Not.Empty);
            Assert.That(first.FinalRuntimeChecksum, Is.Not.Empty);
            Assert.That(first.RuntimeStateDeterministic, Is.True);
            Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint));
            Assert.That(first.InputFingerprint, Is.EqualTo(second.InputFingerprint));
            Assert.That(first.InitialRuntimeChecksum, Is.EqualTo(second.InitialRuntimeChecksum));
            Assert.That(first.FinalRuntimeChecksum, Is.EqualTo(second.FinalRuntimeChecksum));
        }

        [Test]
        public void DeterministicRuntimeWorkload_AdmitsExactlyOneThousandRealEntities()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("1000"),
                    null,
                    0,
                    1);

            Assert.That(workload.RuntimeObjectCount, Is.EqualTo(1000));
            Assert.That(workload.ActualEntityCount, Is.EqualTo(1000));
            Assert.That(workload.RuntimeAdmissionValidated, Is.True);
            Assert.That(workload.RuntimeSlotCapacity, Is.EqualTo(1050));
        }

        [Test]
        public void CurrentSceneWorkload_RejectsMissingOrEmptyPublishedFrame()
        {
            BattleRenderingBenchmarkScenario scenario =
                BattleRenderingBenchmarkScenario.Parse("current-scene");

            Assert.Throws<System.InvalidOperationException>(() =>
                BattleRenderingBenchmarkWorkload.Create(scenario, new SimulationWorld()));
        }

        [Test]
        public void SessionReport_ValidatesNonEmptyCentralWorkload_AndDisposesRecorders()
        {
            var world = new SimulationWorld();
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                0,
                1,
                "100",
                string.Empty);
            var session = new BattleRenderingBenchmarkSession(config, world);
            try
            {
                int guard = 0;
                while (!session.CaptureFrame() && guard++ < 4)
                {
                }
                Assert.That(session.IsComplete, Is.True);
                string first = session.Report.ToJson();
                string second = session.Report.ToJson();
                Assert.That(second, Is.EqualTo(first));
                Assert.That(session.Report.RequestedPresentationEntityCount, Is.EqualTo(100));
                Assert.That(session.Report.ActualPresentationEntityCount, Is.EqualTo(100));
                Assert.That(session.Report.CommandCount, Is.EqualTo(200));
                Assert.That(session.Report.RendererWorkloadValidated, Is.True);
                Assert.That(session.Report.RuntimeAdmissionValidated, Is.True);
                Assert.That(session.Report.LogicTickMetricsValidated, Is.True);
                Assert.That(session.Report.DeterminismValidated, Is.True);
                Assert.That(session.Report.Passed, Is.False);
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Unsupported));
                Assert.That(session.Report.RuntimeObjectCount, Is.EqualTo(100));
                Assert.That(session.Report.RuntimeProfile, Is.EqualTo("MobileExtended"));
                Assert.That(session.Report.Frames[0].LogicTickTimeMs.Available, Is.True);
                Assert.That(session.Report.Frames[0].LogicTickAllocatedBytes.Available, Is.True);
                Assert.That(
                    session.Report.Frames[0].PresenterSubmissionDrawCalls.Available,
                    Is.False,
                    "EditMode builds mesh segments but never calls Graphics.DrawMesh");
                BattleBenchmarkMetricAvailability submissionAvailability =
                    FindMetricAvailability(session.Report, "presenterSubmissionDrawCalls");
                Assert.That(submissionAvailability.Available, Is.False);
                StringAssert.Contains("not in Play Mode", submissionAvailability.Reason);
                Assert.That(
                    submissionAvailability.Source,
                    Is.EqualTo("Graphics.DrawMesh calls issued by the central presenter"));
                Assert.That(session.Report.BenchmarkRenderTargetWidth, Is.EqualTo(256));
                Assert.That(session.Report.BenchmarkRenderTargetHeight, Is.EqualTo(256));
                StringAssert.Contains("ntsd-battle-rendering-benchmark-run-v4", first);
                StringAssert.Contains("\"verdict\":\"Unsupported\"", first);
                StringAssert.Contains("metricAvailability", first);
                StringAssert.Contains("missingRequiredMetrics", first);
                StringAssert.Contains("benchmarkRenderTargetWidth\":256", first);
            }
            finally
            {
                session.Dispose();
            }

            Assert.That(session.IsDisposed, Is.True);
        }

        [Test]
        public void SubmissionPolicy_ReportsOnlyActualPlayDrawMeshCalls()
        {
            Assert.That(
                BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(false, 3),
                Is.EqualTo(BattleRenderingBenchmarkSubmissionPolicy.Unavailable));
            Assert.That(
                BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(true, 3),
                Is.EqualTo(3));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(true, 0));
        }

        [Test]
        public void ABSuite_UsesOneFrozenWorkload_ForCentralAndRendererlessLegacy()
        {
            var world = new SimulationWorld();
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.CentralLegacyAB,
                0,
                1,
                0,
                1024,
                1024,
                "100",
                string.Empty);
            var suite = new BattleRenderingBenchmarkSuiteSession(config, world);
            try
            {
                int guard = 0;
                while (!suite.CaptureFrame() && guard++ < 4)
                {
                }

                Assert.That(suite.IsComplete, Is.True);
                Assert.That(suite.Report.Runs.Count, Is.EqualTo(2));
                Assert.That(
                    suite.Report.Runs[0].Config.Backend,
                    Is.EqualTo(BattlePresentationBackendMode.CentralOnly));
                Assert.That(
                    suite.Report.Runs[1].Config.Backend,
                    Is.EqualTo(BattlePresentationBackendMode.LegacyOnly));
                Assert.That(
                    suite.Report.Runs[0].WorkloadFingerprint,
                    Is.EqualTo(suite.Report.Runs[1].WorkloadFingerprint));
                Assert.That(
                    suite.Report.Runs[0].FinalRuntimeChecksum,
                    Is.EqualTo(suite.Report.Runs[1].FinalRuntimeChecksum));
                Assert.That(
                    suite.Report.Runs[0].InputFingerprint,
                    Is.EqualTo(suite.Report.Runs[1].InputFingerprint));
                Assert.That(suite.Report.Runs[0].RendererWorkloadValidated, Is.True);
                Assert.That(suite.Report.Runs[1].RendererWorkloadValidated, Is.True);
                Assert.That(suite.Report.Runs[1].CommandCount, Is.EqualTo(200));
                Assert.That(suite.Report.Passed, Is.False);
                Assert.That(suite.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Unsupported));
                Assert.That(
                    FindMetricAvailability(suite.Report.Runs[0], "presenterSubmissionDrawCalls").Required,
                    Is.True);
                Assert.That(
                    FindMetricAvailability(suite.Report.Runs[1], "presenterSubmissionDrawCalls").Applicability,
                    Is.EqualTo(BattleBenchmarkMetricApplicability.NotApplicable));
                Assert.That(
                    FindMetricAvailability(suite.Report.Runs[1], "drawCalls").Required,
                    Is.True);
                Assert.That(
                    suite.Report.Runs[0].BenchmarkRenderTargetWidth,
                    Is.EqualTo(suite.Report.Runs[1].BenchmarkRenderTargetWidth));
                Assert.That(
                    suite.Report.Runs[0].BenchmarkRenderTargetHeight,
                    Is.EqualTo(suite.Report.Runs[1].BenchmarkRenderTargetHeight));
                StringAssert.Contains(
                    "BenchmarkRendererlessLegacyCompatibilityPresenter",
                    suite.Report.ToJson());
            }
            finally
            {
                suite.Dispose();
            }
        }

        [Test]
        public void PassPolicy_RejectsMissingRequiredLocalMetrics()
        {
            Assert.That(
                BattleRenderingBenchmarkPassPolicy.Evaluate(
                    countValidated: true,
                    runtimeAdmissionValidated: true,
                    logicTickMetricsValidated: false,
                    determinismValidated: true,
                    rendererWorkloadValidated: true,
                    leakRequested: false,
                    leakPassed: false),
                Is.False);
            Assert.That(
                BattleRenderingBenchmarkPassPolicy.Evaluate(
                    countValidated: true,
                    runtimeAdmissionValidated: true,
                    logicTickMetricsValidated: true,
                    determinismValidated: true,
                    rendererWorkloadValidated: true,
                    leakRequested: false,
                    leakPassed: false),
                Is.True);
        }

        [Test]
        public void V4InjectedCompletedFrames_RequireExactCount_AndKeepDrawScopesDistinct()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    2);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                0,
                2,
                "100",
                string.Empty);
            var context = new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                false,
                true);
            var collector = new BattleBenchmarkInjectedCompletedFrameCollector(
                CreateAvailableCompletedFrame(drawCalls: 123));
            var presenter = new InjectedPresenter(workload, central: true, submissionDrawCalls: 7);
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                context,
                collector,
                presenter);
            try
            {
                int guard = 0;
                while (!session.CaptureFrame() && guard++ < 8)
                {
                }

                Assert.That(session.IsComplete, Is.True);
                Assert.That(session.Report.Frames.Count, Is.EqualTo(2));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
                Assert.That(session.Report.Frames[0].DrawCalls.Value, Is.EqualTo(123));
                Assert.That(session.Report.Frames[0].PresenterSubmissionDrawCalls.Value, Is.EqualTo(7));
                Assert.That(
                    FindMetricAvailability(session.Report, "exactSampleCount").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Passed));
                Assert.That(
                    FindMetricAvailability(session.Report, "renderThreadTimeMs").Applicability,
                    Is.EqualTo(BattleBenchmarkMetricApplicability.NotApplicable));
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void V4LegacyLocalDrawAndMeshMetricsAreNotApplicable_ButActualDrawsAreRequired()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.LegacyOnly,
                0,
                1,
                "100",
                string.Empty);
            var context = new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                true,
                true);
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                context,
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(drawCalls: 44)),
                new InjectedPresenter(workload, central: false, submissionDrawCalls: -1));
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.True);
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
                Assert.That(FindMetricAvailability(session.Report, "drawCalls").Required, Is.True);
                Assert.That(
                    FindMetricAvailability(session.Report, "presenterSubmissionDrawCalls").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.NotApplicable));
                Assert.That(
                    FindMetricAvailability(session.Report, "meshChunks").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.NotApplicable));
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void V4VerdictPolicy_MissingRequiredMetricCannotPass()
        {
            var context = new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                true,
                true);
            var metrics = new[]
            {
                new BattleBenchmarkMetricAvailability(
                    "drawCalls",
                    true,
                    BattleBenchmarkMetricApplicability.Applicable,
                    BattleBenchmarkMetricStatus.Missing,
                    "completed-frame",
                    0,
                    3,
                    "injected",
                    "missing completed-frame counter"),
            };

            BattleRenderingBenchmarkVerdict verdict =
                BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                    context,
                    metrics,
                    out string reason,
                    out string[] missing);

            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
            Assert.That(missing, Does.Contain("drawCalls"));
            StringAssert.Contains("drawCalls", reason);
        }

        [Test]
        public void PlayerArguments_RequireExplicitOptIn_AndParseRequest()
        {
            Assert.That(
                BattleRenderingBenchmarkPlayerArguments.TryParse(
                    new[] { "Game.exe", "-batchmode" },
                    out _,
                    out _),
                Is.False);

            bool parsed = BattleRenderingBenchmarkPlayerArguments.TryParse(
                new[]
                {
                    "Game.exe",
                    BattleRenderingBenchmarkPlayerArguments.EnableArgument,
                    BattleRenderingBenchmarkPlayerArguments.ScenarioArgument, "1000",
                    BattleRenderingBenchmarkPlayerArguments.ComparisonArgument, "ab",
                    BattleRenderingBenchmarkPlayerArguments.WarmupArgument, "2",
                    BattleRenderingBenchmarkPlayerArguments.SampleArgument, "3",
                    BattleRenderingBenchmarkPlayerArguments.LeakArgument, "4",
                    BattleRenderingBenchmarkPlayerArguments.OutputArgument, "Temp/player.json",
                },
                out BattleRenderingBenchmarkRequest request,
                out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(request.targetActiveEntities, Is.EqualTo("1000"));
            Assert.That(request.comparison, Is.EqualTo("ab"));
            Assert.That(request.warmupFrames, Is.EqualTo(2));
            Assert.That(request.sampleFrames, Is.EqualTo(3));
            Assert.That(request.leakCheckFrames, Is.EqualTo(4));
            Assert.That(request.outputPath, Is.EqualTo("Temp/player.json"));
        }

        [Test]
        public void LeakCheck_CollectsTransientGarbage_OutsidePerformanceSamples()
        {
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                1048576,
                1048576,
                "100",
                string.Empty);
            var session = new BattleRenderingBenchmarkSession(config, new SimulationWorld());
            try
            {
                Assert.That(session.CaptureFrame(), Is.False, "the first call requests a completed-frame sample");
                Assert.That(session.CaptureFrame(), Is.False, "the second call drains the sample and captures the leak baseline");
                WeakReference transient = AllocateTransientGarbageWithoutConservativeStackRoot(
                    16 * 1024 * 1024);
                int guard = 0;
                while (!session.CaptureFrame() && guard++ < 8)
                {
                }

                Assert.That(session.IsComplete, Is.True);
                Assert.That(transient.IsAlive, Is.False, "the soak endpoint full collection must reclaim transient garbage");
                Assert.That(session.Report.LeakReport.Available, Is.True);
                if (!session.Report.LeakReport.GraphicsAvailable)
                    Assert.That(session.Report.LeakReport.Passed, Is.False);
                Assert.That(session.Report.Passed, Is.False, "EditMode can never produce a v4 PASS verdict");
                Assert.That(
                    session.Report.LeakReport.MeasurementMode,
                    Is.EqualTo(BattleRenderingBenchmarkSession.RetainedManagedHeapMeasurementMode));
                StringAssert.Contains(
                    "full-gc-retained-managed-heap-outside-performance-sample-window-v1",
                    session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference AllocateTransientGarbage(int bytes)
        {
            var garbage = new byte[bytes];
            garbage[0] = 1;
            garbage[garbage.Length - 1] = 2;
            return new WeakReference(garbage);
        }

        private static WeakReference AllocateTransientGarbageWithoutConservativeStackRoot(int bytes)
        {
            WeakReference result = null;
            var thread = new Thread(() => result = AllocateTransientGarbage(bytes));
            thread.Start();
            thread.Join();
            return result;
        }

        private static BattleBenchmarkMetricAvailability FindMetricAvailability(
            BattleRenderingBenchmarkReport report,
            string metric)
        {
            for (int index = 0; index < report.MetricAvailability.Count; index++)
            {
                BattleBenchmarkMetricAvailability candidate = report.MetricAvailability[index];
                if (candidate.Metric == metric)
                    return candidate;
            }
            Assert.Fail("Missing metric availability entry: " + metric);
            return null;
        }

        private static BattleBenchmarkCompletedFrameMetrics CreateAvailableCompletedFrame(
            int drawCalls)
        {
            return new BattleBenchmarkCompletedFrameMetrics(
                BattleBenchmarkMetric.FromValue(16.0, "ms"),
                BattleBenchmarkMetric.FromValue(8.0, "ms"),
                BattleBenchmarkMetric.FromValue(2.0, "ms"),
                BattleBenchmarkMetric.FromValue(5.0, "ms"),
                BattleBenchmarkMetric.FromValue(0, "bytes"),
                BattleBenchmarkMetric.FromValue(drawCalls, "count"),
                BattleBenchmarkMetric.FromValue(1024, "bytes"),
                BattleBenchmarkMetric.FromValue(2048, "bytes"),
                BattleBenchmarkMetric.FromValue(512, "bytes"));
        }

        private sealed class InjectedPresenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;
            private readonly bool central;
            private readonly int submissionDrawCalls;
            private readonly BattleCentralBuildDiagnostics diagnostics;

            internal InjectedPresenter(
                BattleRenderingBenchmarkWorkload benchmarkWorkload,
                bool central,
                int submissionDrawCalls)
            {
                workload = benchmarkWorkload;
                this.central = central;
                this.submissionDrawCalls = submissionDrawCalls;
                if (central)
                    diagnostics = new BattleCentralBuildDiagnostics();
            }

            public string Implementation => "InjectedPresenter";
            public string EffectiveBackend => central ? "CentralOnly" : "LegacyOnly";
            public string ResourceMode => "Injected";
            public string DrawMode => "Injected";
            public int RenderTargetWidth => 256;
            public int RenderTargetHeight => 256;
            public int ResolvedCommandCount => workload.CommandCount;
            public int MaterializedRenderItemCount => workload.CommandCount;
            public int ResourceSegmentCount => 7;
            public int SubmissionDrawCount => submissionDrawCalls;
            public string SubmissionDrawMetricSource => "injected presenter-local submissions";
            public string SubmissionDrawUnavailableReason => "Legacy local submissions are not applicable.";
            public int OwnedResourceCount => 1;
            public long CachedOwnedResourceMemoryBytes => 256;
            public long MeasureOwnedResourceMemoryBytes() => 256;
            public BattleCentralBuildDiagnostics Diagnostics => diagnostics;
            public double Present() => 0.25;
            public void Dispose()
            {
            }
        }
    }
}
#endif


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Animation\Rendering\Editor\BattleRenderingBenchmarkWindow.cs ---
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using DiagnosticsProcess = System.Diagnostics.Process;
using DiagnosticsProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingBenchmarkWindow : EditorWindow
    {
        private string backend = nameof(BattlePresentationBackendMode.CentralOnly);
        private string comparison = "ab";
        private int warmupFrames = 30;
        private int sampleFrames = 120;
        private int leakCheckFrames = 600;
        private long maxManagedGrowthBytes = 1048576L;
        private long maxGraphicsGrowthBytes = 4194304L;
        private string targetActiveEntities = "current-scene";
        private string outputPath = "Temp/NTSD_BattleRenderingBenchmark.json";
        private string status = "Write a request while Play Mode has an active battle world.";

        [MenuItem("NTSD/Battle Rendering/Benchmark")]
        public static void Open()
        {
            GetWindow<BattleRenderingBenchmarkWindow>("Battle Rendering Benchmark");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Explicit Benchmark Request", EditorStyles.boldLabel);
            backend = EditorGUILayout.TextField("Backend", backend);
            comparison = EditorGUILayout.TextField("Comparison", comparison);
            warmupFrames = EditorGUILayout.IntField("Warmup Frames", warmupFrames);
            sampleFrames = EditorGUILayout.IntField("Sample Frames", sampleFrames);
            leakCheckFrames = EditorGUILayout.IntField("Leak Check Frames", leakCheckFrames);
            maxManagedGrowthBytes = EditorGUILayout.LongField("Max Managed Growth", maxManagedGrowthBytes);
            maxGraphicsGrowthBytes = EditorGUILayout.LongField("Max Graphics Growth", maxGraphicsGrowthBytes);
            targetActiveEntities = EditorGUILayout.TextField("Target Active Entities", targetActiveEntities);
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            EditorGUILayout.HelpBox(status, MessageType.Info);
            if (GUILayout.Button("Write Request File"))
            {
                try
                {
                    BattleRenderingBenchmarkRequest request = new BattleRenderingBenchmarkRequest
                    {
                        backend = backend,
                        comparison = comparison,
                        warmupFrames = warmupFrames,
                        sampleFrames = sampleFrames,
                        leakCheckFrames = leakCheckFrames,
                        maxManagedGrowthBytes = maxManagedGrowthBytes,
                        maxGraphicsGrowthBytes = maxGraphicsGrowthBytes,
                        targetActiveEntities = targetActiveEntities,
                        outputPath = outputPath,
                    };
                    BattleRenderingBenchmarkRequestProcessor.WriteRequest(request);
                    status = "Request written. The Editor processor will start it on the next update.";
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    Debug.LogError($"[BattleRenderingBenchmark] Request write failed: {ex}");
                }
            }
        }
    }

    [InitializeOnLoad]
    internal static class BattleRenderingBenchmarkRequestProcessor
    {
        internal const string RequestFile = "Temp/NTSD_BattleRenderingBenchmark.request.json";
        internal const string ResultFile = "Temp/NTSD_BattleRenderingBenchmark.result";
        internal const string DefaultOutputFile = "Temp/NTSD_BattleRenderingBenchmark.json";

        private static BattleRenderingBenchmarkRunner runner;
        private static bool requestInProgress;

        static BattleRenderingBenchmarkRequestProcessor()
        {
            EditorApplication.update += PollRequest;
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            EditorApplication.quitting += Cleanup;
        }

        internal static string ProjectPath(string path)
        {
            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
        }

        internal static void WriteRequest(BattleRenderingBenchmarkRequest request)
        {
            BattleRenderingBenchmarkConfig.FromRequest(request);
            string path = ProjectPath(RequestFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            File.WriteAllText(path, JsonUtility.ToJson(request), new UTF8Encoding(false));
        }

        private static void PollRequest()
        {
            if (requestInProgress || runner != null)
                return;

            string requestPath = ProjectPath(RequestFile);
            if (!File.Exists(requestPath))
                return;

            requestInProgress = true;
            try
            {
                ProcessRequest(requestPath);
            }
            finally
            {
                TryDelete(requestPath);
                requestInProgress = false;
            }
        }

        private static void ProcessRequest(string requestPath)
        {
            string resultPath = ProjectPath(ResultFile);
            try
            {
                BattleRenderingBenchmarkRequest request =
                    JsonUtility.FromJson<BattleRenderingBenchmarkRequest>(File.ReadAllText(requestPath, Encoding.UTF8));
                BattleRenderingBenchmarkConfig config = BattleRenderingBenchmarkConfig.FromRequest(request);
                SimulationWorld world = SimulationTickDriver.Instance?.World;
                if (world == null && config.Scenario.UsesCurrentScene)
                    throw new InvalidOperationException(
                        "No active SimulationWorld exists. Enter Play Mode and start a battle first.");
                if (world == null)
                    world = new SimulationWorld();

                string outputPath = string.IsNullOrWhiteSpace(config.OutputPath)
                    ? ProjectPath(DefaultOutputFile)
                    : ProjectPath(config.OutputPath);
                runner = BattleRenderingBenchmarkRunner.Start(
                    config,
                    world,
                    outputPath,
                    Complete);
                WriteResult(resultPath, "RUNNING\n" + outputPath);
                Debug.Log($"[BattleRenderingBenchmark] Started: {outputPath}");
            }
            catch (Exception ex)
            {
                if (runner != null)
                {
                    runner.Abort(ex.ToString());
                    runner = null;
                }
                WriteResult(resultPath, "FAIL\n" + ex);
                Debug.LogError($"[BattleRenderingBenchmark] Request failed: {ex}");
            }
        }

        private static void Complete(BattleRenderingBenchmarkRunner completedRunner, string result)
        {
            if (runner == completedRunner)
                runner = null;
            WriteResult(ProjectPath(ResultFile), result);
        }

        private static void WriteResult(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static void Cleanup()
        {
            if (runner == null)
                return;
            runner.Abort("Benchmark stopped during Editor shutdown or domain reload.");
            runner = null;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BattleRenderingBenchmark] Failed to delete request: {ex.Message}");
            }
        }
    }

    internal static class BattleRenderingBenchmarkPlayerBuild
    {
        internal const string PlayerDirectory = "Temp/P8-D-StandalonePlayer";
        internal const string PlayerExecutable = "NTSD-BattleRenderingBenchmark.exe";

        [MenuItem("NTSD/Battle Rendering/Build Standalone Benchmark Player")]
        internal static void BuildWindows64Player()
        {
            string executable = BuildWindows64PlayerInternal();
            UnityEngine.Debug.Log("[BattleRenderingBenchmark] Standalone Player built: " + executable);
        }

        [MenuItem("NTSD/Battle Rendering/Build And Run Standalone Benchmark Matrix")]
        internal static void BuildAndRunWindows64Matrix()
        {
            string executable = BuildWindows64PlayerInternal();
            string[] scenarios = { "100", "300", "500", "1000" };
            for (int index = 0; index < scenarios.Length; index++)
            {
                string scenario = scenarios[index];
                string output = BattleRenderingBenchmarkRequestProcessor.ProjectPath(
                    $"Temp/P8-D-player-{scenario}-ab.json");
                string log = BattleRenderingBenchmarkRequestProcessor.ProjectPath(
                    $"Temp/P8-D-player-{scenario}.log");
                var startInfo = new DiagnosticsProcessStartInfo
                {
                    FileName = executable,
                    Arguments = BuildArguments(scenario, output, log),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(executable) ??
                                       BattleRenderingBenchmarkRequestProcessor.ProjectPath(PlayerDirectory),
                };
                using DiagnosticsProcess process = DiagnosticsProcess.Start(startInfo);
                if (process == null)
                    throw new InvalidOperationException("Failed to start Standalone benchmark Player.");
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Standalone benchmark scenario {scenario} failed with exit code {process.ExitCode}. " +
                        $"See {log}.");
                }
            }
            UnityEngine.Debug.Log(
                "[BattleRenderingBenchmark] Standalone 100/300/500/1000 A/B matrix completed.");
        }

        private static string BuildWindows64PlayerInternal()
        {
            var scenes = new List<string>();
            EditorBuildSettingsScene[] configuredScenes = EditorBuildSettings.scenes;
            for (int index = 0; index < configuredScenes.Length; index++)
            {
                if (configuredScenes[index].enabled && !string.IsNullOrWhiteSpace(configuredScenes[index].path))
                    scenes.Add(configuredScenes[index].path);
            }
            if (scenes.Count == 0)
                throw new InvalidOperationException("No enabled EditorBuildSettings scene is available for the Player harness.");

            string directory = BattleRenderingBenchmarkRequestProcessor.ProjectPath(PlayerDirectory);
            Directory.CreateDirectory(directory);
            string executable = Path.Combine(directory, PlayerExecutable);
            bool previousFrameTimingStats = PlayerSettings.enableFrameTimingStats;
            try
            {
                PlayerSettings.enableFrameTimingStats = true;
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes.ToArray(),
                    locationPathName = executable,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development,
                });
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Standalone benchmark Player build failed: {report.summary.result}, " +
                        $"errors={report.summary.totalErrors}.");
                }
                return executable;
            }
            finally
            {
                PlayerSettings.enableFrameTimingStats = previousFrameTimingStats;
            }
        }

        private static string BuildArguments(string scenario, string output, string log)
        {
            return string.Join(" ", new[]
            {
                "-batchmode",
                "-logFile", Quote(log),
                BattleRenderingBenchmarkPlayerArguments.EnableArgument,
                BattleRenderingBenchmarkPlayerArguments.ScenarioArgument, scenario,
                BattleRenderingBenchmarkPlayerArguments.ComparisonArgument, "ab",
                BattleRenderingBenchmarkPlayerArguments.OutputArgument, Quote(output),
            });
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
        }
    }

}
#endif


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Docs\central-battle-render-system-plan.md ---
# 集中式战斗渲染系统方案

## 2026-07-23 P8 当前验收证据（取代下方相冲突的 P8-C/P8-D 历史快照）

本节是本文的当前 P8 状态。下方任何称 P8-C 仅有 synthetic evidence、称 P8-D 不执行真实 logic tick、或称本轮没有 Windows Standalone Player 实测的文字，均为保留的历史记录，不能覆盖本节。

- **P8-B 诊断契约：**实体诊断包含 `FrameId` 和显式 `AtlasPageIndex`；central binding 采用严格验证；报告记录第一个 unresolved/unsupported 状态；aggregate diagnostics 必须与同一 generation 和逻辑 tick 的 build/submission 一致。Runtime/Editor 的相关构建为 0 errors，focused/full checks 在本轮已有证据范围内通过。
- **P8-C 真实生产链：**`Temp/P8-C-Resume-Live/P8-C-report.json` 于 `2026-07-23 17:28:29` 为 **PASS**。它走正式 `LF2ObjectPointFactory.CreateObjectImmediate` / `FreeEntityLikeExe` 链，而非仅 isolated fixture：`availableBefore=7`、`totalCheckout=9`、`expandedAndPublished=2`、`availableAfter=9`、`uniqueRuntimeHandles=2`，cleanup PASS。生产角色 `Entity(33,0)`（type `0`、`AtlasPageTexture2D`）Legacy/Central 均为 `4971` 个 alpha pixels；生产武器 `Entity(100,0)`（type `4`、`AtlasPageTexture2D`）均为 `2090` 个 alpha pixels；两者 Legacy/Central maximum pixel diff 都是 `0`。该报告的 production scope 是 factory 初始化、pool expansion 和 `SimulationWorld` publication；它不声称已覆盖 skill-input opoint。
- **P8-D 最终 v3：**`Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v3.json` 及对应 `-player-ab-v3.json` 全部 **PASS**。每一档均以真实 `MobileExtended(1050)` primary + mirror `SimulationWorld`、精确 `100/300/500/1000` 个真实 `LF2Entity` fixtures、`FrameInputSet.Empty` 和完整 `NTSDBattleTickSystem` 运行；先 warmup `30` 个逻辑 tick，再采样 `120` 个 tick。A/B 使用同一由真实 runtime handles、generation 和 positions 冻结的 presentation 以及同一 logic workload，记录 deterministic checksum，并通过 `600`-frame leak gate。

| final v3 report | logic tick average ms | logic tick maximum ms | logic tick allocation average/maximum bytes |
|---|---:|---:|---:|
| `runtime-100-editor` | `8.3087375` | `12.0803` | `0 / 0` |
| `runtime-300-editor` | `24.3566941666667` | `33.9412` | `0 / 0` |
| `runtime-500-editor` | `42.7971166666667` | `57.0061` | `0 / 0` |
| `runtime-1000-editor` | `100.006675` | `126.7602` | `0 / 0` |
| `runtime-100-player` | `0.537154166666667` | `1.285` | `0 / 0` |
| `runtime-300-player` | `2.59706583333333` | `29.4842` | `0 / 0` |
| `runtime-500-player` | `1.56702166666667` | `2.752` | `0 / 0` |
| `runtime-1000-player` | `2.980925` | `6.0687` | `0 / 0` |

这些是各报告中 Central/Legacy 共用的真实 logic-tick 数值，并非 central 比 legacy 更快的声明。特别是 Editor `1000` 的平均约 `100 ms/tick`，**不满足 30 Hz**（每 tick 约 `33.33 ms`）；Windows Standalone `1000` final v3 的平均为约 `2.98 ms/tick`。报告内的 presentation、frame、memory 和 platform 指标仍必须按各 backend/run 阅读，不能从 A/B 的 PASS 推导全面性能收益。

- **最终顺序回归：**held geometry 失败不是 benchmark 全局状态泄漏，而是 parentless/root renderer 的 `_visualTransform == rootTransform`：写入正确世界位置后又把同一 Transform 的 local position 清零。`LF2ObjectRenderer` 现在只在 visual 为独立子节点时归零 local position；focused fixture 同时验证 `SetLogicObject` 不改变 runtime X/Y/Z、`FirstPresentationTick`、`CentralShadowBuild` 模式或 legacy suppression，并要求 legacy root position 等于 immutable central command。fresh `Assembly-CSharp.dll` 为 `2026-07-23 18:05:55`，晚于相关源码 `17:59:02`；1000 实体 Central/Legacy A/B 于 `18:10:49` PASS，退出 Play 后完整 `BattleRuntimeSelfCheck` 于 `18:13:03` PASS。最终 dotnet Runtime/Editor 构建分别为 `0 errors / 42 warnings`、`0 errors / 48 warnings`。本段不预写 Architect PASS；P8-E Android/Adreno/Mali 仍由用户负责并排除，T8 默认 `stage.dat` 部署继续排除。

## 2026-07-22 Rendererless Central Mount 收口（当前结论）

- **生产 prefab 与默认路径：**`EntityObject` 和 `Shadow` prefab 的对应节点已挂载 `BattleCentralPresentationMount`；生产 prefab 中原有的持久 `Entity`/`Shadow` `SpriteRenderer` 已移除。common shadow 改由 `BattleCommonShadowDescriptor` 提供描述符；`LF2Sprite` 保存 renderer-independent 的 `visible`、`pic` 与 offset。默认后端为 `CentralOnly`。
- **注册、销毁、失败和陈旧帧契约：**mount 使用 `[ExecuteAlways]`，以 `gameObject.scene.IsValid()` 为 gate；prefab asset 本身不注册。Prefab Stage preview 可以参与编辑态 lifecycle，但没有 runtime handle，因而不属于生产 battle/pool 验证。mount/renderer 在 `OnDestroy` 主动移除 owner binding，避免 pool expire 后销毁对象仍在静态字典保留 destroyed wrapper。冷启动资源或提交失败 fail closed；已成功发布后出现的后续失败保留 last-good frame，并标记为 stale。该行为不回写战斗 runtime。
- **完整自动、编译与 Console 验证：**fresh 链为 mount source `15:41:46` < Unity `Assembly-CSharp.dll` `15:43:40` < 完整 `BattleRuntimeSelfCheck` result **PASS** `15:44:50`。full self-check 已包含真实 `DestroyImmediate(root)` focused 断言，覆盖销毁后的 owner binding 清理。主代理最后一次 `dotnet build` 为 **0 errors / 18 existing warnings**；此前 42 warnings 来自不同的生成视图。最新清空 Console Play/Stop 为 **0 error / 0 warning**。第一轮截图工具自身的 RenderTarget errors 不作为项目错误或项目验证证据，不能据此轮截图写 Console 为零。
- **最新 Play Mode 定向验证：**`NTSD_Battle` 最新观测为 `objects=6`、requested/effective=`CentralOnly`、`frame`/`ownership`/`submission`/`submitted=true`、`draws=6`、`sim/display tick=339` 且 `stale=false`。3 个生产 `LF2ObjectRenderer`、6 个 mount/handle 均有效，并确认 `persistent SpriteRenderer=0`。
- **前一轮视觉证据：**此前 `objects=12`、6 个生产 renderer、12 个 mount/handle、`draws=12` 的 Play 观测仍作为前一轮证据保留；`Temp/central-rendererless-game-20260722.png` 显示角色、武器和阴影。它不代表上述最新运行的对象数量。
- **Prefab Stage 与 Scene View 边界：**当前打开的 `EntityObject` Prefab Stage 仍有一个旧 `SpriteRenderer` preview instance；其 `logic=null`，可参加编辑态 lifecycle，但没有 runtime handle，属于 Prefab Stage 内存态而非生产 battle/pool 对象，不计入上述生产验证。本轮未修改或关闭用户当前的 Prefab Stage。由于当前 Scene View 位于 Prefab Stage，本轮没有 fresh Scene View 截图；此前 Scene View 证据仅作为既有记录保留。
- **范围边界：**T8 默认 `stage.dat` 部署与 Android/真机验证继续排除，不能由本轮 Editor/Play Mode 证据推出结论。

## P8 — 中央渲染可信度、可观察性与验收体系（2026-07-22 执行计划）

**状态（2026-07-23 当前结论）：P8-A/B1/B2 已完成；P8-C 生产验收为 PASS；P8-D 最终 v3 已完成并在 Editor 和 Windows Standalone Player 的四档真实 logic-tick A/B 报告中全部 PASS；P8-E Android/Adreno/Mali 真机验证继续由用户负责并排除。** P8-D 使用真实 `MobileExtended(1050)` primary + mirror `SimulationWorld`、精确 `100/300/500/1000` 个真实 `LF2Entity` fixtures、`FrameInputSet.Empty` 和完整 `NTSDBattleTickSystem`；它记录同一真实 logic workload 下的 Central/Legacy A/B、deterministic checksum 和 600-frame leak gate，但不宣称 Central 比 Legacy 更快。Editor `1000` 平均约 `100 ms/tick`，不满足 30 Hz；Windows Standalone Player `1000` final v3 平均约 `2.98 ms/tick`。诊断只读取 immutable presentation 数据和中央后端结果，绝不回写战斗 runtime。编译通过、self-check 通过、Play Mode 定向验证、像素对照和性能测量是不同证据，不能互相替代；Editor 或 Windows Standalone 结果均不能推出 Android、Adreno 或 Mali 真机结论。

### P8-A 真实架构链与既有能力基线

中央表现链为：

`DAT frameId -> BMP/file grid -> BattleSpriteCatalog -> Texture2DArray 或 OrderedPages -> immutable PresentationSnapshot -> RenderCommand -> 稳定透明排序 -> resource segment -> 持久动态 quad Mesh -> URP Pass`。

| 阶段 | 输入与输出 | 所有权、生命周期与失败行为 | 战斗真值边界 |
|------|------------|--------------------------|--------------|
| DAT/BMP grid | DAT 的 file、row、col、frame/pic 映射为 source rect | Loading 阶段解析；缺声明、越界或 hole 必须显式失败，不能猜图 | 只解释表现资源，不修改逻辑帧或状态 |
| `BattleSpriteCatalog` | typed resource key 映射到 rect、UV、pivot、metrics 与 central binding | publication/lease/retirement 管理共享资源；缺 key fail closed | metrics 可供表现定位读取，但 catalog 不成为战斗状态 |
| Atlas binding | catalog entry 绑定 `Texture2DArray` slice，或确定性的 `OrderedPages` page | 能力、内存或格式不满足时记录 fallback/refusal reason；禁止静默错图 | 只决定 GPU 资源形态 |
| PresentationSnapshot | 每个逻辑 tick 固化实体身份、可见性、位置、颜色、翻转、排序等 value-only 状态 | double buffer/atomic publish；generation 与 stable id 共同防止池复用串帧 | 只读逻辑快照，不反向驱动 runtime |
| RenderCommand | snapshot 展开为 Shadow、Entity、Overlay、HitRecord 等命令 | unsupported 或 unresolved 命令 fail closed，并保留诊断边界 | 命令不能改变对象生命周期或命中结果 |
| 排序与 segment | 按稳定实体 rank 和子序保持透明顺序，再按相邻兼容资源分段 | `A/B/A` 保持三段，不为少 draw 而重排；common shadow 当前可保持独立 `SourceTexture2D`，因此与角色形成单独 segment/draw，这是正确资源边界，不自动等于 bug | 仅决定提交顺序 |
| 动态 Mesh | segment 写入持久 quad Mesh，按 chunk 上限切分 | 复用 buffer；越界、无效资源或构建失败不得提交半成品 | 只生成顶点/索引数据 |
| URP Pass | world/Scene View 合法 camera 消费已准备好的提交数据 | 只在既定 injection point 提交；失败不回写 runtime，冷启动 fail closed，已有成功帧可按契约保留 last-good 并标记 stale | 纯表现终点 |

基线审计确认已有能力必须复用：`BattleCentralBuildDiagnostics`、`BattleRenderingDiagnosticReport`、`BattlePresentationParityDiagnostics`、首个 unresolved command、segment/chunk/draw 统计、atlas effective mode 与 fallback/refusal reason，以及 Legacy probe 对 immutable frame 的对照。P8 不重复建设这些能力；现有缺口是“指定实体/命令为何未绘制”的原因码、按 runtime handle 查询的完整快照，以及正式的正确性和性能验收矩阵。

### P8-B Diagnostic V1（第一实施批）

1. 每帧汇总必须覆盖 snapshot entity count、source/resolved/unresolved command count、segment/chunk/submission draw count，以及 atlas requested/effective mode、page count、array slice 能力与 fallback/refusal reason。
2. 支持按 `RuntimeSlotHandle` 查询；只允许在 generation 匹配时从 slot 解析当前 handle。结果至少包含 stable id、初始 OID/current DAT OID、frame/effective pic、`EntityVisible`/`ShadowVisible`、typed resource key、binding mode、array slice/page、UV、pivot、position、flip、color、sort rank、command index、segment 与 chunk。
3. 使用无字符串分配的 enum/record 表达未绘制原因。V1 至少覆盖：`None`、`InvalidRuntimeHandle`、`GenerationMismatch`、`MissingSnapshotEntity`、`PresentationVisibilityFalse`、`CommandSuppressed`、`MissingCatalogKey`、`MissingTextureOrMaterial`、`InvalidCentralBinding`、`UnsupportedRenderState`、`UnresolvedResource`、`NotSubmitted`；最终命名可按相邻代码统一，但语义不得合并丢失。
4. 逻辑 tick 热路径不得构造字符串、JSON 或扫描完整 capacity。详细文本/JSON 只在诊断显式启用或单条查询时物化；常态仅维护已有构建过程自然产生的数值和索引。
5. focused self-check 覆盖成功的 Entity/Shadow 查询、无效 handle、generation mismatch、不可见、缺 catalog、无效 binding、unsupported、unresolved 与未提交等可构造失败类，并证明查询不改变 runtime、command 或 checksum。

**2026-07-23 P8-B1/B2 fresh 证据：**B1 在首次架构复核中发现陈旧 last-good plan 仍可能报告成功、submission 未冻结 backend mutation version，以及缺 key/无效 binding/unsupported 覆盖不全；随后已加入 `StalePlan`、`BackendMutationMismatch`、submission build identity 校验，并补齐对应 focused checks。最新 `dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` 为 **0 errors / 42 existing warnings**；UnityMCP 刷新当前 Editor 后，相关源码时间早于 `Assembly-CSharp.dll` `2026-07-23 01:40:53`，fresh full `BattleRuntimeSelfCheck` 于 `01:41:55` 返回 **PASS**。B2 新增 `NTSD/Battle Rendering/Central Entity Diagnostic`、确定性 JSON 导出和 request-file 入口；定向 EditMode test 为 **1 passed / 0 failed**。真实 `NTSD_Battle` Play 查询 slot 0 返回 `reason=None`、`submitted=true`、requested/effective pixel mode=`CentralOnly`、source/resolved=`6/6`、unresolved=`0`；未占用 slot 399 明确返回 `ArgumentOutOfRangeException`，不会生成伪成功报告。该证据证明诊断契约与查询工具，不等于 P8-C 全像素矩阵或 P8-D 性能结论；最终架构复核仍需单独记录。

### P8-C 正确性与稳定性验收矩阵

| 场景 | 自动证据 | Play/像素证据 |
|------|----------|---------------|
| pool reuse 1000 次、runtime slot generation 重用 | handle、stable id、资源与诊断不串代 | 抽样观察复用后画面 |
| 超过预热数量的动态 pool 扩容 | 新对象拥有独立 mount、handle、command 与资源 | 连续 opoint 后全部可见 |
| Texture2DArray slice/UV 与 OrderedPages fallback | slice/page、UV、rect、fallback reason 断言 | 代表性角色/武器像素对照 |
| `A/B/A` 资源交错 | segment 保持原序，不合并为 `A/A/B` | 重叠透明对象截图 |
| Shadow/Entity/Overlay/HitRecord 顺序 | command rank/sub-order 断言 | 重叠与遮挡像素对照 |
| Mesh chunk 边界 | 4095/4096/4097 等边界与索引完整性 | 高实体压力画面无缺失/伪影 |
| 缺资源 fail closed | reason code、零错误资源提交、last-good/stale 契约 | 资源故障夹具下不显示错图 |
| Legacy 对照 | Editor-only probe 与 immutable frame 字段相等 | 同一暂停帧的 Legacy/Central 像素差异报告 |

自动断言只能证明覆盖到的契约；Play/像素验证由目标场景证明可观察结果，二者都不能单独扩大为所有资产和设备已正确。

**2026-07-23 P8-C 当前证据（覆盖本节此前“Play/像素项待实施”的历史快照）：**`Temp/P8-C-EditModeTest/P8-C-report.json` 为 **PASS**，以 `256x256` 像素夹具覆盖 generation reuse 1000 次、超预热隔离扩容 33 个 mount、Texture2DArray/OrderedPages fallback、透明 `A/B/A`、Shadow/Entity/Overlay/HitRecord 遮挡、4095/4096/4097 chunk、缺资源 fail-closed 与 rendererless frozen-frame Legacy/Central 像素对照。`Temp/P8-C-LivePool/P8-C-report.json` 为 **PASS**，真实 Play pool 中 `availableBefore=4` 时获取 5 个对象，确认越过 available 的一次动态扩容以及 5 个唯一 mount owner。旧组合 EditMode job `f278668e3a2445139c6a1a5ceb8815be` 的 11/11 保留为历史；P2 回归后的 fresh job `e455b7f70043438a938faa23e82e53f3` 为 **12/12 passed（P8-C 2 + P8-D 10，0 failed / 0 skipped）**。fresh full `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-07-23 12:07:26` 为 **PASS**，freshness 为 P2 `BattleRenderingBenchmark.cs` `11:56:24` < Unity DLL `11:59:33` < result `12:07:26`。Console 过滤到的 2 条 error 均为自检刻意构造的 registration rollback / mismatched rest binding release 拒绝路径（`BattleRuntimeSelfCheck:7046` / `:1133`），无编译错误栈或 benchmark 异常。这些证据完成本矩阵定义的自动/像素/真实 pool 范围，但不扩大为全部生产资源、所有场景、所有设备或 Android 真机的结论。

### P8-D 性能验收矩阵

在同一机器、同一场景、同一逻辑输入和相同采样窗口下做 Legacy/Central A/B，对 `100/300/500/1000` active entities 记录 CPU frame/tick cost、GPU frame cost、GC Alloc、draw calls、resource segments、Texture/Graphics memory 与长时间运行后的资源/内存泄漏。报告必须同时记录 atlas/draw effective mode、分辨率、Editor/Player、warm-up 与采样帧数。

**2026-07-23 P8-D 已被最终 v3 取代的 presentation-only 历史证据：**`Temp/P8-D-presentation-100-ab-rerun.json`、`Temp/P8-D-presentation-300-ab.json`、`Temp/P8-D-presentation-500-ab.json` 与 `Temp/P8-D-presentation-1000-ab.json` 均为 **PASS**。它们只覆盖 CentralOnly/benchmark-only Legacy compatibility 的冻结 presentation workload，不执行 `SimulationWorld` logic tick，也不注册真实战斗实体；该边界仅适用于这些历史报告，不能再作为 P8-D 当前状态或 Windows Standalone 覆盖缺失的依据。P8-D 当前结论以本文顶部的最终 v3 reports 为准：真实 `MobileExtended(1050)` primary + mirror `SimulationWorld`、精确 `100/300/500/1000` 个真实 `LF2Entity` fixtures 和完整 `NTSDBattleTickSystem` 的 Editor 与 Windows Standalone Player 报告均为 **PASS**。这些 A/B 结果不宣称 Central 比 Legacy 更快；Editor `1000` 平均约 `100 ms/tick`，不满足 30 Hz，而 Windows Standalone Player `1000` final v3 平均约 `2.98 ms/tick`。

**额外 current-scene production 覆盖（历史补充）：**`Temp/P8-D-current-scene-ab-v2.json` 为 **PASS**。真实 `NTSD_Battle` Play 在退出前的 `SimulationWorld` 为 `ObjectCount=12`、`tick=3847`；冻结并发布的表现帧为 `EntityCount=6`、`CommandCount=12`。CentralOnly 与 benchmark-only Legacy compatibility 均实际请求/物化 `6` 个 presentation entities、`12` commands，使用同一 fingerprint `f3aaf429518f46ec` 与同一 `256x256` target；600-frame retained 检查中 Central/Legacy managed heap 分别为 `+28672 B`/`+49152 B`，graphics `+0`、owned bytes `+0`、resource count 不变。该生产覆盖只是最终 v3 前的当前场景 presentation 样本；其“不测 logic tick/真实 active capacity/Standalone Player”的历史边界已经被最终 v3 的真实 logic-tick Editor 和 Windows Standalone Player 验收取代。

### P8-E 执行顺序与状态

| 批次 | 内容 | 当前状态 |
|------|------|----------|
| P8-A | 文档化真实链路并审计已有 diagnostics/parity 能力 | **已完成基线审计** |
| P8-B1 | reason enum、allocation-safe records、runtime-handle/slot 查询、focused self-check | **已完成；fresh full self-check PASS** |
| P8-B2 | Editor 可视查询/导出与按实体定位工具 | **已完成；EditMode、真实 Play slot 查询与确定性 JSON 导出通过** |
| P8-C | 自动正确性/稳定性矩阵，以及 Play/像素证据入口 | **已完成；生产验收 PASS：真实 factory 初始化、pool expansion 与 `SimulationWorld` publication 的角色/武器 Legacy/Central 像素对照均为 maximum diff `0`。不扩大为 skill-input opoint、全部资源或所有设备的结论** |
| P8-D | Editor/Windows Standalone Player real-runtime benchmark harness 与结构化报告 | **最终 v3 已完成：真实 `MobileExtended(1050)` primary + mirror `SimulationWorld`，精确 `100/300/500/1000` 个真实 `LF2Entity` fixtures，完整 `NTSDBattleTickSystem`，Editor 与 Windows Standalone Player 报告全部 PASS；不宣称 Central 比 Legacy 更快。Editor 1000 约 `100 ms/tick`，不满足 30 Hz；Player 1000 约 `2.98 ms/tick`** |
| P8-E | Android、Adreno/Mali 外部设备验证 | **用户负责 / 本轮排除** |

本计划不修改 DAT、战斗逻辑、1.5 倍实体缩放或既有渲染行为。T8 默认 `stage.dat` 部署继续取消/排除；Android 真机验收由用户处理。P8-C/D 只能在各自自动、像素或性能证据完成后更新状态，不能由 P8-B1/B2 的诊断证据提前替代。

## 2026-07-22 Editor Scene View Preview Validation

- **Scope and guardrails:** `CentralOnly` submits the same immutable mesh from the base Scene View camera only under `#if UNITY_EDITOR` and `Application.isPlaying`. Only the exact world camera may update renderer readiness. Scene View preview does not alter combat state, Player builds, or the Game camera.
- **Freshness and automated evidence:** Unity `Assembly-CSharp.dll` timestamp `23:47:47` is newer than the relevant source timestamp `23:30`; the direct `BattleRuntimeSelfCheck` result is **PASS**.
- **Observed Play/Scene View evidence:** Play state reported `objects=12` and the central mesh reported `quads=12`. `Temp/screenshot-20260722-000938.png` shows all current entities in the Scene View. The screenshot round's tool-originated RenderTarget errors are not project evidence; this screenshot does not establish a Console-zero result.
- **Validation boundary:** This verifies the current observed Scene View preview state only. It does not establish coverage for all resource scenes. T8 default `stage.dat` deployment and Android/device validation are not part of this task.

## 2026-07-21 Fresh Final Validation（当前结论，覆盖下方旧快照）

- **CentralOnly 已可实际接管像素**：P7 的 Overlay、Shadow、Entity 与 HitRecord ownership 已全部接通。运行时诊断为 `requested/effective=CentralOnly`，且 `frame`、`ownership`、`ready`、`submitted` 均为 `true`，`draws=12`。此前“`CentralOnly` unavailable / 继续拒绝”“Overlay blocker”“P7 未完成”的表述是本次验收前的历史快照，不再代表当前状态。
- **UV 伪影已定位并修复**：根因不是战斗逻辑、Atlas rect 或翻转规则，而是 `BattleDynamicMeshBackend.ClearActive` 将 `subMeshCount` 置为 `0`；Unity 2022.3 会因此释放 native index buffer，随后重建出现错误索引，表现为黑块和三角形 UV 伪影。当前保留一个零索引的 inert submesh，避免释放该 buffer。
- **实际像素对照**：暂停同一帧的 Legacy/Central 截图均为 `1920x1080`，逐像素结果 `changed=0`。截图可直接证明画面中角色、武器/球体与 Shadow 的 CentralOnly/Legacy 视觉一致性；Overlay/HitRecord 的 ownership 与资源路径由 self-check 和运行时 diagnostics 证明，不能把它们写成该截图必然可见的内容。两类证据均不扩大为所有资产和所有设备的结论。
- **最新可执行验证**：`Temp/NTSD_BattleRuntimeSelfCheck.result` 为 **PASS**；Unity Console 为 **0 error / 0 warning**。真实 Play 中显式启用 `LooseQuadtree` 后观测到 `backend=LooseQuadtree`、`objects=12`、`tick=1436`，同样为 **0 error / 0 warning**。B2C 已有 Architect final **PASS / no P0-P2**，不再处于“无 Architect 复核”的状态。
- **Editor Profiler 基线（非真机）**：Legacy 为 `6.1884 ms CPU / 0.346112 ms GPU / 18 draws`；Central 为 `6.5114 ms CPU / 0.70656 ms GPU / 20 draws`。Central 当前内存为 `1391.17 MB allocated / 1005.19 MB graphics`。这些数值是当前 Editor 样本的观测，不代表性能优于 Legacy，也不是移动端预算结论。
- **仍未关闭的外部验收边界**：尚无真实 Adreno/Mali 设备和 Android Player 的像素、兼容性、内存与性能证据，因此不能给出真机结论。T8 默认 `stage.dat` 资产部署按用户要求继续暂缓，且不作为本计划的未完成代码项。

> **历史快照说明**：下方直到下一次明确更新前出现的 `CentralOnly` 被拒绝、Overlay 缺失/阻塞、P7 未完成、B2C 无 Architect final、Play/pixel/Profiler 未验收等措辞，均记录其当时阶段，已由本节的 fresh final validation 取代，不能用于当前状态判断。

## 2026-07-21 P7 Batch6 per-entity Overlay 当前收口（覆盖本文件此前 Overlay blocker 快照）

- **代码、编译、自检和架构复核状态**：P7 Batch6 已完成 per-entity Overlay 的代码侧收口；最新相关源码 `2026-07-21 16:01:49` < Unity `Assembly-CSharp.dll` `16:03:35` < 完整 `BattleRuntimeSelfCheck` result `16:04:54` **PASS**。Unity Console 为 **0 C# error**；最后一次主代理 `dotnet build` 为 **0 errors / 18 existing warnings**；Architect final 为 **PASS / no P0-P2**。`git diff --check` 仍由主任务最终统一执行。
- **权威与资源边界**：`WORDS0.bmp` 至 `WORDS5.bmp` 已加入 Unity Assets，且其 SHA256 与权威 C# host 所引用的运行时依赖来源一致。这只是资源依赖与字形表的核验；战斗逻辑权威仍唯一为 `J:\QQFile\NTSD2.4\ntsd_release_C#`，不引入第三逻辑权威。
- **Catalog / prewarm**：`BattleSpriteCatalog.CommonWordGlyph(sheet, charCode)` 为 typed key，覆盖 `6 * 256` glyph；权威 top-left source rect 在 catalog 中转换为 Unity bottom-left rect。`CharacterAnimtorManager` 对 WORDS 采用 exact-black transparency、Point filter、Clamp wrap、事务式/atomic publication，并把 1536 个 glyph 的 Sprite 纳入 retirement ownership。
- **运行时与布局契约**：`BattleSlotLabelRuntimeState` 保存 `char[10,12]` 标签及 `int[10]` 状态，reset 与 `MatchConfig` bootstrap 均已接线。无分配的 `BattleEntityOverlayLayout` 覆盖 `Hp2Orig > 1` 复活 counter、普通标签、`[label]`、普通 `Com` 与特殊 `WORDS5` `Com`；标签位置 clamp，counter 不 clamp，容量不足 fail-closed。
- **snapshot / command / legacy**：snapshot 同时保留原始 `ObjectId`（供 shadow 223/224 gate）和 current DAT identity（供 Overlay），命令顺序为 `Shadow -> Entity -> OverlayGlyph -> HitRecord`。`BattleEntityOverlayRenderer` 在 legacy 路径使用 pooled `SpriteRenderer` materialize，并核验 generation/stable-id；默认 `LegacyOnly` 仍发布 immutable frame 但不构建 central mesh。`CentralShadowBuild` 保留诊断职责，`CentralOnly` 仍由 `ValidateAvailable` 显式拒绝。
- **生命周期与检查**：frame-level catalog lease 保护发布资源，HitRecord cycle lease 由 finalizer 释放，empty frame 不 retain；self-check 覆盖 retirement 窗口。布局检查还覆盖 HP2、slot/bracket/empty/Com、palette、特殊 OID/type/hitstop、clamp、fail-closed、命令序列与 zero-GC。
- **未验收边界**：本批不等于 P7 全门槛完成。Play Mode、像素基线、Profiler、Adreno/Mali 和真机均未验收；T8 默认 `stage.dat` 部署继续按用户要求排除。下文所有“Overlay 未实现”“WORDS 缺失”“confirmed blocker”或“Overlay 阻塞 CentralOnly/P7”的陈述均为 Batch6 之前的历史快照，已由本节覆盖，除非明确标为历史。

## 2026-07-21 B2C Extended checksum、当前 world 查询、P1-P6 与 P7 Batch1-5 状态

- **代码已实施 / fresh self-check 已通过 / 最终架构复审待补**：`Authority400` 继续冻结为 `ntsd-battle-trace-v3`，direct parity capture 仍严格拒绝非 `Authority400/400`；`MobileExtended` / `DesktopExtended` 通过通用 checksum API 生成独立 `ntsd-unity-extended-battle-checksum-v1`，旧 `LastFrameSnapshot` 仍只表示 Authority v3。
- Extended metadata 覆盖 profile、logical capacity、claimed/object count 与 tick；slot 域覆盖 slot、claimed、generation、stable ID、current DAT OID、active entity runtime 及已物化但未 claimed 的 raw runtime。读取未物化槽不会创建分页。
- ARest/VRest 使用按 victim/attacker 稳定排序的稀疏投影，不构造 `capacity²` 矩阵；claimed entity 若未绑定当前 world 的 rest store 或 victim slot 不一致，capture 会拒绝生成 checksum。
- focused self-check 覆盖 Extended 的 Mobile `1050` / slot `1049`、Desktop `512 -> 768` / slot `700`、高槽 ARest/VRest、raw runtime、generation/stable-ID reuse、profile separation、稀疏 VRest 与 non-mutating repeat capture；同时覆盖 AI Loose Quadtree 查询与即时 weapon/body current-world 查询的结果/回退契约。最新 full self-check `2026-07-21 00:48:06` **PASS**；`dotnet build` **0 errors / 42 existing warnings**。
- 即时 body/weapon 查询已在显式 `LooseQuadtree` 后端下使用当前 world 实体的空间查询，AI 输入快照已使用 generation-aware Loose Quadtree 查询；索引/几何/映射异常均回退 brute，生产默认仍为 `BruteForce`。
- **P1 排序止血已完成代码层收口**：活跃实体按 `(ZInt, runtime slot)` 排序后分配 dense presentation rank；四个短期子序为 `Shadow=0`、`Entity=1`、`Overlay=2` 和 `HitRecord=3`。权威 host 确实在 Entity 后绘制 per-entity Overlay；Unity 保留了子序但尚未实现对应消费者，这是 confirmed blocker。Shadow、Entity、spark 及其 `SortingGroup` 均统一为 Unity `Object` sorting layer，因此排序层不会先于 compact order 打断实体间交错。旧的 `logicalZ * 4096 + runtimeSlot * 4` 映射已移除。
- **P1 容量边界**：旧 `SpriteRenderer` 后端明确 guard 为最多 `8192` 个 materialized active entities；`8193` 会清晰抛错。移动端 `1000` active 预算在此范围内；`DesktopExtended` 在中央渲染后端完成前仍有这个临时表现上限，不等同于 runtime slot 容量上限。
- **P1 自动验证**：真实双实体四 renderer 的 `ForceRefresh` 检查验证 `Shadow(A)=0`、`Entity(A)=1`、`Shadow(B)=4`、`Entity(B)=5`，并覆盖 generation/高 slot 与 sorting layer/order。fresh 链为 source `2026-07-21 03:00:45` < Unity DLL `03:05:59` < full `BattleRuntimeSelfCheck` `03:07:05` **PASS**；`dotnet build Assembly-CSharp.csproj --no-restore -v:q` 为 **0 errors / 42 existing warnings**；最终 architect review 为 **PASS / no blocker**。
- **P2 immutable Catalog 已完成代码层收口**：`BattleSpriteCatalog` 的唯一 key 为 `(LF2Entity.ResolveCurrentDataObjectId(entity), effectivePic)`；不可变 entry 保存 source sheet、共享 `Texture2D`、Unity bottom-left 像素 rect、归一化 UV、宽高 metrics、pivot 和兼容旧 `SpriteRenderer` 的 legacy `Sprite`。正式 prewarm 使用 invocation-local staging 与 generation/disposed gate，只有本轮所有 sheet 成功且仍为当前 generation 时，才将 configs、`MergedSprites` 与 catalog 原子 publish；失败、过期结果和 teardown 均清理本轮资源。
- **P2 图片索引与生命周期契约**：partial BMP 严格按声明的 row/col 和 `localPic` 建立稀疏 rect，保留未声明图片的 holes；normal/swapped 网格仅在完整匹配时择优，并已覆盖 weapon6、weapon3 等生产矩阵。renderer 对 catalog 持有引用计数屏障，旧 catalog 只有在零引用后才退役，避免异步替换期间释放仍在显示的共享 texture/sprite。
- **P2 生产消费者已迁移**：display、collision、anchor、SpecialAttack point-center 与 shadow metrics 在战斗期不再读取 `Sprite.rect`；`pic=999`、缺 key、current DAT identity 切换和 pool reuse 均会隐藏并清除旧 sprite/catalog 引用。`MergedSprites` 仅保留兼容和预览用途，不再定义战斗期 metrics 真值。
- **P2 自动验证**：focused/full self-check 覆盖双文件边界、normal/swapped row/column、partial holes、rect/UV/pivot/shared texture、current identity replacement、missing/`999`、pool reuse、原子 publish、stale/teardown cleanup、renderer refcount retirement 及全部 metrics 消费者。fresh 链为 source `2026-07-21 04:16:00` < Unity DLL `04:17:06` < full `BattleRuntimeSelfCheck` `04:18:04` **PASS**；fresh dotnet build 为 **0 errors**。不同的自动生成 `.csproj` 刷新视图分别显示 18 或 42 条既有 warnings，因此不把 warning 数量冻结为 P2 契约。最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。
- **P3 shadow-build 已完成代码层收口**：渲染模式明确为默认 `LegacyOnly` 与诊断用 `CentralShadowBuild`；`CentralOnly` 明确拒绝。每个逻辑 tick 生成 value-only immutable snapshot/commands，按 `(ZInt, runtime slot)` 为每个实体稳定展开 `Shadow -> Entity -> Overlay -> HitRecord`。早期 `AuthorityExpectedButLegacyMissing` 标记来自不完整权威盘点，现已废止；权威两个 host 实际都绘制 per-entity Overlay，Unity 尚未实现，因此不能宣称 overlay 等价。
- **P3 发布与真实 legacy probe**：snapshot/commands 使用 double buffer、几何增长容量和 atomic publish；persistent scratch 保证 steady `RenderDispatch` self-check 为 zero allocation。legacy probe 直接采样真实 renderer 的 sprite、texture、material instance、rect、pivot、position、flip 与 sorting；HitRecord 在 legacy advance 前采样，避免把推进后的 spark 状态错配到当前 tick。
- **P3 catch-up 与 spark 契约**：同一渲染帧追赶多个逻辑 tick 时，无法对中间 tick 取得实际 legacy renderer 状态，因此显式发布 `Incomplete`，记录 incomplete count、first tick 与 last tick；仅最后可观测 tick 进入完整 probe，不宣称所有逻辑 tick 均已实际 legacy parity verified。zero-hit 仍通过 `SparkRenderer.RenderAll` finalize；正式 production pool 路径覆盖 nonzero spark atlas cells、每 tick 只 age 一次，以及 `OnDisable`/`OnDestroy` 归还池。
- **P3 隔离与验证**：P3 snapshot/command/diagnostic 不进入战斗 checksum，也不反写 runtime 真值。fresh 验证链为 source `2026-07-21 05:38:38` < Unity DLL `05:39:29` < full `BattleRuntimeSelfCheck` `05:40:16` **PASS**；dotnet build **0 errors / 18 existing warnings**（root 当前视图）；最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。
- **P4 Mesh/URP 代码层已完成**：中央后端复用持久 Mesh，并以每 chunk `4096` quad、`16384` 顶点、`24576` 索引的 `UInt16` 契约切分。`OrderedChunks` 只合并原命令流中相邻且状态兼容的命令，保持 `A,A,B,A` 原顺序；`StrictOrderedDraw` 提供逐命令正确性回退。跨 chunk 顺序、unresolved command barrier 与 stale mesh/submesh clear 均已进入 self-check。
- **P4 提交边界与 URP 接线**：`LegacyOnly` 不构建中央 Mesh，`CentralShadowBuild` 只构建诊断数据而不提交 draw，`CentralOnly` 在所有类别 ownership 完成前仍明确拒绝。URP pass 只接受 world camera 的 `Base` camera，并注入 `AfterRenderingTransparents`。`BattleRenderFeature` 已作为 active renderer asset 的唯一 subasset 安装并经安装器验证，不依赖场景临时对象。
- **P4 registration 修复**：初审发现 feature B 覆盖 feature A 后，注销 B 不会恢复 A 的配置。现改为可复用 registration stack，并由 `A -> B -> unregister B -> restore A` 自检验证 fallback material、array material 与 draw mode 全部恢复。
- **P4 自动验证**：fresh 链为 source `2026-07-21 06:32:00.287` < Unity DLL `06:32:56.970` < full `BattleRuntimeSelfCheck` result `06:33:43.796` **PASS**；dotnet build **0 errors / 42 existing warnings**；最终 architect review **PASS / no P0-P2 findings**。
- **P5 Atlas Array/fallback 代码层已完成**：确定性 planner 将 whole-sheet 资源放入 `2048 x 2048` 多页布局，使用 normalized path ordinal 去重；同路径同尺寸但像素内容冲突会拒绝，不允许加载顺序决定结果。每张 sheet 周边做 `1px` extrusion。能力 gate 满足时构建 `RGBA32 Texture2DArray`，否则按相同 page 顺序使用有序 Texture2D fallback。
- **P5 Catalog 与所有权**：catalog entry 保留 P2 legacy source，同时增加 immutable central binding。manager 使用事务式 publish；所有新建 Unity `Object` 在构造起点即进入 ownership，只有完整成功后才发布。legacy renderer lease 与 central consumer lease 都会延迟旧 atlas/catalog 退役，避免异步换代时释放仍被使用的 texture/material。
- **P5 绘制契约**：array 路径把 slice 写入 per-vertex 数据，相邻但跨 slice 的命令可在相同 array material 下保持原序合批；2D fallback 的 `A/B/A` 必须保持三段，不能为减少 draw call 重排。array/fallback 各有 shader、material 与 pass 配置，installer 同时验证两条资源链。
- **P5 复核修复**：首轮复核关闭两个 P2。其一，同 normalized path、同尺寸、不同 pixels 的输入现在对两种排列都拒绝，只有 equal-content duplicate 成功。其二，2D fallback 页在构造时即 owned；显式两页夹具中 page0 成功、page1 失败后两页均销毁，且没有 partial publication，关闭异常页 ownership 泄漏。
- **P5 自动验证**：fresh 链为 source `2026-07-21 07:06:28` < Unity DLL `07:07:12` < full `BattleRuntimeSelfCheck` log `07:08:13` **PASS**；dotnet build **0 errors / 42 existing warnings**；architect final **PASS / no P0-P2 findings**，code review **no P0-P2 findings**。
- **P6 设备策略代码侧已完成**：`BattleRenderingDevicePolicy` 以 immutable capabilities 表示设备边界，只有 `FromSystem` 接触 `SystemInfo`。策略解析严格遵循 CLI > `GameConfig` > Auto，命令行为 `-ntsdBattleAtlasMode` 与 `-ntsdBattleDrawMode`；非法显式值拒绝，不静默改写。Atlas 在 `TextureArray` 与 `OrderedPages` 间安全 fallback 并记录原因；draw mode 支持 Auto、`OrderedChunks`、`StrictOrderedDraw`，`SingleMesh` 不进入生产选择。
- **P6 发布与诊断契约**：resolver 生成显式、确定性的 JSON report，包含 capability、请求、effective mode 与 fallback reason。manager 每次 publication 只解析一次，central backend 缓存 effective draw mode；逻辑 tick 热路径不再查询 `SystemInfo` 或 CLI。该策略不改变 runtime profile、capacity、tick、collision、checksum 或 `CentralOnly` guard。
- **P7 held-object 子批已完成**：权威调用链按 `InteractionRuntimePasses -> WeaponPointRuntime/WeaponRuntime -> SdlBattleRenderer/BattleHostForm` 对照。legacy 与 presentation snapshot 共用纯 held-offset helper；offset 在 capture 时固化为 immutable 值，并追加到 Entity command，不从后续 renderer 状态回读。
- **P7-held 覆盖**：right/left facing、target mismatch、release、missing holder/wpoints、slot generation reuse、dormant holder，以及 legacy/central equality 均进入 self-check。旧 handle 或 inactive/dormant 不会把新 occupant 或过期 held offset 带入当前 command。
- **统一验证**：latest fresh 链为 self-check source UTC `23:42:44` < Unity DLL `23:44:03` < `Unity-P6-P7-Final2-SelfCheck.log` `23:45:00` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS**，code review **approve / no P0-P2 findings**。
- **P7 Batch2 render-state semantic parity 已完成**：snapshot/command 以 value-only `Color32`、`flipX/flipY`、mask/material semantic 和 logical resource key 表达状态，Unity instance ID 仅保留为诊断。catalog 增加 immutable `Sprite -> key[]` 反查和 preferred entity key；legacy probe/Compare 检测 RGB、alpha、flipY、unsupported state 与 logical key。
- **P7 Batch2 central/mesh 契约**：central resolver 转发 color 并对未知语义 fail closed。Mesh 将 color 写入 quad 四个顶点，flipY 通过 V 坐标交换实现；仅 color 不切 segment，material semantic variant 必须断段。pool checkout 将 entity/shadow/spark 规范化为 white、`flipX/flipY=false`、mask none，并在首次干净 checkout 借用 `Sprites/Default.sharedMaterial`，禁止触发 `.material` 实例化。
- **P7 Batch2 alpha contract**：依据 Unity `2022.3.4f1` 官方 builtin shaders ZIP changeset `35713cd46cd7`，两个中央 shader 改为 `Blend One OneMinusSrcAlpha`，最终输出执行 `rgb *= a`，并声明 `NTSDAlphaContract` tag；installer 已验证 shader 为 white 基线且 tag 正确。
- **Batch2 验证**：fresh 链为 source `08:27:50` < Unity DLL `08:28:48` < self-check log `08:29:48` **PASS**；installer validation **PASS**；dotnet build **0 errors**；architect/code review **PASS / no P0-P2 findings**。
- **P7 Batch3 Shadow 已完成**：按 authority `BattleHostForm` / `SdlBattleRenderer.DrawShadow` gates 对齐。资源侧使用 typed `EntitySprite` / `CommonShadow` key；`GameConfig.ShadowPrefab` 作为 immutable borrowed binding，固化真实 sprite、texture、UV、size、pivot、color 与 material。manager 在 main thread 做 atomic common publication，borrowed Unity Object 不进入 atlas/catalog owned retirement。
- **Batch3 snapshot/resolve**：snapshot 保存 actual ObjectId 与 `HasCurrentFrame`；Shadow command 携带真实 descriptor 和 `CommonShadow` key，并保证 Shadow 在 Entity 前。legacy probe 校验 exact sprite。central resolver 校验 sprite、texture、rect、pivot 与 material ID，同时提供 source2D + fallback material；任何 missing config/resource 都 fail closed。
- **Batch3 行为矩阵**：actual OID `223/224`、state `3005/9997`、`Link < 0`、HitStop、missing frame 均与 legacy 对齐。review 关闭 P1 missing-frame 的 legacy/central 差异，以及 P2 material ID、真实 `GameConfig` asset、real commit -> replace retirement tests。
- **Batch3 验证与边界（历史快照）**：fresh 链为 source `09:29:03` < Unity DLL `09:31:10` < self-check log `09:32:07` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect/review **PASS / no P0-P2 findings**。Batch3 当时未执行 Play、实际 pixel baseline 或设备验收，HitRecord/Overlay 均未收口。后续 Batch4/5 已关闭 HitRecord resource/lifecycle 代码缺口；当前仍由 Overlay 阻塞 `CentralOnly`。T8 已排除。
- **P7 Batch4 SPARK / Common HitRecord resource ownership 已完成代码层收口**：typed `CommonSpark(pic)` 覆盖 20 帧；SPARK 经 prewarm 单次 decode/process 后于 main thread atomic publish。legacy `SparkRenderer` 不再在 `Awake` decode 或创建资源；central resolver 验证 logical key、`Sprite`、`Texture`、rect、pivot、size 与 material。publication lease/retirement 已接入 common resource lifecycle。
- **Batch4 失败与状态不变契约**：缺失或无效 SPARK 释放 stale lease，且不改变 `HitRecord` age/count；partial `Texture`/`Sprite` 构造失败会事务式清理所有已创建资源，禁止 partial publication。
- **Batch4 fresh 证据与边界**：source `11:13:05` < Unity DLL `11:15:20` < self-check result `11:17:38` **PASS**；architect re-review **PASS / no P0-P2 findings**。code-review provider 返回 `429`，没有 code-review 通过结论。Batch4 当时未包含 HitRecord lifecycle mutation；该项已由下方 Batch5 收口。Play、pixel、Profiler、真机与真实 SPARK 资源路径仍未验收；T8 继续排除。
- **P7 Batch5 HitRecord presentation cycle 已完成代码层收口**：新增 backend-neutral immutable double-buffer cycle。`RenderDispatch` 捕获 owner slot handle/generation、count、age、x/z 与 frozen common publication；`SparkRenderer` 只负责 materialize/probe，不再写 live HitRecord。`LateUpdate` 固定为 legacy materialize -> central `PrepareFrame` -> one finalizer；catch-up 只 finalize 最后一个 cycle。
- **Batch5 mutation 与隔离契约**：missing SPARK 为 zero-write；valid record 每 cycle 的 age 恰好 `+1`；invalid sampled tail 每 cycle 最多删除 1 项，age `4/14/28/38` 刚进入 gap 的同 cycle 不删除。slot reuse、count/age guard 均已覆盖，pool、camera 与 backend 选择不改变 mutation 结果。
- **Batch5 后续 P2 修复**：common binding 改为 direct ownership transfer，不再依赖 per-tick lease GC；no-hit cycle 不持 binding。coordinator reset 已接入 world reset、driver unbind、world replacement 与 destroy。ordered owner cursor 为 O(N)，`1000` owners fixture 精确验证 `1000` 次 comparisons。
- **Batch5 fresh 证据**：source `12:39:24` < Unity DLL `12:40:40` < self-check result `12:41:20` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS / no P0-P2 findings**；code review **APPROVE / no P0-P2 findings**。Play、pixel 与 device 仍未验收。
- **Overlay authority re-audit confirmed blocker**：权威 `BattleHostForm` 与 `SdlBattleRenderer` 实际顺序均为 `Shadow -> Entity -> EntityOverlays -> HitRecords`。per-entity Overlay 绘制 `Hp2Orig > 1` 的复活次数和 entity label；`WORDS0..5.bmp` 以每 glyph `8x16`、步距 `9`、black colorkey 提供资源。Unity `Assets` 当前没有 `WORDS0..5`，也缺 `BattleSlotLabels[10,12]` / 对应 state 镜像与 snapshot 字段契约，因此 Overlay 未实现并继续阻塞 `CentralOnly`。global function/pause overlay 是独立后置 UI，且 GDI/SDL 行为不一致，不并入 per-entity P7，本批不处理。T8 继续排除。

本节是当前状态；下方早期阶段中“Extended Driver checksum 跳过/为空”或“Extended schema 尚未实施”的文字仅保留为当时历史边界，不再代表当前实现。

## BATTLE-RENDER-PLAN1 状态

- **状态**：方案已确认；R1-R2C-4、B0、B1-B1.3、B2A、B2B、B2C 与 **P1-P6** 已完成代码层实施；P6 真机验收未完成。P7 的 held、render-state semantic parity、Shadow、Batch4 SPARK/Common HitRecord ownership 与 Batch5 HitRecord presentation cycle 子批已完成；per-entity Overlay 是 confirmed blocker，P7 整体未完成。
- **代码状态**：独立 `BruteForce` / `LooseQuadtree` 正式 collision broadphase 后端已具备 generation-aware 增量同步；默认仍为 `BruteForce`。除 fixed-tick candidate collect 外，B2C 已接入即时 weapon/body current-world query 与 AI 输入快照查询；二者均保留失败回退 brute。
- **验证状态**：B2B、B2C/P1、P2-P5 与 P6/P7 Batch1-2 的分项证据保留在上方各节。P7 Batch3 的 fresh 证据为 source `09:29:03` < DLL `09:31:10` < self-check log `09:32:07` **PASS**；Batch4 为 source `11:13:05` < DLL `11:15:20` < result `11:17:38` **PASS**，architect re-review **PASS / no P0-P2**，code-review provider `429` 不记为通过。Batch5 fresh 链为 source `12:39:24` < DLL `12:40:40` < result `12:41:20` **PASS**，dotnet **0 errors / 18 existing warnings**，architect **PASS / no P0-P2**，code review **APPROVE / no P0-P2**。Play/pixel/device 仍未验收；Overlay 未实现，`CentralOnly` 继续拒绝。
- **容量说明**：`400` 是 `Authority400` 兼容模式的 C# 权威槽位边界，不是所有 Unity 运行模式的全局容量上限。权威 `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Common\NtsdConstants.cs` 中的 `NtsdConstants.MaxObjects` 定义 `MaxObjects = 400`，`BattleCore\Simulation\SimulationWorld.cs:28-32` 据此创建 `Objects[400]`、`VRest[400,400]` 和 `ARest[400]`；Unity `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:39-44` 以 `MaxRuntimeSlots = 400` 镜像该契约。扩展模式的 active entity 容量与 render command 容量分开管理；每个实体可产生 `Shadow`、`Entity`、`Overlay`、`HitRecord` 等多个命令，Mesh 仍须按实际命令峰值预分配并分 chunk。
- **平台 Profile 说明**：生产解析优先级固定为“命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认值”；平台宏只提供默认 Profile，不进入战斗逻辑、最小堆、Loose Quadtree、VRest 或命中规则。设备能力降级只改变图集、纹理和渲染后端，不得改变已选 Profile 的战斗容量或结果。
- **实施边界**：fixed-tick formal collect 仍在 B2B 边界对当帧 participant 做 batch synchronize，不把 registry mutation 直接写入 collision 索引。B2C 的即时 weapon/body 与 AI 查询各自从当前 world/snapshot 构建查询视图，generation、几何或映射无法验证时回退 brute；它们不改变 fixed-tick pair 的 authority ordinal、RNG 或 candidate 时序。正式 collect 结果仍按 canonical runtime-slot pair 合并、去重，再按原 authority ordinal 双向派发；任何无法证明完整性的情况均 reset 增量索引、整 tick 回退 brute-force，并原子恢复 RNG/candidate 状态。

### 2026-07-20 R1 第一批实施记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| Profile resolver | **已实施 / 已验证** | 支持显式覆盖 > 配置值 > 平台默认；平台默认由 Unity 条件编译符号选择。Editor/其他平台回落 `Authority400`，Android Player 为 `MobileExtended`，Standalone Player 为 `DesktopExtended` |
| `Authority400` 最低空闲槽分配 | **已实施 / 已验证** | 以 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused` 保留 roster、stage、dynamic band 语义；支持按索引移除、释放回收和最低槽确定性分配 |
| 正式 runtime 接线 | **兼容模式已接入** | `SimulationWorld` 仍显式固定为 `Authority400`，本批不改变 400-slot 行为边界，也不自动启用平台扩展模式 |
| 扩展容量与空间索引 | **R1 历史边界，后续已替代** | R1 当时仅有独立分页 `RuntimeSlotTable` 与 generation handle；`MobileExtended`、`DesktopExtended` 生产接线、桌面动态增长、1000 active admission、AI 与 Loose Quadtree 已由后续阶段实施，当前状态以本文件顶部 B2C 节为准 |

fresh 验证：相关源码时间 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` 结果 `12:05:07` **PASS**；分配器另以 **100,000 次随机 claim/release/allocate 操作**与朴素线性扫描模型逐步对照，结果 **PASS**；架构复核 **PASS**。这些证据只关闭 R1 第一批，不代表 Play Mode、扩展容量、四叉树或集中式渲染已经验收。

### 2026-07-20 R2A 分页槽表与 generation 句柄基础记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| `RuntimeSlotTable` 分页存储 | **基础设施已实施 / 已验证** | 固定 `PageSize = 256`，按首次访问惰性物化页面；`Authority400` 逻辑容量为 400，`MobileExtended` 设计容量为 1050，最后一页超出各自逻辑尾部的地址均被 guard 拒绝 |
| raw runtime / raw rest 存储 | **基础设施已实施 / 已验证** | 每个 slot 持有独立 `NTSDEntityRuntime` 与 `LF2ItrRestTracker.StateSnapshot` 存储；raw 状态与实体 claim 生命周期分开，不因只读查询隐式占用槽位 |
| 占用计数 | **基础设施已实施 / 已验证** | `ClaimedCount` 由 allocator 契约维护，claim、release 与 reset 后均由 focused self-check 校验 |
| `RuntimeEntityHandle` | **基础设施已实施 / 已验证** | 句柄由 `(slot, generation)` 构成；release、同槽 reuse 与 reset 都推进 generation，使旧句柄无法再 resolve 到新占用者 |
| 生产 runtime 接线 | **未实施 / 未启用** | `SimulationWorld` 仍使用现有 `Authority400` registry/raw arrays，并未切换到 `RuntimeSlotTable`；本批不改变战斗结果或现有 400-slot parity schema |

R2A fresh 验证：相关源码时间 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` 结果 `12:36:53` **PASS**；架构复核 **PASS**。这些证据只验证分页地址、惰性物化、独立 raw 存储、`ClaimedCount` 与 generation 失效契约；不代表 `Extended` 已启用，也不覆盖桌面动态增长、移动端 1000 admission、AI 迁移、Loose Quadtree 或 VRest 改造。

### 2026-07-20 R2B `Authority400` 生产 registry 迁移记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 单一槽位存储后端 | **已实施 / 已验证** | 生产 `SimulationWorld` 的 `_runtimeSlotUsed`、`_rawRuntimeSlots`、`_rawRestSlots` 已由单一 `RuntimeSlotTable` 替代；旧字段检索为 0，registry 不再维护并行槽位真值 |
| 当前占用者查询 | **已实施 / 已验证** | `FindEntityByRuntimeSlotIncludingDormant` 与 current-pass 查询直接通过 slot 地址 O(1) 解析当前 occupant；长期引用仍必须使用带 generation 的 `RuntimeEntityHandle` |
| pass 遍历时序 | **已实施 / 已验证** | 保留 live ascending slot scan：游标以上新生实体可进入本 pass，复用游标以下低槽的实体等待下一 pass，保持既有 high-newborn / low-reuse 时序 |
| release 身份保护 | **已实施 / 已验证** | release 必须同时匹配 slot 与 `expectedEntity`/当前 occupant；过期实体不能释放已被另一实体复用的槽 |
| raw rest 语义 | **已实施 / 已验证** | stage spawn 继续恢复并消费复用槽 raw rest；ordinary spawn 继续按既有语义重置，不把 R2B 存储迁移扩大成 VRest/ARest 规则变更 |
| 对外可观察契约 | **保持不变 / 已验证** | `ObjectCount`、对象 buckets、`SceneQueryHit` 的 runtime-slot 地址语义保持不变；生产 Profile 仍固定为 `Authority400` |

R2B fresh 验证：相关生产源码时间 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` 结果 `12:57:02` **PASS**；fresh `dotnet build` 为 **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索为 **0**。这些证据只关闭 `Authority400` 的生产 registry 存储迁移，不代表 `Extended`、移动端 1000 admission、桌面分页增长、AI、Loose Quadtree、VRest 解耦或集中式渲染已启用。

### 2026-07-20 R2C allocator/table 单调增长记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| `RuntimeSlotAllocator.GrowTo` | **基础设施已实施 / 已验证** | 只允许容量单调增加；增长后保留三段边界、dynamic segment 的 indexed binary min-heap、`nextUnused`、已占用槽与 `ClaimedCount`，并继续优先复用增长前的最低空洞，再使用新开放地址 |
| `RuntimeSlotTable.GrowTo` | **基础设施已实施 / 已验证** | 增长时扩展页引用数组但不主动物化新页；保留既有 page object、occupant、generation handle、raw runtime、raw rest 与 claim 状态，新页仍在首次访问时惰性物化 |
| 非增长调用 | **已验证** | 目标容量等于当前容量时成功 no-op；缩容请求返回拒绝，且容量、claims、页面、句柄和 raw 状态保持不变 |
| 移动端地址契约 | **设计边界已修正 / focused 已验证** | `1000 active` 是 admission 预算，不是逻辑地址尾值；保留 `0..49` 后，1000 个动态槽为 `50..1049`，因此逻辑地址容量是 `1050`。`PageSize=256` 时物理数组需要 5 页，但物理尾部 `1050..1279` 必须不可寻址、不可 claim、不可创建 raw runtime |
| 生产接线 | **R2C 时未实施；已由 R2C-4 后续接入** | `SimulationWorld` 在 R2C 时仍固定 `Authority400`；生产 Profile、Mobile total admission 与 Desktop 自动增长已由 R2C-4 接入 |

R2C fresh 验证：相关源码时间 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` 结果 `13:25:34` **PASS**；fresh `dotnet build` 为 **0 errors**；架构复核 **PASS**。这些证据只证明 allocator/table 可在保持既有状态与最低槽语义的前提下单调增长，并验证移动端 `1050` 逻辑地址及物理尾部 guard；不代表 Extended Profile、生产增长、移动端 admission、AI、Loose Quadtree 或集中式渲染已经启用。

### 2026-07-20 R2C-3A `SimulationWorld` 实例容量读取记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| world 容量真值 | **已实施 / 已验证** | `SimulationWorld.RuntimeSlotCapacity` 读取当前 `_runtimeSlots.LogicalCapacity`；registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量循环不再假定固定 400 |
| 默认兼容模式 | **保持不变 / 已验证** | 默认 `SimulationWorld()` 仍创建 `Authority400/400`；现有生产 Driver、400-slot parity 与默认 self-check 不会自动进入扩展模式 |
| focused 扩展契约 | **内部测试入口已实施 / 已验证** | internal 构造以 `DesktopExtended/512` 创建 focused world；slot `511` 可注册、查询并进入 AI 目标扫描，slot `512` 被拒绝，reset 后高槽状态被清理 |
| parity schema | **保持固定 / 已验证** | `BattleParitySnapshot` 继续显式使用 `AuthorityRuntimeSlotCapacity = 400`，没有把历史 400-slot certificate 静默扩展为新 schema |
| 生产与外部边界 | **R2C-3A 时 Profile 未实施；现已由 R2C-4 接入** | `MobileExtended` / `DesktopExtended` Profile 后续已接入生产 Driver；`LF2SpecialAttack` / `LF2Entity` 的外部固定容量边界已在 R2C-3B 按 world capacity 处理 |

R2C-3A fresh 验证：相关源码时间约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` 结果 `13:54:22` **PASS**；fresh `dotnet build` 为 **0 errors / 42 warnings**。这些证据证明默认 400 行为未变，并证明显式 512-slot world 的代码层容量契约可运行；扩展 Profile 当时仍未接入生产 Driver，外部 special/transition 固定边界随后由 R2C-3B 关闭。

### 2026-07-20 R2C-3B 外部容量边界与 parity guard 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| special attack 高槽 holder | **已实施 / 已验证** | `LF2SpecialAttack` 不再用固定 400 拒绝 holder slot；在已绑定 world 时按 `RuntimeSlotCapacity` 验证并解析扩展高槽 holder |
| Karasu 高槽扫描 | **已实施 / 已验证** | Karasu oid209 替换扫描使用当前 world 容量，`DesktopExtended/512` 中的高槽目标不再被 `0..399` 截断 |
| transition effect 容量计数 | **已实施 / 已验证** | `LF2Entity` transition effect 的可用动态槽计数使用当前 world 的 dynamic 起点到逻辑容量尾部，不再固定扫描 `50..399` |
| parity capture guard | **已实施 / 已验证** | 历史 parity capture 必须同时满足 Profile 为 `Authority400` 且逻辑容量为 400；`DesktopExtended/512` 与 `DesktopExtended/400` 均明确拒绝，不能仅凭容量为 400 冒充 authority certificate |
| 生产接线 | **R2C-3B 时未实施；已由 R2C-4 后续接入** | 默认生产 Driver 的 Profile、admission 与桌面自动增长后续已接入；本批仍未实现扩展 parity schema |

R2C-3B fresh 验证：相关源码时间 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` 结果 `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**，warnings 为既有告警。该证据关闭 3A 后遗留的 special attack / transition effect 固定容量边界，并建立严格的 authority parity capture guard；不代表生产 Driver/Profile 接线、admission、桌面自动增长、Loose Quadtree、VRest 或集中式渲染已完成。

### 2026-07-20 R2C-4 生产 Profile 激活记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 生产 Profile 解析优先级 | **已实施 / 已验证** | 命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > Unity 平台宏默认；配置值不再被 `Awake`/重建路径静默覆盖 |
| 默认容量 | **已实施 / 已验证** | `Authority400` 逻辑容量 `400`；`MobileExtended` 逻辑容量 `1050`，`TOTAL active admission = 1000`（跨 roster/stage/dynamic 全部槽区）；`DesktopExtended` 默认初始逻辑容量 `512`，按 `PageSize=256` 规范化并支持自动增长 |
| Driver 生命周期 | **已实施 / 已验证** | `SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析与 world 创建路径；直接 `BattleTestBootstrap` 在实体注册前重新协调晚到的 GameConfig |
| Desktop 增长 | **已实施 / 已验证** | 自动增长保留最低空洞分配顺序，并同步扩展 AI snapshot 容量，避免 world 与 AI 视图分叉 |
| Extended checksum/parity | **历史边界，已由 B2C 替代** | 当时 Extended Driver checksum 输出跳过/为空；当前 B2C 已提供独立 Extended checksum，direct parity capture 仍只接受 `Authority400 + 400` |
| 后续阶段 | **R2C-4 历史边界，后续已替代** | B0 shadow 随后落地；B1-B2B 后续完成 VRest 解耦、增量更新与 formal backend，B2C 已实施即时 weapon/body、AI 查询和 Extended checksum。集中式渲染仍是后续计划，默认 broadphase 仍为 `BruteForce` |

R2C-4 fresh 验证：相关源码时间 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` 结果 `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors / 42 existing warnings**；architect final review **PASS**。

### 2026-07-20 B0 shadow Loose Quadtree 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 纯数据空间树 | **已实施 / 已验证** | X/Z half-open 归属；`looseness = 1.5`、`leafCapacity = 16`、`maxDepth = 8`；不依赖 Transform 或 Unity Physics |
| 构建策略 | **shadow 已实施 / 正式切换未实施** | 每次 collision collect 全量重建；尚未采用增量更新，也未替换正式 brute-force broadphase |
| 诊断边界 | **已实施 / 默认关闭** | 对比 brute AABB pair、tree pair 与正式 accepted subset；诊断关闭时不承担生产结果责任，不据此宣称性能提升 |
| 权威流程保护 | **保持不变 / 已验证** | 正式 `i/j` 遍历、VRest、RNG、candidate 收集/截断/消费顺序继续使用原权威流，shadow 结果不写回战斗真值 |
| 后续接入 | **B0 历史边界，后续已替代** | 即时 weapon/body 与 AI 查询已由 B2C 接入；VRest 解耦、增量更新与 formal broadphase 已由 B1-B2B 接入。生产默认仍为 `BruteForce` |

B0 fresh 验证：相关源码时间不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` 结果 `16:15:43` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。这些证据只证明 shadow 数据结构、pair 诊断和权威流隔离正确，不证明生产 broadphase 已切换或已有性能收益。

### 2026-07-20 B1 `RuntimeRestStore` 基础记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| ARest 存储 | **纯数据基础已实施 / 已验证** | 分页、惰性物化；逻辑容量外地址拒绝，不因只读访问隐式创建页 |
| VRest 存储 | **纯数据基础已实施 / 已验证** | 定向稀疏 `VRest[victim, attacker]`；只保存正值，写零即移除，不把双向 pair 合并 |
| 槽位清理 | **已实施 / 已验证** | `ResetSlot(slot)` 同时清该槽 ARest、VRest victim row 与 attacker column，防止槽复用继承旧 rest |
| 生命周期与扩容 | **已实施 / 已验证** | 支持 `GrowTo`、全局 reset、排序后的 diagnostics/snapshot，以及 snapshot restore；增长保持既有稀疏状态 |
| 差分验证 | **已验证** | 2,000 次随机操作与 dense reference model 逐步 differential，对定向读写、清零移除、slot reset、grow/reset 与 snapshot restore 进行比较 |
| 生产接线 | **B1 时未实施；已由 B1.2 后续接入** | facade lifecycle 与 parity fallback 已由 B1.2 接入；collision pair tick 解耦与正式 quadtree switch 仍 pending |

B1 fresh 验证：相关源码时间 `2026-07-20 16:31:32` < Unity `Assembly-CSharp.dll` `16:36:38` < 完整 `BattleRuntimeSelfCheck` 结果 `16:37:13` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**；architect final review **PASS**。这些证据只验证纯数据 store 契约，不代表生产 VRest/ARest owner 已迁移，也不代表 pair tick 已与 collision broadphase 解耦。

### 2026-07-20 B1.1 optional facade 与 victim-row lease 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| optional facade | **已实施 / 已验证 / 未 production-bound** | `LF2ItrRestTracker` 可选择绑定 `RuntimeRestStore`，未绑定时保留既有实现；当前生产 world 尚未启用该绑定 |
| victim-row ownership | **已实施 / 已验证** | facade 获取 exclusive victim-row lease；同一 victim row 不允许多个 facade 并发拥有，释放 lease 后才允许后续 owner 接管 |
| 语义边界 | **保持不变** | facade 只适配现有 ARest/VRest 定向语义，不改变 store 的 positive-only、zero-removal、row/column reset 或排序 snapshot 契约 |
| state import 原子性 | **已修复 / 已验证** | architect 首轮发现 `ReplaceVictimState` 在 mixed-invalid attacker 输入下可能先写入部分合法项再失败；现已先完整预验证，之后原子替换，失败时原状态不变 |
| failed-import 回归 | **已验证** | direct `ReplaceVictimState` 与 facade `Bind` 两条路径均覆盖 mixed-invalid 输入，并断言失败前后的 ARest/VRest 状态完全一致 |
| 非阻塞补强 | **可后续补充** | invalid bound `RestoreState` 的单独断言尚可增加；该路径复用已验证的 atomic replace 入口，不构成当前 blocker |
| 下一批生产接线 | **B1.1 时未实施；已由 B1.2 后续接入** | registration、release、world reset 已按 ordinary 清理与 `StageSpawnAt` retention 分流接入 |

B1.1 修正后 fresh 验证：复跑 `dotnet build Assembly-CSharp.csproj` 为 **0 errors / 18 existing warnings**；相关源码时间 `2026-07-20 17:34:22` < Unity `Assembly-CSharp.dll` `17:36:49` < 完整 `BattleRuntimeSelfCheck` 结果 `17:39:07` **PASS**；architect final review **PASS / no blocker**。该批证据本身不代表 production-bound；后续绑定由 B1.2 单独实现和验证。

### 2026-07-20 B1.2 production lifecycle binding 记录（已验证 / architect final PASS）

| 项目 | 当前状态 | 证据 |
|---|---|---|
| store ownership | **已实施 / self-check verified** | `SimulationWorld` 独占 `RuntimeRestStore`，store 生命周期随 world 创建、reset 与 grow 同步 |
| ordinary claim | **已实施 / self-check verified** | claim 成功后先 `ResetSlot(slot)`，再以 `Bind(..., importLegacyState: false)` 绑定 tracker |
| release | **第三个 blocker 已修 / self-check verified** | `ReleaseRuntimeSlot` 返回 bool 并事务传播到全部注销/待销毁调用链；错槽拒绝时不继续半注销，正常 release 保留 store 并解绑 |
| `StageSpawnAt` | **blocker 已修 / self-check verified** | rejected bind 走共享完整 pool 回收；真实 pool counts、lease、slot 与 `KillStats` 均有回归断言 |
| public `Unregister` 故障回归 | **已验证** | 通过公开 `Unregister` 触发错槽 release 拒绝，断言完整 registration context（bucket/slot/lease/store/entity）保持不变 |
| 单一 rest 真值 | **已实施 / self-check verified** | 删除 `RuntimeSlotTable.RawRest`；parity fallback 直接读取 `RuntimeRestStore` |
| world reset/grow | **已实施 / self-check verified** | world reset/grow 与 store 同步 |
| 尚未关闭 | **未实施 / 未验证** | collision pair tick 解耦仍未实施；本批不切换正式 broadphase，且与 T8 无关 |

B1.2 初版证据：`dotnet build` **0 errors**；源码 `2026-07-20 18:11:41` < Unity DLL `18:12:23` < full self-check `18:13:00` **PASS**。architect final review 随后发现上述 2 个 blocker；该证据现只说明初版可编译且旧断言通过，**不构成 B1.2 完成/验证证据**。

B1.2 第一轮 blocker 修复证据：`dotnet build` **0 errors**；源码 `18:21:20` < Unity DLL `18:21:58` < self-check `18:22:59` **PASS**。architect 第二轮随后发现 release 拒绝未向 `Unregister` 调用链传播、可能半注销；因此该 PASS 同样是**非完成证据**。

B1.2 最终 fresh 证据：`dotnet build` **0 errors**；相关源码 `2026-07-20 18:31:25` < Unity DLL `18:33:58` < full self-check `18:34:54` **PASS**。公开 `Unregister` 故障矩阵验证完整注册上下文不变；architect final review **PASS / no blocker**。

### 2026-07-20 B1.3 collision pair VRest tick 解耦记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| pass 顺序 | **已实施 / self-check verified** | 单 tick 固定为 `CaptureSnapshots -> sparse Tick -> Collect`；VRest 递减在候选收集前独立完成 |
| eligible row | **blocker 已修 / self-check verified** | 直接遍历 registered bucket items，筛选 `active + CharData` victim；inactive row 冻结，不扫描 `RuntimeSlotCapacity` |
| pair 内副作用 | **已移除 / self-check verified** | `BruteForceSceneQuery` 不再在 pair 枚举内部 tick VRest；early return、无 pair 与候选截断都不能漏 tick 或重复 tick |
| store 热路径 | **已实施 / self-check verified** | `RuntimeRestStore` 维护 active-positive-row/stamp，scratch 随容量预扩；eligibility 无 capacity scan、无 snapshot 分配 |
| Desktop 稀疏高槽 | **已验证** | 高逻辑容量 world 仅两个 registered eligible items 时访问计数严格为 `visited=2` |
| 验证矩阵 | **已覆盖** | dense differential、registration/release lifecycle、inactive freeze、early-return/no-pair、diagnostics 与 parity fallback 均进入 full self-check |
| broadphase | **未切换** | 正式候选仍由原 brute-force collect 产生；B1.3 不代表 Loose Quadtree 已接管生产 broadphase |

B1.3 初版证据：`dotnet build` **0 errors**；源码 `19:09:44` < DLL `19:10:34` < self-check `19:11:13` **PASS**。architect 随后发现 eligibility 仍为 O(`RuntimeSlotCapacity`) 全扫，该证据因此是**非完成证据**。

B1.3 最终 fresh 证据：`dotnet build` **0 errors**；相关源码 `2026-07-20 19:19:14` < Unity DLL `19:19:47` < full self-check `19:22:50` **PASS**；Desktop sparse high-slot `visited=2`；architect final review **PASS / no blocker**。

### 2026-07-20 B2A formal Loose Quadtree broadphase 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 后端选择 | **已实施 / self-check verified** | 独立 `CollisionBroadphaseBackend` 支持 `BruteForce` 与 `LooseQuadtree`；解析优先级为命令行 `-ntsdCollisionBroadphase` > `GameConfig.BattleCollisionBroadphaseName` > 默认 `BruteForce`，平台宏不进入战斗分支 |
| 接管边界 | **B2A 历史边界，已由 B2C 部分替代** | B2A 仅替换 fixed-tick `CollectCollisionCandidates`；B2C 随后接入即时 weapon/body current-world query，失败仍走 brute fallback |
| participant/pair 顺序 | **已实施 / self-check verified** | 收集与 brute outer loop 相同的 eligible participant 并保留 authority ordinal；tree/fallback pair 使用 `(minSlot,maxSlot)` canonical key 全局排序去重，随后按 authority ordinal 以 `a->b`、`b->a` 顺序派发 |
| 无效 AABB | **保守处理 / self-check verified** | 缺失或无效 AABB 的 participant 不被遗漏，而是与全部其他 eligible participant 组成 fallback-all pair；extra formal pair 仍由 narrow phase 过滤 |
| 整 tick 回退 | **已实施 / self-check verified** | runtime slot 缺失/重复/越界、slot-to-entity mapping 不一致、query index/entry count 非法、rebuild/query 异常，或 diagnostics 发现缺少 brute coverage 时，丢弃 formal 部分结果并整 tick 重跑原 brute-force |
| 原子性与确定性 | **已实施 / self-check verified** | formal 失败时恢复进入前 RNG state/call count，清空 candidate carrier/count/distance/cache 后再 brute collect；candidate 20 上限、nearest/type ties、RNG 与消费顺序保持原权威路径 |
| diagnostics | **默认关闭 / self-check verified** | 开启时比较 brute canonical set 与 formal set；缺 pair 强制整 tick brute fallback，extra pair 允许并交 narrow phase；诊断不改变 RNG 或战斗状态 |
| 后续阶段 | **B2A 时未实施；已由 B2B 后续接入** | B2A 当时仍为每 fixed tick full rebuild；generation-aware 增量迁移/更新现已由下节 B2B 接入，生产默认仍未切为 Loose Quadtree |

B2A fresh 证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` **0 errors**；相关源码最新时间 `2026-07-20 22:15:07` < Unity `Assembly-CSharp.dll` `22:18:48` < full `BattleRuntimeSelfCheck` 结果 `22:19:28` **PASS**。architect final review **PASS / no blocker**；本批未执行 Play Mode，不能据此扩大为完整场景验收。T8 默认 `stage.dat` 部署继续暂缓。

### 2026-07-20 B2B generation-aware 增量 Loose Quadtree 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 同步边界 | **已实施 / self-check verified** | formal backend 在每次 fixed-tick collision collect 边界批量同步当帧 eligible participant；注册、注销和移动本身不直接改树，避免把 registry mutation 时序引入权威 pass |
| 稳定身份 | **已实施 / self-check verified** | 索引记录与查询结果使用 `(runtime slot, generation)` 的 `RuntimeEntityHandle`；同槽释放再复用时旧 generation 被移除，新 occupant 作为新 handle 插入，不会把旧空间记录解析到新实体 |
| 增量更新 | **已实施 / self-check verified** | 未移动实体保持原记录；AABB 改变但仍在当前节点 loose 容纳范围内时原位更新；越出 loose 范围时才从旧节点移除并重新插入。新增、销毁、invalid-AABB 转换和同槽复用均由同一 batch sync 收口 |
| root escape | **保守重建 / self-check verified** | 当前有效 AABB 超出既有 root 时执行一次全量 rebuild；正常的 loose 内移动与跨 loose 迁移不重建整棵树 |
| live query validation | **已实施 / self-check verified** | quadtree query 返回 handle，派发前必须由当前 `RuntimeSlotTable` generation 成功解析，并再次核对 slot、entity、participant ordinal 与 handle 映射 |
| 原子回退 | **已实施 / self-check verified** | sync/query/invariant/mapping 异常会 reset 增量索引并整 tick 重跑 brute-force；B2A 已有 RNG/candidate rollback 继续包住 formal collect，部分执行不能污染候选、RNG 或消费顺序 |
| world reset | **已实施 / self-check verified** | `SimulationWorld` registry reset 显式清理 formal spatial index，旧 match 的 node、record 与 handle 不会进入下一 world 生命周期 |
| 启用边界 | **B2B 历史边界，已由 B2C 部分替代** | 生产默认仍为 `BruteForce`；只有显式选择 `LooseQuadtree` 才使用 formal backend。B2C 已接入即时 weapon/body 与 AI 查询及 Extended checksum；集中式渲染仍不属于 B2B/B2C |

B2B fresh 证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` **0 errors**；相关源码最新时间 `2026-07-20 22:43:57` < Unity `Assembly-CSharp.dll` `22:46:36` < full `BattleRuntimeSelfCheck` 结果 `22:47:04` **PASS**。architect final review **PASS / no blocker**；本批未执行 Play Mode，不能据此扩大为完整场景验收。T8 默认 `stage.dat` 部署继续暂缓。

## Runtime 容量与空间索引阶段决策

**状态：B1-B1.3、B2A 与 B2B 已完成代码层实施 / 编译 / full self-check / architect final review。** B2C 已实现 Extended checksum、AI Loose Quadtree 查询和即时 weapon/body current-world query，并有 `2026-07-21 00:48:06` full self-check PASS；B2C 本身尚无 fresh Architect PASS、Play Mode 或性能验收。生产默认 broadphase 仍是 `BruteForce`。

### RuntimeSlot 容量模式

- **`Authority400` 兼容模式**：保留 C# 的 400 runtime slot、既有特殊槽区和最低空闲槽分配语义，用于现有 self-check、parity 和逐帧对照。该模式的 400 是兼容边界，不代表 render command 上限。
- **移动端扩展模式**：逻辑地址容量为 `1050`，最后有效地址为 `1049`；`TOTAL active admission = 1000`，跨 roster/stage/dynamic 全部槽区计数，第 `1001` 个 active entity 必须确定性拒绝生成，不排队、不替换，也不由设备瞬时内存状态决定。拒绝结果必须进入可重放的结果/日志边界。
- **桌面扩展模式**：默认初始逻辑容量 `512`，按 `PageSize=256` 规范化为整页并在需要时自动增长；不设置玩法层面的 active entity 上限，但仍受明确的地址空间、内存、对象池、逻辑帧和 render command 技术预算约束，不能解释为物理上无限容量。
- 空闲槽使用**二叉最小堆 + `nextUnused`**：R1 第一批已在 `Authority400` 内按 `0..19`、`20..49`、`50..399` 三段实现 indexed binary min-heap；已释放槽进入最小堆，分配时优先取最小空闲槽，堆为空时使用并递增 `nextUnused`。R2A 以 256 槽/页建立惰性分页表并复用该 allocator，R2B-R2C-3B 依次接入槽表、增长、实例容量和外部边界，R2C-4 已将 Desktop 自动增长接入生产。增长前的最低空洞仍优先于新页地址，且 AI snapshot 与 world 容量同步扩展；所有分配、释放和分页增长继续保持最低槽确定性，不依赖 `Dictionary`/`HashSet` 枚举顺序。
- **分层位图**仅作为后续候选优化，不作为本阶段实现前提；若采用，必须保持与最小堆相同的最低槽和回放语义。

### 平台 Profile 与选择边界

**状态：resolver 与生产 Profile 激活已实施并通过 self-check / architect final PASS。** 平台差异通过统一 Profile/能力配置入口表达；不得在战斗 pass、opoint、碰撞、命中、对象生命周期或空间查询内部散布 `#if UNITY_ANDROID` / `#if UNITY_STANDALONE` 分支。Unity 官方条件编译符号仅用于选择平台默认值；`SystemInfo` 等运行时能力 API 留给后续渲染后端降级，不改变战斗 Profile 或逻辑结果。

运行模式固定为：

| Profile | 平台默认与用途 | RuntimeSlot / active 边界 |
|---|---|---|
| `Authority400` | `UNITY_EDITOR` 和未明确支持的平台默认；用于 C# 权威对拍、现有 self-check、历史 parity schema 与兼容诊断 | 固定 400 槽，保留权威特殊槽区和最低空闲槽语义 |
| `MobileExtended` | `UNITY_ANDROID && !UNITY_EDITOR` Player 默认 | 逻辑容量 1050；全部槽区合计最多 1000 active，第 1001 个发布尝试确定性拒绝 |
| `DesktopExtended` | `UNITY_STANDALONE && !UNITY_EDITOR` Player 默认 | 默认初始 512，按 256-slot 页规范化并自动增长；不设玩法层面的 active 上限，但受明确技术预算约束 |

宏边界必须按以下规则实现：

- `UNITY_EDITOR` 优先于当前 Build Target 宏。Editor 即使切到 Android Build Target，也不能仅因同时定义 `UNITY_ANDROID` 就自动进入移动端正式 Profile；Editor 平台默认保持 `Authority400`，测试或配置可显式覆盖为 `MobileExtended` / `DesktopExtended`。
- `UNITY_ANDROID && !UNITY_EDITOR` 只负责给 Android Player 选择 `MobileExtended` 默认值；`UNITY_STANDALONE && !UNITY_EDITOR` 只负责给桌面 Player 选择 `DesktopExtended` 默认值。
- 其他 Player 平台在完成单独设计和验收前默认 `Authority400`，不得根据相似平台经验自动套用 Android 或桌面扩展规则。
- 平台宏只允许出现在默认 Profile 选择和不可避免的平台专属 API 适配入口。核心 runtime 统一读取已解析的 Profile/预算，不直接读取平台宏。

配置解析优先级固定为：

```text
命令行显式覆盖
    > GameConfig.BattleRuntimeProfileName
    > 平台宏默认 Profile
```

- 命令行显式覆盖用于 self-check、parity、回放和 Editor A/B 验证，必须能强制选择 `Authority400`、`MobileExtended` 或 `DesktopExtended`。
- `GameConfig.BattleRuntimeProfileName` 是生产项目配置入口；`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用同一解析路径，直接 `BattleTestBootstrap` 在实体注册前协调晚到配置。
- 运行时设备能力检测发生在 Profile 解析之后。`SystemInfo.supports2DArrayTextures`、纹理尺寸/slice 上限、图形 API、格式支持和目标 GPU 验证结果只用于选择可用的资源与渲染后端。
- 推荐降级链为 `Texture2DArray + OrderedChunks` -> `多 Texture2D + OrderedChunks` -> `LegacySpriteBackend`；任何降级都必须保持原 painter 顺序和相同只读表现输入。
- 设备不支持 `Texture2DArray`、命中设备黑名单或内存预算不足时，不得把 `MobileExtended` 静默改成 `Authority400`，也不得降低 1000 active admission 边界来掩盖渲染预算不足；应通过分 chunk、后端降级、可诊断拒绝或明确启动失败处理。

所有 Profile 必须共用同一份二叉最小堆 + `nextUnused`、分页 slot、generation handle、Loose Quadtree、VRest/ARest、候选排序和 lifecycle 实现。平台可以改变容量、预分配、图集格式、chunk 数和渲染回退策略，但不能改变逻辑 tick、slot 决定性、pair 顺序、VRest 计时、opoint 生成顺序或战斗结果。

### 移动端 1000 active admission 边界

- `1000 active` 与 slot address 容量是两个独立数字：`RuntimeSlotTable.LogicalCapacity = 1050`，最后有效地址是 `1049`，其中 `0..19` 为 roster、`20..49` 为 stage、`50..1049` 为 dynamic 地址。active admission 的 1000 是**全部槽区合计预算**，不是只给 dynamic band 的 1000 个 active 名额；5 个 256-slot 物理页仅是存储实现，尾部 `1050..1279` 不属于逻辑地址空间。
- active 计数以**已发布且尚未完成注销的 runtime entity**为准：已注册的 active、dormant/merge shell 和 `pending-destroy` entity 都计入；尚未发布的 `pending-spawn`、未占用的 raw slot 以及已归还对象池且没有 runtime 注册的 shell 不计入。
- `pending-destroy` 在确定性注销边界完成前仍占用 active 预算和 runtime slot；不能因为已经标记销毁就提前释放容量。分配拒绝必须在发布前判断，不能先发布再回滚。
- 同一 tick 的释放与生成不依赖容器枚举顺序：在既定的 lifecycle mutation boundary 内，先按队列/slot 的确定顺序完成已到期注销，再按既定 producer/pass 顺序逐个进行 spawn admission 和发布；只有前一步已完成注销的 entity 才能为后一步释放容量。若生成发生在注销 boundary 之前，则按当时仍包含 `pending-destroy` 的计数判定并可确定性拒绝。
- 每次 spawn admission 成功后立即增加已发布计数；同一 boundary 后续 spawn 看到更新后的计数。移动端达到 1000 后，后续第 1001 个发布尝试稳定返回拒绝结果；Extended replay/checksum schema 尚未实现，当前 Extended Driver checksum 明确跳过/返回空值。

### X/Z Loose Quadtree Broadphase

**当前状态：B0 shadow 诊断、B2A formal backend 与 B2B generation-aware 增量同步均已实施，并通过 full self-check 与 architect final review；生产默认仍为 `BruteForce`。** `LooseQuadtree` 只有经显式命令行或 `GameConfig` 选择时才接管 formal backend；B2C 已随后接入即时 weapon/body query 与 AI 查询，此处旧“未迁移”结论已替代。

- 空间索引使用 X/Z 平面的 **Loose Quadtree**；逻辑实体、AI 范围查询和 itr/bdy 碰撞查询共享空间索引，但查询服务与候选规则分开，不能用 AI 范围结果替代碰撞候选。
- 实体中心点采用严格的**半开区间**归属（左/下含、右/上不含，边界规则全局一致），保证一个中心点只属于一个子节点。
- 实体 AABB 只有在完全被节点的 loose 范围容纳时才留在该节点；超出 loose 范围才迁移到父节点或重新选择的节点。
- 默认参数仅作为 profiling 基准，不能视为最终性能结论：`looseness = 1.5`、`leafCapacity = 16`、`maxDepth = 6..8`。目标设备和真实战斗分布 profiling 后再调整。
- 更新已采用 collect-boundary batch 增量策略：未移动实体保留原记录；AABB 改变但仍处于当前节点 loose 范围时原位更新；离开 loose 范围才迁移。生成、销毁、invalid AABB 和同槽 generation 复用在下一次 collision collect 同步，root escape 才触发全量重建；world reset 显式清空索引。
- broadphase 每 tick 先按 `RuntimeSlot` 升序遍历 active attacker；各 attacker 查询得到的候选先去重为 `(minSlot, maxSlot)` pair，再在全局按 `(minSlot, maxSlot)` 升序排序后交给现有 narrow phase。保留 C# 的 candidate 截断、距离/类型 tie 顺序和 pair 消费规则；空间索引不得改变命中规则、VRest 计时或最终逻辑结果。

### VRest 与 Parity 边界

**当前状态：B1.2 production lifecycle 与 B1.3 sparse tick 已验证；“Extended parity schema 未实施”为 B1.3 历史状态，已由 B2C 独立 Extended checksum 替代。** VRest tick 已移至独立 pass，eligibility 直接遍历 registered bucket items。

- VRest/ARest 的逻辑访问与 broadphase 解耦。空间索引减少候选枚举，不负责 VRest 的递减或过期；VRest 计时必须遍历自己的稀疏活动集合/到期结构，不能因 broadphase 未返回远距离 pair 而停止递减。
- 详细 parity snapshot（完整 slot、ARest/VRest、哈希和诊断字段）退出生产热路径，只在 `Authority400` 对拍、自检、回放或显式诊断模式中生成；生产 tick 不为 parity 预先扫描整页/全容量数据。
- Extended Driver 当前不生成 authority checksum，输出跳过/为空；direct parity capture 继续严格要求 `Authority400` Profile 且容量 400。Extended replay/checksum schema 必须另行设计，不能复用或伪装成旧 400-slot certificate。

## 1. 目标

建立只消费战斗逻辑快照的集中式表现后端，在不改变战斗结果的前提下，逐步替换战斗对象各自持有的 `Sprite` / `SpriteRenderer`：

- Loading 阶段完成 BMP 解码、依赖收集、图集规划和 GPU 资源创建，减少战斗中的资源创建与上传尖峰。
- 使用 source rect / UV 直接绘制，不再为每个图片格创建 `Sprite`。
- 将角色、武器、特殊攻击、其他对象、阴影和火花组织成一条确定顺序的 render command 流。
- 复用持久化 Mesh、顶点缓冲和 Material，避免逐帧 GameObject、Mesh、Sprite、Material 和临时容器分配。
- 通过多页图集和 `Texture2DArray` 减少透明绘制序列中的纹理切换与断批。
- 消除把 `logicalZ * 4096 + runtimeSlot * 4` 塞入 Unity `sortingOrder` 所产生的范围限制。
- 保留旧渲染后端作为迁移期回退，允许逐类切换和结果比对。

## 2. 非目标与边界

- 不改变 30 Hz 战斗逻辑 tick、pass 顺序、碰撞、输入、对象生成、命中结算或实体生命周期。
- 不以 `Transform`、插值位置、Renderer 状态或 GPU 结果反写战斗 runtime。
- 不把渲染帧变成战斗计数来源；参与规则的表现计数仍随逻辑 tick 推进。
- 不在本方案中实现完整联机、回滚、HUD、主菜单或通用场景渲染重构。
- 不以“每角色固定独占一张 2048 图集”作为最终物理布局；角色可以作为依赖收集根，但本局资源应统一装箱以避免空页和跨角色纹理切换。
- 不在本方案中处理或恢复 T8 默认 `stage.dat` 部署；T8 与本渲染方案无关，原暂缓状态不变。
- 第一阶段不承诺单次 draw call；透明正确性优先于极端合批。

## 3. 总体数据流

```text
Loading
data.txt / DAT / BMP
    -> BattleRenderDependencyCollector
    -> BattleAtlasLayoutPlanner
    -> BattleAtlasLoader
    -> Texture2DArray + BattleSpriteCatalog

Runtime（逻辑 tick）
只读 runtime 状态
    -> BattlePresentationSnapshot
    -> BattleRenderCommandBuilder
    -> 权威实体排序 + 实体内命令顺序
    -> BattleDynamicMeshBackend

Render（Unity 渲染帧）
最新完成的 Mesh / command segments
    -> BattleRenderFeature / BattleRenderPass
    -> 背景之后、后处理/UI 之前的目标注入点
```

资源准备、逻辑快照、绘制命令和 Unity 提交必须是明确边界。Loading 只准备表现资源；runtime 只提供只读真值；渲染后端不能成为战斗逻辑 owner。

## 4. 模块划分

| 模块 | 职责 |
|---|---|
| `BattleRenderDependencyCollector` | 从当前对局入口递归收集 DAT/BMP 表现依赖，按规范化路径去重 |
| `BattleAtlasLayoutPlanner` | 统计尺寸，使用确定性装箱算法生成 2048 多页布局 |
| `BattleAtlasLoader` | 解码 BMP，填充 `Texture2DArray`，上传 GPU，并在允许时释放 CPU 可读副本 |
| `BattleSpriteCatalog` | 将视觉对象和有效 pic 映射为 slice、UV、像素尺寸、pivot/中心等表现元数据 |
| `BattlePresentationSnapshot` | 在逻辑 tick 边界捕获渲染所需的只读字段 |
| `BattleRenderCommandBuilder` | 将快照展开为阴影、本体、覆盖物和命中记录等有序命令 |
| `BattleDynamicMeshBackend` | 复用 Mesh/缓冲，将命令写成 quad 顶点并形成连续渲染状态段 |
| `BattleRenderFeature` / `BattleRenderPass` | 在 URP 指定注入点提交有序 Mesh 段 |
| `LegacySpriteBackend` | 迁移期继续使用现有 `SpriteRenderer`，支持回退和 A/B 比对 |

名称只是当前建议，实施时应跟随仓库已有命名和目录边界。

## 5. Loading 依赖闭包

### 5.1 收集入口

`data.txt` 中 `type == 0` 可作为可玩角色 DAT 的资源收集根，但不能当作最终图集边界。一个角色可能通过 opoint、转换、分身、武器、技能体或 stage 生成引用 `type != 0` 的对象；公共阴影、火花、烟雾也可能位于角色 DAT 之外。

当前拟定收集流程：

1. 从本局角色和场景明确入口开始。
2. 读取每个 DAT 的 `LF2CharacterData.files`，收集其全部 BMP。
3. 递归追踪当前对局可达的 opoint、转换对象、武器、特殊攻击和固定表现资源。
4. 按规范化资源路径去重 BMP，而不是按 oid 或 DAT 去重。
5. 对无法静态闭合的动态引用建立明确的预加载清单或受控后备页，不允许在战斗热路径无界创建图集。

依赖闭包的准确规则在实施前仍需结合当前 Unity loader 与 C# 可达对象生成调用链逐项核对。

### 5.2 2048 多页图集

- Loading 阶段先统计本局全部去重 BMP 的尺寸，再运行确定性 MaxRects、Skyline 或等价装箱算法。
- 图集页固定为 `2048 x 2048`；超出一页时增加第二页及后续页面。
- 第一版优先装入完整 BMP sheet，保留 sheet 内格子布局，降低裁剪契约迁移风险。
- 所有同尺寸、同格式页面放入一个 `Texture2DArray`；顶点携带 `atlasSlice`，Shader 以 slice 选择页面。
- `Texture2DArray.depth` 创建后不能无损原地扩展，因此页数应在 Loading 规划结束后确定。
- BMP 大于页面、设备 slice 上限不足、格式不兼容或依赖漏收时必须产生可诊断失败或进入明确 fallback，不能静默显示错误图片。
- 设备不支持 Texture Array 时，回退为多个 `Texture2D`，但仍按原 painter 顺序生成连续纹理段，不按纹理重排对象。

RGBA32 的单张 2048 页面约占 16 MiB GPU 内存；若保持 readable，通常还会保留 CPU 副本。最终应根据目标 Android 格式、mipmap 策略、页数和设备上限制定预算，并在上传完成后按需调用 `Apply(false, true)` 释放 CPU 可读副本。

## 6. 图片索引与格子契约

图片查询使用 frame 的图片编号，不使用动作帧 ID：

```text
effectivePic = LF2FrameData.pic + Runtime.RenderPicOffset
```

然后在 `LF2CharacterData.files` 中找到包含 `effectivePic` 的文件区间：

```text
file.startFrame <= effectivePic <= file.endFrame
localPic = effectivePic - file.startFrame
```

格子按 DAT 现有契约换算：

```text
column     = localPic % columns
rowFromTop = localPic / columns
```

必须在实现前锁定并自动验证以下约束：

- `LF2FrameData.frameId` 是动作状态帧编号，不是图片格子索引。
- `LF2FrameData.pic` 才是图片编号；多个 frame 可以复用同一 pic。
- `RenderPicOffset` 参与最终显示图片查询。
- `pic == 999` 及其他现有无图语义不提交本体命令。
- 当前 DAT 的 `row` / `col` 命名与横纵格数的实际含义必须沿用现有 parser/loader 契约，不能按英文名猜测。
- 格子步长保留当前 sheet 的间隔像素：横纵方向按 `(w + 1, h + 1)` 推进，而不是只用 `(w, h)`。
- BMP 左上角编号与 Unity UV 原点方向不同；Catalog 负责一次性换算，runtime 不重复做易错的 Y 翻转。
- Catalog 同时保存像素宽高、中心/pivot 和必要裁剪元数据，使碰撞/逻辑尺寸不依赖运行时 `Sprite.rect`。

建议 Catalog 的稳定查询键为 `(visualDataId, effectivePic)` 或能唯一定位 DAT file range 的等价结构，结果至少包含 `atlasSlice`、`uvRect`、像素尺寸和 pivot。

## 7. PresentationSnapshot

`BattlePresentationSnapshot` 在逻辑 tick 完成后的稳定边界读取 runtime，只包含表现需要的数据，不持有可变 runtime 引用。候选字段包括：

```text
RuntimeSlot / StableId / Oid
ZInt / XInt / YInt / 表现高度字段
Frame / Pic / RenderPicOffset
Facing / Visible / Alpha / Tint
Shadow 与 overlay/hit-record 所需表现参数
```

最终字段必须从当前实际消费者倒推，不能把整个实体复制进快照。快照生成和消费需要避免逐 tick GC；使用双缓冲或环形缓冲，让 Unity 渲染帧只读取最后一个完整快照。渲染插值只能作用于表现坐标，不改变排序 key，不写回 runtime。

## 8. RenderCommand 与权威顺序

单条 `BattleRenderCommand` 的候选结构：

```text
CommandType
AtlasSlice / UVRect
Position / Size / Pivot
FlipX
Color / Alpha
BlendMode / MaterialVariant
RuntimeSlot / StableId / ZInt
```

全局实体顺序必须沿用 C# 权威可观察绘制顺序：

```text
Runtime.ZInt 升序
相同 ZInt 时 Runtime.SlotIndex 升序
```

对排序后的每个实体，命令按实体内顺序连续追加：

```text
Shadow -> Entity -> Overlay -> HitRecord
```

不得先画全体阴影、再画全体角色；也不得为凑图集或材质批次而跨实体重排透明命令。`YInt`、`displayZ`、`Zz`、shake 和类型专项视觉偏移只能影响顶点位置，不能替换 `(ZInt, RuntimeSlot)` 的全局顺序。

上述“权威”指最终可观察顺序必须与 `J:\QQFile\NTSD2.4\ntsd_release_C#` 对应绘制调用链一致。实施前需重新定位真实调用者、活动 slot 过滤、阴影/本体/覆盖物/命中记录的条件分支，并把证据加入对齐记录；本草案不代替该核验。

## 9. 持久化动态 Quad Mesh

每条可见命令写成一个 quad：4 个顶点、6 个固定索引、2 个三角形。顶点至少包含：

```text
position
uv
color
atlasSlice
```

“持久化”表示以下对象只初始化或扩容时创建，而不是逐帧创建：

- `Mesh`，并调用 `MarkDynamic()`。
- 顶点/索引缓冲和 CPU 侧复用数组或原生容器。
- 固定 quad 索引模板。
- 共享 Material 和 Shader variant。

每个逻辑 tick 或需要重建表现数据时：

1. 将已排序命令顺序写入复用顶点缓冲。
2. 使用 `Mesh.SetVertexBufferData` 或匹配当前 Unity 版本的低分配 API，仅上传活动顶点范围。
3. 更新实际 index count / submesh 或 chunk 范围。
4. 渲染帧重复提交最近完成的数据，不重复推进逻辑计数。

建议以 UInt16 索引限制为边界划分 chunk，例如每 chunk 4096 quad 对应 16384 顶点和 24576 索引；这只是实现候选，不是实体数量上限。命令数可能大于实体数，因为一个实体可以产生阴影、本体和多个附加命令。容量应按命令峰值监测，并在 Loading 预留或按明确策略扩容。

## 10. URP 提交

通过 `ScriptableRendererFeature` / `ScriptableRenderPass` 在战斗相机的确定注入点绘制集中式 Mesh。目标顺序是背景之后、需要参与的世界后处理之前、屏幕 UI 之前；准确 `RenderPassEvent` 需结合当前 URP Renderer 和相机栈验证。

战斗 Mesh 对 Unity 只需要稳定的整体层级。Mesh 内部的对象顺序由 render command 与索引/segment 顺序表达，不再将大范围逻辑 key 编码到 `sortingOrder`。相机裁剪、像素缩放、颜色空间、RenderTexture 和后处理必须在桌面与 Android 目标设备上分别验证。

## 11. 透明绘制与三种模式

默认使用透明混合和 `ZWrite Off`，并按 painter 顺序提交。阴影、烟雾、光效可能含半透明像素，因此不能未经素材和遮挡矩阵验证就统一改为 Alpha Clip 或 `ZWrite On`。

提供三级后端策略：

| 模式 | 说明 | 用途 |
|---|---|---|
| `SingleMesh` | 同一兼容渲染状态尽量由单 Mesh/少量 draw 提交 | 实验性研究候选；当前 P6 resolver 不允许进入生产选择，未来必须先通过目标 GPU 像素验证 |
| `OrderedChunks` | 严格保持命令顺序，只把相邻且状态兼容的命令合并为连续段 | 默认稳妥模式；状态变化时断批 |
| `StrictOrderedDraw` | 以更细粒度 draw 保证问题对象或设备的顺序 | 正确性回退和诊断模式 |

Alpha、Additive、Stencil、不同 Shader 或其他 GPU 状态必须断批；只能在原始命令流中切连续段，不能把不相邻的同材质命令抽出合并。Unity/目标 GPU 是否严格按单 Mesh 索引顺序处理所有透明三角形不能只靠桌面推断，必须在目标 Adreno、Mali 等设备用重叠像素场景验证。若结果不稳定，设备配置自动使用 `OrderedChunks` 或 `StrictOrderedDraw`。

## 12. 双后端迁移

迁移期建议保留以下模式：

```text
LegacyOnly
CentralShadowBuild（集中后端生成但不显示，用于命令/排序比对）
CentralOnly
```

切换顺序：

1. 先独立修复现有 `sortingOrder` 越界，使用活动实体紧凑 rank 或其他短期安全映射；不等待整套渲染重构。
2. 建立不依赖 `Sprite.rect` 的 `SpriteMetricsResolver` / Catalog 数据契约。
3. 建立 `BattleSpriteCatalog`，暂时继续由旧 `SpriteRenderer` 消费。
4. 建立 Snapshot 和 RenderCommand，在 shadow-build 下逐 tick 对比对象数量、图片、位置和顺序。
5. 接入持久动态 Quad Mesh 与 URP Pass，先迁移本体。
6. 依次迁移阴影、持有物、overlay、spark/hit record；每类都有旧后端对照。
7. 接入 2048 多页 `Texture2DArray` 和移动端压缩格式。
8. 完成目标 Android GPU 的正确性、内存和性能验收后，才考虑移除战斗 `SpriteRenderer`。

旧后端与新后端不能同时对同一类别实际出图，避免重复显示；shadow-build 只记录/比较，不提交像素。

## 13. 分阶段计划

| 阶段 | 产物 | 进入下一阶段的门槛 |
|---|---|---|
| P0 契约核验 | C# 绘制调用链、Unity 当前消费者、slot/排序/格子契约清单 | 用户确认知识点和总体设计；证据可定位 |
| P1 排序止血 | 当前后端不越界的紧凑排序映射与 focused check | 编译、自检、重叠对象 Play 验证通过 |
| P2 Catalog | BMP/file/pic 到 metrics/UV 的唯一查询层，旧后端消费 | 全部代表性 DAT 的图片索引矩阵通过 |
| P3 Command shadow-build | Snapshot、命令生成、旧/新顺序对比工具 | 多对象、多 Z、同 Z、生成/回收场景逐 tick 等价 |
| P4 Mesh/URP | 持久 Mesh、Shader、URP Pass、OrderedChunks | 桌面像素基线与 Play 场景通过，无逐帧 GC 回归 |
| P5 Atlas Array | 确定性多页装箱、Texture2DArray、fallback | 图集覆盖、内存预算、设备能力与漏依赖处理通过 |
| P6 移动端验收 | Adreno/Mali 真机结果、模式选择与性能报告 | 正确性矩阵通过，性能/内存达到项目预算 |
| P7 收口 | CentralOnly 默认，旧后端移除条件评审 | 回退期完成且长期场景无差异后单独批准 |

每个阶段都应是可回退、可验证的独立提交；不能以最终架构目标跳过中间的可观察行为对比。

## 14. 验收矩阵

| 维度 | 最低检查 |
|---|---|
| 编译 | Unity 2022.3.4f1c1 脚本编译 0 error |
| 自动自检 | 资源索引、file range 边界、`RenderPicOffset`、`pic=999`、row/col、`w+1/h+1`、排序和容量 focused checks |
| 逻辑隔离 | 启用/禁用新后端时 battle checksum 和 runtime 字段完全不变 |
| 图片正确性 | 每个代表性 DAT 的首格、行尾、下一行、file range 首尾、offset、翻面、pivot 像素对照 |
| 层级正确性 | 不同 Z、同 Z 不同 slot、实体交错阴影、持有物、overlay、hit record 的重叠截图/像素断言 |
| 生命周期 | spawn、回收、复用、变身、分身、武器持有/释放后无旧图、错图或残留命令 |
| 透明状态 | Alpha、Additive、Stencil/特殊 Shader 按原命令流断段且不重排 |
| 容量 | 0 实体、常规负载、峰值命令、超过预留容量、跨 UInt16 chunk 边界 |
| 设备兼容 | Texture Array 支持/不支持、slice 上限、Adreno/Mali 的 array/fallback 像素结果；`SingleMesh` 仅作非生产研究 |
| 性能 | Loading 时间、CPU/GPU 内存、上传峰值、draw call、SetPass、主线程耗时、GC alloc |
| 回退 | `LegacyOnly`、shadow-build、`CentralOnly` 可控切换，故障设备可降级 |

最终报告必须分别标记“方案确认”“逻辑已写”“编译通过”“self-check 通过”“Play Mode 通过”“目标 Android 真机通过”，不得互相代替。

## 15. 主要风险与待确认项

- **依赖漏收**：动态 opoint/转换/stage 引用未进入 Loading 闭包，会导致战斗中缺图。需要权威调用链和生产 DAT 扫描共同闭合。
- **内存预算**：2048 RGBA32 页面约 16 MiB；页面过多、CPU readable 副本和 mipmap 会迅速扩大占用。
- **纹理格式**：运行时拼图与 ASTC/ETC2 构建期压缩的组合方式、颜色空间和 alpha 质量尚待技术验证。
- **透明顺序**：单 Mesh 内透明三角形的实际执行顺序需要目标 GPU 像素验证；不能只以 draw call 数量判定正确。
- **状态断批**：不同 blend/stencil/shader 仍会产生 draw；图集只能消除纹理页切换，不能合并不兼容 GPU 状态。
- **页边缘采样**：线性过滤、mipmap 和 atlas bleeding 需要 padding/extrusion 策略；原 BMP 格子的一像素分隔不能直接等同安全 atlas padding。
- **像素坐标与 pivot**：BMP 顶左编号、Unity UV 原点、翻面和中心点若分散换算，容易出现一像素偏移；应集中到 Catalog 并做边界测试。
- **容量误读**：`400` 必须保留为 `Authority400` 的兼容边界，但不能继续解释为所有 Unity 模式的固定 runtime 槽位上限。slot address 容量、active entity 预算和 render command 数是三个不同概念；移动端 1000 active 或桌面分页增长都不代表同数量的绘制命令。每实体可能展开为阴影、本体、覆盖物和命中记录等多条命令，因此 Mesh 容量与 chunk 边界必须按 render command 峰值独立设计。
- **URP 注入点**：相机栈、后处理、RenderTexture 和 UI 的现状需要实际工程验证。
- **API/平台约束**：正式实现前应查阅 Unity 2022.3 对 `Texture2DArray`、`Mesh.SetVertexBufferData`、URP Renderer Feature/Pass 和移动平台纹理格式的官方文档。
- **迁移双维护**：旧/新后端并存会增加短期复杂度，需要清晰的类别 ownership 和移除门槛。

## 16. 当前决策记录

已确认的设计决策是：保留 `Authority400` 兼容模式；移动端全部槽区合计最多 1000 active 且第 1001 个确定性拒绝；桌面从 512 开始按 256-slot 页自动增长并受技术预算约束；空闲槽使用二叉最小堆 + `nextUnused`；B0 先以 X/Z Loose Quadtree shadow 诊断对比，B2A 提供 formal full-rebuild backend，B2B 再以 `(slot, generation)` 身份在 collision collect 边界实施 batch 增量同步，默认仍为 `BruteForce`；VRest 与 broadphase 解耦；详细 parity snapshot 不进入生产热路径。生产 Profile 优先级为命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认，broadphase 独立遵循命令行 > `GameConfig` > 默认 `BruteForce`；设备能力只降级表现资源/后端，三个 Profile 共用同一套确定性 runtime 算法。

截至 2026-07-20，R1-R2C-4、B0、B1-B1.3、B2A 与 B2B 已完成代码层实施和既定验证。B2B generation-aware incremental backend 的 fresh chain 为 source `22:43:57` < DLL `22:46:36` < result `22:47:04` **PASS**，dotnet **0 errors**，architect final **PASS / no blocker**。该段“即时 weapon/body query、AI 查询、Extended parity schema 仍是后续任务”为 B2B 历史状态，已由 B2C 替代；B2C 最新 full self-check `2026-07-21 00:48:06` **PASS**、dotnet **0 errors / 42 existing warnings**，但未执行 Play Mode、性能或 fresh Architect PASS。生产默认仍为 `BruteForce`，集中式渲染与 T8 默认 `stage.dat` 部署仍暂缓。

## 17. Central Presentation Mount v1（2026-07-22 历史快照，已由文档顶部 rendererless 收口取代）

- **范围和实现状态：**已新增 `BattleCentralPresentationMount` 与 `BattleCentralPresentationMountRegistry`，并由 `LF2ObjectRenderer` 集成。该 v1 只完成 mount 的声明、注册，以及 generation-aware `RuntimeEntityHandle` 绑定；它没有加入渲染、资源加载、`Update`、渲染命令或任何战斗 runtime 改动。
- **生命周期接线：**`SimulationWorld` 在实体注册时登记 mount，在 release 和 reset 时释放/清理登记，避免 slot 复用把旧实体或旧 generation 绑定到新实体。disable -> enable restore 与 rollback clear 均已关闭并纳入 self-check 覆盖；本批新增并通过了针对 world `ResetRuntimeState` 和 registration rollback 的 focused checks。自检同时覆盖 renderer 集成、world register/release/reset，以及 handle generation 失效后的绑定边界。
- **明确未变更项（历史）：**此 v1 批次当时没有编辑 prefab，`Legacy` 表现路径仍保留；它不能单独表述为 CentralOnly 像素接管、资源接管或 Legacy 移除。该限制已由文档顶部的后续 rendererless prefab 接线和 Play Mode 验证取代。
- **最终验证证据：**relevant source `2026-07-22 11:48:18` < Unity `Assembly-CSharp.dll` `11:49:08` < `Temp/NTSD_BattleRuntimeSelfCheck.result` **PASS** `11:50:11`。最终完整命令 `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 完成，结果为 **0 errors / 42 existing warnings**。Architect closure 为 **PASS / no P0-P2**。Console 清空后仍有两类预期的 self-check-active Error：既有 mismatched release，以及新的 registration rollback；这明确不是 Console 0 errors，后续报告不得写成 0 errors。
- **后续历史步骤：**当时计划在 `EntityObject` 的 `EntityModel` 与 `Shadow` nodes 挂载 mount component 并配置 `ownerRenderer`；该步骤已在本轮 rendererless 收口中完成。


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

Review P8-D v4 implementation as an architect. Do not edit files.

Scope:
- Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkEditorTests.cs
- Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkWindow.cs

Requirements to audit:
1. PASS must require every applicable mandatory metric for exactly config.SampleFrames completed frames.
2. FrameTimingManager/ProfilerRecorder sampling must be completed-frame based and generation isolated, not same-Update LastValue mixing.
3. Required metrics: CPU frame/main, GPU, GC allocation, actual full-frame draw calls, total/graphics/texture memory, logic tick time/allocation, presentation build, submitted items, resource segments, owned memory, exact count, runtime/determinism gates. Render-thread may be NotApplicable only when SystemInfo.graphicsMultiThreaded is false. Central local DrawMesh count/mesh chunks are required only for Central; Legacy local equivalents are NotApplicable, actual drawCalls remains required.
4. EditMode/unsupported capability must be Unsupported or Incomplete, never PASS. Leak requested with unavailable graphics memory must not PASS.
5. A/B suite must switch the actual SimulationWorld backend and restore it on completion, dispose, and exception; no cross-generation pending sample contamination.
6. Windows Development Player must enable frame timing stats only during build and restore prior settings.
7. Focused tests must actually cover missing metrics and v4 schema/verdict.

Use exact file/line references. Findings first, severity P0/P1/P2/P3, then residual risks and a concise verdict. Treat existing v3 reports as stale.
