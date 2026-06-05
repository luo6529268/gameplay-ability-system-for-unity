using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 战斗实体最小接口。
    /// 运行时真相状态存放在 NTSDEntityRuntime；LF2Entity 只暴露 Unity 调用侧仍需要的访问入口。
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
