using System.Collections.Generic;
using UnityEngine.Pool;

namespace NTSD.Animation
{
    /// <summary>
    /// FLF/LF2 ITR Rest（arest/vrest）追踪器（数据层，不继承 Mono）。
    /// 对齐 FLF: livingobject.js 的 itr_arest_update / itr_vrest_update / TU_update 递减。
    /// </summary>
    public sealed class LF2ItrRestTracker
    {
        // FLF global.js: GC.default.character.arest
        private const int FLF_DEFAULT_CHARACTER_AREST = 7;

        private int _arest = 0;
        private readonly Dictionary<int, int> _vrestByAttacker = new Dictionary<int, int>();

        /// <summary>
        /// 攻击休息时间（对应 FLF $.itr.arest）
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
        /// 检查是否有受击休息（对应 FLF $.itr.vrest[uid]）
        /// </summary>
        public bool HasVrest(int attackerStableId)
        {
            return _vrestByAttacker.TryGetValue(attackerStableId, out int v) && v > 0;
        }

        /// <summary>
        /// 设置受击休息（对应 FLF $.itr.vrest[uid] = value）
        /// </summary>
        public void SetVrest(int attackerStableId, int value)
        {
            _vrestByAttacker[attackerStableId] = value;
        }

        public void ArestUpdate(InteractionArea itr)
        {
            // FLF: if (ITR && ITR.arest) arest=ITR.arest; else if (!ITR || !ITR.vrest) arest=default
            if (itr != null && itr.arest > 0)
            {
                _arest = itr.arest;
            }
            else if (itr == null || itr.vrest <= 0)
            {
                _arest = FLF_DEFAULT_CHARACTER_AREST;
            }
        }

        public void VrestUpdate(int attackerStableId, InteractionArea itr)
        {
            // FLF: if (ITR && ITR.vrest) vrest[uid]=ITR.vrest
            if (itr != null && itr.vrest > 0)
            {
                _vrestByAttacker[attackerStableId] = itr.vrest;
            }
        }

        public void Tick()
        {
            // 对齐 FLF livingobject.js: 每 TU 递减 vrest/arest
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

