using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public sealed class RuntimeSlotTable
    {
        public const int PageSize = 256;

        private sealed class Entry
        {
            public readonly NTSDEntityRuntime RawRuntime = new NTSDEntityRuntime();
            public LF2Entity Entity;
            public LF2ItrRestTracker.StateSnapshot RawRest;
            public uint Generation;
            public bool Claimed;

            public Entry()
            {
                RawRuntime.Reset();
            }
        }

        private sealed class Page
        {
            public readonly Entry[] Entries = CreateEntries();

            private static Entry[] CreateEntries()
            {
                var entries = new Entry[PageSize];
                for (int i = 0; i < entries.Length; i++)
                    entries[i] = new Entry();

                return entries;
            }
        }

        private readonly RuntimeSlotAllocator allocator;
        private readonly Page[] pages;

        public RuntimeSlotTable(int logicalCapacity, int stageStart = 20, int dynamicStart = 50)
        {
            if (logicalCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(logicalCapacity));

            LogicalCapacity = logicalCapacity;
            allocator = new RuntimeSlotAllocator(logicalCapacity, stageStart, dynamicStart);
            pages = new Page[(logicalCapacity + PageSize - 1) / PageSize];
        }

        public int LogicalCapacity { get; }
        public int ClaimedCount => allocator.ClaimedCount;
        public int MaterializedPageCount { get; private set; }

        public bool IsAddressable(int slot)
        {
            return slot >= 0 && slot < LogicalCapacity;
        }

        public bool IsClaimed(int slot)
        {
            return allocator.IsClaimed(slot);
        }

        public int PeekLowest(int startSlot, int endSlotExclusive)
        {
            return allocator.PeekLowest(startSlot, endSlotExclusive);
        }

        public bool TryClaim(int slot, LF2Entity entity, out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (entity == null || !allocator.ClaimRequired(slot))
                return false;

            Entry entry = GetEntry(slot, true);
            entry.Generation = NextGeneration(entry.Generation);
            entry.Entity = entity;
            entry.Claimed = true;
            handle = new RuntimeEntityHandle(slot, entry.Generation);
            return true;
        }

        public int AllocateLowest(int startSlot, LF2Entity entity, out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (entity == null)
                return -1;

            int slot = allocator.AllocateLowest(startSlot);
            if (slot < 0)
                return -1;

            Entry entry = GetEntry(slot, true);
            entry.Generation = NextGeneration(entry.Generation);
            entry.Entity = entity;
            entry.Claimed = true;
            handle = new RuntimeEntityHandle(slot, entry.Generation);
            return slot;
        }

        public bool Release(RuntimeEntityHandle handle)
        {
            if (!TryGetMatchingEntry(handle, out Entry entry))
                return false;

            return ReleaseEntry(handle.Slot, entry);
        }

        public bool Release(int slot, LF2Entity expectedEntity)
        {
            if (expectedEntity == null || !IsAddressable(slot) || !allocator.IsClaimed(slot))
                return false;

            Entry entry = GetEntry(slot, false);
            if (entry == null || !entry.Claimed || !ReferenceEquals(entry.Entity, expectedEntity))
                return false;

            return ReleaseEntry(slot, entry);
        }

        // World passes intentionally resolve the current occupant by slot. Long-lived
        // references must use RuntimeEntityHandle so generation checks still apply.
        public LF2Entity GetCurrentOccupant(int slot)
        {
            if (!IsAddressable(slot) || !allocator.IsClaimed(slot))
                return null;

            Entry entry = GetEntry(slot, false);
            return entry != null && entry.Claimed ? entry.Entity : null;
        }

        public bool TryGetCurrentOccupant(int slot, out LF2Entity entity)
        {
            entity = GetCurrentOccupant(slot);
            return entity != null;
        }

        private bool ReleaseEntry(int slot, Entry entry)
        {
            if (!allocator.Release(slot))
                return false;

            entry.Entity = null;
            entry.Claimed = false;
            entry.Generation = NextGeneration(entry.Generation);
            return true;
        }

        public bool TryResolve(RuntimeEntityHandle handle, out LF2Entity entity)
        {
            entity = null;
            if (!TryGetMatchingEntry(handle, out Entry entry))
                return false;

            entity = entry.Entity;
            return entity != null;
        }

        public NTSDEntityRuntime GetRawRuntime(int slot)
        {
            return IsAddressable(slot) ? GetEntry(slot, true).RawRuntime : null;
        }

        public LF2ItrRestTracker.StateSnapshot GetRawRest(int slot)
        {
            Entry entry = GetEntry(slot, false);
            return entry?.RawRest;
        }

        public bool SetRawRest(int slot, LF2ItrRestTracker.StateSnapshot state)
        {
            if (!IsAddressable(slot))
                return false;

            GetEntry(slot, true).RawRest = state;
            return true;
        }

        public void Reset()
        {
            allocator.Reset();
            for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
            {
                Page page = pages[pageIndex];
                if (page == null)
                    continue;

                for (int entryIndex = 0; entryIndex < page.Entries.Length; entryIndex++)
                {
                    int slot = pageIndex * PageSize + entryIndex;
                    if (slot >= LogicalCapacity)
                        break;

                    Entry entry = page.Entries[entryIndex];
                    entry.Entity = null;
                    entry.RawRuntime.Reset();
                    entry.RawRest = null;
                    entry.Claimed = false;
                    entry.Generation = NextGeneration(entry.Generation);
                }
            }
        }

        private bool TryGetMatchingEntry(RuntimeEntityHandle handle, out Entry entry)
        {
            entry = null;
            if (!handle.IsValid || !IsAddressable(handle.Slot) || !allocator.IsClaimed(handle.Slot))
                return false;

            entry = GetEntry(handle.Slot, false);
            return entry != null && entry.Claimed && entry.Generation == handle.Generation;
        }

        private Entry GetEntry(int slot, bool materialize)
        {
            if (!IsAddressable(slot))
                return null;

            int pageIndex = slot / PageSize;
            Page page = pages[pageIndex];
            if (page == null && materialize)
            {
                page = new Page();
                pages[pageIndex] = page;
                MaterializedPageCount++;
            }

            return page?.Entries[slot % PageSize];
        }

        private static uint NextGeneration(uint generation)
        {
            generation++;
            return generation == 0 ? 1u : generation;
        }
    }
}
