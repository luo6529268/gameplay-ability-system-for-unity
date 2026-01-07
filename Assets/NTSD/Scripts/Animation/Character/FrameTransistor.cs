using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 帧转换器 - 对齐 FLF 的 lock/lockout 机制
    /// </summary>
    public class FrameTransistor
    {
        private readonly LF2CharacterAnimator animator;

        // FLF 初始值：wait=1, next=999, lock=0, lockout=1
        private int wait = 1;
        private int next = 999;
        private int lockLevel = 0;
        private int lockout = 1;
        private bool switchDirAfterTrans;

        public FrameTransistor(LF2CharacterAnimator animator)
        {
            this.animator = animator;
        }

        public int Next => next;
        public int Wait => wait;


        /// <summary>
        /// 对应 FLF frame(F, au) = set_next + set_wait(0)
        /// </summary>
        public void Frame(int frameId, int au = 0)
        {
            SetNext(frameId, au);
            SetWait(0, au);
        }

        /// <summary>
        /// 对应 FLF set_wait(value, au, out)
        /// </summary>
        public void SetWait(int value, int au = 0, int outCount = 1)
        {
            if (au == 99) au = lockLevel;
            if (au >= lockLevel)
            {
                lockLevel = au;
                lockout = outCount == 99 ? wait : outCount;
                wait = value < 0 ? 0 : value;
            }
        }

        /// <summary>
        /// 对应 FLF inc_wait(inc, au, out)
        /// </summary>
        public void IncWait(int inc, int au = 0, int outCount = 1)
        {
            if (au == 99) au = lockLevel;
            if (au >= lockLevel)
            {
                lockLevel = au;
                lockout = outCount == 99 ? wait : outCount;
                wait += inc;
                if (wait < 0) wait = 0;
            }
        }

        /// <summary>
        /// 对应 FLF set_next(value, au, out)
        /// </summary>
        public void SetNext(int value, int au = 0, int outCount = 1)
        {
            if (au == 99) au = lockLevel;
            if (au >= lockLevel)
            {
                lockLevel = au;
                lockout = outCount == 99 ? wait : outCount;
                if (value < 0)
                {
                    value = -value;
                    switchDirAfterTrans = true;
                }
                next = value;
            }
        }


        /// <summary>
        /// 对应 FLF reset_lock
        /// </summary>
        public void ResetLock(int au = 0)
        {
            if (au == 99) au = lockLevel;
            if (au >= lockLevel)
            {
                lockLevel = 0;
            }
        }

        /// <summary>
        /// FLF 的 next_frame_D 语义：把 999/1280 映射为 0
        /// </summary>
        public int NextFrameResolved()
        {
            var target = next;
            if (target == 999 || target == 1280) target = 0;
            return target;
        }

        /// <summary>
        /// 对应 FLF trans.trans()
        /// </summary>
        public void Trans()
        {
            var oldLock = lockLevel;
            lockout--;
            if (lockout == 0) lockLevel = 0;

            if (wait > 0)
            {
                wait--;
                return;
            }

            if (next == 0) return;

            if (next == 1000)
            {
                // 特例：生命值归零，切换到死亡状态
                // 生命周期由上层处理
                return;
            }

            if (next == 999 || next == 1280)
            {
                next = 0;
            }

            animator.FrameTransitInternal(next, switchDirAfterTrans, oldLock);
            switchDirAfterTrans = false;

            // FLF 特例：oldlock 为 10 或 11 时，wait>0 额外减 1
            if ((oldLock == 10 || oldLock == 11) && wait > 0)
            {
                wait --;
            }
        }
    }
}
