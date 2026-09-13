#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05CpointContentEditorTests
    {
        private const string Witness = "artifacts/diagnostics/NTSD28-Q03-NUMERIC-DECODE-WITNESS-001";

        public static IEnumerable<TestCaseData> NativeCases()
        {
            foreach (string line in File.ReadAllLines(Path.Combine(Witness, "native.tsv")))
            {
                if (line.StartsWith("id\t", StringComparison.Ordinal)) continue;
                string[] values = line.Split('\t');
                yield return new TestCaseData(values[0], values).SetName("Cpoint27Native_" + values[0]);
            }
        }

        [TestCaseSource(nameof(NativeCases))]
        public void NativeValuesSurviveDtoAdapterAndCanonicalBits(string id, string[] expected)
        {
            var dat = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(Path.Combine(Witness, "fixtures", id + ".dat")));
            CatchPoint dto = Decode(dat.Frames[0].SubBlocks.Find(block => block.Name == "cpoint"));
            BattleCatchPointValue value = BattleCatchPointValueAdapter.FromLegacy(dto);
            int[] units = Canonical(value);
            Assert.That(Bits(value.ThrowVx), Is.EqualTo(expected[1]), id + "/vx");
            Assert.That(Bits(value.ThrowVy), Is.EqualTo(expected[2]), id + "/vy");
            Assert.That(Bits(value.ThrowVz), Is.EqualTo(expected[3]), id + "/vz");
            Assert.That(value.Injury, Is.EqualTo(int.Parse(expected[4], CultureInfo.InvariantCulture)));
            Assert.That(unchecked((uint)units[15]).ToString("X8"), Is.EqualTo(expected[1]));
            Assert.That(unchecked((uint)units[16]).ToString("X8"), Is.EqualTo(expected[2]));
            Assert.That(unchecked((uint)units[23]).ToString("X8"), Is.EqualTo(expected[3]));
        }

        [Test]
        public void ModelUsesFloat32AndAllTwentySevenFieldsHaveCanonicalSlots()
        {
            Assert.That(typeof(CatchPoint).GetField("throwvx").FieldType, Is.EqualTo(typeof(float)));
            Assert.That(typeof(BattleCatchPointValue).GetProperty("ThrowVx").PropertyType, Is.EqualTo(typeof(float)));
            CatchPoint dto = Decode(Block("kind: 1 x: 2 y: 3 injury: 4 cover: 5 vaction: 6 aaction: 7 jaction: 8 daction: 9 " +
                "taction: 10 faction: 11 baction: 12 uzaction: 13 dzaction: 14 throwvx: 1.5 throwvy: -2.25 " +
                "hurtable: 17 fronthurtact: 18 backhurtact: 19 decrease: 20 dircontrol: 21 throwinjury: 22 " +
                "throwvz: -0 z: 24 recover: 25 drain: 26 gain: 27"));
            BattleCatchPointValue value = BattleCatchPointValueAdapter.FromLegacy(dto);
            int[] expected = { 1, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14,
                0x3FC00000, unchecked((int)0xC0100000), 17, 18, 19, 20, 21, 22, int.MinValue, 24, 25, 26, 27 };
            Assert.That(Canonical(value), Is.EqualTo(expected));
            var source = new List<BattleCatchPointValue> { value, value };
            var catalog = new BattleCatchPointCatalog(source);
            source[0] = default;
            dto.injury = 999;
            Assert.That(catalog[0], Is.EqualTo(value));
            Assert.That(catalog[1], Is.EqualTo(value));
        }

        [Test]
        public void NewIntegerFieldsParticipateInEqualityAndHash()
        {
            string[] names = { "faction", "baction", "uzaction", "dzaction", "z", "recover", "drain", "gain" };
            var zero = BattleCatchPointValueAdapter.FromLegacy(Decode(Block("kind: 0")));
            foreach (string name in names)
            {
                var changed = BattleCatchPointValueAdapter.FromLegacy(Decode(Block(name + ": 1")));
                Assert.That(changed, Is.Not.EqualTo(zero), name);
                Assert.That(changed.GetHashCode(), Is.Not.EqualTo(zero.GetHashCode()), name);
            }
        }

        [Test]
        public void SignedZeroIdentityMatchesCanonicalBits()
        {
            var positive = BattleCatchPointValueAdapter.FromLegacy(Decode(Block("throwvx: 0")));
            var negative = BattleCatchPointValueAdapter.FromLegacy(Decode(Block("throwvx: -0")));
            var sameNegative = BattleCatchPointValueAdapter.FromLegacy(Decode(Block("throwvx: -0.0")));
            Assert.That(negative, Is.Not.EqualTo(positive));
            Assert.That(negative, Is.EqualTo(sameNegative));
            Assert.That(negative.GetHashCode(), Is.EqualTo(sameNegative.GetHashCode()));
            Assert.That(Canonical(negative), Is.Not.EqualTo(Canonical(positive)));
        }

        [Test]
        public void NativeAdmissionIgnoresUnknownAndUsesExactLastKeysWithoutHurtAliases()
        {
            Lf2DatSubBlock block = Block("injury: 7 injury: 12junk cover: 9 fronthurtact: 230 backhurtact: 232 drain: 600 unknown: 5");
            block.AddProperty(new Lf2DatProperty("INJURY", "999"));
            int count = block.Properties.Count;
            CatchPoint point = Decode(block);
            Assert.That(point.injury, Is.Zero);
            Assert.That(point.cover, Is.EqualTo(9));
            Assert.That(point.fronthurtact, Is.EqualTo(230));
            Assert.That(point.backhurtact, Is.EqualTo(232));
            Assert.That(block.Properties.Count, Is.EqualTo(count));
            Assert.That(block.Properties.Exists(property => property.Key == "unknown"), Is.True);
            Assert.That(point.rawProperties.ContainsKey("unknown"), Is.False);
            Assert.DoesNotThrow(() => BattleCatchPointValueAdapter.FromLegacy(point));
        }

        [Test]
        public void LegacyConverterAdmissionRemainsExplicitUntilSourceIntegration()
        {
            var frame = new Lf2FrameBlock { FrameIndex = 1 };
            frame.SubBlocks.Add(Block("drain: 600"));
            Assert.Throws<InvalidOperationException>(() => Lf2DatConverter.ConvertToFrameData(frame));
        }

        [Test]
        public void CanonicalWriterRejectsShortDestinationWithoutPartialWrite()
        {
            var catalog = new BattleCatchPointCatalog(new[] { default(BattleCatchPointValue) });
            var shortBuffer = new int[27];
            shortBuffer[0] = 999;
            Assert.Throws<ArgumentException>(() => catalog.CopyCanonicalScalars(shortBuffer, 0));
            Assert.That(shortBuffer[0], Is.EqualTo(999));
        }

        private static string Bits(float value) => unchecked((uint)BitConverter.SingleToInt32Bits(value)).ToString("X8");

        private static int[] Canonical(BattleCatchPointValue value)
        {
            var units = new int[28];
            Assert.That(new BattleCatchPointCatalog(new[] { value }).CopyCanonicalScalars(units, 0), Is.EqualTo(28));
            return units;
        }

        private static Lf2DatSubBlock Block(string properties)
        {
            return new Lf2DatParserV2().ParseLoganContent("<frame> 1 standing\ncpoint: " + properties + " cpoint_end:\n<frame_end>")
                .Frames[0].SubBlocks[0];
        }

        private static CatchPoint Decode(Lf2DatSubBlock block)
        {
            Type type = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganCombatRecordDecoder");
            Assert.That(type, Is.Not.Null, "Logan content block decoder is missing");
            MethodInfo method = type.GetMethod("CatchPoint", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (CatchPoint)method.Invoke(null, new object[] { block });
        }
    }
}
#endif
