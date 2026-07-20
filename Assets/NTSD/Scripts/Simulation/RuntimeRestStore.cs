using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
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

        private int[][] aRestPages;
        private VRestPage[] vRestPages;

        public RuntimeRestStore(int logicalCapacity)
        {
            if (logicalCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(logicalCapacity));

            LogicalCapacity = logicalCapacity;
            int pageCount = GetPageCount(logicalCapacity);
            aRestPages = new int[pageCount][];
            vRestPages = new VRestPage[pageCount];
        }

        public int LogicalCapacity { get; private set; }
        public int ARestEntryCount { get; private set; }
        public int VRestEntryCount { get; private set; }
        public int VRestRowCount { get; private set; }
        public int MaterializedARestPageCount { get; private set; }
        public int MaterializedVRestPageCount { get; private set; }

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

            VRestPage page = vRestPages[victimSlot / PageSize];
            Dictionary<int, int> row = page?.Rows[victimSlot % PageSize];
            return row != null && row.TryGetValue(attackerSlot, out int value) ? value : 0;
        }

        public bool SetVRest(int victimSlot, int attackerSlot, int value)
        {
            if (!IsAddressable(victimSlot) || !IsAddressable(attackerSlot))
                return false;

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
                }

                return true;
            }

            if (page == null)
            {
                page = new VRestPage();
                vRestPages[pageIndex] = page;
                MaterializedVRestPageCount++;
            }

            if (row == null)
            {
                row = new Dictionary<int, int>();
                page.Rows[rowIndex] = row;
                VRestRowCount++;
            }

            if (!row.ContainsKey(attackerSlot))
                VRestEntryCount++;

            row[attackerSlot] = value;
            return true;
        }

        public bool ResetSlot(int slot)
        {
            if (!IsAddressable(slot))
                return false;

            SetARest(slot, 0);
            ClearVictimRow(slot);

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
                    }
                }
            }

            return true;
        }

        public void ResetWorld()
        {
            Array.Clear(aRestPages, 0, aRestPages.Length);
            Array.Clear(vRestPages, 0, vRestPages.Length);
            ARestEntryCount = 0;
            VRestEntryCount = 0;
            VRestRowCount = 0;
            MaterializedARestPageCount = 0;
            MaterializedVRestPageCount = 0;
        }

        public bool GrowTo(int newLogicalCapacity)
        {
            if (newLogicalCapacity < LogicalCapacity)
                return false;
            if (newLogicalCapacity == LogicalCapacity)
                return true;

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

        private void ClearVictimRow(int victimSlot)
        {
            VRestPage page = vRestPages[victimSlot / PageSize];
            int rowIndex = victimSlot % PageSize;
            Dictionary<int, int> row = page?.Rows[rowIndex];
            if (row == null)
                return;

            VRestEntryCount -= row.Count;
            VRestRowCount--;
            page.Rows[rowIndex] = null;
        }

        private static int GetPageCount(int logicalCapacity)
        {
            return (logicalCapacity + PageSize - 1) / PageSize;
        }
    }
}
