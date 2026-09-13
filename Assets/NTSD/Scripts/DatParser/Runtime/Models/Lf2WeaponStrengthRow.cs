using System.Collections.Generic;

namespace NTSD.DatParser
{
    public sealed class Lf2WeaponStrengthRow
    {
        public int Index { get; }
        public string Caption { get; }
        public int OpeningLine { get; }
        public List<Lf2DatProperty> Properties { get; } = new List<Lf2DatProperty>();

        internal Lf2WeaponStrengthRow(int index, string caption, int openingLine)
        {
            Index = index;
            Caption = caption;
            OpeningLine = openingLine;
        }
    }
}
