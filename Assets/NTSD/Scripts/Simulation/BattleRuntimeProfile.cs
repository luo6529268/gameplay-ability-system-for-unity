using System;
using NTSD.App;

namespace NTSD.Simulation
{
    public enum CollisionBroadphaseBackend
    {
        BruteForce = 0,
        LooseQuadtree = 1
    }

    public static class CollisionBroadphaseBackendResolver
    {
        public const string BackendArgument = "-ntsdCollisionBroadphase";

        public static CollisionBroadphaseBackend Resolve(
            string explicitOverride,
            string configuredBackend)
        {
            if (TryParse(explicitOverride, out CollisionBroadphaseBackend backend))
                return backend;
            if (TryParse(configuredBackend, out backend))
                return backend;
            return CollisionBroadphaseBackend.BruteForce;
        }

        public static bool TryParse(string value, out CollisionBroadphaseBackend backend)
        {
            if (string.Equals(value, nameof(CollisionBroadphaseBackend.LooseQuadtree), StringComparison.OrdinalIgnoreCase))
            {
                backend = CollisionBroadphaseBackend.LooseQuadtree;
                return true;
            }

            if (string.Equals(value, nameof(CollisionBroadphaseBackend.BruteForce), StringComparison.OrdinalIgnoreCase))
            {
                backend = CollisionBroadphaseBackend.BruteForce;
                return true;
            }

            backend = CollisionBroadphaseBackend.BruteForce;
            return false;
        }
    }

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

    public readonly struct BattleRuntimeWorldSettings
    {
        public BattleRuntimeWorldSettings(
            BattleRuntimeProfile profile,
            int initialRuntimeSlotCapacity,
            int maxActiveRuntimeEntities,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce)
        {
            Profile = profile;
            InitialRuntimeSlotCapacity = initialRuntimeSlotCapacity;
            MaxActiveRuntimeEntities = maxActiveRuntimeEntities;
            CollisionBroadphase = collisionBroadphase;
        }

        public BattleRuntimeProfile Profile { get; }
        public int InitialRuntimeSlotCapacity { get; }
        public int MaxActiveRuntimeEntities { get; }
        public CollisionBroadphaseBackend CollisionBroadphase { get; }
    }

    public static class BattleRuntimeProfilePolicy
    {
        public const int AuthorityRuntimeSlotCapacity = 400;
        public const int MobileRuntimeSlotCapacity = 1050;
        public const int MobileMaxActiveRuntimeEntities = 1000;
        public const int DesktopDefaultInitialRuntimeSlotCapacity = 512;

        public static BattleRuntimeWorldSettings Resolve(
            string explicitOverride,
            string configuredProfile,
            BattleRuntimeProfile platformDefault,
            int configuredDesktopInitialCapacity)
        {
            BattleRuntimeProfile profile = BattleRuntimeProfileResolver.Resolve(
                explicitOverride,
                configuredProfile,
                platformDefault);
            return Create(profile, configuredDesktopInitialCapacity);
        }

        public static BattleRuntimeWorldSettings Create(
            BattleRuntimeProfile profile,
            int configuredDesktopInitialCapacity = DesktopDefaultInitialRuntimeSlotCapacity)
        {
            switch (profile)
            {
                case BattleRuntimeProfile.MobileExtended:
                    return new BattleRuntimeWorldSettings(
                        profile,
                        MobileRuntimeSlotCapacity,
                        MobileMaxActiveRuntimeEntities);
                case BattleRuntimeProfile.DesktopExtended:
                    return new BattleRuntimeWorldSettings(
                        profile,
                        NormalizeDesktopCapacity(configuredDesktopInitialCapacity),
                        int.MaxValue);
                default:
                    return new BattleRuntimeWorldSettings(
                        BattleRuntimeProfile.Authority400,
                        AuthorityRuntimeSlotCapacity,
                        AuthorityRuntimeSlotCapacity);
            }
        }

        public static int NormalizeDesktopCapacity(int requestedCapacity)
        {
            int capacity = requestedCapacity > 0
                ? requestedCapacity
                : DesktopDefaultInitialRuntimeSlotCapacity;
            capacity = Math.Max(capacity, RuntimeSlotTable.PageSize);

            long pageCount = ((long)capacity + RuntimeSlotTable.PageSize - 1) /
                RuntimeSlotTable.PageSize;
            long normalized = pageCount * RuntimeSlotTable.PageSize;
            if (normalized > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(requestedCapacity));

            return (int)normalized;
        }
    }

    public static class BattleRuntimeProfileProductionSource
    {
        public const string ProfileArgument = "-ntsdBattleRuntimeProfile";
        public const string DesktopCapacityArgument = "-ntsdDesktopRuntimeSlotCapacity";

        public static BattleRuntimeWorldSettings Resolve(
            GameConfig config,
            string[] commandLineArguments = null)
        {
            string[] arguments = commandLineArguments ?? Environment.GetCommandLineArgs();
            string explicitProfile = FindArgumentValue(arguments, ProfileArgument);
            string configuredProfile = config?.BattleRuntimeProfileName;
            string explicitBroadphase = FindArgumentValue(
                arguments,
                CollisionBroadphaseBackendResolver.BackendArgument);
            string configuredBroadphase = config?.BattleCollisionBroadphaseName;
            int desktopCapacity = config?.DesktopInitialRuntimeSlotCapacity ??
                BattleRuntimeProfilePolicy.DesktopDefaultInitialRuntimeSlotCapacity;

            string explicitCapacity = FindArgumentValue(arguments, DesktopCapacityArgument);
            if (!string.IsNullOrWhiteSpace(explicitCapacity) &&
                int.TryParse(explicitCapacity, out int parsedCapacity))
            {
                desktopCapacity = parsedCapacity;
            }

            BattleRuntimeWorldSettings profileSettings = BattleRuntimeProfilePolicy.Resolve(
                explicitProfile,
                configuredProfile,
                BattleRuntimeProfileResolver.GetPlatformDefault(),
                desktopCapacity);
            return new BattleRuntimeWorldSettings(
                profileSettings.Profile,
                profileSettings.InitialRuntimeSlotCapacity,
                profileSettings.MaxActiveRuntimeEntities,
                CollisionBroadphaseBackendResolver.Resolve(
                    explicitBroadphase,
                    configuredBroadphase));
        }

        internal static string FindArgumentValue(string[] arguments, string argumentName)
        {
            if (arguments == null || string.IsNullOrEmpty(argumentName))
                return null;

            string assignmentPrefix = argumentName + "=";
            for (int i = 0; i < arguments.Length; i++)
            {
                string argument = arguments[i];
                if (argument != null &&
                    argument.StartsWith(assignmentPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    return argument.Substring(assignmentPrefix.Length);
                }

                if (string.Equals(argument, argumentName, StringComparison.OrdinalIgnoreCase) &&
                    i + 1 < arguments.Length)
                {
                    return arguments[i + 1];
                }
            }

            return null;
        }
    }
}
