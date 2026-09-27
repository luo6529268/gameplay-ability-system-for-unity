#if UNITY_EDITOR
using System;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q10AudioRendererMatrixCalibrationEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/Q10/Calibrate Mono Stereo Matrix Output";
        private const double MeasureBeginSeconds = 0.12;
        private const double MeasureEndSeconds = 0.42;
        private const double CaseSeconds = 0.50;
        private const double GapSeconds = 0.18;
        private const double DeadlineSeconds = 12.0;

        private static readonly CalibrationCase[] Cases =
        {
            new CalibrationCase("full_left_reference", -1f, 1f, 1f, 0f),
            new CalibrationCase("unity_center_reference", 0f, 1f,
                0.70710678f, 0.70710678f),
            CreateNativeCandidate("native_53_47", 53, 47),
            CreateNativeCandidate("native_41_59", 41, 59),
            CreateNativeCandidate("native_50_50", 50, 50),
        };

        private static GameObject sourceObject;
        private static AudioSource source;
        private static AudioClip clip;
        private static CalibrationReport report;
        private static string resultPath;
        private static bool running;
        private static bool ownsCapture;
        private static bool timingOverrideActive;
        private static bool previousRunInBackground;
        private static int previousCaptureFramerate;
        private static bool casePlaying;
        private static int caseIndex;
        private static double startedAt;
        private static double caseStartedAt;
        private static double gapStartedAt;

        [MenuItem(MenuPath)]
        public static void Run()
        {
            if (running)
                return;

            resultPath = Path.GetFullPath(Path.Combine(
                Application.dataPath, "..", "artifacts", "diagnostics",
                "NTSD28-Q10-AUDIORENDERER-MATRIX-CALIBRATION-001",
                "calibration-v3.json"));
            if (File.Exists(resultPath))
            {
                Debug.LogError("Q10 audio calibration result already exists: " + resultPath);
                return;
            }

            report = new CalibrationReport
            {
                status = "RUNNING",
                initialScene = SceneManager.GetActiveScene().path,
                speakerMode = AudioSettings.speakerMode.ToString(),
                sampleRate = AudioSettings.outputSampleRate,
                cases = new CaseResult[Cases.Length],
            };
            for (int i = 0; i < Cases.Length; i++)
            {
                report.cases[i] = new CaseResult
                {
                    name = Cases[i].Name,
                    panStereo = Cases[i].Pan,
                    sourceVolume = Cases[i].Volume,
                    targetLeft = Cases[i].TargetLeft,
                    targetRight = Cases[i].TargetRight,
                };
            }

            if (!EditorApplication.isPlaying ||
                SceneManager.GetActiveScene().name != "NTSD_Menu" ||
                AudioSettings.speakerMode != AudioSpeakerMode.Stereo)
            {
                SaveEarlyFailure("Saved Menu must be active in Play with stereo output.");
                return;
            }

            AudioSource[] existing = UnityEngine.Object.FindObjectsOfType<AudioSource>(true);
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] != null && existing[i].isPlaying)
                {
                    SaveEarlyFailure("Another AudioSource is playing: " + existing[i].name);
                    return;
                }
            }

            running = true;
            startedAt = EditorApplication.timeSinceStartup;
            caseIndex = 0;
            EditorApplication.update += OnUpdate;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.quitting += OnEditorQuitting;
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;

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
                if (rate <= 0)
                    throw new InvalidOperationException("Invalid audio output sample rate.");

                float[] pcm = new float[rate];
                for (int sample = 0; sample < pcm.Length; sample++)
                {
                    pcm[sample] = 0.125f *
                        Mathf.Sin(2f * Mathf.PI * 440f * sample / rate);
                }

                clip = AudioClip.Create("Q10_Mono_Matrix_Calibration",
                    pcm.Length, 1, rate, false);
                if (clip == null || !clip.SetData(pcm, 0))
                    throw new InvalidOperationException("Could not create calibration PCM.");

                sourceObject = new GameObject("Q10 Audio Matrix Calibration");
                source = sourceObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.clip = clip;
                source.loop = false;
                source.outputAudioMixerGroup = null;

                if (!AudioRenderer.Start())
                    throw new InvalidOperationException("AudioRenderer is already recording.");
                ownsCapture = true;
                StartCase(EditorApplication.timeSinceStartup);
            }
            catch (Exception exception)
            {
                Finish(false, exception.ToString());
            }
        }

        private static CalibrationCase CreateNativeCandidate(
            string name, int leftPercent, int rightPercent)
        {
            double left = leftPercent / 100.0;
            double right = rightPercent / 100.0;
            double power = left * left + right * right;
            float pan = (float)((right * right - left * left) / power);
            float volume = (float)Math.Sqrt(power);
            return new CalibrationCase(name, pan, volume,
                (float)left, (float)right);
        }

        private static void StartCase(double now)
        {
            CalibrationCase current = Cases[caseIndex];
            source.Stop();
            source.panStereo = current.Pan;
            source.volume = current.Volume;
            source.timeSamples = 0;
            source.Play();
            caseStartedAt = now;
            casePlaying = true;
        }

        private static void OnUpdate()
        {
            if (!running)
                return;

            try
            {
                report.editorUpdateCalls++;
                EditorApplication.QueuePlayerLoopUpdate();
                double now = EditorApplication.timeSinceStartup;
                if (now - startedAt > DeadlineSeconds)
                    throw new TimeoutException("Audio capture exceeded 12 seconds.");

                CaptureFrame(now);
                if (casePlaying && now - caseStartedAt >= CaseSeconds)
                {
                    source.Stop();
                    casePlaying = false;
                    gapStartedAt = now;
                }
                else if (!casePlaying && now - gapStartedAt >= GapSeconds)
                {
                    caseIndex++;
                    if (caseIndex == Cases.Length)
                    {
                        Finish(true, null);
                        return;
                    }
                    StartCase(now);
                }
            }
            catch (Exception exception)
            {
                Finish(false, exception.ToString());
            }
        }

        private static void CaptureFrame(double now)
        {
            int frameCount = AudioRenderer.GetSampleCountForCaptureFrame();
            if (frameCount <= 0)
                return;
            report.nonzeroCaptureFrames++;
            if (frameCount > 65536)
                throw new InvalidOperationException("Audio capture frame is too large.");

            using (var buffer = new NativeArray<float>(
                       frameCount * 2, Allocator.Temp))
            {
                if (!AudioRenderer.Render(buffer))
                    throw new InvalidOperationException("AudioRenderer.Render failed.");

                double elapsed = now - caseStartedAt;
                if (!casePlaying ||
                    elapsed < MeasureBeginSeconds ||
                    elapsed > MeasureEndSeconds)
                    return;

                CaseResult current = report.cases[caseIndex];
                for (int frame = 0; frame < frameCount; frame++)
                {
                    float left = buffer[frame * 2];
                    float right = buffer[frame * 2 + 1];
                    current.sumLeftSquares += left * left;
                    current.sumRightSquares += right * right;
                }
                current.sampleFrames += frameCount;
            }
        }

        private static void Finish(bool captureCompleted, string error)
        {
            if (!running)
                return;

            running = false;
            EditorApplication.update -= OnUpdate;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.quitting -= OnEditorQuitting;
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;

            if (source != null)
                source.Stop();
            if (ownsCapture)
            {
                AudioRenderer.Stop();
                ownsCapture = false;
            }
            if (timingOverrideActive)
            {
                Time.captureFramerate = previousCaptureFramerate;
                Application.runInBackground = previousRunInBackground;
                timingOverrideActive = false;
            }
            if (sourceObject != null)
                UnityEngine.Object.Destroy(sourceObject);
            if (clip != null)
                UnityEngine.Object.Destroy(clip);
            source = null;
            sourceObject = null;
            clip = null;

            CaseResult reference = report.cases[0];
            float referenceRms = reference.sampleFrames > 0
                ? (float)Math.Sqrt(reference.sumLeftSquares /
                    reference.sampleFrames)
                : 0f;
            report.referenceLeftRms = referenceRms;
            bool enoughSamples = referenceRms > 0.0001f;
            for (int i = 0; i < report.cases.Length; i++)
            {
                CaseResult result = report.cases[i];
                if (result.sampleFrames < 1000)
                    enoughSamples = false;
                if (result.sampleFrames > 0)
                {
                    result.leftRms = (float)Math.Sqrt(
                        result.sumLeftSquares / result.sampleFrames);
                    result.rightRms = (float)Math.Sqrt(
                        result.sumRightSquares / result.sampleFrames);
                    if (referenceRms > 0f)
                    {
                        result.leftGainRelative = result.leftRms / referenceRms;
                        result.rightGainRelative = result.rightRms / referenceRms;
                        result.leftError = result.leftGainRelative -
                            result.targetLeft;
                        result.rightError = result.rightGainRelative -
                            result.targetRight;
                    }
                }
            }

            report.error = error;
            report.finalGameFrame = Time.frameCount;
            report.finalDspTime = AudioSettings.dspTime;
            report.status = captureCompleted && enoughSamples
                ? "CAPTURE_COMPLETE"
                : "CAPTURE_FAILED";
            if (captureCompleted && !enoughSamples)
                report.error = "No usable channel reference or too few samples.";
            report.captureStopped = true;
            Save();
        }

        private static void SaveEarlyFailure(string reason)
        {
            report.status = "PRECONDITION_FAILED";
            report.error = reason;
            Save();
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
                if (File.Exists(resultPath))
                    throw new IOException("Calibration result already exists.");
                File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
                Debug.Log("Q10 audio matrix calibration: " +
                    report.status + " (" + resultPath + ")");
            }
            catch (Exception exception)
            {
                Debug.LogError("Could not save Q10 audio calibration: " +
                    exception);
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
                Finish(false, "Play mode exited before capture completed.");
        }

        private static void OnBeforeAssemblyReload()
        {
            Finish(false, "Assembly reload interrupted capture.");
        }

        private static void OnEditorQuitting()
        {
            Finish(false, "Editor quit interrupted capture.");
        }

        private readonly struct CalibrationCase
        {
            public CalibrationCase(string name, float pan, float volume,
                float targetLeft, float targetRight)
            {
                Name = name;
                Pan = pan;
                Volume = volume;
                TargetLeft = targetLeft;
                TargetRight = targetRight;
            }

            public string Name { get; }
            public float Pan { get; }
            public float Volume { get; }
            public float TargetLeft { get; }
            public float TargetRight { get; }
        }

        [Serializable]
        private sealed class CalibrationReport
        {
            public string status;
            public string error;
            public string initialScene;
            public string speakerMode;
            public int sampleRate;
            public int initialGameFrame;
            public int finalGameFrame;
            public double initialDspTime;
            public double finalDspTime;
            public int editorUpdateCalls;
            public int nonzeroCaptureFrames;
            public bool captureStopped;
            public float referenceLeftRms;
            public CaseResult[] cases;
        }

        [Serializable]
        private sealed class CaseResult
        {
            public string name;
            public float panStereo;
            public float sourceVolume;
            public float targetLeft;
            public float targetRight;
            public long sampleFrames;
            public double sumLeftSquares;
            public double sumRightSquares;
            public float leftRms;
            public float rightRms;
            public float leftGainRelative;
            public float rightGainRelative;
            public float leftError;
            public float rightError;
        }
    }
}
#endif
