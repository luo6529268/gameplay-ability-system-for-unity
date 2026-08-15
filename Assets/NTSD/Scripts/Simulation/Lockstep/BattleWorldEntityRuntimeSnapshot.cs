using System;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    /// <summary>
    /// Preallocated capture of the mutable runtime payload owned by each live entity
    /// and by each materialized raw runtime slot. Entity shells, pending queues, and
    /// derived store bindings are separate snapshot domains.
    /// </summary>
    public sealed class BattleWorldEntityRuntimeSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly bool[] entityRuntimePresent;
        private readonly bool[] rawRuntimePresent;
        private readonly NTSDEntityRuntime[] entityRuntimes;
        private readonly NTSDEntityRuntime[] rawRuntimes;

        public BattleWorldEntityRuntimeSnapshotBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
            entityRuntimePresent = new bool[slotCapacity];
            rawRuntimePresent = new bool[slotCapacity];
            entityRuntimes = CreateRuntimeStorage(slotCapacity);
            rawRuntimes = CreateRuntimeStorage(slotCapacity);
        }

        public int SlotCapacity { get; }
        public int EntityRuntimeCount { get; private set; }
        public int RawRuntimeCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public bool HasEntityRuntime(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            return entityRuntimePresent[runtimeSlot];
        }

        public bool HasRawRuntime(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            return rawRuntimePresent[runtimeSlot];
        }

        public bool TryCopyEntityRuntime(
            int runtimeSlot,
            NTSDEntityRuntime destination)
        {
            ValidateSlot(runtimeSlot);
            return entityRuntimePresent[runtimeSlot] &&
                   entityRuntimes[runtimeSlot]
                       .TryCopyCanonicalStateTo(destination);
        }

        public bool TryCopyRawRuntime(
            int runtimeSlot,
            NTSDEntityRuntime destination)
        {
            ValidateSlot(runtimeSlot);
            return rawRuntimePresent[runtimeSlot] &&
                   rawRuntimes[runtimeSlot]
                       .TryCopyCanonicalStateTo(destination);
        }

        internal bool TryCapture(
            RuntimeSlotTable source,
            LockstepSessionIdentity identity,
            int tick)
        {
            if (source == null ||
                identity == null ||
                source.LogicalCapacity != SlotCapacity ||
                !HasCanonicalSource(source))
            {
                return false;
            }

            int entityCount = 0;
            int rawCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < SlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    source.GetReadOnlyView(runtimeSlot);
                bool hasEntity = view.Claimed;
                bool hasRaw = view.RawRuntime != null;

                entityRuntimePresent[runtimeSlot] = hasEntity;
                rawRuntimePresent[runtimeSlot] = hasRaw;
                if (hasEntity)
                {
                    view.Entity.Runtime.TryCopyCanonicalStateTo(
                        entityRuntimes[runtimeSlot]);
                    entityCount++;
                }
                if (hasRaw)
                {
                    view.RawRuntime.TryCopyCanonicalStateTo(
                        rawRuntimes[runtimeSlot]);
                    rawCount++;
                }
            }

            EntityRuntimeCount = entityCount;
            RawRuntimeCount = rawCount;
            SchemaVersion = CurrentSchemaVersion;
            ProtocolSchemaVersion = identity.SchemaVersion;
            IdentityFingerprint = identity.IdentityFingerprint;
            CapturedTick = tick;
            return true;
        }

        private static bool HasCanonicalSource(RuntimeSlotTable source)
        {
            int observedClaimedCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < source.LogicalCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    source.GetReadOnlyView(runtimeSlot);
                if (view.RawRuntime != null &&
                    !view.RawRuntime.HasCanonicalSnapshotStorage)
                {
                    return false;
                }

                if (!view.Claimed)
                {
                    if (view.Entity != null)
                    {
                        return false;
                    }

                    continue;
                }

                observedClaimedCount++;
                if (view.Generation == 0 ||
                    view.Entity == null ||
                    view.RawRuntime == null ||
                    view.Entity.Runtime == null ||
                    ReferenceEquals(view.Entity.Runtime, view.RawRuntime) ||
                    view.Entity.Runtime.SlotIndex != runtimeSlot ||
                    !view.Entity.Runtime.HasCanonicalSnapshotStorage)
                {
                    return false;
                }
            }

            return observedClaimedCount == source.ClaimedCount;
        }

        private static NTSDEntityRuntime[] CreateRuntimeStorage(int capacity)
        {
            var storage = new NTSDEntityRuntime[capacity];
            for (int index = 0; index < storage.Length; index++)
            {
                storage[index] = new NTSDEntityRuntime();
            }

            return storage;
        }

        private void ValidateSlot(int runtimeSlot)
        {
            if ((uint)runtimeSlot >= (uint)SlotCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(runtimeSlot));
            }
        }
    }

    internal sealed class BattleWorldEntityRuntimeSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldEntityRuntimeSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int SlotCapacity => world.RuntimeSlotCapacity;

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldEntityRuntimeSnapshotBuffer destination)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            return destination.TryCapture(
                world.RuntimeSlotTableForModules,
                identity,
                tick);
        }
    }
}
