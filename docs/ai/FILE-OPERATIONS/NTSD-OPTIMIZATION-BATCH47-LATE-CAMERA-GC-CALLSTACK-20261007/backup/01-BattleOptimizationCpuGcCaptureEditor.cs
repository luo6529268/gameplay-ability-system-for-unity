#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation.Rendering.Editor;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Profiling;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Profiling;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class BattleOptimizationCpuGcCaptureEditor
    {
        private const int CaptureFrameCount = 8;
        private const int ThreadScanLimit = 128;
        private const string CameraBeginMarkerName = "NTSD.Optimization.H11.CameraBeginBoundary";
        private const string CameraEndMarkerName = "NTSD.Optimization.H11.CameraEndBoundary";
        private static readonly ProfilerMarker CameraBeginMarker = new ProfilerMarker(CameraBeginMarkerName);
        private static readonly ProfilerMarker CameraEndMarker = new ProfilerMarker(CameraEndMarkerName);
        private static CaptureState state;

        [Serializable]
        private sealed class ProfilerSettings
        {
            public bool enabled;
            public bool driverEnabled;
            public bool binary;
            public string logFile;
            public bool allocationCallstacks;
            public bool cpuArea;
            public bool profileEditor;
            public bool deepProfiling;
            public int memoryRecordMode;
        }

        [Serializable]
        private sealed class CaptureState
        {
            public string outputRoot;
            public string status = "ARMED";
            public string stopReason;
            public string error;
            public bool recording;
            public bool finishPending;
            public bool settingsRestored;
            public int startLastFrame;
            public int connectionId = -1;
            public int capturedFrames;
            public int[] frameIndices = new int[CaptureFrameCount];
            public int warmupTicks;
            public int sampleTickAtStart;
            public int activeAiAtStart;
            public int baseRosterAtStart;
            public double deadline;
            public ProfilerSettings before;
            public ProfilerSettings after;
            public int validMainThreadFrames;
            public int exportedThreadFrames;
            public bool threadScanLimitReached;
            public bool cameraEnvelopeMode;
            public bool cameraEnvelopeOpen;
            public bool cameraBoundariesValid = true;
            public int completedCameras;
            public CameraBoundary[] cameraBoundaries;
            public int importedFirstFrame = -1;
            public int importedLastFrame = -1;
            public int exportedCameraEnvelopes;
            public bool allocationCountsMatch;
            public bool allocationCallstacksPresent;
        }

        [Serializable]
        private struct CameraBoundary
        {
            public int ordinal;
            public int beginUnityFrame;
            public int endUnityFrame;
            public int beginLogicTick;
            public int endLogicTick;
            public long allocationEvents;
            public bool calibratedScopeValid;
        }

        [Serializable]
        private sealed class CameraSamplesReport
        {
            public string evidence = "INSTRUMENTED_CALLSITE_DIAGNOSTIC_ONLY; profiler indices are not Unity frame numbers; same-frame point pairs follow camera boundary order";
            public string beginMarker = CameraBeginMarkerName;
            public string endMarker = CameraEndMarkerName;
            public CameraEnvelopeSamples[] envelopes;
        }

        [Serializable]
        private sealed class CameraEnvelopeSamples
        {
            public CameraBoundary boundary;
            public int profilerFrameIndex;
            public int beginMarkerSampleIndex;
            public int endMarkerSampleIndex;
            public double beginMarkerEndMs;
            public double endMarkerStartMs;
            public string threadName;
            public Sample[] allocations;
        }

        [Serializable]
        private sealed class SamplesReport
        {
            public string evidence = "INSTRUMENTED_CAPTURE_ONLY; inclusive parent/child times must not be summed";
            public ThreadSamples[] threads;
        }

        [Serializable]
        private sealed class ThreadSamples
        {
            public int frameIndex;
            public int threadIndex;
            public string threadGroup;
            public string threadName;
            public float frameTimeMs;
            public Sample[] samples;
        }

        [Serializable]
        private sealed class Sample
        {
            public int index;
            public string name;
            public float inclusiveMs;
            public double startMs;
            public int children;
            public int recursiveChildren;
            public int metadataCount;
            public long gcAllocBytes = -1;
            public Callsite[] gcCallstack;
        }

        [Serializable]
        private sealed class Callsite
        {
            public string address;
            public string method;
            public string file;
            public uint line;
        }

        static BattleOptimizationCpuGcCaptureEditor()
        {
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayMode;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            ProfilerDriver.NewProfilerFrameRecorded += OnFrameRecorded;
        }

        internal static void RequireAvailable()
        {
            if (state != null || Profiler.enabled || ProfilerDriver.enabled ||
                ProfilerDriver.deepProfiling || ProfilerDriver.profileGPU)
                throw new InvalidOperationException("An idle CPU profiler without deep/GPU profiling is required; existing settings will not be stolen.");
        }

        internal static void Arm(string outputRoot)
        {
            RequireAvailable();
            string root = Path.GetFullPath(outputRoot);
            if (File.Exists(Path.Combine(root, "cpu-gc.raw")) ||
                File.Exists(Path.Combine(root, "cpu-gc-state.json")) ||
                File.Exists(Path.Combine(root, "cpu-gc-samples.json")))
                throw new InvalidOperationException("Capture output must be fresh.");
            state = new CaptureState { outputRoot = root, before = ReadSettings() };
        }

        internal static void BeginCameraWindow(string outputRoot)
        {
            Arm(outputRoot);
            state.cameraEnvelopeMode = true;
            state.cameraBoundaries = new CameraBoundary[CaptureFrameCount];
            state.deadline = EditorApplication.timeSinceStartup + 45d;
            state.status = "RECORDING";
            state.recording = true;
            try
            {
                ProfilerDriver.profileEditor = false;
                ProfilerDriver.SetAreaEnabled(ProfilerArea.CPU, true);
                Profiler.enableAllocationCallstacks = true;
                Profiler.logFile = Path.Combine(state.outputRoot, "cpu-gc.raw");
                Profiler.enableBinaryLog = true;
                Profiler.enabled = true;
            }
            catch (Exception exception)
            {
                state.error = exception.ToString();
                FinishAndRestore("camera-capture-start-error");
                throw;
            }
        }

        internal static void BeginCameraEnvelope(int unityFrame, int logicTick)
        {
            if (state == null || !state.cameraEnvelopeMode || !state.recording || state.finishPending)
                return;
            if (state.cameraEnvelopeOpen || state.completedCameras >= CaptureFrameCount)
            {
                state.cameraBoundariesValid = false;
                state.finishPending = true;
                return;
            }
            state.cameraBoundaries[state.completedCameras] = new CameraBoundary
            {
                ordinal = state.completedCameras + 1, beginUnityFrame = unityFrame,
                beginLogicTick = logicTick,
            };
            state.cameraEnvelopeOpen = true;
            // Point samples remain nested inside their own URP callback wrapper, never across wrappers.
            using (CameraBeginMarker.Auto())
            {
            }
        }

        internal static void EndCameraEnvelope(int unityFrame, int logicTick, long events, bool valid)
        {
            if (state == null || !state.cameraEnvelopeMode || !state.cameraEnvelopeOpen)
                return;
            using (CameraEndMarker.Auto())
            {
            }
            state.cameraEnvelopeOpen = false;
            ref CameraBoundary boundary = ref state.cameraBoundaries[state.completedCameras];
            boundary.endUnityFrame = unityFrame;
            boundary.endLogicTick = logicTick;
            boundary.allocationEvents = events;
            boundary.calibratedScopeValid = valid;
            state.cameraBoundariesValid &= valid && boundary.beginUnityFrame == unityFrame;
            state.completedCameras++;
            // Alignment contract: NTSD-OPT-H11-CAMERA-GC-CALLSTACK-042; stop on the next editor update, after the full camera is recorded.
            state.finishPending = HasCompletedCameraWindow(state.completedCameras);
        }

        private static bool HasCompletedCameraWindow(int completed)
        {
            return completed == CaptureFrameCount;
        }

        private static bool IsWithinCameraTime(double sampleStart, double beginEnd, double endStart)
        {
            return !double.IsNaN(sampleStart) && !double.IsInfinity(sampleStart) &&
                !double.IsNaN(beginEnd) && !double.IsInfinity(beginEnd) &&
                !double.IsNaN(endStart) && !double.IsInfinity(endStart) &&
                endStart >= beginEnd && sampleStart >= beginEnd && sampleStart <= endStart;
        }

        [MenuItem("NTSD/Validation/Optimization/Batch42 Recover Retained Camera GC Raw")]
        private static void RecoverRetainedCameraRaw()
        {
            RequireAvailable();
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
                throw new InvalidOperationException("Retained camera parsing requires idle Edit Mode; no recapture.");
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..",
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH42-CAMERA-GC-CALLSTACK-20261007/camera-02"));
            string statePath = Path.Combine(root, "cpu-gc-state-recovered.json");
            if (File.Exists(statePath) || File.Exists(Path.Combine(root, "camera-gc-callstacks.json")))
                throw new InvalidOperationException("Retained camera output must be fresh; prior evidence is preserved.");
            CaptureState recovered = JsonUtility.FromJson<CaptureState>(File.ReadAllText(Path.Combine(root, "cpu-gc-state.json")));
            if (recovered == null || !recovered.cameraEnvelopeMode || !recovered.settingsRestored ||
                (recovered.status != "PARTIAL" && recovered.status != "RAW_RETAINED_CAMERA_PARSE_PENDING") ||
                !recovered.cameraBoundariesValid ||
                !HasCompletedCameraWindow(recovered.completedCameras) ||
                recovered.cameraBoundaries == null || recovered.cameraBoundaries.Length != CaptureFrameCount)
                throw new InvalidOperationException("Requires the retained partial capture with eight valid camera boundaries.");
            recovered.before = ReadSettings();
            recovered.error = null;
            recovered.stopReason = "retained-camera-raw-parse-only-no-recapture";
            recovered.outputRoot = root;
            try
            {
                ExportCameraSamples(recovered);
                recovered.status = recovered.exportedCameraEnvelopes == CaptureFrameCount &&
                    recovered.allocationCountsMatch && recovered.allocationCallstacksPresent
                    ? "RECOVERED_CAMERA_CALLSITES" : "PARTIAL";
            }
            catch (Exception exception)
            {
                recovered.status = "PARTIAL";
                recovered.error = exception.ToString();
            }
            finally
            {
                RestoreSettings(recovered.before);
                recovered.after = ReadSettings();
                recovered.settingsRestored = JsonUtility.ToJson(recovered.before) == JsonUtility.ToJson(recovered.after);
                if (!recovered.settingsRestored)
                    recovered.status = "PARTIAL";
                SaveNew(statePath, JsonUtility.ToJson(recovered, true));
            }
        }

        [MenuItem("NTSD/Validation/Optimization/Batch39 Recover Retained CPU GC Raw")]
        private static void RecoverRetainedRaw()
        {
            RequireAvailable();
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || ProfilerDriver.firstFrameIndex >= 0)
                throw new InvalidOperationException("Recovery requires idle Edit Mode and an empty history; existing history will not be cleared.");
            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..",
                "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH39-CPU-GC-CAPTURE-20261007/windows-01"));
            string samplesPath = Path.Combine(root, "cpu-gc-samples-recovered.json");
            string statePath = Path.Combine(root, "cpu-gc-state-recovered.json");
            if (File.Exists(samplesPath) || File.Exists(statePath))
                throw new InvalidOperationException("Recovery output must be fresh; no recapture is authorized.");
            var recovered = new CaptureState
            {
                outputRoot = root,
                before = ReadSettings(),
                stopReason = "retained-binary-parse-only-no-recapture",
                status = "PARTIAL",
            };
            try
            {
                if (!ProfilerDriver.LoadProfile(Path.Combine(root, "cpu-gc.raw"), true))
                    throw new InvalidOperationException("The retained binary could not be loaded.");
                // The first binary frame can begin part-way through the enabling PlayerLoop.
                int frame = ProfilerDriver.GetNextFrameIndex(ProfilerDriver.firstFrameIndex);
                while (frame >= 0 && recovered.capturedFrames < CaptureFrameCount)
                {
                    recovered.frameIndices[recovered.capturedFrames++] = frame;
                    frame = ProfilerDriver.GetNextFrameIndex(frame);
                }
                ExportSamples(recovered, "cpu-gc-samples-recovered.json");
                recovered.status = recovered.capturedFrames == CaptureFrameCount &&
                    recovered.validMainThreadFrames == CaptureFrameCount &&
                    !recovered.threadScanLimitReached ? "RECOVERED_RETAINED_BINARY" : "PARTIAL";
            }
            catch (Exception exception)
            {
                recovered.error = exception.ToString();
            }
            finally
            {
                RestoreSettings(recovered.before);
                recovered.after = ReadSettings();
                recovered.settingsRestored = JsonUtility.ToJson(recovered.before) == JsonUtility.ToJson(recovered.after);
                SaveNew(statePath, JsonUtility.ToJson(recovered, true));
            }
        }

        private static bool IsCaptureReady(ProductionEntityStressReport report)
        {
            return report != null && report.status == "Running" &&
                report.warmupTicksCompleted == 120 && report.sampledLogicTicks > 0 &&
                report.baseAiActiveCount == 1000 && report.baseRosterActiveCount == 1000;
        }

        internal static void Observe(ProductionEntityStressReport report)
        {
            if (state == null || state.status != "ARMED" || !IsCaptureReady(report))
                return;
            try
            {
                state.warmupTicks = report.warmupTicksCompleted;
                state.sampleTickAtStart = report.sampledLogicTicks;
                state.activeAiAtStart = report.baseAiActiveCount;
                state.baseRosterAtStart = report.baseRosterActiveCount;
                state.startLastFrame = ProfilerDriver.lastFrameIndex;
                state.deadline = EditorApplication.timeSinceStartup + 45d;
                state.status = "RECORDING";
                state.recording = true;
                ProfilerDriver.profileEditor = false;
                ProfilerDriver.SetAreaEnabled(ProfilerArea.CPU, true);
                Profiler.enableAllocationCallstacks = true;
                Profiler.logFile = Path.Combine(state.outputRoot, "cpu-gc.raw");
                Profiler.enableBinaryLog = true;
                Profiler.enabled = true;
            }
            catch (Exception exception)
            {
                state.error = exception.ToString();
                FinishAndRestore("capture-start-error");
                throw;
            }
        }

        private static void OnFrameRecorded(int connectionId, int frameIndex)
        {
            if (state == null || state.cameraEnvelopeMode || !state.recording || state.finishPending ||
                frameIndex <= state.startLastFrame)
                return;
            if (state.connectionId < 0)
                state.connectionId = connectionId;
            if (state.connectionId != connectionId ||
                (state.capturedFrames > 0 && frameIndex <= state.frameIndices[state.capturedFrames - 1]))
                return;
            state.frameIndices[state.capturedFrames++] = frameIndex;
            if (state.capturedFrames == CaptureFrameCount)
            {
                // Stop recording immediately; sample-name/stack materialization happens only after restoration.
                Profiler.enabled = false;
                state.finishPending = true;
            }
        }

        private static void Update()
        {
            if (state == null)
                return;
            if (state.finishPending)
                FinishAndRestore("eight-completed-frames");
            else if (state.recording && EditorApplication.timeSinceStartup >= state.deadline)
                FinishAndRestore("45-second-deadline");
        }

        private static void OnPlayMode(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode)
                FinishAndRestore("play-exit");
        }

        private static void BeforeReload()
        {
            FinishAndRestore("assembly-reload");
        }

        internal static void FinishAndRestore(string reason)
        {
            CaptureState finished = state;
            if (finished == null)
                return;
            state = null;
            if (finished.cameraEnvelopeOpen)
            {
                finished.cameraEnvelopeOpen = false;
                finished.cameraBoundariesValid = false;
            }
            finished.stopReason = reason;
            finished.recording = false;
            try
            {
                RestoreSettings(finished.before);
                finished.after = ReadSettings();
                finished.settingsRestored = JsonUtility.ToJson(finished.before) == JsonUtility.ToJson(finished.after);
                if (finished.cameraEnvelopeMode)
                {
                    finished.status = finished.settingsRestored &&
                        HasCompletedCameraWindow(finished.completedCameras) &&
                        finished.cameraBoundariesValid && string.IsNullOrEmpty(finished.error)
                        ? "RAW_RETAINED_CAMERA_PARSE_PENDING" : "PARTIAL";
                    return;
                }
                finished.status = finished.settingsRestored && finished.capturedFrames == CaptureFrameCount &&
                    string.IsNullOrEmpty(finished.error) ? "CAPTURED" : "PARTIAL";
                ExportSamples(finished);
                if (finished.validMainThreadFrames != CaptureFrameCount || finished.threadScanLimitReached)
                    finished.status = "PARTIAL";
            }
            catch (Exception exception)
            {
                finished.status = "PARTIAL";
                finished.error += exception.ToString();
            }
            finally
            {
                // A diagnostic export failure must never skip restoration or the battle owner's shutdown.
                RestoreSettings(finished.before);
                finished.after = ReadSettings();
                finished.settingsRestored = JsonUtility.ToJson(finished.before) == JsonUtility.ToJson(finished.after);
                SaveNew(Path.Combine(finished.outputRoot, "cpu-gc-state.json"), JsonUtility.ToJson(finished, true));
            }
        }

        private static ProfilerSettings ReadSettings()
        {
            return new ProfilerSettings
            {
                enabled = Profiler.enabled,
                driverEnabled = ProfilerDriver.enabled,
                binary = Profiler.enableBinaryLog,
                logFile = Profiler.logFile ?? string.Empty,
                allocationCallstacks = Profiler.enableAllocationCallstacks,
                cpuArea = ProfilerDriver.IsAreaEnabled(ProfilerArea.CPU),
                profileEditor = ProfilerDriver.profileEditor,
                deepProfiling = ProfilerDriver.deepProfiling,
                memoryRecordMode = (int)ProfilerDriver.memoryRecordMode,
            };
        }

        private static void RestoreSettings(ProfilerSettings settings)
        {
            Profiler.enabled = false;
            Profiler.enableBinaryLog = false;
            Profiler.enableAllocationCallstacks = settings.allocationCallstacks;
            ProfilerDriver.memoryRecordMode = (ProfilerMemoryRecordMode)settings.memoryRecordMode;
            ProfilerDriver.SetAreaEnabled(ProfilerArea.CPU, settings.cpuArea);
            ProfilerDriver.profileEditor = settings.profileEditor;
            Profiler.logFile = settings.logFile;
            Profiler.enableBinaryLog = settings.binary;
            ProfilerDriver.enabled = settings.driverEnabled;
            Profiler.enabled = settings.enabled;
        }

        private static void ExportSamples(CaptureState finished, string outputFile = "cpu-gc-samples.json")
        {
            var threads = new List<ThreadSamples>();
            var stack = new List<ulong>(64);
            for (int f = 0; f < finished.capturedFrames; f++)
            {
                bool mainValid = false;
                for (int t = 0; t < ThreadScanLimit; t++)
                {
                    using (RawFrameDataView view = ProfilerDriver.GetRawFrameDataView(finished.frameIndices[f], t))
                    {
                        if (!view.valid)
                            break;
                        var thread = new ThreadSamples
                        {
                            frameIndex = view.frameIndex,
                            threadIndex = t,
                            threadGroup = view.threadGroupName,
                            threadName = view.threadName,
                            frameTimeMs = view.frameTimeMs,
                            samples = new Sample[view.sampleCount],
                        };
                        for (int s = 0; s < view.sampleCount; s++)
                        {
                            var sample = new Sample
                            {
                                index = s,
                                name = view.GetSampleName(s),
                                inclusiveMs = view.GetSampleTimeMs(s),
                                startMs = view.GetSampleStartTimeMs(s),
                                children = view.GetSampleChildrenCount(s),
                                recursiveChildren = view.GetSampleChildrenCountRecursive(s),
                                metadataCount = view.GetSampleMetadataCount(s),
                            };
                            if (t == 0 && sample.name == "PlayerLoop")
                                mainValid = true;
                            if (sample.name == "GC.Alloc")
                            {
                                if (sample.metadataCount > 0)
                                    sample.gcAllocBytes = view.GetSampleMetadataAsLong(s, 0);
                                stack.Clear();
                                view.GetSampleCallstack(s, stack);
                                sample.gcCallstack = new Callsite[stack.Count];
                                for (int a = 0; a < stack.Count; a++)
                                {
                                    FrameDataView.MethodInfo method = view.ResolveMethodInfo(stack[a]);
                                    sample.gcCallstack[a] = new Callsite
                                    {
                                        address = stack[a].ToString("X16"),
                                        method = method.methodName,
                                        file = method.sourceFileName,
                                        line = method.sourceFileLine,
                                    };
                                }
                            }
                            thread.samples[s] = sample;
                        }
                        threads.Add(thread);
                        if (t == ThreadScanLimit - 1)
                            finished.threadScanLimitReached = true;
                    }
                }
                if (mainValid)
                    finished.validMainThreadFrames++;
            }
            finished.exportedThreadFrames = threads.Count;
            SaveNew(Path.Combine(finished.outputRoot, outputFile),
                JsonUtility.ToJson(new SamplesReport { threads = threads.ToArray() }, true));
        }

        private static void ExportCameraSamples(CaptureState finished)
        {
            int previousLast = ProfilerDriver.lastFrameIndex;
            if (!ProfilerDriver.LoadProfile(Path.Combine(finished.outputRoot, "cpu-gc.raw"), true))
                throw new InvalidOperationException("Camera raw capture could not be appended; existing history will not be cleared.");
            int first = previousLast < 0 ? ProfilerDriver.firstFrameIndex : ProfilerDriver.GetNextFrameIndex(previousLast);
            int last = ProfilerDriver.lastFrameIndex;
            finished.importedFirstFrame = first;
            finished.importedLastFrame = last;
            if (first < 0 || (previousLast >= 0 && first <= previousLast))
                throw new InvalidOperationException("Cannot identify appended raw frames; no mixed-history attribution.");
            var envelopes = new List<CameraEnvelopeSamples>(CaptureFrameCount);
            var stack = new List<ulong>(64);
            bool countsMatch = true;
            bool callstacksPresent = true;
            int scanned = 0;
            for (int frame = first; frame >= 0 && frame <= last; frame = ProfilerDriver.GetNextFrameIndex(frame))
            {
                if (++scanned > 128)
                    throw new InvalidOperationException("Unexpected camera capture frame range; raw retained without truncation.");
                using (RawFrameDataView view = ProfilerDriver.GetRawFrameDataView(frame, 0))
                {
                    if (!view.valid)
                        continue;
                    if (view.threadName != "Main Thread")
                        throw new InvalidOperationException("Camera raw thread zero is not the main thread.");
                    int beginMarker = -1;
                    int endMarker = -1;
                    for (int sampleIndex = 0; sampleIndex < view.sampleCount; sampleIndex++)
                    {
                        string name = view.GetSampleName(sampleIndex);
                        if (name == CameraBeginMarkerName)
                        {
                            if (beginMarker >= 0)
                                throw new InvalidOperationException("Multiple camera begin points in one profiler frame.");
                            beginMarker = sampleIndex;
                        }
                        if (name == CameraEndMarkerName)
                        {
                            if (endMarker >= 0)
                                throw new InvalidOperationException("Multiple camera end points in one profiler frame.");
                            endMarker = sampleIndex;
                        }
                    }
                    if (beginMarker < 0 && endMarker < 0)
                        continue;
                    if (beginMarker < 0 || endMarker < 0 || envelopes.Count >= finished.completedCameras)
                        throw new InvalidOperationException("Raw camera point pairs differ from recorded boundaries.");
                    double beginEnd = view.GetSampleStartTimeMs(beginMarker) + view.GetSampleTimeMs(beginMarker);
                    double endStart = view.GetSampleStartTimeMs(endMarker);
                    if (!IsWithinCameraTime(beginEnd, beginEnd, endStart))
                        throw new InvalidOperationException("Camera raw point times are non-finite or reversed.");
                    var allocations = new List<Sample>();
                    for (int sampleIndex = 0; sampleIndex < view.sampleCount; sampleIndex++)
                    {
                        if (view.GetSampleName(sampleIndex) != "GC.Alloc" ||
                            !IsWithinCameraTime(view.GetSampleStartTimeMs(sampleIndex), beginEnd, endStart))
                            continue;
                        int metadata = view.GetSampleMetadataCount(sampleIndex);
                        stack.Clear();
                        view.GetSampleCallstack(sampleIndex, stack);
                        callstacksPresent &= stack.Count > 0 && metadata > 0;
                        var callstack = new Callsite[stack.Count];
                        for (int address = 0; address < stack.Count; address++)
                        {
                            FrameDataView.MethodInfo method = view.ResolveMethodInfo(stack[address]);
                            callstack[address] = new Callsite
                            {
                                address = stack[address].ToString("X16"), method = method.methodName,
                                file = method.sourceFileName, line = method.sourceFileLine,
                            };
                        }
                        allocations.Add(new Sample
                        {
                            index = sampleIndex, name = "GC.Alloc", metadataCount = metadata,
                            inclusiveMs = view.GetSampleTimeMs(sampleIndex),
                            startMs = view.GetSampleStartTimeMs(sampleIndex),
                            gcAllocBytes = metadata > 0 ? view.GetSampleMetadataAsLong(sampleIndex, 0) : -1,
                            gcCallstack = callstack,
                        });
                    }
                    CameraBoundary boundary = finished.cameraBoundaries[envelopes.Count];
                    countsMatch &= boundary.calibratedScopeValid && allocations.Count == boundary.allocationEvents;
                    envelopes.Add(new CameraEnvelopeSamples
                    {
                        boundary = boundary, profilerFrameIndex = view.frameIndex,
                        beginMarkerSampleIndex = beginMarker, endMarkerSampleIndex = endMarker,
                        beginMarkerEndMs = beginEnd, endMarkerStartMs = endStart,
                        threadName = view.threadName, allocations = allocations.ToArray(),
                    });
                }
                if (frame == last)
                    break;
            }
            finished.exportedCameraEnvelopes = envelopes.Count;
            finished.allocationCountsMatch = countsMatch && envelopes.Count == finished.completedCameras;
            finished.allocationCallstacksPresent = callstacksPresent;
            SaveNew(Path.Combine(finished.outputRoot, "camera-gc-callstacks.json"),
                JsonUtility.ToJson(new CameraSamplesReport { envelopes = envelopes.ToArray() }, true));
        }

        private static void SaveNew(string path, string content)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(content);
        }
    }
}
#endif
