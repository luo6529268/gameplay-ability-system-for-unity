using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public sealed class RuntimeSlotTable
    {
        public const int PageSize = 256;

        /// <summary>
        /// Read-only slot state for diagnostic/checksum consumers. Obtaining a view never
        /// creates a backing page or resets a raw runtime record.
        /// </summary>
        public readonly struct ReadOnlySlotView
        {
            internal ReadOnlySlotView(
                int runtimeSlot,
                bool claimed,
                uint generation,
                LF2Entity entity,
                NTSDEntityRuntime rawRuntime)
            {
                RuntimeSlot = runtimeSlot;
                Claimed = claimed;
                Generation = generation;
                Entity = entity;
                RawRuntime = rawRuntime;
            }

            public int RuntimeSlot { get; }
            public bool Claimed { get; }
            public uint Generation { get; }
            public LF2Entity Entity { get; }
            public NTSDEntityRuntime RawRuntime { get; }
        }

        private sealed class Entry
        {
            public readonly NTSDEntityRuntime RawRuntime = new NTSDEntityRuntime();
            public LF2Entity Entity;
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
        private Page[] pages;
        private ulong occupancyEpoch = 1;

        public RuntimeSlotTable(int logicalCapacity, int stageStart = 20, int dynamicStart = 50)
        {
            if (logicalCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(logicalCapacity));

            LogicalCapacity = logicalCapacity;
            allocator = new RuntimeSlotAllocator(logicalCapacity, stageStart, dynamicStart);
            pages = new Page[(logicalCapacity + PageSize - 1) / PageSize];
        }

        public int LogicalCapacity { get; private set; }
        public int ClaimedCount => allocator.ClaimedCount;
        public int MaterializedPageCount { get; private set; }
        public ulong OccupancyEpoch => occupancyEpoch;

        public void PrepareAllPages()
        {
            for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
            {
                if (pages[pageIndex] != null)
                    continue;

                pages[pageIndex] = new Page();
                MaterializedPageCount++;
            }
        }

        public bool GrowTo(int newLogicalCapacity)
        {
            if (newLogicalCapacity < LogicalCapacity)
                return false;
            if (newLogicalCapacity == LogicalCapacity)
                return true;

            int newPageCount = (newLogicalCapacity + PageSize - 1) / PageSize;
            var grownPages = new Page[newPageCount];
            Array.Copy(pages, grownPages, pages.Length);

            if (!allocator.GrowTo(newLogicalCapacity))
                return false;

            pages = grownPages;
            LogicalCapacity = newLogicalCapacity;
            AdvanceOccupancyEpoch();
            return true;
        }

        public bool IsAddressable(int slot)
        {
            return slot >= 0 && slot < LogicalCapacity;
        }

        public bool IsClaimed(int slot)
        {
            return allocator.IsClaimed(slot);
        }

        public ReadOnlySlotView GetReadOnlyView(int slot)
        {
            if (!IsAddressable(slot))
                throw new ArgumentOutOfRangeException(nameof(slot));

            Entry entry = GetEntry(slot, false);
            return new ReadOnlySlotView(
                slot,
                allocator.IsClaimed(slot),
                entry?.Generation ?? 0u,
                entry?.Entity,
                entry?.RawRuntime);
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
            AdvanceOccupancyEpoch();
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
            AdvanceOccupancyEpoch();
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

        public bool TryGetCurrentHandle(
            int slot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (expectedEntity == null ||
                !IsAddressable(slot) ||
                !allocator.IsClaimed(slot))
            {
                return false;
            }

            Entry entry = GetEntry(slot, false);
            if (entry == null ||
                !entry.Claimed ||
                entry.Generation == 0 ||
                !ReferenceEquals(entry.Entity, expectedEntity))
            {
                return false;
            }

            handle = new RuntimeEntityHandle(slot, entry.Generation);
            return handle.IsValid;
        }

        private bool ReleaseEntry(int slot, Entry entry)
        {
            if (!allocator.Release(slot))
                return false;

            entry.Entity = null;
            entry.Claimed = false;
            entry.Generation = NextGeneration(entry.Generation);
            AdvanceOccupancyEpoch();
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
                    entry.Claimed = false;
                    entry.Generation = NextGeneration(entry.Generation);
                }
            }
            AdvanceOccupancyEpoch();
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

        private void AdvanceOccupancyEpoch()
        {
            occupancyEpoch++;
            if (occupancyEpoch == 0)
                occupancyEpoch = 1;
        }

#if UNITY_INCLUDE_TESTS
        internal void SetOccupancyEpochForSelfCheck(ulong value)
        {
            occupancyEpoch = value;
        }
#endif
    }
}
