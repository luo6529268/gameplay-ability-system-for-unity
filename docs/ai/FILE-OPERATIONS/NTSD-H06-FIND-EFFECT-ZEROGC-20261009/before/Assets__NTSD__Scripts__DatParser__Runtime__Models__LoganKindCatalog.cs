using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NTSD.DatParser
{
    public enum LoganKindDiagnosticSeverity
    {
        Warning,
        Error
    }

    public readonly struct LoganKindDiagnostic
    {
        public LoganKindDiagnosticSeverity Severity { get; }
        public int Line { get; }
        public string Message { get; }

        public LoganKindDiagnostic(LoganKindDiagnosticSeverity severity, int line, string message)
        {
            Severity = severity;
            Line = line;
            Message = message;
        }
    }

    public sealed class LoganKindRecord
    {
        public int Effect { get; }
        public int Frame { get; }
        public int SourceLine { get; }
        public ReadOnlyCollection<int> BoundIds { get; }
        public ReadOnlyCollection<int> RespondIds { get; }

        internal LoganKindRecord(int effect, int frame, IEnumerable<int> boundIds,
            IEnumerable<int> respondIds, int sourceLine)
        {
            Effect = effect;
            Frame = frame;
            SourceLine = sourceLine;
            BoundIds = new List<int>(boundIds).AsReadOnly();
            RespondIds = new List<int>(respondIds).AsReadOnly();
        }

        public bool Binds(int objectId) => BoundIds.Contains(objectId);
        public bool RespondsTo(int objectId) => RespondIds.Contains(objectId);
    }

    public sealed class LoganKindCatalog
    {
        public const int NativeRecordLimit = 100;

        public bool SourceAvailable { get; }
        public bool IsValid { get; }
        public ReadOnlyCollection<LoganKindRecord> Records { get; }
        public ReadOnlyCollection<LoganKindDiagnostic> Diagnostics { get; }

        internal LoganKindCatalog(bool sourceAvailable, IEnumerable<LoganKindRecord> records,
            IEnumerable<LoganKindDiagnostic> diagnostics)
        {
            SourceAvailable = sourceAvailable;
            Records = new List<LoganKindRecord>(records).AsReadOnly();
            Diagnostics = new List<LoganKindDiagnostic>(diagnostics).AsReadOnly();
            bool valid = sourceAvailable;
            foreach (LoganKindDiagnostic diagnostic in Diagnostics)
                if (diagnostic.Severity == LoganKindDiagnosticSeverity.Error)
                    valid = false;
            IsValid = valid;
        }

        public LoganKindRecord FindEffect(int effect)
        {
            foreach (LoganKindRecord record in Records)
                if (record.Effect == effect)
                    return record;
            return null;
        }
    }
}
