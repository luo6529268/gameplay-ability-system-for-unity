#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NTSD.Simulation.Spatial;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using Stopwatch = System.Diagnostics.Stopwatch;

namespace NTSD.Animation.Rendering.Editor
{
    public enum ProductionEntityStressMode
    {
        Smoke50,
        Dispersed1000,
        Concentrated1000,
    }

    internal enum ProductionEntityStressInputMode
    {
        Ai,
        None,
    }

    internal enum ProductionEntityStressSoundPresentationMode
    {
        Inherit,
        Suppress,
        Dispatch,
    }

    [Serializable]
    public sealed class ProductionEntityStressRequest
    {
        public string action = "dispersed";
        public string inputMode = "ai";
        // Zero preserves the action's legacy population. Dispersed workloads may
        // explicitly select only the frozen 100/300/500/1000 ladder.
        public int entityCount;
        public int warmupTicks = 30;
        public int sampleTicks = 300;
        public int spawnBatchSize = 25;
        public int maxCatchUpTicksPerFrame = 4;
        public int maxBacklogTicks = 8;
        public int maxSaturationDrainTicks = 300;
        public bool enablePhaseTiming;
        public bool enablePresentationTiming;
        public bool enableDetailPhaseTiming;
        public bool enableCollisionCandidateStoreShadow;
        public bool enableCollisionCandidateStoreAuthority;
        public bool enableCollisionRoleZeroItrFastPath;
        public int legacyOracleInterval = 1;
        public bool simulationOnly;
        public string soundPresentationMode = "inherit";
        public bool skipLateRendererUpdate;
        public bool autoStopWhenSampled;
        public uint seed = 0x4E545344u;
        public string aiExecutionProfile = "";
        // Legacy compatibility fields for request files written before the atomic
        // production profile was introduced. New requests should use aiExecutionProfile.
        public string aiSensingMode = "legacy";
        public string lateRuntimeSnapshotMode = "consolidated-final";
        public bool allowUnsafeAiSoACandidate = false;
        public bool enableAiSoADecisionRemainder;
        public bool enableAiDecisionSoAShadow;
        public bool enableAiDecisionSharedShadow;
        public string aiDecisionExecutionMode = "legacy";
        public int aiDecisionFullOracleSampleInterval;
        public bool enableUnifiedAiSnapshotShadow;
        public string aiUnifiedSnapshotExecutionMode = "legacy";
        public bool writeFinalParitySnapshotJson;
        public string formalCollectorMode = "configured";
        public bool forceRoleAwareDirect;
        public bool forceRoleAwareTree;
        public bool forceRoleAwareNestedDirect;
        public bool forceRoleAwareSweepDirect;
        public string outputPath = "Temp/NTSD_ProductionEntityStress.dispersed.json";
    }

    internal readonly struct ProductionEntityStressConfig
    {
        internal ProductionEntityStressConfig(
            ProductionEntityStressMode mode,
            ProductionEntityStressInputMode inputMode,
            int entityCount,
            int warmupTicks,
            int sampleTicks,
            int spawnBatchSize,
            int maxCatchUpTicksPerFrame,
            int maxBacklogTicks,
            bool enablePhaseTiming,
            bool enablePresentationTiming,
            bool enableDetailPhaseTiming,
            bool enableCollisionCandidateStoreShadow,
            bool enableCollisionCandidateStoreAuthority,
            bool enableCollisionRoleZeroItrFastPath,
            int legacyOracleInterval,
            bool simulationOnly,
            ProductionEntityStressSoundPresentationMode soundPresentationMode,
            bool skipLateRendererUpdate,
            bool autoStopWhenSampled,
            uint seed,
            AiSensingMode aiSensingMode,
            BattleLateRuntimeSnapshotMode lateRuntimeSnapshotMode,
            bool allowUnsafeAiSoACandidate,
            bool enableAiSoADecisionRemainder,
            bool writeFinalParitySnapshotJson,
            CollisionFormalCollectorMode formalCollectorMode,
            bool forceRoleAwareDirect,
            bool forceRoleAwareTree,
            bool forceRoleAwareNestedDirect,
            bool forceRoleAwareSweepDirect,
            string outputPath,
            bool enableAiDecisionSoAShadow = false,
            bool enableAiDecisionSharedShadow = false,
            AiDecisionExecutionMode aiDecisionExecutionMode =
                AiDecisionExecutionMode.Legacy,
            int aiDecisionFullOracleSampleInterval = 0,
            bool enableUnifiedAiSnapshotShadow = false,
            AiUnifiedSnapshotExecutionMode aiUnifiedSnapshotExecutionMode =
                AiUnifiedSnapshotExecutionMode.LegacySeparate,
            int maxSaturationDrainTicks = 300,
            BattleAiExecutionProfile aiExecutionProfile =
                BattleAiExecutionProfile.LegacyCanonical,
            bool usesLegacyAiConfigurationCompatibility = true)
        {
            Mode = mode;
            InputMode = inputMode;
            EntityCount = entityCount;
            WarmupTicks = Math.Max(0, warmupTicks);
            SampleTicks = Math.Max(1, sampleTicks);
            SpawnBatchSize = Math.Max(1, Math.Min(100, spawnBatchSize));
            MaxCatchUpTicksPerFrame = Math.Max(1, maxCatchUpTicksPerFrame);
            MaxBacklogTicks = Math.Max(MaxCatchUpTicksPerFrame, maxBacklogTicks);
            MaxSaturationDrainTicks = Math.Max(1, maxSaturationDrainTicks);
            EnablePhaseTiming = enablePhaseTiming;
            EnablePresentationTiming = enablePresentationTiming;
            EnableDetailPhaseTiming = enableDetailPhaseTiming;
            EnableCollisionCandidateStoreShadow = enableCollisionCandidateStoreShadow;
            EnableCollisionCandidateStoreAuthority = enableCollisionCandidateStoreAuthority;
            EnableCollisionRoleZeroItrFastPath = enableCollisionRoleZeroItrFastPath;
            LegacyOracleInterval = Math.Max(0, legacyOracleInterval);
            SimulationOnly = simulationOnly;
            SoundPresentationMode = soundPresentationMode;
            SkipLateRendererUpdate = skipLateRendererUpdate;
            AutoStopWhenSampled = autoStopWhenSampled;
            Seed = seed;
            AiSensingMode = aiSensingMode;
            LateRuntimeSnapshotMode = lateRuntimeSnapshotMode;
            AllowUnsafeAiSoACandidate = allowUnsafeAiSoACandidate;
            EnableAiSoADecisionRemainder = enableAiSoADecisionRemainder;
            EnableAiDecisionSoAShadow = enableAiDecisionSoAShadow;
            EnableAiDecisionSharedShadow = enableAiDecisionSharedShadow;
            AiDecisionExecutionMode = aiDecisionExecutionMode;
            AiDecisionFullOracleSampleInterval = Math.Max(
                0,
                aiDecisionFullOracleSampleInterval);
            EnableUnifiedAiSnapshotShadow = enableUnifiedAiSnapshotShadow;
            AiUnifiedSnapshotExecutionMode = aiUnifiedSnapshotExecutionMode;
            AiExecutionProfile = aiExecutionProfile;
            UsesLegacyAiConfigurationCompatibility =
                usesLegacyAiConfigurationCompatibility;
            if (!UsesLegacyAiConfigurationCompatibility)
            {
                AiSensingMode = AiExecutionProfile ==
                    BattleAiExecutionProfile.DataOrientedCanonical
                        ? AiSensingMode.SoAAiSensing
                        : AiSensingMode.LegacyAiSensing;
                AiDecisionExecutionMode = AiExecutionProfile ==
                    BattleAiExecutionProfile.DataOrientedCanonical
                        ? AiDecisionExecutionMode.IndexedCanonical
                        : AiDecisionExecutionMode.Legacy;
                AiUnifiedSnapshotExecutionMode = AiExecutionProfile ==
                    BattleAiExecutionProfile.DataOrientedCanonical
                        ? AiUnifiedSnapshotExecutionMode.UnifiedAuthority
                        : AiUnifiedSnapshotExecutionMode.LegacySeparate;
            }
            WriteFinalParitySnapshotJson = writeFinalParitySnapshotJson;
            FormalCollectorMode = formalCollectorMode;
            ForceRoleAwareDirect = forceRoleAwareDirect;
            ForceRoleAwareTree = forceRoleAwareTree;
            ForceRoleAwareNestedDirect = forceRoleAwareNestedDirect;
            ForceRoleAwareSweepDirect = forceRoleAwareSweepDirect;
            OutputPath = outputPath ?? string.Empty;
            if (UsesLegacyAiConfigurationCompatibility &&
                AiSensingMode == AiSensingMode.SoAAiSensing &&
                !AllowUnsafeAiSoACandidate)
            {
                throw new ArgumentException(
                    "Candidate AI sensing requires the explicit Diagnostic/Unsafe opt-in.",
                    nameof(allowUnsafeAiSoACandidate));
            }
            if (EnableAiSoADecisionRemainder &&
                AiSensingMode != AiSensingMode.SoAAiSensing)
            {
                throw new ArgumentException(
                    "AI SoA decision remainder requires candidate AI sensing.",
                    nameof(enableAiSoADecisionRemainder));
            }
            if (EnableAiDecisionSoAShadow && EnableAiDecisionSharedShadow)
            {
                throw new ArgumentException(
                    "AI decision Shadow and SharedShadow are mutually exclusive diagnostics.",
                    nameof(enableAiDecisionSharedShadow));
            }
            if (AiDecisionExecutionMode == AiDecisionExecutionMode.IndexedCanonical &&
                (EnableAiDecisionSoAShadow || EnableAiDecisionSharedShadow))
            {
                throw new ArgumentException(
                    "IndexedCanonical execution uses its own sampled Full oracle and cannot be combined with decision shadow modes.",
                    nameof(aiDecisionExecutionMode));
            }
            if (EnableUnifiedAiSnapshotShadow &&
                AiSensingMode == AiSensingMode.LegacyAiSensing &&
                AiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical &&
                !EnableAiDecisionSharedShadow)
            {
                throw new ArgumentException(
                    "Unified AI snapshot shadow requires a SoA sensing snapshot or shared indexed decision rows.",
                    nameof(enableUnifiedAiSnapshotShadow));
            }
            if (AiUnifiedSnapshotExecutionMode ==
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                if (EnableUnifiedAiSnapshotShadow)
                {
                    throw new ArgumentException(
                        "Unified AI snapshot authority and Gate-A shadow are mutually exclusive.",
                        nameof(aiUnifiedSnapshotExecutionMode));
                }
                if (AiSensingMode != AiSensingMode.SoAAiSensing ||
                    AiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical)
                {
                    throw new ArgumentException(
                        "Unified AI snapshot authority requires SoAAiSensing and IndexedCanonical decision execution.",
                        nameof(aiUnifiedSnapshotExecutionMode));
                }
            }
            if (ForceRoleAwareTree &&
                (ForceRoleAwareDirect ||
                 ForceRoleAwareNestedDirect ||
                 ForceRoleAwareSweepDirect))
            {
                throw new ArgumentException(
                    "forceRoleAwareTree is mutually exclusive with " +
                    "forceRoleAwareDirect, forceRoleAwareNestedDirect, and " +
                    "forceRoleAwareSweepDirect diagnostics.",
                    nameof(forceRoleAwareTree));
            }
            if (ForceRoleAwareNestedDirect && ForceRoleAwareSweepDirect)
            {
                throw new ArgumentException(
                    "forceRoleAwareNestedDirect and forceRoleAwareSweepDirect " +
                    "are mutually exclusive diagnostics.",
                    nameof(forceRoleAwareSweepDirect));
            }
            if (SkipLateRendererUpdate && !SimulationOnly)
            {
                throw new ArgumentException(
                    "skipLateRendererUpdate requires simulationOnly=true.",
                    nameof(skipLateRendererUpdate));
            }
        }

        internal ProductionEntityStressMode Mode { get; }
        internal ProductionEntityStressInputMode InputMode { get; }
        internal int EntityCount { get; }
        internal int WarmupTicks { get; }
        internal int SampleTicks { get; }
        internal int SpawnBatchSize { get; }
        internal int MaxCatchUpTicksPerFrame { get; }
        internal int MaxBacklogTicks { get; }
        internal int MaxSaturationDrainTicks { get; }
        internal bool EnablePhaseTiming { get; }
        internal bool EnablePresentationTiming { get; }
        internal bool EnableDetailPhaseTiming { get; }
        internal bool EnableCollisionCandidateStoreShadow { get; }
        internal bool EnableCollisionCandidateStoreAuthority { get; }
        internal bool EnableCollisionRoleZeroItrFastPath { get; }
        internal int LegacyOracleInterval { get; }
        internal bool SimulationOnly { get; }
        internal ProductionEntityStressSoundPresentationMode SoundPresentationMode { get; }
        internal bool SuppressSoundPresentation =>
            SoundPresentationMode == ProductionEntityStressSoundPresentationMode.Suppress ||
            (SoundPresentationMode == ProductionEntityStressSoundPresentationMode.Inherit &&
             SimulationOnly);
        internal bool SkipLateRendererUpdate { get; }
        internal uint Seed { get; }
        internal AiSensingMode AiSensingMode { get; }
        internal BattleAiExecutionProfile AiExecutionProfile { get; }
        internal bool UsesLegacyAiConfigurationCompatibility { get; }
        internal BattleLateRuntimeSnapshotMode LateRuntimeSnapshotMode { get; }
        internal bool AllowUnsafeAiSoACandidate { get; }
        internal bool EnableAiSoADecisionRemainder { get; }
        internal bool EnableAiDecisionSoAShadow { get; }
        internal bool EnableAiDecisionSharedShadow { get; }
        internal AiDecisionExecutionMode AiDecisionExecutionMode { get; }
        internal int AiDecisionFullOracleSampleInterval { get; }
        internal bool EnableUnifiedAiSnapshotShadow { get; }
        internal AiUnifiedSnapshotExecutionMode AiUnifiedSnapshotExecutionMode { get; }
        internal AiDecisionShadowMode RequestedAiDecisionShadowMode =>
            EnableAiDecisionSharedShadow
                ? AiDecisionShadowMode.SharedShadow
                : EnableAiDecisionSoAShadow
                    ? AiDecisionShadowMode.Shadow
                    : AiDecisionShadowMode.Disabled;
        internal bool WriteFinalParitySnapshotJson { get; }
        internal CollisionFormalCollectorMode FormalCollectorMode { get; }
        internal bool ForceRoleAwareDirect { get; }
        internal bool ForceRoleAwareTree { get; }
        internal bool ForceRoleAwareNestedDirect { get; }
        internal bool ForceRoleAwareSweepDirect { get; }
        internal string OutputPath { get; }
        internal bool AutoCleanup => Mode == ProductionEntityStressMode.Smoke50;
        internal bool AutoStopWhenSampled { get; }
        internal bool ShouldAutoStopWhenSampled => AutoCleanup || AutoStopWhenSampled;

        internal static ProductionEntityStressConfig FromRequest(
            ProductionEntityStressRequest request,
            string projectRoot)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            string action = (request.action ?? string.Empty).Trim().ToLowerInvariant();
            ProductionEntityStressMode mode;
            int actionEntityCount = 0;
            switch (action)
            {
                case "smoke":
                case "smoke50":
                    mode = ProductionEntityStressMode.Smoke50;
                    break;
                case "dispersed":
                case "dispersed1000":
                    mode = ProductionEntityStressMode.Dispersed1000;
                    actionEntityCount = action == "dispersed1000" ? 1000 : 0;
                    break;
                case "dispersed100":
                    mode = ProductionEntityStressMode.Dispersed1000;
                    actionEntityCount = 100;
                    break;
                case "dispersed300":
                    mode = ProductionEntityStressMode.Dispersed1000;
                    actionEntityCount = 300;
                    break;
                case "dispersed500":
                    mode = ProductionEntityStressMode.Dispersed1000;
                    actionEntityCount = 500;
                    break;
                case "concentrated":
                case "concentrated1000":
                    mode = ProductionEntityStressMode.Concentrated1000;
                    break;
                default:
                    throw new ArgumentException(
                        $"Unknown production stress action '{request.action}'.",
                        nameof(request));
            }

            string outputPath = string.IsNullOrWhiteSpace(request.outputPath)
                ? $"Temp/NTSD_ProductionEntityStress.{action}.json"
                : request.outputPath;
            outputPath = Path.IsPathRooted(outputPath)
                ? Path.GetFullPath(outputPath)
                : Path.GetFullPath(Path.Combine(projectRoot, outputPath));

            int warmupTicks = mode == ProductionEntityStressMode.Smoke50
                ? Math.Min(Math.Max(0, request.warmupTicks), 5)
                : request.warmupTicks;
            int sampleTicks = mode == ProductionEntityStressMode.Smoke50
                ? Math.Min(Math.Max(1, request.sampleTicks), 30)
                : request.sampleTicks;
            int entityCount = ResolveEntityCount(mode, actionEntityCount, request.entityCount);
            CollisionFormalCollectorMode formalCollectorMode =
                ParseFormalCollectorMode(request.formalCollectorMode);
            bool usesLegacyAiConfigurationCompatibility =
                string.IsNullOrWhiteSpace(request.aiExecutionProfile);
            AiSensingMode requestedAiSensingMode = ParseAiSensingMode(
                request.aiSensingMode,
                request.allowUnsafeAiSoACandidate);
            AiDecisionExecutionMode requestedAiDecisionMode =
                ParseAiDecisionExecutionMode(request.aiDecisionExecutionMode);
            AiUnifiedSnapshotExecutionMode requestedUnifiedMode =
                ParseAiUnifiedSnapshotExecutionMode(
                    request.aiUnifiedSnapshotExecutionMode);
            BattleAiExecutionProfile aiExecutionProfile =
                usesLegacyAiConfigurationCompatibility
                    ? DeriveLegacyAiExecutionProfile(
                        requestedAiSensingMode,
                        requestedAiDecisionMode,
                        requestedUnifiedMode)
                    : ParseAiExecutionProfile(request.aiExecutionProfile);
            return new ProductionEntityStressConfig(
                mode,
                ParseInputMode(request.inputMode),
                entityCount,
                warmupTicks,
                sampleTicks,
                request.spawnBatchSize,
                request.maxCatchUpTicksPerFrame,
                request.maxBacklogTicks,
                request.enablePhaseTiming,
                request.enablePresentationTiming,
                request.enableDetailPhaseTiming,
                request.enableCollisionCandidateStoreShadow,
                request.enableCollisionCandidateStoreAuthority,
                request.enableCollisionRoleZeroItrFastPath,
                request.legacyOracleInterval,
                request.simulationOnly,
                ParseSoundPresentationMode(request.soundPresentationMode),
                request.skipLateRendererUpdate,
                request.autoStopWhenSampled,
                request.seed,
                requestedAiSensingMode,
                ParseLateRuntimeSnapshotMode(request.lateRuntimeSnapshotMode),
                request.allowUnsafeAiSoACandidate,
                request.enableAiSoADecisionRemainder,
                request.writeFinalParitySnapshotJson,
                formalCollectorMode,
                request.forceRoleAwareDirect,
                request.forceRoleAwareTree,
                request.forceRoleAwareNestedDirect,
                request.forceRoleAwareSweepDirect,
                outputPath,
                request.enableAiDecisionSoAShadow,
                request.enableAiDecisionSharedShadow,
                requestedAiDecisionMode,
                request.aiDecisionFullOracleSampleInterval,
                request.enableUnifiedAiSnapshotShadow,
                requestedUnifiedMode,
                request.maxSaturationDrainTicks,
                aiExecutionProfile,
                usesLegacyAiConfigurationCompatibility);
        }

        internal static BattleAiExecutionProfile ParseAiExecutionProfile(string value)
        {
            return BattleAiExecutionProfileResolver.Resolve(value, null);
        }

        internal static string FormatAiExecutionProfile(BattleAiExecutionProfile profile)
        {
            return BattleAiExecutionProfileResolver.Format(profile);
        }

        private static BattleAiExecutionProfile DeriveLegacyAiExecutionProfile(
            AiSensingMode sensingMode,
            AiDecisionExecutionMode decisionMode,
            AiUnifiedSnapshotExecutionMode unifiedMode)
        {
            return sensingMode == AiSensingMode.SoAAiSensing
                ? BattleAiExecutionProfile.DataOrientedCanonical
                : BattleAiExecutionProfile.LegacyCanonical;
        }

