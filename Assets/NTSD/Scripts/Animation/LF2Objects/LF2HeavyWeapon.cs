using UnityEngine;
using NTSD.Animation.LF2Tasks;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 重武器对象
    /// 严格对齐 FLF weapon.js（heavyweapon）
    ///
    /// 参考：
    /// - FLF typeweapon.prototype.init (weapon.js:204-213)
    /// - FLF states.2000, states.2004 (weapon.js:130-161)
    /// </summary>
    public class LF2HeavyWeapon : LF2WeaponBase
    {
        // ========== ILF2Object 实现 ==========
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.HeavyWeapon;
        public override bool IsLight => false;
        public override bool IsHeavy => true;

        // ========== 状态机 ==========

        protected override void OnStateTU(int state)
        {
            switch (state)
            {
                case 2000:
                    LF2WeaponStates.State2000_Frame(this);
                    break;
                case 2004:
                    LF2WeaponStates.State2004_Frame(this);
                    break;
            }
        }

        // ========== Hit 处理 ==========

        public override bool Hit(InteractionArea itr, ILF2LivingObject attacker)
        {
            return LF2WeaponStates.HeavyWeapon_Hit(this, itr, attacker);
        }

        // ========== 交互方法 ==========

        public override void Interaction()
        {
            if (Team == 0) return;
            // 重武器任何时候都可以交互
            // TODO: 实现碰撞检测
        }
    }
}
