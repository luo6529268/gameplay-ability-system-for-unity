using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    public partial class Lf2DatParserV2
    {
        private sealed class LoganHeaderReader
        {
            private static readonly HashSet<string> BareFields = new HashSet<string>(BmpMovementPropertyNames, StringComparer.Ordinal);
            private static readonly HashSet<string> SequenceNames = new HashSet<string>(BmpFrameSequenceNames, StringComparer.Ordinal);
            public Lf2BmpSection Bmp { get; private set; }
            public Lf2DatBlock Stats { get; } = new Lf2DatBlock { Name = "stats" };
            public bool Active { get; private set; }
            private bool sawClose;
            private Lf2BmpFrameSequence sequence;
            private int expectedCount;

            public void ExitContext() { Active = false; }

            public bool ReadMarker(string line)
            {
                if (line.StartsWith("<bmp_begin>", StringComparison.Ordinal))
                {
                    Bmp ??= new Lf2BmpSection();
                    Active = true;
                    return true;
                }
                if (line.StartsWith("<bmp_end>", StringComparison.Ordinal))
                {
                    if (sequence != null) throw new FormatException("BMP sequence was not closed before bmp_end.");
                    sawClose = true;
                    Active = false;
                    return true;
                }
                return false;
            }

            public void ReadStats(string line)
            {
                string inner = line.Substring(7);
                int end = inner.IndexOf("<stats_end>", StringComparison.Ordinal);
                if (end >= 0) inner = inner.Substring(0, end);
                Lf2DatTokenizer.ScanLoganScalarFields(inner, Stats.Properties, "stats");
            }

            public void ReadLine(string line)
            {
                if (line.Length == 0) return;
                if (sequence != null)
                {
                    if (line == sequence.Name + "_end:")
                    {
                        if (sequence.Actions.Count != expectedCount) throw new FormatException("BMP sequence count mismatch.");
                        sequence = null;
                    }
                    else if (!ReadIntegerRow(line, sequence.Actions))
                        throw new FormatException("BMP sequence contains a non-integer action row.");
                    return;
                }
                var fields = new List<Lf2DatProperty>();
                Lf2DatTokenizer.ScanLoganScalarFields(line, fields, "bmp");
                if (fields.Count == 0)
                {
                    int split = line.IndexOfAny(new[] { ' ', '\t' });
                    string key = split < 0 ? line : line.Substring(0, split);
                    if (BareFields.Contains(key))
                        fields.Add(new Lf2DatProperty(key, split < 0 ? string.Empty : line.Substring(split).Trim(LoganWhitespace)));
                }
                foreach (var field in fields)
                {
                    if (TrySpriteRange(field.Key, out int first, out int last))
                        Bmp.Files.Add(new Lf2SpriteFileDef
                        {
                            StartIndex = first, EndIndex = last, Path = field.Value,
                            Width = LastInteger(fields, "w"), Height = LastInteger(fields, "h"),
                            Row = LastInteger(fields, "row"), Col = LastInteger(fields, "col")
                        });
                    Bmp.Properties.Add(field);
                    if (field.Key == "name") Bmp.Name = field.Value;
                    if (field.Key == "head") Bmp.Head = field.Value;
                    if (field.Key == "small") Bmp.Small = field.Value;
                    if (!SequenceNames.Contains(field.Key)) continue;
                    if (!LoganNumericDecoder.TryParseInt32(field.Value.Trim(LoganWhitespace), out expectedCount) || expectedCount <= 0)
                        throw new FormatException("Invalid BMP sequence declared count.");
                    sequence = new Lf2BmpFrameSequence { Name = field.Key };
                    Bmp.FrameSequences.Add(sequence);
                    int colon = line.IndexOf(':');
                    var header = new List<int>();
                    if (colon >= 0 && ReadIntegerRow(line.Substring(colon + 1), header) && header.Count > 1)
                        sequence.Actions.AddRange(header.GetRange(1, header.Count - 1));
                }
            }

            public void Finish()
            {
                if (Bmp != null && !sawClose) throw new FormatException("Missing bmp_end.");
                if (sequence != null) throw new FormatException("Unclosed BMP sequence.");
            }

            private static int LastInteger(List<Lf2DatProperty> fields, string key)
            {
                for (int i = fields.Count - 1; i >= 0; i--)
                    if (fields[i].Key == key) return LoganNumericDecoder.ParseInt32OrZero(fields[i].Value);
                return 0;
            }

            private static bool TrySpriteRange(string key, out int first, out int last)
            {
                first = last = 0;
                if (!key.StartsWith("file(", StringComparison.Ordinal) || !key.EndsWith(")", StringComparison.Ordinal)) return false;
                string inner = key.Substring(5, key.Length - 6);
                int dash = inner.IndexOf('-');
                if (dash < 0)
                {
                    bool valid = LoganNumericDecoder.TryParseInt32(inner.Trim(LoganWhitespace), out first);
                    last = first;
                    return valid;
                }
                return LoganNumericDecoder.TryParseInt32(inner.Substring(0, dash).Trim(LoganWhitespace), out first)
                    && LoganNumericDecoder.TryParseInt32(inner.Substring(dash + 1).Trim(LoganWhitespace), out last);
            }

            // Alignment contract: NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001
            // Match formatted int extraction, including failed final extraction setting eofbit.
            private static bool ReadIntegerRow(string text, List<int> output)
            {
                bool parsed = false;
                int cursor = 0;
                while (true)
                {
                    while (cursor < text.Length && Array.IndexOf(LoganWhitespace, text[cursor]) >= 0) cursor++;
                    if (cursor == text.Length) return parsed;
                    bool negative = text[cursor] == '-';
                    if (text[cursor] == '+' || negative) cursor++;
                    int start = cursor;
                    long value = 0;
                    bool overflow = false;
                    while (cursor < text.Length && text[cursor] >= '0' && text[cursor] <= '9')
                    {
                        if (!overflow)
                        {
                            value = value * 10 + text[cursor] - '0';
                            overflow = value > (negative ? 2147483648L : int.MaxValue);
                        }
                        cursor++;
                    }
                    if (cursor == start || overflow) return parsed && cursor == text.Length;
                    output.Add((int)(negative ? -value : value));
                    parsed = true;
                }
            }
        }
    }
}
