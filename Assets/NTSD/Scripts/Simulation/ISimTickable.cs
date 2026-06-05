namespace NTSD.Simulation
{
    /// <summary>
    /// 可被 SimulationTickDriver 直接驱动的战斗系统接口。
    ///
    /// 主要用于仍以 MonoBehaviour 形式存在的战斗驱动器或桥接系统。
    /// 纯逻辑实体应优先实现 ISimObject，并由 SimulationWorld 统一调度。
    /// </summary>
    public interface ISimTickable
    {
        /// <summary>
        /// 执行一次模拟 Tick
        ///
        /// 对应 C++ release 的固定 30Hz 战斗逻辑 tick（由 SimulationTickDriver 保证）
        /// </summary>
        /// <param name="tickIndex">
        /// 当前 Tick 的索引。
        /// 用途：
        /// - 日志和行为追踪
        /// - 战斗回放验证
        /// </param>
        void SimTick(int tickIndex);

        /// <summary>
        /// 模拟执行顺序
        ///
        /// 同一帧内，系统按 SimOrder 从小到大执行
        /// 必须保证稳定顺序（deterministic），不能使用 InstanceID 等随机值
        ///
        /// 推荐顺序按 SimOrderConstants 或具体驱动器约定维护。
        /// </summary>
        int SimOrder { get; }
    }
}
