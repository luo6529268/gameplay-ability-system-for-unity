using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using NTSD.DatParser;

namespace NTSD.Animation
{
    public sealed class LoganFusionCatalogInput
    {
        private readonly string decodedDatRoot;
        private readonly string extractedRoot;
        private readonly Func<string, byte[]> regularFileReader;

        public LoganFusionCatalog Catalog { get; }
        public string SelectedPath { get; }
        public bool UsesLockedFallback { get; }
        public string InputFingerprint { get; }
        public string SemanticFingerprint { get; }

        private LoganFusionCatalogInput(string decodedDatRoot, string extractedRoot,
            Func<string, byte[]> regularFileReader, LoganFusionCatalog catalog,
            string selectedPath, byte[] bytes)
        {
            this.decodedDatRoot = decodedDatRoot;
            this.extractedRoot = extractedRoot;
            this.regularFileReader = regularFileReader;
            Catalog = catalog;
            SelectedPath = selectedPath;
            UsesLockedFallback = selectedPath == null;
            SemanticFingerprint = ComputeSemanticFingerprint(catalog);
            InputFingerprint = HashContract("NTSD28-FUSION-INPUT-v1",
                UsesLockedFallback ? "LOCKED_TABLE" : "FILE",
                UsesLockedFallback ? SemanticFingerprint : Hash(bytes));
        }

        public static LoganFusionCatalogInput Capture(string decodedDatRoot, string extractedRoot)
        {
            return Capture(decodedDatRoot, extractedRoot, ReadRegularFile);
        }

        internal static LoganFusionCatalogInput Capture(string decodedDatRoot, string extractedRoot,
            Func<string, byte[]> regularFileReader)
        {
            if (string.IsNullOrWhiteSpace(extractedRoot))
                throw new ArgumentException("An explicit extracted root is required.", nameof(extractedRoot));
            if (regularFileReader == null) throw new ArgumentNullException(nameof(regularFileReader));
            string decoded = string.IsNullOrEmpty(decodedDatRoot) ? null : Path.GetFullPath(decodedDatRoot);
            string extracted = Path.GetFullPath(extractedRoot);
            foreach (string path in CandidatePaths(decoded, extracted))
            {
                byte[] supplied = regularFileReader(path);
                if (supplied == null) continue;
                byte[] bytes = (byte[])supplied.Clone();
                // GetString preserves the BOM character; the native parser does not strip it.
                var catalog = LoganFusionCatalogParser.ParseText(Encoding.UTF8.GetString(bytes));
                if (!catalog.IsValid)
                {
                    var message = new StringBuilder("Invalid selected fusion DAT: ").Append(path);
                    foreach (var diagnostic in catalog.Diagnostics)
                        message.Append("; line ").Append(diagnostic.Line).Append(": ").Append(diagnostic.Message);
                    throw new InvalidDataException(message.ToString());
                }
                return new LoganFusionCatalogInput(decoded, extracted, regularFileReader, catalog, path, bytes);
            }
            return new LoganFusionCatalogInput(decoded, extracted, regularFileReader,
                CreateLockedFallback(), null, null);
        }

        public void AssertInputsCurrent()
        {
            var current = Capture(decodedDatRoot, extractedRoot, regularFileReader);
            if (!string.Equals(SelectedPath, current.SelectedPath, StringComparison.Ordinal) ||
                !string.Equals(InputFingerprint, current.InputFingerprint, StringComparison.Ordinal))
                throw new InvalidOperationException("Fusion catalog inputs changed after capture.");
        }

        private static IEnumerable<string> CandidatePaths(string decoded, string extracted)
        {
            if (decoded != null) yield return Path.Combine(decoded, "data", "fusion.dat");
            yield return Path.Combine(extracted, "data", "fusion.dat");
            yield return Path.Combine(extracted, "dat", "data", "fusion.dat");
            yield return Path.Combine(extracted, "assets", "data", "fusion.dat");
            yield return Path.Combine(extracted, "NTSD2.8", "data", "fusion.dat");
        }

        private static byte[] ReadRegularFile(string path)
        {
            return File.Exists(path) ? File.ReadAllBytes(path) : null;
        }

        internal static LoganFusionCatalog CreateLockedFallback()
        {
            // Exact FusionCatalog28::locked_runtime_table_2833, not an inferred Unity default.
            return new LoganFusionCatalog(true, new[]
            {
                new LoganFusionRecord(7, 8, 51, 177, 500, 1, 4500, 900, 2, 290, 112, 0, 0, 0, 9, 260, 0),
                new LoganFusionRecord(10, 11, 52, 375, 500, 0, 200, 100, 2, 310, 112, 1, 1, 1, 9, 260, 0)
            }, Array.Empty<LoganFusionDiagnostic>());
        }

        private static string ComputeSemanticFingerprint(LoganFusionCatalog catalog)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write("NTSD28-FUSION-SEMANTIC-v1");
                writer.Write(catalog.Records.Count);
                foreach (var record in catalog.Records)
                {
                    writer.Write(record.Id1); writer.Write(record.Id2); writer.Write(record.Id3);
                    writer.Write(record.Hp); writer.Write(record.Mp); writer.Write(record.Respond);
                    writer.Write(record.Decrease); writer.Write(record.Wait); writer.Write(record.State);
                    writer.Write(record.Action); writer.Write(record.Frame); writer.Write(record.Chp);
                    writer.Write(record.HitJa); writer.Write(record.Cover);
                    writer.Write(record.FrontHurtAction); writer.Write(record.BackHurtAction);
                }
            }
            return Hash(stream.ToArray());
        }

        private static string HashContract(string tag, string kind, string fingerprint)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                writer.Write(tag);
                writer.Write(kind);
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
