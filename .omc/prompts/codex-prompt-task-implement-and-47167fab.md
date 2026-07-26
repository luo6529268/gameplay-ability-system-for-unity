---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs"
  - "Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs"
  - "Assets/NTSD/Scripts/Animation/LF2Objects/LF2ReferencePool.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs"
  - "Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs"
timestamp: "2026-07-24T08:31:55.165Z"
---

--- File: Assets/NTSD/Scripts/Test/BattleTestBootstrap.cs ---
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
            if (App.AppManager.Instance != null)
            {
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


--- File: Assets/NTSD/Scripts/Animation/LF2ObjectPool.cs ---
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Tools;
using NTSD.App;
using Cysharp.Threading.Tasks;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2 对象池（MonoBehaviour 单例）
    /// 配置数据从 GameConfig.Instance 读取。
    /// </summary>
    public class LF2ObjectPool : MMSingleton<LF2ObjectPool>
    {
        [Header("父节点配置")]
        [SerializeField] private Transform _poolRoot;
        [SerializeField] private Transform _activeRoot;
        [SerializeField] private Transform _spriteRoot;

        // ========== 池数据结构 ==========
        private LinkedList<GameObject> _availableObjects;
        private HashSet<GameObject> _activeObjects;
        private Dictionary<GameObject, float> _releaseTimeMap;
        private float _lastCheckTime;

        private Stack<SpriteRenderer> _spritePool;
        private Material _spriteDefaultSharedMaterial;

        // ========== 配置快捷访问 ==========
        private static GameConfig Cfg => GameConfig.Instance;

        // 缓存 prefab 引用，避免懒加载时 GameConfig.Instance 为 null
        private GameObject _cachedLF2ObjectPrefab;

        // Read-only acceptance evidence; avoids editor tooling reflecting private pool state.
        public int AvailableObjectCountForAcceptance => _availableObjects?.Count ?? 0;
        public int ActiveObjectCountForAcceptance => _activeObjects?.Count ?? 0;

        // ========== 生命周期 ==========

        protected override void Awake()
        {
            base.Awake();
            NormalizeTransform(transform);

            _availableObjects = new LinkedList<GameObject>();
            _activeObjects = new HashSet<GameObject>();
            _releaseTimeMap = new Dictionary<GameObject, float>();
            _spritePool = new Stack<SpriteRenderer>(32);

            // 缓存 prefab 引用 - 延迟到 CreateNewObject 时再获取
            _cachedLF2ObjectPrefab = null;

            for (int i = 0; i < (Cfg?.PoolInitialSize ?? 0); i++)
                CreateNewObject();

            int spritePoolSize = Cfg?.PoolInitialSpritePoolSize ?? 16;
            for (int i = 0; i < spritePoolSize; i++)
            {
                var go = new GameObject("Spark");
                go.layer = LayerMask.NameToLayer("Battle");
                Transform parent = _spriteRoot != null ? _spriteRoot : transform;
                go.transform.SetParent(parent, false);
                var sr = go.AddComponent<SpriteRenderer>();
                CaptureOrApplySpriteDefaultMaterial(sr);
                LF2ObjectRenderer.NormalizeSpriteRendererState(sr, _spriteDefaultSharedMaterial);
                sr.sortingLayerName = "Object";
                sr.gameObject.SetActive(false);
                _spritePool.Push(sr);
            }
        }

        // ========== 核心方法 ==========

        /// <summary>
        /// 创建新对象：优先使用 Prefab，否则动态创建最小 GameObject。
        /// </summary>
        private LF2ObjectRenderer CreateNewObject()
        {
            if (_cachedLF2ObjectPrefab == null) _cachedLF2ObjectPrefab = Cfg?.LF2ObjectPrefab;

            GameObject go;
            if (_cachedLF2ObjectPrefab != null)
            {
                go = Instantiate(_cachedLF2ObjectPrefab, _poolRoot != null ? _poolRoot : this.transform);
                go.layer = LayerMask.NameToLayer("Battle");
            }
            else
            {
                go = new GameObject("LF2Object");
                go.layer = LayerMask.NameToLayer("Battle");
                go.SetActive(false);

                var entityModel = new GameObject("EntityModel");
                entityModel.layer = LayerMask.NameToLayer("Battle");
                entityModel.transform.SetParent(go.transform, false);
                LF2ObjectRenderer fallbackRenderer = entityModel.AddComponent<LF2ObjectRenderer>();
                BattleCentralPresentationMount entityMount =
                    entityModel.AddComponent<BattleCentralPresentationMount>();
                entityMount.ConfigureRuntimeFallback(
                    BattleCentralPresentationMountRole.EntityModel,
                    BattleCentralPresentationMountPurpose.EntitySprite,
                    fallbackRenderer);

                var shadow = new GameObject("Shadow");
                shadow.layer = LayerMask.NameToLayer("Battle");
                shadow.transform.SetParent(go.transform, false);
                BattleCentralPresentationMount shadowMount =
                    shadow.AddComponent<BattleCentralPresentationMount>();
                shadowMount.ConfigureRuntimeFallback(
                    BattleCentralPresentationMountRole.Shadow,
                    BattleCentralPresentationMountPurpose.CommonShadow,
                    fallbackRenderer);
            }

            NormalizeTransform(go.transform, resetScale: false);

            go.SetActive(false);

            // LF2ObjectRenderer 挂在子节点 EntityModel 上，不在根节点
            var r = go.GetComponentInChildren<LF2ObjectRenderer>(true);
            if (r == null)
            {
                Log.Error("[LF2ObjectPool] EntityModel missing LF2ObjectRenderer");
                Destroy(go);
                return null;
            }

            _availableObjects.AddLast(go);
            return r;
        }

        /// <summary>从池中获取对象（懒加载）</summary>
        public GameObject Get(out LF2ObjectRenderer EntityModel)
        {
            int maxPoolSize = Cfg?.PoolMaxSize ?? 200;

            GameObject go;
            EntityModel = null;
            if (_availableObjects.Count == 0)
            {
                if (_activeObjects.Count >= maxPoolSize)
                    Log.Warn("[LF2ObjectPool] Pool over limit: active={0}/{1}, expanding.", _activeObjects.Count, maxPoolSize);
                CreateNewObject();
                if (_availableObjects.Count == 0)
                {
                    Log.Error("[LF2ObjectPool] CreateNewObject failed (active={0})", _activeObjects.Count);
                    return null;
                }
            }

            go = _availableObjects.First.Value;
            _availableObjects.RemoveFirst();

            Transform activeParent = _activeRoot != null ? _activeRoot : this.transform;
            go.transform.SetParent(activeParent, false);
            NormalizeTransform(go.transform, resetScale: false);

            go.SetActive(true);
            _activeObjects.Add(go);
            EntityModel = go.GetComponentInChildren<LF2ObjectRenderer>(true);
            if (EntityModel != null)
            {
                // 回收时 EntityModel 子节点会被 ResetState 关闭，取出时必须显式恢复。
                EntityModel.gameObject.SetActive(true);
                EntityModel.RestorePooledVisualState();
            }
            return go;
        }

        /// <summary>
        /// 批量预热接口（对齐 C++ release SceneManager_Init 的 400 个实体实例预分配）。
        /// </summary>
        public async UniTask PrewarmAsync(int count)
        {
            for (int i = 0; i < count; i++)
            {
                CreateNewObject();
                // 每实例化 5 个对象让出一帧，确保 Loading 动画不卡顿
                if (i % 5 == 0) await UniTask.Yield();
            }
            Log.Info("[LF2ObjectPool] Bulk Prewarm: {0} GameObjects", count);
        }

        /// <summary>归还对象到池</summary>
        public void Release(LF2ObjectRenderer r)
        {
            if (r == null) return;

            r.ResetState();

            var go = r.transform.parent.gameObject;

            if (_poolRoot != null)
                go.transform.SetParent(_poolRoot, false);

            go.SetActive(false);
            _activeObjects.Remove(go);
            _availableObjects.AddLast(go);
            _releaseTimeMap[go] = Time.time;
        }

        // ========== 超时卸载 ==========

        private void Update()
        {
            int initialSize = Cfg?.PoolInitialSize ?? 0;
            float expireTime = Cfg?.PoolExpireTimeSeconds ?? 120f;
            float checkInterval = Cfg?.PoolCheckIntervalSeconds ?? 10f;

            if (_availableObjects.Count <= initialSize)
            {
                _releaseTimeMap.Clear();
                return;
            }

            if (Time.time - _lastCheckTime < checkInterval) return;
            _lastCheckTime = Time.time;

            var node = _availableObjects.First;
            while (node != null)
            {
                var next = node.Next;
                var obj = node.Value;

                if (_releaseTimeMap.TryGetValue(obj, out float t) &&
                    Time.time - t >= expireTime)
                {
                    _availableObjects.Remove(node);
                    _releaseTimeMap.Remove(obj);
                    Destroy(obj);

                    if (_availableObjects.Count <= initialSize)
                    {
                        _releaseTimeMap.Clear();
                        break;
                    }
                }

                node = next;
            }
        }

        // ========== Bucket B：SpriteRenderer 桶 ==========

        /// <summary>
        /// 从轻量 SpriteRenderer 桶取出一个 SpriteRenderer（懒加载）。
        /// 池空时创建新 GameObject 并挂载 SpriteRenderer，统一挂在 _spriteRoot 下（Inspector 指定，null 时挂在本对象上）。
        /// 取出后 SetActive(true)，不注册 SimulationWorld。
        /// </summary>
        public SpriteRenderer GetSprite()
        {
            SpriteRenderer sr;
            if (_spritePool.Count > 0)
            {
                sr = _spritePool.Pop();
            }
            else
            {
                var go = new GameObject("Spark");
                go.layer = LayerMask.NameToLayer("Battle");
                // 挂到场景根节点，避免父节点 inactive 导致无法显示
                Transform parent = _spriteRoot != null ? _spriteRoot : null;
                if (parent != null)
                    go.transform.SetParent(parent, false);
                sr = go.AddComponent<SpriteRenderer>();
                CaptureOrApplySpriteDefaultMaterial(sr);
                sr.sortingLayerName = "Object";
            }

            CaptureOrApplySpriteDefaultMaterial(sr);
            LF2ObjectRenderer.NormalizeSpriteRendererState(sr, _spriteDefaultSharedMaterial);
            sr.gameObject.SetActive(true);
            return sr;
        }

        /// <summary>
        /// 归还 SpriteRenderer 到轻量桶：清空 sprite，SetActive(false)，压栈。
        /// 防重复归还：已处于非激活状态则直接跳过。
        /// </summary>
        public void ReleaseSprite(SpriteRenderer sr)
        {
            if (sr == null) return;
            if (!sr.gameObject.activeSelf) return;  // 已归还过，防重复压栈
            sr.sprite = null;
            CaptureOrApplySpriteDefaultMaterial(sr);
            LF2ObjectRenderer.NormalizeSpriteRendererState(sr, _spriteDefaultSharedMaterial);
            sr.gameObject.SetActive(false);
            _spritePool.Push(sr);
        }

        public string GetPoolStatus() =>
            $"Available: {_availableObjects.Count}, Active: {_activeObjects.Count}";

        private static void NormalizeTransform(Transform target, bool resetScale = true)
        {
            if (target == null) return;
            target.localPosition = Vector3.zero;
            target.localRotation = Quaternion.identity;
            if (resetScale)
                target.localScale = Vector3.one;
        }

        private void CaptureOrApplySpriteDefaultMaterial(SpriteRenderer renderer)
        {
            if (renderer == null)
                return;
            if (_spriteDefaultSharedMaterial == null)
                _spriteDefaultSharedMaterial =
                    LF2ObjectRenderer.ResolveBorrowedDefaultSharedMaterial(renderer);
            else if (renderer.sharedMaterial != _spriteDefaultSharedMaterial)
                renderer.sharedMaterial = _spriteDefaultSharedMaterial;
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/LF2Objects/LF2ReferencePool.cs ---
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 逻辑对象引用池（纯 C# 对象池）
    /// 负责复用 LF2Weapon、LF2SpecialAttack 等逻辑层对象
    /// 避免频繁创建和 GC
    ///
    /// 与 LF2ObjectPool 的区别：
    /// - LF2ObjectPool: 管理 GameObject（LF2ObjectRenderer，实例对象池）
    /// - LF2ReferencePool: 管理纯 C# 对象（ILF2Object，引用池）
    /// </summary>
    public class LF2ReferencePool : MMSingleton<LF2ReferencePool>
    {
        // ========== 配置 ==========

        [Header("预热配置")]
        [SerializeField] private int _initialPoolSize = 50;

        // ========== 逻辑对象池（LF2LivingObject 子类，实现 ILF2Object）==========

        private Dictionary<LF2ObjectType, LinkedList<ILF2Object>> _availablePools;
        private HashSet<ILF2Object> _activeObjects;

        // ========== 初始化 ==========

        protected override void Awake()
        {
            base.Awake();

            EnsureLogicPoolsInitialized();

            PrewarmPool();
        }

        private void EnsureLogicPoolsInitialized()
        {
            _availablePools ??= new Dictionary<LF2ObjectType, LinkedList<ILF2Object>>();
            _activeObjects ??= new HashSet<ILF2Object>();

            if (!_availablePools.ContainsKey(LF2ObjectType.LightWeapon))
                _availablePools[LF2ObjectType.LightWeapon] = new LinkedList<ILF2Object>();
            if (!_availablePools.ContainsKey(LF2ObjectType.HeavyWeapon))
                _availablePools[LF2ObjectType.HeavyWeapon] = new LinkedList<ILF2Object>();
            if (!_availablePools.ContainsKey(LF2ObjectType.SpecialAttack))
                _availablePools[LF2ObjectType.SpecialAttack] = new LinkedList<ILF2Object>();
            if (!_availablePools.ContainsKey(LF2ObjectType.ThrowWeapon))
                _availablePools[LF2ObjectType.ThrowWeapon] = new LinkedList<ILF2Object>();
            if (!_availablePools.ContainsKey(LF2ObjectType.Drink))
                _availablePools[LF2ObjectType.Drink] = new LinkedList<ILF2Object>();
            if (!_availablePools.ContainsKey(LF2ObjectType.Character))
                _availablePools[LF2ObjectType.Character] = new LinkedList<ILF2Object>();
            if (!_availablePools.ContainsKey(LF2ObjectType.Other))
                _availablePools[LF2ObjectType.Other] = new LinkedList<ILF2Object>();
        }

        private void PrewarmPool()
        {
            for (int i = 0; i < _initialPoolSize / 3; i++)
                AddToPool(LF2ObjectType.LightWeapon);
            for (int i = 0; i < _initialPoolSize / 3; i++)
                AddToPool(LF2ObjectType.HeavyWeapon);
            for (int i = 0; i < _initialPoolSize / 3; i++)
                AddToPool(LF2ObjectType.SpecialAttack);
            for (int i = 0; i < _initialPoolSize / 6; i++)
                AddToPool(LF2ObjectType.ThrowWeapon);
            for (int i = 0; i < _initialPoolSize / 6; i++)
                AddToPool(LF2ObjectType.Other);
            
            // 角色逻辑对象也走同一引用池，便于战斗场景复用。
            for (int i = 0; i < 10; i++)
                AddToPool(LF2ObjectType.Character);

            Log.Info("[LF2ReferencePool] Prewarmed: {0} logic objects", _initialPoolSize + 10);
        }

        private void AddToPool(LF2ObjectType objectType)
        {
            var obj = CreateNewObject(objectType);
            if (obj != null && _availablePools.TryGetValue(objectType, out var pool))
                pool.AddLast(obj);
        }

        private ILF2Object CreateNewObject(LF2ObjectType objectType)
        {
            switch (objectType)
            {
                case LF2ObjectType.LightWeapon:
                    var lightWeapon = new LF2Weapon();
                    lightWeapon.SetWeaponType(1); // data.txt type=1 = 轻武器
                    return lightWeapon;
                case LF2ObjectType.HeavyWeapon:
                    var heavyWeapon = new LF2Weapon();
                    heavyWeapon.SetWeaponType(2); // data.txt type=2 = 重武器
                    return heavyWeapon;
                case LF2ObjectType.ThrowWeapon:
                    var throwWeapon = new LF2Weapon();
                    throwWeapon.SetWeaponType(4);
                    return throwWeapon;
                case LF2ObjectType.SpecialAttack:
                    return new LF2SpecialAttack();
                case LF2ObjectType.Drink:
                    var drinkWeapon = new LF2Weapon();
                    drinkWeapon.SetWeaponType(6);
                    return drinkWeapon;
                case LF2ObjectType.Character:
                    return new LF2Character();
                case LF2ObjectType.Other:
                    return new LF2OtherObject();
                default:
                    Log.Error("[LF2ReferencePool] Unsupported object type: {0}", objectType);
                    return null;
            }
        }

        // ========== 公共 API — 逻辑对象（ILF2Object）==========

        /// <summary>获取逻辑对象（LF2LivingObject 子类）</summary>
        public ILF2Object Get(LF2ObjectType objectType, int oid)
        {
            EnsureLogicPoolsInitialized();

            ILF2Object obj = null;

            if (_availablePools.TryGetValue(objectType, out var pool) && pool.Count > 0)
            {
                obj = pool.First.Value;
                pool.RemoveFirst();
            }
            else
            {
                obj = CreateNewObject(objectType);
            }

            if (obj != null)
            {
                obj.Reset();
                // Reset clears the runtime identity as part of pooled-object cleanup.
                // Assign the requested DAT identity only after reset so callers receive
                // the object id they requested (and renderer setup can resolve sprites).
                obj.ObjectId = oid;
                _activeObjects.Add(obj);
            }

            return obj;
        }

        /// <summary>归还逻辑对象到池中</summary>
        public void Release(ILF2Object obj)
        {
            if (obj == null) return;

            EnsureLogicPoolsInitialized();

            // Reset 已由调用方（OnTransitDestroy -> ResetState）执行，此处只做池 management
            if (!_activeObjects.Remove(obj))
                return;

            if (_availablePools.TryGetValue(obj.ObjectTypeEnum, out var pool))
                pool.AddLast(obj);
        }

        /// <summary>
        /// 批量预热接口，由战斗加载流程按需要调用。
        /// </summary>
        public void Prewarm(LF2ObjectType type, int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddToPool(type);
            }
            Log.Info("[LF2ReferencePool] Bulk Prewarm: {0} x {1}", type, count);
        }

        // ========== 查询 ==========

        public int ActiveCount => _activeObjects.Count;

        public int GetAvailableCount(LF2ObjectType objectType)
        {
            if (_availablePools.TryGetValue(objectType, out var pool))
                return pool.Count;
            return 0;
        }

        // ========== 通用引用池（ILF2Recyclable，按 Type 自动分桶）==========

        private Dictionary<System.Type, Stack<ILF2Recyclable>> _genericPool = new();

        public T Fetch<T>() where T : class, ILF2Recyclable, new()
        {
            var type = typeof(T);
            if (_genericPool.TryGetValue(type, out var stack) && stack.Count > 0)
            {
                var obj = (T)stack.Pop();
                obj.IsFromPool = true;
                return obj;
            }
            return new T { IsFromPool = true };
        }

        public void Recycle(ILF2Recyclable obj)
        {
            if (obj == null || !obj.IsFromPool) return;
            obj.IsFromPool = false;
            obj.Clear();
            var type = obj.GetType();
            if (!_genericPool.TryGetValue(type, out var stack))
                _genericPool[type] = stack = new Stack<ILF2Recyclable>();
            stack.Push(obj);
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs ---
﻿using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Presentation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Simulation
{
    public enum SimulationDriveMode
    {
        LocalFreeRun,
        LockstepBuffered,
        Manual
    }

    /// <summary>
    /// 战斗逻辑帧配置。
    /// 逻辑帧长度固定使用 SimulationConstants.SIM_DT；这里的配置只决定外层驱动、追帧和联机预留策略。
    /// </summary>
    [System.Serializable]
    public sealed class LockstepSimulationSettings
    {
        public const int LocalFreeRunMinCatchUpTicks = 4;

        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("单个 Unity 渲染帧最多追多少个逻辑帧。本地模式必须允许有限追帧，避免渲染帧率低于 30 FPS 时拖慢战斗时钟。")]
        public int maxCatchUpTicksPerFrame = LocalFreeRunMinCatchUpTicks;

        [Tooltip("最多保留多少个逻辑帧的时间积压，超过后丢弃外层积压但不改变单个逻辑帧步长。")]
        public int maxBacklogTicks = 8;

        [Tooltip("联机帧同步预留：本地输入写入未来第 N 帧。当前单机可保持 0。")]
        public int inputDelayTicks = 0;

        [Tooltip("联机帧同步预留：推进前是否要求该逻辑帧的输入已经准备好。")]
        public bool requireInputFrameReady = false;

        [Tooltip("在每个逻辑 tick 尾部生成 canonical battle snapshot 和分域 checksum。")]
        public bool enableFrameChecksum = false;

        public void Normalize()
        {
            int minimumCatchUp = driveMode == SimulationDriveMode.LocalFreeRun
                ? LocalFreeRunMinCatchUpTicks
                : 1;
            if (maxCatchUpTicksPerFrame < minimumCatchUp)
                maxCatchUpTicksPerFrame = minimumCatchUp;
            if (maxBacklogTicks < maxCatchUpTicksPerFrame) maxBacklogTicks = maxCatchUpTicksPerFrame;
            if (inputDelayTicks < 0) inputDelayTicks = 0;
        }
    }

    /// <summary>
    /// 逻辑帧输入源预留接口。
    /// 当前单机输入仍由角色自己的 SimInputBuffer 消费；后续联机可在这里接入输入收齐、预测、回滚和重放。
    /// </summary>
    public interface ISimulationFrameInputProvider
    {
        bool IsFrameInputReady(int tickIndex);
        FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
        void BeforeSimTick(int tickIndex) { }
        void AfterSimTick(int tickIndex) { }
        void Reset() { }
    }

    public sealed class LocalSimulationFrameInputProvider : ISimulationFrameInputProvider
    {
        public bool IsFrameInputReady(int tickIndex) => true;
        public FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
    }

    /// <summary>
    /// 战斗场景模拟时钟。
    /// 负责固定 30Hz 逻辑 tick，并把 C# 权威工程的 pass 顺序交给 NTSDBattleTickSystem。
    /// Unity 的 Update/LateUpdate 只作为外层驱动和表现刷新；战斗逻辑内部不能依赖 deltaTime。
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        [Tooltip("记录每个模拟 tick 的开始和结束。")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("启动时暂停，直到 BattleBootstrap 恢复模拟。")]
        [SerializeField] private bool startPaused = true;

        [Header("帧同步时钟")]
        [SerializeField] private LockstepSimulationSettings lockstepSettings = new LockstepSimulationSettings();

        [Header("调试信息（只读）")]
        [SerializeField][MMReadOnly] private int currentTickIndex = 0;
        [SerializeField][MMReadOnly] private float timeAccumulator = 0f;
        [SerializeField][MMReadOnly] private int objectCount = 0;
        [SerializeField][MMReadOnly] private bool paused = true;
        [SerializeField][MMReadOnly] private float renderAlpha = 0f;
        [SerializeField][MMReadOnly] private int backlogTickCount = 0;
        [SerializeField][MMReadOnly] private string lastFrameChecksum = string.Empty;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;
        private NTSD.Animation.BattleEntityOverlayRenderer _overlayRenderer;
        private BattlePresentationBackendMode _presentationBackendMode =
            BattlePresentationBackendMode.CentralOnly;

        private int _sparkRenderFrame = 0;
        private ISimulationFrameInputProvider _frameInputProvider = new LocalSimulationFrameInputProvider();
        private FrameInputSet _lastAppliedFrameInput = FrameInputSet.Empty(0);
        private BattleParityFrameSnapshot _lastFrameSnapshot;
        private IBattleChecksumSnapshot _lastChecksumSnapshot;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;
            lockstepSettings ??= new LockstepSimulationSettings();
            lockstepSettings.Normalize();

            CreateProductionWorld();

            Log.Info($"[SimulationTickDriver] Awake. paused={paused}, World created");
        }

        private void Update()
        {
            if (paused || _world == null || lockstepSettings.driveMode == SimulationDriveMode.Manual)
            {
                RefreshInspectorState();
                return;
            }

            float delta = lockstepSettings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timeAccumulator += delta;

            int maxBacklogTicks = Mathf.Max(lockstepSettings.maxBacklogTicks, lockstepSettings.maxCatchUpTicksPerFrame);
            float maxAccumulator = SimulationConstants.SIM_DT * maxBacklogTicks;
            if (_timeAccumulator > maxAccumulator)
                _timeAccumulator = maxAccumulator;

            int catchUpTicks = 0;
            while (_timeAccumulator >= SimulationConstants.SIM_DT &&
                   catchUpTicks < lockstepSettings.maxCatchUpTicksPerFrame)
            {
                int nextTickIndex = _tickIndex + 1;
                if (!CanAdvanceTick(nextTickIndex))
                    break;

                _timeAccumulator -= SimulationConstants.SIM_DT;
                StepOneTickInternal(nextTickIndex);
                catchUpTicks++;
            }

            RefreshInspectorState();
        }

        private void FixedUpdate()
        {
            // 帧同步逻辑不依赖 Unity FixedUpdate。Unity 物理循环只作为引擎外层回调存在。
        }

        private void LateUpdate()
        {
            if (_overlayRenderer == null)
                _overlayRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.BattleEntityOverlayRenderer>();
            _overlayRenderer.RenderAll(_world);

            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                    _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
            }

            _sparkRenderer.RenderAll(_world);
            _world?.BattlePresentation.FinalizePublishedHitRecordCycle(_world);
        }

        private bool CanAdvanceTick(int tickIndex)
        {
            if (lockstepSettings.driveMode != SimulationDriveMode.LockstepBuffered &&
                !lockstepSettings.requireInputFrameReady)
            {
                return true;
            }

            return _frameInputProvider == null || _frameInputProvider.IsFrameInputReady(tickIndex);
        }

        private bool StepOneTickInternal(int tickIndex)
        {
            if (_world == null || !CanAdvanceTick(tickIndex))
                return false;

            _tickIndex = tickIndex;
            _sparkRenderFrame = tickIndex;
            if (_world.Runtime?.Flow != null)
            {
                _world.Runtime.Flow.SparkRenderFrame = _sparkRenderFrame;
            }

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");

            _frameInputProvider?.BeforeSimTick(tickIndex);
            FrameInputSet frameInput = _frameInputProvider?.GetFrameInput(tickIndex) ??
                                       FrameInputSet.Empty(tickIndex);
            if (frameInput.TickIndex != tickIndex)
                frameInput = FrameInputSet.Empty(tickIndex);

            _lastAppliedFrameInput = frameInput;
            _world.ApplyFrameInputSet(frameInput);
            _battleTickSystem?.RunReleaseTick(tickIndex);
            CaptureFrameChecksumIfNeeded(tickIndex, frameInput);
            _frameInputProvider?.AfterSimTick(tickIndex);

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");

            return true;
        }

        private void CaptureFrameChecksumIfNeeded(int tickIndex, FrameInputSet frameInput)
        {
            if (!lockstepSettings.enableFrameChecksum)
            {
                _lastFrameSnapshot = null;
                _lastChecksumSnapshot = null;
                lastFrameChecksum = string.Empty;
                return;
            }

            _lastChecksumSnapshot = CaptureSupportedChecksumSnapshot(_world, tickIndex, frameInput);
            _lastFrameSnapshot = _lastChecksumSnapshot as BattleParityFrameSnapshot;
            lastFrameChecksum = _lastChecksumSnapshot?.OverallChecksum ?? string.Empty;
        }

        internal static bool SupportsAuthorityFrameChecksum(SimulationWorld world)
        {
            return world != null &&
                   world.RuntimeProfileForServices == BattleRuntimeProfile.Authority400 &&
                   world.MaxRuntimeSlotsForServices == SimulationWorld.AuthorityRuntimeSlotCapacity;
        }

        internal static BattleParityFrameSnapshot CaptureSupportedFrameSnapshot(
            SimulationWorld world,
            int tickIndex,
            FrameInputSet frameInput)
        {
            return SupportsAuthorityFrameChecksum(world)
                ? world.CaptureParityFrameSnapshot(tickIndex, frameInput)
                : null;
        }

        internal static bool SupportsFrameChecksum(SimulationWorld world)
        {
            if (world == null)
                return false;

            return SupportsAuthorityFrameChecksum(world) ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended;
        }

        internal static IBattleChecksumSnapshot CaptureSupportedChecksumSnapshot(
            SimulationWorld world,
            int tickIndex,
            FrameInputSet frameInput)
        {
            if (world == null)
                return null;

            if (SupportsAuthorityFrameChecksum(world))
                return world.CaptureParityFrameSnapshot(tickIndex, frameInput);

            return world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended
                ? world.CaptureExtendedChecksumSnapshot(tickIndex, frameInput)
                : null;
        }

        private void RefreshInspectorState()
        {
            currentTickIndex = _tickIndex;
            timeAccumulator = _timeAccumulator;
            objectCount = _world?.ObjectCount ?? 0;
            renderAlpha = Mathf.Clamp01(_timeAccumulator / SimulationConstants.SIM_DT);
            backlogTickCount = Mathf.FloorToInt(_timeAccumulator / SimulationConstants.SIM_DT);
        }

        public SimulationWorld World => _world;
        public int SparkRenderFrame => _sparkRenderFrame;
        public int CurrentTickIndex => _tickIndex;
        public FrameInputSet LastAppliedFrameInput => _lastAppliedFrameInput;
        public BattleParityFrameSnapshot LastFrameSnapshot => _lastFrameSnapshot;
        public IBattleChecksumSnapshot LastChecksumSnapshot => _lastChecksumSnapshot;
        public bool HasFrameChecksum => _lastChecksumSnapshot != null;
        public string LastFrameChecksum => lastFrameChecksum;
        public BattlePresentationBackendMode PresentationBackendMode => _presentationBackendMode;

        public float RemainingAccumulatorTime => _timeAccumulator;
        public float RenderAlpha => renderAlpha;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        public void ApplySettings(LockstepSimulationSettings settings)
        {
            if (settings == null)
                return;

            lockstepSettings = settings;
            lockstepSettings.Normalize();
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            if (!EnsureRuntimeProfileFromSources())
                return;

            _world.ResetRuntimeState();

            BattleMatchRuntimeState matchState = _world.Runtime?.Match;
            if (matchState != null)
            {
                matchState.LocalGameModeId = config?.gameMode?.gameModeId ?? 0;
                matchState.BattleGameModeId = config?.gameMode?.battleGameModeId ?? 1;
                matchState.BackgroundId = config?.backgroundId ?? -1;
                matchState.Difficulty = config?.difficulty ?? 2;
                matchState.Seed = config?.seed ?? 0;
            }

            _world.Rng?.Seed((uint)(config?.seed ?? 0));
            _world.Runtime?.Roster?.ApplyMatchConfig(config);
            _world.Runtime?.ApplyBootstrapFromMatchConfig(config);
            _world.SetNeedClearInput(true);
            _world.RefreshStageRuntimeSnapshotFromScene();

            List<BattleStageCampaignData> stageCampaigns = BattleStageCampaignLoader.LoadFromFile(
                config?.stageCampaignFilePath);
            _world.ConfigureStageCampaigns(stageCampaigns, config?.stageSeriesId ?? 0, -1);

            _world.SetAiPhaseGate(matchState != null && matchState.BattleGameModeId == 2 ? 1 : 0);
        }

        public void SetFrameInputProvider(ISimulationFrameInputProvider provider)
        {
            _frameInputProvider = provider ?? new LocalSimulationFrameInputProvider();
            _frameInputProvider.Reset();
            _lastAppliedFrameInput = FrameInputSet.Empty(_tickIndex);
        }

        public bool StepOneTick(bool ignorePaused = false)
        {
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(_tickIndex + 1);
            RefreshInspectorState();
            return stepped;
        }

        public void UnbindWorld()
        {
            if (_world != null)
                BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            CreateProductionWorld();
            _tickIndex = 0;
            _timeAccumulator = 0f;
            _sparkRenderFrame = 0;
            _lastAppliedFrameInput = FrameInputSet.Empty(0);
            _lastFrameSnapshot = null;
            _lastChecksumSnapshot = null;
            lastFrameChecksum = string.Empty;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

        private void CreateProductionWorld()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            CreateProductionWorld(settings, presentationMode);
        }

        private void CreateProductionWorld(
            BattleRuntimeWorldSettings settings,
            BattlePresentationBackendMode presentationMode)
        {
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            var nextWorld = new SimulationWorld(
                settings.Profile,
                settings.InitialRuntimeSlotCapacity,
                settings.CollisionBroadphase);
            nextWorld.SetBattlePresentationBackend(presentationMode);
            if (_world != null)
                BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = nextWorld;
            _presentationBackendMode = presentationMode;
            _battleTickSystem = new NTSDBattleTickSystem(_world);
        }

        internal bool EnsureRuntimeProfileFromSources()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            if (WorldMatchesRuntimeSettings(_world, settings))
            {
                _presentationBackendMode = presentationMode;
                _world.SetBattlePresentationBackend(presentationMode);
                return true;
            }

            if (_world != null &&
                (_world.ClaimedRuntimeSlotCountForServices > 0 || _world.ObjectCount > 0))
            {
                Debug.LogError(
                    $"[SimulationTickDriver] Runtime profile change rejected while entities are registered. " +
                    $"Current={_world.RuntimeProfileForServices}/{_world.MaxRuntimeSlotsForServices}, " +
                    $"Requested={settings.Profile}/{settings.InitialRuntimeSlotCapacity}");
                return false;
            }

            CreateProductionWorld(settings, presentationMode);
            return true;
        }

        internal static bool WorldMatchesRuntimeSettings(
            SimulationWorld world,
            BattleRuntimeWorldSettings settings)
        {
            if (world == null || world.RuntimeProfileForServices != settings.Profile)
                return false;

            if (world.CollisionBroadphaseForServices != settings.CollisionBroadphase)
                return false;

            return world.MaxRuntimeSlotsForServices == settings.InitialRuntimeSlotCapacity ||
                   (settings.Profile == BattleRuntimeProfile.DesktopExtended &&
                    world.MaxRuntimeSlotsForServices > settings.InitialRuntimeSlotCapacity);
        }

        protected override void OnSingletonDestroyed()
        {
            BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = null;
            _battleTickSystem = null;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/SimulationWorld.Registry.partial.cs ---
﻿using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 注册、运行时槽位和基础上下文。
    /// </summary>
    public partial class SimulationWorld
    {
        /// <summary>同一 SimOrder 的对象桶；只有桶内容变化后才延迟重新排序。</summary>
        private class Bucket
        {
            public List<ISimObject> items = new List<ISimObject>();
            public bool dirty = false;

            public void EnsureSorted(System.Func<ISimObject, int> stableIdSelector)
            {
                if (dirty)
                {
                    items = items.OrderBy(stableIdSelector).ToList();
                    dirty = false;
                }
            }
        }

        /// <summary>按 SimOrder 建立的模拟桶，SortedDictionary 保证 pass 顺序。</summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();
        /// <summary>注册对象时注入的模拟上下文。</summary>
        private SimContext _context;
        /// <summary>给没有显式运行时 ID 的对象自动分配 StableId。</summary>
        private int _nextAutoStableId = 100;
        internal const int AuthorityRuntimeSlotCapacity =
            BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity;
        private const int DynamicRuntimeSlotStart = 50;
        private readonly BattleRuntimeProfile activeRuntimeProfile;
        private readonly RuntimeSlotTable _runtimeSlots;
        private readonly RuntimeRestStore _runtimeRestStore;
        private readonly int maxActiveRuntimeEntities;
        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();
        private readonly List<LF2Entity> _pendingSlotReleasedDestroy = new List<LF2Entity>();
        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking = false;
        private readonly List<LF2Entity> _entityScratch = new List<LF2Entity>(128);
        private int _cameraX;
        private int _cameraVel;

        public int ReleaseCameraX => _cameraX;
        internal bool IsUnityFixedWorldCameraStateClear => _cameraX == 0 && _cameraVel == 0;
        internal int RuntimeSlotCapacity => _runtimeSlots.LogicalCapacity;
        internal int MaxRuntimeSlotsForServices => RuntimeSlotCapacity;
        internal int DynamicRuntimeSlotStartForServices => DynamicRuntimeSlotStart;
        internal BattleRuntimeProfile RuntimeProfileForServices => activeRuntimeProfile;
        internal CollisionBroadphaseBackend CollisionBroadphaseForServices { get; }
        internal int ClaimedRuntimeSlotCountForServices => _runtimeSlots.ClaimedCount;
        public int ClaimedRuntimeSlotCountForDiagnostics => _runtimeSlots.ClaimedCount;
        internal RuntimeRestStore RuntimeRestStoreForServices => _runtimeRestStore;

        private int GetRuntimeStableId(ISimObject obj)
        {
            return obj is LF2Entity entity ? entity.Runtime.StableId : obj.StableId;
        }

        private int GetRuntimeSlotOrder(LF2Entity entity)
        {
            if (entity == null) return int.MaxValue;
            int slot = entity.Runtime?.SlotIndex ?? -1;
            return slot >= 0 ? slot : entity.StableId;
        }

        private int CompareRuntimeSlotOrder(LF2Entity a, LF2Entity b)
        {
            int cmp = GetRuntimeSlotOrder(a).CompareTo(GetRuntimeSlotOrder(b));
            if (cmp != 0) return cmp;
            return (a?.StableId ?? int.MaxValue).CompareTo(b?.StableId ?? int.MaxValue);
        }

        private void RefreshRuntimeSnapshot(ISimObject obj)
        {
            if (obj is LF2Entity entity)
                entity.RefreshRuntimeSnapshot();
        }

        private List<int> GetBucketKeySnapshot()
        {
            return _buckets.Count > 0 ? new List<int>(_buckets.Keys) : null;
        }

        public ILF2SceneQuery SceneQuery { get; private set; }
        public INTSDItrKindService ItrKindService { get; private set; }
        public DeterministicRng Rng { get; private set; }
        public BattleRuntimeState Runtime { get; private set; }
        public int[] KillStats => Runtime.KillStats;
        public int[] DamageStats => Runtime.DamageStats;

        public SimulationWorld()
            : this(BattleRuntimeProfile.Authority400, AuthorityRuntimeSlotCapacity)
        {
        }

        internal SimulationWorld(
            BattleRuntimeProfile runtimeProfile,
            int runtimeSlotCapacity,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce)
        {
            if (runtimeSlotCapacity < DynamicRuntimeSlotStart)
                throw new System.ArgumentOutOfRangeException(nameof(runtimeSlotCapacity),
                    "Runtime slot capacity must include the dynamic slot band.");
            if (runtimeProfile == BattleRuntimeProfile.Authority400 &&
                runtimeSlotCapacity != AuthorityRuntimeSlotCapacity)
            {
                throw new System.ArgumentException(
                    "Authority400 worlds must use exactly 400 runtime slots.",
                    nameof(runtimeSlotCapacity));
            }

            activeRuntimeProfile = runtimeProfile;
            CollisionBroadphaseForServices = collisionBroadphase;
            maxActiveRuntimeEntities = runtimeProfile == BattleRuntimeProfile.MobileExtended
                ? BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities
                : int.MaxValue;
            _runtimeSlots = new RuntimeSlotTable(runtimeSlotCapacity, 20, DynamicRuntimeSlotStart);
            _runtimeRestStore = new RuntimeRestStore(runtimeSlotCapacity);
            aiInputSlots = new LF2Entity[runtimeSlotCapacity];
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this, collisionBroadphase);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
        }

        internal NTSDEntityRuntime GetRawRuntimeSlotState(int runtimeSlot)
        {
            return _runtimeSlots.GetRawRuntime(runtimeSlot);
        }

        internal bool TryGetCurrentRuntimeHandle(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return _runtimeSlots.TryGetCurrentHandle(runtimeSlot, expectedEntity, out handle);
        }

        internal bool TryResolveRuntimeHandle(RuntimeEntityHandle handle, out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        public bool TryGetCurrentRuntimeHandleForDiagnostics(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return _runtimeSlots.TryGetCurrentHandle(runtimeSlot, expectedEntity, out handle);
        }

        public bool TryResolveRuntimeHandleForDiagnostics(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        internal bool TryGetRuntimeSlotReadOnlyView(
            int runtimeSlot,
            out RuntimeSlotTable.ReadOnlySlotView view)
        {
            if (!_runtimeSlots.IsAddressable(runtimeSlot))
            {
                view = default;
                return false;
            }

            view = _runtimeSlots.GetReadOnlyView(runtimeSlot);
            return true;
        }

        private void ResetRawRuntimeSlotState(int runtimeSlot)
        {
            GetRawRuntimeSlotState(runtimeSlot)?.Reset();
        }

        public void ResetRuntimeState()
        {
            _battlePresentation.Reset();
            ResetRegisteredObjects();

            Runtime ??= new BattleRuntimeState();
            Runtime.Reset();
            // Unity lockstep owns one deterministic stream per SimulationWorld. The
            // explicit reset seed is an adapter boundary: it makes a world reset
            // replayable without sharing RNG state between independent Unity worlds.
            // It must remain distinct from MatchConfig.seed, which is applied by the
            // simulation driver at the formal battle-bootstrap boundary.
            Rng?.Seed(0x4E545344u);
            PendingSounds.Clear();
            _cameraX = 0;
            _cameraVel = 0;
            _nextAutoStableId = 100;
        }

        private void ResetRegisteredObjects()
        {
            (SceneQuery as BruteForceSceneQuery)?.ResetFormalSpatialBroadphase();

            var registeredObjects = new HashSet<ISimObject>();
            List<int> bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys != null)
            {
                for (int keyIndex = 0; keyIndex < bucketKeys.Count; keyIndex++)
                {
                    int key = bucketKeys[keyIndex];
                    if (!_buckets.TryGetValue(key, out Bucket bucket))
                        continue;

                    for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                    {
                        ISimObject item = bucket.items[itemIndex];
                        if (item != null)
                            registeredObjects.Add(item);
                    }
                }
            }

            _ticking = false;
            _pendingUnregister.Clear();
            _pendingSlotReleasedDestroy.Clear();
            _entityScratch.Clear();

            foreach (ISimObject item in registeredObjects)
            {
                item.OnRemoved(_context);
                if (item is not LF2Entity entity)
                    continue;

                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                    entity.Renderer);
                entity.ItrRest?.Unbind(false);
                entity.ItrRest?.Reset();
                entity.Reset();
                entity.Runtime?.Reset();
                entity.SetRuntimeSlotIndex(-1);
                entity.ClearRequiredRuntimeSlot();
                entity.FrameCache?.Clear();
                if (entity.Frame != null)
                {
                    entity.Frame.PN = 0;
                    entity.Frame.Prev = 0;
                    entity.Frame.N = 0;
                    entity.Frame.D = null;
                    entity.Frame.Prev2 = 0;
                    entity.Frame.Prev2D = null;
                }

                entity.Trans?.Reset();
                entity.Effect?.Reset();
                entity.Sprite?.SetPresentationSuppressed(true);
                entity.Sprite?.Hide();
                entity.Sprite?.HideShadow();
            }

            _buckets.Clear();
            _runtimeSlots.Reset();
            _runtimeRestStore.ResetWorld();
        }

        public int CurrentTickIndex => Runtime?.Flow?.CurrentTickIndex ?? 0;
        public int SparkRenderFrame => Runtime?.Flow?.SparkRenderFrame ?? 0;
        public int BattleGameModeId => Runtime?.Match?.BattleGameModeId ?? 0;
        public int LocalGameModeId => Runtime?.Match?.LocalGameModeId ?? 0;
        public int Difficulty => Runtime?.Match?.Difficulty ?? 2;
        public int BackgroundId => Runtime?.Match?.BackgroundId ?? -1;
        public int MatchSeed => Runtime?.Match?.Seed ?? 0;
        public int AiPhaseGate => Runtime?.Flow?.AiPhaseGate ?? 0;
        public int InputPhase => Runtime?.Flow?.InputPhase ?? 0;
        public int FrameMod12 => Runtime?.Flow?.FrameMod12 ?? 0;
        public int FrameToggle => Runtime?.Flow?.FrameToggle ?? 0;
        public int BattleExitCountdown => Runtime?.Flow?.BattleExitCountdown ?? 0;
        public int RouteOutRequest => Runtime?.Flow?.RouteOutRequest ?? 0;
        public int Mode2Request => Runtime?.Flow?.Mode2Request ?? 0;
        public bool NeedClearInput => Runtime?.Flow?.NeedClearInput ?? false;
        public List<BattleStageCampaignData> StageCampaigns => Runtime?.StageCampaigns;
        public BattleStageProgressionState StageProgression => Runtime?.StageProgression;
        public bool StageProgressionValid => Runtime?.StageProgressionValid ?? false;
        public int StageSpawnWaveApplied => Runtime?.StageSpawnWaveApplied ?? -1;
        public int StageSpawnWaveDeferredEntryApplied => Runtime?.StageSpawnWaveDeferredEntryApplied ?? -1;
        public int StageSpawnRuntimeWave => Runtime?.StageSpawnRuntimeWave ?? -1;
        public List<int> StageSpawnRuntimeTargetTotal => Runtime?.StageSpawnRuntimeTargetTotal;
        public List<int> StageSpawnRuntimeEntryCount => Runtime?.StageSpawnRuntimeEntryCount;
        public List<int> StageSpawnRuntimeSpawnedTotal => Runtime?.StageSpawnRuntimeSpawnedTotal;
        public List<int[]> StageSpawnRuntimeSlots => Runtime?.StageSpawnRuntimeSlots;

        public void SetAiPhaseGate(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.AiPhaseGate = value;
        }

        public void SetBattleExitCountdown(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.BattleExitCountdown = value;
        }

        public void SetRouteOutRequest(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.RouteOutRequest = value;
        }

        public void SetMode2Request(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.Mode2Request = value;
        }

        public void SetNeedClearInput(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.NeedClearInput = value;
        }

        public void AdvanceBattleFlowTick(int tickIndex)
        {
            if (Runtime?.Flow == null)
                return;

            Runtime.Flow.CurrentTickIndex = tickIndex;
            Runtime.Flow.InputPhase = (Runtime.Flow.InputPhase + 1) & 1;
            Runtime.Flow.FrameMod12 = tickIndex % 12;
            Runtime.Flow.FrameToggle = 1 - Runtime.Flow.FrameToggle;
        }

        public void SetStageProgressionValid(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageProgressionValid = value;
        }

        public void SetStageSpawnWaveApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveApplied = value;
        }

        public void SetStageSpawnWaveDeferredEntryApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveDeferredEntryApplied = value;
        }

        public void SetStageSpawnRuntimeWave(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnRuntimeWave = value;
        }

        public void Register(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

            // A pooled instance can be reused during the same dynamic late-slot scan.
            // Finalize its queued old lifecycle before registering the new one, and
            // remove the pending entry so the pass-finally flush cannot delete it.
            if (_pendingUnregister.Remove(obj))
                UnregisterImmediate(obj);

            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                bucket = new Bucket();
                _buckets[simOrder] = bucket;
            }

            if (bucket.items.Contains(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object already registered: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            if (obj is LF2Entity registeredEntity)
            {
                _pendingSlotReleasedDestroy.Remove(registeredEntity);
                registeredEntity.ItrRest?.Unbind(false);
                int runtimeSlot = AllocateRuntimeSlot(registeredEntity);
                registeredEntity.SetRuntimeSlotIndex(runtimeSlot);
                registeredEntity.ClearRequiredRuntimeSlot();
                if (runtimeSlot < 0)
                {
                    if (bucket.items.Count == 0)
                        _buckets.Remove(simOrder);
                    Debug.LogWarning(
                        $"[SimulationWorld] Runtime slot exhausted; registration rejected: " +
                        $"StableId={registeredEntity.StableId}, Type={registeredEntity.GetType().Name}");
                    return;
                }

                ResetRawRuntimeSlotState(runtimeSlot);
                if (registeredEntity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
                {
                    if (!ResetCooldownsForRuntimeSlot(runtimeSlot, registeredEntity))
                    {
                        RollbackRuntimeSlotRegistration(registeredEntity, runtimeSlot);
                        if (bucket.items.Count == 0)
                            _buckets.Remove(simOrder);
                        Debug.LogError(
                            $"[SimulationWorld] Runtime rest bind failed; registration rejected: " +
                            $"Slot={runtimeSlot}, StableId={registeredEntity.StableId}, " +
                            $"Type={registeredEntity.GetType().Name}");
                        return;
                    }
                }

                if (!registeredEntity.ShouldDeferInitialRuntimeSnapshot())
                    registeredEntity.RefreshRuntimeSnapshot();
            }

            bucket.items.Add(obj);
            bucket.dirty = true;
            obj.OnAdded(_context);
            if (obj is LF2Entity addedEntity &&
                TryGetCurrentRuntimeHandle(
                    addedEntity.Runtime.SlotIndex,
                    addedEntity,
                    out RuntimeEntityHandle runtimeHandle))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                    addedEntity.Renderer,
                    runtimeHandle);
            }
            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (_ticking)
            {
                if (obj is LF2Entity pendingEntity &&
                    !ReleaseRuntimeSlotAndClearPresentationBinding(pendingEntity))
                {
                    return;
                }
                if (!_pendingUnregister.Contains(obj))
                    _pendingUnregister.Add(obj);
                return;
            }

            UnregisterImmediate(obj);
        }

        private void UnregisterImmediate(ISimObject obj)
        {
            int bucketKey = obj.SimOrder;
            _buckets.TryGetValue(bucketKey, out Bucket bucket);
            if (bucket == null || !bucket.items.Contains(obj))
            {
                bucket = null;
                List<int> bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys != null)
                {
                    for (int i = 0; i < bucketKeys.Count; i++)
                    {
                        int candidateKey = bucketKeys[i];
                        if (!_buckets.TryGetValue(candidateKey, out Bucket candidateBucket) ||
                            !candidateBucket.items.Contains(obj))
                        {
                            continue;
                        }

                        bucketKey = candidateKey;
                        bucket = candidateBucket;
                        break;
                    }
                }
            }

            if (bucket == null)
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            if (obj is LF2Entity entity &&
                entity.Runtime?.SlotIndex >= 0 &&
                !ReleaseRuntimeSlotAndClearPresentationBinding(entity))
            {
                return;
            }

            if (!bucket.items.Remove(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;
            obj.OnRemoved(_context);

            if (bucket.items.Count == 0)
                _buckets.Remove(bucketKey);

            Debug.Log($"[SimulationWorld] Unregistered: SimOrder={bucketKey}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        private void FlushPendingUnregister()
        {
            if (_pendingUnregister.Count == 0) return;
            foreach (var obj in _pendingUnregister)
                UnregisterImmediate(obj);
            _pendingUnregister.Clear();
        }

        private void FlushPendingEntityDestroy()
        {
            // Pending entities are deliberately hidden from active pass queries. Scan the
            // runtime registry directly so the C# authority's late FreeEntity boundary still finalizes them.
            _entityScratch.Clear();
            for (int i = 0; i < _pendingSlotReleasedDestroy.Count; i++)
            {
                LF2Entity released = _pendingSlotReleasedDestroy[i];
                if (released != null && !_entityScratch.Contains(released))
                    _entityScratch.Add(released);
            }
            _pendingSlotReleasedDestroy.Clear();

            for (int runtimeSlot = 0; runtimeSlot < RuntimeSlotCapacity; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (entity?.Runtime != null &&
                    entity.Runtime.PendingFlushDestroy &&
                    !_entityScratch.Contains(entity))
                {
                    _entityScratch.Add(entity);
                }
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity.Runtime != null)
                    entity.Runtime.PendingFlushDestroy = false;

                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        private bool IsActiveForCurrentPass(ISimObject obj)
        {
            if (obj == null || _pendingUnregister.Contains(obj))
                return false;

            if (obj is LF2Entity entity && entity.Runtime != null)
            {
                if (entity.Runtime.OidMergeDormant)
                    return false;

                if (entity.Runtime.PendingFlushDestroy)
                    return false;
            }

            return true;
        }

        internal bool IsActiveForCurrentPassInternal(ISimObject obj)
        {
            return IsActiveForCurrentPass(obj);
        }

        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        private int AllocateRuntimeSlot(LF2Entity entity)
        {
            ReleasePendingDestroySlots();

            if (_runtimeSlots.ClaimedCount >= maxActiveRuntimeEntities)
                return -1;

            bool requiresDynamicSlot = entity.UsesDynamicRuntimeSlot();
            int requiredSlot = entity.RequiredRuntimeSlot;
            if (requiredSlot != -1)
            {
                if (requiredSlot >= RuntimeSlotCapacity &&
                    !TryGrowDesktopRuntimeSlots((long)requiredSlot + 1))
                {
                    return -1;
                }

                if (!_runtimeSlots.TryClaim(requiredSlot, entity, out _))
                    return -1;

                return requiredSlot;
            }

            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            bool existingSlotInRange = existingSlot >= 0 && existingSlot < RuntimeSlotCapacity;
            bool existingSlotInAllowedRange = !requiresDynamicSlot || existingSlot >= DynamicRuntimeSlotStart;
            int minimumExistingSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            if (existingSlotInRange && existingSlotInAllowedRange &&
                existingSlot >= minimumExistingSlot &&
                _runtimeSlots.TryClaim(existingSlot, entity, out _))
            {
                return existingSlot;
            }

            int startSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            int allocatedSlot = _runtimeSlots.AllocateLowest(startSlot, entity, out _);
            if (allocatedSlot >= 0 || !TryGrowDesktopRuntimeSlots((long)RuntimeSlotCapacity + 1))
                return allocatedSlot;

            return _runtimeSlots.AllocateLowest(startSlot, entity, out _);
        }

        private int FindFirstFreeRuntimeSlot(int startSlot, int endSlotExclusive)
        {
            ReleasePendingDestroySlots();

            if (_runtimeSlots.ClaimedCount >= maxActiveRuntimeEntities)
                return -1;

            bool scansCurrentTail = endSlotExclusive >= RuntimeSlotCapacity;
            int slot = _runtimeSlots.PeekLowest(startSlot, endSlotExclusive);
            if (slot >= 0 || !scansCurrentTail ||
                !TryGrowDesktopRuntimeSlots((long)RuntimeSlotCapacity + 1))
            {
                return slot;
            }

            return _runtimeSlots.PeekLowest(startSlot, RuntimeSlotCapacity);
        }

        private bool TryGrowDesktopRuntimeSlots(long minimumCapacity)
        {
            if (minimumCapacity <= RuntimeSlotCapacity)
                return true;
            if (activeRuntimeProfile != BattleRuntimeProfile.DesktopExtended ||
                minimumCapacity > int.MaxValue)
            {
                return false;
            }

            int normalizedCapacity;
            try
            {
                normalizedCapacity = BattleRuntimeProfilePolicy.NormalizeDesktopCapacity(
                    (int)minimumCapacity);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return false;
            }

            var grownAiInputSlots = new LF2Entity[normalizedCapacity];
            System.Array.Copy(aiInputSlots, grownAiInputSlots, aiInputSlots.Length);
            if (!_runtimeRestStore.GrowTo(normalizedCapacity) ||
                !_runtimeSlots.GrowTo(normalizedCapacity))
                return false;

            aiInputSlots = grownAiInputSlots;
            return true;
        }

        private void ReleasePendingDestroySlots()
        {
            List<int> bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null)
                return;

            for (int keyIndex = 0; keyIndex < bucketKeys.Count; keyIndex++)
            {
                int key = bucketKeys[keyIndex];
                if (!_buckets.TryGetValue(key, out Bucket bucket))
                    continue;

                for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                {
                    if (bucket.items[itemIndex] is not LF2Entity entity ||
                        entity.Runtime == null ||
                        !entity.Runtime.PendingFlushDestroy)
                    {
                        continue;
                    }

                    int slot = entity.Runtime.SlotIndex;
                    if (slot < 0 || slot >= RuntimeSlotCapacity)
                        continue;

                    if (object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(slot), entity) &&
                        ReleaseRuntimeSlotAndClearPresentationBinding(entity) &&
                        !_pendingSlotReleasedDestroy.Contains(entity))
                    {
                        _pendingSlotReleasedDestroy.Add(entity);
                    }
                }
            }
        }

        private bool ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot < 0)
                return true;
            if (slot >= RuntimeSlotCapacity ||
                !object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(slot), entity))
            {
                Debug.LogError(
                    $"[SimulationWorld] Refusing runtime slot release without the matching claim: " +
                    $"EntitySlot={slot}, StableId={entity.StableId}");
                return false;
            }

            bool wasBound = entity.ItrRest?.IsBound == true;
            if (wasBound && entity.ItrRest.BoundVictimSlot != slot)
            {
                Debug.LogError(
                    $"[SimulationWorld] Refusing runtime slot release with a mismatched rest binding: " +
                    $"EntitySlot={slot}, BoundVictimSlot={entity.ItrRest.BoundVictimSlot}, " +
                    $"StableId={entity.StableId}");
                return false;
            }
            if (wasBound && !entity.ItrRest.Unbind(false))
                return false;

            if (!_runtimeSlots.Release(slot, entity))
            {
                if (wasBound && !entity.ItrRest.Bind(_runtimeRestStore, slot, false))
                {
                    Debug.LogError(
                        $"[SimulationWorld] Failed to restore runtime rest binding after slot release rollback: " +
                        $"Slot={slot}, StableId={entity.StableId}");
                }
                return false;
            }

            entity.SetRuntimeSlotIndex(-1);
            return true;
        }

        private bool ReleaseRuntimeSlotAndClearPresentationBinding(LF2Entity entity)
        {
            NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                entity?.Renderer);
            if (ReleaseRuntimeSlot(entity))
                return true;

            int slot = entity?.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 &&
                TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle restoredHandle))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                    entity.Renderer,
                    restoredHandle);
            }

            return false;
        }

        private void RollbackRuntimeSlotRegistration(LF2Entity entity, int runtimeSlot)
        {
            entity?.ItrRest?.Unbind(false);
            if (entity != null &&
                object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(runtimeSlot), entity))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                    entity.Renderer);
                _runtimeSlots.Release(runtimeSlot, entity);
            }
            entity?.SetRuntimeSlotIndex(-1);
        }

        internal bool RestoreStageSpawnRestState(int runtimeSlot, LF2Entity entity)
        {
            if (!_runtimeSlots.IsAddressable(runtimeSlot) ||
                entity?.Runtime == null ||
                entity.Runtime.SlotIndex != runtimeSlot ||
                entity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
            {
                return false;
            }

            return entity.ItrRest != null &&
                   entity.ItrRest.Bind(_runtimeRestStore, runtimeSlot, false);
        }

        internal int GetRawRestArest(int runtimeSlot)
        {
            return _runtimeRestStore.GetARest(runtimeSlot);
        }

        internal int GetRawRestVrest(int victimSlot, int attackerSlot)
        {
            return _runtimeRestStore.GetVRest(victimSlot, attackerSlot);
        }

        public int ObjectCount
        {
            get
            {
                int count = 0;
                var bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys == null) return 0;

                foreach (int simOrder in bucketKeys)
                {
                    if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                    for (int i = 0; i < bucket.items.Count; i++)
                    {
                        ISimObject obj = bucket.items[i];
                        if (obj is LF2Entity entity)
                        {
                            if (_pendingUnregister.Contains(entity))
                                continue;

                            if (entity.Runtime != null &&
                                (entity.Runtime.OidMergeDormant || entity.Runtime.PendingFlushDestroy))
                                continue;
                        }

                        count++;
                    }
                }
                return count;
            }
        }

        public SimContext Context => _context;
    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattleRenderingBenchmark.cs ---
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using Unity.Profiling;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Profiling;

