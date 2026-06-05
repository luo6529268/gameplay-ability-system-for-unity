using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    /// <summary>
    /// 帧切换请求和 C++ release 风格的 frame_tick 适配器。
    /// wait_counter 保存上一 tick 结束时的 frame id；attacking 才是 wait 计数。
    /// </summary>
    public class FrameTransistor
    {
        private int wait = 1;
        private int waitCounter = 0;
        private int next = 999;
        private int requestPriority = 0;
        private int priorityRelease = 1;
        private bool switchDirAfterTrans;
        private bool hasFrameRequest;

        private readonly LF2Entity _entity;

        public FrameTransistor(LF2Entity entity)
        {
            _entity = entity;
            waitCounter = _entity?.Frame?.N ?? 0;
            SyncRuntime();
        }

        public int Next => next;
        public int Wait => wait;
        public int WaitCounter => waitCounter;

        /// <summary>
        /// 请求切换到指定帧，并让下一次 Trans 立即处理。
        /// </summary>
        public void Frame(int frameId, int au = 0)
        {
            SetNext(frameId, au);
            SetWait(0, au);
        }

        /// <summary>
        /// 设置当前帧 wait 值。C++ 的攻击计数不因改 wait 自动清零。
        /// </summary>
        public void SetWait(int value, int au = 0, int outCount = 1)
        {
            if (au == 99) au = requestPriority;
            if (au < requestPriority) return;

            requestPriority = au;
            priorityRelease = outCount == 99 ? wait : outCount;
            wait = value < 0 ? 0 : value;
            SyncRuntime();
        }

        /// <summary>
        /// 调整当前帧 wait 值。
        /// </summary>
        public void IncWait(int inc, int au = 0, int outCount = 1)
        {
            if (au == 99) au = requestPriority;
            if (au < requestPriority) return;

            requestPriority = au;
            priorityRelease = outCount == 99 ? wait : outCount;
            wait += inc;
            if (wait < 0) wait = 0;
            SyncRuntime();
        }

        /// <summary>
        /// 设置下一帧请求。
        /// </summary>
        public void SetNext(int value, int au = 0, int outCount = 1)
        {
            if (au == 99) au = requestPriority;
            if (au < requestPriority)
            {
                NTSD.Tools.Log.LogState(_entity?.Name, "FrameRequest",
                    $"SetNext({value}) BLOCKED: priority={au} < active={requestPriority}",
                    NTSD.Tools.Log.StateLogLevel.Warn);
                return;
            }

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
            hasFrameRequest = true;
            SyncRuntime();
        }

        /// <summary>
        /// 直接写帧后的同步入口。directWaitCounter 对应 C++ Entity::wait_counter，
        /// 也就是上一 tick 保存的 frame id，不是等待倒计时。
        /// </summary>
        public void SyncDirectFrameData(int frameWait, int frameNext, int directWaitCounter = int.MinValue)
        {
            wait = frameWait < 0 ? 0 : frameWait;
            next = frameNext;
            if (directWaitCounter != int.MinValue)
                waitCounter = directWaitCounter;

            requestPriority = 0;
            priorityRelease = 1;
            switchDirAfterTrans = false;
            hasFrameRequest = false;
            SyncRuntime();
        }

        /// <summary>
        /// 释放临时高优先级帧请求门。
        /// </summary>
        public void ResetLock(int au = 0)
        {
            if (au == 99) au = requestPriority;
            if (au < requestPriority) return;

            requestPriority = 0;
            SyncRuntime();
        }

        /// <summary>
        /// C++ release next 哨兵语义：999 和 1280 解析为 frame 0。
        /// </summary>
        public int NextFrameResolved()
        {
            int target = next;
            if (target == 999 || target == 1280) target = 0;
            return target;
        }

        /// <summary>
        /// 按 C++ release frame_tick 顺序推进：
        /// frame 变化先清 attacking，然后 attacking++，超过 wait 才按 next 换帧，
        /// 最后 wait_counter 同步为当前 frame id。
        /// </summary>
        public void Trans()
        {
            NTSD.Tools.Log.LogState(_entity?.Name, "Trans",
                $"wait={wait} waitCounterFrame={waitCounter} attacking={_entity?.AttackingCounter ?? 0} next={next} priority={requestPriority}");

            int oldPriority = requestPriority;
            priorityRelease--;
            if (priorityRelease == 0) requestPriority = 0;

            if (_entity == null)
            {
                SyncRuntime();
                return;
            }

            int currentFrame = _entity.Frame?.N ?? 0;
            if (currentFrame != waitCounter)
                _entity.AttackingCounter = 0;

            _entity.AttackingCounter++;

            if (_entity.AttackingCounter <= wait)
            {
                waitCounter = currentFrame;
                SyncRuntime();
                return;
            }

            _entity.AttackingCounter = 0;

            if (next == 0 && !hasFrameRequest)
            {
                waitCounter = currentFrame;
                SyncRuntime();
                return;
            }

            if (next == 1000)
            {
                _entity.OnTransitDestroy();
                waitCounter = _entity.Frame?.N ?? currentFrame;
                hasFrameRequest = false;
                SyncRuntime();
                return;
            }

            if (next == 999 || next == 1280)
                next = 0;

            _entity.OnFrameTransit(next, switchDirAfterTrans, oldPriority);
            switchDirAfterTrans = false;
            hasFrameRequest = false;
            waitCounter = _entity.Frame?.N ?? next;
            SyncRuntime();
        }

        /// <summary>
        /// 重置状态。
        /// </summary>
        public void Reset()
        {
            wait = 1;
            waitCounter = _entity?.Frame?.N ?? 0;
            next = 999;
            requestPriority = 0;
            priorityRelease = 1;
            switchDirAfterTrans = false;
            hasFrameRequest = false;
            if (_entity != null) _entity.AttackingCounter = 0;
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
