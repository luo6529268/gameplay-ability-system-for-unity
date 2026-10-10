#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;

namespace NTSD.Test.Editor
{
    internal static class H06RolePrerequisiteDiagnostics
    {
        internal const string OutputRoot = "artifacts/diagnostics/NTSD-H06-ROLE-PREREQUISITE-DIAG-20261010/windows";
        private static Evidence evidence;
        private static ProfilerMarker[] tickMarkers;
        private static int openMarker = -1;

        [Serializable]
        private struct TickObservation
        {
            public int tick, unityFrame, failureMask, oldTimerMask;
            public bool steady, originalAggregate, maskMatchesOriginal, scopeValid;
            public long allocationEvents, rawValue;
            public int collector, fallbackParticipants, inertParticipants;
            public int bodyOrdinals, fallbackAttackOrdinals, exactAttackOrdinals, fallbackBodyOrdinals;
        }

        [Serializable]
        private sealed class Evidence
        {
            public string purpose = "PREREQUISITE_DIAGNOSTIC_ONLY; original admission assertion is unchanged; instrumented run is not a zero-GC/performance certificate";
            public string[] failureBits =
            {
                "record gate", "stride64", "old branch timer", "84 capacity guard", "89 frame reuse",
                "92 group skip", "94 endpoint reuse", "95 source coordinates", "producer reuse",
                "input boundary timer", "actual Role collector", "collection aborted",
                "fallback participant count", "steady boundary", "old six elapsed slots"
            };
            public int ticks, aggregateFailures, maskDisagreements, captureRequestedAfterTick;
            public string finishReason;
            public TickObservation[] observations = new TickObservation[300];
        }

        internal static ProductionEntityStressRequest BuildRequest(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BattleOptimizationWindowsAiSuiteEditor.BuildRoleRecordCostRequest(0);
            request.outputPath = OutputRoot + "/00-combat1000-prerequisites/report.json";
            return request;
        }

        internal static int AddFailure(int mask, bool conditionPassed, int bit)
        {
            return conditionPassed ? mask : mask | (1 << bit);
        }

        internal static void Start()
        {
            if (evidence != null)
                throw new InvalidOperationException("Prerequisite observer already has an owner.");
            BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
            evidence = new Evidence();
            tickMarkers = new ProfilerMarker[300];
            for (int index = 0; index < tickMarkers.Length; index++)
            {
                tickMarkers[index] = new ProfilerMarker("NTSD.H06.Prerequisite.Driver.Tick" + (index + 1).ToString("D3"));
                tickMarkers[index].Begin();
                tickMarkers[index].End();
            }
            try
            {
                Directory.CreateDirectory(Path.GetFullPath(OutputRoot + "/capture"));
                BattleOptimizationCpuGcCaptureEditor.Arm(Path.GetFullPath(OutputRoot + "/capture"));
                AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            }
            catch
            {
                evidence = null;
                tickMarkers = null;
                throw;
            }
        }

        internal static void BeginDriver(int tick)
        {
            if (evidence == null)
                return;
            if (openMarker != -1 || tick < 1 || tick > 300)
                throw new InvalidOperationException("Prerequisite marker sequence invalid.");
            openMarker = tick - 1;
            tickMarkers[openMarker].Begin();
        }

        internal static void EndDriver()
        {
            if (openMarker == -1)
                return;
            tickMarkers[openMarker].End();
            openMarker = -1;
        }

