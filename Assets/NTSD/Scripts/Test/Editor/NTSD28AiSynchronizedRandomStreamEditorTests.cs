using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28AiSynchronizedRandomStreamEditorTests
    {
        [Test]
        public void LongSequence_MatchesOwnerAndPreservesExplicitCallSites()
        {
            var source = new NTSD28NativeRandom(0x13579BDFu);
            var oracle = new NTSD28NativeRandom(0x13579BDFu);
            var stream = new AiDecisionRandomStream(
                source.CaptureSynchronizedCursor());

            for (int index = 0; index < 5000; index++)
            {
                uint callSite = 0x11u + (uint)(index % 40);
                int upperBound = (index % 97) + 1;
                Assert.That(stream.Rand(callSite, upperBound),
                    Is.EqualTo(oracle.SynchronizedNext(callSite, upperBound)));
            }

            Assert.That(stream.UsesSynchronizedCursor, Is.True);
            Assert.That(stream.Calls, Is.EqualTo(5000UL));
            Assert.That(stream.LastCallSite, Is.EqualTo(0x38u));
            Assert.That(source.CaptureScalarState().SynchronizedCalls, Is.Zero);
        }

        [Test]
        public void Trace_CapturesSiteBoundRawAndValueInOrder()
        {
            var source = new NTSD28NativeRandom(0x2468ACE0u);
            NTSD28SynchronizedRandomState before =
                source.CaptureSynchronizedState();
            var sites = new uint[2];
            var moduli = new int[2];
            var rawValues = new int[2];
            var values = new int[2];
            var stream = new AiDecisionRandomStream(
                source.CaptureSynchronizedCursor(),
                captureTrace: true,
                callSites: sites,
                moduli: moduli,
                rawValues: rawValues,
                values: values);

            int first = stream.Rand(0x1Fu, 17);
            int second = stream.Rand(0x20u, 20);

            int firstCounter = (before.Counter + 1) % 1234;
            int firstIndex = (before.Index + 1) % 3000;
            int firstRaw = before.Table[firstIndex] + firstCounter;
            int secondCounter = (firstCounter + 1) % 1234;
            int secondIndex = (firstIndex + 1) % 3000;
            int secondRaw = before.Table[secondIndex] + secondCounter;
            Assert.That(sites, Is.EqualTo(new[] { 0x1Fu, 0x20u }));
            Assert.That(moduli, Is.EqualTo(new[] { 17, 20 }));
            Assert.That(rawValues, Is.EqualTo(new[] { firstRaw, secondRaw }));
            Assert.That(values, Is.EqualTo(new[] { first, second }));
            Assert.That(first, Is.EqualTo(firstRaw % 17));
            Assert.That(second, Is.EqualTo(secondRaw % 20));
            Assert.That(stream.DrawCount, Is.EqualTo(2));
            Assert.That(stream.TraceOverflow, Is.False);
        }

        [Test]
        public void NonpositiveAndUnscopedCalls_FailClosedWithoutAdvancing()
        {
            var source = new NTSD28NativeRandom(7u);
            var stream = new AiDecisionRandomStream(
                source.CaptureSynchronizedCursor());
            int counter = stream.SynchronizedCounter;
            int index = stream.SynchronizedIndex;
            ulong calls = stream.Calls;
            uint lastCallSite = stream.LastCallSite;

            Assert.That(stream.Rand(0x15u, 0), Is.Zero);
            Assert.That(stream.Rand(0x15u, -3), Is.Zero);
            Assert.Throws<InvalidOperationException>(() => stream.Rand(7));

            Assert.That(stream.SynchronizedCounter, Is.EqualTo(counter));
            Assert.That(stream.SynchronizedIndex, Is.EqualTo(index));
            Assert.That(stream.Calls, Is.EqualTo(calls));
            Assert.That(stream.LastCallSite, Is.EqualTo(lastCallSite));
            Assert.That(stream.DrawCount, Is.Zero);
        }

        [Test]
        public void StructCopiesAdvanceIndependentlyAndCommitExplicitly()
        {
            var owner = new NTSD28NativeRandom(11u);
            var first = new AiDecisionRandomStream(
                owner.CaptureSynchronizedCursor());
            AiDecisionRandomStream second = first;

            int firstValue = first.Rand(0x28u, 7);

            Assert.That(second.Rand(0x28u, 7), Is.EqualTo(firstValue));
            Assert.That(owner.CaptureScalarState().SynchronizedCalls, Is.Zero);
            Assert.That(owner.TryCommitSynchronizedCursor(
                first.CaptureSynchronizedCursor()), Is.True);
            NTSD28NativeRandomScalarState committed = owner.CaptureScalarState();
            Assert.That(committed.SynchronizedCounter,
                Is.EqualTo(first.SynchronizedCounter));
            Assert.That(committed.SynchronizedIndex,
                Is.EqualTo(first.SynchronizedIndex));
            Assert.That(committed.SynchronizedCalls, Is.EqualTo(first.Calls));
            Assert.That(committed.LastSynchronizedCallSite,
                Is.EqualTo(first.LastCallSite));
        }

        [Test]
        public void OwnerResetRejectsStaleStreamCursor()
        {
            var owner = new NTSD28NativeRandom(13u);
            var stream = new AiDecisionRandomStream(
                owner.CaptureSynchronizedCursor());
            stream.Rand(0x37u, 5);

            owner.ResetFromSeed(17u);

            Assert.That(owner.TryCommitSynchronizedCursor(
                stream.CaptureSynchronizedCursor()), Is.False);
            Assert.That(owner.CaptureScalarState().SynchronizedCalls, Is.Zero);
        }

        [Test]
        public void LegacyMode_PreservesCrtAndRejectsScopedApi()
        {
            var stream = new AiDecisionRandomStream(1u, 4UL);

            Assert.That(stream.Rand(10), Is.EqualTo(1));
            Assert.That(stream.State, Is.EqualTo(2745024u));
            Assert.That(stream.Calls, Is.EqualTo(5UL));
            Assert.That(stream.UsesSynchronizedCursor, Is.False);
            Assert.Throws<InvalidOperationException>(
                () => stream.Rand(0x11u, 10));
        }

        [Test]
        public void WarmCaptureDrawCommit_AllocatesZeroManagedBytes()
        {
            var owner = new NTSD28NativeRandom(0x55667788u);
            var warm = new AiDecisionRandomStream(
                owner.CaptureSynchronizedCursor());
            warm.Rand(0x38u, 37);
            Assert.That(owner.TryCommitSynchronizedCursor(
                warm.CaptureSynchronizedCursor()), Is.True);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool committed = true;
            for (int index = 0; index < 4096; index++)
            {
                var stream = new AiDecisionRandomStream(
                    owner.CaptureSynchronizedCursor());
                stream.Rand(0x38u, 37);
                committed &= owner.TryCommitSynchronizedCursor(
                    stream.CaptureSynchronizedCursor());
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(committed, Is.True);
            Assert.That(allocated, Is.Zero);
        }
    }
}
