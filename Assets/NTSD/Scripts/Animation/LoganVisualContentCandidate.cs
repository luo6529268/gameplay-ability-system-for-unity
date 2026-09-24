using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NTSD.App;
using NTSD.DatParser;

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

        public sealed class NativeWordsInput
        {
            private const int FirstWordIndex = 16;
            private const int WordCount = 6;

            public string ResourceDatPath { get; }
            public ReadOnlyCollection<ImageInput> Images { get; }
            public string InputFingerprint { get; }

            private NativeWordsInput(BattleContentSource source, string resourceDatPath,
                byte[] resourceDatBytes, IReadOnlyList<string> virtualPaths)
            {
                ResourceDatPath = resourceDatPath;
                var images = new List<ImageInput>(WordCount);
                using (var bytes = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
                    {
                        writer.Write("NTSD28_WORDS_INPUT_V1");
                        writer.Write(HashBytes(resourceDatBytes));
                        for (int index = 0; index < WordCount; index++)
                        {
                            string virtualPath = virtualPaths[FirstWordIndex + index].Replace('\\', '/');
                            string path = source.ResolveImagePath(virtualPath, null);
                            var image = new ImageInput(path);
                            images.Add(image);
                            writer.Write(virtualPath);
                            writer.Write(image.Sha256);
                        }
                    }
                    InputFingerprint = HashBytes(bytes.ToArray());
                }
                Images = images.AsReadOnly();
            }

            public static NativeWordsInput Capture(BattleContentSource source)
            {
                if (source == null || !source.IsLoganRuntime)
                    throw new ArgumentException("A Logan runtime source is required.", nameof(source));

                string resourceDatPath = Path.Combine(source.DatRoot, "data", "resource.dat");
                if (!File.Exists(resourceDatPath))
                    return null;

                byte[] resourceDatBytes = File.ReadAllBytes(resourceDatPath);
                IReadOnlyList<string> paths = FirstNativeResourceTable(Encoding.UTF8.GetString(resourceDatBytes));
                if (paths.Count < FirstWordIndex + WordCount)
                    throw new InvalidDataException("Native resource.dat has no complete WORDS0..WORDS5 index range.");
                return new NativeWordsInput(source, resourceDatPath, resourceDatBytes, paths);
            }

            internal static IReadOnlyList<string> FirstNativeResourceTable(string text)
            {
                string[] tokens = text.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
                for (int index = 0; index < tokens.Length; index++)
                {
                    if (tokens[index] != "<bmp_begin>")
                        continue;

                    var paths = new List<string>();
                    for (++index; index < tokens.Length; index++)
                    {
                        if (tokens[index] == "<bmp_end>")
                            return paths;
                        if (tokens[index] != "pic:")
                            continue;
                        if (++index >= tokens.Length || tokens[index].StartsWith("<", StringComparison.Ordinal))
                            throw new InvalidDataException("Native resource.dat pic: has no path.");
                        if (paths.Count < 48)
                            paths.Add(tokens[index]);
                    }
                    throw new InvalidDataException("Native resource.dat has no <bmp_end>.");
                }
                throw new InvalidDataException("Native resource.dat has no <bmp_begin>.");
            }
        }

        public sealed class NativeSparkInput
        {
            private const int ResourceIndex = 43;

            public ImageInput Image { get; }
            public int Width { get; }
            public int Height { get; }
            public string InputFingerprint { get; }

            private NativeSparkInput(BattleContentSource source, byte[] resourceBytes,
                byte[] systemBytes)
            {
                IReadOnlyList<string> paths = NativeWordsInput.FirstNativeResourceTable(
                    Encoding.UTF8.GetString(resourceBytes));
                if (paths.Count <= ResourceIndex)
                    throw new InvalidDataException("Native resource.dat has no SPARK index 43.");

                string virtualPath = paths[ResourceIndex].Replace('\\', '/');
                Image = new ImageInput(source.ResolveImagePath(virtualPath, null));

                var fields = new List<Lf2DatProperty>();
                Lf2DatTokenizer.ScanLoganScalarFields(
                    Encoding.UTF8.GetString(systemBytes), fields, "system");
                Width = ReadDimension(fields, "spark_w");
                Height = ReadDimension(fields, "spark_h");
                using (var bytes = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
                    {
                        writer.Write("NTSD28_SPARK_INPUT_V1");
                        writer.Write(HashBytes(resourceBytes));
                        writer.Write(HashBytes(systemBytes));
                        writer.Write(virtualPath);
                        writer.Write(Image.Sha256);
                        writer.Write(Width);
                        writer.Write(Height);
                    }
                    InputFingerprint = HashBytes(bytes.ToArray());
                }
            }

            public static NativeSparkInput Capture(BattleContentSource source)
            {
                if (source == null || !source.IsLoganRuntime)
                    throw new ArgumentException("A Logan runtime source is required.", nameof(source));
                string resourcePath = Path.Combine(source.DatRoot, "data", "resource.dat");
                string systemPath = Path.Combine(source.DatRoot, "data", "system.dat");
                bool hasResource = File.Exists(resourcePath);
                bool hasSystem = File.Exists(systemPath);
                if (!hasResource && !hasSystem)
                    return null;
                if (!hasResource || !hasSystem)
                    throw new InvalidDataException("Native SPARK requires both resource.dat and system.dat.");
                return new NativeSparkInput(source, File.ReadAllBytes(resourcePath),
                    File.ReadAllBytes(systemPath));
            }

            private static int ReadDimension(List<Lf2DatProperty> fields, string key)
            {
                int value = 0;
                int count = 0;
                foreach (Lf2DatProperty field in fields)
                {
                    if (field.Key != key)
                        continue;
                    count++;
                    if (!int.TryParse(field.Value, NumberStyles.Integer,
                            CultureInfo.InvariantCulture, out value) || value <= 0)
                        throw new InvalidDataException("Native system.dat has an invalid " + key + ".");
                }
                if (count != 1)
                    throw new InvalidDataException("Native system.dat must define one " + key + ".");
                return value;
            }
        }

        public sealed class NativeKillIconInput
        {
            private const int TypeCount = 7;

            public ReadOnlyCollection<ImageInput> Images { get; }
            public string InputFingerprint { get; }

            private NativeKillIconInput(BattleContentSource source,
                LoganModeKnockoutFeedInput feed)
            {
                var images = new List<ImageInput>(TypeCount);
                using (var bytes = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
                    {
                        writer.Write("NTSD28_KILL_ICON_INPUT_V1");
                        for (int type = 0; type < TypeCount; type++)
                        {
                            string virtualPath = feed.TypeResourcePath(type)?.Replace('\\', '/')
                                ?? string.Empty;
                            string path = virtualPath.Length == 0
                                ? string.Empty
                                : source.ResolveImagePath(virtualPath, null);
                            ImageInput image = path.Length > 0 && File.Exists(path)
                                ? new ImageInput(path)
                                : null;
                            images.Add(image);
                            writer.Write(type);
                            writer.Write(virtualPath);
                            writer.Write(image != null);
                            if (image != null)
                                writer.Write(image.Sha256);
                        }
                    }
                    InputFingerprint = HashBytes(bytes.ToArray());
                }
                Images = images.AsReadOnly();
            }

            public static NativeKillIconInput Capture(BattleContentSource source,
                LoganModeKnockoutFeedInput feed)
            {
                if (source == null || !source.IsLoganRuntime)
                    throw new ArgumentException("A Logan runtime source is required.", nameof(source));
                return feed == null ? null : new NativeKillIconInput(source, feed);
            }
        }

        private readonly Dictionary<string, string> imageHashes;

        public LoganObjectCatalog Catalog { get; }
        public NativeWordsInput WordsInput { get; }
        public NativeSparkInput SparkInput { get; }
        public NativeKillIconInput KillIconInput { get; }
        public LoganContentIdentity ContentIdentity => Catalog.ContentIdentity;
        public ReadOnlyCollection<ImageInput> Images { get; }
        public string VisualFingerprint { get; }
        public string SourceCacheKey { get; }

        private LoganVisualContentCandidate(LoganObjectCatalog catalog, List<ImageInput> images,
            NativeWordsInput wordsInput, NativeKillIconInput killIconInput,
            NativeSparkInput sparkInput)
        {
            Catalog = catalog;
            WordsInput = wordsInput;
            KillIconInput = killIconInput;
            SparkInput = sparkInput;
            Images = images.AsReadOnly();
            imageHashes = images.ToDictionary(image => image.Path, image => image.Sha256, StringComparer.Ordinal);
            using (var bytes = new MemoryStream())
            {
                using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
                {
                    writer.Write(sparkInput != null ? "LOGAN_VISUAL_INPUTS_V4" :
                        killIconInput != null ? "LOGAN_VISUAL_INPUTS_V3" :
                        wordsInput == null ? "LOGAN_VISUAL_INPUTS_V1" : "LOGAN_VISUAL_INPUTS_V2");
                    writer.Write(catalog.DefinitionFingerprint);
                    foreach (ImageInput image in images)
                    {
                        writer.Write(Path.GetRelativePath(catalog.Source.ImageRoot, image.Path).Replace('\\', '/'));
                        writer.Write(image.Sha256);
                    }
                    if (wordsInput != null)
                        writer.Write(wordsInput.InputFingerprint);
                    if (killIconInput != null)
                    {
                        writer.Write(wordsInput != null);
                        writer.Write(killIconInput.InputFingerprint);
                    }
                    if (sparkInput != null)
                        writer.Write(sparkInput.InputFingerprint);
                }
                using (var hash = SHA256.Create())
                    VisualFingerprint = Hex(hash.ComputeHash(bytes.ToArray()));
            }
            SourceCacheKey = ContentIdentity.CreateSourceCacheKey(catalog.Source.RuntimeRoot)
                + "|" + VisualFingerprint;
        }

        public static LoganVisualContentCandidate Capture(BattleContentSource source,
            ProjectBattleModeConfig.Snapshot projectModeSnapshot = null)
        {
            LoganObjectCatalog catalog = LoganObjectCatalog.Read(source, projectModeSnapshot);
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
            var candidate = new LoganVisualContentCandidate(catalog, images,
                NativeWordsInput.Capture(source),
                NativeKillIconInput.Capture(source, catalog.ModeComboInput?.KnockoutFeed),
                NativeSparkInput.Capture(source));
            candidate.AssertInputsCurrent();
            return candidate;
        }

        /// <summary>Load-time freshness gate. Do not call from a simulation tick.</summary>
        public void AssertInputsCurrent()
        {
            Catalog.ModeComboInput?.AssertInputsCurrent();
            NativeWordsInput currentWords = NativeWordsInput.Capture(Catalog.Source);
            if (!string.Equals(currentWords?.InputFingerprint, WordsInput?.InputFingerprint,
                    StringComparison.Ordinal))
                throw new InvalidDataException("Logan WORDS resource inputs changed after candidate capture.");
            NativeKillIconInput currentKillIcons = NativeKillIconInput.Capture(
                Catalog.Source, Catalog.ModeComboInput?.KnockoutFeed);
            if (!string.Equals(currentKillIcons?.InputFingerprint,
                    KillIconInput?.InputFingerprint, StringComparison.Ordinal))
                throw new InvalidDataException(
                    "Logan knockout-feed icon inputs changed after candidate capture.");
            NativeSparkInput currentSpark = NativeSparkInput.Capture(Catalog.Source);
            if (!string.Equals(currentSpark?.InputFingerprint, SparkInput?.InputFingerprint,
                    StringComparison.Ordinal))
                throw new InvalidDataException("Logan SPARK inputs changed after candidate capture.");
            try
            {
                Catalog.KindInput.AssertInputsCurrent();
            }
            catch (InvalidOperationException error)
            {
                throw new InvalidDataException("Logan kind input selection changed after candidate capture.", error);
            }
            try
            {
                Catalog.FusionInput.AssertInputsCurrent();
            }
            catch (InvalidOperationException error)
            {
                // Preserve the existing publication cache's stale-input invalidation contract.
                throw new InvalidDataException("Logan fusion input selection changed after candidate capture.", error);
            }
            LoganObjectCatalog current = LoganObjectCatalog.Read(Catalog.Source,
                Catalog.ProjectModeSnapshot);
            if (current.DefinitionFingerprint != Catalog.DefinitionFingerprint ||
                current.ContentIdentity.SemanticFingerprint != ContentIdentity.SemanticFingerprint)
                throw new InvalidDataException("Logan catalog, DAT, mode or decoder contract changed after candidate capture.");
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

        private static string HashBytes(byte[] bytes)
        {
            using (var hash = SHA256.Create())
                return Hex(hash.ComputeHash(bytes));
        }

        private static string Hex(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", string.Empty);
        }
    }
}
