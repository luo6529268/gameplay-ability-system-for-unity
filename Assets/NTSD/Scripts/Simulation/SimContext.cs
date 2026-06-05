namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟上下文，向战斗对象提供所属 SimulationWorld。
    ///
    /// SimContext 由 SimulationWorld 创建并在 OnAdded 时传给 ISimObject。
    /// 当前战斗复刻只保留 world 引用；具体逻辑状态存放在对象 Runtime 中。
    /// </summary>
    public class SimContext
    {
        /// <summary>所属的 SimulationWorld 实例。</summary>
        public SimulationWorld World { get; internal set; }

        /// <summary>
        /// 创建 SimContext（仅由 SimulationWorld 调用）
        /// </summary>
        /// <param name="world">所属的 SimulationWorld 实例</param>
        internal SimContext(SimulationWorld world)
        {
            World = world;
        }
    }
}
