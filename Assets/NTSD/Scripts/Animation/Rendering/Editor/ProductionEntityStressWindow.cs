#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text;
using NTSD.Simulation;
using NTSD.Test;
using UnityEditor;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class ProductionEntityStressWindow : EditorWindow
    {
        private int warmupTicks = 30;
        private int sampleTicks = 300;
        private int aiSimulationSmokeSampleTicks = 30;
        private int spawnBatchSize = 25;
        private int maxCatchUpTicksPerFrame = 4;
        private int maxBacklogTicks = 8;
        private int maxSaturationDrainTicks = 300;
        private int formalCollectorModeIndex;
        private int aiExecutionProfileIndex;
        private int lateRuntimeSnapshotModeIndex = 1;
        private int soundPresentationModeIndex;
        private bool enableAiDecisionSoAShadow;
        private bool enableAiDecisionSharedShadow;
        private uint seed = 0x4E545344u;
        private bool simulationOnly;
        private bool skipLateRendererUpdate;
        private bool autoStopWhenSampled;
        private bool enablePhaseTiming;
        private bool enablePresentationTiming;
        private bool enableDetailPhaseTiming;
        private string outputPath = "Temp/NTSD_ProductionEntityStress.dispersed.json";
        private string status = "Ready. A start request enters Play Mode and leaves the 1000-entity run visible until cleanup.";
        private static readonly string[] FormalCollectorModes =
        {
            "configured",
            "legacy",
            "role",
            "brute",
        };
        private static readonly string[] AiExecutionProfiles =
        {
            "legacy",
            "data-oriented-canonical",
        };
        private static readonly string[] AiExecutionProfileLabels =
        {
            "Legacy Canonical",
            "Data-Oriented Canonical",
        };
        private static readonly string[] LateRuntimeSnapshotModes =
        {
            "legacy-three",
            "consolidated-final",
        };
        private static readonly string[] LateRuntimeSnapshotModeLabels =
        {
            "Legacy Three",
            "Consolidated Final",
        };
        private static readonly string[] SoundPresentationModes =
        {
            "inherit",
            "suppress",
            "dispatch",
        };
        private static readonly string[] SoundPresentationModeLabels =
        {
            "Inherit from Simulation Only",
            "Suppress",
            "Dispatch",
        };

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress")]
        public static void Open()
        {
            GetWindow<ProductionEntityStressWindow>("Production Entity Stress");
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 50 Smoke")]
        public static void RunSmokeFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDefaultRequest("smoke", "Temp/NTSD_ProductionEntityStress.smoke.json"));
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 Dispersed")]
        public static void RunDispersedFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDefaultRequest("dispersed", "Temp/NTSD_ProductionEntityStress.dispersed.json"));
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run 1000 Concentrated")]
        public static void RunConcentratedFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDefaultRequest("concentrated", "Temp/NTSD_ProductionEntityStress.concentrated.json"));
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run AI Simulation Smoke/100 Dispersed")]
        public static void RunDispersed100AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(100);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run AI Simulation Smoke/300 Dispersed")]
        public static void RunDispersed300AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(300);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run AI Simulation Smoke/500 Dispersed")]
        public static void RunDispersed500AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(500);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Run AI Simulation Smoke/1000 Dispersed")]
        public static void RunDispersed1000AiSimulationSmokeFromMenu()
        {
            WriteDispersedAiSimulationSmokeRequest(1000);
        }

        [MenuItem("NTSD/Battle Diagnostics/Production Entity Stress/Stop and Cleanup")]
        public static void StopFromMenu()
        {
            ProductionEntityStressRequestProcessor.WriteStopRequest();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Real Production Entity Stress", EditorStyles.boldLabel);
            EditorGUILayout.LabelField(
                "Profile",
                "MobileExtended (1050 slots), LooseQuadtree");
            warmupTicks = EditorGUILayout.IntField("Warmup Logic Ticks", warmupTicks);
            sampleTicks = EditorGUILayout.IntField("Target Sample Ticks", sampleTicks);
            aiSimulationSmokeSampleTicks = EditorGUILayout.IntSlider(
                "AI Simulation Smoke Samples",
                aiSimulationSmokeSampleTicks,
                10,
                30);
            spawnBatchSize = EditorGUILayout.IntSlider("Spawn Batch", spawnBatchSize, 1, 100);
            maxCatchUpTicksPerFrame = EditorGUILayout.IntSlider(
                "Max Catch-up Ticks",
                maxCatchUpTicksPerFrame,
                1,
                12);
            maxBacklogTicks = EditorGUILayout.IntSlider(
                "Max Backlog Ticks",
                maxBacklogTicks,
                maxCatchUpTicksPerFrame,
                30);
            maxSaturationDrainTicks = EditorGUILayout.IntField(
                "Max Saturation Drain Ticks",
                Math.Max(1, maxSaturationDrainTicks));
            formalCollectorModeIndex = EditorGUILayout.Popup(
                "Formal Collector",
                formalCollectorModeIndex,
                FormalCollectorModes);
            aiExecutionProfileIndex = EditorGUILayout.Popup(
                "AI Execution Profile",
                aiExecutionProfileIndex,
                AiExecutionProfileLabels);
            lateRuntimeSnapshotModeIndex = EditorGUILayout.Popup(
                "Late Snapshot",
                lateRuntimeSnapshotModeIndex,
                LateRuntimeSnapshotModeLabels);
            bool requestedDeepShadow = EditorGUILayout.Toggle(
                "AI Decision SoA Shadow",
                enableAiDecisionSoAShadow);
            if (requestedDeepShadow != enableAiDecisionSoAShadow)
            {
                enableAiDecisionSoAShadow = requestedDeepShadow;
                if (enableAiDecisionSoAShadow)
                    enableAiDecisionSharedShadow = false;
            }
            bool requestedSharedShadow = EditorGUILayout.Toggle(
                "AI Decision Shared Shadow",
                enableAiDecisionSharedShadow);
            if (requestedSharedShadow != enableAiDecisionSharedShadow)
            {
                enableAiDecisionSharedShadow = requestedSharedShadow;
                if (enableAiDecisionSharedShadow)
                    enableAiDecisionSoAShadow = false;
            }
            if (AiExecutionProfiles[aiExecutionProfileIndex] ==
                "data-oriented-canonical")
            {
                EditorGUILayout.HelpBox(
                    "Uses the atomic SoA sensing + indexed decision + unified authority production profile.",
                    MessageType.Info);
            }
            long editedSeed = EditorGUILayout.LongField("Deterministic Seed", seed);
            seed = (uint)Math.Max(uint.MinValue, Math.Min(uint.MaxValue, editedSeed));
            simulationOnly = EditorGUILayout.Toggle("Simulation Only", simulationOnly);
            soundPresentationModeIndex = EditorGUILayout.Popup(
                "Sound Presentation",
                soundPresentationModeIndex,
                SoundPresentationModeLabels);
            using (new EditorGUI.DisabledScope(!simulationOnly))
            {
                skipLateRendererUpdate = EditorGUILayout.Toggle(
                    "Skip Late Renderer Update",
                    skipLateRendererUpdate);
            }
            if (skipLateRendererUpdate && !simulationOnly)
            {
                EditorGUILayout.HelpBox(
                    "Skip Late Renderer Update requires Simulation Only.",
                    MessageType.Error);
            }
            autoStopWhenSampled = EditorGUILayout.Toggle(
                "Auto-stop When Sampled",
                autoStopWhenSampled);
            enablePhaseTiming = EditorGUILayout.Toggle("Coarse Phase Timing", enablePhaseTiming);
            enablePresentationTiming = EditorGUILayout.Toggle(
                "Presentation Timing",
                enablePresentationTiming);
            enableDetailPhaseTiming = EditorGUILayout.Toggle(
                "Detail Phase Timing",
                enableDetailPhaseTiming);
            outputPath = EditorGUILayout.TextField("Report Path", outputPath);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("50 Smoke"))
                    WriteStart("smoke", "Temp/NTSD_ProductionEntityStress.smoke.json");
                if (GUILayout.Button("1000 Dispersed"))
                    WriteStart("dispersed", "Temp/NTSD_ProductionEntityStress.dispersed.json");
                if (GUILayout.Button("1000 Concentrated"))
                    WriteStart("concentrated", "Temp/NTSD_ProductionEntityStress.concentrated.json");
            }
            if (GUILayout.Button("Stop / Cleanup"))
                StopFromMenu();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Dispersed AI Simulation-only Smoke", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Frozen ladder: 30 warmup ticks, 10-30 samples, auto-stop, deterministic AI input, and no presentation.",
                MessageType.None);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("100 AI Sim"))
                    WriteDispersedAiSimulationSmoke(100);
                if (GUILayout.Button("300 AI Sim"))
                    WriteDispersedAiSimulationSmoke(300);
                if (GUILayout.Button("500 AI Sim"))
                    WriteDispersedAiSimulationSmoke(500);
                if (GUILayout.Button("1000 AI Sim"))
                    WriteDispersedAiSimulationSmoke(1000);
            }

            ProductionEntityStressRunner runner = ProductionEntityStressRunner.Active;
            if (runner != null && runner.Report != null)
            {
                ProductionEntityStressReport report = runner.Report;
                status = $"{report.status}: activeGO={report.activeGameObjectCount}, " +
                         $"world={report.worldObjectCount}, slots={report.claimedRuntimeSlotCount}, " +
                         $"samples={report.sampledLogicTicks}, collector={report.formalCollectorMode}, " +
                         $"bodyEntries={report.formalCollectorBodyEntries}, " +
                         $"itrQueries={report.formalCollectorItrQueries}";
                Repaint();
            }
            EditorGUILayout.HelpBox(status, MessageType.Info);
        }

        private void WriteStart(string action, string defaultOutputPath)
        {
            try
            {
                string selectedOutput = string.IsNullOrWhiteSpace(outputPath)
                    ? defaultOutputPath
                    : outputPath;
                var request = new ProductionEntityStressRequest
                {
                    action = action,
                    warmupTicks = warmupTicks,
                    sampleTicks = sampleTicks,
                    spawnBatchSize = spawnBatchSize,
                    maxCatchUpTicksPerFrame = maxCatchUpTicksPerFrame,
                    maxBacklogTicks = maxBacklogTicks,
                    maxSaturationDrainTicks = maxSaturationDrainTicks,
                    enablePhaseTiming = enablePhaseTiming,
                    enablePresentationTiming = enablePresentationTiming,
                    enableDetailPhaseTiming = enableDetailPhaseTiming,
                    simulationOnly = simulationOnly,
                    soundPresentationMode =
                        SoundPresentationModes[soundPresentationModeIndex],
                    skipLateRendererUpdate = skipLateRendererUpdate,
                    autoStopWhenSampled = autoStopWhenSampled,
                    seed = seed,
                    aiExecutionProfile = AiExecutionProfiles[aiExecutionProfileIndex],
                    lateRuntimeSnapshotMode =
                        LateRuntimeSnapshotModes[lateRuntimeSnapshotModeIndex],
                    enableAiDecisionSoAShadow = enableAiDecisionSoAShadow,
                    enableAiDecisionSharedShadow = enableAiDecisionSharedShadow,
                    formalCollectorMode = FormalCollectorModes[formalCollectorModeIndex],
                    outputPath = selectedOutput,
                };
                ProductionEntityStressRequestProcessor.WriteRequest(request);
                status = "Request written. Waiting for Play Mode production services.";
            }
            catch (Exception exception)
            {
                status = exception.Message;
                Debug.LogError($"[ProductionEntityStress] Request write failed: {exception}");
            }
        }

        private void WriteDispersedAiSimulationSmoke(int entityCount)
        {
            try
            {
                string defaultOutputPath =
                    $"Temp/NTSD_ProductionEntityStress.dispersed{entityCount}.ai-sim-smoke.json";
                string selectedOutput = string.IsNullOrWhiteSpace(outputPath)
                    ? defaultOutputPath
                    : outputPath;
                ProductionEntityStressRequestProcessor.WriteRequest(
                    CreateDispersedAiSimulationOnlySmokeRequest(
                        entityCount,
                        selectedOutput,
                        aiSimulationSmokeSampleTicks,
                        LateRuntimeSnapshotModes[lateRuntimeSnapshotModeIndex],
                        skipLateRendererUpdate));
                status = "AI simulation-only smoke request written. Waiting for Play Mode production services.";
            }
            catch (Exception exception)
            {
                status = exception.Message;
                Debug.LogError($"[ProductionEntityStress] Request write failed: {exception}");
            }
        }

        internal static ProductionEntityStressRequest CreateDefaultRequest(
            string action,
            string reportPath)
        {
            bool smoke = string.Equals(action, "smoke", StringComparison.Ordinal);
            return new ProductionEntityStressRequest
            {
                action = action,
                warmupTicks = smoke ? 2 : 30,
                sampleTicks = smoke ? 10 : 300,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
                maxSaturationDrainTicks = 300,
                aiExecutionProfile = "legacy",
                lateRuntimeSnapshotMode = "consolidated-final",
                formalCollectorMode = "configured",
                outputPath = reportPath,
            };
        }

        internal static ProductionEntityStressRequest CreateDispersedAiSimulationOnlySmokeRequest(
            int entityCount,
            string reportPath,
            int sampleTicks = 30,
            string lateRuntimeSnapshotMode = "consolidated-final",
            bool skipLateRendererUpdate = false)
        {
            if (sampleTicks < 10 || sampleTicks > 30)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sampleTicks),
                    sampleTicks,
                    "AI simulation smoke samples must be in the inclusive range 10..30.");
            }

            return new ProductionEntityStressRequest
            {
                action = GetDispersedAction(entityCount),
                entityCount = entityCount,
                inputMode = "ai",
                warmupTicks = 30,
                sampleTicks = sampleTicks,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 4,
                maxBacklogTicks = 8,
                maxSaturationDrainTicks = 300,
                simulationOnly = true,
                skipLateRendererUpdate = skipLateRendererUpdate,
                autoStopWhenSampled = true,
                seed = 0x4E545344u,
                aiExecutionProfile = "legacy",
                lateRuntimeSnapshotMode = lateRuntimeSnapshotMode,
                formalCollectorMode = "configured",
                outputPath = reportPath,
            };
        }

        private static void WriteDispersedAiSimulationSmokeRequest(int entityCount)
        {
            string outputPath =
                $"Temp/NTSD_ProductionEntityStress.dispersed{entityCount}.ai-sim-smoke.json";
            ProductionEntityStressRequestProcessor.WriteRequest(
                CreateDispersedAiSimulationOnlySmokeRequest(entityCount, outputPath));
        }

        private static string GetDispersedAction(int entityCount)
        {
            switch (entityCount)
            {
                case 100:
                    return "dispersed100";
                case 300:
                    return "dispersed300";
                case 500:
                    return "dispersed500";
                case 1000:
                    return "dispersed1000";
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(entityCount),
                        entityCount,
                        "Dispersed AI simulation smoke supports only 100, 300, 500, or 1000 entities.");
            }
        }
    }

    internal enum ProductionEntityStressPlayRestartDecision
    {
        None = 0,
        WaitForInitialServices = 1,
        RecordHealthyRuntime = 2,
        WaitForRestartTransition = 3,
        RestartPlayMode = 4,
        RetryLimitExceeded = 5,
    }

    internal enum ProductionEntityStressReloadRecoveryDecision
    {
        None = 0,
        RetryAfterCleanExit = 1,
        TerminalFailure = 2,
    }

    internal enum ProductionEntityStressReloadRecoveryTransition
    {
        None = 0,
        Wait = 1,
        ExitPlayMode = 2,
        EnterPlayMode = 3,
    }

    [InitializeOnLoad]
    internal static class ProductionEntityStressRequestProcessor
    {
        private const string SessionRequestJsonKey =
            "NTSD.ProductionEntityStress.RequestJson";
        private const string SessionServiceWaitDeadlineKey =
            "NTSD.ProductionEntityStress.ServiceWaitDeadlineRealtime";
        private const string SessionManagedRuntimeObservedKey =
            "NTSD.ProductionEntityStress.ManagedRuntimeObserved";
        private const string SessionPlayRestartPendingKey =
            "NTSD.ProductionEntityStress.PlayRestartPending";
        private const string SessionPlayRestartCountKey =
            "NTSD.ProductionEntityStress.PlayRestartCount";
        private const string SessionActiveRequestJsonKey =
            "NTSD.ProductionEntityStress.ActiveRequestJson";
        private const string SessionActiveConfigJsonKey =
            "NTSD.ProductionEntityStress.ActiveConfigJson";
        private const string SessionReloadRecoveryPendingKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryPending";
        private const string SessionReloadRecoveryDispatchedKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryDispatched";
        private const string SessionReloadRecoveryTransitionKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryTransition";
        private const string SessionReloadRecoveryCountKey =
            "NTSD.ProductionEntityStress.ReloadRecoveryCount";
        internal const int PlayRestartLimitForDiagnostics = 1;
        internal const int ReloadRecoveryLimitForDiagnostics = 1;
        internal const double ServiceWaitTimeoutSecondsForDiagnostics = 120d;
        private static bool processing;

        static ProductionEntityStressRequestProcessor()
        {
            ConfigureBootstrapSuppressionFromPendingRequest();
            AssemblyReloadEvents.beforeAssemblyReload += BeforeAssemblyReload;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += PollRequest;
        }

        internal static void WriteRequest(ProductionEntityStressRequest request)
        {
            ProductionEntityStressConfig.FromRequest(
                request,
                ProductionEntityStressPaths.ProjectRoot);
            WriteRequestJson(JsonUtility.ToJson(request));
        }

        internal static void WriteStopRequest()
        {
            WriteRequestJson("{\"action\":\"stop\"}");
        }

        internal static bool ShouldEnterPlayMode(string action, bool isPlaying)
        {
            return !isPlaying && !string.Equals(
                action,
                "stop",
                StringComparison.OrdinalIgnoreCase);
        }

        internal static bool ShouldSuppressBattleTestBootstrap(string action)
        {
            return !string.Equals(
                action,
                "stop",
                StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsReadyToStart(
            bool hasActiveBattleTestBootstrap,
            bool bootstrapReportedReady,
            bool productionServicesReady)
        {
            return productionServicesReady &&
                   (!hasActiveBattleTestBootstrap || bootstrapReportedReady);
        }

        internal static ProductionEntityStressPlayRestartDecision EvaluatePlayRestartDecision(
            bool pendingStartRequest,
            bool isPlaying,
            bool managedRuntimeWasValid,
            bool managedRuntimeExpected,
            bool managedRuntimeIsValid,
            bool restartTransitionPending,
            int restartCount)
        {
            if (!pendingStartRequest || !isPlaying)
                return ProductionEntityStressPlayRestartDecision.None;
            if (managedRuntimeIsValid)
                return ProductionEntityStressPlayRestartDecision.RecordHealthyRuntime;
            if (restartTransitionPending)
                return ProductionEntityStressPlayRestartDecision.WaitForRestartTransition;
            if (!managedRuntimeWasValid && !managedRuntimeExpected && restartCount == 0)
                return ProductionEntityStressPlayRestartDecision.WaitForInitialServices;
            return restartCount < PlayRestartLimitForDiagnostics
                ? ProductionEntityStressPlayRestartDecision.RestartPlayMode
                : ProductionEntityStressPlayRestartDecision.RetryLimitExceeded;
        }

        internal static double ResolveServiceWaitDeadline(
            double realtimeNow,
            string persistedDeadline)
        {
            if (double.TryParse(
                    persistedDeadline,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out double deadline) &&
                !double.IsNaN(deadline) &&
                !double.IsInfinity(deadline))
            {
                return deadline;
            }

            return realtimeNow + ServiceWaitTimeoutSecondsForDiagnostics;
        }

        internal static bool HasServiceWaitTimedOut(double realtimeNow, double deadline)
        {
            return realtimeNow >= deadline;
        }

        internal static ProductionEntityStressReloadRecoveryDecision
            EvaluateReloadRecoveryDecision(
                bool hasCompleteActiveState,
                bool runnerActive,
                int recoveryCount)
        {
            if (!hasCompleteActiveState && !runnerActive)
                return ProductionEntityStressReloadRecoveryDecision.None;
            if (!hasCompleteActiveState || !runnerActive ||
                recoveryCount >= ReloadRecoveryLimitForDiagnostics)
            {
                return ProductionEntityStressReloadRecoveryDecision.TerminalFailure;
            }
            return ProductionEntityStressReloadRecoveryDecision.RetryAfterCleanExit;
        }

        internal static ProductionEntityStressReloadRecoveryTransition
            ResolveReloadRecoveryTransition(
                bool recoveryPending,
                bool isPlaying,
                bool isPlayingOrWillChangePlaymode)
        {
            if (!recoveryPending)
                return ProductionEntityStressReloadRecoveryTransition.None;
            if (isPlaying)
                return ProductionEntityStressReloadRecoveryTransition.ExitPlayMode;
            if (isPlayingOrWillChangePlaymode)
                return ProductionEntityStressReloadRecoveryTransition.Wait;
            return ProductionEntityStressReloadRecoveryTransition.EnterPlayMode;
        }

        internal static string BuildActiveConfigJson(
            string requestJson,
            ProductionEntityStressConfig config)
        {
            var state = new ActiveRunConfigState
            {
                schemaVersion = 1,
                requestHash = BattleCanonicalJson.Sha256(requestJson ?? string.Empty),
                workloadFingerprint = ProductionEntityStressFingerprint.BuildWorkload(config),
                implementationConfigFingerprint =
                    ProductionEntityStressFingerprint.BuildImplementationConfig(config),
                mode = config.Mode.ToString(),
                inputMode = config.InputMode.ToString(),
                entityCount = config.EntityCount,
                warmupTicks = config.WarmupTicks,
                sampleTicks = config.SampleTicks,
                simulationOnly = config.SimulationOnly,
                skipLateRendererUpdate = config.SkipLateRendererUpdate,
                autoStopWhenSampled = config.ShouldAutoStopWhenSampled,
                seed = config.Seed.ToString(CultureInfo.InvariantCulture),
                outputPath = config.OutputPath,
            };
            return JsonUtility.ToJson(state);
        }

        internal static bool IsCompleteActiveRunState(
            string requestJson,
            string configJson)
        {
            if (string.IsNullOrWhiteSpace(requestJson) ||
                string.IsNullOrWhiteSpace(configJson))
            {
                return false;
            }

            try
            {
                ProductionEntityStressRequest request =
                    JsonUtility.FromJson<ProductionEntityStressRequest>(requestJson);
                ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot);
                ActiveRunConfigState state =
                    JsonUtility.FromJson<ActiveRunConfigState>(configJson);
                return state != null &&
                       state.schemaVersion == 1 &&
                       string.Equals(
                           state.requestHash,
                           BattleCanonicalJson.Sha256(requestJson),
                           StringComparison.Ordinal) &&
                       string.Equals(state.mode, config.Mode.ToString(), StringComparison.Ordinal) &&
                       string.Equals(
                           state.inputMode,
                           config.InputMode.ToString(),
                           StringComparison.Ordinal) &&
                       state.entityCount == config.EntityCount &&
                       state.warmupTicks == config.WarmupTicks &&
                       state.sampleTicks == config.SampleTicks &&
                       state.simulationOnly == config.SimulationOnly &&
                       state.skipLateRendererUpdate == config.SkipLateRendererUpdate &&
                       state.autoStopWhenSampled == config.ShouldAutoStopWhenSampled &&
                       string.Equals(
                           state.seed,
                           config.Seed.ToString(CultureInfo.InvariantCulture),
                           StringComparison.Ordinal) &&
                       string.Equals(
                           state.outputPath,
                           config.OutputPath,
                           StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }

        internal static void PollRequestForTests()
        {
            PollRequest();
        }

        private static void WriteRequestJson(string json)
        {
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(json);
            if (ShouldSuppressBattleTestBootstrap(request?.action))
                ClearActiveRunRecoveryState(clearCount: true);
            ClearPlayRestartGuard();
            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(requestPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(requestPath, json, new UTF8Encoding(false));
            SessionState.SetString(SessionRequestJsonKey, json);
            SessionState.SetString(
                SessionServiceWaitDeadlineKey,
                (Time.realtimeSinceStartupAsDouble + ServiceWaitTimeoutSecondsForDiagnostics)
                .ToString("R", CultureInfo.InvariantCulture));
            BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                ShouldSuppressBattleTestBootstrap(request?.action);

            string resultPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.ResultFile);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
        }

        private static void PollRequest()
        {
            if (processing || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (ProcessReloadRecovery())
                return;
            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            if (!File.Exists(requestPath))
            {
                SessionState.EraseString(SessionRequestJsonKey);
                ClearPlayRestartGuard();
                return;
            }

            processing = true;
            try
            {
                string json = File.ReadAllText(requestPath, Encoding.UTF8);
                SessionState.SetString(SessionRequestJsonKey, json);
                ProductionEntityStressRequest request =
                    JsonUtility.FromJson<ProductionEntityStressRequest>(json);
                string action = (request?.action ?? string.Empty).Trim().ToLowerInvariant();
                BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                    ShouldSuppressBattleTestBootstrap(action);

                if (string.Equals(action, "stop", StringComparison.OrdinalIgnoreCase))
                {
                    if (EditorApplication.isPlaying && ProductionEntityStressRunner.Active != null)
                    {
                        ProductionEntityStressRunner.Active.StopAndCleanup("stop-request");
                    }
                    else
                    {
                        ProductionEntityStressPaths.WriteTerminalResult(
                            true,
                            string.Empty,
                            "No active production entity stress runner required cleanup.");
                    }
                    NotifyRunStopped();
                    CompleteRequest(requestPath);
                    return;
                }

                ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot);
                if (ShouldEnterPlayMode(action, EditorApplication.isPlaying))
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        if (SessionState.GetBool(SessionPlayRestartPendingKey, false))
                        {
                            ResetServiceWaitDeadline();
                            SessionState.SetBool(SessionPlayRestartPendingKey, false);
                        }
                        EditorApplication.EnterPlaymode();
                    }
                    return;
                }
                if (!EditorApplication.isPlaying)
                    return;
                if (ProductionEntityStressRunner.Active != null)
                    throw new InvalidOperationException(
                        "Stop the active production stress run before starting another one.");

                bool productionServicesReady =
                    ProductionEntityStressRunner.AreProductionServicesReady();
                ManagedRuntimeState managedRuntime = CaptureManagedRuntimeState();
                ProductionEntityStressPlayRestartDecision restartDecision =
                    EvaluatePlayRestartDecision(
                        true,
                        true,
                        SessionState.GetBool(SessionManagedRuntimeObservedKey, false),
                        managedRuntime.HasServiceFootprint,
                        managedRuntime.IsValid,
                        SessionState.GetBool(SessionPlayRestartPendingKey, false),
                        SessionState.GetInt(SessionPlayRestartCountKey, 0));
                switch (restartDecision)
                {
                    case ProductionEntityStressPlayRestartDecision.RecordHealthyRuntime:
                        SessionState.SetBool(SessionManagedRuntimeObservedKey, true);
                        SessionState.SetBool(SessionPlayRestartPendingKey, false);
                        break;
                    case ProductionEntityStressPlayRestartDecision.RestartPlayMode:
                        RequestCleanPlayRestart(managedRuntime);
                        return;
                    case ProductionEntityStressPlayRestartDecision.RetryLimitExceeded:
                        throw new InvalidOperationException(
                            "Production managed runtime was invalidated again after the single " +
                            $"clean Play Mode restart: {managedRuntime.Describe()}.");
                }

                bool hasActiveBootstrap = HasActiveBattleTestBootstrap();
                bool servicesReady = managedRuntime.IsValid && productionServicesReady;
                if (!IsReadyToStart(
                        hasActiveBootstrap,
                        BattleTestBootstrap.ProductionStressServicesReady,
                        servicesReady))
                {
                    string persistedDeadline = SessionState.GetString(
                        SessionServiceWaitDeadlineKey,
                        string.Empty);
                    double realtimeNow = Time.realtimeSinceStartupAsDouble;
                    double deadline = ResolveServiceWaitDeadline(
                        realtimeNow,
                        persistedDeadline);
                    string normalizedDeadline = deadline.ToString(
                        "R",
                        CultureInfo.InvariantCulture);
                    if (!string.Equals(
                            persistedDeadline,
                            normalizedDeadline,
                            StringComparison.Ordinal))
                    {
                        SessionState.SetString(
                            SessionServiceWaitDeadlineKey,
                            normalizedDeadline);
                    }
                    if (HasServiceWaitTimedOut(realtimeNow, deadline))
                    {
                        throw new TimeoutException(
                            "Timed out after 120 seconds waiting for " +
                            "BattleTestBootstrap-suppressed production services.");
                    }
                    return;
                }

                ProductionEntityStressRunner.StartRun(config);
                PersistActiveRun(json, config);
                WriteRunningResult(config.OutputPath);
                CompleteRequest(requestPath);
            }
            catch (Exception exception)
            {
                if (ProductionEntityStressRunner.Active != null)
                {
                    try
                    {
                        ProductionEntityStressRunner.Active.StopAndCleanup("request-failed");
                    }
                    catch (Exception cleanupException)
                    {
                        Debug.LogError(
                            "[ProductionEntityStress] Request failure cleanup also failed: " +
                            cleanupException);
                    }
                }
                ClearActiveRunRecoveryState(clearCount: true);
                ProductionEntityStressPaths.WriteTerminalResult(
                    false,
                    string.Empty,
                    exception.ToString());
                Debug.LogError($"[ProductionEntityStress] Request failed: {exception}");
                CompleteRequest(requestPath);
            }
            finally
            {
                processing = false;
            }
        }

        private static void WriteRunningResult(string reportPath)
        {
            string resultPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.ResultFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(resultPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(
                resultPath,
                "RUNNING\n" + reportPath,
                new UTF8Encoding(false));
        }

        private static void CompleteRequest(string requestPath)
        {
            try
            {
                if (File.Exists(requestPath))
                    File.Delete(requestPath);
            }
            finally
            {
                SessionState.EraseString(SessionRequestJsonKey);
                SessionState.EraseString(SessionServiceWaitDeadlineKey);
                ClearPlayRestartGuard();
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
            }
        }

        internal static void NotifyRunStopped()
        {
            ClearActiveRunRecoveryState(clearCount: true);
            ClearPlayRestartGuard();
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
        }

        private static void PersistActiveRun(
            string requestJson,
            ProductionEntityStressConfig config)
        {
            SessionState.SetString(SessionActiveRequestJsonKey, requestJson);
            SessionState.SetString(
                SessionActiveConfigJsonKey,
                BuildActiveConfigJson(requestJson, config));
            SessionState.SetBool(SessionReloadRecoveryPendingKey, false);
            SessionState.SetBool(SessionReloadRecoveryDispatchedKey, false);
            SessionState.SetBool(SessionReloadRecoveryTransitionKey, false);
        }

        private static bool ProcessReloadRecovery()
        {
            bool pending = SessionState.GetBool(SessionReloadRecoveryPendingKey, false);
            bool transition = SessionState.GetBool(
                SessionReloadRecoveryTransitionKey,
                false);
            string requestJson = SessionState.GetString(
                SessionActiveRequestJsonKey,
                string.Empty);
            string configJson = SessionState.GetString(
                SessionActiveConfigJsonKey,
                string.Empty);
            bool hasAnyActiveState = !string.IsNullOrWhiteSpace(requestJson) ||
                                     !string.IsNullOrWhiteSpace(configJson);

            if (!pending)
            {
                if (hasAnyActiveState && ProductionEntityStressRunner.Active == null && !transition)
                {
                    FailReloadRecovery(
                        "Persisted active-run state had no live runner after reload.");
                    return true;
                }
                return false;
            }

            if (transition)
                return true;

            if (!IsCompleteActiveRunState(requestJson, configJson) ||
                SessionState.GetInt(SessionReloadRecoveryCountKey, 0) !=
                ReloadRecoveryLimitForDiagnostics)
            {
                FailReloadRecovery(
                    "Reload recovery state was incomplete or exceeded its single retry contract.");
                return true;
            }

            bool dispatched = SessionState.GetBool(
                SessionReloadRecoveryDispatchedKey,
                false);
            if (dispatched)
                return !EditorApplication.isPlaying;

            ProductionEntityStressReloadRecoveryTransition action =
                ResolveReloadRecoveryTransition(
                    recoveryPending: true,
                    EditorApplication.isPlaying,
                    EditorApplication.isPlayingOrWillChangePlaymode);
            switch (action)
            {
                case ProductionEntityStressReloadRecoveryTransition.ExitPlayMode:
                    SessionState.SetBool(SessionReloadRecoveryTransitionKey, true);
                    EditorApplication.ExitPlaymode();
                    return true;
                case ProductionEntityStressReloadRecoveryTransition.EnterPlayMode:
                    PrepareReloadRecoveryRequest(requestJson);
                    SessionState.SetBool(SessionReloadRecoveryDispatchedKey, true);
                    SessionState.SetBool(SessionReloadRecoveryTransitionKey, true);
                    EditorApplication.EnterPlaymode();
                    return true;
                case ProductionEntityStressReloadRecoveryTransition.Wait:
                    return true;
                default:
                    return false;
            }
        }

        private static void PrepareReloadRecoveryRequest(string requestJson)
        {
            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(requestPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(requestPath, requestJson, new UTF8Encoding(false));
            SessionState.SetString(SessionRequestJsonKey, requestJson);
            ResetServiceWaitDeadline();
            ClearPlayRestartGuard();
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;

            string resultPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.ResultFile);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
        }

        private static void BeforeAssemblyReload()
        {
            if (SessionState.GetBool(SessionReloadRecoveryTransitionKey, false))
                return;

            string requestJson = SessionState.GetString(
                SessionActiveRequestJsonKey,
                string.Empty);
            string configJson = SessionState.GetString(
                SessionActiveConfigJsonKey,
                string.Empty);
            bool complete = IsCompleteActiveRunState(requestJson, configJson);
            ProductionEntityStressRunner runner = ProductionEntityStressRunner.Active;
            int recoveryCount = SessionState.GetInt(SessionReloadRecoveryCountKey, 0);
            ProductionEntityStressReloadRecoveryDecision decision =
                EvaluateReloadRecoveryDecision(complete, runner != null, recoveryCount);
            if (decision == ProductionEntityStressReloadRecoveryDecision.None)
                return;
            if (decision == ProductionEntityStressReloadRecoveryDecision.TerminalFailure)
            {
                FailReloadRecovery(
                    recoveryCount >= ReloadRecoveryLimitForDiagnostics
                        ? "A second assembly reload interrupted the one allowed recovery run."
                        : "Assembly reload found incomplete active-run recovery state.");
                DestroyRunnerBeforeReload(runner);
                return;
            }

            SessionState.SetInt(SessionReloadRecoveryCountKey, recoveryCount + 1);
            SessionState.SetBool(SessionReloadRecoveryPendingKey, true);
            SessionState.SetBool(SessionReloadRecoveryDispatchedKey, false);
            SessionState.SetBool(SessionReloadRecoveryTransitionKey, false);
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
            try
            {
                runner.StopAndCleanup(
                    "assembly-reload-recovery",
                    preserveRequestProcessorState: true);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[ProductionEntityStress] Best-effort assembly reload cleanup failed: " +
                    exception);
            }
            DestroyRunnerBeforeReload(runner);
            WriteReloadRecoveryPendingResult(
                TryReadActiveOutputPath(configJson),
                "Assembly reload interrupted the visible run; cleanup completed and one clean " +
                "Play Mode retry is pending.");
        }

        private static void WriteReloadRecoveryPendingResult(
            string reportPath,
            string evidence)
        {
            string resultPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.ResultFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(resultPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(
                resultPath,
                "RESTARTING\n" + (reportPath ?? string.Empty) + "\n" +
                (evidence ?? string.Empty),
                new UTF8Encoding(false));
        }

        private static void DestroyRunnerBeforeReload(ProductionEntityStressRunner runner)
        {
            if (runner == null)
                return;
            try
            {
                UnityEngine.Object.DestroyImmediate(runner.gameObject);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    "[ProductionEntityStress] Could not destroy the cleaned runner before " +
                    "assembly reload: " + exception.Message);
            }
        }

        private static void FailReloadRecovery(string reason)
        {
            string configJson = SessionState.GetString(
                SessionActiveConfigJsonKey,
                string.Empty);
            ProductionEntityStressRunner runner = ProductionEntityStressRunner.Active;
            if (runner != null)
            {
                try
                {
                    runner.StopAndCleanup(
                        "assembly-reload-terminal-failure",
                        preserveRequestProcessorState: true);
                }
                catch (Exception exception)
                {
                    reason += " Cleanup failure: " + exception.Message;
                }
            }

            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            if (File.Exists(requestPath))
                File.Delete(requestPath);
            SessionState.EraseString(SessionRequestJsonKey);
            SessionState.EraseString(SessionServiceWaitDeadlineKey);
            ClearPlayRestartGuard();
            ClearActiveRunRecoveryState(clearCount: true);
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
            ProductionEntityStressPaths.WriteTerminalResult(
                false,
                TryReadActiveOutputPath(configJson),
                reason);
            Debug.LogError("[ProductionEntityStress] " + reason);
        }

        private static string TryReadActiveOutputPath(string configJson)
        {
            try
            {
                return JsonUtility.FromJson<ActiveRunConfigState>(configJson)?.outputPath ??
                       string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode ||
                state == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetBool(SessionReloadRecoveryTransitionKey, false);
            }
        }

        private static void ClearActiveRunRecoveryState(bool clearCount)
        {
            SessionState.EraseString(SessionActiveRequestJsonKey);
            SessionState.EraseString(SessionActiveConfigJsonKey);
            SessionState.EraseBool(SessionReloadRecoveryPendingKey);
            SessionState.EraseBool(SessionReloadRecoveryDispatchedKey);
            SessionState.EraseBool(SessionReloadRecoveryTransitionKey);
            if (clearCount)
                SessionState.EraseInt(SessionReloadRecoveryCountKey);
        }

        private static ManagedRuntimeState CaptureManagedRuntimeState()
        {
            bool driverComponentPresent = HasActiveSceneComponent<SimulationTickDriver>();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            bool driverSingletonPresent = driver != null;
            bool worldPresent = driver?.World != null;
            bool poolComponentPresent = HasActiveSceneComponent<LF2ObjectPool>();
            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            bool poolSingletonPresent = pool != null;
            bool poolRuntimeStateValid = pool != null &&
                                         pool.IsRuntimeStateValidForAcceptance;
            return new ManagedRuntimeState(
                driverComponentPresent,
                driverSingletonPresent,
                worldPresent,
                poolComponentPresent,
                poolSingletonPresent,
                poolRuntimeStateValid);
        }

        private static bool HasActiveSceneComponent<T>() where T : Component
        {
            T[] components = Resources.FindObjectsOfTypeAll<T>();
            for (int index = 0; index < components.Length; index++)
            {
                T component = components[index];
                if (component != null && component.gameObject.scene.IsValid() &&
                    component.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            return false;
        }

        private static void RequestCleanPlayRestart(ManagedRuntimeState state)
        {
            int restartCount = SessionState.GetInt(SessionPlayRestartCountKey, 0) + 1;
            SessionState.SetInt(SessionPlayRestartCountKey, restartCount);
            SessionState.SetBool(SessionManagedRuntimeObservedKey, false);
            SessionState.SetBool(SessionPlayRestartPendingKey, true);
            ResetServiceWaitDeadline();
            Debug.LogWarning(
                "[ProductionEntityStress] Managed runtime invalidated during a pending request; " +
                $"exiting Play Mode for clean restart {restartCount}/" +
                $"{PlayRestartLimitForDiagnostics}. {state.Describe()}");
            EditorApplication.ExitPlaymode();
        }

        private static void ResetServiceWaitDeadline()
        {
            SessionState.SetString(
                SessionServiceWaitDeadlineKey,
                (Time.realtimeSinceStartupAsDouble + ServiceWaitTimeoutSecondsForDiagnostics)
                .ToString("R", CultureInfo.InvariantCulture));
        }

        private static void ClearPlayRestartGuard()
        {
            SessionState.EraseBool(SessionManagedRuntimeObservedKey);
            SessionState.EraseBool(SessionPlayRestartPendingKey);
            SessionState.EraseInt(SessionPlayRestartCountKey);
        }

        [Serializable]
        private sealed class ActiveRunConfigState
        {
            public int schemaVersion;
            public string requestHash;
            public string workloadFingerprint;
            public string implementationConfigFingerprint;
            public string mode;
            public string inputMode;
            public int entityCount;
            public int warmupTicks;
            public int sampleTicks;
            public bool simulationOnly;
            public bool skipLateRendererUpdate;
            public bool autoStopWhenSampled;
            public string seed;
            public string outputPath;
        }

        private readonly struct ManagedRuntimeState
        {
            internal ManagedRuntimeState(
                bool driverComponentPresent,
                bool driverSingletonPresent,
                bool worldPresent,
                bool poolComponentPresent,
                bool poolSingletonPresent,
                bool poolRuntimeStateValid)
            {
                DriverComponentPresent = driverComponentPresent;
                DriverSingletonPresent = driverSingletonPresent;
                WorldPresent = worldPresent;
                PoolComponentPresent = poolComponentPresent;
                PoolSingletonPresent = poolSingletonPresent;
                PoolRuntimeStateValid = poolRuntimeStateValid;
            }

            internal bool DriverComponentPresent { get; }
            internal bool DriverSingletonPresent { get; }
            internal bool WorldPresent { get; }
            internal bool PoolComponentPresent { get; }
            internal bool PoolSingletonPresent { get; }
            internal bool PoolRuntimeStateValid { get; }
            internal bool HasServiceFootprint =>
                DriverComponentPresent || PoolComponentPresent;
            internal bool IsValid =>
                DriverComponentPresent &&
                DriverSingletonPresent &&
                WorldPresent &&
                PoolComponentPresent &&
                PoolSingletonPresent &&
                PoolRuntimeStateValid;

            internal string Describe()
            {
                return $"driverComponent={DriverComponentPresent}, " +
                       $"driverSingleton={DriverSingletonPresent}, " +
                       $"world={WorldPresent}, poolComponent={PoolComponentPresent}, " +
                       $"poolSingleton={PoolSingletonPresent}, " +
                       $"poolRuntime={PoolRuntimeStateValid}";
            }
        }

        private static bool HasActiveBattleTestBootstrap()
        {
            BattleTestBootstrap[] bootstraps =
                Resources.FindObjectsOfTypeAll<BattleTestBootstrap>();
            for (int i = 0; i < bootstraps.Length; i++)
            {
                BattleTestBootstrap bootstrap = bootstraps[i];
                if (bootstrap != null && bootstrap.gameObject.scene.IsValid() &&
                    bootstrap.gameObject.activeInHierarchy)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ConfigureBootstrapSuppressionFromPendingRequest()
        {
            try
            {
                if (SessionState.GetBool(SessionReloadRecoveryPendingKey, false))
                {
                    BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
                    return;
                }
                string requestPath = ProductionEntityStressPaths.ProjectPath(
                    ProductionEntityStressPaths.RequestFile);
                if (!File.Exists(requestPath))
                    return;
                ProductionEntityStressRequest request =
                    JsonUtility.FromJson<ProductionEntityStressRequest>(
                        File.ReadAllText(requestPath, Encoding.UTF8));
                BattleTestBootstrap.SuppressEntityCreationForProductionStress =
                    ShouldSuppressBattleTestBootstrap(request?.action);
            }
            catch (Exception exception)
            {
                Debug.LogWarning(
                    $"[ProductionEntityStress] Could not initialize bootstrap suppression: {exception.Message}");
            }
        }
    }
}
#endif
