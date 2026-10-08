#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Animation.Rendering.Editor;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class BattleOptimizationWindowsAiSuiteEditor
    {
        private const string SessionKey = "NTSD.Optimization.Batch26.WindowsAiSuite";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string OutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH26-H07-CONTINUE-20261007/windows-01";
        private const string RoleCandidateOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH29-ROLE-COLLECTOR-ADMISSION-20261007/windows-01";
        private const string RoleFormalOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH30-ROLE-FORMAL-WINDOWS-20261007/windows-01";
        private const string PairSnapshotOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007/windows-01";
        private const string EmptyItrGuardOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH32-BRUTE-EMPTY-ITR-GUARD-20261007/windows-01";
        private const string EmptyItrRosterOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH34-BRUTE-EMPTY-ITR-ROSTER-20261007/windows-01";
        private const string BruteExactCacheOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH36-BRUTE-EXACT-CACHE-20261007/windows-01";
        private const string BruteGeometryFirstOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH37-BRUTE-GEOMETRY-FIRST-20261007/windows-01";
        private const string CpuGcCaptureOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH39-CPU-GC-CAPTURE-20261007/windows-01";
        private const string BruteProductionOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH40-BRUTE-PRODUCTION-ADMISSION-20261007/windows-01";
        private const string BruteBranchTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH44-BRUTE-BRANCH-TIMING-20261007/windows-01";
        private const string BruteSampledTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH45-BRUTE-SAMPLED-TIMING-20261007/windows-01";
        private const string BruteKind5PresenceOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH38-BRUTE-KIND5-PRESENCE-20261007/windows-01";
        private const string BruteEligibilityReuseOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH50-ELIGIBILITY-WINDOWS-20261007/windows-01";
        private const string BruteCombinedCacheOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH52-COMBINED-CACHE-WINDOWS-20261008/windows-01";
        private const string LogicGcScopeOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH54-LOGIC-GC-SCOPE-20261008/windows-01";
        private const string LogicCallsiteOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH55-LOGIC-GC-CALLSITE-20261008/windows-01";
        private const string LabelPrewarmOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH57-LABEL-PREWARM-WINDOWS-20261008/windows-01";
        private const string BruteCoarseEnvelopeOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH60-COARSE-ENVELOPE-WINDOWS-20261008/windows-01";
        private const string BruteEnvelopeBindingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH63-ENVELOPE-BINDING-WINDOWS-20261008/windows-01";
        private const string BruteEnvelopeBindingEligibilityOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH64-ENVELOPE-BINDING-ELIGIBILITY-20261008/windows-01";
        private const string BruteEnvelopeBranchTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH65-ENVELOPE-PATH-BRANCH-TIMING-20261008/windows-01";
        private const string RenderTextureBindingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008/windows-01";
        private const string SharedRequest = "Temp/NTSD_ProductionEntityStress.request.json";
        private const string BruteOrdinalPacketOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH72-ORDINAL-PACKET-WINDOWS-20261008/windows-01";
        private const string SharedResult = "Temp/NTSD_ProductionEntityStress.result";
        private const string CentralSegmentReadbackOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008/windows-02";
        private static bool startingCentralSegmentReadback;
        private static RunState centralSegmentReadbackRun;
        private static readonly Type ConfigType = typeof(ProductionEntityStressRunner).Assembly.GetType(
            "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
        private static readonly MethodInfo FromRequest = ConfigType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo StartRun = typeof(ProductionEntityStressRunner).GetMethod("StartRun", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ServicesReady = typeof(ProductionEntityStressRunner).GetMethod("AreProductionServicesReady", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo StopRun = typeof(ProductionEntityStressRunner).GetMethod("StopAndCleanup", BindingFlags.Instance | BindingFlags.NonPublic);
        private static SuiteState state;
        private static ProductionEntityStressReport currentReport;
        private static double nextObservation;
        private static double nextProgress;
        private static int progressOrdinal;
        private static BruteForceSceneQuery emptyItrGuardQuery;
        private static BruteForceSceneQuery productionQuery;
        private static BruteForceSceneQuery branchTimingQuery;
        private static bool previousBranchTiming;
        private static int previousBranchTimingSampleStride = 1;
        private static bool previousEmptyItrGuard;
        private static bool previousEmptyItrRoster;
        private static bool previousBruteExactCache;
        private static bool previousBruteGeometryFirst;
        private static BruteForceSceneQuery kind5PresenceQuery;
        private static bool previousKind5Presence;
        private static BruteForceSceneQuery eligibilityReuseQuery;
        private static bool previousEligibilityReuse;
        private static bool startingCoarseEnvelopeWindow;
        private static BruteForceSceneQuery coarseEnvelopeQuery;
        private static bool previousCoarseEnvelope;
        private static bool startingEnvelopeBindingWindow;
        private static BruteForceSceneQuery envelopeBindingQuery;
        private static bool previousEnvelopeBindingEnvelope;
        private static bool previousEnvelopeBindingReuse;
        private static bool startingEnvelopeBindingEligibilityWindow;
        private static bool envelopeBindingEligibilityOwner;
        private static bool previousEnvelopeBindingEligibility;
        private static bool startingEnvelopeBranchTimingWindow;
        private static bool envelopeBindingBranchTimingOwner;
        private static bool previousEnvelopeBranchTiming;
        private static bool startingRenderTextureBindingWindow;
        private static bool renderTextureBindingReuseOwner;
        private static bool previousRenderTextureBindingReuse;
        private static RunState renderTextureBindingReuseRun;
        private static long lastRenderTextureBindingExecuteSequence;
        private static readonly string RenderTextureBindingOwnerError =
            "The renderer texture binding window requires exclusive ownership and a default-OFF flag.";
        private static readonly string RenderTextureBindingDriftError =
            "The owned renderer texture binding flag drifted during the fixed window.";
        private static readonly string RenderTextureBindingObservationError =
            "The fixed window requires fresh valid body recordings from its selected camera.";
        private static readonly string EnvelopeBindingModeError =
            "The envelope binding window requires production defaults, calibrated scope and exclusive ownership without another capture or candidate.";
        private static readonly string EnvelopeBindingOwnerError =
            "The envelope binding comparison requires an unowned query with four production defaults and both candidate flags initially off.";
        private static readonly string EnvelopeBindingDriftError =
            "The owned envelope or binding flag, production defaults or excluded candidate drifted during the fixed window.";
        private static readonly string CoarseEnvelopeModeError =
            "The coarse envelope window requires unchanged production defaults, calibrated scope and no other capture or candidate.";
        private static readonly string CoarseEnvelopeOwnerError =
            "The coarse envelope comparison requires an unowned query, four production defaults and no other candidate or timing flag.";
        private static readonly string CoarseEnvelopeDriftError =
            "The owned coarse envelope flag or production defaults drifted during the fixed window.";
        private static BattleLogicTickGcObserverEditor logicGcObserver;
        private static bool startingOrdinalPacketWindow;
        private static BruteForceSceneQuery ordinalPacketQuery;
        private static RunState ordinalPacketRun;
        private static bool previousOrdinalPacket;

        [Serializable]
        private sealed class RunState
        {
            public bool centralSegmentReadbackCaptured;
            public bool centralSegmentReadbackDetached;
            public string centralSegmentReadbackError;
            public string action;
            public int targetSamples;
            public string reportPath;
            public string terminalStatus;
            public int sampledTicks;
            public int warmupTicks;
            public int minimumObservedActiveAi = int.MaxValue;
            public int minimumObservedBaseRoster = int.MaxValue;
            public int observedSampleWindows;
            public int lastObservedSampleTick;
            public bool workloadValid;
            public bool retainedFailedGcMeasurement;
            public bool zeroGcPassed;
            public bool zeroGcRawGatePassed;
            public string zeroGcEvidenceStatus = "UNCALIBRATED_COUNTER / UNKNOWN";
            public BattleLogicTickGcObserverEditor.Evidence logicGcEvidence;
            public bool harnessValidity;
            public bool teardownRestored;
            public string failure;
            public bool emptyItrGuardApplied;
            public bool emptyItrGuardRestored;
            public bool emptyItrRosterApplied;
            public bool emptyItrRosterRestored;
            public bool emptyItrRosterObservedApplied;
            public bool emptyItrRosterFallbackObserved;
            public int emptyItrRosterMaximumBuildCount;
            public int emptyItrRosterCapacity;
            public long emptyItrRosterPayloadBytes;
            public long emptyItrRosterMaximumVisitedPairs;
            public long emptyItrRosterMaximumSkippedPairs;
            public bool bruteExactCacheApplied;
            public bool bruteExactCacheRestored;
            public bool bruteExactCacheObservedApplied;
            public bool bruteExactCacheFallbackObserved;
            public int bruteExactCacheMaximumBuildCount;
            public long bruteExactCacheMaximumDirections;
            public int bruteExactParticipantCapacity;
            public int bruteExactBodyCapacity;
            public int bruteExactItrCapacity;
            public long bruteExactCacheAppliedBaseline;
            public long bruteExactCacheFallbackBaseline;
            public long bruteExactCacheAppliedDelta;
            public long bruteExactCacheFallbackDelta;
            public bool bruteGeometryFirstApplied;
            public bool bruteGeometryFirstRestored;
            public long bruteGeometryFirstAppliedBaseline;
            public long bruteGeometryFirstAppliedDelta;
            public long bruteGeometryFirstMaximumRejectCount;
            public bool bruteProductionDefaultsObserved;
            public bool bruteProductionDefaultsUnchanged;
            public bool bruteBranchTimingEnabled;
            public bool bruteBranchTimingFlagApplied;
            public bool bruteBranchTimingRestored;
            public int bruteBranchTimingSampleStride;
            public BruteForceSceneQuery.BruteBranchTimingCoverage bruteBranchTimingCoverage;
            public string bruteBranchTimingCoverageScope;
            public bool bruteKind5PresenceFlagApplied;
            public bool bruteKind5PresenceEnabled;
            public bool bruteKind5PresenceRestored;
            public long bruteKind5PresenceAppliedBaseline;
            public long bruteKind5PresenceAppliedDelta;
            public long bruteKind5PresenceMaximumSkippedScans;
            public bool bruteEligibilityReuseEnabled;
            public bool bruteEligibilityReuseFlagApplied;
            public bool bruteEligibilityReuseRestored;
            public long bruteEligibilityReuseAppliedBaseline;
            public long bruteEligibilityReuseAppliedDelta;
            public bool bruteCoarseEnvelopeEnabled;
            public bool bruteCoarseEnvelopeFlagApplied;
            public bool bruteCoarseEnvelopeFlagUnchanged;
            public bool bruteCoarseEnvelopeRestored;
            public long bruteCoarseEnvelopeMaximumObservedDirections;
            public long bruteCoarseEnvelopeMaximumObservedRejects;
            public bool bruteEnvelopeBindingPreviousEnvelope;
            public bool bruteEnvelopeBindingPreviousReuse;
            public bool bruteRejectedBindingReuseEnabled;
            public bool bruteRejectedBindingReuseFlagApplied;
            public bool bruteRejectedBindingReuseFlagUnchanged;
            public bool bruteRejectedBindingReuseRestored;
            public bool bruteRejectedBindingReuseObservedApplied;
            public long bruteRejectedBindingMaximumObservedProbes;
            public long bruteRejectedBindingMaximumObservedReuses;
            public bool bruteEnvelopeBindingEligibilityMode;
            public bool bruteEnvelopeBindingPreviousEligibility;
            public bool bruteEligibilityReuseFlagUnchanged;
            public bool bruteEligibilityReuseObservedApplied;
            public bool bruteEnvelopeBranchTimingMode;
            public bool bruteEnvelopeBranchTimingFlagApplied;
            public bool bruteEnvelopeBranchTimingFlagUnchanged;
            public bool bruteEnvelopeBranchTimingRestored;
            public bool bruteBranchTimingFlagUnchanged;
            public bool renderTextureBindingReuseEnabled;
            public bool renderTextureBindingReuseFlagApplied;
            public bool renderTextureBindingReuseFlagUnchanged;
            public bool renderTextureBindingReuseRestored;
            public int renderTextureBindingAcceptedCameraSamples;
            public int renderTextureBindingCameraId;
            public long renderTextureBindingBodyPrepareCount;
            public long renderTextureBindingBodyReuseCount;
            public bool bruteOrdinalPacketEnabled;
            public bool bruteOrdinalPacketFlagApplied;
            public bool bruteOrdinalPacketFlagUnchanged;
            public bool bruteOrdinalPacketRestored;
            public bool bruteOrdinalPacketObservedApplied;
            public bool bruteOrdinalPacketFallbackObserved;
            public bool bruteOrdinalPacketCapacityStable;
            public int bruteOrdinalPacketCapacityBeforePopulation;
            public int bruteOrdinalPacketSteadyCapacity;
            public int bruteOrdinalPacketMaximumObservedCapacity;
            public int bruteOrdinalPacketLastObservedTick;
            public int bruteOrdinalPacketObservedSamples;
            public long bruteOrdinalPacketMaximumObservedRejects;
            public long bruteOrdinalPacketMaximumObservedProofs;
        }

        [Serializable]
        private sealed class CentralSegmentReadbackRow
        {
            public int index, chunk, subMesh, firstCommand, commands, firstQuad, quads;
            public string bindingMode, textureName, textureType, textureAssetPath;
            public int boundTextureIdentity, SourceTextureIdentity, AtlasPageIdentity, TextureArrayIdentity;
            public int slice, pageIndex, materialIdentity, variant, shaderIdentity, shaderPassCount, renderQueue;
            public string materialName, shaderName, shaderKeywords;
            public int breakMaskFromPrevious;
        }

        [Serializable]
        private sealed class CentralSegmentReadbackSnapshot
        {
            public string scope = "One CPU submission readback; not GPU batch, FPS gain or complete zero-GC evidence.";
            public string utc, drawMode, drawPolicyRequested, drawPolicyEffective, drawPolicyReason;
            public int tick, generation, cameraIdentity, sampledLogicTicks, activeAi;
            public bool atlasInputsKnown, cpuLeaseReleased;
            public string atlasRequested, atlasEffective, atlasReason, catalogMode, atlasDiagnostic;
            public int plannedPageCount, sourceCommands, resolvedCommands, unresolvedCommands;
            public int unsupportedCategories, unsupportedStates, chunks, segments;
            public long estimatedAtlasBytes, atlasBudgetBytes;
            public CentralSegmentReadbackRow[] rows;
        }

        [Serializable]
        private sealed class SuiteState
        {
            public string phase;
            public string status = "IN_PROGRESS";
            public string error;
            public string outputRoot = OutputRoot;
            public bool roleCandidate;
            public bool roleFormalCandidate;
            public bool pairSnapshotCandidate;
            public bool emptyItrGuardCandidate;
            public bool emptyItrRosterCandidate;
            public bool bruteExactCacheCandidate;
            public bool bruteGeometryFirstCandidate;
            public bool cpuGcCaptureOnly;
            public bool bruteProductionOnly;
            public bool bruteBranchTimingOnly;
            public bool bruteBranchTimingSampled;
            public bool bruteKind5PresenceCandidate;
            public bool bruteEligibilityReuseCandidate;
            public bool bruteCombinedCacheCandidate;
            public bool logicGcScopeOnly;
            public bool logicGcCallsiteOnly;
            public bool labelPrewarmValidation;
            public bool bruteCoarseEnvelopeCandidate;
            public bool bruteEnvelopeBindingCandidate;
            public bool bruteEnvelopeBindingEligibilityCandidate;
            public bool bruteEnvelopeBranchTimingCandidate;
            public bool renderTextureBindingCandidate;
            public bool bruteOrdinalPacketCandidate;
            public bool centralSegmentReadbackOnly;
            public string originalScene;
            public string originalSceneHash;
            public string battleSceneHash;
            public bool originalSuppression;
            public double deadline;
            public int runIndex;
            public int completedRuns;
            public int retainedFailedGcMeasurements;
            public RunState[] runs = new RunState[6];
            public bool orderedShutdown;
            public int remainingObjects;
            public int remainingSlots;
            public int remainingBorrowers;
            public bool battleSceneUnchanged;
            public bool originalSceneRestored;
            public string updatedUtc;
        }

        static BattleOptimizationWindowsAiSuiteEditor()
        {
            string json = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                state = JsonUtility.FromJson<SuiteState>(json);
                if (state.phase == "WAITING")
                    BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
            }
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        [MenuItem("NTSD/Validation/Optimization/Batch26 Windows AI Fixed Suite")]
        private static void Begin()
        {
            BeginSuite(false);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch29 Role Collector 1000 AI Admission")]
        private static void BeginRoleCandidate()
        {
            BeginSuite(true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch30 Role Collector Formal Windows")]
        private static void BeginRoleFormalCandidate()
        {
            BeginSuite(true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch31 Role Pair Snapshot Reuse")]
        private static void BeginPairSnapshotCandidate()
        {
            BeginSuite(true, false, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch32 Brute Empty Itr Guard")]
        private static void BeginEmptyItrGuardCandidate()
        {
            BeginSuite(false, false, false, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch34 Brute Empty Itr Roster")]
        private static void BeginEmptyItrRosterCandidate()
        {
            BeginSuite(false, false, false, true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch36 Brute Exact Cache")]
        private static void BeginBruteExactCacheCandidate()
        {
            BeginSuite(false, false, false, true, true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch37 Brute Geometry First")]
        private static void BeginBruteGeometryFirstCandidate()
        {
            BeginSuite(false, false, false, true, true, true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch39 Combat1000 CPU GC Capture")]
        private static void BeginCpuGcCapture()
        {
            BeginSuite(false, cpuGcCaptureOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch40 Brute Production 1000 AI")]
        private static void BeginBruteProduction()
        {
            BeginSuite(false, bruteProductionOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch44 Brute Branch Timing 1000 AI")]
        private static void BeginBruteBranchTiming()
        {
            BeginSuite(false, bruteBranchTimingOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch45 Brute Sampled Timing 1000 AI")]
        private static void BeginBruteSampledTiming()
        {
            BeginSuite(false, bruteBranchTimingOnly: true, bruteBranchTimingSampled: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch38 Brute Kind5 Presence 1000 AI")]
        private static void BeginBruteKind5Presence()
        {
            BeginSuite(false, bruteProductionOnly: true, bruteKind5PresenceCandidate: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch50 Brute Eligibility Reuse 1000 AI OFF ON")]
        private static void BeginBruteEligibilityReuse()
        {
            BeginSuite(false, bruteProductionOnly: true, bruteEligibilityReuseCandidate: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch52 Brute Combined Cache 1000 AI OFF ON")]
        private static void BeginBruteCombinedCache()
        {
            BeginSuite(false, bruteProductionOnly: true, bruteCombinedCacheCandidate: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch54 Calibrated Full Driver GC 1000 AI")]
        private static void BeginLogicGcScope()
        {
            BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch55 Combat Full Driver GC Callsite")]
        private static void BeginLogicGcCallsite()
        {
            BeginSuite(false, cpuGcCaptureOnly: true, bruteProductionOnly: true,
                logicGcScopeOnly: true, logicGcCallsiteOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch57 Label Prewarm Full Driver GC 1000 AI")]
        private static void BeginLabelPrewarm()
        {
            BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true, labelPrewarmValidation: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch60 Coarse Envelope Full Driver GC 1000 AI")]
        private static void BeginBruteCoarseEnvelope()
        {
            Require(!startingCoarseEnvelopeWindow && !startingEnvelopeBranchTimingWindow, CoarseEnvelopeOwnerError);
            startingCoarseEnvelopeWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingCoarseEnvelopeWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch63 Envelope Binding Full Driver GC 1000 AI")]
        private static void BeginBruteEnvelopeBinding()
        {
            Require(!startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow, EnvelopeBindingOwnerError);
            startingEnvelopeBindingWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingEnvelopeBindingWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch64 Envelope Binding Eligibility Full Driver GC 1000 AI")]
        private static void BeginBruteEnvelopeBindingEligibility()
        {
            Require(!startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBindingWindow &&
                !startingCoarseEnvelopeWindow && !startingEnvelopeBranchTimingWindow, EnvelopeBindingOwnerError);
            startingEnvelopeBindingEligibilityWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingEnvelopeBindingEligibilityWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch65 Envelope Branch Timing Full Driver GC 1000 AI")]
        private static void BeginBruteEnvelopeBranchTiming()
        {
            Require(!startingEnvelopeBranchTimingWindow && !startingEnvelopeBindingEligibilityWindow &&
                !startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow, EnvelopeBindingOwnerError);
            startingEnvelopeBranchTimingWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingEnvelopeBranchTimingWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch68 Renderer Texture Binding 1000 AI OFF ON")]
        private static void BeginRenderTextureBindingReuse()
        {
            Require(!startingRenderTextureBindingWindow && !renderTextureBindingReuseOwner &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics, RenderTextureBindingOwnerError);
            startingRenderTextureBindingWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingRenderTextureBindingWindow = false;
            }
        }


        [MenuItem("NTSD/Validation/Optimization/Batch72 Ordinal Packet Full Driver GC 1000 AI OFF ON")]
        private static void BeginBruteOrdinalPacket()
        {
            Require(!startingOrdinalPacketWindow && ordinalPacketQuery == null && ordinalPacketRun == null,
                "The ordinal packet window requires exclusive ownership.");
            startingOrdinalPacketWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingOrdinalPacketWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch75 Central Segment Binding Readback")]
        private static void BeginCentralSegmentReadback()
        {
            Require(!startingCentralSegmentReadback && centralSegmentReadbackRun == null,
                "The central readback requires exclusive Editor observer ownership.");
            startingCentralSegmentReadback = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true);
            }
            finally
            {
                startingCentralSegmentReadback = false;
            }
        }

        private static void BeginSuite(bool roleCandidate, bool roleFormalCandidate = false,
            bool pairSnapshotCandidate = false, bool emptyItrGuardCandidate = false,
            bool emptyItrRosterCandidate = false, bool bruteExactCacheCandidate = false,
            bool bruteGeometryFirstCandidate = false, bool cpuGcCaptureOnly = false,
            bool bruteProductionOnly = false, bool bruteBranchTimingOnly = false,
            bool bruteBranchTimingSampled = false, bool bruteKind5PresenceCandidate = false,
            bool bruteEligibilityReuseCandidate = false, bool bruteCombinedCacheCandidate = false,
            bool logicGcScopeOnly = false, bool logicGcCallsiteOnly = false,
            bool labelPrewarmValidation = false)
        {
            Require(!startingCentralSegmentReadback || bruteProductionOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate && !emptyItrGuardCandidate &&
                !emptyItrRosterCandidate && !bruteExactCacheCandidate && !bruteGeometryFirstCandidate &&
                !cpuGcCaptureOnly && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate &&
                !logicGcScopeOnly && !logicGcCallsiteOnly && !labelPrewarmValidation &&
                !startingOrdinalPacketWindow && !startingRenderTextureBindingWindow &&
                !startingEnvelopeBranchTimingWindow && !startingEnvelopeBindingEligibilityWindow &&
                !startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow,
                "The central readback must use unchanged production defaults without another candidate or capture.");
            Require(!startingOrdinalPacketWindow || bruteProductionOnly && logicGcScopeOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate && !emptyItrGuardCandidate &&
                !emptyItrRosterCandidate && !bruteExactCacheCandidate && !bruteGeometryFirstCandidate &&
                !cpuGcCaptureOnly && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate &&
                !logicGcCallsiteOnly && !labelPrewarmValidation && !startingCoarseEnvelopeWindow &&
                !startingEnvelopeBindingWindow && !startingEnvelopeBindingEligibilityWindow &&
                !startingEnvelopeBranchTimingWindow && !startingRenderTextureBindingWindow &&
                coarseEnvelopeQuery == null && envelopeBindingQuery == null && kind5PresenceQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && !renderTextureBindingReuseOwner &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics,
                "The ordinal packet window requires production/calibrated scope without another mode or owner.");
            Require(ordinalPacketQuery == null && ordinalPacketRun == null,
                "An ordinal packet window already owns the query.");
            // Alignment contract: NTSD-OPT-H07-ENVELOPE-PATH-BRANCH-TIMING-065; instrument only this explicit, owned path.
            Require(!startingRenderTextureBindingWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingCoarseEnvelopeWindow && !startingEnvelopeBindingWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !cpuGcCaptureOnly && !bruteBranchTimingOnly &&
                !bruteBranchTimingSampled && !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate &&
                !bruteCombinedCacheCandidate && !logicGcCallsiteOnly && !labelPrewarmValidation &&
                !renderTextureBindingReuseOwner && !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics,
                RenderTextureBindingOwnerError);
            Require(!startingEnvelopeBranchTimingWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBindingWindow &&
                !startingCoarseEnvelopeWindow && !labelPrewarmValidation && !cpuGcCaptureOnly &&
                !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate,
                "The envelope branch timing window requires production defaults, calibrated scope and exclusive ownership.");
            // Alignment contract: NTSD-OPT-H07-ENVELOPE-BINDING-ELIGIBILITY-064; only this explicit owner allows all three flags.
            Require(!startingEnvelopeBindingEligibilityWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow && !startingEnvelopeBranchTimingWindow && !labelPrewarmValidation &&
                !cpuGcCaptureOnly && !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate &&
                !pairSnapshotCandidate && !emptyItrGuardCandidate && !emptyItrRosterCandidate &&
                !bruteExactCacheCandidate && !bruteGeometryFirstCandidate && !bruteBranchTimingOnly &&
                !bruteBranchTimingSampled && !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate &&
                !bruteCombinedCacheCandidate,
                "The envelope binding eligibility window requires production defaults, calibrated scope and exclusive ownership.");
            // Alignment contract: NTSD-OPT-H07-ENVELOPE-BINDING-WINDOWS-063; the old envelope owner stays binding-OFF only.
            Require(!startingEnvelopeBindingWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !startingCoarseEnvelopeWindow && !labelPrewarmValidation && !cpuGcCaptureOnly &&
                !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate,
                EnvelopeBindingModeError);
            // Alignment contract: NTSD-OPT-H07-COARSE-ENVELOPE-WINDOWS-060; keep the legacy 17-parameter entry intact.
            Require(!startingCoarseEnvelopeWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !labelPrewarmValidation && !cpuGcCaptureOnly && !logicGcCallsiteOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate,
                CoarseEnvelopeModeError);
            Require(state == null && !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Original Editor must be idle and no suite may be active.");
            Require(!labelPrewarmValidation || bruteProductionOnly && logicGcScopeOnly &&
                !cpuGcCaptureOnly && !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate &&
                !pairSnapshotCandidate && !emptyItrGuardCandidate && !emptyItrRosterCandidate &&
                !bruteExactCacheCandidate && !bruteGeometryFirstCandidate && !bruteBranchTimingOnly &&
                !bruteBranchTimingSampled && !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate &&
                !bruteCombinedCacheCandidate && !startingCoarseEnvelopeWindow && !startingEnvelopeBindingWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow,
                "The label prewarm window requires unchanged production defaults, calibrated scope and no capture or candidate.");
            Scene scene = SceneManager.GetActiveScene();
            Require(SceneManager.sceneCount == 1 && !scene.isDirty && !string.IsNullOrEmpty(scene.path),
                "A single saved clean scene is required.");
            Require(!File.Exists(PathFor(SharedRequest)) && IsOwnedTerminalOrAbsent(),
                "Shared request must be absent and an existing terminal must belong to this batch; no user file will be deleted or replaced.");
            Require(string.IsNullOrEmpty(SessionState.GetString("NTSD.ProductionEntityStress.RequestJson", string.Empty)) &&
                !SessionState.GetBool("NTSD.ProductionEntityStress.ActiveStatePresent", false) &&
                !SessionState.GetBool("NTSD.ProductionEntityStress.ReloadRecoveryPending", false),
                "Another pressure request or recovery owns the Editor.");
            if (cpuGcCaptureOnly)
                BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
            Require(!logicGcCallsiteOnly || cpuGcCaptureOnly && bruteProductionOnly && logicGcScopeOnly,
                "The logic callsite window requires capture, production and calibrated full Driver scope together.");
            string outputRoot = startingCentralSegmentReadback ? CentralSegmentReadbackOutputRoot :
                startingOrdinalPacketWindow ? BruteOrdinalPacketOutputRoot :
                startingRenderTextureBindingWindow ? RenderTextureBindingOutputRoot :
                startingEnvelopeBranchTimingWindow ? BruteEnvelopeBranchTimingOutputRoot :
                startingEnvelopeBindingEligibilityWindow ? BruteEnvelopeBindingEligibilityOutputRoot :
                startingEnvelopeBindingWindow ? BruteEnvelopeBindingOutputRoot :
                startingCoarseEnvelopeWindow ? BruteCoarseEnvelopeOutputRoot :
                labelPrewarmValidation ? LabelPrewarmOutputRoot :
                logicGcCallsiteOnly ? LogicCallsiteOutputRoot :
                logicGcScopeOnly ? LogicGcScopeOutputRoot :
                bruteCombinedCacheCandidate ? BruteCombinedCacheOutputRoot :
                bruteEligibilityReuseCandidate ? BruteEligibilityReuseOutputRoot :
                bruteKind5PresenceCandidate ? BruteKind5PresenceOutputRoot :
                bruteBranchTimingSampled ? BruteSampledTimingOutputRoot :
                bruteBranchTimingOnly ? BruteBranchTimingOutputRoot :
                bruteProductionOnly ? BruteProductionOutputRoot :
                cpuGcCaptureOnly ? CpuGcCaptureOutputRoot :
                bruteGeometryFirstCandidate ? BruteGeometryFirstOutputRoot :
                bruteExactCacheCandidate ? BruteExactCacheOutputRoot :
                emptyItrRosterCandidate ? EmptyItrRosterOutputRoot :
                emptyItrGuardCandidate ? EmptyItrGuardOutputRoot :
                pairSnapshotCandidate ? PairSnapshotOutputRoot : roleFormalCandidate ? RoleFormalOutputRoot :
                roleCandidate ? RoleCandidateOutputRoot : OutputRoot;
            Require(!Directory.Exists(PathFor(outputRoot)), "The fixed output directory must be new.");
            state = new SuiteState
            {
                phase = "WAITING",
                outputRoot = outputRoot,
                roleCandidate = roleCandidate,
                roleFormalCandidate = roleFormalCandidate,
                pairSnapshotCandidate = pairSnapshotCandidate,
                emptyItrGuardCandidate = emptyItrGuardCandidate,
                emptyItrRosterCandidate = emptyItrRosterCandidate,
                bruteExactCacheCandidate = bruteExactCacheCandidate,
                bruteGeometryFirstCandidate = bruteGeometryFirstCandidate,
                cpuGcCaptureOnly = cpuGcCaptureOnly,
                bruteProductionOnly = bruteProductionOnly,
                bruteBranchTimingOnly = bruteBranchTimingOnly,
                bruteBranchTimingSampled = bruteBranchTimingSampled,
                bruteKind5PresenceCandidate = bruteKind5PresenceCandidate,
                bruteEligibilityReuseCandidate = bruteEligibilityReuseCandidate,
                bruteCombinedCacheCandidate = bruteCombinedCacheCandidate,
                logicGcScopeOnly = logicGcScopeOnly,
                logicGcCallsiteOnly = logicGcCallsiteOnly,
                labelPrewarmValidation = labelPrewarmValidation,
                bruteCoarseEnvelopeCandidate = startingCoarseEnvelopeWindow,
                bruteEnvelopeBindingCandidate = startingEnvelopeBindingWindow,
                bruteEnvelopeBindingEligibilityCandidate = startingEnvelopeBindingEligibilityWindow,
                bruteEnvelopeBranchTimingCandidate = startingEnvelopeBranchTimingWindow,
                renderTextureBindingCandidate = startingRenderTextureBindingWindow,
                bruteOrdinalPacketCandidate = startingOrdinalPacketWindow,
                centralSegmentReadbackOnly = startingCentralSegmentReadback,
                runs = new RunState[startingCentralSegmentReadback ? 1 : startingOrdinalPacketWindow || startingRenderTextureBindingWindow || startingEnvelopeBranchTimingWindow || startingEnvelopeBindingEligibilityWindow || startingEnvelopeBindingWindow || startingCoarseEnvelopeWindow || bruteCombinedCacheCandidate || bruteEligibilityReuseCandidate || bruteBranchTimingOnly ? 4 : cpuGcCaptureOnly ? 1 : bruteProductionOnly || emptyItrGuardCandidate ? 2 : roleFormalCandidate ? 4 : roleCandidate ? 2 : 6],
                originalScene = scene.path,
                originalSceneHash = HashFile(scene.path),
                battleSceneHash = HashFile(BattleScene),
                originalSuppression = BattleTestBootstrap.SuppressEntityCreationForProductionStress,
                deadline = Now() + 600d,
            };
            for (int i = 0; i < state.runs.Length; i++)
            {
                ProductionEntityStressRequest request = BuildCurrentRequest(i);
                state.runs[i] = new RunState
                {
                    action = request.action,
                    targetSamples = request.sampleTicks,
                    reportPath = request.outputPath,
                };
                SaveNew(request.outputPath + ".request.json", JsonUtility.ToJson(request, true));
            }
            Persist();
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
            EditorSceneManager.OpenScene(BattleScene, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        internal static ProductionEntityStressRequest BuildRequest(int index)
        {
            if (index < 0 || index >= 6)
                throw new ArgumentOutOfRangeException(nameof(index));
            bool dispersed = index == 0 || index == 2 || index == 3;
            string action = dispersed ? "dispersed1000" : "combat1000";
            string label = index < 2 ? "smoke" : "formal-" + ((index == 2 || index == 4) ? "01" : "02");
            return new ProductionEntityStressRequest
            {
                action = action,
                inputMode = "ai",
                entityCount = 1000,
                warmupTicks = 120,
                sampleTicks = index < 2 ? 180 : 1800,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 2,
                maxBacklogTicks = 2,
                catchUpCpuBudgetMs = 0f,
                maxSaturationDrainTicks = 300,
                aiExecutionProfile = "DataOrientedCanonical",
                formalCollectorMode = "brute",
                simulationOnly = false,
                useDedicatedSimulationWorker = false,
                soundPresentationMode = "dispatch",
                enablePhaseTiming = true,
                enablePresentationTiming = true,
                enableDetailPhaseTiming = true,
                enableFrameTiming = true,
                autoStopWhenSampled = true,
                requireZeroGcAfterWarmup = true,
                writeFinalParitySnapshotJson = true,
                seed = 0x4E545344u,
                outputPath = OutputRoot + "/" + index.ToString("D2") + "-" + action + "-" + label + "/report.json",
            };
        }

        internal static ProductionEntityStressRequest BuildRoleCandidateRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.formalCollectorMode = "role";
            request.outputPath = RoleCandidateOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildCpuGcCaptureRequest(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(1);
            request.outputPath = CpuGcCaptureOutputRoot + "/00-combat1000-capture/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildCurrentRequest(int index)
        {
            if (state.centralSegmentReadbackOnly)
                return BuildCentralSegmentReadbackRequest(index);
            if (state.bruteOrdinalPacketCandidate)
                return BuildBruteOrdinalPacketRequest(index);
            if (state.renderTextureBindingCandidate)
                return BuildRenderTextureBindingRequest(index);
            if (state.bruteEnvelopeBranchTimingCandidate)
                return BuildBruteEnvelopeBranchTimingRequest(index);
            if (state.bruteEnvelopeBindingEligibilityCandidate)
                return BuildBruteEnvelopeBindingEligibilityRequest(index);
            if (state.bruteEnvelopeBindingCandidate)
                return BuildBruteEnvelopeBindingRequest(index);
            if (state.bruteCoarseEnvelopeCandidate)
                return BuildBruteCoarseEnvelopeRequest(index);
            if (state.labelPrewarmValidation)
                return BuildLabelPrewarmRequest(index);
            if (state.logicGcCallsiteOnly)
                return BuildLogicCallsiteRequest(index);
            if (state.logicGcScopeOnly)
                return BuildLogicGcScopeRequest(index);
            if (state.bruteCombinedCacheCandidate)
                return BuildBruteCombinedCacheRequest(index);
            if (state.bruteEligibilityReuseCandidate)
                return BuildBruteEligibilityReuseRequest(index);
            if (state.bruteKind5PresenceCandidate)
                return BuildBruteKind5PresenceRequest(index);
            if (state.bruteBranchTimingSampled)
                return BuildBruteSampledTimingRequest(index);
            if (state.bruteBranchTimingOnly)
                return BuildBruteBranchTimingRequest(index);
            if (state.bruteProductionOnly)
                return BuildBruteProductionRequest(index);
            if (state.cpuGcCaptureOnly)
                return BuildCpuGcCaptureRequest(index);
            if (state.bruteGeometryFirstCandidate)
                return BuildBruteGeometryFirstRequest(index);
            if (state.bruteExactCacheCandidate)
                return BuildBruteExactCacheRequest(index);
            if (state.emptyItrRosterCandidate)
                return BuildBruteEmptyItrRosterRequest(index);
            if (state.emptyItrGuardCandidate)
                return BuildBruteEmptyItrGuardRequest(index);
            if (state.pairSnapshotCandidate)
                return BuildRolePairSnapshotReuseRequest(index);
            if (state.roleFormalCandidate)
                return BuildRoleFormalRequest(index);
            return state.roleCandidate ? BuildRoleCandidateRequest(index) : BuildRequest(index);
        }

        private static ProductionEntityStressRequest BuildLogicGcScopeRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index);
            request.outputPath = LogicGcScopeOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildLabelPrewarmRequest(int index)
        {
            // Alignment contract: NTSD-OPT-H07-LABEL-PREWARM-WINDOWS-057; only the cold output identity differs.
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index);
            request.outputPath = LabelPrewarmOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildLogicCallsiteRequest(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(1);
            request.outputPath = LogicCallsiteOutputRoot + "/00-combat1000-callsite/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteBranchTimingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteBranchTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-timing-off" : "-timing-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteSampledTimingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteSampledTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-timing-off" : "-timing-stride64") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteKind5PresenceRequest(int index)
        {
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index);
            request.outputPath = BruteKind5PresenceOutputRoot + "/" +
                index.ToString("D2") + "-" + request.action + "-kind5-presence/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEligibilityReuseRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEligibilityReuseOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-eligibility-off" : "-eligibility-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteCombinedCacheRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteCombinedCacheOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-kind5-off" : "-kind5-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteOrdinalPacketRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteOrdinalPacketOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-packet-off" : "-packet-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEnvelopeBranchTimingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEnvelopeBranchTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-timing-off" : "-timing-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEnvelopeBindingEligibilityRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEnvelopeBindingEligibilityOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-eligibility-off" : "-eligibility-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEnvelopeBindingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEnvelopeBindingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteCoarseEnvelopeRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteCoarseEnvelopeOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-envelope-off" : "-envelope-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildRenderTextureBindingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = RenderTextureBindingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteProductionRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = BruteProductionOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildCentralSegmentReadbackRequest(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(1);
            request.outputPath = CentralSegmentReadbackOutputRoot + "/00-combat1000-readback/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildRolePairSnapshotReuseRequest(int index)
        {
            ProductionEntityStressRequest request = BuildRoleCandidateRequest(index);
            request.outputPath = PairSnapshotOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteEmptyItrRosterRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = EmptyItrRosterOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteExactCacheRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = BruteExactCacheOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteGeometryFirstRequest(int index)
        {
            ProductionEntityStressRequest request = BuildBruteExactCacheRequest(index);
            request.outputPath = BruteGeometryFirstOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteEmptyItrGuardRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = EmptyItrGuardOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildRoleFormalRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index + 2);
            request.formalCollectorMode = "role";
            request.outputPath = RoleFormalOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-formal-" + (index % 2 == 0 ? "01" : "02") + "/report.json";
            return request;
        }

        internal static bool MayContinueAfterRetainedGcFailure(
            ProductionEntityStressReport report, int targetSamples, int observedWindows,
            int minimumActiveAi, int minimumBaseRoster)
        {
            return report != null && report.status == "StoppedWithResidue" &&
                !report.harnessValidity && !report.zeroGcGatePassed &&
                report.failure != null && report.failure.StartsWith("ZeroGcGateFailed:", StringComparison.Ordinal) &&
                targetSamples == 1800 && report.sampledLogicTicks == targetSamples &&
                report.warmupTicksCompleted == 120 && observedWindows > 0 &&
                minimumActiveAi == 1000 && minimumBaseRoster == 1000 &&
                report.capacityPressure != null && report.capacityPressure.passed &&
                report.capacityPressure.capacityCriticalDelta == 0 &&
                report.u6ProductionCanonicalMismatchCount == 0 &&
                report.u6ProductionDataOrientedCompatibilityFallbackCount == 0 &&
                report.aiUnifiedSnapshotExecutionPreCommitFallbackCount == 0 &&
                report.aiUnifiedSnapshotExecutionPostCommitHardBreachCount == 0 &&
                report.teardown != null && report.teardown.attempted && report.teardown.restored &&
                report.teardown.cleanupExceptionCount == 0 && report.teardown.worldObjectsAfter == 0 &&
                report.teardown.worldEntitiesAfter == 0 && report.teardown.claimedSlotsAfter == 0 &&
                report.teardown.activeGameObjectsAfter == 0 && report.teardown.objectPoolActiveAfter == 0 &&
                report.teardown.referencePoolActiveAfter == 0;
        }

        private static void Update()
        {
            if (state == null || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            try
            {
                if (state.phase == "EXITING" || state.phase == "SHUTDOWN_FAILED")
                    return;
                if (Now() > state.deadline)
                    throw new TimeoutException("The fixed service/run deadline expired without extending the window.");
                if (!EditorApplication.isPlaying)
                    return;
                if (state.phase == "WAITING")
                {
                    if (!(bool)ServicesReady.Invoke(null, null))
                        return;
                    Require(SimulationTickDriver.Instance.World.ObjectCount == 0 &&
                        SimulationTickDriver.Instance.World.ClaimedRuntimeSlotCountForDiagnostics == 0,
                        "An empty production World is required; existing entities will not be cleared to force admission.");
                    StartCurrentRun();
                    return;
                }
                Require(currentReport != null, "A reload interrupted the owned run; automatic restart is not permitted.");
                RunState run = state.runs[state.runIndex];
                if (ProductionEntityStressRunner.Active != null)
                {
                    if (state.cpuGcCaptureOnly)
                        BattleOptimizationCpuGcCaptureEditor.Observe(currentReport);
                    if (Now() >= nextObservation)
                    {
                        nextObservation = Now() + 1d;
                        ObserveRunningSample(run, currentReport);
                    }
                    if (Now() >= nextProgress)
                    {
                        nextProgress = Now() + 15d;
                        SaveProgress(run);
                    }
                    return;
                }
                run.terminalStatus = currentReport.status;
                if (state.centralSegmentReadbackOnly)
                {
                    RestoreCentralSegmentReadback();
                    Require(run.centralSegmentReadbackCaptured && string.IsNullOrEmpty(run.centralSegmentReadbackError) &&
                        run.centralSegmentReadbackDetached, "The fixed window requires one valid central segment readback.");
                }
                if (state.bruteOrdinalPacketCandidate)
                    CompleteBruteOrdinalPacket(run);
                if (state.renderTextureBindingCandidate)
                    CompleteRenderTextureBindingReuse(run);
                if (state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                    state.bruteEnvelopeBranchTimingCandidate)
                    CompleteBruteEnvelopeBinding(run);
                if (state.bruteCoarseEnvelopeCandidate)
                    CompleteBruteCoarseEnvelope(run);
                if (state.bruteCombinedCacheCandidate)
                    CompleteBruteCombinedCache(run);
                if (state.bruteEligibilityReuseCandidate)
                    CompleteBruteEligibilityReuse(run);
                if ((state.bruteProductionOnly || state.bruteBranchTimingOnly) && productionQuery != null)
                {
                    run.bruteExactCacheAppliedDelta = productionQuery.TotalBruteExactCacheAppliedForDiagnostics -
                        run.bruteExactCacheAppliedBaseline;
                    run.bruteExactCacheFallbackDelta = productionQuery.TotalBruteExactCacheFallbackForDiagnostics -
                        run.bruteExactCacheFallbackBaseline;
                    run.bruteGeometryFirstAppliedDelta = productionQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics -
                        run.bruteGeometryFirstAppliedBaseline;
                    run.bruteProductionDefaultsUnchanged = ProductionDefaultsEnabled(productionQuery);
                    productionQuery = null;
                }
                if (state.bruteBranchTimingOnly)
                {
                    Require(branchTimingQuery != null && run.bruteBranchTimingFlagApplied &&
                        branchTimingQuery.EnableBruteBranchTimingForDiagnostics == run.bruteBranchTimingEnabled &&
                        branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics == run.bruteBranchTimingSampleStride,
                        "Branch timing flag drifted during the fixed window.");
                    run.bruteBranchTimingCoverage = branchTimingQuery.TotalBruteBranchTimingCoverageForDiagnostics;
                    run.bruteBranchTimingCoverageScope = "Diagnostic totals for all warmup and sampled collections; not steady-only and not a full-cost estimate.";
                    run.bruteBranchTimingRestored = RestoreBruteBranchTiming();
                }
                if (state.bruteKind5PresenceCandidate)
                {
                    Require(kind5PresenceQuery != null && run.bruteKind5PresenceFlagApplied &&
                        kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics,
                        "Kind5 presence flag drifted during the fixed window.");
                    run.bruteKind5PresenceAppliedDelta =
                        kind5PresenceQuery.TotalBruteKind5PresenceCollectionAppliedForDiagnostics -
                        run.bruteKind5PresenceAppliedBaseline;
                    run.bruteKind5PresenceRestored = RestoreBruteKind5Presence();
                }
                if (run.emptyItrGuardApplied)
                {
                    if (run.bruteExactCacheApplied && emptyItrGuardQuery != null)
                    {
                        run.bruteExactCacheAppliedDelta =
                            emptyItrGuardQuery.TotalBruteExactCacheAppliedForDiagnostics - run.bruteExactCacheAppliedBaseline;
                        run.bruteExactCacheFallbackDelta =
                            emptyItrGuardQuery.TotalBruteExactCacheFallbackForDiagnostics - run.bruteExactCacheFallbackBaseline;
                    }
                    if (run.bruteGeometryFirstApplied && emptyItrGuardQuery != null)
                        run.bruteGeometryFirstAppliedDelta =
                            emptyItrGuardQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics -
                            run.bruteGeometryFirstAppliedBaseline;
                    bool restored = RestoreEmptyItrGuard();
                    run.emptyItrGuardRestored = restored;
                    if (run.emptyItrRosterApplied)
                        run.emptyItrRosterRestored = restored;
                    if (run.bruteExactCacheApplied)
                        run.bruteExactCacheRestored = restored;
                    if (run.bruteGeometryFirstApplied)
                        run.bruteGeometryFirstRestored = restored;
                }
                run.sampledTicks = currentReport.sampledLogicTicks;
                run.warmupTicks = currentReport.warmupTicksCompleted;
                run.zeroGcRawGatePassed = currentReport.zeroGcGatePassed;
                run.zeroGcPassed = false;
                if (state.logicGcScopeOnly)
                    FinishLogicGcRun(run, false);
                run.harnessValidity = currentReport.harnessValidity;
                run.teardownRestored = currentReport.teardown.restored;
                run.failure = currentReport.failure;
                run.workloadValid = run.sampledTicks == run.targetSamples && run.warmupTicks == 120 &&
                    run.observedSampleWindows > 0 && run.minimumObservedActiveAi == 1000 &&
                    run.minimumObservedBaseRoster == 1000 && run.teardownRestored &&
                    currentReport.harnessValidity && string.IsNullOrEmpty(run.failure) &&
                    currentReport.status != "Failed" &&
                    (!state.emptyItrRosterCandidate ||
                     (run.emptyItrRosterApplied && run.emptyItrRosterRestored &&
                      run.emptyItrRosterObservedApplied && !run.emptyItrRosterFallbackObserved)) &&
                    (!state.bruteExactCacheCandidate ||
                     (run.bruteExactCacheApplied && run.bruteExactCacheRestored &&
                      run.bruteExactCacheObservedApplied && !run.bruteExactCacheFallbackObserved &&
                      run.bruteExactCacheAppliedDelta > 0 && run.bruteExactCacheFallbackDelta == 0)) &&
                    (!state.bruteGeometryFirstCandidate ||
                     (run.bruteGeometryFirstApplied && run.bruteGeometryFirstRestored &&
                      run.bruteGeometryFirstAppliedDelta > 0)) &&
                    (!state.bruteBranchTimingOnly ||
                     (run.bruteBranchTimingFlagApplied && run.bruteBranchTimingRestored)) &&
                    (!state.bruteKind5PresenceCandidate ||
                      (run.bruteKind5PresenceFlagApplied && run.bruteKind5PresenceRestored &&
                       run.bruteKind5PresenceAppliedDelta == run.warmupTicks + run.sampledTicks)) &&
                    (!state.bruteEligibilityReuseCandidate ||
                     (run.bruteEligibilityReuseFlagApplied && run.bruteEligibilityReuseRestored &&
                      run.bruteEligibilityReuseAppliedDelta ==
                          (run.bruteEligibilityReuseEnabled ? run.warmupTicks + run.sampledTicks : 0))) &&
                    (!state.bruteCombinedCacheCandidate ||
                     (run.bruteEligibilityReuseEnabled && run.bruteEligibilityReuseFlagApplied &&
                      run.bruteEligibilityReuseRestored &&
                      run.bruteEligibilityReuseAppliedDelta == run.warmupTicks + run.sampledTicks &&
                      run.bruteKind5PresenceFlagApplied && run.bruteKind5PresenceRestored &&
                      run.bruteKind5PresenceAppliedDelta ==
                          (run.bruteKind5PresenceEnabled ? run.warmupTicks + run.sampledTicks : 0))) &&
                    (!state.renderTextureBindingCandidate ||
                     (run.renderTextureBindingReuseFlagApplied && run.renderTextureBindingReuseFlagUnchanged &&
                      run.renderTextureBindingReuseRestored && run.renderTextureBindingAcceptedCameraSamples > 0 &&
                      run.renderTextureBindingCameraId != 0 && run.renderTextureBindingBodyPrepareCount > 0 &&
                      run.renderTextureBindingBodyReuseCount >= 0 &&
                      (run.renderTextureBindingReuseEnabled || run.renderTextureBindingBodyReuseCount == 0))) &&
                    (!state.bruteCoarseEnvelopeCandidate || CoarseEnvelopeObservationValid(run)) &&
                    (!state.bruteOrdinalPacketCandidate || OrdinalPacketObservationValid(run)) &&
                    (!state.bruteEnvelopeBindingCandidate || EnvelopeBindingObservationValid(run)) &&
                    (!state.bruteEnvelopeBindingEligibilityCandidate || EnvelopeBindingEligibilityObservationValid(run)) &&
                    (!state.bruteEnvelopeBranchTimingCandidate || EnvelopeBranchTimingObservationValid(run)) &&
                    (!(state.bruteProductionOnly || state.bruteBranchTimingOnly) ||
                     (run.bruteProductionDefaultsObserved && run.bruteProductionDefaultsUnchanged &&
                      run.emptyItrRosterObservedApplied && !run.emptyItrRosterFallbackObserved &&
                      run.bruteExactCacheObservedApplied && !run.bruteExactCacheFallbackObserved &&
                      run.bruteExactCacheAppliedDelta == run.warmupTicks + run.sampledTicks &&
                      run.bruteExactCacheFallbackDelta == 0 &&
                      run.bruteGeometryFirstAppliedDelta == run.warmupTicks + run.sampledTicks));
                if (state.logicGcScopeOnly)
                    run.workloadValid &= run.logicGcEvidence != null && run.logicGcEvidence.coverageValid &&
                        run.logicGcEvidence.before.passed && run.logicGcEvidence.after.passed;
                run.retainedFailedGcMeasurement = state.roleFormalCandidate &&
                    MayContinueAfterRetainedGcFailure(currentReport, run.targetSamples,
                        run.observedSampleWindows, run.minimumObservedActiveAi, run.minimumObservedBaseRoster);
                SaveNew(run.reportPath + ".terminal.txt", File.Exists(PathFor(SharedResult))
                    ? File.ReadAllText(PathFor(SharedResult)) : "MISSING_TERMINAL_RESULT");
                SaveNew(run.reportPath + ".observation.json", JsonUtility.ToJson(run, true));
                state.completedRuns++;
                currentReport = null;
                if (run.retainedFailedGcMeasurement)
                    state.retainedFailedGcMeasurements++;
                if (!run.workloadValid && !run.retainedFailedGcMeasurement)
                    throw new InvalidOperationException("Fixed workload/cleanup admission failed for " + run.action + "; original report retained.");
                state.runIndex++;
                if (state.runIndex == state.runs.Length)
                {
                    state.status = state.retainedFailedGcMeasurements == 0 ?
                        "MEASUREMENTS_COMPLETED" : "MEASUREMENTS_COMPLETED_WITH_FAILURES";
                    ShutdownAndExit();
                }
                else
                {
                    StartCurrentRun();
                }
            }
            catch (Exception exception)
            {
                state.status = "PARTIAL";
                state.error = exception.ToString();
                Require(RestoreRenderTextureBindingReuse(), RenderTextureBindingOwnerError);
                if (ProductionEntityStressRunner.Active != null)
                    StopRun.Invoke(ProductionEntityStressRunner.Active, new object[] { "finite-suite-abort", false });
                ShutdownAndExit();
            }
        }

        private static void StartCurrentRun()
        {
            ProductionEntityStressRequest request = BuildCurrentRequest(state.runIndex);
            string frozen = File.ReadAllText(PathFor(request.outputPath + ".request.json"));
            Require(JsonUtility.ToJson(JsonUtility.FromJson<ProductionEntityStressRequest>(frozen)) ==
                JsonUtility.ToJson(request), "Frozen request drifted.");
            object config = FromRequest.Invoke(null, new object[] { request, PathFor(string.Empty) });
            state.phase = "RUNNING";
            state.deadline = Now() + 7200d;
            Persist();
            var runner = (ProductionEntityStressRunner)StartRun.Invoke(null, new[] { config });
            currentReport = runner.Report;
            if (state.bruteProductionOnly || state.bruteBranchTimingOnly)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "The ordinary production observation must start before any tick.");
                productionQuery = SimulationTickDriver.Instance.World.SceneQuery as BruteForceSceneQuery;
                Require(productionQuery != null &&
                    productionQuery.FormalCollectorMode == CollisionFormalCollectorMode.ForceBruteForce &&
                    ProductionDefaultsEnabled(productionQuery), "Ordinary Brute production defaults are required.");
                RunState run = state.runs[state.runIndex];
                run.bruteProductionDefaultsObserved = true;
                run.bruteExactCacheAppliedBaseline = productionQuery.TotalBruteExactCacheAppliedForDiagnostics;
                run.bruteExactCacheFallbackBaseline = productionQuery.TotalBruteExactCacheFallbackForDiagnostics;
                run.bruteGeometryFirstAppliedBaseline = productionQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics;
            }
            if (state.centralSegmentReadbackOnly)
            {
                Require(centralSegmentReadbackRun == null && OrdinalPacketQueryDefaultsValid(productionQuery) &&
                    !productionQuery.EnableBruteOrdinalPacketForDiagnostics &&
                    !productionQuery.EnableBruteDepthRejectForDiagnostics,
                    "The readback requires all independent candidates off and an unowned observer.");
                centralSegmentReadbackRun = state.runs[state.runIndex];
                RenderPipelineManager.endCameraRendering += ObserveCentralSegmentReadback;
            }
            if (state.bruteOrdinalPacketCandidate)
            {
                Require(currentReport.logicTicksExecuted == 0, "The ordinal packet flag must be applied before any tick.");
                ApplyBruteOrdinalPacket(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.renderTextureBindingCandidate)
            {
                Require(currentReport.logicTicksExecuted == 0 && productionQuery != null &&
                    !productionQuery.EnableBruteCoarseEnvelopeForDiagnostics &&
                    !productionQuery.EnableBruteKind5PresenceForDiagnostics &&
                    !productionQuery.EnableBruteEligibilityReuseForDiagnostics &&
                    !productionQuery.EnableBruteRejectedBindingReuseForDiagnostics &&
                    !productionQuery.EnableBruteBranchTimingForDiagnostics &&
                    !productionQuery.EnableBruteEnvelopeBranchTimingForDiagnostics &&
                    !productionQuery.EnableBruteCoarseDispatchForDiagnostics, RenderTextureBindingOwnerError);
                ApplyRenderTextureBindingReuse(state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.bruteEnvelopeBranchTimingCandidate)
                ApplyBruteEnvelopeBranchTiming(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteEnvelopeBindingEligibilityCandidate)
                ApplyBruteEnvelopeBindingEligibility(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteCoarseEnvelopeCandidate)
                ApplyBruteCoarseEnvelope(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteEnvelopeBindingCandidate)
                ApplyBruteEnvelopeBinding(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteBranchTimingOnly)
            {
                branchTimingQuery = productionQuery;
                previousBranchTiming = branchTimingQuery.EnableBruteBranchTimingForDiagnostics;
                previousBranchTimingSampleStride = branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics;
                RunState run = state.runs[state.runIndex];
                run.bruteBranchTimingEnabled = state.runIndex % 2 != 0;
                run.bruteBranchTimingSampleStride = state.bruteBranchTimingSampled ? 64 : 1;
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "Branch timing must be applied before warmup and sampling.");
                branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics = run.bruteBranchTimingSampleStride;
                branchTimingQuery.EnableBruteBranchTimingForDiagnostics = run.bruteBranchTimingEnabled;
                run.bruteBranchTimingFlagApplied =
                    branchTimingQuery.EnableBruteBranchTimingForDiagnostics == run.bruteBranchTimingEnabled &&
                    branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics == run.bruteBranchTimingSampleStride;
                Require(run.bruteBranchTimingFlagApplied, "Branch timing must be configured before any tick.");
            }
            if (state.bruteKind5PresenceCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0 &&
                    productionQuery != null && !productionQuery.EnableBruteBranchTimingForDiagnostics,
                    "Kind5 presence must be applied before ticks without per-branch timing.");
                kind5PresenceQuery = productionQuery;
                previousKind5Presence = kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics;
                RunState run = state.runs[state.runIndex];
                run.bruteKind5PresenceAppliedBaseline =
                    kind5PresenceQuery.TotalBruteKind5PresenceCollectionAppliedForDiagnostics;
                kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics = true;
                run.bruteKind5PresenceFlagApplied = kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics;
            }
            if (state.bruteEligibilityReuseCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "Eligibility reuse must be configured before any warmup or sampled tick.");
                ApplyBruteEligibilityReuse(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.bruteCombinedCacheCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "Combined caches must be configured before any warmup or sampled tick.");
                ApplyBruteCombinedCache(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.cpuGcCaptureOnly)
            {
                if (state.logicGcCallsiteOnly)
                    BattleOptimizationCpuGcCaptureEditor.ArmLogicCallsiteWindow(PathFor(state.outputRoot));
                else
                    BattleOptimizationCpuGcCaptureEditor.Arm(PathFor(state.outputRoot));
            }
            if (state.emptyItrGuardCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "The candidate must be applied before any warmup/sample tick.");
                emptyItrGuardQuery = SimulationTickDriver.Instance.World.SceneQuery as BruteForceSceneQuery;
                Require(emptyItrGuardQuery != null &&
                    emptyItrGuardQuery.FormalCollectorMode == CollisionFormalCollectorMode.ForceBruteForce,
                    "The same explicit BruteForce collector is required.");
                previousEmptyItrGuard = emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics;
                previousEmptyItrRoster = emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics;
                previousBruteExactCache = emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics;
                previousBruteGeometryFirst = emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics;
                emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics = true;
                emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics = state.emptyItrRosterCandidate;
                emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics = state.bruteExactCacheCandidate;
                emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics = state.bruteGeometryFirstCandidate;
                state.runs[state.runIndex].emptyItrGuardApplied = true;
                if (state.emptyItrRosterCandidate)
                {
                    emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics = true;
                    state.runs[state.runIndex].emptyItrRosterApplied = true;
                    state.runs[state.runIndex].emptyItrRosterCapacity =
                        emptyItrGuardQuery.BruteEmptyItrRosterCapacityForDiagnostics;
                    state.runs[state.runIndex].emptyItrRosterPayloadBytes =
                        4L * (state.runs[state.runIndex].emptyItrRosterCapacity + 1L);
                }
                if (state.bruteExactCacheCandidate)
                {
                    RunState run = state.runs[state.runIndex];
                    run.bruteExactCacheAppliedBaseline = emptyItrGuardQuery.TotalBruteExactCacheAppliedForDiagnostics;
                    run.bruteExactCacheFallbackBaseline = emptyItrGuardQuery.TotalBruteExactCacheFallbackForDiagnostics;
                    emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics = true;
                    run.bruteExactCacheApplied = true;
                    run.bruteExactParticipantCapacity = emptyItrGuardQuery.BruteExactParticipantCapacityForDiagnostics;
                    run.bruteExactBodyCapacity = emptyItrGuardQuery.BruteExactBodyCapacityForDiagnostics;
                    run.bruteExactItrCapacity = emptyItrGuardQuery.BruteExactItrCapacityForDiagnostics;
                }
                if (state.bruteGeometryFirstCandidate)
                {
                    RunState run = state.runs[state.runIndex];
                    run.bruteGeometryFirstAppliedBaseline =
                        emptyItrGuardQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics;
                    emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics = true;
                    run.bruteGeometryFirstApplied = true;
                }
            }
            nextObservation = Now();
            nextProgress = Now() + 15d;
            if (state.logicGcScopeOnly)
            {
                Require(currentReport.logicTicksExecuted == 0 && productionQuery != null &&
                    !productionQuery.EnableBruteKind5PresenceForDiagnostics &&
                    (state.bruteEnvelopeBranchTimingCandidate
                        ? EnvelopeBranchTimingScopeValid(productionQuery, state.runs[state.runIndex])
                        : (state.bruteEnvelopeBindingEligibilityCandidate
                        ? EnvelopeBindingEligibilityScopeValid(productionQuery, state.runs[state.runIndex])
                        : (state.bruteEnvelopeBindingCandidate
                            ? EnvelopeBindingScopeValid(productionQuery, state.runs[state.runIndex])
                            : !productionQuery.EnableBruteRejectedBindingReuseForDiagnostics) &&
                            !productionQuery.EnableBruteEligibilityReuseForDiagnostics) &&
                            !productionQuery.EnableBruteBranchTimingForDiagnostics &&
                            !productionQuery.EnableBruteEnvelopeBranchTimingForDiagnostics) &&
                    !productionQuery.EnableBruteCoarseDispatchForDiagnostics &&
                    logicGcObserver == null,
                    "The calibrated scope requires the existing production path before the first tick.");
                logicGcObserver = new BattleLogicTickGcObserverEditor();
                logicGcObserver.Attach(runner);
            }
        }

        private static void ObserveRunningSample(RunState run, ProductionEntityStressReport report)
        {
            if (report.status != "Running" || report.sampledLogicTicks <= run.lastObservedSampleTick)
                return;
            run.lastObservedSampleTick = report.sampledLogicTicks;
            run.minimumObservedActiveAi = Math.Min(run.minimumObservedActiveAi, report.baseAiActiveCount);
            run.minimumObservedBaseRoster = Math.Min(run.minimumObservedBaseRoster, report.baseRosterActiveCount);
            run.observedSampleWindows++;
            if (state.bruteOrdinalPacketCandidate)
                ObserveBruteOrdinalPacket(run, report.logicTicksExecuted);
            if (state.bruteCoarseEnvelopeCandidate)
                ObserveBruteCoarseEnvelope(run);
            if (state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                state.bruteEnvelopeBranchTimingCandidate)
                ObserveBruteEnvelopeBinding(run);
            BruteForceSceneQuery query = run.bruteProductionDefaultsObserved ? productionQuery : emptyItrGuardQuery;
            if (run.bruteKind5PresenceFlagApplied && query != null)
                run.bruteKind5PresenceMaximumSkippedScans = Math.Max(
                    run.bruteKind5PresenceMaximumSkippedScans, query.LastBruteKind5ScanSkippedForDiagnostics);
            if ((run.emptyItrRosterApplied || run.bruteProductionDefaultsObserved) && query != null)
            {
                run.emptyItrRosterObservedApplied |= query.LastBruteEmptyItrRosterAppliedForDiagnostics;
                run.emptyItrRosterFallbackObserved |= query.LastBruteEmptyItrRosterFallbackForDiagnostics;
                run.emptyItrRosterMaximumBuildCount = Math.Max(run.emptyItrRosterMaximumBuildCount,
                    query.LastBruteEmptyItrRosterBuildCountForDiagnostics);
                run.emptyItrRosterMaximumVisitedPairs = Math.Max(run.emptyItrRosterMaximumVisitedPairs,
                    query.LastBruteEmptyItrRosterVisitedPairCountForDiagnostics);
                run.emptyItrRosterMaximumSkippedPairs = Math.Max(run.emptyItrRosterMaximumSkippedPairs,
                    query.LastBruteEmptyItrRosterSkippedPairCountForDiagnostics);
            }
            if ((run.bruteExactCacheApplied || run.bruteProductionDefaultsObserved) && query != null)
            {
                run.bruteExactCacheObservedApplied |= query.LastBruteExactCacheAppliedForDiagnostics;
                run.bruteExactCacheFallbackObserved |= query.LastBruteExactCacheFallbackForDiagnostics;
                run.bruteExactCacheMaximumBuildCount = Math.Max(run.bruteExactCacheMaximumBuildCount,
                    query.LastBruteExactCacheBuildCountForDiagnostics);
                run.bruteExactCacheMaximumDirections = Math.Max(run.bruteExactCacheMaximumDirections,
                    query.LastBruteExactCacheDirectionCountForDiagnostics);
            }
            if ((run.bruteGeometryFirstApplied || run.bruteProductionDefaultsObserved) && query != null)
            {
                run.bruteGeometryFirstMaximumRejectCount = Math.Max(run.bruteGeometryFirstMaximumRejectCount,
                    query.LastBruteGeometryFirstRejectCountForDiagnostics);
                // Preparation is complete before sampled ticks; do not report the pre-warmup zero capacity.
                run.emptyItrRosterCapacity = query.BruteEmptyItrRosterCapacityForDiagnostics;
                run.emptyItrRosterPayloadBytes = 4L * (run.emptyItrRosterCapacity + 1L);
                run.bruteExactParticipantCapacity = query.BruteExactParticipantCapacityForDiagnostics;
                run.bruteExactBodyCapacity = query.BruteExactBodyCapacityForDiagnostics;
                run.bruteExactItrCapacity = query.BruteExactItrCapacityForDiagnostics;
            }
        }

        private static bool ProductionDefaultsEnabled(BruteForceSceneQuery query) =>
            query.EnableEmptyItrPairGuardForDiagnostics && query.EnableBruteEmptyItrRosterForDiagnostics &&
            query.EnableBruteExactCacheForDiagnostics && query.EnableBruteGeometryFirstForDiagnostics;

        private static void SaveProgress(RunState run)
        {
            run.sampledTicks = currentReport.sampledLogicTicks;
            run.warmupTicks = currentReport.warmupTicksCompleted;
            SaveNew(state.outputRoot + "/progress-" + (progressOrdinal++).ToString("D4") + ".json", JsonUtility.ToJson(run, true));
        }

        private static void ShutdownAndExit()
        {
            RestoreCentralSegmentReadback();
            Require(RestoreBruteOrdinalPacket(), "The owned ordinal packet flag must be restored before shutdown.");
            Require(RestoreRenderTextureBindingReuse(), RenderTextureBindingOwnerError);
            Require(RestoreBruteEnvelopeBindingForExit(), EnvelopeBindingOwnerError);
            if (logicGcObserver != null)
                FinishLogicGcRun(state.runs[state.runIndex], true);
            Require(RestoreBruteCoarseEnvelope(), CoarseEnvelopeOwnerError);
            Require(RestoreBruteEligibilityReuse(), "Eligibility reuse must be restored before owner shutdown.");
            Require(RestoreBruteBranchTiming(), "Branch timing must be restored before owner shutdown.");
            Require(RestoreBruteKind5Presence(), "Kind5 presence must be restored before owner shutdown.");
            productionQuery = null;
            if (state.cpuGcCaptureOnly)
                BattleOptimizationCpuGcCaptureEditor.FinishAndRestore("owner-shutdown");
            Require(RestoreEmptyItrGuard(), "The diagnostic guard must be restored before owner shutdown.");
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World != null)
            {
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap))
                            continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }
                state.remainingObjects = shutdown.RemainingWorldObjects;
                state.remainingSlots = shutdown.RemainingRuntimeSlots;
                state.remainingBorrowers = shutdown.RemainingPoolBorrowers;
                state.orderedShutdown = shutdown.IsComplete && driver.World == null;
                if (!state.orderedShutdown)
                {
                    state.phase = "SHUTDOWN_FAILED";
                    state.status = "PARTIAL";
                    state.error += " Ordered shutdown failed: " + shutdown.FailureReason;
                    Persist();
                    SaveNew(state.outputRoot + "/shutdown-failure.json", JsonUtility.ToJson(state, true));
                    return;
                }
            }
            state.phase = "EXITING";
            Persist();
            EditorApplication.ExitPlaymode();
        }

        private static void OnPlayMode(PlayModeStateChange change)
        {
            if (state == null)
                return;
            if (change == PlayModeStateChange.ExitingPlayMode)
                RestoreCentralSegmentReadback();
            if (state.bruteOrdinalPacketCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteOrdinalPacket(), "The owned ordinal packet flag must be restored on Play exit.");
            if (state.renderTextureBindingCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreRenderTextureBindingReuse(), RenderTextureBindingOwnerError);
            if ((state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                state.bruteEnvelopeBranchTimingCandidate) &&
                change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteEnvelopeBindingForExit(), EnvelopeBindingOwnerError);
            if (state.logicGcScopeOnly && change == PlayModeStateChange.ExitingPlayMode && logicGcObserver != null)
                FinishLogicGcRun(state.runs[state.runIndex], true);
            if (state.bruteCoarseEnvelopeCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteCoarseEnvelope(), CoarseEnvelopeOwnerError);
            if (state.bruteCombinedCacheCandidate && change == PlayModeStateChange.ExitingPlayMode)
            {
                Require(RestoreBruteKind5Presence(), "Combined kind5 cache must be restored on external Play exit.");
                Require(RestoreBruteEligibilityReuse(), "Combined eligibility cache must be restored on external Play exit.");
            }
            if (state.bruteEligibilityReuseCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteEligibilityReuse(), "Eligibility reuse must also be restored on external Play exit.");
            if (state.cpuGcCaptureOnly && change == PlayModeStateChange.ExitingPlayMode)
                BattleOptimizationCpuGcCaptureEditor.FinishAndRestore("play-exit");
            if (change == PlayModeStateChange.ExitingPlayMode && state.phase != "EXITING")
            {
                state.status = "PARTIAL";
                state.error = "Play Mode was stopped externally; no automatic restart.";
                state.phase = "EXITING";
                Persist();
            }
            if (change != PlayModeStateChange.EnteredEditMode || state.phase != "EXITING")
                return;
            Scene scene = SceneManager.GetActiveScene();
            state.battleSceneUnchanged = scene.path == BattleScene && !scene.isDirty &&
                HashFile(BattleScene) == state.battleSceneHash;
            if (state.battleSceneUnchanged && HashFile(state.originalScene) == state.originalSceneHash)
            {
                EditorSceneManager.OpenScene(state.originalScene, OpenSceneMode.Single);
                state.originalSceneRestored = !SceneManager.GetActiveScene().isDirty &&
                    SceneManager.GetActiveScene().path == state.originalScene;
            }
            if (!state.battleSceneUnchanged || !state.originalSceneRestored)
            {
                state.status = "PARTIAL";
                state.error += " Scene identity/dirty guard failed; no forced save or restore.";
            }
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = state.originalSuppression;
            state.phase = "DONE";
            state.updatedUtc = DateTime.UtcNow.ToString("O");
            SaveNew(state.outputRoot + "/suite-result.json", JsonUtility.ToJson(state, true));
            SessionState.EraseString(SessionKey);
            state = null;
            currentReport = null;
            productionQuery = null;
        }

        private static void Persist()
        {
            state.updatedUtc = DateTime.UtcNow.ToString("O");
            SessionState.SetString(SessionKey, JsonUtility.ToJson(state));
        }

        private static double Now() => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        private static void FinishLogicGcRun(RunState run, bool aborted)
        {
            Require(logicGcObserver != null, "The full Driver GC owner was lost; no zero claim is permitted.");
            BattleLogicTickGcObserverEditor observer = logicGcObserver;
            logicGcObserver = null;
            try
            {
                run.logicGcEvidence = observer.Finish(currentReport?.logicTicksExecuted ?? 0, run.targetSamples);
                if (state.logicGcCallsiteOnly)
                {
                    run.logicGcEvidence.zeroAllocationPassed = false;
                    run.logicGcEvidence.provenSteadyAllocatedBytes = -1;
                    run.logicGcEvidence.status = "INSTRUMENTED_CALLSITE_ONLY / " + run.logicGcEvidence.status;
                }
                if (aborted)
                {
                    run.logicGcEvidence.zeroAllocationPassed = false;
                    run.logicGcEvidence.provenSteadyAllocatedBytes = -1;
                    run.logicGcEvidence.status = "ABORTED / INCOMPLETE";
                }
                run.zeroGcPassed = run.logicGcEvidence.zeroAllocationPassed;
                run.zeroGcEvidenceStatus = run.logicGcEvidence.status;
                SaveNew(run.reportPath + (aborted ? ".logic-gc.aborted.json" : ".logic-gc.json"),
                    JsonUtility.ToJson(run.logicGcEvidence, true));
            }
            finally { observer.Dispose(); }
        }
        private static string PathFor(string path) => Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));

        private static bool IsOwnedTerminalOrAbsent()
        {
            if (!File.Exists(PathFor(SharedResult)))
                return true;
            string[] lines = File.ReadAllLines(PathFor(SharedResult));
            string ownerRoot = PathFor("artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH24-WINDOWS-AI-20261007") + Path.DirectorySeparatorChar;
            string currentOwnerRoot = PathFor("artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH26-H07-CONTINUE-20261007") + Path.DirectorySeparatorChar;
            string candidateOwnerRoot = PathFor(RoleCandidateOutputRoot) + Path.DirectorySeparatorChar;
            string formalOwnerRoot = PathFor(RoleFormalOutputRoot) + Path.DirectorySeparatorChar;
            string pairSnapshotOwnerRoot = PathFor(PairSnapshotOutputRoot) + Path.DirectorySeparatorChar;
            string emptyItrOwnerRoot = PathFor(EmptyItrGuardOutputRoot) + Path.DirectorySeparatorChar;
            string emptyItrRosterOwnerRoot = PathFor(EmptyItrRosterOutputRoot) + Path.DirectorySeparatorChar;
            string bruteExactCacheOwnerRoot = PathFor(BruteExactCacheOutputRoot) + Path.DirectorySeparatorChar;
            string bruteGeometryFirstOwnerRoot = PathFor(BruteGeometryFirstOutputRoot) + Path.DirectorySeparatorChar;
            string cpuGcCaptureOwnerRoot = PathFor(CpuGcCaptureOutputRoot) + Path.DirectorySeparatorChar;
            string bruteProductionOwnerRoot = PathFor(BruteProductionOutputRoot) + Path.DirectorySeparatorChar;
            string branchTimingOwnerRoot = PathFor(BruteBranchTimingOutputRoot) + Path.DirectorySeparatorChar;
            string sampledTimingOwnerRoot = PathFor(BruteSampledTimingOutputRoot) + Path.DirectorySeparatorChar;
            string kind5PresenceOwnerRoot = PathFor(BruteKind5PresenceOutputRoot) + Path.DirectorySeparatorChar;
            string eligibilityReuseOwnerRoot = PathFor(BruteEligibilityReuseOutputRoot) + Path.DirectorySeparatorChar;
            string combinedCacheOwnerRoot = PathFor(BruteCombinedCacheOutputRoot) + Path.DirectorySeparatorChar;
            string logicGcOwnerRoot = PathFor(LogicGcScopeOutputRoot) + Path.DirectorySeparatorChar;
            string logicCallsiteOwnerRoot = PathFor(LogicCallsiteOutputRoot) + Path.DirectorySeparatorChar;
            string labelPrewarmOwnerRoot = PathFor(LabelPrewarmOutputRoot) + Path.DirectorySeparatorChar;
            string coarseEnvelopeOwnerRoot = PathFor(BruteCoarseEnvelopeOutputRoot) + Path.DirectorySeparatorChar;
            string envelopeBindingOwnerRoot = PathFor(BruteEnvelopeBindingOutputRoot) + Path.DirectorySeparatorChar;
            string envelopeBindingEligibilityOwnerRoot = PathFor(BruteEnvelopeBindingEligibilityOutputRoot) + Path.DirectorySeparatorChar;
            string envelopeBranchTimingOwnerRoot = PathFor(BruteEnvelopeBranchTimingOutputRoot) + Path.DirectorySeparatorChar;
            string renderTextureBindingOwnerRoot = PathFor(RenderTextureBindingOutputRoot) + Path.DirectorySeparatorChar;
            string ordinalPacketOwnerRoot = PathFor(BruteOrdinalPacketOutputRoot) + Path.DirectorySeparatorChar;
            string centralReadbackOwnerRoot = PathFor("artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008") + Path.DirectorySeparatorChar;
            return lines.Length >= 2 && (lines[0] == "PASS" || lines[0] == "FAIL") &&
                (PathFor(lines[1]).StartsWith(ownerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(currentOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(candidateOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(formalOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(pairSnapshotOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(emptyItrOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(emptyItrRosterOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(bruteExactCacheOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(bruteGeometryFirstOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(cpuGcCaptureOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(bruteProductionOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(branchTimingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(sampledTimingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(kind5PresenceOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(eligibilityReuseOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(combinedCacheOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(logicGcOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(logicCallsiteOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(labelPrewarmOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(coarseEnvelopeOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(envelopeBindingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(envelopeBindingEligibilityOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(envelopeBranchTimingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(renderTextureBindingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(ordinalPacketOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(centralReadbackOwnerRoot, StringComparison.OrdinalIgnoreCase));
        }

        private static bool EnvelopeBindingQueryDefaultsValid(BruteForceSceneQuery query) =>
            query != null && ProductionDefaultsEnabled(query) &&
            !query.EnableBruteKind5PresenceForDiagnostics && !query.EnableBruteEligibilityReuseForDiagnostics &&
            !query.EnableBruteCoarseDispatchForDiagnostics && !query.EnableBruteBranchTimingForDiagnostics &&
            !query.EnableBruteEnvelopeBranchTimingForDiagnostics;

        private static bool EnvelopeBindingScopeValid(BruteForceSceneQuery query, RunState run) =>
            !envelopeBindingEligibilityOwner && !envelopeBindingBranchTimingOwner &&
            query == envelopeBindingQuery && EnvelopeBindingQueryDefaultsValid(query) &&
            query.EnableBruteCoarseEnvelopeForDiagnostics &&
            query.EnableBruteRejectedBindingReuseForDiagnostics == run.bruteRejectedBindingReuseEnabled;

        private static bool EnvelopeBindingEligibilityScopeValid(BruteForceSceneQuery query, RunState run) =>
            envelopeBindingEligibilityOwner && !envelopeBindingBranchTimingOwner &&
            run.bruteEnvelopeBindingEligibilityMode && query != null &&
            query == envelopeBindingQuery && ProductionDefaultsEnabled(query) &&
            !query.EnableBruteKind5PresenceForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            !query.EnableBruteBranchTimingForDiagnostics && !query.EnableBruteEnvelopeBranchTimingForDiagnostics &&
            query.EnableBruteCoarseEnvelopeForDiagnostics &&
            query.EnableBruteRejectedBindingReuseForDiagnostics && run.bruteRejectedBindingReuseEnabled &&
            run.bruteEligibilityReuseFlagApplied &&
            query.EnableBruteEligibilityReuseForDiagnostics == run.bruteEligibilityReuseEnabled;

        private static bool EnvelopeBranchTimingScopeValid(BruteForceSceneQuery query, RunState run) =>
            envelopeBindingBranchTimingOwner && !envelopeBindingEligibilityOwner &&
            run.bruteEnvelopeBranchTimingMode && query != null && query == envelopeBindingQuery &&
            ProductionDefaultsEnabled(query) && !query.EnableBruteKind5PresenceForDiagnostics &&
            !query.EnableBruteEligibilityReuseForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            query.EnableBruteCoarseEnvelopeForDiagnostics && query.EnableBruteRejectedBindingReuseForDiagnostics &&
            query.EnableBruteEnvelopeBranchTimingForDiagnostics && run.bruteEnvelopeBranchTimingFlagApplied &&
            run.bruteBranchTimingFlagApplied && query.EnableBruteBranchTimingForDiagnostics == run.bruteBranchTimingEnabled &&
            query.BruteBranchTimingSampleStrideForDiagnostics == run.bruteBranchTimingSampleStride &&
            run.bruteBranchTimingSampleStride == 64;

        private static void ApplyBruteEnvelopeBranchTiming(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            ApplyBruteEnvelopeBinding(query, run, true);
            previousEnvelopeBranchTiming = query.EnableBruteEnvelopeBranchTimingForDiagnostics;
            previousBranchTiming = query.EnableBruteBranchTimingForDiagnostics;
            previousBranchTimingSampleStride = query.BruteBranchTimingSampleStrideForDiagnostics;
            envelopeBindingBranchTimingOwner = true;
            run.bruteEnvelopeBranchTimingMode = true;
            run.bruteBranchTimingEnabled = enabled;
            run.bruteBranchTimingSampleStride = 64;
            query.EnableBruteEnvelopeBranchTimingForDiagnostics = true;
            query.BruteBranchTimingSampleStrideForDiagnostics = 64;
            query.EnableBruteBranchTimingForDiagnostics = enabled;
            run.bruteEnvelopeBranchTimingFlagApplied = query.EnableBruteEnvelopeBranchTimingForDiagnostics;
            run.bruteEnvelopeBranchTimingFlagUnchanged = run.bruteEnvelopeBranchTimingFlagApplied;
            run.bruteBranchTimingFlagApplied = query.EnableBruteBranchTimingForDiagnostics == enabled &&
                query.BruteBranchTimingSampleStrideForDiagnostics == 64;
            run.bruteBranchTimingFlagUnchanged = run.bruteBranchTimingFlagApplied;
            Require(EnvelopeBranchTimingScopeValid(query, run), EnvelopeBindingOwnerError);
        }

        private static void ApplyBruteEnvelopeBindingEligibility(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            ApplyBruteEnvelopeBinding(query, run, true);
            previousEnvelopeBindingEligibility = query.EnableBruteEligibilityReuseForDiagnostics;
            envelopeBindingEligibilityOwner = true;
            run.bruteEnvelopeBindingEligibilityMode = true;
            run.bruteEnvelopeBindingPreviousEligibility = previousEnvelopeBindingEligibility;
            run.bruteEligibilityReuseEnabled = enabled;
            run.bruteEligibilityReuseAppliedBaseline = query.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics;
            query.EnableBruteEligibilityReuseForDiagnostics = enabled;
            run.bruteEligibilityReuseFlagApplied = query.EnableBruteEligibilityReuseForDiagnostics == enabled;
            run.bruteEligibilityReuseFlagUnchanged = run.bruteEligibilityReuseFlagApplied;
            Require(EnvelopeBindingEligibilityScopeValid(query, run), EnvelopeBindingOwnerError);
        }

        private static void ApplyBruteEnvelopeBinding(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            Require(!envelopeBindingEligibilityOwner && !envelopeBindingBranchTimingOwner &&
                envelopeBindingQuery == null && coarseEnvelopeQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && kind5PresenceQuery == null &&
                emptyItrGuardQuery == null && EnvelopeBindingQueryDefaultsValid(query) &&
                !query.EnableBruteCoarseEnvelopeForDiagnostics && !query.EnableBruteRejectedBindingReuseForDiagnostics,
                EnvelopeBindingOwnerError);
            envelopeBindingQuery = query;
            previousEnvelopeBindingEnvelope = query.EnableBruteCoarseEnvelopeForDiagnostics;
            previousEnvelopeBindingReuse = query.EnableBruteRejectedBindingReuseForDiagnostics;
            run.bruteEnvelopeBindingPreviousEnvelope = previousEnvelopeBindingEnvelope;
            run.bruteEnvelopeBindingPreviousReuse = previousEnvelopeBindingReuse;
            run.bruteCoarseEnvelopeEnabled = true;
            run.bruteRejectedBindingReuseEnabled = enabled;
            query.EnableBruteCoarseEnvelopeForDiagnostics = true;
            query.EnableBruteRejectedBindingReuseForDiagnostics = enabled;
            run.bruteCoarseEnvelopeFlagApplied = query.EnableBruteCoarseEnvelopeForDiagnostics;
            run.bruteRejectedBindingReuseFlagApplied = query.EnableBruteRejectedBindingReuseForDiagnostics == enabled;
            run.bruteCoarseEnvelopeFlagUnchanged = run.bruteCoarseEnvelopeFlagApplied;
            run.bruteRejectedBindingReuseFlagUnchanged = run.bruteRejectedBindingReuseFlagApplied;
            Require(run.bruteCoarseEnvelopeFlagApplied && run.bruteRejectedBindingReuseFlagApplied, EnvelopeBindingOwnerError);
        }

        private static void ObserveBruteEnvelopeBinding(RunState run)
        {
            if (envelopeBindingQuery == null)
            {
                run.bruteCoarseEnvelopeFlagUnchanged = false;
                run.bruteRejectedBindingReuseFlagUnchanged = false;
                if (run.bruteEnvelopeBindingEligibilityMode)
                    run.bruteEligibilityReuseFlagUnchanged = false;
                if (run.bruteEnvelopeBranchTimingMode)
                {
                    run.bruteEnvelopeBranchTimingFlagUnchanged = false;
                    run.bruteBranchTimingFlagUnchanged = false;
                }
                return;
            }
            bool defaultsValid = run.bruteEnvelopeBranchTimingMode
                ? EnvelopeBranchTimingScopeValid(envelopeBindingQuery, run)
                : run.bruteEnvelopeBindingEligibilityMode
                ? EnvelopeBindingEligibilityScopeValid(envelopeBindingQuery, run)
                : EnvelopeBindingQueryDefaultsValid(envelopeBindingQuery);
            if (run.bruteEnvelopeBranchTimingMode)
            {
                run.bruteEnvelopeBranchTimingFlagUnchanged &= defaultsValid;
                run.bruteBranchTimingFlagUnchanged &= defaultsValid;
            }
            if (run.bruteEnvelopeBindingEligibilityMode)
            {
                run.bruteEligibilityReuseFlagUnchanged &= defaultsValid;
                run.bruteEligibilityReuseObservedApplied |= envelopeBindingQuery.LastBruteEligibilityReuseAppliedForDiagnostics;
            }
            run.bruteCoarseEnvelopeFlagUnchanged &= defaultsValid && envelopeBindingQuery.EnableBruteCoarseEnvelopeForDiagnostics;
            run.bruteRejectedBindingReuseFlagUnchanged &= defaultsValid &&
                envelopeBindingQuery.EnableBruteRejectedBindingReuseForDiagnostics == run.bruteRejectedBindingReuseEnabled;
            run.bruteCoarseEnvelopeMaximumObservedDirections = Math.Max(run.bruteCoarseEnvelopeMaximumObservedDirections,
                envelopeBindingQuery.LastBruteCoarseEnvelopeDirectionCountForDiagnostics);
            run.bruteCoarseEnvelopeMaximumObservedRejects = Math.Max(run.bruteCoarseEnvelopeMaximumObservedRejects,
                envelopeBindingQuery.LastBruteCoarseEnvelopeRejectCountForDiagnostics);
            run.bruteRejectedBindingReuseObservedApplied |= envelopeBindingQuery.LastBruteRejectedBindingReuseAppliedForDiagnostics;
            run.bruteRejectedBindingMaximumObservedProbes = Math.Max(run.bruteRejectedBindingMaximumObservedProbes,
                envelopeBindingQuery.LastBruteRejectedBindingProbeCountForDiagnostics);
            run.bruteRejectedBindingMaximumObservedReuses = Math.Max(run.bruteRejectedBindingMaximumObservedReuses,
                envelopeBindingQuery.LastBruteRejectedBindingReuseCountForDiagnostics);
        }

        private static void CompleteBruteEnvelopeBinding(RunState run)
        {
            ObserveBruteEnvelopeBinding(run);
            Require(envelopeBindingQuery != null && run.bruteCoarseEnvelopeFlagApplied &&
                run.bruteRejectedBindingReuseFlagApplied && run.bruteCoarseEnvelopeFlagUnchanged &&
                run.bruteRejectedBindingReuseFlagUnchanged, EnvelopeBindingDriftError);
            if (run.bruteEnvelopeBindingEligibilityMode)
            {
                Require(envelopeBindingEligibilityOwner && run.bruteEligibilityReuseFlagApplied &&
                    run.bruteEligibilityReuseFlagUnchanged, EnvelopeBindingDriftError);
                run.bruteEligibilityReuseAppliedDelta =
                    envelopeBindingQuery.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics -
                    run.bruteEligibilityReuseAppliedBaseline;
            }
            if (run.bruteEnvelopeBranchTimingMode)
            {
                Require(envelopeBindingBranchTimingOwner && run.bruteEnvelopeBranchTimingFlagApplied &&
                    run.bruteEnvelopeBranchTimingFlagUnchanged && run.bruteBranchTimingFlagApplied &&
                    run.bruteBranchTimingFlagUnchanged, EnvelopeBindingDriftError);
                run.bruteBranchTimingCoverage = envelopeBindingQuery.TotalBruteBranchTimingCoverageForDiagnostics;
                run.bruteBranchTimingCoverageScope = "Totals include warmup and sampled collections; clocks only stride64-selected directions, not full-cost or steady-only.";
            }
            bool restored = RestoreBruteEnvelopeBinding();
            run.bruteCoarseEnvelopeRestored = restored;
            run.bruteRejectedBindingReuseRestored = restored;
            if (run.bruteEnvelopeBindingEligibilityMode)
                run.bruteEligibilityReuseRestored = restored;
            if (run.bruteEnvelopeBranchTimingMode)
            {
                run.bruteEnvelopeBranchTimingRestored = restored;
                run.bruteBranchTimingRestored = restored;
            }
        }

        private static bool EnvelopeBindingObservationValid(RunState run) =>
            run.bruteCoarseEnvelopeEnabled && CoarseEnvelopeObservationValid(run) &&
            run.bruteRejectedBindingReuseFlagApplied && run.bruteRejectedBindingReuseFlagUnchanged &&
            run.bruteRejectedBindingReuseRestored &&
            (run.bruteRejectedBindingReuseEnabled
                ? run.bruteRejectedBindingReuseObservedApplied && run.bruteRejectedBindingMaximumObservedProbes > 0 &&
                    run.bruteRejectedBindingMaximumObservedReuses > 0
                : !run.bruteRejectedBindingReuseObservedApplied && run.bruteRejectedBindingMaximumObservedProbes == 0 &&
                    run.bruteRejectedBindingMaximumObservedReuses == 0);

        private static bool EnvelopeBindingEligibilityObservationValid(RunState run) =>
            run.bruteEnvelopeBindingEligibilityMode && EnvelopeBindingObservationValid(run) &&
            run.bruteEligibilityReuseFlagApplied && run.bruteEligibilityReuseFlagUnchanged &&
            run.bruteEligibilityReuseRestored &&
            run.bruteEligibilityReuseObservedApplied == run.bruteEligibilityReuseEnabled &&
            run.bruteEligibilityReuseAppliedDelta == (run.bruteEligibilityReuseEnabled ? run.warmupTicks + run.sampledTicks : 0);

        private static bool EnvelopeBranchTimingObservationValid(RunState run)
        {
            if (!run.bruteEnvelopeBranchTimingMode || !EnvelopeBindingObservationValid(run) ||
                !run.bruteEnvelopeBranchTimingFlagApplied || !run.bruteEnvelopeBranchTimingFlagUnchanged ||
                !run.bruteEnvelopeBranchTimingRestored || !run.bruteBranchTimingFlagApplied ||
                !run.bruteBranchTimingFlagUnchanged || !run.bruteBranchTimingRestored ||
                run.bruteBranchTimingSampleStride != 64)
                return false;
            var coverage = run.bruteBranchTimingCoverage;
            if (!run.bruteBranchTimingEnabled)
                return coverage.eligibleDirections == 0 && coverage.timedDirections == 0 &&
                    coverage.rejectedBindingVisits == 0 && coverage.rejectedBindingTimed == 0 &&
                    coverage.pairAllowedVisits == 0 && coverage.pairAllowedTimed == 0 &&
                    coverage.exactWorkVisits == 0 && coverage.exactWorkTimed == 0;
            return coverage.eligibleDirections > 0 && coverage.timedDirections > 0 &&
                coverage.timedDirections <= coverage.eligibleDirections &&
                coverage.rejectedBindingVisits > 0 && coverage.rejectedBindingTimed > 0 &&
                coverage.rejectedBindingTimed <= coverage.rejectedBindingVisits &&
                coverage.pairAllowedVisits > 0 && coverage.pairAllowedTimed > 0 &&
                coverage.pairAllowedTimed <= coverage.pairAllowedVisits &&
                coverage.exactWorkVisits > 0 && coverage.exactWorkTimed > 0 &&
                coverage.exactWorkTimed <= coverage.exactWorkVisits;
        }

        private static bool RestoreBruteEnvelopeBindingForExit()
        {
            bool restored = RestoreBruteEnvelopeBinding();
            if (state != null && (state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                state.bruteEnvelopeBranchTimingCandidate) &&
                state.runIndex >= 0 &&
                state.runIndex < state.runs.Length)
            {
                RunState run = state.runs[state.runIndex];
                run.bruteCoarseEnvelopeRestored = restored;
                run.bruteRejectedBindingReuseRestored = restored;
                if (run.bruteEnvelopeBindingEligibilityMode)
                    run.bruteEligibilityReuseRestored = restored;
                if (run.bruteEnvelopeBranchTimingMode)
                {
                    run.bruteEnvelopeBranchTimingRestored = restored;
                    run.bruteBranchTimingRestored = restored;
                }
            }
            return restored;
        }

        private static bool RestoreBruteEnvelopeBinding()
        {
            if (envelopeBindingQuery == null)
                return true;
            envelopeBindingQuery.EnableBruteCoarseEnvelopeForDiagnostics = previousEnvelopeBindingEnvelope;
            envelopeBindingQuery.EnableBruteRejectedBindingReuseForDiagnostics = previousEnvelopeBindingReuse;
            bool restored = envelopeBindingQuery.EnableBruteCoarseEnvelopeForDiagnostics == previousEnvelopeBindingEnvelope &&
                envelopeBindingQuery.EnableBruteRejectedBindingReuseForDiagnostics == previousEnvelopeBindingReuse;
            if (envelopeBindingEligibilityOwner)
            {
                envelopeBindingQuery.EnableBruteEligibilityReuseForDiagnostics = previousEnvelopeBindingEligibility;
                restored &= envelopeBindingQuery.EnableBruteEligibilityReuseForDiagnostics == previousEnvelopeBindingEligibility;
            }
            if (envelopeBindingBranchTimingOwner)
            {
                envelopeBindingQuery.EnableBruteEnvelopeBranchTimingForDiagnostics = previousEnvelopeBranchTiming;
                envelopeBindingQuery.EnableBruteBranchTimingForDiagnostics = previousBranchTiming;
                envelopeBindingQuery.BruteBranchTimingSampleStrideForDiagnostics = previousBranchTimingSampleStride;
                restored &= envelopeBindingQuery.EnableBruteEnvelopeBranchTimingForDiagnostics == previousEnvelopeBranchTiming &&
                    envelopeBindingQuery.EnableBruteBranchTimingForDiagnostics == previousBranchTiming &&
                    envelopeBindingQuery.BruteBranchTimingSampleStrideForDiagnostics == previousBranchTimingSampleStride;
            }
            envelopeBindingEligibilityOwner = false;
            envelopeBindingBranchTimingOwner = false;
            envelopeBindingQuery = null;
            return restored;
        }

        private static bool CoarseEnvelopeQueryDefaultsValid(BruteForceSceneQuery query) =>
            query != null && ProductionDefaultsEnabled(query) &&
            !query.EnableBruteKind5PresenceForDiagnostics && !query.EnableBruteEligibilityReuseForDiagnostics &&
            !query.EnableBruteRejectedBindingReuseForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            !query.EnableBruteBranchTimingForDiagnostics && !query.EnableBruteEnvelopeBranchTimingForDiagnostics;

        private static void ApplyRenderTextureBindingReuse(RunState run, bool enabled)
        {
            Require(run != null && !renderTextureBindingReuseOwner &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics, RenderTextureBindingOwnerError);
            previousRenderTextureBindingReuse = BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics;
            renderTextureBindingReuseOwner = true;
            renderTextureBindingReuseRun = run;
            lastRenderTextureBindingExecuteSequence = BattleRenderFeature.LastSegmentTextureBindingExecuteSequenceForDiagnostics;
            run.renderTextureBindingReuseEnabled = enabled;
            BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics = enabled;
            run.renderTextureBindingReuseFlagApplied = BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == enabled;
            run.renderTextureBindingReuseFlagUnchanged = run.renderTextureBindingReuseFlagApplied;
            RenderPipelineManager.endCameraRendering += ObserveRenderTextureBindingCamera;
        }

        private static bool IsRenderTextureBindingObservationFresh(
            long previousSequence, long currentSequence, int expectedCameraId, int actualCameraId,
            int prepareCount, int reuseCount) =>
            previousSequence >= 0 && currentSequence > previousSequence && expectedCameraId != 0 &&
            actualCameraId == expectedCameraId && prepareCount > 0 && reuseCount >= 0;

        private static void ObserveRenderTextureBindingCamera(ScriptableRenderContext context, Camera camera)
        {
            RunState run = renderTextureBindingReuseRun;
            if (!renderTextureBindingReuseOwner || run == null)
                return;
            run.renderTextureBindingReuseFlagUnchanged &=
                BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == run.renderTextureBindingReuseEnabled;
            if (!Application.isPlaying || camera == null || camera.cameraType != CameraType.Game ||
                currentReport == null || currentReport.status != "Running" ||
                currentReport.warmupTicksCompleted < 120 || currentReport.sampledLogicTicks <= 0)
                return;
            int cameraId = camera.GetInstanceID();
            if (run.renderTextureBindingCameraId != 0 && run.renderTextureBindingCameraId != cameraId)
                return;
            long sequence = BattleRenderFeature.LastSegmentTextureBindingExecuteSequenceForDiagnostics;
            int prepareCount = BattleRenderFeature.LastSegmentTextureBindingPrepareCountForDiagnostics;
            int reuseCount = BattleRenderFeature.LastSegmentTextureBindingReuseCountForDiagnostics;
            if (!IsRenderTextureBindingObservationFresh(lastRenderTextureBindingExecuteSequence, sequence,
                    cameraId,
                    BattleRenderFeature.LastSegmentTextureBindingCameraIdForDiagnostics, prepareCount, reuseCount))
                return;
            run.renderTextureBindingCameraId = cameraId;
            lastRenderTextureBindingExecuteSequence = sequence;
            run.renderTextureBindingAcceptedCameraSamples++;
            run.renderTextureBindingBodyPrepareCount += prepareCount;
            run.renderTextureBindingBodyReuseCount += reuseCount;
        }

        private static void CompleteRenderTextureBindingReuse(RunState run)
        {
            Require(renderTextureBindingReuseOwner && ReferenceEquals(renderTextureBindingReuseRun, run) &&
                run.renderTextureBindingReuseFlagApplied, RenderTextureBindingOwnerError);
            run.renderTextureBindingReuseFlagUnchanged &=
                BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == run.renderTextureBindingReuseEnabled;
            Require(run.renderTextureBindingReuseFlagUnchanged, RenderTextureBindingDriftError);
            Require(run.renderTextureBindingAcceptedCameraSamples > 0 && run.renderTextureBindingBodyPrepareCount > 0 &&
                run.renderTextureBindingBodyReuseCount >= 0 &&
                (run.renderTextureBindingReuseEnabled || run.renderTextureBindingBodyReuseCount == 0),
                RenderTextureBindingObservationError);
            run.renderTextureBindingReuseRestored = RestoreRenderTextureBindingReuse();
        }

        private static bool RestoreRenderTextureBindingReuse()
        {
            if (!renderTextureBindingReuseOwner)
                return true;
            RenderPipelineManager.endCameraRendering -= ObserveRenderTextureBindingCamera;
            BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics = previousRenderTextureBindingReuse;
            bool restored = BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == previousRenderTextureBindingReuse;
            if (renderTextureBindingReuseRun != null)
                renderTextureBindingReuseRun.renderTextureBindingReuseRestored = restored;
            renderTextureBindingReuseOwner = false;
            renderTextureBindingReuseRun = null;
            lastRenderTextureBindingExecuteSequence = 0;
            return restored;
        }


        private static int CentralSegmentBreakMask(
            BattleCentralRenderSegment previous, BattleCentralRenderSegment next, BattleCentralDrawMode drawMode)
        {
            // Alignment contract: NTSD-OPT-H07-CENTRAL-SEGMENT-READBACK-075; mirrors only observed CPU split conditions.
            int mask = drawMode == BattleCentralDrawMode.StrictOrderedDraw ? 1 : 0;
            if (previous.ChunkIndex != next.ChunkIndex) mask |= 2;
            if (!ReferenceEquals(previous.Texture, next.Texture)) mask |= 4;
            if (!ReferenceEquals(previous.Material, next.Material)) mask |= 8;
            if (previous.MaterialVariant != next.MaterialVariant) mask |= 16;
            if (previous.BindingMode != next.BindingMode) mask |= 32;
            if (next.BindingMode == BattleSpriteCentralBindingMode.AtlasPageTexture2D &&
                previous.AtlasPageIndex != next.AtlasPageIndex) mask |= 64;
            if (next.BindingMode != BattleSpriteCentralBindingMode.AtlasTextureArray &&
                previous.AtlasSlice != next.AtlasSlice) mask |= 128;
            if (previous.FirstCommandIndex + previous.CommandCount != next.FirstCommandIndex) mask |= 256;
            if (previous.ChunkIndex == next.ChunkIndex &&
                previous.FirstQuad + previous.QuadCount != next.FirstQuad) mask |= 512;
            return mask;
        }

        private static bool CentralSegmentReadbackPending(RunState run) =>
            run != null && !run.centralSegmentReadbackCaptured && string.IsNullOrEmpty(run.centralSegmentReadbackError);

        private static void ObserveCentralSegmentReadback(ScriptableRenderContext context, Camera camera)
        {
            RunState run = centralSegmentReadbackRun;
            if (!CentralSegmentReadbackPending(run) ||
                !Application.isPlaying || camera == null || camera.cameraType != CameraType.Game ||
                currentReport == null || currentReport.status != "Running" ||
                currentReport.warmupTicksCompleted < 120 || currentReport.sampledLogicTicks < 8)
                return;
            CameraRenderType renderType = camera.TryGetComponent(out UniversalAdditionalCameraData cameraData)
                ? cameraData.renderType : CameraRenderType.Base;
            if (!BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    camera, renderType, camera.cameraType, true, out BattleCentralSubmission.BattleCentralSubmissionLease lease))
                return;
            CentralSegmentReadbackSnapshot snapshot = null;
            try
            {
                try
                {
                    Require(OrdinalPacketQueryDefaultsValid(productionQuery) &&
                        !productionQuery.EnableBruteOrdinalPacketForDiagnostics &&
                        !productionQuery.EnableBruteDepthRejectForDiagnostics, "A diagnostic candidate drifted during readback.");
                    BattleDynamicMeshBackend backend = lease.Backend;
                    BattleCentralBuildDiagnostics diagnostics = backend.Diagnostics;
                    BattleDrawPolicyDecision draw = BattleCentralRenderSystem.DrawPolicyDecision;
                    snapshot = new CentralSegmentReadbackSnapshot
                    {
                        utc = DateTime.UtcNow.ToString("o"), tick = lease.TickIndex, generation = lease.Generation,
                        cameraIdentity = camera.GetInstanceID(), sampledLogicTicks = currentReport.sampledLogicTicks,
                        activeAi = currentReport.baseAiActiveCount,
                        drawMode = diagnostics.DrawMode.ToString(), drawPolicyRequested = draw.RequestedMode.ToString(),
                        drawPolicyEffective = draw.EffectiveMode.ToString(), drawPolicyReason = draw.FallbackOrRefusalReason,
                        sourceCommands = diagnostics.SourceCommandCount, resolvedCommands = diagnostics.ResolvedCommandCount,
                        unresolvedCommands = diagnostics.UnresolvedCommandCount,
                        unsupportedCategories = diagnostics.UnsupportedCategoryCount,
                        unsupportedStates = diagnostics.UnsupportedRenderStateCount,
                        chunks = backend.ActiveChunkCount, segments = backend.SegmentCount,
                        rows = new CentralSegmentReadbackRow[backend.SegmentCount],
                    };
                    CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                    if (manager != null)
                    {
                        BattleAtlasDiagnosticInputs atlas = manager.LastAtlasDiagnosticInputs;
                        snapshot.atlasDiagnostic = manager.LastAtlasDiagnostic;
                        if (atlas != null)
                        {
                            snapshot.atlasInputsKnown = true;
                            snapshot.atlasRequested = atlas.Decision.RequestedMode.ToString();
                            snapshot.atlasEffective = atlas.Decision.EffectiveMode.ToString();
                            snapshot.atlasReason = atlas.Decision.FallbackOrRefusalReason;
                            snapshot.catalogMode = atlas.CatalogResourceMode.ToString();
                            snapshot.plannedPageCount = atlas.PlannedPageCount;
                            snapshot.estimatedAtlasBytes = atlas.EstimatedAtlasBytes;
                            snapshot.atlasBudgetBytes = atlas.Capabilities.AtlasMemoryBudgetBytes;
                        }
                    }
                    for (int index = 0; index < backend.SegmentCount; index++)
                    {
                        BattleCentralRenderSegment segment = backend.GetSegment(index);
                        Texture texture = segment.Texture;
                        Material material = segment.Material;
                        Shader shader = material == null ? null : material.shader;
                        int textureId = texture == null ? 0 : texture.GetInstanceID();
                        snapshot.rows[index] = new CentralSegmentReadbackRow
                        {
                            index = index, chunk = segment.ChunkIndex, subMesh = segment.SubMeshIndex,
                            firstCommand = segment.FirstCommandIndex, commands = segment.CommandCount,
                            firstQuad = segment.FirstQuad, quads = segment.QuadCount,
                            bindingMode = segment.BindingMode.ToString(), boundTextureIdentity = textureId,
                            SourceTextureIdentity = segment.BindingMode == BattleSpriteCentralBindingMode.SourceTexture2D ? textureId : 0,
                            AtlasPageIdentity = segment.BindingMode == BattleSpriteCentralBindingMode.AtlasPageTexture2D ? textureId : 0,
                            TextureArrayIdentity = segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ? textureId : 0,
                            textureName = texture == null ? null : texture.name,
                            textureType = texture == null ? null : texture.GetType().Name,
                            textureAssetPath = texture == null ? null : AssetDatabase.GetAssetPath(texture),
                            slice = segment.AtlasSlice, pageIndex = segment.AtlasPageIndex,
                            materialIdentity = material == null ? 0 : material.GetInstanceID(), variant = segment.MaterialVariant,
                            materialName = material == null ? null : material.name,
                            shaderIdentity = shader == null ? 0 : shader.GetInstanceID(),
                            shaderName = shader == null ? null : shader.name,
                            shaderPassCount = material == null ? 0 : material.passCount,
                            renderQueue = material == null ? 0 : material.renderQueue,
                            shaderKeywords = material == null ? null : string.Join(",", material.shaderKeywords),
                            breakMaskFromPrevious = index == 0 ? 0 : CentralSegmentBreakMask(
                                backend.GetSegment(index - 1), segment, diagnostics.DrawMode),
                        };
                    }
                }
                finally
                {
                    lease.Dispose();
                }
                snapshot.cpuLeaseReleased = lease.Submission.ReadLeaseCount == 0;
                Require(snapshot.cpuLeaseReleased, "The readback CPU lease must be returned in the same callback.");
                SaveNew(run.reportPath + ".central-segments.json", JsonUtility.ToJson(snapshot, true));
                run.centralSegmentReadbackCaptured = true;
            }
            catch (Exception exception)
            {
                run.centralSegmentReadbackError = exception.ToString();
            }
            finally
            {
                RestoreCentralSegmentReadback();
            }
        }

        private static bool RestoreCentralSegmentReadback()
        {
            RenderPipelineManager.endCameraRendering -= ObserveCentralSegmentReadback;
            if (centralSegmentReadbackRun != null)
                centralSegmentReadbackRun.centralSegmentReadbackDetached = true;
            centralSegmentReadbackRun = null;
            return true;
        }

        private static bool OrdinalPacketQueryDefaultsValid(BruteForceSceneQuery query) =>
            query != null && query.FormalCollectorMode == CollisionFormalCollectorMode.ForceBruteForce &&
            ProductionDefaultsEnabled(query) && !query.EnableBruteKind5PresenceForDiagnostics &&
            !query.EnableBruteEligibilityReuseForDiagnostics && !query.EnableBruteCoarseEnvelopeForDiagnostics &&
            !query.EnableBruteRejectedBindingReuseForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            !query.EnableBruteCoarseProofReuseForDiagnostics && !query.EnableBruteBranchTimingForDiagnostics &&
            !query.EnableBruteEnvelopeBranchTimingForDiagnostics &&
            !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics;

        private static void ApplyBruteOrdinalPacket(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            // Alignment contract: NTSD-OPT-H07-ORDINAL-PACKET-WINDOWS-072; cold flag ownership, not production admission.
            Require(run != null && ordinalPacketQuery == null && ordinalPacketRun == null &&
                coarseEnvelopeQuery == null && envelopeBindingQuery == null && kind5PresenceQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && !renderTextureBindingReuseOwner &&
                OrdinalPacketQueryDefaultsValid(query) && !query.EnableBruteOrdinalPacketForDiagnostics,
                "The ordinal packet window requires exclusive query/run ownership, four production defaults and no other candidate.");
            ordinalPacketQuery = query;
            ordinalPacketRun = run;
            previousOrdinalPacket = query.EnableBruteOrdinalPacketForDiagnostics;
            run.bruteOrdinalPacketEnabled = enabled;
            run.bruteOrdinalPacketCapacityBeforePopulation = query.BruteOrdinalPacketCapacityForDiagnostics;
            run.bruteOrdinalPacketCapacityStable = true;
            query.EnableBruteOrdinalPacketForDiagnostics = enabled;
            run.bruteOrdinalPacketFlagApplied = query.EnableBruteOrdinalPacketForDiagnostics == enabled;
            run.bruteOrdinalPacketFlagUnchanged = run.bruteOrdinalPacketFlagApplied;
        }

        private static void ObserveBruteOrdinalPacket(RunState run, int logicTick)
        {
            Require(run != null && ordinalPacketQuery != null && ReferenceEquals(ordinalPacketRun, run),
                "The ordinal packet window observation requires its owned run.");
            run.bruteOrdinalPacketFlagUnchanged &= OrdinalPacketQueryDefaultsValid(ordinalPacketQuery) &&
                ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics == run.bruteOrdinalPacketEnabled;
            if (logicTick <= 0 || logicTick <= run.bruteOrdinalPacketLastObservedTick)
                return;
            int capacity = ordinalPacketQuery.BruteOrdinalPacketCapacityForDiagnostics;
            if (run.bruteOrdinalPacketObservedSamples == 0)
                run.bruteOrdinalPacketSteadyCapacity = capacity;
            run.bruteOrdinalPacketCapacityStable &= capacity > 0 && capacity == run.bruteOrdinalPacketSteadyCapacity;
            run.bruteOrdinalPacketMaximumObservedCapacity = Math.Max(run.bruteOrdinalPacketMaximumObservedCapacity, capacity);
            run.bruteOrdinalPacketLastObservedTick = logicTick;
            run.bruteOrdinalPacketObservedSamples++;
            run.bruteOrdinalPacketObservedApplied |= ordinalPacketQuery.LastBruteOrdinalPacketAppliedForDiagnostics;
            run.bruteOrdinalPacketFallbackObserved |= ordinalPacketQuery.LastBruteOrdinalPacketFallbackForDiagnostics;
            run.bruteOrdinalPacketMaximumObservedRejects = Math.Max(run.bruteOrdinalPacketMaximumObservedRejects,
                ordinalPacketQuery.LastBruteOrdinalPacketRejectedDirectionCountForDiagnostics);
            run.bruteOrdinalPacketMaximumObservedProofs = Math.Max(run.bruteOrdinalPacketMaximumObservedProofs,
                ordinalPacketQuery.LastBruteOrdinalPacketProofCountForDiagnostics);
        }

        private static void CompleteBruteOrdinalPacket(RunState run)
        {
            Require(run != null && ordinalPacketQuery != null && ReferenceEquals(ordinalPacketRun, run),
                "The ordinal packet window completion requires its owned run.");
            run.bruteOrdinalPacketFlagUnchanged &= OrdinalPacketQueryDefaultsValid(ordinalPacketQuery) &&
                ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics == run.bruteOrdinalPacketEnabled;
            // Restore even when evidence is invalid; a wrong run never reaches this release.
            run.bruteOrdinalPacketRestored = RestoreBruteOrdinalPacket();
            Require(OrdinalPacketObservationValid(run),
                "The ordinal packet window requires fresh application evidence, unchanged flags/capacity and no observed fallback.");
        }

        private static bool OrdinalPacketObservationValid(RunState run) =>
            run != null && run.bruteOrdinalPacketFlagApplied && run.bruteOrdinalPacketFlagUnchanged &&
            run.bruteOrdinalPacketRestored && run.bruteOrdinalPacketObservedSamples > 0 &&
            run.bruteOrdinalPacketCapacityStable && run.bruteOrdinalPacketSteadyCapacity > 0 &&
            !run.bruteOrdinalPacketFallbackObserved &&
            (run.bruteOrdinalPacketEnabled
                ? run.bruteOrdinalPacketObservedApplied && run.bruteOrdinalPacketMaximumObservedRejects > 0 &&
                    run.bruteOrdinalPacketMaximumObservedProofs > 0
                : !run.bruteOrdinalPacketObservedApplied && run.bruteOrdinalPacketMaximumObservedRejects == 0 &&
                    run.bruteOrdinalPacketMaximumObservedProofs == 0);

        private static bool RestoreBruteOrdinalPacket()
        {
            if (ordinalPacketQuery == null)
                return true;
            ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics = previousOrdinalPacket;
            bool restored = ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics == previousOrdinalPacket;
            if (ordinalPacketRun != null)
                ordinalPacketRun.bruteOrdinalPacketRestored = restored;
            ordinalPacketQuery = null;
            ordinalPacketRun = null;
            return restored;
        }

        private static void ApplyBruteCoarseEnvelope(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            Require(coarseEnvelopeQuery == null && CoarseEnvelopeQueryDefaultsValid(query) &&
                !query.EnableBruteCoarseEnvelopeForDiagnostics, CoarseEnvelopeOwnerError);
            coarseEnvelopeQuery = query;
            previousCoarseEnvelope = query.EnableBruteCoarseEnvelopeForDiagnostics;
            run.bruteCoarseEnvelopeEnabled = enabled;
            query.EnableBruteCoarseEnvelopeForDiagnostics = enabled;
            run.bruteCoarseEnvelopeFlagApplied = query.EnableBruteCoarseEnvelopeForDiagnostics == enabled;
            run.bruteCoarseEnvelopeFlagUnchanged = run.bruteCoarseEnvelopeFlagApplied;
            Require(run.bruteCoarseEnvelopeFlagApplied, CoarseEnvelopeOwnerError);
        }

        private static void ObserveBruteCoarseEnvelope(RunState run)
        {
            if (coarseEnvelopeQuery == null)
            {
                run.bruteCoarseEnvelopeFlagUnchanged = false;
                return;
            }
            run.bruteCoarseEnvelopeFlagUnchanged &= CoarseEnvelopeQueryDefaultsValid(coarseEnvelopeQuery) &&
                coarseEnvelopeQuery.EnableBruteCoarseEnvelopeForDiagnostics == run.bruteCoarseEnvelopeEnabled;
            run.bruteCoarseEnvelopeMaximumObservedDirections = Math.Max(run.bruteCoarseEnvelopeMaximumObservedDirections,
                coarseEnvelopeQuery.LastBruteCoarseEnvelopeDirectionCountForDiagnostics);
            run.bruteCoarseEnvelopeMaximumObservedRejects = Math.Max(run.bruteCoarseEnvelopeMaximumObservedRejects,
                coarseEnvelopeQuery.LastBruteCoarseEnvelopeRejectCountForDiagnostics);
        }

        private static void CompleteBruteCoarseEnvelope(RunState run)
        {
            ObserveBruteCoarseEnvelope(run);
            Require(coarseEnvelopeQuery != null && run.bruteCoarseEnvelopeFlagApplied &&
                run.bruteCoarseEnvelopeFlagUnchanged, CoarseEnvelopeDriftError);
            run.bruteCoarseEnvelopeRestored = RestoreBruteCoarseEnvelope();
        }

        private static bool CoarseEnvelopeObservationValid(RunState run) =>
            run.bruteCoarseEnvelopeFlagApplied && run.bruteCoarseEnvelopeFlagUnchanged && run.bruteCoarseEnvelopeRestored &&
            (run.bruteCoarseEnvelopeEnabled
                ? run.bruteCoarseEnvelopeMaximumObservedDirections > 0 && run.bruteCoarseEnvelopeMaximumObservedRejects > 0
                : run.bruteCoarseEnvelopeMaximumObservedDirections == 0 && run.bruteCoarseEnvelopeMaximumObservedRejects == 0);

        private static bool RestoreBruteCoarseEnvelope()
        {
            if (coarseEnvelopeQuery == null)
                return true;
            coarseEnvelopeQuery.EnableBruteCoarseEnvelopeForDiagnostics = previousCoarseEnvelope;
            bool restored = coarseEnvelopeQuery.EnableBruteCoarseEnvelopeForDiagnostics == previousCoarseEnvelope;
            coarseEnvelopeQuery = null;
            return restored;
        }

        private static void ApplyBruteCombinedCache(BruteForceSceneQuery query, RunState run, bool kind5Enabled)
        {
            Require(kind5PresenceQuery == null, "Another kind5 comparison owns the query.");
            ApplyBruteEligibilityReuse(query, run, true);
            kind5PresenceQuery = query;
            previousKind5Presence = query.EnableBruteKind5PresenceForDiagnostics;
            run.bruteKind5PresenceEnabled = kind5Enabled;
            run.bruteKind5PresenceAppliedBaseline = query.TotalBruteKind5PresenceCollectionAppliedForDiagnostics;
            query.EnableBruteKind5PresenceForDiagnostics = kind5Enabled;
            run.bruteKind5PresenceFlagApplied = query.EnableBruteKind5PresenceForDiagnostics == kind5Enabled;
            Require(run.bruteKind5PresenceFlagApplied, "Combined kind5 configuration failed.");
        }

        private static void CompleteBruteCombinedCache(RunState run)
        {
            Require(kind5PresenceQuery != null && eligibilityReuseQuery == kind5PresenceQuery &&
                run.bruteEligibilityReuseEnabled && run.bruteEligibilityReuseFlagApplied &&
                kind5PresenceQuery.EnableBruteEligibilityReuseForDiagnostics &&
                run.bruteKind5PresenceFlagApplied &&
                kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics == run.bruteKind5PresenceEnabled &&
                ProductionDefaultsEnabled(kind5PresenceQuery) &&
                !kind5PresenceQuery.EnableBruteRejectedBindingReuseForDiagnostics &&
                !kind5PresenceQuery.EnableBruteBranchTimingForDiagnostics,
                "Combined comparison flags drifted during the fixed window.");
            run.bruteKind5PresenceAppliedDelta =
                kind5PresenceQuery.TotalBruteKind5PresenceCollectionAppliedForDiagnostics -
                run.bruteKind5PresenceAppliedBaseline;
            run.bruteKind5PresenceRestored = RestoreBruteKind5Presence();
            CompleteBruteEligibilityReuse(run);
        }

        private static void ApplyBruteEligibilityReuse(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            Require(eligibilityReuseQuery == null && query != null && ProductionDefaultsEnabled(query) &&
                !query.EnableBruteEligibilityReuseForDiagnostics && !query.EnableBruteKind5PresenceForDiagnostics &&
                !query.EnableBruteRejectedBindingReuseForDiagnostics && !query.EnableBruteBranchTimingForDiagnostics,
                "Eligibility comparison requires the same four production defaults and no other candidate or timing flag.");
            eligibilityReuseQuery = query;
            previousEligibilityReuse = query.EnableBruteEligibilityReuseForDiagnostics;
            run.bruteEligibilityReuseEnabled = enabled;
            run.bruteEligibilityReuseAppliedBaseline = query.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics;
            query.EnableBruteEligibilityReuseForDiagnostics = enabled;
            run.bruteEligibilityReuseFlagApplied = query.EnableBruteEligibilityReuseForDiagnostics == enabled;
            Require(run.bruteEligibilityReuseFlagApplied, "Eligibility flag configuration failed.");
        }

        private static void CompleteBruteEligibilityReuse(RunState run)
        {
            Require(eligibilityReuseQuery != null && run.bruteEligibilityReuseFlagApplied &&
                eligibilityReuseQuery.EnableBruteEligibilityReuseForDiagnostics == run.bruteEligibilityReuseEnabled &&
                ProductionDefaultsEnabled(eligibilityReuseQuery) &&
                !eligibilityReuseQuery.EnableBruteKind5PresenceForDiagnostics &&
                !eligibilityReuseQuery.EnableBruteRejectedBindingReuseForDiagnostics &&
                !eligibilityReuseQuery.EnableBruteBranchTimingForDiagnostics,
                "Eligibility comparison flags drifted during the fixed window.");
            run.bruteEligibilityReuseAppliedDelta =
                eligibilityReuseQuery.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics -
                run.bruteEligibilityReuseAppliedBaseline;
            run.bruteEligibilityReuseRestored = RestoreBruteEligibilityReuse();
        }

        private static bool RestoreBruteEligibilityReuse()
        {
            if (eligibilityReuseQuery == null)
                return true;
            eligibilityReuseQuery.EnableBruteEligibilityReuseForDiagnostics = previousEligibilityReuse;
            bool restored = eligibilityReuseQuery.EnableBruteEligibilityReuseForDiagnostics == previousEligibilityReuse;
            eligibilityReuseQuery = null;
            return restored;
        }

        private static bool RestoreBruteBranchTiming()
        {
            if (branchTimingQuery == null)
                return true;
            branchTimingQuery.EnableBruteBranchTimingForDiagnostics = previousBranchTiming;
            branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics = previousBranchTimingSampleStride;
            bool restored = branchTimingQuery.EnableBruteBranchTimingForDiagnostics == previousBranchTiming &&
                branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics == previousBranchTimingSampleStride;
            branchTimingQuery = null;
            return restored;
        }

        private static bool RestoreBruteKind5Presence()
        {
            if (kind5PresenceQuery == null)
                return true;
            kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics = previousKind5Presence;
            bool restored = kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics == previousKind5Presence;
            kind5PresenceQuery = null;
            return restored;
        }

        private static bool RestoreEmptyItrGuard()
        {
            if (emptyItrGuardQuery == null)
                return true;
            emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics = previousEmptyItrGuard;
            emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics = previousEmptyItrRoster;
            emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics = previousBruteExactCache;
            emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics = previousBruteGeometryFirst;
            bool restored = emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics == previousEmptyItrGuard &&
                emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics == previousEmptyItrRoster &&
                emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics == previousBruteExactCache &&
                emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics == previousBruteGeometryFirst;
            emptyItrGuardQuery = null;
            return restored;
        }

        private static string HashFile(string path)
        {
            using (var hash = SHA256.Create())
            using (var input = File.OpenRead(PathFor(path)))
                return BitConverter.ToString(hash.ComputeHash(input)).Replace("-", string.Empty);
        }

        private static void SaveNew(string path, string content)
        {
            path = PathFor(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(content);
        }

        private static void Require(bool condition, string reason)
        {
            if (!condition)
                throw new InvalidOperationException(reason);
        }
    }

#if UNITY_INCLUDE_TESTS
    public sealed class BattleOptimizationWindowsAiSuiteRequestTests
    {
        [NUnit.Framework.TestCase(null)]
        [NUnit.Framework.TestCase("")]
        public void CentralSegmentReadback_ErrorSessionRoundTrip(string error)
        {
            object run = NewEligibilityRun();
            run.GetType().GetField("centralSegmentReadbackError").SetValue(run, error);
            object restored = JsonUtility.FromJson(JsonUtility.ToJson(run), run.GetType());
            NUnit.Framework.Assert.That(EligibilityMethod("CentralSegmentReadbackPending")
                .Invoke(null, new[] { restored }), NUnit.Framework.Is.EqualTo(true));
        }

        [NUnit.Framework.Test]
        public void CentralSegmentReadback_RequestOnlyOutput()
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildCentralSegmentReadbackRequest")
                .Invoke(null, new object[] { 0 });
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(1);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008/windows-02/00-combat1000-readback/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(1)]
        public void CentralSegmentReadback_OutsideMatrix(int index)
        {
            MethodInfo method = EligibilityMethod("BuildCentralSegmentReadbackRequest");
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.TestCase("strict", 1)]
        [NUnit.Framework.TestCase("chunk", 2)]
        [NUnit.Framework.TestCase("texture", 4)]
        [NUnit.Framework.TestCase("material", 8)]
        [NUnit.Framework.TestCase("variant", 16)]
        [NUnit.Framework.TestCase("binding", 32)]
        [NUnit.Framework.TestCase("page", 64)]
        [NUnit.Framework.TestCase("slice", 128)]
        [NUnit.Framework.TestCase("commandGap", 256)]
        [NUnit.Framework.TestCase("quadGap", 512)]
        [NUnit.Framework.TestCase("equal", 0)]
        public void CentralSegmentReadback_BreakMask(string change, int expected)
        {
            MethodInfo method = EligibilityMethod("CentralSegmentBreakMask");
            Shader shader = Shader.Find("NTSD/BattleCentralTransparent");
            NUnit.Framework.Assert.That(shader, NUnit.Framework.Is.Not.Null);
            Texture2D firstTexture = new Texture2D(1, 1);
            Texture2D otherTexture = new Texture2D(1, 1);
            Material firstMaterial = new Material(shader);
            Material otherMaterial = new Material(shader);
            try
            {
                var previous = new BattleCentralRenderSegment(0, 0, 0, 1, 0, 1, firstTexture,
                    firstMaterial, 2, 0, BattleSpriteCentralBindingMode.AtlasPageTexture2D, 0);
                var next = new BattleCentralRenderSegment(change == "chunk" ? 1 : 0, 1,
                    change == "commandGap" ? 2 : 1, 1, change == "quadGap" ? 2 : 1, 1,
                    change == "texture" ? otherTexture : firstTexture,
                    change == "material" ? otherMaterial : firstMaterial, change == "variant" ? 3 : 2,
                    change == "slice" ? 1 : 0,
                    change == "binding" ? BattleSpriteCentralBindingMode.SourceTexture2D :
                        BattleSpriteCentralBindingMode.AtlasPageTexture2D, change == "page" ? 1 : 0);
                int actual = (int)method.Invoke(null, new object[] { previous, next,
                    change == "strict" ? BattleCentralDrawMode.StrictOrderedDraw : BattleCentralDrawMode.OrderedChunks });
                NUnit.Framework.Assert.That(actual, NUnit.Framework.Is.EqualTo(expected));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstMaterial);
                UnityEngine.Object.DestroyImmediate(otherMaterial);
                UnityEngine.Object.DestroyImmediate(firstTexture);
                UnityEngine.Object.DestroyImmediate(otherTexture);
            }
        }

        [NUnit.Framework.Test]
        public void CentralSegmentReadback_ArraySliceIsNotArrayIdentity()
        {
            MethodInfo method = EligibilityMethod("CentralSegmentBreakMask");
            var previous = new BattleCentralRenderSegment(0, 0, 0, 1, 0, 1, null,
                null, 0, 2, BattleSpriteCentralBindingMode.AtlasTextureArray);
            var next = new BattleCentralRenderSegment(0, 1, 1, 1, 1, 1, null,
                null, 0, 7, BattleSpriteCentralBindingMode.AtlasTextureArray);
            NUnit.Framework.Assert.That(method.Invoke(null, new object[] { previous, next,
                BattleCentralDrawMode.OrderedChunks }), NUnit.Framework.Is.EqualTo(0));
        }

        [NUnit.Framework.Test]
        public void CentralSegmentReadback_SourcePageIndexIsNotBoundPageIdentity()
        {
            MethodInfo method = EligibilityMethod("CentralSegmentBreakMask");
            var previous = new BattleCentralRenderSegment(0, 0, 0, 1, 0, 1, null,
                null, 0, 0, BattleSpriteCentralBindingMode.SourceTexture2D, 1);
            var next = new BattleCentralRenderSegment(0, 1, 1, 1, 1, 1, null,
                null, 0, 0, BattleSpriteCentralBindingMode.SourceTexture2D, 8);
            NUnit.Framework.Assert.That(method.Invoke(null, new object[] { previous, next,
                BattleCentralDrawMode.OrderedChunks }), NUnit.Framework.Is.EqualTo(0));
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void CentralSegmentReadback_DetachIdempotent(bool ownsRun)
        {
            MethodInfo restore = EligibilityMethod("RestoreCentralSegmentReadback");
            FieldInfo owner = CoarseEnvelopeStaticField("centralSegmentReadbackRun");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object run = ownsRun ? NewEligibilityRun() : null;
            try
            {
                owner.SetValue(null, run);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                if (run != null)
                    NUnit.Framework.Assert.That(EligibilityRunField(run, "centralSegmentReadbackDetached"),
                        NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteOrdinalPacketWindow_RequestOnlyOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildBruteOrdinalPacketRequest")
                .Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH72-ORDINAL-PACKET-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-packet-off" : "-packet-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void BruteOrdinalPacketWindow_OutsideMatrix(int index)
        {
            MethodInfo method = EligibilityMethod("BuildBruteOrdinalPacketRequest");
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_MenuKeepsLegacy17AndColdOwner()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteOrdinalPacket");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(attributes.Length, NUnit.Framework.Is.EqualTo(1));
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch72 Ordinal Packet Full Driver GC 1000 AI OFF ON"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingOrdinalPacketWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.Null);
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_StateDefaultsFalseAndRoutesFourBeforeGc()
        {
            EligibilityMethod("BuildBruteOrdinalPacketRequest");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            Type type = owner.FieldType;
            FieldInfo mode = type.GetField("bruteOrdinalPacketCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", type)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(type, true);
            mode.SetValue(candidate, true);
            type.GetField("bruteProductionOnly").SetValue(candidate, true);
            type.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteOrdinalPacketRequest").Invoke(null, new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteOrdinalPacketWindow_ModeRejectsBeforeState(int defect)
        {
            EligibilityMethod("BeginBruteOrdinalPacket");
            FieldInfo startup = CoarseEnvelopeStaticField("startingOrdinalPacketWindow");
            FieldInfo oldStartup = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object previous = startup.GetValue(null);
            object previousOld = oldStartup.GetValue(null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
                arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                    parameters[index].Name == "bruteBranchTimingOnly" && defect == 2;
            try
            {
                startup.SetValue(null, true);
                oldStartup.SetValue(null, defect == 3);
                var error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() => begin.Invoke(null, arguments));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(error.InnerException.Message, NUnit.Framework.Does.Contain("ordinal packet window"));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                startup.SetValue(null, previous);
                oldStartup.SetValue(null, previousOld);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteOrdinalPacketWindow_ApplyObserveCompleteRestores(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.BruteOrdinalPacketCapacityForDiagnostics, NUnit.Framework.Is.EqualTo(0),
                    "Population has not completed; zero cold capacity is legal before the allocation seal.");
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, enabled, false, enabled ? 11L : 0L, enabled ? 5L : 0L);
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new object[] { run });
                NUnit.Framework.Assert.That(EligibilityMethod("OrdinalPacketObservationValid").Invoke(null, new[] { run }),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketCapacityBeforePopulation"),
                    NUnit.Framework.Is.EqualTo(0));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketSteadyCapacity"),
                    NUnit.Framework.Is.EqualTo(2));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketQuery").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_RejectSecondOwnerKeepsFirst()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object first = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, first, true });
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { query, NewEligibilityRun(), false })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.SameAs(first));
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.True);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        [NUnit.Framework.TestCase(5)]
        public void BruteOrdinalPacketWindow_RejectQueryScope(int defect)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            if (defect == 0) query.EnableBruteOrdinalPacketForDiagnostics = true;
            if (defect == 1) query.EnableBruteGeometryFirstForDiagnostics = false;
            if (defect == 2) query.EnableBruteKind5PresenceForDiagnostics = true;
            if (defect == 3) query.EnableBruteCoarseEnvelopeForDiagnostics = true;
            if (defect == 4) query.EnableBruteBranchTimingForDiagnostics = true;
            if (defect == 5) query.EnableBruteCoarseProofReuseForDiagnostics = true;
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true })).InnerException,
                NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.EqualTo(defect == 0));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteOrdinalPacketWindow_DriftLatchesUntilComplete(int defect)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, true, false, 11L, 5L);
                if (defect == 0) query.EnableBruteOrdinalPacketForDiagnostics = false;
                else query.EnableBruteGeometryFirstForDiagnostics = false;
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                query.EnableBruteOrdinalPacketForDiagnostics = true;
                query.EnableBruteGeometryFirstForDiagnostics = true;
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketFlagUnchanged"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new[] { run })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketRestored"), NUnit.Framework.Is.EqualTo(true));
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_ObservationDedupsFreshTick()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, true, false, 11L, 5L);
                MethodInfo observe = EligibilityMethod("ObserveBruteOrdinalPacket");
                observe.Invoke(null, new object[] { run, 121 });
                SetOrdinalPacketWindowEvidence(query, true, false, 99L, 98L);
                observe.Invoke(null, new object[] { run, 121 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketObservedSamples"), NUnit.Framework.Is.EqualTo(1));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketMaximumObservedRejects"), NUnit.Framework.Is.EqualTo(11L));
                observe.Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketObservedSamples"), NUnit.Framework.Is.EqualTo(2));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketMaximumObservedRejects"), NUnit.Framework.Is.EqualTo(99L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketMaximumObservedProofs"), NUnit.Framework.Is.EqualTo(98L));
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_RejectSteadyCapacityGrowth()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, true, false, 11L, 5L);
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                PrepareOrdinalPacketWindowQuery(query, 64);
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketCapacityStable"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new[] { run })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteOrdinalPacketWindow_InvalidEvidenceMissingOrFallback(bool fallback)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                if (fallback)
                {
                    PrepareOrdinalPacketWindowQuery(query, 32);
                    SetOrdinalPacketWindowEvidence(query, true, true, 11L, 5L);
                    EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                }
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new[] { run })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteOrdinalPacketWindow_RestorePreviousIdempotent(bool previous)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            FieldInfo owner = CoarseEnvelopeStaticField("ordinalPacketQuery");
            FieldInfo runOwner = CoarseEnvelopeStaticField("ordinalPacketRun");
            FieldInfo oldFlag = CoarseEnvelopeStaticField("previousOrdinalPacket");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object saved = oldFlag.GetValue(null);
            object run = NewEligibilityRun();
            try
            {
                owner.SetValue(null, query);
                runOwner.SetValue(null, run);
                oldFlag.SetValue(null, previous);
                query.EnableBruteOrdinalPacketForDiagnostics = !previous;
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(runOwner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketRestored"), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                restore.Invoke(null, null);
                oldFlag.SetValue(null, saved);
            }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_WrongRunCannotReleaseOwner()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                object wrong = NewEligibilityRun();
                foreach (string name in new[] { "CompleteBruteOrdinalPacket", "ObserveBruteOrdinalPacket" })
                {
                    MethodInfo method = EligibilityMethod(name);
                    object[] args = name == "CompleteBruteOrdinalPacket" ? new[] { wrong } : new object[] { wrong, 121 };
                    NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        method.Invoke(null, args)).InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                    NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.SameAs(run));
                    NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.True);
                }
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_ExternalExitRestoresOwnedFlag()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            object run = NewEligibilityRun();
            FieldInfo stateOwner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(stateOwner.GetValue(null), NUnit.Framework.Is.Null);
            object candidate = Activator.CreateInstance(stateOwner.FieldType, true);
            stateOwner.FieldType.GetField("bruteOrdinalPacketCandidate").SetValue(candidate, true);
            stateOwner.FieldType.GetField("phase").SetValue(candidate, "EXITING");
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                stateOwner.SetValue(null, candidate);
                EligibilityMethod("OnPlayMode").Invoke(null, new object[] { PlayModeStateChange.ExitingPlayMode });
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                stateOwner.SetValue(null, null);
                EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null);
            }
        }

        private static BruteForceSceneQuery NewOrdinalPacketWindowQuery()
        {
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
            return query;
        }

        private static void PrepareOrdinalPacketWindowQuery(BruteForceSceneQuery query, int capacity)
        {
            MethodInfo prepare = typeof(BruteForceSceneQuery).GetMethod("PrepareBattleCapacity",
                BindingFlags.Instance | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(prepare, NUnit.Framework.Is.Not.Null);
            prepare.Invoke(query, new object[] { capacity, 1, 1 });
        }

        private static void SetOrdinalPacketWindowEvidence(BruteForceSceneQuery query, bool applied,
            bool fallback, long rejects, long proofs)
        {
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketAppliedForDiagnostics", applied);
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketFallbackForDiagnostics", fallback);
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketRejectedDirectionCountForDiagnostics", rejects);
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketProofCountForDiagnostics", proofs);
        }

        private static void SetOrdinalPacketWindowProperty(BruteForceSceneQuery query, string name, object value)
        {
            PropertyInfo property = typeof(BruteForceSceneQuery).GetProperty(name);
            NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null);
            property.GetSetMethod(true).Invoke(query, new[] { value });
        }


        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void RenderTextureBindingWindow_ApplyCompleteRestoresOwnedFlag(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            MethodInfo complete = EligibilityMethod("CompleteRenderTextureBindingReuse");
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(previous, NUnit.Framework.Is.False, "The candidate remains default OFF.");
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { run, enabled });
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseEnabled"), NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingAcceptedCameraSamples", 1);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyPrepareCount", 4L);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyReuseCount", enabled ? 2L : 0L);
                complete.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagUnchanged"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                restore.Invoke(null, null);
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.Test]
        public void RenderTextureBindingWindow_RejectsSecondOwnerWithoutChangingFirst()
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(previous, NUnit.Framework.Is.False);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object first = NewEligibilityRun();
            object second = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { first, true });
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { second, false }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(first, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(second, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
            }
            finally
            {
                restore.Invoke(null, null);
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.Test]
        public void RenderTextureBindingWindow_RejectsUnownedNondefaultFlag()
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object run = NewEligibilityRun();
            try
            {
                flag.SetValue(null, true);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { run, false }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(false));
            }
            finally
            {
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void RenderTextureBindingWindow_CompleteRejectsDriftAndExitRestoreIsIdempotent(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            MethodInfo complete = EligibilityMethod("CompleteRenderTextureBindingReuse");
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(previous, NUnit.Framework.Is.False);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { run, enabled });
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingAcceptedCameraSamples", 1);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyPrepareCount", 4L);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyReuseCount", 0L);
                flag.SetValue(null, !enabled);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagUnchanged"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
            }
            finally
            {
                restore.Invoke(null, null);
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void RenderTextureBindingWindow_RestoreWithoutOwnerPreservesExternalFlag(bool externalFlag)
        {
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            try
            {
                flag.SetValue(null, externalFlag);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(externalFlag));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(externalFlag));
            }
            finally
            {
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 2, 8, true)]
        [NUnit.Framework.TestCase(10L, 10L, 73, 73, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 9L, 73, 73, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 74, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 0, 0, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 0, 0, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 0, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 2, -1, false)]
        public void RenderTextureBindingWindow_ObservationRequiresNewValidBodyExecute(
            long previousSequence, long currentSequence, int expectedCameraId, int actualCameraId,
            int prepareCount, int reuseCount, bool expected)
        {
            MethodInfo valid = EligibilityMethod("IsRenderTextureBindingObservationFresh");
            Type feature = typeof(NTSD.Animation.Rendering.BattleRenderFeature);
            foreach (string propertyName in new[]
            {
                "LastSegmentTextureBindingExecuteSequenceForDiagnostics",
                "LastSegmentTextureBindingCameraIdForDiagnostics",
            })
            {
                PropertyInfo property = feature.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null, propertyName);
                NUnit.Framework.Assert.That(property.PropertyType, NUnit.Framework.Is.EqualTo(
                    propertyName.Contains("Sequence") ? typeof(long) : typeof(int)));
                NUnit.Framework.Assert.That(property.GetGetMethod(), NUnit.Framework.Is.Not.Null);
                NUnit.Framework.Assert.That(property.GetSetMethod(), NUnit.Framework.Is.Null, "Observation has no public setter.");
            }
            NUnit.Framework.Assert.That(valid.GetParameters().Length, NUnit.Framework.Is.EqualTo(6),
                "Fresh Execute does not require a new logic tick, publication or GPU completion.");
            NUnit.Framework.Assert.That(valid.Invoke(null, new object[]
            {
                previousSequence, currentSequence, expectedCameraId, actualCameraId, prepareCount, reuseCount,
            }), NUnit.Framework.Is.EqualTo(expected));
            if (expected)
            {
                NUnit.Framework.Assert.That(valid.Invoke(null, new object[]
                {
                    previousSequence, currentSequence, -73, -73, prepareCount, reuseCount,
                }), NUnit.Framework.Is.EqualTo(true), "Unity instance identity may be negative; only zero is invalid.");
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void RenderTextureBindingWindow_RequestKeepsProductionWorkloadAndFreshOutput(int index)
        {
            MethodInfo method = EligibilityMethod("BuildRenderTextureBindingRequest");
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json"));
            NUnit.Framework.Assert.That(actual.inputMode, NUnit.Framework.Is.EqualTo("ai"));
            NUnit.Framework.Assert.That(actual.entityCount, NUnit.Framework.Is.EqualTo(1000));
            NUnit.Framework.Assert.That(actual.warmupTicks, NUnit.Framework.Is.EqualTo(120));
            NUnit.Framework.Assert.That(actual.sampleTicks, NUnit.Framework.Is.EqualTo(180));
            NUnit.Framework.Assert.That(actual.maxCatchUpTicksPerFrame, NUnit.Framework.Is.EqualTo(2));
            NUnit.Framework.Assert.That(actual.maxBacklogTicks, NUnit.Framework.Is.EqualTo(2));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
            if (index == 0)
            {
                foreach (int rejectedIndex in new[] { -1, 4 })
                    NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        method.Invoke(null, new object[] { rejectedIndex })).InnerException,
                        NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static PropertyInfo RenderTextureBindingWindowFlag()
        {
            PropertyInfo property = typeof(NTSD.Animation.Rendering.BattleRenderFeature).GetProperty(
                "EnableSegmentTextureBindingReuseForDiagnostics", BindingFlags.Public | BindingFlags.Static);
            NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null);
            return property;
        }

        private static FieldInfo RenderTextureBindingWindowOwner()
        {
            FieldInfo field = typeof(BattleOptimizationWindowsAiSuiteEditor).GetField(
                "renderTextureBindingReuseOwner", BindingFlags.NonPublic | BindingFlags.Static);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(field.FieldType, NUnit.Framework.Is.EqualTo(typeof(bool)));
            return field;
        }

        private static void SetRenderTextureBindingWindowRunField(object run, string name, object value)
        {
            FieldInfo field = run.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null, name);
            field.SetValue(run, value);
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEnvelopeBranchTimingWindow_RequestKeepsProductionWorkload(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteEnvelopeBranchTimingRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH65-ENVELOPE-PATH-BRANCH-TIMING-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-timing-off" : "-timing-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_RequestRejectsOutsideMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEnvelopeBranchTimingRequest");
            foreach (int index in new[] { -1, 4 })
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index })).InnerException,
                    NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_ModeIsExplicitAndRoutesBeforeScope()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteEnvelopeBranchTiming");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch65 Envelope Branch Timing Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            FieldInfo mode = owner.FieldType.GetField("bruteEnvelopeBranchTimingCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", owner.FieldType)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(owner.FieldType, true);
            mode.SetValue(candidate, true);
            owner.FieldType.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(EligibilityMethod("BuildCurrentRequest").Invoke(null,
                        new object[] { index })), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(
                            EligibilityMethod("BuildBruteEnvelopeBranchTimingRequest").Invoke(null, new object[] { index }))));
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_ModeRejectsMissingScopeAndOtherModes()
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingEnvelopeBranchTimingWindow");
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            try
            {
                for (int defect = 0; defect < 4; defect++)
                {
                    object[] arguments = new object[parameters.Length];
                    for (int index = 0; index < arguments.Length; index++)
                        arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                            parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                            parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                            parameters[index].Name == "bruteBranchTimingOnly" && defect == 3;
                    startup.SetValue(null, true);
                    TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        begin.Invoke(null, arguments));
                    NUnit.Framework.Assert.That(error.InnerException.Message,
                        NUnit.Framework.Does.Contain("envelope branch timing window requires"));
                    NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("state").GetValue(null), NUnit.Framework.Is.Null);
                }
            }
            finally { startup.SetValue(null, false); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_ColdOwnerAndOldScopesRejectNewOptIn()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBranchTiming");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            foreach (string flag in new[] { "EnableBruteEnvelopeBranchTimingForDiagnostics",
                "EnableBruteBranchTimingForDiagnostics", "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics", "EnableBruteEligibilityReuseForDiagnostics",
                "EnableBruteKind5PresenceForDiagnostics", "EnableBruteCoarseDispatchForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                typeof(BruteForceSceneQuery).GetProperty(flag).SetValue(query, true);
                try
                {
                    NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
                    NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null),
                        NUnit.Framework.Is.Null);
                }
                finally { restore.Invoke(null, null); }
            }
            var excluded = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            excluded.EnableBruteEnvelopeBranchTimingForDiagnostics = true;
            foreach (string valid in new[] { "EnvelopeBindingQueryDefaultsValid", "CoarseEnvelopeQueryDefaultsValid" })
                NUnit.Framework.Assert.That(EligibilityMethod(valid).Invoke(null, new object[] { excluded }),
                    NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBranchTimingWindow_ApplyCompleteRestoresFlagsAndStride(bool enabled)
        {
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.BruteBranchTimingSampleStrideForDiagnostics = 8;
            object run = NewEligibilityRun();
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics &&
                    query.EnableBruteRejectedBindingReuseForDiagnostics && query.EnableBruteEnvelopeBranchTimingForDiagnostics,
                    NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(query.BruteBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(64));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBranchTimingScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBindingScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(false));
                EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run });
                foreach (string field in new[] { "bruteCoarseEnvelopeRestored", "bruteRejectedBindingReuseRestored",
                    "bruteEnvelopeBranchTimingRestored", "bruteBranchTimingRestored", "bruteBranchTimingFlagUnchanged" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEnvelopeBranchTimingForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.BruteBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(8));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_DriftLatchesAndStillRestores()
        {
            foreach (string flag in new[] { "EnableBruteEnvelopeBranchTimingForDiagnostics",
                "EnableBruteBranchTimingForDiagnostics", "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                object run = NewEligibilityRun();
                MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
                try
                {
                    EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, true });
                    typeof(BruteForceSceneQuery).GetProperty(flag).SetValue(query, false);
                    EligibilityMethod("ObserveBruteEnvelopeBinding").Invoke(null, new[] { run });
                    typeof(BruteForceSceneQuery).GetProperty(flag).SetValue(query, true);
                    NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
                }
                finally
                {
                    NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                    NUnit.Framework.Assert.That(query.EnableBruteEnvelopeBranchTimingForDiagnostics ||
                        query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                }
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBranchTimingWindow_AbortRestoresCurrentOrCompletedRun(bool currentRun)
        {
            FieldInfo stateOwner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(stateOwner.GetValue(null), NUnit.Framework.Is.Null);
            object run = NewEligibilityRun();
            object candidate = Activator.CreateInstance(stateOwner.FieldType, true);
            Array runs = Array.CreateInstance(run.GetType(), 1);
            runs.SetValue(run, 0);
            stateOwner.FieldType.GetField("runs").SetValue(candidate, runs);
            stateOwner.FieldType.GetField("runIndex").SetValue(candidate, currentRun ? 0 : 1);
            FieldInfo mode = stateOwner.FieldType.GetField("bruteEnvelopeBranchTimingCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            mode.SetValue(candidate, true);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, true });
                stateOwner.SetValue(null, candidate);
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEnvelopeBindingForExit").Invoke(null, null),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEnvelopeBranchTimingForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEnvelopeBranchTimingRestored"),
                    NUnit.Framework.Is.EqualTo(currentRun));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); stateOwner.SetValue(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_StrideDriftIsNotHiddenByRestoringFlag()
        {
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, true });
                query.BruteBranchTimingSampleStrideForDiagnostics = 1;
                EligibilityMethod("ObserveBruteEnvelopeBinding").Invoke(null, new[] { run });
                query.BruteBranchTimingSampleStrideForDiagnostics = 64;
                NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
            }
            finally
            {
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.BruteBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(1));
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBranchTimingWindow_ObservationRequiresCoverageAndRestoration(bool enabled)
        {
            MethodInfo valid = EligibilityMethod("EnvelopeBranchTimingObservationValid");
            object run = NewEligibilityRun();
            foreach (string field in new[] { "bruteEnvelopeBranchTimingMode", "bruteCoarseEnvelopeEnabled",
                "bruteCoarseEnvelopeFlagApplied", "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                "bruteRejectedBindingReuseEnabled", "bruteRejectedBindingReuseFlagApplied",
                "bruteRejectedBindingReuseFlagUnchanged", "bruteRejectedBindingReuseRestored",
                "bruteRejectedBindingReuseObservedApplied", "bruteEnvelopeBranchTimingFlagApplied",
                "bruteEnvelopeBranchTimingFlagUnchanged", "bruteEnvelopeBranchTimingRestored",
                "bruteBranchTimingFlagApplied", "bruteBranchTimingFlagUnchanged", "bruteBranchTimingRestored" })
                SetCoarseEnvelopeRunField(run, field, true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", 8L);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", 4L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedProbes", 3L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedReuses", 2L);
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingSampleStride", 64);
            var coverage = new BruteForceSceneQuery.BruteBranchTimingCoverage();
            if (enabled)
            {
                coverage.eligibleDirections = 128;
                coverage.timedDirections = 2;
                coverage.rejectedBindingVisits = coverage.pairAllowedVisits = coverage.exactWorkVisits = 64;
                coverage.rejectedBindingTimed = coverage.pairAllowedTimed = coverage.exactWorkTimed = 1;
            }
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingCoverage", coverage);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(true));
            var invalid = coverage;
            invalid.rejectedBindingTimed = enabled ? 0 : 1;
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingCoverage", invalid);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingCoverage", coverage);
            SetCoarseEnvelopeRunField(run, "bruteEnvelopeBranchTimingRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEnvelopeBindingEligibility_RequestKeepsProductionWorkload(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteEnvelopeBindingEligibilityRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH64-ENVELOPE-BINDING-ELIGIBILITY-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-eligibility-off" : "-eligibility-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_RequestRejectsOutsideMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEnvelopeBindingEligibilityRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_ModeIsExplicitAndRoutesBeforeScope()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteEnvelopeBindingEligibility");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch64 Envelope Binding Eligibility Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingEnvelopeBindingEligibilityWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            FieldInfo mode = owner.FieldType.GetField("bruteEnvelopeBindingEligibilityCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", owner.FieldType)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(owner.FieldType, true);
            mode.SetValue(candidate, true);
            owner.FieldType.GetField("bruteProductionOnly").SetValue(candidate, true);
            owner.FieldType.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteEnvelopeBindingEligibilityRequest").Invoke(null,
                        new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_ModeRejectsMissingScopeAndOtherModes()
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingEnvelopeBindingEligibilityWindow");
            FieldInfo envelope = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo binding = CoarseEnvelopeStaticField("startingEnvelopeBindingWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            try
            {
                for (int defect = 0; defect < 6; defect++)
                {
                    object[] arguments = new object[parameters.Length];
                    for (int index = 0; index < arguments.Length; index++)
                        arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                            parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                            parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                            parameters[index].Name == "bruteEligibilityReuseCandidate" && defect == 3;
                    startup.SetValue(null, true);
                    envelope.SetValue(null, defect == 4);
                    binding.SetValue(null, defect == 5);
                    TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        begin.Invoke(null, arguments));
                    NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                    NUnit.Framework.Assert.That(error.InnerException.Message,
                        NUnit.Framework.Does.Contain("envelope binding eligibility window requires"));
                    NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                }
            }
            finally
            {
                startup.SetValue(null, false);
                envelope.SetValue(null, false);
                binding.SetValue(null, false);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBindingEligibility_ApplyScopeCompleteRestoresThree(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics &&
                    query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBindingEligibilityScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBindingScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(false));
                EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run });
                foreach (string field in new[] { "bruteCoarseEnvelopeRestored", "bruteRejectedBindingReuseRestored",
                    "bruteEligibilityReuseRestored", "bruteEligibilityReuseFlagUnchanged" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEligibilityReuseForDiagnostics,
                    NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingEligibilityOwner").GetValue(null),
                    NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_DriftLatchesAndRestorationStillWorks()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            MethodInfo observe = EligibilityMethod("ObserveBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            foreach (string property in new[] { "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics", "EnableBruteEligibilityReuseForDiagnostics",
                "EnableBruteExactCacheForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                object run = NewEligibilityRun();
                try
                {
                    apply.Invoke(null, new object[] { query, run, true });
                    query.GetType().GetProperty(property).SetValue(query, false, null);
                    observe.Invoke(null, new[] { run });
                    query.GetType().GetProperty(property).SetValue(query, true, null);
                    TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
                    NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                    NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                    NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                        query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEligibilityReuseForDiagnostics,
                        NUnit.Framework.Is.False);
                }
                finally { restore.Invoke(null, null); }
            }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_ColdApplyRejectsUnownedCandidateWithoutMutation()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            foreach (string property in new[] { "EnableBruteKind5PresenceForDiagnostics",
                "EnableBruteEligibilityReuseForDiagnostics", "EnableBruteCoarseDispatchForDiagnostics",
                "EnableBruteBranchTimingForDiagnostics", "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                query.GetType().GetProperty(property).SetValue(query, true, null);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.GetType().GetProperty(property).GetValue(query, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null), NUnit.Framework.Is.Null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBindingEligibility_ExitRestoresThreeAndOnlyCurrentRun(bool currentRun)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            object candidate = Activator.CreateInstance(owner.FieldType, true);
            Array runs = Array.CreateInstance(run.GetType(), 1);
            runs.SetValue(run, 0);
            owner.FieldType.GetField("runs").SetValue(candidate, runs);
            owner.FieldType.GetField("runIndex").SetValue(candidate, currentRun ? 0 : 1);
            owner.FieldType.GetField("bruteEnvelopeBindingEligibilityCandidate").SetValue(candidate, true);
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                owner.SetValue(null, candidate);
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEnvelopeBindingForExit").Invoke(null, null),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEligibilityReuseForDiagnostics,
                    NUnit.Framework.Is.False);
                foreach (string field in new[] { "bruteCoarseEnvelopeRestored", "bruteRejectedBindingReuseRestored",
                    "bruteEligibilityReuseRestored" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(currentRun), field);
            }
            finally
            {
                owner.SetValue(null, null);
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBindingEligibility_ObservationRequiresActualDeltaAndThreeRestored(bool enabled)
        {
            MethodInfo valid = EligibilityMethod("EnvelopeBindingEligibilityObservationValid");
            object run = NewEligibilityRun();
            foreach (string field in new[] { "bruteEnvelopeBindingEligibilityMode", "bruteCoarseEnvelopeEnabled",
                "bruteCoarseEnvelopeFlagApplied", "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                "bruteRejectedBindingReuseEnabled", "bruteRejectedBindingReuseFlagApplied",
                "bruteRejectedBindingReuseFlagUnchanged", "bruteRejectedBindingReuseRestored",
                "bruteRejectedBindingReuseObservedApplied", "bruteEligibilityReuseFlagApplied",
                "bruteEligibilityReuseFlagUnchanged", "bruteEligibilityReuseRestored" })
                SetCoarseEnvelopeRunField(run, field, true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", 8L);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", 4L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedProbes", 3L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedReuses", 2L);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseObservedApplied", enabled);
            SetCoarseEnvelopeRunField(run, "warmupTicks", 120);
            SetCoarseEnvelopeRunField(run, "sampledTicks", 180);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseAppliedDelta", enabled ? 300L : 0L);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(true));
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseAppliedDelta", enabled ? 299L : 1L);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseAppliedDelta", enabled ? 300L : 0L);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }


        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBinding_ExitRestoresAndRecordsOnlyCurrentRun(bool currentRun)
        {
            FieldInfo stateOwner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(stateOwner.GetValue(null), NUnit.Framework.Is.Null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            object candidate = Activator.CreateInstance(stateOwner.FieldType, true);
            Array runs = Array.CreateInstance(run.GetType(), 1);
            runs.SetValue(run, 0);
            stateOwner.FieldType.GetField("runs").SetValue(candidate, runs);
            stateOwner.FieldType.GetField("runIndex").SetValue(candidate, currentRun ? 0 : 1);
            stateOwner.FieldType.GetField("bruteEnvelopeBindingCandidate").SetValue(candidate, true);
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBinding").Invoke(null, new object[] { query, run, true });
                stateOwner.SetValue(null, candidate);
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEnvelopeBindingForExit").Invoke(null, null),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null),
                    NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeRestored"),
                    NUnit.Framework.Is.EqualTo(currentRun));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseRestored"),
                    NUnit.Framework.Is.EqualTo(currentRun));
            }
            finally
            {
                stateOwner.SetValue(null, null);
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEnvelopeBindingRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteEnvelopeBindingRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH63-ENVELOPE-BINDING-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingRequest_RejectsOutsideFixedMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEnvelopeBindingRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBinding_MenuPreservesLegacySignatureAndColdOwnership()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteEnvelopeBinding");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(attributes.Length, NUnit.Framework.Is.EqualTo(1));
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch63 Envelope Binding Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingEnvelopeBindingWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBinding_ModeDefaultsFalseAndRoutesBeforeScope()
        {
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            Type type = owner.FieldType;
            FieldInfo mode = type.GetField("bruteEnvelopeBindingCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", type)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(type, true);
            mode.SetValue(candidate, true);
            type.GetField("bruteProductionOnly").SetValue(candidate, true);
            type.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteEnvelopeBindingRequest").Invoke(null, new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        public void BruteEnvelopeBinding_ModeRejectsMissingScopeOrOtherOwnerBeforeState(int defect)
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingEnvelopeBindingWindow");
            FieldInfo oldStartup = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object previous = startup.GetValue(null);
            object previousOld = oldStartup.GetValue(null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
                arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                    parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                    parameters[index].Name == "bruteEligibilityReuseCandidate" && defect == 3;
            try
            {
                startup.SetValue(null, true);
                oldStartup.SetValue(null, defect == 4);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    begin.Invoke(null, arguments));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(error.InnerException.Message,
                    NUnit.Framework.Does.Contain("envelope binding window requires"));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                startup.SetValue(null, previous);
                oldStartup.SetValue(null, previousOld);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBinding_ApplyCompleteRestoresBothAndPreservesDefaults(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                foreach (string field in new[] { "bruteCoarseEnvelopeEnabled", "bruteCoarseEnvelopeFlagApplied",
                    "bruteRejectedBindingReuseFlagApplied" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseEnabled"),
                    NUnit.Framework.Is.EqualTo(enabled));
                EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run });
                foreach (string field in new[] { "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                    "bruteRejectedBindingReuseFlagUnchanged", "bruteRejectedBindingReuseRestored" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteKind5PresenceForDiagnostics ||
                    query.EnableBruteEligibilityReuseForDiagnostics || query.EnableBruteCoarseDispatchForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false, false)]
        [NUnit.Framework.TestCase(false, true)]
        [NUnit.Framework.TestCase(true, false)]
        [NUnit.Framework.TestCase(true, true)]
        public void BruteEnvelopeBinding_RestoreBothPriorValuesReleasesOwnerAndIsIdempotent(bool envelope, bool binding)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            FieldInfo owner = CoarseEnvelopeStaticField("envelopeBindingQuery");
            FieldInfo oldEnvelope = CoarseEnvelopeStaticField("previousEnvelopeBindingEnvelope");
            FieldInfo oldBinding = CoarseEnvelopeStaticField("previousEnvelopeBindingReuse");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object savedEnvelope = oldEnvelope.GetValue(null);
            object savedBinding = oldBinding.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteCoarseEnvelopeForDiagnostics = !envelope;
            query.EnableBruteRejectedBindingReuseForDiagnostics = !binding;
            try
            {
                owner.SetValue(null, query);
                oldEnvelope.SetValue(null, envelope);
                oldBinding.SetValue(null, binding);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.EqualTo(envelope));
                NUnit.Framework.Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.EqualTo(binding));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                owner.SetValue(null, null);
                oldEnvelope.SetValue(null, savedEnvelope);
                oldBinding.SetValue(null, savedBinding);
            }
        }

        [NUnit.Framework.TestCase("EnableBruteCoarseEnvelopeForDiagnostics", false)]
        [NUnit.Framework.TestCase("EnableBruteRejectedBindingReuseForDiagnostics", false)]
        [NUnit.Framework.TestCase("EnableBruteExactCacheForDiagnostics", false)]
        [NUnit.Framework.TestCase("EnableBruteKind5PresenceForDiagnostics", true)]
        public void BruteEnvelopeBinding_CompleteRejectsDriftAndRestoreStillWorks(string property, bool value)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                query.GetType().GetProperty(property).SetValue(query, value, null);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase("EnableBruteKind5PresenceForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteEligibilityReuseForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseDispatchForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteBranchTimingForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseEnvelopeForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteRejectedBindingReuseForDiagnostics")]
        public void BruteEnvelopeBinding_RejectsOtherCandidateOrUnownedEnable(string property)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.GetType().GetProperty(property).SetValue(query, true, null);
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(query.GetType().GetProperty(property).GetValue(query, null), NUnit.Framework.Is.EqualTo(true));
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBinding_RejectsSecondOrOldOwnerWithoutChangingFlags(bool oldOwner)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var first = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            var second = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            try
            {
                EligibilityMethod(oldOwner ? "ApplyBruteCoarseEnvelope" : "ApplyBruteEnvelopeBinding")
                    .Invoke(null, new object[] { first, NewEligibilityRun(), true });
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { second, NewEligibilityRun(), true }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(first.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(first.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.EqualTo(!oldOwner));
                NUnit.Framework.Assert.That(second.EnableBruteCoarseEnvelopeForDiagnostics ||
                    second.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
                EligibilityMethod("RestoreBruteCoarseEnvelope").Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false, false, 0L, 0L, true)]
        [NUnit.Framework.TestCase(true, false, 0L, 0L, false)]
        [NUnit.Framework.TestCase(true, true, 12L, 3L, true)]
        [NUnit.Framework.TestCase(true, true, 12L, 0L, false)]
        [NUnit.Framework.TestCase(false, false, 12L, 0L, false)]
        [NUnit.Framework.TestCase(true, false, 12L, 3L, false)]
        public void BruteEnvelopeBinding_ObservationRequiresRealActivityAndBothRestored(
            bool enabled, bool observedApplied, long probes, long reuses, bool expected)
        {
            MethodInfo valid = EligibilityMethod("EnvelopeBindingObservationValid");
            object run = NewEligibilityRun();
            foreach (string field in new[] { "bruteCoarseEnvelopeEnabled", "bruteCoarseEnvelopeFlagApplied",
                "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                "bruteRejectedBindingReuseFlagApplied", "bruteRejectedBindingReuseFlagUnchanged",
                "bruteRejectedBindingReuseRestored" })
                SetCoarseEnvelopeRunField(run, field, true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", 8L);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", 4L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingReuseEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingReuseObservedApplied", observedApplied);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedProbes", probes);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedReuses", reuses);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(expected));
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingReuseRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBinding_ObservationRecordsMaximumAndLatchesDrift()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo observe = EligibilityMethod("ObserveBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                query.GetType().GetProperty("LastBruteRejectedBindingReuseAppliedForDiagnostics").GetSetMethod(true)
                    .Invoke(query, new object[] { true });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingProbeCountForDiagnostics", 11L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingReuseCountForDiagnostics", 5L);
                observe.Invoke(null, new[] { run });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingProbeCountForDiagnostics", 3L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingReuseCountForDiagnostics", 1L);
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingMaximumObservedProbes"), NUnit.Framework.Is.EqualTo(11L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingMaximumObservedReuses"), NUnit.Framework.Is.EqualTo(5L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseObservedApplied"), NUnit.Framework.Is.EqualTo(true));
                query.EnableBruteRejectedBindingReuseForDiagnostics = false;
                observe.Invoke(null, new[] { run });
                query.EnableBruteRejectedBindingReuseForDiagnostics = true;
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseFlagUnchanged"), NUnit.Framework.Is.EqualTo(false));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteCoarseEnvelopeRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteCoarseEnvelopeRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH60-COARSE-ENVELOPE-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-envelope-off" : "-envelope-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelopeRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteCoarseEnvelopeRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_MenuPreservesLegacySignatureAndColdOwnership()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteCoarseEnvelope");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(attributes.Length, NUnit.Framework.Is.EqualTo(1));
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch60 Coarse Envelope Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_ModeDefaultsFalseAndRoutesFourRequestsBeforeScope()
        {
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            Type stateType = owner.FieldType;
            FieldInfo mode = stateType.GetField("bruteCoarseEnvelopeCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            object legacy = JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", stateType);
            NUnit.Framework.Assert.That(mode.GetValue(legacy), NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(stateType, true);
            mode.SetValue(candidate, true);
            stateType.GetField("bruteProductionOnly").SetValue(candidate, true);
            stateType.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteCoarseEnvelopeRequest").Invoke(null, new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally
            {
                owner.SetValue(null, null);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteCoarseEnvelope_ModeRejectsMissingScopeOrOtherCandidateBeforeState(int defect)
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object previous = startup.GetValue(null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
                arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                    parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                    parameters[index].Name == "bruteEligibilityReuseCandidate" && defect == 3;
            try
            {
                startup.SetValue(null, true);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    begin.Invoke(null, arguments));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(error.InnerException.Message,
                    NUnit.Framework.Does.Contain("coarse envelope window requires"));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                startup.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCoarseEnvelope_ApplyCompleteRestoresAndPreservesProductionFlags(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo complete = EligibilityMethod("CompleteBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeEnabled"), NUnit.Framework.Is.EqualTo(enabled));
                complete.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeFlagUnchanged"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                    query.EnableBruteKind5PresenceForDiagnostics || query.EnableBruteRejectedBindingReuseForDiagnostics ||
                    query.EnableBruteCoarseDispatchForDiagnostics || query.EnableBruteBranchTimingForDiagnostics,
                    NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCoarseEnvelope_RestorePriorValueReleasesOwnerAndIsIdempotent(bool previous)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            FieldInfo owner = CoarseEnvelopeStaticField("coarseEnvelopeQuery");
            FieldInfo prior = CoarseEnvelopeStaticField("previousCoarseEnvelope");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object oldPrior = prior.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteCoarseEnvelopeForDiagnostics = !previous;
            try
            {
                owner.SetValue(null, query);
                prior.SetValue(null, previous);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                owner.SetValue(null, null);
                prior.SetValue(null, oldPrior);
            }
        }

        [NUnit.Framework.TestCase("EnableBruteKind5PresenceForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteEligibilityReuseForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteRejectedBindingReuseForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseDispatchForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteBranchTimingForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseEnvelopeForDiagnostics")]
        public void BruteCoarseEnvelope_RejectsOtherCandidateOrUnownedEnable(string property)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.GetType().GetProperty(property).SetValue(query, true, null);
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("coarseEnvelopeQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics,
                NUnit.Framework.Is.EqualTo(property == "EnableBruteCoarseEnvelopeForDiagnostics"));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_RejectsSecondOwnerWithoutChangingEitherFlag()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var first = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            var second = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            try
            {
                apply.Invoke(null, new object[] { first, NewEligibilityRun(), true });
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { second, NewEligibilityRun(), true }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(first.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(second.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCoarseEnvelope_CompleteRejectsDriftAndRestoreStillWorks(bool productionDrift)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo complete = EligibilityMethod("CompleteBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                if (productionDrift)
                    query.EnableBruteExactCacheForDiagnostics = false;
                else
                    query.EnableBruteCoarseEnvelopeForDiagnostics = false;
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false, 0L, 0L, true)]
        [NUnit.Framework.TestCase(true, 0L, 0L, false)]
        [NUnit.Framework.TestCase(true, 12L, 4L, true)]
        [NUnit.Framework.TestCase(true, 12L, 0L, false)]
        public void BruteCoarseEnvelope_ObservationRequiresRealActivityAndRestoration(
            bool enabled, long directions, long rejects, bool expected)
        {
            MethodInfo valid = EligibilityMethod("CoarseEnvelopeObservationValid");
            object run = NewEligibilityRun();
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeFlagApplied", true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeFlagUnchanged", true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeRestored", true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", directions);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", rejects);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(expected));
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_ObservationRecordsMaximumNotFabricatedTickTotals()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo observe = EligibilityMethod("ObserveBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeDirectionCountForDiagnostics", 11L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeRejectCountForDiagnostics", 5L);
                observe.Invoke(null, new[] { run });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeDirectionCountForDiagnostics", 3L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeRejectCountForDiagnostics", 1L);
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections"),
                    NUnit.Framework.Is.EqualTo(11L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects"),
                    NUnit.Framework.Is.EqualTo(5L));
                query.EnableBruteCoarseEnvelopeForDiagnostics = false;
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeFlagUnchanged"),
                    NUnit.Framework.Is.EqualTo(false));
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        private static FieldInfo CoarseEnvelopeStaticField(string name)
        {
            FieldInfo field = typeof(BattleOptimizationWindowsAiSuiteEditor).GetField(
                name, BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null, name);
            return field;
        }

        private static void SetCoarseEnvelopeRunField(object run, string name, object value)
        {
            FieldInfo field = run.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null, name);
            field.SetValue(run, value);
        }

        private static void SetCoarseEnvelopeDiagnosticCount(BruteForceSceneQuery query, string name, long value)
        {
            MethodInfo setter = query.GetType().GetProperty(name).GetSetMethod(true);
            NUnit.Framework.Assert.That(setter, NUnit.Framework.Is.Not.Null, name);
            setter.Invoke(query, new object[] { value });
        }
        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteCombinedCacheRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = EligibilityMethod("BuildBruteCombinedCacheRequest");
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH52-COMBINED-CACHE-WINDOWS"));
            NUnit.Framework.Assert.That(actual.outputPath,
                NUnit.Framework.Does.Contain(index % 2 == 0 ? "-kind5-off" : "-kind5-on"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteCombinedCacheRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteCombinedCacheRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCombinedCache_ApplyCompleteRestoresBothAndPreservesProductionFlags(bool kind5Enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCombinedCache");
            MethodInfo complete = EligibilityMethod("CompleteBruteCombinedCache");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, kind5Enabled });
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.EqualTo(kind5Enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceEnabled"), NUnit.Framework.Is.EqualTo(kind5Enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                complete.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceAppliedDelta"), NUnit.Framework.Is.EqualTo(0L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseAppliedDelta"), NUnit.Framework.Is.EqualTo(0L));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                    query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                EligibilityMethod("RestoreBruteKind5Presence").Invoke(null, null);
                EligibilityMethod("RestoreBruteEligibilityReuse").Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCombinedCache_CompleteRejectsEitherFlagDriftAndRestoreStillWorks(bool kind5Drift)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCombinedCache");
            MethodInfo complete = EligibilityMethod("CompleteBruteCombinedCache");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                if (kind5Drift)
                    query.EnableBruteKind5PresenceForDiagnostics = false;
                else
                    query.EnableBruteEligibilityReuseForDiagnostics = false;
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteKind5Presence").Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEligibilityReuse").Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                    query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                EligibilityMethod("RestoreBruteKind5Presence").Invoke(null, null);
                EligibilityMethod("RestoreBruteEligibilityReuse").Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCombinedCache_RejectsBindingReuseOrBranchTiming(bool timing)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCombinedCache");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteBranchTimingForDiagnostics = timing;
            query.EnableBruteRejectedBindingReuseForDiagnostics = !timing;
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.False);
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEligibilityReuseRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = EligibilityMethod("BuildBruteEligibilityReuseRequest");
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH50-ELIGIBILITY-WINDOWS"));
            NUnit.Framework.Assert.That(actual.outputPath,
                NUnit.Framework.Does.Contain(index % 2 == 0 ? "-eligibility-off" : "-eligibility-on"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEligibilityReuseRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEligibilityReuseRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEligibilityReuse_ApplyAndRestorePreservesProductionFlags(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEligibilityReuse");
            MethodInfo restore = EligibilityMethod("RestoreBruteEligibilityReuse");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseEnabled"), NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteKind5PresenceForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEligibilityReuse_RestoreRestoresPriorValueAndReleasesReference(bool previous)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteEligibilityReuse");
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            FieldInfo queryField = owner.GetField("eligibilityReuseQuery", flags);
            FieldInfo previousField = owner.GetField("previousEligibilityReuse", flags);
            NUnit.Framework.Assert.That(queryField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(previousField, NUnit.Framework.Is.Not.Null);
            object oldQuery = queryField.GetValue(null);
            object oldPrevious = previousField.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteEligibilityReuseForDiagnostics = !previous;
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, previous);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                queryField.SetValue(null, oldQuery);
                previousField.SetValue(null, oldPrevious);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEligibilityReuse_RejectsOtherDefaultOffCandidate(bool kind5)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEligibilityReuse");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteKind5PresenceForDiagnostics = kind5;
            query.EnableBruteRejectedBindingReuseForDiagnostics = !kind5;
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.False);
        }

        [NUnit.Framework.Test]
        public void BruteEligibilityReuse_CompleteRejectsFlagDriftAndRestoreStillWorks()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEligibilityReuse");
            MethodInfo complete = EligibilityMethod("CompleteBruteEligibilityReuse");
            MethodInfo restore = EligibilityMethod("RestoreBruteEligibilityReuse");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                query.EnableBruteEligibilityReuseForDiagnostics = false;
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        private static MethodInfo EligibilityMethod(string name)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                name, BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, name + " required for the candidate family.");
            return method;
        }

        private static object NewEligibilityRun()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("RunState", BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(type, NUnit.Framework.Is.Not.Null);
            return Activator.CreateInstance(type, true);
        }

        private static object EligibilityRunField(object run, string name)
        {
            FieldInfo field = run.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            return field.GetValue(run);
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteKind5PresenceRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteKind5PresenceRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH38-BRUTE-KIND5-PRESENCE"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteKind5PresenceRequest_RejectsOutsideFixedTwoMatrix()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteKind5PresenceRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteKind5Presence_RestoreRestoresOriginalFlag(bool previous)
        {
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            FieldInfo queryField = owner.GetField("kind5PresenceQuery", flags);
            FieldInfo previousField = owner.GetField("previousKind5Presence", flags);
            MethodInfo restore = owner.GetMethod("RestoreBruteKind5Presence", flags);
            NUnit.Framework.Assert.That(queryField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(previousField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(restore, NUnit.Framework.Is.Not.Null);
            object oldQuery = queryField.GetValue(null);
            object oldFlag = previousField.GetValue(null);
            var query = new BruteForceSceneQuery(new SimulationWorld());
            query.EnableBruteKind5PresenceForDiagnostics = !previous;
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, previous);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                queryField.SetValue(null, oldQuery);
                previousField.SetValue(null, oldFlag);
            }
        }
        [NUnit.Framework.TestCase(false, 1)]
        [NUnit.Framework.TestCase(false, 128)]
        [NUnit.Framework.TestCase(true, 1)]
        [NUnit.Framework.TestCase(true, 128)]
        public void BruteSampleTiming_RestoreRestoresStrideAndFlag(bool prior, int priorStride)
        {
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            FieldInfo queryField = owner.GetField("branchTimingQuery", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo previousField = owner.GetField("previousBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo strideField = owner.GetField("previousBranchTimingSampleStride", BindingFlags.Static | BindingFlags.NonPublic);
            PropertyInfo stride = typeof(BruteForceSceneQuery).GetProperty("BruteBranchTimingSampleStrideForDiagnostics");
            MethodInfo restore = owner.GetMethod("RestoreBruteBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(strideField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(stride, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
            object previous = previousField.GetValue(null);
            object previousStride = strideField.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteBranchTimingForDiagnostics = !prior;
            stride.SetValue(query, 64);
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, prior);
                strideField.SetValue(null, priorStride);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(prior));
                NUnit.Framework.Assert.That(stride.GetValue(query), NUnit.Framework.Is.EqualTo(priorStride));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally
            {
                queryField.SetValue(null, null);
                previousField.SetValue(null, previous);
                strideField.SetValue(null, previousStride);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteSampleTimingRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteSampledTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH45-BRUTE-SAMPLED-TIMING"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteSampleTimingRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteSampledTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(
                    () => method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException,
                    NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteBranchTiming_RestoreRestoresPriorValueAndReleasesReference(bool prior)
        {
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            FieldInfo queryField = owner.GetField("branchTimingQuery", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo previousField = owner.GetField("previousBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo restore = owner.GetMethod("RestoreBruteBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
            object previous = previousField.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteBranchTimingForDiagnostics = !prior;
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, prior);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(prior));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally
            {
                queryField.SetValue(null, null);
                previousField.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteBranchTimingRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteBranchTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH44-BRUTE-BRANCH-TIMING"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteBranchTimingRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteBranchTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(
                    () => method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException,
                    NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteGeometryFirstRequest_OnlyChangesOutputFromExactCacheSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteExactCacheRequest(index);
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH37-BRUTE-GEOMETRY-FIRST-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-smoke/report.json";
            MethodInfo method = GetBruteGeometryFirstRequestMethod();
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
        }

        [NUnit.Framework.Test]
        public void BruteGeometryFirstRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            MethodInfo method = GetBruteGeometryFirstRequestMethod();
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(
                    () => method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static MethodInfo GetBruteGeometryFirstRequestMethod()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteGeometryFirstRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            return method;
        }



        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteExactCacheRequest_OnlyChangesOutputFromRosterSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrRosterRequest(index);
            MethodInfo method = GetBruteExactCacheRequestMethod();
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH36-BRUTE-EXACT-CACHE"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteExactCacheRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            MethodInfo method = GetBruteExactCacheRequestMethod();
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException exception = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(exception.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static MethodInfo GetBruteExactCacheRequestMethod()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteExactCacheRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, "Independent exact-cache two-smoke request required.");
            return method;
        }


        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteEmptyItrRosterRequest_OnlyChangesOutputFromBruteSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            MethodInfo method = GetBruteEmptyItrRosterRequestMethod();
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH34-BRUTE-EMPTY-ITR-ROSTER"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEmptyItrRosterRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            MethodInfo method = GetBruteEmptyItrRosterRequestMethod();
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException exception = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(exception.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static MethodInfo GetBruteEmptyItrRosterRequestMethod()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteEmptyItrRosterRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, "Independent two-smoke candidate request required.");
            return method;
        }
        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteEmptyItrGuardRequest_OnlyChangesOutputFromBruteSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrGuardRequest(index);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH32-BRUTE-EMPTY-ITR-GUARD"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEmptyItrGuardRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrGuardRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrGuardRequest(2));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void PairSnapshotReuseRequest_ChangesOnlyOutput(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(index);
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-smoke/report.json";
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildRolePairSnapshotReuseRequest(index);
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
        }

        [NUnit.Framework.Test]
        public void PairSnapshotReuseRequest_RejectsOutsideFixedMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRolePairSnapshotReuseRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRolePairSnapshotReuseRequest(2));
        }

        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Preparing, true)]
        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Running, false)]
        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Stopping, false)]
        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Stopped, false)]
        public void StressPreparation_ReopensOnlyEmptyPreparingServiceOwners(
            BattleRuntimeLifecycleState lifecycle, bool admitted)
        {
            var fixture = new NTSD.Test.NTSD28PreparingShutdownOwnerEditorTests();
            fixture.SetUp();
            try
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                Type fixtureType = fixture.GetType();
                var driver = (SimulationTickDriver)fixtureType.GetField("driver", flags).GetValue(fixture);
                var pool = (NTSD.Animation.LF2ObjectPool)fixtureType.GetField("pool", flags).GetValue(fixture);
                pool.BeginBattleShutdown();
                typeof(SimulationTickDriver).GetField("lifecycleState", flags).SetValue(driver, lifecycle);
                MethodInfo prepare = typeof(ProductionEntityStressRunner).GetMethod(
                    "PrepareStressWorldServices", BindingFlags.Static | BindingFlags.NonPublic);
                NUnit.Framework.Assert.IsNotNull(prepare, "Stress preparation must use the existing preparing-owner contract.");
                if (admitted)
                {
                    prepare.Invoke(null, new object[] { driver });
                    NUnit.Framework.Assert.IsTrue(pool.AcceptingRequestsForDiagnostics);
                    NUnit.Framework.Assert.IsFalse(pool.IsBattleCapacitySealed);
                    NUnit.Framework.Assert.AreSame(pool,
                        typeof(SimulationTickDriver).GetField("_battleObjectPool", flags).GetValue(driver));
                }
                else
                {
                    NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        prepare.Invoke(null, new object[] { driver }));
                    NUnit.Framework.Assert.IsFalse(pool.AcceptingRequestsForDiagnostics);
                }
            }
            finally
            {
                fixture.TearDown();
            }
        }

        [NUnit.Framework.TestCase(0L, true)]
        [NUnit.Framework.TestCase(-1L, false)]
        [NUnit.Framework.TestCase(1L, false)]
        [NUnit.Framework.TestCase(-60000L, false)]
        public void UnifiedAuthorityClosure_RequiresProducerAndInputTailRefresh(
            long refreshDelta, bool expected)
        {
            Type existing = typeof(ProductionEntityStressEditorTests);
            var report = (ProductionEntityStressReport)existing.GetMethod(
                "CreateValidAiUnifiedSnapshotAuthorityReport",
                BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            report.aiUnifiedSnapshotExecutionRefreshCount =
                report.aiUnifiedSnapshotExecutionReadCount * 2L + refreshDelta;
            MethodInfo evaluate = typeof(ProductionEntityStressRunner).GetMethod(
                "EvaluateAiUnifiedSnapshotAuthorityValidityForReport",
                BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.AreEqual(expected, evaluate.Invoke(null, new object[] { report, true }));
            NUnit.Framework.Assert.AreEqual(expected, report.harnessValidity);
        }

        [NUnit.Framework.TestCase(-1, false)]
        [NUnit.Framework.TestCase(0, false)]
        [NUnit.Framework.TestCase(1, true)]
        public void OrdinaryStressSpawnOid_PreservesFactoryAdmission(int oid, bool admitted)
        {
            MethodInfo check = typeof(ProductionEntityStressRunner).GetMethod("IsStressSpawnOidAdmitted", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.AreEqual(admitted, check.Invoke(null, new object[] { oid }));
        }

        [NUnit.Framework.TestCase(false, 1000, 1000, 2000, 1000, 1000, true)]
        [NUnit.Framework.TestCase(false, 0, 0, 1000, 1000, 1000, false)]
        [NUnit.Framework.TestCase(true, 0, 0, 1000, 1000, 1000, true)]
        [NUnit.Framework.TestCase(true, 0, 0, 1000, 999, 1000, false)]
        [NUnit.Framework.TestCase(true, 0, 0, 1000, 1000, 999, false)]
        [NUnit.Framework.TestCase(true, 0, 0, 999, 1000, 1000, false)]
        [NUnit.Framework.TestCase(true, 1, 0, 1000, 1000, 1000, false)]
        [NUnit.Framework.TestCase(true, 0, 1, 1000, 1000, 1000, false)]
        public void PopulationAdmission_DistinguishesLogicOnlyWithoutRelaxingCounts(
            bool logicOnly, int gameObjects, int children, int objects, int entities, int slots, bool expected)
        {
            Type policy = typeof(ProductionEntityStressRunner).Assembly.GetType("NTSD.Animation.Rendering.Editor.ProductionEntityStressPopulationPolicy", true);
            MethodInfo evaluate = policy.GetMethod("Evaluate", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.AreEqual(expected, evaluate.Invoke(null,
                new object[] { 1000, gameObjects, children, objects, entities, slots, logicOnly }));
        }

        [NUnit.Framework.TestCase(0, "dispersed1000", 180)]
        [NUnit.Framework.TestCase(1, "combat1000", 180)]
        [NUnit.Framework.TestCase(2, "dispersed1000", 1800)]
        [NUnit.Framework.TestCase(3, "dispersed1000", 1800)]
        [NUnit.Framework.TestCase(4, "combat1000", 1800)]
        [NUnit.Framework.TestCase(5, "combat1000", 1800)]
        public void FixedRequest_PreservesAiCadenceAndBaseline(int index, string action, int samples)
        {
            ProductionEntityStressRequest request = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            NUnit.Framework.Assert.AreEqual(action, request.action);
            NUnit.Framework.Assert.AreEqual(samples, request.sampleTicks);
            NUnit.Framework.Assert.AreEqual(120, request.warmupTicks);
            NUnit.Framework.Assert.AreEqual(1000, request.entityCount);
            NUnit.Framework.Assert.AreEqual("ai", request.inputMode);
            NUnit.Framework.Assert.AreEqual("DataOrientedCanonical", request.aiExecutionProfile);
            NUnit.Framework.Assert.AreEqual("brute", request.formalCollectorMode);
            NUnit.Framework.Assert.AreEqual(2, request.maxCatchUpTicksPerFrame);
            NUnit.Framework.Assert.AreEqual(2, request.maxBacklogTicks);
            NUnit.Framework.Assert.IsFalse(request.simulationOnly);
            NUnit.Framework.Assert.IsFalse(request.useDedicatedSimulationWorker);
            NUnit.Framework.Assert.IsTrue(request.autoStopWhenSampled);
            NUnit.Framework.Assert.IsTrue(request.requireZeroGcAfterWarmup);
            NUnit.Framework.Assert.AreEqual("dispatch", request.soundPresentationMode);
            NUnit.Framework.Assert.AreEqual(0x4E545344u, request.seed);
            Type configType = typeof(ProductionEntityStressRunner).Assembly.GetType(
                "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
            MethodInfo parse = configType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.DoesNotThrow(() => parse.Invoke(null, new object[] { request, Path.GetFullPath(".") }));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void RoleCandidateRequest_ChangesOnlyCollectorAndOutput(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            expected.formalCollectorMode = "role";
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH29-ROLE-COLLECTOR-ADMISSION-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-smoke/report.json";
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(index);
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
            Type configType = typeof(ProductionEntityStressRunner).Assembly.GetType(
                "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
            MethodInfo parse = configType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.DoesNotThrow(() => parse.Invoke(null, new object[] { actual, Path.GetFullPath(".") }));
        }

        [NUnit.Framework.Test]
        public void RoleCandidateRequest_RejectsOutsideFixedMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(2));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void RoleFormalRequest_ChangesOnlyCollectorAndOutput(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index + 2);
            expected.formalCollectorMode = "role";
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH30-ROLE-FORMAL-WINDOWS-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-formal-" + (index % 2 == 0 ? "01" : "02") + "/report.json";
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildRoleFormalRequest(index);
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
            NUnit.Framework.Assert.AreEqual(1800, actual.sampleTicks);
            Type configType = typeof(ProductionEntityStressRunner).Assembly.GetType(
                "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
            MethodInfo parse = configType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.DoesNotThrow(() => parse.Invoke(null, new object[] { actual, Path.GetFullPath(".") }));
        }

        [NUnit.Framework.Test]
        public void RoleFormalRequest_RejectsOutsideFixedMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleFormalRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleFormalRequest(4));
        }

        [NUnit.Framework.TestCase(0, true)]
        [NUnit.Framework.TestCase(1, false)]
        [NUnit.Framework.TestCase(2, false)]
        [NUnit.Framework.TestCase(3, false)]
        [NUnit.Framework.TestCase(4, false)]
        [NUnit.Framework.TestCase(5, false)]
        [NUnit.Framework.TestCase(6, false)]
        [NUnit.Framework.TestCase(7, false)]
        [NUnit.Framework.TestCase(8, false)]
        [NUnit.Framework.TestCase(9, false)]
        [NUnit.Framework.TestCase(10, false)]
        [NUnit.Framework.TestCase(11, false)]
        [NUnit.Framework.TestCase(12, false)]
        [NUnit.Framework.TestCase(13, false)]
        [NUnit.Framework.TestCase(14, false)]
        [NUnit.Framework.TestCase(15, false)]
        [NUnit.Framework.TestCase(16, false)]
        [NUnit.Framework.TestCase(17, false)]
        [NUnit.Framework.TestCase(18, false)]
        public void RetainedGcFailure_NeverPromotesOrContinuesUnsafeMeasurement(int defect, bool expected)
        {
            var report = new ProductionEntityStressReport
            {
                status = "StoppedWithResidue",
                failure = "ZeroGcGateFailed: retained strict verdict",
                sampledLogicTicks = 1800,
                warmupTicksCompleted = 120,
                zeroGcGatePassed = false,
                harnessValidity = false,
            };
            report.teardown.attempted = true;
            report.teardown.restored = true;
            report.capacityPressure.passed = true;
            int observedWindows = 12;
            int activeAi = 1000;
            int baseRoster = 1000;
            switch (defect)
            {
                case 1: report.status = "Failed"; break;
                case 2: report.sampledLogicTicks = 1799; break;
                case 3: report.warmupTicksCompleted = 119; break;
                case 4: activeAi = 999; break;
                case 5: baseRoster = 999; break;
                case 6: observedWindows = 0; break;
                case 7: report.teardown.restored = false; break;
                case 8: report.capacityPressure.passed = false; break;
                case 9: report.capacityPressure.capacityCriticalDelta = 1; break;
                case 10: report.u6ProductionCanonicalMismatchCount = 1; break;
                case 11: report.aiUnifiedSnapshotExecutionPreCommitFallbackCount = 1; break;
                case 12: report.aiUnifiedSnapshotExecutionPostCommitHardBreachCount = 1; break;
                case 13: report.failure = "CapacityPressureGateFailed: rejected"; break;
                case 14: report.zeroGcGatePassed = true; break;
                case 15: report.teardown.cleanupExceptionCount = 1; break;
                case 16: report.teardown = null; break;
                case 17: report.capacityPressure = null; break;
                case 18: report.u6ProductionDataOrientedCompatibilityFallbackCount = 1; break;
            }
            string originalFailure = report.failure;
            NUnit.Framework.Assert.AreEqual(expected,
                BattleOptimizationWindowsAiSuiteEditor.MayContinueAfterRetainedGcFailure(
                    report, 1800, observedWindows, activeAi, baseRoster));
            NUnit.Framework.Assert.IsFalse(report.harnessValidity, "A retained report remains invalid, not admitted.");
            NUnit.Framework.Assert.AreEqual(originalFailure, report.failure, "The strict failure cannot be overwritten.");
            NUnit.Framework.Assert.AreEqual(defect == 14, report.zeroGcGatePassed);
        }
    }
#endif
}
#endif