        internal static void Observe(int tick, bool steady, bool expectedRecordGate, bool originalAggregate,
            SimulationWorld world, BruteForceSceneQuery query, BattleTickDetailPhaseDiagnostics detail,
            BattleScopedGcAllocationRecorder.ScopeResult scope, ProductionEntityStressReport report)
        {
            if (evidence == null)
                return;
            if (tick != evidence.ticks + 1 || tick > 300)
                throw new InvalidOperationException("Prerequisite observation lost a completed tick.");
            int mask = 0;
            mask = AddFailure(mask, query.EnableRoleRecordCostDiagnostics == expectedRecordGate, 0);
            mask = AddFailure(mask, query.RoleBranchTimingSampleStrideForDiagnostics == 64, 1);
            mask = AddFailure(mask, !query.EnableRoleBranchTimingForDiagnostics, 2);
            mask = AddFailure(mask, !query.EnableRoleCapacityRecordGuardForDiagnostics, 3);
            mask = AddFailure(mask, !query.EnableRoleSnapshotFrameReuseForDiagnostics, 4);
            mask = AddFailure(mask, !query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics, 5);
            mask = AddFailure(mask, !query.EnableRoleEndpointSnapshotReuseForDiagnostics, 6);
            mask = AddFailure(mask, !world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics, 7);
            mask = AddFailure(mask, !world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, 8);
            mask = AddFailure(mask, !world.EnableCharacterInputBoundaryTimingForDiagnostics, 9);
            mask = AddFailure(mask, query.LastFormalCollectorModeForDiagnostics == CollisionFormalCollectorMode.ForceRoleAware, 10);
            mask = AddFailure(mask, !query.LastFormalCollectionAbortedForDiagnostics, 11);
            mask = AddFailure(mask, query.LastFormalFallbackParticipantCountForDiagnostics == 0, 12);
            mask = AddFailure(mask, steady == (tick > 120), 13);
            int timerMask = 0;
            for (int phase = 48; phase <= 53; phase++)
                if (detail.GetLastElapsedTimestampTicks((BattleTickDetailPhase)phase) != 0)
                    timerMask |= 1 << (phase - 48);
            mask = AddFailure(mask, timerMask == 0, 14);
            query.GetLastRoleAwareFallbackOrdinalCountsForSelfCheck(out int bodies,
                out int fallbackAttacks, out int exactAttacks, out int fallbackBodies);
            evidence.observations[tick - 1] = new TickObservation
            {
                tick = tick, unityFrame = Time.frameCount, steady = steady,
                failureMask = mask, oldTimerMask = timerMask, originalAggregate = originalAggregate,
                maskMatchesOriginal = (mask == 0) == originalAggregate,
                scopeValid = scope.valid && scope.calibratedBefore && !scope.saturated && !scope.wrapped,
                allocationEvents = scope.allocationEvents, rawValue = scope.rawValueTotal,
                collector = (int)query.LastFormalCollectorModeForDiagnostics,
                fallbackParticipants = query.LastFormalFallbackParticipantCountForDiagnostics,
                inertParticipants = query.LastRoleAwareInertParticipantCountForDiagnostics,
                bodyOrdinals = bodies, fallbackAttackOrdinals = fallbackAttacks,
                exactAttackOrdinals = exactAttacks, fallbackBodyOrdinals = fallbackBodies
            };
            if (!originalAggregate)
                evidence.aggregateFailures++;
            if ((mask == 0) != originalAggregate)
                evidence.maskDisagreements++;
            evidence.ticks++;
            // Capture setup/export is outside the synchronous Driver allocation/timing scopes.
            if (tick == 124)
            {
                evidence.captureRequestedAfterTick = tick;
                BattleOptimizationCpuGcCaptureEditor.Observe(report);
            }
        }

        internal static void Finish(string reason)
        {
            if (evidence == null)
                return;
            Evidence finished = evidence;
            evidence = null;
            AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try
            {
                EndDriver();
                BattleOptimizationCpuGcCaptureEditor.FinishAndRestore(reason);
            }
            finally
            {
                tickMarkers = null;
                finished.finishReason = reason;
                string path = Path.GetFullPath(OutputRoot + "/prerequisite-observations.json");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(finished, true));
            }
        }

