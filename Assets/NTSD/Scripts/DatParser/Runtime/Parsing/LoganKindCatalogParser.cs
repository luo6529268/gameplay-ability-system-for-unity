using System;
using System.Collections.Generic;

namespace NTSD.DatParser
{
    public static class LoganKindCatalogParser
    {
        private enum ListSection
        {
            None,
            Bound,
            Respond
        }

        public static LoganKindCatalog ParseText(string text)
        {
            text ??= string.Empty;
            var records = new List<LoganKindRecord>();
            var diagnostics = new List<LoganKindDiagnostic>();
            var boundIds = new List<int>();
            var respondIds = new List<int>();
            bool inRecord = false;
            ListSection section = ListSection.None;
            int? declaredCount = null;
            int effect = 0;
            int frame = 0;
            int sourceLine = 0;
            int lineNumber = 0;

            void Error(string message) => diagnostics.Add(new LoganKindDiagnostic(
                LoganKindDiagnosticSeverity.Error, lineNumber, message));
            void Warning(string message) => diagnostics.Add(new LoganKindDiagnostic(
                LoganKindDiagnosticSeverity.Warning, lineNumber, message));
            void CloseList(ListSection expected, string label)
            {
                if (section != expected || !declaredCount.HasValue)
                {
                    Error(label + " appears outside its list");
                    return;
                }
                int count = section == ListSection.Bound ? boundIds.Count : respondIds.Count;
                if (count != declaredCount.Value)
                    Error(label + " count does not match parsed id fields");
                section = ListSection.None;
                declaredCount = null;
            }

            // std::getline splits on LF only; CR is trimmed after comment stripping.
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

                if (line == "<kind>")
                {
                    if (inRecord) Error("nested <kind> started before <kind_end>");
                    boundIds.Clear();
                    respondIds.Clear();
                    effect = 0;
                    frame = 0;
                    sourceLine = lineNumber;
                    inRecord = true;
                    section = ListSection.None;
                    declaredCount = null;
                    continue;
                }
                if (line == "<kind_end>")
                {
                    if (!inRecord)
                        Error("<kind_end> appears without <kind>");
                    else
                    {
                        if (section != ListSection.None)
                            Error("<kind_end> appears before the active list end");
                        if (records.Count >= LoganKindCatalog.NativeRecordLimit)
                            Error("kind record count exceeds the native limit of 100");
                        else
                            records.Add(new LoganKindRecord(effect, frame, boundIds,
                                respondIds, sourceLine));
                    }
                    boundIds.Clear();
                    respondIds.Clear();
                    effect = 0;
                    frame = 0;
                    inRecord = false;
                    section = ListSection.None;
                    declaredCount = null;
                    continue;
                }
                if (!inRecord)
                {
                    Warning("content outside <kind>/<kind_end> was ignored");
                    continue;
                }
                int colon = line.IndexOf(':');
                if (colon < 0)
                {
                    Warning("kind line without a field delimiter was ignored");
                    continue;
                }
                string key = Trim(line.Substring(0, colon));
                string value = Trim(line.Substring(colon + 1));
                if (key == "bound_end")
                {
                    CloseList(ListSection.Bound, "bound_end:");
                    continue;
                }
                if (key == "respond_end")
                {
                    CloseList(ListSection.Respond, "respond_end:");
                    continue;
                }
                if (key == "bound" || key == "respond")
                {
                    if (section != ListSection.None)
                    {
                        Error("a kind list started before the previous list ended");
                        continue;
                    }
                    if (!TryInteger(value, out int count) || count < 0)
                    {
                        Error("kind list count is not a nonnegative integer: " + key);
                        continue;
                    }
                    section = key == "bound" ? ListSection.Bound : ListSection.Respond;
                    declaredCount = count;
                    continue;
                }
                if (key == "id")
                {
                    if (section == ListSection.None || !declaredCount.HasValue)
                    {
                        Error("id: appears outside bound/respond list");
                        continue;
                    }
                    if (!TryInteger(value, out int id))
                    {
                        Error("kind list id is not a strict integer");
                        continue;
                    }
                    List<int> values = section == ListSection.Bound ? boundIds : respondIds;
                    if (values.Count >= declaredCount.Value)
                    {
                        Error("kind list contains more ids than its declared count");
                        continue;
                    }
                    values.Add(id);
                    continue;
                }
                if (section != ListSection.None)
                {
                    Warning("unknown field inside kind id list was ignored: " + key);
                    continue;
                }
                if (key == "effect" || key == "frame")
                {
                    if (!TryInteger(value, out int parsed))
                        Error("kind field is not a strict integer: " + key);
                    else if (key == "effect") effect = parsed;
                    else frame = parsed;
                    continue;
                }
                Warning("unknown kind field was ignored: " + key);
            }
            if (inRecord) Error("<kind> record is missing <kind_end>");
            return new LoganKindCatalog(true, records, diagnostics);
        }

        private static string Trim(string value)
        {
            int first = 0;
            int last = value.Length - 1;
            while (first <= last && IsTrimCharacter(value[first])) first++;
            while (last >= first && IsTrimCharacter(value[last])) last--;
            return value.Substring(first, last - first + 1);
        }

        private static bool IsTrimCharacter(char value) =>
            value == ' ' || value == '\t' || value == '\r' || value == '\n';

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
    }
}
