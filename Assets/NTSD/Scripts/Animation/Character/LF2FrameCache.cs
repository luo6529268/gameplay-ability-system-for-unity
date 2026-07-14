using System;
using System.Collections.Generic;

namespace NTSD.Animation
{
    /// <summary>
    /// LF2 帧数据缓存（数据层，不继承 Mono）。
    /// 将 CharacterID/Wrapper 解析后的 frameId 索引与 frameName 分组从 Animator 中剥离，避免 Animator 承担数据管理职责。
    /// </summary>
    public sealed class LF2FrameCache
    {
        public const int MaxFrameIdExclusive = 400;

        public LF2CharacterDataWrapper Wrapper { get; private set; }

        private LF2FrameData[] _frames = new LF2FrameData[400];
        private readonly Dictionary<string, List<LF2FrameData>> _framesByName = new Dictionary<string, List<LF2FrameData>>();

        public void Clear()
        {
            Wrapper = null;
            for (int i = 0; i < _frames.Length; i++)
                _frames[i] = null;

            _framesByName.Clear();
        }

        public void Load(LF2CharacterDataWrapper wrapper)
        {
            Clear();
            Wrapper = wrapper;

            var frames = wrapper?.characterData?.frames;
            if (frames == null) return;
 
            foreach (var frameData in frames)
            {
                if (frameData == null) continue;

                if (frameData.frameId >= 0)
                {
                    _frames[frameData.frameId] = frameData;
                }

                if (_framesByName.TryGetValue(frameData.frameName, out List<LF2FrameData> list))
                {
                    list.Add(frameData);
                }
                else
                {
                    list = new List<LF2FrameData>(5) { frameData };
                    _framesByName.Add(frameData.frameName, list);
                }
            }
        }

        public LF2FrameData GetFrameDataById(int frameId)
        {
            if ((uint)frameId >= (uint)_frames.Length) return null;
            return _frames[frameId];
        }

        public bool HasFrame(int frameId)
        {
            return GetFrameDataById(frameId) != null;
        }

        public bool TryGetFramesByName(string frameName, out List<LF2FrameData> frames)
        {
            return _framesByName.TryGetValue(frameName, out frames);
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

