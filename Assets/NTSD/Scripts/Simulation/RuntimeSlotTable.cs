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

        private sealed class Page
        {
            public readonly NTSDEntityRuntime[] RawRuntimes = CreateRawRuntimes();
            public readonly LF2Entity[] Entities = new LF2Entity[PageSize];
            public readonly uint[] Generations = new uint[PageSize];

            private static NTSDEntityRuntime[] CreateRawRuntimes()
            {
                var runtimes = new NTSDEntityRuntime[PageSize];
                for (int i = 0; i < runtimes.Length; i++)
                {
                    NTSDEntityRuntime runtime = new NTSDEntityRuntime();
                    runtime.Reset();
                    runtimes[i] = runtime;
                }

                return runtimes;
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

            Page page = GetPage(slot, false);
            int pageOffset = slot % PageSize;
            return new ReadOnlySlotView(
                slot,
                allocator.IsClaimed(slot),
                page?.Generations[pageOffset] ?? 0u,
                page?.Entities[pageOffset],
                page?.RawRuntimes[pageOffset]);
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

            Page page = GetPage(slot, true);
            int pageOffset = slot % PageSize;
            page.Generations[pageOffset] = NextGeneration(
                page.Generations[pageOffset]);
            page.Entities[pageOffset] = entity;
            handle = new RuntimeEntityHandle(
                slot,
                page.Generations[pageOffset]);
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

            Page page = GetPage(slot, true);
            int pageOffset = slot % PageSize;
            page.Generations[pageOffset] = NextGeneration(
                page.Generations[pageOffset]);
            page.Entities[pageOffset] = entity;
            handle = new RuntimeEntityHandle(
                slot,
                page.Generations[pageOffset]);
            AdvanceOccupancyEpoch();
            return slot;
        }

        public bool Release(RuntimeEntityHandle handle)
        {
            if (!TryGetMatchingSlot(handle, out Page page, out int pageOffset))
                return false;

            return ReleaseSlot(handle.Slot, page, pageOffset);
        }

        public bool Release(int slot, LF2Entity expectedEntity)
        {
            if (expectedEntity == null || !IsAddressable(slot) || !allocator.IsClaimed(slot))
                return false;

            Page page = GetPage(slot, false);
            int pageOffset = slot % PageSize;
            if (page == null ||
                !ReferenceEquals(page.Entities[pageOffset], expectedEntity))
            {
                return false;
            }

            return ReleaseSlot(slot, page, pageOffset);
        }

        // World passes intentionally resolve the current occupant by slot. Long-lived
        // references must use RuntimeEntityHandle so generation checks still apply.
        public LF2Entity GetCurrentOccupant(int slot)
        {
            if (!IsAddressable(slot) || !allocator.IsClaimed(slot))
                return null;

            Page page = GetPage(slot, false);
            return page?.Entities[slot % PageSize];
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

            Page page = GetPage(slot, false);
            int pageOffset = slot % PageSize;
            if (page == null ||
                page.Generations[pageOffset] == 0 ||
                !ReferenceEquals(page.Entities[pageOffset], expectedEntity))
            {
                return false;
            }

            handle = new RuntimeEntityHandle(slot, page.Generations[pageOffset]);
            return handle.IsValid;
        }

        private bool ReleaseSlot(int slot, Page page, int pageOffset)
        {
            if (!allocator.Release(slot))
                return false;

            page.Entities[pageOffset] = null;
            page.Generations[pageOffset] = NextGeneration(
                page.Generations[pageOffset]);
            AdvanceOccupancyEpoch();
            return true;
        }

        public bool TryResolve(RuntimeEntityHandle handle, out LF2Entity entity)
        {
            entity = null;
            if (!TryGetMatchingSlot(handle, out Page page, out int pageOffset))
                return false;

            entity = page.Entities[pageOffset];
            return entity != null;
        }

        public NTSDEntityRuntime GetRawRuntime(int slot)
        {
            if (!IsAddressable(slot))
                return null;

            Page page = GetPage(slot, true);
            return page.RawRuntimes[slot % PageSize];
        }

        public void Reset()
        {
            allocator.Reset();
            for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
            {
                Page page = pages[pageIndex];
                if (page == null)
                    continue;

                for (int pageOffset = 0; pageOffset < PageSize; pageOffset++)
                {
                    int slot = pageIndex * PageSize + pageOffset;
                    if (slot >= LogicalCapacity)
                        break;

                    page.Entities[pageOffset] = null;
                    page.RawRuntimes[pageOffset].Reset();
                    page.Generations[pageOffset] = NextGeneration(
                        page.Generations[pageOffset]);
                }
            }
            AdvanceOccupancyEpoch();
        }

        internal bool TryRestoreSnapshotTopology(
            BattleWorldRuntimeSlotSnapshotBuffer snapshot)
        {
            if (snapshot == null ||
                snapshot.SchemaVersion !=
                    BattleWorldRuntimeSlotSnapshotBuffer.CurrentSchemaVersion ||
                snapshot.SlotCapacity != LogicalCapacity)
            {
                return false;
            }

            for (int slot = 0; slot < LogicalCapacity; slot++)
            {
                BattleRuntimeSlotSnapshot state = snapshot.GetSlot(slot);
                if (state.Claimed &&
                    (!snapshot.TryGetLocalEntityShell(slot, out LF2Entity entity) ||
                     entity == null ||
                     state.Generation == 0))
                {
                    return false;
                }
            }

            allocator.Reset();
            for (int slot = 0; slot < LogicalCapacity; slot++)
            {
                BattleRuntimeSlotSnapshot state = snapshot.GetSlot(slot);
                Page page = GetPage(slot, state.Claimed);
                if (page == null)
                    continue;

                int pageOffset = slot % PageSize;
                page.Entities[pageOffset] = null;
                page.Generations[pageOffset] = state.Generation;
                if (!state.Claimed)
                    continue;

                if (!allocator.ClaimRequired(slot) ||
                    !snapshot.TryGetLocalEntityShell(slot, out LF2Entity entity))
                {
                    return false;
                }
                page.Entities[pageOffset] = entity;
            }

            AdvanceOccupancyEpoch();
            return allocator.ClaimedCount == snapshot.ClaimedCount;
        }

        internal void ClearTopologyForSnapshotShellMaterialization()
        {
            allocator.Reset();
            for (int pageIndex = 0; pageIndex < pages.Length; pageIndex++)
            {
                Page page = pages[pageIndex];
                if (page == null)
                    continue;

                for (int pageOffset = 0; pageOffset < PageSize; pageOffset++)
                {
                    int slot = pageIndex * PageSize + pageOffset;
                    if (slot >= LogicalCapacity)
                        break;
                    page.Entities[pageOffset] = null;
                }
            }
            AdvanceOccupancyEpoch();
        }

        private bool TryGetMatchingSlot(
            RuntimeEntityHandle handle,
            out Page page,
            out int pageOffset)
        {
            page = null;
            pageOffset = -1;
            if (!handle.IsValid || !IsAddressable(handle.Slot) || !allocator.IsClaimed(handle.Slot))
                return false;

            page = GetPage(handle.Slot, false);
            pageOffset = handle.Slot % PageSize;
            return page != null &&
                   page.Generations[pageOffset] == handle.Generation &&
                   page.Entities[pageOffset] != null;
        }

        private Page GetPage(int slot, bool materialize)
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

            return page;
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
