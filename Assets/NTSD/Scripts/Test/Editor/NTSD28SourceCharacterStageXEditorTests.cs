using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28SourceCharacterStageXEditorTests
    {
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy)]
        public void StageEdge_ClampsPhysicalWhileKeepingRawSourceTravel(
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
            Assert.That(character.Runtime.SourceRuleX, Is.EqualTo(2040));
            Assert.That(character.Runtime.SourceRuleXInt, Is.EqualTo(2040));
        }

        [TestCase(5, 0, 0, -50.75, 0.0)]
        [TestCase(5, 5, 0, -350.75, -300.0)]
        [TestCase(20, 0, 0, -150.75, -100.0)]
        [TestCase(20, 0, 0, 950.75, 900.0)]
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
