#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28Q05UnclaimedRawRestoreEditorTests
    {
        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void SuccessfulRestoreIncludesExistingUnclaimedScalarAndArrayPayload(BattleRuntimeProfile profile)
        {
            var world = CreateWorld(profile);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            var raw = world.RuntimeSlotTableForModules.GetRawRuntime(world.RuntimeSlotCapacity - 1);
            raw.X = 12.5;
            raw.HP = 123;
            raw.SpawnerSlotIndex = 21;
            raw.InputHistory[5] = 33;
            entity.Runtime.X = 19;
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            string expectedChecksum = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
            raw.X = 777;
            raw.HP = 1;
            raw.SpawnerSlotIndex = 2;
            raw.InputHistory[5] = 3;
            entity.Runtime.X = 4;
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out BattleStateSnapshotRestoreFailure failure), Is.True, failure.ToString());
            raw = world.RuntimeSlotTableForModules.GetRawRuntime(world.RuntimeSlotCapacity - 1);
            Assert.That(raw.X, Is.EqualTo(12.5));
            Assert.That(raw.HP, Is.EqualTo(123));
            Assert.That(raw.SpawnerSlotIndex, Is.EqualTo(21));
            Assert.That(raw.InputHistory[5], Is.EqualTo(33));
            Assert.That(entity.Runtime.X, Is.EqualTo(19));
            Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.EqualTo(expectedChecksum));
        }

        [TestCase(BattleRuntimeProfile.Authority400)]
        [TestCase(BattleRuntimeProfile.MobileExtended)]
        public void InvalidRawSnapshotStorageRejectsBeforeChangingWorld(BattleRuntimeProfile profile)
        {
            var world = CreateWorld(profile);
            var entity = new LF2Character();
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            int rawSlot = world.RuntimeSlotCapacity - 1;
            world.RuntimeSlotTableForModules.GetRawRuntime(rawSlot).X = 12.5;
            var identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = world.CreateBattleStateSnapshotBufferForBootstrap();
            Assert.That(world.TryCaptureBattleStateSnapshot(identity, 0, snapshot), Is.True);
            var captured = (NTSDEntityRuntime[])typeof(BattleWorldEntityRuntimeSnapshotBuffer)
                .GetField("rawRuntimes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(snapshot.EntityRuntime);
            captured[rawSlot].InputHistory = null;
            entity.Runtime.X = 87;
            string before = world.CaptureLockstepChecksumSnapshot(0).OverallChecksum;
            Assert.That(world.TryRestoreBattleStateSnapshot(identity, snapshot, out BattleStateSnapshotRestoreFailure failure), Is.False);
            Assert.That(failure, Is.EqualTo(BattleStateSnapshotRestoreFailure.EntityPayloadMismatch));
            Assert.That(entity.Runtime.X, Is.EqualTo(87));
            Assert.That(world.CaptureLockstepChecksumSnapshot(0).OverallChecksum, Is.EqualTo(before));
        }

        private static SimulationWorld CreateWorld(BattleRuntimeProfile profile)
        {
            return profile == BattleRuntimeProfile.Authority400 ? new SimulationWorld()
                : new SimulationWorld(profile, BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity);
        }
    }
}
#endif
