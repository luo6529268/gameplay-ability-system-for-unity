#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using DiagnosticsProcess = System.Diagnostics.Process;
using DiagnosticsProcessStartInfo = System.Diagnostics.ProcessStartInfo;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingBenchmarkWindow : EditorWindow
    {
        private string backend = nameof(BattlePresentationBackendMode.CentralOnly);
        private string comparison = "ab";
        private int warmupFrames = 30;
        private int sampleFrames = 120;
        private int leakCheckFrames = 600;
        private long maxManagedGrowthBytes = 1048576L;
        private long maxGraphicsGrowthBytes = 4194304L;
        private string targetActiveEntities = "current-scene";
        private string outputPath = "Temp/NTSD_BattleRenderingBenchmark.json";
        private string status = "Write a request while Play Mode has an active battle world.";

        [MenuItem("NTSD/Battle Rendering/Benchmark")]
        public static void Open()
        {
            GetWindow<BattleRenderingBenchmarkWindow>("Battle Rendering Benchmark");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Explicit Benchmark Request", EditorStyles.boldLabel);
            backend = EditorGUILayout.TextField("Backend", backend);
            comparison = EditorGUILayout.TextField("Comparison", comparison);
            warmupFrames = EditorGUILayout.IntField("Warmup Frames", warmupFrames);
            sampleFrames = EditorGUILayout.IntField("Sample Frames", sampleFrames);
            leakCheckFrames = EditorGUILayout.IntField("Leak Check Frames", leakCheckFrames);
            maxManagedGrowthBytes = EditorGUILayout.LongField("Max Managed Growth", maxManagedGrowthBytes);
            maxGraphicsGrowthBytes = EditorGUILayout.LongField("Max Graphics Growth", maxGraphicsGrowthBytes);
            targetActiveEntities = EditorGUILayout.TextField("Target Active Entities", targetActiveEntities);
            outputPath = EditorGUILayout.TextField("Output Path", outputPath);
            EditorGUILayout.HelpBox(status, MessageType.Info);
            if (GUILayout.Button("Write Request File"))
            {
                try
                {
                    BattleRenderingBenchmarkRequest request = new BattleRenderingBenchmarkRequest
                    {
                        backend = backend,
                        comparison = comparison,
                        warmupFrames = warmupFrames,
                        sampleFrames = sampleFrames,
                        leakCheckFrames = leakCheckFrames,
                        maxManagedGrowthBytes = maxManagedGrowthBytes,
                        maxGraphicsGrowthBytes = maxGraphicsGrowthBytes,
                        targetActiveEntities = targetActiveEntities,
                        outputPath = outputPath,
                    };
                    BattleRenderingBenchmarkRequestProcessor.WriteRequest(request);
                    status = "Request written. The Editor processor will start it on the next update.";
                }
                catch (Exception ex)
                {
                    status = ex.Message;
                    Debug.LogError($"[BattleRenderingBenchmark] Request write failed: {ex}");
                }
            }
        }
    }

    [InitializeOnLoad]
    internal static class BattleRenderingBenchmarkRequestProcessor
    {
        internal const string RequestFile = "Temp/NTSD_BattleRenderingBenchmark.request.json";
        internal const string ResultFile = "Temp/NTSD_BattleRenderingBenchmark.result";
        internal const string DefaultOutputFile = "Temp/NTSD_BattleRenderingBenchmark.json";

        private static BattleRenderingBenchmarkRunner runner;
        private static bool requestInProgress;

        static BattleRenderingBenchmarkRequestProcessor()
        {
            EditorApplication.update += PollRequest;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            EditorApplication.quitting += Cleanup;
        }

        internal static string ProjectPath(string path)
        {
            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));
        }

        internal static void WriteRequest(BattleRenderingBenchmarkRequest request)
        {
            BattleRenderingBenchmarkConfig.FromRequest(request);
            string path = ProjectPath(RequestFile);
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            File.WriteAllText(path, JsonUtility.ToJson(request), new UTF8Encoding(false));
        }

        private static void PollRequest()
        {
            PollRequest(Application.isPlaying);
        }

        internal static void PollRequestForTests(bool isPlaying)
        {
            PollRequest(isPlaying);
        }

        internal static bool HasRunnerForTests => runner != null;

        internal static void SetRunnerForTests(BattleRenderingBenchmarkRunner testRunner)
        {
            runner = testRunner;
        }

        internal static void HandlePlayModeStateChangeForTests(PlayModeStateChange state)
        {
            OnPlayModeStateChanged(state);
        }

        private static void PollRequest(bool isPlaying)
        {
            if (!isPlaying)
            {
                AbortCurrentRunner("Benchmark aborted because the Editor is not in Play Mode.");
                return;
            }

            if (requestInProgress || runner != null)
                return;

            string requestPath = ProjectPath(RequestFile);
            if (!File.Exists(requestPath))
                return;

            requestInProgress = true;
            try
            {
                ProcessRequest(requestPath);
            }
            finally
            {
                TryDelete(requestPath);
                requestInProgress = false;
            }
        }

        private static void ProcessRequest(string requestPath)
        {
            string resultPath = ProjectPath(ResultFile);
            try
            {
                BattleRenderingBenchmarkRequest request =
                    JsonUtility.FromJson<BattleRenderingBenchmarkRequest>(File.ReadAllText(requestPath, Encoding.UTF8));
                BattleRenderingBenchmarkConfig config = BattleRenderingBenchmarkConfig.FromRequest(request);
                SimulationWorld world = SimulationTickDriver.Instance?.World;
                if (world == null && config.Scenario.UsesCurrentScene)
                    throw new InvalidOperationException(
                        "No active SimulationWorld exists. Enter Play Mode and start a battle first.");
                if (world == null)
                    world = new SimulationWorld();

                string outputPath = string.IsNullOrWhiteSpace(config.OutputPath)
                    ? ProjectPath(DefaultOutputFile)
                    : ProjectPath(config.OutputPath);
                runner = BattleRenderingBenchmarkRunner.Start(
                    config,
                    world,
                    outputPath,
                    Complete);
                WriteResult(resultPath, "RUNNING\n" + outputPath);
                Debug.Log($"[BattleRenderingBenchmark] Started: {outputPath}");
            }
            catch (Exception ex)
            {
                if (runner != null)
                {
                    AbortCurrentRunner(ex.ToString());
                }
                else
                {
                    WriteResult(resultPath, "FAIL\n" + ex);
                }
                Debug.LogError($"[BattleRenderingBenchmark] Request failed: {ex}");
            }
        }

        private static void Complete(BattleRenderingBenchmarkRunner completedRunner, string result)
        {
            if (runner != completedRunner)
                return;

            runner = null;
            WriteResult(ProjectPath(ResultFile), result);
        }

        private static void WriteResult(string path, string content)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ?? ProjectPath("Temp"));
            File.WriteAllText(path, content ?? string.Empty, new UTF8Encoding(false));
        }

        private static void Cleanup()
        {
            AbortCurrentRunner("Benchmark stopped during Editor shutdown or domain reload.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                AbortCurrentRunner("Benchmark aborted because the Editor is exiting Play Mode.");
            }
        }

        private static void AbortCurrentRunner(string reason)
        {
            BattleRenderingBenchmarkRunner activeRunner = runner;
            if (activeRunner == null)
                return;

            // Abort synchronously invokes Complete; only clear a runner if that callback did not.
            activeRunner.Abort(reason);
            if (runner == activeRunner)
                runner = null;
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[BattleRenderingBenchmark] Failed to delete request: {ex.Message}");
            }
        }
    }

    internal static class BattleRenderingBenchmarkPlayerBuild
    {
        internal const string PlayerDirectory = "Temp/P8-D-StandalonePlayer";
        internal const string PlayerExecutable = "NTSD-BattleRenderingBenchmark.exe";

        [MenuItem("NTSD/Battle Rendering/Build Standalone Benchmark Player")]
        internal static void BuildWindows64Player()
        {
            string executable = BuildWindows64PlayerInternal();
            UnityEngine.Debug.Log("[BattleRenderingBenchmark] Standalone Player built: " + executable);
        }

        [MenuItem("NTSD/Battle Rendering/Build And Run Standalone Benchmark Matrix")]
        internal static void BuildAndRunWindows64Matrix()
        {
            string executable = BuildWindows64PlayerInternal();
            string[] scenarios = { "100", "300", "500", "1000" };
            for (int index = 0; index < scenarios.Length; index++)
            {
                string scenario = scenarios[index];
                string output = BattleRenderingBenchmarkRequestProcessor.ProjectPath(
                    $"Temp/P8-D-runtime-{scenario}-player-ab-v5.json");
                string log = BattleRenderingBenchmarkRequestProcessor.ProjectPath(
                    $"Temp/P8-D-runtime-{scenario}-player-ab-v5.log");
                var startInfo = new DiagnosticsProcessStartInfo
                {
                    FileName = executable,
                    Arguments = BuildArguments(scenario, output, log),
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(executable) ??
                                       BattleRenderingBenchmarkRequestProcessor.ProjectPath(PlayerDirectory),
                };
                using DiagnosticsProcess process = DiagnosticsProcess.Start(startInfo);
                if (process == null)
                    throw new InvalidOperationException("Failed to start Standalone benchmark Player.");
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException(
                        $"Standalone benchmark scenario {scenario} failed with exit code {process.ExitCode}. " +
                        $"See {log}.");
                }
            }
            UnityEngine.Debug.Log(
                "[BattleRenderingBenchmark] Standalone 100/300/500/1000 A/B matrix completed.");
        }

        private static string BuildWindows64PlayerInternal()
        {
            var scenes = new List<string>();
            EditorBuildSettingsScene[] configuredScenes = EditorBuildSettings.scenes;
            for (int index = 0; index < configuredScenes.Length; index++)
            {
                if (configuredScenes[index].enabled && !string.IsNullOrWhiteSpace(configuredScenes[index].path))
                    scenes.Add(configuredScenes[index].path);
            }
            if (scenes.Count == 0)
                throw new InvalidOperationException("No enabled EditorBuildSettings scene is available for the Player harness.");

            string directory = BattleRenderingBenchmarkRequestProcessor.ProjectPath(PlayerDirectory);
            Directory.CreateDirectory(directory);
            string executable = Path.Combine(directory, PlayerExecutable);
            bool previousFrameTimingStats = PlayerSettings.enableFrameTimingStats;
            try
            {
                PlayerSettings.enableFrameTimingStats = true;
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes.ToArray(),
                    locationPathName = executable,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development,
                });
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Standalone benchmark Player build failed: {report.summary.result}, " +
                        $"errors={report.summary.totalErrors}.");
                }
                return executable;
            }
            finally
            {
                PlayerSettings.enableFrameTimingStats = previousFrameTimingStats;
            }
        }

        internal static string BuildArguments(string scenario, string output, string log)
        {
            BattleRenderingBenchmarkConfig config = BattleRenderingBenchmarkConfig.Default;
            return string.Join(" ", new[]
            {
                "-logFile", Quote(log),
                BattleRenderingBenchmarkPlayerArguments.EnableArgument,
                BattleRenderingBenchmarkPlayerArguments.ScenarioArgument, scenario,
                BattleRenderingBenchmarkPlayerArguments.ComparisonArgument, "ab",
                BattleRenderingBenchmarkPlayerArguments.WarmupArgument, config.WarmupFrames.ToString(),
                BattleRenderingBenchmarkPlayerArguments.SampleArgument, config.SampleFrames.ToString(),
                BattleRenderingBenchmarkPlayerArguments.LeakArgument, config.LeakCheckFrames.ToString(),
                BattleRenderingBenchmarkPlayerArguments.OutputArgument, Quote(output),
            });
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
        }
    }

}
#endif
