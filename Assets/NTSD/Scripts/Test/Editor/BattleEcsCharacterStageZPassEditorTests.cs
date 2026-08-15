#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsCharacterStageZPassEditorTests
    {
        [Test]
        public void DefaultMode_IsDataOrientedAfterCanonicalWriterClosureAndCannotSwitchAfterResetBoundary()
        {
            var world = CreateWorld();

            Assert.That(
                world.BattleEcsCharacterStageZPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterStageZPassMode.DataOriented));

            world.AdvanceBattleFlowTick(1);
            Assert.Throws<InvalidOperationException>(() =>
                world.ConfigureBattleEcsCharacterStageZPassForDiagnostics(
                    BattleEcsCharacterStageZPassMode.DataOriented));
        }

        [Test]
        public void ShadowCompare_ExactlyMatchesLegacyWriter()
        {
            var world = CreateWorld(zMin: -10, zMax: 10);
            LF2Character above = RegisterCharacter(world, 50, 20.75);
            LF2Character inside = RegisterCharacter(world, 51, -3.9);
            LF2Character below = RegisterCharacter(world, 52, -20.25);
            world.ConfigureBattleEcsCharacterStageZPassForDiagnostics(
                BattleEcsCharacterStageZPassMode.ShadowCompare);

            world.ClampCharacterZToStageBoundsAll();

            BattleEcsCharacterStageZPassDiagnostics diagnostics =
                world.BattleEcsCharacterStageZPassDiagnosticsForDiagnostics;
            Assert.That(above.Runtime.Z, Is.EqualTo(10.0));
            Assert.That(inside.Runtime.Z, Is.EqualTo(-3.9));
            Assert.That(inside.Runtime.ZInt, Is.EqualTo(-3));
            Assert.That(below.Runtime.Z, Is.EqualTo(-10.0));
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(3));
            Assert.That(diagnostics.ValidationCount, Is.EqualTo(1));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.IsClean, Is.True);
        }

        [Test]
        public void DataOrientedWriter_MatchesLegacyAndSkipsInactiveMembership()
        {
            SimulationWorld legacy = CreateContractWorld(
                BattleEcsCharacterStageZPassMode.Legacy,
                out LF2Character[] legacyEntities);
            SimulationWorld dataOriented = CreateContractWorld(
                BattleEcsCharacterStageZPassMode.DataOriented,
                out LF2Character[] dataEntities);

            legacy.ClampCharacterZToStageBoundsAll();
            dataOriented.ClampCharacterZToStageBoundsAll();

            for (int i = 0; i < legacyEntities.Length; i++)
            {
                Assert.That(
                    BitConverter.DoubleToInt64Bits(dataEntities[i].Runtime.Z),
                    Is.EqualTo(BitConverter.DoubleToInt64Bits(
                        legacyEntities[i].Runtime.Z)),
                    $"Z mismatch at contract entity {i}");
                Assert.That(
                    dataEntities[i].Runtime.ZInt,
                    Is.EqualTo(legacyEntities[i].Runtime.ZInt),
                    $"ZInt mismatch at contract entity {i}");
            }

            Assert.That(dataEntities[0].Runtime.Z, Is.EqualTo(350.0));
            Assert.That(dataEntities[1].Runtime.Z, Is.EqualTo(180.0));
            Assert.That(dataEntities[2].Runtime.Z, Is.EqualTo(225.75));
            Assert.That(dataEntities[3].Runtime.Z, Is.EqualTo(500.0),
                "pending-destroy membership must be skipped");
            Assert.That(dataEntities[4].Runtime.Z, Is.EqualTo(500.0),
                "dormant membership must be skipped");

            BattleEcsCharacterStageZPassDiagnostics diagnostics =
                dataOriented.BattleEcsCharacterStageZPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(3));
            Assert.That(diagnostics.DerivedTypeFallbackCount, Is.Zero);
        }

        [Test]
        public void Extended1000_WarmedDataOrientedWriterDoesNotAllocate()
        {
            const int capacity = 1050;
            const int firstSlot = 50;
            const int entityCount = 1000;
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                capacity);
            world.SetExplicitStageRuntimeSnapshotForTesting(
                800,
                180,
                350,
                0,
                0);

            for (int slot = firstSlot; slot < capacity; slot++)
                RegisterCharacter(world, slot, 100.25 + (slot % 400));

            world.ConfigureBattleEcsCharacterStageZPassForDiagnostics(
                BattleEcsCharacterStageZPassMode.DataOriented);
            world.ClampCharacterZToStageBoundsAll();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.ClampCharacterZToStageBoundsAll();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            BattleEcsCharacterStageZPassDiagnostics diagnostics =
                world.BattleEcsCharacterStageZPassDiagnosticsForDiagnostics;
            Assert.That(allocated, Is.Zero);
            Assert.That(diagnostics.RunCount, Is.EqualTo(2));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(entityCount * 2));
            Assert.That(diagnostics.DerivedTypeFallbackCount, Is.Zero);
        }

        private static SimulationWorld CreateContractWorld(
            BattleEcsCharacterStageZPassMode mode,
            out LF2Character[] entities)
        {
            SimulationWorld world = CreateWorld();
            entities = new[]
            {
                RegisterCharacter(world, 50, 500.0),
                RegisterCharacter(world, 51, 100.0),
                RegisterCharacter(world, 52, 225.75),
                RegisterCharacter(world, 53, 500.0),
                RegisterCharacter(world, 54, 500.0),
            };
            entities[3].Runtime.PendingFlushDestroy = true;
            entities[4].Runtime.OidMergeDormant = true;
            world.ConfigureBattleEcsCharacterStageZPassForDiagnostics(mode);
            return world;
        }

        private static SimulationWorld CreateWorld(
            int zMin = 180,
            int zMax = 350)
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(
                800,
                zMin,
                zMax,
                0,
                0);
            return world;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            double z)
        {
            var character = new LF2Character();
            character.PS.z = z;
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(slot));
            character.Runtime.Z = z;
            character.Runtime.ZInt = int.MinValue + slot;
            return character;
        }
    }
}
#endif
