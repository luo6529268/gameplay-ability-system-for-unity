using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
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

        private List<Lf2DatProperty> _properties = new List<Lf2DatProperty>();
        public List<Lf2DatProperty> Properties => _properties;

        public void AddProperty(Lf2DatProperty prop)
        {
            _properties.Add(prop);
        }

        public override string ToString()
        {
            return $"BmpSection: {Name} ({Files.Count} sprite files)";
        }
    }
}
