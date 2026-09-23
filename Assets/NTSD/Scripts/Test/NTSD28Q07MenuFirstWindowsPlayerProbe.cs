#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using NTSD.UI.Menu;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace NTSD.Test
{
    internal sealed class NTSD28Q07MenuFirstWindowsPlayerProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class Report
        {
            public string status;
            public string phase;
            public string message;
            public string initialScene;
            public string sourceKey;
            public string fingerprint;
            public int selectedCharacterId;
            public int worldObjects;
            public int finalPoolBorrowers;
            public bool prewarmed;
            public bool battleSceneLoaded;
            public bool samePublishedKeys;
            public bool returnedToMenu;
            public bool stayedStopped;
        }

        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string FormalFingerprint =
            "B8B13894088DDE96D71771C9110FBD84E2C03AE712FE32222B99C5DFE8155A45";
        private static string reportPath;

#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (!Environment.GetCommandLineArgs().Contains("-ntsd-q07-menu-first-probe")) return;
            reportPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "q07-menu-first-player.json");
            var gameObject = new GameObject("NTSD28_Q07_MenuFirstWindowsPlayerProbe");
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<NTSD28Q07MenuFirstWindowsPlayerProbe>();
        }
#endif

        private async void Start()
        {
            var report = new Report { status = "RUNNING", phase = "menu",
                initialScene = SceneManager.GetActiveScene().name };
            try
            {
                Require(report.initialScene == "NTSD_Menu",
                    "Player did not cold-start from the confirmed Menu Scene.");
                Require(GameConfig.Instance != null &&
                    GameConfig.Instance.BattleContentRuntimeRoot == FormalRoot,
                    "Menu did not select the serialized formal content root.");
                await UniTask.NextFrame();
                var main = FindObjectOfType<MainMenuController>(true);
                var loading = FindObjectOfType<LoadingPrewarmController>(true);
                Require(main != null && main._GameStartButton != null &&
                    loading != null && EventSystem.current != null,
                    "Player Menu callback components are missing.");

                report.phase = "prewarm";
                main._GameStartButton.OnPointerDown(new PointerEventData(EventSystem.current));
                await WaitUntil(() => loading.IsPrewarmed, 480f,
                    "Player Menu formal prewarm timed out.");
                report.prewarmed = true;
                var manager = CharacterAnimtorManager.TryGetInstance();
                Require(manager?.PublishedLoganContentIdentity?.SemanticFingerprint == FormalFingerprint,
                    "Player Menu did not publish the formal content identity.");

                report.phase = "mode";
                await WaitUntil(() => FindObjectOfType<SelectGameModeController>(true)
                    ?.gameObject.activeInHierarchy == true, 600f,
                    "Player VS mode panel did not appear.");
                var mode = FindObjectOfType<SelectGameModeController>(true);
                var modeOptions = mode.GetComponentInChildren<MenuOptionList>(true);
                Require(modeOptions != null && modeOptions.CurrentIndex == 0,
                    "Player VS mode option is unavailable.");
                modeOptions.OnConfirm();

                report.phase = "role";
                await WaitUntil(() => FindObjectOfType<CharacterSelectionController>(true)
                    ?.gameObject.activeInHierarchy == true, 15f,
                    "Player role panel did not appear.");
                var selection = FindObjectOfType<CharacterSelectionController>(true);
                Require(selection.PlayerSlots.Count > 0 && selection.PlayerSlots[0] != null,
                    "Player role slot is unavailable.");
                SelectRoleItem slot = selection.PlayerSlots[0];
                slot.RefreshAvailableCharacters();
                slot.OnJoin();
                for (int index = 0; index < 400 && slot.SelectedCharacterId != 2; index++)
                    slot.OnNavigateCharacter(1);
                Require(slot.SelectedCharacterId == 2,
                    "Formal Naruto OID2 is not selectable in the Player Menu.");
                slot.OnConfirmCharacter();
                slot.OnConfirmTeam();
                Require(slot.State == SelectRoleState.Confirmed,
                    "Player Naruto role confirmation failed.");
                report.selectedCharacterId = slot.SelectedCharacterId;

                report.phase = "cmc";
                await WaitUntil(() => selection.Step == CharacterSelectionStep.ComputerCount,
                    20f, "Player character flow did not reach CMC.");
                var cmc = FindObjectOfType<CMCRootController>(true);
                var cmcOptions = cmc?.GetComponentInChildren<MenuOptionList>(true);
                Require(cmcOptions != null && cmcOptions.CurrentIndex == 0,
                    "Player zero-computer CMC option is unavailable.");
                cmcOptions.OnConfirm();

                report.phase = "settings";
                await WaitUntil(() => selection.Step == CharacterSelectionStep.SettingBattleBg,
                    15f, "Player battle settings did not appear.");
                var settings = FindObjectOfType<SettingBattleBgController>(true);
                var settingsOptions = settings?.GetComponentInChildren<MenuOptionList>(true);
                Require(settingsOptions != null && settingsOptions.CurrentIndex == 0,
                    "Player Fight confirmation is unavailable.");
                settingsOptions.OnConfirm();

                report.phase = "battle";
                await WaitUntil(() => AppManager.Instance != null &&
                    AppManager.Instance.State == AppFlowState.BattleRunning,
                    180f, "Player Menu callbacks did not reach BattleRunning.");
                Scene battle = SceneManager.GetSceneByName("NTSD_Battle");
                report.battleSceneLoaded = battle.IsValid() && battle.isLoaded;
                Require(report.battleSceneLoaded, "Player Battle Scene was not loaded Additive.");
                Require(AppManager.Instance.CurrentMatchConfig?.players?.Any(player =>
                    player.use && player.characterId == 2) == true,
                    "Player-selected Naruto did not enter MatchConfig.");
                SimulationTickDriver driver = SimulationTickDriver.Instance;
                Require(driver?.World != null && driver.World.ObjectCount > 0,
                    "Player Menu-started battle has no live World actors.");
                report.worldObjects = driver.World.ObjectCount;
                manager = CharacterAnimtorManager.TryGetInstance();
                report.sourceKey = await manager.ValidateConfiguredContentForBattleAsync();
                report.fingerprint = manager.PublishedLoganContentIdentity?.SemanticFingerprint;
                report.samePublishedKeys = !string.IsNullOrEmpty(report.sourceKey) &&
                    GameDataManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey &&
                    CharacterUIResourceManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey;
                Require(report.fingerprint == FormalFingerprint && report.samePublishedKeys,
                    "Player battle did not publish three matching formal content keys.");

                report.phase = "unload";
                AsyncOperation unload = AppManager.Instance.UnloadBattle();
                Require(unload != null, "Player ordered Battle unload refused to start.");
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
                    report.returnedToMenu,
                    "Player Battle unload left runtime or Menu state behind.");
                report.status = "PASS";
                report.message = "Built Menu-first Player entered formal-content Additive Battle through real callbacks and returned to Menu.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.message = error.ToString();
            }
            finally
            {
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                Application.Quit(report.status == "PASS" ? 0 : 1);
            }
        }

        private static async UniTask WaitUntil(Func<bool> predicate, float seconds, string failure)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate() && Time.realtimeSinceStartup < deadline)
                await UniTask.NextFrame();
            Require(predicate(), failure);
        }

        private static void Require(bool condition, string failure)
        {
            if (!condition) throw new InvalidOperationException(failure);
        }
    }
}
#endif
