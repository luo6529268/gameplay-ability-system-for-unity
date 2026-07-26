using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using MoreMountains.TopDownEngine;
using NTSD.Simulation;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;
using NTSD.Tools;
using NTSD.App;
using NTSD.Game;
using NTSD.LevelEditor;
using Cysharp.Threading.Tasks;

namespace NTSD.Test
{
    /// <summary>
    /// 战斗场景测试启动器
    /// 直接打开 NTSD_Battle 场景时，跳过主菜单，自动完成数据加载和战斗初始化。
    /// 
    /// 使用方法：
    /// 1. 将此脚本挂到 NTSD_Battle 场景中任意 GameObject 上
    /// 2. 在 Inspector 中配置 overrideCharacterIds（可选）
    /// 3. 直接 Play NTSD_Battle 场景即可
    /// </summary>
    public class BattleTestBootstrap : MonoBehaviour
    {
#if UNITY_EDITOR
        public static bool SuppressEntityCreationForProductionStress { get; set; }
        public static bool ProductionStressServicesReady { get; private set; }
#endif

        [Header("仅在没有 AppManager 时生效（直接打开战斗场景）")]
        [Tooltip("游戏全局配置（直接拖入 GameConfig.asset）")]
        [SerializeField] private App.GameConfig gameConfig;

        [Tooltip("覆盖角色ID列表（按玩家索引），-1 表示不覆盖")]
        [SerializeField] private int[] overrideCharacterIds;

        [Tooltip("初始化完成后自动取消暂停")]
        [SerializeField] private bool autoResume = true;

        [Tooltip("启动延迟帧数（等待场景内其他单例初始化）")]
        [SerializeField] private int delayFrames = 1;

        [Header("首玩家移动状态测试（F6/F7切换，互斥）")]
        [Tooltip("强制首玩家持续 Walking（F6 切换）")]
        [SerializeField] private bool forceWalkingMode = false;

        [Tooltip("强制首玩家持续 Running（F7 切换）")]
        [SerializeField] private bool forceRunningMode = false;

        [Tooltip("是否打印状态切换日志")]
        [SerializeField] private bool logForceStateToggle = true;

        private LF2Character firstPlayerLf2;

        private async void Start()
        {
#if UNITY_EDITOR
            if (SuppressEntityCreationForProductionStress)
                ProductionStressServicesReady = false;
#endif
            if (App.AppManager.Instance != null)
            {
#if UNITY_EDITOR
                if (SuppressEntityCreationForProductionStress)
                    ProductionStressServicesReady = true;
#endif
                Debug.Log("[BattleTestBootstrap] AppManager exists, skipping test bootstrap.");
                return;
            }

            Debug.Log("[BattleTestBootstrap] No AppManager detected, running test bootstrap...");

            // 创建 AppManager（含 NTSDSoundPlayer, SparkRenderer, EventSystem）
            EnsureAppManager();

            for (int i = 0; i < delayFrames; i++)
                await UniTask.Yield();
            if (this == null) return;

            // 1. 初始化 TimeWheel
            TimeWheel.TimeWheel.CreateSharedInstance();
            Debug.Log("[BattleTestBootstrap] TimeWheel created.");

            // 2. 初始化 GameConfig
            if (App.GameConfig.Instance == null)
            {
                var configs = Resources.FindObjectsOfTypeAll<App.GameConfig>();
                if (configs.Length > 0)
                {
                    App.GameConfig.Instance = configs[0];
                    Debug.Log($"[BattleTestBootstrap] GameConfig set from Resources: {configs[0].name}");
                }
                else
                {
                    Debug.LogWarning("[BattleTestBootstrap] No GameConfig found.");
                }
            }

            // 3. 加载角色数据
            SimulationTickDriver simulationDriver = SimulationTickDriver.Instance;
            if (simulationDriver == null)
            {
                Debug.LogError(
                    "[BattleTestBootstrap] SimulationTickDriver not found before runtime profile reconciliation.");
                return;
            }
            if (!simulationDriver.EnsureRuntimeProfileFromSources())
            {
                Debug.LogError(
                    "[BattleTestBootstrap] Runtime profile reconciliation failed; test bootstrap aborted before entity registration.");
                return;
            }

            await LoadCharacterDataAsync();
            if (this == null) return;

            // 4. 启用 BattleBootstrap 表现层
            var bootstrap = FindObjectOfType<App.BattleBootstrap>(true);
            if (bootstrap != null)
            {
                bootstrap.EnablePresentation();
                Debug.Log("[BattleTestBootstrap] BattleBootstrap presentation enabled.");
            }

            // 5. 设置当前场景为活动场景
            SceneManager.SetActiveScene(gameObject.scene);

#if UNITY_EDITOR
            if (SuppressEntityCreationForProductionStress)
            {
                ProductionStressServicesReady = true;
                Debug.Log(
                    "[BattleTestBootstrap] Production stress services are ready; test entities and auto-resume are suppressed.");
                return;
            }
#endif

            // 6. 配置并启动关卡
            var levelMgr = BoundaryWallManager.Instance;
            SetupTestCharacters(levelMgr, gameObject.scene);
            Debug.Log("[BattleTestBootstrap] SetupTestCharacters called.");

            // 7. 取消暂停
            if (autoResume)
            {
                await UniTask.Yield();
                if (this == null) return;

                if (SimulationTickDriver.Instance != null)
                {
                    SimulationTickDriver.Instance.SetPaused(false);
                    Debug.Log("[BattleTestBootstrap] SimulationTickDriver resumed.");
                }
                else
                {
                    Debug.LogError("[BattleTestBootstrap] SimulationTickDriver not found!");
                }
            }

            Debug.Log("[BattleTestBootstrap] === Test bootstrap complete ===");
        }

