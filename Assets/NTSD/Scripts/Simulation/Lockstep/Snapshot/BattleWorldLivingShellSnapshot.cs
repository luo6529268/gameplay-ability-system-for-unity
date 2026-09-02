using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public readonly struct BattleLivingShellSnapshot
    {
        internal BattleLivingShellSnapshot(
            LF2LivingObject living,
            RuntimeEntityHandle catchingHandle,
            RuntimeEntityHandle attackerHandle)
        {
            InvalidFrameTransitionCount =
                living.InvalidFrameTransitionCountForDiagnostics;
            CatchingHandle = catchingHandle;
            Dead = living.Dead;
            AttackerHandle = attackerHandle;
            FallRecoveryAccum =
                living.HitCounters?.FallRecoveryAccumForSnapshot ?? 0f;
            BdefendRecoveryAccum =
                living.HitCounters?.BdefendRecoveryAccumForSnapshot ?? 0f;
        }

        public long InvalidFrameTransitionCount { get; }
        public RuntimeEntityHandle CatchingHandle { get; }
        public bool Dead { get; }
        public RuntimeEntityHandle AttackerHandle { get; }
        public float FallRecoveryAccum { get; }
        public float BdefendRecoveryAccum { get; }
    }

    /// <summary>
    /// Preallocated capture for state owned by LF2LivingObject rather than its
    /// canonical runtime payload. Controller and store adapters are rebound after
    /// restore and are intentionally not serialized as CLR references.
    /// </summary>
    public sealed class BattleWorldLivingShellSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly bool[] present;
        private readonly BattleLivingShellSnapshot[] states;

        public BattleWorldLivingShellSnapshotBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
            present = new bool[slotCapacity];
            states = new BattleLivingShellSnapshot[slotCapacity];
        }

        public int SlotCapacity { get; }
        public int LivingCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public bool HasLiving(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            return present[runtimeSlot];
        }

        public BattleLivingShellSnapshot GetState(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            if (!present[runtimeSlot])
            {
                throw new InvalidOperationException(
                    "The requested runtime slot has no captured living shell.");
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

            int livingCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < SlotCapacity;
                 runtimeSlot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        runtimeSlot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed ||
                    view.Entity is not LF2LivingObject living)
                {
                    present[runtimeSlot] = false;
                    continue;
                }

                TryResolveLivingHandle(
                    world,
                    living.Catching,
                    out RuntimeEntityHandle catchingHandle);
                TryResolveLivingHandle(
                    world,
                    living.Attacker,
                    out RuntimeEntityHandle attackerHandle);
                present[runtimeSlot] = true;
                states[runtimeSlot] = new BattleLivingShellSnapshot(
                    living,
                    catchingHandle,
                    attackerHandle);
                livingCount++;
            }

            LivingCount = livingCount;
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
                if (!view.Claimed || view.Entity is not LF2LivingObject living)
                {
                    continue;
                }
                if (living.Runtime == null ||
                    living.Runtime.SlotIndex != runtimeSlot ||
                    living.Health == null ||
                    living.ItrRest == null ||
                    living.HitCounters == null ||
                    !TryResolveLivingHandle(world, living.Catching, out _) ||
                    !TryResolveLivingHandle(world, living.Attacker, out _))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryResolveLivingHandle(
            SimulationWorld world,
            LF2LivingObject living,
            out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (living == null)
            {
                return true;
            }

            int runtimeSlot = living.Runtime?.SlotIndex ?? -1;
            return runtimeSlot >= 0 &&
                   world.TryGetCurrentRuntimeHandle(
                       runtimeSlot,
                       living,
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

    internal sealed class BattleWorldLivingShellSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldLivingShellSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int SlotCapacity => world.RuntimeSlotCapacity;

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldLivingShellSnapshotBuffer destination)
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
