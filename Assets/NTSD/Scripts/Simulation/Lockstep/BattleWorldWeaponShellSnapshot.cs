using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public readonly struct BattleWeaponShellSnapshot
    {
        internal BattleWeaponShellSnapshot(LF2WeaponBase weapon)
        {
            LateBreakEffectsHandled =
                weapon.LateBreakEffectsHandledForSnapshot;
            InvalidInitTaskTypeCount =
                weapon.InvalidInitTaskTypeCountForDiagnostics;
            GravityToAdd = weapon.GravityToAddForSnapshot;
            LastLandingVyBeforeClamp =
                weapon.LastLandingVyBeforeClampForSnapshot;
            HasPoolWeaponType = weapon is LF2Weapon;
            PoolWeaponType = weapon is LF2Weapon concrete
                ? concrete.PoolWeaponTypeForSnapshot
                : 0;
        }

        public bool LateBreakEffectsHandled { get; }
        public long InvalidInitTaskTypeCount { get; }
        public double GravityToAdd { get; }
        public double LastLandingVyBeforeClamp { get; }
        public bool HasPoolWeaponType { get; }
        public int PoolWeaponType { get; }
    }

    /// <summary>
    /// Preallocated capture for mutable weapon shell state not owned by the
    /// canonical runtime payload. DAT strength/sound configuration and controller
    /// adapters are resource bindings and are rebuilt after restore.
    /// </summary>
    public sealed class BattleWorldWeaponShellSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly bool[] present;
        private readonly BattleWeaponShellSnapshot[] states;

        public BattleWorldWeaponShellSnapshotBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
            present = new bool[slotCapacity];
            states = new BattleWeaponShellSnapshot[slotCapacity];
        }

        public int SlotCapacity { get; }
        public int WeaponCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public bool HasWeapon(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            return present[runtimeSlot];
        }

        public BattleWeaponShellSnapshot GetState(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            if (!present[runtimeSlot])
            {
                throw new InvalidOperationException(
                    "The requested runtime slot has no captured weapon shell.");
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

            int weaponCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < SlotCapacity;
                 runtimeSlot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        runtimeSlot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed ||
                    view.Entity is not LF2WeaponBase weapon)
                {
                    present[runtimeSlot] = false;
                    continue;
                }

                present[runtimeSlot] = true;
                states[runtimeSlot] = new BattleWeaponShellSnapshot(weapon);
                weaponCount++;
            }

            WeaponCount = weaponCount;
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
                if (!view.Claimed || view.Entity is not LF2WeaponBase weapon)
                {
                    continue;
                }
                if (weapon.Runtime == null ||
                    weapon.Runtime.SlotIndex != runtimeSlot ||
                    weapon.Health == null ||
                    weapon.ItrRest == null)
                {
                    return false;
                }
            }

            return true;
        }

        private void ValidateSlot(int runtimeSlot)
        {
            if ((uint)runtimeSlot >= (uint)SlotCapacity)
            {
                throw new ArgumentOutOfRangeException(nameof(runtimeSlot));
            }
        }
    }

    internal sealed class BattleWorldWeaponShellSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldWeaponShellSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int SlotCapacity => world.RuntimeSlotCapacity;

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldWeaponShellSnapshotBuffer destination)
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
