#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class BattleLogicGcCallsiteWindowEditorTests
    {
        [TestCase(-1, false)]
        [TestCase(0, false)]
        [TestCase(1, false)]
        [TestCase(34, false)]
        [TestCase(35, true)]
        [TestCase(36, true)]
        [TestCase(180, true)]
        [TestCase(181, false)]
        public void LogicCallsiteStart_IsBoundedToLateSteadyWindow(int sampledTicks, bool expected)
        {
            MethodInfo method = RequiredMethod(typeof(BattleOptimizationCpuGcCaptureEditor),
                "HasReachedLogicCallsiteStart");
            Assert.That(method.Invoke(null, new object[] { sampledTicks }), Is.EqualTo(expected));
        }

        [Test]
        public void Request_PreservesAllProductionCombatFieldsExceptOutput()
        {
            MethodInfo candidate = RequiredMethod(typeof(BattleOptimizationWindowsAiSuiteEditor),
                "BuildLogicCallsiteRequest");
            MethodInfo baseline = RequiredMethod(typeof(BattleOptimizationWindowsAiSuiteEditor),
                "BuildBruteProductionRequest");
            object actual = candidate.Invoke(null, new object[] { 0 });
            object expected = baseline.Invoke(null, new object[] { 1 });
            FieldInfo[] fields = actual.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            Assert.That(fields.Length, Is.EqualTo(65));
            foreach (FieldInfo field in fields)
            {
                if (field.Name != "outputPath")
                    Assert.That(field.GetValue(actual), Is.EqualTo(field.GetValue(expected)), field.Name);
            }
        }

        [TestCase(-1)]
        [TestCase(1)]
        public void Request_RejectsIndicesOutsideSingleCombatWindow(int index)
        {
            MethodInfo method = RequiredMethod(typeof(BattleOptimizationWindowsAiSuiteEditor),
                "BuildLogicCallsiteRequest");
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => method.Invoke(null, new object[] { index }));
            Assert.That(exception.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void Request_UsesFreshBatch55OwnedOutput()
        {
            object request = RequiredMethod(typeof(BattleOptimizationWindowsAiSuiteEditor),
                "BuildLogicCallsiteRequest").Invoke(null, new object[] { 0 });
            Assert.That(request.GetType().GetField("outputPath").GetValue(request), Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH55-LOGIC-GC-CALLSITE-20261008/windows-01/00-combat1000-callsite/report.json"));
        }

        [Test]
        public void LegacyCaptureState_DefaultStillStartsAtFirstSteadySample()
        {
            Type stateType = typeof(BattleOptimizationCpuGcCaptureEditor).GetNestedType(
                "CaptureState", BindingFlags.NonPublic);
            Assert.That(stateType, Is.Not.Null);
            FieldInfo field = stateType.GetField("minimumSampleTick");
            Assert.That(field, Is.Not.Null);
            Assert.That(field.GetValue(Activator.CreateInstance(stateType, true)), Is.EqualTo(1));
        }

        [Test]
        public void Capture_RemainsEightPhysicalProfilerFrames()
        {
            FieldInfo capacity = typeof(BattleOptimizationCpuGcCaptureEditor).GetField(
                "CaptureFrameCount", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(capacity.GetRawConstantValue(), Is.EqualTo(8));
        }

        [Test]
        public void DelayedArm_RequiresExplicitOutputRoot()
        {
            MethodInfo method = RequiredMethod(typeof(BattleOptimizationCpuGcCaptureEditor),
                "ArmLogicCallsiteWindow");
            ParameterInfo[] parameters = method.GetParameters();
            Assert.That(parameters.Length, Is.EqualTo(1));
            Assert.That(parameters[0].ParameterType, Is.EqualTo(typeof(string)));
        }

        private static MethodInfo RequiredMethod(Type type, string name)
        {
            MethodInfo method = type.GetMethod(name,
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, type.Name + "." + name);
            return method;
        }
    }
}
#endif
