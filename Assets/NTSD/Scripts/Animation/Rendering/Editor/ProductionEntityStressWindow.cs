#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using NTSD.Test;
using UnityEditor;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class ProductionEntityStressWindow : EditorWindow
    {
        private int warmupTicks = 30;
        private int sampleTicks = 300;
        private int spawnBatchSize = 25;
        private int maxCatchUpTicksPerFrame = 4;
        private int maxBacklogTicks = 8;
        private int formalCollectorModeIndex;
        private string outputPath = "Temp/NTSD_ProductionEntityStress.dispersed.json";
        private string status = "Ready. A start request enters Play Mode and leaves the 1000-entity run visible until cleanup.";
        private static readonly string[] FormalCollectorModes =
        {
            "configured",
            "legacy",
            "role",
            "brute",
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
            formalCollectorModeIndex = EditorGUILayout.Popup(
                "Formal Collector",
                formalCollectorModeIndex,
                FormalCollectorModes);
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

        private static ProductionEntityStressRequest CreateDefaultRequest(
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
                formalCollectorMode = "configured",
                outputPath = reportPath,
            };
        }
    }

    [InitializeOnLoad]
    internal static class ProductionEntityStressRequestProcessor
    {
        private const string SessionRequestJsonKey =
            "NTSD.ProductionEntityStress.RequestJson";
        private const string SessionServiceWaitFramesKey =
            "NTSD.ProductionEntityStress.ServiceWaitFrames";
        private const int MaximumServiceWaitFrames = 1800;
        private static bool processing;

        static ProductionEntityStressRequestProcessor()
        {
            ConfigureBootstrapSuppressionFromPendingRequest();
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

        internal static void PollRequestForTests()
        {
            PollRequest();
        }

        private static void WriteRequestJson(string json)
        {
            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            Directory.CreateDirectory(
                Path.GetDirectoryName(requestPath) ?? ProductionEntityStressPaths.ProjectPath("Temp"));
            File.WriteAllText(requestPath, json, new UTF8Encoding(false));
            SessionState.SetString(SessionRequestJsonKey, json);
            SessionState.SetInt(SessionServiceWaitFramesKey, 0);
            ProductionEntityStressRequest request =
                JsonUtility.FromJson<ProductionEntityStressRequest>(json);
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
            string requestPath = ProductionEntityStressPaths.ProjectPath(
                ProductionEntityStressPaths.RequestFile);
            if (!File.Exists(requestPath))
            {
                SessionState.EraseString(SessionRequestJsonKey);
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
                    CompleteRequest(requestPath);
                    return;
                }

                ProductionEntityStressConfig config = ProductionEntityStressConfig.FromRequest(
                    request,
                    ProductionEntityStressPaths.ProjectRoot);
                if (ShouldEnterPlayMode(action, EditorApplication.isPlaying))
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        EditorApplication.EnterPlaymode();
                    return;
                }
                if (!EditorApplication.isPlaying)
                    return;
                if (ProductionEntityStressRunner.Active != null)
                    throw new InvalidOperationException(
                        "Stop the active production stress run before starting another one.");

                bool hasActiveBootstrap = HasActiveBattleTestBootstrap();
                bool servicesReady = ProductionEntityStressRunner.AreProductionServicesReady();
                if (!IsReadyToStart(
                        hasActiveBootstrap,
                        BattleTestBootstrap.ProductionStressServicesReady,
                        servicesReady))
                {
                    int waitFrames = SessionState.GetInt(SessionServiceWaitFramesKey, 0) + 1;
                    SessionState.SetInt(SessionServiceWaitFramesKey, waitFrames);
                    if (waitFrames >= MaximumServiceWaitFrames)
                    {
                        throw new TimeoutException(
                            "Timed out waiting for BattleTestBootstrap-suppressed production services.");
                    }
                    return;
                }

                ProductionEntityStressRunner.StartRun(config);
                WriteRunningResult(config.OutputPath);
                CompleteRequest(requestPath);
            }
            catch (Exception exception)
            {
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
                SessionState.EraseInt(SessionServiceWaitFramesKey);
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
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
