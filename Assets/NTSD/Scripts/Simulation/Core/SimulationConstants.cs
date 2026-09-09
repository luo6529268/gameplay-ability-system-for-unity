namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟系统常量 - 定义游戏逻辑与Host时间基准
    /// </summary>
    public static class SimulationConstants
    {
        /// <summary>
        /// 现有每tick像素/Unity单位换算常量。它不是NTSD 2.8 Host wall-clock cadence；
        /// B1物理换算复核前保持原值，避免时间Host修复顺带改变每tick位移。
        /// </summary>
        public const int SIM_TICK_RATE = 30;

        /// <summary>
        /// NTSD 2.8-Logan普通Host逻辑间隔：精确33ms。
        /// </summary>
        public const int NORMAL_LOGIC_INTERVAL_MILLISECONDS = 33;
        public const float SIM_DT = NORMAL_LOGIC_INTERVAL_MILLISECONDS / 1000f;

        /// <summary>
        /// NTSD 2.8-Logan F5快速Host逻辑间隔：精确3ms。
        /// </summary>
        public const int FAST_LOGIC_INTERVAL_MILLISECONDS = 3;
        public const float FAST_SIM_DT = FAST_LOGIC_INTERVAL_MILLISECONDS / 1000f;

        /// <summary>
        /// 像素/单位比率（Unity PPU 设置）
        /// 与 PhysicsState 保持一致
        /// </summary>
        public const float PIXELS_PER_UNIT = 100f;
    }
}
