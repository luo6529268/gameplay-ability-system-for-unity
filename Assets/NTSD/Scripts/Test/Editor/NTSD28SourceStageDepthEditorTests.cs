using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28SourceStageDepthEditorTests
    {
        [TestCase(BattleEcsCharacterStageZPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterStageZPassMode.Legacy)]
        [TestCase(BattleEcsCharacterStageZPassMode.ShadowCompare)]
        public void EarlyStageZ_ProjectsSourceClampIntoConfiguredViewForAllTypes(
            BattleEcsCharacterStageZPassMode mode)
        {
            var world = CreateWorld();
            world.ConfigureFixedViewRunDistance(2048, 1152);
            world.ConfigureBattleEcsCharacterStageZPassForDiagnostics(mode);

            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            character.Runtime.SetPosition(0, 0, 300);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.SetSourceRulePosition(0, 300);
            character.Runtime.SyncSourceRuleIntegerPosition();
            character.Runtime.Vz = 40;
            new CharacterMechanics().StepBattleLogic(new CharacterMechanicsContext(
                character.Runtime, null, 0f, 0f, 0.0,
                world.FixedViewRunDistanceScale,
                world.FixedViewRunVerticalDistanceScale));

            var weapon = new LF2Weapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.SetRequiredRuntimeSlot(70);
            world.Register(weapon);
            weapon.Runtime.SetPosition(0, 0, 500);
            weapon.Runtime.SyncIntegerPosition();
            weapon.Runtime.SetSourceRulePosition(0, 400.75);
            weapon.Runtime.SyncSourceRuleIntegerPosition();

            world.ClampCharacterZToStageBoundsAll();

            Assert.That(character.Runtime.Z,
                Is.EqualTo(300 + 40 * 1152.0 / 730.0).Within(1e-9));
            Assert.That(character.Runtime.SourceRuleZ, Is.EqualTo(340));
            Assert.That(character.Runtime.SourceRuleZInt, Is.EqualTo(340));
            Assert.That(weapon.Runtime.Z,
                Is.EqualTo(500 + (351 - 400.75) * 1152.0 / 730.0).Within(1e-9));
            Assert.That(weapon.Runtime.SourceRuleZ, Is.EqualTo(351));
            Assert.That(weapon.Runtime.SourceRuleZInt, Is.EqualTo(351));
            world.ClampCharacterZToStageBoundsAll();
            Assert.That(character.Runtime.Z,
                Is.EqualTo(300 + 40 * 1152.0 / 730.0).Within(1e-9));
            Assert.That(weapon.Runtime.Z,
                Is.EqualTo(500 + (351 - 400.75) * 1152.0 / 730.0).Within(1e-9));
            if (mode == BattleEcsCharacterStageZPassMode.ShadowCompare)
                Assert.That(world.BattleEcsCharacterStageZPassDiagnosticsForDiagnostics.IsClean,
                    Is.True);
        }

        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy)]
        public void PreFrameCharacter_ProjectsNearBoundaryCorrectionInBothPaths(
            BattleEcsCharacterPreFrameBoundsPassMode mode)
        {
            var world = CreateWorld();
            world.ConfigureFixedViewRunDistance(2048, 1152);
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(mode);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            character.Runtime.SetPosition(0, 0, 175);
            character.Runtime.SetSourceRulePosition(0, 177);
            character.Runtime.SyncSourceRuleIntegerPosition();

            world.ApplyPreFrameBoundsAll();

            Assert.That(character.Runtime.SourceRuleZ, Is.EqualTo(180));
            Assert.That(character.Runtime.SourceRuleZInt, Is.EqualTo(180));
            Assert.That(character.Runtime.Z,
                Is.EqualTo(175 + 3 * 1152.0 / 730.0).Within(1e-9));
            Assert.That(character.Runtime.ZInt, Is.EqualTo(179));
        }

        [TestCase(true, 180.0, 350.0)]
        [TestCase(false, 179.0, 351.0)]
        public void PreFrameFallback_UsesFormalTypeMarginAndTruncation(
            bool characterType,
            double expectedNear,
            double expectedFar)
        {
            LF2Entity entity = characterType
                ? new LF2Character()
                : new LF2Weapon();
            if (!characterType)
                ((LF2Weapon)entity).SetWeaponType((int)LF2ObjectType.LightWeapon);
            entity.Runtime.SetPosition(0, 0, 500);
            entity.Runtime.SetSourceRulePosition(0, 500.75);
            entity.Runtime.SyncSourceRuleIntegerPosition();

            entity.ApplyPreFrameZBounds(180, 350);
            Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(expectedFar));
            Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo((int)expectedFar));
            Assert.That(entity.Runtime.Z, Is.EqualTo(expectedFar));

            entity.Runtime.SetPosition(0, 0, 100);
            entity.Runtime.SetSourceRulePosition(0, 100.75);
            entity.Runtime.SyncSourceRuleIntegerPosition();
            entity.ApplyPreFrameZBounds(180, 350);
            Assert.That(entity.Runtime.SourceRuleZ, Is.EqualTo(expectedNear));
            Assert.That(entity.Runtime.SourceRuleZInt, Is.EqualTo((int)expectedNear));
            Assert.That(entity.Runtime.Z, Is.EqualTo(expectedNear));
        }

        [Test]
        public void MissingSourceCarrier_RemainsAbsentWhenPhysicalDepthClamps()
        {
            var world = CreateWorld();
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            character.Runtime.SetPosition(0, 0, 500);

            world.ClampCharacterZToStageBoundsAll();

            Assert.That(character.Runtime.Z, Is.EqualTo(350));
            Assert.That(character.Runtime.SourceRulePositionInitialized, Is.False);
            Assert.That(character.Runtime.SourceRuleZ, Is.Zero);
            Assert.That(character.Runtime.SourceRuleZInt, Is.Zero);
        }

        [Test]
        public void ExplicitSourceStage_ProjectsViewLimitsForMissingSourceFallback()
        {
            var world = CreateWorld();
            world.ConfigureFixedViewRunDistance(2048, 1152);
            Assert.That(world.StageDepthBoundsArePhysical, Is.False);
            Assert.That(world.TryGetStageRuleDepthBounds(
                out double sourceMin, out double sourceMax,
                out double viewMin, out double viewMax), Is.True);
            Assert.That(sourceMin, Is.EqualTo(180));
            Assert.That(sourceMax, Is.EqualTo(350));
            Assert.That(viewMin,
                Is.EqualTo(world.SpatialProjection.SourceToViewZ(180)).Within(1e-9));
            Assert.That(viewMax,
                Is.EqualTo(world.SpatialProjection.SourceToViewZ(350)).Within(1e-9));

            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            character.Runtime.SetPosition(0, 0, 100);
            world.ClampCharacterZToStageBoundsAll();
            Assert.That(character.Runtime.Z, Is.EqualTo(viewMin).Within(1e-9));
            Assert.That(character.Runtime.SourceRulePositionInitialized, Is.False);
        }

        [Test]
        public void ExplicitSourceStage_IdentityProjectionPreservesBothDomains()
        {
            var world = CreateWorld();
            Assert.That(world.TryGetStageRuleDepthBounds(
                out double sourceMin, out double sourceMax,
                out double viewMin, out double viewMax), Is.True);
            Assert.That(sourceMin, Is.EqualTo(180));
            Assert.That(sourceMax, Is.EqualTo(350));
            Assert.That(viewMin, Is.EqualTo(180));
            Assert.That(viewMax, Is.EqualTo(350));
        }

        [TestCase(BattleEcsCharacterStageZPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterStageZPassMode.Legacy)]
        [TestCase(BattleEcsCharacterStageZPassMode.ShadowCompare)]
        public void ProjectPhysicalStage_ClampsBothEdgesThroughSharedProjection(
            BattleEcsCharacterStageZPassMode mode)
        {
            var world = CreateProjectWorld();
            world.ConfigureBattleEcsCharacterStageZPassForDiagnostics(mode);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);

            AssertProjectEdge(world, character, 100, 237);
            AssertProjectEdge(world, character, 600, 760);
            if (mode == BattleEcsCharacterStageZPassMode.ShadowCompare)
                Assert.That(world.BattleEcsCharacterStageZPassDiagnosticsForDiagnostics.IsClean,
                    Is.True);
        }

        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy)]
        public void ProjectPhysicalPreFrame_ClampsBothEdgesThroughSharedProjection(
            BattleEcsCharacterPreFrameBoundsPassMode mode)
        {
            var world = CreateProjectWorld();
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(mode);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);

            SetSourceAndViewZ(world, character, 100);
            world.ApplyPreFrameBoundsAll();
            Assert.That(character.Runtime.Z, Is.EqualTo(237).Within(1e-9));
            SetSourceAndViewZ(world, character, 600);
            world.ApplyPreFrameBoundsAll();
            Assert.That(character.Runtime.Z, Is.EqualTo(760).Within(1e-9));
        }

        [Test]
        public void ProjectPhysicalStage_MissingSourceUsesPhysicalInterval()
        {
            var world = CreateProjectWorld();
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            character.Runtime.SetPosition(0, 0, 100);
            world.ClampCharacterZToStageBoundsAll();
            Assert.That(character.Runtime.Z, Is.EqualTo(237));
            Assert.That(character.Runtime.SourceRulePositionInitialized, Is.False);
        }

        [Test]
        public void ProjectPhysicalStage_NoncharacterKeepsOneSourcePixelMargin()
        {
            var world = CreateProjectWorld();
            var weapon = new LF2Weapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.SetRequiredRuntimeSlot(70);
            world.Register(weapon);
            double margin = world.SpatialProjection.SourceDeltaToViewZ(1);

            SetSourceAndViewZ(world, weapon, 100);
            world.ClampCharacterZToStageBoundsAll();
            Assert.That(weapon.Runtime.Z, Is.EqualTo(237 - margin).Within(1e-9));
            SetSourceAndViewZ(world, weapon, 600);
            world.ApplyPreFrameBoundsAll();
            Assert.That(weapon.Runtime.Z, Is.EqualTo(760 + margin).Within(1e-9));
        }

        [Test]
        public void ProjectPhysicalStage_WorkerCarrierCoreSnapshotAndChecksumKeepDomain()
        {
            var source = CreateProjectWorld();
            var stage = BattleSimulationStageSnapshot.Capture(
                source.Runtime.Stage, source.StageDepthBoundsArePhysical);
            var worker = CreateWorld();
            worker.ConfigureFixedViewRunDistance(2048, 1152);
            stage.Apply(worker);
            Assert.That(worker.StageDepthBoundsArePhysical, Is.True);
            Assert.That(worker.Runtime.Stage.ZMin, Is.EqualTo(237));
            Assert.That(worker.Runtime.Stage.ZMax, Is.EqualTo(760));
            Assert.That(worker.TryGetStageRuleDepthBounds(
                out double sourceMin, out double sourceMax,
                out _, out _), Is.True);
            Assert.That(sourceMin,
                Is.EqualTo(source.SpatialProjection.ViewToSourceZ(237)).Within(1e-9));
            Assert.That(sourceMax,
                Is.EqualTo(source.SpatialProjection.ViewToSourceZ(760)).Within(1e-9));

            var coreStage = new BattleWorldStageScalarSnapshot(
                worker.Runtime.Stage, worker.StageDepthBoundsArePhysical);
            Assert.That(coreStage.PhysicalDepthBounds, Is.True);
            ulong physicalChecksum = worker.CaptureRuntimeChecksum64(
                0, FrameInputSet.Empty(0));
            worker.SetStageDepthBoundsDomain(false);
            Assert.That(worker.CaptureRuntimeChecksum64(0, FrameInputSet.Empty(0)),
                Is.Not.EqualTo(physicalChecksum));
        }

        [Test]
        public void ProjectPhysicalStage_SourceAiUsesIntegerRuleBoundsButPhysicalFallbackDoesNot()
        {
            var world = CreateProjectWorld();
            world.BuildAiInputSlotSnapshot();
            AssertAiStageBounds(world, 237, 760);

            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            SetSourceAndViewZ(world, character, 400);
            world.BuildAiInputSlotSnapshot();
            AssertAiStageBounds(world, 151, 482);
        }

        private static void AssertAiStageBounds(
            SimulationWorld world, int expectedNear, int expectedFar)
        {
            Assert.That(world.StageZMin, Is.EqualTo(expectedNear));
            Assert.That(world.StageZMax, Is.EqualTo(expectedFar));
            var capture = typeof(SimulationWorld).GetMethod(
                "CaptureAiDecisionWorldState",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic);
            Assert.That(capture, Is.Not.Null);
            var state = (AiDecisionWorldState)capture.Invoke(world, null);
            Assert.That(state.StageZMin, Is.EqualTo(expectedNear));
            Assert.That(state.StageZMax, Is.EqualTo(expectedFar));
        }

        private static void AssertProjectEdge(
            SimulationWorld world, LF2Character character, double sourceZ, double expectedViewZ)
        {
            SetSourceAndViewZ(world, character, sourceZ);
            world.ClampCharacterZToStageBoundsAll();
            Assert.That(character.Runtime.Z, Is.EqualTo(expectedViewZ).Within(1e-9));
            Assert.That(character.Runtime.SourceRuleZ,
                Is.EqualTo(world.SpatialProjection.ViewToSourceZ(expectedViewZ)).Within(1e-9));
        }

        private static void SetSourceAndViewZ(
            SimulationWorld world, LF2Entity character, double sourceZ)
        {
            character.Runtime.SetPosition(0, 0,
                world.SpatialProjection.SourceToViewZ(sourceZ));
            character.Runtime.SetSourceRulePosition(0, sourceZ);
            character.Runtime.SyncSourceRuleIntegerPosition();
        }

        private static SimulationWorld CreateProjectWorld()
        {
            var world = CreateWorld();
            world.ConfigureFixedViewRunDistance(2048, 1152);
            world.Runtime.Stage.SetSceneSnapshot(800, 237, 760, 0, 0);
            world.SetStageDepthBoundsDomain(true);
            return world;
        }

        private static SimulationWorld CreateWorld()
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(800, 180, 350, 0, 0);
            return world;
        }
    }
}
