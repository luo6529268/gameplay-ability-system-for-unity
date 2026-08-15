using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterInputPassMode : byte
    {
        Legacy = 0,
        DataOriented = 1,
    }

    public readonly struct BattleEcsCharacterInputPassDiagnostics
    {
        internal BattleEcsCharacterInputPassDiagnostics(
            BattleEcsCharacterInputPassMode mode,
            long runCount,
            long exactCharacterCount,
            long compatibilityFallbackCount,
            long unityCompatibilityShellCount,
            long unexpectedFallbackCount)
        {
            Mode = mode;
            RunCount = runCount;
            ExactCharacterCount = exactCharacterCount;
            CompatibilityFallbackCount = compatibilityFallbackCount;
            UnityCompatibilityShellCount = unityCompatibilityShellCount;
            UnexpectedFallbackCount = unexpectedFallbackCount;
        }

        public BattleEcsCharacterInputPassMode Mode { get; }
        public long RunCount { get; }
        public long ExactCharacterCount { get; }
        public long CompatibilityFallbackCount { get; }
        public long UnityCompatibilityShellCount { get; }
        public long UnexpectedFallbackCount { get; }
    }

    /// <summary>
    /// Owns the authority-ordered input orchestration for exact characters.
    /// Unknown derived entities retain the virtual compatibility path.
    /// </summary>
    internal sealed class BattleEcsCharacterInputPass
    {
        private readonly SimulationWorld world;
        // The mode remains reset-boundary configurable so production A/B can retain the same code path.
        private BattleEcsCharacterInputPassMode mode =
            BattleEcsCharacterInputPassMode.DataOriented;
        private long runCount;
        private long exactCharacterCount;
        private long compatibilityFallbackCount;
        private long unityCompatibilityShellCount;
        private long unexpectedFallbackCount;

        internal BattleEcsCharacterInputPass(SimulationWorld world)
        {
            this.world = world;
        }

        internal BattleEcsCharacterInputPassMode Mode => mode;

        internal BattleEcsCharacterInputPassDiagnostics Diagnostics =>
            new BattleEcsCharacterInputPassDiagnostics(
                mode,
                runCount,
                exactCharacterCount,
                compatibilityFallbackCount,
                unityCompatibilityShellCount,
                unexpectedFallbackCount);

        internal void SetMode(BattleEcsCharacterInputPassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        internal void Reset()
        {
            ResetDiagnostics();
        }

        internal bool TryExecute(LF2Entity entity, int tickIndex)
        {
            runCount++;
            if (mode == BattleEcsCharacterInputPassMode.Legacy ||
                entity == null ||
                entity.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                compatibilityFallbackCount++;
                unexpectedFallbackCount++;
                return false;
            }

            if (entity.GetType() != typeof(LF2Character) ||
                !entity.AiControlled)
            {
                compatibilityFallbackCount++;
                unityCompatibilityShellCount++;
                return false;
            }

            exactCharacterCount++;
            var character = (LF2Character)entity;
            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime == null || runtime.LinkState < 0)
                return true;

            BattleAiInputDetailDiagnostics diagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (character.AiControlled)
            {
                diagnostics?.RecordAi();
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.RemainingAiDecision);
                try
                {
                    world.PrepareAiInputBasic(character, tickIndex);
                }
                finally
                {
                    diagnostics?.EndPhase(BattleAiInputDetailPhase.RemainingAiDecision);
                }
            }

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.ComboUpdate);
            try
            {
                world.CharacterInputActionResolver.ApplyFrameInputFromRuntimeProgress(
                    character,
                    world.CharacterInputWriter,
                    diagnostics);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.ComboUpdate);
            }

            world.CharacterActionWriter.TryApplyExactCharacterFrameVelocityTail(character);
            return true;
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            exactCharacterCount = 0;
            compatibilityFallbackCount = 0;
            unityCompatibilityShellCount = 0;
            unexpectedFallbackCount = 0;
        }
    }
}
