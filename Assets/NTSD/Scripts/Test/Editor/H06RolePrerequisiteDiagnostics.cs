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

        internal const string QualificationRoot = "artifacts/diagnostics/NTSD-H06-ROLE-PREREQUISITE-REPAIR-20261010/windows";
        internal const string QualificationBaseline = OutputRoot + "/00-combat1000-prerequisites/report.json.record-cost.json";

        [Serializable]
        internal struct QualificationGate
        {
            public int failureMask, oldTimerMask;
            public int fallbackParticipants, inertParticipants, fallbackAttackOrdinals, fallbackBodyOrdinals, collector;
            public bool aborted;
        }

        internal static QualificationGate ReadQualificationGate(int tick, bool steady, bool expectedGate,
            SimulationWorld world, BruteForceSceneQuery query, BattleTickDetailPhaseDiagnostics detail)
        {
            var row = new QualificationGate();
            int mask = 0;
            mask = AddFailure(mask, query.EnableRoleRecordCostDiagnostics == expectedGate, 0);
            mask = AddFailure(mask, query.RoleBranchTimingSampleStrideForDiagnostics == 64, 1);
            mask = AddFailure(mask, !query.EnableRoleBranchTimingForDiagnostics, 2);
            mask = AddFailure(mask, !query.EnableRoleCapacityRecordGuardForDiagnostics, 3);
            mask = AddFailure(mask, !query.EnableRoleSnapshotFrameReuseForDiagnostics, 4);
            mask = AddFailure(mask, !query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics, 5);
            mask = AddFailure(mask, !query.EnableRoleEndpointSnapshotReuseForDiagnostics, 6);
            mask = AddFailure(mask, !world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics, 7);
            mask = AddFailure(mask, !world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, 8);
            mask = AddFailure(mask, !world.EnableCharacterInputBoundaryTimingForDiagnostics, 9);
            row.collector = (int)query.LastFormalCollectorModeForDiagnostics;
            row.aborted = query.LastFormalCollectionAbortedForDiagnostics;
            mask = AddFailure(mask, query.LastFormalCollectorModeForDiagnostics == CollisionFormalCollectorMode.ForceRoleAware, 10);
            mask = AddFailure(mask, !row.aborted, 11);
            query.GetLastRoleAwareFallbackOrdinalCountsForSelfCheck(out _, out row.fallbackAttackOrdinals,
                out _, out row.fallbackBodyOrdinals);
            row.fallbackParticipants = query.LastFormalFallbackParticipantCountForDiagnostics;
            row.inertParticipants = query.LastRoleAwareInertParticipantCountForDiagnostics;
            mask = AddFailure(mask, row.fallbackAttackOrdinals == 0 && row.fallbackBodyOrdinals == 0, 12);
            mask = AddFailure(mask, steady == (tick > 120), 13);
            for (int phase = 48; phase <= 53; phase++)
                if (detail.GetLastElapsedTimestampTicks((BattleTickDetailPhase)phase) != 0)
                    row.oldTimerMask |= 1 << (phase - 48);
            mask = AddFailure(mask, row.oldTimerMask == 0, 14);
            mask = AddFailure(mask, row.inertParticipants >= 0 && row.fallbackParticipants == row.inertParticipants, 15);
            row.failureMask = mask;
            return row;
        }

        [Serializable]
        internal sealed class ColdPreparation
        {
            public string api = "RuntimeHelpers.PrepareMethod + MethodHandle.GetFunctionPointer; no Invoke/delegate/business call";
            public string[] methods = new string[2];
            public int[] metadataTokens = new int[2];
            public string assemblyModuleVersion;
            public bool completed;
        }

        internal static IntPtr PrepareMethodWithoutInvocation(MethodInfo method)
        {
            if (method == null || method.IsAbstract || method.ContainsGenericParameters ||
                method.DeclaringType == null || method.DeclaringType.ContainsGenericParameters)
                throw new ArgumentException("Cold preparation requires a concrete closed method.", nameof(method));
            System.Runtime.CompilerServices.RuntimeHelpers.PrepareMethod(method.MethodHandle);
            IntPtr pointer = method.MethodHandle.GetFunctionPointer();
            if (pointer == IntPtr.Zero)
                throw new InvalidOperationException("Cold preparation returned no entry point.");
            return pointer;
        }

        internal static ColdPreparation PrepareCapturedMethods()
        {
            // Alignment contract: NTSD-H06-ROLE-PREREQUISITE-REPAIR-001. Two captured targets only; never invoke.
            Assembly assembly = typeof(SimulationWorld).Assembly;
            var methods = new[]
            {
                assembly.GetType("NTSD.Simulation.Ecs.BattleDamageWriter", true).GetMethod(
                    "RecordAlternateLeadSound", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly),
                assembly.GetType("NTSD.Simulation.Ecs.BattleCharacterActionWriter", true).GetMethod(
                    "RouteNativeRunning", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            };
            if (methods[0] == null || methods[1] == null)
                throw new MissingMethodException("Captured JIT targets changed; no alternate targets permitted.");
            var result = new ColdPreparation { assemblyModuleVersion = assembly.ManifestModule.ModuleVersionId.ToString() };
            for (int index = 0; index < methods.Length; index++)
            {
                PrepareMethodWithoutInvocation(methods[index]);
                result.methods[index] = methods[index].DeclaringType.FullName + "." + methods[index].Name;
                result.metadataTokens[index] = methods[index].MetadataToken;
            }
            result.completed = true;
            return result;
        }

        internal static ProductionEntityStressRequest BuildQualificationRequest(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            var request = BattleOptimizationWindowsAiSuiteEditor.BuildRoleRecordCostRequest(0);
            request.outputPath = QualificationRoot + "/00-combat1000-off/report.json";
            return request;
        }

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

        private static int preparationSideEffects;

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void ColdSentinel() { preparationSideEffects++; }

        private sealed class ColdInstanceSentinel
        {
            public ColdInstanceSentinel() { preparationSideEffects++; }
            [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
            public void Run() { preparationSideEffects++; }
        }

        private static void OpenGenericSentinel<T>() { preparationSideEffects++; }

        [NUnit.Framework.Test]
        public void Qualification_ColdPreparationDoesNotInvokeStaticOrInstanceBusiness()
        {
            preparationSideEffects = 0;
            var method = typeof(H06RolePrerequisiteDiagnosticsTests).GetMethod("ColdSentinel", BindingFlags.NonPublic | BindingFlags.Static);
            NUnit.Framework.Assert.That(H06RolePrerequisiteDiagnostics.PrepareMethodWithoutInvocation(method), NUnit.Framework.Is.Not.EqualTo(IntPtr.Zero));
            method = typeof(ColdInstanceSentinel).GetMethod("Run");
            NUnit.Framework.Assert.That(H06RolePrerequisiteDiagnostics.PrepareMethodWithoutInvocation(method), NUnit.Framework.Is.Not.EqualTo(IntPtr.Zero));
            NUnit.Framework.Assert.That(preparationSideEffects, NUnit.Framework.Is.Zero);
        }

        [NUnit.Framework.Test]
        public void Qualification_ColdPreparationRejectsMissingOrOpenTargets()
        {
            preparationSideEffects = 0;
            NUnit.Framework.Assert.Throws<ArgumentException>(() => H06RolePrerequisiteDiagnostics.PrepareMethodWithoutInvocation(null));
            var method = typeof(H06RolePrerequisiteDiagnosticsTests).GetMethod("OpenGenericSentinel", BindingFlags.NonPublic | BindingFlags.Static);
            NUnit.Framework.Assert.Throws<ArgumentException>(() => H06RolePrerequisiteDiagnostics.PrepareMethodWithoutInvocation(method));
            NUnit.Framework.Assert.That(preparationSideEffects, NUnit.Framework.Is.Zero);
        }

        [NUnit.Framework.Test]
        public void Qualification_RequestIsExactlyOneOriginalOffFixture()
        {
            var actual = H06RolePrerequisiteDiagnostics.BuildQualificationRequest(0);
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildRoleRecordCostRequest(0);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("ROLE-PREREQUISITE-REPAIR"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => H06RolePrerequisiteDiagnostics.BuildQualificationRequest(1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => H06RolePrerequisiteDiagnostics.BuildQualificationRequest(-1));
        }

        private sealed class QualificationFixture : IDisposable
        {
            internal readonly SimulationWorld world;
            internal readonly BruteForceSceneQuery query;
            internal readonly NTSD.Animation.LF2Objects.LF2Character attacker, target, inert;
            internal readonly BattleTickDetailPhaseDiagnostics detail;
            private static object FixtureCall(string name, params object[] args) =>
                typeof(NTSD.Test.RoleAwareCollisionFormalCollectorSelfCheckTests).GetMethod(name,
                    BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, args);

            internal QualificationFixture()
            {
                object[] args = { 0, null, null, null, null };
                FixtureCall("CreatePairSnapshotReuseScenario", args);
                world = (SimulationWorld)args[1];
                query = (BruteForceSceneQuery)args[2];
                attacker = (NTSD.Animation.LF2Objects.LF2Character)args[3];
                target = (NTSD.Animation.LF2Objects.LF2Character)args[4];
                object frame = FixtureCall("MakeFrame", null, null);
                inert = (NTSD.Animation.LF2Objects.LF2Character)FixtureCall("CreateEndpointSnapshotCharacter",
                    "Prerequisite_Inert", 3332, frame, null);
                FixtureCall("Register", world, inert, 2, 3, 123);
                query.PrepareBattleCapacity(3, 8, 8);
                query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
                query.RoleBranchTimingSampleStrideForDiagnostics = 64;
                detail = world.EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
                Collect();
            }

            internal void Collect()
            {
                detail.BeginTick(1);
                world.CaptureCollisionFrameSnapshotsAll();
                world.CollectCollisionCandidatesAll();
                world.EndCollisionCandidateConsumption();
            }

            internal H06RolePrerequisiteDiagnostics.QualificationGate Read() =>
                H06RolePrerequisiteDiagnostics.ReadQualificationGate(1, false, false, world, query, detail);

            public void Dispose()
            {
                query.BeforeRoleAwareFormalInputValidationForSelfCheck = null;
                world.Unregister(attacker);
                world.Unregister(target);
                world.Unregister(inert);
            }
        }

        [NUnit.Framework.Test]
        public void Qualification_ActualInertOnlyCollectionPasses()
        {
            using (var f = new QualificationFixture())
            {
                var row = f.Read();
                NUnit.Framework.Assert.That(row.inertParticipants, NUnit.Framework.Is.EqualTo(1));
                NUnit.Framework.Assert.That(row.fallbackParticipants, NUnit.Framework.Is.EqualTo(1));
                NUnit.Framework.Assert.That(row.fallbackAttackOrdinals + row.fallbackBodyOrdinals, NUnit.Framework.Is.Zero);
                NUnit.Framework.Assert.That(row.failureMask, NUnit.Framework.Is.Zero);
            }
        }

        [NUnit.Framework.TestCase(true)]
        [NUnit.Framework.TestCase(false)]
        public void Qualification_ActualFallbackOrdinalListsFailDespiteInertEquality(bool attack)
        {
            using (var f = new QualificationFixture())
            {
                // Controlled ordinal-list fault at a valid registered participant; do not invent a geometry cause.
                string name = attack ? "_roleFormalFallbackAttackOrdinals" : "_roleFormalFallbackBodyOrdinals";
                var ordinals = (System.Collections.Generic.List<int>)typeof(BruteForceSceneQuery).GetField(name,
                    BindingFlags.Instance | BindingFlags.NonPublic).GetValue(f.query);
                ordinals.Add(attack ? 0 : 1);
                try
                {
                    var row = f.Read();
                    NUnit.Framework.Assert.That(row.fallbackParticipants, NUnit.Framework.Is.EqualTo(row.inertParticipants));
                    NUnit.Framework.Assert.That(attack ? row.fallbackAttackOrdinals : row.fallbackBodyOrdinals, NUnit.Framework.Is.EqualTo(1));
                    NUnit.Framework.Assert.That(row.failureMask, NUnit.Framework.Is.EqualTo(1 << 12));
                }
                finally { ordinals.RemoveAt(ordinals.Count - 1); }
            }
        }

        [NUnit.Framework.Test]
        public void Qualification_AbortedCollectionAndWrongFlagsRemainRejected()
        {
            using (var f = new QualificationFixture())
            {
                f.query.EnableRoleSnapshotFrameReuseForDiagnostics = true;
                NUnit.Framework.Assert.That(f.Read().failureMask, NUnit.Framework.Is.EqualTo(1 << 4));
                f.query.EnableRoleSnapshotFrameReuseForDiagnostics = false;
                f.query.BeforeRoleAwareFormalInputValidationForSelfCheck = () =>
                {
                    f.target.Runtime.SetPosition(11, 0, 0);
                    f.target.Runtime.SyncIntegerPosition();
                };
                f.Collect();
                NUnit.Framework.Assert.That(f.query.LastFormalCollectionAbortedForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(f.Read().failureMask & (1 << 11), NUnit.Framework.Is.Not.Zero);
            }
        }

        [NUnit.Framework.Test]
        public void Qualification_TwoCapturedMethodsPrepareWithoutWorldOrRngChanges()
        {
            using (var f = new QualificationFixture())
            {
                ulong before = f.world.CaptureRuntimeChecksum64(1, default);
                uint rngState = f.world.Rng.State;
                ulong rngCalls = f.world.Rng.CallCount;
                string native = JsonUtility.ToJson(f.world.NativeRandom.CaptureScalarState());
                var prepared = H06RolePrerequisiteDiagnostics.PrepareCapturedMethods();
                NUnit.Framework.Assert.That(prepared.completed, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(prepared.methods.Length, NUnit.Framework.Is.EqualTo(2));
                NUnit.Framework.Assert.That(prepared.methods[0], NUnit.Framework.Does.EndWith(".RecordAlternateLeadSound"));
                NUnit.Framework.Assert.That(prepared.methods[1], NUnit.Framework.Does.EndWith(".RouteNativeRunning"));
                NUnit.Framework.Assert.That(f.world.CaptureRuntimeChecksum64(1, default), NUnit.Framework.Is.EqualTo(before));
                NUnit.Framework.Assert.That(f.world.Rng.State, NUnit.Framework.Is.EqualTo(rngState));
                NUnit.Framework.Assert.That(f.world.Rng.CallCount, NUnit.Framework.Is.EqualTo(rngCalls));
                NUnit.Framework.Assert.That(JsonUtility.ToJson(f.world.NativeRandom.CaptureScalarState()), NUnit.Framework.Is.EqualTo(native));
            }
        }


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
