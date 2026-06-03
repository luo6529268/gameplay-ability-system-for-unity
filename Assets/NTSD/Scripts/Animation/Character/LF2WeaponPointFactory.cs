using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Animation
{
    public class LF2WeaponPointFactory : MMSingleton<LF2WeaponPointFactory>, ILF2WeaponPointFactory
    {
        public void UpdateWeaponPoints(LF2LivingObject animator, LF2FrameData frameData, List<WeaponPoint> weaponPoints)
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

        private static void ProcessHoldPoint(LF2Character character, WeaponPoint wpoint)
        {
            var weapon = character.GetHeldWeapon() as LF2WeaponBase;
            if (weapon == null) return;

            Vector3 holdpoint = CalcHoldPoint(character, wpoint);
            var actResult = weapon.Act(character, wpoint, holdpoint);

            if (actResult.NeedsKind3Drop)
                ProcessForceDropPoint(character, wpoint);

            var ar = actResult.AttackResult;
            if (ar != null && ar.HitUid != 0 && ar.ARest > 0)
                character.ItrRest.Arest = ar.ARest;
        }

        private static void ProcessForceDropPoint(LF2Character character, WeaponPoint wpoint)
        {
            var weapon = character.GetHeldWeapon() as LF2WeaponBase;
            if (weapon == null) return;

            character.ItrRest.Arest = 0;
            weapon.ItrRest.Arest = 0;

            weapon.Trans.Frame(UnityEngine.Random.Range(0, 6), 0);

            float dirH = weapon.Dirh();
            weapon.PS.vx = dirH * (UnityEngine.Random.Range(0, 7) - 3);
            weapon.PS.vy = -UnityEngine.Random.Range(0, 4);
            weapon.PS.vz = (UnityEngine.Random.Range(0, 5) - 2) * 0.2f;

            weapon.PS.zz = 0;
            weapon.Team = 0;
            weapon.ForceClearHolder();
            character.HoldWeapon(null);
        }

        private static Vector3 CalcHoldPoint(LF2LivingObject animator, WeaponPoint wpoint)
        {
            float spriteWidth = animator.GetSpriteWidthPxForCollision();

            float holdX = animator.PS.dir == "right"
                ? animator.PS.sx + wpoint.x
                : animator.PS.sx + spriteWidth - wpoint.x;

            float holdY = animator.PS.sy + wpoint.y;
            float holdZ = animator.PS.sz;

            return new Vector3(holdX, holdY, holdZ);
        }
    }
}
