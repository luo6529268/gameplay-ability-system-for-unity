#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    [Category("FormalKernelBodyBoxValueSeam")]
    public sealed class FormalKernelBodyBoxValueSeamEditorTests
    {
        private const string ExpectedCorpusSha256 =
            "309F4F41AAF152DCCA352A2ABEE4DBD49E0B13221C6734E3849404B6B32EE650";

        private const string GoldenCorpus =
            "schema=ntsd-bdy-cross-consumer-v1|fieldOrder=X,Y,W,H|encoding=utf8-lf\n" +
            "case=defaults|x=0|y=0|w=0|h=0\n" +
            "case=ordinary|x=12|y=34|w=56|h=78\n" +
            "case=raw-negative|x=-10|y=20|w=0|h=-30\n" +
            "case=int-extremes|x=-2147483648|y=2147483647|w=-2147483648|h=2147483647\n" +
            "case=full-height-sentinel|x=-101|y=-2147483648|w=900|h=0\n" +
            "case=full-height-x-boundary|x=-100|y=-2147483648|w=900|h=0\n" +
            "case=full-height-w-boundary|x=-101|y=-2147483648|w=899|h=0\n" +
            "sequence=duplicate|index=0|x=5|y=6|w=7|h=8\n" +
            "sequence=duplicate|index=1|x=-5|y=-6|w=7|h=8\n";

        [Test]
        public void ValuePreservesExactFourScalarsAndIsImmutable()
        {
            var value = new BattleBodyBoxValue(-31, 47, 59, -61);

            Assert.That(value.X, Is.EqualTo(-31));
            Assert.That(value.Y, Is.EqualTo(47));
            Assert.That(value.W, Is.EqualTo(59));
            Assert.That(value.H, Is.EqualTo(-61));

            PropertyInfo[] properties = typeof(BattleBodyBoxValue).GetProperties(
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(properties, Has.Length.EqualTo(4));
            for (int index = 0; index < properties.Length; index++)
                Assert.That(properties[index].CanWrite, Is.False, properties[index].Name);
            Assert.That(typeof(BattleBodyBoxValue).GetFields(
                BindingFlags.Instance | BindingFlags.Public), Is.Empty);

            var equal = new BattleBodyBoxValue(-31, 47, 59, -61);
            Assert.That(value, Is.EqualTo(equal));
            Assert.That(value.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(value == equal, Is.True);
            Assert.That(value != equal, Is.False);
        }

        [Test]
        public void AdapterCopiesOnlyFormalFieldsAndResetsLegacyExtras()
        {
            var legacy = new BodyBox
            {
                kind = 17,
                x = -10,
                y = 20,
                w = 0,
                h = -30,
            };
            legacy.rawProperties["kind"] = "17";
            legacy.rawProperties["custom"] = "drop";

            BattleBodyBoxValue value =
                BattleBodyBoxValueAdapter.FromLegacy(legacy);
            BodyBox projected = BattleBodyBoxValueAdapter.ToLegacy(value);

            Assert.That(value, Is.EqualTo(new BattleBodyBoxValue(-10, 20, 0, -30)));
            Assert.That(projected.kind, Is.Zero);
            Assert.That(projected.x, Is.EqualTo(-10));
            Assert.That(projected.y, Is.EqualTo(20));
            Assert.That(projected.w, Is.Zero);
            Assert.That(projected.h, Is.EqualTo(-30));
            Assert.That(projected.rawProperties, Is.Empty);

            projected.x = 999;
            Assert.That(value.X, Is.EqualTo(-10));
        }

        [Test]
        public void ConverterPreservesSourceOrderDuplicatesAndRawGeometry()
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 77 };
            frameBlock.SubBlocks.Add(BuildBody(7, -10, 20, 0, -30, "first"));
            frameBlock.SubBlocks.Add(BuildBody(8, -101, int.MinValue, 900, 0, "sentinel"));
            frameBlock.SubBlocks.Add(BuildBody(9, -10, 20, 0, -30, "duplicate"));

            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);

            Assert.That(frame.bodies, Is.TypeOf<List<BattleBodyBoxValue>>());
            Assert.That(frame.bodies, Has.Count.EqualTo(3));
            Assert.That(frame.bodies[0], Is.EqualTo(
                new BattleBodyBoxValue(-10, 20, 0, -30)));
            Assert.That(frame.bodies[1], Is.EqualTo(
                new BattleBodyBoxValue(-101, int.MinValue, 900, 0)));
            Assert.That(frame.bodies[2], Is.EqualTo(frame.bodies[0]));
        }

        [Test]
        public void LegacyFixtureProjectionDropsKindAndRawProperties()
        {
            var frame = new LF2FrameData();
            var legacy = new BodyBox
            {
                kind = 5,
                x = 12,
                y = 34,
                w = 56,
                h = 78,
            };
            legacy.rawProperties["debug"] = "not-formal";

            frame.bodies.Add(legacy);

            Assert.That(frame.bodies, Has.Count.EqualTo(1));
            Assert.That(frame.bodies[0], Is.EqualTo(
                new BattleBodyBoxValue(12, 34, 56, 78)));
        }

        [Test]
        public void AdapterCopyToLegacyIsWarmedAllocationFree()
        {
            var value = new BattleBodyBoxValue(-1, 2, 3, -4);
            var destination = new BodyBox();
            BattleBodyBoxValueAdapter.CopyToLegacy(value, destination);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
                BattleBodyBoxValueAdapter.CopyToLegacy(value, destination);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(destination.kind, Is.Zero);
            Assert.That(destination.rawProperties, Is.Empty);
        }

        [Test]
        public void GoldenCorpusMatchesFrozenDigestAndFullHeightBoundaries()
        {
            byte[] payload = Encoding.UTF8.GetBytes(GoldenCorpus);
            using var sha = SHA256.Create();
            string digest = BitConverter.ToString(sha.ComputeHash(payload))
                .Replace("-", string.Empty);
            Assert.That(digest, Is.EqualTo(ExpectedCorpusSha256));

            string[] lines = GoldenCorpus.TrimEnd('\n').Split('\n');
            Assert.That(lines, Has.Length.EqualTo(10));
            Assert.That(lines[0], Is.EqualTo(
                "schema=ntsd-bdy-cross-consumer-v1|" +
                "fieldOrder=X,Y,W,H|encoding=utf8-lf"));

            var expected = new[]
            {
                default(BattleBodyBoxValue),
                new BattleBodyBoxValue(12, 34, 56, 78),
                new BattleBodyBoxValue(-10, 20, 0, -30),
                new BattleBodyBoxValue(
                    int.MinValue,
                    int.MaxValue,
                    int.MinValue,
                    int.MaxValue),
                new BattleBodyBoxValue(-101, int.MinValue, 900, 0),
                new BattleBodyBoxValue(-100, int.MinValue, 900, 0),
                new BattleBodyBoxValue(-101, int.MinValue, 899, 0),
                new BattleBodyBoxValue(5, 6, 7, 8),
                new BattleBodyBoxValue(-5, -6, 7, 8),
            };

            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(ParseCorpusValue(lines[index + 1]),
                    Is.EqualTo(expected[index]),
                    lines[index + 1]);
            }

            Assert.That(IsFullHeight(expected[4]), Is.True);
            Assert.That(IsFullHeight(expected[5]), Is.False);
            Assert.That(IsFullHeight(expected[6]), Is.False);
        }

        private static Lf2DatSubBlock BuildBody(
            int kind,
            int x,
            int y,
            int w,
            int h,
            string custom)
        {
            var block = new Lf2DatSubBlock { Name = "bdy" };
            block.AddProperty(new Lf2DatProperty("kind", kind.ToString()));
            block.AddProperty(new Lf2DatProperty("x", x.ToString()));
            block.AddProperty(new Lf2DatProperty("y", y.ToString()));
            block.AddProperty(new Lf2DatProperty("w", w.ToString()));
            block.AddProperty(new Lf2DatProperty("h", h.ToString()));
            block.AddProperty(new Lf2DatProperty("custom", custom));
            return block;
        }

        private static BattleBodyBoxValue ParseCorpusValue(string line)
        {
            int x = 0;
            int y = 0;
            int w = 0;
            int h = 0;

            string[] parts = line.Split('|');
            for (int index = 1; index < parts.Length; index++)
            {
                int separator = parts[index].IndexOf('=');
                if (separator <= 0)
                    continue;

                string key = parts[index].Substring(0, separator);
                string rawValue = parts[index].Substring(separator + 1);
                if (!int.TryParse(
                        rawValue,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out int value))
                {
                    continue;
                }

                switch (key)
                {
                    case "x": x = value; break;
                    case "y": y = value; break;
                    case "w": w = value; break;
                    case "h": h = value; break;
                }
            }

            return new BattleBodyBoxValue(x, y, w, h);
        }

        private static bool IsFullHeight(BattleBodyBoxValue value)
        {
            return value.Y == int.MinValue &&
                   value.X < -100 &&
                   value.W >= 900;
        }
    }
}
#endif

