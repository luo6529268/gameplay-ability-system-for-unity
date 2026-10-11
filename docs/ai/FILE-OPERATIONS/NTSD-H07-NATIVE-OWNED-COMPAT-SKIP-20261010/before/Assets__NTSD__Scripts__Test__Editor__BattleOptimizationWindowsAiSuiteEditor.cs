#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Animation.Rendering.Editor;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class BattleOptimizationWindowsAiSuiteEditor
    {
        private static bool startingInputCpuDiagnostic;
        [MenuItem("NTSD/Validation/Optimization/H07 Input CPU Diagnostic")]
        private static void BeginInputCpuDiagnostic()
        {
            Require(inputCostObserver == null && !startingInputCostDiagnostic,
                "Input CPU diagnosis already has an owner.");
            Require(!UnityEngine.Profiling.Profiler.enabled, "Input CPU diagnosis requires Profiler OFF.");
            startingInputCpuDiagnostic = true;
            startingInputCostDiagnostic = true;
            H07InputCostDiagnostics.ResetBaseline();
            try { BeginPostBindingsRole(); }
            finally { startingInputCostDiagnostic = false; startingInputCpuDiagnostic = false; }
        }

        private static bool startingInputFlowDiagnostic;
        [MenuItem("NTSD/Validation/Optimization/H07 Full Input Flow Diagnostic")]
        private static void BeginInputFlowDiagnostic()
        {
            Require(!startingInputCostDiagnostic && inputCostObserver == null,
                "Input flow diagnosis already has an owner.");
            Require(!UnityEngine.Profiling.Profiler.enabled, "Input flow diagnosis requires Profiler initially OFF.");
            startingInputFlowDiagnostic = true;
            startingInputCostDiagnostic = true;
            H07InputCostDiagnostics.ResetBaseline();
            try { BeginPostBindingsRole(); }
            finally { startingInputCostDiagnostic = false; startingInputFlowDiagnostic = false; }
        }

        private static bool startingInputCostDiagnostic;
        private static H07InputCostDiagnostics inputCostObserver;

        [MenuItem("NTSD/Validation/Optimization/H07 Input Cost Diagnostic")]
        private static void BeginInputCostDiagnostic()
        {
            Require(!startingInputCostDiagnostic && inputCostObserver == null,
                "Input cost diagnosis already has an owner.");
            Require(!UnityEngine.Profiling.Profiler.enabled, "Input cost diagnosis requires Profiler OFF.");
            startingInputCostDiagnostic = true;
            H07InputCostDiagnostics.ResetBaseline();
            try { BeginPostBindingsRole(); }
            finally { startingInputCostDiagnostic = false; }
        }

        private const string RoleRecordCostOutputRoot = "artifacts/diagnostics/NTSD-H06-ROLE-RECORD-COST-DIAG-20261010/windows";
        private static bool startingRoleRecordCostDiagnostic;
        private static bool startingRoleRecordPrerequisiteDiagnostic;
        private static bool startingRolePrerequisiteQualification;

        [MenuItem("NTSD/Validation/Optimization/H06 Role Prerequisite Qualification")]
        private static void BeginRolePrerequisiteQualification()
        {
            Require(!startingRoleRecordCostDiagnostic && !startingRoleRecordPrerequisiteDiagnostic &&
                !startingRolePrerequisiteQualification && roleRecordTickObserver == null,
                "The prerequisite qualification already has an owner.");
            Require(!UnityEngine.Profiling.Profiler.enabled, "Qualification requires Profiler recording OFF.");
            startingRolePrerequisiteQualification = true;
            startingRoleRecordCostDiagnostic = true;
            roleRecordBaseline = null;
            try { BeginPostBindingsRole(); }
            finally { startingRolePrerequisiteQualification = false; startingRoleRecordCostDiagnostic = false; }
        }

        [MenuItem("NTSD/Validation/Optimization/H06 Role Prerequisite Diagnostic")]
        private static void BeginRoleRecordPrerequisiteDiagnostic()
        {
            Require(!startingRoleRecordCostDiagnostic && !startingRoleRecordPrerequisiteDiagnostic && roleRecordTickObserver == null,
                "The prerequisite diagnostic already has an owner.");
            startingRoleRecordCostDiagnostic = true;
            startingRoleRecordPrerequisiteDiagnostic = true;
            roleRecordBaseline = null;
            try { BeginPostBindingsRole(); }
            finally
            {
                startingRoleRecordPrerequisiteDiagnostic = false;
                startingRoleRecordCostDiagnostic = false;
            }
        }
        private static RoleRecordTickObserver roleRecordTickObserver;
        private static RoleRecordTick[] roleRecordBaseline;

        [MenuItem("NTSD/Validation/Optimization/H06 Role Record Cost Diagnostic")]
        private static void BeginRoleRecordCostDiagnostic()
        {
            Require(!startingRoleRecordCostDiagnostic && roleRecordTickObserver == null,
                "The finite record diagnostic already has an owner.");
            startingRoleRecordCostDiagnostic = true;
            roleRecordBaseline = null;
            try { BeginPostBindingsRole(); }
            finally { startingRoleRecordCostDiagnostic = false; }
        }

        internal static ProductionEntityStressRequest BuildRoleRecordCostRequest(int index)
        {
            if (index < 0 || index > 1) throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildScopedRoleAdmissionRequest(3);
            request.outputPath = RoleRecordCostOutputRoot + (index == 0
                ? "/00-combat1000-off/report.json" : "/01-combat1000-on/report.json");
            return request;
        }

        [Serializable]
        private sealed class RoleRecordTick
        {
            public int tick, playerInputCount;
            public bool steady, actualFlagsValid;
            public H06RolePrerequisiteDiagnostics.QualificationGate qualificationGate;
            public ulong fullRuntimeChecksum, rngCalls;
            public uint rngState, crtState, tableSeed, lastSynchronizedCallSite;
            public ulong crtCalls, synchronizedCalls, synchronizedTableHash;
            public int synchronizedCounter, synchronizedIndex;
            public BruteForceSceneQuery.RoleRecordCostCounters counters;
            public long[] elapsed = new long[7];
        }

        [Serializable]
        private sealed class RoleRecordEvidence
        {
            public int gateVersion = 2;
            public string gateContract = "v2: original bits0..11/13..14; bit12 true attack/body fallback zero; bit15 total equals nonnegative inert; forensic entry remains v1.";
            public H06RolePrerequisiteDiagnostics.ColdPreparation coldPreparation;
            public string comparisonBaseline;
            public bool qualificationNoProfiler;
            public string checksumScope = "Existing CaptureRuntimeChecksum64 schema after completed Driver, including full applied FrameInputSet and runtime AI input fields; not canonical JSON snapshots.";
            public string counterScope = "Per collection. Eligibility counts all directions/records; remaining counters and seven clocks sampled directions only. Warm1-120, steady121-300. No extrapolation.";
            public long stopwatchFrequency = System.Diagnostics.Stopwatch.Frequency;
            public bool enabled, settingsRestored;
            public int ticks, parityMismatches, actualFlagFailures;
            public RoleRecordTick[] observations = new RoleRecordTick[300];
            public long[] emptyBoundaryTicks = new long[1024];
            public long[] sevenBoundaryWallTicks = new long[1024];
            public long[] sevenBoundaryRecordedTicks = new long[1024];
        }

        private sealed class RoleRecordTickObserver : IDisposable
        {
            private readonly ProductionEntityStressRunner runner;
            private readonly SimulationTickDriver driver;
            private readonly SimulationWorld world;
            private readonly BruteForceSceneQuery query;
            private readonly BattleLogicTickGcObserverEditor gc;
            private readonly BattleTickDetailPhaseDiagnostics detail;
            private readonly bool previousEnabled;
            private readonly int previousStride;
            private bool attached, disposed;
            internal readonly RoleRecordEvidence evidence = new RoleRecordEvidence();

            internal RoleRecordTickObserver(ProductionEntityStressRunner runner, SimulationTickDriver driver,
                BruteForceSceneQuery query, BattleLogicTickGcObserverEditor gc, bool enabled)
            {
                this.runner = runner;
                this.driver = driver;
                world = driver.World;
                this.query = query;
                this.gc = gc;
                detail = world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                Require(detail != null && detail.Enabled && !runner.DriverTickObserverAttachedForDiagnostics &&
                    runner.Report.logicTicksExecuted == 0, "Record observation requires an unowned first-tick boundary.");
                Require(!query.EnableRoleRecordCostDiagnostics && !query.EnableRoleBranchTimingForDiagnostics &&
                    !query.EnableRoleCapacityRecordGuardForDiagnostics && !query.EnableRoleSnapshotFrameReuseForDiagnostics &&
                    !query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics && !query.EnableRoleEndpointSnapshotReuseForDiagnostics &&
                    !world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics &&
                    !world.EnableCharacterInputProducerSnapshotReuseForDiagnostics && !world.EnableCharacterInputBoundaryTimingForDiagnostics,
                    "Record diagnosis cannot enable previous candidates or input instrumentation.");
                if (state.roleRecordPrerequisiteDiagnostic)
                    H06RolePrerequisiteDiagnostics.Start();
                evidence.gateVersion = state.roleRecordPrerequisiteDiagnostic ? 1 : 2;
                if (state.rolePrerequisiteQualification)
                {
                    Require(!enabled && !UnityEngine.Profiling.Profiler.enabled, "Qualification must remain OFF without Profiler capture.");
                    var baseline = JsonUtility.FromJson<RoleRecordEvidence>(File.ReadAllText(PathFor(H06RolePrerequisiteDiagnostics.QualificationBaseline)));
                    Require(baseline != null && baseline.ticks == 300 && baseline.observations.Length == 300,
                        "The fixed 300-tick prerequisite baseline is required.");
                    for (int index = 0; index < 300; index++)
                        Require(baseline.observations[index] != null && baseline.observations[index].tick == index + 1,
                            "Prerequisite baseline sequence is incomplete.");
                    roleRecordBaseline = baseline.observations;
                    evidence.comparisonBaseline = H06RolePrerequisiteDiagnostics.QualificationBaseline;
                    evidence.coldPreparation = H06RolePrerequisiteDiagnostics.PrepareCapturedMethods();
                    evidence.qualificationNoProfiler = true;
                }
                previousEnabled = query.EnableRoleRecordCostDiagnostics;
                previousStride = query.RoleBranchTimingSampleStrideForDiagnostics;
                evidence.enabled = enabled;
                for (int index = 0; index < evidence.observations.Length; index++)
                    evidence.observations[index] = new RoleRecordTick();
                MeasureBoundaryOverhead(evidence);
                query.RoleBranchTimingSampleStrideForDiagnostics = 64;
                query.EnableRoleRecordCostDiagnostics = enabled;
                try
                {
                    runner.ConfigureDriverTickObserverForDiagnostics(BeginObservedDriverStep, EndObservedDriverStep, Accept);
                    attached = true;
                }
                catch { Dispose(); throw; }
            }

            private void BeginObservedDriverStep()
            {
                gc.BeginDriverStep();
                if (state.roleRecordPrerequisiteDiagnostic)
                    H06RolePrerequisiteDiagnostics.BeginDriver(evidence.ticks + 1);
            }

            private void EndObservedDriverStep(bool stepped)
            {
                try
                {
                    if (state.roleRecordPrerequisiteDiagnostic)
                        H06RolePrerequisiteDiagnostics.EndDriver();
                }
                finally { gc.EndDriverStep(stepped); }
            }

            private void Accept(int tick, bool steady)
            {
                gc.AcceptCompletedTick(tick, steady);
                if (tick != evidence.ticks + 1 || tick > 300)
                    throw new InvalidOperationException("Record observation lost the fixed 300-tick boundary.");
                RoleRecordTick row = evidence.observations[tick - 1];
                row.tick = tick;
                row.steady = steady;
                row.playerInputCount = driver.LastAppliedFrameInput.Players?.Count ?? 0;
                row.fullRuntimeChecksum = world.CaptureRuntimeChecksum64(driver.CurrentTickIndex, driver.LastAppliedFrameInput);
                row.rngState = world.Rng.State;
                row.rngCalls = world.Rng.CallCount;
                var native = world.NativeRandom.CaptureScalarState();
                row.crtState = native.CrtState;
                row.crtCalls = native.CrtCalls;
                row.tableSeed = native.TableSeed;
                row.synchronizedCounter = native.SynchronizedCounter;
                row.synchronizedIndex = native.SynchronizedIndex;
                row.synchronizedCalls = native.SynchronizedCalls;
                row.lastSynchronizedCallSite = native.LastSynchronizedCallSite;
                row.synchronizedTableHash = native.SynchronizedTableHash;
                row.counters = query.LastRoleRecordCostCounters;
                for (int phase = 0; phase < 7; phase++)
                    row.elapsed[phase] = detail.GetLastElapsedTimestampTicks((BattleTickDetailPhase)(54 + phase));
                row.actualFlagsValid = query.EnableRoleRecordCostDiagnostics == evidence.enabled &&
                    query.RoleBranchTimingSampleStrideForDiagnostics == 64 && !query.EnableRoleBranchTimingForDiagnostics &&
                    !query.EnableRoleCapacityRecordGuardForDiagnostics && !query.EnableRoleSnapshotFrameReuseForDiagnostics &&
                    !query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics && !query.EnableRoleEndpointSnapshotReuseForDiagnostics &&
                    !world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics &&
                    !world.EnableCharacterInputProducerSnapshotReuseForDiagnostics && !world.EnableCharacterInputBoundaryTimingForDiagnostics &&
                    query.LastFormalCollectorModeForDiagnostics == CollisionFormalCollectorMode.ForceRoleAware &&
                    !query.LastFormalCollectionAbortedForDiagnostics && query.LastFormalFallbackParticipantCountForDiagnostics == 0 &&
                    steady == (tick > 120);
                for (int phase = 48; phase <= 53; phase++)
                    row.actualFlagsValid &= detail.GetLastElapsedTimestampTicks((BattleTickDetailPhase)phase) == 0;
                if (!state.roleRecordPrerequisiteDiagnostic)
                {
                    row.qualificationGate = H06RolePrerequisiteDiagnostics.ReadQualificationGate(
                        tick, steady, evidence.enabled, world, query, detail);
                    row.actualFlagsValid = row.qualificationGate.failureMask == 0;
                }
                if (state.rolePrerequisiteQualification)
                {
                    evidence.qualificationNoProfiler &= !UnityEngine.Profiling.Profiler.enabled;
                    row.actualFlagsValid &= evidence.qualificationNoProfiler;
                }
                if (!row.actualFlagsValid) evidence.actualFlagFailures++;
                if (state.roleRecordPrerequisiteDiagnostic)
                    H06RolePrerequisiteDiagnostics.Observe(tick, steady, evidence.enabled, row.actualFlagsValid,
                        world, query, detail, gc.LastCompletedScopeForDiagnostics, runner.Report);
                if ((evidence.enabled || state.rolePrerequisiteQualification) && (roleRecordBaseline == null || !RoleRecordTicksEqual(roleRecordBaseline[tick - 1], row)))
                    evidence.parityMismatches++;
                evidence.ticks++;
            }

            public void Dispose()
            {
                if (disposed) return;
                try { if (attached) runner.ConfigureDriverTickObserverForDiagnostics(null, null, null); }
                finally
                {
                    query.EnableRoleRecordCostDiagnostics = previousEnabled;
                    query.RoleBranchTimingSampleStrideForDiagnostics = previousStride;
                    evidence.settingsRestored = query.EnableRoleRecordCostDiagnostics == previousEnabled &&
                        query.RoleBranchTimingSampleStrideForDiagnostics == previousStride;
                    attached = false;
                    disposed = true;
                    if (state.roleRecordPrerequisiteDiagnostic)
                        H06RolePrerequisiteDiagnostics.Finish("prerequisite-observer-dispose");
                }
            }
        }

        private static bool RoleRecordTicksEqual(RoleRecordTick left, RoleRecordTick right) =>
            left.tick == right.tick && left.steady == right.steady && left.playerInputCount == right.playerInputCount &&
            left.fullRuntimeChecksum == right.fullRuntimeChecksum && left.rngState == right.rngState && left.rngCalls == right.rngCalls &&
            left.crtState == right.crtState && left.crtCalls == right.crtCalls && left.tableSeed == right.tableSeed &&
            left.synchronizedCounter == right.synchronizedCounter && left.synchronizedIndex == right.synchronizedIndex &&
            left.synchronizedCalls == right.synchronizedCalls && left.lastSynchronizedCallSite == right.lastSynchronizedCallSite &&
            left.synchronizedTableHash == right.synchronizedTableHash;

        private static void MeasureBoundaryOverhead(RoleRecordEvidence evidence)
        {
            var calibration = new BattleTickDetailPhaseDiagnostics();
            calibration.SetEnabled(true);
            for (int sample = 0; sample < 1024; sample++)
            {
                long start = System.Diagnostics.Stopwatch.GetTimestamp();
                evidence.emptyBoundaryTicks[sample] = System.Diagnostics.Stopwatch.GetTimestamp() - start;
                calibration.BeginTick(sample);
                long wall = System.Diagnostics.Stopwatch.GetTimestamp();
                long segment = System.Diagnostics.Stopwatch.GetTimestamp();
                for (int index = 0; index < 7; index++)
                {
                    long now = System.Diagnostics.Stopwatch.GetTimestamp();
                    calibration.RecordPhaseElapsed((BattleTickDetailPhase)(54 + index), now - segment);
                    segment = System.Diagnostics.Stopwatch.GetTimestamp();
                }
                evidence.sevenBoundaryWallTicks[sample] = System.Diagnostics.Stopwatch.GetTimestamp() - wall;
                for (int index = 0; index < 7; index++)
                    evidence.sevenBoundaryRecordedTicks[sample] += calibration.GetLastElapsedTimestampTicks((BattleTickDetailPhase)(54 + index));
            }
            calibration.SetEnabled(false);
        }

        private static void FinishRoleRecordObservation(RunState run, bool aborted)
        {
            RoleRecordTickObserver owned = roleRecordTickObserver;
            roleRecordTickObserver = null;
            if (owned == null) return;
            owned.Dispose();
            RoleRecordEvidence evidence = owned.evidence;
            run.roleRecordObservationValid = !aborted && evidence.ticks == 300 && evidence.settingsRestored &&
                evidence.actualFlagFailures == 0 && evidence.parityMismatches == 0;
            if (!evidence.enabled && run.roleRecordObservationValid && run.zeroGcPassed)
                roleRecordBaseline = evidence.observations;
            SaveNew(run.reportPath + (aborted ? ".record-cost.aborted.json" : ".record-cost.json"), JsonUtility.ToJson(evidence, true));
        }

        private const string SessionKey = "NTSD.Optimization.Batch26.WindowsAiSuite";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string OutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH26-H07-CONTINUE-20261007/windows-01";
        private const string RoleCandidateOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH29-ROLE-COLLECTOR-ADMISSION-20261007/windows-01";
        private const string RoleFormalOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH30-ROLE-FORMAL-WINDOWS-20261007/windows-01";
        private const string PairSnapshotOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007/windows-01";
        private const string EmptyItrGuardOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH32-BRUTE-EMPTY-ITR-GUARD-20261007/windows-01";
        private const string EmptyItrRosterOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH34-BRUTE-EMPTY-ITR-ROSTER-20261007/windows-01";
        private const string BruteExactCacheOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH36-BRUTE-EXACT-CACHE-20261007/windows-01";
        private const string BruteGeometryFirstOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH37-BRUTE-GEOMETRY-FIRST-20261007/windows-01";
        private const string CpuGcCaptureOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH39-CPU-GC-CAPTURE-20261007/windows-01";
        private const string BruteProductionOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH40-BRUTE-PRODUCTION-ADMISSION-20261007/windows-01";
        private const string BruteBranchTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH44-BRUTE-BRANCH-TIMING-20261007/windows-01";
        private const string BruteSampledTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH45-BRUTE-SAMPLED-TIMING-20261007/windows-01";
        private const string BruteKind5PresenceOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH38-BRUTE-KIND5-PRESENCE-20261007/windows-01";
        private const string BruteEligibilityReuseOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH50-ELIGIBILITY-WINDOWS-20261007/windows-01";
        private const string BruteCombinedCacheOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH52-COMBINED-CACHE-WINDOWS-20261008/windows-01";
        private const string LogicGcScopeOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH54-LOGIC-GC-SCOPE-20261008/windows-01";
        private const string LogicCallsiteOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH55-LOGIC-GC-CALLSITE-20261008/windows-01";
        private const string LabelPrewarmOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH57-LABEL-PREWARM-WINDOWS-20261008/windows-01";
        private const string BruteCoarseEnvelopeOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH60-COARSE-ENVELOPE-WINDOWS-20261008/windows-01";
        private const string BruteEnvelopeBindingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH63-ENVELOPE-BINDING-WINDOWS-20261008/windows-01";
        private const string BruteEnvelopeBindingEligibilityOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH64-ENVELOPE-BINDING-ELIGIBILITY-20261008/windows-01";
        private const string BruteEnvelopeBranchTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH65-ENVELOPE-PATH-BRANCH-TIMING-20261008/windows-01";
        private const string RenderTextureBindingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008/windows-01";
        private const string SharedRequest = "Temp/NTSD_ProductionEntityStress.request.json";
        private const string BruteOrdinalPacketOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH72-ORDINAL-PACKET-WINDOWS-20261008/windows-01";
        private const string SharedResult = "Temp/NTSD_ProductionEntityStress.result";
        private const string CentralSegmentReadbackOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008/windows-02";
        private static bool startingCentralSegmentReadback;
        private const string MatchCentralBindingsEvidenceRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH80-SETPASS-MATCH-BINDINGS-20261008";
        private const string MatchCentralBindingsOutputRoot = MatchCentralBindingsEvidenceRoot + "/windows-02";
        private static bool startingMatchCentralBindings;
        private const string PostBindingsRoleOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH81-POST-BINDINGS-ROLE-WINDOWS-20261009/windows-01";
        private static bool startingPostBindingsRoleWindow;
        private const string ScopedRoleAdmissionOutputRoot = "artifacts/diagnostics/NTSD-H06-ROLE-DEFAULT-ADMISSION-20261009/windows";
        private static bool startingScopedRoleAdmissionWindow;
        private const string ScopedProducerAdmissionOutputRoot = "artifacts/diagnostics/NTSD-H07-PRODUCER-SNAPSHOT-ADMISSION-20261009/windows";
        private static bool startingScopedProducerAdmissionWindow;
        private const string ProducerAbbaOutputRoot = "artifacts/diagnostics/NTSD-H07-PRODUCER-ABBA-DIAG-20261009/windows";
        private static bool startingProducerAbbaDiagnostic;
        private static ProducerSnapshotExperimentCase producerSnapshotReuseCase;

        [Serializable]
        internal sealed class ProducerSnapshotExperimentCase
        {
            public int sequenceIndex;
            public string action;
            public bool reuseEnabled;
            public bool inputBoundaryTimingEnabled;
        }
        private const string RoleBranchTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH83-ROLE-EXACT-BRANCH-TIMING-20261009/windows-01";
        private static bool startingRoleBranchTimingWindow;
        private static BruteForceSceneQuery roleBranchTimingQuery;
        private static bool previousRoleBranchTiming;
        private static int previousRoleBranchTimingStride = 64;
        private const string InputBoundaryTimingOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH85-INPUT-BOUNDARY-TIMING-20261009/windows-01";
        private static bool startingInputBoundaryTimingWindow;
        private static SimulationWorld inputBoundaryTimingWorld;
        private static bool previousInputBoundaryTiming;
        private const string NativeSampledStateOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/windows-01";
        private static bool startingNativeSampledStateTimingWindow;
        private const string ProducerSnapshotReuseOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH88-PRODUCER-SNAPSHOT-WINDOWS-20261009/windows-01";
        private const string ProducerSnapshotReuseRetryOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH88-PRODUCER-SNAPSHOT-WINDOWS-20261009/windows-02";
        private static bool startingProducerSnapshotReuseWindow;
        private static SimulationWorld producerSnapshotReuseWorld;
        private static bool previousProducerSnapshotReuse;
        private static bool previousProducerSnapshotBoundaryTiming;
        private const string RoleGroupSnapshotSkipOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/windows-01";
        private static bool startingRoleGroupSnapshotSkipWindow;
        private static SimulationWorld roleGroupSnapshotSkipWorld;
        private static BruteForceSceneQuery roleGroupSnapshotSkipQuery;
        private static bool previousRoleGroupSnapshotSkip;
        private static readonly PropertyInfo MatchBindingsEnabled =
            typeof(CharacterAnimtorManager).GetProperty("MatchCentralBindingsEnabledForDiagnostics",
                BindingFlags.NonPublic | BindingFlags.Static);
        private static RunState centralSegmentReadbackRun;
        private static readonly Type ConfigType = typeof(ProductionEntityStressRunner).Assembly.GetType(
            "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
        private static readonly MethodInfo FromRequest = ConfigType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo StartRun = typeof(ProductionEntityStressRunner).GetMethod("StartRun", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo ServicesReady = typeof(ProductionEntityStressRunner).GetMethod("AreProductionServicesReady", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo StopRun = typeof(ProductionEntityStressRunner).GetMethod("StopAndCleanup", BindingFlags.Instance | BindingFlags.NonPublic);
        private static SuiteState state;
        private static ProductionEntityStressReport currentReport;
        private static double nextObservation;
        private static double nextProgress;
        private static int progressOrdinal;
        private static BruteForceSceneQuery emptyItrGuardQuery;
        private static BruteForceSceneQuery productionQuery;
        private static BruteForceSceneQuery branchTimingQuery;
        private static bool previousBranchTiming;
        private static int previousBranchTimingSampleStride = 1;
        private static bool previousEmptyItrGuard;
        private static bool previousEmptyItrRoster;
        private static bool previousBruteExactCache;
        private static bool previousBruteGeometryFirst;
        private static BruteForceSceneQuery kind5PresenceQuery;
        private static bool previousKind5Presence;
        private static BruteForceSceneQuery eligibilityReuseQuery;
        private static bool previousEligibilityReuse;
        private static bool startingCoarseEnvelopeWindow;
        private static BruteForceSceneQuery coarseEnvelopeQuery;
        private static bool previousCoarseEnvelope;
        private static bool startingEnvelopeBindingWindow;
        private static BruteForceSceneQuery envelopeBindingQuery;
        private static bool previousEnvelopeBindingEnvelope;
        private static bool previousEnvelopeBindingReuse;
        private static bool startingEnvelopeBindingEligibilityWindow;
        private static bool envelopeBindingEligibilityOwner;
        private static bool previousEnvelopeBindingEligibility;
        private static bool startingEnvelopeBranchTimingWindow;
        private static bool envelopeBindingBranchTimingOwner;
        private static bool previousEnvelopeBranchTiming;
        private static bool startingRenderTextureBindingWindow;
        private static bool renderTextureBindingReuseOwner;
        private static bool previousRenderTextureBindingReuse;
        private static RunState renderTextureBindingReuseRun;
        private static long lastRenderTextureBindingExecuteSequence;
        private static readonly string RenderTextureBindingOwnerError =
            "The renderer texture binding window requires exclusive ownership and a default-OFF flag.";
        private static readonly string RenderTextureBindingDriftError =
            "The owned renderer texture binding flag drifted during the fixed window.";
        private static readonly string RenderTextureBindingObservationError =
            "The fixed window requires fresh valid body recordings from its selected camera.";
        private static readonly string EnvelopeBindingModeError =
            "The envelope binding window requires production defaults, calibrated scope and exclusive ownership without another capture or candidate.";
        private static readonly string EnvelopeBindingOwnerError =
            "The envelope binding comparison requires an unowned query with four production defaults and both candidate flags initially off.";
        private static readonly string EnvelopeBindingDriftError =
            "The owned envelope or binding flag, production defaults or excluded candidate drifted during the fixed window.";
        private static readonly string CoarseEnvelopeModeError =
            "The coarse envelope window requires unchanged production defaults, calibrated scope and no other capture or candidate.";
        private static readonly string CoarseEnvelopeOwnerError =
            "The coarse envelope comparison requires an unowned query, four production defaults and no other candidate or timing flag.";
        private static readonly string CoarseEnvelopeDriftError =
            "The owned coarse envelope flag or production defaults drifted during the fixed window.";
        private static BattleLogicTickGcObserverEditor logicGcObserver;
        private static bool startingOrdinalPacketWindow;
        private static BruteForceSceneQuery ordinalPacketQuery;
        private static RunState ordinalPacketRun;
        private static bool previousOrdinalPacket;
        private const string BruteDepthRejectOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH77-BRUTE-DEPTH-WINDOWS-20261008/windows-01";
        private static bool startingDepthRejectWindow;
        private static BruteForceSceneQuery depthRejectQuery;
        private static RunState depthRejectRun;
        private static bool previousDepthReject;

        [Serializable]
        private sealed class RunState
        {
            public bool inputCostObservationValid;
            public bool roleGroupSnapshotSkipEnabled;
            public bool roleGroupSnapshotSkipFlagApplied;
            public bool roleGroupSnapshotSkipUnchanged;
            public bool roleGroupSnapshotSkipRestored;
            public int roleGroupSnapshotSkipObservedCollections;
            public long roleGroupSnapshotSkipMaximumObservedRejects;
            public string roleGroupSnapshotSkipObservationScope = "Last completed collection at sampled Editor observations; maxima are not all-tick totals.";
            public bool producerSnapshotReuseEnabled;
            public ProducerSnapshotExperimentCase producerExperimentCase;
            public bool producerSnapshotReuseFlagApplied;
            public bool producerSnapshotReuseUnchanged;
            public bool producerSnapshotReuseRestored;
            public int producerSnapshotReuseObservedPasses;
            public int producerSnapshotReuseMaximumObservedSkips;
            public int producerSnapshotReuseMaximumObservedRefreshes;
            public string producerSnapshotReuseObservationScope = "Last completed input pass at sampled Editor observations; maxima are not all-tick totals.";
            public bool postBindingsRoleConfigured;
            public bool inputBoundaryTimingEnabled;
            public bool inputBoundaryTimingFlagApplied;
            public bool inputBoundaryTimingUnchanged;
            public bool inputBoundaryTimingRestored;
            public bool inputBoundaryTimingCountsAvailable;
            public long[] inputBoundaryTimingPhaseCalls = new long[4];
            public bool postBindingsRoleUnchanged;
            public int postBindingsRoleObservedCollections;
            public bool postBindingsRoleActualMismatch;
            public bool roleRecordObservationValid;
            public int postBindingsRoleExpectedCollector;
            public int postBindingsRoleLastActualCollector;
            public int postBindingsRoleMaximumPairCount;
            public string postBindingsRoleObservationScope = "Last collection at sampled Editor observations only; not an all-tick fallback-free proof.";
            public bool centralSegmentReadbackCaptured;
            public bool centralSegmentReadbackDetached;
            public string centralSegmentReadbackError;
            public string action;
            public int targetSamples;
            public string reportPath;
            public string terminalStatus;
            public int sampledTicks;
            public int warmupTicks;
            public int minimumObservedActiveAi = int.MaxValue;
            public int minimumObservedBaseRoster = int.MaxValue;
            public int observedSampleWindows;
            public int lastObservedSampleTick;
            public bool workloadValid;
            public bool retainedFailedGcMeasurement;
            public bool zeroGcPassed;
            public bool zeroGcRawGatePassed;
            public string zeroGcEvidenceStatus = "UNCALIBRATED_COUNTER / UNKNOWN";
            public BattleLogicTickGcObserverEditor.Evidence logicGcEvidence;
            public bool harnessValidity;
            public bool teardownRestored;
            public string failure;
            public bool emptyItrGuardApplied;
            public bool emptyItrGuardRestored;
            public bool emptyItrRosterApplied;
            public bool emptyItrRosterRestored;
            public bool emptyItrRosterObservedApplied;
            public bool emptyItrRosterFallbackObserved;
            public int emptyItrRosterMaximumBuildCount;
            public int emptyItrRosterCapacity;
            public long emptyItrRosterPayloadBytes;
            public long emptyItrRosterMaximumVisitedPairs;
            public long emptyItrRosterMaximumSkippedPairs;
            public bool bruteExactCacheApplied;
            public bool bruteExactCacheRestored;
            public bool bruteExactCacheObservedApplied;
            public bool bruteExactCacheFallbackObserved;
            public int bruteExactCacheMaximumBuildCount;
            public long bruteExactCacheMaximumDirections;
            public int bruteExactParticipantCapacity;
            public int bruteExactBodyCapacity;
            public int bruteExactItrCapacity;
            public long bruteExactCacheAppliedBaseline;
            public long bruteExactCacheFallbackBaseline;
            public long bruteExactCacheAppliedDelta;
            public long bruteExactCacheFallbackDelta;
            public bool bruteGeometryFirstApplied;
            public bool bruteGeometryFirstRestored;
            public long bruteGeometryFirstAppliedBaseline;
            public long bruteGeometryFirstAppliedDelta;
            public long bruteGeometryFirstMaximumRejectCount;
            public bool bruteProductionDefaultsObserved;
            public bool bruteProductionDefaultsUnchanged;
            public bool bruteBranchTimingEnabled;
            public bool bruteBranchTimingFlagApplied;
            public bool bruteBranchTimingRestored;
            public int bruteBranchTimingSampleStride;
            public BruteForceSceneQuery.BruteBranchTimingCoverage bruteBranchTimingCoverage;
            public string bruteBranchTimingCoverageScope;
            public bool roleBranchTimingEnabled;
            public bool roleBranchTimingFlagApplied;
            public bool roleBranchTimingUnchanged;
            public bool roleBranchTimingRestored;
            public int roleBranchTimingSampleStride;
            public BruteForceSceneQuery.RoleBranchTimingCoverage roleBranchTimingCoverage;
            public string roleBranchTimingCoverageScope;
            public bool bruteKind5PresenceFlagApplied;
            public bool bruteKind5PresenceEnabled;
            public bool bruteKind5PresenceRestored;
            public long bruteKind5PresenceAppliedBaseline;
            public long bruteKind5PresenceAppliedDelta;
            public long bruteKind5PresenceMaximumSkippedScans;
            public bool bruteEligibilityReuseEnabled;
            public bool bruteEligibilityReuseFlagApplied;
            public bool bruteEligibilityReuseRestored;
            public long bruteEligibilityReuseAppliedBaseline;
            public long bruteEligibilityReuseAppliedDelta;
            public bool bruteCoarseEnvelopeEnabled;
            public bool bruteCoarseEnvelopeFlagApplied;
            public bool bruteCoarseEnvelopeFlagUnchanged;
            public bool bruteCoarseEnvelopeRestored;
            public long bruteCoarseEnvelopeMaximumObservedDirections;
            public long bruteCoarseEnvelopeMaximumObservedRejects;
            public bool bruteEnvelopeBindingPreviousEnvelope;
            public bool bruteEnvelopeBindingPreviousReuse;
            public bool bruteRejectedBindingReuseEnabled;
            public bool bruteRejectedBindingReuseFlagApplied;
            public bool bruteRejectedBindingReuseFlagUnchanged;
            public bool bruteRejectedBindingReuseRestored;
            public bool bruteRejectedBindingReuseObservedApplied;
            public long bruteRejectedBindingMaximumObservedProbes;
            public long bruteRejectedBindingMaximumObservedReuses;
            public bool bruteEnvelopeBindingEligibilityMode;
            public bool bruteEnvelopeBindingPreviousEligibility;
            public bool bruteEligibilityReuseFlagUnchanged;
            public bool bruteEligibilityReuseObservedApplied;
            public bool bruteEnvelopeBranchTimingMode;
            public bool bruteEnvelopeBranchTimingFlagApplied;
            public bool bruteEnvelopeBranchTimingFlagUnchanged;
            public bool bruteEnvelopeBranchTimingRestored;
            public bool bruteBranchTimingFlagUnchanged;
            public bool renderTextureBindingReuseEnabled;
            public bool renderTextureBindingReuseFlagApplied;
            public bool renderTextureBindingReuseFlagUnchanged;
            public bool renderTextureBindingReuseRestored;
            public int renderTextureBindingAcceptedCameraSamples;
            public int renderTextureBindingCameraId;
            public long renderTextureBindingBodyPrepareCount;
            public long renderTextureBindingBodyReuseCount;
            public bool bruteOrdinalPacketEnabled;
            public bool bruteOrdinalPacketFlagApplied;
            public bool bruteOrdinalPacketFlagUnchanged;
            public bool bruteOrdinalPacketRestored;
            public bool bruteOrdinalPacketObservedApplied;
            public bool bruteOrdinalPacketFallbackObserved;
            public bool bruteOrdinalPacketCapacityStable;
            public int bruteOrdinalPacketCapacityBeforePopulation;
            public int bruteOrdinalPacketSteadyCapacity;
            public int bruteOrdinalPacketMaximumObservedCapacity;
            public int bruteOrdinalPacketLastObservedTick;
            public int bruteOrdinalPacketObservedSamples;
            public long bruteOrdinalPacketMaximumObservedRejects;
            public long bruteOrdinalPacketMaximumObservedProofs;
            public bool bruteDepthRejectEnabled;
            public bool bruteDepthRejectFlagApplied;
            public bool bruteDepthRejectFlagUnchanged;
            public bool bruteDepthRejectRestored;
            public bool bruteDepthRejectObservedApplied;
            public bool bruteDepthRejectFallbackObserved;
            public bool bruteDepthRejectCapacityStable;
            public int bruteDepthRejectSteadyParticipantCapacity;
            public int bruteDepthRejectSteadyItrCapacity;
            public int bruteDepthRejectSteadyBodyCapacity;
            public int bruteDepthRejectLastObservedTick;
            public int bruteDepthRejectObservedSamples;
            public long bruteDepthRejectMaximumObservedRejects;
        }

        [Serializable]
        private sealed class CentralSegmentReadbackRow
        {
            public int index, chunk, subMesh, firstCommand, commands, firstQuad, quads;
            public string bindingMode, textureName, textureType, textureAssetPath;
            public int boundTextureIdentity, SourceTextureIdentity, AtlasPageIdentity, TextureArrayIdentity;
            public int slice, pageIndex, materialIdentity, variant, shaderIdentity, shaderPassCount, renderQueue;
            public string materialName, shaderName, shaderKeywords;
            public int breakMaskFromPrevious;
        }

        [Serializable]
        private sealed class CentralSegmentReadbackSnapshot
        {
            public string scope = "One CPU submission readback; not GPU batch, FPS gain or complete zero-GC evidence.";
            public string utc, drawMode, drawPolicyRequested, drawPolicyEffective, drawPolicyReason;
            public int tick, generation, cameraIdentity, sampledLogicTicks, activeAi;
            public bool atlasInputsKnown, cpuLeaseReleased;
            public string atlasRequested, atlasEffective, atlasReason, catalogMode, atlasDiagnostic;
            public int plannedPageCount, sourceCommands, resolvedCommands, unresolvedCommands;
            public int unsupportedCategories, unsupportedStates, chunks, segments;
            public long estimatedAtlasBytes, atlasBudgetBytes;
            public CentralSegmentReadbackRow[] rows;
        }

        [Serializable]
        private sealed class SuiteState
        {
            public string phase;
            public string status = "IN_PROGRESS";
            public string error;
            public string outputRoot = OutputRoot;
            public bool roleCandidate;
            public bool roleFormalCandidate;
            public bool pairSnapshotCandidate;
            public bool emptyItrGuardCandidate;
            public bool emptyItrRosterCandidate;
            public bool bruteExactCacheCandidate;
            public bool bruteGeometryFirstCandidate;
            public bool cpuGcCaptureOnly;
            public bool bruteProductionOnly;
            public bool bruteBranchTimingOnly;
            public bool bruteBranchTimingSampled;
            public bool bruteKind5PresenceCandidate;
            public bool bruteEligibilityReuseCandidate;
            public bool bruteCombinedCacheCandidate;
            public bool logicGcScopeOnly;
            public bool logicGcCallsiteOnly;
            public bool labelPrewarmValidation;
            public bool bruteCoarseEnvelopeCandidate;
            public bool bruteEnvelopeBindingCandidate;
            public bool bruteEnvelopeBindingEligibilityCandidate;
            public bool bruteEnvelopeBranchTimingCandidate;
            public bool renderTextureBindingCandidate;
            public bool bruteOrdinalPacketCandidate;
            public bool bruteDepthRejectCandidate;
            public bool centralSegmentReadbackOnly;
            public bool matchCentralBindingsWindow;
            public bool postBindingsRoleWindow;
            public bool scopedRoleAdmissionWindow;
            public bool scopedProducerAdmissionWindow;
            public bool roleRecordCostDiagnostic;
            public bool roleRecordPrerequisiteDiagnostic;
            public bool rolePrerequisiteQualification;
            public bool producerAbbaDiagnostic;
            public bool inputCostDiagnostic;
            public bool inputFlowDiagnostic;
            public bool inputCpuDiagnostic;
            public bool roleBranchTimingWindow;
            public bool inputBoundaryTimingWindow;
            public bool nativeSampledStateTimingWindow;
            public bool producerSnapshotReuseWindow;
            public bool roleGroupSnapshotSkipWindow;
            public string originalScene;
            public string originalSceneHash;
            public string battleSceneHash;
            public bool originalSuppression;
            public double deadline;
            public int runIndex;
            public int completedRuns;
            public int retainedFailedGcMeasurements;
            public RunState[] runs = new RunState[6];
            public bool orderedShutdown;
            public int remainingObjects;
            public int remainingSlots;
            public int remainingBorrowers;
            public bool battleSceneUnchanged;
            public bool originalSceneRestored;
            public string updatedUtc;
        }

        static BattleOptimizationWindowsAiSuiteEditor()
        {
            string json = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(json))
            {
                state = JsonUtility.FromJson<SuiteState>(json);
                if (state.phase == "WAITING")
                    BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
            }
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayMode;
        }

        [MenuItem("NTSD/Validation/Optimization/Batch26 Windows AI Fixed Suite")]
        private static void Begin()
        {
            BeginSuite(false);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch29 Role Collector 1000 AI Admission")]
        private static void BeginRoleCandidate()
        {
            BeginSuite(true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch30 Role Collector Formal Windows")]
        private static void BeginRoleFormalCandidate()
        {
            BeginSuite(true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch31 Role Pair Snapshot Reuse")]
        private static void BeginPairSnapshotCandidate()
        {
            BeginSuite(true, false, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch32 Brute Empty Itr Guard")]
        private static void BeginEmptyItrGuardCandidate()
        {
            BeginSuite(false, false, false, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch34 Brute Empty Itr Roster")]
        private static void BeginEmptyItrRosterCandidate()
        {
            BeginSuite(false, false, false, true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch36 Brute Exact Cache")]
        private static void BeginBruteExactCacheCandidate()
        {
            BeginSuite(false, false, false, true, true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch37 Brute Geometry First")]
        private static void BeginBruteGeometryFirstCandidate()
        {
            BeginSuite(false, false, false, true, true, true, true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch39 Combat1000 CPU GC Capture")]
        private static void BeginCpuGcCapture()
        {
            BeginSuite(false, cpuGcCaptureOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch40 Brute Production 1000 AI")]
        private static void BeginBruteProduction()
        {
            BeginSuite(false, bruteProductionOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch44 Brute Branch Timing 1000 AI")]
        private static void BeginBruteBranchTiming()
        {
            BeginSuite(false, bruteBranchTimingOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch45 Brute Sampled Timing 1000 AI")]
        private static void BeginBruteSampledTiming()
        {
            BeginSuite(false, bruteBranchTimingOnly: true, bruteBranchTimingSampled: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch38 Brute Kind5 Presence 1000 AI")]
        private static void BeginBruteKind5Presence()
        {
            BeginSuite(false, bruteProductionOnly: true, bruteKind5PresenceCandidate: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch50 Brute Eligibility Reuse 1000 AI OFF ON")]
        private static void BeginBruteEligibilityReuse()
        {
            BeginSuite(false, bruteProductionOnly: true, bruteEligibilityReuseCandidate: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch52 Brute Combined Cache 1000 AI OFF ON")]
        private static void BeginBruteCombinedCache()
        {
            BeginSuite(false, bruteProductionOnly: true, bruteCombinedCacheCandidate: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch54 Calibrated Full Driver GC 1000 AI")]
        private static void BeginLogicGcScope()
        {
            BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch55 Combat Full Driver GC Callsite")]
        private static void BeginLogicGcCallsite()
        {
            BeginSuite(false, cpuGcCaptureOnly: true, bruteProductionOnly: true,
                logicGcScopeOnly: true, logicGcCallsiteOnly: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch57 Label Prewarm Full Driver GC 1000 AI")]
        private static void BeginLabelPrewarm()
        {
            BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true, labelPrewarmValidation: true);
        }

        [MenuItem("NTSD/Validation/Optimization/Batch60 Coarse Envelope Full Driver GC 1000 AI")]
        private static void BeginBruteCoarseEnvelope()
        {
            Require(!startingCoarseEnvelopeWindow && !startingEnvelopeBranchTimingWindow, CoarseEnvelopeOwnerError);
            startingCoarseEnvelopeWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingCoarseEnvelopeWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch63 Envelope Binding Full Driver GC 1000 AI")]
        private static void BeginBruteEnvelopeBinding()
        {
            Require(!startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow, EnvelopeBindingOwnerError);
            startingEnvelopeBindingWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingEnvelopeBindingWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch64 Envelope Binding Eligibility Full Driver GC 1000 AI")]
        private static void BeginBruteEnvelopeBindingEligibility()
        {
            Require(!startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBindingWindow &&
                !startingCoarseEnvelopeWindow && !startingEnvelopeBranchTimingWindow, EnvelopeBindingOwnerError);
            startingEnvelopeBindingEligibilityWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingEnvelopeBindingEligibilityWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch65 Envelope Branch Timing Full Driver GC 1000 AI")]
        private static void BeginBruteEnvelopeBranchTiming()
        {
            Require(!startingEnvelopeBranchTimingWindow && !startingEnvelopeBindingEligibilityWindow &&
                !startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow, EnvelopeBindingOwnerError);
            startingEnvelopeBranchTimingWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingEnvelopeBranchTimingWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch68 Renderer Texture Binding 1000 AI OFF ON")]
        private static void BeginRenderTextureBindingReuse()
        {
            Require(!startingRenderTextureBindingWindow && !renderTextureBindingReuseOwner &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics, RenderTextureBindingOwnerError);
            startingRenderTextureBindingWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingRenderTextureBindingWindow = false;
            }
        }


        [MenuItem("NTSD/Validation/Optimization/Batch72 Ordinal Packet Full Driver GC 1000 AI OFF ON")]
        private static void BeginBruteOrdinalPacket()
        {
            Require(!startingOrdinalPacketWindow && ordinalPacketQuery == null && ordinalPacketRun == null,
                "The ordinal packet window requires exclusive ownership.");
            startingOrdinalPacketWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingOrdinalPacketWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch77 Depth Reject Full Driver GC 1000 AI OFF ON")]
        private static void BeginBruteDepthReject()
        {
            Require(!startingDepthRejectWindow && depthRejectQuery == null && depthRejectRun == null,
                "The depth reject window requires exclusive ownership.");
            startingDepthRejectWindow = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true, logicGcScopeOnly: true);
            }
            finally
            {
                startingDepthRejectWindow = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch75 Central Segment Binding Readback")]
        private static void BeginCentralSegmentReadback()
        {
            Require(!startingCentralSegmentReadback && centralSegmentReadbackRun == null,
                "The central readback requires exclusive Editor observer ownership.");
            startingCentralSegmentReadback = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true);
            }
            finally
            {
                startingCentralSegmentReadback = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch80 Match Central Bindings 1000 AI OFF ON")]
        private static void BeginMatchCentralBindings()
        {
            Require(!startingMatchCentralBindings && !startingCentralSegmentReadback &&
                centralSegmentReadbackRun == null && MatchBindingsEnabled != null,
                "The match binding window requires exclusive cold/readback ownership.");
            startingMatchCentralBindings = true;
            startingCentralSegmentReadback = true;
            try
            {
                BeginSuite(false, bruteProductionOnly: true);
            }
            finally
            {
                startingMatchCentralBindings = false;
                startingCentralSegmentReadback = false;
            }
        }

        [MenuItem("NTSD/Validation/Optimization/H06 Scoped Role Admission")]
        private static void BeginScopedRoleAdmission()
        {
            Require(!startingScopedRoleAdmissionWindow, "Scoped Role admission already owns the cold entry.");
            startingScopedRoleAdmissionWindow = true;
            try
            {
                BeginPostBindingsRole();
            }
            finally
            {
                startingScopedRoleAdmissionWindow = false;
            }
        }

        internal static ProductionEntityStressRequest BuildScopedRoleAdmissionRequest(int index)
        {
            ProductionEntityStressRequest request = BuildPostBindingsRoleRequest(index);
            request.outputPath = ScopedRoleAdmissionOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-" + request.formalCollectorMode + "/report.json";
            return request;
        }

        [MenuItem("NTSD/Validation/Optimization/H07 Producer Snapshot ABBA Diagnostic")]
        private static void BeginProducerAbbaDiagnostic()
        {
            Require(!startingProducerAbbaDiagnostic, "Producer ABBA diagnostic already owns the cold entry.");
            startingProducerAbbaDiagnostic = true;
            try
            {
                BeginProducerSnapshotReuse();
            }
            finally
            {
                startingProducerAbbaDiagnostic = false;
            }
        }

        internal static ProducerSnapshotExperimentCase BuildProducerAbbaCase(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            return new ProducerSnapshotExperimentCase
            {
                sequenceIndex = index,
                action = "combat1000",
                reuseEnabled = index == 1 || index == 2,
                inputBoundaryTimingEnabled = false,
            };
        }

        internal static ProductionEntityStressRequest BuildProducerAbbaRequest(int index)
        {
            ProducerSnapshotExperimentCase experiment = BuildProducerAbbaCase(index);
            ProductionEntityStressRequest request = BuildProducerSnapshotReuseRequest(2);
            request.outputPath = ProducerAbbaOutputRoot + "/" + index.ToString("D2") + "-" +
                experiment.action + (experiment.reuseEnabled ? "-on" : "-off") + "/report.json";
            return request;
        }

        [MenuItem("NTSD/Validation/Optimization/H07 Scoped Producer Snapshot Admission")]
        private static void BeginScopedProducerAdmission()
        {
            Require(!startingScopedProducerAdmissionWindow, "Scoped producer admission already owns the cold entry.");
            startingScopedProducerAdmissionWindow = true;
            try
            {
                BeginProducerSnapshotReuse();
            }
            finally
            {
                startingScopedProducerAdmissionWindow = false;
            }
        }

        internal static ProductionEntityStressRequest BuildScopedProducerAdmissionRequest(int index)
        {
            ProductionEntityStressRequest request = BuildProducerSnapshotReuseRequest(index);
            request.outputPath = ScopedProducerAdmissionOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-off" : "-on") + "/report.json";
            return request;
        }

        [MenuItem("NTSD/Validation/Optimization/Batch81 Post Bindings Brute Role 1000 AI")]
        private static void BeginPostBindingsRole()
        {
            Require(!startingPostBindingsRoleWindow, "The post-bindings role window already owns the cold entry.");
            startingPostBindingsRoleWindow = true;
            try
            {
                BeginSuite(false, logicGcScopeOnly: true);
            }
            finally
            {
                startingPostBindingsRoleWindow = false;
            }
        }

        [MenuItem("NTSD/Optimization/Run Batch83 Role Exact Branch Timing Windows")]
        private static void BeginRoleBranchTiming()
        {
            Require(!startingRoleBranchTimingWindow && roleBranchTimingQuery == null,
                "The role branch timing cold entry already has an owner.");
            startingRoleBranchTimingWindow = true;
            try
            {
                BeginPostBindingsRole();
            }
            finally
            {
                startingRoleBranchTimingWindow = false;
            }
        }

        [MenuItem("NTSD/Optimization/Run Batch93 Role Group Snapshot Windows")]
        private static void BeginRoleGroupSnapshotSkip()
        {
            Require(!startingRoleGroupSnapshotSkipWindow && roleGroupSnapshotSkipWorld == null &&
                roleGroupSnapshotSkipQuery == null && !startingProducerSnapshotReuseWindow &&
                producerSnapshotReuseWorld == null && !startingInputBoundaryTimingWindow &&
                inputBoundaryTimingWorld == null && !startingRoleBranchTimingWindow &&
                roleBranchTimingQuery == null && !startingNativeSampledStateTimingWindow,
                "The role group snapshot window requires an exclusive cold owner.");
            startingRoleGroupSnapshotSkipWindow = true;
            try
            {
                BeginPostBindingsRole();
            }
            finally
            {
                startingRoleGroupSnapshotSkipWindow = false;
            }
        }

        private static void BeginSuite(bool roleCandidate, bool roleFormalCandidate = false,
            bool pairSnapshotCandidate = false, bool emptyItrGuardCandidate = false,
            bool emptyItrRosterCandidate = false, bool bruteExactCacheCandidate = false,
            bool bruteGeometryFirstCandidate = false, bool cpuGcCaptureOnly = false,
            bool bruteProductionOnly = false, bool bruteBranchTimingOnly = false,
            bool bruteBranchTimingSampled = false, bool bruteKind5PresenceCandidate = false,
            bool bruteEligibilityReuseCandidate = false, bool bruteCombinedCacheCandidate = false,
            bool logicGcScopeOnly = false, bool logicGcCallsiteOnly = false,
            bool labelPrewarmValidation = false)
        {
            Require(!startingProducerAbbaDiagnostic || startingProducerSnapshotReuseWindow &&
                startingPostBindingsRoleWindow && !startingScopedProducerAdmissionWindow &&
                !startingScopedRoleAdmissionWindow && !startingRoleGroupSnapshotSkipWindow &&
                !startingInputBoundaryTimingWindow && !startingRoleBranchTimingWindow &&
                !startingNativeSampledStateTimingWindow,
                "Producer ABBA diagnostic requires its exclusive existing producer owner.");
            Require(!startingScopedRoleAdmissionWindow || startingPostBindingsRoleWindow &&
                !startingRoleGroupSnapshotSkipWindow && !startingProducerSnapshotReuseWindow &&
                !startingInputBoundaryTimingWindow && !startingRoleBranchTimingWindow &&
                !startingNativeSampledStateTimingWindow,
                "Scoped Role admission must not share another candidate window.");
            Require(!startingScopedProducerAdmissionWindow || startingProducerSnapshotReuseWindow &&
                startingPostBindingsRoleWindow && !startingScopedRoleAdmissionWindow &&
                !startingRoleGroupSnapshotSkipWindow && !startingInputBoundaryTimingWindow &&
                !startingRoleBranchTimingWindow && !startingNativeSampledStateTimingWindow,
                "Scoped producer admission requires its exclusive existing producer owner.");
            Require(!startingRoleGroupSnapshotSkipWindow || startingPostBindingsRoleWindow &&
                roleGroupSnapshotSkipWorld == null && roleGroupSnapshotSkipQuery == null &&
                !startingProducerSnapshotReuseWindow && producerSnapshotReuseWorld == null &&
                !startingInputBoundaryTimingWindow && inputBoundaryTimingWorld == null &&
                !startingRoleBranchTimingWindow && roleBranchTimingQuery == null &&
                !startingNativeSampledStateTimingWindow,
                "Role group snapshot skip requires exclusive post-bindings ownership.");
            Require(!(startingProducerSnapshotReuseWindow || startingInputBoundaryTimingWindow ||
                startingRoleBranchTimingWindow || startingNativeSampledStateTimingWindow) ||
                !startingRoleGroupSnapshotSkipWindow && roleGroupSnapshotSkipWorld == null && roleGroupSnapshotSkipQuery == null,
                "Other role windows cannot share the group snapshot skip owner.");
            Require(!startingNativeSampledStateTimingWindow || startingInputBoundaryTimingWindow,
                "Native sampled state timing requires the existing input boundary owner.");
            Require(!startingProducerSnapshotReuseWindow || startingPostBindingsRoleWindow &&
                !startingInputBoundaryTimingWindow && inputBoundaryTimingWorld == null &&
                !startingRoleBranchTimingWindow && roleBranchTimingQuery == null && producerSnapshotReuseWorld == null,
                "Producer snapshot reuse requires exclusive post-bindings ownership.");
            Require(!startingInputBoundaryTimingWindow || !startingProducerSnapshotReuseWindow && producerSnapshotReuseWorld == null,
                "Input boundary timing cannot share the producer snapshot reuse owner.");
            Require(!startingRoleBranchTimingWindow || !startingProducerSnapshotReuseWindow && producerSnapshotReuseWorld == null,
                "Role branch timing cannot share the producer snapshot reuse owner.");
            Require(!startingInputBoundaryTimingWindow || startingPostBindingsRoleWindow &&
                !startingRoleBranchTimingWindow && roleBranchTimingQuery == null && inputBoundaryTimingWorld == null,
                "Input boundary timing requires exclusive post-bindings ownership.");
            Require(!startingRoleBranchTimingWindow || !startingInputBoundaryTimingWindow && inputBoundaryTimingWorld == null,
                "Role branch timing cannot share the input boundary timing owner.");
            Require(!startingRoleBranchTimingWindow || startingPostBindingsRoleWindow && roleBranchTimingQuery == null,
                "Role branch timing requires the post-bindings owner and no previous query lease.");
            // Alignment contract: NTSD-OPT-H07-POST-BINDINGS-ROLE-WINDOWS-081; diagnostic Worlds only.
            Require(!startingPostBindingsRoleWindow || logicGcScopeOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate && !emptyItrGuardCandidate &&
                !emptyItrRosterCandidate && !bruteExactCacheCandidate && !bruteGeometryFirstCandidate &&
                !cpuGcCaptureOnly && !bruteProductionOnly && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate &&
                !logicGcCallsiteOnly && !labelPrewarmValidation && !startingDepthRejectWindow &&
                !startingMatchCentralBindings && !startingCentralSegmentReadback && !startingOrdinalPacketWindow &&
                !startingCoarseEnvelopeWindow && !startingEnvelopeBindingWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !startingRenderTextureBindingWindow && productionQuery == null &&
                centralSegmentReadbackRun == null && MatchBindingsEnabled != null &&
                (bool)MatchBindingsEnabled.GetValue(null) &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics,
                "The post-bindings role window requires the calibrated scope, shared bindings and no other mode or owner.");
            // Alignment contract: NTSD-OPT-H07-BRUTE-DEPTH-WINDOWS-077; explicit cold owner, no default promotion.
            Require(!startingDepthRejectWindow || bruteProductionOnly && logicGcScopeOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate && !emptyItrGuardCandidate &&
                !emptyItrRosterCandidate && !bruteExactCacheCandidate && !bruteGeometryFirstCandidate &&
                !cpuGcCaptureOnly && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate &&
                !logicGcCallsiteOnly && !labelPrewarmValidation && !startingCentralSegmentReadback &&
                !startingOrdinalPacketWindow && !startingCoarseEnvelopeWindow && !startingEnvelopeBindingWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !startingRenderTextureBindingWindow && ordinalPacketQuery == null && ordinalPacketRun == null &&
                coarseEnvelopeQuery == null && envelopeBindingQuery == null && kind5PresenceQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && !renderTextureBindingReuseOwner &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics,
                "The depth reject window requires production/calibrated scope without another mode or owner.");
            Require(depthRejectQuery == null && depthRejectRun == null, "A depth reject window already owns the query.");
            Require(!startingCentralSegmentReadback || bruteProductionOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate && !emptyItrGuardCandidate &&
                !emptyItrRosterCandidate && !bruteExactCacheCandidate && !bruteGeometryFirstCandidate &&
                !cpuGcCaptureOnly && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate &&
                !logicGcScopeOnly && !logicGcCallsiteOnly && !labelPrewarmValidation &&
                !startingOrdinalPacketWindow && !startingRenderTextureBindingWindow &&
                !startingEnvelopeBranchTimingWindow && !startingEnvelopeBindingEligibilityWindow &&
                !startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow,
                "The central readback must use unchanged production defaults without another candidate or capture.");
            Require(!startingOrdinalPacketWindow || bruteProductionOnly && logicGcScopeOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate && !emptyItrGuardCandidate &&
                !emptyItrRosterCandidate && !bruteExactCacheCandidate && !bruteGeometryFirstCandidate &&
                !cpuGcCaptureOnly && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate &&
                !logicGcCallsiteOnly && !labelPrewarmValidation && !startingCoarseEnvelopeWindow &&
                !startingEnvelopeBindingWindow && !startingEnvelopeBindingEligibilityWindow &&
                !startingEnvelopeBranchTimingWindow && !startingRenderTextureBindingWindow &&
                coarseEnvelopeQuery == null && envelopeBindingQuery == null && kind5PresenceQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && !renderTextureBindingReuseOwner &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics,
                "The ordinal packet window requires production/calibrated scope without another mode or owner.");
            Require(ordinalPacketQuery == null && ordinalPacketRun == null,
                "An ordinal packet window already owns the query.");
            // Alignment contract: NTSD-OPT-H07-ENVELOPE-PATH-BRANCH-TIMING-065; instrument only this explicit, owned path.
            Require(!startingRenderTextureBindingWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingCoarseEnvelopeWindow && !startingEnvelopeBindingWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !cpuGcCaptureOnly && !bruteBranchTimingOnly &&
                !bruteBranchTimingSampled && !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate &&
                !bruteCombinedCacheCandidate && !logicGcCallsiteOnly && !labelPrewarmValidation &&
                !renderTextureBindingReuseOwner && !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics,
                RenderTextureBindingOwnerError);
            Require(!startingEnvelopeBranchTimingWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBindingWindow &&
                !startingCoarseEnvelopeWindow && !labelPrewarmValidation && !cpuGcCaptureOnly &&
                !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate,
                "The envelope branch timing window requires production defaults, calibrated scope and exclusive ownership.");
            // Alignment contract: NTSD-OPT-H07-ENVELOPE-BINDING-ELIGIBILITY-064; only this explicit owner allows all three flags.
            Require(!startingEnvelopeBindingEligibilityWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingWindow && !startingCoarseEnvelopeWindow && !startingEnvelopeBranchTimingWindow && !labelPrewarmValidation &&
                !cpuGcCaptureOnly && !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate &&
                !pairSnapshotCandidate && !emptyItrGuardCandidate && !emptyItrRosterCandidate &&
                !bruteExactCacheCandidate && !bruteGeometryFirstCandidate && !bruteBranchTimingOnly &&
                !bruteBranchTimingSampled && !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate &&
                !bruteCombinedCacheCandidate,
                "The envelope binding eligibility window requires production defaults, calibrated scope and exclusive ownership.");
            // Alignment contract: NTSD-OPT-H07-ENVELOPE-BINDING-WINDOWS-063; the old envelope owner stays binding-OFF only.
            Require(!startingEnvelopeBindingWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !startingCoarseEnvelopeWindow && !labelPrewarmValidation && !cpuGcCaptureOnly &&
                !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate,
                EnvelopeBindingModeError);
            // Alignment contract: NTSD-OPT-H07-COARSE-ENVELOPE-WINDOWS-060; keep the legacy 17-parameter entry intact.
            Require(!startingCoarseEnvelopeWindow || bruteProductionOnly && logicGcScopeOnly &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow &&
                !labelPrewarmValidation && !cpuGcCaptureOnly && !logicGcCallsiteOnly &&
                !roleCandidate && !roleFormalCandidate && !pairSnapshotCandidate &&
                !emptyItrGuardCandidate && !emptyItrRosterCandidate && !bruteExactCacheCandidate &&
                !bruteGeometryFirstCandidate && !bruteBranchTimingOnly && !bruteBranchTimingSampled &&
                !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate && !bruteCombinedCacheCandidate,
                CoarseEnvelopeModeError);
            Require(state == null && !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Original Editor must be idle and no suite may be active.");
            Require(!labelPrewarmValidation || bruteProductionOnly && logicGcScopeOnly &&
                !cpuGcCaptureOnly && !logicGcCallsiteOnly && !roleCandidate && !roleFormalCandidate &&
                !pairSnapshotCandidate && !emptyItrGuardCandidate && !emptyItrRosterCandidate &&
                !bruteExactCacheCandidate && !bruteGeometryFirstCandidate && !bruteBranchTimingOnly &&
                !bruteBranchTimingSampled && !bruteKind5PresenceCandidate && !bruteEligibilityReuseCandidate &&
                !bruteCombinedCacheCandidate && !startingCoarseEnvelopeWindow && !startingEnvelopeBindingWindow &&
                !startingEnvelopeBindingEligibilityWindow && !startingEnvelopeBranchTimingWindow,
                "The label prewarm window requires unchanged production defaults, calibrated scope and no capture or candidate.");
            Scene scene = SceneManager.GetActiveScene();
            Require(SceneManager.sceneCount == 1 && !scene.isDirty && !string.IsNullOrEmpty(scene.path),
                "A single saved clean scene is required.");
            Require(!File.Exists(PathFor(SharedRequest)) && IsOwnedTerminalOrAbsent(),
                "Shared request must be absent and an existing terminal must belong to this batch; no user file will be deleted or replaced.");
            Require(string.IsNullOrEmpty(SessionState.GetString("NTSD.ProductionEntityStress.RequestJson", string.Empty)) &&
                !SessionState.GetBool("NTSD.ProductionEntityStress.ActiveStatePresent", false) &&
                !SessionState.GetBool("NTSD.ProductionEntityStress.ReloadRecoveryPending", false),
                "Another pressure request or recovery owns the Editor.");
            if (cpuGcCaptureOnly)
                BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
            Require(!logicGcCallsiteOnly || cpuGcCaptureOnly && bruteProductionOnly && logicGcScopeOnly,
                "The logic callsite window requires capture, production and calibrated full Driver scope together.");
            string outputRoot = startingInputCpuDiagnostic ? H07InputCostDiagnostics.CpuOutputRoot :
                startingInputFlowDiagnostic ? H07InputCostDiagnostics.FlowOutputRoot :
                startingInputCostDiagnostic ? H07InputCostDiagnostics.OutputRoot :
                startingRolePrerequisiteQualification ? H06RolePrerequisiteDiagnostics.QualificationRoot :
                startingRoleRecordPrerequisiteDiagnostic ? H06RolePrerequisiteDiagnostics.OutputRoot :
                startingRoleRecordCostDiagnostic ? RoleRecordCostOutputRoot :
                startingProducerAbbaDiagnostic ? ProducerAbbaOutputRoot :
                startingScopedProducerAdmissionWindow ? ScopedProducerAdmissionOutputRoot :
                startingScopedRoleAdmissionWindow ? ScopedRoleAdmissionOutputRoot :
                startingRoleGroupSnapshotSkipWindow ? RoleGroupSnapshotSkipOutputRoot :
                startingNativeSampledStateTimingWindow ? NativeSampledStateOutputRoot :
                startingProducerSnapshotReuseWindow ? ProducerSnapshotReuseRetryOutputRoot :
                startingInputBoundaryTimingWindow ? InputBoundaryTimingOutputRoot :
                startingRoleBranchTimingWindow ? RoleBranchTimingOutputRoot :
                startingPostBindingsRoleWindow ? PostBindingsRoleOutputRoot :
                startingMatchCentralBindings ? MatchCentralBindingsOutputRoot :
                startingCentralSegmentReadback ? CentralSegmentReadbackOutputRoot :
                startingDepthRejectWindow ? BruteDepthRejectOutputRoot :
                startingOrdinalPacketWindow ? BruteOrdinalPacketOutputRoot :
                startingRenderTextureBindingWindow ? RenderTextureBindingOutputRoot :
                startingEnvelopeBranchTimingWindow ? BruteEnvelopeBranchTimingOutputRoot :
                startingEnvelopeBindingEligibilityWindow ? BruteEnvelopeBindingEligibilityOutputRoot :
                startingEnvelopeBindingWindow ? BruteEnvelopeBindingOutputRoot :
                startingCoarseEnvelopeWindow ? BruteCoarseEnvelopeOutputRoot :
                labelPrewarmValidation ? LabelPrewarmOutputRoot :
                logicGcCallsiteOnly ? LogicCallsiteOutputRoot :
                logicGcScopeOnly ? LogicGcScopeOutputRoot :
                bruteCombinedCacheCandidate ? BruteCombinedCacheOutputRoot :
                bruteEligibilityReuseCandidate ? BruteEligibilityReuseOutputRoot :
                bruteKind5PresenceCandidate ? BruteKind5PresenceOutputRoot :
                bruteBranchTimingSampled ? BruteSampledTimingOutputRoot :
                bruteBranchTimingOnly ? BruteBranchTimingOutputRoot :
                bruteProductionOnly ? BruteProductionOutputRoot :
                cpuGcCaptureOnly ? CpuGcCaptureOutputRoot :
                bruteGeometryFirstCandidate ? BruteGeometryFirstOutputRoot :
                bruteExactCacheCandidate ? BruteExactCacheOutputRoot :
                emptyItrRosterCandidate ? EmptyItrRosterOutputRoot :
                emptyItrGuardCandidate ? EmptyItrGuardOutputRoot :
                pairSnapshotCandidate ? PairSnapshotOutputRoot : roleFormalCandidate ? RoleFormalOutputRoot :
                roleCandidate ? RoleCandidateOutputRoot : OutputRoot;
            Require(!Directory.Exists(PathFor(outputRoot)), "The fixed output directory must be new.");
            state = new SuiteState
            {
                phase = "WAITING",
                outputRoot = outputRoot,
                roleCandidate = roleCandidate,
                roleFormalCandidate = roleFormalCandidate,
                pairSnapshotCandidate = pairSnapshotCandidate,
                emptyItrGuardCandidate = emptyItrGuardCandidate,
                emptyItrRosterCandidate = emptyItrRosterCandidate,
                bruteExactCacheCandidate = bruteExactCacheCandidate,
                bruteGeometryFirstCandidate = bruteGeometryFirstCandidate,
                cpuGcCaptureOnly = cpuGcCaptureOnly,
                bruteProductionOnly = bruteProductionOnly,
                bruteBranchTimingOnly = bruteBranchTimingOnly,
                bruteBranchTimingSampled = bruteBranchTimingSampled,
                bruteKind5PresenceCandidate = bruteKind5PresenceCandidate,
                bruteEligibilityReuseCandidate = bruteEligibilityReuseCandidate,
                bruteCombinedCacheCandidate = bruteCombinedCacheCandidate,
                logicGcScopeOnly = logicGcScopeOnly,
                logicGcCallsiteOnly = logicGcCallsiteOnly,
                labelPrewarmValidation = labelPrewarmValidation,
                bruteCoarseEnvelopeCandidate = startingCoarseEnvelopeWindow,
                bruteEnvelopeBindingCandidate = startingEnvelopeBindingWindow,
                bruteEnvelopeBindingEligibilityCandidate = startingEnvelopeBindingEligibilityWindow,
                bruteEnvelopeBranchTimingCandidate = startingEnvelopeBranchTimingWindow,
                renderTextureBindingCandidate = startingRenderTextureBindingWindow,
                bruteOrdinalPacketCandidate = startingOrdinalPacketWindow,
                bruteDepthRejectCandidate = startingDepthRejectWindow,
                centralSegmentReadbackOnly = startingCentralSegmentReadback,
                matchCentralBindingsWindow = startingMatchCentralBindings,
                postBindingsRoleWindow = startingPostBindingsRoleWindow,
                scopedRoleAdmissionWindow = startingScopedRoleAdmissionWindow,
                scopedProducerAdmissionWindow = startingScopedProducerAdmissionWindow,
                roleRecordCostDiagnostic = startingRoleRecordCostDiagnostic,
                roleRecordPrerequisiteDiagnostic = startingRoleRecordPrerequisiteDiagnostic,
                rolePrerequisiteQualification = startingRolePrerequisiteQualification,
                producerAbbaDiagnostic = startingProducerAbbaDiagnostic,
                inputCostDiagnostic = startingInputCostDiagnostic,
                inputFlowDiagnostic = startingInputFlowDiagnostic,
                inputCpuDiagnostic = startingInputCpuDiagnostic,
                roleBranchTimingWindow = startingRoleBranchTimingWindow,
                inputBoundaryTimingWindow = startingInputBoundaryTimingWindow,
                nativeSampledStateTimingWindow = startingNativeSampledStateTimingWindow,
                producerSnapshotReuseWindow = startingProducerSnapshotReuseWindow,
                roleGroupSnapshotSkipWindow = startingRoleGroupSnapshotSkipWindow,
                runs = new RunState[startingInputCpuDiagnostic ? 2 : startingInputFlowDiagnostic || startingRolePrerequisiteQualification || startingRoleRecordPrerequisiteDiagnostic ? 1 : startingRoleRecordCostDiagnostic ? 2 : startingPostBindingsRoleWindow ? 4 : startingMatchCentralBindings ? 2 : startingCentralSegmentReadback ? 1 : startingDepthRejectWindow || startingOrdinalPacketWindow || startingRenderTextureBindingWindow || startingEnvelopeBranchTimingWindow || startingEnvelopeBindingEligibilityWindow || startingEnvelopeBindingWindow || startingCoarseEnvelopeWindow || bruteCombinedCacheCandidate || bruteEligibilityReuseCandidate || bruteBranchTimingOnly ? 4 : cpuGcCaptureOnly ? 1 : bruteProductionOnly || emptyItrGuardCandidate ? 2 : roleFormalCandidate ? 4 : roleCandidate ? 2 : 6],
                originalScene = scene.path,
                originalSceneHash = HashFile(scene.path),
                battleSceneHash = HashFile(BattleScene),
                originalSuppression = BattleTestBootstrap.SuppressEntityCreationForProductionStress,
                deadline = Now() + 600d,
            };
            for (int i = 0; i < state.runs.Length; i++)
            {
                ProductionEntityStressRequest request = BuildCurrentRequest(i);
                state.runs[i] = new RunState
                {
                    action = request.action,
                    targetSamples = request.sampleTicks,
                    reportPath = request.outputPath,
                };
                SaveNew(request.outputPath + ".request.json", JsonUtility.ToJson(request, true));
                if (state.producerAbbaDiagnostic)
                {
                    state.runs[i].producerExperimentCase = BuildProducerAbbaCase(i);
                    SaveNew(request.outputPath + ".case.json", JsonUtility.ToJson(state.runs[i].producerExperimentCase, true));
                }
            }
            Persist();
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
            EditorSceneManager.OpenScene(BattleScene, OpenSceneMode.Single);
            EditorApplication.EnterPlaymode();
        }

        internal static ProductionEntityStressRequest BuildRequest(int index)
        {
            if (index < 0 || index >= 6)
                throw new ArgumentOutOfRangeException(nameof(index));
            bool dispersed = index == 0 || index == 2 || index == 3;
            string action = dispersed ? "dispersed1000" : "combat1000";
            string label = index < 2 ? "smoke" : "formal-" + ((index == 2 || index == 4) ? "01" : "02");
            return new ProductionEntityStressRequest
            {
                action = action,
                inputMode = "ai",
                entityCount = 1000,
                warmupTicks = 120,
                sampleTicks = index < 2 ? 180 : 1800,
                spawnBatchSize = 25,
                maxCatchUpTicksPerFrame = 2,
                maxBacklogTicks = 2,
                catchUpCpuBudgetMs = 0f,
                maxSaturationDrainTicks = 300,
                aiExecutionProfile = "DataOrientedCanonical",
                formalCollectorMode = "brute",
                simulationOnly = false,
                useDedicatedSimulationWorker = false,
                soundPresentationMode = "dispatch",
                enablePhaseTiming = true,
                enablePresentationTiming = true,
                enableDetailPhaseTiming = true,
                enableFrameTiming = true,
                autoStopWhenSampled = true,
                requireZeroGcAfterWarmup = true,
                writeFinalParitySnapshotJson = true,
                seed = 0x4E545344u,
                outputPath = OutputRoot + "/" + index.ToString("D2") + "-" + action + "-" + label + "/report.json",
            };
        }

        internal static ProductionEntityStressRequest BuildRoleCandidateRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.formalCollectorMode = "role";
            request.outputPath = RoleCandidateOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildCpuGcCaptureRequest(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(1);
            request.outputPath = CpuGcCaptureOutputRoot + "/00-combat1000-capture/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildCurrentRequest(int index)
        {
            if (state.inputCpuDiagnostic)
                return H07InputCostDiagnostics.BuildCpuRequest(index);
            if (state.inputFlowDiagnostic)
                return H07InputCostDiagnostics.BuildFlowRequest(index);
            if (state.inputCostDiagnostic)
                return H07InputCostDiagnostics.BuildRequest(index);
            if (state.rolePrerequisiteQualification)
                return H06RolePrerequisiteDiagnostics.BuildQualificationRequest(index);
            if (state.roleRecordPrerequisiteDiagnostic)
                return H06RolePrerequisiteDiagnostics.BuildRequest(index);
            if (state.roleRecordCostDiagnostic)
                return BuildRoleRecordCostRequest(index);
            if (state.producerAbbaDiagnostic)
                return BuildProducerAbbaRequest(index);
            if (state.scopedProducerAdmissionWindow)
                return BuildScopedProducerAdmissionRequest(index);
            if (state.scopedRoleAdmissionWindow)
                return BuildScopedRoleAdmissionRequest(index);
            if (state.roleGroupSnapshotSkipWindow)
                return BuildRoleGroupSnapshotSkipRequest(index);
            if (state.nativeSampledStateTimingWindow)
                return BuildNativeSampledStateTimingRequest(index);
            if (state.producerSnapshotReuseWindow)
                return BuildProducerSnapshotReuseRequest(index);
            if (state.inputBoundaryTimingWindow)
                return BuildInputBoundaryTimingRequest(index);
            if (state.roleBranchTimingWindow)
                return BuildRoleBranchTimingRequest(index);
            if (state.postBindingsRoleWindow)
                return BuildPostBindingsRoleRequest(index);
            if (state.matchCentralBindingsWindow)
                return BuildMatchCentralBindingsRequest(index);
            if (state.centralSegmentReadbackOnly)
                return BuildCentralSegmentReadbackRequest(index);
            if (state.bruteOrdinalPacketCandidate)
                return BuildBruteOrdinalPacketRequest(index);
            if (state.bruteDepthRejectCandidate)
                return BuildBruteDepthRejectRequest(index);
            if (state.renderTextureBindingCandidate)
                return BuildRenderTextureBindingRequest(index);
            if (state.bruteEnvelopeBranchTimingCandidate)
                return BuildBruteEnvelopeBranchTimingRequest(index);
            if (state.bruteEnvelopeBindingEligibilityCandidate)
                return BuildBruteEnvelopeBindingEligibilityRequest(index);
            if (state.bruteEnvelopeBindingCandidate)
                return BuildBruteEnvelopeBindingRequest(index);
            if (state.bruteCoarseEnvelopeCandidate)
                return BuildBruteCoarseEnvelopeRequest(index);
            if (state.labelPrewarmValidation)
                return BuildLabelPrewarmRequest(index);
            if (state.logicGcCallsiteOnly)
                return BuildLogicCallsiteRequest(index);
            if (state.logicGcScopeOnly)
                return BuildLogicGcScopeRequest(index);
            if (state.bruteCombinedCacheCandidate)
                return BuildBruteCombinedCacheRequest(index);
            if (state.bruteEligibilityReuseCandidate)
                return BuildBruteEligibilityReuseRequest(index);
            if (state.bruteKind5PresenceCandidate)
                return BuildBruteKind5PresenceRequest(index);
            if (state.bruteBranchTimingSampled)
                return BuildBruteSampledTimingRequest(index);
            if (state.bruteBranchTimingOnly)
                return BuildBruteBranchTimingRequest(index);
            if (state.bruteProductionOnly)
                return BuildBruteProductionRequest(index);
            if (state.cpuGcCaptureOnly)
                return BuildCpuGcCaptureRequest(index);
            if (state.bruteGeometryFirstCandidate)
                return BuildBruteGeometryFirstRequest(index);
            if (state.bruteExactCacheCandidate)
                return BuildBruteExactCacheRequest(index);
            if (state.emptyItrRosterCandidate)
                return BuildBruteEmptyItrRosterRequest(index);
            if (state.emptyItrGuardCandidate)
                return BuildBruteEmptyItrGuardRequest(index);
            if (state.pairSnapshotCandidate)
                return BuildRolePairSnapshotReuseRequest(index);
            if (state.roleFormalCandidate)
                return BuildRoleFormalRequest(index);
            return state.roleCandidate ? BuildRoleCandidateRequest(index) : BuildRequest(index);
        }

        private static ProductionEntityStressRequest BuildLogicGcScopeRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index);
            request.outputPath = LogicGcScopeOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildLabelPrewarmRequest(int index)
        {
            // Alignment contract: NTSD-OPT-H07-LABEL-PREWARM-WINDOWS-057; only the cold output identity differs.
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index);
            request.outputPath = LabelPrewarmOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildLogicCallsiteRequest(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(1);
            request.outputPath = LogicCallsiteOutputRoot + "/00-combat1000-callsite/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteBranchTimingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteBranchTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-timing-off" : "-timing-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteSampledTimingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteSampledTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-timing-off" : "-timing-stride64") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteKind5PresenceRequest(int index)
        {
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index);
            request.outputPath = BruteKind5PresenceOutputRoot + "/" +
                index.ToString("D2") + "-" + request.action + "-kind5-presence/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEligibilityReuseRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEligibilityReuseOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-eligibility-off" : "-eligibility-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteCombinedCacheRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteCombinedCacheOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-kind5-off" : "-kind5-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteDepthRejectRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteDepthRejectOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-depth-off" : "-depth-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteOrdinalPacketRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteOrdinalPacketOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-packet-off" : "-packet-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEnvelopeBranchTimingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEnvelopeBranchTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-timing-off" : "-timing-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEnvelopeBindingEligibilityRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEnvelopeBindingEligibilityOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-eligibility-off" : "-eligibility-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteEnvelopeBindingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteEnvelopeBindingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildBruteCoarseEnvelopeRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = BruteCoarseEnvelopeOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-envelope-off" : "-envelope-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildRenderTextureBindingRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.outputPath = RenderTextureBindingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteProductionRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = BruteProductionOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildCentralSegmentReadbackRequest(int index)
        {
            if (index != 0)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(1);
            request.outputPath = CentralSegmentReadbackOutputRoot + "/00-combat1000-readback/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildRoleBranchTimingRequest(int index)
        {
            ProductionEntityStressRequest request = BuildPostBindingsRoleRequest(index);
            request.formalCollectorMode = "role";
            request.outputPath = RoleBranchTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-role-timing-off" : "-role-timing-on") + "/report.json";
            return request;
        }

        [MenuItem("NTSD/Optimization/Run Batch88 Producer Snapshot Reuse Windows")]
        private static void BeginProducerSnapshotReuse()
        {
            Require(!startingProducerSnapshotReuseWindow && producerSnapshotReuseWorld == null &&
                !startingInputBoundaryTimingWindow && inputBoundaryTimingWorld == null &&
                !startingRoleBranchTimingWindow && roleBranchTimingQuery == null,
                "Producer snapshot reuse requires an unowned cold entry.");
            startingProducerSnapshotReuseWindow = true;
            try
            {
                BeginPostBindingsRole();
            }
            finally
            {
                startingProducerSnapshotReuseWindow = false;
            }
        }

        private static ProductionEntityStressRequest BuildProducerSnapshotReuseRequest(int index)
        {
            ProductionEntityStressRequest request = BuildPostBindingsRoleRequest(index);
            request.formalCollectorMode = "role";
            request.outputPath = ProducerSnapshotReuseRetryOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-producer-off" : "-producer-on") + "/report.json";
            return request;
        }

        [MenuItem("NTSD/Optimization/Run Batch91 Native Sampled State Timing Windows")]
        private static void BeginNativeSampledStateTiming()
        {
            Require(!startingNativeSampledStateTimingWindow, "The native sampled state cold entry already has an owner.");
            startingNativeSampledStateTimingWindow = true;
            try
            {
                BeginInputBoundaryTiming();
            }
            finally
            {
                startingNativeSampledStateTimingWindow = false;
            }
        }

        private static ProductionEntityStressRequest BuildNativeSampledStateTimingRequest(int index)
        {
            ProductionEntityStressRequest request = BuildInputBoundaryTimingRequest(index);
            request.outputPath = NativeSampledStateOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-native-off" : "-native-on") + "/report.json";
            return request;
        }

        [MenuItem("NTSD/Optimization/Run Batch85 Input Boundary Timing Windows")]
        private static void BeginInputBoundaryTiming()
        {
            Require(!startingInputBoundaryTimingWindow && inputBoundaryTimingWorld == null &&
                !startingRoleBranchTimingWindow && roleBranchTimingQuery == null,
                "The input boundary timing cold entry already has an owner.");
            startingInputBoundaryTimingWindow = true;
            try
            {
                BeginPostBindingsRole();
            }
            finally
            {
                startingInputBoundaryTimingWindow = false;
            }
        }

        private static ProductionEntityStressRequest BuildInputBoundaryTimingRequest(int index)
        {
            ProductionEntityStressRequest request = BuildPostBindingsRoleRequest(index);
            request.formalCollectorMode = "role";
            request.outputPath = InputBoundaryTimingOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-boundary-off" : "-boundary-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildRoleGroupSnapshotSkipRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildPostBindingsRoleRequest(index);
            request.formalCollectorMode = "role";
            request.outputPath = RoleGroupSnapshotSkipOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + (index % 2 == 0 ? "-group-off" : "-group-on") + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildPostBindingsRoleRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(index / 2);
            request.formalCollectorMode = index % 2 == 0 ? "brute" : "role";
            request.outputPath = PostBindingsRoleOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-" + request.formalCollectorMode + "/report.json";
            return request;
        }

        private static ProductionEntityStressRequest BuildMatchCentralBindingsRequest(int index)
        {
            if (index < 0 || index > 1)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildBruteProductionRequest(1);
            request.outputPath = MatchCentralBindingsOutputRoot +
                (index == 0 ? "/00-combat1000-off/report.json" : "/01-combat1000-on/report.json");
            return request;
        }

        internal static ProductionEntityStressRequest BuildRolePairSnapshotReuseRequest(int index)
        {
            ProductionEntityStressRequest request = BuildRoleCandidateRequest(index);
            request.outputPath = PairSnapshotOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteEmptyItrRosterRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = EmptyItrRosterOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteExactCacheRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = BruteExactCacheOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteGeometryFirstRequest(int index)
        {
            ProductionEntityStressRequest request = BuildBruteExactCacheRequest(index);
            request.outputPath = BruteGeometryFirstOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildBruteEmptyItrGuardRequest(int index)
        {
            if (index < 0 || index >= 2)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index);
            request.outputPath = EmptyItrGuardOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-smoke/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildRoleFormalRequest(int index)
        {
            if (index < 0 || index >= 4)
                throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BuildRequest(index + 2);
            request.formalCollectorMode = "role";
            request.outputPath = RoleFormalOutputRoot + "/" + index.ToString("D2") + "-" +
                request.action + "-formal-" + (index % 2 == 0 ? "01" : "02") + "/report.json";
            return request;
        }

        internal static bool MayContinueAfterRetainedGcFailure(
            ProductionEntityStressReport report, int targetSamples, int observedWindows,
            int minimumActiveAi, int minimumBaseRoster)
        {
            return report != null && report.status == "StoppedWithResidue" &&
                !report.harnessValidity && !report.zeroGcGatePassed &&
                report.failure != null && report.failure.StartsWith("ZeroGcGateFailed:", StringComparison.Ordinal) &&
                targetSamples == 1800 && report.sampledLogicTicks == targetSamples &&
                report.warmupTicksCompleted == 120 && observedWindows > 0 &&
                minimumActiveAi == 1000 && minimumBaseRoster == 1000 &&
                report.capacityPressure != null && report.capacityPressure.passed &&
                report.capacityPressure.capacityCriticalDelta == 0 &&
                report.u6ProductionCanonicalMismatchCount == 0 &&
                report.u6ProductionDataOrientedCompatibilityFallbackCount == 0 &&
                report.aiUnifiedSnapshotExecutionPreCommitFallbackCount == 0 &&
                report.aiUnifiedSnapshotExecutionPostCommitHardBreachCount == 0 &&
                report.teardown != null && report.teardown.attempted && report.teardown.restored &&
                report.teardown.cleanupExceptionCount == 0 && report.teardown.worldObjectsAfter == 0 &&
                report.teardown.worldEntitiesAfter == 0 && report.teardown.claimedSlotsAfter == 0 &&
                report.teardown.activeGameObjectsAfter == 0 && report.teardown.objectPoolActiveAfter == 0 &&
                report.teardown.referencePoolActiveAfter == 0;
        }

        private static void Update()
        {
            if (state == null || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            try
            {
                if (state.phase == "EXITING" || state.phase == "SHUTDOWN_FAILED")
                    return;
                if (Now() > state.deadline)
                    throw new TimeoutException("The fixed service/run deadline expired without extending the window.");
                if (!EditorApplication.isPlaying)
                    return;
                if (state.phase == "WAITING")
                {
                    if (!(bool)ServicesReady.Invoke(null, null))
                        return;
                    Require(SimulationTickDriver.Instance.World.ObjectCount == 0 &&
                        SimulationTickDriver.Instance.World.ClaimedRuntimeSlotCountForDiagnostics == 0,
                        "An empty production World is required; existing entities will not be cleared to force admission.");
                    StartCurrentRun();
                    return;
                }
                Require(currentReport != null, "A reload interrupted the owned run; automatic restart is not permitted.");
                RunState run = state.runs[state.runIndex];
                if (ProductionEntityStressRunner.Active != null)
                {
                    if (state.cpuGcCaptureOnly)
                        BattleOptimizationCpuGcCaptureEditor.Observe(currentReport);
                    if (Now() >= nextObservation)
                    {
                        nextObservation = Now() + 1d;
                        ObserveRunningSample(run, currentReport);
                    }
                    if (Now() >= nextProgress)
                    {
                        nextProgress = Now() + 15d;
                        SaveProgress(run);
                    }
                    return;
                }
                run.terminalStatus = currentReport.status;
                if (state.centralSegmentReadbackOnly)
                {
                    RestoreCentralSegmentReadback();
                    Require(run.centralSegmentReadbackCaptured && string.IsNullOrEmpty(run.centralSegmentReadbackError) &&
                        run.centralSegmentReadbackDetached, "The fixed window requires one valid central segment readback.");
                }
                if (state.bruteOrdinalPacketCandidate)
                    CompleteBruteOrdinalPacket(run);
                if (state.bruteDepthRejectCandidate)
                    CompleteBruteDepthReject(run);
                if (state.renderTextureBindingCandidate)
                    CompleteRenderTextureBindingReuse(run);
                if (state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                    state.bruteEnvelopeBranchTimingCandidate)
                    CompleteBruteEnvelopeBinding(run);
                if (state.bruteCoarseEnvelopeCandidate)
                    CompleteBruteCoarseEnvelope(run);
                if (state.bruteCombinedCacheCandidate)
                    CompleteBruteCombinedCache(run);
                if (state.bruteEligibilityReuseCandidate)
                    CompleteBruteEligibilityReuse(run);
                if (state.postBindingsRoleWindow)
                {
                    run.postBindingsRoleUnchanged &= CurrentPostBindingsRoleQueryValid(productionQuery, state.runIndex);
                    if (state.roleGroupSnapshotSkipWindow)
                    {
                        run.roleGroupSnapshotSkipUnchanged &= RoleGroupSnapshotSkipQueryValid(productionQuery, state.runIndex);
                        run.roleGroupSnapshotSkipRestored = RestoreRoleGroupSnapshotSkip();
                    }
                    if (state.producerSnapshotReuseWindow)
                    {
                        run.producerSnapshotReuseUnchanged &= ProducerSnapshotReuseQueryValid(productionQuery, state.runIndex);
                        run.inputBoundaryTimingUnchanged &= run.producerSnapshotReuseUnchanged;
                        ProductionEntityStressAiInputDetailCounters counters = currentReport.aiInputDetailCounters;
                        run.inputBoundaryTimingCountsAvailable = counters != null && counters.available &&
                            counters.phaseCallCounts != null && counters.phaseCallCounts.Length == BattleAiInputDetailDiagnostics.PhaseCount;
                        if (run.inputBoundaryTimingCountsAvailable)
                            Array.Copy(counters.phaseCallCounts, (int)BattleAiInputDetailPhase.ProducerFreeze,
                                run.inputBoundaryTimingPhaseCalls, 0, 4);
                        run.producerSnapshotReuseRestored = RestoreProducerSnapshotReuse();
                        run.inputBoundaryTimingRestored = run.producerSnapshotReuseRestored;
                    }
                    if (state.inputBoundaryTimingWindow)
                    {
                        run.inputBoundaryTimingUnchanged &= InputBoundaryTimingQueryValid(productionQuery, state.runIndex);
                        ProductionEntityStressAiInputDetailCounters counters = currentReport.aiInputDetailCounters;
                        run.inputBoundaryTimingCountsAvailable = counters != null && counters.available &&
                            counters.phaseCallCounts != null && counters.phaseCallCounts.Length == BattleAiInputDetailDiagnostics.PhaseCount;
                        if (run.inputBoundaryTimingCountsAvailable)
                            Array.Copy(counters.phaseCallCounts, (int)BattleAiInputDetailPhase.ProducerFreeze,
                                run.inputBoundaryTimingPhaseCalls, 0, 4);
                        run.inputBoundaryTimingRestored = RestoreInputBoundaryTiming();
                    }
                    if (state.roleBranchTimingWindow)
                    {
                        run.roleBranchTimingUnchanged &= RoleBranchTimingQueryValid(productionQuery, state.runIndex);
                        run.roleBranchTimingCoverage = productionQuery.TotalRoleBranchTimingCoverageForDiagnostics;
                        run.roleBranchTimingCoverageScope = "Query totals for all warmup and sampled collections; not steady-only, sampled direction clocks are not a full-cost estimate.";
                        run.roleBranchTimingRestored = RestoreRoleBranchTiming();
                    }
                    productionQuery = null;
                }
                if ((state.bruteProductionOnly || state.bruteBranchTimingOnly) && productionQuery != null)
                {
                    run.bruteExactCacheAppliedDelta = productionQuery.TotalBruteExactCacheAppliedForDiagnostics -
                        run.bruteExactCacheAppliedBaseline;
                    run.bruteExactCacheFallbackDelta = productionQuery.TotalBruteExactCacheFallbackForDiagnostics -
                        run.bruteExactCacheFallbackBaseline;
                    run.bruteGeometryFirstAppliedDelta = productionQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics -
                        run.bruteGeometryFirstAppliedBaseline;
                    run.bruteProductionDefaultsUnchanged = ProductionDefaultsEnabled(productionQuery);
                    productionQuery = null;
                }
                if (state.bruteBranchTimingOnly)
                {
                    Require(branchTimingQuery != null && run.bruteBranchTimingFlagApplied &&
                        branchTimingQuery.EnableBruteBranchTimingForDiagnostics == run.bruteBranchTimingEnabled &&
                        branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics == run.bruteBranchTimingSampleStride,
                        "Branch timing flag drifted during the fixed window.");
                    run.bruteBranchTimingCoverage = branchTimingQuery.TotalBruteBranchTimingCoverageForDiagnostics;
                    run.bruteBranchTimingCoverageScope = "Diagnostic totals for all warmup and sampled collections; not steady-only and not a full-cost estimate.";
                    run.bruteBranchTimingRestored = RestoreBruteBranchTiming();
                }
                if (state.bruteKind5PresenceCandidate)
                {
                    Require(kind5PresenceQuery != null && run.bruteKind5PresenceFlagApplied &&
                        kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics,
                        "Kind5 presence flag drifted during the fixed window.");
                    run.bruteKind5PresenceAppliedDelta =
                        kind5PresenceQuery.TotalBruteKind5PresenceCollectionAppliedForDiagnostics -
                        run.bruteKind5PresenceAppliedBaseline;
                    run.bruteKind5PresenceRestored = RestoreBruteKind5Presence();
                }
                if (run.emptyItrGuardApplied)
                {
                    if (run.bruteExactCacheApplied && emptyItrGuardQuery != null)
                    {
                        run.bruteExactCacheAppliedDelta =
                            emptyItrGuardQuery.TotalBruteExactCacheAppliedForDiagnostics - run.bruteExactCacheAppliedBaseline;
                        run.bruteExactCacheFallbackDelta =
                            emptyItrGuardQuery.TotalBruteExactCacheFallbackForDiagnostics - run.bruteExactCacheFallbackBaseline;
                    }
                    if (run.bruteGeometryFirstApplied && emptyItrGuardQuery != null)
                        run.bruteGeometryFirstAppliedDelta =
                            emptyItrGuardQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics -
                            run.bruteGeometryFirstAppliedBaseline;
                    bool restored = RestoreEmptyItrGuard();
                    run.emptyItrGuardRestored = restored;
                    if (run.emptyItrRosterApplied)
                        run.emptyItrRosterRestored = restored;
                    if (run.bruteExactCacheApplied)
                        run.bruteExactCacheRestored = restored;
                    if (run.bruteGeometryFirstApplied)
                        run.bruteGeometryFirstRestored = restored;
                }
                run.sampledTicks = currentReport.sampledLogicTicks;
                run.warmupTicks = currentReport.warmupTicksCompleted;
                run.zeroGcRawGatePassed = currentReport.zeroGcGatePassed;
                run.zeroGcPassed = false;
                if (state.logicGcScopeOnly)
                    FinishLogicGcRun(run, false);
                run.harnessValidity = currentReport.harnessValidity;
                run.teardownRestored = currentReport.teardown.restored;
                run.failure = currentReport.failure;
                run.workloadValid = run.sampledTicks == run.targetSamples && run.warmupTicks == 120 &&
                    run.observedSampleWindows > 0 && run.minimumObservedActiveAi == 1000 &&
                    run.minimumObservedBaseRoster == 1000 && run.teardownRestored &&
                    currentReport.harnessValidity && string.IsNullOrEmpty(run.failure) &&
                    currentReport.status != "Failed" &&
                    (!state.emptyItrRosterCandidate ||
                     (run.emptyItrRosterApplied && run.emptyItrRosterRestored &&
                      run.emptyItrRosterObservedApplied && !run.emptyItrRosterFallbackObserved)) &&
                    (!state.bruteExactCacheCandidate ||
                     (run.bruteExactCacheApplied && run.bruteExactCacheRestored &&
                      run.bruteExactCacheObservedApplied && !run.bruteExactCacheFallbackObserved &&
                      run.bruteExactCacheAppliedDelta > 0 && run.bruteExactCacheFallbackDelta == 0)) &&
                    (!state.bruteGeometryFirstCandidate ||
                     (run.bruteGeometryFirstApplied && run.bruteGeometryFirstRestored &&
                      run.bruteGeometryFirstAppliedDelta > 0)) &&
                    (!state.bruteBranchTimingOnly ||
                     (run.bruteBranchTimingFlagApplied && run.bruteBranchTimingRestored)) &&
                    (!state.bruteKind5PresenceCandidate ||
                      (run.bruteKind5PresenceFlagApplied && run.bruteKind5PresenceRestored &&
                       run.bruteKind5PresenceAppliedDelta == run.warmupTicks + run.sampledTicks)) &&
                    (!state.bruteEligibilityReuseCandidate ||
                     (run.bruteEligibilityReuseFlagApplied && run.bruteEligibilityReuseRestored &&
                      run.bruteEligibilityReuseAppliedDelta ==
                          (run.bruteEligibilityReuseEnabled ? run.warmupTicks + run.sampledTicks : 0))) &&
                    (!state.bruteCombinedCacheCandidate ||
                     (run.bruteEligibilityReuseEnabled && run.bruteEligibilityReuseFlagApplied &&
                      run.bruteEligibilityReuseRestored &&
                      run.bruteEligibilityReuseAppliedDelta == run.warmupTicks + run.sampledTicks &&
                      run.bruteKind5PresenceFlagApplied && run.bruteKind5PresenceRestored &&
                      run.bruteKind5PresenceAppliedDelta ==
                          (run.bruteKind5PresenceEnabled ? run.warmupTicks + run.sampledTicks : 0))) &&
                    (!state.renderTextureBindingCandidate ||
                     (run.renderTextureBindingReuseFlagApplied && run.renderTextureBindingReuseFlagUnchanged &&
                      run.renderTextureBindingReuseRestored && run.renderTextureBindingAcceptedCameraSamples > 0 &&
                      run.renderTextureBindingCameraId != 0 && run.renderTextureBindingBodyPrepareCount > 0 &&
                      run.renderTextureBindingBodyReuseCount >= 0 &&
                      (run.renderTextureBindingReuseEnabled || run.renderTextureBindingBodyReuseCount == 0))) &&
                    (!state.bruteCoarseEnvelopeCandidate || CoarseEnvelopeObservationValid(run)) &&
                    (!state.bruteOrdinalPacketCandidate || OrdinalPacketObservationValid(run)) &&
                    (!state.bruteDepthRejectCandidate || DepthRejectObservationValid(run)) &&
                    (!state.postBindingsRoleWindow || PostBindingsRoleObservationValid(run)) &&
                    (!state.inputCostDiagnostic || run.inputCostObservationValid) &&
                    (!state.roleGroupSnapshotSkipWindow || RoleGroupSnapshotSkipObservationValid(run)) &&
                    (!state.roleBranchTimingWindow || RoleBranchTimingObservationValid(run)) &&
                    (!state.inputBoundaryTimingWindow || InputBoundaryTimingObservationValid(run)) &&
                    (!state.producerSnapshotReuseWindow || ProducerSnapshotReuseObservationValid(run)) &&
                    (!state.bruteEnvelopeBindingCandidate || EnvelopeBindingObservationValid(run)) &&
                    (!state.bruteEnvelopeBindingEligibilityCandidate || EnvelopeBindingEligibilityObservationValid(run)) &&
                    (!state.bruteEnvelopeBranchTimingCandidate || EnvelopeBranchTimingObservationValid(run)) &&
                    (!(state.bruteProductionOnly || state.bruteBranchTimingOnly) ||
                     (run.bruteProductionDefaultsObserved && run.bruteProductionDefaultsUnchanged &&
                      run.emptyItrRosterObservedApplied && !run.emptyItrRosterFallbackObserved &&
                      run.bruteExactCacheObservedApplied && !run.bruteExactCacheFallbackObserved &&
                      run.bruteExactCacheAppliedDelta == run.warmupTicks + run.sampledTicks &&
                      run.bruteExactCacheFallbackDelta == 0 &&
                      run.bruteGeometryFirstAppliedDelta == run.warmupTicks + run.sampledTicks));
                if (state.logicGcScopeOnly)
                    run.workloadValid &= run.logicGcEvidence != null && run.logicGcEvidence.coverageValid &&
                        run.logicGcEvidence.before.passed && run.logicGcEvidence.after.passed;
                run.retainedFailedGcMeasurement = state.roleFormalCandidate &&
                    MayContinueAfterRetainedGcFailure(currentReport, run.targetSamples,
                        run.observedSampleWindows, run.minimumObservedActiveAi, run.minimumObservedBaseRoster);
                SaveNew(run.reportPath + ".terminal.txt", File.Exists(PathFor(SharedResult))
                    ? File.ReadAllText(PathFor(SharedResult)) : "MISSING_TERMINAL_RESULT");
                SaveNew(run.reportPath + ".observation.json", JsonUtility.ToJson(run, true));
                state.completedRuns++;
                currentReport = null;
                if (run.retainedFailedGcMeasurement)
                    state.retainedFailedGcMeasurements++;
                if (state.inputCostDiagnostic && (!run.workloadValid || (!state.inputCpuDiagnostic && !run.zeroGcPassed) || !run.inputCostObservationValid))
                    throw new InvalidOperationException("Finite input diagnosis gate failed; no next window is authorized.");
                if (state.roleRecordCostDiagnostic && (!run.workloadValid || !run.zeroGcPassed || !run.roleRecordObservationValid))
                    throw new InvalidOperationException("Finite record diagnostic gate failed; no next window is authorized.");
                if (!run.workloadValid && !run.retainedFailedGcMeasurement)
                    throw new InvalidOperationException("Fixed workload/cleanup admission failed for " + run.action + "; original report retained.");
                state.runIndex++;
                if (state.runIndex == state.runs.Length)
                {
                    state.status = state.retainedFailedGcMeasurements == 0 ?
                        "MEASUREMENTS_COMPLETED" : "MEASUREMENTS_COMPLETED_WITH_FAILURES";
                    ShutdownAndExit();
                }
                else
                {
                    StartCurrentRun();
                }
            }
            catch (Exception exception)
            {
                state.status = "PARTIAL";
                state.error = exception.ToString();
                Require(RestoreRenderTextureBindingReuse(), RenderTextureBindingOwnerError);
                if (ProductionEntityStressRunner.Active != null)
                    StopRun.Invoke(ProductionEntityStressRunner.Active, new object[] { "finite-suite-abort", false });
                ShutdownAndExit();
            }
        }

        private static void StartCurrentRun()
        {
            ProductionEntityStressRequest request = BuildCurrentRequest(state.runIndex);
            string frozen = File.ReadAllText(PathFor(request.outputPath + ".request.json"));
            Require(JsonUtility.ToJson(JsonUtility.FromJson<ProductionEntityStressRequest>(frozen)) ==
                JsonUtility.ToJson(request), "Frozen request drifted.");
            object config = FromRequest.Invoke(null, new object[] { request, PathFor(string.Empty) });
            if ((state.matchCentralBindingsWindow || state.postBindingsRoleWindow) && File.Exists(PathFor(SharedResult)))
            {
                string backup = PathFor(request.outputPath + ".shared-terminal-before.txt");
                Require(!File.Exists(backup), "The shared terminal backup must be new.");
                File.Copy(PathFor(SharedResult), backup, false);
            }
            state.phase = "RUNNING";
            state.deadline = Now() + 7200d;
            Persist();
            bool previousMatchBindings = state.matchCentralBindingsWindow &&
                (bool)MatchBindingsEnabled.GetValue(null);
            ProductionEntityStressRunner runner;
            try
            {
                Require(!state.postBindingsRoleWindow || MatchBindingsEnabled != null &&
                    (bool)MatchBindingsEnabled.GetValue(null), "The post-bindings role window lost its shared bindings.");
                if (state.matchCentralBindingsWindow)
                    MatchBindingsEnabled.SetValue(null, state.runIndex != 0);
                runner = (ProductionEntityStressRunner)StartRun.Invoke(null, new[] { config });
            }
            finally
            {
                if (state.matchCentralBindingsWindow)
                    MatchBindingsEnabled.SetValue(null, previousMatchBindings);
            }
            currentReport = runner.Report;
            if (state.postBindingsRoleWindow)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "The post-bindings role observation must start before any tick.");
                productionQuery = SimulationTickDriver.Instance.World.SceneQuery as BruteForceSceneQuery;
                if (state.nativeSampledStateTimingWindow)
                {
                    Require(!SimulationTickDriver.Instance.World.EnableCharacterInputProducerSnapshotReuseForDiagnostics &&
                        productionQuery != null && !productionQuery.EnableRoleSnapshotFrameReuseForDiagnostics,
                        "Native sampled state attribution requires producer and frame snapshot reuse OFF.");
                }
                if (state.roleGroupSnapshotSkipWindow)
                    ApplyRoleGroupSnapshotSkip(SimulationTickDriver.Instance.World, productionQuery, state.runs[state.runIndex], state.runIndex);
                if (state.producerSnapshotReuseWindow)
                {
                    if (state.producerAbbaDiagnostic)
                        ApplyProducerSnapshotReuseCase(SimulationTickDriver.Instance.World, productionQuery,
                            state.runs[state.runIndex], BuildProducerAbbaCase(state.runIndex));
                    else
                        ApplyProducerSnapshotReuse(SimulationTickDriver.Instance.World, productionQuery, state.runs[state.runIndex], state.runIndex);
                }
                if (state.inputBoundaryTimingWindow)
                    ApplyInputBoundaryTiming(SimulationTickDriver.Instance.World, productionQuery, state.runs[state.runIndex], state.runIndex);
                if (state.roleBranchTimingWindow)
                    ApplyRoleBranchTiming(productionQuery, state.runs[state.runIndex], state.runIndex);
                Require(CurrentPostBindingsRoleQueryValid(productionQuery, state.runIndex),
                    "The post-bindings role window requires its requested collector and unchanged production defaults.");
                RunState run = state.runs[state.runIndex];
                run.postBindingsRoleConfigured = true;
                run.postBindingsRoleUnchanged = true;
                run.postBindingsRoleExpectedCollector = (int)productionQuery.FormalCollectorMode;
            }
            if (state.bruteProductionOnly || state.bruteBranchTimingOnly)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "The ordinary production observation must start before any tick.");
                productionQuery = SimulationTickDriver.Instance.World.SceneQuery as BruteForceSceneQuery;
                Require(productionQuery != null &&
                    productionQuery.FormalCollectorMode == CollisionFormalCollectorMode.ForceBruteForce &&
                    ProductionDefaultsEnabled(productionQuery), "Ordinary Brute production defaults are required.");
                RunState run = state.runs[state.runIndex];
                run.bruteProductionDefaultsObserved = true;
                run.bruteExactCacheAppliedBaseline = productionQuery.TotalBruteExactCacheAppliedForDiagnostics;
                run.bruteExactCacheFallbackBaseline = productionQuery.TotalBruteExactCacheFallbackForDiagnostics;
                run.bruteGeometryFirstAppliedBaseline = productionQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics;
            }
            if (state.centralSegmentReadbackOnly)
            {
                Require(centralSegmentReadbackRun == null && OrdinalPacketQueryDefaultsValid(productionQuery) &&
                    !productionQuery.EnableBruteOrdinalPacketForDiagnostics &&
                    !productionQuery.EnableBruteDepthRejectForDiagnostics,
                    "The readback requires all independent candidates off and an unowned observer.");
                centralSegmentReadbackRun = state.runs[state.runIndex];
                RenderPipelineManager.endCameraRendering += ObserveCentralSegmentReadback;
            }
            if (state.bruteOrdinalPacketCandidate)
            {
                Require(currentReport.logicTicksExecuted == 0, "The ordinal packet flag must be applied before any tick.");
                ApplyBruteOrdinalPacket(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.bruteDepthRejectCandidate)
            {
                Require(currentReport.logicTicksExecuted == 0, "The depth reject flag must be applied before any tick.");
                ApplyBruteDepthReject(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.renderTextureBindingCandidate)
            {
                Require(currentReport.logicTicksExecuted == 0 && productionQuery != null &&
                    !productionQuery.EnableBruteCoarseEnvelopeForDiagnostics &&
                    !productionQuery.EnableBruteKind5PresenceForDiagnostics &&
                    !productionQuery.EnableBruteEligibilityReuseForDiagnostics &&
                    !productionQuery.EnableBruteRejectedBindingReuseForDiagnostics &&
                    !productionQuery.EnableBruteBranchTimingForDiagnostics &&
                    !productionQuery.EnableBruteEnvelopeBranchTimingForDiagnostics &&
                    !productionQuery.EnableBruteCoarseDispatchForDiagnostics, RenderTextureBindingOwnerError);
                ApplyRenderTextureBindingReuse(state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.bruteEnvelopeBranchTimingCandidate)
                ApplyBruteEnvelopeBranchTiming(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteEnvelopeBindingEligibilityCandidate)
                ApplyBruteEnvelopeBindingEligibility(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteCoarseEnvelopeCandidate)
                ApplyBruteCoarseEnvelope(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteEnvelopeBindingCandidate)
                ApplyBruteEnvelopeBinding(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            if (state.bruteBranchTimingOnly)
            {
                branchTimingQuery = productionQuery;
                previousBranchTiming = branchTimingQuery.EnableBruteBranchTimingForDiagnostics;
                previousBranchTimingSampleStride = branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics;
                RunState run = state.runs[state.runIndex];
                run.bruteBranchTimingEnabled = state.runIndex % 2 != 0;
                run.bruteBranchTimingSampleStride = state.bruteBranchTimingSampled ? 64 : 1;
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "Branch timing must be applied before warmup and sampling.");
                branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics = run.bruteBranchTimingSampleStride;
                branchTimingQuery.EnableBruteBranchTimingForDiagnostics = run.bruteBranchTimingEnabled;
                run.bruteBranchTimingFlagApplied =
                    branchTimingQuery.EnableBruteBranchTimingForDiagnostics == run.bruteBranchTimingEnabled &&
                    branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics == run.bruteBranchTimingSampleStride;
                Require(run.bruteBranchTimingFlagApplied, "Branch timing must be configured before any tick.");
            }
            if (state.bruteKind5PresenceCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0 &&
                    productionQuery != null && !productionQuery.EnableBruteBranchTimingForDiagnostics,
                    "Kind5 presence must be applied before ticks without per-branch timing.");
                kind5PresenceQuery = productionQuery;
                previousKind5Presence = kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics;
                RunState run = state.runs[state.runIndex];
                run.bruteKind5PresenceAppliedBaseline =
                    kind5PresenceQuery.TotalBruteKind5PresenceCollectionAppliedForDiagnostics;
                kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics = true;
                run.bruteKind5PresenceFlagApplied = kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics;
            }
            if (state.bruteEligibilityReuseCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "Eligibility reuse must be configured before any warmup or sampled tick.");
                ApplyBruteEligibilityReuse(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.bruteCombinedCacheCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "Combined caches must be configured before any warmup or sampled tick.");
                ApplyBruteCombinedCache(productionQuery, state.runs[state.runIndex], state.runIndex % 2 != 0);
            }
            if (state.cpuGcCaptureOnly)
            {
                if (state.logicGcCallsiteOnly)
                    BattleOptimizationCpuGcCaptureEditor.ArmLogicCallsiteWindow(PathFor(state.outputRoot));
                else
                    BattleOptimizationCpuGcCaptureEditor.Arm(PathFor(state.outputRoot));
            }
            if (state.emptyItrGuardCandidate)
            {
                Require(currentReport.warmupTicksCompleted == 0 && currentReport.sampledLogicTicks == 0,
                    "The candidate must be applied before any warmup/sample tick.");
                emptyItrGuardQuery = SimulationTickDriver.Instance.World.SceneQuery as BruteForceSceneQuery;
                Require(emptyItrGuardQuery != null &&
                    emptyItrGuardQuery.FormalCollectorMode == CollisionFormalCollectorMode.ForceBruteForce,
                    "The same explicit BruteForce collector is required.");
                previousEmptyItrGuard = emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics;
                previousEmptyItrRoster = emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics;
                previousBruteExactCache = emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics;
                previousBruteGeometryFirst = emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics;
                emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics = true;
                emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics = state.emptyItrRosterCandidate;
                emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics = state.bruteExactCacheCandidate;
                emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics = state.bruteGeometryFirstCandidate;
                state.runs[state.runIndex].emptyItrGuardApplied = true;
                if (state.emptyItrRosterCandidate)
                {
                    emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics = true;
                    state.runs[state.runIndex].emptyItrRosterApplied = true;
                    state.runs[state.runIndex].emptyItrRosterCapacity =
                        emptyItrGuardQuery.BruteEmptyItrRosterCapacityForDiagnostics;
                    state.runs[state.runIndex].emptyItrRosterPayloadBytes =
                        4L * (state.runs[state.runIndex].emptyItrRosterCapacity + 1L);
                }
                if (state.bruteExactCacheCandidate)
                {
                    RunState run = state.runs[state.runIndex];
                    run.bruteExactCacheAppliedBaseline = emptyItrGuardQuery.TotalBruteExactCacheAppliedForDiagnostics;
                    run.bruteExactCacheFallbackBaseline = emptyItrGuardQuery.TotalBruteExactCacheFallbackForDiagnostics;
                    emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics = true;
                    run.bruteExactCacheApplied = true;
                    run.bruteExactParticipantCapacity = emptyItrGuardQuery.BruteExactParticipantCapacityForDiagnostics;
                    run.bruteExactBodyCapacity = emptyItrGuardQuery.BruteExactBodyCapacityForDiagnostics;
                    run.bruteExactItrCapacity = emptyItrGuardQuery.BruteExactItrCapacityForDiagnostics;
                }
                if (state.bruteGeometryFirstCandidate)
                {
                    RunState run = state.runs[state.runIndex];
                    run.bruteGeometryFirstAppliedBaseline =
                        emptyItrGuardQuery.TotalBruteGeometryFirstCollectionAppliedForDiagnostics;
                    emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics = true;
                    run.bruteGeometryFirstApplied = true;
                }
            }
            nextObservation = Now();
            nextProgress = Now() + 15d;
            if (state.logicGcScopeOnly)
            {
                Require(currentReport.logicTicksExecuted == 0 && productionQuery != null &&
                    !productionQuery.EnableBruteKind5PresenceForDiagnostics &&
                    (state.bruteEnvelopeBranchTimingCandidate
                        ? EnvelopeBranchTimingScopeValid(productionQuery, state.runs[state.runIndex])
                        : (state.bruteEnvelopeBindingEligibilityCandidate
                        ? EnvelopeBindingEligibilityScopeValid(productionQuery, state.runs[state.runIndex])
                        : (state.bruteEnvelopeBindingCandidate
                            ? EnvelopeBindingScopeValid(productionQuery, state.runs[state.runIndex])
                            : !productionQuery.EnableBruteRejectedBindingReuseForDiagnostics) &&
                            !productionQuery.EnableBruteEligibilityReuseForDiagnostics) &&
                            !productionQuery.EnableBruteBranchTimingForDiagnostics &&
                            !productionQuery.EnableBruteEnvelopeBranchTimingForDiagnostics) &&
                    !productionQuery.EnableBruteCoarseDispatchForDiagnostics &&
                    logicGcObserver == null,
                    "The calibrated scope requires the existing production path before the first tick.");
                logicGcObserver = new BattleLogicTickGcObserverEditor();
                if (state.inputCostDiagnostic)
                    inputCostObserver = new H07InputCostDiagnostics(runner, SimulationTickDriver.Instance, productionQuery, logicGcObserver, state.runIndex, state.inputFlowDiagnostic, state.inputCpuDiagnostic);
                else if (state.roleRecordCostDiagnostic)
                    roleRecordTickObserver = new RoleRecordTickObserver(runner, SimulationTickDriver.Instance, productionQuery, logicGcObserver, state.runIndex == 1);
                else
                    logicGcObserver.Attach(runner);
            }
        }

        private static void ObserveRunningSample(RunState run, ProductionEntityStressReport report)
        {
            if (report.status != "Running" || report.sampledLogicTicks <= run.lastObservedSampleTick)
                return;
            run.lastObservedSampleTick = report.sampledLogicTicks;
            run.minimumObservedActiveAi = Math.Min(run.minimumObservedActiveAi, report.baseAiActiveCount);
            run.minimumObservedBaseRoster = Math.Min(run.minimumObservedBaseRoster, report.baseRosterActiveCount);
            run.observedSampleWindows++;
            if (state.postBindingsRoleWindow)
            {
                run.postBindingsRoleUnchanged &= CurrentPostBindingsRoleQueryValid(productionQuery, state.runIndex);
                if (state.roleGroupSnapshotSkipWindow)
                    ObserveRoleGroupSnapshotSkip(run, state.runIndex);
                if (state.producerSnapshotReuseWindow)
                    ObserveProducerSnapshotReuse(run, state.runIndex);
                if (state.roleBranchTimingWindow)
                    run.roleBranchTimingUnchanged &= RoleBranchTimingQueryValid(productionQuery, state.runIndex);
                if (state.inputBoundaryTimingWindow)
                    run.inputBoundaryTimingUnchanged &= InputBoundaryTimingQueryValid(productionQuery, state.runIndex) &&
                        inputBoundaryTimingWorld.ActiveBattleAiInputDetailDiagnosticsForDiagnostics != null;
                if (productionQuery == null)
                {
                    run.postBindingsRoleActualMismatch = true;
                }
                else
                {
                    run.postBindingsRoleObservedCollections++;
                    run.postBindingsRoleLastActualCollector = (int)productionQuery.LastFormalCollectorModeForDiagnostics;
                    run.postBindingsRoleActualMismatch |= productionQuery.LastFormalCollectionAbortedForDiagnostics ||
                        run.postBindingsRoleLastActualCollector != run.postBindingsRoleExpectedCollector;
                    run.postBindingsRoleMaximumPairCount = Math.Max(run.postBindingsRoleMaximumPairCount,
                        productionQuery.LastFormalPairCountForDiagnostics);
                }
            }
            if (state.bruteOrdinalPacketCandidate)
                ObserveBruteOrdinalPacket(run, report.logicTicksExecuted);
            if (state.bruteDepthRejectCandidate)
                ObserveBruteDepthReject(run, report.logicTicksExecuted);
            if (state.bruteCoarseEnvelopeCandidate)
                ObserveBruteCoarseEnvelope(run);
            if (state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                state.bruteEnvelopeBranchTimingCandidate)
                ObserveBruteEnvelopeBinding(run);
            BruteForceSceneQuery query = run.bruteProductionDefaultsObserved ? productionQuery : emptyItrGuardQuery;
            if (run.bruteKind5PresenceFlagApplied && query != null)
                run.bruteKind5PresenceMaximumSkippedScans = Math.Max(
                    run.bruteKind5PresenceMaximumSkippedScans, query.LastBruteKind5ScanSkippedForDiagnostics);
            if ((run.emptyItrRosterApplied || run.bruteProductionDefaultsObserved) && query != null)
            {
                run.emptyItrRosterObservedApplied |= query.LastBruteEmptyItrRosterAppliedForDiagnostics;
                run.emptyItrRosterFallbackObserved |= query.LastBruteEmptyItrRosterFallbackForDiagnostics;
                run.emptyItrRosterMaximumBuildCount = Math.Max(run.emptyItrRosterMaximumBuildCount,
                    query.LastBruteEmptyItrRosterBuildCountForDiagnostics);
                run.emptyItrRosterMaximumVisitedPairs = Math.Max(run.emptyItrRosterMaximumVisitedPairs,
                    query.LastBruteEmptyItrRosterVisitedPairCountForDiagnostics);
                run.emptyItrRosterMaximumSkippedPairs = Math.Max(run.emptyItrRosterMaximumSkippedPairs,
                    query.LastBruteEmptyItrRosterSkippedPairCountForDiagnostics);
            }
            if ((run.bruteExactCacheApplied || run.bruteProductionDefaultsObserved) && query != null)
            {
                run.bruteExactCacheObservedApplied |= query.LastBruteExactCacheAppliedForDiagnostics;
                run.bruteExactCacheFallbackObserved |= query.LastBruteExactCacheFallbackForDiagnostics;
                run.bruteExactCacheMaximumBuildCount = Math.Max(run.bruteExactCacheMaximumBuildCount,
                    query.LastBruteExactCacheBuildCountForDiagnostics);
                run.bruteExactCacheMaximumDirections = Math.Max(run.bruteExactCacheMaximumDirections,
                    query.LastBruteExactCacheDirectionCountForDiagnostics);
            }
            if ((run.bruteGeometryFirstApplied || run.bruteProductionDefaultsObserved) && query != null)
            {
                run.bruteGeometryFirstMaximumRejectCount = Math.Max(run.bruteGeometryFirstMaximumRejectCount,
                    query.LastBruteGeometryFirstRejectCountForDiagnostics);
                // Preparation is complete before sampled ticks; do not report the pre-warmup zero capacity.
                run.emptyItrRosterCapacity = query.BruteEmptyItrRosterCapacityForDiagnostics;
                run.emptyItrRosterPayloadBytes = 4L * (run.emptyItrRosterCapacity + 1L);
                run.bruteExactParticipantCapacity = query.BruteExactParticipantCapacityForDiagnostics;
                run.bruteExactBodyCapacity = query.BruteExactBodyCapacityForDiagnostics;
                run.bruteExactItrCapacity = query.BruteExactItrCapacityForDiagnostics;
            }
        }

        private static bool CurrentPostBindingsRoleQueryValid(BruteForceSceneQuery query, int index) =>
            state.inputCostDiagnostic || state.roleRecordCostDiagnostic ? PostBindingsRoleQueryValid(query, 3) :
            state.roleGroupSnapshotSkipWindow ? RoleGroupSnapshotSkipQueryValid(query, index) :
            state.producerSnapshotReuseWindow ? ProducerSnapshotReuseQueryValid(query, index) :
            state.inputBoundaryTimingWindow ? InputBoundaryTimingQueryValid(query, index) :
            state.roleBranchTimingWindow ? RoleBranchTimingQueryValid(query, index) : PostBindingsRoleQueryValid(query, index);


        private static bool RoleGroupSnapshotSkipQueryValid(BruteForceSceneQuery query, int index) =>
            index >= 0 && index < 4 && roleGroupSnapshotSkipWorld != null &&
            query == roleGroupSnapshotSkipQuery && roleGroupSnapshotSkipWorld.SceneQuery == query &&
            producerSnapshotReuseWorld == null && inputBoundaryTimingWorld == null && roleBranchTimingQuery == null &&
            PostBindingsRoleQueryValid(query, 1) && !query.EnableRoleBranchTimingForDiagnostics &&
            !query.EnableRoleCapacityRecordGuardForDiagnostics && !query.EnableRoleSnapshotFrameReuseForDiagnostics &&
            !roleGroupSnapshotSkipWorld.EnableCharacterInputProducerSnapshotReuseForDiagnostics &&
            !roleGroupSnapshotSkipWorld.EnableCharacterInputBoundaryTimingForDiagnostics &&
            query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics == (index % 2 != 0);

        private static void ApplyRoleGroupSnapshotSkip(SimulationWorld world, BruteForceSceneQuery query, RunState run, int index)
        {
            // Alignment contract: NTSD-OPT-H07-ROLE-GROUP-SNAPSHOT-WINDOWS-093; cold diagnostic World only.
            Require(index >= 0 && index < 4 && run != null && roleGroupSnapshotSkipWorld == null &&
                roleGroupSnapshotSkipQuery == null && producerSnapshotReuseWorld == null &&
                inputBoundaryTimingWorld == null && roleBranchTimingQuery == null &&
                world != null && world.SceneQuery == query && PostBindingsRoleQueryValid(query, 1) &&
                !query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics && !query.EnableRoleBranchTimingForDiagnostics &&
                !query.EnableRoleCapacityRecordGuardForDiagnostics && !query.EnableRoleSnapshotFrameReuseForDiagnostics &&
                !world.EnableCharacterInputProducerSnapshotReuseForDiagnostics &&
                !world.EnableCharacterInputBoundaryTimingForDiagnostics &&
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics != null,
                "Role group snapshot skip requires an unowned recording World and unchanged Role query.");
            previousRoleGroupSnapshotSkip = query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics;
            roleGroupSnapshotSkipWorld = world;
            roleGroupSnapshotSkipQuery = query;
            run.roleGroupSnapshotSkipEnabled = index % 2 != 0;
            query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics = run.roleGroupSnapshotSkipEnabled;
            run.roleGroupSnapshotSkipFlagApplied = RoleGroupSnapshotSkipQueryValid(query, index);
            run.roleGroupSnapshotSkipUnchanged = run.roleGroupSnapshotSkipFlagApplied;
        }

        private static void ObserveRoleGroupSnapshotSkip(RunState run, int index)
        {
            bool valid = RoleGroupSnapshotSkipQueryValid(productionQuery, index) &&
                roleGroupSnapshotSkipWorld.ActiveBattleAiInputDetailDiagnosticsForDiagnostics != null;
            run.roleGroupSnapshotSkipUnchanged &= valid;
            if (!valid)
                return;
            run.roleGroupSnapshotSkipObservedCollections++;
            run.roleGroupSnapshotSkipMaximumObservedRejects = Math.Max(run.roleGroupSnapshotSkipMaximumObservedRejects,
                productionQuery.LastRoleRejectedGroupSnapshotSkipCountForDiagnostics);
        }

        private static bool RoleGroupSnapshotSkipObservationValid(RunState run) =>
            run != null && run.roleGroupSnapshotSkipFlagApplied && run.roleGroupSnapshotSkipUnchanged &&
            run.roleGroupSnapshotSkipRestored && run.roleGroupSnapshotSkipObservedCollections > 0 &&
            (run.roleGroupSnapshotSkipEnabled ? run.roleGroupSnapshotSkipMaximumObservedRejects > 0 :
                run.roleGroupSnapshotSkipMaximumObservedRejects == 0);

        private static bool RestoreRoleGroupSnapshotSkip()
        {
            if (roleGroupSnapshotSkipQuery == null)
                return roleGroupSnapshotSkipWorld == null;
            roleGroupSnapshotSkipQuery.EnableRoleRejectedGroupSnapshotSkipForDiagnostics = previousRoleGroupSnapshotSkip;
            bool restored = roleGroupSnapshotSkipQuery.EnableRoleRejectedGroupSnapshotSkipForDiagnostics == previousRoleGroupSnapshotSkip;
            if (restored)
            {
                roleGroupSnapshotSkipQuery = null;
                roleGroupSnapshotSkipWorld = null;
            }
            return restored;
        }

        private static bool ProducerSnapshotReuseQueryValid(BruteForceSceneQuery query, int index) =>
            producerSnapshotReuseWorld != null && producerSnapshotReuseWorld.SceneQuery == query &&
            inputBoundaryTimingWorld == null && roleBranchTimingQuery == null &&
            PostBindingsRoleQueryValid(query, 1) && !query.EnableRoleBranchTimingForDiagnostics &&
            !query.EnableRoleCapacityRecordGuardForDiagnostics &&
            producerSnapshotReuseCase != null && producerSnapshotReuseCase.sequenceIndex == index &&
            producerSnapshotReuseWorld.EnableCharacterInputProducerSnapshotReuseForDiagnostics == producerSnapshotReuseCase.reuseEnabled &&
            producerSnapshotReuseWorld.EnableCharacterInputBoundaryTimingForDiagnostics == producerSnapshotReuseCase.inputBoundaryTimingEnabled;

        private static void ApplyProducerSnapshotReuse(SimulationWorld world, BruteForceSceneQuery query, RunState run, int index)
        {
            ApplyProducerSnapshotReuseCase(world, query, run, new ProducerSnapshotExperimentCase
            {
                sequenceIndex = index,
                action = index < 2 ? "dispersed1000" : "combat1000",
                reuseEnabled = index % 2 != 0,
                inputBoundaryTimingEnabled = true,
            });
        }

        private static void ApplyProducerSnapshotReuseCase(SimulationWorld world, BruteForceSceneQuery query,
            RunState run, ProducerSnapshotExperimentCase experiment)
        {
            // Alignment contract: NTSD-H07-PRODUCER-ABBA-DIAG-001; cold case owner, historical wrapper unchanged.
            Require(experiment != null && experiment.sequenceIndex >= 0 && experiment.sequenceIndex < 4 &&
                run != null && producerSnapshotReuseWorld == null && producerSnapshotReuseCase == null &&
                inputBoundaryTimingWorld == null && roleBranchTimingQuery == null &&
                world != null && world.SceneQuery == query && PostBindingsRoleQueryValid(query, 1) &&
                !query.EnableRoleBranchTimingForDiagnostics && !query.EnableRoleCapacityRecordGuardForDiagnostics &&
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics != null,
                "Producer snapshot reuse requires an unowned recording World and its unchanged Role query.");
            previousProducerSnapshotReuse = world.EnableCharacterInputProducerSnapshotReuseForDiagnostics;
            previousProducerSnapshotBoundaryTiming = world.EnableCharacterInputBoundaryTimingForDiagnostics;
            producerSnapshotReuseWorld = world;
            producerSnapshotReuseCase = experiment;
            run.producerExperimentCase = experiment;
            run.producerSnapshotReuseEnabled = experiment.reuseEnabled;
            world.EnableCharacterInputProducerSnapshotReuseForDiagnostics = run.producerSnapshotReuseEnabled;
            world.EnableCharacterInputBoundaryTimingForDiagnostics = experiment.inputBoundaryTimingEnabled;
            run.producerSnapshotReuseFlagApplied = ProducerSnapshotReuseQueryValid(query, experiment.sequenceIndex);
            run.producerSnapshotReuseUnchanged = run.producerSnapshotReuseFlagApplied;
            run.inputBoundaryTimingEnabled = experiment.inputBoundaryTimingEnabled;
            run.inputBoundaryTimingFlagApplied = run.producerSnapshotReuseFlagApplied;
            run.inputBoundaryTimingUnchanged = run.producerSnapshotReuseFlagApplied;
        }

        private static void ObserveProducerSnapshotReuse(RunState run, int index)
        {
            bool valid = ProducerSnapshotReuseQueryValid(productionQuery, index) &&
                producerSnapshotReuseWorld.ActiveBattleAiInputDetailDiagnosticsForDiagnostics != null;
            run.producerSnapshotReuseUnchanged &= valid;
            run.inputBoundaryTimingUnchanged &= valid;
            if (!valid)
                return;
            run.producerSnapshotReuseObservedPasses++;
            run.producerSnapshotReuseMaximumObservedSkips = Math.Max(run.producerSnapshotReuseMaximumObservedSkips,
                producerSnapshotReuseWorld.LastCharacterInputProducerSnapshotReuseSkipCountForDiagnostics);
            run.producerSnapshotReuseMaximumObservedRefreshes = Math.Max(run.producerSnapshotReuseMaximumObservedRefreshes,
                producerSnapshotReuseWorld.LastCharacterInputProducerSnapshotRefreshCountForDiagnostics);
        }

        private static bool ProducerSnapshotReuseObservationValid(RunState run) =>
            run != null && run.producerSnapshotReuseFlagApplied && run.producerSnapshotReuseUnchanged &&
            run.producerSnapshotReuseRestored && run.producerSnapshotReuseObservedPasses > 0 &&
            (run.producerSnapshotReuseEnabled ? run.producerSnapshotReuseMaximumObservedSkips > 0 :
                run.producerSnapshotReuseMaximumObservedSkips == 0) && InputBoundaryTimingObservationValid(run);

        private static bool RestoreProducerSnapshotReuse()
        {
            if (producerSnapshotReuseWorld == null)
                return true;
            producerSnapshotReuseWorld.EnableCharacterInputProducerSnapshotReuseForDiagnostics = previousProducerSnapshotReuse;
            producerSnapshotReuseWorld.EnableCharacterInputBoundaryTimingForDiagnostics = previousProducerSnapshotBoundaryTiming;
            bool restored = producerSnapshotReuseWorld.EnableCharacterInputProducerSnapshotReuseForDiagnostics == previousProducerSnapshotReuse &&
                producerSnapshotReuseWorld.EnableCharacterInputBoundaryTimingForDiagnostics == previousProducerSnapshotBoundaryTiming;
            producerSnapshotReuseWorld = null;
            producerSnapshotReuseCase = null;
            return restored;
        }

        private static bool InputBoundaryTimingQueryValid(BruteForceSceneQuery query, int index) =>
            inputBoundaryTimingWorld != null && inputBoundaryTimingWorld.SceneQuery == query &&
            PostBindingsRoleQueryValid(query, 1) && !query.EnableRoleBranchTimingForDiagnostics &&
            inputBoundaryTimingWorld.EnableCharacterInputBoundaryTimingForDiagnostics == (index % 2 != 0);

        private static bool InputBoundaryTimingObservationValid(RunState run)
        {
            if (run == null || !run.inputBoundaryTimingFlagApplied || !run.inputBoundaryTimingUnchanged ||
                !run.inputBoundaryTimingRestored || !run.inputBoundaryTimingCountsAvailable ||
                run.inputBoundaryTimingPhaseCalls == null || run.inputBoundaryTimingPhaseCalls.Length != 4)
                return false;
            for (int i = 0; i < run.inputBoundaryTimingPhaseCalls.Length; i++)
            {
                if (run.inputBoundaryTimingEnabled ? run.inputBoundaryTimingPhaseCalls[i] <= 0 : run.inputBoundaryTimingPhaseCalls[i] != 0)
                    return false;
            }
            return true;
        }

        private static void ApplyInputBoundaryTiming(SimulationWorld world, BruteForceSceneQuery query, RunState run, int index)
        {
            // Alignment contract: NTSD-OPT-H07-INPUT-BOUNDARY-TIMING-085; cold diagnostic owner only.
            Require(index >= 0 && index < 4 && run != null && inputBoundaryTimingWorld == null && producerSnapshotReuseWorld == null &&
                world != null && world.SceneQuery == query && PostBindingsRoleQueryValid(query, 1) &&
                !query.EnableRoleBranchTimingForDiagnostics && world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics != null,
                "Input boundary timing requires an unowned recording World and its unchanged Role query.");
            previousInputBoundaryTiming = world.EnableCharacterInputBoundaryTimingForDiagnostics;
            inputBoundaryTimingWorld = world;
            run.inputBoundaryTimingEnabled = index % 2 != 0;
            world.EnableCharacterInputBoundaryTimingForDiagnostics = run.inputBoundaryTimingEnabled;
            run.inputBoundaryTimingFlagApplied = InputBoundaryTimingQueryValid(query, index);
            run.inputBoundaryTimingUnchanged = run.inputBoundaryTimingFlagApplied;
        }

        private static bool RestoreInputBoundaryTiming()
        {
            if (inputBoundaryTimingWorld == null)
                return true;
            inputBoundaryTimingWorld.EnableCharacterInputBoundaryTimingForDiagnostics = previousInputBoundaryTiming;
            bool restored = inputBoundaryTimingWorld.EnableCharacterInputBoundaryTimingForDiagnostics == previousInputBoundaryTiming;
            inputBoundaryTimingWorld = null;
            return restored;
        }

        private static bool RoleBranchTimingQueryValid(BruteForceSceneQuery query, int index) =>
            query == roleBranchTimingQuery && PostBindingsRoleQueryValid(query, 1) &&
            query.EnableRoleBranchTimingForDiagnostics == (index % 2 != 0) &&
            query.RoleBranchTimingSampleStrideForDiagnostics == 64;

        private static bool RoleBranchTimingObservationValid(RunState run) =>
            run != null && run.roleBranchTimingFlagApplied && run.roleBranchTimingUnchanged &&
            run.roleBranchTimingRestored && (run.roleBranchTimingEnabled
                ? run.roleBranchTimingCoverage.eligibleDirections > 0 && run.roleBranchTimingCoverage.timedDirections > 0
                : run.roleBranchTimingCoverage.eligibleDirections == 0 && run.roleBranchTimingCoverage.timedDirections == 0);

        private static void ApplyRoleBranchTiming(BruteForceSceneQuery query, RunState run, int index)
        {
            // Alignment contract: NTSD-OPT-H07-ROLE-EXACT-BRANCH-TIMING-083; cold diagnostic owner only.
            Require(index >= 0 && index < 4 && run != null && roleBranchTimingQuery == null && producerSnapshotReuseWorld == null &&
                PostBindingsRoleQueryValid(query, 1), "Role branch timing requires an unowned production Role query.");
            previousRoleBranchTiming = query.EnableRoleBranchTimingForDiagnostics;
            previousRoleBranchTimingStride = query.RoleBranchTimingSampleStrideForDiagnostics;
            roleBranchTimingQuery = query;
            run.roleBranchTimingEnabled = index % 2 != 0;
            run.roleBranchTimingSampleStride = 64;
            query.RoleBranchTimingSampleStrideForDiagnostics = 64;
            query.EnableRoleBranchTimingForDiagnostics = run.roleBranchTimingEnabled;
            run.roleBranchTimingFlagApplied = RoleBranchTimingQueryValid(query, index);
            run.roleBranchTimingUnchanged = run.roleBranchTimingFlagApplied;
        }

        private static bool RestoreRoleBranchTiming()
        {
            if (roleBranchTimingQuery == null)
                return true;
            roleBranchTimingQuery.EnableRoleBranchTimingForDiagnostics = previousRoleBranchTiming;
            roleBranchTimingQuery.RoleBranchTimingSampleStrideForDiagnostics = previousRoleBranchTimingStride;
            bool restored = roleBranchTimingQuery.EnableRoleBranchTimingForDiagnostics == previousRoleBranchTiming &&
                roleBranchTimingQuery.RoleBranchTimingSampleStrideForDiagnostics == previousRoleBranchTimingStride;
            roleBranchTimingQuery = null;
            return restored;
        }

        private static bool PostBindingsRoleObservationValid(RunState run) =>
            run != null && run.postBindingsRoleConfigured && run.postBindingsRoleUnchanged &&
            run.postBindingsRoleObservedCollections > 0 && !run.postBindingsRoleActualMismatch;

        private static bool PostBindingsRoleQueryValid(BruteForceSceneQuery query, int index) =>
            query != null && query.FormalCollectorMode == (index % 2 == 0
                ? CollisionFormalCollectorMode.ForceBruteForce : CollisionFormalCollectorMode.ForceRoleAware) &&
            ProductionDefaultsEnabled(query) && !query.EnableBruteKind5PresenceForDiagnostics &&
            !query.EnableBruteEligibilityReuseForDiagnostics && !query.EnableBruteCoarseEnvelopeForDiagnostics &&
            !query.EnableBruteRejectedBindingReuseForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            !query.EnableBruteCoarseProofReuseForDiagnostics && !query.EnableBruteBranchTimingForDiagnostics &&
            !query.EnableBruteEnvelopeBranchTimingForDiagnostics && !query.EnableBruteOrdinalPacketForDiagnostics &&
            !query.EnableBruteDepthRejectForDiagnostics && !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics &&
            MatchBindingsEnabled != null && (bool)MatchBindingsEnabled.GetValue(null);

        private static bool ProductionDefaultsEnabled(BruteForceSceneQuery query) =>
            query.EnableEmptyItrPairGuardForDiagnostics && query.EnableBruteEmptyItrRosterForDiagnostics &&
            query.EnableBruteExactCacheForDiagnostics && query.EnableBruteGeometryFirstForDiagnostics;

        private static void SaveProgress(RunState run)
        {
            run.sampledTicks = currentReport.sampledLogicTicks;
            run.warmupTicks = currentReport.warmupTicksCompleted;
            SaveNew(state.outputRoot + "/progress-" + (progressOrdinal++).ToString("D4") + ".json", JsonUtility.ToJson(run, true));
        }

        private static void ShutdownAndExit()
        {
            Require(RestoreRoleGroupSnapshotSkip(), "Role group snapshot skip must be restored before owner shutdown.");
            Require(RestoreProducerSnapshotReuse(), "Producer snapshot reuse must be restored before owner shutdown.");
            Require(RestoreInputBoundaryTiming(), "Input boundary timing must be restored before owner shutdown.");
            Require(RestoreRoleBranchTiming(), "Role branch timing must be restored before owner shutdown.");
            RestoreCentralSegmentReadback();
            Require(RestoreBruteOrdinalPacket(), "The owned ordinal packet flag must be restored before shutdown.");
            Require(RestoreBruteDepthReject(), "The owned depth reject flag must be restored before shutdown.");
            Require(RestoreRenderTextureBindingReuse(), RenderTextureBindingOwnerError);
            Require(RestoreBruteEnvelopeBindingForExit(), EnvelopeBindingOwnerError);
            if (logicGcObserver != null)
                FinishLogicGcRun(state.runs[state.runIndex], true);
            Require(RestoreBruteCoarseEnvelope(), CoarseEnvelopeOwnerError);
            Require(RestoreBruteEligibilityReuse(), "Eligibility reuse must be restored before owner shutdown.");
            Require(RestoreBruteBranchTiming(), "Branch timing must be restored before owner shutdown.");
            Require(RestoreBruteKind5Presence(), "Kind5 presence must be restored before owner shutdown.");
            if (roleRecordTickObserver != null) FinishRoleRecordObservation(state.runs[state.runIndex], true);
            roleRecordBaseline = null;
            if (inputCostObserver != null) FinishInputCostObservation(state.runs[state.runIndex], true);
            H07InputCostDiagnostics.ResetBaseline();
            productionQuery = null;
            if (state.cpuGcCaptureOnly)
                BattleOptimizationCpuGcCaptureEditor.FinishAndRestore("owner-shutdown");
            Require(RestoreEmptyItrGuard(), "The diagnostic guard must be restored before owner shutdown.");
            SimulationTickDriver driver = SimulationTickDriver.Instance;
            if (driver?.World != null)
            {
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap))
                            continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }
                state.remainingObjects = shutdown.RemainingWorldObjects;
                state.remainingSlots = shutdown.RemainingRuntimeSlots;
                state.remainingBorrowers = shutdown.RemainingPoolBorrowers;
                state.orderedShutdown = shutdown.IsComplete && driver.World == null;
                if (!state.orderedShutdown)
                {
                    state.phase = "SHUTDOWN_FAILED";
                    state.status = "PARTIAL";
                    state.error += " Ordered shutdown failed: " + shutdown.FailureReason;
                    Persist();
                    SaveNew(state.outputRoot + "/shutdown-failure.json", JsonUtility.ToJson(state, true));
                    return;
                }
            }
            state.phase = "EXITING";
            Persist();
            EditorApplication.ExitPlaymode();
        }

        private static void OnPlayMode(PlayModeStateChange change)
        {
            if (state == null)
                return;
            if (state.roleGroupSnapshotSkipWindow && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreRoleGroupSnapshotSkip(), "Role group snapshot skip must also be restored on external Play exit.");
            if (state.producerSnapshotReuseWindow && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreProducerSnapshotReuse(), "Producer snapshot reuse must also be restored on external Play exit.");
            if (state.inputBoundaryTimingWindow && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreInputBoundaryTiming(), "Input boundary timing must also be restored on external Play exit.");
            if (state.roleBranchTimingWindow && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreRoleBranchTiming(), "Role branch timing must also be restored on external Play exit.");
            if (change == PlayModeStateChange.ExitingPlayMode)
                RestoreCentralSegmentReadback();
            if (state.bruteOrdinalPacketCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteOrdinalPacket(), "The owned ordinal packet flag must be restored on Play exit.");
            if (state.bruteDepthRejectCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteDepthReject(), "The owned depth reject flag must be restored on Play exit.");
            if (state.renderTextureBindingCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreRenderTextureBindingReuse(), RenderTextureBindingOwnerError);
            if ((state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                state.bruteEnvelopeBranchTimingCandidate) &&
                change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteEnvelopeBindingForExit(), EnvelopeBindingOwnerError);
            if (state.logicGcScopeOnly && change == PlayModeStateChange.ExitingPlayMode && logicGcObserver != null)
                FinishLogicGcRun(state.runs[state.runIndex], true);
            if (state.bruteCoarseEnvelopeCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteCoarseEnvelope(), CoarseEnvelopeOwnerError);
            if (state.bruteCombinedCacheCandidate && change == PlayModeStateChange.ExitingPlayMode)
            {
                Require(RestoreBruteKind5Presence(), "Combined kind5 cache must be restored on external Play exit.");
                Require(RestoreBruteEligibilityReuse(), "Combined eligibility cache must be restored on external Play exit.");
            }
            if (state.bruteEligibilityReuseCandidate && change == PlayModeStateChange.ExitingPlayMode)
                Require(RestoreBruteEligibilityReuse(), "Eligibility reuse must also be restored on external Play exit.");
            if (state.cpuGcCaptureOnly && change == PlayModeStateChange.ExitingPlayMode)
                BattleOptimizationCpuGcCaptureEditor.FinishAndRestore("play-exit");
            if (change == PlayModeStateChange.ExitingPlayMode && state.phase != "EXITING")
            {
                state.status = "PARTIAL";
                state.error = "Play Mode was stopped externally; no automatic restart.";
                state.phase = "EXITING";
                Persist();
            }
            if (change != PlayModeStateChange.EnteredEditMode || state.phase != "EXITING")
                return;
            Scene scene = SceneManager.GetActiveScene();
            state.battleSceneUnchanged = scene.path == BattleScene && !scene.isDirty &&
                HashFile(BattleScene) == state.battleSceneHash;
            if (state.battleSceneUnchanged && HashFile(state.originalScene) == state.originalSceneHash)
            {
                EditorSceneManager.OpenScene(state.originalScene, OpenSceneMode.Single);
                state.originalSceneRestored = !SceneManager.GetActiveScene().isDirty &&
                    SceneManager.GetActiveScene().path == state.originalScene;
            }
            if (!state.battleSceneUnchanged || !state.originalSceneRestored)
            {
                state.status = "PARTIAL";
                state.error += " Scene identity/dirty guard failed; no forced save or restore.";
            }
            BattleTestBootstrap.SuppressEntityCreationForProductionStress = state.originalSuppression;
            state.phase = "DONE";
            state.updatedUtc = DateTime.UtcNow.ToString("O");
            SaveNew(state.outputRoot + "/suite-result.json", JsonUtility.ToJson(state, true));
            SessionState.EraseString(SessionKey);
            state = null;
            currentReport = null;
            productionQuery = null;
        }

        private static void Persist()
        {
            state.updatedUtc = DateTime.UtcNow.ToString("O");
            SessionState.SetString(SessionKey, JsonUtility.ToJson(state));
        }

        private static double Now() => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        private static void FinishLogicGcRun(RunState run, bool aborted)
        {
            Require(logicGcObserver != null, "The full Driver GC owner was lost; no zero claim is permitted.");
            BattleLogicTickGcObserverEditor observer = logicGcObserver;
            logicGcObserver = null;
            try
            {
                run.logicGcEvidence = observer.Finish(currentReport?.logicTicksExecuted ?? 0, run.targetSamples);
                if (state.logicGcCallsiteOnly || state.roleRecordPrerequisiteDiagnostic || state.inputFlowDiagnostic || state.inputCpuDiagnostic)
                {
                    run.logicGcEvidence.zeroAllocationPassed = false;
                    run.logicGcEvidence.provenSteadyAllocatedBytes = -1;
                    run.logicGcEvidence.status = (state.inputCpuDiagnostic ? "CPU_DIAGNOSTIC_NOT_CERTIFICATE / " : "INSTRUMENTED_CALLSITE_ONLY / ") + run.logicGcEvidence.status;
                }
                if (aborted)
                {
                    run.logicGcEvidence.zeroAllocationPassed = false;
                    run.logicGcEvidence.provenSteadyAllocatedBytes = -1;
                    run.logicGcEvidence.status = "ABORTED / INCOMPLETE";
                }
                run.zeroGcPassed = run.logicGcEvidence.zeroAllocationPassed;
                run.zeroGcEvidenceStatus = run.logicGcEvidence.status;
                SaveNew(run.reportPath + (aborted ? ".logic-gc.aborted.json" : ".logic-gc.json"),
                    JsonUtility.ToJson(run.logicGcEvidence, true));
            }
            finally
            {
                observer.Dispose();
                if (state.roleRecordCostDiagnostic) FinishRoleRecordObservation(run, aborted);
                if (state.inputCostDiagnostic) FinishInputCostObservation(run, aborted);
            }
        }
        private static void FinishInputCostObservation(RunState run, bool aborted)
        {
            H07InputCostDiagnostics observer = inputCostObserver;
            inputCostObserver = null;
            if (observer == null) return;
            try
            {
                run.inputCostObservationValid = observer.Finish(aborted);
                SaveNew(run.reportPath + (aborted ? ".input-cost.aborted.json" : ".input-cost.json"),
                    JsonUtility.ToJson(observer.evidence, true));
            }
            finally { observer.Dispose(); }
        }

        private static string PathFor(string path) => Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));

        private static bool IsOwnedTerminalOrAbsent()
        {
            if (!File.Exists(PathFor(SharedResult)))
                return true;
            string[] lines = File.ReadAllLines(PathFor(SharedResult));
            string ownerRoot = PathFor("artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH24-WINDOWS-AI-20261007") + Path.DirectorySeparatorChar;
            string currentOwnerRoot = PathFor("artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH26-H07-CONTINUE-20261007") + Path.DirectorySeparatorChar;
            string candidateOwnerRoot = PathFor(RoleCandidateOutputRoot) + Path.DirectorySeparatorChar;
            string formalOwnerRoot = PathFor(RoleFormalOutputRoot) + Path.DirectorySeparatorChar;
            string pairSnapshotOwnerRoot = PathFor(PairSnapshotOutputRoot) + Path.DirectorySeparatorChar;
            string emptyItrOwnerRoot = PathFor(EmptyItrGuardOutputRoot) + Path.DirectorySeparatorChar;
            string emptyItrRosterOwnerRoot = PathFor(EmptyItrRosterOutputRoot) + Path.DirectorySeparatorChar;
            string bruteExactCacheOwnerRoot = PathFor(BruteExactCacheOutputRoot) + Path.DirectorySeparatorChar;
            string bruteGeometryFirstOwnerRoot = PathFor(BruteGeometryFirstOutputRoot) + Path.DirectorySeparatorChar;
            string cpuGcCaptureOwnerRoot = PathFor(CpuGcCaptureOutputRoot) + Path.DirectorySeparatorChar;
            string bruteProductionOwnerRoot = PathFor(BruteProductionOutputRoot) + Path.DirectorySeparatorChar;
            string branchTimingOwnerRoot = PathFor(BruteBranchTimingOutputRoot) + Path.DirectorySeparatorChar;
            string sampledTimingOwnerRoot = PathFor(BruteSampledTimingOutputRoot) + Path.DirectorySeparatorChar;
            string kind5PresenceOwnerRoot = PathFor(BruteKind5PresenceOutputRoot) + Path.DirectorySeparatorChar;
            string eligibilityReuseOwnerRoot = PathFor(BruteEligibilityReuseOutputRoot) + Path.DirectorySeparatorChar;
            string combinedCacheOwnerRoot = PathFor(BruteCombinedCacheOutputRoot) + Path.DirectorySeparatorChar;
            string logicGcOwnerRoot = PathFor(LogicGcScopeOutputRoot) + Path.DirectorySeparatorChar;
            string logicCallsiteOwnerRoot = PathFor(LogicCallsiteOutputRoot) + Path.DirectorySeparatorChar;
            string labelPrewarmOwnerRoot = PathFor(LabelPrewarmOutputRoot) + Path.DirectorySeparatorChar;
            string coarseEnvelopeOwnerRoot = PathFor(BruteCoarseEnvelopeOutputRoot) + Path.DirectorySeparatorChar;
            string envelopeBindingOwnerRoot = PathFor(BruteEnvelopeBindingOutputRoot) + Path.DirectorySeparatorChar;
            string envelopeBindingEligibilityOwnerRoot = PathFor(BruteEnvelopeBindingEligibilityOutputRoot) + Path.DirectorySeparatorChar;
            string envelopeBranchTimingOwnerRoot = PathFor(BruteEnvelopeBranchTimingOutputRoot) + Path.DirectorySeparatorChar;
            string renderTextureBindingOwnerRoot = PathFor(RenderTextureBindingOutputRoot) + Path.DirectorySeparatorChar;
            string ordinalPacketOwnerRoot = PathFor(BruteOrdinalPacketOutputRoot) + Path.DirectorySeparatorChar;
            string depthRejectOwnerRoot = PathFor(BruteDepthRejectOutputRoot) + Path.DirectorySeparatorChar;
            string centralReadbackOwnerRoot = PathFor("artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008") + Path.DirectorySeparatorChar;
            return lines.Length >= 2 && (lines[0] == "PASS" || lines[0] == "FAIL") &&
                (PathFor(lines[1]).StartsWith(ownerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(currentOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(candidateOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(formalOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(pairSnapshotOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(emptyItrOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(emptyItrRosterOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(bruteExactCacheOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(bruteGeometryFirstOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(cpuGcCaptureOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(bruteProductionOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                 PathFor(lines[1]).StartsWith(branchTimingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(sampledTimingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(kind5PresenceOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(eligibilityReuseOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(combinedCacheOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(logicGcOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(logicCallsiteOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                  PathFor(lines[1]).StartsWith(labelPrewarmOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(coarseEnvelopeOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(envelopeBindingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(envelopeBindingEligibilityOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                   PathFor(lines[1]).StartsWith(envelopeBranchTimingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(renderTextureBindingOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(ordinalPacketOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                     PathFor(lines[1]).StartsWith(depthRejectOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(centralReadbackOwnerRoot, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(MatchCentralBindingsEvidenceRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(RoleBranchTimingOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(H07InputCostDiagnostics.CpuOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(H07InputCostDiagnostics.FlowOutputRoot) +
                         Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(H07InputCostDiagnostics.OutputRoot) +
                         Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                     PathFor(lines[1]).StartsWith(PathFor(H06RolePrerequisiteDiagnostics.QualificationRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(H06RolePrerequisiteDiagnostics.OutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(RoleRecordCostOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                     PathFor(lines[1]).StartsWith(PathFor(ProducerAbbaOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(ProducerSnapshotReuseOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(ProducerSnapshotReuseRetryOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(InputBoundaryTimingOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(RoleGroupSnapshotSkipOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(NativeSampledStateOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(ScopedProducerAdmissionOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(ScopedRoleAdmissionOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
                    PathFor(lines[1]).StartsWith(PathFor(PostBindingsRoleOutputRoot) +
                        Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));
        }

        private static bool EnvelopeBindingQueryDefaultsValid(BruteForceSceneQuery query) =>
            query != null && ProductionDefaultsEnabled(query) &&
            !query.EnableBruteKind5PresenceForDiagnostics && !query.EnableBruteEligibilityReuseForDiagnostics &&
            !query.EnableBruteCoarseDispatchForDiagnostics && !query.EnableBruteBranchTimingForDiagnostics &&
            !query.EnableBruteEnvelopeBranchTimingForDiagnostics;

        private static bool EnvelopeBindingScopeValid(BruteForceSceneQuery query, RunState run) =>
            !envelopeBindingEligibilityOwner && !envelopeBindingBranchTimingOwner &&
            query == envelopeBindingQuery && EnvelopeBindingQueryDefaultsValid(query) &&
            query.EnableBruteCoarseEnvelopeForDiagnostics &&
            query.EnableBruteRejectedBindingReuseForDiagnostics == run.bruteRejectedBindingReuseEnabled;

        private static bool EnvelopeBindingEligibilityScopeValid(BruteForceSceneQuery query, RunState run) =>
            envelopeBindingEligibilityOwner && !envelopeBindingBranchTimingOwner &&
            run.bruteEnvelopeBindingEligibilityMode && query != null &&
            query == envelopeBindingQuery && ProductionDefaultsEnabled(query) &&
            !query.EnableBruteKind5PresenceForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            !query.EnableBruteBranchTimingForDiagnostics && !query.EnableBruteEnvelopeBranchTimingForDiagnostics &&
            query.EnableBruteCoarseEnvelopeForDiagnostics &&
            query.EnableBruteRejectedBindingReuseForDiagnostics && run.bruteRejectedBindingReuseEnabled &&
            run.bruteEligibilityReuseFlagApplied &&
            query.EnableBruteEligibilityReuseForDiagnostics == run.bruteEligibilityReuseEnabled;

        private static bool EnvelopeBranchTimingScopeValid(BruteForceSceneQuery query, RunState run) =>
            envelopeBindingBranchTimingOwner && !envelopeBindingEligibilityOwner &&
            run.bruteEnvelopeBranchTimingMode && query != null && query == envelopeBindingQuery &&
            ProductionDefaultsEnabled(query) && !query.EnableBruteKind5PresenceForDiagnostics &&
            !query.EnableBruteEligibilityReuseForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            query.EnableBruteCoarseEnvelopeForDiagnostics && query.EnableBruteRejectedBindingReuseForDiagnostics &&
            query.EnableBruteEnvelopeBranchTimingForDiagnostics && run.bruteEnvelopeBranchTimingFlagApplied &&
            run.bruteBranchTimingFlagApplied && query.EnableBruteBranchTimingForDiagnostics == run.bruteBranchTimingEnabled &&
            query.BruteBranchTimingSampleStrideForDiagnostics == run.bruteBranchTimingSampleStride &&
            run.bruteBranchTimingSampleStride == 64;

        private static void ApplyBruteEnvelopeBranchTiming(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            ApplyBruteEnvelopeBinding(query, run, true);
            previousEnvelopeBranchTiming = query.EnableBruteEnvelopeBranchTimingForDiagnostics;
            previousBranchTiming = query.EnableBruteBranchTimingForDiagnostics;
            previousBranchTimingSampleStride = query.BruteBranchTimingSampleStrideForDiagnostics;
            envelopeBindingBranchTimingOwner = true;
            run.bruteEnvelopeBranchTimingMode = true;
            run.bruteBranchTimingEnabled = enabled;
            run.bruteBranchTimingSampleStride = 64;
            query.EnableBruteEnvelopeBranchTimingForDiagnostics = true;
            query.BruteBranchTimingSampleStrideForDiagnostics = 64;
            query.EnableBruteBranchTimingForDiagnostics = enabled;
            run.bruteEnvelopeBranchTimingFlagApplied = query.EnableBruteEnvelopeBranchTimingForDiagnostics;
            run.bruteEnvelopeBranchTimingFlagUnchanged = run.bruteEnvelopeBranchTimingFlagApplied;
            run.bruteBranchTimingFlagApplied = query.EnableBruteBranchTimingForDiagnostics == enabled &&
                query.BruteBranchTimingSampleStrideForDiagnostics == 64;
            run.bruteBranchTimingFlagUnchanged = run.bruteBranchTimingFlagApplied;
            Require(EnvelopeBranchTimingScopeValid(query, run), EnvelopeBindingOwnerError);
        }

        private static void ApplyBruteEnvelopeBindingEligibility(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            ApplyBruteEnvelopeBinding(query, run, true);
            previousEnvelopeBindingEligibility = query.EnableBruteEligibilityReuseForDiagnostics;
            envelopeBindingEligibilityOwner = true;
            run.bruteEnvelopeBindingEligibilityMode = true;
            run.bruteEnvelopeBindingPreviousEligibility = previousEnvelopeBindingEligibility;
            run.bruteEligibilityReuseEnabled = enabled;
            run.bruteEligibilityReuseAppliedBaseline = query.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics;
            query.EnableBruteEligibilityReuseForDiagnostics = enabled;
            run.bruteEligibilityReuseFlagApplied = query.EnableBruteEligibilityReuseForDiagnostics == enabled;
            run.bruteEligibilityReuseFlagUnchanged = run.bruteEligibilityReuseFlagApplied;
            Require(EnvelopeBindingEligibilityScopeValid(query, run), EnvelopeBindingOwnerError);
        }

        private static void ApplyBruteEnvelopeBinding(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            Require(!envelopeBindingEligibilityOwner && !envelopeBindingBranchTimingOwner &&
                envelopeBindingQuery == null && coarseEnvelopeQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && kind5PresenceQuery == null &&
                emptyItrGuardQuery == null && EnvelopeBindingQueryDefaultsValid(query) &&
                !query.EnableBruteCoarseEnvelopeForDiagnostics && !query.EnableBruteRejectedBindingReuseForDiagnostics,
                EnvelopeBindingOwnerError);
            envelopeBindingQuery = query;
            previousEnvelopeBindingEnvelope = query.EnableBruteCoarseEnvelopeForDiagnostics;
            previousEnvelopeBindingReuse = query.EnableBruteRejectedBindingReuseForDiagnostics;
            run.bruteEnvelopeBindingPreviousEnvelope = previousEnvelopeBindingEnvelope;
            run.bruteEnvelopeBindingPreviousReuse = previousEnvelopeBindingReuse;
            run.bruteCoarseEnvelopeEnabled = true;
            run.bruteRejectedBindingReuseEnabled = enabled;
            query.EnableBruteCoarseEnvelopeForDiagnostics = true;
            query.EnableBruteRejectedBindingReuseForDiagnostics = enabled;
            run.bruteCoarseEnvelopeFlagApplied = query.EnableBruteCoarseEnvelopeForDiagnostics;
            run.bruteRejectedBindingReuseFlagApplied = query.EnableBruteRejectedBindingReuseForDiagnostics == enabled;
            run.bruteCoarseEnvelopeFlagUnchanged = run.bruteCoarseEnvelopeFlagApplied;
            run.bruteRejectedBindingReuseFlagUnchanged = run.bruteRejectedBindingReuseFlagApplied;
            Require(run.bruteCoarseEnvelopeFlagApplied && run.bruteRejectedBindingReuseFlagApplied, EnvelopeBindingOwnerError);
        }

        private static void ObserveBruteEnvelopeBinding(RunState run)
        {
            if (envelopeBindingQuery == null)
            {
                run.bruteCoarseEnvelopeFlagUnchanged = false;
                run.bruteRejectedBindingReuseFlagUnchanged = false;
                if (run.bruteEnvelopeBindingEligibilityMode)
                    run.bruteEligibilityReuseFlagUnchanged = false;
                if (run.bruteEnvelopeBranchTimingMode)
                {
                    run.bruteEnvelopeBranchTimingFlagUnchanged = false;
                    run.bruteBranchTimingFlagUnchanged = false;
                }
                return;
            }
            bool defaultsValid = run.bruteEnvelopeBranchTimingMode
                ? EnvelopeBranchTimingScopeValid(envelopeBindingQuery, run)
                : run.bruteEnvelopeBindingEligibilityMode
                ? EnvelopeBindingEligibilityScopeValid(envelopeBindingQuery, run)
                : EnvelopeBindingQueryDefaultsValid(envelopeBindingQuery);
            if (run.bruteEnvelopeBranchTimingMode)
            {
                run.bruteEnvelopeBranchTimingFlagUnchanged &= defaultsValid;
                run.bruteBranchTimingFlagUnchanged &= defaultsValid;
            }
            if (run.bruteEnvelopeBindingEligibilityMode)
            {
                run.bruteEligibilityReuseFlagUnchanged &= defaultsValid;
                run.bruteEligibilityReuseObservedApplied |= envelopeBindingQuery.LastBruteEligibilityReuseAppliedForDiagnostics;
            }
            run.bruteCoarseEnvelopeFlagUnchanged &= defaultsValid && envelopeBindingQuery.EnableBruteCoarseEnvelopeForDiagnostics;
            run.bruteRejectedBindingReuseFlagUnchanged &= defaultsValid &&
                envelopeBindingQuery.EnableBruteRejectedBindingReuseForDiagnostics == run.bruteRejectedBindingReuseEnabled;
            run.bruteCoarseEnvelopeMaximumObservedDirections = Math.Max(run.bruteCoarseEnvelopeMaximumObservedDirections,
                envelopeBindingQuery.LastBruteCoarseEnvelopeDirectionCountForDiagnostics);
            run.bruteCoarseEnvelopeMaximumObservedRejects = Math.Max(run.bruteCoarseEnvelopeMaximumObservedRejects,
                envelopeBindingQuery.LastBruteCoarseEnvelopeRejectCountForDiagnostics);
            run.bruteRejectedBindingReuseObservedApplied |= envelopeBindingQuery.LastBruteRejectedBindingReuseAppliedForDiagnostics;
            run.bruteRejectedBindingMaximumObservedProbes = Math.Max(run.bruteRejectedBindingMaximumObservedProbes,
                envelopeBindingQuery.LastBruteRejectedBindingProbeCountForDiagnostics);
            run.bruteRejectedBindingMaximumObservedReuses = Math.Max(run.bruteRejectedBindingMaximumObservedReuses,
                envelopeBindingQuery.LastBruteRejectedBindingReuseCountForDiagnostics);
        }

        private static void CompleteBruteEnvelopeBinding(RunState run)
        {
            ObserveBruteEnvelopeBinding(run);
            Require(envelopeBindingQuery != null && run.bruteCoarseEnvelopeFlagApplied &&
                run.bruteRejectedBindingReuseFlagApplied && run.bruteCoarseEnvelopeFlagUnchanged &&
                run.bruteRejectedBindingReuseFlagUnchanged, EnvelopeBindingDriftError);
            if (run.bruteEnvelopeBindingEligibilityMode)
            {
                Require(envelopeBindingEligibilityOwner && run.bruteEligibilityReuseFlagApplied &&
                    run.bruteEligibilityReuseFlagUnchanged, EnvelopeBindingDriftError);
                run.bruteEligibilityReuseAppliedDelta =
                    envelopeBindingQuery.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics -
                    run.bruteEligibilityReuseAppliedBaseline;
            }
            if (run.bruteEnvelopeBranchTimingMode)
            {
                Require(envelopeBindingBranchTimingOwner && run.bruteEnvelopeBranchTimingFlagApplied &&
                    run.bruteEnvelopeBranchTimingFlagUnchanged && run.bruteBranchTimingFlagApplied &&
                    run.bruteBranchTimingFlagUnchanged, EnvelopeBindingDriftError);
                run.bruteBranchTimingCoverage = envelopeBindingQuery.TotalBruteBranchTimingCoverageForDiagnostics;
                run.bruteBranchTimingCoverageScope = "Totals include warmup and sampled collections; clocks only stride64-selected directions, not full-cost or steady-only.";
            }
            bool restored = RestoreBruteEnvelopeBinding();
            run.bruteCoarseEnvelopeRestored = restored;
            run.bruteRejectedBindingReuseRestored = restored;
            if (run.bruteEnvelopeBindingEligibilityMode)
                run.bruteEligibilityReuseRestored = restored;
            if (run.bruteEnvelopeBranchTimingMode)
            {
                run.bruteEnvelopeBranchTimingRestored = restored;
                run.bruteBranchTimingRestored = restored;
            }
        }

        private static bool EnvelopeBindingObservationValid(RunState run) =>
            run.bruteCoarseEnvelopeEnabled && CoarseEnvelopeObservationValid(run) &&
            run.bruteRejectedBindingReuseFlagApplied && run.bruteRejectedBindingReuseFlagUnchanged &&
            run.bruteRejectedBindingReuseRestored &&
            (run.bruteRejectedBindingReuseEnabled
                ? run.bruteRejectedBindingReuseObservedApplied && run.bruteRejectedBindingMaximumObservedProbes > 0 &&
                    run.bruteRejectedBindingMaximumObservedReuses > 0
                : !run.bruteRejectedBindingReuseObservedApplied && run.bruteRejectedBindingMaximumObservedProbes == 0 &&
                    run.bruteRejectedBindingMaximumObservedReuses == 0);

        private static bool EnvelopeBindingEligibilityObservationValid(RunState run) =>
            run.bruteEnvelopeBindingEligibilityMode && EnvelopeBindingObservationValid(run) &&
            run.bruteEligibilityReuseFlagApplied && run.bruteEligibilityReuseFlagUnchanged &&
            run.bruteEligibilityReuseRestored &&
            run.bruteEligibilityReuseObservedApplied == run.bruteEligibilityReuseEnabled &&
            run.bruteEligibilityReuseAppliedDelta == (run.bruteEligibilityReuseEnabled ? run.warmupTicks + run.sampledTicks : 0);

        private static bool EnvelopeBranchTimingObservationValid(RunState run)
        {
            if (!run.bruteEnvelopeBranchTimingMode || !EnvelopeBindingObservationValid(run) ||
                !run.bruteEnvelopeBranchTimingFlagApplied || !run.bruteEnvelopeBranchTimingFlagUnchanged ||
                !run.bruteEnvelopeBranchTimingRestored || !run.bruteBranchTimingFlagApplied ||
                !run.bruteBranchTimingFlagUnchanged || !run.bruteBranchTimingRestored ||
                run.bruteBranchTimingSampleStride != 64)
                return false;
            var coverage = run.bruteBranchTimingCoverage;
            if (!run.bruteBranchTimingEnabled)
                return coverage.eligibleDirections == 0 && coverage.timedDirections == 0 &&
                    coverage.rejectedBindingVisits == 0 && coverage.rejectedBindingTimed == 0 &&
                    coverage.pairAllowedVisits == 0 && coverage.pairAllowedTimed == 0 &&
                    coverage.exactWorkVisits == 0 && coverage.exactWorkTimed == 0;
            return coverage.eligibleDirections > 0 && coverage.timedDirections > 0 &&
                coverage.timedDirections <= coverage.eligibleDirections &&
                coverage.rejectedBindingVisits > 0 && coverage.rejectedBindingTimed > 0 &&
                coverage.rejectedBindingTimed <= coverage.rejectedBindingVisits &&
                coverage.pairAllowedVisits > 0 && coverage.pairAllowedTimed > 0 &&
                coverage.pairAllowedTimed <= coverage.pairAllowedVisits &&
                coverage.exactWorkVisits > 0 && coverage.exactWorkTimed > 0 &&
                coverage.exactWorkTimed <= coverage.exactWorkVisits;
        }

        private static bool RestoreBruteEnvelopeBindingForExit()
        {
            bool restored = RestoreBruteEnvelopeBinding();
            if (state != null && (state.bruteEnvelopeBindingCandidate || state.bruteEnvelopeBindingEligibilityCandidate ||
                state.bruteEnvelopeBranchTimingCandidate) &&
                state.runIndex >= 0 &&
                state.runIndex < state.runs.Length)
            {
                RunState run = state.runs[state.runIndex];
                run.bruteCoarseEnvelopeRestored = restored;
                run.bruteRejectedBindingReuseRestored = restored;
                if (run.bruteEnvelopeBindingEligibilityMode)
                    run.bruteEligibilityReuseRestored = restored;
                if (run.bruteEnvelopeBranchTimingMode)
                {
                    run.bruteEnvelopeBranchTimingRestored = restored;
                    run.bruteBranchTimingRestored = restored;
                }
            }
            return restored;
        }

        private static bool RestoreBruteEnvelopeBinding()
        {
            if (envelopeBindingQuery == null)
                return true;
            envelopeBindingQuery.EnableBruteCoarseEnvelopeForDiagnostics = previousEnvelopeBindingEnvelope;
            envelopeBindingQuery.EnableBruteRejectedBindingReuseForDiagnostics = previousEnvelopeBindingReuse;
            bool restored = envelopeBindingQuery.EnableBruteCoarseEnvelopeForDiagnostics == previousEnvelopeBindingEnvelope &&
                envelopeBindingQuery.EnableBruteRejectedBindingReuseForDiagnostics == previousEnvelopeBindingReuse;
            if (envelopeBindingEligibilityOwner)
            {
                envelopeBindingQuery.EnableBruteEligibilityReuseForDiagnostics = previousEnvelopeBindingEligibility;
                restored &= envelopeBindingQuery.EnableBruteEligibilityReuseForDiagnostics == previousEnvelopeBindingEligibility;
            }
            if (envelopeBindingBranchTimingOwner)
            {
                envelopeBindingQuery.EnableBruteEnvelopeBranchTimingForDiagnostics = previousEnvelopeBranchTiming;
                envelopeBindingQuery.EnableBruteBranchTimingForDiagnostics = previousBranchTiming;
                envelopeBindingQuery.BruteBranchTimingSampleStrideForDiagnostics = previousBranchTimingSampleStride;
                restored &= envelopeBindingQuery.EnableBruteEnvelopeBranchTimingForDiagnostics == previousEnvelopeBranchTiming &&
                    envelopeBindingQuery.EnableBruteBranchTimingForDiagnostics == previousBranchTiming &&
                    envelopeBindingQuery.BruteBranchTimingSampleStrideForDiagnostics == previousBranchTimingSampleStride;
            }
            envelopeBindingEligibilityOwner = false;
            envelopeBindingBranchTimingOwner = false;
            envelopeBindingQuery = null;
            return restored;
        }

        private static bool CoarseEnvelopeQueryDefaultsValid(BruteForceSceneQuery query) =>
            query != null && ProductionDefaultsEnabled(query) &&
            !query.EnableBruteKind5PresenceForDiagnostics && !query.EnableBruteEligibilityReuseForDiagnostics &&
            !query.EnableBruteRejectedBindingReuseForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            !query.EnableBruteBranchTimingForDiagnostics && !query.EnableBruteEnvelopeBranchTimingForDiagnostics;

        private static void ApplyRenderTextureBindingReuse(RunState run, bool enabled)
        {
            Require(run != null && !renderTextureBindingReuseOwner &&
                !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics, RenderTextureBindingOwnerError);
            previousRenderTextureBindingReuse = BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics;
            renderTextureBindingReuseOwner = true;
            renderTextureBindingReuseRun = run;
            lastRenderTextureBindingExecuteSequence = BattleRenderFeature.LastSegmentTextureBindingExecuteSequenceForDiagnostics;
            run.renderTextureBindingReuseEnabled = enabled;
            BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics = enabled;
            run.renderTextureBindingReuseFlagApplied = BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == enabled;
            run.renderTextureBindingReuseFlagUnchanged = run.renderTextureBindingReuseFlagApplied;
            RenderPipelineManager.endCameraRendering += ObserveRenderTextureBindingCamera;
        }

        private static bool IsRenderTextureBindingObservationFresh(
            long previousSequence, long currentSequence, int expectedCameraId, int actualCameraId,
            int prepareCount, int reuseCount) =>
            previousSequence >= 0 && currentSequence > previousSequence && expectedCameraId != 0 &&
            actualCameraId == expectedCameraId && prepareCount > 0 && reuseCount >= 0;

        private static void ObserveRenderTextureBindingCamera(ScriptableRenderContext context, Camera camera)
        {
            RunState run = renderTextureBindingReuseRun;
            if (!renderTextureBindingReuseOwner || run == null)
                return;
            run.renderTextureBindingReuseFlagUnchanged &=
                BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == run.renderTextureBindingReuseEnabled;
            if (!Application.isPlaying || camera == null || camera.cameraType != CameraType.Game ||
                currentReport == null || currentReport.status != "Running" ||
                currentReport.warmupTicksCompleted < 120 || currentReport.sampledLogicTicks <= 0)
                return;
            int cameraId = camera.GetInstanceID();
            if (run.renderTextureBindingCameraId != 0 && run.renderTextureBindingCameraId != cameraId)
                return;
            long sequence = BattleRenderFeature.LastSegmentTextureBindingExecuteSequenceForDiagnostics;
            int prepareCount = BattleRenderFeature.LastSegmentTextureBindingPrepareCountForDiagnostics;
            int reuseCount = BattleRenderFeature.LastSegmentTextureBindingReuseCountForDiagnostics;
            if (!IsRenderTextureBindingObservationFresh(lastRenderTextureBindingExecuteSequence, sequence,
                    cameraId,
                    BattleRenderFeature.LastSegmentTextureBindingCameraIdForDiagnostics, prepareCount, reuseCount))
                return;
            run.renderTextureBindingCameraId = cameraId;
            lastRenderTextureBindingExecuteSequence = sequence;
            run.renderTextureBindingAcceptedCameraSamples++;
            run.renderTextureBindingBodyPrepareCount += prepareCount;
            run.renderTextureBindingBodyReuseCount += reuseCount;
        }

        private static void CompleteRenderTextureBindingReuse(RunState run)
        {
            Require(renderTextureBindingReuseOwner && ReferenceEquals(renderTextureBindingReuseRun, run) &&
                run.renderTextureBindingReuseFlagApplied, RenderTextureBindingOwnerError);
            run.renderTextureBindingReuseFlagUnchanged &=
                BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == run.renderTextureBindingReuseEnabled;
            Require(run.renderTextureBindingReuseFlagUnchanged, RenderTextureBindingDriftError);
            Require(run.renderTextureBindingAcceptedCameraSamples > 0 && run.renderTextureBindingBodyPrepareCount > 0 &&
                run.renderTextureBindingBodyReuseCount >= 0 &&
                (run.renderTextureBindingReuseEnabled || run.renderTextureBindingBodyReuseCount == 0),
                RenderTextureBindingObservationError);
            run.renderTextureBindingReuseRestored = RestoreRenderTextureBindingReuse();
        }

        private static bool RestoreRenderTextureBindingReuse()
        {
            if (!renderTextureBindingReuseOwner)
                return true;
            RenderPipelineManager.endCameraRendering -= ObserveRenderTextureBindingCamera;
            BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics = previousRenderTextureBindingReuse;
            bool restored = BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics == previousRenderTextureBindingReuse;
            if (renderTextureBindingReuseRun != null)
                renderTextureBindingReuseRun.renderTextureBindingReuseRestored = restored;
            renderTextureBindingReuseOwner = false;
            renderTextureBindingReuseRun = null;
            lastRenderTextureBindingExecuteSequence = 0;
            return restored;
        }


        private static int CentralSegmentBreakMask(
            BattleCentralRenderSegment previous, BattleCentralRenderSegment next, BattleCentralDrawMode drawMode)
        {
            // Alignment contract: NTSD-OPT-H07-CENTRAL-SEGMENT-READBACK-075; mirrors only observed CPU split conditions.
            int mask = drawMode == BattleCentralDrawMode.StrictOrderedDraw ? 1 : 0;
            if (previous.ChunkIndex != next.ChunkIndex) mask |= 2;
            if (!ReferenceEquals(previous.Texture, next.Texture)) mask |= 4;
            if (!ReferenceEquals(previous.Material, next.Material)) mask |= 8;
            if (previous.MaterialVariant != next.MaterialVariant) mask |= 16;
            if (previous.BindingMode != next.BindingMode) mask |= 32;
            if (next.BindingMode == BattleSpriteCentralBindingMode.AtlasPageTexture2D &&
                previous.AtlasPageIndex != next.AtlasPageIndex) mask |= 64;
            if (next.BindingMode != BattleSpriteCentralBindingMode.AtlasTextureArray &&
                previous.AtlasSlice != next.AtlasSlice) mask |= 128;
            if (previous.FirstCommandIndex + previous.CommandCount != next.FirstCommandIndex) mask |= 256;
            if (previous.ChunkIndex == next.ChunkIndex &&
                previous.FirstQuad + previous.QuadCount != next.FirstQuad) mask |= 512;
            return mask;
        }

        private static bool CentralSegmentReadbackPending(RunState run) =>
            run != null && !run.centralSegmentReadbackCaptured && string.IsNullOrEmpty(run.centralSegmentReadbackError);

        private static void ObserveCentralSegmentReadback(ScriptableRenderContext context, Camera camera)
        {
            RunState run = centralSegmentReadbackRun;
            if (!CentralSegmentReadbackPending(run) ||
                !Application.isPlaying || camera == null || camera.cameraType != CameraType.Game ||
                currentReport == null || currentReport.status != "Running" ||
                currentReport.warmupTicksCompleted < 120 || currentReport.sampledLogicTicks < 8)
                return;
            CameraRenderType renderType = camera.TryGetComponent(out UniversalAdditionalCameraData cameraData)
                ? cameraData.renderType : CameraRenderType.Base;
            if (!BattleCentralRenderSystem.TryAcquireSubmissionForSelfCheck(
                    camera, renderType, camera.cameraType, true, out BattleCentralSubmission.BattleCentralSubmissionLease lease))
                return;
            CentralSegmentReadbackSnapshot snapshot = null;
            try
            {
                try
                {
                    Require(OrdinalPacketQueryDefaultsValid(productionQuery) &&
                        !productionQuery.EnableBruteOrdinalPacketForDiagnostics &&
                        !productionQuery.EnableBruteDepthRejectForDiagnostics, "A diagnostic candidate drifted during readback.");
                    BattleDynamicMeshBackend backend = lease.Backend;
                    BattleCentralBuildDiagnostics diagnostics = backend.Diagnostics;
                    BattleDrawPolicyDecision draw = BattleCentralRenderSystem.DrawPolicyDecision;
                    snapshot = new CentralSegmentReadbackSnapshot
                    {
                        utc = DateTime.UtcNow.ToString("o"), tick = lease.TickIndex, generation = lease.Generation,
                        cameraIdentity = camera.GetInstanceID(), sampledLogicTicks = currentReport.sampledLogicTicks,
                        activeAi = currentReport.baseAiActiveCount,
                        drawMode = diagnostics.DrawMode.ToString(), drawPolicyRequested = draw.RequestedMode.ToString(),
                        drawPolicyEffective = draw.EffectiveMode.ToString(), drawPolicyReason = draw.FallbackOrRefusalReason,
                        sourceCommands = diagnostics.SourceCommandCount, resolvedCommands = diagnostics.ResolvedCommandCount,
                        unresolvedCommands = diagnostics.UnresolvedCommandCount,
                        unsupportedCategories = diagnostics.UnsupportedCategoryCount,
                        unsupportedStates = diagnostics.UnsupportedRenderStateCount,
                        chunks = backend.ActiveChunkCount, segments = backend.SegmentCount,
                        rows = new CentralSegmentReadbackRow[backend.SegmentCount],
                    };
                    CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                    if (manager != null)
                    {
                        BattleAtlasDiagnosticInputs atlas = manager.LastAtlasDiagnosticInputs;
                        snapshot.atlasDiagnostic = manager.LastAtlasDiagnostic;
                        if (atlas != null)
                        {
                            snapshot.atlasInputsKnown = true;
                            snapshot.atlasRequested = atlas.Decision.RequestedMode.ToString();
                            snapshot.atlasEffective = atlas.Decision.EffectiveMode.ToString();
                            snapshot.atlasReason = atlas.Decision.FallbackOrRefusalReason;
                            snapshot.catalogMode = atlas.CatalogResourceMode.ToString();
                            snapshot.plannedPageCount = atlas.PlannedPageCount;
                            snapshot.estimatedAtlasBytes = atlas.EstimatedAtlasBytes;
                            snapshot.atlasBudgetBytes = atlas.Capabilities.AtlasMemoryBudgetBytes;
                        }
                    }
                    for (int index = 0; index < backend.SegmentCount; index++)
                    {
                        BattleCentralRenderSegment segment = backend.GetSegment(index);
                        Texture texture = segment.Texture;
                        Material material = segment.Material;
                        Shader shader = material == null ? null : material.shader;
                        int textureId = texture == null ? 0 : texture.GetInstanceID();
                        snapshot.rows[index] = new CentralSegmentReadbackRow
                        {
                            index = index, chunk = segment.ChunkIndex, subMesh = segment.SubMeshIndex,
                            firstCommand = segment.FirstCommandIndex, commands = segment.CommandCount,
                            firstQuad = segment.FirstQuad, quads = segment.QuadCount,
                            bindingMode = segment.BindingMode.ToString(), boundTextureIdentity = textureId,
                            SourceTextureIdentity = segment.BindingMode == BattleSpriteCentralBindingMode.SourceTexture2D ? textureId : 0,
                            AtlasPageIdentity = segment.BindingMode == BattleSpriteCentralBindingMode.AtlasPageTexture2D ? textureId : 0,
                            TextureArrayIdentity = segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ? textureId : 0,
                            textureName = texture == null ? null : texture.name,
                            textureType = texture == null ? null : texture.GetType().Name,
                            textureAssetPath = texture == null ? null : AssetDatabase.GetAssetPath(texture),
                            slice = segment.AtlasSlice, pageIndex = segment.AtlasPageIndex,
                            materialIdentity = material == null ? 0 : material.GetInstanceID(), variant = segment.MaterialVariant,
                            materialName = material == null ? null : material.name,
                            shaderIdentity = shader == null ? 0 : shader.GetInstanceID(),
                            shaderName = shader == null ? null : shader.name,
                            shaderPassCount = material == null ? 0 : material.passCount,
                            renderQueue = material == null ? 0 : material.renderQueue,
                            shaderKeywords = material == null ? null : string.Join(",", material.shaderKeywords),
                            breakMaskFromPrevious = index == 0 ? 0 : CentralSegmentBreakMask(
                                backend.GetSegment(index - 1), segment, diagnostics.DrawMode),
                        };
                    }
                }
                finally
                {
                    lease.Dispose();
                }
                snapshot.cpuLeaseReleased = lease.Submission.ReadLeaseCount == 0;
                Require(snapshot.cpuLeaseReleased, "The readback CPU lease must be returned in the same callback.");
                SaveNew(run.reportPath + ".central-segments.json", JsonUtility.ToJson(snapshot, true));
                run.centralSegmentReadbackCaptured = true;
            }
            catch (Exception exception)
            {
                run.centralSegmentReadbackError = exception.ToString();
            }
            finally
            {
                RestoreCentralSegmentReadback();
            }
        }

        private static bool RestoreCentralSegmentReadback()
        {
            RenderPipelineManager.endCameraRendering -= ObserveCentralSegmentReadback;
            if (centralSegmentReadbackRun != null)
                centralSegmentReadbackRun.centralSegmentReadbackDetached = true;
            centralSegmentReadbackRun = null;
            return true;
        }

        private static bool OrdinalPacketQueryDefaultsValid(BruteForceSceneQuery query) =>
            query != null && query.FormalCollectorMode == CollisionFormalCollectorMode.ForceBruteForce &&
            ProductionDefaultsEnabled(query) && !query.EnableBruteKind5PresenceForDiagnostics &&
            !query.EnableBruteEligibilityReuseForDiagnostics && !query.EnableBruteCoarseEnvelopeForDiagnostics &&
            !query.EnableBruteRejectedBindingReuseForDiagnostics && !query.EnableBruteCoarseDispatchForDiagnostics &&
            !query.EnableBruteCoarseProofReuseForDiagnostics && !query.EnableBruteBranchTimingForDiagnostics &&
            !query.EnableBruteEnvelopeBranchTimingForDiagnostics &&
            !BattleRenderFeature.EnableSegmentTextureBindingReuseForDiagnostics;

        private static void ApplyBruteOrdinalPacket(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            // Alignment contract: NTSD-OPT-H07-ORDINAL-PACKET-WINDOWS-072; cold flag ownership, not production admission.
            Require(run != null && ordinalPacketQuery == null && ordinalPacketRun == null &&
                coarseEnvelopeQuery == null && envelopeBindingQuery == null && kind5PresenceQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && !renderTextureBindingReuseOwner &&
                OrdinalPacketQueryDefaultsValid(query) && !query.EnableBruteOrdinalPacketForDiagnostics,
                "The ordinal packet window requires exclusive query/run ownership, four production defaults and no other candidate.");
            Require(!query.EnableBruteDepthRejectForDiagnostics, "The ordinal packet window excludes depth reject.");
            ordinalPacketQuery = query;
            ordinalPacketRun = run;
            previousOrdinalPacket = query.EnableBruteOrdinalPacketForDiagnostics;
            run.bruteOrdinalPacketEnabled = enabled;
            run.bruteOrdinalPacketCapacityBeforePopulation = query.BruteOrdinalPacketCapacityForDiagnostics;
            run.bruteOrdinalPacketCapacityStable = true;
            query.EnableBruteOrdinalPacketForDiagnostics = enabled;
            run.bruteOrdinalPacketFlagApplied = query.EnableBruteOrdinalPacketForDiagnostics == enabled;
            run.bruteOrdinalPacketFlagUnchanged = run.bruteOrdinalPacketFlagApplied;
        }

        private static void ObserveBruteOrdinalPacket(RunState run, int logicTick)
        {
            Require(run != null && ordinalPacketQuery != null && ReferenceEquals(ordinalPacketRun, run),
                "The ordinal packet window observation requires its owned run.");
            run.bruteOrdinalPacketFlagUnchanged &= OrdinalPacketQueryDefaultsValid(ordinalPacketQuery) &&
                !ordinalPacketQuery.EnableBruteDepthRejectForDiagnostics &&
                ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics == run.bruteOrdinalPacketEnabled;
            if (logicTick <= 0 || logicTick <= run.bruteOrdinalPacketLastObservedTick)
                return;
            int capacity = ordinalPacketQuery.BruteOrdinalPacketCapacityForDiagnostics;
            if (run.bruteOrdinalPacketObservedSamples == 0)
                run.bruteOrdinalPacketSteadyCapacity = capacity;
            run.bruteOrdinalPacketCapacityStable &= capacity > 0 && capacity == run.bruteOrdinalPacketSteadyCapacity;
            run.bruteOrdinalPacketMaximumObservedCapacity = Math.Max(run.bruteOrdinalPacketMaximumObservedCapacity, capacity);
            run.bruteOrdinalPacketLastObservedTick = logicTick;
            run.bruteOrdinalPacketObservedSamples++;
            run.bruteOrdinalPacketObservedApplied |= ordinalPacketQuery.LastBruteOrdinalPacketAppliedForDiagnostics;
            run.bruteOrdinalPacketFallbackObserved |= ordinalPacketQuery.LastBruteOrdinalPacketFallbackForDiagnostics;
            run.bruteOrdinalPacketMaximumObservedRejects = Math.Max(run.bruteOrdinalPacketMaximumObservedRejects,
                ordinalPacketQuery.LastBruteOrdinalPacketRejectedDirectionCountForDiagnostics);
            run.bruteOrdinalPacketMaximumObservedProofs = Math.Max(run.bruteOrdinalPacketMaximumObservedProofs,
                ordinalPacketQuery.LastBruteOrdinalPacketProofCountForDiagnostics);
        }

        private static void CompleteBruteOrdinalPacket(RunState run)
        {
            Require(run != null && ordinalPacketQuery != null && ReferenceEquals(ordinalPacketRun, run),
                "The ordinal packet window completion requires its owned run.");
            run.bruteOrdinalPacketFlagUnchanged &= OrdinalPacketQueryDefaultsValid(ordinalPacketQuery) &&
                !ordinalPacketQuery.EnableBruteDepthRejectForDiagnostics &&
                ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics == run.bruteOrdinalPacketEnabled;
            // Restore even when evidence is invalid; a wrong run never reaches this release.
            run.bruteOrdinalPacketRestored = RestoreBruteOrdinalPacket();
            Require(OrdinalPacketObservationValid(run),
                "The ordinal packet window requires fresh application evidence, unchanged flags/capacity and no observed fallback.");
        }

        private static bool OrdinalPacketObservationValid(RunState run) =>
            run != null && run.bruteOrdinalPacketFlagApplied && run.bruteOrdinalPacketFlagUnchanged &&
            run.bruteOrdinalPacketRestored && run.bruteOrdinalPacketObservedSamples > 0 &&
            run.bruteOrdinalPacketCapacityStable && run.bruteOrdinalPacketSteadyCapacity > 0 &&
            !run.bruteOrdinalPacketFallbackObserved &&
            (run.bruteOrdinalPacketEnabled
                ? run.bruteOrdinalPacketObservedApplied && run.bruteOrdinalPacketMaximumObservedRejects > 0 &&
                    run.bruteOrdinalPacketMaximumObservedProofs > 0
                : !run.bruteOrdinalPacketObservedApplied && run.bruteOrdinalPacketMaximumObservedRejects == 0 &&
                    run.bruteOrdinalPacketMaximumObservedProofs == 0);

        private static bool RestoreBruteOrdinalPacket()
        {
            if (ordinalPacketQuery == null)
                return true;
            ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics = previousOrdinalPacket;
            bool restored = ordinalPacketQuery.EnableBruteOrdinalPacketForDiagnostics == previousOrdinalPacket;
            if (ordinalPacketRun != null)
                ordinalPacketRun.bruteOrdinalPacketRestored = restored;
            ordinalPacketQuery = null;
            ordinalPacketRun = null;
            return restored;
        }


        private static bool DepthRejectQueryDefaultsValid(BruteForceSceneQuery query) =>
            OrdinalPacketQueryDefaultsValid(query) && !query.EnableBruteOrdinalPacketForDiagnostics;

        private static void ApplyBruteDepthReject(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            Require(run != null && depthRejectQuery == null && depthRejectRun == null &&
                ordinalPacketQuery == null && ordinalPacketRun == null &&
                coarseEnvelopeQuery == null && envelopeBindingQuery == null && kind5PresenceQuery == null &&
                eligibilityReuseQuery == null && branchTimingQuery == null && !renderTextureBindingReuseOwner &&
                DepthRejectQueryDefaultsValid(query) && !query.EnableBruteDepthRejectForDiagnostics,
                "The depth reject window requires exclusive query/run ownership, four production defaults and no other candidate.");
            depthRejectQuery = query;
            depthRejectRun = run;
            previousDepthReject = query.EnableBruteDepthRejectForDiagnostics;
            run.bruteDepthRejectEnabled = enabled;
            run.bruteDepthRejectCapacityStable = true;
            query.EnableBruteDepthRejectForDiagnostics = enabled;
            run.bruteDepthRejectFlagApplied = query.EnableBruteDepthRejectForDiagnostics == enabled;
            run.bruteDepthRejectFlagUnchanged = run.bruteDepthRejectFlagApplied;
        }

        private static void ObserveBruteDepthReject(RunState run, int logicTick)
        {
            Require(run != null && depthRejectQuery != null && ReferenceEquals(depthRejectRun, run),
                "The depth reject window observation requires its owned run.");
            run.bruteDepthRejectFlagUnchanged &= DepthRejectQueryDefaultsValid(depthRejectQuery) &&
                depthRejectQuery.EnableBruteDepthRejectForDiagnostics == run.bruteDepthRejectEnabled;
            if (logicTick <= 0 || logicTick <= run.bruteDepthRejectLastObservedTick)
                return;
            int participant = depthRejectQuery.BruteExactParticipantCapacityForDiagnostics;
            int itr = depthRejectQuery.BruteExactItrCapacityForDiagnostics;
            int body = depthRejectQuery.BruteExactBodyCapacityForDiagnostics;
            if (run.bruteDepthRejectObservedSamples == 0)
            {
                run.bruteDepthRejectSteadyParticipantCapacity = participant;
                run.bruteDepthRejectSteadyItrCapacity = itr;
                run.bruteDepthRejectSteadyBodyCapacity = body;
            }
            run.bruteDepthRejectCapacityStable &= participant > 0 && itr > 0 && body > 0 &&
                participant == run.bruteDepthRejectSteadyParticipantCapacity &&
                itr == run.bruteDepthRejectSteadyItrCapacity && body == run.bruteDepthRejectSteadyBodyCapacity;
            run.bruteDepthRejectLastObservedTick = logicTick;
            run.bruteDepthRejectObservedSamples++;
            run.bruteDepthRejectObservedApplied |= depthRejectQuery.LastBruteDepthRejectAppliedForDiagnostics;
            run.bruteDepthRejectFallbackObserved |= depthRejectQuery.LastBruteExactCacheFallbackForDiagnostics;
            run.bruteDepthRejectMaximumObservedRejects = Math.Max(run.bruteDepthRejectMaximumObservedRejects,
                depthRejectQuery.LastBruteDepthRejectedDirectionCountForDiagnostics);
        }

        private static void CompleteBruteDepthReject(RunState run)
        {
            Require(run != null && depthRejectQuery != null && ReferenceEquals(depthRejectRun, run),
                "The depth reject window completion requires its owned run.");
            run.bruteDepthRejectFlagUnchanged &= DepthRejectQueryDefaultsValid(depthRejectQuery) &&
                depthRejectQuery.EnableBruteDepthRejectForDiagnostics == run.bruteDepthRejectEnabled;
            run.bruteDepthRejectRestored = RestoreBruteDepthReject();
            Require(DepthRejectObservationValid(run),
                "The depth reject window requires fresh application evidence, unchanged flags/capacities and no observed fallback.");
        }

        private static bool DepthRejectObservationValid(RunState run) =>
            run != null && run.bruteDepthRejectFlagApplied && run.bruteDepthRejectFlagUnchanged &&
            run.bruteDepthRejectRestored && run.bruteDepthRejectObservedSamples > 0 &&
            run.bruteDepthRejectCapacityStable && run.bruteDepthRejectSteadyParticipantCapacity > 0 &&
            run.bruteDepthRejectSteadyItrCapacity > 0 && run.bruteDepthRejectSteadyBodyCapacity > 0 &&
            !run.bruteDepthRejectFallbackObserved &&
            (run.bruteDepthRejectEnabled
                ? run.bruteDepthRejectObservedApplied
                : !run.bruteDepthRejectObservedApplied && run.bruteDepthRejectMaximumObservedRejects == 0);

        private static bool RestoreBruteDepthReject()
        {
            if (depthRejectQuery == null)
                return true;
            depthRejectQuery.EnableBruteDepthRejectForDiagnostics = previousDepthReject;
            bool restored = depthRejectQuery.EnableBruteDepthRejectForDiagnostics == previousDepthReject;
            if (depthRejectRun != null)
                depthRejectRun.bruteDepthRejectRestored = restored;
            depthRejectQuery = null;
            depthRejectRun = null;
            return restored;
        }

        private static void ApplyBruteCoarseEnvelope(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            Require(coarseEnvelopeQuery == null && CoarseEnvelopeQueryDefaultsValid(query) &&
                !query.EnableBruteCoarseEnvelopeForDiagnostics, CoarseEnvelopeOwnerError);
            coarseEnvelopeQuery = query;
            previousCoarseEnvelope = query.EnableBruteCoarseEnvelopeForDiagnostics;
            run.bruteCoarseEnvelopeEnabled = enabled;
            query.EnableBruteCoarseEnvelopeForDiagnostics = enabled;
            run.bruteCoarseEnvelopeFlagApplied = query.EnableBruteCoarseEnvelopeForDiagnostics == enabled;
            run.bruteCoarseEnvelopeFlagUnchanged = run.bruteCoarseEnvelopeFlagApplied;
            Require(run.bruteCoarseEnvelopeFlagApplied, CoarseEnvelopeOwnerError);
        }

        private static void ObserveBruteCoarseEnvelope(RunState run)
        {
            if (coarseEnvelopeQuery == null)
            {
                run.bruteCoarseEnvelopeFlagUnchanged = false;
                return;
            }
            run.bruteCoarseEnvelopeFlagUnchanged &= CoarseEnvelopeQueryDefaultsValid(coarseEnvelopeQuery) &&
                coarseEnvelopeQuery.EnableBruteCoarseEnvelopeForDiagnostics == run.bruteCoarseEnvelopeEnabled;
            run.bruteCoarseEnvelopeMaximumObservedDirections = Math.Max(run.bruteCoarseEnvelopeMaximumObservedDirections,
                coarseEnvelopeQuery.LastBruteCoarseEnvelopeDirectionCountForDiagnostics);
            run.bruteCoarseEnvelopeMaximumObservedRejects = Math.Max(run.bruteCoarseEnvelopeMaximumObservedRejects,
                coarseEnvelopeQuery.LastBruteCoarseEnvelopeRejectCountForDiagnostics);
        }

        private static void CompleteBruteCoarseEnvelope(RunState run)
        {
            ObserveBruteCoarseEnvelope(run);
            Require(coarseEnvelopeQuery != null && run.bruteCoarseEnvelopeFlagApplied &&
                run.bruteCoarseEnvelopeFlagUnchanged, CoarseEnvelopeDriftError);
            run.bruteCoarseEnvelopeRestored = RestoreBruteCoarseEnvelope();
        }

        private static bool CoarseEnvelopeObservationValid(RunState run) =>
            run.bruteCoarseEnvelopeFlagApplied && run.bruteCoarseEnvelopeFlagUnchanged && run.bruteCoarseEnvelopeRestored &&
            (run.bruteCoarseEnvelopeEnabled
                ? run.bruteCoarseEnvelopeMaximumObservedDirections > 0 && run.bruteCoarseEnvelopeMaximumObservedRejects > 0
                : run.bruteCoarseEnvelopeMaximumObservedDirections == 0 && run.bruteCoarseEnvelopeMaximumObservedRejects == 0);

        private static bool RestoreBruteCoarseEnvelope()
        {
            if (coarseEnvelopeQuery == null)
                return true;
            coarseEnvelopeQuery.EnableBruteCoarseEnvelopeForDiagnostics = previousCoarseEnvelope;
            bool restored = coarseEnvelopeQuery.EnableBruteCoarseEnvelopeForDiagnostics == previousCoarseEnvelope;
            coarseEnvelopeQuery = null;
            return restored;
        }

        private static void ApplyBruteCombinedCache(BruteForceSceneQuery query, RunState run, bool kind5Enabled)
        {
            Require(kind5PresenceQuery == null, "Another kind5 comparison owns the query.");
            ApplyBruteEligibilityReuse(query, run, true);
            kind5PresenceQuery = query;
            previousKind5Presence = query.EnableBruteKind5PresenceForDiagnostics;
            run.bruteKind5PresenceEnabled = kind5Enabled;
            run.bruteKind5PresenceAppliedBaseline = query.TotalBruteKind5PresenceCollectionAppliedForDiagnostics;
            query.EnableBruteKind5PresenceForDiagnostics = kind5Enabled;
            run.bruteKind5PresenceFlagApplied = query.EnableBruteKind5PresenceForDiagnostics == kind5Enabled;
            Require(run.bruteKind5PresenceFlagApplied, "Combined kind5 configuration failed.");
        }

        private static void CompleteBruteCombinedCache(RunState run)
        {
            Require(kind5PresenceQuery != null && eligibilityReuseQuery == kind5PresenceQuery &&
                run.bruteEligibilityReuseEnabled && run.bruteEligibilityReuseFlagApplied &&
                kind5PresenceQuery.EnableBruteEligibilityReuseForDiagnostics &&
                run.bruteKind5PresenceFlagApplied &&
                kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics == run.bruteKind5PresenceEnabled &&
                ProductionDefaultsEnabled(kind5PresenceQuery) &&
                !kind5PresenceQuery.EnableBruteRejectedBindingReuseForDiagnostics &&
                !kind5PresenceQuery.EnableBruteBranchTimingForDiagnostics,
                "Combined comparison flags drifted during the fixed window.");
            run.bruteKind5PresenceAppliedDelta =
                kind5PresenceQuery.TotalBruteKind5PresenceCollectionAppliedForDiagnostics -
                run.bruteKind5PresenceAppliedBaseline;
            run.bruteKind5PresenceRestored = RestoreBruteKind5Presence();
            CompleteBruteEligibilityReuse(run);
        }

        private static void ApplyBruteEligibilityReuse(BruteForceSceneQuery query, RunState run, bool enabled)
        {
            Require(eligibilityReuseQuery == null && query != null && ProductionDefaultsEnabled(query) &&
                !query.EnableBruteEligibilityReuseForDiagnostics && !query.EnableBruteKind5PresenceForDiagnostics &&
                !query.EnableBruteRejectedBindingReuseForDiagnostics && !query.EnableBruteBranchTimingForDiagnostics,
                "Eligibility comparison requires the same four production defaults and no other candidate or timing flag.");
            eligibilityReuseQuery = query;
            previousEligibilityReuse = query.EnableBruteEligibilityReuseForDiagnostics;
            run.bruteEligibilityReuseEnabled = enabled;
            run.bruteEligibilityReuseAppliedBaseline = query.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics;
            query.EnableBruteEligibilityReuseForDiagnostics = enabled;
            run.bruteEligibilityReuseFlagApplied = query.EnableBruteEligibilityReuseForDiagnostics == enabled;
            Require(run.bruteEligibilityReuseFlagApplied, "Eligibility flag configuration failed.");
        }

        private static void CompleteBruteEligibilityReuse(RunState run)
        {
            Require(eligibilityReuseQuery != null && run.bruteEligibilityReuseFlagApplied &&
                eligibilityReuseQuery.EnableBruteEligibilityReuseForDiagnostics == run.bruteEligibilityReuseEnabled &&
                ProductionDefaultsEnabled(eligibilityReuseQuery) &&
                !eligibilityReuseQuery.EnableBruteKind5PresenceForDiagnostics &&
                !eligibilityReuseQuery.EnableBruteRejectedBindingReuseForDiagnostics &&
                !eligibilityReuseQuery.EnableBruteBranchTimingForDiagnostics,
                "Eligibility comparison flags drifted during the fixed window.");
            run.bruteEligibilityReuseAppliedDelta =
                eligibilityReuseQuery.TotalBruteEligibilityReuseCollectionAppliedForDiagnostics -
                run.bruteEligibilityReuseAppliedBaseline;
            run.bruteEligibilityReuseRestored = RestoreBruteEligibilityReuse();
        }

        private static bool RestoreBruteEligibilityReuse()
        {
            if (eligibilityReuseQuery == null)
                return true;
            eligibilityReuseQuery.EnableBruteEligibilityReuseForDiagnostics = previousEligibilityReuse;
            bool restored = eligibilityReuseQuery.EnableBruteEligibilityReuseForDiagnostics == previousEligibilityReuse;
            eligibilityReuseQuery = null;
            return restored;
        }

        private static bool RestoreBruteBranchTiming()
        {
            if (branchTimingQuery == null)
                return true;
            branchTimingQuery.EnableBruteBranchTimingForDiagnostics = previousBranchTiming;
            branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics = previousBranchTimingSampleStride;
            bool restored = branchTimingQuery.EnableBruteBranchTimingForDiagnostics == previousBranchTiming &&
                branchTimingQuery.BruteBranchTimingSampleStrideForDiagnostics == previousBranchTimingSampleStride;
            branchTimingQuery = null;
            return restored;
        }

        private static bool RestoreBruteKind5Presence()
        {
            if (kind5PresenceQuery == null)
                return true;
            kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics = previousKind5Presence;
            bool restored = kind5PresenceQuery.EnableBruteKind5PresenceForDiagnostics == previousKind5Presence;
            kind5PresenceQuery = null;
            return restored;
        }

        private static bool RestoreEmptyItrGuard()
        {
            if (emptyItrGuardQuery == null)
                return true;
            emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics = previousEmptyItrGuard;
            emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics = previousEmptyItrRoster;
            emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics = previousBruteExactCache;
            emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics = previousBruteGeometryFirst;
            bool restored = emptyItrGuardQuery.EnableEmptyItrPairGuardForDiagnostics == previousEmptyItrGuard &&
                emptyItrGuardQuery.EnableBruteEmptyItrRosterForDiagnostics == previousEmptyItrRoster &&
                emptyItrGuardQuery.EnableBruteExactCacheForDiagnostics == previousBruteExactCache &&
                emptyItrGuardQuery.EnableBruteGeometryFirstForDiagnostics == previousBruteGeometryFirst;
            emptyItrGuardQuery = null;
            return restored;
        }

        private static string HashFile(string path)
        {
            using (var hash = SHA256.Create())
            using (var input = File.OpenRead(PathFor(path)))
                return BitConverter.ToString(hash.ComputeHash(input)).Replace("-", string.Empty);
        }

        private static void SaveNew(string path, string content)
        {
            path = PathFor(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(content);
        }

        private static void Require(bool condition, string reason)
        {
            if (!condition)
                throw new InvalidOperationException(reason);
        }
    }

#if UNITY_INCLUDE_TESTS
    public sealed class BattleOptimizationWindowsAiSuiteRequestTests
    {
        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void RecordCost_RequestUsesOneFixedCombatFixture(int index)
        {
            var actual = BattleOptimizationWindowsAiSuiteEditor.BuildRoleRecordCostRequest(index);
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(3);
            NUnit.Framework.Assert.That(actual.action, NUnit.Framework.Is.EqualTo("combat1000"));
            NUnit.Framework.Assert.That(actual.formalCollectorMode, NUnit.Framework.Is.EqualTo("role"));
            NUnit.Framework.Assert.That(actual.warmupTicks, NUnit.Framework.Is.EqualTo(120));
            NUnit.Framework.Assert.That(actual.sampleTicks, NUnit.Framework.Is.EqualTo(180));
            NUnit.Framework.Assert.That(actual.seed, NUnit.Framework.Is.EqualTo(1314149188));
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain(index == 0 ? "00-combat1000-off" : "01-combat1000-on"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(2)]
        public void RecordCost_RequestRejectsExtraWindows(int index)
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleRecordCostRequest(index));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void NativeSampleWindow_RequestChangesOnlyOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildNativeSampledStateTimingRequest")
                .Invoke(null, new object[] { index });
            var expected = (ProductionEntityStressRequest)EligibilityMethod("BuildInputBoundaryTimingRequest")
                .Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH91-NATIVE-SAMPLED-STATE-WINDOWS-20261009/windows-01/" +
                index.ToString("D2") + "-" + expected.action + (index % 2 == 0 ? "-native-off" : "-native-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void NativeSampleWindow_RequestRejectsOutsideMatrix(int index)
        {
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                EligibilityMethod("BuildNativeSampledStateTimingRequest").Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void NativeSampleWindow_StateDefaultsFalseAndRoundTripsWithoutSignatureChange()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("SuiteState", BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(type, NUnit.Framework.Is.Not.Null);
            FieldInfo flag = type.GetField("nativeSampledStateTimingWindow", BindingFlags.Instance | BindingFlags.Public);
            NUnit.Framework.Assert.That(flag, NUnit.Framework.Is.Not.Null);
            object old = JsonUtility.FromJson("{}", type);
            NUnit.Framework.Assert.That(flag.GetValue(old), NUnit.Framework.Is.False);
            flag.SetValue(old, true);
            object restored = JsonUtility.FromJson(JsonUtility.ToJson(old), type);
            NUnit.Framework.Assert.That(flag.GetValue(restored), NUnit.Framework.Is.True);
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
        }

        [NUnit.Framework.Test]
        public void NativeSampleWindow_CurrentRequestRoutesToIndependentRoot()
        {
            FieldInfo stateField = CoarseEnvelopeStaticField("state");
            object previous = stateField.GetValue(null);
            object current = Activator.CreateInstance(stateField.FieldType, true);
            FieldInfo flag = current.GetType().GetField("nativeSampledStateTimingWindow", BindingFlags.Instance | BindingFlags.Public);
            NUnit.Framework.Assert.That(flag, NUnit.Framework.Is.Not.Null);
            flag.SetValue(current, true);
            try
            {
                stateField.SetValue(null, current);
                var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { 2 });
                var expected = (ProductionEntityStressRequest)EligibilityMethod("BuildNativeSampledStateTimingRequest")
                    .Invoke(null, new object[] { 2 });
                NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
            }
            finally
            {
                stateField.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void NativeSampleWindow_RejectedBeginRestoresSelectorWithoutStartingSuite(bool roleBusy)
        {
            FieldInfo native = CoarseEnvelopeStaticField("startingNativeSampledStateTimingWindow");
            FieldInfo input = CoarseEnvelopeStaticField("startingInputBoundaryTimingWindow");
            FieldInfo role = CoarseEnvelopeStaticField("startingRoleBranchTimingWindow");
            object previousNative = native.GetValue(null);
            object previousInput = input.GetValue(null);
            object previousRole = role.GetValue(null);
            try
            {
                native.SetValue(null, false);
                input.SetValue(null, !roleBusy);
                role.SetValue(null, roleBusy);
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("BeginNativeSampledStateTiming").Invoke(null, null)).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(native.GetValue(null), NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(input.GetValue(null), NUnit.Framework.Is.EqualTo(!roleBusy));
                NUnit.Framework.Assert.That(role.GetValue(null), NUnit.Framework.Is.EqualTo(roleBusy));
            }
            finally
            {
                native.SetValue(null, previousNative);
                input.SetValue(null, previousInput);
                role.SetValue(null, previousRole);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void RoleGroupWindow_RequestChangesOnlyCollectorAndOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildRoleGroupSnapshotSkipRequest")
                .Invoke(null, new object[] { index });
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.formalCollectorMode, NUnit.Framework.Is.EqualTo("role"));
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH93-ROLE-GROUP-SNAPSHOT-WINDOWS-20261009/windows-01/" +
                index.ToString("D2") + "-" + expected.action + (index % 2 == 0 ? "-group-off" : "-group-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            actual.formalCollectorMode = expected.formalCollectorMode;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void RoleGroupWindow_RequestRejectsOutsideMatrix(int index)
        {
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                EligibilityMethod("BuildRoleGroupSnapshotSkipRequest").Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void RoleGroupWindow_AppliesDetectsDriftAndRestoresOwner(int index)
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            object run = NewEligibilityRun();
            var restore = EligibilityMethod("RestoreRoleGroupSnapshotSkip");
            try
            {
                EligibilityMethod("ApplyRoleGroupSnapshotSkip").Invoke(null, new object[] { world, query, run, index });
                NUnit.Framework.Assert.That(query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics, NUnit.Framework.Is.EqualTo(index % 2 != 0));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "roleGroupSnapshotSkipFlagApplied"), NUnit.Framework.Is.True);
                query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics = index % 2 == 0;
                NUnit.Framework.Assert.That(EligibilityMethod("RoleGroupSnapshotSkipQueryValid")
                    .Invoke(null, new object[] { query, index }), NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("roleGroupSnapshotSkipWorld").GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("roleGroupSnapshotSkipQuery").GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void RoleGroupWindow_StructuralOwnerSurvivesStoppedRecording()
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            var restore = EligibilityMethod("RestoreRoleGroupSnapshotSkip");
            try
            {
                EligibilityMethod("ApplyRoleGroupSnapshotSkip").Invoke(null, new object[] { world, query, NewEligibilityRun(), 0 });
                world.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
                NUnit.Framework.Assert.That(EligibilityMethod("RoleGroupSnapshotSkipQueryValid")
                    .Invoke(null, new object[] { query, 0 }), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(true)]
        [NUnit.Framework.TestCase(false)]
        public void RoleGroupWindow_LiveObservationRequiresRecording(bool recording)
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            var queryField = CoarseEnvelopeStaticField("productionQuery");
            object previous = queryField.GetValue(null);
            var restore = EligibilityMethod("RestoreRoleGroupSnapshotSkip");
            try
            {
                object run = NewEligibilityRun();
                EligibilityMethod("ApplyRoleGroupSnapshotSkip").Invoke(null, new object[] { world, query, run, 1 });
                queryField.SetValue(null, query);
                typeof(BruteForceSceneQuery).GetProperty("LastRoleRejectedGroupSnapshotSkipCountForDiagnostics")
                    .SetValue(query, 5L);
                if (!recording)
                    world.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
                EligibilityMethod("ObserveRoleGroupSnapshotSkip").Invoke(null, new object[] { run, 1 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "roleGroupSnapshotSkipUnchanged"), NUnit.Framework.Is.EqualTo(recording));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "roleGroupSnapshotSkipObservedCollections"), NUnit.Framework.Is.EqualTo(recording ? 1 : 0));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "roleGroupSnapshotSkipMaximumObservedRejects"), NUnit.Framework.Is.EqualTo(recording ? 5L : 0L));
            }
            finally
            {
                restore.Invoke(null, null);
                queryField.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        public void RoleGroupWindow_InvalidOwnerRejectedBeforeMutation(int defect)
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = defect == 1 ? CollisionFormalCollectorMode.ForceBruteForce : CollisionFormalCollectorMode.ForceRoleAware;
            if (defect != 3)
                world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            var apply = EligibilityMethod("ApplyRoleGroupSnapshotSkip");
            var restore = EligibilityMethod("RestoreRoleGroupSnapshotSkip");
            try
            {
                if (defect == 4)
                    apply.Invoke(null, new object[] { world, query, NewEligibilityRun(), 0 });
                bool before = query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics;
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { defect == 0 ? null : defect == 2 ? new SimulationWorld() : world,
                        query, NewEligibilityRun(), 1 })).InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics, NUnit.Framework.Is.EqualTo(before));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void RoleGroupWindow_SessionIsBackwardCompatible()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("SuiteState", BindingFlags.NonPublic);
            FieldInfo field = type.GetField("roleGroupSnapshotSkipWindow");
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            object session = JsonUtility.FromJson("{}", type);
            NUnit.Framework.Assert.That(field.GetValue(session), NUnit.Framework.Is.False);
            field.SetValue(session, true);
            NUnit.Framework.Assert.That(field.GetValue(JsonUtility.FromJson(JsonUtility.ToJson(session), type)), NUnit.Framework.Is.True);
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
        }

        [NUnit.Framework.TestCase(false, 0L, true, true)]
        [NUnit.Framework.TestCase(true, 5L, true, true)]
        [NUnit.Framework.TestCase(true, 0L, true, false)]
        [NUnit.Framework.TestCase(true, 5L, false, false)]
        public void RoleGroupWindow_ObservationRequiresStableOwnerAndActualSkip(bool enabled, long skips, bool valid, bool expected)
        {
            object run = NewEligibilityRun();
            foreach (string name in new[] { "roleGroupSnapshotSkipFlagApplied", "roleGroupSnapshotSkipRestored" })
            {
                FieldInfo field = run.GetType().GetField(name);
                NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
                field.SetValue(run, true);
            }
            run.GetType().GetField("roleGroupSnapshotSkipEnabled").SetValue(run, enabled);
            run.GetType().GetField("roleGroupSnapshotSkipUnchanged").SetValue(run, valid);
            run.GetType().GetField("roleGroupSnapshotSkipObservedCollections").SetValue(run, 1);
            run.GetType().GetField("roleGroupSnapshotSkipMaximumObservedRejects").SetValue(run, skips);
            NUnit.Framework.Assert.That(EligibilityMethod("RoleGroupSnapshotSkipObservationValid").Invoke(null, new[] { run }),
                NUnit.Framework.Is.EqualTo(expected));
        }

        [NUnit.Framework.Test]
        public void RoleGroupWindow_CurrentRequestRoutesToIndependentRoot()
        {
            FieldInfo stateField = CoarseEnvelopeStaticField("state");
            object previous = stateField.GetValue(null);
            object current = Activator.CreateInstance(stateField.FieldType, true);
            FieldInfo flag = current.GetType().GetField("roleGroupSnapshotSkipWindow");
            NUnit.Framework.Assert.That(flag, NUnit.Framework.Is.Not.Null);
            flag.SetValue(current, true);
            try
            {
                stateField.SetValue(null, current);
                var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { 2 });
                var expected = (ProductionEntityStressRequest)EligibilityMethod("BuildRoleGroupSnapshotSkipRequest").Invoke(null, new object[] { 2 });
                NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
            }
            finally { stateField.SetValue(null, previous); }
        }

        [NUnit.Framework.TestCase(true)]
        [NUnit.Framework.TestCase(false)]
        public void RoleGroupWindow_RejectedBeginDoesNotChangeExistingOwner(bool producerBusy)
        {
            var group = CoarseEnvelopeStaticField("startingRoleGroupSnapshotSkipWindow");
            var producer = CoarseEnvelopeStaticField("startingProducerSnapshotReuseWindow");
            var input = CoarseEnvelopeStaticField("startingInputBoundaryTimingWindow");
            object previousGroup = group.GetValue(null);
            object previousProducer = producer.GetValue(null);
            object previousInput = input.GetValue(null);
            try
            {
                group.SetValue(null, false);
                producer.SetValue(null, producerBusy);
                input.SetValue(null, !producerBusy);
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("BeginRoleGroupSnapshotSkip").Invoke(null, null)).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(group.GetValue(null), NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(producer.GetValue(null), NUnit.Framework.Is.EqualTo(producerBusy));
                NUnit.Framework.Assert.That(input.GetValue(null), NUnit.Framework.Is.EqualTo(!producerBusy));
            }
            finally
            {
                group.SetValue(null, previousGroup);
                producer.SetValue(null, previousProducer);
                input.SetValue(null, previousInput);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void ProducerSnapshotOwner_RequestChangesOnlyCollectorAndOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildProducerSnapshotReuseRequest")
                .Invoke(null, new object[] { index });
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.formalCollectorMode, NUnit.Framework.Is.EqualTo("role"));
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH88-PRODUCER-SNAPSHOT-WINDOWS-20261009/windows-02/" +
                index.ToString("D2") + "-" + expected.action + (index % 2 == 0 ? "-producer-off" : "-producer-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            actual.formalCollectorMode = expected.formalCollectorMode;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void ProducerSnapshotOwner_RequestRejectsOutsideMatrix(int index)
        {
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                EligibilityMethod("BuildProducerSnapshotReuseRequest").Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.TestCase(0, false)]
        [NUnit.Framework.TestCase(0, true)]
        [NUnit.Framework.TestCase(1, false)]
        [NUnit.Framework.TestCase(1, true)]
        public void ProducerSnapshotOwner_AppliesRestoresBothFlagsAndReleasesWorld(int index, bool prior)
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            world.EnableCharacterInputProducerSnapshotReuseForDiagnostics = prior;
            world.EnableCharacterInputBoundaryTimingForDiagnostics = !prior;
            object run = NewEligibilityRun();
            var restore = EligibilityMethod("RestoreProducerSnapshotReuse");
            try
            {
                EligibilityMethod("ApplyProducerSnapshotReuse").Invoke(null, new object[] { world, query, run, index });
                NUnit.Framework.Assert.That(world.EnableCharacterInputProducerSnapshotReuseForDiagnostics,
                    NUnit.Framework.Is.EqualTo(index % 2 != 0));
                NUnit.Framework.Assert.That(world.EnableCharacterInputBoundaryTimingForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "producerSnapshotReuseFlagApplied"), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, NUnit.Framework.Is.EqualTo(prior));
                NUnit.Framework.Assert.That(world.EnableCharacterInputBoundaryTimingForDiagnostics, NUnit.Framework.Is.EqualTo(!prior));
                NUnit.Framework.Assert.That(typeof(BattleOptimizationWindowsAiSuiteEditor).GetField(
                    "producerSnapshotReuseWorld", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void ProducerSnapshotOwner_StructuralOwnerSurvivesStoppedRecording()
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            var restore = EligibilityMethod("RestoreProducerSnapshotReuse");
            try
            {
                object run = NewEligibilityRun();
                EligibilityMethod("ApplyProducerSnapshotReuse").Invoke(null, new object[] { world, query, run, 0 });
                world.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
                NUnit.Framework.Assert.That(EligibilityMethod("ProducerSnapshotReuseQueryValid").Invoke(null,
                    new object[] { query, 0 }), NUnit.Framework.Is.True,
                    "normal runner teardown stops recording before the structural owner is restored");
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(true)]
        [NUnit.Framework.TestCase(false)]
        public void ProducerSnapshotOwner_LiveObservationRequiresRecording(bool recording)
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            var queryField = typeof(BattleOptimizationWindowsAiSuiteEditor).GetField("productionQuery", BindingFlags.NonPublic | BindingFlags.Static);
            object previousQuery = queryField.GetValue(null);
            var restore = EligibilityMethod("RestoreProducerSnapshotReuse");
            try
            {
                object run = NewEligibilityRun();
                EligibilityMethod("ApplyProducerSnapshotReuse").Invoke(null, new object[] { world, query, run, 0 });
                queryField.SetValue(null, query);
                if (!recording)
                    world.DisableBattleAiInputDetailDiagnosticsForDiagnostics();
                EligibilityMethod("ObserveProducerSnapshotReuse").Invoke(null, new object[] { run, 0 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "producerSnapshotReuseUnchanged"), NUnit.Framework.Is.EqualTo(recording));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "producerSnapshotReuseObservedPasses"), NUnit.Framework.Is.EqualTo(recording ? 1 : 0));
            }
            finally
            {
                restore.Invoke(null, null);
                queryField.SetValue(null, previousQuery);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        public void ProducerSnapshotOwner_InvalidOwnerRejectedBeforeMutation(int defect)
        {
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = defect == 1 ? CollisionFormalCollectorMode.ForceBruteForce : CollisionFormalCollectorMode.ForceRoleAware;
            if (defect != 3)
                world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            var apply = EligibilityMethod("ApplyProducerSnapshotReuse");
            var restore = EligibilityMethod("RestoreProducerSnapshotReuse");
            try
            {
                if (defect == 4)
                    apply.Invoke(null, new object[] { world, query, NewEligibilityRun(), 0 });
                bool before = world.EnableCharacterInputProducerSnapshotReuseForDiagnostics;
                bool timingBefore = world.EnableCharacterInputBoundaryTimingForDiagnostics;
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { defect == 0 ? null : defect == 2 ? new SimulationWorld() : world,
                        query, NewEligibilityRun(), 1 })).InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(world.EnableCharacterInputProducerSnapshotReuseForDiagnostics, NUnit.Framework.Is.EqualTo(before));
                NUnit.Framework.Assert.That(world.EnableCharacterInputBoundaryTimingForDiagnostics, NUnit.Framework.Is.EqualTo(timingBefore));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void ProducerSnapshotOwner_SessionIsBackwardCompatible()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("SuiteState", BindingFlags.NonPublic);
            FieldInfo field = type.GetField("producerSnapshotReuseWindow");
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            object session = JsonUtility.FromJson("{}", type);
            NUnit.Framework.Assert.That(field.GetValue(session), NUnit.Framework.Is.False);
            field.SetValue(session, true);
            NUnit.Framework.Assert.That(field.GetValue(JsonUtility.FromJson(JsonUtility.ToJson(session), type)), NUnit.Framework.Is.True);
        }

        [NUnit.Framework.TestCase(false, true, true)]
        [NUnit.Framework.TestCase(true, true, true)]
        [NUnit.Framework.TestCase(false, false, false)]
        [NUnit.Framework.TestCase(true, false, false)]
        public void ProducerSnapshotOwner_ObservationRequiresStableOwnerAndActualSkip(bool enabled, bool valid, bool expected)
        {
            object run = NewEligibilityRun();
            foreach (string name in new[] { "producerSnapshotReuseFlagApplied", "producerSnapshotReuseRestored",
                "inputBoundaryTimingFlagApplied", "inputBoundaryTimingUnchanged", "inputBoundaryTimingRestored",
                "inputBoundaryTimingCountsAvailable", "inputBoundaryTimingEnabled" })
            {
                FieldInfo field = run.GetType().GetField(name);
                NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
                field.SetValue(run, true);
            }
            run.GetType().GetField("producerSnapshotReuseEnabled").SetValue(run, enabled);
            run.GetType().GetField("producerSnapshotReuseUnchanged").SetValue(run, valid);
            run.GetType().GetField("producerSnapshotReuseObservedPasses").SetValue(run, 1);
            run.GetType().GetField("producerSnapshotReuseMaximumObservedSkips").SetValue(run, enabled ? 1000 : 0);
            run.GetType().GetField("inputBoundaryTimingPhaseCalls").SetValue(run, new long[] { 1000, 1000, 1000, 1000 });
            NUnit.Framework.Assert.That(EligibilityMethod("ProducerSnapshotReuseObservationValid").Invoke(null, new[] { run }),
                NUnit.Framework.Is.EqualTo(expected));
        }


        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void InputBoundaryOwner_RequestChangesOnlyCollectorAndOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildInputBoundaryTimingRequest").Invoke(null, new object[] { index });
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.formalCollectorMode, NUnit.Framework.Is.EqualTo("role"));
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH85-INPUT-BOUNDARY-TIMING-20261009/windows-01/" +
                index.ToString("D2") + "-" + expected.action + (index % 2 == 0 ? "-boundary-off" : "-boundary-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            actual.formalCollectorMode = expected.formalCollectorMode;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void InputBoundaryOwner_RequestRejectsOutsideMatrix(int index)
        {
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                EligibilityMethod("BuildInputBoundaryTimingRequest").Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.TestCase(0, false)]
        [NUnit.Framework.TestCase(0, true)]
        [NUnit.Framework.TestCase(1, false)]
        [NUnit.Framework.TestCase(1, true)]
        public void InputBoundaryOwner_AppliesRestoresAndReleasesWorld(int index, bool prior)
        {
            var property = typeof(SimulationWorld).GetProperty("EnableCharacterInputBoundaryTimingForDiagnostics");
            NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            property.SetValue(world, prior);
            object run = NewEligibilityRun();
            var restore = EligibilityMethod("RestoreInputBoundaryTiming");
            try
            {
                EligibilityMethod("ApplyInputBoundaryTiming").Invoke(null, new object[] { world, query, run, index });
                NUnit.Framework.Assert.That(property.GetValue(world), NUnit.Framework.Is.EqualTo(index % 2 != 0));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "inputBoundaryTimingFlagApplied"), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(property.GetValue(world), NUnit.Framework.Is.EqualTo(prior));
                NUnit.Framework.Assert.That(typeof(BattleOptimizationWindowsAiSuiteEditor).GetField(
                    "inputBoundaryTimingWorld", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        public void InputBoundaryOwner_InvalidOwnerRejectedBeforeMutation(int defect)
        {
            var property = typeof(SimulationWorld).GetProperty("EnableCharacterInputBoundaryTimingForDiagnostics");
            NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null);
            var world = new SimulationWorld();
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = defect == 1 ? CollisionFormalCollectorMode.ForceBruteForce : CollisionFormalCollectorMode.ForceRoleAware;
            if (defect != 3)
                world.EnableBattleAiInputDetailDiagnosticsForDiagnostics();
            var apply = EligibilityMethod("ApplyInputBoundaryTiming");
            var restore = EligibilityMethod("RestoreInputBoundaryTiming");
            try
            {
                if (defect == 4)
                    apply.Invoke(null, new object[] { world, query, NewEligibilityRun(), 0 });
                bool before = (bool)property.GetValue(world);
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { defect == 0 ? null : defect == 2 ? new SimulationWorld() : world,
                        query, NewEligibilityRun(), 1 })).InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(property.GetValue(world), NUnit.Framework.Is.EqualTo(before));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void InputBoundaryOwner_SessionIsBackwardCompatible()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("SuiteState", BindingFlags.NonPublic);
            FieldInfo field = type.GetField("inputBoundaryTimingWindow");
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            object state = JsonUtility.FromJson("{}", type);
            NUnit.Framework.Assert.That(field.GetValue(state), NUnit.Framework.Is.False);
            field.SetValue(state, true);
            NUnit.Framework.Assert.That(field.GetValue(JsonUtility.FromJson(JsonUtility.ToJson(state), type)), NUnit.Framework.Is.True);
        }

        [NUnit.Framework.TestCase(false, true, true)]
        [NUnit.Framework.TestCase(true, true, true)]
        [NUnit.Framework.TestCase(false, false, false)]
        [NUnit.Framework.TestCase(true, false, false)]
        public void InputBoundaryOwner_ObservationRequiresStableOwner(bool enabled, bool valid, bool expected)
        {
            object run = NewEligibilityRun();
            foreach (string name in new[] { "inputBoundaryTimingFlagApplied", "inputBoundaryTimingRestored" })
            {
                FieldInfo field = run.GetType().GetField(name);
                NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
                field.SetValue(run, true);
            }
            run.GetType().GetField("inputBoundaryTimingEnabled").SetValue(run, enabled);
            run.GetType().GetField("inputBoundaryTimingUnchanged").SetValue(run, valid);
            run.GetType().GetField("inputBoundaryTimingCountsAvailable").SetValue(run, true);
            run.GetType().GetField("inputBoundaryTimingPhaseCalls").SetValue(run,
                enabled ? new long[] { 1000, 1000, 1000, 1000 } : new long[4]);
            NUnit.Framework.Assert.That(EligibilityMethod("InputBoundaryTimingObservationValid").Invoke(null, new[] { run }),
                NUnit.Framework.Is.EqualTo(expected));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void RoleBranchOwner_RequestChangesOnlyCollectorAndOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildRoleBranchTimingRequest")
                .Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.formalCollectorMode, NUnit.Framework.Is.EqualTo("role"));
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH83-ROLE-EXACT-BRANCH-TIMING-20261009/windows-01/" +
                index.ToString("D2") + "-" + expected.action + (index % 2 == 0 ? "-role-timing-off" : "-role-timing-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            actual.formalCollectorMode = expected.formalCollectorMode;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void RoleBranchOwner_RequestRejectsOutsideMatrix(int index)
        {
            MethodInfo method = EligibilityMethod("BuildRoleBranchTimingRequest");
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.TestCase(0, false)]
        [NUnit.Framework.TestCase(0, true)]
        [NUnit.Framework.TestCase(1, false)]
        [NUnit.Framework.TestCase(1, true)]
        public void RoleBranchOwner_AppliesRestoresPriorValuesAndReleasesReference(int index, bool previous)
        {
            MethodInfo apply = EligibilityMethod("ApplyRoleBranchTiming");
            MethodInfo restore = EligibilityMethod("RestoreRoleBranchTiming");
            FieldInfo owned = typeof(BattleOptimizationWindowsAiSuiteEditor).GetField(
                "roleBranchTimingQuery", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(owned, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(owned.GetValue(null), NUnit.Framework.Is.Null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            query.EnableRoleBranchTimingForDiagnostics = previous;
            query.RoleBranchTimingSampleStrideForDiagnostics = 8;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, index });
                NUnit.Framework.Assert.That(query.EnableRoleBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(index % 2 != 0));
                NUnit.Framework.Assert.That(query.RoleBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(64));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "roleBranchTimingFlagApplied"), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableRoleBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(query.RoleBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(8));
                NUnit.Framework.Assert.That(owned.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void RoleBranchOwner_InvalidOwnerRejectedBeforeFlagMutation(int defect)
        {
            MethodInfo apply = EligibilityMethod("ApplyRoleBranchTiming");
            MethodInfo restore = EligibilityMethod("RestoreRoleBranchTiming");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.FormalCollectorMode = defect == 1 ? CollisionFormalCollectorMode.ForceBruteForce : CollisionFormalCollectorMode.ForceRoleAware;
            query.EnableBruteBranchTimingForDiagnostics = defect == 2;
            query.RoleBranchTimingSampleStrideForDiagnostics = 8;
            object run = NewEligibilityRun();
            try
            {
                if (defect == 3)
                    apply.Invoke(null, new object[] { query, run, 0 });
                bool prior = query.EnableRoleBranchTimingForDiagnostics;
                int stride = query.RoleBranchTimingSampleStrideForDiagnostics;
                Exception error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { defect == 0 ? null : query, run, 1 })).InnerException;
                NUnit.Framework.Assert.That(error, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableRoleBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(prior));
                NUnit.Framework.Assert.That(query.RoleBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(stride));
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.Test]
        public void RoleBranchOwner_SessionIsBackwardCompatible()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("SuiteState", BindingFlags.NonPublic);
            FieldInfo field = type.GetField("roleBranchTimingWindow");
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            object old = JsonUtility.FromJson("{}", type);
            NUnit.Framework.Assert.That(field.GetValue(old), NUnit.Framework.Is.False);
            field.SetValue(old, true);
            NUnit.Framework.Assert.That(field.GetValue(JsonUtility.FromJson(JsonUtility.ToJson(old), type)), NUnit.Framework.Is.True);
        }

        [NUnit.Framework.TestCase(false, true, true)]
        [NUnit.Framework.TestCase(true, true, true)]
        [NUnit.Framework.TestCase(true, false, false)]
        [NUnit.Framework.TestCase(false, false, false)]
        public void RoleBranchOwner_ObservationRequiresStableOwnerAndCoverage(bool enabled, bool valid, bool expected)
        {
            object run = NewEligibilityRun();
            Type type = run.GetType();
            FieldInfo applied = type.GetField("roleBranchTimingFlagApplied");
            NUnit.Framework.Assert.That(applied, NUnit.Framework.Is.Not.Null);
            applied.SetValue(run, true);
            type.GetField("roleBranchTimingEnabled").SetValue(run, enabled);
            type.GetField("roleBranchTimingRestored").SetValue(run, true);
            type.GetField("roleBranchTimingUnchanged").SetValue(run, enabled || valid);
            var coverage = new BruteForceSceneQuery.RoleBranchTimingCoverage();
            coverage.eligibleDirections = enabled && valid ? 64 : 0;
            coverage.timedDirections = enabled && valid ? 1 : 0;
            type.GetField("roleBranchTimingCoverage").SetValue(run, coverage);
            NUnit.Framework.Assert.That(EligibilityMethod("RoleBranchTimingObservationValid").Invoke(null, new[] { run }),
                NUnit.Framework.Is.EqualTo(expected));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void PostBindingsRole_RequestChangesOnlyCollectorAndOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildPostBindingsRoleRequest")
                .Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.formalCollectorMode,
                NUnit.Framework.Is.EqualTo(index % 2 == 0 ? "brute" : "role"));
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH81-POST-BINDINGS-ROLE-WINDOWS-20261009/windows-01/" +
                index.ToString("D2") + "-" + expected.action + (index % 2 == 0 ? "-brute" : "-role") + "/report.json"));
            actual.outputPath = expected.outputPath;
            actual.formalCollectorMode = expected.formalCollectorMode;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void PostBindingsRole_OutsideFrozenMatrix(int index)
        {
            MethodInfo method = EligibilityMethod("BuildPostBindingsRoleRequest");
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void PostBindingsRole_SessionFieldIsBackwardCompatible()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("SuiteState", BindingFlags.NonPublic);
            FieldInfo field = type.GetField("postBindingsRoleWindow");
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            object old = JsonUtility.FromJson("{}", type);
            NUnit.Framework.Assert.That(field.GetValue(old), NUnit.Framework.Is.EqualTo(false));
            field.SetValue(old, true);
            NUnit.Framework.Assert.That(field.GetValue(JsonUtility.FromJson(JsonUtility.ToJson(old), type)), NUnit.Framework.Is.EqualTo(true));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        public void PostBindingsRole_InvalidColdModeFailsBeforeSideEffects(int defect)
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor);
            FieldInfo starting = type.GetField("startingPostBindingsRoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(starting, NUnit.Framework.Is.Not.Null);
            FieldInfo depth = type.GetField("startingDepthRejectWindow", BindingFlags.Static | BindingFlags.NonPublic);
            PropertyInfo binding = typeof(CharacterAnimtorManager).GetProperty("MatchCentralBindingsEnabledForDiagnostics",
                BindingFlags.Static | BindingFlags.NonPublic);
            bool priorStarting = (bool)starting.GetValue(null);
            bool priorDepth = (bool)depth.GetValue(null);
            bool priorBinding = (bool)binding.GetValue(null);
            MethodInfo method = EligibilityMethod("BeginSuite");
            object[] arguments = new object[17];
            ParameterInfo[] parameters = method.GetParameters();
            for (int i = 0; i < arguments.Length; i++)
                arguments[i] = parameters[i].Name == "logicGcScopeOnly" && defect != 0 ||
                    parameters[i].Name == "roleCandidate" && defect == 1 ||
                    parameters[i].Name == "bruteProductionOnly" && defect == 2;
            try
            {
                starting.SetValue(null, true);
                depth.SetValue(null, defect == 3);
                binding.SetValue(null, defect != 4);
                Exception error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() => method.Invoke(null, arguments)).InnerException;
                NUnit.Framework.Assert.That(error, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(error.Message, NUnit.Framework.Does.Contain("post-bindings role"));
            }
            finally
            {
                starting.SetValue(null, priorStarting);
                depth.SetValue(null, priorDepth);
                binding.SetValue(null, priorBinding);
            }
        }

        [NUnit.Framework.TestCase("valid", true)]
        [NUnit.Framework.TestCase("null", false)]
        [NUnit.Framework.TestCase("unconfigured", false)]
        [NUnit.Framework.TestCase("drift", false)]
        [NUnit.Framework.TestCase("unobserved", false)]
        [NUnit.Framework.TestCase("mismatch", false)]
        public void PostBindingsRole_ObservationRequiresActualRouteAndStableConfiguration(string defect, bool expected)
        {
            object run = defect == "null" ? null : NewEligibilityRun();
            if (run != null)
            {
                Type type = run.GetType();
                FieldInfo configured = type.GetField("postBindingsRoleConfigured");
                NUnit.Framework.Assert.That(configured, NUnit.Framework.Is.Not.Null);
                configured.SetValue(run, defect != "unconfigured");
                type.GetField("postBindingsRoleUnchanged").SetValue(run, defect != "drift");
                type.GetField("postBindingsRoleObservedCollections").SetValue(run, defect == "unobserved" ? 0 : 4);
                type.GetField("postBindingsRoleActualMismatch").SetValue(run, defect == "mismatch");
            }
            NUnit.Framework.Assert.That(EligibilityMethod("PostBindingsRoleObservationValid").Invoke(null, new[] { run }),
                NUnit.Framework.Is.EqualTo(expected));
        }


        [NUnit.Framework.TestCase(null)]
        [NUnit.Framework.TestCase("")]
        public void CentralSegmentReadback_ErrorSessionRoundTrip(string error)
        {
            object run = NewEligibilityRun();
            run.GetType().GetField("centralSegmentReadbackError").SetValue(run, error);
            object restored = JsonUtility.FromJson(JsonUtility.ToJson(run), run.GetType());
            NUnit.Framework.Assert.That(EligibilityMethod("CentralSegmentReadbackPending")
                .Invoke(null, new[] { restored }), NUnit.Framework.Is.EqualTo(true));
        }

        [NUnit.Framework.Test]
        public void CentralSegmentReadback_RequestOnlyOutput()
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildCentralSegmentReadbackRequest")
                .Invoke(null, new object[] { 0 });
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(1);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH75-CENTRAL-SEGMENT-READBACK-20261008/windows-02/00-combat1000-readback/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(1)]
        public void CentralSegmentReadback_OutsideMatrix(int index)
        {
            MethodInfo method = EligibilityMethod("BuildCentralSegmentReadbackRequest");
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.TestCase("strict", 1)]
        [NUnit.Framework.TestCase("chunk", 2)]
        [NUnit.Framework.TestCase("texture", 4)]
        [NUnit.Framework.TestCase("material", 8)]
        [NUnit.Framework.TestCase("variant", 16)]
        [NUnit.Framework.TestCase("binding", 32)]
        [NUnit.Framework.TestCase("page", 64)]
        [NUnit.Framework.TestCase("slice", 128)]
        [NUnit.Framework.TestCase("commandGap", 256)]
        [NUnit.Framework.TestCase("quadGap", 512)]
        [NUnit.Framework.TestCase("equal", 0)]
        public void CentralSegmentReadback_BreakMask(string change, int expected)
        {
            MethodInfo method = EligibilityMethod("CentralSegmentBreakMask");
            Shader shader = Shader.Find("NTSD/BattleCentralTransparent");
            NUnit.Framework.Assert.That(shader, NUnit.Framework.Is.Not.Null);
            Texture2D firstTexture = new Texture2D(1, 1);
            Texture2D otherTexture = new Texture2D(1, 1);
            Material firstMaterial = new Material(shader);
            Material otherMaterial = new Material(shader);
            try
            {
                var previous = new BattleCentralRenderSegment(0, 0, 0, 1, 0, 1, firstTexture,
                    firstMaterial, 2, 0, BattleSpriteCentralBindingMode.AtlasPageTexture2D, 0);
                var next = new BattleCentralRenderSegment(change == "chunk" ? 1 : 0, 1,
                    change == "commandGap" ? 2 : 1, 1, change == "quadGap" ? 2 : 1, 1,
                    change == "texture" ? otherTexture : firstTexture,
                    change == "material" ? otherMaterial : firstMaterial, change == "variant" ? 3 : 2,
                    change == "slice" ? 1 : 0,
                    change == "binding" ? BattleSpriteCentralBindingMode.SourceTexture2D :
                        BattleSpriteCentralBindingMode.AtlasPageTexture2D, change == "page" ? 1 : 0);
                int actual = (int)method.Invoke(null, new object[] { previous, next,
                    change == "strict" ? BattleCentralDrawMode.StrictOrderedDraw : BattleCentralDrawMode.OrderedChunks });
                NUnit.Framework.Assert.That(actual, NUnit.Framework.Is.EqualTo(expected));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(firstMaterial);
                UnityEngine.Object.DestroyImmediate(otherMaterial);
                UnityEngine.Object.DestroyImmediate(firstTexture);
                UnityEngine.Object.DestroyImmediate(otherTexture);
            }
        }

        [NUnit.Framework.Test]
        public void CentralSegmentReadback_ArraySliceIsNotArrayIdentity()
        {
            MethodInfo method = EligibilityMethod("CentralSegmentBreakMask");
            var previous = new BattleCentralRenderSegment(0, 0, 0, 1, 0, 1, null,
                null, 0, 2, BattleSpriteCentralBindingMode.AtlasTextureArray);
            var next = new BattleCentralRenderSegment(0, 1, 1, 1, 1, 1, null,
                null, 0, 7, BattleSpriteCentralBindingMode.AtlasTextureArray);
            NUnit.Framework.Assert.That(method.Invoke(null, new object[] { previous, next,
                BattleCentralDrawMode.OrderedChunks }), NUnit.Framework.Is.EqualTo(0));
        }

        [NUnit.Framework.Test]
        public void CentralSegmentReadback_SourcePageIndexIsNotBoundPageIdentity()
        {
            MethodInfo method = EligibilityMethod("CentralSegmentBreakMask");
            var previous = new BattleCentralRenderSegment(0, 0, 0, 1, 0, 1, null,
                null, 0, 0, BattleSpriteCentralBindingMode.SourceTexture2D, 1);
            var next = new BattleCentralRenderSegment(0, 1, 1, 1, 1, 1, null,
                null, 0, 0, BattleSpriteCentralBindingMode.SourceTexture2D, 8);
            NUnit.Framework.Assert.That(method.Invoke(null, new object[] { previous, next,
                BattleCentralDrawMode.OrderedChunks }), NUnit.Framework.Is.EqualTo(0));
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void CentralSegmentReadback_DetachIdempotent(bool ownsRun)
        {
            MethodInfo restore = EligibilityMethod("RestoreCentralSegmentReadback");
            FieldInfo owner = CoarseEnvelopeStaticField("centralSegmentReadbackRun");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object run = ownsRun ? NewEligibilityRun() : null;
            try
            {
                owner.SetValue(null, run);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                if (run != null)
                    NUnit.Framework.Assert.That(EligibilityRunField(run, "centralSegmentReadbackDetached"),
                        NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteOrdinalPacketWindow_RequestOnlyOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildBruteOrdinalPacketRequest")
                .Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH72-ORDINAL-PACKET-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-packet-off" : "-packet-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void BruteOrdinalPacketWindow_OutsideMatrix(int index)
        {
            MethodInfo method = EligibilityMethod("BuildBruteOrdinalPacketRequest");
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                method.Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_MenuKeepsLegacy17AndColdOwner()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteOrdinalPacket");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(attributes.Length, NUnit.Framework.Is.EqualTo(1));
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch72 Ordinal Packet Full Driver GC 1000 AI OFF ON"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingOrdinalPacketWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.Null);
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_StateDefaultsFalseAndRoutesFourBeforeGc()
        {
            EligibilityMethod("BuildBruteOrdinalPacketRequest");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            Type type = owner.FieldType;
            FieldInfo mode = type.GetField("bruteOrdinalPacketCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", type)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(type, true);
            mode.SetValue(candidate, true);
            type.GetField("bruteProductionOnly").SetValue(candidate, true);
            type.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteOrdinalPacketRequest").Invoke(null, new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteOrdinalPacketWindow_ModeRejectsBeforeState(int defect)
        {
            EligibilityMethod("BeginBruteOrdinalPacket");
            FieldInfo startup = CoarseEnvelopeStaticField("startingOrdinalPacketWindow");
            FieldInfo oldStartup = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object previous = startup.GetValue(null);
            object previousOld = oldStartup.GetValue(null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
                arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                    parameters[index].Name == "bruteBranchTimingOnly" && defect == 2;
            try
            {
                startup.SetValue(null, true);
                oldStartup.SetValue(null, defect == 3);
                var error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() => begin.Invoke(null, arguments));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(error.InnerException.Message, NUnit.Framework.Does.Contain("ordinal packet window"));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                startup.SetValue(null, previous);
                oldStartup.SetValue(null, previousOld);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteOrdinalPacketWindow_ApplyObserveCompleteRestores(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.BruteOrdinalPacketCapacityForDiagnostics, NUnit.Framework.Is.EqualTo(0),
                    "Population has not completed; zero cold capacity is legal before the allocation seal.");
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, enabled, false, enabled ? 11L : 0L, enabled ? 5L : 0L);
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new object[] { run });
                NUnit.Framework.Assert.That(EligibilityMethod("OrdinalPacketObservationValid").Invoke(null, new[] { run }),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketCapacityBeforePopulation"),
                    NUnit.Framework.Is.EqualTo(0));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketSteadyCapacity"),
                    NUnit.Framework.Is.EqualTo(2));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketQuery").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_RejectSecondOwnerKeepsFirst()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object first = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, first, true });
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { query, NewEligibilityRun(), false })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.SameAs(first));
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.True);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        [NUnit.Framework.TestCase(5)]
        public void BruteOrdinalPacketWindow_RejectQueryScope(int defect)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            if (defect == 0) query.EnableBruteOrdinalPacketForDiagnostics = true;
            if (defect == 1) query.EnableBruteGeometryFirstForDiagnostics = false;
            if (defect == 2) query.EnableBruteKind5PresenceForDiagnostics = true;
            if (defect == 3) query.EnableBruteCoarseEnvelopeForDiagnostics = true;
            if (defect == 4) query.EnableBruteBranchTimingForDiagnostics = true;
            if (defect == 5) query.EnableBruteCoarseProofReuseForDiagnostics = true;
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true })).InnerException,
                NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.EqualTo(defect == 0));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteOrdinalPacketWindow_DriftLatchesUntilComplete(int defect)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, true, false, 11L, 5L);
                if (defect == 0) query.EnableBruteOrdinalPacketForDiagnostics = false;
                else query.EnableBruteGeometryFirstForDiagnostics = false;
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                query.EnableBruteOrdinalPacketForDiagnostics = true;
                query.EnableBruteGeometryFirstForDiagnostics = true;
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketFlagUnchanged"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new[] { run })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketRestored"), NUnit.Framework.Is.EqualTo(true));
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_ObservationDedupsFreshTick()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, true, false, 11L, 5L);
                MethodInfo observe = EligibilityMethod("ObserveBruteOrdinalPacket");
                observe.Invoke(null, new object[] { run, 121 });
                SetOrdinalPacketWindowEvidence(query, true, false, 99L, 98L);
                observe.Invoke(null, new object[] { run, 121 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketObservedSamples"), NUnit.Framework.Is.EqualTo(1));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketMaximumObservedRejects"), NUnit.Framework.Is.EqualTo(11L));
                observe.Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketObservedSamples"), NUnit.Framework.Is.EqualTo(2));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketMaximumObservedRejects"), NUnit.Framework.Is.EqualTo(99L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketMaximumObservedProofs"), NUnit.Framework.Is.EqualTo(98L));
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_RejectSteadyCapacityGrowth()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetOrdinalPacketWindowEvidence(query, true, false, 11L, 5L);
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                PrepareOrdinalPacketWindowQuery(query, 64);
                EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketCapacityStable"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new[] { run })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteOrdinalPacketWindow_InvalidEvidenceMissingOrFallback(bool fallback)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            object run = NewEligibilityRun();
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                if (fallback)
                {
                    PrepareOrdinalPacketWindowQuery(query, 32);
                    SetOrdinalPacketWindowEvidence(query, true, true, 11L, 5L);
                    EligibilityMethod("ObserveBruteOrdinalPacket").Invoke(null, new object[] { run, 121 });
                }
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteOrdinalPacket").Invoke(null, new[] { run })).InnerException,
                    NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteOrdinalPacketWindow_RestorePreviousIdempotent(bool previous)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            FieldInfo owner = CoarseEnvelopeStaticField("ordinalPacketQuery");
            FieldInfo runOwner = CoarseEnvelopeStaticField("ordinalPacketRun");
            FieldInfo oldFlag = CoarseEnvelopeStaticField("previousOrdinalPacket");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object saved = oldFlag.GetValue(null);
            object run = NewEligibilityRun();
            try
            {
                owner.SetValue(null, query);
                runOwner.SetValue(null, run);
                oldFlag.SetValue(null, previous);
                query.EnableBruteOrdinalPacketForDiagnostics = !previous;
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(runOwner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketRestored"), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                restore.Invoke(null, null);
                oldFlag.SetValue(null, saved);
            }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_WrongRunCannotReleaseOwner()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                object wrong = NewEligibilityRun();
                foreach (string name in new[] { "CompleteBruteOrdinalPacket", "ObserveBruteOrdinalPacket" })
                {
                    MethodInfo method = EligibilityMethod(name);
                    object[] args = name == "CompleteBruteOrdinalPacket" ? new[] { wrong } : new object[] { wrong, 121 };
                    NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        method.Invoke(null, args)).InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                    NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.SameAs(run));
                    NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.True);
                }
            }
            finally { EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteOrdinalPacketWindow_ExternalExitRestoresOwnedFlag()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteOrdinalPacket");
            BruteForceSceneQuery query = NewOrdinalPacketWindowQuery();
            object run = NewEligibilityRun();
            FieldInfo stateOwner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(stateOwner.GetValue(null), NUnit.Framework.Is.Null);
            object candidate = Activator.CreateInstance(stateOwner.FieldType, true);
            stateOwner.FieldType.GetField("bruteOrdinalPacketCandidate").SetValue(candidate, true);
            stateOwner.FieldType.GetField("phase").SetValue(candidate, "EXITING");
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                stateOwner.SetValue(null, candidate);
                EligibilityMethod("OnPlayMode").Invoke(null, new object[] { PlayModeStateChange.ExitingPlayMode });
                NUnit.Framework.Assert.That(query.EnableBruteOrdinalPacketForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteOrdinalPacketRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("ordinalPacketRun").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                stateOwner.SetValue(null, null);
                EligibilityMethod("RestoreBruteOrdinalPacket").Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteDepthRejectWindow_RequestOnlyOutput(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod("BuildBruteDepthRejectRequest")
                .Invoke(null, new object[] { index });
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH77-BRUTE-DEPTH-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action + (index % 2 == 0 ? "-depth-off" : "-depth-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.TestCase(-1)]
        [NUnit.Framework.TestCase(4)]
        public void BruteDepthRejectWindow_OutsideMatrix(int index)
        {
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                EligibilityMethod("BuildBruteDepthRejectRequest").Invoke(null, new object[] { index })).InnerException,
                NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void BruteDepthRejectWindow_MenuKeepsLegacy17AndColdOwner()
        {
            var attributes = EligibilityMethod("BeginBruteDepthReject").GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(attributes.Length, NUnit.Framework.Is.EqualTo(1));
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch77 Depth Reject Full Driver GC 1000 AI OFF ON"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingDepthRejectWindow").GetValue(null), NUnit.Framework.Is.EqualTo(false));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("depthRejectQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("depthRejectRun").GetValue(null), NUnit.Framework.Is.Null);
        }

        [NUnit.Framework.Test]
        public void BruteDepthRejectWindow_StateDefaultsFalseAndRoutesFourBeforeGc()
        {
            EligibilityMethod("BuildBruteDepthRejectRequest");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            Type type = owner.FieldType;
            FieldInfo mode = type.GetField("bruteDepthRejectCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", type)), NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(type, true);
            mode.SetValue(candidate, true);
            type.GetField("bruteProductionOnly").SetValue(candidate, true);
            type.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index })),
                        NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(EligibilityMethod("BuildBruteDepthRejectRequest").Invoke(null, new object[] { index }))));
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteDepthRejectWindow_ModeRejectsBeforeState(int defect)
        {
            EligibilityMethod("BeginBruteDepthReject");
            FieldInfo startup = CoarseEnvelopeStaticField("startingDepthRejectWindow");
            FieldInfo other = CoarseEnvelopeStaticField("startingOrdinalPacketWindow");
            object saved = startup.GetValue(null), savedOther = other.GetValue(null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            object[] args = new object[parameters.Length];
            for (int index = 0; index < args.Length; index++)
                args[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                    parameters[index].Name == "bruteBranchTimingOnly" && defect == 2;
            try
            {
                startup.SetValue(null, true);
                other.SetValue(null, defect == 3);
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    begin.Invoke(null, args)).InnerException.Message, NUnit.Framework.Does.Contain("depth reject window"));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("state").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally { startup.SetValue(null, saved); other.SetValue(null, savedOther); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        [NUnit.Framework.TestCase(5)]
        [NUnit.Framework.TestCase(6)]
        [NUnit.Framework.TestCase(7)]
        public void BruteDepthRejectWindow_RejectQueryScope(int defect)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteDepthReject");
            var query = NewOrdinalPacketWindowQuery();
            if (defect == 0) query.EnableBruteDepthRejectForDiagnostics = true;
            if (defect == 1) query.EnableBruteGeometryFirstForDiagnostics = false;
            if (defect == 2) query.EnableBruteKind5PresenceForDiagnostics = true;
            if (defect == 3) query.EnableBruteCoarseEnvelopeForDiagnostics = true;
            if (defect == 4) query.EnableBruteBranchTimingForDiagnostics = true;
            if (defect == 5) query.EnableBruteCoarseProofReuseForDiagnostics = true;
            if (defect == 6) query.EnableBruteOrdinalPacketForDiagnostics = true;
            if (defect == 7) query.EnableBruteExactCacheForDiagnostics = false;
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true })).InnerException,
                NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("depthRejectQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(query.EnableBruteDepthRejectForDiagnostics, NUnit.Framework.Is.EqualTo(defect == 0));
        }

        [NUnit.Framework.TestCase(false, false)]
        [NUnit.Framework.TestCase(false, true)]
        [NUnit.Framework.TestCase(true, false)]
        [NUnit.Framework.TestCase(true, true)]
        public void BruteDepthRejectWindow_ApplyObserveCompleteRestores(bool enabled, bool positiveCoverage)
        {
            object run = NewEligibilityRun();
            var query = NewOrdinalPacketWindowQuery();
            try
            {
                EligibilityMethod("ApplyBruteDepthReject").Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.BruteExactParticipantCapacityForDiagnostics, NUnit.Framework.Is.EqualTo(0));
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetDepthRejectWindowEvidence(query, enabled, false, enabled && positiveCoverage ? 11L : 0L);
                EligibilityMethod("ObserveBruteDepthReject").Invoke(null, new object[] { run, 121 });
                EligibilityMethod("CompleteBruteDepthReject").Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityMethod("DepthRejectObservationValid").Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(true));
                foreach (string field in new[] { "Participant", "Itr", "Body" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectSteady" + field + "Capacity"), NUnit.Framework.Is.EqualTo(32));
                NUnit.Framework.Assert.That(query.EnableBruteDepthRejectForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("depthRejectRun").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally { EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteDepthRejectWindow_DriftLatchesUntilComplete(int defect)
        {
            object run = NewEligibilityRun();
            var query = NewOrdinalPacketWindowQuery();
            try
            {
                EligibilityMethod("ApplyBruteDepthReject").Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetDepthRejectWindowEvidence(query, true, false, 11L);
                if (defect == 0) query.EnableBruteDepthRejectForDiagnostics = false;
                else query.EnableBruteGeometryFirstForDiagnostics = false;
                EligibilityMethod("ObserveBruteDepthReject").Invoke(null, new object[] { run, 121 });
                query.EnableBruteDepthRejectForDiagnostics = query.EnableBruteGeometryFirstForDiagnostics = true;
                EligibilityMethod("ObserveBruteDepthReject").Invoke(null, new object[] { run, 122 });
                AssertDepthRejectWindowCompletionRejected(query, run);
            }
            finally { EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        public void BruteDepthRejectWindow_RejectSteadyCapacityGrowth(int capacity)
        {
            object run = NewEligibilityRun();
            var query = NewOrdinalPacketWindowQuery();
            try
            {
                EligibilityMethod("ApplyBruteDepthReject").Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetDepthRejectWindowEvidence(query, true, false, 11L);
                EligibilityMethod("ObserveBruteDepthReject").Invoke(null, new object[] { run, 121 });
                typeof(BruteForceSceneQuery).GetField(new[] { "_bruteExactParticipantCapacity", "_bruteExactItrCapacity", "_bruteExactBodyCapacity" }[capacity],
                    BindingFlags.Instance | BindingFlags.NonPublic).SetValue(query, 64);
                EligibilityMethod("ObserveBruteDepthReject").Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectCapacityStable"), NUnit.Framework.Is.EqualTo(false));
                AssertDepthRejectWindowCompletionRejected(query, run);
            }
            finally { EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteDepthRejectWindow_InvalidEvidenceMissingOrFallback(bool fallback)
        {
            object run = NewEligibilityRun();
            var query = NewOrdinalPacketWindowQuery();
            try
            {
                EligibilityMethod("ApplyBruteDepthReject").Invoke(null, new object[] { query, run, true });
                if (fallback)
                {
                    PrepareOrdinalPacketWindowQuery(query, 32);
                    SetDepthRejectWindowEvidence(query, true, true, 11L);
                    EligibilityMethod("ObserveBruteDepthReject").Invoke(null, new object[] { run, 121 });
                }
                AssertDepthRejectWindowCompletionRejected(query, run);
            }
            finally { EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteDepthRejectWindow_RestorePreviousIdempotent(bool previous)
        {
            var query = NewOrdinalPacketWindowQuery();
            object run = NewEligibilityRun();
            FieldInfo savedFlag = CoarseEnvelopeStaticField("previousDepthReject");
            object saved = savedFlag.GetValue(null);
            try
            {
                CoarseEnvelopeStaticField("depthRejectQuery").SetValue(null, query);
                CoarseEnvelopeStaticField("depthRejectRun").SetValue(null, run);
                savedFlag.SetValue(null, previous);
                query.EnableBruteDepthRejectForDiagnostics = !previous;
                for (int index = 0; index < 2; index++)
                    NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteDepthRejectForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("depthRejectQuery").GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("depthRejectRun").GetValue(null), NUnit.Framework.Is.Null);
            }
            finally { EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); savedFlag.SetValue(null, saved); }
        }

        [NUnit.Framework.Test]
        public void BruteDepthRejectWindow_SecondAndWrongOwnerCannotRelease()
        {
            object run = NewEligibilityRun();
            var query = NewOrdinalPacketWindowQuery();
            try
            {
                MethodInfo apply = EligibilityMethod("ApplyBruteDepthReject");
                apply.Invoke(null, new object[] { query, run, true });
                object wrong = NewEligibilityRun();
                NUnit.Framework.Assert.Throws<TargetInvocationException>(() => apply.Invoke(null, new object[] { query, wrong, false }));
                NUnit.Framework.Assert.Throws<TargetInvocationException>(() => EligibilityMethod("ObserveBruteDepthReject").Invoke(null, new object[] { wrong, 121 }));
                NUnit.Framework.Assert.Throws<TargetInvocationException>(() => EligibilityMethod("CompleteBruteDepthReject").Invoke(null, new[] { wrong }));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("depthRejectRun").GetValue(null), NUnit.Framework.Is.SameAs(run));
                NUnit.Framework.Assert.That(query.EnableBruteDepthRejectForDiagnostics, NUnit.Framework.Is.True);
            }
            finally { EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteDepthRejectWindow_ExternalExitRestoresOwnedFlag()
        {
            object run = NewEligibilityRun();
            var query = NewOrdinalPacketWindowQuery();
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            object candidate = Activator.CreateInstance(owner.FieldType, true);
            owner.FieldType.GetField("bruteDepthRejectCandidate").SetValue(candidate, true);
            owner.FieldType.GetField("phase").SetValue(candidate, "EXITING");
            try
            {
                EligibilityMethod("ApplyBruteDepthReject").Invoke(null, new object[] { query, run, true });
                owner.SetValue(null, candidate);
                EligibilityMethod("OnPlayMode").Invoke(null, new object[] { PlayModeStateChange.ExitingPlayMode });
                NUnit.Framework.Assert.That(query.EnableBruteDepthRejectForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectRestored"), NUnit.Framework.Is.EqualTo(true));
            }
            finally { owner.SetValue(null, null); EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteDepthRejectWindow_ObservationDedupsFreshTick()
        {
            object run = NewEligibilityRun();
            var query = NewOrdinalPacketWindowQuery();
            try
            {
                EligibilityMethod("ApplyBruteDepthReject").Invoke(null, new object[] { query, run, true });
                PrepareOrdinalPacketWindowQuery(query, 32);
                SetDepthRejectWindowEvidence(query, true, false, 11L);
                MethodInfo observe = EligibilityMethod("ObserveBruteDepthReject");
                observe.Invoke(null, new object[] { run, 121 });
                SetDepthRejectWindowEvidence(query, true, false, 99L);
                observe.Invoke(null, new object[] { run, 121 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectObservedSamples"), NUnit.Framework.Is.EqualTo(1));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectMaximumObservedRejects"), NUnit.Framework.Is.EqualTo(11L));
                observe.Invoke(null, new object[] { run, 122 });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectObservedSamples"), NUnit.Framework.Is.EqualTo(2));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectMaximumObservedRejects"), NUnit.Framework.Is.EqualTo(99L));
            }
            finally { EligibilityMethod("RestoreBruteDepthReject").Invoke(null, null); }
        }

        private static void SetDepthRejectWindowEvidence(BruteForceSceneQuery query, bool applied, bool fallback, long rejects)
        {
            SetOrdinalPacketWindowProperty(query, "LastBruteDepthRejectAppliedForDiagnostics", applied);
            SetOrdinalPacketWindowProperty(query, "LastBruteExactCacheFallbackForDiagnostics", fallback);
            SetOrdinalPacketWindowProperty(query, "LastBruteDepthRejectedDirectionCountForDiagnostics", rejects);
        }

        private static void AssertDepthRejectWindowCompletionRejected(BruteForceSceneQuery query, object run)
        {
            NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                EligibilityMethod("CompleteBruteDepthReject").Invoke(null, new[] { run })).InnerException,
                NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(query.EnableBruteDepthRejectForDiagnostics, NUnit.Framework.Is.False);
            NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteDepthRejectRestored"), NUnit.Framework.Is.EqualTo(true));
        }

        private static BruteForceSceneQuery NewOrdinalPacketWindowQuery()
        {
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceBruteForce;
            return query;
        }

        private static void PrepareOrdinalPacketWindowQuery(BruteForceSceneQuery query, int capacity)
        {
            MethodInfo prepare = typeof(BruteForceSceneQuery).GetMethod("PrepareBattleCapacity",
                BindingFlags.Instance | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(prepare, NUnit.Framework.Is.Not.Null);
            prepare.Invoke(query, new object[] { capacity, 1, 1 });
        }

        private static void SetOrdinalPacketWindowEvidence(BruteForceSceneQuery query, bool applied,
            bool fallback, long rejects, long proofs)
        {
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketAppliedForDiagnostics", applied);
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketFallbackForDiagnostics", fallback);
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketRejectedDirectionCountForDiagnostics", rejects);
            SetOrdinalPacketWindowProperty(query, "LastBruteOrdinalPacketProofCountForDiagnostics", proofs);
        }

        private static void SetOrdinalPacketWindowProperty(BruteForceSceneQuery query, string name, object value)
        {
            PropertyInfo property = typeof(BruteForceSceneQuery).GetProperty(name);
            NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null);
            property.GetSetMethod(true).Invoke(query, new[] { value });
        }


        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void RenderTextureBindingWindow_ApplyCompleteRestoresOwnedFlag(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            MethodInfo complete = EligibilityMethod("CompleteRenderTextureBindingReuse");
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(previous, NUnit.Framework.Is.False, "The candidate remains default OFF.");
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { run, enabled });
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseEnabled"), NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingAcceptedCameraSamples", 1);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyPrepareCount", 4L);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyReuseCount", enabled ? 2L : 0L);
                complete.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagUnchanged"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                restore.Invoke(null, null);
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.Test]
        public void RenderTextureBindingWindow_RejectsSecondOwnerWithoutChangingFirst()
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(previous, NUnit.Framework.Is.False);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object first = NewEligibilityRun();
            object second = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { first, true });
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { second, false }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(first, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(second, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
            }
            finally
            {
                restore.Invoke(null, null);
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.Test]
        public void RenderTextureBindingWindow_RejectsUnownedNondefaultFlag()
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object run = NewEligibilityRun();
            try
            {
                flag.SetValue(null, true);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { run, false }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagApplied"), NUnit.Framework.Is.EqualTo(false));
            }
            finally
            {
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void RenderTextureBindingWindow_CompleteRejectsDriftAndExitRestoreIsIdempotent(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyRenderTextureBindingReuse");
            MethodInfo complete = EligibilityMethod("CompleteRenderTextureBindingReuse");
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(previous, NUnit.Framework.Is.False);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { run, enabled });
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingAcceptedCameraSamples", 1);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyPrepareCount", 4L);
                SetRenderTextureBindingWindowRunField(run, "renderTextureBindingBodyReuseCount", 0L);
                flag.SetValue(null, !enabled);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseFlagUnchanged"), NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "renderTextureBindingReuseRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(previous));
            }
            finally
            {
                restore.Invoke(null, null);
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void RenderTextureBindingWindow_RestoreWithoutOwnerPreservesExternalFlag(bool externalFlag)
        {
            MethodInfo restore = EligibilityMethod("RestoreRenderTextureBindingReuse");
            PropertyInfo flag = RenderTextureBindingWindowFlag();
            bool previous = (bool)flag.GetValue(null);
            NUnit.Framework.Assert.That(RenderTextureBindingWindowOwner().GetValue(null), NUnit.Framework.Is.EqualTo(false));
            try
            {
                flag.SetValue(null, externalFlag);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(externalFlag));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(flag.GetValue(null), NUnit.Framework.Is.EqualTo(externalFlag));
            }
            finally
            {
                flag.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 2, 8, true)]
        [NUnit.Framework.TestCase(10L, 10L, 73, 73, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 9L, 73, 73, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 74, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 0, 0, 2, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 0, 0, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 0, 8, false)]
        [NUnit.Framework.TestCase(10L, 11L, 73, 73, 2, -1, false)]
        public void RenderTextureBindingWindow_ObservationRequiresNewValidBodyExecute(
            long previousSequence, long currentSequence, int expectedCameraId, int actualCameraId,
            int prepareCount, int reuseCount, bool expected)
        {
            MethodInfo valid = EligibilityMethod("IsRenderTextureBindingObservationFresh");
            Type feature = typeof(NTSD.Animation.Rendering.BattleRenderFeature);
            foreach (string propertyName in new[]
            {
                "LastSegmentTextureBindingExecuteSequenceForDiagnostics",
                "LastSegmentTextureBindingCameraIdForDiagnostics",
            })
            {
                PropertyInfo property = feature.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static);
                NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null, propertyName);
                NUnit.Framework.Assert.That(property.PropertyType, NUnit.Framework.Is.EqualTo(
                    propertyName.Contains("Sequence") ? typeof(long) : typeof(int)));
                NUnit.Framework.Assert.That(property.GetGetMethod(), NUnit.Framework.Is.Not.Null);
                NUnit.Framework.Assert.That(property.GetSetMethod(), NUnit.Framework.Is.Null, "Observation has no public setter.");
            }
            NUnit.Framework.Assert.That(valid.GetParameters().Length, NUnit.Framework.Is.EqualTo(6),
                "Fresh Execute does not require a new logic tick, publication or GPU completion.");
            NUnit.Framework.Assert.That(valid.Invoke(null, new object[]
            {
                previousSequence, currentSequence, expectedCameraId, actualCameraId, prepareCount, reuseCount,
            }), NUnit.Framework.Is.EqualTo(expected));
            if (expected)
            {
                NUnit.Framework.Assert.That(valid.Invoke(null, new object[]
                {
                    previousSequence, currentSequence, -73, -73, prepareCount, reuseCount,
                }), NUnit.Framework.Is.EqualTo(true), "Unity instance identity may be negative; only zero is invalid.");
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void RenderTextureBindingWindow_RequestKeepsProductionWorkloadAndFreshOutput(int index)
        {
            MethodInfo method = EligibilityMethod("BuildRenderTextureBindingRequest");
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH68-RENDER-BINDING-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json"));
            NUnit.Framework.Assert.That(actual.inputMode, NUnit.Framework.Is.EqualTo("ai"));
            NUnit.Framework.Assert.That(actual.entityCount, NUnit.Framework.Is.EqualTo(1000));
            NUnit.Framework.Assert.That(actual.warmupTicks, NUnit.Framework.Is.EqualTo(120));
            NUnit.Framework.Assert.That(actual.sampleTicks, NUnit.Framework.Is.EqualTo(180));
            NUnit.Framework.Assert.That(actual.maxCatchUpTicksPerFrame, NUnit.Framework.Is.EqualTo(2));
            NUnit.Framework.Assert.That(actual.maxBacklogTicks, NUnit.Framework.Is.EqualTo(2));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
            if (index == 0)
            {
                foreach (int rejectedIndex in new[] { -1, 4 })
                    NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        method.Invoke(null, new object[] { rejectedIndex })).InnerException,
                        NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static PropertyInfo RenderTextureBindingWindowFlag()
        {
            PropertyInfo property = typeof(NTSD.Animation.Rendering.BattleRenderFeature).GetProperty(
                "EnableSegmentTextureBindingReuseForDiagnostics", BindingFlags.Public | BindingFlags.Static);
            NUnit.Framework.Assert.That(property, NUnit.Framework.Is.Not.Null);
            return property;
        }

        private static FieldInfo RenderTextureBindingWindowOwner()
        {
            FieldInfo field = typeof(BattleOptimizationWindowsAiSuiteEditor).GetField(
                "renderTextureBindingReuseOwner", BindingFlags.NonPublic | BindingFlags.Static);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(field.FieldType, NUnit.Framework.Is.EqualTo(typeof(bool)));
            return field;
        }

        private static void SetRenderTextureBindingWindowRunField(object run, string name, object value)
        {
            FieldInfo field = run.GetType().GetField(name, BindingFlags.Public | BindingFlags.Instance);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null, name);
            field.SetValue(run, value);
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEnvelopeBranchTimingWindow_RequestKeepsProductionWorkload(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteEnvelopeBranchTimingRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH65-ENVELOPE-PATH-BRANCH-TIMING-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-timing-off" : "-timing-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_RequestRejectsOutsideMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEnvelopeBranchTimingRequest");
            foreach (int index in new[] { -1, 4 })
                NUnit.Framework.Assert.That(NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index })).InnerException,
                    NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_ModeIsExplicitAndRoutesBeforeScope()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteEnvelopeBranchTiming");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch65 Envelope Branch Timing Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            FieldInfo mode = owner.FieldType.GetField("bruteEnvelopeBranchTimingCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", owner.FieldType)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(owner.FieldType, true);
            mode.SetValue(candidate, true);
            owner.FieldType.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(EligibilityMethod("BuildCurrentRequest").Invoke(null,
                        new object[] { index })), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(
                            EligibilityMethod("BuildBruteEnvelopeBranchTimingRequest").Invoke(null, new object[] { index }))));
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_ModeRejectsMissingScopeAndOtherModes()
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingEnvelopeBranchTimingWindow");
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            try
            {
                for (int defect = 0; defect < 4; defect++)
                {
                    object[] arguments = new object[parameters.Length];
                    for (int index = 0; index < arguments.Length; index++)
                        arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                            parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                            parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                            parameters[index].Name == "bruteBranchTimingOnly" && defect == 3;
                    startup.SetValue(null, true);
                    TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        begin.Invoke(null, arguments));
                    NUnit.Framework.Assert.That(error.InnerException.Message,
                        NUnit.Framework.Does.Contain("envelope branch timing window requires"));
                    NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("state").GetValue(null), NUnit.Framework.Is.Null);
                }
            }
            finally { startup.SetValue(null, false); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_ColdOwnerAndOldScopesRejectNewOptIn()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBranchTiming");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            foreach (string flag in new[] { "EnableBruteEnvelopeBranchTimingForDiagnostics",
                "EnableBruteBranchTimingForDiagnostics", "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics", "EnableBruteEligibilityReuseForDiagnostics",
                "EnableBruteKind5PresenceForDiagnostics", "EnableBruteCoarseDispatchForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                typeof(BruteForceSceneQuery).GetProperty(flag).SetValue(query, true);
                try
                {
                    NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
                    NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null),
                        NUnit.Framework.Is.Null);
                }
                finally { restore.Invoke(null, null); }
            }
            var excluded = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            excluded.EnableBruteEnvelopeBranchTimingForDiagnostics = true;
            foreach (string valid in new[] { "EnvelopeBindingQueryDefaultsValid", "CoarseEnvelopeQueryDefaultsValid" })
                NUnit.Framework.Assert.That(EligibilityMethod(valid).Invoke(null, new object[] { excluded }),
                    NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBranchTimingWindow_ApplyCompleteRestoresFlagsAndStride(bool enabled)
        {
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.BruteBranchTimingSampleStrideForDiagnostics = 8;
            object run = NewEligibilityRun();
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics &&
                    query.EnableBruteRejectedBindingReuseForDiagnostics && query.EnableBruteEnvelopeBranchTimingForDiagnostics,
                    NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(query.BruteBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(64));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBranchTimingScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBindingScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(false));
                EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run });
                foreach (string field in new[] { "bruteCoarseEnvelopeRestored", "bruteRejectedBindingReuseRestored",
                    "bruteEnvelopeBranchTimingRestored", "bruteBranchTimingRestored", "bruteBranchTimingFlagUnchanged" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEnvelopeBranchTimingForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.BruteBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(8));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_DriftLatchesAndStillRestores()
        {
            foreach (string flag in new[] { "EnableBruteEnvelopeBranchTimingForDiagnostics",
                "EnableBruteBranchTimingForDiagnostics", "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                object run = NewEligibilityRun();
                MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
                try
                {
                    EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, true });
                    typeof(BruteForceSceneQuery).GetProperty(flag).SetValue(query, false);
                    EligibilityMethod("ObserveBruteEnvelopeBinding").Invoke(null, new[] { run });
                    typeof(BruteForceSceneQuery).GetProperty(flag).SetValue(query, true);
                    NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
                }
                finally
                {
                    NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                    NUnit.Framework.Assert.That(query.EnableBruteEnvelopeBranchTimingForDiagnostics ||
                        query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                }
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBranchTimingWindow_AbortRestoresCurrentOrCompletedRun(bool currentRun)
        {
            FieldInfo stateOwner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(stateOwner.GetValue(null), NUnit.Framework.Is.Null);
            object run = NewEligibilityRun();
            object candidate = Activator.CreateInstance(stateOwner.FieldType, true);
            Array runs = Array.CreateInstance(run.GetType(), 1);
            runs.SetValue(run, 0);
            stateOwner.FieldType.GetField("runs").SetValue(candidate, runs);
            stateOwner.FieldType.GetField("runIndex").SetValue(candidate, currentRun ? 0 : 1);
            FieldInfo mode = stateOwner.FieldType.GetField("bruteEnvelopeBranchTimingCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            mode.SetValue(candidate, true);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, true });
                stateOwner.SetValue(null, candidate);
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEnvelopeBindingForExit").Invoke(null, null),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEnvelopeBranchTimingForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEnvelopeBranchTimingRestored"),
                    NUnit.Framework.Is.EqualTo(currentRun));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); stateOwner.SetValue(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBranchTimingWindow_StrideDriftIsNotHiddenByRestoringFlag()
        {
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBranchTiming").Invoke(null, new object[] { query, run, true });
                query.BruteBranchTimingSampleStrideForDiagnostics = 1;
                EligibilityMethod("ObserveBruteEnvelopeBinding").Invoke(null, new[] { run });
                query.BruteBranchTimingSampleStrideForDiagnostics = 64;
                NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
            }
            finally
            {
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.BruteBranchTimingSampleStrideForDiagnostics, NUnit.Framework.Is.EqualTo(1));
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBranchTimingWindow_ObservationRequiresCoverageAndRestoration(bool enabled)
        {
            MethodInfo valid = EligibilityMethod("EnvelopeBranchTimingObservationValid");
            object run = NewEligibilityRun();
            foreach (string field in new[] { "bruteEnvelopeBranchTimingMode", "bruteCoarseEnvelopeEnabled",
                "bruteCoarseEnvelopeFlagApplied", "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                "bruteRejectedBindingReuseEnabled", "bruteRejectedBindingReuseFlagApplied",
                "bruteRejectedBindingReuseFlagUnchanged", "bruteRejectedBindingReuseRestored",
                "bruteRejectedBindingReuseObservedApplied", "bruteEnvelopeBranchTimingFlagApplied",
                "bruteEnvelopeBranchTimingFlagUnchanged", "bruteEnvelopeBranchTimingRestored",
                "bruteBranchTimingFlagApplied", "bruteBranchTimingFlagUnchanged", "bruteBranchTimingRestored" })
                SetCoarseEnvelopeRunField(run, field, true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", 8L);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", 4L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedProbes", 3L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedReuses", 2L);
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingSampleStride", 64);
            var coverage = new BruteForceSceneQuery.BruteBranchTimingCoverage();
            if (enabled)
            {
                coverage.eligibleDirections = 128;
                coverage.timedDirections = 2;
                coverage.rejectedBindingVisits = coverage.pairAllowedVisits = coverage.exactWorkVisits = 64;
                coverage.rejectedBindingTimed = coverage.pairAllowedTimed = coverage.exactWorkTimed = 1;
            }
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingCoverage", coverage);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(true));
            var invalid = coverage;
            invalid.rejectedBindingTimed = enabled ? 0 : 1;
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingCoverage", invalid);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
            SetCoarseEnvelopeRunField(run, "bruteBranchTimingCoverage", coverage);
            SetCoarseEnvelopeRunField(run, "bruteEnvelopeBranchTimingRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEnvelopeBindingEligibility_RequestKeepsProductionWorkload(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteEnvelopeBindingEligibilityRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH64-ENVELOPE-BINDING-ELIGIBILITY-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-eligibility-off" : "-eligibility-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_RequestRejectsOutsideMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEnvelopeBindingEligibilityRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_ModeIsExplicitAndRoutesBeforeScope()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteEnvelopeBindingEligibility");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch64 Envelope Binding Eligibility Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingEnvelopeBindingEligibilityWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            FieldInfo mode = owner.FieldType.GetField("bruteEnvelopeBindingEligibilityCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", owner.FieldType)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(owner.FieldType, true);
            mode.SetValue(candidate, true);
            owner.FieldType.GetField("bruteProductionOnly").SetValue(candidate, true);
            owner.FieldType.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteEnvelopeBindingEligibilityRequest").Invoke(null,
                        new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_ModeRejectsMissingScopeAndOtherModes()
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingEnvelopeBindingEligibilityWindow");
            FieldInfo envelope = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo binding = CoarseEnvelopeStaticField("startingEnvelopeBindingWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            try
            {
                for (int defect = 0; defect < 6; defect++)
                {
                    object[] arguments = new object[parameters.Length];
                    for (int index = 0; index < arguments.Length; index++)
                        arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                            parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                            parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                            parameters[index].Name == "bruteEligibilityReuseCandidate" && defect == 3;
                    startup.SetValue(null, true);
                    envelope.SetValue(null, defect == 4);
                    binding.SetValue(null, defect == 5);
                    TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        begin.Invoke(null, arguments));
                    NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                    NUnit.Framework.Assert.That(error.InnerException.Message,
                        NUnit.Framework.Does.Contain("envelope binding eligibility window requires"));
                    NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                }
            }
            finally
            {
                startup.SetValue(null, false);
                envelope.SetValue(null, false);
                binding.SetValue(null, false);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBindingEligibility_ApplyScopeCompleteRestoresThree(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics &&
                    query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBindingEligibilityScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityMethod("EnvelopeBindingScopeValid").Invoke(null,
                    new object[] { query, run }), NUnit.Framework.Is.EqualTo(false));
                EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run });
                foreach (string field in new[] { "bruteCoarseEnvelopeRestored", "bruteRejectedBindingReuseRestored",
                    "bruteEligibilityReuseRestored", "bruteEligibilityReuseFlagUnchanged" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEligibilityReuseForDiagnostics,
                    NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingEligibilityOwner").GetValue(null),
                    NUnit.Framework.Is.EqualTo(false));
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_DriftLatchesAndRestorationStillWorks()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            MethodInfo observe = EligibilityMethod("ObserveBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            foreach (string property in new[] { "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics", "EnableBruteEligibilityReuseForDiagnostics",
                "EnableBruteExactCacheForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                object run = NewEligibilityRun();
                try
                {
                    apply.Invoke(null, new object[] { query, run, true });
                    query.GetType().GetProperty(property).SetValue(query, false, null);
                    observe.Invoke(null, new[] { run });
                    query.GetType().GetProperty(property).SetValue(query, true, null);
                    TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
                    NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                    NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                    NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                        query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEligibilityReuseForDiagnostics,
                        NUnit.Framework.Is.False);
                }
                finally { restore.Invoke(null, null); }
            }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingEligibility_ColdApplyRejectsUnownedCandidateWithoutMutation()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            foreach (string property in new[] { "EnableBruteKind5PresenceForDiagnostics",
                "EnableBruteEligibilityReuseForDiagnostics", "EnableBruteCoarseDispatchForDiagnostics",
                "EnableBruteBranchTimingForDiagnostics", "EnableBruteCoarseEnvelopeForDiagnostics",
                "EnableBruteRejectedBindingReuseForDiagnostics" })
            {
                var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
                query.GetType().GetProperty(property).SetValue(query, true, null);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(query.GetType().GetProperty(property).GetValue(query, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null), NUnit.Framework.Is.Null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBindingEligibility_ExitRestoresThreeAndOnlyCurrentRun(bool currentRun)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBindingEligibility");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            object candidate = Activator.CreateInstance(owner.FieldType, true);
            Array runs = Array.CreateInstance(run.GetType(), 1);
            runs.SetValue(run, 0);
            owner.FieldType.GetField("runs").SetValue(candidate, runs);
            owner.FieldType.GetField("runIndex").SetValue(candidate, currentRun ? 0 : 1);
            owner.FieldType.GetField("bruteEnvelopeBindingEligibilityCandidate").SetValue(candidate, true);
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                owner.SetValue(null, candidate);
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEnvelopeBindingForExit").Invoke(null, null),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteEligibilityReuseForDiagnostics,
                    NUnit.Framework.Is.False);
                foreach (string field in new[] { "bruteCoarseEnvelopeRestored", "bruteRejectedBindingReuseRestored",
                    "bruteEligibilityReuseRestored" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(currentRun), field);
            }
            finally
            {
                owner.SetValue(null, null);
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBindingEligibility_ObservationRequiresActualDeltaAndThreeRestored(bool enabled)
        {
            MethodInfo valid = EligibilityMethod("EnvelopeBindingEligibilityObservationValid");
            object run = NewEligibilityRun();
            foreach (string field in new[] { "bruteEnvelopeBindingEligibilityMode", "bruteCoarseEnvelopeEnabled",
                "bruteCoarseEnvelopeFlagApplied", "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                "bruteRejectedBindingReuseEnabled", "bruteRejectedBindingReuseFlagApplied",
                "bruteRejectedBindingReuseFlagUnchanged", "bruteRejectedBindingReuseRestored",
                "bruteRejectedBindingReuseObservedApplied", "bruteEligibilityReuseFlagApplied",
                "bruteEligibilityReuseFlagUnchanged", "bruteEligibilityReuseRestored" })
                SetCoarseEnvelopeRunField(run, field, true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", 8L);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", 4L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedProbes", 3L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedReuses", 2L);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseObservedApplied", enabled);
            SetCoarseEnvelopeRunField(run, "warmupTicks", 120);
            SetCoarseEnvelopeRunField(run, "sampledTicks", 180);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseAppliedDelta", enabled ? 300L : 0L);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(true));
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseAppliedDelta", enabled ? 299L : 1L);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseAppliedDelta", enabled ? 300L : 0L);
            SetCoarseEnvelopeRunField(run, "bruteEligibilityReuseRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }


        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBinding_ExitRestoresAndRecordsOnlyCurrentRun(bool currentRun)
        {
            FieldInfo stateOwner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(stateOwner.GetValue(null), NUnit.Framework.Is.Null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            object candidate = Activator.CreateInstance(stateOwner.FieldType, true);
            Array runs = Array.CreateInstance(run.GetType(), 1);
            runs.SetValue(run, 0);
            stateOwner.FieldType.GetField("runs").SetValue(candidate, runs);
            stateOwner.FieldType.GetField("runIndex").SetValue(candidate, currentRun ? 0 : 1);
            stateOwner.FieldType.GetField("bruteEnvelopeBindingCandidate").SetValue(candidate, true);
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            try
            {
                EligibilityMethod("ApplyBruteEnvelopeBinding").Invoke(null, new object[] { query, run, true });
                stateOwner.SetValue(null, candidate);
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEnvelopeBindingForExit").Invoke(null, null),
                    NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null),
                    NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeRestored"),
                    NUnit.Framework.Is.EqualTo(currentRun));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseRestored"),
                    NUnit.Framework.Is.EqualTo(currentRun));
            }
            finally
            {
                stateOwner.SetValue(null, null);
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEnvelopeBindingRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteEnvelopeBindingRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH63-ENVELOPE-BINDING-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-binding-off" : "-binding-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBindingRequest_RejectsOutsideFixedMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEnvelopeBindingRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBinding_MenuPreservesLegacySignatureAndColdOwnership()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteEnvelopeBinding");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(attributes.Length, NUnit.Framework.Is.EqualTo(1));
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch63 Envelope Binding Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingEnvelopeBindingWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBinding_ModeDefaultsFalseAndRoutesBeforeScope()
        {
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            Type type = owner.FieldType;
            FieldInfo mode = type.GetField("bruteEnvelopeBindingCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(mode.GetValue(JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", type)),
                NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(type, true);
            mode.SetValue(candidate, true);
            type.GetField("bruteProductionOnly").SetValue(candidate, true);
            type.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteEnvelopeBindingRequest").Invoke(null, new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally { owner.SetValue(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        [NUnit.Framework.TestCase(4)]
        public void BruteEnvelopeBinding_ModeRejectsMissingScopeOrOtherOwnerBeforeState(int defect)
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingEnvelopeBindingWindow");
            FieldInfo oldStartup = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object previous = startup.GetValue(null);
            object previousOld = oldStartup.GetValue(null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
                arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                    parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                    parameters[index].Name == "bruteEligibilityReuseCandidate" && defect == 3;
            try
            {
                startup.SetValue(null, true);
                oldStartup.SetValue(null, defect == 4);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    begin.Invoke(null, arguments));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(error.InnerException.Message,
                    NUnit.Framework.Does.Contain("envelope binding window requires"));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                startup.SetValue(null, previous);
                oldStartup.SetValue(null, previousOld);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBinding_ApplyCompleteRestoresBothAndPreservesDefaults(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                foreach (string field in new[] { "bruteCoarseEnvelopeEnabled", "bruteCoarseEnvelopeFlagApplied",
                    "bruteRejectedBindingReuseFlagApplied" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseEnabled"),
                    NUnit.Framework.Is.EqualTo(enabled));
                EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run });
                foreach (string field in new[] { "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                    "bruteRejectedBindingReuseFlagUnchanged", "bruteRejectedBindingReuseRestored" })
                    NUnit.Framework.Assert.That(EligibilityRunField(run, field), NUnit.Framework.Is.EqualTo(true), field);
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics || query.EnableBruteKind5PresenceForDiagnostics ||
                    query.EnableBruteEligibilityReuseForDiagnostics || query.EnableBruteCoarseDispatchForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(false, false)]
        [NUnit.Framework.TestCase(false, true)]
        [NUnit.Framework.TestCase(true, false)]
        [NUnit.Framework.TestCase(true, true)]
        public void BruteEnvelopeBinding_RestoreBothPriorValuesReleasesOwnerAndIsIdempotent(bool envelope, bool binding)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            FieldInfo owner = CoarseEnvelopeStaticField("envelopeBindingQuery");
            FieldInfo oldEnvelope = CoarseEnvelopeStaticField("previousEnvelopeBindingEnvelope");
            FieldInfo oldBinding = CoarseEnvelopeStaticField("previousEnvelopeBindingReuse");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object savedEnvelope = oldEnvelope.GetValue(null);
            object savedBinding = oldBinding.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteCoarseEnvelopeForDiagnostics = !envelope;
            query.EnableBruteRejectedBindingReuseForDiagnostics = !binding;
            try
            {
                owner.SetValue(null, query);
                oldEnvelope.SetValue(null, envelope);
                oldBinding.SetValue(null, binding);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.EqualTo(envelope));
                NUnit.Framework.Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.EqualTo(binding));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                owner.SetValue(null, null);
                oldEnvelope.SetValue(null, savedEnvelope);
                oldBinding.SetValue(null, savedBinding);
            }
        }

        [NUnit.Framework.TestCase("EnableBruteCoarseEnvelopeForDiagnostics", false)]
        [NUnit.Framework.TestCase("EnableBruteRejectedBindingReuseForDiagnostics", false)]
        [NUnit.Framework.TestCase("EnableBruteExactCacheForDiagnostics", false)]
        [NUnit.Framework.TestCase("EnableBruteKind5PresenceForDiagnostics", true)]
        public void BruteEnvelopeBinding_CompleteRejectsDriftAndRestoreStillWorks(string property, bool value)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                query.GetType().GetProperty(property).SetValue(query, value, null);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    EligibilityMethod("CompleteBruteEnvelopeBinding").Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase("EnableBruteKind5PresenceForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteEligibilityReuseForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseDispatchForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteBranchTimingForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseEnvelopeForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteRejectedBindingReuseForDiagnostics")]
        public void BruteEnvelopeBinding_RejectsOtherCandidateOrUnownedEnable(string property)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.GetType().GetProperty(property).SetValue(query, true, null);
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("envelopeBindingQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(query.GetType().GetProperty(property).GetValue(query, null), NUnit.Framework.Is.EqualTo(true));
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEnvelopeBinding_RejectsSecondOrOldOwnerWithoutChangingFlags(bool oldOwner)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var first = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            var second = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            try
            {
                EligibilityMethod(oldOwner ? "ApplyBruteCoarseEnvelope" : "ApplyBruteEnvelopeBinding")
                    .Invoke(null, new object[] { first, NewEligibilityRun(), true });
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { second, NewEligibilityRun(), true }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(first.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(first.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.EqualTo(!oldOwner));
                NUnit.Framework.Assert.That(second.EnableBruteCoarseEnvelopeForDiagnostics ||
                    second.EnableBruteRejectedBindingReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
                EligibilityMethod("RestoreBruteCoarseEnvelope").Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false, false, 0L, 0L, true)]
        [NUnit.Framework.TestCase(true, false, 0L, 0L, false)]
        [NUnit.Framework.TestCase(true, true, 12L, 3L, true)]
        [NUnit.Framework.TestCase(true, true, 12L, 0L, false)]
        [NUnit.Framework.TestCase(false, false, 12L, 0L, false)]
        [NUnit.Framework.TestCase(true, false, 12L, 3L, false)]
        public void BruteEnvelopeBinding_ObservationRequiresRealActivityAndBothRestored(
            bool enabled, bool observedApplied, long probes, long reuses, bool expected)
        {
            MethodInfo valid = EligibilityMethod("EnvelopeBindingObservationValid");
            object run = NewEligibilityRun();
            foreach (string field in new[] { "bruteCoarseEnvelopeEnabled", "bruteCoarseEnvelopeFlagApplied",
                "bruteCoarseEnvelopeFlagUnchanged", "bruteCoarseEnvelopeRestored",
                "bruteRejectedBindingReuseFlagApplied", "bruteRejectedBindingReuseFlagUnchanged",
                "bruteRejectedBindingReuseRestored" })
                SetCoarseEnvelopeRunField(run, field, true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", 8L);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", 4L);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingReuseEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingReuseObservedApplied", observedApplied);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedProbes", probes);
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingMaximumObservedReuses", reuses);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(expected));
            SetCoarseEnvelopeRunField(run, "bruteRejectedBindingReuseRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteEnvelopeBinding_ObservationRecordsMaximumAndLatchesDrift()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEnvelopeBinding");
            MethodInfo observe = EligibilityMethod("ObserveBruteEnvelopeBinding");
            MethodInfo restore = EligibilityMethod("RestoreBruteEnvelopeBinding");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                query.GetType().GetProperty("LastBruteRejectedBindingReuseAppliedForDiagnostics").GetSetMethod(true)
                    .Invoke(query, new object[] { true });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingProbeCountForDiagnostics", 11L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingReuseCountForDiagnostics", 5L);
                observe.Invoke(null, new[] { run });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingProbeCountForDiagnostics", 3L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteRejectedBindingReuseCountForDiagnostics", 1L);
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingMaximumObservedProbes"), NUnit.Framework.Is.EqualTo(11L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingMaximumObservedReuses"), NUnit.Framework.Is.EqualTo(5L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseObservedApplied"), NUnit.Framework.Is.EqualTo(true));
                query.EnableBruteRejectedBindingReuseForDiagnostics = false;
                observe.Invoke(null, new[] { run });
                query.EnableBruteRejectedBindingReuseForDiagnostics = true;
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteRejectedBindingReuseFlagUnchanged"), NUnit.Framework.Is.EqualTo(false));
            }
            finally { restore.Invoke(null, null); }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteCoarseEnvelopeRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            var actual = (ProductionEntityStressRequest)EligibilityMethod(
                "BuildBruteCoarseEnvelopeRequest").Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Is.EqualTo(
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH60-COARSE-ENVELOPE-WINDOWS-20261008/windows-01/" +
                index.ToString("D2") + "-" + expected.action +
                (index % 2 == 0 ? "-envelope-off" : "-envelope-on") + "/report.json"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelopeRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteCoarseEnvelopeRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_MenuPreservesLegacySignatureAndColdOwnership()
        {
            MethodInfo menu = EligibilityMethod("BeginBruteCoarseEnvelope");
            var attributes = menu.GetCustomAttributes(typeof(MenuItem), false);
            NUnit.Framework.Assert.That(attributes.Length, NUnit.Framework.Is.EqualTo(1));
            NUnit.Framework.Assert.That(((MenuItem)attributes[0]).menuItem, NUnit.Framework.Is.EqualTo(
                "NTSD/Validation/Optimization/Batch60 Coarse Envelope Full Driver GC 1000 AI"));
            NUnit.Framework.Assert.That(EligibilityMethod("BeginSuite").GetParameters().Length, NUnit.Framework.Is.EqualTo(17));
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow").GetValue(null),
                NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_ModeDefaultsFalseAndRoutesFourRequestsBeforeScope()
        {
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            Type stateType = owner.FieldType;
            FieldInfo mode = stateType.GetField("bruteCoarseEnvelopeCandidate");
            NUnit.Framework.Assert.That(mode, NUnit.Framework.Is.Not.Null);
            object legacy = JsonUtility.FromJson("{\"logicGcScopeOnly\":true}", stateType);
            NUnit.Framework.Assert.That(mode.GetValue(legacy), NUnit.Framework.Is.EqualTo(false));
            object candidate = Activator.CreateInstance(stateType, true);
            mode.SetValue(candidate, true);
            stateType.GetField("bruteProductionOnly").SetValue(candidate, true);
            stateType.GetField("logicGcScopeOnly").SetValue(candidate, true);
            try
            {
                owner.SetValue(null, candidate);
                for (int index = 0; index < 4; index++)
                {
                    object actual = EligibilityMethod("BuildCurrentRequest").Invoke(null, new object[] { index });
                    object expected = EligibilityMethod("BuildBruteCoarseEnvelopeRequest").Invoke(null, new object[] { index });
                    NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
                }
            }
            finally
            {
                owner.SetValue(null, null);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteCoarseEnvelope_ModeRejectsMissingScopeOrOtherCandidateBeforeState(int defect)
        {
            FieldInfo startup = CoarseEnvelopeStaticField("startingCoarseEnvelopeWindow");
            FieldInfo owner = CoarseEnvelopeStaticField("state");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object previous = startup.GetValue(null);
            MethodInfo begin = EligibilityMethod("BeginSuite");
            ParameterInfo[] parameters = begin.GetParameters();
            object[] arguments = new object[parameters.Length];
            for (int index = 0; index < arguments.Length; index++)
                arguments[index] = parameters[index].Name == "bruteProductionOnly" && defect != 0 ||
                    parameters[index].Name == "logicGcScopeOnly" && defect != 1 ||
                    parameters[index].Name == "cpuGcCaptureOnly" && defect == 2 ||
                    parameters[index].Name == "bruteEligibilityReuseCandidate" && defect == 3;
            try
            {
                startup.SetValue(null, true);
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    begin.Invoke(null, arguments));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(error.InnerException.Message,
                    NUnit.Framework.Does.Contain("coarse envelope window requires"));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            }
            finally
            {
                startup.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCoarseEnvelope_ApplyCompleteRestoresAndPreservesProductionFlags(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo complete = EligibilityMethod("CompleteBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeEnabled"), NUnit.Framework.Is.EqualTo(enabled));
                complete.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeFlagUnchanged"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                    query.EnableBruteKind5PresenceForDiagnostics || query.EnableBruteRejectedBindingReuseForDiagnostics ||
                    query.EnableBruteCoarseDispatchForDiagnostics || query.EnableBruteBranchTimingForDiagnostics,
                    NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCoarseEnvelope_RestorePriorValueReleasesOwnerAndIsIdempotent(bool previous)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            FieldInfo owner = CoarseEnvelopeStaticField("coarseEnvelopeQuery");
            FieldInfo prior = CoarseEnvelopeStaticField("previousCoarseEnvelope");
            NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
            object oldPrior = prior.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteCoarseEnvelopeForDiagnostics = !previous;
            try
            {
                owner.SetValue(null, query);
                prior.SetValue(null, previous);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(owner.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                owner.SetValue(null, null);
                prior.SetValue(null, oldPrior);
            }
        }

        [NUnit.Framework.TestCase("EnableBruteKind5PresenceForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteEligibilityReuseForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteRejectedBindingReuseForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseDispatchForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteBranchTimingForDiagnostics")]
        [NUnit.Framework.TestCase("EnableBruteCoarseEnvelopeForDiagnostics")]
        public void BruteCoarseEnvelope_RejectsOtherCandidateOrUnownedEnable(string property)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.GetType().GetProperty(property).SetValue(query, true, null);
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(CoarseEnvelopeStaticField("coarseEnvelopeQuery").GetValue(null), NUnit.Framework.Is.Null);
            NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics,
                NUnit.Framework.Is.EqualTo(property == "EnableBruteCoarseEnvelopeForDiagnostics"));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_RejectsSecondOwnerWithoutChangingEitherFlag()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var first = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            var second = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            try
            {
                apply.Invoke(null, new object[] { first, NewEligibilityRun(), true });
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    apply.Invoke(null, new object[] { second, NewEligibilityRun(), true }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(first.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(second.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCoarseEnvelope_CompleteRejectsDriftAndRestoreStillWorks(bool productionDrift)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo complete = EligibilityMethod("CompleteBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                if (productionDrift)
                    query.EnableBruteExactCacheForDiagnostics = false;
                else
                    query.EnableBruteCoarseEnvelopeForDiagnostics = false;
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteCoarseEnvelopeForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false, 0L, 0L, true)]
        [NUnit.Framework.TestCase(true, 0L, 0L, false)]
        [NUnit.Framework.TestCase(true, 12L, 4L, true)]
        [NUnit.Framework.TestCase(true, 12L, 0L, false)]
        public void BruteCoarseEnvelope_ObservationRequiresRealActivityAndRestoration(
            bool enabled, long directions, long rejects, bool expected)
        {
            MethodInfo valid = EligibilityMethod("CoarseEnvelopeObservationValid");
            object run = NewEligibilityRun();
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeEnabled", enabled);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeFlagApplied", true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeFlagUnchanged", true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeRestored", true);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections", directions);
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects", rejects);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(expected));
            SetCoarseEnvelopeRunField(run, "bruteCoarseEnvelopeRestored", false);
            NUnit.Framework.Assert.That(valid.Invoke(null, new[] { run }), NUnit.Framework.Is.EqualTo(false));
        }

        [NUnit.Framework.Test]
        public void BruteCoarseEnvelope_ObservationRecordsMaximumNotFabricatedTickTotals()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCoarseEnvelope");
            MethodInfo observe = EligibilityMethod("ObserveBruteCoarseEnvelope");
            MethodInfo restore = EligibilityMethod("RestoreBruteCoarseEnvelope");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeDirectionCountForDiagnostics", 11L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeRejectCountForDiagnostics", 5L);
                observe.Invoke(null, new[] { run });
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeDirectionCountForDiagnostics", 3L);
                SetCoarseEnvelopeDiagnosticCount(query, "LastBruteCoarseEnvelopeRejectCountForDiagnostics", 1L);
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeMaximumObservedDirections"),
                    NUnit.Framework.Is.EqualTo(11L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeMaximumObservedRejects"),
                    NUnit.Framework.Is.EqualTo(5L));
                query.EnableBruteCoarseEnvelopeForDiagnostics = false;
                observe.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteCoarseEnvelopeFlagUnchanged"),
                    NUnit.Framework.Is.EqualTo(false));
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        private static FieldInfo CoarseEnvelopeStaticField(string name)
        {
            FieldInfo field = typeof(BattleOptimizationWindowsAiSuiteEditor).GetField(
                name, BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null, name);
            return field;
        }

        private static void SetCoarseEnvelopeRunField(object run, string name, object value)
        {
            FieldInfo field = run.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null, name);
            field.SetValue(run, value);
        }

        private static void SetCoarseEnvelopeDiagnosticCount(BruteForceSceneQuery query, string name, long value)
        {
            MethodInfo setter = query.GetType().GetProperty(name).GetSetMethod(true);
            NUnit.Framework.Assert.That(setter, NUnit.Framework.Is.Not.Null, name);
            setter.Invoke(query, new object[] { value });
        }
        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteCombinedCacheRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = EligibilityMethod("BuildBruteCombinedCacheRequest");
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH52-COMBINED-CACHE-WINDOWS"));
            NUnit.Framework.Assert.That(actual.outputPath,
                NUnit.Framework.Does.Contain(index % 2 == 0 ? "-kind5-off" : "-kind5-on"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteCombinedCacheRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteCombinedCacheRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCombinedCache_ApplyCompleteRestoresBothAndPreservesProductionFlags(bool kind5Enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCombinedCache");
            MethodInfo complete = EligibilityMethod("CompleteBruteCombinedCache");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, kind5Enabled });
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.EqualTo(kind5Enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceEnabled"), NUnit.Framework.Is.EqualTo(kind5Enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteRejectedBindingReuseForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                complete.Invoke(null, new[] { run });
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseRestored"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteKind5PresenceAppliedDelta"), NUnit.Framework.Is.EqualTo(0L));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseAppliedDelta"), NUnit.Framework.Is.EqualTo(0L));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                    query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                EligibilityMethod("RestoreBruteKind5Presence").Invoke(null, null);
                EligibilityMethod("RestoreBruteEligibilityReuse").Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCombinedCache_CompleteRejectsEitherFlagDriftAndRestoreStillWorks(bool kind5Drift)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCombinedCache");
            MethodInfo complete = EligibilityMethod("CompleteBruteCombinedCache");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                if (kind5Drift)
                    query.EnableBruteKind5PresenceForDiagnostics = false;
                else
                    query.EnableBruteEligibilityReuseForDiagnostics = false;
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteKind5Presence").Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityMethod("RestoreBruteEligibilityReuse").Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                    query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                EligibilityMethod("RestoreBruteKind5Presence").Invoke(null, null);
                EligibilityMethod("RestoreBruteEligibilityReuse").Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteCombinedCache_RejectsBindingReuseOrBranchTiming(bool timing)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteCombinedCache");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteBranchTimingForDiagnostics = timing;
            query.EnableBruteRejectedBindingReuseForDiagnostics = !timing;
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics ||
                query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.False);
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteEligibilityReuseRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = EligibilityMethod("BuildBruteEligibilityReuseRequest");
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH50-ELIGIBILITY-WINDOWS"));
            NUnit.Framework.Assert.That(actual.outputPath,
                NUnit.Framework.Does.Contain(index % 2 == 0 ? "-eligibility-off" : "-eligibility-on"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEligibilityReuseRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = EligibilityMethod("BuildBruteEligibilityReuseRequest");
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEligibilityReuse_ApplyAndRestorePreservesProductionFlags(bool enabled)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEligibilityReuse");
            MethodInfo restore = EligibilityMethod("RestoreBruteEligibilityReuse");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, enabled });
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseFlagApplied"), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(EligibilityRunField(run, "bruteEligibilityReuseEnabled"), NUnit.Framework.Is.EqualTo(enabled));
                NUnit.Framework.Assert.That(query.EnableEmptyItrPairGuardForDiagnostics &&
                    query.EnableBruteEmptyItrRosterForDiagnostics && query.EnableBruteExactCacheForDiagnostics &&
                    query.EnableBruteGeometryFirstForDiagnostics, NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteKind5PresenceForDiagnostics ||
                    query.EnableBruteRejectedBindingReuseForDiagnostics ||
                    query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.False);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEligibilityReuse_RestoreRestoresPriorValueAndReleasesReference(bool previous)
        {
            MethodInfo restore = EligibilityMethod("RestoreBruteEligibilityReuse");
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            FieldInfo queryField = owner.GetField("eligibilityReuseQuery", flags);
            FieldInfo previousField = owner.GetField("previousEligibilityReuse", flags);
            NUnit.Framework.Assert.That(queryField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(previousField, NUnit.Framework.Is.Not.Null);
            object oldQuery = queryField.GetValue(null);
            object oldPrevious = previousField.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteEligibilityReuseForDiagnostics = !previous;
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, previous);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                queryField.SetValue(null, oldQuery);
                previousField.SetValue(null, oldPrevious);
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteEligibilityReuse_RejectsOtherDefaultOffCandidate(bool kind5)
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEligibilityReuse");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteKind5PresenceForDiagnostics = kind5;
            query.EnableBruteRejectedBindingReuseForDiagnostics = !kind5;
            TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                apply.Invoke(null, new object[] { query, NewEligibilityRun(), true }));
            NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
            NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.False);
        }

        [NUnit.Framework.Test]
        public void BruteEligibilityReuse_CompleteRejectsFlagDriftAndRestoreStillWorks()
        {
            MethodInfo apply = EligibilityMethod("ApplyBruteEligibilityReuse");
            MethodInfo complete = EligibilityMethod("CompleteBruteEligibilityReuse");
            MethodInfo restore = EligibilityMethod("RestoreBruteEligibilityReuse");
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            object run = NewEligibilityRun();
            try
            {
                apply.Invoke(null, new object[] { query, run, true });
                query.EnableBruteEligibilityReuseForDiagnostics = false;
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    complete.Invoke(null, new[] { run }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<InvalidOperationException>());
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteEligibilityReuseForDiagnostics, NUnit.Framework.Is.False);
            }
            finally
            {
                restore.Invoke(null, null);
            }
        }

        private static MethodInfo EligibilityMethod(string name)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                name, BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, name + " required for the candidate family.");
            return method;
        }

        private static object NewEligibilityRun()
        {
            Type type = typeof(BattleOptimizationWindowsAiSuiteEditor).GetNestedType("RunState", BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(type, NUnit.Framework.Is.Not.Null);
            return Activator.CreateInstance(type, true);
        }

        private static object EligibilityRunField(object run, string name)
        {
            FieldInfo field = run.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
            NUnit.Framework.Assert.That(field, NUnit.Framework.Is.Not.Null);
            return field.GetValue(run);
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteKind5PresenceRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteKind5PresenceRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH38-BRUTE-KIND5-PRESENCE"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteKind5PresenceRequest_RejectsOutsideFixedTwoMatrix()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteKind5PresenceRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteKind5Presence_RestoreRestoresOriginalFlag(bool previous)
        {
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            FieldInfo queryField = owner.GetField("kind5PresenceQuery", flags);
            FieldInfo previousField = owner.GetField("previousKind5Presence", flags);
            MethodInfo restore = owner.GetMethod("RestoreBruteKind5Presence", flags);
            NUnit.Framework.Assert.That(queryField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(previousField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(restore, NUnit.Framework.Is.Not.Null);
            object oldQuery = queryField.GetValue(null);
            object oldFlag = previousField.GetValue(null);
            var query = new BruteForceSceneQuery(new SimulationWorld());
            query.EnableBruteKind5PresenceForDiagnostics = !previous;
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, previous);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
                NUnit.Framework.Assert.That(query.EnableBruteKind5PresenceForDiagnostics, NUnit.Framework.Is.EqualTo(previous));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.EqualTo(true));
            }
            finally
            {
                queryField.SetValue(null, oldQuery);
                previousField.SetValue(null, oldFlag);
            }
        }
        [NUnit.Framework.TestCase(false, 1)]
        [NUnit.Framework.TestCase(false, 128)]
        [NUnit.Framework.TestCase(true, 1)]
        [NUnit.Framework.TestCase(true, 128)]
        public void BruteSampleTiming_RestoreRestoresStrideAndFlag(bool prior, int priorStride)
        {
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            FieldInfo queryField = owner.GetField("branchTimingQuery", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo previousField = owner.GetField("previousBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo strideField = owner.GetField("previousBranchTimingSampleStride", BindingFlags.Static | BindingFlags.NonPublic);
            PropertyInfo stride = typeof(BruteForceSceneQuery).GetProperty("BruteBranchTimingSampleStrideForDiagnostics");
            MethodInfo restore = owner.GetMethod("RestoreBruteBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(strideField, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(stride, NUnit.Framework.Is.Not.Null);
            NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
            object previous = previousField.GetValue(null);
            object previousStride = strideField.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteBranchTimingForDiagnostics = !prior;
            stride.SetValue(query, 64);
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, prior);
                strideField.SetValue(null, priorStride);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(prior));
                NUnit.Framework.Assert.That(stride.GetValue(query), NUnit.Framework.Is.EqualTo(priorStride));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally
            {
                queryField.SetValue(null, null);
                previousField.SetValue(null, previous);
                strideField.SetValue(null, previousStride);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteSampleTimingRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteSampledTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH45-BRUTE-SAMPLED-TIMING"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteSampleTimingRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteSampledTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(
                    () => method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException,
                    NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(false)]
        [NUnit.Framework.TestCase(true)]
        public void BruteBranchTiming_RestoreRestoresPriorValueAndReleasesReference(bool prior)
        {
            Type owner = typeof(BattleOptimizationWindowsAiSuiteEditor);
            FieldInfo queryField = owner.GetField("branchTimingQuery", BindingFlags.Static | BindingFlags.NonPublic);
            FieldInfo previousField = owner.GetField("previousBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            MethodInfo restore = owner.GetMethod("RestoreBruteBranchTiming", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
            object previous = previousField.GetValue(null);
            var query = (BruteForceSceneQuery)new SimulationWorld().SceneQuery;
            query.EnableBruteBranchTimingForDiagnostics = !prior;
            try
            {
                queryField.SetValue(null, query);
                previousField.SetValue(null, prior);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
                NUnit.Framework.Assert.That(query.EnableBruteBranchTimingForDiagnostics, NUnit.Framework.Is.EqualTo(prior));
                NUnit.Framework.Assert.That(queryField.GetValue(null), NUnit.Framework.Is.Null);
                NUnit.Framework.Assert.That(restore.Invoke(null, null), NUnit.Framework.Is.True);
            }
            finally
            {
                queryField.SetValue(null, null);
                previousField.SetValue(null, previous);
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void BruteBranchTimingRequest_OnlyChangesOutputFromProductionSmoke(int index)
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteBranchTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            ProductionEntityStressRequest expected =
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteProductionRequest(index / 2);
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH44-BRUTE-BRANCH-TIMING"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteBranchTimingRequest_RejectsOutsideFixedOffOnMatrix()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteBranchTimingRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            foreach (int index in new[] { -1, 4 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(
                    () => method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException,
                    NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteGeometryFirstRequest_OnlyChangesOutputFromExactCacheSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteExactCacheRequest(index);
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH37-BRUTE-GEOMETRY-FIRST-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-smoke/report.json";
            MethodInfo method = GetBruteGeometryFirstRequestMethod();
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
        }

        [NUnit.Framework.Test]
        public void BruteGeometryFirstRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            MethodInfo method = GetBruteGeometryFirstRequestMethod();
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException error = NUnit.Framework.Assert.Throws<TargetInvocationException>(
                    () => method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(error.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static MethodInfo GetBruteGeometryFirstRequestMethod()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteGeometryFirstRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null);
            return method;
        }



        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteExactCacheRequest_OnlyChangesOutputFromRosterSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrRosterRequest(index);
            MethodInfo method = GetBruteExactCacheRequestMethod();
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH36-BRUTE-EXACT-CACHE"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteExactCacheRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            MethodInfo method = GetBruteExactCacheRequestMethod();
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException exception = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(exception.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static MethodInfo GetBruteExactCacheRequestMethod()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteExactCacheRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, "Independent exact-cache two-smoke request required.");
            return method;
        }


        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteEmptyItrRosterRequest_OnlyChangesOutputFromBruteSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            MethodInfo method = GetBruteEmptyItrRosterRequestMethod();
            var actual = (ProductionEntityStressRequest)method.Invoke(null, new object[] { index });
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH34-BRUTE-EMPTY-ITR-ROSTER"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEmptyItrRosterRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            MethodInfo method = GetBruteEmptyItrRosterRequestMethod();
            foreach (int index in new[] { -1, 2 })
            {
                TargetInvocationException exception = NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                    method.Invoke(null, new object[] { index }));
                NUnit.Framework.Assert.That(exception.InnerException, NUnit.Framework.Is.TypeOf<ArgumentOutOfRangeException>());
            }
        }

        private static MethodInfo GetBruteEmptyItrRosterRequestMethod()
        {
            MethodInfo method = typeof(BattleOptimizationWindowsAiSuiteEditor).GetMethod(
                "BuildBruteEmptyItrRosterRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.That(method, NUnit.Framework.Is.Not.Null, "Independent two-smoke candidate request required.");
            return method;
        }
        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void BruteEmptyItrGuardRequest_OnlyChangesOutputFromBruteSmoke(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrGuardRequest(index);
            NUnit.Framework.Assert.That(actual.outputPath, NUnit.Framework.Does.Contain("BATCH32-BRUTE-EMPTY-ITR-GUARD"));
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(JsonUtility.ToJson(expected)));
        }

        [NUnit.Framework.Test]
        public void BruteEmptyItrGuardRequest_RejectsOutsideFixedTwoSmokeMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrGuardRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildBruteEmptyItrGuardRequest(2));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void PairSnapshotReuseRequest_ChangesOnlyOutput(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(index);
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH31-PAIR-SNAPSHOT-REUSE-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-smoke/report.json";
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildRolePairSnapshotReuseRequest(index);
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
        }

        [NUnit.Framework.Test]
        public void PairSnapshotReuseRequest_RejectsOutsideFixedMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRolePairSnapshotReuseRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRolePairSnapshotReuseRequest(2));
        }

        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Preparing, true)]
        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Running, false)]
        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Stopping, false)]
        [NUnit.Framework.TestCase(BattleRuntimeLifecycleState.Stopped, false)]
        public void StressPreparation_ReopensOnlyEmptyPreparingServiceOwners(
            BattleRuntimeLifecycleState lifecycle, bool admitted)
        {
            var fixture = new NTSD.Test.NTSD28PreparingShutdownOwnerEditorTests();
            fixture.SetUp();
            try
            {
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                Type fixtureType = fixture.GetType();
                var driver = (SimulationTickDriver)fixtureType.GetField("driver", flags).GetValue(fixture);
                var pool = (NTSD.Animation.LF2ObjectPool)fixtureType.GetField("pool", flags).GetValue(fixture);
                pool.BeginBattleShutdown();
                typeof(SimulationTickDriver).GetField("lifecycleState", flags).SetValue(driver, lifecycle);
                MethodInfo prepare = typeof(ProductionEntityStressRunner).GetMethod(
                    "PrepareStressWorldServices", BindingFlags.Static | BindingFlags.NonPublic);
                NUnit.Framework.Assert.IsNotNull(prepare, "Stress preparation must use the existing preparing-owner contract.");
                if (admitted)
                {
                    prepare.Invoke(null, new object[] { driver });
                    NUnit.Framework.Assert.IsTrue(pool.AcceptingRequestsForDiagnostics);
                    NUnit.Framework.Assert.IsFalse(pool.IsBattleCapacitySealed);
                    NUnit.Framework.Assert.AreSame(pool,
                        typeof(SimulationTickDriver).GetField("_battleObjectPool", flags).GetValue(driver));
                }
                else
                {
                    NUnit.Framework.Assert.Throws<TargetInvocationException>(() =>
                        prepare.Invoke(null, new object[] { driver }));
                    NUnit.Framework.Assert.IsFalse(pool.AcceptingRequestsForDiagnostics);
                }
            }
            finally
            {
                fixture.TearDown();
            }
        }

        [NUnit.Framework.TestCase(0L, true)]
        [NUnit.Framework.TestCase(-1L, false)]
        [NUnit.Framework.TestCase(1L, false)]
        [NUnit.Framework.TestCase(-60000L, false)]
        public void UnifiedAuthorityClosure_RequiresProducerAndInputTailRefresh(
            long refreshDelta, bool expected)
        {
            Type existing = typeof(ProductionEntityStressEditorTests);
            var report = (ProductionEntityStressReport)existing.GetMethod(
                "CreateValidAiUnifiedSnapshotAuthorityReport",
                BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            report.aiUnifiedSnapshotExecutionRefreshCount =
                report.aiUnifiedSnapshotExecutionReadCount * 2L + refreshDelta;
            MethodInfo evaluate = typeof(ProductionEntityStressRunner).GetMethod(
                "EvaluateAiUnifiedSnapshotAuthorityValidityForReport",
                BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.AreEqual(expected, evaluate.Invoke(null, new object[] { report, true }));
            NUnit.Framework.Assert.AreEqual(expected, report.harnessValidity);
        }

        [NUnit.Framework.TestCase(-1, false)]
        [NUnit.Framework.TestCase(0, false)]
        [NUnit.Framework.TestCase(1, true)]
        public void OrdinaryStressSpawnOid_PreservesFactoryAdmission(int oid, bool admitted)
        {
            MethodInfo check = typeof(ProductionEntityStressRunner).GetMethod("IsStressSpawnOidAdmitted", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.AreEqual(admitted, check.Invoke(null, new object[] { oid }));
        }

        [NUnit.Framework.TestCase(false, 1000, 1000, 2000, 1000, 1000, true)]
        [NUnit.Framework.TestCase(false, 0, 0, 1000, 1000, 1000, false)]
        [NUnit.Framework.TestCase(true, 0, 0, 1000, 1000, 1000, true)]
        [NUnit.Framework.TestCase(true, 0, 0, 1000, 999, 1000, false)]
        [NUnit.Framework.TestCase(true, 0, 0, 1000, 1000, 999, false)]
        [NUnit.Framework.TestCase(true, 0, 0, 999, 1000, 1000, false)]
        [NUnit.Framework.TestCase(true, 1, 0, 1000, 1000, 1000, false)]
        [NUnit.Framework.TestCase(true, 0, 1, 1000, 1000, 1000, false)]
        public void PopulationAdmission_DistinguishesLogicOnlyWithoutRelaxingCounts(
            bool logicOnly, int gameObjects, int children, int objects, int entities, int slots, bool expected)
        {
            Type policy = typeof(ProductionEntityStressRunner).Assembly.GetType("NTSD.Animation.Rendering.Editor.ProductionEntityStressPopulationPolicy", true);
            MethodInfo evaluate = policy.GetMethod("Evaluate", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.AreEqual(expected, evaluate.Invoke(null,
                new object[] { 1000, gameObjects, children, objects, entities, slots, logicOnly }));
        }

        [NUnit.Framework.TestCase(0, "dispersed1000", 180)]
        [NUnit.Framework.TestCase(1, "combat1000", 180)]
        [NUnit.Framework.TestCase(2, "dispersed1000", 1800)]
        [NUnit.Framework.TestCase(3, "dispersed1000", 1800)]
        [NUnit.Framework.TestCase(4, "combat1000", 1800)]
        [NUnit.Framework.TestCase(5, "combat1000", 1800)]
        public void FixedRequest_PreservesAiCadenceAndBaseline(int index, string action, int samples)
        {
            ProductionEntityStressRequest request = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            NUnit.Framework.Assert.AreEqual(action, request.action);
            NUnit.Framework.Assert.AreEqual(samples, request.sampleTicks);
            NUnit.Framework.Assert.AreEqual(120, request.warmupTicks);
            NUnit.Framework.Assert.AreEqual(1000, request.entityCount);
            NUnit.Framework.Assert.AreEqual("ai", request.inputMode);
            NUnit.Framework.Assert.AreEqual("DataOrientedCanonical", request.aiExecutionProfile);
            NUnit.Framework.Assert.AreEqual("brute", request.formalCollectorMode);
            NUnit.Framework.Assert.AreEqual(2, request.maxCatchUpTicksPerFrame);
            NUnit.Framework.Assert.AreEqual(2, request.maxBacklogTicks);
            NUnit.Framework.Assert.IsFalse(request.simulationOnly);
            NUnit.Framework.Assert.IsFalse(request.useDedicatedSimulationWorker);
            NUnit.Framework.Assert.IsTrue(request.autoStopWhenSampled);
            NUnit.Framework.Assert.IsTrue(request.requireZeroGcAfterWarmup);
            NUnit.Framework.Assert.AreEqual("dispatch", request.soundPresentationMode);
            NUnit.Framework.Assert.AreEqual(0x4E545344u, request.seed);
            Type configType = typeof(ProductionEntityStressRunner).Assembly.GetType(
                "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
            MethodInfo parse = configType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.DoesNotThrow(() => parse.Invoke(null, new object[] { request, Path.GetFullPath(".") }));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        public void RoleCandidateRequest_ChangesOnlyCollectorAndOutput(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index);
            expected.formalCollectorMode = "role";
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH29-ROLE-COLLECTOR-ADMISSION-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-smoke/report.json";
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(index);
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
            Type configType = typeof(ProductionEntityStressRunner).Assembly.GetType(
                "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
            MethodInfo parse = configType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.DoesNotThrow(() => parse.Invoke(null, new object[] { actual, Path.GetFullPath(".") }));
        }

        [NUnit.Framework.Test]
        public void RoleCandidateRequest_RejectsOutsideFixedMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleCandidateRequest(2));
        }

        [NUnit.Framework.TestCase(0)]
        [NUnit.Framework.TestCase(1)]
        [NUnit.Framework.TestCase(2)]
        [NUnit.Framework.TestCase(3)]
        public void RoleFormalRequest_ChangesOnlyCollectorAndOutput(int index)
        {
            ProductionEntityStressRequest expected = BattleOptimizationWindowsAiSuiteEditor.BuildRequest(index + 2);
            expected.formalCollectorMode = "role";
            expected.outputPath = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH30-ROLE-FORMAL-WINDOWS-20261007/windows-01/" +
                index.ToString("D2") + "-" + expected.action + "-formal-" + (index % 2 == 0 ? "01" : "02") + "/report.json";
            ProductionEntityStressRequest actual = BattleOptimizationWindowsAiSuiteEditor.BuildRoleFormalRequest(index);
            NUnit.Framework.Assert.AreEqual(JsonUtility.ToJson(expected), JsonUtility.ToJson(actual));
            NUnit.Framework.Assert.AreEqual(1800, actual.sampleTicks);
            Type configType = typeof(ProductionEntityStressRunner).Assembly.GetType(
                "NTSD.Animation.Rendering.Editor.ProductionEntityStressConfig", true);
            MethodInfo parse = configType.GetMethod("FromRequest", BindingFlags.Static | BindingFlags.NonPublic);
            NUnit.Framework.Assert.DoesNotThrow(() => parse.Invoke(null, new object[] { actual, Path.GetFullPath(".") }));
        }

        [NUnit.Framework.Test]
        public void RoleFormalRequest_RejectsOutsideFixedMatrix()
        {
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleFormalRequest(-1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleOptimizationWindowsAiSuiteEditor.BuildRoleFormalRequest(4));
        }

        [NUnit.Framework.TestCase(0, true)]
        [NUnit.Framework.TestCase(1, false)]
        [NUnit.Framework.TestCase(2, false)]
        [NUnit.Framework.TestCase(3, false)]
        [NUnit.Framework.TestCase(4, false)]
        [NUnit.Framework.TestCase(5, false)]
        [NUnit.Framework.TestCase(6, false)]
        [NUnit.Framework.TestCase(7, false)]
        [NUnit.Framework.TestCase(8, false)]
        [NUnit.Framework.TestCase(9, false)]
        [NUnit.Framework.TestCase(10, false)]
        [NUnit.Framework.TestCase(11, false)]
        [NUnit.Framework.TestCase(12, false)]
        [NUnit.Framework.TestCase(13, false)]
        [NUnit.Framework.TestCase(14, false)]
        [NUnit.Framework.TestCase(15, false)]
        [NUnit.Framework.TestCase(16, false)]
        [NUnit.Framework.TestCase(17, false)]
        [NUnit.Framework.TestCase(18, false)]
        public void RetainedGcFailure_NeverPromotesOrContinuesUnsafeMeasurement(int defect, bool expected)
        {
            var report = new ProductionEntityStressReport
            {
                status = "StoppedWithResidue",
                failure = "ZeroGcGateFailed: retained strict verdict",
                sampledLogicTicks = 1800,
                warmupTicksCompleted = 120,
                zeroGcGatePassed = false,
                harnessValidity = false,
            };
            report.teardown.attempted = true;
            report.teardown.restored = true;
            report.capacityPressure.passed = true;
            int observedWindows = 12;
            int activeAi = 1000;
            int baseRoster = 1000;
            switch (defect)
            {
                case 1: report.status = "Failed"; break;
                case 2: report.sampledLogicTicks = 1799; break;
                case 3: report.warmupTicksCompleted = 119; break;
                case 4: activeAi = 999; break;
                case 5: baseRoster = 999; break;
                case 6: observedWindows = 0; break;
                case 7: report.teardown.restored = false; break;
                case 8: report.capacityPressure.passed = false; break;
                case 9: report.capacityPressure.capacityCriticalDelta = 1; break;
                case 10: report.u6ProductionCanonicalMismatchCount = 1; break;
                case 11: report.aiUnifiedSnapshotExecutionPreCommitFallbackCount = 1; break;
                case 12: report.aiUnifiedSnapshotExecutionPostCommitHardBreachCount = 1; break;
                case 13: report.failure = "CapacityPressureGateFailed: rejected"; break;
                case 14: report.zeroGcGatePassed = true; break;
                case 15: report.teardown.cleanupExceptionCount = 1; break;
                case 16: report.teardown = null; break;
                case 17: report.capacityPressure = null; break;
                case 18: report.u6ProductionDataOrientedCompatibilityFallbackCount = 1; break;
            }
            string originalFailure = report.failure;
            NUnit.Framework.Assert.AreEqual(expected,
                BattleOptimizationWindowsAiSuiteEditor.MayContinueAfterRetainedGcFailure(
                    report, 1800, observedWindows, activeAi, baseRoster));
            NUnit.Framework.Assert.IsFalse(report.harnessValidity, "A retained report remains invalid, not admitted.");
            NUnit.Framework.Assert.AreEqual(originalFailure, report.failure, "The strict failure cannot be overwritten.");
            NUnit.Framework.Assert.AreEqual(defect == 14, report.zeroGcGatePassed);
        }
    }
#endif
}
#endif
