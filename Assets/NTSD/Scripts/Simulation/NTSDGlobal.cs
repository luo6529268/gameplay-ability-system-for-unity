
using System.Collections.Generic;
using System.Threading;

namespace NTSD.Simulation
{
    /// <summary>
    /// FLF/LF2 世界规则（global.js 的 gameplay 部分）- 直接死写数值（不走 JSON/ScriptableObject）。
    /// 目标：在 Unity 中复刻 FLF 的常量语义与默认值。
    /// </summary>
    public static class NTSDGlobal
    {
        public static class Gameplay
        {
            public const int Framerate = 30;

            // FLF: GC.min_speed
            public const float MinSpeed = 1f;

            // FLF: GC.gravity
            public const float Gravity = 1.7f;

            // FLF: GC.friction.fell
            // 注意：FLF 的 util.lookup_abs() 依赖 key 的有序遍历（数值 key 升序）。
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

            // FLF: GC.weapon.bounceup
            public const float WeaponBounceupLimit = 8f;
            public const float WeaponBounceupSpeedX = 3f;
            public const float WeaponBounceupSpeedY = -6f;
            public const float WeaponBounceupSpeedZ = 1f;

            // 反汇编 Entity_FrameAdvance 常量（从 .rdata 段确认，weapon_issues_report_v2.txt）
            // ---- state=1002 投射物重力分级（按 chardata.type_sub / [+6F4h]） ----
            public const float WeaponGravityTypeSub7C  = 0.17f;    // dbl_4433B0: type_sub=0x7C（极轻，气球）
            public const float WeaponGravityTypeSub78  = 0.425f;   // dbl_4433A8: type_sub=0x78（轻，苦无）
            public const float WeaponGravityTypeSub65  = 1.1333f;  // dbl_4433B8: type_sub=0x65（中等）
            public const float WeaponGravityDefault1002 = 0.5667f; // dbl_4433A0: state=1002 默认重力
            public const float WeaponGravityDefault    = 1.7f;     // dbl_443398: 非 state=1002 默认重力

            // ---- type=4 / type_sub=0x78 额外速度加成系数 ----
            public const float WeaponExtraVxFactor = 0.2f;         // dbl_4433E0: x_pos += vx * 0.2

            // ---- 落地弹射（Entity_FrameAdvance 0x416A35-0x416D0E） ----
            // type=1 大弹阈值及反弹
            public const float WeaponType1BigBounceThreshold = 9.9f;  // dbl_443368
            public const float WeaponType1BigBounceVy = -8.0f;        // 0xC0200000
            public const float WeaponType1VxFactor    = 0.5f;         // dbl_443360
            // type=2 大弹阈值及反弹
            public const float WeaponType2BigBounceThreshold = 9.0f;  // dbl_4433C8
            public const float WeaponType2BigBounceVy = -5.0f;        // 0xC0140000
            public const float WeaponType2VxFactor    = 0.5f;         // dbl_443360
            // type=4/6 大弹阈值及反弹
            public const float WeaponType46BigBounceThreshold = 8.5f; // dbl_443358
            public const float WeaponType46BigBounceVyFactor  = -0.7f; // dbl_443348: vy *= -0.7 反弹
            public const float WeaponType46BigBounceVyClamp   = -10.0f; // 0xC0240000
            public const float WeaponType46VxFactor  = 0.7f;          // dbl_443288
            // vx 上下限 clamp（回旋镖）
            public const float WeaponBoomerangVxMax  = 9.0f;          // dbl_4433C8
            public const float WeaponBoomerangVxMin  = -9.0f;         // dbl_4433C0

            // 反汇编 AI_Process2 0x41ACF3/0x41ACFB：饮料/食物恢复 PP 上限（0x1F4 = 500）
            public const int DrinkPPCap = 500; // [holder+308h] 上限

            // FLF: GC.weapon.hit
            public const float WeaponHitVx = 3f;
            public const float WeaponHitVy = -3f;

            // FLF: GC.weapon.reverse.factor
            public const float WeaponReverseFactorVx = -0.4f;
            public const float WeaponReverseFactorVy = 0.8f;
            public const float WeaponReverseFactorVz = 0.8f;

            // FLF: GC.weapon.soft_bounceup
            public const float WeaponSoftBounceupSpeedY = -3f;

            // FLF: GC.defend.break_limit
            public const int DefendBreakLimit = 60;

            // FLF: GC.defend.injury.factor
            public const float DefendInjuryFactor = 0.5f;

            // FLF: GC.defend.absorb — lookup_abs 表，key=|ef_dvx| 阈值，value=吸收量
            public static readonly IReadOnlyDictionary<int, float> DefendAbsorb = new Dictionary<int, float>
            {
                { 15, 5f },
            };

            // FLF: GC.effect.duration — 效果默认持续帧数
            public const int EffectDuration = 20;

            // FLF global.js:217  GC.fall.KO = 60
            public const int FallKO = 60;

            // FLF global.js:218-226  GC.fall.wait180 = { 7:1, 9:2, 11:3, 13:4 }
            // 用于 State 12 frame 事件：lookup_abs(GC.fall.wait180, effect.dvy) → 帧180的等待时间
            public static readonly IReadOnlyDictionary<int, float> FallWait180 = new Dictionary<int, float>
            {
                { 7,  1f },
                { 9,  2f },
                { 11, 3f },
                { 13, 4f },
            };

            // FLF global.js:192-200  GC.character.bounceup
            // limit.xy=9.9, limit.y=11, y=8.5
            // absorb = { 9:1, 14:4, 20:10, 40:20, 60:30 }
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

            // FLF: GC.effect.num_to_id
            public const int EffectNumToId = 300;

            // FLF global.js:177-178  GC.recover
            // fall/bdefend 每 TU 自然恢复量（负数 = 减少）
            public const float RecoverFall    = -0.45f;
            public const float RecoverBdefend = -0.5f;

            // 反汇编 Entity_FrameLogic dbl_4432D8：角色互碰时 vx 推开量
            public const float CharCollisionVxPush = 0.85f;
            // 反汇编 Entity_FrameLogic dbl_4432C8：角色互碰时 vz 衰减系数
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
                // 反汇编 GameMode_Process 0x0042266F: mov byte ptr [+0F0h], 0Ah（正常命中 10 帧）
                public const int VRest = 10;
            }

            public static class Character
            {
                public const int ARest = 7;
            }

            public static class Machanics
            {
                // FLF: GC.default.machanics.mass
                public const float Mass = 1f;
            }
        }

        public static class Combo 
        {
           public const int Timeout = 10; // 连招超时时间（时间单位）
        }

        public static class Sound
        {
            public const string DefendGuard = "Battle/Defend/Guard";
            public const string FireBurn    = "Battle/Fire/Burn";
            public const string IceFreeze   = "Battle/Ice/Freeze";
            public const string IceShatter  = "Battle/Ice/Shatter";
            public const string FallLand    = "Battle/Fall/Land";
            // asm sub_42C8C0: slot 0=001.wav normal hit, slot 2=006.wav knockdown hit
            public const string HitNormal    = "Battle/Hit/Normal";    // 001.wav
            public const string HitKnockdown = "Battle/Hit/Knockdown"; // 006.wav
        }

        /// <summary>
        /// FLF util.lookup_abs(A, x) 的 C# 等价实现：
        /// - 取 abs(x)
        /// - 返回第一个 key >= abs(x) 对应的 value
        /// - 若 abs(x) 大于所有 key，则返回最后一个 key 的 value
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

