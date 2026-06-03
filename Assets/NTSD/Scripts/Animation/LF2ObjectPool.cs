using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
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

        // ========== 配置快捷访问 ==========
        private static GameConfig Cfg => GameConfig.Instance;

        // 缓存 prefab 引用，避免懒加载时 GameConfig.Instance 为 null
        private GameObject _cachedLF2ObjectPrefab;

        // ========== 生命周期 ==========

        protected override void Awake()
        {
            base.Awake();

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
                go = Instantiate(_cachedLF2ObjectPrefab,this.transform);
                go.layer = LayerMask.NameToLayer("Battle");
            }
            else
            {
                go = new GameObject("LF2Object");
                go.layer = LayerMask.NameToLayer("Battle");
                var entityModel = new GameObject("EntityModel");
                entityModel.layer = LayerMask.NameToLayer("Battle");
                entityModel.transform.SetParent(go.transform, false);
                entityModel.AddComponent<SpriteRenderer>();
                entityModel.AddComponent<LF2ObjectRenderer>();
            }

            go.SetActive(false);

            // LF2ObjectRenderer 挂在子节点 EntityModel 上，不在根节点
            var r = go.GetComponentInChildren<LF2ObjectRenderer>(true);
            if (r == null)
            {
                Log.Error("[LF2ObjectPool] EntityModel missing LF2ObjectRenderer");
                Destroy(go);
                return null;
            }

            // Shadow 已内嵌在 prefab 中，查找名为 Shadow 的子节点
            SpriteRenderer shadowRenderer = null;
            var shadowTransform = go.transform.Find("Shadow");
            if (shadowTransform != null)
                shadowRenderer = shadowTransform.GetComponent<SpriteRenderer>();

            r.SetShadowRenderer(shadowRenderer);

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

            //if (_activeRoot != null)
            go.transform.SetParent(this.transform, false);

            go.SetActive(true);
            _activeObjects.Add(go);
            EntityModel = go.GetComponentInChildren<LF2ObjectRenderer>(true);
            return go;
        }

        /// <summary>
        /// 批量预热接口（对齐反汇编 SceneManager_Init: 预分配 400 个实体实例）
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
                sr.sortingLayerName = "Object";
            }

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
            sr.gameObject.SetActive(false);
            _spritePool.Push(sr);
        }

        public string GetPoolStatus() =>
            $"Available: {_availableObjects.Count}, Active: {_activeObjects.Count}";
    }
}
