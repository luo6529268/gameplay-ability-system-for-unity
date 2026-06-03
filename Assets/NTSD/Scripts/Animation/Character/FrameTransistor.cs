using NTSD.Animation.LF2Objects;
using System;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// Frame transition request wrapper.
    /// Unity still keeps request arbitration here, but the public state is synchronized to
    /// the C++ release entity fields: current frame, next frame, and wait counter.
    /// </summary>
    public class FrameTransistor
    {
        // C++ release frame advancement uses entity.wait_counter against frame.wait.
        // The current Unity call graph stores the requested frame wait as a remaining wait value;
        // waitCounter mirrors how many Trans() ticks have elapsed since the current frame request.
        private int wait = 1;
        private int waitCounter = 0;
        private int next = 999;
        private int requestPriority = 0;
        private int priorityRelease = 1;
        private bool switchDirAfterTrans;

        LF2Entity _entity;
        /// <summary>
        /// 构造函数（接受所有 LF2Entity 子类）
        /// </summary>
        public FrameTransistor(LF2Entity entity)
        {
            _entity = entity;
            SyncRuntime();
        }

        public int Next => next;
        public int Wait => wait;
        public int WaitCounter => waitCounter;

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
            if (au == 99) au = requestPriority;
            if (au >= requestPriority)
            {
                requestPriority = au;
                priorityRelease = outCount == 99 ? wait : outCount;
                wait = value < 0 ? 0 : value;
                waitCounter = 0;
                SyncRuntime();
            }
        }

        /// <summary>
        /// 对应 FLF inc_wait(inc, au, out)
        /// </summary>
        public void IncWait(int inc, int au = 0, int outCount = 1)
        {
            if (au == 99) au = requestPriority;
            if (au >= requestPriority)
            {
                requestPriority = au;
                priorityRelease = outCount == 99 ? wait : outCount;
                wait += inc;
                if (wait < 0) wait = 0;
                SyncRuntime();
            }
        }

        /// <summary>
        /// 对应 FLF set_next(value, au, out)
        /// </summary>
        public void SetNext(int value, int au = 0, int outCount = 1)
        {
            if (au == 99) au = requestPriority;
            if (au >= requestPriority)
            {
                NTSD.Tools.Log.LogState(_entity?.Name, "FrameRequest",
                    $"SetNext({value}) OK: priority {requestPriority}->{au}");
                requestPriority = au;
                priorityRelease = outCount == 99 ? wait : outCount;
                if (value < 0)
                {
                    value = -value;
                    switchDirAfterTrans = true;
                }
                next = value;
                SyncRuntime();
            }
            else
            {
                NTSD.Tools.Log.LogState(_entity?.Name, "FrameRequest",
                    $"SetNext({value}) BLOCKED: priority={au} < active={requestPriority}",
                    NTSD.Tools.Log.StateLogLevel.Warn);
            }
        }


        /// <summary>
        /// Releases a temporary high-priority frame request gate.
        /// </summary>
        public void ResetLock(int au = 0)
        {
            if (au == 99) au = requestPriority;
            if (au >= requestPriority)
            {
                requestPriority = 0;
                SyncRuntime();
            }
        }

        /// <summary>
        /// C++ release next-frame sentinel semantics: 999 and 1280 resolve to frame 0.
        /// </summary>
        public int NextFrameResolved()
        {
            var target = next;
            if (target == 999 || target == 1280) target = 0;
            return target;
        }

        /// <summary>
        /// Advances the pending frame request for one simulation tick.
        /// </summary>
        public void Trans()
        {
            NTSD.Tools.Log.LogState(_entity?.Name, "Trans", $"wait={wait} waitCounter={waitCounter} next={next} priority={requestPriority}");

            var oldPriority = requestPriority;
            priorityRelease--;
            if (priorityRelease == 0) requestPriority = 0;

            if (wait > 0)
            {
                wait--;
                waitCounter++;
                SyncRuntime();
                return;
            }

            if (next == 0)
            {
                NTSD.Tools.Log.LogState(_entity?.Name, "Trans", "STUCK: next==0", NTSD.Tools.Log.StateLogLevel.Error);
                SyncRuntime();
                return;
            }

            if (next == 1000)
            {
                _entity?.OnTransitDestroy();
                SyncRuntime();
                return;
            }

            if (next == 999 || next == 1280)
            {
                next = 0;
            }

            waitCounter = 0;
            _entity?.OnFrameTransit(next, switchDirAfterTrans, oldPriority);
            switchDirAfterTrans = false;
            SyncRuntime();

            // Existing Unity high-priority hit/jump requests consume one extra wait tick.
            if ((oldPriority == 10 || oldPriority == 11) && wait > 0)
            {
                wait--;
                waitCounter++;
                SyncRuntime();
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            wait = 1;
            waitCounter = 0;
            next = 999;
            requestPriority = 0;
            priorityRelease = 1;
            switchDirAfterTrans = false;
            SyncRuntime();
        }

        private void SyncRuntime()
        {
            if (_entity == null) return;
            _entity.Runtime.WaitCounter = waitCounter;
            _entity.Runtime.NextFrame = next;
        }
    }
}
