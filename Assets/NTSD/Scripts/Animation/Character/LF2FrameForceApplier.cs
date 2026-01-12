namespace NTSD.Animation
{
    /// <summary>
    /// FLF 对齐：frame_force（动画帧数据驱动的速度变化）。
    /// 从 Animator 中剥离为纯运算模块，避免 Animator 直接承载“物理数据处理”细节。
    /// </summary>
    public static class LF2FrameForceApplier
    {
        /// <param name="ps">物理状态（对应 FLF $.ps）</param>
        /// <param name="frame">当前帧数据</param>
        /// <param name="dirv">纵向输入（-1/0/1），用于 dvz</param>
        public static void Apply(PhysicsState ps, LF2FrameData frame, int dirv)
        {
            if (ps == null || frame == null) return;

            // dvx: 水平速度（需要考虑角色朝向）
            if (frame.dvx != 0)
            {
                float avx = ps.vx > 0 ? ps.vx : -ps.vx;
                if (ps.y < 0 || avx < frame.dvx)
                    ps.vx = ps.Dirh() * frame.dvx; // 加速
                if (frame.dvx < 0)
                    ps.vx = ps.vx - ps.Dirh(); // 减速
            }

            if (frame.dvz != 0)
                ps.vz = dirv * frame.dvz;
            if (frame.dvy != 0)
                ps.vy += frame.dvy;

            // 550: reset velocity sentinel
            if (frame.dvx == 550)
                ps.vx = 0;
            if (frame.dvy == 550)
                ps.vy = 0;
            if (frame.dvz == 550)
                ps.vz = 0;
        }
    }
}

