using UnityEngine;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// Weapon 状态机实现
    /// 严格对齐 FLF weapon.js states 对象
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\weapon.js
    /// </summary>
    public static class LF2WeaponStates
    {
        // ========== Generic 状态处理 ==========
        // 对应 FLF weapon.js:28-95 states.generic

        /// <summary>
        /// Generic TU 事件处理
        /// 对应 FLF weapon.js:32-90
        /// </summary>
        public static void Generic_TU(LF2WeaponBase weapon)
        {
            weapon.Interaction();

            int state = weapon.GetState();
            switch (state)
            {
                case 1001: // 轻武器被持有
                case 2001: // 重武器被持有
                    break;
                default:
                    // 应用物理动力学
                    CharacterMechanics.Dynamics(weapon.PS);
                    break;
            }

            var ps = weapon.PS;
            if (ps.y == 0 && ps.vy > 0) // 落地
            {
                if (weapon.GetSpeed() > NTSDGlobal.Gameplay.WeaponBounceupLimit)
                {
                    // 弹跳
                    if (weapon.IsLight)
                    {
                        ps.vy = 0;
                        weapon.Trans.Frame(70, 0);
                    }
                    if (weapon.IsHeavy)
                    {
                        ps.vy = NTSDGlobal.Gameplay.WeaponBounceupSpeedY;
                    }
                    if (ps.vx != 0) ps.vx = Mathf.Sign(ps.vx) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                    if (ps.vz != 0) ps.vz = Mathf.Sign(ps.vz) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;

                    weapon.Health.HP -= weapon.WeaponDropHurt;
                }
                else
                {
                    // 缓慢落地
                    weapon.Team = 0;
                    ps.vy = 0;
                    if (weapon.IsLight)
                    {
                        weapon.Trans.Frame(70, 0);
                    }
                    if (weapon.IsHeavy)
                    {
                        weapon.Trans.Frame(21, 0);
                    }
                }
                ps.zz = 0;
            }
        }

        /// <summary>
        /// Generic Die 事件处理
        /// 对应 FLF weapon.js:86-95
        /// </summary>
        public static void Generic_Die(LF2WeaponBase weapon)
        {
            weapon.Trans.Frame(1000, 0);
            weapon.PlaySound(weapon.WeaponBrokenSound);
            weapon.CreateBrokenEffect();
        }

        // ========== State 1003（轻武器刚落地）==========
        // 对应 FLF weapon.js:98-113

        /// <summary>
        /// State 1003 Frame 事件
        /// </summary>
        public static void State1003_Frame(LF2WeaponBase weapon)
        {
            if (weapon.Frame.N == 70)
            {
                var frame = weapon.Frame.D;
                if (frame == null || string.IsNullOrEmpty(frame.sound))
                {
                    weapon.PlaySound(weapon.WeaponDropSound);
                }
            }
        }

        // ========== State 1004（轻武器在地面）==========
        // 对应 FLF weapon.js:116-127

        /// <summary>
        /// State 1004 Frame 事件
        /// </summary>
        public static void State1004_Frame(LF2WeaponBase weapon)
        {
            if (weapon.Frame.N == 64)
            {
                weapon.Team = 0;
            }
        }

        // ========== State 2000（重武器在空中）==========
        // 对应 FLF weapon.js:130-147

        /// <summary>
        /// State 2000 Frame 事件
        /// </summary>
        public static void State2000_Frame(LF2WeaponBase weapon)
        {
            if (weapon.Frame.N == 21)
            {
                weapon.Trans.SetNext(20);
                var frame = weapon.Frame.D;
                if (frame == null || string.IsNullOrEmpty(frame.sound))
                {
                    weapon.PlaySound(weapon.WeaponDropSound);
                }
            }
        }

        // ========== State 2004（重武器在地面）==========
        // 对应 FLF weapon.js:150-161

        /// <summary>
        /// State 2004 Frame 事件
        /// </summary>
        public static void State2004_Frame(LF2WeaponBase weapon)
        {
            if (weapon.Frame.N == 20)
            {
                weapon.Team = 0;
            }
        }

        // ========== Hit 处理 ==========
        // 对应 FLF weapon.js:275-365

        /// <summary>
        /// 轻武器被击中处理
        /// </summary>
        public static bool LightWeapon_Hit(LF2WeaponBase weapon, InteractionArea itr, ILF2LivingObject attacker)
        {
            if (weapon.HoldObj != null) return false;
            if (weapon.IsVRest(attacker)) return false;

            // 特殊攻击处理
            if (itr.kind == 15) // 龙卷风
            {
                weapon.WhirlwindForce(itr);
                return true;
            }
            if (itr.kind == 10 || itr.kind == 11) // 笛子
            {
                weapon.FluteForce();
                return true;
            }

            bool accept = false;
            int state = weapon.GetState();
            var ps = weapon.PS;

            if (state == 1002) // 投掷中
            {
                accept = true;
                var attackerPs = attacker?.PS;
                if (attackerPs != null)
                {
                    // 迎面碰撞
                    if ((attackerPs.Dirh() > 0) != (ps.vx > 0))
                    {
                        ps.vx *= NTSDGlobal.Gameplay.WeaponReverseFactorVx;
                    }
                }
                ps.vy *= NTSDGlobal.Gameplay.WeaponReverseFactorVy;
                ps.vz *= NTSDGlobal.Gameplay.WeaponReverseFactorVz;
                weapon.Team = attacker?.Team ?? 0;
            }
            else if (state == 1004) // 在地面
            {
                var attackerWeapon = attacker as LF2WeaponBase;
                if (attackerWeapon != null)
                {
                    accept = true;
                    var aps = attackerWeapon.PS;
                    ps.vx = (aps.vx != 0 ? Mathf.Sign(aps.vx) : 0) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                    ps.vz = (aps.vz != 0 ? Mathf.Sign(aps.vz) : 0) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;
                }
            }

            if (accept)
            {
                ApplyHitEffects(weapon, itr, attacker);
            }

            return accept;
        }

        /// <summary>
        /// 重武器被击中处理
        /// </summary>
        public static bool HeavyWeapon_Hit(LF2WeaponBase weapon, InteractionArea itr, ILF2LivingObject attacker)
        {
            if (weapon.HoldObj != null) return false;
            if (weapon.IsVRest(attacker)) return false;

            // 特殊攻击处理
            if (itr.kind == 15)
            {
                weapon.WhirlwindForce(itr);
                return true;
            }
            if (itr.kind == 10 || itr.kind == 11)
            {
                weapon.FluteForce();
                return true;
            }

            bool accept = false;
            int state = weapon.GetState();
            var ps = weapon.PS;

            int fall = itr.fall != 0 ? itr.fall : NTSDGlobal.Default.Fall.Value;

            if (state == 2004) // 在地面
            {
                accept = true;
                if (fall < 30)
                {
                    weapon.CreateEffect(0);
                }
                else if (fall < NTSDGlobal.Gameplay.FallKO)
                {
                    ps.vy = NTSDGlobal.Gameplay.WeaponSoftBounceupSpeedY;
                }
                else
                {
                    ps.vy = NTSDGlobal.Gameplay.WeaponBounceupSpeedY;
                    var attackerPs = attacker?.PS;
                    if (attackerPs != null)
                    {
                        if (attackerPs.vx != 0)
                            ps.vx = Mathf.Sign(attackerPs.vx) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                        if (attackerPs.vz != 0)
                            ps.vz = Mathf.Sign(attackerPs.vz) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;
                    }
                    weapon.Trans.Frame(999, 0);
                }
            }
            else if (state == 2000) // 在空中
            {
                if (fall >= NTSDGlobal.Gameplay.FallKO)
                {
                    accept = true;
                    var attackerPs = attacker?.PS;
                    if (attackerPs != null)
                    {
                        if ((attackerPs.Dirh() > 0) != (ps.vx > 0))
                        {
                            ps.vx *= NTSDGlobal.Gameplay.WeaponReverseFactorVx;
                        }
                    }
                    ps.vy *= NTSDGlobal.Gameplay.WeaponReverseFactorVy;
                    ps.vz *= NTSDGlobal.Gameplay.WeaponReverseFactorVz;
                    weapon.Team = attacker?.Team ?? 0;
                }
            }

            if (accept)
            {
                ApplyHitEffects(weapon, itr, attacker);
            }

            return accept;
        }

        // ========== 辅助方法 ==========

        private static void ApplyHitEffects(LF2WeaponBase weapon, InteractionArea itr, ILF2LivingObject attacker)
        {
            if (itr.vrest > 0)
            {
                weapon.SetVRest(attacker, itr.vrest);
            }
            if (itr.injury > 0)
            {
                weapon.Health.HP -= itr.injury;
            }
            weapon.PlaySound(weapon.WeaponHitSound);
        }
    }
}
