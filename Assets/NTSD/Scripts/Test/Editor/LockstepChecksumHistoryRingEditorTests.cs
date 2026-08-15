#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class LockstepChecksumHistoryRingEditorTests
    {
        [Test]
        public void WrapsOldEntriesAndPreservesSchemaIdentityAndHashes()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepChecksumHistoryRing(identity, 3);

            for (int tick = 1; tick <= 5; tick++)
            {
                Assert.That(
                    history.TryRecordConsumed(
                        tick,
                        (ulong)(tick * 10),
                        BattleLockstepChecksumModule.CurrentSchemaVersion,
                        (ulong)(tick * 100),
                        out var reason),
                    Is.True,
                    reason.ToString());
            }

            Assert.That(history.Count, Is.EqualTo(3));
            Assert.That(history.EarliestTick, Is.EqualTo(3));
            Assert.That(history.LatestTick, Is.EqualTo(5));
            Assert.That(history.TryGet(2, out _), Is.False);
            Assert.That(history.TryGet(4, out LockstepChecksumHistoryEntry entry), Is.True);
            Assert.That(entry.ProtocolSchemaVersion, Is.EqualTo(identity.SchemaVersion));
            Assert.That(entry.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
            Assert.That(entry.ChecksumSchemaVersion,
                Is.EqualTo(BattleLockstepChecksumModule.CurrentSchemaVersion));
            Assert.That(entry.InputHash, Is.EqualTo(40UL));
            Assert.That(entry.StateChecksum, Is.EqualTo(400UL));
            Assert.That(entry.HasStateChecksum, Is.True);
        }

        [Test]
        public void DisabledChecksumStillRetainsAlignedTickWithoutChecksumValue()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepChecksumHistoryRing(identity, 2);

            Assert.That(
                history.TryRecordConsumed(1, 91UL, 0, 123UL, out _),
                Is.True);
            Assert.That(history.TryGet(1, out LockstepChecksumHistoryEntry entry), Is.True);
            Assert.That(entry.TickIndex, Is.EqualTo(1));
            Assert.That(entry.InputHash, Is.EqualTo(91UL));
            Assert.That(entry.HasStateChecksum, Is.False);
            Assert.That(entry.StateChecksum, Is.Zero);
        }

        [Test]
        public void RejectsWrongTickDuplicateAndInvalidSchema()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepChecksumHistoryRing(identity, 2);

            Assert.That(
                history.TryRecordConsumed(2, 0UL, 0, 0UL, out var reason),
                Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.WrongFrameTick));
            Assert.That(
                history.TryRecordConsumed(1, 0UL, -1, 0UL, out reason),
                Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.InvalidConfiguration));
            Assert.That(history.TryRecordConsumed(1, 0UL, 0, 0UL, out reason), Is.True);
            Assert.That(history.TryRecordConsumed(1, 0UL, 0, 0UL, out reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.FrameAlreadyJournaled));
        }

        [Test]
        public void WarmRecordAndLookupDoNotAllocate()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepChecksumHistoryRing(identity, 64);
            Assert.That(history.TryRecordConsumed(1, 1UL, 3, 2UL, out _), Is.True);
            Assert.That(history.TryGet(1, out _), Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 2; tick <= 1025; tick++)
            {
                if (!history.TryRecordConsumed(tick, (ulong)tick, 3, (ulong)(tick * 2), out _))
                    Assert.Fail($"Failed to record tick {tick}.");
                if (!history.TryGet(tick, out _))
                    Assert.Fail($"Failed to resolve tick {tick}.");
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }
    }
}
#endif
