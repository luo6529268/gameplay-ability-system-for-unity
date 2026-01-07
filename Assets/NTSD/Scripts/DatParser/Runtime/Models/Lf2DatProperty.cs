using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 文件中的属性（键值对）
    /// </summary>
    [Serializable]
    public class Lf2DatProperty
    {
        public string Key;
        public string Value;

        public Lf2DatProperty() { }

        public Lf2DatProperty(string key, string value)
        {
            Key = key;
            Value = value;
        }

        public override string ToString()
        {
            return Key + ": " + Value;
        }
    }

    /// <summary>
    /// 属性容器接口
    /// </summary>
    public interface ILf2DatPropertyContainer
    {
        List<Lf2DatProperty> Properties { get; }
        void AddProperty(Lf2DatProperty prop);
    }
}
