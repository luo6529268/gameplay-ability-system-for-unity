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
            int zMin = world.Runtime?.Stage?.ZMin ?? 180;
            int zMax = world.Runtime?.Stage?.ZMax ?? 350;
            if (zMax < zMin || baseStageWidth <= 0)
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
                        zMin,
                        zMax))
                {
                    exactCharacterWriteCount++;
                    continue;
                }

                entity.ApplyPreFrameZBounds(zMin, zMax);
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
            int zMin,
            int zMax)
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
            double z = runtime.Z;
            if (z < zMin)
                z = zMin;
            if (z > zMax)
                z = zMax;
            runtime.Z = z;
            runtime.ZInt = (int)z;

            double x = runtime.X;
            if (slot >= 20)
            {
                if (x < -100.0)
                    x = -100.0;
                if (x > baseStageWidth + 100.0)
                    x = baseStageWidth + 100.0;
            }
            else
            {
                int relationTeam = runtime.RelationTeam;
                if (relationTeam == 5)
                {
                    if (x < -300.0)
                        x = -300.0;
                }
                else if (x < 0.0)
                {
                    x = 0.0;
                }

                if (x > baseStageWidth)
                    x = baseStageWidth;
                if (xMaxOverride > 0 &&
                    x > xMaxOverride &&
                    relationTeam != 5 &&
                    runtime.HitStop == 0)
                {
                    x = xMaxOverride;
                }
            }

            runtime.X = x;
            runtime.XInt = (int)x;
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
