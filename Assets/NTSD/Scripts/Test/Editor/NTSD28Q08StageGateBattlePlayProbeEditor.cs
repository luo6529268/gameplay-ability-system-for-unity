#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q08StageGateBattlePlayProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_Q08_StageGatePlay_20260927.request.json";
        private const string ResultPath =
            "Temp/NTSD28_Q08_StageGatePlay_20260927.result.json";
        private static bool running;

        [Serializable]
        private sealed class Report
        {
            public string status = "FAIL";
            public string error;
            public string scene;
            public int tick;
            public int observedGate;
            public int projectGate;
            public int activeSlotCount;
            public bool formalCatalogReady;
            public bool exitRequested;
        }

        static NTSD28Q08StageGateBattlePlayProbeEditor()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || !File.Exists(RequestPath) || File.Exists(ResultPath) ||
                EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            running = true;
            try
            {
                Scene scene = SceneManager.GetActiveScene();
                if (!EditorApplication.isPlaying)
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                        return;
                    if (scene.path != "Assets/NTSD/Scene/NTSD_Battle.unity" ||
                        scene.isDirty)
                    {
                        Finish(new Report
                        {
                            scene = scene.path,
                            error = "A clean saved NTSD_Battle scene is required.",
                        });
                        return;
                    }

                    EditorApplication.EnterPlaymode();
                    return;
                }

                SimulationTickDriver driver =
                    UnityEngine.Object.FindObjectOfType<SimulationTickDriver>();
                if (driver?.World == null ||
                    driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                    driver.CurrentTickIndex < 2)
                {
                    if ((DateTime.UtcNow - File.GetCreationTimeUtc(RequestPath))
                        .TotalSeconds <= 180)
                        return;
                    Finish(new Report
                    {
                        scene = scene.path,
                        error = "Battle runtime did not reach Running/tick>=2 within 180 seconds.",
                    });
                    EditorApplication.ExitPlaymode();
                    return;
                }

                var report = new Report
                {
                    scene = scene.path,
                    tick = driver.CurrentTickIndex,
                    observedGate = driver.World.Runtime.SelectedModeStageGate50,
                    projectGate = ProjectBattleModeConfig.LoadDefault()
                        .Capture().SelectedStageGate50,
                    activeSlotCount = driver.World.Runtime.Roster.ActiveSlotCount,
                    formalCatalogReady =
                        driver.World.RuntimeDataCatalog?.LoganContentIdentity != null,
                    exitRequested = true,
                };
                if (report.observedGate == 1 && report.projectGate == 1 &&
                    report.activeSlotCount >= 2 && report.formalCatalogReady)
                    report.status = "PASS_CAPTURED";
                else
                    report.error = "Project stage gate or battle readiness differs.";
                Finish(report);
                EditorApplication.ExitPlaymode();
            }
            catch (Exception exception)
            {
                Finish(new Report { error = exception.ToString() });
                if (EditorApplication.isPlaying)
                    EditorApplication.ExitPlaymode();
            }
            finally
            {
                running = false;
            }
        }

        private static void Finish(Report report)
        {
            if (!File.Exists(ResultPath))
                File.WriteAllText(ResultPath, JsonUtility.ToJson(report, true));
        }
    }
}
#endif
