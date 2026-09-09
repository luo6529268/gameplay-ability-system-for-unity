#if UNITY_EDITOR
using System;
using System.IO;

using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class NTSD28C25NestedTailSkeletonPlayModeProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_B3_C25_NestedTailSkeleton.request";
        private const string ResultPath =
            "Temp/NTSD28_B3_C25_NestedTailSkeleton.result.json";

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string requestPath = ProjectPath(RequestPath);
            if (!File.Exists(requestPath))
                return;
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    EditorApplication.EnterPlaymode();
                return;
            }

            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World == null || driver.CurrentTickIndex < 5)
                return;

            File.Delete(requestPath);
            string resultPath = ProjectPath(ResultPath);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
            Run();
            EditorApplication.delayCall += ExitPlayModeAfterRequest;
        }

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C25 Nested Tail Skeleton Play Probe")]
        public static void Run()
        {
            var report = new Report();
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            SimulationWorld world = driver?.World;
            if (!EditorApplication.isPlaying || driver == null || world == null)
            {
                report.status = "FAIL";
                report.message = "Play Mode production world is unavailable.";
                Write(report);
                return;
            }

            bool previousPaused = driver.IsPaused;
            bool diagnosticsAlreadyEnabled =
                world.ActiveBattleTickPhaseDiagnosticsForDiagnostics?.Enabled ==
                true;
            try
            {
                driver.SetPaused(true);
                BattleTickPhaseDiagnostics diagnostics =
                    diagnosticsAlreadyEnabled
                        ? world.ActiveBattleTickPhaseDiagnosticsForDiagnostics
                        : world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(
                        ignorePaused: true,
                        buildPresentation: true),
                    "Production driver rejected the C25 skeleton tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.phaseCount = diagnostics.LastPhaseSequenceCount;
                report.phase25 = PhaseAt(diagnostics, 25);
                report.phase26 = PhaseAt(diagnostics, 26);
                report.phase27 = PhaseAt(diagnostics, 27);
                report.phase28 = PhaseAt(diagnostics, 28);
                report.phase29 = PhaseAt(diagnostics, 29);
                report.phase30 = PhaseAt(diagnostics, 30);
                report.phase31 = PhaseAt(diagnostics, 31);
                report.phase32 = PhaseAt(diagnostics, 32);
                report.phase33 = PhaseAt(diagnostics, 33);
                report.publishedTick =
                    world.BattlePresentation?.PublishedFrame?.TickIndex ?? -1;

                Require(driver.CurrentTickIndex == expectedTick &&
                        report.phaseCount == 34 &&
                        report.phase25 == "NativeResourceTick" &&
                        report.phase26 == "NativeFrameTick" &&
                        report.phase27 == "LateEntityUpdate" &&
                        report.phase28 == "FrameAdvance" &&
                        report.phase29 == "Stage" &&
                        report.phase30 == "RandomWeaponDropTail" &&
                        report.phase31 == "EntityPostFrameTail" &&
                        report.phase32 == "BattleResults" &&
                        report.phase33 == "RenderDispatch" &&
                        report.publishedTick == expectedTick,
                    "C25 skeleton, legacy serial, session tail, or completed-tick presentation placement is incorrect.");

                report.status = "PASS";
                report.message =
                    "C25 entered immediately after C24 and the published frame observed the completed normal tick.";
            }
            catch (Exception exception)
            {
                report.status = "FAIL";
                report.message = exception.ToString();
            }
            finally
            {
                if (!diagnosticsAlreadyEnabled)
                    world.DisableBattleTickPhaseDiagnosticsForDiagnostics();
                driver.SetPaused(previousPaused);
                Write(report);
            }
        }

        private static string PhaseAt(
            BattleTickPhaseDiagnostics diagnostics,
            int index)
        {
            return diagnostics.TryGetLastPhaseAt(
                    index,
                    out BattleTickPhase phase)
                ? BattleTickPhaseDiagnostics.GetPhaseName(phase)
                : string.Empty;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void Write(Report report)
        {
            File.WriteAllText(
                ProjectPath(ResultPath),
                JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C25NestedTailSkeletonPlayProbe] PASS");
            else
                Debug.LogError(
                    "[NTSD28C25NestedTailSkeletonPlayProbe] " +
                    report.message);
        }

        private static void ExitPlayModeAfterRequest()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", relativePath));
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int phaseCount;
            public string phase25;
            public string phase26;
            public string phase27;
            public string phase28;
            public string phase29;
            public string phase30;
            public string phase31;
            public string phase32;
            public string phase33;
            public int publishedTick;
        }
    }
}
#endif

