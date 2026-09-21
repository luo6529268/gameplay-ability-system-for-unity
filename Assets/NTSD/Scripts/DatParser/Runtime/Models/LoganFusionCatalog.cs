using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace NTSD.DatParser
{
    public enum LoganFusionDiagnosticSeverity
    {
        Warning,
        Error
    }

    public readonly struct LoganFusionDiagnostic
    {
        public LoganFusionDiagnosticSeverity Severity { get; }
        public int Line { get; }
        public string Message { get; }

        public LoganFusionDiagnostic(LoganFusionDiagnosticSeverity severity, int line, string message)
        {
            Severity = severity;
            Line = line;
            Message = message;
        }
    }

    public readonly struct LoganFusionRecord
    {
        public int Id1 { get; }
        public int Id2 { get; }
        public int Id3 { get; }
        public int Hp { get; }
        public int Mp { get; }
        public int Respond { get; }
        public int Decrease { get; }
        public int Wait { get; }
        public int State { get; }
        public int Action { get; }
        public int Frame { get; }
        public int Chp { get; }
        public int HitJa { get; }
        public int Cover { get; }
        public int FrontHurtAction { get; }
        public int BackHurtAction { get; }
        public int SourceLine { get; }

        public LoganFusionRecord(
            int id1,
            int id2,
            int id3,
            int hp,
            int mp,
            int respond,
            int decrease,
            int wait,
            int state,
            int action,
            int frame,
            int chp,
            int hitJa,
            int cover,
            int frontHurtAction,
            int backHurtAction,
            int sourceLine)
        {
            Id1 = id1;
            Id2 = id2;
            Id3 = id3;
            Hp = hp;
            Mp = mp;
            Respond = respond;
            Decrease = decrease;
            Wait = wait;
            State = state;
            Action = action;
            Frame = frame;
            Chp = chp;
            HitJa = hitJa;
            Cover = cover;
            FrontHurtAction = frontHurtAction;
            BackHurtAction = backHurtAction;
            SourceLine = sourceLine;
        }
    }

    public sealed class LoganFusionCatalog
    {
        public bool SourceAvailable { get; }
        public bool IsValid { get; }
        public ReadOnlyCollection<LoganFusionRecord> Records { get; }
        public ReadOnlyCollection<LoganFusionDiagnostic> Diagnostics { get; }

        internal LoganFusionCatalog(bool sourceAvailable, IEnumerable<LoganFusionRecord> records,
            IEnumerable<LoganFusionDiagnostic> diagnostics)
        {
            SourceAvailable = sourceAvailable;
            Records = new List<LoganFusionRecord>(records).AsReadOnly();
            Diagnostics = new List<LoganFusionDiagnostic>(diagnostics).AsReadOnly();
            bool valid = sourceAvailable;
            foreach (var diagnostic in Diagnostics)
            {
                if (diagnostic.Severity == LoganFusionDiagnosticSeverity.Error)
                    valid = false;
            }
            IsValid = valid;
        }
    }
}
