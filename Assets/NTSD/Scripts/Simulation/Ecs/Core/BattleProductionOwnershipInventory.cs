using System;

namespace NTSD.Simulation.Ecs
{
    public enum BattleProductionOwnershipDisposition : byte
    {
        WorldCanonical = 1,
        RetainedMeasuredOracle = 2,
        UnityCompatibilityShell = 3,
    }

    public enum BattleProductionOwnershipDomain : byte
    {
        AiSensingAndDecision = 1,
        Cooldown = 2,
        CharacterStageZ = 3,
        CharacterPreFrameBounds = 4,
        CharacterFrameAdvance = 5,
        CharacterRecovery = 6,
        CharacterFrameTick = 7,
        CharacterInput = 8,
        FramePostProcess = 9,
        CharacterPostFrameTail = 10,
        HitExecutionPlan = 11,
        HumanOrDerivedCharacterShell = 12,
        DerivedWeaponShell = 13,
        DerivedSpecialAttackShell = 14,
        DerivedOtherObjectShell = 15,
    }

    public enum BattleProductionOwnershipReason : byte
    {
        CanonicalWorldStoreAndWriter = 1,
        PerformanceGateRejectedCandidate = 2,
        ParityGateRejectedCandidate = 3,
        UnityHostOrDerivedCompatibility = 4,
    }

    public enum BattleProductionOwnershipFailure : byte
    {
        None = 0,
        WorldUnavailable = 1,
        AiExecutionProfile = 2,
        AiSensing = 3,
        AiDecision = 4,
        AiUnifiedSnapshot = 5,
        Cooldown = 6,
        CharacterStageZ = 7,
        CharacterPreFrameBounds = 8,
        CharacterFrameAdvance = 9,
        CharacterRecovery = 10,
        CharacterFrameTick = 11,
        CharacterInput = 12,
        FramePostProcessOracle = 13,
        CharacterPostFrameTailOracle = 14,
        HitExecutionPlanOracle = 15,
    }

    public readonly struct BattleProductionOwnershipEntry
    {
        internal BattleProductionOwnershipEntry(
            BattleProductionOwnershipDomain domain,
            BattleProductionOwnershipDisposition disposition,
            BattleProductionOwnershipReason reason)
        {
            Domain = domain;
            Disposition = disposition;
            Reason = reason;
        }

        public BattleProductionOwnershipDomain Domain { get; }
        public BattleProductionOwnershipDisposition Disposition { get; }
        public BattleProductionOwnershipReason Reason { get; }
    }

    public readonly struct BattleProductionOwnershipConfiguration
    {
        internal BattleProductionOwnershipConfiguration(
            BattleProductionOwnershipFailure failure,
            int canonicalOwnerCount,
            int retainedMeasuredOracleCount,
            int unityCompatibilityShellCount)
        {
            Failure = failure;
            CanonicalOwnerCount = canonicalOwnerCount;
            RetainedMeasuredOracleCount = retainedMeasuredOracleCount;
            UnityCompatibilityShellCount = unityCompatibilityShellCount;
        }

        public bool Passed => Failure == BattleProductionOwnershipFailure.None;
        public BattleProductionOwnershipFailure Failure { get; }
        public int CanonicalOwnerCount { get; }
        public int RetainedMeasuredOracleCount { get; }
        public int UnityCompatibilityShellCount { get; }
    }

    /// <summary>
    /// Executable U6 production ownership manifest. It is instantiated by diagnostics
    /// outside the battle window and never participates in a simulation tick.
    /// </summary>
    public sealed class BattleProductionOwnershipInventory
    {
        public const string Schema = "ntsd-battle-production-ownership/v1";
        public const int ExpectedCanonicalOwnerCount = 8;
        public const int ExpectedRetainedMeasuredOracleCount = 3;
        public const int ExpectedUnityCompatibilityShellCount = 4;

        private readonly BattleProductionOwnershipEntry[] entries =
        {
            Canonical(BattleProductionOwnershipDomain.AiSensingAndDecision),
            Canonical(BattleProductionOwnershipDomain.Cooldown),
            Canonical(BattleProductionOwnershipDomain.CharacterStageZ),
            Canonical(BattleProductionOwnershipDomain.CharacterPreFrameBounds),
            Canonical(BattleProductionOwnershipDomain.CharacterFrameAdvance),
            Canonical(BattleProductionOwnershipDomain.CharacterRecovery),
            Canonical(BattleProductionOwnershipDomain.CharacterFrameTick),
            Canonical(BattleProductionOwnershipDomain.CharacterInput),
            MeasuredOracle(
                BattleProductionOwnershipDomain.FramePostProcess,
                BattleProductionOwnershipReason.PerformanceGateRejectedCandidate),
            MeasuredOracle(
                BattleProductionOwnershipDomain.CharacterPostFrameTail,
                BattleProductionOwnershipReason.PerformanceGateRejectedCandidate),
            MeasuredOracle(
                BattleProductionOwnershipDomain.HitExecutionPlan,
                BattleProductionOwnershipReason.ParityGateRejectedCandidate),
            CompatibilityShell(BattleProductionOwnershipDomain.HumanOrDerivedCharacterShell),
            CompatibilityShell(BattleProductionOwnershipDomain.DerivedWeaponShell),
            CompatibilityShell(BattleProductionOwnershipDomain.DerivedSpecialAttackShell),
            CompatibilityShell(BattleProductionOwnershipDomain.DerivedOtherObjectShell),
        };

