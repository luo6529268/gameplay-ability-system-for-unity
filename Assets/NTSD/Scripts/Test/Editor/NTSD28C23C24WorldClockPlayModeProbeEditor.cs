#if UNITY_EDITOR
using System;
using System.IO;

using NTSD.Simulation;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public static class NTSD28C23C24WorldClockPlayModeProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_B3_C23_C24_WorldClock.request";
        private const string ResultPath = "Temp/NTSD28_B3_C23_C24_WorldClock.result.json";

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

        [MenuItem("NTSD/Battle Diagnostics/B3/Run C23 C24 World Clock Play Probe")]
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
                world.ActiveBattleTickPhaseDiagnosticsForDiagnostics?.Enabled == true;
            try
            {
                driver.SetPaused(true);
                BattleTickPhaseDiagnostics diagnostics =
                    diagnosticsAlreadyEnabled
                        ? world.ActiveBattleTickPhaseDiagnosticsForDiagnostics
                        : world.EnableBattleTickPhaseDiagnosticsForDiagnostics();

                report.phase12Before = world.NativeResourcePhase12;
                report.phase3Before = world.NativeResourcePhase3;
                report.sequenceBefore = world.NativeFrameSequence;
                int expectedTick = driver.CurrentTickIndex + 1;
                Require(driver.StepOneTick(
                        ignorePaused: true,
                        buildPresentation: false),
                    "Production driver rejected the C23/C24 tick.");

                report.startTick = expectedTick;
                report.endTick = driver.CurrentTickIndex;
                report.phase12After = world.NativeResourcePhase12;
                report.phase3After = world.NativeResourcePhase3;
                report.sequenceAfter = world.NativeFrameSequence;
                report.phaseCount = diagnostics.LastPhaseSequenceCount;
                report.phase24 = PhaseAt(diagnostics, 24);
                report.phase25 = PhaseAt(diagnostics, 25);
                report.phase26 = PhaseAt(diagnostics, 26);
                report.phase27 = PhaseAt(diagnostics, 27);

                Require(driver.CurrentTickIndex == expectedTick &&
                        report.phase12After == (report.phase12Before + 1) % 12 &&
                        report.phase3After == (report.phase3Before + 1) % 3 &&
                        report.sequenceAfter == report.sequenceBefore + 1UL &&
                        report.phaseCount == 34 &&
                        report.phase24 == "FramePostProcess" &&
                        report.phase25 == "NativeResourceTick" &&
                        report.phase26 == "NativeFrameTick" &&
                        report.phase27 == "FrameAdvance",
                    "C23/C24 world clocks or phase placement are incorrect.");

                report.status = "PASS";
                report.message = "C23 resource phases and C24 frame sequence committed once before the temporary C25 proxy.";
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

        private static string PhaseAt(BattleTickPhaseDiagnostics diagnostics, int index)
        {
            return diagnostics.TryGetLastPhaseAt(index, out BattleTickPhase phase)
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
            File.WriteAllText(ProjectPath(ResultPath), JsonUtility.ToJson(report, true));
            if (report.status == "PASS")
                Debug.Log("[NTSD28C23C24WorldClockPlayProbe] PASS");
            else
                Debug.LogError("[NTSD28C23C24WorldClockPlayProbe] " + report.message);
        }

        private static void ExitPlayModeAfterRequest()
        {
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public int startTick;
            public int endTick;
            public int phase12Before;
            public int phase12After;
            public int phase3Before;
            public int phase3After;
            public ulong sequenceBefore;
            public ulong sequenceAfter;
            public int phaseCount;
            public string phase24;
            public string phase25;
            public string phase26;
            public string phase27;
        }
    }
}
#endif
