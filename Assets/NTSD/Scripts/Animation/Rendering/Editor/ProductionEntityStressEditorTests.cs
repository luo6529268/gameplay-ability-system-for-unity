#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class ProductionEntityStressEditorTests
    {
        [Test]
        public void LocalCatchUp_FourTicksBuildPresentationOnlyOnFinalTick()
        {
            float accumulator = SimulationConstants.SIM_DT * 4f;
            var flags = new bool[4];

            for (int tick = 0; tick < flags.Length; tick++)
            {
                accumulator -= SimulationConstants.SIM_DT;
                flags[tick] = SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LocalFreeRun,
                    requireInputFrameReady: false,
                    accumulator,
                    tick,
                    maxCatchUpTicks: 4);
            }

            CollectionAssert.AreEqual(
                new[] { false, false, false, true },
                flags);
        }

        [Test]
        public void LocalCatchUp_SingleTickBuildsPresentation()
        {
            float accumulator = SimulationConstants.SIM_DT;
            accumulator -= SimulationConstants.SIM_DT;

            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LocalFreeRun,
                    requireInputFrameReady: false,
                    accumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True);
        }

        [Test]
        public void InputReadyAndNonLocalModes_KeepEveryTickPresentationBuild()
        {
            float remainingAccumulator = SimulationConstants.SIM_DT * 3f;

            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LocalFreeRun,
                    requireInputFrameReady: true,
                    remainingAccumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True);
            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.LockstepBuffered,
                    requireInputFrameReady: false,
                    remainingAccumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True);
            Assert.That(
                SimulationTickDriver.ShouldBuildPresentationForCatchUpTick(
                    SimulationDriveMode.Manual,
                    requireInputFrameReady: false,
                    remainingAccumulator,
                    ticksAlreadyExecuted: 0,
                    maxCatchUpTicks: 4),
                Is.True,
                "Manual driver ticks keep the public default build=true contract; " +
                "the stress harness opts into intermediate suppression explicitly.");
        }

        [Test]
        public void SmokeRequest_UsesFiftyEntitiesAndBoundedSampling()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                warmupTicks = 100,
                sampleTicks = 100,
                spawnBatchSize = 500,
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 2,
                outputPath = "Temp/smoke.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.Mode, Is.EqualTo(ProductionEntityStressMode.Smoke50));
            Assert.That(config.EntityCount, Is.EqualTo(50));
            Assert.That(config.WarmupTicks, Is.EqualTo(5));
            Assert.That(config.SampleTicks, Is.EqualTo(30));
            Assert.That(config.SpawnBatchSize, Is.EqualTo(100));
            Assert.That(config.MaxBacklogTicks, Is.EqualTo(4));
            Assert.That(
                config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(Path.IsPathRooted(config.OutputPath), Is.True);
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("  ")]
        public void InputMode_MissingOrEmptyDefaultsToAi(string inputMode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                inputMode = inputMode,
                outputPath = "Temp/input-default.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
            Assert.That(
                ProductionEntityStressConfig.FormatInputMode(config.InputMode),
                Is.EqualTo("ai"));
        }

        [Test]
        public void InputMode_LegacyRequestWithoutFieldRemainsAi()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(
                    "{\"action\":\"smoke\",\"outputPath\":\"Temp/input-legacy.json\"}");

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(request.inputMode, Is.EqualTo("ai"));
            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.Ai));
        }

        [Test]
        public void InputMode_NoneParsesAndDisablesAiPolicy()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed",
                inputMode = "none",
                outputPath = "Temp/input-none.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.InputMode, Is.EqualTo(ProductionEntityStressInputMode.None));
            Assert.That(
                config.InputMode == ProductionEntityStressInputMode.Ai,
                Is.False);
        }

        [Test]
        public void InputMode_UnknownValueIsRejected()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                inputMode = "human",
                outputPath = "Temp/input-invalid.json",
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("human"));
            Assert.That(exception.Message, Does.Contain("ai or none"));
        }

        [Test]
        public void InputMode_ReportJsonRecordsActualMode()
        {
            var report = new ProductionEntityStressReport
            {
                inputMode = ProductionEntityStressConfig.FormatInputMode(
                    ProductionEntityStressInputMode.None),
            };

            string json = JsonUtility.ToJson(report);
            ProductionEntityStressReport roundTrip =
                JsonUtility.FromJson<ProductionEntityStressReport>(json);

            Assert.That(json, Does.Contain("\"inputMode\":\"none\""));
            Assert.That(roundTrip.inputMode, Is.EqualTo("none"));
        }

        [Test]
        public void FormalCollector_LegacyRequestDefaultsToConfigured()
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(
                    "{\"action\":\"smoke\",\"outputPath\":\"Temp/formal-default.json\"}");

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();
            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);

            Assert.That(request.formalCollectorMode, Is.EqualTo("configured"));
            Assert.That(
                config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(
                query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(
                ProductionEntityStressRunner.ResolveAppliedFormalCollectorModeForDiagnostics(
                    world,
                    query),
                Is.EqualTo(CollisionFormalCollectorMode.ForceBruteForce));
        }

        [TestCase(
            CollisionBroadphaseBackend.LooseQuadtree,
            CollisionFormalCollectorMode.ForceRoleAware,
            "role")]
        [TestCase(
            CollisionBroadphaseBackend.BruteForce,
            CollisionFormalCollectorMode.ForceBruteForce,
            "brute")]
        public void FormalCollector_ConfiguredReportReflectsProductionBackend(
            CollisionBroadphaseBackend backend,
            CollisionFormalCollectorMode expectedAppliedMode,
            string expectedReportMode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                formalCollectorMode = "configured",
                outputPath = "Temp/formal-configured-report.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                backend);
            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);
            CollisionFormalCollectorMode applied =
                ProductionEntityStressRunner.ResolveAppliedFormalCollectorModeForDiagnostics(
                    world,
                    query);
            var report = new ProductionEntityStressReport
            {
                formalCollectorRequestedMode =
                    ProductionEntityStressConfig.FormatFormalCollectorMode(
                        config.FormalCollectorMode),
                formalCollectorMode =
                    ProductionEntityStressConfig.FormatFormalCollectorMode(applied),
            };

            Assert.That(config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            Assert.That(applied, Is.EqualTo(expectedAppliedMode));
            Assert.That(report.formalCollectorRequestedMode, Is.EqualTo("configured"));
            Assert.That(report.formalCollectorMode, Is.EqualTo(expectedReportMode));
        }

        [Test]
        public void FormalCollector_RoleRequestParsesAndAppliesToStressWorld()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                formalCollectorMode = "role",
                outputPath = "Temp/formal-role.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var world = new SimulationWorld();

            BruteForceSceneQuery query =
                ProductionEntityStressRunner.ApplyFormalCollectorModeForDiagnostics(
                    world,
                    config.FormalCollectorMode);

            Assert.That(
                config.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
            Assert.That(
                query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.ForceRoleAware));
            Assert.That(
                ProductionEntityStressConfig.FormatFormalCollectorMode(
                    query.FormalCollectorMode),
                Is.EqualTo("role"));
        }

        [Test]
        public void FormalCollector_UnknownValueIsRejected()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                formalCollectorMode = "adaptive",
                outputPath = "Temp/formal-invalid.json",
            };

            ArgumentException exception = Assert.Throws<ArgumentException>(() =>
                ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot));

            Assert.That(exception.Message, Does.Contain("adaptive"));
            Assert.That(exception.Message, Does.Contain("configured, legacy, role, or brute"));
        }

        [Test]
        public void DetailTiming_LegacyRequestDefaultsToDisabledAndReportMarksItUnavailable()
        {
            ProductionEntityStressRequest request = JsonUtility.FromJson<ProductionEntityStressRequest>(
                "{\"action\":\"dispersed\",\"outputPath\":\"Temp/legacy.json\"}");
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            var report = new ProductionEntityStressReport
            {
                detailPhaseTimingEnabled = config.EnableDetailPhaseTiming,
            };

            new ProductionEntityStressDetailPhaseTimingCollector().PopulateReport(report);

            Assert.That(config.EnableDetailPhaseTiming, Is.False);
            Assert.That(report.detailPhaseTimingEnabled, Is.False);
            Assert.That(report.detailPhaseTimings, Is.Empty);
            Assert.That(report.detailPhaseTimingSource, Is.Empty);
            Assert.That(report.detailPhaseTimingUnavailableReason, Does.Contain("Disabled by request"));
            Assert.That(report.aiInputDetailTimings, Is.Empty);
            Assert.That(report.aiInputDetailTimingSource, Is.Empty);
            Assert.That(
                report.aiInputDetailTimingUnavailableReason,
                Does.Contain("Disabled by request"));
        }

        [Test]
        public void DetailTiming_LegacyReportJsonInitializesNewAiTimingFieldsAsUnavailable()
        {
            ProductionEntityStressReport report =
                JsonUtility.FromJson<ProductionEntityStressReport>(
                    "{\"schema\":\"ntsd-production-entity-stress/v1\"," +
                    "\"status\":\"StoppedCleanly\",\"detailPhaseTimingEnabled\":false}");

            new ProductionEntityStressDetailPhaseTimingCollector().PopulateReport(report);

            Assert.That(report.aiInputDetailTimings, Is.Not.Null);
            Assert.That(report.aiInputDetailTimings, Is.Empty);
            Assert.That(report.aiInputDetailCounters, Is.Not.Null);
            Assert.That(report.aiInputDetailCounters.available, Is.False);
            Assert.That(report.aiInputDetailTimingSource, Is.Empty);
            Assert.That(
                report.aiInputDetailTimingUnavailableReason,
                Does.Contain("Disabled by request"));
        }

        [Test]
        public void DetailTiming_RequestExplicitlyEnablesNestedDiagnostics()
        {
            var request = new ProductionEntityStressRequest
            {
                action = "dispersed",
                enableDetailPhaseTiming = true,
                outputPath = "Temp/detail-timing.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.EnableDetailPhaseTiming, Is.True);
        }

        [TestCase("dispersed", ProductionEntityStressMode.Dispersed1000)]
        [TestCase("concentrated", ProductionEntityStressMode.Concentrated1000)]
        public void ProductionModes_RequestOneThousandEntities(
            string action,
            ProductionEntityStressMode expectedMode)
        {
            var request = new ProductionEntityStressRequest
            {
                action = action,
                warmupTicks = 30,
                sampleTicks = 300,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
                outputPath = $"Temp/{action}.json",
            };

            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);

            Assert.That(config.Mode, Is.EqualTo(expectedMode));
            Assert.That(config.EntityCount, Is.EqualTo(1000));
            Assert.That(config.AutoCleanup, Is.False);
        }

        [Test]
        public void SpawnLayouts_SeparateDispersedAndConcentratedDomains()
        {
            Vector3 dispersedFirst = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Dispersed1000,
                0,
                1000);
            Vector3 dispersedLast = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Dispersed1000,
                999,
                1000);
            Vector3 concentratedFirst = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Concentrated1000,
                0,
                1000);
            Vector3 concentratedLast = ProductionEntityStressRunner.BuildSpawnPosition(
                ProductionEntityStressMode.Concentrated1000,
                999,
                1000);

            Assert.That(Vector3.Distance(dispersedFirst, dispersedLast), Is.GreaterThan(700f));
            Assert.That(Vector3.Distance(concentratedFirst, concentratedLast), Is.LessThan(40f));
        }

        [Test]
        public void MetricSummary_ComputesInterpolatedPercentiles()
        {
            var values = new List<double> { 1d, 2d, 3d, 4d, 5d };
            ProductionEntityStressMetricSummary summary =
                ProductionEntityStressStatistics.Summarize(values, "ms", "test");

            Assert.That(summary.available, Is.True);
            Assert.That(summary.sampleCount, Is.EqualTo(5));
            Assert.That(summary.average, Is.EqualTo(3d));
            Assert.That(summary.maximum, Is.EqualTo(5d));
            Assert.That(summary.p95, Is.EqualTo(4.8d).Within(0.0001d));
            Assert.That(summary.p99, Is.EqualTo(4.96d).Within(0.0001d));
        }

        [Test]
        public void BattleTickPhaseRecorder_OffDoesNotRecord_AndOnRecords()
        {
            var recorder = new BattleTickPhaseDiagnostics();

            recorder.BeginTick(10);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);

            Assert.That(recorder.Enabled, Is.False);
            Assert.That(recorder.LastTickIndex, Is.EqualTo(-1));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.CharacterInput),
                Is.EqualTo(0));

            recorder.SetEnabled(true);
            recorder.BeginTick(11);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);

            Assert.That(recorder.LastTickIndex, Is.EqualTo(11));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.CharacterInput),
                Is.GreaterThan(0));
        }

        [Test]
        public void BattleTickPhaseRecorder_BeginTickResetsLastValues()
        {
            var recorder = new BattleTickPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(20);
            recorder.BeginPhase(BattleTickPhase.FrameAdvance);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.FrameAdvance);
            Assert.That(recorder.GetLastPhaseSumTimestampTicks(), Is.GreaterThan(0));

            recorder.BeginTick(21);

            Assert.That(recorder.LastTickIndex, Is.EqualTo(21));
            Assert.That(recorder.GetLastPhaseSumTimestampTicks(), Is.EqualTo(0));
        }

        [Test]
        public void BattleTickPhaseRecorder_RepeatedPhaseAccumulatesWithinTick()
        {
            var recorder = new BattleTickPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(30);

            recorder.BeginPhase(BattleTickPhase.StageBounds);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.StageBounds);
            long firstElapsed = recorder.GetLastElapsedTimestampTicks(
                BattleTickPhase.StageBounds);

            recorder.BeginPhase(BattleTickPhase.StageBounds);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.StageBounds);

            Assert.That(firstElapsed, Is.GreaterThan(0));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.StageBounds),
                Is.GreaterThan(firstElapsed));
        }

        [Test]
        public void BattleTickSystem_InputClearEarlyReturnClosesItsPhase()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics recorder =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            world.SetNeedClearInput(true);

            var tickSystem = new NTSDBattleTickSystem(world);
            tickSystem.RunReleaseTick(31);
            long inputClearElapsed = recorder.GetLastElapsedTimestampTicks(
                BattleTickPhase.InputClear);

            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.InputClear);

            Assert.That(world.NeedClearInput, Is.False);
            Assert.That(recorder.LastTickIndex, Is.EqualTo(31));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.InputClear),
                Is.EqualTo(inputClearElapsed),
                "EndPhase after the early return must be ignored because InputClear was closed.");
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleTickPhase.CharacterInput),
                Is.EqualTo(0));
        }

        [Test]
        public void BattleTickPhaseRecorder_PhaseNamesAndCountAreStable()
        {
            string[] expected =
            {
                "BattleFlow",
                "Cooldown",
                "HumanInput",
                "RuntimeMaintenance",
                "InputClear",
                "CharacterInput",
                "EarlyFrameAdvance",
                "FrameLogic",
                "FrameAdvance",
                "DeathCleanup",
                "StageBounds",
                "PreInteraction",
                "HeldLinkValidation",
                "HeldProcess",
                "CollisionSnapshot",
                "PairVRest",
                "CandidateCollect",
                "CharacterHitConsumePostInteraction",
                "RandomWeaponDrop",
                "ObjectHitConsume",
                "CandidateConsumptionEnd",
                "PreFrameBounds",
                "Stage",
                "RenderDispatch",
                "FramePostProcess",
                "LateEntityUpdate",
                "RandomWeaponDropTail",
                "EntityPostFrameTail",
                "BattleResults",
            };

            Assert.That(BattleTickPhaseDiagnostics.PhaseCount, Is.EqualTo(expected.Length));
            for (int i = 0; i < expected.Length; i++)
            {
                Assert.That(
                    BattleTickPhaseDiagnostics.GetPhaseName((BattleTickPhase)i),
                    Is.EqualTo(expected[i]),
                    $"Phase id {i} changed its diagnostic contract.");
            }
        }

        [Test]
        public void PhaseTimingCollector_SamplesOnlyAfterWarmup()
        {
            var recorder = new BattleTickPhaseDiagnostics();
            var collector = new ProductionEntityStressPhaseTimingCollector();
            recorder.SetEnabled(true);

            recorder.BeginTick(1);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);
            collector.CaptureAfterTick(recorder, 10d, 1, 1);
            Assert.That(collector.SampleCount, Is.EqualTo(0));

            recorder.BeginTick(2);
            recorder.BeginPhase(BattleTickPhase.CharacterInput);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickPhase.CharacterInput);
            collector.CaptureAfterTick(recorder, 10d, 2, 1);

            var report = new ProductionEntityStressReport();
            collector.PopulateReport(report);
            Assert.That(collector.SampleCount, Is.EqualTo(1));
            Assert.That(report.phaseTimings, Has.Count.EqualTo(BattleTickPhaseDiagnostics.PhaseCount));
            Assert.That(
                report.phaseTimings[(int)BattleTickPhase.CharacterInput].timing.sampleCount,
                Is.EqualTo(1));
            Assert.That(report.phaseTimingUnattributedMilliseconds.sampleCount, Is.EqualTo(1));
        }

        [Test]
        public void PhaseTimingLifecycle_DisablesRecorderDuringCleanup()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics recorder =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            Assert.That(world.ActiveBattleTickPhaseDiagnosticsForDiagnostics, Is.SameAs(recorder));

            ProductionEntityStressPhaseTimingLifecycle.Disable(world);

            Assert.That(recorder.Enabled, Is.False);
            Assert.That(world.ActiveBattleTickPhaseDiagnosticsForDiagnostics, Is.Null);
        }

        [TestCase(true, "InterruptedCleanly")]
        [TestCase(false, "InterruptedWithResidue")]
        public void RunStatusPolicy_LabelsDirectDestroyAsInterrupted(
            bool restored,
            string expected)
        {
            Assert.That(
                ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    "Running",
                    "runner-destroyed",
                    restored),
                Is.EqualTo(expected));
        }

        [Test]
        public void RunStatusPolicy_PreservesNormalStopAndFailureStatuses()
        {
            Assert.That(
                ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    "Running",
                    "manual-stop",
                    true),
                Is.EqualTo("Running"));
            Assert.That(
                ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    "Failed",
                    "exception",
                    false),
                Is.EqualTo("Failed"));
        }

        [Test]
        public void RequestProcessor_OnlyEntersPlayModeForStartActions()
        {
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldEnterPlayMode("dispersed", false),
                Is.True);
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldEnterPlayMode("stop", false),
                Is.False);
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldEnterPlayMode("concentrated", true),
                Is.False);
        }

        [Test]
        public void PopulationPolicy_AccountsForRendererAndCharacterRegistrations()
        {
            Assert.That(
                ProductionEntityStressPopulationPolicy.Evaluate(
                    50,
                    50,
                    50,
                    100,
                    50,
                    50),
                Is.True);
            Assert.That(
                ProductionEntityStressPopulationPolicy.Evaluate(
                    50,
                    50,
                    50,
                    50,
                    50,
                    50),
                Is.False,
                "ObjectCount must include each LF2ObjectRenderer plus its LF2Character.");
        }

        [Test]
        public void BootstrapGate_RequiresSuppressedBootstrapAndProductionServices()
        {
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldSuppressBattleTestBootstrap("dispersed"),
                Is.True);
            Assert.That(
                ProductionEntityStressRequestProcessor.ShouldSuppressBattleTestBootstrap("stop"),
                Is.False);
            Assert.That(
                ProductionEntityStressRequestProcessor.IsReadyToStart(true, false, true),
                Is.False);
            Assert.That(
                ProductionEntityStressRequestProcessor.IsReadyToStart(true, true, true),
                Is.True);
            Assert.That(
                ProductionEntityStressRequestProcessor.IsReadyToStart(false, false, true),
                Is.True);
        }

        [Test]
        public void TeardownPolicy_RequiresActiveStateToReturnToBaseline()
        {
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 0, 0, 20, 0,
                    0, 12, 0),
                Is.True);
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 1, 0, 12, 0,
                    0, 12, 0),
                Is.False);
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 0, 0, 11, 0,
                    0, 12, 0),
                Is.True,
                "Retained inactive pool capacity is an allowed cache and is not active residue.");
            Assert.That(
                ProductionEntityStressTeardownPolicy.IsRestored(
                    0, 0, 0, 0, 1, 1001, 0,
                    0, 12, 0),
                Is.False,
                "Active pooled objects still prevent restoration.");
        }

        [Test]
        public void CleanupJournal_ContinuesAfterAnIndividualCleanupFailure()
        {
            var order = new List<string>();
            var journal = new ProductionEntityStressCleanupJournal();

            bool failed = journal.Attempt("release-entity-0", () =>
            {
                order.Add("first");
                throw new InvalidOperationException("injected release failure");
            });
            bool continued = journal.Attempt("release-entity-1", () => order.Add("second"));
            bool restored = journal.Attempt("restore-driver", () => order.Add("restore"));

            Assert.That(failed, Is.False);
            Assert.That(continued, Is.True);
            Assert.That(restored, Is.True);
            Assert.That(order, Is.EqualTo(new[] { "first", "second", "restore" }));
            Assert.That(journal.FailureCount, Is.EqualTo(1));
            Assert.That(journal.FormatFailures(), Does.Contain("release-entity-0"));
            Assert.That(journal.FormatFailures(), Does.Contain("injected release failure"));
        }

        [Test]
        public void ActiveGameObjectAfterScan_UsesStressRootInsteadOfTrackingList()
        {
            var stressRoot = new GameObject("stress-root");
            try
            {
                var active = new GameObject("active-residue");
                active.transform.SetParent(stressRoot.transform);
                var inactive = new GameObject("retained-inactive");
                inactive.transform.SetParent(stressRoot.transform);
                inactive.SetActive(false);
                var trackingList = new List<GameObject> { active };
                trackingList.Clear();

                Assert.That(trackingList, Is.Empty);
                Assert.That(
                    ProductionEntityStressTeardownPolicy.CountActiveStressRootGameObjects(
                        stressRoot.transform),
                    Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(stressRoot);
            }
        }

        [Test]
        public void TeardownEvidence_SeparatesRetainedInactiveCapacityFromActiveCleanup()
        {
            var teardown = new ProductionEntityStressTeardownReport
            {
                restored = true,
                activeStateRestored = true,
                driverStateRestored = true,
                loggingStateRestored = true,
                objectPoolActiveBeforeRun = 0,
                objectPoolActiveAfter = 0,
                referencePoolActiveBeforeRun = 0,
                referencePoolActiveAfter = 0,
                retainedInactiveObjectPoolCapacityBeforeRun = 10,
                retainedInactiveObjectPoolCapacityAfter = 1001,
                retainedInactiveObjectPoolCapacityDelta = 991,
            };

            string evidence = ProductionEntityStressTeardownPolicy.BuildEvidence(
                "test-cleanup",
                teardown);

            Assert.That(evidence, Does.Contain("activeCleanupRestored=True"));
            Assert.That(evidence, Does.Contain("retainedInactiveObjectPoolCapacity=10->1001"));
            Assert.That(evidence, Does.Contain("doesNotAffectRestored=True"));
        }

        [Test]
        public void DerivedObservationPolicy_ExcludesHarnessHandlesAndDeduplicatesGenerations()
        {
            var owned = new HashSet<RuntimeEntityHandle>
            {
                new RuntimeEntityHandle(50, 1),
            };
            var observed = new HashSet<RuntimeEntityHandle>();
            var derived = new RuntimeEntityHandle(51, 1);

            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(
                    new RuntimeEntityHandle(50, 1), owned, observed),
                Is.False);
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(derived, owned, observed),
                Is.True);
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(derived, owned, observed),
                Is.False);
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(
                    new RuntimeEntityHandle(51, 2), owned, observed),
                Is.True);
        }

        [Test]
        public void DerivedObservationPolicy_RejectsInvalidHandles()
        {
            Assert.That(
                ProductionEntityStressDerivedObservationPolicy.TryRecord(
                    RuntimeEntityHandle.Invalid,
                    new HashSet<RuntimeEntityHandle>(),
                    new HashSet<RuntimeEntityHandle>()),
                Is.False);
        }

        [Test]
        public void LoggingPolicy_SuppressesLogAndWarningThenRestoresTheOriginalFilter()
        {
            LogType currentFilter = LogType.Warning;
            var report = new ProductionEntityStressLoggingPolicyReport();
            var policy = new ProductionEntityStressLoggingPolicy(
                () => currentFilter,
                value => currentFilter = value);

            policy.Apply(report);
            policy.Apply(report);

            Assert.That(currentFilter, Is.EqualTo(LogType.Error));
            Assert.That(report.originalFilterLogType, Is.EqualTo(LogType.Warning.ToString()));
            Assert.That(report.runningFilterLogType, Is.EqualTo(LogType.Error.ToString()));
            Assert.That(report.applied, Is.True);
            Assert.That(report.restored, Is.False);
            Assert.That(report.policy, Does.Contain("Log and Warning"));

            policy.Restore(report);
            policy.Restore(report);

            Assert.That(currentFilter, Is.EqualTo(LogType.Warning));
            Assert.That(report.applied, Is.False);
            Assert.That(report.restored, Is.True);
        }
    }
}
#endif
