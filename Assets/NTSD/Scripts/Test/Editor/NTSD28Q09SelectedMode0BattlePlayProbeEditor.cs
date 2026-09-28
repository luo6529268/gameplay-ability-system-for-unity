#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q09SelectedMode0BattlePlayProbeEditor
    {
        private const string RequestPath =
            "Temp/NTSD28_Q09_ModeGateDefault_20260928_04.request.json";
        private const string ResultPath =
            "Temp/NTSD28_Q09_ModeGateDefault_20260928_04.result.json";
        private static bool running;
        private static DateTime firstPlayPollUtc;

        [Serializable]
        private sealed class Report
        {
            public string status = "FAIL";
            public string error;
            public string scene;
            public int currentTick;
            public int frameTick;
            public int assetGate;
            public int catalogGate;
            public int frameGate;
            public int publishedFrameGate;
            public int planTick;
            public bool planValid;
            public string assetFingerprint;
            public string catalogFingerprint;
            public bool formalCatalogReady;
            public int entityCount;
            public int commandCount;
            public bool commandsMaterialized;
            public long skippedMissingCueFiles;
            public long failedCueLoads;
            public bool exitRequested;
            public string lifecycleState;
            public bool isPaused;
            public float observedPlaySeconds;
        }

        static NTSD28Q09SelectedMode0BattlePlayProbeEditor()
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

                if (firstPlayPollUtc == DateTime.MinValue)
                    firstPlayPollUtc = DateTime.UtcNow;
                SimulationTickDriver driver =
                    UnityEngine.Object.FindObjectOfType<SimulationTickDriver>();
                BattlePresentationFrame frame =
                    driver?.World?.BattlePresentation?.PublishedFrame;
                BattlePixelFramePlan plan =
                    frame != null && frame.EntityCount >= 2 &&
                    driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.CurrentTickIndex >= 2
                        ? BattleCentralRenderSystem.PrepareFrame(driver.World)
                        : default;
                BattlePresentationFrame commandFrame = plan.CapturedFrame;
                if (driver?.World == null ||
                    driver.LifecycleState != BattleRuntimeLifecycleState.Running ||
                    driver.CurrentTickIndex < 2 || frame == null ||
                    frame.TickIndex < 1 || frame.EntityCount < 2 ||
                    !plan.IsValid || plan.IsStale ||
                    plan.SimulationTick != frame.TickIndex ||
                    commandFrame == null || !commandFrame.CommandsMaterialized ||
                    commandFrame.EntityCount < 2 || commandFrame.CommandCount == 0)
                {
                    float observedPlaySeconds =
                        (float)(DateTime.UtcNow - firstPlayPollUtc).TotalSeconds;
                    if (observedPlaySeconds <= 180f)
                        return;
                    Finish(new Report
                    {
                        scene = scene.path,
                        currentTick = driver?.CurrentTickIndex ?? -1,
                        frameTick = frame?.TickIndex ?? -1,
                        entityCount = frame?.EntityCount ?? -1,
                        commandsMaterialized = commandFrame?.CommandsMaterialized ?? false,
                        commandCount = commandFrame?.CommandCount ?? 0,
                        planValid = plan.IsValid && !plan.IsStale,
                        planTick = plan.SimulationTick,
                        publishedFrameGate = frame?.SelectedModeReviveLivesGate54 ?? -1,
                        skippedMissingCueFiles = UnityEngine.Object
                            .FindObjectOfType<NTSDSoundPlayer>()?
                            .SkippedMissingBattleCueFileCountForDiagnostics ?? 0,
                        lifecycleState = driver?.LifecycleState.ToString(),
                        isPaused = driver?.IsPaused ?? false,
                        observedPlaySeconds = observedPlaySeconds,
                        error = "Battle did not publish a ready frame within 180 seconds.",
                    });
                    EditorApplication.ExitPlaymode();
                    return;
                }

                ProjectBattleModeConfig.Snapshot asset =
                    ProjectBattleModeConfig.LoadDefault().Capture();
                ProjectBattleModeConfig.Snapshot catalog =
                    driver.World.RuntimeDataCatalog.ProjectModeSnapshot;
                NTSDSoundPlayer soundPlayer =
                    UnityEngine.Object.FindObjectOfType<NTSDSoundPlayer>();
                var report = new Report
                {
                    scene = scene.path,
                    currentTick = driver.CurrentTickIndex,
                    frameTick = commandFrame.TickIndex,
                    assetGate = asset.SelectedModeReviveLivesGate54,
                    catalogGate = catalog?.SelectedModeReviveLivesGate54 ?? -1,
                    frameGate = commandFrame.SelectedModeReviveLivesGate54,
                    publishedFrameGate = frame.SelectedModeReviveLivesGate54,
                    planTick = plan.SimulationTick,
                    planValid = plan.IsValid && !plan.IsStale,
                    assetFingerprint = asset.Fingerprint,
                    catalogFingerprint = catalog?.Fingerprint,
                    formalCatalogReady =
                        driver.World.RuntimeDataCatalog.LoganContentIdentity != null,
                    entityCount = commandFrame.EntityCount,
                    commandCount = commandFrame.CommandCount,
                    commandsMaterialized = commandFrame.CommandsMaterialized,
                    skippedMissingCueFiles = soundPlayer?
                        .SkippedMissingBattleCueFileCountForDiagnostics ?? 0,
                    failedCueLoads = soundPlayer?
                        .FailedPreparedCueLoadCountForDiagnostics ?? 0,
                    exitRequested = true,
                    lifecycleState = driver.LifecycleState.ToString(),
                    isPaused = driver.IsPaused,
                    observedPlaySeconds =
                        (float)(DateTime.UtcNow - firstPlayPollUtc).TotalSeconds,
                };
                if (report.assetGate == 0 && report.catalogGate == 0 &&
                    report.frameGate == 0 && report.publishedFrameGate == 0 &&
                    report.planValid && report.planTick == report.frameTick &&
                    string.Equals(report.assetFingerprint,
                        report.catalogFingerprint, StringComparison.Ordinal) &&
                    report.formalCatalogReady && report.entityCount >= 2 &&
                    report.commandCount > 0 && report.commandsMaterialized)
                    report.status = "PASS_CAPTURED";
                else
                    report.error = "Selected project mode carrier or presentation frame differs.";
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
            using (var stream = new FileStream(ResultPath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(report, true));
        }
    }
}
#endif
