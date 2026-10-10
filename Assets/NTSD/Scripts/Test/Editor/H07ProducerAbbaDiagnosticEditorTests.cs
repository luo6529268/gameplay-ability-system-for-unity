#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public sealed class H07ProducerAbbaDiagnosticEditorTests
    {
        private const BindingFlags Static = BindingFlags.Static | BindingFlags.NonPublic;
        private const string Root = "artifacts/diagnostics/NTSD-H07-PRODUCER-ABBA-DIAG-20261009/";
        private static Type Suite => typeof(BattleOptimizationWindowsAiSuiteEditor);
        private static MethodInfo Method(string name) => Suite.GetMethod(name, Static);
        private static FieldInfo Field(string name) => Suite.GetField(name, Static);
        private static object NewRun() => Activator.CreateInstance(Suite.GetNestedType("RunState", BindingFlags.NonPublic));

        [TestCase(0, false)]
        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(3, false)]
        public void CaseAndActualRequestRoutingPreserveCombatConfiguration(int index, bool enabled)
        {
            var experiment = BattleOptimizationWindowsAiSuiteEditor.BuildProducerAbbaCase(index);
            Assert.That(experiment.sequenceIndex, Is.EqualTo(index));
            Assert.That(experiment.action, Is.EqualTo("combat1000"));
            Assert.That(experiment.reuseEnabled, Is.EqualTo(enabled));
            Assert.That(experiment.inputBoundaryTimingEnabled, Is.False);
            var request = BattleOptimizationWindowsAiSuiteEditor.BuildProducerAbbaRequest(index);
            var baseline = (ProductionEntityStressRequest)Method("BuildProducerSnapshotReuseRequest").Invoke(null, new object[] { 2 });
            Assert.That(request.outputPath, Does.StartWith(Root + "windows/" + index.ToString("D2")));
            Assert.That(request.outputPath, Does.Contain(enabled ? "-on/" : "-off/"));
            var stateField = Field("state");
            object previous = stateField.GetValue(null);
            var state = Activator.CreateInstance(stateField.FieldType);
            stateField.FieldType.GetField("producerAbbaDiagnostic").SetValue(state, true);
            try
            {
                stateField.SetValue(null, state);
                Assert.That(JsonUtility.ToJson(Method("BuildCurrentRequest").Invoke(null, new object[] { index })),
                    Is.EqualTo(JsonUtility.ToJson(request)));
            }
            finally
            {
                stateField.SetValue(null, previous);
            }
            request.outputPath = baseline.outputPath;
            Assert.That(JsonUtility.ToJson(request), Is.EqualTo(JsonUtility.ToJson(baseline)));
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void InvalidSequenceRejected(int index)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => BattleOptimizationWindowsAiSuiteEditor.BuildProducerAbbaRequest(index));
        }

        [TestCase(0, false)]
        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(1, true)]
        [TestCase(2, false)]
        [TestCase(2, true)]
        [TestCase(3, false)]
        [TestCase(3, true)]
        public void OwnerAppliesExplicitCaseAndRestoresAfterRecordingStops(int index, bool prior)
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            world.EnableCharacterInputProducerSnapshotReuseForDiagnostics = prior;
            world.EnableCharacterInputBoundaryTimingForDiagnostics = !prior;
            var experiment = BattleOptimizationWindowsAiSuiteEditor.BuildProducerAbbaCase(index);
            object run = NewRun();
            try
            {
                Method("ApplyProducerSnapshotReuseCase").Invoke(null, new object[] { world, query, run, experiment });
                Assert.That(world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, Is.EqualTo(experiment.reuseEnabled));
                Assert.That(world.EnableCharacterInputBoundaryTimingForDiagnostics, Is.False);
                Assert.That(Method("ProducerSnapshotReuseQueryValid").Invoke(null, new object[] { query, index }), Is.True);
                Assert.That(Method("ProducerSnapshotReuseQueryValid").Invoke(null, new object[] { query, (index + 1) % 4 }), Is.False);
                world.EnableCharacterInputBoundaryTimingForDiagnostics = true;
                Assert.That(Method("ProducerSnapshotReuseQueryValid").Invoke(null, new object[] { query, index }), Is.False);
                world.EnableCharacterInputBoundaryTimingForDiagnostics = false;
                world.EnableCharacterInputProducerSnapshotReuseForDiagnostics = !experiment.reuseEnabled;
                Assert.That(Method("ProducerSnapshotReuseQueryValid").Invoke(null, new object[] { query, index }), Is.False);
                world.EnableCharacterInputProducerSnapshotReuseForDiagnostics = experiment.reuseEnabled;
                world.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
                Assert.That(Method("ProducerSnapshotReuseQueryValid").Invoke(null, new object[] { query, index }), Is.True);
                Assert.That(Method("RestoreProducerSnapshotReuse").Invoke(null, null), Is.True);
                Assert.That(world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, Is.EqualTo(prior));
                Assert.That(world.EnableCharacterInputBoundaryTimingForDiagnostics, Is.EqualTo(!prior));
                Assert.That(Field("producerSnapshotReuseWorld").GetValue(null), Is.Null);
                Assert.That(Field("producerSnapshotReuseCase").GetValue(null), Is.Null);
                Assert.That(Method("RestoreProducerSnapshotReuse").Invoke(null, null), Is.True);
            }
            finally
            {
                Method("RestoreProducerSnapshotReuse").Invoke(null, null);
                world.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
            }
        }

        [TestCase(false, 0, true)]
        [TestCase(true, 1000, true)]
        [TestCase(true, 0, false)]
        public void ObservationRequiresActualSkipWithBoundaryCallsZero(bool enabled, int skips, bool expected)
        {
            object run = NewRun();
            foreach (string name in new[] { "producerSnapshotReuseFlagApplied", "producerSnapshotReuseUnchanged",
                "producerSnapshotReuseRestored", "inputBoundaryTimingFlagApplied", "inputBoundaryTimingUnchanged",
                "inputBoundaryTimingRestored", "inputBoundaryTimingCountsAvailable" })
                run.GetType().GetField(name).SetValue(run, true);
            run.GetType().GetField("producerSnapshotReuseEnabled").SetValue(run, enabled);
            run.GetType().GetField("producerSnapshotReuseObservedPasses").SetValue(run, 1);
            run.GetType().GetField("producerSnapshotReuseMaximumObservedSkips").SetValue(run, skips);
            Assert.That(Method("ProducerSnapshotReuseObservationValid").Invoke(null, new[] { run }), Is.EqualTo(expected));
            run.GetType().GetField("inputBoundaryTimingPhaseCalls").SetValue(run, new long[] { 1, 0, 0, 0 });
            Assert.That(Method("ProducerSnapshotReuseObservationValid").Invoke(null, new[] { run }), Is.False);
        }

        [TestCase("before")]
        [TestCase("after")]
        [Explicit("Read-only settings snapshot for this one authorized ABBA group.")]
        public void EditorSettings(string phase)
        {
            Assert.That(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating, Is.False);
            var scene = SceneManager.GetSceneByPath("Assets/NTSD/Scene/NTSD_Menu.unity");
            Assert.That(scene.isLoaded && !scene.isDirty, Is.True);
            BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
            var snapshot = (JObject)typeof(H06CollectorPreparationEditorTests).GetMethod("Snapshot", Static).Invoke(null, null);
            snapshot["scene"] = scene.path;
            snapshot["roots"] = scene.rootCount;
            using (var stream = new FileStream(Root + "editor-" + phase + ".json", FileMode.CreateNew))
            using (var writer = new StreamWriter(stream))
                writer.Write(snapshot.ToString());
            if (phase == "after")
                Assert.That(JToken.DeepEquals(snapshot, JObject.Parse(File.ReadAllText(Root + "editor-before.json"))), Is.True);
        }
    }
}
#endif
