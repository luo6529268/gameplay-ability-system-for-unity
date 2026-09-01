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
                ulong allocationEpoch,
                LF2Entity entity,
                NTSDEntityRuntime rawRuntime)
            {
                RuntimeSlot = runtimeSlot;
                Claimed = claimed;
                Generation = generation;
                AllocationEpoch = allocationEpoch;
                Entity = entity;
                RawRuntime = rawRuntime;
            }

            public int RuntimeSlot { get; }
            public bool Claimed { get; }
            public uint Generation { get; }
            public ulong AllocationEpoch { get; }
            public LF2Entity Entity { get; }
            public NTSDEntityRuntime RawRuntime { get; }
        }

        private sealed class Page
        {
            public readonly NTSDEntityRuntime[] RawRuntimes = CreateRawRuntimes();
            public readonly LF2Entity[] Entities = new LF2Entity[PageSize];

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

        private readonly RuntimeSlotLifecycleState lifecycle;
        private Page[] pages;

        public RuntimeSlotTable(int logicalCapacity, int stageStart = 20, int dynamicStart = 50)
        {
            if (logicalCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(logicalCapacity));

            LogicalCapacity = logicalCapacity;
            lifecycle = new RuntimeSlotLifecycleState(
                logicalCapacity,
                stageStart,
                dynamicStart);
            pages = new Page[(logicalCapacity + PageSize - 1) / PageSize];
        }

        public int LogicalCapacity { get; private set; }
        public int ClaimedCount => lifecycle.ClaimedCount;
        public int MaterializedPageCount { get; private set; }
        public ulong OccupancyEpoch => lifecycle.OccupancyEpoch;

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

            if (!lifecycle.GrowTo(newLogicalCapacity))
                return false;

            pages = grownPages;
            LogicalCapacity = newLogicalCapacity;
            return true;
        }

        public bool IsAddressable(int slot)
        {
            return lifecycle.IsAddressable(slot);
        }

        public bool IsClaimed(int slot)
        {
            return lifecycle.IsClaimed(slot);
        }

        public ReadOnlySlotView GetReadOnlyView(int slot)
        {
            if (!IsAddressable(slot))
                throw new ArgumentOutOfRangeException(nameof(slot));

            Page page = GetPage(slot, false);
            int pageOffset = slot % PageSize;
            return new ReadOnlySlotView(
                slot,
                lifecycle.IsClaimed(slot),
                lifecycle.GetGeneration(slot),
                lifecycle.GetAllocationEpoch(slot),
                page?.Entities[pageOffset],
                page?.RawRuntimes[pageOffset]);
        }

        public int PeekLowest(int startSlot, int endSlotExclusive)
        {
            return lifecycle.PeekLowest(startSlot, endSlotExclusive);
        }

        public bool TryClaim(int slot, LF2Entity entity, out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (!TryBeginClaim(
                    slot,
                    entity,
                    out RuntimeSlotAllocationTicket ticket))
            {
                return false;
            }

            if (!TryCompleteRequiredSideEffect(ticket, true) ||
                !TryCommitAllocation(ticket, out _))
            {
                TryRollbackAllocation(ticket, entity);
                return false;
            }

            handle = ticket.Handle;
            return handle.IsValid;
        }

        public int AllocateLowest(int startSlot, LF2Entity entity, out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            int slot = BeginAllocateLowest(
                startSlot,
                entity,
                out RuntimeSlotAllocationTicket ticket);
            if (slot < 0)
                return -1;

            if (!TryCompleteRequiredSideEffect(ticket, true) ||
                !TryCommitAllocation(ticket, out _))
            {
                TryRollbackAllocation(ticket, entity);
                return -1;
            }

            handle = ticket.Handle;
            return slot;
        }

        internal bool TryBeginClaim(
            int slot,
            LF2Entity entity,
            out RuntimeSlotAllocationTicket ticket)
        {
            ticket = RuntimeSlotAllocationTicket.Invalid;
            if (entity == null ||
                !lifecycle.TryBeginClaimRequired(slot, out ticket))
            {
                return false;
            }

            Page page = GetPage(slot, true);
            page.Entities[slot % PageSize] = entity;
            return true;
        }

        internal int BeginAllocateLowest(
            int startSlot,
            LF2Entity entity,
            out RuntimeSlotAllocationTicket ticket)
        {
            ticket = RuntimeSlotAllocationTicket.Invalid;
            if (entity == null ||
                !lifecycle.TryBeginAllocateLowest(startSlot, out ticket))
            {
                return -1;
            }

            int slot = ticket.Handle.Slot;
            Page page = GetPage(slot, true);
            page.Entities[slot % PageSize] = entity;
            return slot;
        }

        internal bool TryCompleteRequiredSideEffect(
            RuntimeSlotAllocationTicket ticket,
            bool succeeded)
        {
            return lifecycle.TryCompleteRequiredSideEffect(ticket, succeeded);
        }

        internal bool TryCommitAllocation(
            RuntimeSlotAllocationTicket ticket,
            out RuntimeSlotAllocationIdentity identity)
        {
            return lifecycle.TryCommit(ticket, out identity);
        }

        internal bool TryRollbackAllocation(
            RuntimeSlotAllocationTicket ticket,
            LF2Entity expectedEntity)
        {
            if (expectedEntity == null ||
                !ticket.IsValid ||
                !IsAddressable(ticket.Handle.Slot))
            {
                return false;
            }

            int slot = ticket.Handle.Slot;
            Page page = GetPage(slot, false);
            int pageOffset = slot % PageSize;
            if (page == null ||
                !ReferenceEquals(page.Entities[pageOffset], expectedEntity) ||
                !lifecycle.TryRollback(ticket))
            {
                return false;
            }

            page.Entities[pageOffset] = null;
            return true;
        }

        public ulong GetAllocationEpoch(int slot)
        {
            return lifecycle.GetAllocationEpoch(slot);
        }

        public bool Release(RuntimeEntityHandle handle)
        {
            if (!TryGetMatchingSlot(handle, out Page page, out int pageOffset))
                return false;

            return ReleaseSlot(handle, page, pageOffset);
        }

        public bool Release(int slot, LF2Entity expectedEntity)
        {
            if (expectedEntity == null ||
                !IsAddressable(slot) ||
                !lifecycle.IsCommitted(slot))
                return false;

            Page page = GetPage(slot, false);
            int pageOffset = slot % PageSize;
            if (page == null ||
                !ReferenceEquals(page.Entities[pageOffset], expectedEntity))
            {
                return false;
            }

            if (!lifecycle.TryGetCurrentHandle(slot, out RuntimeEntityHandle handle))
                return false;

            return ReleaseSlot(handle, page, pageOffset);
        }

        // World passes intentionally resolve the current occupant by slot. Long-lived
        // references must use RuntimeEntityHandle so generation checks still apply.
        public LF2Entity GetCurrentOccupant(int slot)
        {
            if (!IsAddressable(slot) || !lifecycle.IsCommitted(slot))
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
                !lifecycle.IsClaimed(slot))
            {
                return false;
            }

            Page page = GetPage(slot, false);
            int pageOffset = slot % PageSize;
            if (page == null ||
                !ReferenceEquals(page.Entities[pageOffset], expectedEntity))
            {
                return false;
            }

            return lifecycle.TryGetCurrentHandle(slot, out handle);
        }

        private bool ReleaseSlot(
            RuntimeEntityHandle handle,
            Page page,
            int pageOffset)
        {
            if (!lifecycle.Release(handle))
                return false;

            page.Entities[pageOffset] = null;
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
            lifecycle.ResetFreshWorld();
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
                    lifecycle.InvalidateLocalLeaseForWorldReset(slot);
                }
            }
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

            lifecycle.BeginTopologyRestore();
            for (int slot = 0; slot < LogicalCapacity; slot++)
            {
                BattleRuntimeSlotSnapshot state = snapshot.GetSlot(slot);
                Page page = GetPage(slot, state.Claimed);
                if (page == null)
                    continue;

                int pageOffset = slot % PageSize;
                page.Entities[pageOffset] = null;
                if (!lifecycle.SetLocalGenerationForTopologyRestore(
                        slot,
                        state.Generation))
                {
                    return false;
                }
                if (!state.Claimed)
                    continue;

                if (!lifecycle.TryRestoreCommittedClaim(
                        slot,
                        state.Generation) ||
                    !snapshot.TryGetLocalEntityShell(slot, out LF2Entity entity))
                {
                    return false;
                }
                page.Entities[pageOffset] = entity;
            }

            lifecycle.CompleteTopologyRestore();
            return lifecycle.ClaimedCount == snapshot.ClaimedCount;
        }

        internal void ClearTopologyForSnapshotShellMaterialization()
        {
            lifecycle.BeginTopologyRestore();
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
            lifecycle.CompleteTopologyRestore();
        }

        private bool TryGetMatchingSlot(
            RuntimeEntityHandle handle,
            out Page page,
            out int pageOffset)
        {
            page = null;
            pageOffset = -1;
            if (!handle.IsValid ||
                !IsAddressable(handle.Slot) ||
                !lifecycle.IsClaimed(handle.Slot))
                return false;

            page = GetPage(handle.Slot, false);
            pageOffset = handle.Slot % PageSize;
            return page != null &&
                   lifecycle.TryGetCurrentHandle(
                       handle.Slot,
                       out RuntimeEntityHandle currentHandle) &&
                   currentHandle == handle &&
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

#if UNITY_INCLUDE_TESTS
        internal void SetOccupancyEpochForSelfCheck(ulong value)
        {
            lifecycle.SetOccupancyEpochForSelfCheck(value);
        }
#endif
    }
}
