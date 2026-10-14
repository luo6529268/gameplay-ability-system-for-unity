using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

namespace NTSD.Animation
{
    internal sealed class BattleAstcTextures
    {
        [Serializable]
        private sealed class Manifest
        {
            public int version;
            public string pixelContract;
            public EntryDto[] entries;
        }

        [Serializable]
        private sealed class EntryDto
        {
            public string source;
            public string sourceSha256;
            public string astc;
            public string astcSha256;
            public int width;
            public int height;
        }

        internal sealed class Entry
        {
            internal readonly string SourcePath;
            internal readonly string SourceSha256;
            internal readonly string FilePath;
            internal readonly string FileSha256;
            internal readonly int Width;
            internal readonly int Height;
            internal int PayloadBytes => BattleAstcBlockData.PayloadBytes(Width, Height);
            internal long FileBytes => (long)PayloadBytes + BattleAstcBlockData.HeaderBytes;

            internal Entry(string source, string sourceHash, string file, string fileHash, int width, int height)
            {
                SourcePath = source;
                SourceSha256 = sourceHash;
                FilePath = file;
                FileSha256 = fileHash;
                Width = width;
                Height = height;
            }

            internal byte[] ReadVerifiedFile()
            {
                byte[] bytes = File.ReadAllBytes(FilePath);
                BattleAstcBlockData.ValidateFile(bytes, Width, Height, FileSha256);
                return bytes;
            }

            internal void AssertCurrent()
            {
                using (var stream = File.OpenRead(FilePath))
                {
                    var header = new byte[BattleAstcBlockData.HeaderBytes];
                    int read = 0;
                    while (read < header.Length)
                    {
                        int count = stream.Read(header, read, header.Length - read);
                        if (count == 0) throw new InvalidDataException("Truncated ASTC header.");
                        read += count;
                    }
                    BattleAstcBlockData.ValidateHeader(header, stream.Length, Width, Height);
                    stream.Position = 0;
                    using (var hash = SHA256.Create())
                    {
                        string actual = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
                        if (!string.Equals(actual, FileSha256, StringComparison.OrdinalIgnoreCase))
                            throw new InvalidDataException("ASTC input changed: " + FilePath);
                    }
                }
            }
        }

        private readonly Dictionary<string, Entry> entries;
        private readonly string manifestPath;
        internal string Fingerprint { get; }

        private BattleAstcTextures(string path, string fingerprint, Dictionary<string, Entry> selections)
        {
            manifestPath = path;
            Fingerprint = fingerprint;
            entries = selections;
        }

        internal bool TryGet(string sourcePath, string sourceSha256, out Entry entry)
        {
            if (!entries.TryGetValue(Path.GetFullPath(sourcePath), out entry)) return false;
            if (!string.Equals(entry.SourceSha256, sourceSha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("ASTC source identity mismatch: " + sourcePath);
            return true;
        }

        internal static string CurrentFingerprint(BattleContentSource source)
        {
            string path = Path.Combine(source.RuntimeRoot, "derived_astc", "manifest.json");
            return File.Exists(path) ? HashFile(path) : null;
        }

        internal static BattleAstcTextures Capture(BattleContentSource source,
            IEnumerable<LoganVisualContentCandidate.ImageInput> images)
        {
            if (!source.IsLoganRuntime) return null;
            string manifestPath = Path.Combine(source.RuntimeRoot, "derived_astc", "manifest.json");
            if (!File.Exists(manifestPath)) return null;
            var file = new FileInfo(manifestPath);
            if (file.Length <= 0 || file.Length > 8 * 1024 * 1024)
                throw new InvalidDataException("ASTC manifest length is invalid.");
            byte[] bytes = File.ReadAllBytes(manifestPath);
            string fingerprint = HashBytes(bytes);
            Manifest manifest = JsonUtility.FromJson<Manifest>(System.Text.Encoding.UTF8.GetString(bytes));
            if (manifest == null || manifest.version != 1 ||
                manifest.pixelContract != BattleAstcBlockData.PixelContract ||
                manifest.entries == null || manifest.entries.Length > 4096)
                throw new InvalidDataException("Unsupported ASTC manifest contract.");
            var sources = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var image in images) sources[Path.GetFullPath(image.Path)] = image.Sha256;
            var selections = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            foreach (EntryDto dto in manifest.entries)
            {
                string sourcePath = ResolveInside(source.RuntimeRoot, dto.source);
                string astcPath = ResolveInside(Path.GetDirectoryName(manifestPath), dto.astc);
                if (!sources.TryGetValue(sourcePath, out string expected) ||
                    !string.Equals(expected, dto.sourceSha256, StringComparison.OrdinalIgnoreCase) ||
                    string.IsNullOrEmpty(dto.astcSha256) || dto.astcSha256.Length != 64 ||
                    !astcPath.EndsWith(".astc", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Unknown or stale ASTC source selection: " + sourcePath);
                var entry = new Entry(sourcePath, expected, astcPath, dto.astcSha256, dto.width, dto.height);
                entry.AssertCurrent();
                if (selections.ContainsKey(sourcePath))
                    throw new InvalidDataException("Duplicate ASTC source selection: " + sourcePath);
                selections.Add(sourcePath, entry);
            }
            return new BattleAstcTextures(manifestPath, fingerprint, selections);
        }

        internal void AssertInputsCurrent()
        {
            if (!string.Equals(HashFile(manifestPath), Fingerprint, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("ASTC manifest changed after capture.");
            foreach (Entry entry in entries.Values) entry.AssertCurrent();
        }

        internal static bool IsCompressed(Texture2D texture)
        {
            return texture != null && texture.format == TextureFormat.ASTC_4x4;
        }

        internal static void Upload(Texture2D texture, Entry entry, byte[] bytes)
        {
            BattleAstcBlockData.ValidateFile(bytes, entry.Width, entry.Height, entry.FileSha256);
            GCHandle pinned = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            try
            {
                texture.LoadRawTextureData(IntPtr.Add(pinned.AddrOfPinnedObject(), BattleAstcBlockData.HeaderBytes),
                    entry.PayloadBytes);
                texture.Apply(false, true);
            }
            finally
            {
                pinned.Free();
            }
        }

        private static string ResolveInside(string root, string relative)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative))
                throw new InvalidDataException("ASTC paths must be relative.");
            string basePath = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string full = Path.GetFullPath(Path.Combine(basePath, relative));
            if (!full.StartsWith(basePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("ASTC path escapes its content root.");
            return full;
        }

        private static string HashFile(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private static string HashBytes(byte[] bytes)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty);
        }
    }
}
