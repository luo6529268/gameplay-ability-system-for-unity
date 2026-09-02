namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟系统常量 - 定义游戏逻辑的时间基准
    ///
    /// 对应 C++ release 的 30Hz 战斗逻辑时间基准。
    /// 所有游戏逻辑（状态机、连招、物理）都基于这个频率运行。
    /// </summary>
    public static class SimulationConstants
    {
        /// <summary>
        /// 模拟 Tick 频率（Hz）
        /// 对应 C++ release 的战斗逻辑帧率：30 tick/秒。
        ///
        /// ⚠️ 这是游戏逻辑的"真相频率"，不是渲染帧率
        /// Unity 可以在 60Hz 或更高频率渲染，但游戏逻辑必须在 30Hz 运行
        /// </summary>
        public const int SIM_TICK_RATE = 30;

        /// <summary>
        /// 每个模拟 Tick 的时间间隔（秒）
        /// = 1 / SIM_TICK_RATE = 1/30 ≈ 0.0333 秒
        /// </summary>
        public const float SIM_DT = 1f / SIM_TICK_RATE;

        /// <summary>
        /// 像素/单位比率（Unity PPU 设置）
        /// 与 PhysicsState 保持一致
        /// </summary>
        public const float PIXELS_PER_UNIT = 100f;
    }
}
