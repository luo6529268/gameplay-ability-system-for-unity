using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    /// <summary>
    /// C++ release 风格的 frame_tick 适配器。
    /// wait_counter 保存上一 tick 结束时的 frame id，真正的等待计数使用 attacking。
    /// </summary>
    public class FrameTransistor
    {
        private int wait = 1;
        private int waitCounter;
        private int next = 999;
        private bool switchDirAfterTrans;

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
        public void Frame(int frameId)
        {
            SetNext(frameId);
            SetWait(0);
        }

        /// <summary>
        /// 设置当前帧等待值。
        /// </summary>
        public void SetWait(int value)
        {
            wait = value < 0 ? 0 : value;
            SyncRuntime();
        }

        public void SetWait(int value, int directWaitCounter)
        {
            wait = value < 0 ? 0 : value;
            waitCounter = directWaitCounter;
            SyncRuntime();
        }

        /// <summary>
        /// 调整当前帧等待值。
        /// </summary>
        public void IncWait(int inc)
        {
            wait += inc;
            if (wait < 0)
                wait = 0;

            SyncRuntime();
        }

        /// <summary>
        /// 设置下一帧请求。
        /// 负帧沿用现有 Unity 约定：记录翻面请求，实际换帧时再应用。
        /// </summary>
        public void SetNext(int value)
        {
            if (value < 0)
            {
                value = -value;
                switchDirAfterTrans = true;
            }

            next = value;
            SyncRuntime();
        }

        /// <summary>
        /// 直接写帧后的同步入口。
        /// directWaitCounter 对应 C++ Entity::wait_counter，也就是上一 tick 保存的 frame id，
        /// 不是等待倒计时。
        /// </summary>
        public void SyncDirectFrameData(int frameWait, int frameNext, int directWaitCounter = int.MinValue)
        {
            wait = frameWait < 0 ? 0 : frameWait;
            next = frameNext;
            if (directWaitCounter != int.MinValue)
                waitCounter = directWaitCounter;

            switchDirAfterTrans = false;
            SyncRuntime();
        }

        /// <summary>
        /// 仅展开 next=999 的正式版语义。
        /// 其余值包括 >=400 在内，仍然要先按 frame_tick 写入 frame，再由后续路径处理。
        /// </summary>
        public int NextFrameResolved()
        {
            int target = next;
            if (target == 999)
                target = 0;

            return target;
        }

        /// <summary>
        /// 按 C++ release frame_tick 顺序推进。
        /// frame 变化先清 attacking，然后 attacking++，超过 wait 才按 next 换帧。
        /// 只有未早退时，尾部才会把 wait_counter 同步为当前 frame id。
        /// </summary>
        public bool Trans()
        {
            NTSD.Tools.Log.LogState(_entity?.Name, "Trans",
                $"wait={wait} waitCounterFrame={waitCounter} attacking={_entity?.AttackingCounter ?? 0} next={next}");

            if (_entity == null)
            {
                SyncRuntime();
                return false;
            }

            int currentFrame = _entity.Frame?.N ?? 0;
            if (currentFrame != waitCounter)
            {
                _entity.OnFrameTickFrameChangedFromWaitCounter();
                _entity.AttackingCounter = 0;
            }

            _entity.AttackingCounter++;

            if (!_entity.OnFrameTickBeforeWaitAdvance(waitCounter))
            {
                waitCounter = _entity.Frame?.N ?? currentFrame;
                SyncRuntime();
                return false;
            }

            currentFrame = _entity.Frame?.N ?? currentFrame;

            if (_entity.AttackingCounter <= wait)
            {
                waitCounter = currentFrame;
                SyncRuntime();
                return true;
            }

            _entity.AttackingCounter = 0;

            if (next == 0)
            {
                waitCounter = currentFrame;
                SyncRuntime();
                return true;
            }

            int targetFrame = next;
            bool switchDir = switchDirAfterTrans;
            bool allowJumpInit = true;

            if (targetFrame == 999)
            {
                targetFrame = _entity.ResolveFrameTickNext999Target(out allowJumpInit);
            }
            else if (targetFrame < 0)
            {
                targetFrame = -targetFrame;
                switchDir = !switchDir;
            }

            if (targetFrame < 0)
            {
                waitCounter = 0;
                switchDirAfterTrans = false;
                SyncRuntime();
                return false;
            }

            int previousFrame = waitCounter;
            _entity.OnFrameTickTransit(targetFrame, switchDir);
            switchDirAfterTrans = false;

            // 对齐 C++ release：
            // frame 改写后，如果 frame<0、frame>=400 或目标帧不存在，会在这里直接 return，
            // 不再执行 jump_init / PP / turn，也不会写 wait_counter=frame。
            int frameAfterTransit = _entity.Frame?.N ?? targetFrame;
            if (frameAfterTransit < 0 || frameAfterTransit >= 400 || _entity.Frame?.D == null)
            {
                SyncRuntime();
                return false;
            }

            _entity.OnFrameTickAfterWaitAdvance(previousFrame, allowJumpInit);
            waitCounter = _entity.Frame?.N ?? targetFrame;
            SyncRuntime();
            return true;
        }

        /// <summary>
        /// 重置状态。
        /// </summary>
        public void Reset()
        {
            wait = 1;
            waitCounter = _entity?.Frame?.N ?? 0;
            next = 999;
            switchDirAfterTrans = false;
            if (_entity != null)
                _entity.AttackingCounter = 0;

            SyncRuntime();
        }

        private void SyncRuntime()
        {
            if (_entity == null)
                return;

            _entity.Runtime.WaitCounter = waitCounter;
            _entity.Runtime.NextFrame = next;
        }
    }
}
