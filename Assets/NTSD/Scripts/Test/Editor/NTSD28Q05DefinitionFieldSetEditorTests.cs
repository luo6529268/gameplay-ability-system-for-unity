#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05DefinitionFieldSetEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-DEFINITION-FIELDSET-CONTRACT-001";

        [Test]
        public void OptionalNumericValidityAndBitsMatchNative()
        {
            Type type = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganNumericDecoder");
            MethodInfo method = type.GetMethod("TryParseFiniteFloat64", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            string[] inputs = File.ReadAllLines(Path.Combine(Root, "optional-input.tsv"));
            string[] expected = File.ReadAllLines(Path.Combine(Root, "optional-native.tsv"));
            Assert.That(inputs.Length, Is.EqualTo(expected.Length));
            for (int i = 0; i < inputs.Length; i++)
            {
                string text = DecodeHex(inputs[i].Split('\t')[1]);
                string[] row = expected[i].Split('\t');
                object[] args = { text, 0d };
                bool valid = (bool)method.Invoke(null, args);
                Assert.That(valid, Is.EqualTo(row[3] == "1"), "valid row " + i);
                Assert.That(BitConverter.DoubleToInt64Bits((double)args[1]), Is.EqualTo(long.Parse(row[4], CultureInfo.InvariantCulture)), "bits row " + i);
            }
        }

        [Test]
        public void FieldSetCachesLastWinValidityAndPreservesOrderedRawDeclarations()
        {
            Type type = FieldSetType();
            var values = new List<KeyValuePair<string, string>>
            {
                Pair("max_mp", "500"), Pair("max_mp", "invalid"), Pair("zero", "-0"),
                Pair("speed", "3.3"), Pair("MAX_MP", "100"), Pair("unknown", "")
            };
            object fields = Activator.CreateInstance(type, new object[] { values });
            Assert.That((int)type.GetProperty("Count").GetValue(fields), Is.EqualTo(6));
            Assert.That((bool)type.GetMethod("Contains").Invoke(fields, new object[] { "max_mp" }), Is.True);
            Assert.That((int)type.GetMethod("Int32OrDefault").Invoke(fields, new object[] { "max_mp", 777 }), Is.EqualTo(777));
            Assert.That((int)type.GetMethod("Int32OrDefault").Invoke(fields, new object[] { "missing", 777 }), Is.EqualTo(777));
            Assert.That((int)type.GetMethod("Int32OrDefault").Invoke(fields, new object[] { "zero", 777 }), Is.Zero);
            Assert.That((int)type.GetMethod("Int32OrDefault").Invoke(fields, new object[] { "MAX_MP", 0 }), Is.EqualTo(100));
            double zero = (double)type.GetMethod("Float64OrDefault").Invoke(fields, new object[] { "zero", 1d });
            Assert.That(BitConverter.DoubleToInt64Bits(zero), Is.EqualTo(long.MinValue));
            Assert.That((double)type.GetMethod("Float64OrDefault").Invoke(fields, new object[] { "speed", 0d }), Is.EqualTo(3.3d));
            values.Clear();
            var rows = (IList<KeyValuePair<string, string>>)type.GetProperty("Rows").GetValue(fields);
            Assert.That(rows.Count, Is.EqualTo(6));
            Assert.That(rows[0], Is.EqualTo(Pair("max_mp", "500")));
            Assert.Throws<NotSupportedException>(() => rows.Add(Pair("mutated", "1")));
            Assert.That(type.GetMethod("TextOrNull").Invoke(fields, new object[] { "max_mp" }), Is.EqualTo("invalid"));
        }

        [Test]
        public void EmptyStatsAndUnknownDeclaredStatsRemainDistinct()
        {
            Type fieldType = FieldSetType();
            Type metadataType = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.Animation.LoganDefinitionMetadata");
            Assert.That(metadataType, Is.Not.Null);
            object empty = Activator.CreateInstance(fieldType, new object[] { Array.Empty<KeyValuePair<string, string>>() });
            object unknown = Activator.CreateInstance(fieldType, new object[] { new[] { Pair("unknown", "") } });
            object first = Activator.CreateInstance(metadataType, new[] { empty, empty });
            object second = Activator.CreateInstance(metadataType, new[] { empty, unknown });
            Assert.That(metadataType.GetProperty("HasStatsRecord").GetValue(first), Is.False);
            Assert.That(metadataType.GetProperty("HasStatsRecord").GetValue(second), Is.True);
        }

        [Test]
        public void CachedFieldValuesMatchEveryNativeOptionalSample()
        {
            Type type = FieldSetType();
            string[] inputs = File.ReadAllLines(Path.Combine(Root, "optional-input.tsv"));
            string[] expected = File.ReadAllLines(Path.Combine(Root, "optional-native.tsv"));
            MethodInfo integer = type.GetMethod("TryGetInt32");
            MethodInfo number = type.GetMethod("TryGetFloat64");
            for (int i = 0; i < inputs.Length; i++)
            {
                object fields = Activator.CreateInstance(type, new object[] { new[] { Pair("v", DecodeHex(inputs[i].Split('\t')[1])) } });
                string[] row = expected[i].Split('\t');
                object[] a = { "v", 0 };
                Assert.That((bool)integer.Invoke(fields, a), Is.EqualTo(row[1] == "1"), "integer validity " + i);
                Assert.That((int)a[1], Is.EqualTo(int.Parse(row[2], CultureInfo.InvariantCulture)), "integer value " + i);
                object[] b = { "v", 0d };
                Assert.That((bool)number.Invoke(fields, b), Is.EqualTo(row[3] == "1"), "double validity " + i);
                Assert.That(BitConverter.DoubleToInt64Bits((double)b[1]), Is.EqualTo(long.Parse(row[4], CultureInfo.InvariantCulture)), "double value " + i);
            }
        }

        private static Type FieldSetType()
        {
            Type type = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.Animation.LoganDefinitionFieldSet");
            Assert.That(type, Is.Not.Null);
            return type;
        }

        private static KeyValuePair<string, string> Pair(string key, string value) => new KeyValuePair<string, string>(key, value);

        private static string DecodeHex(string value)
        {
            byte[] bytes = new byte[value.Length / 2];
            for (int i = 0; i < bytes.Length; i++) bytes[i] = byte.Parse(value.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
#endif
