#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28B4StatusMotionCarrierEditorTests
    {
        [Test]
        public void RuntimeLifecycle_UsesAuthorityDefaultsAndPreservesInputReset()
        {
            var runtime = new NTSDEntityRuntime();
            AssertDefaults(runtime);

            Mutate(runtime);
            runtime.ResetInputState();
            AssertMutated(runtime);

            runtime.Reset();
            AssertDefaults(runtime);
        }

        [Test]
        public void CanonicalCopy_PreservesAllStatusMotionCarriers()
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            Mutate(source);

            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            AssertMutated(destination);
        }

        [Test]
        public void EntityRuntimeSnapshot_RoundTripsAllStatusMotionCarriers()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            Mutate(entity.Runtime);
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.MaxRuntimeSlotsForServices);

            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules,
                identity,
                19), Is.True);
            entity.Runtime.Reset();
            Assert.That(snapshot.TryCopyEntityRuntime(3, entity.Runtime), Is.True);

            AssertMutated(entity.Runtime);
            Assert.That(snapshot.SchemaVersion, Is.EqualTo(12));
        }

        [Test]
        public void EveryCarrier_ChangesChecksum_AndParityExposesAllValues()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(0);
            world.Register(entity);
            NTSDEntityRuntime runtime = entity.Runtime;
            ulong baseline = world.CaptureRuntimeChecksum64(1, null);
            var cases = new (string Name, Action Set, Action Reset)[]
            {
                (nameof(runtime.StatusDx1C0), () => runtime.StatusDx1C0 = -11, () => runtime.StatusDx1C0 = 0),
                (nameof(runtime.StatusDy1C4), () => runtime.StatusDy1C4 = 12, () => runtime.StatusDy1C4 = 0),
                (nameof(runtime.StatusDz1C8), () => runtime.StatusDz1C8 = -13, () => runtime.StatusDz1C8 = 0),
                (nameof(runtime.StatusGain1CC), () => runtime.StatusGain1CC = 1, () => runtime.StatusGain1CC = 0),
                (nameof(runtime.StatusHitFacing1D0), () => runtime.StatusHitFacing1D0 = 1, () => runtime.StatusHitFacing1D0 = 0),
                (nameof(runtime.StatusPickedAction1D4), () => runtime.StatusPickedAction1D4 = 300, () => runtime.StatusPickedAction1D4 = 191),
                (nameof(runtime.StatusPickingAction1D8), () => runtime.StatusPickingAction1D8 = 301, () => runtime.StatusPickingAction1D8 = 185),
            };

            for (int index = 0; index < cases.Length; index++)
            {
                cases[index].Set();
                Assert.That(world.CaptureRuntimeChecksum64(1, null),
                    Is.Not.EqualTo(baseline), cases[index].Name);
                cases[index].Reset();
                Assert.That(world.CaptureRuntimeChecksum64(1, null),
                    Is.EqualTo(baseline), cases[index].Name + " reset");
            }

            Mutate(runtime);
            ulong changed = world.CaptureRuntimeChecksum64(1, null);
            string parity = world.CaptureParityFrameSnapshot(1).ToJson(full: true);

            Assert.That(changed, Is.Not.EqualTo(baseline));
            Assert.That(parity, Does.Contain("\"statusDx1C0\":-11"));
            Assert.That(parity, Does.Contain("\"statusDy1C4\":12"));
            Assert.That(parity, Does.Contain("\"statusDz1C8\":-13"));
            Assert.That(parity, Does.Contain("\"statusGain1CC\":1"));
            Assert.That(parity, Does.Contain("\"statusHitFacing1D0\":1"));
            Assert.That(parity, Does.Contain("\"statusPickedAction1D4\":300"));
            Assert.That(parity, Does.Contain("\"statusPickingAction1D8\":301"));
        }

        [Test]
        public void SnapshotAndChecksumSchemas_AdvanceForStatusMotionCarriers()
        {
            Assert.That(BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(12));
            Assert.That(BattleStateSnapshotBuffer.CurrentSchemaVersion,
                Is.EqualTo(20));
            Assert.That(BattleLockstepChecksumModule.CurrentSchemaVersion,
                Is.EqualTo(23));
        }

        [Test]
        public void WarmCopyAndChecksum_DoNotAllocate()
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            Mutate(source);
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(0);
            world.Register(entity);
            Mutate(entity.Runtime);
            _ = source.TryCopyCanonicalStateTo(destination);
            _ = world.CaptureRuntimeChecksum64(0, null);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            ulong checksum = 0;
            for (int index = 0; index < 4096; index++)
            {
                source.TryCopyCanonicalStateTo(destination);
                checksum ^= world.CaptureRuntimeChecksum64(index, null);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            AssertMutated(destination);
            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static void Mutate(NTSDEntityRuntime runtime)
        {
            runtime.StatusDx1C0 = -11;
            runtime.StatusDy1C4 = 12;
            runtime.StatusDz1C8 = -13;
            runtime.StatusGain1CC = 1;
            runtime.StatusHitFacing1D0 = 1;
            runtime.StatusPickedAction1D4 = 300;
            runtime.StatusPickingAction1D8 = 301;
        }

        private static void AssertDefaults(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.StatusDx1C0, Is.Zero);
            Assert.That(runtime.StatusDy1C4, Is.Zero);
            Assert.That(runtime.StatusDz1C8, Is.Zero);
            Assert.That(runtime.StatusGain1CC, Is.Zero);
            Assert.That(runtime.StatusHitFacing1D0, Is.Zero);
            Assert.That(runtime.StatusPickedAction1D4, Is.EqualTo(191));
            Assert.That(runtime.StatusPickingAction1D8, Is.EqualTo(185));
        }

        private static void AssertMutated(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.StatusDx1C0, Is.EqualTo(-11));
            Assert.That(runtime.StatusDy1C4, Is.EqualTo(12));
            Assert.That(runtime.StatusDz1C8, Is.EqualTo(-13));
            Assert.That(runtime.StatusGain1CC, Is.EqualTo(1));
            Assert.That(runtime.StatusHitFacing1D0, Is.EqualTo(1));
            Assert.That(runtime.StatusPickedAction1D4, Is.EqualTo(300));
            Assert.That(runtime.StatusPickingAction1D8, Is.EqualTo(301));
        }
    }
}
#endif
