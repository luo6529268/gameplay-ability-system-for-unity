namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟上下文 - 全局 sim world 的依赖注入容器
    ///
    /// 职责：
    /// - 提供对世界级服务的访问（时间配置、世界查询、网络钩子等）
    /// - 所有 ISimObject 在 OnAdded 时接收此上下文
    /// - 避免全局单例，支持未来的多 World 实例（如场景切换、测试）
    ///
    /// Plan B 架构：
    /// - SimContext 由 SimulationWorld 创建并持有
    /// - ISimObject 通过 OnAdded(ctx) 获取上下文引用
    /// - 不允许 ISimObject 直接访问全局单例（除了 SimulationTickDriver.Instance）
    /// </summary>
    public class SimContext
    {
        // ==================== 时间配置 ====================

        /// <summary>
        /// 模拟 Tick 频率（Hz）
        /// 对应 SimulationConstants.SIM_TICK_RATE = 30
        /// </summary>
        public int TickRate => SimulationConstants.SIM_TICK_RATE;

        /// <summary>
        /// 每个 Tick 的时间间隔（秒）
        /// 对应 SimulationConstants.SIM_DT ≈ 0.0333
        /// </summary>
        public float TickDeltaTime => SimulationConstants.SIM_DT;

        // ==================== 世界引用 ====================

        /// <summary>
        /// 所属的 SimulationWorld 实例
        /// （预留：用于对象查询、事件广播等）
        /// </summary>
        public SimulationWorld World { get; internal set; }

        // ==================== 预留：网络钩子 (Plan B Networking) ====================

        /// <summary>
        /// 是否为网络会话（单机/多人）
        /// 默认：false（单机模式）
        /// 未来：由服务器/客户端初始化时设置
        /// </summary>
        public bool IsNetworked { get; internal set; } = false;

        /// <summary>
        /// 当前本地玩家的 StableId
        /// 单机模式：默认 1
        /// 多人模式：由服务器分配
        /// </summary>
        public int LocalPlayerStableId { get; internal set; } = 1;

        // ==================== 预留：世界服务 ====================

        // 未来扩展示例：
        // public IInputBuffer InputBuffer { get; internal set; }
        // public IPhysicsQuery PhysicsQuery { get; internal set; }
        // public IEventBus EventBus { get; internal set; }
        // public INetworkSync NetworkSync { get; internal set; }

        // ==================== 构造函数 ====================

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
