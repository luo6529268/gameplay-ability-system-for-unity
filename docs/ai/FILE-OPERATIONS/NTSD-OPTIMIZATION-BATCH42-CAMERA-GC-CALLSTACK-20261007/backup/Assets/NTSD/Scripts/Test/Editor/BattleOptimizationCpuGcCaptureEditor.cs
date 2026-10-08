#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NTSD.Animation.Rendering.Editor;
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
            if (state == null || !state.recording || state.finishPending ||
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
            finished.stopReason = reason;
            finished.recording = false;
            try
            {
                RestoreSettings(finished.before);
                finished.after = ReadSettings();
                finished.settingsRestored = JsonUtility.ToJson(finished.before) == JsonUtility.ToJson(finished.after);
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

        private static void SaveNew(string path, string content)
        {
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(content);
        }
    }
}
#endif
