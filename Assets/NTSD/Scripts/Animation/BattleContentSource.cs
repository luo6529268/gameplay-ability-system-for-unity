using System;
using System.IO;

namespace NTSD.Animation
{
    /// <summary>
    /// Immutable load-time paths. Selecting a source does not publish content or change a cache.
    /// </summary>
    public sealed class BattleContentSource
    {
        public bool IsLoganRuntime { get; }
        public string ProjectRoot { get; }
        public string RuntimeRoot { get; }
        public string CatalogPath { get; }
        public string DataIndexPath { get; }
        public string DatRoot { get; }
        /// <summary>
        /// VFS root for Logan; containing project for legacy Unity paths.
        /// Call ResolveImagePath to retain the legacy per-DAT base.
        /// </summary>
        public string ImageRoot { get; }

        private BattleContentSource(string root, bool loganRuntime)
        {
            if (string.IsNullOrWhiteSpace(root))
                throw new ArgumentException("A content root is required.", nameof(root));

            string absoluteRoot = Path.GetFullPath(root);
            IsLoganRuntime = loganRuntime;
            if (loganRuntime)
            {
                // Alignment contract: NTSD28-B11-CONTENT-SOURCE-PATH-CONTRACT-001
                RuntimeRoot = absoluteRoot;
                CatalogPath = Path.Combine(absoluteRoot, "catalog.csv");
                DatRoot = Path.Combine(absoluteRoot, "decoded_dat");
                ImageRoot = Path.Combine(absoluteRoot, "vfs");
                DataIndexPath = Path.Combine(DatRoot, "data", "data.txt");
            }
            else
            {
                ProjectRoot = absoluteRoot;
                DatRoot = Path.Combine(absoluteRoot, "Assets", "NTSD", "Config");
                ImageRoot = absoluteRoot;
                DataIndexPath = Path.Combine(DatRoot, "data.txt");
            }
        }

        /// <summary>Describes the formal unified portable runtime, not alternate extracted layouts.</summary>
        public static BattleContentSource ForLoganRuntime(string runtimeRoot)
        {
            return new BattleContentSource(runtimeRoot, true);
        }

        public static BattleContentSource ForUnityProject(string projectRoot)
        {
            return new BattleContentSource(projectRoot, false);
        }

        public string ResolveDatPath(string sourcePath)
        {
            string relative = NormalizeKey(sourcePath);
            string root = ProjectRoot != null && IsAssetsPath(relative) ? ProjectRoot : DatRoot;
            return ResolveWithin(root, root, relative);
        }

        public string ResolvePublishedFolderPath(string publishedFolder)
        {
            if (!IsLoganRuntime)
                throw new InvalidOperationException("Published folders belong to a Logan runtime source.");
            return ResolveWithin(RuntimeRoot, RuntimeRoot, NormalizeKey(publishedFolder));
        }

        public string ResolveImagePath(string sourcePath, string legacyDatDirectory)
        {
            if (string.IsNullOrEmpty(sourcePath))
                return string.Empty;

            string relative = NormalizeKey(sourcePath);
            if (IsLoganRuntime)
                return ResolveWithin(ImageRoot, ImageRoot, relative);
            if (IsAssetsPath(relative))
                return ResolveWithin(ProjectRoot, ProjectRoot, relative);
            if (string.IsNullOrWhiteSpace(legacyDatDirectory) || !Path.IsPathRooted(legacyDatDirectory))
                throw new ArgumentException("Legacy relative images require their absolute DAT directory.", nameof(legacyDatDirectory));

            // Legacy DAT paths may reach sibling asset folders inside the project.
            return ResolveWithin(ProjectRoot, legacyDatDirectory, relative);
        }

        private static bool IsAssetsPath(string path)
        {
            return path.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("A relative content key is required.", nameof(key));
            string normalized = key.Replace('\\', '/');
            if (Path.IsPathRooted(normalized) || normalized.IndexOf(':') >= 0)
                throw new ArgumentException("Content keys must be relative to their source root.", nameof(key));
            return normalized;
        }

        private static string ResolveWithin(string allowedRoot, string baseDirectory, string key)
        {
            string resolved = Path.GetFullPath(Path.Combine(baseDirectory, key));
            string prefix = allowedRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            StringComparison comparison = Path.DirectorySeparatorChar == '\\'
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;
            if (!resolved.StartsWith(prefix, comparison))
                throw new ArgumentException("Content key escapes its source root.", nameof(key));
            return resolved;
        }
    }
}
