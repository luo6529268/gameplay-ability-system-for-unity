using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NTSD.Animation
{
    /// <summary>A captured definition/image input set. It is not a published or decoded presentation.</summary>
    public sealed class LoganVisualContentCandidate
    {
        public sealed class ImageInput
        {
            public string Path { get; }
            public string Sha256 { get; }

            internal ImageInput(string path)
            {
                Path = path;
                Sha256 = HashFile(path);
            }
        }

        private readonly Dictionary<string, string> imageHashes;

        public LoganObjectCatalog Catalog { get; }
        public LoganContentIdentity ContentIdentity => Catalog.ContentIdentity;
        public ReadOnlyCollection<ImageInput> Images { get; }
        public string VisualFingerprint { get; }
        public string SourceCacheKey { get; }

        private LoganVisualContentCandidate(LoganObjectCatalog catalog, List<ImageInput> images)
        {
            Catalog = catalog;
            Images = images.AsReadOnly();
            imageHashes = images.ToDictionary(image => image.Path, image => image.Sha256, StringComparer.Ordinal);
            using (var bytes = new MemoryStream())
            {
                using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
                {
                    writer.Write("LOGAN_VISUAL_INPUTS_V1");
                    writer.Write(catalog.DefinitionFingerprint);
                    foreach (ImageInput image in images)
                    {
                        writer.Write(Path.GetRelativePath(catalog.Source.ImageRoot, image.Path).Replace('\\', '/'));
                        writer.Write(image.Sha256);
                    }
                }
                using (var hash = SHA256.Create())
                    VisualFingerprint = Hex(hash.ComputeHash(bytes.ToArray()));
            }
            SourceCacheKey = ContentIdentity.CreateSourceCacheKey(catalog.Source.RuntimeRoot)
                + "|" + VisualFingerprint;
        }

        public static LoganVisualContentCandidate Capture(BattleContentSource source)
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(source);
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            var paths = new SortedSet<string>(StringComparer.Ordinal);
            foreach (LF2CharacterDataWrapper wrapper in configs.Values)
            {
                foreach (SpriteFileInfo file in wrapper.characterData.files)
                    paths.Add(Path.GetFullPath(file.filePath));
                if (!string.IsNullOrEmpty(wrapper.characterData.head))
                    paths.Add(Path.GetFullPath(wrapper.characterData.head));
                if (!string.IsNullOrEmpty(wrapper.characterData.small))
                    paths.Add(Path.GetFullPath(wrapper.characterData.small));
            }
            var images = new List<ImageInput>(paths.Count);
            foreach (string path in paths)
                images.Add(new ImageInput(path));
            var candidate = new LoganVisualContentCandidate(catalog, images);
            candidate.AssertInputsCurrent();
            return candidate;
        }

        /// <summary>Load-time freshness gate. Do not call from a simulation tick.</summary>
        public void AssertInputsCurrent()
        {
            LoganObjectCatalog current = LoganObjectCatalog.Read(Catalog.Source);
            if (current.DefinitionFingerprint != Catalog.DefinitionFingerprint ||
                current.ContentIdentity.SemanticFingerprint != ContentIdentity.SemanticFingerprint)
                throw new InvalidDataException("Logan catalog, DAT or decoder contract changed after candidate capture.");
            foreach (ImageInput image in Images)
                if (!string.Equals(HashFile(image.Path), image.Sha256, StringComparison.Ordinal))
                    throw new InvalidDataException("Logan image changed after candidate capture: " + image.Path);
        }

        internal Dictionary<int, LF2CharacterDataWrapper> CopyCharacterConfigs()
        {
            // Alignment contract: NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001
            // Cache captured DAT inputs, never a previous owner's mutable publication values.
            return CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(Catalog);
        }

        internal string GetImageSha256(string path)
        {
            string fullPath = Path.GetFullPath(path);
            if (!imageHashes.TryGetValue(fullPath, out string hash))
                throw new InvalidDataException("Image is not part of this Logan candidate: " + fullPath);
            return hash;
        }

        private static string HashFile(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                return Hex(hash.ComputeHash(stream));
        }

        private static string Hex(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
