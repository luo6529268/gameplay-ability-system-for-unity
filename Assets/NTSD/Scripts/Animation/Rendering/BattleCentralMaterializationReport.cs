using System;
using System.Collections.Generic;
using NTSD.Simulation;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleCentralMaterializationReport
    {
        private const string CumulativeScope = "cumulative-since-reset";
        private const string WindowScope = "window-delta";
        private static readonly string[] BuildKeys =
        {
            "buildInvocationCount", "admittedBuildCount", "completedBuildCount", "rejectedBeforeBuildCount",
            "failedBuildCount", "vertexUploadCallCount", "uploadedVertexCount", "uploadedVertexBytes",
        };

        private readonly object sourceToken;
        private readonly long[] values;
        private readonly long[] windowStart;
        private readonly long[] windowEnd;

        private BattleCentralMaterializationReport(
            object sourceToken, long counterEpoch, string scope, long[] values,
            long[] windowStart = null, long[] windowEnd = null)
        {
            this.sourceToken = sourceToken;
            CounterEpoch = counterEpoch;
            Scope = scope;
            this.values = values;
            this.windowStart = windowStart;
            this.windowEnd = windowEnd;
        }

        public long CounterEpoch { get; }
        public string Scope { get; }

        // Explicit main-thread diagnostic read; copying/exporting is not a zero-allocation hot path.
        internal static BattleCentralMaterializationReport Capture(BattleCentralRuntimeDiagnostics diagnostics)
        {
            var values = new long[43];
            values[0] = diagnostics.MaterializationRequestCount;
            values[1] = diagnostics.PublicationChangedRequestCount;
            values[2] = diagnostics.AlphaChangedRequestCount;
            values[3] = diagnostics.RepeatedSampleRequestCount;
            values[4] = diagnostics.MaterializationAttemptCount;
            values[5] = diagnostics.PublicationChangedAttemptCount;
            values[6] = diagnostics.AlphaChangedAttemptCount;
            values[7] = diagnostics.RepeatedSampleAttemptCount;
            values[8] = diagnostics.SameUnityFrameReuseCount;
            values[9] = diagnostics.SameSampleReuseCount;
            values[10] = diagnostics.ReentrantSkipCount;
            CopyBuild(diagnostics.MaterializationBuilds, values, 11);
            CopyBuild(diagnostics.PublicationChangedBuilds, values, 19);
            CopyBuild(diagnostics.AlphaChangedBuilds, values, 27);
            CopyBuild(diagnostics.RepeatedSampleBuilds, values, 35);
            return new BattleCentralMaterializationReport(
                diagnostics.MaterializationReportSourceToken, diagnostics.MaterializationCounterEpoch,
                CumulativeScope, values);
        }

        public bool TryCreateWindow(
            BattleCentralMaterializationReport baseline,
            out BattleCentralMaterializationReport report,
            out string reason)
        {
            report = null;
            if (baseline == null)
            {
                reason = "A cumulative baseline is required.";
                return false;
            }
            if (Scope != CumulativeScope || baseline.Scope != CumulativeScope)
            {
                reason = "Window endpoints must both be cumulative snapshots.";
                return false;
            }
            if (!ReferenceEquals(sourceToken, baseline.sourceToken))
            {
                reason = "Counter source changed.";
                return false;
            }
            if (CounterEpoch != baseline.CounterEpoch)
            {
                reason = "Counter epoch changed; reset invalidates the baseline.";
                return false;
            }
            for (int index = 0; index < values.Length; index++)
            {
                if (values[index] < baseline.values[index])
                {
                    reason = "A counter decreased; the whole window is rejected.";
                    return false;
                }
            }

            var delta = new long[values.Length];
            for (int index = 0; index < values.Length; index++)
                delta[index] = values[index] - baseline.values[index];
            report = new BattleCentralMaterializationReport(
                sourceToken, CounterEpoch, WindowScope, delta, baseline.values, values);
            reason = string.Empty;
            return true;
        }

        public Dictionary<string, object> ToProjection()
        {
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["schemaVersion"] = "ntsd-central-materialization-counters-v1",
                ["scope"] = Scope,
                ["counterEpoch"] = CounterEpoch,
                ["collectionContext"] = "explicit-main-thread-snapshot-of-production-queued-materialization-counters",
                ["units"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["requestAttemptGateAndBuildCounts"] = "count",
                    ["vertexUploadCallCount"] = "count",
                    ["uploadedVertexCount"] = "vertices",
                    ["uploadedVertexBytes"] = "bytes",
                },
                ["counts"] = ProjectCounts(values),
                ["limitations"] =
                    "Counters only; not GPU traffic, GPU batch, successful pixels, frame-time or device certification. " +
                    "Request/Prepare/return-gate counts are not successful rendering. Completed Build may have zero payload; " +
                    "failed Build includes only completed vertex-upload APIs, never permission for partial submission. " +
                    "Entity Mesh vertex payload only; excludes Foot/Health, index/submesh, RenderPass and benchmark-local. " +
                    "Snapshot/JSON export allocates outside the hot path; caller must exclude writer concurrency. " +
                    "Scope is a reset epoch or counter delta, not a measured frame/tick/time denominator or complete M0.",
            };
            if (windowStart != null)
            {
                root["window"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["start"] = ProjectCounts(windowStart),
                    ["end"] = ProjectCounts(windowEnd),
                };
            }
            return root;
        }

        public string ToJson()
        {
            return BattleCanonicalJson.Serialize(ToProjection());
        }

        private static void CopyBuild(BattleCentralMaterializationBuildCounters counters, long[] target, int offset)
        {
            target[offset] = counters.BuildInvocationCount;
            target[offset + 1] = counters.AdmittedBuildCount;
            target[offset + 2] = counters.CompletedBuildCount;
            target[offset + 3] = counters.RejectedBeforeBuildCount;
            target[offset + 4] = counters.FailedBuildCount;
            target[offset + 5] = counters.VertexUploadCallCount;
            target[offset + 6] = counters.UploadedVertexCount;
            target[offset + 7] = counters.UploadedVertexBytes;
        }

        private static Dictionary<string, object> ProjectCounts(long[] source)
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["requests"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["total"] = source[0],
                    ["publicationChanged"] = source[1],
                    ["alphaChanged"] = source[2],
                    ["repeated"] = source[3],
                },
                ["prepareAttempts"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["total"] = source[4],
                    ["publicationChanged"] = source[5],
                    ["alphaChanged"] = source[6],
                    ["repeated"] = source[7],
                },
                ["returnGates"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["sameUnityFrame"] = source[8],
                    ["sameSample"] = source[9],
                    ["reentrant"] = source[10],
                },
                ["builds"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["total"] = ProjectBuild(source, 11),
                    ["publicationChanged"] = ProjectBuild(source, 19),
                    ["alphaChanged"] = ProjectBuild(source, 27),
                    ["repeated"] = ProjectBuild(source, 35),
                },
            };
        }

        private static Dictionary<string, object> ProjectBuild(long[] source, int offset)
        {
            var result = new Dictionary<string, object>(BuildKeys.Length, StringComparer.Ordinal);
            for (int index = 0; index < BuildKeys.Length; index++)
                result.Add(BuildKeys[index], source[offset + index]);
            return result;
        }
    }
}
