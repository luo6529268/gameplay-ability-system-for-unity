#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NTSD.Test
{
    internal static class BattleSinglePlayerRuntimeValidationBuild
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string OutputDirectory = "Temp/U7-Windows-IL2CPP";
        private const string PendingBackendKey =
            "NTSD.BattleSinglePlayerRuntimeValidation.PendingBackend";
        private static readonly string[] PlayerCompileDependencyPaths =
        {
            "Packages/com.unity.nuget.newtonsoft-json/Runtime/AOT/Newtonsoft.Json.dll",
            "Packages/com.unity.visualscripting/Runtime/VisualScripting.Flow/Dependencies/NCalc/Unity.VisualScripting.Antlr3.Runtime.dll",
            "Assets/Plugins/Sirenix/Assemblies/Sirenix.Serialization.Config.dll",
        };

        static BattleSinglePlayerRuntimeValidationBuild()
        {
            EditorApplication.update -= PollPendingBuild;
            EditorApplication.update += PollPendingBuild;
        }

        [MenuItem("NTSD/Battle Architecture/U7/Build And Run Windows IL2CPP Gate")]
        internal static void BuildAndRunWindowsIl2CppGate()
        {
            RequestBuild("IL2CPP");
        }

        [MenuItem("NTSD/Battle Architecture/U7/Build And Run Windows Mono Gate")]
        internal static void BuildAndRunWindowsMonoGate()
        {
            RequestBuild("Mono");
        }

        private static void RequestBuild(string backendName)
        {
            SessionState.SetString(PendingBackendKey, backendName);
            Debug.Log(
                "[BattleSinglePlayerRuntimeValidation] Requested Windows " +
                backendName + " gate.");
        }

        private static void PollPendingBuild()
        {
            string backendName = SessionState.GetString(PendingBackendKey, string.Empty);
            if (string.IsNullOrEmpty(backendName) ||
                EditorApplication.isCompiling ||
                EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            SessionState.EraseString(PendingBackendKey);
            ScriptingImplementation backend =
                string.Equals(backendName, "IL2CPP", StringComparison.Ordinal)
                    ? ScriptingImplementation.IL2CPP
                    : ScriptingImplementation.Mono2x;
            try
            {
                BuildAndRunWindowsGate(backend, backendName);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void BuildAndRunWindowsGate(
            ScriptingImplementation backend,
            string backendName)
        {
            ValidateWindowsBackendSupport(backend, backendName);
            ValidatePlayerCompileDependencies();

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(
                projectRoot,
                OutputDirectory,
                backendName);
            Directory.CreateDirectory(outputDirectory);
            string executable = Path.Combine(
                outputDirectory,
                "NTSD-U7-Windows-" + backendName + ".exe");
            string reportPath = Path.Combine(outputDirectory, "u7-runtime-report.json");
            string logPath = Path.Combine(outputDirectory, "u7-runtime-player.log");
            string runId = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fffffff");

            ScriptingImplementation previousBackend =
                PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone);
            bool previousFrameTimingStats = PlayerSettings.enableFrameTimingStats;
            try
            {
                PlayerSettings.SetScriptingBackend(
                    BuildTargetGroup.Standalone,
                    backend);
                PlayerSettings.enableFrameTimingStats = true;
                BuildReport buildReport = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = executable,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development |
                        BuildOptions.CleanBuildCache,
                });
                if (buildReport.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "U7 Windows " + backendName + " Player build failed: " +
                        buildReport.summary.result + ", errors=" +
                        buildReport.summary.totalErrors + ".");
                }
            }
            finally
            {
                PlayerSettings.enableFrameTimingStats = previousFrameTimingStats;
                PlayerSettings.SetScriptingBackend(
                    BuildTargetGroup.Standalone,
                    previousBackend);
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = string.Join(" ", new[]
                {
                    "-batchmode",
                    "-nographics",
                    "-logFile", Quote(logPath),
                    "--ntsd-u7-runtime-validation",
                    "--ntsd-u7-output", Quote(reportPath),
                    "--ntsd-u7-run-id", Quote(runId),
                }),
                WorkingDirectory = outputDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            using Process process = Process.Start(startInfo);
            if (process == null)
            {
                throw new InvalidOperationException(
                    "Failed to start U7 Windows " + backendName + " Player.");
            }
            if (!process.WaitForExit(120000))
            {
                throw new TimeoutException(
                    "U7 Windows " + backendName +
                    " Player did not exit within 120 seconds.");
            }
            if (!File.Exists(reportPath))
            {
                throw new InvalidOperationException(
                    "U7 Windows " + backendName +
                    " Player did not write its report. See " + logPath + ".");
            }

            BattleSinglePlayerRuntimeValidationReport validationReport =
                JsonUtility.FromJson<BattleSinglePlayerRuntimeValidationReport>(
                    File.ReadAllText(reportPath));
            if (validationReport == null ||
                !string.Equals(validationReport.runId, runId, StringComparison.Ordinal) ||
                !string.Equals(validationReport.status, "Passed", StringComparison.Ordinal) ||
                !string.Equals(
                    validationReport.scriptingBackend,
                    backendName,
                    StringComparison.Ordinal) ||
                !validationReport.pureValueTransferPassed ||
                !validationReport.restoreReplayPassed ||
                process.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    "U7 Windows " + backendName + " runtime gate failed. Exit=" + process.ExitCode +
                    ", status=" + validationReport?.status +
                    ", backend=" + validationReport?.scriptingBackend +
                    ", failure=" + validationReport?.failure +
                    ". See " + reportPath + " and " + logPath + ".");
            }

            if (backend == ScriptingImplementation.IL2CPP)
            {
                ValidateCrossRuntimeParity(projectRoot, validationReport);
            }

            Debug.Log(
                "[BattleSinglePlayerRuntimeValidation] Windows " + backendName + " PASS. " +
                "report=" + reportPath + ", checksum=" +
                validationReport.restoredChecksum + ".");
        }

        private static void ValidateWindowsBackendSupport(
            ScriptingImplementation backend,
            string backendName)
        {
            if (backend != ScriptingImplementation.IL2CPP)
            {
                return;
            }

            string variationsDirectory = Path.Combine(
                EditorApplication.applicationContentsPath,
                "PlaybackEngines",
                "windowsstandalonesupport",
                "Variations");
            string[] variations = Directory.Exists(variationsDirectory)
                ? Directory.GetDirectories(variationsDirectory)
                : Array.Empty<string>();
            for (int i = 0; i < variations.Length; i++)
            {
                string variationName = Path.GetFileName(variations[i]);
                if (variationName.IndexOf(
                        "il2cpp",
                        StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return;
                }
            }

            string[] installedVariationNames = new string[variations.Length];
            for (int i = 0; i < variations.Length; i++)
            {
                installedVariationNames[i] = Path.GetFileName(variations[i]);
            }

            throw new InvalidOperationException(
                "U7 Windows " + backendName + " gate cannot run because Unity " +
                Application.unityVersion +
                " has no Windows IL2CPP Player variation. Install Windows Build " +
                "Support (IL2CPP) for this exact Editor version. Variations path=" +
                variationsDirectory + ", installed=[" +
                string.Join(", ", installedVariationNames) + "].");
        }

        private static void ValidateCrossRuntimeParity(
            string projectRoot,
            BattleSinglePlayerRuntimeValidationReport il2CppReport)
        {
            string monoDirectory = Path.Combine(
                projectRoot,
                OutputDirectory,
                "Mono");
            string monoReportPath = Path.Combine(
                monoDirectory,
                "u7-runtime-report.json");
            if (!File.Exists(monoReportPath))
            {
                monoReportPath = Path.Combine(
                    monoDirectory,
                    "u7-runtime-report-final.json");
            }
            if (!File.Exists(monoReportPath))
            {
                throw new InvalidOperationException(
                    "U7 Windows IL2CPP runtime passed, but no Windows Mono report " +
                    "exists for the required cross-runtime comparison. Run the " +
                    "Windows Mono gate first. Expected " + monoReportPath + ".");
            }

            BattleSinglePlayerRuntimeValidationReport monoReport =
                JsonUtility.FromJson<BattleSinglePlayerRuntimeValidationReport>(
                    File.ReadAllText(monoReportPath));
            if (monoReport == null ||
                !string.Equals(monoReport.status, "Passed", StringComparison.Ordinal) ||
                !string.Equals(
                    monoReport.scriptingBackend,
                    "Mono",
                    StringComparison.Ordinal) ||
                !string.Equals(
                    monoReport.unityVersion,
                    il2CppReport.unityVersion,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    monoReport.platform,
                    il2CppReport.platform,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    monoReport.sourceChecksum,
                    il2CppReport.sourceChecksum,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    monoReport.restoredChecksum,
                    il2CppReport.restoredChecksum,
                    StringComparison.Ordinal) ||
                !string.Equals(
                    monoReport.replayChecksum,
                    il2CppReport.replayChecksum,
                    StringComparison.Ordinal) ||
                monoReport.restoredSlot != il2CppReport.restoredSlot ||
                monoReport.restoredStableId != il2CppReport.restoredStableId ||
                monoReport.restoredGeneration != il2CppReport.restoredGeneration)
            {
                throw new InvalidOperationException(
                    "U7 Windows Mono/IL2CPP cross-runtime parity failed. " +
                    "Mono report=" + monoReportPath +
                    ", Mono source/restored/replay=" +
                    monoReport?.sourceChecksum + "/" +
                    monoReport?.restoredChecksum + "/" +
                    monoReport?.replayChecksum +
                    ", IL2CPP source/restored/replay=" +
                    il2CppReport.sourceChecksum + "/" +
                    il2CppReport.restoredChecksum + "/" +
                    il2CppReport.replayChecksum + ".");
            }

            Debug.Log(
                "[BattleSinglePlayerRuntimeValidation] Windows Mono/IL2CPP " +
                "cross-runtime parity PASS. Mono report=" + monoReportPath +
                ", source/restored/replay=" + il2CppReport.sourceChecksum + "/" +
                il2CppReport.restoredChecksum + "/" +
                il2CppReport.replayChecksum + ".");
        }

        private static void ValidatePlayerCompileDependencies()
        {
            for (int i = 0; i < PlayerCompileDependencyPaths.Length; i++)
            {
                string path = PlayerCompileDependencyPaths[i];
                PluginImporter importer =
                    AssetImporter.GetAtPath(path) as PluginImporter;
                if (importer == null)
                {
                    throw new InvalidOperationException(
                        "Required Player compile dependency was not imported: " + path + ".");
                }

                Debug.Log(
                    "[BattleSinglePlayerRuntimeValidation] Player dependency " + path +
                    ", any=" + importer.GetCompatibleWithAnyPlatform() +
                    ", win64=" + importer.GetCompatibleWithPlatform(
                        BuildTarget.StandaloneWindows64) + ".");
            }
        }

        private static string Quote(string value)
        {
            return "\"" + (value ?? string.Empty).Replace("\"", "\\\"") + "\"";
        }
    }
}
#endif
