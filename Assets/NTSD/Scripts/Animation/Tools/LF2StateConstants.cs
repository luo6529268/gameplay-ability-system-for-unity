namespace NTSD.Animation
{
    /// <summary>
    /// LF2 状态处理器相关常量定义
    ///
    /// 包含：
    /// 1. 抓取系统常量
    /// 2. 等待时间常量
    /// 3. 恢复速率常量
    /// 4. 特效时长常量
    /// 5. 其他固定值
    ///
    /// 参考来源：
    /// - LF2 源码: I:\C++Test\NTSD\F.LF-master\LF\character.js
    /// </summary>
    public static class LF2StateConstants
    {
        // ==================== 抓取系统常量（State 9/10）====================
        // 对应 FLF character.js:728-730

        /// <summary>
        /// 抓取计数器初始值
        /// 对应 FLF character.js:729
        /// </summary>
        public const int CatchingCounterInit = 43;

        /// <summary>
        /// 抓取攻击次数初始值
        /// 对应 FLF character.js:730
        /// </summary>
        public const int CatchingAttacksInit = 0;

        /// <summary>
        /// 被抓取等待时间
        /// 对应 FLF character.js:872
        /// </summary>
        public const int BeingCaughtWaitTime = 99;

        /// <summary>
        /// 抓取成功攻击时增加的等待时间
        /// 对应 FLF character.js:744
        /// </summary>
        public const int CatchingAttackWaitBonus = 3;

        // ==================== 等待时间常量 ====================

        /// <summary>
        /// 连招转换等待时间（防御、攻击、跳跃、双击方向键等）
        /// 对应 FLF character.js:289, 301, 333, 441, 448, 461 等
        /// </summary>
        public const int ComboTransitionWait = 10;

        /// <summary>
        /// 方向移动等待时间
        /// 对应 FLF character.js:269
        /// </summary>
        public const int DirectionalMoveWait = 5;

        /// <summary>
        /// 通用连招转换等待时间（用于多键连招）
        /// 对应 FLF character.js:212
        /// </summary>
        public const int GenericComboWait = 11;

        /// <summary>
        /// 防御成功时增加的等待时间
        /// 对应 FLF character.js:691
        /// </summary>
        public const int DefendSuccessWaitBonus = 4;

        /// <summary>
        /// 受伤状态等待时间上限
        /// 对应 FLF character.js:947
        /// </summary>
        public const int InjuredWaitTimeMax = 20;

        /// <summary>
        /// 划船状态等待时间
        /// 对应 FLF character.js:670
        /// </summary>
        public const int RowingWaitTime = 1;

        // ==================== 特效时长常量（State 14）====================
        // 对应 FLF character.js:1132-1134

        /// <summary>
        /// 躺地起身后的闪烁效果持续时间（帧数）
        /// 对应 FLF character.js:1132-1134
        /// </summary>
        public const int LyingBlinkDuration = 30;

        // ==================== 攻击锁定时间常量（State 4）====================
        // 对应 FLF character.js:561, 896

        /// <summary>
        /// 跳跃攻击后的攻击锁定时间（帧数）
        /// 对应 FLF character.js:561
        /// </summary>
        public const int JumpAttackLockFrames = 2;

        /// <summary>
        /// 空中攻击锁定时间（帧数）
        /// 对应 FLF character.js:896
        /// </summary>
        public const int AirAttackLockFrames = 15;

        // ==================== 恢复速率常量（Generic TU）====================
        // 对应 FLF character.js:145-167

        /// <summary>
        /// HP 恢复间隔（帧数）
        /// 每12帧恢复1点HP
        /// 对应 FLF character.js:145-149
        /// </summary>
        public const int HpRecoveryInterval = 12;

        /// <summary>
        /// HP 每次恢复量
        /// 对应 FLF character.js:147
        /// </summary>
        public const int HpRecoveryAmount = 1;

        /// <summary>
        /// 治疗效果间隔（帧数）
        /// 每8帧恢复8点HP
        /// 对应 FLF character.js:152-160
        /// </summary>
        public const int HealEffectInterval = 8;

        /// <summary>
        /// 治疗效果每次恢复量
        /// 对应 FLF character.js:156
        /// </summary>
        public const int HealEffectAmount = 8;

        /// <summary>
        /// MP 恢复间隔（帧数）
        /// 每3帧恢复MP
        /// 对应 FLF character.js:163-167
        /// </summary>
        public const int MpRecoveryInterval = 3;

        /// <summary>
        /// MP 基础恢复量
        /// 对应 FLF character.js:165
        /// </summary>
        public const int MpRecoveryBaseAmount = 1;

        /// <summary>
        /// MP 额外恢复量计算因子
        /// 额外恢复量 = (hp_full - hp) / 100
        /// 对应 FLF character.js:165
        /// </summary>
        public const int MpRecoveryExtraFactor = 100;

        // ==================== 倒地系统常量（State 12）====================

        /// <summary>
        /// 倒地起身速度 X 轴
        /// 对应 FLF character.js:1074
        /// </summary>
        public const float FallingGetupVelocityX = 5f;

        /// <summary>
        /// 倒地起身速度 Y 轴
        /// 对应 FLF character.js:1075
        /// </summary>
        public const float FallingGetupVelocityY = 5f;

        /// <summary>
        /// 倒地起身速度 Z 轴
        /// 对应 FLF character.js:1076
        /// </summary>
        public const float FallingGetupVelocityZ = 2f;

        // ==================== 特殊帧转换常量 ====================

        /// <summary>
        /// 抓取计数器归零时，满足释放条件的最大攻击次数
        /// 对应 FLF character.js:807
        /// </summary>
        public const int CatchingMaxAttacksForRelease = 4;

        // ==================== 特效对象ID常量 ====================

        /// <summary>
        /// 冰冻破碎特效ID
        /// 对应 FLF character.js:1103
        /// </summary>
        public const int FrozenBrokenEffectId = 212;

        /// <summary>
        /// 燃烧特效ID
        /// 对应 FLF character.js:1248, 1252
        /// </summary>
        public const int BurningEffectId = 302;

        /// <summary>
        /// 燃烧特效类型参数
        /// 对应 FLF character.js:1248
        /// </summary>
        public const int BurningEffectType = 1;

        // ==================== 特殊攻击常量（笛子攻击）====================
        // 对应 FLF character.js:511-547

        /// <summary>
        /// 笛子攻击 ITR kind 1
        /// 对应 FLF character.js:514
        /// </summary>
        public const int FluteAttackKind1 = 10;

        /// <summary>
        /// 笛子攻击 ITR kind 2
        /// 对应 FLF character.js:516
        /// </summary>
        public const int FluteAttackKind2 = 11;

        /// <summary>
        /// 笛子攻击检测间隔（每2个时间单位检测一次）
        /// 对应 FLF character.js:517
        /// </summary>
        public const int FluteAttackInterval = 2;

        /// <summary>
        /// 笛子攻击范围
        /// 对应 FLF character.js:519
        /// </summary>
        public const int FluteAttackRange = 150;

        // ==================== 其他常量 ====================

        /// <summary>
        /// MP 消耗值取模基数（用于解析 mp 字段）
        /// MP变化 = mp % 1000
        /// 对应 FLF character.js:37
        /// </summary>
        public const int MpConsumptionModBase = 1000;

        /// <summary>
        /// HP 伤害值计算因子（用于解析 mp 字段）
        /// HP伤害 = (mp / 1000) * 10
        /// 对应 FLF character.js:37
        /// </summary>
        public const int HpDamageFromMpFactor = 10;
    }
}
