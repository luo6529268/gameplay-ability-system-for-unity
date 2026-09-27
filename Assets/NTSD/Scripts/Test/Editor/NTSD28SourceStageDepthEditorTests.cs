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

        private static SimulationWorld CreateWorld()
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(800, 180, 350, 0, 0);
            return world;
        }
    }
}
