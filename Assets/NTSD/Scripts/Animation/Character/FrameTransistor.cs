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
        /// 负帧保持 DAT 原始值，由统一 frame_tick 在真正换帧前翻面。
        /// </summary>
        public void SetNext(int value)
        {
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
        /// 委托实体唯一的 current-DAT frame_tick 实现，避免适配器维护第二套顺序。
        /// </summary>
        public bool Trans()
        {
            if (_entity == null)
            {
                SyncRuntime();
                return false;
            }
            return _entity.RunCommonFrameTickFromTransistor();
        }

        public void SyncWaitCounterFrame(int frameId)
        {
            waitCounter = frameId;
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
