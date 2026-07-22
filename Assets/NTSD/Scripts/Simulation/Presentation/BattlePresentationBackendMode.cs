using System;
using NTSD.App;

namespace NTSD.Simulation.Presentation
{
    public enum BattlePresentationBackendMode
    {
        LegacyOnly = 0,
        CentralShadowBuild = 1,
        CentralOnly = 2,
    }

    public static class BattlePresentationBackendResolver
    {
        public const string BackendArgument = "-ntsdBattlePresentationBackend";

        public static BattlePresentationBackendMode Resolve(
            string explicitOverride,
            string configuredBackend)
        {
            if (TryParse(explicitOverride, out BattlePresentationBackendMode mode))
                return mode;
            if (TryParse(configuredBackend, out mode))
                return mode;
            return BattlePresentationBackendMode.LegacyOnly;
        }

        public static BattlePresentationBackendMode Resolve(
            GameConfig config,
            string[] commandLineArguments = null)
        {
            string[] arguments = commandLineArguments ?? Environment.GetCommandLineArgs();
            string explicitOverride = BattleRuntimeProfileProductionSource.FindArgumentValue(
                arguments,
                BackendArgument);
            return Resolve(explicitOverride, config?.BattlePresentationBackendName);
        }

        public static bool TryParse(string value, out BattlePresentationBackendMode mode)
        {
            if (string.Equals(value, nameof(BattlePresentationBackendMode.LegacyOnly), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattlePresentationBackendMode.LegacyOnly;
                return true;
            }
            if (string.Equals(value, nameof(BattlePresentationBackendMode.CentralShadowBuild), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattlePresentationBackendMode.CentralShadowBuild;
                return true;
            }
            if (string.Equals(value, nameof(BattlePresentationBackendMode.CentralOnly), StringComparison.OrdinalIgnoreCase))
            {
                mode = BattlePresentationBackendMode.CentralOnly;
                return true;
            }

            mode = BattlePresentationBackendMode.LegacyOnly;
            return false;
        }

        public static void ValidateAvailable(BattlePresentationBackendMode mode)
        {
            if (mode != BattlePresentationBackendMode.LegacyOnly &&
                mode != BattlePresentationBackendMode.CentralShadowBuild &&
                mode != BattlePresentationBackendMode.CentralOnly)
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown battle presentation backend mode.");
        }
    }
}
