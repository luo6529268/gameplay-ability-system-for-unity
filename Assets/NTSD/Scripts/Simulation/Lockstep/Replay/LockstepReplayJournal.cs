using System;
using System.Collections;
using System.Collections.Generic;

namespace NTSD.Simulation.Lockstep
{
    public sealed class LockstepReplayJournal : IReadOnlyList<FrameInputSet>
    {
        private readonly LockstepSessionIdentity identity;
        private readonly SimulationPlayerInput[][] inputStorage;
        private readonly FrameInputSet[] frames;
        private int count;
        private int lastRecordedTick;

        public LockstepReplayJournal(LockstepSessionIdentity identity, int capacity)
        {
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            inputStorage = new SimulationPlayerInput[capacity][];
            frames = new FrameInputSet[capacity];
            for (int i = 0; i < capacity; i++)
            {
                inputStorage[i] = new SimulationPlayerInput[identity.PlayerCount];
                frames[i] = FrameInputSetPreallocation.CreateReusable();
            }
        }

        public int Count => count;
        public int Capacity => frames.Length;
        public int LastRecordedTick => lastRecordedTick;
        public bool HasCapacity => count < frames.Length;

        public FrameInputSet this[int index]
        {
            get
            {
                if ((uint)index >= (uint)count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return frames[index];
            }
        }

        public bool TryRecordConsumed(
            FrameInputSet frame,
            out LockstepProtocolReason reason)
        {
            if (frame == null)
            {
                reason = LockstepProtocolReason.NonCanonicalPlayerOrder;
                return false;
            }
            if (frame.TickIndex <= lastRecordedTick)
            {
                reason = LockstepProtocolReason.FrameAlreadyJournaled;
                return false;
            }
            if (frame.TickIndex != lastRecordedTick + 1)
            {
                reason = LockstepProtocolReason.WrongFrameTick;
                return false;
            }
            if (!frame.IsCanonicalFor(frame.TickIndex, identity.CanonicalPlayerSlots))
            {
                reason = LockstepProtocolReason.NonCanonicalPlayerOrder;
                return false;
            }
            if (count >= frames.Length)
            {
                reason = LockstepProtocolReason.JournalCapacityExceeded;
                return false;
            }

            SimulationPlayerInput[] destination = inputStorage[count];
            for (int i = 0; i < destination.Length; i++)
                destination[i] = frame.Players[i];
            frames[count].ResetPreallocated(frame.TickIndex, destination);
            count++;
            lastRecordedTick = frame.TickIndex;
            reason = LockstepProtocolReason.None;
            return true;
        }

        public void Reset(int consumedTick = 0)
        {
            if (consumedTick < 0)
                throw new ArgumentOutOfRangeException(nameof(consumedTick));
            count = 0;
            lastRecordedTick = consumedTick;
        }

        public IEnumerator<FrameInputSet> GetEnumerator()
        {
            for (int i = 0; i < count; i++)
                yield return frames[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
