using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// WPoint 武器点驱动工厂（对应 FLF character.js wpoint 函数）。
    ///
    /// 在 transit 阶段被 LF2WeaponPointModule 调用，负责：
    ///   - kind=1（CHARACTER 帧）：计算持有点，调用 weapon.Act() 完成跟随 / 投掷
    ///   - kind=3（CHARACTER 帧）：强制丢弃武器
    ///
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\character.js  wpoint()
    /// </summary>
    public class LF2WeaponPointFactory : MMSingleton<LF2WeaponPointFactory>, ILF2WeaponPointFactory
    {
        /// <summary>
        /// 更新武器持有点（ILF2WeaponPointFactory 接口实现）。
        /// </summary>
        public void UpdateWeaponPoints(LF2LivingObject animator,
                                       LF2FrameData frameData,
                                       List<WeaponPoint> weaponPoints)
        {
            if (animator == null || weaponPoints == null) return;

            var character = animator as LF2Character;
            if (character == null) return;

            foreach (var wpoint in weaponPoints)
            {
                switch (wpoint.kind)
                {
                    case 1:
                        ProcessHoldPoint(character, wpoint);
                        break;

                    case 3:
                        // 强制丢弃（对应 FLF wpoint.kind===3 → $.drop_weapon()）
                        character.DropWeapon();
                        break;
                }
            }
        }

        // ─── 私有实现 ──────────────────────────────────────────────────────────

        /// <summary>
        /// kind=1：角色持有武器，计算持有点并调用 weapon.Act()。
        /// 对应 FLF character.js wpoint() kind===1 分支。
        /// </summary>
        private static void ProcessHoldPoint(LF2Character character, WeaponPoint wpoint)
        {
            var weapon = character.GetHeldWeapon() as LF2WeaponBase;
            if (weapon == null) return;

            Vector3 holdpoint = CalcHoldPoint(character, wpoint);
            weapon.Act(character, wpoint, holdpoint);
            // weapon.Act() 内部：若投掷成功，已调用 (holder as LF2Character)?.HoldWeapon(null)
        }

        /// <summary>
        /// 将 wpoint.x/y 从精灵左上角坐标系转换为像素世界坐标（FLF 内部单位）。
        /// 对应 FLF make_point($.ps, wpoint) 的计算方式。
        ///
        ///   dir=="right" : holdX = ps.sx + wpoint.x
        ///   dir=="left"  : holdX = ps.sx + spriteWidth - wpoint.x  （水平镜像）
        ///   holdY = ps.sy + wpoint.y
        ///   holdZ = ps.sz
        /// </summary>
        private static Vector3 CalcHoldPoint(LF2LivingObject animator, WeaponPoint wpoint)
        {
            float spriteWidth = animator.Sprite?.GetWidthPx() ?? 0f;

            float holdX = (animator.PS.dir == "right")
                ? animator.PS.sx + wpoint.x
                : animator.PS.sx + spriteWidth - wpoint.x;

            float holdY = animator.PS.sy + wpoint.y;
            float holdZ = animator.PS.sz;

            return new Vector3(holdX, holdY, holdZ);
        }
    }
}
