using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCooldownPassMode : byte
    {
        Legacy = 0,
        ShadowCompare = 1,
        DataOriented = 2,
    }

    public enum BattleEcsCooldownMismatchKind : byte
    {
        None = 0,
        Occupancy = 1,
        Generation = 2,
        ARest = 3,
        AttackExempt = 4,
    }

    public readonly struct BattleEcsCooldownPassDiagnostics
    {
        internal BattleEcsCooldownPassDiagnostics(
            BattleEcsCooldownPassMode mode,
            long runCount,
            long slotVisitCount,
            long validationCount,
            long mismatchCount,
            long trackerFallbackCount,
            int firstMismatchSlot,
            BattleEcsCooldownMismatchKind firstMismatchKind)
        {
            Mode = mode;
            RunCount = runCount;
            SlotVisitCount = slotVisitCount;
            ValidationCount = validationCount;
            MismatchCount = mismatchCount;
            TrackerFallbackCount = trackerFallbackCount;
            FirstMismatchSlot = firstMismatchSlot;
            FirstMismatchKind = firstMismatchKind;
        }

        public BattleEcsCooldownPassMode Mode { get; }
        public long RunCount { get; }
        public long SlotVisitCount { get; }
        public long ValidationCount { get; }
        public long MismatchCount { get; }
        public long TrackerFallbackCount { get; }
        public int FirstMismatchSlot { get; }
        public BattleEcsCooldownMismatchKind FirstMismatchKind { get; }
        public bool IsClean => MismatchCount == 0;
    }

    /// <summary>
    /// U4 cooldown migration slice. Legacy remains available as a read-only oracle;
    /// the data-oriented writer touches only the world-owned rest store and runtime.
    /// </summary>
    internal sealed class BattleEcsCooldownPass
    {
        private readonly SimulationWorld world;
        private readonly RuntimeSlotTable runtimeSlots;
        private readonly RuntimeRestStore restStore;
        private readonly BattleSlotBitSet expectedSlots;
        private readonly uint[] expectedGenerations;
        private readonly int[] expectedARest;
        private readonly int[] expectedAttackExempt;
        private BattleEcsCooldownPassMode mode =
            BattleEcsCooldownPassMode.DataOriented;
        private long runCount;
        private long slotVisitCount;
        private long validationCount;
        private long mismatchCount;
        private long trackerFallbackCount;
        private int firstMismatchSlot = -1;
        private BattleEcsCooldownMismatchKind firstMismatchKind;

        public BattleEcsCooldownPass(
            SimulationWorld world,
            RuntimeSlotTable runtimeSlots,
            RuntimeRestStore restStore,
            int capacity)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.runtimeSlots = runtimeSlots ?? throw new ArgumentNullException(nameof(runtimeSlots));
            this.restStore = restStore ?? throw new ArgumentNullException(nameof(restStore));
            if (capacity <= 0 || capacity != runtimeSlots.LogicalCapacity)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            expectedSlots = new BattleSlotBitSet(capacity);
            expectedGenerations = new uint[capacity];
            expectedARest = new int[capacity];
            expectedAttackExempt = new int[capacity];
        }

        public BattleEcsCooldownPassMode Mode => mode;

        public BattleEcsCooldownPassDiagnostics Diagnostics =>
            new BattleEcsCooldownPassDiagnostics(
                mode,
                runCount,
                slotVisitCount,
                validationCount,
                mismatchCount,
                trackerFallbackCount,
                firstMismatchSlot,
                firstMismatchKind);

        public void SetMode(BattleEcsCooldownPassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        public void Reset()
        {
            expectedSlots.ClearAll();
            ResetDiagnostics();
        }

        public void Execute(int tickIndex)
        {
            switch (mode)
            {
                case BattleEcsCooldownPassMode.Legacy:
                    world.VrestTickAll(tickIndex);
                    break;

                case BattleEcsCooldownPassMode.ShadowCompare:
                    CaptureExpected();
                    world.VrestTickAll(tickIndex);
                    ValidateExpected();
                    break;

                case BattleEcsCooldownPassMode.DataOriented:
                    ExecuteDataOriented();
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported cooldown pass mode: {mode}.");
            }

            runCount++;
        }

        private void CaptureExpected()
        {
            expectedSlots.ClearAll();
            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view = runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!view.Claimed || entity == null ||
                    !world.IsActiveForCurrentPassInternal(entity))
                {
                    continue;
                }

                expectedSlots.Set(slot);
                expectedGenerations[slot] = view.Generation;
                int arest = restStore.GetARest(slot);
                expectedARest[slot] = arest > 0 ? arest - 1 : 0;
                expectedAttackExempt[slot] = ResolveAttackExemptAfterCooldown(entity);
                slotVisitCount++;
            }
        }

        private void ExecuteDataOriented()
        {
            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view = runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!view.Claimed || entity == null ||
                    !world.IsActiveForCurrentPassInternal(entity))
                {
                    continue;
                }

                LF2ItrRestTracker tracker = entity.ItrRest;
                if (tracker != null && tracker.IsBoundTo(restStore, slot))
                {
                    restStore.TickARest(slot);
                }
                else
                {
                    tracker?.TickArest();
                    trackerFallbackCount++;
                }

                entity.AttackExempt = ResolveAttackExemptAfterCooldown(entity);
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
                RuntimeSlotTable.ReadOnlySlotView view = runtimeSlots.GetReadOnlyView(slot);
                if (!view.Claimed || view.Entity == null ||
                    !world.IsActiveForCurrentPassInternal(view.Entity))
                {
                    RecordMismatch(slot, BattleEcsCooldownMismatchKind.Occupancy);
                    continue;
                }

                if (view.Generation != expectedGenerations[slot])
                {
                    RecordMismatch(slot, BattleEcsCooldownMismatchKind.Generation);
                    continue;
                }

                if (restStore.GetARest(slot) != expectedARest[slot])
                {
                    RecordMismatch(slot, BattleEcsCooldownMismatchKind.ARest);
                    continue;
                }

                if (view.Entity.AttackExempt != expectedAttackExempt[slot])
                    RecordMismatch(slot, BattleEcsCooldownMismatchKind.AttackExempt);
            }
        }

        private int ResolveAttackExemptAfterCooldown(LF2Entity entity)
        {
            int current = entity?.AttackExempt ?? 0;
            if (entity == null || current <= 0)
                return current;

            LF2CharacterData entityData =
                (entity as LF2LivingObject)?._FrameDataWrapper?.characterData
                ?? entity.FrameCache?.Wrapper?.characterData;
            if (entityData == null)
                return current;

            LF2FrameData frame = entity.Frame?.D;
            bool clear = frame?.itrs == null || frame.itrs.Count == 0;
            if (!clear &&
                frame.state == LF2States.WeaponOnHand &&
                entity.Runtime != null)
            {
                int holderSlot = entity.Runtime.ResolveActiveHolderSlotIndex();
                LF2Entity holder = holderSlot >= 0
                    ? world.FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                LF2CharacterData holderData =
                    (holder as LF2LivingObject)?._FrameDataWrapper?.characterData
                    ?? holder?.FrameCache?.Wrapper?.characterData;
                if (holder != null && holderData != null)
                {
                    LF2FrameData holderFrame = holder.Frame?.D;
                    clear = holderFrame?.wpoints == null ||
                            holderFrame.wpoints.Count == 0 ||
                            holderFrame.wpoints[0].attacking == 0;
                }
            }

            return clear ? 0 : current;
        }

        private void RecordMismatch(
            int slot,
            BattleEcsCooldownMismatchKind mismatchKind)
        {
            mismatchCount++;
            if (firstMismatchKind != BattleEcsCooldownMismatchKind.None)
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
            trackerFallbackCount = 0;
            firstMismatchSlot = -1;
            firstMismatchKind = BattleEcsCooldownMismatchKind.None;
        }
    }
}
