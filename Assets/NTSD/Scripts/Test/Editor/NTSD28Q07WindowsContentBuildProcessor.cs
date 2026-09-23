#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q07WindowsContentBuildProcessor : IPostprocessBuildWithReport
    {
        private const string ContentRelativeRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string ManifestRelativePath =
            "artifacts/diagnostics/NTSD28-Q07-WINDOWS-PLAYER-MANIFEST-V2-001/copy-manifest-v2.csv";
        private const int ExpectedFileCount = 1371;
        private const long ExpectedBytes = 46883057;

        public int callbackOrder => 1000;

        public void OnPostprocessBuild(BuildReport report)
        {
            Debug.Log("[NTSD28 Q07] Postprocess entered: platform=" + report.summary.platform +
                ", result=" + report.summary.result + ", output=" + report.summary.outputPath);
            if (report.summary.platform != BuildTarget.StandaloneWindows64 &&
                report.summary.platform != BuildTarget.StandaloneWindows)
                return;

            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string sourceRoot = Path.Combine(projectRoot, "Assets", "NTSD", "Content", "LoganRuntime");
            string outputPath = Path.GetFullPath(report.summary.outputPath);
            string outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrEmpty(outputDirectory) || !File.Exists(outputPath))
                throw new InvalidOperationException("Q07 Windows content packaging requires a built Player EXE.");

            string destinationRoot = Path.Combine(outputDirectory, "Assets", "NTSD", "Content", "LoganRuntime");
            var entries = ReadManifest(Path.Combine(projectRoot, ManifestRelativePath));
            VerifySourceSet(sourceRoot, entries);
            Directory.CreateDirectory(destinationRoot);
            foreach (var entry in entries)
            {
                string source = ResolveWithin(sourceRoot, entry.RelativePath);
                VerifyFile(source, entry.Bytes, entry.Sha256);
                string destination = ResolveWithin(destinationRoot, entry.RelativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                if (!File.Exists(destination)) File.Copy(source, destination);
                VerifyFile(destination, entry.Bytes, entry.Sha256);
            }
            VerifyDestinationSet(destinationRoot, entries);

            string sparkSource = Path.Combine(projectRoot, "Assets", "NTSD", "Sprite", "UIPanels", "SPARK.bmp");
            if (!File.Exists(sparkSource))
                throw new FileNotFoundException("Q07 Player needs the existing raw battle SPARK.bmp.", sparkSource);
            string dataDirectory = Path.Combine(outputDirectory,
                Path.GetFileNameWithoutExtension(outputPath) + "_Data");
            if (!Directory.Exists(dataDirectory))
                throw new DirectoryNotFoundException("Q07 Player data directory is missing: " + dataDirectory);
            string sparkDestination = Path.Combine(dataDirectory, "NTSD", "Sprite", "UIPanels", "SPARK.bmp");
            Directory.CreateDirectory(Path.GetDirectoryName(sparkDestination));
            long sparkBytes = new FileInfo(sparkSource).Length;
            string sparkHash = Sha256(sparkSource);
            if (!File.Exists(sparkDestination)) File.Copy(sparkSource, sparkDestination);
            VerifyFile(sparkDestination, sparkBytes, sparkHash);

            Debug.Log("[NTSD28 Q07] Windows Player content packaged: " + entries.Count +
                " files, " + ExpectedBytes + " bytes, plus existing SPARK.bmp; output=" + destinationRoot);
        }

        private static List<ManifestEntry> ReadManifest(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("Q07 content manifest is missing.", path);
            string[] lines = File.ReadAllLines(path);
            if (lines.Length != ExpectedFileCount + 1 ||
                lines[0].TrimStart('\uFEFF') != "role,sourceRelative,targetRelative,bytes,sha256")
                throw new InvalidDataException("Q07 manifest header or file count changed.");

            var entries = new List<ManifestEntry>(ExpectedFileCount);
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            long totalBytes = 0;
            for (int index = 1; index < lines.Length; index++)
            {
                string[] fields = lines[index].Split(',');
                if (fields.Length != 5 || !long.TryParse(fields[3], out long bytes) || bytes < 0 ||
                    fields[4].Length != 64 ||
                    fields[2] != ContentRelativeRoot + "/" + fields[1] ||
                    !names.Add(fields[1]))
                    throw new InvalidDataException("Q07 manifest row is invalid or duplicated: " + index);
                entries.Add(new ManifestEntry(fields[1], bytes, fields[4]));
                totalBytes = checked(totalBytes + bytes);
            }
            if (totalBytes != ExpectedBytes)
                throw new InvalidDataException("Q07 manifest total bytes changed: " + totalBytes);
            return entries;
        }

        private static void VerifySourceSet(string root, List<ManifestEntry> entries)
        {
            if (!Directory.Exists(root)) throw new DirectoryNotFoundException(root);
            var expected = new HashSet<string>(entries.Select(value => value.RelativePath),
                StringComparer.OrdinalIgnoreCase);
            var actual = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Where(path => !path.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
                .Select(path => RelativePath(root, path)).ToArray();
            if (actual.Length != expected.Count || actual.Any(path => !expected.Contains(path)))
                throw new InvalidDataException("Q07 staged source contains missing or extra non-meta files.");
        }

        private static void VerifyDestinationSet(string root, List<ManifestEntry> entries)
        {
            var expected = new HashSet<string>(entries.Select(value => value.RelativePath),
                StringComparer.OrdinalIgnoreCase);
            var actual = Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                .Select(path => RelativePath(root, path)).ToArray();
            if (actual.Length != expected.Count || actual.Any(path => !expected.Contains(path)))
                throw new InvalidDataException("Q07 Player output contains missing or extra formal-content files.");
        }

        private static string RelativePath(string root, string path)
        {
            return path.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                .Replace('\\', '/');
        }

        private static string ResolveWithin(string root, string relative)
        {
            if (string.IsNullOrWhiteSpace(relative) || Path.IsPathRooted(relative) || relative.IndexOf(':') >= 0)
                throw new InvalidDataException("Q07 manifest path is not relative: " + relative);
            string full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
            string prefix = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Q07 manifest path escapes its content root: " + relative);
            return full;
        }

        private static void VerifyFile(string path, long bytes, string sha256)
        {
            if (!File.Exists(path) || new FileInfo(path).Length != bytes ||
                !string.Equals(Sha256(path), sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Q07 Player content differs from the formal manifest: " + path);
        }

        private static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var algorithm = SHA256.Create())
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", string.Empty);
        }

        private readonly struct ManifestEntry
        {
            public readonly string RelativePath;
            public readonly long Bytes;
            public readonly string Sha256;

            public ManifestEntry(string relativePath, long bytes, string sha256)
            {
                RelativePath = relativePath;
                Bytes = bytes;
                Sha256 = sha256;
            }
        }
    }
}
#endif
