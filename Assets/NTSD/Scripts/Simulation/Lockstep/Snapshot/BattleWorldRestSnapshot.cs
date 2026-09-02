using System;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public enum BattleRestSnapshotStorageMode : byte
    {
        Dense = 0,
        Sparse = 1,
    }

    /// <summary>
    /// Preallocated capture buffer for directional aRest/vRest truth. It mirrors
    /// the prepared runtime store: dense worlds copy one contiguous matrix while
    /// large desktop worlds copy deterministic sparse triples.
    /// </summary>
    public sealed class BattleWorldRestSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly int[] aRestValues;
        private readonly int[] denseVRestValues;
        private readonly int[] sparseVictimSlots;
        private readonly int[] sparseAttackerSlots;
        private readonly int[] sparseValues;

        internal BattleWorldRestSnapshotBuffer(
            int logicalCapacity,
            BattleRestSnapshotStorageMode storageMode,
            int sparseEntryCapacity)
        {
            if (logicalCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(logicalCapacity));
            }
            if (sparseEntryCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sparseEntryCapacity));
            }

            LogicalCapacity = logicalCapacity;
            StorageMode = storageMode;
            aRestValues = new int[logicalCapacity];
            if (storageMode == BattleRestSnapshotStorageMode.Dense)
            {
                denseVRestValues = new int[
                    checked(logicalCapacity * logicalCapacity)];
                SparseEntryCapacity = 0;
                sparseVictimSlots = Array.Empty<int>();
                sparseAttackerSlots = Array.Empty<int>();
                sparseValues = Array.Empty<int>();
            }
            else
            {
                denseVRestValues = Array.Empty<int>();
                SparseEntryCapacity = sparseEntryCapacity;
                sparseVictimSlots = new int[sparseEntryCapacity];
                sparseAttackerSlots = new int[sparseEntryCapacity];
                sparseValues = new int[sparseEntryCapacity];
            }
        }

        public int LogicalCapacity { get; }
        public BattleRestSnapshotStorageMode StorageMode { get; }
        public int SparseEntryCapacity { get; }
        public int ARestEntryCount { get; private set; }
        public int VRestEntryCount { get; private set; }
        public int VRestRowCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public int GetARest(int attackerSlot)
        {
            ValidateSlot(attackerSlot, nameof(attackerSlot));
            return aRestValues[attackerSlot];
        }

        public int GetVRest(int victimSlot, int attackerSlot)
        {
            ValidateSlot(victimSlot, nameof(victimSlot));
            ValidateSlot(attackerSlot, nameof(attackerSlot));
            if (StorageMode == BattleRestSnapshotStorageMode.Dense)
            {
                return denseVRestValues[
                    victimSlot * LogicalCapacity + attackerSlot];
            }

            for (int index = 0; index < VRestEntryCount; index++)
            {
                int storedVictim = sparseVictimSlots[index];
                if (storedVictim > victimSlot)
                {
                    break;
                }
                if (storedVictim == victimSlot &&
                    sparseAttackerSlots[index] == attackerSlot)
                {
                    return sparseValues[index];
                }
            }

            return 0;
        }

        internal int[] ARestValues => aRestValues;
        internal int[] DenseVRestValues => denseVRestValues;
        internal int[] SparseVictimSlots => sparseVictimSlots;
        internal int[] SparseAttackerSlots => sparseAttackerSlots;
        internal int[] SparseValues => sparseValues;

        internal void CommitCapture(
            LockstepSessionIdentity identity,
            int tick,
            int aRestEntryCount,
            int vRestEntryCount,
            int vRestRowCount)
        {
            ARestEntryCount = aRestEntryCount;
            VRestEntryCount = vRestEntryCount;
            VRestRowCount = vRestRowCount;
            SchemaVersion = CurrentSchemaVersion;
            ProtocolSchemaVersion = identity.SchemaVersion;
            IdentityFingerprint = identity.IdentityFingerprint;
            CapturedTick = tick;
        }

        private void ValidateSlot(int slot, string parameterName)
        {
            if ((uint)slot >= (uint)LogicalCapacity)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    internal sealed class BattleWorldRestSnapshotModule
    {
        private readonly RuntimeRestStore store;

        internal BattleWorldRestSnapshotModule(RuntimeRestStore store)
        {
            this.store = store ?? throw new ArgumentNullException(nameof(store));
        }

        internal BattleWorldRestSnapshotBuffer CreateBufferForBootstrap()
        {
            store.PrepareForBattle();
            return new BattleWorldRestSnapshotBuffer(
                store.LogicalCapacity,
                store.UsesDenseBattleStorage
                    ? BattleRestSnapshotStorageMode.Dense
                    : BattleRestSnapshotStorageMode.Sparse,
                store.UsesDenseBattleStorage
                    ? 0
                    : store.PreparedSparseVRestEntryCapacity);
        }

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldRestSnapshotBuffer destination)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (!TryCopyCanonicalState(store, destination))
            {
                return false;
            }

            destination.CommitCapture(
                identity,
                tick,
                store.ARestEntryCount,
                store.VRestEntryCount,
                store.VRestRowCount);
            return true;
        }

        internal static bool TryRestore(
            RuntimeRestStore target,
            BattleWorldRestSnapshotBuffer source)
        {
            if (target == null ||
                source == null ||
                source.SchemaVersion != BattleWorldRestSnapshotBuffer.CurrentSchemaVersion ||
                source.LogicalCapacity != target.LogicalCapacity ||
                !target.IsPreparedForBattle ||
                target.UsesDenseBattleStorage !=
                    (source.StorageMode == BattleRestSnapshotStorageMode.Dense) ||
                (!target.UsesDenseBattleStorage &&
                 (!target.UsesPreallocatedSparseBattleStorage ||
                  source.VRestEntryCount >
                      target.PreparedSparseVRestEntryCapacity)))
            {
                return false;
            }

            target.ResetWorld();
            for (int slot = 0; slot < target.LogicalCapacity; slot++)
            {
                int value = source.GetARest(slot);
                if (value > 0 && !target.SetARest(slot, value))
                    return false;
            }

            if (target.UsesDenseBattleStorage)
            {
                for (int victimSlot = 0;
                     victimSlot < target.LogicalCapacity;
                     victimSlot++)
                {
                    for (int attackerSlot = 0;
                         attackerSlot < target.LogicalCapacity;
                         attackerSlot++)
                    {
                        int value = source.GetVRest(victimSlot, attackerSlot);
                        if (value > 0 &&
                            !target.SetVRest(victimSlot, attackerSlot, value))
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
                    if (!target.SetVRest(
                            source.SparseVictimSlots[index],
                            source.SparseAttackerSlots[index],
                            source.SparseValues[index]))
                    {
                        return false;
                    }
                }
            }

            return target.ARestEntryCount == source.ARestEntryCount &&
                   target.VRestEntryCount == source.VRestEntryCount &&
                   target.VRestRowCount == source.VRestRowCount;
        }

        private static bool TryCopyCanonicalState(
            RuntimeRestStore source,
            BattleWorldRestSnapshotBuffer destination)
        {
            if (!source.IsPreparedForBattle ||
                destination.LogicalCapacity != source.LogicalCapacity)
            {
                return false;
            }

            bool usesDense = source.UsesDenseBattleStorage;
            if (usesDense !=
                (destination.StorageMode == BattleRestSnapshotStorageMode.Dense))
            {
                return false;
            }
            if (usesDense &&
                destination.DenseVRestValues.Length !=
                    checked(source.LogicalCapacity * source.LogicalCapacity))
            {
                return false;
            }
            if (!usesDense &&
                (!source.UsesPreallocatedSparseBattleStorage ||
                 destination.SparseEntryCapacity < source.VRestEntryCount))
            {
                return false;
            }

            Array.Clear(
                destination.ARestValues,
                0,
                destination.ARestValues.Length);
            int copiedARestEntries = 0;
            foreach (RuntimeRestStore.ARestEntry entry in
                     source.EnumerateCanonicalARestEntries())
            {
                destination.ARestValues[entry.AttackerSlot] = entry.Value;
                copiedARestEntries++;
            }

            int copiedVRestEntries = 0;
            if (usesDense)
            {
                Array.Clear(
                    destination.DenseVRestValues,
                    0,
                    destination.DenseVRestValues.Length);
                foreach (RuntimeRestStore.VRestEntry entry in
                         source.EnumerateCanonicalVRestEntries())
                {
                    destination.DenseVRestValues[
                        entry.VictimSlot * source.LogicalCapacity +
                        entry.AttackerSlot] = entry.Value;
                    copiedVRestEntries++;
                }
            }
            else
            {
                foreach (RuntimeRestStore.VRestEntry entry in
                         source.EnumerateCanonicalVRestEntries())
                {
                    destination.SparseVictimSlots[copiedVRestEntries] =
                        entry.VictimSlot;
                    destination.SparseAttackerSlots[copiedVRestEntries] =
                        entry.AttackerSlot;
                    destination.SparseValues[copiedVRestEntries] = entry.Value;
                    copiedVRestEntries++;
                }
            }

            return copiedARestEntries == source.ARestEntryCount &&
                   copiedVRestEntries == source.VRestEntryCount;
        }
    }
}
