using System.Reflection;
using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28SourceCharacterStageXEditorTests
    {
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, 0, 20, 0, -150.0, -100.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, 0, 20, 0, -150.0, -100.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, 0, 20, 0, 900.0, 810.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, 0, 20, 0, 900.0, 810.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, 1, 20, 0, -49.0, 0.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, 1, 20, 0, -49.0, 0.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, 3, 20, 0, 900.0, 800.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, 3, 20, 0, 900.0, 800.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, 1, 5, 5, -100.0, 0.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, 1, 5, 5, -100.0, 0.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, 1, 5, -1, -350.0, -300.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, 1, 5, -1, -350.0, -300.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, 0, 5, 5, -350.0, -300.0)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, 0, 5, 5, -350.0, -300.0)]
        public void SelectedStageGate_UsesFormalType0BoundsInBothProfilesAndCoordinateDomains(
            BattleEcsCharacterPreFrameBoundsPassMode mode, int gate, int slot,
            int relationTeam, double start, double expected)
        {
            SimulationWorld world = CreateWorld(800);
            FieldInfo selectedGate = typeof(BattleRuntimeState).GetField(
                "SelectedModeStageGate50", BindingFlags.Public | BindingFlags.Instance);
            Assert.That(selectedGate, Is.Not.Null,
                "Selected mode stage gate must be a deterministic battle runtime scalar.");
            selectedGate.SetValue(world.Runtime, gate);
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(mode);
            LF2Character character = RegisterCharacter(world, slot, start);
            character.Runtime.RelationTeam = relationTeam;
            world.ApplyPreFrameBoundsAll();

            Assert.That(character.Runtime.X, Is.EqualTo(expected));
            Assert.That(character.Runtime.SourceRuleX, Is.EqualTo(expected));
            Assert.That(character.Runtime.XInt, Is.EqualTo((int)expected));
            Assert.That(character.Runtime.SourceRuleXInt, Is.EqualTo((int)expected));
        }

        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy)]
        public void StageEdge_ClampsPhysicalAndSynchronizesProjectedSource(
            BattleEcsCharacterPreFrameBoundsPassMode mode)
        {
            var world = CreateWorld(2048);
            world.ConfigureFixedViewRunDistance(2048, 1152);
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(mode);
            var character = RegisterCharacter(world, 5, 2000);
            character.Runtime.Vx = 40;

            new CharacterMechanics().StepBattleLogic(new CharacterMechanicsContext(
                character.Runtime, null, 0f, 0f, 0.0,
                world.FixedViewRunDistanceScale,
                world.FixedViewRunVerticalDistanceScale));
            Assert.That(character.Runtime.X,
                Is.GreaterThan(2048));
            Assert.That(character.Runtime.SourceRuleX, Is.EqualTo(2040));

            world.ApplyPreFrameBoundsAll();

            Assert.That(character.Runtime.X, Is.EqualTo(2048));
            Assert.That(character.Runtime.XInt, Is.EqualTo(2048));
            Assert.That(character.Runtime.SourceRuleX,
                Is.EqualTo(world.SpatialProjection.ViewToSourceX(2048)).Within(1e-10));
            Assert.That(character.Runtime.SourceRuleXInt, Is.EqualTo(1333));
        }

        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, false)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, false)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented, true)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy, true)]
        public void RepeatedProjectedWallImpact_LeavesCollisionAndViewAtSameBoundary(
            BattleEcsCharacterPreFrameBoundsPassMode mode, bool left)
        {
            SimulationWorld world = CreateWorld(2048);
            world.ConfigureFixedViewRunDistance(2048, 1152);
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(mode);
            LF2Character character = RegisterCharacter(world, 1, left ? 5 : 1327);
            NTSDEntityRuntime runtime = character.Runtime;
            runtime.X = world.SpatialProjection.SourceToViewX(runtime.SourceRuleX);
            runtime.SyncIntegerPosition();
            for (int impact = 0; impact < 4; impact++)
            {
                double delta = left ? -40 : 40;
                runtime.SourceRuleX += delta;
                runtime.X += world.SpatialProjection.SourceDeltaToViewX(delta);
                runtime.SyncIntegerPosition();
                runtime.SyncSourceRuleIntegerPosition();
                world.ApplyPreFrameBoundsAll();
                Assert.That(runtime.X, Is.EqualTo(left ? 0 : 2048));
                Assert.That(runtime.SourceRuleX, Is.EqualTo(left ? 0 : 1333).Within(1e-10));
                Assert.That(runtime.SourceRuleXInt, Is.EqualTo(left ? 0 : 1333));
                Assert.That(world.SpatialProjection.SourceToViewX(runtime.SourceRuleX),
                    Is.EqualTo(runtime.X).Within(1e-10), "Collision source must not keep moving through the wall.");
            }
        }

        [Test]
        public void UnclampedProjectedMotion_PreservesSourceBits()
        {
            SimulationWorld world = CreateWorld(2048);
            world.ConfigureFixedViewRunDistance(2048, 1152);
            LF2Character character = RegisterCharacter(world, 1, 123.987654321);
            character.Runtime.X = world.SpatialProjection.SourceToViewX(character.Runtime.SourceRuleX);
            character.Runtime.SyncIntegerPosition();
            long before = System.BitConverter.DoubleToInt64Bits(character.Runtime.SourceRuleX);
            world.ApplyPreFrameBoundsAll();
            Assert.That(System.BitConverter.DoubleToInt64Bits(character.Runtime.SourceRuleX), Is.EqualTo(before));
        }

        [TestCase(5, 0, 0, -50.75, 0.0)]
        [TestCase(5, 5, 0, -350.75, -300.0)]
        [TestCase(20, 0, 0, -150.75, -100.0)]
        [TestCase(20, 0, 0, 950.75, 810.0)]
        [TestCase(5, 0, 0, 750.75, 700.0)]
        [TestCase(5, 0, 10, 750.75, 750.75)]
        [TestCase(5, 5, 0, 750.75, 750.75)]
        public void SourceCharacterBounds_UseSlotTeamPhaseAndHitStop(
            int slot,
            int team,
            int hitStop,
            double start,
            double expected)
        {
            var runtime = new NTSDEntityRuntime();
            runtime.SetSourceRulePosition(start, 250);
            runtime.SyncSourceRuleIntegerPosition();

            runtime.ClampSourceRuleCharacterX(slot, team, hitStop, 800, 700);

            Assert.That(runtime.SourceRuleX, Is.EqualTo(expected));
            Assert.That(runtime.SourceRuleXInt, Is.EqualTo((int)expected));
        }

        [Test]
        public void DerivedCharacter_UsesCompatibilitySourceWriter()
        {
            var world = CreateWorld(800);
            var character = new DerivedCharacter();
            character.SetRequiredRuntimeSlot(25);
            world.Register(character);
            character.Runtime.SetPosition(-200, 0, 250);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.SetSourceRulePosition(-150.75, 250);
            character.Runtime.SyncSourceRuleIntegerPosition();

            world.ApplyPreFrameBoundsAll();

            Assert.That(character.Runtime.X, Is.EqualTo(-100));
            Assert.That(character.Runtime.SourceRuleX, Is.EqualTo(-100));
            Assert.That(character.Runtime.SourceRuleXInt, Is.EqualTo(-100));
        }

        [Test]
        public void MissingSourceCarrier_DoesNotStartOnStageClamp()
        {
            var world = CreateWorld(800);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            character.Runtime.SetPosition(900, 0, 250);
            world.ApplyPreFrameBoundsAll();
            Assert.That(character.Runtime.X, Is.EqualTo(800));
            Assert.That(character.Runtime.SourceRulePositionInitialized, Is.False);
            Assert.That(character.Runtime.SourceRuleX, Is.Zero);
        }

        private static SimulationWorld CreateWorld(int width)
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(
                width, 180, 350, 0, 0);
            return world;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            double x)
        {
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            character.Runtime.SetPosition(x, 0, 250);
            character.Runtime.SyncIntegerPosition();
            character.Runtime.SetSourceRulePosition(x, 250);
            character.Runtime.SyncSourceRuleIntegerPosition();
            return character;
        }

        private sealed class DerivedCharacter : LF2Character
        {
        }
    }
}
