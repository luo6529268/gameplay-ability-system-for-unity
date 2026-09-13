#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05StrengthTableEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-STRENGTH-TABLE-ADMISSION-001";
        private const string RuntimeRoot = "J:/QQFile/NTSD2.8.3.3 zip/NTSD2.8.3.3/NTSD 2.8-Logan/resources/runtime";
        public static IEnumerable<TestCaseData> Cases()
        {
            foreach (string line in File.ReadAllLines(Path.Combine(Root, "native.tsv")))
            {
                string[] columns = line.Split('\t');
                if (columns[1] == "document")
                    yield return new TestCaseData(columns[0], columns[2] == "1").SetName("NativeStrengthTable_" + columns[0]);
            }
        }

        [TestCaseSource(nameof(Cases))]
        public void AdmissionRowsAndManagerMatchNative(string file, bool accepted)
        {
            string path = Path.GetFullPath(Path.Combine(Root, "fixtures", file));
            string text = File.ReadAllText(path);
            var source = BattleContentSource.ForLoganRuntime(RuntimeRoot);
            if (!accepted)
            {
                Assert.Throws<FormatException>(() => new Lf2DatParserV2().ParseLoganContent(text, path));
                Assert.Throws<FormatException>(() => CharacterAnimtorManager.BuildCharacterDataFromSource(text, path, source));
                return;
            }
            Lf2DatFile dat = new Lf2DatParserV2().ParseLoganContent(text, path);
            var rows = ((IEnumerable)Property(dat, "LoganWeaponStrengthRows")).Cast<object>().ToList();
            Assert.That(Property(dat, "LoganOriginalText"), Is.EqualTo(text));
            AssertRowsAndManager(dat, rows, file, Path.Combine(Root, "native.tsv"), text, path, source);
            LF2CharacterData data = CharacterAnimtorManager.BuildCharacterDataFromSource(text, path, source);
            Assert.That(data.weapon_hp, Is.EqualTo(83), "table captions/fields cannot overwrite BMP weapon_hp");
            Assert.That(data.walking_speed, Is.EqualTo(2), "table captions/fields cannot overwrite movement metadata");
        }

        public static IEnumerable<TestCaseData> CorpusCases()
        {
            foreach (string line in File.ReadAllLines(Path.Combine(Root, "native-corpus.tsv")))
            {
                string[] columns = line.Split('\t');
                if (columns[1] == "document" && columns[3] != "0")
                    yield return new TestCaseData(columns[0], columns[2] == "1").SetName("NativeStrengthCorpus_" + columns[0]);
            }
        }

        [TestCaseSource(nameof(CorpusCases))]
        public void FormalStrengthDefinitionsMatchNative(string file, bool accepted)
        {
            Assert.That(accepted, Is.True);
            string path = Path.Combine(RuntimeRoot, file);
            string text = File.ReadAllText(path);
            var dat = new Lf2DatParserV2().ParseLoganContent(text, path);
            var rows = ((IEnumerable)Property(dat, "LoganWeaponStrengthRows")).Cast<object>().ToList();
            AssertRowsAndManager(dat, rows, file, Path.Combine(Root, "native-corpus.tsv"), text, path,
                BattleContentSource.ForLoganRuntime(RuntimeRoot));
        }

        private static void AssertRowsAndManager(Lf2DatFile dat, List<object> rows, string file, string evidence,
            string text, string path, BattleContentSource source)
        {
            Assert.That(Property(dat, "LoganOriginalText"), Is.EqualTo(text));
            string[][] expected = File.ReadAllLines(evidence).Select(line => line.Split('\t')).Where(c => c[0] == file).ToArray();
            string[][] nativeRows = expected.Where(c => c[1] == "row").ToArray();
            Assert.That(rows.Count, Is.EqualTo(nativeRows.Length));
            for (int i = 0; i < rows.Count; i++)
            {
                int index = int.Parse(nativeRows[i][2], CultureInfo.InvariantCulture);
                Assert.That(Property(rows[i], "Index"), Is.EqualTo(index));
                Assert.That(Property(rows[i], "OpeningLine"), Is.EqualTo(int.Parse(nativeRows[i][3], CultureInfo.InvariantCulture)));
                Assert.That(Property(rows[i], "Caption"), Is.EqualTo(Unhex(nativeRows[i][5])));
                var properties = ((IEnumerable)Property(rows[i], "Properties")).Cast<Lf2DatProperty>().ToArray();
                string[][] nativeFields = expected.Where(c => c[1] == "field" && c[2] == nativeRows[i][2]).ToArray();
                Assert.That(properties.Length, Is.EqualTo(nativeFields.Length));
                for (int j = 0; j < properties.Length; j++)
                {
                    Assert.That(properties[j].Key, Is.EqualTo(nativeFields[j][4]));
                    Assert.That(properties[j].Value, Is.EqualTo(Unhex(nativeFields[j][5])));
                }
            }
            LF2CharacterData data = CharacterAnimtorManager.BuildCharacterDataFromSource(text, path, source);
            Assert.That(data.weapon_strength_list.Count, Is.EqualTo(rows.Count));
            MethodInfo decoder = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganCombatRecordDecoder")
                .GetMethod("WeaponStrength", BindingFlags.Static | BindingFlags.NonPublic);
            for (int i = 0; i < rows.Count; i++)
            {
                var expectedRow = (WeaponStrengthEntry)decoder.Invoke(null, new[] { Property(rows[i], "Index"), Property(rows[i], "Properties") });
                foreach (FieldInfo field in typeof(WeaponStrengthEntry).GetFields())
                    Assert.That(field.GetValue(data.weapon_strength_list[i]), Is.EqualTo(field.GetValue(expectedRow)), field.Name);
            }
        }

        [Test]
        public void LegacyParserAndBuilderRetainTheirExplicitPath()
        {
            string path = Path.GetFullPath(Path.Combine(Root, "fixtures", "zero.dat"));
            string text = File.ReadAllText(path);
            Lf2DatFile dat = new Lf2DatParserV2().Parse(text, path);
            PropertyInfo property = typeof(Lf2DatFile).GetProperty("LoganWeaponStrengthRows");
            Assert.That(property, Is.Not.Null);
            Assert.That(property.GetValue(dat), Is.Null);
            var data = CharacterAnimtorManager.BuildCharacterDataFromSource(text, path, BattleContentSource.ForUnityProject(Directory.GetCurrentDirectory()));
            Assert.That(data.weapon_strength_list[0].index, Is.Zero);
        }

        private static object Property(object source, string name)
        {
            PropertyInfo property = source.GetType().GetProperty(name);
            Assert.That(property, Is.Not.Null, name);
            return property.GetValue(source);
        }
        private static string Unhex(string text)
        {
            var bytes = new byte[text.Length / 2];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = byte.Parse(text.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
#endif
