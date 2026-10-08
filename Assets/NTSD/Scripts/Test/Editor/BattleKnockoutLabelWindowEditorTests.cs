#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.Rendering.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class BattleKnockoutLabelWindowEditorTests
    {
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        private const string Root = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH57-LABEL-PREWARM-WINDOWS-20261008/windows-01";

        [TestCase(0, "dispersed1000")]
        [TestCase(1, "combat1000")]
        public void RequestPreservesCompleteProductionInputExceptFreshOutput(int index, string action)
        {
            var actual = Request("BuildLabelPrewarmRequest", index);
            var expected = Request("BuildBruteProductionRequest", index);
            Assert.That(actual.outputPath, Is.EqualTo(Root + "/" + index.ToString("D2") + "-" + action + "/report.json"));
            Assert.That(actual.action, Is.EqualTo(action));
            Assert.That(actual.entityCount, Is.EqualTo(1000));
            Assert.That(actual.warmupTicks, Is.EqualTo(120));
            Assert.That(actual.sampleTicks, Is.EqualTo(180));
            Assert.That(actual.formalCollectorMode, Is.EqualTo("brute"));
            Assert.That(actual.useDedicatedSimulationWorker, Is.False);
            actual.outputPath = expected.outputPath;
            Assert.That(JsonUtility.ToJson(actual), Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void RequestRejectsOutOfRangeWithoutStartingSuite(int index)
        {
            MethodInfo method = RequiredMethod("BuildLabelPrewarmRequest");
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(
                () => method.Invoke(null, new object[] { index }));
            Assert.That(exception.InnerException, Is.TypeOf<ArgumentOutOfRangeException>());
            Assert.That(StateField().GetValue(null), Is.Null);
        }

        [Test]
        public void Legacy54And55OutputsRemainUnchanged()
        {
            Assert.That(Request("BuildLogicGcScopeRequest", 0).outputPath, Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH54-LOGIC-GC-SCOPE-20261008/windows-01/00-dispersed1000/report.json"));
            Assert.That(Request("BuildLogicGcScopeRequest", 1).outputPath, Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH54-LOGIC-GC-SCOPE-20261008/windows-01/01-combat1000/report.json"));
            Assert.That(Request("BuildLogicCallsiteRequest", 0).outputPath, Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH55-LOGIC-GC-CALLSITE-20261008/windows-01/00-combat1000-callsite/report.json"));
        }

        [Test]
        public void LabelWindowHasExplicitMenuAndColdOptIn()
        {
            MethodInfo menu = RequiredMethod("BeginLabelPrewarm");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            Assert.That(attributes.Length, Is.EqualTo(1));
            Assert.That(((MenuItem)attributes[0]).menuItem, Is.EqualTo(
                "NTSD/Validation/Optimization/Batch57 Label Prewarm Full Driver GC 1000 AI"));
            ParameterInfo parameter = Array.Find(RequiredMethod("BeginSuite").GetParameters(),
                value => value.Name == "labelPrewarmValidation");
            Assert.That(parameter, Is.Not.Null);
            Assert.That(parameter.IsOptional, Is.True);
            Assert.That(parameter.DefaultValue, Is.False);
        }

        [Test]
        public void DeserializedLegacyStateDefaultsLabelWindowToFalse()
        {
            Type stateType = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("SuiteState", BindingFlags.NonPublic);
            FieldInfo field = stateType.GetField("labelPrewarmValidation");
            Assert.That(field, Is.Not.Null);
            object state = JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", stateType);
            Assert.That(field.GetValue(state), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CurrentRequestRoutingKeepsLabelAndLegacyIdentitiesSeparate(bool labelWindow)
        {
            FieldInfo owner = StateField();
            Assert.That(owner.GetValue(null), Is.Null, "No active suite may be replaced by a test.");
            Type stateType = owner.FieldType;
            object state = Activator.CreateInstance(stateType, true);
            FieldInfo label = stateType.GetField("labelPrewarmValidation");
            Assert.That(label, Is.Not.Null);
            label.SetValue(state, labelWindow);
            stateType.GetField("logicGcScopeOnly").SetValue(state, true);
            stateType.GetField("bruteProductionOnly").SetValue(state, true);
            try
            {
                owner.SetValue(null, state);
                for (int index = 0; index < 2; index++)
                {
                    var actual = Request("BuildCurrentRequest", index);
                    var expected = Request(labelWindow ? "BuildLabelPrewarmRequest" : "BuildLogicGcScopeRequest", index);
                    Assert.That(JsonUtility.ToJson(actual), Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally
            {
                owner.SetValue(null, null);
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void LabelWindowRejectsIncompleteScopeOrCaptureBeforeStateCreation(int defect)
        {
            Assert.That(StateField().GetValue(null), Is.Null);
            MethodInfo begin = RequiredMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            Assert.That(parameters.Length, Is.EqualTo(17));
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
                arguments[index] = parameters[index].Name == "labelPrewarmValidation" ||
                    parameters[index].Name == "bruteProductionOnly" && defect != 2 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 0 ||
                    parameters[index].Name == "cpuGcCaptureOnly" && defect == 1;
            TargetInvocationException exception = Assert.Throws<TargetInvocationException>(() => begin.Invoke(null, arguments));
            Assert.That(exception.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(exception.InnerException.Message, Does.Contain("label prewarm window requires"));
            Assert.That(StateField().GetValue(null), Is.Null);
        }

        private static MethodInfo RequiredMethod(string name)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(name, StaticFlags);
            Assert.That(method, Is.Not.Null, name);
            return method;
        }

        private static ProductionEntityStressRequest Request(string method, int index) =>
            (ProductionEntityStressRequest)RequiredMethod(method).Invoke(null, new object[] { index });

        private static FieldInfo StateField() =>
            typeof(BattleOptimizationWindowsAiSuiteEditor).GetField("state", StaticFlags);
    }
}
#endif
