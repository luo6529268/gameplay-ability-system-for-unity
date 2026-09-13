using System.Collections.Generic;

namespace NTSD.DatParser
{
    public sealed class Lf2LoganArmorBlock : Lf2DatBlock
    {
        public int OpeningLine;
        public int ClosingLine;
    }

    public sealed class Lf2WeaponPieceVariant : Lf2DatBlock
    {
        public int Piece;
        public int OpeningLine;
        public int ClosingLine;
    }

    public sealed class Lf2WeaponPieceGroup : Lf2DatBlock
    {
        public int Piece;
        public List<Lf2WeaponPieceVariant> Variants = new List<Lf2WeaponPieceVariant>();
    }

    public sealed class Lf2WeaponPieceBlock : Lf2DatBlock
    {
        public int OpeningLine;
        public int ClosingLine;
        public List<Lf2WeaponPieceGroup> Groups = new List<Lf2WeaponPieceGroup>();
    }
}
