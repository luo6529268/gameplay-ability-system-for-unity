#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28")]
    [Category("NTSD28_Q08")]
    public sealed class NTSD28Q08NativeKnockoutEventStateEditorTests
    {
        [Test]
        public void NewestTailExpiryRetainsOlderRecordsUntilTheNewestExpires()
        {
            var state = new SimulationBattleBufferModule(400);
            state.RecordNativeKnockout(Event(1, 0));
            state.RecordNativeKnockout(Event(65, 1));

            Assert.That(state.PruneNativeKnockoutTail(72, 70), Is.Zero);
            Assert.That(state.NativeKnockoutEvents.Count, Is.EqualTo(2));
            Assert.That(state.PruneNativeKnockoutTail(136, 70), Is.EqualTo(2));
            Assert.That(state.NativeKnockoutEvents, Is.Empty);
        }

        [Test]
        public void PendingSnapshotOwnsOrderedKnockoutsAndRestoresThem()
        {
            var source = new SimulationBattleBufferModule(400);
            source.RecordNativeKnockout(Event(12, 3));
            source.RecordNativeKnockout(Event(13, 4));
            var snapshot = new BattleWorldPendingEventSnapshotBuffer(1);
            LockstepSessionIdentity identity =
                NTSD.Test.StrictDelayedInputBufferEditorTests.CreateIdentity();

            Assert.That(snapshot.TryCapture(source, identity, 13), Is.True);
            Assert.That(snapshot.KnockoutCount, Is.EqualTo(2));
            Assert.That(snapshot.GetKnockout(0).BattleTimeTick, Is.EqualTo(12));
            Assert.That(snapshot.GetKnockout(1).SourceObjectType, Is.EqualTo(4));

            source.NativeKnockoutEvents.Clear();
            var destination = new SimulationBattleBufferModule(400);
            Assert.That(snapshot.TryRestoreTo(destination), Is.True);
            Assert.That(destination.NativeKnockoutEvents.Count, Is.EqualTo(2));
            Assert.That(destination.NativeKnockoutEvents[0].SourceSlot, Is.EqualTo(7));
            Assert.That(destination.NativeKnockoutEvents[1].CreditSlot, Is.EqualTo(9));
        }

        [Test]
        public void LockstepChecksumAndResetTrackKnockoutEventState()
        {
            var world = new SimulationWorld();
            var checksum = new BattleLockstepChecksumModule();
            world.ResetRuntimeState();
            ulong before = checksum.Capture(world, 0, null);
            world.Runtime.NativeKnockoutFeed.RestoreForSnapshot(true, 70);
            ulong withFeed = checksum.Capture(world, 0, null);
            Assert.That(withFeed, Is.Not.EqualTo(before));
            world.BattleBuffersForServices.RecordNativeKnockout(Event(1, 2));
            ulong withEvent = checksum.Capture(world, 0, null);
            Assert.That(withEvent, Is.Not.EqualTo(withFeed));

            world.ResetRuntimeState();
            Assert.That(world.NativeKnockoutEvents, Is.Empty);
            Assert.That(world.Runtime.NativeKnockoutFeed.RecordPresent, Is.False);
            Assert.That(checksum.Capture(world, 0, null), Is.EqualTo(before));
        }

        private static NativeKnockoutEvent Event(int tick, int type)
        {
            return new NativeKnockoutEvent(tick, type, 8, 3, 7, 9);
        }
    }
}
#endif
