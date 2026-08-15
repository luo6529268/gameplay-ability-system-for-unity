using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// Exclusive lease for one victim slot in a RuntimeRestStore. The token is
    /// invalidated when the slot/world is reset or the lease is released.
    /// </summary>
    public readonly struct RuntimeRestBindingHandle
    {
        internal RuntimeRestBindingHandle(RuntimeRestStore owner, int victimSlot, int token)
        {
            Owner = owner;
            VictimSlot = victimSlot;
            Token = token;
        }

        internal RuntimeRestStore Owner { get; }
        internal int Token { get; }
        public int BoundVictimSlot => VictimSlot;
        public bool IsValid => Owner != null && Token != 0;
        private int VictimSlot { get; }
    }

    /// <summary>
    /// World-independent storage for the directional runtime rest domains.
    /// This is foundation infrastructure and is not connected to production hit resolution yet.
    /// </summary>
    public sealed class RuntimeRestStore
    {
        public const int PageSize = 256;

        public readonly struct ARestEntry
        {
            public ARestEntry(int attackerSlot, int value)
            {
                AttackerSlot = attackerSlot;
                Value = value;
            }

            public int AttackerSlot { get; }
            public int Value { get; }
        }

        public readonly struct VRestEntry
        {
            public VRestEntry(int victimSlot, int attackerSlot, int value)
            {
                VictimSlot = victimSlot;
                AttackerSlot = attackerSlot;
                Value = value;
            }

            public int VictimSlot { get; }
            public int AttackerSlot { get; }
            public int Value { get; }
        }

        public sealed class DiagnosticSnapshot
        {
            internal DiagnosticSnapshot(
                int logicalCapacity,
                ARestEntry[] aRestEntries,
                VRestEntry[] vRestEntries)
            {
                LogicalCapacity = logicalCapacity;
                ARestEntries = aRestEntries;
                VRestEntries = vRestEntries;
            }

            public int LogicalCapacity { get; }
            public IReadOnlyList<ARestEntry> ARestEntries { get; }
            public IReadOnlyList<VRestEntry> VRestEntries { get; }
        }

        private sealed class VRestPage
        {
            public readonly Dictionary<int, int>[] Rows = new Dictionary<int, int>[PageSize];
        }

        /// <summary>
        /// Preallocated sparse rows for extended desktop worlds. Nodes live in
        /// contiguous arrays and are linked by integer indices, so insertion and
        /// removal reuse storage without allocating managed objects during battle.
        /// Each row remains sorted by attacker slot for deterministic traversal.
        /// </summary>
        private sealed class SparseVRestTable
        {
            private int[] rowHeads;
            private int[] attackerSlots;
            private int[] values;
            private int[] nextIndices;
            private int freeHead;

            internal SparseVRestTable(int rowCapacity, int entryCapacity)
            {
                rowHeads = new int[rowCapacity];
                Array.Fill(rowHeads, -1);
                attackerSlots = new int[entryCapacity];
                values = new int[entryCapacity];
                nextIndices = new int[entryCapacity];
                InitializeFreeRange(0, entryCapacity, -1);
            }

            internal int EntryCapacity => values.Length;

            internal bool TryGet(int victimSlot, int attackerSlot, out int value)
            {
                int node = rowHeads[victimSlot];
                while (node >= 0 && attackerSlots[node] < attackerSlot)
                    node = nextIndices[node];

                if (node >= 0 && attackerSlots[node] == attackerSlot)
                {
                    value = values[node];
                    return true;
                }

                value = 0;
                return false;
            }

            internal bool TrySet(
                int victimSlot,
                int attackerSlot,
                int value,
                out bool added,
                out bool removed)
            {
                added = false;
                removed = false;
                int previous = -1;
                int node = rowHeads[victimSlot];
                while (node >= 0 && attackerSlots[node] < attackerSlot)
                {
                    previous = node;
                    node = nextIndices[node];
                }

                if (node >= 0 && attackerSlots[node] == attackerSlot)
                {
                    if (value > 0)
                    {
                        values[node] = value;
                        return true;
                    }

                    int next = nextIndices[node];
                    if (previous < 0)
                        rowHeads[victimSlot] = next;
                    else
                        nextIndices[previous] = next;
                    ReleaseNode(node);
                    removed = true;
                    return true;
                }

                if (value <= 0)
                    return true;
                if (freeHead < 0)
                    return false;

                int acquired = freeHead;
                freeHead = nextIndices[acquired];
                attackerSlots[acquired] = attackerSlot;
                values[acquired] = value;
                nextIndices[acquired] = node;
                if (previous < 0)
                    rowHeads[victimSlot] = acquired;
                else
                    nextIndices[previous] = acquired;
                added = true;
                return true;
            }

            internal int ClearRow(int victimSlot)
            {
                int removed = 0;
                int node = rowHeads[victimSlot];
                rowHeads[victimSlot] = -1;
                while (node >= 0)
                {
                    int next = nextIndices[node];
                    ReleaseNode(node);
                    removed++;
                    node = next;
                }

                return removed;
            }

            internal void ClearAll()
            {
                Array.Fill(rowHeads, -1);
                Array.Clear(attackerSlots, 0, attackerSlots.Length);
                Array.Clear(values, 0, values.Length);
                InitializeFreeRange(0, nextIndices.Length, -1);
            }

            internal bool HasRow(int victimSlot)
            {
                return rowHeads[victimSlot] >= 0;
            }

            internal int GetFirstNode(int victimSlot)
            {
                return rowHeads[victimSlot];
            }

            internal int GetNextNode(int node)
            {
                return nextIndices[node];
            }

            internal int GetAttackerSlot(int node)
            {
                return attackerSlots[node];
            }

            internal int GetValue(int node)
            {
                return values[node];
            }

            internal void Grow(int rowCapacity, int entryCapacity)
            {
                if (rowCapacity > rowHeads.Length)
                {
                    int oldRowCapacity = rowHeads.Length;
                    Array.Resize(ref rowHeads, rowCapacity);
                    Array.Fill(rowHeads, -1, oldRowCapacity, rowCapacity - oldRowCapacity);
                }

                if (entryCapacity <= values.Length)
                    return;

                int oldEntryCapacity = values.Length;
                Array.Resize(ref attackerSlots, entryCapacity);
                Array.Resize(ref values, entryCapacity);
                Array.Resize(ref nextIndices, entryCapacity);
                InitializeFreeRange(oldEntryCapacity, entryCapacity, freeHead);
            }

            private void InitializeFreeRange(int start, int end, int tail)
            {
                if (end <= start)
                {
                    freeHead = tail;
                    return;
                }

                for (int index = start; index < end - 1; index++)
                    nextIndices[index] = index + 1;
                nextIndices[end - 1] = tail;
                freeHead = start;
            }

            private void ReleaseNode(int node)
            {
                attackerSlots[node] = 0;
                values[node] = 0;
                nextIndices[node] = freeHead;
                freeHead = node;
            }
        }

        private const int MaximumDenseBattleCapacity = 2048;
        private const int DefaultSparseEntriesPerRuntimeSlot = 32;
        private const int MinimumSparseEntryCapacity = 128;

        private int[][] aRestPages;
        private VRestPage[] vRestPages;
        private int[] bindingTokensByVictim;
        private int[] denseVRestValues;
        private int[] denseVRestRowEntryCounts;
        private SparseVRestTable sparseVRestTable;
        private bool preparedForBattle;
        private bool capacitySealed;
        private int[] collisionEligibilityStamp;
        private int collisionEligibilityEpoch;
        private readonly List<int> collisionTickScratch;
        private int[] activeVRestRowIndices;
        private readonly List<int> activeVRestVictimSlots;
        private int nextBindingToken = 1;

        public RuntimeRestStore(int logicalCapacity)
        {
            if (logicalCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(logicalCapacity));

            LogicalCapacity = logicalCapacity;
            int pageCount = GetPageCount(logicalCapacity);
            aRestPages = new int[pageCount][];
            vRestPages = new VRestPage[pageCount];
            bindingTokensByVictim = new int[logicalCapacity];
            collisionEligibilityStamp = new int[logicalCapacity];
            collisionTickScratch = new List<int>(logicalCapacity);
            activeVRestRowIndices = new int[logicalCapacity];
            Array.Fill(activeVRestRowIndices, -1);
            activeVRestVictimSlots = new List<int>(logicalCapacity);
        }

        public int LogicalCapacity { get; private set; }
        public int ARestEntryCount { get; private set; }
        public int VRestEntryCount { get; private set; }
        public int VRestRowCount { get; private set; }
        public int MaterializedARestPageCount { get; private set; }
        public int MaterializedVRestPageCount { get; private set; }
        public bool IsCapacitySealed => capacitySealed;
        public bool UsesDenseBattleStorage => denseVRestValues != null;
        public bool UsesPreallocatedSparseBattleStorage => sparseVRestTable != null;
        public int PreparedSparseVRestEntryCapacity =>
            sparseVRestTable?.EntryCapacity ?? 0;
        public long RejectedVRestWriteCount { get; private set; }

        internal bool TryCopyCanonicalStateTo(
            BattleWorldRestSnapshotBuffer destination,
            out int aRestEntryCount,
            out int vRestEntryCount,
            out int vRestRowCount)
        {
            aRestEntryCount = 0;
            vRestEntryCount = 0;
            vRestRowCount = 0;
            if (destination == null ||
                !preparedForBattle ||
                destination.LogicalCapacity != LogicalCapacity)
            {
                return false;
            }

            bool usesDense = denseVRestValues != null;
            if (usesDense !=
                (destination.StorageMode == BattleRestSnapshotStorageMode.Dense))
            {
                return false;
            }
            if (usesDense &&
                destination.DenseVRestValues.Length != denseVRestValues.Length)
            {
                return false;
            }
            if (!usesDense &&
                (sparseVRestTable == null ||
                 destination.SparseEntryCapacity < VRestEntryCount))
            {
                return false;
            }

            int[] destinationARest = destination.ARestValues;
            for (int attackerSlot = 0;
                 attackerSlot < LogicalCapacity;
                 attackerSlot++)
            {
                destinationARest[attackerSlot] = GetARest(attackerSlot);
            }

            if (usesDense)
            {
                Array.Copy(
                    denseVRestValues,
                    destination.DenseVRestValues,
                    denseVRestValues.Length);
            }
            else
            {
                int destinationIndex = 0;
                for (int victimSlot = 0;
                     victimSlot < LogicalCapacity;
                     victimSlot++)
                {
                    for (int node = sparseVRestTable.GetFirstNode(victimSlot);
                         node >= 0;
                         node = sparseVRestTable.GetNextNode(node))
                    {
                        destination.SparseVictimSlots[destinationIndex] = victimSlot;
                        destination.SparseAttackerSlots[destinationIndex] =
                            sparseVRestTable.GetAttackerSlot(node);
                        destination.SparseValues[destinationIndex] =
                            sparseVRestTable.GetValue(node);
                        destinationIndex++;
                    }
                }

                if (destinationIndex != VRestEntryCount)
                {
                    return false;
                }
            }

            aRestEntryCount = ARestEntryCount;
            vRestEntryCount = VRestEntryCount;
            vRestRowCount = VRestRowCount;
            return true;
        }

        internal bool TryRestoreCanonicalStateFrom(
            BattleWorldRestSnapshotBuffer source)
        {
            if (source == null ||
                source.SchemaVersion != BattleWorldRestSnapshotBuffer.CurrentSchemaVersion ||
                source.LogicalCapacity != LogicalCapacity ||
                !preparedForBattle ||
                (denseVRestValues != null) !=
                    (source.StorageMode == BattleRestSnapshotStorageMode.Dense) ||
                (denseVRestValues == null &&
                 (sparseVRestTable == null ||
                  source.VRestEntryCount > sparseVRestTable.EntryCapacity)))
            {
                return false;
            }

            ResetWorld();
            for (int slot = 0; slot < LogicalCapacity; slot++)
            {
                int value = source.GetARest(slot);
                if (value > 0 && !SetARest(slot, value))
                    return false;
            }

            if (denseVRestValues != null)
            {
                for (int victimSlot = 0; victimSlot < LogicalCapacity; victimSlot++)
                {
                    for (int attackerSlot = 0;
                         attackerSlot < LogicalCapacity;
                         attackerSlot++)
                    {
                        int value = source.GetVRest(victimSlot, attackerSlot);
                        if (value > 0 &&
                            !SetVRest(victimSlot, attackerSlot, value))
                        {
                            return false;
                        }
                    }
                }
            }
            else
            {
                for (int index = 0; index < source.VRestEntryCount; index++)
                {
                    if (!SetVRest(
                            source.SparseVictimSlots[index],
                            source.SparseAttackerSlots[index],
                            source.SparseValues[index]))
                    {
                        return false;
                    }
                }
            }

            return ARestEntryCount == source.ARestEntryCount &&
                   VRestEntryCount == source.VRestEntryCount &&
                   VRestRowCount == source.VRestRowCount;
        }

        internal void AppendDeterministicChecksum(ref BattleChecksum64Builder builder)
        {
            builder.AddInt32(LogicalCapacity);
            builder.AddInt32(ARestEntryCount);
            for (int attackerSlot = 0; attackerSlot < LogicalCapacity; attackerSlot++)
            {
                int value = GetARest(attackerSlot);
                if (value == 0)
                    continue;

                builder.AddInt32(attackerSlot);
                builder.AddInt32(value);
            }

            builder.AddInt32(VRestEntryCount);
            builder.AddInt32(VRestRowCount);
            ulong unorderedEntries = 0UL;
            for (int index = 0; index < activeVRestVictimSlots.Count; index++)
            {
                int victimSlot = activeVRestVictimSlots[index];
                if (denseVRestValues != null)
                {
                    int rowStart = victimSlot * LogicalCapacity;
                    for (int attackerSlot = 0; attackerSlot < LogicalCapacity; attackerSlot++)
                    {
                        int value = denseVRestValues[rowStart + attackerSlot];
                        if (value != 0)
                        {
                            unorderedEntries ^= MixRestEntry(
                                victimSlot,
                                attackerSlot,
                                value);
                        }
                    }

                    continue;
                }

                if (sparseVRestTable != null)
                {
                    for (int node = sparseVRestTable.GetFirstNode(victimSlot);
                         node >= 0;
                         node = sparseVRestTable.GetNextNode(node))
                    {
                        int value = sparseVRestTable.GetValue(node);
                        if (value == 0)
                            continue;

                        unorderedEntries ^= MixRestEntry(
                            victimSlot,
                            sparseVRestTable.GetAttackerSlot(node),
                            value);
                    }

                    continue;
                }

                VRestPage page = vRestPages[victimSlot / PageSize];
                Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
                if (row == null)
                    continue;

                foreach (KeyValuePair<int, int> pair in row)
                {
                    if (pair.Value != 0)
                    {
                        unorderedEntries ^= MixRestEntry(
                            victimSlot,
                            pair.Key,
                            pair.Value);
                    }
                }
            }

            builder.AddUInt64(unorderedEntries);
        }

        private static ulong MixRestEntry(int victimSlot, int attackerSlot, int value)
        {
            unchecked
            {
                ulong mixed = (uint)victimSlot;
                mixed = (mixed << 32) | (uint)attackerSlot;
                mixed ^= (ulong)(uint)value * 0x9e3779b185ebca87UL;
                mixed ^= mixed >> 30;
                mixed *= 0xbf58476d1ce4e5b9UL;
                mixed ^= mixed >> 27;
                mixed *= 0x94d049bb133111ebUL;
                mixed ^= mixed >> 31;
                return mixed;
            }
        }

        public void PrepareForBattle()
        {
            if (capacitySealed || preparedForBattle)
                return;

            for (int pageIndex = 0; pageIndex < aRestPages.Length; pageIndex++)
            {
                if (aRestPages[pageIndex] != null)
                    continue;

                aRestPages[pageIndex] = new int[PageSize];
                MaterializedARestPageCount++;
            }

            if (LogicalCapacity <= MaximumDenseBattleCapacity)
                PrepareDenseVRestStorage();
            else
                PrepareSparseVRestStorage(DefaultSparseEntryCapacity(LogicalCapacity));

            preparedForBattle = true;
        }

        public void SealCapacity()
        {
            PrepareForBattle();
            capacitySealed = true;
        }

        public void UnsealCapacity()
        {
            capacitySealed = false;
        }

        public bool TryAcquireBinding(int victimSlot, out RuntimeRestBindingHandle handle)
        {
            handle = default;
            if (!IsAddressable(victimSlot) || bindingTokensByVictim[victimSlot] != 0)
                return false;

            int token = nextBindingToken++;
            if (token == 0)
                token = nextBindingToken++;
            bindingTokensByVictim[victimSlot] = token;
            handle = new RuntimeRestBindingHandle(this, victimSlot, token);
            return true;
        }

        public bool IsBindingValid(RuntimeRestBindingHandle handle)
        {
            return handle.Owner == this &&
                   IsAddressable(handle.BoundVictimSlot) &&
                   bindingTokensByVictim[handle.BoundVictimSlot] == handle.Token;
        }

        public bool ReleaseBinding(RuntimeRestBindingHandle handle)
        {
            if (!IsBindingValid(handle))
                return false;

            bindingTokensByVictim[handle.BoundVictimSlot] = 0;
            return true;
        }

        public bool IsAddressable(int slot)
        {
            return slot >= 0 && slot < LogicalCapacity;
        }

        public int GetARest(int attackerSlot)
        {
            if (!IsAddressable(attackerSlot))
                return 0;

            int[] page = aRestPages[attackerSlot / PageSize];
            return page == null ? 0 : page[attackerSlot % PageSize];
        }

        public bool SetARest(int attackerSlot, int value)
        {
            if (!IsAddressable(attackerSlot))
                return false;

            int pageIndex = attackerSlot / PageSize;
            int entryIndex = attackerSlot % PageSize;
            int[] page = aRestPages[pageIndex];
            int oldValue = page == null ? 0 : page[entryIndex];
            int storedValue = Math.Max(0, value);
            if (oldValue == storedValue)
                return true;

            if (page == null)
            {
                page = new int[PageSize];
                aRestPages[pageIndex] = page;
                MaterializedARestPageCount++;
            }

            page[entryIndex] = storedValue;
            if (oldValue == 0)
                ARestEntryCount++;
            else if (storedValue == 0)
                ARestEntryCount--;

            return true;
        }

        public int GetVRest(int victimSlot, int attackerSlot)
        {
            if (!IsAddressable(victimSlot) || !IsAddressable(attackerSlot))
                return 0;

            if (denseVRestValues != null)
                return denseVRestValues[DenseVRestIndex(victimSlot, attackerSlot)];

            if (sparseVRestTable != null)
            {
                return sparseVRestTable.TryGet(victimSlot, attackerSlot, out int sparseValue)
                    ? sparseValue
                    : 0;
            }

            VRestPage page = vRestPages[victimSlot / PageSize];
            Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
            return row != null && row.TryGetValue(attackerSlot, out int value) ? value : 0;
        }

        public bool SetVRest(int victimSlot, int attackerSlot, int value)
        {
            if (!IsAddressable(victimSlot) || !IsAddressable(attackerSlot))
                return false;

            if (denseVRestValues != null)
                return SetDenseVRest(victimSlot, attackerSlot, value);

            if (sparseVRestTable != null)
            {
                bool rowWasActive = sparseVRestTable.HasRow(victimSlot);
                if (!sparseVRestTable.TrySet(
                        victimSlot,
                        attackerSlot,
                        Math.Max(0, value),
                        out bool added,
                        out bool removed))
                {
                    RejectedVRestWriteCount++;
                    return false;
                }

                if (added)
                    VRestEntryCount++;
                else if (removed)
                    VRestEntryCount--;

                bool rowIsActive = sparseVRestTable.HasRow(victimSlot);
                if (!rowWasActive && rowIsActive)
                {
                    VRestRowCount++;
                    AddActiveVRestRow(victimSlot);
                }
                else if (rowWasActive && !rowIsActive)
                {
                    VRestRowCount--;
                    RemoveActiveVRestRow(victimSlot);
                }

                return true;
            }

            int pageIndex = victimSlot / PageSize;
            int rowIndex = victimSlot % PageSize;
            VRestPage page = vRestPages[pageIndex];
            Dictionary<int, int> row = page?.Rows[rowIndex];

            if (value <= 0)
            {
                if (row == null || !row.Remove(attackerSlot))
                    return true;

                VRestEntryCount--;
                if (row.Count == 0)
                {
                    page.Rows[rowIndex] = null;
                    VRestRowCount--;
                    RemoveActiveVRestRow(victimSlot);
                }

                return true;
            }

            if (page == null)
            {
                if (capacitySealed)
                {
                    RejectedVRestWriteCount++;
                    return false;
                }

                page = new VRestPage();
                vRestPages[pageIndex] = page;
                MaterializedVRestPageCount++;
            }

            if (row == null)
            {
                if (capacitySealed)
                {
                    RejectedVRestWriteCount++;
                    return false;
                }

                row = new Dictionary<int, int>();
                page.Rows[rowIndex] = row;
                VRestRowCount++;
                AddActiveVRestRow(victimSlot);
            }

            bool addsEntry = !row.ContainsKey(attackerSlot);
            if (addsEntry && capacitySealed)
            {
                RejectedVRestWriteCount++;
                return false;
            }

            if (addsEntry)
                VRestEntryCount++;

            row[attackerSlot] = value;
            return true;
        }

        public bool ResetSlot(int slot)
        {
            if (!IsAddressable(slot))
                return false;

            InvalidateBinding(slot);
            SetARest(slot, 0);
            ClearVictimRowOnly(slot);

            if (denseVRestValues != null)
            {
                for (int victimSlot = 0; victimSlot < LogicalCapacity; victimSlot++)
                    SetDenseVRest(victimSlot, slot, 0);
                return true;
            }

            if (sparseVRestTable != null)
            {
                int activeRowIndex = 0;
                while (activeRowIndex < activeVRestVictimSlots.Count)
                {
                    int victimSlot = activeVRestVictimSlots[activeRowIndex];
                    SetVRest(victimSlot, slot, 0);
                    if (activeRowIndex < activeVRestVictimSlots.Count &&
                        activeVRestVictimSlots[activeRowIndex] == victimSlot)
                    {
                        activeRowIndex++;
                    }
                }

                return true;
            }

            for (int pageIndex = 0; pageIndex < vRestPages.Length; pageIndex++)
            {
                VRestPage page = vRestPages[pageIndex];
                if (page == null)
                    continue;

                for (int rowIndex = 0; rowIndex < page.Rows.Length; rowIndex++)
                {
                    Dictionary<int, int> row = page.Rows[rowIndex];
                    if (row == null || !row.Remove(slot))
                        continue;

                    VRestEntryCount--;
                    if (row.Count == 0)
                    {
                        page.Rows[rowIndex] = null;
                        VRestRowCount--;
                        RemoveActiveVRestRow(pageIndex * PageSize + rowIndex);
                    }
                }
            }

            return true;
        }

        public void ResetWorld()
        {
            Array.Clear(bindingTokensByVictim, 0, bindingTokensByVictim.Length);
            if (preparedForBattle)
            {
                for (int pageIndex = 0; pageIndex < aRestPages.Length; pageIndex++)
                {
                    int[] page = aRestPages[pageIndex];
                    if (page != null)
                        Array.Clear(page, 0, page.Length);
                }

                if (denseVRestValues != null)
                {
                    Array.Clear(denseVRestValues, 0, denseVRestValues.Length);
                    Array.Clear(denseVRestRowEntryCounts, 0, denseVRestRowEntryCounts.Length);
                }
                else if (sparseVRestTable != null)
                {
                    sparseVRestTable.ClearAll();
                }
                else
                {
                    ClearSparseVRestStorageRetainingRows();
                }
            }
            else
            {
                Array.Clear(aRestPages, 0, aRestPages.Length);
                Array.Clear(vRestPages, 0, vRestPages.Length);
                MaterializedARestPageCount = 0;
                MaterializedVRestPageCount = 0;
            }
            ARestEntryCount = 0;
            VRestEntryCount = 0;
            VRestRowCount = 0;
            activeVRestVictimSlots.Clear();
            Array.Fill(activeVRestRowIndices, -1);
        }

        public bool GrowTo(int newLogicalCapacity)
        {
            if (newLogicalCapacity < LogicalCapacity)
                return false;
            if (newLogicalCapacity == LogicalCapacity)
                return true;
            if (capacitySealed || denseVRestValues != null)
                return false;

            int newPageCount = GetPageCount(newLogicalCapacity);
            if (newPageCount != aRestPages.Length)
            {
                var grownARestPages = new int[newPageCount][];
                var grownVRestPages = new VRestPage[newPageCount];
                Array.Copy(aRestPages, grownARestPages, aRestPages.Length);
                Array.Copy(vRestPages, grownVRestPages, vRestPages.Length);
                aRestPages = grownARestPages;
                vRestPages = grownVRestPages;
            }

            var grownEligibilityStamp = new int[newLogicalCapacity];
            Array.Copy(
                collisionEligibilityStamp,
                grownEligibilityStamp,
                collisionEligibilityStamp.Length);
            collisionEligibilityStamp = grownEligibilityStamp;
            if (collisionTickScratch.Capacity < newLogicalCapacity)
                collisionTickScratch.Capacity = newLogicalCapacity;
            var grownActiveRowIndices = new int[newLogicalCapacity];
            Array.Fill(grownActiveRowIndices, -1);
            Array.Copy(activeVRestRowIndices, grownActiveRowIndices, activeVRestRowIndices.Length);
            activeVRestRowIndices = grownActiveRowIndices;
            if (activeVRestVictimSlots.Capacity < newLogicalCapacity)
                activeVRestVictimSlots.Capacity = newLogicalCapacity;

            var grownBindingTokens = new int[newLogicalCapacity];
            Array.Copy(bindingTokensByVictim, grownBindingTokens, bindingTokensByVictim.Length);
            bindingTokensByVictim = grownBindingTokens;

            sparseVRestTable?.Grow(
                newLogicalCapacity,
                DefaultSparseEntryCapacity(newLogicalCapacity));

            LogicalCapacity = newLogicalCapacity;
            return true;
        }

        public DiagnosticSnapshot CaptureDiagnosticSnapshot()
        {
            var aRestEntries = new List<ARestEntry>(ARestEntryCount);
            var vRestEntries = new List<VRestEntry>(VRestEntryCount);

            for (int slot = 0; slot < LogicalCapacity; slot++)
            {
                int value = GetARest(slot);
                if (value > 0)
                    aRestEntries.Add(new ARestEntry(slot, value));
            }

            for (int victimSlot = 0; victimSlot < LogicalCapacity; victimSlot++)
            {
                if (denseVRestValues != null)
                {
                    for (int attackerSlot = 0; attackerSlot < LogicalCapacity; attackerSlot++)
                    {
                        int value = denseVRestValues[DenseVRestIndex(victimSlot, attackerSlot)];
                        if (value > 0)
                            vRestEntries.Add(new VRestEntry(victimSlot, attackerSlot, value));
                    }

                    continue;
                }

                if (sparseVRestTable != null)
                {
                    for (int node = sparseVRestTable.GetFirstNode(victimSlot);
                         node >= 0;
                         node = sparseVRestTable.GetNextNode(node))
                    {
                        vRestEntries.Add(new VRestEntry(
                            victimSlot,
                            sparseVRestTable.GetAttackerSlot(node),
                            sparseVRestTable.GetValue(node)));
                    }

                    continue;
                }

                VRestPage page = vRestPages[victimSlot / PageSize];
                Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
                if (row == null)
                    continue;

                var attackerSlots = new List<int>(row.Keys);
                attackerSlots.Sort();
                for (int i = 0; i < attackerSlots.Count; i++)
                {
                    int attackerSlot = attackerSlots[i];
                    vRestEntries.Add(new VRestEntry(victimSlot, attackerSlot, row[attackerSlot]));
                }
            }

            return new DiagnosticSnapshot(
                LogicalCapacity,
                aRestEntries.ToArray(),
                vRestEntries.ToArray());
        }

        /// <summary>
        /// Captures the canonical sparse rest state for checksum/replay consumers.
        /// This intentionally contains no dense victim-by-attacker matrix.
        /// </summary>
        public DiagnosticSnapshot CaptureSparseSnapshot()
        {
            return CaptureDiagnosticSnapshot();
        }

        public bool RestoreDiagnosticSnapshot(DiagnosticSnapshot snapshot)
        {
            if (snapshot == null || snapshot.LogicalCapacity <= 0)
                return false;
            if (snapshot.LogicalCapacity > LogicalCapacity && !GrowTo(snapshot.LogicalCapacity))
                return false;

            ResetWorld();
            for (int i = 0; i < snapshot.ARestEntries.Count; i++)
            {
                ARestEntry entry = snapshot.ARestEntries[i];
                if (!SetARest(entry.AttackerSlot, entry.Value))
                    return false;
            }

            for (int i = 0; i < snapshot.VRestEntries.Count; i++)
            {
                VRestEntry entry = snapshot.VRestEntries[i];
                if (!SetVRest(entry.VictimSlot, entry.AttackerSlot, entry.Value))
                    return false;
            }

            return true;
        }

        public bool TickARest(int attackerSlot)
        {
            if (!IsAddressable(attackerSlot))
                return false;

            int value = GetARest(attackerSlot);
            return SetARest(attackerSlot, value > 0 ? value - 1 : 0);
        }

        public bool TickVRestForAttacker(int victimSlot, int attackerSlot)
        {
            if (!IsAddressable(victimSlot) || !IsAddressable(attackerSlot))
                return false;

            int value = GetVRest(victimSlot, attackerSlot);
            return SetVRest(victimSlot, attackerSlot, value > 0 ? value - 1 : 0);
        }

        public bool TickVictim(int victimSlot)
        {
            if (!IsAddressable(victimSlot))
                return false;

            TickARest(victimSlot);
            if (denseVRestValues != null)
            {
                int rowStart = victimSlot * LogicalCapacity;
                for (int attackerSlot = 0; attackerSlot < LogicalCapacity; attackerSlot++)
                {
                    int value = denseVRestValues[rowStart + attackerSlot];
                    if (value > 0)
                        SetDenseVRest(victimSlot, attackerSlot, value - 1);
                }

                return true;
            }

            if (sparseVRestTable != null)
            {
                int node = sparseVRestTable.GetFirstNode(victimSlot);
                while (node >= 0)
                {
                    int next = sparseVRestTable.GetNextNode(node);
                    int attackerSlot = sparseVRestTable.GetAttackerSlot(node);
                    int value = sparseVRestTable.GetValue(node);
                    if (value > 0)
                        SetVRest(victimSlot, attackerSlot, value - 1);
                    node = next;
                }

                return true;
            }

            VRestPage page = vRestPages[victimSlot / PageSize];
            Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
            if (row == null || row.Count == 0)
                return true;

            var attackers = new List<int>(row.Keys);
            for (int i = 0; i < attackers.Count; i++)
                TickVRestForAttacker(victimSlot, attackers[i]);
            return true;
        }

        public bool TickCollisionPairVRest(IReadOnlyList<int> eligibleSlots)
        {
            if (eligibleSlots == null)
                return false;

            BeginCollisionPairVRestEligibility();
            for (int i = 0; i < eligibleSlots.Count; i++)
                MarkCollisionPairVRestEligible(eligibleSlots[i]);

            TickMarkedCollisionPairVRest();
            return true;
        }

        internal void BeginCollisionPairVRestEligibility()
        {
            AdvanceCollisionEligibilityEpoch();
        }

        internal bool MarkCollisionPairVRestEligible(int slot)
        {
            if (!IsAddressable(slot))
                return false;
            collisionEligibilityStamp[slot] = collisionEligibilityEpoch;
            return true;
        }

        internal void TickMarkedCollisionPairVRest()
        {
            int activeRowIndex = 0;
            while (activeRowIndex < activeVRestVictimSlots.Count)
            {
                int victimSlot = activeVRestVictimSlots[activeRowIndex];
                if (collisionEligibilityStamp[victimSlot] != collisionEligibilityEpoch)
                {
                    activeRowIndex++;
                    continue;
                }

                VRestPage page = vRestPages[victimSlot / PageSize];
                Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
                if (denseVRestValues != null)
                {
                    int rowStart = victimSlot * LogicalCapacity;
                    for (int attackerSlot = 0; attackerSlot < LogicalCapacity; attackerSlot++)
                    {
                        if (attackerSlot == victimSlot ||
                            collisionEligibilityStamp[attackerSlot] != collisionEligibilityEpoch)
                        {
                            continue;
                        }

                        int value = denseVRestValues[rowStart + attackerSlot];
                        if (value > 0)
                            SetDenseVRest(victimSlot, attackerSlot, value - 1);
                    }

                    if (activeRowIndex < activeVRestVictimSlots.Count &&
                        activeVRestVictimSlots[activeRowIndex] == victimSlot)
                    {
                        activeRowIndex++;
                    }

                    continue;
                }

                if (sparseVRestTable != null)
                {
                    int node = sparseVRestTable.GetFirstNode(victimSlot);
                    while (node >= 0)
                    {
                        int next = sparseVRestTable.GetNextNode(node);
                        int attackerSlot = sparseVRestTable.GetAttackerSlot(node);
                        if (attackerSlot != victimSlot &&
                            collisionEligibilityStamp[attackerSlot] ==
                            collisionEligibilityEpoch)
                        {
                            int value = sparseVRestTable.GetValue(node);
                            if (value > 0)
                                SetVRest(victimSlot, attackerSlot, value - 1);
                        }

                        node = next;
                    }

                    if (activeRowIndex < activeVRestVictimSlots.Count &&
                        activeVRestVictimSlots[activeRowIndex] == victimSlot)
                    {
                        activeRowIndex++;
                    }

                    continue;
                }

                if (row == null || row.Count == 0)
                {
                    RemoveActiveVRestRow(victimSlot);
                    continue;
                }

                collisionTickScratch.Clear();
                foreach (KeyValuePair<int, int> pair in row)
                {
                    int attackerSlot = pair.Key;
                    if (attackerSlot != victimSlot &&
                        collisionEligibilityStamp[attackerSlot] == collisionEligibilityEpoch)
                    {
                        collisionTickScratch.Add(attackerSlot);
                    }
                }

                for (int i = 0; i < collisionTickScratch.Count; i++)
                {
                    int attackerSlot = collisionTickScratch[i];
                    int value = GetVRest(victimSlot, attackerSlot);
                    SetVRest(victimSlot, attackerSlot, value - 1);
                }

                if (activeRowIndex < activeVRestVictimSlots.Count &&
                    activeVRestVictimSlots[activeRowIndex] == victimSlot)
                {
                    activeRowIndex++;
                }
            }

            collisionTickScratch.Clear();
        }

        public bool ClearVictimRowOnly(int victimSlot)
        {
            if (!IsAddressable(victimSlot))
                return false;
            ClearVictimRowOnlyUnchecked(victimSlot);
            return true;
        }

        public bool ReplaceVictimState(int victimSlot, int arest, IReadOnlyDictionary<int, int> vrestByAttacker)
        {
            if (!IsAddressable(victimSlot))
                return false;

            if (vrestByAttacker != null)
            {
                foreach (KeyValuePair<int, int> pair in vrestByAttacker)
                {
                    if (pair.Value <= 0)
                        continue;
                    if (!IsAddressable(pair.Key))
                        return false;
                }
            }

            SetARest(victimSlot, arest);
            ClearVictimRowOnlyUnchecked(victimSlot);
            if (vrestByAttacker == null)
                return true;

            foreach (KeyValuePair<int, int> pair in vrestByAttacker)
            {
                if (pair.Value > 0 && !SetVRest(victimSlot, pair.Key, pair.Value))
                    return false;
            }
            return true;
        }

        public Dictionary<int, int> CaptureVictimRow(int victimSlot)
        {
            var values = new Dictionary<int, int>();
            if (!IsAddressable(victimSlot))
                return values;

            if (denseVRestValues != null)
            {
                int rowStart = victimSlot * LogicalCapacity;
                for (int attackerSlot = 0; attackerSlot < LogicalCapacity; attackerSlot++)
                {
                    int value = denseVRestValues[rowStart + attackerSlot];
                    if (value > 0)
                        values[attackerSlot] = value;
                }

                return values;
            }

            if (sparseVRestTable != null)
            {
                for (int node = sparseVRestTable.GetFirstNode(victimSlot);
                     node >= 0;
                     node = sparseVRestTable.GetNextNode(node))
                {
                    values[sparseVRestTable.GetAttackerSlot(node)] =
                        sparseVRestTable.GetValue(node);
                }

                return values;
            }

            VRestPage page = vRestPages[victimSlot / PageSize];
            Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
            if (row == null)
                return values;
            foreach (KeyValuePair<int, int> pair in row)
                values[pair.Key] = pair.Value;
            return values;
        }

        private void ClearVictimRowOnlyUnchecked(int victimSlot)
        {
            if (denseVRestValues != null)
            {
                int rowStart = victimSlot * LogicalCapacity;
                int removedCount = denseVRestRowEntryCounts[victimSlot];
                if (removedCount == 0)
                    return;

                Array.Clear(denseVRestValues, rowStart, LogicalCapacity);
                denseVRestRowEntryCounts[victimSlot] = 0;
                VRestEntryCount -= removedCount;
                VRestRowCount--;
                RemoveActiveVRestRow(victimSlot);
                return;
            }

            if (sparseVRestTable != null)
            {
                int removedCount = sparseVRestTable.ClearRow(victimSlot);
                if (removedCount == 0)
                    return;

                VRestEntryCount -= removedCount;
                VRestRowCount--;
                RemoveActiveVRestRow(victimSlot);
                return;
            }

            VRestPage page = vRestPages[victimSlot / PageSize];
            int rowIndex = victimSlot % PageSize;
            Dictionary<int, int> row = page?.Rows[rowIndex];
            if (row == null)
                return;

            VRestEntryCount -= row.Count;
            VRestRowCount--;
            page.Rows[rowIndex] = null;
            RemoveActiveVRestRow(victimSlot);
        }

        private void InvalidateBinding(int victimSlot)
        {
            bindingTokensByVictim[victimSlot] = 0;
        }

        private void AddActiveVRestRow(int victimSlot)
        {
            if (activeVRestRowIndices[victimSlot] >= 0)
                return;
            activeVRestRowIndices[victimSlot] = activeVRestVictimSlots.Count;
            activeVRestVictimSlots.Add(victimSlot);
        }

        private void RemoveActiveVRestRow(int victimSlot)
        {
            int index = activeVRestRowIndices[victimSlot];
            if (index < 0)
                return;

            int lastIndex = activeVRestVictimSlots.Count - 1;
            int movedVictimSlot = activeVRestVictimSlots[lastIndex];
            activeVRestVictimSlots[index] = movedVictimSlot;
            activeVRestRowIndices[movedVictimSlot] = index;
            activeVRestVictimSlots.RemoveAt(lastIndex);
            activeVRestRowIndices[victimSlot] = -1;
        }

        private void PrepareDenseVRestStorage()
        {
            int denseLength = checked(LogicalCapacity * LogicalCapacity);
            var values = new int[denseLength];
            var rowEntryCounts = new int[LogicalCapacity];

            for (int victimSlot = 0; victimSlot < LogicalCapacity; victimSlot++)
            {
                VRestPage page = vRestPages[victimSlot / PageSize];
                Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
                if (row == null)
                    continue;

                int rowStart = victimSlot * LogicalCapacity;
                foreach (KeyValuePair<int, int> pair in row)
                {
                    if (pair.Value <= 0)
                        continue;

                    values[rowStart + pair.Key] = pair.Value;
                    rowEntryCounts[victimSlot]++;
                }
            }

            denseVRestValues = values;
            denseVRestRowEntryCounts = rowEntryCounts;
            Array.Clear(vRestPages, 0, vRestPages.Length);
            MaterializedVRestPageCount = 0;
        }

        private void PrepareSparseVRestStorage(int entryCapacity)
        {
            var prepared = new SparseVRestTable(LogicalCapacity, entryCapacity);
            for (int victimSlot = 0; victimSlot < LogicalCapacity; victimSlot++)
            {
                VRestPage page = vRestPages[victimSlot / PageSize];
                Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
                if (row == null)
                    continue;

                foreach (KeyValuePair<int, int> pair in row)
                {
                    if (pair.Value <= 0)
                        continue;
                    if (!prepared.TrySet(
                            victimSlot,
                            pair.Key,
                            pair.Value,
                            out _,
                            out _))
                    {
                        throw new InvalidOperationException(
                            "Prepared sparse VRest capacity is smaller than the existing state.");
                    }
                }
            }

            sparseVRestTable = prepared;
            Array.Clear(vRestPages, 0, vRestPages.Length);
            MaterializedVRestPageCount = 0;
        }

        private static int DefaultSparseEntryCapacity(int logicalCapacity)
        {
            return Math.Max(
                MinimumSparseEntryCapacity,
                checked(logicalCapacity * DefaultSparseEntriesPerRuntimeSlot));
        }

        private int DenseVRestIndex(int victimSlot, int attackerSlot)
        {
            return victimSlot * LogicalCapacity + attackerSlot;
        }

        private bool SetDenseVRest(int victimSlot, int attackerSlot, int value)
        {
            int index = DenseVRestIndex(victimSlot, attackerSlot);
            int oldValue = denseVRestValues[index];
            int storedValue = Math.Max(0, value);
            if (oldValue == storedValue)
                return true;

            denseVRestValues[index] = storedValue;
            if (oldValue == 0)
            {
                VRestEntryCount++;
                if (denseVRestRowEntryCounts[victimSlot]++ == 0)
                {
                    VRestRowCount++;
                    AddActiveVRestRow(victimSlot);
                }
            }
            else if (storedValue == 0)
            {
                VRestEntryCount--;
                if (--denseVRestRowEntryCounts[victimSlot] == 0)
                {
                    VRestRowCount--;
                    RemoveActiveVRestRow(victimSlot);
                }
            }

            return true;
        }

        private void ClearSparseVRestStorageRetainingRows()
        {
            for (int pageIndex = 0; pageIndex < vRestPages.Length; pageIndex++)
            {
                VRestPage page = vRestPages[pageIndex];
                if (page == null)
                    continue;

                for (int rowIndex = 0; rowIndex < page.Rows.Length; rowIndex++)
                    page.Rows[rowIndex]?.Clear();
            }
        }

        private void AdvanceCollisionEligibilityEpoch()
        {
            if (collisionEligibilityEpoch == int.MaxValue)
            {
                Array.Clear(collisionEligibilityStamp, 0, collisionEligibilityStamp.Length);
                collisionEligibilityEpoch = 1;
                return;
            }

            collisionEligibilityEpoch++;
            if (collisionEligibilityEpoch == 0)
                collisionEligibilityEpoch = 1;
        }

        private static int GetPageCount(int logicalCapacity)
        {
            return (logicalCapacity + PageSize - 1) / PageSize;
        }
    }
}
