namespace NTSD.Animation
{
    /// <summary>
    /// 角色状态逻辑仍在使用的少量帧常量。
    /// 复刻基准以 C++ release 工程为准；这里只保留战斗逻辑仍在使用的常量。
    /// </summary>
    public static class LF2StateConstants
    {
        /// <summary>
        /// 普通输入触发帧切换时使用的默认等待权重。
        /// </summary>
        public const int ComboTransitionWait = 10;

        /// <summary>
        /// 划船状态默认等待时间。
        /// </summary>
        public const int RowingWaitTime = 1;
    }
}
