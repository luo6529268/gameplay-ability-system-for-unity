using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Simulation;

namespace NTSD.EditorTools
{
    internal static class NTSD28TraceContentIdentity
    {
        internal const string LegacyTag = "NTSD28_UNITY_LEGACY_DAT_SEMANTICS_V1";

        internal static Dictionary<string, object> FromLoganRaw(string raw)
        {
            return Build("logan-runtime", LoganContentIdentity.FromDefinitionFingerprint(raw));
        }

        internal static Dictionary<string, object> CaptureLegacy(string configRoot, string indexPath)
        {
            string root = Path.GetFullPath(configRoot);
            var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
                .Where(path => string.Equals(Path.GetExtension(path), ".dat", StringComparison.OrdinalIgnoreCase))
                .Select(path => new { Path = path, Relative = Path.GetRelativePath(root, path).Replace('\\', '/') })
                .OrderBy(file => file.Relative, StringComparer.Ordinal);
            using var bytes = new MemoryStream();
            using (var writer = new BinaryWriter(bytes, Encoding.UTF8, true))
            {
                writer.Write("NTSD28_UNITY_LEGACY_DAT_FILES_V1");
                writer.Write(Hash(File.ReadAllBytes(indexPath)));
                foreach (var file in files)
                {
                    writer.Write(file.Relative);
                    writer.Write(Hash(File.ReadAllBytes(file.Path)));
                }
            }
            return Build("unity-legacy", LoganContentIdentity.ForDecodeContract(Hash(bytes.ToArray()), LegacyTag));
        }

        private static Dictionary<string, object> Build(string profile, LoganContentIdentity identity)
        {
            return new Dictionary<string, object>
            {
                ["policy"] = "logan-dat-character-images",
                ["scope"] = profile == "logan-runtime" ? "catalog-object-definitions" : "unity-legacy-dat-files",
                ["profile"] = profile,
                ["rawDefinitionSha256"] = identity.RawDefinitionFingerprint,
                ["decodeContract"] = identity.DecodeContractTag,
                ["semanticSha256"] = identity.SemanticFingerprint,
                ["catalogFingerprint64"] = identity.CatalogFingerprint.ToString("X16"),
                ["schemas"] = new Dictionary<string, object>
                {
                    ["entityRuntime"] = BattleWorldEntityRuntimeSnapshotBuffer.CurrentSchemaVersion,
                    ["aggregate"] = BattleStateSnapshotBuffer.CurrentSchemaVersion,
                    ["checksum"] = BattleLockstepChecksumModule.CurrentSchemaVersion,
                    ["characterShell"] = BattleWorldCharacterShellSnapshotBuffer.CurrentSchemaVersion,
                    ["entityBaseShell"] = BattleWorldEntityBaseShellSnapshotBuffer.CurrentSchemaVersion,
                },
            };
        }

        private static string Hash(byte[] bytes)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty);
        }
    }
}
