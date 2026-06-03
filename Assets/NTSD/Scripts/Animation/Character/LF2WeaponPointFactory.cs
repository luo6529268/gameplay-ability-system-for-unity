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
        public void UpdateWeaponPoints(LF2LivingObject animator,LF2FrameData frameData,List<WeaponPoint> weaponPoints)
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
                        ProcessForceDropPoint(character, wpoint);
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
            if (LF2WeaponBase.ShouldTraceHeldWeapon(weapon.StableId))
            {
                Debug.LogError($"[PickupTrace][HoldPointEnter] picker={character.StableId} weapon={weapon.StableId} charFrame={character.Frame?.N ?? -1} charState={character.GetState()} weaponFrame={weapon.Frame?.N ?? -1} weaponState={weapon.GetState()} holdpoint=({holdpoint.x},{holdpoint.y},{holdpoint.z}) wpointKind={wpoint.kind} wpointWeaponAct={wpoint.weaponact} wpointAttack={wpoint.attacking} cover={wpoint.cover}");
            }
            var actResult = weapon.Act(character, wpoint, holdpoint);
            if (LF2WeaponBase.ShouldTraceHeldWeapon(weapon.StableId))
            {
                Debug.LogError($"[PickupTrace][HoldPointResult] picker={character.StableId} weapon={weapon.StableId} weaponFrame={weapon.Frame?.N ?? -1} weaponState={weapon.GetState()} weaponPos=({weapon.PS?.x ?? 0f},{weapon.PS?.y ?? 0f},{weapon.PS?.z ?? 0f}) thrown={actResult.Thrown} forceDrop={actResult.ForceDrop} needsKind3Drop={actResult.NeedsKind3Drop}");
                LF2WeaponBase.ConsumeHeldTrace();
            }

            // 反汇编 AI_Process2 0x41B155~0x41B16D：
            // type=0 武器且 dvx≠0 时，weapon.Act() 不投掷（NeedsKind3Drop=true），
            // 此处补充调用 kind=3 强制丢弃路径
            if (actResult.NeedsKind3Drop)
                ProcessForceDropPoint(character, wpoint);
            // ForceDrop：Act() 内部已执行 ForceClearHolder + HoldWeapon(null)，无需额外处理

            // 反汇编 Entity_AI_Update 0x42CAB1：持有攻击命中后，
            // attacker.arest 写回（对应 weapon_strength_list entry.arest）
            // 武器的 ItrArestUpdate 已在 ProcessAttack 内部调用，
            // 此处将 ARest 写回持有者 character 的 arest（角色攻击冷却）
            var ar = actResult.AttackResult;
            if (ar != null && ar.HitUid != 0 && ar.ARest > 0)
                character.ItrRest.Arest = ar.ARest;
        }

        /// <summary>
        /// kind=3：强制丢弃武器。
        /// 反汇编依据：AI_Process2 (0x0041B21D~0x0041B28D)
        ///   - 角色和武器的 arest 均归零
        ///   - 武器帧 = Random_Int(6)，范围 [0, 5]
        ///   - weapon.vx = dir * (Random_Int(7) - 3)，范围 [-3, 3]
        ///   - weapon.vy = -Random_Int(4)，范围 [-3, 0]
        ///   - weapon.vz = (Random_Int(5) - 2) * 0.2（dbl_4433E0）
        /// 注意：直接操作武器字段，不通过 Drop() 以避免帧被覆盖
        /// </summary>
        private static void ProcessForceDropPoint(LF2Character character, WeaponPoint wpoint)
        {
            var weapon = character.GetHeldWeapon() as LF2WeaponBase;
            if (weapon == null) return;

            // arest 归零（双方，反汇编 0x41B226/0x41B233）
            character.ItrRest.Arest = 0;
            weapon.ItrRest.Arest = 0;

            // 武器随机帧（反汇编：Random_Int(6) → [0,5]）
            weapon.Trans.Frame(UnityEngine.Random.Range(0, 6), 0);

            // 随机速度（反汇编：0x41B242-0x41B28D）
            float dirH = weapon.Dirh();
            weapon.PS.vx = dirH * (UnityEngine.Random.Range(0, 7) - 3);
            weapon.PS.vy = -UnityEngine.Random.Range(0, 4);
            weapon.PS.vz = (UnityEngine.Random.Range(0, 5) - 2) * 0.2f; // dbl_4433E0 = 0.2

            // 脱离持有关系（直接操作，不通过 Drop() 避免帧被覆盖）
            weapon.PS.zz = 0;
            weapon.Team = 0;
            weapon.ForceClearHolder();
            character.HoldWeapon(null);
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
            float spriteWidth = animator.GetSpriteWidthPxForCollision();

            float holdX = (animator.PS.dir == "right")
                ? animator.PS.sx + wpoint.x
                : animator.PS.sx + spriteWidth - wpoint.x;

            float holdY = animator.PS.sy + wpoint.y;
            float holdZ = animator.PS.sz;

            return new Vector3(holdX, holdY, holdZ);
        }
    }
}
