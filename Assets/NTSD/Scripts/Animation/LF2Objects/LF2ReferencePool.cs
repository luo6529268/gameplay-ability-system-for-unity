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
                obj.ObjectId = oid;
                obj.Reset();
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
            _activeObjects.Remove(obj);

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
