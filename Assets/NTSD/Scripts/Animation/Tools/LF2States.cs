namespace NTSD.Animation
{
    /// <summary>
    /// NTSD 状态值常量定义
    ///
    /// 设计说明：
    /// 1. state 值是 NTSD 中所有角色一致的状态标识符（数字）
    /// 2. frameName 只是描述性字符串，每个角色可能不同
    /// 3. 使用 state 值绑定 Handler，而不是 frameName
    ///
    /// 参考来源：
    /// - LF2 源码: I:\C++Test\NTSD\F.LF-master\LF\character.js (line 238-1405)
    /// - NTSD DAT 分析: I:\C++Test\NTSD\NTSD_DAT状态分析报告_增强版.md
    /// - 分析日期: 2025-12-20
    /// - 总计发现的唯一状态数: 36
    /// </summary>
    public static class LF2States
    {
        // ==================== 基础状态 (0-17) ====================
        // 参考: character.js line 239-1405

        /// <summary>
        /// 状态 0: 站立
        /// LF2: standing
        /// 出现次数: 25
        /// </summary>
        public const int Standing = 0;

        /// <summary>
        /// 状态 1: 行走
        /// LF2: walking
        /// 出现次数: 45
        /// </summary>
        public const int Walking = 1;

        /// <summary>
        /// 状态 2: 奔跑
        /// LF2: running
        /// 出现次数: 20
        /// </summary>
        public const int Running = 2;

        /// <summary>
        /// 状态 3: 攻击/出拳
        /// LF2: punch, attack
        /// 出现次数: 286
        /// </summary>
        public const int Attack = 3;

        /// <summary>
        /// 状态 4: 跳跃
        /// LF2: jump
        /// 出现次数: 19
        /// </summary>
        public const int Jump = 4;

        /// <summary>
        /// 状态 5: 冲刺
        /// LF2: dash
        /// 出现次数: 21
        /// </summary>
        public const int Dash = 5;

        /// <summary>
        /// 状态 6: 划船动作（防御的一种）
        /// LF2: rowing
        /// 出现次数: 48
        /// </summary>
        public const int Rowing = 6;

        /// <summary>
        /// 状态 7: 防御
        /// LF2: defending
        /// 出现次数: 13
        /// </summary>
        public const int Defending = 7;

        /// <summary>
        /// 状态 8: 防御被破
        /// LF2: broken_defend
        /// 出现次数: 15
        /// </summary>
        public const int BrokenDefend = 8;

        /// <summary>
        /// 状态 9: 抓取
        /// LF2: catching
        /// 出现次数: 156
        /// </summary>
        public const int Catching = 9;

        /// <summary>
        /// 状态 10: 被抓取
        /// LF2: being_caught
        /// 出现次数: 84
        /// </summary>
        public const int BeingCaught = 10;

        /// <summary>
        /// 状态 11: 受伤
        /// LF2: injured
        /// 出现次数: 30
        /// </summary>
        public const int Injured = 11;

        /// <summary>
        /// 状态 12: 跌倒/下落
        /// LF2: falling
        /// 出现次数: 60
        /// </summary>
        public const int Falling = 12;

        /// <summary>
        /// 状态 13: 冰冻
        /// LF2: frozen
        /// 出现次数: 0 (未使用)
        /// </summary>
        public const int Frozen = 13;

        /// <summary>
        /// 状态 14: 躺地
        /// LF2: lying
        /// 出现次数: 14
        /// </summary>
        public const int Lying = 14;

        /// <summary>
        /// 状态 15: 停止奔跑/蹲下/其他
        /// LF2: stop_running, crouch, etc
        /// 出现次数: 527 (使用频繁)
        /// </summary>
        public const int StopRunning = 15;

        /// <summary>
        /// 状态 16: 受伤2（痛苦之舞）
        /// LF2: injured_2 (dance of pain)
        /// 出现次数: 20
        /// </summary>
        public const int Injured2 = 16;

        /// <summary>
        /// 状态 17: 蓄力/充能
        /// LF2: charging
        /// 用途: 角色进行技能蓄力时的状态（如 "charge" 帧）
        /// 出现在: kakashi.dat, naruto.dat, sakura.dat, sasuke.dat
        /// 出现次数: 16
        /// </summary>
        public const int Charging = 17;

        // ==================== 特殊状态 (18-19) ====================

        /// <summary>
        /// 状态 18: 燃烧
        /// LF2: burning
        /// 出现次数: 92
        /// </summary>
        public const int Burning = 18;

        /// <summary>
        /// 状态 19: Firen 特有状态
        /// LF2: firen_specific
        /// 出现次数: 0 (未使用 - LF2 原版角色)
        /// 建议标注为 [Obsolete]
        /// </summary>
        public const int FirenSpecific = 19;

        // ==================== 角色特定状态 (100-999) ====================

        /// <summary>
        /// 状态 100: 自定义技能状态
        /// 出现在: sasuke.dat
        /// 出现次数: 2
        /// </summary>
        public const int CustomSkill1 = 100;

        /// <summary>
        /// 状态 301: Deep 特有状态
        /// LF2: deep_specific
        /// 出现次数: 0 (未使用 - LF2 原版角色)
        /// 建议标注为 [Obsolete]
        /// </summary>
        public const int DeepSpecific = 301;

        /// <summary>
        /// 状态 400: 传送到最近敌人
        /// LF2: teleport_to_enemy
        /// 出现次数: 12
        /// </summary>
        public const int TeleportToEnemy = 400;

        /// <summary>
        /// 状态 401: 传送到最远队友
        /// LF2: teleport_to_teammate
        /// 出现次数: 0 (未使用)
        /// </summary>
        public const int TeleportToTeammate = 401;

        /// <summary>
        /// 状态 501: Rudolf 变身相关
        /// LF2: rudolf_transform
        /// 出现次数: 0 (未使用 - LF2 原版角色)
        /// 建议标注为 [Obsolete]
        /// </summary>
        public const int RudolfTransform = 501;

        /// <summary>
        /// 状态 1700: 治疗
        /// LF2: heal
        /// 出现次数: 1
        /// </summary>
        public const int Heal = 1700;

        // ==================== 武器状态 (1000-1999) ====================

        /// <summary>
        /// 状态 1000: 武器在空中（投掷武器飞行中）
        /// LF2: weapon_in_sky
        /// 用途: 苦无、手里剑等投掷武器在空中飞行时的状态
        /// 出现在: rasengan_ball.dat, weapon5.dat
        /// 出现次数: 52
        /// </summary>
        public const int WeaponInSky = 1000;

        /// <summary>
        /// 状态 1001: 武器在手上（待投掷/持有）
        /// LF2: weapon_on_hand
        /// 用途: 武器被角色持有，尚未投掷时的状态
        /// 出现在: rasengan_ball.dat, weapon5.dat, weapon8.dat
        /// 出现次数: 52
        /// </summary>
        public const int WeaponOnHand = 1001;

        /// <summary>
        /// 状态 1002: 武器投掷中
        /// 用途: 武器被投掷出去的过程
        /// 出现在: weapon5.dat
        /// 出现次数: 16
        /// </summary>
        public const int WeaponThrowing = 1002;

        /// <summary>
        /// 状态 1003: 武器刚落地
        /// 用途: 武器刚落地的瞬间状态
        /// 出现在: weapon5.dat
        /// 出现次数: 7
        /// </summary>
        public const int WeaponJustOnGround = 1003;

        /// <summary>
        /// 状态 1004: 武器在地面
        /// 用途: 武器在地面上的状态
        /// 出现在: weapon5.dat
        /// 出现次数: 1
        /// </summary>
        public const int WeaponOnGround = 1004;

        // ==================== 重武器状态 (2000-2999) ====================
        // 反汇编依据: ntsd24_full_disasm.txt sub_4063B0 0x407378, Entity_AI_Update 0x42CC54

        /// <summary>
        /// 状态 2000: 重武器在空中（飞行中）
        /// 反汇编: [+7ACh] == 7D0h
        /// </summary>
        public const int HeavyWeaponInSky = 2000;

        /// <summary>
        /// 状态 2004: 重武器在地面
        /// 反汇编: [+7ACh] == 7D4h，sub_4063B0 0x407380 确认地面重武器才能击打角色
        /// </summary>
        public const int HeavyWeaponOnGround = 2004;

        // ==================== 投射物/对象状态 (3000-3999) ====================

        /// <summary>
        /// 状态 3000: 投射物飞行
        /// LF2: projectile_flying
        /// 用途: 快速移动的攻击性投射物（如千鸟、剑气）
        /// 出现在: chidori.dat, doggy.dat, poison.dat, sword.dat
        /// 出现次数: 17
        /// </summary>
        public const int ProjectileFlying = 3000;

        /// <summary>
        /// 状态 3001: 投射物命中中
        /// LF2: projectile_hitting
        /// 用途: 投射物正在命中目标的过程
        /// 出现在: charge.dat, wind.dat
        /// 出现次数: 37
        /// </summary>
        public const int ProjectileHiting = 3001;

        /// <summary>
        /// 状态 3002: 投射物命中后
        /// LF2: projectile_hit
        /// 用途: 投射物命中目标后的状态
        /// 出现在: doggy.dat, wind.dat
        /// 出现次数: 47
        /// </summary>
        public const int ProjectileHit = 3002;

        /// <summary>
        /// 状态 3003: 投射物瞬移/快速移动
        /// LF2: projectile_teleport
        /// 用途: 天照等需要瞬移到目标的技能
        /// 出现在: chidori.dat, poison.dat, rasenshuriken 1.dat, rasenshuriken.dat, sword.dat, wind.dat
        /// 出现次数: 89
        /// </summary>
        public const int ProjectileTeleport = 3003;

        /// <summary>
        /// 状态 3005: 对象飞行/替身术
        /// LF2: object_flying, replacement
        /// 用途: 通用对象飞行状态，也用于替身术
        /// 这是使用最频繁的状态之一（407 次）
        /// 出现在: charge.dat, chidori.dat, death.dat, doggy.dat, flash.dat, kakashi.dat, naruto.dat, poison.dat, rasenshuriken 1.dat, rasenshuriken.dat, sakura.dat, sasuke.dat, sword.dat, weapon8.dat, wind.dat
        /// 出现次数: 407
        /// </summary>
        public const int ObjectFlying = 3005;

        /// <summary>
        /// 状态 3006: 对象扩散/膨胀
        /// LF2: object_expanding
        /// 用途: 技能特效扩散或膨胀动画（如爆炸效果）
        /// 出现在: chidori.dat, death.dat, poison.dat, rasenshuriken 1.dat, rasenshuriken.dat
        /// 出现次数: 152
        /// </summary>
        public const int ObjectExpanding = 3006;

        // ==================== 角色专属技能 (4000-4999) ====================

        /// <summary>
        /// 状态 4038: 佐助专属技能 1
        /// 出现在: sasuke.dat (catching, chidori_run, cs2TRANSFORM)
        /// 出现次数: 3
        /// </summary>
        public const int SasukeSkill1 = 4038;

        /// <summary>
        /// 状态 4052: 鸣人专属技能 1（九尾化）
        /// 出现在: naruto.dat (kyubii)
        /// 出现次数: 1
        /// </summary>
        public const int NarutoSkill1 = 4052;

        // ==================== 特效状态 (9000+) ====================

        /// <summary>
        /// 状态 9997: 特效播放
        /// LF2: effect_playing
        /// 用途: 视觉特效播放（如写轮眼特效）
        /// 出现在: chidori.dat, flash.dat, poison.dat, sword.dat
        /// 出现次数: 47
        /// </summary>
        public const int EffectPlaying = 9997;

        /// <summary>
        /// 状态 30005: 特殊效果
        /// 出现在: rasenshuriken 1.dat, rasenshuriken.dat
        /// 出现次数: 2
        /// </summary>
        public const int SpecialEffect = 30005;

        // ==================== 状态范围规划 ====================
        // 基于 NTSD DAT 分析的状态范围规划：
        //
        // 0-17: LF2 标准基础状态
        // 18-99: 扩展基础状态（燃烧、冰冻等）
        // 100-199: 通用技能状态
        // 200-299: 预留
        // 300-399: 预留（原 LF2 角色特定状态）
        // 400-499: 传送/位移类技能
        // 500-999: 角色特定技能
        // 1000-1099: 武器基础状态（在空中、在手上等）
        // 1100-1999: 特定武器状态
        // 3000-3099: 投射物飞行相关
        // 3100-3999: 对象特定状态
        // 4000-4099: 鸣人专属
        // 4100-4199: 佐助专属
        // 4200-4299: 小樱专属
        // 4300-4399: 卡卡西专属
        // 4400-8999: 其他角色预留
        // 9000-9999: 视觉特效
        // 10000+: 特殊状态

        /// <summary>
        /// 技能状态起始值（用于注册技能状态处理器）
        /// </summary>
        public const int AbilityStart = 301;

        /// <summary>
        /// 技能状态结束值（用于注册技能状态处理器）
        /// </summary>
        public const int AbilityEnd = 999;

        /// <summary>
        /// 检查是否为基础状态（0-17）
        /// </summary>
        public static bool IsBaseState(int state)
        {
            return state >= 0 && state <= 17;
        }

        /// <summary>
        /// 检查是否为特殊状态（18-99）
        /// </summary>
        public static bool IsSpecialState(int state)
        {
            return state >= 18 && state <= 99;
        }

        /// <summary>
        /// 检查是否为自定义状态（100+）
        /// </summary>
        public static bool IsCustomState(int state)
        {
            return state >= 100;
        }

        /// <summary>
        /// 检查是否为技能状态（301-999）
        /// </summary>
        public static bool IsAbilityState(int state)
        {
            return state >= AbilityStart && state <= AbilityEnd;
        }

        /// <summary>
        /// 检查是否为武器状态（1000-1999）
        /// </summary>
        public static bool IsWeaponState(int state)
        {
            return state >= 1000 && state <= 1999;
        }

        /// <summary>
        /// 检查是否为投射物/对象状态（3000-3999）
        /// </summary>
        public static bool IsProjectileOrObjectState(int state)
        {
            return state >= 3000 && state <= 3999;
        }

        /// <summary>
        /// 检查是否为角色专属技能状态（4000-8999）
        /// </summary>
        public static bool IsCharacterSpecificState(int state)
        {
            return state >= 4000 && state <= 8999;
        }

        /// <summary>
        /// 检查是否为特效状态（9000+）
        /// </summary>
        public static bool IsEffectState(int state)
        {
            return state >= 9000;
        }
    }
}
