#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Simulation;
using UnityEditor;

namespace NTSD.Test
{
    [InitializeOnLoad]
    internal static class NTSD28Q06State13ExitPlayProbe
    {
        private const string Request = "Temp/NTSD28_Q06_State13ExitPlay.request";
        private const string Result = "Temp/NTSD28_Q06_State13ExitPlay.result.json";

        static NTSD28Q06State13ExitPlayProbe()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(Request) || File.ReadAllText(Request).Trim() != "run")
                return;
            var driver = SimulationTickDriver.Instance;
            var scene = driver?.World;
            if (scene == null || driver.CurrentTickIndex < 5 || !scene.IsBattleSnapshotBoundaryReady)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            File.WriteAllText(Request, "running");
            var input = new FrameInputSet(driver.CurrentTickIndex, Array.Empty<SimulationPlayerInput>());
            ulong checksum = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input);
            int borrowers = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            var errors = new List<string>();
            int passed = 0;
            foreach (int index in new[] { 0, 1, 4 })
            {
                try
                {
                    NTSD28Q06State13ExitTailEditorTests.VerifyRendererForPlay(index);
                    passed++;
                }
                catch (Exception error)
                {
                    errors.Add("case=" + index + ": " + error);
                }
            }
            bool unchanged = scene.CaptureRuntimeChecksum64(driver.CurrentTickIndex, input) == checksum;
            int after = LF2ObjectPool.Instance.ActiveObjectCountForAcceptance;
            File.WriteAllText(Result, JsonConvert.SerializeObject(new
            {
                status = errors.Count == 0 && passed == 3 && unchanged && borrowers == after ? "PASS" : "FAIL",
                passedCases = passed, errors, sceneChecksumUnchanged = unchanged,
                rendererBorrowersBefore = borrowers, rendererBorrowersAfter = after,
                scope = "Actual pooled Renderer state13/action200 exit and state18 positive control; source count/audio/native RNG, approved C17 exception preserved; no visual/physical-key parity claim."
            }, Formatting.Indented));
            File.WriteAllText(Request, "done");
            File.WriteAllText("Temp/NTSD28_Q05_ReplayPlay.request", "run");
        }
    }
}
#endif
