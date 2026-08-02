#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NTSD.Simulation.Lockstep;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattleLockstepReplayJournalEditorTests
    {
        [Test]
        public void RecordsCanonicalFramesOnceWithoutRetainingCallerArrays()
        {
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var journal = new LockstepReplayJournal(identity, 4);
            var inputs = new[]
            {
                new SimulationPlayerInput(2, SimulationInputButtons.Left),
                new SimulationPlayerInput(5, SimulationInputButtons.Attack),
            };
            var frame = new FrameInputSet(1, inputs);

            Assert.That(journal.TryRecordConsumed(frame, out var reason), Is.True);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.None));
            inputs[0] = new SimulationPlayerInput(2, SimulationInputButtons.Jump);
            Assert.That(journal[0].Players[0].Buttons, Is.EqualTo(SimulationInputButtons.Left));
            Assert.That(journal.TryRecordConsumed(frame, out reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.FrameAlreadyJournaled));
        }

        [Test]
        public void RejectsNonCanonicalFramesAndCapacityOverflow()
        {
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var journal = new LockstepReplayJournal(identity, 1);
            var reversed = new FrameInputSet(1, new[]
            {
                new SimulationPlayerInput(5, SimulationInputButtons.None),
                new SimulationPlayerInput(2, SimulationInputButtons.None),
            });

            Assert.That(journal.TryRecordConsumed(reversed, out var reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.NonCanonicalPlayerOrder));
            Assert.That(journal.TryRecordConsumed(Canonical(1), out reason), Is.True);
            Assert.That(journal.TryRecordConsumed(Canonical(2), out reason), Is.False);
            Assert.That(reason, Is.EqualTo(LockstepProtocolReason.JournalCapacityExceeded));
        }

        [Test]
        public void ResetClearsExportCursor()
        {
            LockstepSessionIdentity identity = StrictDelayedInputBufferEditorTests.CreateIdentity();
            var journal = new LockstepReplayJournal(identity, 2);
            Assert.That(journal.TryRecordConsumed(Canonical(1), out _), Is.True);

            journal.Reset(7);

            Assert.That(journal.Count, Is.Zero);
            Assert.That(journal.LastRecordedTick, Is.EqualTo(7));
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
