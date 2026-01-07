using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 帧块（<frame> ... <frame_end>）
    /// 表示动画的一帧
    /// </summary>
    [Serializable]
    public class Lf2FrameBlock : ILf2DatPropertyContainer
    {
        public int FrameIndex = -1;     // 帧索引（如 <frame> 0）
        public string FrameName;        // 帧名称（如 <frame> 0 standing）
        public List<Lf2DatSubBlock> SubBlocks = new List<Lf2DatSubBlock>();

        private List<Lf2DatProperty> _properties = new List<Lf2DatProperty>();
        public List<Lf2DatProperty> Properties => _properties;

        public void AddProperty(Lf2DatProperty prop)
        {
            _properties.Add(prop);
        }

        public override string ToString()
        {
            return $"Frame {FrameIndex} {FrameName} ({Properties.Count} properties, {SubBlocks.Count} sub-blocks)";
        }
    }
}
