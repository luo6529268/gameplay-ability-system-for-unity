using UnityEngine;
using NTSD.Animation.LF2Tasks;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 轻武器对象
    /// 严格对齐 FLF weapon.js（lightweapon）
    ///
    /// 参考：
    /// - FLF typeweapon.prototype.init (weapon.js:204-213)
    /// - FLF states.1003, states.1004 (weapon.js:98-127)
    /// </summary>
    public class LF2LightWeapon : LF2WeaponBase
    {
        // ========== ILF2Object 实现 ==========
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.LightWeapon;
        public override bool IsLight => true;
        public override bool IsHeavy => false;

        // ========== 状态机 ==========

        protected override void OnStateTU(int state)
        {
            switch (state)
            {
                case 1003:
                    LF2WeaponStates.State1003_Frame(this);
                    break;
                case 1004:
                    LF2WeaponStates.State1004_Frame(this);
                    break;
            }
        }

        // ========== Hit 处理 ==========

        public override bool Hit(InteractionArea itr, ILF2LivingObject attacker)
        {
            return LF2WeaponStates.LightWeapon_Hit(this, itr, attacker);
        }

        // ========== 交互方法 ==========

        public override void Interaction()
        {
            if (Team == 0) return;

            int state = GetState();
            // 轻武器只有在 state 1002（投掷中）时才能交互
            if (state != 1002) return;

            // TODO: 实现碰撞检测
        }
    }
}