        internal static AiUnifiedSnapshotExecutionMode ParseAiUnifiedSnapshotExecutionMode(
            string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                case "legacy":
                case "legacy-separate":
                    return AiUnifiedSnapshotExecutionMode.LegacySeparate;
                case "unified":
                case "unified-authority":
                    return AiUnifiedSnapshotExecutionMode.UnifiedAuthority;
                default:
                    throw new ArgumentException(
                        $"Unknown unified AI snapshot execution mode '{value}'. Expected legacy or unified-authority.",
                        nameof(value));
            }
        }

        internal static AiDecisionExecutionMode ParseAiDecisionExecutionMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                case "legacy":
                    return AiDecisionExecutionMode.Legacy;
                case "indexed":
                case "indexed-canonical":
                    return AiDecisionExecutionMode.IndexedCanonical;
                default:
                    throw new ArgumentException(
                        $"Unknown AI decision execution mode '{value}'. Expected legacy or indexed-canonical.",
                        nameof(value));
            }
        }

        internal static int ResolveEntityCount(
            ProductionEntityStressMode mode,
            int actionEntityCount,
            int requestedEntityCount)
        {
            int fixedCount = mode == ProductionEntityStressMode.Smoke50 ? 50 : 1000;
            if (mode != ProductionEntityStressMode.Dispersed1000)
            {
                if (requestedEntityCount != 0 && requestedEntityCount != fixedCount)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(requestedEntityCount),
                        requestedEntityCount,
                        $"{mode} only supports {fixedCount} entities.");
                }

                return fixedCount;
            }

            int resolvedEntityCount = actionEntityCount != 0
                ? actionEntityCount
                : requestedEntityCount == 0 ? fixedCount : requestedEntityCount;
            if (actionEntityCount != 0 &&
                requestedEntityCount != 0 &&
                requestedEntityCount != actionEntityCount)
            {
                throw new ArgumentException(
                    $"The dispersed action fixes entityCount at {actionEntityCount}, " +
                    $"but the request specified {requestedEntityCount}.",
                    nameof(requestedEntityCount));
            }
            if (resolvedEntityCount != 100 &&
                resolvedEntityCount != 300 &&
                resolvedEntityCount != 500 &&
                resolvedEntityCount != 1000)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(requestedEntityCount),
                    resolvedEntityCount,
                    "Dispersed stress workloads support only 100, 300, 500, or 1000 entities.");
            }

            return resolvedEntityCount;
        }

        internal static ProductionEntityStressInputMode ParseInputMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                case "ai":
                    return ProductionEntityStressInputMode.Ai;
                case "none":
                    return ProductionEntityStressInputMode.None;
                default:
                    throw new ArgumentException(
                        $"Unknown production stress input mode '{value}'. Expected ai or none.",
                        nameof(value));
            }
        }

        internal static string FormatInputMode(ProductionEntityStressInputMode mode)
        {
            switch (mode)
            {
                case ProductionEntityStressInputMode.Ai:
                    return "ai";
                case ProductionEntityStressInputMode.None:
                    return "none";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        internal static AiSensingMode ParseAiSensingMode(string value)
        {
            return ParseAiSensingMode(value, allowUnsafeAiSoACandidate: false);
        }

        internal static AiSensingMode ParseAiSensingMode(
            string value,
            bool allowUnsafeAiSoACandidate)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                case "legacy":
                    return AiSensingMode.LegacyAiSensing;
                case "shadow":
                    return AiSensingMode.SoAShadowAiSensing;
                case "candidate":
                    if (allowUnsafeAiSoACandidate)
                        return AiSensingMode.SoAAiSensing;
                    throw new ArgumentException(
                        "AI sensing mode 'candidate' requires allowUnsafeAiSoACandidate=true. " +
                        "Candidate is Diagnostic/Unsafe and is disabled by default.",
                        nameof(value));
                default:
                    throw new ArgumentException(
                        $"Unknown AI sensing mode '{value}'. Expected legacy or shadow; " +
                        "SoAAiSensing is intentionally unavailable to the stress protocol.",
                        nameof(value));
            }
        }

        internal static string FormatAiSensingMode(AiSensingMode mode)
        {
            switch (mode)
            {
                case AiSensingMode.LegacyAiSensing:
                    return "legacy";
                case AiSensingMode.SoAShadowAiSensing:
                    return "shadow";
                case AiSensingMode.SoAAiSensing:
                    return "candidate";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(mode),
                        mode,
                        "The production stress protocol exposes legacy, shadow, and explicitly opted-in candidate AI sensing.");
            }
        }

        internal static ProductionEntityStressSoundPresentationMode
            ParseSoundPresentationMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                case "inherit":
                    return ProductionEntityStressSoundPresentationMode.Inherit;
                case "suppress":
                    return ProductionEntityStressSoundPresentationMode.Suppress;
                case "dispatch":
                    return ProductionEntityStressSoundPresentationMode.Dispatch;
                default:
                    throw new ArgumentException(
                        $"Unknown sound presentation mode '{value}'. " +
                        "Expected inherit, suppress, or dispatch.",
                        nameof(value));
            }
        }

        internal static string FormatSoundPresentationMode(
            ProductionEntityStressSoundPresentationMode mode)
        {
            switch (mode)
            {
                case ProductionEntityStressSoundPresentationMode.Inherit:
                    return "inherit";
                case ProductionEntityStressSoundPresentationMode.Suppress:
                    return "suppress";
                case ProductionEntityStressSoundPresentationMode.Dispatch:
                    return "dispatch";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        internal static string FormatResolvedSoundPresentationMode(
            ProductionEntityStressConfig config)
        {
            return config.SuppressSoundPresentation ? "suppress" : "dispatch";
        }

        internal static BattleLateRuntimeSnapshotMode ParseLateRuntimeSnapshotMode(
            string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "":
                    return BattleLateRuntimeSnapshotMode.ConsolidatedFinal;
                case "legacy":
                case "legacy-three":
                    return BattleLateRuntimeSnapshotMode.LegacyThree;
                case "consolidated":
                case "consolidated-final":
                    return BattleLateRuntimeSnapshotMode.ConsolidatedFinal;
                default:
                    throw new ArgumentException(
                        $"Unknown late runtime snapshot mode '{value}'. " +
                        "Expected legacy-three or consolidated-final.",
                        nameof(value));
            }
        }

        internal static string FormatLateRuntimeSnapshotMode(
            BattleLateRuntimeSnapshotMode mode)
        {
            switch (mode)
            {
                case BattleLateRuntimeSnapshotMode.LegacyThree:
                    return "legacy-three";
                case BattleLateRuntimeSnapshotMode.ConsolidatedFinal:
                    return "consolidated-final";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        internal static CollisionFormalCollectorMode ParseFormalCollectorMode(string value)
        {
            string normalized = (value ?? string.Empty).Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "configured":
                    return CollisionFormalCollectorMode.Configured;
                case "legacy":
                    return CollisionFormalCollectorMode.ForceLegacyUnionAabb;
                case "role":
                    return CollisionFormalCollectorMode.ForceRoleAware;
                case "brute":
                    return CollisionFormalCollectorMode.ForceBruteForce;
                default:
                    throw new ArgumentException(
                        $"Unknown formal collector mode '{value}'. " +
                        "Expected configured, legacy, role, or brute.",
                        nameof(value));
            }
        }

        internal static string FormatFormalCollectorMode(CollisionFormalCollectorMode mode)
        {
            switch (mode)
            {
                case CollisionFormalCollectorMode.Configured:
                    return "configured";
                case CollisionFormalCollectorMode.ForceLegacyUnionAabb:
                    return "legacy";
                case CollisionFormalCollectorMode.ForceRoleAware:
                    return "role";
                case CollisionFormalCollectorMode.ForceBruteForce:
                    return "brute";
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }
    }

    [Serializable]
    public sealed class ProductionEntityStressMetricSummary
    {
        public bool available;
        public string unit;
        public string source;
        public string unavailableReason;
        public int sampleCount;
        public double average;
        public double maximum;
        public double p95;
        public double p99;
    }

    [Serializable]
    public struct ProductionEntityStressAllocationRegionMetrics
    {
        public long sampleCount;
        public long sumBytes;
        public long maximumBytes;
        public long lastBytes;
    }

    internal enum ProductionEntityStressAllocationRegion
    {
        PostTickTimingCollectors = 0,
        CaptureProductionCountersTotal = 1,
        CaptureProductionCountersActiveEntityScan = 2,
        CaptureProductionCountersSceneQueryDiagnostics = 3,
        CaptureProductionCountersAiReportDiagnostics = 4,
        CaptureProductionCountersObserveRuntimeEntitySnapshot = 5,
        WriteReport = 6,
    }

    [Serializable]
    public struct ProductionEntityStressCpuRegionMetrics
    {
        public long sampleCount;
        public double sumMilliseconds;
        public double maximumMilliseconds;
        public double lastMilliseconds;
    }

    internal enum ProductionEntityStressCpuRegion
    {
        RunnerUpdateTotal = 0,
        SpawnOrRemove = 1,
        StepMeasuredTickTotal = 2,
        DriverStepOneTick = 3,
        PostTickTimingCollectors = 4,
        CaptureProductionCountersTotal = 5,
        CaptureProductionCountersActiveEntityScan = 6,
        CaptureProductionCountersSceneQueryDiagnostics = 7,
        CaptureProductionCountersAiReportDiagnostics = 8,
        CaptureProductionCountersObserveRuntimeEntitySnapshot = 9,
        WriteReport = 10,
    }

    [Serializable]
    public sealed class ProductionEntityStressPhaseTimingSummary
    {
        public string phase;
        public ProductionEntityStressMetricSummary timing;
    }

    [Serializable]
    public sealed class ProductionEntityStressLateRuntimeSnapshotTimingSummary
    {
        public string stage;
        public long callCount;
        public ProductionEntityStressMetricSummary timing;
    }

    [Serializable]
    public sealed class ProductionEntityStressAiInputDetailCounters
    {
        public bool available;
        public string unavailableReason;
        public long aiCount;
        public long spatialQueryCount;
        public long queriedHandleVisits;
        public long candidateVisits;
        public long radiusExpansions;
        public long bruteFallbackCount;
        public long bruteSlotVisits;
        public long phase1ListVisits;
        public long refreshCount;
        public long[] phaseCallCounts =
            new long[BattleAiInputDetailDiagnostics.PhaseCount];
        public long[] phaseSlotVisitCounts =
            new long[BattleAiInputDetailDiagnostics.PhaseCount];
        public long[] phaseRngCallCounts =
            new long[BattleAiInputDetailDiagnostics.PhaseCount];
        public long[] radiusHistogram =
            new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
    }

    [Serializable]
    public sealed class ProductionEntityStressTeardownReport
    {
        public bool attempted;
        public bool restored;
        public bool activeStateRestored;
        public bool driverStateRestored;
        public bool loggingStateRestored;
        public int activeGameObjectsBefore;
        public int activeGameObjectsAfter;
        public int worldObjectsBefore;
        public int worldObjectsAfter;
        public int worldEntitiesBefore;
        public int worldEntitiesAfter;
        public int claimedSlotsBefore;
        public int claimedSlotsAfter;
        public int objectPoolActiveBeforeRun;
        public int objectPoolActiveAfter;
        public int objectPoolAvailableBeforeRun;
        public int objectPoolAvailableAfter;
        public int retainedInactiveObjectPoolCapacityBeforeRun;
        public int retainedInactiveObjectPoolCapacityAfter;
        public int retainedInactiveObjectPoolCapacityDelta;
        public string retainedInactiveObjectPoolCapacityPolicy;
        public int referencePoolActiveBeforeRun;
        public int referencePoolActiveAfter;
        public int cleanupExceptionCount;
        public string cleanupExceptions;
        public string evidence;
    }

    [Serializable]
    public sealed class ProductionEntityStressLoggingPolicyReport
    {
        public string originalFilterLogType;
        public string runningFilterLogType;
        public string policy;
        public bool applied;
        public bool restored;
    }

    [Serializable]
    public sealed class ProductionEntityStressReport
    {
        public string schema = "ntsd-production-entity-stress/v2";
        public string status;
        public string mode;
        public string inputMode;
        public uint seed;
        public string aiExecutionProfileRequested;
        public string aiExecutionProfileEffective;
        public bool aiExecutionProfileLegacyCompatibility;
        public string aiSensingRequestedMode;
        public string aiSensingEffectiveMode;
        public bool aiDecisionSoAShadowRequested;
        public bool aiDecisionSharedShadowRequested;
        public string aiDecisionExecutionRequestedMode;
        public string aiDecisionExecutionEffectiveMode;
        public bool aiDecisionExecutionRestored;
        public bool aiDecisionSoAShadowApplied;
        public bool aiDecisionSharedShadowApplied;
        public bool aiDecisionSoAShadowRestored;
        public long aiDecisionSoAShadowEligibleCount;
        public long aiDecisionSoAShadowAvailableCount;
        public long aiDecisionSoAShadowUnavailableCount;
        public long aiDecisionSoAShadowComparedCount;
        public long aiDecisionSoAShadowMismatchCount;
        public string aiDecisionSoAShadowFirstReason;
        public string aiDecisionSoAShadowFirstUnavailableReason;
        public string aiDecisionShadowFirstExceptionStage;
        public string aiDecisionShadowFirstExceptionType;
        public long aiDecisionSoAShadowCloneRngCallCount;
        public long aiDecisionSoAShadowRowVisitCount;
        public long aiDecisionSharedShadowBuildCount;
        public long aiDecisionSharedShadowRefreshCount;
        public long aiDecisionIndexedEligibleCount;
        public long aiDecisionIndexedAvailableCount;
        public long aiDecisionIndexedUnavailableCount;
        public long aiDecisionIndexedComparedCount;
        public long aiDecisionIndexedMismatchCount;
        public long aiDecisionIndexedFullRowVisitCount;
        public long aiDecisionIndexedRowVisitCount;
        public string aiDecisionIndexedFirstMismatchReason;
        public long aiDecisionIndexedCanonicalEligibleCount;
        public long aiDecisionIndexedCanonicalCommittedCount;
        public long aiDecisionIndexedCanonicalFallbackCount;
        public long aiDecisionIndexedCanonicalFullOracleSampleCount;
        public long aiDecisionIndexedCanonicalFullOracleMismatchCount;
        public string aiDecisionIndexedCanonicalFirstFallbackReason;
        public string aiDecisionIndexedCanonicalFirstOracleMismatchReason;
        public bool unifiedAiSnapshotShadowRequested;
        public bool unifiedAiSnapshotShadowApplied;
        public bool unifiedAiSnapshotShadowRestored;
        public long unifiedAiSnapshotShadowBuildCount;
        public long unifiedAiSnapshotShadowSlotVisitCount;
        public long unifiedAiSnapshotShadowRefreshCount;
        public long unifiedAiSnapshotShadowFullComparisonSlotVisitCount;
        public long unifiedAiSnapshotShadowRefreshComparisonSlotVisitCount;
        public long unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount;
        public long unifiedAiSnapshotShadowMutationWitnessComparedCount;
        public long unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount;
        public long unifiedAiSnapshotShadowSensingComparedCount;
        public long unifiedAiSnapshotShadowDecisionComparedCount;
        public long unifiedAiSnapshotShadowUnavailableCount;
        public long unifiedAiSnapshotShadowMismatchCount;
        public long unifiedAiSnapshotShadowExceptionCount;
        public long unifiedAiSnapshotShadowDistinctBoundaryEncodingRowCount;
        public string unifiedAiSnapshotShadowFirstMismatch;
        public string unifiedAiSnapshotShadowFirstExceptionStage;
        public string unifiedAiSnapshotShadowFirstExceptionType;
        public string aiUnifiedSnapshotExecutionRequestedMode;
        public string aiUnifiedSnapshotExecutionEffectiveMode;
        public bool aiUnifiedSnapshotExecutionRestored;
        public long aiUnifiedSnapshotExecutionBuildCount;
        public long aiUnifiedSnapshotExecutionSlotVisitCount;
        public long aiUnifiedSnapshotExecutionRefreshCount;
        public long aiUnifiedSnapshotExecutionReadCount;
        public long aiUnifiedSnapshotExecutionCommittedPassCount;
        public long aiUnifiedSnapshotExecutionLegacyFusedSensingBuildCount;
        public long aiUnifiedSnapshotExecutionLegacyDecisionSharedBuildCount;
        public long aiUnifiedSnapshotExecutionLegacyShadowBuildCount;
        public long aiUnifiedSnapshotExecutionLegacyNearestFactsBuildCount;
        public long aiUnifiedSnapshotExecutionLegacySnapshotIndexBuildCount;
        public long aiUnifiedSnapshotExecutionLegacyQuadtreeSyncCount;
        public long aiUnifiedSnapshotExecutionLegacyDecisionSharedRefreshCount;
        public long aiUnifiedSnapshotExecutionLegacyShadowRefreshCount;
        public long aiUnifiedSnapshotExecutionLegacySnapshotMutationCount;
        public long aiUnifiedSnapshotExecutionLegacyCandidateRefreshCount;
        public long aiUnifiedSnapshotExecutionPreCommitFailureCount;
        public long aiUnifiedSnapshotExecutionPreCommitFallbackCount;
        public long aiUnifiedSnapshotExecutionPostCommitHardBreachCount;
        public string aiUnifiedSnapshotExecutionFirstFailureStage;
        public string aiUnifiedSnapshotExecutionFirstFailureType;
        public bool aiUnifiedSnapshotExecutionAuthoritySuccess;
        public bool aiUnifiedSnapshotExecutionRollbackObserved;
        public bool aiUnifiedSnapshotExecutionRollbackContractSatisfied;
        public string lateRuntimeSnapshotRequestedMode;
        public string lateRuntimeSnapshotEffectiveMode;
        public bool allowUnsafeAiSoACandidate;
        public bool simulationOnly;
        public bool skipLateRendererUpdateRequested;
        public bool skipLateRendererUpdateConfigured;
        public bool skipLateRendererUpdateApplied;
        public bool skipLateRendererUpdateRestored;
        public long skipLateRendererUpdateTickCount;
        public string soundPresentationModeRequested;
        public string soundPresentationModeResolved;
        public bool soundPresentationSuppressionRequested;
        public bool soundPresentationSuppressionConfigured;
        public bool soundPresentationSuppressionApplied;
        public bool soundPresentationSuppressionRestored;
        public long soundPresentationDispatchedEventCountDelta;
        public long soundPresentationSuppressedEventCountDelta;
        public bool autoStopWhenSampled;
        public string startedUtc;
        public string updatedUtc;
        public string unityVersion;
        public string platform;
        public string scene;
        public string stressRootName;
        public string outputPath;
        public string failure;
        public bool harnessValidity;
        public string performanceVerdict = "EvidenceOnlyNoThreshold";
        public int requestedEntityCount;
        public int selectedCharacterOid;
        public int totalEntitiesCreated;
        public int lifecycleReplacements;
        public int baseRosterActiveCount;
        public int baseAiActiveCount;
        public int derivedOrTemporaryActiveCount;
        public int totalActiveRuntimeEntityCount;
        public int totalClaimedRuntimeSlotCount;
        public string replenishmentState;
        public int maxSaturationDrainTicks;
        public int currentSaturationDrainTicks;
        public int saturationDrainTickCount;
        public int replenishAttemptCount;
        public int replenishDeferredCount;
        public int replenishedEntityCount;
        public int rosterRemovalCount;
        public int saturationBlockedBaseRosterActiveCount;
        public int saturationBlockedBaseAiActiveCount;
        public int saturationBlockedDerivedOrTemporaryActiveCount;
        public int saturationBlockedTotalActiveRuntimeEntityCount;
        public int saturationBlockedTotalClaimedRuntimeSlotCount;
        public int nonSteadyLogicTicks;
        public int sampleRejectedForIncompleteBaseRoster;
        public int sampleRejectedForRosterMutation;
        public int sampleRejectedForPoolExpansion;
        public int activeGameObjectCount;
        public int stressRootChildCount;
        public int worldObjectCountBeforeRun;
        public int worldEntityCountBeforeRun;
        public int claimedRuntimeSlotCountBeforeRun;
        public int worldObjectCount;
        public int worldEntityCount;
        public int peakWorldEntityCount;
        public int claimedRuntimeSlotCount;
        public string runtimeProfile;
        public int runtimeSlotCapacity;
        public string broadphaseBackend;
        public bool collisionCandidateStoreShadowRequested;
        public bool collisionCandidateStoreShadowApplied;
        public bool collisionCandidateStoreShadowRestored;
        public long collisionCandidateStoreShadowBuildTickCount;
        public long collisionCandidateStoreShadowComparedAttackerCount;
        public long collisionCandidateStoreShadowComparedCandidateCount;
        public long collisionCandidateStoreShadowMismatchCount;
        public long collisionCandidateStoreShadowInvalidCount;
        public string collisionCandidateStoreShadowFirstMismatchReason;
        public int collisionCandidateStoreShadowRuntimeCapacity;
        public bool collisionCandidateStoreAuthorityRequested;
        public bool collisionCandidateStoreAuthorityConfigured;
        public bool collisionCandidateStoreAuthorityApplied;
        public bool collisionCandidateStoreAuthorityRestored;
        public int collisionCandidateStoreLegacyOracleInterval;
        public long collisionCandidateStoreAuthorityRequestedTickCount;
        public long collisionCandidateStoreAuthorityAppliedTickCount;
        public long collisionCandidateStoreAuthorityLegacyFallbackTickCount;
        public long collisionCandidateStoreAuthoritySampledOracleTickCount;
        public long collisionCandidateStoreAuthorityStoreOnlyTickCount;
        public long collisionCandidateStoreAuthorityExpectedSampledOracleTickCount;
        public long collisionCandidateStoreAuthorityExpectedStoreOnlyTickCount;
        public long collisionCandidateStoreAuthorityLegacyListCreatedOrWrittenCount;
        public long collisionCandidateStoreAuthorityStoreOnlyHardFailureCount;
        public long collisionCandidateStoreAuthorityRangeReadCount;
        public long collisionCandidateStoreAuthorityEntryReadCount;
        public long collisionCandidateStoreAuthorityFailureCount;
        public string collisionCandidateStoreAuthorityFirstFailureReason;
        public bool collisionRoleZeroItrFastPathRequested;
        public bool collisionRoleZeroItrFastPathConfigured;
        public bool collisionRoleZeroItrFastPathApplied;
        public bool collisionRoleZeroItrFastPathRestored;
        public long collisionRoleZeroItrFastPathExpectedAppliedTickCount;
        public long collisionRoleZeroItrFastPathAppliedCount;
        public long collisionRoleZeroItrFastPathFallbackCount;
        public long collisionRoleZeroItrFastPathInvalidCount;
        public long collisionRoleZeroItrFastPathZeroItrCount;
        // JSON compatibility: this is the role-aware participant count observed by
        // the in-place early return. No separate handle scan is performed.
        public int collisionRoleZeroItrFastPathTouchedHandleCount;
        public string formalCollectorRequestedMode;
        public string formalCollectorMode;
        public bool forceRoleAwareDirectRequested;
        public bool forceRoleAwareTreeRequested;
        public bool forceRoleAwareNestedDirectRequested;
        public bool forceRoleAwareSweepDirectRequested;
        public bool forceRoleAwareDirectApplied;
        public bool forceRoleAwareTreeApplied;
        public bool forceRoleAwareNestedDirectApplied;
        public bool forceRoleAwareSweepDirectApplied;
        public long roleAwareDirectTickCount;
        public long roleAwareTreeTickCount;
        public long roleAwareNestedDirectTickCount;
        public long roleAwareSweepDirectTickCount;
        public int roleAwareLastDirectTickCount;
        public int roleAwareLastTreeTickCount;
        public int roleAwareLastNestedDirectTickCount;
        public int roleAwareLastSweepDirectTickCount;
        public long roleAwareSweepXCandidateCount;
        public long roleAwareLastSweepXCandidateCount;
        public long roleAwareSweepFullOverlapCheckCount;
        public long roleAwareLastSweepFullOverlapCheckCount;
        public long roleAwareDirectComparisonCount;
        public long roleAwareLastDirectComparisonCount;
        public string roleAwareDirectCostTickScope;
        public long roleAwareDirectCostObservedTickCount;
        public long roleAwareDirectCostSum;
        public long roleAwareDirectCostMax;
        public long roleAwareDirectCostAbove32768TickCount;
        public long roleAwareDirectCostAbove65536TickCount;
        public long roleAwareDirectCostAbove131072TickCount;
        public long roleAwareDirectCostAbove262144TickCount;
        public string rosterFingerprint;
        public string workloadFingerprint;
        public string implementationConfigFingerprint;
        public string finalParitySnapshotSchema;
        public int finalParitySnapshotTick;
        public string finalParityInputHash;
        public string finalParityRngHash;
        public string finalParityMetadataHash;
        public string finalParityWorldHash;
        public string finalParitySlotsHash;
        public string finalParityARestHash;
        public string finalParityVRestHash;
        public string finalParityStatsHash;
        public string finalParityEventsHash;
        public string finalParityOverallHash;
        public string finalParitySnapshotJsonPath;
        public string finalLockstepSchema;
        public int finalLockstepTick;
        public string finalLockstepInputHash;
        public string finalLockstepRngHash;
        public string finalLockstepMetadataHash;
        public string finalLockstepWorldHash;
        public string finalLockstepSlotsHash;
        public string finalLockstepARestHash;
        public string finalLockstepVRestHash;
        public string finalLockstepStatsHash;
        public string finalLockstepEventsHash;
        public string finalLockstepOverallHash;
        public int aiSoASensingShadowQueryCount;
        public int aiSoASensingShadowInvalidationCount;
        public int aiSoASensingShadowPurityMismatchCount;
        public int aiSoASensingShadowInitialMismatchCount;
        public int aiSoASensingShadowCachedMismatchCount;
        public int aiSoASensingShadowPostSpecialMismatchCount;
        public int aiSoASensingShadowMismatchMask;
        public int aiSoASensingShadowLastMismatchMask;
        public bool aiSoASensingShadowComparisonPublished;
        public string aiSoASensingShadowFirstMismatch;
        public int aiSoACandidateNearestQueryCount;
        public int aiSoACandidateSpecialQueryCount;
        public long aiSoACandidateGroundXRowVisitCount;
        public long aiSoACandidateAirXRowVisitCount;
        public long aiSoACandidateSpecialSlotVisitCount;
        public int aiSoACandidateLegacyNearestScanCount;
        public int aiSoACandidateLegacySpecialScanCount;
        public int aiSoACandidatePreRandomFailureCount;
        public int aiSoACandidatePostRandomFailureCount;
        public bool aiSoADecisionRemainderRequested;
        public bool aiSoADecisionRemainderApplied;
        public bool aiSoADecisionRemainderRestored;
        public long aiSoADecisionRemainderExpectedAppliedCount;
        public int aiSoADecisionRemainderEligibleAttemptCount;
        public int aiSoADecisionRemainderAppliedCount;
        public int aiSoADecisionRemainderFallbackCount;
        public int aiSoADecisionRemainderPreRandomFailureCount;
        public int aiSoADecisionRemainderPostRandomFailureCount;
        public int aiSoADecisionRemainderHardFailureCount;
        public int aiSoADecisionRemainderContextBindCount;
        public int aiSoADecisionRemainderGatewayValidationCount;
        public long aiSoADecisionRemainderRowVisitCount;
        public int aiLegacyNearestFactsBuildCount;
        public int aiLegacySnapshotIndexBuildCount;
        public int aiLegacyQuadtreeSyncCount;
        public int aiLegacySnapshotMutationCount;
        public int formalCollectorBodyEntries;
        public int formalCollectorItrQueries;
        public int logicTicksExecuted;
        public int warmupTicksCompleted;
        public int sampledLogicTicks;
        public int sampledUnityFrames;
        public int framesWithCatchUp;
        public int maximumCatchUpTicksInFrame;
        public int currentBacklogTicks;
        public int maximumBacklogTicks;
        public int droppedBacklogTicks;
        public long aiControlledEntityTicks;
        public long collisionCandidateConsumerEntityTicks;
        public long collisionCandidateCountSum;
        public int collisionCandidateCountPeak;
        public long broadphasePairCountSum;
        public int broadphasePairCountPeak;
        public int broadphaseFallbackParticipantPeak;
        public int broadphaseAbortedTicks;
        public int broadphaseLastIndexedCount;
        public int damageStatTotal;
        public int killStatTotal;
        public bool opointCounterAvailable;
        public int observedOpointCreates;
        public string opointCounterReason;
        public ProductionEntityStressMetricSummary logicTickMilliseconds;
        public ProductionEntityStressMetricSummary logicTickWithPresentationMilliseconds;
        public ProductionEntityStressMetricSummary logicTickWithoutPresentationMilliseconds;
        public ProductionEntityStressMetricSummary unityFrameMilliseconds;
        public ProductionEntityStressMetricSummary logicTickAllocatedBytes;
        public ProductionEntityStressMetricSummary profilerFrameGcAllocatedBytes;
        public ProductionEntityStressMetricSummary profilerFrameMainThreadGcAllocEventCount;
        public string profilerFrameGcAllocatedBytesRecorderCategory;
        public string profilerFrameGcAllocatedBytesRecorderUnitType;
        public string profilerFrameMainThreadGcAllocRecorderCategory;
        public string profilerFrameMainThreadGcAllocRecorderUnitType;
        public string profilerFrameGcEvidenceAlignment;
        public int profilerFrameGcCandidateWindowCount;
        public int profilerFrameGcAcceptedFrameCount;
        public int profilerFrameGcDiscardedWindowCount;
        public int profilerFrameGcTrailingIncompleteFrameCount;
        public string profilerFrameGcLastDiscardReason;
        public int profilerFrameGcAllocatedBytesAcceptedFrameCount;
        public int profilerFrameGcAllocatedBytesDiscardedWindowCount;
        public int profilerFrameGcAllocatedBytesTrailingIncompleteFrameCount;
        public string profilerFrameGcAllocatedBytesLastDiscardReason;
        public int profilerFrameMainThreadGcAllocAcceptedFrameCount;
        public int profilerFrameMainThreadGcAllocDiscardedWindowCount;
        public int profilerFrameMainThreadGcAllocTrailingIncompleteFrameCount;
        public string profilerFrameMainThreadGcAllocLastDiscardReason;
        public ProductionEntityStressAllocationRegionMetrics
            allocationPostTickTimingCollectors;
        public ProductionEntityStressAllocationRegionMetrics
            allocationCaptureProductionCountersTotal;
        public ProductionEntityStressAllocationRegionMetrics
            allocationCaptureProductionCountersActiveEntityScan;
        public ProductionEntityStressAllocationRegionMetrics
            allocationCaptureProductionCountersSceneQueryDiagnostics;
        public ProductionEntityStressAllocationRegionMetrics
            allocationCaptureProductionCountersAiReportDiagnostics;
        public ProductionEntityStressAllocationRegionMetrics
            allocationCaptureProductionCountersObserveRuntimeEntitySnapshot;
        public ProductionEntityStressAllocationRegionMetrics allocationWriteReport;
        public ProductionEntityStressCpuRegionMetrics cpuRunnerUpdateTotal;
        public ProductionEntityStressCpuRegionMetrics cpuSpawnOrRemove;
        public ProductionEntityStressCpuRegionMetrics cpuStepMeasuredTickTotal;
        public ProductionEntityStressCpuRegionMetrics cpuDriverStepOneTick;
        public ProductionEntityStressCpuRegionMetrics cpuPostTickTimingCollectors;
        public ProductionEntityStressCpuRegionMetrics
            cpuCaptureProductionCountersTotal;
        public ProductionEntityStressCpuRegionMetrics
            cpuCaptureProductionCountersActiveEntityScan;
        public ProductionEntityStressCpuRegionMetrics
            cpuCaptureProductionCountersSceneQueryDiagnostics;
        public ProductionEntityStressCpuRegionMetrics
            cpuCaptureProductionCountersAiReportDiagnostics;
        public ProductionEntityStressCpuRegionMetrics
            cpuCaptureProductionCountersObserveRuntimeEntitySnapshot;
        public ProductionEntityStressCpuRegionMetrics cpuWriteReport;
        public bool phaseTimingEnabled;
        public string phaseTimingSource;
        public List<ProductionEntityStressPhaseTimingSummary> phaseTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public ProductionEntityStressMetricSummary phaseTimingUnattributedMilliseconds;
        public bool presentationTimingEnabled;
        public string presentationTimingSource;
        public string presentationTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> presentationTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public bool detailPhaseTimingEnabled;
        public string detailPhaseTimingSource;
        public string detailPhaseTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> detailPhaseTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public string aiInputDetailTimingSource;
        public string aiInputDetailTimingUnavailableReason;
        public List<ProductionEntityStressPhaseTimingSummary> aiInputDetailTimings =
            new List<ProductionEntityStressPhaseTimingSummary>();
        public string lateRuntimeSnapshotTimingSource;
        public string lateRuntimeSnapshotTimingUnavailableReason;
        public List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>
            lateRuntimeSnapshotTimings =
                new List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>();
        public ProductionEntityStressAiInputDetailCounters aiInputDetailCounters =
            new ProductionEntityStressAiInputDetailCounters();
        public ProductionEntityStressLoggingPolicyReport loggingPolicy =
            new ProductionEntityStressLoggingPolicyReport();
        public ProductionEntityStressTeardownReport teardown =
            new ProductionEntityStressTeardownReport();
    }

    internal sealed class ProductionEntityStressLoggingPolicy
    {
        private readonly Func<LogType> getFilterLogType;
        private readonly Action<LogType> setFilterLogType;
        private bool applied;
        private LogType originalFilterLogType;

        internal ProductionEntityStressLoggingPolicy()
            : this(
                () => Debug.unityLogger.filterLogType,
                value => Debug.unityLogger.filterLogType = value)
        {
        }

        internal ProductionEntityStressLoggingPolicy(
            Func<LogType> getFilterLogType,
            Action<LogType> setFilterLogType)
        {
            this.getFilterLogType = getFilterLogType ?? throw new ArgumentNullException(nameof(getFilterLogType));
            this.setFilterLogType = setFilterLogType ?? throw new ArgumentNullException(nameof(setFilterLogType));
        }

        internal void Apply(ProductionEntityStressLoggingPolicyReport report)
        {
            if (applied)
                return;

            originalFilterLogType = getFilterLogType();
            setFilterLogType(LogType.Error);
            applied = true;
            UpdateReport(report);
        }

        internal void Restore(ProductionEntityStressLoggingPolicyReport report)
        {
            if (applied)
            {
                setFilterLogType(originalFilterLogType);
                applied = false;
            }
            UpdateReport(report);
            if (report != null)
                report.restored = getFilterLogType() == originalFilterLogType;
        }

        private void UpdateReport(ProductionEntityStressLoggingPolicyReport report)
        {
            if (report == null)
                return;

            report.originalFilterLogType = originalFilterLogType.ToString();
            report.runningFilterLogType = LogType.Error.ToString();
            report.policy = "Suppress Log and Warning during the stress run while retaining Error.";
            report.applied = applied;
        }
    }

    internal static class ProductionEntityStressStatistics
    {
        internal static ProductionEntityStressMetricSummary Summarize(
            IReadOnlyList<double> values,
            string unit,
            string source)
        {
            var result = new ProductionEntityStressMetricSummary
            {
                available = values != null && values.Count > 0,
                unit = unit ?? string.Empty,
                source = source ?? string.Empty,
                unavailableReason = values == null || values.Count == 0
                    ? "No completed samples."
                    : string.Empty,
                sampleCount = values?.Count ?? 0,
            };
            if (!result.available)
                return result;

            var sorted = new double[values.Count];
            double sum = 0d;
            double maximum = double.MinValue;
            for (int i = 0; i < values.Count; i++)
            {
                double value = values[i];
                sorted[i] = value;
                sum += value;
                maximum = Math.Max(maximum, value);
            }
            Array.Sort(sorted);
            result.average = sum / values.Count;
            result.maximum = maximum;
            result.p95 = Percentile(sorted, 0.95d);
            result.p99 = Percentile(sorted, 0.99d);
            return result;
        }

        internal static double Percentile(IReadOnlyList<double> sortedValues, double percentile)
        {
            if (sortedValues == null || sortedValues.Count == 0)
                return 0d;
            double rank = Math.Max(0d, Math.Min(1d, percentile)) * (sortedValues.Count - 1);
            int lower = (int)Math.Floor(rank);
            int upper = Math.Min(sortedValues.Count - 1, lower + 1);
            double blend = rank - lower;
            return sortedValues[lower] + (sortedValues[upper] - sortedValues[lower]) * blend;
        }
    }

    internal static class ProductionEntityStressProfilerFrameSamplePolicy
    {
        internal const string TrailingIncompleteReason =
            "The active recorder window ended before its Profiler frame completed; " +
            "the single trailing current frame was discarded.";
        internal const string NonFormalWindowReason =
            "The recorder window contained a non-formal logic tick and was discarded.";
        internal const string MissingCompletedSampleReason =
            "The exact recorders produced no aligned completed Profiler-frame sample.";
        internal const string WrappedRecorderReason =
            "A ProfilerRecorder wrapped before its samples were copied; the window was discarded.";
        internal const string MisalignedRecorderReason =
            "The exact recorders returned different completed-frame sample counts; " +
            "the unmatched trailing sample was discarded.";

        internal static bool ShouldStart(
            int completedLogicTicks,
            int warmupTicks,
            int baseRosterActiveCount,
            int requestedEntityCount,
            bool rosterMutationPending,
            bool saturationDrainActive,
            float accumulator,
            int sampledLogicTicks,
            int targetSampleTicks,
            bool shouldAutoStopWhenSampled)
        {
            return completedLogicTicks >= warmupTicks &&
                   baseRosterActiveCount == requestedEntityCount &&
                   !rosterMutationPending &&
                   !saturationDrainActive &&
                   accumulator >= SimulationConstants.SIM_DT &&
                   (!shouldAutoStopWhenSampled || sampledLogicTicks < targetSampleTicks);
        }

        internal static bool IsFormalWindow(
            int logicTickCountAtStart,
            int sampledLogicTickCountAtStart,
            int nonSteadyLogicTickCountAtStart,
            int logicTickCountAtStop,
            int sampledLogicTickCountAtStop,
            int nonSteadyLogicTickCountAtStop)
        {
            int executed = logicTickCountAtStop - logicTickCountAtStart;
            int sampled = sampledLogicTickCountAtStop - sampledLogicTickCountAtStart;
            return executed > 0 &&
                   sampled == executed &&
                   nonSteadyLogicTickCountAtStop == nonSteadyLogicTickCountAtStart;
        }

        internal static int ResolveAlignedCompletedSampleCount(
            int gcAllocatedSampleCount,
            int gcAllocEventSampleCount,
            bool gcAllocatedWrapped,
            bool gcAllocEventWrapped,
            out int trailingIncompleteSampleCount,
            out string reason)
        {
            trailingIncompleteSampleCount = 0;
            if (gcAllocatedWrapped || gcAllocEventWrapped)
            {
                reason = WrappedRecorderReason;
                return 0;
            }

            int alignedCount = Math.Min(
                Math.Max(0, gcAllocatedSampleCount),
                Math.Max(0, gcAllocEventSampleCount));
            int unmatched = Math.Abs(gcAllocatedSampleCount - gcAllocEventSampleCount);
            if (alignedCount <= 0)
            {
                trailingIncompleteSampleCount = Math.Min(1, unmatched);
                reason = MissingCompletedSampleReason;
                return 0;
            }
            if (unmatched > 1)
            {
                reason = MisalignedRecorderReason;
                return 0;
            }

            trailingIncompleteSampleCount = unmatched;
            reason = unmatched == 0 ? string.Empty : MisalignedRecorderReason;
            return alignedCount;
        }

        internal static int ResolveCompletedSampleCount(
            int sampleCount,
            bool wrapped,
            out int trailingIncompleteSampleCount,
            out string reason)
        {
            trailingIncompleteSampleCount = 0;
            if (wrapped)
            {
                reason = WrappedRecorderReason;
                return 0;
            }
            if (sampleCount <= 0)
            {
                reason = MissingCompletedSampleReason;
                return 0;
            }

            reason = string.Empty;
            return sampleCount;
        }

        internal static double NormalizeGcAllocatedBytes(long sampleValue)
        {
            return Math.Max(0L, sampleValue);
        }

        internal static double NormalizeGcAllocEventCount(long sampleCount)
        {
            return Math.Max(0L, sampleCount);
        }
    }

    internal sealed class ProductionEntityStressProfilerFrameGcCollector : IDisposable
    {
        internal const string GcAllocatedMarker = "GC Allocated In Frame";
        internal const string GcAllocMarker = "GC.Alloc";
        internal const string GcAllocatedSource =
            "ProfilerRecorder Memory/GC Allocated In Frame; one value per completed Profiler frame.";
        internal const string GcAllocEventSource =
            "ProfilerRecorder GC.Alloc with Default | CollectOnlyOnCurrentThread; " +
            "the recorder is constructed and started by the runner on Unity's main thread; " +
            "the exact handle has UnitType Bytes, while the reported metric is " +
            "ProfilerRecorderSample.Count events per completed Profiler frame.";
        internal const string Alignment =
            "Recorder windows start immediately before a candidate formal logic-tick batch and stop " +
            "at the beginning of the next MonoBehaviour.Update. Only windows whose logic ticks all " +
            "passed the production steady-state sample gate are retained. Each exact recorder is " +
            "accepted independently, so an unavailable GC.Alloc event marker cannot invalidate the " +
            "whole-frame GC byte metric. Report generation and teardown run only after Stop.";

        private const int RecorderCapacity = 8;
        private readonly List<ProfilerRecorderSample> gcAllocatedBuffer =
            new List<ProfilerRecorderSample>(RecorderCapacity);
        private readonly List<ProfilerRecorderSample> gcAllocEventBuffer =
            new List<ProfilerRecorderSample>(RecorderCapacity);
        private readonly List<double> gcAllocatedBytesSamples =
            new List<double>(ProductionEntityStressRunner.MaximumRetainedSamples);
        private readonly List<double> gcAllocEventCountSamples =
            new List<double>(ProductionEntityStressRunner.MaximumRetainedSamples);
        private ProfilerRecorder gcAllocatedRecorder;
        private ProfilerRecorder gcAllocEventRecorder;
        private bool gcAllocatedConstructed;
        private bool gcAllocEventConstructed;
        private bool gcAllocatedValid;
        private bool gcAllocEventValid;
        private bool active;
        private bool disposed;
        private int logicTickCountAtStart;
        private int sampledLogicTickCountAtStart;
        private int nonSteadyLogicTickCountAtStart;
        private string gcAllocatedUnavailableReason = string.Empty;
        private string gcAllocEventUnavailableReason = string.Empty;
        private string gcAllocatedCategory = string.Empty;
        private string gcAllocEventCategory = string.Empty;
        private string gcAllocatedUnitType = string.Empty;
        private string gcAllocEventUnitType = string.Empty;
        private int gcAllocatedAcceptedFrameCount;
        private int gcAllocatedDiscardedWindowCount;
        private int gcAllocatedTrailingIncompleteFrameCount;
        private string gcAllocatedLastDiscardReason = string.Empty;
        private int gcAllocEventAcceptedFrameCount;
        private int gcAllocEventDiscardedWindowCount;
        private int gcAllocEventTrailingIncompleteFrameCount;
        private string gcAllocEventLastDiscardReason = string.Empty;

        internal ProductionEntityStressProfilerFrameGcCollector()
        {
            var handles = new List<ProfilerRecorderHandle>(256);
            ProfilerRecorderHandle.GetAvailable(handles);
            bool gcAllocatedHandleValid = TryFindExactHandle(
                handles,
                ProfilerCategory.Memory,
                GcAllocatedMarker,
                ProfilerMarkerDataUnit.Bytes,
                out ProfilerRecorderHandle gcAllocatedHandle,
                out gcAllocatedCategory,
                out gcAllocatedUnitType,
                out gcAllocatedUnavailableReason);
            bool gcAllocEventHandleValid = TryFindPreferredExactHandle(
                handles,
                ProfilerCategory.Internal,
                GcAllocMarker,
                ProfilerMarkerDataUnit.Bytes,
                out ProfilerRecorderHandle gcAllocEventHandle,
                out gcAllocEventCategory,
                out gcAllocEventUnitType,
                out gcAllocEventUnavailableReason);

            if (gcAllocatedHandleValid)
            {
                try
                {
                    gcAllocatedRecorder = new ProfilerRecorder(
                        gcAllocatedHandle,
                        RecorderCapacity,
                        ProfilerRecorderOptions.Default);
                    gcAllocatedConstructed = true;
                    gcAllocatedValid = gcAllocatedRecorder.Valid;
                    if (!gcAllocatedValid)
                    {
                        gcAllocatedUnavailableReason =
                            "The exact Memory/GC Allocated In Frame recorder was invalid.";
                    }
                }
                catch (Exception exception)
                {
                    gcAllocatedUnavailableReason =
                        "GC Allocated In Frame recorder construction failed: " +
                        exception.GetType().Name;
                }
            }

            if (gcAllocEventHandleValid)
            {
                try
                {
                    gcAllocEventRecorder = new ProfilerRecorder(
                        gcAllocEventHandle,
                        RecorderCapacity,
                        ProfilerRecorderOptions.Default |
                        ProfilerRecorderOptions.CollectOnlyOnCurrentThread);
                    gcAllocEventConstructed = true;
                    gcAllocEventValid = gcAllocEventRecorder.Valid;
                    if (!gcAllocEventValid)
                    {
                        gcAllocEventUnavailableReason =
                            "The exact main-thread Internal/GC.Alloc recorder was invalid.";
                    }
                }
                catch (Exception exception)
                {
                    gcAllocEventUnavailableReason =
                        "Main-thread GC.Alloc recorder construction failed: " +
                        exception.GetType().Name;
                }
            }
        }

        internal bool Active => active;
        internal int CandidateWindowCount { get; private set; }
        internal int AcceptedFrameCount { get; private set; }
        internal int DiscardedWindowCount { get; private set; }
        internal int TrailingIncompleteFrameCount { get; private set; }
        internal string LastDiscardReason { get; private set; } = string.Empty;

        internal bool StartCandidate(
            int logicTickCount,
            int sampledLogicTickCount,
            int nonSteadyLogicTickCount)
        {
            if (!CanStart(disposed, active, gcAllocatedValid, gcAllocEventValid))
                return false;

            if (gcAllocatedValid)
                gcAllocatedRecorder.Reset();
            if (gcAllocEventValid)
                gcAllocEventRecorder.Reset();
            logicTickCountAtStart = logicTickCount;
            sampledLogicTickCountAtStart = sampledLogicTickCount;
            nonSteadyLogicTickCountAtStart = nonSteadyLogicTickCount;
            CandidateWindowCount++;
            if (gcAllocatedValid)
                gcAllocatedRecorder.Start();
            if (gcAllocEventValid)
                gcAllocEventRecorder.Start();
            active = true;
            return true;
        }

        internal static bool CanStart(
            bool disposed,
            bool active,
            bool gcAllocatedValid,
            bool gcAllocEventValid)
        {
            return !disposed && !active && (gcAllocatedValid || gcAllocEventValid);
        }

        internal void StopAndCollect(
            int logicTickCount,
            int sampledLogicTickCount,
            int nonSteadyLogicTickCount,
            bool frameBoundaryCompleted)
        {
            if (!active)
                return;

            if (gcAllocatedValid)
                gcAllocatedRecorder.Stop();
            if (gcAllocEventValid)
                gcAllocEventRecorder.Stop();
            active = false;
            gcAllocatedBuffer.Clear();
            gcAllocEventBuffer.Clear();
            if (gcAllocatedValid)
                gcAllocatedRecorder.CopyTo(gcAllocatedBuffer, false);
            if (gcAllocEventValid)
                gcAllocEventRecorder.CopyTo(gcAllocEventBuffer, false);

            if (!frameBoundaryCompleted)
            {
                DiscardedWindowCount++;
                TrailingIncompleteFrameCount++;
                LastDiscardReason =
                    ProductionEntityStressProfilerFrameSamplePolicy
                        .TrailingIncompleteReason;
                DiscardAvailableMetricWindow(
                    ProductionEntityStressProfilerFrameSamplePolicy.TrailingIncompleteReason,
                    trailingIncomplete: true);
                return;
            }
            if (!ProductionEntityStressProfilerFrameSamplePolicy.IsFormalWindow(
                    logicTickCountAtStart,
                    sampledLogicTickCountAtStart,
                    nonSteadyLogicTickCountAtStart,
                    logicTickCount,
                    sampledLogicTickCount,
                    nonSteadyLogicTickCount))
            {
                DiscardedWindowCount++;
                LastDiscardReason =
                    ProductionEntityStressProfilerFrameSamplePolicy.NonFormalWindowReason;
                DiscardAvailableMetricWindow(
                    ProductionEntityStressProfilerFrameSamplePolicy.NonFormalWindowReason,
                    trailingIncomplete: false);
                return;
            }

            int acceptedThisWindow = 0;
            if (gcAllocatedValid)
            {
                int count = ProductionEntityStressProfilerFrameSamplePolicy
                    .ResolveCompletedSampleCount(
                        gcAllocatedBuffer.Count,
                        gcAllocatedRecorder.WrappedAround,
                        out int trailing,
                        out string reason);
                gcAllocatedTrailingIncompleteFrameCount += trailing;
                if (count <= 0)
                {
                    gcAllocatedDiscardedWindowCount++;
                    gcAllocatedLastDiscardReason = reason;
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        AddRollingSample(
                            gcAllocatedBytesSamples,
                            ProductionEntityStressProfilerFrameSamplePolicy
                                .NormalizeGcAllocatedBytes(gcAllocatedBuffer[i].Value));
                    }
                    gcAllocatedAcceptedFrameCount += count;
                    acceptedThisWindow = Math.Max(acceptedThisWindow, count);
                }
            }
            if (gcAllocEventValid)
            {
                int count = ProductionEntityStressProfilerFrameSamplePolicy
                    .ResolveCompletedSampleCount(
                        gcAllocEventBuffer.Count,
                        gcAllocEventRecorder.WrappedAround,
                        out int trailing,
                        out string reason);
                gcAllocEventTrailingIncompleteFrameCount += trailing;
                if (count <= 0)
                {
                    gcAllocEventDiscardedWindowCount++;
                    gcAllocEventLastDiscardReason = reason;
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        AddRollingSample(
                            gcAllocEventCountSamples,
                            ProductionEntityStressProfilerFrameSamplePolicy
                                .NormalizeGcAllocEventCount(gcAllocEventBuffer[i].Count));
                    }
                    gcAllocEventAcceptedFrameCount += count;
                    acceptedThisWindow = Math.Max(acceptedThisWindow, count);
                }
            }

            AcceptedFrameCount += acceptedThisWindow;
            TrailingIncompleteFrameCount = Math.Max(
                gcAllocatedTrailingIncompleteFrameCount,
                gcAllocEventTrailingIncompleteFrameCount);
            if (acceptedThisWindow == 0)
            {
                DiscardedWindowCount++;
                LastDiscardReason = !string.IsNullOrEmpty(gcAllocatedLastDiscardReason)
                    ? gcAllocatedLastDiscardReason
                    : gcAllocEventLastDiscardReason;
            }
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.profilerFrameGcAllocatedBytesRecorderCategory = gcAllocatedCategory;
            report.profilerFrameGcAllocatedBytesRecorderUnitType = gcAllocatedUnitType;
            report.profilerFrameMainThreadGcAllocRecorderCategory = gcAllocEventCategory;
            report.profilerFrameMainThreadGcAllocRecorderUnitType = gcAllocEventUnitType;
            report.profilerFrameGcEvidenceAlignment = Alignment;
            report.profilerFrameGcCandidateWindowCount = CandidateWindowCount;
            report.profilerFrameGcAcceptedFrameCount = AcceptedFrameCount;
            report.profilerFrameGcDiscardedWindowCount = DiscardedWindowCount;
            report.profilerFrameGcTrailingIncompleteFrameCount =
                TrailingIncompleteFrameCount;
            report.profilerFrameGcLastDiscardReason = LastDiscardReason;
            report.profilerFrameGcAllocatedBytesAcceptedFrameCount =
                gcAllocatedAcceptedFrameCount;
            report.profilerFrameGcAllocatedBytesDiscardedWindowCount =
                gcAllocatedDiscardedWindowCount;
            report.profilerFrameGcAllocatedBytesTrailingIncompleteFrameCount =
                gcAllocatedTrailingIncompleteFrameCount;
            report.profilerFrameGcAllocatedBytesLastDiscardReason =
                gcAllocatedLastDiscardReason;
            report.profilerFrameMainThreadGcAllocAcceptedFrameCount =
                gcAllocEventAcceptedFrameCount;
            report.profilerFrameMainThreadGcAllocDiscardedWindowCount =
                gcAllocEventDiscardedWindowCount;
            report.profilerFrameMainThreadGcAllocTrailingIncompleteFrameCount =
                gcAllocEventTrailingIncompleteFrameCount;
            report.profilerFrameMainThreadGcAllocLastDiscardReason =
                gcAllocEventLastDiscardReason;
            report.profilerFrameGcAllocatedBytes = BuildMetric(
                gcAllocatedBytesSamples,
                "bytes",
                GcAllocatedSource + " Resolved category: " + gcAllocatedCategory + ".",
                gcAllocatedValid ? string.Empty : gcAllocatedUnavailableReason);
            report.profilerFrameMainThreadGcAllocEventCount = BuildMetric(
                gcAllocEventCountSamples,
                "count",
                GcAllocEventSource + " Resolved category: " + gcAllocEventCategory + ".",
                gcAllocEventValid ? string.Empty : gcAllocEventUnavailableReason);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            if (active)
            {
                if (gcAllocatedValid)
                    gcAllocatedRecorder.Stop();
                if (gcAllocEventValid)
                    gcAllocEventRecorder.Stop();
                active = false;
            }
            if (gcAllocatedConstructed)
                gcAllocatedRecorder.Dispose();
            if (gcAllocEventConstructed)
                gcAllocEventRecorder.Dispose();
            disposed = true;
        }

        private void DiscardAvailableMetricWindow(string reason, bool trailingIncomplete)
        {
            if (gcAllocatedValid)
            {
                gcAllocatedDiscardedWindowCount++;
                gcAllocatedLastDiscardReason = reason;
                if (trailingIncomplete)
                    gcAllocatedTrailingIncompleteFrameCount++;
            }
            if (gcAllocEventValid)
            {
                gcAllocEventDiscardedWindowCount++;
                gcAllocEventLastDiscardReason = reason;
                if (trailingIncomplete)
                    gcAllocEventTrailingIncompleteFrameCount++;
            }
        }

        private static bool TryFindExactHandle(
            IReadOnlyList<ProfilerRecorderHandle> handles,
            ProfilerCategory category,
            string markerName,
            ProfilerMarkerDataUnit expectedUnitType,
            out ProfilerRecorderHandle handle,
            out string resolvedCategory,
            out string unitType,
            out string reason)
        {
            handle = default;
            resolvedCategory = string.Empty;
            unitType = string.Empty;
            reason = string.Empty;
            for (int i = 0; i < handles.Count; i++)
            {
                ProfilerRecorderDescription description =
                    ProfilerRecorderHandle.GetDescription(handles[i]);
                if (description.Category != category ||
                    !string.Equals(description.Name, markerName, StringComparison.Ordinal))
                {
                    continue;
                }

                unitType = description.UnitType.ToString();
                resolvedCategory = description.Category.ToString();
                if (description.UnitType != expectedUnitType)
                {
                    reason = "The exact profiler metric had UnitType " + unitType +
                             "; expected " + expectedUnitType + ".";
                    return false;
                }
                handle = handles[i];
                return true;
            }

            reason = "The exact profiler metric " + category + "/" + markerName +
                     " was not discovered on this platform.";
            return false;
        }

        private static bool TryFindPreferredExactHandle(
            IReadOnlyList<ProfilerRecorderHandle> handles,
            ProfilerCategory preferredCategory,
            string markerName,
            ProfilerMarkerDataUnit expectedUnitType,
            out ProfilerRecorderHandle handle,
            out string resolvedCategory,
            out string unitType,
            out string reason)
        {
            handle = default;
            resolvedCategory = string.Empty;
            unitType = string.Empty;
            reason = string.Empty;
            int preferredMatchCount = 0;
            int fallbackMatchCount = 0;
            ProfilerRecorderHandle preferredHandle = default;
            ProfilerRecorderHandle fallbackHandle = default;
            string preferredCategoryName = string.Empty;
            string fallbackCategoryName = string.Empty;

            for (int i = 0; i < handles.Count; i++)
            {
                ProfilerRecorderDescription description =
                    ProfilerRecorderHandle.GetDescription(handles[i]);
                if (!string.Equals(description.Name, markerName, StringComparison.Ordinal) ||
                    description.UnitType != expectedUnitType)
                {
                    continue;
                }

                if (description.Category == preferredCategory)
                {
                    preferredMatchCount++;
                    preferredHandle = handles[i];
                    preferredCategoryName = description.Category.ToString();
                }
                else
                {
                    fallbackMatchCount++;
                    fallbackHandle = handles[i];
                    fallbackCategoryName = description.Category.ToString();
                }
            }

            unitType = expectedUnitType.ToString();
            if (preferredMatchCount == 1)
            {
                handle = preferredHandle;
                resolvedCategory = preferredCategoryName;
                return true;
            }
            if (preferredMatchCount > 1)
            {
                reason = "The preferred profiler metric " + preferredCategory + "/" +
                         markerName + " was ambiguous (" + preferredMatchCount + " matches).";
                return false;
            }
            if (fallbackMatchCount == 1)
            {
                handle = fallbackHandle;
                resolvedCategory = fallbackCategoryName;
                return true;
            }
            if (fallbackMatchCount > 1)
            {
                reason = "The fallback profiler metric " + markerName +
                         " with UnitType " + expectedUnitType + " was ambiguous across " +
                         fallbackMatchCount + " handles.";
                return false;
            }

            reason = "The exact profiler metric " + markerName + " with UnitType " +
                     expectedUnitType + " was not discovered in preferred category " +
                     preferredCategory + " or any fallback category on this platform.";
            return false;
        }

        private static ProductionEntityStressMetricSummary BuildMetric(
            IReadOnlyList<double> samples,
            string unit,
            string source,
            string unavailableReason)
        {
            ProductionEntityStressMetricSummary metric =
                ProductionEntityStressStatistics.Summarize(samples, unit, source);
            if (!metric.available && !string.IsNullOrEmpty(unavailableReason))
                metric.unavailableReason = unavailableReason;
            return metric;
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal sealed class ProductionEntityStressPhaseTimingCollector
    {
        internal const string Source =
            "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; " +
            "diagnostic evidence only, with no performance threshold.";

        private readonly List<double>[] phaseSamples;
        private readonly List<double> unattributedSamples = new List<double>(512);

        internal ProductionEntityStressPhaseTimingCollector()
        {
            phaseSamples = new List<double>[BattleTickPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }

        internal void CaptureAfterTick(
            BattleTickPhaseDiagnostics diagnostics,
            double outerMilliseconds,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled ||
                completedTickCount <= warmupTickCount)
            {
                return;
            }

            double timestampToMilliseconds =
                1000d / BattleTickPhaseDiagnostics.TimestampFrequency;
            double phaseSumMilliseconds = 0d;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double phaseMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattleTickPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], phaseMilliseconds);
                phaseSumMilliseconds += phaseMilliseconds;
            }

            AddRollingSample(
                unattributedSamples,
                Math.Max(0d, outerMilliseconds - phaseSumMilliseconds));
            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.phaseTimings.Clear();
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattleTickPhase phase = (BattleTickPhase)i;
                report.phaseTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleTickPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }

            report.phaseTimingUnattributedMilliseconds =
                ProductionEntityStressStatistics.Summarize(
                    unattributedSamples,
                    "ms",
                    "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings");
        }

        internal static void PopulateDisabledReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.phaseTimingEnabled = false;
            report.phaseTimingSource = string.Empty;
            if (report.phaseTimings == null)
                report.phaseTimings = new List<ProductionEntityStressPhaseTimingSummary>();
            else
                report.phaseTimings.Clear();
            report.phaseTimingUnattributedMilliseconds = new ProductionEntityStressMetricSummary
            {
                available = false,
                unit = "ms",
                unavailableReason =
                    "Disabled by request; set enablePhaseTiming to true to collect battle tick phase timings.",
            };
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal readonly struct CollisionCandidateStoreAuthorityDiagnosticsSnapshot
    {
        internal CollisionCandidateStoreAuthorityDiagnosticsSnapshot(
            long requestedTickCount,
            long appliedTickCount,
            long legacyFallbackTickCount,
            long sampledOracleTickCount,
            long storeOnlyTickCount,
            long legacyListCreatedOrWrittenCount,
            long storeOnlyHardFailureCount,
            long rangeReadCount,
            long entryReadCount,
            long failureCount)
        {
            RequestedTickCount = requestedTickCount;
            AppliedTickCount = appliedTickCount;
            LegacyFallbackTickCount = legacyFallbackTickCount;
            SampledOracleTickCount = sampledOracleTickCount;
            StoreOnlyTickCount = storeOnlyTickCount;
            LegacyListCreatedOrWrittenCount = legacyListCreatedOrWrittenCount;
            StoreOnlyHardFailureCount = storeOnlyHardFailureCount;
            RangeReadCount = rangeReadCount;
            EntryReadCount = entryReadCount;
            FailureCount = failureCount;
        }

        internal long RequestedTickCount { get; }
        internal long AppliedTickCount { get; }
        internal long LegacyFallbackTickCount { get; }
        internal long SampledOracleTickCount { get; }
        internal long StoreOnlyTickCount { get; }
        internal long LegacyListCreatedOrWrittenCount { get; }
        internal long StoreOnlyHardFailureCount { get; }
        internal long RangeReadCount { get; }
        internal long EntryReadCount { get; }
        internal long FailureCount { get; }

        internal static CollisionCandidateStoreAuthorityDiagnosticsSnapshot Capture(
            BruteForceSceneQuery sceneQuery)
        {
            CollisionCandidateStoreAuthorityDiagnostics diagnostics =
                sceneQuery?.CollisionCandidateStoreAuthorityDiagnostics;
            return diagnostics == null
                ? default
                : new CollisionCandidateStoreAuthorityDiagnosticsSnapshot(
                    diagnostics.RequestedTickCount,
                    diagnostics.AppliedTickCount,
                    diagnostics.LegacyFallbackTickCount,
                    diagnostics.SampledOracleTickCount,
                    diagnostics.StoreOnlyTickCount,
                    diagnostics.LegacyListCreatedOrWrittenCount,
                    diagnostics.StoreOnlyHardFailureCount,
                    diagnostics.RangeReadCount,
                    diagnostics.EntryReadCount,
                    diagnostics.FailureCount);
        }
    }

    internal enum CollisionCandidateStoreValidationPhase
    {
        PreTick = 0,
        Final = 1,
        Teardown = 2,
    }

    internal sealed class ProductionEntityStressPresentationTimingCollector
    {
        internal const string Source =
            "Opt-in coarse Stopwatch timestamps around presentation publication boundaries; " +
            "independent from battle tick and per-entity detail diagnostics.";

        private readonly List<double>[] phaseSamples;
        private long lastCapturedSequence;

        internal ProductionEntityStressPresentationTimingCollector()
        {
            phaseSamples = new List<double>[BattlePresentationPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }

        internal void CaptureAfterTick(
            BattlePresentationPhaseDiagnostics diagnostics,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled)
                return;

            long completedSequence = diagnostics.CompletedSampleSequence;
            if (completedSequence <= 0 || completedSequence == lastCapturedSequence)
                return;

            lastCapturedSequence = completedSequence;
            if (completedTickCount <= warmupTickCount)
                return;

            double timestampToMilliseconds =
                1000d / BattlePresentationPhaseDiagnostics.TimestampFrequency;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double elapsedMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattlePresentationPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], elapsedMilliseconds);
            }

            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            if (report.presentationTimings == null)
                report.presentationTimings = new List<ProductionEntityStressPhaseTimingSummary>();
            else
                report.presentationTimings.Clear();

            report.presentationTimingEnabled = true;
            report.presentationTimingSource = Source;
            report.presentationTimingUnavailableReason = SampleCount > 0
                ? string.Empty
                : "No completed presentation timing samples.";
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattlePresentationPhase phase = (BattlePresentationPhase)i;
                report.presentationTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattlePresentationPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }
        }

        internal static void PopulateDisabledReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.presentationTimingEnabled = false;
            report.presentationTimingSource = string.Empty;
            report.presentationTimingUnavailableReason =
                "Disabled by request; set enablePresentationTiming to true to collect " +
                "coarse presentation publication timings.";
            if (report.presentationTimings == null)
                report.presentationTimings = new List<ProductionEntityStressPhaseTimingSummary>();
            else
                report.presentationTimings.Clear();
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal sealed class ProductionEntityStressLogicTickTimingCollector
    {
        private const string Source =
            "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick";

        private readonly List<double> allSamples = new List<double>(512);
        private readonly List<double> withPresentationSamples = new List<double>(512);
        private readonly List<double> withoutPresentationSamples = new List<double>(512);

        internal int AllSampleCount => allSamples.Count;
        internal int WithPresentationSampleCount => withPresentationSamples.Count;
        internal int WithoutPresentationSampleCount => withoutPresentationSamples.Count;

        internal void AddSample(double elapsedMilliseconds, bool buildPresentation)
        {
            AddRollingSample(allSamples, elapsedMilliseconds);
            AddRollingSample(
                buildPresentation ? withPresentationSamples : withoutPresentationSamples,
                elapsedMilliseconds);
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            report.logicTickMilliseconds = ProductionEntityStressStatistics.Summarize(
                allSamples,
                "ms",
                Source);
            report.logicTickWithPresentationMilliseconds = ProductionEntityStressStatistics.Summarize(
                withPresentationSamples,
                "ms",
                Source + "; buildPresentation=true");
            report.logicTickWithoutPresentationMilliseconds = ProductionEntityStressStatistics.Summarize(
                withoutPresentationSamples,
                "ms",
                Source + "; buildPresentation=false");
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal sealed class ProductionEntityStressDetailPhaseTimingCollector
    {
        internal const string Source =
            "Independent Stopwatch timestamps accumulated inside CharacterInput and " +
            "LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried " +
            "from the preceding render pass into the next logic-tick sample. Nested diagnostic " +
            "evidence only, with no performance threshold.";
        internal const string AiInputSource =
            "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; " +
            "nested diagnostic evidence only, with no performance threshold.";
        internal const string LateRuntimeSnapshotSource =
            "Independent Stopwatch timestamps around individual LateEntityUpdate " +
            "RefreshRuntimeSnapshot calls. Stage names are stable pass-location markers, " +
            "not source-code line numbers; nested diagnostic evidence only, with no " +
            "performance threshold.";

        private readonly List<double>[] phaseSamples;
        private List<double>[] aiInputPhaseSamples;
        private List<double>[] lateRuntimeSnapshotSamples;
        private readonly long[] radiusHistogram =
            new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
        private readonly long[] aiInputPhaseCallCounts =
            new long[BattleAiInputDetailDiagnostics.PhaseCount];
        private readonly long[] aiInputPhaseSlotVisitCounts =
            new long[BattleAiInputDetailDiagnostics.PhaseCount];
        private readonly long[] aiInputPhaseRngCallCounts =
            new long[BattleAiInputDetailDiagnostics.PhaseCount];
        private readonly long[] lateRuntimeSnapshotCallCounts =
            new long[(int)BattleLateRuntimeSnapshotStage.Count];
        private long aiCount;
        private long spatialQueryCount;
        private long queriedHandleVisits;
        private long candidateVisits;
        private long radiusExpansions;
        private long bruteFallbackCount;
        private long bruteSlotVisits;
        private long phase1ListVisits;
        private long refreshCount;

        internal ProductionEntityStressDetailPhaseTimingCollector()
        {
            phaseSamples = new List<double>[BattleTickDetailPhaseDiagnostics.PhaseCount];
            for (int i = 0; i < phaseSamples.Length; i++)
                phaseSamples[i] = new List<double>(512);
        }

        internal int SampleCount { get; private set; }
        internal int AiInputSampleCount { get; private set; }
        internal bool AiInputPhaseSamplesAllocatedForDiagnostics =>
            aiInputPhaseSamples != null;
        internal int LateRuntimeSnapshotSampleCount { get; private set; }
        internal bool LateRuntimeSnapshotSamplesAllocatedForDiagnostics =>
            lateRuntimeSnapshotSamples != null;

        internal void CaptureAfterTick(
            BattleTickDetailPhaseDiagnostics diagnostics,
            BattleAiInputDetailDiagnostics aiDiagnostics,
            int completedTickCount,
            int warmupTickCount)
        {
            if (diagnostics == null || !diagnostics.Enabled ||
                completedTickCount <= warmupTickCount)
            {
                return;
            }

            double timestampToMilliseconds =
                1000d / BattleTickDetailPhaseDiagnostics.TimestampFrequency;
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                double phaseMilliseconds = diagnostics.GetLastElapsedTimestampTicks(
                    (BattleTickDetailPhase)i) * timestampToMilliseconds;
                AddRollingSample(phaseSamples[i], phaseMilliseconds);
            }

            if (aiDiagnostics != null && aiDiagnostics.Enabled)
            {
                EnsureAiInputPhaseSamples();
                double aiTimestampToMilliseconds =
                    1000d / BattleAiInputDetailDiagnostics.TimestampFrequency;
                for (int i = 0; i < aiInputPhaseSamples.Length; i++)
                {
                    BattleAiInputDetailPhase phase =
                        (BattleAiInputDetailPhase)i;
                    double phaseMilliseconds = aiDiagnostics.GetLastElapsedTimestampTicks(
                        phase) * aiTimestampToMilliseconds;
                    AddRollingSample(aiInputPhaseSamples[i], phaseMilliseconds);
                    aiInputPhaseCallCounts[i] +=
                        aiDiagnostics.GetLastCallCount(phase);
                    aiInputPhaseSlotVisitCounts[i] +=
                        aiDiagnostics.GetLastSlotVisitCount(phase);
                    aiInputPhaseRngCallCounts[i] +=
                        aiDiagnostics.GetLastRngCallCount(phase);
                }

                aiCount += aiDiagnostics.AiCount;
                spatialQueryCount += aiDiagnostics.SpatialQueryCount;
                queriedHandleVisits += aiDiagnostics.QueriedHandleVisits;
                candidateVisits += aiDiagnostics.CandidateVisits;
                radiusExpansions += aiDiagnostics.RadiusExpansions;
                bruteFallbackCount += aiDiagnostics.BruteFallbackCount;
                bruteSlotVisits += aiDiagnostics.BruteSlotVisits;
                phase1ListVisits += aiDiagnostics.Phase1ListVisits;
                refreshCount += aiDiagnostics.RefreshCount;
                for (int i = 0; i < radiusHistogram.Length; i++)
                    radiusHistogram[i] += aiDiagnostics.GetRadiusHistogramValue(i);
                AiInputSampleCount++;
            }

            EnsureLateRuntimeSnapshotSamples();
            for (int i = 0; i < lateRuntimeSnapshotSamples.Length; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                double elapsedMilliseconds = diagnostics
                    .GetLastLateRuntimeSnapshotElapsedTimestampTicks(stage) *
                    timestampToMilliseconds;
                AddRollingSample(lateRuntimeSnapshotSamples[i], elapsedMilliseconds);
                lateRuntimeSnapshotCallCounts[i] +=
                    diagnostics.GetLastLateRuntimeSnapshotCallCount(stage);
            }
            LateRuntimeSnapshotSampleCount++;

            SampleCount++;
        }

        internal void PopulateReport(ProductionEntityStressReport report)
        {
            if (report == null)
                return;

            if (report.detailPhaseTimings == null)
            {
                report.detailPhaseTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.detailPhaseTimings.Clear();
            }
            if (report.aiInputDetailTimings == null)
            {
                report.aiInputDetailTimings =
                    new List<ProductionEntityStressPhaseTimingSummary>();
            }
            else
            {
                report.aiInputDetailTimings.Clear();
            }
            if (report.lateRuntimeSnapshotTimings == null)
            {
                report.lateRuntimeSnapshotTimings =
                    new List<ProductionEntityStressLateRuntimeSnapshotTimingSummary>();
            }
            else
            {
                report.lateRuntimeSnapshotTimings.Clear();
            }
            if (report.aiInputDetailCounters == null)
            {
                report.aiInputDetailCounters =
                    new ProductionEntityStressAiInputDetailCounters();
            }
            else if (report.aiInputDetailCounters.radiusHistogram == null ||
                     report.aiInputDetailCounters.radiusHistogram.Length !=
                     BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount)
            {
                report.aiInputDetailCounters.radiusHistogram =
                    new long[BattleAiInputDetailDiagnostics.RadiusHistogramBucketCount];
            }
            if (report.aiInputDetailCounters.phaseCallCounts == null ||
                report.aiInputDetailCounters.phaseCallCounts.Length !=
                    BattleAiInputDetailDiagnostics.PhaseCount)
            {
                report.aiInputDetailCounters.phaseCallCounts =
                    new long[BattleAiInputDetailDiagnostics.PhaseCount];
            }
            if (report.aiInputDetailCounters.phaseSlotVisitCounts == null ||
                report.aiInputDetailCounters.phaseSlotVisitCounts.Length !=
                    BattleAiInputDetailDiagnostics.PhaseCount)
            {
                report.aiInputDetailCounters.phaseSlotVisitCounts =
                    new long[BattleAiInputDetailDiagnostics.PhaseCount];
            }
            if (report.aiInputDetailCounters.phaseRngCallCounts == null ||
                report.aiInputDetailCounters.phaseRngCallCounts.Length !=
                    BattleAiInputDetailDiagnostics.PhaseCount)
            {
                report.aiInputDetailCounters.phaseRngCallCounts =
                    new long[BattleAiInputDetailDiagnostics.PhaseCount];
            }
            if (!report.detailPhaseTimingEnabled)
            {
                report.detailPhaseTimingSource = string.Empty;
                report.detailPhaseTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.";
                report.aiInputDetailCounters.available = false;
                report.aiInputDetailCounters.unavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail counters.";
                report.aiInputDetailTimingSource = string.Empty;
                report.aiInputDetailTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect AI input detail timings.";
                report.lateRuntimeSnapshotTimingSource = string.Empty;
                report.lateRuntimeSnapshotTimingUnavailableReason =
                    "Disabled by request; set enableDetailPhaseTiming to true to collect " +
                    "LateEntityUpdate RefreshRuntimeSnapshot timings.";
                return;
            }

            report.detailPhaseTimingSource = Source;
            report.detailPhaseTimingUnavailableReason = string.Empty;
            report.aiInputDetailTimingSource = AiInputSource;
            report.aiInputDetailTimingUnavailableReason = AiInputSampleCount > 0
                ? string.Empty
                : "No completed AI input detail timing samples.";
            report.aiInputDetailCounters.available = AiInputSampleCount > 0;
            report.aiInputDetailCounters.unavailableReason = AiInputSampleCount > 0
                ? string.Empty
                : "No completed AI input detail timing samples.";
            report.aiInputDetailCounters.aiCount = aiCount;
            report.aiInputDetailCounters.spatialQueryCount = spatialQueryCount;
            report.aiInputDetailCounters.queriedHandleVisits = queriedHandleVisits;
            report.aiInputDetailCounters.candidateVisits = candidateVisits;
            report.aiInputDetailCounters.radiusExpansions = radiusExpansions;
            report.aiInputDetailCounters.bruteFallbackCount = bruteFallbackCount;
            report.aiInputDetailCounters.bruteSlotVisits = bruteSlotVisits;
            report.aiInputDetailCounters.phase1ListVisits = phase1ListVisits;
            report.aiInputDetailCounters.refreshCount = refreshCount;
            Array.Copy(
                aiInputPhaseCallCounts,
                report.aiInputDetailCounters.phaseCallCounts,
                aiInputPhaseCallCounts.Length);
            Array.Copy(
                aiInputPhaseSlotVisitCounts,
                report.aiInputDetailCounters.phaseSlotVisitCounts,
                aiInputPhaseSlotVisitCounts.Length);
            Array.Copy(
                aiInputPhaseRngCallCounts,
                report.aiInputDetailCounters.phaseRngCallCounts,
                aiInputPhaseRngCallCounts.Length);
            Array.Copy(radiusHistogram, report.aiInputDetailCounters.radiusHistogram, radiusHistogram.Length);
            report.lateRuntimeSnapshotTimingSource = LateRuntimeSnapshotSource;
            report.lateRuntimeSnapshotTimingUnavailableReason =
                LateRuntimeSnapshotSampleCount > 0
                    ? string.Empty
                    : "No completed LateEntityUpdate RefreshRuntimeSnapshot timing samples.";
            for (int i = 0; i < phaseSamples.Length; i++)
            {
                BattleTickDetailPhase phase = (BattleTickDetailPhase)i;
                report.detailPhaseTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleTickDetailPhaseDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        phaseSamples[i],
                        "ms",
                        Source),
                });
            }

            for (int i = 0; i < BattleAiInputDetailDiagnostics.PhaseCount; i++)
            {
                BattleAiInputDetailPhase phase = (BattleAiInputDetailPhase)i;
                report.aiInputDetailTimings.Add(new ProductionEntityStressPhaseTimingSummary
                {
                    phase = BattleAiInputDetailDiagnostics.GetPhaseName(phase),
                    timing = ProductionEntityStressStatistics.Summarize(
                        aiInputPhaseSamples?[i],
                        "ms",
                        AiInputSource),
                });
            }

            for (int i = 0; i < (int)BattleLateRuntimeSnapshotStage.Count; i++)
            {
                BattleLateRuntimeSnapshotStage stage =
                    (BattleLateRuntimeSnapshotStage)i;
                report.lateRuntimeSnapshotTimings.Add(
                    new ProductionEntityStressLateRuntimeSnapshotTimingSummary
                    {
                        stage = BattleTickDetailPhaseDiagnostics
                            .GetLateRuntimeSnapshotStageName(stage),
                        callCount = lateRuntimeSnapshotCallCounts[i],
                        timing = ProductionEntityStressStatistics.Summarize(
                            lateRuntimeSnapshotSamples?[i],
                            "ms",
                            LateRuntimeSnapshotSource),
                    });
            }
        }

        private void EnsureAiInputPhaseSamples()
        {
            if (aiInputPhaseSamples != null)
                return;

            aiInputPhaseSamples =
                new List<double>[BattleAiInputDetailDiagnostics.PhaseCount];
            for (int i = 0; i < aiInputPhaseSamples.Length; i++)
                aiInputPhaseSamples[i] = new List<double>(512);
        }

        private void EnsureLateRuntimeSnapshotSamples()
        {
            if (lateRuntimeSnapshotSamples != null)
                return;

            lateRuntimeSnapshotSamples =
                new List<double>[(int)BattleLateRuntimeSnapshotStage.Count];
            for (int i = 0; i < lateRuntimeSnapshotSamples.Length; i++)
                lateRuntimeSnapshotSamples[i] = new List<double>(512);
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= ProductionEntityStressRunner.MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal static class ProductionEntityStressPhaseTimingLifecycle
    {
        internal static void Disable(SimulationWorld world)
        {
            world?.DisableBattleTickPhaseDiagnosticsForDiagnostics();
            world?.DisableBattlePresentationPhaseDiagnosticsForDiagnostics();
            world?.DisableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            world?.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
        }
    }

    internal static class ProductionEntityStressRunStatusPolicy
    {
        internal static string ResolveCleanupStatus(
            string currentStatus,
            string reason,
            bool restored)
        {
            if (!string.Equals(reason, "runner-destroyed", StringComparison.Ordinal))
                return currentStatus;

            return restored ? "InterruptedCleanly" : "InterruptedWithResidue";
        }
    }

    internal static class ProductionEntityStressPopulationPolicy
    {
        internal static bool Evaluate(
            int requestedEntityCount,
            int activeGameObjectCount,
            int rootChildCount,
            int worldObjectCount,
            int worldEntityCount,
            int claimedRuntimeSlotCount)
        {
            return requestedEntityCount > 0 &&
                   activeGameObjectCount == requestedEntityCount &&
                   rootChildCount == requestedEntityCount &&
                   worldObjectCount == requestedEntityCount * 2 &&
                   worldEntityCount == requestedEntityCount &&
                   claimedRuntimeSlotCount == requestedEntityCount;
        }
    }

    internal enum ProductionEntityStressReplenishmentAction
    {
        None,
        Attempt,
        Drain,
        SaturationBlockedReplenishment,
    }

    internal static class ProductionEntityStressReplenishmentPolicy
    {
        internal const string SaturationBlockedResult =
            "SaturationBlockedReplenishment";

        internal static ProductionEntityStressReplenishmentAction Evaluate(
            int baseRosterActiveCount,
            int requestedEntityCount,
            int totalActiveRuntimeEntityCount,
            int totalClaimedRuntimeSlotCount,
            int maximumActiveRuntimeEntityCount,
            int currentSaturationDrainTicks,
            int maximumSaturationDrainTicks)
        {
            if (baseRosterActiveCount >= requestedEntityCount)
                return ProductionEntityStressReplenishmentAction.None;

            bool saturated = totalActiveRuntimeEntityCount >= maximumActiveRuntimeEntityCount ||
                             totalClaimedRuntimeSlotCount >= maximumActiveRuntimeEntityCount;
            if (!saturated)
                return ProductionEntityStressReplenishmentAction.Attempt;

            return currentSaturationDrainTicks >= Math.Max(1, maximumSaturationDrainTicks)
                ? ProductionEntityStressReplenishmentAction
                    .SaturationBlockedReplenishment
                : ProductionEntityStressReplenishmentAction.Drain;
        }
    }

    internal static class ProductionEntityStressSamplePolicy
    {
        internal static bool IsSteadyStateSample(
            int completedLogicTickCount,
            int warmupTickCount,
            int baseRosterActiveCount,
            int requestedEntityCount,
            bool rosterMutatedDuringTick,
            bool poolExpandedDuringTick)
        {
            return completedLogicTickCount > warmupTickCount &&
                   baseRosterActiveCount == requestedEntityCount &&
                   !rosterMutatedDuringTick &&
                   !poolExpandedDuringTick;
        }
    }

    internal static class ProductionEntityStressTeardownPolicy
    {
        internal static bool IsRestored(
            int activeGameObjects,
            int worldObjects,
            int worldEntities,
            int claimedSlots,
            int objectPoolActive,
            int objectPoolAvailable,
            int referencePoolActive,
            int objectPoolActiveBaseline,
            int objectPoolAvailableBaseline,
            int referencePoolActiveBaseline)
        {
            _ = objectPoolAvailable;
            _ = objectPoolAvailableBaseline;
            return activeGameObjects == 0 &&
                   worldObjects == 0 &&
                   worldEntities == 0 &&
                   claimedSlots == 0 &&
                   objectPoolActive == objectPoolActiveBaseline &&
                   referencePoolActive == referencePoolActiveBaseline;
        }

        internal static int CountActiveStressRootGameObjects(Transform stressRoot)
        {
            if (stressRoot == null)
                return 0;

            int count = 0;
            for (int i = 0; i < stressRoot.childCount; i++)
            {
                GameObject child = stressRoot.GetChild(i)?.gameObject;
                if (child != null && child.activeInHierarchy)
                    count++;
            }
            return count;
        }

        internal static string BuildEvidence(
            string reason,
            ProductionEntityStressTeardownReport teardown)
        {
            if (teardown == null)
                return string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "reason={0}; restored={1}; activeCleanupRestored={2}; driverRestored={3}; " +
                "loggerRestored={4}; cleanupExceptions={5}; activeGO={6}->{7}; " +
                "worldObjects={8}->{9}; worldEntities={10}->{11}; claimed={12}->{13}; " +
                "objectPoolActive={14}->{15}; referencePoolActive={16}->{17}; " +
                "retainedInactiveObjectPoolCapacity={18}->{19} (delta={20}; " +
                "doesNotAffectRestored=True)",
                reason,
                teardown.restored,
                teardown.activeStateRestored,
                teardown.driverStateRestored,
                teardown.loggingStateRestored,
                teardown.cleanupExceptionCount,
                teardown.activeGameObjectsBefore,
                teardown.activeGameObjectsAfter,
                teardown.worldObjectsBefore,
                teardown.worldObjectsAfter,
                teardown.worldEntitiesBefore,
                teardown.worldEntitiesAfter,
                teardown.claimedSlotsBefore,
                teardown.claimedSlotsAfter,
                teardown.objectPoolActiveBeforeRun,
                teardown.objectPoolActiveAfter,
                teardown.referencePoolActiveBeforeRun,
                teardown.referencePoolActiveAfter,
                teardown.retainedInactiveObjectPoolCapacityBeforeRun,
                teardown.retainedInactiveObjectPoolCapacityAfter,
                teardown.retainedInactiveObjectPoolCapacityDelta);
        }
    }

    internal sealed class ProductionEntityStressCleanupJournal
    {
        private readonly List<string> failures = new List<string>();

        internal int FailureCount => failures.Count;

        internal bool Attempt(string phase, Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
                return true;
            }
            catch (Exception exception)
            {
                failures.Add(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}: {1}: {2}",
                    phase ?? "cleanup",
                    exception.GetType().Name,
                    exception.Message));
                return false;
            }
        }

        internal string FormatFailures()
        {
            return string.Join(" | ", failures);
        }
    }

    internal static class ProductionEntityStressDerivedObservationPolicy
    {
        internal static bool TryRecord(
            RuntimeEntityHandle handle,
            ISet<RuntimeEntityHandle> harnessOwned,
            ISet<RuntimeEntityHandle> observedDerived)
        {
            return handle.IsValid &&
                   (harnessOwned == null || !harnessOwned.Contains(handle)) &&
                   observedDerived != null &&
                   observedDerived.Add(handle);
        }
    }

    internal static class ProductionEntityStressFingerprint
    {
        internal static string BuildWorkload(ProductionEntityStressConfig config)
        {
            string canonical = string.Join(
                "|",
                "workload-v3",
                config.Mode.ToString(),
                ProductionEntityStressConfig.FormatInputMode(config.InputMode),
                config.EntityCount.ToString(CultureInfo.InvariantCulture),
                config.WarmupTicks.ToString(CultureInfo.InvariantCulture),
                config.SampleTicks.ToString(CultureInfo.InvariantCulture),
                config.Seed.ToString(CultureInfo.InvariantCulture),
                config.SimulationOnly ? "simulation-only" : "with-presentation",
                ProductionEntityStressConfig.FormatFormalCollectorMode(config.FormalCollectorMode),
                "spawn-batch-" + config.SpawnBatchSize.ToString(CultureInfo.InvariantCulture),
                "max-catchup-" + config.MaxCatchUpTicksPerFrame.ToString(CultureInfo.InvariantCulture),
                "max-backlog-" + config.MaxBacklogTicks.ToString(CultureInfo.InvariantCulture),
                "max-saturation-drain-" +
                config.MaxSaturationDrainTicks.ToString(CultureInfo.InvariantCulture),
                config.ShouldAutoStopWhenSampled ? "auto-stop-on" : "auto-stop-off",
                config.EnablePhaseTiming ? "phase-on" : "phase-off",
                config.EnablePresentationTiming ? "presentation-timing-on" : "presentation-timing-off",
                config.EnableDetailPhaseTiming ? "detail-on" : "detail-off");
            return BattleCanonicalJson.Sha256(canonical);
        }

        internal static string BuildImplementationConfig(ProductionEntityStressConfig config)
        {
            string canonical = string.Join(
                "|",
                "implementation-config-v6",
                "ai-execution-profile-" +
                ProductionEntityStressConfig.FormatAiExecutionProfile(
                    config.AiExecutionProfile),
                config.UsesLegacyAiConfigurationCompatibility
                    ? "ai-profile-legacy-request-compatibility"
                    : "ai-profile-production-api",
                ProductionEntityStressConfig.FormatAiSensingMode(config.AiSensingMode),
                ProductionEntityStressConfig.FormatLateRuntimeSnapshotMode(
                    config.LateRuntimeSnapshotMode),
                config.AllowUnsafeAiSoACandidate
                    ? "unsafe-candidate-opt-in"
                    : "unsafe-candidate-disabled",
                ProductionEntityStressConfig.FormatFormalCollectorMode(
                    config.FormalCollectorMode),
                config.ForceRoleAwareDirect
                    ? "role-direct-force-on"
                    : "role-direct-force-off",
                config.ForceRoleAwareTree
                    ? "role-tree-force-on"
                    : "role-tree-force-off",
                config.ForceRoleAwareNestedDirect
                    ? "role-nested-force-on"
                    : "role-nested-force-off",
                config.ForceRoleAwareSweepDirect
                    ? "role-sweep-force-on"
                    : "role-sweep-force-off",
                config.EnableCollisionCandidateStoreShadow
                    ? "candidate-store-shadow-on"
                    : "candidate-store-shadow-off",
                config.EnableCollisionCandidateStoreAuthority
                    ? "candidate-store-authority-on"
                    : "candidate-store-authority-off",
                config.EnableCollisionRoleZeroItrFastPath
                    ? "role-zero-itr-fastpath-on"
                    : "role-zero-itr-fastpath-off",
                config.SkipLateRendererUpdate
                    ? "skip-late-renderer-update-on"
                    : "skip-late-renderer-update-off",
                "sound-presentation-requested-" +
                ProductionEntityStressConfig.FormatSoundPresentationMode(
                    config.SoundPresentationMode),
                "sound-presentation-resolved-" +
                ProductionEntityStressConfig.FormatResolvedSoundPresentationMode(config),
                config.EnableAiSoADecisionRemainder
                    ? "ai-soa-decision-remainder-on"
                    : "ai-soa-decision-remainder-off",
                config.EnableAiDecisionSoAShadow
                    ? "ai-decision-soa-shadow-on"
                    : "ai-decision-soa-shadow-off",
                config.EnableAiDecisionSharedShadow
                    ? "ai-decision-shared-shadow-on"
                    : "ai-decision-shared-shadow-off",
                "candidate-store-oracle-interval-" +
                config.LegacyOracleInterval.ToString(CultureInfo.InvariantCulture));
            return BattleCanonicalJson.Sha256(canonical);
        }

        internal static string BuildRoster(
            ProductionEntityStressMode mode,
            int entityCount,
            int selectedCharacterOid)
        {
            var canonical = new StringBuilder(Math.Max(128, entityCount * 48));
            canonical.Append("roster-v1|");
            canonical.Append(selectedCharacterOid.ToString(CultureInfo.InvariantCulture));
            canonical.Append('|');
            canonical.Append(entityCount.ToString(CultureInfo.InvariantCulture));
            for (int index = 0; index < entityCount; index++)
            {
                Vector3 position = ProductionEntityStressRunner.BuildSpawnPosition(
                    mode,
                    index,
                    entityCount);
                canonical.Append('|');
                canonical.Append(index.ToString(CultureInfo.InvariantCulture));
                canonical.Append(',');
                canonical.Append(((index & 1) + 1).ToString(CultureInfo.InvariantCulture));
                canonical.Append(',');
                canonical.Append((index & 1).ToString(CultureInfo.InvariantCulture));
                canonical.Append(',');
                canonical.Append(position.x.ToString("R", CultureInfo.InvariantCulture));
                canonical.Append(',');
                canonical.Append(position.y.ToString("R", CultureInfo.InvariantCulture));
                canonical.Append(',');
                canonical.Append(position.z.ToString("R", CultureInfo.InvariantCulture));
            }
            return BattleCanonicalJson.Sha256(canonical.ToString());
        }
    }

    internal static class ProductionEntityStressParityReport
    {
        internal static void Populate(
            ProductionEntityStressReport report,
            IBattleChecksumSnapshot snapshot)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            report.finalParitySnapshotSchema = snapshot.Schema ?? string.Empty;
            report.finalParitySnapshotTick = snapshot.Tick;
            report.finalParityOverallHash = snapshot.OverallChecksum ?? string.Empty;
            switch (snapshot)
            {
                case BattleParityFrameSnapshot parity:
                    report.finalParityInputHash = parity.Hashes?.Input ?? string.Empty;
                    report.finalParityRngHash = parity.Hashes?.Rng ?? string.Empty;
                    report.finalParityWorldHash = parity.Hashes?.World ?? string.Empty;
                    report.finalParitySlotsHash = parity.Hashes?.Slots ?? string.Empty;
                    report.finalParityARestHash = parity.Hashes?.ARest ?? string.Empty;
                    report.finalParityVRestHash = parity.Hashes?.VRest ?? string.Empty;
                    report.finalParityStatsHash = parity.Hashes?.Stats ?? string.Empty;
                    report.finalParityEventsHash = parity.Hashes?.Events ?? string.Empty;
                    break;
                case BattleExtendedChecksumSnapshot extended:
                    report.finalParityInputHash = extended.Hashes?.Input ?? string.Empty;
                    report.finalParityRngHash = extended.Hashes?.Rng ?? string.Empty;
                    report.finalParityMetadataHash = extended.Hashes?.Metadata ?? string.Empty;
                    report.finalParityWorldHash = extended.Hashes?.World ?? string.Empty;
                    report.finalParitySlotsHash = extended.Hashes?.Slots ?? string.Empty;
                    report.finalParityARestHash = extended.Hashes?.ARest ?? string.Empty;
                    report.finalParityVRestHash = extended.Hashes?.VRest ?? string.Empty;
                    report.finalParityStatsHash = extended.Hashes?.Stats ?? string.Empty;
                    report.finalParityEventsHash = extended.Hashes?.Events ?? string.Empty;
                    break;
                default:
                    throw new NotSupportedException(
                        $"Unsupported production stress checksum snapshot '{snapshot.GetType().FullName}'.");
            }
        }

        internal static void PopulateLockstep(
            ProductionEntityStressReport report,
            BattleLockstepChecksumSnapshot snapshot)
        {
            if (report == null)
                throw new ArgumentNullException(nameof(report));
            if (snapshot == null)
                throw new ArgumentNullException(nameof(snapshot));

            report.finalLockstepSchema = snapshot.Schema ?? string.Empty;
            report.finalLockstepTick = snapshot.Tick;
            report.finalLockstepInputHash = snapshot.Hashes?.Input ?? string.Empty;
            report.finalLockstepRngHash = snapshot.Hashes?.Rng ?? string.Empty;
            report.finalLockstepMetadataHash = snapshot.Hashes?.Metadata ?? string.Empty;
            report.finalLockstepWorldHash = snapshot.Hashes?.World ?? string.Empty;
            report.finalLockstepSlotsHash = snapshot.Hashes?.Slots ?? string.Empty;
            report.finalLockstepARestHash = snapshot.Hashes?.ARest ?? string.Empty;
            report.finalLockstepVRestHash = snapshot.Hashes?.VRest ?? string.Empty;
            report.finalLockstepStatsHash = snapshot.Hashes?.Stats ?? string.Empty;
            report.finalLockstepEventsHash = snapshot.Hashes?.Events ?? string.Empty;
            report.finalLockstepOverallHash = snapshot.Hashes?.Overall ?? string.Empty;
        }
    }

    public sealed class ProductionEntityStressRunner : MonoBehaviour
    {
        internal const int MaximumRetainedSamples = 4096;
        private const int MaximumCleanupPasses = 8;
        private const string CollisionCandidateStoreShadowFailurePrefix =
            "CollisionCandidateStoreShadowInvalid:";
        private const string CollisionCandidateStoreAuthorityFailurePrefix =
            "CollisionCandidateStoreAuthorityInvalid:";
        private static readonly ProfilerMarker RunnerUpdateProfilerMarker =
            new ProfilerMarker("NTSD.ProductionEntityStress.Runner.Update");
        private static readonly ProfilerMarker SpawnOrRemoveProfilerMarker =
            new ProfilerMarker("NTSD.ProductionEntityStress.SpawnOrRemove");
        private static readonly ProfilerMarker StepMeasuredTickProfilerMarker =
            new ProfilerMarker("NTSD.ProductionEntityStress.StepMeasuredTick");
        private static readonly ProfilerMarker DriverStepOneTickProfilerMarker =
            new ProfilerMarker("NTSD.ProductionEntityStress.Driver.StepOneTick");
        private static readonly ProfilerMarker PostTickTimingCollectorsProfilerMarker =
            new ProfilerMarker("NTSD.ProductionEntityStress.PostTickTimingCollectors");
        private static readonly ProfilerMarker CaptureProductionCountersProfilerMarker =
            new ProfilerMarker("NTSD.ProductionEntityStress.CaptureProductionCounters");
        private static readonly ProfilerMarker ActiveEntityScanProfilerMarker =
            new ProfilerMarker(
                "NTSD.ProductionEntityStress.CaptureProductionCounters.ActiveEntityScan");
        private static readonly ProfilerMarker SceneQueryDiagnosticsProfilerMarker =
            new ProfilerMarker(
                "NTSD.ProductionEntityStress.CaptureProductionCounters.SceneQueryDiagnostics");
        private static readonly ProfilerMarker AiReportDiagnosticsProfilerMarker =
            new ProfilerMarker(
                "NTSD.ProductionEntityStress.CaptureProductionCounters.AiReportDiagnostics");
        private static readonly ProfilerMarker ObserveRuntimeEntitySnapshotProfilerMarker =
            new ProfilerMarker(
                "NTSD.ProductionEntityStress.CaptureProductionCounters.ObserveRuntimeEntitySnapshot");
        private static readonly ProfilerMarker WriteReportProfilerMarker =
            new ProfilerMarker("NTSD.ProductionEntityStress.WriteReport");

        private readonly List<LF2Character> entities = new List<LF2Character>(1000);
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(1050);
        private readonly HashSet<RuntimeEntityHandle> harnessOwnedHandles =
            new HashSet<RuntimeEntityHandle>();
        private readonly HashSet<RuntimeEntityHandle> observedDerivedHandles =
            new HashSet<RuntimeEntityHandle>();
        private readonly List<double> frameSamples = new List<double>(512);
        private readonly List<double> allocationSamples = new List<double>(512);
        private readonly ProductionEntityStressLoggingPolicy loggingPolicy =
            new ProductionEntityStressLoggingPolicy();
        private readonly ProductionEntityStressLogicTickTimingCollector logicTickTimingCollector =
            new ProductionEntityStressLogicTickTimingCollector();
        private readonly ProductionEntityStressDetailPhaseTimingCollector detailPhaseTimingCollector =
            new ProductionEntityStressDetailPhaseTimingCollector();
        private ProductionEntityStressProfilerFrameGcCollector profilerFrameGcCollector;

        private ProductionEntityStressConfig config;
        private ProductionEntityStressReport report;
        private SimulationTickDriver driver;
        private SimulationWorld world;
        private LF2ObjectPool objectPool;
        private LF2ReferencePool referencePool;
        private LF2ObjectPointFactory objectPointFactory;
        private ProductionEntityStressPhaseTimingCollector phaseTimingCollector;
        private ProductionEntityStressPresentationTimingCollector presentationTimingCollector;
        private BattleTickPhaseDiagnostics phaseTimingDiagnostics;
        private BattlePresentationPhaseDiagnostics presentationTimingDiagnostics;
        private BattleTickDetailPhaseDiagnostics detailPhaseTimingDiagnostics;
        private BattleAiInputDetailDiagnostics aiInputDetailDiagnostics;
        private BruteForceSceneQuery collisionCandidateStoreShadowQuery;
        private CollisionCandidateStoreAuthorityDiagnosticsSnapshot
            collisionCandidateStoreAuthorityDiagnosticsBaseline;
        private LockstepSimulationSettings previousSettings;
        private bool previousCollisionCandidateStoreShadowEnabled;
        private bool previousCollisionCandidateStoreAuthorityEnabled;
        private int previousCollisionCandidateStoreLegacyOracleInterval;
        private bool previousCollisionRoleZeroItrFastPathEnabled;
        private bool previousAiSoADecisionRemainderEnabled;
        private AiDecisionShadowMode previousAiDecisionShadowMode;
        private AiDecisionExecutionMode previousAiDecisionExecutionMode;
        private int previousAiDecisionFullOracleSampleInterval;
        private AiUnifiedSnapshotShadowMode previousUnifiedAiSnapshotShadowMode;
        private AiUnifiedSnapshotExecutionMode previousAiUnifiedSnapshotExecutionMode;
        private bool previousSkipLateRendererUpdate;
        private long skipLateRendererUpdateTickCountBaseline;
        private bool previousSoundPresentationSuppressed;
        private long dispatchedSoundEventCountBaseline;
        private long suppressedSoundEventCountBaseline;
        private bool soundPresentationSuppressionStateCaptured;
        private bool previousPaused;
        private int objectPoolActiveBaseline;
        private int objectPoolAvailableBaseline;
        private int referencePoolActiveBaseline;
        private int selectedCharacterOid;
        private int frameCounter;
        private float accumulator;
        private bool initialPopulationComplete;
        private bool saturationDrainActive;
        private bool saturationBlocked;
        private bool rosterMutationPendingForNextTick;
        private bool reportWriteDeferredForProfilerFrame;
        private bool configured;
        private bool cleaned;
        private bool cleanupInProgress;
        private bool terminalCpuDiagnosticsFlushPending;
        private bool driverConfigurationChanged;
        [SerializeField, HideInInspector]
        private bool preserveRequestProcessorStateOnDestroy;

        public static ProductionEntityStressRunner Active { get; private set; }
        public ProductionEntityStressReport Report => report;

        internal static ProductionEntityStressRunner StartRun(ProductionEntityStressConfig runConfig)
        {
            if (!Application.isPlaying)
                throw new InvalidOperationException("Production entity stress requires Play Mode.");
            if (Active != null)
                throw new InvalidOperationException("A production entity stress run is already active.");

            var root = new GameObject($"NTSD Production Entity Stress [{runConfig.Mode}]");
            var runner = root.AddComponent<ProductionEntityStressRunner>();
            Active = runner;
            try
            {
                runner.Configure(runConfig);
                return runner;
            }
            catch
            {
                runner.CleanupInternal("start-failed", true);
                UnityEngine.Object.Destroy(root);
                throw;
            }
        }

        internal static bool AreProductionServicesReady()
        {
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            return SimulationTickDriver.Instance?.World != null &&
                   LF2ObjectPool.Instance != null &&
                   LF2ReferencePool.Instance != null &&
                   LF2ObjectPointFactory.Instance != null &&
                   manager != null &&
                   dataManager != null &&
                   dataManager.IsLoaded() &&
                   TrySelectLoadedCharacter(manager, dataManager, out _);
        }

        internal static Vector3 BuildSpawnPosition(
            ProductionEntityStressMode mode,
            int index,
            int entityCount)
        {
            if (mode == ProductionEntityStressMode.Concentrated1000)
            {
                int column = index % 20;
                int row = (index / 20) % 10;
                return new Vector3(385f + column * 1.5f, 0f, 242f + row * 1.5f);
            }

            int columns = mode == ProductionEntityStressMode.Smoke50 ? 10 : 40;
            int rows = Math.Max(1, (entityCount + columns - 1) / columns);
            int xIndex = index % columns;
            int zIndex = index / columns;
            float x = 20f + xIndex * (760f / Math.Max(1, columns - 1));
            float z = 185f + zIndex * (160f / Math.Max(1, rows - 1));
            return new Vector3(x, 0f, z);
        }

        internal static bool ResolveBuildPresentationForStressTick(
            bool simulationOnly,
            float remainingAccumulator,
            int ticksAlreadyExecuted,
            int maxCatchUpTicksPerFrame)
        {
            return !simulationOnly && SimulationTickDriver.IsFinalCatchUpTick(
                remainingAccumulator,
                ticksAlreadyExecuted,
                maxCatchUpTicksPerFrame);
        }

        internal static bool ShouldExecuteCatchUpTickForStressSample(
            bool shouldAutoStopWhenSampled,
            int sampledLogicTicks,
            int targetSampleTicks)
        {
            return !shouldAutoStopWhenSampled || sampledLogicTicks < targetSampleTicks;
        }

        internal static bool ApplySkipLateRendererUpdateForDiagnostics(
            SimulationWorld targetWorld,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport,
            out long tickCountBaseline)
        {
            if (targetWorld == null)
                throw new ArgumentNullException(nameof(targetWorld));
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));
            if (runConfig.SkipLateRendererUpdate && !runConfig.SimulationOnly)
            {
                throw new InvalidOperationException(
                    "SkipLateRendererUpdate may only be enabled for a simulation-only stress run.");
            }

            tickCountBaseline = targetWorld.SkippedLateRendererUpdateTickCountForDiagnostics;
            bool previous = targetWorld.ConfigureSkipLateRendererUpdateForDiagnostics(
                runConfig.SkipLateRendererUpdate,
                runConfig.SimulationOnly);
            targetReport.skipLateRendererUpdateRequested =
                runConfig.SkipLateRendererUpdate;
            targetReport.skipLateRendererUpdateConfigured =
                targetWorld.SkipLateRendererUpdateForDiagnostics;
            targetReport.skipLateRendererUpdateApplied = false;
            targetReport.skipLateRendererUpdateRestored = false;
            targetReport.skipLateRendererUpdateTickCount = 0;
            if (targetWorld.SkipLateRendererUpdateForDiagnostics !=
                runConfig.SkipLateRendererUpdate)
            {
                throw new InvalidOperationException(
                    "SimulationWorld rejected the requested SkipLateRendererUpdate state.");
            }

            return previous;
        }

        internal static void CaptureSkipLateRendererUpdateForReport(
            ProductionEntityStressReport targetReport,
            SimulationWorld targetWorld,
            long tickCountBaseline)
        {
            if (targetReport == null || targetWorld == null)
                return;

            long current = targetWorld.SkippedLateRendererUpdateTickCountForDiagnostics;
            long observedTickCount = Math.Max(0L, current - tickCountBaseline);
            targetReport.skipLateRendererUpdateTickCount = Math.Max(
                targetReport.skipLateRendererUpdateTickCount,
                observedTickCount);
            targetReport.skipLateRendererUpdateApplied |=
                targetReport.skipLateRendererUpdateRequested &&
                targetReport.skipLateRendererUpdateConfigured &&
                observedTickCount > 0;
        }

        internal static bool RestoreSkipLateRendererUpdateForDiagnostics(
            SimulationWorld targetWorld,
            bool previous,
            ProductionEntityStressReport targetReport,
            long tickCountBaseline)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));
            if (targetWorld == null)
            {
                targetReport.skipLateRendererUpdateRestored = false;
                return false;
            }

            CaptureSkipLateRendererUpdateForReport(
                targetReport,
                targetWorld,
                tickCountBaseline);
            targetWorld.RestoreSkipLateRendererUpdateForDiagnostics(previous);
            targetReport.skipLateRendererUpdateRestored =
                targetWorld.SkipLateRendererUpdateForDiagnostics == previous;
            return targetReport.skipLateRendererUpdateRestored;
        }

        internal static bool ApplySoundPresentationSuppressionForDiagnostics(
            SimulationTickDriver targetDriver,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport,
            out long dispatchedEventCountBaseline,
            out long suppressedEventCountBaseline)
        {
            if (targetDriver == null)
                throw new ArgumentNullException(nameof(targetDriver));
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            bool previous = targetDriver.SuppressSoundPresentationForDiagnostics;
            dispatchedEventCountBaseline =
                targetDriver.DispatchedSoundEventCountForDiagnostics;
            suppressedEventCountBaseline =
                targetDriver.SuppressedSoundEventCountForDiagnostics;
            bool suppressSoundPresentation = runConfig.SuppressSoundPresentation;
            targetReport.soundPresentationModeRequested =
                ProductionEntityStressConfig.FormatSoundPresentationMode(
                    runConfig.SoundPresentationMode);
            targetReport.soundPresentationModeResolved =
                ProductionEntityStressConfig.FormatResolvedSoundPresentationMode(runConfig);
            targetReport.soundPresentationSuppressionRequested = suppressSoundPresentation;
            targetDriver.SetSoundPresentationSuppressedForDiagnostics(
                suppressSoundPresentation);
            targetReport.soundPresentationSuppressionConfigured =
                suppressSoundPresentation &&
                targetDriver.SuppressSoundPresentationForDiagnostics;
            targetReport.soundPresentationSuppressionApplied =
                targetReport.soundPresentationSuppressionConfigured;
            targetReport.soundPresentationSuppressionRestored = false;
            targetReport.soundPresentationDispatchedEventCountDelta = 0;
            targetReport.soundPresentationSuppressedEventCountDelta = 0;
            return previous;
        }

        internal static void CaptureSoundPresentationSuppressionForReport(
            ProductionEntityStressReport targetReport,
            SimulationTickDriver targetDriver,
            long dispatchedEventCountBaseline,
            long suppressedEventCountBaseline)
        {
            if (targetReport == null || targetDriver == null)
                return;

            targetReport.soundPresentationDispatchedEventCountDelta = Math.Max(
                targetReport.soundPresentationDispatchedEventCountDelta,
                Math.Max(
                    0L,
                    targetDriver.DispatchedSoundEventCountForDiagnostics -
                    dispatchedEventCountBaseline));
            targetReport.soundPresentationSuppressedEventCountDelta = Math.Max(
                targetReport.soundPresentationSuppressedEventCountDelta,
                Math.Max(
                    0L,
                    targetDriver.SuppressedSoundEventCountForDiagnostics -
                    suppressedEventCountBaseline));
        }

        internal static bool RestoreSoundPresentationSuppressionForDiagnostics(
            SimulationTickDriver targetDriver,
            bool previous,
            ProductionEntityStressReport targetReport,
            long dispatchedEventCountBaseline,
            long suppressedEventCountBaseline)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));
            if (targetDriver == null)
            {
                targetReport.soundPresentationSuppressionRestored = false;
                return false;
            }

            CaptureSoundPresentationSuppressionForReport(
                targetReport,
                targetDriver,
                dispatchedEventCountBaseline,
                suppressedEventCountBaseline);
            targetDriver.SetSoundPresentationSuppressedForDiagnostics(previous);
            targetReport.soundPresentationSuppressionRestored =
                targetDriver.SuppressSoundPresentationForDiagnostics == previous;
            return targetReport.soundPresentationSuppressionRestored;
        }

        private void Configure(ProductionEntityStressConfig runConfig)
        {
            config = runConfig;
            report = new ProductionEntityStressReport
            {
                status = "Starting",
                mode = config.Mode.ToString(),
                inputMode = ProductionEntityStressConfig.FormatInputMode(config.InputMode),
                seed = config.Seed,
                aiExecutionProfileRequested =
                    ProductionEntityStressConfig.FormatAiExecutionProfile(
                        config.AiExecutionProfile),
                aiExecutionProfileLegacyCompatibility =
                    config.UsesLegacyAiConfigurationCompatibility,
                aiSensingRequestedMode =
                    ProductionEntityStressConfig.FormatAiSensingMode(config.AiSensingMode),
                aiDecisionSoAShadowRequested = config.EnableAiDecisionSoAShadow,
                aiDecisionSharedShadowRequested = config.EnableAiDecisionSharedShadow,
                aiDecisionExecutionRequestedMode = config.AiDecisionExecutionMode.ToString(),
                unifiedAiSnapshotShadowRequested = config.EnableUnifiedAiSnapshotShadow,
                unifiedAiSnapshotShadowFirstMismatch =
                    AiUnifiedSnapshotMismatchKind.None.ToString(),
                unifiedAiSnapshotShadowFirstExceptionStage =
                    AiUnifiedSnapshotExceptionStage.None.ToString(),
                unifiedAiSnapshotShadowFirstExceptionType = string.Empty,
                aiUnifiedSnapshotExecutionRequestedMode =
                    config.AiUnifiedSnapshotExecutionMode.ToString(),
                aiUnifiedSnapshotExecutionFirstFailureStage =
                    AiUnifiedSnapshotExceptionStage.None.ToString(),
                aiUnifiedSnapshotExecutionFirstFailureType = string.Empty,
                aiDecisionSoAShadowFirstReason =
                    AiDecisionShadowMismatchReason.None.ToString(),
                aiDecisionSoAShadowFirstUnavailableReason =
                    AiDecisionAvailability.None.ToString(),
                aiDecisionShadowFirstExceptionStage =
                    AiDecisionShadowExceptionStage.None.ToString(),
                aiDecisionShadowFirstExceptionType = string.Empty,
                aiDecisionIndexedFirstMismatchReason =
                    AiDecisionIndexedMismatchReason.None.ToString(),
                aiDecisionIndexedCanonicalFirstFallbackReason =
                    AiDecisionAvailability.None.ToString(),
                aiDecisionIndexedCanonicalFirstOracleMismatchReason =
                    AiDecisionIndexedMismatchReason.None.ToString(),
                lateRuntimeSnapshotRequestedMode =
                    ProductionEntityStressConfig.FormatLateRuntimeSnapshotMode(
                        config.LateRuntimeSnapshotMode),
                allowUnsafeAiSoACandidate = config.AllowUnsafeAiSoACandidate,
                forceRoleAwareDirectRequested = config.ForceRoleAwareDirect,
                forceRoleAwareTreeRequested = config.ForceRoleAwareTree,
                forceRoleAwareNestedDirectRequested =
                    config.ForceRoleAwareNestedDirect,
                forceRoleAwareSweepDirectRequested =
                    config.ForceRoleAwareSweepDirect,
                roleAwareDirectCostTickScope =
                    "All successful logic ticks including warmup; mirrors role-aware direct/tree total tick counters.",
                collisionCandidateStoreShadowRequested =
                    config.EnableCollisionCandidateStoreShadow,
                collisionCandidateStoreShadowFirstMismatchReason =
                    CollisionCandidateStoreMismatchReason.None.ToString(),
                collisionCandidateStoreAuthorityRequested =
                    config.EnableCollisionCandidateStoreAuthority,
                collisionCandidateStoreLegacyOracleInterval =
                    config.LegacyOracleInterval,
                collisionCandidateStoreAuthorityFirstFailureReason =
                    CollisionCandidateStoreAuthorityFailureReason.None.ToString(),
                collisionRoleZeroItrFastPathRequested =
                    config.EnableCollisionRoleZeroItrFastPath,
                simulationOnly = config.SimulationOnly,
                skipLateRendererUpdateRequested = config.SkipLateRendererUpdate,
                soundPresentationModeRequested =
                    ProductionEntityStressConfig.FormatSoundPresentationMode(
                        config.SoundPresentationMode),
                soundPresentationModeResolved =
                    ProductionEntityStressConfig.FormatResolvedSoundPresentationMode(config),
                soundPresentationSuppressionRequested =
                    config.SuppressSoundPresentation,
                autoStopWhenSampled = config.ShouldAutoStopWhenSampled,
                startedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                updatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
                unityVersion = Application.unityVersion,
                platform = Application.platform.ToString(),
                scene = gameObject.scene.name,
                stressRootName = gameObject.name,
                outputPath = config.OutputPath,
                requestedEntityCount = config.EntityCount,
                maxSaturationDrainTicks = config.MaxSaturationDrainTicks,
                replenishmentState = "InitialPopulation",
                opointCounterAvailable = true,
                opointCounterReason =
                    "Runtime-derived observable proxy: unique active non-harness runtime handles observed " +
                    "after each logic tick. It is not a production opoint creation counter.",
                phaseTimingEnabled = config.EnablePhaseTiming,
                phaseTimingSource = config.EnablePhaseTiming
                    ? ProductionEntityStressPhaseTimingCollector.Source
                    : string.Empty,
                presentationTimingEnabled = config.EnablePresentationTiming,
                presentationTimingSource = config.EnablePresentationTiming
                    ? ProductionEntityStressPresentationTimingCollector.Source
                    : string.Empty,
                detailPhaseTimingEnabled = config.EnableDetailPhaseTiming,
                detailPhaseTimingSource = config.EnableDetailPhaseTiming
                    ? ProductionEntityStressDetailPhaseTimingCollector.Source
                    : string.Empty,
                detailPhaseTimingUnavailableReason = config.EnableDetailPhaseTiming
                    ? string.Empty
                    : "Disabled by request; set enableDetailPhaseTiming to true to collect nested per-entity timings.",
            };
            report.workloadFingerprint = ProductionEntityStressFingerprint.BuildWorkload(config);
            report.implementationConfigFingerprint =
                ProductionEntityStressFingerprint.BuildImplementationConfig(config);
            loggingPolicy.Apply(report.loggingPolicy);
            profilerFrameGcCollector =
                new ProductionEntityStressProfilerFrameGcCollector();

            driver = SimulationTickDriver.Instance;
            objectPool = LF2ObjectPool.Instance;
            referencePool = LF2ReferencePool.Instance;
            objectPointFactory = LF2ObjectPointFactory.Instance;
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.Instance;
            if (driver == null || objectPool == null || referencePool == null ||
                objectPointFactory == null || manager == null || dataManager == null)
            {
                throw new InvalidOperationException(
                    "Required production services are missing. Load a battle scene before starting the stress harness.");
            }
            if (driver.World == null || driver.World.ObjectCount != 0 ||
                driver.World.ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new InvalidOperationException(
                    "The production stress harness requires an empty SimulationWorld.");
            }
            if (objectPool.ActiveObjectCountForAcceptance != 0 || referencePool.ActiveCount != 0)
            {
                throw new InvalidOperationException(
                    "The production stress harness requires both production pools to have zero active objects.");
            }
            report.worldObjectCountBeforeRun = driver.World.ObjectCount;
            driver.World.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch);
            report.worldEntityCountBeforeRun = entityScratch.Count;
            entityScratch.Clear();
            report.claimedRuntimeSlotCountBeforeRun =
                driver.World.ClaimedRuntimeSlotCountForDiagnostics;
            if (!TrySelectLoadedCharacter(manager, dataManager, out selectedCharacterOid))
            {
                throw new InvalidOperationException(
                    "No loaded type-0 character with DAT frames and visible sprites is available.");
            }

            previousSettings = CloneSettings(driver.Settings);
            previousPaused = driver.IsPaused;
            objectPoolActiveBaseline = objectPool.ActiveObjectCountForAcceptance;
            objectPoolAvailableBaseline = objectPool.AvailableObjectCountForAcceptance;
            referencePoolActiveBaseline = referencePool.ActiveCount;
            previousSoundPresentationSuppressed =
                ApplySoundPresentationSuppressionForDiagnostics(
                    driver,
                    config,
                    report,
                    out dispatchedSoundEventCountBaseline,
                    out suppressedSoundEventCountBaseline);
            soundPresentationSuppressionStateCaptured = true;

            var settings = new BattleRuntimeWorldSettings(
                BattleRuntimeProfile.MobileExtended,
                BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity,
                BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities,
                CollisionBroadphaseBackend.LooseQuadtree);
            driverConfigurationChanged = true;
            if (!driver.TryConfigureEmptyDiagnosticWorld(
                    settings,
                    config.AiExecutionProfile,
                    out string failureReason))
                throw new InvalidOperationException(failureReason);

            world = driver.World;
            if (world.RuntimeProfileForDiagnostics != BattleRuntimeProfile.MobileExtended ||
                world.RuntimeSlotCapacityForDiagnostics != BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity ||
                world.CollisionBroadphaseForDiagnostics != CollisionBroadphaseBackend.LooseQuadtree)
            {
                throw new InvalidOperationException(
                    "Diagnostic world did not apply MobileExtended(1050) + LooseQuadtree before registration.");
            }
            previousSkipLateRendererUpdate = ApplySkipLateRendererUpdateForDiagnostics(
                world,
                config,
                report,
                out skipLateRendererUpdateTickCountBaseline);
            world.LateRuntimeSnapshotModeForDiagnostics = config.LateRuntimeSnapshotMode;
            report.lateRuntimeSnapshotEffectiveMode =
                ProductionEntityStressConfig.FormatLateRuntimeSnapshotMode(
                    world.LateRuntimeSnapshotModeForDiagnostics);
            ApplyAiSensingConfigurationForDiagnostics(world, config, report);
            previousAiDecisionShadowMode = world.AiDecisionShadowMode;
            previousAiDecisionExecutionMode = world.AiDecisionExecutionMode;
            previousAiDecisionFullOracleSampleInterval =
                world.AiDecisionIndexedCanonicalFullOracleSampleInterval;
            previousUnifiedAiSnapshotShadowMode = world.AiUnifiedSnapshotShadowMode;
            previousAiUnifiedSnapshotExecutionMode =
                world.AiUnifiedSnapshotExecutionMode;
            world.AiUnifiedSnapshotExecutionMode =
                AiUnifiedSnapshotExecutionMode.LegacySeparate;
            world.AiDecisionShadowMode = config.RequestedAiDecisionShadowMode;
            world.AiDecisionExecutionMode = config.AiDecisionExecutionMode;
            world.AiDecisionIndexedCanonicalFullOracleSampleInterval =
                config.AiDecisionFullOracleSampleInterval;
            world.AiUnifiedSnapshotShadowMode = config.EnableUnifiedAiSnapshotShadow
                ? AiUnifiedSnapshotShadowMode.Shadow
                : AiUnifiedSnapshotShadowMode.Disabled;
            world.AiUnifiedSnapshotExecutionMode =
                config.AiUnifiedSnapshotExecutionMode;
            world.ResetAiDecisionShadowDiagnostics();
            world.ResetAiUnifiedSnapshotShadowDiagnostics();
            world.ResetAiUnifiedSnapshotExecutionDiagnostics();
            report.aiDecisionExecutionEffectiveMode =
                world.AiDecisionExecutionMode.ToString();
            report.aiDecisionSoAShadowApplied =
                !config.EnableAiDecisionSoAShadow ||
                world.AiDecisionShadowMode == AiDecisionShadowMode.Shadow;
            report.aiDecisionSharedShadowApplied =
                !config.EnableAiDecisionSharedShadow ||
                world.AiDecisionShadowMode == AiDecisionShadowMode.SharedShadow;
            report.unifiedAiSnapshotShadowApplied =
                !config.EnableUnifiedAiSnapshotShadow ||
                world.AiUnifiedSnapshotShadowMode == AiUnifiedSnapshotShadowMode.Shadow;
            report.aiUnifiedSnapshotExecutionEffectiveMode =
                world.AiUnifiedSnapshotExecutionMode.ToString();
            previousAiSoADecisionRemainderEnabled =
                ApplyAiSoADecisionRemainderForDiagnostics(
                    world,
                    config,
                    report);
            report.rosterFingerprint = ProductionEntityStressFingerprint.BuildRoster(
                config.Mode,
                config.EntityCount,
                selectedCharacterOid);
            BruteForceSceneQuery stressSceneQuery =
                ApplyFormalCollectorModeForDiagnostics(world, config.FormalCollectorMode);
            ApplyRoleAwareBroadphaseDiagnosticsForDiagnostics(
                stressSceneQuery,
                config,
                report);
            collisionCandidateStoreShadowQuery = stressSceneQuery;
            previousCollisionCandidateStoreShadowEnabled =
                ApplyCollisionCandidateStoreShadowForDiagnostics(
                    stressSceneQuery,
                    config,
                    report);
            previousCollisionCandidateStoreAuthorityEnabled =
                ApplyCollisionCandidateStoreAuthorityForDiagnostics(
                    stressSceneQuery,
                    config,
                    report,
                    out collisionCandidateStoreAuthorityDiagnosticsBaseline,
                    out previousCollisionCandidateStoreLegacyOracleInterval);
            previousCollisionRoleZeroItrFastPathEnabled =
                ApplyCollisionRoleZeroItrFastPathForDiagnostics(
                    stressSceneQuery,
                    config,
                    report);
            report.formalCollectorRequestedMode =
                ProductionEntityStressConfig.FormatFormalCollectorMode(config.FormalCollectorMode);
            report.formalCollectorMode =
                ProductionEntityStressConfig.FormatFormalCollectorMode(
                    ResolveAppliedFormalCollectorModeForDiagnostics(
                        world,
                        stressSceneQuery));
            report.formalCollectorBodyEntries =
                stressSceneQuery.LastRoleAwareBodyEntryCountForDiagnostics;
            report.formalCollectorItrQueries =
                stressSceneQuery.LastRoleAwareItrQueryCountForDiagnostics;
            ProductionEntityStressPhaseTimingLifecycle.Disable(world);
            if (config.EnablePhaseTiming)
            {
                phaseTimingDiagnostics = world.EnableBattleTickPhaseDiagnosticsForDiagnostics();
                phaseTimingCollector = new ProductionEntityStressPhaseTimingCollector();
            }
            else
            {
                ProductionEntityStressPhaseTimingCollector.PopulateDisabledReport(report);
            }
            if (config.EnablePresentationTiming)
            {
                presentationTimingDiagnostics =
                    world.EnableBattlePresentationPhaseDiagnosticsForDiagnostics();
                presentationTimingCollector =
                    new ProductionEntityStressPresentationTimingCollector();
            }
            else
            {
                ProductionEntityStressPresentationTimingCollector.PopulateDisabledReport(report);
            }
            if (config.EnableDetailPhaseTiming)
            {
                detailPhaseTimingDiagnostics =
                    world.EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
                aiInputDetailDiagnostics =
                    world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            }
            driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                useUnscaledTime = true,
                maxCatchUpTicksPerFrame = config.MaxCatchUpTicksPerFrame,
                maxBacklogTicks = config.MaxBacklogTicks,
                inputDelayTicks = 0,
                requireInputFrameReady = false,
                enableFrameChecksum = false,
            });
            driver.SetPaused(true);
            configured = true;
            report.selectedCharacterOid = selectedCharacterOid;
            RefreshReportCounts();
            WriteReport();
        }

        private void Update()
        {
            FinalizeProfilerFrameGcEvidence(frameBoundaryCompleted: true);
            long updateTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope updateProfilerScope =
                RunnerUpdateProfilerMarker.Auto();
            try
            {
                if (!configured || cleaned)
                    return;

                try
                {
                    if (reportWriteDeferredForProfilerFrame)
                    {
                        reportWriteDeferredForProfilerFrame = false;
                        WriteReport();
                    }
                    double frameMs = Time.unscaledDeltaTime * 1000d;
                    if (!initialPopulationComplete)
                    {
                        SpawnBatch(config.SpawnBatchSize);
                        if (CountActiveBaseRoster(out _) == config.EntityCount)
                        {
                            initialPopulationComplete = true;
                            accumulator = 0f;
                            report.status = "Running";
                            report.replenishmentState = "Stable";
                            ValidatePeakPopulation();
                            WriteReport();
                        }
                        return;
                    }

                    int removedCount = RemoveReleasedEntities();
                    if (removedCount > 0)
                    {
                        report.rosterRemovalCount += removedCount;
                        rosterMutationPendingForNextTick = true;
                    }

                    RefreshRosterAndCapacityDiagnostics();
                    ProductionEntityStressReplenishmentAction replenishmentAction =
                        ProductionEntityStressReplenishmentPolicy.Evaluate(
                            report.baseRosterActiveCount,
                            config.EntityCount,
                            report.totalActiveRuntimeEntityCount,
                            report.totalClaimedRuntimeSlotCount,
                            BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities,
                            report.currentSaturationDrainTicks,
                            config.MaxSaturationDrainTicks);
                    switch (replenishmentAction)
                    {
                        case ProductionEntityStressReplenishmentAction.Attempt:
                        {
                            report.replenishmentState = "Replenishing";
                            report.replenishAttemptCount++;
                            int replenishedCount = SpawnBatch(
                                Math.Min(
                                    config.SpawnBatchSize,
                                    config.EntityCount - report.baseRosterActiveCount),
                                deferWhenSaturated: true);
                            report.replenishedEntityCount += replenishedCount;
                            rosterMutationPendingForNextTick |= replenishedCount > 0;
                            if (CountActiveBaseRoster(out _) == config.EntityCount)
                            {
                                report.currentSaturationDrainTicks = 0;
                                report.replenishmentState = "Stable";
                            }
                            return;
                        }
                        case ProductionEntityStressReplenishmentAction.Drain:
                            saturationDrainActive = true;
                            report.replenishmentState = "SaturationDrain";
                            report.replenishDeferredCount++;
                            break;
                        case ProductionEntityStressReplenishmentAction
                            .SaturationBlockedReplenishment:
                            saturationBlocked = true;
                            break;
                        default:
                            saturationDrainActive = false;
                            report.currentSaturationDrainTicks = 0;
                            report.replenishmentState = "Stable";
                            break;
                    }

                    if (saturationBlocked)
                    {
                        StopForSaturationBlockedReplenishment();
                        return;
                    }

                    if (!saturationDrainActive)
                    {
                        AddRollingSample(frameSamples, frameMs);
                        report.sampledUnityFrames++;
                    }
                    accumulator += Time.unscaledDeltaTime;
                    float maximumAccumulator =
                        SimulationConstants.SIM_DT * config.MaxBacklogTicks;
                    if (accumulator > maximumAccumulator)
                    {
                        report.droppedBacklogTicks += Mathf.FloorToInt(
                            (accumulator - maximumAccumulator) /
                            SimulationConstants.SIM_DT);
                        accumulator = maximumAccumulator;
                    }

                    int ticksThisFrame = 0;
                    if (ProductionEntityStressProfilerFrameSamplePolicy.ShouldStart(
                            report.logicTicksExecuted,
                            config.WarmupTicks,
                            report.baseRosterActiveCount,
                            config.EntityCount,
                            rosterMutationPendingForNextTick,
                            saturationDrainActive,
                            accumulator,
                            report.sampledLogicTicks,
                            config.SampleTicks,
                            config.ShouldAutoStopWhenSampled))
                    {
                        profilerFrameGcCollector?.StartCandidate(
                            report.logicTicksExecuted,
                            report.sampledLogicTicks,
                            report.nonSteadyLogicTicks);
                    }
                    while (accumulator >= SimulationConstants.SIM_DT &&
                           ticksThisFrame < config.MaxCatchUpTicksPerFrame &&
                           ShouldExecuteCatchUpTickForStressSample(
                               config.ShouldAutoStopWhenSampled,
                               report.sampledLogicTicks,
                               config.SampleTicks))
                    {
                        accumulator -= SimulationConstants.SIM_DT;
                        bool buildPresentation = ResolveBuildPresentationForStressTick(
                            config.SimulationOnly,
                            accumulator,
                            ticksThisFrame,
                            config.MaxCatchUpTicksPerFrame);
                        StepMeasuredTick(
                            buildPresentation,
                            rosterMutationPendingForNextTick);
                        rosterMutationPendingForNextTick = false;
                        ticksThisFrame++;
                        if (saturationDrainActive)
                        {
                            report.currentSaturationDrainTicks++;
                            report.saturationDrainTickCount++;
                            RefreshRosterAndCapacityDiagnostics();
                            ProductionEntityStressReplenishmentAction drainAction =
                                ProductionEntityStressReplenishmentPolicy.Evaluate(
                                    report.baseRosterActiveCount,
                                    config.EntityCount,
                                    report.totalActiveRuntimeEntityCount,
                                    report.totalClaimedRuntimeSlotCount,
                                    BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities,
                                    report.currentSaturationDrainTicks,
                                    config.MaxSaturationDrainTicks);
                            if (drainAction ==
                                ProductionEntityStressReplenishmentAction.Attempt)
                            {
                                saturationDrainActive = false;
                                report.replenishmentState = "ReplenishmentPending";
                                break;
                            }
                            if (drainAction == ProductionEntityStressReplenishmentAction
                                    .SaturationBlockedReplenishment)
                            {
                                saturationBlocked = true;
                                break;
                            }
                        }
                    }

                    if (ticksThisFrame > 1)
                        report.framesWithCatchUp++;
                    report.maximumCatchUpTicksInFrame = Math.Max(
                        report.maximumCatchUpTicksInFrame,
                        ticksThisFrame);
                    report.currentBacklogTicks = Mathf.FloorToInt(
                        accumulator / SimulationConstants.SIM_DT);
                    report.maximumBacklogTicks = Math.Max(
                        report.maximumBacklogTicks,
                        report.currentBacklogTicks);

                    frameCounter++;
                    if (frameCounter % 30 == 0)
                    {
                        if (profilerFrameGcCollector?.Active == true)
                            reportWriteDeferredForProfilerFrame = true;
                        else
                            WriteReport();
                    }

                    if (saturationBlocked)
                    {
                        StopForSaturationBlockedReplenishment();
                        return;
                    }

                    if (config.ShouldAutoStopWhenSampled &&
                        report.sampledLogicTicks >= config.SampleTicks)
                    {
                        StopAndCleanup(config.AutoCleanup
                            ? "smoke-complete"
                            : "sample-complete");
                    }
                }
                catch (Exception exception)
                {
                    FinalizeProfilerFrameGcEvidence(frameBoundaryCompleted: false);
                    report.status = "Failed";
                    report.failure = exception.ToString();
                    try
                    {
                        CleanupInternal("exception", true);
                        terminalCpuDiagnosticsFlushPending = true;
                        ProductionEntityStressPaths.WriteTerminalResult(
                            false,
                            config.OutputPath,
                            exception.Message);
                        Debug.LogError($"[ProductionEntityStress] Failed: {exception}");
                    }
                    finally
                    {
                        ProductionEntityStressRequestProcessor.NotifyRunStopped();
                        Destroy(gameObject);
                    }
                }
            }
            finally
            {
                updateProfilerScope.Dispose();
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion.RunnerUpdateTotal,
                    Stopwatch.GetTimestamp() - updateTimestamp);
                if (terminalCpuDiagnosticsFlushPending)
                {
                    terminalCpuDiagnosticsFlushPending = false;
                    WriteReport();
                }
            }
        }

        internal void StopAndCleanup(
            string reason,
            bool preserveRequestProcessorState = false)
        {
            FinalizeProfilerFrameGcEvidence(frameBoundaryCompleted: false);
            preserveRequestProcessorStateOnDestroy |= preserveRequestProcessorState;
            if (cleaned)
            {
                if (!preserveRequestProcessorStateOnDestroy)
                    ProductionEntityStressRequestProcessor.NotifyRunStopped();
                return;
            }

            try
            {
                bool requestedSamplingCompleted = config.ShouldAutoStopWhenSampled &&
                                                  report.sampledLogicTicks >= config.SampleTicks &&
                                                  string.IsNullOrEmpty(report.failure);
                CleanupInternal(reason, true);
                bool success = report.harnessValidity && report.teardown.restored &&
                               (!config.ShouldAutoStopWhenSampled || requestedSamplingCompleted);
                report.status = config.AutoCleanup
                    ? (success ? "SmokePassed" : "SmokeFailed")
                    : (success ? "StoppedCleanly" : "StoppedWithResidue");
                WriteReport();
                terminalCpuDiagnosticsFlushPending = true;
                ProductionEntityStressPaths.WriteTerminalResult(
                    success,
                    config.OutputPath,
                    report.teardown.evidence);
                Debug.Log($"[ProductionEntityStress] {report.status}: {config.OutputPath}");
            }
            finally
            {
                if (!preserveRequestProcessorStateOnDestroy)
                    ProductionEntityStressRequestProcessor.NotifyRunStopped();
                Destroy(gameObject);
            }
        }

        private void StopForSaturationBlockedReplenishment()
        {
            FinalizeProfilerFrameGcEvidence(frameBoundaryCompleted: false);
            string result =
                ProductionEntityStressReplenishmentPolicy.SaturationBlockedResult;
            report.status = result;
            report.replenishmentState = result;
            report.failure = result;
            report.harnessValidity = false;
            RefreshRosterAndCapacityDiagnostics();
            report.saturationBlockedBaseRosterActiveCount =
                report.baseRosterActiveCount;
            report.saturationBlockedBaseAiActiveCount = report.baseAiActiveCount;
            report.saturationBlockedDerivedOrTemporaryActiveCount =
                report.derivedOrTemporaryActiveCount;
            report.saturationBlockedTotalActiveRuntimeEntityCount =
                report.totalActiveRuntimeEntityCount;
            report.saturationBlockedTotalClaimedRuntimeSlotCount =
                report.totalClaimedRuntimeSlotCount;
            try
            {
                CleanupInternal("saturation-blocked-replenishment", true);
                report.status = result;
                report.replenishmentState = result;
                report.harnessValidity = false;
                WriteReport();
                terminalCpuDiagnosticsFlushPending = true;
                ProductionEntityStressPaths.WriteTerminalResult(
                    false,
                    config.OutputPath,
                    result);
                Debug.LogError($"[ProductionEntityStress] {result}: {config.OutputPath}");
            }
            finally
            {
                ProductionEntityStressRequestProcessor.NotifyRunStopped();
                Destroy(gameObject);
            }
        }

        private void StepMeasuredTick(
            bool buildPresentation = true,
            bool rosterMutatedDuringTick = false)
        {
            long stepMeasuredTickTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope stepMeasuredTickProfilerScope =
                StepMeasuredTickProfilerMarker.Auto();
            try
            {
                long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                int poolCapacityBeforeTick = GetObjectPoolCapacityForDiagnostics();
                ProfilerMarker.AutoScope driverStepOneTickProfilerScope =
                    DriverStepOneTickProfilerMarker.Auto();
                long driverStepOneTickTimestamp = Stopwatch.GetTimestamp();
                bool stepped;
                long driverStepOneTickElapsedTicks;
                try
                {
                    stepped = driver.StepOneTick(
                        ignorePaused: true,
                        buildPresentation: buildPresentation);
                }
                finally
                {
                    driverStepOneTickElapsedTicks =
                        Stopwatch.GetTimestamp() - driverStepOneTickTimestamp;
                    driverStepOneTickProfilerScope.Dispose();
                    RecordCpuElapsedTicksForReport(
                        report,
                        ProductionEntityStressCpuRegion.DriverStepOneTick,
                        driverStepOneTickElapsedTicks);
                }
                double elapsedMs =
                    driverStepOneTickElapsedTicks * 1000d / Stopwatch.Frequency;
                long allocatedBytes =
                    GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
                if (!stepped)
                {
                    throw new InvalidOperationException(
                        "Production SimulationTickDriver refused a manual tick.");
                }

                report.logicTicksExecuted++;
                int baseRosterActiveCount = CountActiveBaseRoster(out int baseAiActiveCount);
                report.baseRosterActiveCount = baseRosterActiveCount;
                report.baseAiActiveCount = baseAiActiveCount;
                bool poolExpandedDuringTick =
                    GetObjectPoolCapacityForDiagnostics() > poolCapacityBeforeTick;
                bool isSteadyStateSample =
                    ProductionEntityStressSamplePolicy.IsSteadyStateSample(
                        report.logicTicksExecuted,
                        config.WarmupTicks,
                        baseRosterActiveCount,
                        config.EntityCount,
                        rosterMutatedDuringTick,
                        poolExpandedDuringTick);
                long postTickTimingCollectorsAllocatedBefore =
                    GC.GetAllocatedBytesForCurrentThread();
                long postTickTimingCollectorsTimestamp = Stopwatch.GetTimestamp();
                ProfilerMarker.AutoScope postTickTimingCollectorsProfilerScope =
                    PostTickTimingCollectorsProfilerMarker.Auto();
                try
                {
                    if (isSteadyStateSample)
                    {
                        phaseTimingCollector?.CaptureAfterTick(
                            phaseTimingDiagnostics,
                            elapsedMs,
                            report.logicTicksExecuted,
                            config.WarmupTicks);
                        presentationTimingCollector?.CaptureAfterTick(
                            presentationTimingDiagnostics,
                            report.logicTicksExecuted,
                            config.WarmupTicks);
                        if (config.EnableDetailPhaseTiming)
                        {
                            detailPhaseTimingCollector.CaptureAfterTick(
                                detailPhaseTimingDiagnostics,
                                aiInputDetailDiagnostics,
                                report.logicTicksExecuted,
                                config.WarmupTicks);
                        }
                    }
                }
                finally
                {
                    postTickTimingCollectorsProfilerScope.Dispose();
                    RecordAllocationBytesForReport(
                        report,
                        ProductionEntityStressAllocationRegion
                            .PostTickTimingCollectors,
                        GC.GetAllocatedBytesForCurrentThread() -
                        postTickTimingCollectorsAllocatedBefore);
                    RecordCpuElapsedTicksForReport(
                        report,
                        ProductionEntityStressCpuRegion.PostTickTimingCollectors,
                        Stopwatch.GetTimestamp() -
                        postTickTimingCollectorsTimestamp);
                }
                if (report.logicTicksExecuted <= config.WarmupTicks)
                {
                    report.warmupTicksCompleted = report.logicTicksExecuted;
                }
                else if (isSteadyStateSample)
                {
                    logicTickTimingCollector.AddSample(elapsedMs, buildPresentation);
                    AddRollingSample(allocationSamples, allocatedBytes);
                    report.sampledLogicTicks++;
                }
                else
                {
                    report.nonSteadyLogicTicks++;
                    if (baseRosterActiveCount != config.EntityCount)
                        report.sampleRejectedForIncompleteBaseRoster++;
                    if (rosterMutatedDuringTick)
                        report.sampleRejectedForRosterMutation++;
                    if (poolExpandedDuringTick)
                        report.sampleRejectedForPoolExpansion++;
                }

                long captureProductionCountersAllocatedBefore =
                    GC.GetAllocatedBytesForCurrentThread();
                long captureProductionCountersTimestamp = Stopwatch.GetTimestamp();
                ProfilerMarker.AutoScope captureProductionCountersProfilerScope =
                    CaptureProductionCountersProfilerMarker.Auto();
                try
                {
                    CaptureProductionCounters();
                }
                finally
                {
                    captureProductionCountersProfilerScope.Dispose();
                    RecordAllocationBytesForReport(
                        report,
                        ProductionEntityStressAllocationRegion
                            .CaptureProductionCountersTotal,
                        GC.GetAllocatedBytesForCurrentThread() -
                        captureProductionCountersAllocatedBefore);
                    RecordCpuElapsedTicksForReport(
                        report,
                        ProductionEntityStressCpuRegion
                            .CaptureProductionCountersTotal,
                        Stopwatch.GetTimestamp() -
                        captureProductionCountersTimestamp);
                }
            }
            finally
            {
                stepMeasuredTickProfilerScope.Dispose();
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion.StepMeasuredTickTotal,
                    Stopwatch.GetTimestamp() - stepMeasuredTickTimestamp);
            }
        }

        private void FinalizeProfilerFrameGcEvidence(bool frameBoundaryCompleted)
        {
            profilerFrameGcCollector?.StopAndCollect(
                report?.logicTicksExecuted ?? 0,
                report?.sampledLogicTicks ?? 0,
                report?.nonSteadyLogicTicks ?? 0,
                frameBoundaryCompleted);
        }

        private void CaptureProductionCounters()
        {
            long activeEntityScanAllocatedBefore =
                GC.GetAllocatedBytesForCurrentThread();
            long activeEntityScanTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope activeEntityScanProfilerScope =
                ActiveEntityScanProfilerMarker.Auto();
            try
            {
                long candidateCount = 0;
                int aiCount = 0;
                int candidateConsumerCount = 0;
                for (int i = 0; i < entities.Count; i++)
                {
                    LF2Character entity = entities[i];
                    if (!IsActive(entity))
                        continue;
                    candidateConsumerCount++;
                    candidateCount += Math.Max(0, entity.Runtime.HitCandidateCount);
                    if (entity.AiControlled)
                        aiCount++;
                }
                report.aiControlledEntityTicks += aiCount;
                report.collisionCandidateConsumerEntityTicks += candidateConsumerCount;
                report.collisionCandidateCountSum += candidateCount;
                report.collisionCandidateCountPeak = Math.Max(
                    report.collisionCandidateCountPeak,
                    candidateCount > int.MaxValue
                        ? int.MaxValue
                        : (int)candidateCount);
            }
            finally
            {
                activeEntityScanProfilerScope.Dispose();
                RecordAllocationBytesForReport(
                    report,
                    ProductionEntityStressAllocationRegion
                        .CaptureProductionCountersActiveEntityScan,
                    GC.GetAllocatedBytesForCurrentThread() -
                    activeEntityScanAllocatedBefore);
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion
                        .CaptureProductionCountersActiveEntityScan,
                    Stopwatch.GetTimestamp() - activeEntityScanTimestamp);
            }

            long sceneQueryDiagnosticsAllocatedBefore =
                GC.GetAllocatedBytesForCurrentThread();
            long sceneQueryDiagnosticsTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope sceneQueryDiagnosticsProfilerScope =
                SceneQueryDiagnosticsProfilerMarker.Auto();
            try
            {
                if (world.SceneQuery is BruteForceSceneQuery sceneQuery)
                {
                    RecordExpectedCollisionCandidateStoreCadenceForDiagnostics(
                        report,
                        config.LegacyOracleInterval,
                        world.CurrentTickIndex);
                    RecordExpectedCollisionRoleZeroItrFastPathForDiagnostics(report);
                    report.formalCollectorMode =
                        ProductionEntityStressConfig.FormatFormalCollectorMode(
                            sceneQuery.LastFormalCollectorModeForDiagnostics);
                    report.formalCollectorBodyEntries =
                        sceneQuery.LastRoleAwareBodyEntryCountForDiagnostics;
                    report.formalCollectorItrQueries =
                        sceneQuery.LastRoleAwareItrQueryCountForDiagnostics;
                    CaptureRoleAwareBroadphaseDiagnosticsForReport(report, sceneQuery);
                    CaptureCollisionCandidateStoreShadowDiagnosticsForReport(
                        report,
                        sceneQuery);
                    EvaluateCollisionCandidateStoreShadowValidityForReport(
                        report,
                        CollisionCandidateStoreValidationPhase.Final);
                    CaptureCollisionCandidateStoreAuthorityDiagnosticsForReport(
                        report,
                        sceneQuery,
                        in collisionCandidateStoreAuthorityDiagnosticsBaseline);
                    EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                        report,
                        CollisionCandidateStoreValidationPhase.Final);
                    CaptureCollisionRoleZeroItrFastPathDiagnosticsForReport(
                        report,
                        sceneQuery);
                    EvaluateCollisionRoleZeroItrFastPathValidityForReport(
                        report,
                        CollisionCandidateStoreValidationPhase.Final);
                    AccumulateRoleAwareDirectCostForReport(
                        report,
                        sceneQuery.LastRoleAwareDirectCostForDiagnostics,
                        sceneQuery.LastRoleAwareDirectCostAvailableForDiagnostics);
                    int pairCount = sceneQuery.LastFormalPairCountForDiagnostics;
                    report.broadphasePairCountSum += pairCount;
                    report.broadphasePairCountPeak = Math.Max(
                        report.broadphasePairCountPeak,
                        pairCount);
                    report.broadphaseFallbackParticipantPeak = Math.Max(
                        report.broadphaseFallbackParticipantPeak,
                        sceneQuery.LastFormalFallbackParticipantCountForDiagnostics);
                    if (sceneQuery.LastFormalCollectionAbortedForDiagnostics)
                        report.broadphaseAbortedTicks++;
                    SpatialSynchronizeResult sync =
                        sceneQuery.LastFormalSynchronizeResultForDiagnostics;
                    report.broadphaseLastIndexedCount = sync.IndexedCount;
                }
            }
            finally
            {
                sceneQueryDiagnosticsProfilerScope.Dispose();
                RecordAllocationBytesForReport(
                    report,
                    ProductionEntityStressAllocationRegion
                        .CaptureProductionCountersSceneQueryDiagnostics,
                    GC.GetAllocatedBytesForCurrentThread() -
                    sceneQueryDiagnosticsAllocatedBefore);
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion
                        .CaptureProductionCountersSceneQueryDiagnostics,
                    Stopwatch.GetTimestamp() - sceneQueryDiagnosticsTimestamp);
            }

            long aiReportDiagnosticsAllocatedBefore =
                GC.GetAllocatedBytesForCurrentThread();
            long aiReportDiagnosticsTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope aiReportDiagnosticsProfilerScope =
                AiReportDiagnosticsProfilerMarker.Auto();
            try
            {
                AggregateAiSoADecisionRemainderDiagnosticsForReport(
                    report,
                    world.AiSoADecisionRemainderEligibleAttemptCountForDiagnostics,
                    world.AiSoADecisionRemainderAppliedCountForDiagnostics,
                    world.AiSoADecisionRemainderFallbackCountForDiagnostics,
                    world.AiSoADecisionRemainderPreRandomFailureCountForDiagnostics,
                    world.AiSoADecisionRemainderPostRandomFailureCountForDiagnostics,
                    world.AiSoADecisionRemainderHardFailureCountForDiagnostics,
                    world.AiSoADecisionRemainderContextBindCountForDiagnostics,
                    world.AiSoADecisionRemainderGatewayValidationCountForDiagnostics,
                    world.AiSoADecisionRemainderRowVisitCountForDiagnostics);
                CaptureAiDecisionSoAShadowDiagnosticsForReport(report, world);
                report.damageStatTotal = Sum(world.DamageStats);
                report.killStatTotal = Sum(world.KillStats);
            }
            finally
            {
                aiReportDiagnosticsProfilerScope.Dispose();
                RecordAllocationBytesForReport(
                    report,
                    ProductionEntityStressAllocationRegion
                        .CaptureProductionCountersAiReportDiagnostics,
                    GC.GetAllocatedBytesForCurrentThread() -
                    aiReportDiagnosticsAllocatedBefore);
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion
                        .CaptureProductionCountersAiReportDiagnostics,
                    Stopwatch.GetTimestamp() - aiReportDiagnosticsTimestamp);
            }

            long observeRuntimeEntitySnapshotAllocatedBefore =
                GC.GetAllocatedBytesForCurrentThread();
            long observeRuntimeEntitySnapshotTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope observeRuntimeEntitySnapshotProfilerScope =
                ObserveRuntimeEntitySnapshotProfilerMarker.Auto();
            try
            {
                ObserveRuntimeEntitySnapshot();
            }
            finally
            {
                observeRuntimeEntitySnapshotProfilerScope.Dispose();
                RecordAllocationBytesForReport(
                    report,
                    ProductionEntityStressAllocationRegion
                        .CaptureProductionCountersObserveRuntimeEntitySnapshot,
                    GC.GetAllocatedBytesForCurrentThread() -
                    observeRuntimeEntitySnapshotAllocatedBefore);
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion
                        .CaptureProductionCountersObserveRuntimeEntitySnapshot,
                    Stopwatch.GetTimestamp() -
                    observeRuntimeEntitySnapshotTimestamp);
            }
        }

        internal static BruteForceSceneQuery ApplyFormalCollectorModeForDiagnostics(
            SimulationWorld targetWorld,
            CollisionFormalCollectorMode mode)
        {
            if (targetWorld?.SceneQuery is not BruteForceSceneQuery sceneQuery)
            {
                throw new InvalidOperationException(
                    "Production stress requires BruteForceSceneQuery diagnostics.");
            }

            sceneQuery.FormalCollectorMode = mode;
            return sceneQuery;
        }

        internal static void ApplyRoleAwareBroadphaseDiagnosticsForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport)
        {
            if (sceneQuery == null)
                throw new ArgumentNullException(nameof(sceneQuery));

            sceneQuery.ForceRoleAwareDirectForDiagnostics = runConfig.ForceRoleAwareDirect;
            sceneQuery.ForceRoleAwareTreeForDiagnostics = runConfig.ForceRoleAwareTree;
            sceneQuery.ForceRoleAwareNestedDirectForDiagnostics =
                runConfig.ForceRoleAwareNestedDirect;
            sceneQuery.ForceRoleAwareSweepDirectForDiagnostics =
                runConfig.ForceRoleAwareSweepDirect;
            if (targetReport == null)
                return;

            targetReport.forceRoleAwareDirectRequested = runConfig.ForceRoleAwareDirect;
            targetReport.forceRoleAwareTreeRequested = runConfig.ForceRoleAwareTree;
            targetReport.forceRoleAwareNestedDirectRequested =
                runConfig.ForceRoleAwareNestedDirect;
            targetReport.forceRoleAwareSweepDirectRequested =
                runConfig.ForceRoleAwareSweepDirect;
            CaptureRoleAwareBroadphaseDiagnosticsForReport(targetReport, sceneQuery);
        }

        internal static bool ApplyCollisionCandidateStoreShadowForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport)
        {
            if (sceneQuery == null)
                throw new ArgumentNullException(nameof(sceneQuery));

            bool previousEnabled = sceneQuery.CollisionCandidateStoreShadowDiagnosticsEnabled;
            sceneQuery.CollisionCandidateStoreShadowDiagnosticsEnabled =
                runConfig.EnableCollisionCandidateStoreShadow ||
                runConfig.EnableCollisionCandidateStoreAuthority;
            if (targetReport != null)
            {
                targetReport.collisionCandidateStoreShadowRequested =
                    runConfig.EnableCollisionCandidateStoreShadow;
                targetReport.collisionCandidateStoreShadowApplied =
                    runConfig.EnableCollisionCandidateStoreShadow &&
                    sceneQuery.CollisionCandidateStoreShadowDiagnosticsEnabled;
                CaptureCollisionCandidateStoreShadowDiagnosticsForReport(
                    targetReport,
                    sceneQuery);
            }

            return previousEnabled;
        }

        internal static bool ApplyCollisionCandidateStoreAuthorityForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport,
            out CollisionCandidateStoreAuthorityDiagnosticsSnapshot diagnosticsBaseline)
        {
            return ApplyCollisionCandidateStoreAuthorityForDiagnostics(
                sceneQuery,
                runConfig,
                targetReport,
                out diagnosticsBaseline,
                out _);
        }

        internal static bool ApplyCollisionCandidateStoreAuthorityForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport,
            out CollisionCandidateStoreAuthorityDiagnosticsSnapshot diagnosticsBaseline,
            out int previousLegacyOracleInterval)
        {
            if (sceneQuery == null)
                throw new ArgumentNullException(nameof(sceneQuery));

            diagnosticsBaseline =
                CollisionCandidateStoreAuthorityDiagnosticsSnapshot.Capture(sceneQuery);
            bool previousEnabled = sceneQuery.CollisionCandidateStoreAuthorityEnabled;
            previousLegacyOracleInterval =
                sceneQuery.CollisionCandidateStoreLegacyOracleInterval;
            sceneQuery.CollisionCandidateStoreAuthorityEnabled =
                runConfig.EnableCollisionCandidateStoreAuthority;
            sceneQuery.CollisionCandidateStoreLegacyOracleInterval =
                runConfig.LegacyOracleInterval;
            if (targetReport != null)
            {
                targetReport.collisionCandidateStoreAuthorityRequested =
                    runConfig.EnableCollisionCandidateStoreAuthority;
                targetReport.collisionCandidateStoreAuthorityConfigured =
                    runConfig.EnableCollisionCandidateStoreAuthority &&
                    sceneQuery.CollisionCandidateStoreAuthorityEnabled &&
                    sceneQuery.CollisionCandidateStoreLegacyOracleInterval ==
                    runConfig.LegacyOracleInterval;
                targetReport.collisionCandidateStoreLegacyOracleInterval =
                    runConfig.LegacyOracleInterval;
                targetReport.collisionCandidateStoreAuthorityApplied = false;
                CaptureCollisionCandidateStoreAuthorityDiagnosticsForReport(
                    targetReport,
                    sceneQuery,
                    in diagnosticsBaseline);
            }

            return previousEnabled;
        }

        internal static void CaptureCollisionCandidateStoreAuthorityDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            BruteForceSceneQuery sceneQuery,
            in CollisionCandidateStoreAuthorityDiagnosticsSnapshot diagnosticsBaseline)
        {
            if (targetReport == null || sceneQuery == null)
                return;

            CollisionCandidateStoreAuthorityDiagnostics diagnostics =
                sceneQuery.CollisionCandidateStoreAuthorityDiagnostics;
            targetReport.collisionCandidateStoreAuthorityRequestedTickCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityRequestedTickCount,
                NonNegativeDelta(
                    diagnostics.RequestedTickCount,
                    diagnosticsBaseline.RequestedTickCount));
            targetReport.collisionCandidateStoreAuthorityAppliedTickCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityAppliedTickCount,
                NonNegativeDelta(
                    diagnostics.AppliedTickCount,
                    diagnosticsBaseline.AppliedTickCount));
            targetReport.collisionCandidateStoreAuthorityLegacyFallbackTickCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityLegacyFallbackTickCount,
                NonNegativeDelta(
                    diagnostics.LegacyFallbackTickCount,
                    diagnosticsBaseline.LegacyFallbackTickCount));
            targetReport.collisionCandidateStoreAuthoritySampledOracleTickCount = Math.Max(
                targetReport.collisionCandidateStoreAuthoritySampledOracleTickCount,
                NonNegativeDelta(
                    diagnostics.SampledOracleTickCount,
                    diagnosticsBaseline.SampledOracleTickCount));
            targetReport.collisionCandidateStoreAuthorityStoreOnlyTickCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityStoreOnlyTickCount,
                NonNegativeDelta(
                    diagnostics.StoreOnlyTickCount,
                    diagnosticsBaseline.StoreOnlyTickCount));
            targetReport.collisionCandidateStoreAuthorityLegacyListCreatedOrWrittenCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityLegacyListCreatedOrWrittenCount,
                NonNegativeDelta(
                    diagnostics.LegacyListCreatedOrWrittenCount,
                    diagnosticsBaseline.LegacyListCreatedOrWrittenCount));
            targetReport.collisionCandidateStoreAuthorityStoreOnlyHardFailureCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityStoreOnlyHardFailureCount,
                NonNegativeDelta(
                    diagnostics.StoreOnlyHardFailureCount,
                    diagnosticsBaseline.StoreOnlyHardFailureCount));
            targetReport.collisionCandidateStoreAuthorityRangeReadCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityRangeReadCount,
                NonNegativeDelta(
                    diagnostics.RangeReadCount,
                    diagnosticsBaseline.RangeReadCount));
            targetReport.collisionCandidateStoreAuthorityEntryReadCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityEntryReadCount,
                NonNegativeDelta(
                    diagnostics.EntryReadCount,
                    diagnosticsBaseline.EntryReadCount));
            targetReport.collisionCandidateStoreAuthorityFailureCount = Math.Max(
                targetReport.collisionCandidateStoreAuthorityFailureCount,
                NonNegativeDelta(
                    diagnostics.FailureCount,
                    diagnosticsBaseline.FailureCount));
            targetReport.collisionCandidateStoreAuthorityApplied =
                targetReport.collisionCandidateStoreAuthorityRequested &&
                targetReport.collisionCandidateStoreAuthorityAppliedTickCount > 0;
            targetReport.collisionCandidateStoreAuthorityFirstFailureReason =
                targetReport.collisionCandidateStoreAuthorityFailureCount > 0
                    ? diagnostics.FirstFailureReason.ToString()
                    : CollisionCandidateStoreAuthorityFailureReason.None.ToString();
        }

        internal static bool ApplyCollisionRoleZeroItrFastPathForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            bool previousEnabled = sceneQuery?.CollisionRoleZeroItrFastPathEnabled ?? false;
            targetReport.collisionRoleZeroItrFastPathRequested =
                runConfig.EnableCollisionRoleZeroItrFastPath;
            targetReport.collisionRoleZeroItrFastPathConfigured =
                runConfig.EnableCollisionRoleZeroItrFastPath &&
                sceneQuery != null;
            targetReport.collisionRoleZeroItrFastPathApplied = false;
            if (sceneQuery != null)
            {
                sceneQuery.SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(
                    runConfig.EnableCollisionRoleZeroItrFastPath);
                targetReport.collisionRoleZeroItrFastPathConfigured =
                    sceneQuery.CollisionRoleZeroItrFastPathEnabled ==
                    runConfig.EnableCollisionRoleZeroItrFastPath;
                CaptureCollisionRoleZeroItrFastPathDiagnosticsForReport(targetReport, sceneQuery);
            }

            return previousEnabled;
        }

        internal static void CaptureCollisionRoleZeroItrFastPathDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            BruteForceSceneQuery sceneQuery)
        {
            if (targetReport == null || sceneQuery == null)
                return;

            targetReport.collisionRoleZeroItrFastPathAppliedCount = Math.Max(
                targetReport.collisionRoleZeroItrFastPathAppliedCount,
                sceneQuery.CollisionRoleZeroItrFastPathAppliedCountForDiagnostics);
            targetReport.collisionRoleZeroItrFastPathFallbackCount = Math.Max(
                targetReport.collisionRoleZeroItrFastPathFallbackCount,
                sceneQuery.CollisionRoleZeroItrFastPathFallbackCountForDiagnostics);
            targetReport.collisionRoleZeroItrFastPathInvalidCount = Math.Max(
                targetReport.collisionRoleZeroItrFastPathInvalidCount,
                sceneQuery.CollisionRoleZeroItrFastPathInvalidCountForDiagnostics);
            targetReport.collisionRoleZeroItrFastPathZeroItrCount = Math.Max(
                targetReport.collisionRoleZeroItrFastPathZeroItrCount,
                sceneQuery.CollisionRoleZeroItrFastPathZeroItrCountForDiagnostics);
            targetReport.collisionRoleZeroItrFastPathTouchedHandleCount = Math.Max(
                targetReport.collisionRoleZeroItrFastPathTouchedHandleCount,
                sceneQuery.CollisionRoleZeroItrFastPathTouchedHandleCountForDiagnostics);
            targetReport.collisionRoleZeroItrFastPathApplied =
                targetReport.collisionRoleZeroItrFastPathRequested &&
                targetReport.collisionRoleZeroItrFastPathAppliedCount > 0;
        }

        internal static bool RestoreCollisionRoleZeroItrFastPathForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            bool previousEnabled,
            ProductionEntityStressReport targetReport)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            CaptureCollisionRoleZeroItrFastPathDiagnosticsForReport(targetReport, sceneQuery);
            if (sceneQuery == null)
            {
                targetReport.collisionRoleZeroItrFastPathRestored =
                    !targetReport.collisionRoleZeroItrFastPathRequested;
                return targetReport.collisionRoleZeroItrFastPathRestored;
            }

            sceneQuery.SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(previousEnabled);
            targetReport.collisionRoleZeroItrFastPathRestored =
                sceneQuery.CollisionRoleZeroItrFastPathEnabled == previousEnabled;
            return targetReport.collisionRoleZeroItrFastPathRestored;
        }

        internal static void RecordExpectedCollisionRoleZeroItrFastPathForDiagnostics(
            ProductionEntityStressReport targetReport)
        {
            if (targetReport?.collisionRoleZeroItrFastPathRequested == true)
                targetReport.collisionRoleZeroItrFastPathExpectedAppliedTickCount++;
        }

        internal static bool EvaluateCollisionRoleZeroItrFastPathValidityForReport(
            ProductionEntityStressReport targetReport,
            CollisionCandidateStoreValidationPhase phase)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));
            if (!targetReport.collisionRoleZeroItrFastPathRequested)
                return true;

            bool valid = targetReport.collisionRoleZeroItrFastPathConfigured;
            if (phase != CollisionCandidateStoreValidationPhase.PreTick)
            {
                valid = valid &&
                        targetReport.collisionRoleZeroItrFastPathApplied &&
                        targetReport.collisionRoleZeroItrFastPathAppliedCount ==
                        targetReport.collisionRoleZeroItrFastPathExpectedAppliedTickCount &&
                        targetReport.collisionRoleZeroItrFastPathFallbackCount == 0 &&
                        targetReport.collisionRoleZeroItrFastPathInvalidCount == 0 &&
                        targetReport.collisionRoleZeroItrFastPathZeroItrCount ==
                        targetReport.collisionRoleZeroItrFastPathExpectedAppliedTickCount;
            }
            if (phase == CollisionCandidateStoreValidationPhase.Teardown)
                valid = valid && targetReport.collisionRoleZeroItrFastPathRestored;
            if (!valid)
                targetReport.harnessValidity = false;
            return valid;
        }

        internal static bool RestoreCollisionCandidateStoreAuthorityForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            bool previousEnabled,
            ProductionEntityStressReport targetReport,
            in CollisionCandidateStoreAuthorityDiagnosticsSnapshot diagnosticsBaseline)
        {
            int previousLegacyOracleInterval = sceneQuery != null
                ? sceneQuery.CollisionCandidateStoreLegacyOracleInterval
                : 1;
            return RestoreCollisionCandidateStoreAuthorityForDiagnostics(
                sceneQuery,
                previousEnabled,
                previousLegacyOracleInterval,
                targetReport,
                in diagnosticsBaseline);
        }

        internal static bool RestoreCollisionCandidateStoreAuthorityForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            bool previousEnabled,
            int previousLegacyOracleInterval,
            ProductionEntityStressReport targetReport,
            in CollisionCandidateStoreAuthorityDiagnosticsSnapshot diagnosticsBaseline)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            CaptureCollisionCandidateStoreAuthorityDiagnosticsForReport(
                targetReport,
                sceneQuery,
                in diagnosticsBaseline);
            if (sceneQuery == null)
            {
                targetReport.collisionCandidateStoreAuthorityRestored =
                    !targetReport.collisionCandidateStoreAuthorityRequested;
                return targetReport.collisionCandidateStoreAuthorityRestored;
            }

            sceneQuery.CollisionCandidateStoreAuthorityEnabled = previousEnabled;
            sceneQuery.CollisionCandidateStoreLegacyOracleInterval =
                previousLegacyOracleInterval;
            targetReport.collisionCandidateStoreAuthorityRestored =
                sceneQuery.CollisionCandidateStoreAuthorityEnabled == previousEnabled &&
                sceneQuery.CollisionCandidateStoreLegacyOracleInterval ==
                previousLegacyOracleInterval;
            return targetReport.collisionCandidateStoreAuthorityRestored;
        }

        internal static bool EvaluateCollisionCandidateStoreAuthorityValidityForReport(
            ProductionEntityStressReport targetReport,
            CollisionCandidateStoreValidationPhase phase)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));
            if (!targetReport.collisionCandidateStoreAuthorityRequested)
                return true;

            bool valid = targetReport.collisionCandidateStoreAuthorityConfigured;
            if (phase != CollisionCandidateStoreValidationPhase.PreTick)
            {
                valid = valid &&
                        targetReport.collisionCandidateStoreAuthorityApplied &&
                        targetReport.collisionCandidateStoreAuthorityRequestedTickCount ==
                        targetReport.logicTicksExecuted &&
                        targetReport.collisionCandidateStoreAuthorityAppliedTickCount ==
                        targetReport.logicTicksExecuted &&
                        targetReport.collisionCandidateStoreAuthorityLegacyFallbackTickCount == 0 &&
                        targetReport.collisionCandidateStoreAuthorityFailureCount == 0 &&
                        targetReport.collisionCandidateStoreAuthorityStoreOnlyHardFailureCount == 0 &&
                        targetReport.collisionCandidateStoreShadowBuildTickCount ==
                        targetReport.logicTicksExecuted &&
                        targetReport.collisionCandidateStoreAuthoritySampledOracleTickCount +
                        targetReport.collisionCandidateStoreAuthorityStoreOnlyTickCount ==
                        targetReport.logicTicksExecuted &&
                        targetReport.collisionCandidateStoreAuthoritySampledOracleTickCount ==
                        targetReport.collisionCandidateStoreAuthorityExpectedSampledOracleTickCount &&
                        targetReport.collisionCandidateStoreAuthorityStoreOnlyTickCount ==
                        targetReport.collisionCandidateStoreAuthorityExpectedStoreOnlyTickCount &&
                        targetReport.collisionCandidateStoreAuthorityRangeReadCount ==
                        targetReport.collisionCandidateConsumerEntityTicks &&
                        targetReport.collisionCandidateStoreAuthorityEntryReadCount ==
                        targetReport.collisionCandidateCountSum &&
                        (targetReport.collisionCandidateStoreLegacyOracleInterval != 0 ||
                         targetReport.collisionCandidateStoreAuthorityLegacyListCreatedOrWrittenCount == 0);
            }
            if (phase == CollisionCandidateStoreValidationPhase.Teardown)
                valid = valid && targetReport.collisionCandidateStoreAuthorityRestored;
            if (valid)
                return true;

            targetReport.harnessValidity = false;
            string failureReason = BuildCollisionCandidateStoreAuthorityFailureReason(
                targetReport,
                phase);
            if (string.IsNullOrEmpty(targetReport.failure))
            {
                targetReport.failure = failureReason;
            }
            else if (targetReport.failure.IndexOf(
                         CollisionCandidateStoreAuthorityFailurePrefix,
                         StringComparison.Ordinal) < 0)
            {
                targetReport.failure += Environment.NewLine + failureReason;
            }
            return false;
        }

        internal static void RecordExpectedCollisionCandidateStoreCadenceForDiagnostics(
            ProductionEntityStressReport targetReport,
            int legacyOracleInterval,
            int frozenTickIndex)
        {
            if (targetReport == null ||
                !targetReport.collisionCandidateStoreAuthorityRequested)
            {
                return;
            }

            if (BruteForceSceneQuery.IsCollisionCandidateLegacyOracleSampleTick(
                    frozenTickIndex,
                    legacyOracleInterval))
            {
                targetReport
                    .collisionCandidateStoreAuthorityExpectedSampledOracleTickCount++;
            }
            else
            {
                targetReport
                    .collisionCandidateStoreAuthorityExpectedStoreOnlyTickCount++;
            }
        }

        internal static string BuildCollisionCandidateStoreAuthorityFailureReason(
            ProductionEntityStressReport targetReport,
            CollisionCandidateStoreValidationPhase phase)
        {
            if (targetReport == null ||
                !targetReport.collisionCandidateStoreAuthorityRequested)
            {
                return string.Empty;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                CollisionCandidateStoreAuthorityFailurePrefix +
                " phase={0}; requested={1}; configured={2}; applied={3}; " +
                "requestedTicks={4}; appliedTicks={5}; buildTicks={6}; " +
                "sampledOracleTicks={7}/{8}; storeOnlyTicks={9}/{10}; " +
                "legacyListTouches={11}; hardFailures={12}; legacyFallbackTicks={13}; " +
                "rangeReads={14}/{15}; entryReads={16}/{17}; failures={18}; " +
                "restored={19}; firstFailureReason={20}",
                phase,
                targetReport.collisionCandidateStoreAuthorityRequested,
                targetReport.collisionCandidateStoreAuthorityConfigured,
                targetReport.collisionCandidateStoreAuthorityApplied,
                targetReport.collisionCandidateStoreAuthorityRequestedTickCount,
                targetReport.collisionCandidateStoreAuthorityAppliedTickCount,
                targetReport.collisionCandidateStoreShadowBuildTickCount,
                targetReport.collisionCandidateStoreAuthoritySampledOracleTickCount,
                targetReport.collisionCandidateStoreAuthorityExpectedSampledOracleTickCount,
                targetReport.collisionCandidateStoreAuthorityStoreOnlyTickCount,
                targetReport.collisionCandidateStoreAuthorityExpectedStoreOnlyTickCount,
                targetReport.collisionCandidateStoreAuthorityLegacyListCreatedOrWrittenCount,
                targetReport.collisionCandidateStoreAuthorityStoreOnlyHardFailureCount,
                targetReport.collisionCandidateStoreAuthorityLegacyFallbackTickCount,
                targetReport.collisionCandidateStoreAuthorityRangeReadCount,
                targetReport.collisionCandidateConsumerEntityTicks,
                targetReport.collisionCandidateStoreAuthorityEntryReadCount,
                targetReport.collisionCandidateCountSum,
                targetReport.collisionCandidateStoreAuthorityFailureCount,
                targetReport.collisionCandidateStoreAuthorityRestored,
                string.IsNullOrEmpty(
                    targetReport.collisionCandidateStoreAuthorityFirstFailureReason)
                    ? CollisionCandidateStoreAuthorityFailureReason.None.ToString()
                    : targetReport.collisionCandidateStoreAuthorityFirstFailureReason);
        }

        private static long NonNegativeDelta(long current, long baseline)
        {
            return current >= baseline ? current - baseline : 0;
        }

        internal static void CaptureCollisionCandidateStoreShadowDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            BruteForceSceneQuery sceneQuery)
        {
            if (targetReport == null || sceneQuery == null)
                return;

            CollisionCandidateStoreDiagnostics diagnostics =
                sceneQuery.CollisionCandidateStoreShadowDiagnostics;
            targetReport.collisionCandidateStoreShadowBuildTickCount = Math.Max(
                targetReport.collisionCandidateStoreShadowBuildTickCount,
                diagnostics.BuildTickCount);
            targetReport.collisionCandidateStoreShadowComparedAttackerCount = Math.Max(
                targetReport.collisionCandidateStoreShadowComparedAttackerCount,
                diagnostics.ComparedAttackerCount);
            targetReport.collisionCandidateStoreShadowComparedCandidateCount = Math.Max(
                targetReport.collisionCandidateStoreShadowComparedCandidateCount,
                diagnostics.ComparedCandidateCount);
            targetReport.collisionCandidateStoreShadowMismatchCount = Math.Max(
                targetReport.collisionCandidateStoreShadowMismatchCount,
                diagnostics.MismatchCount);
            targetReport.collisionCandidateStoreShadowInvalidCount = Math.Max(
                targetReport.collisionCandidateStoreShadowInvalidCount,
                diagnostics.InvalidCount);
            targetReport.collisionCandidateStoreShadowFirstMismatchReason =
                diagnostics.FirstMismatchReason.ToString();
            targetReport.collisionCandidateStoreShadowRuntimeCapacity = Math.Max(
                targetReport.collisionCandidateStoreShadowRuntimeCapacity,
                sceneQuery.CollisionCandidateStoreRuntimeCapacityForDiagnostics);
        }

        internal static bool RestoreCollisionCandidateStoreShadowForDiagnostics(
            BruteForceSceneQuery sceneQuery,
            bool previousEnabled,
            ProductionEntityStressReport targetReport)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            CaptureCollisionCandidateStoreShadowDiagnosticsForReport(targetReport, sceneQuery);
            if (sceneQuery == null)
            {
                targetReport.collisionCandidateStoreShadowRestored =
                    !targetReport.collisionCandidateStoreShadowRequested;
                return targetReport.collisionCandidateStoreShadowRestored;
            }

            sceneQuery.CollisionCandidateStoreShadowDiagnosticsEnabled = previousEnabled;
            targetReport.collisionCandidateStoreShadowRestored =
                sceneQuery.CollisionCandidateStoreShadowDiagnosticsEnabled == previousEnabled;
            return targetReport.collisionCandidateStoreShadowRestored;
        }

        internal static bool EvaluateCollisionCandidateStoreShadowValidityForReport(
            ProductionEntityStressReport targetReport,
            CollisionCandidateStoreValidationPhase phase)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));
            if (!targetReport.collisionCandidateStoreShadowRequested)
                return true;

            bool valid = targetReport.collisionCandidateStoreShadowApplied;
            if (phase != CollisionCandidateStoreValidationPhase.PreTick)
            {
                valid = valid &&
                        targetReport.collisionCandidateStoreShadowBuildTickCount > 0 &&
                        targetReport.collisionCandidateStoreShadowMismatchCount == 0 &&
                        targetReport.collisionCandidateStoreShadowInvalidCount == 0;
            }
            if (phase == CollisionCandidateStoreValidationPhase.Teardown)
                valid = valid && targetReport.collisionCandidateStoreShadowRestored;
            if (valid)
                return true;

            targetReport.harnessValidity = false;
            string failureReason = BuildCollisionCandidateStoreShadowFailureReason(
                targetReport,
                phase);
            if (string.IsNullOrEmpty(targetReport.failure))
            {
                targetReport.failure = failureReason;
            }
            else if (targetReport.failure.IndexOf(
                         CollisionCandidateStoreShadowFailurePrefix,
                         StringComparison.Ordinal) < 0)
            {
                targetReport.failure += Environment.NewLine + failureReason;
            }
            return false;
        }

        internal static string BuildCollisionCandidateStoreShadowFailureReason(
            ProductionEntityStressReport targetReport,
            CollisionCandidateStoreValidationPhase phase)
        {
            if (targetReport == null ||
                !targetReport.collisionCandidateStoreShadowRequested)
            {
                return string.Empty;
            }

            return string.Format(
                CultureInfo.InvariantCulture,
                CollisionCandidateStoreShadowFailurePrefix +
                " phase={0}; requested={1}; applied={2}; buildTicks={3}; " +
                "mismatch={4}; invalid={5}; restored={6}; firstMismatchReason={7}",
                phase,
                targetReport.collisionCandidateStoreShadowRequested,
                targetReport.collisionCandidateStoreShadowApplied,
                targetReport.collisionCandidateStoreShadowBuildTickCount,
                targetReport.collisionCandidateStoreShadowMismatchCount,
                targetReport.collisionCandidateStoreShadowInvalidCount,
                targetReport.collisionCandidateStoreShadowRestored,
                string.IsNullOrEmpty(
                    targetReport.collisionCandidateStoreShadowFirstMismatchReason)
                    ? CollisionCandidateStoreMismatchReason.None.ToString()
                    : targetReport.collisionCandidateStoreShadowFirstMismatchReason);
        }

        internal static void CaptureRoleAwareBroadphaseDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            BruteForceSceneQuery sceneQuery)
        {
            if (targetReport == null || sceneQuery == null)
                return;

            targetReport.forceRoleAwareDirectApplied =
                sceneQuery.ForceRoleAwareDirectForDiagnostics;
            targetReport.forceRoleAwareTreeApplied =
                sceneQuery.ForceRoleAwareTreeForDiagnostics;
            targetReport.forceRoleAwareNestedDirectApplied =
                sceneQuery.ForceRoleAwareNestedDirectForDiagnostics;
            targetReport.forceRoleAwareSweepDirectApplied =
                sceneQuery.ForceRoleAwareSweepDirectForDiagnostics;
            targetReport.roleAwareDirectTickCount = Math.Max(
                targetReport.roleAwareDirectTickCount,
                sceneQuery.TotalRoleAwareDirectTickCountForDiagnostics);
            targetReport.roleAwareTreeTickCount = Math.Max(
                targetReport.roleAwareTreeTickCount,
                sceneQuery.TotalRoleAwareTreeTickCountForDiagnostics);
            targetReport.roleAwareNestedDirectTickCount = Math.Max(
                targetReport.roleAwareNestedDirectTickCount,
                sceneQuery.TotalRoleAwareNestedDirectTickCountForDiagnostics);
            targetReport.roleAwareSweepDirectTickCount = Math.Max(
                targetReport.roleAwareSweepDirectTickCount,
                sceneQuery.TotalRoleAwareSweepDirectTickCountForDiagnostics);
            targetReport.roleAwareLastDirectTickCount =
                sceneQuery.LastRoleAwareDirectTickCountForDiagnostics;
            targetReport.roleAwareLastTreeTickCount =
                sceneQuery.LastRoleAwareTreeTickCountForDiagnostics;
            targetReport.roleAwareLastNestedDirectTickCount =
                sceneQuery.LastRoleAwareNestedDirectTickCountForDiagnostics;
            targetReport.roleAwareLastSweepDirectTickCount =
                sceneQuery.LastRoleAwareSweepDirectTickCountForDiagnostics;
            targetReport.roleAwareSweepXCandidateCount = Math.Max(
                targetReport.roleAwareSweepXCandidateCount,
                sceneQuery.TotalRoleAwareSweepXCandidateCountForDiagnostics);
            targetReport.roleAwareLastSweepXCandidateCount =
                sceneQuery.LastRoleAwareSweepXCandidateCountForDiagnostics;
            targetReport.roleAwareSweepFullOverlapCheckCount = Math.Max(
                targetReport.roleAwareSweepFullOverlapCheckCount,
                sceneQuery.TotalRoleAwareSweepFullOverlapCheckCountForDiagnostics);
            targetReport.roleAwareLastSweepFullOverlapCheckCount =
                sceneQuery.LastRoleAwareSweepFullOverlapCheckCountForDiagnostics;
            targetReport.roleAwareDirectComparisonCount = Math.Max(
                targetReport.roleAwareDirectComparisonCount,
                sceneQuery.TotalRoleAwareDirectComparisonCountForDiagnostics);
            targetReport.roleAwareLastDirectComparisonCount =
                sceneQuery.LastRoleAwareDirectComparisonCountForDiagnostics;
        }

        internal static void AccumulateRoleAwareDirectCostForReport(
            ProductionEntityStressReport targetReport,
            long directCost,
            bool available)
        {
            if (targetReport == null || !available)
                return;

            long normalizedCost = Math.Max(0L, directCost);
            targetReport.roleAwareDirectCostObservedTickCount = SaturatingIncrement(
                targetReport.roleAwareDirectCostObservedTickCount);
            targetReport.roleAwareDirectCostSum = SaturatingAdd(
                targetReport.roleAwareDirectCostSum,
                normalizedCost);
            targetReport.roleAwareDirectCostMax = Math.Max(
                targetReport.roleAwareDirectCostMax,
                normalizedCost);
            if (normalizedCost > 32768L)
            {
                targetReport.roleAwareDirectCostAbove32768TickCount = SaturatingIncrement(
                    targetReport.roleAwareDirectCostAbove32768TickCount);
            }
            if (normalizedCost > 65536L)
            {
                targetReport.roleAwareDirectCostAbove65536TickCount = SaturatingIncrement(
                    targetReport.roleAwareDirectCostAbove65536TickCount);
            }
            if (normalizedCost > 131072L)
            {
                targetReport.roleAwareDirectCostAbove131072TickCount = SaturatingIncrement(
                    targetReport.roleAwareDirectCostAbove131072TickCount);
            }
            if (normalizedCost > 262144L)
            {
                targetReport.roleAwareDirectCostAbove262144TickCount = SaturatingIncrement(
                    targetReport.roleAwareDirectCostAbove262144TickCount);
            }
        }

        private static long SaturatingIncrement(long value)
        {
            return value < long.MaxValue ? value + 1L : long.MaxValue;
        }

        internal static void RecordAllocationBytesForReport(
            ProductionEntityStressReport targetReport,
            ProductionEntityStressAllocationRegion region,
            long allocatedBytes)
        {
            if (targetReport == null)
                return;

            long normalizedBytes = Math.Max(0L, allocatedBytes);
            switch (region)
            {
                case ProductionEntityStressAllocationRegion.PostTickTimingCollectors:
                    RecordAllocationBytes(
                        ref targetReport.allocationPostTickTimingCollectors,
                        normalizedBytes);
                    break;
                case ProductionEntityStressAllocationRegion.CaptureProductionCountersTotal:
                    RecordAllocationBytes(
                        ref targetReport.allocationCaptureProductionCountersTotal,
                        normalizedBytes);
                    break;
                case ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersActiveEntityScan:
                    RecordAllocationBytes(
                        ref targetReport.allocationCaptureProductionCountersActiveEntityScan,
                        normalizedBytes);
                    break;
                case ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersSceneQueryDiagnostics:
                    RecordAllocationBytes(
                        ref targetReport.allocationCaptureProductionCountersSceneQueryDiagnostics,
                        normalizedBytes);
                    break;
                case ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersAiReportDiagnostics:
                    RecordAllocationBytes(
                        ref targetReport.allocationCaptureProductionCountersAiReportDiagnostics,
                        normalizedBytes);
                    break;
                case ProductionEntityStressAllocationRegion
                    .CaptureProductionCountersObserveRuntimeEntitySnapshot:
                    RecordAllocationBytes(
                        ref targetReport
                            .allocationCaptureProductionCountersObserveRuntimeEntitySnapshot,
                        normalizedBytes);
                    break;
                case ProductionEntityStressAllocationRegion.WriteReport:
                    RecordAllocationBytes(
                        ref targetReport.allocationWriteReport,
                        normalizedBytes);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
        }

        private static void RecordAllocationBytes(
            ref ProductionEntityStressAllocationRegionMetrics metrics,
            long allocatedBytes)
        {
            metrics.sampleCount = SaturatingIncrement(metrics.sampleCount);
            metrics.sumBytes = allocatedBytes >= long.MaxValue - metrics.sumBytes
                ? long.MaxValue
                : metrics.sumBytes + allocatedBytes;
            metrics.maximumBytes = Math.Max(metrics.maximumBytes, allocatedBytes);
            metrics.lastBytes = allocatedBytes;
        }

        internal static void RecordCpuElapsedTicksForReport(
            ProductionEntityStressReport targetReport,
            ProductionEntityStressCpuRegion region,
            long elapsedTicks)
        {
            double elapsedMilliseconds = elapsedTicks <= 0L
                ? 0d
                : elapsedTicks * 1000d / Stopwatch.Frequency;
            RecordCpuMillisecondsForReport(
                targetReport,
                region,
                elapsedMilliseconds);
        }

        internal static void RecordCpuMillisecondsForReport(
            ProductionEntityStressReport targetReport,
            ProductionEntityStressCpuRegion region,
            double elapsedMilliseconds)
        {
            if (targetReport == null)
                return;

            double normalizedMilliseconds;
            if (double.IsNaN(elapsedMilliseconds) || elapsedMilliseconds <= 0d)
                normalizedMilliseconds = 0d;
            else if (double.IsPositiveInfinity(elapsedMilliseconds))
                normalizedMilliseconds = double.MaxValue;
            else
                normalizedMilliseconds = elapsedMilliseconds;

            switch (region)
            {
                case ProductionEntityStressCpuRegion.RunnerUpdateTotal:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuRunnerUpdateTotal,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion.SpawnOrRemove:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuSpawnOrRemove,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion.StepMeasuredTickTotal:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuStepMeasuredTickTotal,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion.DriverStepOneTick:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuDriverStepOneTick,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion.PostTickTimingCollectors:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuPostTickTimingCollectors,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion.CaptureProductionCountersTotal:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuCaptureProductionCountersTotal,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion
                    .CaptureProductionCountersActiveEntityScan:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuCaptureProductionCountersActiveEntityScan,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion
                    .CaptureProductionCountersSceneQueryDiagnostics:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuCaptureProductionCountersSceneQueryDiagnostics,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion
                    .CaptureProductionCountersAiReportDiagnostics:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuCaptureProductionCountersAiReportDiagnostics,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion
                    .CaptureProductionCountersObserveRuntimeEntitySnapshot:
                    RecordCpuMilliseconds(
                        ref targetReport
                            .cpuCaptureProductionCountersObserveRuntimeEntitySnapshot,
                        normalizedMilliseconds);
                    break;
                case ProductionEntityStressCpuRegion.WriteReport:
                    RecordCpuMilliseconds(
                        ref targetReport.cpuWriteReport,
                        normalizedMilliseconds);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(region), region, null);
            }
        }

        private static void RecordCpuMilliseconds(
            ref ProductionEntityStressCpuRegionMetrics metrics,
            double elapsedMilliseconds)
        {
            metrics.sampleCount = SaturatingIncrement(metrics.sampleCount);
            metrics.sumMilliseconds =
                elapsedMilliseconds >= double.MaxValue - metrics.sumMilliseconds
                    ? double.MaxValue
                    : metrics.sumMilliseconds + elapsedMilliseconds;
            metrics.maximumMilliseconds = Math.Max(
                metrics.maximumMilliseconds,
                elapsedMilliseconds);
            metrics.lastMilliseconds = elapsedMilliseconds;
        }

        private static long SaturatingAdd(long first, long second)
        {
            if (first >= long.MaxValue - second)
                return long.MaxValue;
            return first + second;
        }

        internal static void ApplyAiSensingConfigurationForDiagnostics(
            SimulationWorld targetWorld,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport)
        {
            if (targetWorld == null)
                throw new ArgumentNullException(nameof(targetWorld));
            if (targetWorld.ObjectCount != 0 ||
                targetWorld.ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new InvalidOperationException(
                    "AI sensing mode and seed must be applied before stress entities are registered.");
            }

            targetWorld.ConfigureAiExecutionProfile(runConfig.AiExecutionProfile);
            if (runConfig.UsesLegacyAiConfigurationCompatibility &&
                runConfig.AiSensingMode != AiSensingMode.SoAAiSensing)
            {
                targetWorld.AiSensingMode = runConfig.AiSensingMode;
            }
            targetWorld.ResetAiSoASensingShadowDiagnostics();
            targetWorld.ResetAiSoACandidateDiagnostics();
            targetWorld.Rng.Seed(runConfig.Seed);
            if (targetReport == null)
                return;

            targetReport.seed = runConfig.Seed;
            targetReport.allowUnsafeAiSoACandidate = runConfig.AllowUnsafeAiSoACandidate;
            targetReport.aiExecutionProfileRequested =
                ProductionEntityStressConfig.FormatAiExecutionProfile(
                    runConfig.AiExecutionProfile);
            targetReport.aiExecutionProfileEffective =
                ProductionEntityStressConfig.FormatAiExecutionProfile(
                    targetWorld.AiExecutionProfile);
            targetReport.aiExecutionProfileLegacyCompatibility =
                runConfig.UsesLegacyAiConfigurationCompatibility;
            targetReport.aiSensingRequestedMode =
                ProductionEntityStressConfig.FormatAiSensingMode(runConfig.AiSensingMode);
            targetReport.aiSensingEffectiveMode =
                ProductionEntityStressConfig.FormatAiSensingMode(targetWorld.AiSensingMode);
        }

        internal static void CloseAiExecutionProfileForDiagnostics(
            SimulationWorld targetWorld)
        {
            if (targetWorld == null)
                return;
            targetWorld.ConfigureAiExecutionProfile(
                BattleAiExecutionProfile.LegacyCanonical);
        }

        [Obsolete("Use CloseAiExecutionProfileForDiagnostics. Retained for legacy Editor tests only.")]
        internal static void CloseUnsafeAiSensingConfigurationForDiagnostics(
            SimulationWorld targetWorld)
        {
            CloseAiExecutionProfileForDiagnostics(targetWorld);
        }

        internal static bool ApplyAiSoADecisionRemainderForDiagnostics(
            SimulationWorld targetWorld,
            ProductionEntityStressConfig runConfig,
            ProductionEntityStressReport targetReport)
        {
            if (targetWorld == null)
                throw new ArgumentNullException(nameof(targetWorld));

            bool previous = targetWorld.AiSoADecisionRemainderEnabledForDiagnostics;
            InvokeAiSoADecisionRemainderModeForDiagnostics(
                targetWorld,
                runConfig.EnableAiSoADecisionRemainder);
            targetWorld.ResetAiSoACandidateDiagnostics();
            if (targetReport != null)
            {
                targetReport.aiSoADecisionRemainderRequested =
                    runConfig.EnableAiSoADecisionRemainder;
                targetReport.aiSoADecisionRemainderApplied =
                    targetWorld.AiSoADecisionRemainderEnabledForDiagnostics ==
                    runConfig.EnableAiSoADecisionRemainder;
            }
            return previous;
        }

        internal static void RestoreAiSoADecisionRemainderForDiagnostics(
            SimulationWorld targetWorld,
            bool previousEnabled,
            ProductionEntityStressReport targetReport)
        {
            if (targetWorld == null)
                return;

            InvokeAiSoADecisionRemainderModeForDiagnostics(
                targetWorld,
                previousEnabled);
            if (targetReport != null)
            {
                targetReport.aiSoADecisionRemainderRestored =
                    targetWorld.AiSoADecisionRemainderEnabledForDiagnostics ==
                    previousEnabled;
            }
        }

        private static void InvokeAiSoADecisionRemainderModeForDiagnostics(
            SimulationWorld targetWorld,
            bool enabled)
        {
            MethodInfo method = typeof(SimulationWorld).GetMethod(
                "SetAiSoADecisionRemainderModeForSelfCheck",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new MissingMethodException(
                    typeof(SimulationWorld).FullName,
                    "SetAiSoADecisionRemainderModeForSelfCheck");
            }

            try
            {
                method.Invoke(targetWorld, new object[] { enabled });
            }
            catch (TargetInvocationException exception) when (exception.InnerException != null)
            {
                throw exception.InnerException;
            }
        }

        internal static bool AreAiSoADecisionRemainderDiagnosticsValid(
            ProductionEntityStressReport targetReport)
        {
            if (targetReport == null)
                return false;
            if (!targetReport.aiSoADecisionRemainderRequested)
                return true;

            long completedAttemptCount =
                (long)targetReport.aiSoADecisionRemainderAppliedCount +
                targetReport.aiSoADecisionRemainderFallbackCount +
                targetReport.aiSoADecisionRemainderHardFailureCount;
            return targetReport.aiSoADecisionRemainderApplied &&
                   targetReport.aiSoADecisionRemainderEligibleAttemptCount ==
                       targetReport.aiSoADecisionRemainderExpectedAppliedCount &&
                   completedAttemptCount ==
                       targetReport.aiSoADecisionRemainderEligibleAttemptCount &&
                   targetReport.aiSoADecisionRemainderFallbackCount == 0 &&
                   targetReport.aiSoADecisionRemainderPreRandomFailureCount == 0 &&
                   targetReport.aiSoADecisionRemainderPostRandomFailureCount == 0 &&
                   targetReport.aiSoADecisionRemainderHardFailureCount == 0 &&
                   targetReport.aiSoADecisionRemainderContextBindCount ==
                       targetReport.aiSoADecisionRemainderAppliedCount +
                       targetReport.aiSoADecisionRemainderHardFailureCount &&
                   targetReport.aiSoADecisionRemainderGatewayValidationCount ==
                       2 * targetReport.aiSoADecisionRemainderContextBindCount &&
                   targetReport.aiSoADecisionRemainderRowVisitCount ==
                       6L * targetReport.aiSoADecisionRemainderContextBindCount;
        }

        internal static void AggregateAiSoACandidateDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            int nearestQueryCount,
            int specialQueryCount,
            long groundXRowVisitCount,
            long airXRowVisitCount,
            long specialSlotVisitCount,
            int legacyNearestScanCount,
            int legacySpecialScanCount,
            int preRandomFailureCount,
            int postRandomFailureCount)
        {
            if (targetReport == null)
                return;

            targetReport.aiSoACandidateNearestQueryCount = Math.Max(
                targetReport.aiSoACandidateNearestQueryCount,
                nearestQueryCount);
            targetReport.aiSoACandidateSpecialQueryCount = Math.Max(
                targetReport.aiSoACandidateSpecialQueryCount,
                specialQueryCount);
            targetReport.aiSoACandidateGroundXRowVisitCount = Math.Max(
                targetReport.aiSoACandidateGroundXRowVisitCount,
                groundXRowVisitCount);
            targetReport.aiSoACandidateAirXRowVisitCount = Math.Max(
                targetReport.aiSoACandidateAirXRowVisitCount,
                airXRowVisitCount);
            targetReport.aiSoACandidateSpecialSlotVisitCount = Math.Max(
                targetReport.aiSoACandidateSpecialSlotVisitCount,
                specialSlotVisitCount);
            targetReport.aiSoACandidateLegacyNearestScanCount = Math.Max(
                targetReport.aiSoACandidateLegacyNearestScanCount,
                legacyNearestScanCount);
            targetReport.aiSoACandidateLegacySpecialScanCount = Math.Max(
                targetReport.aiSoACandidateLegacySpecialScanCount,
                legacySpecialScanCount);
            targetReport.aiSoACandidatePreRandomFailureCount = Math.Max(
                targetReport.aiSoACandidatePreRandomFailureCount,
                preRandomFailureCount);
            targetReport.aiSoACandidatePostRandomFailureCount = Math.Max(
                targetReport.aiSoACandidatePostRandomFailureCount,
                postRandomFailureCount);
        }

        internal static bool AreAiSoACandidateFallbackDiagnosticsClean(
            ProductionEntityStressReport targetReport)
        {
            return targetReport != null &&
                   targetReport.aiSoACandidateLegacyNearestScanCount == 0 &&
                   targetReport.aiSoACandidateLegacySpecialScanCount == 0 &&
                   targetReport.aiSoACandidatePreRandomFailureCount == 0 &&
                   targetReport.aiSoACandidatePostRandomFailureCount == 0;
        }

        internal static void AggregateAiSoADecisionRemainderDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            int eligibleAttemptCount,
            int appliedCount,
            int fallbackCount,
            int preRandomFailureCount,
            int postRandomFailureCount,
            int hardFailureCount,
            int contextBindCount,
            int gatewayValidationCount,
            long rowVisitCount)
        {
            if (targetReport == null)
                return;

            targetReport.aiSoADecisionRemainderEligibleAttemptCount = Math.Max(
                targetReport.aiSoADecisionRemainderEligibleAttemptCount,
                eligibleAttemptCount);
            targetReport.aiSoADecisionRemainderExpectedAppliedCount = Math.Max(
                targetReport.aiSoADecisionRemainderExpectedAppliedCount,
                eligibleAttemptCount);
            targetReport.aiSoADecisionRemainderAppliedCount = Math.Max(
                targetReport.aiSoADecisionRemainderAppliedCount,
                appliedCount);
            targetReport.aiSoADecisionRemainderFallbackCount = Math.Max(
                targetReport.aiSoADecisionRemainderFallbackCount,
                fallbackCount);
            targetReport.aiSoADecisionRemainderPreRandomFailureCount = Math.Max(
                targetReport.aiSoADecisionRemainderPreRandomFailureCount,
                preRandomFailureCount);
            targetReport.aiSoADecisionRemainderPostRandomFailureCount = Math.Max(
                targetReport.aiSoADecisionRemainderPostRandomFailureCount,
                postRandomFailureCount);
            targetReport.aiSoADecisionRemainderHardFailureCount = Math.Max(
                targetReport.aiSoADecisionRemainderHardFailureCount,
                hardFailureCount);
            targetReport.aiSoADecisionRemainderContextBindCount = Math.Max(
                targetReport.aiSoADecisionRemainderContextBindCount,
                contextBindCount);
            targetReport.aiSoADecisionRemainderGatewayValidationCount = Math.Max(
                targetReport.aiSoADecisionRemainderGatewayValidationCount,
                gatewayValidationCount);
            targetReport.aiSoADecisionRemainderRowVisitCount = Math.Max(
                targetReport.aiSoADecisionRemainderRowVisitCount,
                rowVisitCount);
        }

        internal static void CaptureAiDecisionSoAShadowDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            SimulationWorld targetWorld)
        {
            if (targetReport == null || targetWorld == null)
                return;
            targetReport.aiDecisionSoAShadowEligibleCount = Math.Max(
                targetReport.aiDecisionSoAShadowEligibleCount,
                targetWorld.AiDecisionShadowEligibleCountForDiagnostics);
            targetReport.aiDecisionSoAShadowAvailableCount = Math.Max(
                targetReport.aiDecisionSoAShadowAvailableCount,
                targetWorld.AiDecisionShadowAvailableCountForDiagnostics);
            targetReport.aiDecisionSoAShadowUnavailableCount = Math.Max(
                targetReport.aiDecisionSoAShadowUnavailableCount,
                targetWorld.AiDecisionShadowUnavailableCountForDiagnostics);
            targetReport.aiDecisionSoAShadowComparedCount = Math.Max(
                targetReport.aiDecisionSoAShadowComparedCount,
                targetWorld.AiDecisionShadowComparedCountForDiagnostics);
            targetReport.aiDecisionSoAShadowMismatchCount = Math.Max(
                targetReport.aiDecisionSoAShadowMismatchCount,
                targetWorld.AiDecisionShadowMismatchCountForDiagnostics);
            targetReport.aiDecisionSoAShadowCloneRngCallCount = Math.Max(
                targetReport.aiDecisionSoAShadowCloneRngCallCount,
                targetWorld.AiDecisionShadowCloneRngCallCountForDiagnostics);
            targetReport.aiDecisionSoAShadowRowVisitCount = Math.Max(
                targetReport.aiDecisionSoAShadowRowVisitCount,
                targetWorld.AiDecisionShadowRowVisitCountForDiagnostics);
            targetReport.aiDecisionSharedShadowBuildCount = Math.Max(
                targetReport.aiDecisionSharedShadowBuildCount,
                targetWorld.AiDecisionSharedBuildCountForDiagnostics);
            targetReport.aiDecisionSharedShadowRefreshCount = Math.Max(
                targetReport.aiDecisionSharedShadowRefreshCount,
                targetWorld.AiDecisionSharedRefreshCountForDiagnostics);
            targetReport.aiDecisionIndexedEligibleCount = Math.Max(
                targetReport.aiDecisionIndexedEligibleCount,
                targetWorld.AiDecisionIndexedEligibleCountForDiagnostics);
            targetReport.aiDecisionIndexedAvailableCount = Math.Max(
                targetReport.aiDecisionIndexedAvailableCount,
                targetWorld.AiDecisionIndexedAvailableCountForDiagnostics);
            targetReport.aiDecisionIndexedUnavailableCount = Math.Max(
                targetReport.aiDecisionIndexedUnavailableCount,
                targetWorld.AiDecisionIndexedUnavailableCountForDiagnostics);
            targetReport.aiDecisionIndexedComparedCount = Math.Max(
                targetReport.aiDecisionIndexedComparedCount,
                targetWorld.AiDecisionIndexedComparedCountForDiagnostics);
            targetReport.aiDecisionIndexedMismatchCount = Math.Max(
                targetReport.aiDecisionIndexedMismatchCount,
                targetWorld.AiDecisionIndexedMismatchCountForDiagnostics);
            targetReport.aiDecisionIndexedFullRowVisitCount = Math.Max(
                targetReport.aiDecisionIndexedFullRowVisitCount,
                targetWorld.AiDecisionIndexedFullRowVisitCountForDiagnostics);
            targetReport.aiDecisionIndexedRowVisitCount = Math.Max(
                targetReport.aiDecisionIndexedRowVisitCount,
                targetWorld.AiDecisionIndexedRowVisitCountForDiagnostics);
            targetReport.aiDecisionIndexedCanonicalEligibleCount = Math.Max(
                targetReport.aiDecisionIndexedCanonicalEligibleCount,
                targetWorld.AiDecisionIndexedCanonicalEligibleCountForDiagnostics);
            targetReport.aiDecisionIndexedCanonicalCommittedCount = Math.Max(
                targetReport.aiDecisionIndexedCanonicalCommittedCount,
                targetWorld.AiDecisionIndexedCanonicalCommittedCountForDiagnostics);
            targetReport.aiDecisionIndexedCanonicalFallbackCount = Math.Max(
                targetReport.aiDecisionIndexedCanonicalFallbackCount,
                targetWorld.AiDecisionIndexedCanonicalFallbackCountForDiagnostics);
            targetReport.aiDecisionIndexedCanonicalFullOracleSampleCount = Math.Max(
                targetReport.aiDecisionIndexedCanonicalFullOracleSampleCount,
                targetWorld.AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics);
            targetReport.aiDecisionIndexedCanonicalFullOracleMismatchCount = Math.Max(
                targetReport.aiDecisionIndexedCanonicalFullOracleMismatchCount,
                targetWorld.AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics);
            AiDecisionAvailability canonicalFallbackReason =
                targetWorld.AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics;
            if (canonicalFallbackReason != AiDecisionAvailability.None)
            {
                targetReport.aiDecisionIndexedCanonicalFirstFallbackReason =
                    canonicalFallbackReason.ToString();
            }
            AiDecisionIndexedMismatchReason canonicalOracleMismatchReason =
                targetWorld.AiDecisionIndexedCanonicalFirstOracleMismatchReasonForDiagnostics;
            if (canonicalOracleMismatchReason != AiDecisionIndexedMismatchReason.None)
            {
                targetReport.aiDecisionIndexedCanonicalFirstOracleMismatchReason =
                    canonicalOracleMismatchReason.ToString();
            }
            AiDecisionIndexedMismatchReason indexedMismatchReason =
                targetWorld.AiDecisionIndexedFirstMismatchReasonForDiagnostics;
            if (indexedMismatchReason != AiDecisionIndexedMismatchReason.None)
            {
                targetReport.aiDecisionIndexedFirstMismatchReason =
                    indexedMismatchReason.ToString();
            }
            AiDecisionShadowMismatchReason mismatchReason =
                targetWorld.AiDecisionShadowFirstMismatchReasonForDiagnostics;
            if (mismatchReason != AiDecisionShadowMismatchReason.None)
                targetReport.aiDecisionSoAShadowFirstReason = mismatchReason.ToString();
            AiDecisionAvailability unavailableReason =
                targetWorld.AiDecisionShadowFirstUnavailableReasonForDiagnostics;
            if (unavailableReason != AiDecisionAvailability.None)
                targetReport.aiDecisionSoAShadowFirstUnavailableReason =
                    unavailableReason.ToString();
            AiDecisionShadowExceptionStage exceptionStage =
                targetWorld.AiDecisionShadowFirstExceptionStageForDiagnostics;
            if (exceptionStage != AiDecisionShadowExceptionStage.None &&
                (string.IsNullOrEmpty(targetReport.aiDecisionShadowFirstExceptionStage) ||
                 string.Equals(
                     targetReport.aiDecisionShadowFirstExceptionStage,
                     AiDecisionShadowExceptionStage.None.ToString(),
                     StringComparison.Ordinal)))
            {
                targetReport.aiDecisionShadowFirstExceptionStage = exceptionStage.ToString();
                targetReport.aiDecisionShadowFirstExceptionType =
                    targetWorld.AiDecisionShadowFirstExceptionTypeForDiagnostics?.FullName ??
                    string.Empty;
            }
            targetReport.unifiedAiSnapshotShadowBuildCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowBuildCount,
                targetWorld.AiUnifiedSnapshotShadowBuildCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowSlotVisitCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowSlotVisitCount,
                targetWorld.AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowRefreshCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowRefreshCount,
                targetWorld.AiUnifiedSnapshotShadowRefreshCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowFullComparisonSlotVisitCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowFullComparisonSlotVisitCount,
                targetWorld.AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowRefreshComparisonSlotVisitCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowRefreshComparisonSlotVisitCount,
                targetWorld.AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount,
                targetWorld.AiUnifiedSnapshotShadowDerivedComparisonEntryVisitCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowMutationWitnessComparedCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowMutationWitnessComparedCount,
                targetWorld.AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount,
                targetWorld.AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowSensingComparedCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowSensingComparedCount,
                targetWorld.AiUnifiedSnapshotShadowSensingComparedCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowDecisionComparedCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowDecisionComparedCount,
                targetWorld.AiUnifiedSnapshotShadowDecisionComparedCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowUnavailableCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowUnavailableCount,
                targetWorld.AiUnifiedSnapshotShadowUnavailableCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowMismatchCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowMismatchCount,
                targetWorld.AiUnifiedSnapshotShadowMismatchCountForDiagnostics);
            targetReport.unifiedAiSnapshotShadowDistinctBoundaryEncodingRowCount = Math.Max(
                targetReport.unifiedAiSnapshotShadowDistinctBoundaryEncodingRowCount,
                targetWorld.AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics);
            AiUnifiedSnapshotMismatch unifiedMismatch =
                targetWorld.AiUnifiedSnapshotShadowFirstMismatchForDiagnostics;
            if (unifiedMismatch.Kind != AiUnifiedSnapshotMismatchKind.None)
            {
                targetReport.unifiedAiSnapshotShadowFirstMismatch = string.Format(
                    CultureInfo.InvariantCulture,
                    "{0}/{1}/{2}/slot={3}/expected={4}/actual={5}",
                    unifiedMismatch.Consumer,
                    unifiedMismatch.Kind,
                    unifiedMismatch.Field,
                    unifiedMismatch.Slot,
                    unifiedMismatch.ExpectedValue,
                    unifiedMismatch.ActualValue);
            }
            AiUnifiedSnapshotExceptionStage unifiedExceptionStage =
                targetWorld.AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics;
            if (unifiedExceptionStage != AiUnifiedSnapshotExceptionStage.None)
            {
                targetReport.unifiedAiSnapshotShadowExceptionCount = Math.Max(
                    targetReport.unifiedAiSnapshotShadowExceptionCount,
                    1L);
                targetReport.unifiedAiSnapshotShadowFirstExceptionStage =
                    unifiedExceptionStage.ToString();
                targetReport.unifiedAiSnapshotShadowFirstExceptionType =
                    targetWorld.AiUnifiedSnapshotShadowFirstExceptionTypeForDiagnostics?.FullName ??
                    string.Empty;
            }
            targetReport.aiUnifiedSnapshotExecutionBuildCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionBuildCount,
                targetWorld.AiUnifiedSnapshotExecutionBuildCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionSlotVisitCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionSlotVisitCount,
                targetWorld.AiUnifiedSnapshotExecutionSlotVisitCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionRefreshCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionRefreshCount,
                targetWorld.AiUnifiedSnapshotExecutionRefreshCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionReadCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionReadCount,
                targetWorld.AiUnifiedSnapshotExecutionReadCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionCommittedPassCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionCommittedPassCount,
                targetWorld.AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionPreCommitFailureCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionPreCommitFailureCount,
                targetWorld.AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionPreCommitFallbackCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionPreCommitFallbackCount,
                targetWorld.AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionPostCommitHardBreachCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionPostCommitHardBreachCount,
                targetWorld.AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyFusedSensingBuildCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyFusedSensingBuildCount,
                targetWorld.AiSoACandidateFusedSnapshotBuildCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyDecisionSharedBuildCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyDecisionSharedBuildCount,
                targetWorld.AiDecisionSharedBuildCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyShadowBuildCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyShadowBuildCount,
                targetWorld.AiUnifiedSnapshotShadowBuildCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyNearestFactsBuildCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyNearestFactsBuildCount,
                targetWorld.AiLegacyNearestFactsBuildCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacySnapshotIndexBuildCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacySnapshotIndexBuildCount,
                targetWorld.AiLegacySnapshotIndexBuildCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyQuadtreeSyncCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyQuadtreeSyncCount,
                targetWorld.AiLegacyQuadtreeSyncCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyDecisionSharedRefreshCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyDecisionSharedRefreshCount,
                targetWorld.AiDecisionSharedRefreshCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyShadowRefreshCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyShadowRefreshCount,
                targetWorld.AiUnifiedSnapshotShadowRefreshCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacySnapshotMutationCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacySnapshotMutationCount,
                targetWorld.AiLegacySnapshotMutationCountForDiagnostics);
            targetReport.aiUnifiedSnapshotExecutionLegacyCandidateRefreshCount = Math.Max(
                targetReport.aiUnifiedSnapshotExecutionLegacyCandidateRefreshCount,
                targetWorld.AiSoACandidateSnapshotRefreshCountForDiagnostics);
            AiUnifiedSnapshotExceptionStage executionFailureStage =
                targetWorld.AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics;
            if (executionFailureStage != AiUnifiedSnapshotExceptionStage.None &&
                (string.IsNullOrEmpty(
                     targetReport.aiUnifiedSnapshotExecutionFirstFailureStage) ||
                 string.Equals(
                     targetReport.aiUnifiedSnapshotExecutionFirstFailureStage,
                     AiUnifiedSnapshotExceptionStage.None.ToString(),
                     StringComparison.Ordinal)))
            {
                targetReport.aiUnifiedSnapshotExecutionFirstFailureStage =
                    executionFailureStage.ToString();
                targetReport.aiUnifiedSnapshotExecutionFirstFailureType =
                    targetWorld.AiUnifiedSnapshotExecutionFirstFailureTypeForDiagnostics
                        ?.FullName ?? string.Empty;
            }
        }

        internal static void AggregateAiLegacyDiagnosticsForReport(
            ProductionEntityStressReport targetReport,
            int nearestFactsBuildCount,
            int snapshotIndexBuildCount,
            int quadtreeSyncCount,
            int snapshotMutationCount)
        {
            if (targetReport == null)
                return;

            targetReport.aiLegacyNearestFactsBuildCount = Math.Max(
                targetReport.aiLegacyNearestFactsBuildCount,
                nearestFactsBuildCount);
            targetReport.aiLegacySnapshotIndexBuildCount = Math.Max(
                targetReport.aiLegacySnapshotIndexBuildCount,
                snapshotIndexBuildCount);
            targetReport.aiLegacyQuadtreeSyncCount = Math.Max(
                targetReport.aiLegacyQuadtreeSyncCount,
                quadtreeSyncCount);
            targetReport.aiLegacySnapshotMutationCount = Math.Max(
                targetReport.aiLegacySnapshotMutationCount,
                snapshotMutationCount);
        }

        internal static bool AreAiLegacyDiagnosticsValidForMode(
            ProductionEntityStressReport targetReport,
            AiSensingMode aiSensingMode)
        {
            if (targetReport == null)
                return false;

            switch (aiSensingMode)
            {
                case AiSensingMode.SoAAiSensing:
                    return targetReport.aiLegacyNearestFactsBuildCount == 0 &&
                           targetReport.aiLegacySnapshotIndexBuildCount == 0 &&
                           targetReport.aiLegacyQuadtreeSyncCount == 0 &&
                           targetReport.aiLegacySnapshotMutationCount == 0;
                case AiSensingMode.LegacyAiSensing:
                    return !HasExecutedAiEntityInputPassForDiagnostics(targetReport) ||
                           targetReport.aiLegacyNearestFactsBuildCount > 0 &&
                           targetReport.aiLegacySnapshotIndexBuildCount > 0 &&
                           targetReport.aiLegacyQuadtreeSyncCount > 0 &&
                           targetReport.aiLegacySnapshotMutationCount > 0;
                case AiSensingMode.SoAShadowAiSensing:
                    return true;
                default:
                    return false;
            }
        }

        internal static bool HasExecutedAiEntityInputPassForDiagnostics(
            ProductionEntityStressReport targetReport)
        {
            // SimulationWorld.CharacterInputAll intentionally skips tick indices 0 and 1.
            // The report increments logicTicksExecuted after each successful driver step, so
            // Legacy build/mutation diagnostics can first be required after the second step.
            return targetReport != null && targetReport.logicTicksExecuted > 1;
        }

        internal static long ResolveExpectedUnifiedAiSnapshotObservedPassCount(
            int logicTicksExecuted)
        {
            // Stress logicTicksExecuted includes warmup and sampled ticks. Character input skips
            // the first completed driver step, so every later observed step owns one build.
            return Math.Max(0L, (long)logicTicksExecuted - 1L);
        }

        internal static bool ShouldEvaluateAiDecisionShadowAsTerminalForReport(
            ProductionEntityStressReport targetReport)
        {
            return targetReport?.teardown != null &&
                   targetReport.teardown.attempted &&
                   targetReport.teardown.restored;
        }

        internal static bool EvaluateUnifiedAiSnapshotExactClosureForReport(
            ProductionEntityStressReport targetReport,
            bool sensingRowsRequested,
            bool decisionRowsRequested)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            long expectedBuild = ResolveExpectedUnifiedAiSnapshotObservedPassCount(
                targetReport.logicTicksExecuted);
            if (expectedBuild <= 0 ||
                targetReport.requestedEntityCount <= 0 ||
                targetReport.runtimeSlotCapacity <= 0 ||
                !TryMultiplyNonNegative(
                    expectedBuild,
                    targetReport.runtimeSlotCapacity,
                    out long expectedSlotVisits) ||
                !TryMultiplyNonNegative(
                    expectedBuild,
                    targetReport.requestedEntityCount,
                    out long expectedRefresh) ||
                expectedRefresh > long.MaxValue - expectedBuild)
            {
                return false;
            }

            int activeConsumerCount = (sensingRowsRequested ? 1 : 0) +
                                      (decisionRowsRequested ? 1 : 0);
            if (!TryMultiplyNonNegative(
                    expectedSlotVisits,
                    activeConsumerCount,
                    out long expectedFullComparisonSlotVisits) ||
                !TryMultiplyNonNegative(
                    expectedRefresh,
                    activeConsumerCount,
                    out long expectedRefreshComparisonSlotVisits) ||
                !TryMultiplyNonNegative(
                    targetReport.runtimeSlotCapacity,
                    6L,
                    out long maximumDerivedEntriesPerConsumerBuild) ||
                maximumDerivedEntriesPerConsumerBuild > long.MaxValue - 9L ||
                !TryMultiplyNonNegative(
                    expectedBuild,
                    activeConsumerCount,
                    out long expectedConsumerBuilds))
            {
                return false;
            }

            maximumDerivedEntriesPerConsumerBuild += 9L;
            if (!TryMultiplyNonNegative(
                    expectedConsumerBuilds,
                    maximumDerivedEntriesPerConsumerBuild,
                    out long maximumInitialDerivedEntryVisits))
            {
                return false;
            }

            long expectedConsumerComparisons = expectedBuild + expectedRefresh;
            return targetReport.unifiedAiSnapshotShadowBuildCount == expectedBuild &&
                   targetReport.unifiedAiSnapshotShadowSlotVisitCount == expectedSlotVisits &&
                   targetReport.unifiedAiSnapshotShadowRefreshCount == expectedRefresh &&
                   targetReport.unifiedAiSnapshotShadowFullComparisonSlotVisitCount ==
                   expectedFullComparisonSlotVisits &&
                   targetReport.unifiedAiSnapshotShadowRefreshComparisonSlotVisitCount ==
                   expectedRefreshComparisonSlotVisits &&
                   targetReport.unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount >= 0 &&
                   targetReport.unifiedAiSnapshotShadowDerivedComparisonEntryVisitCount <=
                   maximumInitialDerivedEntryVisits &&
                   targetReport.unifiedAiSnapshotShadowMutationWitnessComparedCount ==
                   expectedRefreshComparisonSlotVisits &&
                   targetReport.unifiedAiSnapshotShadowRefreshDerivedFullLoopEntryVisitCount == 0 &&
                   targetReport.unifiedAiSnapshotShadowSensingComparedCount ==
                   (sensingRowsRequested ? expectedConsumerComparisons : 0L) &&
                   targetReport.unifiedAiSnapshotShadowDecisionComparedCount ==
                   (decisionRowsRequested ? expectedConsumerComparisons : 0L) &&
                   targetReport.unifiedAiSnapshotShadowUnavailableCount == 0 &&
                   targetReport.unifiedAiSnapshotShadowMismatchCount == 0 &&
                   targetReport.unifiedAiSnapshotShadowExceptionCount == 0 &&
                   (string.IsNullOrEmpty(targetReport.unifiedAiSnapshotShadowFirstExceptionStage) ||
                    string.Equals(
                        targetReport.unifiedAiSnapshotShadowFirstExceptionStage,
                        AiUnifiedSnapshotExceptionStage.None.ToString(),
                        StringComparison.Ordinal)) &&
                   string.IsNullOrEmpty(
                       targetReport.unifiedAiSnapshotShadowFirstExceptionType);
        }

        internal static bool EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
            ProductionEntityStressReport targetReport,
            bool terminal)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            bool requested = string.Equals(
                targetReport.aiUnifiedSnapshotExecutionRequestedMode,
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority.ToString(),
                StringComparison.Ordinal);
            if (!requested)
            {
                targetReport.aiUnifiedSnapshotExecutionAuthoritySuccess = false;
                return true;
            }

            bool evidenceRequired = terminal ||
                                    HasExecutedAiEntityInputPassForDiagnostics(targetReport);
            bool valid = string.Equals(
                             targetReport.aiUnifiedSnapshotExecutionEffectiveMode,
                             AiUnifiedSnapshotExecutionMode.UnifiedAuthority.ToString(),
                             StringComparison.Ordinal) &&
                         targetReport.aiUnifiedSnapshotExecutionPreCommitFailureCount == 0 &&
                         targetReport.aiUnifiedSnapshotExecutionPreCommitFallbackCount == 0 &&
                         targetReport.aiUnifiedSnapshotExecutionPostCommitHardBreachCount == 0 &&
                         HasNoAiUnifiedSnapshotExecutionFirstFailure(targetReport);
            if (terminal)
                valid = valid && targetReport.aiUnifiedSnapshotExecutionRestored;

            if (evidenceRequired)
            {
                long expectedBuild = ResolveExpectedUnifiedAiSnapshotObservedPassCount(
                    targetReport.logicTicksExecuted);
                if (expectedBuild <= 0 ||
                    targetReport.requestedEntityCount <= 0 ||
                    targetReport.runtimeSlotCapacity <= 0 ||
                    !TryMultiplyNonNegative(
                        expectedBuild,
                        targetReport.runtimeSlotCapacity,
                        out long expectedSlotVisits) ||
                    !TryMultiplyNonNegative(
                        expectedBuild,
                        targetReport.requestedEntityCount,
                        out long expectedRefreshAndRead))
                {
                    valid = false;
                }
                else
                {
                    valid = valid &&
                            targetReport.aiUnifiedSnapshotExecutionBuildCount == expectedBuild &&
                            targetReport.aiUnifiedSnapshotExecutionCommittedPassCount ==
                            expectedBuild &&
                            targetReport.aiUnifiedSnapshotExecutionSlotVisitCount ==
                            expectedSlotVisits &&
                            targetReport.aiUnifiedSnapshotExecutionRefreshCount ==
                            expectedRefreshAndRead &&
                            targetReport.aiUnifiedSnapshotExecutionReadCount ==
                            expectedRefreshAndRead &&
                            AreReplacedAiSnapshotPipelinesClean(targetReport);
                }
            }

            targetReport.aiUnifiedSnapshotExecutionAuthoritySuccess =
                valid && evidenceRequired;
            if (!valid)
                targetReport.harnessValidity = false;
            return valid;
        }

        internal static bool EvaluateAiUnifiedSnapshotRollbackContractForReport(
            ProductionEntityStressReport targetReport,
            bool terminal)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));

            bool requested = string.Equals(
                targetReport.aiUnifiedSnapshotExecutionRequestedMode,
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority.ToString(),
                StringComparison.Ordinal);
            long failureCount =
                targetReport.aiUnifiedSnapshotExecutionPreCommitFailureCount;
            long fallbackCount =
                targetReport.aiUnifiedSnapshotExecutionPreCommitFallbackCount;
            bool observed = requested && (failureCount != 0 || fallbackCount != 0);
            targetReport.aiUnifiedSnapshotExecutionRollbackObserved = observed;
            targetReport.aiUnifiedSnapshotExecutionRollbackContractSatisfied = false;
            if (!observed)
                return true;
            targetReport.aiUnifiedSnapshotExecutionAuthoritySuccess = false;

            long expectedBuild = ResolveExpectedUnifiedAiSnapshotObservedPassCount(
                targetReport.logicTicksExecuted);
            long committedPassCount =
                targetReport.aiUnifiedSnapshotExecutionCommittedPassCount;
            bool valid = string.Equals(
                             targetReport.aiUnifiedSnapshotExecutionEffectiveMode,
                             AiUnifiedSnapshotExecutionMode.UnifiedAuthority.ToString(),
                             StringComparison.Ordinal) &&
                         failureCount > 0 &&
                         failureCount == fallbackCount &&
                         fallbackCount <= expectedBuild &&
                         committedPassCount == expectedBuild - fallbackCount &&
                         targetReport.aiUnifiedSnapshotExecutionBuildCount == expectedBuild &&
                         targetReport.aiUnifiedSnapshotExecutionPostCommitHardBreachCount == 0 &&
                         HasRecordedAiUnifiedSnapshotPreCommitFailure(targetReport) &&
                         targetReport.requestedEntityCount > 0 &&
                         targetReport.runtimeSlotCapacity > 0 &&
                         TryMultiplyNonNegative(
                             expectedBuild,
                             targetReport.runtimeSlotCapacity,
                             out long maximumSlotVisits) &&
                         TryMultiplyNonNegative(
                             committedPassCount,
                             targetReport.requestedEntityCount,
                             out long expectedRefreshAndRead) &&
                         targetReport.aiUnifiedSnapshotExecutionSlotVisitCount >= 0 &&
                         targetReport.aiUnifiedSnapshotExecutionSlotVisitCount <=
                         maximumSlotVisits &&
                         targetReport.aiUnifiedSnapshotExecutionRefreshCount ==
                         expectedRefreshAndRead &&
                         targetReport.aiUnifiedSnapshotExecutionReadCount ==
                         expectedRefreshAndRead;
            if (terminal)
                valid = valid && targetReport.aiUnifiedSnapshotExecutionRestored;
            targetReport.aiUnifiedSnapshotExecutionRollbackContractSatisfied = valid;
            return valid;
        }

        private static bool AreReplacedAiSnapshotPipelinesClean(
            ProductionEntityStressReport targetReport)
        {
            return targetReport.aiUnifiedSnapshotExecutionLegacyFusedSensingBuildCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacyDecisionSharedBuildCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacyShadowBuildCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacyNearestFactsBuildCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacySnapshotIndexBuildCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacyQuadtreeSyncCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacyDecisionSharedRefreshCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacyShadowRefreshCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacySnapshotMutationCount == 0 &&
                   targetReport.aiUnifiedSnapshotExecutionLegacyCandidateRefreshCount == 0;
        }

        private static bool HasNoAiUnifiedSnapshotExecutionFirstFailure(
            ProductionEntityStressReport targetReport)
        {
            bool parsed = Enum.TryParse(
                targetReport.aiUnifiedSnapshotExecutionFirstFailureStage,
                out AiUnifiedSnapshotExceptionStage stage);
            return (!parsed || stage == AiUnifiedSnapshotExceptionStage.None) &&
                   string.IsNullOrEmpty(
                       targetReport.aiUnifiedSnapshotExecutionFirstFailureType);
        }

        private static bool HasRecordedAiUnifiedSnapshotPreCommitFailure(
            ProductionEntityStressReport targetReport)
        {
            return Enum.TryParse(
                       targetReport.aiUnifiedSnapshotExecutionFirstFailureStage,
                       out AiUnifiedSnapshotExceptionStage stage) &&
                   stage >= AiUnifiedSnapshotExceptionStage.Prepare &&
                   stage <= AiUnifiedSnapshotExceptionStage.Validate &&
                   !string.IsNullOrEmpty(
                       targetReport.aiUnifiedSnapshotExecutionFirstFailureType);
        }

        private static bool TryMultiplyNonNegative(
            long left,
            long right,
            out long product)
        {
            product = 0;
            if (left < 0 || right < 0 || left != 0 && right > long.MaxValue / left)
                return false;
            product = left * right;
            return true;
        }

        internal static bool EvaluateAiDecisionShadowValidityForReport(
            ProductionEntityStressReport targetReport,
            bool terminal)
        {
            if (targetReport == null)
                throw new ArgumentNullException(nameof(targetReport));
            bool deepRequested = targetReport.aiDecisionSoAShadowRequested;
            bool sharedRequested = targetReport.aiDecisionSharedShadowRequested;
            bool indexedCanonicalRequested = string.Equals(
                targetReport.aiDecisionExecutionRequestedMode,
                AiDecisionExecutionMode.IndexedCanonical.ToString(),
                StringComparison.Ordinal);
            bool unifiedRequested = targetReport.unifiedAiSnapshotShadowRequested;
            bool shadowRequested = deepRequested || sharedRequested;
            if (!shadowRequested && !indexedCanonicalRequested && !unifiedRequested)
                return true;

            bool evidenceRequired = terminal ||
                                    HasExecutedAiEntityInputPassForDiagnostics(targetReport);
            bool valid = (!deepRequested || targetReport.aiDecisionSoAShadowApplied) &&
                         (!sharedRequested || targetReport.aiDecisionSharedShadowApplied) &&
                         (!unifiedRequested || targetReport.unifiedAiSnapshotShadowApplied);
            if (shadowRequested)
            {
                valid = valid &&
                        targetReport.aiDecisionSoAShadowUnavailableCount == 0 &&
                        targetReport.aiDecisionSoAShadowMismatchCount == 0 &&
                        targetReport.aiDecisionSoAShadowComparedCount ==
                        targetReport.aiDecisionSoAShadowAvailableCount;
            }
            if (indexedCanonicalRequested)
            {
                valid = valid &&
                        string.Equals(
                            targetReport.aiDecisionExecutionEffectiveMode,
                            AiDecisionExecutionMode.IndexedCanonical.ToString(),
                            StringComparison.Ordinal) &&
                        targetReport.aiDecisionIndexedCanonicalFallbackCount == 0 &&
                        targetReport.aiDecisionIndexedCanonicalFullOracleMismatchCount == 0;
            }
            if (unifiedRequested)
            {
                valid = valid &&
                        targetReport.unifiedAiSnapshotShadowUnavailableCount == 0 &&
                        targetReport.unifiedAiSnapshotShadowMismatchCount == 0;
                if (terminal)
                    valid = valid && targetReport.unifiedAiSnapshotShadowRestored;
            }
            if (evidenceRequired)
            {
                if (shadowRequested)
                {
                    long eligible = targetReport.aiDecisionSoAShadowEligibleCount;
                    long available = targetReport.aiDecisionSoAShadowAvailableCount;
                    long unavailable = targetReport.aiDecisionSoAShadowUnavailableCount;
                    valid = valid &&
                            eligible > 0 &&
                            available >= 0 &&
                            available <= eligible &&
                            unavailable == eligible - available &&
                            targetReport.aiDecisionSoAShadowComparedCount == available;
                    if (sharedRequested)
                    {
                        long indexedEligible = targetReport.aiDecisionIndexedEligibleCount;
                        long indexedAvailable = targetReport.aiDecisionIndexedAvailableCount;
                        long indexedUnavailable = targetReport.aiDecisionIndexedUnavailableCount;
                        long expectedPassCount = Math.Max(
                            0L,
                            (long)targetReport.logicTicksExecuted - 1L);
                        valid = valid &&
                                targetReport.aiDecisionSharedShadowBuildCount ==
                                expectedPassCount &&
                                targetReport.aiDecisionSharedShadowRefreshCount == eligible &&
                                indexedEligible > 0 &&
                                indexedEligible == indexedAvailable + indexedUnavailable &&
                                indexedUnavailable == 0 &&
                                targetReport.aiDecisionIndexedComparedCount == indexedEligible &&
                                targetReport.aiDecisionIndexedMismatchCount == 0;
                    }
                }
                if (indexedCanonicalRequested)
                {
                    long canonicalEligible =
                        targetReport.aiDecisionIndexedCanonicalEligibleCount;
                    valid = valid &&
                            canonicalEligible > 0 &&
                            canonicalEligible ==
                            targetReport.aiDecisionIndexedCanonicalCommittedCount +
                            targetReport.aiDecisionIndexedCanonicalFallbackCount &&
                            targetReport.aiDecisionIndexedCanonicalCommittedCount > 0;
                }
                if (unifiedRequested)
                {
                    bool sensingRowsRequested = !string.Equals(
                        targetReport.aiSensingRequestedMode,
                        "legacy",
                        StringComparison.Ordinal);
                    bool decisionRowsRequested = indexedCanonicalRequested || sharedRequested;
                    valid = valid && EvaluateUnifiedAiSnapshotExactClosureForReport(
                        targetReport,
                        sensingRowsRequested,
                        decisionRowsRequested);
                }
            }
            if (!valid)
                targetReport.harnessValidity = false;
            return valid;
        }

        internal static CollisionFormalCollectorMode
            ResolveAppliedFormalCollectorModeForDiagnostics(
                SimulationWorld targetWorld,
                BruteForceSceneQuery sceneQuery)
        {
            if (targetWorld == null)
                throw new ArgumentNullException(nameof(targetWorld));
            if (sceneQuery == null)
                throw new ArgumentNullException(nameof(sceneQuery));
            if (!ReferenceEquals(targetWorld.SceneQuery, sceneQuery))
            {
                throw new ArgumentException(
                    "The scene query does not belong to the supplied stress world.",
                    nameof(sceneQuery));
            }

            if (sceneQuery.FormalCollectorMode != CollisionFormalCollectorMode.Configured)
                return sceneQuery.FormalCollectorMode;

            return targetWorld.CollisionBroadphaseForDiagnostics ==
                   CollisionBroadphaseBackend.LooseQuadtree
                ? CollisionFormalCollectorMode.ForceRoleAware
                : CollisionFormalCollectorMode.ForceBruteForce;
        }

        private int SpawnBatch(int count, bool deferWhenSaturated = false)
        {
            long spawnOrRemoveTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope spawnOrRemoveProfilerScope =
                SpawnOrRemoveProfilerMarker.Auto();
            try
            {
                int createdCount = 0;
                int remaining = config.EntityCount - CountActiveBaseRoster(out _);
                int spawnCount = Math.Min(Math.Max(0, count), remaining);
                for (int i = 0; i < spawnCount; i++)
                {
                    if (deferWhenSaturated)
                    {
                        RefreshRosterAndCapacityDiagnostics();
                        if (ProductionEntityStressReplenishmentPolicy.Evaluate(
                                report.baseRosterActiveCount,
                                config.EntityCount,
                                report.totalActiveRuntimeEntityCount,
                                report.totalClaimedRuntimeSlotCount,
                                BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities,
                                report.currentSaturationDrainTicks,
                                config.MaxSaturationDrainTicks) !=
                            ProductionEntityStressReplenishmentAction.Attempt)
                        {
                            report.replenishmentState = "SaturationDrain";
                            break;
                        }
                    }

                    int placementIndex = FindFirstAvailableBaseRosterIndex();
                    LF2Character entity = SpawnCharacter(placementIndex);
                    if (entity == null)
                    {
                        if (deferWhenSaturated)
                        {
                            RefreshRosterAndCapacityDiagnostics();
                            if (ProductionEntityStressReplenishmentPolicy.Evaluate(
                                    report.baseRosterActiveCount,
                                    config.EntityCount,
                                    report.totalActiveRuntimeEntityCount,
                                    report.totalClaimedRuntimeSlotCount,
                                    BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities,
                                    report.currentSaturationDrainTicks,
                                    config.MaxSaturationDrainTicks) !=
                                ProductionEntityStressReplenishmentAction.Attempt)
                            {
                                report.replenishmentState = "SaturationDrain";
                                break;
                            }
                        }
                        throw new InvalidOperationException(
                            $"Production creation chain failed at entity {placementIndex}/{config.EntityCount}.");
                    }
                    if (placementIndex < entities.Count)
                        entities[placementIndex] = entity;
                    else
                        entities.Add(entity);
                    report.totalEntitiesCreated++;
                    createdCount++;
                }
                RefreshReportCounts();
                return createdCount;
            }
            finally
            {
                spawnOrRemoveProfilerScope.Dispose();
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion.SpawnOrRemove,
                    Stopwatch.GetTimestamp() - spawnOrRemoveTimestamp);
            }
        }

        private LF2Character SpawnCharacter(int placementIndex)
        {
            Vector3 position = BuildSpawnPosition(config.Mode, placementIndex, config.EntityCount);
            int team = (placementIndex & 1) + 1;
            OPointCreateTask task = referencePool.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                kind = 1,
                oid = selectedCharacterOid,
                action = 0,
                facing = placementIndex & 1,
            };
            task.team = team;
            task.relationTeam = team;
            task.useExplicitRelationIdentity = true;
            task.dir = (placementIndex & 1) == 0 ? "right" : "left";
            task.useDirectRuntimePosition = true;
            task.directX = position.x;
            task.directY = position.y;
            task.directZ = position.z;
            task.useDirectVelocity = true;
            task.directVx = 0d;
            task.directVy = 0d;
            task.directVz = 0d;
            task.preserveActionZero = true;
            task.skipPostInitZOffset = true;

            LF2Entity spawned;
            try
            {
                spawned = objectPointFactory.CreateObjectImmediate(task);
            }
            finally
            {
                referencePool.Recycle(task);
            }

            if (spawned is not LF2Character character)
            {
                spawned?.OnTransitDestroy();
                return null;
            }
            if (!IsActive(character))
            {
                character.OnTransitDestroy();
                return null;
            }
            GameObject entityRoot = character.Renderer?.transform.parent?.gameObject;
            if (entityRoot == null)
            {
                character.OnTransitDestroy();
                return null;
            }
            entityRoot.name = string.Format(
                CultureInfo.InvariantCulture,
                "StressEntity_{0:D4}_Slot_{1:D4}_Team_{2}",
                report.totalEntitiesCreated,
                character.Runtime.SlotIndex,
                team);
            entityRoot.transform.SetParent(transform, true);
            character.AiControlled = config.InputMode == ProductionEntityStressInputMode.Ai;
            character.Renderer.ForceRefreshPresentation();
            if (world.TryGetCurrentRuntimeHandleForDiagnostics(
                    character.Runtime.SlotIndex,
                    character,
                    out RuntimeEntityHandle handle))
            {
                harnessOwnedHandles.Add(handle);
            }
            return character;
        }

        private int RemoveReleasedEntities()
        {
            long spawnOrRemoveTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope spawnOrRemoveProfilerScope =
                SpawnOrRemoveProfilerMarker.Auto();
            try
            {
                int removedCount = 0;
                for (int i = entities.Count - 1; i >= 0; i--)
                {
                    LF2Character entity = entities[i];
                    if (entity == null || IsActive(entity))
                        continue;
                    entities[i] = null;
                    report.lifecycleReplacements++;
                    removedCount++;
                }
                return removedCount;
            }
            finally
            {
                spawnOrRemoveProfilerScope.Dispose();
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion.SpawnOrRemove,
                    Stopwatch.GetTimestamp() - spawnOrRemoveTimestamp);
            }
        }

        private int CountActiveBaseRoster(out int aiCount)
        {
            int activeCount = 0;
            aiCount = 0;
            for (int i = 0; i < entities.Count; i++)
            {
                LF2Character entity = entities[i];
                if (!IsActive(entity))
                    continue;
                activeCount++;
                if (entity.AiControlled)
                    aiCount++;
            }
            return activeCount;
        }

        private int FindFirstAvailableBaseRosterIndex()
        {
            for (int i = 0; i < entities.Count; i++)
            {
                if (!IsActive(entities[i]))
                    return i;
            }
            return entities.Count;
        }

        private void RefreshRosterAndCapacityDiagnostics()
        {
            if (report == null)
                return;

            report.baseRosterActiveCount = CountActiveBaseRoster(out int aiCount);
            report.baseAiActiveCount = aiCount;
            report.totalActiveRuntimeEntityCount = CountWorldEntities();
            report.totalClaimedRuntimeSlotCount =
                world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
            report.derivedOrTemporaryActiveCount = Math.Max(
                0,
                report.totalActiveRuntimeEntityCount - report.baseRosterActiveCount);
        }

        private int GetObjectPoolCapacityForDiagnostics()
        {
            return objectPool == null
                ? 0
                : objectPool.ActiveObjectCountForAcceptance +
                  objectPool.AvailableObjectCountForAcceptance;
        }

        private void ValidatePeakPopulation()
        {
            RefreshReportCounts();
            bool populationValid =
                ProductionEntityStressPopulationPolicy.Evaluate(
                    config.EntityCount,
                    report.activeGameObjectCount,
                    report.stressRootChildCount,
                    report.worldObjectCount,
                    report.worldEntityCount,
                    report.claimedRuntimeSlotCount) &&
                world.RuntimeProfileForDiagnostics == BattleRuntimeProfile.MobileExtended &&
                world.RuntimeSlotCapacityForDiagnostics == BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity &&
                world.CollisionBroadphaseForDiagnostics == CollisionBroadphaseBackend.LooseQuadtree;
            bool shadowValid = EvaluateCollisionCandidateStoreShadowValidityForReport(
                report,
                CollisionCandidateStoreValidationPhase.PreTick);
            bool authorityValid = EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                report,
                CollisionCandidateStoreValidationPhase.PreTick);
            bool zeroItrFastPathValid =
                EvaluateCollisionRoleZeroItrFastPathValidityForReport(
                    report,
                    CollisionCandidateStoreValidationPhase.PreTick);
            bool aiSoADecisionRemainderValid =
                AreAiSoADecisionRemainderDiagnosticsValid(report);
            bool aiUnifiedSnapshotAuthorityValid =
                EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                    report,
                    terminal: false);
            report.harnessValidity = populationValid &&
                                     shadowValid &&
                                     authorityValid &&
                                     zeroItrFastPathValid &&
                                     aiSoADecisionRemainderValid &&
                                     aiUnifiedSnapshotAuthorityValid;
            if (!report.harnessValidity)
            {
                string shadowFailure = shadowValid
                    ? string.Empty
                    : BuildCollisionCandidateStoreShadowFailureReason(
                        report,
                        CollisionCandidateStoreValidationPhase.PreTick);
                string authorityFailure = authorityValid
                    ? string.Empty
                    : BuildCollisionCandidateStoreAuthorityFailureReason(
                        report,
                        CollisionCandidateStoreValidationPhase.PreTick);
                throw new InvalidOperationException(
                    !string.IsNullOrEmpty(shadowFailure)
                        ? shadowFailure
                        : !string.IsNullOrEmpty(authorityFailure)
                            ? authorityFailure
                            : "Peak population validation failed; see structured count fields in the report.");
            }
        }

        private void CleanupInternal(string reason, bool restoreDriver)
        {
            if (cleaned || cleanupInProgress)
                return;

            FinalizeProfilerFrameGcEvidence(frameBoundaryCompleted: false);
            reportWriteDeferredForProfilerFrame = false;
            cleanupInProgress = true;
            report ??= new ProductionEntityStressReport();
            var journal = new ProductionEntityStressCleanupJournal();
            try
            {
                journal.Attempt("capture-final-parity-snapshot", CaptureFinalParitySnapshot);
                journal.Attempt(
                    "disable-battle-tick-phase-timing",
                    () => world?.DisableBattleTickPhaseDiagnosticsForDiagnostics());
                journal.Attempt(
                    "disable-battle-presentation-phase-timing",
                    () => world?.DisableBattlePresentationPhaseDiagnosticsForDiagnostics());
                journal.Attempt(
                    "disable-battle-tick-detail-timing",
                    () => world?.DisableBattleTickDetailPhaseDiagnosticsForDiagnostics());
                journal.Attempt(
                    "disable-battle-ai-input-detail-timing",
                    () => world?.DisableBattleAiInputDetailDiagnosticsForDiagnostics());
                journal.Attempt(
                    "dispose-profiler-frame-gc-recorders",
                    () => profilerFrameGcCollector?.Dispose());
                report.teardown.attempted = true;
                journal.Attempt("capture-before-state", () =>
                {
                    RefreshReportCounts();
                    report.teardown.activeGameObjectsBefore = report.activeGameObjectCount;
                    report.teardown.worldObjectsBefore = world?.ObjectCount ?? 0;
                    report.teardown.worldEntitiesBefore = CountWorldEntities();
                    report.teardown.claimedSlotsBefore =
                        world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
                    report.teardown.objectPoolActiveBeforeRun = objectPoolActiveBaseline;
                    report.teardown.objectPoolAvailableBeforeRun = objectPoolAvailableBaseline;
                    report.teardown.referencePoolActiveBeforeRun = referencePoolActiveBaseline;
                });
                journal.Attempt("restore-sound-presentation-suppression", () =>
                {
                    if (!soundPresentationSuppressionStateCaptured)
                    {
                        report.soundPresentationSuppressionRestored = true;
                        return;
                    }
                    if (!RestoreSoundPresentationSuppressionForDiagnostics(
                            driver,
                            previousSoundPresentationSuppressed,
                            report,
                            dispatchedSoundEventCountBaseline,
                            suppressedSoundEventCountBaseline))
                    {
                        throw new InvalidOperationException(
                            "Sound presentation suppression diagnostic state restore failed.");
                    }
                });
                journal.Attempt("restore-skip-late-renderer-update", () =>
                {
                    if (!RestoreSkipLateRendererUpdateForDiagnostics(
                            world,
                            previousSkipLateRendererUpdate,
                            report,
                            skipLateRendererUpdateTickCountBaseline))
                    {
                        throw new InvalidOperationException(
                            "SkipLateRendererUpdate diagnostic state restore failed.");
                    }
                });
                journal.Attempt("restore-collision-candidate-store-authority", () =>
                {
                    if (!RestoreCollisionCandidateStoreAuthorityForDiagnostics(
                            collisionCandidateStoreShadowQuery,
                            previousCollisionCandidateStoreAuthorityEnabled,
                            previousCollisionCandidateStoreLegacyOracleInterval,
                            report,
                            in collisionCandidateStoreAuthorityDiagnosticsBaseline))
                    {
                        throw new InvalidOperationException(
                            BuildCollisionCandidateStoreAuthorityFailureReason(
                                report,
                                CollisionCandidateStoreValidationPhase.Teardown));
                    }
                });
                journal.Attempt("restore-collision-role-zero-itr-fast-path", () =>
                {
                    if (!RestoreCollisionRoleZeroItrFastPathForDiagnostics(
                            collisionCandidateStoreShadowQuery,
                            previousCollisionRoleZeroItrFastPathEnabled,
                            report))
                    {
                        throw new InvalidOperationException(
                            "Collision role zero-itr fast path restore failed.");
                    }
                });
                journal.Attempt("restore-collision-candidate-store-shadow", () =>
                {
                    if (!RestoreCollisionCandidateStoreShadowForDiagnostics(
                            collisionCandidateStoreShadowQuery,
                            previousCollisionCandidateStoreShadowEnabled,
                            report))
                    {
                        throw new InvalidOperationException(
                            BuildCollisionCandidateStoreShadowFailureReason(
                                report,
                                CollisionCandidateStoreValidationPhase.Teardown));
                    }
                });

                CleanupActiveRuntimeEntities(journal);
                journal.Attempt(
                    "capture-ai-decision-diagnostics",
                    () => CaptureAiDecisionSoAShadowDiagnosticsForReport(report, world));
                if (world != null)
                {
                    journal.Attempt(
                        "restore-unified-ai-snapshot-execution-mode",
                        () =>
                        {
                            world.AiUnifiedSnapshotExecutionMode =
                                previousAiUnifiedSnapshotExecutionMode;
                            report.aiUnifiedSnapshotExecutionRestored =
                                world.AiUnifiedSnapshotExecutionMode ==
                                previousAiUnifiedSnapshotExecutionMode;
                        });
                    journal.Attempt(
                        "restore-ai-decision-shadow-mode",
                        () =>
                        {
                            world.AiDecisionShadowMode = previousAiDecisionShadowMode;
                            report.aiDecisionSoAShadowRestored =
                                world.AiDecisionShadowMode == previousAiDecisionShadowMode;
                        });
                    journal.Attempt(
                        "restore-ai-decision-execution-mode",
                        () =>
                        {
                            world.AiDecisionExecutionMode = previousAiDecisionExecutionMode;
                            report.aiDecisionExecutionRestored =
                                world.AiDecisionExecutionMode == previousAiDecisionExecutionMode;
                        });
                    journal.Attempt(
                        "restore-ai-decision-oracle-interval",
                        () => world.AiDecisionIndexedCanonicalFullOracleSampleInterval =
                            previousAiDecisionFullOracleSampleInterval);
                    journal.Attempt(
                        "restore-unified-ai-snapshot-shadow-mode",
                        () =>
                        {
                            world.AiUnifiedSnapshotShadowMode =
                                previousUnifiedAiSnapshotShadowMode;
                            report.unifiedAiSnapshotShadowRestored =
                                world.AiUnifiedSnapshotShadowMode ==
                                previousUnifiedAiSnapshotShadowMode;
                        });
                }
                else
                {
                    journal.Attempt(
                        "record-missing-world-ai-diagnostics-restored",
                        () =>
                        {
                            report.aiDecisionSoAShadowRestored = true;
                            report.aiDecisionExecutionRestored = true;
                            report.unifiedAiSnapshotShadowRestored = true;
                            report.aiUnifiedSnapshotExecutionRestored = true;
                        });
                }
                journal.Attempt(
                    "restore-ai-soa-decision-remainder",
                    () => RestoreAiSoADecisionRemainderForDiagnostics(
                        world,
                        previousAiSoADecisionRemainderEnabled,
                        report));
                journal.Attempt(
                    "close-ai-execution-profile",
                    () => CloseAiExecutionProfileForDiagnostics(world));
                journal.Attempt("clear-entity-tracking", entities.Clear);

                bool afterStateCaptured = journal.Attempt("capture-after-state", () =>
                {
                    report.teardown.activeGameObjectsAfter = CountActiveEntityGameObjects();
                    report.teardown.worldObjectsAfter = world?.ObjectCount ?? 0;
                    report.teardown.worldEntitiesAfter = CountWorldEntities();
                    report.teardown.claimedSlotsAfter =
                        world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
                    report.teardown.objectPoolActiveAfter =
                        objectPool?.ActiveObjectCountForAcceptance ?? -1;
                    report.teardown.objectPoolAvailableAfter =
                        objectPool?.AvailableObjectCountForAcceptance ?? -1;
                    report.teardown.referencePoolActiveAfter = referencePool?.ActiveCount ?? -1;
                });
                report.teardown.retainedInactiveObjectPoolCapacityBeforeRun =
                    report.teardown.objectPoolAvailableBeforeRun;
                report.teardown.retainedInactiveObjectPoolCapacityAfter =
                    report.teardown.objectPoolAvailableAfter;
                report.teardown.retainedInactiveObjectPoolCapacityDelta =
                    report.teardown.retainedInactiveObjectPoolCapacityAfter -
                    report.teardown.retainedInactiveObjectPoolCapacityBeforeRun;
                report.teardown.retainedInactiveObjectPoolCapacityPolicy =
                    "Informational inactive cache capacity only; it is not active cleanup residue " +
                    "and the stress harness does not trim it.";
                report.teardown.activeStateRestored = afterStateCaptured &&
                    ProductionEntityStressTeardownPolicy.IsRestored(
                        report.teardown.activeGameObjectsAfter,
                        report.teardown.worldObjectsAfter,
                        report.teardown.worldEntitiesAfter,
                        report.teardown.claimedSlotsAfter,
                        report.teardown.objectPoolActiveAfter,
                        report.teardown.objectPoolAvailableAfter,
                        report.teardown.referencePoolActiveAfter,
                        objectPoolActiveBaseline,
                        objectPoolAvailableBaseline,
                        referencePoolActiveBaseline);

                report.teardown.driverStateRestored = !driverConfigurationChanged;
                if (driverConfigurationChanged && restoreDriver && driver != null)
                {
                    bool recreated = journal.Attempt("restore-driver-world", () =>
                    {
                        driver.RecreateWorld();
                        world = driver.World;
                    });
                    bool settingsApplied = previousSettings == null ||
                                           journal.Attempt(
                                               "restore-driver-settings",
                                               () => driver.ApplySettings(previousSettings));
                    bool pauseRestored = journal.Attempt(
                        "restore-driver-pause",
                        () => driver.SetPaused(previousPaused));
                    bool stateVerified = journal.Attempt("verify-driver-state", () =>
                    {
                        if (driver.World == null || driver.IsPaused != previousPaused ||
                            !SettingsMatch(driver.Settings, previousSettings))
                        {
                            throw new InvalidOperationException(
                                "SimulationTickDriver did not match its captured pre-run state.");
                        }
                    });
                    report.teardown.driverStateRestored =
                        recreated && settingsApplied && pauseRestored && stateVerified;
                }
                else if (driverConfigurationChanged)
                {
                    journal.Attempt("restore-driver-state", () =>
                    {
                        throw new InvalidOperationException(
                            restoreDriver
                                ? "SimulationTickDriver was unavailable during cleanup."
                                : "SimulationTickDriver restoration was not permitted in the current lifecycle state.");
                    });
                }

                journal.Attempt("restore-logger", () => loggingPolicy.Restore(report.loggingPolicy));
                report.teardown.loggingStateRestored = report.loggingPolicy.restored;
                report.teardown.cleanupExceptionCount = journal.FailureCount;
                report.teardown.cleanupExceptions = journal.FormatFailures();
                bool collisionCandidateStoreShadowValid =
                    EvaluateCollisionCandidateStoreShadowValidityForReport(
                        report,
                        CollisionCandidateStoreValidationPhase.Teardown);
                bool collisionCandidateStoreAuthorityValid =
                    EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                        report,
                        CollisionCandidateStoreValidationPhase.Teardown);
                bool collisionRoleZeroItrFastPathValid =
                    EvaluateCollisionRoleZeroItrFastPathValidityForReport(
                        report,
                        CollisionCandidateStoreValidationPhase.Teardown);
                report.teardown.restored = report.teardown.activeStateRestored &&
                                           report.teardown.driverStateRestored &&
                                           report.teardown.loggingStateRestored &&
                                           report.skipLateRendererUpdateRestored &&
                                           report.soundPresentationSuppressionRestored &&
                                           report.teardown.cleanupExceptionCount == 0 &&
                                           collisionCandidateStoreShadowValid &&
                                           collisionCandidateStoreAuthorityValid &&
                                           collisionRoleZeroItrFastPathValid;
                report.teardown.evidence =
                    ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                journal.Attempt("refresh-report-metrics", RefreshReportMetrics);

                if (journal.FailureCount != report.teardown.cleanupExceptionCount)
                {
                    report.teardown.cleanupExceptionCount = journal.FailureCount;
                    report.teardown.cleanupExceptions = journal.FormatFailures();
                    report.teardown.restored = false;
                    report.teardown.evidence =
                        ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                }

                report.status = ProductionEntityStressRunStatusPolicy.ResolveCleanupStatus(
                    report.status,
                    reason,
                    report.teardown.restored);
                journal.Attempt("write-report", WriteReport);
                if (journal.FailureCount != report.teardown.cleanupExceptionCount)
                {
                    report.teardown.cleanupExceptionCount = journal.FailureCount;
                    report.teardown.cleanupExceptions = journal.FormatFailures();
                    report.teardown.restored = false;
                    report.teardown.evidence =
                        ProductionEntityStressTeardownPolicy.BuildEvidence(reason, report.teardown);
                }
            }
            finally
            {
                cleaned = true;
                cleanupInProgress = false;
                if (ReferenceEquals(Active, this))
                    Active = null;
            }
        }

        private void OnDestroy()
        {
            bool interruptedWithoutTerminalResult = !cleaned;
            if (!cleaned)
                CleanupInternal("runner-destroyed", restoreDriver: Application.isPlaying);
            if (ReferenceEquals(Active, this))
                Active = null;
            if (!preserveRequestProcessorStateOnDestroy)
            {
                if (interruptedWithoutTerminalResult)
                {
                    ProductionEntityStressPaths.WriteTerminalResult(
                        false,
                        config.OutputPath,
                        report?.teardown?.evidence ??
                        "Production stress runner was destroyed before terminal cleanup.");
                }
                ProductionEntityStressRequestProcessor.NotifyRunStopped();
            }
        }

        private void RefreshReportCounts()
        {
            if (report == null)
                return;
            CaptureSkipLateRendererUpdateForReport(
                report,
                world,
                skipLateRendererUpdateTickCountBaseline);
            if (soundPresentationSuppressionStateCaptured)
            {
                CaptureSoundPresentationSuppressionForReport(
                    report,
                    driver,
                    dispatchedSoundEventCountBaseline,
                    suppressedSoundEventCountBaseline);
            }
            CaptureCollisionCandidateStoreShadowDiagnosticsForReport(
                report,
                collisionCandidateStoreShadowQuery);
            EvaluateCollisionCandidateStoreShadowValidityForReport(
                report,
                CollisionCandidateStoreValidationPhase.PreTick);
            CaptureCollisionCandidateStoreAuthorityDiagnosticsForReport(
                report,
                collisionCandidateStoreShadowQuery,
                in collisionCandidateStoreAuthorityDiagnosticsBaseline);
            EvaluateCollisionCandidateStoreAuthorityValidityForReport(
                report,
                CollisionCandidateStoreValidationPhase.PreTick);
            CaptureCollisionRoleZeroItrFastPathDiagnosticsForReport(
                report,
                collisionCandidateStoreShadowQuery);
            EvaluateCollisionRoleZeroItrFastPathValidityForReport(
                report,
                CollisionCandidateStoreValidationPhase.PreTick);
            report.activeGameObjectCount = CountActiveEntityGameObjects();
            report.stressRootChildCount = transform != null ? transform.childCount : 0;
            report.worldObjectCount = world?.ObjectCount ?? 0;
            report.worldEntityCount = CountWorldEntities();
            report.claimedRuntimeSlotCount = world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0;
            report.baseRosterActiveCount = CountActiveBaseRoster(out int baseAiCount);
            report.baseAiActiveCount = baseAiCount;
            report.totalActiveRuntimeEntityCount = report.worldEntityCount;
            report.totalClaimedRuntimeSlotCount = report.claimedRuntimeSlotCount;
            report.derivedOrTemporaryActiveCount = Math.Max(
                0,
                report.totalActiveRuntimeEntityCount - report.baseRosterActiveCount);
            if (world != null)
            {
                CaptureAiDecisionSoAShadowDiagnosticsForReport(report, world);
                report.aiSoASensingShadowQueryCount = Math.Max(
                    report.aiSoASensingShadowQueryCount,
                    world.AiSoASensingShadowQueryCountForDiagnostics);
                report.aiSoASensingShadowInvalidationCount = Math.Max(
                    report.aiSoASensingShadowInvalidationCount,
                    world.AiSoASensingShadowInvalidationCountForDiagnostics);
                report.aiSoASensingShadowPurityMismatchCount = Math.Max(
                    report.aiSoASensingShadowPurityMismatchCount,
                    world.AiSoASensingShadowPurityMismatchCountForDiagnostics);
                report.aiSoASensingShadowInitialMismatchCount = Math.Max(
                    report.aiSoASensingShadowInitialMismatchCount,
                    world.AiSoASensingShadowInitialMismatchCountForDiagnostics);
                report.aiSoASensingShadowCachedMismatchCount = Math.Max(
                    report.aiSoASensingShadowCachedMismatchCount,
                    world.AiSoASensingShadowCachedMismatchCountForDiagnostics);
                report.aiSoASensingShadowPostSpecialMismatchCount = Math.Max(
                    report.aiSoASensingShadowPostSpecialMismatchCount,
                    world.AiSoASensingShadowPostSpecialMismatchCountForDiagnostics);
                report.aiSoASensingShadowMismatchMask |=
                    world.AiSoASensingShadowMismatchMaskForDiagnostics;
                report.aiSoASensingShadowLastMismatchMask =
                    world.AiSoASensingShadowLastMismatchMaskForDiagnostics;
                report.aiSoASensingShadowComparisonPublished |=
                    world.AiSoASensingShadowComparisonPublishedForDiagnostics;
                if (string.IsNullOrEmpty(report.aiSoASensingShadowFirstMismatch))
                {
                    report.aiSoASensingShadowFirstMismatch = FormatAiSensingFirstMismatch(
                        world.AiSoASensingShadowFirstMismatchForDiagnostics);
                }
                AggregateAiSoACandidateDiagnosticsForReport(
                    report,
                    world.AiSoACandidateNearestQueryCountForDiagnostics,
                    world.AiSoACandidateSpecialQueryCountForDiagnostics,
                    world.AiSoACandidateGroundXRowVisitCountForDiagnostics,
                    world.AiSoACandidateAirXRowVisitCountForDiagnostics,
                    world.AiSoACandidateSpecialSlotVisitCountForDiagnostics,
                    world.AiSoACandidateLegacyNearestScanCountForDiagnostics,
                    world.AiSoACandidateLegacySpecialScanCountForDiagnostics,
                    world.AiSoACandidatePreRandomFailureCountForDiagnostics,
                    world.AiSoACandidatePostRandomFailureCountForDiagnostics);
                AggregateAiLegacyDiagnosticsForReport(
                    report,
                    world.AiLegacyNearestFactsBuildCountForDiagnostics,
                    world.AiLegacySnapshotIndexBuildCountForDiagnostics,
                    world.AiLegacyQuadtreeSyncCountForDiagnostics,
                    world.AiLegacySnapshotMutationCountForDiagnostics);
                if (config.AiSensingMode == AiSensingMode.SoAAiSensing &&
                    !AreAiSoACandidateFallbackDiagnosticsClean(report))
                {
                    report.harnessValidity = false;
                }
                if (!AreAiLegacyDiagnosticsValidForMode(
                        report,
                        config.AiSensingMode))
                {
                    report.harnessValidity = false;
                }
                if (!AreAiSoADecisionRemainderDiagnosticsValid(report))
                    report.harnessValidity = false;
                EvaluateAiDecisionShadowValidityForReport(
                    report,
                    ShouldEvaluateAiDecisionShadowAsTerminalForReport(report));
                EvaluateAiUnifiedSnapshotAuthorityValidityForReport(
                    report,
                    ShouldEvaluateAiDecisionShadowAsTerminalForReport(report));
                EvaluateAiUnifiedSnapshotRollbackContractForReport(
                    report,
                    ShouldEvaluateAiDecisionShadowAsTerminalForReport(report));
            }
            if (!report.teardown.attempted)
            {
                report.runtimeProfile = world?.RuntimeProfileForDiagnostics.ToString() ?? string.Empty;
                report.runtimeSlotCapacity = world?.RuntimeSlotCapacityForDiagnostics ?? 0;
                report.broadphaseBackend = world?.CollisionBroadphaseForDiagnostics.ToString() ?? string.Empty;
            }
        }

        private void RefreshReportMetrics()
        {
            RefreshReportCounts();
            logicTickTimingCollector.PopulateReport(report);
            report.unityFrameMilliseconds = ProductionEntityStressStatistics.Summarize(
                frameSamples,
                "ms",
                "Time.unscaledDeltaTime for visible Play Mode frames");
            report.logicTickAllocatedBytes = ProductionEntityStressStatistics.Summarize(
                allocationSamples,
                "bytes",
                "GC.GetAllocatedBytesForCurrentThread around production logic tick");
            profilerFrameGcCollector?.PopulateReport(report);
            if (config.EnablePhaseTiming)
                phaseTimingCollector?.PopulateReport(report);
            else
                ProductionEntityStressPhaseTimingCollector.PopulateDisabledReport(report);
            if (config.EnablePresentationTiming)
                presentationTimingCollector?.PopulateReport(report);
            else
                ProductionEntityStressPresentationTimingCollector.PopulateDisabledReport(report);
            detailPhaseTimingCollector.PopulateReport(report);
            report.updatedUtc = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        }

        private void WriteReport()
        {
            if (profilerFrameGcCollector?.Active == true)
            {
                reportWriteDeferredForProfilerFrame = true;
                return;
            }

            long writeReportTimestamp = Stopwatch.GetTimestamp();
            ProfilerMarker.AutoScope writeReportProfilerScope =
                WriteReportProfilerMarker.Auto();
            try
            {
                if (report == null || string.IsNullOrWhiteSpace(config.OutputPath))
                    return;

                long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                try
                {
                    RefreshReportMetrics();
                    string directory = Path.GetDirectoryName(config.OutputPath);
                    if (!string.IsNullOrEmpty(directory))
                        Directory.CreateDirectory(directory);
                    File.WriteAllText(
                        config.OutputPath,
                        JsonUtility.ToJson(report, true),
                        new UTF8Encoding(false));
                }
                finally
                {
                    // The completed write is published by the next report write; rewriting here
                    // would recursively measure another JSON serialization and file write.
                    RecordAllocationBytesForReport(
                        report,
                        ProductionEntityStressAllocationRegion.WriteReport,
                        GC.GetAllocatedBytesForCurrentThread() - allocatedBefore);
                }
            }
            finally
            {
                writeReportProfilerScope.Dispose();
                RecordCpuElapsedTicksForReport(
                    report,
                    ProductionEntityStressCpuRegion.WriteReport,
                    Stopwatch.GetTimestamp() - writeReportTimestamp);
            }
        }

        private int CountActiveEntityGameObjects()
        {
            return ProductionEntityStressTeardownPolicy.CountActiveStressRootGameObjects(transform);
        }

        private void CaptureFinalParitySnapshot()
        {
            if (world == null || !driverConfigurationChanged || report == null)
                return;

            IBattleChecksumSnapshot snapshot;
            switch (world.RuntimeProfileForDiagnostics)
            {
                case BattleRuntimeProfile.Authority400:
                    snapshot = world.CaptureParityFrameSnapshot(report.logicTicksExecuted);
                    break;
                case BattleRuntimeProfile.MobileExtended:
                case BattleRuntimeProfile.DesktopExtended:
                    snapshot = world.CaptureExtendedChecksumSnapshot(report.logicTicksExecuted);
                    break;
                default:
                    throw new InvalidOperationException(
                        $"Unsupported stress runtime profile '{world.RuntimeProfileForDiagnostics}'.");
            }
            ProductionEntityStressParityReport.Populate(report, snapshot);
            BattleLockstepChecksumSnapshot lockstepSnapshot =
                world.CaptureLockstepChecksumSnapshot(report.logicTicksExecuted);
            ProductionEntityStressParityReport.PopulateLockstep(report, lockstepSnapshot);
            if (config.WriteFinalParitySnapshotJson)
            {
                string snapshotPath = Path.GetFullPath(config.OutputPath + ".final-checksum.json");
                Directory.CreateDirectory(
                    Path.GetDirectoryName(snapshotPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
                File.WriteAllText(snapshotPath, snapshot.ToJson(), new UTF8Encoding(false));
                report.finalParitySnapshotJsonPath = snapshotPath;
            }
        }

        private static string FormatAiSensingFirstMismatch(
            AiSoASensingShadowMismatch mismatch)
        {
            if (mismatch.Kind == AiSoASensingShadowMismatchKind.None)
                return string.Empty;

            return string.Format(
                CultureInfo.InvariantCulture,
                "kind={0};selfSlot={1};expectedSelection={2};actualSelection={3};" +
                "expectedValue={4};actualValue={5};expectedFlags={6};actualFlags={7}",
                mismatch.Kind,
                mismatch.SelfSlot,
                mismatch.ExpectedSelection,
                mismatch.ActualSelection,
                mismatch.ExpectedValue,
                mismatch.ActualValue,
                mismatch.ExpectedFlags,
                mismatch.ActualFlags);
        }

        private int CountWorldEntities()
        {
            if (world == null)
                return 0;
            world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch);
            return entityScratch.Count;
        }

        private void ObserveRuntimeEntitySnapshot()
        {
            if (world == null)
                return;

            world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch);
            report.peakWorldEntityCount = Math.Max(report.peakWorldEntityCount, entityScratch.Count);
            for (int i = 0; i < entityScratch.Count; i++)
            {
                LF2Entity entity = entityScratch[i];
                if (entity?.Runtime == null ||
                    !world.TryGetCurrentRuntimeHandleForDiagnostics(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    continue;
                }

                if (ProductionEntityStressDerivedObservationPolicy.TryRecord(
                        handle,
                        harnessOwnedHandles,
                        observedDerivedHandles))
                {
                    report.observedOpointCreates++;
                }
            }
        }

        private void CleanupActiveRuntimeEntities(ProductionEntityStressCleanupJournal journal)
        {
            if (world == null)
                return;

            int previousRemaining = -1;
            for (int pass = 0; pass < MaximumCleanupPasses; pass++)
            {
                bool captured = journal.Attempt(
                    $"capture-world-entities-pass-{pass}",
                    () => world.GetActiveRuntimeEntitySnapshotForDiagnostics(entityScratch));
                if (!captured)
                    break;

                int remaining = entityScratch.Count;
                if (remaining == 0 || remaining >= previousRemaining && previousRemaining >= 0)
                    break;

                previousRemaining = remaining;
                for (int i = 0; i < entityScratch.Count; i++)
                {
                    LF2Entity entity = entityScratch[i];
                    if (entity == null)
                        continue;
                    journal.Attempt(
                        $"release-world-entity-pass-{pass}-index-{i}",
                        entity.OnTransitDestroy);
                }

                journal.Attempt(
                    $"flush-world-destroy-pass-{pass}",
                    world.FlushPendingDestroyForDiagnostics);
            }

            journal.Attempt("flush-world-destroy-final", world.FlushPendingDestroyForDiagnostics);
        }

        private static bool SettingsMatch(
            LockstepSimulationSettings current,
            LockstepSimulationSettings expected)
        {
            if (current == null || expected == null)
                return current == expected;

            return current.driveMode == expected.driveMode &&
                   current.useUnscaledTime == expected.useUnscaledTime &&
                   current.maxCatchUpTicksPerFrame == expected.maxCatchUpTicksPerFrame &&
                   current.maxBacklogTicks == expected.maxBacklogTicks &&
                   current.inputDelayTicks == expected.inputDelayTicks &&
                   current.requireInputFrameReady == expected.requireInputFrameReady &&
                   current.enableFrameChecksum == expected.enableFrameChecksum;
        }

        private static bool TrySelectLoadedCharacter(
            CharacterAnimtorManager manager,
            GameDataManager dataManager,
            out int oid)
        {
            List<int> ids = manager.GetAllLoadedCharacterIds();
            ids.Sort();
            for (int i = 0; i < ids.Count; i++)
            {
                int candidate = ids[i];
                ObjectDefinition definition = dataManager.GetObjectById(candidate);
                LF2CharacterDataWrapper config = manager.GetCharacterConfig(candidate);
                if (definition == null || definition.type != (int)LF2ObjectType.Character ||
                    config?.characterData?.frames == null || config.characterData.frames.Count == 0 ||
                    !manager.TryGetSprites(candidate, out List<Sprite> sprites) || sprites == null ||
                    sprites.Count == 0)
                {
                    continue;
                }
                oid = candidate;
                return true;
            }
            oid = -1;
            return false;
        }

        private static LockstepSimulationSettings CloneSettings(LockstepSimulationSettings source)
        {
            if (source == null)
                return new LockstepSimulationSettings();
            return new LockstepSimulationSettings
            {
                driveMode = source.driveMode,
                useUnscaledTime = source.useUnscaledTime,
                maxCatchUpTicksPerFrame = source.maxCatchUpTicksPerFrame,
                maxBacklogTicks = source.maxBacklogTicks,
                inputDelayTicks = source.inputDelayTicks,
                requireInputFrameReady = source.requireInputFrameReady,
                enableFrameChecksum = source.enableFrameChecksum,
            };
        }

        private static bool IsActive(LF2Character entity)
        {
            return entity != null && entity.Renderer != null && entity.Runtime != null &&
                   entity.Runtime.SlotIndex >= 0;
        }

        private static int Sum(int[] values)
        {
            if (values == null)
                return 0;
            long sum = 0;
            for (int i = 0; i < values.Length; i++)
                sum += values[i];
            return sum > int.MaxValue ? int.MaxValue : (int)sum;
        }

        private static void AddRollingSample(List<double> samples, double value)
        {
            if (samples.Count >= MaximumRetainedSamples)
                samples.RemoveAt(0);
            samples.Add(value);
        }
    }

    internal static class ProductionEntityStressPaths
    {
        internal const string RequestFile = "Temp/NTSD_ProductionEntityStress.request.json";
        internal const string ResultFile = "Temp/NTSD_ProductionEntityStress.result";

        internal static string ProjectRoot =>
            Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

        internal static string ProjectPath(string path)
        {
            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(ProjectRoot, path));
        }

        internal static void WriteTerminalResult(bool success, string reportPath, string evidence)
        {
            string path = ProjectPath(ResultFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            string content = (success ? "PASS" : "FAIL") + "\n" +
                             (reportPath ?? string.Empty) + "\n" +
                             (evidence ?? string.Empty);
            File.WriteAllText(path, content, new UTF8Encoding(false));
        }
    }
}
#endif
