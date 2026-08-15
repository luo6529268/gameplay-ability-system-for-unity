using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public enum BattleSpecialOtherShellKind : byte
    {
        None = 0,
        SpecialAttack = 1,
        OtherObject = 2,
    }

    public readonly struct BattleSpecialOtherShellSnapshot
    {
        internal BattleSpecialOtherShellSnapshot(
            LF2SpecialAttack special,
            RuntimeEntityHandle parentHandle)
        {
            Kind = BattleSpecialOtherShellKind.SpecialAttack;
            ParentHandle = parentHandle;
            LastState = special.LastStateForSnapshot;
            NoBounce = special.NoBounce;
            InvalidInitTaskTypeCount =
                special.InvalidInitTaskTypeCountForDiagnostics;
        }

        internal BattleSpecialOtherShellSnapshot(LF2OtherObject other)
        {
            Kind = BattleSpecialOtherShellKind.OtherObject;
            ParentHandle = RuntimeEntityHandle.Invalid;
            LastState = 0;
            NoBounce = false;
            InvalidInitTaskTypeCount =
                other.InvalidInitTaskTypeCountForDiagnostics;
        }

        public BattleSpecialOtherShellKind Kind { get; }
        public RuntimeEntityHandle ParentHandle { get; }
        public int LastState { get; }
        public bool NoBounce { get; }
        public long InvalidInitTaskTypeCount { get; }
    }

    /// <summary>
    /// Preallocated capture for the remaining special-attack and other-object
    /// subtype shell state. Resource and renderer bindings are rebuilt after
    /// restore and are not copied into this deterministic payload.
    /// </summary>
    public sealed class BattleWorldSpecialOtherShellSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly bool[] present;
        private readonly BattleSpecialOtherShellSnapshot[] states;

        public BattleWorldSpecialOtherShellSnapshotBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
            present = new bool[slotCapacity];
            states = new BattleSpecialOtherShellSnapshot[slotCapacity];
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

        public BattleSpecialOtherShellSnapshot GetState(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            if (!present[runtimeSlot])
            {
                throw new InvalidOperationException(
                    "The requested runtime slot has no captured special/other shell.");
            }

            return states[runtimeSlot];
        }

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

                if (view.Entity is LF2SpecialAttack special)
                {
                    TryResolveParentHandle(
                        world,
                        special.ParentForSnapshot,
                        out RuntimeEntityHandle parentHandle);
                    present[runtimeSlot] = true;
                    states[runtimeSlot] = new BattleSpecialOtherShellSnapshot(
                        special,
                        parentHandle);
                    entityCount++;
                }
                else if (view.Entity is LF2OtherObject other)
                {
                    present[runtimeSlot] = true;
                    states[runtimeSlot] = new BattleSpecialOtherShellSnapshot(other);
                    entityCount++;
                }
                else
                {
                    present[runtimeSlot] = false;
                }
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
                    continue;
                }

                if (view.Entity is LF2SpecialAttack special)
                {
                    if (special.Runtime == null ||
                        special.Runtime.SlotIndex != runtimeSlot ||
                        special.Health == null ||
                        special.ItrRest == null ||
                        !TryResolveParentHandle(
                            world,
                            special.ParentForSnapshot,
                            out _))
                    {
                        return false;
                    }
                }
                else if (view.Entity is LF2OtherObject other &&
                         (other.Runtime == null ||
                          other.Runtime.SlotIndex != runtimeSlot ||
                          other.Health == null ||
                          other.ItrRest == null))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryResolveParentHandle(
            SimulationWorld world,
            LF2LivingObject parent,
            out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (parent == null)
            {
                return true;
            }

            int runtimeSlot = parent.Runtime?.SlotIndex ?? -1;
            return runtimeSlot >= 0 &&
                   world.TryGetCurrentRuntimeHandle(
                       runtimeSlot,
                       parent,
                       out handle);
        }

        private void ValidateSlot(int runtimeSlot)
        {
            if ((uint)runtimeSlot >= (uint)SlotCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(runtimeSlot));
            }
        }
    }

    internal sealed class BattleWorldSpecialOtherShellSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldSpecialOtherShellSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int SlotCapacity => world.RuntimeSlotCapacity;

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldSpecialOtherShellSnapshotBuffer destination)
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
