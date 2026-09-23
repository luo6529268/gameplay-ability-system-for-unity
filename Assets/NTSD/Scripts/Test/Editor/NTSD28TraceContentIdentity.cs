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

        internal static Dictionary<string, object> FromLoganCatalog(LoganObjectCatalog catalog)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            return Build("logan-runtime", catalog.ContentIdentity);
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
            bool hasMode = profile == "logan-runtime" && identity.ModeInputFingerprint != null;
            bool hasKind = profile == "logan-runtime" && identity.KindInputFingerprint != null;
            var result = new Dictionary<string, object>
            {
                ["policy"] = "logan-dat-character-images",
                ["scope"] = profile == "logan-runtime"
                    ? (hasKind
                        ? (hasMode ? "catalog-object-fusion-mode-kind-definitions" : "catalog-object-fusion-kind-definitions")
                        : (hasMode ? "catalog-object-fusion-mode-definitions" : "catalog-object-fusion-definitions"))
                    : "unity-legacy-dat-files",
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
            if (profile == "logan-runtime")
            {
                if (identity.DecodeContractTag != LoganContentIdentity.CurrentDecodeContractTag ||
                    identity.ObjectDefinitionFingerprint == null || identity.FusionInputFingerprint == null ||
                    identity.FusionSemanticFingerprint == null ||
                    (hasMode != (identity.ModeSemanticFingerprint != null)) ||
                    (hasKind != (identity.KindSemanticFingerprint != null)) ||
                    identity.BattleInputContractTag != (hasKind
                        ? (hasMode ? "NTSD28_LOGAN_BATTLE_INPUTS_V3" : "NTSD28_LOGAN_BATTLE_INPUTS_V3_KIND_ONLY")
                        : (hasMode ? "NTSD28_LOGAN_BATTLE_INPUTS_V2" : "NTSD28_LOGAN_BATTLE_INPUTS_V1")))
                    throw new InvalidOperationException("Current Logan trace requires complete battle content identity.");
                result["objectDefinitionSha256"] = identity.ObjectDefinitionFingerprint;
                result["fusionInputSha256"] = identity.FusionInputFingerprint;
                result["fusionSemanticSha256"] = identity.FusionSemanticFingerprint;
                if (hasMode)
                {
                    result["battleInputContract"] = identity.BattleInputContractTag;
                    result["modeInputSha256"] = identity.ModeInputFingerprint;
                    result["modeSemanticSha256"] = identity.ModeSemanticFingerprint;
                }
                if (hasKind)
                {
                    result["battleInputContract"] = identity.BattleInputContractTag;
                    result["kindInputSha256"] = identity.KindInputFingerprint;
                    result["kindSemanticSha256"] = identity.KindSemanticFingerprint;
                }
            }
            return result;
        }

        private static string Hash(byte[] bytes)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", string.Empty);
        }
    }
}
