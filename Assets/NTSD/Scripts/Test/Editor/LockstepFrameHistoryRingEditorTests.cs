#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class LockstepFrameHistoryRingEditorTests
    {
        [Test]
        public void WrapsOldFramesAndPreservesChronologicalLookup()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepFrameHistoryRing(identity, 3);

            for (int tick = 1; tick <= 5; tick++)
            {
                Assert.That(
                    history.TryRecordConsumed(Canonical(tick), out var reason),
                    Is.True,
                    reason.ToString());
            }

            Assert.That(history.Count, Is.EqualTo(3));
            Assert.That(history.EarliestTick, Is.EqualTo(3));
            Assert.That(history.LatestTick, Is.EqualTo(5));
            Assert.That(history.TryGet(2, out _), Is.False);
            for (int tick = 3; tick <= 5; tick++)
            {
                Assert.That(history.TryGet(tick, out LockstepFrameHistoryEntry entry), Is.True);
                Assert.That(entry.TickIndex, Is.EqualTo(tick));
                Assert.That(entry.SchemaVersion, Is.EqualTo(identity.SchemaVersion));
                Assert.That(entry.IdentityFingerprint, Is.EqualTo(identity.IdentityFingerprint));
                Assert.That(entry.InputHash, Is.EqualTo(entry.Frame.GetCanonicalHash64()));
            }
        }

        [Test]
        public void OwnsInputStorageAndRejectsNonSequentialFrames()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepFrameHistoryRing(identity, 2);
            var source = new[]
            {
                new SimulationPlayerInput(2, SimulationInputButtons.Left),
                new SimulationPlayerInput(5, SimulationInputButtons.Attack),
            };

            Assert.That(
                history.TryRecordConsumed(new FrameInputSet(1, source), out var reason),
                Is.True);
            source[0] = new SimulationPlayerInput(2, SimulationInputButtons.Jump);

            Assert.That(history.TryGet(1, out LockstepFrameHistoryEntry entry), Is.True);
            Assert.That(entry.Frame.Players[0].Buttons, Is.EqualTo(SimulationInputButtons.Left));
            Assert.That(history.TryRecordConsumed(Canonical(3), out reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.WrongFrameTick));
            Assert.That(history.TryRecordConsumed(Canonical(1), out reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.FrameAlreadyJournaled));
        }

        [Test]
        public void WarmRecordAndLookupDoNotAllocate()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepFrameHistoryRing(identity, 64);
            var players = new SimulationPlayerInput[identity.PlayerCount];
            for (int playerIndex = 0; playerIndex < players.Length; playerIndex++)
            {
                players[playerIndex] = new SimulationPlayerInput(
                    identity.CanonicalPlayerSlots[playerIndex],
                    SimulationInputButtons.None);
            }
            FrameInputSet frame = FrameInputSetPreallocation.CreateReusable();

            frame.ResetPreallocated(1, players);
            Assert.That(history.TryRecordConsumed(frame, out _), Is.True);
            Assert.That(history.TryGet(1, out _), Is.True);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int tick = 2; tick <= 1025; tick++)
            {
                frame.ResetPreallocated(tick, players);
                if (!history.TryRecordConsumed(frame, out _))
                    Assert.Fail($"Failed to record tick {tick}.");
                if (!history.TryGet(tick, out _))
                    Assert.Fail($"Failed to resolve tick {tick}.");
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void ResetStartsAnewContiguousWindow()
        {
            LockstepSessionIdentity identity =
                StrictDelayedInputBufferEditorTests.CreateIdentity();
            var history = new LockstepFrameHistoryRing(identity, 2);
            Assert.That(history.TryRecordConsumed(Canonical(1), out _), Is.True);

            history.Reset(10);

            Assert.That(history.Count, Is.Zero);
            Assert.That(history.TryGet(1, out _), Is.False);
            Assert.That(history.TryRecordConsumed(Canonical(11), out _), Is.True);
            Assert.That(history.EarliestTick, Is.EqualTo(11));
            Assert.That(history.LatestTick, Is.EqualTo(11));
        }

        private static FrameInputSet Canonical(int tick)
        {
            return new FrameInputSet(tick, new[]
            {
                new SimulationPlayerInput(2, SimulationInputButtons.None),
                new SimulationPlayerInput(5, SimulationInputButtons.None),
            });
        }
    }
}
#endif
