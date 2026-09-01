using System;
using System.Collections.Generic;
using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Formal admission and compatibility adapter for the 19 release CPoint
    /// scalars. Unknown explicit properties fail closed.
    /// </summary>
    public static class BattleCatchPointValueAdapter
    {
        private static readonly HashSet<string> FormalPropertyNames =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "kind", "x", "y", "injury", "cover", "vaction",
                "aaction", "jaction", "daction", "throwvx", "throwvy",
                "hurtable", "decrease", "dircontrol", "taction",
                "throwinjury", "throwvz", "fronthurtact", "backhurtact",
            };

        public static BattleCatchPointValue FromLegacy(CatchPoint source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            ValidateFormalAdmission(source);
            return new BattleCatchPointValue(
                source.kind,
                source.x,
                source.y,
                source.injury,
                source.cover,
                source.vaction,
                source.aaction,
                source.jaction,
                source.daction,
                source.throwvx,
                source.throwvy,
                source.hurtable,
                source.decrease,
                source.dircontrol,
                source.taction,
                source.throwinjury,
                source.throwvz,
                source.fronthurtact,
                source.backhurtact);
        }

        public static BattleCatchPointValue PrimaryFromLegacyOrDefault(
            CatchPoint source)
        {
            return source == null ? default : FromLegacy(source);
        }

        public static void ValidateFormalPropertyName(string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName) ||
                !FormalPropertyNames.Contains(propertyName))
            {
                throw new InvalidOperationException(
                    $"CPoint property '{propertyName}' is outside the formal release contract.");
            }
        }

        private static void ValidateFormalAdmission(CatchPoint source)
        {
            if (source.rawProperties == null)
                return;
            foreach (string propertyName in source.rawProperties.Keys)
                ValidateFormalPropertyName(propertyName);
        }
    }
}
