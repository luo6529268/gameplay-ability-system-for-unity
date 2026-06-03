using System.Collections.Generic;
using NTSD.Simulation;
using UnityEngine.Pool;

namespace NTSD.Animation
{
    /// <summary>
    /// Tracks release battle hit cooldowns for arest and per-attacker vrest.
    /// </summary>
    public sealed class LF2ItrRestTracker
    {
        private int _arest = 0;
        private readonly Dictionary<int, int> _vrestByAttacker = new Dictionary<int, int>();

        /// <summary>
        /// Attacker cooldown before this entity may apply another arest-gated hit.
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

        public bool VrestTest(int attackerStableId)
        {
            return !_vrestByAttacker.TryGetValue(attackerStableId, out int v) || v <= 0;
        }

        /// <summary>
        /// Checks whether the specified attacker is still under victim-side rest.
        /// </summary>
        public bool HasVrest(int attackerStableId)
        {
            return _vrestByAttacker.TryGetValue(attackerStableId, out int v) && v > 0;
        }

        /// <summary>
        /// Sets victim-side rest for the specified attacker.
        /// </summary>
        public void SetVrest(int attackerStableId, int value)
        {
            _vrestByAttacker[attackerStableId] = value;
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

        public void VrestUpdate(int attackerStableId, InteractionArea itr)
        {
            if (itr != null && itr.vrest > 0)
            {
                _vrestByAttacker[attackerStableId] = itr.vrest;
            }
        }

        public void Tick()
        {
            if (_arest > 0) _arest--;

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
