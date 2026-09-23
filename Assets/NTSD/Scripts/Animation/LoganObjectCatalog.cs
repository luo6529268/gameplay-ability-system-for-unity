using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NTSD.Animation
{
    /// <summary>Captured directory inputs, not a certificate of DAT semantics or published runtime readiness.</summary>
    public sealed class LoganObjectCatalog
    {
        public sealed class Entry
        {
            public int RegistryIndex { get; }
            public int Id { get; }
            public int Type { get; }
            public string SourcePath { get; }
            public string PublishedFolder { get; }
            public string DatPath { get; }
            public string DatText { get; }
            public string DatSha256 { get; }

            internal Entry(int registryIndex, int id, int type, string sourcePath,
                string publishedFolder, string datPath, byte[] bytes)
            {
                RegistryIndex = registryIndex;
                Id = id;
                Type = type;
                SourcePath = sourcePath;
                PublishedFolder = publishedFolder;
                DatPath = datPath;
                DatText = DecodeUtf8(bytes);
                DatSha256 = Hash(bytes);
            }
        }

        public BattleContentSource Source { get; }
        public ReadOnlyCollection<Entry> Entries { get; }
        public string CatalogSha256 { get; }
        /// <summary>Only catalog and object DAT inputs; image/audio identity belongs to complete publication.</summary>
        public string DefinitionFingerprint { get; }
        public LoganFusionCatalogInput FusionInput { get; }
        public LoganModeComboInput ModeComboInput { get; }
        public LoganKindCatalogInput KindInput { get; }
        public string BattleDefinitionFingerprint => ContentIdentity.RawDefinitionFingerprint;
        public LoganContentIdentity ContentIdentity { get; }
        public string SourceCacheKey { get; }

        private LoganObjectCatalog(BattleContentSource source, byte[] catalogBytes, List<Entry> entries)
        {
            Source = source;
            entries.Sort((a, b) => a.RegistryIndex.CompareTo(b.RegistryIndex));
            Entries = entries.AsReadOnly();
            CatalogSha256 = Hash(catalogBytes);
            using (var bytes = new MemoryStream())
            {
                using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
                {
                    writer.Write("LOGAN_OBJECT_DEFINITIONS_V1");
                    writer.Write(CatalogSha256);
                    foreach (Entry entry in entries)
                    {
                        writer.Write(entry.RegistryIndex);
                        writer.Write(Path.GetRelativePath(source.RuntimeRoot, entry.DatPath).Replace('\\', '/'));
                        writer.Write(entry.DatSha256);
                    }
                }
                DefinitionFingerprint = Hash(bytes.ToArray());
            }
            // Canonical portable layout: both native roots are RuntimeRoot; DatRoot is its decoded_dat.
            FusionInput = LoganFusionCatalogInput.Capture(source.DatRoot, source.RuntimeRoot);
            ModeComboInput = LoganModeComboInput.Capture(source);
            KindInput = LoganKindCatalogInput.Capture(source.DatRoot, source.RuntimeRoot);
            ContentIdentity = LoganContentIdentity.FromBattleComponents(DefinitionFingerprint,
                FusionInput.InputFingerprint, FusionInput.SemanticFingerprint,
                ModeComboInput?.InputFingerprint, ModeComboInput?.SemanticFingerprint,
                KindInput.InputFingerprint, KindInput.SemanticFingerprint);
            SourceCacheKey = ContentIdentity.CreateSourceCacheKey(source.RuntimeRoot);
        }

        public static LoganObjectCatalog Read(BattleContentSource source)
        {
            if (source == null || !source.IsLoganRuntime)
                throw new ArgumentException("A Logan runtime content source is required.", nameof(source));
            try
            {
                byte[] catalogBytes = File.ReadAllBytes(source.CatalogPath);
                using (var reader = new StringReader(DecodeUtf8(catalogBytes)))
                {
                    string headerLine = reader.ReadLine();
                    if (headerLine == null)
                        throw new InvalidDataException("catalog.csv is empty");
                    List<string> header = ParseCsvRow(headerLine);
                    var columns = new Dictionary<string, int>(StringComparer.Ordinal);
                    for (int i = 0; i < header.Count; i++) columns[header[i]] = i;
                    foreach (string required in new[] { "registry_section", "registry_index", "id", "type", "source_path", "published_folder" })
                        if (!columns.ContainsKey(required))
                            throw new InvalidDataException("missing catalog column: " + required);

                    var entries = new List<Entry>();
                    var ids = new HashSet<int>();
                    var indices = new HashSet<int>();
                    int lineNumber = 1;
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        List<string> row = ParseCsvRow(line);
                        string Field(string name) => columns[name] < row.Count ? row[columns[name]] : null;
                        if (Field("registry_section") != "object") continue;
                        string sourcePath = Field("source_path");
                        string folder = Field("published_folder");
                        if (!TryNativeInt(Field("registry_index"), out int index) || index < 0 ||
                            !TryNativeInt(Field("id"), out int id) || !TryNativeInt(Field("type"), out int type) ||
                            sourcePath == null || string.IsNullOrEmpty(folder))
                            throw new InvalidDataException("Line " + lineNumber + ": object row has invalid required values");
                        if (!ids.Add(id)) throw new InvalidDataException("Line " + lineNumber + ": duplicate object id: " + id);
                        if (!indices.Add(index)) throw new InvalidDataException("Line " + lineNumber + ": duplicate object registry index: " + index);

                        string publishedPath = source.ResolvePublishedFolderPath(folder);
                        string datPath;
                        if (Directory.Exists(publishedPath))
                        {
                            string[] files = Directory.EnumerateFiles(publishedPath)
                                .Where(path => string.Equals(Path.GetExtension(path), ".dat", StringComparison.OrdinalIgnoreCase)).ToArray();
                            if (files.Length != 1)
                                throw new InvalidDataException("Line " + lineNumber + ": published object folder must contain exactly one readable top-level DAT: " + publishedPath);
                            datPath = Path.GetFullPath(files[0]);
                        }
                        else
                        {
                            datPath = source.ResolveDatPath(sourcePath);
                            if (!File.Exists(datPath))
                                throw new InvalidDataException("Line " + lineNumber + ": published object folder and canonical decoded DAT are missing: " + publishedPath);
                        }
                        entries.Add(new Entry(index, id, type, sourcePath, folder, datPath, File.ReadAllBytes(datPath)));
                    }
                    return new LoganObjectCatalog(source, catalogBytes, entries);
                }
            }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException || error is ArgumentException)
            {
                throw new InvalidDataException("Logan catalog could not be captured: " + error.Message, error);
            }
        }

        private static bool TryNativeInt(string value, out int result)
        {
            result = 0;
            if (string.IsNullOrEmpty(value)) return false;
            int first = value[0] == '-' ? 1 : 0;
            if (first == value.Length) return false;
            for (int i = first; i < value.Length; i++)
                if (value[i] < '0' || value[i] > '9') return false;
            return int.TryParse(value, NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out result);
        }

        private static List<string> ParseCsvRow(string line)
        {
            // Alignment contract: NTSD28-B11-LOGAN-CATALOG-CONFIG-CANDIDATE-001
            // Match the native single-line CSV parser, including doubled quotes and last duplicate header.
            var columns = new List<string>();
            var value = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (quoted)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            value.Append('"');
                            i++;
                        }
                        else quoted = false;
                    }
                    else value.Append(ch);
                }
                else if (ch == '"' && value.Length == 0) quoted = true;
                else if (ch == ',')
                {
                    columns.Add(value.ToString());
                    value.Clear();
                }
                else if (ch != '\r') value.Append(ch);
            }
            columns.Add(value.ToString());
            return columns;
        }

        private static string DecodeUtf8(byte[] bytes)
        {
            int offset = bytes.Length >= 3 && bytes[0] == 0xef && bytes[1] == 0xbb && bytes[2] == 0xbf ? 3 : 0;
            return Encoding.UTF8.GetString(bytes, offset, bytes.Length - offset);
        }

        private static string Hash(byte[] bytes)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty);
        }
    }
}
