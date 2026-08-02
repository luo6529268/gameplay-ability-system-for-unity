#if UNITY_EDITOR
using System;
using System.Threading;
using NUnit.Framework;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class ProductionEntityStressDetailTimingEditorTests
    {
        [Test]
        public void LateEntityUpdateAll_DoesNotInvokeReservedCollisionCompatibilityHook()
        {
            var world = new SimulationWorld();
            var probe = new LateEntityCollisionGhostProbe(7000);
            probe.SetRequiredRuntimeSlot(0);
            world.Register(probe);
            BattleTickDetailPhaseDiagnostics recorder =
                world.EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            recorder.BeginTick(1);

            world.LateEntityUpdateAll(1);

            Assert.That(probe.FrameTickCount, Is.EqualTo(1));
            Assert.That(probe.EntityCollisionCount, Is.Zero);
            Assert.That(
                recorder.GetLastElapsedTimestampTicks((BattleTickDetailPhase)6),
                Is.Zero);
            Assert.That(
                BattleTickDetailPhaseDiagnostics.GetPhaseName((BattleTickDetailPhase)6),
                Is.EqualTo("Reserved/RemovedLateEntityCollision"));
        }

        [Test]
        public void DetailTiming_DefaultWorldKeepsRecorderUnallocatedAndInactive()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleTickDetailPhaseDiagnosticsAllocatedForDiagnostics,
                Is.False);
            Assert.That(
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics,
                Is.Null);
            Assert.That(
                world.BattleAiInputDetailDiagnosticsAllocatedForDiagnostics,
                Is.False);
            Assert.That(
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics,
                Is.Null);
        }

        [Test]
        public void DetailTiming_OneThousandRepeatedEntitySegmentsAccumulateWithinTick()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(100);

            for (int i = 0; i < 1000; i++)
            {
                recorder.BeginPhase(BattleTickDetailPhase.LateEntityFrameTick);
                Thread.SpinWait(100);
                recorder.EndPhase(BattleTickDetailPhase.LateEntityFrameTick);
            }

            long accumulated = recorder.GetLastElapsedTimestampTicks(
                BattleTickDetailPhase.LateEntityFrameTick);
            Assert.That(recorder.LastTickIndex, Is.EqualTo(100));
            Assert.That(accumulated, Is.GreaterThan(0));
        }

        [Test]
        public void LateRuntimeSnapshotTiming_DisabledRecorderHasNoAllocationsOrCounters()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(false);

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.StateSpecial);
                recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.StateSpecial);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.EqualTo(0));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.StateSpecial),
                Is.EqualTo(0));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotElapsedTimestampTicks(
                    BattleLateRuntimeSnapshotStage.StateSpecial),
                Is.EqualTo(0));
        }

        [Test]
        public void LateRuntimeSnapshotTiming_RetainsRemovedStagesAtZeroAndSurvivesNestedException()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(700);

            for (int i = 0; i < (int)BattleLateRuntimeSnapshotStage.Count; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                if (IsRemovedLateRuntimeSnapshotStage(stage))
                    continue;
                recorder.BeginLateRuntimeSnapshot(stage);
                Thread.SpinWait(2000);
                recorder.EndLateRuntimeSnapshot(stage);
            }

            recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.DeathOpoint);
            try
            {
                recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
                try
                {
                    throw new InvalidOperationException("focused diagnostics exception");
                }
                finally
                {
                    recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
                }
            }
            catch (InvalidOperationException)
            {
            }
            finally
            {
                recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.DeathOpoint);
            }

            for (int i = 0; i < (int)BattleLateRuntimeSnapshotStage.Count; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                Assert.That(
                    BattleTickDetailPhaseDiagnostics.GetLateRuntimeSnapshotStageName(stage),
                    Does.StartWith("LateEntityUpdate/RefreshRuntimeSnapshot/"));
                if (IsRemovedLateRuntimeSnapshotStage(stage))
                {
                    Assert.That(
                        recorder.GetLastLateRuntimeSnapshotCallCount(stage),
                        Is.EqualTo(0));
                    Assert.That(
                        recorder.GetLastLateRuntimeSnapshotElapsedTimestampTicks(stage),
                        Is.EqualTo(0));
                    continue;
                }
                Assert.That(
                    recorder.GetLastLateRuntimeSnapshotCallCount(stage),
                    Is.GreaterThanOrEqualTo(1));
                Assert.That(
                    recorder.GetLastLateRuntimeSnapshotElapsedTimestampTicks(stage),
                    Is.GreaterThan(0));
            }
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.Recovery),
                Is.EqualTo(0));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTickSuppressed),
                Is.EqualTo(0));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.CleanupCompleted),
                Is.EqualTo(0));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTick),
                Is.EqualTo(2));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.DeathOpoint),
                Is.EqualTo(2));
        }

        [Test]
        public void LateRuntimeSnapshotTiming_BeginTickResetsCountsAndElapsedTime()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(710);
            recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
            Thread.SpinWait(10000);
            recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);

            recorder.BeginTick(711);

            Assert.That(recorder.LastTickIndex, Is.EqualTo(711));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTick),
                Is.EqualTo(0));
            Assert.That(
                recorder.GetLastLateRuntimeSnapshotElapsedTimestampTicks(
                    BattleLateRuntimeSnapshotStage.FrameTick),
                Is.EqualTo(0));
        }

        [Test]
        public void LateRuntimeSnapshotTiming_EnabledSteadyStateAddsNoManagedAllocations()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(720);
            recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
            recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
                recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.EqualTo(0));
        }

        [Test]
        public void DetailTiming_BeginTickResetsAllNestedPhaseTotals()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(200);
            recorder.BeginPhase(BattleTickDetailPhase.CharacterInputSnapshotBuild);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickDetailPhase.CharacterInputSnapshotBuild);
            Assert.That(recorder.GetLastPhaseSumTimestampTicks(), Is.GreaterThan(0));

            recorder.BeginTick(201);

            Assert.That(recorder.LastTickIndex, Is.EqualTo(201));
            Assert.That(recorder.GetLastPhaseSumTimestampTicks(), Is.EqualTo(0));
        }

        [Test]
        public void DetailTimingCollector_PopulatesSeparateReportOnlyAfterWarmup()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            var collector = new ProductionEntityStressDetailPhaseTimingCollector();
            var request = new ProductionEntityStressRequest
            {
                action = "smoke",
                enableDetailPhaseTiming = true,
                outputPath = "Temp/detail-timing-focused.json",
            };
            ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            recorder.SetEnabled(true);

            recorder.BeginTick(1);
            recorder.BeginPhase(BattleTickDetailPhase.LateEntityFrameTick);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickDetailPhase.LateEntityFrameTick);
            collector.CaptureAfterTick(recorder, null, 1, 1);
            Assert.That(collector.SampleCount, Is.EqualTo(0));

            recorder.BeginTick(2);
            recorder.BeginPhase(BattleTickDetailPhase.LateEntityFrameTick);
            Thread.SpinWait(20000);
            recorder.EndPhase(BattleTickDetailPhase.LateEntityFrameTick);
            recorder.RecordPhaseElapsed(
                BattleTickDetailPhase.RenderBuildCommandsOverlay,
                1234);
            collector.CaptureAfterTick(recorder, null, 2, 1);

            var report = new ProductionEntityStressReport
            {
                detailPhaseTimingEnabled = config.EnableDetailPhaseTiming,
            };
            collector.PopulateReport(report);

            Assert.That(collector.SampleCount, Is.EqualTo(1));
            Assert.That(
                report.detailPhaseTimings,
                Has.Count.EqualTo(BattleTickDetailPhaseDiagnostics.PhaseCount));
            Assert.That(
                report.detailPhaseTimings[
                    (int)BattleTickDetailPhase.LateEntityFrameTick].timing.sampleCount,
                Is.EqualTo(1));
            Assert.That(
                report.detailPhaseTimings[6].phase,
                Does.Contain("Reserved"));
            Assert.That(
                report.detailPhaseTimings[6].timing.sampleCount,
                Is.EqualTo(1),
                "collector samples every phase slot per tick, including a reserved zero-duration slot");
            Assert.That(
                report.detailPhaseTimings[6].timing.average,
                Is.Zero);
            Assert.That(
                BattleTickDetailPhaseDiagnostics.PhaseCount,
                Is.EqualTo(40));
            Assert.That(
                report.detailPhaseTimings[
                    (int)BattleTickDetailPhase.RenderBuildCommandsOverlay].phase,
                Is.EqualTo("Render/BeginFrame/BuildCommands/Overlay"));
            Assert.That(
                report.detailPhaseTimings[
                    (int)BattleTickDetailPhase.RenderBuildCommandsOverlay].timing.average,
                Is.GreaterThan(0d));
            Assert.That(
                report.detailPhaseTimingSource,
                Is.EqualTo(ProductionEntityStressDetailPhaseTimingCollector.Source));
            Assert.That(report.detailPhaseTimingUnavailableReason, Is.Empty);
            Assert.That(
                report.lateRuntimeSnapshotTimings,
                Has.Count.EqualTo((int)BattleLateRuntimeSnapshotStage.Count));
            Assert.That(
                report.lateRuntimeSnapshotTimingSource,
                Is.EqualTo(
                    ProductionEntityStressDetailPhaseTimingCollector.LateRuntimeSnapshotSource));
            Assert.That(report.lateRuntimeSnapshotTimingUnavailableReason, Is.Empty);
            Assert.That(
                report.lateRuntimeSnapshotTimings[
                    (int)BattleLateRuntimeSnapshotStage.StateSpecial].callCount,
                Is.EqualTo(0));
            Assert.That(report.phaseTimings, Is.Empty);
        }

        [Test]
        public void AiInputDetailTimingCollector_CapturesEveryPhaseAsIndependentRollingSummary()
        {
            var detailRecorder = new BattleTickDetailPhaseDiagnostics();
            var aiRecorder = new BattleAiInputDetailDiagnostics();
            var collector = new ProductionEntityStressDetailPhaseTimingCollector();
            detailRecorder.SetEnabled(true);
            aiRecorder.SetEnabled(true);
            detailRecorder.BeginTick(10);
            aiRecorder.BeginTick(10);

            for (int i = 0; i < BattleAiInputDetailDiagnostics.PhaseCount; i++)
            {
                BattleAiInputDetailPhase phase = (BattleAiInputDetailPhase)i;
                aiRecorder.BeginPhase(phase);
                Thread.SpinWait(20000);
                aiRecorder.EndPhase(phase);
                aiRecorder.RecordPhaseCall(phase);
                aiRecorder.RecordPhaseSlotVisits(phase, i + 1);
                aiRecorder.RecordPhaseRngCalls(phase, (ulong)(i % 3));
            }
            aiRecorder.RecordAi();
            collector.CaptureAfterTick(detailRecorder, aiRecorder, 10, 9);

            var report = new ProductionEntityStressReport
            {
                detailPhaseTimingEnabled = true,
            };
            collector.PopulateReport(report);

            Assert.That(collector.AiInputSampleCount, Is.EqualTo(1));
            Assert.That(
                collector.AiInputPhaseSamplesAllocatedForDiagnostics,
                Is.True);
            Assert.That(
                report.aiInputDetailTimings,
                Has.Count.EqualTo(BattleAiInputDetailDiagnostics.PhaseCount));
            Assert.That(
                report.aiInputDetailTimingSource,
                Is.EqualTo(ProductionEntityStressDetailPhaseTimingCollector.AiInputSource));
            Assert.That(report.aiInputDetailTimingUnavailableReason, Is.Empty);
            for (int i = 0; i < report.aiInputDetailTimings.Count; i++)
            {
                ProductionEntityStressPhaseTimingSummary summary =
                    report.aiInputDetailTimings[i];
                BattleAiInputDetailPhase phase = (BattleAiInputDetailPhase)i;
                Assert.That(
                    summary.phase,
                    Is.EqualTo(BattleAiInputDetailDiagnostics.GetPhaseName(phase)));
                Assert.That(summary.timing.available, Is.True);
                Assert.That(summary.timing.sampleCount, Is.EqualTo(1));
                Assert.That(summary.timing.average, Is.GreaterThan(0d));
                Assert.That(summary.timing.p95, Is.EqualTo(summary.timing.average));
                Assert.That(summary.timing.source, Is.EqualTo(
                    ProductionEntityStressDetailPhaseTimingCollector.AiInputSource));
                Assert.That(
                    report.aiInputDetailCounters.phaseCallCounts[i],
                    Is.EqualTo(1));
                Assert.That(
                    report.aiInputDetailCounters.phaseSlotVisitCounts[i],
                    Is.EqualTo(i + 1));
                Assert.That(
                    report.aiInputDetailCounters.phaseRngCallCounts[i],
                    Is.EqualTo(i % 3));
            }
        }

        [Test]
        public void AiInputDetailTimingCollector_DefaultReportIsUnavailableWithoutAllocatingSamples()
        {
            var collector = new ProductionEntityStressDetailPhaseTimingCollector();
            var report = new ProductionEntityStressReport();

            collector.PopulateReport(report);

            Assert.That(
                collector.AiInputPhaseSamplesAllocatedForDiagnostics,
                Is.False);
            Assert.That(report.aiInputDetailTimings, Is.Empty);
            Assert.That(report.aiInputDetailTimingSource, Is.Empty);
            Assert.That(
                report.aiInputDetailTimingUnavailableReason,
                Does.Contain("Disabled by request"));
            Assert.That(report.aiInputDetailCounters.available, Is.False);
            Assert.That(report.lateRuntimeSnapshotTimings, Is.Empty);
            Assert.That(report.lateRuntimeSnapshotTimingSource, Is.Empty);
            Assert.That(
                report.lateRuntimeSnapshotTimingUnavailableReason,
                Does.Contain("Disabled by request"));
        }

        [Test]
        public void LateRuntimeSnapshotTimingCollector_SerializesCallsAndMillisecondsByStableStage()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            var collector = new ProductionEntityStressDetailPhaseTimingCollector();
            recorder.SetEnabled(true);
            recorder.BeginTick(730);
            recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
            Thread.SpinWait(20000);
            recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.FrameTick);
            recorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.TailAndQueuedFlush);
            Thread.SpinWait(20000);
            recorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.TailAndQueuedFlush);
            collector.CaptureAfterTick(recorder, null, 1, 0);

            var report = new ProductionEntityStressReport
            {
                detailPhaseTimingEnabled = true,
            };
            collector.PopulateReport(report);
            string json = UnityEngine.JsonUtility.ToJson(report);
            ProductionEntityStressLateRuntimeSnapshotTimingSummary stateSpecial =
                report.lateRuntimeSnapshotTimings[
                    (int)BattleLateRuntimeSnapshotStage.StateSpecial];
            ProductionEntityStressLateRuntimeSnapshotTimingSummary frameTick =
                report.lateRuntimeSnapshotTimings[
                    (int)BattleLateRuntimeSnapshotStage.FrameTick];
            ProductionEntityStressLateRuntimeSnapshotTimingSummary frameExit =
                report.lateRuntimeSnapshotTimings[
                    (int)BattleLateRuntimeSnapshotStage.FrameExit];
            ProductionEntityStressLateRuntimeSnapshotTimingSummary tailAndQueuedFlush =
                report.lateRuntimeSnapshotTimings[
                    (int)BattleLateRuntimeSnapshotStage.TailAndQueuedFlush];
            ProductionEntityStressLateRuntimeSnapshotTimingSummary prevFrameMirror =
                report.lateRuntimeSnapshotTimings[
                    (int)BattleLateRuntimeSnapshotStage.PrevFrameMirror];

            Assert.That(collector.LateRuntimeSnapshotSampleCount, Is.EqualTo(1));
            Assert.That(collector.LateRuntimeSnapshotSamplesAllocatedForDiagnostics, Is.True);
            Assert.That(stateSpecial.callCount, Is.EqualTo(0));
            Assert.That(stateSpecial.timing.available, Is.True);
            Assert.That(stateSpecial.timing.average, Is.EqualTo(0d));
            Assert.That(frameTick.callCount, Is.EqualTo(1));
            Assert.That(frameTick.timing.average, Is.GreaterThan(0d));
            Assert.That(frameExit.callCount, Is.EqualTo(0));
            Assert.That(frameExit.timing.average, Is.EqualTo(0d));
            Assert.That(tailAndQueuedFlush.callCount, Is.EqualTo(1));
            Assert.That(tailAndQueuedFlush.timing.average, Is.GreaterThan(0d));
            Assert.That(prevFrameMirror.callCount, Is.EqualTo(0));
            Assert.That(prevFrameMirror.timing.average, Is.EqualTo(0d));
            Assert.That(json, Does.Contain("lateRuntimeSnapshotTimings"));
            Assert.That(json, Does.Contain("callCount"));
            Assert.That(json, Does.Contain("StateSpecial"));
        }

        private static bool IsRemovedLateRuntimeSnapshotStage(
            BattleLateRuntimeSnapshotStage stage)
        {
            return stage == BattleLateRuntimeSnapshotStage.StateSpecial ||
                   stage == BattleLateRuntimeSnapshotStage.Recovery ||
                   stage == BattleLateRuntimeSnapshotStage.FrameTickSuppressed ||
                   stage == BattleLateRuntimeSnapshotStage.FrameExit ||
                   stage == BattleLateRuntimeSnapshotStage.CleanupCompleted ||
                   stage == BattleLateRuntimeSnapshotStage.PrevFrameMirror;
        }

        [Test]
        public void DetailTimingLifecycle_CleanupDisablesTopLevelAndDetailRecorders()
        {
            var world = new SimulationWorld();
            BattleTickPhaseDiagnostics phaseRecorder =
                world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
            BattleTickDetailPhaseDiagnostics detailRecorder =
                world.EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            BattleAiInputDetailDiagnostics aiRecorder =
                world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            aiRecorder.BeginTick(1);
            aiRecorder.BeginPhase(BattleAiInputDetailPhase.ComboUpdate);
            Thread.SpinWait(20000);
            aiRecorder.EndPhase(BattleAiInputDetailPhase.ComboUpdate);
            detailRecorder.BeginTick(1);
            detailRecorder.BeginLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.StateSpecial);
            Thread.SpinWait(20000);
            detailRecorder.EndLateRuntimeSnapshot(BattleLateRuntimeSnapshotStage.StateSpecial);
            Assert.That(
                aiRecorder.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.ComboUpdate),
                Is.GreaterThan(0));
            Assert.That(
                detailRecorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.StateSpecial),
                Is.EqualTo(1));

            ProductionEntityStressPhaseTimingLifecycle.Disable(world);

            Assert.That(phaseRecorder.Enabled, Is.False);
            Assert.That(detailRecorder.Enabled, Is.False);
            Assert.That(aiRecorder.Enabled, Is.False);
            Assert.That(
                world.ActiveBattleTickPhaseDiagnosticsForDiagnostics,
                Is.Null);
            Assert.That(
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics,
                Is.Null);
            Assert.That(
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics,
                Is.Null);
            Assert.That(aiRecorder.LastTickIndex, Is.EqualTo(-1));
            Assert.That(
                aiRecorder.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.ComboUpdate),
                Is.EqualTo(0));
            Assert.That(
                detailRecorder.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.StateSpecial),
                Is.EqualTo(0));
        }

        [Test]
        public void DetailTimingRecorder_EnabledSteadyStateAddsNoManagedAllocations()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(300);
            recorder.BeginPhase(BattleTickDetailPhase.LateEntityRecovery);
            recorder.EndPhase(BattleTickDetailPhase.LateEntityRecovery);

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                recorder.BeginPhase(BattleTickDetailPhase.LateEntityRecovery);
                recorder.EndPhase(BattleTickDetailPhase.LateEntityRecovery);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.EqualTo(0));
        }

        [Test]
        public void AiInputDetail_OneThousandNestedSegmentsAccumulateAndResetPerTick()
        {
            var recorder = new BattleAiInputDetailDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(400);

            for (int i = 0; i < 1000; i++)
            {
                recorder.BeginPhase(BattleAiInputDetailPhase.RemainingAiDecision);
                recorder.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
                Thread.SpinWait(50);
                recorder.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
                recorder.EndPhase(BattleAiInputDetailPhase.RemainingAiDecision);
                recorder.RecordAi();
                recorder.RecordSpatialQuery();
            }

            Assert.That(recorder.AiCount, Is.EqualTo(1000));
            Assert.That(recorder.SpatialQueryCount, Is.EqualTo(1000));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleAiInputDetailPhase.FindNearestGround),
                Is.GreaterThan(0));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleAiInputDetailPhase.RemainingAiDecision),
                Is.GreaterThan(0));

            recorder.BeginTick(401);
            Assert.That(recorder.LastTickIndex, Is.EqualTo(401));
            Assert.That(recorder.AiCount, Is.EqualTo(0));
            Assert.That(recorder.SpatialQueryCount, Is.EqualTo(0));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(BattleAiInputDetailPhase.FindNearestGround),
                Is.EqualTo(0));
        }

        [Test]
        public void AiInputDetail_EnabledSteadyStateAddsNoManagedAllocations()
        {
            var recorder = new BattleAiInputDetailDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(500);

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                recorder.BeginPhase(BattleAiInputDetailPhase.FindNearestGround);
                recorder.RecordCandidateVisits(4);
                recorder.RecordRadius(64);
                recorder.RecordPhaseCall(
                    BattleAiInputDetailPhase.ContextMoveMode);
                recorder.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    4);
                recorder.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    2);
                recorder.EndPhase(BattleAiInputDetailPhase.FindNearestGround);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.EqualTo(0));
            Assert.That(
                recorder.GetLastCallCount(
                    BattleAiInputDetailPhase.ContextMoveMode),
                Is.EqualTo(1000));
            Assert.That(
                recorder.GetLastSlotVisitCount(
                    BattleAiInputDetailPhase.ContextMoveMode),
                Is.EqualTo(4000));
            Assert.That(
                recorder.GetLastRngCallCount(
                    BattleAiInputDetailPhase.ContextMoveMode),
                Is.EqualTo(2000));
        }

        [Test]
        public void AiInputDetail_DisabledPhaseCountersStayZeroAndAllocateNothing()
        {
            var recorder = new BattleAiInputDetailDiagnostics();

            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++)
            {
                recorder.BeginPhase(BattleAiInputDetailPhase.ContextMoveMode);
                recorder.RecordPhaseCall(
                    BattleAiInputDetailPhase.ContextMoveMode);
                recorder.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    10);
                recorder.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    3);
                recorder.EndPhase(BattleAiInputDetailPhase.ContextMoveMode);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleAiInputDetailPhase.ContextMoveMode),
                Is.Zero);
            Assert.That(
                recorder.GetLastCallCount(
                    BattleAiInputDetailPhase.ContextMoveMode),
                Is.Zero);
            Assert.That(
                recorder.GetLastSlotVisitCount(
                    BattleAiInputDetailPhase.ContextMoveMode),
                Is.Zero);
            Assert.That(
                recorder.GetLastRngCallCount(
                    BattleAiInputDetailPhase.ContextMoveMode),
                Is.Zero);
        }

        private sealed class LateEntityCollisionGhostProbe : LF2Entity
        {
            public LateEntityCollisionGhostProbe(int stableId)
            {
                StableId = stableId;
            }

            public int FrameTickCount { get; private set; }
            public int EntityCollisionCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

            public override void SimFrameTick(int tickIndex)
            {
                FrameTickCount++;
            }

            public override void SimEntityCollision(int tickIndex)
            {
                EntityCollisionCount++;
            }

            public override void Reset()
            {
            }

            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
            {
                Renderer = renderer;
            }
        }
    }
}
#endif
