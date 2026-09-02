using System;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsShadowMode : byte
    {
        Disabled = 0,
        Compare = 1,
    }

    public readonly struct BattleEcsShadowDiagnostics
    {
        internal BattleEcsShadowDiagnostics(
            BattleEcsShadowMode mode,
            int capturedTick,
            long captureCount,
            long slotVisitCount,
            long validationCount,
            long mismatchCount,
            long exceptionCount,
            int firstMismatchSlot,
            BattleEcsShadowMismatchKind firstMismatchKind)
        {
            Mode = mode;
            CapturedTick = capturedTick;
            CaptureCount = captureCount;
            SlotVisitCount = slotVisitCount;
            ValidationCount = validationCount;
            MismatchCount = mismatchCount;
            ExceptionCount = exceptionCount;
            FirstMismatchSlot = firstMismatchSlot;
            FirstMismatchKind = firstMismatchKind;
        }

        public BattleEcsShadowMode Mode { get; }
        public int CapturedTick { get; }
        public long CaptureCount { get; }
        public long SlotVisitCount { get; }
        public long ValidationCount { get; }
        public long MismatchCount { get; }
        public long ExceptionCount { get; }
        public int FirstMismatchSlot { get; }
        public BattleEcsShadowMismatchKind FirstMismatchKind { get; }
        public bool IsClean => MismatchCount == 0 && ExceptionCount == 0;
    }

    /// <summary>
    /// U3 read-only migration shadow. It copies the canonical slot runtime into
    /// fixed-capacity data stores and compares it without ever writing back.
    /// </summary>
    internal sealed class BattleEcsShadowModule
    {
        private readonly SimulationWorld world;
        private readonly BattleEcsWorld shadow;
        private BattleEcsShadowMode mode;
        private long captureCount;
        private long slotVisitCount;
        private long validationCount;
        private long mismatchCount;
        private long exceptionCount;
        private int firstMismatchSlot = -1;
        private BattleEcsShadowMismatchKind firstMismatchKind;

        public BattleEcsShadowModule(
            SimulationWorld world,
            BattleEcsCapacityProfile capacityProfile)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            shadow = new BattleEcsWorld(capacityProfile);
        }

        public BattleEcsCapacityProfile CapacityProfile => shadow.CapacityProfile;
        public BattleEcsShadowMode Mode => mode;

        public BattleEcsShadowDiagnostics Diagnostics =>
            new BattleEcsShadowDiagnostics(
                mode,
                shadow.CapturedTick,
                captureCount,
                slotVisitCount,
                validationCount,
                mismatchCount,
                exceptionCount,
                firstMismatchSlot,
                firstMismatchKind);

        public void SetMode(BattleEcsShadowMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        public void Reset()
        {
            shadow.BeginCapture(-1, world.RuntimeSlotOccupancyEpochForServices);
            ResetDiagnostics();
        }

        public void CaptureAndCompareNoThrow(int tickIndex)
        {
            if (mode == BattleEcsShadowMode.Disabled)
                return;

            try
            {
                Capture(tickIndex);
                Validate();
            }
            catch
            {
                exceptionCount++;
                RecordFirstMismatch(-1, BattleEcsShadowMismatchKind.CaptureException);
            }
        }

        public void Capture(int tickIndex)
        {
            ulong occupancyEpoch = world.RuntimeSlotOccupancyEpochForServices;
            shadow.BeginCapture(tickIndex, occupancyEpoch);
            int capacity = shadow.CapacityProfile.SlotCapacity;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view))
                {
                    throw new InvalidOperationException(
                        "The ECS shadow capacity exceeds the canonical runtime capacity.");
                }

                shadow.CaptureSlot(slot, view.Claimed, view.Generation, view.Entity);
                slotVisitCount++;
            }

            captureCount++;
        }

        public bool Validate()
        {
            validationCount++;
            bool clean = true;
            if (shadow.CapturedOccupancyEpoch != world.RuntimeSlotOccupancyEpochForServices)
            {
                RecordMismatch(-1, BattleEcsShadowMismatchKind.OccupancyEpoch);
                clean = false;
            }

            int capacity = shadow.CapacityProfile.SlotCapacity;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view))
                {
                    RecordMismatch(slot, BattleEcsShadowMismatchKind.EntityReference);
                    clean = false;
                    continue;
                }

                if (!shadow.MatchesCanonicalSlot(
                        slot,
                        view.Claimed,
                        view.Generation,
                        view.Entity,
                        out BattleEcsShadowMismatchKind mismatchKind))
                {
                    RecordMismatch(slot, mismatchKind);
                    clean = false;
                }
            }

            return clean;
        }

        public bool TryGetEntityView(int slot, out BattleEcsShadowEntityView view)
        {
            return shadow.TryGetEntityView(slot, out view);
        }

        public int FindNextActiveSlot(int startSlot)
        {
            return shadow.FindNextActiveSlot(startSlot);
        }

        private void RecordMismatch(int slot, BattleEcsShadowMismatchKind mismatchKind)
        {
            mismatchCount++;
            RecordFirstMismatch(slot, mismatchKind);
        }

        private void RecordFirstMismatch(int slot, BattleEcsShadowMismatchKind mismatchKind)
        {
            if (firstMismatchKind != BattleEcsShadowMismatchKind.None)
                return;

            firstMismatchSlot = slot;
            firstMismatchKind = mismatchKind;
        }

        private void ResetDiagnostics()
        {
            captureCount = 0;
            slotVisitCount = 0;
            validationCount = 0;
            mismatchCount = 0;
            exceptionCount = 0;
            firstMismatchSlot = -1;
            firstMismatchKind = BattleEcsShadowMismatchKind.None;
        }
    }
}
