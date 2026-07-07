using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// NTSD 战斗全局常量。
    /// 复刻基准以 C++ release 工程为准；保留的 LF2 名称仅用于说明 DAT 语义。
    /// </summary>
    public static class NTSDGlobal
    {
        public static class Gameplay
        {
            public const int Framerate = 30;

            // 基础物理常量。
            public const float MinSpeed = 1f;
            public const double Gravity = 1.7; // P0-f-2a: double sim gravity (baseline GravityDefault=1.7)

            // 倒地摩擦查表。正式版等价逻辑仍使用按阈值取值的语义。
            // 注意：LookupAbs 依赖 key 升序遍历。
            public static readonly IReadOnlyDictionary<int, float> FrictionFell = new Dictionary<int, float>
            {
                { 2, 0f },
                { 3, 1f },
                { 5, 2f },
                { 6, 4f },
                { 9, 5f },
                { 13, 7f },
                { 25, 9f },
            };

            // 武器命中后弹起参数。
            public const float WeaponBounceupSpeedX = 3f;
            public const float WeaponBounceupSpeedZ = 1f;

            // C++ release Entity_FrameAdvance / physics_update 常量。
            // state=1002 投射物重力分级（按 chardata.type_sub / DAT 对象类型）。
            // P0-f-2a: double sim gravity, baseline full-precision literals (NtsdConstants.cs).
            public const double WeaponGravityTypeSub7C  = 0.17;                // type_sub=0x7C：极轻对象 (baseline oid124=0.17)
            public const double WeaponGravityTypeSub78  = 0.425;               // type_sub=0x78：轻对象 (baseline oid120=0.425)
            public const double WeaponGravityTypeSub65  = 1.1333333333333333;  // type_sub=0x65：中等对象 (baseline GravityType6)
            public const double WeaponGravityDefault1002 = 0.5666666666666667; // state=1002 默认重力 (baseline)
            public const double WeaponGravityDefault    = 1.7;                 // 非 state=1002 默认重力 (baseline GravityDefault)

            // type=4 / type_sub=0x78 额外 X 速度位置修正。
            public const float WeaponExtraVxFactor = 0.2f;

            // 武器落地反弹参数。P0-f-2b B1: float→double，对齐 baseline Physics.cs 全 double 落地反弹链。
            public const double WeaponType1BigBounceThreshold = 9.9;
            public const double WeaponType1BigBounceVy = -8.0;
            public const double WeaponType1VxFactor    = 0.5;

            public const double WeaponType2BigBounceThreshold = 9.0;
            public const double WeaponType2BigBounceVy = -5.0;
            public const double WeaponType2VxFactor    = 0.5;

            public const double WeaponType46BigBounceThreshold = 8.5;
            public const double WeaponType46BigBounceVyFactor  = -0.7;
            public const double WeaponType46BigBounceVyClamp   = -10.0;
            public const double WeaponType46VxFactor  = 0.7;

            // 回旋镖 vx 上下限 clamp。
            public const float WeaponBoomerangVxMax  = 9.0f;
            public const float WeaponBoomerangVxMin  = -9.0f;

            // C++ release AI_Process2：饮料/食物恢复 PP 上限，0x1F4 = 500。
            public const int DrinkPPCap = 500;

            public const float WeaponHitVx = 3f;
            public const float WeaponHitVy = -3f;

            public const float WeaponReverseFactorVx = -0.4f;
            public const float WeaponReverseFactorVy = 0.8f;
            public const float WeaponReverseFactorVz = 0.8f;

            public const float WeaponSoftBounceupSpeedY = -3f;

            public const int DefendBreakLimit = 60;
            public const float DefendInjuryFactor = 0.5f;

            // 防御吸收 lookup_abs 表：key=|ef_dvx| 阈值，value=吸收量。
            public static readonly IReadOnlyDictionary<int, float> DefendAbsorb = new Dictionary<int, float>
            {
                { 15, 5f },
            };

            public const int EffectDuration = 20;

            public const int FallKO = 60;

            // 倒地等待查表：State 12 frame 事件中按 effect.dvy 计算帧 180 的等待时间。
            public static readonly IReadOnlyDictionary<int, float> FallWait180 = new Dictionary<int, float>
            {
                { 7,  1f },
                { 9,  2f },
                { 11, 3f },
                { 13, 4f },
            };

            // 角色落地弹起参数。
            public const float CharBounceupLimitXY = 9.9f;
            public const float CharBounceupLimitY  = 11f;
            public const float CharBounceupY       = 8.5f;
            public static readonly IReadOnlyDictionary<int, float> CharBounceupAbsorb = new Dictionary<int, float>
            {
                { 9,  1f  },
                { 14, 4f  },
                { 20, 10f },
                { 40, 20f },
                { 60, 30f },
            };

            public const int EffectNumToId = 300;

            // fall/bdefend 每 TU 自然恢复量，负数表示减少累计值。
            public const float RecoverFall    = -0.45f;
            public const float RecoverBdefend = -0.5f;

            // C++ release regenerate_pre_collision_stats。
            public const int HpRecoverPeriod = 12;
            public const int PpRecoverPeriod = 3;
            public const int PpRecoverCap = 500;
            public const int PpRecoverLowLimit = 150;
            public const int PpRecoverHpRateDivisor = 100;
            public const int NegativeWeaponCountInjury = 9;
            public const int NegativeWeaponCountScaledInjury = 900;
            public const int NegativeWeaponCountHpBoundDivisor = 3;
            public const int FluteCharacterWeaponCount = -20;

            // C++ release Entity_FrameLogic：角色互撞时的速度处理。
            public const float CharCollisionVxPush = 0.85f;
            public const float CharCollisionVzDecay = 5f / 7f;
        }

        public static class Default
        {
            public static class Health
            {
                public const int HpFull = 500;
                public const int MpFull = 500;
                public const int MpStart = 200;
            }

            public static class Itr
            {
                public const float ZWidth = 12f;
                public const int HitStop = 3;
                public const int ThrowInjury = 10;
            }

            public static class CPoint
            {
                public const int Hurtable = 0;
                public const int Cover = 0;
                public const int VAction = 135;
            }

            public static class WPoint
            {
                public const int Cover = 0;
            }

            public static class Effect
            {
                public const int Num = 0;
            }

            public static class Fall
            {
                public const int Value = 20;
                public const float Dvy = -6.9f;
            }

            public static class Weapon
            {
                // C++ release GameMode_Process：普通命中 vrest 默认 10 帧。
                public const int VRest = 10;
            }

            public static class Character
            {
                public const int ARest = 7;
            }

            public static class Machanics
            {
                public const float Mass = 1f;
            }
        }

        public static class Combo
        {
           public const int Timeout = 10; // 连招超时时间。
        }

        /// <summary>
        /// 全局 MP 消耗开关，对应 C++ release 的 g_pp_mode / dword_446970。
        /// true 表示启用 MP/PP 消耗；false 表示跳过消耗逻辑。
        /// </summary>
        public static bool MPEnabled = true;

        public static class Sound
        {
            public const string DefendGuard = "Battle/Defend/Guard";
            public const string FireBurn    = "Battle/Fire/Burn";
            public const string IceFreeze   = "Battle/Ice/Freeze";
            public const string IceShatter  = "Battle/Ice/Shatter";
            public const string FallLand    = "Battle/Fall/Land";
            public const string HitNormal    = "Battle/Hit/Normal";    // 001.wav
            public const string HitKnockdown = "Battle/Hit/Knockdown"; // 006.wav
        }

        /// <summary>
        /// 按绝对值查表：
        /// 取 abs(x)，返回第一个 key >= abs(x) 的 value；
        /// 若 abs(x) 大于所有 key，则返回最后一个 key 的 value。
        /// </summary>
        public static float LookupAbs(IReadOnlyDictionary<int, float> table, float x)
        {
            if (table == null || table.Count == 0)
                return 0f;

            if (x < 0f) x = -x;

            int? lastKey = null;
            foreach (var kv in table)
            {
                lastKey = kv.Key;
                if (x <= kv.Key)
                {
                    return kv.Value;
                }
            }

            return lastKey.HasValue ? table[lastKey.Value] : 0f;
        }
    }
}
