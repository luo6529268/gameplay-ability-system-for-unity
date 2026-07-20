using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;
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

        [Header("Battle Lockstep")]
        [Tooltip("战斗帧同步底层配置。进入战斗前应用到 SimulationTickDriver。")]
        public LockstepSimulationSettings battleLockstepSettings = new LockstepSimulationSettings();

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

            SimulationTickDriver.Instance?.ApplyMatchConfig(CurrentMatchConfig);

            // Step 6: Use pool-based assembly instead of levelMgr.StartLevel()
            SetupBattleCharacters(scene);

            if (SimulationTickDriver.Instance != null)
            {
                SimulationTickDriver.Instance.ApplySettings(battleLockstepSettings);
                SimulationTickDriver.Instance.SetPaused(false);
            }

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

        private void SetupBattleCharacters(Scene battleScene)
        {
            if (CurrentMatchConfig == null) return;

            SimulationWorld world = SimulationTickDriver.Instance?.World;
            BattleRosterRuntimeState roster = world?.Runtime?.Roster;
            BattleSlotRuntimeState[] rosterSlots = roster?.Slots;
            if (world == null || rosterSlots == null)
                return;

            int slotCount = System.Math.Min(CurrentMatchConfig.players.Count, rosterSlots.Length);
            for (int i = 0; i < slotCount; i++)
            {
                var slot = CurrentMatchConfig.players[i];
                if (slot == null || !slot.use) continue;

                var frameData = CharacterAnimtorManager.Instance?.GetCharacterConfig(slot.characterId);
                if (frameData == null)
                {
                    Debug.LogWarning(
                        $"[AppManager] Character {slot.characterId} for battle slot {i} is not loaded in scene '{battleScene.name}'.");
                    continue;
                }

                BattleStageRuntimeState stage = world.Runtime?.Stage;
                int stageWidth = stage != null && stage.BaseStageWidthPx > 0
                    ? stage.BaseStageWidthPx
                    : 800;
                int zMin = stage != null && stage.ZMin > 0 ? stage.ZMin : 180;
                int zMax = stage != null && stage.ZMax > 0 ? stage.ZMax : 350;
                int xRange = stageWidth / 2;
                int zRange = zMax - zMin;
                double spawnX = stageWidth / 4 + (xRange > 0 ? world.Rng.NextRaw() % xRange : 0);
                int spawnZ = zMin + (zRange > 0 ? world.Rng.NextRaw() % zRange : 0);

                var entityObj = LF2ObjectPool.Instance.Get(out LF2ObjectRenderer EntityModel);
                var lf2 = LF2ReferencePool.Instance.Get(LF2ObjectType.Character, slot.characterId) as LF2Character;

                int inputId = BattleRosterRuntimeState.ResolveInputId(slot.inputId, i);
                int team = BattleRosterRuntimeState.ResolveBattleTeam(slot.team, i);

                lf2.Controller.SetInputID(inputId);

                lf2.InjectDependencies(entityObj.transform, EntityModel.transform, $"Player_{inputId}");
                lf2.ModuleInitialize();

                EntityModel.SetLogicObject(lf2, null);

                lf2.ModuleBind(frameData, slot.characterId);
                lf2.Initialize(NTSDGlobal.Default.Health.HpFull, NTSDGlobal.Default.Health.MpFull);
                lf2.Team = team;
                lf2.RelationTeam = team;
                lf2.AiControlled = !slot.isHuman;

                lf2.PS.x = spawnX;
                lf2.PS.y = 0;
                lf2.PS.z = spawnZ;
                lf2.PS.vx = 0.1;
                lf2.PS.vy = 0;
                lf2.PS.vz = 0.1;
                lf2.HitStun = 75;
                lf2.Runtime.SyncIntegerPosition();
                lf2.RefreshRuntimeSnapshot();

                BattleSlotRuntimeState rosterSlot = rosterSlots[i];
                if (rosterSlot != null && rosterSlot.Active)
                {
                    rosterSlot.RuntimeSlotIndex = lf2.Runtime.SlotIndex;
                    rosterSlot.StableId = lf2.Runtime.StableId;
                }
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
