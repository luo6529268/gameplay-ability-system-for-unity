using System;
using System.Collections.Generic;

namespace UnityEngine
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string header) { }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;

        public CreateAssetMenuAttribute() { }
    }

    public class ScriptableObject
    {
    }

    public static class Debug
    {
        private static readonly List<DiagnosticEntry> EntriesInternal =
            new List<DiagnosticEntry>();

        public static IReadOnlyList<DiagnosticEntry> Entries => EntriesInternal;

        public static void ClearAuditSink()
        {
            EntriesInternal.Clear();
        }

        public static void Log(object message)
        {
            EntriesInternal.Add(new DiagnosticEntry("Log", message));
        }

        public static void LogWarning(object message)
        {
            EntriesInternal.Add(new DiagnosticEntry("Warning", message));
        }

        public static void LogError(object message)
        {
            EntriesInternal.Add(new DiagnosticEntry("Error", message));
        }
    }

    public readonly struct DiagnosticEntry
    {
        public readonly string Level;
        public readonly string Message;

        public DiagnosticEntry(string level, object message)
        {
            Level = level;
            Message = message?.ToString() ?? string.Empty;
        }
    }

    public static class JsonUtility
    {
        public static string ToJson(object value, bool prettyPrint = true)
        {
            throw new NotSupportedException(
                "Unity JsonUtility is intentionally unavailable in the headless audit; the audit uses its own JSONL projection.");
        }
    }
}
