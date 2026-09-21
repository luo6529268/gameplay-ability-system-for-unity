using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    public static class LoganFusionCatalogParser
    {
        public static LoganFusionCatalog ParseText(string text)
        {
            text ??= string.Empty;
            var records = new List<LoganFusionRecord>();
            var diagnostics = new List<LoganFusionDiagnostic>();
            int lineNumber = 0;
            bool inOuter = false;
            bool inRecord = false;
            bool sawOuterBegin = false;
            var current = new int[16];
            int sourceLine = 0;
            void Error(string message) => diagnostics.Add(new LoganFusionDiagnostic(LoganFusionDiagnosticSeverity.Error, lineNumber, message));
            void Warning(string message) => diagnostics.Add(new LoganFusionDiagnostic(LoganFusionDiagnosticSeverity.Warning, lineNumber, message));

            // Split only on LF, matching std::getline; lone CR is trimmed, not a line delimiter.
            int offset = 0;
            while (offset < text.Length)
            {
                int end = text.IndexOf('\n', offset);
                if (end < 0) end = text.Length;
                string line = text.Substring(offset, end - offset);
                offset = end + 1;
                lineNumber++;
                int semicolon = line.IndexOf(';');
                int hash = line.IndexOf('#');
                int cut = semicolon < 0 ? hash : hash < 0 ? semicolon : Math.Min(semicolon, hash);
                if (cut >= 0) line = line.Substring(0, cut);
                line = Trim(line);
                if (line.Length == 0) continue;
                if (line == "<fusion_begin>")
                {
                    if (inOuter) Error("nested <fusion_begin> is invalid");
                    inOuter = true;
                    sawOuterBegin = true;
                    continue;
                }
                if (line == "<fusion_end>")
                {
                    if (!inOuter) Error("<fusion_end> appears without <fusion_begin>");
                    else if (inRecord)
                    {
                        Error("<fusion_end> appears before fusion_end:");
                        inRecord = false;
                    }
                    inOuter = false;
                    continue;
                }
                if (!inOuter)
                {
                    Warning("content outside <fusion_begin>/<fusion_end> was ignored");
                    continue;
                }
                int colon = line.IndexOf(':');
                if (colon < 0)
                {
                    Warning("fusion line without a field delimiter was ignored");
                    continue;
                }
                string key = Trim(line.Substring(0, colon));
                string value = Trim(line.Substring(colon + 1));
                if (key == "fusion")
                {
                    if (inRecord) Error("a new fusion: record started before fusion_end:");
                    Array.Clear(current, 0, current.Length);
                    sourceLine = lineNumber;
                    inRecord = true;
                    continue;
                }
                if (key == "fusion_end")
                {
                    if (!inRecord) Error("fusion_end: appears without fusion:");
                    else if (records.Count >= 50) Error("fusion record count exceeds the native limit of 50");
                    else records.Add(new LoganFusionRecord(
                        current[0], current[1], current[2], current[3], current[4], current[5],
                        current[6], current[7], current[8], current[9], current[10], current[11],
                        current[12], current[13], current[14], current[15], sourceLine));
                    inRecord = false;
                    continue;
                }
                if (!inRecord)
                {
                    Warning("fusion field outside a fusion:/fusion_end: record was ignored");
                    continue;
                }
                int field = FieldIndex(key);
                if (field < 0)
                {
                    Warning("unknown fusion field was ignored: " + key);
                    continue;
                }
                if (!TryInteger(value, out int parsed))
                {
                    Error("fusion field is not a strict integer: " + key);
                    continue;
                }
                current[field] = parsed;
            }
            if (!sawOuterBegin) Error("missing <fusion_begin>");
            if (inRecord) Error("fusion: record is missing fusion_end:");
            if (inOuter) Error("missing <fusion_end>");
            return new LoganFusionCatalog(true, records, diagnostics);
        }

        private static string Trim(string value)
        {
            int first = 0;
            int last = value.Length - 1;
            while (first <= last && IsTrimCharacter(value[first])) first++;
            while (last >= first && IsTrimCharacter(value[last])) last--;
            return value.Substring(first, last - first + 1);
        }

        private static bool IsTrimCharacter(char value) => value == ' ' || value == '\t' || value == '\r' || value == '\n';

        private static bool TryInteger(string value, out int result)
        {
            result = 0;
            if (value.Length == 0) return false;
            bool negative = value[0] == '-';
            int index = negative ? 1 : 0;
            if (index == value.Length) return false;
            long limit = negative ? 2147483648L : int.MaxValue;
            long magnitude = 0;
            for (; index < value.Length; index++)
            {
                char digit = value[index];
                if (digit < '0' || digit > '9') return false;
                magnitude = magnitude * 10 + digit - '0';
                if (magnitude > limit) return false;
            }
            result = (int)(negative ? -magnitude : magnitude);
            return true;
        }

        private static int FieldIndex(string key)
        {
            switch (key)
            {
                case "id1": return 0;
                case "id2": return 1;
                case "id3": return 2;
                case "hp": return 3;
                case "mp": return 4;
                case "respond": return 5;
                case "decrease": return 6;
                case "wait": return 7;
                case "state": return 8;
                case "action": return 9;
                case "frame": return 10;
                case "chp": return 11;
                case "hit_ja": return 12;
                case "cover": return 13;
                case "fronthurtact": return 14;
                case "backhurtact": return 15;
                default: return -1;
            }
        }
    }
}
