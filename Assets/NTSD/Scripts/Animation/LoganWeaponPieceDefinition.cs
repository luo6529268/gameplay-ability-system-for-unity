using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NTSD.DatParser;

namespace NTSD.Animation
{
    public sealed class LoganWeaponPieceVariant
    {
        public int Piece { get; }
        public LoganDefinitionFieldSet Fields { get; }

        internal LoganWeaponPieceVariant(Lf2WeaponPieceVariant source)
        {
            Piece = source.Piece;
            Fields = LoganDefinitionMetadata.CopyFields(source.Properties);
        }
    }

    public sealed class LoganWeaponPieceGroup
    {
        public int Piece { get; }
        public LoganDefinitionFieldSet Fields { get; }
        public IReadOnlyList<LoganWeaponPieceVariant> Variants { get; }

        internal LoganWeaponPieceGroup(Lf2WeaponPieceGroup source)
        {
            Piece = source.Piece;
            Fields = LoganDefinitionMetadata.CopyFields(source.Properties);
            var variants = new List<LoganWeaponPieceVariant>();
            foreach (var variant in source.Variants) variants.Add(new LoganWeaponPieceVariant(variant));
            Variants = new ReadOnlyCollection<LoganWeaponPieceVariant>(variants);
        }
    }

    public sealed class LoganWeaponPieceDefinition
    {
        public LoganDefinitionFieldSet Fields { get; }
        public IReadOnlyList<LoganWeaponPieceGroup> Groups { get; }

        internal LoganWeaponPieceDefinition(Lf2WeaponPieceBlock source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Fields = LoganDefinitionMetadata.CopyFields(source.Properties);
            var groups = new List<LoganWeaponPieceGroup>();
            foreach (var group in source.Groups) groups.Add(new LoganWeaponPieceGroup(group));
            Groups = new ReadOnlyCollection<LoganWeaponPieceGroup>(groups);
        }
    }
}
