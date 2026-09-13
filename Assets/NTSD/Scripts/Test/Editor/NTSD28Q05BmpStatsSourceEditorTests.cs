#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05BmpStatsSourceEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-BMP-STATS-SOURCE-INTEGRATION-001";
        private const string RuntimeRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private static readonly Dictionary<string, JObject> Documents = new Dictionary<string, JObject>();

        [OneTimeSetUp]
        public void ClearPreviousCaptureCache()
        {
            Documents.Clear();
        }

        public static IEnumerable<TestCaseData> Cases()
        {
            foreach (string profile in new[] { "fixtures", "corpus" })
            {
                foreach (string line in File.ReadAllLines(Path.Combine(Root, "native-" + profile + "-headers.jsonl")))
                {
                    var doc = JObject.Parse(line);
                    yield return new TestCaseData(profile, (string)doc["path"])
                        .SetName("NativeBmpStats_" + profile + "_" + (string)doc["path"]);
                }
            }
        }

        [TestCaseSource(nameof(Cases))]
        public void NativeHeaderAstAndActualMetadataMatch(string profile, string file)
        {
            string key = profile + ":" + file;
            if (!Documents.ContainsKey(key))
                foreach (string line in File.ReadAllLines(Path.Combine(Root, "native-" + profile + "-headers.jsonl")))
                {
                    var doc = JObject.Parse(line);
                    Documents[profile + ":" + (string)doc["path"]] = doc;
                }
            JObject expected = Documents[key];
            string path = Path.Combine(profile == "corpus" ? Path.Combine(RuntimeRoot, "decoded_dat") : Path.Combine(Root, "fixtures"), file);
            string text = File.ReadAllText(path);
            if (!(bool)expected["parseSuccess"])
            {
                Assert.Throws<FormatException>(() => new Lf2DatParserV2().ParseLoganContent(text, path));
                return;
            }
            var dat = new Lf2DatParserV2().ParseLoganContent(text, path);
            PropertyInfo statsProperty = typeof(Lf2DatFile).GetProperty("LoganStats");
            Assert.That(statsProperty, Is.Not.Null);
            var stats = (Lf2DatBlock)statsProperty.GetValue(dat);
            Assert.That(stats, Is.Not.Null);
            CheckFields(dat.Bmp?.Properties, (JArray)expected["bmp"]);
            CheckFields(stats.Properties, (JArray)expected["stats"]);
            var sequences = dat.Bmp?.FrameSequences ?? new List<Lf2BmpFrameSequence>();
            Assert.That(sequences.Count, Is.EqualTo(expected["bmpSequences"].Count()));
            for (int i = 0; i < sequences.Count; i++)
            {
                Assert.That(sequences[i].Name, Is.EqualTo((string)expected["bmpSequences"][i]["name"]));
                Assert.That(sequences[i].Actions, Is.EqualTo(expected["bmpSequences"][i]["actions"].Values<int>().ToArray()));
            }
            var files = dat.Bmp?.Files ?? new List<Lf2SpriteFileDef>();
            Assert.That(files.Count, Is.EqualTo(expected["sprites"].Count()));
            for (int i = 0; i < files.Count; i++)
            {
                JToken sheet = expected["sprites"][i];
                Assert.That(files[i].StartIndex, Is.EqualTo((int)sheet["declaredFirst"]));
                Assert.That(files[i].EndIndex, Is.EqualTo((int)sheet["declaredLast"]));
                Assert.That(files[i].Path, Is.EqualTo((string)sheet["path"]));
                Assert.That(files[i].Width, Is.EqualTo((int?)sheet["width"] ?? 0));
                Assert.That(files[i].Height, Is.EqualTo((int?)sheet["height"] ?? 0));
                Assert.That(files[i].Row, Is.EqualTo((int?)sheet["row"] ?? 0));
                Assert.That(files[i].Col, Is.EqualTo((int?)sheet["col"] ?? 0));
            }
            // Full manager projection is meaningful for the indexed gameplay catalog and owned fixtures.
            if (profile == "corpus" && !(bool)expected["indexed"]) return;
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(text, path, BattleContentSource.ForLoganRuntime(RuntimeRoot));
            PropertyInfo property = typeof(LF2CharacterData).GetProperty("NativeMetadata");
            Assert.That(property, Is.Not.Null);
            var metadata = (LoganDefinitionMetadata)property.GetValue(data);
            Assert.That(metadata, Is.Not.Null);
            Assert.That(metadata.Bmp.Rows.Select(p => p.Key + "\t" + p.Value), Is.EqualTo(expected["bmp"].Select(f => (string)f["key"] + "\t" + (string)f["value"])));
            Assert.That(metadata.Stats.Rows.Select(p => p.Key + "\t" + p.Value), Is.EqualTo(expected["stats"].Select(f => (string)f["key"] + "\t" + (string)f["value"])));
            Assert.That(metadata.HasStatsRecord, Is.EqualTo(expected["stats"].Any()));
            Assert.That(data.walking_frame_rate, Is.EqualTo(metadata.Bmp.Int32OrDefault("walking_frame_rate", 1)));
            Assert.That(data.running_frame_rate, Is.EqualTo(metadata.Bmp.Int32OrDefault("running_frame_rate", 1)));
            Assert.That(data.walking_speed, Is.EqualTo((float)metadata.Bmp.Float64OrDefault("walking_speed", 0)));
            Assert.That(data.weapon_hp, Is.EqualTo(metadata.Bmp.Int32OrDefault("weapon_hp", 0)));
            Assert.That(data.use_ai, Is.EqualTo(metadata.Bmp.Int32OrDefault("use_ai", 0)));
            Assert.That(data.recmp, Is.EqualTo(metadata.Stats.Int32OrDefault("recmp", 0)));
        }

        [Test]
        public void LegacyMetadataRemainsUnbound()
        {
            var path = Path.GetFullPath(Path.Combine(Root, "fixtures", "basic.dat"));
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(File.ReadAllText(path), path,
                BattleContentSource.ForUnityProject(Directory.GetCurrentDirectory()));
            PropertyInfo property = typeof(LF2CharacterData).GetProperty("NativeMetadata");
            Assert.That(property, Is.Not.Null);
            Assert.That(property.GetValue(data), Is.Null);
        }

        private static void CheckFields(IReadOnlyList<Lf2DatProperty> actual, JArray expected)
        {
            Assert.That(actual?.Count ?? 0, Is.EqualTo(expected.Count));
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.That(actual[i].Key, Is.EqualTo((string)expected[i]["key"]));
                Assert.That(actual[i].Value, Is.EqualTo((string)expected[i]["value"]));
            }
        }
    }
}
#endif
