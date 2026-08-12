using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterStageZPassMode : byte
    {
        Legacy = 0,
        ShadowCompare = 1,
        DataOriented = 2,
    }

    public enum BattleEcsCharacterStageZMismatchKind : byte
    {
        None = 0,
        Occupancy = 1,
        Generation = 2,
        Z = 3,
        ZInt = 4,
    }

    public readonly struct BattleEcsCharacterStageZPassDiagnostics
    {
        internal BattleEcsCharacterStageZPassDiagnostics(
            BattleEcsCharacterStageZPassMode mode,
            long runCount,
            long slotVisitCount,
            long validationCount,
            long mismatchCount,
            long derivedTypeFallbackCount,
            int firstMismatchSlot,
            BattleEcsCharacterStageZMismatchKind firstMismatchKind)
        {
            Mode = mode;
            RunCount = runCount;
            SlotVisitCount = slotVisitCount;
            ValidationCount = validationCount;
            MismatchCount = mismatchCount;
            DerivedTypeFallbackCount = derivedTypeFallbackCount;
            FirstMismatchSlot = firstMismatchSlot;
            FirstMismatchKind = firstMismatchKind;
        }

        public BattleEcsCharacterStageZPassMode Mode { get; }
        public long RunCount { get; }
        public long SlotVisitCount { get; }
        public long ValidationCount { get; }
        public long MismatchCount { get; }
        public long DerivedTypeFallbackCount { get; }
        public int FirstMismatchSlot { get; }
        public BattleEcsCharacterStageZMismatchKind FirstMismatchKind { get; }
        public bool IsClean => MismatchCount == 0;
    }

    /// <summary>
    /// U4 character stage-Z migration slice. The authority pass only writes Z and
    /// ZInt; the legacy path remains available as an explicit comparison oracle.
    /// </summary>
    internal sealed class BattleEcsCharacterStageZPass
    {
        private readonly SimulationWorld world;
        private readonly RuntimeSlotTable runtimeSlots;
        private readonly BattleSlotBitSet expectedSlots;
        private readonly uint[] expectedGenerations;
        private readonly long[] expectedZBits;
        private readonly int[] expectedZInt;
        private BattleEcsCharacterStageZPassMode mode =
            BattleEcsCharacterStageZPassMode.Legacy;
        private long runCount;
        private long slotVisitCount;
        private long validationCount;
        private long mismatchCount;
        private long derivedTypeFallbackCount;
        private int firstMismatchSlot = -1;
        private BattleEcsCharacterStageZMismatchKind firstMismatchKind;

        public BattleEcsCharacterStageZPass(
            SimulationWorld world,
            RuntimeSlotTable runtimeSlots,
            int capacity)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.runtimeSlots = runtimeSlots ??
                throw new ArgumentNullException(nameof(runtimeSlots));
            if (capacity <= 0 || capacity != runtimeSlots.LogicalCapacity)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            expectedSlots = new BattleSlotBitSet(capacity);
            expectedGenerations = new uint[capacity];
            expectedZBits = new long[capacity];
            expectedZInt = new int[capacity];
        }

        public BattleEcsCharacterStageZPassMode Mode => mode;

        public BattleEcsCharacterStageZPassDiagnostics Diagnostics =>
            new BattleEcsCharacterStageZPassDiagnostics(
                mode,
                runCount,
                slotVisitCount,
                validationCount,
                mismatchCount,
                derivedTypeFallbackCount,
                firstMismatchSlot,
                firstMismatchKind);

        public void SetMode(BattleEcsCharacterStageZPassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        public void Reset()
        {
            expectedSlots.ClearAll();
            ResetDiagnostics();
        }

        public void Execute()
        {
            switch (mode)
            {
                case BattleEcsCharacterStageZPassMode.Legacy:
                    world.RunLegacyCharacterZStageBounds();
                    break;

                case BattleEcsCharacterStageZPassMode.ShadowCompare:
                    CaptureExpected();
                    world.RunLegacyCharacterZStageBounds();
                    ValidateExpected();
                    break;

                case BattleEcsCharacterStageZPassMode.DataOriented:
                    ExecuteDataOriented();
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported character stage-Z pass mode: {mode}.");
            }

            runCount++;
        }

        private void CaptureExpected()
        {
            expectedSlots.ClearAll();
            if (!TryGetStageBounds(out int zMin, out int zMax))
                return;

            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!IsEligible(view, entity))
                    continue;

                double z = ClampZ(entity.Runtime.Z, zMin, zMax);
                expectedSlots.Set(slot);
                expectedGenerations[slot] = view.Generation;
                expectedZBits[slot] = BitConverter.DoubleToInt64Bits(z);
                expectedZInt[slot] = (int)z;
                slotVisitCount++;
            }
        }

        private void ExecuteDataOriented()
        {
            if (!TryGetStageBounds(out int zMin, out int zMax))
                return;

            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!IsEligible(view, entity))
                    continue;

                NTSDEntityRuntime runtime = entity.Runtime;
                runtime.Z = ClampZ(runtime.Z, zMin, zMax);
                runtime.ZInt = (int)runtime.Z;

                // Preserve the legacy pass' complete base-runtime synchronization.
                // Exact production characters can use the non-virtual base path;
                // derived/custom characters retain their virtual refresh contract.
                if (entity.GetType() == typeof(LF2Character))
                {
                    entity.RefreshBaseRuntimeSnapshotForStageBounds();
                }
                else
                {
                    entity.RefreshRuntimeSnapshot();
                    derivedTypeFallbackCount++;
                }

                slotVisitCount++;
            }
        }

        private void ValidateExpected()
        {
            validationCount++;
            for (int slot = expectedSlots.FindNextSet(0);
                 slot >= 0;
                 slot = expectedSlots.FindNextSet(slot + 1))
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!IsEligible(view, entity))
                {
                    RecordMismatch(
                        slot,
                        BattleEcsCharacterStageZMismatchKind.Occupancy);
                    continue;
                }

                if (view.Generation != expectedGenerations[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsCharacterStageZMismatchKind.Generation);
                    continue;
                }

                if (BitConverter.DoubleToInt64Bits(entity.Runtime.Z) !=
                    expectedZBits[slot])
                {
                    RecordMismatch(slot, BattleEcsCharacterStageZMismatchKind.Z);
                    continue;
                }

                if (entity.Runtime.ZInt != expectedZInt[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsCharacterStageZMismatchKind.ZInt);
                }
            }
        }

        private bool IsEligible(
            RuntimeSlotTable.ReadOnlySlotView view,
            LF2Entity entity)
        {
            return view.Claimed &&
                   entity != null &&
                   entity.PS != null &&
                   world.IsActiveForCurrentPassInternal(entity) &&
                   entity.IsStageBoundedCharacter();
        }

        private bool TryGetStageBounds(out int zMin, out int zMax)
        {
            zMin = world.Runtime?.Stage?.ZMin ?? 180;
            zMax = world.Runtime?.Stage?.ZMax ?? 350;
            return zMax >= zMin;
        }

        private static double ClampZ(double z, int zMin, int zMax)
        {
            if (z > zMax)
                z = zMax;
            if (z < zMin)
                z = zMin;
            return z;
        }

        private void RecordMismatch(
            int slot,
            BattleEcsCharacterStageZMismatchKind mismatchKind)
        {
            mismatchCount++;
            if (firstMismatchKind != BattleEcsCharacterStageZMismatchKind.None)
                return;

            firstMismatchSlot = slot;
            firstMismatchKind = mismatchKind;
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            slotVisitCount = 0;
            validationCount = 0;
            mismatchCount = 0;
            derivedTypeFallbackCount = 0;
            firstMismatchSlot = -1;
            firstMismatchKind = BattleEcsCharacterStageZMismatchKind.None;
        }
    }
}
