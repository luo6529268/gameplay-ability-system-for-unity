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
        internal const string OutputRoot = "Temp/R8-Windows-Player";
        internal const string MonoOutputDirectory = OutputRoot + "/Mono";
        internal const string Il2CppOutputDirectory = OutputRoot + "/IL2CPP";
        internal const string MonoExecutableName = "NTSD-R8-Windows-Mono.exe";
        internal const string Il2CppExecutableName =
            "NTSD-R8-Windows-IL2CPP.exe";

        [MenuItem("NTSD/Battle Architecture/R8/Build Windows Mono Player")]
        internal static void BuildWindowsMonoPlayer()
        {
            BuildWindowsPlayer(
                ScriptingImplementation.Mono2x,
                MonoOutputDirectory,
                MonoExecutableName,
                "Mono");
        }

        [MenuItem("NTSD/Battle Architecture/R8/Build Windows IL2CPP Player")]
        internal static void BuildWindowsIl2CppPlayer()
        {
            BuildWindowsPlayer(
                ScriptingImplementation.IL2CPP,
                Il2CppOutputDirectory,
                Il2CppExecutableName,
                "IL2CPP");
        }

        [MenuItem("NTSD/Battle Architecture/U9/Build Windows Mono Player")]
        internal static void BuildLegacyU9WindowsMonoPlayer()
        {
            BuildWindowsMonoPlayer();
        }

        private static void BuildWindowsPlayer(
            ScriptingImplementation backend,
            string relativeOutputDirectory,
            string executableName,
            string backendLabel)
        {
            string projectRoot = Path.GetFullPath(
                Path.Combine(Application.dataPath, ".."));
            string outputDirectory = Path.Combine(
                projectRoot,
                relativeOutputDirectory);
            Directory.CreateDirectory(outputDirectory);
            string executable = Path.Combine(outputDirectory, executableName);

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
                    backend);
                PlayerSettings.enableFrameTimingStats = true;
                PlayerSettings.runInBackground = true;

                Debug.Log(
                    "[ProductionEntityStressPlayerBuild] START: backend=" +
                    backendLabel + ", output=" + executable);
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
                        "R8 Windows " + backendLabel +
                        " Player build failed: " +
                        report.summary.result + ", errors=" +
                        report.summary.totalErrors + ".");
                }

                CopyRawBattleFiles(projectRoot, executable);

                Debug.Log(
                    "[ProductionEntityStressPlayerBuild] PASS: backend=" +
                    backendLabel + ", output=" + executable +
                    ", size=" + report.summary.totalSize +
                    ", duration=" + report.summary.totalTime + ".");
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
                    "R8 diagnostic build could not locate the Windows Burst setting.");
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
