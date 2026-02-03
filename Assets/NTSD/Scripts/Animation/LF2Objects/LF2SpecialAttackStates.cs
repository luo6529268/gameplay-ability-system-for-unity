using UnityEngine;
using NTSD.Animation;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// SpecialAttack 状态机实现
    /// 严格对齐 FLF specialattack.js states 对象
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\specialattack.js
    /// </summary>
    public static class LF2SpecialAttackStates
    {
        // ========== Generic 状态处理 ==========
        // 对应 FLF specialattack.js:17-72 states.generic

        /// <summary>
        /// Generic TU 事件处理
        /// 对应 FLF specialattack.js:20-28
        /// </summary>
        public static void Generic_TU(LF2SpecialAttack obj)
        {
            // $.interaction()
            obj.Interaction();

            // $.mech.dynamics()
            CharacterMechanics.Dynamics(obj.PS);

            // 处理 hit_a（生命值减少）
            var frame = obj.Frame.D;
            if (frame != null && frame.hit_a != 0)
            {
                obj.Health.HP -= frame.hit_a;
            }
        }

        /// <summary>
        /// Generic Frame 事件处理
        /// 对应 FLF specialattack.js:30-43
        /// </summary>
        public static void Generic_Frame(LF2SpecialAttack obj)
        {
            var frame = obj.Frame.D;
            if (frame == null) return;

            // 创建新对象（opoint）
            if (frame.opoint != null && frame.opoint.oid > 0)
            {
                obj.CreateObject(frame.opoint);
            }

            // 播放音效
            if (!string.IsNullOrEmpty(frame.sound))
            {
                obj.PlaySound(frame.sound);
            }

            // 帧 15 销毁
            if (obj.Frame.N == 15)
            {
                obj.Trans.Frame(1000, 0);
            }
        }

        /// <summary>
        /// Generic Frame_Force / TU_Force 事件处理
        /// 对应 FLF specialattack.js:45-51
        /// </summary>
        public static void Generic_Force(LF2SpecialAttack obj)
        {
            var frame = obj.Frame.D;
            if (frame == null) return;

            // hit_j 控制 vz
            if (frame.hit_j != 0)
            {
                float dvz = frame.hit_j - 50;
                obj.PS.vz = dvz;
            }
        }

        /// <summary>
        /// Generic Leaving 事件处理（离开场景边界）
        /// 对应 FLF specialattack.js:53-57
        /// </summary>
        public static void Generic_Leaving(LF2SpecialAttack obj)
        {
            // 如果离开边界超过 200，销毁
            if (obj.IsLeavingBoundary(200))
            {
                obj.Trans.Frame(1000, 0);
            }
        }

        /// <summary>
        /// Generic Die 事件处理
        /// 对应 FLF specialattack.js:65-67
        /// </summary>
        public static void Generic_Die(LF2SpecialAttack obj)
        {
            var frame = obj.Frame.D;
            if (frame != null && frame.hit_d != 0)
            {
                obj.Trans.Frame(frame.hit_d, 0);
            }
        }

        // ========== State 300X（追踪弹）==========
        // 对应 FLF specialattack.js:74-99

        /// <summary>
        /// State 300X TU 处理（追踪逻辑）
        /// </summary>
        public static void State300X_TU(LF2SpecialAttack obj)
        {
            var frame = obj.Frame.D;
            if (frame == null) return;

            var PS = obj.PS;

            // hit_Fa == 1 或 2：追踪模式
            if (frame.hit_Fa == 1 || frame.hit_Fa == 2)
            {
                if (obj.Health.HP > 0)
                {
                    var target = obj.ChaseTarget();
                    if (target != null)
                    {
                        float dx = target.PS.x - PS.x;
                        float dz = target.PS.z - PS.z;

                        // 加速追踪
                        if (PS.vx * Mathf.Sign(dx) < 14)
                        {
                            PS.vx += Mathf.Sign(dx) * 0.7f;
                        }
                        if (PS.vz * Mathf.Sign(dz) < 2.2f)
                        {
                            PS.vz += Mathf.Sign(dz) * 0.4f;
                        }

                        // 更新朝向
                        obj.SwitchDir(PS.vx >= 0 ? "right" : "left");
                    }
                }
            }

            // hit_Fa == 10：直线飞行模式
            if (frame.hit_Fa == 10)
            {
                PS.vx = Mathf.Sign(PS.vx) * 17;
                PS.vz = 0;
            }
        }

        // ========== State 1002（投掷物）==========
        // 对应 FLF specialattack.js:102-125

        /// <summary>
        /// State 1002 Entry
        /// </summary>
        public static void State1002_Entry(LF2SpecialAttack obj)
        {
            // nobounce = parent.PS.y == 0
            obj.NoBounce = (obj.Parent?.PS?.y ?? 0) == 0;
        }

        /// <summary>
        /// State 1002 TU
        /// </summary>
        public static void State1002_TU(LF2SpecialAttack obj)
        {
            var PS = obj.PS;

            // 落地检测
            if (PS.y == 0 && PS.vy > 0)
            {
                if (obj.NoBounce)
                {
                    obj.Trans.Frame(1000, 0);
                }
                else if (obj.GetSpeed() > NTSDGlobal.Gameplay.WeaponBounceupLimit)
                {
                    obj.Trans.Frame(10, 0);
                    PS.vy = NTSDGlobal.Gameplay.WeaponBounceupSpeedY;
                    if (PS.vx != 0) PS.vx = Mathf.Sign(PS.vx) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                    if (PS.vz != 0) PS.vz = Mathf.Sign(PS.vz) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;
                }
            }
        }

        /// <summary>
        /// State 1002 HitOthers
        /// </summary>
        public static void State1002_HitOthers(LF2SpecialAttack obj)
        {
            var PS = obj.PS;
            PS.vx = 0;
            obj.Trans.Frame(10, 0);
        }

        // ========== State 3000（飞行弹）==========
        // 对应 FLF specialattack.js:127-189

        /// <summary>
        /// State 3000 HitOthers
        /// </summary>
        public static bool State3000_HitOthers(LF2SpecialAttack obj, LF2SpecialAttack attacker, InteractionArea itr)
        {
            var PS = obj.PS;

            // 冰冻弹特殊处理
            var frame = obj.Frame.D;
            var frameItr = GetFirstItr(frame);
            if (frameItr != null)
            {
                // effect==3 的冰冻弹被非冰冻弹击中
                if (itr.effect != 3 && itr.effect != 2 &&
                    attacker != null && attacker.GetState() == 3000 &&
                    frameItr.effect == 3)
                {
                    PS.vx = 0;
                    obj.Trans.Frame(1000, 0);
                    // 创建冰冻爆炸效果
                    obj.CreateObjectAt(209, attacker);
                    return true;
                }
            }

            PS.vx = 0;
            obj.Trans.Frame(10, 0);
            return true;
        }

        /// <summary>
        /// State 3000 Hit（被击中）
        /// </summary>
        public static bool State3000_Hit(LF2SpecialAttack obj, ILF2LivingObject attacker, InteractionArea itr)
        {
            var PS = obj.PS;
            var frame = obj.Frame.D;

            // kind==14 障碍物
            if (itr.kind == 14)
            {
                obj.Trans.SetWait(0, 20);
                return true;
            }

            // 同队同向不碰撞
            if (attacker != null)
            {
                if (obj.Team == attacker.Team && PS.dir == attacker.PS?.dir)
                {
                    return false;
                }
            }

            // 冰冻弹特殊处理
            var frameItr = GetFirstItr(frame);
            if (frameItr != null && frameItr.effect == 3)
            {
                var attackerSA = attacker as LF2SpecialAttack;
                if (attackerSA != null && attackerSA.GetState() == 3000 &&
                    itr.effect != 3 && itr.effect != 2)
                {
                    return true;
                }
            }

            // 被 SpecialAttack 击中
            var attackerSpecial = attacker as LF2SpecialAttack;
            if (attackerSpecial != null)
            {
                if (frameItr != null && frameItr.effect != 3 && frameItr.effect != 2 && itr.effect == 3)
                {
                    PS.vx = 0;
                    obj.Trans.Frame(1000, 0);
                    obj.CreateObjectAt(209, attackerSpecial);
                    return true;
                }

                if (itr.kind == 0)
                {
                    PS.vx = 0;
                    obj.Trans.Frame(20, 0);
                    return true;
                }
            }

            // 被 kind==0 或 kind==9 击中：反弹
            if (itr.kind == 0 || itr.kind == 9)
            {
                PS.vx = 0;
                obj.Team = attacker?.Team ?? 0;
                obj.Trans.Frame(30, 0);
                // 立即更新两次
                obj.Trans.Trans();
                obj.TUUpdate();
                obj.Trans.Trans();
                obj.TUUpdate();
                return true;
            }

            return false;
        }

        /// <summary>
        /// State 3000 Exit（创建破碎效果）
        /// </summary>
        public static void State3000_Exit(LF2SpecialAttack obj)
        {
            obj.CreateBrokenEffect();
        }

        // ========== State 3006（特殊弹）==========
        // 对应 FLF specialattack.js:197-243

        /// <summary>
        /// State 3006 HitOthers
        /// </summary>
        public static bool State3006_HitOthers(LF2SpecialAttack obj, LF2SpecialAttack attacker)
        {
            if (attacker != null)
            {
                int attackerState = attacker.GetState();
                if (attackerState == 3005 || attackerState == 3006)
                {
                    obj.Trans.Frame(10, 0);
                    obj.PS.vx = 0;
                    obj.PS.vz = 0;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// State 3006 Hit
        /// </summary>
        public static bool State3006_Hit(LF2SpecialAttack obj, ILF2LivingObject attacker, InteractionArea itr)
        {
            var PS = obj.PS;

            // kind==9 反弹
            if (itr.kind == 9)
            {
                PS.vx *= -1;
                PS.z += 0.3f;
                return true;
            }

            var attackerSA = attacker as LF2SpecialAttack;
            if (attackerSA != null)
            {
                int attackerState = attackerSA.GetState();

                // 被 3005/3006 击中
                if (attackerState == 3005 || attackerState == 3006)
                {
                    obj.Trans.Frame(20, 0);
                    PS.vx = 0;
                    PS.vz = 0;
                    return true;
                }

                // 被 3000 击中
                if (attackerState == 3000)
                {
                    PS.vx = (PS.vx > 0 ? -1 : 1) * 7;
                    return true;
                }
            }

            // kind==0 普通攻击
            if (itr.kind == 0)
            {
                PS.vx = (PS.vx > 0 ? -1 : 1) * 1;
                if (itr.bdefend > NTSDGlobal.Gameplay.DefendBreakLimit)
                {
                    obj.Health.HP = 0;
                }
                return true;
            }

            return false;
        }

        // ========== State 15（速度维持）==========
        // 对应 FLF specialattack.js:246-253

        /// <summary>
        /// State 15 TU
        /// </summary>
        public static void State15_TU(LF2SpecialAttack obj)
        {
            var frame = obj.Frame.D;
            if (frame != null && frame.dvx != 0)
            {
                obj.PS.vx = obj.Dirh() * frame.dvx;
            }
        }

        // ========== 辅助方法 ==========

        /// <summary>
        /// 获取帧的第一个 ITR（对应 FLF $.frame.D.itr）
        /// </summary>
        private static InteractionArea GetFirstItr(LF2FrameData frame)
        {
            if (frame?.itrs == null || frame.itrs.Count == 0) return null;
            return frame.itrs[0];
        }
    }
}
