namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns canonical AI input, shared flow and RNG commits after the value-only
    /// decision kernel has completed. The kernel remains pure; this writer is the
    /// single composition boundary that publishes its result to the battle world.
    /// </summary>
    internal sealed class BattleAiInputWriter
    {
        private readonly SimulationWorld world;
        private readonly BattleCharacterInputWriter characterInputWriter;

        internal BattleAiInputWriter(
            SimulationWorld world,
            BattleCharacterInputWriter characterInputWriter)
        {
            this.world = world;
            this.characterInputWriter = characterInputWriter;
        }

        internal bool CommitIndexedCanonicalDecision(
            NTSDEntityRuntime runtime,
            in AiDecisionWitness witness)
        {
            BattleFlowRuntimeState flow = world.Runtime?.Flow;
            if (runtime == null || flow == null)
                return false;

            bool synchronized = witness.TryGetSynchronizedRngCursor(
                out NTSD28SynchronizedRandomCursor synchronizedCursor);
            if (synchronized)
            {
                if (world.NativeRandom == null ||
                    !world.NativeRandom.TryCommitSynchronizedCursor(
                        synchronizedCursor))
                {
                    return false;
                }
            }
            else if (world.Rng == null)
            {
                return false;
            }

            ref readonly AiDecisionInputState input = ref witness.Input;
            characterInputWriter.CommitAiDecisionState(runtime, input);
            runtime.Unk360 = input.Unk360;
            runtime.Unk3FC = input.Unk3FC;
            runtime.Unk400 = input.Unk400;

            ref readonly AiDecisionWorldState decisionWorld = ref witness.World;
            flow.AiDifficulty = decisionWorld.FlowAiDifficulty;
            flow.AiRand3 = decisionWorld.FlowRand3;
            flow.AiRand5 = decisionWorld.FlowRand5;
            flow.AiRand15 = decisionWorld.FlowRand15;
            flow.AiRand20 = decisionWorld.FlowRand20;
            flow.AiMoveMode = decisionWorld.FlowMoveMode;
            flow.AiStageTargetX = decisionWorld.FlowStageTargetX;
            if (!synchronized)
                world.Rng.RestoreState(witness.RngState, witness.RngCalls);
            return true;
        }

        internal void SetCoordinateTarget(
            NTSDEntityRuntime runtime,
            int x,
            int z)
        {
            if (runtime == null)
                return;

            characterInputWriter.SetCoordinateTarget(runtime, x, z);
            runtime.Unk3FC = x;
            runtime.Unk400 = z;
        }
    }
}
