#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using NTSD.UI.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q07MenuSceneCallbackPlayProbeEditor
    {
        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool restorePending;
            public string runId;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string phase;
            public string message;
            public string sourceKey;
            public string shutdownStage;
            public int selectedCharacterId;
            public int worldObjects;
            public int finalPoolBorrowers;
            public int pendingLoadingTextsAfterPrewarm;
            public bool prewarmed;
            public bool modeSelected;
            public bool roleConfirmed;
            public bool battleSceneLoaded;
            public bool battleRunning;
            public bool samePublishedKeys;
            public bool stayedStopped;
            public bool returnedToMenu;
        }

        private const string MenuScenePath = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string BattleScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string FormalFingerprint =
            "B8B13894088DDE96D71771C9110FBD84E2C03AE712FE32222B99C5DFE8155A45";
        private static bool running;
        private static string Root => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(Root, "Temp/NTSD28_Q07_MenuSceneCallback.request.json");
        private static string ResultRoot => Path.Combine(Root,
            "artifacts/diagnostics/NTSD28-Q07-MENU-SCENE-CALLBACK-PLAY-001");

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath))
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
            if (request == null) return;
            if (!EditorApplication.isPlaying && request.restorePending &&
                !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (SceneManager.GetActiveScene().path == MenuScenePath &&
                    !SceneManager.GetActiveScene().isDirty)
                {
                    EditorSceneManager.OpenScene(BattleScenePath, OpenSceneMode.Single);
                    request.restorePending = false;
                    File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
                }
                return;
            }
            if (!request.requested || EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isPlaying) return;
            string output = Path.Combine(ResultRoot, request.runId + ".json");
            if (!EditorApplication.isPlaying)
            {
                try
                {
                    Require(ValidRunId(request.runId), "Invalid run ID.");
                    Require(!File.Exists(output), "Result already exists: " + output);
                    Require(SceneManager.GetActiveScene().path == BattleScenePath &&
                        !SceneManager.GetActiveScene().isDirty, "Expected a saved Battle Scene before Menu Play.");
                    Require(EditorBuildSettings.scenes.Any(scene =>
                        scene.enabled && scene.path == BattleScenePath),
                        "Additive Battle Scene is absent from enabled Editor Build Settings scenes.");
                    EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
                    request.restorePending = true;
                    File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
                    EditorApplication.EnterPlaymode();
                }
                catch (Exception error)
                {
                    request.requested = false;
                    File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
                    if (ValidRunId(request.runId) && !File.Exists(output))
                    {
                        Directory.CreateDirectory(ResultRoot);
                        File.WriteAllText(output, JsonUtility.ToJson(new Report
                        {
                            runId = request.runId,
                            status = "FAIL",
                            phase = "edit-preflight",
                            message = error.ToString()
                        }, true));
                    }
                }
                return;
            }
            request.requested = false;
            File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
            running = true;
            Run(request.runId, output).Forget();
        }

        private static async UniTask Run(string runId, string output)
        {
            var report = new Report { runId = runId, status = "RUNNING", phase = "menu" };
            try
            {
                Require(SceneManager.GetActiveScene().path == MenuScenePath,
                    "Play did not start from the real Menu Scene.");
                await UniTask.NextFrame();
                var main = UnityEngine.Object.FindObjectOfType<MainMenuController>(true);
                var menu = UnityEngine.Object.FindObjectOfType<MenuUIController>(true);
                var loading = UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true);
                Require(main != null && menu != null && loading != null &&
                    main._GameStartButton != null && EventSystem.current != null,
                    "Menu Scene callback components are missing.");
                Require(GameConfig.Instance != null && GameConfig.Instance.BattleContentRuntimeRoot ==
                    "Assets/NTSD/Content/LoganRuntime", "Menu did not select the serialized formal root.");

                report.phase = "game-start";
                main._GameStartButton.OnPointerDown(new PointerEventData(EventSystem.current));
                await WaitUntil(() => loading.IsPrewarmed, 240f, "Menu loading prewarm timed out.");
                report.prewarmed = true;
                var manager = CharacterAnimtorManager.TryGetInstance();
                Require(manager?.PublishedLoganContentIdentity?.SemanticFingerprint == FormalFingerprint,
                    "Menu loading did not publish the formal content identity.");
                var pendingTexts = (Queue<string>)typeof(LoadingPrewarmController)
                    .GetField("pendingTexts", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(loading);
                report.pendingLoadingTextsAfterPrewarm = pendingTexts.Count;

                report.phase = "mode";
                await WaitUntil(() => UnityEngine.Object.FindObjectOfType<SelectGameModeController>(true)
                    ?.gameObject.activeInHierarchy == true,
                    Mathf.Clamp(pendingTexts.Count * 0.2f + 15f, 30f, 600f),
                    "VS mode panel did not appear after loading progress drained.");
                var mode = UnityEngine.Object.FindObjectOfType<SelectGameModeController>(true);
                var modeOptions = mode.GetComponentInChildren<MenuOptionList>(true);
                Require(modeOptions != null && modeOptions.CurrentIndex == 0, "VS mode option is unavailable.");
                modeOptions.OnConfirm();
                report.modeSelected = true;

                report.phase = "role";
                await WaitUntil(() => UnityEngine.Object.FindObjectOfType<CharacterSelectionController>(true)
                    ?.gameObject.activeInHierarchy == true, 10f, "Character panel did not appear.");
                var selection = UnityEngine.Object.FindObjectOfType<CharacterSelectionController>(true);
                Require(selection.PlayerSlots.Count > 0 && selection.PlayerSlots[0] != null,
                    "The first role slot is unavailable.");
                SelectRoleItem slot = selection.PlayerSlots[0];
                slot.RefreshAvailableCharacters();
                slot.OnJoin();
                for (int i = 0; i < 400 && slot.SelectedCharacterId != 2; i++)
                    slot.OnNavigateCharacter(1);
                Require(slot.SelectedCharacterId == 2, "Formal Naruto OID2 is not selectable in the Menu.");
                slot.OnConfirmCharacter();
                slot.OnConfirmTeam();
                Require(slot.State == SelectRoleState.Confirmed, "Naruto role confirmation failed.");
                report.selectedCharacterId = slot.SelectedCharacterId;
                report.roleConfirmed = true;

                report.phase = "cmc";
                await WaitUntil(() => selection.Step == CharacterSelectionStep.ComputerCount,
                    15f, "Character countdown did not reach CMC.");
                var cmc = UnityEngine.Object.FindObjectOfType<CMCRootController>(true);
                var cmcOptions = cmc?.GetComponentInChildren<MenuOptionList>(true);
                Require(cmcOptions != null && cmcOptions.CurrentIndex == 0,
                    "Zero-computer CMC confirmation is unavailable.");
                cmcOptions.OnConfirm();

                report.phase = "settings";
                await WaitUntil(() => selection.Step == CharacterSelectionStep.SettingBattleBg,
                    10f, "Battle settings did not appear.");
                var settings = UnityEngine.Object.FindObjectOfType<SettingBattleBgController>(true);
                var settingsOptions = settings?.GetComponentInChildren<MenuOptionList>(true);
                Require(settingsOptions != null && settingsOptions.CurrentIndex == 0,
                    "Fight confirmation is unavailable.");
                settingsOptions.OnConfirm();

                report.phase = "battle";
                await WaitUntil(() => AppManager.Instance != null &&
                    AppManager.Instance.State == AppFlowState.BattleRunning, 120f,
                    "Menu callback chain did not reach BattleRunning.");
                report.battleRunning = true;
                Scene battle = SceneManager.GetSceneByName("NTSD_Battle");
                report.battleSceneLoaded = battle.IsValid() && battle.isLoaded;
                Require(report.battleSceneLoaded, "Battle Scene was not additively loaded.");
                Require(AppManager.Instance.CurrentMatchConfig?.players?.Any(p =>
                    p.use && p.characterId == 2) == true, "The selected Naruto did not enter MatchConfig.");
                SimulationTickDriver driver = SimulationTickDriver.Instance;
                Require(driver?.World != null && driver.World.ObjectCount > 0,
                    "Menu-started battle has no live World actors.");
                report.worldObjects = driver.World.ObjectCount;
                manager = CharacterAnimtorManager.TryGetInstance();
                report.sourceKey = await manager.ValidateConfiguredContentForBattleAsync();
                report.samePublishedKeys = !string.IsNullOrEmpty(report.sourceKey) &&
                    GameDataManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey &&
                    CharacterUIResourceManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey;
                Require(report.samePublishedKeys &&
                    manager.PublishedLoganContentIdentity?.SemanticFingerprint == FormalFingerprint,
                    "Menu-started battle does not have three matching formal publication owners.");

                report.phase = "unload";
                AsyncOperation unload = AppManager.Instance.UnloadBattle();
                Require(unload != null, "Ordered battle unload refused to start.");
                await unload.ToUniTask();
                await UniTask.NextFrame();
                await UniTask.NextFrame();
                report.stayedStopped = driver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                    driver.World == null;
                var pool = LF2ObjectPool.TryGetInstance();
                report.finalPoolBorrowers = pool == null ? -1 :
                    pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
                report.returnedToMenu = !SceneManager.GetSceneByName("NTSD_Battle").isLoaded &&
                    AppManager.Instance.State == AppFlowState.MenuMain;
                Require(report.stayedStopped && report.finalPoolBorrowers == 0 &&
                    report.returnedToMenu, "Battle unload left runtime or Menu state behind.");
                report.shutdownStage = "Stopped";
                report.status = "PASS";
                report.message = "Actual Menu Scene component callbacks reached formal-content additive battle and ordered Menu return.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.message = error.ToString();
            }
            finally
            {
                Directory.CreateDirectory(ResultRoot);
                File.WriteAllText(output, JsonUtility.ToJson(report, true));
                running = false;
                EditorApplication.delayCall += () =>
                {
                    if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
                };
            }
        }

        private static async UniTask WaitUntil(Func<bool> predicate, float seconds, string failure)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
                await UniTask.NextFrame();
            Require(predicate(), failure);
        }

        private static bool ValidRunId(string id)
        {
            return !string.IsNullOrEmpty(id) && id.All(c => char.IsLetterOrDigit(c) || c == '-');
        }

        private static void Require(bool condition, string failure)
        {
            if (!condition) throw new InvalidOperationException(failure);
        }
    }
}
#endif
