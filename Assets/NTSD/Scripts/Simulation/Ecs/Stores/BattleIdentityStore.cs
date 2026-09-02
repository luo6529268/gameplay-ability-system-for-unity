using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleIdentityAiProjection
    {
        internal BattleIdentityAiProjection(
            int stableId,
            int objectId,
            int dataObjectType)
        {
            StableId = stableId;
            ObjectId = objectId;
            DataObjectType = dataObjectType;
        }

        internal int StableId { get; }
        internal int ObjectId { get; }
        internal int DataObjectType { get; }
    }

    /// <summary>
    /// Persistent slot/generation-owned identity metadata. Object shells remain
    /// compatibility owners during U6, while authority snapshot capture reads the
    /// stable values from this store instead of traversing the entity data graph.
    /// </summary>
    internal sealed class BattleIdentityStore
    {
        private LF2Entity[] owners;
        private uint[] generations;
        private int[] stableIds;
        private int[] objectIds;
        private int[] dataObjectTypes;

        internal BattleIdentityStore(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            owners = new LF2Entity[capacity];
            generations = new uint[capacity];
            stableIds = new int[capacity];
            objectIds = new int[capacity];
            dataObjectTypes = new int[capacity];
        }

        internal void Bind(LF2Entity entity, RuntimeEntityHandle handle)
        {
            if (entity?.Runtime == null ||
                !handle.IsValid ||
                handle.Slot >= owners.Length ||
                entity.Runtime.SlotIndex != handle.Slot)
            {
                throw new InvalidOperationException(
                    "Identity store requires a current runtime handle.");
            }

            int slot = handle.Slot;
            owners[slot] = entity;
            generations[slot] = handle.Generation;
            Capture(slot, entity);
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            if (!handle.IsValid ||
                handle.Slot >= owners.Length ||
                generations[handle.Slot] != handle.Generation)
            {
                return;
            }

            int slot = handle.Slot;
            owners[slot] = null;
            generations[slot] = 0;
            stableIds[slot] = 0;
            objectIds[slot] = 0;
            dataObjectTypes[slot] = 0;
        }

        internal void Reset()
        {
            Array.Clear(owners, 0, owners.Length);
            Array.Clear(generations, 0, generations.Length);
            Array.Clear(stableIds, 0, stableIds.Length);
            Array.Clear(objectIds, 0, objectIds.Length);
            Array.Clear(dataObjectTypes, 0, dataObjectTypes.Length);
        }

        internal void GrowTo(int capacity)
        {
            if (capacity <= owners.Length)
                return;

            Array.Resize(ref owners, capacity);
            Array.Resize(ref generations, capacity);
            Array.Resize(ref stableIds, capacity);
            Array.Resize(ref objectIds, capacity);
            Array.Resize(ref dataObjectTypes, capacity);
        }

        internal bool SyncFromEntity(LF2Entity entity)
        {
            if (!TryResolve(entity, out int slot))
                return false;

            Capture(slot, entity);
            return true;
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleIdentityAiProjection projection)
        {
            if (!TryResolve(handle, out int slot))
            {
                projection = default;
                return false;
            }

            projection = new BattleIdentityAiProjection(
                stableIds[slot],
                objectIds[slot],
                dataObjectTypes[slot]);
            return true;
        }

        private bool TryResolve(LF2Entity entity, out int slot)
        {
            slot = entity?.Runtime?.SlotIndex ?? -1;
            return (uint)slot < (uint)owners.Length &&
                   generations[slot] != 0 &&
                   ReferenceEquals(owners[slot], entity);
        }

        private bool TryResolve(RuntimeEntityHandle handle, out int slot)
        {
            slot = handle.Slot;
            return handle.IsValid &&
                   (uint)slot < (uint)owners.Length &&
                   generations[slot] == handle.Generation &&
                   owners[slot] != null;
        }

        private void Capture(int slot, LF2Entity entity)
        {
            stableIds[slot] = entity.Runtime.StableId;
            objectIds[slot] = entity.ObjectId;
            dataObjectTypes[slot] =
                entity.GetCurrentDataObjectTypeForSimulation();
        }
    }

    internal sealed class BattleIdentityWriter
    {
        private readonly BattleIdentityStore store;

        internal BattleIdentityWriter(int capacity)
        {
            store = new BattleIdentityStore(capacity);
        }

        internal void Bind(LF2Entity entity, RuntimeEntityHandle handle)
        {
            store.Bind(entity, handle);
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            store.Release(handle);
        }

        internal void Reset()
        {
            store.Reset();
        }

        internal void GrowTo(int capacity)
        {
            store.GrowTo(capacity);
        }

        internal bool SyncFromEntity(LF2Entity entity)
        {
            return store.SyncFromEntity(entity);
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleIdentityAiProjection projection)
        {
            return store.TryCaptureAiProjection(handle, out projection);
        }
    }
}
