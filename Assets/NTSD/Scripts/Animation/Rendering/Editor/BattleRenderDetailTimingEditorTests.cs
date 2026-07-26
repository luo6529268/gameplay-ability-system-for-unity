#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Threading;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderDetailTimingEditorTests
    {
        [Test]
        public void DisabledWorld_DoesNotAllocateOrActivateDetailRecorder()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleTickDetailPhaseDiagnosticsAllocatedForDiagnostics,
                Is.False);
            Assert.That(
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics,
                Is.Null);
        }

        [Test]
        public void NestedRenderPhases_AccumulateWithoutReplacingParentPhase()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(12);
            recorder.BeginPhase(BattleTickDetailPhase.RenderBeginFrame);
            Thread.SpinWait(1000);
            recorder.BeginPhase(BattleTickDetailPhase.RenderBeginFrameSortEntities);
            Thread.SpinWait(1000);
            recorder.EndPhase(BattleTickDetailPhase.RenderBeginFrameSortEntities);
            recorder.EndPhase(BattleTickDetailPhase.RenderBeginFrame);

            long parent = recorder.GetLastElapsedTimestampTicks(
                BattleTickDetailPhase.RenderBeginFrame);
            long child = recorder.GetLastElapsedTimestampTicks(
                BattleTickDetailPhase.RenderBeginFrameSortEntities);
            Assert.That(parent, Is.GreaterThan(0));
            Assert.That(child, Is.GreaterThan(0));
            Assert.That(parent, Is.GreaterThanOrEqualTo(child));
        }

        [Test]
        public void DisabledRecorder_IgnoresPhasesAndDeferredRenderTiming()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();

            recorder.BeginTick(20);
            recorder.BeginPhase(BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
            recorder.EndPhase(BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
            recorder.RecordDeferredPhaseElapsed(
                BattleTickDetailPhase.RenderExecuteCommandBuffer,
                1234);

            Assert.That(recorder.LastTickIndex, Is.EqualTo(-1));
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.RenderPrepareFrameResolveCommands),
                Is.Zero);
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.RenderExecuteCommandBuffer),
                Is.Zero);
        }

        [Test]
        public void DeferredExecuteCommandBufferTiming_IsConsumedByNextTickOnly()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.RecordDeferredPhaseElapsed(
                BattleTickDetailPhase.RenderExecuteCommandBuffer,
                1234);

            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.RenderExecuteCommandBuffer),
                Is.Zero);
            recorder.BeginTick(30);
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.RenderExecuteCommandBuffer),
                Is.EqualTo(1234));
            recorder.BeginTick(31);
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.RenderExecuteCommandBuffer),
                Is.Zero);
        }

        [Test]
        public void DeferredMaterializationTiming_IsPublishedIntoNextTickOnly()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);

            Assert.That(recorder.BeginDeferredRenderMaterialization(), Is.True);
            recorder.BeginPhase(
                BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard);
            Thread.SpinWait(1000);
            recorder.BeginPhase(BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
            Thread.SpinWait(1000);
            recorder.EndPhase(BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
            recorder.EndPhase(
                BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard);
            recorder.EndDeferredRenderMaterialization();

            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.RenderPrepareFrameResolveCommands),
                Is.Zero);

            recorder.BeginTick(35);
            long outer = recorder.GetLastElapsedTimestampTicks(
                BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard);
            long resolve = recorder.GetLastElapsedTimestampTicks(
                BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
            Assert.That(outer, Is.GreaterThan(0));
            Assert.That(resolve, Is.GreaterThan(0));
            Assert.That(outer, Is.GreaterThanOrEqualTo(resolve));

            recorder.BeginTick(36);
            Assert.That(
                recorder.GetLastElapsedTimestampTicks(
                    BattleTickDetailPhase.RenderPrepareFrameResolveCommands),
                Is.Zero);
        }

        [Test]
        public void DetailReport_ContainsEveryRenderSubPhaseWithStableNames()
        {
            var recorder = new BattleTickDetailPhaseDiagnostics();
            recorder.SetEnabled(true);
            recorder.BeginTick(40);
            BattleTickDetailPhase[] phases =
            {
                BattleTickDetailPhase.RenderBeginFrameSortEntities,
                BattleTickDetailPhase.RenderBeginFrameCaptureHitRecords,
                BattleTickDetailPhase.RenderBeginFrameCaptureEntities,
                BattleTickDetailPhase.RenderBeginFrameBuildCommands,
                BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy,
                BattleTickDetailPhase.RenderPrepareFrameResolveCommands,
                BattleTickDetailPhase.RenderPrepareFrameWriteQuads,
                BattleTickDetailPhase.RenderPrepareFrameSetVertexBufferData,
                BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes,
            };
            for (int index = 0; index < phases.Length; index++)
            {
                recorder.BeginPhase(phases[index]);
                Thread.SpinWait(100);
                recorder.EndPhase(phases[index]);
            }
            recorder.RecordDeferredPhaseElapsed(
                BattleTickDetailPhase.RenderExecuteCommandBuffer,
                100);
            recorder.BeginTick(41);

            var collector = new ProductionEntityStressDetailPhaseTimingCollector();
            collector.CaptureAfterTick(recorder, null, 1, 0);
            var report = new ProductionEntityStressReport
            {
                detailPhaseTimingEnabled = true,
            };
            collector.PopulateReport(report);

            Assert.That(
                report.detailPhaseTimings,
                Has.Count.EqualTo(BattleTickDetailPhaseDiagnostics.PhaseCount));
            AssertReportContains(report, "Render/BeginFrame/SortEntities");
            AssertReportContains(report, "Render/BeginFrame/CaptureHitRecords");
            AssertReportContains(report, "Render/BeginFrame/CaptureEntities");
            AssertReportContains(report, "Render/BeginFrame/BuildCommands");
            AssertReportContains(report, "Render/PrepareFrame/FrozenFrameCopy");
            AssertReportContains(report, "Render/PrepareFrame/ResolveCommands");
            AssertReportContains(report, "Render/PrepareFrame/WriteQuads");
            AssertReportContains(report, "Render/PrepareFrame/SetVertexBufferData");
            AssertReportContains(report, "Render/PrepareFrame/SetSubMeshes");
            AssertReportContains(report, "Render/ExecuteCommandBuffer");
        }

        private static void AssertReportContains(
            ProductionEntityStressReport report,
            string phaseName)
        {
            for (int index = 0; index < report.detailPhaseTimings.Count; index++)
            {
                if (report.detailPhaseTimings[index].phase == phaseName)
                    return;
            }
            Assert.Fail("Missing detail timing phase: " + phaseName);
        }
    }
}
#endif
