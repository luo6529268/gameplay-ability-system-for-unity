using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace NTSD.Animation
{
    // Alignment contract: NTSD-M12-DAT-SPRITE-PAGING-PILOT-001.
    internal sealed class BattleDerivedSpritePages
    {
        internal const string RelativeManifestPath = "derived_sprite_pages/manifest.json";

        internal sealed class Page
        {
            internal readonly string Id, Path, Sha256;
            internal readonly int Width, Height;

            internal Page(PageJson data, string path)
            {
                Id = data.id; Path = path; Sha256 = data.sha256;
                Width = data.width; Height = data.height;
            }
        }

        internal sealed class Chunk
        {
            internal readonly Page Page;
            internal readonly int FirstLocalPic, Count, Columns, Rows;
            internal readonly ReadOnlyCollection<Rect> Rects;
            internal readonly ReadOnlyCollection<Rect> ContentRects;
            internal readonly ReadOnlyCollection<BattleSpriteReplacementFrame.Patch> ReplacementPatches;

            internal Chunk(Page page, ChunkJson data, int frameWidth, int frameHeight)
            {
                ReplacementPatches = data.replacementPatches == null ? null : Array.AsReadOnly(
                    data.replacementPatches.Select(patch => patch == null ? default : new BattleSpriteReplacementFrame.Patch(
                        new Rect(patch.source.x, patch.source.y, patch.source.width, patch.source.height),
                        new Rect(patch.destination.x, patch.destination.y, patch.destination.width, patch.destination.height))).ToArray());
                Page = page; FirstLocalPic = data.firstLocalPic; Count = data.count;
                Columns = data.columns; Rows = data.rows;
                Rects = Array.AsReadOnly(data.rects.Select(rect =>
                    new Rect(rect.x, rect.y, rect.width, rect.height)).ToArray());
                ContentRects = Array.AsReadOnly(data.contentRects == null
                    ? Enumerable.Repeat(new Rect(0, 0, frameWidth, frameHeight), data.count).ToArray()
                    : data.contentRects.Select(rect => new Rect(rect.x, rect.y, rect.width, rect.height)).ToArray());
            }
        }

        internal sealed class Source
        {
            internal readonly string Path, Sha256;
            internal readonly int Width, Height, FrameWidth, FrameHeight, Columns, Rows;
            internal readonly ReadOnlyCollection<Chunk> Chunks;
            private readonly HashSet<string> bindings;

            internal Source(SourceJson data, string path, IEnumerable<Chunk> chunks)
            {
                Path = path; Sha256 = data.sha256; Width = data.width; Height = data.height;
                FrameWidth = data.frameWidth; FrameHeight = data.frameHeight;
                Columns = data.columns; Rows = data.rows;
                Chunks = Array.AsReadOnly(chunks.ToArray());
                bindings = new HashSet<string>(data.bindings.Select(binding =>
                    BindingKey(binding.oid, binding.firstPic, binding.lastPic)), StringComparer.Ordinal);
            }

            internal void RequireBinding(int oid, SpriteFileInfo file)
            {
                if (!bindings.Contains(BindingKey(oid, file.startFrame, file.endFrame)) ||
                    file.width != FrameWidth || file.height != FrameHeight ||
                    file.row != Columns || file.col != Rows)
                    throw new InvalidDataException("Derived page binding differs from native DAT: " + Path);
            }
        }

        internal readonly ReadOnlyCollection<Page> Pages;
        internal readonly string Fingerprint;
        private readonly string manifestPath;
        private readonly Dictionary<string, Source> sources;
        private readonly HashSet<string> pagePaths;
        private readonly Dictionary<string, string> datHashes;

        private BattleDerivedSpritePages(string path, string fingerprint, List<Page> pages,
            Dictionary<string, Source> sources, Dictionary<string, string> datHashes)
        {
            manifestPath = path; Fingerprint = fingerprint;
            Pages = pages.AsReadOnly(); this.sources = sources; this.datHashes = datHashes;
            pagePaths = new HashSet<string>(pages.Select(page => page.Path), StringComparer.OrdinalIgnoreCase);
        }

        internal static BattleDerivedSpritePages Capture(LoganObjectCatalog catalog,
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs)
        {
            return Load(catalog.Source, configs,
                catalog.Entries.ToDictionary(entry => entry.Id, entry => entry.DatPath));
        }

        internal static BattleDerivedSpritePages Load(BattleContentSource source,
            IReadOnlyDictionary<int, LF2CharacterDataWrapper> configs,
            IReadOnlyDictionary<int, string> datPaths)
        {
            string manifestPath = Path.Combine(source.RuntimeRoot, RelativeManifestPath);
            if (!File.Exists(manifestPath)) return null;
            byte[] bytes = File.ReadAllBytes(manifestPath);
            if (bytes.Length == 0 || bytes.Length > 4 * 1024 * 1024)
                throw new InvalidDataException("Derived page manifest size is invalid.");
            ManifestJson manifest;
            try
            {
                manifest = JsonConvert.DeserializeObject<ManifestJson>(Encoding.UTF8.GetString(bytes),
                    new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
            }
            catch (JsonException error)
            {
                throw new InvalidDataException("Derived page manifest is invalid JSON.", error);
            }
            if (manifest == null || manifest.version != 1 ||
                manifest.generatorVersion != "NTSD_DAT_PAGES_V1" ||
                manifest.pages == null || manifest.sources == null ||
                manifest.pages.Length == 0 || manifest.sources.Length == 0)
                throw new InvalidDataException("Unsupported or empty derived page manifest.");

            var pages = new List<Page>();
            var byId = new Dictionary<string, Page>(StringComparer.Ordinal);
            var usedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (PageJson data in manifest.pages)
            {
                if (data == null || string.IsNullOrWhiteSpace(data.id) || byId.ContainsKey(data.id) ||
                    data.width <= 0 || data.height <= 0 || data.width > 2048 || data.height > 2048)
                    throw new InvalidDataException("Invalid derived page descriptor.");
                string path = ResolveWithin(Path.GetDirectoryName(manifestPath), data.path);
                if (!usedPaths.Add(path))
                    throw new InvalidDataException("Duplicate derived page path.");
                BMPLoader.VerifiedImageLayout layout = BMPLoader.ReadVerifiedImageLayout(path, data.sha256);
                if (!layout.IsPng || layout.Width != data.width || layout.Height != data.height)
                    throw new InvalidDataException("Derived page header differs from manifest: " + path);
                var page = new Page(data, path);
                pages.Add(page); byId.Add(data.id, page);
            }
            var sources = new Dictionary<string, Source>(StringComparer.OrdinalIgnoreCase);
            var datHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var usedPages = new HashSet<string>(StringComparer.Ordinal);
            foreach (SourceJson data in manifest.sources)
            {
                if (data == null || data.width <= 0 || data.height <= 0 ||
                    data.frameWidth <= 0 || data.frameHeight <= 0 ||
                    data.frameWidth > 2046 || data.frameHeight > 2046 ||
                    data.columns <= 0 || data.rows <= 0 ||
                    data.bindings == null || data.bindings.Length == 0 ||
                    data.chunks == null || data.chunks.Length == 0)
                    throw new InvalidDataException("Invalid derived source descriptor.");
                string path = ResolveWithin(source.ImageRoot, data.path);
                if (sources.ContainsKey(path))
                    throw new InvalidDataException("Duplicate derived source path.");
                BMPLoader.VerifiedImageLayout layout = BMPLoader.ReadVerifiedImageLayout(path, data.sha256);
                if (!layout.IsPng || layout.Width != data.width || layout.Height != data.height)
                    throw new InvalidDataException("Derived source identity/geometry changed: " + path);
                int capacity = checked(data.columns * data.rows);
                if (capacity > 100000)
                    throw new InvalidDataException("Derived source has excessive cells.");
                for (int local = 0; !data.clampBaked && local < capacity; local++)
                {
                    long x = (long)(local % data.columns) * (data.frameWidth + 1L);
                    long y = (long)(local / data.columns) * (data.frameHeight + 1L);
                    if (x + data.frameWidth > data.width || y + data.frameHeight > data.height)
                        throw new InvalidDataException("Derived paging cannot replace clamped cells.");
                }
                var bindingKeys = new HashSet<string>(StringComparer.Ordinal);
                foreach (BindingJson binding in data.bindings)
                {
                    if (binding == null || !configs.TryGetValue(binding.oid, out var config) ||
                        !datPaths.TryGetValue(binding.oid, out string actualDatPath) ||
                        !bindingKeys.Add(BindingKey(binding.oid, binding.firstPic, binding.lastPic)))
                        throw new InvalidDataException("Unknown or duplicate derived DAT binding.");
                    string datPath = ResolveWithin(source.DatRoot, binding.datPath);
                    if (!string.Equals(Path.GetFullPath(actualDatPath), datPath, StringComparison.OrdinalIgnoreCase) ||
                        !EqualHash(HashFile(datPath), binding.datSha256))
                        throw new InvalidDataException("Derived page DAT identity changed.");
                    datHashes[datPath] = binding.datSha256;
                    SpriteFileInfo file = config.characterData.files.FirstOrDefault(info =>
                        string.Equals(Path.GetFullPath(info.filePath), path, StringComparison.OrdinalIgnoreCase) &&
                        info.startFrame == binding.firstPic && info.endFrame == binding.lastPic);
                    if (file == null || file.width != data.frameWidth || file.height != data.frameHeight ||
                        file.row != data.columns || file.col != data.rows ||
                        (long)binding.lastPic - binding.firstPic + 1 != capacity)
                        throw new InvalidDataException("Derived binding differs from native cumulative pic range.");
                }
                var chunks = new List<Chunk>();
                int next = 0;
                foreach (ChunkJson chunk in data.chunks)
                {
                    if (chunk == null || !byId.TryGetValue(chunk.page ?? "", out Page page) ||
                        chunk.firstLocalPic != next || chunk.count <= 0 ||
                        chunk.columns <= 0 || chunk.rows <= 0 ||
                        (long)chunk.columns * chunk.rows < chunk.count ||
                        chunk.rects == null || chunk.rects.Length != chunk.count ||
                        (chunk.contentRects != null && chunk.contentRects.Length != chunk.count))
                        throw new InvalidDataException("Derived chunks have a gap, overlap or invalid capacity.");
                    next = checked(next + chunk.count);
                    if (next > capacity) throw new InvalidDataException("Derived chunks exceed source cells.");
                    for (int index = 0; index < chunk.rects.Length; index++)
                    {
                        RectJson rect = chunk.rects[index];
                        RectJson content = chunk.contentRects == null
                            ? new RectJson { x = 0, y = 0, width = data.frameWidth, height = data.frameHeight }
                            : chunk.contentRects[index];
                        if (content == null || content.x < 0 || content.y < 0 ||
                            content.width <= 0 || content.height <= 0 ||
                            (long)content.x + content.width > data.frameWidth ||
                            (long)content.y + content.height > data.frameHeight ||
                            rect == null || rect.width != content.width || rect.height != content.height ||
                            rect.x < 1 || rect.y < 1 ||
                            (long)rect.x + rect.width >= page.Width ||
                            (long)rect.y + rect.height >= page.Height)
                            throw new InvalidDataException("Derived sprite rectangle is invalid.");
                        for (int other = 0; other < index; other++)
                        {
                            RectJson prior = chunk.rects[other];
                            if (rect.x < (long)prior.x + prior.width && prior.x < (long)rect.x + rect.width &&
                                rect.y < (long)prior.y + prior.height && prior.y < (long)rect.y + rect.height)
                            {
                                bool exactAlias = chunk.allowExactFrameAliases &&
                                    rect.x == prior.x && rect.y == prior.y &&
                                    rect.width == prior.width && rect.height == prior.height;
                                if (!exactAlias)
                                    throw new InvalidDataException("Derived sprite rectangles overlap.");
                            }
                        }
                    }
                    if (chunk.replacementPatches != null)
                    {
                        if (chunk.count > BattleSpriteReplacementFrame.MaximumFrames ||
                            chunk.replacementPatches.Length != chunk.count || !chunk.allowExactFrameAliases ||
                            chunk.contentRects != null || chunk.rects.Any(rect => rect.x != chunk.rects[0].x || rect.y != chunk.rects[0].y))
                            throw new InvalidDataException("Replacement frames require one shared full-canvas base.");
                        var physical = new List<RectJson> { chunk.rects[0] };
                        foreach (PatchJson patch in chunk.replacementPatches)
                        {
                            if (patch == null) continue;
                            RectJson p = patch.source, d = patch.destination;
                            if (p == null || d == null || p.width <= 0 || p.height <= 0 ||
                                p.width != d.width || p.height != d.height || p.x < 1 || p.y < 1 ||
                                (long)p.x + p.width >= page.Width || (long)p.y + p.height >= page.Height ||
                                d.x < 0 || d.y < 0 || (long)d.x + d.width > data.frameWidth ||
                                (long)d.y + d.height > data.frameHeight)
                                throw new InvalidDataException("Invalid RGBA replacement patch geometry.");
                            foreach (RectJson prior in physical)
                                if (p.x < (long)prior.x + prior.width && prior.x < (long)p.x + p.width &&
                                    p.y < (long)prior.y + prior.height && prior.y < (long)p.y + p.height &&
                                    !(p.x == prior.x && p.y == prior.y && p.width == prior.width && p.height == prior.height && prior != chunk.rects[0]))
                                    throw new InvalidDataException("RGBA replacement blocks overlap.");
                            physical.Add(p);
                        }
                    }
                    usedPages.Add(page.Id);
                    chunks.Add(new Chunk(page, chunk, data.frameWidth, data.frameHeight));
                }
                if (next != capacity) throw new InvalidDataException("Derived manifest omits source cells.");
                sources.Add(path, new Source(data, path, chunks));
            }
            if (usedPages.Count != pages.Count)
                throw new InvalidDataException("Derived manifest contains unused pages.");
            var result = new BattleDerivedSpritePages(manifestPath, Hash(bytes), pages, sources, datHashes);
            foreach (var config in configs.Values)
                foreach (SpriteFileInfo file in config.characterData.files)
                    if (result.TryGetSource(file.filePath, out Source mapped))
                        mapped.RequireBinding(config.characterId, file);
            return result;
        }

        internal bool TryGetSource(string path, out Source source)
        {
            return sources.TryGetValue(Path.GetFullPath(path), out source);
        }

        internal bool IsPagePath(string path)
        {
            return pagePaths.Contains(Path.GetFullPath(path));
        }

        internal void AssertInputsCurrent()
        {
            if (!File.Exists(manifestPath) || !EqualHash(HashFile(manifestPath), Fingerprint))
                throw new InvalidDataException("Derived page manifest changed after capture.");
            foreach (var pair in datHashes)
                if (!EqualHash(HashFile(pair.Key), pair.Value))
                    throw new InvalidDataException("Derived DAT input changed after capture.");
        }

        internal static string CurrentManifestFingerprint(BattleContentSource source)
        {
            string path = Path.Combine(source.RuntimeRoot, RelativeManifestPath);
            return File.Exists(path) ? HashFile(path) : null;
        }

        private static string BindingKey(int oid, int first, int last) => oid + ":" + first + ":" + last;

        private static string ResolveWithin(string root, string relative)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) ||
                relative.IndexOf(':') >= 0 || relative.Replace('\\', '/').Split('/').Any(part => part == ".." || part == "."))
                throw new InvalidDataException("Derived page paths must be relative and contained.");
            string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string path = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            if (!path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Derived page path escapes its owner.");
            return path;
        }

        private static bool EqualHash(string left, string right) =>
            !string.IsNullOrEmpty(right) && right.Length == 64 &&
            string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

        private static string HashFile(string path) => Hash(File.ReadAllBytes(path));

        private static string Hash(byte[] bytes)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        [Serializable] internal sealed class ManifestJson
        {
            public int version;
            public string generatorVersion;
            public PageJson[] pages;
            public SourceJson[] sources;
        }
        [Serializable] internal sealed class PageJson
        {
            public string id, path, sha256;
            public int width, height;
        }
        [Serializable] internal sealed class SourceJson
        {
            public string path, sha256;
            public int width, height, frameWidth, frameHeight, columns, rows;
            public bool clampBaked;
            public BindingJson[] bindings;
            public ChunkJson[] chunks;
        }
        [Serializable] internal sealed class BindingJson
        {
            public int oid, firstPic, lastPic;
            public string datPath, datSha256;
        }
        [Serializable] internal sealed class ChunkJson
        {
            public string page;
            public int firstLocalPic, count, columns, rows;
            public bool allowExactFrameAliases;
            public RectJson[] rects, contentRects;
            public PatchJson[] replacementPatches;
        }
        [Serializable] internal sealed class PatchJson
        {
            public RectJson source, destination;
        }
        [Serializable] internal sealed class RectJson
        {
            public int x, y, width, height;
        }
    }
}

