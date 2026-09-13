using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    public partial class Lf2DatParserV2
    {
        private sealed class LoganFrameReader
        {
            private static readonly string[] SubBlockNames = { "itr", "bdy", "opoint", "wpoint", "bpoint", "cpoint", "ppoint" };
            private readonly List<Lf2FrameBlock> frames = new List<Lf2FrameBlock>();
            private readonly HashSet<int> frameIds = new HashSet<int>();
            private Lf2FrameBlock current;
            private Lf2DatSubBlock pending;

            public void Open(string line, int lineNumber)
            {
                FlushPending();
                string header = line.Substring(7).Trim(LoganWhitespace);
                int separator = header.IndexOfAny(new[] { ' ', '\t' });
                string idText = (separator < 0 ? header : header.Substring(0, separator)).Trim(LoganWhitespace);
                if (!LoganNumericDecoder.TryParseInt32(idText, out int id) || id < 0 || id > 999)
                    throw new FormatException($"Invalid native frame header at line {lineNumber}: {idText}");
                if (!frameIds.Add(id))
                    throw new FormatException($"Duplicate native frame {id} at line {lineNumber}.");
                current = new Lf2FrameBlock
                {
                    FrameIndex = id,
                    FrameName = separator < 0 ? string.Empty : header.Substring(separator).Trim(LoganWhitespace)
                };
                frames.Add(current);
            }

            public void Close(int lineNumber)
            {
                FlushPending();
                if (current == null)
                    throw new FormatException($"Native frame_end without an open frame at line {lineNumber}.");
                current = null;
            }

            public bool ReadLine(string line)
            {
                if (pending != null)
                {
                    int close = line.IndexOf(pending.Name + "_end", StringComparison.Ordinal);
                    Lf2DatTokenizer.ScanLoganScalarFields(close < 0 ? line : line.Substring(0, close), pending.Properties, pending.Name);
                    if (close >= 0)
                        FlushPending();
                    return true;
                }

                bool closesFrame = line.IndexOf("<frame_end>", StringComparison.Ordinal) >= 0;
                bool parsedSubBlock = false;
                foreach (string kind in SubBlockNames)
                {
                    string prefix = kind + ":";
                    if (!line.StartsWith(prefix, StringComparison.Ordinal))
                        continue;
                    string inner = line.Substring(prefix.Length);
                    int close = inner.IndexOf(kind + "_end", StringComparison.Ordinal);
                    var block = new Lf2DatSubBlock { Name = kind };
                    Lf2DatTokenizer.ScanLoganScalarFields(close < 0 ? inner : inner.Substring(0, close), block.Properties, kind);
                    if (close >= 0)
                        current.SubBlocks.Add(block);
                    else
                        pending = block;
                    parsedSubBlock = true;
                    break;
                }
                if (!parsedSubBlock)
                    Lf2DatTokenizer.ScanLoganScalarFields(line, current.Properties, "frame");

                // Alignment contract: NTSD28-Q05-NATIVE-FRAME-AST-ADMISSION-001
                // Native inline close clears the frame without flushing a just-opened subblock.
                if (closesFrame)
                    current = null;
                return !closesFrame;
            }

            public List<Lf2FrameBlock> Finish()
            {
                FlushPending();
                return frames;
            }

            private void FlushPending()
            {
                if (current != null && pending != null)
                {
                    current.SubBlocks.Add(pending);
                    pending = null;
                }
            }
        }
    }
}
