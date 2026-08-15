#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleEcsFramePostProcessPassEditorTests
    {
        [Test]
        public void DefaultMode_RemainsLegacyAfterPerformanceEvaluation()
        {
            var world = new SimulationWorld();

            Assert.That(
                world.BattleEcsFramePostProcessPassModeForDiagnostics,
                Is.EqualTo(BattleEcsFramePostProcessPassMode.Legacy));

            world.AdvanceBattleFlowTick(1);
            Assert.Throws<InvalidOperationException>(() =>
                world.ConfigureBattleEcsFramePostProcessPassForDiagnostics(
                    BattleEcsFramePostProcessPassMode.DataOriented));
        }

        [Test]
        public void LegacyWriter_PreservesAuthorityNegativeHitCountContract()
        {
            var world = new SimulationWorld();
            LF2Character entity = RegisterCharacter(world, 50);
            ConfigureRuntime(
                entity.Runtime,
                frameDelay: 0,
                hitCount: -2,
                vx: 7.0,
                vy: 8.0,
                vz: 9.0,
                knockbackVx: 11.0,
                knockbackVy: 12.0,
                knockbackVz: 13.0);

            world.RunBattleEcsFramePostProcessPass();

            Assert.That(entity.Runtime.HitCount, Is.EqualTo(-2));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(7.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(8.0));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(9.0));
            Assert.That(entity.Runtime.KnockbackVx, Is.Zero);
            Assert.That(entity.Runtime.KnockbackVy, Is.Zero);
            Assert.That(entity.Runtime.KnockbackVz, Is.Zero);
            Assert.That(
                world.LastFramePostProcessRuntimeSnapshotSkipCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void ForcedLegacyRuntimeSnapshot_DisablesExactCharacterPostFrameSkip()
        {
            var world = new SimulationWorld
            {
                ForceLegacyPostFrameRuntimeSnapshotForDiagnostics = true,
            };
            LF2Character entity = RegisterCharacter(world, 50);
            ConfigureRuntime(entity.Runtime, 0, 2, 0, 0, 0, 9, -6, 3);

            world.FramePostProcessAll();

            Assert.That(
                world.LastFramePostProcessRuntimeSnapshotSkipCountForDiagnostics,
                Is.Zero);
            Assert.That(entity.Runtime.Vx, Is.EqualTo(6.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-4.0));
            Assert.That(entity.Runtime.Vz, Is.EqualTo(2.0));
        }

        [Test]
        public void EntityPostFrameTail_ExactCharacterSkipsOnlyRedundantWideSnapshot()
        {
            var world = new SimulationWorld();
            LF2Character entity = RegisterCharacter(world, 50);
            entity.Health.HP = 400;
            entity.Health.HPBound = 500;
            entity.HealTimer = 1001;
            entity.CatchTimer = 0;

            world.EntityPostFrameTailAll(1);

            Assert.That(entity.HealTimer, Is.Zero);
            Assert.That(entity.Runtime.HealTimer, Is.Zero);
            Assert.That(entity.Runtime.TransientMp, Is.Zero);
            Assert.That(entity.Runtime.TransientMp2, Is.EqualTo(1000));
            Assert.That(entity.Runtime.TransientMp3, Is.EqualTo(1000));
            Assert.That(entity.Runtime.TransientMp4, Is.EqualTo(1000));
            Assert.That(
                world.LastEntityPostFrameTailRuntimeSnapshotSkipCountForDiagnostics,
                Is.EqualTo(1));
        }

        [Test]
        public void ShadowCompare_ExactlyMatchesLegacyWriter()
        {
            SimulationWorld world = CreateContractWorld(
                BattleEcsFramePostProcessPassMode.ShadowCompare,
                out LF2Character[] entities);

            world.RunBattleEcsFramePostProcessPass();

            BattleEcsFramePostProcessPassDiagnostics diagnostics =
                world.BattleEcsFramePostProcessPassDiagnosticsForDiagnostics;
            Assert.That(entities[0].Runtime.Vx, Is.EqualTo(8.0));
            Assert.That(entities[0].Runtime.Vy, Is.EqualTo(-4.0));
            Assert.That(entities[0].Runtime.Vz, Is.EqualTo(2.0));
            Assert.That(entities[0].Runtime.HitCount, Is.Zero);
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(3));
            Assert.That(diagnostics.ValidationCount, Is.EqualTo(1));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.IsClean, Is.True);
        }

        [Test]
        public void DataOrientedWriter_MatchesLegacyAndSkipsInactiveOrDelayedSlots()
        {
            SimulationWorld legacy = CreateContractWorld(
                BattleEcsFramePostProcessPassMode.Legacy,
                out LF2Character[] legacyEntities);
            SimulationWorld dataOriented = CreateContractWorld(
                BattleEcsFramePostProcessPassMode.DataOriented,
                out LF2Character[] dataEntities);

            legacy.RunBattleEcsFramePostProcessPass();
            dataOriented.RunBattleEcsFramePostProcessPass();

            for (int i = 0; i < legacyEntities.Length; i++)
                AssertRuntimeEquals(legacyEntities[i].Runtime, dataEntities[i].Runtime, i);

            Assert.That(dataEntities[1].Runtime.HitCount, Is.Zero);
            Assert.That(dataEntities[1].Runtime.Vx, Is.EqualTo(3.0));
            Assert.That(dataEntities[2].Runtime.HitCount, Is.EqualTo(-2));
            Assert.That(dataEntities[3].Runtime.HitCount, Is.EqualTo(2),
                "FrameDelay != 0 must skip the writer");
            Assert.That(dataEntities[4].Runtime.HitCount, Is.EqualTo(2),
                "pending-destroy membership must be skipped");
            Assert.That(dataEntities[5].Runtime.HitCount, Is.EqualTo(2),
                "dormant membership must be skipped");

            BattleEcsFramePostProcessPassDiagnostics diagnostics =
                dataOriented.BattleEcsFramePostProcessPassDiagnosticsForDiagnostics;
            Assert.That(diagnostics.RunCount, Is.EqualTo(1));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(3));
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

            for (int slot = firstSlot; slot < capacity; slot++)
            {
                LF2Character entity = RegisterCharacter(world, slot);
                ConfigureRuntime(
                    entity.Runtime,
                    frameDelay: 0,
                    hitCount: 2,
                    vx: 0.0,
                    vy: 0.0,
                    vz: 0.0,
                    knockbackVx: slot,
                    knockbackVy: -slot,
                    knockbackVz: slot * 0.5);
            }

            world.ConfigureBattleEcsFramePostProcessPassForDiagnostics(
                BattleEcsFramePostProcessPassMode.DataOriented);
            world.RunBattleEcsFramePostProcessPass();

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            world.RunBattleEcsFramePostProcessPass();
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            BattleEcsFramePostProcessPassDiagnostics diagnostics =
                world.BattleEcsFramePostProcessPassDiagnosticsForDiagnostics;
            Assert.That(allocated, Is.Zero);
            Assert.That(diagnostics.RunCount, Is.EqualTo(2));
            Assert.That(diagnostics.SlotVisitCount, Is.EqualTo(entityCount * 2));
        }

        private static SimulationWorld CreateContractWorld(
            BattleEcsFramePostProcessPassMode mode,
            out LF2Character[] entities)
        {
            var world = new SimulationWorld();
            entities = new LF2Character[6];
            for (int i = 0; i < entities.Length; i++)
                entities[i] = RegisterCharacter(world, 50 + i);

            ConfigureRuntime(entities[0].Runtime, 0, 3, 1, 2, 3, 16, -8, 4);
            ConfigureRuntime(entities[1].Runtime, 0, 0, 3, 4, 5, 9, 8, 7);
            ConfigureRuntime(entities[2].Runtime, 0, -2, 6, 7, 8, 5, 4, 3);
            ConfigureRuntime(entities[3].Runtime, 2, 2, 9, 10, 11, 4, 5, 6);
            ConfigureRuntime(entities[4].Runtime, 0, 2, 12, 13, 14, 7, 8, 9);
            ConfigureRuntime(entities[5].Runtime, 0, 2, 15, 16, 17, 10, 11, 12);
            entities[4].Runtime.PendingFlushDestroy = true;
            entities[5].Runtime.OidMergeDormant = true;
            world.ConfigureBattleEcsFramePostProcessPassForDiagnostics(mode);
            return world;
        }

        private static LF2Character RegisterCharacter(
            SimulationWorld world,
            int slot)
        {
            var character = new LF2Character();
            character.SetRequiredRuntimeSlot(slot);
            world.Register(character);
            Assert.That(character.Runtime.SlotIndex, Is.EqualTo(slot));
            return character;
        }

        private static void ConfigureRuntime(
            NTSDEntityRuntime runtime,
            int frameDelay,
            int hitCount,
            double vx,
            double vy,
            double vz,
            double knockbackVx,
            double knockbackVy,
            double knockbackVz)
        {
            runtime.FrameDelay = frameDelay;
            runtime.HitCount = hitCount;
            runtime.Vx = vx;
            runtime.Vy = vy;
            runtime.Vz = vz;
            runtime.KnockbackVx = knockbackVx;
            runtime.KnockbackVy = knockbackVy;
            runtime.KnockbackVz = knockbackVz;
        }

        private static void AssertRuntimeEquals(
            NTSDEntityRuntime expected,
            NTSDEntityRuntime actual,
            int index)
        {
            Assert.That(BitConverter.DoubleToInt64Bits(actual.Vx),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.Vx)),
                $"Vx mismatch at entity {index}");
            Assert.That(BitConverter.DoubleToInt64Bits(actual.Vy),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.Vy)),
                $"Vy mismatch at entity {index}");
            Assert.That(BitConverter.DoubleToInt64Bits(actual.Vz),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.Vz)),
                $"Vz mismatch at entity {index}");
            Assert.That(actual.HitCount, Is.EqualTo(expected.HitCount),
                $"HitCount mismatch at entity {index}");
            Assert.That(BitConverter.DoubleToInt64Bits(actual.KnockbackVx),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.KnockbackVx)),
                $"KnockbackVx mismatch at entity {index}");
            Assert.That(BitConverter.DoubleToInt64Bits(actual.KnockbackVy),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.KnockbackVy)),
                $"KnockbackVy mismatch at entity {index}");
            Assert.That(BitConverter.DoubleToInt64Bits(actual.KnockbackVz),
                Is.EqualTo(BitConverter.DoubleToInt64Bits(expected.KnockbackVz)),
                $"KnockbackVz mismatch at entity {index}");
        }
    }
}
#endif
