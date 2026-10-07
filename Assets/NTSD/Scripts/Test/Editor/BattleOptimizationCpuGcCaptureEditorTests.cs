#if UNITY_EDITOR
using System;
using System.Reflection;
using NTSD.Animation.Rendering.Editor;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleOptimizationCpuGcCaptureEditorTests
    {
        [Test]
        public void CaptureRequest_PreservesCombatBaselineExceptOutput()
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(1);
            var actual = (ProductionEntityStressRequest)GetMethod(
                typeof(BattleOptimizationWindowsAiSuiteEditor), "BuildCpuGcCaptureRequest")
                .Invoke(null, new object[] { 0 });
            Assert.That(actual.outputPath, Does.Contain("BATCH39-CPU-GC-CAPTURE"));
            actual.outputPath = expected.outputPath;
            Assert.That(JsonUtility.ToJson(actual), Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [TestCase(-1)]
        [TestCase(1)]
        public void CaptureRequest_RejectsOutsideSingleWindow(int index)
        {
            MethodInfo method = GetMethod(typeof(BattleOptimizationWindowsAiSuiteEditor),
                "BuildCpuGcCaptureRequest");
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => method.Invoke(null, new object[] { index }));
            Assert.That(exception.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void CaptureGate_RequiresCompletedWarmupAndObservedThousandAi()
        {
            Assert.That(IsReady(ReadyReport()), Is.True);
        }

        [TestCase("warmup")]
        [TestCase("sample")]
        [TestCase("ai")]
        [TestCase("roster")]
        [TestCase("status")]
        public void CaptureGate_RejectsIncompleteWorkload(string condition)
        {
            ProductionEntityStressReport report = ReadyReport();
            switch (condition)
            {
                case "warmup": report.warmupTicksCompleted = 119; break;
                case "sample": report.sampledLogicTicks = 0; break;
                case "ai": report.baseAiActiveCount = 999; break;
                case "roster": report.baseRosterActiveCount = 999; break;
                case "status": report.status = "Preparing"; break;
            }
            Assert.That(IsReady(report), Is.False);
        }

        [Test]
        public void CaptureGate_RejectsMissingReport()
        {
            Assert.That(IsReady(null), Is.False);
        }

        private static bool IsReady(ProductionEntityStressReport report)
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).Assembly.GetType(
                "NTSD.Test.Editor.BattleOptimizationCpuGcCaptureEditor", false);
            Assert.That(type, Is.Not.Null, "The bounded capture owner is not implemented.");
            return (bool)GetMethod(type, "IsCaptureReady").Invoke(null, new object[] { report });
        }

        private static MethodInfo GetMethod(Type type, string name)
        {
            MethodInfo method = type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, name + " is not implemented.");
            return method;
        }

        private static ProductionEntityStressReport ReadyReport()
        {
            return new ProductionEntityStressReport
            {
                status = "Running",
                warmupTicksCompleted = 120,
                sampledLogicTicks = 1,
                baseAiActiveCount = 1000,
                baseRosterActiveCount = 1000,
            };
        }
    }
}
#endif

