#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28EnvironmentStateCarrierEditorTests
    {
        [Test]
        public void RuntimeLifecycle_DefaultsToZero_InputResetPreserves_FullResetClears()
        {
            var runtime = new NTSDEntityRuntime();

            Assert.That(runtime.EnvironmentState320, Is.Zero);

            runtime.EnvironmentState320 = -17;
            runtime.ResetInputState();

            Assert.That(runtime.EnvironmentState320, Is.EqualTo(-17));

            runtime.Reset();

            Assert.That(runtime.EnvironmentState320, Is.Zero);
        }

        [Test]
        public void CanonicalCopy_PreservesSignedEnvironmentState()
        {
            var source = new NTSDEntityRuntime
            {
                EnvironmentState320 = -29,
            };
            var destination = new NTSDEntityRuntime
            {
                EnvironmentState320 = 41,
            };

            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            Assert.That(destination.EnvironmentState320, Is.EqualTo(-29));
        }

        [Test]
        public void EntityRuntimeSnapshot_RoundTripsSignedEnvironmentState()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            entity.Runtime.EnvironmentState320 = -33;
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.MaxRuntimeSlotsForServices);

            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules,
                identity,
                17), Is.True);
            entity.Runtime.EnvironmentState320 = 64;
            Assert.That(snapshot.TryCopyEntityRuntime(3, entity.Runtime), Is.True);

            Assert.That(entity.Runtime.EnvironmentState320, Is.EqualTo(-33));
            Assert.That(snapshot.SchemaVersion, Is.EqualTo(13));
        }

        [Test]
        public void RuntimeChecksum_TracksEnvironmentState_WithoutWarmAllocations()
        {
            var source = new NTSDEntityRuntime
            {
                EnvironmentState320 = -51,
            };
            var destination = new NTSDEntityRuntime();
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            ulong beforeStateChange = world.CaptureRuntimeChecksum64(0, null);

            entity.Runtime.EnvironmentState320 = -51;
            ulong afterStateChange = world.CaptureRuntimeChecksum64(0, null);

            Assert.That(afterStateChange, Is.Not.EqualTo(beforeStateChange));
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            _ = world.CaptureRuntimeChecksum64(0, null);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long beforeAllocation = GC.GetAllocatedBytesForCurrentThread();
            bool copied = true;
            ulong checksum = 0UL;
            for (int index = 0; index < 4096; index++)
            {
                copied &= source.TryCopyCanonicalStateTo(destination);
                checksum ^= world.CaptureRuntimeChecksum64(index, null);
            }
            long allocated =
                GC.GetAllocatedBytesForCurrentThread() - beforeAllocation;

            Assert.That(copied, Is.True);
            Assert.That(destination.EnvironmentState320, Is.EqualTo(-51));
            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void SnapshotAndChecksumSchemas_AreEnvironmentStateCarrierVersions()
        {
            Assert.That(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(13));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(21));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion,
                Is.EqualTo(24));
        }
    }
}
#endif
