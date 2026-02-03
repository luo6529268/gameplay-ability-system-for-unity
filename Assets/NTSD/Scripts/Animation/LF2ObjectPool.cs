using UnityEngine;
using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.Tools;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2 对象池（MonoBehaviour 单例）
    /// 管理通用 LF2Object Prefab 的预实例化和复用
    ///
    /// 参考：FLF 游戏中除了 character 用特殊 prefab，特效和武器都用同一个 prefab
    /// </summary>
    public class LF2ObjectPool : MMSingleton<LF2ObjectPool>
    {
        // ========== 配置 ==========
        [Header("对象池配置")]
        [SerializeField] private GameObject _lf2ObjectPrefab;  // 通用 Prefab
        [SerializeField] private int _initialPoolSize = 50;    // 初始池大小
        [SerializeField] private int _maxPoolSize = 200;       // 最大池大小

        [Header("超时卸载配置")]
        [SerializeField] private float _expireTimeSeconds = 120f;   // 超时时间：120 秒
        [SerializeField] private float _checkIntervalSeconds = 10f; // 检查频率：每 10 秒

        [Header("父节点配置")]
        [SerializeField] private Transform _poolRoot;          // 未激活对象的父节点（预热/归还时）
        [SerializeField] private Transform _activeRoot;        // 激活对象的父节点（使用时）

        // ========== 池数据结构 ==========
        private LinkedList<LF2ObjectRenderer> _availableObjects;
        private HashSet<LF2ObjectRenderer> _activeObjects;
        private Dictionary<LF2ObjectRenderer, float> _releaseTimeMap;
        private float _lastCheckTime;

        // ========== 生命周期 ==========

        protected override void Awake()
        {
            base.Awake();

            _availableObjects = new LinkedList<LF2ObjectRenderer>();
            _activeObjects = new HashSet<LF2ObjectRenderer>();
            _releaseTimeMap = new Dictionary<LF2ObjectRenderer, float>();

            PrewarmPool();
        }

        // ========== 核心方法 ==========

        /// <summary>
        /// 预热对象池（启动时调用）
        /// </summary>
        private void PrewarmPool()
        {
            for (int i = 0; i < _initialPoolSize; i++)
            {
                CreateNewObject();
            }
        }

        /// <summary>
        /// 创建新对象并加入池
        /// </summary>
        private LF2ObjectRenderer CreateNewObject()
        {
            GameObject obj = Instantiate(_lf2ObjectPrefab, _poolRoot);
            obj.SetActive(false);

            var renderer = obj.GetComponent<LF2ObjectRenderer>();
            if (renderer == null)
            {
                Log.Error("[LF2ObjectPool] Prefab missing LF2ObjectRenderer component");
                Destroy(obj);
                return null;
            }

            _availableObjects.AddLast(renderer);
            return renderer;
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public LF2ObjectRenderer Get()
        {
            LF2ObjectRenderer renderer;

            // 如果池为空，创建新对象（如果未达上限）
            if (_availableObjects.Count == 0)
            {
                if (_activeObjects.Count >= _maxPoolSize)
                {
                    Log.Warn("[LF2ObjectPool] Pool limit reached, cannot create more objects");
                    return null;
                }
                renderer = CreateNewObject();
            }
            else
            {
                // 从链表头取出
                renderer = _availableObjects.First.Value;
                _availableObjects.RemoveFirst();
            }

            // 移到激活父节点并激活对象
            if (_activeRoot != null)
            {
                renderer.transform.SetParent(_activeRoot, false);
            }
            renderer.gameObject.SetActive(true);
            _activeObjects.Add(renderer);

            return renderer;
        }

        /// <summary>
        /// 归还对象到池
        /// </summary>
        public void Release(LF2ObjectRenderer renderer)
        {
            if (renderer == null) return;

            renderer.ResetState();

            if (_poolRoot != null)
            {
                renderer.transform.SetParent(_poolRoot, false);
            }

            _activeObjects.Remove(renderer);
            _availableObjects.AddLast(renderer);
            _releaseTimeMap[renderer] = Time.time;
        }

        // ========== 调试信息 ==========

        private void Update()
        {
            if (_availableObjects.Count <= _initialPoolSize)
            {
                if (_releaseTimeMap.Count > 0)
                {
                    _releaseTimeMap.Clear();
                }
                return;
            }

            if (Time.time - _lastCheckTime < _checkIntervalSeconds)
            {
                return;
            }
            _lastCheckTime = Time.time;

            var node = _availableObjects.First;
            while (node != null)
            {
                var next = node.Next;
                var obj = node.Value;

                if (_releaseTimeMap.TryGetValue(obj, out float releaseTime))
                {
                    if (Time.time - releaseTime >= _expireTimeSeconds)
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
                }

                node = next;
            }
        }

        /// <summary>
        /// 获取池状态（调试用）
        /// </summary>
        public string GetPoolStatus()
        {
            return $"Available: {_availableObjects.Count}, Active: {_activeObjects.Count}";
        }
    }
}
