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

        public void Reset()
        {
            Fall = 0;
            Bdefend = 0;
        }

        public void AddFall(int amount)
        {
            Fall += Mathf.Abs(amount);
        }

        public void ResetFall()
        {
            Fall = 0;
        }

        public void AddBdefend(int amount)
        {
            Bdefend += Mathf.Abs(amount);
        }

        public void ResetBdefend()
        {
            Bdefend = 0;
        }
    }
}

