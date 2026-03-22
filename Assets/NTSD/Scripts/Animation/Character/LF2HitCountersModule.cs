using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// FLF 对齐：hit() 相关的计数器（fall / bdefend）。
    /// 纯数据层，不依赖 Animator/Mono。
    /// </summary>
    public sealed class LF2HitCountersModule
    {
        public int Fall { get; private set; }
        public int Bdefend { get; private set; }

        // 浮点累加器：FLF 的 fall/bdefend 恢复值是小数（-0.45/-0.5 每 TU）
        // 累加到整数阈值后才真正扣减，避免精度丢失
        private float _fallAccum;
        private float _bdefendAccum;

        public void Reset()
        {
            Fall = 0;
            Bdefend = 0;
            _fallAccum = 0f;
            _bdefendAccum = 0f;
        }

        public void AddFall(int amount)
        {
            Fall = Mathf.Max(0, Fall + amount);
        }

        public void ResetFall()
        {
            Fall = 0;
            _fallAccum = 0f;
        }

        public void AddBdefend(int amount)
        {
            Bdefend = Mathf.Max(0, Bdefend + amount);
        }

        public void ResetBdefend()
        {
            Bdefend = 0;
            _bdefendAccum = 0f;
        }

        /// <summary>直接赋值（对应 FLF: $.health.bdefend = value）</summary>
        public void SetBdefend(int value)
        {
            Bdefend = value;
        }

        /// <summary>
        /// fall 自然恢复（对应 FLF: if (fall > 0) fall += GC.recover.fall）
        /// amount 传入 NTSDGlobal.Gameplay.RecoverFall（负数）
        /// </summary>
        public void RecoverFall(float amount)
        {
            if (Fall <= 0) return;
            _fallAccum += amount;
            if (_fallAccum <= -1f)
            {
                int decrement = Mathf.FloorToInt(-_fallAccum);
                Fall = Mathf.Max(0, Fall - decrement);
                _fallAccum += decrement;
            }
        }

        /// <summary>
        /// bdefend 自然恢复（对应 FLF: if (bdefend > 0) bdefend += GC.recover.bdefend）
        /// amount 传入 NTSDGlobal.Gameplay.RecoverBdefend（负数）
        /// </summary>
        public void RecoverBdefend(float amount)
        {
            if (Bdefend <= 0) return;
            _bdefendAccum += amount;
            if (_bdefendAccum <= -1f)
            {
                int decrement = Mathf.FloorToInt(-_bdefendAccum);
                Bdefend = Mathf.Max(0, Bdefend - decrement);
                _bdefendAccum += decrement;
            }
        }
    }
}

