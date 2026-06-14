using NTSD.Input;

namespace NTSD.Simulation
{
    /// <summary>
    /// 记录在某个模拟逻辑帧上的不可变输入事件。
    /// </summary>
    public struct SimInputEvent
    {
        /// <summary>
        /// 目标 30Hz 逻辑帧序号。
        /// </summary>
        public readonly int tickIndex;

        /// <summary>
        /// 逻辑按键掩码。
        /// </summary>
        public readonly FuncKeyMask key;

        /// <summary>
        /// `true` 表示按下，`false` 表示抬起。
        /// </summary>
        public readonly bool down;

        public SimInputEvent(int tickIndex, FuncKeyMask key, bool down)
        {
            this.tickIndex = tickIndex;
            this.key = key;
            this.down = down;
        }
    }
}
