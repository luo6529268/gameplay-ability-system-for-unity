#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NTSD.Test
{
    internal static class ProductionEntityStressPlayerBuild
    {
        internal const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        internal const string OutputDirectory = "Temp/U9-Windows-Player";
        internal const string ExecutableName = "NTSD-U9-Windows-Mono.exe";

        [MenuItem("NTSD/Battle Architecture/U9/Build Windows Mono Player")]
        internal static void BuildWindowsMonoPlayer()
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(projectRoot, OutputDirectory);
            Directory.CreateDirectory(outputDirectory);
            string executable = Path.Combine(outputDirectory, ExecutableName);

            ScriptingImplementation previousBackend =
                PlayerSettings.GetScriptingBackend(BuildTargetGroup.Standalone);
            bool previousFrameTimingStats = PlayerSettings.enableFrameTimingStats;
            bool previousRunInBackground = PlayerSettings.runInBackground;
            string burstSettingsPath = Path.Combine(
                projectRoot,
                "ProjectSettings",
                "BurstAotSettings_StandaloneWindows.json");
            byte[] previousBurstSettings = DisableBurstForDiagnosticBuild(
                burstSettingsPath);
            try
            {
                PlayerSettings.SetScriptingBackend(
                    BuildTargetGroup.Standalone,
                    ScriptingImplementation.Mono2x);
                PlayerSettings.enableFrameTimingStats = true;
                PlayerSettings.runInBackground = true;
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = executable,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development,
                });
                if (report.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        "U9 Windows Mono Player build failed: " +
                        report.summary.result + ", errors=" +
                        report.summary.totalErrors + ".");
                }

                CopyRawBattleFiles(projectRoot, executable);

                Debug.Log(
                    "[ProductionEntityStressPlayerBuild] PASS: " + executable);
            }
            finally
            {
                PlayerSettings.runInBackground = previousRunInBackground;
                PlayerSettings.enableFrameTimingStats = previousFrameTimingStats;
                PlayerSettings.SetScriptingBackend(
                    BuildTargetGroup.Standalone,
                    previousBackend);
                File.WriteAllBytes(burstSettingsPath, previousBurstSettings);
                AssetDatabase.SaveAssets();
            }
        }

        private static byte[] DisableBurstForDiagnosticBuild(string settingsPath)
        {
            byte[] previous = File.ReadAllBytes(settingsPath);
            string json = File.ReadAllText(settingsPath);
            const string enabled = "\"EnableBurstCompilation\": true";
            const string disabled = "\"EnableBurstCompilation\": false";
            if (!json.Contains(enabled))
            {
                throw new InvalidOperationException(
                    "U9 diagnostic build could not locate the Windows Burst setting.");
            }

            File.WriteAllText(settingsPath, json.Replace(enabled, disabled));
            return previous;
        }

        private static void CopyRawBattleFiles(
            string projectRoot,
            string executable)
        {
            string outputDirectory = Path.GetDirectoryName(executable) ??
                                     string.Empty;
            string portableAssetsRoot = Path.Combine(
                outputDirectory,
                "Assets",
                "NTSD");
            CopyTreeByExtension(
                Path.Combine(projectRoot, "Assets", "NTSD", "Config"),
                Path.Combine(portableAssetsRoot, "Config"),
                ".dat",
                ".txt");
            CopyTreeByExtension(
                Path.Combine(projectRoot, "Assets", "NTSD", "Sprite"),
                Path.Combine(portableAssetsRoot, "Sprite"),
                ".bmp");

            string sourceDirectory = Path.Combine(
                projectRoot,
                "Assets",
                "NTSD",
                "Sprite",
                "UIPanels");
            string playerDataDirectory = Path.Combine(
                outputDirectory,
                Path.GetFileNameWithoutExtension(executable) + "_Data");
            string destinationDirectory = Path.Combine(
                playerDataDirectory,
                "NTSD",
                "Sprite",
                "UIPanels");
            Directory.CreateDirectory(destinationDirectory);

            CopyRequiredFile(sourceDirectory, destinationDirectory, "SPARK.bmp");
        }

        private static void CopyTreeByExtension(
            string sourceRoot,
            string destinationRoot,
            params string[] extensions)
        {
            if (!Directory.Exists(sourceRoot))
            {
                throw new DirectoryNotFoundException(sourceRoot);
            }

            foreach (string source in Directory.GetFiles(
                         sourceRoot,
                         "*",
                         SearchOption.AllDirectories))
            {
                string extension = Path.GetExtension(source);
                bool included = false;
                for (int index = 0; index < extensions.Length; index++)
                {
                    if (string.Equals(
                            extension,
                            extensions[index],
                            StringComparison.OrdinalIgnoreCase))
                    {
                        included = true;
                        break;
                    }
                }

                if (!included)
                    continue;

                string relative = source.Substring(sourceRoot.Length)
                    .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                string destination = Path.Combine(destinationRoot, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(destination) ??
                                          destinationRoot);
                File.Copy(source, destination, true);
            }
        }

        private static void CopyRequiredFile(
            string sourceDirectory,
            string destinationDirectory,
            string fileName)
        {
            string source = Path.Combine(sourceDirectory, fileName);
            if (!File.Exists(source))
            {
                throw new FileNotFoundException(
                    "Required raw battle visual file is missing.",
                    source);
            }

            File.Copy(
                source,
                Path.Combine(destinationDirectory, fileName),
                true);
        }
    }
}
#endif
