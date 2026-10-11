#if UNITY_EDITOR
using System;
using NTSD.Animation;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    internal sealed class H07InputCostDiagnostics : IDisposable
    {
        internal const string OutputRoot = "artifacts/diagnostics/NTSD-H07-INPUT-COST-DIAG-20261010/windows";
        internal const string FlowOutputRoot = "artifacts/diagnostics/NTSD-H07-INPUT-FLOW-DIAG-20261010/windows";
        internal const string CpuOutputRoot = "artifacts/diagnostics/NTSD-H07-INPUT-SPIKE-CPU-DIAG-20261010/windows";
        private static Tick[] baseline;
        private readonly bool fullFlow;
        private readonly bool cpuDiagnostic;
        private H07WindowsThreadCounter.Snapshot cpuBegin, cpuEnd;
        private long cpuElapsedBegin, cpuElapsedEnd;
        private int gen0Begin, gen1Begin, gen2Begin;
        internal const ulong CpuPhaseMask = (1UL << 5) | (1UL << 7) | (1UL << 21) |
            (1UL << 24) | (1UL << 29) | (1UL << 31) | (1UL << 33);
        private Unity.Profiling.ProfilerMarker[] tickMarkers;
        private int openMarker = -1;
        private readonly ProductionEntityStressRunner runner;
        private readonly SimulationTickDriver driver;
        private readonly SimulationWorld world;
        private readonly BruteForceSceneQuery query;
        private readonly BattleLogicTickGcObserverEditor gc;
        private readonly BattleAiInputDetailDiagnostics detail;
        private readonly bool previousBoundary;
        private readonly int sequence;
        private bool attached, disposed;
        internal readonly Evidence evidence = new Evidence();

        [Serializable]
        internal sealed class Tick
        {
            public int tick, playerInputCount, progressCommits, progressSkips, producerRefreshes, producerSkips;
            public bool steady, actualFlagsValid;
            public ulong fullRuntimeChecksum, rngCalls, crtCalls, synchronizedCalls, synchronizedTableHash;
            public uint rngState, crtState, tableSeed, lastSynchronizedCallSite;
            public int synchronizedCounter, synchronizedIndex, fallbackAttack, fallbackBody, inert, fallbackTotal;
            public long driverAllocationEvents, driverAllocationRawValue;
            public bool driverScopeValid, profilerEnabled;
            public int unityFrame;
            public long[] slotVisits = new long[42];
            public long[] elapsed = new long[42];
            public long[] calls = new long[42];
            public ulong[] phaseThreadCycles = new ulong[42];
            public long[] phaseThreadCyclePairs = new long[42];
            public ulong driverThreadCycles, driverThreadCpu100ns;
            public long driverEnvelopeElapsed;
            public int gen0Collections, gen1Collections, gen2Collections;
        }

        [Serializable]
        internal sealed class Evidence
        {
            public string scope = "Per completed Driver tick; checksum and counter capture after Driver timing/GC scope. Warm1-120; steady121-300. Boundary toggle only; other AI fine timers remain equal and ON.";
            public long stopwatchFrequency = System.Diagnostics.Stopwatch.Frequency;
            public bool boundaryEnabled, settingsRestored, aborted, fullFlowDiagnostic;
            public int sequenceIndex, ticks, parityMismatches, actualFlagFailures;
            public Tick[] observations = new Tick[300];
            public bool cpuDiagnostic, phaseCpuEnabled;
            public H07WindowsThreadCounter.Calibration cpuCalibration;
        }

        internal static void ResetBaseline() { baseline = null; }

        internal static ProductionEntityStressRequest BuildRequest(int index)
        {
            if (index < 0 || index >= 4) throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(3);
            request.outputPath = OutputRoot + "/" + index.ToString("D2") +
                (index == 1 || index == 2 ? "-combat1000-on/report.json" : "-combat1000-off/report.json");
            return request;
        }

        internal static ProductionEntityStressRequest BuildFlowRequest(int index)
        {
            if (index != 0) throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(3);
            request.outputPath = FlowOutputRoot + "/00-combat1000-input-on/report.json";
            return request;
        }

        internal static ProductionEntityStressRequest BuildCpuRequest(int index)
        {
            if (index < 0 || index > 1) throw new ArgumentOutOfRangeException(nameof(index));
            ProductionEntityStressRequest request = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(3);
            request.outputPath = CpuOutputRoot + "/" + index.ToString("D2") +
                (index == 0 ? "-combat1000-phase-cpu-off/report.json" : "-combat1000-phase-cpu-on/report.json");
            return request;
        }

        internal H07InputCostDiagnostics(ProductionEntityStressRunner runner, SimulationTickDriver driver,
            BruteForceSceneQuery query, BattleLogicTickGcObserverEditor gc, int sequence, bool fullFlow = false,
            bool cpuDiagnostic = false)
        {
            this.fullFlow = fullFlow;
            this.cpuDiagnostic = cpuDiagnostic;
            this.runner = runner;
            this.driver = driver;
            this.query = query;
            this.gc = gc;
            this.sequence = sequence;
            world = driver.World;
            detail = world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (detail == null || !detail.Enabled || BattleAiInputDetailDiagnostics.PhaseCount != 42 ||
                runner.DriverTickObserverAttachedForDiagnostics || runner.Report.logicTicksExecuted != 0 ||
                world.EnableCharacterInputBoundaryTimingForDiagnostics || detail.ThreadCycleCounterAttachedForDiagnostics)
                throw new InvalidOperationException("Input observation requires an unowned first-tick boundary and fixed AI timing schema.");
            evidence.sequenceIndex = sequence;
            evidence.fullFlowDiagnostic = fullFlow;
            evidence.boundaryEnabled = fullFlow || cpuDiagnostic || sequence == 1 || sequence == 2;
            evidence.cpuDiagnostic = cpuDiagnostic;
            evidence.phaseCpuEnabled = cpuDiagnostic && sequence == 1;
            if (cpuDiagnostic)
            {
                if (fullFlow || sequence < 0 || sequence > 1 || UnityEngine.Profiling.Profiler.enabled)
                    throw new InvalidOperationException("CPU diagnosis requires its own two unprofiled windows.");
                evidence.scope = "CPU_DIAGNOSTIC_NOT_CERTIFICATE; same full-input elapsed instrumentation, OFF then ON seven raw thread-cycle sub-phases; no cycles-to-ms conversion; Driver CPU envelope includes GC observer overhead.";
                evidence.cpuCalibration = H07WindowsThreadCounter.Calibrate();
            }
            if (fullFlow)
            {
                evidence.scope = "INSTRUMENTED_FULL_INPUT_FLOW_DIAGNOSTIC_ONLY; one 120+180 window, all 42 input slots; all Driver allocation events retained; no performance/zero-GC certificate.";
                BattleOptimizationCpuGcCaptureEditor.RequireAvailable();
                tickMarkers = new Unity.Profiling.ProfilerMarker[300];
                for (int i = 0; i < tickMarkers.Length; i++)
                {
                    tickMarkers[i] = new Unity.Profiling.ProfilerMarker("NTSD.H07.InputFlow.Driver.Tick" + (i + 1).ToString("D3"));
                    tickMarkers[i].Begin(); tickMarkers[i].End();
                }
            }
            for (int i = 0; i < 300; i++) evidence.observations[i] = new Tick();
            previousBoundary = world.EnableCharacterInputBoundaryTimingForDiagnostics;
            world.EnableCharacterInputBoundaryTimingForDiagnostics = evidence.boundaryEnabled;
            try
            {
                if (evidence.phaseCpuEnabled)
                    detail.SetThreadCycleCounterForDiagnostics(H07WindowsThreadCounter.ReadCycles, CpuPhaseMask);
                if (fullFlow)
                {
                    System.IO.Directory.CreateDirectory(System.IO.Path.GetFullPath(FlowOutputRoot + "/capture"));
                    BattleOptimizationCpuGcCaptureEditor.Arm(System.IO.Path.GetFullPath(FlowOutputRoot + "/capture"));
                }
                runner.ConfigureDriverTickObserverForDiagnostics(BeginDriver, EndDriver, Accept);
                UnityEditor.AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
                attached = true;
            }
            catch { Dispose(); throw; }
        }

        private void BeforeReload() { Dispose(); }

        private void BeginDriver()
        {
            if (cpuDiagnostic)
            {
                cpuBegin = H07WindowsThreadCounter.Read();
                gen0Begin = GC.CollectionCount(0); gen1Begin = GC.CollectionCount(1); gen2Begin = GC.CollectionCount(2);
                cpuElapsedBegin = System.Diagnostics.Stopwatch.GetTimestamp();
            }
            if (fullFlow)
            {
                if (openMarker != -1 || evidence.ticks >= 300) throw new InvalidOperationException("Input tick marker sequence invalid.");
                openMarker = evidence.ticks;
                tickMarkers[openMarker].Begin();
            }
            gc.BeginDriverStep();
        }

        private void EndDriver(bool stepped)
        {
            try { gc.EndDriverStep(stepped); }
            finally
            {
                if (cpuDiagnostic)
                {
                    cpuElapsedEnd = System.Diagnostics.Stopwatch.GetTimestamp();
                    cpuEnd = H07WindowsThreadCounter.Read();
                }
                if (openMarker >= 0) { tickMarkers[openMarker].End(); openMarker = -1; }
            }
        }

        private void Accept(int tick, bool steady)
        {
            gc.AcceptCompletedTick(tick, steady);
            if (tick != evidence.ticks + 1 || tick > 300)
                throw new InvalidOperationException("Input observation lost the fixed 300-tick sequence.");
            Tick row = evidence.observations[tick - 1];
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
            row.progressCommits = world.LastCharacterInputProgressCommitCountForDiagnostics;
            row.progressSkips = world.LastCharacterInputProgressCommitSkipCountForDiagnostics;
            row.producerRefreshes = world.LastCharacterInputProducerSnapshotRefreshCountForDiagnostics;
            row.producerSkips = world.LastCharacterInputProducerSnapshotReuseSkipCountForDiagnostics;
            row.driverAllocationEvents = gc.LastCompletedScopeForDiagnostics.allocationEvents;
            row.driverAllocationRawValue = gc.LastCompletedScopeForDiagnostics.rawValueTotal;
            row.driverScopeValid = gc.LastCompletedScopeForDiagnostics.valid && gc.LastCompletedScopeForDiagnostics.calibratedBefore &&
                !gc.LastCompletedScopeForDiagnostics.saturated && !gc.LastCompletedScopeForDiagnostics.wrapped;
            row.unityFrame = UnityEngine.Time.frameCount;
            row.profilerEnabled = UnityEngine.Profiling.Profiler.enabled;
            if (cpuDiagnostic)
            {
                if (cpuEnd.cycles < cpuBegin.cycles || cpuEnd.cpu100ns < cpuBegin.cpu100ns)
                    throw new InvalidOperationException("Thread CPU counters regressed.");
                row.driverThreadCycles = cpuEnd.cycles - cpuBegin.cycles;
                row.driverThreadCpu100ns = cpuEnd.cpu100ns - cpuBegin.cpu100ns;
                row.driverEnvelopeElapsed = cpuElapsedEnd - cpuElapsedBegin;
                row.gen0Collections = GC.CollectionCount(0) - gen0Begin;
                row.gen1Collections = GC.CollectionCount(1) - gen1Begin;
                row.gen2Collections = GC.CollectionCount(2) - gen2Begin;
            }
            query.GetLastRoleAwareFallbackOrdinalCountsForSelfCheck(out _, out row.fallbackAttack, out _, out row.fallbackBody);
            row.inert = query.LastRoleAwareInertParticipantCountForDiagnostics;
            row.fallbackTotal = query.LastFormalFallbackParticipantCountForDiagnostics;
            row.actualFlagsValid = detail.Enabled &&
                world.EnableCharacterInputBoundaryTimingForDiagnostics == evidence.boundaryEnabled &&
                !world.EnableCharacterInputProducerSnapshotReuseForDiagnostics &&
                !world.AiUnifiedSnapshotSourceCoordinateRefreshForDiagnostics &&
                !query.EnableRoleRecordCostDiagnostics && !query.EnableRoleBranchTimingForDiagnostics &&
                !query.EnableRoleCapacityRecordGuardForDiagnostics && !query.EnableRoleSnapshotFrameReuseForDiagnostics &&
                !query.EnableRoleRejectedGroupSnapshotSkipForDiagnostics && !query.EnableRoleEndpointSnapshotReuseForDiagnostics &&
                query.LastFormalCollectorModeForDiagnostics == CollisionFormalCollectorMode.ForceRoleAware &&
                !query.LastFormalCollectionAbortedForDiagnostics && row.fallbackAttack == 0 && row.fallbackBody == 0 &&
                row.inert >= 0 && row.fallbackTotal == row.inert && steady == (tick > 120);
            for (int phase = 0; phase < 42; phase++)
            {
                row.elapsed[phase] = detail.GetLastElapsedTimestampTicks((BattleAiInputDetailPhase)phase);
                row.calls[phase] = detail.GetLastCallCount((BattleAiInputDetailPhase)phase);
                row.slotVisits[phase] = detail.GetLastSlotVisitCount((BattleAiInputDetailPhase)phase);
                row.phaseThreadCycles[phase] = detail.GetLastThreadCyclesForDiagnostics((BattleAiInputDetailPhase)phase);
                row.phaseThreadCyclePairs[phase] = detail.GetLastThreadCyclePairsForDiagnostics((BattleAiInputDetailPhase)phase);
                if (!evidence.boundaryEnabled && phase >= 30) row.actualFlagsValid &= row.elapsed[phase] == 0;
            }
            if (cpuDiagnostic) row.actualFlagsValid &= !row.profilerEnabled && detail.ThreadCycleInvalidsForDiagnostics == 0 &&
                detail.ThreadCycleCounterAttachedForDiagnostics == evidence.phaseCpuEnabled;
            if (!row.actualFlagsValid) evidence.actualFlagFailures++;
            if (sequence != 0 && (baseline == null || !EqualState(baseline[tick - 1], row))) evidence.parityMismatches++;
            evidence.ticks++;
            if (fullFlow && tick == 124) BattleOptimizationCpuGcCaptureEditor.Observe(runner.Report);
        }

        internal bool Finish(bool aborted)
        {
            evidence.aborted = aborted;
            Dispose();
            bool valid = !aborted && evidence.ticks == 300 && evidence.actualFlagFailures == 0 &&
                evidence.parityMismatches == 0 && evidence.settingsRestored;
            if (sequence == 0 && valid) baseline = evidence.observations;
            return valid;
        }

        private static bool EqualState(Tick a, Tick b) => a != null && a.tick == b.tick && a.steady == b.steady &&
            a.playerInputCount == b.playerInputCount && a.fullRuntimeChecksum == b.fullRuntimeChecksum &&
            a.rngState == b.rngState && a.rngCalls == b.rngCalls && a.crtState == b.crtState && a.crtCalls == b.crtCalls &&
            a.tableSeed == b.tableSeed && a.synchronizedCounter == b.synchronizedCounter &&
            a.synchronizedIndex == b.synchronizedIndex && a.synchronizedCalls == b.synchronizedCalls &&
            a.lastSynchronizedCallSite == b.lastSynchronizedCallSite && a.synchronizedTableHash == b.synchronizedTableHash;


        [UnityEditor.MenuItem("NTSD/Validation/Optimization/H07 Parse Retained Input Flow Capture")]
        private static void ParseRetainedFlowCapture()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling || UnityEditor.EditorApplication.isUpdating)
                throw new InvalidOperationException("Parsing requires idle Editor; no recapture.");
            string captureRoot = System.IO.Path.GetFullPath(FlowOutputRoot + "/capture");
            string output = System.IO.Path.Combine(captureRoot, "cpu-gc-recovery.json");
            if (System.IO.File.Exists(output) || System.IO.File.Exists(System.IO.Path.Combine(captureRoot, "cpu-gc-samples-recovered.json")))
                throw new InvalidOperationException("Retained parsing outputs must be fresh.");
            Type capture = typeof(BattleOptimizationCpuGcCaptureEditor);
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            Type stateType = capture.GetNestedType("CaptureState", System.Reflection.BindingFlags.NonPublic);
            object recovered = UnityEngine.JsonUtility.FromJson(System.IO.File.ReadAllText(System.IO.Path.Combine(captureRoot, "cpu-gc-state.json")), stateType);
            object before = capture.GetMethod("ReadSettings", flags).Invoke(null, null);
            stateType.GetField("before").SetValue(recovered, before);
            stateType.GetField("capturedFrames").SetValue(recovered, 0);
            stateType.GetField("validMainThreadFrames").SetValue(recovered, 0);
            stateType.GetField("stopReason").SetValue(recovered, "H07 appended retained raw only; select actual steady Driver allocation frames; no recapture or history clear");
            try
            {
                int previousLast = UnityEditorInternal.ProfilerDriver.lastFrameIndex;
                if (!UnityEditorInternal.ProfilerDriver.LoadProfile(System.IO.Path.Combine(captureRoot, "cpu-gc.raw"), true))
                    throw new InvalidOperationException("Retained raw import failed.");
                int first = previousLast < 0 ? UnityEditorInternal.ProfilerDriver.firstFrameIndex : UnityEditorInternal.ProfilerDriver.GetNextFrameIndex(previousLast);
                int count = 0, scanned = 0;
                int[] frames = (int[])stateType.GetField("frameIndices").GetValue(recovered);
                for (int frame = first; frame >= 0; frame = UnityEditorInternal.ProfilerDriver.GetNextFrameIndex(frame))
                {
                    if (++scanned > 300) throw new InvalidOperationException("Retained frame scan limit reached.");
                    bool target = false;
                    using (var view = UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(frame, 0))
                    {
                        if (!view.valid) continue;
                        for (int sample = 0; sample < view.sampleCount && !target; sample++)
                        {
                            string name = view.GetSampleName(sample);
                            if (!name.StartsWith("NTSD.H07.InputFlow.Driver.Tick", StringComparison.Ordinal)) continue;
                            if (!int.TryParse(name.Substring(name.Length - 3), out int tick) || tick <= 120) continue;
                            int end = sample + view.GetSampleChildrenCountRecursive(sample);
                            for (int child = sample + 1; child <= end; child++)
                                if (view.GetSampleName(child) == "GC.Alloc") { target = true; break; }
                        }
                    }
                    if (target)
                    {
                        if (count == frames.Length) throw new InvalidOperationException("Allocation frame selection capacity reached.");
                        frames[count++] = frame;
                    }
                }
                stateType.GetField("capturedFrames").SetValue(recovered, count);
                capture.GetMethod("ExportSamples", flags).Invoke(null, new object[] { recovered, "cpu-gc-samples-recovered.json" });
                stateType.GetField("status").SetValue(recovered, count > 0 ? "RETAINED_ALLOCATION_FRAMES_PARSED" : "NO_STEADY_ALLOCATION_FRAME");
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
                stateType.GetField("settingsRestored").SetValue(recovered, UnityEngine.JsonUtility.ToJson(before) == UnityEngine.JsonUtility.ToJson(after));
                using (var stream = new System.IO.FileStream(output, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
                using (var writer = new System.IO.StreamWriter(stream)) writer.Write(UnityEngine.JsonUtility.ToJson(recovered, true));
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            UnityEditor.AssemblyReloadEvents.beforeAssemblyReload -= BeforeReload;
            try
            {
                if (attached) runner.ConfigureDriverTickObserverForDiagnostics(null, null, null);
                if (openMarker >= 0) { tickMarkers[openMarker].End(); openMarker = -1; }
                if (fullFlow) BattleOptimizationCpuGcCaptureEditor.FinishAndRestore("input-flow-observer-dispose");
            }
            finally
            {
                if (cpuDiagnostic) detail.SetThreadCycleCounterForDiagnostics(null);
                world.EnableCharacterInputBoundaryTimingForDiagnostics = previousBoundary;
                evidence.settingsRestored = world.EnableCharacterInputBoundaryTimingForDiagnostics == previousBoundary &&
                    (!cpuDiagnostic || !detail.ThreadCycleCounterAttachedForDiagnostics);
                disposed = true;
            }
        }
    }

    internal static class H07WindowsThreadCounter
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool QueryThreadCycleTime(IntPtr thread, out ulong cycles);

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetThreadTimes(IntPtr thread, out ulong creation, out ulong exit,
            out ulong kernel, out ulong user);

        internal struct Snapshot { internal ulong cycles, cpu100ns; }

        [Serializable]
        internal sealed class Calibration
        {
            public bool monotonic;
            public int emptyPairs;
            public long emptyElapsedMeanTimestampTicks, emptyElapsedMaxTimestampTicks;
            public ulong emptyCycleMean, emptyCycleMax;
            public string units = "elapsed=Stopwatch ticks; CPU time=GetThreadTimes 100ns units (resolution not asserted); cycles=raw QueryThreadCycleTime, never converted to ms";
        }

        internal static ulong ReadCycles()
        {
            if (!QueryThreadCycleTime(new IntPtr(-2), out ulong cycles))
                throw new InvalidOperationException("QueryThreadCycleTime failed: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
            return cycles;
        }

        internal static Snapshot Read()
        {
            ulong cycles = ReadCycles();
            if (!GetThreadTimes(new IntPtr(-2), out _, out _, out ulong kernel, out ulong user))
                throw new InvalidOperationException("GetThreadTimes failed: " + System.Runtime.InteropServices.Marshal.GetLastWin32Error());
            return new Snapshot { cycles = cycles, cpu100ns = kernel + user };
        }

        internal static Calibration Calibrate()
        {
            Snapshot before = Read();
            System.Threading.Thread.SpinWait(100000);
            Snapshot after = Read();
            if (after.cycles <= before.cycles || after.cpu100ns < before.cpu100ns)
                throw new InvalidOperationException("Thread CPU counter calibration failed.");
            var result = new Calibration { monotonic = true, emptyPairs = 64 };
            var recorder = new BattleAiInputDetailDiagnostics();
            recorder.SetEnabled(true);
            recorder.SetThreadCycleCounterForDiagnostics(ReadCycles, 1UL << 7);
            for (int i = 0; i < result.emptyPairs; i++)
            {
                recorder.BeginTick(i);
                recorder.BeginPhase(BattleAiInputDetailPhase.ComboUpdate);
                recorder.EndPhase(BattleAiInputDetailPhase.ComboUpdate);
                long elapsed = recorder.GetLastElapsedTimestampTicks(BattleAiInputDetailPhase.ComboUpdate);
                ulong cycles = recorder.GetLastThreadCyclesForDiagnostics(BattleAiInputDetailPhase.ComboUpdate);
                result.emptyElapsedMeanTimestampTicks += elapsed;
                result.emptyCycleMean += cycles;
                result.emptyElapsedMaxTimestampTicks = Math.Max(result.emptyElapsedMaxTimestampTicks, elapsed);
                result.emptyCycleMax = Math.Max(result.emptyCycleMax, cycles);
            }
            recorder.SetThreadCycleCounterForDiagnostics(null);
            result.emptyElapsedMeanTimestampTicks /= result.emptyPairs;
            result.emptyCycleMean /= (ulong)result.emptyPairs;
            return result;
        }
    }

    internal static class H07RetainedInputPeakDiagnostics
    {
        [Serializable]
        private sealed class Row
        {
            public int tick, frame, allocationEvents;
            public double driverMs, inputMs, entityInputMs, jitMs, collectMs;
        }
        [Serializable]
        private sealed class Evidence
        {
            public string status = "STARTED", error = "";
            public bool settingsRestored;
            public string scope = "Same retained raw append-only import; main-thread Driver samples only; inclusive elapsed timings, not CPU time or complete OS scheduling trace";
            public System.Collections.Generic.List<Row> ticks = new System.Collections.Generic.List<Row>(300);
        }
        [UnityEditor.MenuItem("NTSD/Validation/Optimization/H07 Inspect Retained Input Peaks")]
        private static void Inspect()
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode || UnityEditor.EditorApplication.isCompiling || UnityEngine.Profiling.Profiler.enabled)
                throw new InvalidOperationException("Retained peak inspection requires idle, nonrecording Editor.");
            string output = System.IO.Path.GetFullPath(H07InputCostDiagnostics.CpuOutputRoot + "/../retained-raw-ticks.json");
            if (System.IO.File.Exists(output)) throw new InvalidOperationException("Peak evidence must be fresh.");
            const System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic;
            Type capture = typeof(BattleOptimizationCpuGcCaptureEditor);
            object before = capture.GetMethod("ReadSettings", flags).Invoke(null, null);
            var result = new Evidence();
            try
            {
                int previousLast = UnityEditorInternal.ProfilerDriver.lastFrameIndex;
                if (!UnityEditorInternal.ProfilerDriver.LoadProfile(System.IO.Path.GetFullPath(H07InputCostDiagnostics.FlowOutputRoot + "/capture/cpu-gc.raw"), true))
                    throw new InvalidOperationException("Retained profile import failed.");
                int first = previousLast < 0 ? UnityEditorInternal.ProfilerDriver.firstFrameIndex : UnityEditorInternal.ProfilerDriver.GetNextFrameIndex(previousLast);
                int scanned = 0;
                for (int frame = first; frame >= 0; frame = UnityEditorInternal.ProfilerDriver.GetNextFrameIndex(frame))
                {
                    if (++scanned > 600) throw new InvalidOperationException("Retained scan bound exceeded.");
                    using (var view = UnityEditorInternal.ProfilerDriver.GetRawFrameDataView(frame, 0))
                    {
                        if (!view.valid) continue;
                        for (int sample = 0; sample < view.sampleCount; sample++)
                        {
                            string name = view.GetSampleName(sample);
                            if (!name.StartsWith("NTSD.H07.InputFlow.Driver.Tick", StringComparison.Ordinal)) continue;
                            if (!int.TryParse(name.Substring(name.Length - 3), out int tick) || tick <= 120) continue;
                            if (result.ticks.Count >= 300) throw new InvalidOperationException("Retained tick capacity exceeded.");
                            var row = new Row { tick = tick, frame = frame, driverMs = view.GetSampleTimeMs(sample) };
                            int end = sample + view.GetSampleChildrenCountRecursive(sample);
                            for (int child = sample + 1; child <= end; child++)
                            {
                                string childName = view.GetSampleName(child);
                                double ms = view.GetSampleTimeMs(child);
                                if (childName == "NTSD.BattleTick.CharacterInput") row.inputMs += ms;
                                if (childName == "NTSD.BattleTick.Detail.CharacterInput/EntityInputPass") row.entityInputMs += ms;
                                if (childName == "Mono.JIT") row.jitMs += ms;
                                if (childName == "GC.Alloc") row.allocationEvents++;
                                if (childName == "GC.Collect") row.collectMs += ms;
                            }
                            result.ticks.Add(row);
                            sample = end;
                        }
                    }
                }
                result.status = "RETAINED_MAIN_THREAD_TICKS_EXPORTED";
            }
            catch (Exception exception) { result.status = "PARTIAL"; result.error = exception.ToString(); }
            finally
            {
                capture.GetMethod("RestoreSettings", flags).Invoke(null, new[] { before });
                object after = capture.GetMethod("ReadSettings", flags).Invoke(null, null);
                result.settingsRestored = UnityEngine.JsonUtility.ToJson(before) == UnityEngine.JsonUtility.ToJson(after);
                using (var stream = new System.IO.FileStream(output, System.IO.FileMode.CreateNew, System.IO.FileAccess.Write))
                using (var writer = new System.IO.StreamWriter(stream)) writer.Write(UnityEngine.JsonUtility.ToJson(result, true));
            }
        }
    }

