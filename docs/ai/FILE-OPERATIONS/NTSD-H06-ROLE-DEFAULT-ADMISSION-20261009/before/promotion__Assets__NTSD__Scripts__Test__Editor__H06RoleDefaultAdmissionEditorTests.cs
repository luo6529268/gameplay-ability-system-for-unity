#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Reflection;
using System.IO;
using Newtonsoft.Json.Linq;
using NTSD.Animation.Rendering.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public sealed class H06RoleDefaultAdmissionEditorTests
    {
        [TestCase("before")]
        [TestCase("after")]
        [Explicit("Capture this authorized window's exact Editor settings without changing them.")]
        public void AdmissionEditorSettings(string phase)
        {
            Assert.That(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                EditorApplication.isUpdating, Is.False);
            var scene = SceneManager.GetSceneByPath("Assets/NTSD/Scene/NTSD_Menu.unity");
            Assert.That(scene.isLoaded, Is.True);
            Assert.That(scene.isDirty, Is.False);
            BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
            var snapshot = (JObject)typeof(H06CollectorPreparationEditorTests).GetMethod("Snapshot",
                BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            snapshot["scene"] = scene.path;
            snapshot["roots"] = scene.rootCount;
            const string root = "artifacts/diagnostics/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009/";
            using (var stream = new FileStream(root + "editor-" + phase + ".json", FileMode.CreateNew))
            using (var writer = new StreamWriter(stream)) writer.Write(snapshot.ToString());
            if (phase == "after")
                Assert.That(JToken.DeepEquals(snapshot, JObject.Parse(File.ReadAllText(root + "editor-before.json"))), Is.True);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void ScopedRequestPreservesHistoricalConfiguration(int index)
        {
            var method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod("BuildPostBindingsRoleRequest",
                BindingFlags.Static | BindingFlags.NonPublic);
            var historical = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            var actual = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(index);
            Assert.That(actual.outputPath, Does.StartWith("artifacts/diagnostics/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009/windows/"));
            Assert.That(actual.outputPath, Is.Not.EqualTo(historical.outputPath));
            actual.outputPath = historical.outputPath;
            Assert.That(JsonUtility.ToJson(actual), Is.EqualTo(JsonUtility.ToJson(historical)));
        }

        [TestCase(0)]
        [TestCase(2)]
        public void ScopedPairChangesOnlyCollectorAndOutput(int index)
        {
            var brute = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(index);
            var role = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(index + 1);
            Assert.That(brute.formalCollectorMode, Is.EqualTo("brute"));
            Assert.That(role.formalCollectorMode, Is.EqualTo("role"));
            var a = JObject.Parse(JsonUtility.ToJson(brute));
            var b = JObject.Parse(JsonUtility.ToJson(role));
            a.Remove("formalCollectorMode"); b.Remove("formalCollectorMode");
            a.Remove("outputPath"); b.Remove("outputPath");
            Assert.That(JToken.DeepEquals(a, b), Is.True);
        }
    }
}
#endif
