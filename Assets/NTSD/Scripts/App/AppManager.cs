using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Game;
using NTSD.Tools;
using NTSD.UI;
using NTSD.Simulation;
using MoreMountains.TopDownEngine;
using System.Collections;
using MoreMountains.Tools;

namespace NTSD.App
{
    public enum AppFlowState
    {
        MenuMain,
        MenuLoading,
        MenuSelectGameMode,
        MenuSelectCharacter,
        BattleLoading,
        BattleRunning,
        BattlePaused,
    }

    public sealed class AppManager : SingletonBehaviour<AppManager>
    {
        [Header("Scenes")]
        [SerializeField] private string menuSceneName = "NTSD_Menu";
        [SerializeField] private string battleSceneName = "NTSD_Battle";

        [Header("Menu")]
        [SerializeField] private MenuUIController menuUi;

        [Header("EventSystem (global unique)")]
        [SerializeField] private EventSystem eventSystem;

        [Header("Global Config")]
        [SerializeField] private GameConfig gameConfig;

        private NTSDSoundPlayer soundPlayer;
        private SparkRenderer sparkRenderer;
        private InputModule inputModule;

        [Header("Runtime")]
        [SerializeField] private AppFlowState state = AppFlowState.MenuMain;

        public AppFlowState State => state;
        public MatchConfig CurrentMatchConfig { get; private set; }
        public NTSDSoundPlayer SoundPlayer => soundPlayer;
        public SparkRenderer SparkRenderer => sparkRenderer;
        public InputModule InputModule => inputModule;

        protected override bool PersistAcrossScenes => true;

        protected override void OnSingletonAwake()
        {
            InitializeGameConfig();
            EnsureRuntimeModules();
            EnsureSingleEventSystem();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void EnsureRuntimeModules()
        {
            soundPlayer = gameObject.MMGetOrAddComponent<NTSDSoundPlayer>();
            sparkRenderer = gameObject.MMGetOrAddComponent<SparkRenderer>();
            inputModule = new InputModule();
        }

        private void InitializeGameConfig()
        {
            if (gameConfig != null)
            {
                GameConfig.Instance = gameConfig;
            }
            else
            {
                Debug.LogWarning("[AppManager] GameConfig is not assigned!");
            }
        }

        protected override void OnSingletonDestroyed()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == battleSceneName)
            {
                StartCoroutine(DelayedInitializeBattle(scene));
            }
        }

        private IEnumerator DelayedInitializeBattle(Scene scene)
        {
            yield return null;
            InitializeBattle(scene);
        }

        private void InitializeBattle(Scene scene)
        {
            InitializeBattleSingletons();

            var bootstrap = FindBattleBootstrap(scene);
            bootstrap?.EnablePresentation();

            SceneManager.SetActiveScene(scene);

            // Step 6: Use pool-based assembly instead of levelMgr.StartLevel()
            SetupBattleCharacters();

            SimulationTickDriver.Instance?.SetPaused(false);

            state = AppFlowState.BattleRunning;
            EnsureSingleEventSystem();
        }

        private void InitializeBattleSingletons()
        {
            NTSD.TimeWheel.TimeWheel.CreateSharedInstance();
        }

        private BattleBootstrap FindBattleBootstrap(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                var bootstrap = roots[i].GetComponentInChildren<BattleBootstrap>(true);
                if (bootstrap != null) return bootstrap;
            }
            return null;
        }

        private void EnsureSingleEventSystem()
        {
            if (eventSystem == null)
            {
                eventSystem = GetComponentInChildren<EventSystem>(true);
            }

            if (eventSystem == null)
            {
                var go = new GameObject("EventSystem");
                go.transform.SetParent(transform);
                eventSystem = go.AddComponent<EventSystem>();
                go.AddComponent<StandaloneInputModule>();
            }

            var all = FindObjectsOfType<EventSystem>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] != eventSystem)
                {
                    all[i].gameObject.SetActive(false);
                }
            }

            eventSystem.gameObject.SetActive(true);
        }

        private void SetupBattleCharacters()
        {
            if (CurrentMatchConfig == null) return;

            var levelMgr = FindObjectOfType<MultiplayerLevelManager>();
            if (levelMgr == null) return;

            var spawnPoints = levelMgr.SpawnPoints;

            for (int i = 0; i < CurrentMatchConfig.players.Count; i++)
            {
                var slot = CurrentMatchConfig.players[i];
                if (!slot.use) continue;

                var entityObj = LF2ObjectPool.Instance.Get(out LF2ObjectRenderer EntityModel);
                var lf2 = LF2ReferencePool.Instance.Get(LF2ObjectType.Character, slot.characterId) as LF2Character;

                lf2.Controller.SetInputID(slot.inputId);

                lf2.InjectDependencies(entityObj.transform, EntityModel.transform, $"Player_{slot.inputId}");

                EntityModel.SetLogicObject(lf2, null);

                var frameData = CharacterAnimtorManager.Instance.GetCharacterConfig(slot.characterId);
                lf2.ModuleBind(frameData, slot.characterId);

                Vector3 spawnPos = (spawnPoints != null && i < spawnPoints.Count)
                    ? spawnPoints[i].transform.position: Vector3.zero;
                
                float ppu = SimulationConstants.PIXELS_PER_UNIT;
                lf2.PS.x = spawnPos.x * ppu;
                lf2.PS.z = spawnPos.y * ppu; 
                lf2.PS.y = 0;
            }
        }

        public void SetMatchConfig(MatchConfig config)
        {
            CurrentMatchConfig = config;
        }

        public AsyncOperation LoadBattleAdditive()
        {
            state = AppFlowState.BattleLoading;

            if (menuUi == null)
            {
                menuUi = FindObjectOfType<MenuUIController>(true);
            }

            menuUi?.HideAll();
            menuUi?.EnableMenuUiCamera(false);

            return SceneManager.LoadSceneAsync(battleSceneName, LoadSceneMode.Additive);
        }

        public AsyncOperation UnloadBattle()
        {
            if (SimulationTickDriver.Instance != null)
            {
                SimulationTickDriver.Instance.SetPaused(true);
                SimulationTickDriver.Instance.UnbindWorld();
            }

            var scene = SceneManager.GetSceneByName(battleSceneName);
            if (scene.IsValid() && scene.isLoaded)
            {
                var bootstrap = FindBattleBootstrap(scene);
                bootstrap?.DisablePresentation();
            }

            NTSD.TimeWheel.TimeWheel.DestroySharedInstance();

            state = AppFlowState.MenuMain;
            var op = SceneManager.UnloadSceneAsync(battleSceneName);

            if (menuUi == null)
            {
                menuUi = FindObjectOfType<MenuUIController>(true);
            }

            if (op != null)
            {
                op.completed += _ =>
                {
                    menuUi?.EnableMenuUiCamera(true);
                    menuUi?.ShowMainMenu();
                    EnsureSingleEventSystem();
                };
            }

            return op;
        }

        public void SetMenuState(AppFlowState menuState)
        {
            state = menuState;
        }

        public void SetBattlePaused(bool paused)
        {
            state = paused ? AppFlowState.BattlePaused : AppFlowState.BattleRunning;
        }
    }
}
