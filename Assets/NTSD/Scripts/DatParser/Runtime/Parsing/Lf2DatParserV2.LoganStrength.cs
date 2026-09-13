using System;
using System.Collections.Generic;
using System.Text;

namespace NTSD.DatParser
{
    public partial class Lf2DatParserV2
    {
        private static readonly char[] LoganWhitespace = { ' ', '\t', '\r', '\n', '\v', '\f' };
        private static readonly string[] LoganStrengthContextExits =
        {
            "<bmp_begin>", "<bmp_end>", "<weapon_piece>", "<weapon_piece_end>",
            "<menu_face>", "<menu_face_end>", "<armor>", "<armor_end>"
        };

        private static List<Lf2WeaponStrengthRow> ParseLoganContentRegions(
            string text, out string remainder, out List<Lf2FrameBlock> frames, out LoganHeaderReader headerReader, out LoganDefinitionBlockReader definitionReader)
        {
            var rows = new List<Lf2WeaponStrengthRow>();
            var indices = new HashSet<int>();
            var remainingText = new StringBuilder(text?.Length ?? 0);
            bool inStrength = false;
            bool inFrame = false;
            var frameReader = new LoganFrameReader();
            headerReader = new LoganHeaderReader();
            definitionReader = new LoganDefinitionBlockReader();
            Lf2WeaponStrengthRow current = null;
            string[] lines = (text ?? string.Empty).Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim(LoganWhitespace);
                bool consumed = false;
                if (line.StartsWith("<stats>", StringComparison.Ordinal))
                {
                    headerReader.ReadStats(line);
                    consumed = true;
                }
                else if (definitionReader.ReadMarker(line, i + 1))
                {
                    headerReader.ExitContext();
                    inStrength = false;
                    inFrame = false;
                    current = null;
                    consumed = true;
                }
                else if (headerReader.ReadMarker(line))
                {
                    definitionReader.ExitContext();
                    inStrength = false;
                    inFrame = false;
                    current = null;
                    consumed = true;
                }
                else if (line.StartsWith("<weapon_strength_list>", StringComparison.Ordinal))
                {
                    headerReader.ExitContext();
                    definitionReader.ExitContext();
                    inStrength = true;
                    inFrame = false;
                    current = null;
                    consumed = true;
                }
                else if (line.StartsWith("<weapon_strength_list_end>", StringComparison.Ordinal))
                {
                    headerReader.ExitContext();
                    definitionReader.ExitContext();
                    inStrength = false;
                    inFrame = false;
                    current = null;
                    consumed = true;
                }
                else if (line.StartsWith("<frame>", StringComparison.Ordinal))
                {
                    headerReader.ExitContext();
                    definitionReader.ExitContext();
                    frameReader.Open(line, i + 1);
                    inFrame = true;
                    inStrength = false;
                    consumed = true;
                }
                else if (line.StartsWith("<frame_end>", StringComparison.Ordinal))
                {
                    headerReader.ExitContext();
                    definitionReader.ExitContext();
                    frameReader.Close(i + 1);
                    inFrame = false;
                    inStrength = false;
                    consumed = true;
                }
                else if (ExitsLoganStrengthContext(line))
                {
                    headerReader.ExitContext();
                    definitionReader.ExitContext();
                    inStrength = false;
                    inFrame = false;
                    current = null;
                }
                else if (definitionReader.Active)
                {
                    definitionReader.ReadLine(line, i + 1);
                    consumed = true;
                }
                else if (headerReader.Active)
                {
                    headerReader.ReadLine(line);
                    consumed = true;
                }
                else if (inFrame && !line.StartsWith("<stats>", StringComparison.Ordinal))
                {
                    consumed = true;
                    if (line.Length != 0)
                        inFrame = frameReader.ReadLine(line);
                }
                else if (inStrength && !line.StartsWith("<stats>", StringComparison.Ordinal))
                {
                    consumed = true;
                    if (line.StartsWith("entry:", StringComparison.Ordinal))
                    {
                        string header = line.Substring(6).Trim(LoganWhitespace);
                        int separator = header.IndexOfAny(new[] { ' ', '\t' });
                        string indexText = (separator < 0 ? header : header.Substring(0, separator)).Trim(LoganWhitespace);
                        if (!LoganNumericDecoder.TryParseInt32(indexText, out int index) || index < 1 || index > 9)
                            throw new FormatException($"Invalid weapon strength entry at line {i + 1}: {indexText}");
                        if (!indices.Add(index))
                            throw new FormatException($"Duplicate weapon strength entry {index} at line {i + 1}.");
                        string caption = separator < 0 ? string.Empty : header.Substring(separator).Trim(LoganWhitespace);
                        current = new Lf2WeaponStrengthRow(index, caption, i + 1);
                        rows.Add(current);
                    }
                    else if (line.Length != 0)
                    {
                        if (current == null)
                            throw new FormatException($"Weapon strength fields precede their entry header at line {i + 1}.");
                        Lf2DatTokenizer.ScanLoganScalarFields(line, current.Properties);
                    }
                }

                // Alignment contract: NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001
                // Consumed captions/fields cannot leak into the generic metadata namespace.
                if (!consumed)
                    remainingText.Append(lines[i]);
                if (i + 1 < lines.Length)
                    remainingText.Append('\n');
            }
            remainder = remainingText.ToString();
            frames = frameReader.Finish();
            headerReader.Finish();
            definitionReader.Finish();
            return rows;
        }

        private static bool ExitsLoganStrengthContext(string line)
        {
            foreach (string marker in LoganStrengthContextExits)
            {
                if (line.StartsWith(marker, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }
    }
}
