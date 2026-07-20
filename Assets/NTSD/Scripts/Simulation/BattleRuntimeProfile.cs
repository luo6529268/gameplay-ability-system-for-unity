using System;

namespace NTSD.Simulation
{
    public enum BattleRuntimeProfile
    {
        Authority400 = 0,
        MobileExtended = 1,
        DesktopExtended = 2
    }

    public static class BattleRuntimeProfileResolver
    {
        public static BattleRuntimeProfile Resolve(
            string explicitOverride,
            string configuredProfile,
            BattleRuntimeProfile platformDefault)
        {
            if (TryParse(explicitOverride, out BattleRuntimeProfile resolved))
                return resolved;

            if (TryParse(configuredProfile, out resolved))
                return resolved;

            return platformDefault;
        }

        public static BattleRuntimeProfile ResolveDefault(
            string explicitOverride,
            string configuredProfile)
        {
            return Resolve(explicitOverride, configuredProfile, GetPlatformDefault());
        }

        public static bool TryParse(string value, out BattleRuntimeProfile profile)
        {
            if (string.Equals(value, nameof(BattleRuntimeProfile.Authority400), StringComparison.OrdinalIgnoreCase))
            {
                profile = BattleRuntimeProfile.Authority400;
                return true;
            }

            if (string.Equals(value, nameof(BattleRuntimeProfile.MobileExtended), StringComparison.OrdinalIgnoreCase))
            {
                profile = BattleRuntimeProfile.MobileExtended;
                return true;
            }

            if (string.Equals(value, nameof(BattleRuntimeProfile.DesktopExtended), StringComparison.OrdinalIgnoreCase))
            {
                profile = BattleRuntimeProfile.DesktopExtended;
                return true;
            }

            profile = default;
            return false;
        }

        public static BattleRuntimeProfile GetPlatformDefault()
        {
#if UNITY_EDITOR
            return BattleRuntimeProfile.Authority400;
#elif UNITY_ANDROID
            return BattleRuntimeProfile.MobileExtended;
#elif UNITY_STANDALONE
            return BattleRuntimeProfile.DesktopExtended;
#else
            return BattleRuntimeProfile.Authority400;
#endif
        }
    }
}
