#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEditor;

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
                StringAssert.Contains("ntsd-battle-rendering-benchmark-run-v5", first);
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
        public void CompletedFrameAttribution_SnapshotsCountersOnce_AndRejectsStaleGenerations()
        {
            var attribution = new BattleBenchmarkCompletedFrameAttribution();
            attribution.Request(10, 100, 500UL);

            Assert.That(attribution.ShouldSnapshotCounters(9, 101), Is.False);
            Assert.That(attribution.ShouldSnapshotCounters(10, 100), Is.False);
            Assert.That(attribution.ShouldSnapshotCounters(10, 101), Is.True);
            Assert.That(attribution.ShouldSnapshotCounters(10, 102), Is.False);
            Assert.That(attribution.TryAcceptTiming(9, 501UL), Is.False);
            Assert.That(attribution.TryAcceptTiming(10, 500UL), Is.False);
            Assert.That(attribution.TryAcceptTiming(10, 501UL), Is.True);

            attribution.Request(11, 200, 400UL);
            Assert.That(attribution.ShouldSnapshotCounters(11, 201), Is.True);
            Assert.That(attribution.TryAcceptTiming(11, 501UL), Is.False,
                "the previous accepted frame timestamp is also a monotonic watermark");
            Assert.That(attribution.TryAcceptTiming(11, 502UL), Is.True);
        }

        [Test]
        public void PositiveCounterPolicy_RejectsZeroCompletedFrameSample()
        {
            BattleBenchmarkMetric zero = BattleBenchmarkCounterSamplePolicy.Capture(
                true,
                1,
                0L,
                "bytes",
                true,
                out string zeroReason);
            BattleBenchmarkMetric missing = BattleBenchmarkCounterSamplePolicy.Capture(
                true,
                0,
                1024L,
                "bytes",
                true,
                out string missingReason);
            BattleBenchmarkMetric positive = BattleBenchmarkCounterSamplePolicy.Capture(
                true,
                1,
                1024L,
                "bytes",
                true,
                out string positiveReason);

            Assert.That(zero.Available, Is.False);
            StringAssert.Contains("non-empty benchmark render workload", zeroReason);
            Assert.That(missing.Available, Is.False);
            StringAssert.Contains("no completed-frame sample", missingReason);
            Assert.That(positive.Available, Is.True);
            Assert.That(positive.Value, Is.EqualTo(1024L));
            Assert.That(positiveReason, Is.Empty);
        }

        [Test]
        public void DrawCallPolicy_RejectsZeroForNonEmptyBenchmarkWorkload()
        {
            BattleBenchmarkMetric zero = BattleBenchmarkDrawCallPolicy.RequirePositiveForNonEmptyWorkload(
                BattleBenchmarkMetric.FromValue(0, "count"));
            BattleBenchmarkMetric positive = BattleBenchmarkDrawCallPolicy.RequirePositiveForNonEmptyWorkload(
                BattleBenchmarkMetric.FromValue(3, "count"));

            Assert.That(zero.Available, Is.False);
            Assert.That(positive.Available, Is.True);
            Assert.That(positive.Value, Is.EqualTo(3));
        }

        [Test]
        public void BenchmarkOwnedTextureMemoryPolicy_RequiresGenerationOwnedTexturesAndPositiveBytes()
        {
            BattleBenchmarkMetric missingGeneration = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                0,
                2,
                1024L,
                out string missingGenerationReason);
            BattleBenchmarkMetric missingTextures = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                7,
                0,
                1024L,
                out string missingTexturesReason);
            BattleBenchmarkMetric zeroBytes = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                7,
                2,
                0L,
                out string zeroBytesReason);
            BattleBenchmarkMetric positive = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                7,
                2,
                1024L,
                out string positiveReason);

            Assert.That(missingGeneration.Available, Is.False);
            StringAssert.Contains("generation", missingGenerationReason);
            Assert.That(missingTextures.Available, Is.False);
            StringAssert.Contains("owns no Texture2D", missingTexturesReason);
            Assert.That(zeroBytes.Available, Is.False);
            StringAssert.Contains("no positive bytes", zeroBytesReason);
            Assert.That(positive.Available, Is.True);
            Assert.That(positive.Value, Is.EqualTo(1024L));
            Assert.That(positiveReason, Is.Empty);
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
            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.LegacyOnly));
        }

        [Test]
        public void ABSuite_ExplicitDispose_RestoresPreviousBackend()
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralShadowBuild);
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

            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.CentralOnly));
            suite.Dispose();

            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.CentralShadowBuild));
        }

        [Test]
        public void ABSuite_PresenterCaptureFailure_DisposesAndRestoresPreviousBackend()
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.LegacyOnly);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                0,
                1024,
                1024,
                "100",
                string.Empty);
            ThrowingPresenter presenter = null;
            var suite = new BattleRenderingBenchmarkSuiteSession(
                config,
                world,
                (runConfig, runWorld, workload) =>
                {
                    presenter = new ThrowingPresenter(workload);
                    return new BattleRenderingBenchmarkSession(
                        runConfig,
                        runWorld,
                        workload,
                        SupportedPolicyContext(),
                        new BattleBenchmarkInjectedCompletedFrameCollector(
                            CreateAvailableCompletedFrame(7)),
                        presenter);
                });

            Assert.Throws<InvalidOperationException>(() => suite.CaptureFrame());
            Assert.That(presenter.Disposed, Is.True);
            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.LegacyOnly));
            Assert.Throws<ObjectDisposedException>(() => suite.CaptureFrame());
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
        public void V5InjectedCompletedFrames_RequireExactCount_AndKeepDrawScopesDistinct()
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
                Assert.That(session.Report.Frames[0].BenchmarkOwnedTextureMemoryBytes.Value, Is.EqualTo(128));
                Assert.That(session.Report.Frames[0].BenchmarkResourceGeneration, Is.EqualTo(7));
                BattleBenchmarkMetricAvailability textureAvailability =
                    FindMetricAvailability(session.Report, "benchmarkOwnedTextureMemoryBytes");
                Assert.That(textureAvailability.Scope, Is.EqualTo("benchmark-owned-textures"));
                StringAssert.Contains("generation 7", textureAvailability.Source);
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
        public void V5LegacyLocalDrawAndMeshMetricsAreNotApplicable_ButActualDrawsAreRequired()
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
        public void V5FixedDrawingWorkload_RejectsZeroDrawCallsAsMissingEvidence()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    1,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(drawCalls: 0)),
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(
                        ++guard,
                        Is.LessThan(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts * 3));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
                Assert.That(
                    session.Report.CompletedFrameRejectedAttemptCount,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                Assert.That(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts, Is.EqualTo(16));
                Assert.That(
                    FindMetricAvailability(session.Report, "drawCalls").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Missing));
                Assert.That(session.Report.MissingRequiredMetrics, Does.Contain("drawCalls"));
                StringAssert.Contains(
                    "required applicable metrics unavailable: drawCalls",
                    session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void CompletedFrameSampling_RetriesOneInvalidAttempt_AndStillCommitsExactly120Samples()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var collector = new SequencedCompletedFrameCollector(
                CreateUnavailableGpuCompletedFrame(drawCalls: 7),
                CreateAvailableCompletedFrame(drawCalls: 7));
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    120,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                collector,
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(++guard, Is.LessThan(300));

                Assert.That(session.Report.Frames.Count, Is.EqualTo(120));
                Assert.That(session.Report.CompletedFrameRejectedAttemptCount, Is.EqualTo(1));
                Assert.That(
                    session.Report.MaxCompletedFrameSampleAttempts,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                Assert.That(session.Report.CompletedFrameSamplingFailureReason, Is.Empty);
                Assert.That(
                    FindMetricAvailability(session.Report, "exactSampleCount").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Passed));
                for (int index = 0; index < session.Report.Frames.Count; index++)
                    Assert.That(session.Report.Frames[index].GpuFrameTimeMs.Available, Is.True);
                Assert.That(collector.RequestedGenerations.Count, Is.EqualTo(121));
                AssertGenerationsStrictlyIncrease(collector.RequestedGenerations);
                StringAssert.Contains("\"rejectedAttemptCount\":1", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void CompletedFrameSampling_AcceptsPositiveDrawCallsOnSixteenthBoundedAttempt()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var attempts = new BattleBenchmarkCompletedFrameMetrics[
                BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts];
            for (int index = 0; index < attempts.Length - 1; index++)
                attempts[index] = CreateAvailableCompletedFrame(drawCalls: 0);
            attempts[attempts.Length - 1] = CreateAvailableCompletedFrame(drawCalls: 7);
            var collector = new SequencedCompletedFrameCollector(attempts);
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    1,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                collector,
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(
                        ++guard,
                        Is.LessThan(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts * 3));

                Assert.That(session.Report.Frames.Count, Is.EqualTo(1));
                Assert.That(
                    session.Report.CompletedFrameRejectedAttemptCount,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts - 1));
                Assert.That(collector.RequestedGenerations.Count,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                Assert.That(session.Report.CompletedFrameSamplingFailureReason, Is.Empty);
                Assert.That(FindMetricAvailability(session.Report, "drawCalls").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Available));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void CompletedFrameSampling_ExhaustedRetryBudget_IsIncompleteWithExplicitReason()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var collector = new SequencedCompletedFrameCollector(
                CreateUnavailableGpuCompletedFrame(drawCalls: 7));
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    1,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                collector,
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(
                        ++guard,
                        Is.LessThan(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts * 3));

                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
                Assert.That(session.Report.Frames.Count, Is.EqualTo(0));
                Assert.That(
                    session.Report.CompletedFrameRejectedAttemptCount,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                StringAssert.Contains("exhausted", session.Report.CompletedFrameSamplingFailureReason);
                StringAssert.Contains("gpuFrameTimeMs", session.Report.CompletedFrameSamplingFailureReason);
                Assert.That(
                    FindMetricAvailability(session.Report, "exactSampleCount").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Missing));
                StringAssert.Contains(
                    session.Report.CompletedFrameSamplingFailureReason,
                    FindMetricAvailability(session.Report, "gpuFrameTimeMs").Reason);
                Assert.That(
                    collector.RequestedGenerations.Count,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                AssertGenerationsStrictlyIncrease(collector.RequestedGenerations);
                StringAssert.Contains("terminalFailureReason", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void V5VerdictPolicy_MissingRequiredMetricCannotPass()
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
        public void V5VerdictPolicy_EntirelyOmittedMandatoryEntryCannotPass()
        {
            var metrics = CreateCompleteMetricSchema();
            metrics.RemoveAll(metric => metric.Metric == "benchmarkOwnedTextureMemoryBytes");

            BattleRenderingBenchmarkVerdict verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                SupportedPolicyContext(),
                metrics,
                out _,
                out string[] missing);

            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
            Assert.That(missing, Does.Contain("benchmarkOwnedTextureMemoryBytes"));
        }

        [Test]
        public void V5VerdictPolicy_DuplicateMandatoryEntryCannotPass()
        {
            var metrics = CreateCompleteMetricSchema();
            metrics.Add(metrics[0]);

            BattleRenderingBenchmarkVerdict verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                SupportedPolicyContext(),
                metrics,
                out _,
                out string[] missing);

            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
            Assert.That(missing, Does.Contain(metrics[0].Metric + " (duplicate schema entry)"));
        }

        [Test]
        public void CurrentSceneUnmeasuredDeterminismEvidence_IsMissingRatherThanFailed()
        {
            Assert.That(
                BattleRenderingBenchmarkEvidencePolicy.ValidationStatus(null),
                Is.EqualTo(BattleBenchmarkMetricStatus.Missing));
            Assert.That(
                BattleRenderingBenchmarkEvidencePolicy.ValidationStatus(false),
                Is.EqualTo(BattleBenchmarkMetricStatus.Failed));

            var metrics = CreateCompleteMetricSchema();
            int index = metrics.FindIndex(metric => metric.Metric == "determinismValidated");
            metrics[index] = new BattleBenchmarkMetricAvailability(
                "determinismValidated",
                true,
                BattleBenchmarkMetricApplicability.Applicable,
                BattleBenchmarkMetricStatus.Missing,
                "validation-gate",
                0,
                1,
                "current-scene runtime checksum",
                "The current-scene workload did not measure this validation gate.");

            BattleRenderingBenchmarkVerdict verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                SupportedPolicyContext(),
                metrics,
                out _,
                out _);
            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
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
        public void StandalonePlayerBuildArguments_PreserveRealRenderingAndV5BenchmarkRequest()
        {
            string output = "Temp/P8-D-runtime-100-player-ab-v5.json";
            string log = "Temp/P8-D-runtime-100-player-ab-v5.log";
            string arguments = BattleRenderingBenchmarkPlayerBuild.BuildArguments("100", output, log);

            StringAssert.DoesNotContain("-batchmode", arguments);
            StringAssert.DoesNotContain("-nographics", arguments);
            StringAssert.Contains("-logFile \"" + log + "\"", arguments);

            var parsedArguments = new List<string> { "Game.exe" };
            string[] commandLineTokens = arguments.Split(' ');
            for (int index = 0; index < commandLineTokens.Length; index++)
                parsedArguments.Add(commandLineTokens[index].Trim('"'));
            bool parsed = BattleRenderingBenchmarkPlayerArguments.TryParse(
                parsedArguments.ToArray(),
                out BattleRenderingBenchmarkRequest request,
                out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(request.targetActiveEntities, Is.EqualTo("100"));
            Assert.That(request.comparison, Is.EqualTo("ab"));
            Assert.That(request.warmupFrames, Is.EqualTo(30));
            Assert.That(request.sampleFrames, Is.EqualTo(120));
            Assert.That(request.leakCheckFrames, Is.EqualTo(600));
            Assert.That(request.outputPath, Is.EqualTo(output));
        }

        [Test]
        public void RequestProcessor_ExitingPlayMode_AbortsAndClearsCurrentRunner()
        {
            var host = new UnityEngine.GameObject("Benchmark Processor Exit Test");
            BattleRenderingBenchmarkRunner staleRunner =
                host.AddComponent<BattleRenderingBenchmarkRunner>();
            BattleRenderingBenchmarkRequestProcessor.SetRunnerForTests(staleRunner);

            BattleRenderingBenchmarkRequestProcessor.HandlePlayModeStateChangeForTests(
                PlayModeStateChange.ExitingPlayMode);

            Assert.That(BattleRenderingBenchmarkRequestProcessor.HasRunnerForTests, Is.False);
            Assert.That(host == null, Is.True, "Abort must destroy the stale runner host.");
        }

        [Test]
        public void RequestProcessor_NonPlayPolling_ReconcilesStaleRunner()
        {
            var host = new UnityEngine.GameObject("Benchmark Processor Stale Poll Test");
            BattleRenderingBenchmarkRunner staleRunner =
                host.AddComponent<BattleRenderingBenchmarkRunner>();
            BattleRenderingBenchmarkRequestProcessor.SetRunnerForTests(staleRunner);

            BattleRenderingBenchmarkRequestProcessor.PollRequestForTests(false);

            Assert.That(BattleRenderingBenchmarkRequestProcessor.HasRunnerForTests, Is.False);
            Assert.That(host == null, Is.True, "Non-Play polling must not leave a stale runner registered.");
        }

        [Test]
        public void RequestProcessor_NonPlayPolling_PreservesQueuedRequest()
        {
            string requestPath = BattleRenderingBenchmarkRequestProcessor.ProjectPath(
                BattleRenderingBenchmarkRequestProcessor.RequestFile);
            string priorContent = File.Exists(requestPath) ? File.ReadAllText(requestPath) : null;
            try
            {
                BattleRenderingBenchmarkRequestProcessor.WriteRequest(
                    new BattleRenderingBenchmarkRequest
                    {
                        backend = "CentralOnly",
                        comparison = "ab",
                        warmupFrames = 0,
                        sampleFrames = 1,
                        leakCheckFrames = 0,
                        maxManagedGrowthBytes = 1024L,
                        maxGraphicsGrowthBytes = 1024L,
                        targetActiveEntities = "100",
                        outputPath = "Temp/request-preservation-test.json",
                    });

                BattleRenderingBenchmarkRequestProcessor.PollRequestForTests(false);

                Assert.That(File.Exists(requestPath), Is.True,
                    "A request must remain queued until the Editor enters Play Mode.");
            }
            finally
            {
                if (priorContent == null)
                {
                    if (File.Exists(requestPath))
                        File.Delete(requestPath);
                }
                else
                {
                    File.WriteAllText(requestPath, priorContent);
                }
            }
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
                Assert.That(session.Report.Passed, Is.False, "EditMode can never produce a v5 PASS verdict");
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

        [TestCase(false, BattleRenderingBenchmarkVerdict.Pass, BattleBenchmarkMetricStatus.Passed)]
        [TestCase(true, BattleRenderingBenchmarkVerdict.Fail, BattleBenchmarkMetricStatus.Failed)]
        public void LeakCheck_RequiresSuccessfulPostDisposeTeardown(
            bool retainOwnershipAfterDispose,
            BattleRenderingBenchmarkVerdict expectedVerdict,
            BattleBenchmarkMetricStatus expectedTeardownStatus)
        {
            BattleRenderingBenchmarkWorkload workload = BattleRenderingBenchmarkWorkload.Create(
                BattleRenderingBenchmarkScenario.Parse("100"),
                null,
                0,
                1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                200,
                200,
                "100",
                string.Empty);
            var presenter = new LeakPresenter(workload, retainOwnershipAfterDispose);
            var probe = new ScriptedLeakProbe(
                new long[] { 1000, 1100, 1150, 1050 },
                new long[] { 2000, 2100, 2150, 2050 });
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(7)),
                presenter,
                probe);
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False,
                    "the last soak frame begins teardown but cannot finalize in the same call");
                Assert.That(presenter.Disposed, Is.True);
                Assert.That(session.CaptureFrame(), Is.False,
                    "deferred Unity destruction must wait two Play frames");
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.DeferredDestructionPlayFrames);
                Assert.That(session.CaptureFrame(), Is.False,
                    "post-destroy cleanup must start after the deferred-destruction wait");
                Assert.That(probe.PostDisposeCleanupRequestCount, Is.EqualTo(1));
                Assert.That(session.CaptureFrame(), Is.False,
                    "the completed cleanup must be flushed before teardown metrics are read");
                Assert.That(probe.PostDisposeCleanupCompletionCount, Is.EqualTo(1));
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.PostDisposeCleanupPlayFrames);
                Assert.That(session.CaptureFrame(), Is.True);

                Assert.That(presenter.PresentCalls, Is.EqualTo(3));
                Assert.That(session.Report.LeakReport.TeardownStatus, Is.EqualTo(expectedTeardownStatus));
                Assert.That(session.Report.LeakReport.TeardownResourcesEnd,
                    Is.EqualTo(retainOwnershipAfterDispose ? 1 : 0));
                Assert.That(session.Report.LeakReport.TeardownOwnedEndBytes,
                    Is.EqualTo(retainOwnershipAfterDispose ? 256 : 0));
                Assert.That(session.Report.Verdict, Is.EqualTo(expectedVerdict));
                StringAssert.Contains("teardownStatus", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void LeakCheck_TeardownUsesSteadyStateBaseline_NotPrePresenterInitialization()
        {
            BattleRenderingBenchmarkWorkload workload = BattleRenderingBenchmarkWorkload.Create(
                BattleRenderingBenchmarkScenario.Parse("100"),
                null,
                0,
                1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                200,
                200,
                "100",
                string.Empty);
            var presenter = new LeakPresenter(workload, false);
            var probe = new ScriptedLeakProbe(
                new long[] { 1000, 2000, 2100, 2050 },
                new long[] { 2000, 4000, 4100, 4050 });
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(7)),
                presenter,
                probe);
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.DeferredDestructionPlayFrames);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.PostDisposeCleanupPlayFrames);
                Assert.That(session.CaptureFrame(), Is.True);

                BattleRenderingBenchmarkLeakReport leak = session.Report.LeakReport;
                Assert.That(leak.PrePresenterManagedBytes, Is.EqualTo(1000));
                Assert.That(leak.ManagedStartBytes, Is.EqualTo(2000));
                Assert.That(leak.TeardownManagedGrowthBytes, Is.EqualTo(50));
                Assert.That(leak.PrePresenterGraphicsBytes, Is.EqualTo(2000));
                Assert.That(leak.GraphicsStartBytes, Is.EqualTo(4000));
                Assert.That(leak.TeardownGraphicsGrowthBytes, Is.EqualTo(50));
                Assert.That(leak.TeardownOwnedEndBytes, Is.Zero);
                Assert.That(leak.TeardownResourcesEnd, Is.Zero);
                Assert.That(leak.TeardownStatus, Is.EqualTo(BattleBenchmarkMetricStatus.Passed));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
                StringAssert.Contains("teardownMemoryBaseline", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void LeakCheck_TeardownCleanupTimeoutFailsBoundedly()
        {
            BattleRenderingBenchmarkWorkload workload = BattleRenderingBenchmarkWorkload.Create(
                BattleRenderingBenchmarkScenario.Parse("100"),
                null,
                0,
                1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                200,
                200,
                "100",
                string.Empty);
            var probe = new ScriptedLeakProbe(
                new long[] { 1000, 1100, 1150, 1050 },
                new long[] { 2000, 2100, 2150, 2050 },
                postDisposeCleanupComplete: false);
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(7)),
                new LeakPresenter(workload, false),
                probe);
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.MaxPostDisposeCleanupPlayFrames);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.True);

                Assert.That(probe.PostDisposeCleanupRequestCount, Is.EqualTo(1));
                Assert.That(probe.PostDisposeCleanupCompletionCount, Is.Zero);
                Assert.That(session.Report.LeakReport.TeardownStatus, Is.EqualTo(BattleBenchmarkMetricStatus.Failed));
                StringAssert.Contains("did not complete within", session.Report.LeakReport.TeardownReason);
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Fail));
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

        private static BattleRenderingBenchmarkPolicyContext SupportedPolicyContext()
        {
            return new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                true,
                true);
        }

        private static List<BattleBenchmarkMetricAvailability> CreateCompleteMetricSchema()
        {
            var metrics = new List<BattleBenchmarkMetricAvailability>(
                BattleRenderingBenchmarkVerdictPolicy.RequiredMetricNames.Count);
            for (int index = 0;
                 index < BattleRenderingBenchmarkVerdictPolicy.RequiredMetricNames.Count;
                 index++)
            {
                metrics.Add(new BattleBenchmarkMetricAvailability(
                    BattleRenderingBenchmarkVerdictPolicy.RequiredMetricNames[index],
                    true,
                    BattleBenchmarkMetricApplicability.Applicable,
                    BattleBenchmarkMetricStatus.Available,
                    "test",
                    1,
                    1,
                    "injected complete schema",
                    string.Empty));
            }
            return metrics;
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
                BattleBenchmarkMetric.FromValue(2048, "bytes"));
        }

        private static BattleBenchmarkCompletedFrameMetrics CreateUnavailableGpuCompletedFrame(
            int drawCalls)
        {
            BattleBenchmarkCompletedFrameMetrics available = CreateAvailableCompletedFrame(drawCalls);
            return new BattleBenchmarkCompletedFrameMetrics(
                available.FrameTimeMs,
                available.MainThreadTimeMs,
                available.RenderThreadTimeMs,
                BattleBenchmarkMetric.Unavailable("ms"),
                available.ManagedAllocationBytes,
                available.DrawCalls,
                available.TotalAllocatedMemoryBytes,
                available.GraphicsMemoryBytes);
        }

        private static void AssertGenerationsStrictlyIncrease(IReadOnlyList<int> generations)
        {
            for (int index = 1; index < generations.Count; index++)
                Assert.That(generations[index], Is.GreaterThan(generations[index - 1]));
        }

        private sealed class SequencedCompletedFrameCollector :
            IBattleBenchmarkCompletedFrameCollector
        {
            private readonly BattleBenchmarkCompletedFrameMetrics[] sequence;
            private int pendingGeneration;
            private int nextIndex;

            internal SequencedCompletedFrameCollector(
                params BattleBenchmarkCompletedFrameMetrics[] completedFrameMetrics)
            {
                sequence = completedFrameMetrics ?? throw new ArgumentNullException(nameof(completedFrameMetrics));
                if (sequence.Length == 0)
                    throw new ArgumentException("At least one completed-frame sample is required.", nameof(completedFrameMetrics));
            }

            internal List<int> RequestedGenerations { get; } = new List<int>();
            public bool IsSupported => true;
            public string UnsupportedReason => string.Empty;

            public void Request(int generation)
            {
                if (pendingGeneration != 0)
                    throw new InvalidOperationException("A completed-frame sample is already pending.");
                pendingGeneration = generation;
                RequestedGenerations.Add(generation);
            }

            public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics)
            {
                if (pendingGeneration != generation)
                {
                    metrics = default;
                    return false;
                }
                pendingGeneration = 0;
                int index = Math.Min(nextIndex++, sequence.Length - 1);
                metrics = sequence[index];
                return true;
            }

            public string Source(BattleBenchmarkRecorderKind kind) => "sequenced completed-frame test sample";
            public string Reason(BattleBenchmarkRecorderKind kind) => string.Empty;
            public void Reset() => pendingGeneration = 0;
            public void Dispose() => Reset();
        }

        private sealed class InjectedPresenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;
            private readonly bool central;
            private readonly int submissionDrawCalls;
            private readonly BattleCentralBuildDiagnostics diagnostics;
            private bool disposed;

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
            public int ResourceGeneration => 7;
            public int OwnedTextureResourceCount => disposed ? 0 : 2;
            public int OwnedResourceCount => disposed ? 0 : 1;
            public long CachedOwnedResourceMemoryBytes => disposed ? 0L : 256L;
            public long MeasureOwnedResourceMemoryBytes() => disposed ? 0L : 256L;
            public long MeasureOwnedTextureMemoryBytes() => disposed ? 0L : 128L;
            public BattleCentralBuildDiagnostics Diagnostics => diagnostics;
            public double Present() => 0.25;
            public void Dispose()
            {
                disposed = true;
            }
        }

        private sealed class ThrowingPresenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;

            internal ThrowingPresenter(BattleRenderingBenchmarkWorkload benchmarkWorkload)
            {
                workload = benchmarkWorkload;
            }

            internal bool Disposed { get; private set; }
            public string Implementation => "ThrowingPresenter";
            public string EffectiveBackend => "CentralOnly";
            public string ResourceMode => "Injected";
            public string DrawMode => "Injected";
            public int RenderTargetWidth => 256;
            public int RenderTargetHeight => 256;
            public int ResolvedCommandCount => workload.CommandCount;
            public int MaterializedRenderItemCount => workload.CommandCount;
            public int ResourceSegmentCount => 1;
            public int SubmissionDrawCount => 1;
            public string SubmissionDrawMetricSource => "injected";
            public string SubmissionDrawUnavailableReason => string.Empty;
            public int ResourceGeneration => 7;
            public int OwnedTextureResourceCount => Disposed ? 0 : 2;
            public int OwnedResourceCount => Disposed ? 0 : 1;
            public long CachedOwnedResourceMemoryBytes => Disposed ? 0L : 256L;
            public long MeasureOwnedResourceMemoryBytes() => Disposed ? 0L : 256L;
            public long MeasureOwnedTextureMemoryBytes() => Disposed ? 0L : 128L;
            public BattleCentralBuildDiagnostics Diagnostics { get; } = new BattleCentralBuildDiagnostics();

            public double Present()
            {
                throw new InvalidOperationException("Injected presenter failure.");
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private sealed class LeakPresenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;
            private readonly bool retainOwnershipAfterDispose;

            internal LeakPresenter(
                BattleRenderingBenchmarkWorkload benchmarkWorkload,
                bool retainOwnership)
            {
                workload = benchmarkWorkload;
                retainOwnershipAfterDispose = retainOwnership;
            }

            internal bool Disposed { get; private set; }
            internal int PresentCalls { get; private set; }
            private bool OwnsResources => !Disposed || retainOwnershipAfterDispose;
            public string Implementation => "LeakPresenter";
            public string EffectiveBackend => "CentralOnly";
            public string ResourceMode => "Injected";
            public string DrawMode => "Injected";
            public int RenderTargetWidth => 256;
            public int RenderTargetHeight => 256;
            public int ResolvedCommandCount => workload.CommandCount;
            public int MaterializedRenderItemCount => workload.CommandCount;
            public int ResourceSegmentCount => 1;
            public int SubmissionDrawCount => 1;
            public string SubmissionDrawMetricSource => "injected";
            public string SubmissionDrawUnavailableReason => string.Empty;
            public int ResourceGeneration => 7;
            public int OwnedTextureResourceCount => OwnsResources ? 2 : 0;
            public int OwnedResourceCount => OwnsResources ? 1 : 0;
            public long CachedOwnedResourceMemoryBytes => OwnsResources ? 256L : 0L;
            public long MeasureOwnedResourceMemoryBytes() => OwnsResources ? 256L : 0L;
            public long MeasureOwnedTextureMemoryBytes() => OwnsResources ? 128L : 0L;
            public BattleCentralBuildDiagnostics Diagnostics { get; } = new BattleCentralBuildDiagnostics();

            public double Present()
            {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(LeakPresenter));
                PresentCalls++;
                return 0.25;
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private sealed class ScriptedLeakProbe : IBattleBenchmarkLeakProbe
        {
            private readonly long[] managedSamples;
            private readonly long[] graphicsSamples;
            private int managedIndex;
            private int graphicsIndex;

            private readonly bool postDisposeCleanupComplete;

            internal ScriptedLeakProbe(
                long[] retainedManagedSamples,
                long[] retainedGraphicsSamples,
                bool postDisposeCleanupComplete = true)
            {
                managedSamples = retainedManagedSamples;
                graphicsSamples = retainedGraphicsSamples;
                this.postDisposeCleanupComplete = postDisposeCleanupComplete;
            }

            public int CurrentUnityFrame { get; private set; }
            public bool RequiresDeferredDestructionWait => true;
            public int PostDisposeCleanupRequestCount { get; private set; }
            public int PostDisposeCleanupCompletionCount { get; private set; }
            public bool IsPostDisposeCleanupComplete => postDisposeCleanupComplete;

            public long CaptureRetainedManagedHeapBytes()
            {
                return managedSamples[managedIndex++];
            }

            public BattleBenchmarkMetric CaptureGraphicsMemory()
            {
                return BattleBenchmarkMetric.FromValue(graphicsSamples[graphicsIndex++], "bytes");
            }

            public void BeginPostDisposeCleanup()
            {
                PostDisposeCleanupRequestCount++;
            }

            public void CompletePostDisposeCleanup()
            {
                PostDisposeCleanupCompletionCount++;
            }

            internal void AdvanceFrames(int count)
            {
                CurrentUnityFrame += count;
            }
        }
    }
}
#endif
