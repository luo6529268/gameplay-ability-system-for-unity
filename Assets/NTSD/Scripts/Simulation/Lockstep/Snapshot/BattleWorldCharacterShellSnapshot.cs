using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public readonly struct BattleCharacterShellSnapshot
    {
        internal BattleCharacterShellSnapshot(
            LF2Character character,
            RuntimeEntityHandle heldWeaponHandle)
        {
            HeldWeaponHandle = heldWeaponHandle;
            Mass = character.MassForFrameAdvance;
            DeadBlinkCount = character.DeadBlinkCountInternal;
            InitializedFromOpoint = character.InitializedFromOpointForSnapshot;
            PreserveOpointActionZero =
                character.PreserveOpointActionZeroForSnapshot;
        }

        public RuntimeEntityHandle HeldWeaponHandle { get; }
        public float Mass { get; }
        public int DeadBlinkCount { get; }
        public bool InitializedFromOpoint { get; }
        public bool PreserveOpointActionZero { get; }
    }

    /// <summary>
    /// Preallocated capture for scalar and relationship state owned directly by
    /// LF2Character. Runtime-mirrored input and Unity host adapters are rebuilt
    /// after restore rather than serialized as duplicate object graphs.
    /// </summary>
    public sealed class BattleWorldCharacterShellSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly bool[] present;
        private readonly BattleCharacterShellSnapshot[] states;

        public BattleWorldCharacterShellSnapshotBuffer(int slotCapacity)
        {
            if (slotCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
            present = new bool[slotCapacity];
            states = new BattleCharacterShellSnapshot[slotCapacity];
        }

        public int SlotCapacity { get; }
        public int CharacterCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public bool HasCharacter(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            return present[runtimeSlot];
        }

        public BattleCharacterShellSnapshot GetState(int runtimeSlot)
        {
            ValidateSlot(runtimeSlot);
            if (!present[runtimeSlot])
            {
                throw new InvalidOperationException(
                    "The requested runtime slot has no captured character shell.");
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

            int characterCount = 0;
            for (int runtimeSlot = 0;
                 runtimeSlot < SlotCapacity;
                 runtimeSlot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        runtimeSlot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed ||
                    view.Entity is not LF2Character character)
                {
                    present[runtimeSlot] = false;
                    continue;
                }

                TryResolveHeldWeaponHandle(
                    world,
                    character.HeldWeaponReferenceInternal,
                    out RuntimeEntityHandle heldWeaponHandle);
                present[runtimeSlot] = true;
                states[runtimeSlot] = new BattleCharacterShellSnapshot(
                    character,
                    heldWeaponHandle);
                characterCount++;
            }

            CharacterCount = characterCount;
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
                if (!view.Claimed || view.Entity is not LF2Character character)
                {
                    continue;
                }
                if (character.Runtime == null ||
                    character.Runtime.SlotIndex != runtimeSlot ||
                    character.InputState == null ||
                    character.Controller == null ||
                    character.HitCounters == null ||
                    !TryResolveHeldWeaponHandle(
                        world,
                        character.HeldWeaponReferenceInternal,
                        out _))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryResolveHeldWeaponHandle(
            SimulationWorld world,
            ILF2Object heldWeapon,
            out RuntimeEntityHandle handle)
        {
            handle = RuntimeEntityHandle.Invalid;
            if (heldWeapon == null)
            {
                return true;
            }
            if (heldWeapon is not LF2Entity entity)
            {
                return false;
            }

            int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
            return runtimeSlot >= 0 &&
                   world.TryGetCurrentRuntimeHandle(
                       runtimeSlot,
                       entity,
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

    internal sealed class BattleWorldCharacterShellSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldCharacterShellSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal int SlotCapacity => world.RuntimeSlotCapacity;

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldCharacterShellSnapshotBuffer destination)
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
