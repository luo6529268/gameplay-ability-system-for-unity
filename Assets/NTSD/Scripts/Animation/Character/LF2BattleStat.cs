namespace NTSD.Animation
{
    /// <summary>
    /// FLF 对齐：战斗统计（对应 FLF $.stat）。
    /// 纯数据层，不依赖 Animator/Mono。
    /// </summary>
    public sealed class LF2BattleStat
    {
        /// <summary>累计造成的伤害（对应 FLF stat.attack）</summary>
        public int Attack { get; set; }

        /// <summary>击杀数（对应 FLF stat.kill）</summary>
        public int Kill { get; set; }

        /// <summary>拾取数（对应 FLF stat.picking）</summary>
        public int Picking { get; set; }

        public void Reset()
        {
            Attack = 0;
            Kill = 0;
            Picking = 0;
        }
    }
}
