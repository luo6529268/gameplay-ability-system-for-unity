---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkWindow.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkEditorTests.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs"
  - "Assets/NTSD/Docs/central-battle-render-system-plan.md"
  - "Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md"
  - "Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md"
timestamp: "2026-07-24T07:16:15.922Z"
---

--- File: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkWindow.cs ---
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


--- File: Assets/NTSD/Scripts/Animation/Rendering/Editor/BattleRenderingBenchmarkEditorTests.cs ---
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEditor;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleRenderingBenchmarkEditorTests
    {
        [Test]
        public void RequestConfig_ParsesOnlyExplicitCapacityScenarios()
        {
            var request = new BattleRenderingBenchmarkRequest
            {
                backend = "centralonly",
                comparison = "ab",
                warmupFrames = 2,
                sampleFrames = 3,
                leakCheckFrames = 4,
                maxManagedGrowthBytes = 1024,
                maxGraphicsGrowthBytes = 2048,
                targetActiveEntities = " 300 ",
                outputPath = "Temp/test-benchmark.json",
            };

            BattleRenderingBenchmarkConfig config =
                BattleRenderingBenchmarkConfig.FromRequest(request);

            Assert.That(config.Backend, Is.EqualTo(BattlePresentationBackendMode.CentralOnly));
            Assert.That(config.Comparison, Is.EqualTo(BattleRenderingBenchmarkComparison.CentralLegacyAB));
            Assert.That(config.WarmupFrames, Is.EqualTo(2));
            Assert.That(config.SampleFrames, Is.EqualTo(3));
            Assert.That(config.LeakCheckFrames, Is.EqualTo(4));
            Assert.That(config.TargetActiveEntities, Is.EqualTo("300"));
            Assert.That(config.Scenario.RequestedEntityCount, Is.EqualTo(300));
            Assert.That(config.OutputPath, Is.EqualTo("Temp/test-benchmark.json"));
            Assert.Throws<System.ArgumentException>(() =>
                BattleRenderingBenchmarkScenario.Parse("24 actors"));
            Assert.Throws<System.ArgumentException>(() =>
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralShadowBuild,
                    0,
                    1,
                    "100",
                    string.Empty));
            Assert.Throws<System.ArgumentOutOfRangeException>(() =>
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    0,
                    "100",
                    string.Empty));
        }

        [TestCase("100", 100, 200)]
        [TestCase("300", 300, 600)]
        [TestCase("500", 500, 1000)]
        [TestCase("1000", 1000, 2000)]
        public void DeterministicRuntimeWorkload_BuildsExactRequestedCount(
            string scenarioText,
            int expectedEntities,
            int expectedCommands)
        {
            BattleRenderingBenchmarkScenario scenario =
                BattleRenderingBenchmarkScenario.Parse(scenarioText);

            BattleRenderingBenchmarkWorkload first =
                BattleRenderingBenchmarkWorkload.Create(scenario, null, 1, 2);
            BattleRenderingBenchmarkWorkload second =
                BattleRenderingBenchmarkWorkload.Create(scenario, null, 1, 2);

            Assert.That(first.RequestedEntityCount, Is.EqualTo(expectedEntities));
            Assert.That(first.ActualEntityCount, Is.EqualTo(expectedEntities));
            Assert.That(first.FrozenFrame.EntityCount, Is.EqualTo(expectedEntities));
            Assert.That(first.CommandCount, Is.EqualTo(expectedCommands));
            Assert.That(first.RuntimeObjectCount, Is.EqualTo(expectedEntities));
            Assert.That(first.RuntimeProfile, Is.EqualTo(BattleRuntimeProfile.MobileExtended.ToString()));
            Assert.That(first.RuntimeSlotCapacity, Is.EqualTo(1050));
            Assert.That(first.WarmupTickCount, Is.EqualTo(1));
            Assert.That(first.WarmupLogicTickSamples.Count, Is.EqualTo(1));
            Assert.That(first.WarmupLogicTickSamples[0].ElapsedMilliseconds.Available, Is.True);
            Assert.That(first.WarmupLogicTickSamples[0].AllocatedBytes.Available, Is.True);
            Assert.That(first.WarmupLogicTickSamples[0].Checksum, Is.Not.Empty);
            Assert.That(first.SampleTickCount, Is.EqualTo(2));
            Assert.That(first.LogicTickSamples.Count, Is.EqualTo(2));
            Assert.That(first.LogicTickSamples[0].ElapsedMilliseconds.Available, Is.True);
            Assert.That(first.LogicTickSamples[0].AllocatedBytes.Available, Is.True);
            Assert.That(first.InputFingerprint, Is.Not.Empty);
            Assert.That(first.InitialRuntimeChecksum, Is.Not.Empty);
            Assert.That(first.FinalRuntimeChecksum, Is.Not.Empty);
            Assert.That(first.RuntimeStateDeterministic, Is.True);
            Assert.That(first.Fingerprint, Is.EqualTo(second.Fingerprint));
            Assert.That(first.InputFingerprint, Is.EqualTo(second.InputFingerprint));
            Assert.That(first.InitialRuntimeChecksum, Is.EqualTo(second.InitialRuntimeChecksum));
            Assert.That(first.FinalRuntimeChecksum, Is.EqualTo(second.FinalRuntimeChecksum));
        }

        [Test]
        public void DeterministicRuntimeWorkload_AdmitsExactlyOneThousandRealEntities()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("1000"),
                    null,
                    0,
                    1);

            Assert.That(workload.RuntimeObjectCount, Is.EqualTo(1000));
            Assert.That(workload.ActualEntityCount, Is.EqualTo(1000));
            Assert.That(workload.RuntimeAdmissionValidated, Is.True);
            Assert.That(workload.RuntimeSlotCapacity, Is.EqualTo(1050));
        }

        [Test]
        public void CurrentSceneWorkload_RejectsMissingOrEmptyPublishedFrame()
        {
            BattleRenderingBenchmarkScenario scenario =
                BattleRenderingBenchmarkScenario.Parse("current-scene");

            Assert.Throws<System.InvalidOperationException>(() =>
                BattleRenderingBenchmarkWorkload.Create(scenario, new SimulationWorld()));
        }

        [Test]
        public void SessionReport_ValidatesNonEmptyCentralWorkload_AndDisposesRecorders()
        {
            var world = new SimulationWorld();
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                0,
                1,
                "100",
                string.Empty);
            var session = new BattleRenderingBenchmarkSession(config, world);
            try
            {
                int guard = 0;
                while (!session.CaptureFrame() && guard++ < 4)
                {
                }
                Assert.That(session.IsComplete, Is.True);
                string first = session.Report.ToJson();
                string second = session.Report.ToJson();
                Assert.That(second, Is.EqualTo(first));
                Assert.That(session.Report.RequestedPresentationEntityCount, Is.EqualTo(100));
                Assert.That(session.Report.ActualPresentationEntityCount, Is.EqualTo(100));
                Assert.That(session.Report.CommandCount, Is.EqualTo(200));
                Assert.That(session.Report.RendererWorkloadValidated, Is.True);
                Assert.That(session.Report.RuntimeAdmissionValidated, Is.True);
                Assert.That(session.Report.LogicTickMetricsValidated, Is.True);
                Assert.That(session.Report.DeterminismValidated, Is.True);
                Assert.That(session.Report.Passed, Is.False);
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Unsupported));
                Assert.That(session.Report.RuntimeObjectCount, Is.EqualTo(100));
                Assert.That(session.Report.RuntimeProfile, Is.EqualTo("MobileExtended"));
                Assert.That(session.Report.Frames[0].LogicTickTimeMs.Available, Is.True);
                Assert.That(session.Report.Frames[0].LogicTickAllocatedBytes.Available, Is.True);
                Assert.That(
                    session.Report.Frames[0].PresenterSubmissionDrawCalls.Available,
                    Is.False,
                    "EditMode builds mesh segments but never calls Graphics.DrawMesh");
                BattleBenchmarkMetricAvailability submissionAvailability =
                    FindMetricAvailability(session.Report, "presenterSubmissionDrawCalls");
                Assert.That(submissionAvailability.Available, Is.False);
                StringAssert.Contains("not in Play Mode", submissionAvailability.Reason);
                Assert.That(
                    submissionAvailability.Source,
                    Is.EqualTo("Graphics.DrawMesh calls issued by the central presenter"));
                Assert.That(session.Report.BenchmarkRenderTargetWidth, Is.EqualTo(256));
                Assert.That(session.Report.BenchmarkRenderTargetHeight, Is.EqualTo(256));
                StringAssert.Contains("ntsd-battle-rendering-benchmark-run-v5", first);
                StringAssert.Contains("\"verdict\":\"Unsupported\"", first);
                StringAssert.Contains("metricAvailability", first);
                StringAssert.Contains("missingRequiredMetrics", first);
                StringAssert.Contains("benchmarkRenderTargetWidth\":256", first);
            }
            finally
            {
                session.Dispose();
            }

            Assert.That(session.IsDisposed, Is.True);
        }

        [Test]
        public void SubmissionPolicy_ReportsOnlyActualPlayDrawMeshCalls()
        {
            Assert.That(
                BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(false, 3),
                Is.EqualTo(BattleRenderingBenchmarkSubmissionPolicy.Unavailable));
            Assert.That(
                BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(true, 3),
                Is.EqualTo(3));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(true, 0));
        }

        [Test]
        public void CompletedFrameAttribution_SnapshotsCountersOnce_AndRejectsStaleGenerations()
        {
            var attribution = new BattleBenchmarkCompletedFrameAttribution();
            attribution.Request(10, 100, 500UL);

            Assert.That(attribution.ShouldSnapshotCounters(9, 101), Is.False);
            Assert.That(attribution.ShouldSnapshotCounters(10, 100), Is.False);
            Assert.That(attribution.ShouldSnapshotCounters(10, 101), Is.True);
            Assert.That(attribution.ShouldSnapshotCounters(10, 102), Is.False);
            Assert.That(attribution.TryAcceptTiming(9, 501UL), Is.False);
            Assert.That(attribution.TryAcceptTiming(10, 500UL), Is.False);
            Assert.That(attribution.TryAcceptTiming(10, 501UL), Is.True);

            attribution.Request(11, 200, 400UL);
            Assert.That(attribution.ShouldSnapshotCounters(11, 201), Is.True);
            Assert.That(attribution.TryAcceptTiming(11, 501UL), Is.False,
                "the previous accepted frame timestamp is also a monotonic watermark");
            Assert.That(attribution.TryAcceptTiming(11, 502UL), Is.True);
        }

        [Test]
        public void PositiveCounterPolicy_RejectsZeroCompletedFrameSample()
        {
            BattleBenchmarkMetric zero = BattleBenchmarkCounterSamplePolicy.Capture(
                true,
                1,
                0L,
                "bytes",
                true,
                out string zeroReason);
            BattleBenchmarkMetric missing = BattleBenchmarkCounterSamplePolicy.Capture(
                true,
                0,
                1024L,
                "bytes",
                true,
                out string missingReason);
            BattleBenchmarkMetric positive = BattleBenchmarkCounterSamplePolicy.Capture(
                true,
                1,
                1024L,
                "bytes",
                true,
                out string positiveReason);

            Assert.That(zero.Available, Is.False);
            StringAssert.Contains("non-empty benchmark render workload", zeroReason);
            Assert.That(missing.Available, Is.False);
            StringAssert.Contains("no completed-frame sample", missingReason);
            Assert.That(positive.Available, Is.True);
            Assert.That(positive.Value, Is.EqualTo(1024L));
            Assert.That(positiveReason, Is.Empty);
        }

        [Test]
        public void DrawCallPolicy_RejectsZeroForNonEmptyBenchmarkWorkload()
        {
            BattleBenchmarkMetric zero = BattleBenchmarkDrawCallPolicy.RequirePositiveForNonEmptyWorkload(
                BattleBenchmarkMetric.FromValue(0, "count"));
            BattleBenchmarkMetric positive = BattleBenchmarkDrawCallPolicy.RequirePositiveForNonEmptyWorkload(
                BattleBenchmarkMetric.FromValue(3, "count"));

            Assert.That(zero.Available, Is.False);
            Assert.That(positive.Available, Is.True);
            Assert.That(positive.Value, Is.EqualTo(3));
        }

        [Test]
        public void BenchmarkOwnedTextureMemoryPolicy_RequiresGenerationOwnedTexturesAndPositiveBytes()
        {
            BattleBenchmarkMetric missingGeneration = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                0,
                2,
                1024L,
                out string missingGenerationReason);
            BattleBenchmarkMetric missingTextures = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                7,
                0,
                1024L,
                out string missingTexturesReason);
            BattleBenchmarkMetric zeroBytes = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                7,
                2,
                0L,
                out string zeroBytesReason);
            BattleBenchmarkMetric positive = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                7,
                2,
                1024L,
                out string positiveReason);

            Assert.That(missingGeneration.Available, Is.False);
            StringAssert.Contains("generation", missingGenerationReason);
            Assert.That(missingTextures.Available, Is.False);
            StringAssert.Contains("owns no Texture2D", missingTexturesReason);
            Assert.That(zeroBytes.Available, Is.False);
            StringAssert.Contains("no positive bytes", zeroBytesReason);
            Assert.That(positive.Available, Is.True);
            Assert.That(positive.Value, Is.EqualTo(1024L));
            Assert.That(positiveReason, Is.Empty);
        }

        [Test]
        public void ABSuite_UsesOneFrozenWorkload_ForCentralAndRendererlessLegacy()
        {
            var world = new SimulationWorld();
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.CentralLegacyAB,
                0,
                1,
                0,
                1024,
                1024,
                "100",
                string.Empty);
            var suite = new BattleRenderingBenchmarkSuiteSession(config, world);
            try
            {
                int guard = 0;
                while (!suite.CaptureFrame() && guard++ < 4)
                {
                }

                Assert.That(suite.IsComplete, Is.True);
                Assert.That(suite.Report.Runs.Count, Is.EqualTo(2));
                Assert.That(
                    suite.Report.Runs[0].Config.Backend,
                    Is.EqualTo(BattlePresentationBackendMode.CentralOnly));
                Assert.That(
                    suite.Report.Runs[1].Config.Backend,
                    Is.EqualTo(BattlePresentationBackendMode.LegacyOnly));
                Assert.That(
                    suite.Report.Runs[0].WorkloadFingerprint,
                    Is.EqualTo(suite.Report.Runs[1].WorkloadFingerprint));
                Assert.That(
                    suite.Report.Runs[0].FinalRuntimeChecksum,
                    Is.EqualTo(suite.Report.Runs[1].FinalRuntimeChecksum));
                Assert.That(
                    suite.Report.Runs[0].InputFingerprint,
                    Is.EqualTo(suite.Report.Runs[1].InputFingerprint));
                Assert.That(suite.Report.Runs[0].RendererWorkloadValidated, Is.True);
                Assert.That(suite.Report.Runs[1].RendererWorkloadValidated, Is.True);
                Assert.That(suite.Report.Runs[1].CommandCount, Is.EqualTo(200));
                Assert.That(suite.Report.Passed, Is.False);
                Assert.That(suite.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Unsupported));
                Assert.That(
                    FindMetricAvailability(suite.Report.Runs[0], "presenterSubmissionDrawCalls").Required,
                    Is.True);
                Assert.That(
                    FindMetricAvailability(suite.Report.Runs[1], "presenterSubmissionDrawCalls").Applicability,
                    Is.EqualTo(BattleBenchmarkMetricApplicability.NotApplicable));
                Assert.That(
                    FindMetricAvailability(suite.Report.Runs[1], "drawCalls").Required,
                    Is.True);
                Assert.That(
                    suite.Report.Runs[0].BenchmarkRenderTargetWidth,
                    Is.EqualTo(suite.Report.Runs[1].BenchmarkRenderTargetWidth));
                Assert.That(
                    suite.Report.Runs[0].BenchmarkRenderTargetHeight,
                    Is.EqualTo(suite.Report.Runs[1].BenchmarkRenderTargetHeight));
                StringAssert.Contains(
                    "BenchmarkRendererlessLegacyCompatibilityPresenter",
                    suite.Report.ToJson());
            }
            finally
            {
                suite.Dispose();
            }
            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.LegacyOnly));
        }

        [Test]
        public void ABSuite_ExplicitDispose_RestoresPreviousBackend()
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralShadowBuild);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.CentralLegacyAB,
                0,
                1,
                0,
                1024,
                1024,
                "100",
                string.Empty);
            var suite = new BattleRenderingBenchmarkSuiteSession(config, world);

            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.CentralOnly));
            suite.Dispose();

            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.CentralShadowBuild));
        }

        [Test]
        public void ABSuite_PresenterCaptureFailure_DisposesAndRestoresPreviousBackend()
        {
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.LegacyOnly);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                0,
                1024,
                1024,
                "100",
                string.Empty);
            ThrowingPresenter presenter = null;
            var suite = new BattleRenderingBenchmarkSuiteSession(
                config,
                world,
                (runConfig, runWorld, workload) =>
                {
                    presenter = new ThrowingPresenter(workload);
                    return new BattleRenderingBenchmarkSession(
                        runConfig,
                        runWorld,
                        workload,
                        SupportedPolicyContext(),
                        new BattleBenchmarkInjectedCompletedFrameCollector(
                            CreateAvailableCompletedFrame(7)),
                        presenter);
                });

            Assert.Throws<InvalidOperationException>(() => suite.CaptureFrame());
            Assert.That(presenter.Disposed, Is.True);
            Assert.That(world.BattlePresentation.Mode, Is.EqualTo(BattlePresentationBackendMode.LegacyOnly));
            Assert.Throws<ObjectDisposedException>(() => suite.CaptureFrame());
        }

        [Test]
        public void PassPolicy_RejectsMissingRequiredLocalMetrics()
        {
            Assert.That(
                BattleRenderingBenchmarkPassPolicy.Evaluate(
                    countValidated: true,
                    runtimeAdmissionValidated: true,
                    logicTickMetricsValidated: false,
                    determinismValidated: true,
                    rendererWorkloadValidated: true,
                    leakRequested: false,
                    leakPassed: false),
                Is.False);
            Assert.That(
                BattleRenderingBenchmarkPassPolicy.Evaluate(
                    countValidated: true,
                    runtimeAdmissionValidated: true,
                    logicTickMetricsValidated: true,
                    determinismValidated: true,
                    rendererWorkloadValidated: true,
                    leakRequested: false,
                    leakPassed: false),
                Is.True);
        }

        [Test]
        public void V5InjectedCompletedFrames_RequireExactCount_AndKeepDrawScopesDistinct()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    2);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                0,
                2,
                "100",
                string.Empty);
            var context = new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                false,
                true);
            var collector = new BattleBenchmarkInjectedCompletedFrameCollector(
                CreateAvailableCompletedFrame(drawCalls: 123));
            var presenter = new InjectedPresenter(workload, central: true, submissionDrawCalls: 7);
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                context,
                collector,
                presenter);
            try
            {
                int guard = 0;
                while (!session.CaptureFrame() && guard++ < 8)
                {
                }

                Assert.That(session.IsComplete, Is.True);
                Assert.That(session.Report.Frames.Count, Is.EqualTo(2));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
                Assert.That(session.Report.Frames[0].DrawCalls.Value, Is.EqualTo(123));
                Assert.That(session.Report.Frames[0].BenchmarkOwnedTextureMemoryBytes.Value, Is.EqualTo(128));
                Assert.That(session.Report.Frames[0].BenchmarkResourceGeneration, Is.EqualTo(7));
                BattleBenchmarkMetricAvailability textureAvailability =
                    FindMetricAvailability(session.Report, "benchmarkOwnedTextureMemoryBytes");
                Assert.That(textureAvailability.Scope, Is.EqualTo("benchmark-owned-textures"));
                StringAssert.Contains("generation 7", textureAvailability.Source);
                Assert.That(session.Report.Frames[0].PresenterSubmissionDrawCalls.Value, Is.EqualTo(7));
                Assert.That(
                    FindMetricAvailability(session.Report, "exactSampleCount").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Passed));
                Assert.That(
                    FindMetricAvailability(session.Report, "renderThreadTimeMs").Applicability,
                    Is.EqualTo(BattleBenchmarkMetricApplicability.NotApplicable));
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void V5LegacyLocalDrawAndMeshMetricsAreNotApplicable_ButActualDrawsAreRequired()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.LegacyOnly,
                0,
                1,
                "100",
                string.Empty);
            var context = new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                true,
                true);
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                context,
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(drawCalls: 44)),
                new InjectedPresenter(workload, central: false, submissionDrawCalls: -1));
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.True);
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
                Assert.That(FindMetricAvailability(session.Report, "drawCalls").Required, Is.True);
                Assert.That(
                    FindMetricAvailability(session.Report, "presenterSubmissionDrawCalls").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.NotApplicable));
                Assert.That(
                    FindMetricAvailability(session.Report, "meshChunks").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.NotApplicable));
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void V5FixedDrawingWorkload_RejectsZeroDrawCallsAsMissingEvidence()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    1,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(drawCalls: 0)),
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(
                        ++guard,
                        Is.LessThan(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts * 3));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
                Assert.That(
                    session.Report.CompletedFrameRejectedAttemptCount,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                Assert.That(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts, Is.EqualTo(16));
                Assert.That(
                    FindMetricAvailability(session.Report, "drawCalls").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Missing));
                Assert.That(session.Report.MissingRequiredMetrics, Does.Contain("drawCalls"));
                StringAssert.Contains(
                    "required applicable metrics unavailable: drawCalls",
                    session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void CompletedFrameSampling_RetriesOneInvalidAttempt_AndStillCommitsExactly120Samples()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var collector = new SequencedCompletedFrameCollector(
                CreateUnavailableGpuCompletedFrame(drawCalls: 7),
                CreateAvailableCompletedFrame(drawCalls: 7));
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    120,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                collector,
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(++guard, Is.LessThan(300));

                Assert.That(session.Report.Frames.Count, Is.EqualTo(120));
                Assert.That(session.Report.CompletedFrameRejectedAttemptCount, Is.EqualTo(1));
                Assert.That(
                    session.Report.MaxCompletedFrameSampleAttempts,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                Assert.That(session.Report.CompletedFrameSamplingFailureReason, Is.Empty);
                Assert.That(
                    FindMetricAvailability(session.Report, "exactSampleCount").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Passed));
                for (int index = 0; index < session.Report.Frames.Count; index++)
                    Assert.That(session.Report.Frames[index].GpuFrameTimeMs.Available, Is.True);
                Assert.That(collector.RequestedGenerations.Count, Is.EqualTo(121));
                AssertGenerationsStrictlyIncrease(collector.RequestedGenerations);
                StringAssert.Contains("\"rejectedAttemptCount\":1", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void CompletedFrameSampling_AcceptsPositiveDrawCallsOnSixteenthBoundedAttempt()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var attempts = new BattleBenchmarkCompletedFrameMetrics[
                BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts];
            for (int index = 0; index < attempts.Length - 1; index++)
                attempts[index] = CreateAvailableCompletedFrame(drawCalls: 0);
            attempts[attempts.Length - 1] = CreateAvailableCompletedFrame(drawCalls: 7);
            var collector = new SequencedCompletedFrameCollector(attempts);
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    1,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                collector,
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(
                        ++guard,
                        Is.LessThan(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts * 3));

                Assert.That(session.Report.Frames.Count, Is.EqualTo(1));
                Assert.That(
                    session.Report.CompletedFrameRejectedAttemptCount,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts - 1));
                Assert.That(collector.RequestedGenerations.Count,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                Assert.That(session.Report.CompletedFrameSamplingFailureReason, Is.Empty);
                Assert.That(FindMetricAvailability(session.Report, "drawCalls").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Available));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void CompletedFrameSampling_ExhaustedRetryBudget_IsIncompleteWithExplicitReason()
        {
            BattleRenderingBenchmarkWorkload workload =
                BattleRenderingBenchmarkWorkload.Create(
                    BattleRenderingBenchmarkScenario.Parse("100"),
                    null,
                    0,
                    1);
            var collector = new SequencedCompletedFrameCollector(
                CreateUnavailableGpuCompletedFrame(drawCalls: 7));
            var session = new BattleRenderingBenchmarkSession(
                new BattleRenderingBenchmarkConfig(
                    BattlePresentationBackendMode.CentralOnly,
                    0,
                    1,
                    "100",
                    string.Empty),
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                collector,
                new InjectedPresenter(workload, central: true, submissionDrawCalls: 1));
            try
            {
                int guard = 0;
                while (!session.CaptureFrame())
                    Assert.That(
                        ++guard,
                        Is.LessThan(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts * 3));

                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
                Assert.That(session.Report.Frames.Count, Is.EqualTo(0));
                Assert.That(
                    session.Report.CompletedFrameRejectedAttemptCount,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                StringAssert.Contains("exhausted", session.Report.CompletedFrameSamplingFailureReason);
                StringAssert.Contains("gpuFrameTimeMs", session.Report.CompletedFrameSamplingFailureReason);
                Assert.That(
                    FindMetricAvailability(session.Report, "exactSampleCount").Status,
                    Is.EqualTo(BattleBenchmarkMetricStatus.Missing));
                StringAssert.Contains(
                    session.Report.CompletedFrameSamplingFailureReason,
                    FindMetricAvailability(session.Report, "gpuFrameTimeMs").Reason);
                Assert.That(
                    collector.RequestedGenerations.Count,
                    Is.EqualTo(BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts));
                AssertGenerationsStrictlyIncrease(collector.RequestedGenerations);
                StringAssert.Contains("terminalFailureReason", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void V5VerdictPolicy_MissingRequiredMetricCannotPass()
        {
            var context = new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                true,
                true);
            var metrics = new[]
            {
                new BattleBenchmarkMetricAvailability(
                    "drawCalls",
                    true,
                    BattleBenchmarkMetricApplicability.Applicable,
                    BattleBenchmarkMetricStatus.Missing,
                    "completed-frame",
                    0,
                    3,
                    "injected",
                    "missing completed-frame counter"),
            };

            BattleRenderingBenchmarkVerdict verdict =
                BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                    context,
                    metrics,
                    out string reason,
                    out string[] missing);

            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
            Assert.That(missing, Does.Contain("drawCalls"));
            StringAssert.Contains("drawCalls", reason);
        }

        [Test]
        public void V5VerdictPolicy_EntirelyOmittedMandatoryEntryCannotPass()
        {
            var metrics = CreateCompleteMetricSchema();
            metrics.RemoveAll(metric => metric.Metric == "benchmarkOwnedTextureMemoryBytes");

            BattleRenderingBenchmarkVerdict verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                SupportedPolicyContext(),
                metrics,
                out _,
                out string[] missing);

            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
            Assert.That(missing, Does.Contain("benchmarkOwnedTextureMemoryBytes"));
        }

        [Test]
        public void V5VerdictPolicy_DuplicateMandatoryEntryCannotPass()
        {
            var metrics = CreateCompleteMetricSchema();
            metrics.Add(metrics[0]);

            BattleRenderingBenchmarkVerdict verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                SupportedPolicyContext(),
                metrics,
                out _,
                out string[] missing);

            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
            Assert.That(missing, Does.Contain(metrics[0].Metric + " (duplicate schema entry)"));
        }

        [Test]
        public void CurrentSceneUnmeasuredDeterminismEvidence_IsMissingRatherThanFailed()
        {
            Assert.That(
                BattleRenderingBenchmarkEvidencePolicy.ValidationStatus(null),
                Is.EqualTo(BattleBenchmarkMetricStatus.Missing));
            Assert.That(
                BattleRenderingBenchmarkEvidencePolicy.ValidationStatus(false),
                Is.EqualTo(BattleBenchmarkMetricStatus.Failed));

            var metrics = CreateCompleteMetricSchema();
            int index = metrics.FindIndex(metric => metric.Metric == "determinismValidated");
            metrics[index] = new BattleBenchmarkMetricAvailability(
                "determinismValidated",
                true,
                BattleBenchmarkMetricApplicability.Applicable,
                BattleBenchmarkMetricStatus.Missing,
                "validation-gate",
                0,
                1,
                "current-scene runtime checksum",
                "The current-scene workload did not measure this validation gate.");

            BattleRenderingBenchmarkVerdict verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                SupportedPolicyContext(),
                metrics,
                out _,
                out _);
            Assert.That(verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Incomplete));
        }

        [Test]
        public void PlayerArguments_RequireExplicitOptIn_AndParseRequest()
        {
            Assert.That(
                BattleRenderingBenchmarkPlayerArguments.TryParse(
                    new[] { "Game.exe", "-batchmode" },
                    out _,
                    out _),
                Is.False);

            bool parsed = BattleRenderingBenchmarkPlayerArguments.TryParse(
                new[]
                {
                    "Game.exe",
                    BattleRenderingBenchmarkPlayerArguments.EnableArgument,
                    BattleRenderingBenchmarkPlayerArguments.ScenarioArgument, "1000",
                    BattleRenderingBenchmarkPlayerArguments.ComparisonArgument, "ab",
                    BattleRenderingBenchmarkPlayerArguments.WarmupArgument, "2",
                    BattleRenderingBenchmarkPlayerArguments.SampleArgument, "3",
                    BattleRenderingBenchmarkPlayerArguments.LeakArgument, "4",
                    BattleRenderingBenchmarkPlayerArguments.OutputArgument, "Temp/player.json",
                },
                out BattleRenderingBenchmarkRequest request,
                out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(request.targetActiveEntities, Is.EqualTo("1000"));
            Assert.That(request.comparison, Is.EqualTo("ab"));
            Assert.That(request.warmupFrames, Is.EqualTo(2));
            Assert.That(request.sampleFrames, Is.EqualTo(3));
            Assert.That(request.leakCheckFrames, Is.EqualTo(4));
            Assert.That(request.outputPath, Is.EqualTo("Temp/player.json"));
        }

        [Test]
        public void StandalonePlayerBuildArguments_PreserveRealRenderingAndV5BenchmarkRequest()
        {
            string output = "Temp/P8-D-runtime-100-player-ab-v5.json";
            string log = "Temp/P8-D-runtime-100-player-ab-v5.log";
            string arguments = BattleRenderingBenchmarkPlayerBuild.BuildArguments("100", output, log);

            StringAssert.DoesNotContain("-batchmode", arguments);
            StringAssert.DoesNotContain("-nographics", arguments);
            StringAssert.Contains("-logFile \"" + log + "\"", arguments);

            var parsedArguments = new List<string> { "Game.exe" };
            string[] commandLineTokens = arguments.Split(' ');
            for (int index = 0; index < commandLineTokens.Length; index++)
                parsedArguments.Add(commandLineTokens[index].Trim('"'));
            bool parsed = BattleRenderingBenchmarkPlayerArguments.TryParse(
                parsedArguments.ToArray(),
                out BattleRenderingBenchmarkRequest request,
                out string error);

            Assert.That(parsed, Is.True, error);
            Assert.That(request.targetActiveEntities, Is.EqualTo("100"));
            Assert.That(request.comparison, Is.EqualTo("ab"));
            Assert.That(request.warmupFrames, Is.EqualTo(30));
            Assert.That(request.sampleFrames, Is.EqualTo(120));
            Assert.That(request.leakCheckFrames, Is.EqualTo(600));
            Assert.That(request.outputPath, Is.EqualTo(output));
        }

        [Test]
        public void RequestProcessor_ExitingPlayMode_AbortsAndClearsCurrentRunner()
        {
            var host = new UnityEngine.GameObject("Benchmark Processor Exit Test");
            BattleRenderingBenchmarkRunner staleRunner =
                host.AddComponent<BattleRenderingBenchmarkRunner>();
            BattleRenderingBenchmarkRequestProcessor.SetRunnerForTests(staleRunner);

            BattleRenderingBenchmarkRequestProcessor.HandlePlayModeStateChangeForTests(
                PlayModeStateChange.ExitingPlayMode);

            Assert.That(BattleRenderingBenchmarkRequestProcessor.HasRunnerForTests, Is.False);
            Assert.That(host == null, Is.True, "Abort must destroy the stale runner host.");
        }

        [Test]
        public void RequestProcessor_NonPlayPolling_ReconcilesStaleRunner()
        {
            var host = new UnityEngine.GameObject("Benchmark Processor Stale Poll Test");
            BattleRenderingBenchmarkRunner staleRunner =
                host.AddComponent<BattleRenderingBenchmarkRunner>();
            BattleRenderingBenchmarkRequestProcessor.SetRunnerForTests(staleRunner);

            BattleRenderingBenchmarkRequestProcessor.PollRequestForTests(false);

            Assert.That(BattleRenderingBenchmarkRequestProcessor.HasRunnerForTests, Is.False);
            Assert.That(host == null, Is.True, "Non-Play polling must not leave a stale runner registered.");
        }

        [Test]
        public void RequestProcessor_NonPlayPolling_PreservesQueuedRequest()
        {
            string requestPath = BattleRenderingBenchmarkRequestProcessor.ProjectPath(
                BattleRenderingBenchmarkRequestProcessor.RequestFile);
            string priorContent = File.Exists(requestPath) ? File.ReadAllText(requestPath) : null;
            try
            {
                BattleRenderingBenchmarkRequestProcessor.WriteRequest(
                    new BattleRenderingBenchmarkRequest
                    {
                        backend = "CentralOnly",
                        comparison = "ab",
                        warmupFrames = 0,
                        sampleFrames = 1,
                        leakCheckFrames = 0,
                        maxManagedGrowthBytes = 1024L,
                        maxGraphicsGrowthBytes = 1024L,
                        targetActiveEntities = "100",
                        outputPath = "Temp/request-preservation-test.json",
                    });

                BattleRenderingBenchmarkRequestProcessor.PollRequestForTests(false);

                Assert.That(File.Exists(requestPath), Is.True,
                    "A request must remain queued until the Editor enters Play Mode.");
            }
            finally
            {
                if (priorContent == null)
                {
                    if (File.Exists(requestPath))
                        File.Delete(requestPath);
                }
                else
                {
                    File.WriteAllText(requestPath, priorContent);
                }
            }
        }

        [Test]
        public void LeakCheck_CollectsTransientGarbage_OutsidePerformanceSamples()
        {
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                1048576,
                1048576,
                "100",
                string.Empty);
            var session = new BattleRenderingBenchmarkSession(config, new SimulationWorld());
            try
            {
                Assert.That(session.CaptureFrame(), Is.False, "the first call requests a completed-frame sample");
                Assert.That(session.CaptureFrame(), Is.False, "the second call drains the sample and captures the leak baseline");
                WeakReference transient = AllocateTransientGarbageWithoutConservativeStackRoot(
                    16 * 1024 * 1024);
                int guard = 0;
                while (!session.CaptureFrame() && guard++ < 8)
                {
                }

                Assert.That(session.IsComplete, Is.True);
                Assert.That(transient.IsAlive, Is.False, "the soak endpoint full collection must reclaim transient garbage");
                Assert.That(session.Report.LeakReport.Available, Is.True);
                if (!session.Report.LeakReport.GraphicsAvailable)
                    Assert.That(session.Report.LeakReport.Passed, Is.False);
                Assert.That(session.Report.Passed, Is.False, "EditMode can never produce a v5 PASS verdict");
                Assert.That(
                    session.Report.LeakReport.MeasurementMode,
                    Is.EqualTo(BattleRenderingBenchmarkSession.RetainedManagedHeapMeasurementMode));
                StringAssert.Contains(
                    "full-gc-retained-managed-heap-outside-performance-sample-window-v1",
                    session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [TestCase(false, BattleRenderingBenchmarkVerdict.Pass, BattleBenchmarkMetricStatus.Passed)]
        [TestCase(true, BattleRenderingBenchmarkVerdict.Fail, BattleBenchmarkMetricStatus.Failed)]
        public void LeakCheck_RequiresSuccessfulPostDisposeTeardown(
            bool retainOwnershipAfterDispose,
            BattleRenderingBenchmarkVerdict expectedVerdict,
            BattleBenchmarkMetricStatus expectedTeardownStatus)
        {
            BattleRenderingBenchmarkWorkload workload = BattleRenderingBenchmarkWorkload.Create(
                BattleRenderingBenchmarkScenario.Parse("100"),
                null,
                0,
                1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                200,
                200,
                "100",
                string.Empty);
            var presenter = new LeakPresenter(workload, retainOwnershipAfterDispose);
            var probe = new ScriptedLeakProbe(
                new long[] { 1000, 1100, 1150, 1050 },
                new long[] { 2000, 2100, 2150, 2050 });
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(7)),
                presenter,
                probe);
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False,
                    "the last soak frame begins teardown but cannot finalize in the same call");
                Assert.That(presenter.Disposed, Is.True);
                Assert.That(session.CaptureFrame(), Is.False,
                    "deferred Unity destruction must wait two Play frames");
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.DeferredDestructionPlayFrames);
                Assert.That(session.CaptureFrame(), Is.False,
                    "post-destroy cleanup must start after the deferred-destruction wait");
                Assert.That(probe.PostDisposeCleanupRequestCount, Is.EqualTo(1));
                Assert.That(session.CaptureFrame(), Is.False,
                    "the completed cleanup must be flushed before teardown metrics are read");
                Assert.That(probe.PostDisposeCleanupCompletionCount, Is.EqualTo(1));
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.PostDisposeCleanupPlayFrames);
                Assert.That(session.CaptureFrame(), Is.True);

                Assert.That(presenter.PresentCalls, Is.EqualTo(3));
                Assert.That(session.Report.LeakReport.TeardownStatus, Is.EqualTo(expectedTeardownStatus));
                Assert.That(session.Report.LeakReport.TeardownResourcesEnd,
                    Is.EqualTo(retainOwnershipAfterDispose ? 1 : 0));
                Assert.That(session.Report.LeakReport.TeardownOwnedEndBytes,
                    Is.EqualTo(retainOwnershipAfterDispose ? 256 : 0));
                Assert.That(session.Report.Verdict, Is.EqualTo(expectedVerdict));
                StringAssert.Contains("teardownStatus", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void LeakCheck_TeardownUsesSteadyStateBaseline_NotPrePresenterInitialization()
        {
            BattleRenderingBenchmarkWorkload workload = BattleRenderingBenchmarkWorkload.Create(
                BattleRenderingBenchmarkScenario.Parse("100"),
                null,
                0,
                1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                200,
                200,
                "100",
                string.Empty);
            var presenter = new LeakPresenter(workload, false);
            var probe = new ScriptedLeakProbe(
                new long[] { 1000, 2000, 2100, 2050 },
                new long[] { 2000, 4000, 4100, 4050 });
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(7)),
                presenter,
                probe);
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.DeferredDestructionPlayFrames);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.PostDisposeCleanupPlayFrames);
                Assert.That(session.CaptureFrame(), Is.True);

                BattleRenderingBenchmarkLeakReport leak = session.Report.LeakReport;
                Assert.That(leak.PrePresenterManagedBytes, Is.EqualTo(1000));
                Assert.That(leak.ManagedStartBytes, Is.EqualTo(2000));
                Assert.That(leak.TeardownManagedGrowthBytes, Is.EqualTo(50));
                Assert.That(leak.PrePresenterGraphicsBytes, Is.EqualTo(2000));
                Assert.That(leak.GraphicsStartBytes, Is.EqualTo(4000));
                Assert.That(leak.TeardownGraphicsGrowthBytes, Is.EqualTo(50));
                Assert.That(leak.TeardownOwnedEndBytes, Is.Zero);
                Assert.That(leak.TeardownResourcesEnd, Is.Zero);
                Assert.That(leak.TeardownStatus, Is.EqualTo(BattleBenchmarkMetricStatus.Passed));
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Pass));
                StringAssert.Contains("teardownMemoryBaseline", session.Report.ToJson());
            }
            finally
            {
                session.Dispose();
            }
        }

        [Test]
        public void LeakCheck_TeardownCleanupTimeoutFailsBoundedly()
        {
            BattleRenderingBenchmarkWorkload workload = BattleRenderingBenchmarkWorkload.Create(
                BattleRenderingBenchmarkScenario.Parse("100"),
                null,
                0,
                1);
            var config = new BattleRenderingBenchmarkConfig(
                BattlePresentationBackendMode.CentralOnly,
                BattleRenderingBenchmarkComparison.Single,
                0,
                1,
                2,
                200,
                200,
                "100",
                string.Empty);
            var probe = new ScriptedLeakProbe(
                new long[] { 1000, 1100, 1150, 1050 },
                new long[] { 2000, 2100, 2150, 2050 },
                postDisposeCleanupComplete: false);
            var session = new BattleRenderingBenchmarkSession(
                config,
                new SimulationWorld(),
                workload,
                SupportedPolicyContext(),
                new BattleBenchmarkInjectedCompletedFrameCollector(
                    CreateAvailableCompletedFrame(7)),
                new LeakPresenter(workload, false),
                probe);
            try
            {
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.False);
                probe.AdvanceFrames(BattleRenderingBenchmarkSession.MaxPostDisposeCleanupPlayFrames);
                Assert.That(session.CaptureFrame(), Is.False);
                Assert.That(session.CaptureFrame(), Is.True);

                Assert.That(probe.PostDisposeCleanupRequestCount, Is.EqualTo(1));
                Assert.That(probe.PostDisposeCleanupCompletionCount, Is.Zero);
                Assert.That(session.Report.LeakReport.TeardownStatus, Is.EqualTo(BattleBenchmarkMetricStatus.Failed));
                StringAssert.Contains("did not complete within", session.Report.LeakReport.TeardownReason);
                Assert.That(session.Report.Verdict, Is.EqualTo(BattleRenderingBenchmarkVerdict.Fail));
            }
            finally
            {
                session.Dispose();
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference AllocateTransientGarbage(int bytes)
        {
            var garbage = new byte[bytes];
            garbage[0] = 1;
            garbage[garbage.Length - 1] = 2;
            return new WeakReference(garbage);
        }

        private static WeakReference AllocateTransientGarbageWithoutConservativeStackRoot(int bytes)
        {
            WeakReference result = null;
            var thread = new Thread(() => result = AllocateTransientGarbage(bytes));
            thread.Start();
            thread.Join();
            return result;
        }

        private static BattleBenchmarkMetricAvailability FindMetricAvailability(
            BattleRenderingBenchmarkReport report,
            string metric)
        {
            for (int index = 0; index < report.MetricAvailability.Count; index++)
            {
                BattleBenchmarkMetricAvailability candidate = report.MetricAvailability[index];
                if (candidate.Metric == metric)
                    return candidate;
            }
            Assert.Fail("Missing metric availability entry: " + metric);
            return null;
        }

        private static BattleRenderingBenchmarkPolicyContext SupportedPolicyContext()
        {
            return new BattleRenderingBenchmarkPolicyContext(
                true,
                true,
                UnityEngine.RuntimePlatform.WindowsEditor,
                true,
                true);
        }

        private static List<BattleBenchmarkMetricAvailability> CreateCompleteMetricSchema()
        {
            var metrics = new List<BattleBenchmarkMetricAvailability>(
                BattleRenderingBenchmarkVerdictPolicy.RequiredMetricNames.Count);
            for (int index = 0;
                 index < BattleRenderingBenchmarkVerdictPolicy.RequiredMetricNames.Count;
                 index++)
            {
                metrics.Add(new BattleBenchmarkMetricAvailability(
                    BattleRenderingBenchmarkVerdictPolicy.RequiredMetricNames[index],
                    true,
                    BattleBenchmarkMetricApplicability.Applicable,
                    BattleBenchmarkMetricStatus.Available,
                    "test",
                    1,
                    1,
                    "injected complete schema",
                    string.Empty));
            }
            return metrics;
        }

        private static BattleBenchmarkCompletedFrameMetrics CreateAvailableCompletedFrame(
            int drawCalls)
        {
            return new BattleBenchmarkCompletedFrameMetrics(
                BattleBenchmarkMetric.FromValue(16.0, "ms"),
                BattleBenchmarkMetric.FromValue(8.0, "ms"),
                BattleBenchmarkMetric.FromValue(2.0, "ms"),
                BattleBenchmarkMetric.FromValue(5.0, "ms"),
                BattleBenchmarkMetric.FromValue(0, "bytes"),
                BattleBenchmarkMetric.FromValue(drawCalls, "count"),
                BattleBenchmarkMetric.FromValue(1024, "bytes"),
                BattleBenchmarkMetric.FromValue(2048, "bytes"));
        }

        private static BattleBenchmarkCompletedFrameMetrics CreateUnavailableGpuCompletedFrame(
            int drawCalls)
        {
            BattleBenchmarkCompletedFrameMetrics available = CreateAvailableCompletedFrame(drawCalls);
            return new BattleBenchmarkCompletedFrameMetrics(
                available.FrameTimeMs,
                available.MainThreadTimeMs,
                available.RenderThreadTimeMs,
                BattleBenchmarkMetric.Unavailable("ms"),
                available.ManagedAllocationBytes,
                available.DrawCalls,
                available.TotalAllocatedMemoryBytes,
                available.GraphicsMemoryBytes);
        }

        private static void AssertGenerationsStrictlyIncrease(IReadOnlyList<int> generations)
        {
            for (int index = 1; index < generations.Count; index++)
                Assert.That(generations[index], Is.GreaterThan(generations[index - 1]));
        }

        private sealed class SequencedCompletedFrameCollector :
            IBattleBenchmarkCompletedFrameCollector
        {
            private readonly BattleBenchmarkCompletedFrameMetrics[] sequence;
            private int pendingGeneration;
            private int nextIndex;

            internal SequencedCompletedFrameCollector(
                params BattleBenchmarkCompletedFrameMetrics[] completedFrameMetrics)
            {
                sequence = completedFrameMetrics ?? throw new ArgumentNullException(nameof(completedFrameMetrics));
                if (sequence.Length == 0)
                    throw new ArgumentException("At least one completed-frame sample is required.", nameof(completedFrameMetrics));
            }

            internal List<int> RequestedGenerations { get; } = new List<int>();
            public bool IsSupported => true;
            public string UnsupportedReason => string.Empty;

            public void Request(int generation)
            {
                if (pendingGeneration != 0)
                    throw new InvalidOperationException("A completed-frame sample is already pending.");
                pendingGeneration = generation;
                RequestedGenerations.Add(generation);
            }

            public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics)
            {
                if (pendingGeneration != generation)
                {
                    metrics = default;
                    return false;
                }
                pendingGeneration = 0;
                int index = Math.Min(nextIndex++, sequence.Length - 1);
                metrics = sequence[index];
                return true;
            }

            public string Source(BattleBenchmarkRecorderKind kind) => "sequenced completed-frame test sample";
            public string Reason(BattleBenchmarkRecorderKind kind) => string.Empty;
            public void Reset() => pendingGeneration = 0;
            public void Dispose() => Reset();
        }

        private sealed class InjectedPresenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;
            private readonly bool central;
            private readonly int submissionDrawCalls;
            private readonly BattleCentralBuildDiagnostics diagnostics;
            private bool disposed;

            internal InjectedPresenter(
                BattleRenderingBenchmarkWorkload benchmarkWorkload,
                bool central,
                int submissionDrawCalls)
            {
                workload = benchmarkWorkload;
                this.central = central;
                this.submissionDrawCalls = submissionDrawCalls;
                if (central)
                    diagnostics = new BattleCentralBuildDiagnostics();
            }

            public string Implementation => "InjectedPresenter";
            public string EffectiveBackend => central ? "CentralOnly" : "LegacyOnly";
            public string ResourceMode => "Injected";
            public string DrawMode => "Injected";
            public int RenderTargetWidth => 256;
            public int RenderTargetHeight => 256;
            public int ResolvedCommandCount => workload.CommandCount;
            public int MaterializedRenderItemCount => workload.CommandCount;
            public int ResourceSegmentCount => 7;
            public int SubmissionDrawCount => submissionDrawCalls;
            public string SubmissionDrawMetricSource => "injected presenter-local submissions";
            public string SubmissionDrawUnavailableReason => "Legacy local submissions are not applicable.";
            public int ResourceGeneration => 7;
            public int OwnedTextureResourceCount => disposed ? 0 : 2;
            public int OwnedResourceCount => disposed ? 0 : 1;
            public long CachedOwnedResourceMemoryBytes => disposed ? 0L : 256L;
            public long MeasureOwnedResourceMemoryBytes() => disposed ? 0L : 256L;
            public long MeasureOwnedTextureMemoryBytes() => disposed ? 0L : 128L;
            public BattleCentralBuildDiagnostics Diagnostics => diagnostics;
            public double Present() => 0.25;
            public void Dispose()
            {
                disposed = true;
            }
        }

        private sealed class ThrowingPresenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;

            internal ThrowingPresenter(BattleRenderingBenchmarkWorkload benchmarkWorkload)
            {
                workload = benchmarkWorkload;
            }

            internal bool Disposed { get; private set; }
            public string Implementation => "ThrowingPresenter";
            public string EffectiveBackend => "CentralOnly";
            public string ResourceMode => "Injected";
            public string DrawMode => "Injected";
            public int RenderTargetWidth => 256;
            public int RenderTargetHeight => 256;
            public int ResolvedCommandCount => workload.CommandCount;
            public int MaterializedRenderItemCount => workload.CommandCount;
            public int ResourceSegmentCount => 1;
            public int SubmissionDrawCount => 1;
            public string SubmissionDrawMetricSource => "injected";
            public string SubmissionDrawUnavailableReason => string.Empty;
            public int ResourceGeneration => 7;
            public int OwnedTextureResourceCount => Disposed ? 0 : 2;
            public int OwnedResourceCount => Disposed ? 0 : 1;
            public long CachedOwnedResourceMemoryBytes => Disposed ? 0L : 256L;
            public long MeasureOwnedResourceMemoryBytes() => Disposed ? 0L : 256L;
            public long MeasureOwnedTextureMemoryBytes() => Disposed ? 0L : 128L;
            public BattleCentralBuildDiagnostics Diagnostics { get; } = new BattleCentralBuildDiagnostics();

            public double Present()
            {
                throw new InvalidOperationException("Injected presenter failure.");
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private sealed class LeakPresenter : IBattleRenderingBenchmarkPresenter
        {
            private readonly BattleRenderingBenchmarkWorkload workload;
            private readonly bool retainOwnershipAfterDispose;

            internal LeakPresenter(
                BattleRenderingBenchmarkWorkload benchmarkWorkload,
                bool retainOwnership)
            {
                workload = benchmarkWorkload;
                retainOwnershipAfterDispose = retainOwnership;
            }

            internal bool Disposed { get; private set; }
            internal int PresentCalls { get; private set; }
            private bool OwnsResources => !Disposed || retainOwnershipAfterDispose;
            public string Implementation => "LeakPresenter";
            public string EffectiveBackend => "CentralOnly";
            public string ResourceMode => "Injected";
            public string DrawMode => "Injected";
            public int RenderTargetWidth => 256;
            public int RenderTargetHeight => 256;
            public int ResolvedCommandCount => workload.CommandCount;
            public int MaterializedRenderItemCount => workload.CommandCount;
            public int ResourceSegmentCount => 1;
            public int SubmissionDrawCount => 1;
            public string SubmissionDrawMetricSource => "injected";
            public string SubmissionDrawUnavailableReason => string.Empty;
            public int ResourceGeneration => 7;
            public int OwnedTextureResourceCount => OwnsResources ? 2 : 0;
            public int OwnedResourceCount => OwnsResources ? 1 : 0;
            public long CachedOwnedResourceMemoryBytes => OwnsResources ? 256L : 0L;
            public long MeasureOwnedResourceMemoryBytes() => OwnsResources ? 256L : 0L;
            public long MeasureOwnedTextureMemoryBytes() => OwnsResources ? 128L : 0L;
            public BattleCentralBuildDiagnostics Diagnostics { get; } = new BattleCentralBuildDiagnostics();

            public double Present()
            {
                if (Disposed)
                    throw new ObjectDisposedException(nameof(LeakPresenter));
                PresentCalls++;
                return 0.25;
            }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        private sealed class ScriptedLeakProbe : IBattleBenchmarkLeakProbe
        {
            private readonly long[] managedSamples;
            private readonly long[] graphicsSamples;
            private int managedIndex;
            private int graphicsIndex;

            private readonly bool postDisposeCleanupComplete;

            internal ScriptedLeakProbe(
                long[] retainedManagedSamples,
                long[] retainedGraphicsSamples,
                bool postDisposeCleanupComplete = true)
            {
                managedSamples = retainedManagedSamples;
                graphicsSamples = retainedGraphicsSamples;
                this.postDisposeCleanupComplete = postDisposeCleanupComplete;
            }

            public int CurrentUnityFrame { get; private set; }
            public bool RequiresDeferredDestructionWait => true;
            public int PostDisposeCleanupRequestCount { get; private set; }
            public int PostDisposeCleanupCompletionCount { get; private set; }
            public bool IsPostDisposeCleanupComplete => postDisposeCleanupComplete;

            public long CaptureRetainedManagedHeapBytes()
            {
                return managedSamples[managedIndex++];
            }

            public BattleBenchmarkMetric CaptureGraphicsMemory()
            {
                return BattleBenchmarkMetric.FromValue(graphicsSamples[graphicsIndex++], "bytes");
            }

            public void BeginPostDisposeCleanup()
            {
                PostDisposeCleanupRequestCount++;
            }

            public void CompletePostDisposeCleanup()
            {
                PostDisposeCleanupCompletionCount++;
            }

            internal void AdvanceFrames(int count)
            {
                CurrentUnityFrame += count;
            }
        }
    }
}
#endif


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs ---
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

namespace NTSD.Animation.Rendering
{
    public enum BattleRenderingBenchmarkComparison : byte
    {
        Single = 0,
        CentralLegacyAB = 1,
    }

    public enum BattleRenderingBenchmarkScenarioKind : byte
    {
        CurrentScene = 0,
        Entities100 = 1,
        Entities300 = 2,
        Entities500 = 3,
        Entities1000 = 4,
    }

    public enum BattleRenderingBenchmarkVerdict : byte
    {
        Pass = 0,
        Fail = 1,
        Incomplete = 2,
        Unsupported = 3,
    }

    public enum BattleBenchmarkMetricApplicability : byte
    {
        Applicable = 0,
        NotApplicable = 1,
    }

    public enum BattleBenchmarkMetricStatus : byte
    {
        Available = 0,
        Missing = 1,
        NotApplicable = 2,
        Unsupported = 3,
        Passed = 4,
        Failed = 5,
    }

    public readonly struct BattleRenderingBenchmarkScenario
    {
        private BattleRenderingBenchmarkScenario(
            BattleRenderingBenchmarkScenarioKind kind,
            int requestedEntityCount,
            string name)
        {
            Kind = kind;
            RequestedEntityCount = requestedEntityCount;
            Name = name;
        }

        public BattleRenderingBenchmarkScenarioKind Kind { get; }
        public int RequestedEntityCount { get; }
        public string Name { get; }
        public bool UsesCurrentScene => Kind == BattleRenderingBenchmarkScenarioKind.CurrentScene;

        public static BattleRenderingBenchmarkScenario Parse(string value)
        {
            string normalized = string.IsNullOrWhiteSpace(value)
                ? "current-scene"
                : value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "current-scene":
                    return new BattleRenderingBenchmarkScenario(
                        BattleRenderingBenchmarkScenarioKind.CurrentScene,
                        -1,
                        "current-scene");
                case "100":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities100, 100);
                case "300":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities300, 300);
                case "500":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities500, 500);
                case "1000":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities1000, 1000);
                default:
                    throw new ArgumentException(
                        $"Unknown benchmark scenario '{value}'. Expected current-scene, 100, 300, 500, or 1000.",
                        nameof(value));
            }
        }

        private static BattleRenderingBenchmarkScenario Fixed(
            BattleRenderingBenchmarkScenarioKind kind,
            int count)
        {
            return new BattleRenderingBenchmarkScenario(kind, count, count.ToString());
        }
    }

    [Serializable]
    public sealed class BattleRenderingBenchmarkRequest
    {
        public string backend = nameof(BattlePresentationBackendMode.CentralOnly);
        public string comparison = "single";
        public int warmupFrames = 30;
        public int sampleFrames = 120;
        public int leakCheckFrames = 600;
        public long maxManagedGrowthBytes = 1048576L;
        public long maxGraphicsGrowthBytes = 4194304L;
        public string targetActiveEntities = "current-scene";
        public string outputPath = "Temp/NTSD_BattleRenderingBenchmark.json";
    }

    public readonly struct BattleRenderingBenchmarkConfig
    {
        public BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode backend,
            int warmupFrames,
            int sampleFrames,
            string targetActiveEntities,
            string outputPath)
            : this(
                backend,
                BattleRenderingBenchmarkComparison.Single,
                warmupFrames,
                sampleFrames,
                0,
                1048576L,
                4194304L,
                targetActiveEntities,
                outputPath)
        {
        }

        public BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode backend,
            BattleRenderingBenchmarkComparison comparison,
            int warmupFrames,
            int sampleFrames,
            int leakCheckFrames,
            long maxManagedGrowthBytes,
            long maxGraphicsGrowthBytes,
            string targetActiveEntities,
            string outputPath)
        {
            BattlePresentationBackendResolver.ValidateAvailable(backend);
            if (backend == BattlePresentationBackendMode.CentralShadowBuild)
            {
                throw new ArgumentException(
                    "CentralShadowBuild fixes pixel ownership to Legacy and is not a valid P8-D A/B backend.",
                    nameof(backend));
            }
            if (comparison != BattleRenderingBenchmarkComparison.Single &&
                comparison != BattleRenderingBenchmarkComparison.CentralLegacyAB)
            {
                throw new ArgumentOutOfRangeException(nameof(comparison));
            }
            if (warmupFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(warmupFrames));
            if (sampleFrames <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleFrames));
            if (leakCheckFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(leakCheckFrames));
            if (maxManagedGrowthBytes < 0L)
                throw new ArgumentOutOfRangeException(nameof(maxManagedGrowthBytes));
            if (maxGraphicsGrowthBytes < 0L)
                throw new ArgumentOutOfRangeException(nameof(maxGraphicsGrowthBytes));

            Scenario = BattleRenderingBenchmarkScenario.Parse(targetActiveEntities);
            Backend = backend;
            Comparison = comparison;
            WarmupFrames = warmupFrames;
            SampleFrames = sampleFrames;
            LeakCheckFrames = leakCheckFrames;
            MaxManagedGrowthBytes = maxManagedGrowthBytes;
            MaxGraphicsGrowthBytes = maxGraphicsGrowthBytes;
            OutputPath = outputPath ?? string.Empty;
        }

        public BattlePresentationBackendMode Backend { get; }
        public BattleRenderingBenchmarkComparison Comparison { get; }
        public int WarmupFrames { get; }
        public int SampleFrames { get; }
        public int LeakCheckFrames { get; }
        public long MaxManagedGrowthBytes { get; }
        public long MaxGraphicsGrowthBytes { get; }
        public BattleRenderingBenchmarkScenario Scenario { get; }
        public string TargetActiveEntities => Scenario.Name;
        public string OutputPath { get; }

        public static BattleRenderingBenchmarkConfig Default => new BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode.CentralOnly,
            BattleRenderingBenchmarkComparison.CentralLegacyAB,
            30,
            120,
            600,
            1048576L,
            4194304L,
            "current-scene",
            "Temp/NTSD_BattleRenderingBenchmark.json");

        public BattleRenderingBenchmarkConfig ForBackend(BattlePresentationBackendMode backend)
        {
            return new BattleRenderingBenchmarkConfig(
                backend,
                BattleRenderingBenchmarkComparison.Single,
                WarmupFrames,
                SampleFrames,
                LeakCheckFrames,
                MaxManagedGrowthBytes,
                MaxGraphicsGrowthBytes,
                Scenario.Name,
                OutputPath);
        }

        public static BattleRenderingBenchmarkConfig FromRequest(BattleRenderingBenchmarkRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            string backendText = string.IsNullOrWhiteSpace(request.backend)
                ? nameof(BattlePresentationBackendMode.CentralOnly)
                : request.backend;
            if (!BattlePresentationBackendResolver.TryParse(backendText, out BattlePresentationBackendMode backend))
                throw new ArgumentException($"Unknown battle presentation backend '{backendText}'.", nameof(request));
            BattleRenderingBenchmarkComparison comparison = ParseComparison(request.comparison);
            return new BattleRenderingBenchmarkConfig(
                backend,
                comparison,
                request.warmupFrames,
                request.sampleFrames,
                request.leakCheckFrames,
                request.maxManagedGrowthBytes,
                request.maxGraphicsGrowthBytes,
                request.targetActiveEntities,
                request.outputPath);
        }

        private static BattleRenderingBenchmarkComparison ParseComparison(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                string.Equals(value.Trim(), "single", StringComparison.OrdinalIgnoreCase))
            {
                return BattleRenderingBenchmarkComparison.Single;
            }
            if (string.Equals(value.Trim(), "ab", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value.Trim(), "central-legacy-ab", StringComparison.OrdinalIgnoreCase))
            {
                return BattleRenderingBenchmarkComparison.CentralLegacyAB;
            }
            throw new ArgumentException(
                $"Unknown benchmark comparison '{value}'. Expected single or ab.",
                nameof(value));
        }
    }

    public readonly struct BattleBenchmarkMetric
    {
        private BattleBenchmarkMetric(bool available, double value, string unit)
        {
            Available = available;
            Value = value;
            Unit = unit ?? string.Empty;
        }

        public bool Available { get; }
        public double Value { get; }
        public string Unit { get; }

        public static BattleBenchmarkMetric Unavailable(string unit = "") =>
            new BattleBenchmarkMetric(false, 0d, unit);

        public static BattleBenchmarkMetric FromValue(double value, string unit = "") =>
            new BattleBenchmarkMetric(true, value, unit);

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["unit"] = Unit,
                ["value"] = Available ? (object)Value : null,
            };
        }
    }

    public static class BattleRenderingBenchmarkSubmissionPolicy
    {
        public const int Unavailable = -1;

        public static int FromGraphicsDrawMeshCalls(bool callsIssued, int actualCallCount)
        {
            if (!callsIssued)
                return Unavailable;
            if (actualCallCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(actualCallCount),
                    "An available Graphics.DrawMesh submission count must contain at least one actual call.");
            }
            return actualCallCount;
        }
    }

    public static class BattleRenderingBenchmarkPassPolicy
    {
        public static bool Evaluate(
            bool countValidated,
            bool runtimeAdmissionValidated,
            bool logicTickMetricsValidated,
            bool determinismValidated,
            bool rendererWorkloadValidated,
            bool leakRequested,
            bool leakPassed)
        {
            return countValidated &&
                   runtimeAdmissionValidated &&
                   logicTickMetricsValidated &&
                   determinismValidated &&
                   rendererWorkloadValidated &&
                   (!leakRequested || leakPassed);
        }
    }

    public static class BattleRenderingBenchmarkEvidencePolicy
    {
        public static BattleBenchmarkMetricStatus ValidationStatus(bool? observedResult)
        {
            if (!observedResult.HasValue)
                return BattleBenchmarkMetricStatus.Missing;
            return observedResult.Value
                ? BattleBenchmarkMetricStatus.Passed
                : BattleBenchmarkMetricStatus.Failed;
        }
    }

    public static class BattleBenchmarkDrawCallPolicy
    {
        public static BattleBenchmarkMetric RequirePositiveForNonEmptyWorkload(
            BattleBenchmarkMetric metric)
        {
            return metric.Available && metric.Value <= 0d
                ? BattleBenchmarkMetric.Unavailable(metric.Unit)
                : metric;
        }
    }

    public readonly struct BattleRenderingBenchmarkLogicTickSample
    {
        internal BattleRenderingBenchmarkLogicTickSample(
            int tickIndex,
            BattleBenchmarkMetric elapsedMilliseconds,
            BattleBenchmarkMetric allocatedBytes,
            string checksum)
        {
            TickIndex = tickIndex;
            ElapsedMilliseconds = elapsedMilliseconds;
            AllocatedBytes = allocatedBytes;
            Checksum = checksum ?? string.Empty;
        }

        public int TickIndex { get; }
        public BattleBenchmarkMetric ElapsedMilliseconds { get; }
        public BattleBenchmarkMetric AllocatedBytes { get; }
        public string Checksum { get; }

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["allocatedBytes"] = AllocatedBytes.ToProjection(),
                ["checksum"] = Checksum,
                ["elapsedMilliseconds"] = ElapsedMilliseconds.ToProjection(),
                ["tickIndex"] = TickIndex,
            };
        }
    }

    public sealed class BattleBenchmarkMetricAvailability
    {
        internal BattleBenchmarkMetricAvailability(
            string metric,
            string source,
            bool available,
            string reason)
            : this(
                metric,
                required: false,
                BattleBenchmarkMetricApplicability.Applicable,
                available ? BattleBenchmarkMetricStatus.Available : BattleBenchmarkMetricStatus.Missing,
                "completed-frame",
                available ? 1 : 0,
                1,
                source,
                reason)
        {
        }

        public BattleBenchmarkMetricAvailability(
            string metric,
            bool required,
            BattleBenchmarkMetricApplicability applicability,
            BattleBenchmarkMetricStatus status,
            string scope,
            int sampleCount,
            int expectedSampleCount,
            string source,
            string reason)
        {
            Metric = metric ?? string.Empty;
            Required = required;
            Applicability = applicability;
            Status = status;
            Scope = scope ?? string.Empty;
            SampleCount = Math.Max(0, sampleCount);
            ExpectedSampleCount = Math.Max(0, expectedSampleCount);
            Source = source ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public string Metric { get; }
        public bool Required { get; }
        public BattleBenchmarkMetricApplicability Applicability { get; }
        public BattleBenchmarkMetricStatus Status { get; }
        public string Scope { get; }
        public int SampleCount { get; }
        public int ExpectedSampleCount { get; }
        public string Source { get; }
        public bool Available =>
            Status == BattleBenchmarkMetricStatus.Available ||
            Status == BattleBenchmarkMetricStatus.Passed;
        public string Reason { get; }

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["applicability"] = Applicability.ToString(),
                ["expectedSampleCount"] = ExpectedSampleCount,
                ["metric"] = Metric,
                ["reason"] = Reason,
                ["required"] = Required,
                ["sampleCount"] = SampleCount,
                ["scope"] = Scope,
                ["source"] = Source,
                ["status"] = Status.ToString(),
            };
        }
    }

    public readonly struct BattleRenderingBenchmarkPolicyContext
    {
        public BattleRenderingBenchmarkPolicyContext(
            bool isPlaying,
            bool isEditor,
            RuntimePlatform platform,
            bool graphicsMultiThreaded,
            bool frameTimingStatsEnabled)
        {
            IsPlaying = isPlaying;
            IsEditor = isEditor;
            Platform = platform;
            GraphicsMultiThreaded = graphicsMultiThreaded;
            FrameTimingStatsEnabled = frameTimingStatsEnabled;
        }

        public bool IsPlaying { get; }
        public bool IsEditor { get; }
        public RuntimePlatform Platform { get; }
        public bool GraphicsMultiThreaded { get; }
        public bool FrameTimingStatsEnabled { get; }
        public bool IsWindowsStandalone =>
            Platform == RuntimePlatform.WindowsPlayer;
        public bool IsSupportedExecutionScope =>
            IsPlaying && (IsEditor || IsWindowsStandalone);
        public string Scope => !IsPlaying
            ? "EditMode"
            : IsEditor
                ? "PlayModeEditor"
                : IsWindowsStandalone
                    ? "WindowsStandalone"
                    : Platform.ToString();

        internal static BattleRenderingBenchmarkPolicyContext Capture()
        {
            return new BattleRenderingBenchmarkPolicyContext(
                Application.isPlaying,
                Application.isEditor,
                Application.platform,
                SystemInfo.graphicsMultiThreaded,
                FrameTimingManager.IsFeatureEnabled());
        }
    }

    public static class BattleRenderingBenchmarkVerdictPolicy
    {
        public const string PolicyId = "ntsd-battle-rendering-benchmark-policy-v5";

        private static readonly string[] MandatoryMetricNames =
        {
            "frameTimeMs",
            "mainThreadTimeMs",
            "renderThreadTimeMs",
            "gpuFrameTimeMs",
            "managedAllocationBytes",
            "drawCalls",
            "totalAllocatedMemoryBytes",
            "graphicsMemoryBytes",
            "benchmarkOwnedTextureMemoryBytes",
            "logicTickTimeMs",
            "logicTickAllocatedBytes",
            "presentationBuildTimeMs",
            "presenterSubmittedRenderItems",
            "resourceSegments",
            "benchmarkOwnedMemoryBytes",
            "presenterSubmissionDrawCalls",
            "meshChunks",
            "exactSampleCount",
            "countValidated",
            "runtimeAdmissionValidated",
            "determinismValidated",
            "rendererWorkloadValidated",
            "leakCheck",
        };
        private static readonly IReadOnlyList<string> MandatoryMetricRegistry =
            Array.AsReadOnly(MandatoryMetricNames);

        public static IReadOnlyList<string> RequiredMetricNames => MandatoryMetricRegistry;

        public static BattleRenderingBenchmarkVerdict Evaluate(
            BattleRenderingBenchmarkPolicyContext context,
            IReadOnlyList<BattleBenchmarkMetricAvailability> metrics,
            out string reason,
            out string[] missingRequiredMetrics)
        {
            var missing = new List<string>();
            var failed = new List<string>();
            var metricCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var mandatoryNames = new HashSet<string>(MandatoryMetricNames, StringComparer.Ordinal);
            for (int index = 0; index < metrics.Count; index++)
            {
                BattleBenchmarkMetricAvailability metric = metrics[index];
                metricCounts.TryGetValue(metric.Metric, out int count);
                metricCounts[metric.Metric] = count + 1;
                if (!mandatoryNames.Contains(metric.Metric))
                    missing.Add(metric.Metric + " (unknown schema entry)");
                if (!metric.Required ||
                    metric.Applicability == BattleBenchmarkMetricApplicability.NotApplicable)
                {
                    continue;
                }

                if (metric.Status == BattleBenchmarkMetricStatus.Failed)
                    failed.Add(metric.Metric);
                else if (!metric.Available)
                    missing.Add(metric.Metric);
            }

            for (int index = 0; index < MandatoryMetricNames.Length; index++)
            {
                string metricName = MandatoryMetricNames[index];
                metricCounts.TryGetValue(metricName, out int count);
                if (count == 0)
                    missing.Add(metricName);
                else if (count > 1)
                    missing.Add(metricName + " (duplicate schema entry)");
            }
            missingRequiredMetrics = missing.ToArray();

            if (!context.IsPlaying)
            {
                reason = "EditMode does not provide a completed rendered-frame benchmark scope.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (!context.IsSupportedExecutionScope)
            {
                reason = "The v5 policy supports Play Mode Editor and Windows Standalone only.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (!context.FrameTimingStatsEnabled)
            {
                reason = "FrameTimingManager is disabled; completed-frame CPU/GPU metrics are unsupported.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (failed.Count > 0)
            {
                reason = "Required validation gates failed: " + string.Join(", ", failed) + ".";
                return BattleRenderingBenchmarkVerdict.Fail;
            }
            if (missing.Count > 0)
            {
                reason = "Required metrics are missing or incomplete: " + string.Join(", ", missing) + ".";
                return BattleRenderingBenchmarkVerdict.Incomplete;
            }

            reason = "All required v5 metrics and validation gates passed.";
            return BattleRenderingBenchmarkVerdict.Pass;
        }
    }

    public sealed class BattleRenderingBenchmarkFrame
    {
        internal BattleRenderingBenchmarkFrame(int frameIndex, int presentationEntityCount, int commandCount)
        {
            FrameIndex = frameIndex;
            PresentationEntityCount = presentationEntityCount;
            CommandCount = commandCount;
        }

        public int FrameIndex { get; }
        public int PresentationEntityCount { get; }
        public int CommandCount { get; }
        public BattleBenchmarkMetric FrameTimeMs { get; internal set; }
        public BattleBenchmarkMetric MainThreadTimeMs { get; internal set; }
        public BattleBenchmarkMetric RenderThreadTimeMs { get; internal set; }
        public BattleBenchmarkMetric GpuFrameTimeMs { get; internal set; }
        public BattleBenchmarkMetric LogicTickTimeMs { get; internal set; }
        public BattleBenchmarkMetric LogicTickAllocatedBytes { get; internal set; }
        public string LogicTickChecksum { get; internal set; } = string.Empty;
        public BattleBenchmarkMetric PresentationBuildTimeMs { get; internal set; }
        public BattleBenchmarkMetric ManagedAllocationBytes { get; internal set; }
        public BattleBenchmarkMetric DrawCalls { get; internal set; }
        public BattleBenchmarkMetric PresenterSubmittedRenderItems { get; internal set; }
        public BattleBenchmarkMetric PresenterSubmissionDrawCalls { get; internal set; }
        public BattleBenchmarkMetric TotalAllocatedMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric GraphicsMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric BenchmarkOwnedTextureMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric BenchmarkOwnedMemoryBytes { get; internal set; }
        public int BenchmarkResourceGeneration { get; internal set; }
        public BattleBenchmarkMetric SourceCommands { get; internal set; }
        public BattleBenchmarkMetric ResolvedCommands { get; internal set; }
        public BattleBenchmarkMetric UnresolvedCommands { get; internal set; }
        public BattleBenchmarkMetric ResourceSegments { get; internal set; }
        public BattleBenchmarkMetric MeshChunks { get; internal set; }
        public string RequestedBackend { get; internal set; } = string.Empty;
        public string EffectiveBackend { get; internal set; } = string.Empty;

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["presentationEntityCount"] = PresentationEntityCount,
                ["benchmarkOwnedMemoryBytes"] = BenchmarkOwnedMemoryBytes.ToProjection(),
                ["benchmarkOwnedTextureMemoryBytes"] = BenchmarkOwnedTextureMemoryBytes.ToProjection(),
                ["benchmarkResourceGeneration"] = BenchmarkResourceGeneration,
                ["commandCount"] = CommandCount,
                ["drawCalls"] = DrawCalls.ToProjection(),
                ["effectiveBackend"] = EffectiveBackend,
                ["frameIndex"] = FrameIndex,
                ["frameTimeMs"] = FrameTimeMs.ToProjection(),
                ["gpuFrameTimeMs"] = GpuFrameTimeMs.ToProjection(),
                ["graphicsMemoryBytes"] = GraphicsMemoryBytes.ToProjection(),
                ["logicTickTimeMs"] = LogicTickTimeMs.ToProjection(),
                ["logicTickAllocatedBytes"] = LogicTickAllocatedBytes.ToProjection(),
                ["logicTickChecksum"] = LogicTickChecksum,
                ["mainThreadTimeMs"] = MainThreadTimeMs.ToProjection(),
                ["managedAllocationBytes"] = ManagedAllocationBytes.ToProjection(),
                ["meshChunks"] = MeshChunks.ToProjection(),
                ["presentationBuildTimeMs"] = PresentationBuildTimeMs.ToProjection(),
                ["presenterSubmittedRenderItems"] = PresenterSubmittedRenderItems.ToProjection(),
                ["presenterSubmissionDrawCalls"] = PresenterSubmissionDrawCalls.ToProjection(),
                ["renderThreadTimeMs"] = RenderThreadTimeMs.ToProjection(),
                ["requestedBackend"] = RequestedBackend,
                ["resolvedCommands"] = ResolvedCommands.ToProjection(),
                ["resourceSegments"] = ResourceSegments.ToProjection(),
                ["sourceCommands"] = SourceCommands.ToProjection(),
                ["totalAllocatedMemoryBytes"] = TotalAllocatedMemoryBytes.ToProjection(),
                ["unresolvedCommands"] = UnresolvedCommands.ToProjection(),
            };
        }
    }

    public sealed class BattleRenderingBenchmarkLeakReport
    {
        internal BattleRenderingBenchmarkLeakReport(
            bool available,
            bool passed,
            int soakFrames,
            long prePresenterManaged,
            long prePresenterGraphics,
            bool prePresenterGraphicsAvailable,
            long managedStart,
            long managedEnd,
            long graphicsStart,
            long graphicsEnd,
            bool graphicsAvailable,
            long ownedStart,
            long ownedEnd,
            int resourcesStart,
            int resourcesEnd,
            long maxManagedGrowth,
            long maxGraphicsGrowth,
            int teardownFrames,
            long teardownManagedEnd,
            long teardownGraphicsEnd,
            bool teardownGraphicsAvailable,
            long teardownOwnedEnd,
            int teardownResourcesEnd,
            BattleBenchmarkMetricStatus teardownStatus,
            string teardownReason,
            string measurementMode,
            string reason)
        {
            Available = available;
            Passed = passed;
            SoakFrames = soakFrames;
            PrePresenterManagedBytes = prePresenterManaged;
            PrePresenterGraphicsBytes = prePresenterGraphics;
            PrePresenterGraphicsAvailable = prePresenterGraphicsAvailable;
            ManagedStartBytes = managedStart;
            ManagedEndBytes = managedEnd;
            GraphicsStartBytes = graphicsStart;
            GraphicsEndBytes = graphicsEnd;
            GraphicsAvailable = graphicsAvailable;
            OwnedStartBytes = ownedStart;
            OwnedEndBytes = ownedEnd;
            ResourcesStart = resourcesStart;
            ResourcesEnd = resourcesEnd;
            MaxManagedGrowthBytes = maxManagedGrowth;
            MaxGraphicsGrowthBytes = maxGraphicsGrowth;
            TeardownFrames = teardownFrames;
            TeardownManagedEndBytes = teardownManagedEnd;
            TeardownGraphicsEndBytes = teardownGraphicsEnd;
            TeardownGraphicsAvailable = teardownGraphicsAvailable;
            TeardownOwnedEndBytes = teardownOwnedEnd;
            TeardownResourcesEnd = teardownResourcesEnd;
            TeardownStatus = teardownStatus;
            TeardownReason = teardownReason ?? string.Empty;
            MeasurementMode = measurementMode ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public bool Available { get; }
        public bool Passed { get; }
        public int SoakFrames { get; }
        public long PrePresenterManagedBytes { get; }
        public long PrePresenterGraphicsBytes { get; }
        public bool PrePresenterGraphicsAvailable { get; }
        public long ManagedStartBytes { get; }
        public long ManagedEndBytes { get; }
        public long GraphicsStartBytes { get; }
        public long GraphicsEndBytes { get; }
        public bool GraphicsAvailable { get; }
        public long OwnedStartBytes { get; }
        public long OwnedEndBytes { get; }
        public int ResourcesStart { get; }
        public int ResourcesEnd { get; }
        public long MaxManagedGrowthBytes { get; }
        public long MaxGraphicsGrowthBytes { get; }
        public int TeardownFrames { get; }
        public long TeardownManagedEndBytes { get; }
        public long TeardownGraphicsEndBytes { get; }
        public bool TeardownGraphicsAvailable { get; }
        public long TeardownOwnedEndBytes { get; }
        public int TeardownResourcesEnd { get; }
        public BattleBenchmarkMetricStatus TeardownStatus { get; }
        public string TeardownReason { get; }
        public string MeasurementMode { get; }
        public string Reason { get; }
        public long ManagedGrowthBytes => ManagedEndBytes - ManagedStartBytes;
        public long GraphicsGrowthBytes => GraphicsEndBytes - GraphicsStartBytes;
        public long OwnedGrowthBytes => OwnedEndBytes - OwnedStartBytes;
        public long TeardownManagedGrowthBytes => TeardownManagedEndBytes - ManagedStartBytes;
        public long TeardownGraphicsGrowthBytes => TeardownGraphicsEndBytes - GraphicsStartBytes;

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["graphicsAvailable"] = GraphicsAvailable,
                ["graphicsEndBytes"] = GraphicsAvailable ? (object)GraphicsEndBytes : null,
                ["graphicsGrowthBytes"] = GraphicsAvailable ? (object)GraphicsGrowthBytes : null,
                ["graphicsStartBytes"] = GraphicsAvailable ? (object)GraphicsStartBytes : null,
                ["prePresenterGraphicsAvailable"] = PrePresenterGraphicsAvailable,
                ["prePresenterGraphicsBytes"] = PrePresenterGraphicsAvailable
                    ? (object)PrePresenterGraphicsBytes
                    : null,
                ["prePresenterManagedBytes"] = Available ? (object)PrePresenterManagedBytes : null,
                ["managedEndBytes"] = Available ? (object)ManagedEndBytes : null,
                ["managedGrowthBytes"] = Available ? (object)ManagedGrowthBytes : null,
                ["managedStartBytes"] = Available ? (object)ManagedStartBytes : null,
                ["maxGraphicsGrowthBytes"] = MaxGraphicsGrowthBytes,
                ["maxManagedGrowthBytes"] = MaxManagedGrowthBytes,
                ["measurementMode"] = MeasurementMode,
                ["ownedEndBytes"] = Available ? (object)OwnedEndBytes : null,
                ["ownedGrowthBytes"] = Available ? (object)OwnedGrowthBytes : null,
                ["ownedStartBytes"] = Available ? (object)OwnedStartBytes : null,
                ["passed"] = Available ? (object)Passed : null,
                ["reason"] = Reason,
                ["resourcesEnd"] = Available ? (object)ResourcesEnd : null,
                ["resourcesStart"] = Available ? (object)ResourcesStart : null,
                ["soakFrames"] = SoakFrames,
                ["teardownFrames"] = TeardownFrames,
                ["teardownGraphicsAvailable"] = TeardownGraphicsAvailable,
                ["teardownGraphicsEndBytes"] = TeardownGraphicsAvailable
                    ? (object)TeardownGraphicsEndBytes
                    : null,
                ["teardownGraphicsGrowthBytes"] = TeardownGraphicsAvailable
                    ? (object)TeardownGraphicsGrowthBytes
                    : null,
                ["teardownManagedEndBytes"] = Available ? (object)TeardownManagedEndBytes : null,
                ["teardownManagedGrowthBytes"] = Available ? (object)TeardownManagedGrowthBytes : null,
                ["teardownMemoryBaseline"] =
                    "steady-state soak baseline; pre-presenter fields are initialization diagnostics only",
                ["teardownOwnedEndBytes"] = Available ? (object)TeardownOwnedEndBytes : null,
                ["teardownReason"] = TeardownReason,
                ["teardownResourcesEnd"] = Available ? (object)TeardownResourcesEnd : null,
                ["teardownStatus"] = TeardownStatus.ToString(),
            };
        }

        internal static BattleRenderingBenchmarkLeakReport NotRequested()
        {
            return NotRun("Leak/long-run soak was not requested.", "not-requested", BattleBenchmarkMetricStatus.NotApplicable);
        }

        internal static BattleRenderingBenchmarkLeakReport NotRun(string reason)
        {
            return NotRun(reason, "not-run", BattleBenchmarkMetricStatus.Missing);
        }

        private static BattleRenderingBenchmarkLeakReport NotRun(
            string reason,
            string measurementMode,
            BattleBenchmarkMetricStatus teardownStatus)
        {
            return new BattleRenderingBenchmarkLeakReport(
                false,
                false,
                0,
                0L,
                0L,
                false,
                0L,
                0L,
                0L,
                0L,
                false,
                0L,
                0L,
                0,
                0,
                0L,
                0L,
                0,
                0L,
                0L,
                false,
                0L,
                0,
                teardownStatus,
                reason,
                measurementMode,
                reason);
        }
    }

        public sealed class BattleRenderingBenchmarkReport
    {
        internal BattleRenderingBenchmarkReport(
            BattleRenderingBenchmarkConfig config,
            BattleRenderingBenchmarkFrame[] frames,
            BattleBenchmarkMetricAvailability[] metricAvailability,
            BattleRenderingBenchmarkPolicyContext policyContext,
            int requestedPresentationEntityCount,
            int actualPresentationEntityCount,
            int commandCount,
            string workloadFingerprint,
            string presenterImplementation,
            string resourceMode,
            string drawMode,
            int benchmarkRenderTargetWidth,
            int benchmarkRenderTargetHeight,
            bool countValidated,
            bool runtimeAdmissionValidated,
            bool logicTickMetricsValidated,
            bool determinismValidated,
            bool rendererWorkloadValidated,
            BattleRenderingBenchmarkLeakReport leakReport)
        {
            Config = config;
            Frames = frames ?? Array.Empty<BattleRenderingBenchmarkFrame>();
            MetricAvailability = metricAvailability ?? Array.Empty<BattleBenchmarkMetricAvailability>();
            PolicyContext = policyContext;
            RequestedPresentationEntityCount = requestedPresentationEntityCount;
            ActualPresentationEntityCount = actualPresentationEntityCount;
            CommandCount = commandCount;
            WorkloadFingerprint = workloadFingerprint ?? string.Empty;
            PresenterImplementation = presenterImplementation ?? string.Empty;
            ResourceMode = resourceMode ?? string.Empty;
            DrawMode = drawMode ?? string.Empty;
            BenchmarkRenderTargetWidth = benchmarkRenderTargetWidth;
            BenchmarkRenderTargetHeight = benchmarkRenderTargetHeight;
            CountValidated = countValidated;
            RuntimeAdmissionValidated = runtimeAdmissionValidated;
            LogicTickMetricsValidated = logicTickMetricsValidated;
            DeterminismValidated = determinismValidated;
            RendererWorkloadValidated = rendererWorkloadValidated;
            LeakReport = leakReport ?? BattleRenderingBenchmarkLeakReport.NotRequested();
            Verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                PolicyContext,
                MetricAvailability,
                out string verdictReason,
                out string[] missingRequiredMetrics);
            VerdictReason = verdictReason;
            MissingRequiredMetrics = missingRequiredMetrics;
        }

        public BattleRenderingBenchmarkConfig Config { get; }
        public IReadOnlyList<BattleRenderingBenchmarkFrame> Frames { get; }
        public IReadOnlyList<BattleBenchmarkMetricAvailability> MetricAvailability { get; }
        internal BattleRenderingBenchmarkPolicyContext PolicyContext { get; }
        public int RequestedPresentationEntityCount { get; }
        public int ActualPresentationEntityCount { get; }
        public int CommandCount { get; }
        public string WorkloadFingerprint { get; }
        public string PresenterImplementation { get; }
        public string ResourceMode { get; }
        public string DrawMode { get; }
        public int BenchmarkRenderTargetWidth { get; }
        public int BenchmarkRenderTargetHeight { get; }
        public bool CountValidated { get; }
        public bool RuntimeAdmissionValidated { get; }
        public bool LogicTickMetricsValidated { get; }
        public bool DeterminismValidated { get; }
        public bool RendererWorkloadValidated { get; }
        public int RuntimeObjectCount { get; internal set; }
        public int RuntimeSlotCapacity { get; internal set; }
        public string RuntimeProfile { get; internal set; } = string.Empty;
        public int WarmupLogicTickCount { get; internal set; }
        public int SampleLogicTickCount { get; internal set; }
        public string InputFingerprint { get; internal set; } = string.Empty;
        public string InitialRuntimeChecksum { get; internal set; } = string.Empty;
        public string FinalRuntimeChecksum { get; internal set; } = string.Empty;
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> WarmupLogicTickSamples
        {
            get;
            internal set;
        } = Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> SampleLogicTickSamples
        {
            get;
            internal set;
        } = Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
        public BattleRenderingBenchmarkLeakReport LeakReport { get; }
        public BattleRenderingBenchmarkVerdict Verdict { get; }
        public string VerdictReason { get; }
        public IReadOnlyList<string> MissingRequiredMetrics { get; }
        public int CompletedFrameRejectedAttemptCount { get; internal set; }
        public int MaxCompletedFrameSampleAttempts { get; internal set; }
        public string CompletedFrameSamplingFailureReason { get; internal set; } = string.Empty;
        public bool Passed => Verdict == BattleRenderingBenchmarkVerdict.Pass;

        public string ToJson()
        {
            return BattleCanonicalJson.Serialize(ToProjection(true));
        }

        internal Dictionary<string, object> ToProjection(bool includeEnvironment)
        {
            var frameProjection = new List<object>(Frames.Count);
            for (int i = 0; i < Frames.Count; i++)
                frameProjection.Add(Frames[i].ToProjection());
            var availability = new List<object>(MetricAvailability.Count);
            var unavailable = new List<object>();
            for (int i = 0; i < MetricAvailability.Count; i++)
            {
                BattleBenchmarkMetricAvailability item = MetricAvailability[i];
                availability.Add(item.ToProjection());
                if (!item.Available)
                    unavailable.Add(item.Metric);
            }
            var missingRequired = new List<object>(MissingRequiredMetrics.Count);
            for (int index = 0; index < MissingRequiredMetrics.Count; index++)
                missingRequired.Add(MissingRequiredMetrics[index]);

            var config = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["backend"] = Config.Backend.ToString(),
                ["leakCheckFrames"] = Config.LeakCheckFrames,
                ["maxCompletedFrameSampleAttempts"] = BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts,
                ["sampleFrames"] = Config.SampleFrames,
                ["targetActiveEntities"] = Config.TargetActiveEntities,
                ["warmupFrames"] = Config.WarmupFrames,
            };
            var workload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["actualPresentationEntityCount"] = ActualPresentationEntityCount,
                ["commandCount"] = CommandCount,
                ["countValidated"] = CountValidated,
                ["runtimeAdmissionValidated"] = RuntimeAdmissionValidated,
                ["logicTickMetricsValidated"] = LogicTickMetricsValidated,
                ["determinismValidated"] = DeterminismValidated,
                ["fingerprint"] = WorkloadFingerprint,
                ["frozenPresentationFrame"] = true,
                ["gameplayRuntimeMutated"] = false,
                ["requestedPresentationEntityCount"] = RequestedPresentationEntityCount,
                ["rendererWorkloadValidated"] = RendererWorkloadValidated,
                ["runtimeObjectCount"] = RuntimeObjectCount,
                ["runtimeProfile"] = RuntimeProfile,
                ["runtimeSlotCapacity"] = RuntimeSlotCapacity,
                ["warmupLogicTickCount"] = WarmupLogicTickCount,
                ["sampleLogicTickCount"] = SampleLogicTickCount,
                ["inputFingerprint"] = InputFingerprint,
                ["initialRuntimeChecksum"] = InitialRuntimeChecksum,
                ["finalRuntimeChecksum"] = FinalRuntimeChecksum,
                ["scenario"] = Config.Scenario.Name,
                ["source"] = Config.Scenario.UsesCurrentScene
                    ? "current-scene-frozen-presentation-frame"
                    : "deterministic-mobileextended-runtime-fixture-v1",
                ["workloadKind"] = Config.Scenario.UsesCurrentScene
                    ? "frozen-current-scene-presentation"
                    : "frozen-real-runtime-presentation",
            };
            var warmupLogicTicks = new List<object>(WarmupLogicTickSamples.Count);
            for (int index = 0; index < WarmupLogicTickSamples.Count; index++)
                warmupLogicTicks.Add(WarmupLogicTickSamples[index].ToProjection());
            var sampleLogicTicks = new List<object>(SampleLogicTickSamples.Count);
            for (int index = 0; index < SampleLogicTickSamples.Count; index++)
                sampleLogicTicks.Add(SampleLogicTickSamples[index].ToProjection());
            var runtimeTrace = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["finalChecksum"] = FinalRuntimeChecksum,
                ["initialChecksum"] = InitialRuntimeChecksum,
                ["inputFingerprint"] = InputFingerprint,
                ["profile"] = RuntimeProfile,
                ["fixtureInput"] = "FrameInputSet.Empty for every logic tick",
                ["fixtureInteraction"] = Config.Scenario.UsesCurrentScene
                    ? "production current-scene runtime"
                    : "non-interacting LF2Entity fixtures with collision candidates explicitly suppressed",
                ["rngInitialSeed"] = Config.Scenario.UsesCurrentScene
                    ? "captured production state"
                    : "0x4E545344",
                ["runtimeObjectCount"] = RuntimeObjectCount,
                ["runtimeSlotCapacity"] = RuntimeSlotCapacity,
                ["sampleTicks"] = sampleLogicTicks,
                ["warmupTicks"] = warmupLogicTicks,
            };
            var limitations = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["drawAndGpuCounterScope"] =
                    "ProfilerRecorder frame counters include the complete Editor/Player frame; presenter-specific work is separately reported.",
                ["renderTargetScope"] =
                    $"Screen resolution describes the Editor/Player window; the benchmark presentation workload renders to {BenchmarkRenderTargetWidth}x{BenchmarkRenderTargetHeight}.",
                ["legacyPresenterScope"] =
                    "LegacyOnly uses a benchmark-only SpriteRenderer compatibility presenter because production battle prefabs are rendererless.",
                ["legacyVisualParityClaimed"] = false,
                ["logicTickMeasured"] = LogicTickMetricsValidated,
                ["logicTickReason"] = LogicTickMetricsValidated
                    ? "Full NTSDBattleTickSystem ticks were measured locally with Stopwatch and thread allocation counters."
                    : "No reliable full logic-tick sample was observed for this current-scene capture.",
                ["runtimeActiveEntityCapacityClaimed"] = RuntimeAdmissionValidated,
                ["runtimeActiveEntityLimitation"] = Config.Scenario.UsesCurrentScene
                    ? "The scene frame was frozen at benchmark start; runtime admission reflects the active production world at capture time."
                    : "Fixed scenarios register exactly the requested LF2Entity fixtures in a MobileExtended(1050) SimulationWorld.",
                ["productionAtlasPerformanceClaimed"] = false,
                ["productionAtlasLimitation"] =
                    "The deterministic A/B resolver uses one shared SourceTexture2D so both presenters consume identical drawable resources; production atlas modes require a separate current production-scene sample.",
                ["benchmarkOwnedTextureMemoryScope"] =
                    "benchmarkOwnedTextureMemoryBytes sums Profiler.GetRuntimeMemorySizeLong for the Texture2D and RenderTexture objects owned by the reported benchmarkResourceGeneration. It excludes global Editor/Player textures, production atlas resources, and non-texture presenter resources.",
            };
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["config"] = config,
                ["benchmarkRenderTargetHeight"] = BenchmarkRenderTargetHeight,
                ["benchmarkRenderTargetWidth"] = BenchmarkRenderTargetWidth,
                ["completedFrameSampling"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["acceptedSampleCount"] = Frames.Count,
                    ["maxAttemptsPerSample"] = MaxCompletedFrameSampleAttempts,
                    ["rejectedAttemptCount"] = CompletedFrameRejectedAttemptCount,
                    ["terminalFailureReason"] = string.IsNullOrEmpty(CompletedFrameSamplingFailureReason)
                        ? null
                        : (object)CompletedFrameSamplingFailureReason,
                },
                ["frames"] = frameProjection,
                ["leakCheck"] = LeakReport.ToProjection(),
                ["limitations"] = limitations,
                ["metricAvailability"] = availability,
                ["missingRequiredMetrics"] = missingRequired,
                ["policyId"] = BattleRenderingBenchmarkVerdictPolicy.PolicyId,
                ["drawMode"] = DrawMode,
                ["presenterImplementation"] = PresenterImplementation,
                ["resourceMode"] = ResourceMode,
                ["runtimeTrace"] = runtimeTrace,
                ["passed"] = Passed,
                ["reason"] = VerdictReason,
                ["schema"] = "ntsd-battle-rendering-benchmark-run-v5",
                ["summary"] = BuildSummary(),
                ["unavailableMetrics"] = unavailable,
                ["verdict"] = Verdict.ToString(),
                ["workload"] = workload,
            };
            if (includeEnvironment)
                root["environment"] = BattleRenderingBenchmarkEnvironment.Capture();
            return root;
        }

        private Dictionary<string, object> BuildSummary()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["benchmarkOwnedMemoryBytes"] = Summarize(frame => frame.BenchmarkOwnedMemoryBytes),
                ["benchmarkOwnedTextureMemoryBytes"] = Summarize(frame => frame.BenchmarkOwnedTextureMemoryBytes),
                ["drawCalls"] = Summarize(frame => frame.DrawCalls),
                ["frameTimeMs"] = Summarize(frame => frame.FrameTimeMs),
                ["gpuFrameTimeMs"] = Summarize(frame => frame.GpuFrameTimeMs),
                ["graphicsMemoryBytes"] = Summarize(frame => frame.GraphicsMemoryBytes),
                ["logicTickTimeMs"] = Summarize(frame => frame.LogicTickTimeMs),
                ["logicTickAllocatedBytes"] = Summarize(frame => frame.LogicTickAllocatedBytes),
                ["mainThreadTimeMs"] = Summarize(frame => frame.MainThreadTimeMs),
                ["managedAllocationBytes"] = Summarize(frame => frame.ManagedAllocationBytes),
                ["presentationBuildTimeMs"] = Summarize(frame => frame.PresentationBuildTimeMs),
                ["presenterSubmittedRenderItems"] = Summarize(frame => frame.PresenterSubmittedRenderItems),
                ["presenterSubmissionDrawCalls"] = Summarize(frame => frame.PresenterSubmissionDrawCalls),
                ["renderThreadTimeMs"] = Summarize(frame => frame.RenderThreadTimeMs),
                ["resourceSegments"] = Summarize(frame => frame.ResourceSegments),
                ["totalAllocatedMemoryBytes"] = Summarize(frame => frame.TotalAllocatedMemoryBytes),
            };
        }

        private Dictionary<string, object> Summarize(
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector)
        {
            int count = 0;
            double sum = 0d;
            double min = double.MaxValue;
            double max = double.MinValue;
            string unit = string.Empty;
            for (int index = 0; index < Frames.Count; index++)
            {
                BattleBenchmarkMetric metric = selector(Frames[index]);
                if (!metric.Available)
                    continue;
                count++;
                sum += metric.Value;
                min = Math.Min(min, metric.Value);
                max = Math.Max(max, metric.Value);
                unit = metric.Unit;
            }
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = count > 0,
                ["average"] = count > 0 ? (object)(sum / count) : null,
                ["maximum"] = count > 0 ? (object)max : null,
                ["minimum"] = count > 0 ? (object)min : null,
                ["sampleCount"] = count,
                ["unit"] = unit,
            };
        }

        public void WriteJson(string path)
        {
            BattleRenderingBenchmarkEnvironment.WriteJson(path, ToJson());
        }
    }

    public sealed class BattleRenderingBenchmarkSuiteReport
    {
        internal BattleRenderingBenchmarkSuiteReport(
            BattleRenderingBenchmarkConfig config,
            BattleRenderingBenchmarkReport[] runs,
            string workloadFingerprint)
        {
            Config = config;
            Runs = runs ?? Array.Empty<BattleRenderingBenchmarkReport>();
            WorkloadFingerprint = workloadFingerprint ?? string.Empty;
        }

        public BattleRenderingBenchmarkConfig Config { get; }
        public IReadOnlyList<BattleRenderingBenchmarkReport> Runs { get; }
        public string WorkloadFingerprint { get; }
        public BattleRenderingBenchmarkVerdict Verdict
        {
            get
            {
                if (Runs.Count == 0)
                    return BattleRenderingBenchmarkVerdict.Incomplete;
                bool unsupported = false;
                for (int index = 0; index < Runs.Count; index++)
                {
                    if (Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Fail)
                        return BattleRenderingBenchmarkVerdict.Fail;
                    if (Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Incomplete)
                        return BattleRenderingBenchmarkVerdict.Incomplete;
                    unsupported |= Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Unsupported;
                }
                return unsupported
                    ? BattleRenderingBenchmarkVerdict.Unsupported
                    : BattleRenderingBenchmarkVerdict.Pass;
            }
        }
        public bool Passed => Verdict == BattleRenderingBenchmarkVerdict.Pass;
        public string VerdictReason
        {
            get
            {
                if (Runs.Count == 0)
                    return "The suite contains no completed runs.";
                if (Passed)
                    return "All suite runs passed the v5 policy.";
                for (int index = 0; index < Runs.Count; index++)
                {
                    if (Runs[index].Verdict == Verdict)
                        return Runs[index].VerdictReason;
                }
                return "One or more suite runs did not pass.";
            }
        }

        public string ToJson()
        {
            var runProjection = new List<object>(Runs.Count);
            for (int index = 0; index < Runs.Count; index++)
                runProjection.Add(Runs[index].ToProjection(false));
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["comparison"] = Config.Comparison.ToString(),
                ["environment"] = BattleRenderingBenchmarkEnvironment.Capture(),
                ["missingRequiredMetrics"] = BuildMissingRequiredMetrics(),
                ["passed"] = Passed,
                ["policyId"] = BattleRenderingBenchmarkVerdictPolicy.PolicyId,
                ["reason"] = VerdictReason,
                ["runs"] = runProjection,
                ["schema"] = "ntsd-battle-rendering-benchmark-suite-v5",
                ["verdict"] = Verdict.ToString(),
                ["workloadFingerprint"] = WorkloadFingerprint,
            };
            return BattleCanonicalJson.Serialize(root);
        }

        private List<object> BuildMissingRequiredMetrics()
        {
            var result = new List<object>();
            var unique = new HashSet<string>(StringComparer.Ordinal);
            for (int runIndex = 0; runIndex < Runs.Count; runIndex++)
            {
                IReadOnlyList<string> missing = Runs[runIndex].MissingRequiredMetrics;
                for (int metricIndex = 0; metricIndex < missing.Count; metricIndex++)
                {
                    string qualified = Runs[runIndex].Config.Backend + ":" + missing[metricIndex];
                    if (unique.Add(qualified))
                        result.Add(qualified);
                }
            }
            return result;
        }

        public void WriteJson(string path)
        {
            BattleRenderingBenchmarkEnvironment.WriteJson(path, ToJson());
        }
    }

    public sealed class BattleRenderingBenchmarkWorkload
    {
        private BattleRenderingBenchmarkWorkload(
            BattlePresentationFrame frozenFrame,
            int requestedEntityCount,
            int actualEntityCount,
            string fingerprint,
            string source,
            int runtimeObjectCount,
            int runtimeSlotCapacity,
            string runtimeProfile,
            BattleRenderingBenchmarkLogicTickSample[] warmupLogicTickSamples,
            BattleRenderingBenchmarkLogicTickSample[] logicTickSamples,
            string inputFingerprint,
            string initialRuntimeChecksum,
            string finalRuntimeChecksum,
            bool runtimeAdmissionValidated,
            bool runtimeStateDeterministic)
        {
            FrozenFrame = frozenFrame ?? throw new ArgumentNullException(nameof(frozenFrame));
            RequestedEntityCount = requestedEntityCount;
            ActualEntityCount = actualEntityCount;
            Fingerprint = fingerprint ?? string.Empty;
            Source = source ?? string.Empty;
            RuntimeObjectCount = runtimeObjectCount;
            RuntimeSlotCapacity = runtimeSlotCapacity;
            RuntimeProfile = runtimeProfile ?? string.Empty;
            WarmupLogicTickSamples = warmupLogicTickSamples ??
                                     Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
            LogicTickSamples = logicTickSamples ?? Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
            InputFingerprint = inputFingerprint ?? string.Empty;
            InitialRuntimeChecksum = initialRuntimeChecksum ?? string.Empty;
            FinalRuntimeChecksum = finalRuntimeChecksum ?? string.Empty;
            RuntimeAdmissionValidated = runtimeAdmissionValidated;
            RuntimeStateDeterministic = runtimeStateDeterministic;
        }

        public BattlePresentationFrame FrozenFrame { get; }
        public int RequestedEntityCount { get; }
        public int ActualEntityCount { get; }
        public int CommandCount => FrozenFrame.CommandCount;
        public string Fingerprint { get; }
        public string Source { get; }
        public int RuntimeObjectCount { get; }
        public int RuntimeSlotCapacity { get; }
        public string RuntimeProfile { get; }
        public int WarmupTickCount => WarmupLogicTickSamples.Count;
        public int SampleTickCount => LogicTickSamples.Count;
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> WarmupLogicTickSamples { get; }
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> LogicTickSamples { get; }
        public string InputFingerprint { get; }
        public string InitialRuntimeChecksum { get; }
        public string FinalRuntimeChecksum { get; }
        public bool RuntimeAdmissionValidated { get; }
        public bool RuntimeStateDeterministic { get; }
        public bool LogicTickMetricsAvailable
        {
            get
            {
                if (LogicTickSamples.Count <= 0)
                    return false;
                return ValidateLogicSamples(WarmupLogicTickSamples) &&
                       ValidateLogicSamples(LogicTickSamples);
            }
        }

        private static bool ValidateLogicSamples(
            IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> samples)
        {
            for (int index = 0; index < samples.Count; index++)
            {
                if (!samples[index].ElapsedMilliseconds.Available ||
                    !samples[index].AllocatedBytes.Available ||
                    string.IsNullOrEmpty(samples[index].Checksum))
                {
                    return false;
                }
            }
            return true;
        }

        public static BattleRenderingBenchmarkWorkload Create(
            BattleRenderingBenchmarkScenario scenario,
            SimulationWorld world)
        {
            return Create(scenario, world, 0, 1);
        }

        public static BattleRenderingBenchmarkWorkload Create(
            BattleRenderingBenchmarkScenario scenario,
            SimulationWorld world,
            int warmupTickCount,
            int sampleTickCount)
        {
            if (warmupTickCount < 0)
                throw new ArgumentOutOfRangeException(nameof(warmupTickCount));
            if (sampleTickCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleTickCount));

            BattlePresentationFrame frame;
            int requested;
            string source;
            if (scenario.UsesCurrentScene)
            {
                BattlePresentationFrame published = world?.BattlePresentation?.PublishedFrame;
                if (published == null)
                {
                    throw new InvalidOperationException(
                        "The current-scene benchmark requires an active immutable presentation frame.");
                }
                if (published.EntityCount <= 0 || published.CommandCount <= 0)
                {
                    throw new InvalidOperationException(
                        "The current-scene benchmark refuses an empty entity or render-command workload.");
                }
                frame = CloneFrame(published);
                requested = published.EntityCount;
                source = "current-scene-frozen-presentation-frame";
                int runtimeObjectCount = world.ObjectCount;
                string checksum = CaptureRuntimeChecksum(world, published.TickIndex);
                string inputFingerprint = ComputeInputFingerprint(0, 0);
                string fingerprint = ComputeFingerprint(frame, requested, source);
                return new BattleRenderingBenchmarkWorkload(
                    frame,
                    requested,
                    frame.EntityCount,
                    fingerprint,
                    source,
                    runtimeObjectCount,
                    world.RuntimeSlotCapacity,
                    world.RuntimeProfileForServices.ToString(),
                    Array.Empty<BattleRenderingBenchmarkLogicTickSample>(),
                    Array.Empty<BattleRenderingBenchmarkLogicTickSample>(),
                    inputFingerprint,
                    checksum,
                    checksum,
                    runtimeObjectCount > 0,
                    false);
            }

            requested = scenario.RequestedEntityCount;
            return BuildRuntimeWorkload(requested, warmupTickCount, sampleTickCount);
        }

        private static BattleRenderingBenchmarkWorkload BuildRuntimeWorkload(
            int requested,
            int warmupTickCount,
            int sampleTickCount)
        {
            const int runtimeCapacity = BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity;
            var primaryWorld = new SimulationWorld(BattleRuntimeProfile.MobileExtended, runtimeCapacity);
            var mirrorWorld = new SimulationWorld(BattleRuntimeProfile.MobileExtended, runtimeCapacity);
            BattleRenderingBenchmarkEntity[] primaryEntities = RegisterRuntimeEntities(primaryWorld, requested);
            RegisterRuntimeEntities(mirrorWorld, requested);

            string initialPrimary = CaptureRuntimeChecksum(primaryWorld, 0);
            string initialMirror = CaptureRuntimeChecksum(mirrorWorld, 0);
            int totalTicks = checked(warmupTickCount + sampleTickCount);
            string inputFingerprint = ComputeInputFingerprint(warmupTickCount, sampleTickCount);
            var warmupSamples = new BattleRenderingBenchmarkLogicTickSample[warmupTickCount];
            var samples = new BattleRenderingBenchmarkLogicTickSample[sampleTickCount];
            var primaryTickSystem = new NTSDBattleTickSystem(primaryWorld);
            var mirrorTickSystem = new NTSDBattleTickSystem(mirrorWorld);
            int sampleIndex = 0;
            for (int tickIndex = 1; tickIndex <= totalTicks; tickIndex++)
            {
                FrameInputSet primaryInput = FrameInputSet.Empty(tickIndex);
                FrameInputSet mirrorInput = FrameInputSet.Empty(tickIndex);
                primaryWorld.ApplyFrameInputSet(primaryInput);
                mirrorWorld.ApplyFrameInputSet(mirrorInput);

                long allocationStart = GC.GetAllocatedBytesForCurrentThread();
                long started = Stopwatch.GetTimestamp();
                primaryTickSystem.RunReleaseTick(tickIndex);
                double elapsedMilliseconds = BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
                long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
                mirrorTickSystem.RunReleaseTick(tickIndex);

                string checksum = CaptureRuntimeChecksum(primaryWorld, tickIndex);
                var tickSample = new BattleRenderingBenchmarkLogicTickSample(
                    tickIndex,
                    BattleBenchmarkMetric.FromValue(elapsedMilliseconds, "ms"),
                    BattleBenchmarkMetric.FromValue(allocatedBytes, "bytes"),
                    checksum);
                if (tickIndex <= warmupTickCount)
                    warmupSamples[tickIndex - 1] = tickSample;
                else
                    samples[sampleIndex++] = tickSample;
            }

            string finalPrimary = CaptureRuntimeChecksum(primaryWorld, totalTicks);
            string finalMirror = CaptureRuntimeChecksum(mirrorWorld, totalTicks);
            bool admissionValidated = primaryWorld.ObjectCount == requested &&
                                      mirrorWorld.ObjectCount == requested;
            bool deterministic = admissionValidated &&
                                 string.Equals(initialPrimary, initialMirror, StringComparison.Ordinal) &&
                                 string.Equals(finalPrimary, finalMirror, StringComparison.Ordinal);
            if (!admissionValidated)
            {
                throw new InvalidOperationException(
                    $"Runtime benchmark admission mismatch: requested={requested}, " +
                    $"primary={primaryWorld.ObjectCount}, mirror={mirrorWorld.ObjectCount}.");
            }
            if (!deterministic)
            {
                throw new InvalidOperationException(
                    "The deterministic empty-input runtime fixture produced different checksums in its mirror world.");
            }

            BattlePresentationFrame frame = BuildRuntimeDerivedFrame(primaryWorld, primaryEntities, totalTicks);
            string source = "deterministic-mobileextended-runtime-fixture-v1";

            int actual = frame.EntityCount;
            if (actual != requested)
            {
                throw new InvalidOperationException(
                    $"Benchmark workload count mismatch: requested {requested}, built {actual}.");
            }
            if (frame.CommandCount <= 0)
                throw new InvalidOperationException("Benchmark workload contains no render commands.");
            string fingerprint = ComputeFingerprint(frame, requested, source);
            return new BattleRenderingBenchmarkWorkload(
                frame,
                requested,
                actual,
                fingerprint,
                source,
                primaryWorld.ObjectCount,
                primaryWorld.RuntimeSlotCapacity,
                primaryWorld.RuntimeProfileForServices.ToString(),
                warmupSamples,
                samples,
                inputFingerprint,
                initialPrimary,
                finalPrimary,
                admissionValidated,
                deterministic);
        }

        private static BattlePresentationFrame CloneFrame(BattlePresentationFrame source)
        {
            var frame = new BattlePresentationFrame();
            frame.Reset(source.TickIndex);
            frame.EnsureEntityCapacity(source.EntityCount);
            frame.EnsureHitRecordCapacity(source.HitRecordCount);
            frame.EnsureCommandCapacity(source.CommandCount);
            for (int index = 0; index < source.EntityCount; index++)
                frame.AddEntity(source.GetEntity(index));
            for (int index = 0; index < source.HitRecordCount; index++)
                frame.AddHitRecord(source.GetHitRecord(index));
            for (int index = 0; index < source.CommandCount; index++)
                frame.AddCommand(source.GetCommand(index));
            frame.OverlayUnsupportedCount = source.OverlayUnsupportedCount;
            return frame;
        }

        private static BattleRenderingBenchmarkEntity[] RegisterRuntimeEntities(
            SimulationWorld world,
            int entityCount)
        {
            var entities = new BattleRenderingBenchmarkEntity[entityCount];
            for (int index = 0; index < entityCount; index++)
            {
                int column = index % 40;
                int row = index / 40;
                var entity = new BattleRenderingBenchmarkEntity(
                    index + 1,
                    40 + column * 16,
                    200 + row * 4);
                world.Register(entity);
                if (entity.Runtime.SlotIndex < 50)
                {
                    throw new InvalidOperationException(
                        $"Runtime benchmark fixture {index} was not assigned a valid dynamic slot.");
                }
                entities[index] = entity;
            }
            return entities;
        }

        private static BattlePresentationFrame BuildRuntimeDerivedFrame(
            SimulationWorld world,
            BattleRenderingBenchmarkEntity[] entities,
            int tickIndex)
        {
            var frame = new BattlePresentationFrame();
            frame.Reset(tickIndex);
            frame.EnsureEntityCapacity(entities.Length);
            frame.EnsureCommandCapacity(checked(entities.Length * 2));
            for (int index = 0; index < entities.Length; index++)
            {
                BattleRenderingBenchmarkEntity entity = entities[index];
                if (!world.TryGetCurrentRuntimeHandle(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    throw new InvalidOperationException(
                        $"Runtime benchmark fixture lost its generation-aware handle at index {index}.");
                }
                int stableId = entity.Runtime.StableId;
                int runtimeSlot = entity.Runtime.SlotIndex;
                int logicalZ = entity.Runtime.ZInt;
                Vector3 position = NTSDRenderSpace.ScreenPixelToWorld(
                    entity.Runtime.XInt,
                    logicalZ,
                    logicalZ * 0.001f);
                int baseOrder = checked(index * 4);
                frame.AddEntity(new BattlePresentationEntitySnapshot(
                    handle,
                    stableId,
                    entity.ObjectId,
                    entity.GetCurrentDataObjectTypeForSimulation(),
                    0,
                    logicalZ,
                    runtimeSlot,
                    baseOrder,
                    0,
                    true,
                    0,
                    0,
                    0,
                    0,
                    0,
                    entity.Runtime.XInt,
                    logicalZ,
                    position.z,
                    0f,
                    0,
                    0,
                    8f,
                    8f,
                    16f,
                    16f,
                    Vector2.zero,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    (index & 1) != 0,
                    false,
                    default,
                    0,
                    0));
                AddSyntheticCommand(
                    frame,
                    BattleRenderCommandType.Shadow,
                    handle,
                    stableId,
                    runtimeSlot,
                    baseOrder,
                    position + new Vector3(0f, -0.04f, 0f),
                    0,
                    false);
                AddSyntheticCommand(
                    frame,
                    BattleRenderCommandType.Entity,
                    handle,
                    stableId,
                    runtimeSlot,
                    baseOrder,
                    position,
                    1,
                    (index & 1) != 0);
            }
            return frame;
        }

        private static string CaptureRuntimeChecksum(SimulationWorld world, int tickIndex)
        {
            if (world == null)
                return string.Empty;
            FrameInputSet input = FrameInputSet.Empty(tickIndex);
            if (world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended)
            {
                return world.CaptureExtendedChecksumSnapshot(tickIndex, input).OverallChecksum;
            }
            return world.CaptureParityFrameSnapshot(tickIndex, input).OverallChecksum;
        }

        private static string ComputeInputFingerprint(int warmupTickCount, int sampleTickCount)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                Hash(ref hash, warmupTickCount);
                Hash(ref hash, sampleTickCount);
                for (int tickIndex = 1; tickIndex <= warmupTickCount + sampleTickCount; tickIndex++)
                {
                    Hash(ref hash, tickIndex);
                    Hash(ref hash, 0);
                }
                return hash.ToString("x16");
            }
        }

        private static void AddSyntheticCommand(
            BattlePresentationFrame frame,
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int runtimeSlot,
            int baseOrder,
            Vector3 position,
            int localSequence,
            bool flipX)
        {
            frame.AddCommand(new BattleRenderCommand(
                type,
                handle,
                stableId,
                1,
                0,
                runtimeSlot / 40,
                runtimeSlot,
                baseOrder + localSequence,
                0,
                localSequence,
                position,
                new Vector2(16f, 16f),
                new Vector2(0.5f, 0.5f),
                new Rect(0f, 0f, 1f, 1f),
                BattleSpriteRenderState.Default(flipX),
                default));
        }

        private static string ComputeFingerprint(
            BattlePresentationFrame frame,
            int requested,
            string source)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                Hash(ref hash, requested);
                Hash(ref hash, frame.EntityCount);
                Hash(ref hash, frame.CommandCount);
                for (int index = 0; index < source.Length; index++)
                    Hash(ref hash, source[index]);
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    Hash(ref hash, (int)command.Type);
                    Hash(ref hash, command.Handle.Slot);
                    Hash(ref hash, unchecked((int)command.Handle.Generation));
                    Hash(ref hash, command.StableId);
                    Hash(ref hash, command.RuntimeSlot);
                    Hash(ref hash, command.SortOrder);
                    Hash(ref hash, command.LocalSequence);
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.x));
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.y));
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.z));
                    Hash(ref hash, command.FlipX ? 1 : 0);
                }
                return hash.ToString("x16");
            }
        }

        private static void Hash(ref ulong hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 1099511628211UL;
            }
        }
    }

    internal sealed class BattleRenderingBenchmarkEntity : LF2Entity
    {
        public BattleRenderingBenchmarkEntity(int stableId, int x, int z)
        {
            StableId = stableId;
            ObjectId = 10000 + stableId;
            Team = 0;
            Health = new LF2Health();
            Health.BindRuntime(Runtime);
            Health.HP = 500;
            Health.HPBound = 500;
            ItrRest = new LF2ItrRestTracker();
            PS.BindRuntime(Runtime);
            Trans = new FrameTransistor(this);
            Frame.D = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 999,
                wait = 1000000,
                next = 0,
                centerx = 8,
                centery = 8,
            };
            Frame.N = 0;
            Frame.PN = 0;
            Frame.Prev = 0;
            Runtime.X = x;
            Runtime.Y = 0;
            Runtime.Z = z;
            Runtime.SuppressCollisionCandidateUntilTick = int.MaxValue;
            Runtime.SyncIntegerPosition();
            RefreshRuntimeSnapshot();
        }

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

        internal override bool UsesDynamicRuntimeSlot() => true;

        public override int GetCurrentDataObjectTypeForSimulation() => (int)LF2ObjectType.Other;

        public override void RunFrameLogicBeforeAdvance()
        {
        }

        public override void SimTransit(int tickIndex)
        {
        }

        public override void SimTU(int tickIndex)
        {
        }

        public override void SimPostInteraction(int tickIndex)
        {
        }

        public override void SimObjectInteraction(int tickIndex)
        {
        }

        public override void SimPreInteraction(int tickIndex)
        {
        }

        public override void SimEntityCollision(int tickIndex)
        {
        }

        public override void SimFrameTick(int tickIndex)
        {
        }

        public override void SimLateTick(int tickIndex)
        {
        }

        public override void Reset()
        {
        }

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
        }
    }

    public readonly struct BattleBenchmarkCompletedFrameMetrics
    {
        public BattleBenchmarkCompletedFrameMetrics(
            BattleBenchmarkMetric frameTimeMs,
            BattleBenchmarkMetric mainThreadTimeMs,
            BattleBenchmarkMetric renderThreadTimeMs,
            BattleBenchmarkMetric gpuFrameTimeMs,
            BattleBenchmarkMetric managedAllocationBytes,
            BattleBenchmarkMetric drawCalls,
            BattleBenchmarkMetric totalAllocatedMemoryBytes,
            BattleBenchmarkMetric graphicsMemoryBytes)
        {
            FrameTimeMs = frameTimeMs;
            MainThreadTimeMs = mainThreadTimeMs;
            RenderThreadTimeMs = renderThreadTimeMs;
            GpuFrameTimeMs = gpuFrameTimeMs;
            ManagedAllocationBytes = managedAllocationBytes;
            DrawCalls = drawCalls;
            TotalAllocatedMemoryBytes = totalAllocatedMemoryBytes;
            GraphicsMemoryBytes = graphicsMemoryBytes;
        }

        public BattleBenchmarkMetric FrameTimeMs { get; }
        public BattleBenchmarkMetric MainThreadTimeMs { get; }
        public BattleBenchmarkMetric RenderThreadTimeMs { get; }
        public BattleBenchmarkMetric GpuFrameTimeMs { get; }
        public BattleBenchmarkMetric ManagedAllocationBytes { get; }
        public BattleBenchmarkMetric DrawCalls { get; }
        public BattleBenchmarkMetric TotalAllocatedMemoryBytes { get; }
        public BattleBenchmarkMetric GraphicsMemoryBytes { get; }

        internal static BattleBenchmarkCompletedFrameMetrics Unavailable()
        {
            return new BattleBenchmarkCompletedFrameMetrics(
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("bytes"),
                BattleBenchmarkMetric.Unavailable("count"),
                BattleBenchmarkMetric.Unavailable("bytes"),
                BattleBenchmarkMetric.Unavailable("bytes"));
        }
    }

    public interface IBattleBenchmarkCompletedFrameCollector : IDisposable
    {
        bool IsSupported { get; }
        string UnsupportedReason { get; }
        void Request(int generation);
        bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics);
        string Source(BattleBenchmarkRecorderKind kind);
        string Reason(BattleBenchmarkRecorderKind kind);
        void Reset();
    }

    public sealed class BattleBenchmarkInjectedCompletedFrameCollector :
        IBattleBenchmarkCompletedFrameCollector
    {
        private readonly BattleBenchmarkCompletedFrameMetrics metrics;
        private int pendingGeneration;

        public BattleBenchmarkInjectedCompletedFrameCollector(
            BattleBenchmarkCompletedFrameMetrics completedFrameMetrics)
        {
            metrics = completedFrameMetrics;
        }

        public bool IsSupported => true;
        public string UnsupportedReason => string.Empty;

        public void Request(int generation)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
        }

        public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics result)
        {
            if (pendingGeneration != generation)
            {
                result = default;
                return false;
            }
            pendingGeneration = 0;
            result = metrics;
            return true;
        }

        public string Source(BattleBenchmarkRecorderKind kind) => "injected-completed-frame-test-sample";
        public string Reason(BattleBenchmarkRecorderKind kind) => string.Empty;
        public void Reset() => pendingGeneration = 0;
        public void Dispose() => Reset();
    }

    public interface IBattleRenderingBenchmarkRunSession : IDisposable
    {
        bool CaptureFrame();
        BattleRenderingBenchmarkReport Report { get; }
    }

    public interface IBattleBenchmarkLeakProbe
    {
        long CaptureRetainedManagedHeapBytes();
        BattleBenchmarkMetric CaptureGraphicsMemory();
        int CurrentUnityFrame { get; }
        bool RequiresDeferredDestructionWait { get; }
        void BeginPostDisposeCleanup();
        bool IsPostDisposeCleanupComplete { get; }
        void CompletePostDisposeCleanup();
    }

    internal sealed class BattleBenchmarkUnityLeakProbe : IBattleBenchmarkLeakProbe
    {
        private AsyncOperation postDisposeUnload;

        public int CurrentUnityFrame => Time.frameCount;
        public bool RequiresDeferredDestructionWait => Application.isPlaying;
        public bool IsPostDisposeCleanupComplete =>
            postDisposeUnload == null || postDisposeUnload.isDone;

        public long CaptureRetainedManagedHeapBytes()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return GC.GetTotalMemory(false);
        }

        public BattleBenchmarkMetric CaptureGraphicsMemory()
        {
            return BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
        }

        public void BeginPostDisposeCleanup()
        {
            if (!Application.isPlaying)
                return;
            GL.Flush();
            postDisposeUnload = Resources.UnloadUnusedAssets();
        }

        public void CompletePostDisposeCleanup()
        {
            if (Application.isPlaying)
                GL.Flush();
        }
    }

    public sealed class BattleRenderingBenchmarkSession : IBattleRenderingBenchmarkRunSession
    {
        private readonly BattleRenderingBenchmarkConfig config;
        private readonly SimulationWorld world;
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleRenderingBenchmarkPolicyContext policyContext;
        private readonly IBattleBenchmarkCompletedFrameCollector completedFrameCollector;
        private readonly IBattleRenderingBenchmarkPresenter presenter;
        private readonly IBattleBenchmarkLeakProbe leakProbe;
        private readonly List<BattleRenderingBenchmarkFrame> frames;
        private readonly string presenterImplementation;
        private readonly string presenterResourceMode;
        private readonly string presenterDrawMode;
        private readonly string presenterSubmissionDrawMetricSource;
        private readonly string presenterSubmissionDrawUnavailableReason;
        private readonly int presenterRenderTargetWidth;
        private readonly int presenterRenderTargetHeight;
        private readonly int presenterResourceGeneration;
        private readonly int presenterOwnedTextureResourceCount;
        private static int nextGeneration;
        private BattleRenderingBenchmarkReport report;
        private BattleRenderingBenchmarkLeakReport leakReport;
        private bool disposed;
        private int frameIndex;
        private int pendingGeneration;
        private int pendingSampleIndex;
        private bool pendingWarmup;
        private bool pendingLeakFrame;
        private int pendingSampleAttempt;
        private double pendingPresentationMs;
        private int completedFrameRejectedAttemptCount;
        private string completedFrameSamplingFailureReason = string.Empty;
        private int leakFramesCaptured;
        private bool leakBaselineCaptured;
        private bool presenterDisposed;
        private bool teardownPending;
        private int teardownStartedFrame;
        private int teardownFramesWaited;
        private bool teardownCleanupRequested;
        private bool teardownCleanupCompleted;
        private int teardownCleanupCompletedFrame;
        private long leakPrePresenterManaged;
        private long leakPrePresenterGraphics;
        private bool leakPrePresenterGraphicsAvailable;
        private long leakManagedStart;
        private long leakGraphicsStart;
        private bool leakGraphicsAvailable;
        private long leakOwnedStart;
        private int leakResourcesStart;
        private long leakManagedEnd;
        private long leakGraphicsEnd;
        private bool leakSoakGraphicsAvailable;
        private long leakOwnedEnd;
        private int leakResourcesEnd;

        public const string RetainedManagedHeapMeasurementMode =
            "full-gc-retained-managed-heap-outside-performance-sample-window-v1";
        public const int DeferredDestructionPlayFrames = 2;
        public const int PostDisposeCleanupPlayFrames = 2;
        public const int MaxPostDisposeCleanupPlayFrames = 120;
        public const int MaxCompletedFrameSampleAttempts = 16;

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world)
            : this(
                config,
                world,
                BattleRenderingBenchmarkWorkload.Create(
                    config.Scenario,
                    world,
                    config.WarmupFrames,
                    config.SampleFrames))
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload)
            : this(
                config,
                world,
                workload,
                BattleRenderingBenchmarkPolicyContext.Capture(),
                null,
                null)
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload,
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext,
            IBattleBenchmarkCompletedFrameCollector collector,
            IBattleRenderingBenchmarkPresenter benchmarkPresenter)
            : this(
                config,
                world,
                workload,
                benchmarkPolicyContext,
                collector,
                benchmarkPresenter,
                null)
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload,
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext,
            IBattleBenchmarkCompletedFrameCollector collector,
            IBattleRenderingBenchmarkPresenter benchmarkPresenter,
            IBattleBenchmarkLeakProbe benchmarkLeakProbe)
        {
            if (config.Comparison != BattleRenderingBenchmarkComparison.Single)
                throw new ArgumentException("A single run session requires Single comparison mode.", nameof(config));
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            policyContext = benchmarkPolicyContext;
            leakProbe = benchmarkLeakProbe ?? new BattleBenchmarkUnityLeakProbe();
            ValidateCount();
            frames = new List<BattleRenderingBenchmarkFrame>(config.SampleFrames);
            completedFrameCollector = collector ??
                new BattleBenchmarkUnityCompletedFrameCollector(policyContext);
            IBattleRenderingBenchmarkPresenter presenterCandidate = benchmarkPresenter;
            try
            {
                if (config.LeakCheckFrames > 0)
                    CapturePrePresenterLeakBaseline();
                presenterCandidate = presenterCandidate ??
                                     BattleRenderingBenchmarkPresenterFactory.Create(config.Backend, workload);
                presenter = presenterCandidate;
                ValidatePresenterWorkload();
                presenterImplementation = presenter.Implementation;
                presenterResourceMode = presenter.ResourceMode;
                presenterDrawMode = presenter.DrawMode;
                presenterSubmissionDrawMetricSource = presenter.SubmissionDrawMetricSource;
                presenterSubmissionDrawUnavailableReason = presenter.SubmissionDrawUnavailableReason;
                presenterRenderTargetWidth = presenter.RenderTargetWidth;
                presenterRenderTargetHeight = presenter.RenderTargetHeight;
                presenterResourceGeneration = presenter.ResourceGeneration;
                presenterOwnedTextureResourceCount = presenter.OwnedTextureResourceCount;
            }
            catch
            {
                try
                {
                    presenterCandidate?.Dispose();
                }
                catch (Exception cleanupException)
                {
                    UnityEngine.Debug.LogException(cleanupException);
                }
                finally
                {
                    completedFrameCollector.Dispose();
                }
                throw;
            }
        }

        public BattleRenderingBenchmarkConfig Config => config;
        public bool IsComplete => report != null;
        public BattleRenderingBenchmarkReport Report => report;
        public bool IsDisposed => disposed;
        public int WarmupFramesCaptured => Math.Min(frameIndex, config.WarmupFrames);
        public int SampleFramesCaptured => frames.Count;
        public int LeakFramesCaptured => leakFramesCaptured;
        public BattleRenderingBenchmarkWorkload Workload => workload;

        public bool CaptureFrame()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleRenderingBenchmarkSession));
            if (report != null)
                return true;

            if (pendingGeneration != 0)
            {
                if (!completedFrameCollector.TryDrain(
                        pendingGeneration,
                        out BattleBenchmarkCompletedFrameMetrics completedMetrics))
                {
                    return false;
                }

                bool completedLeakFrame = pendingLeakFrame;
                if (completedLeakFrame)
                    leakFramesCaptured++;
                else if (!pendingWarmup)
                {
                    BattleRenderingBenchmarkFrame sample =
                        CaptureSample(pendingSampleIndex, pendingPresentationMs, completedMetrics);
                    if (!HasAllApplicableFormalSampleMetrics(sample, out string rejectionReason))
                    {
                        int rejectedGeneration = pendingGeneration;
                        completedFrameRejectedAttemptCount++;
                        pendingGeneration = 0;
                        pendingWarmup = false;
                        pendingLeakFrame = false;
                        if (pendingSampleAttempt < MaxCompletedFrameSampleAttempts)
                        {
                            pendingSampleAttempt++;
                            BeginCompletedFrameRequest();
                            return false;
                        }

                        completedFrameSamplingFailureReason =
                            $"Formal sample {pendingSampleIndex} exhausted {MaxCompletedFrameSampleAttempts} " +
                            $"completed-frame attempts; last generation {rejectedGeneration}: {rejectionReason}";
                        leakReport = BattleRenderingBenchmarkLeakReport.NotRun(
                            "Leak/long-run soak was not run because " + completedFrameSamplingFailureReason);
                        FinalizeReport();
                        return true;
                    }
                    frames.Add(sample);
                    pendingSampleAttempt = 0;
                }
                pendingGeneration = 0;
                pendingWarmup = false;
                pendingLeakFrame = false;

                if (completedLeakFrame)
                {
                    if (leakFramesCaptured < config.LeakCheckFrames)
                        return false;
                    BeginLeakTeardown();
                    return false;
                }

                if (frames.Count < config.SampleFrames)
                    return false;
                if (config.LeakCheckFrames <= 0)
                {
                    leakReport = BattleRenderingBenchmarkLeakReport.NotRequested();
                    FinalizeReport();
                    return true;
                }
                CaptureLeakBaseline();
                return false;
            }

            if (teardownPending)
            {
                teardownFramesWaited = Math.Max(0, leakProbe.CurrentUnityFrame - teardownStartedFrame);
                if (leakProbe.RequiresDeferredDestructionWait &&
                    teardownFramesWaited < DeferredDestructionPlayFrames)
                {
                    return false;
                }

                if (!teardownCleanupRequested)
                {
                    teardownCleanupRequested = true;
                    leakProbe.BeginPostDisposeCleanup();
                    return false;
                }

                if (!leakProbe.IsPostDisposeCleanupComplete)
                {
                    if (leakProbe.RequiresDeferredDestructionWait &&
                        teardownFramesWaited >= MaxPostDisposeCleanupPlayFrames)
                    {
                        FinalizeLeakReport(
                            "Post-Dispose Unity cleanup did not complete within " +
                            MaxPostDisposeCleanupPlayFrames + " Play frames.");
                        FinalizeReport();
                        return true;
                    }
                    return false;
                }

                if (!teardownCleanupCompleted)
                {
                    teardownCleanupCompleted = true;
                    teardownCleanupCompletedFrame = leakProbe.CurrentUnityFrame;
                    leakProbe.CompletePostDisposeCleanup();
                    return false;
                }

                if (leakProbe.RequiresDeferredDestructionWait &&
                    leakProbe.CurrentUnityFrame - teardownCleanupCompletedFrame <
                    PostDisposeCleanupPlayFrames)
                {
                    return false;
                }

                FinalizeLeakReport();
                FinalizeReport();
                return true;
            }

            if (frames.Count < config.SampleFrames)
            {
                ValidateCount();
                int currentFrame = frameIndex;
                pendingWarmup = currentFrame < config.WarmupFrames;
                pendingSampleIndex = frames.Count;
                pendingLeakFrame = false;
                pendingSampleAttempt = 1;
                BeginCompletedFrameRequest();
                frameIndex++;
                return false;
            }

            if (!leakBaselineCaptured)
                CaptureLeakBaseline();
            ValidateCount();
            pendingWarmup = false;
            pendingLeakFrame = true;
            pendingSampleAttempt = 0;
            BeginCompletedFrameRequest();
            return false;
        }

        private void BeginCompletedFrameRequest()
        {
            pendingGeneration = Interlocked.Increment(ref nextGeneration);
            if (pendingGeneration == 0)
                pendingGeneration = Interlocked.Increment(ref nextGeneration);
            try
            {
                completedFrameCollector.Request(pendingGeneration);
                pendingPresentationMs = presenter.Present();
                ValidatePresenterWorkload();
            }
            catch
            {
                completedFrameCollector.Reset();
                pendingGeneration = 0;
                pendingWarmup = false;
                pendingLeakFrame = false;
                pendingSampleAttempt = 0;
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            try
            {
                DisposePresenter();
            }
            finally
            {
                completedFrameCollector.Dispose();
            }
        }

        private BattleRenderingBenchmarkFrame CaptureSample(
            int index,
            double presentationMs,
            BattleBenchmarkCompletedFrameMetrics completedMetrics)
        {
            BattleCentralBuildDiagnostics diagnostics = presenter.Diagnostics;
            BattleBenchmarkMetric logicTickTime;
            BattleBenchmarkMetric logicTickAllocatedBytes;
            if (index < workload.LogicTickSamples.Count)
            {
                BattleRenderingBenchmarkLogicTickSample logicSample = workload.LogicTickSamples[index];
                logicTickTime = logicSample.ElapsedMilliseconds;
                logicTickAllocatedBytes = logicSample.AllocatedBytes;
            }
            else
            {
                logicTickTime = BattleBenchmarkMetric.Unavailable("ms");
                logicTickAllocatedBytes = BattleBenchmarkMetric.Unavailable("bytes");
            }
            var frame = new BattleRenderingBenchmarkFrame(
                index,
                workload.ActualEntityCount,
                workload.CommandCount)
            {
                FrameTimeMs = completedMetrics.FrameTimeMs,
                MainThreadTimeMs = completedMetrics.MainThreadTimeMs,
                RenderThreadTimeMs = completedMetrics.RenderThreadTimeMs,
                GpuFrameTimeMs = completedMetrics.GpuFrameTimeMs,
                LogicTickTimeMs = logicTickTime,
                LogicTickAllocatedBytes = logicTickAllocatedBytes,
                LogicTickChecksum = index < workload.LogicTickSamples.Count
                    ? workload.LogicTickSamples[index].Checksum
                    : string.Empty,
                PresentationBuildTimeMs = BattleBenchmarkMetric.FromValue(presentationMs, "ms"),
                ManagedAllocationBytes = completedMetrics.ManagedAllocationBytes,
                DrawCalls = BattleBenchmarkDrawCallPolicy.RequirePositiveForNonEmptyWorkload(
                    completedMetrics.DrawCalls),
                PresenterSubmittedRenderItems = BattleBenchmarkMetric.FromValue(
                    presenter.MaterializedRenderItemCount,
                    "count"),
                PresenterSubmissionDrawCalls = presenter.SubmissionDrawCount >= 0
                    ? BattleBenchmarkMetric.FromValue(presenter.SubmissionDrawCount, "count")
                    : BattleBenchmarkMetric.Unavailable("count"),
                TotalAllocatedMemoryBytes = completedMetrics.TotalAllocatedMemoryBytes,
                GraphicsMemoryBytes = completedMetrics.GraphicsMemoryBytes,
                BenchmarkOwnedTextureMemoryBytes = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                    presenterResourceGeneration,
                    presenterOwnedTextureResourceCount,
                    presenter.MeasureOwnedTextureMemoryBytes(),
                    out _),
                BenchmarkOwnedMemoryBytes = BattleBenchmarkMetric.FromValue(
                    presenter.CachedOwnedResourceMemoryBytes,
                    "bytes"),
                BenchmarkResourceGeneration = presenterResourceGeneration,
                SourceCommands = BattleBenchmarkMetric.FromValue(workload.CommandCount, "count"),
                ResolvedCommands = BattleBenchmarkMetric.FromValue(presenter.ResolvedCommandCount, "count"),
                UnresolvedCommands = BattleBenchmarkMetric.FromValue(
                    workload.CommandCount - presenter.ResolvedCommandCount,
                    "count"),
                ResourceSegments = BattleBenchmarkMetric.FromValue(presenter.ResourceSegmentCount, "count"),
                MeshChunks = diagnostics == null
                    ? BattleBenchmarkMetric.Unavailable("count")
                    : BattleBenchmarkMetric.FromValue(diagnostics.ActiveChunkCount, "count"),
                RequestedBackend = config.Backend.ToString(),
                EffectiveBackend = presenter.EffectiveBackend,
            };
            return frame;
        }

        private bool HasAllApplicableFormalSampleMetrics(
            BattleRenderingBenchmarkFrame frame,
            out string reason)
        {
            if (!completedFrameCollector.IsSupported)
            {
                reason = string.Empty;
                return true;
            }
            var missing = new List<string>();
            AddMissingFormalSampleMetric(missing, "frameTimeMs", frame.FrameTimeMs);
            AddMissingFormalSampleMetric(missing, "mainThreadTimeMs", frame.MainThreadTimeMs);
            if (policyContext.GraphicsMultiThreaded)
                AddMissingFormalSampleMetric(missing, "renderThreadTimeMs", frame.RenderThreadTimeMs);
            AddMissingFormalSampleMetric(missing, "gpuFrameTimeMs", frame.GpuFrameTimeMs);
            AddMissingFormalSampleMetric(missing, "managedAllocationBytes", frame.ManagedAllocationBytes);
            AddMissingFormalSampleMetric(missing, "drawCalls", frame.DrawCalls);
            AddMissingFormalSampleMetric(missing, "totalAllocatedMemoryBytes", frame.TotalAllocatedMemoryBytes);
            AddMissingFormalSampleMetric(missing, "graphicsMemoryBytes", frame.GraphicsMemoryBytes);
            AddMissingFormalSampleMetric(
                missing,
                "benchmarkOwnedTextureMemoryBytes",
                frame.BenchmarkOwnedTextureMemoryBytes);
            AddMissingFormalSampleMetric(missing, "presentationBuildTimeMs", frame.PresentationBuildTimeMs);
            AddMissingFormalSampleMetric(
                missing,
                "presenterSubmittedRenderItems",
                frame.PresenterSubmittedRenderItems);
            AddMissingFormalSampleMetric(missing, "resourceSegments", frame.ResourceSegments);
            AddMissingFormalSampleMetric(missing, "benchmarkOwnedMemoryBytes", frame.BenchmarkOwnedMemoryBytes);
            if (config.Backend == BattlePresentationBackendMode.CentralOnly)
            {
                AddMissingFormalSampleMetric(
                    missing,
                    "presenterSubmissionDrawCalls",
                    frame.PresenterSubmissionDrawCalls);
                AddMissingFormalSampleMetric(missing, "meshChunks", frame.MeshChunks);
            }

            if (missing.Count == 0)
            {
                reason = string.Empty;
                return true;
            }

            reason = "required applicable metrics unavailable: " + string.Join(", ", missing) + ".";
            return false;
        }

        private static void AddMissingFormalSampleMetric(
            List<string> missing,
            string name,
            BattleBenchmarkMetric metric)
        {
            if (!metric.Available)
                missing.Add(name);
        }

        private void CaptureLeakBaseline()
        {
            leakBaselineCaptured = true;
            leakManagedStart = leakProbe.CaptureRetainedManagedHeapBytes();
            leakOwnedStart = presenter.MeasureOwnedResourceMemoryBytes();
            leakResourcesStart = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakGraphicsAvailable = graphics.Available;
            leakGraphicsStart = graphics.Available ? (long)graphics.Value : 0L;
        }

        private void CapturePrePresenterLeakBaseline()
        {
            leakPrePresenterManaged = leakProbe.CaptureRetainedManagedHeapBytes();
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakPrePresenterGraphicsAvailable = graphics.Available;
            leakPrePresenterGraphics = graphics.Available ? (long)graphics.Value : 0L;
        }

        private void BeginLeakTeardown()
        {
            leakManagedEnd = leakProbe.CaptureRetainedManagedHeapBytes();
            leakOwnedEnd = presenter.MeasureOwnedResourceMemoryBytes();
            leakResourcesEnd = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakSoakGraphicsAvailable = leakGraphicsAvailable && graphics.Available;
            leakGraphicsEnd = leakSoakGraphicsAvailable ? (long)graphics.Value : 0L;
            DisposePresenter();
            teardownPending = true;
            teardownStartedFrame = leakProbe.CurrentUnityFrame;
            teardownCleanupRequested = false;
            teardownCleanupCompleted = false;
            teardownCleanupCompletedFrame = 0;
        }

        private void FinalizeLeakReport(string teardownCleanupFailureReason = null)
        {
            long teardownManagedEnd = leakProbe.CaptureRetainedManagedHeapBytes();
            BattleBenchmarkMetric teardownGraphics = leakProbe.CaptureGraphicsMemory();
            bool teardownGraphicsAvailable = leakPrePresenterGraphicsAvailable &&
                                             teardownGraphics.Available;
            long teardownGraphicsEnd = teardownGraphicsAvailable
                ? (long)teardownGraphics.Value
                : 0L;
            long teardownOwnedEnd = presenter.MeasureOwnedResourceMemoryBytes();
            int teardownResourcesEnd = presenter.OwnedResourceCount;

            long managedGrowth = leakManagedEnd - leakManagedStart;
            long graphicsGrowth = leakGraphicsEnd - leakGraphicsStart;
            bool soakPassed = leakSoakGraphicsAvailable &&
                              managedGrowth <= config.MaxManagedGrowthBytes &&
                              graphicsGrowth <= config.MaxGraphicsGrowthBytes &&
                              leakOwnedEnd <= leakOwnedStart &&
                              leakResourcesEnd <= leakResourcesStart;
            // The pre-presenter values expose one-time pipeline initialization, while the
            // post-Dispose gate detects retained growth relative to the steady-state soak baseline.
            long teardownManagedGrowth = teardownManagedEnd - leakManagedStart;
            long teardownGraphicsGrowth = teardownGraphicsEnd - leakGraphicsStart;
            bool teardownPassed = string.IsNullOrEmpty(teardownCleanupFailureReason) &&
                                  teardownGraphicsAvailable &&
                                  teardownManagedGrowth <= config.MaxManagedGrowthBytes &&
                                  teardownGraphicsGrowth <= config.MaxGraphicsGrowthBytes &&
                                  teardownOwnedEnd == 0L &&
                                  teardownResourcesEnd == 0;
            BattleBenchmarkMetricStatus teardownStatus = !teardownGraphicsAvailable
                ? BattleBenchmarkMetricStatus.Missing
                : teardownPassed
                    ? BattleBenchmarkMetricStatus.Passed
                    : BattleBenchmarkMetricStatus.Failed;
            string teardownReason = !string.IsNullOrEmpty(teardownCleanupFailureReason)
                ? teardownCleanupFailureReason
                : !teardownGraphicsAvailable
                ? "Post-Dispose graphics memory evidence is required but unavailable."
                : teardownPassed
                    ? "Post-Dispose ownership returned to zero and retained managed/graphics memory returned within steady-state thresholds after bounded Unity cleanup."
                    : "Post-Dispose ownership remained nonzero or retained managed/graphics memory exceeded a steady-state threshold after bounded Unity cleanup.";
            bool passed = soakPassed && teardownPassed;
            string reason = !leakSoakGraphicsAvailable
                ? "Steady-state graphics memory evidence is required but unavailable."
                : passed
                    ? "Steady-state soak and post-Dispose teardown both passed."
                    : "Steady-state soak or post-Dispose teardown failed: " + teardownReason;
            leakReport = new BattleRenderingBenchmarkLeakReport(
                true,
                passed,
                leakFramesCaptured,
                leakPrePresenterManaged,
                leakPrePresenterGraphics,
                leakPrePresenterGraphicsAvailable,
                leakManagedStart,
                leakManagedEnd,
                leakGraphicsStart,
                leakGraphicsEnd,
                leakSoakGraphicsAvailable,
                leakOwnedStart,
                leakOwnedEnd,
                leakResourcesStart,
                leakResourcesEnd,
                config.MaxManagedGrowthBytes,
                config.MaxGraphicsGrowthBytes,
                teardownFramesWaited,
                teardownManagedEnd,
                teardownGraphicsEnd,
                teardownGraphicsAvailable,
                teardownOwnedEnd,
                teardownResourcesEnd,
                teardownStatus,
                teardownReason,
                RetainedManagedHeapMeasurementMode,
                reason);
            teardownPending = false;
        }

        private void DisposePresenter()
        {
            if (presenterDisposed)
                return;
            presenterDisposed = true;
            presenter.Dispose();
        }

        private void FinalizeReport()
        {
            bool logicTickMetricsValidated = ValidateLogicTickMetrics();
            BattleBenchmarkMetricAvailability[] metricAvailability = BuildMetricAvailability();
            report = new BattleRenderingBenchmarkReport(
                config,
                frames.ToArray(),
                metricAvailability,
                policyContext,
                workload.RequestedEntityCount,
                workload.ActualEntityCount,
                workload.CommandCount,
                workload.Fingerprint,
                presenterImplementation,
                presenterResourceMode,
                presenterDrawMode,
                presenterRenderTargetWidth,
                presenterRenderTargetHeight,
                true,
                workload.RuntimeAdmissionValidated,
                logicTickMetricsValidated,
                workload.RuntimeStateDeterministic,
                true,
                leakReport);
            report.RuntimeObjectCount = workload.RuntimeObjectCount;
            report.RuntimeSlotCapacity = workload.RuntimeSlotCapacity;
            report.RuntimeProfile = workload.RuntimeProfile;
            report.WarmupLogicTickCount = workload.WarmupTickCount;
            report.SampleLogicTickCount = workload.SampleTickCount;
            report.InputFingerprint = workload.InputFingerprint;
            report.InitialRuntimeChecksum = workload.InitialRuntimeChecksum;
            report.FinalRuntimeChecksum = workload.FinalRuntimeChecksum;
            report.WarmupLogicTickSamples = workload.WarmupLogicTickSamples;
            report.SampleLogicTickSamples = workload.LogicTickSamples;
            report.CompletedFrameRejectedAttemptCount = completedFrameRejectedAttemptCount;
            report.MaxCompletedFrameSampleAttempts = MaxCompletedFrameSampleAttempts;
            report.CompletedFrameSamplingFailureReason = completedFrameSamplingFailureReason;
        }

        private BattleBenchmarkMetricAvailability[] BuildMetricAvailability()
        {
            var result = new List<BattleBenchmarkMetricAvailability>(24);
            AddFrameMetric(result, "frameTimeMs", BattleBenchmarkRecorderKind.FrameTime, frame => frame.FrameTimeMs);
            AddFrameMetric(result, "mainThreadTimeMs", BattleBenchmarkRecorderKind.MainThread, frame => frame.MainThreadTimeMs);
            AddFrameMetric(
                result,
                "renderThreadTimeMs",
                BattleBenchmarkRecorderKind.RenderThread,
                frame => frame.RenderThreadTimeMs,
                policyContext.GraphicsMultiThreaded);
            AddFrameMetric(result, "gpuFrameTimeMs", BattleBenchmarkRecorderKind.GpuFrame, frame => frame.GpuFrameTimeMs);
            AddFrameMetric(result, "managedAllocationBytes", BattleBenchmarkRecorderKind.ManagedAllocation, frame => frame.ManagedAllocationBytes);
            AddFrameMetric(
                result,
                "drawCalls",
                BattleBenchmarkRecorderKind.DrawCalls,
                frame => frame.DrawCalls,
                unavailableReason: "A positive completed-frame draw-call count is required for this non-empty benchmark render workload.");
            AddFrameMetric(result, "totalAllocatedMemoryBytes", BattleBenchmarkRecorderKind.TotalMemory, frame => frame.TotalAllocatedMemoryBytes);
            AddFrameMetric(result, "graphicsMemoryBytes", BattleBenchmarkRecorderKind.GraphicsMemory, frame => frame.GraphicsMemoryBytes);
            AddLocalMetric(
                result,
                "benchmarkOwnedTextureMemoryBytes",
                "benchmark-owned-textures",
                frame => frame.BenchmarkOwnedTextureMemoryBytes,
                BenchmarkOwnedTextureMemorySource(),
                unavailableReason: BenchmarkOwnedTextureMemoryUnavailableReason());
            AddLocalMetric(result, "logicTickTimeMs", "logic-tick", frame => frame.LogicTickTimeMs,
                "Stopwatch around full NTSDBattleTickSystem.RunReleaseTick");
            AddLocalMetric(result, "logicTickAllocatedBytes", "logic-tick", frame => frame.LogicTickAllocatedBytes,
                "GC.GetAllocatedBytesForCurrentThread around full NTSDBattleTickSystem.RunReleaseTick");
            AddLocalMetric(result, "presentationBuildTimeMs", "presenter-local", frame => frame.PresentationBuildTimeMs,
                "Stopwatch around benchmark presenter update/build");
            AddLocalMetric(result, "presenterSubmittedRenderItems", "presenter-local", frame => frame.PresenterSubmittedRenderItems,
                "Validated frozen render-command/materializer count");
            AddLocalMetric(result, "resourceSegments", "presenter-local", frame => frame.ResourceSegments,
                "Presenter resource compatibility grouping");
            AddLocalMetric(result, "benchmarkOwnedMemoryBytes", "presenter-local", frame => frame.BenchmarkOwnedMemoryBytes,
                "Profiler.GetRuntimeMemorySizeLong over benchmark-owned resources");

            bool central = config.Backend == BattlePresentationBackendMode.CentralOnly;
            AddLocalMetric(
                result,
                "presenterSubmissionDrawCalls",
                "presenter-local",
                frame => frame.PresenterSubmissionDrawCalls,
                presenterSubmissionDrawMetricSource,
                central,
                presenterSubmissionDrawUnavailableReason);
            AddLocalMetric(
                result,
                "meshChunks",
                "presenter-local",
                frame => frame.MeshChunks,
                central ? "BattleDynamicMeshBackend diagnostics" : "not applicable",
                central,
                "Legacy compatibility presentation does not build central mesh chunks.");

            bool? exactSampleCount = string.IsNullOrEmpty(completedFrameSamplingFailureReason)
                ? frames.Count == config.SampleFrames
                : (bool?)null;
            AddGate(result, "exactSampleCount", exactSampleCount,
                frames.Count,
                config.SampleFrames,
                "completed-frame collector");
            AddGate(result, "countValidated", workload.ActualEntityCount == workload.RequestedEntityCount,
                1, 1, "frozen workload entity counts");
            AddGate(result, "runtimeAdmissionValidated", workload.RuntimeAdmissionValidated,
                1, 1, "SimulationWorld runtime admission");
            AddGate(result, "determinismValidated",
                config.Scenario.UsesCurrentScene ? (bool?)null : workload.RuntimeStateDeterministic,
                1, 1, "runtime checksum replay");
            AddGate(result, "rendererWorkloadValidated", true,
                1, 1, "presenter materialization validation");
            if (config.LeakCheckFrames > 0)
            {
                BattleBenchmarkMetricStatus leakStatus = !leakReport.GraphicsAvailable ||
                                                         leakReport.TeardownStatus == BattleBenchmarkMetricStatus.Missing
                    ? BattleBenchmarkMetricStatus.Missing
                    : leakReport.Passed
                        ? BattleBenchmarkMetricStatus.Passed
                        : BattleBenchmarkMetricStatus.Failed;
                result.Add(new BattleBenchmarkMetricAvailability(
                    "leakCheck",
                    true,
                    BattleBenchmarkMetricApplicability.Applicable,
                    leakStatus,
                    "long-run",
                    leakReport.Available && leakReport.GraphicsAvailable &&
                    leakReport.TeardownStatus != BattleBenchmarkMetricStatus.Missing ? 1 : 0,
                    1,
                    RetainedManagedHeapMeasurementMode,
                    leakReport.Reason));
            }
            else
            {
                result.Add(new BattleBenchmarkMetricAvailability(
                    "leakCheck",
                    false,
                    BattleBenchmarkMetricApplicability.NotApplicable,
                    BattleBenchmarkMetricStatus.NotApplicable,
                    "long-run",
                    0,
                    0,
                    "not requested",
                    "Leak/long-run soak and teardown were not requested."));
            }
            return result.ToArray();
        }

        private string BenchmarkOwnedTextureMemorySource()
        {
            return "Profiler.GetRuntimeMemorySizeLong summed over " +
                   presenterOwnedTextureResourceCount +
                   " Texture2D/RenderTexture objects owned by benchmark resource generation " +
                   presenterResourceGeneration + ".";
        }

        private string BenchmarkOwnedTextureMemoryUnavailableReason()
        {
            return "No positive runtime-memory sample was observed for the " +
                   presenterOwnedTextureResourceCount +
                   " Texture2D/RenderTexture objects owned by benchmark resource generation " +
                   presenterResourceGeneration + ".";
        }

        private void AddFrameMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            BattleBenchmarkRecorderKind kind,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            bool applicable = true,
            string unavailableReason = "")
        {
            AddMetric(
                result,
                name,
                "completed-frame",
                selector,
                completedFrameCollector.Source(kind),
                required: applicable,
                applicable,
                string.IsNullOrWhiteSpace(completedFrameCollector.Reason(kind))
                    ? unavailableReason
                    : completedFrameCollector.Reason(kind));
        }

        private void AddLocalMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            string scope,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            string source,
            bool applicable = true,
            string unavailableReason = "")
        {
            AddMetric(result, name, scope, selector, source, applicable, applicable, unavailableReason);
        }

        private void AddMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            string scope,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            string source,
            bool required,
            bool applicable,
            string unavailableReason)
        {
            if (!applicable)
            {
                result.Add(new BattleBenchmarkMetricAvailability(
                    name,
                    false,
                    BattleBenchmarkMetricApplicability.NotApplicable,
                    BattleBenchmarkMetricStatus.NotApplicable,
                    scope,
                    0,
                    0,
                    source,
                    unavailableReason));
                return;
            }

            int sampleCount = 0;
            for (int index = 0; index < frames.Count; index++)
            {
                if (selector(frames[index]).Available)
                    sampleCount++;
            }
            BattleBenchmarkMetricStatus status = sampleCount == config.SampleFrames
                ? BattleBenchmarkMetricStatus.Available
                : completedFrameCollector.IsSupported
                    ? BattleBenchmarkMetricStatus.Missing
                    : BattleBenchmarkMetricStatus.Unsupported;
            string reason = status == BattleBenchmarkMetricStatus.Available
                ? string.Empty
                : !string.IsNullOrEmpty(completedFrameSamplingFailureReason)
                    ? completedFrameSamplingFailureReason
                    : string.IsNullOrWhiteSpace(unavailableReason)
                        ? string.IsNullOrWhiteSpace(completedFrameCollector.UnsupportedReason)
                            ? $"Captured {sampleCount} of {config.SampleFrames} required samples."
                            : completedFrameCollector.UnsupportedReason
                        : unavailableReason;
            result.Add(new BattleBenchmarkMetricAvailability(
                name,
                required,
                BattleBenchmarkMetricApplicability.Applicable,
                status,
                scope,
                sampleCount,
                config.SampleFrames,
                source,
                reason));
        }

        private static void AddGate(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            bool? passed,
            int sampleCount,
            int expectedSampleCount,
            string source)
        {
            BattleBenchmarkMetricStatus status =
                BattleRenderingBenchmarkEvidencePolicy.ValidationStatus(passed);
            result.Add(new BattleBenchmarkMetricAvailability(
                name,
                true,
                BattleBenchmarkMetricApplicability.Applicable,
                status,
                "validation-gate",
                passed.HasValue ? sampleCount : 0,
                expectedSampleCount,
                source,
                !passed.HasValue
                    ? "The current-scene workload did not measure this validation gate."
                    : passed.Value
                        ? string.Empty
                        : "The required validation gate failed."));
        }

        private bool ValidateLogicTickMetrics()
        {
            if (frames.Count <= 0)
                return false;
            for (int index = 0; index < frames.Count; index++)
            {
                if (!frames[index].LogicTickTimeMs.Available)
                    return false;
                if (!frames[index].LogicTickAllocatedBytes.Available)
                    return false;
            }
            return true;
        }

        private void ValidateCount()
        {
            if (workload.ActualEntityCount != workload.RequestedEntityCount ||
                workload.FrozenFrame.EntityCount != workload.ActualEntityCount)
            {
                throw new InvalidOperationException(
                    $"Benchmark presentation entity count changed or mismatched: requested={workload.RequestedEntityCount}, " +
                    $"actual={workload.ActualEntityCount}, frame={workload.FrozenFrame.EntityCount}.");
            }
        }

        private void ValidatePresenterWorkload()
        {
            if (presenter.ResolvedCommandCount != workload.CommandCount ||
                presenter.MaterializedRenderItemCount != workload.CommandCount)
            {
                throw new InvalidOperationException(
                    $"{presenter.Implementation} did not materialize the complete workload: " +
                    $"commands={workload.CommandCount}, resolved={presenter.ResolvedCommandCount}, " +
                    $"materializedItems={presenter.MaterializedRenderItemCount}.");
            }
        }
    }

    public sealed class BattleRenderingBenchmarkSuiteSession : IDisposable
    {
        private readonly BattleRenderingBenchmarkConfig config;
        private readonly SimulationWorld world;
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly List<BattleRenderingBenchmarkReport> runs =
            new List<BattleRenderingBenchmarkReport>(2);
        private readonly BattlePresentationBackendMode previousBackend;
        private readonly Func<BattleRenderingBenchmarkConfig, SimulationWorld,
            BattleRenderingBenchmarkWorkload, IBattleRenderingBenchmarkRunSession> sessionFactory;
        private IBattleRenderingBenchmarkRunSession activeSession;
        private BattleRenderingBenchmarkSuiteReport report;
        private int nextBackendIndex;
        private bool disposed;
        private bool backendRestored;

        public BattleRenderingBenchmarkSuiteSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world)
            : this(config, world, null)
        {
        }

        public BattleRenderingBenchmarkSuiteSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            Func<BattleRenderingBenchmarkConfig, SimulationWorld,
                BattleRenderingBenchmarkWorkload, IBattleRenderingBenchmarkRunSession> benchmarkSessionFactory)
        {
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            previousBackend = this.world.BattlePresentation.Mode;
            sessionFactory = benchmarkSessionFactory ??
                ((runConfig, runWorld, runWorkload) =>
                    new BattleRenderingBenchmarkSession(runConfig, runWorld, runWorkload));
            workload = BattleRenderingBenchmarkWorkload.Create(
                config.Scenario,
                this.world,
                config.WarmupFrames,
                config.SampleFrames);
            try
            {
                StartNextRun();
            }
            catch
            {
                RestoreBackend();
                throw;
            }
        }

        public bool IsComplete => report != null;
        public BattleRenderingBenchmarkSuiteReport Report => report;
        public BattleRenderingBenchmarkWorkload Workload => workload;

        public bool CaptureFrame()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleRenderingBenchmarkSuiteSession));
            if (report != null)
                return true;
            try
            {
                if (!activeSession.CaptureFrame())
                    return false;

                BattleRenderingBenchmarkReport completed = activeSession.Report;
                runs.Add(completed);
                activeSession.Dispose();
                activeSession = null;
                if (StartNextRun())
                    return false;

                ValidateABWorkload();
                report = new BattleRenderingBenchmarkSuiteReport(config, runs.ToArray(), workload.Fingerprint);
                RestoreBackend();
                return true;
            }
            catch
            {
                try
                {
                    DisposeAfterFailure();
                }
                catch (Exception cleanupException)
                {
                    UnityEngine.Debug.LogException(cleanupException);
                }
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            try
            {
                activeSession?.Dispose();
            }
            finally
            {
                activeSession = null;
                RestoreBackend();
            }
        }

        private bool StartNextRun()
        {
            BattlePresentationBackendMode backend;
            if (config.Comparison == BattleRenderingBenchmarkComparison.Single)
            {
                if (nextBackendIndex > 0)
                    return false;
                backend = config.Backend;
            }
            else
            {
                if (nextBackendIndex == 0)
                    backend = BattlePresentationBackendMode.CentralOnly;
                else if (nextBackendIndex == 1)
                    backend = BattlePresentationBackendMode.LegacyOnly;
                else
                    return false;
            }
            nextBackendIndex++;
            world.SetBattlePresentationBackend(backend);
            activeSession = sessionFactory(
                config.ForBackend(backend),
                world,
                workload);
            return true;
        }

        private void ValidateABWorkload()
        {
            if (config.Comparison != BattleRenderingBenchmarkComparison.CentralLegacyAB)
                return;
            if (runs.Count != 2 ||
                runs[0].Config.Backend != BattlePresentationBackendMode.CentralOnly ||
                runs[1].Config.Backend != BattlePresentationBackendMode.LegacyOnly ||
                runs[0].WorkloadFingerprint != workload.Fingerprint ||
                runs[1].WorkloadFingerprint != workload.Fingerprint ||
                runs[0].InputFingerprint != runs[1].InputFingerprint ||
                runs[0].FinalRuntimeChecksum != runs[1].FinalRuntimeChecksum ||
                runs[0].BenchmarkRenderTargetWidth != runs[1].BenchmarkRenderTargetWidth ||
                runs[0].BenchmarkRenderTargetHeight != runs[1].BenchmarkRenderTargetHeight ||
                !runs[0].RendererWorkloadValidated ||
                !runs[1].RendererWorkloadValidated)
            {
                throw new InvalidOperationException(
                    "Central/Legacy A/B did not consume the same validated frozen workload.");
            }
        }

        private void RestoreBackend()
        {
            if (backendRestored)
                return;
            backendRestored = true;
            world.SetBattlePresentationBackend(previousBackend);
        }

        private void DisposeAfterFailure()
        {
            disposed = true;
            try
            {
                activeSession?.Dispose();
            }
            finally
            {
                activeSession = null;
                RestoreBackend();
            }
        }
    }

    public sealed class BattleRenderingBenchmarkRunner : MonoBehaviour
    {
        private BattleRenderingBenchmarkSuiteSession session;
        private string outputPath;
        private Action<BattleRenderingBenchmarkRunner, string> completion;
        private bool stopping;

        public static BattleRenderingBenchmarkRunner Start(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            string outputPath,
            Action<BattleRenderingBenchmarkRunner, string> completion = null)
        {
            var host = new GameObject("NTSD Battle Rendering Benchmark Runner")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            DontDestroyOnLoad(host);
            BattleRenderingBenchmarkRunner runner = host.AddComponent<BattleRenderingBenchmarkRunner>();
            try
            {
                runner.Initialize(config, world, outputPath, completion);
                return runner;
            }
            catch
            {
                DisposeHost(host);
                throw;
            }
        }

        public void Abort(string reason)
        {
            if (stopping)
                return;
            stopping = true;
            session?.Dispose();
            session = null;
            Notify("FAIL\n" + (reason ?? "Benchmark aborted."));
            DisposeHost(gameObject);
        }

        private void Initialize(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            string path,
            Action<BattleRenderingBenchmarkRunner, string> callback)
        {
            outputPath = path;
            completion = callback;
            session = new BattleRenderingBenchmarkSuiteSession(config, world);
        }

        private void Update()
        {
            if (stopping || session == null)
                return;
            try
            {
                if (!session.CaptureFrame())
                    return;
                session.Report.WriteJson(outputPath);
                StopWithResult(session.Report.Verdict.ToString().ToUpperInvariant() + "\n" + outputPath);
            }
            catch (Exception ex)
            {
                Abort(ex.ToString());
            }
        }

        private void StopWithResult(string result)
        {
            if (stopping)
                return;
            stopping = true;
            session.Dispose();
            session = null;
            Notify(result);
            DisposeHost(gameObject);
        }

        private void Notify(string result)
        {
            Action<BattleRenderingBenchmarkRunner, string> callback = completion;
            completion = null;
            callback?.Invoke(this, result);
        }

        private void OnDestroy()
        {
            if (!stopping && session != null)
            {
                session.Dispose();
                session = null;
                Notify("FAIL\nBenchmark runner was destroyed before completion.");
            }
        }

        private static void DisposeHost(UnityEngine.Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }

    public static class BattleRenderingBenchmarkPlayerArguments
    {
        public const string EnableArgument = "-ntsdBattleRenderingBenchmark";
        public const string ScenarioArgument = "-ntsdBattleRenderingBenchmarkScenario";
        public const string BackendArgument = "-ntsdBattleRenderingBenchmarkBackend";
        public const string ComparisonArgument = "-ntsdBattleRenderingBenchmarkComparison";
        public const string WarmupArgument = "-ntsdBattleRenderingBenchmarkWarmup";
        public const string SampleArgument = "-ntsdBattleRenderingBenchmarkSamples";
        public const string LeakArgument = "-ntsdBattleRenderingBenchmarkLeakFrames";
        public const string OutputArgument = "-ntsdBattleRenderingBenchmarkOutput";

        public static bool TryParse(
            string[] arguments,
            out BattleRenderingBenchmarkRequest request,
            out string error)
        {
            request = null;
            error = string.Empty;
            if (!ContainsFlag(arguments, EnableArgument))
                return false;

            var parsed = new BattleRenderingBenchmarkRequest();
            try
            {
                parsed.targetActiveEntities = FindValue(arguments, ScenarioArgument) ?? "1000";
                parsed.backend = FindValue(arguments, BackendArgument) ??
                                 nameof(BattlePresentationBackendMode.CentralOnly);
                parsed.comparison = FindValue(arguments, ComparisonArgument) ?? "ab";
                parsed.warmupFrames = ParseInt(arguments, WarmupArgument, parsed.warmupFrames);
                parsed.sampleFrames = ParseInt(arguments, SampleArgument, parsed.sampleFrames);
                parsed.leakCheckFrames = ParseInt(arguments, LeakArgument, parsed.leakCheckFrames);
                parsed.outputPath = FindValue(arguments, OutputArgument) ??
                                    "NTSD_BattleRenderingBenchmark-Player.json";
                BattleRenderingBenchmarkConfig.FromRequest(parsed);
                request = parsed;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static int ParseInt(string[] arguments, string name, int fallback)
        {
            string value = FindValue(arguments, name);
            if (string.IsNullOrWhiteSpace(value))
                return fallback;
            if (!int.TryParse(value, out int parsed))
                throw new ArgumentException($"Argument {name} requires an integer value.");
            return parsed;
        }

        private static bool ContainsFlag(string[] arguments, string name)
        {
            if (arguments == null)
                return false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arguments[index], name + "=true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static string FindValue(string[] arguments, string name)
        {
            if (arguments == null)
                return null;
            string prefix = name + "=";
            for (int index = 0; index < arguments.Length; index++)
            {
                string argument = arguments[index];
                if (argument != null &&
                    argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return argument.Substring(prefix.Length);
                }
                if (string.Equals(argument, name, StringComparison.OrdinalIgnoreCase) &&
                    index + 1 < arguments.Length)
                {
                    return arguments[index + 1];
                }
            }
            return null;
        }
    }

    internal static class BattleRenderingBenchmarkPlayerBootstrap
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TryStart()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            bool explicitlyRequested = false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(
                        arguments[index],
                        BattleRenderingBenchmarkPlayerArguments.EnableArgument,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        arguments[index],
                        BattleRenderingBenchmarkPlayerArguments.EnableArgument + "=true",
                        StringComparison.OrdinalIgnoreCase))
                {
                    explicitlyRequested = true;
                    break;
                }
            }
            if (!explicitlyRequested)
                return;

            if (!BattleRenderingBenchmarkPlayerArguments.TryParse(
                    arguments,
                    out BattleRenderingBenchmarkRequest request,
                    out string error))
            {
                UnityEngine.Debug.LogError("[BattleRenderingBenchmark] Invalid Player request: " + error);
                Application.Quit(2);
                return;
            }

            try
            {
                BattleRenderingBenchmarkConfig config =
                    BattleRenderingBenchmarkConfig.FromRequest(request);
                SimulationWorld world = config.Scenario.UsesCurrentScene
                    ? SimulationTickDriver.Instance?.World
                    : null;
                if (config.Scenario.UsesCurrentScene && world == null)
                    throw new InvalidOperationException("Current-scene Player benchmark has no active SimulationWorld.");
                BattleRenderingBenchmarkRunner.Start(
                    config,
                    world,
                    config.OutputPath,
                    (_, result) =>
                    {
                        bool passed = result != null && result.StartsWith("PASS", StringComparison.Ordinal);
                        UnityEngine.Debug.Log("[BattleRenderingBenchmark] Player result: " + result);
                        Application.Quit(passed ? 0 : 1);
                    });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[BattleRenderingBenchmark] Player start failed: " + ex);
                Application.Quit(2);
            }
        }
#endif
    }

    public enum BattleBenchmarkRecorderKind : byte
    {
        FrameTime = 0,
        MainThread = 1,
        RenderThread = 2,
        GpuFrame = 3,
        LogicTick = 4,
        ManagedAllocation = 5,
        DrawCalls = 6,
        TotalMemory = 7,
        GraphicsMemory = 8,
    }

    internal static class BattleRenderingBenchmarkMemory
    {
        internal static BattleBenchmarkMetric CaptureTotalAllocatedMemory()
        {
            return BattleBenchmarkMetric.FromValue(Profiler.GetTotalAllocatedMemoryLong(), "bytes");
        }

        internal static BattleBenchmarkMetric CaptureGraphicsMemory()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return BattleBenchmarkMetric.FromValue(
                Profiler.GetAllocatedMemoryForGraphicsDriver(),
                "bytes");
#else
            return BattleBenchmarkMetric.Unavailable("bytes");
#endif
        }

    }

    public sealed class BattleBenchmarkCompletedFrameAttribution
    {
        private int pendingGeneration;
        private int requestedUnityFrame;
        private ulong timingWatermark;
        private ulong lastAcceptedTimingTimestamp;
        private bool countersSnapshotted;

        public bool CountersSnapshotted => countersSnapshotted;

        public void Request(int generation, int unityFrame, ulong latestTimingTimestamp)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
            requestedUnityFrame = unityFrame;
            timingWatermark = Math.Max(latestTimingTimestamp, lastAcceptedTimingTimestamp);
            countersSnapshotted = false;
        }

        public bool ShouldSnapshotCounters(int generation, int unityFrame)
        {
            if (generation != pendingGeneration || countersSnapshotted || unityFrame <= requestedUnityFrame)
                return false;
            countersSnapshotted = true;
            return true;
        }

        public bool TryAcceptTiming(int generation, ulong frameStartTimestamp)
        {
            if (generation != pendingGeneration || !countersSnapshotted ||
                frameStartTimestamp == 0UL || frameStartTimestamp <= timingWatermark)
            {
                return false;
            }
            lastAcceptedTimingTimestamp = frameStartTimestamp;
            pendingGeneration = 0;
            return true;
        }

        public void CompleteWithoutTiming(int generation)
        {
            if (generation == pendingGeneration)
                pendingGeneration = 0;
        }

        public void ResetPending()
        {
            pendingGeneration = 0;
            requestedUnityFrame = 0;
            timingWatermark = lastAcceptedTimingTimestamp;
            countersSnapshotted = false;
        }
    }

    internal sealed class BattleBenchmarkUnityCompletedFrameCollector :
        IBattleBenchmarkCompletedFrameCollector
    {
        private const int MaxFrameTimingDrainAttempts = 4;
        private readonly BattleRenderingBenchmarkPolicyContext context;
        private readonly BattleBenchmarkCounterRecorder managedAllocation;
        private readonly BattleBenchmarkCounterRecorder drawCalls;
        private readonly BattleBenchmarkCompletedFrameAttribution attribution =
            new BattleBenchmarkCompletedFrameAttribution();
        private readonly FrameTiming[] timings = new FrameTiming[1];
        private int pendingGeneration;
        private int drainAttempts;
        private int lastDrainUnityFrame = -1;
        private string frameTimingReason = string.Empty;
        private BattleBenchmarkMetric managedAllocationSnapshot;
        private BattleBenchmarkMetric drawCallsSnapshot;
        private BattleBenchmarkMetric totalMemorySnapshot;
        private BattleBenchmarkMetric graphicsMemorySnapshot;

        internal BattleBenchmarkUnityCompletedFrameCollector(
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext)
        {
            context = benchmarkPolicyContext;
            managedAllocation = new BattleBenchmarkCounterRecorder(
                ProfilerCategory.Memory,
                "GC Allocated In Frame",
                "bytes");
            drawCalls = new BattleBenchmarkCounterRecorder(
                ProfilerCategory.Render,
                "Draw Calls Count",
                "count");
        }

        public bool IsSupported =>
            context.IsSupportedExecutionScope && context.FrameTimingStatsEnabled;

        public string UnsupportedReason
        {
            get
            {
                if (!context.IsPlaying)
                    return "EditMode has no completed rendered-frame collection scope.";
                if (!context.IsSupportedExecutionScope)
                    return "Completed-frame collection is supported only in Play Mode Editor and Windows Standalone.";
                if (!context.FrameTimingStatsEnabled)
                    return "FrameTimingManager.IsFeatureEnabled returned false.";
                return frameTimingReason;
            }
        }

        public void Request(int generation)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
            drainAttempts = 0;
            lastDrainUnityFrame = -1;
            frameTimingReason = string.Empty;
            managedAllocationSnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            drawCallsSnapshot = BattleBenchmarkMetric.Unavailable("count");
            totalMemorySnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            graphicsMemorySnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            managedAllocation.Restart();
            drawCalls.Restart();
            if (IsSupported)
            {
                attribution.Request(generation, Time.frameCount, LatestTimingTimestamp());
                FrameTimingManager.CaptureFrameTimings();
            }
        }

        public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics)
        {
            if (pendingGeneration != generation)
            {
                metrics = default;
                return false;
            }
            if (!IsSupported)
            {
                pendingGeneration = 0;
                managedAllocation.Abort();
                drawCalls.Abort();
                metrics = BattleBenchmarkCompletedFrameMetrics.Unavailable();
                return true;
            }
            if (!attribution.CountersSnapshotted &&
                !attribution.ShouldSnapshotCounters(generation, Time.frameCount))
            {
                metrics = default;
                return false;
            }
            if (attribution.CountersSnapshotted && drainAttempts == 0)
                SnapshotAndStopCounters();
            if (lastDrainUnityFrame == Time.frameCount)
            {
                metrics = default;
                return false;
            }
            lastDrainUnityFrame = Time.frameCount;

            drainAttempts++;
            uint count = FrameTimingManager.GetLatestTimings(1, timings);
            bool timingAccepted = count > 0 &&
                                  attribution.TryAcceptTiming(
                                      generation,
                                      timings[0].frameStartTimestamp);
            if (!timingAccepted && drainAttempts < MaxFrameTimingDrainAttempts)
            {
                metrics = default;
                return false;
            }

            pendingGeneration = 0;
            if (!timingAccepted)
            {
                frameTimingReason =
                    count == 0
                        ? "FrameTimingManager returned no completed timing after the bounded drain window."
                        : "FrameTimingManager returned only stale timing generations after the bounded drain window.";
                attribution.CompleteWithoutTiming(generation);
                metrics = new BattleBenchmarkCompletedFrameMetrics(
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    managedAllocationSnapshot,
                    drawCallsSnapshot,
                    totalMemorySnapshot,
                    graphicsMemorySnapshot);
                return true;
            }

            FrameTiming timing = timings[0];
            metrics = new BattleBenchmarkCompletedFrameMetrics(
                PositiveMilliseconds(timing.cpuFrameTime),
                PositiveMilliseconds(timing.cpuMainThreadFrameTime),
                context.GraphicsMultiThreaded
                    ? PositiveMilliseconds(timing.cpuRenderThreadFrameTime)
                    : BattleBenchmarkMetric.Unavailable("ms"),
                PositiveMilliseconds(timing.gpuFrameTime),
                managedAllocationSnapshot,
                drawCallsSnapshot,
                totalMemorySnapshot,
                graphicsMemorySnapshot);
            return true;
        }

        public string Source(BattleBenchmarkRecorderKind kind)
        {
            switch (kind)
            {
                case BattleBenchmarkRecorderKind.FrameTime:
                case BattleBenchmarkRecorderKind.MainThread:
                case BattleBenchmarkRecorderKind.RenderThread:
                case BattleBenchmarkRecorderKind.GpuFrame:
                    return "FrameTimingManager completed frame";
                case BattleBenchmarkRecorderKind.ManagedAllocation:
                    return managedAllocation.Source;
                case BattleBenchmarkRecorderKind.DrawCalls:
                    return drawCalls.Source;
                case BattleBenchmarkRecorderKind.TotalMemory:
                    return "Profiler.GetTotalAllocatedMemoryLong";
                case BattleBenchmarkRecorderKind.GraphicsMemory:
                    return "Profiler.GetAllocatedMemoryForGraphicsDriver";
                default:
                    return string.Empty;
            }
        }

        public string Reason(BattleBenchmarkRecorderKind kind)
        {
            switch (kind)
            {
                case BattleBenchmarkRecorderKind.ManagedAllocation:
                    return managedAllocation.Reason;
                case BattleBenchmarkRecorderKind.DrawCalls:
                    return drawCalls.Reason;
                case BattleBenchmarkRecorderKind.GraphicsMemory:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    return string.Empty;
#else
                    return "Profiler.GetAllocatedMemoryForGraphicsDriver is available only in Editor or Development Player.";
#endif
                default:
                    return frameTimingReason;
            }
        }

        public void Reset()
        {
            pendingGeneration = 0;
            drainAttempts = 0;
            lastDrainUnityFrame = -1;
            frameTimingReason = string.Empty;
            attribution.ResetPending();
            managedAllocation.Abort();
            drawCalls.Abort();
        }

        public void Dispose()
        {
            Reset();
            managedAllocation.Dispose();
            drawCalls.Dispose();
        }

        private void SnapshotAndStopCounters()
        {
            managedAllocationSnapshot = managedAllocation.SnapshotAndStop();
            drawCallsSnapshot = drawCalls.SnapshotAndStop(requirePositive: true);
            totalMemorySnapshot = BattleRenderingBenchmarkMemory.CaptureTotalAllocatedMemory();
            graphicsMemorySnapshot = BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
        }

        private ulong LatestTimingTimestamp()
        {
            uint count = FrameTimingManager.GetLatestTimings(1, timings);
            return count > 0 ? timings[0].frameStartTimestamp : 0UL;
        }

        private static BattleBenchmarkMetric PositiveMilliseconds(double value)
        {
            return value > 0d
                ? BattleBenchmarkMetric.FromValue(value, "ms")
                : BattleBenchmarkMetric.Unavailable("ms");
        }
    }

    internal sealed class BattleBenchmarkCounterRecorder : IDisposable
    {
        private readonly string marker;
        private readonly string unit;
        private ProfilerRecorder recorder;
        private bool valid;

        internal BattleBenchmarkCounterRecorder(
            ProfilerCategory category,
            string profilerMarker,
            string metricUnit)
        {
            marker = profilerMarker;
            unit = metricUnit;
            Source = "ProfilerRecorderHandle:" + marker;
            var handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);
            for (int index = 0; index < handles.Count; index++)
            {
                ProfilerRecorderDescription description =
                    ProfilerRecorderHandle.GetDescription(handles[index]);
                if (description.Category != category ||
                    !string.Equals(description.Name, marker, StringComparison.Ordinal))
                {
                    continue;
                }
                try
                {
                    recorder = new ProfilerRecorder(
                        handles[index],
                        1,
                        ProfilerRecorderOptions.Default);
                    valid = recorder.Valid;
                }
                catch (Exception ex)
                {
                    Reason = "ProfilerRecorder start failed: " + ex.GetType().Name;
                }
                break;
            }
            if (!valid && string.IsNullOrEmpty(Reason))
                Reason = "The exact profiler counter was not discovered on this platform.";
        }

        internal string Source { get; }
        internal string Reason { get; private set; } = string.Empty;

        internal void Restart()
        {
            if (!valid)
                return;
            recorder.Reset();
            Reason = string.Empty;
            recorder.Start();
        }

        internal BattleBenchmarkMetric SnapshotAndStop(bool requirePositive = false)
        {
            if (valid)
                recorder.Stop();
            BattleBenchmarkMetric metric = BattleBenchmarkCounterSamplePolicy.Capture(
                valid,
                valid ? recorder.Count : 0,
                valid && recorder.Count > 0 ? recorder.LastValue : 0L,
                unit,
                requirePositive,
                out string reason);
            if (!string.IsNullOrEmpty(reason))
                Reason = reason;
            return metric;
        }

        internal void Abort()
        {
            if (valid)
                recorder.Reset();
        }

        public void Dispose()
        {
            recorder.Dispose();
            valid = false;
        }
    }

    public static class BattleBenchmarkCounterSamplePolicy
    {
        public static BattleBenchmarkMetric Capture(
            bool recorderValid,
            int sampleCount,
            long value,
            string unit,
            bool requirePositive,
            out string reason)
        {
            if (!recorderValid)
            {
                reason = "The exact profiler counter was not discovered on this platform.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            if (sampleCount <= 0)
            {
                reason = "The profiler counter produced no completed-frame sample.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            if (requirePositive && value <= 0L)
            {
                reason = "The profiler counter returned zero for a non-empty benchmark render workload.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            reason = string.Empty;
            return BattleBenchmarkMetric.FromValue(value, unit);
        }
    }

    public static class BattleBenchmarkOwnedTextureMemoryPolicy
    {
        public static BattleBenchmarkMetric Capture(
            int resourceGeneration,
            int ownedTextureResourceCount,
            long measuredBytes,
            out string reason)
        {
            if (resourceGeneration <= 0)
            {
                reason = "The benchmark-owned texture evidence has no valid resource generation.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }
            if (ownedTextureResourceCount <= 0)
            {
                reason = "The benchmark resource generation owns no Texture2D or RenderTexture objects.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }
            if (measuredBytes <= 0L)
            {
                reason = "Profiler.GetRuntimeMemorySizeLong returned no positive bytes for the benchmark-owned texture resources.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }

            reason = string.Empty;
            return BattleBenchmarkMetric.FromValue(measuredBytes, "bytes");
        }
    }

    public interface IBattleRenderingBenchmarkPresenter : IDisposable
    {
        string Implementation { get; }
        string EffectiveBackend { get; }
        string ResourceMode { get; }
        string DrawMode { get; }
        int RenderTargetWidth { get; }
        int RenderTargetHeight { get; }
        int ResolvedCommandCount { get; }
        int MaterializedRenderItemCount { get; }
        int ResourceSegmentCount { get; }
        int SubmissionDrawCount { get; }
        string SubmissionDrawMetricSource { get; }
        string SubmissionDrawUnavailableReason { get; }
        int ResourceGeneration { get; }
        int OwnedTextureResourceCount { get; }
        int OwnedResourceCount { get; }
        long CachedOwnedResourceMemoryBytes { get; }
        long MeasureOwnedResourceMemoryBytes();
        long MeasureOwnedTextureMemoryBytes();
        BattleCentralBuildDiagnostics Diagnostics { get; }
        double Present();
    }

    internal static class BattleRenderingBenchmarkPresenterFactory
    {
        internal static IBattleRenderingBenchmarkPresenter Create(
            BattlePresentationBackendMode backend,
            BattleRenderingBenchmarkWorkload workload)
        {
            switch (backend)
            {
                case BattlePresentationBackendMode.CentralOnly:
                    return new BattleBenchmarkCentralPresenter(workload);
                case BattlePresentationBackendMode.LegacyOnly:
                    return new BattleBenchmarkLegacyPresenter(workload);
                default:
                    throw new ArgumentOutOfRangeException(nameof(backend));
            }
        }
    }

    internal sealed class BattleBenchmarkCentralPresenter : IBattleRenderingBenchmarkPresenter
    {
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleBenchmarkResourceSet resources;
        private readonly BattleDynamicMeshBackend backend = new BattleDynamicMeshBackend();
        private readonly long cachedOwnedResourceMemoryBytes;
        private int lastSubmissionDrawCount = BattleRenderingBenchmarkSubmissionPolicy.Unavailable;
        private bool disposed;

        internal BattleBenchmarkCentralPresenter(BattleRenderingBenchmarkWorkload workload)
        {
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            resources = new BattleBenchmarkResourceSet("Central");
            Present();
            cachedOwnedResourceMemoryBytes = MeasureOwnedResourceMemoryBytes();
        }

        public string Implementation => "BenchmarkCentralPersistentDynamicMesh";
        public string EffectiveBackend => BattlePresentationBackendMode.CentralOnly.ToString();
        public string ResourceMode => BattleSpriteCentralBindingMode.SourceTexture2D.ToString();
        public string DrawMode => BattleCentralDrawMode.OrderedChunks.ToString();
        public int RenderTargetWidth => resources.RenderTargetWidth;
        public int RenderTargetHeight => resources.RenderTargetHeight;
        public int ResolvedCommandCount => backend.Diagnostics.ResolvedCommandCount;
        public int MaterializedRenderItemCount => backend.Diagnostics.ResolvedCommandCount;
        public int ResourceSegmentCount => backend.Diagnostics.SegmentCount;
        public int SubmissionDrawCount => lastSubmissionDrawCount;
        public string SubmissionDrawMetricSource => "Graphics.DrawMesh calls issued by the central presenter";
        public string SubmissionDrawUnavailableReason =>
            "Application is not in Play Mode; the central presenter built mesh segments but did not call Graphics.DrawMesh.";
        public int ResourceGeneration => resources.ResourceGeneration;
        public int OwnedTextureResourceCount => resources.OwnedTextureResourceCount;
        public int OwnedResourceCount => disposed
            ? 0
            : resources.OwnedResourceCount + backend.AllocatedChunkCount;
        public long CachedOwnedResourceMemoryBytes => disposed ? 0L : cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
            if (disposed)
                return 0L;
            long bytes = resources.OwnedResourceMemoryBytes;
            for (int index = 0; index < backend.AllocatedChunkCount; index++)
            {
                Mesh mesh = backend.GetChunkMesh(index);
                if (mesh != null)
                    bytes += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            return bytes;
        }
        public long MeasureOwnedTextureMemoryBytes() => resources.OwnedTextureMemoryBytes;
        public BattleCentralBuildDiagnostics Diagnostics => backend.Diagnostics;

        public double Present()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleBenchmarkCentralPresenter));
            long started = Stopwatch.GetTimestamp();
            backend.Build(
                workload.FrozenFrame,
                resources,
                BattleCentralDrawMode.OrderedChunks);
            double elapsed = BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
            int issuedDrawCalls = 0;
            if (Application.isPlaying)
            {
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    Graphics.DrawMesh(
                        backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity,
                        segment.Material,
                        BattleBenchmarkResourceSet.BenchmarkLayer,
                        resources.Camera,
                        segment.SubMeshIndex,
                        null,
                        false,
                        false,
                        false);
                    issuedDrawCalls++;
                }
            }
            lastSubmissionDrawCount = BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(
                issuedDrawCalls > 0,
                issuedDrawCalls);
            return elapsed;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            backend.Dispose();
            resources.Dispose();
        }
    }

    internal sealed class BattleBenchmarkLegacyPresenter : IBattleRenderingBenchmarkPresenter
    {
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleBenchmarkResourceSet resources;
        private readonly GameObject root;
        private readonly Transform[] transforms;
        private readonly SpriteRenderer[] renderers;
        private readonly long cachedOwnedResourceMemoryBytes;
        private bool disposed;

        internal BattleBenchmarkLegacyPresenter(BattleRenderingBenchmarkWorkload workload)
        {
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            resources = new BattleBenchmarkResourceSet("Legacy");
            root = new GameObject("NTSD Benchmark Legacy Presenter")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = BattleBenchmarkResourceSet.BenchmarkLayer,
            };
            transforms = new Transform[workload.CommandCount];
            renderers = new SpriteRenderer[workload.CommandCount];
            for (int index = 0; index < workload.CommandCount; index++)
            {
                var child = new GameObject("LegacyCommand" + index)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    layer = BattleBenchmarkResourceSet.BenchmarkLayer,
                };
                child.transform.SetParent(root.transform, false);
                child.transform.localScale = NTSDRenderSpace.RenderScale;
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                renderer.sprite = resources.Sprite;
                renderer.sharedMaterial = resources.Material;
                transforms[index] = child.transform;
                renderers[index] = renderer;
            }
            Present();
            cachedOwnedResourceMemoryBytes = MeasureOwnedResourceMemoryBytes();
        }

        public string Implementation => "BenchmarkRendererlessLegacyCompatibilityPresenter";
        public string EffectiveBackend => BattlePresentationBackendMode.LegacyOnly.ToString();
        public string ResourceMode => "SharedSourceTexture2D";
        public string DrawMode => "UnitySpriteRendererTransparentSortAndBatch";
        public int RenderTargetWidth => resources.RenderTargetWidth;
        public int RenderTargetHeight => resources.RenderTargetHeight;
        public int ResolvedCommandCount => renderers.Length;
        public int MaterializedRenderItemCount => renderers.Length;
        public int ResourceSegmentCount => renderers.Length > 0 ? 1 : 0;
        public int SubmissionDrawCount => -1;
        public string SubmissionDrawMetricSource =>
            "Unity SpriteRenderer batching is represented by the frame draw-call counter";
        public string SubmissionDrawUnavailableReason =>
            "Legacy SpriteRenderer batching has no reliable presenter-local draw count; use drawCalls when its ProfilerRecorder counter is available.";
        public int ResourceGeneration => resources.ResourceGeneration;
        public int OwnedTextureResourceCount => resources.OwnedTextureResourceCount;
        public int OwnedResourceCount => disposed
            ? 0
            : resources.OwnedResourceCount + 1 + renderers.Length * 3;
        public long CachedOwnedResourceMemoryBytes => disposed ? 0L : cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
            if (disposed)
                return 0L;
            long bytes = resources.OwnedResourceMemoryBytes;
            if (root != null)
                bytes += Profiler.GetRuntimeMemorySizeLong(root);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                    bytes += Profiler.GetRuntimeMemorySizeLong(renderers[index]);
                if (transforms[index] != null)
                {
                    bytes += Profiler.GetRuntimeMemorySizeLong(transforms[index]);
                    bytes += Profiler.GetRuntimeMemorySizeLong(transforms[index].gameObject);
                }
            }
            return bytes;
        }
        public long MeasureOwnedTextureMemoryBytes() => resources.OwnedTextureMemoryBytes;
        public BattleCentralBuildDiagnostics Diagnostics => null;

        public double Present()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleBenchmarkLegacyPresenter));
            long started = Stopwatch.GetTimestamp();
            for (int index = 0; index < workload.CommandCount; index++)
            {
                BattleRenderCommand command = workload.FrozenFrame.GetCommand(index);
                Transform target = transforms[index];
                SpriteRenderer renderer = renderers[index];
                target.localPosition = command.Position;
                renderer.flipX = command.FlipX;
                renderer.flipY = command.FlipY;
                renderer.color = command.Color;
                renderer.sortingOrder = command.SortOrder;
                renderer.enabled = true;
            }
            return BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            BattleRenderingBenchmarkEnvironment.DestroyObject(root);
            resources.Dispose();
        }
    }

    internal sealed class BattleBenchmarkResourceSet : IBattleCentralResourceResolver, IDisposable
    {
        internal const int BenchmarkLayer = 31;
        internal const int BenchmarkRenderTargetWidth = 256;
        internal const int BenchmarkRenderTargetHeight = 256;
        private readonly Texture2D texture;
        private readonly Material material;
        private readonly Sprite sprite;
        private readonly GameObject cameraObject;
        private readonly Camera camera;
        private readonly RenderTexture renderTexture;
        private static int nextResourceGeneration;
        private bool disposed;

        internal BattleBenchmarkResourceSet(string suffix)
        {
            ResourceGeneration = Interlocked.Increment(ref nextResourceGeneration);
            if (ResourceGeneration <= 0)
                throw new InvalidOperationException("Benchmark resource generation overflowed.");
            Shader shader = Shader.Find(BattleSpriteMaterialContract.BuiltInSpriteShaderName);
            if (shader == null)
                throw new InvalidOperationException("Sprites/Default shader is unavailable for the benchmark harness.");
            texture = new Texture2D(16, 16, TextureFormat.RGBA32, false, true)
            {
                name = "NTSD Benchmark Texture " + suffix,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[16 * 16];
            for (int index = 0; index < pixels.Length; index++)
            {
                byte shade = (byte)(((index / 16 + index % 16) & 1) == 0 ? 255 : 192);
                pixels[index] = new Color32(shade, shade, shade, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 16f, 16f),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "NTSD Benchmark Sprite " + suffix;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            material = new Material(shader)
            {
                name = "NTSD Benchmark Material " + suffix,
                mainTexture = texture,
                hideFlags = HideFlags.HideAndDontSave,
            };
            cameraObject = new GameObject("NTSD Benchmark Camera " + suffix)
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = BenchmarkLayer,
            };
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.transform.position = new Vector3(4f, 4f, -10f);
            camera.cullingMask = 1 << BenchmarkLayer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            renderTexture = new RenderTexture(
                BenchmarkRenderTargetWidth,
                BenchmarkRenderTargetHeight,
                16,
                RenderTextureFormat.ARGB32)
            {
                name = "NTSD Benchmark Target " + suffix,
                hideFlags = HideFlags.HideAndDontSave,
            };
            renderTexture.Create();
            camera.targetTexture = renderTexture;
            camera.enabled = Application.isPlaying;
        }

        internal Camera Camera => camera;
        internal int ResourceGeneration { get; }
        internal int RenderTargetWidth => disposed ? 0 : renderTexture.width;
        internal int RenderTargetHeight => disposed ? 0 : renderTexture.height;
        internal Material Material => material;
        internal Sprite Sprite => sprite;
        internal int OwnedResourceCount => disposed ? 0 : 6;
        internal int OwnedTextureResourceCount => disposed ? 0 : 2;
        internal long OwnedTextureMemoryBytes =>
            disposed
                ? 0L
                : BattleRenderingBenchmarkEnvironment.RuntimeMemory(texture) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(renderTexture);
        internal long OwnedResourceMemoryBytes =>
            disposed
                ? 0L
                : BattleRenderingBenchmarkEnvironment.RuntimeMemory(texture) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(material) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(sprite) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(cameraObject) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(camera) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(renderTexture);

        public BattleCentralResourceStatus Resolve(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = new BattleCentralResolvedResource(
                texture,
                material,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(16f, 16f),
                new Vector2(0.5f, 0.5f),
                command.Color,
                0,
                0,
                BattleSpriteCentralBindingMode.SourceTexture2D);
            return BattleCentralResourceStatus.Resolved;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            if (camera != null)
                camera.targetTexture = null;
            if (renderTexture != null)
                renderTexture.Release();
            BattleRenderingBenchmarkEnvironment.DestroyObject(cameraObject);
            BattleRenderingBenchmarkEnvironment.DestroyObject(renderTexture);
            BattleRenderingBenchmarkEnvironment.DestroyObject(material);
            BattleRenderingBenchmarkEnvironment.DestroyObject(sprite);
            BattleRenderingBenchmarkEnvironment.DestroyObject(texture);
        }
    }

    internal static class BattleRenderingBenchmarkEnvironment
    {
        internal static Dictionary<string, object> Capture()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["applicationPlatform"] = Application.platform.ToString(),
                ["deviceModel"] = SystemInfo.deviceModel,
                ["editor"] = Application.isEditor,
                ["graphicsApi"] = SystemInfo.graphicsDeviceType.ToString(),
                ["gpu"] = SystemInfo.graphicsDeviceName,
                ["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion,
                ["graphicsMemoryCapacityMB"] = SystemInfo.graphicsMemorySize,
                ["resolutionHeight"] = Screen.height,
                ["resolutionWidth"] = Screen.width,
                ["runtime"] = Application.isEditor ? "Editor" : "Player",
            };
        }

        internal static double ElapsedMilliseconds(long startedTimestamp)
        {
            long elapsed = Stopwatch.GetTimestamp() - startedTimestamp;
            return elapsed * 1000d / Stopwatch.Frequency;
        }

        internal static long RuntimeMemory(UnityEngine.Object target)
        {
            return target == null ? 0L : Profiler.GetRuntimeMemorySizeLong(target);
        }

        internal static void WriteJson(string path, string json)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An output path is required.", nameof(path));
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, json, new System.Text.UTF8Encoding(false));
        }

        internal static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }
    }
}


--- File: Assets/NTSD/Docs/central-battle-render-system-plan.md ---
# 集中式战斗渲染系统方案

## 2026-07-24 P8 当前验收证据（取代下方相冲突的 v3/v4 历史快照）

本节是本文当前 P8 状态。下方 v3、v4、presentation-only 或“待重新运行”的段落只保留为历史，不能覆盖本节。

- **P8-A/B/C：**诊断、生产 factory/pool publication、像素与稳定性矩阵维持既有 PASS 范围；P8-C 仍不扩大为 skill-input opoint、全部资源或全部设备的证明。
- **P8-D v4 失败原因：**v4 的 `textureMemoryBytes` 读取全局 `Texture Memory` counter，Central/Legacy 短 probe 都得到 `0`，因此 v4 按门禁为 `Incomplete`，不能作为通过证据。Windows Player 使用 `-batchmode`/`-nographics` 的早期尝试也无法形成真实 GPU/draw-call 证据，已废止。
- **P8-D v5 契约：**`benchmarkOwnedTextureMemoryBytes` 只汇总当前 presenter generation 拥有的 `Texture2D`/`RenderTexture` 的 `Profiler.GetRuntimeMemorySizeLong`。无 generation、无 owned texture、非正内存值、非空 workload 的 `drawCalls == 0` 或任何适用必需指标样本不足都阻止 PASS。Player 使用窗口化真实 graphics device，不带 `-batchmode`/`-nographics`。每个正式 sample 最多 16 次 bounded retry；耗尽记为 `Incomplete`，不会伪造 0 值样本。
- **P8-D v5 最终矩阵：**`Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v5.json` 和对应 `-player-ab-v5.json` 共 8 份报告全部为 `ntsd-battle-rendering-benchmark-suite-v5 / Pass`。每份 Central/Legacy 都是 `120/120` 正式样本、0 个适用必需指标缺失、owned texture memory 为正、600-frame leak gate 通过，teardown 的 owned bytes/resource count 均归零；A/B 的 workload fingerprint、input fingerprint 和 final runtime checksum 一致。

| v5 report | logic tick average ms | logic tick maximum ms | Central/Legacy GPU average ms | Central/Legacy draw calls average |
|---|---:|---:|---:|---:|
| `100-editor` | `13.227` | `45.537` | `2.092 / 2.684` | `21 / 9` |
| `300-editor` | `42.752` | `198.637` | `1.397 / 3.136` | `21 / 9` |
| `500-editor` | `78.149` | `221.383` | `2.045 / 2.662` | `21 / 9` |
| `1000-editor` | `36.488` | `201.219` | `2.364 / 1.271` | `9.67 / 9` |
| `100-player` | `1.298072` | `19.8363` | `1.866752 / 2.459802` | `10 / 10` |
| `300-player` | `2.180152` | `21.4017` | `0.933555 / 2.759791` | `10 / 12` |
| `500-player` | `4.711264` | `27.0955` | `0.433101 / 2.84765` | `10 / 13` |
| `1000-player` | `9.123012` | `42.3011` | `2.600414 / 12.112324` | `10 / 17` |

这些 Editor 报告由当前 16-retry/cleanup 源码重新生成：`100/300/500/1000` 的完成时间分别为 `2026-07-24 03:00:12`、`03:06:39`、`03:12:02`、`14:10:19`。Editor `300`、`500`、`1000` 的平均 logic tick 分别为 `42.752`、`78.149`、`36.488 ms`，均超过 30 Hz 的 `33.33 ms` 平均预算；1000 最大值为 `201.219 ms`。因此 v5 `Pass` 只证明报告完整性、可比 workload 和资源/teardown 门禁通过，**不表示性能预算达标，也不表示 Central 必然快于 Legacy**。数值非单调，且受 Editor 与当前机器影响；Windows Player 1000 平均约 `9.12 ms`、最大约 `42.30 ms`，同样不能外推 Android、Adreno 或 Mali。

- **fresh 验证：**benchmark focused EditMode job `d19b6fb074a2441f97273e7edf48218b` 为 `34/34 passed`；完整 `BattleRuntimeSelfCheck` 为 `PASS`；Runtime/Editor dotnet build 为 0 errors。连续矩阵首次启动 300 Player 时曾出现一次 native exit `-805306369`，未生成报告；同一 build 的 300 单样本和完整独立重跑均退出码 0，最终 300 v5 报告通过，500/1000 也按单实例串行通过。该偶发启动失败不被隐藏，也不替代最终报告内容。
- **本轮 P1 修复：**Play Mode 退出时曾遗留 hidden benchmark runner，使请求已消费却永久停在 `RUNNING`。processor 现监听 `ExitingPlayMode` 并 fail-close，非 Play 状态会 reconcile 残留 runner；EditMode 下保留 request 供下一次 Play 执行。针对该生命周期契约新增 3 个 focused tests，均通过。
- **排除项：**P8-E Android/Adreno/Mali 真机验证由用户负责；T8 默认 `stage.dat` 部署取消/排除，不是当前未完成代码项。

## 2026-07-22 Rendererless Central Mount 收口（当前结论）

- **生产 prefab 与默认路径：**`EntityObject` 和 `Shadow` prefab 的对应节点已挂载 `BattleCentralPresentationMount`；生产 prefab 中原有的持久 `Entity`/`Shadow` `SpriteRenderer` 已移除。common shadow 改由 `BattleCommonShadowDescriptor` 提供描述符；`LF2Sprite` 保存 renderer-independent 的 `visible`、`pic` 与 offset。默认后端为 `CentralOnly`。
- **注册、销毁、失败和陈旧帧契约：**mount 使用 `[ExecuteAlways]`，以 `gameObject.scene.IsValid()` 为 gate；prefab asset 本身不注册。Prefab Stage preview 可以参与编辑态 lifecycle，但没有 runtime handle，因而不属于生产 battle/pool 验证。mount/renderer 在 `OnDestroy` 主动移除 owner binding，避免 pool expire 后销毁对象仍在静态字典保留 destroyed wrapper。冷启动资源或提交失败 fail closed；已成功发布后出现的后续失败保留 last-good frame，并标记为 stale。该行为不回写战斗 runtime。
- **完整自动、编译与 Console 验证：**fresh 链为 mount source `15:41:46` < Unity `Assembly-CSharp.dll` `15:43:40` < 完整 `BattleRuntimeSelfCheck` result **PASS** `15:44:50`。full self-check 已包含真实 `DestroyImmediate(root)` focused 断言，覆盖销毁后的 owner binding 清理。主代理最后一次 `dotnet build` 为 **0 errors / 18 existing warnings**；此前 42 warnings 来自不同的生成视图。最新清空 Console Play/Stop 为 **0 error / 0 warning**。第一轮截图工具自身的 RenderTarget errors 不作为项目错误或项目验证证据，不能据此轮截图写 Console 为零。
- **最新 Play Mode 定向验证：**`NTSD_Battle` 最新观测为 `objects=6`、requested/effective=`CentralOnly`、`frame`/`ownership`/`submission`/`submitted=true`、`draws=6`、`sim/display tick=339` 且 `stale=false`。3 个生产 `LF2ObjectRenderer`、6 个 mount/handle 均有效，并确认 `persistent SpriteRenderer=0`。
- **前一轮视觉证据：**此前 `objects=12`、6 个生产 renderer、12 个 mount/handle、`draws=12` 的 Play 观测仍作为前一轮证据保留；`Temp/central-rendererless-game-20260722.png` 显示角色、武器和阴影。它不代表上述最新运行的对象数量。
- **Prefab Stage 与 Scene View 边界：**当前打开的 `EntityObject` Prefab Stage 仍有一个旧 `SpriteRenderer` preview instance；其 `logic=null`，可参加编辑态 lifecycle，但没有 runtime handle，属于 Prefab Stage 内存态而非生产 battle/pool 对象，不计入上述生产验证。本轮未修改或关闭用户当前的 Prefab Stage。由于当前 Scene View 位于 Prefab Stage，本轮没有 fresh Scene View 截图；此前 Scene View 证据仅作为既有记录保留。
- **范围边界：**T8 默认 `stage.dat` 部署与 Android/真机验证继续排除，不能由本轮 Editor/Play Mode 证据推出结论。

## P8 — 中央渲染可信度、可观察性与验收体系（2026-07-22 执行计划）

**状态（2026-07-24 当前结论）：P8-A/B1/B2 已完成；P8-C 生产验收为 PASS；P8-D v5 已完成 Editor 与 Windows Development Player 的四档 real-runtime A/B 验收；P8-E Android/Adreno/Mali 真机验证继续由用户负责并排除。** 当前证据与边界以本文顶部 2026-07-24 节为准。诊断只读取 immutable presentation 数据和中央后端结果，绝不回写战斗 runtime。编译、self-check、Play Mode、像素对照和性能测量是不同证据；Editor/Windows 报告不能推出 Android 真机结论，benchmark PASS 也不等于 Central 必然快于 Legacy。

### P8-A 真实架构链与既有能力基线

中央表现链为：

`DAT frameId -> BMP/file grid -> BattleSpriteCatalog -> Texture2DArray 或 OrderedPages -> immutable PresentationSnapshot -> RenderCommand -> 稳定透明排序 -> resource segment -> 持久动态 quad Mesh -> URP Pass`。

| 阶段 | 输入与输出 | 所有权、生命周期与失败行为 | 战斗真值边界 |
|------|------------|--------------------------|--------------|
| DAT/BMP grid | DAT 的 file、row、col、frame/pic 映射为 source rect | Loading 阶段解析；缺声明、越界或 hole 必须显式失败，不能猜图 | 只解释表现资源，不修改逻辑帧或状态 |
| `BattleSpriteCatalog` | typed resource key 映射到 rect、UV、pivot、metrics 与 central binding | publication/lease/retirement 管理共享资源；缺 key fail closed | metrics 可供表现定位读取，但 catalog 不成为战斗状态 |
| Atlas binding | catalog entry 绑定 `Texture2DArray` slice，或确定性的 `OrderedPages` page | 能力、内存或格式不满足时记录 fallback/refusal reason；禁止静默错图 | 只决定 GPU 资源形态 |
| PresentationSnapshot | 每个逻辑 tick 固化实体身份、可见性、位置、颜色、翻转、排序等 value-only 状态 | double buffer/atomic publish；generation 与 stable id 共同防止池复用串帧 | 只读逻辑快照，不反向驱动 runtime |
| RenderCommand | snapshot 展开为 Shadow、Entity、Overlay、HitRecord 等命令 | unsupported 或 unresolved 命令 fail closed，并保留诊断边界 | 命令不能改变对象生命周期或命中结果 |
| 排序与 segment | 按稳定实体 rank 和子序保持透明顺序，再按相邻兼容资源分段 | `A/B/A` 保持三段，不为少 draw 而重排；common shadow 当前可保持独立 `SourceTexture2D`，因此与角色形成单独 segment/draw，这是正确资源边界，不自动等于 bug | 仅决定提交顺序 |
| 动态 Mesh | segment 写入持久 quad Mesh，按 chunk 上限切分 | 复用 buffer；越界、无效资源或构建失败不得提交半成品 | 只生成顶点/索引数据 |
| URP Pass | world/Scene View 合法 camera 消费已准备好的提交数据 | 只在既定 injection point 提交；失败不回写 runtime，冷启动 fail closed，已有成功帧可按契约保留 last-good 并标记 stale | 纯表现终点 |

基线审计确认已有能力必须复用：`BattleCentralBuildDiagnostics`、`BattleRenderingDiagnosticReport`、`BattlePresentationParityDiagnostics`、首个 unresolved command、segment/chunk/draw 统计、atlas effective mode 与 fallback/refusal reason，以及 Legacy probe 对 immutable frame 的对照。P8 不重复建设这些能力；现有缺口是“指定实体/命令为何未绘制”的原因码、按 runtime handle 查询的完整快照，以及正式的正确性和性能验收矩阵。

### P8-B Diagnostic V1（第一实施批）

1. 每帧汇总必须覆盖 snapshot entity count、source/resolved/unresolved command count、segment/chunk/submission draw count，以及 atlas requested/effective mode、page count、array slice 能力与 fallback/refusal reason。
2. 支持按 `RuntimeSlotHandle` 查询；只允许在 generation 匹配时从 slot 解析当前 handle。结果至少包含 stable id、初始 OID/current DAT OID、frame/effective pic、`EntityVisible`/`ShadowVisible`、typed resource key、binding mode、array slice/page、UV、pivot、position、flip、color、sort rank、command index、segment 与 chunk。
3. 使用无字符串分配的 enum/record 表达未绘制原因。V1 至少覆盖：`None`、`InvalidRuntimeHandle`、`GenerationMismatch`、`MissingSnapshotEntity`、`PresentationVisibilityFalse`、`CommandSuppressed`、`MissingCatalogKey`、`MissingTextureOrMaterial`、`InvalidCentralBinding`、`UnsupportedRenderState`、`UnresolvedResource`、`NotSubmitted`；最终命名可按相邻代码统一，但语义不得合并丢失。
4. 逻辑 tick 热路径不得构造字符串、JSON 或扫描完整 capacity。详细文本/JSON 只在诊断显式启用或单条查询时物化；常态仅维护已有构建过程自然产生的数值和索引。
5. focused self-check 覆盖成功的 Entity/Shadow 查询、无效 handle、generation mismatch、不可见、缺 catalog、无效 binding、unsupported、unresolved 与未提交等可构造失败类，并证明查询不改变 runtime、command 或 checksum。

**2026-07-23 P8-B1/B2 fresh 证据：**B1 在首次架构复核中发现陈旧 last-good plan 仍可能报告成功、submission 未冻结 backend mutation version，以及缺 key/无效 binding/unsupported 覆盖不全；随后已加入 `StalePlan`、`BackendMutationMismatch`、submission build identity 校验，并补齐对应 focused checks。最新 `dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` 为 **0 errors / 42 existing warnings**；UnityMCP 刷新当前 Editor 后，相关源码时间早于 `Assembly-CSharp.dll` `2026-07-23 01:40:53`，fresh full `BattleRuntimeSelfCheck` 于 `01:41:55` 返回 **PASS**。B2 新增 `NTSD/Battle Rendering/Central Entity Diagnostic`、确定性 JSON 导出和 request-file 入口；定向 EditMode test 为 **1 passed / 0 failed**。真实 `NTSD_Battle` Play 查询 slot 0 返回 `reason=None`、`submitted=true`、requested/effective pixel mode=`CentralOnly`、source/resolved=`6/6`、unresolved=`0`；未占用 slot 399 明确返回 `ArgumentOutOfRangeException`，不会生成伪成功报告。该证据证明诊断契约与查询工具，不等于 P8-C 全像素矩阵或 P8-D 性能结论；最终架构复核仍需单独记录。

### P8-C 正确性与稳定性验收矩阵

| 场景 | 自动证据 | Play/像素证据 |
|------|----------|---------------|
| pool reuse 1000 次、runtime slot generation 重用 | handle、stable id、资源与诊断不串代 | 抽样观察复用后画面 |
| 超过预热数量的动态 pool 扩容 | 新对象拥有独立 mount、handle、command 与资源 | 连续 opoint 后全部可见 |
| Texture2DArray slice/UV 与 OrderedPages fallback | slice/page、UV、rect、fallback reason 断言 | 代表性角色/武器像素对照 |
| `A/B/A` 资源交错 | segment 保持原序，不合并为 `A/A/B` | 重叠透明对象截图 |
| Shadow/Entity/Overlay/HitRecord 顺序 | command rank/sub-order 断言 | 重叠与遮挡像素对照 |
| Mesh chunk 边界 | 4095/4096/4097 等边界与索引完整性 | 高实体压力画面无缺失/伪影 |
| 缺资源 fail closed | reason code、零错误资源提交、last-good/stale 契约 | 资源故障夹具下不显示错图 |
| Legacy 对照 | Editor-only probe 与 immutable frame 字段相等 | 同一暂停帧的 Legacy/Central 像素差异报告 |

自动断言只能证明覆盖到的契约；Play/像素验证由目标场景证明可观察结果，二者都不能单独扩大为所有资产和设备已正确。

**2026-07-23 P8-C 当前证据（覆盖本节此前“Play/像素项待实施”的历史快照）：**`Temp/P8-C-EditModeTest/P8-C-report.json` 为 **PASS**，以 `256x256` 像素夹具覆盖 generation reuse 1000 次、超预热隔离扩容 33 个 mount、Texture2DArray/OrderedPages fallback、透明 `A/B/A`、Shadow/Entity/Overlay/HitRecord 遮挡、4095/4096/4097 chunk、缺资源 fail-closed 与 rendererless frozen-frame Legacy/Central 像素对照。`Temp/P8-C-LivePool/P8-C-report.json` 为 **PASS**，真实 Play pool 中 `availableBefore=4` 时获取 5 个对象，确认越过 available 的一次动态扩容以及 5 个唯一 mount owner。旧组合 EditMode job `f278668e3a2445139c6a1a5ceb8815be` 的 11/11 保留为历史；P2 回归后的 fresh job `e455b7f70043438a938faa23e82e53f3` 为 **12/12 passed（P8-C 2 + P8-D 10，0 failed / 0 skipped）**。fresh full `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `2026-07-23 12:07:26` 为 **PASS**，freshness 为 P2 `BattleRenderingBenchmark.cs` `11:56:24` < Unity DLL `11:59:33` < result `12:07:26`。Console 过滤到的 2 条 error 均为自检刻意构造的 registration rollback / mismatched rest binding release 拒绝路径（`BattleRuntimeSelfCheck:7046` / `:1133`），无编译错误栈或 benchmark 异常。这些证据完成本矩阵定义的自动/像素/真实 pool 范围，但不扩大为全部生产资源、所有场景、所有设备或 Android 真机的结论。

### P8-D 性能验收矩阵

在同一机器、同一场景、同一逻辑输入和相同采样窗口下做 Legacy/Central A/B，对 `100/300/500/1000` active entities 记录 CPU frame/tick cost、GPU frame cost、GC Alloc、draw calls、resource segments、Texture/Graphics memory 与长时间运行后的资源/内存泄漏。报告必须同时记录 atlas/draw effective mode、分辨率、Editor/Player、warm-up 与采样帧数。

**2026-07-23 P8-D presentation-only / v3 历史证据：**`Temp/P8-D-presentation-100-ab-rerun.json`、`Temp/P8-D-presentation-300-ab.json`、`Temp/P8-D-presentation-500-ab.json` 与 `Temp/P8-D-presentation-1000-ab.json` 当时均为 **PASS**，随后 v3 又补充了 real-runtime Editor/Player 报告。它们只用于追溯；由于 v4 暴露旧 texture metric 无效，当前 P8-D 结论必须以本文顶部 2026-07-24 的 v5 八份报告为准。v3 的 Editor 1000 约 `100 ms/tick`、Player 1000 约 `2.98 ms/tick` 也是历史机器样本，不能替代 v5 数值或 Android 证据。

**额外 current-scene production 覆盖（历史补充）：**`Temp/P8-D-current-scene-ab-v2.json` 为 **PASS**。真实 `NTSD_Battle` Play 在退出前的 `SimulationWorld` 为 `ObjectCount=12`、`tick=3847`；冻结并发布的表现帧为 `EntityCount=6`、`CommandCount=12`。CentralOnly 与 benchmark-only Legacy compatibility 均实际请求/物化 `6` 个 presentation entities、`12` commands，使用同一 fingerprint `f3aaf429518f46ec` 与同一 `256x256` target；600-frame retained 检查中 Central/Legacy managed heap 分别为 `+28672 B`/`+49152 B`，graphics `+0`、owned bytes `+0`、resource count 不变。该 production presentation 样本继续保留，但当前容量、logic tick、纹理与 Windows Player 性能门禁只由顶部 v5 报告证明。

### P8-E 执行顺序与状态

| 批次 | 内容 | 当前状态 |
|------|------|----------|
| P8-A | 文档化真实链路并审计已有 diagnostics/parity 能力 | **已完成基线审计** |
| P8-B1 | reason enum、allocation-safe records、runtime-handle/slot 查询、focused self-check | **已完成；fresh full self-check PASS** |
| P8-B2 | Editor 可视查询/导出与按实体定位工具 | **已完成；EditMode、真实 Play slot 查询与确定性 JSON 导出通过** |
| P8-C | 自动正确性/稳定性矩阵，以及 Play/像素证据入口 | **已完成；生产验收 PASS：真实 factory 初始化、pool expansion 与 `SimulationWorld` publication 的角色/武器 Legacy/Central 像素对照均为 maximum diff `0`。不扩大为 skill-input opoint、全部资源或所有设备的结论** |
| P8-D | Editor/Windows Standalone Player real-runtime benchmark harness 与结构化报告 | **v5 已完成：8 份 `*-v5.json` 均 PASS；每个 backend 120/120 样本、16-retry 上限、必需指标完整、owned texture 为正、leak/teardown 通过、A/B workload identity 一致。fresh Editor 1000 平均约 `36.488 ms/tick`、最大约 `201.219 ms`，且 Editor 300/500/1000 平均均超过 30 Hz 预算；Player 1000 平均约 `9.12 ms`、最大约 `42.30 ms`。PASS 不等于性能达标或 Central 必然快于 Legacy** |
| P8-E | Android、Adreno/Mali 外部设备验证 | **用户负责 / 本轮排除** |

本计划不修改 DAT、战斗逻辑、1.5 倍实体缩放或既有渲染行为。T8 默认 `stage.dat` 部署继续取消/排除；Android 真机验收由用户处理。P8-C/D 只能在各自自动、像素或性能证据完成后更新状态，不能由 P8-B1/B2 的诊断证据提前替代。

## 2026-07-22 Editor Scene View Preview Validation

- **Scope and guardrails:** `CentralOnly` submits the same immutable mesh from the base Scene View camera only under `#if UNITY_EDITOR` and `Application.isPlaying`. Only the exact world camera may update renderer readiness. Scene View preview does not alter combat state, Player builds, or the Game camera.
- **Freshness and automated evidence:** Unity `Assembly-CSharp.dll` timestamp `23:47:47` is newer than the relevant source timestamp `23:30`; the direct `BattleRuntimeSelfCheck` result is **PASS**.
- **Observed Play/Scene View evidence:** Play state reported `objects=12` and the central mesh reported `quads=12`. `Temp/screenshot-20260722-000938.png` shows all current entities in the Scene View. The screenshot round's tool-originated RenderTarget errors are not project evidence; this screenshot does not establish a Console-zero result.
- **Validation boundary:** This verifies the current observed Scene View preview state only. It does not establish coverage for all resource scenes. T8 default `stage.dat` deployment and Android/device validation are not part of this task.

## 2026-07-21 Fresh Final Validation（当前结论，覆盖下方旧快照）

- **CentralOnly 已可实际接管像素**：P7 的 Overlay、Shadow、Entity 与 HitRecord ownership 已全部接通。运行时诊断为 `requested/effective=CentralOnly`，且 `frame`、`ownership`、`ready`、`submitted` 均为 `true`，`draws=12`。此前“`CentralOnly` unavailable / 继续拒绝”“Overlay blocker”“P7 未完成”的表述是本次验收前的历史快照，不再代表当前状态。
- **UV 伪影已定位并修复**：根因不是战斗逻辑、Atlas rect 或翻转规则，而是 `BattleDynamicMeshBackend.ClearActive` 将 `subMeshCount` 置为 `0`；Unity 2022.3 会因此释放 native index buffer，随后重建出现错误索引，表现为黑块和三角形 UV 伪影。当前保留一个零索引的 inert submesh，避免释放该 buffer。
- **实际像素对照**：暂停同一帧的 Legacy/Central 截图均为 `1920x1080`，逐像素结果 `changed=0`。截图可直接证明画面中角色、武器/球体与 Shadow 的 CentralOnly/Legacy 视觉一致性；Overlay/HitRecord 的 ownership 与资源路径由 self-check 和运行时 diagnostics 证明，不能把它们写成该截图必然可见的内容。两类证据均不扩大为所有资产和所有设备的结论。
- **最新可执行验证**：`Temp/NTSD_BattleRuntimeSelfCheck.result` 为 **PASS**；Unity Console 为 **0 error / 0 warning**。真实 Play 中显式启用 `LooseQuadtree` 后观测到 `backend=LooseQuadtree`、`objects=12`、`tick=1436`，同样为 **0 error / 0 warning**。B2C 已有 Architect final **PASS / no P0-P2**，不再处于“无 Architect 复核”的状态。
- **Editor Profiler 基线（非真机）**：Legacy 为 `6.1884 ms CPU / 0.346112 ms GPU / 18 draws`；Central 为 `6.5114 ms CPU / 0.70656 ms GPU / 20 draws`。Central 当前内存为 `1391.17 MB allocated / 1005.19 MB graphics`。这些数值是当前 Editor 样本的观测，不代表性能优于 Legacy，也不是移动端预算结论。
- **仍未关闭的外部验收边界**：尚无真实 Adreno/Mali 设备和 Android Player 的像素、兼容性、内存与性能证据，因此不能给出真机结论。T8 默认 `stage.dat` 资产部署按用户要求继续暂缓，且不作为本计划的未完成代码项。

> **历史快照说明**：下方直到下一次明确更新前出现的 `CentralOnly` 被拒绝、Overlay 缺失/阻塞、P7 未完成、B2C 无 Architect final、Play/pixel/Profiler 未验收等措辞，均记录其当时阶段，已由本节的 fresh final validation 取代，不能用于当前状态判断。

## 2026-07-21 P7 Batch6 per-entity Overlay 当前收口（覆盖本文件此前 Overlay blocker 快照）

- **代码、编译、自检和架构复核状态**：P7 Batch6 已完成 per-entity Overlay 的代码侧收口；最新相关源码 `2026-07-21 16:01:49` < Unity `Assembly-CSharp.dll` `16:03:35` < 完整 `BattleRuntimeSelfCheck` result `16:04:54` **PASS**。Unity Console 为 **0 C# error**；最后一次主代理 `dotnet build` 为 **0 errors / 18 existing warnings**；Architect final 为 **PASS / no P0-P2**。`git diff --check` 仍由主任务最终统一执行。
- **权威与资源边界**：`WORDS0.bmp` 至 `WORDS5.bmp` 已加入 Unity Assets，且其 SHA256 与权威 C# host 所引用的运行时依赖来源一致。这只是资源依赖与字形表的核验；战斗逻辑权威仍唯一为 `J:\QQFile\NTSD2.4\ntsd_release_C#`，不引入第三逻辑权威。
- **Catalog / prewarm**：`BattleSpriteCatalog.CommonWordGlyph(sheet, charCode)` 为 typed key，覆盖 `6 * 256` glyph；权威 top-left source rect 在 catalog 中转换为 Unity bottom-left rect。`CharacterAnimtorManager` 对 WORDS 采用 exact-black transparency、Point filter、Clamp wrap、事务式/atomic publication，并把 1536 个 glyph 的 Sprite 纳入 retirement ownership。
- **运行时与布局契约**：`BattleSlotLabelRuntimeState` 保存 `char[10,12]` 标签及 `int[10]` 状态，reset 与 `MatchConfig` bootstrap 均已接线。无分配的 `BattleEntityOverlayLayout` 覆盖 `Hp2Orig > 1` 复活 counter、普通标签、`[label]`、普通 `Com` 与特殊 `WORDS5` `Com`；标签位置 clamp，counter 不 clamp，容量不足 fail-closed。
- **snapshot / command / legacy**：snapshot 同时保留原始 `ObjectId`（供 shadow 223/224 gate）和 current DAT identity（供 Overlay），命令顺序为 `Shadow -> Entity -> OverlayGlyph -> HitRecord`。`BattleEntityOverlayRenderer` 在 legacy 路径使用 pooled `SpriteRenderer` materialize，并核验 generation/stable-id；默认 `LegacyOnly` 仍发布 immutable frame 但不构建 central mesh。`CentralShadowBuild` 保留诊断职责，`CentralOnly` 仍由 `ValidateAvailable` 显式拒绝。
- **生命周期与检查**：frame-level catalog lease 保护发布资源，HitRecord cycle lease 由 finalizer 释放，empty frame 不 retain；self-check 覆盖 retirement 窗口。布局检查还覆盖 HP2、slot/bracket/empty/Com、palette、特殊 OID/type/hitstop、clamp、fail-closed、命令序列与 zero-GC。
- **未验收边界**：本批不等于 P7 全门槛完成。Play Mode、像素基线、Profiler、Adreno/Mali 和真机均未验收；T8 默认 `stage.dat` 部署继续按用户要求排除。下文所有“Overlay 未实现”“WORDS 缺失”“confirmed blocker”或“Overlay 阻塞 CentralOnly/P7”的陈述均为 Batch6 之前的历史快照，已由本节覆盖，除非明确标为历史。

## 2026-07-21 B2C Extended checksum、当前 world 查询、P1-P6 与 P7 Batch1-5 状态

- **代码已实施 / fresh self-check 已通过 / 最终架构复审待补**：`Authority400` 继续冻结为 `ntsd-battle-trace-v3`，direct parity capture 仍严格拒绝非 `Authority400/400`；`MobileExtended` / `DesktopExtended` 通过通用 checksum API 生成独立 `ntsd-unity-extended-battle-checksum-v1`，旧 `LastFrameSnapshot` 仍只表示 Authority v3。
- Extended metadata 覆盖 profile、logical capacity、claimed/object count 与 tick；slot 域覆盖 slot、claimed、generation、stable ID、current DAT OID、active entity runtime 及已物化但未 claimed 的 raw runtime。读取未物化槽不会创建分页。
- ARest/VRest 使用按 victim/attacker 稳定排序的稀疏投影，不构造 `capacity²` 矩阵；claimed entity 若未绑定当前 world 的 rest store 或 victim slot 不一致，capture 会拒绝生成 checksum。
- focused self-check 覆盖 Extended 的 Mobile `1050` / slot `1049`、Desktop `512 -> 768` / slot `700`、高槽 ARest/VRest、raw runtime、generation/stable-ID reuse、profile separation、稀疏 VRest 与 non-mutating repeat capture；同时覆盖 AI Loose Quadtree 查询与即时 weapon/body current-world 查询的结果/回退契约。最新 full self-check `2026-07-21 00:48:06` **PASS**；`dotnet build` **0 errors / 42 existing warnings**。
- 即时 body/weapon 查询已在显式 `LooseQuadtree` 后端下使用当前 world 实体的空间查询，AI 输入快照已使用 generation-aware Loose Quadtree 查询；索引/几何/映射异常均回退 brute，生产默认仍为 `BruteForce`。
- **P1 排序止血已完成代码层收口**：活跃实体按 `(ZInt, runtime slot)` 排序后分配 dense presentation rank；四个短期子序为 `Shadow=0`、`Entity=1`、`Overlay=2` 和 `HitRecord=3`。权威 host 确实在 Entity 后绘制 per-entity Overlay；Unity 保留了子序但尚未实现对应消费者，这是 confirmed blocker。Shadow、Entity、spark 及其 `SortingGroup` 均统一为 Unity `Object` sorting layer，因此排序层不会先于 compact order 打断实体间交错。旧的 `logicalZ * 4096 + runtimeSlot * 4` 映射已移除。
- **P1 容量边界**：旧 `SpriteRenderer` 后端明确 guard 为最多 `8192` 个 materialized active entities；`8193` 会清晰抛错。移动端 `1000` active 预算在此范围内；`DesktopExtended` 在中央渲染后端完成前仍有这个临时表现上限，不等同于 runtime slot 容量上限。
- **P1 自动验证**：真实双实体四 renderer 的 `ForceRefresh` 检查验证 `Shadow(A)=0`、`Entity(A)=1`、`Shadow(B)=4`、`Entity(B)=5`，并覆盖 generation/高 slot 与 sorting layer/order。fresh 链为 source `2026-07-21 03:00:45` < Unity DLL `03:05:59` < full `BattleRuntimeSelfCheck` `03:07:05` **PASS**；`dotnet build Assembly-CSharp.csproj --no-restore -v:q` 为 **0 errors / 42 existing warnings**；最终 architect review 为 **PASS / no blocker**。
- **P2 immutable Catalog 已完成代码层收口**：`BattleSpriteCatalog` 的唯一 key 为 `(LF2Entity.ResolveCurrentDataObjectId(entity), effectivePic)`；不可变 entry 保存 source sheet、共享 `Texture2D`、Unity bottom-left 像素 rect、归一化 UV、宽高 metrics、pivot 和兼容旧 `SpriteRenderer` 的 legacy `Sprite`。正式 prewarm 使用 invocation-local staging 与 generation/disposed gate，只有本轮所有 sheet 成功且仍为当前 generation 时，才将 configs、`MergedSprites` 与 catalog 原子 publish；失败、过期结果和 teardown 均清理本轮资源。
- **P2 图片索引与生命周期契约**：partial BMP 严格按声明的 row/col 和 `localPic` 建立稀疏 rect，保留未声明图片的 holes；normal/swapped 网格仅在完整匹配时择优，并已覆盖 weapon6、weapon3 等生产矩阵。renderer 对 catalog 持有引用计数屏障，旧 catalog 只有在零引用后才退役，避免异步替换期间释放仍在显示的共享 texture/sprite。
- **P2 生产消费者已迁移**：display、collision、anchor、SpecialAttack point-center 与 shadow metrics 在战斗期不再读取 `Sprite.rect`；`pic=999`、缺 key、current DAT identity 切换和 pool reuse 均会隐藏并清除旧 sprite/catalog 引用。`MergedSprites` 仅保留兼容和预览用途，不再定义战斗期 metrics 真值。
- **P2 自动验证**：focused/full self-check 覆盖双文件边界、normal/swapped row/column、partial holes、rect/UV/pivot/shared texture、current identity replacement、missing/`999`、pool reuse、原子 publish、stale/teardown cleanup、renderer refcount retirement 及全部 metrics 消费者。fresh 链为 source `2026-07-21 04:16:00` < Unity DLL `04:17:06` < full `BattleRuntimeSelfCheck` `04:18:04` **PASS**；fresh dotnet build 为 **0 errors**。不同的自动生成 `.csproj` 刷新视图分别显示 18 或 42 条既有 warnings，因此不把 warning 数量冻结为 P2 契约。最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。
- **P3 shadow-build 已完成代码层收口**：渲染模式明确为默认 `LegacyOnly` 与诊断用 `CentralShadowBuild`；`CentralOnly` 明确拒绝。每个逻辑 tick 生成 value-only immutable snapshot/commands，按 `(ZInt, runtime slot)` 为每个实体稳定展开 `Shadow -> Entity -> Overlay -> HitRecord`。早期 `AuthorityExpectedButLegacyMissing` 标记来自不完整权威盘点，现已废止；权威两个 host 实际都绘制 per-entity Overlay，Unity 尚未实现，因此不能宣称 overlay 等价。
- **P3 发布与真实 legacy probe**：snapshot/commands 使用 double buffer、几何增长容量和 atomic publish；persistent scratch 保证 steady `RenderDispatch` self-check 为 zero allocation。legacy probe 直接采样真实 renderer 的 sprite、texture、material instance、rect、pivot、position、flip 与 sorting；HitRecord 在 legacy advance 前采样，避免把推进后的 spark 状态错配到当前 tick。
- **P3 catch-up 与 spark 契约**：同一渲染帧追赶多个逻辑 tick 时，无法对中间 tick 取得实际 legacy renderer 状态，因此显式发布 `Incomplete`，记录 incomplete count、first tick 与 last tick；仅最后可观测 tick 进入完整 probe，不宣称所有逻辑 tick 均已实际 legacy parity verified。zero-hit 仍通过 `SparkRenderer.RenderAll` finalize；正式 production pool 路径覆盖 nonzero spark atlas cells、每 tick 只 age 一次，以及 `OnDisable`/`OnDestroy` 归还池。
- **P3 隔离与验证**：P3 snapshot/command/diagnostic 不进入战斗 checksum，也不反写 runtime 真值。fresh 验证链为 source `2026-07-21 05:38:38` < Unity DLL `05:39:29` < full `BattleRuntimeSelfCheck` `05:40:16` **PASS**；dotnet build **0 errors / 18 existing warnings**（root 当前视图）；最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。
- **P4 Mesh/URP 代码层已完成**：中央后端复用持久 Mesh，并以每 chunk `4096` quad、`16384` 顶点、`24576` 索引的 `UInt16` 契约切分。`OrderedChunks` 只合并原命令流中相邻且状态兼容的命令，保持 `A,A,B,A` 原顺序；`StrictOrderedDraw` 提供逐命令正确性回退。跨 chunk 顺序、unresolved command barrier 与 stale mesh/submesh clear 均已进入 self-check。
- **P4 提交边界与 URP 接线**：`LegacyOnly` 不构建中央 Mesh，`CentralShadowBuild` 只构建诊断数据而不提交 draw，`CentralOnly` 在所有类别 ownership 完成前仍明确拒绝。URP pass 只接受 world camera 的 `Base` camera，并注入 `AfterRenderingTransparents`。`BattleRenderFeature` 已作为 active renderer asset 的唯一 subasset 安装并经安装器验证，不依赖场景临时对象。
- **P4 registration 修复**：初审发现 feature B 覆盖 feature A 后，注销 B 不会恢复 A 的配置。现改为可复用 registration stack，并由 `A -> B -> unregister B -> restore A` 自检验证 fallback material、array material 与 draw mode 全部恢复。
- **P4 自动验证**：fresh 链为 source `2026-07-21 06:32:00.287` < Unity DLL `06:32:56.970` < full `BattleRuntimeSelfCheck` result `06:33:43.796` **PASS**；dotnet build **0 errors / 42 existing warnings**；最终 architect review **PASS / no P0-P2 findings**。
- **P5 Atlas Array/fallback 代码层已完成**：确定性 planner 将 whole-sheet 资源放入 `2048 x 2048` 多页布局，使用 normalized path ordinal 去重；同路径同尺寸但像素内容冲突会拒绝，不允许加载顺序决定结果。每张 sheet 周边做 `1px` extrusion。能力 gate 满足时构建 `RGBA32 Texture2DArray`，否则按相同 page 顺序使用有序 Texture2D fallback。
- **P5 Catalog 与所有权**：catalog entry 保留 P2 legacy source，同时增加 immutable central binding。manager 使用事务式 publish；所有新建 Unity `Object` 在构造起点即进入 ownership，只有完整成功后才发布。legacy renderer lease 与 central consumer lease 都会延迟旧 atlas/catalog 退役，避免异步换代时释放仍被使用的 texture/material。
- **P5 绘制契约**：array 路径把 slice 写入 per-vertex 数据，相邻但跨 slice 的命令可在相同 array material 下保持原序合批；2D fallback 的 `A/B/A` 必须保持三段，不能为减少 draw call 重排。array/fallback 各有 shader、material 与 pass 配置，installer 同时验证两条资源链。
- **P5 复核修复**：首轮复核关闭两个 P2。其一，同 normalized path、同尺寸、不同 pixels 的输入现在对两种排列都拒绝，只有 equal-content duplicate 成功。其二，2D fallback 页在构造时即 owned；显式两页夹具中 page0 成功、page1 失败后两页均销毁，且没有 partial publication，关闭异常页 ownership 泄漏。
- **P5 自动验证**：fresh 链为 source `2026-07-21 07:06:28` < Unity DLL `07:07:12` < full `BattleRuntimeSelfCheck` log `07:08:13` **PASS**；dotnet build **0 errors / 42 existing warnings**；architect final **PASS / no P0-P2 findings**，code review **no P0-P2 findings**。
- **P6 设备策略代码侧已完成**：`BattleRenderingDevicePolicy` 以 immutable capabilities 表示设备边界，只有 `FromSystem` 接触 `SystemInfo`。策略解析严格遵循 CLI > `GameConfig` > Auto，命令行为 `-ntsdBattleAtlasMode` 与 `-ntsdBattleDrawMode`；非法显式值拒绝，不静默改写。Atlas 在 `TextureArray` 与 `OrderedPages` 间安全 fallback 并记录原因；draw mode 支持 Auto、`OrderedChunks`、`StrictOrderedDraw`，`SingleMesh` 不进入生产选择。
- **P6 发布与诊断契约**：resolver 生成显式、确定性的 JSON report，包含 capability、请求、effective mode 与 fallback reason。manager 每次 publication 只解析一次，central backend 缓存 effective draw mode；逻辑 tick 热路径不再查询 `SystemInfo` 或 CLI。该策略不改变 runtime profile、capacity、tick、collision、checksum 或 `CentralOnly` guard。
- **P7 held-object 子批已完成**：权威调用链按 `InteractionRuntimePasses -> WeaponPointRuntime/WeaponRuntime -> SdlBattleRenderer/BattleHostForm` 对照。legacy 与 presentation snapshot 共用纯 held-offset helper；offset 在 capture 时固化为 immutable 值，并追加到 Entity command，不从后续 renderer 状态回读。
- **P7-held 覆盖**：right/left facing、target mismatch、release、missing holder/wpoints、slot generation reuse、dormant holder，以及 legacy/central equality 均进入 self-check。旧 handle 或 inactive/dormant 不会把新 occupant 或过期 held offset 带入当前 command。
- **统一验证**：latest fresh 链为 self-check source UTC `23:42:44` < Unity DLL `23:44:03` < `Unity-P6-P7-Final2-SelfCheck.log` `23:45:00` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS**，code review **approve / no P0-P2 findings**。
- **P7 Batch2 render-state semantic parity 已完成**：snapshot/command 以 value-only `Color32`、`flipX/flipY`、mask/material semantic 和 logical resource key 表达状态，Unity instance ID 仅保留为诊断。catalog 增加 immutable `Sprite -> key[]` 反查和 preferred entity key；legacy probe/Compare 检测 RGB、alpha、flipY、unsupported state 与 logical key。
- **P7 Batch2 central/mesh 契约**：central resolver 转发 color 并对未知语义 fail closed。Mesh 将 color 写入 quad 四个顶点，flipY 通过 V 坐标交换实现；仅 color 不切 segment，material semantic variant 必须断段。pool checkout 将 entity/shadow/spark 规范化为 white、`flipX/flipY=false`、mask none，并在首次干净 checkout 借用 `Sprites/Default.sharedMaterial`，禁止触发 `.material` 实例化。
- **P7 Batch2 alpha contract**：依据 Unity `2022.3.4f1` 官方 builtin shaders ZIP changeset `35713cd46cd7`，两个中央 shader 改为 `Blend One OneMinusSrcAlpha`，最终输出执行 `rgb *= a`，并声明 `NTSDAlphaContract` tag；installer 已验证 shader 为 white 基线且 tag 正确。
- **Batch2 验证**：fresh 链为 source `08:27:50` < Unity DLL `08:28:48` < self-check log `08:29:48` **PASS**；installer validation **PASS**；dotnet build **0 errors**；architect/code review **PASS / no P0-P2 findings**。
- **P7 Batch3 Shadow 已完成**：按 authority `BattleHostForm` / `SdlBattleRenderer.DrawShadow` gates 对齐。资源侧使用 typed `EntitySprite` / `CommonShadow` key；`GameConfig.ShadowPrefab` 作为 immutable borrowed binding，固化真实 sprite、texture、UV、size、pivot、color 与 material。manager 在 main thread 做 atomic common publication，borrowed Unity Object 不进入 atlas/catalog owned retirement。
- **Batch3 snapshot/resolve**：snapshot 保存 actual ObjectId 与 `HasCurrentFrame`；Shadow command 携带真实 descriptor 和 `CommonShadow` key，并保证 Shadow 在 Entity 前。legacy probe 校验 exact sprite。central resolver 校验 sprite、texture、rect、pivot 与 material ID，同时提供 source2D + fallback material；任何 missing config/resource 都 fail closed。
- **Batch3 行为矩阵**：actual OID `223/224`、state `3005/9997`、`Link < 0`、HitStop、missing frame 均与 legacy 对齐。review 关闭 P1 missing-frame 的 legacy/central 差异，以及 P2 material ID、真实 `GameConfig` asset、real commit -> replace retirement tests。
- **Batch3 验证与边界（历史快照）**：fresh 链为 source `09:29:03` < Unity DLL `09:31:10` < self-check log `09:32:07` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect/review **PASS / no P0-P2 findings**。Batch3 当时未执行 Play、实际 pixel baseline 或设备验收，HitRecord/Overlay 均未收口。后续 Batch4/5 已关闭 HitRecord resource/lifecycle 代码缺口；当前仍由 Overlay 阻塞 `CentralOnly`。T8 已排除。
- **P7 Batch4 SPARK / Common HitRecord resource ownership 已完成代码层收口**：typed `CommonSpark(pic)` 覆盖 20 帧；SPARK 经 prewarm 单次 decode/process 后于 main thread atomic publish。legacy `SparkRenderer` 不再在 `Awake` decode 或创建资源；central resolver 验证 logical key、`Sprite`、`Texture`、rect、pivot、size 与 material。publication lease/retirement 已接入 common resource lifecycle。
- **Batch4 失败与状态不变契约**：缺失或无效 SPARK 释放 stale lease，且不改变 `HitRecord` age/count；partial `Texture`/`Sprite` 构造失败会事务式清理所有已创建资源，禁止 partial publication。
- **Batch4 fresh 证据与边界**：source `11:13:05` < Unity DLL `11:15:20` < self-check result `11:17:38` **PASS**；architect re-review **PASS / no P0-P2 findings**。code-review provider 返回 `429`，没有 code-review 通过结论。Batch4 当时未包含 HitRecord lifecycle mutation；该项已由下方 Batch5 收口。Play、pixel、Profiler、真机与真实 SPARK 资源路径仍未验收；T8 继续排除。
- **P7 Batch5 HitRecord presentation cycle 已完成代码层收口**：新增 backend-neutral immutable double-buffer cycle。`RenderDispatch` 捕获 owner slot handle/generation、count、age、x/z 与 frozen common publication；`SparkRenderer` 只负责 materialize/probe，不再写 live HitRecord。`LateUpdate` 固定为 legacy materialize -> central `PrepareFrame` -> one finalizer；catch-up 只 finalize 最后一个 cycle。
- **Batch5 mutation 与隔离契约**：missing SPARK 为 zero-write；valid record 每 cycle 的 age 恰好 `+1`；invalid sampled tail 每 cycle 最多删除 1 项，age `4/14/28/38` 刚进入 gap 的同 cycle 不删除。slot reuse、count/age guard 均已覆盖，pool、camera 与 backend 选择不改变 mutation 结果。
- **Batch5 后续 P2 修复**：common binding 改为 direct ownership transfer，不再依赖 per-tick lease GC；no-hit cycle 不持 binding。coordinator reset 已接入 world reset、driver unbind、world replacement 与 destroy。ordered owner cursor 为 O(N)，`1000` owners fixture 精确验证 `1000` 次 comparisons。
- **Batch5 fresh 证据**：source `12:39:24` < Unity DLL `12:40:40` < self-check result `12:41:20` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS / no P0-P2 findings**；code review **APPROVE / no P0-P2 findings**。Play、pixel 与 device 仍未验收。
- **Overlay authority re-audit confirmed blocker**：权威 `BattleHostForm` 与 `SdlBattleRenderer` 实际顺序均为 `Shadow -> Entity -> EntityOverlays -> HitRecords`。per-entity Overlay 绘制 `Hp2Orig > 1` 的复活次数和 entity label；`WORDS0..5.bmp` 以每 glyph `8x16`、步距 `9`、black colorkey 提供资源。Unity `Assets` 当前没有 `WORDS0..5`，也缺 `BattleSlotLabels[10,12]` / 对应 state 镜像与 snapshot 字段契约，因此 Overlay 未实现并继续阻塞 `CentralOnly`。global function/pause overlay 是独立后置 UI，且 GDI/SDL 行为不一致，不并入 per-entity P7，本批不处理。T8 继续排除。

本节是当前状态；下方早期阶段中“Extended Driver checksum 跳过/为空”或“Extended schema 尚未实施”的文字仅保留为当时历史边界，不再代表当前实现。

## BATTLE-RENDER-PLAN1 状态

- **状态**：方案已确认；R1-R2C-4、B0、B1-B1.3、B2A、B2B、B2C 与 **P1-P6** 已完成代码层实施；P6 真机验收未完成。P7 的 held、render-state semantic parity、Shadow、Batch4 SPARK/Common HitRecord ownership 与 Batch5 HitRecord presentation cycle 子批已完成；per-entity Overlay 是 confirmed blocker，P7 整体未完成。
- **代码状态**：独立 `BruteForce` / `LooseQuadtree` 正式 collision broadphase 后端已具备 generation-aware 增量同步；默认仍为 `BruteForce`。除 fixed-tick candidate collect 外，B2C 已接入即时 weapon/body current-world query 与 AI 输入快照查询；二者均保留失败回退 brute。
- **验证状态**：B2B、B2C/P1、P2-P5 与 P6/P7 Batch1-2 的分项证据保留在上方各节。P7 Batch3 的 fresh 证据为 source `09:29:03` < DLL `09:31:10` < self-check log `09:32:07` **PASS**；Batch4 为 source `11:13:05` < DLL `11:15:20` < result `11:17:38` **PASS**，architect re-review **PASS / no P0-P2**，code-review provider `429` 不记为通过。Batch5 fresh 链为 source `12:39:24` < DLL `12:40:40` < result `12:41:20` **PASS**，dotnet **0 errors / 18 existing warnings**，architect **PASS / no P0-P2**，code review **APPROVE / no P0-P2**。Play/pixel/device 仍未验收；Overlay 未实现，`CentralOnly` 继续拒绝。
- **容量说明**：`400` 是 `Authority400` 兼容模式的 C# 权威槽位边界，不是所有 Unity 运行模式的全局容量上限。权威 `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Common\NtsdConstants.cs` 中的 `NtsdConstants.MaxObjects` 定义 `MaxObjects = 400`，`BattleCore\Simulation\SimulationWorld.cs:28-32` 据此创建 `Objects[400]`、`VRest[400,400]` 和 `ARest[400]`；Unity `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:39-44` 以 `MaxRuntimeSlots = 400` 镜像该契约。扩展模式的 active entity 容量与 render command 容量分开管理；每个实体可产生 `Shadow`、`Entity`、`Overlay`、`HitRecord` 等多个命令，Mesh 仍须按实际命令峰值预分配并分 chunk。
- **平台 Profile 说明**：生产解析优先级固定为“命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认值”；平台宏只提供默认 Profile，不进入战斗逻辑、最小堆、Loose Quadtree、VRest 或命中规则。设备能力降级只改变图集、纹理和渲染后端，不得改变已选 Profile 的战斗容量或结果。
- **实施边界**：fixed-tick formal collect 仍在 B2B 边界对当帧 participant 做 batch synchronize，不把 registry mutation 直接写入 collision 索引。B2C 的即时 weapon/body 与 AI 查询各自从当前 world/snapshot 构建查询视图，generation、几何或映射无法验证时回退 brute；它们不改变 fixed-tick pair 的 authority ordinal、RNG 或 candidate 时序。正式 collect 结果仍按 canonical runtime-slot pair 合并、去重，再按原 authority ordinal 双向派发；任何无法证明完整性的情况均 reset 增量索引、整 tick 回退 brute-force，并原子恢复 RNG/candidate 状态。

### 2026-07-20 R1 第一批实施记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| Profile resolver | **已实施 / 已验证** | 支持显式覆盖 > 配置值 > 平台默认；平台默认由 Unity 条件编译符号选择。Editor/其他平台回落 `Authority400`，Android Player 为 `MobileExtended`，Standalone Player 为 `DesktopExtended` |
| `Authority400` 最低空闲槽分配 | **已实施 / 已验证** | 以 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused` 保留 roster、stage、dynamic band 语义；支持按索引移除、释放回收和最低槽确定性分配 |
| 正式 runtime 接线 | **兼容模式已接入** | `SimulationWorld` 仍显式固定为 `Authority400`，本批不改变 400-slot 行为边界，也不自动启用平台扩展模式 |
| 扩展容量与空间索引 | **R1 历史边界，后续已替代** | R1 当时仅有独立分页 `RuntimeSlotTable` 与 generation handle；`MobileExtended`、`DesktopExtended` 生产接线、桌面动态增长、1000 active admission、AI 与 Loose Quadtree 已由后续阶段实施，当前状态以本文件顶部 B2C 节为准 |

fresh 验证：相关源码时间 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` 结果 `12:05:07` **PASS**；分配器另以 **100,000 次随机 claim/release/allocate 操作**与朴素线性扫描模型逐步对照，结果 **PASS**；架构复核 **PASS**。这些证据只关闭 R1 第一批，不代表 Play Mode、扩展容量、四叉树或集中式渲染已经验收。

### 2026-07-20 R2A 分页槽表与 generation 句柄基础记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| `RuntimeSlotTable` 分页存储 | **基础设施已实施 / 已验证** | 固定 `PageSize = 256`，按首次访问惰性物化页面；`Authority400` 逻辑容量为 400，`MobileExtended` 设计容量为 1050，最后一页超出各自逻辑尾部的地址均被 guard 拒绝 |
| raw runtime / raw rest 存储 | **基础设施已实施 / 已验证** | 每个 slot 持有独立 `NTSDEntityRuntime` 与 `LF2ItrRestTracker.StateSnapshot` 存储；raw 状态与实体 claim 生命周期分开，不因只读查询隐式占用槽位 |
| 占用计数 | **基础设施已实施 / 已验证** | `ClaimedCount` 由 allocator 契约维护，claim、release 与 reset 后均由 focused self-check 校验 |
| `RuntimeEntityHandle` | **基础设施已实施 / 已验证** | 句柄由 `(slot, generation)` 构成；release、同槽 reuse 与 reset 都推进 generation，使旧句柄无法再 resolve 到新占用者 |
| 生产 runtime 接线 | **未实施 / 未启用** | `SimulationWorld` 仍使用现有 `Authority400` registry/raw arrays，并未切换到 `RuntimeSlotTable`；本批不改变战斗结果或现有 400-slot parity schema |

R2A fresh 验证：相关源码时间 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` 结果 `12:36:53` **PASS**；架构复核 **PASS**。这些证据只验证分页地址、惰性物化、独立 raw 存储、`ClaimedCount` 与 generation 失效契约；不代表 `Extended` 已启用，也不覆盖桌面动态增长、移动端 1000 admission、AI 迁移、Loose Quadtree 或 VRest 改造。

### 2026-07-20 R2B `Authority400` 生产 registry 迁移记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 单一槽位存储后端 | **已实施 / 已验证** | 生产 `SimulationWorld` 的 `_runtimeSlotUsed`、`_rawRuntimeSlots`、`_rawRestSlots` 已由单一 `RuntimeSlotTable` 替代；旧字段检索为 0，registry 不再维护并行槽位真值 |
| 当前占用者查询 | **已实施 / 已验证** | `FindEntityByRuntimeSlotIncludingDormant` 与 current-pass 查询直接通过 slot 地址 O(1) 解析当前 occupant；长期引用仍必须使用带 generation 的 `RuntimeEntityHandle` |
| pass 遍历时序 | **已实施 / 已验证** | 保留 live ascending slot scan：游标以上新生实体可进入本 pass，复用游标以下低槽的实体等待下一 pass，保持既有 high-newborn / low-reuse 时序 |
| release 身份保护 | **已实施 / 已验证** | release 必须同时匹配 slot 与 `expectedEntity`/当前 occupant；过期实体不能释放已被另一实体复用的槽 |
| raw rest 语义 | **已实施 / 已验证** | stage spawn 继续恢复并消费复用槽 raw rest；ordinary spawn 继续按既有语义重置，不把 R2B 存储迁移扩大成 VRest/ARest 规则变更 |
| 对外可观察契约 | **保持不变 / 已验证** | `ObjectCount`、对象 buckets、`SceneQueryHit` 的 runtime-slot 地址语义保持不变；生产 Profile 仍固定为 `Authority400` |

R2B fresh 验证：相关生产源码时间 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` 结果 `12:57:02` **PASS**；fresh `dotnet build` 为 **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索为 **0**。这些证据只关闭 `Authority400` 的生产 registry 存储迁移，不代表 `Extended`、移动端 1000 admission、桌面分页增长、AI、Loose Quadtree、VRest 解耦或集中式渲染已启用。

### 2026-07-20 R2C allocator/table 单调增长记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| `RuntimeSlotAllocator.GrowTo` | **基础设施已实施 / 已验证** | 只允许容量单调增加；增长后保留三段边界、dynamic segment 的 indexed binary min-heap、`nextUnused`、已占用槽与 `ClaimedCount`，并继续优先复用增长前的最低空洞，再使用新开放地址 |
| `RuntimeSlotTable.GrowTo` | **基础设施已实施 / 已验证** | 增长时扩展页引用数组但不主动物化新页；保留既有 page object、occupant、generation handle、raw runtime、raw rest 与 claim 状态，新页仍在首次访问时惰性物化 |
| 非增长调用 | **已验证** | 目标容量等于当前容量时成功 no-op；缩容请求返回拒绝，且容量、claims、页面、句柄和 raw 状态保持不变 |
| 移动端地址契约 | **设计边界已修正 / focused 已验证** | `1000 active` 是 admission 预算，不是逻辑地址尾值；保留 `0..49` 后，1000 个动态槽为 `50..1049`，因此逻辑地址容量是 `1050`。`PageSize=256` 时物理数组需要 5 页，但物理尾部 `1050..1279` 必须不可寻址、不可 claim、不可创建 raw runtime |
| 生产接线 | **R2C 时未实施；已由 R2C-4 后续接入** | `SimulationWorld` 在 R2C 时仍固定 `Authority400`；生产 Profile、Mobile total admission 与 Desktop 自动增长已由 R2C-4 接入 |

R2C fresh 验证：相关源码时间 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` 结果 `13:25:34` **PASS**；fresh `dotnet build` 为 **0 errors**；架构复核 **PASS**。这些证据只证明 allocator/table 可在保持既有状态与最低槽语义的前提下单调增长，并验证移动端 `1050` 逻辑地址及物理尾部 guard；不代表 Extended Profile、生产增长、移动端 admission、AI、Loose Quadtree 或集中式渲染已经启用。

### 2026-07-20 R2C-3A `SimulationWorld` 实例容量读取记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| world 容量真值 | **已实施 / 已验证** | `SimulationWorld.RuntimeSlotCapacity` 读取当前 `_runtimeSlots.LogicalCapacity`；registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量循环不再假定固定 400 |
| 默认兼容模式 | **保持不变 / 已验证** | 默认 `SimulationWorld()` 仍创建 `Authority400/400`；现有生产 Driver、400-slot parity 与默认 self-check 不会自动进入扩展模式 |
| focused 扩展契约 | **内部测试入口已实施 / 已验证** | internal 构造以 `DesktopExtended/512` 创建 focused world；slot `511` 可注册、查询并进入 AI 目标扫描，slot `512` 被拒绝，reset 后高槽状态被清理 |
| parity schema | **保持固定 / 已验证** | `BattleParitySnapshot` 继续显式使用 `AuthorityRuntimeSlotCapacity = 400`，没有把历史 400-slot certificate 静默扩展为新 schema |
| 生产与外部边界 | **R2C-3A 时 Profile 未实施；现已由 R2C-4 接入** | `MobileExtended` / `DesktopExtended` Profile 后续已接入生产 Driver；`LF2SpecialAttack` / `LF2Entity` 的外部固定容量边界已在 R2C-3B 按 world capacity 处理 |

R2C-3A fresh 验证：相关源码时间约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` 结果 `13:54:22` **PASS**；fresh `dotnet build` 为 **0 errors / 42 warnings**。这些证据证明默认 400 行为未变，并证明显式 512-slot world 的代码层容量契约可运行；扩展 Profile 当时仍未接入生产 Driver，外部 special/transition 固定边界随后由 R2C-3B 关闭。

### 2026-07-20 R2C-3B 外部容量边界与 parity guard 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| special attack 高槽 holder | **已实施 / 已验证** | `LF2SpecialAttack` 不再用固定 400 拒绝 holder slot；在已绑定 world 时按 `RuntimeSlotCapacity` 验证并解析扩展高槽 holder |
| Karasu 高槽扫描 | **已实施 / 已验证** | Karasu oid209 替换扫描使用当前 world 容量，`DesktopExtended/512` 中的高槽目标不再被 `0..399` 截断 |
| transition effect 容量计数 | **已实施 / 已验证** | `LF2Entity` transition effect 的可用动态槽计数使用当前 world 的 dynamic 起点到逻辑容量尾部，不再固定扫描 `50..399` |
| parity capture guard | **已实施 / 已验证** | 历史 parity capture 必须同时满足 Profile 为 `Authority400` 且逻辑容量为 400；`DesktopExtended/512` 与 `DesktopExtended/400` 均明确拒绝，不能仅凭容量为 400 冒充 authority certificate |
| 生产接线 | **R2C-3B 时未实施；已由 R2C-4 后续接入** | 默认生产 Driver 的 Profile、admission 与桌面自动增长后续已接入；本批仍未实现扩展 parity schema |

R2C-3B fresh 验证：相关源码时间 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` 结果 `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**，warnings 为既有告警。该证据关闭 3A 后遗留的 special attack / transition effect 固定容量边界，并建立严格的 authority parity capture guard；不代表生产 Driver/Profile 接线、admission、桌面自动增长、Loose Quadtree、VRest 或集中式渲染已完成。

### 2026-07-20 R2C-4 生产 Profile 激活记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 生产 Profile 解析优先级 | **已实施 / 已验证** | 命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > Unity 平台宏默认；配置值不再被 `Awake`/重建路径静默覆盖 |
| 默认容量 | **已实施 / 已验证** | `Authority400` 逻辑容量 `400`；`MobileExtended` 逻辑容量 `1050`，`TOTAL active admission = 1000`（跨 roster/stage/dynamic 全部槽区）；`DesktopExtended` 默认初始逻辑容量 `512`，按 `PageSize=256` 规范化并支持自动增长 |
| Driver 生命周期 | **已实施 / 已验证** | `SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析与 world 创建路径；直接 `BattleTestBootstrap` 在实体注册前重新协调晚到的 GameConfig |
| Desktop 增长 | **已实施 / 已验证** | 自动增长保留最低空洞分配顺序，并同步扩展 AI snapshot 容量，避免 world 与 AI 视图分叉 |
| Extended checksum/parity | **历史边界，已由 B2C 替代** | 当时 Extended Driver checksum 输出跳过/为空；当前 B2C 已提供独立 Extended checksum，direct parity capture 仍只接受 `Authority400 + 400` |
| 后续阶段 | **R2C-4 历史边界，后续已替代** | B0 shadow 随后落地；B1-B2B 后续完成 VRest 解耦、增量更新与 formal backend，B2C 已实施即时 weapon/body、AI 查询和 Extended checksum。集中式渲染仍是后续计划，默认 broadphase 仍为 `BruteForce` |

R2C-4 fresh 验证：相关源码时间 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` 结果 `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors / 42 existing warnings**；architect final review **PASS**。

### 2026-07-20 B0 shadow Loose Quadtree 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 纯数据空间树 | **已实施 / 已验证** | X/Z half-open 归属；`looseness = 1.5`、`leafCapacity = 16`、`maxDepth = 8`；不依赖 Transform 或 Unity Physics |
| 构建策略 | **shadow 已实施 / 正式切换未实施** | 每次 collision collect 全量重建；尚未采用增量更新，也未替换正式 brute-force broadphase |
| 诊断边界 | **已实施 / 默认关闭** | 对比 brute AABB pair、tree pair 与正式 accepted subset；诊断关闭时不承担生产结果责任，不据此宣称性能提升 |
| 权威流程保护 | **保持不变 / 已验证** | 正式 `i/j` 遍历、VRest、RNG、candidate 收集/截断/消费顺序继续使用原权威流，shadow 结果不写回战斗真值 |
| 后续接入 | **B0 历史边界，后续已替代** | 即时 weapon/body 与 AI 查询已由 B2C 接入；VRest 解耦、增量更新与 formal broadphase 已由 B1-B2B 接入。生产默认仍为 `BruteForce` |

B0 fresh 验证：相关源码时间不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` 结果 `16:15:43` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。这些证据只证明 shadow 数据结构、pair 诊断和权威流隔离正确，不证明生产 broadphase 已切换或已有性能收益。

### 2026-07-20 B1 `RuntimeRestStore` 基础记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| ARest 存储 | **纯数据基础已实施 / 已验证** | 分页、惰性物化；逻辑容量外地址拒绝，不因只读访问隐式创建页 |
| VRest 存储 | **纯数据基础已实施 / 已验证** | 定向稀疏 `VRest[victim, attacker]`；只保存正值，写零即移除，不把双向 pair 合并 |
| 槽位清理 | **已实施 / 已验证** | `ResetSlot(slot)` 同时清该槽 ARest、VRest victim row 与 attacker column，防止槽复用继承旧 rest |
| 生命周期与扩容 | **已实施 / 已验证** | 支持 `GrowTo`、全局 reset、排序后的 diagnostics/snapshot，以及 snapshot restore；增长保持既有稀疏状态 |
| 差分验证 | **已验证** | 2,000 次随机操作与 dense reference model 逐步 differential，对定向读写、清零移除、slot reset、grow/reset 与 snapshot restore 进行比较 |
| 生产接线 | **B1 时未实施；已由 B1.2 后续接入** | facade lifecycle 与 parity fallback 已由 B1.2 接入；collision pair tick 解耦与正式 quadtree switch 仍 pending |

B1 fresh 验证：相关源码时间 `2026-07-20 16:31:32` < Unity `Assembly-CSharp.dll` `16:36:38` < 完整 `BattleRuntimeSelfCheck` 结果 `16:37:13` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` 为 **0 errors**；architect final review **PASS**。这些证据只验证纯数据 store 契约，不代表生产 VRest/ARest owner 已迁移，也不代表 pair tick 已与 collision broadphase 解耦。

### 2026-07-20 B1.1 optional facade 与 victim-row lease 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| optional facade | **已实施 / 已验证 / 未 production-bound** | `LF2ItrRestTracker` 可选择绑定 `RuntimeRestStore`，未绑定时保留既有实现；当前生产 world 尚未启用该绑定 |
| victim-row ownership | **已实施 / 已验证** | facade 获取 exclusive victim-row lease；同一 victim row 不允许多个 facade 并发拥有，释放 lease 后才允许后续 owner 接管 |
| 语义边界 | **保持不变** | facade 只适配现有 ARest/VRest 定向语义，不改变 store 的 positive-only、zero-removal、row/column reset 或排序 snapshot 契约 |
| state import 原子性 | **已修复 / 已验证** | architect 首轮发现 `ReplaceVictimState` 在 mixed-invalid attacker 输入下可能先写入部分合法项再失败；现已先完整预验证，之后原子替换，失败时原状态不变 |
| failed-import 回归 | **已验证** | direct `ReplaceVictimState` 与 facade `Bind` 两条路径均覆盖 mixed-invalid 输入，并断言失败前后的 ARest/VRest 状态完全一致 |
| 非阻塞补强 | **可后续补充** | invalid bound `RestoreState` 的单独断言尚可增加；该路径复用已验证的 atomic replace 入口，不构成当前 blocker |
| 下一批生产接线 | **B1.1 时未实施；已由 B1.2 后续接入** | registration、release、world reset 已按 ordinary 清理与 `StageSpawnAt` retention 分流接入 |

B1.1 修正后 fresh 验证：复跑 `dotnet build Assembly-CSharp.csproj` 为 **0 errors / 18 existing warnings**；相关源码时间 `2026-07-20 17:34:22` < Unity `Assembly-CSharp.dll` `17:36:49` < 完整 `BattleRuntimeSelfCheck` 结果 `17:39:07` **PASS**；architect final review **PASS / no blocker**。该批证据本身不代表 production-bound；后续绑定由 B1.2 单独实现和验证。

### 2026-07-20 B1.2 production lifecycle binding 记录（已验证 / architect final PASS）

| 项目 | 当前状态 | 证据 |
|---|---|---|
| store ownership | **已实施 / self-check verified** | `SimulationWorld` 独占 `RuntimeRestStore`，store 生命周期随 world 创建、reset 与 grow 同步 |
| ordinary claim | **已实施 / self-check verified** | claim 成功后先 `ResetSlot(slot)`，再以 `Bind(..., importLegacyState: false)` 绑定 tracker |
| release | **第三个 blocker 已修 / self-check verified** | `ReleaseRuntimeSlot` 返回 bool 并事务传播到全部注销/待销毁调用链；错槽拒绝时不继续半注销，正常 release 保留 store 并解绑 |
| `StageSpawnAt` | **blocker 已修 / self-check verified** | rejected bind 走共享完整 pool 回收；真实 pool counts、lease、slot 与 `KillStats` 均有回归断言 |
| public `Unregister` 故障回归 | **已验证** | 通过公开 `Unregister` 触发错槽 release 拒绝，断言完整 registration context（bucket/slot/lease/store/entity）保持不变 |
| 单一 rest 真值 | **已实施 / self-check verified** | 删除 `RuntimeSlotTable.RawRest`；parity fallback 直接读取 `RuntimeRestStore` |
| world reset/grow | **已实施 / self-check verified** | world reset/grow 与 store 同步 |
| 尚未关闭 | **未实施 / 未验证** | collision pair tick 解耦仍未实施；本批不切换正式 broadphase，且与 T8 无关 |

B1.2 初版证据：`dotnet build` **0 errors**；源码 `2026-07-20 18:11:41` < Unity DLL `18:12:23` < full self-check `18:13:00` **PASS**。architect final review 随后发现上述 2 个 blocker；该证据现只说明初版可编译且旧断言通过，**不构成 B1.2 完成/验证证据**。

B1.2 第一轮 blocker 修复证据：`dotnet build` **0 errors**；源码 `18:21:20` < Unity DLL `18:21:58` < self-check `18:22:59` **PASS**。architect 第二轮随后发现 release 拒绝未向 `Unregister` 调用链传播、可能半注销；因此该 PASS 同样是**非完成证据**。

B1.2 最终 fresh 证据：`dotnet build` **0 errors**；相关源码 `2026-07-20 18:31:25` < Unity DLL `18:33:58` < full self-check `18:34:54` **PASS**。公开 `Unregister` 故障矩阵验证完整注册上下文不变；architect final review **PASS / no blocker**。

### 2026-07-20 B1.3 collision pair VRest tick 解耦记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| pass 顺序 | **已实施 / self-check verified** | 单 tick 固定为 `CaptureSnapshots -> sparse Tick -> Collect`；VRest 递减在候选收集前独立完成 |
| eligible row | **blocker 已修 / self-check verified** | 直接遍历 registered bucket items，筛选 `active + CharData` victim；inactive row 冻结，不扫描 `RuntimeSlotCapacity` |
| pair 内副作用 | **已移除 / self-check verified** | `BruteForceSceneQuery` 不再在 pair 枚举内部 tick VRest；early return、无 pair 与候选截断都不能漏 tick 或重复 tick |
| store 热路径 | **已实施 / self-check verified** | `RuntimeRestStore` 维护 active-positive-row/stamp，scratch 随容量预扩；eligibility 无 capacity scan、无 snapshot 分配 |
| Desktop 稀疏高槽 | **已验证** | 高逻辑容量 world 仅两个 registered eligible items 时访问计数严格为 `visited=2` |
| 验证矩阵 | **已覆盖** | dense differential、registration/release lifecycle、inactive freeze、early-return/no-pair、diagnostics 与 parity fallback 均进入 full self-check |
| broadphase | **未切换** | 正式候选仍由原 brute-force collect 产生；B1.3 不代表 Loose Quadtree 已接管生产 broadphase |

B1.3 初版证据：`dotnet build` **0 errors**；源码 `19:09:44` < DLL `19:10:34` < self-check `19:11:13` **PASS**。architect 随后发现 eligibility 仍为 O(`RuntimeSlotCapacity`) 全扫，该证据因此是**非完成证据**。

B1.3 最终 fresh 证据：`dotnet build` **0 errors**；相关源码 `2026-07-20 19:19:14` < Unity DLL `19:19:47` < full self-check `19:22:50` **PASS**；Desktop sparse high-slot `visited=2`；architect final review **PASS / no blocker**。

### 2026-07-20 B2A formal Loose Quadtree broadphase 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 后端选择 | **已实施 / self-check verified** | 独立 `CollisionBroadphaseBackend` 支持 `BruteForce` 与 `LooseQuadtree`；解析优先级为命令行 `-ntsdCollisionBroadphase` > `GameConfig.BattleCollisionBroadphaseName` > 默认 `BruteForce`，平台宏不进入战斗分支 |
| 接管边界 | **B2A 历史边界，已由 B2C 部分替代** | B2A 仅替换 fixed-tick `CollectCollisionCandidates`；B2C 随后接入即时 weapon/body current-world query，失败仍走 brute fallback |
| participant/pair 顺序 | **已实施 / self-check verified** | 收集与 brute outer loop 相同的 eligible participant 并保留 authority ordinal；tree/fallback pair 使用 `(minSlot,maxSlot)` canonical key 全局排序去重，随后按 authority ordinal 以 `a->b`、`b->a` 顺序派发 |
| 无效 AABB | **保守处理 / self-check verified** | 缺失或无效 AABB 的 participant 不被遗漏，而是与全部其他 eligible participant 组成 fallback-all pair；extra formal pair 仍由 narrow phase 过滤 |
| 整 tick 回退 | **已实施 / self-check verified** | runtime slot 缺失/重复/越界、slot-to-entity mapping 不一致、query index/entry count 非法、rebuild/query 异常，或 diagnostics 发现缺少 brute coverage 时，丢弃 formal 部分结果并整 tick 重跑原 brute-force |
| 原子性与确定性 | **已实施 / self-check verified** | formal 失败时恢复进入前 RNG state/call count，清空 candidate carrier/count/distance/cache 后再 brute collect；candidate 20 上限、nearest/type ties、RNG 与消费顺序保持原权威路径 |
| diagnostics | **默认关闭 / self-check verified** | 开启时比较 brute canonical set 与 formal set；缺 pair 强制整 tick brute fallback，extra pair 允许并交 narrow phase；诊断不改变 RNG 或战斗状态 |
| 后续阶段 | **B2A 时未实施；已由 B2B 后续接入** | B2A 当时仍为每 fixed tick full rebuild；generation-aware 增量迁移/更新现已由下节 B2B 接入，生产默认仍未切为 Loose Quadtree |

B2A fresh 证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` **0 errors**；相关源码最新时间 `2026-07-20 22:15:07` < Unity `Assembly-CSharp.dll` `22:18:48` < full `BattleRuntimeSelfCheck` 结果 `22:19:28` **PASS**。architect final review **PASS / no blocker**；本批未执行 Play Mode，不能据此扩大为完整场景验收。T8 默认 `stage.dat` 部署继续暂缓。

### 2026-07-20 B2B generation-aware 增量 Loose Quadtree 记录

| 项目 | 当前状态 | 证据 |
|---|---|---|
| 同步边界 | **已实施 / self-check verified** | formal backend 在每次 fixed-tick collision collect 边界批量同步当帧 eligible participant；注册、注销和移动本身不直接改树，避免把 registry mutation 时序引入权威 pass |
| 稳定身份 | **已实施 / self-check verified** | 索引记录与查询结果使用 `(runtime slot, generation)` 的 `RuntimeEntityHandle`；同槽释放再复用时旧 generation 被移除，新 occupant 作为新 handle 插入，不会把旧空间记录解析到新实体 |
| 增量更新 | **已实施 / self-check verified** | 未移动实体保持原记录；AABB 改变但仍在当前节点 loose 容纳范围内时原位更新；越出 loose 范围时才从旧节点移除并重新插入。新增、销毁、invalid-AABB 转换和同槽复用均由同一 batch sync 收口 |
| root escape | **保守重建 / self-check verified** | 当前有效 AABB 超出既有 root 时执行一次全量 rebuild；正常的 loose 内移动与跨 loose 迁移不重建整棵树 |
| live query validation | **已实施 / self-check verified** | quadtree query 返回 handle，派发前必须由当前 `RuntimeSlotTable` generation 成功解析，并再次核对 slot、entity、participant ordinal 与 handle 映射 |
| 原子回退 | **已实施 / self-check verified** | sync/query/invariant/mapping 异常会 reset 增量索引并整 tick 重跑 brute-force；B2A 已有 RNG/candidate rollback 继续包住 formal collect，部分执行不能污染候选、RNG 或消费顺序 |
| world reset | **已实施 / self-check verified** | `SimulationWorld` registry reset 显式清理 formal spatial index，旧 match 的 node、record 与 handle 不会进入下一 world 生命周期 |
| 启用边界 | **B2B 历史边界，已由 B2C 部分替代** | 生产默认仍为 `BruteForce`；只有显式选择 `LooseQuadtree` 才使用 formal backend。B2C 已接入即时 weapon/body 与 AI 查询及 Extended checksum；集中式渲染仍不属于 B2B/B2C |

B2B fresh 证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` **0 errors**；相关源码最新时间 `2026-07-20 22:43:57` < Unity `Assembly-CSharp.dll` `22:46:36` < full `BattleRuntimeSelfCheck` 结果 `22:47:04` **PASS**。architect final review **PASS / no blocker**；本批未执行 Play Mode，不能据此扩大为完整场景验收。T8 默认 `stage.dat` 部署继续暂缓。

## Runtime 容量与空间索引阶段决策

**状态：B1-B1.3、B2A 与 B2B 已完成代码层实施 / 编译 / full self-check / architect final review。** B2C 已实现 Extended checksum、AI Loose Quadtree 查询和即时 weapon/body current-world query，并有 `2026-07-21 00:48:06` full self-check PASS；B2C 本身尚无 fresh Architect PASS、Play Mode 或性能验收。生产默认 broadphase 仍是 `BruteForce`。

### RuntimeSlot 容量模式

- **`Authority400` 兼容模式**：保留 C# 的 400 runtime slot、既有特殊槽区和最低空闲槽分配语义，用于现有 self-check、parity 和逐帧对照。该模式的 400 是兼容边界，不代表 render command 上限。
- **移动端扩展模式**：逻辑地址容量为 `1050`，最后有效地址为 `1049`；`TOTAL active admission = 1000`，跨 roster/stage/dynamic 全部槽区计数，第 `1001` 个 active entity 必须确定性拒绝生成，不排队、不替换，也不由设备瞬时内存状态决定。拒绝结果必须进入可重放的结果/日志边界。
- **桌面扩展模式**：默认初始逻辑容量 `512`，按 `PageSize=256` 规范化为整页并在需要时自动增长；不设置玩法层面的 active entity 上限，但仍受明确的地址空间、内存、对象池、逻辑帧和 render command 技术预算约束，不能解释为物理上无限容量。
- 空闲槽使用**二叉最小堆 + `nextUnused`**：R1 第一批已在 `Authority400` 内按 `0..19`、`20..49`、`50..399` 三段实现 indexed binary min-heap；已释放槽进入最小堆，分配时优先取最小空闲槽，堆为空时使用并递增 `nextUnused`。R2A 以 256 槽/页建立惰性分页表并复用该 allocator，R2B-R2C-3B 依次接入槽表、增长、实例容量和外部边界，R2C-4 已将 Desktop 自动增长接入生产。增长前的最低空洞仍优先于新页地址，且 AI snapshot 与 world 容量同步扩展；所有分配、释放和分页增长继续保持最低槽确定性，不依赖 `Dictionary`/`HashSet` 枚举顺序。
- **分层位图**仅作为后续候选优化，不作为本阶段实现前提；若采用，必须保持与最小堆相同的最低槽和回放语义。

### 平台 Profile 与选择边界

**状态：resolver 与生产 Profile 激活已实施并通过 self-check / architect final PASS。** 平台差异通过统一 Profile/能力配置入口表达；不得在战斗 pass、opoint、碰撞、命中、对象生命周期或空间查询内部散布 `#if UNITY_ANDROID` / `#if UNITY_STANDALONE` 分支。Unity 官方条件编译符号仅用于选择平台默认值；`SystemInfo` 等运行时能力 API 留给后续渲染后端降级，不改变战斗 Profile 或逻辑结果。

运行模式固定为：

| Profile | 平台默认与用途 | RuntimeSlot / active 边界 |
|---|---|---|
| `Authority400` | `UNITY_EDITOR` 和未明确支持的平台默认；用于 C# 权威对拍、现有 self-check、历史 parity schema 与兼容诊断 | 固定 400 槽，保留权威特殊槽区和最低空闲槽语义 |
| `MobileExtended` | `UNITY_ANDROID && !UNITY_EDITOR` Player 默认 | 逻辑容量 1050；全部槽区合计最多 1000 active，第 1001 个发布尝试确定性拒绝 |
| `DesktopExtended` | `UNITY_STANDALONE && !UNITY_EDITOR` Player 默认 | 默认初始 512，按 256-slot 页规范化并自动增长；不设玩法层面的 active 上限，但受明确技术预算约束 |

宏边界必须按以下规则实现：

- `UNITY_EDITOR` 优先于当前 Build Target 宏。Editor 即使切到 Android Build Target，也不能仅因同时定义 `UNITY_ANDROID` 就自动进入移动端正式 Profile；Editor 平台默认保持 `Authority400`，测试或配置可显式覆盖为 `MobileExtended` / `DesktopExtended`。
- `UNITY_ANDROID && !UNITY_EDITOR` 只负责给 Android Player 选择 `MobileExtended` 默认值；`UNITY_STANDALONE && !UNITY_EDITOR` 只负责给桌面 Player 选择 `DesktopExtended` 默认值。
- 其他 Player 平台在完成单独设计和验收前默认 `Authority400`，不得根据相似平台经验自动套用 Android 或桌面扩展规则。
- 平台宏只允许出现在默认 Profile 选择和不可避免的平台专属 API 适配入口。核心 runtime 统一读取已解析的 Profile/预算，不直接读取平台宏。

配置解析优先级固定为：

```text
命令行显式覆盖
    > GameConfig.BattleRuntimeProfileName
    > 平台宏默认 Profile
```

- 命令行显式覆盖用于 self-check、parity、回放和 Editor A/B 验证，必须能强制选择 `Authority400`、`MobileExtended` 或 `DesktopExtended`。
- `GameConfig.BattleRuntimeProfileName` 是生产项目配置入口；`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用同一解析路径，直接 `BattleTestBootstrap` 在实体注册前协调晚到配置。
- 运行时设备能力检测发生在 Profile 解析之后。`SystemInfo.supports2DArrayTextures`、纹理尺寸/slice 上限、图形 API、格式支持和目标 GPU 验证结果只用于选择可用的资源与渲染后端。
- 推荐降级链为 `Texture2DArray + OrderedChunks` -> `多 Texture2D + OrderedChunks` -> `LegacySpriteBackend`；任何降级都必须保持原 painter 顺序和相同只读表现输入。
- 设备不支持 `Texture2DArray`、命中设备黑名单或内存预算不足时，不得把 `MobileExtended` 静默改成 `Authority400`，也不得降低 1000 active admission 边界来掩盖渲染预算不足；应通过分 chunk、后端降级、可诊断拒绝或明确启动失败处理。

所有 Profile 必须共用同一份二叉最小堆 + `nextUnused`、分页 slot、generation handle、Loose Quadtree、VRest/ARest、候选排序和 lifecycle 实现。平台可以改变容量、预分配、图集格式、chunk 数和渲染回退策略，但不能改变逻辑 tick、slot 决定性、pair 顺序、VRest 计时、opoint 生成顺序或战斗结果。

### 移动端 1000 active admission 边界

- `1000 active` 与 slot address 容量是两个独立数字：`RuntimeSlotTable.LogicalCapacity = 1050`，最后有效地址是 `1049`，其中 `0..19` 为 roster、`20..49` 为 stage、`50..1049` 为 dynamic 地址。active admission 的 1000 是**全部槽区合计预算**，不是只给 dynamic band 的 1000 个 active 名额；5 个 256-slot 物理页仅是存储实现，尾部 `1050..1279` 不属于逻辑地址空间。
- active 计数以**已发布且尚未完成注销的 runtime entity**为准：已注册的 active、dormant/merge shell 和 `pending-destroy` entity 都计入；尚未发布的 `pending-spawn`、未占用的 raw slot 以及已归还对象池且没有 runtime 注册的 shell 不计入。
- `pending-destroy` 在确定性注销边界完成前仍占用 active 预算和 runtime slot；不能因为已经标记销毁就提前释放容量。分配拒绝必须在发布前判断，不能先发布再回滚。
- 同一 tick 的释放与生成不依赖容器枚举顺序：在既定的 lifecycle mutation boundary 内，先按队列/slot 的确定顺序完成已到期注销，再按既定 producer/pass 顺序逐个进行 spawn admission 和发布；只有前一步已完成注销的 entity 才能为后一步释放容量。若生成发生在注销 boundary 之前，则按当时仍包含 `pending-destroy` 的计数判定并可确定性拒绝。
- 每次 spawn admission 成功后立即增加已发布计数；同一 boundary 后续 spawn 看到更新后的计数。移动端达到 1000 后，后续第 1001 个发布尝试稳定返回拒绝结果；Extended replay/checksum schema 尚未实现，当前 Extended Driver checksum 明确跳过/返回空值。

### X/Z Loose Quadtree Broadphase

**当前状态：B0 shadow 诊断、B2A formal backend 与 B2B generation-aware 增量同步均已实施，并通过 full self-check 与 architect final review；生产默认仍为 `BruteForce`。** `LooseQuadtree` 只有经显式命令行或 `GameConfig` 选择时才接管 formal backend；B2C 已随后接入即时 weapon/body query 与 AI 查询，此处旧“未迁移”结论已替代。

- 空间索引使用 X/Z 平面的 **Loose Quadtree**；逻辑实体、AI 范围查询和 itr/bdy 碰撞查询共享空间索引，但查询服务与候选规则分开，不能用 AI 范围结果替代碰撞候选。
- 实体中心点采用严格的**半开区间**归属（左/下含、右/上不含，边界规则全局一致），保证一个中心点只属于一个子节点。
- 实体 AABB 只有在完全被节点的 loose 范围容纳时才留在该节点；超出 loose 范围才迁移到父节点或重新选择的节点。
- 默认参数仅作为 profiling 基准，不能视为最终性能结论：`looseness = 1.5`、`leafCapacity = 16`、`maxDepth = 6..8`。目标设备和真实战斗分布 profiling 后再调整。
- 更新已采用 collect-boundary batch 增量策略：未移动实体保留原记录；AABB 改变但仍处于当前节点 loose 范围时原位更新；离开 loose 范围才迁移。生成、销毁、invalid AABB 和同槽 generation 复用在下一次 collision collect 同步，root escape 才触发全量重建；world reset 显式清空索引。
- broadphase 每 tick 先按 `RuntimeSlot` 升序遍历 active attacker；各 attacker 查询得到的候选先去重为 `(minSlot, maxSlot)` pair，再在全局按 `(minSlot, maxSlot)` 升序排序后交给现有 narrow phase。保留 C# 的 candidate 截断、距离/类型 tie 顺序和 pair 消费规则；空间索引不得改变命中规则、VRest 计时或最终逻辑结果。

### VRest 与 Parity 边界

**当前状态：B1.2 production lifecycle 与 B1.3 sparse tick 已验证；“Extended parity schema 未实施”为 B1.3 历史状态，已由 B2C 独立 Extended checksum 替代。** VRest tick 已移至独立 pass，eligibility 直接遍历 registered bucket items。

- VRest/ARest 的逻辑访问与 broadphase 解耦。空间索引减少候选枚举，不负责 VRest 的递减或过期；VRest 计时必须遍历自己的稀疏活动集合/到期结构，不能因 broadphase 未返回远距离 pair 而停止递减。
- 详细 parity snapshot（完整 slot、ARest/VRest、哈希和诊断字段）退出生产热路径，只在 `Authority400` 对拍、自检、回放或显式诊断模式中生成；生产 tick 不为 parity 预先扫描整页/全容量数据。
- Extended Driver 当前不生成 authority checksum，输出跳过/为空；direct parity capture 继续严格要求 `Authority400` Profile 且容量 400。Extended replay/checksum schema 必须另行设计，不能复用或伪装成旧 400-slot certificate。

## 1. 目标

建立只消费战斗逻辑快照的集中式表现后端，在不改变战斗结果的前提下，逐步替换战斗对象各自持有的 `Sprite` / `SpriteRenderer`：

- Loading 阶段完成 BMP 解码、依赖收集、图集规划和 GPU 资源创建，减少战斗中的资源创建与上传尖峰。
- 使用 source rect / UV 直接绘制，不再为每个图片格创建 `Sprite`。
- 将角色、武器、特殊攻击、其他对象、阴影和火花组织成一条确定顺序的 render command 流。
- 复用持久化 Mesh、顶点缓冲和 Material，避免逐帧 GameObject、Mesh、Sprite、Material 和临时容器分配。
- 通过多页图集和 `Texture2DArray` 减少透明绘制序列中的纹理切换与断批。
- 消除把 `logicalZ * 4096 + runtimeSlot * 4` 塞入 Unity `sortingOrder` 所产生的范围限制。
- 保留旧渲染后端作为迁移期回退，允许逐类切换和结果比对。

## 2. 非目标与边界

- 不改变 30 Hz 战斗逻辑 tick、pass 顺序、碰撞、输入、对象生成、命中结算或实体生命周期。
- 不以 `Transform`、插值位置、Renderer 状态或 GPU 结果反写战斗 runtime。
- 不把渲染帧变成战斗计数来源；参与规则的表现计数仍随逻辑 tick 推进。
- 不在本方案中实现完整联机、回滚、HUD、主菜单或通用场景渲染重构。
- 不以“每角色固定独占一张 2048 图集”作为最终物理布局；角色可以作为依赖收集根，但本局资源应统一装箱以避免空页和跨角色纹理切换。
- 不在本方案中处理或恢复 T8 默认 `stage.dat` 部署；T8 与本渲染方案无关，原暂缓状态不变。
- 第一阶段不承诺单次 draw call；透明正确性优先于极端合批。

## 3. 总体数据流

```text
Loading
data.txt / DAT / BMP
    -> BattleRenderDependencyCollector
    -> BattleAtlasLayoutPlanner
    -> BattleAtlasLoader
    -> Texture2DArray + BattleSpriteCatalog

Runtime（逻辑 tick）
只读 runtime 状态
    -> BattlePresentationSnapshot
    -> BattleRenderCommandBuilder
    -> 权威实体排序 + 实体内命令顺序
    -> BattleDynamicMeshBackend

Render（Unity 渲染帧）
最新完成的 Mesh / command segments
    -> BattleRenderFeature / BattleRenderPass
    -> 背景之后、后处理/UI 之前的目标注入点
```

资源准备、逻辑快照、绘制命令和 Unity 提交必须是明确边界。Loading 只准备表现资源；runtime 只提供只读真值；渲染后端不能成为战斗逻辑 owner。

## 4. 模块划分

| 模块 | 职责 |
|---|---|
| `BattleRenderDependencyCollector` | 从当前对局入口递归收集 DAT/BMP 表现依赖，按规范化路径去重 |
| `BattleAtlasLayoutPlanner` | 统计尺寸，使用确定性装箱算法生成 2048 多页布局 |
| `BattleAtlasLoader` | 解码 BMP，填充 `Texture2DArray`，上传 GPU，并在允许时释放 CPU 可读副本 |
| `BattleSpriteCatalog` | 将视觉对象和有效 pic 映射为 slice、UV、像素尺寸、pivot/中心等表现元数据 |
| `BattlePresentationSnapshot` | 在逻辑 tick 边界捕获渲染所需的只读字段 |
| `BattleRenderCommandBuilder` | 将快照展开为阴影、本体、覆盖物和命中记录等有序命令 |
| `BattleDynamicMeshBackend` | 复用 Mesh/缓冲，将命令写成 quad 顶点并形成连续渲染状态段 |
| `BattleRenderFeature` / `BattleRenderPass` | 在 URP 指定注入点提交有序 Mesh 段 |
| `LegacySpriteBackend` | 迁移期继续使用现有 `SpriteRenderer`，支持回退和 A/B 比对 |

名称只是当前建议，实施时应跟随仓库已有命名和目录边界。

## 5. Loading 依赖闭包

### 5.1 收集入口

`data.txt` 中 `type == 0` 可作为可玩角色 DAT 的资源收集根，但不能当作最终图集边界。一个角色可能通过 opoint、转换、分身、武器、技能体或 stage 生成引用 `type != 0` 的对象；公共阴影、火花、烟雾也可能位于角色 DAT 之外。

当前拟定收集流程：

1. 从本局角色和场景明确入口开始。
2. 读取每个 DAT 的 `LF2CharacterData.files`，收集其全部 BMP。
3. 递归追踪当前对局可达的 opoint、转换对象、武器、特殊攻击和固定表现资源。
4. 按规范化资源路径去重 BMP，而不是按 oid 或 DAT 去重。
5. 对无法静态闭合的动态引用建立明确的预加载清单或受控后备页，不允许在战斗热路径无界创建图集。

依赖闭包的准确规则在实施前仍需结合当前 Unity loader 与 C# 可达对象生成调用链逐项核对。

### 5.2 2048 多页图集

- Loading 阶段先统计本局全部去重 BMP 的尺寸，再运行确定性 MaxRects、Skyline 或等价装箱算法。
- 图集页固定为 `2048 x 2048`；超出一页时增加第二页及后续页面。
- 第一版优先装入完整 BMP sheet，保留 sheet 内格子布局，降低裁剪契约迁移风险。
- 所有同尺寸、同格式页面放入一个 `Texture2DArray`；顶点携带 `atlasSlice`，Shader 以 slice 选择页面。
- `Texture2DArray.depth` 创建后不能无损原地扩展，因此页数应在 Loading 规划结束后确定。
- BMP 大于页面、设备 slice 上限不足、格式不兼容或依赖漏收时必须产生可诊断失败或进入明确 fallback，不能静默显示错误图片。
- 设备不支持 Texture Array 时，回退为多个 `Texture2D`，但仍按原 painter 顺序生成连续纹理段，不按纹理重排对象。

RGBA32 的单张 2048 页面约占 16 MiB GPU 内存；若保持 readable，通常还会保留 CPU 副本。最终应根据目标 Android 格式、mipmap 策略、页数和设备上限制定预算，并在上传完成后按需调用 `Apply(false, true)` 释放 CPU 可读副本。

## 6. 图片索引与格子契约

图片查询使用 frame 的图片编号，不使用动作帧 ID：

```text
effectivePic = LF2FrameData.pic + Runtime.RenderPicOffset
```

然后在 `LF2CharacterData.files` 中找到包含 `effectivePic` 的文件区间：

```text
file.startFrame <= effectivePic <= file.endFrame
localPic = effectivePic - file.startFrame
```

格子按 DAT 现有契约换算：

```text
column     = localPic % columns
rowFromTop = localPic / columns
```

必须在实现前锁定并自动验证以下约束：

- `LF2FrameData.frameId` 是动作状态帧编号，不是图片格子索引。
- `LF2FrameData.pic` 才是图片编号；多个 frame 可以复用同一 pic。
- `RenderPicOffset` 参与最终显示图片查询。
- `pic == 999` 及其他现有无图语义不提交本体命令。
- 当前 DAT 的 `row` / `col` 命名与横纵格数的实际含义必须沿用现有 parser/loader 契约，不能按英文名猜测。
- 格子步长保留当前 sheet 的间隔像素：横纵方向按 `(w + 1, h + 1)` 推进，而不是只用 `(w, h)`。
- BMP 左上角编号与 Unity UV 原点方向不同；Catalog 负责一次性换算，runtime 不重复做易错的 Y 翻转。
- Catalog 同时保存像素宽高、中心/pivot 和必要裁剪元数据，使碰撞/逻辑尺寸不依赖运行时 `Sprite.rect`。

建议 Catalog 的稳定查询键为 `(visualDataId, effectivePic)` 或能唯一定位 DAT file range 的等价结构，结果至少包含 `atlasSlice`、`uvRect`、像素尺寸和 pivot。

## 7. PresentationSnapshot

`BattlePresentationSnapshot` 在逻辑 tick 完成后的稳定边界读取 runtime，只包含表现需要的数据，不持有可变 runtime 引用。候选字段包括：

```text
RuntimeSlot / StableId / Oid
ZInt / XInt / YInt / 表现高度字段
Frame / Pic / RenderPicOffset
Facing / Visible / Alpha / Tint
Shadow 与 overlay/hit-record 所需表现参数
```

最终字段必须从当前实际消费者倒推，不能把整个实体复制进快照。快照生成和消费需要避免逐 tick GC；使用双缓冲或环形缓冲，让 Unity 渲染帧只读取最后一个完整快照。渲染插值只能作用于表现坐标，不改变排序 key，不写回 runtime。

## 8. RenderCommand 与权威顺序

单条 `BattleRenderCommand` 的候选结构：

```text
CommandType
AtlasSlice / UVRect
Position / Size / Pivot
FlipX
Color / Alpha
BlendMode / MaterialVariant
RuntimeSlot / StableId / ZInt
```

全局实体顺序必须沿用 C# 权威可观察绘制顺序：

```text
Runtime.ZInt 升序
相同 ZInt 时 Runtime.SlotIndex 升序
```

对排序后的每个实体，命令按实体内顺序连续追加：

```text
Shadow -> Entity -> Overlay -> HitRecord
```

不得先画全体阴影、再画全体角色；也不得为凑图集或材质批次而跨实体重排透明命令。`YInt`、`displayZ`、`Zz`、shake 和类型专项视觉偏移只能影响顶点位置，不能替换 `(ZInt, RuntimeSlot)` 的全局顺序。

上述“权威”指最终可观察顺序必须与 `J:\QQFile\NTSD2.4\ntsd_release_C#` 对应绘制调用链一致。实施前需重新定位真实调用者、活动 slot 过滤、阴影/本体/覆盖物/命中记录的条件分支，并把证据加入对齐记录；本草案不代替该核验。

## 9. 持久化动态 Quad Mesh

每条可见命令写成一个 quad：4 个顶点、6 个固定索引、2 个三角形。顶点至少包含：

```text
position
uv
color
atlasSlice
```

“持久化”表示以下对象只初始化或扩容时创建，而不是逐帧创建：

- `Mesh`，并调用 `MarkDynamic()`。
- 顶点/索引缓冲和 CPU 侧复用数组或原生容器。
- 固定 quad 索引模板。
- 共享 Material 和 Shader variant。

每个逻辑 tick 或需要重建表现数据时：

1. 将已排序命令顺序写入复用顶点缓冲。
2. 使用 `Mesh.SetVertexBufferData` 或匹配当前 Unity 版本的低分配 API，仅上传活动顶点范围。
3. 更新实际 index count / submesh 或 chunk 范围。
4. 渲染帧重复提交最近完成的数据，不重复推进逻辑计数。

建议以 UInt16 索引限制为边界划分 chunk，例如每 chunk 4096 quad 对应 16384 顶点和 24576 索引；这只是实现候选，不是实体数量上限。命令数可能大于实体数，因为一个实体可以产生阴影、本体和多个附加命令。容量应按命令峰值监测，并在 Loading 预留或按明确策略扩容。

## 10. URP 提交

通过 `ScriptableRendererFeature` / `ScriptableRenderPass` 在战斗相机的确定注入点绘制集中式 Mesh。目标顺序是背景之后、需要参与的世界后处理之前、屏幕 UI 之前；准确 `RenderPassEvent` 需结合当前 URP Renderer 和相机栈验证。

战斗 Mesh 对 Unity 只需要稳定的整体层级。Mesh 内部的对象顺序由 render command 与索引/segment 顺序表达，不再将大范围逻辑 key 编码到 `sortingOrder`。相机裁剪、像素缩放、颜色空间、RenderTexture 和后处理必须在桌面与 Android 目标设备上分别验证。

## 11. 透明绘制与三种模式

默认使用透明混合和 `ZWrite Off`，并按 painter 顺序提交。阴影、烟雾、光效可能含半透明像素，因此不能未经素材和遮挡矩阵验证就统一改为 Alpha Clip 或 `ZWrite On`。

提供三级后端策略：

| 模式 | 说明 | 用途 |
|---|---|---|
| `SingleMesh` | 同一兼容渲染状态尽量由单 Mesh/少量 draw 提交 | 实验性研究候选；当前 P6 resolver 不允许进入生产选择，未来必须先通过目标 GPU 像素验证 |
| `OrderedChunks` | 严格保持命令顺序，只把相邻且状态兼容的命令合并为连续段 | 默认稳妥模式；状态变化时断批 |
| `StrictOrderedDraw` | 以更细粒度 draw 保证问题对象或设备的顺序 | 正确性回退和诊断模式 |

Alpha、Additive、Stencil、不同 Shader 或其他 GPU 状态必须断批；只能在原始命令流中切连续段，不能把不相邻的同材质命令抽出合并。Unity/目标 GPU 是否严格按单 Mesh 索引顺序处理所有透明三角形不能只靠桌面推断，必须在目标 Adreno、Mali 等设备用重叠像素场景验证。若结果不稳定，设备配置自动使用 `OrderedChunks` 或 `StrictOrderedDraw`。

## 12. 双后端迁移

迁移期建议保留以下模式：

```text
LegacyOnly
CentralShadowBuild（集中后端生成但不显示，用于命令/排序比对）
CentralOnly
```

切换顺序：

1. 先独立修复现有 `sortingOrder` 越界，使用活动实体紧凑 rank 或其他短期安全映射；不等待整套渲染重构。
2. 建立不依赖 `Sprite.rect` 的 `SpriteMetricsResolver` / Catalog 数据契约。
3. 建立 `BattleSpriteCatalog`，暂时继续由旧 `SpriteRenderer` 消费。
4. 建立 Snapshot 和 RenderCommand，在 shadow-build 下逐 tick 对比对象数量、图片、位置和顺序。
5. 接入持久动态 Quad Mesh 与 URP Pass，先迁移本体。
6. 依次迁移阴影、持有物、overlay、spark/hit record；每类都有旧后端对照。
7. 接入 2048 多页 `Texture2DArray` 和移动端压缩格式。
8. 完成目标 Android GPU 的正确性、内存和性能验收后，才考虑移除战斗 `SpriteRenderer`。

旧后端与新后端不能同时对同一类别实际出图，避免重复显示；shadow-build 只记录/比较，不提交像素。

## 13. 分阶段计划

| 阶段 | 产物 | 进入下一阶段的门槛 |
|---|---|---|
| P0 契约核验 | C# 绘制调用链、Unity 当前消费者、slot/排序/格子契约清单 | 用户确认知识点和总体设计；证据可定位 |
| P1 排序止血 | 当前后端不越界的紧凑排序映射与 focused check | 编译、自检、重叠对象 Play 验证通过 |
| P2 Catalog | BMP/file/pic 到 metrics/UV 的唯一查询层，旧后端消费 | 全部代表性 DAT 的图片索引矩阵通过 |
| P3 Command shadow-build | Snapshot、命令生成、旧/新顺序对比工具 | 多对象、多 Z、同 Z、生成/回收场景逐 tick 等价 |
| P4 Mesh/URP | 持久 Mesh、Shader、URP Pass、OrderedChunks | 桌面像素基线与 Play 场景通过，无逐帧 GC 回归 |
| P5 Atlas Array | 确定性多页装箱、Texture2DArray、fallback | 图集覆盖、内存预算、设备能力与漏依赖处理通过 |
| P6 移动端验收 | Adreno/Mali 真机结果、模式选择与性能报告 | 正确性矩阵通过，性能/内存达到项目预算 |
| P7 收口 | CentralOnly 默认，旧后端移除条件评审 | 回退期完成且长期场景无差异后单独批准 |

每个阶段都应是可回退、可验证的独立提交；不能以最终架构目标跳过中间的可观察行为对比。

## 14. 验收矩阵

| 维度 | 最低检查 |
|---|---|
| 编译 | Unity 2022.3.4f1c1 脚本编译 0 error |
| 自动自检 | 资源索引、file range 边界、`RenderPicOffset`、`pic=999`、row/col、`w+1/h+1`、排序和容量 focused checks |
| 逻辑隔离 | 启用/禁用新后端时 battle checksum 和 runtime 字段完全不变 |
| 图片正确性 | 每个代表性 DAT 的首格、行尾、下一行、file range 首尾、offset、翻面、pivot 像素对照 |
| 层级正确性 | 不同 Z、同 Z 不同 slot、实体交错阴影、持有物、overlay、hit record 的重叠截图/像素断言 |
| 生命周期 | spawn、回收、复用、变身、分身、武器持有/释放后无旧图、错图或残留命令 |
| 透明状态 | Alpha、Additive、Stencil/特殊 Shader 按原命令流断段且不重排 |
| 容量 | 0 实体、常规负载、峰值命令、超过预留容量、跨 UInt16 chunk 边界 |
| 设备兼容 | Texture Array 支持/不支持、slice 上限、Adreno/Mali 的 array/fallback 像素结果；`SingleMesh` 仅作非生产研究 |
| 性能 | Loading 时间、CPU/GPU 内存、上传峰值、draw call、SetPass、主线程耗时、GC alloc |
| 回退 | `LegacyOnly`、shadow-build、`CentralOnly` 可控切换，故障设备可降级 |

最终报告必须分别标记“方案确认”“逻辑已写”“编译通过”“self-check 通过”“Play Mode 通过”“目标 Android 真机通过”，不得互相代替。

## 15. 主要风险与待确认项

- **依赖漏收**：动态 opoint/转换/stage 引用未进入 Loading 闭包，会导致战斗中缺图。需要权威调用链和生产 DAT 扫描共同闭合。
- **内存预算**：2048 RGBA32 页面约 16 MiB；页面过多、CPU readable 副本和 mipmap 会迅速扩大占用。
- **纹理格式**：运行时拼图与 ASTC/ETC2 构建期压缩的组合方式、颜色空间和 alpha 质量尚待技术验证。
- **透明顺序**：单 Mesh 内透明三角形的实际执行顺序需要目标 GPU 像素验证；不能只以 draw call 数量判定正确。
- **状态断批**：不同 blend/stencil/shader 仍会产生 draw；图集只能消除纹理页切换，不能合并不兼容 GPU 状态。
- **页边缘采样**：线性过滤、mipmap 和 atlas bleeding 需要 padding/extrusion 策略；原 BMP 格子的一像素分隔不能直接等同安全 atlas padding。
- **像素坐标与 pivot**：BMP 顶左编号、Unity UV 原点、翻面和中心点若分散换算，容易出现一像素偏移；应集中到 Catalog 并做边界测试。
- **容量误读**：`400` 必须保留为 `Authority400` 的兼容边界，但不能继续解释为所有 Unity 模式的固定 runtime 槽位上限。slot address 容量、active entity 预算和 render command 数是三个不同概念；移动端 1000 active 或桌面分页增长都不代表同数量的绘制命令。每实体可能展开为阴影、本体、覆盖物和命中记录等多条命令，因此 Mesh 容量与 chunk 边界必须按 render command 峰值独立设计。
- **URP 注入点**：相机栈、后处理、RenderTexture 和 UI 的现状需要实际工程验证。
- **API/平台约束**：正式实现前应查阅 Unity 2022.3 对 `Texture2DArray`、`Mesh.SetVertexBufferData`、URP Renderer Feature/Pass 和移动平台纹理格式的官方文档。
- **迁移双维护**：旧/新后端并存会增加短期复杂度，需要清晰的类别 ownership 和移除门槛。

## 16. 当前决策记录

已确认的设计决策是：保留 `Authority400` 兼容模式；移动端全部槽区合计最多 1000 active 且第 1001 个确定性拒绝；桌面从 512 开始按 256-slot 页自动增长并受技术预算约束；空闲槽使用二叉最小堆 + `nextUnused`；B0 先以 X/Z Loose Quadtree shadow 诊断对比，B2A 提供 formal full-rebuild backend，B2B 再以 `(slot, generation)` 身份在 collision collect 边界实施 batch 增量同步，默认仍为 `BruteForce`；VRest 与 broadphase 解耦；详细 parity snapshot 不进入生产热路径。生产 Profile 优先级为命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认，broadphase 独立遵循命令行 > `GameConfig` > 默认 `BruteForce`；设备能力只降级表现资源/后端，三个 Profile 共用同一套确定性 runtime 算法。

截至 2026-07-20，R1-R2C-4、B0、B1-B1.3、B2A 与 B2B 已完成代码层实施和既定验证。B2B generation-aware incremental backend 的 fresh chain 为 source `22:43:57` < DLL `22:46:36` < result `22:47:04` **PASS**，dotnet **0 errors**，architect final **PASS / no blocker**。该段“即时 weapon/body query、AI 查询、Extended parity schema 仍是后续任务”为 B2B 历史状态，已由 B2C 替代；B2C 最新 full self-check `2026-07-21 00:48:06` **PASS**、dotnet **0 errors / 42 existing warnings**，但未执行 Play Mode、性能或 fresh Architect PASS。生产默认仍为 `BruteForce`，集中式渲染与 T8 默认 `stage.dat` 部署仍暂缓。

## 17. Central Presentation Mount v1（2026-07-22 历史快照，已由文档顶部 rendererless 收口取代）

- **范围和实现状态：**已新增 `BattleCentralPresentationMount` 与 `BattleCentralPresentationMountRegistry`，并由 `LF2ObjectRenderer` 集成。该 v1 只完成 mount 的声明、注册，以及 generation-aware `RuntimeEntityHandle` 绑定；它没有加入渲染、资源加载、`Update`、渲染命令或任何战斗 runtime 改动。
- **生命周期接线：**`SimulationWorld` 在实体注册时登记 mount，在 release 和 reset 时释放/清理登记，避免 slot 复用把旧实体或旧 generation 绑定到新实体。disable -> enable restore 与 rollback clear 均已关闭并纳入 self-check 覆盖；本批新增并通过了针对 world `ResetRuntimeState` 和 registration rollback 的 focused checks。自检同时覆盖 renderer 集成、world register/release/reset，以及 handle generation 失效后的绑定边界。
- **明确未变更项（历史）：**此 v1 批次当时没有编辑 prefab，`Legacy` 表现路径仍保留；它不能单独表述为 CentralOnly 像素接管、资源接管或 Legacy 移除。该限制已由文档顶部的后续 rendererless prefab 接线和 Play Mode 验证取代。
- **最终验证证据：**relevant source `2026-07-22 11:48:18` < Unity `Assembly-CSharp.dll` `11:49:08` < `Temp/NTSD_BattleRuntimeSelfCheck.result` **PASS** `11:50:11`。最终完整命令 `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 完成，结果为 **0 errors / 42 existing warnings**。Architect closure 为 **PASS / no P0-P2**。Console 清空后仍有两类预期的 self-check-active Error：既有 mismatched release，以及新的 registration rollback；这明确不是 Console 0 errors，后续报告不得写成 0 errors。
- **后续历史步骤：**当时计划在 `EntityObject` 的 `EntityModel` 与 `Shadow` nodes 挂载 mount component 并配置 `ownerRenderer`；该步骤已在本轮 rendererless 收口中完成。


--- File: Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md ---
# NTSD C# 工程 vs Unity 工程 — 战斗逻辑差异与对齐清单

## 2026-07-24 P8 v5 最终代码侧验收（覆盖下方 v3/v4 历史结论）

- P8-D v4 的全局 `Texture Memory` counter 在 Central/Legacy probe 中均为 `0`，因此 v4 是 `Incomplete` 历史证据。v5 改为 generation-owned `benchmarkOwnedTextureMemoryBytes`；无 generation、无 owned texture、非正值、非空 workload 的 0 draw calls 或任一适用必需指标样本不足都会阻止 PASS。
- `Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v5.json` 与对应 `-player-ab-v5.json` 共 8 份报告全部为 suite v5 `Pass`。每份 Central/Legacy 都是 120/120 正式样本、0 个必需指标缺失、owned texture 为正、600-frame leak 与 teardown 通过，且 teardown owned bytes/resources 为 0；A/B workload/input/final checksum 一致。
- Windows Player 采用真实窗口化 graphics device，不使用 `-batchmode`/`-nographics`。当前 16-retry/cleanup 源码生成的 Editor `100/300/500/1000` 报告完成于 `2026-07-24 03:00:12`、`03:06:39`、`03:12:02`、`14:10:19`：logic tick 平均/最大依次为 `13.227/45.537`、`42.752/198.637`、`78.149/221.383`、`36.488/201.219 ms`。Editor 300/500/1000 平均均超过 30 Hz 的 `33.33 ms` 预算；Windows Player 1000 为 `9.123012 / 42.3011 ms`。报告 PASS 只证明门禁和可比 workload 通过，不等于性能达标，也不表示 Central 必然快于 Legacy；数据非单调且受 Editor/当前机器影响。
- fresh focused EditMode 为 `34/34 passed`，完整 `BattleRuntimeSelfCheck` 为 `PASS`，Runtime/Editor dotnet build 为 0 errors。连续矩阵的 300 Player 首次曾 native exit `-805306369`；同 build 独立 300 单样本和完整重跑均退出码 0，最终 300/500/1000 报告有效通过。该偶发启动失败保留为已知运行记录。
- 本轮修复一个 P1 benchmark 生命周期问题：Play Mode 退出可能留下 hidden runner，令已经消费的请求永久显示 `RUNNING`。processor 现于 `ExitingPlayMode` fail-close、在非 Play 状态 reconcile 残留 runner，并在 EditMode 保留 request；新增 3 个 focused tests 通过。
- P8-A/B/C 维持既有验收范围；P8-E Android/Adreno/Mali 真机由用户负责，T8 默认 `stage.dat` 部署取消/排除。下方 v3、v4、presentation-only 或“没有 Standalone Player”的描述只作历史追溯。

## 2026-07-23 P8 当前渲染验收（覆盖下方 P8-C/P8-D 的过时结论）

本节仅更新中央渲染 P8 的当前证据，保留下方历史审计记录。任何下方“P8-D 未运行真实 logic tick”或“没有 Standalone Player”的描述均已被 final v3 报告取代。

- **P8-B：**诊断数据现在有 `FrameId`、显式 `AtlasPageIndex`、strict central-binding validation、first unresolved/unsupported status，以及 generation/tick-coherent aggregate diagnostics。Runtime/Editor 的相关构建为 0 errors；focused/full checks 在当前证据范围内通过。
- **P8-C：**`Temp/P8-C-Resume-Live/P8-C-report.json` 在 `2026-07-23 17:28:29` **PASS**，覆盖正式 `LF2ObjectPointFactory.CreateObjectImmediate` / `FreeEntityLikeExe` 链。Pool 结果为 `availableBefore=7`、`totalCheckout=9`、`expandedAndPublished=2`、`availableAfter=9`、`uniqueRuntimeHandles=2`，且 cleanup PASS。`Entity(33,0)` type `0` 与 `Entity(100,0)` type `4` 均使用 `AtlasPageTexture2D`；前者 Legacy/Central alpha pixels 为 `4971/4971`，后者为 `2090/2090`，两者 maximum pixel diff 都为 `0`。范围仍不包含 skill-input opoint。
- **P8-D：**final v3 的 eight reports，即 `Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v3.json` 与对应 `-player-ab-v3.json`，均 **PASS**。它们不是 synthetic presentation-only test：每档使用 `MobileExtended(1050)` primary + mirror `SimulationWorld`、准确数量的真实 `LF2Entity` fixtures、`FrameInputSet.Empty`、完整 `NTSDBattleTickSystem`、30 warmup + 120 sample logic ticks、deterministic checksum、从真实 handle/generation/position 冻结的 presentation，以及 600-frame leak gate。A/B 运行相同 logic workload；不得将 PASS 写成 central 快于 legacy。

| report | logic tick avg ms | max ms | tick alloc avg/max B |
|---|---:|---:|---:|
| `100-editor` | `8.3087375` | `12.0803` | `0/0` |
| `300-editor` | `24.3566941666667` | `33.9412` | `0/0` |
| `500-editor` | `42.7971166666667` | `57.0061` | `0/0` |
| `1000-editor` | `100.006675` | `126.7602` | `0/0` |
| `100-player` | `0.537154166666667` | `1.285` | `0/0` |
| `300-player` | `2.59706583333333` | `29.4842` | `0/0` |
| `500-player` | `1.56702166666667` | `2.752` | `0/0` |
| `1000-player` | `2.980925` | `6.0687` | `0/0` |

Editor `1000` 约 `100 ms/tick`，所以不满足 30 Hz；Windows Standalone final v3 `1000` 的平均约 `2.98 ms/tick`。这些数值不替代 Android/Adreno/Mali 真机验证，后者仍为用户负责的 P8-E 排除项。T8 默认 `stage.dat` 部署同样继续排除。

最终顺序回归已关闭。held geometry 失败不是 benchmark 全局状态泄漏，而是 parentless/root renderer 的 `_visualTransform == rootTransform`：正确世界位置写入后，同一 Transform 又被 local-zero 重置。`LF2ObjectRenderer` 现只对独立 child visual 归零 local position；focused fixture 验证 `SetLogicObject` 保持 runtime X/Y/Z、`FirstPresentationTick`、`CentralShadowBuild` 模式与 legacy suppression，并要求 legacy root position 等于 immutable central command。fresh `Assembly-CSharp.dll` `18:05:55` 晚于相关源码 `17:59:02`；1000 实体 Central/Legacy A/B 于 `18:10:49` PASS，退出 Play 后完整 `BattleRuntimeSelfCheck` 于 `18:13:03` PASS。最终 Runtime/Editor dotnet 构建为 `0 errors / 42 warnings` 和 `0 errors / 48 warnings`。本节不提前声明新的 Architect PASS。

## 2026-07-23 P8 中央渲染验收更新（当前证据）

- **P8-C 已完成定义内的正确性/像素矩阵。** `Temp/P8-C-EditModeTest/P8-C-report.json` 为 PASS，覆盖 1000 次 generation reuse、超预热隔离扩容、Texture2DArray/OrderedPages、`A/B/A`、类别遮挡、4095/4096/4097 chunk、缺资源 fail-closed 与 frozen-frame Legacy/Central 像素对照；`Temp/P8-C-LivePool/P8-C-report.json` 为 PASS，真实 Play pool 从 `availableBefore=4` 获取 5 个对象，确认 5 个唯一 mount owner。旧 job `f278668e3a2445139c6a1a5ceb8815be` 的 11/11 是历史证据；P2 回归后的 fresh job `e455b7f70043438a938faa23e82e53f3` 为 12/12 passed（P8-C 2 + P8-D 10，0 failed/skipped）；fresh full `Temp/NTSD_BattleRuntimeSelfCheck.result` 于 2026-07-23 12:07:26 PASS，P2 `BattleRenderingBenchmark.cs` 11:56:24 < Unity DLL 11:59:33 < result 12:07:26。过滤到的 2 条 Console error 是自检刻意构造的 registration rollback / mismatched rest binding release 拒绝路径（`BattleRuntimeSelfCheck:7046` / `:1133`），无编译错误栈或 benchmark 异常。
- **P8-D 已完成受控表现基准矩阵，不是战斗容量或完整性能宣言。** `Temp/P8-D-presentation-100-ab-rerun.json`、`300`、`500`、`1000` 四份报告均 PASS；每档严格验证 presentation entity/command 数、256x256、资源/owned heap 与 retained heap 增长阈值。P2 已关闭 EditMode 把 mesh segment 冒充 `Graphics.DrawMesh` submission 的问题：`presenterSubmissionDrawCalls` 显式为 unavailable，Play 仅在实际调用提交后计数。它们是冻结的 synthetic presentation workload，不创建 `SimulationWorld` active entities、也不执行 logic tick；不可用的 main/render/GPU/draw 指标保持 unavailable。本轮没有 Standalone Player 实测，不能据此宣称全面性能收益或真实 active-entity 上限。
- **额外 current-scene production 覆盖。** `Temp/P8-D-current-scene-ab-v2.json` PASS：退出 Play 前真实 `NTSD_Battle` 的 `SimulationWorld ObjectCount=12/tick=3847`，published frame 为 `6 entities/12 commands`。Central/Legacy 均实际为 `6/12`、同 fingerprint `f3aaf429518f46ec`、同 256x256；retained managed heap 为 Central `+28672 B`、Legacy `+49152 B`，graphics/owned bytes 为 `+0`、resource count 不变。presentation build/GPU 只作本次 Windows Editor 样本，main/render/draw 仍 unavailable；这是额外生产覆盖，不是独立 P8 gate 或全面性能结论。
- **范围。** P8-E Android/Adreno/Mali 真机验证由用户负责；T8 默认 `stage.dat` 部署继续排除。下方 P8-C/D “待实施”“未验收”仅为历史快照，若与本节冲突，以本节为准。

## 2026-07-22 对象池预热上限后 opoint 武器不可见（当前状态）

- **复现：**隔离 `PoolInitialSize=10`，经生产 `opoint`/factory 保留 12 个 `LightWeapon`。第 11/12 个实体的逻辑、声音、unique root/renderer、mount/runtime handle、sprite 与 12 条 Entity command 均存在，但中央像素缺失。
- **定性与根因：**这不是 C# 战斗逻辑差异，也不是 pool 扩容、runtime handle 或资源问题；它是 Unity 表现后端适配缺陷。根因位于 `BattleDynamicMeshBackend` 的动态 submesh descriptor 生命周期：旧布局/增长时默认 descriptor 曾临时重叠；Unity 2022.3 收缩 `subMeshCount` 会截断 index buffer。权威 C# 不定义此 Unity 渲染实现。
- **修复：**每个 chunk 维护 `activeSubMeshCount`；physical `subMeshCount` 作为只增不减的 high-water。增长后先将全部 descriptor 置 inert，再写 active；非增长时先清旧 active，再写 active；empty 不收缩。禁止 bulk `SetSubMeshes`，此前该路径触发 native crash。
- **回归矩阵：**隔离预热 10，经生产 `opoint`/factory 保留 12 个 `LightWeapon`；检查 unique root/renderer、mount/handle、sprite、12 条 Entity command；覆盖 `1 -> 32 -> 1 -> 33 -> 1`、inactive inert tail、`GraphicsBuffer.count=24576`、`4096/4097` 边界、recovery、0 GC 与 scoped warning 捕获。
- **fresh 证据：**source `20:24:58` / `20:26:45` < DLL `20:28:54` < result `20:29:44` **PASS**；Unity 编译 `0 error`。本轮 `Editor.log` offset `31277122` 后 descriptor overlap、bulk `SetSubMeshes` 与 native crash 均为 `0`；Editor PID 响应正常。
- **验收边界：**代码、编译、self-check 与生产 `opoint` 链已验证；用户真实 Play Mode 视觉复测仍待确认。T8 明确排除，默认 `stage.dat` 部署继续暂缓。

## 2026-07-22 Rendererless 武器显示回归修复（当前状态）

- **复现与根因（旧复现限定）：**4 个随机掉落武器已存在时，角色 `opoint` 再生成武器会使既有掉落武器及新武器不显示；后续 `opoint` 仍不显示，但落地声音继续。rendererless `LF2Sprite.Hide` 将 `EntityVisible=false`，而成功 `ShowPic(valid)` 没有像旧 `SpriteRenderer` 路径那样恢复可见性。因此 `CurrentEntry`、`pic`、战斗逻辑与声音均正常，中央渲染仍永久过滤 Entity command。此 `EntityVisible` 根因只解释该旧复现，不解释 `PoolInitialSize=10` 后第 11/12 个实体已有 command 但缺像素的问题；后者以本文件上方的动态 submesh descriptor 适配缺陷为准。
- **修复边界：**只在 catalog 或 legacy sprite 成功解析时恢复 `EntityVisible`；`pic=999` 和缺失资源仍保持不可见，不把失败语义改为显示。
- **验证证据：**Unity `Assembly-CSharp.dll` 于 `2026-07-22 18:56:11` fresh compile，Console 为 `0 error`；完整 `BattleRuntimeSelfCheck` 于 `18:58:50` **PASS**；`dotnet build Assembly-CSharp.csproj` 为 `0 errors / 42 warnings`。Play Mode 中先保留 4 个随机武器，再经 `LF2ObjectPointFactory` 的 `opoint oid121` 调用 `Hide -> ShowPic`，随机 slot `50` 和 opoint slot `54` 均仍有 Entity command；销毁并复用同一 renderer instance 后再次 `opoint`，slot `54` command 仍存在；central `IsStale=false`、`unresolved=0`。
- **范围：**此记录只关闭该 rendererless 显示回归；不宣称全部战斗系统、全部资源组合或设备表现已完成验收。T8 默认 `stage.dat` 部署继续暂缓。

## 2026-07-21 集中式渲染 Fresh Final Validation（当前状态）

本节记录与战斗可观察行为直接相关的中央渲染验收，覆盖本文件顶部及后续旧快照中“`CentralOnly` 不可用、Overlay blocker、P7 未完成、B2C 未经 Architect 验证”的过期措辞。

- **CentralOnly 实际运行**：诊断为 `requested/effective=CentralOnly`，`frame/ownership/ready/submitted=true`，`draws=12`；P7 Overlay、Shadow、Entity、HitRecord 已共同进入单帧 pixel owner，不再有旧后端与中央后端双重出像素。
- **伪影修复依据**：`BattleDynamicMeshBackend.ClearActive` 曾把 `subMeshCount=0`，触发 Unity 2022.3 释放 native index buffer，造成后续索引错误、黑块及三角形 UV 伪影。修复为保留零索引 inert submesh；不是对战斗 runtime、DAT、挂点或排序规则的改写。
- **同帧像素验收**：暂停同一帧的 Legacy/Central `1920x1080` 截图比较为 `changed=0`。该截图直接覆盖当前可见的角色、武器/球体与阴影；Overlay/HitRecord 的 ownership 与资源路径另由 self-check 和运行时 diagnostics 证明，不宣称它们在该截图中一定可见。它不能代替所有角色、资源组合和设备的逐帧生产证书。
- **运行时与空间查询**：`Temp/NTSD_BattleRuntimeSelfCheck.result` 为 **PASS**，Unity Console **0 error / 0 warning**。真实 Play 的 `LooseQuadtree` 为 `backend=LooseQuadtree, objects=12, tick=1436`，Console 同为 **0 error / 0 warning**。B2C 的 Architect final 结论为 **PASS / no P0-P2**。
- **Editor 性能记录，不作移动端结论**：Legacy `6.1884 ms CPU / 0.346112 ms GPU / 18 draws`；Central `6.5114 ms CPU / 0.70656 ms GPU / 20 draws`；Central 内存 `1391.17 MB allocated / 1005.19 MB graphics`。
- **外部边界**：尚未取得真实 Adreno/Mali 或 Android Player 的像素、兼容性及性能证据。T8 默认 `stage.dat` 部署仍按用户要求暂缓；该资源前置不构成当前代码差异。

> 下方出现的“CentralOnly 继续拒绝”“Overlay 未实现/阻塞”“P7 仍未完成”“B2C 未经 Architect 复核”以及“Play/pixel/Profiler 尚未验收”等表述均为历史快照。保留它们用于追溯，但当前状态以本节为准。

## P7 Batch6 per-entity Overlay 当前状态（2026-07-21，覆盖旧 Overlay blocker 结论）

- P7 Batch6 已完成代码侧 Overlay 收口，不再把 per-entity Overlay、`WORDS0..5` 缺失或“current Overlay blocker”列为当前代码差异。`WORDS0.bmp` 至 `WORDS5.bmp` 已加入 Unity Assets；其 SHA256 与权威 C# host 引用的运行时资源来源一致。此核验只确认资源依赖，不改变唯一战斗逻辑权威 `J:\QQFile\NTSD2.4\ntsd_release_C#`。
- `BattleSpriteCatalog.CommonWordGlyph(sheet, charCode)` 覆盖 `6 * 256` glyph，按 top-left authority rect 转 Unity bottom-left rect；WORDS prewarm 使用 exact-black transparency、Point/Clamp、atomic publication 与 retirement ownership。`BattleSlotLabelRuntimeState` 已提供 `char[10,12]` + `int[10]` 并接入 reset/`MatchConfig` bootstrap。
- `BattleEntityOverlayLayout` 已无分配地布局复活 counter、普通/括号标签、普通 `Com` 与特殊 `WORDS5 Com`；标签 clamp、counter 不 clamp，容量异常 fail-closed。presentation snapshot 保留原始 `ObjectId` 用于 shadow OID223/224 gate，并单列 current DAT identity 用于 Overlay；命令固定为 `Shadow -> Entity -> OverlayGlyph -> HitRecord`。
- legacy 后端已有 pooled `BattleEntityOverlayRenderer`，含 generation/stable-id guard；`LegacyOnly` 发布 immutable frame 但不构建 central mesh，`CentralShadowBuild` 仍仅诊断，`CentralOnly` 继续由 `ValidateAvailable` 显式拒绝。frame-level catalog lease、HitRecord cycle lease finalizer 和 empty-frame no-retain 均已覆盖；retirement 窗口、命令顺序与 zero-GC 进入 self-check。
- fresh 证据：latest relevant source `2026-07-21 16:01:49` < Unity DLL `16:03:35` < full self-check result `16:04:54` **PASS**；Unity Console **0 C# error**；最后一次主代理 `dotnet build` **0 errors / 18 existing warnings**；Architect final **PASS / no P0-P2**。`git diff --check` 待主任务最终统一执行。
- 这只关闭代码/编译/self-check/静态复核层的 Overlay 缺口，不构成 P7 全门槛或完整 Play 验收：Play/pixel/Profiler/Adreno/Mali 未验收，T8 默认 `stage.dat` 部署继续排除。本文后续相反的 Overlay 结论均为历史快照，除非明确另行重开。

## B2C Extended checksum（2026-07-21 当前状态）

`Authority400` 的 `ntsd-battle-trace-v3` 与 direct parity guard 保持不变；`MobileExtended` / `DesktopExtended` 已使用独立 `ntsd-unity-extended-battle-checksum-v1`。容量感知 slot metadata、generation/stable ID、active/runtime raw state、稀疏 ARest/VRest、rest binding guard 与 non-materializing capture 已落地。B2C 也已接入 generation-aware AI Loose Quadtree 输入快照查询，以及显式 `LooseQuadtree` 后端下的即时 weapon/body current-world 查询；索引、几何或映射异常均回退 brute，生产默认仍是 `BruteForce`。最新 full `BattleRuntimeSelfCheck` `2026-07-21 00:48:06` **PASS**；`dotnet build` **0 errors / 42 existing warnings**；`git diff --check` 通过。先前复审的两个 blocker 已修复并进入 self-check，但 fresh 最终架构复审待补，因此当前状态不是 Architect PASS。

## 集中式渲染 P1-P6 与 P7 Batch1-3 当前状态（2026-07-21）

P1 compact legacy sorting 已完成代码与自动验证；具体排序、`8192` legacy guard、同层 renderer 检查和 Play-unverified 边界见集中式方案文档。P2 immutable `BattleSpriteCatalog` 也已完成代码层实施：唯一 key 为 `(LF2Entity.ResolveCurrentDataObjectId(entity), effectivePic)`，entry 保存 source sheet/shared texture、bottom-left rect、UV、metrics、pivot 与 legacy `Sprite`。

P2 prewarm 使用 invocation-local staging、generation/disposed gate 和原子 publish；configs、`MergedSprites`、catalog 只会整体替换，失败、stale result 与 teardown 均清理。renderer 引用计数把旧 catalog 的退役推迟到零引用。正式 partial BMP 按 declared row/col + `localPic` 建立稀疏 rect 并保留 holes，normal/swapped 仅在完整匹配时择优；weapon6、weapon3 等生产矩阵已进入 self-check。display、collision、anchor、SpecialAttack point-center 与 shadow metrics 不再以战斗期 `Sprite.rect` 为真值；`pic=999`、missing key、current identity 切换和 pool reuse 均清除旧表现引用。

P2 fresh 证据为 source `2026-07-21 04:16:00` < Unity DLL `04:17:06` < full `BattleRuntimeSelfCheck` `04:18:04` **PASS**；dotnet build **0 errors**。由于 Unity 自动生成 `.csproj` 的刷新视图不同，既有 warning 数分别出现 18 与 42，本节不冻结 warning 数。最终 architect review **PASS / no blocker**；最终 code review **no P0-P2 findings**。本轮未执行 Play Mode、真实异步 BMP stress 或性能验收，因此 P2 仅能标为“代码、编译、self-check、静态复核完成；Play/stress/performance-unverified”。

P3 已实现 value-only immutable presentation snapshot/commands、double buffering、几何容量增长和 atomic publish。模式边界是默认 `LegacyOnly`、诊断 `CentralShadowBuild` 和明确拒绝的 `CentralOnly`。命令按 `(ZInt, runtime slot)` 排序，并为每实体依次产生 `Shadow -> Entity -> Overlay -> HitRecord`。早期 `AuthorityExpectedButLegacyMissing` 标记来自不完整权威盘点，现已废止；权威两个 host 实际都绘制 per-entity Overlay，Unity 尚未实现，所以 P3 不能宣称 overlay 等价。

P3 actual legacy probe 直接采样真实 renderer 的 sprite、texture、material instance、rect、pivot、position、flip 和 sorting；HitRecord 在 legacy advance 前采样。catch-up 帧的中间逻辑 tick 因没有对应实际 renderer 状态而明确记录为 `Incomplete`，包含 count/first/last，只有最后可观测 tick 能做完整 probe。persistent scratch 已由 steady `RenderDispatch` zero-allocation self-check 覆盖；zero-hit 经 `SparkRenderer.RenderAll` finalize，production pool 路径覆盖 nonzero spark atlas cells、每 tick age once 和 `OnDisable`/`OnDestroy` 归池。P3 诊断与战斗 checksum 隔离。

P3 fresh 证据为 source `2026-07-21 05:38:38` < Unity DLL `05:39:29` < full `BattleRuntimeSelfCheck` `05:40:16` **PASS**；dotnet build **0 errors / 18 existing warnings**；最终 architect review **PASS / no blocker**，最终 code review **no P0-P2 findings**。未执行 Play Mode、真实 SPARK BMP/设备或性能验收；未来异步 consumer 仍必须持有 catalog lease 并验证 generation。不能把 catch-up `Incomplete` 中间 tick 扩大为逐 tick actual legacy parity 已验证。

P4 已完成代码层实现：中央 Mesh 后端持久复用，并以 `4096` quad/`UInt16` index 的固定 chunk 契约切分。`OrderedChunks` 只合并相邻兼容命令并保持 `A,A,B,A` 原序；`StrictOrderedDraw` 提供更细的正确性回退。unresolved command 是提交 barrier，stale chunk/submesh 会清空。模式边界继续是 `LegacyOnly` 不 build、`CentralShadowBuild` 不提交、`CentralOnly` 在全类别 ownership 完成前拒绝。

P4 URP pass 过滤为 world camera 的 `Base` camera，注入点为 `AfterRenderingTransparents`。`BattleRenderFeature` 已验证为 active renderer asset 的唯一 subasset。初审发现 feature B 覆盖 A 后注销 B 不恢复 A，现已改为 registration stack，并以 `A -> B -> unregister B -> restore A` 覆盖 fallback material、array material 与 draw mode 恢复。

P4 fresh 证据为 source `2026-07-21 06:32:00.287` < Unity DLL `06:32:56.970` < full `BattleRuntimeSelfCheck` result `06:33:43.796` **PASS**；dotnet build **0 errors / 42 existing warnings**；最终 architect review **PASS / no P0-P2 findings**。没有执行 Play Mode、桌面像素 baseline、Profiler GC 或 Android/Adreno/Mali 验证，故只能标为 P4 代码/self-check/静态复核完成，不能宣称全部验收门槛完成。

P5 已完成代码层实现：确定性 planner 以 whole-sheet 为单位生成 `2048 x 2048` 多页布局，按 normalized path ordinal 去重；同 path/同尺寸的 pixels 冲突会拒绝。sheet 使用 `1px` extrusion。满足能力 gate 时建立 `RGBA32 Texture2DArray`，否则使用保持相同 page 顺序的 2D fallback。catalog entry 保留 legacy source 并增加 immutable central binding；manager 以事务方式发布，明确持有 Unity Object ownership，renderer 与 central lease 一起控制旧资源退役。

P5 array shader 使用 per-vertex slice，允许相邻跨 slice 命令在相同 array material 下保持顺序合批；2D fallback 的 `A/B/A` 保持三个连续段，禁止重排。array/fallback 双 shader、material、pass 和 installer 均已接线。复核关闭两个 P2：同 path、同尺寸、不同 pixels 对两种输入排列都拒绝，equal-content duplicate 成功；显式两页 fallback 在 page0 成功、page1 失败时，两页均销毁且不产生 partial publication。

P5 fresh 证据为 source `2026-07-21 07:06:28` < Unity DLL `07:07:12` < full `BattleRuntimeSelfCheck` log `07:08:13` **PASS**；dotnet build **0 errors / 42 existing warnings**；architect final review **PASS / no P0-P2 findings**，code review **no P0-P2 findings**。未执行生产 BMP Play、桌面 overlap pixel baseline、Profiler/allocation stress、Android/Adreno/Mali array/fallback 与内存性能验收，因此 P5 仅为代码/self-check/静态复核完成，不能宣称全部验收完成。

P6 已完成设备策略与诊断代码：`BattleRenderingDevicePolicy` 是 immutable capabilities，`FromSystem` 是唯一系统能力采集边界。resolver 严格按 CLI > `GameConfig` > Auto 解析 `-ntsdBattleAtlasMode` / `-ntsdBattleDrawMode`，在 `TextureArray` 与 `OrderedPages` 间安全 fallback 并报告原因；draw mode 只在 Auto、`OrderedChunks`、`StrictOrderedDraw` 中选择，`SingleMesh` 不进入生产。确定性 JSON report 显式记录请求、capabilities、effective mode 与 fallback reason。

P6 manager 每次 publication 只解析一次，central 使用缓存的 effective draw mode；每 tick 不再查询 `SystemInfo` 或 CLI。该策略不改变 profile、capacity、tick、collision、checksum 或 `CentralOnly` guard。P6 尚未完成 Adreno/Mali、Play、pixel baseline 或 Profiler 验收，因此只能宣称代码策略/诊断完成。

P7 Batch1 完成 held-object 子批。权威链为 `InteractionRuntimePasses -> WeaponPointRuntime/WeaponRuntime -> SdlBattleRenderer/BattleHostForm`；legacy 与 snapshot 共用 pure held-offset helper，在 capture 时将 offset 固化为 immutable 值并追加到 Entity command。right/left、target mismatch、release、missing holder/wpoints、slot generation reuse、dormant holder 与 legacy/central equality 均已覆盖。

P6/P7-held 统一 fresh 证据为 self-check source UTC `23:42:44` < Unity DLL `23:44:03` < `Unity-P6-P7-Final2-SelfCheck.log` `23:45:00` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS**，code review **approve / no P0-P2 findings**。

P7 Batch2 已完成 render-state semantic parity：snapshot/command 持有 value-only `Color32`、flipX/flipY、mask/material semantic 与 logical resource key，instance ID 仅用于诊断。catalog 提供 immutable `Sprite -> key[]` 反查和 preferred entity key。legacy probe/Compare 检测 RGB、alpha、flipY、unsupported state 与 logical key；central resolver 转发 color，对无法解析的语义 fail closed。

Mesh 把 color 写入 quad 四顶点，flipY 通过交换 V 坐标实现；color 变化不切 segment，material semantic variant 必须断段。pool entity/shadow/spark checkout 重置为 white、flipXY false、mask none；首次干净 checkout 借用 `Sprites/Default.sharedMaterial`，不触发 `.material` 实例。

两个中央 shader 依据 Unity 官方 `2022.3.4f1` builtin shaders ZIP changeset `35713cd46cd7` 改为 `Blend One OneMinusSrcAlpha`，最终 `rgb *= a`，并声明 `NTSDAlphaContract` tag；installer 验证 white/tag。fresh 链为 source `08:27:50` < DLL `08:28:48` < self-check log `08:29:48` **PASS**；installer validation **PASS**；dotnet **0 errors**；architect/code review **PASS / no P0-P2 findings**。

P7 Batch3 已完成 Shadow。实现依据 authority `BattleHostForm` / `SdlBattleRenderer.DrawShadow` gates；资源采用 typed `EntitySprite`/`CommonShadow` key。`GameConfig.ShadowPrefab` 被捕获为 immutable borrowed binding，包含真实 sprite、texture、UV、size、pivot、color 和 material；manager 在 main thread atomic common publication，borrowed Unity Object 不进入 owned retirement。

snapshot 保存 actual ObjectId 与 `HasCurrentFrame`；Shadow command 携带 real descriptor/`CommonShadow` 并位于 Entity 前，legacy probe 对比 exact sprite。central resolver 校验 sprite、texture、rect、pivot、material ID，并使用 source2D + fallback material；missing config/resource 一律 fail closed。actual OID223/224、state3005/9997、`Link < 0`、HitStop 与 missing frame 已对齐。

review 关闭 P1 missing-frame legacy/central，以及 P2 material ID、真实 `GameConfig` asset、real commit -> replace retirement tests。fresh 链为 source `09:29:03` < Unity DLL `09:31:10` < self-check log `09:32:07` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect/review **PASS / no P0-P2 findings**。

Batch3 结束时 P7 仍未完成，Play、实际 pixel baseline 与设备未验收，HitRecord/Overlay 当时均未收口。后续 Batch4/5 已关闭 HitRecord resource/lifecycle 代码缺口；当前仍由 Overlay 阻塞 `CentralOnly`。T8 继续排除。

P7 Batch4 已完成 **SPARK / Common HitRecord resource ownership** 的代码层收口：typed `CommonSpark(pic)` 的 20 帧资源在 prewarm 中只 decode/process 一次，再于 main thread atomic publish；legacy `SparkRenderer` 不再在 `Awake` decode 或创建资源。central resolver 验证 logical key、`Sprite`、`Texture`、rect、pivot、size 和 material；publication lease/retirement 已接入。

Batch4 失败契约：缺失/无效 SPARK 释放 stale lease 且不修改 `HitRecord` age/count；partial `Texture`/`Sprite` 构造失败事务式清理，不能发布半成品资源。fresh 链为 source `11:13:05` < Unity DLL `11:15:20` < result `11:17:38` **PASS**；architect re-review **PASS / no P0-P2 findings**。code-review provider 为 `429`，不得表述为 code-review 通过。

此项不构成 P7 整体或运行时验收：Play、pixel、Profiler、真机和真实 SPARK 资源路径未验证；Batch4 当时未包含的 HitRecord lifecycle mutation 已由下方 Batch5 收口。T8 继续排除。

P7 Batch5 已完成 backend-neutral immutable double-buffer HitRecord presentation cycle。`RenderDispatch` 捕获 owner handle/generation、count、age、x/z 与 frozen common publication；`SparkRenderer` 只 materialize/probe，不写 live HitRecord。`LateUpdate` 顺序固定为 legacy materialize -> central `PrepareFrame` -> one finalizer；catch-up 只 finalize 最后 cycle。

Batch5 mutation 契约：missing SPARK zero-write；valid age 每 cycle 恰好 `+1`；invalid sampled tail 每 cycle 最多删除 1 项，`4/14/28/38` 进入 gap 的同 cycle 不删除。slot reuse、count/age guard 已覆盖，pool/camera/backend 不影响结果。后续 P2 修复将 binding 改为 direct ownership transfer，无 per-tick lease GC；no-hit 不持 binding。coordinator reset 接入 world reset、driver unbind、world replacement、destroy；ordered owner cursor 为 O(N)，`1000` owners 精确为 `1000` comparisons。

Batch5 fresh 链为 source `12:39:24` < Unity DLL `12:40:40` < result `12:41:20` **PASS**；dotnet build **0 errors / 18 existing warnings**；architect **PASS / no P0-P2 findings**；code review **APPROVE / no P0-P2 findings**。Play、pixel 与 device 仍未验收。

Overlay authority re-audit 将其确认为当前 blocker，而不是空占位：权威 `BattleHostForm` 与 `SdlBattleRenderer` 都按 `Shadow -> Entity -> EntityOverlays -> HitRecords` 绘制。per-entity Overlay 内容为 `Hp2Orig > 1` 的复活次数和 entity label；资源 `WORDS0..5.bmp` 的 glyph 为 `8x16`、步距 `9`、black colorkey。Unity `Assets` 当前没有 `WORDS0..5`，也缺 `BattleSlotLabels[10,12]` / state 镜像和 snapshot 字段契约，因此 Overlay 未实现，`CentralOnly` 继续拒绝。global function/pause overlay 是独立后置 UI，且 GDI/SDL 不一致，不塞入 per-entity P7，本批不处理；T8 继续排除。

以下旧阶段中“Extended checksum 跳过/为空、schema 未实施”或“即时 weapon/body、AI 查询未迁移”的陈述是历史快照，已由本节覆盖。Loose Quadtree 默认启用证据、P1-P6 的运行时表现/真机/真实资源/性能验收，以及 P7 Overlay 实现仍未完成；Batch5 已关闭 HitRecord lifecycle mutation 代码层缺口。T8 已排除，不计入完成条件。

## BATTLE-RENDER-PLAN1 集中式战斗渲染系统方案（更新于 2026-07-20）

移动端集中式战斗渲染与 runtime 容量/空间索引决策已记录在 [central-battle-render-system-plan.md](central-battle-render-system-plan.md)。当前状态是 **R1-R2C-4、B0、B1-B1.3、B2A 与 B2B generation-aware incremental Loose Quadtree 已完成代码层实施和既定验证**。

`Authority400` 已接入 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused`，保留 C# 权威 400 槽、特殊槽区与最低空闲槽语义；`SimulationWorld` 仍显式 pin `Authority400`。fresh 证据为源码 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` `12:05:07` **PASS**；100,000 次随机分配操作与朴素扫描模型对照 **PASS**；架构复核 **PASS**。

R2A 已建立固定 `PageSize = 256`、按需物化的 `RuntimeSlotTable`，验证 `Authority400` 的 400 逻辑地址、`MobileExtended` 设计所需的 1050 逻辑地址及最后一页尾部 guard、每槽独立 raw runtime/rest 存储、`ClaimedCount`，以及 `(slot, generation)` 句柄在 release、同槽 reuse、reset 后使旧引用失效。fresh 证据为源码 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` `12:36:53` **PASS**；架构复核 **PASS**。

R2B 已将生产 `Authority400` registry 迁移到单一 `RuntimeSlotTable`，替换旧的 used/raw runtime/raw rest 并行数组；slot 到当前 occupant 为 O(1) 查询。live ascending slot scan 保留游标以上新生实体同 pass 可见、游标以下低槽复用实体延至下一 pass 的时序；release 以 `expectedEntity`/当前 occupant 防止旧实体释放复用槽。stage spawn 的 raw rest 恢复/消费、ordinary spawn 重置语义，以及 `ObjectCount`、buckets、`SceneQueryHit` 的 slot-address 契约均保持不变。fresh 证据为生产源码 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` `12:57:02` **PASS**；`dotnet build` **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索 **0**。

R2C 已为 `RuntimeSlotAllocator` 与 `RuntimeSlotTable` 实现单调 `GrowTo`：增长保持 min-heap、`nextUnused`、claims、既有 pages、occupants、generation handles、raw runtime/rest；等容量调用为成功 no-op，缩容拒绝且不改变状态。移动端契约同时修正为 **1000 active admission + 1050 logical slot addresses**：保留 `0..49` 后，1000 个动态槽为 `50..1049`；256 槽分页会建立 5 个物理页地址区间，但 `1050..1279` 必须不可访问。fresh 证据为源码 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` `13:25:34` **PASS**；`dotnet build` **0 errors**；架构复核 **PASS**。

R2C-3A 已让 `SimulationWorld.RuntimeSlotCapacity` 读取当前槽表逻辑容量，并将 registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量边界改为当前实例容量。默认 `SimulationWorld()` 仍是 `Authority400/400`；新增 internal `DesktopExtended/512` focused contract 仅验证 slot `511` 注册/查询/AI 可见、slot `512` 拒绝和 reset 清理，不是生产 Profile 接线。`BattleParitySnapshot` 继续固定使用明确的 400-slot authority schema。fresh 证据为相关源码约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` `13:54:22` **PASS**；fresh `dotnet build` **0 errors / 42 warnings**。

R2C-3B 已关闭外部固定容量边界：`LF2SpecialAttack` 的高槽 holder 验证和 Karasu oid209 扫描使用当前 world capacity；`LF2Entity` transition effect 的可用槽计数使用当前 dynamic range。历史 parity capture 现在必须同时满足 `Authority400` Profile 与 400 逻辑容量，明确拒绝 `DesktopExtended/512` 和 `DesktopExtended/400`，避免同容量非 authority world 伪装成旧 certificate。fresh 证据为相关源码 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors**，warnings 为既有告警。

R2C-4 已激活生产 Profile：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用解析/创建路径，直接 `BattleTestBootstrap` 在实体注册前协调晚到的 `GameConfig`。默认容量为 `Authority400=400`、`MobileExtended=1050 logical / TOTAL active admission 1000`（跨全部槽区计数）、`DesktopExtended=512 initial`（按 256-slot 页规范化并自动增长）；Desktop 增长保持最低空洞优先并同步 AI snapshot。Extended Driver checksum 当前跳过/为空，direct parity 仍严格拒绝非 `Authority400/400`。fresh 证据为相关源码 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors / 42 existing warnings**；architect final review **PASS**。

B0 已落地纯数据 X/Z half-open Loose Quadtree shadow：`looseness=1.5`、`leafCapacity=16`、`maxDepth=8`，每次 collision collect 全量重建，诊断默认关闭。诊断比较 brute AABB pair、tree pair 与正式 accepted subset；正式 `i/j`、VRest、RNG、candidate 收集/截断/消费流程保持不变，shadow 结果不写回战斗真值。fresh 证据为相关源码不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` `16:15:43` **PASS**；fresh `dotnet build` **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。该结果不代表性能提升或正式 broadphase 已切换。

B1 已建立纯数据 `RuntimeRestStore`：分页/惰性 ARest；定向稀疏 `VRest[victim, attacker]` 只存正值、写零移除；`ResetSlot` 同时清 ARest、victim row 与 attacker column；支持 `GrowTo`、全局 reset、排序 diagnostics/snapshot 与 restore。2,000 次随机操作已与 dense reference model 逐步 differential。fresh 证据为相关源码 `2026-07-20 16:31:32` < Unity `Assembly-CSharp.dll` `16:36:38` < 完整 `BattleRuntimeSelfCheck` `16:37:13` **PASS**；fresh `dotnet build` **0 errors**；architect final review **PASS**。B1 尚未接入生产。

B1.1 已实现 optional `LF2ItrRestTracker` facade 与 exclusive victim-row lease：绑定 store 的 facade 独占一个 victim row，释放后其他 owner 才能接管；未绑定时保留既有 tracker 路径。architect 首轮发现 `ReplaceVictimState` 对 mixed-invalid attacker 输入可能部分写入，现已改为完整预验证后原子替换；direct `ReplaceVictimState` 与 facade `Bind` 均新增 failed-import 原状态不变测试。B1.1 阶段 production world 尚未绑定 facade，后续由 B1.2 接入。复跑 `dotnet build` **0 errors / 18 existing warnings**；相关源码 `2026-07-20 17:34:22` < Unity `Assembly-CSharp.dll` `17:36:49` < 完整 `BattleRuntimeSelfCheck` `17:39:07` **PASS**；architect final review **PASS / no blocker**。invalid bound `RestoreState` 可后续补独立断言，但复用已验证 atomic 入口，不构成 blocker。

B1.2 production lifecycle 已完成代码层实施与验证：`SimulationWorld` 持有 store，ordinary claim `ResetSlot + Bind(false)`，release 保留 store 并解绑，`StageSpawnAt` post-Initialize retention，world reset/grow 同步；`RuntimeSlotTable.RawRest` 已删除，parity fallback 直读 store。B1.2 初轮审查发现 Stage pool 回收不完整与错槽 release 未拒绝，次轮发现 release 拒绝未传播，末轮复核 PASS/no blocker；partial import 属于 B1.1，不计入 B1.2 三轮审查。`18:13:00` 与 `18:22:59` 保留为非完成历史证据。最终 `dotnet build` **0 errors**，源码 `18:31:25` < DLL `18:33:58` < self-check `18:34:54` **PASS**。

B1.3 已实现 collision pair VRest tick 解耦：正式顺序为 `CaptureSnapshots -> sparse Tick -> Collect`；eligible `active + CharData` row 递减，inactive row 冻结；`BruteForceSceneQuery` 删除 pair 内 tick。初版 `19:11:13` PASS 后 architect 发现 eligibility 仍按 `RuntimeSlotCapacity` 全扫，该证据保留为非完成记录。最终改为直接遍历 registered bucket items，无 capacity scan/eligibility snapshot 分配；Desktop sparse high-slot 测试 `visited=2`。最终 `dotnet build` **0 errors**，源码 `19:19:14` < DLL `19:19:47` < self-check `19:22:50` **PASS**；architect final review **PASS / no blocker**。

B2A 已实现独立 `CollisionBroadphaseBackend.BruteForce/LooseQuadtree` 正式后端，选择优先级为命令行 `-ntsdCollisionBroadphase` > `GameConfig.BattleCollisionBroadphaseName` > 默认 `BruteForce`。它只替换 fixed-tick candidate collect；即时 weapon/body query 不变。formal participant 保留 brute authority ordinal，tree 与 invalid-AABB fallback-all pair 统一转换为 canonical slot pair、排序去重，再按 authority ordinal 双向派发。slot/mapping/index/entry count 非法、rebuild/query 异常或 diagnostics 缺少 brute coverage 时，整 tick 丢弃 formal 输出，原子恢复 RNG/candidate state 并重跑 brute-force；extra pair 交 narrow phase。fresh 证据为源码 `2026-07-20 22:15:07` < Unity `Assembly-CSharp.dll` `22:18:48` < full `BattleRuntimeSelfCheck` `22:19:28` **PASS**，`dotnet build` **0 errors**；architect final review **PASS / no blocker**。

B2B 已把 formal backend 从每 tick full rebuild 改为 collision collect 边界的 batch synchronize。索引身份使用 `(runtime slot, generation)` handle：未移动实体保持原记录，AABB 在当前 loose 范围内变化时原位更新，跨 loose 范围时迁移；spawn/remove、valid/invalid AABB 转换与同槽复用都在下一 collect 收口，root escape 才执行 full rebuild。query handle 必须通过当前槽表 generation 解析并核对 entity/ordinal；sync、query、invariant 或 mapping 失败会 reset 索引并走 B2A 的整 tick brute/RNG/candidate rollback。world reset 也显式清空 formal index。fresh 证据为源码 `2026-07-20 22:43:57` < Unity `Assembly-CSharp.dll` `22:46:36` < full `BattleRuntimeSelfCheck` `22:47:04` **PASS**，`dotnet build` **0 errors**；architect final review **PASS / no blocker**。

本批未执行 Play Mode。生产默认仍为 `BruteForce`；即时 weapon/body query、AI 查询、Extended parity/replay/checksum schema 与集中式渲染仍未迁移或实施。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT14 DAT movement 显式值读取回归（2026-07-19）

本段覆盖下方 BATTLE-AUDIT13 关于“可玩 Naruto `oid2 running_speed=8`”和“`BattleVisualScale` 临时为 `1`”的旧结论。生产 Naruto DAT 的显式值是 `running_speed=15`，Unity 实体表现缩放已恢复为项目要求的 `BattleVisualScale=1.5`。

| 项目 | 根因 / 契约 | 修复与验证 |
|---|---|---|
| DATA-01A movement regression | DATA-01A 先前把 `LF2CharacterData` 的兜底值从 Unity 旧值 `15` 改为 C# 权威默认值 `8`；但 Unity parser 同时遗漏了 `<bmp_begin>` 内无冒号的 movement `key value`，导致生产 Naruto 的显式 `15` 错误回退为 `8`。这是 Unity loader bug 和本轮对齐回归；先前仅归因于 `1.5` 缩放并不完整 | `Lf2DatParserV2` 只对白名单中的 BMP 顶层 18 个 movement 键支持无冒号 `key value`，不扩大通用语法；`ExtractMovementParameters` 读取 `Bmp.Properties`，浮点数和 `frame_rate` 均使用 `InvariantCulture` 正确解析；DAT 真正缺字段时仍保留 C# 默认 `8` |
| 生产与合成覆盖 | 显式 DAT 值必须覆盖默认值，且 BMP 顶层 movement 不能泄漏到 frame、weapon、stage 或 data 语法 | 生产夹具断言 Naruto `15`、Kakashi `18`、Sakura `17`、Sasuke `23.9`、clone `15`；weapon4 冒号语法 guard；synthetic 覆盖全部 18 键、last-wins、frame 隔离和缺省 `8` |
| 同类遗漏审计 | 审计当前 101 份 DAT；除 5 份角色 DAT 的 18 个 movement 字段外，没有第二组当前生产数据会触发同类无冒号属性遗漏 | weapon/frame/stage/data 现有生产语法安全。多词 `name` 属潜在 parser 表示风险但不进入战斗逻辑；`catchingact/caughtact` 双值是未来风险，当前 218 处两值均相等，现有消费者无可观察差异 |

fresh 证据：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 14:39:43.992`，晚于相关源码，Console C# error 为 **0**。一次请求因 Editor 误留在 Play Mode 未作为结果；退出 Play 后 fresh full `BattleRuntimeSelfCheck` 于 `14:44:58.748` 返回 **PASS**。真实双击 D 的 Play trace 因 UnityMCP 临时注入卡住而未完成，因此本轮不宣称该 Play 场景已验收；T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT13 Naruto 防下攻与跑速缩放复核（2026-07-19）

常规战斗逻辑的唯一权威仍为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。本项是用户明确指定的例外：Naruto 防下攻以用户已验证表现正确的 `J:\QQFile\NTSD2.4\ntsd_release` C++ 版本作定向参考；该例外不改变其他战斗逻辑的 C# 唯一权威规则。

| 项目 | 参考行为 / 根因 | Unity 修复与当前状态 |
|---|---|---|
| 防下攻（DDA） | C++ 中 `oid2 frame286` 的 `centery=79`，opoint 为 `y=80 action=240 dvy=0 oid=33`，因此 child 初始 `Y=+1, Vy=0`。角色物理落地要求 `new_y > 0.0001 && pre-move Vy > 0.0001`；初生分身不会立即进入 frame219，而是按 `240 -> 241 -> 242 -> 243 -> 235 -> 236(dvy=-7) -> 244..247` 推进，真实下降落地后才进入 `219 / AI` | Unity 根因是 `CharacterMechanics` 的 `landed` 判定缺少 `Vy` 门槛；旧 `LateOpoint + state15` 专项 gate 范围过宽，并且仍会把 `Y` 钳为 0。现已改为与参考行为一致的通用 `landed` 条件，并移除专项 gate；`CheckLateOpointState15LandingControls` 与 `PH-02` 三向速度矩阵已同步更新 |
| 奔跑与缩放 | 可玩 Naruto `oid2` 的逻辑 `running_speed` 仍为 `8`，固定逻辑频率仍为 30 Hz，本轮未修改跑速规则 | 按用户要求，`BattleVisualScale` 临时由 `1.5` 改为 `1`，仅供用户复测奔跑速度体感；此项是 Unity 表现缩放测试，不代表逻辑跑速发生变化 |

fresh 证据：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 03:21:41.985`，晚于测试时间 `03:20:06.169`；Console C# error 为 **0**；fresh full `BattleRuntimeSelfCheck` result 时间 `03:22:49.668`，结果 **PASS**。本轮没有可复用的真实 Play 自动 trace 入口，因此没有重新运行真实 Play trace；防下攻与 scale 1 奔跑仍需用户手测，当前不宣称 Play Mode 验收通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT12 代码差异清单收口（2026-07-18，Play Mode 除外）

本段覆盖下方 BATTLE-AUDIT9/10/11 的历史冻结状态。按用户限定，本轮只验收脚本定义的战斗逻辑与战斗可观察契约；4 组 Naruto/武器 Play Mode 场景仍由用户自行验证，不在本轮结论内。最新证据链为：相关源码最晚 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` `2026-07-18 16:44:31.210` < Unity `Library/ScriptAssemblies/Assembly-CSharp.dll` `16:45:52.868` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `16:46:29.080` **PASS**。fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 18 warnings**。

| 项目 | 当前代码状态 | fresh 验证 |
|---|---|---|
| `FW-FLOW-01` | 已按 `GameTick.Run` 恢复普通 tick 的 `cooldown -> human input` 顺序 | `CheckFrameworkCooldownBeforeHumanInputOrder` + full self-check PASS |
| `LP-03` | typed/generic 正式投掷均不再写 Unity-only `Zz=1`，release 后保持 `Zz=0` | weapon/generic release 矩阵 + full self-check PASS |
| `LP-05` | formal release 保留 authority `TargetIdx/HolderIdx`，只清 active link 与 held slot；consume 单独写 `0/0`；force-clear 仍执行完整清理 | `CheckAudit7WeaponReleaseTickContracts`、`CheckAudit9GenericHeldReleaseTickContracts` + full self-check PASS |
| `FW-RESULT-01` | 固定 roster slot、dormant/inactive gate、`RelationTeam`（Unity 的 `Unk364` alias）和 alive bucket 已按 authority 收口 | `CheckBattleResultsSlotAndRelationContracts` + full self-check PASS |
| `UNRES.04` | N30 `triggerCode==100` 同 `Unk364` 存活角色坐标广播已落地 | `CheckAudit11N30Code100Broadcast` + full self-check PASS |
| `DATA-01A-D` | `running_speed=8`、frame cache 600、合法缺帧 `EmptyFrame`/authored gate、cpoint action alias 已落地 | `CheckDataDefaultsFrameCacheAndCpointAliases` 及 authored-frame 回归矩阵 + full self-check PASS |

重新核销后，`FW-FLOW-02` 在 Unity 生产代码中没有 writer，权威 writer 仅位于 Host 单步/调试控制，归为 **dormant / scope-excluded**。`FW-BOOT-01` 对 `Unk344/HolderCopy` 的写入只存在于 authority `resultStartRematch` 分支；`FW-BOOT-02` 的普通非-rematch 路径在 reset 后 `HpMax/Hp3=500`、difficulty bonus clamp、PP/respawn/input/Cd/速度字段上与 Unity 现状等价，唯一 `PP=200` 同属 rematch；两项均从正式普通战斗差异中移除。`FW-RESET-01/DEP.RNG.01` 保留为批准的 per-`SimulationWorld` lockstep RNG adapter，不要求改成 authority 的进程静态 owner。

本段只证明上述代码差异已写入且进入 fresh full self-check。它不证明 4 组 Play Mode 场景，也不构成任意角色、任意 DAT、长时间对局的完整逐帧 production certificate。

## BATTLE-AUDIT11 12 项代码核验全部定性（2026-07-18，仅代码层）

本轮完成原 `authority-unresolved` 清单的代码层核验，并已落地对应的 Unity 代码修复。范围严格限定为 C# 权威源码与 Unity 脚本调用链、字段契约、默认值、重置时机和可达分支；不包含 Play Mode、资源部署、DAT 文件表示差异或非脚本表现确认。当前 fresh Unity full self-check 仍为 **FAIL**：2026-07-18 最新 fresh run 的 `CheckStateTransformLandingMatrix` transform fixture 断言失败，实际为 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`；这是既有 transformed landing fixture/代码契约回归，不是 Play Mode 结论。因此生产修复状态为“已落地 / 编译通过 / self-check 阻塞”，不能宣称已对齐。依据报告：

- `.omc/research/final-verify-unres-02-05-code-parity-20260718.md`
- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

代码层结论：

| 分类 | 项目 |
|---|---|
| **equivalent / Unity-adapter** | `UNRES.01`、`UNRES.02`、`UNRES.03`、`UNRES.05`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01` |
| **confirmed code difference** | `UNRES.04`、`DATA-01A`、`DATA-01B`、`DATA-01C`、`DATA-01D` |
| **Unity-adapter / policy-open** | `DEP.RNG.01`（算法等价；RNG owner/reset 边界保留为 Unity lockstep 适配策略待定） |
| **关联确认代码差异** | `FW-RESULT-01` |
| **不计入正式 runtime 差异** | `DATA-01E`（当前 consumer 已屏蔽的 adapter/masked）、`DATA-01F`（schema-only omission）、`DATA-01G`（closed in source） |

`UNRES.04` 的具体差异是：权威 `GameTick` 在 `triggerCode == 100` 的 N30 历史触发中，对同 `Unk364` 的存活角色写入 `Unk3FC/Unk400` 随机坐标；Unity 已补齐该生产广播路径，并加入对应 self-check，但当前 full self-check 被独立的 transformed landing 断言阻塞。

因此，在 **code-only scope** 下，原清单中的 `authority-unresolved` 已由 4 项（`UNRES.02`-`UNRES.05`）降为 **0 项**。这只表示代码层项目已完成定性，不表示生产差异已经全部修复，也不改变用户自行负责的 Play Mode 场景验证状态。`UNRES.04`、`DATA-01A-D` 的首轮修复已落地，但 fresh full self-check 仍被上述 transformed landing fixture 回归阻塞；`FW-RESULT-01` 仍是确认差异，`DEP.RNG.01` 保留为 Unity-adapter/policy-open。本段不构成“完整战斗逻辑已对齐”声明。

## BATTLE-AUDIT10 代码核验（2026-07-18，仅代码层）

本段为 BATTLE-AUDIT10 历史核验快照：当时按用户限定只核验脚本/代码层面的 authority-unresolved 项，未进行 Play Mode、资源部署或场景/表现验证，也未修改生产代码；后续生产修复与当前阻塞状态以 BATTLE-AUDIT11 为准。核验依据为以下三份只读报告：

- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

核验结论：

- `UNRES.01`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01`：代码层已闭合为 **equivalent / Unity-adapter**，从 authority-unresolved 清单移出。
- `DEP.RNG.01`：LCG 算法和单次取值算术与权威一致；owner/lifetime 与 reset/seed 边界属于 Unity lockstep adapter 的策略选择，当前标为 **Unity-adapter / policy-open**，不作为待修复生产差异。
- `DEP.DATA.01`：拆分为 `DATA-01A`/`DATA-01B`/`DATA-01C`/`DATA-01D` 四项 **confirmed code difference**；`DATA-01E` 为当前 consumer 已屏蔽的 **Unity-adapter / masked**；`DATA-01F` 为 **schema-only omission**；`DATA-01G` **closed in source**。
- `FW-RESULT-01`：确认代码差异，非正常 roster/lifecycle 状态下 dormant/inactive 选择及 relation identity alias 与 authority 不同；当前仍未修复、未完成运行时验证。
- `UNRES.02`-`UNRES.05`：**BATTLE-AUDIT10 的历史中间快照**曾暂列为 authority-unresolved；该状态已由 BATTLE-AUDIT11 的代码核验取代，当前 code-only scope 下已分别定性为 equivalent（02/03/05）或 confirmed difference（04）。

本段为 **BATTLE-AUDIT10 历史中间快照，已被 BATTLE-AUDIT11 取代**：当时统计为剩余 authority-unresolved 4 项（`UNRES.02`-`UNRES.05`）。BATTLE-AUDIT9 中既有 LP 项的状态与计数、4 组 Play Mode 未验证场景的计数均不因本轮代码核验改变；这些场景由用户自行验证。本段不构成“完整战斗逻辑已对齐”结论。

## BATTLE-AUDIT9 当前冻结（2026-07-18，先盘点后修复）

本段为 **BATTLE-AUDIT9 历史冻结快照，已被 BATTLE-AUDIT11 代码核验取代**。当时计数为 9 个正式 runtime 差异、1 个 parity trace 工具差异、12 个 authority-unresolved 待确认项和 4 个 Play Mode 未验证场景；其中原 12 项现已在 code-only scope 下全部定性为 equivalent/adapter 或 confirmed difference。逐项权威/Unity 方法、触发条件、预期/实际、分类和证据仍保留在本历史表中。

F1-F7 仅达到 source/static + focused self-check 闭合，尚未全部 Play Mode 复核；DAT 表示差异排除，T8 默认 `stage.dat` 部署继续暂缓，fixed-world camera 为用户批准的 Unity adapter。

### BATTLE-AUDIT9 修复进度（LP-01 / LP-02 / LP-04）

冻结清单建立后，`LP-01`、`LP-02` 与 `LP-04` 已进入“**代码已写 / self-check verified / Play-unverified**”，但仍保留在 BATTLE-AUDIT9 差异清单中，不能据此关闭整个清单。`LP-01` 的 generic held 正式 throw/kind3 释放写回已落在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs` 的 `ThrowHeldObject`、`DropRandomly` 与 `ClearLinks(..., stampReleaseTick: true)`，并由 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 的 `CheckAudit9GenericHeldReleaseTickContracts` 覆盖；`LP-02` 现为 `(ZInt, runtime slot)` dense presentation rank，落在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` 的 `GetPresentationRenderSortingOrder` 与 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` 的 `ForceRefreshPresentation`，并由 `CheckCompactPresentationRenderSorting`、`CheckLegacySpriteRendererPresentationSorting` 覆盖；`LP-04` 的实体/阴影 `HitStop` 阈值与四拍显示门控已落在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` 的 `UpdateSprite`、`ShouldDrawEntityForHitStop`、`ShouldDrawShadowForHitStop`，并由 `CheckHitStopPresentationGates` 覆盖。

本批验证证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 warnings**；fresh Unity full `BattleRuntimeSelfCheck` 于 `2026-07-18 14:01:51.078` 返回 **PASS**，`Assembly-CSharp.dll` 时间 `14:01:27.540` 晚于本轮最新相关源码。该证据关闭三项的编译与 self-check 层级；generic held 实际投掷/掉落、同 Z slot 排序的画面顺序和负 `HitStop` 实体/阴影闪烁仍需 Play Mode 定向验证。

`LP-05`（新增 reviewer 候选，只记录、不修复）：权威 `BattleCore/Interaction/WeaponRuntime.cs:289-295` `ReleaseHeldWeaponRuntime` 只清双方 `LinkState`、写 `ReleaseTick` 并清 held slot，不写 `holder.TargetIdx` 或 `held.HolderIdx`；Unity `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs:23-28,39-59` 当前正式 release 会清 holder `TargetSlotIndex` 并将 held `HolderStableId` 置 `-1`，generic `ClearLinks` 也有同类清理。当前分类为 **confirmed-candidate / 未修复 / 需 authority 调用链与 Play Mode 复核**；它不纳入 `LP-01` 的已写/self-check 结论，也不改动上述冻结计数。

## BATTLE-AUDIT8 当前进度（历史交接，已由上方冻结覆盖）

本轮 BATTLE-AUDIT7 生产修复及新增断言已经进入一次 **fresh Unity full `BattleRuntimeSelfCheck` PASS**：`BattleRuntimeSelfCheck.cs` source `2026-07-18 12:45:10.110` < `Assembly-CSharp.dll` `12:46:15.927` < result `12:46:40.638` **PASS**。其中 F6/R1 的正式 Unity 输入路径已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs` 的 `UpdateLocalInputStateFromControllerBuffer` 中先执行 `InputState.SyncFromRuntime(Runtime)`，再轮询 controller buffer；它对应权威 `BattleCore/Input/InputRuntime.cs` 的 human poll/cooldown 单一 runtime 真值，并避免 results-active human observation 使用滞后的 `CdDefendLock`。results early-return 的 pass 边界仍以权威 `BattleCore/Simulation/GameTick.cs` 对照 Unity `NTSDBattleTickSystem.RunReleaseTick`。

为使 `CheckAudit7AppManagerSpawnContract` 能在 EditMode 生命周期下运行，`Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs` 的 `TemporaryAppManagerRuntimeScope` 补齐了测试 fixture 的 singleton/Awake 初始化与清理。这一项**仅是 self-check helper 修复**，没有修改生产 `Assets/NTSD/Scripts/App/AppManager.cs`，不能记作新的生产行为差异或 AppManager 生产修复。

Frame/Input 权威账的 **39 equivalent、181 Unity-adapter、4 confirmed-difference、1 missing、12 authority-unresolved** 计数属于 **BATTLE-AUDIT8 历史静态快照**；完整证据见 `.omc/research/unity-frame-input-mapping-complete-20260718.md`。BATTLE-AUDIT11 已完成原 authority-unresolved 项的代码层定性，当前 code-only scope 为 0；Play Mode 与非脚本表现仍按用户范围处理。

**验收边界**：本次 PASS 只证明当前 fresh DLL 中已被 `BattleRuntimeSelfCheck` 覆盖的断言全部通过，不等于全部战斗逻辑已完成逐帧最终对齐，也不替代必要的目标 Play Mode/双端 trace。DAT 文件表示差异继续不处理；T8 默认 `stage.dat` 部署继续按用户要求暂缓，stage runtime 只使用内存 fixture 验证。

> **当前结论（BATTLE-AUDIT7，2026-07-18）**：撤销此前任何“完整战斗逻辑已对齐”“无剩余差异”的推断。重新以唯一权威 C# 的完整框架、字段和正式调用链做正向映射及 Unity-only 反向审计后，现有静态证据确认 **13 个去重开放根因**：其中 **12 个战斗 runtime/语义根因**、**1 个 parity trace 投影工具根因**。它们均为“已确认、未修复、未运行时验证”。Audit5 的 **74/74** 与原 trace 风险 **15/15** 仍是对应历史批次的真实关闭记录，但不覆盖本轮新发现；`2026-07-18 01:07:52.834 PASS` 与当时 Architect `P0/P1/P2=0` 同样只覆盖当时源码和断言，不能证明 BATTLE-AUDIT7 的开放项已通过。

> 创建日期：2026-07-12
>
> **唯一 gameplay authority**：`J:\QQFile\NTSD2.4\ntsd_release_C#`。战斗规则、pass 顺序、字段副作用和可观察行为只能以该 C# 工程为准。
>
> **核心入口**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore`。旧工程、反汇编和旧对齐结论只保留为历史记录，不得作为当前实现或验收依据。
>
> **历史表说明**：下文历史表中仍可能出现旧来源坐标；这些坐标只说明当时的追踪过程。若与唯一权威 C# 冲突，必须重新按 C# 审计并更新结论。
>
> **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
>
> 说明：
> - 本文只覆盖**战斗相关逻辑**：固定 tick/pass 顺序、输入与 AI、帧推进/状态、实体位移与逻辑 X 边界、碰撞/命中、武器/cpoint/opoint、死亡复活、波次和实体生命周期。菜单、选人、加载、HUD/结算、相机、背景/纯渲染、音频播放系统、网络、回放/回滚基础设施不在本清单内。
> - bg.dat 的 Z 可活动范围与相机/背景表现不对齐，Unity 保留自己的 BoundaryWall + ProCamera2D；但 `ApplyPreframeBounds` 中会改变实体存亡或 X 坐标的逻辑分支仍属于战斗逻辑，不能随表现层一起排除。
> - "冗余脚本可删除"的判定必须严格：**只有在 C# 无对应分支、且 Unity 自身也不引用时才可删**；若只是 Unity 换了一种架构实现同一件事（组合/resolver/partial），**不算冗余，不得删除**。
> - **最终表现效果一致原则（重要）**：对于因 Unity 框架/架构限制而**无法做到逻辑层完全对齐**的项，退而求其次的底线是——**运行时最终表现效果必须与 C# 工程完全一致**（位置、帧号、速度、判定结果、伤害数值、时序等对外可观测行为逐帧等价）。即"实现方式可不同，但结果必须等价"。凡标 🔷 的项，验收标准就是这条：不比对代码是否同构，而比对运行结果是否逐帧一致。
> - 标记含义：✅ 已对齐 / ⚠️ 部分对齐或存疑 / ❌ 缺失或明显偏差 / 🔷 架构不同但结果需等价 / 🗑️ 疑似可删（需二次确认）
> - **历史批次口径（Audit5 + Audit6）**：Audit4 只保留为已完成的定向回归基线。Audit5 原始总账保持 74 个确认差异簇 + 15 个 trace 风险；对应批次曾达到 **74/74 逻辑实现 + focused/full self-check**，原 15 项风险为 **15/15 已关闭**。`BATTLE-AUDIT6-01/02`、CP-NV1/2/3、STEP10 与原 3 个受控 P2 也曾完成生产修复和验证。该历史 freshness 为 source `2026-07-18 01:06:21.499` < Unity DLL `01:07:21.125` < result `01:07:52.834`，结果 **PASS**，Architect 当时复核 `P0/P1/P2=0`。这些结论不覆盖 BATTLE-AUDIT7 新账，不能再据此宣称当前完整对齐。C# 与 Unity 的 raw DAT/manifest 差异仍属于读取方式和 Unity 适配的预期表示差异，不是阻塞或 backlog；T8 默认 `stage.dat` 部署继续暂缓。

---

## 第六次权威框架全量映射审计（BATTLE-AUDIT7，2026-07-18，开放）

### 审计方法、集合覆盖与结论边界

本轮不要求把 Unity 改写成与权威工程同形的纯 C#。`MonoBehaviour`、`GameObject`、`Transform`、对象池、渲染帧和 Unity 生命周期可以保留为适配层；判差异的标准是它们是否改变权威 C# 的 pass 顺序、字段真值、RNG 消耗、对象生命周期或逐 tick 可观察结果。

| 分区 | 正向权威账 | 当前核销结论 | 反向 Unity-only 结论 |
|---|---:|---|---|
| Framework / bootstrap / world / tick shell | **172/172 ID** | 独立复核后为 64 equivalent、57 Unity-adapter、13 confirmed-difference ID、8 authority-unresolved、30 scope-excluded；13 个 difference ID 去重为 7 根因 | 已扫描生产 framework 路径；未发现 F1-F7 之外的新 framework 根因。scene walkability 当前无 mechanics reader，属 dormant adapter；fixed camera 是用户指定适配 |
| Frame / input / physics / runtime | **237/237 ID** | **[历史 BA7 快照，已由 BATTLE-AUDIT8 237/237 分类取代；当前开放项见顶部]**：当时校正后为 4 个 difference ID + 1 个 missing ID，`IN.JUMP.03` 为 equivalent，另有 219 个 ID 尚未逐项拆分；该旧静态边界不再作为当前状态依据 | 正式可达新增是 results 普通 pass 与 late held 逻辑重同步；`SimEntityCollision`、6 个 `Suppress*UntilTick` 无生产效果；input event queue 是 adapter；duplicate player slot 仅 public contract 可构造、正式 provider 不可达 |
| Interaction / collision / hit / cpoint / weapon / opoint / stage | **105/105 ID** | 集合相等只证明 ID 存在；独立复核确认 2 个正式可达差异。原“0 difference / 212 semantic identities 完整闭合”不可作为 parity 结论 | 直接音频播放为表现 adapter；F8、step-wait、Mode2 debug 为 scope-excluded；未发现下列 I1-I2 之外的新普通战斗状态根因 |

Frame/input 权威 ledger 另有两处已校正的账本问题。第一，按分组显式字段机械相加为 138，而 footer 写 137；这是计数口径不一致，不是 Unity 战斗逻辑差异。第二，ledger 曾将 `IN.JUMP.03` 记为成功 `DoFrameJump` 清 8 Cd；当前权威 `InputRuntime.cs:926-927` 与 Unity `NTSDInputStateModule.ClearActionAndDirectionCooldowns` 实际都只清 Right/Left/Up/Down/Attack/Jump/Defend 7 个普通 Cd，并都保留 `CdDefendLock`，所以 `IN.JUMP.03` 应为 equivalent，不进入差异账。`IN.CD.02` 的 AI 递减归属差异仍独立成立，故去重根因总数不变。**“后续逐项拆分 219 个 ID”是 BATTLE-AUDIT7 历史门槛，已由 BATTLE-AUDIT8 的 237/237 完整分类取代。**

权威总账与复核证据：

- `.omc/research/csharp-authority-framework-ledger-20260718.md`
- `.omc/research/verify-unity-framework-mapping-20260718.md`
- `.omc/research/csharp-authority-frame-input-ledger-20260718.md`
- `.omc/research/unity-frame-input-mapping-ledger-20260718.md`
- `.omc/research/csharp-authority-interaction-ledger-20260718.md`
- `.omc/research/verify-unity-interaction-mapping-20260718.md`

### BATTLE-AUDIT7 去重开放总账

状态统一为 **已确认 / 未修复 / 未运行时验证**。表中的“运行时验证”指修复后 fresh Unity 编译、focused/full `BattleRuntimeSelfCheck` 和必要 Play Mode；旧 PASS 不计入。

| 编号 | 根因与关联 ID | 权威 C# 文件/方法 | Unity 对应 | 前置/输入、预期与实际 | 依赖与状态 |
|---|---|---|---|---|---|
| BA7-F1 | 首波 `WaveIdx` 被 Unity 提前推进；`FW-BS-008`，关联 `FW-LC-004` | `DirectBattleBootstrap.InitializeFromConfig` 写 `WaveIdx=-1`；`GameTick.ApplyCurrentWavePhaseAdvance` 在 `<0` 时返回 | `SimulationTickDriver.ApplyMatchConfig` 调 `StartInitialStageWave`，后者 `-1 -> 0` | 前置：加载任意内存 stage campaign。预期：bootstrap 后仍为 -1；实际：Unity 进入 wave 0 | 不部署默认 `stage.dat`，用内存 fixture；**已确认 / 未修复 / 未运行时验证** |
| BA7-F2 | 8-slot roster 被压缩且 independent team 未规范化；`FW-BS-008-B1` | `DirectBattleBootstrap.InitializeFromConfig` 保留原 0..7 index，team 0 规范为 `10+index` | `BattleRuntimeState.ApplyMatchConfig` 跳过 inactive 并按 `writeIndex` 连续写；`AppManager` 实体 team 与 roster raw `-1` 不一致 | 前置：8-slot 中间有洞并含 independent team。预期：原 slot、规范 team、实体 binding/human poll 一致；实际：slot 压缩且正常 unbound roster match 失败 | 依赖 bootstrap fixture 与 input binding；**已确认 / 未修复 / 未运行时验证** |
| BA7-F3 | 初始出生位置与 RNG 消耗不同；`FW-BS-008-B2` | `DirectBattleBootstrap.InitializeFromConfig` 每个有效角色按 bounds 消耗两次 `Rand` 写 X/Z | `AppManager` 使用 scene spawn transform，不消耗 battle RNG，并写入 `PS.x/z` 逻辑真值 | 前置：同 seed、同 bounds、至少一名有效角色。预期：权威 X/Z 和精确 RNG call count；实际：scene 坐标且下游 RNG 序列偏移 | fixed camera 不影响此结论；需先统一 bootstrap RNG；**已确认 / 未修复 / 未运行时验证** |
| BA7-F4 | 初始 `HitStop`/velocity prime 缺失；`FW-BS-009` | `DirectBattleBootstrap.InitializeBattleStats` 写 `HitStop=75,Vx=Vz=0.1,Vy=0` | `NTSDEntityRuntime.Reset` 为 0，`LF2Character.Initialize` 未补 prime | 前置：正常角色出生。预期：首 tick 前为 75/0.1/0/0.1；实际：0/0/0/0，普通命中 gate 和首段运动可变 | 与 F3 同属 bootstrap fixture，但独立根因；**已确认 / 未修复 / 未运行时验证** |
| BA7-F5 | stage spawn 误清复用槽 ARest/VRest；`FW-WR-005`,`FW-TK-028`,`FW-H-050`,`FW-H-059`,`FW-LC-004` | `SimulationWorld.Registry.SpawnAt` 与 stage spawn 不调用 `ResetCooldowns` | `SimulationWorld.Registry.Register` 对所有注册实体无条件清 rest；stage spawn 走该通路 | 前置：later stage spawn 复用带非零 ARest/VRest 的槽。预期：rest 行列保留；实际：被清零 | 需按 spawn semantic 拆 rest policy；默认 `stage.dat` 不需要；**已确认 / 未修复 / 未运行时验证** |
| BA7-F6 | Results active 后缺少权威 early return；`FW-TK-002`,`FW-END-002`,`FLOW.05` | `GameTick.Run` tick header/瞬态清理后，`Results.IsActive` 时只 `RunResultsTick` 并 return | `NTSDBattleTickSystem.RunReleaseTick` 无 results gate，继续 cooldown/input/frame/collision/stage/late/tail | 前置：`BattleResultsFlowAll` 激活 summary 后再跑 1 tick。预期：只推进 header/results；实际：普通战斗状态继续变化 | framework/frame 重复发现只计一次；不要求实现菜单；**已确认 / 未修复 / 未运行时验证** |
| BA7-F7 | hit candidate 瞬态载体晚一 tick 清理；`FW-TK-034`,`FW-H-042` | `GameTick.RunEntityPostframeTail -> ClearHitCandidateCarriers` 当 tick 清 `HitConfirm2` 等 scratch | `EntityPostFrameTailAll` 不清；`BruteForceSceneQuery` 下次 collect 才清 `HitConfirm2` | 前置：weapon/special hit 设置 `HitConfirm2`。预期：interaction 内可见、post-tail/checksum 前归零；实际：跨 tick 残留 | 当前证明 runtime/checksum 差异，未证明招式结果变化；**已确认 / 未修复 / 未运行时验证** |
| BA7-R1 | `CdDefendLock` 递减归属不一致；仅 `IN.CD.02` | `InputRuntime.PollHumanInput -> NtsdEntityInputRuntime.TickCooldowns` 只为 human 递减 8 Cd；AI `PrepareBasic` 不走该 cooldown tick | `VrestTickAll -> TickDefendLockCooldown` 对所有 active entity 递减 | 前置：human/AI 各 `lock=3`。预期：human poll 递减，AI 不递减；实际：Unity human/AI 都在 Vrest pass 递减 | `IN.JUMP.03` 双方均清 7 个普通 Cd、保留 lock，已从差异账删除；ownership 根因 **已确认 / 未修复 / 未运行时验证** |
| BA7-R2 | late holder 切帧后额外重同步 held 逻辑真值；`FLOW.09` | `GameTick.Run` 早期 `SyncHeldWeapons` 后，late `RunLatePerEntityUpdatePass` 不再执行 held sync | `SimulationWorld.LateEntityUpdateAll -> SyncHeldPoseAfterLateHolderFrameChange` 再写 held Frame/Facing/FrameDelay/X/Y/Z/Zz | 前置：holder 在 late `SimFrameTick` 改帧且持有对象。预期：held 本 tick 保持早期同步值，下一 tick 再同步；实际：Unity 当 tick 二次改逻辑帧/位置 | 表现 renderer 可同 tick 刷新，但不得反写逻辑；**已确认 / 未修复 / 未运行时验证** |
| BA7-R3 | `ReleaseTick` runtime 字段和写回缺失；`RT.LINKS.01` | `NtsdEntityRuntime.Links.ReleaseTick` reset/copy/hash；`WeaponRuntime` 两条释放路径写 current tick | Unity 无对应 storage/writer，`BattleParitySnapshot` 固定投影 `-1` | 前置：普通 drop 与 consume release。预期：两路径写当前 tick并进入 hash；实际：字段不存在且永远 -1 | 需 storage、reset、writer、snapshot/hash 一体落地；**已确认 / 未修复 / 未运行时验证** |
| BA7-I1 | IronBall 预处理类型 gate 错把 type6 当 type2；`INT-HIT-005` | `HitResolve.PreprocessCandidate` 仅在 `ObjType == IronBall(2)` 时将 itr `Dvx/Dvy` 各减半 | Unity shared hit preprocess 检查 `Drink/FlyingB(6)` | 前置：相同 itr 分别命中 DAT type2 与 type6 victim。预期：仅 type2 缩放；实际：type2 不缩放、type6 错缩放 | 需 real/shared 生产路径矩阵；**已确认 / 未修复 / 未运行时验证** |
| BA7-I2 | late opoint 使用 spawner 浮点 X/Y，而非权威整数坐标；`INT-OP-001`,`INT-OP-002` | `FrameTick.SpawnFromOpoint` 从 spawner `XInt/YInt` 计算 child，并同时写 child 浮点/整数 X/Y；Z 保留浮点 `spawner.Z+1` | `LF2ObjectPointFactory.ProcessOpointSpawnAlignedToCpp` 从 `PS.x/PS.y` 构造 task，未启用 direct runtime integer position；weapon/special 复制浮点 | 前置：正、负及跨零的小数 spawner 坐标。预期：child X/Y 与 XInt/YInt 均按整数源，Z 保持浮点；实际：child 继承小数并影响 next-tick physics | 需覆盖 weapon/special、slot、velocity 与下一 tick snapshot；**已确认 / 未修复 / 未运行时验证** |
| BA7-T1 | parity snapshot 错投影已有 runtime 字段；`RT.CHECK.01` | `NtsdEntityRuntime` 默认值与 `CharacterSync` hash 投影真实 `ReleaseTick`、Block、`Unk318/31C/324/33C`、owner/关系字段 | `BattleParitySnapshot.ProjectEntityRuntime` 硬编码/错映射空槽 category、release、block、transform/weapon 字段、grabbed/owner/`Unk364` | 前置：default 400 slots、bounds block、transform、weapon、release 场景。预期：snapshot 投影实际 runtime；实际：hash 可假相等或假不同 | 这是验证工具根因，不计入 12 个战斗 runtime 根因；依赖 R3 的 `ReleaseTick`；**已确认 / 未修复 / 未运行时验证** |

### 保留适配、排除项与验收门槛

- DAT 文件及 raw manifest 不要求相同；两端读取方式和 Unity DAT 适配差异不处理，也不作为 BATTLE-AUDIT7 backlog。
- T8 默认 `stage.dat` 资产部署继续暂缓；BA7-F1/F5 使用内存 stage fixture 验证 runtime，不读取、生成或私自部署默认资产。
- fixed-world camera 是用户指定 Unity 适配；Unity-native `Transform`、GameObject/CLR 壳、对象池、渲染刷新和生命周期接入继续保留，前提是不改变逻辑真值。
- F1/F8、step-wait、Mode2 debug、results 菜单交互、普通 HUD、完整 rollback/host rematch 继续排除；results active 后停止普通战斗 pass 仍属于战斗 runtime。
- **BATTLE-AUDIT7/8 历史验收门槛**曾记录 4 个 confirmed-difference、1 个 missing 和 12 个 authority-unresolved；这些是历史中间状态，已由 BATTLE-AUDIT11 的 code-only 定性取代。当前仍不得恢复“完整战斗逻辑已对齐”的结论，原因是确认的代码差异尚未全部修复，且 Play Mode 场景由用户自行验证。

---

## 0. 权威工程 BattleCore 结构 → Unity 映射总表

| C# BattleCore 文件 | 职责 | Unity 对应 | 映射类型 |
|--------------------|------|-----------|---------|
| `Simulation/GameTick.cs` | 单 tick 总调度（顺序主干） | `Simulation/NTSDBattleTickSystem.cs` + `SimulationWorld.Passes.partial.cs` | 🔷 pass 拆分 |
| `Simulation/NtsdBattleTickSystem.cs` | tick 外层入口 | `Simulation/NTSDBattleTickSystem.cs` | ✅ |
| `Simulation/SimulationWorld.cs` | 世界容器/对象池 | `Simulation/SimulationWorld*.cs` | 🔷 固定槽 vs 动态槽 |
| `Frame/FrameTick.cs` | frame_tick 帧推进 | `Character/FrameTransistor.cs` + `LF2Entity.RunCommonFrameTick` | 🔷 |
| `Frame/FrameAdvance.cs` / `Physics.cs` | 帧推进物理 | `Character/CharacterMechanics.cs` + `PhysicsState` | 🔷 |
| `Interaction/HitResolve.cs` | 命中结算（kind 0~16） | `LF2CharacterHitResolver.cs` + `LF2Weapon.ApplyHitEffects` + `LF2CharacterDatHitResolver` | 🔷 分散到多类 |
| `Interaction/CollisionCollect.cs` | 候选收集 | `Character/BruteForceSceneQuery.cs` | 🔷 |
| `Interaction/CPointRuntime.cs` | 抓取 cpoint | `LF2CharacterCatchResolver.cs` + `PreInteractionTickAll` | 🔷 |
| `Interaction/WeaponRuntime.cs` | 持武器同步/投掷/掉落 | `LF2WeaponHeldStateResolver.cs` + `LF2WeaponReleaseFlowResolver.cs` | 🔷 |
| `Interaction/ObjectPointFactory.cs` (`FrameTick.SpawnFromOpoint`) | opoint 生成 | `Character/LF2ObjectPointFactory.cs` | ✅ Naruto DDJ 生命周期差异修复后已验证 |
| `Input/InputRuntime.cs` | 输入消费 + AI | `Input/CharacterInputModule.cs` + `LF2Entity` shared-DAT 桥 | 🔷 |
| `Entity/Entity.cs` (大字段实体) | 实体真值 | `NTSDEntityRuntime.cs` + `LF2Entity` | 🔷 字段化 |
| `Entity/NtsdCharacter/NtsdWeapon/...` | 实体类别 | `LF2Character/LF2Weapon/LF2SpecialAttack/LF2OtherObject` | 🔷 |

---

## 1. Tick 主循环顺序（C# authority vs Unity pass）

C# `GameTick.Run` 是唯一正式顺序。Unity 拆成 `NTSDBattleTickSystem` 调度多个 `SimulationWorld` pass，两侧顺序必须逐段等价。

| # | C# 正式顺序 | Unity pass | 状态 |
|---|------------------------|-----------|------|
| 1 | `GameTick++` / `InputPhase` / `FrameMod12` / `FrameToggle` | `NTSDBattleTickSystem` + `BattleRuntimeState.Flow` | ✅ `AdvanceBattleFlowTick` 在 tick 头统一推进四项；state 400/401 读取持久化 `FrameToggle` |
| 2 | 清瞬时状态 `PendingSounds.Clear()` 等 | 战斗候选载体在 `EntityPostFrameTailAll` 清理 | 🔷 音频/overlay 瞬时状态排除；战斗候选清理已存在，仍随碰撞快照专项验收 |
| 3 | `RunCooldownsTick`（arest-- + attack_exempt 清理） | `VrestTickAll` + `ClearAttackExemptIfCurrentFrameCannotHit` | 🔷 |
| 4 | `GameTick.Run:61-62` `postCooldownInput` callback | `PostCooldownHumanInputAll` → `AiInputAndComboAll` | ⚠️ 历史自检曾通过；当前必须以 C# callback 契约重新核验 |
| 5 | `GameTick.Run:63-64` `RunOid5152RuntimeMaintenance`（实现见 `:1093-1263`） | `Oid5152RuntimeMaintenanceAll` + `TryMergeOid7Or8Into51` / `TrySplitOid51BackToPair` | ✅ Audit6 已按唯一 C# 权威重审并经 fresh full self-check |
| 6 | `GameTick.Run:75-78` `ApplyCharacterInputPass` | Unity 输入 pass 拆分 | 🔷 以 C# 正式调用顺序判定等价性 |
| 7 | `RunEarlyStatePasses`（400/401/500/501） | `EarlyFrameAdvanceSpecialsAll` | ✅ 含 BMD-023 修复 |
| 8 | `FrameRuntimePasses.RunFrameLogic`（hit_fa>0 非角色） | `FrameLogicBeforeAdvanceAll` | ✅ |
| 9 | `RunFrameAdvance`（所有 active，清方向键 + 帧推进） | `SerialTickAll`（SimTransit+SimTU） | 🔷 |
| 10 | `RunPostFrameAdvanceStatePasses`（9998 清理 + 复活） | `CleanupState9998Entities` + `PostFrameAdvanceDeathCleanupAll` + `RunReleaseEntityCleanupTail` | ✅ 复活由 T5 完成并通过运行时自检 |
| 11 | `ClampCharactersToStageZ` | (Z 边界，属可活动范围) | 🚫 不对齐 |
| 12 | `RunCPoint` | `PreInteractionTickAll`→`RunCpointCheckStep10` | ✅ |
| 13 | `SyncHeldWeapons` | `RunWeaponSyncHeldStep10` | ✅ |
| 14 | `ValidatePositiveLinks` | `ValidateHeldLinksAll` | ✅ 全局扫描 active slot `0..399`；invalid 只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot`，不清 target 反向字段 |
| 15 | `RunHeldWeaponStep12` | `PreInteractionTickAll` 内 | ✅ |
| 16 | `SnapshotPrevFrame2` | `CaptureCollisionFrameSnapshotsAll` | ✅ |
| 17 | `CollectCandidates` | `CollectCollisionCandidatesAll` | ✅ |
| 18 | `ResolveCharacterHits` | `PostInteractionTickAll`（角色候选消费） | 🔷 |
| 19 | `RunNaturalRandomWeaponDrop` | `RandomWeaponDropTickAll` | ✅ |
| 20 | `RunF8WeaponDrop` | **未找到 F8 路径** | 🗑️? 调试功能，见 §7 |
| 21 | `ResolveObjectHits` | `ObjectInteractionTickAll` | 🔷 |
| 22 | `ApplyPreframeBounds`（含相机/bg） | `ApplyPreFrameBoundsAll`（只做逻辑边界） | 🔷 相机部分不对齐 |
| 23 | `ApplyCurrentWavePhaseAdvance` / `StageSpawns` | `CurrentWaveStageTickAll`（`SimulationWorld.StageWave.partial.cs`） | ✅ 已完成并通过 fresh Unity 运行时验收 |
| 24 | `ApplyFramePostProcess`（HitCount→Vx 平均） | `FramePostProcessAll` | ✅ |
| 25 | `RunLatePerEntityUpdatePass` | `LateEntityUpdateAll` | ✅ 主对齐点 |
| 26 | `RunMode2RandomWeaponDrop` | `Mode2RandomWeaponDropTailAll` | 🚫 C# baseline 的 F7-F9/debug 控制路径，不作为正式战斗对齐项 |
| 27 | `RunEntityPostframeTail`（heal/catch timer） | `EntityPostFrameTailAll` | ✅ heal/catch timer 与战斗候选载体清理已落地；`InitStats`/mode2 debug 分支排除 |
| 28 | `UpdateBattleResultsFlow` | (结算流程) | 🚫 非战斗运行时范围 |

**关键差异**：
- C# 是**固定 400 槽 `Objects[]` 线性遍历**；Unity 是**动态 runtime slot + SortedDictionary bucket**。这是 🔷 架构差异，结果需等价，遍历顺序必须仍是 slot 升序。
- C# `RunLateEntityUpdate` 单函数内顺序：`RunStateSpecialPreCollision → RegeneratePreCollisionStats → FrameTickRuntime.Tick → 帧组1100/1200 → 死亡掉武器/弹地 → ProcessOpointSpawn → 破武器回收 → RunN30InputTrigger → SpawnStateTransitionEffects → PrevFrame 镜像`。Unity `LateEntityUpdateAll` 已按同序拆分（✅），但 **`RegeneratePreCollisionStats`（HP/PP 自然恢复）** 的位置需核对（见 §5）。

---

## 2. 受击/命中结算（`HitResolve.cs` vs `LF2CharacterHitResolver` + `LF2Weapon`）

C# 把**所有对象**的命中都集中在 `HitResolve.ApplyCandidate`（一个 switch(kind)）。Unity 拆成三条独立路径：
- 角色被击 → `LF2CharacterHitResolver.ResolveHit`
- 武器被击 → `LF2Weapon.Hit` / `ApplyHitEffects`
- 非角色 DAT 实体 → `LF2CharacterDatHitResolver`

这是 🔷 架构差异（合法）。以下逐 kind 核对行为是否等价。

| kind | C# `HitResolve` 分支 | Unity 分支 | 状态 |
|------|---------------------|-----------|------|
| 0/4，以及预处理后的 9→0 → 伤害 | `ApplyDamageCandidate` | `ResolveHit` 普通伤害入口；raw kind9 先由 `BruteForceSceneQuery` 转为 kind0 | ✅ alternate 路径已补齐并运行验证，见下方逐点 |
| 6 | `victim.HitConfirm=3` | `HitConfirmEa=3` return | ✅ |
| 8 | `ApplyKind8`（heal_timer/传送） | `ResolveHit` kind 8 | ✅ |
| 10/11 | `ApplyKind10Or11`（笛子）：kind==11 && weaponCount>=0 return false；WeaponCount=FluteForce 值；Falling 双倍伤害 | `LF2CharacterHitResolver.cs:357-369`（✅）+ `LF2Weapon.cs:481-501`（✅） | ✅ |
| 14 | `ApplyKind14`（方向阻挡） | `ResolveHit` kind 14 + `ApplyKind14DirectionalBlockFrom` | ✅ |
| 15 | `ApplyKind15Movement`（KnockbackVx/Vx/Vz/YInt=-2，按对象类型分 vyStep=3.0/2.3） | `LF2CharacterHitResolver.cs:373-380` 简化实现；武器侧 `LF2Weapon.cs:503-506` `WhirlwindForce` | ⚠️ 形式不同（C# 走 KnockbackVx+真实 Vx/Vz+设 YInt=-2 三段；Unity 走 PS.vx/vz 增量；C# 按对象类型分 3.0/2.3 vyStep，Unity 未区分） |
| 16 | `ApplyKind15Or16` kind=16 路径：Hp-、KillStat++、ComboCountAtk、SFX_065、frame=200、vrest 写入、LinkState 断开 | `LF2CharacterHitResolver.cs:383-390`：`ImmediateFrame(MpDrain=200)` ✅ + MaxMP 缩放伤害 ✅；**缺** KillStat++、ComboCountAtk、SFX_065 音效、vrest 写入、LinkState 断开处理 | ⚠️ |
| 1/3 | `ApplyKind1Grab`/`ApplyKind3Grab` | 走 pre-interaction（`LF2CharacterInteractionResolver`） | 🔷 时序不同，见 §4 |
| 2/7 | `ApplyPickupCandidate` | pre-interaction | 🔷 见 §4 |
| kind 4+WeaponCount>0→0 + dvx 翻转 | `PreprocessCandidate` 154-172 | `BruteForceSceneQuery.cs:602-615` 完整实现（kind 翻转 + dvx 翻转按 PS.dir） | ✅ |
| kind 5 委托攻击 | `PreprocessCandidate`（holder wpoint 替换） | `ResolveHit` kind 5（TrackerParent） | ✅ |
| oid 300 特判 | `ApplyOid300SpecialHit` | `ResolveHit` `ObjectId==300` 分支（`LF2CharacterHitResolver.cs:279`） | ✅ |

### 2.1 kind 0/4/9 伤害主流程逐点核对

C# `ApplyDamageCandidate`（character victim）关键顺序：

1. `itrArest = (itr.Arest < 4 && itr.Vrest == 0) ? 4 : itr.Arest`（`HitResolve.cs:268`） — ✅ **C# 用 Arest 判定 + 取值**
   Unity 已由 `LF2Entity.ResolveArestCooldown` 统一实现同一公式，并供普通角色命中路径复用；`CheckArestCooldownRule` 已在 Unity batchmode 中通过。
2. IronBall victim → dvx/dvy 减半（`PreprocessCandidate`）— Unity 在 `LF2Weapon` 侧，角色路径无此（正确，角色不是 IronBall）
3. alternate 受击路径 — ✅ **已完整落地并通过 Unity 运行时自检**：
   - C# `ShouldUseAlternateHurt`（629-680）→ `ApplyAlternateDamage`（实际逻辑延续到约 line 827）。Unity 以共享 `LF2AlternateDamageResolver` 承载，真实 `LF2Character.Hit` 由 `LF2CharacterHitResolver` 接入，当前 DAT 为角色但 CLR shell 非角色的对象由 `LF2CharacterDatHitResolver.TryResolveHit` 接入；两条入口调用同一 `ShouldUseAlternateHurt` / `ApplyAlternateDamage`，并各自只记录一次 `RecordKind0Hit`。
   - `ShouldUseAlternateHurt` 已覆盖 oid 37/6/52 的 `HitStateCount`/frame 窗口、heavy effect、attacker oid 214/208，以及 `PrevFrame2` state 7 的 HP、`bdefend`、朝向、负 `dvx` 和特殊攻击者判定。
   - 伤害契约为 `FallDamageDiv` 整数换算后 `reducedInjury = injury / 10`；扣 `HP`，`HPBound -= reducedInjury / 3`（整数除法），不累计 `HPLost`。致死与统计副作用使用 holder-copy 的 `KillStat`/`ComboCountAtk`、victim `ComboCountVic`，并以 `Unk344` 索引稳定 3 槽 `KillStats`/`DamageStats`；世界 reset 保持数组 identity 并清零内容。
   - 其余已覆盖 `Fall=80`、hit/attacking 计数、attacker/victim/negative-link holder 的 FrameDelay、attacker-only AttackExempt、vrest clamp、frame 111/112 保留 wait counter、ground/air knockback、state 1002/2000/3000 尾分支。state1002 随机切帧只改 frame/速度，不额外写 `Runtime.WeaponState`；状态判断继续以当前 `Frame.D.state` 为准。
   - heavy weapon 普通伤害的减半发生在 alternate 判断之后，因此 alternate 始终消费原始 itr，不会错误变成 `injury/20`。`ApplyAlternateDamage` 本身也保留 character DAT/type guard，不能被非角色 victim 直接调用。
   - **raw kind9 不直接触发 alternate**：真实角色与 shared-character-DAT 两个 caller 都以 `itr.kind != 9` 为门；raw kind9 必须先由 `BruteForceSceneQuery.ResolveRuntimeItrForPair` 转换为 kind0，才会在非 kind9 普通伤害入口判断 alternate。`LF2SpecialAttack` 也统一在 object interaction pass 使用这条预处理，覆盖 kind4 的 `WeaponCount`/反向 `dvx`（读取逻辑真值 `Dirh()`/`Runtime.Vx`）和 kind9 的 kind0 转换/攻击者 HP 清零。
   - alternate 已写入的 clamp 后 vrest 不会再被角色 DAT、武器或技能对象外层 generic rest 更新覆盖。type3（`Consumable3`/Unity `SpecialAttack`）lead sound 条件已按权威修正；该声音分支属于代码权威对齐，headless 自检无法直接观测音频播放。
   - 针对性自检：`CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess`；均包含在 2026-07-14 02:54:22 的 fresh Unity batchmode PASS 中。
4. fall 累积档位（Light/Medium/Heavy/Fall 阈值 → frame 220/222/224/226/180/186）— Unity `HitFall`/`HitFallDown` ✅ 已对齐（注意 5f/7f→0.714 修复已在）
5. `victim.HitStateCount = 45` → Unity `SetHitStateCount(45)` ✅
6. `attacker.FrameDelay=3 / victim.FrameDelay=-3` 普通路径 — Unity 多处 `-3` ✅；alternate 路径独立写 `victim.FrameDelay=-5`，并传播 negative-link holder delay ✅。
7. 攻击方攻击豁免写入 — Unity `attackerLiving?.HitCounters?.SetAttackExempt(exemptVal)` ✅（公式按点 1 修正）

### 2.2 武器被击（`LF2Weapon.ApplyHitEffects` vs `HitResolve.ApplyObjectHurtTail`）

Unity `LF2Weapon.ApplyHitEffects` 已注明"C# baseline: ApplyObjectHurtTail + ApplyStandardDamageKnockbackX"，逐段抄写。核对：
- `FallCounter += fall!=0?fall:20` ✅
- `lightThrow||heavyLike||specialLike → FallCounter=80` ✅
- ApplyStandardDamageKnockbackX 五分支（固定5 / state2000+dvx / FlyingA/B scaled / effect22/23 / 常规）✅
- knockback 帧 180/186 + KnockbackVy ✅
- 攻击者 state 1002 反弹 / state 2000 减速 / state 3000 归 frame 10 — Unity `ApplyAttackerResponse` ✅

**✅ `RecordKind0Hit` 已统一**：`LF2Entity.RecordKind0Hit` 承载 C# timer、owner、随机坐标和 10 槽上限语义，角色与 `LF2Weapon.ApplyHitEffects` 的 kind0 路径均接入；`CheckKind0HitRecords` 已在 Unity batchmode 中通过。

---

## 3. 帧推进（`FrameTick.cs` vs `FrameTransistor` + `RunCommonFrameTick`）

C# `FrameTick.Tick` 是单函数，Unity 拆成 `FrameTransistor.Trans()`（wait/next 推进）+ `LF2Entity.RunCommonFrameTick`（前置门控 + 倒计时）+ hook（`OnFrameTickBeforeWaitAdvance` / `OnFrameTickAfterWaitAdvance`）。

| C# `FrameTick.Tick` 步骤 | Unity | 状态 |
|--------------------------|-------|------|
| `ThrowFrameGuard==Frame` early return | `RunCommonFrameTick` 门控 | ⚠️ 需确认 |
| `FrameDelay!=0 && !Consumable3` return | ✅ | ✅ |
| `AttackExempt--` | ✅ | ✅ |
| `LinkState<0` return | ✅ | ✅ |
| cpoint kind==2 return | ✅ | ✅ |
| Consumable3 + hitA>0 → HP-=hitA, HP<=0 跳 hitD | `LF2Entity.RunCommonFrameTick` type3 分支 | ✅ |
| HitStop/Fall/HitStateCount/HitConfirm 倒计时 | `RunCommonFrameTick` | ✅ |
| frame!=waitCounter → 音效+attacking=0 | `FrameTransistor.Trans` frame 变化清 attacking | ✅ |
| `attacking++` | `Trans.AttackingCounter++` | ✅ |
| state 0 + YInt<0 → frame 212 + SuppressJumpInit | `OnFrameTickBeforeWaitAdvance` | ✅ BMD-023 相关 |
| IronBall state 2000 静止 return | `LF2Weapon.ApplyObjectSpecificFrameTickBeforeWaitAdvance` | ✅ |
| state 14 HP<=0 → HitStop=30 | `RunCommonFrameTick` | ✅ |
| state 2000 facing=vx | ✅ | ✅ |
| `attacking>wait` → next 换帧 | `Trans` attacking>wait | ✅ |
| next=999 → 212/0（空中角色） | `ResolveFrameTickNext999Target` | ✅ |
| next<0 翻面 | `Trans` switchDir | ✅ |
| 上一帧 state14→非13 的 HitStop=15 逻辑 | `OnFrameTickAfterWaitAdvance` | ✅ 含 oid/5==3 skip + difficulty 分支 |
| frame 212 + JumpInitPending → 跳跃初速 | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame mp<0 PP 扣费 + hitD turn | `OnFrameTickAfterWaitAdvance` | ✅ |
| frame 110/114 → CdDefendLock=3 | `RunCommonFrameTick` 尾 | ✅，`CheckFrameTickDefendLockTail` 运行通过 |
| frame 202 → HitStop=20 | ✅ | ✅ |

**结论**：帧推进主干及上述 state14、frame mp、110/114、202 尾部特判均已核实对齐（🔷 hook 拆分合法）。

**逐点核实结果（§3 全部）：**
- §3-1 state14 入口 HitStun=30 + AttackingCounter=0（KillCount>=0 OR Unk364==5 OR slot>=20）— Unity `LF2Character.cs:2205-2211` ✅ **完整对齐**
- §3-2 state14→非13 复活 HitStun=15 分支（aiControlled 检查 + Difficulty!=2 + oid/5==3 + GameMode==1/4 + Oid!=38）— Unity `LF2Character.cs:2134-2163` `ApplyCommonCaughtExitHitStop` ✅ **完整对齐**
- §3-3 frame mp turn-around（C# `HitResolve.cs:178-203`）— Unity `LF2Entity.cs:3284-3321` `ApplyCommonFrameTickPpDisplayPostAdvance` ✅ **完整对齐**（含 PP 扣费、frame.hitD turn、Dual KeyLeft/Right + Facing + YInt==0 条件）
- §3-4 frame==202 → HitStun=20 — Unity `LF2Entity.cs:3634-3635` ✅
- §3-5 frame==110 || frame==114 → `CdDefendLock=3` — Unity `LF2Entity.RunCommonFrameTick` 尾部已实现，runtime Reset/cooldown 衰减已承载；`CheckFrameTickDefendLockTail` 已运行通过 ✅

---

## 4. 交互（pre/post-interaction, cpoint 抓取, opoint）

### 4.1 命中候选消费时序差异（重要）

C# 在 `HitResolve.ApplyCandidate` 里**同一个 switch** 同时处理攻击(0/4/9)、抓取(1/3)、拾取(2/7)。Unity 分成两个阶段：
- `PostInteractionTickAll` → 角色候选消费（攻击 + pre-interaction 混合，`LF2CharacterInteractionResolver.TryConsumeUnifiedStep7CandidateSequence`）
- `ObjectInteractionTickAll` → 武器/技能候选消费

🔷 这是合法架构差异，但 **候选序列消费顺序必须与 C# 一致**（按 step6 收集顺序）。Unity 已用 `TryGetCollisionCandidateSequence` 保序 ✅。

### 4.2 抓取 cpoint

| C# | Unity | 状态 |
|----|-------|------|
| `ApplyKind1Grab`/`ApplyKind3Grab`（命中即建立） | `HandlePreInteractionKind`（pre-interaction 建立） | 🔷 时序不同 |
| `AlignGrabPair`（对位公式 centerx/wact/lerp） | `ApplyImmediateCatchPairState`（同公式） | ✅ 公式一致 |
| `CPointRuntime.Run`（step10 维护） | `RunCpointCheckStep10` + `RunCpointMismatchTailStep10` | ✅ |
| cpoint kind==2 受击 fronthurtact/backhurtact | `ApplyCaughtVictimHurtFrame` / `TryCaughtA` | ✅ |
| throwvx/vy/vz 投掷 + throwinjury | `LF2CharacterCatchResolver`（自检覆盖） | ✅ 有 BattleRuntimeSelfCheck |

### 4.3 opoint 生成 — ✅ 已在 `skill_release_flow_comparison.md` 验证一致

`FrameTick.ProcessOpointSpawn` / `SpawnFromOpoint` vs `LF2ObjectPointFactory.ProcessOpointSpawn` / `ProcessOneLateOpoint`：条件（kind>0 && oid>0 && attacking==0 && (角色→FrameDelay==0)）、facing 展开（>10 → count/facing）、多发 AttackExempt+VRest 扩散、state 3003 linked slot vrest — 均已对齐。

**2026-07-16 Naruto DDJ 完整链专项回归：** 既有 combo wrapper 测试只能证明输入可跳到技能起始帧，不能证明递归 opoint 和对象池生命周期正确。本次按真实 DAT/authority 链新增端到端断言：

- 同 tick held chord 内部输入 `att + down + def` 命中 Naruto frame `271`；随后 frame `272` 生成 oid205/action98，辅助链继续经过 99/325/341；frame `273` 生成 oid204/action130，并展开六个分支，最终各自到 frame `147` 生成 `6 x oid33/action307`。clone 从 307 后落地进入 frame `219` 是 authority 行为，不应把 219 误判为生成失败。
- 新确认差异 1：`LF2ReferencePool.Release` 无条件接收外部 synthetic 实例，造成逻辑池类型污染。
- 新确认差异 2：factory 角色 opoint 在 `ModuleBind` 注册前用 `slot < 0` 过早拒绝，合法生成会被提前丢弃。
- 新确认差异 3：tick 中 pending-unregister 对象同 tick 归池复用时，旧生命周期仍留在 registry bucket；后续 `Register` 被旧 bucket 的 `Contains` 拒绝，递归六分支只生成 3 个 clone。
- 新确认差异 4：池化 `LF2Character.Init` 没有重新分配 `StableId`，复用角色无法保持独立生命周期身份。
- 新确认差异 5：`SpawnFromOpoint` 缺少 `RelationTeam`、`Unk364` 与 holder-copy 继承，生成角色的关系字段与 authority 不完整。
- 已修复契约：`Release` 只把 active 实例归池；`Register` 先 finalize 旧 pending lifecycle；`slot < 0` guard 移到 `ModuleBind + Initialize` 之后；character `Init` 重新 `AllocateStableId`；`PostInitLiving` 继承 `Team`、`RelationTeam` 与 holder-copy（含 `Unk364`）。
- 回归结果：PP `500 -> 295`，所有生成对象使用 dynamic slot，6 个 clone 拥有 6 个唯一 `StableId`，均实际到达 action/frame `307`，且 6 个 renderer 均可见。

**真实 Unity Play Mode 生产输入链验收：** 在 `NTSD_Battle` Play Mode 中等待 slot0 的 `CharacterInputModule`/`ActionMap` 就绪，再通过 UnityMCP 临时 `InputSystem.Keyboard` 事件按默认物理绑定依次注入 `L (Defend) -> S (Down) -> K (Jump)`。事件真实经过 `InputActionMap -> CharacterInputModule -> SimInputBuffer`，没有直接调用技能、写帧或调用 opoint。观测日志为 `INPUT focused=True buffered=1, attackAction=0, jumpAction=1, defendAction=1, moveY=-1`；这里的 crossed internal mapping 是项目/C# baseline 的预期映射，不是错误。运行结果：

- `frame271=True`，`max204=11`，`max205=3`，`maxClones=6`，`maxSpriteReady=6`，`maxVisible=6`。
- clone 数量时间线：`t=0.446: 3`、`t=0.473: 4`、`t=0.509: 5`、`t=0.541: 6`；测试窗口无异常。
- 峰值截图：`Temp/naruto-ddj-unitymcp-peak.png`。
- 验收限制：Win32 `keybd_event` 不被 Unity RawInput 接收，因此本次不是物理硬件键盘证明；它证明的是 UnityMCP `InputSystem.Keyboard` 事件通过完整生产输入链可以稳定释放真实六分身技能。

---

## 5. HP/PP 自然恢复 + heal/catch timer

**✅ HP/PP 自然恢复语义对齐**（逐字段核实）：
- C# `RegeneratePreCollisionStats`（`GameTick.cs:1474-1519`） vs Unity `LF2Character.cs:2534-2584`：
  - HP `Hp < HpMax`（HP < HPBound）每12tick+1 ✅
  - `hpForRate = Hp; >500 → 500; oid 51/52 /=2; PP += (500-hpForRate)/100+1` ✅
  - `WeaponCount<0` 每12tick 扣血（injury=900/FallDamageDiv）✅，HP -= injury、HPBound -= injury/3、ComboCountVic += 9 ✅
- 字段映射：`HpMax`↔`HPBound`、`Pp`↔`PP`，通过 `Runtime.HpMax` / `Health.HPBound` / `Runtime.Pp` / `Health.PP` 字段映射。
- 调用入口：Unity `RunPreCollisionRecoveryPhase` 虚函数（`LF2Entity.cs:972` + `LF2Character.cs:2619-2622`），由 `SimulationWorld.Passes.partial.cs:264` 调用。✅

**heal/catch timer（C# `RunEntityPostframeTail`）**：Unity `EntityPostFrameTailAll` 覆盖 HealTimer/CatchTimer/state1700 ✅（之前已确认）。

---

## 6. 输入 + AI

### 6.1 玩家输入消费（`InputRuntime.ApplyCharacterInput` vs `CharacterInputModule` + `LF2CharacterActionResolver`）

C# `ApplyCharacterInput` 单函数：combo wrapper → hitA/hitD/hitJ frame jump → frame110 facing → state 301/19 lane → LinkState2 heavy → frame215 landing → frame182/188 recovery → state 0/1/2/4/5 分发 → ApplyFrameVelocityTail。

Unity 有两套：
- `LF2Character` → `LF2CharacterActionResolver`（完整角色输入）
- `LF2Entity` shared-DAT 桥（`RunSharedCharacterDatStandingActionInputPhase` 等，用于"当前 DAT 是角色但 CLR 实例不是 LF2Character"的 transform 后对象）

🔷 合法架构分层。**注意**：shared-DAT 桥自称"最小实现"，只覆盖 standing/walking/running/dash/jump 基础，**不覆盖 combo/catching/held-weapon 全动作**。这不是冗余 —— 它服务 transform（state 501/4000/8000）后仍挂在 wrong shell 的角色。

关键值对齐（已修复）：
- walk 斜向 `Vx *= 5.0/7.0` = 0.7142857142857143 ✅（两侧都是）
- heavy run 斜向 `Vx *= 5f/6f` / `0.8333...` ✅

**✅ combo wrapper（DJA 等 9 组方向+攻击/跳连招）已落地并补 fresh 运行时验证**：Unity 现已由 `NTSDInputStateModule` 承载 9 组 wrapper 与 oid6（Sasuke）DjaGuard 特判，真实输入消费路径是 `LF2Character.RunPostCooldownInputPhase -> UpdateLocalInputStateFromControllerBuffer -> ComboUpdate -> NTSDInputStateModule.ApplyFrameInput`。本轮新增 `BattleRuntimeSelfCheck` 覆盖 9 组连招帧跳与 oid6 guard hold/release，`Temp/NTSD_BattleRuntimeSelfCheck.result` fresh 返回 `PASS`。

### 6.2 AI（`InputRuntime.PrepareAiInputBasic`）

**✅ AI 输入生成器已完整落地并通过 fresh Unity 运行时验证**：
- C# `InputRuntime.cs:16` `PrepareAiInputBasic`（~600 行巨型函数，oid 专属 combo 决策、C8 威胁扫描、7A/7B 守卫、队友守卫、held weapon 决策、历史闸门、oid1/4/5/33/52 多种 oid 专属 combo）。
- 实际包含 14 个辅助函数（已 grep 确认）：
  - `AiBetweenX`、`AiPostCacheCoordinateAllowsSpecial`、`AiPreUpdateTarget3000SideEffect`
  - `AiUpdateOid33_19_16PredictedDuaDecision`、`AiUpdateOid52_1_2_21PreLabel591Decision`
  - `AiUpdateLabel591Oid51_2_18_7Decision`、`AiUpdateFirstDecision`、`AiUpdateTeammateGuardDecision`
  - `AiUpdateOid1ComboDecision`、`AiUpdateCloseOid1Decision`、`AiUpdateOid4ComboDecision`、`AiUpdateOid5ComboDecision`
  - `AiProcessSubOidGroup`、`AiSpecialOidForSubGate`、`AiProcessHelper`
- Unity `SimulationWorld.AiInput.partial.cs` 已覆盖主入口及文档原先漏列的 target/team/move-mode/no-target/三个 `AiProcessSub*` 等完整直接/间接 helper 闭包。
- 输入 pass、runtime 字段、deterministic RNG、runtime-slot 顺序、shared-DAT shell 与 roster/opoint bootstrap 均已接通；fresh build 0 errors，fresh Unity batch 自检通过。

---

## 7. C# 有、Unity 未确认/缺失的战斗逻辑（重点排查项）

| 编号 | C# 逻辑 | 位置 | Unity 状态 | 判定 |
|------|---------|------|-----------|------|
| M-1 | **oid 7/8 → 51 合体 / 51 拆分**；唯一权威为 C# `GameTick.cs:1093-1263` | `GameTick.Run:61-64` 的 input poll 后、正式 character input 前 | `NTSDBattleTickSystem` / `SimulationWorld.Passes` / `NTSDEntityRuntime` / `BattleRuntimeSelfCheck` 已按 poll → M-1 → input apply 分相 | **✅ Audit6 生产修复、延迟 split/输入 gate 矩阵和 fresh full self-check 已通过** |
| M-2 | **复活 pass**（`RunRespawnPass` `GameTick.cs:839-934`：state14+HP<=0 + HitStop 窗口 + 两分支[Hp2Overlay/RespawnCount] + 队友位置平均 + Pp=500/HpMax=Hp3 + Frame=212/YInt=-300 + 生成 oid998 复活特效） | GameTick step10 | ✅ `SimulationWorld.Passes` / `BattleRuntimeSelfCheck` 主逻辑与样例已落地；已补 no-renderer 销毁注销链与 reference-pool 惰性初始化 | **✅ 已完成 / Unity 运行时已验证（T5）** |
| M-3 | **N30 输入触发**（`RunN30InputTrigger`：input history 9/0/9/0→触发码 100/102/104 生成 998 + history gate 广播） | LateEntityUpdate | ✅ `RunLateCharacterDatInputTrigger`（LF2Entity） | ✅ 已移植 |
| M-4 | **状态转换特效**（`SpawnStateTransitionEffects`：state13/frame200 退出 + state18/19 燃烧特效） | LateEntityUpdate | ✅ `SpawnLateTransitionEffects` | ✅ |
| M-5 | **死亡弹地帧**（`ApplyDeathBounceFrame`：frame186 + Vy=-3） | LateEntityUpdate | ✅ `RunLateDeathOpointPreCleanupPhase` 已对齐并由 `CheckLateDeathBounceFrame` 覆盖 | **✅ 已完成 / Unity 运行时已验证（提交 `995c860b`）** |
| M-6 | **F8 强制掉武器**（`RunF8WeaponDrop`） | GameTick | ❌ grep `F8/force drop` 0 命中 | 🗑️ **确认是调试功能，可不移植** |
| M-7 | **kind 4 + WeaponCount>0 → kind 0 + dvx 翻转**（`PreprocessCandidate` 154-172） | HitResolve | ✅ `BruteForceSceneQuery.cs:602-615` 完整实现 | ✅ 已对齐 |
| M-8 | **ShouldUseAlternateHurt / ApplyAlternateDamage**（injury/10 减伤 + KnockbackVx 特殊累积 + FrameDelay=-5） | HitResolve 629-约827 | ✅ 共享 `LF2AlternateDamageResolver`；`LF2Character.Hit` 与 shared-character-DAT resolver 两入口均接入；runtime/stat/运动尾契约均有自检 | **✅ 已完成 / Unity 运行时已验证（T1）** |
| M-9 | **RecordKind0Hit**（命中记录锚点 + spark，武器命中也调用） | HitResolve 1150 | ✅ `LF2Entity.RecordKind0Hit` 统一角色/武器 kind0 记录 | **✅ 已完成 / Unity 运行时已验证（T2）** |
| M-10 | **oid300 特殊命中**（bdy.x>1000→帧号） | HitResolve | ✅ `ResolveHit` ObjectId==300（`LF2CharacterHitResolver.cs:279`） | ✅ |
| M-11 | **state 400/401 传送**（最近敌/最远友） | GameTick early | ✅ `RunEarlyTeleportSpecialsPhase` | ✅ |
| M-12 | **state 500/501 变身 transform** | GameTick early | ✅ `RunEarlyState500/501Specials`（BMD-023） | ✅ |
| M-13 | **stage 波次生成**（`ApplyCurrentWavePhaseAdvance` `GameTick.cs:2317` + `ApplyCurrentWaveImmediateStageSpawns` :2350 + `RefillCurrentWavePositiveStageSpawns` :2226，StageProgression/StageSpawnRuntime 一整套） | GameTick step 23 | ✅ `BattleStageCampaignLoader` / `ApplyMatchConfig` 生产接线 + progression + spawn/refill/advance/bound + identity/dynamic-slot 契约已落地 | **✅ 逻辑与接线已完成 / Unity 运行时已验证；默认 `stage.dat` 部署由用户明确暂缓，不进入当前 backlog（T8）** |
| M-14 | **frame 110/114 → CdDefendLock=3**（`FrameTick.cs:208-209`） | FrameTick 尾 | ✅ `LF2Entity.RunCommonFrameTick` 尾部 + runtime Reset/cooldown | **✅ 已完成 / Unity 运行时已验证（T3）** |
| M-15 | **kind 16 完整结算**（`ApplyKind15Or16` kind=16：KillStat++/ComboCountAtk/SFX_065/vrest/LinkState 断开） | HitResolve 1640-1704 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已补齐 FallDamageDiv 缩放、KillStat/ComboCount、frame200、vrest、2/-2 持有断开与 SFX_065 | **✅ 已完成 / Unity 运行时已验证（T6）** |
| M-16 | **kind 15 完整位移**（`ApplyKind15Movement`：KnockbackVx+真实 Vx/Vz+YInt=-2，按对象类型分 vyStep 3.0/2.3） | HitResolve 1737 | ✅ 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已改为 authority 的 KnockbackVx/Vz + YInt/Vy 语义；武器/铁球侧原 `WhirlwindForce` 保持 3.0/2.3 分支 | **✅ 已完成 / Unity 运行时已验证（T6）** |

> **判定原则提醒**：当前仍标 ❌/⚠️ 的项目都**不能直接删对应 Unity 脚本**；它们是"C# 有 Unity 缺/结果仍需验证"。M-1/M-2/M-7/M-8/M-9/M-10/M-11/M-12/M-13/M-14/M-15/M-16 已确认对齐或完成并运行验证。只有 M-6（F8 调试）确认是调试功能后可不移植。

---

## 8. 判定为"架构不同但等价"的项（🔷 — 不得当冗余删除）

以下 Unity 代码看似"多出来"，实为 Unity 框架下实现 C# 同一逻辑的必要产物，**严禁因为 C# 没有同名文件就删除**：

| Unity 脚本/机制 | 对应 C# 逻辑 | 说明 |
|-----------------|-------------|------|
| `LF2Character*Resolver.cs`（Hit/Catch/DamageState/Action/Interaction/State/WeaponLink） | `NtsdCharacter` + `HitResolve`/`CPointRuntime`/`InputRuntime` 各段 | 组合模式拆分，逻辑等价 |
| `LF2AlternateDamageResolver` + `LF2CharacterDatHitResolver` | `HitResolve.ShouldUseAlternateHurt` / `ApplyAlternateDamage` | alternate 真值集中一次实现，由真实 `LF2Character.Hit` 与 shared-character-DAT 两入口复用 |
| `LF2Weapon*Resolver.cs`（Interaction/HeldState/ReleaseFlow/FrameLogic） | `WeaponRuntime` 各段 | 同上 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 中"当前 DAT 是角色"的分发 | 服务 transform 后 wrong-shell 角色，C# 因为是纯数据 Entity 不需要 shell 概念 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | Unity 运行时化，字段一一对应 |
| `FrameTransistor` hook（OnFrameTickBeforeWaitAdvance 等） | `FrameTick.Tick` 内联步骤 | 拆成 hook 供子类覆写 |
| `SimulationWorld` 动态 runtime slot | `Objects[400]` 固定槽 | Unity 用对象池，遍历顺序需保持 slot 升序 |
| `RefreshRuntimeSnapshot` 调用 | `CharacterSync.SyncRuntimeFromLegacy` | Unity 每 pass 后刷快照 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023：区别于 `ImmediateFrame`（会清 attacking） |

---

## 9. 不需要对齐的部分（明确排除）

| 项 | C# 位置 | 原因 |
|----|---------|------|
| 可活动范围 / Z 边界钳制 | `ApplyPreframeBounds` Z 段、`ClampCharactersToStageZ`、`Bg.ZBoundary*` | 用户明确：bg.dat 可活动范围不对齐，Unity 用 BoundaryWall |
| 相机 | `UpdateCameraAndBgAnimation`、`CameraX`/`CameraVel` | 用户明确：相机不对齐，Unity 用 ProCamera2D |
| bg 层动画 | `layer.AnimCounter` | 背景表现 |
| 结算界面 | `RunResultsTick`、`UpdateBattleResultsFlow` | 非战斗运行时（菜单/结算） |
| SDL/Host/音频桥 | `src/Host/*` | C# EXE 适配层 |
| 数据加载 | `src/Data/*` | Unity 用自己的 DatParser |

---

## 10. 对齐优先级清单（已全部逐行核实，✅=已核实定性）

### P0 — 已修复并完成 Unity 运行时验证
- [x] **§2.1-1 / T0** `exemptVal` 公式 — **已修复并通过 Unity 运行时自检**：`LF2Entity.ResolveArestCooldown` 与 `LF2CharacterHitResolver` 已按 arest/vrest 权威公式处理
- [x] **§2.1-3 / M-8 / T1** ApplyAlternateDamage — **已完成并通过 Unity 运行时自检**：共享 `LF2AlternateDamageResolver` 覆盖约 line 827 的完整权威契约；真实 `LF2Character.Hit` 与 shared-character-DAT resolver 两入口、`Unk344`/统计数组/`HPBound`、heavy/rest/preprocess/state tail 均有针对性检查

### P1 — 已补齐并完成 fresh Unity 运行时验证
- [x] **M-1 / T4** oid 7/8→51 合体拆分 — 已按唯一 C# 权威 `GameTick.cs:1093-1263` 重审；生产顺序为 human poll → M-1 → `NeedClearInput`/tick gate → unified character input，矩阵覆盖 frame85 gate 外延迟 split、oid8 镜像、identity/presentation、human+AI、split reset 与外部 `ItrRest`，并进入 `21:57:40` fresh full PASS
- [x] **M-2 / T5** 复活 pass（`RunRespawnPass` 完整逻辑）— **已完成并通过 fresh Unity 运行时自检**
- [x] **M-13 / T8** stage 波次生成（`ApplyCurrentWaveXxx` 整套）— **逻辑与生产接线已完成并通过 fresh Unity 运行时自检；默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进**
- [x] **P1 / BOUNDS-X** PreFrame 实体 X clamp/free — **已完成并通过 physical worktree fresh Unity 运行时自检**：base `bg.width` 与 phase override 分离、current-DAT 分派、`RelationTeam`/`HitStop`/`Unk344`/`YInt`/严格边界与 `XInt` 契约均有矩阵覆盖

### P1 — 已确认缺失战斗逻辑（需新增）
- [x] **§6.2 AI / T9** `PrepareAiInputBasic` 完整调用闭包 — **已完成并通过 fresh Unity 运行时自检**

### P1 — 已确认对齐（无需动作）
- [x] **M-7** kind4+WeaponCount>0→0 dvx 翻转 — ✅ `BruteForceSceneQuery.cs:602-615`
- [x] **M-9 / T2** 武器命中 spark（`RecordKind0Hit`）— **已完成并通过 Unity 运行时自检**（角色与武器 kind0 路径统一记录）
- [x] **§5** HP/PP 自然恢复 + HpMax/HPBound — ✅ 逐字段对齐
- [x] **kind 10/11 笛子** ✅、**kind 14 方向阻挡** ✅、**oid300** ✅、**kind 5 委托** ✅

### P2 — 帧推进尾部特判（已核实）
- [x] **§3-1/§3-2** state14 复活 HitStop（oid/5==3 + difficulty 分支）— ✅ 完整对齐（`LF2Character.cs:2134-2163 / 2205-2211`）
- [x] **§3-3** frame mp turn-around — ✅ 完整对齐（`LF2Entity.cs:3284-3321`）
- [x] **§3-4** frame 202 HitStun=20 — ✅（`LF2Entity.cs:3634`）
- [x] **M-14 / T3** frame 110/114 CdDefendLock=3 — **已完成并通过 Unity 运行时自检**

### P2 — 已补齐并完成 Unity 运行时验证
- [x] **M-15 / M-16 / T6** kind 15/16 完整位移与副作用 — **已完成并通过 Unity 运行时自检**

### P3 — 确认可不移植
- [x] **M-6** F8 强制掉武器 — ✅ 确认是调试功能，Unity 不需实现（非冗余，是未移植的调试项）

### 二次审计战斗差异收口（2026-07-15）

> 本表只列会改变战斗模拟结果的项目。UI/HUD、camera/background/render、audio playback、network、replay，以及 F7-F9/debug 路径均不进入 backlog。`stage.dat` 默认资产部署由用户明确暂缓，也不进入本轮推进。
>
> 计数单位是“差异簇”而不是原子代码点；例如 INPUT-8 同时包含 shared-DAT running 的提前返回和缺 defend 分支。下表是 **Audit2 历史记录**；其中旧来源坐标不得用于当前实现，任何后续复查都必须回到 `ntsd_release_C#`。当前执行状态以 Audit4 为准。

#### 已确认差异簇（14/14 已修复并通过新增自检）

| 编号 | 差异 | Unity 证据 | Authority 证据 |
|---|---|---|---|
| INPUT-1 | state 7 `Defending` 被加入正式输入 state switch；authority switch 只分发 0/1/2/4/5 | `LF2CharacterActionResolver.cs:54-81` | C# `InputRuntime.ApplyCharacterInput:718-735` |
| INPUT-2 | jump 输入门槛读取 `PS.y`/浮点 Y，authority 使用 `YInt`；real character 与 shared-DAT 路径均需统一 | `LF2CharacterActionResolver.cs:61-68`；`LF2Entity.cs:1529` | C# `InputRuntime.ApplyCharacterInput:728-730` |
| INPUT-3 | state 301/19 的纵向移动门槛读取 `PS.y`，authority 使用整数 Y 门槛 | `LF2CharacterActionResolver.cs:503-516` | C# `InputRuntime.ApplyCharacterInput:680-685` |
| INPUT-4 | 正式 battle input pass 调用 `RunPostCooldownInputPhase` 后没有执行当前帧 `dvx/dvy/dvz` tail；唯一 tail 留在当前无生产调用者的 `RunCharacterInputPhase` | `SimulationWorld.Passes.partial.cs:54-63`；`LF2Character.cs:750-779` | C# `InputRuntime.ApplyCharacterInput:737`；`InputRuntime.ApplyFrameVelocityTail:1463-1510` |
| INPUT-5 | `CdDefendLock` 同时由 Runtime 与 `NTSDInputStateModule` 持有/衰减/回写，存在双状态源不同步 | `SimulationWorld.Passes.partial.cs:920-928`；`NTSDInputStateModule.cs:75-111,165-174,408-436`；`LF2Entity.cs:1188-1196` | authority 仅有实体 input runtime 单一字段 |
| INPUT-6 | Super Punch 分支提前清零 `HitConfirmEa`；authority 在这里只读取命中确认并切帧 | `LF2CharacterActionResolver.cs:92-104`；shared-DAT `LF2Entity.cs:1269-1281` | C# `InputRuntime.ApplyStandingActions:942-953` |
| INPUT-7 | `ImmediateFrame` 统一清零 `AttackingCounter`，把 authority 的 raw frame write 和计数副作用合并，影响多个输入动作跳帧 | `LF2LivingObject.cs:480-497` | C# `InputRuntime.ApplyJumping:1210-1247`；`ApplyDash:1250-1315`；`ApplyFrame215Landing:1402-1441`；这些分支直接写 `Frame`，只在对应分支明确清 `Attacking` |
| INPUT-8 | transformed/shared-DAT running 路径存在提前返回，并缺少 authority 的 running defend 分支（一个关联差异簇） | `LF2Entity.cs:1578-1636` | C# `InputRuntime.ApplyRunning:1131-1205` |
| INPUT-9 | transformed/shared-DAT frame 215 额外接受 attack 分支，authority 只处理其正式输入条件 | `LF2Entity.cs:1774-1810` | C# `InputRuntime.ApplyFrame215Landing:1405-1438` |
| INTERACT-1 | `LF2SpecialAttack` 没有声明使用 dynamic runtime slot，opoint 技能实体不能稳定遵循 `50..399` 槽区契约 | `LF2SpecialAttack.cs:68`；`LF2Entity.cs:1014` | C# `FrameTick.SpawnFromOpoint:333-350`；`NtsdConstants.MaxObjects:9`；`SimulationWorld.Objects:28` |
| INTERACT-2 | dynamic slot `50..399` 满后 Unity 回退分配 `0..49`；authority 应直接生成失败 | `SimulationWorld.Registry.partial.cs:359-369` | C# `FrameTick.SpawnFromOpoint:333-350`；只扫描 `50..399`，无槽时直接返回 `null` |
| INTERACT-3 | vrest key 混用 `StableId` 与 runtime slot，可能导致互斥命中对象身份与固定槽 authority 不一致 | `LF2WeaponBase.cs:672,718`；`LF2ObjectPointFactory.cs:260-261`；`LF2SpecialAttack.cs:1001`；对照 `LF2SpecialAttack.cs:995-996` | production collision/vrest 路径以 `Runtime.SlotIndex` 为对象身份 |
| INTERACT-4 | state 3003 opoint 的双向 vrest 参与对象/身份写入与 authority 不一致 | `LF2ObjectPointFactory.cs:213-216,533-537` | C# `FrameTick.ProcessOpointSpawn:280-287` |
| INTERACT-5 | 非角色 parent 的 kind 2 链接把 `StableId` 写入 `TargetSlotIndex`/`HeldWeaponStableId`/`HolderStableId` 等 slot 字段 | `LF2ObjectPointFactory.cs:540-555`；消费端 `SimulationWorld.QueryAndLinks.partial.cs:119-133` | C# `FrameTick.SpawnFromOpoint:422-430`；kind 2 的 `TargetIdx/HeldWeaponSlot/HolderIdx` 均写 runtime slot |

当前收口状态：

- **INPUT-1~9：全部已修复并运行时验证。** `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖 state switch、`YInt` 门、frame velocity tail、单一 defend-lock 真值、Super Punch、raw frame write、running 顺序/defend/反向停跑和 frame215。
- **INTERACT-1~5：全部已修复并运行时验证。** `CheckInteractionRuntimeSlotContracts` 覆盖 dynamic slot `50..399`、满槽直接拒绝、runtime-slot vrest、state3003 双向 vrest 和 non-character kind2 链接；满槽拒绝同时断言不遗留空 registry bucket、renderer pool 或 reference/logic pool 生命周期残留。
- **NARUTO-DDJ / OPOINT-LIFECYCLE：已修复并运行时验证。** 真实 frame271→272/273→oid205/204→六分支→`6 x oid33/action307` 回归覆盖 reference pool 类型安全、pending lifecycle finalize、factory 注册时机、池化角色 `StableId` 重分配和 opoint 关系字段继承；详细链路见 §4.3。

#### 历史 Audit2 风险收口（当前均已关闭）

| 编号 | 状态 | 审计结论 / 验证 |
|---|---|---|
| RISK-1 | ✅ 已修复 / Unity 运行时已验证 | late frame rollover 不再通过 `FrameEvent` 二次推进 walking/running locomotion；新增矩阵验证同 tick `AnimCounter` 只推进一次并保留 state-entry 副作用 |
| RISK-2 | ✅ 已修复 / Unity 运行时已验证 | input/move raw frame write 均保持 `PrevFrame/PN`、wait counter 和非显式清零的 attacking；新增 raw move write 矩阵通过 |
| RISK-3 | ✅ 已修复 / Unity 运行时已验证 | held/`TrackerParent` 行为引用改由 runtime slot 和反向关系校验；注销、同槽复用、异槽复用均清理失效缓存，`CheckHeldReferenceSlotReuseContracts` 通过 |
| RISK-4 | ✅ 已由 Audit5 `R-HC-05` 关闭 | fixed slot、注销与同槽复用矩阵已补齐；本行仅保留旧风险来源，不再是开放项 |
| RISK-5 | ✅ 已修复 / Unity 运行时已验证 | step7/step9 capability 与入口按 current DAT `obj_type` 中央分派；character shell→non-character 和 special/non-character shell→character 双向矩阵验证不会漏跑或重复跑 interaction pass |

#### 历史 backlog / 验收矩阵（CP-NV1/2/3 与 STEP10 已重新关闭）

下表保留既有工作的**历史来源坐标与 self-check 证据**；旧工程/EXE 坐标不具当前权威性，不得用于当前实现或验收，也不能覆盖或冲销 Audit4。

| 优先级 / 编号 | 状态 | Authority | Unity 现状 | 明确缺口 | 验收标准 |
|---|---|---|---|---|---|
| P0 / CP-NV1 action selection | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.RunKind1Pass:109-130`；`ApplyCpointAction:189-203`；`FrameRuntime.SetFrameImmediate:12-16` | real/shared 两路 immediate helper 已清 `Runtime.FrameWaitCounter`，并保留 `Trans.WaitCounter` 与 `Prev2` | 已关闭 | `CheckCpointNegativeActionMatrix` 现覆盖 real/shared 双壳、aaction/taction/jaction 三类负 action、双方 FWC 清零、方向/attacking/wait/Prev2；combined fresh PASS |
| P0 / CP-NV2 throw snapshot/raw | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.ApplyThrow:306-343`；`SwapAttackerCharData:345-362` | throw 使用进入 `ApplyThrow` 的 source `atkFrame`；victim `Vz` 先清 0，再按方向覆盖；raw frame 顺序保持 | 已关闭 | `CheckCpointThrowRawAndTransformMatrix` 覆盖 none/up/down/both=`0/-3/+3/0`、raw carrier 与 transform source snapshot 的 frame112、victim `(76,-36)`；combined fresh PASS |
| P0 / CP-NV3 held sync | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.SyncHeldCpoint:22-48`；`SyncCaughtByCpoint:206-304`；`FrameRuntime.SetFrameImmediate:12-16` | `vaction=0` 保留进入 frame/facing/FWC；非零 immediate 切帧并清 FWC；负值 flip/abs；center/cpoint 均取最终 resolved current frame | 已关闭 | `CheckCpointHeldSyncVactionMatrix` 的 real/shared `-131/0/131` 完整矩阵 fresh PASS |
| P1 / FLOW-1 FrameToggle | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:32-36`；`GameTick.RunState400401Pass:962-1030` | Flow 新增 `FrameMod12`/`FrameToggle` 并由 `AdvanceBattleFlowTick` 与 CurrentTick/InputPhase 同步推进；early teleport 读取 toggle，source 无 Character gate、401 可选 self、target 保留 Character 过滤 | 已关闭 | `CheckBattleFlowToggleAndTeleportMatrix` 覆盖 tick 1-4/11-13、reset、401 self、non-character source、target 选择/no-target，Unity self-check PASS |
| P1 / LINK-1 positive link validation | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:113`；`GameTick.ValidatePositiveLinks:2009-2034` | `ValidateHeldLinksAll` 按 runtime slot `0..399` 覆盖所有 active `LF2Entity`；valid 仅 target range/active/反向 holder；invalid 只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot`，不清 inactive/mismatch target 的反向字段 | 已关闭 | `CheckValidatePositiveLinksMatrix` 覆盖 valid character/non-character、slot0/399、target -1/400、inactive/mismatch、link<=0、target link 状态和多 holder slot 顺序，Unity self-check PASS |
| P1 / BOUNDS-X | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:128`；`GameTick.ApplyPreframeBounds:1301-1398` | `LF2Entity.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride)` 按 current DAT type/OID 中央分派；实体 pass 显式使用 `BaseStageWidthPx`，不改变 stage spawn/AI/camera 的既有 `StageWidthPx` 消费 | 已关闭；oid122/123 条件使用独立 `Unk344>0`，不再误用 `WeaponFlightCounter` | `CheckPreFrameXBoundsMatrix` 覆盖 slot/team/hit-stop/override、strict edges、type3/free、oid122/123、`YInt`、current-DAT/CLR 交叉、base/active width 分离、`XInt` 与 world lifecycle；physical worktree fresh Unity 日志双 PASS |
| P1 / TRANSFORM-SHELL | ⚠️ 历史 focused PASS，当前 fresh full self-check FAIL | C# `GameTick.RunStateSpecialPreCollision:1615-1662`；`GameTick.InitRuntimeIdentity:1664-1671`；`EntityCategoryResolver.Get` | frame/physics/landing 及 step7/step9 interaction capability/entry 已按 current DAT 中央分派；transform 后以目标 `weapon_hp` 刷新 `WeaponFlightCounter`，不改 `WeaponCount`；state8000 再设 hit-stop 140 | 既有修复保留；最新 transformed landing fixture 仍有代码契约回归 | `CheckStateTransformLandingMatrix` + `CheckStateTransformInteractionPhaseRouting`；2026-07-15 focused PASS 为历史证据，2026-07-18 最新 full run 失败，实际 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4` |
| P0 / STEP10 call-chain/tail/stats | ✅ 已完成 / fresh full self-check | C# `CPointRuntime.RunKind1Pass:50-149`；`SyncCaughtByCpoint:206-304`；`ApplyThrow:306-343` | state9 首次 sync、mismatch/escape immediate + early return、escape 同 tick `Vx/Vy`、FWC 清零及实体 stats-only 契约均已落地 | 已关闭 | `CheckCpointEscapeAndMismatchEarlyReturn`、`CheckCpointDecreaseEscape`、`CheckSharedDatCpointStep10StatsAndInputOrder` 已按权威重写；覆盖不跑 throw tail、即时速度和 world stats 不变，combined fresh PASS |
| P1 / OPOINT-VIS | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:81-148`；`RunNaturalRandomWeaponDrop:636-697`；`RunLatePerEntityUpdatePass:1533-1537`；`RunLateEntityUpdate:1539-1612`；`FrameTick.ProcessOpointSpawn:233-331` | 已恢复 pre-advance frame_logic、natural drop、逐实体 late producer 三个发布边界；late pass 保持动态 slot 扫描 | 验收过程修复 pending destroy 实体被 active-only 采集过滤，确保 fragment/transition 发布后只回收一次 | `CheckQueuedObjectPointPassBoundaries`、`CheckSimulationWorldLateMutation` 覆盖 real factory queue、三边界、父回收、高/low slot 可见性；2026-07-15 fresh PASS |
| P2 / FRAME-ADV | ✅ 已完成 / Unity 运行时已验证 | C# `GameTick.Run:92-104`；`EntityCategoryResolver.Get`；`EntityDispatch.DispatchFrameAdvance:36-57`；`FrameAdvance.Advance:13-47`；`Physics.Update:16-31` | `SerialTickAll` 按 runtime slot 交错执行 Transit/TU；per-class 路由已收到 current DAT；SpecialAttack 不再提前运行 wait/next | 验收过程修复 character/weapon 壳的 `PS.BindRuntime`，防止物理仍读写脱离 runtime 的状态 | `CheckSerialTickInterleaveAndFrameEdgeMatrix` 覆盖逐 slot 顺序、SpecialAttack 单次 physics、weapon shell 的 type3/other DAT、negative next；落地矩阵同时通过 |
| P2 / FRAME-TICK | ⚠️ 历史 focused PASS，当前 fresh full self-check FAIL | C# `FrameTick.Tick:13-216` | current-DAT 公共主干已集中到 `RunCommonFrameTick`；type3 `hit_a`、state14、iron-ball frame20、wait/next/999/negative、frame mp/`PpDisplay` 与 tail 统一执行 | C# `FrameTick.Tick` 无 oid9 专属 drain 分支；SpecialAttack 的旧重复 drain/counter 已移除；transformed landing fixture 仍需修复 | `CheckSpecialAttackStep4AndLateFrameTick`、`CheckFrameTickPpDisplayAndCurrentDatMatrix`、`CheckStateTransformLandingMatrix`；2026-07-15 focused PASS 为历史证据，2026-07-18 最新 full run 失败 |
| P3 / COLLISION-SNAPSHOT | 🔷 权威审计未发现生产差异 / 保留回归风险 | C# `GameTick.SnapshotPrevFrame2:1521-1531`；`CollisionCollect.CollectCandidates:14-40`；`CollectPair:50-136`；`RecordCandidate:162-302`；`NtsdConstants.HitCandidateMax:11` | `CaptureCollisionFrameSnapshotsAll` + `BruteForceSceneQuery` 的普通生产路径与权威一致，当前没有实锤修复项 | Unity carrier 缓存对象引用；权威 C# `NtsdEntityRuntime.HitCandidateSlots:219-220` 缓存 runtime slot。若未来在 snapshot 消费期间引入同 slot 即时复用 producer，语义可能分叉 | 保留多候选/20+、同距、Prev2、cache 隔离回归；未来新增 pass 内 slot reuse 时必须补专项测试 |

#### CP-NV / STEP10 C# 重审矩阵（原始总账既有项，重开后重新关闭）

本段是对原历史 backlog 的重审，不修改 Audit5 原始 74 项分母。旧历史 PASS 不作为新证据；生产和检查已按下列权威矩阵重写，并统一进入 `21:57:40` combined fresh full PASS。

1. **CP-NV1 immediate frame 字段边界**：real character 与 shared-DAT 两路 attacker/victim immediate frame 均清 `Runtime.FrameWaitCounter`，同时保留 `Trans.WaitCounter`、`Prev2` 和 C# 未写的其他 carrier。最终负向覆盖包含 aaction/taction/jaction、负 action、方向翻转、双方 attacking 清理及双壳字段边界。
2. **CP-NV2 source snapshot 与 raw 顺序**：throw 保留调用前 `atkFrame`，先据 source snapshot 算位置，再 raw 写 attacker `Frame/Prev2`、清 attacker `Attacking`，之后写 victim `Vx/Vy`、先将 `Vz=0`、按方向覆盖，最后 raw 写 victim `Frame/Prev2`。定向矩阵已覆盖 none/up/down/both 的 victim `Vz=0/-3/+3/0`，以及 transform fixture 的 attacker frame112 与 victim `(76,-36)`。
3. **CP-NV3 held `vaction` 矩阵**：`SyncCaughtByCpointStep10` 和 held position sync 已统一读取最终 resolved current frame；权威矩阵如下：

| `vaction` | 权威 frame/facing | `Runtime.FrameWaitCounter` | held 位置数据源 |
|---:|---|---|---|
| `-131` | immediate 写 `-131` 后 flip/abs 为 frame131、朝向翻转 | 清 0 | frame131 的 center 与 cpoint |
| `0` | 不切帧、不翻面，保留进入时 current frame | 保留进入值 | 保留下来的 current frame 的 center 与 cpoint |
| `131` | immediate 切 frame131，不翻面 | 清 0 | frame131 的 center 与 cpoint |

4. **STEP10 P0 调用链**：state9 已先执行第一次 `SyncCaughtByCpoint` 再做 decrease/action；invalid victim/mismatch 与 caught-duration escape 均在 immediate frame 后 early return，不再继续 throw/dir。escape 同 tick 写 victim `Vx/Vy`；held injury 保留 holder/victim entity stats，不写 world `KillStats/DamageStats`。

最终证据：上述共享 helper、throw snapshot 和 Step10 顺序均已收口；相关检查不是删除失败断言求绿，而是按 C# 权威改写并扩展 real/shared-DAT、负 action、early-return、速度与 stats 负向覆盖。combined Architect 最终结论为 `P0/P1/P2=0`。

---

## 附：核对方法

1. 本文所有 ⚠️/❓ 项都需**打开对应 C# 源码段 + Unity 源码段逐行比对**后才能定性。
2. 定性为"Unity 用别的方式实现了" → 标 🔷 并记录对应关系，**不删**。
3. 定性为"C# 有 Unity 真没有，且是正式战斗逻辑" → 标 ❌ 进 P1 待补。
4. 定性为"C# 是调试/表现/菜单，非战斗运行时" → 标 🚫 排除。
5. 每完成一项核对，更新对应行状态并在 §10 勾选。

---

## 附二：核实总账（更新至 2026-07-16）

**✅ 二次审计确认差异已收口（14/14）：**

输入/动作 9 项（INPUT-1~9）与交互/opoint/vrest 5 项（INTERACT-1~5）均已修复并通过新增自检。RISK-1/2/3/5 经审计实锤后也已修复并运行时验证；只剩 RISK-4 一项未找到正式主循环可达触发边界的待审计风险，不计入确认差异。

**✅ Naruto DDJ 新确认差异已收口（1 个关联差异簇 / 5 个根因）：**

真实 Naruto 防下跳链暴露的 reference pool 污染、factory 注册时机、pending lifecycle 同槽复用、池化角色 StableId 和 opoint 关系字段继承问题均已修复；完整链回归确认 6 个 clone 到达 action307 且 renderer 可见。

**✅ 已修复真 bug（共 1 项）：**

| 项 | 内容 |
|----|------|
| §2.1-1 / T0 | `exemptVal` 已改用权威 arest/vrest 公式，并通过 Unity 运行时自检 |

**✅ 原缺失项已完成并通过 Unity 运行时自检（主要项）：**

| 项 | 内容 |
|----|------|
| M-1 / T4 | oid 7/8→51 合体拆分；C# `GameTick.RunOid5152RuntimeMaintenance:1093-1121`、`TryMergeOid7Or8Into51:1123-1212`、`SplitOid51BackToPair:1214-1263` 的 gate/oid8 镜像/身份表现/DJA human+AI full-tick/split reset 与 `ItrRest` 契约均已覆盖 |
| M-2 / T5 | 复活 pass（含 free-entity gate、队友平均落点、stored-count 分支与 oid998 特效） |
| M-8 / T1 | 共享 ApplyAlternateDamage 完整契约、真实角色/shared-DAT 两入口及 object-pass 预处理 |
| M-9 / T2 | 角色/武器统一 `RecordKind0Hit` |
| M-14 / T3 | frame 110/114 写 `CdDefendLock=3` 及 cooldown 生命周期 |
| M-15 / M-16 / T6 | kind15 authority 位移 + kind16 完整结算、副作用与持有断开 |
| combo / T7 | RunComboWrappers 9 组连招 + oid6 DjaGuard |
| Naruto DDJ / OPOINT-LIFECYCLE | frame271 起始、oid205/204 递归链、6 x oid33/action307、对象池/slot/StableId/关系字段完整契约 |
| M-13 / T8 | stage immediate spawn、positive refill、清场推进与 phase bound |

**历史快照（Audit4 前）：** 当时只保留 RISK-4 与完整对局逐帧对拍缺口；该结论已被 Audit4-01..16 取代，不代表当前无待实现差异。

**✅ 已确认对齐或已完成并验证（主要项）：**
tick 主循环主干、kind 0/4/9 主流程（含 raw kind9→kind0 预处理与 alternate）、kind 6/8/10/11/14 命中、oid300、kind5 委托、kind4+WeaponCount 翻转（M-7）、HP/PP 自然恢复（§5）、heal/catch timer、帧推进主干 + state14 复活 HitStop（§3-1~§3-5）、frame mp turn-around、opoint 生成、cpoint 抓取、state 400/401/500/501、N30 触发、状态转换特效。

**🔷 架构不同但等价（严禁删，见 §8）：** resolver / shared-DAT 桥 / 字段化 runtime / hook 拆分 / 动态槽 / DirectWriteFramePreserveWaitCounter 等。

**🚫 不需对齐（见 §9）：** UI/HUD、camera/background/render、audio playback、network/replay、Host 和 F7-F9/debug 控制路径。**🗑️ 确认可不移植：** M-6 F8 调试掉武器。

**⏸️ 用户明确暂缓：** T8 默认 `stage.dat` 资产部署。T8 逻辑/接线和 self-check 状态不变，但该资产工作不进入当前推进。

---

### Audit4 前历史总结（已失效）

**本段只记录 Audit4 前的历史验收快照，不是当前执行口径。** BATTLE-AUDIT4-01..14 的生产修复和已有断言现已通过 fresh full self-check，但 3 项定向 Play Mode 尚未完成；T8 默认 `stage.dat` 资产部署仍由用户明确暂缓并排除在当前 backlog 之外。

## 第三次实战/静态审计（2026-07-16，最高优先级）

旧版“当前无确认差异”结论已失效。以下 BATTLE-AUDIT3-01..17 均为已静态确认的战斗逻辑差异，17 项生产修复现已全部落地。最新 fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` 源码时间 `2026-07-16 18:24:04`，Unity `Assembly-CSharp.dll` 时间 `18:31:52`，`Temp/NTSD_BattleRuntimeSelfCheck.result` 于 `18:33:00` fresh 返回 **PASS**，满足 source < DLL < result。该结果包含本轮 M-1/T4 完整矩阵。此前生产 diff 的 Architect 复核结论保留；新增自检覆盖由本次 fresh build/PASS 证明。上述证据只关闭编译、静态复核和针对性 self-check 门槛；本轮变更后的真实 `NTSD_Battle` Naruto 防前跳螺旋丸、奔跑防跳命中及防下跳六分身仍待 Play Mode 验收，因此不得把 17 项标成 Play Mode 全完成，也不得宣称战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。

| 编号 | 双方证据 | 影响 | 状态 |
|---|---|---|---|
| BATTLE-AUDIT3-01 | Unity `BattleTestBootstrap.cs:203` 只写 Team；C#/正式入口 `AppManager.cs:206-207` 写 Team+RelationTeam；`LF2WeaponInteractionResolver.cs:20-23` 对 RelationTeam=0 退出 | oid434 action396 kind3 消费被阻断，Naruto frame256 链不成立 | 生产修复和针对性 self-check 已通过；`RelationTeam` 已补，仍待真实 bootstrap 与 Naruto 螺旋丸 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-02 | C# `WeaponRuntime.cs:140-149` cover=0 为 z+1/y-1；Unity `LF2CharacterWeaponLinkResolver.cs:265-277` 相反；renderer `LF2ObjectRenderer.cs:219-220` 另加 zz | held 武器 Y/Z 与排序偏移，renderer 仅部分抵消 | 生产修复和针对性 self-check 已通过；held 层级、位置与跟手仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-03 | Unity `BruteForceSceneQuery.cs:1630-1643` coarse union 排除 kind5，消费侧 `:529-660` 才替换；C# `CollisionCollect.cs:431-451` union 纳入全部 itr | kind5-only 命中在粗筛阶段消失 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-04 | Unity `BruteForceSceneQuery.cs:1614-1625,1658-1668` 过滤大坐标；C# `CollisionCollect.cs:431-478` 保留原始几何；DAT 有 Naruto y=80000 kind3 | 高层碰撞候选无法进入 Unity | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-05 | Unity `LF2WeaponHeldStateResolver.cs:75-78`、`LF2Weapon.cs:675-730` 有 ordinary weapon_strength held 旁路；C# `WeaponRuntime.cs:71-213` 无此旁路 | 普通武器 held 动作/伤害路径偏离 | 生产修复和针对性 self-check 已通过；螺旋丸按攻击键的真实 weapon 路径仍待 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-06 | Unity `NTSDBattleTickSystem.cs:37,50,56` 每 tick 三次 HeldObjectProcessAll；C# `GameTick.cs:99-103` 一次 Step12、一次 SyncHeld | 重复同步/释放/消耗 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-07 | Unity `NTSDBattleTickSystem.cs:38-50` candidate/hit 后才 PreInteraction；C# `GameTick.cs:99-106` 先 cpoint/link 再 collect | 本 tick cpoint/held 状态不能影响候选 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-08 | C# `GameTick.cs:95-106` candidate 前 clamp Z；Unity `NTSDBattleTickSystem.cs:37-39,55-56` clamp 在交互后 | 候选读取未 clamp 的角色 Z | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-09 | Unity 原实现对 invalid positive link 只清 `LinkState`；C# `ValidatePositiveLinks` 对无效链接只清 holder 的 `LinkState`、`TargetIdx`、`HeldWeaponSlot` | holder 残留 target/held slot 污染后续 held；inactive/mismatch target 的反向字段不在此处清理 | 已按 C# 契约只清 holder 三字段，针对性 self-check 已通过；不清 target 反向字段，仍待真实 Play Mode |
| BATTLE-AUDIT3-10 | Unity `SimulationWorld.Passes.partial.cs:635-648` 依赖 Supports；基类 `LF2Entity.cs:1034-1036` 默认 false，Special/Other 无 override；C# `GameTick.cs:83-90,165-170` 统一分派非角色 DAT hit_Fa | Special/Other hit_Fa 时机/执行路径错误 | 生产重构和 fresh self-check 已通过：`hit_Fa1..14` 唯一下沉 `LF2Entity`，Special/Other/current-DAT shell 共用；新增覆盖 3/4/10/14，3/14 对 Other、current-DAT Character、Special 三壳连续两 tick 验证副作用仅一次，4 覆盖 catch frame/速度/`CatchTimer`，10 覆盖原路径与落地摩擦防重复；仍待真实 Play Mode 场景验收 |
| BATTLE-AUDIT3-11 | Unity `LF2ObjectPointFactory.cs:221-229` logicalY+PS.z；C# `FrameTick.cs:381-394` spawnY 不加 Z；Character/Weapon/Other 初始化直接用 task.pos.y，renderer `LF2ObjectRenderer.cs:278-280` 再加 displayZ | non-special opoint 出生高度可能双加 Z；SpecialAttack `LF2SpecialAttack.cs:1383-1387` 会减回 | 生产修复和针对性 self-check 已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-12 | Unity `SimulationWorld.QueryAndLinks.partial.cs:77-83` 强制 LF2Character holder；C# `WeaponRuntime.cs:86-94` 接受任意带 CharData Entity | shared-DAT/非 Character holder 断链 | generic holder、damaged 后继续 dvx/kind3 与 IronBall `FrameDelay=1` 已落地；新增 `CheckWorldLevelRealWeaponStep12Contracts` 经 `SimulationWorld.HeldObjectProcessAll`、generic `LF2Entity` holder、真实 `LF2Weapon` 覆盖 damaged→dvx、damaged→kind3、IronBall `FrameDelay=1` 并 fresh PASS；仍待真实 Play Mode 场景验收 |
| BATTLE-AUDIT3-13 | Unity `BruteForceSceneQuery.cs:1603-1627,1646-1677` 过滤 body kind、x/y、w/h/zwidth；C# `CollisionCollect.cs:431-478` 不过滤；full-height 识别两边均有 | 正式大范围技能/特殊几何被 Unity 粗筛排除 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-14 | Unity `BruteForceSceneQuery.cs:446-526,1277-1304` nearest/bodyX gate 依赖 modeArg==1；C# `CollisionCollect.cs:181-240` 无 mode gate | 默认模式目标选择/候选数不同 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-15 | C# `CollisionCollect.cs:144-158` 有 oid205→oid9 frame301、hit_a/d/j=999、同非零 Unk364 pair gate；Unity 仅有 oid→209 kind9 gate `BruteForceSceneQuery.cs:1064-1075` | Naruto 相关同关系对象错误进入候选 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待 Naruto 真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-16 | C# same-team 例外 `CollisionCollect.cs:304-355` 读 attacker Prev2/collision；Unity `BruteForceSceneQuery.cs:988-1007,1034-1037` 读 current | 帧边界放行/拒绝相反 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |
| BATTLE-AUDIT3-17 | C# kind8/state3005 lead-in `CollisionCollect.cs:99-101` 读 current；Unity `BruteForceSceneQuery.cs:990-1002` 传 Prev2 collision | kind8 延迟命中时机偏移 | 生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；仍待真实 Play Mode，未完成场景验收 |

**本轮验收状态：**fresh `/m:1` build 已为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full self-check 返回 **PASS**。除 Audit3-10 的 3/4/10/14 扩展矩阵和 Audit3-12 的 world-level generic holder/真实 weapon Step12 矩阵外，本结果也覆盖 M-1/T4 的完整运行时矩阵。下一步仍须在真实 `NTSD_Battle` 回归 Naruto 防前跳螺旋丸的层级/位置/跟手/攻击路径、奔跑防跳命中，以及防下跳六分身。因此 17 项只能称为“生产修复已落地、针对性 self-check 已通过、Play Mode 未全部验收”。T8 默认 `stage.dat` 部署继续暂缓。

## 实施进度（2026-07-16）

> §10 的 `[x]` 仅表示“已核实定性”，不表示已经实现；实际完成状态以本表为准。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState`。type3 lead sound 已按代码权威对齐，headless 未直接观测音频 | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **历史实现/self-check 已通过；待 C# 重审** | 唯一权威为 C# `GameTick.cs:1093-1263`；旧实现的 pass 顺序、merge/split 与身份链需据此重新核验 | 既有 7 项检查仅保留为回归基线，不能代替 C# 权威重审 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |
| T7（§6.1 / combo） | **已完成 / Unity 运行时已验证** | `NTSDInputStateModule` 已承载 9 组 combo wrapper 与 oid6 DjaGuard；角色真实输入路径经 `RunPostCooldownInputPhase` 消费并落到 `ApplyFrameInput` | `CheckComboWrappersCharacterFrameJumps`、`CheckOid6DjaGuardComboHold` 已覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release 并通过 |
| T8（M-13 / stage） | **逻辑与接线已完成 / Unity 运行时已验证；默认资产部署暂缓** | `BattleStageCampaignLoader`、`ApplyMatchConfig` 生产接线；stage progression/runtime；立即刷敌、positive refill、清场推进、phase bound、精确身份字段与 dynamic slot 50+ | 三项 stage self-check 均通过；默认 `stage.dat` 部署由用户明确暂缓，不进入当前 backlog |
| T9（AI） | **已完成 / Unity 运行时已验证** | `SimulationWorld.AiInput.partial.cs` 完整 AI 闭包；human/AI 输入 pass 分段；runtime 字段与 roster/opoint bootstrap；shared-DAT shell | `CheckAiTargetCacheCoordinateAndDeterminism`、`CheckAiHumanInputIsolation` 通过，并回归 T0-T8 |
| 二次审计 INPUT-1~9 | **全部已修复 / Unity 运行时已验证** | real/shared-DAT input state、raw frame、velocity tail、running/frame215 等契约已按 authority 收口 | `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵通过 |
| 二次审计 INTERACT-1~5 | **全部已修复 / Unity 运行时已验证** | dynamic slot、满槽拒绝、runtime-slot vrest、state3003、non-character kind2 已收口；拒绝路径清理空 bucket/pool/reference 生命周期 | `CheckInteractionRuntimeSlotContracts` 通过 |
| Naruto DDJ / OPOINT-LIFECYCLE | **已修复 / 当前版本真实 Play Mode 已通过** | active-only reference release；register finalize pending old lifecycle；factory slot guard 后移；pooled character 重分配 StableId；`PostInitLiving` 补 Team/RelationTeam/HolderCopy 继承 | 真实生产输入链 `L -> L+S -> L+S+K` 通过；6 个 unique clone 均到 action307，6 个 renderer 同时可见 |
| 二次审计 RISK | **历史 RISK-1..5 均已关闭** | locomotion、raw move frame、held/Tracker slot、current-DAT interaction 与 fixed-slot reuse 已收口 | Audit5 对应 `R-GP/R-HC/R-FL/R-LC/R-FT` 总账 15/15 关闭 |

Audit3 历史验证（2026-07-16）：fresh `/m:1` build 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。M-1/T4 的 gate、oid8 镜像、identity/presentation、human+AI DJA full-tick、split formal reset 与 `ItrRest` 保留矩阵，以及 Audit3-10/12 的扩展矩阵均包含在该结果中。该结果是针对性断言证据，不是完整 Play Mode 或逐帧等价证明；M-1 已完成 runtime self-check，不能据此扩大为全部战斗逻辑完全对齐。RISK-4 与完整对局逐帧对拍仍是验证缺口；T8 默认 `stage.dat` 部署继续由用户明确暂缓。

当前版本已在真实 `NTSD_Battle` Play Mode 重新验证 Naruto 防下跳六分身：生产输入按同一逻辑帧渐进注入 `L -> L+S -> L+S+K`，经 `InputActionMap -> CharacterInputModule -> SimInputBuffer`；tick1 到 frame271，tick12 到 frame272 且 PP `500 -> 295`、生成 oid205，tick15 到 frame273 并开始展开 oid204，tick29-32 出现 6 个 unique oid33/action307，tick38 共有 6 个 renderer 同时可见。峰值为 `max204=11`、`max205=3`、`uniqueClones=6`、`action307=6`、`maxVisible=6`，因此该项定向 Play Mode **PASS**。Audit4 后续三项定向 Play 也已全部通过，证据见本节末。T8 默认 `stage.dat` 资产部署继续暂缓。

## 第四次战斗命中/技能链审计实施进度（BATTLE-AUDIT4，2026-07-17 最终状态）

> 唯一权威为 `J:\QQFile\NTSD2.4\ntsd_release_C#`；表内所有 C# 坐标均指向该工程。以下 16 项的生产修复已经落地，fresh full `BattleRuntimeSelfCheck` 与 3 项真实角色/输入/对象链 Play Mode 均已通过，Architect 最终复核为 **PASS**。本段是 Audit4 历史快照；当时保留的 RISK-4 已由 Audit5 `R-HC-05` 关闭。完整对局逐帧 production certificate 仍未取得。T8 默认 `stage.dat` 资产部署继续按用户要求暂缓，不进入本批任务。

| 编号 | C# 权威（文件 / 方法 / 行） | Unity 差异（文件 / 方法 / 行） | 影响 | 当前状态 |
|---|---|---|---|---|
| BATTLE-AUDIT4-01 | `Simulation/GameTick.cs:1265-1297` `RunCooldownsTick`：以当前 frame 的 `Itrs` 判定是否清 `AttackExempt`，并处理 state1001 holder/wpoint/attacking 分支 | `SimulationWorld.Passes.partial.cs:943-958` `ClearAttackExemptIfCurrentFrameCannotHit` 错查 `opoints/opoint`，且没有 holder 分支 | 攻击豁免可能在仍有 itr 时被清除，或在无 itr 时残留，导致技能/武器重复命中或错误漏命中 | **生产修复已落地；Audit4 针对性矩阵 fresh PASS** |
| BATTLE-AUDIT4-02 | `Interaction/HitResolve.cs:262-510` `ApplyDamageCandidate` 是真实角色、shared-DAT 和对象的统一标准命中结算；其中 `:447-485` 统一写 `FrameDelay/AttackExempt`，state1002 随机帧与 `Vx/Vy=-4`，并处理 FlyingA 对撞 | 真实角色 `LF2CharacterHitResolver.cs:360-420` 只向 `LF2LivingObject.HitCounters` 写豁免、普通受击写 `FrameDelay=-5`，state1002 不换帧且读取 victim `PS.vx`/写 `Vy=-3.5`；shared-DAT `LF2CharacterDatHitResolver.cs:681-744` 又采用另一套行为并额外写 `WeaponState`/ProjectileFlying frame10 | 同一 C# 命中规则在两条 Unity 路径漂移；投掷物反弹、首击结束时机、飞行物互撞和不同实体壳表现不一致 | **生产修复已落地；标准命中矩阵 fresh PASS；投掷武器 Play 09:45:21 PASS** |
| BATTLE-AUDIT4-03 | `Interaction/HitResolve.cs:26-65` `ResolveCandidates` 除显式 `AbortRemainingHitPairs` 外继续消费同 tick 后续候选 | `LF2WeaponInteractionResolver.cs:38-100` 成功命中后无条件在 `:99` `break` | 武器同 tick 多目标/多候选只处理首个成功对象，与 C# 候选消费数量和顺序不一致 | **生产修复已落地；连续候选/显式 abort 矩阵 fresh PASS** |
| BATTLE-AUDIT4-04 | `Interaction/HitResolve.cs:26-65,447-485` 使用 world `ARest/VRest` 契约；`Interaction/WeaponRuntime.cs:99-215` 的 held/throw/drop 路径没有额外清零双方 arest | `LF2WeaponInteractionResolver.cs:91-99` 额外调用 `ItrArestUpdate`；`LF2WeaponHeldStateResolver.cs:92-95,108-111` 在投掷/受伤掉落时清零 weapon 与 holder 的 `ItrRest.Arest` | Unity 的第二套 arest 状态会暂时挡住命中，冷却结束后又重新命中；投掷/掉落还会改变下一次可命中时机 | **生产修复已落地；held/Arest 断言 fresh PASS；投掷武器 Play 09:45:21 PASS** |
| BATTLE-AUDIT4-05 | `Interaction/CollisionCollect.cs:14-240` 在 collect 阶段完成 pair/geometry/team 等筛选；`HitResolve.cs:26-65` consume 只校验 slot、itr index、active/CharData 与 VRest | `LF2CharacterInteractionResolver.cs:45-139` 和 `LF2WeaponInteractionResolver.cs:43-99` consume 时再次计算 allow gate、runtime itr、target/team/type/geometry/arest 等条件 | collect 后到 consume 前状态变化会让已收集候选被 Unity 二次拒绝，技能命中窗口和候选顺序偏离 C# | **生产修复已落地；SpecialAttack 已删除 live Team gate；collect 后 attacker `Team=0` 仍消费两个冻结候选并 fresh PASS** |
| BATTLE-AUDIT4-06 | `Interaction/HitResolve.cs:563-617` `ApplyKind3Grab/AlignGrabPair`：raw 写双方 frame、按整数坐标快照对位、建立 slot 关系，不附带丢武器 | `LF2CharacterInteractionResolver.cs:265-350,419-450`：限制目标必须是真实 `LF2Character`，使用 `ImmediateFrame`，坐标/计数副作用不同，并在 `:446-447` 额外 `DropWeapon` | Naruto 奔跑 `L -> K` 的 `102 -> 295/296 -> kind3 -> 297 -> 298 -> 299 -> 275...` 后续链可能在抓取帧、对位或目标壳 gate 中断，导致命中后缺少下一招 | **生产修复已落地；kind3 real/shared-DAT 矩阵 fresh PASS；Naruto 奔跑防跳 Play 09:34:36 PASS** |
| BATTLE-AUDIT4-07 | `Interaction/HitResolve.cs:1318-1529` `ApplyKind0Type3Tail` 完整覆盖 state3000/3005/3006 的关系继承、双方速度/帧/延迟、effect 尾和声音 | `LF2SpecialAttack.cs:456-519` `Hit/ApplyPostHitSelfDestruct` 只覆盖部分 3000/3006 分支，且 oid201/214 的 `DieEvent`/HP 清零后处理按 Unity CLR attacker 类型分流 | 技能对象互撞、扩张/飞行态转换、关系字段及 oid201/214 自毁方向/时机与 C# 不一致 | **生产修复已落地；type3/oid201/214 针对性矩阵 fresh PASS** |
| BATTLE-AUDIT4-08 | `Simulation/GameTick.cs:1773-1870` `SpawnStateTransitionEffects` 规定 branch 判定及每个碎片的 RNG 调用顺序（Y、X、Vy、Vx 等） | `LF2Entity.cs:3501-3564` `SpawnLateTransitionEffects/SpawnTransitionEffectBranch1/2` 的随机取值顺序和次数不同 | 即使单个特效范围相同，也会推进不同的全局 RNG 状态，继而改变后续战斗随机结果 | **生产修复已落地；现有 transition/RNG 断言随 full self-check fresh PASS** |
| BATTLE-AUDIT4-09 | `Interaction/WeaponRuntime.cs:99-155` `RunHeldObjectStep12ForPair` 每 tick raw 写 `held.Frame/Facing/FrameDelay`，朝向直接跟 holder | `LF2CharacterWeaponLinkResolver.cs:251-292` 与 `LF2WeaponHeldStateResolver.cs:32-41,139-175` 每 tick `ImmediateFrame`，并按 cover 十位再执行额外 flip | held 对象的 attacking/wait 等计数被重复重置，朝向和挂点帧可能抖动或滞后，影响螺旋丸跟手、层级与按攻击键后的动作 | **生产修复已落地；raw frame/wait/facing 矩阵 fresh PASS；Naruto 螺旋丸 Play 01:10:34 PASS** |
| BATTLE-AUDIT4-10 | `Interaction/HitResolve.cs:382-406,889-906` 受击帧按 attacker/victim 的 `Facing` 关系选择 | `LF2CharacterHitResolver.cs:581-596,673-680` 与 `LF2CharacterDatHitResolver.cs:954-968,1011-1016` 通过 attacker 相对 X 推断方向 | 交叉、瞬移、同 X 或攻击者背向出招时会进入错误的正面/背面受击帧 | **生产修复已落地；real/shared-DAT facing 矩阵 fresh PASS** |
| BATTLE-AUDIT4-11 | `Frame/FrameTick.cs:242-252` 要求 first op 同时满足 `Kind>0 && Oid>0`；`:414-419` 为 oid5/52 初始化 `Hp/HpMax/Hp3/Pp=10/10/10/5` | `LF2ObjectPointFactory.cs:139-145` first-op 总闸门漏 `oid>0`；`:536-547` 的 oid5/52 初始化字段不完整 | 无效 first-op 可能错误放行后续生成；oid5/52 技能实体初始生命/PP 契约错误 | **生产修复已落地；first-op 与 oid5/52 初始化矩阵 fresh PASS** |
| BATTLE-AUDIT4-12 | `Interaction/HitResolve.cs:1084-1147` `RecordDamageEffectSound/RecordStandardHurtSounds/RecordAlternateHurtLeadSound` 覆盖 effect cue、effect1 附加声、attacker/victim 武器声音及 oid 条件 | `LF2CharacterHitResolver.cs:439-446` 与 `LF2CharacterDatHitResolver.cs:762-767` 主要只播通用 `SFX_001/006`；shared 路径部分判断还使用 `type_sub` 代替 oid（`:276-282`） | 命中确认的声音组合、声源位置和特定技能反馈与 C# 不一致 | **生产修复已落地；声音记录随 Audit4 full self-check fresh PASS** |
| BATTLE-AUDIT4-13 | `Frame/FrameTick.cs:13-216,218-230` 在规定 frame_tick 边界统一 `QueueFrameSound`；`SpawnFromOpoint` 仍按正常实体生命周期生成对象 | `LF2SpecialAttack.cs:96-98,230-231` 存在类内独立 frame sound；`LF2ObjectPointFactory.cs:331-340,467-477` 对 `pic=999,wait=0,next=1000` 直接播放并立即回收 | 同一声音可能在不同 pass 播放、重复或丢失；pic999 对象不再经历 C# 的注册、frame tick 和回收边界 | **生产修复已落地；living/weapon/SpecialAttack `PendingSounds` 单次精确断言与 tick/reset 清理 fresh PASS** |
| BATTLE-AUDIT4-14 | `Interaction/HitResolve.cs:503-507,1150-1195` 对成功 kind0 统一 `RecordKind0Hit`，不以 effect6/23 排除 spark 记录 | shared-DAT `LF2CharacterDatHitResolver.cs:770-773` 显式跳过 effect6/23 的 `SpawnSpark`，真实角色路径又在 `LF2CharacterHitResolver.cs:449-450` 无该排除 | 同一命中在真实角色与 shared-DAT 壳的 spark 记录数量/随机数消费不同 | **生产修复已落地；effect6/23 统一 spark 断言 fresh PASS** |
| BATTLE-AUDIT4-15 | `Simulation/GameTick.cs:142-147` 在交互后的 late update 推进 holder frame；`Interaction/WeaponRuntime.cs:99-155` 定义 held frame/挂点/整数位置契约。Unity 必须在 late holder 切帧后刷新该契约的表现结果 | `HeldObjectProcessAll` 早于 late `SimFrameTick`，holder 首 tick 切帧后 held 仍使用旧挂点；renderer 刷新也没有保证 holder 后于 held 的同 tick 可见顺序 | 螺旋丸已生成但首 tick 位置滞后、移动不跟手或层级/攻击表现落后一拍 | **生产修复已落地：late frame 变化后只调用纯 `SyncHeldPose`，不重复 step12，并按 holder→held 刷新 renderer；focused self-check 01:07:01 PASS；Rasengan Play 01:10:34 PASS** |
| BATTLE-AUDIT4-16 | `Interaction/CPointRuntime.cs:58-85` 按 `PrevFrame2` 与持久 `CaughtIdx/CatcherIdx` 维持抓取链；`Runtime/NtsdEntityRuntime.cs:178-190` 只在完整实体 reset 时清关系字段 | `LF2CharacterCatchResolver` 的普通 `state_exit` 与 `LF2Character.ResetStateRuntime` 提前清 `CaughtSlotIndex/CatcherSlotIndex`；`276 -> 277` 后下一 tick 的 cpoint 仍读 `PrevFrame2=276`，却因关系已清而强制 frame0 | Naruto 奔跑防跳抓取链在 276 后中断，缺失 277/278/279 与 86/87/88 后续招 | **生产修复已落地：普通 state transition 保留 catch link，完整实体 Reset 仍清；fresh full self-check 09:26:55 PASS；Running Play 09:34:36 PASS** |

### Audit4 fresh 验证证据（2026-07-17）

- 当前 Unity Editor PID `11540` 完成 fresh script compile，Console 为 **0 C# error**。
- 最终 freshness 链：`BattleRuntimeSelfCheck.cs` source/test `01:39:46` < `Library/ScriptAssemblies/Assembly-CSharp.dll` `09:26:23` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `09:26:55`；fresh full self-check **PASS**。
- 早一轮 held late pose focused freshness 链为 source `01:05:07` < DLL `01:06:22` < result `01:07:01`，结果 **PASS**；最终 full PASS 已再次覆盖该回归。
- Architect 复核后新增的 SpecialAttack 候选矩阵已进入本次 PASS：生产 consume 删除 live `Team` gate；候选在 collect 后把 attacker `Team` 改为 `0`，仍按冻结的 geometry/team 连续消费两个目标；显式 oid300 abort 仍会停止后续候选。
- SpecialAttack frame sound 断言精确要求 `PendingSounds.Count == 1`，且 Cue、WorldX、Tick 均匹配；living/weapon 分支、下一逻辑 tick 清空及 `ResetRuntimeState` 清空也在同次 PASS 中。
- Naruto 防前跳螺旋丸 Play `01:10:34` **PASS**：frame240 / oid434 / link 均成立；change runtime/holderVisual/heldVisual=`5/5/5/5`，move=`9/9/9/9`，sorting `526 -> 527`；攻击链 `20 -> 257 -> 258 -> 259`，oid434 `396 -> 397`。
- Naruto 奔跑防跳 Play `09:34:36` **PASS**：完整链为 `9 -> 102 -> 295(prev2)/297(pn) -> 298 -> 299 -> 275 -> 276 -> 277 -> 278 -> 279 -> 86 -> 87 -> 88`，victim 保持 frame130/catch；oid33 `current311/pn310` 是 wait0 的正确观测口径。
- 投掷武器 Play `09:45:21` **PASS**：使用生产 oid120 / hold / double-D / D+J；HP 只在 tick17 从 `500 -> 489` 下降一次；weapon state1002/frame41 后同 tick 切到 frame7/state1000，`AttackExempt=4`；跨 35 tick 冷却归零并落地，HP 无二次下降。
- 当前 Unity 自动生成的 dotnet `.csproj` 仍包含 35 个已删除历史源文件，最终 `dotnet build` 被 `CS2001` 阻塞。不得把此前的 dotnet 0 error 冒充为 Audit4-16 后的最终证据；最终有效编译证据是上述 Unity fresh script compile 0 C# error。

### Audit4 实施顺序与剩余边界

- **已完成的串行核心链**：`01 -> 02 -> 03/04 -> 05` 已按依赖顺序收口，cooldown、标准命中和 candidate 消费矩阵已进入 fresh PASS。
- **已完成的独立轨**：`07`（SpecialAttack type3 tail）、`08`（状态转换 RNG）、`09`（held 同步）生产修复已合并并通过已有断言。
- **已完成的第二阶段**：`06/10/12/14` 的命中尾与 `11/13` 的 opoint/声音生命周期生产修复已落地并通过已有断言。
- **Play 抓出的后续修复**：`15` 收口 late holder 切帧后的 held pose/renderer 同 tick 刷新；`16` 收口普通 state transition 错清 catch link。两项均已进入最终 full self-check，并由对应 Play 场景验证。
- **目标 Play Mode**：Naruto 奔跑 `L -> K` 后续招、Naruto 防前跳螺旋丸 held/层级/跟手/攻击链、投掷武器首击后的单次命中/Arest 时间线均已 **PASS**。
- **仍保留的审计/验证边界**：完整对局逐帧对拍尚未完成，RISK-4 仍是待审计风险，因此不能将 Audit4 本批验收扩大成“全部战斗逻辑完全对齐”。
- **非行为性清理债**：`WeaponSpawner` 仍有历史非 C# 注释，F9 debug 说明也存在与当前 C# 唯一权威措辞冲突的历史文字；F7-F9/debug 已按 `AGENTS.md` 排除正式战斗 backlog，不计为生产逻辑差异。

## 第五次全量逐帧审计（BATTLE-AUDIT5，2026-07-18 风险账收口）

### Audit5 权威、废止声明与验收口径

- 唯一战斗逻辑权威是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。Audit5 的差异定性、修复方向和对拍预期只来自该工程的正式 C# 调用链。
- 本文此前所有依赖其他旧来源的“已对齐”“已关闭”或“仅作映射参考”结论，在 Audit5 中一律废止为当前权威证据；相关实现只能作为 Unity 现状或历史回归基线，必须按 C# 重新核验后才能恢复完成状态。
- T8 默认 `stage.dat` 部署继续按用户要求暂缓；对拍场景默认 `stageFixture=false`，不会把默认资产缺失混入当前战斗逻辑差异。
- Audit5 的最终目标是在双方各自正式读取与 Unity 适配后，基于语义等价的 runtime 输入、同场景、同 seed、同 `FrameInputSet`，逐逻辑 tick 比较 400 个固定 runtime slot、world、RNG、arest/vrest、stats、sound events；这里的 400-slot parity 口径仅适用于 `Authority400` 兼容模式和该历史 schema。`Extended` 模式不能复用旧 certificate，必须新增分页 slot、generation handle 和稀疏 rest projection/schema。若继续签发 production parity certificate，也应以该语义 runtime 与 full/full trace 为准，不要求 raw DAT 文件或 manifest 相等。

状态词严格区分如下：

| 状态 | 只证明什么 | 不能据此声称什么 |
|---|---|---|
| 逻辑已写 | 生产代码中已落地目标修改 | 不能证明可编译或行为正确 |
| isolated/目标编译 0 error | 当前隔离编译范围没有诊断 | 不能代替 Unity fresh script compile |
| fresh 编译 | 最新 Unity 脚本程序集晚于目标源码且 0 error | 不能代替 self-check 或真实行为 |
| full self-check PASS | 最新程序集上的现有自动断言通过 | 不能覆盖未写断言、Play Mode 或双端逐帧等价 |
| diagnostic trace 一致 | 使用诊断夹具隔离后的已比较 tick/domain 一致 | 不能自动关闭未覆盖风险，也不能代替必要 Play Mode |
| production certificate | 适配后的语义 runtime 输入声明成立，full/full 全 tick、全 domain 相等 | 可作为聚合对拍证据；目前尚未取得，且 raw DAT/manifest 相等不是前置条件 |

### 静态审计总账与当前修复层级

三份报告按不同调用链分区，原始总账为 **74 个确认差异簇 + 15 个 trace 风险**。确认项现为 **74/74 逻辑实现 + focused/full `BattleRuntimeSelfCheck`**，原 15 项风险现为 **15/15 已关闭**。该计数是差异簇而非代码行；`BATTLE-AUDIT6-01/02` 仍作为原总账后新增且已关闭的项目单列，CP-NV1/2/3 与 STEP10 是原总账既有项重开后重新关闭，不另改分母。风险账关闭不等于取得任意对局、全输入、长时间 production certificate。

| 分区报告 | 静态覆盖与发现 | 当前实现与 fresh 证据 | 风险账状态 |
|---|---|---|---|
| GameTick / Physics | `GameTick.cs` 正式对局主干与 `Physics.cs` 全分支 100%；21 确认 + 3 风险 | `GT-01..15`、`PH-01..06` 共 **21/21 逻辑已写并进入 fresh full PASS** | `R-GP-01..03` **3/3 关闭** |
| HitResolve / CollisionCollect | 两个权威入口全分支；33 确认 + 6 风险 | `C-01..33` 共 **33/33 逻辑已写并进入 fresh full PASS** | `R-HC-01..06` **6/6 关闭** |
| Frame / lifecycle | 25/25 权威方法及 reset/registry/cooldown 依赖；20 确认 + 6 风险 | `FL-01..06`、`FT-01..04`、`OP-01..05`、`LC-01..05` 共 **20/20 生产实现与 focused/full self-check 通过** | `R-FL-01..03`、`R-LC-01..02`、`R-FT-01` **6/6 关闭** |

最终 combined freshness 链为：`BattleRuntimeSelfCheck.cs` source `2026-07-18 01:06:21.499` < Unity `Assembly-CSharp.dll` `01:07:21.125` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `01:07:52.834`，结果为 **PASS**。原 3 个受控 P2 已补强并关闭，Architect 最终复核为 `P0/P1/P2=0`。这仍不是任意对局、全输入、完整逐 tick 的 production certificate。

原 3 个受控 P2 的关闭证据：

- **HC-04 完整 step6 整链**：真实 `collect -> wrong loop 不消费 -> post consumer 消费` 链已进入 self-check，并覆盖 current type3 非武器负例，确认负例不产生 pickup/link/计数等副作用。
- **missing-definition 完整分派链**：Character 与 Weapon 两类 missing-definition shell 均覆盖候选收集、错误循环不消费、正确循环消费及 tail 结算，不再停留在 helper 级断言。
- **Interaction resolver helper 去漂移**：`LF2CharacterInteractionResolver` 的本地类型 helper 仅单行委托中央 `LF2Entity.ResolveCurrentDataObjectType`，不再维护第二份类型判定逻辑。

### 原 15 项 trace 风险关闭总账

| 分区 | 风险 | 状态与关闭证据 |
|---|---|---|
| GameTick / Physics | `R-GP-01` | ✅ 已关闭：fresh 双端 2 tick frame/wait trace；tick1 slot0 `frame=0, wait=37, FWC=11, HitStop=75`，tick2 `frame=5, wait=37, FWC=0, HitStop=74`，双方一致 |
| GameTick / Physics | `R-GP-02` | ✅ 已关闭：production 扫描确认可部署对象 `mass > 0`，static close |
| GameTick / Physics | `R-GP-03` | ✅ 已关闭：中央 active filter 覆盖 pass 输入集合与失活实体边界 |
| HitResolve / CollisionCollect | `R-HC-01` | ✅ 已关闭：确认差异后修复 zero-width strict overlap；Unity 适配后扫描锁定 90 项非正宽几何，并按权威严格交叠语义收口 |
| HitResolve / CollisionCollect | `R-HC-02` | ✅ 已关闭：oid999 `next` 闭包 14 帧均为零有效 geometry，`IsPureTransitionSmoke` gate 不吞有效碰撞 |
| HitResolve / CollisionCollect | `R-HC-03` | ✅ 已关闭：current OID/type 统一，gate A/B 正负路径均有覆盖 |
| HitResolve / CollisionCollect | `R-HC-04` | ✅ 已关闭：pickup 使用 current DAT type/OID，移除 CLR `LF2WeaponBase` cast 前置；真实 step6 collect、错误循环不消费、post consumer 消费及 current type3 负例均已覆盖 |
| HitResolve / CollisionCollect | `R-HC-05` | ✅ 已关闭：fixed slot 与 slot reuse 边界已验证 |
| HitResolve / CollisionCollect | `R-HC-06` | ✅ 已关闭：碰撞/命中整数坐标路径已验证 |
| Frame / lifecycle | `R-FL-01` | ✅ 已关闭：四类 weapon 矩阵覆盖 current DAT 分派与 frame lifecycle |
| Frame / lifecycle | `R-FL-02` | ✅ 已关闭：current-DAT boomerang 路径已验证 |
| Frame / lifecycle | `R-FL-03` | ✅ 已关闭：raw empty fixed slot 的 `CatchTimer=100` side effect、后续占槽清理与 world reset 已验证 |
| Frame / lifecycle | `R-LC-01` | ✅ 已关闭：pooled instance 的 snapshot/cache reset 已验证，旧 DAT 不泄漏到复用实例 |
| Frame / lifecycle | `R-LC-02` | ✅ 已关闭：StableId alias、注销与复用边界已验证 |
| Frame / lifecycle | `R-FT-01` | ✅ 已关闭：这是已关闭 `FT-01` 的 trace 验证债，不是重复生产风险；现有 fresh trace/self-check 已补证 |

R-GP-01 freshness：authority source `2026-07-18 00:11:23` < authority DLL `00:11:49` < trace `00:12:07`；Unity source `00:11:23` < Editor DLL `00:12:22` < trace `00:13:44`；compare `00:14:02` 返回 `status=equal-diagnostic`、`ticksCompared=2`、`firstDifference=null`。该证据关闭 R-GP-01，但只覆盖这 2 tick 的已观察域。

最终 PASS 前的失败均保留为诊断证据，不以最终结果淡化：

1. `C-05` 首先暴露 oid300 no-redirect 未继续 frozen pairs；根因是 CLR `LF2SpecialAttack` 覆盖了 current Character-DAT 分派，修正为 current-DAT 优先。
2. `BATTLE-AUDIT3-12` real `LF2Weapon` damaged release 未进入 dvx；根因是 GT current-DAT 新增的 `wrapper.type_sub` fallback 过宽，将未填 `type_sub` 的 real weapon 误判为 Character。已撤销该 fallback，并让 fixture 完整注册 `GameDataManager` 类型。
3. state8000/current type6 检查曾期待 landing 后 `Unk31C=-1`；权威与 production 实际在同一个 late pass 先 landing 写 `-1`，随后 weapon cleanup 归 0 并释放 slot。旧 fixture 停在中间态，现按最终 `0 + slot released` 断言。
4. `C-12` 的 `YInt<0/Vy>=0` fixture 朝向预期错误；权威先补 `KnockbackX=+5`，再为朝右 victim 选择 frame186 / `FallingBack`。修正 fixture 后 actual/shared 两路径通过。
5. 后续 Architect 对 GameTick / Physics 抓出的 `GT-04/GT-07/PH-02` 以及对 Weapon 抓出的 C-26/C-27 P1 均已按权威 C# 收口；原 3 个受控 P2 补强后最终复核为 `P0/P1/P2=0`。这些复核和 self-check 不能替代生产逐 tick 或目标 Play Mode。

### 原始总账后新增确认差异（BATTLE-AUDIT6，2026-07-17）

以下两项是在 Audit5 原始 **74 个确认差异簇 + 15 个风险** 建账后，由唯一权威 C# 调用链重新核实出的新增差异。它们不并入 74 的分母；生产、focused/full self-check 与最终 Architect 复核现均已收口。

| 编号 | C# 权威 | Unity 现状 / 修复 | 影响与当前证据 |
|---|---|---|---|
| BATTLE-AUDIT6-01 | `SimulationTickDriver.cs:42-47,93-116` 只把本 tick `SimulationFrameInput` 交给 `InputRuntime.PollHumanInput`；`InputRuntime.cs:611-624` 仅 roll 当前输入、写键值、tick cooldown、apply edges。正式 combo/direct/action/velocity 消费位于 `GameTick.cs:52-77`：`RunCooldownsTick -> marker/M-1 -> NeedClearInput gate -> GameTick>1 时 ApplyCharacterInputPass` | Unity 已拆分 human poll 与 unified character input，正式顺序改为 poll → cooldown/M-1 → clear/tick gate → character input；AI 同走 gate 后统一入口 | **已关闭**；矩阵覆盖 tick1、`NeedClearInput`、oid51 frame85 gate 外延迟 split、AI 顺序，以及 CLR character 在 current DAT 转为 non-character 后仍轮询 roster human 输入但不错误执行 character action。该 transformed-human P1 已补齐，combined fresh PASS |
| BATTLE-AUDIT6-02 | `InputRuntime.cs:826-893` 将九组 combo 复制为 locals；在 `frame null / comboDja != 3`、oid6 DjaGuard、成功/失败目标 frame jump、`Unk328==1` 四类 early-return 中均不执行 `:885-893` 的 locals 回写，只有正常尾路径才统一 commit | Unity 已按 C# 让 early-return 保留进入的 private/runtime combo locals，正常尾路径才 commit | **已关闭**；缺 target、有效 target、oid6 guard、`Unk328` 与正常尾 commit 的负向/正向覆盖进入 combined fresh PASS |

#### oid51 DJA 旧检查的反权威点

- 旧 `CheckOid5152DjaReleaseTriggersSameTickSplit` 假设 PostCooldown input 会在 M-1 前消费 DJA 并同 tick split；这正是 Unity 当前错误 pass-order 的产物，不能继续作为权威验收标准。
- C# 正式顺序中，M-1 先把 split cooldown `30 -> 29`；之后角色输入消费将 combo 清零，并在 state2 进入 frame85。frame85 落在 M-1 的 `[9,260]` gate 内，因此同 tick或下一 tick都不能自然 split。
- 旧 synthetic fixture 缺少 frame85，导致 Unity 跳转失败并停在 frame9，进而制造了“同 tick split”的假阳性。修复测试时必须补齐 frame85，并在离开 `[9,260]` gate 后验证延迟 split；不得通过删除 gate 或继续提前消费输入让旧断言变绿。
- 旧 same-tick split 断言与缺 frame85 fixture 已按权威重写；Audit6 两项和 Frame / Lifecycle 20 项均由 `21:57:40` combined fresh full PASS 覆盖。

### DAT 诊断统计与 production trace 口径

`Temp/NTSDParity/data-audit-v3-required.json` 对 137 个权威 OID 的结构化结果是：

- 34 个 OID 相同，66 个 OID 不同，37 个 OID 在 Unity 侧 raw 结构审计中未匹配，解析错误为 0。
- 差异类别计数包括 frame 126、碰撞 geometry 31、sound cue 155；这些是字段/类别差异计数，不是额外 OID 数量，也不互斥。
- 权威 production battle-logic manifest 为 `41c088d2...0375`，Unity production manifest 为 `6b34e118...332a`，当前不相等。
- 上述 34 / 66 / 37 与 manifest 只描述两套 raw DAT 在不同读取方式和 Unity 适配下的表示差异，保留作诊断统计；它们不是战斗逻辑阻塞、backlog 或资源部署清单，不需要把文件或 manifest 改成相同。
- `Temp/NTSDParity/compare-v3-full-final.json` 因旧工具按 raw manifest 做 header gate，在 tick 比较前返回 `status=different`、`certificateEligible=false`、`ticksCompared=0`。这说明该次工具运行没有 production parity certificate，不代表生产战斗逻辑失败。未来 certificate 必须改以适配后的语义 runtime 输入和 trace 为准，raw DAT/manifest 相等不得作为前置条件。

“37 missing Unity”不能解释为 Unity 缺少 37 个必须补部署的生产 DAT；它只是当前 raw 结构审计在 Unity 适配表示中未找到一一对应项。diagnostic runner 仍可使用权威 DAT headless 夹具隔离特定代码路径，但夹具结果只能证明明确覆盖的行为，不能外推到任意对局。

### v3 trace 工具与当前诊断结果

- `Tools/NTSDParity/NTSDParity.csproj` 已提供 `data-audit`、`trace-authority`、`compare` 和 trace self-test；Audit5 工具构建为 0 warning / 0 error。
- schema v3 逐 tick 校验 input、RNG、world、400 slot commitments、arest/vrest、stats 与 sound events；该 400-slot schema 仅是 `Authority400`/历史 parity schema。`Extended` 模式需新的分页 slot、generation handle 和稀疏 rest projection，不能伪装成旧 authority certificate。certificate 应比较双方正式读取/适配后的语义 runtime 与 full/full trace。最新 `Temp/NTSDParity/trace-compare-self-test-iter7.json` 为 **20/20 PASS**，覆盖连续 tick、空 trace、body/hash/slot commitment 防篡改、dense human input、diagnostic 显式 opt-in、diagnostic 永不签发 certificate、strict/fixed-world camera profile及非 camera world 字段严格比较。
- iter7 authority/Unity full-detail diagnostic trace 已生成。`Temp/NTSDParity/compare-v3-diagnostic-full-iter7.json` 返回 `status=equal-diagnostic`、`ticksCompared=6`、`firstDifference=null`、`comparisonProfile=fixed-world-camera`、`diagnosticComparison=true`、`certificateEligible=false`、`certificateClass=none`。
- iter7 的 authority 端使用 production authority DAT，Unity 端明确使用 `authority-dat-diagnostic` 夹具；该 6 tick 结果只证明对应样例的已观察域一致。原 15 项风险已由各自证据逐项关闭，不是由 iter7 一次性关闭；iter7 和 R-GP-01 的 2 tick diagnostic 均不能被扩大为全战斗逐帧等价或 production certificate。

### 报告与证据索引

- `.omc/research/game-tick-physics-audit-20260717.md`：GameTick / Physics，21 确认 + 3 风险。
- `.omc/research/hit-collision-audit-20260717.md`：HitResolve / CollisionCollect，33 确认 + 6 风险。
- `.omc/research/frame-lifecycle-audit-20260717.md`：Frame / lifecycle，20 确认 + 6 风险。
- `Temp/NTSDParity/data-audit-v3-required.json`：137 OID 审计与 production manifest。
- `Temp/NTSDParity/compare-v3-full-final.json`：旧 raw-manifest header gate 的诊断结果，不是战斗逻辑失败证据。
- `Temp/NTSDParity/authority-v3-full-iter7.jsonl` 与 `Temp/NTSDParity/unity-trace-v3-diagnostic-full-iter7.jsonl`：iter7 双端 6 tick full-detail trace。
- `Temp/NTSDParity/compare-v3-diagnostic-full-iter7.json`：`equal-diagnostic`、6 tick、无首差但不具 certificate 资格。
- `Temp/NTSDParity/trace-compare-self-test-iter7.json`：20/20 防护、输入与 profile 用例 PASS。

### 下一执行顺序

1. 原 15 项风险账已 15/15 关闭，不再把“逐项关闭 15 风险”列为下一步。
2. 若继续建设 production certificate，扩展到双方正式读取/适配后的语义 runtime、更多真实输入和长时间 full/full trace；2 tick diagnostic 只作为 R-GP-01 定向证据。
3. 保持 source < DLL < result/trace freshness；不处理 raw DAT 文件或 manifest 差异，也不得把 diagnostic 写成 production certificate。
4. T8 默认 `stage.dat` 部署继续独立暂缓，不得为 trace、certificate 或测试私自部署默认资产。

**Audit5/Audit6 历史结论（已被顶部 BATTLE-AUDIT7 当前结论取代）：原始确认项曾达到 74/74 逻辑实现 + focused/full self-check，原 15 项 trace 风险曾达到 15/15 已关闭；Audit6、CP-NV1/2/3 与 STEP10 也保持关闭，原 3 个受控 P2 亦已补强关闭。该批 fresh full self-check 为 source `01:06:21.499` < DLL `01:07:21.125` < result `01:07:52.834` PASS，Architect 当时为 `P0/P1/P2=0`。R-GP-01 fresh 2 tick compare 为 `equal-diagnostic`、无差异；它只能证明这 2 tick 的已观察域，不能扩大为任意对局、全输入 production certificate，更不能覆盖 BATTLE-AUDIT7 新发现。34 equal / 66 different / 37 missing Unity 仍只是 raw DAT 适配诊断，不是阻塞或 backlog；raw DAT/manifest 相等不是 certificate 前置。T8 默认 `stage.dat` 部署继续独立暂缓。**

## BATTLE-AUDIT9 详细差异冻结表（2026-07-18）

本节是“先盘点、后修复”的冻结边界。本轮只合并并去重四份只读报告中的未收口项，不修改生产代码，也不把历史 PASS、静态等价或单个 self-check 扩大成完整对齐结论。后续修复必须严格按本清单逐项进行；在清单冻结后，按清单实施修复的阶段尚未开始。

### 冻结计数

| 类别 | 数量 | 口径 |
|---|---:|---|
| 正式战斗 runtime 差异 | **9** | 5 个 Framework/pass/bootstrap/reset + 4 个 lifecycle/presentation；均为当前源码可确认且未修复 |
| 工具/trace 差异 | **1** | `RT.CHECK.01` parity snapshot projection；不等同于 runtime 语义差异 |
| authority-unresolved 待确认（BATTLE-AUDIT9 历史冻结） | **12** | `UNRES.01-05`、`DEP.INT.01-04`、`DEP.WORLD.01`、`DEP.RNG.01`、`DEP.DATA.01`；当前 code-only 定性数量为 0，不得作为现状计数 |
| Play Mode 未验证场景 | **4** | Naruto 防下跳、Naruto 防前跳螺旋丸、Naruto 奔跑防跳、投掷武器首击/持续命中；是验收缺口，不额外重复计数 |

报告依据：`.omc/research/full-diff-inventory-framework-20260718.md`、`.omc/research/full-diff-inventory-input-interaction-20260718.md`、`.omc/research/full-diff-inventory-lifecycle-presentation-20260718.md`、`.omc/research/reaudit-open-differences-20260718.md`。

### 正式战斗差异（冻结，未修复）

| ID | 权威 C# 调用链 | Unity 对应链 | 触发条件 / 预期与实际 | 分类 / 证据 |
|---|---|---|---|---|
| `FW-FLOW-01` | `J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:53-67`：先 cooldown/step gate，再 `postCooldownInput` | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:32-43`、`RunFrameAdvancePhase`：input 观察早于 `VrestTickAll` | 普通非-results tick 且输入边沿与 ARest/AttackExempt 同 tick 到期；预期先递减再读输入，实际 Unity 先读输入 | confirmed-difference；静态调用链，未修复、未运行时验证。报告：framework inventory |
| `FW-FLOW-02` | `GameTick.cs:56-67`：清/设置 `BattleStepGate44905C`，mode=2 转 step-wait 并抑制 input | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:272-281`、`NTSDBattleTickSystem.RunReleaseTick` 无对应转换/抑制 | 单步/慢速 `BattleStepMode=1/2`；预期 gate 控制顺序，实际 Unity 无条件继续 input | confirmed-difference；静态调用链，是否可达仍需 production fixture。报告：framework inventory |
| `FW-BOOT-01` | `DirectBattleBootstrap.cs:138-140` 写 `Unk344=battleTeam`、`HolderCopy=slot` | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 未显式写 `Unk344`/`HolderCopySlot` | 初始玩家参与统计、holder-copy 或相关 AI/技能分支；预期 identity 字段完整，实际可能为默认 `0/99` | confirmed-difference；静态字段契约，未修复、未运行时验证。报告：framework inventory |
| `FW-BOOT-02` | `DirectBattleBootstrap.InitializeBattleStats:224-244`：difficulty HP bonus/cap、PPBound、respawn、HitStop、速度、输入边沿、Cd 全集 | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 主要依赖 `Initialize` 默认值，仅显式写部分 team/位置/速度/HitStun | 非默认 difficulty、DAT `Hp3` 非默认、pool/rebootstrap 或初始边沿非零；预期完整字段集合，实际依赖隐式 reset/default | confirmed-difference；字段契约缺失，未修复、未运行时验证。报告：framework inventory |
| `FW-RESET-01` | `BattleCore/Simulation/SimulationWorld.Passes.cs:13-70` reset 不调用 `NtsdRng.Srand`，进程级 RNG 延续 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:138-151` 每次 reset `Rng.Seed(0x4E545344u)`，之后 config seed 再播种 | 同进程重开/重赛后发生随机掉落或 stage spawn；预期按权威入口延续/显式播种，实际 Unity 增加 reset 播种边界 | confirmed-difference；静态调用链，播种归属仍有 authority-unresolved 依赖，未修复。报告：framework inventory |
| `LP-01` | `BattleCore/Interaction/WeaponRuntime.cs:169-212,287-303` generic held 正式 throw/kind3 都写 `ReleaseTick=currentTick` | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` generic throw/kind3 通过 `ClearLinks(..., stampReleaseTick: true)` 写当前 tick | 非 `LF2WeaponBase` CLR 壳但按 DAT held 参加 step12，且 `Dvx != 0` 或 kind3；预期清 link 同时写 tick | confirmed-difference；**代码已写 / `CheckAudit9GenericHeldReleaseTickContracts` self-check verified / Play-unverified**。报告：lifecycle inventory |
| `LP-02` | `src/Host/SdlBattleRenderer.cs:476-497` 先 `ZInt`，同 Z 按 runtime slot 升序稳定绘制；随后按 `Shadow -> Entity -> EntityOverlays -> HitRecords` 展开 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` compact presentation sort、`Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` `ForceRefresh`；`LF2Sprite.cs` 表现刷新 | 两个实体同 `ZInt` 且无额外 cover；按 `(ZInt, runtime slot)` dense rank。四槽为 `Shadow/Entity/Overlay/HitRecord=0/1/2/3`；Unity Overlay 子序存在但对应 per-entity consumer 未实现。真实双实体 renderer 检查为 `Shadow(A)=0`、`Entity(A)=1`、`Shadow(B)=4`、`Entity(B)=5` | confirmed-difference；排序代码/self-check/architect verified，Overlay 仍为 confirmed blocker，Play-unverified。legacy 后端 guard 为 `8192` materialized active entities；移动端 `1000` 安全，DesktopExtended 在中央后端完成前受此临时表现上限约束。报告：lifecycle inventory |
| `LP-03` | `BattleCore/Interaction/WeaponRuntime.cs:169-212` 释放只写逻辑位置/速度/owner/link/ReleaseTick，层级由 `ZInt/slot` | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:77-98,391-402` 额外写 `Runtime.Zz=1`，由 `LF2Entity.GetRenderSortingOrder` 加入排序 | 正式投掷起始帧；预期由权威 Z/slot 决定，实际 Unity 额外上抬一个 sorting order | confirmed-difference；静态表现契约，未修复、未 Play。报告：lifecycle inventory |
| `LP-04` | `src/Host/SdlBattleRenderer.cs:519-548`：实体/阴影分别按负 `HitStop` 阈值和四拍相位隐藏 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:416-448`、`LF2ObjectRenderer.cs:206-243` 已接入实体/阴影各自的 `HitStop` gate | 实体进入负 HitStop 闪烁/隐藏区间；预期按实体/阴影不同阈值隐藏 | confirmed-difference；**代码已写 / `CheckHitStopPresentationGates` self-check verified / Play-unverified**。报告：lifecycle inventory |

### 工具差异

`RT.CHECK.01`：权威 `BattleCore/Entity/CharacterSync.cs:796-877,173-317` 是内部 runtime snapshot；Unity `Assets/NTSD/Scripts/Simulation/BattleParitySnapshot.cs:385-542` 输出带 alias/default/reset-slot 的 trace projection。两者 schema 不同，但当前 runtime 语义未证明不同；分类为 **trace/validator adapter difference**，不是正式战斗 runtime 差异。`reaudit-open-differences-20260718.md:44-56` 的 focused projection 已通过，但不得要求 JSON 形状相等来替代 runtime 对齐。

### Play Mode 未验证场景

以下四项保留为“本轮未验证”，不把旧日志自动视为本轮 freshness，也不把它们重复计入上面的 9 个正式差异：

1. **Naruto 防下跳六分身**：权威 `InputRuntime.RunCombo -> FrameTick` opoint 递归 oid205/204/oid33；Unity `CharacterInputModule -> SimInputBuffer -> SimulationWorld`。预期六个 clone、关系字段、renderer 可见且生命周期不提前结束。
2. **Naruto 防前跳螺旋丸**：权威 combo frame240 -> oid434/action396/397 -> held wpoint/step12 -> 257/258/259；Unity `LF2ObjectPointFactory`、`LF2WeaponHeldStateResolver`、`LF2CharacterWeaponLinkResolver`。预期层级、整数挂点、跟手和攻击键驱动 held DAT。
3. **Naruto 奔跑防跳后续招**：权威 running frame102 -> kind3/cpoint 295-299 -> 275-279 -> 86-88；Unity `LF2CharacterInteractionResolver`/catch/link pass。预期命中后下一招和 caught/catcher link 均持续。
4. **投掷武器首击与持续命中**：权威 `WeaponRuntime` release -> `HitResolve` -> ARest/VRest/AttackExempt；Unity `LF2WeaponHeldStateResolver`/`LF2WeaponReleaseFlowResolver`/hit resolver。预期首击只结算一次、ReleaseTick 与 rest 窗口一致。

报告依据：`full-diff-inventory-input-interaction-20260718.md:77-86`、`full-diff-inventory-lifecycle-presentation-20260718.md:97-118`。

### 明确排除与当前阶段边界

- F1-F7 已达到 **source/static + focused self-check 闭合**，但不等于全部 Play Mode 已验证；本冻结不把它们重新计入开放正式差异。
- 12 个 authority-unresolved 是历史冻结时的原始计数；BATTLE-AUDIT11 已将其全部定性，当前 code-only scope 下为 0。未修复的 confirmed code differences 仍不得视为已对齐。
- raw DAT/manifest 表示差异不属于当前差异清单；T8 默认 `stage.dat` 部署按用户要求继续暂缓。
- fixed-world camera 是用户批准的 Unity adapter；不得恢复 C# camera_x 表现链，也不得将 camera offset 写回 runtime 真值。
## 2026-07-22 — C++ 跳跃水平动量例外核验

- **问题**：移动中起跳后，Unity 未稳定保留起跳前的水平移动速度；按住方向进入普通跳跃时也可能读不到该方向。
- **本项行为依据（用户明确指定的例外）**：`J:\QQFile\NTSD2.4\ntsd_release\src\entity\frame_advance.cpp` 的 frame 212 初始化。C++ 在进入 212 时始终写 `vy = jump_height`；只有右/左或上/下为互斥按住态时才以 DAT 的 `jump_distance/jump_distancez` 覆盖对应轴，否则保留起跳前 `vx/vz`。空中不执行地面摩擦。
- **共同根因**：C# `src/BattleCore/Simulation/GameTick.cs` 与 Unity `SimulationWorld.SerialTickAll` 都曾在 frame advance 前清除当前 action/directional keys。这样 late `frame_tick` 的 211 -> 212 初始化看不到本 tick 的按住态，属于 C# 移植与 Unity 共有、但 C++ 表现正确的差异。
- **Unity 修正**：`SimulationWorld.SerialTickAll` 不再在 `SimTransit` 前清当前键。输入 poll/AI preparation 继续负责下一 tick 的 previous/current 滚动与 release，`NeedClearInput` 的战斗入口全量清理保持不变。没有修改 DAT 数值、1.5 表现缩放或空中物理倍率。
- **回归契约**：`CheckGameTickInputClearBoundaries` 的 GT-02 改为断言 current/previous keys 在 frame advance 可见；新增 frame 211 -> 212 回归，覆盖“按住右/上使用 DAT jump distance”“无方向覆盖时继承原 Vx/Vz”“不制造 cooldown/history edge”。
- **当前证据**：`git diff --check` 无 whitespace error；`dotnet build Assembly-CSharp.csproj --no-restore /m:1 /v:minimal` 为 **0 errors / 42 existing warnings**。目标源码最晚时间 `23:15:11` < Unity `Assembly-CSharp.dll` `23:15:37` < fresh result `23:16:33`，`Temp/NTSD_BattleRuntimeSelfCheck.result` 为 **PASS**。本项状态是 **逻辑已修正 / Unity 自动运行时已验证**；真实键盘 Play Mode 的移动起跳体感仍待用户或后续定向验证。
- **同时关闭的表现阻塞**：fresh 自检先定位出动态扩容池实例的 EntityModel mount 保持 `Invalid` handle。`BattleCentralPresentationMountRegistry.BindOwnerRuntime` 现会直接更新 renderer 本体 mount，并继续保留 slot+generation 校验；P4 pool-overflow 回归随后通过。

### Texture2DArray 现状澄清

- 中央渲染的角色图集主路径已经使用 `BattleSpriteCentralBindingMode.AtlasTextureArray`；设备不支持数组或策略选择 `OrderedPages` 时才回退到多 `Texture2D`。
- 公共阴影当前由 `BattleCommonVisualCatalog` 发布为 `SourceTexture2D`，没有进入角色的 `Texture2DArray`，所以阴影与角色仍会形成不同 resource segment/draw。该事实是批次边界，不代表角色数组路径未实现。


--- File: Assets/NTSD/Docs/HANDOFF-codex-battle-alignment.md ---
# 接手文档 — NTSD C# → Unity 战斗逻辑对齐（Codex 无缝接手版）

## 2026-07-24 P8 v5 当前交接结论（覆盖下方 v3/v4 历史快照）

- P8-A/B1/B2 已完成，P8-C 保持既有生产 factory/pool、像素与稳定性验收范围。P8-D v5 的 Editor/Windows Development Player `100/300/500/1000` 共 8 份 real-runtime A/B 报告全部 PASS。
- 报告路径为 `Temp/P8-D-runtime-{100,300,500,1000}-{editor,player}-ab-v5.json`。每个 backend 有 120/120 样本，适用必需指标完整，generation-owned texture memory 为正，600-frame leak 与 post-dispose teardown 通过，owned bytes/resources 归零；Central/Legacy 的 workload fingerprint、input fingerprint、final runtime checksum 相等。
- v4 的全局 `Texture Memory` counter 返回 0，属于 `Incomplete` 历史证据；v5 不再依赖它。Player 也不再使用 `-batchmode`/`-nographics`，从而保留真实 graphics device、GPU timing 和 draw-call 证据。0 draw calls 对非空 workload 无效，正式样本最多重试 16 次，耗尽为 `Incomplete`。
- 当前 16-retry/cleanup 源码重新生成的 Editor `100/300/500/1000` 报告完成于 `2026-07-24 03:00:12`、`03:06:39`、`03:12:02`、`14:10:19`：logic tick 平均/最大依次为 `13.227/45.537`、`42.752/198.637`、`78.149/221.383`、`36.488/201.219 ms`。其中 Editor 300/500/1000 平均均超过 30 Hz 的 `33.33 ms` 预算；Player 1000 为 `9.123012 / 42.3011 ms`。数据非单调且受 Editor/当前机器影响；PASS 只说明门禁和可比 workload 通过，**不代表性能预算达标，也不代表 Central 必然快于 Legacy**。
- fresh 验证：benchmark EditMode `34/34 passed`，完整 `BattleRuntimeSelfCheck` `PASS`，Runtime/Editor dotnet build 0 errors。矩阵连续启动 300 Player 时曾一次 native exit `-805306369`；同 build 的独立 300 单样本与完整重跑均退出码 0，最终四档 Player 报告均有效。
- 本轮修复 P1：Play Mode 退出可能遗留 hidden benchmark runner，让请求已被消费但永久显示 `RUNNING`。processor 已接入 `ExitingPlayMode` fail-close、非 Play 状态 runner reconcile 与 EditMode request 保留；新增 3 个 focused tests 通过。
- P8-E Android/Adreno/Mali 真机继续由用户负责；T8 默认 `stage.dat` 部署取消/排除。后续接手者不得用下方 v3/v4 或 presentation-only 结论覆盖本节。

## 2026-07-23 P8 当前交接证据（优先于下方 P8-C/P8-D 历史快照）

- **P8-B 已具备可核验诊断契约：**`FrameId`、显式 `AtlasPageIndex`、strict binding validation、first unresolved/unsupported status 和 generation/tick-coherent aggregate diagnostics 均已进入当前实现与 focused/full checks。Runtime/Editor 相关构建为 0 errors。
- **P8-C 生产验收已更新：**`Temp/P8-C-Resume-Live/P8-C-report.json` 于 `2026-07-23 17:28:29` **PASS**。正式链是 `LF2ObjectPointFactory.CreateObjectImmediate` / `FreeEntityLikeExe`：`availableBefore=7`、`totalCheckout=9`、`expandedAndPublished=2`、`availableAfter=9`、`uniqueRuntimeHandles=2`，cleanup PASS。生产 `Entity(33,0)` type `0`、`AtlasPageTexture2D` 在 Legacy/Central 都有 `4971` alpha pixels；`Entity(100,0)` type `4`、`AtlasPageTexture2D` 都有 `2090` alpha pixels；两者 maximum pixel diff 均为 `0`。该 factory/pool/publication 证据不覆盖 skill-input opoint。
- **P8-D final v3 取代 synthetic-only 描述：**`Temp/P8-D-runtime-{100,300,500,1000}-editor-ab-v3.json` 及 matching `-player-ab-v3.json` 全部 **PASS**。每一项使用真实 `MobileExtended(1050)` primary + mirror `SimulationWorld`、准确数量的真实 `LF2Entity`、`FrameInputSet.Empty` 和完整 `NTSDBattleTickSystem`；执行 30 warmup + 120 sample logic ticks、deterministic checksum、真实 handles/generation/positions 的 frozen presentation、相同 A/B workload 与 600-frame leak gate。不能由此写成 central 快于 legacy。

| v3 report | logic tick avg/max ms | tick allocation avg/max B |
|---|---:|---:|
| `100-editor` | `8.3087375 / 12.0803` | `0 / 0` |
| `300-editor` | `24.3566941666667 / 33.9412` | `0 / 0` |
| `500-editor` | `42.7971166666667 / 57.0061` | `0 / 0` |
| `1000-editor` | `100.006675 / 126.7602` | `0 / 0` |
| `100-player` | `0.537154166666667 / 1.285` | `0 / 0` |
| `300-player` | `2.59706583333333 / 29.4842` | `0 / 0` |
| `500-player` | `1.56702166666667 / 2.752` | `0 / 0` |
| `1000-player` | `2.980925 / 6.0687` | `0 / 0` |

- **性能解释：**Editor 1000 的约 `100 ms/tick` 不满足 30 Hz；Windows Standalone final v3 1000 约 `2.98 ms/tick`。A/B PASS 证明同一工作负载和 gate 的正确执行，不是一般性的 performance-winner 结论。
- **最终顺序回归：**held geometry 失败已定性为 parentless/root renderer 的 `_visualTransform == rootTransform`，不是 benchmark 全局状态泄漏；正确世界位置曾被随后对同一 Transform 的 local-zero 重置。当前实现只对独立 child visual 归零，并有 focused fixture 验证 runtime X/Y/Z、`FirstPresentationTick`、`CentralShadowBuild`、legacy suppression 与 immutable central command 对照。fresh DLL `18:05:55` 晚于源码 `17:59:02`；1000 实体 A/B `18:10:49` PASS，退出 Play 后 full self-check `18:13:03` PASS；最终 Runtime/Editor dotnet 构建分别为 `0 errors / 42 warnings`、`0 errors / 48 warnings`。当前仍不预写 Architect PASS。P8-E Android/Adreno/Mali 与 T8 默认 `stage.dat` 继续排除。

## 2026-07-23 P8 中央渲染交接状态（当前）

- P8-C 正确性/像素矩阵已闭合到其定义范围：`Temp/P8-C-EditModeTest/P8-C-report.json` PASS；`Temp/P8-C-LivePool/P8-C-report.json` PASS（真实 Play pool `4 available -> acquire 5`，5 个唯一 mount owner）。旧 job `f278668e3a2445139c6a1a5ceb8815be` 的 11/11 仅为历史；P2 回归后的 fresh job `e455b7f70043438a938faa23e82e53f3` 为 12/12（P8-C 2 + P8-D 10，0 failed/skipped）；fresh full self-check `Temp/NTSD_BattleRuntimeSelfCheck.result` 为 2026-07-23 12:07:26 PASS（P2 `BattleRenderingBenchmark.cs` 11:56:24 < Unity DLL 11:59:33 < result 12:07:26）。过滤到的 2 条 Console error 是 self-check 刻意构造的 registration rollback / mismatched rest binding release 拒绝路径（`BattleRuntimeSelfCheck:7046` / `:1133`），无编译错误栈或 benchmark 异常。
- P8-D 四档受控表现 A/B 报告均通过：`Temp/P8-D-presentation-100-ab-rerun.json`、`300`、`500`、`1000`。它们严格验证 presentation count/commands、256x256、资源、owned memory 与 retained heap 阈值，但只是 deterministic synthetic presentation workload，不表示真实 SimulationWorld active capacity、logic tick 性能、生产 atlas 性能或全面性能收益。P2 已关闭 EditMode 将 mesh segment 冒充 `Graphics.DrawMesh` submission：`presenterSubmissionDrawCalls` 显式 unavailable，Play 仅在实际调用提交后计数；其他无法取得的 main/render/GPU/draw 指标也保持 unavailable。本轮没有 Standalone Player 实测。
- 额外 current-scene production 覆盖：`Temp/P8-D-current-scene-ab-v2.json` PASS。退出 Play 前真实 `NTSD_Battle` 是 `ObjectCount=12/tick=3847`，冻结 published frame 是 `6 entities/12 commands`；Central/Legacy 均实际 `6/12`、同 fingerprint `f3aaf429518f46ec`、同 256x256。retained managed heap 为 Central `+28672 B`、Legacy `+49152 B`，graphics/owned bytes `+0`、resource count 不变。presentation build/GPU 仅为一次 Windows Editor 样本，main/render/draw unavailable；该项只是额外生产覆盖，不是 P8 独立 gate 或整体性能结论，运行后已退出 Play。
- P8-E Android/Adreno/Mali 真机验证仍由用户负责，T8 默认 `stage.dat` 部署继续排除。下方关于 P8-C/D 待实施、Play/pixel/Profiler 待验收的相反表述均为历史记录，不能覆盖本节。

## 2026-07-22 对象池预热上限后 opoint 武器不可见（当前接手结论）

- **复现：**隔离 `PoolInitialSize=10`，经生产 `opoint`/factory 保留 12 个 `LightWeapon`。第 11/12 个实体逻辑、声音、unique root/renderer、mount/runtime handle、sprite、12 条 Entity command 均存在，中央像素却缺失。
- **定性与修复：**这不是 C# 战斗逻辑差异，也不是 pool 扩容、runtime handle 或资源问题，而是 Unity `BattleDynamicMeshBackend` 的动态 submesh descriptor 生命周期适配缺陷；权威 C# 不定义此 Unity 渲染实现。旧布局/增长时默认 descriptor 临时重叠，Unity 2022.3 收缩 `subMeshCount` 会截断 index buffer。每 chunk 现维护 `activeSubMeshCount`，physical `subMeshCount` 为只增不减的 high-water；增长后先置全部 descriptor inert 再写 active，非增长先清旧 active 再写 active，empty 不收缩；禁止 bulk `SetSubMeshes`，此前会触发 native crash。
- **回归覆盖：**隔离预热 10、生产 `opoint`/factory 12 个 `LightWeapon`，检查 unique root/renderer、mount/handle、sprite、12 条 Entity command；并覆盖 `1 -> 32 -> 1 -> 33 -> 1`、inactive inert tail、`GraphicsBuffer.count=24576`、`4096/4097`、recovery、0 GC、scoped warning 捕获。
- **fresh 证据：**source `20:24:58` / `20:26:45` < DLL `20:28:54` < result `20:29:44` **PASS**；Unity `0 compile errors`。本轮 `Editor.log` offset `31277122` 后 descriptor overlap、bulk `SetSubMeshes`、native crash 均为 `0`；Editor PID 响应正常。
- **诚实状态：**代码、编译、self-check、生产 `opoint` 链已验证；用户真实 Play Mode 视觉复测仍待确认。T8 明确排除，默认 `stage.dat` 部署继续暂缓。

## 2026-07-22 Rendererless 武器显示回归修复（当前接手结论）

- **问题与根因（旧复现限定）：**4 个随机掉落武器存在后，角色 `opoint` 武器会使掉落武器与新武器不显示；后续 `opoint` 仍不显示，但落地声音继续。rendererless `LF2Sprite.Hide` 把 `EntityVisible=false`，成功 `ShowPic(valid)` 未恢复它，导致 `CurrentEntry`、`pic`、逻辑和声音正常而中央 Entity command 被持续过滤。此 `EntityVisible` 根因只解释该旧复现，不解释 `PoolInitialSize=10` 后第 11/12 个实体已有 command 但缺像素；该问题以本文件上方的 Unity 动态 submesh descriptor 适配缺陷为准。
- **已修复的边界：**仅成功解析 catalog 或 legacy sprite 时恢复 `EntityVisible`。`pic=999` 与 missing sprite 不恢复显示，保留失败/隐藏语义。
- **fresh 证据：**Unity `Assembly-CSharp.dll` `2026-07-22 18:56:11` fresh compile，Console `0 error`；完整 `BattleRuntimeSelfCheck` `18:58:50` **PASS**；`dotnet build Assembly-CSharp.csproj` 为 `0 errors / 42 warnings`。Play Mode：4 个预存随机武器后，经 `LF2ObjectPointFactory` `opoint oid121` 的 `Hide -> ShowPic`，随机 slot `50` 和 opoint slot `54` 都有 Entity command；销毁复用同一 renderer instance 后再执行 `opoint`，slot `54` command 仍存在；central `IsStale=false`、`unresolved=0`。
- **验收边界：**本项仅证明该 rendererless 武器显示回归已完成编译、self-check 和定向 Play Mode 验证；不扩大为完整战斗系统或全设备/资源组合验收。T8 默认 `stage.dat` 部署继续暂缓。

## 2026-07-22 Rendererless Central Mount 收口（当前接手结论）

- **生产 prefab 已接线：**`EntityObject` 与 `Shadow` prefab 的对应节点均已挂载 `BattleCentralPresentationMount`；持久 `Entity`/`Shadow` `SpriteRenderer` 已从生产 prefab 移除。common shadow 使用 `BattleCommonShadowDescriptor`；`LF2Sprite` 维护 renderer-independent 的 `visible`、`pic` 与 offset。默认模式为 `CentralOnly`。
- **生命周期、销毁和失败语义：**mount 标记 `[ExecuteAlways]`，以 `gameObject.scene.IsValid()` 为 gate；prefab asset 本身不注册。Prefab Stage preview 可参加编辑态 lifecycle，但没有 runtime handle，因而不属于生产 battle/pool 验证。mount/renderer 在 `OnDestroy` 主动移除 owner binding，防止 pool expire 销毁后静态字典仍保留 destroyed wrapper。冷启动失败 fail closed；已有成功帧后出现失败时保留 last-good frame 并记录 stale。该表现路径不回写战斗 runtime。
- **fresh 自动、编译与 Console 证据：**mount source `15:41:46` < Unity `Assembly-CSharp.dll` `15:43:40` < 完整 `BattleRuntimeSelfCheck` result **PASS** `15:44:50`；full self-check 已加入真实 `DestroyImmediate(root)` focused 断言，覆盖销毁 owner binding 清理。主代理最后一次 `dotnet build` 为 **0 errors / 18 existing warnings**，此前 42 warnings 属于不同生成视图。最新清空 Console Play/Stop 为 **0 error / 0 warning**。第一轮截图工具自身的 RenderTarget errors 不作为项目错误或项目验证证据，第一轮截图不能用于写 Console 为零。
- **最新 Play Mode 状态：**`NTSD_Battle` 最新观测为 `objects=6`，requested/effective=`CentralOnly`，`frame`、`ownership`、`submission`、`submitted` 均为 true，`draws=6`，`sim/display tick=339`，`stale=false`；3 个生产 `LF2ObjectRenderer`、6 个 mount/handle 均有效，且 `persistent SpriteRenderer=0`。
- **前一轮视觉证据：**此前 `objects=12`、6 个生产 renderer、12 个 mount/handle、`draws=12` 的观测继续保留为前一轮视觉证据；`Temp/central-rendererless-game-20260722.png` 可见角色、武器与阴影。该截图不代表最新运行的对象数量。
- **当前 Prefab Stage 例外：**一个 `EntityObject` prefab-stage preview instance 仍带旧 `SpriteRenderer`，但 `logic=null`；它可参加编辑态 lifecycle，却没有 runtime handle，是当前打开 Prefab Stage 的内存态，不属于生产 battle/pool 对象，未计入生产验证。本轮没有修改或关闭用户的 Prefab Stage。当前 Scene View 位于该 Prefab Stage，故本轮没有 fresh Scene View 截图；此前 Scene View 证据仅保留为历史证据。
- **继续排除：**T8 默认 `stage.dat` 部署和 Android/真机验证仍不在本轮范围内。

## 2026-07-22 Central Presentation Mount v1（历史快照，已由上方 rendererless 收口取代）

- **已实施，范围严格受限：**新增 `BattleCentralPresentationMount` 和 `BattleCentralPresentationMountRegistry`，并在 `LF2ObjectRenderer` 中声明/注册。v1 只建立 generation-aware `RuntimeEntityHandle` 绑定；没有更改渲染、资源加载、`Update`、render command 或战斗 runtime。
- **World 生命周期：**`SimulationWorld` 已在 register、release 与 reset 路径接线，确保 handle generation 变化或 runtime slot 复用后，旧 mount 不会继续代表新实体。disable -> enable restore 与 rollback clear 均已关闭并纳入 `BattleRuntimeSelfCheck` 覆盖；本批新增并通过了 world `ResetRuntimeState` 与 registration rollback focused checks。自检同时覆盖 `LF2ObjectRenderer` 集成、注册、release、reset 和 generation-aware binding。
- **场景与迁移边界（历史）：**当时尚未编辑 prefab，`Legacy` 保留；当时的下一项是向 `EntityObject` 的 `EntityModel` 和 `Shadow` nodes 挂 mount component 并设置 `ownerRenderer`。此段不代表当前 rendererless `CentralOnly` 状态。
- **最终验证：**relevant source `2026-07-22 11:48:18` < Unity `Assembly-CSharp.dll` `11:49:08` < `Temp/NTSD_BattleRuntimeSelfCheck.result` **PASS** `11:50:11`。最终完整命令 `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 完成，结果为 **0 errors / 42 existing warnings**；Architect closure 为 **PASS / no P0-P2**。Console 清空后仍有两类预期 self-check-active Error：既有 mismatched release 和新的 registration rollback。因此本批不是“Console 0 errors”验证，任何旧的 0-error Console 叙述不得用于证明此批。

## 2026-07-22 Editor Scene View Preview Validation

- **Scope and guardrails:** `CentralOnly` submits the same immutable mesh from the base Scene View camera only under `#if UNITY_EDITOR` and `Application.isPlaying`. Only the exact world camera may update renderer readiness. Scene View preview does not alter combat state, Player builds, or the Game camera.
- **Freshness and automated evidence:** Unity `Assembly-CSharp.dll` timestamp `23:47:47` is newer than the relevant source timestamp `23:30`; the direct `BattleRuntimeSelfCheck` result is **PASS**.
- **Observed Play/Scene View evidence:** Play state reported `objects=12` and the central mesh reported `quads=12`. `Temp/screenshot-20260722-000938.png` shows all current entities in the Scene View. The screenshot round's tool-originated RenderTarget errors are not project evidence; this screenshot does not establish a Console-zero result.
- **Validation boundary:** This verifies the current observed Scene View preview state only. It does not establish coverage for all resource scenes. T8 default `stage.dat` deployment and Android/device validation are not part of this task.

## 2026-07-21 Fresh Final Validation（接手时的当前渲染状态）

- **CentralOnly 已在真实 Play 接管**：运行时为 `requested/effective=CentralOnly`，`frame/ownership/ready/submitted=true`，`draws=12`。P7 Overlay、Shadow、Entity 与 HitRecord 都使用同一帧 pixel owner；“CentralOnly 继续拒绝”“Overlay blocker”“P7 未完成”均已是历史状态。
- **已修复的像素根因**：`BattleDynamicMeshBackend.ClearActive` 置 `subMeshCount=0` 会让 Unity 2022.3 释放 native index buffer，下一次写入造成黑块/三角形 UV 伪影。现保留零索引 inert submesh，恢复稳定索引缓冲；该修复不改变战斗 tick 或实体逻辑。
- **像素与运行时证据**：暂停同一帧 Legacy/Central `1920x1080` 截图比较 `changed=0`，截图直接覆盖其中可见的角色、武器/球体与阴影。Overlay/HitRecord 的 ownership 与资源路径由 self-check 和运行时 diagnostics 证明，不宣称它们在该截图中一定可见。`Temp/NTSD_BattleRuntimeSelfCheck.result` **PASS**；Unity Console **0 error / 0 warning**。显式 `LooseQuadtree` 真实 Play 为 `backend=LooseQuadtree, objects=12, tick=1436`，亦 **0 error / 0 warning**。B2C Architect final：**PASS / no P0-P2**。
- **Editor 性能快照**：Legacy `6.1884 ms CPU / 0.346112 ms GPU / 18 draws`；Central `6.5114 ms CPU / 0.70656 ms GPU / 20 draws`；Central `1391.17 MB allocated / 1005.19 MB graphics`。这是当前 Editor 样本，不代表 Central 已取得性能优势。
- **仍需外部环境的验收**：没有真实 Adreno/Mali 或 Android Player 的目标设备数据，故不得声称移动真机通过。T8 默认 `stage.dat` 部署继续按用户决定暂缓，不能为了验收私自补资产。

> **历史注记**：下方关于 `CentralOnly` unavailable/拒绝、Overlay blocker/P7 未完成、B2C 无 Architect final，以及 Play/pixel/Profiler 未验收的文字，均为本次 fresh final validation 前的阶段快照；保留用于溯源，当前接手判断以本节为准。

## P7 Batch6 per-entity Overlay 接手状态（2026-07-21，覆盖旧 blocker 叙述）

- **已收口的代码侧缺口**：P7 Batch6 完成了 per-entity Overlay。Unity 已新增 `WORDS0.bmp` 至 `WORDS5.bmp`，SHA256 与权威 C# host 使用的运行时资源来源一致；这只是资源依赖核验，不引入 C# 之外的战斗逻辑权威。唯一战斗逻辑权威继续是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。
- **资源和 runtime 契约**：typed `CommonWordGlyph(sheet, charCode)` 覆盖 `6 * 256` glyph，authority top-left rect 转 Unity bottom-left；WORDS 预热采用 exact-black transparency、Point/Clamp、atomic publication/retirement。`BattleSlotLabelRuntimeState` 为 `char[10,12]` + `int[10]`，已接入 reset 和 `MatchConfig` bootstrap。
- **绘制行为**：`BattleEntityOverlayLayout` 无分配处理复活 counter、普通与括号标签、普通 `Com`、特殊 `WORDS5 Com`，标签 clamp、counter 不 clamp，容量错误 fail-closed。snapshot 分离原始 `ObjectId`（shadow 223/224 gate）与 current DAT identity（Overlay）；顺序是 `Shadow -> Entity -> OverlayGlyph -> HitRecord`。legacy pooled `BattleEntityOverlayRenderer` 有 generation/stable-id guard；默认 `LegacyOnly` 仍发布 immutable frame 但不构建 central mesh，`CentralShadowBuild` 保持诊断，`CentralOnly` 继续由 `ValidateAvailable` 拒绝。
- **资源生命周期与自检**：frame-level catalog lease、HitRecord cycle lease finalizer、empty-frame no-retain 已实现；self-check 覆盖 retirement 窗口，以及 HP2、slot/bracket/empty/Com、palette、特殊 OID/type/hitstop、clamp、fail-closed、命令序列和 zero-GC。
- **fresh 验证**：latest relevant source `2026-07-21 16:01:49` < Unity `Assembly-CSharp.dll` `16:03:35` < full self-check result `16:04:54` **PASS**；Unity Console **0 C# error**；最后一次主代理 `dotnet build` **0 errors / 18 existing warnings**；Architect final **PASS / no P0-P2**。`git diff --check` 留待主任务最终执行。
- **未完成门槛**：不得表述 P7 全门槛完成。Play/pixel/Profiler/Adreno/Mali 未验收；T8 默认 `stage.dat` 部署保持排除。下方所有“Overlay 未实现”“Assets 没有 WORDS0..5”“confirmed blocker”或“Overlay 阻塞”的文字均属 Batch6 前历史快照，已被本节覆盖。

## B2C Extended checksum 交接（2026-07-21 当前状态）

- `Authority400` 继续使用冻结的 `ntsd-battle-trace-v3`，direct parity capture 仍只接受 `Authority400/400`；Extended 通过通用 API 生成独立 `ntsd-unity-extended-battle-checksum-v1`，`LastFrameSnapshot` 保持 Authority-only。
- Extended checksum 已覆盖 profile/capacity/count/tick、slot claimed/generation/stable ID/current DAT、active runtime、已物化未 claimed raw runtime，以及确定性稀疏 ARest/VRest；未物化分页不会因 capture 创建，错误 rest-store/victim-slot binding 会使 capture 失败。
- focused matrix 覆盖 Mobile `1050` / high slot `1049`、Desktop `512 -> 768` / slot `700`、高槽 rest、raw runtime、纯 generation reuse、profile separation、`65536` 稀疏容量和 repeat/non-mutating capture。
- B2C 还已接入 generation-aware AI Loose Quadtree 输入快照查询，以及显式 `LooseQuadtree` 后端下的即时 weapon/body current-world 查询；索引、几何或映射异常均回退 brute，生产默认仍为 `BruteForce`。
- freshness：source `2026-07-21 03:00:45` < Unity DLL `03:05:59` < latest full self-check `03:07:05` **PASS**；dotnet **0 errors / 42 existing warnings**；最终 architect review **PASS / no blocker**。
- P1 compact legacy sorting 已完成代码和自动验证：`(ZInt, runtime slot)` dense rank，`Shadow/Entity/Overlay/HitRecord=0/1/2/3`。权威 host 确实在 Entity 后绘制 per-entity Overlay；Unity 只有子序、没有对应消费者，这是 confirmed blocker。相关 `SpriteRenderer`/`SortingGroup` 全部使用 `Object` sorting layer。legacy 表现后端 guard 为 `8192` materialized active entities，移动端 `1000` 安全，DesktopExtended 在中央后端完成前有该临时表现上限。真实双实体 `ForceRefresh` 验证 `Shadow(A)=0`、`Entity(A)=1`、`Shadow(B)=4`、`Entity(B)=5`。未执行 Play Mode，故 P1 仍为 **代码/self-check/architect 通过，Play-unverified**，不可写作 P1 全部验收或 P1-P7 已完成。
- P2 immutable `BattleSpriteCatalog` 已完成代码层实施：key 为 `(ResolveCurrentDataObjectId, effectivePic)`；entry 固化 shared sheet texture、bottom-left pixel rect、UV、metrics、pivot 与 legacy `Sprite`。prewarm 以 invocation-local staging + generation/disposed gate 原子发布 configs、`MergedSprites` 和 catalog；失败、stale 与 teardown 均清理，renderer 引用计数保证旧 catalog 零引用后才退役。
- P2 已覆盖正式 partial BMP 的 declared row/col + sparse `localPic` holes、normal/swapped 完整匹配择优，以及 weapon6/weapon3 等矩阵。display、collision、anchor、SpecialAttack point-center、shadow metrics 已脱离战斗期 `Sprite.rect`；`pic=999`、missing key、identity switch 与 pool reuse 会清除旧表现引用。`MergedSprites` 只作兼容/预览。
- P2 fresh 证据为 source `2026-07-21 04:16:00` < Unity DLL `04:17:06` < full self-check `04:18:04` **PASS**；dotnet **0 errors**（自动生成 `.csproj` 的不同刷新视图显示 18 或 42 条既有 warnings，不冻结 warning 数）；最终 architect **PASS / no blocker**，最终 code review **no P0-P2**。未执行 Play Mode、真实异步 BMP stress 或性能验收，因此 P2 状态是 **代码+编译+self-check+静态复核完成，Play/perf/stress-unverified**。
- P3 已实现默认 `LegacyOnly` 与诊断 `CentralShadowBuild`；`CentralOnly` 明确拒绝。value-only immutable snapshot/commands 按 `(ZInt, slot)` 展开每实体 `Shadow -> Entity -> Overlay -> HitRecord`，使用 double buffer、几何容量增长和 atomic publish；persistent scratch 的 steady `RenderDispatch` self-check 为 zero allocation。早期 `AuthorityExpectedButLegacyMissing` 标记已由权威复核废止；权威 Overlay 可绘制而 Unity 未实现，P3 不宣称 overlay 等价。
- P3 actual legacy probe 读取真实 renderer 的 sprite/texture/material instance、rect/pivot/position/flip/sorting，HitRecord 在 advance 前采样。catch-up 中间 tick 明确发布 `Incomplete` count/first/last，只对最后可观测 tick 做完整 probe，不能写成全部逻辑 tick 已实际 parity verified。zero-hit 经 `SparkRenderer.RenderAll` finalize；production pool path 覆盖 nonzero spark atlas cells、age once、`OnDisable`/`OnDestroy` pool return。
- P3 与 battle checksum 隔离。fresh 证据为 source `2026-07-21 05:38:38` < Unity DLL `05:39:29` < full self-check `05:40:16` **PASS**；dotnet **0 errors / 18 existing warnings**；最终 architect **PASS / no blocker**，最终 code review **no P0-P2**。未执行 Play Mode、真实 SPARK BMP/设备或性能验收；未来异步 consumer 仍需 catalog lease/generation。P1-P3 代码/self-check/静态复核完成。
- P4 代码层已完成：持久 `4096`-quad/`UInt16` chunks；`OrderedChunks` 保持 `A,A,B,A` 原顺序，`StrictOrderedDraw` 为正确性回退；unresolved barrier、stale clear、跨 chunk 顺序均有 self-check。`LegacyOnly` 不 build，`CentralShadowBuild` 不提交 draw，`CentralOnly` 在全类别 ownership 前拒绝。URP 只接收 world-camera `Base` camera，并在 `AfterRenderingTransparents` 注入。
- `BattleRenderFeature` 已作为 active renderer asset 唯一 subasset 安装并验证。初审发现 feature B 覆盖 A 后注销 B 不恢复 A，现以 registration stack 修复；`A -> B -> unregister B -> restore A` 已验证 fallback material、array material 与 draw mode。
- P4 fresh 证据为 source `2026-07-21 06:32:00.287` < Unity DLL `06:32:56.970` < full self-check `06:33:43.796` **PASS**；dotnet **0 errors / 42 existing warnings**；最终 architect **PASS / no P0-P2 findings**。未执行 Play Mode、桌面像素 baseline、Profiler GC、Android/Adreno/Mali，因此 P4 为 **代码/self-check/静态复核完成，全部验收门槛未完成**。
- P5 代码/self-check/静态复核已完成：确定性 `2048` whole-sheet 多页 planner、normalized path ordinal 去重与像素冲突拒绝、`1px` extrusion、`RGBA32 Texture2DArray` 能力 gate 和有序 2D fallback 已落地。catalog 保留 legacy source 并增加 immutable central binding；manager 事务发布并持有 Unity Object ownership，renderer/central lease 共同保护退役资源。
- P5 array 路径使用 per-vertex slice，使相邻跨 slice 命令在同一 material 下保持原序合批；fallback `A/B/A` 保持三段不重排。双 shader/material/pass 与 installer 已接线。复核关闭两项 P2：同 path/同尺寸/不同 pixels 的双排列均拒绝、equal content 成功；显式两页 fallback 在 page1 失败后 page0/page1 全销毁且无 partial publication。
- P5 fresh 证据为 source `2026-07-21 07:06:28` < Unity DLL `07:07:12` < full self-check log `07:08:13` **PASS**；dotnet **0 errors / 42 existing warnings**；architect final **PASS / no P0-P2**，code review **no P0-P2**。未执行生产 BMP Play、桌面 overlap pixel baseline、Profiler/allocation stress、Android/Adreno/Mali array/fallback 或内存性能，因此不得宣称 P5 全部验收完成。
- P6 设备策略/诊断代码已完成：immutable `BattleRenderingDevicePolicy` + `FromSystem` 边界；CLI > `GameConfig` > Auto strict resolver（`-ntsdBattleAtlasMode` / `-ntsdBattleDrawMode`）；`TextureArray`/`OrderedPages` fallback 及原因；Auto/`OrderedChunks`/`StrictOrderedDraw`，且 `SingleMesh` 不入生产。resolver 输出确定性 JSON report，manager publication 一次解析，central 缓存 effective draw，tick 热路径无 `SystemInfo`/CLI。
- P6 不改变 profile/capacity/tick/collision/checksum/`CentralOnly` guard。它只是代码策略与诊断完成，Adreno/Mali、Play、pixel baseline 与 Profiler 仍未验收。
- P7 held-object 子批已完成：按 `InteractionRuntimePasses -> WeaponPointRuntime/WeaponRuntime -> SdlBattleRenderer/BattleHostForm` 权威链核对；legacy/snapshot 共用 pure held-offset helper，capture-time 固化 immutable offset 并追加到 Entity command。矩阵覆盖 right/left、target mismatch、release、missing holder/wpoints、slot generation reuse、dormant 与 legacy/central equality。
- P6/P7-held 的分项 fresh 证据为 source UTC `23:42:44` < Unity DLL `23:44:03` < `Unity-P6-P7-Final2-SelfCheck.log` `23:45:00` **PASS**；dotnet **0 errors / 18 existing warnings**；architect **PASS**，code review **approve / no P0-P2**。
- P7 Batch2 render-state semantic parity 已完成：value-only `Color32`、flipXY、mask/material semantic 和 logical resource key 已进入 snapshot/command，instance ID 仅诊断；catalog 支持 immutable `Sprite -> key[]` 与 preferred entity key。legacy probe/Compare 覆盖 RGB/alpha/flipY/unsupported/key，central resolver 转发 color 并 fail closed。
- Mesh 四顶点写 color，flipY 交换 V；color 不切 segment，semantic variant 断段。pool entity/shadow/spark checkout 归一为 white、flipXY false、mask none，首次干净 checkout 使用 `Sprites/Default.sharedMaterial`，不调用 `.material`。
- 两个中央 shader 依据 Unity `2022.3.4f1` builtin shaders ZIP changeset `35713cd46cd7` 使用 `Blend One OneMinusSrcAlpha` + final `rgb *= a`，并带 `NTSDAlphaContract` tag；installer 验证 white/tag。fresh source `08:27:50` < DLL `08:28:48` < self-check log `08:29:48` **PASS**；installer **PASS**；dotnet **0 errors**；architect/code review **PASS / no P0-P2**。
- P7 Batch3 Shadow 已完成：authority `BattleHostForm` / `SdlBattleRenderer.DrawShadow` gates；typed `EntitySprite/CommonShadow` key；immutable borrowed `GameConfig.ShadowPrefab` binding（真实 sprite/texture/UV/size/pivot/color/material）。manager main-thread atomic common publication，borrowed object 不进入 owned retirement。
- snapshot 保存 actual ObjectId/`HasCurrentFrame`；Shadow command 使用 real descriptor/`CommonShadow` 并位于 Entity 前；legacy probe exact sprite；central resolver 校验 sprite/texture/rect/pivot/material ID，并提供 source2D + fallback material。missing config/resource fail closed。矩阵覆盖 actual OID223/224、state3005/9997、`Link<0`、HitStop、missing frame。
- review 关闭 P1 missing-frame legacy/central，以及 P2 material ID、真实 `GameConfig` asset、real commit -> replace retirement tests。fresh source `09:29:03` < DLL `09:31:10` < log `09:32:07` **PASS**；dotnet **0 errors / 18 existing warnings**；architect/review **PASS / no P0-P2**。
- P7 Batch4 已完成 SPARK / Common HitRecord resource ownership 代码层收口：typed `CommonSpark(pic)` 覆盖 20 帧，prewarm 仅 decode/process 一次并在 main thread atomic publish；legacy `SparkRenderer` 不再在 `Awake` decode 或创建资源。central resolver 验证 logical key、`Sprite`、`Texture`、rect、pivot、size 和 material，publication lease/retirement 已接入。
- Batch4 错误边界已覆盖：缺失/无效 SPARK 释放 stale lease 且不改变 `HitRecord` age/count；partial `Texture`/`Sprite` 构造失败事务式 cleanup，禁止 partial publication。fresh 证据为 source `11:13:05` < DLL `11:15:20` < result `11:17:38` **PASS**，architect re-review **PASS / no P0-P2 findings**。code-review provider 返回 `429`，未取得 code-review 通过结论。
- P7 Batch5 已完成 backend-neutral immutable double-buffer HitRecord presentation cycle：`RenderDispatch` 冻结 owner handle/generation、count、age、x/z 和 common publication；`SparkRenderer` 仅 materialize/probe。`LateUpdate` 为 legacy materialize -> central `PrepareFrame` -> one finalizer，catch-up 仅 finalize 最后 cycle。
- mutation 矩阵覆盖 missing SPARK zero-write、valid age 每 cycle `+1`、invalid sampled tail 每 cycle最多删 1、`4/14/28/38` 入 gap 同 cycle 不删，以及 slot reuse/count/age guards；pool/camera/backend 不影响结果。binding direct ownership transfer 无 per-tick lease GC，no-hit 不持 binding；coordinator reset 接 world reset/driver unbind/replacement/destroy；ordered owner cursor O(N)，`1000` owners=`1000` comparisons。
- Batch5 fresh source `12:39:24` < DLL `12:40:40` < result `12:41:20` **PASS**；dotnet **0 errors / 18 existing warnings**；architect **PASS / no P0-P2**；code review **APPROVE / no P0-P2**。Play/pixel/device 仍未验收。
- Overlay authority re-audit 确认为 blocker：`BattleHostForm` 与 `SdlBattleRenderer` 都按 `Shadow -> Entity -> EntityOverlays -> HitRecords`；per-entity 内容为 `Hp2Orig > 1` 复活次数和 entity label，`WORDS0..5.bmp` glyph 为 `8x16`、步距 `9`、black colorkey。Unity `Assets` 没有 `WORDS0..5`，也缺 `BattleSlotLabels[10,12]` / state 镜像和 snapshot 字段契约。Overlay 未实现，`CentralOnly` 继续拒绝。global function/pause overlay 是独立后置 UI 且 GDI/SDL 不一致，不纳入 per-entity P7。
- P7 仍未完成：Play/pixel/device 未验收；Overlay confirmed blocker，`CentralOnly` 继续拒绝。下方旧状态只表示历史阶段；T8 已排除。

## BATTLE-RENDER-PLAN1 集中式战斗渲染系统方案交接（更新于 2026-07-20）

方案入口：[central-battle-render-system-plan.md](central-battle-render-system-plan.md)。当前状态为 **R1-R2C-4、B0、B1-B1.3、B2A 与 B2B generation-aware incremental Loose Quadtree 已完成代码层实施和既定验证**。

- **已落地**：`BattleRuntimeProfile` / `BattleRuntimeProfileResolver`；生产解析顺序为命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认。平台宏只负责默认值：Editor/其他平台为 `Authority400`、Android Player 为 `MobileExtended`、Standalone Player 为 `DesktopExtended`。Unity 条件编译符号不进入战斗 pass；后续设备能力检测只允许选择或降级渲染后端。
- **已接线**：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析/创建路径；直接 `BattleTestBootstrap` 在实体注册前协调晚到的 GameConfig。`Authority400` 使用 `0..19`、`20..49`、`50..399` 三段 indexed binary min-heap + `nextUnused`；Mobile total active admission 与 Desktop 自动分页增长已接入，Desktop 增长保留最低空洞并同步 AI snapshot。
- **fresh 验证**：相关源码 `2026-07-20 11:49:59` < Unity `Assembly-CSharp.dll` `12:04:36` < 完整 `BattleRuntimeSelfCheck` `12:05:07` **PASS**；100,000 次随机 claim/release/allocate 与朴素扫描模型逐步对照 **PASS**；架构复核 **PASS**。
- **R2A 已落地 / 已验证**：独立 `RuntimeSlotTable` 固定 256 槽/页并按需物化；`Authority400` 的 400 逻辑地址、`MobileExtended` 设计所需的 1050 逻辑地址及尾页 guard、每槽独立 raw runtime/rest、`ClaimedCount` 与 `(slot, generation)` 句柄契约均有 focused self-check。release、同槽 reuse 与 reset 后旧句柄均失效。
- **R2A fresh 验证**：相关源码 `2026-07-20 12:33:20` < Unity `Assembly-CSharp.dll` `12:36:25` < 完整 `BattleRuntimeSelfCheck` `12:36:53` **PASS**；架构复核 **PASS**。
- **R2B 已落地 / 已验证**：生产 `Authority400` registry 已由单一 `RuntimeSlotTable` 替换 used/raw runtime/raw rest 并行数组；slot 当前 occupant 为 O(1) 查询。live ascending scan 保留 high-newborn / low-reuse 时序；release 以 `expectedEntity`/当前 occupant 防止旧实体释放复用槽；stage/ordinary raw rest 语义、`ObjectCount`、buckets 与 `SceneQueryHit` slot-address 契约保持不变。
- **R2B fresh 验证**：生产源码 `2026-07-20 12:55:14` < Unity `Assembly-CSharp.dll` `12:56:37` < 完整 `BattleRuntimeSelfCheck` `12:57:02` **PASS**；fresh `dotnet build` **0 errors**；架构复核 **PASS**；旧并行 registry 字段检索 **0**。
- **R2C 已落地 / 已验证**：`RuntimeSlotAllocator.GrowTo` 与 `RuntimeSlotTable.GrowTo` 只允许单调增长；增长保留 dynamic min-heap、`nextUnused`、claims、既有 pages、occupants、generation handles、raw runtime/rest，并优先复用旧低槽空洞。等容量调用为成功 no-op；缩容拒绝且原状态不变。
- **移动端地址契约修正**：`1000 active` 是 admission 预算，不是最大 slot address。保留 `0..49` 后，1000 个动态槽为 `50..1049`，故逻辑地址容量为 `1050`；`PageSize=256` 时物理需要 5 页，但 `1050..1279` 尾部地址必须不可访问、不可 claim、不可创建 raw runtime。
- **R2C fresh 验证**：相关源码 `2026-07-20 13:23:00` < Unity `Assembly-CSharp.dll` `13:24:49` < 完整 `BattleRuntimeSelfCheck` `13:25:34` **PASS**；fresh `dotnet build` **0 errors**；架构复核 **PASS**。
- **R2C-3A 已落地 / 已验证**：`SimulationWorld.RuntimeSlotCapacity` 读取当前 `_runtimeSlots.LogicalCapacity`；registry、frame input、entity passes、query/link、stage wave 与 AI 的真实 world 容量循环已改为实例容量。默认 `SimulationWorld()` 仍创建 `Authority400/400`。
- **R2C-3A focused 契约**：internal `DesktopExtended/512` world 仅用于代码层验证；slot `511` 可注册、查询并进入 AI 目标扫描，slot `512` 被拒绝，reset 后高槽被清理。`BattleParitySnapshot` 继续固定 400-slot authority schema。
- **R2C-3A fresh 验证**：相关源码约 `2026-07-20 13:45:39` < Unity `Assembly-CSharp.dll` `13:51:07` < 完整 `BattleRuntimeSelfCheck` `13:54:22` **PASS**；fresh `dotnet build` **0 errors / 42 warnings**。
- **R2C-3B 已落地 / 已验证**：`LF2SpecialAttack` 的高槽 holder 验证和 Karasu oid209 扫描读取当前 world capacity；`LF2Entity` transition effect 统计当前 dynamic range，不再固定 `50..399`。
- **parity capture guard**：历史 capture 必须同时满足 `Authority400` Profile 与 400 逻辑容量；`DesktopExtended/512`、`DesktopExtended/400` 都被拒绝，现有 400-slot schema 不能用于非 authority Profile。
- **R2C-3B fresh 验证**：相关源码 `2026-07-20 14:37:37` < Unity `Assembly-CSharp.dll` `14:38:09` < 完整 `BattleRuntimeSelfCheck` `14:44:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors**，warnings 为既有告警。
- **R2C-4 Profile 优先级**：命令行显式覆盖 > `GameConfig.BattleRuntimeProfileName` > 平台宏默认。默认容量为 `Authority400=400`、`MobileExtended=1050 logical / TOTAL active admission 1000`（跨全部槽区）、`DesktopExtended=512 initial`（按 256-slot 页规范化并自动增长）。
- **R2C-4 生产接线**：`SimulationTickDriver.Awake`、`Recreate`、`ApplyMatchConfig` 共用 Profile 解析/创建路径；直接 `BattleTestBootstrap` 在实体注册前协调晚到的 GameConfig。Desktop 增长保留最低空洞优先并同步 AI snapshot。
- **R2C-4 checksum 边界**：Extended Driver checksum 跳过/为空；direct parity capture 继续严格拒绝非 `Authority400/400`，Extended replay/checksum schema 尚未实施。
- **R2C-4 fresh 验证**：相关源码 `2026-07-20 15:24:26` < Unity `Assembly-CSharp.dll` `15:25:30` < 完整 `BattleRuntimeSelfCheck` `15:26:04` **PASS**；fresh `dotnet build Assembly-CSharp.csproj` **0 errors / 42 existing warnings**；architect final review **PASS**。
- **B0 shadow Loose Quadtree 已落地 / 已验证**：纯数据 X/Z half-open tree，`looseness=1.5`、`leafCapacity=16`、`maxDepth=8`；每次 collision collect 全量重建，诊断默认关闭。比较 brute AABB pair、tree pair 与 accepted subset，正式 `i/j`、VRest、RNG、candidate flow 不变。
- **B0 fresh 验证**：相关源码不晚于 `2026-07-20 16:14:10` < Unity `Assembly-CSharp.dll` `16:14:27` < 完整 `BattleRuntimeSelfCheck` `16:15:43` **PASS**；fresh `dotnet build` **0 errors**；`NTSDParity` **19 PASS**；architect final review **PASS**。
- **B1 `RuntimeRestStore` 已落地 / 已验证**：分页/惰性 ARest；定向稀疏 `VRest[victim, attacker]` 只存正值、写零移除；`ResetSlot` 清 ARest + victim row + attacker column；支持 `GrowTo`、全局 reset、排序 diagnostics/snapshot 与 restore。
- **B1 differential / fresh 验证**：2,000 次随机操作与 dense reference model 逐步对照 PASS；源码 `2026-07-20 16:31:32` < Unity `Assembly-CSharp.dll` `16:36:38` < 完整 `BattleRuntimeSelfCheck` `16:37:13` **PASS**；fresh `dotnet build` **0 errors**；architect final review **PASS**。
- **B1.1 optional facade 已落地 / 已验证**：`LF2ItrRestTracker` 可选绑定 `RuntimeRestStore`；exclusive victim-row lease 保证同一 victim row 同时只有一个 facade owner，释放后才允许接管。B1.1 阶段未 production-bound，后续已由 B1.2 接入。
- **B1.1 architect 首轮修正**：`ReplaceVictimState` 对 mixed-invalid attacker 输入原可能部分写入；现已先完整预验证再原子替换。direct `ReplaceVictimState` 与 facade `Bind` 均补 failed-import 原状态不变测试。
- **B1.1 修正后 fresh 验证**：复跑 `dotnet build` **0 errors / 18 existing warnings**；源码 `2026-07-20 17:34:22` < Unity `Assembly-CSharp.dll` `17:36:49` < 完整 `BattleRuntimeSelfCheck` `17:39:07` **PASS**；architect final review **PASS / no blocker**。
- **B1.1 非阻塞补强**：invalid bound `RestoreState` 可后续补独立断言；该路径复用已验证 atomic replace 入口，不影响当前结论。
- **B1.2 lifecycle binding**：`SimulationWorld` owns store；ordinary claim、release、`StageSpawnAt`、world reset/grow 与 parity fallback 已接入，`RuntimeSlotTable.RawRest` 已删除。
- **B1.2 三轮审查归属**：初轮发现 Stage pool 回收不完整与错槽 release 未拒绝，次轮发现 release 拒绝未传播，末轮 PASS/no blocker；partial import 属于 B1.1，不计入 B1.2。
- **B1.2 三个 architect blockers 已修 / self-check verified**：Stage rejected bind 走共享完整 pool 回收；错槽 release 被拒绝；`ReleaseRuntimeSlot` bool 事务传播到全部注销/待销毁调用链，拒绝时不再半注销。
- **B1.2 旧 fresh 证据**：`dotnet build` **0 errors**；源码 `18:11:41` < DLL `18:12:23` < self-check `18:13:00` **PASS**。该证据早于 blocker 修复，只证明初版可编译/旧断言通过。
- **B1.2 第一轮修复证据**：源码 `18:21:20` < DLL `18:21:58` < self-check `18:22:59` **PASS**；第二轮审查随后发现半注销 blocker，因此仍是非完成证据。
- **B1.2 最新 fresh 证据**：`dotnet build` **0 errors**；源码 `18:31:25` < DLL `18:33:58` < self-check `18:34:54` **PASS**。公开 `Unregister` 故障测试验证 bucket/slot/lease/store/entity 完整注册上下文不变；architect final review **PASS / no blocker**。
- **B1.3 tick 解耦已实现 / self-check verified**：`CaptureSnapshots -> sparse Tick -> Collect`；eligible active+CharData row 递减，inactive row 冻结；`BruteForceSceneQuery` 已删除 pair 内 tick。
- **B1.3 初版非完成证据**：源码 `19:09:44` < DLL `19:10:34` < self-check `19:11:13` **PASS**；architect 随后发现 eligibility 仍按 capacity 全扫。
- **B1.3 sparse 修复**：eligibility 直接遍历 registered bucket items，无 capacity scan/snapshot 分配；Desktop sparse high-slot `visited=2`。active-positive-row/stamp + scratch 预扩继续保持。
- **B1.3 最终 fresh 证据**：`dotnet build` **0 errors**；源码 `19:19:14` < DLL `19:19:47` < self-check `19:22:50` **PASS**；architect final review **PASS / no blocker**。
- **B2A 后端选择**：新增独立 `BruteForce` / `LooseQuadtree` formal backend；命令行 `-ntsdCollisionBroadphase` > `GameConfig.BattleCollisionBroadphaseName` > 默认 `BruteForce`，不按平台分叉战斗规则。
- **B2A 固定帧边界（历史）**：B2A 当时仅接管 fixed-tick candidate collect，保持 `CaptureCollisionFrameSnapshots -> TickCollisionPairVRest -> CollectCollisionCandidates`；即时 weapon/body current-world query 已由后续 B2C 在显式 `LooseQuadtree` 后端下接入，并保留 brute fallback。
- **B2A pair/回退契约**：eligible participant 保留 authority ordinal；tree pair 与 invalid-AABB fallback-all pair 使用 canonical slot key 合并、排序、去重，再按原 ordinal 双向派发。slot/mapping/index/count 异常、rebuild/query exception 或 diagnostics 缺 brute coverage 时整 tick brute fallback；formal 失败会恢复 RNG 并清除 candidate 中间态，保证 candidate 20 上限、tie、RNG 与消费顺序不被部分执行污染。
- **B2A fresh 证据**：源码 `2026-07-20 22:15:07` < Unity `Assembly-CSharp.dll` `22:18:48` < full `BattleRuntimeSelfCheck` `22:19:28` **PASS**；`dotnet build` **0 errors**；architect final review **PASS / no blocker**。
- **B2B 同步与身份**：formal backend 在 collision collect 边界 batch synchronize 当帧 participant；索引键改为 `(runtime slot, generation)` handle。同槽复用不会继承旧 occupant 的空间身份，query 结果必须经当前槽表 generation 解析并核对 entity/ordinal。
- **B2B 增量策略**：未移动实体保持原记录；AABB 改变但仍在当前节点 loose 范围内时原位更新，越出 loose 范围时迁移。spawn/remove、invalid AABB 与同槽 reuse 均在下一 collect 收口；root escape 才 full rebuild。
- **B2B 回退/lifecycle**：sync/query/invariant/mapping 异常会 reset 索引并整 tick brute fallback，继续使用 B2A 的 RNG/candidate rollback；world reset 显式清空 formal index。
- **B2B fresh 证据**：源码 `2026-07-20 22:43:57` < Unity `Assembly-CSharp.dll` `22:46:36` < full `BattleRuntimeSelfCheck` `22:47:04` **PASS**；`dotnet build` **0 errors**；architect final review **PASS / no blocker**。
- **后续边界（B2B 历史状态，已由 B2C 部分替代）**：生产默认仍为 `BruteForce`；即时 weapon/body 与 AI 查询、Extended checksum 已由 B2C 接入。Extended replay、Loose Quadtree 默认启用证据与完整渲染仍未完成。B2C 未执行 Play Mode 或性能验收；T8 已排除，默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT14 DAT movement 显式值读取回归交接（2026-07-19）

- **最新覆盖结论**：下方 BATTLE-AUDIT13 的“可玩 Naruto `oid2 running_speed=8`”与“`BattleVisualScale=1`”已经失效。生产 Naruto DAT 显式配置为 `running_speed=15`；Unity 实体表现缩放已恢复为项目要求的 `1.5`。
- **回归根因**：DATA-01A 把 `LF2CharacterData` 的兜底值从旧 `15` 改为 C# 权威默认 `8` 本身正确，但 Unity parser 未读取 `<bmp_begin>` 内无冒号的 movement `key value`，使生产显式 `15` 回退到 `8`。这是 Unity loader bug 和对齐回归；此前将慢速主要归因于缩放并不完整。
- **生产修复**：`Lf2DatParserV2` 仅对白名单中的 BMP 顶层 18 个 movement 键接受无冒号 `key value`；`ExtractMovementParameters` 现读取 `Bmp.Properties`；浮点字段和 `frame_rate` 均以 `InvariantCulture` 解析。DAT 缺字段时仍保留 C# 默认 `8`，没有恢复错误的 Unity 默认 `15`。
- **测试矩阵**：生产 DAT 覆盖 Naruto `15`、Kakashi `18`、Sakura `17`、Sasuke `23.9`、clone `15`，并保留 weapon4 冒号语法 guard；synthetic 覆盖全部 18 键、last-wins、frame 隔离与缺省 `8`。
- **同类风险审计**：已审计当前 101 份 DAT。除上述 5 份角色 DAT 的 18 个 movement 字段外，没有第二组当前生产数据触发同类遗漏；weapon/frame/stage/data 当前安全。多词 `name` 是非战斗的潜在表示风险；`catchingact/caughtact` 双值为未来风险，但当前 218 处两值均相等，当前无可观察战斗差异。
- **fresh 验证**：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 14:39:43.992`，晚于相关源码，Console C# error 为 **0**。一次请求因 Editor 误留 Play Mode 未计入结果；退出后 fresh full `BattleRuntimeSelfCheck` 于 `14:44:58.748` 返回 **PASS**。
- **未验收边界**：真实双击 D Play trace 因 UnityMCP 临时注入卡住而未完成，本轮不宣称 Play Mode 通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT13 Naruto 防下攻与跑速缩放交接（2026-07-19）

- 常规战斗逻辑的唯一权威仍为 `J:\QQFile\NTSD2.4\ntsd_release_C#`。本项是用户明确指定的例外：Naruto 防下攻以用户已验证表现正确的 `J:\QQFile\NTSD2.4\ntsd_release` C++ 版本作定向参考；该例外不扩展到其他战斗逻辑。
- C++ 行为依据：`oid2 frame286` 的 `centery=79`，opoint 为 `y=80 action=240 dvy=0 oid=33`，child 初始为 `Y=+1, Vy=0`。角色物理落地要求 `new_y > 0.0001 && pre-move Vy > 0.0001`，所以不会立即进入 frame219；后续链为 `240 -> 241 -> 242 -> 243 -> 235 -> 236(dvy=-7) -> 244..247`，真实下降落地后才进入 `219 / AI`。
- Unity 根因与修复：`CharacterMechanics` 的 `landed` 判定缺少 `Vy` 门槛；旧 `LateOpoint + state15` 专项 gate 过宽，并且仍会把 `Y` 钳为 0。现已改为通用 `landed` 条件并移除专项 gate；`CheckLateOpointState15LandingControls` 与 `PH-02` 三向速度矩阵已同步更新。
- 跑速测试状态：按用户要求，`BattleVisualScale` 临时由 `1.5` 改为 `1`，供用户复测奔跑速度体感。可玩 Naruto `oid2` 的逻辑 `running_speed` 仍为 `8`，固定逻辑频率仍为 30 Hz，本轮没有修改逻辑跑速。

fresh 验证链：`dotnet build` 为 **0 errors / 72 warnings**；Unity `Assembly-CSharp.dll` 时间 `2026-07-19 03:21:41.985`，晚于测试时间 `03:20:06.169`；Console C# error 为 **0**；fresh full `BattleRuntimeSelfCheck` result 时间 `03:22:49.668`，结果 **PASS**。本轮没有可复用的真实 Play 自动 trace 入口，因此没有重新运行真实 Play trace；防下攻与 scale 1 奔跑仍需用户手测，当前不宣称 Play Mode 验收通过。T8 默认 `stage.dat` 部署继续暂缓。

## BATTLE-AUDIT12 代码差异修复与 fresh 验证（2026-07-18）

本段是当前交接状态，并覆盖下方 BATTLE-AUDIT9/10/11 的历史冻结措辞。用户负责 4 组 Naruto/武器 Play Mode 场景，本轮不运行也不代替其验收。最新 freshness：相关源码最晚 `BattleRuntimeSelfCheck.cs` `16:44:31.210` < Unity `Assembly-CSharp.dll` `16:45:52.868` < self-check result `16:46:29.080` **PASS**；fresh `dotnet build` 为 **0 errors / 18 warnings**。

- `FW-FLOW-01`：已恢复普通 tick 的 cooldown-before-human-input 顺序，focused check 与 full self-check 通过。
- `LP-03`：typed/generic formal throw 已移除 `Zz=1` 额外层级，release 矩阵通过。
- `LP-05`：formal release、consume、force-clear 的 `TargetIdx/HolderIdx/HeldWeaponSlot/HolderCopy` 写入边界已按 authority 分开，typed/generic 矩阵通过。
- `FW-RESULT-01`：固定 roster slot、inactive/dormant 与 alive/team bucket 矩阵已补齐并通过。
- `UNRES.04`、`DATA-01A-D`：生产修复和对应断言均进入本次 fresh full PASS；此前 transformed landing 阻塞已由 authored-frame gate 修复消除。
- `FW-FLOW-02`：Unity 生产无 writer，authority 仅 Host debug/step 控制，归 dormant/scope-excluded。
- `FW-BOOT-01/02`：旧表误把 rematch-only 写入及普通 reset 后偶合等价字段记成正式差异；普通非-rematch 路径关闭为 equivalent，result rematch 保持 scope-excluded。
- `FW-RESET-01/DEP.RNG.01`：保留 per-world lockstep RNG adapter；算法等价，不迁移为进程静态 owner。

当前 code-only 清单没有未修复的 confirmed item；但这只关闭脚本差异与 self-check 层，不是 Play Mode 结论，也不是完整逐帧 production certificate。T8 默认 `stage.dat` 部署继续暂缓，raw DAT 表示差异继续排除。

## BATTLE-AUDIT11 代码层 12 项待确认项已全部定性（2026-07-18）

本轮只核验脚本/代码层，不进行 Play Mode、资源部署、DAT 文件表示或场景/表现确认；核验后的 Unity 代码修复已落地，但最新 fresh Unity full self-check 仍为 **FAIL**。2026-07-18 最新 fresh run 的 `CheckStateTransformLandingMatrix` transformed landing fixture 断言失败，实际为 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`；这是既有代码契约回归，不是 Play Mode 结论。最终依据为：

- `.omc/research/final-verify-unres-02-05-code-parity-20260718.md`
- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

分类汇总：

- **equivalent / Unity-adapter**：`UNRES.01/02/03/05`、`DEP.INT.01-04`、`DEP.WORLD.01`。
- **confirmed code difference**：`UNRES.04`、`DATA-01A`（`running_speed` 默认值）、`DATA-01B`（frame index 容量）、`DATA-01C`（合法缺帧语义）、`DATA-01D`（cpoint front/back action alias）。首轮修复已落地，但 fresh full self-check 仍被 transformed landing fixture 回归阻塞。
- **Unity-adapter / policy-open**：`DEP.RNG.01`（LCG 算法等价；owner/reset 边界保留为 Unity lockstep 策略待定）。
- **关联确认代码差异**：`FW-RESULT-01`（非正常 roster/lifecycle 下 dormant/inactive 选择与 relation identity alias）。
- `DATA-01E` 为当前 consumer 已屏蔽的 adapter/masked，`DATA-01F` 为 schema-only omission，`DATA-01G` 已在源码闭合，不计为正式 runtime 差异。

在本轮 **code-only scope** 下，原先剩余的 4 个 `authority-unresolved`（`UNRES.02`-`UNRES.05`）现已全部定性，数量为 **0**。这不是修复完成声明：`FW-RESULT-01` 仍是确认差异，且 `UNRES.04`/`DATA-01A-D` 的 fresh full self-check 被 transformed landing fixture 回归阻塞；4 组 Play Mode 场景仍由用户自行验证，本轮不改变 LP 或 Play 验证状态。

## BATTLE-AUDIT10 代码核验结果（2026-07-18）

本轮只处理代码层面的待确认项，不进行 Play Mode、资源部署或场景/表现验证，也未修改任何生产代码。核验报告：

- `.omc/research/verify-authority-unresolved-input-20260718.md`
- `.omc/research/verify-authority-unresolved-world-rng-20260718.md`
- `.omc/research/verify-authority-unresolved-data-results-20260718.md`

结论与交接状态：

- 已闭合为 **equivalent / Unity-adapter**：`UNRES.01`、`UNRES.02`、`UNRES.03`、`UNRES.05`、`DEP.INT.01`-`DEP.INT.04`、`DEP.WORLD.01`。
- 已升级为 **confirmed code difference**：`UNRES.04`、`DATA-01A`/`DATA-01B`/`DATA-01C`/`DATA-01D`（DAT parser/runtime contract）；首轮修复已落地，但最新 full self-check 仍被 `CheckStateTransformLandingMatrix` transformed landing fixture 回归阻塞（实际 `frame=60/runtimeFrame=60/durability=15/state=1004/vy=0/vx=8.4`）。
- `DEP.RNG.01` 为 **Unity-adapter / policy-open**（算法等价，owner/reset 边界待策略决定）；`FW-RESULT-01` 仍为未修复的确认差异（非正常 roster/lifecycle 的结果 slot/relation identity）。
- `DATA-01E` 为 **Unity-adapter / masked**，`DATA-01F` 为 **schema-only omission**，`DATA-01G` 为 **closed in source**，不作为正式 runtime 差异计数。
- **BATTLE-AUDIT10 历史中间快照**曾保持 `UNRES.02`-`UNRES.05` 为 authority-unresolved；该状态已被 BATTLE-AUDIT11 取代，当前 code-only scope 下 02/03/05 为 equivalent，04 为 confirmed difference。

**本段为 BATTLE-AUDIT10 历史中间快照，已被 BATTLE-AUDIT11 取代。** 当时统计为剩余 authority-unresolved 4 项（`UNRES.02`-`UNRES.05`）；当前 code-only scope 下已全部定性为 0。BATTLE-AUDIT9 的 LP 项状态和计数保持不变，4 组 Play Mode 场景仍由用户自行验证，本轮不对其下结论。以上仅是代码核验，不是完整战斗逻辑对齐声明。

## BATTLE-AUDIT9 差异盘点冻结（2026-07-18）

当前执行口径已切换为“先完成差异盘点，再按文档集中修复”。本轮只读合并以下报告，**没有按冻结清单修改生产代码**：

- `.omc/research/full-diff-inventory-framework-20260718.md`
- `.omc/research/full-diff-inventory-input-interaction-20260718.md`
- `.omc/research/full-diff-inventory-lifecycle-presentation-20260718.md`
- `.omc/research/reaudit-open-differences-20260718.md`

冻结计数（**BATTLE-AUDIT9 历史快照，已由 BATTLE-AUDIT11 取代**）：9 个正式 runtime 差异、1 个工具/trace 差异、12 个 authority-unresolved 待确认项、4 个 Play Mode 未验证场景。原 12 项现已在 code-only scope 下全部定性；正式差异表保留作历史追踪。

| ID | 权威 C# | Unity 对应 | 触发与预期/实际 | 证据/分类 |
|---|---|---|---|---|
| `FW-FLOW-01` | `BattleCore/Simulation/GameTick.cs:53-67` cooldown/step gate 在 input 前 | `Assets/NTSD/Scripts/Simulation/NTSDBattleTickSystem.cs:32-43`、`RunFrameAdvancePhase` | ARest/AttackExempt 与输入边沿同 tick 到期；应先递减再读，Unity 先读 | 静态 confirmed-difference，未修复 |
| `FW-FLOW-02` | `GameTick.cs:56-67` `BattleStepGate44905C` mode=2 转换与抑制 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:272-281`、`NTSDBattleTickSystem.RunReleaseTick` | 单步/慢速模式；应 gate，Unity 无转换/抑制 | 静态 confirmed-difference，生产可达性待确认 |
| `FW-BOOT-01` | `DirectBattleBootstrap.cs:138-140` 写 `Unk344`/`HolderCopy` | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 未显式写两字段 | 初始玩家统计/holder 分支；应 team/slot，实际可能 `0/99` | 静态 confirmed-difference，未修复 |
| `FW-BOOT-02` | `DirectBattleBootstrap.InitializeBattleStats:224-244` 完整 difficulty/HP/PP/respawn/Cd/edge | `Assets/NTSD/Scripts/App/AppManager.cs:224-235` 依赖隐式初始化 | 非默认 difficulty、DAT Hp3、复用；应完整字段契约，实际缺显式写入 | 静态 confirmed-difference，未修复 |
| `FW-RESET-01` | `SimulationWorld.Passes.cs:13-70` reset 不播 RNG | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs:138-151` reset 播 `0x4E545344` 后再播 config seed | 连续重开/重赛随机序列；应遵循权威播种边界，实际 Unity 增加边界 | 静态 confirmed-difference，播种归属待确认 |
| `LP-01` | `BattleCore/Interaction/WeaponRuntime.cs:169-212,287-303` generic held throw/kind3 写 `ReleaseTick` | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` generic throw/kind3 通过 `ClearLinks(..., stampReleaseTick: true)` 写当前 tick | generic DAT held 正式释放；应清 link并写当前 tick | confirmed-difference；**代码已写 / `CheckAudit9GenericHeldReleaseTickContracts` self-check verified / Play-unverified** |
| `LP-02` | `src/Host/SdlBattleRenderer.cs:476-497` 同 Z 按 slot 稳定排序 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` compact presentation sort、`Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs` `ForceRefresh`；`LF2Sprite.cs` 表现刷新 | 同 Z 实体；按 `(ZInt, runtime slot)` dense rank，四槽中实际使用 `Shadow/Entity/HitRecord=0/1/3`；同层统一 `Object` sorting layer。真实双实体 renderer 检查为 `0/1/4/5` | confirmed-difference；**代码/self-check/architect verified / Play-unverified** |
| `LP-03` | `BattleCore/Interaction/WeaponRuntime.cs:169-212` 释放不写额外 Zz | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:77-98,391-402` 写 `Zz=1` | 正式投掷；应由 Z/slot 决定，实际额外抬层 | 静态 confirmed-difference，未 Play |
| `LP-04` | `src/Host/SdlBattleRenderer.cs:519-548` 实体/阴影按负 HitStop 阈值与四拍相位隐藏 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs:416-448`、`LF2ObjectRenderer.cs:206-243` 已接入 gate | 负 HitStop 闪烁区间；应按实体/阴影各自阈值与四拍相位隐藏 | confirmed-difference；**代码已写 / `CheckHitStopPresentationGates` self-check verified / Play-unverified** |

### BATTLE-AUDIT9 修复进度（LP-01 / LP-04）

Fresh verification: `Assembly-CSharp.dll` `2026-07-18 14:01:27.540`; full `BattleRuntimeSelfCheck` `2026-07-18 14:01:51.078` returned **PASS**.

冻结后仅 `LP-01`、`LP-04` 更新为“**代码已写 / self-check verified / Play-unverified**”，其余冻结状态和计数不变，整个差异清单仍保持开放。`LP-01` 已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponHeldStateResolver.cs:391-424` 的 `ThrowHeldObject`、`DropRandomly`、`ClearLinks(..., stampReleaseTick: true)` 补齐 generic held `ReleaseTick`，由 `Assets/NTSD/Scripts/Test/BattleRuntimeSelfCheck.cs:3062` `CheckAudit9GenericHeldReleaseTickContracts` 覆盖；`LP-04` 已在 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2ObjectRenderer.cs:206-243` 的 `UpdateSprite`、`ShouldDrawEntityForHitStop`、`ShouldDrawShadowForHitStop` 接入表现门控，由 `BattleRuntimeSelfCheck.cs:1394` `CheckHitStopPresentationGates` 覆盖。

验证证据：`dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 warnings**；`Assembly-CSharp.dll` `13:43:59.791` 晚于本轮最新相关源码，fresh Unity full `BattleRuntimeSelfCheck` 于 `2026-07-18 13:44:26.093` 返回 **PASS**。两项仍需 Play Mode 定向验证：generic held 的实际投掷/掉落，以及负 `HitStop` 下实体与阴影的阈值隐藏和四拍闪烁。

`LP-05`（新增 reviewer 候选，只记录、不修复）：权威 `BattleCore/Interaction/WeaponRuntime.cs:289-295` `ReleaseHeldWeaponRuntime` 不清 `holder.TargetIdx`/`held.HolderIdx`；Unity `Assets/NTSD/Scripts/Animation/LF2Objects/LF2WeaponReleaseFlowResolver.cs:23-28,39-59` 正式 release 当前清 holder `TargetSlotIndex` 与 held `HolderStableId=-1`，generic `ClearLinks` 也有同类清理。分类保持 **confirmed-candidate / 未修复 / 需 authority 调用链与 Play Mode 复核**，不纳入 `LP-01` 已写结论，也不改变冻结计数。

`RT.CHECK.01` 是 `CharacterSync.cs:796-877,173-317` 内部 snapshot 与 `BattleParitySnapshot.cs:385-542` trace projection 的 schema/alias 差异，分类为 **validator adapter**，不是 runtime 语义差异（见 `reaudit-open-differences-20260718.md:44-56`）。12 个 unresolved 只保持待确认，不得猜测为等价。四个 Play 未验证场景的详细输入与预期见主文档 BATTLE-AUDIT9 详细冻结表：Naruto 防下跳六分身、防前跳螺旋丸、奔跑防跳后续招、投掷武器首击/持续命中。

F1-F7 仅 static/focused self-check 闭合，不能替代 Play Mode；DAT 表示差异不处理，T8 默认 `stage.dat` 部署继续暂缓，fixed-world camera 为批准的 Unity adapter。修复阶段必须从本冻结表开始，逐项取得编译、self-check 和必要 Play Mode 证据后再更新状态。

## BATTLE-AUDIT8 当前交接（2026-07-18，继续开放）

- fresh Unity full `BattleRuntimeSelfCheck` 已于 `2026-07-18 12:46:40.638` 返回 **PASS**；freshness 为 test source `12:45:10.110` < `Assembly-CSharp.dll` `12:46:15.927` < result `12:46:40.638`。
- F6/R1 的生产修复位于 `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Character.cs`：`UpdateLocalInputStateFromControllerBuffer` 先 `SyncFromRuntime`，再轮询 controller buffer；权威对照为 `BattleCore/Input/InputRuntime.cs` 的 human poll/cooldown runtime 真值及 `BattleCore/Simulation/GameTick.cs` 的 results early return。
- `BattleRuntimeSelfCheck.cs` 的 `TemporaryAppManagerRuntimeScope` 只修复 EditMode 下测试 fixture 的 AppManager singleton/Awake 生命周期；生产 `Assets/NTSD/Scripts/App/AppManager.cs` **未修改**。
- Frame/Input 已完成 **237/237** 分类：39 equivalent、181 Unity-adapter、4 confirmed-difference、1 missing、12 authority-unresolved。完整账见 `.omc/research/unity-frame-input-mapping-complete-20260718.md`；`FLOW.05`、`FLOW.09`、`IN.CD.02`、`RT.CHECK.01`、`RT.LINKS.01 / ReleaseTick` 及 12 个 unresolved 在按最新生产代码重新核销前仍开放。
- 该 PASS 只覆盖当前 self-check，不等于全战斗逐帧最终完成；后续仍需静态重审、必要 Play Mode/双端 trace 和最终独立复核。DAT 表示差异不处理；T8 默认 `stage.dat` 部署继续暂缓。

> 生成：2026-07-13 ｜ 供 Codex 或任何接手者直接开工，无需追溯历史会话。
>
> **当前状态（BATTLE-AUDIT7，2026-07-18）**：旧的“完整对齐/无剩余差异”推断已撤销。重新按唯一权威 C# 做完整框架正向映射和 Unity-only 反向审计后，确认 **13 个去重开放根因**：**12 个战斗 runtime/语义差异 + 1 个 trace 投影工具差异**，均为“已确认 / 未修复 / 未运行时验证”。Audit5 的 74/74 与原 15/15 仅表示历史批次已关闭；旧 `01:07:52.834 PASS` 和 Architect `P0/P1/P2=0` 不覆盖本轮新发现。

## 0. 你要做什么

把 **NTSD C# 战斗核心** 里 Unity 尚未对齐的战斗逻辑，逐条补齐 / 修正到 Unity 工程。
T0-T9、Audit2、Audit3、Audit4、Audit5 和 Audit6 只保留为历史实现/定向回归基线。Audit5 的 **74/74** 与原 trace 风险 **15/15** 仍是对应历史批次已关闭的记录，但不能覆盖 BATTLE-AUDIT7 的 13 个新开放根因，也不能作为当前完整对齐证明。C# 与 Unity 的 raw DAT/manifest 差异属于 Unity 适配预期，不是待处理项；T8 默认 `stage.dat` 继续暂缓。

- **唯一 gameplay authority**：`J:\QQFile\NTSD2.4\ntsd_release_C#`；核心战斗入口位于 `src\BattleCore`。旧工程、反汇编及历史对齐结论不得作为当前实现或验收依据。
- **被对齐工程**：`I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts`
- **完整差异清单（配套读）**：`Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md`
  - 本文是「行动版」，那份是「全量核实版」。当前状态优先回查其顶部 BATTLE-AUDIT7 章节。

## BATTLE-AUDIT7 全量权威映射交接（2026-07-18，开放）

### 覆盖与结论

- Framework：权威 **172/172 ID** 已映射；独立复核为 13 difference ID，去重为 **7 个 framework 根因**。反向 Unity-only 扫描没有发现这 7 项之外的新 framework 根因。
- Frame/input/physics/runtime：**[历史 BA7 快照，已由 BATTLE-AUDIT8 237/237 分类取代；当前开放项见顶部]**：旧记录曾记权威 **237/237 ID** 集合定位、4 difference ID + 1 missing ID，并注明其余 219 ID 尚未逐项拆分；该旧静态边界不再作为当前状态依据。
- Interaction：权威 **105/105 ID** 集合相等，但独立复核确认 **2 个正式可达差异**；原 0 difference 结论失效。
- 总账：framework 7 + frame/input 新增 3（Results 去重）+ interaction 2 = **12 个战斗 runtime/语义根因**；另有 **1 个 trace 工具根因**，合计 **13**。
- Frame/input 权威 ledger 有两处账本校正：字段组机械相加为 138，而 footer 写 137；`IN.JUMP.03` 曾误写权威成功 jump 清 8 Cd，实际权威与 Unity 都只清 7 个普通 Cd并保留 `CdDefendLock`，因此该 ID 为 equivalent。两者都不是 Unity 差异；`IN.CD.02` 的 AI 递减 ownership 根因仍成立，所以 13 个去重根因不变。

### 13 项开放根因

| 组 | 根因 | 关联 ID | 状态 |
|---|---|---|---|
| Framework | bootstrap 把 `WaveIdx -1 -> 0` | `FW-BS-008`,`FW-LC-004` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 8-slot roster 压缩且 independent team 未规范化 | `FW-BS-008-B1` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 初始出生 X/Z 与 RNG 消耗改用 scene transform | `FW-BS-008-B2` | 已确认 / 未修复 / 未运行时验证 |
| Framework | 初始 `HitStop=75,Vx=Vz=0.1` prime 缺失 | `FW-BS-009` | 已确认 / 未修复 / 未运行时验证 |
| Framework | stage spawn 经通用 Register 误清复用槽 ARest/VRest | `FW-WR-005`,`FW-TK-028`,`FW-H-050`,`FW-H-059`,`FW-LC-004` | 已确认 / 未修复 / 未运行时验证 |
| Framework | Results active 后仍执行普通战斗 pass | `FW-TK-002`,`FW-END-002`,`FLOW.05` | 已确认 / 未修复 / 未运行时验证 |
| Framework | `HitConfirm2` 等 candidate carrier 到下一次 collect 才清 | `FW-TK-034`,`FW-H-042` | 已确认 / 未修复 / 未运行时验证 |
| Frame/input | `CdDefendLock` 错对 AI 递减；成功 jump 双方均清7个普通Cd并保留lock，不是差异 | `IN.CD.02`；`IN.JUMP.03` 已移出差异账 | ownership 已确认 / 未修复 / 未运行时验证 |
| Frame/input | late holder 改帧后再次写 held Frame/位置 | `FLOW.09` | 已确认 / 未修复 / 未运行时验证 |
| Frame/input | `ReleaseTick` storage/writer/hash 缺失 | `RT.LINKS.01` | 已确认 / 未修复 / 未运行时验证 |
| Interaction | IronBall type2 的 dvx/dvy 预处理 gate 错落到 type6 | `INT-HIT-005` | 已确认 / 未修复 / 未运行时验证 |
| Interaction | late opoint child X/Y 使用浮点 `PS`，未按 spawner `XInt/YInt` | `INT-OP-001`,`INT-OP-002` | 已确认 / 未修复 / 未运行时验证 |
| Trace 工具 | `BattleParitySnapshot` 对空槽/category、release、block、transform/weapon/owner 等字段硬编码或错映射 | `RT.CHECK.01` | 已确认 / 未修复 / 未运行时验证 |

每项的权威方法、Unity 对应、可复现前置、预期/实际和依赖见完整差异清单的 BATTLE-AUDIT7 总表。DAT 文件适配不处理；T8 默认 `stage.dat` 部署暂缓，stage runtime 用内存 fixture；fixed-world camera 和不改变逻辑结果的 Unity-native 适配保持。

### 行动顺序

1. 先修 tick/runtime 契约：Results early return、`CdDefendLock`、late held、`ReleaseTick`、candidate carrier；同步补 focused self-check。
2. 修 interaction：IronBall type gate、late opoint 整数 X/Y；覆盖 real/shared、正负和跨零坐标。
3. 修 bootstrap/stage：WaveIdx、8-slot/team、spawn RNG、HitStop/velocity prime、stage rest policy；全部使用内存 fixture，不部署默认 `stage.dat`。
4. **历史行动项（已由 BATTLE-AUDIT8 取代）**：修 trace snapshot 投影，并完成剩余 219 个 frame/input ID 的逐项 equivalent/adapter 分类和反向 Unity-only 零未分类核销；237/237 分类现已完成，但 trace snapshot 等开放差异仍需按最新生产代码重新核销。
5. 最后跑 fresh Unity 编译、full `BattleRuntimeSelfCheck`、normal + hole/independent roster Play Mode、held/opoint/结果态定向场景，再做独立 Architect 复核。证据齐全前不得宣称完整战斗逻辑对齐。

---

## Audit5 全量逐帧审计交接（2026-07-18，风险账已收口）

### 权威与历史结论

- 唯一战斗逻辑权威是 `J:\QQFile\NTSD2.4\ntsd_release_C#`。所有差异定性、修复方向和预期 trace 都必须从该工程的正式 C# 调用链闭合。
- 旧章节中依赖其他来源得出的“已对齐”“已关闭”或“仅作映射参考”结论，在 Audit5 当前状态下全部废止为权威证据。它们只能保留为历史回归基线；未经 C# 重审、fresh 验证和双端 trace，不得恢复完成状态。
- T8 默认 `stage.dat` 部署继续暂缓；默认 trace 使用 `stageFixture=false`，不读取或生成默认资产。

### 当前总账

| 分区 | 报告结果 | 当前实现与 fresh 证据 | 风险账状态 |
|---|---|---|---|
| GameTick / Physics | 21 确认 + 3 风险；正式主干和 Physics 全分支 100% 审计 | `GT-01..15`、`PH-01..06` 共 **21/21 逻辑已写并进入 fresh full PASS** | `R-GP-01..03` **3/3 关闭** |
| HitResolve / CollisionCollect | 33 确认 + 6 风险；两个权威入口全分支审计 | `C-01..33` 共 **33/33 逻辑已写并进入 fresh full PASS** | `R-HC-01..06` **6/6 关闭** |
| Frame / lifecycle | 20 确认 + 6 风险；25/25 方法及 reset/registry/rest 依赖审计 | `FL-01..06`、`FT-01..04`、`OP-01..05`、`LC-01..05` 共 **20/20 生产实现与 focused/full self-check 通过** | `R-FL-01..03`、`R-LC-01..02`、`R-FT-01` **6/6 关闭** |

跨分区原始确认项现为 **74/74 逻辑实现 + focused/full self-check**，原 15 项风险为 **15/15 已关闭**。`BATTLE-AUDIT6-01/02` 是原总账后新增且已关闭；CP-NV1/2/3 与 STEP10 是既有项重开后重新关闭。原 3 个受控 P2 已补强并关闭；最终 freshness 链为 source `2026-07-18 01:06:21.499` < Unity DLL `01:07:21.125` < result `01:07:52.834` **PASS**，Architect `P0/P1/P2=0`。这仍不是任意对局、全输入 production trace certificate。

原 3 个受控 P2 的关闭证据：HC-04 已覆盖真实 step6 `collect -> wrong loop 不消费 -> post consumer 消费` 整链及 current type3 负例；missing-definition 已覆盖 Character/Weapon 的完整错误循环、正确循环与 tail；`LF2CharacterInteractionResolver` 的本地类型 helper 仅单行委托中央 `LF2Entity.ResolveCurrentDataObjectType`，不再存在第二份类型判定维护漂移。

### 原 15 项 trace 风险关闭状态

| 分区 | 风险 | 状态 |
|---|---|---|
| GameTick / Physics | `R-GP-01` | ✅ fresh 2 tick trace 关闭；tick1 slot0=`frame0/wait37/FWC11/HitStop75`，tick2=`frame5/wait37/FWC0/HitStop74`，双方一致 |
| GameTick / Physics | `R-GP-02` | ✅ production `mass > 0` 扫描，static close |
| GameTick / Physics | `R-GP-03` | ✅ central active filter 关闭 |
| Hit / Collision | `R-HC-01` | ✅ 确认差异并修复 zero-width strict overlap；90 项已知非正宽 geometry 纳入权威等价覆盖 |
| Hit / Collision | `R-HC-02` | ✅ oid999 `next` 闭包 14 帧均为零有效 geometry |
| Hit / Collision | `R-HC-03` | ✅ current OID/type 统一与 gate A/B 覆盖 |
| Hit / Collision | `R-HC-04` | ✅ current-DAT pickup 去除 CLR cast；真实 step6 collect、错误循环不消费、post consumer 消费及 current type3 负例均已覆盖 |
| Hit / Collision | `R-HC-05` | ✅ fixed slot/reuse 关闭 |
| Hit / Collision | `R-HC-06` | ✅ 整数路径关闭 |
| Frame / lifecycle | `R-FL-01` | ✅ 四 weapon 矩阵关闭 |
| Frame / lifecycle | `R-FL-02` | ✅ current-DAT boomerang 关闭 |
| Frame / lifecycle | `R-FL-03` | ✅ raw empty slot `CatchTimer`、占槽清理与 reset 关闭 |
| Frame / lifecycle | `R-LC-01` | ✅ pooled snapshot/cache reset 关闭 |
| Frame / lifecycle | `R-LC-02` | ✅ StableId alias/reuse 关闭 |
| Frame / lifecycle | `R-FT-01` | ✅ 已关闭 FT-01 的 trace 验证债；不是重复风险 |

R-GP-01 freshness：authority source `00:11:23` < DLL `00:11:49` < trace `00:12:07`；Unity source `00:11:23` < Editor DLL `00:12:22` < trace `00:13:44`；compare `00:14:02` 为 `equal-diagnostic`、2 ticks、`firstDifference=null`。它关闭 R-GP-01，但不构成任意对局证书。

最终 PASS 前的失败和收口不可省略：`C-05`、`BATTLE-AUDIT3-12`、state8000/type6 fixture、`C-12`、`GT-04/GT-07/PH-02` 与 Weapon C-26/C-27 均已按权威收口；此前 `18:16:36.721 PASS` 与 `21:57:40.670 PASS` 只保留为过期历史证据，当前统一以 `01:07:52.834 PASS` 和 combined Architect `P0/P1/P2=0` 为准。该结论仍不能替代逐 tick 或目标 Play Mode。

### 原始总账后新增确认差异（BATTLE-AUDIT6）

- **BATTLE-AUDIT6-01 / GameTick-Input pass order，已关闭**：Unity 已拆分 human poll 和 unified character input，正式顺序为 poll → cooldown/M-1 → `NeedClearInput`/tick gate → character input。矩阵覆盖 tick1、清输入、oid51 frame85 gate 外延迟 split、AI 顺序；另补 transformed-human P1：CLR character 即使 current DAT 转为 non-character，仍按 roster human 轮询输入，但不会错误执行 character action。
- **BATTLE-AUDIT6-02 / DJA locals persistence，已关闭**：四类 early-return 保留进入的 private/runtime combo locals，只有正常尾路径 commit；缺/有效 target、oid6 guard、`Unk328` 与正常尾路径均有正负覆盖。
- **旧检查已按权威重写**：synthetic fixture 已补 frame85，same-tick 假阳性改为 gate 外延迟 split；不是删除断言求绿。
- **LC-02 最终契约**：plain free 清 pending、注销 slot/bucket 并归池，不触发虚拟 destroy/event/effect/额外 sound；显式 renderer/manual destroy 路径仍保留各自销毁事件。Frame / Lifecycle 20 项已由 combined fresh full PASS 覆盖。

### CP-NV1/2/3 与 STEP10 C# 重审（重开后已关闭）

这批是对原历史 backlog 的重审，不修改原始 74 项分母。旧历史 PASS 不作为当前证据；生产与检查已按 C# 调用链重写，并进入 `21:57:40.670` combined fresh full PASS。

- **CP-NV1 / immediate frame**：real/shared 双壳均清 Runtime FWC，保留 Trans wait/Prev2；最终负向矩阵覆盖 aaction/taction/jaction、负 action、方向、attacking 和双方 carrier。
- **CP-NV2 / throw snapshot/raw**：throw 已使用 source `atkFrame`；transform fixture 为 attacker frame112、victim `(76,-36)`；none/up/down/both 的 victim `Vz` 为 `0/-3/+3/0`，raw carrier 同步覆盖。
- **CP-NV3 / held sync**：`-131/0/131` 分别验证 frame131+翻面+FWC0、保留进入 frame/facing/FWC、frame131+不翻面+FWC0；位置 center/cpoint 均读最终 resolved current frame。
- **STEP10 P0**：state9 首次 sync、mismatch/escape immediate + early return、escape 同 tick `Vx/Vy`、FWC 清零与 entity stats-only 契约均已落地。
- **最终检查**：旧反权威断言已按唯一 C# 权威重写并扩展 real/shared-DAT、负 action、early-return、速度和 world stats 不变覆盖；combined Architect `P0/P1/P2=0`。

### DAT 诊断统计与 trace 证据

- `Temp/NTSDParity/data-audit-v3-required.json`：137 个权威 OID = 34 equal / 66 different / 37 missing Unity / 0 parse error；差异类别计数为 frame 126、geometry 31、sound cue 155。该统计只描述两套 raw DAT 在各自读取/适配前后的结构差异，保留作诊断信息；它不是战斗逻辑阻塞、backlog 或资源缺失清单，不要求把 DAT 文件改成相同。
- raw production battle-logic manifest 当前为 C# `41c088d2...0375`、Unity `6b34e118...332a`。旧 `compare-v3-full-final.json` 因工具按 raw manifest 做 header gate，返回 `different`、`certificateEligible=false`、`ticksCompared=0`。这只说明该次工具运行没有签发 certificate，不代表生产战斗逻辑失败；未来 certificate 应基于双方正式读取/Unity 适配后的语义 runtime 输入与 trace，raw DAT/manifest 相等不得作为前置条件。
- `Tools/NTSDParity` 构建 0 warning / 0 error。最新 `trace-compare-self-test-iter7.json` 为 **20/20 PASS**，覆盖连续 tick、空 trace、body/hash/slot commitment 防篡改、dense human input、diagnostic 显式 opt-in、diagnostic 永不签发 certificate 与 strict/fixed-world camera profile。
- iter7 authority/Unity full-detail diagnostic trace 均已生成。`compare-v3-diagnostic-full-iter7.json` 返回 `status=equal-diagnostic`、`ticksCompared=6`、`firstDifference=null`、`comparisonProfile=fixed-world-camera`、`diagnosticComparison=true`、`certificateEligible=false`、`certificateClass=none`。
- iter7 的 Unity 端使用 `authority-dat-diagnostic` 夹具；该结果只证明这 6 tick 样例的已观察域一致。原 15 项风险由各自证据逐项关闭，不是由 iter7 一次性关闭；iter7 与 R-GP-01 的 2 tick trace 都不能扩大为完整战斗逐帧等价或 production certificate。

### 状态纪律与下一步

必须按“逻辑已写 → isolated/目标编译 → Unity fresh 编译 → full self-check → 逐风险 trace → 必要 Play Mode”逐级报告，任何一级都不能替代后一级。production certificate 可以继续作为聚合对拍证据建设，但当前数量仍为 0，不能冒充已完成，也不能以 raw DAT/manifest 相等作为签发前置。

1. 原 15 项风险账已 15/15 关闭，不再把“关闭 15 风险”列为下一步。
2. 若继续建设 production certificate，扩展双方正式读取/适配后的语义 runtime、真实输入与长时间 full/full trace；保持 source < DLL < trace/result freshness。
3. 不处理 raw DAT 文件或 manifest 差异；T8 默认 `stage.dat` 部署继续暂缓，不读取、生成或私自部署默认资产。

**Audit5/Audit6 历史交接结论（已被顶部 BATTLE-AUDIT7 当前状态取代）：原始确认项曾达到 74/74 逻辑实现 + focused/full self-check，原 15 项 trace 风险曾达到 15/15 已关闭；Audit6 与重开的 CP-NV1/2/3、STEP10 也保持关闭，原 3 个受控 P2 亦已补强关闭。该批 full self-check 为 source `01:06:21.499` < DLL `01:07:21.125` < result `01:07:52.834` PASS，Architect 当时为 `P0/P1/P2=0`。R-GP-01 fresh compare 为 `equal-diagnostic`、2 ticks、无差异，但不能扩大为任意对局、全输入 production certificate，更不能覆盖 BATTLE-AUDIT7 新发现。34 equal / 66 different / 37 missing Unity 只保留为 raw DAT 适配诊断，不是阻塞或 backlog；raw DAT/manifest 相等不是 certificate 前置。T8 默认 `stage.dat` 部署继续独立暂缓。**

完整报告：

- `.omc/research/game-tick-physics-audit-20260717.md`
- `.omc/research/hit-collision-audit-20260717.md`
- `.omc/research/frame-lifecycle-audit-20260717.md`
- `Temp/NTSDParity/authority-v3-full-iter7.jsonl`
- `Temp/NTSDParity/unity-trace-v3-diagnostic-full-iter7.jsonl`
- `Temp/NTSDParity/compare-v3-diagnostic-full-iter7.json`
- `Temp/NTSDParity/trace-compare-self-test-iter7.json`

## 1. 铁律（不可违反）

1. **权威锁死**：任何正式战斗改动必须能在 `ntsd_release_C#` 的真实调用链中找到对应行为；无法确认时标“待确认”，不得以旧工程或历史资料补写规则。
2. **表现效果一致优先**：能逐行对齐就对齐；Unity 框架限制无法同构时，**运行时最终表现必须逐帧等价**（位置/帧号/速度/伤害/时序）。
3. **只新增不误删**：本文的 ❌ 项都是「C# 有 Unity 无」，是**新增**任务，**不是删除**。
4. **架构等价严禁删**：见 §5 清单——Unity 用 resolver/组合/hook 换方式实现的，不算冗余。
5. **排除范围不碰**：bg.dat 可活动范围、相机——不对齐，不改。

## 第三次实战/静态审计交接（2026-07-16，历史记录；已被 Audit4 取代）

旧版“当前没有已确认但未实现的正式战斗逻辑差异”结论已失效。完整编号和双方证据见 `csharp-vs-unity-battle-alignment.md` 的 BATTLE-AUDIT3-01..17。17 项生产修复现已全部落地；10 已完成通用 hit_Fa 重构并补齐 3/4/10/14 直接覆盖，12 已补齐 generic holder、damaged 后继续 dvx/kind3、IronBall `FrameDelay=1` 及 world-level 真实武器覆盖。最新 fresh `dotnet build Assembly-CSharp.csproj --no-restore /m:1` 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `2026-07-16 18:24:04` < Unity `Assembly-CSharp.dll` `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。该结果包含 M-1/T4 最新矩阵；此前生产 diff 的 Architect 复核结论保留。当前仍不代表 17 项 Play Mode 全完成：真实 `NTSD_Battle` 的 Naruto 防前跳螺旋丸、奔跑防跳命中及防下跳六分身仍待本轮回归，也不能宣称全部战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署仍按用户要求暂缓。

### 分组进度

- **既有候选收集 7 项（03/04/13/14/15/16/17）**：生产修复与对应 `BattleRuntimeSelfCheck` 矩阵已通过；真实 Play Mode 尚未运行，不能标记场景验收完成。
- **本批已落地 8 项（01/02/05/06/07/08/09/11）**：生产修复与针对性 self-check 已通过。01 已补 `RelationTeam`，仍等待真实 bootstrap/螺旋丸 Play Mode；02/05 等 held 表现和攻击链也须在同一场景回归。09 的权威契约是 invalid positive link 只清 holder 的 `LinkState/TargetIdx/HeldWeaponSlot`，不清 inactive/mismatch target 的反向字段。
- **本批新增落地 2 项（10/12）**：10 已将 `hit_Fa1..14` 唯一实现下沉 `LF2Entity`，由 Special/Other/current-DAT shell 共用，并删除旧 TU/重复副本；新增 self-check 覆盖 3/4/10/14，其中 3/14 对 Other、current-DAT Character、Special 三种壳连续两 tick 验证副作用仅执行一次，4 覆盖 catch frame/速度/`CatchTimer`，10 覆盖原路径与落地摩擦防重复。12 的 generic holder、damaged 后继续 dvx/kind3 与 IronBall `FrameDelay=1` 已落地；`CheckWorldLevelRealWeaponStep12Contracts` 经 `SimulationWorld.HeldObjectProcessAll`、generic `LF2Entity` holder 和真实 `LF2Weapon` 覆盖 damaged→dvx、damaged→kind3、IronBall `FrameDelay=1`。新增矩阵 fresh PASS；两项仍未完成真实场景 Play Mode 验收。
- **T8**：默认 `stage.dat` 资产部署继续暂缓，不进入本轮推进。

### 执行顺序

1. **编译与自检已清**：fresh `/m:1` build 为 0 errors / 42 existing warnings；source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full self-check fresh PASS。编译和针对性自检仍不能替代真实场景行为。
2. **关系与 held 前置**：01、09、12 的生产修复和现有 self-check 已通过；09 只清 holder 三字段；12 的 world-level generic holder/真实 weapon 覆盖已补齐。01 仍待 bootstrap Play Mode。
3. **held/坐标相位**：02、05、06、08、11 的生产修复和针对性 self-check 已通过，等待真实 Play Mode。
4. **候选收集**：03、04、07、13、14、15、16、17 的生产修复和对应矩阵已通过，等待真实 Play Mode。
5. **frame logic 分派**：10 的生产重构和 fresh self-check 已通过；`hit_Fa1..14` 唯一实现已下沉 `LF2Entity`，直接覆盖已扩展到 3/4/10/14 及三壳两 tick 单次副作用矩阵。
6. **运行验收**：当前版本防下跳六分身已通过；继续回归 Naruto 防前跳螺旋丸的层级、位置、跟手和攻击路径、奔跑防跳完整后续招，以及投掷武器单次命中/Arest。
7. **Audit3 历史回写状态**：当时可写“生产修复已落地、针对性 self-check 已通过”；该阶段后来被 Audit4 的实现与 Play 验收取代，最终状态以本文后部 Audit4-01..16 为准。

### 验收门槛

- 编译错误必须为 0；“隔离 Roslyn 本轮 0 诊断”不能代替 Unity 编译成功。
- `BattleRuntimeSelfCheck` 已 fresh PASS；该结果只证明现有断言通过，不自动补齐未覆盖分支或真实场景。
- 17 个差异簇的现有针对性矩阵已通过；10 的 3/4/10/14 与三壳两 tick 矩阵、12 的 world-level generic holder/真实 weapon Step12 矩阵均已 fresh PASS。
- `NTSD_Battle` 当前版本的防下跳六分身已通过；仍需回归 Naruto 防前跳螺旋丸、奔跑防跳完整后续招和投掷武器单次命中/Arest。
- T8 只记录逻辑/生产接线状态；默认 `stage.dat` 资产部署继续暂缓。

### Audit3 历史对外措辞（已失效）

**“已发现并记录 17 个战斗逻辑差异簇，生产修复现已全部落地；fresh `/m:1` build 为 0 errors / 42 existing warnings，source `18:24:04` < Unity DLL `18:31:52` < result `18:33:00`，full `BattleRuntimeSelfCheck` PASS；M-1/T4 与 Audit3-10/12 的新增矩阵均已覆盖。但本轮真实 `NTSD_Battle` Naruto 螺旋丸、奔跑防跳和六分身仍待 Play Mode 验收，因此不能把 17 项标成 Play Mode 全完成，也不能宣称 C# 与 Unity 战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。”**

## 第四次战斗命中/技能链审计交接（BATTLE-AUDIT4，2026-07-17 最终状态）

完整双方坐标、影响和逐项状态见 `csharp-vs-unity-battle-alignment.md` 的 BATTLE-AUDIT4-01..16。本批 16 项**生产修复已落地**，Audit4 针对性断言已进入最终 fresh full self-check 并通过，3 项目标 Play Mode 也已全部通过。该结论只关闭本批确认差异，不能关闭完整对局逐帧对拍和 RISK-4。

| 执行组 | 编号 | 内容 | 当前状态 |
|---|---|---|---|
| 核心命中链 | 01、02、03、04、05 | AttackExempt 清理、统一标准命中、weapon candidate 消费、额外 Arest、post-collect 重筛 | 生产修复已落地，针对性矩阵 fresh PASS；投掷武器 Play 09:45:21 PASS |
| 独立实现轨 | 07、08、09、15 | SpecialAttack type3 tail、状态转换 RNG、held raw frame/facing、late frame 后 held pose/presentation | 生产修复已落地，已有断言 fresh PASS；螺旋丸 Play 01:10:34 PASS |
| 命中尾收口 | 06、10、12、14、16 | Naruto kind3、受击方向、命中声音、effect6/23 spark、catching state-exit/full reset | 生产修复已落地，针对性矩阵 fresh PASS；奔跑防跳 Play 09:34:36 PASS |
| opoint/声音生命周期 | 11、13 | first-op/OID5/52 与 frame sound/pic999 生命周期 | 生产修复已落地，针对性矩阵 fresh PASS |

最终 fresh 证据链：Unity Editor PID `11540` fresh script compile 为 **0 C# error**；`BattleRuntimeSelfCheck.cs` source/test `2026-07-17 01:39:46` < `Assembly-CSharp.dll` `09:26:23` < result `09:26:55`，full self-check **PASS**。Architect 最终复核为 **PASS**。Architect 复核后补入的矩阵明确覆盖：SpecialAttack consume 删除 live `Team` gate；collect 后将 attacker `Team=0` 仍按冻结候选连续消费两个目标；显式 oid300 abort 仍停止后续候选；SpecialAttack `PendingSounds` 严格断言单条 Cue/WorldX/Tick，并覆盖下一 tick 与 reset 清理。

`BATTLE-AUDIT4-15` 是 Play 抓出的 held late frame pose/presentation 差异：`HeldObjectProcess` 早于 late `SimFrameTick`，holder 首 tick 切帧后 held 仍读旧挂点。现已在 late frame 变化后执行纯 `SyncHeldPose`，不重复 step12，并按 holder→held 刷新 renderer。focused freshness 链 source `01:05:07` < DLL `01:06:22` < result `01:07:01` **PASS**；Rasengan Play `01:10:34` **PASS**：frame240 / oid434 / link 成立，change runtime/holderVisual/heldVisual=`5/5/5/5`，move=`9/9/9/9`，sorting `526 -> 527`，攻击链 `20 -> 257 -> 258 -> 259`，oid434 `396 -> 397`。

`BATTLE-AUDIT4-16` 是 Play 抓出的 catching state-exit/full reset 差异：Unity 普通 state transition 提前清 catch link，导致 `276 -> 277` 后下一 tick 按 `PrevFrame2=276` cpoint 强制 frame0。现已取消普通 state transition 清 link，完整实体 Reset 仍清。最终 full self-check `09:26:55` **PASS**；Running Play `09:34:36` **PASS**，完整链为 `9 -> 102 -> 295(prev2)/297(pn) -> 298 -> 299 -> 275 -> 276 -> 277 -> 278 -> 279 -> 86 -> 87 -> 88`，victim 保持 frame130/catch；oid33 `current311/pn310` 为 wait0 的正确口径。

Naruto 防下跳六分身的当前版本定向 Play Mode 已通过：真实生产输入链 `L -> L+S -> L+S+K`，tick1 frame271，tick12 frame272 且 PP `500 -> 295`/生成 oid205，tick15 frame273/oid204 展开，tick29-32 出现 6 个 unique oid33/action307，tick38 共有 6 个 renderer 可见；峰值 `max204=11`、`max205=3`、`uniqueClones=6`、`action307=6`、`maxVisible=6`。

投掷武器 Play `09:45:21` **PASS**：使用生产 oid120 / hold / double-D / D+J；HP 只在 tick17 从 `500 -> 489` 下降一次；weapon state1002/frame41 后同 tick 切到 frame7/state1000，`AttackExempt=4`；跨 35 tick 冷却归零并落地，HP 无二次下降。至此三项目标 Play Mode 已全部完成。T8 默认 `stage.dat` 资产部署继续暂缓。

当前 Unity 自动生成的 dotnet `.csproj` 仍包含 35 个已删除历史源文件，最终 `dotnet build` 被 `CS2001` 阻塞。不得把此前的 dotnet 0 error 冒充为 Audit4-16 后的最终编译证据；有效证据是 Unity fresh script compile 0 C# error。

当前对外措辞更新为：**“Audit4-01..16 的生产修复已落地并经 Architect 最终复核 PASS；Unity fresh script compile 为 0 C# error，fresh full `BattleRuntimeSelfCheck` PASS；Naruto 防下跳六分身、螺旋丸、奔跑防跳后续招和投掷武器目标 Play 均通过。本批确认差异已关闭，但完整对局逐帧对拍和 RISK-4 仍在，因此不能宣称 C# 与 Unity 全部战斗逻辑完全对齐。T8 默认 `stage.dat` 资产部署继续暂缓。”**

非行为性清理债：`WeaponSpawner` 仍有历史非 C# 注释，F9 debug 说明也存在与当前 C# 唯一权威措辞冲突的历史文字。F7-F9/debug 按 `AGENTS.md` 排除正式战斗 backlog，不计为生产逻辑差异。

## 2. 任务清单（按建议顺序，坐标精确到行）

### T0 — 修真 bug：exemptVal 用错变量（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:268` → `int itrArest = itr.Arest < 4 && itr.Vrest == 0 ? 4 : itr.Arest;`
- **Unity 落点**：`LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入已改用 arest/vrest 权威公式。
- **验收**：`CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 Unity 运行时自检。

### T1 — ApplyAlternateDamage（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:629-827` `ShouldUseAlternateHurt` / `ApplyAlternateDamage` 完整方法。
- **Unity 落点**：共享 `LF2AlternateDamageResolver`，由真实角色与 shared-DAT 两入口复用；runtime/stat/运动尾契约已补齐。
- **验收**：alternate trigger/core/motion/character/shared-DAT/heavy/object-pass 针对性检查均通过。

### T2 — 武器命中 spark（M-9，已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1150` `RecordKind0Hit`（timer：`Fall>60 ? sparkPhase*20 : sparkPhase*20+10`），312/320/**506** 三处调用，**武器命中路径（506）也调**。
- **Unity 落点**：`LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径已接入。
- **验收**：`CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 Unity 运行时自检。

### T3 — frame 110/114 → CdDefendLock=3（M-14，已完成，Unity 运行时已验证）
- **C# 权威**：`FrameTick.cs:208-209` → `if (frame==110 || frame==114) CdDefendLock=3;`
- **Unity 落点**：`LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载。
- **验收**：`CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 Unity 运行时自检。

### T4 — oid 7/8 → 51 合体拆分（Audit6 重审已关闭）
- **C# 权威**：`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs:1093-1263` `RunOid5152RuntimeMaintenance/TryMergeOid7Or8Into51/SplitOid51BackToPair`。旧来源结论只保留为历史记录，不能覆盖 C#。
- **历史错误顺序（已被 Audit6 推翻）**：旧实现曾按 `TickCooldowns -> human input -> AI input/combo -> M-1` 提前消费 DJA，并据此要求同 tick 拆分。唯一权威 C# 的正式输入消费在 M-1 与 `NeedClearInput` gate 之后，详见 `BATTLE-AUDIT6-01`。
- **Unity 落点**：`Oid5152RuntimeMaintenanceAll`、merge/split helper 与 runtime 身份/表现维护链已落地；split partner 在 `Reset()` 后恢复正式默认值 `FrameDelay=0`、三轴 knockback=`0.1`、`HolderCopy=99`、prev carriers 清零、`Effect`/`DeadBlink` reset，同时保留 Entity 外部 `ItrRest`。
- **当前验收**：旧 same-tick 期待已按权威改为 frame85 gate 外延迟 split，synthetic fixture 已补 frame85；poll/M-1/apply、tick1/clear gate、human/AI 与 transformed-human 均进入 combined fresh PASS。
- **freshness**：旧 `18:33:00` PASS 已过期；当前统一使用 source `21:55:28` < DLL `21:56:56` < result `21:57:40` PASS。

### T5 — 复活 pass（已完成，Unity 运行时已验证）
- **C# 权威**：`GameTick.cs:839-934` `RunRespawnPass`（tick step10）
  - 门控：state==14 + Hp<=0 + (KillCount>=0 OR Unk364==5 OR slot>=20) + HitStop∈(0,5)
  - 分支A（RespawnCount<=0）：Hp2Orig<2→FreeEntity；否则 Hp2Overlay-1、队友 X/Z 平均+随机、Pp=500、HpMax=Hp3、Hp=HpMax、HitStop=20、Frame=212、YInt=-300
  - 分支B（RespawnCount>0）：Pp=0、HpMax=RespawnCount、Hp3=HpMax、Hp=HpMax、RespawnCount=0、Unk364=1、oid∈[0x1E,0x24]→Unk318=0x8C、Frame=0xDB、FrameDelay=0xA、生成 oid998 复活特效
- **Unity 落点**：`PostFrameAdvanceDeathCleanupAll` 已实现两分支、free gate、队友平均落点、血量/PP/帧字段与 oid998 特效。
- **验收**：无 stored-count、free gate、stored-count + effect 三项检查均通过。

### T6 — kind 15/16 副作用补齐（已完成，Unity 运行时已验证）
- **C# 权威**：`HitResolve.cs:1628` `ApplyKind15Or16` + `1737` `ApplyKind15Movement`
  - kind16 完整：Hp-、KillStat++、ComboCountAtk、`RecordSound("SFX_065")`、Frame=200、vrest 写入、LinkState==2 断开
  - kind15 位移：`KnockbackVx = Vx + (±1)`、真实 Vx=KnockbackVx、`KnockbackVz = Vz + (±0.5)`、`YInt=-2`；按对象类型分 vyStep（角色3.0 / 飞行道具3.0 / IronBall2.3）
- **Unity 落点**：真实角色与 shared-DAT resolver 已补 kind15 authority 位移、kind16 统计/vrest/link/SFX 副作用。
- **验收**：`CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过。

### T7 — combo 连招 wrapper（大）
- **C# 权威**：`InputRuntime.cs:740` `RunComboWrappers`（9 组：Dra/Dla/Dld/Dlu/Drd/Dru/Djd/Dja/Daa/Dab + DjaGuard，含 oid6 Sasuke DjaGuard 特判），入口 `InputRuntime.cs:647`。
- **Unity 现状**：已由 `NTSDInputStateModule` 承载 9 组 wrapper 与 oid6 DjaGuard，真实消费路径为 `LF2Character.RunPostCooldownInputPhase -> UpdateLocalInputStateFromControllerBuffer -> ComboUpdate -> NTSDInputStateModule.ApplyFrameInput`。
- **本轮新增验证**：`BattleRuntimeSelfCheck` 已补 `CheckComboWrappersCharacterFrameJumps` 与 `CheckOid6DjaGuardComboHold`，覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release。
- **验收现状**：已通过当前打开 Unity 的 request 自检机制，`Temp/NTSD_BattleRuntimeSelfCheck.result` fresh 返回 `PASS`。**T7 已完成。**
- **Naruto DDJ 完整链补充验收（2026-07-16）**：同 tick held chord 的内部输入 `att + down + def` 先命中 frame271；272 生成 oid205/action98，辅助链经过 99/325/341；273 生成 oid204/action130，展开六分支并各自到 147 生成 `6 x oid33/action307`。clone 在 307 后落地到 frame219 是 authority 行为。
- **本次确认的 5 个根因**：`LF2ReferencePool.Release` 无条件接收外部 synthetic，污染逻辑池类型；factory 角色 opoint 在 `ModuleBind` 注册前过早用 `slot < 0` 拒绝；pending-unregister 对象同 tick 归池复用时，旧 registry bucket 的 `Contains` 拒绝后续递归分支，六 clone 只出 3；pooled `LF2Character.Init` 未重新分配 `StableId`；`SpawnFromOpoint` 缺 `RelationTeam`、`Unk364` 与 holder-copy 继承。
- **修复契约**：`Release` 只归池 active 实例；`Register` 先 finalize pending old lifecycle；slot guard 移至 `ModuleBind + Initialize` 后；character `Init` 重新 `AllocateStableId`；`PostInitLiving` 继承 `Team`、`RelationTeam` 与 holder-copy。专项回归验证 PP 500→295、dynamic slot、6 unique StableId、6 x action307 和 6 visible renderer。
- **真实 Play Mode 生产输入链验收**：在 `NTSD_Battle` Play 中等待 slot0 `CharacterInputModule`/`ActionMap` 就绪，通过 UnityMCP 临时 `InputSystem.Keyboard` 按物理绑定注入 `L (Defend) -> S (Down) -> K (Jump)`。事件完整经过 `InputActionMap -> CharacterInputModule -> SimInputBuffer`，未直接调用技能、帧或 opoint。日志为 `INPUT focused=True buffered=1, attackAction=0, jumpAction=1, defendAction=1, moveY=-1`，crossed internal mapping 符合预期；结果 `frame271=True, max204=11, max205=3, maxClones=6, maxSpriteReady=6, maxVisible=6`。
- **Play Mode 时间线/证据/限制**：clone 数在 `t=0.446/0.473/0.509/0.541` 依次为 `3/4/5/6`，测试窗口无异常，截图 `Temp/naruto-ddj-unitymcp-peak.png`。Win32 `keybd_event` 不被 Unity RawInput 接收，所以这不是物理硬件键盘证明；成功证据是 UnityMCP Input System Keyboard 事件经过完整生产输入链。

### T8 — stage 波次刷敌（M-13，大）
- **C# 权威**：`GameTick.cs:2317` `ApplyCurrentWavePhaseAdvance` + `2350` `ApplyCurrentWaveImmediateStageSpawns` + `2226` `RefillCurrentWavePositiveStageSpawns`（配套 `StageProgression` + `StageSpawnRuntime*` 一整套，见 `SimulationWorld.cs:68-80`），tick step23。
- **Unity 落点**：`BattleRuntimeState` 已补齐 `StageProgression` / `StageSpawnRuntime*`；`SimulationWorld.StageWave.partial.cs` 已实现立即刷敌、正 ratio 并发槽/总量补充、清场推进和 phase bound 写回；`NTSDBattleTickSystem` 在 `PreFrameBounds` 后、`RenderDispatch` 前执行该 pass，匹配权威 step23 顺序。spawn 契约已补 `Unk344=2`、DAT type 0/5 的 character-init `RelationTeam=2/HitStun=20`、其他类型 `RelationTeam=0/HitStun=0`、dynamic slot 50+ 和 action 0 保留。
- **生产接线**：`AppManager.InitializeBattle -> SimulationTickDriver.ApplyMatchConfig -> BattleStageCampaignLoader -> ConfigureStageCampaigns(-1) -> StartInitialStageWave()` 已接通；默认读取 `Application.streamingAssetsPath/NTSD/data/stage.dat`，也可由 `MatchConfig.stageCampaignFilePath` 显式覆盖。仓库当前未纳入二进制 `stage.dat`，缺失时会明确 warning 并保持 `StageProgressionValid=false`。
- **本轮新增验证**：`CheckStageWaveBootstrapAndSpawnContract` 覆盖 stage 文本解析、pre-wave -1→0、bound、type0/type5/非角色身份契约和 action 0；`CheckStageWaveImmediateSpawnAndAdvance` 覆盖真实 direct spawn、dynamic slot 50+、20-49 非 stage 槽隔离、清场推进；`CheckStageWavePositiveSpawnRefill` 覆盖并发槽补位与总量上限。
- **验收现状**：fresh Unity batch self-check 返回 `PASS`。**T8 逻辑与生产接线代码已完成并通过运行时验证；默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。**

### T9 — AI 输入生成器（已完成，Unity 运行时已验证）
- **C# 权威**：`InputRuntime.cs:16` `PrepareAiInputBasic` + 14 辅助函数：
  `AiBetweenX / AiPostCacheCoordinateAllowsSpecial / AiPreUpdateTarget3000SideEffect / AiUpdateOid33_19_16PredictedDuaDecision / AiUpdateOid52_1_2_21PreLabel591Decision / AiUpdateLabel591Oid51_2_18_7Decision / AiUpdateFirstDecision / AiUpdateTeammateGuardDecision / AiUpdateOid1ComboDecision / AiUpdateCloseOid1Decision / AiUpdateOid4ComboDecision / AiUpdateOid5ComboDecision / AiProcessSubOidGroup / AiSpecialOidForSubGate / AiProcessHelper`（行号见差异清单 §6.2）
- **Unity 落点**：`SimulationWorld.AiInput.partial.cs` 已完整承载主入口及直接/间接 helper 闭包，包含 runtime-slot target/cache、coordinate、team/history/held gate、C8/D3/D4/7A/7B 扫描、oid 决策组、move-mode/no-target 和三个 `AiProcessSub*` 尾部分支。
- **历史输入接线（已由 Audit6 修正）**：Unity 曾让 human 与 AI input/combo 在 oid51/52 maintenance 前执行；`BATTLE-AUDIT6-01` 已按唯一权威 C# 拆分 poll 与 apply，并经 tick1/clear gate、human/AI、oid51 延迟 split 和 transformed-human 矩阵 fresh 验证。
- **验收**：fresh dotnet build 为 0 errors / 42 existing warnings；fresh Unity full self-check 返回 `PASS`。自检覆盖 target/cache、coordinate、同 seed 确定性、human 隔离，并由 M-1 full-tick 矩阵覆盖 AI DJA 在 maintenance 前同 tick 拆分。**T9 已完成。**

## 3. 已确认对齐（不要重复处理）

tick 主循环主干（含 `InputPhase`/`FrameMod12`/`FrameToggle` 统一推进）、全局 `ValidatePositiveLinks`、kind 0/4/9 主流程、kind 6/8/10/11/14、oid300、kind5 委托、M-5 死亡弹地、M-7 kind4+WeaponCount 翻转、HP/PP 自然恢复、heal/catch timer、state14 复活与 respawn pass、frame mp turn-around、frame202 HitStun=20、opoint 生成、cpoint 正值主流程、state 400/401/500/501、N30 触发、状态转换特效。

## 4. 确认可不移植

- **M-6 F8 强制掉武器**（`RunF8WeaponDrop`）：调试功能，Unity 不需实现（非冗余）。
- `RunMode2RandomWeaponDrop`、`InitStats`/mode2 postframe 分支：属于 C# 权威工程的 F7-F9/debug 控制路径，不作为正式战斗对齐项。

## 5. 架构等价（🔷 严禁当冗余删除）

| Unity 机制 | 对应 C# | 说明 |
|-----------|---------|------|
| `LF2Character*Resolver` / `LF2Weapon*Resolver` | `NtsdCharacter`/`HitResolve`/`CPointRuntime`/`WeaponRuntime` 各段 | 组合模式拆分 |
| `LF2Entity` shared-DAT 输入桥（~900 行） | `InputRuntime.ApplyCharacterInput` 角色分发 | 服务 transform 后 wrong-shell 角色 |
| `NTSDEntityRuntime` 字段分桶 | `Entity` 大字段对象 | 运行时化，字段一一对应 |
| `FrameTransistor` hook | `FrameTick.Tick` 内联步骤 | 拆 hook 供覆写 |
| `SimulationWorld` 动态槽 | `Objects[400]` 固定槽 | 遍历顺序须保持 slot 升序 |
| `DirectWriteFramePreserveWaitCounter` | `SetFrameImmediate`（不清 attacking） | BMD-023，区别于会清 attacking 的 ImmediateFrame |

## 6. 排除范围（不对齐、不改）

菜单/选人/加载、HUD/结算、bg.dat 的 Z 可活动范围、相机、背景/纯渲染、音频播放系统、网络、回放/回滚基础设施、`src/Host/*`。注意：PreFrame 中改变实体存亡或 X 坐标的逻辑边界仍在战斗范围内。

## 7. 工作流（每个任务照做）

1. **溯源**：打开 C# 权威行号，读懂完整逻辑（含分支/常量/字段读取顺序）。
2. **索要原型**：向 Codex 要 unified diff patch（`sandbox=read-only`，严禁真实改码），作为逻辑参考。
3. **重写**：以原型为参考，写成符合 Unity 架构的生产级代码（用现有 resolver/hook/runtime 字段）。
4. **改码**：用 executor-high（多文件）或 executor（单文件）落地。
5. **Review**：改完立即用 Codex review 或 `code-reviewer-low`。
6. **验收**：按每项的「验收」标准，优先跑 `BattleRuntimeSelfCheck`；无法运行时说明原因，不谎报。
7. **更新清单**：完成一项，去 `csharp-vs-unity-battle-alignment.md` §10 勾选对应行。

## 8. 关键文件速查

| 用途 | 路径 |
|------|------|
| 全量差异清单 | `Assets/NTSD/Docs/csharp-vs-unity-battle-alignment.md` |
| C# tick 主干 | `ntsd_release_C#/src/BattleCore/Simulation/GameTick.cs` |
| C# 命中结算 | `ntsd_release_C#/src/BattleCore/Interaction/HitResolve.cs` |
| C# 帧推进 | `ntsd_release_C#/src/BattleCore/Frame/FrameTick.cs` |
| C# 输入+AI | `ntsd_release_C#/src/BattleCore/Input/InputRuntime.cs` |
| Unity 角色命中 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2CharacterHitResolver.cs` |
| Unity 武器 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Weapon.cs` |
| Unity 帧推进钩子 | `Assets/NTSD/Scripts/Animation/LF2Objects/LF2Entity.cs` / `LF2Character.cs` |
| Unity pass 调度 | `Assets/NTSD/Scripts/Simulation/SimulationWorld.Passes.partial.cs` |
| Unity 候选收集 | `Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs` |

## 9. 优先级建议

T0-T9、Audit2/Audit3/Audit4、P1 BOUNDS-X 以及 OPOINT-VIS、STEP10、TRANSFORM-SHELL、FRAME-ADV/FRAME-TICK 的既有 self-check 继续作为回归基线。**当前 Audit5 原始确认总账为 74/74 逻辑实现 + focused/full self-check，Audit6 与重开的 CP-NV/STEP10 也已关闭；freshness 为 source `21:55:28` < DLL `21:56:56` < result `21:57:40` PASS，combined Architect `P0/P1/P2=0`**。这不替代本批目标 Play Mode、完整逐 tick 或 production certificate。

| 优先级 | 当前推进 |
|---|---|
| P0 | ✅ CP-NV1/2/3 与 STEP10 已按唯一权威 C# 重审、修复并重新关闭；immediate FWC、source throw snapshot/Vz、held resolved frame、early-return/即时速度和 entity stats-only 均进入 `21:57:40` combined fresh PASS |
| P1 | ✅ INPUT-1~9 与 INTERACT-1~5 全部修复并通过新增运行时矩阵；既有 OPOINT-VIS、Step10 等 runtime matrix 继续作为回归基线 |
| P2 | ✅ RISK-1/2/3/5 与 NARUTO-DDJ/OPOINT-LIFECYCLE 已修复并运行时验证；后者覆盖 pending 注销、同 tick 归池复用、递归 opoint、StableId 和关系字段继承 |
| P3 | ⚠️ Audit4-01..16 与 3 项目标 Play 已清；继续保留 RISK-4 与完整对局逐帧对拍缺口，不扩大为全战斗完成声明 |

T8 默认 `stage.dat` 部署由用户明确暂缓，不进入当前推进。

16 个 Audit4 差异的发现证据与逐项收口状态不在本行动版重复维护，统一见完整差异清单的 Audit4 章节。INPUT-1~9 由 `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖；INTERACT-1~5 由 `CheckInteractionRuntimeSlotContracts` 覆盖；NARUTO-DDJ/OPOINT-LIFECYCLE 由真实 frame271→oid205/204→6 x oid33/action307 完整链覆盖。

P0 的旧 CP-NV 检查曾含覆盖不足或反权威期待，历史 PASS 已废止。当前 `CheckCpointNegativeActionMatrix`、`CheckCpointHeldSyncVactionMatrix`、`CheckCpointThrowRawAndTransformMatrix` 已按 C# 重写并覆盖 real/shared 双壳、负 action、FWC、source snapshot、Vz 和 `-131/0/131`；STEP10 的 mismatch/escape、即时速度和 world stats 不变也已纳入 combined fresh PASS。

本批已验收项：

- OPOINT-VIS：`CheckQueuedObjectPointPassBoundaries` 与 late-mutation 矩阵已验证 pre-advance、natural drop、逐实体 late 发布边界、real factory queue、父回收与高/low slot 可见性；过程修复 pending-destroy active-filter。
- STEP10：state9 首次 sync、mismatch/escape early return、即时速度、real/shared-DAT cpoint 与 entity stats-only/world stats 不变矩阵已通过。
- TRANSFORM-SHELL / FRAME-ADV / FRAME-TICK / LC-02：已验证 character/weapon `PS.BindRuntime`、逐 slot Transit/TU、SpecialAttack 单次 physics/frame_tick/type3 drain、`PpDisplay`、state14、negative next、state4000/8000 WFC/hit-stop 顺序、type1/2/4/6/oid999 current-DAT landing，以及 cross-SimOrder pending plain free 只注销一次且不触发虚拟 destroy/event/effect/额外 sound。
- INPUT-1~9：real character 与 shared-DAT 输入路径均已修复；`CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵覆盖 state switch、`YInt` 门、velocity tail、单一 defend-lock 真值、Super Punch、raw frame write、running 和 frame215。
- INTERACT-1~5：dynamic slot、runtime-slot vrest、state3003 双向 vrest 与 non-character kind2 链接均已修复；动态槽 `50..399` 耗尽时直接拒绝生成，并由 `CheckInteractionRuntimeSlotContracts` 断言不遗留 registry 空桶、renderer pool 对象或 reference/logic pool 生命周期残留。
- NARUTO-DDJ / OPOINT-LIFECYCLE：reference pool active-only release、pending lifecycle finalize、factory 注册时机、pooled character StableId 重分配和 opoint Team/RelationTeam/HolderCopy 继承均已修复；真实链验证 6 个 clone 使用 dynamic slot、拥有 unique StableId、到达 action307 且 renderer 可见。
- RISK-1/2/3/5：locomotion 单次推进、raw move frame、held/`TrackerParent` runtime-slot 生命周期和 current-DAT step7/step9 路由均已修复并运行时验证；`CheckHeldReferenceSlotReuseContracts`、`CheckStateTransformInteractionPhaseRouting` 等新增矩阵通过。
- RISK-4 / COLLISION-SNAPSHOT：这是 Audit2 历史风险名，现已由 Audit5 `R-HC-05` 的 fixed-slot/reuse 覆盖关闭，不再是开放项。

## 10. 实施进度（2026-07-16）

> 下表是 Audit4 前的历史实施快照，不代表当前验收已经结束。Audit4 当前状态以本文前部交接段和完整差异清单为准；旧来源记录不得用于当前实现。

| 任务 | 状态 | 关键落点 | 针对性自检 |
|------|------|----------|------------|
| T0 | **已完成 / Unity 运行时已验证** | `LF2Entity.ResolveArestCooldown`；`LF2CharacterHitResolver` 的 AttackExempt 写入改用 arest/vrest 公式 | `CheckArestCooldownRule` 已覆盖 arest/vrest 边界组合并通过 |
| T2（M-9） | **已完成 / Unity 运行时已验证** | `LF2Entity.RecordKind0Hit` 统一命中记录；`LF2Weapon.ApplyHitEffects` 的 kind 0 路径接入 | `CheckKind0HitRecords` 已覆盖 owner、timer、随机坐标范围和 10 槽上限并通过 |
| T3（M-14） | **已完成 / Unity 运行时已验证** | `LF2Entity.RunCommonFrameTick` 尾部写 `CdDefendLock=3`；runtime 字段、Reset 和 cooldown 衰减已承载 | `CheckFrameTickDefendLockTail` 已覆盖 110/114、早退、普通帧和 3→0 衰减并通过 |
| T1（M-8） | **已完成 / Unity 运行时已验证** | 共享 `LF2AlternateDamageResolver`；真实 `LF2Character.Hit` 与 `LF2CharacterDatHitResolver.TryResolveHit` 两入口；`NTSDEntityRuntime.Unk344`；稳定 3 槽 `KillStats`/`DamageStats` 与保 identity reset；`HPBound` 整数扣减且 `HPLost` 不变；heavy 顺序、character guard、clamp 后 vrest、SpecialAttack object-pass kind4/9 预处理、state1002 不写 `WeaponState` | `CheckAlternateHurtTriggerMatrix`、`CheckAlternateDamageCoreSideEffects`、`CheckAlternateDamageMotionTailMatrix`、`CheckAlternateDamageCharacterEntry`、`CheckAlternateDamageSharedDatEntry`、`CheckAlternateDamageHeavyWeaponEntries`、`CheckAlternateDamageInteractionVrest`、`CheckSpecialAttackDamagePreprocess` 均通过 |
| T4（M-1） | **历史实现/self-check 已通过；待 C# 重审** | 唯一权威为 C# `GameTick.cs:1093-1263`；merge/split 与 pass 顺序需据此重新核验 | 既有 7 项检查只保留为回归基线，不能代替 C# 权威重审 |
| T5（M-2） | **已完成 / Unity 运行时已验证** | `SimulationWorld.PostFrameAdvanceDeathCleanupAll` 已补齐 respawn 两分支、队友平均落点、PP/HP/HpMax/Frame212/Y=-300、oid998 特效生成；`LF2Entity` / `LF2LivingObject` / `LF2Character` 已补 no-renderer 销毁注销链；`LF2ReferencePool` 已补惰性初始化，允许 self-check 直接 new 的角色安全释放 | `CheckRespawnPassWithoutStoredCount`、`CheckRespawnPassFreeEntityGate`、`CheckRespawnPassWithStoredCountAndEffectSpawn` 均通过 |
| T6（M-15/M-16） | **已完成 / Unity 运行时已验证** | 真实 `LF2CharacterHitResolver` 与 shared-DAT `LF2CharacterDatHitResolver` 均已对齐 kind15 authority 位移与 kind16 完整结算；角色 victim 不再走旧的 MaxMP 缩放或 `PS.vx/vz` 增量路径 | `CheckKind15CharacterWhirlwind`、`CheckKind16CharacterSideEffects` 均通过 |
| T7（§6.1 / combo） | **已完成 / Unity 运行时已验证** | `NTSDInputStateModule` 已承载 9 组 combo wrapper 与 oid6 DjaGuard；角色真实输入路径经 `RunPostCooldownInputPhase` 消费并落到 `ApplyFrameInput` | `CheckComboWrappersCharacterFrameJumps`、`CheckOid6DjaGuardComboHold` 已覆盖 9 组 frame jump、左右向切换、cooldown 清空，以及 oid6 guard hold/release 并通过 |
| T8（M-13 / stage） | **逻辑与接线已完成 / Unity 运行时已验证；默认资产部署暂缓** | `BattleStageCampaignLoader`、`ApplyMatchConfig` 生产接线；stage progression/runtime；立即刷敌、positive refill、清场推进、phase bound、精确身份字段与 dynamic slot 50+ | 三项 stage self-check 均通过；默认 `stage.dat` 部署由用户明确暂缓 |
| T9（AI） | **已完成 / Unity 运行时已验证** | `SimulationWorld.AiInput.partial.cs` 完整 AI 闭包；输入 pass 分段；runtime 字段与 roster/opoint bootstrap | `CheckAiTargetCacheCoordinateAndDeterminism`、`CheckAiHumanInputIsolation` 通过，并回归 T0-T8 |
| 二次审计 INPUT-1~9 | **全部已修复 / Unity 运行时已验证** | real/shared-DAT input state、raw frame、velocity tail、running/frame215 等契约已按 authority 收口 | `CheckRecordedInputAlignmentContracts` 与 shared-DAT 输入矩阵通过 |
| 二次审计 INTERACT-1~5 | **全部已修复 / Unity 运行时已验证** | dynamic slot、满槽拒绝、runtime-slot vrest、state3003、non-character kind2 已收口；拒绝路径不遗留 registry 空桶、renderer pool 或 reference/logic pool 生命周期残留 | `CheckInteractionRuntimeSlotContracts` 通过 |
| Naruto DDJ / OPOINT-LIFECYCLE | **已修复 / 当前版本真实 Play Mode 已通过** | active-only reference release；register finalize pending lifecycle；slot guard 后移；pooled character 重分配 StableId；`PostInitLiving` 补 Team/RelationTeam/HolderCopy | 当前回归通过 PP 500→295、dynamic slot、6 unique StableId、6 x oid33/action307、6 visible renderer |
| 二次审计 RISK | **历史 RISK-1..5 均已关闭** | locomotion、raw move frame、held/Tracker slot、current-DAT interaction 与 fixed-slot reuse 已收口 | Audit5 原 15 项风险总账 15/15 关闭 |

Audit3 历史验证（2026-07-16）：fresh `/m:1` build 为 **0 errors / 42 existing warnings**；`BattleRuntimeSelfCheck.cs` source `18:24:04` < Unity DLL `18:31:52` < `Temp/NTSD_BattleRuntimeSelfCheck.result` `18:33:00`，full self-check 返回 **PASS**。M-1/T4 的 gate、oid8 镜像、identity/presentation、human+AI DJA full-tick、split formal reset 与 `ItrRest` 保留矩阵，以及 Audit3-10/12 的扩展矩阵均包含在该结果中。M-1 runtime self-check 已完成，但仍不能扩大为全部战斗逻辑完全对齐。T8 默认 `stage.dat` 部署继续由用户明确暂缓。

当前版本 `NTSD_Battle` Play Mode 已通过 Input System 的 `L -> L+S -> L+S+K` 完整生产输入链并观测 `frame271=True`、`max204/max205=11/3`、`uniqueClones/action307/maxVisible=6/6/6`。螺旋丸、奔跑防跳和投掷武器三项 Play 也已分别于 `01:10:34`、`09:34:36`、`09:45:21` 通过。上述证据完成本批定向场景验收；历史 RISK-4 已由 Audit5 `R-HC-05` 关闭，但这些定向证据仍不能替代任意对局、全输入 production certificate。T8 默认 `stage.dat` 部署仍暂缓。
## 2026-07-22 交接补充：C++ 跳跃动量与 Texture2DArray 边界

- 用户明确要求本项以 `J:\QQFile\NTSD2.4\ntsd_release` 的 C++ 表现纠正 C# 共有缺陷。C++ `src/entity/frame_advance.cpp` 的 frame 212 只在存在互斥方向输入时用 DAT `jump_distance/jump_distancez` 覆盖 `vx/vz`，否则保留起跳前水平速度；空中不走地面摩擦。
- 根因位于 Unity `SimulationWorld.SerialTickAll`：它与 C# `GameTick.cs` 一样，在 frame advance 前清 current keys，使同 tick 后段 211 -> 212 初始化看不到按住方向。已移除这两次清键；战斗入口 `NeedClearInput` 全量 reset 不变，human/AI 下一 tick 的输入滚动仍由各自输入阶段负责。
- `BattleRuntimeSelfCheck` 已把 GT-02 改为 current/previous keys through-frame-advance 保留契约，并新增真实 211 -> 212 定向/无方向 Vx/Vz 回归及 cooldown/history 不变断言。
- 新鲜证据：`git diff --check` 无错误；dotnet build 为 **0 errors / 42 existing warnings**；源码 `23:15:11` < Unity DLL `23:15:37` < result `23:16:33`，fresh full `BattleRuntimeSelfCheck` 返回 **PASS**。因此代码与自动运行时契约已关闭；真实键盘 Play Mode 移动起跳仍未执行。
- 同轮解除 P4 阻塞：pool overflow 的首个动态扩容武器 mount 实际为 `Invalid`，而 world handle 为 `62:1`。`BindOwnerRuntime` 已直接同步 renderer 本体的 EntityModel mount，同时保留 generation-aware handle；P4 回归通过后全套才继续执行到跳跃矩阵并最终 PASS。
- 渲染澄清：角色中央资源已经有 `Texture2DArray` 主路径；公共阴影仍为独立 `SourceTexture2D`，因此角色/阴影会拆 resource segment。不要再把当前系统描述成“未使用 Texture2DArray”。


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# Final P8 Architect Review

Review the current repository state for the active objective: complete all Codex-owned P8 work in `Assets/NTSD/Docs/central-battle-render-system-plan.md`, excluding P8-E Android/Adreno/Mali real-device validation and T8 default stage.dat deployment.

Inspect the current implementation, the lifecycle fix in the benchmark Editor window/tests, the P8 plan and handoff docs, the eight v5 benchmark reports under `Temp/`, and the current self-check result. Do not assume prior summaries are correct. Look for correctness, lifecycle, resource ownership, stale-runner, benchmark validity, documentation or verification gaps. Distinguish user-owned unrelated dirty files from P8 work. Report findings severity-rated P0/P1/P2/P3 with exact file/line references and state whether any P0/P1/P2 finding remains. Do not edit files.
