using System;
using NTSD.Input;

namespace NTSD.Simulation
{
    public readonly struct SimInputEventBatch
    {
        private readonly SimInputEvent[] events;
        private readonly int offset;

        internal SimInputEventBatch(SimInputEvent[] events, int offset, int count)
        {
            this.events = events;
            this.offset = offset;
            Count = count;
        }

        public int Count { get; }

        public SimInputEvent this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return events[offset + index];
            }
        }
    }

    /// <summary>
    /// Fixed-capacity tick-indexed input ring. All managed storage is allocated by
    /// the constructor so enqueue and dequeue remain allocation-free in battle.
    /// </summary>
    public sealed class SimInputBuffer
    {
        private const int DefaultTickCapacity = 64;
        private const int DefaultEventsPerTick = 16;

        private readonly int tickCapacity;
        private readonly int eventsPerTick;
        private readonly int[] tickTags;
        private readonly byte[] eventCounts;
        private readonly SimInputEvent[] events;

        private int currentTickIndex;
        private int bufferedTickCount;

        public SimInputBuffer(
            int tickCapacity = DefaultTickCapacity,
            int eventsPerTick = DefaultEventsPerTick)
        {
            if (tickCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(tickCapacity));
            if (eventsPerTick <= 0 || eventsPerTick > byte.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(eventsPerTick));

            this.tickCapacity = tickCapacity;
            this.eventsPerTick = eventsPerTick;
            tickTags = new int[tickCapacity];
            eventCounts = new byte[tickCapacity];
            events = new SimInputEvent[tickCapacity * eventsPerTick];
            Clear();
        }

        public int BufferedTickCount => bufferedTickCount;
        public int CurrentTickIndex => currentTickIndex;
        public long RejectedEventCount { get; private set; }

        public void EnqueueForNextTick(FuncKeyMask key, bool down)
        {
            EnqueueForTick(currentTickIndex + 1, key, down, completePacket: false);
        }

        public void EnqueueForTick(int tickIndex, FuncKeyMask key, bool down)
        {
            EnqueueForTick(tickIndex, key, down, completePacket: false);
        }

        public void EnqueueCompletePacketKeyForTick(
            int tickIndex,
            FuncKeyMask key,
            bool down)
        {
            EnqueueForTick(tickIndex, key, down, completePacket: true);
        }

        private void EnqueueForTick(
            int tickIndex,
            FuncKeyMask key,
            bool down,
            bool completePacket)
        {
            if (tickIndex < 0)
            {
                RejectedEventCount++;
                return;
            }

            int frameIndex = tickIndex % tickCapacity;
            int storedTick = tickTags[frameIndex];
            if (storedTick != tickIndex)
            {
                if (storedTick >= 0 && eventCounts[frameIndex] != 0)
                {
                    RejectedEventCount++;
                    return;
                }

                tickTags[frameIndex] = tickIndex;
                eventCounts[frameIndex] = 0;
                bufferedTickCount++;
            }

            int count = eventCounts[frameIndex];
            if (count >= eventsPerTick)
            {
                RejectedEventCount++;
                return;
            }

            events[frameIndex * eventsPerTick + count] = new SimInputEvent(
                tickIndex,
                key,
                down,
                completePacket);
            eventCounts[frameIndex] = (byte)(count + 1);
        }

        public bool TryDequeueAll(int tickIndex, out SimInputEventBatch batch)
        {
            currentTickIndex = tickIndex;
            if (tickIndex < 0)
            {
                batch = default;
                return false;
            }

            int frameIndex = tickIndex % tickCapacity;
            int count = eventCounts[frameIndex];
            if (tickTags[frameIndex] != tickIndex || count == 0)
            {
                batch = default;
                return false;
            }

            batch = new SimInputEventBatch(
                events,
                frameIndex * eventsPerTick,
                count);
            tickTags[frameIndex] = -1;
            eventCounts[frameIndex] = 0;
            bufferedTickCount--;
            return true;
        }

        public void Clear()
        {
            Array.Fill(tickTags, -1);
            Array.Clear(eventCounts, 0, eventCounts.Length);
            Array.Clear(events, 0, events.Length);
            currentTickIndex = 0;
            bufferedTickCount = 0;
            RejectedEventCount = 0;
        }
    }
}
