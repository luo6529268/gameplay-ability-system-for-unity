using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterFrameAdvancePassMode : byte
    {
        Legacy = 0,
        DataOriented = 1,
    }

    public readonly struct BattleEcsCharacterFrameAdvancePassDiagnostics
    {
        internal BattleEcsCharacterFrameAdvancePassDiagnostics(
            BattleEcsCharacterFrameAdvancePassMode mode,
            long runCount,
            long exactCharacterCount,
            long compatibilityFallbackCount)
        {
            Mode = mode;
            RunCount = runCount;
            ExactCharacterCount = exactCharacterCount;
            CompatibilityFallbackCount = compatibilityFallbackCount;
        }

        public BattleEcsCharacterFrameAdvancePassMode Mode { get; }
        public long RunCount { get; }
        public long ExactCharacterCount { get; }
        public long CompatibilityFallbackCount { get; }
    }

    /// <summary>
    /// Owns the canonical exact-character FrameAdvance orchestration. Unknown
    /// derived entities and character shells backed by non-character DAT retain
    /// the virtual compatibility path.
    /// </summary>
    internal sealed class BattleEcsCharacterFrameAdvancePass
    {
        private readonly SimulationWorld world;
        private BattleEcsCharacterFrameAdvancePassMode mode =
            BattleEcsCharacterFrameAdvancePassMode.DataOriented;
        private long runCount;
        private long exactCharacterCount;
        private long compatibilityFallbackCount;

        internal BattleEcsCharacterFrameAdvancePass(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public BattleEcsCharacterFrameAdvancePassMode Mode => mode;

        public BattleEcsCharacterFrameAdvancePassDiagnostics Diagnostics =>
            new BattleEcsCharacterFrameAdvancePassDiagnostics(
                mode,
                runCount,
                exactCharacterCount,
                compatibilityFallbackCount);

        public void SetMode(BattleEcsCharacterFrameAdvancePassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        public void Reset()
        {
            ResetDiagnostics();
        }

        public bool TryExecute(LF2Entity entity, int tickIndex)
        {
            runCount++;
            if (mode == BattleEcsCharacterFrameAdvancePassMode.Legacy)
            {
                compatibilityFallbackCount++;
                return false;
            }

            if (entity == null || entity.GetType() != typeof(LF2Character))
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
            if (!character.TryEnterReleaseFrameAdvanceAfterDelay())
                return true;
            if (character.IsBlockedByReleaseLinkOrCaughtCpoint())
                return true;

            LF2FrameData frame = character.Frame?.D;
            if (frame != null &&
                frame.HasPrimaryCatchPoint &&
                frame.PrimaryCatchPoint.Kind == 2)
                return true;

            ExecuteCharacterDynamics(character, frame, tickIndex);
            character.ResetWeaponCountOutsideState12FrameAdvanceTail();
            return true;
        }

        private void ExecuteCharacterDynamics(
            LF2Character character,
            LF2FrameData frame,
            int tickIndex)
        {
            NTSDEntityRuntime runtime = character.Runtime;
            var context = new CharacterMechanicsContext(
                runtime,
                frame,
                0f,
                NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                world.FixedViewRunDistanceScale,
                world.FixedViewRunVerticalDistanceScale);

            BattleMechanicsStepResult stepResult =
                world.CharacterMechanicsForServices.StepBattleLogic(context);

            world.BoundaryWriter.SyncConsumedFlags(runtime);
            // Alignment contract: NTSD28-Q06-CANONICAL-CHARACTER-PHYSICS-TAIL-001.
            character.ApplyCurrentDatType0State1218EnvironmentDamage(
                character.Frame?.D,
                stepResult);
            bool state1218ContactResolved =
                character.ApplyCurrentDatType0State1218ContactAction(
                    character.Frame?.D,
                    stepResult);
            if (!state1218ContactResolved &&
                character.ShouldResolveCharacterLanding(stepResult))
            {
                if (!character.ApplyCurrentDatType0OrdinaryLanding(
                        character.Frame?.D,
                        stepResult))
                {
                    character.HandleLandingEventForFrameAdvance(
                        stepResult.VerticalVelocityBeforeLanding);
                }
            }

            character.ApplyCurrentDatType0AirborneAction(
                character.Frame?.D,
                stepResult,
                tickIndex);
            runtime.SyncIntegerPosition();
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            exactCharacterCount = 0;
            compatibilityFallbackCount = 0;
        }
    }
}
