using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public readonly struct BattleEntityBaseShellSnapshot
    {
        internal BattleEntityBaseShellSnapshot(
            LF2Entity entity,
            RuntimeEntityHandle trackerParentHandle)
        {
            RequiredRuntimeSlot = entity.RequiredRuntimeSlot;
            CurrentItrIndex = entity.CurrentItrIndex;
            TrackerParentHandle = trackerParentHandle;

            FramePreviousNumber = entity.Frame.PN;
            FramePreviousTick = entity.Frame.Prev;
            FrameNumber = entity.Frame.N;
            FrameDataId = entity.Frame.D?.frameId ?? -1;
            CollisionPreviousFrame = entity.Frame.Prev2;
            CollisionFrameDataId = entity.Frame.Prev2D?.frameId ?? -1;

            TransistorWait = entity.Trans.Wait;
            TransistorWaitCounter = entity.Trans.WaitCounter;
            TransistorNext = entity.Trans.Next;

            EffectNumber = entity.Effect.Num;
            EffectDvx = entity.Effect.Dvx;
            EffectDvy = entity.Effect.Dvy;
            EffectStuck = entity.Effect.Stuck;
            EffectOscillate = entity.Effect.Oscillate;
            EffectBlink = entity.Effect.Blink;
            EffectSuper = entity.Effect.Super;
            EffectTimeIn = entity.Effect.TimeIn;
            EffectTimeOut = entity.Effect.TimeOut;
            EffectOscillateDirection = entity.Effect.OscillateDirection;
            EffectBlinkCounter = entity.Effect.BlinkCounter;

            PhysicsGroundY = entity.PS.groundY;
            PhysicsFacingLeft = entity.PS.dir == "left";
            PhysicsFriction = entity.PS.fric;
            PhysicsDepthOffset = entity.PS.zz;
            PhysicsZBoundPositive = entity.PS.zBoundPositive;
            PhysicsZBoundNegative = entity.PS.zBoundNegative;
            PhysicsXBoundPositive = entity.PS.xBoundPositive;
            PhysicsXBoundNegative = entity.PS.xBoundNegative;
            HitRecordCount = entity.HitRecordCount;
        }

        public int RequiredRuntimeSlot { get; }
        public int CurrentItrIndex { get; }
        public RuntimeEntityHandle TrackerParentHandle { get; }
        public int FramePreviousNumber { get; }
        public int FramePreviousTick { get; }
        public int FrameNumber { get; }
        public int FrameDataId { get; }
        public int CollisionPreviousFrame { get; }
        public int CollisionFrameDataId { get; }
        public int TransistorWait { get; }
        public int TransistorWaitCounter { get; }
        public int TransistorNext { get; }
        public int EffectNumber { get; }
        public float EffectDvx { get; }
        public float EffectDvy { get; }
        public bool EffectStuck { get; }
        public int EffectOscillate { get; }
        public bool EffectBlink { get; }
        public bool EffectSuper { get; }
        public int EffectTimeIn { get; }
        public int EffectTimeOut { get; }
        public int EffectOscillateDirection { get; }
        public int EffectBlinkCounter { get; }
        public float PhysicsGroundY { get; }
        public bool PhysicsFacingLeft { get; }
        public float PhysicsFriction { get; }
        public float PhysicsDepthOffset { get; }
        public bool PhysicsZBoundPositive { get; }
        public bool PhysicsZBoundNegative { get; }
        public bool PhysicsXBoundPositive { get; }
        public bool PhysicsXBoundNegative { get; }
        public int HitRecordCount { get; }
    }

    /// <summary>
    /// Preallocated base-shell capture shared by every LF2 entity type. Static DAT
    /// objects and Unity presentation adapters are deliberately excluded and will be
    /// rebound from identity metadata during restore.
    /// </summary>
    public sealed class BattleWorldEntityBaseShellSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly bool[] present;
        private readonly BattleEntityBaseShellSnapshot[] states;
        private readonly int[] hitRecordDamage;
        private readonly int[] hitRecordX;
        private readonly int[] hitRecordZ;
        private readonly int[] hitRecordLastAdvanceTick;

        public BattleWorldEntityBaseShellSnapshotBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
            present = new bool[slotCapacity];
            states = new BattleEntityBaseShellSnapshot[slotCapacity];
            int hitRecordCapacity = checked(
                slotCapacity * LF2Entity.MaxHitRecordSlots);
            hitRecordDamage = new int[hitRecordCapacity];
            hitRecordX = new int[hitRecordCapacity];
            hitRecordZ = new int[hitRecordCapacity];
            hitRecordLastAdvanceTick = new int[hitRecordCapacity];
        }

        public int SlotCapacity { get; }
        public int EntityCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public bool HasEntity(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            return present[runtimeSlot];
        }

        public BattleEntityBaseShellSnapshot GetState(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            if (!present[runtimeSlot])
            {
                throw new InvalidOperationException(
                    "The requested runtime slot has no captured entity shell.");
            }

            return states[runtimeSlot];
        }

        public int GetHitRecordDamage(int runtimeSlot, int recordIndex)
            => hitRecordDamage[ResolveHitRecordIndex(runtimeSlot, recordIndex)];

        public int GetHitRecordX(int runtimeSlot, int recordIndex)
            => hitRecordX[ResolveHitRecordIndex(runtimeSlot, recordIndex)];

        public int GetHitRecordZ(int runtimeSlot, int recordIndex)
            => hitRecordZ[ResolveHitRecordIndex(runtimeSlot, recordIndex)];

        public int GetHitRecordLastAdvanceTick(
            int runtimeSlot,
            int recordIndex)
            => hitRecordLastAdvanceTick[
                ResolveHitRecordIndex(runtimeSlot, recordIndex)];

        internal bool TryCapture(
            SimulationWorld world,
            LockstepSessionIdentity identity,
            int tick)
        {
            if (world == null ||
                identity == null ||
                world.RuntimeSlotCapacity != SlotCapacity ||
                !HasCanonicalSource(world))
            {
                return false;
            }

            int entityCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < SlotCapacity;
                 runtimeSlot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        runtimeSlot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed)
                {
                    present[runtimeSlot] = false;
                    continue;
                }

                LF2Entity entity = view.Entity;
                TryResolveHandle(
                    world,
                    entity.TrackerParent,
                    out RuntimeEntityHandle trackerParentHandle);
                present[runtimeSlot] = true;
                states[runtimeSlot] = new BattleEntityBaseShellSnapshot(
                    entity,
                    trackerParentHandle);
                int hitRecordBase =
                    runtimeSlot * LF2Entity.MaxHitRecordSlots;
                for (int recordIndex = 0;
                     recordIndex < LF2Entity.MaxHitRecordSlots;
                     recordIndex++)
                {
                    int destinationIndex = hitRecordBase + recordIndex;
                    if (recordIndex < entity.HitRecordCount)
                    {
                        hitRecordDamage[destinationIndex] =
                            entity.GetHitRecordAge(recordIndex);
                        hitRecordX[destinationIndex] =
                            entity.GetHitRecordX(recordIndex);
                        hitRecordZ[destinationIndex] =
                            entity.GetHitRecordZ(recordIndex);
                        hitRecordLastAdvanceTick[destinationIndex] =
                            entity.GetHitRecordLastAdvanceTickForSnapshot(
                                recordIndex);
                    }
                    else
                    {
                        hitRecordDamage[destinationIndex] = 0;
                        hitRecordX[destinationIndex] = 0;
                        hitRecordZ[destinationIndex] = 0;
                        hitRecordLastAdvanceTick[destinationIndex] = 0;
                    }
                }

                entityCount++;
            }

            EntityCount = entityCount;
            SchemaVersion = CurrentSchemaVersion;
            ProtocolSchemaVersion = identity.SchemaVersion;
            IdentityFingerprint = identity.IdentityFingerprint;
            CapturedTick = tick;
            return true;
        }

        private static bool HasCanonicalSource(SimulationWorld world)
        {
            int observedEntityCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < world.RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        runtimeSlot,
                        out RuntimeSlotTable.ReadOnlySlotView view))
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

                LF2Entity entity = view.Entity;
                observedEntityCount++;
                if (entity == null ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != runtimeSlot ||
                    entity.Frame == null ||
                    entity.Trans == null ||
                    entity.Effect == null ||
                    entity.PS == null ||
                    entity.HitRecordCount < 0 ||
                    entity.HitRecordCount > LF2Entity.MaxHitRecordSlots ||
                    !TryResolveHandle(
                        world,
                        entity.TrackerParent,
                        out _))
                {
                    return false;
                }
            }

            return observedEntityCount ==
                   world.RuntimeSlotTableForModules.ClaimedCount;
        }

        private static bool TryResolveHandle(
            SimulationWorld world,
            LF2Entity entity,
            out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (entity == null)
            {
                return true;
            }

            int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
            return runtimeSlot >= 0 &&
                   world.TryGetCurrentRuntimeHandle(
                       runtimeSlot,
                       entity,
                       out handle);
        }

        private int ResolveHitRecordIndex(int runtimeSlot, int recordIndex)
        {
            ValidateSlot(runtimeSlot);
            if (!present[runtimeSlot])
            {
                throw new InvalidOperationException(
                    "The requested runtime slot has no captured entity shell.");
            }
            if ((uint)recordIndex >= (uint)states[runtimeSlot].HitRecordCount)
            {
                throw new ArgumentOutOfRangeException(nameof(recordIndex));
            }

            return runtimeSlot * LF2Entity.MaxHitRecordSlots + recordIndex;
        }

        private void ValidateSlot(int runtimeSlot)
        {
            if ((uint)runtimeSlot >= (uint)SlotCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(runtimeSlot));
            }
        }
    }

    internal sealed class BattleWorldEntityBaseShellSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldEntityBaseShellSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int SlotCapacity => world.RuntimeSlotCapacity;

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldEntityBaseShellSnapshotBuffer destination)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            return destination.TryCapture(world, identity, tick);
        }
    }
}
