using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterPostFrameTailPassMode : byte
    {
        Legacy = 0,
        DataOriented = 1,
    }

    public readonly struct BattleEcsCharacterPostFrameTailPassDiagnostics
    {
        internal BattleEcsCharacterPostFrameTailPassDiagnostics(
            BattleEcsCharacterPostFrameTailPassMode mode,
            long runCount,
            long exactCharacterCount,
            long compatibilityFallbackCount)
        {
            Mode = mode;
            RunCount = runCount;
            ExactCharacterCount = exactCharacterCount;
            CompatibilityFallbackCount = compatibilityFallbackCount;
        }

        public BattleEcsCharacterPostFrameTailPassMode Mode { get; }
        public long RunCount { get; }
        public long ExactCharacterCount { get; }
        public long CompatibilityFallbackCount { get; }
    }

    /// <summary>
    /// Owns the authority post-frame maintenance writes for exact characters.
    /// Derived compatibility shells and non-character entities retain the
    /// virtual entity path.
    /// </summary>
    internal sealed class BattleEcsCharacterPostFrameTailPass
    {
        private BattleEcsCharacterPostFrameTailPassMode mode =
            BattleEcsCharacterPostFrameTailPassMode.Legacy;
        private long runCount;
        private long exactCharacterCount;
        private long compatibilityFallbackCount;

        internal BattleEcsCharacterPostFrameTailPassMode Mode => mode;

        internal BattleEcsCharacterPostFrameTailPassDiagnostics Diagnostics =>
            new BattleEcsCharacterPostFrameTailPassDiagnostics(
                mode,
                runCount,
                exactCharacterCount,
                compatibilityFallbackCount);

        internal void SetMode(
            BattleEcsCharacterPostFrameTailPassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        internal void Reset()
        {
            ResetDiagnostics();
        }

        internal bool TryExecute(LF2Entity entity)
        {
            runCount++;
            if (mode == BattleEcsCharacterPostFrameTailPassMode.Legacy ||
                entity == null ||
                entity.GetType() != typeof(LF2Character) ||
                entity.Runtime == null ||
                entity.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                compatibilityFallbackCount++;
                return false;
            }

            exactCharacterCount++;
            ApplyAuthorityMaintenance(entity.Runtime);
            return true;
        }

        private static void ApplyAuthorityMaintenance(NTSDEntityRuntime runtime)
        {
            runtime.HitConfirm2 = 0;
            runtime.TransientMp = 0;
            runtime.TransientMp2 = 1000;
            runtime.TransientMp3 = 1000;
            runtime.TransientMp4 = 1000;
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            exactCharacterCount = 0;
            compatibilityFallbackCount = 0;
        }
    }
}
