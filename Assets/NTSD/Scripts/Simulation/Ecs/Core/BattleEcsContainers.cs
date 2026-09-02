using System;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Fixed-capacity bitset used by authoritative slot-ordered queries.
    /// It never grows after the world capacity is sealed.
    /// </summary>
    internal sealed class BattleSlotBitSet
    {
        private readonly ulong[] words;

        public BattleSlotBitSet(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
            words = new ulong[(capacity + 63) >> 6];
        }

        public int Capacity { get; }
        public int Count { get; private set; }

        public bool Contains(int slot)
        {
            return (uint)slot < (uint)Capacity &&
                   (words[slot >> 6] & (1UL << (slot & 63))) != 0;
        }

        public void Set(int slot)
        {
            ValidateSlot(slot);
            int wordIndex = slot >> 6;
            ulong mask = 1UL << (slot & 63);
            if ((words[wordIndex] & mask) != 0)
                return;

            words[wordIndex] |= mask;
            Count++;
        }

        public void Clear(int slot)
        {
            ValidateSlot(slot);
            int wordIndex = slot >> 6;
            ulong mask = 1UL << (slot & 63);
            if ((words[wordIndex] & mask) == 0)
                return;

            words[wordIndex] &= ~mask;
            Count--;
        }

        public void ClearAll()
        {
            Array.Clear(words, 0, words.Length);
            Count = 0;
        }

        public int FindNextSet(int startSlot)
        {
            if (startSlot < 0)
                startSlot = 0;
            if (startSlot >= Capacity)
                return -1;

            int wordIndex = startSlot >> 6;
            int bitOffset = startSlot & 63;
            ulong word = words[wordIndex] & (ulong.MaxValue << bitOffset);
            while (true)
            {
                if (word != 0)
                {
                    int firstBit = 0;
                    while ((word & 1UL) == 0)
                    {
                        word >>= 1;
                        firstBit++;
                    }

                    int slot = (wordIndex << 6) + firstBit;
                    return slot < Capacity ? slot : -1;
                }

                wordIndex++;
                if (wordIndex >= words.Length)
                    return -1;
                word = words[wordIndex];
            }
        }

        private void ValidateSlot(int slot)
        {
            if ((uint)slot >= (uint)Capacity)
                throw new ArgumentOutOfRangeException(nameof(slot));
        }
    }

    /// <summary>
    /// Fixed-capacity sparse set. Dense order is deliberately not authoritative;
    /// result-sensitive consumers must stabilize by slot before consumption.
    /// </summary>
    internal sealed class BattleSparseSet<T>
        where T : struct
    {
        private readonly int[] sparse;
        private readonly int[] denseSlots;
        private readonly T[] denseValues;

        public BattleSparseSet(int slotCapacity, int denseCapacity)
        {
            if (slotCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            if (denseCapacity <= 0 || denseCapacity > slotCapacity)
                throw new ArgumentOutOfRangeException(nameof(denseCapacity));

            sparse = new int[slotCapacity];
            denseSlots = new int[denseCapacity];
            denseValues = new T[denseCapacity];
            for (int i = 0; i < sparse.Length; i++)
                sparse[i] = -1;
        }

        public int SlotCapacity => sparse.Length;
        public int DenseCapacity => denseSlots.Length;
        public int Count { get; private set; }

        public bool Contains(int slot)
        {
            if ((uint)slot >= (uint)sparse.Length)
                return false;

            int denseIndex = sparse[slot];
            return (uint)denseIndex < (uint)Count && denseSlots[denseIndex] == slot;
        }

        public bool TryGet(int slot, out T value)
        {
            if (Contains(slot))
            {
                value = denseValues[sparse[slot]];
                return true;
            }

            value = default;
            return false;
        }

        public bool AddOrSet(int slot, in T value)
        {
            if ((uint)slot >= (uint)sparse.Length)
                throw new ArgumentOutOfRangeException(nameof(slot));

            int denseIndex = sparse[slot];
            if ((uint)denseIndex < (uint)Count && denseSlots[denseIndex] == slot)
            {
                denseValues[denseIndex] = value;
                return true;
            }

            if (Count >= denseSlots.Length)
                return false;

            denseIndex = Count++;
            sparse[slot] = denseIndex;
            denseSlots[denseIndex] = slot;
            denseValues[denseIndex] = value;
            return true;
        }

        public bool Remove(int slot)
        {
            if (!Contains(slot))
                return false;

            int denseIndex = sparse[slot];
            int lastIndex = Count - 1;
            int movedSlot = denseSlots[lastIndex];
            if (denseIndex != lastIndex)
            {
                denseSlots[denseIndex] = movedSlot;
                denseValues[denseIndex] = denseValues[lastIndex];
                sparse[movedSlot] = denseIndex;
            }

            sparse[slot] = -1;
            denseSlots[lastIndex] = 0;
            denseValues[lastIndex] = default;
            Count = lastIndex;
            return true;
        }

        public int GetDenseSlot(int denseIndex)
        {
            if ((uint)denseIndex >= (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(denseIndex));
            return denseSlots[denseIndex];
        }

        public T GetDenseValue(int denseIndex)
        {
            if ((uint)denseIndex >= (uint)Count)
                throw new ArgumentOutOfRangeException(nameof(denseIndex));
            return denseValues[denseIndex];
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++)
            {
                sparse[denseSlots[i]] = -1;
                denseSlots[i] = 0;
                denseValues[i] = default;
            }
            Count = 0;
        }
    }
}
