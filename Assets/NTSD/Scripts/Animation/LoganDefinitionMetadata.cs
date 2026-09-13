using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NTSD.DatParser;

namespace NTSD.Animation
{
    public sealed class LoganDefinitionFieldSet
    {
        private readonly struct Value
        {
            public readonly string Text;
            public readonly int? Integer;
            public readonly double? Number;

            public Value(string text)
            {
                Text = text;
                Integer = LoganNumericDecoder.TryParseInt32(text, out int integer) ? integer : (int?)null;
                Number = LoganNumericDecoder.TryParseFiniteFloat64(text, out double number) ? number : (double?)null;
            }
        }

        private readonly ReadOnlyCollection<KeyValuePair<string, string>> rows;
        private readonly Dictionary<string, Value> values;
        public IReadOnlyList<KeyValuePair<string, string>> Rows => rows;
        public int Count => rows.Count;

        public LoganDefinitionFieldSet(IEnumerable<KeyValuePair<string, string>> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var copy = new List<KeyValuePair<string, string>>();
            values = new Dictionary<string, Value>(StringComparer.Ordinal);
            foreach (var field in source)
            {
                if (field.Key == null || field.Value == null)
                    throw new ArgumentException("Native definition fields require a key and text value.", nameof(source));
                copy.Add(field);
                values[field.Key] = new Value(field.Value);
            }
            rows = copy.AsReadOnly();
        }

        public bool Contains(string key) => values.ContainsKey(key);

        public string TextOrNull(string key)
        {
            return values.TryGetValue(key, out Value value) ? value.Text : null;
        }

        public bool TryGetInt32(string key, out int result)
        {
            if (values.TryGetValue(key, out Value value) && value.Integer.HasValue)
            {
                result = value.Integer.Value;
                return true;
            }
            result = 0;
            return false;
        }

        public bool TryGetFloat64(string key, out double result)
        {
            if (values.TryGetValue(key, out Value value) && value.Number.HasValue)
            {
                result = value.Number.Value;
                return true;
            }
            result = 0;
            return false;
        }

        public int Int32OrDefault(string key, int fallback)
        {
            return TryGetInt32(key, out int value) ? value : fallback;
        }

        public double Float64OrDefault(string key, double fallback)
        {
            return TryGetFloat64(key, out double value) ? value : fallback;
        }
    }

    public sealed class LoganDefinitionMetadata
    {
        public LoganDefinitionFieldSet Bmp { get; }
        public LoganDefinitionFieldSet Stats { get; }
        public bool HasStatsRecord => Stats.Count != 0;
        public IReadOnlyList<LoganDefinitionFieldSet> Armors { get; }
        public LoganWeaponPieceDefinition WeaponPiece { get; }

        internal static LoganDefinitionFieldSet CopyFields(IEnumerable<Lf2DatProperty> fields)
        {
            var rows = new List<KeyValuePair<string, string>>();
            if (fields != null)
                foreach (var field in fields) rows.Add(new KeyValuePair<string, string>(field.Key, field.Value));
            return new LoganDefinitionFieldSet(rows);
        }

        public LoganDefinitionMetadata(LoganDefinitionFieldSet bmp, LoganDefinitionFieldSet stats)
            : this(bmp, stats, null, null)
        {
        }

        public LoganDefinitionMetadata(LoganDefinitionFieldSet bmp, LoganDefinitionFieldSet stats,
            IEnumerable<LoganDefinitionFieldSet> armors, LoganWeaponPieceDefinition weaponPiece)
        {
            Bmp = bmp ?? throw new ArgumentNullException(nameof(bmp));
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            var copy = new List<LoganDefinitionFieldSet>();
            if (armors != null)
                foreach (var armor in armors)
                    copy.Add(armor ?? throw new ArgumentException("Null armor field set.", nameof(armors)));
            Armors = new ReadOnlyCollection<LoganDefinitionFieldSet>(copy);
            WeaponPiece = weaponPiece;
        }
    }
}
