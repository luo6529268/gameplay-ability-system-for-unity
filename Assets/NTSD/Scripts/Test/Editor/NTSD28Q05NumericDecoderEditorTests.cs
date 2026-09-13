#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NTSD.DatParser;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05NumericDecoderEditorTests
    {
        private const string WitnessRoot = "artifacts/diagnostics/NTSD28-Q03-NUMERIC-DECODE-WITNESS-001";

        public static IEnumerable<TestCaseData> NativeCases()
        {
            foreach (string line in File.ReadAllLines(Path.Combine(WitnessRoot, "native.tsv")))
            {
                if (line.StartsWith("id\t", StringComparison.Ordinal)) continue;
                string[] columns = line.Split('\t');
                yield return new TestCaseData(columns[0], columns).SetName("NativeNumeric_" + columns[0]);
            }
        }

        [TestCaseSource(nameof(NativeCases))]
        public void ExactNativeValuesAndFloatBits(string id, string[] expected)
        {
            string text = File.ReadAllText(Path.Combine(WitnessRoot, "fixtures", id + ".dat"));
            Lf2FrameBlock frame = new Lf2DatParserV2().ParseLoganContent(text).Frames[0];
            string[] floats = { "throwvx", "throwvy", "throwvz" };
            for (int index = 0; index < floats.Length; index++)
            {
                float value = (float)Invoke("ParseFiniteFloat32OrZero", Last(frame, "cpoint", floats[index]));
                string bits = unchecked((uint)BitConverter.SingleToInt32Bits(value)).ToString("X8");
                Assert.That(bits, Is.EqualTo(expected[index + 1]), id + "/" + floats[index]);
            }
            Assert.That(Invoke("ParseInt32OrZero", Last(frame, "cpoint", "injury")),
                Is.EqualTo(int.Parse(expected[4], CultureInfo.InvariantCulture)), id + "/injury");
            Assert.That(Invoke("ParseInt32OrZero", Last(frame, "wpoint", "x")),
                Is.EqualTo(int.Parse(expected[5], CultureInfo.InvariantCulture)), id + "/wpoint.x");
            string[] actions = { "caughtact", "catchingact", "pickedact", "pickingact" };
            for (int index = 0; index < actions.Length; index++)
                Assert.That(Invoke("ParseFirstInt32OrZero", Last(frame, "itr", actions[index])),
                    Is.EqualTo(int.Parse(expected[index + 6], CultureInfo.InvariantCulture)), id + "/" + actions[index]);
        }

        [TestCase("0", true, 0)]
        [TestCase("bad", false, 0)]
        [TestCase("+0", false, 0)]
        [TestCase("-2147483648", true, int.MinValue)]
        public void StrictIntegerPreservesSuccessVersusDefault(string text, bool valid, int expected)
        {
            Type type = DecoderType();
            MethodInfo method = type.GetMethod("TryParseInt32", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            object[] arguments = { text, 0 };
            Assert.That(method.Invoke(null, arguments), Is.EqualTo(valid));
            Assert.That(arguments[1], Is.EqualTo(expected));
        }

        [TestCase("raw", true)]
        [TestCase("differential", false)]
        public void ExpandedNativeWitnessValues(string prefix, bool rawFields)
        {
            const string root = "artifacts/diagnostics/NTSD28-Q05-NATIVE-NUMERIC-DECODER-001";
            var inputs = new Dictionary<string, string>();
            foreach (string line in File.ReadAllLines(Path.Combine(root, prefix + "-input.tsv")))
            {
                string[] columns = line.Split('\t');
                inputs.Add(columns[0], Encoding.UTF8.GetString(Convert.FromBase64String(columns[1])));
            }
            int compared = 0;
            foreach (string line in File.ReadAllLines(Path.Combine(root, prefix + "-native.tsv")))
            {
                if (line.StartsWith("id\t", StringComparison.Ordinal)) continue;
                string[] columns = line.Split('\t');
                string value = inputs[columns[0]];
                float decoded = (float)Invoke("ParseFiniteFloat32OrZero", value);
                Assert.That(unchecked((uint)BitConverter.SingleToInt32Bits(decoded)).ToString("X8"),
                    Is.EqualTo(columns[1]), columns[0] + "/float");
                Assert.That(Invoke("ParseInt32OrZero", value),
                    Is.EqualTo(int.Parse(columns[rawFields ? 2 : 4], CultureInfo.InvariantCulture)), columns[0] + "/strict");
                Assert.That(Invoke("ParseFirstInt32OrZero", value),
                    Is.EqualTo(int.Parse(columns[rawFields ? 3 : 6], CultureInfo.InvariantCulture)), columns[0] + "/first");
                if (rawFields)
                    StrictIntegerPreservesSuccessVersusDefault(value, columns[4] == "1",
                        int.Parse(columns[2], CultureInfo.InvariantCulture));
                compared++;
            }
            Assert.That(compared, Is.EqualTo(rawFields ? 969 : 3622));
        }

        private static Type DecoderType()
        {
            Type type = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganNumericDecoder");
            Assert.That(type, Is.Not.Null, "Q05 native numeric decoder is not implemented");
            return type;
        }

        private static object Invoke(string name, string text)
        {
            MethodInfo method = DecoderType().GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, name);
            return method.Invoke(null, new object[] { text });
        }

        private static string Last(Lf2FrameBlock frame, string kind, string key)
        {
            Lf2DatSubBlock block = frame.SubBlocks.Find(item => item.Name == kind);
            string result = null;
            foreach (Lf2DatProperty field in block.Properties)
                if (field.Key == key) result = field.Value;
            return result;
        }
    }
}
#endif
