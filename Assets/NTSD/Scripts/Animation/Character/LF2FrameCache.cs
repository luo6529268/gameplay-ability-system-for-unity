namespace NTSD.Animation
{
    internal interface ILF2FrameCacheObserver
    {
        void OnFrameCacheIdentityChanged();
    }

    /// <summary>
    /// LF2 帧数据缓存（数据层，不继承 Mono）。
    /// 运行时仅维护无分配的 frameId 数组索引；描述性的 frameName 分组留在
    /// Loading/Editor 数据层，不再为每次 opoint 生成重复创建 Dictionary/List。
    /// </summary>
    public sealed class LF2FrameCache
    {
        public const int MaxFrameIdExclusive = 600;

        private static readonly LF2FrameData EmptyFrame = new LF2FrameData();

        public LF2CharacterDataWrapper Wrapper { get; private set; }

        private readonly LF2FrameData[] _frames = new LF2FrameData[MaxFrameIdExclusive];
        private readonly ILF2FrameCacheObserver observer;

        public LF2FrameCache()
        {
        }

        internal LF2FrameCache(ILF2FrameCacheObserver observer)
        {
            this.observer = observer;
        }

        public void Clear()
        {
            ClearCore();
            observer?.OnFrameCacheIdentityChanged();
        }

        public void Load(LF2CharacterDataWrapper wrapper)
        {
            ClearCore();
            Wrapper = wrapper;

            var frames = wrapper?.characterData?.frames;
            if (frames != null)
            {
                foreach (var frameData in frames)
                {
                    if (frameData == null) continue;

                    if ((uint)frameData.frameId < MaxFrameIdExclusive)
                    {
                        _frames[frameData.frameId] = frameData;
                    }
                }
            }

            observer?.OnFrameCacheIdentityChanged();
        }

        private void ClearCore()
        {
            Wrapper = null;
            for (int i = 0; i < _frames.Length; i++)
                _frames[i] = null;
        }

        public LF2FrameData GetFrameDataById(int frameId)
        {
            if ((uint)frameId >= (uint)_frames.Length) return null;
            return _frames[frameId] ?? EmptyFrame;
        }

        public bool HasFrame(int frameId)
        {
            return (uint)frameId < (uint)_frames.Length && _frames[frameId] != null;
        }

        public int GetFirstFrameByState(int targetState)
        {
            for (int i = 0; i < _frames.Length; i++)
            {
                if (_frames[i] != null && _frames[i].state == targetState)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}

