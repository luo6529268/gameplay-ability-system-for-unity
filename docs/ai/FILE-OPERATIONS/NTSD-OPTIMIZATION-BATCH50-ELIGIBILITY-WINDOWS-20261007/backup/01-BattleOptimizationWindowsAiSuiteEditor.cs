#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.Rendering.Editor;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
        private const string SharedRequest = "Temp/NTSD_ProductionEntityStress.request.json";
        private const string SharedResult = "Temp/NTSD_ProductionEntityStress.result";
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

        [Serializable]
        private sealed class RunState
        {
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
            public bool bruteKind5PresenceRestored;
            public long bruteKind5PresenceAppliedBaseline;
            public long bruteKind5PresenceAppliedDelta;
            public long bruteKind5PresenceMaximumSkippedScans;
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

        private static void BeginSuite(bool roleCandidate, bool roleFormalCandidate = false,
            bool pairSnapshotCandidate = false, bool emptyItrGuardCandidate = false,
            bool emptyItrRosterCandidate = false, bool bruteExactCacheCandidate = false,
            bool bruteGeometryFirstCandidate = false, bool cpuGcCaptureOnly = false,
            bool bruteProductionOnly = false, bool bruteBranchTimingOnly = false,
            bool bruteBranchTimingSampled = false, bool bruteKind5PresenceCandidate = false)
        {
            Require(state == null && !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Original Editor must be idle and no suite may be active.");
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
            string outputRoot = bruteKind5PresenceCandidate ? BruteKind5PresenceOutputRoot :
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
                runs = new RunState[bruteBranchTimingOnly ? 4 : cpuGcCaptureOnly ? 1 : bruteProductionOnly || emptyItrGuardCandidate ? 2 : roleFormalCandidate ? 4 : roleCandidate ? 2 : 6],
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

        internal static ProductionEntityStressRequest BuildBruteProductionRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = BruteProductionOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
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
                    (!(state.bruteProductionOnly || state.bruteBranchTimingOnly) ||
                     (run.bruteProductionDefaultsObserved && run.bruteProductionDefaultsUnchanged &&
                      run.emptyItrRosterObservedApplied && !run.emptyItrRosterFallbackObserved &&
                      run.bruteExactCacheObservedApplied && !run.bruteExactCacheFallbackObserved &&
                      run.bruteExactCacheAppliedDelta == run.warmupTicks + run.sampledTicks &&
                      run.bruteExactCacheFallbackDelta == 0 &&
                      run.bruteGeometryFirstAppliedDelta == run.warmupTicks + run.sampledTicks));
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
            if (state.cpuGcCaptureOnly)
                BattleOptimizationCpuGcCaptureEditor.Arm(PathFor(state.outputRoot));
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
        }

        private static void ObserveRunningSample(RunState run, ProductionEntityStressReport report)
        {
            if (report.status != "Running" || report.sampledLogicTicks <= run.lastObservedSampleTick)
                return;
            run.lastObservedSampleTick = report.sampledLogicTicks;
            run.minimumObservedActiveAi = Math.Min(run.minimumObservedActiveAi, report.baseAiActiveCount);
            run.minimumObservedBaseRoster = Math.Min(run.minimumObservedBaseRoster, report.baseRosterActiveCount);
            run.observedSampleWindows++;
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
                  PathFor(lines[1]).StartsWith(kind5PresenceOwnerRoot, StringComparison.OrdinalIgnoreCase));
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
