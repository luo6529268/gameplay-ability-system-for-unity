using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 子块（如 body:, itr:, bdy:, opoint: 等）
    /// </summary>
    [Serializable]
    public class Lf2DatSubBlock : ILf2DatPropertyContainer
    {
        public string Name;

        private List<Lf2DatProperty> _properties = new List<Lf2DatProperty>();
        public List<Lf2DatProperty> Properties => _properties;

        public void AddProperty(Lf2DatProperty prop)
        {
            _properties.Add(prop);
        }

        public override string ToString()
        {
            return $"{Name}: ({Properties.Count} properties)";
        }
    }
}
