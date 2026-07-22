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

        private int[][] aRestPages;
        private VRestPage[] vRestPages;
        private readonly Dictionary<int, int> bindingTokensByVictim = new Dictionary<int, int>();
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

        public bool TryAcquireBinding(int victimSlot, out RuntimeRestBindingHandle handle)
        {
            handle = default;
            if (!IsAddressable(victimSlot) || bindingTokensByVictim.ContainsKey(victimSlot))
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
                   bindingTokensByVictim.TryGetValue(handle.BoundVictimSlot, out int token) &&
                   token == handle.Token;
        }

        public bool ReleaseBinding(RuntimeRestBindingHandle handle)
        {
            if (!IsBindingValid(handle))
                return false;

            bindingTokensByVictim.Remove(handle.BoundVictimSlot);
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
                    RemoveActiveVRestRow(victimSlot);
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
                AddActiveVRestRow(victimSlot);
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

            InvalidateBinding(slot);
            SetARest(slot, 0);
            ClearVictimRowOnly(slot);

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
            bindingTokensByVictim.Clear();
            Array.Clear(aRestPages, 0, aRestPages.Length);
            Array.Clear(vRestPages, 0, vRestPages.Length);
            ARestEntryCount = 0;
            VRestEntryCount = 0;
            VRestRowCount = 0;
            MaterializedARestPageCount = 0;
            MaterializedVRestPageCount = 0;
            activeVRestVictimSlots.Clear();
            Array.Fill(activeVRestRowIndices, -1);
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

            Dictionary<int, int> normalizedVrest = null;
            if (vrestByAttacker != null)
            {
                normalizedVrest = new Dictionary<int, int>(vrestByAttacker.Count);
                foreach (KeyValuePair<int, int> pair in vrestByAttacker)
                {
                    if (pair.Value <= 0)
                        continue;
                    if (!IsAddressable(pair.Key))
                        return false;
                    normalizedVrest[pair.Key] = pair.Value;
                }
            }

            SetARest(victimSlot, arest);
            ClearVictimRowOnlyUnchecked(victimSlot);
            if (normalizedVrest == null)
                return true;

            foreach (KeyValuePair<int, int> pair in normalizedVrest)
                SetVRest(victimSlot, pair.Key, pair.Value);
            return true;
        }

        public Dictionary<int, int> CaptureVictimRow(int victimSlot)
        {
            var values = new Dictionary<int, int>();
            if (!IsAddressable(victimSlot))
                return values;

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
            bindingTokensByVictim.Remove(victimSlot);
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
