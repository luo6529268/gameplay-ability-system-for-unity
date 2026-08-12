using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsFramePostProcessPassMode : byte
    {
        Legacy = 0,
        ShadowCompare = 1,
        DataOriented = 2,
    }

    public enum BattleEcsFramePostProcessMismatchKind : byte
    {
        None = 0,
        Occupancy = 1,
        Generation = 2,
        Vx = 3,
        Vy = 4,
        Vz = 5,
        HitCount = 6,
        KnockbackVx = 7,
        KnockbackVy = 8,
        KnockbackVz = 9,
    }

    public readonly struct BattleEcsFramePostProcessPassDiagnostics
    {
        internal BattleEcsFramePostProcessPassDiagnostics(
            BattleEcsFramePostProcessPassMode mode,
            long runCount,
            long slotVisitCount,
            long validationCount,
            long mismatchCount,
            int firstMismatchSlot,
            BattleEcsFramePostProcessMismatchKind firstMismatchKind)
        {
            Mode = mode;
            RunCount = runCount;
            SlotVisitCount = slotVisitCount;
            ValidationCount = validationCount;
            MismatchCount = mismatchCount;
            FirstMismatchSlot = firstMismatchSlot;
            FirstMismatchKind = firstMismatchKind;
        }

        public BattleEcsFramePostProcessPassMode Mode { get; }
        public long RunCount { get; }
        public long SlotVisitCount { get; }
        public long ValidationCount { get; }
        public long MismatchCount { get; }
        public int FirstMismatchSlot { get; }
        public BattleEcsFramePostProcessMismatchKind FirstMismatchKind { get; }
        public bool IsClean => MismatchCount == 0;
    }

    /// <summary>
    /// U4 frame-postprocess migration slice. The data path writes only the
    /// runtime-backed velocity, hit-count and accumulated knockback fields.
    /// </summary>
    internal sealed class BattleEcsFramePostProcessPass
    {
        private readonly SimulationWorld world;
        private readonly RuntimeSlotTable runtimeSlots;
        private readonly BattleSlotBitSet expectedSlots;
        private readonly uint[] expectedGenerations;
        private readonly long[] expectedVxBits;
        private readonly long[] expectedVyBits;
        private readonly long[] expectedVzBits;
        private readonly int[] expectedHitCount;
        private readonly long[] expectedKnockbackVxBits;
        private readonly long[] expectedKnockbackVyBits;
        private readonly long[] expectedKnockbackVzBits;
        private BattleEcsFramePostProcessPassMode mode =
            BattleEcsFramePostProcessPassMode.Legacy;
        private long runCount;
        private long slotVisitCount;
        private long validationCount;
        private long mismatchCount;
        private int firstMismatchSlot = -1;
        private BattleEcsFramePostProcessMismatchKind firstMismatchKind;

        public BattleEcsFramePostProcessPass(
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
            expectedVxBits = new long[capacity];
            expectedVyBits = new long[capacity];
            expectedVzBits = new long[capacity];
            expectedHitCount = new int[capacity];
            expectedKnockbackVxBits = new long[capacity];
            expectedKnockbackVyBits = new long[capacity];
            expectedKnockbackVzBits = new long[capacity];
        }

        public BattleEcsFramePostProcessPassMode Mode => mode;

        public BattleEcsFramePostProcessPassDiagnostics Diagnostics =>
            new BattleEcsFramePostProcessPassDiagnostics(
                mode,
                runCount,
                slotVisitCount,
                validationCount,
                mismatchCount,
                firstMismatchSlot,
                firstMismatchKind);

        public void SetMode(BattleEcsFramePostProcessPassMode requestedMode)
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
                case BattleEcsFramePostProcessPassMode.Legacy:
                    world.FramePostProcessAll();
                    break;

                case BattleEcsFramePostProcessPassMode.ShadowCompare:
                    CaptureExpected();
                    world.FramePostProcessAll();
                    ValidateExpected();
                    break;

                case BattleEcsFramePostProcessPassMode.DataOriented:
                    ExecuteDataOriented();
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported frame-postprocess pass mode: {mode}.");
            }

            runCount++;
        }

        private void CaptureExpected()
        {
            expectedSlots.ClearAll();
            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!IsEligible(view, entity))
                    continue;

                NTSDEntityRuntime runtime = entity.Runtime;
                double vx = runtime.Vx;
                double vy = runtime.Vy;
                double vz = runtime.Vz;
                int hitCount = runtime.HitCount;
                if (hitCount > 0)
                {
                    double divisor = hitCount + 1.0;
                    vx = runtime.KnockbackVx * 2.0 / divisor;
                    vy = runtime.KnockbackVy * 2.0 / divisor;
                    vz = runtime.KnockbackVz * 2.0 / divisor;
                    hitCount = 0;
                }

                expectedSlots.Set(slot);
                expectedGenerations[slot] = view.Generation;
                expectedVxBits[slot] = BitConverter.DoubleToInt64Bits(vx);
                expectedVyBits[slot] = BitConverter.DoubleToInt64Bits(vy);
                expectedVzBits[slot] = BitConverter.DoubleToInt64Bits(vz);
                expectedHitCount[slot] = hitCount;
                expectedKnockbackVxBits[slot] = 0L;
                expectedKnockbackVyBits[slot] = 0L;
                expectedKnockbackVzBits[slot] = 0L;
                slotVisitCount++;
            }
        }

        private void ExecuteDataOriented()
        {
            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!IsEligible(view, entity))
                    continue;

                Apply(entity.Runtime);
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
                        BattleEcsFramePostProcessMismatchKind.Occupancy);
                    continue;
                }

                if (view.Generation != expectedGenerations[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsFramePostProcessMismatchKind.Generation);
                    continue;
                }

                NTSDEntityRuntime runtime = entity.Runtime;
                if (!Matches(runtime.Vx, expectedVxBits[slot]))
                {
                    RecordMismatch(slot, BattleEcsFramePostProcessMismatchKind.Vx);
                }
                else if (!Matches(runtime.Vy, expectedVyBits[slot]))
                {
                    RecordMismatch(slot, BattleEcsFramePostProcessMismatchKind.Vy);
                }
                else if (!Matches(runtime.Vz, expectedVzBits[slot]))
                {
                    RecordMismatch(slot, BattleEcsFramePostProcessMismatchKind.Vz);
                }
                else if (runtime.HitCount != expectedHitCount[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsFramePostProcessMismatchKind.HitCount);
                }
                else if (!Matches(
                    runtime.KnockbackVx,
                    expectedKnockbackVxBits[slot]))
                {
                    RecordMismatch(
                        slot,
                        BattleEcsFramePostProcessMismatchKind.KnockbackVx);
                }
                else if (!Matches(
                    runtime.KnockbackVy,
                    expectedKnockbackVyBits[slot]))
                {
                    RecordMismatch(
                        slot,
                        BattleEcsFramePostProcessMismatchKind.KnockbackVy);
                }
                else if (!Matches(
                    runtime.KnockbackVz,
                    expectedKnockbackVzBits[slot]))
                {
                    RecordMismatch(
                        slot,
                        BattleEcsFramePostProcessMismatchKind.KnockbackVz);
                }
            }
        }

        private bool IsEligible(
            RuntimeSlotTable.ReadOnlySlotView view,
            LF2Entity entity)
        {
            return view.Claimed &&
                   entity != null &&
                   entity.Runtime != null &&
                   world.IsActiveForCurrentPassInternal(entity) &&
                   entity.Runtime.FrameDelay == 0;
        }

        private static void Apply(NTSDEntityRuntime runtime)
        {
            if (runtime.HitCount > 0)
            {
                double divisor = runtime.HitCount + 1.0;
                runtime.Vx = runtime.KnockbackVx * 2.0 / divisor;
                runtime.Vy = runtime.KnockbackVy * 2.0 / divisor;
                runtime.Vz = runtime.KnockbackVz * 2.0 / divisor;
                runtime.HitCount = 0;
            }

            runtime.KnockbackVx = 0.0;
            runtime.KnockbackVy = 0.0;
            runtime.KnockbackVz = 0.0;
        }

        private static bool Matches(double value, long expectedBits)
        {
            return BitConverter.DoubleToInt64Bits(value) == expectedBits;
        }

        private void RecordMismatch(
            int slot,
            BattleEcsFramePostProcessMismatchKind mismatchKind)
        {
            mismatchCount++;
            if (firstMismatchKind !=
                BattleEcsFramePostProcessMismatchKind.None)
            {
                return;
            }

            firstMismatchSlot = slot;
            firstMismatchKind = mismatchKind;
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            slotVisitCount = 0;
            validationCount = 0;
            mismatchCount = 0;
            firstMismatchSlot = -1;
            firstMismatchKind = BattleEcsFramePostProcessMismatchKind.None;
        }
    }
}
