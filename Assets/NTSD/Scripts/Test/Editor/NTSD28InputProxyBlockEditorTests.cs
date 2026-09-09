#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class NTSD28InputProxyBlockEditorTests
    {
        [Test]
        public void SerializedLayout_MatchesExactEntityBeThroughDeSpan()
        {
            NTSD28InputProxyBlock block = CreateDistinctBlock();
            var actual = new byte[NTSD28InputProxyBlock.SerializedByteCount];

            block.WriteSerialized(actual);

            byte[] expected =
            {
                0x10, 0x11, 0x12, 0x13,
                0x14, 0x15, 0x16, 0x17,
                0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26,
                0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36,
                0x40, 0x41, 0x42, 0x43, 0x44,
                0x45, 0x46, 0x47, 0x48, 0x49,
                0x5A,
            };
            Assert.That(NTSD28InputProxyBlock.SerializedByteCount, Is.EqualTo(0x21));
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void CopyFrom_CopiesOnlyOwnedStateWithoutAliasingSource()
        {
            NTSD28InputProxyBlock source = CreateDistinctBlock();
            var destination = new NTSD28InputProxyBlock();

            destination.CopyFrom(source);
            source.EdgeWindow[0] = 0xEE;
            source.Previous[0] = 0xED;
            source.Current[0] = 0xEC;
            source.ComboState[0] = 0xEB;
            source.DefendReentryCooldown = 0xEA;
            source.ProxyTail = 0xE9;

            var actual = new byte[NTSD28InputProxyBlock.SerializedByteCount];
            destination.WriteSerialized(actual);
            Assert.That(actual[0], Is.EqualTo(0x10));
            Assert.That(actual[3], Is.EqualTo(0x13));
            Assert.That(actual[8], Is.EqualTo(0x20));
            Assert.That(actual[15], Is.EqualTo(0x30));
            Assert.That(actual[22], Is.EqualTo(0x40));
            Assert.That(actual[32], Is.EqualTo(0x5A));
        }

        [Test]
        public void PublicStateSurface_ExcludesPendingHistoryAndRunAccumulator()
        {
            string[] properties = typeof(NTSD28InputProxyBlock)
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Select(property => property.Name)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();

            Assert.That(properties, Is.EqualTo(new[]
            {
                nameof(NTSD28InputProxyBlock.ComboState),
                nameof(NTSD28InputProxyBlock.Current),
                nameof(NTSD28InputProxyBlock.DefendReentryCooldown),
                nameof(NTSD28InputProxyBlock.EdgeWindow),
                nameof(NTSD28InputProxyBlock.Previous),
                nameof(NTSD28InputProxyBlock.ProxyTail),
            }));
        }

        [Test]
        public void WarmCopyAndSerialization_AllocateZeroManagedBytes()
        {
            NTSD28InputProxyBlock source = CreateDistinctBlock();
            var destination = new NTSD28InputProxyBlock();
            var bytes = new byte[NTSD28InputProxyBlock.SerializedByteCount];
            destination.CopyFrom(source);
            destination.WriteSerialized(bytes);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 4096; index++)
            {
                destination.CopyFrom(source);
                destination.WriteSerialized(bytes);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void InvalidCopyAndSerializationTargets_FailClosed()
        {
            var block = new NTSD28InputProxyBlock();

            Assert.Throws<ArgumentNullException>(() => block.CopyFrom(null));
            Assert.Throws<ArgumentNullException>(() => block.WriteSerialized(null));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                block.WriteSerialized(new byte[0x21], -1));
            Assert.Throws<ArgumentException>(() =>
                block.WriteSerialized(new byte[0x20]));
        }

        private static NTSD28InputProxyBlock CreateDistinctBlock()
        {
            var block = new NTSD28InputProxyBlock();
            block.EdgeWindow[0] = 0x10;
            block.EdgeWindow[1] = 0x11;
            block.EdgeWindow[2] = 0x12;
            block.DefendReentryCooldown = 0x13;
            block.EdgeWindow[3] = 0x14;
            block.EdgeWindow[4] = 0x15;
            block.EdgeWindow[5] = 0x16;
            block.EdgeWindow[6] = 0x17;
            for (int index = 0; index < 7; index++)
            {
                block.Previous[index] = (byte)(0x20 + index);
                block.Current[index] = (byte)(0x30 + index);
            }
            for (int index = 0; index < 10; index++)
                block.ComboState[index] = (byte)(0x40 + index);
            block.ProxyTail = 0x5A;
            return block;
        }
    }
}
#endif
