using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using NTSD.Tools;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 逻辑对象引用池（纯 C# 对象池）
    /// 负责复用 LF2SpecialAttack、LF2LightWeapon 等逻辑层对象
    /// 避免频繁创建和 GC
    ///
    /// 与 LF2ObjectPool 的区别：
    /// - LF2ObjectPool: 管理 GameObject（LF2ObjectRenderer，实例对象池）
    /// - LF2ObjectLogicPool: 管理纯 C# 对象（ILF2Object，引用池）
    /// </summary>
    public class LF2ObjectLogicPool : MMSingleton<LF2ObjectLogicPool>
    {
        // ========== 配置 ==========

        [Header("预热配置")]
        [SerializeField] private int _initialPoolSize = 50;

        // ========== 引用池（按类型分组）==========

        private Dictionary<LF2ObjectType, LinkedList<ILF2Object>> _availablePools;
        private HashSet<ILF2Object> _activeObjects;

        // ========== 初始化 ==========

        protected override void Awake()
        {
            base.Awake();

            _availablePools = new Dictionary<LF2ObjectType, LinkedList<ILF2Object>>();
            _activeObjects = new HashSet<ILF2Object>();

            // 初始化各类型的池
            _availablePools[LF2ObjectType.LightWeapon] = new LinkedList<ILF2Object>();
            _availablePools[LF2ObjectType.HeavyWeapon] = new LinkedList<ILF2Object>();
            _availablePools[LF2ObjectType.SpecialAttack] = new LinkedList<ILF2Object>();

            PrewarmPool();
        }

        /// <summary>
        /// 预热池（创建初始对象）
        /// </summary>
        private void PrewarmPool()
        {
            // 预创建轻武器
            for (int i = 0; i < _initialPoolSize / 3; i++)
                CreateNewObject(LF2ObjectType.LightWeapon);

            // 预创建重武器
            for (int i = 0; i < _initialPoolSize / 3; i++)
                CreateNewObject(LF2ObjectType.HeavyWeapon);

            // 预创建特殊攻击
            for (int i = 0; i < _initialPoolSize / 3; i++)
                CreateNewObject(LF2ObjectType.SpecialAttack);

            Log.Info("[LF2ObjectLogicPool] Prewarmed: {0} logic objects", _initialPoolSize);
        }

        /// <summary>
        /// 创建新的逻辑对象
        /// </summary>
        private ILF2Object CreateNewObject(LF2ObjectType objectType)
        {
            ILF2Object obj = null;

            switch (objectType)
            {
                case LF2ObjectType.LightWeapon:
                    obj = new LF2LightWeapon();
                    break;

                case LF2ObjectType.HeavyWeapon:
                    obj = new LF2HeavyWeapon();
                    break;

                case LF2ObjectType.SpecialAttack:
                    obj = new LF2SpecialAttack();
                    break;

                default:
                    Log.Error("[LF2ObjectLogicPool] Unsupported object type: {0}", objectType);
                    return null;
            }

            if (_availablePools.TryGetValue(objectType, out var pool))
            {
                pool.AddLast(obj);
            }

            return obj;
        }

        // ========== 公共 API ==========

        /// <summary>
        /// 获取对象（从池中取出或创建新对象）
        /// </summary>
        /// <param name="objectType">对象类型枚举</param>
        /// <param name="oid">对象 ID（数据定义 ID）</param>
        /// <returns>逻辑对象</returns>
        public ILF2Object Get(LF2ObjectType objectType, int oid)
        {
            ILF2Object obj = null;

            // 尝试从池中获取
            if (_availablePools.TryGetValue(objectType, out var pool) && pool.Count > 0)
            {
                obj = pool.First.Value;
                pool.RemoveFirst();
            }
            else
            {
                // 池中没有可用对象，创建新对象
                obj = CreateNewObject(objectType);
            }

            if (obj != null)
            {
                obj.ObjectId = oid;
                _activeObjects.Add(obj);
                // 移除热路径日志，避免性能问题
            }

            return obj;
        }

        /// <summary>
        /// 归还对象到池中
        /// </summary>
        /// <param name="obj">要归还的对象</param>
        public void Release(ILF2Object obj)
        {
            if (obj == null) return;

            // 重置对象状态
            obj.Reset();

            // 从激活集合移除
            _activeObjects.Remove(obj);

            // 归还到对应类型的池
            if (_availablePools.TryGetValue(obj.ObjectTypeEnum, out var pool))
            {
                pool.AddLast(obj);
                // 移除热路径日志，避免性能问题
            }
        }

        /// <summary>
        /// 获取激活对象数量
        /// </summary>
        public int ActiveCount => _activeObjects.Count;

        /// <summary>
        /// 获取指定类型的可用对象数量
        /// </summary>
        public int GetAvailableCount(LF2ObjectType objectType)
        {
            if (_availablePools.TryGetValue(objectType, out var pool))
                return pool.Count;
            return 0;
        }
    }
}
