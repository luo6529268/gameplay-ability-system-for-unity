#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28NonCharacterWalkableTtlEditorTests
    {
        [Test]
        public void Snapshot_UsesPolygonUnionAndIncludesEdges()
        {
            var area = new BattleWalkableAreaSnapshot(new[]
            {
                new[] { new Vector2(0, 0), new Vector2(1, 0),
                    new Vector2(1, -1), new Vector2(0, -1) },
                new[] { new Vector2(2, 0), new Vector2(3, 0),
                    new Vector2(3, -1), new Vector2(2, -1) },
            }, Vector2.zero, 0.01, 0.01);

            Assert.That(area.ContainsGroundPixel(50, 50), Is.True);
            Assert.That(area.ContainsGroundPixel(250, 50), Is.True);
            Assert.That(area.ContainsGroundPixel(100, 50), Is.True);
            Assert.That(area.ContainsGroundPixel(150, 50), Is.False);
        }

        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented)]
        [TestCase(BattleEcsCharacterPreFrameBoundsPassMode.Legacy)]
        public void OrdinaryWeapon_ClearsAfter304ElapsedLogicTicks(
            BattleEcsCharacterPreFrameBoundsPassMode mode)
        {
            SimulationWorld world = NewWorld(mode);
            LF2Weapon weapon = NewWeapon(world, 70, 150);

            AdvanceBounds(world, 1);
            Assert.That(weapon.Runtime.OutsideWalkableSinceTick, Is.EqualTo(1));
            AdvanceBounds(world, 304);
            Assert.That(world.FindEntityByRuntimeSlotForQuery(70), Is.SameAs(weapon));
            AdvanceBounds(world, 305);
            Assert.That(world.FindEntityByRuntimeSlotForQuery(70), Is.Null);
        }

        [Test]
        public void Reentry_ResetsTheContinuousOutsideInterval()
        {
            SimulationWorld world = NewWorld(
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented);
            LF2Weapon weapon = NewWeapon(world, 70, 150);

            AdvanceBounds(world, 1);
            weapon.Runtime.SetPosition(50, 0, 50);
            AdvanceBounds(world, 200);
            Assert.That(weapon.Runtime.OutsideWalkableSinceTick, Is.EqualTo(-1));
            weapon.Runtime.SetPosition(150, 0, 50);
            AdvanceBounds(world, 201);
            Assert.That(weapon.Runtime.OutsideWalkableSinceTick, Is.EqualTo(201));
            AdvanceBounds(world, 504);
            Assert.That(world.FindEntityByRuntimeSlotForQuery(70), Is.SameAs(weapon));
            AdvanceBounds(world, 505);
            Assert.That(world.FindEntityByRuntimeSlotForQuery(70), Is.Null);
        }

        [Test]
        public void SpecialAttackIsEligible_CharacterIsNot()
        {
            SimulationWorld world = NewWorld(
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented);
            var special = new LF2SpecialAttack();
            special.Runtime.EntityType = (int)LF2ObjectType.SpecialAttack;
            special.SetRequiredRuntimeSlot(71);
            world.Register(special);
            special.Runtime.SetPosition(150, 0, 50);
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(5);
            world.Register(character);
            character.Runtime.SetPosition(150, 0, 50);

            AdvanceBounds(world, 1);
            Assert.That(special.Runtime.OutsideWalkableSinceTick, Is.EqualTo(1));
            Assert.That(character.Runtime.OutsideWalkableSinceTick, Is.EqualTo(-1));
        }

        [Test]
        public void MissingWalkableData_DoesNotDeleteOrStartTimer()
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(800, 0, 100, 0, 0);
            LF2Weapon weapon = NewWeapon(world, 70, 900);

            AdvanceBounds(world, 1);
            AdvanceBounds(world, 500);

            Assert.That(world.FindEntityByRuntimeSlotForQuery(70), Is.SameAs(weapon));
            Assert.That(weapon.Runtime.OutsideWalkableSinceTick, Is.EqualTo(-1));
        }

        [Test]
        public void TimerCopiesResetsAndAffectsChecksum()
        {
            SimulationWorld world = NewWorld(
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented);
            LF2Weapon weapon = NewWeapon(world, 70, 150);
            ulong initial = world.CaptureRuntimeChecksum64(0, null);

            AdvanceBounds(world, 1);
            ulong outside = world.CaptureRuntimeChecksum64(0, null);
            Assert.That(outside, Is.Not.EqualTo(initial));

            var copy = new NTSDEntityRuntime();
            Assert.That(weapon.Runtime.TryCopyCanonicalStateTo(copy), Is.True);
            Assert.That(copy.OutsideWalkableSinceTick, Is.EqualTo(1));
            copy.Reset();
            Assert.That(copy.OutsideWalkableSinceTick, Is.EqualTo(-1));
        }

        [Test]
        public void EntityRuntimeSnapshot_KeepsTheOutsideStartTick()
        {
            SimulationWorld world = NewWorld(
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented);
            LF2Weapon weapon = NewWeapon(world, 70, 150);
            AdvanceBounds(world, 1);
            LockstepSessionIdentity identity =
                NTSD.Test.StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.RuntimeSlotCapacityForDiagnostics);

            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules, identity, 1), Is.True);
            weapon.Runtime.OutsideWalkableSinceTick = -1;
            var restored = new NTSDEntityRuntime();
            Assert.That(snapshot.TryCopyEntityRuntime(70, restored), Is.True);
            Assert.That(restored.OutsideWalkableSinceTick, Is.EqualTo(1));
        }

        private static SimulationWorld NewWorld(
            BattleEcsCharacterPreFrameBoundsPassMode mode)
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(800, 0, 100, 0, 0);
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(mode);
            world.SetWalkableAreaSnapshotForTesting(new BattleWalkableAreaSnapshot(
                new[] { new[] { new Vector2(0, 0), new Vector2(1, 0),
                    new Vector2(1, -1), new Vector2(0, -1) } },
                Vector2.zero, 0.01, 0.01));
            return world;
        }

        private static LF2Weapon NewWeapon(SimulationWorld world, int slot, double x)
        {
            var weapon = new LF2Weapon();
            weapon.SetWeaponType((int)LF2ObjectType.LightWeapon);
            weapon.SetRequiredRuntimeSlot(slot);
            world.Register(weapon);
            weapon.Runtime.SetPosition(x, 0, 50);
            return weapon;
        }

        private static void AdvanceBounds(SimulationWorld world, int tick)
        {
            world.AdvanceBattleFlowTick(tick);
            world.ApplyPreFrameBoundsAll();
        }
    }
}
#endif
