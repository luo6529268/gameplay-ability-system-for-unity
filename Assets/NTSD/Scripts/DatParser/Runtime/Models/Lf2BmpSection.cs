using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    [Serializable]
    public class Lf2BmpFrameSequence
    {
        public string Name;
        public List<int> Actions = new List<int>();
    }

    /// <summary>
    /// LF2 Bmp 节（bmp_begin ... bmp_end）
    /// 包含精灵图片文件的定义
    /// </summary>
    [Serializable]
    public class Lf2BmpSection : ILf2DatPropertyContainer
    {
        public string Name;
        public string Head;
        public string Small;
        public List<Lf2SpriteFileDef> Files = new List<Lf2SpriteFileDef>();
        public List<Lf2BmpFrameSequence> FrameSequences =
            new List<Lf2BmpFrameSequence>();

        private List<Lf2DatProperty> _properties = new List<Lf2DatProperty>();
        public List<Lf2DatProperty> Properties => _properties;

        public void AddProperty(Lf2DatProperty prop)
        {
            _properties.Add(prop);
        }

        public void AddFrameSequence(Lf2BmpFrameSequence sequence)
        {
            if (sequence != null)
                FrameSequences.Add(sequence);
        }

        public override string ToString()
        {
            return $"BmpSection: {Name} ({Files.Count} sprite files)";
        }
    }
}
