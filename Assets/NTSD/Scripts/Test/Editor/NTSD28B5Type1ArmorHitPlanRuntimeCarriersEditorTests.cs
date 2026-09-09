#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type1ArmorHitPlanRuntimeCarriersEditorTests
    {
        private static readonly string[] RequiredFields =
        {
            "TargetRuntimeArmorHp",
            "TargetInputHpConsumedTotal",
            "TargetInputMpConsumedTotal",
        };

        [Test]
        public void WriterEffectSnapshot_DeclaresArmorTransactionFields()
        {
            Type snapshotType = GetSnapshotType();

            foreach (string fieldName in RequiredFields)
            {
                FieldInfo field = snapshotType.GetField(
                    fieldName,
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(field, Is.Not.Null, fieldName);
                Assert.That(field.FieldType, Is.EqualTo(typeof(int)), fieldName);
            }
        }

        [Test]
        public void CaptureWriterEffectSnapshot_ReadsArmorTransactionRuntimeTruth()
        {
            var world = new SimulationWorld();
            var target = new LF2Character();
            target.Runtime.RuntimeArmorHp118 = -1;
            target.Runtime.InputHpConsumedTotal34C = 17;
            target.Runtime.InputMpConsumedTotal350 = 23;

            object snapshot = Capture(world, target);

            Assert.That(Read(snapshot, RequiredFields[0]), Is.EqualTo(-1));
            Assert.That(Read(snapshot, RequiredFields[1]), Is.EqualTo(17));
            Assert.That(Read(snapshot, RequiredFields[2]), Is.EqualTo(23));
        }

        [Test]
        public void CaptureWriterEffectSnapshot_UsesMissingTargetSentinels()
        {
            var world = new SimulationWorld();

            object snapshot = Capture(world, null);

            foreach (string fieldName in RequiredFields)
                Assert.That(Read(snapshot, fieldName), Is.EqualTo(int.MinValue));
        }

        [Test]
        public void DifferenceMask_ObservesEveryArmorTransactionField()
        {
            Type snapshotType = GetSnapshotType();
            MethodInfo differenceMask = GetDifferenceMask(snapshotType);

            foreach (string fieldName in RequiredFields)
            {
                object expected = Activator.CreateInstance(snapshotType);
                object actual = Activator.CreateInstance(snapshotType);
                FieldInfo field = RequireField(snapshotType, fieldName);
                field.SetValue(actual, 1);

                ulong mask = (ulong)differenceMask.Invoke(
                    null,
                    new[] { expected, actual });

                Assert.That(mask, Is.Not.Zero, fieldName);
            }
        }

        private static object Capture(SimulationWorld world, LF2Entity target)
        {
            FieldInfo planField = typeof(SimulationWorld).GetField(
                "battleEcsHitExecutionPlan",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(planField, Is.Not.Null);
            object plan = planField.GetValue(world);
            MethodInfo capture = plan.GetType().GetMethod(
                "CaptureWriterEffectSnapshot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(capture, Is.Not.Null);
            return capture.Invoke(plan, new object[] { null, target, -1 });
        }

        private static int Read(object snapshot, string fieldName)
        {
            return (int)RequireField(snapshot.GetType(), fieldName).GetValue(snapshot);
        }

        private static FieldInfo RequireField(Type snapshotType, string fieldName)
        {
            FieldInfo field = snapshotType.GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            return field;
        }

        private static Type GetSnapshotType()
        {
            Type planType = typeof(SimulationWorld).Assembly.GetType(
                "NTSD.Simulation.Ecs.BattleEcsHitExecutionPlan");
            Assert.That(planType, Is.Not.Null);
            Type snapshotType = planType.GetNestedType(
                "WriterEffectSnapshot",
                BindingFlags.NonPublic);
            Assert.That(snapshotType, Is.Not.Null);
            return snapshotType;
        }

        private static MethodInfo GetDifferenceMask(Type snapshotType)
        {
            Type planType = snapshotType.DeclaringType;
            foreach (MethodInfo method in planType.GetMethods(
                         BindingFlags.Static | BindingFlags.NonPublic))
            {
                if (!string.Equals(method.Name, "DifferenceMask", StringComparison.Ordinal))
                    continue;
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length == 2 &&
                    parameters[0].ParameterType.IsByRef &&
                    parameters[0].ParameterType.GetElementType() == snapshotType)
                {
                    return method;
                }
            }

            Assert.Fail("WriterEffectSnapshot DifferenceMask overload is missing.");
            return null;
        }
    }
}
#endif
