#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q10StereoDiagonalCalibrationEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/Q10/Calibrate 336B44 Stereo Diagonal Matrix";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q10-STEREO-DIAGONAL-CALIBRATION-001/";
        private const double MeasureBeginSeconds = 0.12;
        private const double MeasureEndSeconds = 0.38;
        private const double CaseSeconds = 0.48;
        private const double GapSeconds = 0.16;
        private const double DeadlineSeconds = 22.0;
        private static readonly string[] ProtectedPaths =
        {
            "Assets/NTSD/Scene/NTSD_Battle.unity",
            MenuScene,
            "Assets/NTSD/Config/GameConfig/GameConfig.asset",
            "Assets/NTSD/Resources/ProjectBattleModeConfig.asset"
        };
        private static readonly SignalCase[] Cases =
        {
            new SignalCase("left_center_reference", 0, 0f),
            new SignalCase("right_center_reference", 1, 0f),
            new SignalCase("left_pan_minus_1", 0, -1f),
            new SignalCase("right_pan_minus_1", 1, -1f),
            new SignalCase("left_pan_minus_0_75", 0, -0.75f),
            new SignalCase("right_pan_minus_0_75", 1, -0.75f),
            new SignalCase("left_pan_minus_2_3", 0, -2f / 3f),
            new SignalCase("right_pan_minus_2_3", 1, -2f / 3f),
            new SignalCase("left_native_75_25_candidate", 0, -2f / 3f, 0.75f),
            new SignalCase("right_native_75_25_candidate", 1, -2f / 3f, 0.75f),
            new SignalCase("left_pan_minus_0_5", 0, -0.5f),
            new SignalCase("right_pan_minus_0_5", 1, -0.5f),
            new SignalCase("left_pan_plus_0_5", 0, 0.5f),
            new SignalCase("right_pan_plus_0_5", 1, 0.5f),
            new SignalCase("left_pan_plus_1", 0, 1f),
            new SignalCase("right_pan_plus_1", 1, 1f)
        };

        private static Report report;
        private static string resultPath;
        private static GameObject temporaryObject;
        private static AudioSource voice;
        private static readonly AudioClip[] clips = new AudioClip[2];
        private static bool running;
        private static bool ownsCapture;
        private static bool casePlaying;
        private static bool timingOverrideActive;
        private static bool previousRunInBackground;
        private static int previousCaptureFramerate;
        private static int caseIndex;
        private static double startedAt;
        private static double caseStartedAt;
        private static double gapStartedAt;

        [Serializable]
        private sealed class FileHash
        {
            public string path;
            public string sha256;
        }

        [Serializable]
        private sealed class SampleResult
        {
            public string name;
            public int inputChannel;
            public float panStereo;
            public float sourceVolume;
            public long sampleFrames;
            public double leftSquares;
            public double rightSquares;
            public double leftRms;
            public double rightRms;
            public double leftGainRelative;
            public double rightGainRelative;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string startedUtc;
            public string initialScene;
            public bool sceneDirtyAtStart;
            public int sceneCountAtStart;
            public string speakerMode;
            public int sampleRate;
            public int initialGameFrame;
            public int finalGameFrame;
            public double initialDspTime;
            public double finalDspTime;
            public int editorUpdateCalls;
            public int nonzeroCaptureFrames;
            public bool captureStopped;
            public bool temporaryObjectsDestroyed;
            public bool timingRestored;
            public bool protectedHashesStable;
            public string matrixLimit = "AudioRenderer software mixer output with temporary left-only/right-only stereo PCM. No hardware or native EXE PCM capture; output Mixer group is null.";
            public List<FileHash> before = new List<FileHash>();
            public List<FileHash> after = new List<FileHash>();
            public SampleResult[] samples;
        }

        private readonly struct SignalCase
        {
            public SignalCase(string name, int inputChannel, float pan, float volume = 1f)
            {
                Name = name;
                InputChannel = inputChannel;
                Pan = pan;
                Volume = volume;
            }

            public string Name { get; }
            public int InputChannel { get; }
            public float Pan { get; }
            public float Volume { get; }
        }

        private static string PathInProject(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static List<FileHash> HashProtectedFiles()
        {
            var result = new List<FileHash>(ProtectedPaths.Length);
            foreach (string relative in ProtectedPaths)
            {
                using (SHA256 hash = SHA256.Create())
                using (FileStream stream = File.OpenRead(PathInProject(relative)))
                {
                    result.Add(new FileHash
                    {
                        path = relative,
                        sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "")
                    });
                }
            }
            return result;
        }

        private static bool HashesMatch(List<FileHash> before, List<FileHash> after) =>
            before != null && after != null &&
            before.Count == ProtectedPaths.Length && after.Count == ProtectedPaths.Length &&
            before.Zip(after, (left, right) =>
                left.path == right.path && left.sha256 == right.sha256).All(equal => equal);

        [MenuItem(MenuPath)]
        private static void Run()
        {
            if (running) return;
            resultPath = PathInProject(ResultRoot + "calibration-" +
                DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff") + ".json");
            Scene scene = SceneManager.GetActiveScene();
            report = new Report
            {
                status = "RUNNING",
                startedUtc = DateTime.UtcNow.ToString("O"),
                initialScene = scene.path,
                sceneDirtyAtStart = scene.isDirty,
                sceneCountAtStart = SceneManager.sceneCount,
                speakerMode = AudioSettings.speakerMode.ToString(),
                sampleRate = AudioSettings.outputSampleRate,
                samples = Cases.Select(value => new SampleResult
                {
                    name = value.Name,
                    inputChannel = value.InputChannel,
                    panStereo = value.Pan,
                    sourceVolume = value.Volume
                }).ToArray(),
                before = HashProtectedFiles()
            };
            if (!EditorApplication.isPlaying || scene.path != MenuScene || scene.isDirty ||
                SceneManager.sceneCount != 1 || AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
            {
                SaveEarlyFailure("Expected sole clean Menu Scene in Play and stereo output.");
                return;
            }
            if (!string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase))
            {
                SaveEarlyFailure("Only the original project Editor is allowed.");
                return;
            }
            if (UnityEngine.Object.FindObjectsOfType<AudioSource>(true)
                .Any(value => value != null && value.isPlaying))
            {
                SaveEarlyFailure("Another AudioSource is playing.");
                return;
            }

            running = true;
            startedAt = EditorApplication.timeSinceStartup;
            caseIndex = 0;
            EditorApplication.update += OnUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            EditorApplication.quitting += OnEditorQuitting;
            try
            {
                previousCaptureFramerate = Time.captureFramerate;
                previousRunInBackground = Application.runInBackground;
                timingOverrideActive = true;
                Time.captureFramerate = 30;
                Application.runInBackground = true;
                report.initialGameFrame = Time.frameCount;
                report.initialDspTime = AudioSettings.dspTime;

                int rate = AudioSettings.outputSampleRate;
                if (rate <= 0) throw new InvalidOperationException("Invalid sample rate.");
                for (int channel = 0; channel < clips.Length; channel++)
                {
                    float[] pcm = new float[rate * 2];
                    for (int sample = 0; sample < rate; sample++)
                        pcm[sample * 2 + channel] = 0.125f *
                            Mathf.Sin(2f * Mathf.PI * 440f * sample / rate);
                    clips[channel] = AudioClip.Create(
                        "Q10_Stereo_Only_Channel_" + channel, rate, 2, rate, false);
                    if (clips[channel] == null || !clips[channel].SetData(pcm, 0))
                        throw new InvalidOperationException("Could not create stereo PCM.");
                }
                temporaryObject = new GameObject("Q10 Stereo Matrix Calibration");
                voice = temporaryObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.spatialBlend = 0f;
                voice.loop = false;
                voice.outputAudioMixerGroup = null;
                if (!AudioRenderer.Start())
                    throw new InvalidOperationException("AudioRenderer is already recording.");
                ownsCapture = true;
                StartCase(EditorApplication.timeSinceStartup);
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private static void StartCase(double now)
        {
            SignalCase value = Cases[caseIndex];
            voice.Stop();
            voice.clip = clips[value.InputChannel];
            voice.panStereo = value.Pan;
            voice.volume = value.Volume;
            voice.timeSamples = 0;
            voice.Play();
            caseStartedAt = now;
            casePlaying = true;
        }

        private static void OnUpdate()
        {
            if (!running) return;
            try
            {
                report.editorUpdateCalls++;
                EditorApplication.QueuePlayerLoopUpdate();
                double now = EditorApplication.timeSinceStartup;
                if (now - startedAt > DeadlineSeconds)
                    throw new TimeoutException("Stereo capture exceeded 22 seconds.");
                CaptureFrame(now);
                if (casePlaying && now - caseStartedAt >= CaseSeconds)
                {
                    voice.Stop();
                    casePlaying = false;
                    gapStartedAt = now;
                }
                else if (!casePlaying && now - gapStartedAt >= GapSeconds)
                {
                    caseIndex++;
                    if (caseIndex == Cases.Length) { Finish(true, null); return; }
                    StartCase(now);
                }
            }
            catch (Exception error) { Finish(false, error.ToString()); }
        }

        private static void CaptureFrame(double now)
        {
            int frameCount = AudioRenderer.GetSampleCountForCaptureFrame();
            if (frameCount <= 0) return;
            if (frameCount > 65536)
                throw new InvalidOperationException("Audio capture frame is too large.");
            report.nonzeroCaptureFrames++;
            using (var buffer = new NativeArray<float>(frameCount * 2, Allocator.Temp))
            {
                if (!AudioRenderer.Render(buffer))
                    throw new InvalidOperationException("AudioRenderer.Render failed.");
                double elapsed = now - caseStartedAt;
                if (!casePlaying || elapsed < MeasureBeginSeconds ||
                    elapsed > MeasureEndSeconds) return;
                SampleResult sample = report.samples[caseIndex];
                for (int frame = 0; frame < frameCount; frame++)
                {
                    float left = buffer[frame * 2];
                    float right = buffer[frame * 2 + 1];
                    sample.leftSquares += left * left;
                    sample.rightSquares += right * right;
                }
                sample.sampleFrames += frameCount;
            }
        }

        private static void Finish(bool captureCompleted, string error)
        {
            if (!running) return;
            running = false;
            EditorApplication.update -= OnUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            EditorApplication.quitting -= OnEditorQuitting;
            if (voice != null) voice.Stop();
            if (ownsCapture) { AudioRenderer.Stop(); ownsCapture = false; }
            report.captureStopped = true;
            if (timingOverrideActive)
            {
                Time.captureFramerate = previousCaptureFramerate;
                Application.runInBackground = previousRunInBackground;
                timingOverrideActive = false;
            }
            report.timingRestored = true;
            if (temporaryObject != null) UnityEngine.Object.Destroy(temporaryObject);
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i] != null) UnityEngine.Object.Destroy(clips[i]);
                clips[i] = null;
            }
            voice = null;
            temporaryObject = null;
            report.temporaryObjectsDestroyed = true;

            double leftReference = CalculateRms(report.samples[0].leftSquares,
                report.samples[0].sampleFrames);
            double rightReference = CalculateRms(report.samples[1].rightSquares,
                report.samples[1].sampleFrames);
            bool enough = leftReference > 0.0001 && rightReference > 0.0001;
            foreach (SampleResult sample in report.samples)
            {
                if (sample.sampleFrames < 1000) enough = false;
                sample.leftRms = CalculateRms(sample.leftSquares, sample.sampleFrames);
                sample.rightRms = CalculateRms(sample.rightSquares, sample.sampleFrames);
                double reference = sample.inputChannel == 0
                    ? leftReference : rightReference;
                if (reference <= 0) continue;
                sample.leftGainRelative = sample.leftRms / reference;
                sample.rightGainRelative = sample.rightRms / reference;
            }
            report.finalGameFrame = Time.frameCount;
            report.finalDspTime = AudioSettings.dspTime;
            report.after = HashProtectedFiles();
            report.protectedHashesStable = HashesMatch(report.before, report.after);
            report.error = error ?? (enough ? "" : "Insufficient reference or sample frames.");
            report.status = captureCompleted && enough && report.protectedHashesStable &&
                report.captureStopped && report.temporaryObjectsDestroyed &&
                report.timingRestored ? "CAPTURE_COMPLETE" : "CAPTURE_FAILED";
            Save();
        }

        private static double CalculateRms(double squares, long frames) =>
            frames > 0 ? Math.Sqrt(squares / frames) : 0.0;

        private static void SaveEarlyFailure(string reason)
        {
            report.status = "PRECONDITION_FAILED";
            report.error = reason;
            report.after = HashProtectedFiles();
            report.protectedHashesStable = HashesMatch(report.before, report.after);
            Save();
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
                using (var stream = new FileStream(resultPath, FileMode.CreateNew,
                           FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(report, true));
                Debug.Log("Q10 stereo diagonal calibration: " + report.status +
                    " (" + resultPath + ")");
            }
            catch (Exception error)
            {
                Debug.LogError("Could not save Q10 stereo calibration: " + error);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
                Finish(false, "Play Mode exited before capture completed.");
        }

        private static void OnBeforeAssemblyReload()
        {
            Finish(false, "Assembly reload interrupted capture.");
        }

        private static void OnEditorQuitting()
        {
            Finish(false, "Editor quit interrupted capture.");
        }
    }
}
#endif
