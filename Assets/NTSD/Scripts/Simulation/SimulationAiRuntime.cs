namespace NTSD.Simulation
{
    /// <summary>
    /// Match-scoped AI aggregate. Input, sensing and decision state move here in
    /// verified slices while SimulationWorld keeps its compatibility façade.
    /// </summary>
    internal sealed class SimulationAiRuntime
    {
        internal SimulationAiRuntime(
            SimulationWorld world,
            int runtimeSlotCapacity,
            RuntimeSlotTable runtimeSlots)
        {
            Input = new SimulationAiInputModule(world, runtimeSlotCapacity);
            Sensing = new SimulationAiSensingModule(runtimeSlots);
            Decision = new SimulationAiDecisionModule(
                runtimeSlotCapacity,
                runtimeSlots,
                Input,
                Sensing);
        }

        internal SimulationAiInputModule Input { get; }
        internal SimulationAiSensingModule Sensing { get; }
        internal SimulationAiDecisionModule Decision { get; }
    }
}
