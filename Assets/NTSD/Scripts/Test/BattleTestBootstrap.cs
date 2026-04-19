using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using MoreMountains.TopDownEngine;
using NTSD.Simulation;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Tools;
using NTSD.App;
using Cysharp.Threading.Tasks;

namespace NTSD.Test
{
    /// <summary>
    /// 战斗场景测试启动器
    /// 直接打开 NTSD_Battle 场景时，跳过主菜单，自动完成数据加载和战斗初始化。
    /// 
    /// 使用方法：
    /// 1. 将此脚本挂到 NTSD_Battle 场景中任意 GameObject 上
    /// 2. 在 Inspector 中配置 PlayerPrefabs（可选，不配则使用 LevelManager 自带的）
    /// 3. 直接 Play NTSD_Battle 场景即可
    /// </summary>
    public class BattleTestBootstrap : MonoBehaviour
    {
        [Header("仅在没有 AppManager 时生效（直接打开战斗场景）")]
        [Tooltip("如果为空，则使用 MultiplayerLevelManager 上已配置的 PlayerPrefabs")]
        [SerializeField] private Character[] overridePlayerPrefabs;

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

        private Character firstPlayer;
        private LF2Character firstPlayerLf2;

        private async void Start()
        {
            if (App.AppManager.Instance != null)
            {
                Debug.Log("[BattleTestBootstrap] AppManager exists, skipping test bootstrap.");
                return;
            }

            Debug.Log("[BattleTestBootstrap] No AppManager detected, running test bootstrap...");

            // 创建 AppManager（含 NTSDSoundPlayer、SparkRenderer、EventSystem）
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

            // 6. 配置并启动关卡
            var levelMgr = FindObjectOfType<MultiplayerLevelManager>(true);
            if (levelMgr != null)
            {
                if (overridePlayerPrefabs != null && overridePlayerPrefabs.Length > 0)
                {
                    levelMgr.PlayerPrefabs = overridePlayerPrefabs;
                    Debug.Log($"[BattleTestBootstrap] Overriding PlayerPrefabs: {overridePlayerPrefabs.Length} characters.");
                }

                if (overrideCharacterIds != null && overrideCharacterIds.Length > 0)
                {
                    levelMgr.CharacterIdSelectionMode =
                        MultiplayerLevelManager.CharacterIdSelectionModes.OverrideByPlayerIndex;
                    levelMgr.OverrideCharacterIdsByPlayerIndex =
                        new System.Collections.Generic.List<int>(overrideCharacterIds);
                    Debug.Log($"[BattleTestBootstrap] Overriding CharacterIds: [{string.Join(", ", overrideCharacterIds)}]");
                }

                levelMgr.StartLevel();
                Debug.Log("[BattleTestBootstrap] LevelManager.StartLevel() called.");

                CacheFirstPlayer(levelMgr);
                EnsureForceModesExclusive();
            }
            else
            {
                Debug.LogError("[BattleTestBootstrap] MultiplayerLevelManager not found in scene!");
            }

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

        private void CacheFirstPlayer(MultiplayerLevelManager levelMgr)
        {
            if (firstPlayerLf2 != null) return;
            if (levelMgr == null || levelMgr.Players == null || levelMgr.Players.Count == 0) return;

            firstPlayer = levelMgr.Players[0];
            firstPlayerLf2 = firstPlayer?._LF2Character;

            if (firstPlayer != null && firstPlayerLf2 != null && logForceStateToggle)
            {
                Log.Info("[BattleTestBootstrap] First player cached: {0}", firstPlayer.name);
            }
        }

        private void ApplyForcedMovementState()
        {
            if (!forceWalkingMode && !forceRunningMode) return;

            if (firstPlayerLf2 == null)
            {
                CacheFirstPlayer(FindObjectOfType<MultiplayerLevelManager>(true));
                if (firstPlayerLf2 == null) return;
            }

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
