using System;
using System.Collections.Generic;
using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Formal admission and compatibility adapter for the nine release WPoint
    /// scalars. Unity-only and unknown properties fail closed.
    /// </summary>
    public static class BattleWeaponPointValueAdapter
    {
        private static readonly HashSet<string> FormalPropertyNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "kind",
                "x",
                "y",
                "attacking",
                "cover",
                "weaponact",
                "dvx",
                "dvy",
                "dvz",
            };

        public static BattleWeaponPointValue FromLegacy(WeaponPoint source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateFormalAdmission(source);
            return new BattleWeaponPointValue(
                source.kind,
                source.x,
                source.y,
                source.attacking,
                source.cover,
                source.weaponact,
                source.dvx,
                source.dvy,
                source.dvz);
        }

        public static WeaponPoint ToLegacy(BattleWeaponPointValue source)
        {
            return new WeaponPoint
            {
                kind = source.Kind,
                x = source.X,
                y = source.Y,
                attacking = source.Attacking,
                cover = source.Cover,
                weaponact = source.WeaponAct,
                dvx = source.Dvx,
                dvy = source.Dvy,
                dvz = source.Dvz,
            };
        }

        public static BattleWeaponPointValue[] CopyOrdered(
            IReadOnlyList<WeaponPoint> source)
        {
            if (source == null || source.Count == 0)
                return Array.Empty<BattleWeaponPointValue>();

            var copy = new BattleWeaponPointValue[source.Count];
            for (int index = 0; index < source.Count; index++)
                copy[index] = FromLegacy(source[index]);
            return copy;
        }

        public static BattleWeaponPointValue PrimaryOrDefault(
            IReadOnlyList<BattleWeaponPointValue> source)
        {
            return source != null && source.Count > 0
                ? source[0]
                : default;
        }

        public static BattleWeaponPointValue PrimaryFromLegacyOrDefault(
            IReadOnlyList<WeaponPoint> source)
        {
            return source != null && source.Count > 0
                ? FromLegacy(source[0])
                : default;
        }

        public static void ValidateFormalPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName) ||
                !FormalPropertyNames.Contains(propertyName))
            {
                throw new InvalidOperationException(
                    $"WPoint property '{propertyName}' is outside the formal release contract.");
            }
        }

        private static void ValidateFormalAdmission(WeaponPoint source)
        {
            if (source.w != 0 ||
                source.h != 0 ||
                source.injury != 0 ||
                source.fall != 0 ||
                source.vaction != 0 ||
                source.arest != 0 ||
                source.vrest != 0 ||
                source.effect != 0 ||
                source.kill != 0 ||
                source.bdefend != 0)
            {
                throw new InvalidOperationException(
                    "Unity-only WPoint fields cannot enter formal content.");
            }

            if (source.rawProperties == null)
                return;
            foreach (string propertyName in source.rawProperties.Keys)
                ValidateFormalPropertyName(propertyName);
        }
    }
}
