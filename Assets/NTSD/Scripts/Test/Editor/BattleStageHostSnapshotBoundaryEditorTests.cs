#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleStageHostSnapshotBoundaryEditorTests
    {
        [Test]
        public void HostPreparedSnapshot_IsReusedAcrossKernelStagePasses()
        {
            var world = new SimulationWorld();

            world.PrepareStageRuntimeSnapshotForTick(17);
            long refreshCount = world.StageRuntimeSceneRefreshCountForDiagnostics;
            world.ClampCharacterZToStageBoundsAll();
            world.ClampCharacterZToStageBoundsAll();
            world.ApplyPreFrameBoundsAll();

            Assert.That(refreshCount, Is.EqualTo(1));
            Assert.That(
                world.StageRuntimeSceneRefreshCountForDiagnostics,
                Is.EqualTo(refreshCount));
            Assert.That(world.StageRuntimeHostPrepareCountForDiagnostics, Is.EqualTo(1));
            Assert.That(
                world.StageRuntimeLegacyPerPassRefreshCountForDiagnostics,
                Is.Zero);
        }

        [Test]
        public void SameHostTick_DeduplicatesAndNextTickRefreshes()
        {
            var world = new SimulationWorld();

            world.PrepareStageRuntimeSnapshotForTick(20);
            world.PrepareStageRuntimeSnapshotForTick(20);
            world.PrepareStageRuntimeSnapshotForTick(21);

            Assert.That(world.StageRuntimeSceneRefreshCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.StageRuntimeHostPrepareCountForDiagnostics, Is.EqualTo(2));
            Assert.That(world.StageRuntimeHostReuseCountForDiagnostics, Is.EqualTo(1));
        }

        [Test]
        public void LegacyPerPassMode_SkipsHostAndRefreshesEachAuthorityBoundary()
        {
            var world = new SimulationWorld();
            world.ConfigureLegacyPerPassStageRefreshForDiagnostics(true);

            world.PrepareStageRuntimeSnapshotForTick(31);
            world.ClampCharacterZToStageBoundsAll();
            world.ClampCharacterZToStageBoundsAll();
            world.ApplyPreFrameBoundsAll();

            Assert.That(world.StageRuntimeHostPrepareCountForDiagnostics, Is.Zero);
            Assert.That(world.StageRuntimeSceneRefreshCountForDiagnostics, Is.EqualTo(3));
            Assert.That(
                world.StageRuntimeLegacyPerPassRefreshCountForDiagnostics,
                Is.EqualTo(3));
        }

        [Test]
        public void ExplicitStageSnapshot_RemainsKernelTruthWithoutSceneRead()
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(1200, 140, 410, 3, 9);

            world.PrepareStageRuntimeSnapshotForTick(40);
            world.ClampCharacterZToStageBoundsAll();
            world.ApplyPreFrameBoundsAll();

            Assert.That(world.Runtime.Stage.BaseStageWidthPx, Is.EqualTo(1200));
            Assert.That(world.Runtime.Stage.ZMin, Is.EqualTo(140));
            Assert.That(world.Runtime.Stage.ZMax, Is.EqualTo(410));
            Assert.That(world.StageRuntimeSceneRefreshCountForDiagnostics, Is.Zero);
        }
    }
}
#endif
