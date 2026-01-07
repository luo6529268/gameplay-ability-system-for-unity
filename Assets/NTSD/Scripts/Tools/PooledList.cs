using System.Collections.Generic;


namespace NTSD.Tools
{
    public class PooledList<T> : List<T>, IPoolable
    {
        public void OnSpawned() => Clear();
        public void OnRecycled()
        {
            for (int i = 0; i < Count; i++)
            {
                var item = this[i];
                if (item is IPoolable)
                {
                    // 调用泛型回收版本（类型安全，不使用 dynamic）
                    ReferencePoolManager.RecycleUnsafe((IPoolable)item);
                }
            }

            Clear();
        }

        public static PooledList<T> Get() => ReferencePoolManager.Spawn<PooledList<T>>();
        public static void Release(PooledList<T> list) => ReferencePoolManager.Recycle(list);
    }
}