        private void Update()
        {
            HandleToggleInput();
            ApplyForcedMovementState();
        }

        private void HandleToggleInput()
        {
            var kb = Keyboard.current;
            if (kb == null) return;

            if (kb.f6Key.wasPressedThisFrame)
            {
                forceWalkingMode = !forceWalkingMode;
                if (forceWalkingMode)
                {
                    forceRunningMode = false;
                }
                if (logForceStateToggle)
                {
                    Log.Info("[BattleTestBootstrap] ForceWalking={0}, ForceRunning={1}", forceWalkingMode, forceRunningMode);
                }
            }

            if (kb.f7Key.wasPressedThisFrame)
            {
                forceRunningMode = !forceRunningMode;
                if (forceRunningMode)
                {
                    forceWalkingMode = false;
                }
                if (logForceStateToggle)
                {
                    Log.Info("[BattleTestBootstrap] ForceWalking={0}, ForceRunning={1}", forceWalkingMode, forceRunningMode);
                }
            }

            EnsureForceModesExclusive();
        }

        private void EnsureForceModesExclusive()
        {
            if (forceWalkingMode && forceRunningMode)
            {
                forceWalkingMode = false;
            }
        }

        private void SetupTestCharacters(BoundaryWallManager levelMgr, Scene battleScene)
        {
            var spawnPoints = levelMgr.ResolveSpawnPoints(battleScene);
            int count = overrideCharacterIds != null ? overrideCharacterIds.Length : 1;
            SimulationWorld world = SimulationTickDriver.Instance?.World;
            BattleRosterRuntimeState roster = world?.Runtime?.Roster;
            roster?.Reset();

            for (int i = 0; i < count; i++)
            {
                int characterId = (overrideCharacterIds != null && i < overrideCharacterIds.Length)
                    ? overrideCharacterIds[i]
                    : 0;

                if (characterId < 0) continue;

                var entityObj = LF2ObjectPool.Instance.Get(out LF2ObjectRenderer EntityModel);
                var lf2 = LF2ReferencePool.Instance.Get(LF2ObjectType.Character, characterId) as LF2Character;

                lf2.Controller.SetInputID(i + 1); // Test mode: player index as input id

                lf2.InjectDependencies(entityObj.transform, EntityModel.transform, $"TestPlayer_{i}");
                lf2.ModuleInitialize();

                EntityModel.SetLogicObject(lf2, null);

                var frameData = CharacterAnimtorManager.Instance.GetCharacterConfig(characterId);
                lf2.ModuleBind(frameData, characterId);
                lf2.Initialize(NTSDGlobal.Default.Health.HpFull, NTSDGlobal.Default.Health.MpFull);
                lf2.Team = i + 1;
                lf2.RelationTeam = i + 1;
                lf2.AiControlled = false;

                if (roster?.Slots != null && i < roster.Slots.Length)
                {
                    BattleSlotRuntimeState rosterSlot = roster.Slots[i];
                    rosterSlot.Active = true;
                    rosterSlot.IsHuman = true;
                    rosterSlot.CharacterId = characterId;
                    rosterSlot.Team = lf2.Team;
                    rosterSlot.InputId = i + 1;
                    rosterSlot.RuntimeSlotIndex = lf2.Runtime.SlotIndex;
                    rosterSlot.StableId = lf2.Runtime.StableId;
                    roster.ActiveSlotCount++;
                }

                Vector3 spawnPos;
                if (i < spawnPoints.Count)
                {
                    spawnPos = spawnPoints[i].transform.position;
                }
                else
                {
                    spawnPos = Vector3.zero;
                    Debug.LogWarning($"[BattleTestBootstrap] No spawn point for player index {i} in scene '{battleScene.name}'; using Vector3.zero.");
                }

                lf2.PS.x = NTSDRenderSpace.WorldToGroundPixel(spawnPos).x;
                lf2.PS.z = PhysicsState.UnityYToDepth(spawnPos.y);
                lf2.PS.y = 0;

                if (i == 0)
                {
                    firstPlayerLf2 = lf2;
                }
            }
        }

