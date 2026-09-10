#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;

using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    [Category("NTSD28_B6")]
    public sealed class NTSD28B6NativeImpactHitPlanCarriersEditorTests
    {
        private static readonly string[] RequiredFields =
        {
            "TargetEnvironmentState320",
            "TargetImpactSourceSlot164",
            "TargetCatchSourceSlot90",
        };

        [Test]
        public void WriterEffectSnapshot_DeclaresImpactTransactionFields()
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

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(19)]
        [TestCase(399)]
        [TestCase(int.MinValue)]
        [TestCase(int.MaxValue)]
        public void CaptureWriterEffectSnapshot_ReadsImpactTransactionRuntimeTruth(int value)
        {
            var world = new SimulationWorld();
            var target = new LF2Character();
            target.Runtime.EnvironmentState320 = value;
            target.Runtime.ImpactSourceSlot164 = value;
            target.Runtime.CatchSourceSlot90 = value;

            object snapshot = Capture(world, target);

            Assert.That(Read(snapshot, RequiredFields[0]), Is.EqualTo(value));
            Assert.That(Read(snapshot, RequiredFields[1]), Is.EqualTo(value));
            Assert.That(Read(snapshot, RequiredFields[2]), Is.EqualTo(value));
        }

        [Test]
        public void CaptureWriterEffectSnapshot_UsesMissingTargetSentinels()
        {
            var world = new SimulationWorld();

            object snapshot = Capture(world, null);

            foreach (string fieldName in RequiredFields)
                Assert.That(Read(snapshot, fieldName), Is.EqualTo(int.MinValue));
        }

        [TestCase("TargetEnvironmentState320")]
        [TestCase("TargetImpactSourceSlot164")]
        [TestCase("TargetCatchSourceSlot90")]
        public void DifferenceMask_ObservesEveryImpactTransactionField(string fieldName)
        {
            Type snapshotType = GetSnapshotType();
            object expected = Activator.CreateInstance(snapshotType);
            object actual = Activator.CreateInstance(snapshotType);
            RequireField(snapshotType, fieldName).SetValue(actual, -1);
            Assert.That((ulong)GetDifferenceMask(snapshotType).Invoke(null,
                new[] { expected, actual }), Is.Not.Zero, fieldName);
        }

        [Test]
        public void ZeroSnapshots_HaveNoDifference()
        {
            Type type = GetSnapshotType();
            Assert.That((ulong)GetDifferenceMask(type).Invoke(null,
                new[] { Activator.CreateInstance(type), Activator.CreateInstance(type) }), Is.Zero);
        }

        [Test]
        public void ExistingHitConfirmProjection_PreservesImpactAndCatchFields()
        {
            var world = new SimulationWorld();
            var attacker = new LF2Character();
            var target = new LF2Character();
            target.Runtime.EnvironmentState320 = -99;
            target.Runtime.ImpactSourceSlot164 = 399;
            target.Runtime.CatchSourceSlot90 = 0x2000;
            object snapshot = Capture(world, target);
            Type planType = snapshot.GetType().DeclaringType;
            MethodInfo project = planType.GetMethod("ProjectWriterEffect",
                BindingFlags.Static | BindingFlags.NonPublic);
            Type disposition = project.GetParameters()[3].ParameterType;
            object itr = Activator.CreateInstance(project.GetParameters()[2].ParameterType);
            object[] args = { attacker, target, itr, Enum.Parse(disposition, "HitConfirm"), snapshot };
            Assert.That(project.Invoke(null, args), Is.EqualTo(true));
            Assert.That(Read(args[4], "TargetEnvironmentState320"), Is.EqualTo(-99));
            Assert.That(Read(args[4], "TargetImpactSourceSlot164"), Is.EqualTo(399));
            Assert.That(Read(args[4], "TargetCatchSourceSlot90"), Is.EqualTo(0x2000));
            Assert.That(target.Runtime.EnvironmentState320, Is.EqualTo(-99));
            Assert.That(target.Runtime.ImpactSourceSlot164, Is.EqualTo(399));
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
