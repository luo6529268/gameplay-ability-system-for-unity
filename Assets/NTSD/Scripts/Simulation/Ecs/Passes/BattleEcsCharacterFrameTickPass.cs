using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterFrameTickPassMode : byte
    {
        Legacy = 0,
        DataOriented = 1,
    }

    public readonly struct BattleEcsCharacterFrameTickPassDiagnostics
    {
        internal BattleEcsCharacterFrameTickPassDiagnostics(
            BattleEcsCharacterFrameTickPassMode mode,
            long runCount,
            long exactCharacterCount,
            long compatibilityFallbackCount)
        {
            Mode = mode;
            RunCount = runCount;
            ExactCharacterCount = exactCharacterCount;
            CompatibilityFallbackCount = compatibilityFallbackCount;
        }

        public BattleEcsCharacterFrameTickPassMode Mode { get; }
        public long RunCount { get; }
        public long ExactCharacterCount { get; }
        public long CompatibilityFallbackCount { get; }
    }

    /// <summary>
    /// Owns the authority-ordered FrameTick orchestration for exact characters.
    /// Unknown derived entities and non-character DAT shells retain the virtual
    /// compatibility path.
    /// </summary>
    internal sealed class BattleEcsCharacterFrameTickPass
    {
        private BattleEcsCharacterFrameTickPassMode mode =
            BattleEcsCharacterFrameTickPassMode.DataOriented;
        private long runCount;
        private long exactCharacterCount;
        private long compatibilityFallbackCount;

        internal BattleEcsCharacterFrameTickPassMode Mode => mode;

        internal BattleEcsCharacterFrameTickPassDiagnostics Diagnostics =>
            new BattleEcsCharacterFrameTickPassDiagnostics(
                mode,
                runCount,
                exactCharacterCount,
                compatibilityFallbackCount);

        internal void SetMode(BattleEcsCharacterFrameTickPassMode requestedMode)
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
            if (mode == BattleEcsCharacterFrameTickPassMode.Legacy ||
                entity == null ||
                entity.GetType() != typeof(LF2Character))
            {
                compatibilityFallbackCount++;
                return false;
            }

            var character = (LF2Character)entity;
            if (character.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                compatibilityFallbackCount++;
                return false;
            }

            exactCharacterCount++;
            ExecuteExactCharacter(character);
            return true;
        }

        private static void ExecuteExactCharacter(LF2Character character)
        {
            character.RunNativeC25FrameBodyForWorldPass();
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            exactCharacterCount = 0;
            compatibilityFallbackCount = 0;
        }
    }
}
