using System;
using NUnit.Framework;

namespace NTSD.Simulation.Tests
{
    public sealed class NTSD28NativeRandomEditorTests
    {
        [Test]
        public void Msvcr80_SeedOne_MatchesAuthorityVector()
        {
            var random = new NTSD28Msvcr80Random(1u);

            Assert.That(random.Next(), Is.EqualTo(41u));
            Assert.That(random.Next(), Is.EqualTo(18467u));
            Assert.That(random.Next(), Is.EqualTo(6334u));
            Assert.That(random.Calls, Is.EqualTo(3UL));
        }

        [Test]
        public void ResetFromSeed_BuildsAuthoritySynchronizedTable()
        {
            var random = new NTSD28NativeRandom();

            random.ResetFromSeed(1u);
            NTSD28NativeRandomState state = random.CaptureState();

            Assert.That(state.CrtCalls, Is.EqualTo(3000UL));
            Assert.That(state.Synchronized.Table, Has.Length.EqualTo(3001));
            Assert.That(state.Synchronized.Table[0], Is.EqualTo(42));
            Assert.That(state.Synchronized.Table[1], Is.EqualTo(108));
            Assert.That(state.Synchronized.Table[3000], Is.Zero);
            for (int index = 0; index < 3000; index++)
            {
                Assert.That(state.Synchronized.Table[index], Is.InRange(1, 255));
            }
        }

        [Test]
        public void SynchronizedNext_AdvancesBeforeRead_AndNonpositiveDoesNotConsume()
        {
            var random = new NTSD28NativeRandom();
            var synchronized = new NTSD28SynchronizedRandomState
            {
                Index = 2999,
                Counter = 1233
            };
            synchronized.Table[0] = 5;
            synchronized.Table[1] = 9;
            random.RestoreSynchronized(synchronized);

            Assert.That(random.SynchronizedNext(0x82u, 7), Is.EqualTo(5));
            NTSD28SynchronizedRandomState after = random.CaptureSynchronizedState();
            Assert.That(after.Index, Is.Zero);
            Assert.That(after.Counter, Is.Zero);
            Assert.That(after.Calls, Is.EqualTo(1UL));
            Assert.That(after.LastCallSite, Is.EqualTo(0x82u));

            Assert.That(random.SynchronizedNext(0x85u, 0), Is.Zero);
            after = random.CaptureSynchronizedState();
            Assert.That(after.Index, Is.Zero);
            Assert.That(after.Counter, Is.Zero);
            Assert.That(after.Calls, Is.EqualTo(1UL));
            Assert.That(after.LastCallSite, Is.EqualTo(0x82u));
        }

        [Test]
        public void Restore_NormalizesCounters_AndOwnsDeepCopiedTable()
        {
            var source = new NTSD28SynchronizedRandomState
            {
                Index = -1,
                Counter = -1,
                Calls = 7,
                LastCallSite = 0x1234u
            };
            source.Table[0] = 11;
            source.Table[2999] = 13;
            var random = new NTSD28NativeRandom();

            random.Restore(new NTSD28NativeRandomState(9u, 4UL, source));
            source.Table[0] = 99;
            NTSD28NativeRandomState captured = random.CaptureState();

            Assert.That(captured.CrtState, Is.EqualTo(9u));
            Assert.That(captured.CrtCalls, Is.EqualTo(4UL));
            Assert.That(captured.Synchronized.Index, Is.EqualTo(2999));
            Assert.That(captured.Synchronized.Counter, Is.EqualTo(1233));
            Assert.That(captured.Synchronized.Table[0], Is.EqualTo(11));

            captured.Synchronized.Table[0] = 77;
            Assert.That(random.CaptureState().Synchronized.Table[0], Is.EqualTo(11));
        }

        [Test]
        public void SameSeedAndIndex_ProduceSameHashAndLongSequence()
        {
            var first = new NTSD28NativeRandom();
            var second = new NTSD28NativeRandom();
            first.ResetFromSeed(0x12345678u, 2981);
            second.ResetFromSeed(0x12345678u, 2981);

            Assert.That(first.SynchronizedTableHash(),
                Is.EqualTo(second.SynchronizedTableHash()));
            for (int index = 0; index < 5000; index++)
            {
                Assert.That(first.SynchronizedNext(0x90u + (uint)index, 97),
                    Is.EqualTo(second.SynchronizedNext(0x90u + (uint)index, 97)));
            }
        }

        [Test]
        public void B0AuthorityCaptureSeed_MatchesCrtStateAndTableHash()
        {
            var random = new NTSD28NativeRandom();

            random.ResetFromSeed(682973786u);
            NTSD28NativeRandomState state = random.CaptureState();

            Assert.That(state.CrtState, Is.EqualTo(1758127634u));
            Assert.That(state.CrtCalls, Is.EqualTo(3000UL));
            Assert.That(random.SynchronizedTableHash(),
                Is.EqualTo(0xA1BA1B90EA55796DUL));
        }