namespace NTSD.Animation.Rendering
{
    public enum BattleRenderingBenchmarkComparison : byte
    {
        Single = 0,
        CentralLegacyAB = 1,
    }

    public enum BattleRenderingBenchmarkScenarioKind : byte
    {
        CurrentScene = 0,
        Entities100 = 1,
        Entities300 = 2,
        Entities500 = 3,
        Entities1000 = 4,
    }

    public enum BattleRenderingBenchmarkVerdict : byte
    {
        Pass = 0,
        Fail = 1,
        Incomplete = 2,
        Unsupported = 3,
    }

    public enum BattleBenchmarkMetricApplicability : byte
    {
        Applicable = 0,
        NotApplicable = 1,
    }

    public enum BattleBenchmarkMetricStatus : byte
    {
        Available = 0,
        Missing = 1,
        NotApplicable = 2,
        Unsupported = 3,
        Passed = 4,
        Failed = 5,
    }

    public readonly struct BattleRenderingBenchmarkScenario
    {
        private BattleRenderingBenchmarkScenario(
            BattleRenderingBenchmarkScenarioKind kind,
            int requestedEntityCount,
            string name)
        {
            Kind = kind;
            RequestedEntityCount = requestedEntityCount;
            Name = name;
        }

        public BattleRenderingBenchmarkScenarioKind Kind { get; }
        public int RequestedEntityCount { get; }
        public string Name { get; }
        public bool UsesCurrentScene => Kind == BattleRenderingBenchmarkScenarioKind.CurrentScene;

        public static BattleRenderingBenchmarkScenario Parse(string value)
        {
            string normalized = string.IsNullOrWhiteSpace(value)
                ? "current-scene"
                : value.Trim().ToLowerInvariant();
            switch (normalized)
            {
                case "current-scene":
                    return new BattleRenderingBenchmarkScenario(
                        BattleRenderingBenchmarkScenarioKind.CurrentScene,
                        -1,
                        "current-scene");
                case "100":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities100, 100);
                case "300":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities300, 300);
                case "500":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities500, 500);
                case "1000":
                    return Fixed(BattleRenderingBenchmarkScenarioKind.Entities1000, 1000);
                default:
                    throw new ArgumentException(
                        $"Unknown benchmark scenario '{value}'. Expected current-scene, 100, 300, 500, or 1000.",
                        nameof(value));
            }
        }

        private static BattleRenderingBenchmarkScenario Fixed(
            BattleRenderingBenchmarkScenarioKind kind,
            int count)
        {
            return new BattleRenderingBenchmarkScenario(kind, count, count.ToString());
        }
    }

    [Serializable]
    public sealed class BattleRenderingBenchmarkRequest
    {
        public string backend = nameof(BattlePresentationBackendMode.CentralOnly);
        public string comparison = "single";
        public int warmupFrames = 30;
        public int sampleFrames = 120;
        public int leakCheckFrames = 600;
        public long maxManagedGrowthBytes = 1048576L;
        public long maxGraphicsGrowthBytes = 4194304L;
        public string targetActiveEntities = "current-scene";
        public string outputPath = "Temp/NTSD_BattleRenderingBenchmark.json";
    }

    public readonly struct BattleRenderingBenchmarkConfig
    {
        public BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode backend,
            int warmupFrames,
            int sampleFrames,
            string targetActiveEntities,
            string outputPath)
            : this(
                backend,
                BattleRenderingBenchmarkComparison.Single,
                warmupFrames,
                sampleFrames,
                0,
                1048576L,
                4194304L,
                targetActiveEntities,
                outputPath)
        {
        }

        public BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode backend,
            BattleRenderingBenchmarkComparison comparison,
            int warmupFrames,
            int sampleFrames,
            int leakCheckFrames,
            long maxManagedGrowthBytes,
            long maxGraphicsGrowthBytes,
            string targetActiveEntities,
            string outputPath)
        {
            BattlePresentationBackendResolver.ValidateAvailable(backend);
            if (backend == BattlePresentationBackendMode.CentralShadowBuild)
            {
                throw new ArgumentException(
                    "CentralShadowBuild fixes pixel ownership to Legacy and is not a valid P8-D A/B backend.",
                    nameof(backend));
            }
            if (comparison != BattleRenderingBenchmarkComparison.Single &&
                comparison != BattleRenderingBenchmarkComparison.CentralLegacyAB)
            {
                throw new ArgumentOutOfRangeException(nameof(comparison));
            }
            if (warmupFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(warmupFrames));
            if (sampleFrames <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleFrames));
            if (leakCheckFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(leakCheckFrames));
            if (maxManagedGrowthBytes < 0L)
                throw new ArgumentOutOfRangeException(nameof(maxManagedGrowthBytes));
            if (maxGraphicsGrowthBytes < 0L)
                throw new ArgumentOutOfRangeException(nameof(maxGraphicsGrowthBytes));

            Scenario = BattleRenderingBenchmarkScenario.Parse(targetActiveEntities);
            Backend = backend;
            Comparison = comparison;
            WarmupFrames = warmupFrames;
            SampleFrames = sampleFrames;
            LeakCheckFrames = leakCheckFrames;
            MaxManagedGrowthBytes = maxManagedGrowthBytes;
            MaxGraphicsGrowthBytes = maxGraphicsGrowthBytes;
            OutputPath = outputPath ?? string.Empty;
        }

        public BattlePresentationBackendMode Backend { get; }
        public BattleRenderingBenchmarkComparison Comparison { get; }
        public int WarmupFrames { get; }
        public int SampleFrames { get; }
        public int LeakCheckFrames { get; }
        public long MaxManagedGrowthBytes { get; }
        public long MaxGraphicsGrowthBytes { get; }
        public BattleRenderingBenchmarkScenario Scenario { get; }
        public string TargetActiveEntities => Scenario.Name;
        public string OutputPath { get; }

        public static BattleRenderingBenchmarkConfig Default => new BattleRenderingBenchmarkConfig(
            BattlePresentationBackendMode.CentralOnly,
            BattleRenderingBenchmarkComparison.CentralLegacyAB,
            30,
            120,
            600,
            1048576L,
            4194304L,
            "current-scene",
            "Temp/NTSD_BattleRenderingBenchmark.json");

        public BattleRenderingBenchmarkConfig ForBackend(BattlePresentationBackendMode backend)
        {
            return new BattleRenderingBenchmarkConfig(
                backend,
                BattleRenderingBenchmarkComparison.Single,
                WarmupFrames,
                SampleFrames,
                LeakCheckFrames,
                MaxManagedGrowthBytes,
                MaxGraphicsGrowthBytes,
                Scenario.Name,
                OutputPath);
        }

        public static BattleRenderingBenchmarkConfig FromRequest(BattleRenderingBenchmarkRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            string backendText = string.IsNullOrWhiteSpace(request.backend)
                ? nameof(BattlePresentationBackendMode.CentralOnly)
                : request.backend;
            if (!BattlePresentationBackendResolver.TryParse(backendText, out BattlePresentationBackendMode backend))
                throw new ArgumentException($"Unknown battle presentation backend '{backendText}'.", nameof(request));
            BattleRenderingBenchmarkComparison comparison = ParseComparison(request.comparison);
            return new BattleRenderingBenchmarkConfig(
                backend,
                comparison,
                request.warmupFrames,
                request.sampleFrames,
                request.leakCheckFrames,
                request.maxManagedGrowthBytes,
                request.maxGraphicsGrowthBytes,
                request.targetActiveEntities,
                request.outputPath);
        }

        private static BattleRenderingBenchmarkComparison ParseComparison(string value)
        {
            if (string.IsNullOrWhiteSpace(value) ||
                string.Equals(value.Trim(), "single", StringComparison.OrdinalIgnoreCase))
            {
                return BattleRenderingBenchmarkComparison.Single;
            }
            if (string.Equals(value.Trim(), "ab", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(value.Trim(), "central-legacy-ab", StringComparison.OrdinalIgnoreCase))
            {
                return BattleRenderingBenchmarkComparison.CentralLegacyAB;
            }
            throw new ArgumentException(
                $"Unknown benchmark comparison '{value}'. Expected single or ab.",
                nameof(value));
        }
    }

    public readonly struct BattleBenchmarkMetric
    {
        private BattleBenchmarkMetric(bool available, double value, string unit)
        {
            Available = available;
            Value = value;
            Unit = unit ?? string.Empty;
        }

        public bool Available { get; }
        public double Value { get; }
        public string Unit { get; }

        public static BattleBenchmarkMetric Unavailable(string unit = "") =>
            new BattleBenchmarkMetric(false, 0d, unit);

        public static BattleBenchmarkMetric FromValue(double value, string unit = "") =>
            new BattleBenchmarkMetric(true, value, unit);

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["unit"] = Unit,
                ["value"] = Available ? (object)Value : null,
            };
        }
    }

    public static class BattleRenderingBenchmarkSubmissionPolicy
    {
        public const int Unavailable = -1;

        public static int FromGraphicsDrawMeshCalls(bool callsIssued, int actualCallCount)
        {
            if (!callsIssued)
                return Unavailable;
            if (actualCallCount <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(actualCallCount),
                    "An available Graphics.DrawMesh submission count must contain at least one actual call.");
            }
            return actualCallCount;
        }
    }

    public static class BattleRenderingBenchmarkPassPolicy
    {
        public static bool Evaluate(
            bool countValidated,
            bool runtimeAdmissionValidated,
            bool logicTickMetricsValidated,
            bool determinismValidated,
            bool rendererWorkloadValidated,
            bool leakRequested,
            bool leakPassed)
        {
            return countValidated &&
                   runtimeAdmissionValidated &&
                   logicTickMetricsValidated &&
                   determinismValidated &&
                   rendererWorkloadValidated &&
                   (!leakRequested || leakPassed);
        }
    }

    public static class BattleRenderingBenchmarkEvidencePolicy
    {
        public static BattleBenchmarkMetricStatus ValidationStatus(bool? observedResult)
        {
            if (!observedResult.HasValue)
                return BattleBenchmarkMetricStatus.Missing;
            return observedResult.Value
                ? BattleBenchmarkMetricStatus.Passed
                : BattleBenchmarkMetricStatus.Failed;
        }
    }

    public static class BattleBenchmarkDrawCallPolicy
    {
        public static BattleBenchmarkMetric RequirePositiveForNonEmptyWorkload(
            BattleBenchmarkMetric metric)
        {
            return metric.Available && metric.Value <= 0d
                ? BattleBenchmarkMetric.Unavailable(metric.Unit)
                : metric;
        }
    }

    public readonly struct BattleRenderingBenchmarkLogicTickSample
    {
        internal BattleRenderingBenchmarkLogicTickSample(
            int tickIndex,
            BattleBenchmarkMetric elapsedMilliseconds,
            BattleBenchmarkMetric allocatedBytes,
            string checksum)
        {
            TickIndex = tickIndex;
            ElapsedMilliseconds = elapsedMilliseconds;
            AllocatedBytes = allocatedBytes;
            Checksum = checksum ?? string.Empty;
        }

        public int TickIndex { get; }
        public BattleBenchmarkMetric ElapsedMilliseconds { get; }
        public BattleBenchmarkMetric AllocatedBytes { get; }
        public string Checksum { get; }

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["allocatedBytes"] = AllocatedBytes.ToProjection(),
                ["checksum"] = Checksum,
                ["elapsedMilliseconds"] = ElapsedMilliseconds.ToProjection(),
                ["tickIndex"] = TickIndex,
            };
        }
    }

    public sealed class BattleBenchmarkMetricAvailability
    {
        internal BattleBenchmarkMetricAvailability(
            string metric,
            string source,
            bool available,
            string reason)
            : this(
                metric,
                required: false,
                BattleBenchmarkMetricApplicability.Applicable,
                available ? BattleBenchmarkMetricStatus.Available : BattleBenchmarkMetricStatus.Missing,
                "completed-frame",
                available ? 1 : 0,
                1,
                source,
                reason)
        {
        }

        public BattleBenchmarkMetricAvailability(
            string metric,
            bool required,
            BattleBenchmarkMetricApplicability applicability,
            BattleBenchmarkMetricStatus status,
            string scope,
            int sampleCount,
            int expectedSampleCount,
            string source,
            string reason)
        {
            Metric = metric ?? string.Empty;
            Required = required;
            Applicability = applicability;
            Status = status;
            Scope = scope ?? string.Empty;
            SampleCount = Math.Max(0, sampleCount);
            ExpectedSampleCount = Math.Max(0, expectedSampleCount);
            Source = source ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public string Metric { get; }
        public bool Required { get; }
        public BattleBenchmarkMetricApplicability Applicability { get; }
        public BattleBenchmarkMetricStatus Status { get; }
        public string Scope { get; }
        public int SampleCount { get; }
        public int ExpectedSampleCount { get; }
        public string Source { get; }
        public bool Available =>
            Status == BattleBenchmarkMetricStatus.Available ||
            Status == BattleBenchmarkMetricStatus.Passed;
        public string Reason { get; }

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["applicability"] = Applicability.ToString(),
                ["expectedSampleCount"] = ExpectedSampleCount,
                ["metric"] = Metric,
                ["reason"] = Reason,
                ["required"] = Required,
                ["sampleCount"] = SampleCount,
                ["scope"] = Scope,
                ["source"] = Source,
                ["status"] = Status.ToString(),
            };
        }
    }

    public readonly struct BattleRenderingBenchmarkPolicyContext
    {
        public BattleRenderingBenchmarkPolicyContext(
            bool isPlaying,
            bool isEditor,
            RuntimePlatform platform,
            bool graphicsMultiThreaded,
            bool frameTimingStatsEnabled)
        {
            IsPlaying = isPlaying;
            IsEditor = isEditor;
            Platform = platform;
            GraphicsMultiThreaded = graphicsMultiThreaded;
            FrameTimingStatsEnabled = frameTimingStatsEnabled;
        }

        public bool IsPlaying { get; }
        public bool IsEditor { get; }
        public RuntimePlatform Platform { get; }
        public bool GraphicsMultiThreaded { get; }
        public bool FrameTimingStatsEnabled { get; }
        public bool IsWindowsStandalone =>
            Platform == RuntimePlatform.WindowsPlayer;
        public bool IsSupportedExecutionScope =>
            IsPlaying && (IsEditor || IsWindowsStandalone);
        public string Scope => !IsPlaying
            ? "EditMode"
            : IsEditor
                ? "PlayModeEditor"
                : IsWindowsStandalone
                    ? "WindowsStandalone"
                    : Platform.ToString();

        internal static BattleRenderingBenchmarkPolicyContext Capture()
        {
            return new BattleRenderingBenchmarkPolicyContext(
                Application.isPlaying,
                Application.isEditor,
                Application.platform,
                SystemInfo.graphicsMultiThreaded,
                FrameTimingManager.IsFeatureEnabled());
        }
    }

    public static class BattleRenderingBenchmarkVerdictPolicy
    {
        public const string PolicyId = "ntsd-battle-rendering-benchmark-policy-v5";

        private static readonly string[] MandatoryMetricNames =
        {
            "frameTimeMs",
            "mainThreadTimeMs",
            "renderThreadTimeMs",
            "gpuFrameTimeMs",
            "managedAllocationBytes",
            "drawCalls",
            "totalAllocatedMemoryBytes",
            "graphicsMemoryBytes",
            "benchmarkOwnedTextureMemoryBytes",
            "logicTickTimeMs",
            "logicTickAllocatedBytes",
            "presentationBuildTimeMs",
            "presenterSubmittedRenderItems",
            "resourceSegments",
            "benchmarkOwnedMemoryBytes",
            "presenterSubmissionDrawCalls",
            "meshChunks",
            "exactSampleCount",
            "countValidated",
            "runtimeAdmissionValidated",
            "determinismValidated",
            "rendererWorkloadValidated",
            "leakCheck",
        };
        private static readonly IReadOnlyList<string> MandatoryMetricRegistry =
            Array.AsReadOnly(MandatoryMetricNames);

        public static IReadOnlyList<string> RequiredMetricNames => MandatoryMetricRegistry;

        public static BattleRenderingBenchmarkVerdict Evaluate(
            BattleRenderingBenchmarkPolicyContext context,
            IReadOnlyList<BattleBenchmarkMetricAvailability> metrics,
            out string reason,
            out string[] missingRequiredMetrics)
        {
            var missing = new List<string>();
            var failed = new List<string>();
            var metricCounts = new Dictionary<string, int>(StringComparer.Ordinal);
            var mandatoryNames = new HashSet<string>(MandatoryMetricNames, StringComparer.Ordinal);
            for (int index = 0; index < metrics.Count; index++)
            {
                BattleBenchmarkMetricAvailability metric = metrics[index];
                metricCounts.TryGetValue(metric.Metric, out int count);
                metricCounts[metric.Metric] = count + 1;
                if (!mandatoryNames.Contains(metric.Metric))
                    missing.Add(metric.Metric + " (unknown schema entry)");
                if (!metric.Required ||
                    metric.Applicability == BattleBenchmarkMetricApplicability.NotApplicable)
                {
                    continue;
                }

                if (metric.Status == BattleBenchmarkMetricStatus.Failed)
                    failed.Add(metric.Metric);
                else if (!metric.Available)
                    missing.Add(metric.Metric);
            }

            for (int index = 0; index < MandatoryMetricNames.Length; index++)
            {
                string metricName = MandatoryMetricNames[index];
                metricCounts.TryGetValue(metricName, out int count);
                if (count == 0)
                    missing.Add(metricName);
                else if (count > 1)
                    missing.Add(metricName + " (duplicate schema entry)");
            }
            missingRequiredMetrics = missing.ToArray();

            if (!context.IsPlaying)
            {
                reason = "EditMode does not provide a completed rendered-frame benchmark scope.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (!context.IsSupportedExecutionScope)
            {
                reason = "The v5 policy supports Play Mode Editor and Windows Standalone only.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (!context.FrameTimingStatsEnabled)
            {
                reason = "FrameTimingManager is disabled; completed-frame CPU/GPU metrics are unsupported.";
                return BattleRenderingBenchmarkVerdict.Unsupported;
            }
            if (failed.Count > 0)
            {
                reason = "Required validation gates failed: " + string.Join(", ", failed) + ".";
                return BattleRenderingBenchmarkVerdict.Fail;
            }
            if (missing.Count > 0)
            {
                reason = "Required metrics are missing or incomplete: " + string.Join(", ", missing) + ".";
                return BattleRenderingBenchmarkVerdict.Incomplete;
            }

            reason = "All required v5 metrics and validation gates passed.";
            return BattleRenderingBenchmarkVerdict.Pass;
        }
    }

    public sealed class BattleRenderingBenchmarkFrame
    {
        internal BattleRenderingBenchmarkFrame(int frameIndex, int presentationEntityCount, int commandCount)
        {
            FrameIndex = frameIndex;
            PresentationEntityCount = presentationEntityCount;
            CommandCount = commandCount;
        }

        public int FrameIndex { get; }
        public int PresentationEntityCount { get; }
        public int CommandCount { get; }
        public BattleBenchmarkMetric FrameTimeMs { get; internal set; }
        public BattleBenchmarkMetric MainThreadTimeMs { get; internal set; }
        public BattleBenchmarkMetric RenderThreadTimeMs { get; internal set; }
        public BattleBenchmarkMetric GpuFrameTimeMs { get; internal set; }
        public BattleBenchmarkMetric LogicTickTimeMs { get; internal set; }
        public BattleBenchmarkMetric LogicTickAllocatedBytes { get; internal set; }
        public string LogicTickChecksum { get; internal set; } = string.Empty;
        public BattleBenchmarkMetric PresentationBuildTimeMs { get; internal set; }
        public BattleBenchmarkMetric ManagedAllocationBytes { get; internal set; }
        public BattleBenchmarkMetric DrawCalls { get; internal set; }
        public BattleBenchmarkMetric PresenterSubmittedRenderItems { get; internal set; }
        public BattleBenchmarkMetric PresenterSubmissionDrawCalls { get; internal set; }
        public BattleBenchmarkMetric TotalAllocatedMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric GraphicsMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric BenchmarkOwnedTextureMemoryBytes { get; internal set; }
        public BattleBenchmarkMetric BenchmarkOwnedMemoryBytes { get; internal set; }
        public int BenchmarkResourceGeneration { get; internal set; }
        public BattleBenchmarkMetric SourceCommands { get; internal set; }
        public BattleBenchmarkMetric ResolvedCommands { get; internal set; }
        public BattleBenchmarkMetric UnresolvedCommands { get; internal set; }
        public BattleBenchmarkMetric ResourceSegments { get; internal set; }
        public BattleBenchmarkMetric MeshChunks { get; internal set; }
        public string RequestedBackend { get; internal set; } = string.Empty;
        public string EffectiveBackend { get; internal set; } = string.Empty;

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["presentationEntityCount"] = PresentationEntityCount,
                ["benchmarkOwnedMemoryBytes"] = BenchmarkOwnedMemoryBytes.ToProjection(),
                ["benchmarkOwnedTextureMemoryBytes"] = BenchmarkOwnedTextureMemoryBytes.ToProjection(),
                ["benchmarkResourceGeneration"] = BenchmarkResourceGeneration,
                ["commandCount"] = CommandCount,
                ["drawCalls"] = DrawCalls.ToProjection(),
                ["effectiveBackend"] = EffectiveBackend,
                ["frameIndex"] = FrameIndex,
                ["frameTimeMs"] = FrameTimeMs.ToProjection(),
                ["gpuFrameTimeMs"] = GpuFrameTimeMs.ToProjection(),
                ["graphicsMemoryBytes"] = GraphicsMemoryBytes.ToProjection(),
                ["logicTickTimeMs"] = LogicTickTimeMs.ToProjection(),
                ["logicTickAllocatedBytes"] = LogicTickAllocatedBytes.ToProjection(),
                ["logicTickChecksum"] = LogicTickChecksum,
                ["mainThreadTimeMs"] = MainThreadTimeMs.ToProjection(),
                ["managedAllocationBytes"] = ManagedAllocationBytes.ToProjection(),
                ["meshChunks"] = MeshChunks.ToProjection(),
                ["presentationBuildTimeMs"] = PresentationBuildTimeMs.ToProjection(),
                ["presenterSubmittedRenderItems"] = PresenterSubmittedRenderItems.ToProjection(),
                ["presenterSubmissionDrawCalls"] = PresenterSubmissionDrawCalls.ToProjection(),
                ["renderThreadTimeMs"] = RenderThreadTimeMs.ToProjection(),
                ["requestedBackend"] = RequestedBackend,
                ["resolvedCommands"] = ResolvedCommands.ToProjection(),
                ["resourceSegments"] = ResourceSegments.ToProjection(),
                ["sourceCommands"] = SourceCommands.ToProjection(),
                ["totalAllocatedMemoryBytes"] = TotalAllocatedMemoryBytes.ToProjection(),
                ["unresolvedCommands"] = UnresolvedCommands.ToProjection(),
            };
        }
    }

    public sealed class BattleRenderingBenchmarkLeakReport
    {
        internal BattleRenderingBenchmarkLeakReport(
            bool available,
            bool passed,
            int soakFrames,
            long prePresenterManaged,
            long prePresenterGraphics,
            bool prePresenterGraphicsAvailable,
            long managedStart,
            long managedEnd,
            long graphicsStart,
            long graphicsEnd,
            bool graphicsAvailable,
            long ownedStart,
            long ownedEnd,
            int resourcesStart,
            int resourcesEnd,
            long maxManagedGrowth,
            long maxGraphicsGrowth,
            int teardownFrames,
            long teardownManagedEnd,
            long teardownGraphicsEnd,
            bool teardownGraphicsAvailable,
            long teardownOwnedEnd,
            int teardownResourcesEnd,
            BattleBenchmarkMetricStatus teardownStatus,
            string teardownReason,
            string measurementMode,
            string reason)
        {
            Available = available;
            Passed = passed;
            SoakFrames = soakFrames;
            PrePresenterManagedBytes = prePresenterManaged;
            PrePresenterGraphicsBytes = prePresenterGraphics;
            PrePresenterGraphicsAvailable = prePresenterGraphicsAvailable;
            ManagedStartBytes = managedStart;
            ManagedEndBytes = managedEnd;
            GraphicsStartBytes = graphicsStart;
            GraphicsEndBytes = graphicsEnd;
            GraphicsAvailable = graphicsAvailable;
            OwnedStartBytes = ownedStart;
            OwnedEndBytes = ownedEnd;
            ResourcesStart = resourcesStart;
            ResourcesEnd = resourcesEnd;
            MaxManagedGrowthBytes = maxManagedGrowth;
            MaxGraphicsGrowthBytes = maxGraphicsGrowth;
            TeardownFrames = teardownFrames;
            TeardownManagedEndBytes = teardownManagedEnd;
            TeardownGraphicsEndBytes = teardownGraphicsEnd;
            TeardownGraphicsAvailable = teardownGraphicsAvailable;
            TeardownOwnedEndBytes = teardownOwnedEnd;
            TeardownResourcesEnd = teardownResourcesEnd;
            TeardownStatus = teardownStatus;
            TeardownReason = teardownReason ?? string.Empty;
            MeasurementMode = measurementMode ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        public bool Available { get; }
        public bool Passed { get; }
        public int SoakFrames { get; }
        public long PrePresenterManagedBytes { get; }
        public long PrePresenterGraphicsBytes { get; }
        public bool PrePresenterGraphicsAvailable { get; }
        public long ManagedStartBytes { get; }
        public long ManagedEndBytes { get; }
        public long GraphicsStartBytes { get; }
        public long GraphicsEndBytes { get; }
        public bool GraphicsAvailable { get; }
        public long OwnedStartBytes { get; }
        public long OwnedEndBytes { get; }
        public int ResourcesStart { get; }
        public int ResourcesEnd { get; }
        public long MaxManagedGrowthBytes { get; }
        public long MaxGraphicsGrowthBytes { get; }
        public int TeardownFrames { get; }
        public long TeardownManagedEndBytes { get; }
        public long TeardownGraphicsEndBytes { get; }
        public bool TeardownGraphicsAvailable { get; }
        public long TeardownOwnedEndBytes { get; }
        public int TeardownResourcesEnd { get; }
        public BattleBenchmarkMetricStatus TeardownStatus { get; }
        public string TeardownReason { get; }
        public string MeasurementMode { get; }
        public string Reason { get; }
        public long ManagedGrowthBytes => ManagedEndBytes - ManagedStartBytes;
        public long GraphicsGrowthBytes => GraphicsEndBytes - GraphicsStartBytes;
        public long OwnedGrowthBytes => OwnedEndBytes - OwnedStartBytes;
        public long TeardownManagedGrowthBytes => TeardownManagedEndBytes - ManagedStartBytes;
        public long TeardownGraphicsGrowthBytes => TeardownGraphicsEndBytes - GraphicsStartBytes;

        internal Dictionary<string, object> ToProjection()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = Available,
                ["graphicsAvailable"] = GraphicsAvailable,
                ["graphicsEndBytes"] = GraphicsAvailable ? (object)GraphicsEndBytes : null,
                ["graphicsGrowthBytes"] = GraphicsAvailable ? (object)GraphicsGrowthBytes : null,
                ["graphicsStartBytes"] = GraphicsAvailable ? (object)GraphicsStartBytes : null,
                ["prePresenterGraphicsAvailable"] = PrePresenterGraphicsAvailable,
                ["prePresenterGraphicsBytes"] = PrePresenterGraphicsAvailable
                    ? (object)PrePresenterGraphicsBytes
                    : null,
                ["prePresenterManagedBytes"] = Available ? (object)PrePresenterManagedBytes : null,
                ["managedEndBytes"] = Available ? (object)ManagedEndBytes : null,
                ["managedGrowthBytes"] = Available ? (object)ManagedGrowthBytes : null,
                ["managedStartBytes"] = Available ? (object)ManagedStartBytes : null,
                ["maxGraphicsGrowthBytes"] = MaxGraphicsGrowthBytes,
                ["maxManagedGrowthBytes"] = MaxManagedGrowthBytes,
                ["measurementMode"] = MeasurementMode,
                ["ownedEndBytes"] = Available ? (object)OwnedEndBytes : null,
                ["ownedGrowthBytes"] = Available ? (object)OwnedGrowthBytes : null,
                ["ownedStartBytes"] = Available ? (object)OwnedStartBytes : null,
                ["passed"] = Available ? (object)Passed : null,
                ["reason"] = Reason,
                ["resourcesEnd"] = Available ? (object)ResourcesEnd : null,
                ["resourcesStart"] = Available ? (object)ResourcesStart : null,
                ["soakFrames"] = SoakFrames,
                ["teardownFrames"] = TeardownFrames,
                ["teardownGraphicsAvailable"] = TeardownGraphicsAvailable,
                ["teardownGraphicsEndBytes"] = TeardownGraphicsAvailable
                    ? (object)TeardownGraphicsEndBytes
                    : null,
                ["teardownGraphicsGrowthBytes"] = TeardownGraphicsAvailable
                    ? (object)TeardownGraphicsGrowthBytes
                    : null,
                ["teardownManagedEndBytes"] = Available ? (object)TeardownManagedEndBytes : null,
                ["teardownManagedGrowthBytes"] = Available ? (object)TeardownManagedGrowthBytes : null,
                ["teardownMemoryBaseline"] =
                    "steady-state soak baseline; pre-presenter fields are initialization diagnostics only",
                ["teardownOwnedEndBytes"] = Available ? (object)TeardownOwnedEndBytes : null,
                ["teardownReason"] = TeardownReason,
                ["teardownResourcesEnd"] = Available ? (object)TeardownResourcesEnd : null,
                ["teardownStatus"] = TeardownStatus.ToString(),
            };
        }

        internal static BattleRenderingBenchmarkLeakReport NotRequested()
        {
            return NotRun("Leak/long-run soak was not requested.", "not-requested", BattleBenchmarkMetricStatus.NotApplicable);
        }

        internal static BattleRenderingBenchmarkLeakReport NotRun(string reason)
        {
            return NotRun(reason, "not-run", BattleBenchmarkMetricStatus.Missing);
        }

        private static BattleRenderingBenchmarkLeakReport NotRun(
            string reason,
            string measurementMode,
            BattleBenchmarkMetricStatus teardownStatus)
        {
            return new BattleRenderingBenchmarkLeakReport(
                false,
                false,
                0,
                0L,
                0L,
                false,
                0L,
                0L,
                0L,
                0L,
                false,
                0L,
                0L,
                0,
                0,
                0L,
                0L,
                0,
                0L,
                0L,
                false,
                0L,
                0,
                teardownStatus,
                reason,
                measurementMode,
                reason);
        }
    }

        public sealed class BattleRenderingBenchmarkReport
    {
        internal BattleRenderingBenchmarkReport(
            BattleRenderingBenchmarkConfig config,
            BattleRenderingBenchmarkFrame[] frames,
            BattleBenchmarkMetricAvailability[] metricAvailability,
            BattleRenderingBenchmarkPolicyContext policyContext,
            int requestedPresentationEntityCount,
            int actualPresentationEntityCount,
            int commandCount,
            string workloadFingerprint,
            string presenterImplementation,
            string resourceMode,
            string drawMode,
            int benchmarkRenderTargetWidth,
            int benchmarkRenderTargetHeight,
            bool countValidated,
            bool runtimeAdmissionValidated,
            bool logicTickMetricsValidated,
            bool determinismValidated,
            bool rendererWorkloadValidated,
            BattleRenderingBenchmarkLeakReport leakReport)
        {
            Config = config;
            Frames = frames ?? Array.Empty<BattleRenderingBenchmarkFrame>();
            MetricAvailability = metricAvailability ?? Array.Empty<BattleBenchmarkMetricAvailability>();
            PolicyContext = policyContext;
            RequestedPresentationEntityCount = requestedPresentationEntityCount;
            ActualPresentationEntityCount = actualPresentationEntityCount;
            CommandCount = commandCount;
            WorkloadFingerprint = workloadFingerprint ?? string.Empty;
            PresenterImplementation = presenterImplementation ?? string.Empty;
            ResourceMode = resourceMode ?? string.Empty;
            DrawMode = drawMode ?? string.Empty;
            BenchmarkRenderTargetWidth = benchmarkRenderTargetWidth;
            BenchmarkRenderTargetHeight = benchmarkRenderTargetHeight;
            CountValidated = countValidated;
            RuntimeAdmissionValidated = runtimeAdmissionValidated;
            LogicTickMetricsValidated = logicTickMetricsValidated;
            DeterminismValidated = determinismValidated;
            RendererWorkloadValidated = rendererWorkloadValidated;
            LeakReport = leakReport ?? BattleRenderingBenchmarkLeakReport.NotRequested();
            Verdict = BattleRenderingBenchmarkVerdictPolicy.Evaluate(
                PolicyContext,
                MetricAvailability,
                out string verdictReason,
                out string[] missingRequiredMetrics);
            VerdictReason = verdictReason;
            MissingRequiredMetrics = missingRequiredMetrics;
        }

        public BattleRenderingBenchmarkConfig Config { get; }
        public IReadOnlyList<BattleRenderingBenchmarkFrame> Frames { get; }
        public IReadOnlyList<BattleBenchmarkMetricAvailability> MetricAvailability { get; }
        internal BattleRenderingBenchmarkPolicyContext PolicyContext { get; }
        public int RequestedPresentationEntityCount { get; }
        public int ActualPresentationEntityCount { get; }
        public int CommandCount { get; }
        public string WorkloadFingerprint { get; }
        public string PresenterImplementation { get; }
        public string ResourceMode { get; }
        public string DrawMode { get; }
        public int BenchmarkRenderTargetWidth { get; }
        public int BenchmarkRenderTargetHeight { get; }
        public bool CountValidated { get; }
        public bool RuntimeAdmissionValidated { get; }
        public bool LogicTickMetricsValidated { get; }
        public bool DeterminismValidated { get; }
        public bool RendererWorkloadValidated { get; }
        public int RuntimeObjectCount { get; internal set; }
        public int RuntimeSlotCapacity { get; internal set; }
        public string RuntimeProfile { get; internal set; } = string.Empty;
        public int WarmupLogicTickCount { get; internal set; }
        public int SampleLogicTickCount { get; internal set; }
        public string InputFingerprint { get; internal set; } = string.Empty;
        public string InitialRuntimeChecksum { get; internal set; } = string.Empty;
        public string FinalRuntimeChecksum { get; internal set; } = string.Empty;
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> WarmupLogicTickSamples
        {
            get;
            internal set;
        } = Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> SampleLogicTickSamples
        {
            get;
            internal set;
        } = Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
        public BattleRenderingBenchmarkLeakReport LeakReport { get; }
        public BattleRenderingBenchmarkVerdict Verdict { get; }
        public string VerdictReason { get; }
        public IReadOnlyList<string> MissingRequiredMetrics { get; }
        public int CompletedFrameRejectedAttemptCount { get; internal set; }
        public int MaxCompletedFrameSampleAttempts { get; internal set; }
        public string CompletedFrameSamplingFailureReason { get; internal set; } = string.Empty;
        public bool Passed => Verdict == BattleRenderingBenchmarkVerdict.Pass;

        public string ToJson()
        {
            return BattleCanonicalJson.Serialize(ToProjection(true));
        }

        internal Dictionary<string, object> ToProjection(bool includeEnvironment)
        {
            var frameProjection = new List<object>(Frames.Count);
            for (int i = 0; i < Frames.Count; i++)
                frameProjection.Add(Frames[i].ToProjection());
            var availability = new List<object>(MetricAvailability.Count);
            var unavailable = new List<object>();
            for (int i = 0; i < MetricAvailability.Count; i++)
            {
                BattleBenchmarkMetricAvailability item = MetricAvailability[i];
                availability.Add(item.ToProjection());
                if (!item.Available)
                    unavailable.Add(item.Metric);
            }
            var missingRequired = new List<object>(MissingRequiredMetrics.Count);
            for (int index = 0; index < MissingRequiredMetrics.Count; index++)
                missingRequired.Add(MissingRequiredMetrics[index]);

            var config = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["backend"] = Config.Backend.ToString(),
                ["leakCheckFrames"] = Config.LeakCheckFrames,
                ["maxCompletedFrameSampleAttempts"] = BattleRenderingBenchmarkSession.MaxCompletedFrameSampleAttempts,
                ["sampleFrames"] = Config.SampleFrames,
                ["targetActiveEntities"] = Config.TargetActiveEntities,
                ["warmupFrames"] = Config.WarmupFrames,
            };
            var workload = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["actualPresentationEntityCount"] = ActualPresentationEntityCount,
                ["commandCount"] = CommandCount,
                ["countValidated"] = CountValidated,
                ["runtimeAdmissionValidated"] = RuntimeAdmissionValidated,
                ["logicTickMetricsValidated"] = LogicTickMetricsValidated,
                ["determinismValidated"] = DeterminismValidated,
                ["fingerprint"] = WorkloadFingerprint,
                ["frozenPresentationFrame"] = true,
                ["gameplayRuntimeMutated"] = false,
                ["requestedPresentationEntityCount"] = RequestedPresentationEntityCount,
                ["rendererWorkloadValidated"] = RendererWorkloadValidated,
                ["runtimeObjectCount"] = RuntimeObjectCount,
                ["runtimeProfile"] = RuntimeProfile,
                ["runtimeSlotCapacity"] = RuntimeSlotCapacity,
                ["warmupLogicTickCount"] = WarmupLogicTickCount,
                ["sampleLogicTickCount"] = SampleLogicTickCount,
                ["inputFingerprint"] = InputFingerprint,
                ["initialRuntimeChecksum"] = InitialRuntimeChecksum,
                ["finalRuntimeChecksum"] = FinalRuntimeChecksum,
                ["scenario"] = Config.Scenario.Name,
                ["source"] = Config.Scenario.UsesCurrentScene
                    ? "current-scene-frozen-presentation-frame"
                    : "deterministic-mobileextended-runtime-fixture-v1",
                ["workloadKind"] = Config.Scenario.UsesCurrentScene
                    ? "frozen-current-scene-presentation"
                    : "frozen-real-runtime-presentation",
            };
            var warmupLogicTicks = new List<object>(WarmupLogicTickSamples.Count);
            for (int index = 0; index < WarmupLogicTickSamples.Count; index++)
                warmupLogicTicks.Add(WarmupLogicTickSamples[index].ToProjection());
            var sampleLogicTicks = new List<object>(SampleLogicTickSamples.Count);
            for (int index = 0; index < SampleLogicTickSamples.Count; index++)
                sampleLogicTicks.Add(SampleLogicTickSamples[index].ToProjection());
            var runtimeTrace = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["finalChecksum"] = FinalRuntimeChecksum,
                ["initialChecksum"] = InitialRuntimeChecksum,
                ["inputFingerprint"] = InputFingerprint,
                ["profile"] = RuntimeProfile,
                ["fixtureInput"] = "FrameInputSet.Empty for every logic tick",
                ["fixtureInteraction"] = Config.Scenario.UsesCurrentScene
                    ? "production current-scene runtime"
                    : "non-interacting LF2Entity fixtures with collision candidates explicitly suppressed",
                ["rngInitialSeed"] = Config.Scenario.UsesCurrentScene
                    ? "captured production state"
                    : "0x4E545344",
                ["runtimeObjectCount"] = RuntimeObjectCount,
                ["runtimeSlotCapacity"] = RuntimeSlotCapacity,
                ["sampleTicks"] = sampleLogicTicks,
                ["warmupTicks"] = warmupLogicTicks,
            };
            var limitations = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["drawAndGpuCounterScope"] =
                    "ProfilerRecorder frame counters include the complete Editor/Player frame; presenter-specific work is separately reported.",
                ["renderTargetScope"] =
                    $"Screen resolution describes the Editor/Player window; the benchmark presentation workload renders to {BenchmarkRenderTargetWidth}x{BenchmarkRenderTargetHeight}.",
                ["legacyPresenterScope"] =
                    "LegacyOnly uses a benchmark-only SpriteRenderer compatibility presenter because production battle prefabs are rendererless.",
                ["legacyVisualParityClaimed"] = false,
                ["logicTickMeasured"] = LogicTickMetricsValidated,
                ["logicTickReason"] = LogicTickMetricsValidated
                    ? "Full NTSDBattleTickSystem ticks were measured locally with Stopwatch and thread allocation counters."
                    : "No reliable full logic-tick sample was observed for this current-scene capture.",
                ["runtimeActiveEntityCapacityClaimed"] = RuntimeAdmissionValidated,
                ["runtimeActiveEntityLimitation"] = Config.Scenario.UsesCurrentScene
                    ? "The scene frame was frozen at benchmark start; runtime admission reflects the active production world at capture time."
                    : "Fixed scenarios register exactly the requested LF2Entity fixtures in a MobileExtended(1050) SimulationWorld.",
                ["productionAtlasPerformanceClaimed"] = false,
                ["productionAtlasLimitation"] =
                    "The deterministic A/B resolver uses one shared SourceTexture2D so both presenters consume identical drawable resources; production atlas modes require a separate current production-scene sample.",
                ["benchmarkOwnedTextureMemoryScope"] =
                    "benchmarkOwnedTextureMemoryBytes sums Profiler.GetRuntimeMemorySizeLong for the Texture2D and RenderTexture objects owned by the reported benchmarkResourceGeneration. It excludes global Editor/Player textures, production atlas resources, and non-texture presenter resources.",
            };
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["config"] = config,
                ["benchmarkRenderTargetHeight"] = BenchmarkRenderTargetHeight,
                ["benchmarkRenderTargetWidth"] = BenchmarkRenderTargetWidth,
                ["completedFrameSampling"] = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["acceptedSampleCount"] = Frames.Count,
                    ["maxAttemptsPerSample"] = MaxCompletedFrameSampleAttempts,
                    ["rejectedAttemptCount"] = CompletedFrameRejectedAttemptCount,
                    ["terminalFailureReason"] = string.IsNullOrEmpty(CompletedFrameSamplingFailureReason)
                        ? null
                        : (object)CompletedFrameSamplingFailureReason,
                },
                ["frames"] = frameProjection,
                ["leakCheck"] = LeakReport.ToProjection(),
                ["limitations"] = limitations,
                ["metricAvailability"] = availability,
                ["missingRequiredMetrics"] = missingRequired,
                ["policyId"] = BattleRenderingBenchmarkVerdictPolicy.PolicyId,
                ["drawMode"] = DrawMode,
                ["presenterImplementation"] = PresenterImplementation,
                ["resourceMode"] = ResourceMode,
                ["runtimeTrace"] = runtimeTrace,
                ["passed"] = Passed,
                ["reason"] = VerdictReason,
                ["schema"] = "ntsd-battle-rendering-benchmark-run-v5",
                ["summary"] = BuildSummary(),
                ["unavailableMetrics"] = unavailable,
                ["verdict"] = Verdict.ToString(),
                ["workload"] = workload,
            };
            if (includeEnvironment)
                root["environment"] = BattleRenderingBenchmarkEnvironment.Capture();
            return root;
        }

        private Dictionary<string, object> BuildSummary()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["benchmarkOwnedMemoryBytes"] = Summarize(frame => frame.BenchmarkOwnedMemoryBytes),
                ["benchmarkOwnedTextureMemoryBytes"] = Summarize(frame => frame.BenchmarkOwnedTextureMemoryBytes),
                ["drawCalls"] = Summarize(frame => frame.DrawCalls),
                ["frameTimeMs"] = Summarize(frame => frame.FrameTimeMs),
                ["gpuFrameTimeMs"] = Summarize(frame => frame.GpuFrameTimeMs),
                ["graphicsMemoryBytes"] = Summarize(frame => frame.GraphicsMemoryBytes),
                ["logicTickTimeMs"] = Summarize(frame => frame.LogicTickTimeMs),
                ["logicTickAllocatedBytes"] = Summarize(frame => frame.LogicTickAllocatedBytes),
                ["mainThreadTimeMs"] = Summarize(frame => frame.MainThreadTimeMs),
                ["managedAllocationBytes"] = Summarize(frame => frame.ManagedAllocationBytes),
                ["presentationBuildTimeMs"] = Summarize(frame => frame.PresentationBuildTimeMs),
                ["presenterSubmittedRenderItems"] = Summarize(frame => frame.PresenterSubmittedRenderItems),
                ["presenterSubmissionDrawCalls"] = Summarize(frame => frame.PresenterSubmissionDrawCalls),
                ["renderThreadTimeMs"] = Summarize(frame => frame.RenderThreadTimeMs),
                ["resourceSegments"] = Summarize(frame => frame.ResourceSegments),
                ["totalAllocatedMemoryBytes"] = Summarize(frame => frame.TotalAllocatedMemoryBytes),
            };
        }

        private Dictionary<string, object> Summarize(
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector)
        {
            int count = 0;
            double sum = 0d;
            double min = double.MaxValue;
            double max = double.MinValue;
            string unit = string.Empty;
            for (int index = 0; index < Frames.Count; index++)
            {
                BattleBenchmarkMetric metric = selector(Frames[index]);
                if (!metric.Available)
                    continue;
                count++;
                sum += metric.Value;
                min = Math.Min(min, metric.Value);
                max = Math.Max(max, metric.Value);
                unit = metric.Unit;
            }
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["available"] = count > 0,
                ["average"] = count > 0 ? (object)(sum / count) : null,
                ["maximum"] = count > 0 ? (object)max : null,
                ["minimum"] = count > 0 ? (object)min : null,
                ["sampleCount"] = count,
                ["unit"] = unit,
            };
        }

        public void WriteJson(string path)
        {
            BattleRenderingBenchmarkEnvironment.WriteJson(path, ToJson());
        }
    }

    public sealed class BattleRenderingBenchmarkSuiteReport
    {
        internal BattleRenderingBenchmarkSuiteReport(
            BattleRenderingBenchmarkConfig config,
            BattleRenderingBenchmarkReport[] runs,
            string workloadFingerprint)
        {
            Config = config;
            Runs = runs ?? Array.Empty<BattleRenderingBenchmarkReport>();
            WorkloadFingerprint = workloadFingerprint ?? string.Empty;
        }

        public BattleRenderingBenchmarkConfig Config { get; }
        public IReadOnlyList<BattleRenderingBenchmarkReport> Runs { get; }
        public string WorkloadFingerprint { get; }
        public BattleRenderingBenchmarkVerdict Verdict
        {
            get
            {
                if (Runs.Count == 0)
                    return BattleRenderingBenchmarkVerdict.Incomplete;
                bool unsupported = false;
                for (int index = 0; index < Runs.Count; index++)
                {
                    if (Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Fail)
                        return BattleRenderingBenchmarkVerdict.Fail;
                    if (Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Incomplete)
                        return BattleRenderingBenchmarkVerdict.Incomplete;
                    unsupported |= Runs[index].Verdict == BattleRenderingBenchmarkVerdict.Unsupported;
                }
                return unsupported
                    ? BattleRenderingBenchmarkVerdict.Unsupported
                    : BattleRenderingBenchmarkVerdict.Pass;
            }
        }
        public bool Passed => Verdict == BattleRenderingBenchmarkVerdict.Pass;
        public string VerdictReason
        {
            get
            {
                if (Runs.Count == 0)
                    return "The suite contains no completed runs.";
                if (Passed)
                    return "All suite runs passed the v5 policy.";
                for (int index = 0; index < Runs.Count; index++)
                {
                    if (Runs[index].Verdict == Verdict)
                        return Runs[index].VerdictReason;
                }
                return "One or more suite runs did not pass.";
            }
        }

        public string ToJson()
        {
            var runProjection = new List<object>(Runs.Count);
            for (int index = 0; index < Runs.Count; index++)
                runProjection.Add(Runs[index].ToProjection(false));
            var root = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["comparison"] = Config.Comparison.ToString(),
                ["environment"] = BattleRenderingBenchmarkEnvironment.Capture(),
                ["missingRequiredMetrics"] = BuildMissingRequiredMetrics(),
                ["passed"] = Passed,
                ["policyId"] = BattleRenderingBenchmarkVerdictPolicy.PolicyId,
                ["reason"] = VerdictReason,
                ["runs"] = runProjection,
                ["schema"] = "ntsd-battle-rendering-benchmark-suite-v5",
                ["verdict"] = Verdict.ToString(),
                ["workloadFingerprint"] = WorkloadFingerprint,
            };
            return BattleCanonicalJson.Serialize(root);
        }

        private List<object> BuildMissingRequiredMetrics()
        {
            var result = new List<object>();
            var unique = new HashSet<string>(StringComparer.Ordinal);
            for (int runIndex = 0; runIndex < Runs.Count; runIndex++)
            {
                IReadOnlyList<string> missing = Runs[runIndex].MissingRequiredMetrics;
                for (int metricIndex = 0; metricIndex < missing.Count; metricIndex++)
                {
                    string qualified = Runs[runIndex].Config.Backend + ":" + missing[metricIndex];
                    if (unique.Add(qualified))
                        result.Add(qualified);
                }
            }
            return result;
        }

        public void WriteJson(string path)
        {
            BattleRenderingBenchmarkEnvironment.WriteJson(path, ToJson());
        }
    }

    public sealed class BattleRenderingBenchmarkWorkload
    {
        private BattleRenderingBenchmarkWorkload(
            BattlePresentationFrame frozenFrame,
            int requestedEntityCount,
            int actualEntityCount,
            string fingerprint,
            string source,
            int runtimeObjectCount,
            int runtimeSlotCapacity,
            string runtimeProfile,
            BattleRenderingBenchmarkLogicTickSample[] warmupLogicTickSamples,
            BattleRenderingBenchmarkLogicTickSample[] logicTickSamples,
            string inputFingerprint,
            string initialRuntimeChecksum,
            string finalRuntimeChecksum,
            bool runtimeAdmissionValidated,
            bool runtimeStateDeterministic)
        {
            FrozenFrame = frozenFrame ?? throw new ArgumentNullException(nameof(frozenFrame));
            RequestedEntityCount = requestedEntityCount;
            ActualEntityCount = actualEntityCount;
            Fingerprint = fingerprint ?? string.Empty;
            Source = source ?? string.Empty;
            RuntimeObjectCount = runtimeObjectCount;
            RuntimeSlotCapacity = runtimeSlotCapacity;
            RuntimeProfile = runtimeProfile ?? string.Empty;
            WarmupLogicTickSamples = warmupLogicTickSamples ??
                                     Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
            LogicTickSamples = logicTickSamples ?? Array.Empty<BattleRenderingBenchmarkLogicTickSample>();
            InputFingerprint = inputFingerprint ?? string.Empty;
            InitialRuntimeChecksum = initialRuntimeChecksum ?? string.Empty;
            FinalRuntimeChecksum = finalRuntimeChecksum ?? string.Empty;
            RuntimeAdmissionValidated = runtimeAdmissionValidated;
            RuntimeStateDeterministic = runtimeStateDeterministic;
        }

        public BattlePresentationFrame FrozenFrame { get; }
        public int RequestedEntityCount { get; }
        public int ActualEntityCount { get; }
        public int CommandCount => FrozenFrame.CommandCount;
        public string Fingerprint { get; }
        public string Source { get; }
        public int RuntimeObjectCount { get; }
        public int RuntimeSlotCapacity { get; }
        public string RuntimeProfile { get; }
        public int WarmupTickCount => WarmupLogicTickSamples.Count;
        public int SampleTickCount => LogicTickSamples.Count;
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> WarmupLogicTickSamples { get; }
        public IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> LogicTickSamples { get; }
        public string InputFingerprint { get; }
        public string InitialRuntimeChecksum { get; }
        public string FinalRuntimeChecksum { get; }
        public bool RuntimeAdmissionValidated { get; }
        public bool RuntimeStateDeterministic { get; }
        public bool LogicTickMetricsAvailable
        {
            get
            {
                if (LogicTickSamples.Count <= 0)
                    return false;
                return ValidateLogicSamples(WarmupLogicTickSamples) &&
                       ValidateLogicSamples(LogicTickSamples);
            }
        }

        private static bool ValidateLogicSamples(
            IReadOnlyList<BattleRenderingBenchmarkLogicTickSample> samples)
        {
            for (int index = 0; index < samples.Count; index++)
            {
                if (!samples[index].ElapsedMilliseconds.Available ||
                    !samples[index].AllocatedBytes.Available ||
                    string.IsNullOrEmpty(samples[index].Checksum))
                {
                    return false;
                }
            }
            return true;
        }

        public static BattleRenderingBenchmarkWorkload Create(
            BattleRenderingBenchmarkScenario scenario,
            SimulationWorld world)
        {
            return Create(scenario, world, 0, 1);
        }

        public static BattleRenderingBenchmarkWorkload Create(
            BattleRenderingBenchmarkScenario scenario,
            SimulationWorld world,
            int warmupTickCount,
            int sampleTickCount)
        {
            if (warmupTickCount < 0)
                throw new ArgumentOutOfRangeException(nameof(warmupTickCount));
            if (sampleTickCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(sampleTickCount));

            BattlePresentationFrame frame;
            int requested;
            string source;
            if (scenario.UsesCurrentScene)
            {
                BattlePresentationFrame published = world?.BattlePresentation?.PublishedFrame;
                if (published == null)
                {
                    throw new InvalidOperationException(
                        "The current-scene benchmark requires an active immutable presentation frame.");
                }
                if (published.EntityCount <= 0 || published.CommandCount <= 0)
                {
                    throw new InvalidOperationException(
                        "The current-scene benchmark refuses an empty entity or render-command workload.");
                }
                frame = CloneFrame(published);
                requested = published.EntityCount;
                source = "current-scene-frozen-presentation-frame";
                int runtimeObjectCount = world.ObjectCount;
                string checksum = CaptureRuntimeChecksum(world, published.TickIndex);
                string inputFingerprint = ComputeInputFingerprint(0, 0);
                string fingerprint = ComputeFingerprint(frame, requested, source);
                return new BattleRenderingBenchmarkWorkload(
                    frame,
                    requested,
                    frame.EntityCount,
                    fingerprint,
                    source,
                    runtimeObjectCount,
                    world.RuntimeSlotCapacity,
                    world.RuntimeProfileForServices.ToString(),
                    Array.Empty<BattleRenderingBenchmarkLogicTickSample>(),
                    Array.Empty<BattleRenderingBenchmarkLogicTickSample>(),
                    inputFingerprint,
                    checksum,
                    checksum,
                    runtimeObjectCount > 0,
                    false);
            }

            requested = scenario.RequestedEntityCount;
            return BuildRuntimeWorkload(requested, warmupTickCount, sampleTickCount);
        }

        private static BattleRenderingBenchmarkWorkload BuildRuntimeWorkload(
            int requested,
            int warmupTickCount,
            int sampleTickCount)
        {
            const int runtimeCapacity = BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity;
            var primaryWorld = new SimulationWorld(BattleRuntimeProfile.MobileExtended, runtimeCapacity);
            var mirrorWorld = new SimulationWorld(BattleRuntimeProfile.MobileExtended, runtimeCapacity);
            BattleRenderingBenchmarkEntity[] primaryEntities = RegisterRuntimeEntities(primaryWorld, requested);
            RegisterRuntimeEntities(mirrorWorld, requested);

            string initialPrimary = CaptureRuntimeChecksum(primaryWorld, 0);
            string initialMirror = CaptureRuntimeChecksum(mirrorWorld, 0);
            int totalTicks = checked(warmupTickCount + sampleTickCount);
            string inputFingerprint = ComputeInputFingerprint(warmupTickCount, sampleTickCount);
            var warmupSamples = new BattleRenderingBenchmarkLogicTickSample[warmupTickCount];
            var samples = new BattleRenderingBenchmarkLogicTickSample[sampleTickCount];
            var primaryTickSystem = new NTSDBattleTickSystem(primaryWorld);
            var mirrorTickSystem = new NTSDBattleTickSystem(mirrorWorld);
            int sampleIndex = 0;
            for (int tickIndex = 1; tickIndex <= totalTicks; tickIndex++)
            {
                FrameInputSet primaryInput = FrameInputSet.Empty(tickIndex);
                FrameInputSet mirrorInput = FrameInputSet.Empty(tickIndex);
                primaryWorld.ApplyFrameInputSet(primaryInput);
                mirrorWorld.ApplyFrameInputSet(mirrorInput);

                long allocationStart = GC.GetAllocatedBytesForCurrentThread();
                long started = Stopwatch.GetTimestamp();
                primaryTickSystem.RunReleaseTick(tickIndex);
                double elapsedMilliseconds = BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
                long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
                mirrorTickSystem.RunReleaseTick(tickIndex);

                string checksum = CaptureRuntimeChecksum(primaryWorld, tickIndex);
                var tickSample = new BattleRenderingBenchmarkLogicTickSample(
                    tickIndex,
                    BattleBenchmarkMetric.FromValue(elapsedMilliseconds, "ms"),
                    BattleBenchmarkMetric.FromValue(allocatedBytes, "bytes"),
                    checksum);
                if (tickIndex <= warmupTickCount)
                    warmupSamples[tickIndex - 1] = tickSample;
                else
                    samples[sampleIndex++] = tickSample;
            }

            string finalPrimary = CaptureRuntimeChecksum(primaryWorld, totalTicks);
            string finalMirror = CaptureRuntimeChecksum(mirrorWorld, totalTicks);
            bool admissionValidated = primaryWorld.ObjectCount == requested &&
                                      mirrorWorld.ObjectCount == requested;
            bool deterministic = admissionValidated &&
                                 string.Equals(initialPrimary, initialMirror, StringComparison.Ordinal) &&
                                 string.Equals(finalPrimary, finalMirror, StringComparison.Ordinal);
            if (!admissionValidated)
            {
                throw new InvalidOperationException(
                    $"Runtime benchmark admission mismatch: requested={requested}, " +
                    $"primary={primaryWorld.ObjectCount}, mirror={mirrorWorld.ObjectCount}.");
            }
            if (!deterministic)
            {
                throw new InvalidOperationException(
                    "The deterministic empty-input runtime fixture produced different checksums in its mirror world.");
            }

            BattlePresentationFrame frame = BuildRuntimeDerivedFrame(primaryWorld, primaryEntities, totalTicks);
            string source = "deterministic-mobileextended-runtime-fixture-v1";

            int actual = frame.EntityCount;
            if (actual != requested)
            {
                throw new InvalidOperationException(
                    $"Benchmark workload count mismatch: requested {requested}, built {actual}.");
            }
            if (frame.CommandCount <= 0)
                throw new InvalidOperationException("Benchmark workload contains no render commands.");
            string fingerprint = ComputeFingerprint(frame, requested, source);
            return new BattleRenderingBenchmarkWorkload(
                frame,
                requested,
                actual,
                fingerprint,
                source,
                primaryWorld.ObjectCount,
                primaryWorld.RuntimeSlotCapacity,
                primaryWorld.RuntimeProfileForServices.ToString(),
                warmupSamples,
                samples,
                inputFingerprint,
                initialPrimary,
                finalPrimary,
                admissionValidated,
                deterministic);
        }

        private static BattlePresentationFrame CloneFrame(BattlePresentationFrame source)
        {
            var frame = new BattlePresentationFrame();
            frame.Reset(source.TickIndex);
            frame.EnsureEntityCapacity(source.EntityCount);
            frame.EnsureHitRecordCapacity(source.HitRecordCount);
            frame.EnsureCommandCapacity(source.CommandCount);
            for (int index = 0; index < source.EntityCount; index++)
                frame.AddEntity(source.GetEntity(index));
            for (int index = 0; index < source.HitRecordCount; index++)
                frame.AddHitRecord(source.GetHitRecord(index));
            for (int index = 0; index < source.CommandCount; index++)
                frame.AddCommand(source.GetCommand(index));
            frame.OverlayUnsupportedCount = source.OverlayUnsupportedCount;
            return frame;
        }

        private static BattleRenderingBenchmarkEntity[] RegisterRuntimeEntities(
            SimulationWorld world,
            int entityCount)
        {
            var entities = new BattleRenderingBenchmarkEntity[entityCount];
            for (int index = 0; index < entityCount; index++)
            {
                int column = index % 40;
                int row = index / 40;
                var entity = new BattleRenderingBenchmarkEntity(
                    index + 1,
                    40 + column * 16,
                    200 + row * 4);
                world.Register(entity);
                if (entity.Runtime.SlotIndex < 50)
                {
                    throw new InvalidOperationException(
                        $"Runtime benchmark fixture {index} was not assigned a valid dynamic slot.");
                }
                entities[index] = entity;
            }
            return entities;
        }

        private static BattlePresentationFrame BuildRuntimeDerivedFrame(
            SimulationWorld world,
            BattleRenderingBenchmarkEntity[] entities,
            int tickIndex)
        {
            var frame = new BattlePresentationFrame();
            frame.Reset(tickIndex);
            frame.EnsureEntityCapacity(entities.Length);
            frame.EnsureCommandCapacity(checked(entities.Length * 2));
            for (int index = 0; index < entities.Length; index++)
            {
                BattleRenderingBenchmarkEntity entity = entities[index];
                if (!world.TryGetCurrentRuntimeHandle(
                        entity.Runtime.SlotIndex,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    throw new InvalidOperationException(
                        $"Runtime benchmark fixture lost its generation-aware handle at index {index}.");
                }
                int stableId = entity.Runtime.StableId;
                int runtimeSlot = entity.Runtime.SlotIndex;
                int logicalZ = entity.Runtime.ZInt;
                Vector3 position = NTSDRenderSpace.ScreenPixelToWorld(
                    entity.Runtime.XInt,
                    logicalZ,
                    logicalZ * 0.001f);
                int baseOrder = checked(index * 4);
                frame.AddEntity(new BattlePresentationEntitySnapshot(
                    handle,
                    stableId,
                    entity.ObjectId,
                    entity.GetCurrentDataObjectTypeForSimulation(),
                    0,
                    logicalZ,
                    runtimeSlot,
                    baseOrder,
                    0,
                    true,
                    0,
                    0,
                    0,
                    0,
                    0,
                    entity.Runtime.XInt,
                    logicalZ,
                    position.z,
                    0f,
                    0,
                    0,
                    8f,
                    8f,
                    16f,
                    16f,
                    Vector2.zero,
                    new Rect(0f, 0f, 1f, 1f),
                    new Vector2(0.5f, 0.5f),
                    (index & 1) != 0,
                    false,
                    default,
                    0,
                    0));
                AddSyntheticCommand(
                    frame,
                    BattleRenderCommandType.Shadow,
                    handle,
                    stableId,
                    runtimeSlot,
                    baseOrder,
                    position + new Vector3(0f, -0.04f, 0f),
                    0,
                    false);
                AddSyntheticCommand(
                    frame,
                    BattleRenderCommandType.Entity,
                    handle,
                    stableId,
                    runtimeSlot,
                    baseOrder,
                    position,
                    1,
                    (index & 1) != 0);
            }
            return frame;
        }

        private static string CaptureRuntimeChecksum(SimulationWorld world, int tickIndex)
        {
            if (world == null)
                return string.Empty;
            FrameInputSet input = FrameInputSet.Empty(tickIndex);
            if (world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended)
            {
                return world.CaptureExtendedChecksumSnapshot(tickIndex, input).OverallChecksum;
            }
            return world.CaptureParityFrameSnapshot(tickIndex, input).OverallChecksum;
        }

        private static string ComputeInputFingerprint(int warmupTickCount, int sampleTickCount)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                Hash(ref hash, warmupTickCount);
                Hash(ref hash, sampleTickCount);
                for (int tickIndex = 1; tickIndex <= warmupTickCount + sampleTickCount; tickIndex++)
                {
                    Hash(ref hash, tickIndex);
                    Hash(ref hash, 0);
                }
                return hash.ToString("x16");
            }
        }

        private static void AddSyntheticCommand(
            BattlePresentationFrame frame,
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int runtimeSlot,
            int baseOrder,
            Vector3 position,
            int localSequence,
            bool flipX)
        {
            frame.AddCommand(new BattleRenderCommand(
                type,
                handle,
                stableId,
                1,
                0,
                runtimeSlot / 40,
                runtimeSlot,
                baseOrder + localSequence,
                0,
                localSequence,
                position,
                new Vector2(16f, 16f),
                new Vector2(0.5f, 0.5f),
                new Rect(0f, 0f, 1f, 1f),
                BattleSpriteRenderState.Default(flipX),
                default));
        }

        private static string ComputeFingerprint(
            BattlePresentationFrame frame,
            int requested,
            string source)
        {
            unchecked
            {
                ulong hash = 1469598103934665603UL;
                Hash(ref hash, requested);
                Hash(ref hash, frame.EntityCount);
                Hash(ref hash, frame.CommandCount);
                for (int index = 0; index < source.Length; index++)
                    Hash(ref hash, source[index]);
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    Hash(ref hash, (int)command.Type);
                    Hash(ref hash, command.Handle.Slot);
                    Hash(ref hash, unchecked((int)command.Handle.Generation));
                    Hash(ref hash, command.StableId);
                    Hash(ref hash, command.RuntimeSlot);
                    Hash(ref hash, command.SortOrder);
                    Hash(ref hash, command.LocalSequence);
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.x));
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.y));
                    Hash(ref hash, BitConverter.SingleToInt32Bits(command.Position.z));
                    Hash(ref hash, command.FlipX ? 1 : 0);
                }
                return hash.ToString("x16");
            }
        }

        private static void Hash(ref ulong hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 1099511628211UL;
            }
        }
    }

    internal sealed class BattleRenderingBenchmarkEntity : LF2Entity
    {
        public BattleRenderingBenchmarkEntity(int stableId, int x, int z)
        {
            StableId = stableId;
            ObjectId = 10000 + stableId;
            Team = 0;
            Health = new LF2Health();
            Health.BindRuntime(Runtime);
            Health.HP = 500;
            Health.HPBound = 500;
            ItrRest = new LF2ItrRestTracker();
            PS.BindRuntime(Runtime);
            Trans = new FrameTransistor(this);
            Frame.D = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                pic = 999,
                wait = 1000000,
                next = 0,
                centerx = 8,
                centery = 8,
            };
            Frame.N = 0;
            Frame.PN = 0;
            Frame.Prev = 0;
            Runtime.X = x;
            Runtime.Y = 0;
            Runtime.Z = z;
            Runtime.SuppressCollisionCandidateUntilTick = int.MaxValue;
            Runtime.SyncIntegerPosition();
            RefreshRuntimeSnapshot();
        }

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;

        internal override bool UsesDynamicRuntimeSlot() => true;

        public override int GetCurrentDataObjectTypeForSimulation() => (int)LF2ObjectType.Other;

        public override void RunFrameLogicBeforeAdvance()
        {
        }

        public override void SimTransit(int tickIndex)
        {
        }

        public override void SimTU(int tickIndex)
        {
        }

        public override void SimPostInteraction(int tickIndex)
        {
        }

        public override void SimObjectInteraction(int tickIndex)
        {
        }

        public override void SimPreInteraction(int tickIndex)
        {
        }

        public override void SimEntityCollision(int tickIndex)
        {
        }

        public override void SimFrameTick(int tickIndex)
        {
        }

        public override void SimLateTick(int tickIndex)
        {
        }

        public override void Reset()
        {
        }

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
        }
    }

    public readonly struct BattleBenchmarkCompletedFrameMetrics
    {
        public BattleBenchmarkCompletedFrameMetrics(
            BattleBenchmarkMetric frameTimeMs,
            BattleBenchmarkMetric mainThreadTimeMs,
            BattleBenchmarkMetric renderThreadTimeMs,
            BattleBenchmarkMetric gpuFrameTimeMs,
            BattleBenchmarkMetric managedAllocationBytes,
            BattleBenchmarkMetric drawCalls,
            BattleBenchmarkMetric totalAllocatedMemoryBytes,
            BattleBenchmarkMetric graphicsMemoryBytes)
        {
            FrameTimeMs = frameTimeMs;
            MainThreadTimeMs = mainThreadTimeMs;
            RenderThreadTimeMs = renderThreadTimeMs;
            GpuFrameTimeMs = gpuFrameTimeMs;
            ManagedAllocationBytes = managedAllocationBytes;
            DrawCalls = drawCalls;
            TotalAllocatedMemoryBytes = totalAllocatedMemoryBytes;
            GraphicsMemoryBytes = graphicsMemoryBytes;
        }

        public BattleBenchmarkMetric FrameTimeMs { get; }
        public BattleBenchmarkMetric MainThreadTimeMs { get; }
        public BattleBenchmarkMetric RenderThreadTimeMs { get; }
        public BattleBenchmarkMetric GpuFrameTimeMs { get; }
        public BattleBenchmarkMetric ManagedAllocationBytes { get; }
        public BattleBenchmarkMetric DrawCalls { get; }
        public BattleBenchmarkMetric TotalAllocatedMemoryBytes { get; }
        public BattleBenchmarkMetric GraphicsMemoryBytes { get; }

        internal static BattleBenchmarkCompletedFrameMetrics Unavailable()
        {
            return new BattleBenchmarkCompletedFrameMetrics(
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("ms"),
                BattleBenchmarkMetric.Unavailable("bytes"),
                BattleBenchmarkMetric.Unavailable("count"),
                BattleBenchmarkMetric.Unavailable("bytes"),
                BattleBenchmarkMetric.Unavailable("bytes"));
        }
    }

    public interface IBattleBenchmarkCompletedFrameCollector : IDisposable
    {
        bool IsSupported { get; }
        string UnsupportedReason { get; }
        void Request(int generation);
        bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics);
        string Source(BattleBenchmarkRecorderKind kind);
        string Reason(BattleBenchmarkRecorderKind kind);
        void Reset();
    }

    public sealed class BattleBenchmarkInjectedCompletedFrameCollector :
        IBattleBenchmarkCompletedFrameCollector
    {
        private readonly BattleBenchmarkCompletedFrameMetrics metrics;
        private int pendingGeneration;

        public BattleBenchmarkInjectedCompletedFrameCollector(
            BattleBenchmarkCompletedFrameMetrics completedFrameMetrics)
        {
            metrics = completedFrameMetrics;
        }

        public bool IsSupported => true;
        public string UnsupportedReason => string.Empty;

        public void Request(int generation)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
        }

        public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics result)
        {
            if (pendingGeneration != generation)
            {
                result = default;
                return false;
            }
            pendingGeneration = 0;
            result = metrics;
            return true;
        }

        public string Source(BattleBenchmarkRecorderKind kind) => "injected-completed-frame-test-sample";
        public string Reason(BattleBenchmarkRecorderKind kind) => string.Empty;
        public void Reset() => pendingGeneration = 0;
        public void Dispose() => Reset();
    }

    public interface IBattleRenderingBenchmarkRunSession : IDisposable
    {
        bool CaptureFrame();
        BattleRenderingBenchmarkReport Report { get; }
    }

    public interface IBattleBenchmarkLeakProbe
    {
        long CaptureRetainedManagedHeapBytes();
        BattleBenchmarkMetric CaptureGraphicsMemory();
        int CurrentUnityFrame { get; }
        bool RequiresDeferredDestructionWait { get; }
        void BeginPostDisposeCleanup();
        bool IsPostDisposeCleanupComplete { get; }
        void CompletePostDisposeCleanup();
    }

    internal sealed class BattleBenchmarkUnityLeakProbe : IBattleBenchmarkLeakProbe
    {
        private AsyncOperation postDisposeUnload;

        public int CurrentUnityFrame => Time.frameCount;
        public bool RequiresDeferredDestructionWait => Application.isPlaying;
        public bool IsPostDisposeCleanupComplete =>
            postDisposeUnload == null || postDisposeUnload.isDone;

        public long CaptureRetainedManagedHeapBytes()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            return GC.GetTotalMemory(false);
        }

        public BattleBenchmarkMetric CaptureGraphicsMemory()
        {
            return BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
        }

        public void BeginPostDisposeCleanup()
        {
            if (!Application.isPlaying)
                return;
            GL.Flush();
            postDisposeUnload = Resources.UnloadUnusedAssets();
        }

        public void CompletePostDisposeCleanup()
        {
            if (Application.isPlaying)
                GL.Flush();
        }
    }

    public sealed class BattleRenderingBenchmarkSession : IBattleRenderingBenchmarkRunSession
    {
        private readonly BattleRenderingBenchmarkConfig config;
        private readonly SimulationWorld world;
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleRenderingBenchmarkPolicyContext policyContext;
        private readonly IBattleBenchmarkCompletedFrameCollector completedFrameCollector;
        private readonly IBattleRenderingBenchmarkPresenter presenter;
        private readonly IBattleBenchmarkLeakProbe leakProbe;
        private readonly List<BattleRenderingBenchmarkFrame> frames;
        private readonly string presenterImplementation;
        private readonly string presenterResourceMode;
        private readonly string presenterDrawMode;
        private readonly string presenterSubmissionDrawMetricSource;
        private readonly string presenterSubmissionDrawUnavailableReason;
        private readonly int presenterRenderTargetWidth;
        private readonly int presenterRenderTargetHeight;
        private readonly int presenterResourceGeneration;
        private readonly int presenterOwnedTextureResourceCount;
        private static int nextGeneration;
        private BattleRenderingBenchmarkReport report;
        private BattleRenderingBenchmarkLeakReport leakReport;
        private bool disposed;
        private int frameIndex;
        private int pendingGeneration;
        private int pendingSampleIndex;
        private bool pendingWarmup;
        private bool pendingLeakFrame;
        private int pendingSampleAttempt;
        private double pendingPresentationMs;
        private int completedFrameRejectedAttemptCount;
        private string completedFrameSamplingFailureReason = string.Empty;
        private int leakFramesCaptured;
        private bool leakBaselineCaptured;
        private bool presenterDisposed;
        private bool teardownPending;
        private int teardownStartedFrame;
        private int teardownFramesWaited;
        private bool teardownCleanupRequested;
        private bool teardownCleanupCompleted;
        private int teardownCleanupCompletedFrame;
        private long leakPrePresenterManaged;
        private long leakPrePresenterGraphics;
        private bool leakPrePresenterGraphicsAvailable;
        private long leakManagedStart;
        private long leakGraphicsStart;
        private bool leakGraphicsAvailable;
        private long leakOwnedStart;
        private int leakResourcesStart;
        private long leakManagedEnd;
        private long leakGraphicsEnd;
        private bool leakSoakGraphicsAvailable;
        private long leakOwnedEnd;
        private int leakResourcesEnd;

        public const string RetainedManagedHeapMeasurementMode =
            "full-gc-retained-managed-heap-outside-performance-sample-window-v1";
        public const int DeferredDestructionPlayFrames = 2;
        public const int PostDisposeCleanupPlayFrames = 2;
        public const int MaxPostDisposeCleanupPlayFrames = 120;
        public const int MaxCompletedFrameSampleAttempts = 16;

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world)
            : this(
                config,
                world,
                BattleRenderingBenchmarkWorkload.Create(
                    config.Scenario,
                    world,
                    config.WarmupFrames,
                    config.SampleFrames))
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload)
            : this(
                config,
                world,
                workload,
                BattleRenderingBenchmarkPolicyContext.Capture(),
                null,
                null)
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload,
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext,
            IBattleBenchmarkCompletedFrameCollector collector,
            IBattleRenderingBenchmarkPresenter benchmarkPresenter)
            : this(
                config,
                world,
                workload,
                benchmarkPolicyContext,
                collector,
                benchmarkPresenter,
                null)
        {
        }

        public BattleRenderingBenchmarkSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            BattleRenderingBenchmarkWorkload workload,
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext,
            IBattleBenchmarkCompletedFrameCollector collector,
            IBattleRenderingBenchmarkPresenter benchmarkPresenter,
            IBattleBenchmarkLeakProbe benchmarkLeakProbe)
        {
            if (config.Comparison != BattleRenderingBenchmarkComparison.Single)
                throw new ArgumentException("A single run session requires Single comparison mode.", nameof(config));
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            policyContext = benchmarkPolicyContext;
            leakProbe = benchmarkLeakProbe ?? new BattleBenchmarkUnityLeakProbe();
            ValidateCount();
            frames = new List<BattleRenderingBenchmarkFrame>(config.SampleFrames);
            completedFrameCollector = collector ??
                new BattleBenchmarkUnityCompletedFrameCollector(policyContext);
            IBattleRenderingBenchmarkPresenter presenterCandidate = benchmarkPresenter;
            try
            {
                if (config.LeakCheckFrames > 0)
                    CapturePrePresenterLeakBaseline();
                presenterCandidate = presenterCandidate ??
                                     BattleRenderingBenchmarkPresenterFactory.Create(config.Backend, workload);
                presenter = presenterCandidate;
                ValidatePresenterWorkload();
                presenterImplementation = presenter.Implementation;
                presenterResourceMode = presenter.ResourceMode;
                presenterDrawMode = presenter.DrawMode;
                presenterSubmissionDrawMetricSource = presenter.SubmissionDrawMetricSource;
                presenterSubmissionDrawUnavailableReason = presenter.SubmissionDrawUnavailableReason;
                presenterRenderTargetWidth = presenter.RenderTargetWidth;
                presenterRenderTargetHeight = presenter.RenderTargetHeight;
                presenterResourceGeneration = presenter.ResourceGeneration;
                presenterOwnedTextureResourceCount = presenter.OwnedTextureResourceCount;
            }
            catch
            {
                try
                {
                    presenterCandidate?.Dispose();
                }
                catch (Exception cleanupException)
                {
                    UnityEngine.Debug.LogException(cleanupException);
                }
                finally
                {
                    completedFrameCollector.Dispose();
                }
                throw;
            }
        }

        public BattleRenderingBenchmarkConfig Config => config;
        public bool IsComplete => report != null;
        public BattleRenderingBenchmarkReport Report => report;
        public bool IsDisposed => disposed;
        public int WarmupFramesCaptured => Math.Min(frameIndex, config.WarmupFrames);
        public int SampleFramesCaptured => frames.Count;
        public int LeakFramesCaptured => leakFramesCaptured;
        public BattleRenderingBenchmarkWorkload Workload => workload;

        public bool CaptureFrame()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleRenderingBenchmarkSession));
            if (report != null)
                return true;

            if (pendingGeneration != 0)
            {
                if (!completedFrameCollector.TryDrain(
                        pendingGeneration,
                        out BattleBenchmarkCompletedFrameMetrics completedMetrics))
                {
                    return false;
                }

                bool completedLeakFrame = pendingLeakFrame;
                if (completedLeakFrame)
                    leakFramesCaptured++;
                else if (!pendingWarmup)
                {
                    BattleRenderingBenchmarkFrame sample =
                        CaptureSample(pendingSampleIndex, pendingPresentationMs, completedMetrics);
                    if (!HasAllApplicableFormalSampleMetrics(sample, out string rejectionReason))
                    {
                        int rejectedGeneration = pendingGeneration;
                        completedFrameRejectedAttemptCount++;
                        pendingGeneration = 0;
                        pendingWarmup = false;
                        pendingLeakFrame = false;
                        if (pendingSampleAttempt < MaxCompletedFrameSampleAttempts)
                        {
                            pendingSampleAttempt++;
                            BeginCompletedFrameRequest();
                            return false;
                        }

                        completedFrameSamplingFailureReason =
                            $"Formal sample {pendingSampleIndex} exhausted {MaxCompletedFrameSampleAttempts} " +
                            $"completed-frame attempts; last generation {rejectedGeneration}: {rejectionReason}";
                        leakReport = BattleRenderingBenchmarkLeakReport.NotRun(
                            "Leak/long-run soak was not run because " + completedFrameSamplingFailureReason);
                        FinalizeReport();
                        return true;
                    }
                    frames.Add(sample);
                    pendingSampleAttempt = 0;
                }
                pendingGeneration = 0;
                pendingWarmup = false;
                pendingLeakFrame = false;

                if (completedLeakFrame)
                {
                    if (leakFramesCaptured < config.LeakCheckFrames)
                        return false;
                    BeginLeakTeardown();
                    return false;
                }

                if (frames.Count < config.SampleFrames)
                    return false;
                if (config.LeakCheckFrames <= 0)
                {
                    leakReport = BattleRenderingBenchmarkLeakReport.NotRequested();
                    FinalizeReport();
                    return true;
                }
                CaptureLeakBaseline();
                return false;
            }

            if (teardownPending)
            {
                teardownFramesWaited = Math.Max(0, leakProbe.CurrentUnityFrame - teardownStartedFrame);
                if (leakProbe.RequiresDeferredDestructionWait &&
                    teardownFramesWaited < DeferredDestructionPlayFrames)
                {
                    return false;
                }

                if (!teardownCleanupRequested)
                {
                    teardownCleanupRequested = true;
                    leakProbe.BeginPostDisposeCleanup();
                    return false;
                }

                if (!leakProbe.IsPostDisposeCleanupComplete)
                {
                    if (leakProbe.RequiresDeferredDestructionWait &&
                        teardownFramesWaited >= MaxPostDisposeCleanupPlayFrames)
                    {
                        FinalizeLeakReport(
                            "Post-Dispose Unity cleanup did not complete within " +
                            MaxPostDisposeCleanupPlayFrames + " Play frames.");
                        FinalizeReport();
                        return true;
                    }
                    return false;
                }

                if (!teardownCleanupCompleted)
                {
                    teardownCleanupCompleted = true;
                    teardownCleanupCompletedFrame = leakProbe.CurrentUnityFrame;
                    leakProbe.CompletePostDisposeCleanup();
                    return false;
                }

                if (leakProbe.RequiresDeferredDestructionWait &&
                    leakProbe.CurrentUnityFrame - teardownCleanupCompletedFrame <
                    PostDisposeCleanupPlayFrames)
                {
                    return false;
                }

                FinalizeLeakReport();
                FinalizeReport();
                return true;
            }

            if (frames.Count < config.SampleFrames)
            {
                ValidateCount();
                int currentFrame = frameIndex;
                pendingWarmup = currentFrame < config.WarmupFrames;
                pendingSampleIndex = frames.Count;
                pendingLeakFrame = false;
                pendingSampleAttempt = 1;
                BeginCompletedFrameRequest();
                frameIndex++;
                return false;
            }

            if (!leakBaselineCaptured)
                CaptureLeakBaseline();
            ValidateCount();
            pendingWarmup = false;
            pendingLeakFrame = true;
            pendingSampleAttempt = 0;
            BeginCompletedFrameRequest();
            return false;
        }

        private void BeginCompletedFrameRequest()
        {
            pendingGeneration = Interlocked.Increment(ref nextGeneration);
            if (pendingGeneration == 0)
                pendingGeneration = Interlocked.Increment(ref nextGeneration);
            try
            {
                completedFrameCollector.Request(pendingGeneration);
                pendingPresentationMs = presenter.Present();
                ValidatePresenterWorkload();
            }
            catch
            {
                completedFrameCollector.Reset();
                pendingGeneration = 0;
                pendingWarmup = false;
                pendingLeakFrame = false;
                pendingSampleAttempt = 0;
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            try
            {
                DisposePresenter();
            }
            finally
            {
                completedFrameCollector.Dispose();
            }
        }

        private BattleRenderingBenchmarkFrame CaptureSample(
            int index,
            double presentationMs,
            BattleBenchmarkCompletedFrameMetrics completedMetrics)
        {
            BattleCentralBuildDiagnostics diagnostics = presenter.Diagnostics;
            BattleBenchmarkMetric logicTickTime;
            BattleBenchmarkMetric logicTickAllocatedBytes;
            if (index < workload.LogicTickSamples.Count)
            {
                BattleRenderingBenchmarkLogicTickSample logicSample = workload.LogicTickSamples[index];
                logicTickTime = logicSample.ElapsedMilliseconds;
                logicTickAllocatedBytes = logicSample.AllocatedBytes;
            }
            else
            {
                logicTickTime = BattleBenchmarkMetric.Unavailable("ms");
                logicTickAllocatedBytes = BattleBenchmarkMetric.Unavailable("bytes");
            }
            var frame = new BattleRenderingBenchmarkFrame(
                index,
                workload.ActualEntityCount,
                workload.CommandCount)
            {
                FrameTimeMs = completedMetrics.FrameTimeMs,
                MainThreadTimeMs = completedMetrics.MainThreadTimeMs,
                RenderThreadTimeMs = completedMetrics.RenderThreadTimeMs,
                GpuFrameTimeMs = completedMetrics.GpuFrameTimeMs,
                LogicTickTimeMs = logicTickTime,
                LogicTickAllocatedBytes = logicTickAllocatedBytes,
                LogicTickChecksum = index < workload.LogicTickSamples.Count
                    ? workload.LogicTickSamples[index].Checksum
                    : string.Empty,
                PresentationBuildTimeMs = BattleBenchmarkMetric.FromValue(presentationMs, "ms"),
                ManagedAllocationBytes = completedMetrics.ManagedAllocationBytes,
                DrawCalls = BattleBenchmarkDrawCallPolicy.RequirePositiveForNonEmptyWorkload(
                    completedMetrics.DrawCalls),
                PresenterSubmittedRenderItems = BattleBenchmarkMetric.FromValue(
                    presenter.MaterializedRenderItemCount,
                    "count"),
                PresenterSubmissionDrawCalls = presenter.SubmissionDrawCount >= 0
                    ? BattleBenchmarkMetric.FromValue(presenter.SubmissionDrawCount, "count")
                    : BattleBenchmarkMetric.Unavailable("count"),
                TotalAllocatedMemoryBytes = completedMetrics.TotalAllocatedMemoryBytes,
                GraphicsMemoryBytes = completedMetrics.GraphicsMemoryBytes,
                BenchmarkOwnedTextureMemoryBytes = BattleBenchmarkOwnedTextureMemoryPolicy.Capture(
                    presenterResourceGeneration,
                    presenterOwnedTextureResourceCount,
                    presenter.MeasureOwnedTextureMemoryBytes(),
                    out _),
                BenchmarkOwnedMemoryBytes = BattleBenchmarkMetric.FromValue(
                    presenter.CachedOwnedResourceMemoryBytes,
                    "bytes"),
                BenchmarkResourceGeneration = presenterResourceGeneration,
                SourceCommands = BattleBenchmarkMetric.FromValue(workload.CommandCount, "count"),
                ResolvedCommands = BattleBenchmarkMetric.FromValue(presenter.ResolvedCommandCount, "count"),
                UnresolvedCommands = BattleBenchmarkMetric.FromValue(
                    workload.CommandCount - presenter.ResolvedCommandCount,
                    "count"),
                ResourceSegments = BattleBenchmarkMetric.FromValue(presenter.ResourceSegmentCount, "count"),
                MeshChunks = diagnostics == null
                    ? BattleBenchmarkMetric.Unavailable("count")
                    : BattleBenchmarkMetric.FromValue(diagnostics.ActiveChunkCount, "count"),
                RequestedBackend = config.Backend.ToString(),
                EffectiveBackend = presenter.EffectiveBackend,
            };
            return frame;
        }

        private bool HasAllApplicableFormalSampleMetrics(
            BattleRenderingBenchmarkFrame frame,
            out string reason)
        {
            if (!completedFrameCollector.IsSupported)
            {
                reason = string.Empty;
                return true;
            }
            var missing = new List<string>();
            AddMissingFormalSampleMetric(missing, "frameTimeMs", frame.FrameTimeMs);
            AddMissingFormalSampleMetric(missing, "mainThreadTimeMs", frame.MainThreadTimeMs);
            if (policyContext.GraphicsMultiThreaded)
                AddMissingFormalSampleMetric(missing, "renderThreadTimeMs", frame.RenderThreadTimeMs);
            AddMissingFormalSampleMetric(missing, "gpuFrameTimeMs", frame.GpuFrameTimeMs);
            AddMissingFormalSampleMetric(missing, "managedAllocationBytes", frame.ManagedAllocationBytes);
            AddMissingFormalSampleMetric(missing, "drawCalls", frame.DrawCalls);
            AddMissingFormalSampleMetric(missing, "totalAllocatedMemoryBytes", frame.TotalAllocatedMemoryBytes);
            AddMissingFormalSampleMetric(missing, "graphicsMemoryBytes", frame.GraphicsMemoryBytes);
            AddMissingFormalSampleMetric(
                missing,
                "benchmarkOwnedTextureMemoryBytes",
                frame.BenchmarkOwnedTextureMemoryBytes);
            AddMissingFormalSampleMetric(missing, "presentationBuildTimeMs", frame.PresentationBuildTimeMs);
            AddMissingFormalSampleMetric(
                missing,
                "presenterSubmittedRenderItems",
                frame.PresenterSubmittedRenderItems);
            AddMissingFormalSampleMetric(missing, "resourceSegments", frame.ResourceSegments);
            AddMissingFormalSampleMetric(missing, "benchmarkOwnedMemoryBytes", frame.BenchmarkOwnedMemoryBytes);
            if (config.Backend == BattlePresentationBackendMode.CentralOnly)
            {
                AddMissingFormalSampleMetric(
                    missing,
                    "presenterSubmissionDrawCalls",
                    frame.PresenterSubmissionDrawCalls);
                AddMissingFormalSampleMetric(missing, "meshChunks", frame.MeshChunks);
            }

            if (missing.Count == 0)
            {
                reason = string.Empty;
                return true;
            }

            reason = "required applicable metrics unavailable: " + string.Join(", ", missing) + ".";
            return false;
        }

        private static void AddMissingFormalSampleMetric(
            List<string> missing,
            string name,
            BattleBenchmarkMetric metric)
        {
            if (!metric.Available)
                missing.Add(name);
        }

        private void CaptureLeakBaseline()
        {
            leakBaselineCaptured = true;
            leakManagedStart = leakProbe.CaptureRetainedManagedHeapBytes();
            leakOwnedStart = presenter.MeasureOwnedResourceMemoryBytes();
            leakResourcesStart = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakGraphicsAvailable = graphics.Available;
            leakGraphicsStart = graphics.Available ? (long)graphics.Value : 0L;
        }

        private void CapturePrePresenterLeakBaseline()
        {
            leakPrePresenterManaged = leakProbe.CaptureRetainedManagedHeapBytes();
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakPrePresenterGraphicsAvailable = graphics.Available;
            leakPrePresenterGraphics = graphics.Available ? (long)graphics.Value : 0L;
        }

        private void BeginLeakTeardown()
        {
            leakManagedEnd = leakProbe.CaptureRetainedManagedHeapBytes();
            leakOwnedEnd = presenter.MeasureOwnedResourceMemoryBytes();
            leakResourcesEnd = presenter.OwnedResourceCount;
            BattleBenchmarkMetric graphics = leakProbe.CaptureGraphicsMemory();
            leakSoakGraphicsAvailable = leakGraphicsAvailable && graphics.Available;
            leakGraphicsEnd = leakSoakGraphicsAvailable ? (long)graphics.Value : 0L;
            DisposePresenter();
            teardownPending = true;
            teardownStartedFrame = leakProbe.CurrentUnityFrame;
            teardownCleanupRequested = false;
            teardownCleanupCompleted = false;
            teardownCleanupCompletedFrame = 0;
        }

        private void FinalizeLeakReport(string teardownCleanupFailureReason = null)
        {
            long teardownManagedEnd = leakProbe.CaptureRetainedManagedHeapBytes();
            BattleBenchmarkMetric teardownGraphics = leakProbe.CaptureGraphicsMemory();
            bool teardownGraphicsAvailable = leakPrePresenterGraphicsAvailable &&
                                             teardownGraphics.Available;
            long teardownGraphicsEnd = teardownGraphicsAvailable
                ? (long)teardownGraphics.Value
                : 0L;
            long teardownOwnedEnd = presenter.MeasureOwnedResourceMemoryBytes();
            int teardownResourcesEnd = presenter.OwnedResourceCount;

            long managedGrowth = leakManagedEnd - leakManagedStart;
            long graphicsGrowth = leakGraphicsEnd - leakGraphicsStart;
            bool soakPassed = leakSoakGraphicsAvailable &&
                              managedGrowth <= config.MaxManagedGrowthBytes &&
                              graphicsGrowth <= config.MaxGraphicsGrowthBytes &&
                              leakOwnedEnd <= leakOwnedStart &&
                              leakResourcesEnd <= leakResourcesStart;
            // The pre-presenter values expose one-time pipeline initialization, while the
            // post-Dispose gate detects retained growth relative to the steady-state soak baseline.
            long teardownManagedGrowth = teardownManagedEnd - leakManagedStart;
            long teardownGraphicsGrowth = teardownGraphicsEnd - leakGraphicsStart;
            bool teardownPassed = string.IsNullOrEmpty(teardownCleanupFailureReason) &&
                                  teardownGraphicsAvailable &&
                                  teardownManagedGrowth <= config.MaxManagedGrowthBytes &&
                                  teardownGraphicsGrowth <= config.MaxGraphicsGrowthBytes &&
                                  teardownOwnedEnd == 0L &&
                                  teardownResourcesEnd == 0;
            BattleBenchmarkMetricStatus teardownStatus = !teardownGraphicsAvailable
                ? BattleBenchmarkMetricStatus.Missing
                : teardownPassed
                    ? BattleBenchmarkMetricStatus.Passed
                    : BattleBenchmarkMetricStatus.Failed;
            string teardownReason = !string.IsNullOrEmpty(teardownCleanupFailureReason)
                ? teardownCleanupFailureReason
                : !teardownGraphicsAvailable
                ? "Post-Dispose graphics memory evidence is required but unavailable."
                : teardownPassed
                    ? "Post-Dispose ownership returned to zero and retained managed/graphics memory returned within steady-state thresholds after bounded Unity cleanup."
                    : "Post-Dispose ownership remained nonzero or retained managed/graphics memory exceeded a steady-state threshold after bounded Unity cleanup.";
            bool passed = soakPassed && teardownPassed;
            string reason = !leakSoakGraphicsAvailable
                ? "Steady-state graphics memory evidence is required but unavailable."
                : passed
                    ? "Steady-state soak and post-Dispose teardown both passed."
                    : "Steady-state soak or post-Dispose teardown failed: " + teardownReason;
            leakReport = new BattleRenderingBenchmarkLeakReport(
                true,
                passed,
                leakFramesCaptured,
                leakPrePresenterManaged,
                leakPrePresenterGraphics,
                leakPrePresenterGraphicsAvailable,
                leakManagedStart,
                leakManagedEnd,
                leakGraphicsStart,
                leakGraphicsEnd,
                leakSoakGraphicsAvailable,
                leakOwnedStart,
                leakOwnedEnd,
                leakResourcesStart,
                leakResourcesEnd,
                config.MaxManagedGrowthBytes,
                config.MaxGraphicsGrowthBytes,
                teardownFramesWaited,
                teardownManagedEnd,
                teardownGraphicsEnd,
                teardownGraphicsAvailable,
                teardownOwnedEnd,
                teardownResourcesEnd,
                teardownStatus,
                teardownReason,
                RetainedManagedHeapMeasurementMode,
                reason);
            teardownPending = false;
        }

        private void DisposePresenter()
        {
            if (presenterDisposed)
                return;
            presenterDisposed = true;
            presenter.Dispose();
        }

        private void FinalizeReport()
        {
            bool logicTickMetricsValidated = ValidateLogicTickMetrics();
            BattleBenchmarkMetricAvailability[] metricAvailability = BuildMetricAvailability();
            report = new BattleRenderingBenchmarkReport(
                config,
                frames.ToArray(),
                metricAvailability,
                policyContext,
                workload.RequestedEntityCount,
                workload.ActualEntityCount,
                workload.CommandCount,
                workload.Fingerprint,
                presenterImplementation,
                presenterResourceMode,
                presenterDrawMode,
                presenterRenderTargetWidth,
                presenterRenderTargetHeight,
                true,
                workload.RuntimeAdmissionValidated,
                logicTickMetricsValidated,
                workload.RuntimeStateDeterministic,
                true,
                leakReport);
            report.RuntimeObjectCount = workload.RuntimeObjectCount;
            report.RuntimeSlotCapacity = workload.RuntimeSlotCapacity;
            report.RuntimeProfile = workload.RuntimeProfile;
            report.WarmupLogicTickCount = workload.WarmupTickCount;
            report.SampleLogicTickCount = workload.SampleTickCount;
            report.InputFingerprint = workload.InputFingerprint;
            report.InitialRuntimeChecksum = workload.InitialRuntimeChecksum;
            report.FinalRuntimeChecksum = workload.FinalRuntimeChecksum;
            report.WarmupLogicTickSamples = workload.WarmupLogicTickSamples;
            report.SampleLogicTickSamples = workload.LogicTickSamples;
            report.CompletedFrameRejectedAttemptCount = completedFrameRejectedAttemptCount;
            report.MaxCompletedFrameSampleAttempts = MaxCompletedFrameSampleAttempts;
            report.CompletedFrameSamplingFailureReason = completedFrameSamplingFailureReason;
        }

        private BattleBenchmarkMetricAvailability[] BuildMetricAvailability()
        {
            var result = new List<BattleBenchmarkMetricAvailability>(24);
            AddFrameMetric(result, "frameTimeMs", BattleBenchmarkRecorderKind.FrameTime, frame => frame.FrameTimeMs);
            AddFrameMetric(result, "mainThreadTimeMs", BattleBenchmarkRecorderKind.MainThread, frame => frame.MainThreadTimeMs);
            AddFrameMetric(
                result,
                "renderThreadTimeMs",
                BattleBenchmarkRecorderKind.RenderThread,
                frame => frame.RenderThreadTimeMs,
                policyContext.GraphicsMultiThreaded);
            AddFrameMetric(result, "gpuFrameTimeMs", BattleBenchmarkRecorderKind.GpuFrame, frame => frame.GpuFrameTimeMs);
            AddFrameMetric(result, "managedAllocationBytes", BattleBenchmarkRecorderKind.ManagedAllocation, frame => frame.ManagedAllocationBytes);
            AddFrameMetric(
                result,
                "drawCalls",
                BattleBenchmarkRecorderKind.DrawCalls,
                frame => frame.DrawCalls,
                unavailableReason: "A positive completed-frame draw-call count is required for this non-empty benchmark render workload.");
            AddFrameMetric(result, "totalAllocatedMemoryBytes", BattleBenchmarkRecorderKind.TotalMemory, frame => frame.TotalAllocatedMemoryBytes);
            AddFrameMetric(result, "graphicsMemoryBytes", BattleBenchmarkRecorderKind.GraphicsMemory, frame => frame.GraphicsMemoryBytes);
            AddLocalMetric(
                result,
                "benchmarkOwnedTextureMemoryBytes",
                "benchmark-owned-textures",
                frame => frame.BenchmarkOwnedTextureMemoryBytes,
                BenchmarkOwnedTextureMemorySource(),
                unavailableReason: BenchmarkOwnedTextureMemoryUnavailableReason());
            AddLocalMetric(result, "logicTickTimeMs", "logic-tick", frame => frame.LogicTickTimeMs,
                "Stopwatch around full NTSDBattleTickSystem.RunReleaseTick");
            AddLocalMetric(result, "logicTickAllocatedBytes", "logic-tick", frame => frame.LogicTickAllocatedBytes,
                "GC.GetAllocatedBytesForCurrentThread around full NTSDBattleTickSystem.RunReleaseTick");
            AddLocalMetric(result, "presentationBuildTimeMs", "presenter-local", frame => frame.PresentationBuildTimeMs,
                "Stopwatch around benchmark presenter update/build");
            AddLocalMetric(result, "presenterSubmittedRenderItems", "presenter-local", frame => frame.PresenterSubmittedRenderItems,
                "Validated frozen render-command/materializer count");
            AddLocalMetric(result, "resourceSegments", "presenter-local", frame => frame.ResourceSegments,
                "Presenter resource compatibility grouping");
            AddLocalMetric(result, "benchmarkOwnedMemoryBytes", "presenter-local", frame => frame.BenchmarkOwnedMemoryBytes,
                "Profiler.GetRuntimeMemorySizeLong over benchmark-owned resources");

            bool central = config.Backend == BattlePresentationBackendMode.CentralOnly;
            AddLocalMetric(
                result,
                "presenterSubmissionDrawCalls",
                "presenter-local",
                frame => frame.PresenterSubmissionDrawCalls,
                presenterSubmissionDrawMetricSource,
                central,
                presenterSubmissionDrawUnavailableReason);
            AddLocalMetric(
                result,
                "meshChunks",
                "presenter-local",
                frame => frame.MeshChunks,
                central ? "BattleDynamicMeshBackend diagnostics" : "not applicable",
                central,
                "Legacy compatibility presentation does not build central mesh chunks.");

            bool? exactSampleCount = string.IsNullOrEmpty(completedFrameSamplingFailureReason)
                ? frames.Count == config.SampleFrames
                : (bool?)null;
            AddGate(result, "exactSampleCount", exactSampleCount,
                frames.Count,
                config.SampleFrames,
                "completed-frame collector");
            AddGate(result, "countValidated", workload.ActualEntityCount == workload.RequestedEntityCount,
                1, 1, "frozen workload entity counts");
            AddGate(result, "runtimeAdmissionValidated", workload.RuntimeAdmissionValidated,
                1, 1, "SimulationWorld runtime admission");
            AddGate(result, "determinismValidated",
                config.Scenario.UsesCurrentScene ? (bool?)null : workload.RuntimeStateDeterministic,
                1, 1, "runtime checksum replay");
            AddGate(result, "rendererWorkloadValidated", true,
                1, 1, "presenter materialization validation");
            if (config.LeakCheckFrames > 0)
            {
                BattleBenchmarkMetricStatus leakStatus = !leakReport.GraphicsAvailable ||
                                                         leakReport.TeardownStatus == BattleBenchmarkMetricStatus.Missing
                    ? BattleBenchmarkMetricStatus.Missing
                    : leakReport.Passed
                        ? BattleBenchmarkMetricStatus.Passed
                        : BattleBenchmarkMetricStatus.Failed;
                result.Add(new BattleBenchmarkMetricAvailability(
                    "leakCheck",
                    true,
                    BattleBenchmarkMetricApplicability.Applicable,
                    leakStatus,
                    "long-run",
                    leakReport.Available && leakReport.GraphicsAvailable &&
                    leakReport.TeardownStatus != BattleBenchmarkMetricStatus.Missing ? 1 : 0,
                    1,
                    RetainedManagedHeapMeasurementMode,
                    leakReport.Reason));
            }
            else
            {
                result.Add(new BattleBenchmarkMetricAvailability(
                    "leakCheck",
                    false,
                    BattleBenchmarkMetricApplicability.NotApplicable,
                    BattleBenchmarkMetricStatus.NotApplicable,
                    "long-run",
                    0,
                    0,
                    "not requested",
                    "Leak/long-run soak and teardown were not requested."));
            }
            return result.ToArray();
        }

        private string BenchmarkOwnedTextureMemorySource()
        {
            return "Profiler.GetRuntimeMemorySizeLong summed over " +
                   presenterOwnedTextureResourceCount +
                   " Texture2D/RenderTexture objects owned by benchmark resource generation " +
                   presenterResourceGeneration + ".";
        }

        private string BenchmarkOwnedTextureMemoryUnavailableReason()
        {
            return "No positive runtime-memory sample was observed for the " +
                   presenterOwnedTextureResourceCount +
                   " Texture2D/RenderTexture objects owned by benchmark resource generation " +
                   presenterResourceGeneration + ".";
        }

        private void AddFrameMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            BattleBenchmarkRecorderKind kind,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            bool applicable = true,
            string unavailableReason = "")
        {
            AddMetric(
                result,
                name,
                "completed-frame",
                selector,
                completedFrameCollector.Source(kind),
                required: applicable,
                applicable,
                string.IsNullOrWhiteSpace(completedFrameCollector.Reason(kind))
                    ? unavailableReason
                    : completedFrameCollector.Reason(kind));
        }

        private void AddLocalMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            string scope,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            string source,
            bool applicable = true,
            string unavailableReason = "")
        {
            AddMetric(result, name, scope, selector, source, applicable, applicable, unavailableReason);
        }

        private void AddMetric(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            string scope,
            Func<BattleRenderingBenchmarkFrame, BattleBenchmarkMetric> selector,
            string source,
            bool required,
            bool applicable,
            string unavailableReason)
        {
            if (!applicable)
            {
                result.Add(new BattleBenchmarkMetricAvailability(
                    name,
                    false,
                    BattleBenchmarkMetricApplicability.NotApplicable,
                    BattleBenchmarkMetricStatus.NotApplicable,
                    scope,
                    0,
                    0,
                    source,
                    unavailableReason));
                return;
            }

            int sampleCount = 0;
            for (int index = 0; index < frames.Count; index++)
            {
                if (selector(frames[index]).Available)
                    sampleCount++;
            }
            BattleBenchmarkMetricStatus status = sampleCount == config.SampleFrames
                ? BattleBenchmarkMetricStatus.Available
                : completedFrameCollector.IsSupported
                    ? BattleBenchmarkMetricStatus.Missing
                    : BattleBenchmarkMetricStatus.Unsupported;
            string reason = status == BattleBenchmarkMetricStatus.Available
                ? string.Empty
                : !string.IsNullOrEmpty(completedFrameSamplingFailureReason)
                    ? completedFrameSamplingFailureReason
                    : string.IsNullOrWhiteSpace(unavailableReason)
                        ? string.IsNullOrWhiteSpace(completedFrameCollector.UnsupportedReason)
                            ? $"Captured {sampleCount} of {config.SampleFrames} required samples."
                            : completedFrameCollector.UnsupportedReason
                        : unavailableReason;
            result.Add(new BattleBenchmarkMetricAvailability(
                name,
                required,
                BattleBenchmarkMetricApplicability.Applicable,
                status,
                scope,
                sampleCount,
                config.SampleFrames,
                source,
                reason));
        }

        private static void AddGate(
            List<BattleBenchmarkMetricAvailability> result,
            string name,
            bool? passed,
            int sampleCount,
            int expectedSampleCount,
            string source)
        {
            BattleBenchmarkMetricStatus status =
                BattleRenderingBenchmarkEvidencePolicy.ValidationStatus(passed);
            result.Add(new BattleBenchmarkMetricAvailability(
                name,
                true,
                BattleBenchmarkMetricApplicability.Applicable,
                status,
                "validation-gate",
                passed.HasValue ? sampleCount : 0,
                expectedSampleCount,
                source,
                !passed.HasValue
                    ? "The current-scene workload did not measure this validation gate."
                    : passed.Value
                        ? string.Empty
                        : "The required validation gate failed."));
        }

        private bool ValidateLogicTickMetrics()
        {
            if (frames.Count <= 0)
                return false;
            for (int index = 0; index < frames.Count; index++)
            {
                if (!frames[index].LogicTickTimeMs.Available)
                    return false;
                if (!frames[index].LogicTickAllocatedBytes.Available)
                    return false;
            }
            return true;
        }

        private void ValidateCount()
        {
            if (workload.ActualEntityCount != workload.RequestedEntityCount ||
                workload.FrozenFrame.EntityCount != workload.ActualEntityCount)
            {
                throw new InvalidOperationException(
                    $"Benchmark presentation entity count changed or mismatched: requested={workload.RequestedEntityCount}, " +
                    $"actual={workload.ActualEntityCount}, frame={workload.FrozenFrame.EntityCount}.");
            }
        }

        private void ValidatePresenterWorkload()
        {
            if (presenter.ResolvedCommandCount != workload.CommandCount ||
                presenter.MaterializedRenderItemCount != workload.CommandCount)
            {
                throw new InvalidOperationException(
                    $"{presenter.Implementation} did not materialize the complete workload: " +
                    $"commands={workload.CommandCount}, resolved={presenter.ResolvedCommandCount}, " +
                    $"materializedItems={presenter.MaterializedRenderItemCount}.");
            }
        }
    }

    public sealed class BattleRenderingBenchmarkSuiteSession : IDisposable
    {
        private readonly BattleRenderingBenchmarkConfig config;
        private readonly SimulationWorld world;
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly List<BattleRenderingBenchmarkReport> runs =
            new List<BattleRenderingBenchmarkReport>(2);
        private readonly BattlePresentationBackendMode previousBackend;
        private readonly Func<BattleRenderingBenchmarkConfig, SimulationWorld,
            BattleRenderingBenchmarkWorkload, IBattleRenderingBenchmarkRunSession> sessionFactory;
        private IBattleRenderingBenchmarkRunSession activeSession;
        private BattleRenderingBenchmarkSuiteReport report;
        private int nextBackendIndex;
        private bool disposed;
        private bool backendRestored;

        public BattleRenderingBenchmarkSuiteSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world)
            : this(config, world, null)
        {
        }

        public BattleRenderingBenchmarkSuiteSession(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            Func<BattleRenderingBenchmarkConfig, SimulationWorld,
                BattleRenderingBenchmarkWorkload, IBattleRenderingBenchmarkRunSession> benchmarkSessionFactory)
        {
            this.config = config;
            this.world = world ?? (config.Scenario.UsesCurrentScene
                ? throw new ArgumentNullException(nameof(world))
                : new SimulationWorld());
            previousBackend = this.world.BattlePresentation.Mode;
            sessionFactory = benchmarkSessionFactory ??
                ((runConfig, runWorld, runWorkload) =>
                    new BattleRenderingBenchmarkSession(runConfig, runWorld, runWorkload));
            workload = BattleRenderingBenchmarkWorkload.Create(
                config.Scenario,
                this.world,
                config.WarmupFrames,
                config.SampleFrames);
            try
            {
                StartNextRun();
            }
            catch
            {
                RestoreBackend();
                throw;
            }
        }

        public bool IsComplete => report != null;
        public BattleRenderingBenchmarkSuiteReport Report => report;
        public BattleRenderingBenchmarkWorkload Workload => workload;

        public bool CaptureFrame()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleRenderingBenchmarkSuiteSession));
            if (report != null)
                return true;
            try
            {
                if (!activeSession.CaptureFrame())
                    return false;

                BattleRenderingBenchmarkReport completed = activeSession.Report;
                runs.Add(completed);
                activeSession.Dispose();
                activeSession = null;
                if (StartNextRun())
                    return false;

                ValidateABWorkload();
                report = new BattleRenderingBenchmarkSuiteReport(config, runs.ToArray(), workload.Fingerprint);
                RestoreBackend();
                return true;
            }
            catch
            {
                try
                {
                    DisposeAfterFailure();
                }
                catch (Exception cleanupException)
                {
                    UnityEngine.Debug.LogException(cleanupException);
                }
                throw;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            try
            {
                activeSession?.Dispose();
            }
            finally
            {
                activeSession = null;
                RestoreBackend();
            }
        }

        private bool StartNextRun()
        {
            BattlePresentationBackendMode backend;
            if (config.Comparison == BattleRenderingBenchmarkComparison.Single)
            {
                if (nextBackendIndex > 0)
                    return false;
                backend = config.Backend;
            }
            else
            {
                if (nextBackendIndex == 0)
                    backend = BattlePresentationBackendMode.CentralOnly;
                else if (nextBackendIndex == 1)
                    backend = BattlePresentationBackendMode.LegacyOnly;
                else
                    return false;
            }
            nextBackendIndex++;
            world.SetBattlePresentationBackend(backend);
            activeSession = sessionFactory(
                config.ForBackend(backend),
                world,
                workload);
            return true;
        }

        private void ValidateABWorkload()
        {
            if (config.Comparison != BattleRenderingBenchmarkComparison.CentralLegacyAB)
                return;
            if (runs.Count != 2 ||
                runs[0].Config.Backend != BattlePresentationBackendMode.CentralOnly ||
                runs[1].Config.Backend != BattlePresentationBackendMode.LegacyOnly ||
                runs[0].WorkloadFingerprint != workload.Fingerprint ||
                runs[1].WorkloadFingerprint != workload.Fingerprint ||
                runs[0].InputFingerprint != runs[1].InputFingerprint ||
                runs[0].FinalRuntimeChecksum != runs[1].FinalRuntimeChecksum ||
                runs[0].BenchmarkRenderTargetWidth != runs[1].BenchmarkRenderTargetWidth ||
                runs[0].BenchmarkRenderTargetHeight != runs[1].BenchmarkRenderTargetHeight ||
                !runs[0].RendererWorkloadValidated ||
                !runs[1].RendererWorkloadValidated)
            {
                throw new InvalidOperationException(
                    "Central/Legacy A/B did not consume the same validated frozen workload.");
            }
        }

        private void RestoreBackend()
        {
            if (backendRestored)
                return;
            backendRestored = true;
            world.SetBattlePresentationBackend(previousBackend);
        }

        private void DisposeAfterFailure()
        {
            disposed = true;
            try
            {
                activeSession?.Dispose();
            }
            finally
            {
                activeSession = null;
                RestoreBackend();
            }
        }
    }

    public sealed class BattleRenderingBenchmarkRunner : MonoBehaviour
    {
        private BattleRenderingBenchmarkSuiteSession session;
        private string outputPath;
        private Action<BattleRenderingBenchmarkRunner, string> completion;
        private bool stopping;

        public static BattleRenderingBenchmarkRunner Start(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            string outputPath,
            Action<BattleRenderingBenchmarkRunner, string> completion = null)
        {
            var host = new GameObject("NTSD Battle Rendering Benchmark Runner")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            DontDestroyOnLoad(host);
            BattleRenderingBenchmarkRunner runner = host.AddComponent<BattleRenderingBenchmarkRunner>();
            try
            {
                runner.Initialize(config, world, outputPath, completion);
                return runner;
            }
            catch
            {
                DisposeHost(host);
                throw;
            }
        }

        public void Abort(string reason)
        {
            if (stopping)
                return;
            stopping = true;
            session?.Dispose();
            session = null;
            Notify("FAIL\n" + (reason ?? "Benchmark aborted."));
            DisposeHost(gameObject);
        }

        private void Initialize(
            BattleRenderingBenchmarkConfig config,
            SimulationWorld world,
            string path,
            Action<BattleRenderingBenchmarkRunner, string> callback)
        {
            outputPath = path;
            completion = callback;
            session = new BattleRenderingBenchmarkSuiteSession(config, world);
        }

        private void Update()
        {
            if (stopping || session == null)
                return;
            try
            {
                if (!session.CaptureFrame())
                    return;
                session.Report.WriteJson(outputPath);
                StopWithResult(session.Report.Verdict.ToString().ToUpperInvariant() + "\n" + outputPath);
            }
            catch (Exception ex)
            {
                Abort(ex.ToString());
            }
        }

        private void StopWithResult(string result)
        {
            if (stopping)
                return;
            stopping = true;
            session.Dispose();
            session = null;
            Notify(result);
            DisposeHost(gameObject);
        }

        private void Notify(string result)
        {
            Action<BattleRenderingBenchmarkRunner, string> callback = completion;
            completion = null;
            callback?.Invoke(this, result);
        }

        private void OnDestroy()
        {
            if (!stopping && session != null)
            {
                session.Dispose();
                session = null;
                Notify("FAIL\nBenchmark runner was destroyed before completion.");
            }
        }

        private static void DisposeHost(UnityEngine.Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                Destroy(target);
            else
                DestroyImmediate(target);
        }
    }

    public static class BattleRenderingBenchmarkPlayerArguments
    {
        public const string EnableArgument = "-ntsdBattleRenderingBenchmark";
        public const string ScenarioArgument = "-ntsdBattleRenderingBenchmarkScenario";
        public const string BackendArgument = "-ntsdBattleRenderingBenchmarkBackend";
        public const string ComparisonArgument = "-ntsdBattleRenderingBenchmarkComparison";
        public const string WarmupArgument = "-ntsdBattleRenderingBenchmarkWarmup";
        public const string SampleArgument = "-ntsdBattleRenderingBenchmarkSamples";
        public const string LeakArgument = "-ntsdBattleRenderingBenchmarkLeakFrames";
        public const string OutputArgument = "-ntsdBattleRenderingBenchmarkOutput";

        public static bool TryParse(
            string[] arguments,
            out BattleRenderingBenchmarkRequest request,
            out string error)
        {
            request = null;
            error = string.Empty;
            if (!ContainsFlag(arguments, EnableArgument))
                return false;

            var parsed = new BattleRenderingBenchmarkRequest();
            try
            {
                parsed.targetActiveEntities = FindValue(arguments, ScenarioArgument) ?? "1000";
                parsed.backend = FindValue(arguments, BackendArgument) ??
                                 nameof(BattlePresentationBackendMode.CentralOnly);
                parsed.comparison = FindValue(arguments, ComparisonArgument) ?? "ab";
                parsed.warmupFrames = ParseInt(arguments, WarmupArgument, parsed.warmupFrames);
                parsed.sampleFrames = ParseInt(arguments, SampleArgument, parsed.sampleFrames);
                parsed.leakCheckFrames = ParseInt(arguments, LeakArgument, parsed.leakCheckFrames);
                parsed.outputPath = FindValue(arguments, OutputArgument) ??
                                    "NTSD_BattleRenderingBenchmark-Player.json";
                BattleRenderingBenchmarkConfig.FromRequest(parsed);
                request = parsed;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static int ParseInt(string[] arguments, string name, int fallback)
        {
            string value = FindValue(arguments, name);
            if (string.IsNullOrWhiteSpace(value))
                return fallback;
            if (!int.TryParse(value, out int parsed))
                throw new ArgumentException($"Argument {name} requires an integer value.");
            return parsed;
        }

        private static bool ContainsFlag(string[] arguments, string name)
        {
            if (arguments == null)
                return false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(arguments[index], name + "=true", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private static string FindValue(string[] arguments, string name)
        {
            if (arguments == null)
                return null;
            string prefix = name + "=";
            for (int index = 0; index < arguments.Length; index++)
            {
                string argument = arguments[index];
                if (argument != null &&
                    argument.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return argument.Substring(prefix.Length);
                }
                if (string.Equals(argument, name, StringComparison.OrdinalIgnoreCase) &&
                    index + 1 < arguments.Length)
                {
                    return arguments[index + 1];
                }
            }
            return null;
        }
    }

    internal static class BattleRenderingBenchmarkPlayerBootstrap
    {
#if !UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void TryStart()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            bool explicitlyRequested = false;
            for (int index = 0; index < arguments.Length; index++)
            {
                if (string.Equals(
                        arguments[index],
                        BattleRenderingBenchmarkPlayerArguments.EnableArgument,
                        StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(
                        arguments[index],
                        BattleRenderingBenchmarkPlayerArguments.EnableArgument + "=true",
                        StringComparison.OrdinalIgnoreCase))
                {
                    explicitlyRequested = true;
                    break;
                }
            }
            if (!explicitlyRequested)
                return;

            if (!BattleRenderingBenchmarkPlayerArguments.TryParse(
                    arguments,
                    out BattleRenderingBenchmarkRequest request,
                    out string error))
            {
                UnityEngine.Debug.LogError("[BattleRenderingBenchmark] Invalid Player request: " + error);
                Application.Quit(2);
                return;
            }

            try
            {
                BattleRenderingBenchmarkConfig config =
                    BattleRenderingBenchmarkConfig.FromRequest(request);
                SimulationWorld world = config.Scenario.UsesCurrentScene
                    ? SimulationTickDriver.Instance?.World
                    : null;
                if (config.Scenario.UsesCurrentScene && world == null)
                    throw new InvalidOperationException("Current-scene Player benchmark has no active SimulationWorld.");
                BattleRenderingBenchmarkRunner.Start(
                    config,
                    world,
                    config.OutputPath,
                    (_, result) =>
                    {
                        bool passed = result != null && result.StartsWith("PASS", StringComparison.Ordinal);
                        UnityEngine.Debug.Log("[BattleRenderingBenchmark] Player result: " + result);
                        Application.Quit(passed ? 0 : 1);
                    });
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("[BattleRenderingBenchmark] Player start failed: " + ex);
                Application.Quit(2);
            }
        }
#endif
    }

    public enum BattleBenchmarkRecorderKind : byte
    {
        FrameTime = 0,
        MainThread = 1,
        RenderThread = 2,
        GpuFrame = 3,
        LogicTick = 4,
        ManagedAllocation = 5,
        DrawCalls = 6,
        TotalMemory = 7,
        GraphicsMemory = 8,
    }

    internal static class BattleRenderingBenchmarkMemory
    {
        internal static BattleBenchmarkMetric CaptureTotalAllocatedMemory()
        {
            return BattleBenchmarkMetric.FromValue(Profiler.GetTotalAllocatedMemoryLong(), "bytes");
        }

        internal static BattleBenchmarkMetric CaptureGraphicsMemory()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            return BattleBenchmarkMetric.FromValue(
                Profiler.GetAllocatedMemoryForGraphicsDriver(),
                "bytes");
#else
            return BattleBenchmarkMetric.Unavailable("bytes");
#endif
        }

    }

    public sealed class BattleBenchmarkCompletedFrameAttribution
    {
        private int pendingGeneration;
        private int requestedUnityFrame;
        private ulong timingWatermark;
        private ulong lastAcceptedTimingTimestamp;
        private bool countersSnapshotted;

        public bool CountersSnapshotted => countersSnapshotted;

        public void Request(int generation, int unityFrame, ulong latestTimingTimestamp)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
            requestedUnityFrame = unityFrame;
            timingWatermark = Math.Max(latestTimingTimestamp, lastAcceptedTimingTimestamp);
            countersSnapshotted = false;
        }

        public bool ShouldSnapshotCounters(int generation, int unityFrame)
        {
            if (generation != pendingGeneration || countersSnapshotted || unityFrame <= requestedUnityFrame)
                return false;
            countersSnapshotted = true;
            return true;
        }

        public bool TryAcceptTiming(int generation, ulong frameStartTimestamp)
        {
            if (generation != pendingGeneration || !countersSnapshotted ||
                frameStartTimestamp == 0UL || frameStartTimestamp <= timingWatermark)
            {
                return false;
            }
            lastAcceptedTimingTimestamp = frameStartTimestamp;
            pendingGeneration = 0;
            return true;
        }

        public void CompleteWithoutTiming(int generation)
        {
            if (generation == pendingGeneration)
                pendingGeneration = 0;
        }

        public void ResetPending()
        {
            pendingGeneration = 0;
            requestedUnityFrame = 0;
            timingWatermark = lastAcceptedTimingTimestamp;
            countersSnapshotted = false;
        }
    }

    internal sealed class BattleBenchmarkUnityCompletedFrameCollector :
        IBattleBenchmarkCompletedFrameCollector
    {
        private const int MaxFrameTimingDrainAttempts = 4;
        private readonly BattleRenderingBenchmarkPolicyContext context;
        private readonly BattleBenchmarkCounterRecorder managedAllocation;
        private readonly BattleBenchmarkCounterRecorder drawCalls;
        private readonly BattleBenchmarkCompletedFrameAttribution attribution =
            new BattleBenchmarkCompletedFrameAttribution();
        private readonly FrameTiming[] timings = new FrameTiming[1];
        private int pendingGeneration;
        private int drainAttempts;
        private int lastDrainUnityFrame = -1;
        private string frameTimingReason = string.Empty;
        private BattleBenchmarkMetric managedAllocationSnapshot;
        private BattleBenchmarkMetric drawCallsSnapshot;
        private BattleBenchmarkMetric totalMemorySnapshot;
        private BattleBenchmarkMetric graphicsMemorySnapshot;

        internal BattleBenchmarkUnityCompletedFrameCollector(
            BattleRenderingBenchmarkPolicyContext benchmarkPolicyContext)
        {
            context = benchmarkPolicyContext;
            managedAllocation = new BattleBenchmarkCounterRecorder(
                ProfilerCategory.Memory,
                "GC Allocated In Frame",
                "bytes");
            drawCalls = new BattleBenchmarkCounterRecorder(
                ProfilerCategory.Render,
                "Draw Calls Count",
                "count");
        }

        public bool IsSupported =>
            context.IsSupportedExecutionScope && context.FrameTimingStatsEnabled;

        public string UnsupportedReason
        {
            get
            {
                if (!context.IsPlaying)
                    return "EditMode has no completed rendered-frame collection scope.";
                if (!context.IsSupportedExecutionScope)
                    return "Completed-frame collection is supported only in Play Mode Editor and Windows Standalone.";
                if (!context.FrameTimingStatsEnabled)
                    return "FrameTimingManager.IsFeatureEnabled returned false.";
                return frameTimingReason;
            }
        }

        public void Request(int generation)
        {
            if (pendingGeneration != 0)
                throw new InvalidOperationException("A completed-frame sample is already pending.");
            pendingGeneration = generation;
            drainAttempts = 0;
            lastDrainUnityFrame = -1;
            frameTimingReason = string.Empty;
            managedAllocationSnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            drawCallsSnapshot = BattleBenchmarkMetric.Unavailable("count");
            totalMemorySnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            graphicsMemorySnapshot = BattleBenchmarkMetric.Unavailable("bytes");
            managedAllocation.Restart();
            drawCalls.Restart();
            if (IsSupported)
            {
                attribution.Request(generation, Time.frameCount, LatestTimingTimestamp());
                FrameTimingManager.CaptureFrameTimings();
            }
        }

        public bool TryDrain(int generation, out BattleBenchmarkCompletedFrameMetrics metrics)
        {
            if (pendingGeneration != generation)
            {
                metrics = default;
                return false;
            }
            if (!IsSupported)
            {
                pendingGeneration = 0;
                managedAllocation.Abort();
                drawCalls.Abort();
                metrics = BattleBenchmarkCompletedFrameMetrics.Unavailable();
                return true;
            }
            if (!attribution.CountersSnapshotted &&
                !attribution.ShouldSnapshotCounters(generation, Time.frameCount))
            {
                metrics = default;
                return false;
            }
            if (attribution.CountersSnapshotted && drainAttempts == 0)
                SnapshotAndStopCounters();
            if (lastDrainUnityFrame == Time.frameCount)
            {
                metrics = default;
                return false;
            }
            lastDrainUnityFrame = Time.frameCount;

            drainAttempts++;
            uint count = FrameTimingManager.GetLatestTimings(1, timings);
            bool timingAccepted = count > 0 &&
                                  attribution.TryAcceptTiming(
                                      generation,
                                      timings[0].frameStartTimestamp);
            if (!timingAccepted && drainAttempts < MaxFrameTimingDrainAttempts)
            {
                metrics = default;
                return false;
            }

            pendingGeneration = 0;
            if (!timingAccepted)
            {
                frameTimingReason =
                    count == 0
                        ? "FrameTimingManager returned no completed timing after the bounded drain window."
                        : "FrameTimingManager returned only stale timing generations after the bounded drain window.";
                attribution.CompleteWithoutTiming(generation);
                metrics = new BattleBenchmarkCompletedFrameMetrics(
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    BattleBenchmarkMetric.Unavailable("ms"),
                    managedAllocationSnapshot,
                    drawCallsSnapshot,
                    totalMemorySnapshot,
                    graphicsMemorySnapshot);
                return true;
            }

            FrameTiming timing = timings[0];
            metrics = new BattleBenchmarkCompletedFrameMetrics(
                PositiveMilliseconds(timing.cpuFrameTime),
                PositiveMilliseconds(timing.cpuMainThreadFrameTime),
                context.GraphicsMultiThreaded
                    ? PositiveMilliseconds(timing.cpuRenderThreadFrameTime)
                    : BattleBenchmarkMetric.Unavailable("ms"),
                PositiveMilliseconds(timing.gpuFrameTime),
                managedAllocationSnapshot,
                drawCallsSnapshot,
                totalMemorySnapshot,
                graphicsMemorySnapshot);
            return true;
        }

        public string Source(BattleBenchmarkRecorderKind kind)
        {
            switch (kind)
            {
                case BattleBenchmarkRecorderKind.FrameTime:
                case BattleBenchmarkRecorderKind.MainThread:
                case BattleBenchmarkRecorderKind.RenderThread:
                case BattleBenchmarkRecorderKind.GpuFrame:
                    return "FrameTimingManager completed frame";
                case BattleBenchmarkRecorderKind.ManagedAllocation:
                    return managedAllocation.Source;
                case BattleBenchmarkRecorderKind.DrawCalls:
                    return drawCalls.Source;
                case BattleBenchmarkRecorderKind.TotalMemory:
                    return "Profiler.GetTotalAllocatedMemoryLong";
                case BattleBenchmarkRecorderKind.GraphicsMemory:
                    return "Profiler.GetAllocatedMemoryForGraphicsDriver";
                default:
                    return string.Empty;
            }
        }

        public string Reason(BattleBenchmarkRecorderKind kind)
        {
            switch (kind)
            {
                case BattleBenchmarkRecorderKind.ManagedAllocation:
                    return managedAllocation.Reason;
                case BattleBenchmarkRecorderKind.DrawCalls:
                    return drawCalls.Reason;
                case BattleBenchmarkRecorderKind.GraphicsMemory:
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    return string.Empty;
#else
                    return "Profiler.GetAllocatedMemoryForGraphicsDriver is available only in Editor or Development Player.";
#endif
                default:
                    return frameTimingReason;
            }
        }

        public void Reset()
        {
            pendingGeneration = 0;
            drainAttempts = 0;
            lastDrainUnityFrame = -1;
            frameTimingReason = string.Empty;
            attribution.ResetPending();
            managedAllocation.Abort();
            drawCalls.Abort();
        }

        public void Dispose()
        {
            Reset();
            managedAllocation.Dispose();
            drawCalls.Dispose();
        }

        private void SnapshotAndStopCounters()
        {
            managedAllocationSnapshot = managedAllocation.SnapshotAndStop();
            drawCallsSnapshot = drawCalls.SnapshotAndStop(requirePositive: true);
            totalMemorySnapshot = BattleRenderingBenchmarkMemory.CaptureTotalAllocatedMemory();
            graphicsMemorySnapshot = BattleRenderingBenchmarkMemory.CaptureGraphicsMemory();
        }

        private ulong LatestTimingTimestamp()
        {
            uint count = FrameTimingManager.GetLatestTimings(1, timings);
            return count > 0 ? timings[0].frameStartTimestamp : 0UL;
        }

        private static BattleBenchmarkMetric PositiveMilliseconds(double value)
        {
            return value > 0d
                ? BattleBenchmarkMetric.FromValue(value, "ms")
                : BattleBenchmarkMetric.Unavailable("ms");
        }
    }

    internal sealed class BattleBenchmarkCounterRecorder : IDisposable
    {
        private readonly string marker;
        private readonly string unit;
        private ProfilerRecorder recorder;
        private bool valid;

        internal BattleBenchmarkCounterRecorder(
            ProfilerCategory category,
            string profilerMarker,
            string metricUnit)
        {
            marker = profilerMarker;
            unit = metricUnit;
            Source = "ProfilerRecorderHandle:" + marker;
            var handles = new List<ProfilerRecorderHandle>();
            ProfilerRecorderHandle.GetAvailable(handles);
            for (int index = 0; index < handles.Count; index++)
            {
                ProfilerRecorderDescription description =
                    ProfilerRecorderHandle.GetDescription(handles[index]);
                if (description.Category != category ||
                    !string.Equals(description.Name, marker, StringComparison.Ordinal))
                {
                    continue;
                }
                try
                {
                    recorder = new ProfilerRecorder(
                        handles[index],
                        1,
                        ProfilerRecorderOptions.Default);
                    valid = recorder.Valid;
                }
                catch (Exception ex)
                {
                    Reason = "ProfilerRecorder start failed: " + ex.GetType().Name;
                }
                break;
            }
            if (!valid && string.IsNullOrEmpty(Reason))
                Reason = "The exact profiler counter was not discovered on this platform.";
        }

        internal string Source { get; }
        internal string Reason { get; private set; } = string.Empty;

        internal void Restart()
        {
            if (!valid)
                return;
            recorder.Reset();
            Reason = string.Empty;
            recorder.Start();
        }

        internal BattleBenchmarkMetric SnapshotAndStop(bool requirePositive = false)
        {
            if (valid)
                recorder.Stop();
            BattleBenchmarkMetric metric = BattleBenchmarkCounterSamplePolicy.Capture(
                valid,
                valid ? recorder.Count : 0,
                valid && recorder.Count > 0 ? recorder.LastValue : 0L,
                unit,
                requirePositive,
                out string reason);
            if (!string.IsNullOrEmpty(reason))
                Reason = reason;
            return metric;
        }

        internal void Abort()
        {
            if (valid)
                recorder.Reset();
        }

        public void Dispose()
        {
            recorder.Dispose();
            valid = false;
        }
    }

    public static class BattleBenchmarkCounterSamplePolicy
    {
        public static BattleBenchmarkMetric Capture(
            bool recorderValid,
            int sampleCount,
            long value,
            string unit,
            bool requirePositive,
            out string reason)
        {
            if (!recorderValid)
            {
                reason = "The exact profiler counter was not discovered on this platform.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            if (sampleCount <= 0)
            {
                reason = "The profiler counter produced no completed-frame sample.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            if (requirePositive && value <= 0L)
            {
                reason = "The profiler counter returned zero for a non-empty benchmark render workload.";
                return BattleBenchmarkMetric.Unavailable(unit);
            }
            reason = string.Empty;
            return BattleBenchmarkMetric.FromValue(value, unit);
        }
    }

    public static class BattleBenchmarkOwnedTextureMemoryPolicy
    {
        public static BattleBenchmarkMetric Capture(
            int resourceGeneration,
            int ownedTextureResourceCount,
            long measuredBytes,
            out string reason)
        {
            if (resourceGeneration <= 0)
            {
                reason = "The benchmark-owned texture evidence has no valid resource generation.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }
            if (ownedTextureResourceCount <= 0)
            {
                reason = "The benchmark resource generation owns no Texture2D or RenderTexture objects.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }
            if (measuredBytes <= 0L)
            {
                reason = "Profiler.GetRuntimeMemorySizeLong returned no positive bytes for the benchmark-owned texture resources.";
                return BattleBenchmarkMetric.Unavailable("bytes");
            }

            reason = string.Empty;
            return BattleBenchmarkMetric.FromValue(measuredBytes, "bytes");
        }
    }

    public interface IBattleRenderingBenchmarkPresenter : IDisposable
    {
        string Implementation { get; }
        string EffectiveBackend { get; }
        string ResourceMode { get; }
        string DrawMode { get; }
        int RenderTargetWidth { get; }
        int RenderTargetHeight { get; }
        int ResolvedCommandCount { get; }
        int MaterializedRenderItemCount { get; }
        int ResourceSegmentCount { get; }
        int SubmissionDrawCount { get; }
        string SubmissionDrawMetricSource { get; }
        string SubmissionDrawUnavailableReason { get; }
        int ResourceGeneration { get; }
        int OwnedTextureResourceCount { get; }
        int OwnedResourceCount { get; }
        long CachedOwnedResourceMemoryBytes { get; }
        long MeasureOwnedResourceMemoryBytes();
        long MeasureOwnedTextureMemoryBytes();
        BattleCentralBuildDiagnostics Diagnostics { get; }
        double Present();
    }

    internal static class BattleRenderingBenchmarkPresenterFactory
    {
        internal static IBattleRenderingBenchmarkPresenter Create(
            BattlePresentationBackendMode backend,
            BattleRenderingBenchmarkWorkload workload)
        {
            switch (backend)
            {
                case BattlePresentationBackendMode.CentralOnly:
                    return new BattleBenchmarkCentralPresenter(workload);
                case BattlePresentationBackendMode.LegacyOnly:
                    return new BattleBenchmarkLegacyPresenter(workload);
                default:
                    throw new ArgumentOutOfRangeException(nameof(backend));
            }
        }
    }

    internal sealed class BattleBenchmarkCentralPresenter : IBattleRenderingBenchmarkPresenter
    {
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleBenchmarkResourceSet resources;
        private readonly BattleDynamicMeshBackend backend = new BattleDynamicMeshBackend();
        private readonly long cachedOwnedResourceMemoryBytes;
        private int lastSubmissionDrawCount = BattleRenderingBenchmarkSubmissionPolicy.Unavailable;
        private bool disposed;

        internal BattleBenchmarkCentralPresenter(BattleRenderingBenchmarkWorkload workload)
        {
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            resources = new BattleBenchmarkResourceSet("Central");
            Present();
            cachedOwnedResourceMemoryBytes = MeasureOwnedResourceMemoryBytes();
        }

        public string Implementation => "BenchmarkCentralPersistentDynamicMesh";
        public string EffectiveBackend => BattlePresentationBackendMode.CentralOnly.ToString();
        public string ResourceMode => BattleSpriteCentralBindingMode.SourceTexture2D.ToString();
        public string DrawMode => BattleCentralDrawMode.OrderedChunks.ToString();
        public int RenderTargetWidth => resources.RenderTargetWidth;
        public int RenderTargetHeight => resources.RenderTargetHeight;
        public int ResolvedCommandCount => backend.Diagnostics.ResolvedCommandCount;
        public int MaterializedRenderItemCount => backend.Diagnostics.ResolvedCommandCount;
        public int ResourceSegmentCount => backend.Diagnostics.SegmentCount;
        public int SubmissionDrawCount => lastSubmissionDrawCount;
        public string SubmissionDrawMetricSource => "Graphics.DrawMesh calls issued by the central presenter";
        public string SubmissionDrawUnavailableReason =>
            "Application is not in Play Mode; the central presenter built mesh segments but did not call Graphics.DrawMesh.";
        public int ResourceGeneration => resources.ResourceGeneration;
        public int OwnedTextureResourceCount => resources.OwnedTextureResourceCount;
        public int OwnedResourceCount => disposed
            ? 0
            : resources.OwnedResourceCount + backend.AllocatedChunkCount;
        public long CachedOwnedResourceMemoryBytes => disposed ? 0L : cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
            if (disposed)
                return 0L;
            long bytes = resources.OwnedResourceMemoryBytes;
            for (int index = 0; index < backend.AllocatedChunkCount; index++)
            {
                Mesh mesh = backend.GetChunkMesh(index);
                if (mesh != null)
                    bytes += Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            return bytes;
        }
        public long MeasureOwnedTextureMemoryBytes() => resources.OwnedTextureMemoryBytes;
        public BattleCentralBuildDiagnostics Diagnostics => backend.Diagnostics;

        public double Present()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleBenchmarkCentralPresenter));
            long started = Stopwatch.GetTimestamp();
            backend.Build(
                workload.FrozenFrame,
                resources,
                BattleCentralDrawMode.OrderedChunks);
            double elapsed = BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
            int issuedDrawCalls = 0;
            if (Application.isPlaying)
            {
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    Graphics.DrawMesh(
                        backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity,
                        segment.Material,
                        BattleBenchmarkResourceSet.BenchmarkLayer,
                        resources.Camera,
                        segment.SubMeshIndex,
                        null,
                        false,
                        false,
                        false);
                    issuedDrawCalls++;
                }
            }
            lastSubmissionDrawCount = BattleRenderingBenchmarkSubmissionPolicy.FromGraphicsDrawMeshCalls(
                issuedDrawCalls > 0,
                issuedDrawCalls);
            return elapsed;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            backend.Dispose();
            resources.Dispose();
        }
    }

    internal sealed class BattleBenchmarkLegacyPresenter : IBattleRenderingBenchmarkPresenter
    {
        private readonly BattleRenderingBenchmarkWorkload workload;
        private readonly BattleBenchmarkResourceSet resources;
        private readonly GameObject root;
        private readonly Transform[] transforms;
        private readonly SpriteRenderer[] renderers;
        private readonly long cachedOwnedResourceMemoryBytes;
        private bool disposed;

        internal BattleBenchmarkLegacyPresenter(BattleRenderingBenchmarkWorkload workload)
        {
            this.workload = workload ?? throw new ArgumentNullException(nameof(workload));
            resources = new BattleBenchmarkResourceSet("Legacy");
            root = new GameObject("NTSD Benchmark Legacy Presenter")
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = BattleBenchmarkResourceSet.BenchmarkLayer,
            };
            transforms = new Transform[workload.CommandCount];
            renderers = new SpriteRenderer[workload.CommandCount];
            for (int index = 0; index < workload.CommandCount; index++)
            {
                var child = new GameObject("LegacyCommand" + index)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    layer = BattleBenchmarkResourceSet.BenchmarkLayer,
                };
                child.transform.SetParent(root.transform, false);
                child.transform.localScale = NTSDRenderSpace.RenderScale;
                SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
                renderer.sprite = resources.Sprite;
                renderer.sharedMaterial = resources.Material;
                transforms[index] = child.transform;
                renderers[index] = renderer;
            }
            Present();
            cachedOwnedResourceMemoryBytes = MeasureOwnedResourceMemoryBytes();
        }

        public string Implementation => "BenchmarkRendererlessLegacyCompatibilityPresenter";
        public string EffectiveBackend => BattlePresentationBackendMode.LegacyOnly.ToString();
        public string ResourceMode => "SharedSourceTexture2D";
        public string DrawMode => "UnitySpriteRendererTransparentSortAndBatch";
        public int RenderTargetWidth => resources.RenderTargetWidth;
        public int RenderTargetHeight => resources.RenderTargetHeight;
        public int ResolvedCommandCount => renderers.Length;
        public int MaterializedRenderItemCount => renderers.Length;
        public int ResourceSegmentCount => renderers.Length > 0 ? 1 : 0;
        public int SubmissionDrawCount => -1;
        public string SubmissionDrawMetricSource =>
            "Unity SpriteRenderer batching is represented by the frame draw-call counter";
        public string SubmissionDrawUnavailableReason =>
            "Legacy SpriteRenderer batching has no reliable presenter-local draw count; use drawCalls when its ProfilerRecorder counter is available.";
        public int ResourceGeneration => resources.ResourceGeneration;
        public int OwnedTextureResourceCount => resources.OwnedTextureResourceCount;
        public int OwnedResourceCount => disposed
            ? 0
            : resources.OwnedResourceCount + 1 + renderers.Length * 3;
        public long CachedOwnedResourceMemoryBytes => disposed ? 0L : cachedOwnedResourceMemoryBytes;
        public long MeasureOwnedResourceMemoryBytes()
        {
            if (disposed)
                return 0L;
            long bytes = resources.OwnedResourceMemoryBytes;
            if (root != null)
                bytes += Profiler.GetRuntimeMemorySizeLong(root);
            for (int index = 0; index < renderers.Length; index++)
            {
                if (renderers[index] != null)
                    bytes += Profiler.GetRuntimeMemorySizeLong(renderers[index]);
                if (transforms[index] != null)
                {
                    bytes += Profiler.GetRuntimeMemorySizeLong(transforms[index]);
                    bytes += Profiler.GetRuntimeMemorySizeLong(transforms[index].gameObject);
                }
            }
            return bytes;
        }
        public long MeasureOwnedTextureMemoryBytes() => resources.OwnedTextureMemoryBytes;
        public BattleCentralBuildDiagnostics Diagnostics => null;

        public double Present()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleBenchmarkLegacyPresenter));
            long started = Stopwatch.GetTimestamp();
            for (int index = 0; index < workload.CommandCount; index++)
            {
                BattleRenderCommand command = workload.FrozenFrame.GetCommand(index);
                Transform target = transforms[index];
                SpriteRenderer renderer = renderers[index];
                target.localPosition = command.Position;
                renderer.flipX = command.FlipX;
                renderer.flipY = command.FlipY;
                renderer.color = command.Color;
                renderer.sortingOrder = command.SortOrder;
                renderer.enabled = true;
            }
            return BattleRenderingBenchmarkEnvironment.ElapsedMilliseconds(started);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            BattleRenderingBenchmarkEnvironment.DestroyObject(root);
            resources.Dispose();
        }
    }

    internal sealed class BattleBenchmarkResourceSet : IBattleCentralResourceResolver, IDisposable
    {
        internal const int BenchmarkLayer = 31;
        internal const int BenchmarkRenderTargetWidth = 256;
        internal const int BenchmarkRenderTargetHeight = 256;
        private readonly Texture2D texture;
        private readonly Material material;
        private readonly Sprite sprite;
        private readonly GameObject cameraObject;
        private readonly Camera camera;
        private readonly RenderTexture renderTexture;
        private static int nextResourceGeneration;
        private bool disposed;

        internal BattleBenchmarkResourceSet(string suffix)
        {
            ResourceGeneration = Interlocked.Increment(ref nextResourceGeneration);
            if (ResourceGeneration <= 0)
                throw new InvalidOperationException("Benchmark resource generation overflowed.");
            Shader shader = Shader.Find(BattleSpriteMaterialContract.BuiltInSpriteShaderName);
            if (shader == null)
                throw new InvalidOperationException("Sprites/Default shader is unavailable for the benchmark harness.");
            texture = new Texture2D(16, 16, TextureFormat.RGBA32, false, true)
            {
                name = "NTSD Benchmark Texture " + suffix,
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var pixels = new Color32[16 * 16];
            for (int index = 0; index < pixels.Length; index++)
            {
                byte shade = (byte)(((index / 16 + index % 16) & 1) == 0 ? 255 : 192);
                pixels[index] = new Color32(shade, shade, shade, 255);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, 16f, 16f),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect);
            sprite.name = "NTSD Benchmark Sprite " + suffix;
            sprite.hideFlags = HideFlags.HideAndDontSave;
            material = new Material(shader)
            {
                name = "NTSD Benchmark Material " + suffix,
                mainTexture = texture,
                hideFlags = HideFlags.HideAndDontSave,
            };
            cameraObject = new GameObject("NTSD Benchmark Camera " + suffix)
            {
                hideFlags = HideFlags.HideAndDontSave,
                layer = BenchmarkLayer,
            };
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.transform.position = new Vector3(4f, 4f, -10f);
            camera.cullingMask = 1 << BenchmarkLayer;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.clear;
            renderTexture = new RenderTexture(
                BenchmarkRenderTargetWidth,
                BenchmarkRenderTargetHeight,
                16,
                RenderTextureFormat.ARGB32)
            {
                name = "NTSD Benchmark Target " + suffix,
                hideFlags = HideFlags.HideAndDontSave,
            };
            renderTexture.Create();
            camera.targetTexture = renderTexture;
            camera.enabled = Application.isPlaying;
        }

        internal Camera Camera => camera;
        internal int ResourceGeneration { get; }
        internal int RenderTargetWidth => disposed ? 0 : renderTexture.width;
        internal int RenderTargetHeight => disposed ? 0 : renderTexture.height;
        internal Material Material => material;
        internal Sprite Sprite => sprite;
        internal int OwnedResourceCount => disposed ? 0 : 6;
        internal int OwnedTextureResourceCount => disposed ? 0 : 2;
        internal long OwnedTextureMemoryBytes =>
            disposed
                ? 0L
                : BattleRenderingBenchmarkEnvironment.RuntimeMemory(texture) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(renderTexture);
        internal long OwnedResourceMemoryBytes =>
            disposed
                ? 0L
                : BattleRenderingBenchmarkEnvironment.RuntimeMemory(texture) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(material) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(sprite) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(cameraObject) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(camera) +
                  BattleRenderingBenchmarkEnvironment.RuntimeMemory(renderTexture);

        public BattleCentralResourceStatus Resolve(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = new BattleCentralResolvedResource(
                texture,
                material,
                new Rect(0f, 0f, 1f, 1f),
                new Vector2(16f, 16f),
                new Vector2(0.5f, 0.5f),
                command.Color,
                0,
                0,
                BattleSpriteCentralBindingMode.SourceTexture2D);
            return BattleCentralResourceStatus.Resolved;
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            if (camera != null)
                camera.targetTexture = null;
            if (renderTexture != null)
                renderTexture.Release();
            BattleRenderingBenchmarkEnvironment.DestroyObject(cameraObject);
            BattleRenderingBenchmarkEnvironment.DestroyObject(renderTexture);
            BattleRenderingBenchmarkEnvironment.DestroyObject(material);
            BattleRenderingBenchmarkEnvironment.DestroyObject(sprite);
            BattleRenderingBenchmarkEnvironment.DestroyObject(texture);
        }
    }

    internal static class BattleRenderingBenchmarkEnvironment
    {
        internal static Dictionary<string, object> Capture()
        {
            return new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["applicationPlatform"] = Application.platform.ToString(),
                ["deviceModel"] = SystemInfo.deviceModel,
                ["editor"] = Application.isEditor,
                ["graphicsApi"] = SystemInfo.graphicsDeviceType.ToString(),
                ["gpu"] = SystemInfo.graphicsDeviceName,
                ["graphicsDeviceVersion"] = SystemInfo.graphicsDeviceVersion,
                ["graphicsMemoryCapacityMB"] = SystemInfo.graphicsMemorySize,
                ["resolutionHeight"] = Screen.height,
                ["resolutionWidth"] = Screen.width,
                ["runtime"] = Application.isEditor ? "Editor" : "Player",
            };
        }

        internal static double ElapsedMilliseconds(long startedTimestamp)
        {
            long elapsed = Stopwatch.GetTimestamp() - startedTimestamp;
            return elapsed * 1000d / Stopwatch.Frequency;
        }

        internal static long RuntimeMemory(UnityEngine.Object target)
        {
            return target == null ? 0L : Profiler.GetRuntimeMemorySizeLong(target);
        }

        internal static void WriteJson(string path, string json)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("An output path is required.", nameof(path));
            string fullPath = Path.GetFullPath(path);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, json, new System.Text.UTF8Encoding(false));
        }

        internal static void DestroyObject(UnityEngine.Object target)
        {
            if (target == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }
    }
}


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# Task: implement and run a real 1000-production-entity Unity stress harness

