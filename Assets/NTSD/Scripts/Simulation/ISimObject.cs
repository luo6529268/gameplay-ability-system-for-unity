namespace NTSD.Simulation
{
    /// <summary>
    /// 可被 SimulationWorld 管理的模拟对象接口
    ///
    /// 与 ISimTickable 的区别：
    /// - ISimTickable: MonoBehaviour 组件，由 SimulationTickDriver 直接发现和驱动（Plan A）
    /// - ISimObject: 纯 C# 对象，由 SimulationWorld 管理，支持 StableId 确定性排序（Plan B）
    ///
    /// Plan B 架构：
    /// - 所有 gameplay 真相由 ISimObject 实现（纯 C# sim 模块）
    /// - MonoBehaviour 仅作为 "Hub"（组件缓存、注册/反注册）或 "View"（渲染、调试）
    /// - SimulationWorld 负责按确定性顺序（SimOrder → StableId）驱动所有 ISimObject
    /// </summary>
    public interface ISimObject
    {
        /// <summary>
        /// 执行顺序（第一优先级）
        ///
        /// 同 ISimTickable.SimOrder，但用于纯 C# 模拟对象
        /// 推荐顺序：
        /// - 0-99: 输入系统
        /// - 100-199: 角色模拟
        /// - 200-299: AI 系统
        /// - 300+: 其他
        /// </summary>
        int SimOrder { get; }

        /// <summary>
        /// 稳定 ID（第二优先级，用于确定性排序）
        ///
        /// 要求：
        /// - 同一局游戏内必须稳定（不能使用 InstanceID 等随机值）
        /// - 多人游戏：由服务器分配（保证所有客户端一致）
        /// - 单人游戏/本地 AI：可由 World 自动分配递增 ID
        ///
        /// 用途：
        /// - 当多个对象有相同 SimOrder 时，按 StableId 排序
        /// - 确保执行顺序在网络同步/回放时完全一致
        /// </summary>
        int StableId { get; }

        /// <summary>
        /// 当对象被添加到 SimulationWorld 时调用
        ///
        /// 用途：
        /// - 保存 SimContext 引用
        /// - 初始化依赖的世界服务
        /// - 注册到其他系统（如事件总线）
        /// </summary>
        /// <param name="ctx">模拟上下文（包含世界服务引用）</param>
        void OnAdded(SimContext ctx);

        /// <summary>
        /// 当对象从 SimulationWorld 移除时调用
        ///
        /// 用途：
        /// - 清理资源
        /// - 反注册事件监听
        /// - 断开依赖引用
        /// </summary>
        /// <param name="ctx">模拟上下文</param>
        void OnRemoved(SimContext ctx);

        /// <summary>
        /// 每个模拟 Tick 执行一次（主要逻辑）
        ///
        /// 对应 FLF 的主循环中的一次 TU (Time Unit) 更新
        /// 调用频率：固定 30Hz（由 SimulationTickDriver 保证）
        ///
        /// 执行顺序：
        /// 1. 按 SimOrder 从小到大
        /// 2. 相同 SimOrder 按 StableId 从小到大
        /// </summary>
        /// <param name="tickIndex">当前 Tick 索引（从游戏启动开始累加）</param>
        void SimTick(int tickIndex);

        /// <summary>
        /// 每个模拟 Tick 的后期处理（可选）
        ///
        /// 调用时机：所有对象的 SimTick 完成后
        /// 用途：
        /// - 视图更新（Transform 同步、动画状态）
        /// - 调试绘制
        /// - 延迟清理
        ///
        /// 默认实现：空操作（大多数对象不需要 LateTick）
        /// </summary>
        /// <param name="tickIndex">当前 Tick 索引</param>
        void SimLateTick(int tickIndex) { }
    }
}
