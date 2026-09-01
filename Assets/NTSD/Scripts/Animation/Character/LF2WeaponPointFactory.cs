using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Animation
{
    public class LF2WeaponPointFactory : MMSingleton<LF2WeaponPointFactory>, ILF2WeaponPointFactory
    {
        public void UpdateWeaponPoints(
            LF2LivingObject animator,
            LF2FrameData frameData,
            IReadOnlyList<BattleWeaponPointValue> weaponPoints)
        {
            if (animator == null || weaponPoints == null) return;

            var character = animator as LF2Character;
            if (character == null) return;

            foreach (var wpoint in weaponPoints)
            {
                switch (wpoint.Kind)
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

        private static void ProcessHoldPoint(
            LF2Character character,
            BattleWeaponPointValue wpoint)
        {
            if (!character.ReleaseHeldObjectByWPoint(wpoint, out var actResult))
                return;

            if (actResult.NeedsKind3Drop)
                character.DropHeldObjectByWPoint(wpoint);

            var ar = actResult.AttackResult;
            if (ar.HitUid != 0 && ar.ARest > 0)
                character.ItrRest.Arest = ar.ARest;
        }

        private static void ProcessDropPoint(
            LF2Character character,
            BattleWeaponPointValue wpoint)
        {
            if (character.ReleaseHeldObjectByWPoint(wpoint, out _))
                return;

            character.TryDropHeldWeaponFallbackRandomly();
        }
    }
}
