#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28D024AiChildScenePlayProbeEditor
    {
        private sealed class Request
        {
            public bool requested;
            public string runId;
        }

        private sealed class Report
        {
            public string runId;
            public string status;
            public string phase;
            public string error;
            public string initialScene;
            public string contentKey;
            public int battleMode = -1;
            public int initialTick;
            public int ticksStepped;
            public int aiTargetSlot = -1;
            public string participantState;
            public string aiTargetHistory;
            public string aiFrameTransitions;
            public string newBirths;
            public int childSlot = -1;
            public int childObjectId = -1;
            public int missingSourceCount;
            public string missingSourceSlots;
            public double sourceX;
            public double sourceZ;
            public double physicalX;
            public double physicalZ;
            public double viewScale;
            public bool childSourceInitialized;
            public bool childRendererPresent;
            public bool stopped;
            public bool returnedToMenu;
            public int poolBorrowersAfter;
        }

        private static bool running;
        private static string Root => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(Root,
            "Temp/NTSD28_D024_AiChildScenePlay.request.json");
        private static string ResultRoot => Path.Combine(Root,
            "artifacts/diagnostics/NTSD28-USER-D024-AI-CHILD-SCENE-PLAY-001");

        static NTSD28D024AiChildScenePlayProbeEditor()
        {
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(RequestPath))
                return;

            Request request;
            try
            {
                request = JsonConvert.DeserializeObject<Request>(
                    File.ReadAllText(RequestPath));
            }
            catch (IOException)
            {
                return;
            }

            if (request == null || !request.requested)
                return;
            if (!IsSafeRunId(request.runId))
            {
                WriteImmediateFailure(request.runId, "Invalid request runId.");
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene current = SceneManager.GetActiveScene();
                if (current.isDirty || current.name != "NTSD_Menu")
                {
                    WriteImmediateFailure(request.runId,
                        "Play probe requires a clean, saved NTSD_Menu scene.");
                    return;
                }
                EditorApplication.EnterPlaymode();
                return;
            }

            request.requested = false;
            File.WriteAllText(RequestPath, JsonConvert.SerializeObject(request));
            running = true;
            Run(request).Forget();
        }

        private static async UniTask Run(Request request)
        {
            var report = new Report
            {
                runId = request.runId,
                status = "RUNNING",
                phase = "menu-prewarm",
                initialScene = SceneManager.GetActiveScene().path,
            };
            try
            {
                WriteReport(request.runId, report);
                Require(SceneManager.GetActiveScene().name == "NTSD_Menu",
                    "Play did not start in the saved Menu scene.");
                Require(GameConfig.Instance != null &&
                    GameConfig.Instance.BattleContentRuntimeRoot ==
                    "Assets/NTSD/Content/LoganRuntime",
                    "Current GameConfig does not select staged Logan content.");
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
                await UniTask.NextFrame();
                LoadingPrewarmController loading =
                    UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true);
                Require(loading != null, "Menu prewarm controller is missing.");
                await loading.PrewarmOnceAsync();
                report.phase = "menu-prewarm-complete";
                WriteReport(request.runId, report);
                CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                Require(manager?.GetCharacterConfig(2) != null &&
                    manager.GetCharacterConfig(7) != null,
                    "Formal OID2/OID7 definitions are not prewarmed.");
                report.contentKey = await manager.ValidateConfiguredContentForBattleAsync();
                Require(!string.IsNullOrEmpty(report.contentKey),
                    "Formal content identity was not published.");

                report.phase = "app-birth";
                WriteReport(request.runId, report);
                var match = new MatchConfig
                {
                    seed = 2833,
                    gameMode = new GameModeConfig { battleGameModeId = 0 },
                };
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = true, characterId = 2,
                    team = 1, inputId = 1,
                });
                match.players.Add(new PlayerSlotConfig
                {
                    use = true, isHuman = false, characterId = 7,
                    team = 2, inputId = 2, aiId = 1,
                });
                AppManager app = AppManager.Instance;
                Require(app != null, "Menu AppManager is missing.");
                app.SetMatchConfig(match);
                AsyncOperation load = app.LoadBattleAdditive();
                Require(load != null, "Battle additive load was refused.");
                await load.ToUniTask();
                float deadline = Time.realtimeSinceStartup + 120f;
                SimulationTickDriver driver = null;
                while (Time.realtimeSinceStartup < deadline)
                {
                    driver = SimulationTickDriver.Instance;
                    if (app.State == AppFlowState.BattleRunning &&
                        driver?.LifecycleState == BattleRuntimeLifecycleState.Running &&
                        driver.World?.ObjectCount >= 2)
                        break;
                    await UniTask.Yield();
                }
                Require(driver?.World != null &&
                    app.State == AppFlowState.BattleRunning,
                    "AppManager did not reach a running Battle World.");

                driver.SetPaused(true);
                SimulationWorld world = driver.World;
                report.battleMode = world.BattleGameModeId;
                Require(report.battleMode == 0,
                    "Production World did not apply the requested battle mode 0.");
                LF2Entity human = world.FindEntityByRuntimeSlotForQuery(0);
                LF2Entity ai = world.FindEntityByRuntimeSlotForQuery(1);
                Require(human?.Runtime != null && ai?.Runtime != null &&
                    human.ObjectId == 2 && ai.ObjectId == 7 && ai.AiControlled,
                    "AppManager did not create the requested human/AI participants.");
                Require(human.Runtime.SourceRulePositionInitialized &&
                    ai.Runtime.SourceRulePositionInitialized,
                    "AppManager participant source position was not initialized.");
                report.viewScale = world.FixedViewRunDistanceScale;
                Require(report.viewScale > 1.0,
                    "Current battle did not apply the approved full-view distance ratio.");
                report.initialTick = driver.CurrentTickIndex;
                report.participantState = "human team=" + human.Runtime.Team +
                    "/" + human.Runtime.RelationTeam +
                    " hp=" + human.Runtime.HP +
                    " x=" + human.Runtime.X + " z=" + human.Runtime.Z +
                    " type=" + human.GetCurrentDataObjectTypeForSimulation() +
                    " state=" + human.GetState() +
                    " frame=" + (human.Frame != null ? human.Frame.N : -1) +
                    "; ai team=" + ai.Runtime.Team +
                    "/" + ai.Runtime.RelationTeam +
                    " hp=" + ai.Runtime.HP +
                    " x=" + ai.Runtime.X + " z=" + ai.Runtime.Z +
                    " type=" + ai.GetCurrentDataObjectTypeForSimulation() +
                    " state=" + ai.GetState() +
                    " controlled=" + ai.AiControlled +
                    " frame=" + (ai.Frame != null ? ai.Frame.N : -1) +
                    " coordinateX=" + ai.Runtime.Unk3FC;
                var initialIds = new HashSet<int>();
                for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                {
                    LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                    if (entity != null)
                        initialIds.Add(entity.Runtime.StableId);
                }

                report.phase = "ai-full-driver";
                WriteReport(request.runId, report);
                LF2Entity child = null;
                var targetSamples = new List<string>();
                var frameTransitions = new List<string>();
                var newBirths = new List<string>();
                var observedBirthIds = new HashSet<int>();
                int previousAiFrame = ai.Frame != null ? ai.Frame.N : -1;
                BattleAiInputDetailDiagnostics aiDiagnostics =
                    world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
                for (int step = 0; step < 600 && child == null; step++)
                {
                    int nextTick = driver.CurrentTickIndex + 1;
                    var input = new FrameInputSet(nextTick, new[]
                    {
                        new SimulationPlayerInput(0, SimulationInputButtons.Right),
                        new SimulationPlayerInput(1, SimulationInputButtons.None),
                    });
                    Require(driver.StepOneTick(input, true, true),
                        "Full Driver rejected tick " + nextTick);
                    report.ticksStepped++;
                    if (ai.Runtime.Unk360 == 0)
                        report.aiTargetSlot = 0;
                    int currentAiFrame = ai.Frame != null ? ai.Frame.N : -1;
                    if (currentAiFrame != previousAiFrame && frameTransitions.Count < 160)
                        frameTransitions.Add(nextTick + ":" + currentAiFrame);
                    previousAiFrame = currentAiFrame;
                    if (step < 10 || step == 19 || step == 39 ||
                        step == 59 || step == 73 || step == 74 ||
                        step == 75 || step == 89 || step == 119 ||
                        step == 179)
                    {
                        targetSamples.Add(nextTick + ":" + world.InputPhase +
                            "/" + human.Runtime.HP +
                            "/" + human.Runtime.HitStop +
                            "/" + human.Runtime.RelationTeam +
                            "/" + human.GetCurrentDataObjectTypeForSimulation() +
                            "/" + human.GetState() +
                            "/" + ai.Runtime.Unk360 +
                            "/" + ai.Runtime.RelationTeam +
                            "/" + ai.GetCurrentDataObjectTypeForSimulation() +
                            "/" + ai.AiControlled +
                            "/" + aiDiagnostics.AiCount +
                            "/" + aiDiagnostics.GetLastCallCount(
                                BattleAiInputDetailPhase.FindNearestGround) +
                            "/" + aiDiagnostics.GetLastCallCount(
                                BattleAiInputDetailPhase.IndexedCanonicalNearestSearch) +
                            "/" + aiDiagnostics.GetLastCallCount(
                                BattleAiInputDetailPhase.IndexedCanonicalCommitApply) +
                            "/" + ai.Runtime.Unk3FC + "/" +
                            (ai.Frame != null ? ai.Frame.N : -1) +
                            "/" + ai.Runtime.X + "/" + ai.Runtime.Z);
                    }
                    var missing = new List<string>();
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    {
                        LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                        if (entity == null)
                            continue;
                        if (!entity.Runtime.SourceRulePositionInitialized)
                            missing.Add(slot + ":" + entity.ObjectId);
                        if (!initialIds.Contains(entity.Runtime.StableId) &&
                            entity.OwnerEntityIndex == 1)
                        {
                            child = entity;
                            report.childSlot = slot;
                            report.childObjectId = entity.ObjectId;
                        }
                        if (!initialIds.Contains(entity.Runtime.StableId) &&
                            observedBirthIds.Add(entity.Runtime.StableId) && newBirths.Count < 40)
                        {
                            string birth = nextTick + ":" + slot + ":" + entity.ObjectId +
                                ":" + entity.OwnerEntityIndex + ":" + entity.Runtime.StableId;
                            newBirths.Add(birth);
                        }
                    }
                    report.missingSourceCount += missing.Count;
                    if (missing.Count > 0)
                        report.missingSourceSlots = string.Join(",", missing);
                }
                world.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
                report.aiTargetHistory = string.Join(",", targetSamples);
                report.aiFrameTransitions = string.Join(",", frameTransitions);
                report.newBirths = string.Join(",", newBirths);

                Require(report.aiTargetSlot == 0,
                    "AI did not select the human participant in full Driver.");
                Require(child != null,
                    "NO_CHILD_OBSERVED: AI did not spawn an owned child in 600 manual ticks.");
                report.childSourceInitialized =
                    child.Runtime.SourceRulePositionInitialized;
                report.childRendererPresent = child.Renderer != null;
                report.sourceX = child.Runtime.SourceRuleX;
                report.sourceZ = child.Runtime.SourceRuleZ;
                report.physicalX = child.Runtime.X;
                report.physicalZ = child.Runtime.Z;
                Require(report.missingSourceCount == 0 &&
                    report.childSourceInitialized && report.childRendererPresent,
                    "AI child lacks source history, Renderer or complete pass carriers.");
                report.status = "PASS";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }
            finally
            {
                report.phase = "shutdown";
                WriteReport(request.runId, report);
                try
                {
                    AppManager app = AppManager.Instance;
                    SimulationTickDriver driver = SimulationTickDriver.Instance;
                    if (driver?.World != null && app != null)
                    {
                        Require(app.TryShutdownBattleRuntimeBeforeSceneDestroy(
                            out BattleRuntimeShutdownReport shutdown),
                            shutdown.FailureReason);
                        report.stopped = shutdown.IsComplete;
                    }
                    if (app != null &&
                        SceneManager.GetSceneByName("NTSD_Battle").isLoaded)
                    {
                        AsyncOperation unload = app.UnloadBattle();
                        if (unload != null)
                            await unload.ToUniTask();
                    }
                    await UniTask.NextFrame();
                    LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                    report.poolBorrowersAfter = pool == null ? -1 :
                        pool.ActiveObjectCountForAcceptance +
                        pool.ActiveSpriteCountForAcceptance;
                    report.returnedToMenu = app != null &&
                        app.State == AppFlowState.MenuMain &&
                        !SceneManager.GetSceneByName("NTSD_Battle").isLoaded;
                    if (!report.stopped || !report.returnedToMenu ||
                        report.poolBorrowersAfter != 0)
                    {
                        report.status = "FAIL";
                        report.error += "\nOrdered shutdown or pool guard failed.";
                    }
                }
                catch (Exception cleanupError)
                {
                    report.status = "FAIL";
                    report.error += "\nCleanup: " + cleanupError;
                }
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
                WriteReport(request.runId, report);
                running = false;
                EditorApplication.delayCall += () =>
                {
                    if (EditorApplication.isPlaying)
                        EditorApplication.ExitPlaymode();
                };
            }
        }

        private static void WriteReport(string runId, Report report)
        {
            Directory.CreateDirectory(ResultRoot);
            File.WriteAllText(Path.Combine(ResultRoot, runId + ".json"),
                JsonConvert.SerializeObject(report, Formatting.Indented));
        }

        private static void WriteImmediateFailure(string runId, string message)
        {
            File.WriteAllText(RequestPath,
                JsonConvert.SerializeObject(new Request
                {
                    requested = false,
                    runId = runId,
                }));
            if (!IsSafeRunId(runId))
                return;
            Directory.CreateDirectory(ResultRoot);
            File.WriteAllText(Path.Combine(ResultRoot, runId + ".json"),
                JsonConvert.SerializeObject(new Report
                {
                    runId = runId,
                    status = "FAIL",
                    phase = "preflight",
                    error = message,
                }, Formatting.Indented));
        }

        private static bool IsSafeRunId(string runId)
        {
            if (string.IsNullOrEmpty(runId))
                return false;
            foreach (char value in runId)
            {
                if (!char.IsLetterOrDigit(value) && value != '-')
                    return false;
            }
            return true;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
#endif
