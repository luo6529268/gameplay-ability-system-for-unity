using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    /// <summary>
    /// LF2 Dat 块（如 <object>, <weapon_strength_list> 等）
    /// </summary>
    [Serializable]
    public class Lf2DatBlock : ILf2DatPropertyContainer
    {
        public string Name;
        public List<Lf2DatSubBlock> SubBlocks = new List<Lf2DatSubBlock>();

        private List<Lf2DatProperty> _properties = new List<Lf2DatProperty>();
        public List<Lf2DatProperty> Properties => _properties;

        public void AddProperty(Lf2DatProperty prop)
        {
            _properties.Add(prop);
        }

        public override string ToString()
        {
            return $"<{Name}> ({Properties.Count} properties, {SubBlocks.Count} sub-blocks)";
        }
    }
}
