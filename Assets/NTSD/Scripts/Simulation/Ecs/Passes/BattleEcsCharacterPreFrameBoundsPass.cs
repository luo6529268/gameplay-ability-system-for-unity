using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterPreFrameBoundsPassMode : byte
    {
        Legacy = 0,
        DataOriented = 1,
    }

    public readonly struct BattleEcsCharacterPreFrameBoundsPassDiagnostics
    {
        internal BattleEcsCharacterPreFrameBoundsPassDiagnostics(
            BattleEcsCharacterPreFrameBoundsPassMode mode,
            long runCount,
            long slotVisitCount,
            long exactCharacterWriteCount,
            long compatibilityFallbackCount)
        {
            Mode = mode;
            RunCount = runCount;
            SlotVisitCount = slotVisitCount;
            ExactCharacterWriteCount = exactCharacterWriteCount;
            CompatibilityFallbackCount = compatibilityFallbackCount;
        }

        public BattleEcsCharacterPreFrameBoundsPassMode Mode { get; }
        public long RunCount { get; }
        public long SlotVisitCount { get; }
        public long ExactCharacterWriteCount { get; }
        public long CompatibilityFallbackCount { get; }
    }

    /// <summary>
    /// Applies the authority PreFrame X/Z writes directly for exact production
    /// characters. Non-character identities and derived compatibility shells keep
    /// the existing virtual path, including its destruction semantics.
    /// </summary>
    internal sealed class BattleEcsCharacterPreFrameBoundsPass
    {
        private readonly SimulationWorld world;
        private readonly RuntimeSlotTable runtimeSlots;
        private BattleEcsCharacterPreFrameBoundsPassMode mode =
            BattleEcsCharacterPreFrameBoundsPassMode.DataOriented;
        private long runCount;
        private long slotVisitCount;
        private long exactCharacterWriteCount;
        private long compatibilityFallbackCount;

        internal BattleEcsCharacterPreFrameBoundsPass(
            SimulationWorld world,
            RuntimeSlotTable runtimeSlots)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.runtimeSlots = runtimeSlots ??
                throw new ArgumentNullException(nameof(runtimeSlots));
        }

        internal BattleEcsCharacterPreFrameBoundsPassMode Mode => mode;

        internal BattleEcsCharacterPreFrameBoundsPassDiagnostics Diagnostics =>
            new BattleEcsCharacterPreFrameBoundsPassDiagnostics(
                mode,
                runCount,
                slotVisitCount,
                exactCharacterWriteCount,
                compatibilityFallbackCount);

        internal void SetMode(BattleEcsCharacterPreFrameBoundsPassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        internal void Reset()
        {
            ResetDiagnostics();
        }

        internal void Execute()
        {
            if (mode == BattleEcsCharacterPreFrameBoundsPassMode.Legacy)
            {
                world.RunLegacyPreFrameBoundsAll();
                runCount++;
                return;
            }

            if (mode != BattleEcsCharacterPreFrameBoundsPassMode.DataOriented)
            {
                throw new InvalidOperationException(
                    $"Unsupported character PreFrame bounds pass mode: {mode}.");
            }

            ExecuteDataOriented();
            runCount++;
        }

        private void ExecuteDataOriented()
        {
            int baseStageWidth = world.Runtime?.Stage?.BaseStageWidthPx ?? 800;
            int xMaxOverride = world.Runtime?.Stage?.XMaxOverride ?? 0;
            if (!world.TryGetStageRuleDepthBounds(out double sourceMin,
                    out double sourceMax, out double viewMin, out double viewMax) ||
                baseStageWidth <= 0)
                return;

            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                LF2Entity entity = view.Entity;
                if (!view.Claimed ||
                    entity == null ||
                    entity.PS == null ||
                    !world.IsActiveForCurrentPassInternal(entity))
                {
                    continue;
                }

                slotVisitCount++;
                if (TryApplyExactCharacter(
                        slot,
                        view.Generation,
                        entity,
                        baseStageWidth,
                        xMaxOverride,
                        sourceMin,
                        sourceMax,
                        viewMin,
                        viewMax))
                {
                    exactCharacterWriteCount++;
                    continue;
                }

                entity.ApplyPreFrameZBounds(sourceMin, sourceMax, viewMin, viewMax);
                bool destroyed = entity.ApplyPreFrameXBounds(
                    baseStageWidth,
                    xMaxOverride);
                if (!destroyed)
                    entity.RefreshRuntimeSnapshot();
                compatibilityFallbackCount++;
            }
        }

        private bool TryApplyExactCharacter(
            int slot,
            uint generation,
            LF2Entity entity,
            int baseStageWidth,
            int xMaxOverride,
            double sourceMin,
            double sourceMax,
            double viewMin,
            double viewMax)
        {
            if (generation == 0 ||
                entity.GetType() != typeof(LF2Character) ||
                entity.Runtime == null ||
                entity.Runtime.SlotIndex != slot ||
                !world.IdentityWriter.TryCaptureAiProjection(
                    new RuntimeEntityHandle(slot, generation),
                    out BattleIdentityAiProjection identity) ||
                identity.DataObjectType != (int)LF2ObjectType.Character)
            {
                return false;
            }

            NTSDEntityRuntime runtime = entity.Runtime;
            runtime.ClampStageZ(
                sourceMin,
                sourceMax,
                world.FixedViewRunVerticalDistanceScale,
                viewMin,
                viewMax);

            int selectedModeStageGate50 =
                world.Runtime?.SelectedModeStageGate50 ?? 0;
            double x = NTSDEntityRuntime.ClampSelectedModeType0StageX(
                runtime.X, slot, runtime.RelationTeam, runtime.HitStop,
                baseStageWidth, xMaxOverride, selectedModeStageGate50);
            runtime.X = x;
            runtime.XInt = (int)x;
            // Alignment contract: NTSD28-USER-SOURCE-CHARACTER-STAGE-X-001.
            runtime.ClampSourceRuleCharacterX(
                slot, runtime.RelationTeam, runtime.HitStop,
                baseStageWidth, xMaxOverride, selectedModeStageGate50);
            return true;
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            slotVisitCount = 0;
            exactCharacterWriteCount = 0;
            compatibilityFallbackCount = 0;
        }
    }
}
