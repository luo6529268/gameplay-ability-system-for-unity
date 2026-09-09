#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ItrStatusFieldCarrierEditorTests
    {
        [Test]
        public void ParserAndConverter_PreserveAllThirteenStatusFields()
        {
            InteractionArea itr = ParseFirstItr(@"
<frame> 0 status
itr:
  delay: 101 poison: 102 confus: 103 weak: 104 manacle: 105
  join: 106 mimic: 107 bound: 108 facing: 109
  dx: 110 dy: 111 dz: 112 gain: 113
itr_end:
<frame_end>");

            AssertFields(itr, 101);
        }

        [Test]
        public void DefaultsAreZero_AndConfuseTypoRemainsRawOnly()
        {
            var defaults = new InteractionArea();
            AssertFields(defaults, 0, sequential: false);

            InteractionArea typo = ParseFirstItr(@"
<frame> 0 typo
itr:
  confuse: 999
itr_end:
<frame_end>");

            Assert.That(typo.confus, Is.Zero);
            Assert.That(typo.rawProperties["confuse"], Is.EqualTo("999"));
        }

        [Test]
        public void CopyFrom_PreservesAllThirteenStatusFields()
        {
            var source = new InteractionArea
            {
                delay = 201,
                poison = 202,
                confus = 203,
                weak = 204,
                manacle = 205,
                join = 206,
                mimic = 207,
                bound = 208,
                facing = 209,
                dx = 210,
                dy = 211,
                dz = 212,
                gain = 213,
            };
            var destination = new InteractionArea();

            destination.CopyFrom(source);

            AssertFields(destination, 201);
        }

        [Test]
        public void HitPlanFingerprints_ChangeForEveryStatusField()
        {
            MethodInfo fingerprint = typeof(BattleEcsHitExecutionPlan)
                .GetMethod(
                    "Fingerprint",
                    BindingFlags.Static | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(InteractionArea) },
                    null);
            Assert.That(fingerprint, Is.Not.Null);
            ulong baseline = (ulong)fingerprint.Invoke(
                null,
                new object[] { new InteractionArea() });
            var cases = new List<(string Name, Action<InteractionArea> Set)>
            {
                ("delay", value => value.delay = 1),
                ("poison", value => value.poison = 2),
                ("confus", value => value.confus = 3),
                ("weak", value => value.weak = 4),
                ("manacle", value => value.manacle = 5),
                ("join", value => value.join = 6),
                ("mimic", value => value.mimic = 7),
                ("bound", value => value.bound = 8),
                ("facing", value => value.facing = 9),
                ("dx", value => value.dx = 10),
                ("dy", value => value.dy = 11),
                ("dz", value => value.dz = 12),
                ("gain", value => value.gain = 13),
            };

            for (int index = 0; index < cases.Count; index++)
            {
                var itr = new InteractionArea();
                cases[index].Set(itr);
                ulong changed = (ulong)fingerprint.Invoke(
                    null,
                    new object[] { itr });
                Assert.That(changed, Is.Not.EqualTo(baseline), cases[index].Name);
            }
        }

        [Test]
        public void HitPlanProjection_CapturesEveryStatusField()
        {
            Type projectionType = typeof(BattleEcsHitExecutionPlan)
                .GetNestedType("ItrProjection", BindingFlags.NonPublic);
            Assert.That(projectionType, Is.Not.Null);
            InteractionArea source = ParseFirstItr(@"
<frame> 0 projection
itr:
  delay: 301 poison: 302 confus: 303 weak: 304 manacle: 305
  join: 306 mimic: 307 bound: 308 facing: 309
  dx: 310 dy: 311 dz: 312 gain: 313
itr_end:
<frame_end>");
            object projection = Activator.CreateInstance(
                projectionType,
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic,
                null,
                new object[] { source },
                null);
            string[] names =
            {
                "Delay", "Poison", "Confus", "Weak", "Manacle", "Join",
                "Mimic", "Bound", "Facing", "Dx", "Dy", "Dz", "Gain",
            };

            for (int index = 0; index < names.Length; index++)
            {
                FieldInfo field = projectionType.GetField(
                    names[index],
                    BindingFlags.Instance | BindingFlags.Public |
                    BindingFlags.NonPublic);
                Assert.That(field, Is.Not.Null, names[index]);
                Assert.That(field.GetValue(projection), Is.EqualTo(301 + index),
                    names[index]);
            }
        }

        private static InteractionArea ParseFirstItr(string text)
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(text);
            return Lf2DatConverter.ConvertToFrameData(dat.Frames[0]).itrs[0];
        }

        private static void AssertFields(
            InteractionArea itr,
            int first,
            bool sequential = true)
        {
            int Value(int offset) => sequential ? first + offset : first;
            Assert.That(itr.delay, Is.EqualTo(Value(0)));
            Assert.That(itr.poison, Is.EqualTo(Value(1)));
            Assert.That(itr.confus, Is.EqualTo(Value(2)));
            Assert.That(itr.weak, Is.EqualTo(Value(3)));
            Assert.That(itr.manacle, Is.EqualTo(Value(4)));
            Assert.That(itr.join, Is.EqualTo(Value(5)));
            Assert.That(itr.mimic, Is.EqualTo(Value(6)));
            Assert.That(itr.bound, Is.EqualTo(Value(7)));
            Assert.That(itr.facing, Is.EqualTo(Value(8)));
            Assert.That(itr.dx, Is.EqualTo(Value(9)));
            Assert.That(itr.dy, Is.EqualTo(Value(10)));
            Assert.That(itr.dz, Is.EqualTo(Value(11)));
            Assert.That(itr.gain, Is.EqualTo(Value(12)));
        }
    }
}
#endif
