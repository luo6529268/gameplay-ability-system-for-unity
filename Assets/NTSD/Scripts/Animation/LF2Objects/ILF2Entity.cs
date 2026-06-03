using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// Minimal battle-entity contract. Runtime gameplay state lives in NTSDEntityRuntime;
    /// LF2Entity exposes compatibility properties for existing Unity callers.
    /// </summary>
    public interface ILF2Entity : ILF2Object
    {
        string Name { get; set; }
        NTSDEntityRuntime Runtime { get; }
        PhysicsState PS { get; }
        LF2FrameInfo Frame { get; }
        LF2FrameCache FrameCache { get; }
        LF2ObjectRenderer Renderer { get; }
        SimulationWorld Match { get; }
    }
}
