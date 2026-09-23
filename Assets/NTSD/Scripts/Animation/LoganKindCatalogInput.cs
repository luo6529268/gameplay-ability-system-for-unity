using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NTSD.DatParser;

namespace NTSD.Animation
{
    /// <summary>Frozen native kind DAT selection and parsed semantics before battle publication.</summary>
    public sealed class LoganKindCatalogInput
    {
        private const string LockedText =
            "<kind>\neffect: 209\nframe: 40\nbound: 3\n" +
            "id: 8\nid: 209\nid: 213\nbound_end:\nrespond: 7\n" +
            "id: 200\nid: 203\nid: 205\nid: 206\nid: 207\nid: 215\nid: 216\n" +
            "respond_end:\n<kind_end>\n";
        private static readonly LoganKindCatalog lockedCatalog =
            LoganKindCatalogParser.ParseText(LockedText);

        public static LoganKindCatalog LockedCatalog => lockedCatalog;

        private readonly string decodedDatRoot;
        private readonly string extractedRoot;
        private readonly Func<string, byte[]> regularFileReader;

        public LoganKindCatalog Catalog { get; }
        public string SelectedPath { get; }
        public bool UsesLockedFallback => SelectedPath == null;
        public string InputFingerprint { get; }
        public string SemanticFingerprint { get; }

        private LoganKindCatalogInput(string decodedDatRoot, string extractedRoot,
            Func<string, byte[]> regularFileReader, LoganKindCatalog catalog,
            string selectedPath, byte[] bytes)
        {
            this.decodedDatRoot = decodedDatRoot;
            this.extractedRoot = extractedRoot;
            this.regularFileReader = regularFileReader;
            Catalog = catalog;
            SelectedPath = selectedPath;
            SemanticFingerprint = ComputeSemanticFingerprint(catalog);
            InputFingerprint = HashContract("NTSD28-KIND-INPUT-v1",
                UsesLockedFallback ? "LOCKED_TABLE" : "FILE",
                UsesLockedFallback ? SemanticFingerprint : Hash(bytes));
        }

        public static LoganKindCatalogInput Capture(string decodedDatRoot, string extractedRoot)
        {
            return Capture(decodedDatRoot, extractedRoot, ReadRegularFile);
        }

        internal static LoganKindCatalogInput Capture(string decodedDatRoot, string extractedRoot,
            Func<string, byte[]> regularFileReader)
        {
            if (string.IsNullOrWhiteSpace(extractedRoot))
                throw new ArgumentException("An explicit extracted root is required.", nameof(extractedRoot));
            if (regularFileReader == null)
                throw new ArgumentNullException(nameof(regularFileReader));
            string decoded = string.IsNullOrEmpty(decodedDatRoot) ? null : Path.GetFullPath(decodedDatRoot);
            string extracted = Path.GetFullPath(extractedRoot);
            foreach (string path in CandidatePaths(decoded, extracted))
            {
                byte[] supplied = regularFileReader(path);
                if (supplied == null) continue;
                byte[] bytes = (byte[])supplied.Clone();
                // Keep a UTF-8 BOM as data: the native parser does not strip it.
                LoganKindCatalog catalog = LoganKindCatalogParser.ParseText(Encoding.UTF8.GetString(bytes));
                if (!catalog.IsValid)
                {
                    var message = new StringBuilder("Invalid selected kind DAT: ").Append(path);
                    foreach (LoganKindDiagnostic diagnostic in catalog.Diagnostics)
                        message.Append("; line ").Append(diagnostic.Line).Append(": ")
                            .Append(diagnostic.Message);
                    throw new InvalidDataException(message.ToString());
                }
                return new LoganKindCatalogInput(decoded, extracted, regularFileReader,
                    catalog, path, bytes);
            }
            return new LoganKindCatalogInput(decoded, extracted, regularFileReader,
                LockedCatalog, null, null);
        }

        public void AssertInputsCurrent()
        {
            LoganKindCatalogInput current = Capture(decodedDatRoot, extractedRoot, regularFileReader);
            if (!string.Equals(SelectedPath, current.SelectedPath, StringComparison.Ordinal) ||
                !string.Equals(InputFingerprint, current.InputFingerprint, StringComparison.Ordinal))
                throw new InvalidOperationException("Kind catalog inputs changed after capture.");
        }

        private static IEnumerable<string> CandidatePaths(string decoded, string extracted)
        {
            if (decoded != null) yield return Path.Combine(decoded, "data", "kind.dat");
            yield return Path.Combine(extracted, "data", "kind.dat");
            yield return Path.Combine(extracted, "dat", "data", "kind.dat");
            yield return Path.Combine(extracted, "assets", "data", "kind.dat");
            yield return Path.Combine(extracted, "NTSD2.8", "data", "kind.dat");
        }

        private static byte[] ReadRegularFile(string path)
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        private static string ComputeSemanticFingerprint(LoganKindCatalog catalog)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write("NTSD28-KIND-SEMANTIC-v1");
                writer.Write(catalog.Records.Count);
                foreach (LoganKindRecord record in catalog.Records)
                {
                    writer.Write(record.Effect);
                    writer.Write(record.Frame);
                    writer.Write(record.BoundIds.Count);
                    foreach (int id in record.BoundIds) writer.Write(id);
                    writer.Write(record.RespondIds.Count);
                    foreach (int id in record.RespondIds) writer.Write(id);
                }
            }
            return Hash(stream.ToArray());
        }

        private static string HashContract(string tag, string sourceKind, string fingerprint)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(tag);
                writer.Write(sourceKind);
                writer.Write(fingerprint);
            }
            return Hash(stream.ToArray());
        }

        private static string Hash(byte[] bytes)
        {
            using var algorithm = SHA256.Create();
            return BitConverter.ToString(algorithm.ComputeHash(bytes)).Replace("-", string.Empty);
        }
    }
}
