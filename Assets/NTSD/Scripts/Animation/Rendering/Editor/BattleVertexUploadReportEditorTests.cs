#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleVertexUploadReportEditorTests
    {
        [Test]
        public void Snapshot_FreezesSuccessfulPresentPayload_WithUnitsAndLongBytes()
        {
            var fixture = new Fixture();
            using (fixture.Session)
            {
                fixture.Presenter.SetPayload(3, 123, 4294967300L);
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                fixture.Presenter.SetPayload(99, 999, 999);
                Complete(fixture.Session);
                AssertPayload(fixture.Session.Report.Frames[0], 3, 123, 4294967300L);
            }
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        public void InapplicableOrMissingDiagnostics_AreUnavailable_NotMeasuredZero(
            bool central, bool diagnostics)
        {
            var fixture = new Fixture(central: central, diagnostics: diagnostics);
            using (fixture.Session)
            {
                Complete(fixture.Session);
                foreach (string name in PropertyNames)
                {
                    BattleBenchmarkMetric metric = ReadMetric(fixture.Session.Report.Frames[0], name);
                    Assert.That(metric.Available, Is.False);
                    Assert.That(metric.Unit, Is.EqualTo(name.EndsWith("Bytes") ? "bytes" : "count"));
                }
                foreach (string key in ProjectionNames)
                {
                    Dictionary<string, object> summary = Summary(fixture.Session.Report, key);
                    Assert.That(summary["available"], Is.False);
                    Assert.That(summary["sampleCount"], Is.EqualTo(0));
                    Assert.That(summary["average"], Is.Null);
                }
            }
        }

        [Test]
        public void AvailableZeroPayload_IsAnObservedZero()
        {
            var fixture = new Fixture();
            using (fixture.Session)
            {
                Complete(fixture.Session);
                AssertPayload(fixture.Session.Report.Frames[0], 0, 0, 0);
                Assert.That(Summary(fixture.Session.Report, ProjectionNames[2])["average"], Is.EqualTo(0d));
            }
        }

        [Test]
        public void IndependentSamples_ArePerBuild_NotAccumulated()
        {
            var fixture = new Fixture(samples: 2);
            using (fixture.Session)
            {
                fixture.Presenter.SetPayload(2, 8, 352);
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                fixture.Presenter.SetPayload(1, 4, 176);
                Complete(fixture.Session);
                AssertPayload(fixture.Session.Report.Frames[0], 2, 8, 352);
                AssertPayload(fixture.Session.Report.Frames[1], 1, 4, 176);
                Assert.That(Summary(fixture.Session.Report, ProjectionNames[2])["average"], Is.EqualTo(264d));
                Assert.That(Summary(fixture.Session.Report, ProjectionNames[2])["sampleCount"], Is.EqualTo(2));
            }
        }

        [Test]
        public void WarmupPayload_IsExcludedFromAcceptedSampleSummary()
        {
            var fixture = new Fixture(warmup: 1);
            using (fixture.Session)
            {
                fixture.Presenter.SetPayload(9, 36, 1584);
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                fixture.Presenter.SetPayload(1, 4, 176);
                Complete(fixture.Session);
                Assert.That(fixture.Presenter.PresentCount, Is.EqualTo(2));
                AssertPayload(fixture.Session.Report.Frames[0], 1, 4, 176);
                Assert.That(Summary(fixture.Session.Report, ProjectionNames[2])["average"], Is.EqualTo(176d));
            }
        }

        [Test]
        public void RejectedCompletedFrame_UsesRetrySnapshot_NotRejectedAttempt()
        {
            var fixture = new Fixture(rejectFirst: true);
            using (fixture.Session)
            {
                fixture.Presenter.SetPayload(9, 36, 1584);
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                fixture.Presenter.SetPayload(1, 4, 176);
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                fixture.Presenter.SetPayload(99, 396, 17424);
                Complete(fixture.Session);
                Assert.That(fixture.Session.Report.CompletedFrameRejectedAttemptCount, Is.EqualTo(1));
                Assert.That(fixture.Session.Report.Frames.Count, Is.EqualTo(1));
                AssertPayload(fixture.Session.Report.Frames[0], 1, 4, 176);
            }
        }

        [Test]
        public void DelayedDrain_DoesNotPresentAgain_OrReadLivePayload()
        {
            var fixture = new Fixture();
            using (fixture.Session)
            {
                fixture.Presenter.SetPayload(1, 4, 176);
                fixture.Collector.BlockDrain = true;
                Assert.That(fixture.Session.CaptureFrame(), Is.False);
                fixture.Presenter.SetPayload(9, 36, 1584);
                for (int index = 0; index < 3; index++)
                    Assert.That(fixture.Session.CaptureFrame(), Is.False);
                Assert.That(fixture.Presenter.PresentCount, Is.EqualTo(1));
                fixture.Collector.BlockDrain = false;
                Complete(fixture.Session);
                AssertPayload(fixture.Session.Report.Frames[0], 1, 4, 176);
            }
        }

        [Test]
        public void FrameAndSummaryProjection_RemainFrozen_AfterDiagnosticsChange()
        {
            var fixture = new Fixture();
            using (fixture.Session)
            {
                fixture.Presenter.SetPayload(1, 4, 176);
                Complete(fixture.Session);
                fixture.Presenter.SetPayload(99, 999, 999);
                Dictionary<string, object> frame = fixture.Session.Report.Frames[0].ToProjection();
                for (int index = 0; index < ProjectionNames.Length; index++)
                {
                    var metric = (Dictionary<string, object>)frame[ProjectionNames[index]];
                    Assert.That(metric["available"], Is.True);
                    Assert.That(metric["value"], Is.EqualTo(new double[] { 1, 4, 176 }[index]));
                }
                Assert.That(Summary(fixture.Session.Report, ProjectionNames[2])["maximum"], Is.EqualTo(176d));
                Assert.That(fixture.Session.Report.ToJson(), Does.Contain("\"centralUploadedVertexBytes\""));
            }
        }

        [Test]
        public void OptionalDiagnostics_DoNotChangeV5MandatorySchema_OrVerdict()
        {
            var fixture = new Fixture();
            using (fixture.Session)
            {
                Complete(fixture.Session);
                AssertPayload(fixture.Session.Report.Frames[0], 0, 0, 0);
                Assert.That(fixture.Session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
                Assert.That(BattleRenderingBenchmarkVerdictPolicy.PolicyId,
                    Is.EqualTo("ntsd-battle-rendering-benchmark-policy-v5"));
                var actualNames = new List<string>();
                foreach (BattleBenchmarkMetricAvailability metric in fixture.Session.Report.MetricAvailability)
                    actualNames.Add(metric.Metric);
                Assert.That(actualNames, Is.EquivalentTo(BattleRenderingBenchmarkVerdictPolicy.RequiredMetricNames));
                foreach (string key in ProjectionNames)
                    Assert.That(actualNames, Does.Not.Contain(key));
                Assert.That(fixture.Session.Report.ToProjection(false)["schema"],
                    Is.EqualTo("ntsd-battle-rendering-benchmark-run-v5"));
            }
        }

        [Test]
        public void Scope_ExplicitlyExcludesGpuProductionAndRejectedAttempts()
        {
            var fixture = new Fixture();
            using (fixture.Session)
            {
                Complete(fixture.Session);
                var limitations = (Dictionary<string, object>)fixture.Session.Report.ToProjection(false)["limitations"];
                Assert.That(limitations.ContainsKey("centralVertexUploadScope"), Is.True);
                string scope = (string)limitations["centralVertexUploadScope"];
                foreach (string clause in new[]
                {
                    "benchmark-local", "Mesh API", "accepted completed-frame", "warmup",
                    "rejected", "not GPU", "production RenderPass", "whole-run total", "unavailable"
                })
                    Assert.That(scope, Does.Contain(clause));
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void SnapshotHelper_AfterPreparation_AllocatesZeroManagedBytes(bool central)
        {
            var fixture = new Fixture(central: central);
            using (fixture.Session)
            {
                MethodInfo method = typeof(BattleRenderingBenchmarkSession).GetMethod(
                    "SnapshotVertexUploadMetrics", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(method, Is.Not.Null, "Missing upload snapshot helper.");
                var snapshot = (Action)Delegate.CreateDelegate(typeof(Action), fixture.Session, method);
                for (int index = 0; index < 32; index++)
                    snapshot();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int index = 0; index < 128; index++)
                    snapshot();
                long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(allocated, Is.EqualTo(0L));
                Complete(fixture.Session);
                Assert.That(ReadMetric(fixture.Session.Report.Frames[0], PropertyNames[0]).Available,
                    Is.EqualTo(central));
            }
        }

        [Test]
        public void ThrowingPresent_CommitsNoSample_AndNextSuccessUsesFreshPayload()
        {
            var fixture = new Fixture();
            using (fixture.Session)
            {
                fixture.Presenter.ThrowNext = true;
                fixture.Presenter.SetPayload(9, 36, 1584);
                Assert.Throws<InvalidOperationException>(() => fixture.Session.CaptureFrame());
                Assert.That(fixture.Session.SampleFramesCaptured, Is.EqualTo(0));
                Assert.That(fixture.Collector.ResetCount, Is.EqualTo(1));
                fixture.Presenter.SetPayload(1, 4, 176);
                Complete(fixture.Session);
                AssertPayload(fixture.Session.Report.Frames[0], 1, 4, 176);
            }
        }

        private static readonly string[] PropertyNames =
        {
            "CentralVertexUploadCalls", "CentralUploadedVertices", "CentralUploadedVertexBytes"
        };

        private static readonly string[] ProjectionNames =
        {
            "centralVertexUploadCalls", "centralUploadedVertices", "centralUploadedVertexBytes"
        };

        private static BattleBenchmarkMetric ReadMetric(BattleRenderingBenchmarkFrame frame, string name)
        {
            PropertyInfo property = typeof(BattleRenderingBenchmarkFrame).GetProperty(name);
            Assert.That(property, Is.Not.Null, $"Missing optional metric {name}.");
            return (BattleBenchmarkMetric)property.GetValue(frame);
        }

        private static void AssertPayload(BattleRenderingBenchmarkFrame frame, int calls, long vertices, long bytes)
        {
            var expected = new double[] { calls, vertices, bytes };
            for (int index = 0; index < PropertyNames.Length; index++)
            {
                BattleBenchmarkMetric metric = ReadMetric(frame, PropertyNames[index]);
                Assert.That(metric.Available, Is.True);
                Assert.That(metric.Value, Is.EqualTo(expected[index]));
                Assert.That(metric.Unit, Is.EqualTo(index == 2 ? "bytes" : "count"));
            }
        }

        private static Dictionary<string, object> Summary(BattleRenderingBenchmarkReport report, string key)
        {
            var summary = (Dictionary<string, object>)report.ToProjection(false)["summary"];
            Assert.That(summary.ContainsKey(key), Is.True, $"Missing optional summary {key}.");
            return (Dictionary<string, object>)summary[key];
        }

        private static void Complete(BattleRenderingBenchmarkSession session)
        {
            for (int guard = 0; guard < 32; guard++)
            {
                if (session.CaptureFrame())
                    return;
            }
            Assert.Fail("Injected capture did not complete in the bounded test window.");
        }

        private sealed class Fixture
        {
            internal readonly Presenter Presenter;
            internal readonly Collector Collector;
            internal readonly BattleRenderingBenchmarkSession Session;

            internal Fixture(int samples = 1, int warmup = 0, bool central = true,
                bool diagnostics = true, bool rejectFirst = false)
            {
                BattlePresentationBackendMode mode = central
                    ? BattlePresentationBackendMode.CentralOnly : BattlePresentationBackendMode.LegacyOnly;
                var config = new BattleRenderingBenchmarkConfig(mode, warmup, samples, "100", string.Empty);
                BattleRenderingBenchmarkWorkload workload = BattleRenderingBenchmarkWorkload.Create(
                    config.Scenario, null, 0, samples);
                Presenter = new Presenter(workload, central, diagnostics);
                Collector = new Collector(rejectFirst, !central || diagnostics);
                // Synthetic policy context only: the real Editor stays outside Play, with no Profiler collector.
                Session = new BattleRenderingBenchmarkSession(config, new SimulationWorld(), workload,
                    new BattleRenderingBenchmarkPolicyContext(true, true, RuntimePlatform.WindowsEditor, true, true),
                    Collector, Presenter);
            }
        }

        private sealed class Presenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;
            private readonly bool central;
            internal int PresentCount;
            internal bool ThrowNext;
            public BattleCentralBuildDiagnostics Diagnostics { get; }

            internal Presenter(BattleRenderingBenchmarkWorkload workload, bool central, bool diagnostics)
            {
                this.workload = workload;
                this.central = central;
                if (diagnostics)
                    Diagnostics = new BattleCentralBuildDiagnostics();
            }

            internal void SetPayload(int calls, long vertices, long bytes)
            {
                Diagnostics.VertexUploadCallCount = calls;
                Diagnostics.UploadedVertexCount = vertices;
                Diagnostics.UploadedVertexBytes = bytes;
            }

            public string Implementation => "InjectedVertexUploadPresenter";
            public string EffectiveBackend => central ? "CentralOnly" : "LegacyOnly";
            public string ResourceMode => "Injected";
            public string DrawMode => "Injected";
            public int RenderTargetWidth => 256;
            public int RenderTargetHeight => 256;
            public int ResolvedCommandCount => workload.CommandCount;
            public int MaterializedRenderItemCount => workload.CommandCount;
            public int ResourceSegmentCount => 7;
            public int SubmissionDrawCount => central ? 7 : -1;
            public string SubmissionDrawMetricSource => "injected benchmark-local calls";
            public string SubmissionDrawUnavailableReason => "Legacy local calls are not applicable.";
            public int ResourceGeneration => 7;
            public int OwnedTextureResourceCount => 2;
            public int OwnedResourceCount => 1;
            public long CachedOwnedResourceMemoryBytes => 256;
            public long MeasureOwnedResourceMemoryBytes() => 256;
            public long MeasureOwnedTextureMemoryBytes() => 128;
            public double Present()
            {
                PresentCount++;
                if (ThrowNext)
                {
                    ThrowNext = false;
                    throw new InvalidOperationException("Injected failed Present.");
                }
                return 0.25;
            }
            public void Dispose() { }
        }

        private sealed class Collector : IBattleBenchmarkCompletedFrameCollector
        {
            private readonly bool rejectFirst;
            private readonly bool supported;
            private int pendingGeneration;
            private int drained;
            internal bool BlockDrain;
            internal int ResetCount;
            internal Collector(bool rejectFirst, bool supported)
            {
                this.rejectFirst = rejectFirst;
                this.supported = supported;
            }
            public bool IsSupported => supported;
            public string UnsupportedReason => supported ? string.Empty : "Injected diagnostic-only capture.";
            public void Request(int generation)
            {
                if (pendingGeneration != 0)
                    throw new InvalidOperationException("Sample already pending.");
                pendingGeneration = generation;
            }
            public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics)
            {
                metrics = default;
                if (BlockDrain || pendingGeneration != generation)
                    return false;
                pendingGeneration = 0;
                bool reject = rejectFirst && drained++ == 0;
                metrics = new BattleBenchmarkCompletedFrameMetrics(
                    BattleBenchmarkMetric.FromValue(16, "ms"), BattleBenchmarkMetric.FromValue(8, "ms"),
                    BattleBenchmarkMetric.FromValue(2, "ms"),
                    reject ? BattleBenchmarkMetric.Unavailable("ms") : BattleBenchmarkMetric.FromValue(5, "ms"),
                    BattleBenchmarkMetric.FromValue(0, "bytes"), BattleBenchmarkMetric.FromValue(7, "count"),
                    BattleBenchmarkMetric.FromValue(1024, "bytes"), BattleBenchmarkMetric.FromValue(2048, "bytes"));
                return true;
            }
            public string Source(BattleBenchmarkRecorderKind kind) => "injected counters, not GPU measurement";
            public string Reason(BattleBenchmarkRecorderKind kind) => string.Empty;
            public void Reset() { ResetCount++; pendingGeneration = 0; }
            public void Dispose() => Reset();
        }
    }
}
#endif
