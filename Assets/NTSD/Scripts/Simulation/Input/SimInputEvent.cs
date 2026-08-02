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

        /// <summary>
        /// True when this key value belongs to a complete per-tick held-state packet.
        /// Sparse Unity callbacks leave this false and rely on the local held mirror.
        /// </summary>
        public readonly bool completePacket;

        public SimInputEvent(
            int tickIndex,
            FuncKeyMask key,
            bool down,
            bool completePacket = false)
        {
            this.tickIndex = tickIndex;
            this.key = key;
            this.down = down;
            this.completePacket = completePacket;
        }
    }
}
