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
    [Category("FormalKernelObjectPointValueSeam")]
    public sealed class FormalKernelObjectPointValueSeamEditorTests
    {
        private const string ExpectedCorpusSha256 =
            "2363910A2686D28D5FDE161C00C1777717408FD0736AEF3D5AB7A7CC57C7360E";

        private const string GoldenCorpus =
            "schema=ntsd-opoint-cross-consumer-v1|fieldOrder=Kind,X,Y,Action,Dvx,Dvy,Oid,Facing|encoding=utf8-lf\n" +
            "case=defaults|kind=0|x=0|y=0|action=0|dvx=0|dvy=0|oid=0|facing=0\n" +
            "case=ordinary|kind=1|x=37|y=-12|action=70|dvx=8|dvy=-3|oid=211|facing=0\n" +
            "case=signed|kind=-3|x=-1000|y=900|action=-1|dvx=-25|dvy=19|oid=-7|facing=-2\n" +
            "case=int-extremes|kind=-2147483648|x=2147483647|y=-2147483648|action=2147483647|dvx=-2147483648|dvy=2147483647|oid=-2147483648|facing=2147483647\n" +
            "case=multi-spawn|kind=1|x=4|y=5|action=6|dvx=7|dvy=8|oid=212|facing=31\n" +
            "case=kind2-holder|kind=2|x=10|y=20|action=30|dvx=4|dvy=-5|oid=213|facing=1\n" +
            "case=invalid-preserved|kind=1|x=2|y=3|action=4|dvx=5|dvy=6|oid=0|facing=0\n" +
            "sequence=duplicate|index=0|kind=1|x=10|y=11|action=12|dvx=13|dvy=14|oid=215|facing=0\n" +
            "sequence=duplicate|index=1|kind=1|x=-10|y=-11|action=22|dvx=-13|dvy=-14|oid=215|facing=1\n";

        [Test]
        public void ValuePreservesExactEightScalarsAndIsImmutable()
        {
            var value = new BattleObjectPointValue(
                kind: 2,
                x: -31,
                y: 47,
                action: 240,
                dvx: -12,
                dvy: 19,
                oid: 999,
                facing: 31);

            Assert.That(value.Kind, Is.EqualTo(2));
            Assert.That(value.X, Is.EqualTo(-31));
            Assert.That(value.Y, Is.EqualTo(47));
            Assert.That(value.Action, Is.EqualTo(240));
            Assert.That(value.Dvx, Is.EqualTo(-12));
            Assert.That(value.Dvy, Is.EqualTo(19));
            Assert.That(value.Oid, Is.EqualTo(999));
            Assert.That(value.Facing, Is.EqualTo(31));

            PropertyInfo[] properties = typeof(BattleObjectPointValue).GetProperties(
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(properties, Has.Length.EqualTo(8));
            for (int index = 0; index < properties.Length; index++)
                Assert.That(properties[index].CanWrite, Is.False, properties[index].Name);
            Assert.That(typeof(BattleObjectPointValue).GetFields(
                BindingFlags.Instance | BindingFlags.Public), Is.Empty);

            var equal = new BattleObjectPointValue(2, -31, 47, 240, -12, 19, 999, 31);
            Assert.That(value, Is.EqualTo(equal));
            Assert.That(value.GetHashCode(), Is.EqualTo(equal.GetHashCode()));
            Assert.That(value == equal, Is.True);
            Assert.That(value != equal, Is.False);
        }

        [Test]
        public void AdapterCopiesOnlyFormalFieldsAndLeavesCanonicalValueUnchanged()
        {
            var legacy = new ObjectPoint
            {
                kind = 1,
                x = 10,
                y = -20,
                action = 307,
                dvx = 8,
                dvy = -9,
                oid = 733,
                facing = 31,
                objectId = 123456,
                dvz = -654321,
            };

            BattleObjectPointValue value =
                BattleObjectPointValueAdapter.FromLegacyTask(legacy);
            ObjectPoint taskValue =
                BattleObjectPointValueAdapter.ToLegacyTask(value);

            Assert.That(value, Is.EqualTo(new BattleObjectPointValue(
                1, 10, -20, 307, 8, -9, 733, 31)));
            Assert.That(taskValue.kind, Is.EqualTo(1));
            Assert.That(taskValue.x, Is.EqualTo(10));
            Assert.That(taskValue.y, Is.EqualTo(-20));
            Assert.That(taskValue.action, Is.EqualTo(307));
            Assert.That(taskValue.dvx, Is.EqualTo(8));
            Assert.That(taskValue.dvy, Is.EqualTo(-9));
            Assert.That(taskValue.oid, Is.EqualTo(733));
            Assert.That(taskValue.facing, Is.EqualTo(31));
            Assert.That(taskValue.objectId, Is.Zero);
            Assert.That(taskValue.dvz, Is.Zero);

            taskValue.facing = 1;
            Assert.That(value.Facing, Is.EqualTo(31));
        }

        [Test]
        public void ConverterPreservesSourceOrderDuplicatesInvalidEntriesAndFirstAlias()
        {
            var frameBlock = new Lf2FrameBlock { FrameIndex = 88 };
            frameBlock.SubBlocks.Add(BuildOpoint(
                kind: 1,
                x: 10,
                y: 20,
                action: 30,
                dvx: 40,
                dvy: 50,
                oid: 60,
                facing: 31,
                objectId: 700,
                dvz: 800));
            frameBlock.SubBlocks.Add(BuildOpoint(
                kind: 0,
                x: -10,
                y: -20,
                action: -30,
                dvx: -40,
                dvy: -50,
                oid: 0,
                facing: -1,
                objectId: 900,
                dvz: 1000));
            frameBlock.SubBlocks.Add(BuildOpoint(
                kind: 1,
                x: 10,
                y: 20,
                action: 30,
                dvx: 40,
                dvy: 50,
                oid: 60,
                facing: 31,
                objectId: 1100,
                dvz: 1200));

            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(frameBlock);

            Assert.That(frame.opoints, Is.TypeOf<List<BattleObjectPointValue>>());
            Assert.That(frame.opoints, Has.Count.EqualTo(3));
            Assert.That(frame.opoints[0], Is.EqualTo(
                new BattleObjectPointValue(1, 10, 20, 30, 40, 50, 60, 31)));
            Assert.That(frame.opoints[1], Is.EqualTo(
                new BattleObjectPointValue(0, -10, -20, -30, -40, -50, 0, -1)));
            Assert.That(frame.opoints[2], Is.EqualTo(frame.opoints[0]));
            Assert.That(frame.opoint.HasValue, Is.True);
            Assert.That(frame.opoint.Value, Is.EqualTo(frame.opoints[0]));
        }

        [Test]
        public void LegacyFixtureProjectionSupportsAliasAndOrderedListWithoutExtras()
        {
            var frame = new LF2FrameData
            {
                opoint = new ObjectPoint
                {
                    kind = 2,
                    x = 3,
                    y = 4,
                    action = 5,
                    dvx = 6,
                    dvy = 7,
                    oid = 8,
                    facing = 9,
                    objectId = 100,
                    dvz = 200,
                },
            };
            frame.opoints.Add(new ObjectPoint
            {
                kind = 1,
                x = -3,
                y = -4,
                action = -5,
                dvx = -6,
                dvy = -7,
                oid = 9,
                facing = 21,
                objectId = 300,
                dvz = 400,
            });

            Assert.That(frame.opoint.Value, Is.EqualTo(
                new BattleObjectPointValue(2, 3, 4, 5, 6, 7, 8, 9)));
            Assert.That(frame.opoints[0], Is.EqualTo(
                new BattleObjectPointValue(1, -3, -4, -5, -6, -7, 9, 21)));
        }

        [Test]
        public void AdapterIsWarmedAllocationFree()
        {
            var legacy = new ObjectPoint
            {
                kind = 2,
                x = 1,
                y = 2,
                action = 3,
                dvx = 4,
                dvy = 5,
                oid = 6,
                facing = 7,
            };
            BattleObjectPointValue value =
                BattleObjectPointValueAdapter.FromLegacyTask(legacy);
            BattleObjectPointValueAdapter.ToLegacyTask(value);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 1024; index++)
            {
                value = BattleObjectPointValueAdapter.FromLegacyTask(legacy);
                legacy = BattleObjectPointValueAdapter.ToLegacyTask(value);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
        }

        [Test]
        public void GoldenCorpusMatchesFrozenDigestAndProjectsEveryVector()
        {
            byte[] payload = Encoding.UTF8.GetBytes(GoldenCorpus);
            using var sha = SHA256.Create();
            string digest = BitConverter.ToString(sha.ComputeHash(payload))
                .Replace("-", string.Empty);
            Assert.That(digest, Is.EqualTo(ExpectedCorpusSha256));

            string[] lines = GoldenCorpus.TrimEnd('\n').Split('\n');
            Assert.That(lines, Has.Length.EqualTo(10));
            Assert.That(lines[0], Is.EqualTo(
                "schema=ntsd-opoint-cross-consumer-v1|" +
                "fieldOrder=Kind,X,Y,Action,Dvx,Dvy,Oid,Facing|" +
                "encoding=utf8-lf"));

            var expected = new[]
            {
                default(BattleObjectPointValue),
                new BattleObjectPointValue(1, 37, -12, 70, 8, -3, 211, 0),
                new BattleObjectPointValue(-3, -1000, 900, -1, -25, 19, -7, -2),
                new BattleObjectPointValue(
                    int.MinValue,
                    int.MaxValue,
                    int.MinValue,
                    int.MaxValue,
                    int.MinValue,
                    int.MaxValue,
                    int.MinValue,
                    int.MaxValue),
                new BattleObjectPointValue(1, 4, 5, 6, 7, 8, 212, 31),
                new BattleObjectPointValue(2, 10, 20, 30, 4, -5, 213, 1),
                new BattleObjectPointValue(1, 2, 3, 4, 5, 6, 0, 0),
                new BattleObjectPointValue(1, 10, 11, 12, 13, 14, 215, 0),
                new BattleObjectPointValue(1, -10, -11, 22, -13, -14, 215, 1),
            };

            for (int index = 0; index < expected.Length; index++)
            {
                Assert.That(ParseCorpusValue(lines[index + 1]),
                    Is.EqualTo(expected[index]),
                    lines[index + 1]);
            }
        }

        private static Lf2DatSubBlock BuildOpoint(
            int kind,
            int x,
            int y,
            int action,
            int dvx,
            int dvy,
            int oid,
            int facing,
            int objectId,
            int dvz)
        {
            var block = new Lf2DatSubBlock { Name = "opoint" };
            block.AddProperty(new Lf2DatProperty("kind", kind.ToString()));
            block.AddProperty(new Lf2DatProperty("x", x.ToString()));
            block.AddProperty(new Lf2DatProperty("y", y.ToString()));
            block.AddProperty(new Lf2DatProperty("action", action.ToString()));
            block.AddProperty(new Lf2DatProperty("dvx", dvx.ToString()));
            block.AddProperty(new Lf2DatProperty("dvy", dvy.ToString()));
            block.AddProperty(new Lf2DatProperty("oid", oid.ToString()));
            block.AddProperty(new Lf2DatProperty("facing", facing.ToString()));
            block.AddProperty(new Lf2DatProperty("objectid", objectId.ToString()));
            block.AddProperty(new Lf2DatProperty("dvz", dvz.ToString()));
            return block;
        }

        private static BattleObjectPointValue ParseCorpusValue(string line)
        {
            int kind = 0;
            int x = 0;
            int y = 0;
            int action = 0;
            int dvx = 0;
            int dvy = 0;
            int oid = 0;
            int facing = 0;

            string[] parts = line.Split('|');
            for (int index = 1; index < parts.Length; index++)
            {
                int separator = parts[index].IndexOf('=');
                if (separator <= 0)
                    continue;

                string key = parts[index].Substring(0, separator);
                string rawValue = parts[index].Substring(separator + 1);
                int value;
                if (!int.TryParse(
                        rawValue,
                        NumberStyles.Integer,
                        CultureInfo.InvariantCulture,
                        out value))
                {
                    continue;
                }

                switch (key)
                {
                    case "kind": kind = value; break;
                    case "x": x = value; break;
                    case "y": y = value; break;
                    case "action": action = value; break;
                    case "dvx": dvx = value; break;
                    case "dvy": dvy = value; break;
                    case "oid": oid = value; break;
                    case "facing": facing = value; break;
                }
            }

            return new BattleObjectPointValue(
                kind,
                x,
                y,
                action,
                dvx,
                dvy,
                oid,
                facing);
        }
    }
}
#endif
