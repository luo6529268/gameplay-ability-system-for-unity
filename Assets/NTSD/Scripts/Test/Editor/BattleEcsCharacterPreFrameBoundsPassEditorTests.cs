#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsCharacterPreFrameBoundsPassEditorTests
    {
        [Test]
        public void DefaultMode_IsDataOrientedAndCannotChangeAfterTickBoundary()
        {
            var world = CreateWorld();

            Assert.That(
                world.BattleEcsCharacterPreFrameBoundsPassModeForDiagnostics,
                Is.EqualTo(BattleEcsCharacterPreFrameBoundsPassMode.DataOriented));

            world.AdvanceBattleFlowTick(1);
            Assert.Throws<InvalidOperationException>(() =>
                world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(
                    BattleEcsCharacterPreFrameBoundsPassMode.Legacy));
        }

        [Test]
        public void DataOrientedExactCharacters_MatchLegacyBoundaryMatrix()
        {
            SimulationWorld legacy = CreateContractWorld(
                BattleEcsCharacterPreFrameBoundsPassMode.Legacy,
                out LF2Character[] legacyCharacters);
            SimulationWorld data = CreateContractWorld(
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented,
                out LF2Character[] dataCharacters);

            legacy.ApplyPreFrameBoundsAll();
            data.ApplyPreFrameBoundsAll();

            for (int index = 0; index < legacyCharacters.Length; index++)
            {
                NTSDEntityRuntime expected = legacyCharacters[index].Runtime;
                NTSDEntityRuntime actual = dataCharacters[index].Runtime;
                Assert.That(
                    BitConverter.DoubleToInt64Bits(actual.X),
                    Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.X)),
                    $"X mismatch at contract character {index}");
                Assert.That(actual.XInt, Is.EqualTo(expected.XInt));
                Assert.That(
                    BitConverter.DoubleToInt64Bits(actual.Z),
                    Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.Z)),
                    $"Z mismatch at contract character {index}");
                Assert.That(actual.ZInt, Is.EqualTo(expected.ZInt));
            }

            Assert.That(dataCharacters[0].Runtime.X, Is.EqualTo(0.0));
            Assert.That(dataCharacters[1].Runtime.X, Is.EqualTo(-300.0));
            Assert.That(dataCharacters[2].Runtime.X, Is.EqualTo(700.0));
            Assert.That(dataCharacters[3].Runtime.X, Is.EqualTo(750.0));
            Assert.That(dataCharacters[4].Runtime.X, Is.EqualTo(-100.0));
            Assert.That(dataCharacters[5].Runtime.X, Is.EqualTo(900.0));
            Assert.That(dataCharacters[6].Runtime.XInt, Is.EqualTo(123));

            BattleEcsCharacterPreFrameBoundsPassDiagnostics diagnostics =
                data.BattleEcsCharacterPreFrameBoundsPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(7));
            Assert.That(diagnostics.ExactCharacterWriteCount, Is.EqualTo(7));
            Assert.That(diagnostics.CompatibilityFallbackCount, Is.Zero);
        }

        [Test]
        public void DerivedCharacter_RetainsVirtualCompatibilityFallback()
        {
            SimulationWorld legacy = CreateWorld();
            SimulationWorld data = CreateWorld();
            var expected = new DerivedPreFrameCharacter();
            var actual = new DerivedPreFrameCharacter();
            ConfigureCharacter(expected, 25, -250.0, 500.0, 0, 0);
            ConfigureCharacter(actual, 25, -250.0, 500.0, 0, 0);
            legacy.Register(expected);
            data.Register(actual);
            legacy.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(
                BattleEcsCharacterPreFrameBoundsPassMode.Legacy);
            data.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented);

            legacy.ApplyPreFrameBoundsAll();
            data.ApplyPreFrameBoundsAll();

            Assert.That(actual.Runtime.X, Is.EqualTo(expected.Runtime.X));
            Assert.That(actual.Runtime.XInt, Is.EqualTo(expected.Runtime.XInt));
            Assert.That(actual.Runtime.Z, Is.EqualTo(expected.Runtime.Z));
            Assert.That(actual.Runtime.ZInt, Is.EqualTo(expected.Runtime.ZInt));
            Assert.That(actual.PreFrameZCallCount, Is.EqualTo(1));
            Assert.That(actual.PreFrameXCallCount, Is.EqualTo(1));
            Assert.That(
                data.BattleEcsCharacterPreFrameBoundsPassDiagnosticsForDiagnostics
                    .CompatibilityFallbackCount,
                Is.EqualTo(1));
        }

        [Test]
        public void Extended1000_WarmedDataOrientedPassDoesNotAllocate()
        {
            const int capacity = 1050;
            var world = new SimulationWorld(
                BattleRuntimeProfile.MobileExtended,
                capacity);
            world.SetExplicitStageRuntimeSnapshotForTesting(800, 180, 350, 0, 0);
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented);
            for (int slot = 50; slot < capacity; slot++)
            {
                LF2Character character = RegisterCharacter(
                    world,
                    slot,
                    100.25 + slot,
                    100.25 + slot,
                    0,
                    0);
                Assert.That(character.Runtime.SlotIndex, Is.EqualTo(slot));
            }

            world.ApplyPreFrameBoundsAll();
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.ApplyPreFrameBoundsAll();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            BattleEcsCharacterPreFrameBoundsPassDiagnostics diagnostics =
                world.BattleEcsCharacterPreFrameBoundsPassDiagnosticsForDiagnostics;
            Assert.That(allocated, Is.Zero);
            Assert.That(diagnostics.RunCount, Is.EqualTo(2));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(2000));
            Assert.That(diagnostics.ExactCharacterWriteCount, Is.EqualTo(2000));
            Assert.That(diagnostics.CompatibilityFallbackCount, Is.Zero);
        }

        private static SimulationWorld CreateContractWorld(
            BattleEcsCharacterPreFrameBoundsPassMode mode,
            out LF2Character[] characters)
        {
            SimulationWorld world = CreateWorld();
            world.Runtime.Stage.ApplyPhaseBound(700);
            world.ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(mode);
            characters = new[]
            {
                RegisterCharacter(world, 5, -50.0, 500.0, 0, 0),
                RegisterCharacter(world, 6, -500.0, 100.0, 5, 0),
                RegisterCharacter(world, 7, 750.0, 225.75, 0, 0),
                RegisterCharacter(world, 8, 750.0, 225.75, 0, 10),
                RegisterCharacter(world, 20, -200.0, 225.75, 0, 0),
                RegisterCharacter(world, 21, 1000.0, 225.75, 0, 0),
                RegisterCharacter(world, 22, 123.9, 225.75, 0, 0),
            };
            return world;
        }

        private static SimulationWorld CreateWorld()
        {
            var world = new SimulationWorld();
            world.SetExplicitStageRuntimeSnapshotForTesting(800, 180, 350, 0, 0);
            return world;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot,
            double x,
            double z,
            int relationTeam,
            int hitStop)
        {
            var character = new LF2Character();
            ConfigureCharacter(character, slot, x, z, relationTeam, hitStop);
            world.Register(character);
            return character;
        }

        private static void ConfigureCharacter(
            LF2Character character,
            int slot,
            double x,
            double z,
            int relationTeam,
            int hitStop)
        {
            character.SetRequiredRuntimeSlot(slot);
            character.Runtime.X = x;
            character.Runtime.Z = z;
            character.Runtime.XInt = int.MinValue + slot;
            character.Runtime.ZInt = int.MinValue + slot;
            character.Runtime.RelationTeam = relationTeam;
            character.Runtime.HitStop = hitStop;
        }

        private sealed class DerivedPreFrameCharacter : LF2Character
        {
            internal int PreFrameZCallCount { get; private set; }
            internal int PreFrameXCallCount { get; private set; }

            internal override void ApplyPreFrameZBounds(float zMin, float zMax)
            {
                PreFrameZCallCount++;
                base.ApplyPreFrameZBounds(zMin, zMax);
            }

            internal override bool ApplyPreFrameXBounds(
                float baseStageWidth,
                int xMaxOverride)
            {
                PreFrameXCallCount++;
                return base.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride);
            }
        }
    }
}
#endif
