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
        private static bool previousEmptyItrGuard;
        private static bool previousEmptyItrRoster;

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

        private static void BeginSuite(bool roleCandidate, bool roleFormalCandidate = false,
            bool pairSnapshotCandidate = false, bool emptyItrGuardCandidate = false,
            bool emptyItrRosterCandidate = false)
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
            string outputRoot = emptyItrRosterCandidate ? EmptyItrRosterOutputRoot :
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
                runs = new RunState[emptyItrGuardCandidate ? 2 : roleFormalCandidate ? 4 : roleCandidate ? 2 : 6],
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

        private static ProductionEntityStressRequest BuildCurrentRequest(int index)
        {
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
                if (run.emptyItrGuardApplied)
                {
                    bool restored = RestoreEmptyItrGuard();
                    run.emptyItrGuardRestored = restored;
                    if (run.emptyItrRosterApplied)
                        run.emptyItrRosterRestored = restored;
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
                      run.emptyItrRosterObservedApplied && !run.emptyItrRosterFallbackObserved));
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
                emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics = true;
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
            if (run.emptyItrRosterApplied && emptyItrGuardQuery != null)
            {
                run.emptyItrRosterObservedApplied |= emptyItrGuardQuery.LastBruteEmptyItrRosterAppliedForDiagnostics;
                run.emptyItrRosterFallbackObserved |= emptyItrGuardQuery.LastBruteEmptyItrRosterFallbackForDiagnostics;
                run.emptyItrRosterMaximumBuildCount = Math.Max(run.emptyItrRosterMaximumBuildCount,
                    emptyItrGuardQuery.LastBruteEmptyItrRosterBuildCountForDiagnostics);
                run.emptyItrRosterMaximumVisitedPairs = Math.Max(run.emptyItrRosterMaximumVisitedPairs,
                    emptyItrGuardQuery.LastBruteEmptyItrRosterVisitedPairCountForDiagnostics);
                run.emptyItrRosterMaximumSkippedPairs = Math.Max(run.emptyItrRosterMaximumSkippedPairs,
                    emptyItrGuardQuery.LastBruteEmptyItrRosterSkippedPairCountForDiagnostics);
            }
        }

        private static void SaveProgress(RunState run)
        {
            run.sampledTicks = currentReport.sampledLogicTicks;
            run.warmupTicks = currentReport.warmupTicksCompleted;
            SaveNew(state.outputRoot + "/progress-" + (progressOrdinal++).ToString("D4") + ".json", JsonUtility.ToJson(run, true));
        }

        private static void ShutdownAndExit()
        {
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
            return lines.Length >= 2 && (lines[0] == "PASS" || lines[0] == "FAIL") &&
                (PathFor(lines[1]).StartsWith(ownerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(currentOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(candidateOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(formalOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(pairSnapshotOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(emptyItrOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(emptyItrRosterOwnerRoot, StringComparison.OrdinalIgnoreCase));
        }

        private static bool RestoreEmptyItrGuard()
        {
            if (emptyItrGuardQuery == null)
                return true;
            emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics = previousEmptyItrGuard;
            emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics = previousEmptyItrRoster;
            bool restored = emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics == previousEmptyItrGuard &&
                emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics == previousEmptyItrRoster;
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