        private void ApplyForcedMovementState()
        {
            if (!forceWalkingMode && !forceRunningMode) return;
            if (firstPlayerLf2 == null) return;

            if (forceWalkingMode)
            {
                if (firstPlayerLf2.GetState() != LF2States.Walking)
                {
                    firstPlayerLf2.TransitionToFrame(LF2StandardFrames.WalkingStart, 20);
                }
            }
            else if (forceRunningMode)
            {
                if (firstPlayerLf2.GetState() != LF2States.Running)
                {
                    firstPlayerLf2.TransitionToFrame(LF2StandardFrames.RunningStart, 20);
                }
            }
        }

        private async UniTask LoadCharacterDataAsync()
        {
            var mgr = CharacterAnimtorManager.Instance;
            if (mgr == null)
            {
                Debug.LogError("[BattleTestBootstrap] CharacterAnimtorManager not found!");
                return;
            }

            if (mgr.IsPrewarmCompleted)
            {
                Debug.Log("[BattleTestBootstrap] Character data already loaded, skipping.");
                return;
            }

            Debug.Log("[BattleTestBootstrap] Loading character configs...");
            var dataManager = GameDataManager.Instance;
            var configs = await UniTask.RunOnThreadPool(() =>
                mgr.ParseCharacterFrameConfigs(dataManager, text =>
                    Debug.Log($"[BattleTestBootstrap] Parsing: {text}"))
            );
            mgr.ApplyLoadedCharacterConfigs(configs);
            Debug.Log("[BattleTestBootstrap] Character configs loaded.");

            Debug.Log("[BattleTestBootstrap] Loading character sprites...");
            await mgr.LoadCharacterSpritesAsync(text =>
                Debug.Log($"[BattleTestBootstrap] Loading sprite: {text}"));
            Debug.Log("[BattleTestBootstrap] Character sprites loaded.");
        }

        private void OnDestroy()
        {
            if (App.AppManager.Instance != null) return;
            TimeWheel.TimeWheel.DestroySharedInstance();
            Debug.Log("[BattleTestBootstrap] TimeWheel destroyed (test cleanup).");
        }

        private static void EnsureAppManager()
        {
            if (AppManager.Instance != null) return;
            var go = new GameObject("AppManager [TestBootstrap]");
            DontDestroyOnLoad(go);
            go.AddComponent<AppManager>();
            Debug.Log("[BattleTestBootstrap] AppManager created.");
        }
    }
}
