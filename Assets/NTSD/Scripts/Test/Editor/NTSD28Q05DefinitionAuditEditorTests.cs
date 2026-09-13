#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05DefinitionAuditEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-DEFINITION-HEADER-CONTRACT-AUDIT-001";
        private const string RuntimeRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";

        [Test]
        public void CaptureActualIndexedDefinitionHeaders()
        {
            var source = BattleContentSource.ForLoganRuntime(RuntimeRoot);
            var catalog = LoganObjectCatalog.Read(source);
            Assert.That(catalog.Entries.Count, Is.EqualTo(330));
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog);
            var definitions = new JArray();
            var fieldTypes = new JObject();
            FieldInfo[] fields = typeof(LF2CharacterData).GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields) fieldTypes[field.Name] = field.FieldType.FullName;
            foreach (var entry in catalog.Entries)
            {
                var dat = new Lf2DatParserV2().ParseLoganContent(entry.DatText, entry.DatPath);
                var data = configs[entry.Id].characterData;
                var values = new JObject();
                var floatBits = new JObject();
                var doubleBits = new JObject();
                foreach (var field in fields)
                {
                    if (field.Name == "frames" || field.Name == "files") continue;
                    object value = field.GetValue(data);
                    values[field.Name] = value == null ? JValue.CreateNull() : JToken.FromObject(value);
                    if (value is float single)
                    {
                        floatBits[field.Name] = BitConverter.ToInt32(BitConverter.GetBytes(single), 0);
                        doubleBits[field.Name] = BitConverter.DoubleToInt64Bits(single);
                    }
                }
                var stats = new JArray();
                var blocks = new JArray();
                foreach (var block in dat.Blocks)
                {
                    var properties = Properties(block.Properties);
                    blocks.Add(new JObject { ["name"] = block.Name, ["properties"] = properties });
                    if (string.Equals(block.Name, "stats", StringComparison.OrdinalIgnoreCase))
                        foreach (var property in block.Properties)
                            stats.Add(new JObject { ["key"] = property.Key, ["value"] = property.Value });
                }
                definitions.Add(new JObject
                {
                    ["id"] = entry.Id,
                    ["type"] = entry.Type,
                    ["registryIndex"] = entry.RegistryIndex,
                    ["path"] = entry.SourcePath.Replace('\\', '/'),
                    ["datSha256"] = entry.DatSha256,
                    ["values"] = values,
                    ["float32Bits"] = floatBits,
                    ["floatPromotedDoubleBits"] = doubleBits,
                    ["frameCount"] = data.frames.Count,
                    ["rawTop"] = Properties(dat.Properties),
                    ["rawBmp"] = Properties(dat.Bmp?.Properties),
                    ["rawStats"] = stats,
                    ["rawBlocks"] = blocks,
                    ["rawBmpSequences"] = dat.Bmp == null ? new JArray() : JToken.FromObject(dat.Bmp.FrameSequences)
                });
            }
            var hashes = new JObject();
            foreach (string path in new[]
            {
                "Assets/NTSD/Scripts/Animation/LF2CharacterData.cs",
                "Assets/NTSD/Scripts/Animation/LF2ArmorData.cs",
                "Assets/NTSD/Scripts/Animation/Manager/CharacterAnimtorManager.cs",
                "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.cs",
                "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganStrength.cs",
                "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatParserV2.LoganFrames.cs",
                "Assets/NTSD/Scripts/DatParser/Runtime/Parsing/Lf2DatTokenizer.cs",
                "Assets/NTSD/Scripts/DatParser/Runtime/Utils/Lf2DatConverter.cs"
            })
            {
                using (var sha = SHA256.Create())
                    hashes[path] = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(path))).Replace("-", "");
            }
            var result = new JObject
            {
                ["captureUtc"] = DateTime.UtcNow.ToString("O"),
                ["kind"] = "ACTUAL_MANAGER_HEADER_CAPTURE_NOT_PARITY",
                ["unityVersion"] = UnityEngine.Application.unityVersion,
                ["catalogSha256"] = catalog.CatalogSha256,
                ["definitionFingerprint"] = catalog.DefinitionFingerprint,
                ["fieldTypes"] = fieldTypes,
                ["productionHashes"] = hashes,
                ["definitions"] = definitions
            };
            Directory.CreateDirectory(Root);
            File.WriteAllText(Path.Combine(Root, "unity-headers.json"), result.ToString(Formatting.Indented), new UTF8Encoding(false));
            Assert.That(definitions.Count, Is.EqualTo(330));
            TestContext.WriteLine("Captured 330 actual manager definitions; comparison is a separate audit artifact.");
        }

        private static JArray Properties(IEnumerable<Lf2DatProperty> properties)
        {
            var result = new JArray();
            if (properties != null)
                foreach (var property in properties)
                    result.Add(new JObject { ["key"] = property.Key, ["value"] = property.Value });
            return result;
        }
    }
}
#endif
