#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05GeometryContentEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-GEOMETRY-CONTENT-CONTRACT-001";

        public static IEnumerable<TestCaseData> NativeCases()
        {
            foreach (string line in File.ReadAllLines(Path.Combine(Root, "native.tsv")))
            {
                if (line.StartsWith("path\t", StringComparison.Ordinal)) continue;
                string[] values = line.Split('\t');
                yield return new TestCaseData(new object[] { values })
                    .SetName("NativeGeometry_" + values[0] + "_" + values[1] + "_" + values[2]);
            }
        }

        [TestCaseSource(nameof(NativeCases))]
        public void NativeGeometryValidityAndRawCoordinatesMatch(string[] expected)
        {
            var dat = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(Path.Combine(Root, "fixtures", expected[0])));
            var block = dat.Frames[Number(expected[1])].SubBlocks[Number(expected[2])];
            int originalCount = block.Properties.Count;
            if (expected[3] == "bdy")
            {
                BattleBodyBoxValue value = Body(block);
                Assert.That(value.X, Is.EqualTo(Number(expected[5])));
                Assert.That(value.Y, Is.EqualTo(Number(expected[6])));
                Assert.That(value.W, Is.EqualTo(Number(expected[7])));
                Assert.That(value.H, Is.EqualTo(Number(expected[8])));
                Assert.That(Property(value, "ZWidth"), Is.EqualTo(Number(expected[9])));
                Assert.That(Property(value, "HasGeometry"), Is.EqualTo(expected[11] == "1"));
                BodyBox dto = BattleBodyBoxValueAdapter.ToLegacy(value);
                Assert.That(BattleBodyBoxValueAdapter.FromLegacy(dto), Is.EqualTo(value));
                Assert.That(Field(dto, "zwidth"), Is.EqualTo(Number(expected[9])));
                Assert.That(Field(dto, "hasGeometry"), Is.EqualTo(expected[11] == "1"));
            }
            else
            {
                var area = new InteractionArea { injury = 777, dz = 29, dvz = 31 };
                Apply(block, area);
                Assert.That(area.kind, Is.EqualTo(Number(expected[4])));
                Assert.That(area.x, Is.EqualTo(Number(expected[5])));
                Assert.That(area.y, Is.EqualTo(Number(expected[6])));
                Assert.That(area.w, Is.EqualTo(Number(expected[7])));
                Assert.That(area.h, Is.EqualTo(Number(expected[8])));
                Assert.That(area.zwidth, Is.EqualTo(Number(expected[9])));
                Assert.That(Field(area, "z"), Is.EqualTo(Number(expected[10])));
                Assert.That(Field(area, "hasGeometry"), Is.EqualTo(expected[11] == "1"));
                Assert.That(area.kind == 100100 && !(bool)Field(area, "hasGeometry"), Is.EqualTo(expected[12] == "1"));
                Assert.That(area.injury, Is.EqualTo(777));
                Assert.That(area.dz, Is.EqualTo(29));
                Assert.That(area.dvz, Is.EqualTo(31));
            }
            Assert.That(block.Properties.Count, Is.EqualTo(originalCount));
        }

        [Test]
        public void NativeMissingIsDistinctFromExplicitZeroWhileLegacyDefaultRemainsStable()
        {
            BattleBodyBoxValue missing = Body(Block("bdy", ""));
            BattleBodyBoxValue zero = Body(Block("bdy", "x: 0 y: 0 w: 0 h: 0"));
            Assert.That(Property(missing, "HasGeometry"), Is.False);
            Assert.That(Property(zero, "HasGeometry"), Is.True);
            Assert.That(missing, Is.Not.EqualTo(zero));
            Assert.That(missing.GetHashCode(), Is.Not.EqualTo(zero.GetHashCode()));
            Assert.That(zero, Is.EqualTo(default(BattleBodyBoxValue)));
            Assert.That(zero, Is.EqualTo(BattleBodyBoxValueAdapter.FromLegacy(new BodyBox())));
        }

        [Test]
        public void BodyIdentityIncludesDepthButLocalBodyZDoesNotBecomeAnOffset()
        {
            const string rectangle = "x: 1 y: 2 w: -3 h: 0";
            BattleBodyBoxValue first = Body(Block("bdy", rectangle + " zwidth: 9 z: -100"));
            BattleBodyBoxValue same = Body(Block("bdy", rectangle + " zwidth: 9 z: 200"));
            BattleBodyBoxValue changed = Body(Block("bdy", rectangle + " zwidth: 10"));
            Assert.That(Property(first, "HasGeometry"), Is.True);
            Assert.That(first, Is.EqualTo(same));
            Assert.That(first.GetHashCode(), Is.EqualTo(same.GetHashCode()));
            Assert.That(first, Is.Not.EqualTo(changed));
            Assert.That(first.GetHashCode(), Is.Not.EqualTo(changed.GetHashCode()));
        }

        [Test]
        public void CopyAndCloneOverwritePriorGeometryAndDoNotAliasFields()
        {
            var source = new InteractionArea();
            var copy = new InteractionArea();
            Apply(Block("itr", "x: 1 y: 2 w: 3 z: -17 zwidth: 19"), source);
            copy.CopyFrom(source);
            InteractionArea clone = source.ShallowCopy();
            Assert.That(Field(copy, "z"), Is.EqualTo(-17));
            Assert.That(Field(copy, "hasGeometry"), Is.False);
            Assert.That(Field(clone, "z"), Is.EqualTo(-17));
            Assert.That(Field(clone, "hasGeometry"), Is.False);
            Apply(Block("itr", "x: 0 y: 0 w: 0 h: 0"), source);
            copy.CopyFrom(source);
            Assert.That(Field(copy, "z"), Is.EqualTo(0));
            Assert.That(Field(copy, "hasGeometry"), Is.True);
            Assert.That(copy.zwidth, Is.Zero);
            Assert.That(Field(clone, "z"), Is.EqualTo(-17));
            Assert.That(Field(clone, "hasGeometry"), Is.False);
        }

        [Test]
        public void ProjectionAndBothFingerprintsPreserveDepthAndValidity()
        {
            var source = new InteractionArea();
            SetField(source, "z", -17);
            SetField(source, "hasGeometry", false);
            Type type = typeof(BattleEcsHitExecutionPlan).GetNestedType("ItrProjection", BindingFlags.NonPublic);
            object projection = Activator.CreateInstance(type, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, new object[] { source }, null);
            Assert.That(Field(projection, "Z"), Is.EqualTo(-17));
            Assert.That(Field(projection, "HasGeometry"), Is.False);
            Assert.That(Fingerprint(source, typeof(InteractionArea)), Is.EqualTo(Fingerprint(projection, type.MakeByRefType())));
            ulong original = Fingerprint(source, typeof(InteractionArea));
            SetField(source, "z", 0);
            Assert.That(Fingerprint(source, typeof(InteractionArea)), Is.Not.EqualTo(original));
            ulong missing = Fingerprint(source, typeof(InteractionArea));
            SetField(source, "hasGeometry", true);
            Assert.That(Fingerprint(source, typeof(InteractionArea)), Is.Not.EqualTo(missing));

            var replacement = new InteractionArea { zwidth = 75 };
            SetField(replacement, "z", 99);
            type.GetMethod("ApplyKind5Replacement", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(projection, new object[] { replacement });
            Assert.That(Field(projection, "Z"), Is.EqualTo(-17));
            Assert.That(Field(projection, "HasGeometry"), Is.False);
        }

        private static int Number(string value) => int.Parse(value, CultureInfo.InvariantCulture);
        private static object Property(object value, string name)
        {
            PropertyInfo property = value.GetType().GetProperty(name);
            Assert.That(property, Is.Not.Null, name);
            return property.GetValue(value);
        }
        private static object Field(object value, string name)
        {
            FieldInfo field = value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            return field.GetValue(value);
        }
        private static void SetField(object value, string name, object fieldValue)
        {
            FieldInfo field = value.GetType().GetField(name);
            Assert.That(field, Is.Not.Null, name);
            field.SetValue(value, fieldValue);
        }
        private static ulong Fingerprint(object value, Type type)
        {
            return (ulong)typeof(BattleEcsHitExecutionPlan).GetMethod("Fingerprint", BindingFlags.Static | BindingFlags.NonPublic,
                null, new[] { type }, null).Invoke(null, new[] { value });
        }
        private static Lf2DatSubBlock Block(string kind, string fields)
        {
            return new Lf2DatParserV2().ParseLoganContent("<frame> 0 test\n" + kind + ": " + fields + " " + kind + "_end:\n<frame_end>").Frames[0].SubBlocks[0];
        }
        private static BattleBodyBoxValue Body(Lf2DatSubBlock block)
        {
            MethodInfo method = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganCombatRecordDecoder")
                .GetMethod("BodyBox", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Native body geometry decoder missing");
            return (BattleBodyBoxValue)method.Invoke(null, new object[] { block });
        }
        private static void Apply(Lf2DatSubBlock block, InteractionArea target)
        {
            MethodInfo method = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganCombatRecordDecoder")
                .GetMethod("ApplyInteractionGeometry", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Native interaction geometry decoder missing");
            method.Invoke(null, new object[] { block, target });
        }
    }
}
#endif
