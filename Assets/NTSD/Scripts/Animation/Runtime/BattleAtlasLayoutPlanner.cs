using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using UnityEngine;

namespace NTSD.Animation
{
    public readonly struct BattleAtlasSheetDescriptor
    {
        public BattleAtlasSheetDescriptor(string path, int width, int height)
        {
            Path = path ?? string.Empty;
            Width = width;
            Height = height;
        }

        public string Path { get; }
        public int Width { get; }
        public int Height { get; }
    }

    public readonly struct BattleAtlasPlacement
    {
        public BattleAtlasPlacement(
            string normalizedPath,
            int pageIndex,
            RectInt allocatedRect,
            RectInt contentRect)
        {
            NormalizedPath = normalizedPath;
            PageIndex = pageIndex;
            AllocatedRect = allocatedRect;
            ContentRect = contentRect;
        }

        public string NormalizedPath { get; }
        public int PageIndex { get; }
        public RectInt AllocatedRect { get; }
        public RectInt ContentRect { get; }
    }

    public sealed class BattleAtlasPlan
    {
        private readonly IReadOnlyList<BattleAtlasPlacement> placements;
        private readonly IReadOnlyDictionary<string, BattleAtlasPlacement> placementsByPath;

        internal BattleAtlasPlan(
            int pageSize,
            int padding,
            List<BattleAtlasPlacement> sourcePlacements)
        {
            PageSize = pageSize;
            Padding = padding;
            var placementCopy = new List<BattleAtlasPlacement>(sourcePlacements);
            placements = new ReadOnlyCollection<BattleAtlasPlacement>(placementCopy);
            var byPath = new Dictionary<string, BattleAtlasPlacement>(StringComparer.Ordinal);
            int maximumPage = -1;
            for (int index = 0; index < placementCopy.Count; index++)
            {
                BattleAtlasPlacement placement = placementCopy[index];
                byPath.Add(placement.NormalizedPath, placement);
                maximumPage = Math.Max(maximumPage, placement.PageIndex);
            }
            placementsByPath = new ReadOnlyDictionary<string, BattleAtlasPlacement>(byPath);
            PageCount = maximumPage + 1;
        }

        public int PageSize { get; }
        public int Padding { get; }
        public int PageCount { get; }
        public IReadOnlyList<BattleAtlasPlacement> Placements => placements;

        public bool TryGetPlacement(string path, out BattleAtlasPlacement placement)
        {
            return placementsByPath.TryGetValue(BattleAtlasLayoutPlanner.NormalizePath(path), out placement);
        }
    }

    public sealed class BattleAtlasPlanResult
    {
        internal BattleAtlasPlanResult(BattleAtlasPlan plan, string diagnostic)
        {
            Plan = plan;
            Diagnostic = diagnostic ?? string.Empty;
        }

        public BattleAtlasPlan Plan { get; }
        public string Diagnostic { get; }
        public bool Succeeded => Plan != null;
    }

    public static class BattleAtlasLayoutPlanner
    {
        public const int PageSize = 2048;
        public const int ExtrusionPadding = 1;
        public const int MaximumContentSize = PageSize - ExtrusionPadding * 2;

        public static bool IsPageEligible(int width, int height)
        {
            return width > 0 && height > 0 && width <= MaximumContentSize && height <= MaximumContentSize;
        }

        public static BattleAtlasPlanResult Plan(IEnumerable<BattleAtlasSheetDescriptor> sources)
        {
            if (sources == null)
                return new BattleAtlasPlanResult(null, "Atlas source list is null.");

            var unique = new Dictionary<string, BattleAtlasSheetDescriptor>(StringComparer.Ordinal);
            foreach (BattleAtlasSheetDescriptor source in sources)
            {
                string normalizedPath;
                try
                {
                    normalizedPath = NormalizePath(source.Path);
                }
                catch (Exception exception)
                {
                    return new BattleAtlasPlanResult(null, $"Invalid atlas source path '{source.Path}': {exception.Message}");
                }

                if (string.IsNullOrEmpty(normalizedPath))
                    return new BattleAtlasPlanResult(null, "Atlas source path is empty.");
                if (source.Width <= 0 || source.Height <= 0)
                    return new BattleAtlasPlanResult(null, $"Atlas source '{normalizedPath}' has invalid size {source.Width}x{source.Height}.");

                var normalized = new BattleAtlasSheetDescriptor(normalizedPath, source.Width, source.Height);
                if (unique.TryGetValue(normalizedPath, out BattleAtlasSheetDescriptor prior))
                {
                    if (prior.Width != source.Width || prior.Height != source.Height)
                    {
                        return new BattleAtlasPlanResult(
                            null,
                            $"Conflicting atlas source '{normalizedPath}': {prior.Width}x{prior.Height} versus {source.Width}x{source.Height}.");
                    }
                    continue;
                }
                unique.Add(normalizedPath, normalized);
            }

            var ordered = new List<BattleAtlasSheetDescriptor>(unique.Values);
            ordered.Sort((left, right) => StringComparer.Ordinal.Compare(left.Path, right.Path));
            var placements = new List<BattleAtlasPlacement>(ordered.Count);
            int pageIndex = 0;
            int cursorX = 0;
            int cursorY = 0;
            int shelfHeight = 0;

            for (int index = 0; index < ordered.Count; index++)
            {
                BattleAtlasSheetDescriptor source = ordered[index];
                int allocatedWidth = checked(source.Width + ExtrusionPadding * 2);
                int allocatedHeight = checked(source.Height + ExtrusionPadding * 2);
                if (!IsPageEligible(source.Width, source.Height))
                {
                    return new BattleAtlasPlanResult(
                        null,
                        $"Oversized atlas source '{source.Path}' is {source.Width}x{source.Height}; maximum content size with {ExtrusionPadding}px extrusion is {MaximumContentSize}x{MaximumContentSize}.");
                }

                if (cursorX + allocatedWidth > PageSize)
                {
                    cursorX = 0;
                    cursorY += shelfHeight;
                    shelfHeight = 0;
                }
                if (cursorY + allocatedHeight > PageSize)
                {
                    pageIndex++;
                    cursorX = 0;
                    cursorY = 0;
                    shelfHeight = 0;
                }

                var allocated = new RectInt(cursorX, cursorY, allocatedWidth, allocatedHeight);
                var content = new RectInt(
                    cursorX + ExtrusionPadding,
                    cursorY + ExtrusionPadding,
                    source.Width,
                    source.Height);
                placements.Add(new BattleAtlasPlacement(source.Path, pageIndex, allocated, content));
                cursorX += allocatedWidth;
                shelfHeight = Math.Max(shelfHeight, allocatedHeight);
            }

            return new BattleAtlasPlanResult(
                new BattleAtlasPlan(PageSize, ExtrusionPadding, placements),
                string.Empty);
        }

        public static string NormalizePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return string.Empty;
            return Path.GetFullPath(path.Trim()).Replace('\\', '/');
        }
    }
}
