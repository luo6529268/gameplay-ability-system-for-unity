#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28Q05OpointContentEditorTests
    {
        private const string Root = "artifacts/diagnostics/NTSD28-Q05-OPOINT24-CONTENT-CONTRACT-001";
        private static readonly string[] Keys = "kind x y z action dvx dvy dvz oid facing hp mp team reserve effect pic centerx centery centerz framea attacking join join_reserve join_pic".Split(' ');
        private static readonly string[] Properties = "Kind X Y Z Action Dvx Dvy Dvz Oid Facing Hp Mp Team Reserve Effect Pic CenterX CenterY CenterZ FrameA Attacking Join JoinReserve JoinPic".Split(' ');

        public static IEnumerable<TestCaseData> NativeCases()
        {
            foreach (string line in File.ReadAllLines(Path.Combine(Root, "native.tsv")))
            {
                if (line.StartsWith("id\t", StringComparison.Ordinal)) continue;
                string[] columns = line.Split('\t');
                yield return new TestCaseData(new object[] { columns }).SetName("Opoint24Native_" + columns[0] + "_" + columns[1]);
            }
        }

        [TestCaseSource(nameof(NativeCases))]
        public void NativeFieldsSurviveValueAndTaskRoundTrip(string[] columns)
        {
            var dat = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(Path.Combine(Root, "fixtures", columns[0] + ".dat")));
            BattleObjectPointValue value = Decode(dat.Frames[0].SubBlocks[int.Parse(columns[1], CultureInfo.InvariantCulture)]);
            ObjectPoint dto = BattleObjectPointValueAdapter.ToLegacyTask(value);
            Assert.That(BattleObjectPointValueAdapter.FromLegacyTask(dto), Is.EqualTo(value));
            Assert.That(dto.objectId, Is.Zero);
            for (int i = 0; i < Keys.Length; i++)
            {
                PropertyInfo property = typeof(BattleObjectPointValue).GetProperty(Properties[i]);
                FieldInfo field = typeof(ObjectPoint).GetField(Keys[i]);
                Assert.That(property, Is.Not.Null, Properties[i]);
                Assert.That(field, Is.Not.Null, Keys[i]);
                int expected = int.Parse(columns[i + 2], CultureInfo.InvariantCulture);
                Assert.That(property.GetValue(value), Is.EqualTo(expected), Properties[i]);
                Assert.That(field.GetValue(dto), Is.EqualTo(expected), Keys[i]);
            }
        }

        [Test]
        public void TwentyFourFieldsAreImmutableInt32AndParticipateInIdentity()
        {
            PropertyInfo[] properties = typeof(BattleObjectPointValue).GetProperties();
            Assert.That(properties.Length, Is.EqualTo(24));
            foreach (PropertyInfo property in properties)
            {
                Assert.That(property.PropertyType, Is.EqualTo(typeof(int)), property.Name);
                Assert.That(property.CanWrite, Is.False, property.Name);
            }
            foreach (string key in Keys)
            {
                BattleObjectPointValue value = Decode(Block(key + ": 17"));
                Assert.That(value, Is.Not.EqualTo(default(BattleObjectPointValue)), key);
                Assert.That(value.GetHashCode(), Is.Not.EqualTo(default(BattleObjectPointValue).GetHashCode()), key);
                Assert.That(value, Is.EqualTo(Decode(Block(key + ": 17"))));
            }
        }

        [Test]
        public void MultipleToSingleCopiesCompletePayloadAndSeparatesTaskSpreadAndTeam()
        {
            BattleObjectPointValue value = OrderedValue();
            var source = new OPointCreateMultipleTask
            {
                opoint = BattleObjectPointValueAdapter.ToLegacyTask(value), team = 99, dvz = 77f,
            };
            var target = new OPointCreateTask();
            Type type = typeof(SimulationWorld).Assembly.GetType("NTSD.Simulation.BattleLogicObjectPointRuntime");
            MethodInfo copy = type.GetMethod("CopyMultipleTaskToSingle", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(copy, Is.Not.Null);
            copy.Invoke(null, new object[] { source, target, 2.5f });
            Assert.That(BattleObjectPointValueAdapter.FromLegacyTask(target.opoint), Is.EqualTo(value));
            Assert.That(target.team, Is.EqualTo(99));
            Assert.That(target.dvz, Is.EqualTo(2.5f));
            Assert.That(target.opoint.dvz, Is.EqualTo(8));
            source.opoint = default;
            Assert.That(BattleObjectPointValueAdapter.FromLegacyTask(target.opoint), Is.EqualTo(value));
        }

        [Test]
        public void PoolRecycleAndReuseClearEveryFieldInBothTaskKinds()
        {
            BattleObjectPointValue value = OrderedValue();
            var pool = new BattleLogicReferencePool();
            var single = pool.Fetch<OPointCreateTask>();
            var multiple = pool.Fetch<OPointCreateMultipleTask>();
            single.opoint = BattleObjectPointValueAdapter.ToLegacyTask(value);
            multiple.opoint = single.opoint;
            pool.Recycle(single);
            pool.Recycle(multiple);
            var reusedSingle = pool.Fetch<OPointCreateTask>();
            var reusedMultiple = pool.Fetch<OPointCreateMultipleTask>();
            Assert.That(reusedSingle, Is.SameAs(single));
            Assert.That(reusedMultiple, Is.SameAs(multiple));
            Assert.That(BattleObjectPointValueAdapter.FromLegacyTask(reusedSingle.opoint), Is.EqualTo(default(BattleObjectPointValue)));
            Assert.That(BattleObjectPointValueAdapter.FromLegacyTask(reusedMultiple.opoint), Is.EqualTo(default(BattleObjectPointValue)));
            foreach (FieldInfo field in typeof(ObjectPoint).GetFields())
            {
                Assert.That(field.GetValue(reusedSingle.opoint), Is.EqualTo(0), field.Name);
                Assert.That(field.GetValue(reusedMultiple.opoint), Is.EqualTo(0), field.Name);
            }
            reusedSingle.opoint = BattleObjectPointValueAdapter.ToLegacyTask(value);
            Assert.That(BattleObjectPointValueAdapter.FromLegacyTask(reusedSingle.opoint), Is.EqualTo(value));
            pool.Recycle(reusedSingle);
            pool.Recycle(reusedMultiple);
        }

        [Test]
        public void ExactLastKeysIgnoreUnknownWithoutMutatingAstOrUsingRuntimeObjectId()
        {
            Lf2DatSubBlock block = Block("oid: 17 hp: 100 hp: 12junk OID: 999 objectid: 555 unknown: 31");
            int count = block.Properties.Count;
            BattleObjectPointValue value = Decode(block);
            ObjectPoint dto = BattleObjectPointValueAdapter.ToLegacyTask(value);
            Assert.That(value.Oid, Is.EqualTo(17));
            Assert.That(typeof(BattleObjectPointValue).GetProperty("Hp").GetValue(value), Is.EqualTo(0));
            Assert.That(dto.objectId, Is.Zero);
            Assert.That(block.Properties.Count, Is.EqualTo(count));
            Assert.That(block.Properties.Exists(property => property.Key == "unknown"), Is.True);
            dto.objectId = 8123;
            Assert.That(BattleObjectPointValueAdapter.FromLegacyTask(dto), Is.EqualTo(value));
        }

        private static BattleObjectPointValue OrderedValue()
        {
            var dat = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(Path.Combine(Root, "fixtures", "ordered.dat")));
            return Decode(dat.Frames[0].SubBlocks[0]);
        }

        private static Lf2DatSubBlock Block(string text)
        {
            return new Lf2DatParserV2().ParseLoganContent("<frame> 0 standing\nopoint: " + text + " opoint_end:\n<frame_end>").Frames[0].SubBlocks[0];
        }

        private static BattleObjectPointValue Decode(Lf2DatSubBlock block)
        {
            Type type = typeof(Lf2DatParserV2).Assembly.GetType("NTSD.DatParser.LoganCombatRecordDecoder");
            MethodInfo method = type.GetMethod("ObjectPoint", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Native OPoint24 decoder is missing");
            return (BattleObjectPointValue)method.Invoke(null, new object[] { block });
        }
    }
}
#endif
