namespace NTSD.Extensions
{
    /// <summary>
    /// NTSD Community Edition Storm 扩展常量
    /// 基于 DLL 补丁逻辑还原
    /// </summary>
    public static class NTSDConstants
    {
        #region ITR Kind 扩展 (800-896)
        
        // 治疗队友 (800-806)
        public const int ITR_KIND_HEAL_TEAMMATE_START = 800;
        public const int ITR_KIND_HEAL_TEAMMATE_END = 806;
        
        // 治疗敌人 (810-816)
        public const int ITR_KIND_HEAL_ENEMY_START = 810;
        public const int ITR_KIND_HEAL_ENEMY_END = 816;
        
        // 所有者专属 (880-886)
        public const int ITR_KIND_OWNER_ONLY_START = 880;
        public const int ITR_KIND_OWNER_ONLY_END = 886;
        
        // 所有者队伍 (890-896)
        public const int ITR_KIND_OWNER_TEAM_START = 890;
        public const int ITR_KIND_OWNER_TEAM_END = 896;
        
        // 高级控制 ITR (100099-100103)
        public const int ITR_KIND_RANDOM_MOVE = 100099;
        public const int ITR_KIND_INPUT_CONTROL = 100100;
        public const int ITR_KIND_GRAVITY_CONTROL = 100101;
        public const int ITR_KIND_REMOTE_CONTROL = 100102;
        public const int ITR_KIND_CONTROLLABLE_MARKER = 100103;
        
        #endregion
        
        #region ITR Effect 扩展 (1xxx-9xxx)
        
        // Effect 1xxx: HP吸取 (伤害的 xxx% 转化为HP)
        public const int EFFECT_HP_DRAIN_START = 1000;
        public const int EFFECT_HP_DRAIN_END = 1999;
        
        // Effect 2xxx: HP治疗 (治疗 xxx 点HP)
        public const int EFFECT_HP_HEAL_START = 2000;
        public const int EFFECT_HP_HEAL_END = 2999;
        
        // Effect 3xxx: MP吸取 (吸取 xxx 点MP)
        public const int EFFECT_MP_DRAIN_START = 3000;
        public const int EFFECT_MP_DRAIN_END = 3999;
        
        // Effect 4xxx: MP给予 (给予 xxx 点MP)
        public const int EFFECT_MP_GIVE_START = 4000;
        public const int EFFECT_MP_GIVE_END = 4999;
        
        // Effect 5xxx: 强制跳帧 (目标跳转到帧 xxx)
        public const int EFFECT_FORCE_FRAME_START = 5000;
        public const int EFFECT_FORCE_FRAME_END = 5999;
        
        // Effect 9xxx: 复合效果
        public const int EFFECT_COMPOSITE_START = 9000;
        public const int EFFECT_COMPOSITE_END = 9999;
        
        #endregion
        
        #region State 扩展
        
        // State 2xxxyyy: MP消耗跳帧 (消耗 xxx MP, 跳转到帧 yyy)
        public const int STATE_MP_COST_MIN = 2000000;
        public const int STATE_MP_COST_MAX = 2999999;
        
        // State 3xxxyyy: 时停触发 (时停 xxx 帧, 跳转到帧 yyy)
        public const int STATE_TIMESTOP_MIN = 3000000;
        public const int STATE_TIMESTOP_MAX = 3999999;
        
        // State 8xxx: 无帧修复直接跳帧
        public const int STATE_DIRECT_FRAME_MIN = 8000;
        public const int STATE_DIRECT_FRAME_MAX = 8999;
        
        // State 9xxx: 受击跳帧
        public const int STATE_HIT_GOTO_MIN = 9000;
        public const int STATE_HIT_GOTO_MAX = 9999;
        
        #endregion
        
        #region BDY Kind 扩展
        
        // BDY Kind 1000-4999: 扩展碰撞类型
        public const int BDY_KIND_EXTENDED_MIN = 1000;
        public const int BDY_KIND_EXTENDED_MAX = 4999;
        
        #endregion
        
        #region RPG 系统常量
        
        public const int STAT_COUNT = 8;
        public const int RESIST_COUNT = 8;
        public const int EQUIPMENT_SLOT_COUNT = 8;
        public const int BUFF_SLOT_COUNT = 16;
        
        public const int DEFAULT_MAX_HP = 500;
        public const int DEFAULT_MAX_MP = 500;
        
        public const float DEFAULT_CRIT_CHANCE = 0f;
        public const float DEFAULT_CRIT_MULTIPLIER = 1.5f;
        
        #endregion
        
        #region 时停系统常量
        
        public const int TIMESTOP_TYPE_ALL = 0;
        public const int TIMESTOP_TYPE_CHARACTERS_ONLY = 2;
        
        #endregion
    }
}
