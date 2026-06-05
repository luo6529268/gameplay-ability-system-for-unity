using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;

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
                        ProcessDropPoint(character, wpoint);
                        break;
                }
            }
        }

        private static void ProcessHoldPoint(LF2Character character, WeaponPoint wpoint)
        {
            if (!character.ReleaseHeldObjectByWPoint(wpoint, out var actResult))
                return;

            if (actResult.NeedsKind3Drop)
                ProcessDropPoint(character, wpoint);

            var ar = actResult.AttackResult;
            if (ar != null && ar.HitUid != 0 && ar.ARest > 0)
                character.ItrRest.Arest = ar.ARest;
        }

        private static void ProcessDropPoint(LF2Character character, WeaponPoint wpoint)
        {
            if (character.ReleaseHeldObjectByWPoint(wpoint, out _))
                return;

            ProcessWeaponFallbackDrop(character);
        }

        private static void ProcessWeaponFallbackDrop(LF2Character character)
        {
            var weapon = character.GetHeldWeapon() as LF2WeaponBase;
            if (weapon == null) return;

            character.ItrRest.Arest = 0;
            weapon.ItrRest.Arest = 0;

            weapon.SetFrameDirect(weapon.BattleRandInt(0, 6));
            weapon.PS.vx = weapon.BattleRandInt(0, 7) - 3;
            weapon.PS.vy = -weapon.BattleRandInt(0, 4);
            weapon.PS.vz = (weapon.BattleRandInt(0, 5) - 2) * 0.2f;
            weapon.PS.zz = 0;
            weapon.Team = 0;
            weapon.ForceClearHolder();
            character.HoldWeapon(null);
        }
    }
}
