using System;

using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsPositiveLinkValidationPassMode : byte
    {
        Legacy = 0,
        ShadowCompare = 1,
        DataOriented = 2,
    }

    public enum BattleEcsPositiveLinkValidationMismatchKind : byte
    {
        None = 0,
        Occupancy = 1,
        Generation = 2,
        LinkState = 3,
        TargetSlot = 4,
        HeldWeaponStableId = 5,
    }

    public readonly struct BattleEcsPositiveLinkValidationPassDiagnostics
    {
        internal BattleEcsPositiveLinkValidationPassDiagnostics(
            BattleEcsPositiveLinkValidationPassMode mode,
            long runCount,
            long slotVisitCount,
            long validationCount,
            long mismatchCount,
            long keptCount,
            long clearedCount,
            int firstMismatchSlot,
            BattleEcsPositiveLinkValidationMismatchKind firstMismatchKind)
        {
            Mode = mode;
            RunCount = runCount;
            SlotVisitCount = slotVisitCount;
            ValidationCount = validationCount;
            MismatchCount = mismatchCount;
            KeptCount = keptCount;
            ClearedCount = clearedCount;
            FirstMismatchSlot = firstMismatchSlot;
            FirstMismatchKind = firstMismatchKind;
        }

        public BattleEcsPositiveLinkValidationPassMode Mode { get; }
        public long RunCount { get; }
        public long SlotVisitCount { get; }
        public long ValidationCount { get; }
        public long MismatchCount { get; }
        public long KeptCount { get; }
        public long ClearedCount { get; }
        public int FirstMismatchSlot { get; }
        public BattleEcsPositiveLinkValidationMismatchKind FirstMismatchKind { get; }
        public bool IsClean => MismatchCount == 0;
    }

    /// <summary>
    /// U5/U6 positive-link migration slice. This pass preserves the authority slot
    /// order and reads the live runtime written by cpoint/weapon synchronization
    /// earlier in the same tick; the end-of-tick ECS shadow is not used as truth.
    /// </summary>
    internal sealed class BattleEcsPositiveLinkValidationPass
    {
        private readonly SimulationWorld world;
        private readonly RuntimeSlotTable runtimeSlots;
        private readonly BattleSlotBitSet expectedSlots;
        private readonly uint[] expectedGenerations;
        private readonly int[] expectedLinkState;
        private readonly int[] expectedTargetSlot;
        private readonly int[] expectedHeldWeaponStableId;
        private BattleEcsPositiveLinkValidationPassMode mode =
            BattleEcsPositiveLinkValidationPassMode.DataOriented;
        private long runCount;
        private long slotVisitCount;
        private long validationCount;
        private long mismatchCount;
        private long keptCount;
        private long clearedCount;
        private int firstMismatchSlot = -1;
        private BattleEcsPositiveLinkValidationMismatchKind firstMismatchKind;

        public BattleEcsPositiveLinkValidationPass(
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
            expectedLinkState = new int[capacity];
            expectedTargetSlot = new int[capacity];
            expectedHeldWeaponStableId = new int[capacity];
        }

        public BattleEcsPositiveLinkValidationPassMode Mode => mode;

        public BattleEcsPositiveLinkValidationPassDiagnostics Diagnostics =>
            new BattleEcsPositiveLinkValidationPassDiagnostics(
                mode,
                runCount,
                slotVisitCount,
                validationCount,
                mismatchCount,
                keptCount,
                clearedCount,
                firstMismatchSlot,
                firstMismatchKind);

        public void SetMode(BattleEcsPositiveLinkValidationPassMode requestedMode)
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
                case BattleEcsPositiveLinkValidationPassMode.Legacy:
                    world.RunLegacyPositiveLinkValidation(tickIndex);
                    break;

                case BattleEcsPositiveLinkValidationPassMode.ShadowCompare:
                    CaptureExpected();
                    world.RunLegacyPositiveLinkValidation(tickIndex);
                    ValidateExpected();
                    break;

                case BattleEcsPositiveLinkValidationPassMode.DataOriented:
                    ExecuteDataOriented(tickIndex);
                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported positive-link validation pass mode: {mode}.");
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
                LF2Entity holder = view.Entity;
                if (!TryResolveParticipant(view, holder, out int holderSlot))
                    continue;

                NTSDEntityRuntime runtime = holder.Runtime;
                bool valid = IsReciprocalTarget(runtime.TargetSlotIndex, holderSlot);
                expectedSlots.Set(slot);
                expectedGenerations[slot] = view.Generation;
                expectedLinkState[slot] = valid ? runtime.LinkState : 0;
                expectedTargetSlot[slot] = runtime.TargetSlotIndex;
                expectedHeldWeaponStableId[slot] = runtime.HeldWeaponStableId;
                slotVisitCount++;
            }
        }

        private void ExecuteDataOriented(int tickIndex)
        {
            IBattleParityStructuralEventSink eventSink =
                world.StructuralEventSinkForServices;
            if (eventSink != null)
            {
                world.SetStructuralEventContextForDiagnostics(
                    tickIndex,
                    "positive-link-validation");
            }

            BattleRelationLinkWriter relationLinkWriter =
                world.RelationLinkWriter;
            for (int slot = relationLinkWriter.FindNextPositiveLinkSlot(0);
                 slot >= 0;
                 slot = relationLinkWriter.FindNextPositiveLinkSlot(slot + 1))
            {
                if (!relationLinkWriter.TryGetPositiveLinkHandle(
                        slot,
                        out RuntimeEntityHandle indexedHandle))
                {
                    continue;
                }

                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                LF2Entity holder = view.Entity;
                if (view.Generation != indexedHandle.Generation ||
                    !TryResolveParticipant(view, holder, out int holderSlot))
                {
                    continue;
                }

                NTSDEntityRuntime runtime = holder.Runtime;
                int targetRuntimeSlot = runtime.TargetSlotIndex;
                LF2Entity target = FindActiveEntity(targetRuntimeSlot);
                bool targetActive = target != null;
                int observedHolderSlot = targetActive
                    ? target.Runtime.HolderStableId
                    : -1;
                bool valid = targetActive && observedHolderSlot == holderSlot;

                int beforeLinkState = 0;
                int beforeTargetSlot = -1;
                int beforeHeldWeaponSlot = -1;
                int targetBeforeLinkState = 0;
                if (eventSink != null)
                {
                    beforeLinkState = runtime.LinkState;
                    beforeTargetSlot = runtime.TargetSlotIndex;
                    beforeHeldWeaponSlot = runtime.HeldWeaponStableId;
                    targetBeforeLinkState = targetActive
                        ? target.Runtime.LinkState
                        : 0;
                }

                if (!valid)
                {
                    runtime.LinkState = 0;
                    holder.RefreshRuntimeSnapshot();
                    clearedCount++;
                }
                else
                {
                    keptCount++;
                }

                if (eventSink != null)
                {
                    RecordStructuralEvent(
                        eventSink,
                        tickIndex,
                        holderSlot,
                        holder,
                        target,
                        targetActive,
                        observedHolderSlot,
                        beforeLinkState,
                        beforeTargetSlot,
                        beforeHeldWeaponSlot,
                        targetBeforeLinkState,
                        valid);
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
                LF2Entity holder = view.Entity;
                if (!view.Claimed || holder?.Runtime == null ||
                    !world.IsActiveForCurrentPassInternal(holder))
                {
                    RecordMismatch(
                        slot,
                        BattleEcsPositiveLinkValidationMismatchKind.Occupancy);
                    continue;
                }

                if (view.Generation != expectedGenerations[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsPositiveLinkValidationMismatchKind.Generation);
                    continue;
                }

                NTSDEntityRuntime runtime = holder.Runtime;
                if (runtime.LinkState != expectedLinkState[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsPositiveLinkValidationMismatchKind.LinkState);
                }
                else if (runtime.TargetSlotIndex != expectedTargetSlot[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsPositiveLinkValidationMismatchKind.TargetSlot);
                }
                else if (runtime.HeldWeaponStableId !=
                         expectedHeldWeaponStableId[slot])
                {
                    RecordMismatch(
                        slot,
                        BattleEcsPositiveLinkValidationMismatchKind.HeldWeaponStableId);
                }
            }
        }

        private bool TryResolveParticipant(
            RuntimeSlotTable.ReadOnlySlotView view,
            LF2Entity holder,
            out int holderSlot)
        {
            holderSlot = GetRuntimeSlotOrder(holder);
            return view.Claimed &&
                   holder?.Runtime != null &&
                   world.IsActiveForCurrentPassInternal(holder) &&
                   holderSlot >= 0 &&
                   holderSlot < runtimeSlots.LogicalCapacity &&
                   holder.Runtime.LinkState > 0;
        }

        private bool IsReciprocalTarget(int targetSlot, int holderSlot)
        {
            LF2Entity target = FindActiveEntity(targetSlot);
            return target != null && target.Runtime.HolderStableId == holderSlot;
        }

        private LF2Entity FindActiveEntity(int slot)
        {
            if ((uint)slot >= (uint)runtimeSlots.LogicalCapacity)
                return null;

            LF2Entity entity = runtimeSlots.GetCurrentOccupant(slot);
            return world.IsActiveForCurrentPassInternal(entity) ? entity : null;
        }

        private static int GetRuntimeSlotOrder(LF2Entity entity)
        {
            if (entity == null)
                return int.MaxValue;
            int slot = entity.Runtime?.SlotIndex ?? -1;
            return slot >= 0 ? slot : entity.StableId;
        }

        private static void RecordStructuralEvent(
            IBattleParityStructuralEventSink eventSink,
            int tickIndex,
            int holderSlot,
            LF2Entity holder,
            LF2Entity target,
            bool targetActive,
            int observedHolderSlot,
            int beforeLinkState,
            int beforeTargetSlot,
            int beforeHeldWeaponSlot,
            int targetBeforeLinkState,
            bool valid)
        {
            NTSDEntityRuntime runtime = holder.Runtime;
            int targetAfterHolderSlot = targetActive
                ? target.Runtime.HolderStableId
                : -1;
            int targetAfterLinkState = targetActive
                ? target.Runtime.LinkState
                : 0;
            eventSink.Record(new BattleParityStructuralEvent
            {
                Tick = tickIndex,
                Pass = "positive-link-validation",
                Action = "link-validation",
                CursorSlot = holderSlot,
                ActorSlot = holderSlot,
                Slot = holderSlot,
                Before = $"{beforeLinkState}/{beforeTargetSlot}/{beforeHeldWeaponSlot}",
                After = $"{runtime.LinkState}/{runtime.TargetSlotIndex}/{runtime.HeldWeaponStableId}",
                SourceKind = "positive-link",
                BeforeLinkState = beforeLinkState,
                BeforeTargetSlot = beforeTargetSlot,
                BeforeHeldWeaponSlot = beforeHeldWeaponSlot,
                AfterLinkState = runtime.LinkState,
                AfterTargetSlot = runtime.TargetSlotIndex,
                AfterHeldWeaponSlot = runtime.HeldWeaponStableId,
                TargetActive = targetActive,
                ObservedHolderSlot = observedHolderSlot,
                Outcome = valid ? "kept" : "cleared",
                Reason = valid
                    ? "reciprocal"
                    : targetActive
                        ? "holder-mismatch"
                        : "target-inactive",
                TargetBeforeHolderSlot = observedHolderSlot,
                TargetBeforeLinkState = targetBeforeLinkState,
                TargetAfterHolderSlot = targetAfterHolderSlot,
                TargetAfterLinkState = targetAfterLinkState,
            });
        }

        private void RecordMismatch(
            int slot,
            BattleEcsPositiveLinkValidationMismatchKind mismatchKind)
        {
            mismatchCount++;
            if (firstMismatchKind !=
                BattleEcsPositiveLinkValidationMismatchKind.None)
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
            keptCount = 0;
            clearedCount = 0;
            firstMismatchSlot = -1;
            firstMismatchKind =
                BattleEcsPositiveLinkValidationMismatchKind.None;
        }
    }
}
