using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Tools;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2 对象池（MonoBehaviour 单例）
    /// 管理 LF2ObjectRenderer 的复用，不依赖预设 Prefab。
    /// 若 Inspector 中指定了 _lf2ObjectPrefab 则使用它，否则运行时动态创建 GameObject。
    /// </summary>
    public class LF2ObjectPool : MMSingleton<LF2ObjectPool>
    {
        // ========== 配置 ==========
        [Header("对象池配置")]
        [SerializeField] private GameObject _lf2ObjectPrefab;  // 可选：自定义 Prefab，不填则运行时动态创建
        [SerializeField] private int _initialPoolSize = 0;     // 初始预热数量，0 表示完全懒加载
        [SerializeField] private int _maxPoolSize = 200;

        [Header("超时卸载配置")]
        [SerializeField] private float _expireTimeSeconds = 120f;
        [SerializeField] private float _checkIntervalSeconds = 10f;

        [Header("父节点配置")]
        [SerializeField] private Transform _poolRoot;
        [SerializeField] private Transform _activeRoot;
        [SerializeField] private Transform _spriteRoot;  // Bucket B SpriteRenderer 挂载根节点（null 时挂在 LF2ObjectPool 自身）
        [SerializeField] private int _initialSpritePoolSize = 16; // 预热 SpriteRenderer 数量

        // ========== 池数据结构 ==========
        private LinkedList<LF2ObjectRenderer> _availableObjects;
        private HashSet<LF2ObjectRenderer> _activeObjects;
        private Dictionary<LF2ObjectRenderer, float> _releaseTimeMap;
        private float _lastCheckTime;

        // Bucket B：轻量 SpriteRenderer 桶（供 SparkRenderer 使用，懒加载，无超时卸载）
        private Stack<SpriteRenderer> _spritePool;

        // ========== 生命周期 ==========

        protected override void Awake()
        {
            base.Awake();

            _availableObjects = new LinkedList<LF2ObjectRenderer>();
            _activeObjects = new HashSet<LF2ObjectRenderer>();
            _releaseTimeMap = new Dictionary<LF2ObjectRenderer, float>();
            _spritePool = new Stack<SpriteRenderer>(32);

            for (int i = 0; i < _initialPoolSize; i++)
                CreateNewObject();

            for (int i = 0; i < _initialSpritePoolSize; i++)
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
            if (_lf2ObjectPrefab != null)
            {
                go = Instantiate(_lf2ObjectPrefab, _poolRoot);
            }
            else
            {
                go = new GameObject("LF2Object");
                SpriteRenderer spriteRenderer = go.MMGetOrAddComponent<SpriteRenderer>();
                go.AddComponent<LF2ObjectRenderer>();
                if (_poolRoot != null)
                    go.transform.SetParent(_poolRoot, false);
            }

            go.SetActive(false);

            var r = go.GetComponent<LF2ObjectRenderer>();
            if (r == null)
            {
                Log.Error("[LF2ObjectPool] GameObject missing LF2ObjectRenderer");
                Destroy(go);
                return null;
            }

            _availableObjects.AddLast(r);
            return r;
        }

        /// <summary>从池中获取对象（懒加载）</summary>
        public LF2ObjectRenderer Get()
        {
            LF2ObjectRenderer r;

            if (_availableObjects.Count == 0)
            {
                if (_activeObjects.Count >= _maxPoolSize)
                {
                    Log.Warn("[LF2ObjectPool] Pool limit reached ({0})", _maxPoolSize);
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
            if (_availableObjects.Count <= _initialPoolSize)
            {
                _releaseTimeMap.Clear();
                return;
            }

            if (Time.time - _lastCheckTime < _checkIntervalSeconds) return;
            _lastCheckTime = Time.time;

            var node = _availableObjects.First;
            while (node != null)
            {
                var next = node.Next;
                var obj = node.Value;

                if (_releaseTimeMap.TryGetValue(obj, out float t) &&
                    Time.time - t >= _expireTimeSeconds)
                {
                    _availableObjects.Remove(node);
                    _releaseTimeMap.Remove(obj);
                    Destroy(obj.gameObject);

                    if (_availableObjects.Count <= _initialPoolSize)
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
