#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Linq;
using System.Reflection;

using NTSD.Animation;
using NTSD.DatParser;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ItrDefenseFieldsCarrierEditorTests
    {
        [Test]
        public void SparkAndDbdefend_DefaultParseAndCopyAsTypedFields()
        {
            FieldInfo spark = typeof(InteractionArea).GetField("spark");
            FieldInfo dbdefend = typeof(InteractionArea).GetField("dbdefend");
            Assert.That(spark, Is.Not.Null);
            Assert.That(dbdefend, Is.Not.Null);

            var defaults = new InteractionArea();
            Assert.That(spark.GetValue(defaults), Is.Zero);
            Assert.That(dbdefend.GetValue(defaults), Is.Zero);

            InteractionArea parsed = ParseFirstItr(@"
<frame> 0 test
  itr:
    kind: 0 spark: 5 dbdefend: 1 injury: 7
  itr_end:
<frame_end>");
            Assert.That(spark.GetValue(parsed), Is.EqualTo(5));
            Assert.That(dbdefend.GetValue(parsed), Is.EqualTo(1));

            var copy = new InteractionArea();
            copy.CopyFrom(parsed);
            Assert.That(spark.GetValue(copy), Is.EqualTo(5));
            Assert.That(dbdefend.GetValue(copy), Is.EqualTo(1));
        }

        [Test]
        public void RawItrFingerprint_IncludesBothDefenseFields()
        {
            FieldInfo spark = typeof(InteractionArea).GetField("spark");
            FieldInfo dbdefend = typeof(InteractionArea).GetField("dbdefend");
            Assert.That(spark, Is.Not.Null);
            Assert.That(dbdefend, Is.Not.Null);
            MethodInfo fingerprint = typeof(BattleEcsHitExecutionPlan).GetMethod(
                "Fingerprint",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(InteractionArea) },
                null);
            Assert.That(fingerprint, Is.Not.Null);

            var baselineItr = new InteractionArea();
            var sparkItr = new InteractionArea();
            spark.SetValue(sparkItr, 5);
            var dbdefendItr = new InteractionArea();
            dbdefend.SetValue(dbdefendItr, 1);
            ulong baseline = InvokeFingerprint(fingerprint, baselineItr);

            Assert.That(InvokeFingerprint(fingerprint, sparkItr),
                Is.Not.EqualTo(baseline));
            Assert.That(InvokeFingerprint(fingerprint, dbdefendItr),
                Is.Not.EqualTo(baseline));
        }

        [Test]
        public void ItrProjection_PreservesAndFingerprintsBothFields()
        {
            Type projectionType = ProjectionType();
            object baseline = CreateProjection(new InteractionArea());
            InteractionArea changed = ParseFirstItr(@"
<frame> 0 test
  itr:
    kind: 0 spark: 7 dbdefend: 1
  itr_end:
<frame_end>");
            object projection = CreateProjection(changed);

            Assert.That(FieldValue(projectionType, projection, "Spark"),
                Is.EqualTo(7));
            Assert.That(FieldValue(projectionType, projection, "Dbdefend"),
                Is.EqualTo(1));

            MethodInfo fingerprint = typeof(BattleEcsHitExecutionPlan)
                .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .Single(method => method.Name == "Fingerprint" &&
                    method.GetParameters().Length == 1 &&
                    method.GetParameters()[0].ParameterType.IsByRef &&
                    method.GetParameters()[0].ParameterType.GetElementType() ==
                        projectionType);
            ulong baselineHash = InvokeFingerprint(fingerprint, baseline);
            Assert.That(InvokeFingerprint(fingerprint, projection),
                Is.Not.EqualTo(baselineHash));
        }

        [Test]
        public void Kind5Replacement_ReplacesBothFieldsWithoutStaleValues()
        {
            Type projectionType = ProjectionType();
            InteractionArea source = ParseFirstItr(@"
<frame> 0 test
  itr:
    kind: 5 spark: 9 dbdefend: 8
  itr_end:
<frame_end>");
            object projection = CreateProjection(source);
            InteractionArea replacement = ParseFirstItr(@"
<frame> 0 test
  itr:
    kind: 0 spark: 2 dbdefend: 1
  itr_end:
<frame_end>");
            MethodInfo apply = projectionType.GetMethod(
                "ApplyKind5Replacement",
                BindingFlags.Instance | BindingFlags.NonPublic |
                BindingFlags.Public);
            Assert.That(apply, Is.Not.Null);

            apply.Invoke(projection, new object[] { replacement });

            Assert.That(FieldValue(projectionType, projection, "Spark"),
                Is.EqualTo(2));
            Assert.That(FieldValue(projectionType, projection, "Dbdefend"),
                Is.EqualTo(1));
        }

        private static Type ProjectionType()
        {
            Type type = typeof(BattleEcsHitExecutionPlan).GetNestedType(
                "ItrProjection",
                BindingFlags.NonPublic);
            Assert.That(type, Is.Not.Null);
            return type;
        }

        private static object CreateProjection(InteractionArea source)
        {
            return Activator.CreateInstance(
                ProjectionType(),
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic,
                null,
                new object[] { source },
                null);
        }

        private static int FieldValue(Type type, object value, string name)
        {
            FieldInfo field = type.GetField(
                name,
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return (int)field.GetValue(value);
        }

        private static ulong InvokeFingerprint(MethodInfo method, object value)
        {
            return (ulong)method.Invoke(null, new[] { value });
        }

        private static InteractionArea ParseFirstItr(string text)
        {
            Lf2DatFile dat = new Lf2DatParserV2().Parse(text);
            return Lf2DatConverter.ConvertToFrameData(dat.Frames[0]).itrs[0];
        }
    }
}
#endif
