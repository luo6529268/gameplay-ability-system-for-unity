using UnityEngine;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Tools;
using NTSD.App;

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
        private LinkedList<LF2ObjectRenderer> _availableObjects;
        private HashSet<LF2ObjectRenderer> _activeObjects;
        private Dictionary<LF2ObjectRenderer, float> _releaseTimeMap;
        private float _lastCheckTime;

        private Stack<SpriteRenderer> _spritePool;

        // ========== 配置快捷访问 ==========
        private static GameConfig Cfg => GameConfig.Instance;

        // ========== 生命周期 ==========

        protected override void Awake()
        {
            base.Awake();

            _availableObjects = new LinkedList<LF2ObjectRenderer>();
            _activeObjects = new HashSet<LF2ObjectRenderer>();
            _releaseTimeMap = new Dictionary<LF2ObjectRenderer, float>();
            _spritePool = new Stack<SpriteRenderer>(32);

            for (int i = 0; i < (Cfg?.PoolInitialSize ?? 0); i++)
                CreateNewObject();

            int spritePoolSize = Cfg?.PoolInitialSpritePoolSize ?? 16;
            for (int i = 0; i < spritePoolSize; i++)
            {
                var go = new GameObject("Spark");
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
            GameObject go;
            var lf2ObjectPrefab = NTSD.App.GameConfig.Instance?.LF2ObjectPrefab;
            if (lf2ObjectPrefab != null)
            {
                go = Instantiate(lf2ObjectPrefab, _poolRoot);
            }
            else
            {
                go = new GameObject("LF2Object");
                go.MMGetOrAddComponent<SpriteRenderer>();
                go.AddComponent<LF2ObjectRenderer>();
                if (_poolRoot != null)
                    go.transform.SetParent(_poolRoot, false);
            }

            // 创建 shadow 子节点
            SpriteRenderer shadowRenderer = null;
            var shadowPrefab = NTSD.App.GameConfig.Instance?.ShadowPrefab;
            if (shadowPrefab != null)
            {
                var shadowGo = Instantiate(shadowPrefab, go.transform);
                shadowGo.transform.localPosition = Vector3.zero;
                shadowRenderer = shadowGo.GetComponent<SpriteRenderer>();
            }

            go.SetActive(false);

            var r = go.GetComponent<LF2ObjectRenderer>();
            if (r == null)
            {
                Log.Error("[LF2ObjectPool] GameObject missing LF2ObjectRenderer");
                Destroy(go);
                return null;
            }

            r.SetShadowRenderer(shadowRenderer);

            _availableObjects.AddLast(r);
            return r;
        }

        /// <summary>从池中获取对象（懒加载）</summary>
        public LF2ObjectRenderer Get()
        {
            LF2ObjectRenderer r;
            int maxPoolSize = Cfg?.PoolMaxSize ?? 200;

            if (_availableObjects.Count == 0)
            {
                if (_activeObjects.Count >= maxPoolSize)
                {
                    Log.Warn("[LF2ObjectPool] Pool limit reached ({0})", maxPoolSize);
                    return null;
                }
                r = CreateNewObject();
            }
            else
            {
                r = _availableObjects.First.Value;
                _availableObjects.RemoveFirst();
            }

            if (r == null) return null;

            if (_activeRoot != null)
                r.transform.SetParent(_activeRoot, false);

            r.gameObject.SetActive(true);
            _activeObjects.Add(r);
            return r;
        }

        /// <summary>归还对象到池</summary>
        public void Release(LF2ObjectRenderer r)
        {
            if (r == null) return;

            r.ResetState();

            if (_poolRoot != null)
                r.transform.SetParent(_poolRoot, false);

            _activeObjects.Remove(r);
            _availableObjects.AddLast(r);
            _releaseTimeMap[r] = Time.time;
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
                    Destroy(obj.gameObject);

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
