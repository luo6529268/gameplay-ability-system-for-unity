using System;
using NTSD.App;

namespace NTSD.Simulation
{
    public enum BattleAiExecutionProfile
    {
        LegacyCanonical = 0,
        DataOrientedCanonical = 1,
    }

    public static class BattleAiExecutionProfileResolver
    {
        public const string ProfileArgument = "-ntsdBattleAiExecutionProfile";

        public static BattleAiExecutionProfile Resolve(
            string explicitOverride,
            string configuredProfile)
        {
            if (!string.IsNullOrWhiteSpace(explicitOverride))
                return ParseRequired(explicitOverride, nameof(explicitOverride));
            if (!string.IsNullOrWhiteSpace(configuredProfile))
                return ParseRequired(configuredProfile, nameof(configuredProfile));
            return BattleAiExecutionProfile.DataOrientedCanonical;
        }

        public static bool TryParse(string value, out BattleAiExecutionProfile profile)
        {
            string normalized = (value ?? string.Empty).Trim();
            if (string.Equals(
                    normalized,
                    nameof(BattleAiExecutionProfile.LegacyCanonical),
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "legacy", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "legacy-canonical", StringComparison.OrdinalIgnoreCase))
            {
                profile = BattleAiExecutionProfile.LegacyCanonical;
                return true;
            }

            if (string.Equals(
                    normalized,
                    nameof(BattleAiExecutionProfile.DataOrientedCanonical),
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "data-oriented", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(
                    normalized,
                    "data-oriented-canonical",
                    StringComparison.OrdinalIgnoreCase))
            {
                profile = BattleAiExecutionProfile.DataOrientedCanonical;
                return true;
            }

            profile = BattleAiExecutionProfile.LegacyCanonical;
            return false;
        }

        public static string Format(BattleAiExecutionProfile profile)
        {
            switch (profile)
            {
                case BattleAiExecutionProfile.LegacyCanonical:
                    return "legacy";
                case BattleAiExecutionProfile.DataOrientedCanonical:
                    return "data-oriented-canonical";
                default:
                    throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }
        }

        private static BattleAiExecutionProfile ParseRequired(string value, string parameterName)
        {
            if (TryParse(value, out BattleAiExecutionProfile profile))
                return profile;

            throw new ArgumentException(
                $"Unknown battle AI execution profile '{value}'. Expected legacy or data-oriented-canonical.",
                parameterName);
        }
    }

    public static class BattleAiExecutionProfileProductionSource
    {
        public static BattleAiExecutionProfile Resolve(
            GameConfig config,
            string[] commandLineArguments = null)
        {
            string[] arguments = commandLineArguments ?? Environment.GetCommandLineArgs();
            string explicitProfile = BattleRuntimeProfileProductionSource.FindArgumentValue(
                arguments,
                BattleAiExecutionProfileResolver.ProfileArgument);
            return BattleAiExecutionProfileResolver.Resolve(
                explicitProfile,
                config?.BattleAiExecutionProfileName);
        }
    }
}