#if UNITY_INCLUDE_TESTS
    public sealed class H07InputGuardDiagnosticTests
    {
        private static NTSD.Animation.LF2Objects.LF2Character Register(SimulationWorld world, int id)
        {
            var method = typeof(NTSD.Test.CharacterInputLiveSlotLoopEditorTests).GetMethod("RegisterCharacter",
                System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            return (NTSD.Animation.LF2Objects.LF2Character)method.Invoke(null, new object[] { world, 0, id });
        }

        [NUnit.Framework.Test]
        public void CanonicalGuardMatchesProgressForBoundReleasedAndReplacedOwners()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
            var first = Register(world, 7001);
            NUnit.Framework.Assert.That(world.CharacterInputWriter.CanEvaluateCanonicalDecision(first.Runtime), IsTrue());
            NUnit.Framework.Assert.That(world.CharacterInputWriter.TryCaptureProgressState(first.Runtime, out _), IsTrue());
            world.Unregister(first);
            NUnit.Framework.Assert.That(world.CharacterInputWriter.CanEvaluateCanonicalDecision(first.Runtime), IsFalse());
            NUnit.Framework.Assert.That(world.CharacterInputWriter.TryCaptureProgressState(first.Runtime, out _), IsFalse());
            var second = Register(world, 7002);
            NUnit.Framework.Assert.That(world.CharacterInputWriter.CanEvaluateCanonicalDecision(second.Runtime), IsTrue());
            NUnit.Framework.Assert.That(world.CharacterInputWriter.TryCaptureProgressState(second.Runtime, out _), IsTrue());
            world.Unregister(second);
        }

        [NUnit.Framework.Test]
        public void CanonicalGuardRejectsNullAndForeignObjectWithBorrowedSlot()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.DataOrientedCanonical);
            var owner = Register(world, 7003);
            var foreign = new NTSDEntityRuntime { SlotIndex = owner.Runtime.SlotIndex };
            NUnit.Framework.Assert.That(world.CharacterInputWriter.CanEvaluateCanonicalDecision(null), IsFalse());
            NUnit.Framework.Assert.That(world.CharacterInputWriter.TryCaptureProgressState(null, out _), IsFalse());
            NUnit.Framework.Assert.That(world.CharacterInputWriter.CanEvaluateCanonicalDecision(foreign), IsFalse());
            NUnit.Framework.Assert.That(world.CharacterInputWriter.TryCaptureProgressState(foreign, out _), IsFalse());
            world.Unregister(owner);
        }

        [NUnit.Framework.Test]
        public void LegacyProfileNeedsItsOwnGateEvenWhenOwnerIsValid()
        {
            var world = new SimulationWorld();
            world.ConfigureAiExecutionProfile(BattleAiExecutionProfile.LegacyCanonical);
            var owner = Register(world, 7004);
            NUnit.Framework.Assert.That(world.CharacterInputWriter.CanEvaluateCanonicalDecision(owner.Runtime), IsTrue());
            NUnit.Framework.Assert.That(world.CharacterInputWriter.TryCaptureProgressState(owner.Runtime, out _), IsFalse());
            world.Unregister(owner);
        }

        [NUnit.Framework.Test]
        public void FlowRequestIsOneOriginalFixtureWithOnlyFreshOutput()
        {
            var actual = H07InputCostDiagnostics.BuildFlowRequest(0);
            var expected = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(3);
            actual.outputPath = expected.outputPath;
            NUnit.Framework.Assert.That(UnityEngine.JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(UnityEngine.JsonUtility.ToJson(expected)));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => H07InputCostDiagnostics.BuildFlowRequest(1));
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => H07InputCostDiagnostics.BuildFlowRequest(-1));
        }

        private static NUnit.Framework.Constraints.TrueConstraint IsTrue() => NUnit.Framework.Is.True;
        private static NUnit.Framework.Constraints.FalseConstraint IsFalse() => NUnit.Framework.Is.False;

        [NUnit.Framework.Test]
        public void ThreadCycleRecorder_NestsMasksAndResetsWithoutChangingDefault()
        {
            var recorder = new BattleAiInputDetailDiagnostics();
            recorder.SetEnabled(true);
            ulong counter = 0;
            recorder.SetThreadCycleCounterForDiagnostics(() => counter += 10, (1UL << 5) | (1UL << 24));
            recorder.BeginTick(1);
            recorder.BeginPhase(BattleAiInputDetailPhase.RemainingAiDecision);
            recorder.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalNearestSearch);
            recorder.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalNearestSearch);
            recorder.EndPhase(BattleAiInputDetailPhase.RemainingAiDecision);
            NUnit.Framework.Assert.That(recorder.GetLastThreadCyclesForDiagnostics(BattleAiInputDetailPhase.RemainingAiDecision), NUnit.Framework.Is.EqualTo(30));
            NUnit.Framework.Assert.That(recorder.GetLastThreadCyclesForDiagnostics(BattleAiInputDetailPhase.IndexedCanonicalNearestSearch), NUnit.Framework.Is.EqualTo(10));
            NUnit.Framework.Assert.That(recorder.GetLastThreadCyclePairsForDiagnostics(BattleAiInputDetailPhase.RemainingAiDecision), NUnit.Framework.Is.EqualTo(1));
            recorder.BeginTick(2);
            NUnit.Framework.Assert.That(recorder.GetLastThreadCyclesForDiagnostics(BattleAiInputDetailPhase.RemainingAiDecision), NUnit.Framework.Is.Zero);
            recorder.BeginPhase(BattleAiInputDetailPhase.ComboUpdate);
            recorder.EndPhase(BattleAiInputDetailPhase.ComboUpdate);
            NUnit.Framework.Assert.That(counter, NUnit.Framework.Is.EqualTo(40));
            recorder.SetThreadCycleCounterForDiagnostics(null);
            recorder.BeginPhase(BattleAiInputDetailPhase.RemainingAiDecision);
            recorder.EndPhase(BattleAiInputDetailPhase.RemainingAiDecision);
            NUnit.Framework.Assert.That(counter, NUnit.Framework.Is.EqualTo(40));
            NUnit.Framework.Assert.That(recorder.ThreadCycleCounterAttachedForDiagnostics, IsFalse());
        }

        [NUnit.Framework.Test]
        public void ThreadCycleRecorder_RejectsMidPhaseOwnerChangeAndCountsRegressions()
        {
            var recorder = new BattleAiInputDetailDiagnostics();
            recorder.SetEnabled(true);
            ulong counter = 110;
            recorder.SetThreadCycleCounterForDiagnostics(() => counter -= 10, 1UL << 7);
            recorder.BeginTick(1);
            recorder.BeginPhase(BattleAiInputDetailPhase.ComboUpdate);
            NUnit.Framework.Assert.Throws<InvalidOperationException>(() => recorder.SetThreadCycleCounterForDiagnostics(null));
            recorder.EndPhase(BattleAiInputDetailPhase.ComboUpdate);
            NUnit.Framework.Assert.That(recorder.ThreadCycleInvalidsForDiagnostics, NUnit.Framework.Is.EqualTo(1));
            recorder.SetThreadCycleCounterForDiagnostics(null);
            recorder.BeginTick(2);
            NUnit.Framework.Assert.That(recorder.ThreadCycleInvalidsForDiagnostics, NUnit.Framework.Is.Zero);
        }

        [NUnit.Framework.Test]
        public void CpuRequest_ContainsOnlyTwoOriginalFixtureWindows()
        {
            for (int index = 0; index < 2; index++)
            {
                var actual = H07InputCostDiagnostics.BuildCpuRequest(index);
                var expected = BattleOptimizationWindowsAiSuiteEditor.BuildScopedRoleAdmissionRequest(3);
                actual.outputPath = expected.outputPath;
                NUnit.Framework.Assert.That(UnityEngine.JsonUtility.ToJson(actual), NUnit.Framework.Is.EqualTo(UnityEngine.JsonUtility.ToJson(expected)));
            }
            NUnit.Framework.Assert.Throws<ArgumentOutOfRangeException>(() => H07InputCostDiagnostics.BuildCpuRequest(2));
        }

        [NUnit.Framework.Test]
        public void WindowsThreadCounter_CalibratesOutsideBusinessExecution()
        {
            var result = H07WindowsThreadCounter.Calibrate();
            NUnit.Framework.Assert.That(result.monotonic, IsTrue());
            NUnit.Framework.Assert.That(result.emptyPairs, NUnit.Framework.Is.EqualTo(64));
            NUnit.Framework.Assert.That(result.emptyCycleMean, NUnit.Framework.Is.GreaterThan(0));
        }
    }
#endif
}
#endif
