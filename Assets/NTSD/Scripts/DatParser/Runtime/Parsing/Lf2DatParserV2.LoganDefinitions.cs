using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    public partial class Lf2DatParserV2
    {
        private sealed class LoganDefinitionBlockReader
        {
            private enum Context { None, Armor, Piece }
            private Context context;
            private Lf2LoganArmorBlock armor;
            private Lf2WeaponPieceGroup group;
            private Lf2WeaponPieceVariant variant;
            public List<Lf2LoganArmorBlock> Armors { get; } = new List<Lf2LoganArmorBlock>();
            public Lf2WeaponPieceBlock WeaponPiece { get; private set; }
            public bool Active => context != Context.None;

            public void ExitContext() { context = Context.None; }

            public bool ReadMarker(string line, int number)
            {
                if (line.StartsWith("<armor>", StringComparison.Ordinal))
                {
                    armor = new Lf2LoganArmorBlock { Name = "armor", OpeningLine = number };
                    Armors.Add(armor);
                    context = Context.Armor;
                    return true;
                }
                if (line.StartsWith("<armor_end>", StringComparison.Ordinal))
                {
                    if (armor == null) throw new FormatException("Armor end without an open armor block.");
                    armor.ClosingLine = number;
                    armor = null;
                    context = Context.None;
                    return true;
                }
                if (line.StartsWith("<weapon_piece>", StringComparison.Ordinal))
                {
                    if (WeaponPiece != null) throw new FormatException("Duplicate weapon piece block.");
                    WeaponPiece = new Lf2WeaponPieceBlock { Name = "weapon_piece", OpeningLine = number };
                    group = null;
                    variant = null;
                    context = Context.Piece;
                    return true;
                }
                if (line.StartsWith("<weapon_piece_end>", StringComparison.Ordinal))
                {
                    if (WeaponPiece == null || context != Context.Piece)
                        throw new FormatException("Weapon piece end without an active block.");
                    if (variant != null) throw new FormatException("Weapon piece variant was not closed by piece_end.");
                    WeaponPiece.ClosingLine = number;
                    group = null;
                    variant = null;
                    context = Context.None;
                    return true;
                }
                return false;
            }

            public void ReadLine(string line, int number)
            {
                if (line.Length == 0) return;
                if (context == Context.Armor)
                {
                    Lf2DatTokenizer.ScanLoganScalarFields(line, armor.Properties, "armor");
                    return;
                }
                if (line.StartsWith("piece_end:", StringComparison.Ordinal))
                {
                    if (variant == null) throw new FormatException("piece_end without a weapon piece variant.");
                    variant.ClosingLine = number;
                    group = null;
                    variant = null;
                    return;
                }
                var fields = new List<Lf2DatProperty>();
                Lf2DatTokenizer.ScanLoganScalarFields(line, fields, "weapon_piece");
                Lf2DatProperty pieceField = fields.FindLast(p => p.Key == "piece");
                if (pieceField != null && LoganNumericDecoder.TryParseInt32(pieceField.Value, out int piece))
                {
                    group = WeaponPiece.Groups.Find(g => g.Piece == piece);
                    if (group == null)
                    {
                        if (WeaponPiece.Groups.Count >= 5) throw new FormatException("Weapon piece group count exceeds five.");
                        group = new Lf2WeaponPieceGroup { Piece = piece };
                        WeaponPiece.Groups.Add(group);
                    }
                    if (group.Variants.Count >= 5) throw new FormatException("Weapon piece variant count exceeds five.");
                    variant = new Lf2WeaponPieceVariant { Piece = piece, OpeningLine = number };
                    group.Variants.Add(variant);
                }
                // Alignment contract: NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001
                // Amount is group-owned even when authored on a variant's physical line.
                foreach (var field in fields)
                {
                    if (field.Key == "amount")
                    {
                        if (group == null) throw new FormatException("Weapon piece amount precedes piece.");
                        group.Properties.Add(field);
                    }
                    else if (variant != null) variant.Properties.Add(field);
                    else WeaponPiece.Properties.Add(field);
                }
            }

            public void Finish()
            {
                if (armor != null) throw new FormatException("Unclosed armor block at end of input.");
                if (WeaponPiece != null && WeaponPiece.ClosingLine == 0)
                    throw new FormatException("Unclosed weapon piece block at end of input.");
            }
        }
    }
}
