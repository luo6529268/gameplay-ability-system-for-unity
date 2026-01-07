namespace NTSD.Simulation
{
    /// <summary>
    /// 可被模拟系统驱动的对象接口
    ///
    /// 所有需要在固定 30Hz 时钟下运行的系统都应该实现这个接口
    /// 例如：角色动画系统、输入/连招检测、AI 系统等
    /// </summary>
    public interface ISimTickable
    {
        /// <summary>
        /// 执行一次模拟 Tick
        ///
        /// 对应 FLF 的主循环中的一次 TU (Time Unit) 更新
        /// 调用频率：固定 30Hz（由 SimulationTickDriver 保证）
        /// </summary>
        /// <param name="tickIndex">
        /// 当前 Tick 的索引（从游戏启动开始累加）
        /// 用途：
        /// - 调试/日志（追踪特定 Tick 的行为）
        /// - 未来网络同步/回放（确定性验证）
        /// </param>
        void SimTick(int tickIndex);

        /// <summary>
        /// 模拟执行顺序
        ///
        /// 同一帧内，系统按 SimOrder 从小到大执行
        /// 必须保证稳定顺序（deterministic），不能使用 InstanceID 等随机值
        ///
        /// 推荐顺序：
        /// - 0-99: 输入系统（ActionSequenceDetector）
        /// - 100-199: 角色模拟（LF2CharacterAnimator）
        /// - 200-299: AI 系统
        /// - 300-399: 效果/粒子系统
        /// - 400+: 其他
        /// </summary>
        int SimOrder { get; }
    }
}
