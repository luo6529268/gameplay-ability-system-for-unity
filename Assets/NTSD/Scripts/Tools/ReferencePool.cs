using System;
using System.Collections.Generic;

namespace NTSD.Tools
{
    public class ReferencePool<T> where T : class, IPoolable, new()
    {
        private readonly Stack<T> _stack = new Stack<T>();
        private readonly int _maxCount;

        public ReferencePool(int maxCount = 256)
        {
            _maxCount = Math.Max(4, maxCount);
        }

        public T Get()
        {
            T item = _stack.Count > 0 ? _stack.Pop() : new T();
            item.OnSpawned();
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            item.OnRecycled();
            if (_stack.Count < _maxCount)
                _stack.Push(item);
        }

        public int Count => _stack.Count;
    }
}