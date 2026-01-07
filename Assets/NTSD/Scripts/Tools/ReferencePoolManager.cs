using System;
using System.Collections.Generic;

namespace NTSD.Tools
{
    /// <summary>
    /// 可统一管理所有类型对象池的全局引用池管理器。
    /// 用于动态创建、缓存、清理、统计。
    /// </summary>
    public static class ReferencePoolManager
    {
        private static readonly Dictionary<Type, object> _pools = new Dictionary<Type, object>();

        /// <summary>
        /// 获取指定类型的引用池（若不存在则自动创建）
        /// </summary>
        public static ReferencePool<T> GetPool<T>() where T : class, IPoolable, new()
        {
            var type = typeof(T);
            if (_pools.TryGetValue(type, out var pool))
                return (ReferencePool<T>)pool;

            var newPool = new ReferencePool<T>();
            _pools.Add(type, newPool);
            return newPool;
        }

        /// <summary>
        /// 从池中获取对象
        /// </summary>
        public static T Spawn<T>() where T : class, IPoolable, new()
        {
            return GetPool<T>().Get();
        }

        /// <summary>
        /// 回收对象
        /// </summary>
        public static void Recycle<T>(T obj) where T : class, IPoolable, new()
        {
            GetPool<T>().Release(obj);
        }

        /// <summary>
        /// 通用接口回收（当类型未知但实现了 IPoolable）
        /// </summary>
        public static void RecycleUnsafe(IPoolable obj)
        {
            if (obj == null) return;
            var type = obj.GetType();
            if (_pools.TryGetValue(type, out var poolObj))
            {
                var releaseMethod = poolObj.GetType().GetMethod("Release");
                releaseMethod?.Invoke(poolObj, new object[] { obj });
            }
            else
            {
                obj.OnRecycled(); // 即使没有池，也调用一次清理
            }
        }


        /// <summary>
        /// 清空所有池
        /// </summary>
        public static void ClearAll()
        {
            _pools.Clear();
        }
    }
}