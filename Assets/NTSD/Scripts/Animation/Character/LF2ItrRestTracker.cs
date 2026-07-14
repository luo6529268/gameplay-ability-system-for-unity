using System.Collections.Generic;
using NTSD.Simulation;
using UnityEngine.Pool;

namespace NTSD.Animation
{
    /// <summary>
    /// 记录正式战斗流程中的 arest 和按攻击者区分的 vrest 冷却。
    /// 冷却递减由 SimulationWorld.VrestTickAll() 在每个 tick 开头统一驱动。
    /// </summary>
    public sealed class LF2ItrRestTracker
    {
        private int _arest = 0;
        private readonly Dictionary<int, int> _vrestByAttacker = new Dictionary<int, int>();

        /// <summary>
        /// 攻击者自身的命中冷却；大于 0 时不能再次执行 arest 门控的命中。
        /// </summary>
        public int Arest
        {
            get => _arest;
            set => _arest = value;
        }

        public void Reset()
        {
            _arest = 0;
            _vrestByAttacker.Clear();
        }

        public bool ArestTest() => _arest <= 0;

        public bool VrestTest(int attackerKey)
        {
            return !_vrestByAttacker.TryGetValue(attackerKey, out int v) || v <= 0;
        }

        /// <summary>
        /// 检查指定攻击者是否仍在受击者侧的 vrest 冷却中。
        /// </summary>
        public bool HasVrest(int attackerKey)
        {
            return _vrestByAttacker.TryGetValue(attackerKey, out int v) && v > 0;
        }

        public int GetVrest(int attackerKey)
        {
            return _vrestByAttacker.TryGetValue(attackerKey, out int value) ? value : 0;
        }

        /// <summary>
        /// 为指定攻击者设置受击者侧 vrest 冷却。
        /// </summary>
        public void SetVrest(int attackerKey, int value)
        {
            _vrestByAttacker[attackerKey] = value;
        }

        public void ArestUpdate(InteractionArea itr)
        {
            if (itr != null && itr.arest > 0)
            {
                _arest = itr.arest;
            }
            else if (itr == null || itr.vrest <= 0)
            {
                _arest = NTSDGlobal.Default.Character.ARest;
            }
        }

        public void VrestUpdate(int attackerKey, InteractionArea itr)
        {
            if (itr != null && itr.vrest > 0)
            {
                _vrestByAttacker[attackerKey] = itr.vrest;
            }
        }

        public void TickArest()
        {
            if (_arest > 0) _arest--;
        }

        public void TickVrestForAttacker(int attackerKey)
        {
            if (!_vrestByAttacker.TryGetValue(attackerKey, out int value) || value <= 0)
                return;

            _vrestByAttacker[attackerKey] = value - 1;
        }

        public void Tick()
        {
            TickArest();

            if (_vrestByAttacker.Count == 0) return;

            var keys = ListPool<int>.Get();
            keys.AddRange(_vrestByAttacker.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                int k = keys[i];
                if (_vrestByAttacker[k] > 0) _vrestByAttacker[k]--;
            }
            ListPool<int>.Release(keys);
        }
    }
}
