using NTSD.Animation.LF2Objects;
using System;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 帧转换器 - 对齐 FLF 的 lock/lockout 机制
    /// </summary>
    public class FrameTransistor
    {
        // FLF 初始值：wait=1, next=999, lock=0, lockout=1
        private int wait = 1;
        private int next = 999;
        private int lockLevel = 0;
        private int lockout = 1;
        private bool switchDirAfterTrans;

        LF2LivingObject _lF2LivingObject;
        /// <summary>
        /// 无参构造函数（用于 LF2LivingObject）
        /// </summary>
        public FrameTransistor(LF2LivingObject lF2LivingObject)
        {
            _lF2LivingObject = lF2LivingObject;
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
                NTSD.Tools.Log.LogState(_lF2LivingObject?.Name, "Lock",
                    $"SetNext({value}) OK: lock {lockLevel}→{au}");
                lockLevel = au;
                lockout = outCount == 99 ? wait : outCount;
                if (value < 0)
                {
                    value = -value;
                    switchDirAfterTrans = true;
                }
                next = value;
            }
            else
            {
                NTSD.Tools.Log.LogState(_lF2LivingObject?.Name, "Lock",
                    $"SetNext({value}) BLOCKED: au={au} < lock={lockLevel}",
                    NTSD.Tools.Log.StateLogLevel.Warn);
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
            NTSD.Tools.Log.LogState(_lF2LivingObject?.Name, "Trans", $"wait={wait} next={next} lock={lockLevel}");

            var oldLock = lockLevel;
            lockout--;
            if (lockout == 0) lockLevel = 0;

            if (wait > 0)
            {
                wait--;
                return;
            }

            if (next == 0)
            {
                NTSD.Tools.Log.LogState(_lF2LivingObject?.Name, "Trans", "STUCK: next==0", NTSD.Tools.Log.StateLogLevel.Error);
                return;
            }

            if (next == 1000)
            {
                _lF2LivingObject?.OnTransitDestroy();
                return;
            }

            if (next == 999 || next == 1280)
            {
                next = 0;
            }

            // 调用帧转换回调
            _lF2LivingObject?.OnFrameTransit(next, switchDirAfterTrans, oldLock);
            switchDirAfterTrans = false;

            // FLF 特例：oldlock 为 10 或 11 时，wait>0 额外减 1
            if ((oldLock == 10 || oldLock == 11) && wait > 0)
            {
                wait--;
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            wait = 1;
            next = 999;
            lockLevel = 0;
            lockout = 1;
            switchDirAfterTrans = false;
        }
    }
}
