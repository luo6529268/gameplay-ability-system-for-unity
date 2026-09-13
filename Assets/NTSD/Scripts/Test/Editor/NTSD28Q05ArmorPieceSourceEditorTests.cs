#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
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
    public sealed class NTSD28Q05ArmorPieceSourceEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-ARMOR-WEAPON-PIECE-SOURCE-INTEGRATION-001";
        private const string RuntimeRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        private readonly Dictionary<string, JObject> documents = new Dictionary<string, JObject>();

        public static IEnumerable<TestCaseData> Cases()
        {
            foreach (string profile in new[] { "fixtures", "corpus" })
                foreach (string line in File.ReadLines(Path.Combine(Root, "native-" + profile + "-definitions.jsonl")))
                {
                    var doc = JObject.Parse(line);
                    yield return new TestCaseData(profile, (string)doc["path"])
                        .SetName("NativeArmorPiece_" + profile + "_" + (string)doc["path"]);
                }
        }

        [OneTimeSetUp]
        public void ReadCurrentCaptures()
        {
            documents.Clear();
            foreach (string profile in new[] { "fixtures", "corpus" })
                foreach (string line in File.ReadLines(Path.Combine(Root, "native-" + profile + "-definitions.jsonl")))
                {
                    var doc = JObject.Parse(line);
                    documents[profile + ":" + (string)doc["path"]] = doc;
                }
        }

        [TestCaseSource(nameof(Cases))]
        public void NativeAstAndActualDefinitionMatch(string profile, string file)
        {
            JObject expected = documents[profile + ":" + file];
            string path = Path.Combine(profile == "corpus" ? Path.Combine(RuntimeRoot, "decoded_dat") : Path.Combine(Root, "fixtures"), file);
            string text = File.ReadAllText(path);
            if (!(bool)expected["parseSuccess"])
            {
                Assert.Throws<FormatException>(() => new Lf2DatParserV2().ParseLoganContent(text, path));
                return;
            }
            var dat = new Lf2DatParserV2().ParseLoganContent(text, path);
            var armors = Items(Read(dat, "LoganArmors"));
            var nativeArmors = (JArray)expected["armor"];
            Assert.That(armors.Length, Is.EqualTo(nativeArmors.Count));
            for (int i = 0; i < armors.Length; i++)
            {
                CheckLines(armors[i], nativeArmors[i]);
                CheckRaw((IEnumerable<Lf2DatProperty>)Read(armors[i], "Properties"), nativeArmors[i]["fields"]);
            }
            CheckPiece(Read(dat, "LoganWeaponPiece"), (JObject)expected["weaponPiece"], false);
            if (profile == "corpus" && !(bool)expected["indexed"]) return;
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(text, path, BattleContentSource.ForLoganRuntime(RuntimeRoot));
            Assert.That(data.armors.Count, Is.EqualTo(nativeArmors.Count));
            var metadataArmors = Items(Read(data.NativeMetadata, "Armors"));
            Assert.That(metadataArmors.Length, Is.EqualTo(nativeArmors.Count));
            for (int i = 0; i < data.armors.Count; i++)
            {
                CheckTypedArmor(data.armors[i], (JObject)nativeArmors[i]["normalized"]);
                CheckFields((LoganDefinitionFieldSet)metadataArmors[i], nativeArmors[i]["fields"]);
            }
            CheckPiece(Read(data.NativeMetadata, "WeaponPiece"), (JObject)expected["weaponPiece"], true);
        }

        [TestCase("armor_unclosed.dat")]
        [TestCase("piece_six_groups.dat")]
        public void InvalidDefinitionRejectsWholeCatalog(string fixture)
        {
            string root = Path.GetFullPath(Path.Combine("Temp/Q05ArmorPieceCatalog", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            File.WriteAllText(Path.Combine(root, "decoded_dat/good.dat"), File.ReadAllText(Path.Combine(Root, "fixtures/empty.dat")));
            File.WriteAllText(Path.Combine(root, "decoded_dat/bad.dat"), File.ReadAllText(Path.Combine(Root, "fixtures", fixture)));
            File.WriteAllText(Path.Combine(root, "catalog.csv"), "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,19,0,good.dat,one\nobject,1,23,0,bad.dat,two\n");
            var catalog = LoganObjectCatalog.Read(BattleContentSource.ForLoganRuntime(root));
            var error = Assert.Throws<AggregateException>(() => CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(catalog));
            Assert.That(error.InnerExceptions.Count, Is.EqualTo(1));
            Assert.That(error.InnerExceptions[0].Message, Does.Contain("bad.dat"));
        }

        [Test]
        public void ImmutableDefinitionOwnsNestedFieldsAndCollections()
        {
            var dat = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(Path.Combine(Root, "fixtures/piece_basic.dat")));
            var piece = new LoganWeaponPieceDefinition(dat.LoganWeaponPiece);
            dat.LoganWeaponPiece.Properties.Clear();
            dat.LoganWeaponPiece.Groups[0].Properties[0].Value = "99";
            dat.LoganWeaponPiece.Groups[0].Variants[0].Properties.Clear();
            dat.LoganWeaponPiece.Groups[0].Variants.Clear();
            dat.LoganWeaponPiece.Groups.Clear();
            Assert.That(piece.Fields.Int32OrDefault("team", 0), Is.EqualTo(1));
            Assert.That(piece.Groups[0].Fields.Int32OrDefault("amount", 0), Is.EqualTo(2));
            Assert.That(piece.Groups[0].Variants[0].Fields.Int32OrDefault("oid", 0), Is.EqualTo(999));
            Assert.Throws<NotSupportedException>(() => ((IList<LoganWeaponPieceGroup>)piece.Groups).Clear());
            Assert.Throws<NotSupportedException>(() => ((IList<LoganWeaponPieceVariant>)piece.Groups[0].Variants).Clear());
            var fields = new LoganDefinitionFieldSet(new[] { new KeyValuePair<string, string>("hp", "42") });
            var armors = new List<LoganDefinitionFieldSet> { fields };
            var metadata = new LoganDefinitionMetadata(fields, fields, armors, piece);
            armors.Clear();
            Assert.That(metadata.Armors.Count, Is.EqualTo(1));
            Assert.Throws<NotSupportedException>(() => ((IList<LoganDefinitionFieldSet>)metadata.Armors).Clear());
        }

        private static object Read(object owner, string name)
        {
            Assert.That(owner, Is.Not.Null);
            var property = owner.GetType().GetProperty(name);
            if (property != null) return property.GetValue(owner);
            var field = owner.GetType().GetField(name);
            Assert.That(field, Is.Not.Null, owner.GetType().Name + "." + name);
            return field.GetValue(owner);
        }

        private static object[] Items(object value) => ((IEnumerable)value).Cast<object>().ToArray();
        private static void CheckLines(object value, JToken expected)
        {
            Assert.That(Read(value, "OpeningLine"), Is.EqualTo((int)expected["openingLine"]));
            Assert.That(Read(value, "ClosingLine"), Is.EqualTo((int)expected["closingLine"]));
        }
        private static void CheckRaw(IEnumerable<Lf2DatProperty> value, JToken expected)
        {
            Assert.That(value.Select(p => p.Key + "\t" + p.Value), Is.EqualTo(expected.Select(p => (string)p["key"] + "\t" + (string)p["value"])));
        }
        private static void CheckFields(LoganDefinitionFieldSet value, JToken expected)
        {
            Assert.That(value.Rows.Select(p => p.Key + "\t" + p.Value), Is.EqualTo(expected.Select(p => (string)p["key"] + "\t" + (string)p["value"])));
        }
        private static void CheckPiece(object value, JObject expected, bool metadata)
        {
            if (!expected.HasValues) { Assert.That(value, Is.Null); return; }
            Assert.That(value, Is.Not.Null);
            if (!metadata) CheckLines(value, expected);
            CheckPieceFields(value, expected, metadata);
            object[] groups = Items(Read(value, "Groups"));
            Assert.That(groups.Length, Is.EqualTo(expected["groups"].Count()));
            for (int i = 0; i < groups.Length; i++)
            {
                JToken group = expected["groups"][i];
                Assert.That(Read(groups[i], "Piece"), Is.EqualTo((int)group["piece"]));
                CheckPieceFields(groups[i], group, metadata);
                object[] variants = Items(Read(groups[i], "Variants"));
                Assert.That(variants.Length, Is.EqualTo(group["variants"].Count()));
                for (int j = 0; j < variants.Length; j++)
                {
                    JToken variant = group["variants"][j];
                    Assert.That(Read(variants[j], "Piece"), Is.EqualTo((int)variant["piece"]));
                    if (!metadata) CheckLines(variants[j], variant);
                    CheckPieceFields(variants[j], variant, metadata);
                }
            }
        }
        private static void CheckPieceFields(object value, JToken expected, bool metadata)
        {
            if (metadata) CheckFields((LoganDefinitionFieldSet)Read(value, "Fields"), expected["fields"]);
            else CheckRaw((IEnumerable<Lf2DatProperty>)Read(value, "Properties"), expected["fields"]);
        }
        private static void CheckTypedArmor(LF2ArmorData armor, JObject expected)
        {
            foreach (string name in new[] { "type", "ratio", "decrease", "mp", "fall", "bdefend", "injury", "spark", "hp", "recover", "facing", "action", "reserve", "delay" })
                Assert.That(Read(armor, name), Is.EqualTo((int)expected[name]), name);
            foreach (var pair in new[] { ("states", "state"), ("kinds", "kind"), ("ids", "id"), ("effects", "effect") })
                Assert.That((IEnumerable<int>)Read(armor, pair.Item1), Is.EqualTo(expected[pair.Item2].Values<int>()), pair.Item1);
            Assert.That(armor.frame_ranges.Select(p => p.first + ":" + p.last), Is.EqualTo(expected["frame"].Select(p => (int)p[0] + ":" + (int)p[1])));
            Assert.That(armor.sound1, Is.EqualTo((string)expected["sound1"]));
            Assert.That(armor.sound2, Is.EqualTo((string)expected["sound2"]));
        }
    }
}
#endif
