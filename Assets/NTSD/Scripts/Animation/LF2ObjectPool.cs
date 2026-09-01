using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Tools;
using NTSD.App;
using NTSD.Simulation;
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
        private const int DefaultBattlePoolCapacity =
            BattleRuntimeProfilePolicy.MobileRuntimeSlotCapacity;

        private Queue<GameObject> _availableObjects;
        private HashSet<GameObject> _activeObjects;
        private Dictionary<GameObject, float> _releaseTimeMap;
        private HashSet<SpriteRenderer> _activeSprites;
        private List<GameObject> _shutdownObjectScratch;
        private List<SpriteRenderer> _shutdownSpriteScratch;
        private float _lastCheckTime;

        private Stack<SpriteRenderer> _spritePool;
        private Material _spriteDefaultSharedMaterial;
        private bool _runtimeStateInvalidationLogged;
        private bool _battleCapacitySealed;
        private bool _acceptingRequests = true;
        private bool _quiesced;
        private int _preparedObjectCapacity;
        private int _preparedSpriteCapacity;
        private long _rejectedObjectFetchCount;
        private long _rejectedSpriteFetchCount;

        // ========== 配置快捷访问 ==========
        private static GameConfig Cfg => GameConfig.Instance;

        // 缓存 prefab 引用，避免懒加载时 GameConfig.Instance 为 null
        private GameObject _cachedLF2ObjectPrefab;

        // Read-only acceptance evidence; avoids editor tooling reflecting private pool state.
        public int AvailableObjectCountForAcceptance => _availableObjects?.Count ?? 0;
        public int ActiveObjectCountForAcceptance => _activeObjects?.Count ?? 0;
        public bool IsRuntimeStateValidForAcceptance => HasValidRuntimeState();
        public bool IsQuiescedForDiagnostics => _quiesced;
        public bool AcceptingRequestsForDiagnostics => _acceptingRequests;
        public int ActiveSpriteCountForAcceptance => _activeSprites?.Count ?? 0;

        // ========== 生命周期 ==========

        protected override void Awake()
        {
            base.Awake();
            NormalizeTransform(transform);

            _availableObjects = new Queue<GameObject>(DefaultBattlePoolCapacity);
            _activeObjects = new HashSet<GameObject>(DefaultBattlePoolCapacity);
            _releaseTimeMap = new Dictionary<GameObject, float>(DefaultBattlePoolCapacity);
            _spritePool = new Stack<SpriteRenderer>(32);
            _activeSprites = new HashSet<SpriteRenderer>(32);
            _shutdownObjectScratch = new List<GameObject>(DefaultBattlePoolCapacity);
            _shutdownSpriteScratch = new List<SpriteRenderer>(32);
            _acceptingRequests = true;
            _quiesced = false;
            _runtimeStateInvalidationLogged = false;

            // 缓存 prefab 引用 - 延迟到 CreateNewObject 时再获取
            _cachedLF2ObjectPrefab = null;

            for (int i = 0; i < (Cfg?.PoolInitialSize ?? 0); i++)
                CreateNewObject();
            _preparedObjectCapacity = _availableObjects.Count;

            int spritePoolSize = Cfg?.PoolInitialSpritePoolSize ?? 16;
            for (int i = 0; i < spritePoolSize; i++)
                CreateNewSpriteRenderer();
            _preparedSpriteCapacity = _spritePool.Count;
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

            _availableObjects.Enqueue(go);
            return r;
        }

        /// <summary>从池中获取对象（懒加载）</summary>
        public GameObject Get(out LF2ObjectRenderer EntityModel)
        {
            if (!_acceptingRequests)
            {
                _rejectedObjectFetchCount++;
                EntityModel = null;
                return null;
            }

            int maxPoolSize = Cfg?.PoolMaxSize ?? 200;

            GameObject go;
            EntityModel = null;
            if (_availableObjects.Count == 0)
            {
                if (_battleCapacitySealed)
                {
                    _rejectedObjectFetchCount++;
                    return null;
                }

                if (_activeObjects.Count >= maxPoolSize)
                    Log.Warn("[LF2ObjectPool] Pool over limit: active={0}/{1}, expanding.", _activeObjects.Count, maxPoolSize);
                CreateNewObject();
                if (_availableObjects.Count == 0)
                {
                    Log.Error("[LF2ObjectPool] CreateNewObject failed (active={0})", _activeObjects.Count);
                    return null;
                }
            }

            go = _availableObjects.Dequeue();

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
            if (!_acceptingRequests || _battleCapacitySealed || count <= 0)
                return;

            BattleCentralPresentationMountRegistry.PrepareCapacity(
                _availableObjects.Count + _activeObjects.Count + count);
            for (int i = 0; i < count; i++)
            {
                CreateNewObject();
                // 每实例化 5 个对象让出一帧，确保 Loading 动画不卡顿
                if (i % 5 == 0) await UniTask.Yield();
            }
            _preparedObjectCapacity = Mathf.Max(
                _preparedObjectCapacity,
                _availableObjects.Count + _activeObjects.Count);
            Log.Info("[LF2ObjectPool] Bulk Prewarm: {0} GameObjects", count);
        }

        public async UniTask PrepareCapacityAsync(int targetObjectCount, int targetSpriteCount)
        {
            if (!_acceptingRequests || _battleCapacitySealed)
                return;

            int normalizedObjectTarget = Mathf.Max(0, targetObjectCount);
            BattleCentralPresentationMountRegistry.PrepareCapacity(normalizedObjectTarget);
            int currentObjectCount = _availableObjects.Count + _activeObjects.Count;
            _activeObjects.EnsureCapacity(normalizedObjectTarget);
            _releaseTimeMap.EnsureCapacity(normalizedObjectTarget);
            if (_shutdownObjectScratch.Capacity < normalizedObjectTarget)
                _shutdownObjectScratch.Capacity = normalizedObjectTarget;
            int missingObjects = normalizedObjectTarget - currentObjectCount;
            if (missingObjects > 0)
            {
                for (int i = 0; i < missingObjects; i++)
                {
                    CreateNewObject();
                    if ((i + 1) % 5 == 0)
                        await UniTask.Yield();
                }
            }

            int normalizedSpriteTarget = Mathf.Max(0, targetSpriteCount);
            _activeSprites.EnsureCapacity(normalizedSpriteTarget);
            if (_shutdownSpriteScratch.Capacity < normalizedSpriteTarget)
                _shutdownSpriteScratch.Capacity = normalizedSpriteTarget;
            int missingSprites = normalizedSpriteTarget - _spritePool.Count;
            for (int i = 0; i < missingSprites; i++)
            {
                CreateNewSpriteRenderer();
                if ((i + 1) % 5 == 0)
                    await UniTask.Yield();
            }

            _preparedObjectCapacity = Mathf.Max(_preparedObjectCapacity, normalizedObjectTarget);
            _preparedSpriteCapacity = Mathf.Max(_preparedSpriteCapacity, normalizedSpriteTarget);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal void PrepareObjectCapacityImmediateForDiagnostics(int targetObjectCount)
        {
            if (!_acceptingRequests || _battleCapacitySealed)
            {
                throw new System.InvalidOperationException(
                    "Cannot prepare diagnostic object capacity after the battle seal is active.");
            }

            int normalizedTarget = Mathf.Max(0, targetObjectCount);
            BattleCentralPresentationMountRegistry.PrepareCapacity(normalizedTarget);
            _activeObjects.EnsureCapacity(normalizedTarget);
            _releaseTimeMap.EnsureCapacity(normalizedTarget);
            if (_shutdownObjectScratch.Capacity < normalizedTarget)
                _shutdownObjectScratch.Capacity = normalizedTarget;
            while (_availableObjects.Count + _activeObjects.Count < normalizedTarget)
                CreateNewObject();
            _preparedObjectCapacity = Mathf.Max(_preparedObjectCapacity, normalizedTarget);
        }
#endif

        public void SealBattleCapacity()
        {
            _battleCapacitySealed = true;
            BattleCentralPresentationMountRegistry.SealCapacity();
        }

        public void UnsealBattleCapacity()
        {
            _battleCapacitySealed = false;
            BattleCentralPresentationMountRegistry.UnsealCapacity();
        }

        public bool IsBattleCapacitySealed => _battleCapacitySealed;
        public int PreparedObjectCapacity => _preparedObjectCapacity;
        public int PreparedSpriteCapacity => _preparedSpriteCapacity;
        public long RejectedObjectFetchCount => _rejectedObjectFetchCount;
        public long RejectedSpriteFetchCount => _rejectedSpriteFetchCount;
        public long RejectedMountRegistrationCountForDiagnostics =>
            BattleCentralPresentationMountRegistry.RejectedMountRegistrationCount;
        public long RejectedMountOwnerBindingCountForDiagnostics =>
            BattleCentralPresentationMountRegistry.RejectedOwnerBindingCount;

        /// <summary>归还对象到池</summary>
        public void Release(LF2ObjectRenderer r)
        {
            if (r == null) return;

            r.ResetState();

            var go = r.transform.parent.gameObject;

            Transform poolParent = _poolRoot != null ? _poolRoot : transform;
            go.transform.SetParent(poolParent, false);
            NormalizeTransform(go.transform, resetScale: false);

            go.SetActive(false);
            _activeObjects.Remove(go);
            _availableObjects.Enqueue(go);
            _releaseTimeMap[go] = Time.time;
        }

        // ========== 超时卸载 ==========

        private void Update()
        {
            if (!HasValidRuntimeState())
            {
                if (!_runtimeStateInvalidationLogged)
                {
                    _runtimeStateInvalidationLogged = true;
                    Debug.LogError(
                        "[LF2ObjectPool] managed runtime state was invalidated; " +
                        "disabling the component until a clean Play Mode restart.");
                }
                enabled = false;
                return;
            }

            if (_quiesced || _battleCapacitySealed)
                return;

            int initialSize = Mathf.Max(Cfg?.PoolInitialSize ?? 0, _preparedObjectCapacity);
            float expireTime = Cfg?.PoolExpireTimeSeconds ?? 120f;
            float checkInterval = Cfg?.PoolCheckIntervalSeconds ?? 10f;

            if (_availableObjects.Count <= initialSize)
            {
                _releaseTimeMap.Clear();
                return;
            }

            if (Time.time - _lastCheckTime < checkInterval) return;
            _lastCheckTime = Time.time;

            int availableCount = _availableObjects.Count;
            int removableCount = availableCount - initialSize;
            int removedCount = 0;
            for (int i = 0; i < availableCount; i++)
            {
                GameObject obj = _availableObjects.Dequeue();

                if (removedCount < removableCount &&
                    _releaseTimeMap.TryGetValue(obj, out float t) &&
                    Time.time - t >= expireTime)
                {
                    _releaseTimeMap.Remove(obj);
                    Destroy(obj);
                    removedCount++;
                    continue;
                }

                _availableObjects.Enqueue(obj);
            }

            if (_availableObjects.Count <= initialSize)
                _releaseTimeMap.Clear();
        }

        // ========== Bucket B：SpriteRenderer 桶 ==========

        /// <summary>
        /// 从轻量 SpriteRenderer 桶取出一个 SpriteRenderer（懒加载）。
        /// 池空时创建新 GameObject 并挂载 SpriteRenderer，统一挂在 _spriteRoot 下（Inspector 指定，null 时挂在本对象上）。
        /// 取出后 SetActive(true)，不注册 SimulationWorld。
        /// </summary>
        private SpriteRenderer CreateNewSpriteRenderer()
        {
            var go = new GameObject("Spark");
            go.layer = LayerMask.NameToLayer("Battle");
            Transform parent = _spriteRoot != null ? _spriteRoot : transform;
            go.transform.SetParent(parent, false);
            var renderer = go.AddComponent<SpriteRenderer>();
            CaptureOrApplySpriteDefaultMaterial(renderer);
            LF2ObjectRenderer.NormalizeSpriteRendererState(
                renderer,
                _spriteDefaultSharedMaterial);
            renderer.sortingLayerName = "Object";
            renderer.gameObject.SetActive(false);
            _spritePool.Push(renderer);
            return renderer;
        }

        public SpriteRenderer GetSprite()
        {
            if (!_acceptingRequests)
            {
                _rejectedSpriteFetchCount++;
                return null;
            }

            SpriteRenderer sr;
            if (_spritePool.Count > 0)
            {
                sr = _spritePool.Pop();
            }
            else
            {
                if (_battleCapacitySealed)
                {
                    _rejectedSpriteFetchCount++;
                    return null;
                }

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
            _activeSprites.Add(sr);
            return sr;
        }

        /// <summary>
        /// 归还 SpriteRenderer 到轻量桶：清空 sprite，SetActive(false)，压栈。
        /// 防重复归还：已处于非激活状态则直接跳过。
        /// </summary>
        public void ReleaseSprite(SpriteRenderer sr)
        {
            if (sr == null) return;
            if (!_activeSprites.Remove(sr) && !sr.gameObject.activeSelf) return;
            sr.sprite = null;
            CaptureOrApplySpriteDefaultMaterial(sr);
            LF2ObjectRenderer.NormalizeSpriteRendererState(sr, _spriteDefaultSharedMaterial);
            sr.gameObject.SetActive(false);
            _spritePool.Push(sr);
        }

        public void BeginBattlePreparation()
        {
            _acceptingRequests = true;
            _quiesced = false;
            enabled = true;
        }

        public void BeginBattleShutdown()
        {
            _acceptingRequests = false;
        }

        public bool ReleaseAllActiveForShutdown(
            out int returnedRenderers,
            out int returnedSpriteRenderers,
            out string failureReason)
        {
            returnedRenderers = 0;
            returnedSpriteRenderers = 0;
            failureReason = string.Empty;
            if (!HasValidRuntimeState())
            {
                failureReason = "object-pool-runtime-state-is-invalid";
                return false;
            }

            _shutdownObjectScratch.Clear();
            foreach (GameObject activeObject in _activeObjects)
                _shutdownObjectScratch.Add(activeObject);

            for (int index = 0; index < _shutdownObjectScratch.Count; index++)
            {
                GameObject root = _shutdownObjectScratch[index];
                if (root == null)
                {
                    _activeObjects.Remove(root);
                    continue;
                }

                LF2ObjectRenderer renderer =
                    root.GetComponentInChildren<LF2ObjectRenderer>(true);
                LF2Entity entity = renderer?.LogicObject as LF2Entity;
                BattleLogicReferencePool referencePool =
                    entity?.RegisteredWorldForSimulation?.LogicReferencePool;
                if (renderer != null)
                {
                    Release(renderer);
                    referencePool?.Release(entity);
                    returnedRenderers++;
                    continue;
                }

                root.SetActive(false);
                _activeObjects.Remove(root);
                _availableObjects.Enqueue(root);
            }
            _shutdownObjectScratch.Clear();

            _shutdownSpriteScratch.Clear();
            foreach (SpriteRenderer activeSprite in _activeSprites)
                _shutdownSpriteScratch.Add(activeSprite);
            for (int index = 0; index < _shutdownSpriteScratch.Count; index++)
            {
                SpriteRenderer activeSprite = _shutdownSpriteScratch[index];
                if (activeSprite == null)
                {
                    _activeSprites.Remove(activeSprite);
                    continue;
                }
                ReleaseSprite(activeSprite);
                returnedSpriteRenderers++;
            }
            _shutdownSpriteScratch.Clear();

            if (_activeObjects.Count != 0 || _activeSprites.Count != 0)
            {
                failureReason = "active-pool-borrowers-remained-after-shutdown-release";
                return false;
            }

            return true;
        }

        public bool CompleteBattleQuiesce(out string failureReason)
        {
            if (_activeObjects?.Count != 0 || _activeSprites?.Count != 0)
            {
                failureReason = "pool-cannot-quiesce-with-active-borrowers";
                return false;
            }

            _acceptingRequests = false;
            _quiesced = true;
            _releaseTimeMap?.Clear();
            enabled = false;
            failureReason = string.Empty;
            return true;
        }

        public string GetPoolStatus()
        {
            return HasValidRuntimeState()
                ? $"Available: {_availableObjects.Count}, Active: {_activeObjects.Count}"
                : "Unavailable: managed runtime state invalidated";
        }

        private bool HasValidRuntimeState()
        {
            return _availableObjects != null &&
                   _activeObjects != null &&
                   _releaseTimeMap != null &&
                   _spritePool != null &&
                   _activeSprites != null &&
                   _shutdownObjectScratch != null &&
                   _shutdownSpriteScratch != null;
        }

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
