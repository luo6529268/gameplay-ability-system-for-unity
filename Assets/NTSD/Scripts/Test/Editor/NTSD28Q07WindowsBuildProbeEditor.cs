#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07WindowsBuildProbeEditor
    {
        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public bool menuFirst;
        }

        [Serializable]
        private sealed class Result
        {
            public string runId;
            public string status;
            public string message;
            public string buildResult;
            public string outputPath;
            public int totalErrors;
            public long totalSize;
            public string scenes;
        }

        private static bool running;
        private static string Root => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(Root, "Temp/NTSD28_Q07_WindowsBuild.request.json");

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode || !File.Exists(RequestPath))
                return;
            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
            running = true;
            Debug.Log("[NTSD28 Q07] Build probe queued: " + request.runId);
            Build(request);
        }

        private static void Build(Request request)
        {
            var result = new Result { runId = request.runId, status = "RUNNING" };
            string resultPath = null;
            ScriptingImplementation previousBackend = default;
            bool backendCaptured = false;
            byte[] previousBurstSettings = null;
            string burstSettingsPath = Path.Combine(Root, "ProjectSettings", "BurstAotSettings_StandaloneWindows.json");
            try
            {
                Debug.Log("[NTSD28 Q07] Build probe entered: " + request.runId);
                if (string.IsNullOrEmpty(request.runId) ||
                    !request.runId.All(value => char.IsLetterOrDigit(value) || value == '-'))
                    throw new InvalidOperationException("Q07 build runId must be alphanumeric or hyphenated.");
                string artifactDirectory = request.menuFirst
                    ? "artifacts/diagnostics/NTSD28-Q07-MENU-FIRST-WINDOWS-PLAYER-001"
                    : "artifacts/diagnostics/NTSD28-Q07-CONTENT-MIGRATION-READINESS-001";
                resultPath = Path.Combine(Root, artifactDirectory, request.runId + "-build.json");
                Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
                if (File.Exists(resultPath))
                    throw new IOException("Q07 build report already exists: " + resultPath);
                string outputDirectory = Path.Combine(Root, "Temp",
                    request.menuFirst ? "Q07-MenuFirst-Windows-Player" : "Q07-Windows-Player",
                    request.runId);
                if (Directory.Exists(outputDirectory))
                    throw new IOException("Q07 build output already exists: " + outputDirectory);
                Directory.CreateDirectory(outputDirectory);
                result.outputPath = Path.Combine(outputDirectory, request.menuFirst
                    ? "NTSD-Q07-MenuFirst.exe" : "NTSD-Q07-Windows-Mono.exe");
                File.WriteAllText(resultPath, JsonUtility.ToJson(result, true));

                string[] buildScenes = request.menuFirst
                    ? EditorBuildSettings.scenes.Where(scene => scene.enabled)
                        .Select(scene => scene.path).ToArray()
                    : new[] { "Assets/NTSD/Scene/NTSD_Battle.unity" };
                if (request.menuFirst && (buildScenes.Length != 2 ||
                    buildScenes[0] != "Assets/NTSD/Scene/NTSD_Menu.unity" ||
                    buildScenes[1] != "Assets/NTSD/Scene/NTSD_Battle.unity"))
                    throw new InvalidOperationException("Q07 Menu-first Player requires the confirmed Menu/Battle Build Settings order.");
                result.scenes = string.Join(";", buildScenes);

                previousBackend = PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone);
                backendCaptured = true;
                MethodInfo disableBurst = typeof(ProductionEntityStressPlayerBuild).GetMethod(
                    "DisableBurstForDiagnosticBuild", BindingFlags.Static | BindingFlags.NonPublic);
                if (disableBurst == null)
                    throw new MissingMethodException("The existing R8 Burst diagnostic build helper is unavailable.");
                previousBurstSettings = (byte[])disableBurst.Invoke(null, new object[] { burstSettingsPath });
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
                BuildReport buildReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = buildScenes,
                    locationPathName = result.outputPath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development,
                });
                result.buildResult = buildReport.summary.result.ToString();
                result.totalErrors = buildReport.summary.totalErrors;
                result.totalSize = checked((long)buildReport.summary.totalSize);
                if (buildReport.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Q07 Player build failed: " + result.buildResult +
                        ", errors=" + result.totalErrors);
                result.status = "PASS";
                result.message = "Windows Mono Development Player build completed; audit packaged content separately.";
            }
            catch (Exception error)
            {
                result.status = "FAIL";
                result.message = error.ToString();
                Debug.LogException(error);
            }
            finally
            {
                try
                {
                    if (backendCaptured)
                        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, previousBackend);
                }
                finally
                {
                    if (previousBurstSettings != null)
                        File.WriteAllBytes(burstSettingsPath, previousBurstSettings);
                    AssetDatabase.SaveAssets();
                    if (resultPath != null) File.WriteAllText(resultPath, JsonUtility.ToJson(result, true));
                    running = false;
                }
            }
        }
    }
}
#endif
