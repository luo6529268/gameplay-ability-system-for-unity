using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace NTSD.Animation
{
    public sealed class BattleAtlasSourcePixels
    {
        public BattleAtlasSourcePixels(string path, int width, int height, Color32[] pixels)
        {
            Path = path ?? string.Empty;
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public string Path { get; }
        public int Width { get; }
        public int Height { get; }
        public Color32[] Pixels { get; }
    }

    public readonly struct BattleAtlasArrayDecision
    {
        public BattleAtlasArrayDecision(bool useTextureArray, bool useCpuUpload, string reason)
        {
            UseTextureArray = useTextureArray;
            UseCpuUpload = useCpuUpload;
            Reason = reason ?? string.Empty;
        }

        public bool UseTextureArray { get; }
        public bool UseCpuUpload { get; }
        public string Reason { get; }
    }

    public sealed class BattleAtlasCapabilityPolicy
    {
        private readonly Func<long, bool> allocationGuard;
        private readonly string forcedOrderedPagesReason;

        public BattleAtlasCapabilityPolicy(
            bool supports2DArrayTextures,
            int maxTextureSize,
            int maxTextureArraySlices,
            bool supportsSampleFormat,
            bool supportsCopyTexture,
            long arrayBudgetBytes,
            Func<long, bool> allocationGuard = null,
            string forcedOrderedPagesReason = null)
        {
            Supports2DArrayTextures = supports2DArrayTextures;
            MaxTextureSize = maxTextureSize;
            MaxTextureArraySlices = maxTextureArraySlices;
            SupportsSampleFormat = supportsSampleFormat;
            SupportsCopyTexture = supportsCopyTexture;
            ArrayBudgetBytes = arrayBudgetBytes;
            this.allocationGuard = allocationGuard;
            this.forcedOrderedPagesReason = forcedOrderedPagesReason ?? string.Empty;
        }

        public bool Supports2DArrayTextures { get; }
        public int MaxTextureSize { get; }
        public int MaxTextureArraySlices { get; }
        public bool SupportsSampleFormat { get; }
        public bool SupportsCopyTexture { get; }
        public long ArrayBudgetBytes { get; }

        public BattleAtlasArrayDecision EvaluateArray(int pageCount)
        {
            long byteCount = checked((long)pageCount * BattleAtlasLayoutPlanner.PageSize *
                                     BattleAtlasLayoutPlanner.PageSize * 4L);
            if (!string.IsNullOrEmpty(forcedOrderedPagesReason))
                return new BattleAtlasArrayDecision(false, true, forcedOrderedPagesReason);
            if (!Supports2DArrayTextures)
                return new BattleAtlasArrayDecision(false, true, "Texture2DArray sampling is unsupported.");
            if (MaxTextureSize < BattleAtlasLayoutPlanner.PageSize)
                return new BattleAtlasArrayDecision(false, true, $"Maximum texture size {MaxTextureSize} is below 2048.");
            if (pageCount > MaxTextureArraySlices)
                return new BattleAtlasArrayDecision(false, true, $"Atlas requires {pageCount} slices but the device limit is {MaxTextureArraySlices}.");
            if (!SupportsSampleFormat)
                return new BattleAtlasArrayDecision(false, true, "RGBA32 sampling is unsupported.");
            if (ArrayBudgetBytes >= 0 && byteCount > ArrayBudgetBytes)
                return new BattleAtlasArrayDecision(false, true, $"Atlas array allocation {byteCount} bytes exceeds budget {ArrayBudgetBytes}.");
            if (allocationGuard != null && !allocationGuard(byteCount))
                return new BattleAtlasArrayDecision(false, true, "Atlas array allocation guard rejected the request.");

            // CopyTexture support only chooses a potential upload optimization.
            // CPU assembly remains the formal first path and does not disable arrays.
            return new BattleAtlasArrayDecision(true, true, SupportsCopyTexture
                ? string.Empty
                : "CopyTexture is unavailable; using CPU page upload.");
        }

        public static BattleAtlasCapabilityPolicy FromSystem(long arrayBudgetBytes = 256L * 1024L * 1024L)
        {
            return new BattleAtlasCapabilityPolicy(
                SystemInfo.supports2DArrayTextures,
                SystemInfo.maxTextureSize,
                SystemInfo.maxTextureArraySlices,
                SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32),
                SystemInfo.copyTextureSupport != UnityEngine.Rendering.CopyTextureSupport.None,
                arrayBudgetBytes);
        }
    }

    public sealed class BattleAtlasResources
    {
        private readonly IReadOnlyList<Texture2D> pages;
        private readonly IReadOnlyCollection<UnityEngine.Object> ownedObjects;

        internal BattleAtlasResources(
            BattleSpriteCentralBindingMode mode,
            Texture2DArray textureArray,
            List<Texture2D> pageTextures,
            List<UnityEngine.Object> resources,
            string diagnostic)
        {
            Mode = mode;
            TextureArray = textureArray;
            pages = new ReadOnlyCollection<Texture2D>(pageTextures ?? new List<Texture2D>());
            ownedObjects = new ReadOnlyCollection<UnityEngine.Object>(resources ?? new List<UnityEngine.Object>());
            Diagnostic = diagnostic ?? string.Empty;
        }

        public BattleSpriteCentralBindingMode Mode { get; }
        public Texture2DArray TextureArray { get; }
        public IReadOnlyList<Texture2D> Pages => pages;
        public IReadOnlyCollection<UnityEngine.Object> OwnedObjects => ownedObjects;
        public string Diagnostic { get; }

        public Texture ResolveTexture(int pageIndex)
        {
            if (Mode == BattleSpriteCentralBindingMode.AtlasTextureArray)
                return TextureArray;
            return (uint)pageIndex < (uint)pages.Count ? pages[pageIndex] : null;
        }
    }

    public static class BattleAtlasResourceBuilder
    {
        public static bool TryBuild(
            BattleAtlasPlan plan,
            IEnumerable<BattleAtlasSourcePixels> sources,
            BattleAtlasCapabilityPolicy policy,
            out BattleAtlasResources resources,
            out string diagnostic)
        {
            return TryBuild(plan, sources, policy, null, out resources, out diagnostic);
        }

        internal static bool TryBuild(
            BattleAtlasPlan plan,
            IEnumerable<BattleAtlasSourcePixels> sources,
            BattleAtlasCapabilityPolicy policy,
            Action<Texture2D, int> beforeFallbackUpload,
            out BattleAtlasResources resources,
            out string diagnostic)
        {
            resources = null;
            diagnostic = string.Empty;
            if (plan == null)
            {
                diagnostic = "Atlas plan is null.";
                return false;
            }
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            if (!TryIndexSources(sources, out Dictionary<string, BattleAtlasSourcePixels> indexed, out diagnostic))
                return false;
            for (int index = 0; index < plan.Placements.Count; index++)
            {
                BattleAtlasPlacement placement = plan.Placements[index];
                if (!indexed.TryGetValue(placement.NormalizedPath, out BattleAtlasSourcePixels source))
                {
                    diagnostic = $"Missing decoded atlas source '{placement.NormalizedPath}'.";
                    return false;
                }
                if (source.Width != placement.ContentRect.width || source.Height != placement.ContentRect.height ||
                    source.Pixels == null || source.Pixels.Length != checked(source.Width * source.Height))
                {
                    diagnostic = $"Decoded atlas source '{placement.NormalizedPath}' does not match the completed layout.";
                    return false;
                }
            }

            Color32[][] pages;
            try
            {
                pages = AssemblePages(plan, indexed);
            }
            catch (Exception exception)
            {
                diagnostic = $"Atlas CPU assembly failed: {exception.Message}";
                return false;
            }

            BattleAtlasArrayDecision decision = policy.EvaluateArray(plan.PageCount);
            if (decision.UseTextureArray)
            {
                Texture2DArray array = null;
                try
                {
                    array = new Texture2DArray(
                        BattleAtlasLayoutPlanner.PageSize,
                        BattleAtlasLayoutPlanner.PageSize,
                        plan.PageCount,
                        TextureFormat.RGBA32,
                        false,
                        false)
                    {
                        name = "NTSD Battle Atlas Array",
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                    };
                    for (int page = 0; page < pages.Length; page++)
                        array.SetPixels32(pages[page], page, 0);
                    array.Apply(false, true);
                    resources = new BattleAtlasResources(
                        BattleSpriteCentralBindingMode.AtlasTextureArray,
                        array,
                        null,
                        new List<UnityEngine.Object> { array },
                        decision.Reason);
                    return true;
                }
                catch (Exception exception)
                {
                    DestroyObject(array);
                    diagnostic = $"Texture2DArray allocation/upload failed ({exception.Message}); using ordered page fallback.";
                }
            }
            else
            {
                diagnostic = decision.Reason;
            }

            var pageTextures = new List<Texture2D>(plan.PageCount);
            var owned = new List<UnityEngine.Object>(plan.PageCount);
            try
            {
                for (int page = 0; page < pages.Length; page++)
                {
                    var texture = new Texture2D(
                        BattleAtlasLayoutPlanner.PageSize,
                        BattleAtlasLayoutPlanner.PageSize,
                        TextureFormat.RGBA32,
                        false,
                        false)
                    {
                        name = $"NTSD Battle Atlas Page {page}",
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                    };
                    owned.Add(texture);
                    beforeFallbackUpload?.Invoke(texture, page);
                    texture.SetPixels32(pages[page]);
                    texture.Apply(false, true);
                    pageTextures.Add(texture);
                }
                resources = new BattleAtlasResources(
                    BattleSpriteCentralBindingMode.AtlasPageTexture2D,
                    null,
                    pageTextures,
                    owned,
                    diagnostic);
                return true;
            }
            catch (Exception exception)
            {
                for (int index = 0; index < owned.Count; index++)
                    DestroyObject(owned[index]);
                resources = null;
                diagnostic = $"Ordered atlas page fallback failed: {exception.Message}";
                return false;
            }
        }

        public static bool TryBindCatalog(
            BattleSpriteCatalog sourceCatalog,
            BattleAtlasPlan plan,
            BattleAtlasResources resources,
            out BattleSpriteCatalog boundCatalog,
            out string diagnostic)
        {
            return TryBindCatalog(
                sourceCatalog,
                plan,
                resources,
                null,
                out boundCatalog,
                out diagnostic);
        }

        /// <summary>
        /// Binds planned sources to atlas resources while retaining only the explicitly
        /// excluded sources as their original Texture2D bindings. This is intentionally
        /// fail-closed for every other missing placement.
        /// </summary>
        public static bool TryBindCatalog(
            BattleSpriteCatalog sourceCatalog,
            BattleAtlasPlan plan,
            BattleAtlasResources resources,
            IEnumerable<string> sourceTexture2DExcludedPaths,
            out BattleSpriteCatalog boundCatalog,
            out string diagnostic)
        {
            boundCatalog = null;
            diagnostic = string.Empty;
            if (sourceCatalog == null || plan == null || resources == null)
            {
                diagnostic = "Catalog, atlas plan, and atlas resources are required.";
                return false;
            }

            var excludedPaths = new HashSet<string>(StringComparer.Ordinal);
            if (sourceTexture2DExcludedPaths != null)
            {
                foreach (string path in sourceTexture2DExcludedPaths)
                    excludedPaths.Add(BattleAtlasLayoutPlanner.NormalizePath(path));
            }

            var bindings = new Dictionary<BattleSpriteKey, BattleSpriteCentralBinding>();
            foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in sourceCatalog.Entries)
            {
                BattleSpriteEntry entry = pair.Value;
                if (!plan.TryGetPlacement(entry.SourceSheetPath, out BattleAtlasPlacement placement))
                {
                    string normalizedPath = BattleAtlasLayoutPlanner.NormalizePath(entry.SourceSheetPath);
                    if (excludedPaths.Contains(normalizedPath) &&
                        entry.CentralBinding.Mode == BattleSpriteCentralBindingMode.SourceTexture2D &&
                        entry.CentralBinding.IsValid)
                    {
                        bindings.Add(pair.Key, entry.CentralBinding);
                        continue;
                    }
                    diagnostic = $"Catalog entry {pair.Key} references missing atlas source '{entry.SourceSheetPath}'.";
                    return false;
                }

                Rect sourceRect = entry.PixelRect;
                var atlasRect = new Rect(
                    placement.ContentRect.x + sourceRect.x,
                    placement.ContentRect.y + sourceRect.y,
                    sourceRect.width,
                    sourceRect.height);
                float pageSize = plan.PageSize;
                var uv = new Rect(
                    atlasRect.x / pageSize,
                    atlasRect.y / pageSize,
                    atlasRect.width / pageSize,
                    atlasRect.height / pageSize);
                Texture texture = resources.ResolveTexture(placement.PageIndex);
                var binding = new BattleSpriteCentralBinding(
                    resources.Mode,
                    texture,
                    resources.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray
                        ? placement.PageIndex
                        : 0,
                    uv,
                    atlasRect);
                if (!binding.IsValid)
                {
                    diagnostic = $"Catalog entry {pair.Key} produced an invalid atlas binding.";
                    return false;
                }
                bindings.Add(pair.Key, binding);
            }

            try
            {
                boundCatalog = sourceCatalog.WithCentralBindings(bindings);
                return true;
            }
            catch (Exception exception)
            {
                diagnostic = exception.Message;
                return false;
            }
        }

        internal static Color32[][] AssemblePages(
            BattleAtlasPlan plan,
            IReadOnlyDictionary<string, BattleAtlasSourcePixels> indexedSources)
        {
            var pages = new Color32[plan.PageCount][];
            int pixelsPerPage = checked(plan.PageSize * plan.PageSize);
            for (int page = 0; page < pages.Length; page++)
                pages[page] = new Color32[pixelsPerPage];

            for (int index = 0; index < plan.Placements.Count; index++)
            {
                BattleAtlasPlacement placement = plan.Placements[index];
                BattleAtlasSourcePixels source = indexedSources[placement.NormalizedPath];
                CopyWithExtrusion(
                    source.Pixels,
                    source.Width,
                    source.Height,
                    pages[placement.PageIndex],
                    plan.PageSize,
                    placement.ContentRect,
                    plan.Padding);
            }
            return pages;
        }

        internal static bool TryValidateSourceSet(
            IEnumerable<BattleAtlasSourcePixels> sources,
            out string diagnostic)
        {
            return TryIndexSources(sources, out _, out diagnostic);
        }

        private static bool TryIndexSources(
            IEnumerable<BattleAtlasSourcePixels> sources,
            out Dictionary<string, BattleAtlasSourcePixels> indexed,
            out string diagnostic)
        {
            indexed = new Dictionary<string, BattleAtlasSourcePixels>(StringComparer.Ordinal);
            diagnostic = string.Empty;
            if (sources == null)
            {
                diagnostic = "Decoded atlas source list is null.";
                return false;
            }

            foreach (BattleAtlasSourcePixels source in sources)
            {
                if (source == null)
                    continue;
                string path = BattleAtlasLayoutPlanner.NormalizePath(source.Path);
                if (indexed.TryGetValue(path, out BattleAtlasSourcePixels prior))
                {
                    if (prior.Width != source.Width || prior.Height != source.Height ||
                        !HasValidPixels(prior) || !HasValidPixels(source) ||
                        !PixelsEqual(prior.Pixels, source.Pixels))
                    {
                        diagnostic = $"Conflicting decoded atlas source '{path}'.";
                        return false;
                    }
                    continue;
                }
                indexed.Add(path, source);
            }

            foreach (KeyValuePair<string, BattleAtlasSourcePixels> pair in indexed)
            {
                if (!HasValidPixels(pair.Value))
                {
                    diagnostic = $"Decoded atlas source '{pair.Key}' has invalid pixels or dimensions.";
                    return false;
                }
            }
            return true;
        }

        private static bool HasValidPixels(BattleAtlasSourcePixels source)
        {
            if (source == null || source.Width <= 0 || source.Height <= 0 || source.Pixels == null)
                return false;
            long expectedLength = (long)source.Width * source.Height;
            return expectedLength <= int.MaxValue && source.Pixels.Length == expectedLength;
        }

        private static bool PixelsEqual(Color32[] left, Color32[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
                return false;
            for (int index = 0; index < left.Length; index++)
            {
                if (!left[index].Equals(right[index]))
                    return false;
            }
            return true;
        }

        private static void CopyWithExtrusion(
            Color32[] source,
            int sourceWidth,
            int sourceHeight,
            Color32[] destination,
            int destinationWidth,
            RectInt contentRect,
            int padding)
        {
            for (int y = -padding; y < sourceHeight + padding; y++)
            {
                int sourceY = Mathf.Clamp(y, 0, sourceHeight - 1);
                int destinationY = contentRect.y + y;
                int destinationRow = destinationY * destinationWidth;
                int sourceRow = sourceY * sourceWidth;
                for (int x = -padding; x < sourceWidth + padding; x++)
                {
                    int sourceX = Mathf.Clamp(x, 0, sourceWidth - 1);
                    destination[destinationRow + contentRect.x + x] = source[sourceRow + sourceX];
                }
            }
        }

        private static void DestroyObject(UnityEngine.Object value)
        {
            if (value == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(value);
            else
                UnityEngine.Object.DestroyImmediate(value);
        }
    }
}
