#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05ItrStrengthContentEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-ITR40-STRENGTH19-RECORD-DECODING-001";
        private static readonly string[] Keys = "kind x y w h dvx dvy fall arest vrest respond effect drain spark recover dbdefend bdefend injury zwidth z dvz sound cover caughtact catchingact pickedact pickingact delay poison confus weak manacle join mimic bound facing dx dy dz gain".Split(' ');
        private static readonly HashSet<string> StrengthKeys = new HashSet<string>("dvx dvy fall arest vrest respond effect drain spark recover dbdefend bdefend injury zwidth z dvz sound cover caughtact".Split(' '));

        public static IEnumerable<TestCaseData> NativeCases()
        {
            foreach (string line in File.ReadAllLines(Path.Combine(Root, "native.tsv")))
            {
                if (line.StartsWith("path\t", StringComparison.Ordinal)) continue;
                string[] values = line.Split('\t');
                if (values[1] == "document" || values[0].StartsWith("admission_", StringComparison.Ordinal)) continue;
                yield return new TestCaseData(new object[] { values }).SetName("NativeRecord_" + values[1] + "_" + values[0]);
            }
        }

        [TestCaseSource(nameof(NativeCases))]
        public void AllNativeRecordValuesMatch(string[] expected)
        {
            if (expected[1] == "itr")
            {
                var dat = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(Path.Combine(Root, "fixtures", expected[0])));
                var block = dat.Frames[0].SubBlocks[0];
                int count = block.Properties.Count;
                var area = (InteractionArea)Invoke("Interaction", block);
                for (int i = 0; i < Keys.Length; i++)
                    Assert.That(Scalar(area, Keys[i]), Is.EqualTo(Number(expected[i + 3])), Keys[i]);
                Assert.That(block.Properties.Count, Is.EqualTo(count));
                Assert.That(area.catchingact, Has.Length.EqualTo(1));
                Assert.That(area.caughtact, Has.Length.EqualTo(1));
                Assert.That(area.catchingact2, Is.Null);
                Assert.That(area.caughtact2, Is.Null);
                Assert.That(area.vaction, Is.Zero);
                Assert.That(area.kill, Is.Zero);
                Assert.That(area.throwvz, Is.Zero);
            }
            else
            {
                List<Lf2DatProperty> fields = Fields(Path.GetFileNameWithoutExtension(expected[0]));
                var row = (WeaponStrengthEntry)Invoke("WeaponStrength", Number(expected[2]), fields);
                Assert.That(row.index, Is.EqualTo(Number(expected[2])));
                for (int i = 0; i < Keys.Length; i++)
                    if (StrengthKeys.Contains(Keys[i])) Assert.That(Scalar(row, Keys[i]), Is.EqualTo(Number(expected[i + 3])), Keys[i]);
                Assert.That(typeof(WeaponStrengthEntry).GetFields(), Has.Length.EqualTo(20));
            }
        }

        [TestCase(0)]
        [TestCase(10)]
        [TestCase(-1)]
        public void SingleRecordDecoderRejectsNonNativeIndex(int index)
        {
            MethodInfo method = DecoderMethod("WeaponStrength");
            var error = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { index, Fields("ordered") }));
            Assert.That(error.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [TestCase(1)]
        [TestCase(9)]
        public void SingleRecordAcceptsBothValidIndexBoundaries(int index)
        {
            var row = (WeaponStrengthEntry)Invoke("WeaponStrength", index, Fields("ordered"));
            Assert.That(row.index, Is.EqualTo(index));
        }

        [Test]
        public void NewFieldsCopyCloneAndFingerprintIndependently()
        {
            var area = new InteractionArea();
            var copy = new InteractionArea();
            foreach (string name in new[] { "drain", "sound", "cover" })
            {
                FieldInfo field = typeof(InteractionArea).GetField(name);
                Assert.That(field, Is.Not.Null, name);
                ulong before = Fingerprint(area, typeof(InteractionArea));
                field.SetValue(area, 17);
                copy.CopyFrom(area);
                Assert.That(Scalar(copy, name), Is.EqualTo(17));
                Assert.That(Scalar(area.ShallowCopy(), name), Is.EqualTo(17));
                Assert.That(Fingerprint(area, typeof(InteractionArea)), Is.Not.EqualTo(before));
                Type type = typeof(BattleEcsHitExecutionPlan).GetNestedType("ItrProjection", BindingFlags.NonPublic);
                object projection = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null, new object[] { area }, null);
                Assert.That(type.GetField(char.ToUpperInvariant(name[0]) + name.Substring(1), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(projection), Is.EqualTo(17));
                Assert.That(Fingerprint(area, typeof(InteractionArea)), Is.EqualTo(Fingerprint(projection, type.MakeByRefType())));
                type.GetMethod("ApplyKind5Replacement", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(projection, new object[] { new InteractionArea() });
                Assert.That(type.GetField(char.ToUpperInvariant(name[0]) + name.Substring(1), BindingFlags.Instance | BindingFlags.NonPublic).GetValue(projection), Is.EqualTo(17));
                field.SetValue(area, 0);
                copy.CopyFrom(area);
                Assert.That(Scalar(copy, name), Is.Zero);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NativeFirstActionFeedsCurrentSignedActionReader(bool facesLeft)
        {
            var block = new Lf2DatSubBlock { Name = "itr" };
            block.AddProperty(new Lf2DatProperty("catchingact", "-23 99"));
            var area = (InteractionArea)Invoke("Interaction", block);
            Assert.That(area.catchingact, Is.EqualTo(new[] { -23 }));
            Type writer = typeof(SimulationWorld).Assembly.GetType("NTSD.Simulation.Ecs.BattleInteractionWriter");
            MethodInfo method = writer.GetMethod("ResolveRelationAction", BindingFlags.Static | BindingFlags.NonPublic);
            object[] args = { area.catchingact, facesLeft };
            Assert.That(method.Invoke(null, args), Is.EqualTo(23));
            Assert.That(args[1], Is.EqualTo(!facesLeft));
        }

        private static int Number(string text) => int.Parse(text, CultureInfo.InvariantCulture);
        private static int Scalar(object source, string name)
        {
            FieldInfo field = source.GetType().GetField(name);
            Assert.That(field, Is.Not.Null, name);
            object value = field.GetValue(source);
            return value is int[] pair ? (pair.Length == 0 ? 0 : pair[0]) : (int)value;
        }
        private static List<Lf2DatProperty> Fields(string name)
        {
            var fields = new List<Lf2DatProperty>();
            foreach (string line in File.ReadAllLines(Path.Combine(Root, "native-strength-fields.tsv")))
            {
                string[] columns = line.Split('\t');
                if (columns[0] != name + ".dat") continue;
                string hex = columns[3];
                var bytes = new byte[hex.Length / 2];
                for (int index = 0; index < bytes.Length; index++)
                    bytes[index] = byte.Parse(hex.Substring(index * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                fields.Add(new Lf2DatProperty(columns[2], Encoding.UTF8.GetString(bytes)));
            }
            Assert.That(fields.Count, Is.GreaterThan(0), name);
            return fields;
        }
        private static MethodInfo DecoderMethod(string name)
        {
            MethodInfo method = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganCombatRecordDecoder")
                .GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, name + " native decoder missing");
            return method;
        }
        private static object Invoke(string name, params object[] args) => DecoderMethod(name).Invoke(null, args);
        private static ulong Fingerprint(object value, Type type)
        {
            return (ulong)typeof(BattleEcsHitExecutionPlan).GetMethod("Fingerprint", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { type }, null).Invoke(null, new[] { value });
        }
    }
}
#endif
