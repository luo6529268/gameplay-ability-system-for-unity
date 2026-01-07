using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 帧数据包装器（用于JSON序列化）
    /// </summary>
    [System.Serializable]
    public class FrameDataWrapper
    {
        public int ID;
        public List<LF2FrameData> frames;

        public LF2FrameData GetFrameData(int frameId)
        {
            if (frames[frameId].frameId == frameId)
                return frames[frameId];

            foreach (var frame in frames)
            {
                if (frame.frameId == frameId)
                    return frame;
            }

            return null;
        }
    }

}