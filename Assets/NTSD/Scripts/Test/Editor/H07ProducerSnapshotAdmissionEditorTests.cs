#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NTSD.Animation.Rendering.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public sealed class H07ProducerSnapshotAdmissionEditorTests
    {
        private const string Output = "artifacts/diagnostics/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009/";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void ScopedRequestPreservesRoleAndHistoricalConfiguration(int index)
        {
            var historical = (ProductionEntityStressRequest)typeof(BattleOptimizationWindowsAiSuiteEditor)
                .GetMethod("BuildProducerSnapshotReuseRequest", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { index });
            var actual = BattleOptimizationWindowsAiSuiteEditor.BuildScopedProducerAdmissionRequest(index);
            Assert.That(actual.formalCollectorMode, Is.EqualTo("role"));
            Assert.That(actual.outputPath, Does.StartWith(Output + "windows/"));
            actual.outputPath = historical.outputPath;
            Assert.That(JsonUtility.ToJson(actual), Is.EqualTo(JsonUtility.ToJson(historical)));
        }

        [TestCase(0)]
        [TestCase(2)]
        public void ScopedPairChangesOnlyOutputWithReuseOwnedByColdObserver(int index)
        {
            var a = BattleOptimizationWindowsAiSuiteEditor.BuildScopedProducerAdmissionRequest(index);
            var b = BattleOptimizationWindowsAiSuiteEditor.BuildScopedProducerAdmissionRequest(index + 1);
            Assert.That(a.outputPath, Is.Not.EqualTo(b.outputPath));
            a.outputPath = b.outputPath;
            Assert.That(JsonUtility.ToJson(a), Is.EqualTo(JsonUtility.ToJson(b)));
        }

        [TestCase("before")]
        [TestCase("after")]
        [Explicit("Capture this authorized window's exact settings without changing them.")]
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
            using (var stream = new FileStream(Output + "editor-" + phase + ".json", FileMode.CreateNew))
            using (var writer = new StreamWriter(stream)) writer.Write(snapshot.ToString());
            if (phase == "after")
                Assert.That(JToken.DeepEquals(snapshot, JObject.Parse(File.ReadAllText(Output + "editor-before.json"))), Is.True);
        }
    }
}
#endif
