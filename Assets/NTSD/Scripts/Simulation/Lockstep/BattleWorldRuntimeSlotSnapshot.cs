using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public enum BattleRuntimeEntityKind : byte
    {
        None = 0,
        Character = 1,
        Weapon = 2,
        SpecialAttack = 3,
        Other = 4,
        Unknown = byte.MaxValue,
    }

    public readonly struct BattleRuntimeSlotSnapshot
    {
        internal BattleRuntimeSlotSnapshot(
            bool claimed,
            uint generation,
            BattleRuntimeEntityKind entityKind,
            int stableId,
            int objectId,
            int currentDataObjectId,
            int currentDataObjectType,
            int runtimeObjectType,
            int runtimeEntityType,
            int spawnSemantic)
        {
            Claimed = claimed;
            Generation = generation;
            EntityKind = entityKind;
            StableId = stableId;
            ObjectId = objectId;
            CurrentDataObjectId = currentDataObjectId;
            CurrentDataObjectType = currentDataObjectType;
            RuntimeObjectType = runtimeObjectType;
            RuntimeEntityType = runtimeEntityType;
            SpawnSemantic = spawnSemantic;
        }

        public bool Claimed { get; }
        public uint Generation { get; }
        public BattleRuntimeEntityKind EntityKind { get; }
        public int StableId { get; }
        public int ObjectId { get; }
        public int CurrentDataObjectId { get; }
        public int CurrentDataObjectType { get; }
        public int RuntimeObjectType { get; }
        public int RuntimeEntityType { get; }
        public int SpawnSemantic { get; }
    }

    /// <summary>
    /// Preallocated capture of runtime-slot ownership and generation truth.
    /// Entity payload, rest state, pending queues, and derived stores are separate
    /// snapshot domains; this buffer deliberately does not expose Restore.
    /// </summary>
    public sealed class BattleWorldRuntimeSlotSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly BattleRuntimeSlotSnapshot[] slots;

        public BattleWorldRuntimeSlotSnapshotBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
            slots = new BattleRuntimeSlotSnapshot[slotCapacity];
        }

        public int SlotCapacity { get; }
        public int ClaimedCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public BattleRuntimeSlotSnapshot GetSlot(int runtimeSlot)
        {
            if ((uint)runtimeSlot >= (uint)SlotCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(runtimeSlot));
            }

            return slots[runtimeSlot];
        }

        internal bool TryCapture(
            RuntimeSlotTable source,
            LockstepSessionIdentity identity,
            int tick)
        {
            if (source == null || identity == null ||
                source.LogicalCapacity != SlotCapacity ||
                !HasCanonicalSource(source))
            {
                return false;
            }

            for (int runtimeSlot = 0;
                 runtimeSlot < SlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    source.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                {
                    slots[runtimeSlot] = new BattleRuntimeSlotSnapshot(
                        false,
                        view.Generation,
                        BattleRuntimeEntityKind.None,
                        -1,
                        -1,
                        -1,
                        -1,
                        -1,
                        -1,
                        0);
                    continue;
                }

                LF2Entity entity = view.Entity;
                NTSDEntityRuntime runtime = entity.Runtime;
                slots[runtimeSlot] = new BattleRuntimeSlotSnapshot(
                    true,
                    view.Generation,
                    ResolveEntityKind(entity),
                    runtime.StableId,
                    runtime.ObjectId,
                    LF2Entity.ResolveCurrentDataObjectId(entity),
                    entity.GetCurrentDataObjectTypeForSimulation(),
                    runtime.ObjType,
                    runtime.EntityType,
                    runtime.SpawnSemantic);
            }

            ClaimedCount = source.ClaimedCount;
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
                    view.Entity.Runtime.SlotIndex != runtimeSlot)
                {
                    return false;
                }
            }

            return observedClaimedCount == source.ClaimedCount;
        }

        private static BattleRuntimeEntityKind ResolveEntityKind(LF2Entity entity)
        {
            if (entity is LF2Character)
            {
                return BattleRuntimeEntityKind.Character;
            }
            if (entity is LF2Weapon)
            {
                return BattleRuntimeEntityKind.Weapon;
            }
            if (entity is LF2SpecialAttack)
            {
                return BattleRuntimeEntityKind.SpecialAttack;
            }
            if (entity is LF2OtherObject)
            {
                return BattleRuntimeEntityKind.Other;
            }

            return BattleRuntimeEntityKind.Unknown;
        }
    }

    internal sealed class BattleWorldRuntimeSlotSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldRuntimeSlotSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int SlotCapacity => world.RuntimeSlotCapacity;

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldRuntimeSlotSnapshotBuffer destination)
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
