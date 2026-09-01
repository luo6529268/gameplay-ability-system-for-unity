namespace NTSD.Simulation
{
    /// <summary>
    /// Client-owned physical input adapter exposed to the logic-frame assembler.
    /// The value is already mapped to NTSD logical actions and contains no Unity input object.
    /// </summary>
    public interface ILocalFrameInputSource
    {
        SimulationInputButtons CaptureHeldSimulationButtons();
    }
}