        [Test]
        public void SynchronizedCursor_MatchesOwnerLongSequenceWithoutMutatingSource()
        {
            var source = new NTSD28NativeRandom(0x13579BDFu);
            var oracle = new NTSD28NativeRandom(0x13579BDFu);
            NTSD28NativeRandomScalarState before = source.CaptureScalarState();
            NTSD28SynchronizedRandomCursor cursor =
                source.CaptureSynchronizedCursor();

            for (int index = 0; index < 5000; index++)
            {
                uint callSite = 0x20u + (uint)(index % 40);
                int upperBound = (index % 97) + 1;
                Assert.That(cursor.Next(callSite, upperBound),
                    Is.EqualTo(oracle.SynchronizedNext(callSite, upperBound)));
            }

            NTSD28NativeRandomScalarState unchanged = source.CaptureScalarState();
            Assert.That(unchanged.SynchronizedCounter,
                Is.EqualTo(before.SynchronizedCounter));
            Assert.That(unchanged.SynchronizedIndex,
                Is.EqualTo(before.SynchronizedIndex));
            Assert.That(unchanged.SynchronizedCalls,
                Is.EqualTo(before.SynchronizedCalls));
            Assert.That(cursor.Calls, Is.EqualTo(5000UL));
            Assert.That(cursor.LastCallSite, Is.EqualTo(0x47u));
        }

        [Test]
        public void SynchronizedCursor_StructCopiesAdvanceIndependentlyAndCommitExplicitly()
        {
            var random = new NTSD28NativeRandom(0x2468ACE0u);
            NTSD28SynchronizedRandomCursor first =
                random.CaptureSynchronizedCursor();
            NTSD28SynchronizedRandomCursor second = first;

            int firstValue = first.Next(0x31u, 19);

            Assert.That(second.Next(0x31u, 19), Is.EqualTo(firstValue));
            Assert.That(random.CaptureScalarState().SynchronizedCalls, Is.Zero);
            Assert.That(random.TryCommitSynchronizedCursor(first), Is.True);
            NTSD28NativeRandomScalarState committed = random.CaptureScalarState();
            Assert.That(committed.SynchronizedCounter, Is.EqualTo(first.Counter));
            Assert.That(committed.SynchronizedIndex, Is.EqualTo(first.Index));
            Assert.That(committed.SynchronizedCalls, Is.EqualTo(first.Calls));
            Assert.That(committed.LastSynchronizedCallSite,
                Is.EqualTo(first.LastCallSite));
        }

        [Test]
        public void SynchronizedCursor_NonpositiveBoundDoesNotAdvance()
        {
            var random = new NTSD28NativeRandom(0x10203040u);
            NTSD28SynchronizedRandomCursor cursor =
                random.CaptureSynchronizedCursor();
            int counter = cursor.Counter;
            int index = cursor.Index;
            ulong calls = cursor.Calls;
            uint lastCallSite = cursor.LastCallSite;

            Assert.That(cursor.Next(0x44u, 0), Is.Zero);
            Assert.That(cursor.Next(0x45u, -7), Is.Zero);

            Assert.That(cursor.Counter, Is.EqualTo(counter));
            Assert.That(cursor.Index, Is.EqualTo(index));
            Assert.That(cursor.Calls, Is.EqualTo(calls));
            Assert.That(cursor.LastCallSite, Is.EqualTo(lastCallSite));
        }

        [Test]
        public void SynchronizedCursor_ResetAndRestoreRejectStaleCommit()
        {
            var random = new NTSD28NativeRandom(7u);
            NTSD28SynchronizedRandomCursor resetStale =
                random.CaptureSynchronizedCursor();
            resetStale.Next(0x51u, 11);

            random.ResetFromSeed(8u);

            Assert.That(random.TryCommitSynchronizedCursor(resetStale), Is.False);
            NTSD28SynchronizedRandomCursor restoreStale =
                random.CaptureSynchronizedCursor();
            restoreStale.Next(0x52u, 13);
            NTSD28SynchronizedRandomState restored =
                random.CaptureSynchronizedState();
            random.RestoreSynchronized(restored);

            Assert.That(random.TryCommitSynchronizedCursor(restoreStale), Is.False);
        }

        [Test]
        public void SynchronizedCursor_LaterCommitRejectsSameGenerationStaleOrigin()
        {
            var random = new NTSD28NativeRandom(0x31415926u);
            NTSD28SynchronizedRandomCursor first =
                random.CaptureSynchronizedCursor();
            NTSD28SynchronizedRandomCursor stale =
                random.CaptureSynchronizedCursor();
            first.Next(0x71u, 17);
            stale.Next(0x72u, 19);

            Assert.That(random.TryCommitSynchronizedCursor(first), Is.True);
            NTSD28NativeRandomScalarState committed = random.CaptureScalarState();
            Assert.That(random.TryCommitSynchronizedCursor(stale), Is.False);
            NTSD28NativeRandomScalarState after = random.CaptureScalarState();

            Assert.That(after.SynchronizedCounter,
                Is.EqualTo(committed.SynchronizedCounter));
            Assert.That(after.SynchronizedIndex,
                Is.EqualTo(committed.SynchronizedIndex));
            Assert.That(after.SynchronizedCalls,
                Is.EqualTo(committed.SynchronizedCalls));
            Assert.That(after.LastSynchronizedCallSite,
                Is.EqualTo(committed.LastSynchronizedCallSite));
        }

        [Test]
        public void WarmCursorCaptureNextCommit_AllocatesZeroManagedBytes()
        {
            var random = new NTSD28NativeRandom(0x55667788u);
            NTSD28SynchronizedRandomCursor warm =
                random.CaptureSynchronizedCursor();
            warm.Next(0x61u, 31);
            Assert.That(random.TryCommitSynchronizedCursor(warm), Is.True);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            bool committed = true;
            for (int index = 0; index < 4096; index++)
            {
                NTSD28SynchronizedRandomCursor cursor =
                    random.CaptureSynchronizedCursor();
                cursor.Next(0x62u, 37);
                committed &= random.TryCommitSynchronizedCursor(cursor);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(committed, Is.True);
            Assert.That(allocated, Is.Zero);
        }
    }
}