        public int Count => entries.Length;

        public BattleProductionOwnershipEntry GetEntry(int index)
        {
            if ((uint)index >= (uint)entries.Length)
                throw new ArgumentOutOfRangeException(nameof(index));
            return entries[index];
        }

        public BattleProductionOwnershipConfiguration Evaluate(SimulationWorld world)
        {
            BattleProductionOwnershipFailure failure = EvaluateFailure(world);
            return new BattleProductionOwnershipConfiguration(
                failure,
                ExpectedCanonicalOwnerCount,
                ExpectedRetainedMeasuredOracleCount,
                ExpectedUnityCompatibilityShellCount);
        }

        private static BattleProductionOwnershipFailure EvaluateFailure(
            SimulationWorld world)
        {
            if (world == null)
                return BattleProductionOwnershipFailure.WorldUnavailable;
            if (world.AiExecutionProfile != BattleAiExecutionProfile.DataOrientedCanonical)
                return BattleProductionOwnershipFailure.AiExecutionProfile;
            if (world.AiSensingMode != AiSensingMode.SoAAiSensing)
                return BattleProductionOwnershipFailure.AiSensing;
            if (world.AiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical)
                return BattleProductionOwnershipFailure.AiDecision;
            if (world.AiUnifiedSnapshotExecutionMode !=
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                return BattleProductionOwnershipFailure.AiUnifiedSnapshot;
            }
            if (world.BattleEcsCooldownPassModeForDiagnostics !=
                BattleEcsCooldownPassMode.DataOriented)
            {
                return BattleProductionOwnershipFailure.Cooldown;
            }
            if (world.BattleEcsCharacterStageZPassModeForDiagnostics !=
                BattleEcsCharacterStageZPassMode.DataOriented)
            {
                return BattleProductionOwnershipFailure.CharacterStageZ;
            }
            if (world.BattleEcsCharacterPreFrameBoundsPassModeForDiagnostics !=
                BattleEcsCharacterPreFrameBoundsPassMode.DataOriented)
            {
                return BattleProductionOwnershipFailure.CharacterPreFrameBounds;
            }
            if (world.BattleEcsCharacterFrameAdvancePassModeForDiagnostics !=
                BattleEcsCharacterFrameAdvancePassMode.DataOriented)
            {
                return BattleProductionOwnershipFailure.CharacterFrameAdvance;
            }
            if (world.BattleEcsCharacterRecoveryPassModeForDiagnostics !=
                BattleEcsCharacterRecoveryPassMode.DataOriented)
            {
                return BattleProductionOwnershipFailure.CharacterRecovery;
            }
            if (world.BattleEcsCharacterFrameTickPassModeForDiagnostics !=
                BattleEcsCharacterFrameTickPassMode.DataOriented)
            {
                return BattleProductionOwnershipFailure.CharacterFrameTick;
            }
            if (world.BattleEcsCharacterInputPassModeForDiagnostics !=
                BattleEcsCharacterInputPassMode.DataOriented)
            {
                return BattleProductionOwnershipFailure.CharacterInput;
            }
            if (world.BattleEcsFramePostProcessPassModeForDiagnostics !=
                BattleEcsFramePostProcessPassMode.Legacy)
            {
                return BattleProductionOwnershipFailure.FramePostProcessOracle;
            }
            if (world.BattleEcsCharacterPostFrameTailPassModeForDiagnostics !=
                BattleEcsCharacterPostFrameTailPassMode.Legacy)
            {
                return BattleProductionOwnershipFailure.CharacterPostFrameTailOracle;
            }
            if (world.BattleHitExecutionPlanModeForDiagnostics !=
                BattleHitExecutionPlanMode.Disabled)
            {
                return BattleProductionOwnershipFailure.HitExecutionPlanOracle;
            }

            return BattleProductionOwnershipFailure.None;
        }

        private static BattleProductionOwnershipEntry Canonical(
            BattleProductionOwnershipDomain domain)
        {
            return new BattleProductionOwnershipEntry(
                domain,
                BattleProductionOwnershipDisposition.WorldCanonical,
                BattleProductionOwnershipReason.CanonicalWorldStoreAndWriter);
        }

        private static BattleProductionOwnershipEntry MeasuredOracle(
            BattleProductionOwnershipDomain domain,
            BattleProductionOwnershipReason reason)
        {
            return new BattleProductionOwnershipEntry(
                domain,
                BattleProductionOwnershipDisposition.RetainedMeasuredOracle,
                reason);
        }

        private static BattleProductionOwnershipEntry CompatibilityShell(
            BattleProductionOwnershipDomain domain)
        {
            return new BattleProductionOwnershipEntry(
                domain,
                BattleProductionOwnershipDisposition.UnityCompatibilityShell,
                BattleProductionOwnershipReason.UnityHostOrDerivedCompatibility);
        }
    }
}
