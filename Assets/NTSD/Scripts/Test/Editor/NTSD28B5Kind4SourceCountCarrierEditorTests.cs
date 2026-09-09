#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Kind4SourceCountCarrierEditorTests
    {
        [Test]
        public void Lifecycle_DefaultZero_InputResetPreserves_FullResetClears()
        {
            var runtime = new NTSDEntityRuntime();
            Assert.That(runtime.Kind4SourceCount92, Is.Zero);

            runtime.Kind4SourceCount92 = 0xFFFF;
            runtime.ResetInputState();
            Assert.That(runtime.Kind4SourceCount92, Is.EqualTo(0xFFFF));

            runtime.Reset();
            Assert.That(runtime.Kind4SourceCount92, Is.Zero);
        }

        [Test]
        public void CanonicalCopy_PreservesCountWithoutAliasingOtherFields()
        {
            var source = new NTSDEntityRuntime
            {
                Kind4SourceCount92 = 0xABCD,
                CatchSourceSlot90 = 17,
                EnvironmentState320 = 23,
                WeaponCount = 29,
            };
            var destination = new NTSDEntityRuntime();

            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            Assert.That(destination.Kind4SourceCount92, Is.EqualTo(0xABCD));
            Assert.That(destination.CatchSourceSlot90, Is.EqualTo(17));
            Assert.That(destination.EnvironmentState320, Is.EqualTo(23));
            Assert.That(destination.WeaponCount, Is.EqualTo(29));
        }

        [Test]
        public void EntityRuntimeSnapshot_RoundTripsCount()
        {
            SimulationWorld world = World(out LF2Character entity);
            entity.Runtime.Kind4SourceCount92 = 0x9876;
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.MaxRuntimeSlotsForServices);

            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules,
                identity,
                19), Is.True);
            entity.Runtime.Kind4SourceCount92 = 1;
            Assert.That(snapshot.TryCopyEntityRuntime(3, entity.Runtime), Is.True);

            Assert.That(entity.Runtime.Kind4SourceCount92, Is.EqualTo(0x9876));
            Assert.That(snapshot.SchemaVersion, Is.EqualTo(12));
        }

        [Test]
        public void ChecksumAndParity_TrackCount()
        {
            SimulationWorld world = World(out LF2Character entity);
            ulong checksumBefore = world.CaptureRuntimeChecksum64(0, null);
            BattleParityFrameSnapshot parityBefore = world.CaptureParityFrameSnapshot(
                0,
                FrameInputSet.Empty(0));

            entity.Runtime.Kind4SourceCount92 = 7;
            ulong checksumAfter = world.CaptureRuntimeChecksum64(0, null);
            BattleParityFrameSnapshot parityAfter = world.CaptureParityFrameSnapshot(
                0,
                FrameInputSet.Empty(0));

            Assert.That(checksumAfter, Is.Not.EqualTo(checksumBefore));
            Assert.That(parityAfter.Hashes.Overall, Is.Not.EqualTo(parityBefore.Hashes.Overall));
        }

        [Test]
        public void WarmCopyAndChecksum_AllocateZero_AndSchemasAdvance()
        {
            var source = new NTSDEntityRuntime { Kind4SourceCount92 = 0xFFFF };
            var destination = new NTSDEntityRuntime();
            SimulationWorld world = World(out LF2Character entity);
            entity.Runtime.Kind4SourceCount92 = 0xFFFF;
            source.TryCopyCanonicalStateTo(destination);
            world.CaptureRuntimeChecksum64(0, null);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            ulong checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                source.TryCopyCanonicalStateTo(destination);
                checksum ^= world.CaptureRuntimeChecksum64(index, null);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(destination.Kind4SourceCount92, Is.EqualTo(0xFFFF));
            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
            Assert.That(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion, Is.EqualTo(12));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion, Is.EqualTo(20));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion, Is.EqualTo(23));
        }

        private static SimulationWorld World(out LF2Character entity)
        {
            var world = new SimulationWorld();
            entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            return world;
        }
    }
}
#endif
