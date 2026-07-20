using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Deterministically allocates the lowest free runtime slot while preserving
    /// the authority roster, stage, and dynamic slot bands.
    /// </summary>
    public sealed class RuntimeSlotAllocator
    {
        private sealed class Segment
        {
            private readonly int start;
            private int end;
            private int[] heap;
            private int[] positions;
            private int count;
            private int nextUnused;

            public Segment(int start, int end)
            {
                this.start = start;
                this.end = end;
                heap = new int[end - start];
                positions = new int[end - start];
                Reset();
            }

            public int Start => start;
            public int End => end;

            public void GrowTo(int newEnd)
            {
                if (newEnd <= end)
                    return;

                int oldLength = end - start;
                int newLength = newEnd - start;
                var grownHeap = new int[newLength];
                var grownPositions = new int[newLength];
                Array.Copy(heap, grownHeap, count);
                Array.Copy(positions, grownPositions, oldLength);
                for (int index = oldLength; index < newLength; index++)
                    grownPositions[index] = -1;

                heap = grownHeap;
                positions = grownPositions;
                end = newEnd;
            }

            public void Reset()
            {
                count = 0;
                nextUnused = start;
                Array.Fill(positions, -1);
            }

            public void Claim(int slot)
            {
                if (slot < nextUnused)
                {
                    Remove(slot);
                    return;
                }

                for (int skipped = nextUnused; skipped < slot; skipped++)
                    Add(skipped);

                nextUnused = slot + 1;
            }

            public void Release(int slot)
            {
                if (slot < nextUnused)
                    Add(slot);
            }

            public int PeekLowest(int rangeStart, int rangeEnd, bool[] claimed)
            {
                int lower = Math.Max(start, rangeStart);
                int upper = Math.Min(end, rangeEnd);
                if (lower >= upper)
                    return -1;

                if (lower == start)
                {
                    int recycled = count > 0 ? heap[0] : int.MaxValue;
                    int untouched = nextUnused < upper ? nextUnused : int.MaxValue;
                    int candidate = Math.Min(recycled, untouched);
                    return candidate < upper ? candidate : -1;
                }

                // Production allocation starts at a segment boundary (0, 20, or 50).
                // Retain exact range semantics for diagnostic callers with a partial band.
                for (int slot = lower; slot < upper; slot++)
                {
                    if (!claimed[slot])
                        return slot;
                }

                return -1;
            }

            private void Add(int slot)
            {
                int local = slot - start;
                if (positions[local] >= 0)
                    return;

                int index = count++;
                heap[index] = slot;
                positions[local] = index;
                SiftUp(index);
            }

            private void Remove(int slot)
            {
                int local = slot - start;
                int index = positions[local];
                if (index < 0)
                    return;

                int lastIndex = --count;
                positions[local] = -1;
                if (index == lastIndex)
                    return;

                int replacement = heap[lastIndex];
                heap[index] = replacement;
                positions[replacement - start] = index;

                int parent = (index - 1) / 2;
                if (index > 0 && heap[index] < heap[parent])
                    SiftUp(index);
                else
                    SiftDown(index);
            }

            private void SiftUp(int index)
            {
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (heap[parent] <= heap[index])
                        return;

                    Swap(parent, index);
                    index = parent;
                }
            }

            private void SiftDown(int index)
            {
                while (true)
                {
                    int left = index * 2 + 1;
                    if (left >= count)
                        return;

                    int right = left + 1;
                    int smaller = right < count && heap[right] < heap[left] ? right : left;
                    if (heap[index] <= heap[smaller])
                        return;

                    Swap(index, smaller);
                    index = smaller;
                }
            }

            private void Swap(int first, int second)
            {
                int value = heap[first];
                heap[first] = heap[second];
                heap[second] = value;
                positions[heap[first] - start] = first;
                positions[heap[second] - start] = second;
            }
        }

        private bool[] claimed;
        private readonly Segment[] segments;

        public RuntimeSlotAllocator(int capacity, int stageStart = 20, int dynamicStart = 50)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            if (stageStart < 0 || stageStart > dynamicStart || dynamicStart > capacity)
                throw new ArgumentOutOfRangeException(nameof(stageStart));

            Capacity = capacity;
            claimed = new bool[capacity];
            segments = new[]
            {
                new Segment(0, stageStart),
                new Segment(stageStart, dynamicStart),
                new Segment(dynamicStart, capacity)
            };
        }

        public int Capacity { get; private set; }
        public int ClaimedCount { get; private set; }

        public bool GrowTo(int newCapacity)
        {
            if (newCapacity < Capacity)
                return false;
            if (newCapacity == Capacity)
                return true;

            var grownClaimed = new bool[newCapacity];
            Array.Copy(claimed, grownClaimed, claimed.Length);

            segments[segments.Length - 1].GrowTo(newCapacity);
            claimed = grownClaimed;
            Capacity = newCapacity;
            return true;
        }

        public bool ClaimRequired(int slot)
        {
            if (!IsInRange(slot) || claimed[slot])
                return false;

            claimed[slot] = true;
            FindSegment(slot).Claim(slot);
            ClaimedCount++;
            return true;
        }

        public bool ClaimExisting(int slot, int minimumSlot = 0)
        {
            return slot >= minimumSlot && ClaimRequired(slot);
        }

        public int AllocateLowest(int startSlot)
        {
            int slot = PeekLowest(startSlot, Capacity);
            return slot >= 0 && ClaimRequired(slot) ? slot : -1;
        }

        public int PeekLowest(int startSlot, int endSlotExclusive)
        {
            int start = Math.Max(0, startSlot);
            int end = Math.Min(Capacity, endSlotExclusive);
            if (start >= end)
                return -1;

            for (int i = 0; i < segments.Length; i++)
            {
                int candidate = segments[i].PeekLowest(start, end, claimed);
                if (candidate >= 0)
                    return candidate;
            }

            return -1;
        }

        public bool Release(int slot)
        {
            if (!IsInRange(slot) || !claimed[slot])
                return false;

            claimed[slot] = false;
            FindSegment(slot).Release(slot);
            ClaimedCount--;
            return true;
        }

        public bool IsClaimed(int slot)
        {
            return IsInRange(slot) && claimed[slot];
        }

        public void Reset()
        {
            Array.Clear(claimed, 0, claimed.Length);
            ClaimedCount = 0;
            for (int i = 0; i < segments.Length; i++)
                segments[i].Reset();
        }

        private bool IsInRange(int slot)
        {
            return slot >= 0 && slot < Capacity;
        }

        private Segment FindSegment(int slot)
        {
            for (int i = 0; i < segments.Length; i++)
            {
                if (slot >= segments[i].Start && slot < segments[i].End)
                    return segments[i];
            }

            throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }
}
