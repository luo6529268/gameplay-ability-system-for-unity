namespace NTSD.Animation
{
    /// <summary>
    /// Release battle statistics mirrored from C++ entity/global stat fields.
    /// </summary>
    public sealed class LF2BattleStat
    {
        /// <summary>Damage contribution, equivalent to release damage_stats / per-attacker accounting.</summary>
        public int Attack { get; set; }

        /// <summary>Kill count, equivalent to release kill_stat / kill_stats accounting.</summary>
        public int Kill { get; set; }

        /// <summary>Weapon pickup count, equivalent to release Entity.pickup_count.</summary>
        public int Picking { get; set; }

        public void Reset()
        {
            Attack = 0;
            Kill = 0;
            Picking = 0;
        }
    }
}
