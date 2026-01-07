using System;
using System.Collections.Generic;

namespace BeatEmUpTemplate2D
{
    // 状态工厂 + 对象池
    public class StatePool
    {
        private readonly Dictionary<Type, Queue<StateNode>> _pool = new();
        private readonly int _maxPoolSize = 5;

        public T GetState<T>() where T : StateNode, new()
        {
            var type = typeof(T);

            if (_pool.TryGetValue(type, out var queue) && queue.Count > 0)
            {
                return (T)queue.Dequeue();
            }

            return new T();
        }

        public void ReturnState(StateNode state)
        {
            if (state == null) return;

            var type = state.GetType();

            if (!_pool.ContainsKey(type))
            {
                _pool[type] = new Queue<StateNode>();
            }

            // 重置状态
            state.stateStartTime = 0;
            state.unit = null;

            if (_pool[type].Count < _maxPoolSize)
            {
                _pool[type].Enqueue(state);
            }
        }
    }
}