        [MenuItem("NTSD/Validation/Optimization/H06 Parse Retained Prerequisite Capture")]
        private static void ParseRetainedCapture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                throw new InvalidOperationException("Retained parsing requires idle Editor; no recapture.");
            string captureRoot = Path.GetFullPath(OutputRoot + "/capture");
            string output = Path.Combine(captureRoot, "cpu-gc-recovery.json");
            string samples = Path.Combine(captureRoot, "cpu-gc-samples-recovered.json");
            if (File.Exists(output) || File.Exists(samples))
                throw new InvalidOperationException("Retained parsing outputs must be fresh.");
            Type capture = typeof(BattleOptimizationCpuGcCaptureEditor);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            Type stateType = capture.GetNestedType("CaptureState", BindingFlags.NonPublic);
            object recovered = JsonUtility.FromJson(File.ReadAllText(Path.Combine(captureRoot, "cpu-gc-state.json")), stateType);
            object before = capture.GetMethod("ReadSettings", flags).Invoke(null, null);
            stateType.GetField("before").SetValue(recovered, before);
            stateType.GetField("capturedFrames").SetValue(recovered, 0);
            stateType.GetField("validMainThreadFrames").SetValue(recovered, 0);
            stateType.GetField("outputRoot").SetValue(recovered, captureRoot);
            stateType.GetField("stopReason").SetValue(recovered, "retained-binary-parse-only; target ticks130/162; no recapture/history clear");
            try
            {
                int previousLast = UnityEditorInternal.ProfilerDriver.lastFrameIndex;
                if (!UnityEditorInternal.ProfilerDriver.LoadProfile(Path.Combine(captureRoot, "cpu-gc.raw"), true))
                    throw new InvalidOperationException("Retained binary import failed.");
                int first = previousLast < 0 ? UnityEditorInternal.ProfilerDriver.firstFrameIndex :
                    UnityEditorInternal.ProfilerDriver.GetNextFrameIndex(previousLast);
                int count = 0;
                int[] frames = (int[])stateType.GetField("frameIndices").GetValue(recovered);
                for (int frame = first; frame >= 0; frame = UnityEditorInternal.ProfilerDriver.GetNextFrameIndex(frame))
                {
                    bool target = false;
                    using (var view = UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(frame, 0))
                    {
                        if (!view.valid)
                            continue;
                        for (int sample = 0; sample < view.sampleCount && !target; sample++)
                        {
                            string name = view.GetSampleName(sample);
                            target = name == "NTSD.H06.Prerequisite.Driver.Tick130" ||
                                name == "NTSD.H06.Prerequisite.Driver.Tick162";
                        }
                    }
                    if (target)
                    {
                        if (count == frames.Length)
                            throw new InvalidOperationException("Unexpected duplicate target frames.");
                        frames[count++] = frame;
                    }
                }
                stateType.GetField("capturedFrames").SetValue(recovered, count);
                capture.GetMethod("ExportSamples", flags).Invoke(null, new object[] { recovered, "cpu-gc-samples-recovered.json" });
                stateType.GetField("status").SetValue(recovered, count > 0 ? "RETAINED_TARGET_FRAMES_PARSED" : "NO_TARGET_FRAME");
            }
            catch (Exception exception)
            {
                stateType.GetField("status").SetValue(recovered, "PARTIAL");
                stateType.GetField("error").SetValue(recovered, exception.ToString());
            }
            finally
            {
                capture.GetMethod("RestoreSettings", flags).Invoke(null, new[] { before });
                object after = capture.GetMethod("ReadSettings", flags).Invoke(null, null);
                stateType.GetField("after").SetValue(recovered, after);
                stateType.GetField("settingsRestored").SetValue(recovered, JsonUtility.ToJson(before) == JsonUtility.ToJson(after));
                using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(recovered, true));
            }
        }

        private static void BeforeReload()
        {
            Finish("assembly-reload-abort");
        }
    }

#if UNITY_INCLUDE_TESTS
    public sealed class H06RolePrerequisiteDiagnosticsTests
    {
        [NUnit.Framework.Test]
        public void RequestChangesOnlyFreshOutputFromOriginalOffFixture()
        {
            var actual = H06RolePrerequisiteDiagnostics.BuildRequest(0);
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildRoleRecordCostRequest(0);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("ROLE-PREREQUISITE-DIAG"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => H06RolePrerequisiteDiagnostics.BuildRequest(1));
        }

        [NUnit.Framework.Test]
        public void CompletedGcScopeRemainsReadableAfterAccept()
        {
            using (var observer = new BattleLogicTickGcObserverEditor())
            {
                observer.BeginDriverStep();
                GC.KeepAlive(new byte[64]);
                observer.EndDriverStep(true);
                var beforeAccept = observer.LastCompletedScopeForDiagnostics;
                observer.AcceptCompletedTick(1, true);
                var afterAccept = observer.LastCompletedScopeForDiagnostics;
                NUnit.Framework.Assert.That(beforeAccept.valid && beforeAccept.calibratedBefore, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(beforeAccept.allocationEvents, NUnit.Framework.Is.GreaterThan(0));
                NUnit.Framework.Assert.That(afterAccept.allocationEvents, NUnit.Framework.Is.EqualTo(beforeAccept.allocationEvents));
                NUnit.Framework.Assert.That(afterAccept.rawValueTotal, NUnit.Framework.Is.EqualTo(beforeAccept.rawValueTotal));
                var final = observer.Finish(1, 1);
                NUnit.Framework.Assert.That(final.coverageValid, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(final.steadyAllocationEvents, NUnit.Framework.Is.EqualTo(beforeAccept.allocationEvents));
            }
        }

    }
#endif
}
#endif
