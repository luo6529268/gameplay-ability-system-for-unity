namespace NTSD.Animation
{
    /// <summary>
    /// FLF bg 边界（像素坐标，匹配 ps.x/ps.z 的单位）。
    /// 对齐 FLF mechanics.js dynamics() 中的 x/z clamp 行为。
    /// </summary>
    public readonly struct LF2StageBoundsPx
    {
        public readonly bool floorXBound;
        public readonly float xMinPx;
        public readonly float xMaxPx;
        public readonly float zMinPx;
        public readonly float zMaxPx;

        public LF2StageBoundsPx(bool floorXBound, float xMinPx, float xMaxPx, float zMinPx, float zMaxPx)
        {
            this.floorXBound = floorXBound;
            this.xMinPx = xMinPx;
            this.xMaxPx = xMaxPx;
            this.zMinPx = zMinPx;
            this.zMaxPx = zMaxPx;
        }
    }

    public interface ILF2StageBoundsProvider
    {
        /// <summary>
        /// 返回 FLF 风格的关卡边界（以像素为单位）；false 表示当前无可用边界（不进行 clamp）。
        /// </summary>
        bool TryGetStageBoundsPx(out LF2StageBoundsPx bounds);
    }
}

