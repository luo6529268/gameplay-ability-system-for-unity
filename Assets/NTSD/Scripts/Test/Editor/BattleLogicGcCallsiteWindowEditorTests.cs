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

        [TestCase(10, 3, 10, false)]
        [TestCase(10, 3, 11, true)]
        [TestCase(10, 3, 13, true)]
        [TestCase(10, 3, 14, false)]
        [TestCase(10, 0, 11, false)]
        [TestCase(-1, 3, 0, false)]
        [TestCase(10, -1, 11, false)]
        [TestCase(int.MaxValue - 1, 1, int.MaxValue, true)]
        public void LogicRecovery_AttributesOnlyStrictDescendants(int root, int children, int sample, bool expected)
        {
            MethodInfo method = RequiredMethod(typeof(BattleOptimizationCpuGcCaptureEditor), "IsLogicDescendant");
            Assert.That(method.Invoke(null, new object[] { root, children, sample }), Is.EqualTo(expected));
        }

        [TestCase(-1, 0, 0, true)]
        [TestCase(1864, 1865, 1900, true)]
        [TestCase(1864, 1864, 1900, false)]
        [TestCase(1864, -1, 1900, false)]
        [TestCase(1864, 1865, 1864, false)]
        [TestCase(-2, 0, 1, false)]
        [TestCase(int.MaxValue, int.MaxValue, int.MaxValue, false)]
        public void LogicRecovery_RejectsMixedOrMissingAppendedRange(int previousLast, int first, int last, bool expected)
        {
            MethodInfo method = RequiredMethod(typeof(BattleOptimizationCpuGcCaptureEditor), "IsLogicAppendedRange");
            Assert.That(method.Invoke(null, new object[] { previousLast, first, last }), Is.EqualTo(expected));
        }

        [Test]
        public void LogicRecovery_ExposesDedicatedOfflineEntry()
        {
            MethodInfo method = RequiredMethod(typeof(BattleOptimizationCpuGcCaptureEditor), "RecoverRetainedLogicRaw");
            Assert.That(method.GetParameters(), Is.Empty);
        }

        [Test]
        public void LogicRecovery_BoundsRetainedScanAndUsesExistingDriverMarker()
        {
            Type type = typeof(BattleOptimizationCpuGcCaptureEditor);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            FieldInfo limit = type.GetField("LogicRecoveryFrameLimit", flags);
            FieldInfo marker = type.GetField("LogicDriverMarkerName", flags);
            Assert.That(limit, Is.Not.Null);
            Assert.That(marker, Is.Not.Null);
            Assert.That(limit.GetRawConstantValue(), Is.EqualTo(300));
            Assert.That(marker.GetRawConstantValue(), Is.EqualTo("NTSD.ProductionEntityStress.Driver.StepOneTick"));
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
