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

        internal void CommitIndexedCanonicalDecision(
            NTSDEntityRuntime runtime,
            in AiDecisionWitness witness)
        {
            ref readonly AiDecisionInputState input = ref witness.Input;
            characterInputWriter.CommitAiDecisionState(runtime, input);
            runtime.Unk360 = input.Unk360;
            runtime.Unk3FC = input.Unk3FC;
            runtime.Unk400 = input.Unk400;

            ref readonly AiDecisionWorldState decisionWorld = ref witness.World;
            BattleFlowRuntimeState flow = world.Runtime.Flow;
            flow.AiDifficulty = decisionWorld.FlowAiDifficulty;
            flow.AiRand3 = decisionWorld.FlowRand3;
            flow.AiRand5 = decisionWorld.FlowRand5;
            flow.AiRand15 = decisionWorld.FlowRand15;
            flow.AiRand20 = decisionWorld.FlowRand20;
            flow.AiMoveMode = decisionWorld.FlowMoveMode;
            flow.AiStageTargetX = decisionWorld.FlowStageTargetX;
            world.Rng.RestoreState(witness.RngState, witness.RngCalls);
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