Work in the current Unity NTSD repository. The user requires an Editor Play Mode stress test with 1000 real, visible production GameObjects, not the existing hidden pure-C# BattleRenderingBenchmark fixtures.

Requirements:

1. Use the real LF2ObjectPool/LF2ReferencePool/LF2Character creation chain and the production SimulationWorld/NTSDBattleTickSystem pass order.
2. Hierarchy must show 1000 active entity GameObjects in a clearly named stress-test root. No HideAndDontSave, hidden camera, or RenderTexture-only workload.
3. Ensure the world uses MobileExtended capacity 1050 and LooseQuadtree before any entities are registered. Do not mutate user config assets merely to run the test; add a scoped diagnostic/test entry point.
4. Provide two modes: dispersed combat and concentrated worst-case combat. Entities must use real AI/input, collision candidate, hit, opoint, death and pool lifecycle code when DAT/config permits.
5. Add an Editor menu/window/request entry that can start dispersed, start concentrated, stop/cleanup. It must keep running long enough to inspect Scene/Game/Hierarchy and write a structured JSON report.
6. Report at least real active GameObject count, world object/entity count, claimed runtime slot count, logic tick/frame timing avg/max/p95/p99, backlog/catch-up, GC allocation if safely available, broadphase backend, collision/AI/hit/opoint counters where existing production diagnostics expose them, and teardown restoration counts.
7. First add/run a small lifecycle smoke test (10 or 50) to verify create/register/tick/unregister/release and no pool/world residue, then run 1000 dispersed and 1000 concentrated in the currently open Unity Editor if UnityMCP or an existing request mechanism is available. Do not start a second Unity instance against the same Library.
8. Preserve dirty user changes and avoid unrelated edits. Do not commit.
9. Update the central rendering plan and handoff/alignment docs with honest evidence. Clearly separate harness validity from performance result.
10. Run compile/self-check and inspect console. If current editor automation cannot drive the test, leave a runnable menu/request and report that runtime evidence is pending; do not claim PASS.

Before editing, inspect all relevant lifecycle and existing benchmark/editor-window patterns. Implement minimal cohesive changes with tests. Return a summary with exact files, commands/menu steps, and evidence paths.
