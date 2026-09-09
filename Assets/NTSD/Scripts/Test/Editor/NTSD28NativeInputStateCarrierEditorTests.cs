#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28NativeInputStateCarrierEditorTests
    {
        [Test]
        public void RuntimeDefaultsAndReset_OwnExactBlockAndControlFields()
        {
            var runtime = new NTSDEntityRuntime();

            Assert.That(runtime.NativeInputProxy, Is.Not.Null);
            Assert.That(runtime.InputProxyCounter14C, Is.Zero);
            Assert.That(runtime.InputProxySourceSlot178, Is.EqualTo(-1));
            Assert.That(runtime.InputProxyEnabled17C, Is.Zero);

            FillDistinct(runtime.NativeInputProxy);
            runtime.InputProxyCounter14C = 7;
            runtime.InputProxySourceSlot178 = 23;
            runtime.InputProxyEnabled17C = 1;
            runtime.ResetInputState();

            AssertBlockIsZero(runtime.NativeInputProxy);
            Assert.That(runtime.InputProxyCounter14C, Is.EqualTo(7));
            Assert.That(runtime.InputProxySourceSlot178, Is.EqualTo(23));
            Assert.That(runtime.InputProxyEnabled17C, Is.EqualTo(1));

            FillDistinct(runtime.NativeInputProxy);
            runtime.Reset();
            AssertBlockIsZero(runtime.NativeInputProxy);
            Assert.That(runtime.InputProxyCounter14C, Is.Zero);
            Assert.That(runtime.InputProxySourceSlot178, Is.EqualTo(-1));
            Assert.That(runtime.InputProxyEnabled17C, Is.Zero);
        }

        [Test]
        public void CanonicalCopy_PreservesCarrierWithoutAliasing()
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            FillDistinct(source.NativeInputProxy);
            source.InputProxyCounter14C = 9;
            source.InputProxySourceSlot178 = 31;
            source.InputProxyEnabled17C = 1;

            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);
            byte[] expected = Serialize(destination.NativeInputProxy);

            source.NativeInputProxy.EdgeWindow[0] = 0xEE;
            source.NativeInputProxy.ComboState[9] = 0xEF;
            source.NativeInputProxy.ProxyTail = 0xF0;
            source.InputProxyCounter14C = 0;
            source.InputProxySourceSlot178 = -1;
            source.InputProxyEnabled17C = 0;

            Assert.That(Serialize(destination.NativeInputProxy), Is.EqualTo(expected));
            Assert.That(destination.InputProxyCounter14C, Is.EqualTo(9));
            Assert.That(destination.InputProxySourceSlot178, Is.EqualTo(31));
            Assert.That(destination.InputProxyEnabled17C, Is.EqualTo(1));
        }

        [Test]
        public void EntityRuntimeSnapshot_CapturesAndRestoresCarrier()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            FillDistinct(entity.Runtime.NativeInputProxy);
            entity.Runtime.InputProxyCounter14C = 5;
            entity.Runtime.InputProxySourceSlot178 = 11;
            entity.Runtime.InputProxyEnabled17C = 1;
            byte[] expected = Serialize(entity.Runtime.NativeInputProxy);
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var snapshot = new BattleWorldEntityRuntimeSnapshotBuffer(
                world.MaxRuntimeSlotsForServices);

            Assert.That(snapshot.TryCapture(
                world.RuntimeSlotTableForModules,
                identity,
                0), Is.True);
            entity.Runtime.Reset();
            Assert.That(snapshot.TryCopyEntityRuntime(3, entity.Runtime), Is.True);

            Assert.That(Serialize(entity.Runtime.NativeInputProxy), Is.EqualTo(expected));
            Assert.That(entity.Runtime.InputProxyCounter14C, Is.EqualTo(5));
            Assert.That(entity.Runtime.InputProxySourceSlot178, Is.EqualTo(11));
            Assert.That(entity.Runtime.InputProxyEnabled17C, Is.EqualTo(1));
        }

        [Test]
        public void RuntimeChecksum_TracksBlockAndEveryControlField()
        {
            var world = new SimulationWorld();
            var entity = new LF2Character { ObjectId = 7 };
            entity.SetRequiredRuntimeSlot(3);
            world.Register(entity);
            ulong baseline = world.CaptureRuntimeChecksum64(0, null);

            entity.Runtime.InputProxyCounter14C = 1;
            ulong counter = world.CaptureRuntimeChecksum64(0, null);
            entity.Runtime.InputProxySourceSlot178 = 2;
            ulong source = world.CaptureRuntimeChecksum64(0, null);
            entity.Runtime.InputProxyEnabled17C = 1;
            ulong enabled = world.CaptureRuntimeChecksum64(0, null);
            entity.Runtime.NativeInputProxy.ComboState[9] = 3;
            ulong block = world.CaptureRuntimeChecksum64(0, null);

            Assert.That(counter, Is.Not.EqualTo(baseline));
            Assert.That(source, Is.Not.EqualTo(counter));
            Assert.That(enabled, Is.Not.EqualTo(source));
            Assert.That(block, Is.Not.EqualTo(enabled));
        }

        [Test]
        public void WarmCanonicalCopy_AllocatesZeroManagedBytes()
        {
            var source = new NTSDEntityRuntime();
            var destination = new NTSDEntityRuntime();
            FillDistinct(source.NativeInputProxy);
            Assert.That(source.TryCopyCanonicalStateTo(destination), Is.True);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool copied = true;
            for (int index = 0; index < 4096; index++)
                copied &= source.TryCopyCanonicalStateTo(destination);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(copied, Is.True);
            Assert.That(allocated, Is.Zero);
        }

        private static void FillDistinct(NTSD28InputProxyBlock block)
        {
            for (int index = 0; index < block.EdgeWindow.Length; index++)
                block.EdgeWindow[index] = (byte)(0x10 + index);
            block.DefendReentryCooldown = 0x21;
            for (int index = 0; index < block.Previous.Length; index++)
            {
                block.Previous[index] = (byte)(0x30 + index);
                block.Current[index] = (byte)(0x40 + index);
            }
            for (int index = 0; index < block.ComboState.Length; index++)
                block.ComboState[index] = (byte)(0x50 + index);
            block.ProxyTail = 0x6A;
        }

        private static byte[] Serialize(NTSD28InputProxyBlock block)
        {
            var bytes = new byte[NTSD28InputProxyBlock.SerializedByteCount];
            block.WriteSerialized(bytes);
            return bytes;
        }

        private static void AssertBlockIsZero(NTSD28InputProxyBlock block)
        {
            Assert.That(Serialize(block),
                Is.EqualTo(new byte[NTSD28InputProxyBlock.SerializedByteCount]));
        }
    }
}
#endif
