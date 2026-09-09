#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Exercises the production LocalFreeRun F1/F2/F5 path in the live battle scene.
    /// The probe deliberately queues physical Input System device states instead of
    /// calling the driver's diagnostic command seam.
    /// </summary>
    public static class BattleHostControlPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/验证/NTSD28/B1 Host控制真实Play探针";
        private const string ResultRelativePath =
            "Temp/NTSD28B1/host-control-play.result.json";
        private const int NormalMeasurementTicks = 10;
        private const int FastMeasurementTicks = 24;
        private const double StablePauseSeconds = 0.15;
        private const double StageTimeoutSeconds = 3.0;
        private const double OverallTimeoutSeconds = 15.0;
        private const double DebtToleranceSeconds = 0.000001;
        private const double MaximumNormalSecondsPerTick = 0.055;
        private const double MinimumNormalSecondsPerTick = 0.020;
        private const double MaximumFastSecondsPerTick = 0.006;
        private const double MaximumFastToNormalRatio = 0.25;

        private static readonly List<TraceRow> Trace = new List<TraceRow>(256);
        private static SimulationTickDriver driver;
        private static Keyboard keyboard;
        private static ProbeStage stage;
        private static bool running;
        private static long startedTimestamp;
        private static long stageStartedTimestamp;
        private static int lastObservedTick;
        private static int stageFrameCount;
        private static int pauseTick;
        private static int singleStepStartTick;
        private static int singleStepResultTick;
        private static int resumeTick;
        private static int runningF2Tick;
        private static int runningF2PausedTick;
        private static int measurementFirstTick;
        private static int measurementLastTick;
        private static long measurementFirstTimestamp;
        private static long measurementLastTimestamp;
        private static double normalElapsedSeconds;
        private static double normalSecondsPerTick;
        private static double fastElapsedSeconds;
        private static double fastSecondsPerTick;
        private static int normalTickDelta;
        private static int fastTickDelta;
        private static int maximumObservedTickJump;
        private static bool dedicatedWorkerActive;
        private static string dedicatedWorkerIneligibilityReason;
        private static string dedicatedWorkerFailure;
        private static string dedicatedWorkerLastSubmissionFailure;
        private static string pendingFailure;
        private static bool runningF2DropVerified;
        private static InputSettings.BackgroundBehavior previousBackgroundBehavior;
        private static InputSettings.EditorInputBehaviorInPlayMode
            previousEditorInputBehavior;
        private static bool previousRunInBackground;
        private static bool inputEnvironmentAdjusted;
        private static bool keyboardWasEnabled;
        private static bool keyboardStatePending;
        private static Key[] pendingKeyboardState = Array.Empty<Key>();
        private static int keyboardStateInjectionCount;
        private static string lastInjectedKeyboardState = string.Empty;
        private static PlayerLoopSystem originalPlayerLoop;
        private static bool playerLoopInjectionInstalled;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            if (!EditorApplication.isPlaying)
            {
                WriteImmediateFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            if (driver == null || driver.World == null)
            {
                WriteImmediateFailure("The production SimulationTickDriver/world is not ready.");
                return;
            }
            if (driver.Settings.driveMode != SimulationDriveMode.LocalFreeRun)
            {
                WriteImmediateFailure("The production driver is not in LocalFreeRun mode.");
                return;
            }
            if (driver.LifecycleState != BattleRuntimeLifecycleState.Running)
            {
                WriteImmediateFailure(
                    $"The production driver lifecycle is {driver.LifecycleState}, not Running.");
                return;
            }
            if (driver.IsPaused || driver.IsFastMode)
            {
                WriteImmediateFailure(
                    "The probe requires a running Normal baseline; restart Play Mode and retry.");
                return;
            }

            keyboard = InputSystem.AddDevice<Keyboard>();

            previousBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            previousEditorInputBehavior =
                InputSystem.settings.editorInputBehaviorInPlayMode;
            previousRunInBackground = Application.runInBackground;
            InputSystem.settings.backgroundBehavior =
                InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode
                    .AllDeviceInputAlwaysGoesToGameView;
            Application.runInBackground = true;
            keyboardWasEnabled = keyboard.enabled;
            if (!keyboardWasEnabled)
                InputSystem.EnableDevice(keyboard);
            inputEnvironmentAdjusted = true;
            InstallPlayerLoopInjection();

            Trace.Clear();
            pendingFailure = string.Empty;
            runningF2DropVerified = false;
            normalElapsedSeconds = 0.0;
            normalSecondsPerTick = 0.0;
            fastElapsedSeconds = 0.0;
            fastSecondsPerTick = 0.0;
            normalTickDelta = 0;
            fastTickDelta = 0;
            maximumObservedTickJump = 0;
            keyboardStateInjectionCount = 0;
            lastInjectedKeyboardState = string.Empty;
            dedicatedWorkerActive = driver.DedicatedSimulationWorkerActiveForDiagnostics;
            dedicatedWorkerIneligibilityReason =
                driver.DedicatedSimulationWorkerIneligibilityReasonForDiagnostics;
            dedicatedWorkerFailure =
                driver.DedicatedSimulationWorkerFailureForDiagnostics?.ToString() ??
                string.Empty;
            dedicatedWorkerLastSubmissionFailure =
                driver.DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics;
            lastObservedTick = driver.CurrentTickIndex;
            startedTimestamp = Stopwatch.GetTimestamp();
            running = true;
            BeginStage(ProbeStage.MeasureNormal);
            ResetMeasurement();
            QueueKeyboardState();
            EditorApplication.update += Observe;
            AppendTrace("probe-start");
            UnityEngine.Debug.Log(
                "[BattleHostControlPlayModeProbe] Started physical F1/F2/F5 trace.");
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null || driver.World == null)
            {
                Fail("Play Mode or the production battle world ended during the probe.");
                return;
            }

            double overallSeconds = ElapsedSeconds(startedTimestamp);
            if (overallSeconds > OverallTimeoutSeconds)
            {
                Fail($"Overall timeout in stage {stage}.");
                return;
            }

            int currentTick = driver.CurrentTickIndex;
            int tickJump = Math.Abs(currentTick - lastObservedTick);
            if (tickJump > maximumObservedTickJump)
                maximumObservedTickJump = tickJump;
            if (currentTick != lastObservedTick)
            {
                lastObservedTick = currentTick;
                AppendTrace("tick");
            }

            stageFrameCount++;
            if (ElapsedSeconds(stageStartedTimestamp) > StageTimeoutSeconds)
            {
                Fail($"Stage {stage} timed out.");
                return;
            }

            switch (stage)
            {
                case ProbeStage.MeasureNormal:
                    ObserveMeasurement(
                        NormalMeasurementTicks,
                        CompleteNormalMeasurement);
                    break;
                case ProbeStage.WaitPause:
                    if (driver.IsPaused)
                    {
                        QueueKeyboardState();
                        pauseTick = currentTick;
                        BeginStage(ProbeStage.VerifyPaused);
                        AppendTrace("f1-paused");
                    }
                    break;
                case ProbeStage.VerifyPaused:
                    if (!VerifyPausedAtTick(pauseTick, "F1 pause"))
                        return;
                    if (ElapsedSeconds(stageStartedTimestamp) >= StablePauseSeconds &&
                        stageFrameCount >= 4)
                    {
                        singleStepStartTick = currentTick;
                        QueueKeyboardState(Key.F2);
                        BeginStage(ProbeStage.WaitSingleStep);
                        AppendTrace("f2-paused-press");
                    }
                    break;
                case ProbeStage.WaitSingleStep:
                    if (!driver.IsPaused)
                    {
                        Fail("Paused F2 unexpectedly resumed the simulation.");
                        return;
                    }
                    if (currentTick > singleStepStartTick + 1)
                    {
                        Fail("Paused F2 advanced more than one tick.");
                        return;
                    }
                    if (currentTick == singleStepStartTick + 1)
                    {
                        QueueKeyboardState();
                        singleStepResultTick = currentTick;
                        BeginStage(ProbeStage.VerifySingleStepStable);
                        AppendTrace("f2-paused-step");
                    }
                    break;
                case ProbeStage.VerifySingleStepStable:
                    if (!VerifyPausedAtTick(singleStepResultTick, "paused F2"))
                        return;
                    if (ElapsedSeconds(stageStartedTimestamp) >= StablePauseSeconds &&
                        stageFrameCount >= 4)
                    {
                        QueueKeyboardState(Key.F1);
                        BeginStage(ProbeStage.WaitResumeForRunningF2);
                        AppendTrace("f1-resume-press");
                    }
                    break;
                case ProbeStage.WaitResumeForRunningF2:
                    if (!driver.IsPaused)
                    {
                        QueueKeyboardState();
                        resumeTick = currentTick;
                        BeginStage(ProbeStage.WaitResumeTick);
                        AppendTrace("f1-resumed");
                    }
                    break;
                case ProbeStage.WaitResumeTick:
                    if (currentTick > resumeTick)
                    {
                        runningF2Tick = currentTick;
                        QueueKeyboardState(Key.F2);
                        BeginStage(ProbeStage.WaitRunningF2Tick);
                        AppendTrace("f2-running-press");
                    }
                    break;
                case ProbeStage.WaitRunningF2Tick:
                    if (driver.IsPaused)
                    {
                        Fail("Running F2 changed the pause state.");
                        return;
                    }
                    if (currentTick > runningF2Tick)
                    {
                        QueueKeyboardState();
                        BeginStage(ProbeStage.ReleaseRunningF2);
                        AppendTrace("f2-running-release");
                    }
                    break;
                case ProbeStage.ReleaseRunningF2:
                    if (stageFrameCount >= 2)
                    {
                        QueueKeyboardState(Key.F1);
                        BeginStage(ProbeStage.WaitPauseAfterRunningF2);
                        AppendTrace("f1-after-running-f2");
                    }
                    break;
                case ProbeStage.WaitPauseAfterRunningF2:
                    if (driver.IsPaused)
                    {
                        QueueKeyboardState();
                        runningF2PausedTick = currentTick;
                        BeginStage(ProbeStage.VerifyRunningF2Dropped);
                        AppendTrace("running-f2-pause-observed");
                    }
                    break;
                case ProbeStage.VerifyRunningF2Dropped:
                    if (!VerifyPausedAtTick(runningF2PausedTick, "running F2 drop"))
                        return;
                    if (ElapsedSeconds(stageStartedTimestamp) >= StablePauseSeconds &&
                        stageFrameCount >= 4)
                    {
                        runningF2DropVerified = true;
                        QueueKeyboardState(Key.F1);
                        BeginStage(ProbeStage.WaitResumeForFast);
                        AppendTrace("f1-resume-for-fast");
                    }
                    break;
                case ProbeStage.WaitResumeForFast:
                    if (!driver.IsPaused)
                    {
                        QueueKeyboardState();
                        resumeTick = currentTick;
                        BeginStage(ProbeStage.WaitTickBeforeFast);
                        AppendTrace("resumed-for-fast");
                    }
                    break;
                case ProbeStage.WaitTickBeforeFast:
                    if (currentTick > resumeTick)
                    {
                        QueueKeyboardState(Key.F5);
                        BeginStage(ProbeStage.WaitFastMode);
                        AppendTrace("f5-fast-press");
                    }
                    break;
                case ProbeStage.WaitFastMode:
                    if (driver.IsFastMode)
                    {
                        QueueKeyboardState();
                        if (Math.Abs(driver.ActiveHostIntervalSeconds - 0.003f) >
                            DebtToleranceSeconds)
                        {
                            Fail(
                                $"F5 selected {driver.ActiveHostIntervalSeconds:R}s, not 0.003s.");
                            return;
                        }
                        BeginStage(ProbeStage.MeasureFast);
                        ResetMeasurement();
                        AppendTrace("f5-fast-active");
                    }
                    break;
                case ProbeStage.MeasureFast:
                    ObserveMeasurement(
                        FastMeasurementTicks,
                        CompleteFastMeasurement);
                    break;
                case ProbeStage.WaitNormalRestore:
                    if (!driver.IsFastMode)
                    {
                        QueueKeyboardState();
                        AppendTrace("f5-normal-restored");
                        FinishFromMeasurements();
                    }
                    break;
            }
        }

        private static void CompleteNormalMeasurement(
            double elapsedSeconds,
            int tickDelta)
        {
            normalElapsedSeconds = elapsedSeconds;
            normalTickDelta = tickDelta;
            normalSecondsPerTick = elapsedSeconds / tickDelta;
            QueueKeyboardState(Key.F1);
            BeginStage(ProbeStage.WaitPause);
            AppendTrace("normal-measured-f1-press");
        }

        private static void CompleteFastMeasurement(
            double elapsedSeconds,
            int tickDelta)
        {
            fastElapsedSeconds = elapsedSeconds;
            fastTickDelta = tickDelta;
            fastSecondsPerTick = elapsedSeconds / tickDelta;
            QueueKeyboardState(Key.F5);
            BeginStage(ProbeStage.WaitNormalRestore);
            AppendTrace("fast-measured-f5-restore");
        }

        private static void ObserveMeasurement(
            int requiredTickDelta,
            Action<double, int> completed)
        {
            int currentTick = driver.CurrentTickIndex;
            long now = Stopwatch.GetTimestamp();
            if (measurementFirstTick < 0)
            {
                if (currentTick == measurementLastTick)
                    return;
                measurementFirstTick = currentTick;
                measurementFirstTimestamp = now;
                measurementLastTick = currentTick;
                measurementLastTimestamp = now;
                return;
            }
            if (currentTick == measurementLastTick)
                return;

            measurementLastTick = currentTick;
            measurementLastTimestamp = now;
            int tickDelta = measurementLastTick - measurementFirstTick;
            if (tickDelta < requiredTickDelta)
                return;

            double elapsed = ElapsedSeconds(
                measurementFirstTimestamp,
                measurementLastTimestamp);
            completed(elapsed, tickDelta);
        }

        private static bool VerifyPausedAtTick(int expectedTick, string label)
        {
            if (!driver.IsPaused)
            {
                Fail($"{label} unexpectedly resumed the simulation.");
                return false;
            }
            if (driver.CurrentTickIndex != expectedTick)
            {
                Fail(
                    $"{label} did not remain stable: expected tick {expectedTick}, " +
                    $"actual {driver.CurrentTickIndex}.");
                return false;
            }
            if (Math.Abs(driver.RemainingAccumulatorTime) > DebtToleranceSeconds)
            {
                Fail(
                    $"{label} retained {driver.RemainingAccumulatorTime:R}s wall-clock debt.");
                return false;
            }
            return true;
        }

        private static void FinishFromMeasurements()
        {
            double ratio = normalSecondsPerTick > 0.0
                ? fastSecondsPerTick / normalSecondsPerTick
                : double.PositiveInfinity;
            bool normalPassed =
                normalSecondsPerTick >= MinimumNormalSecondsPerTick &&
                normalSecondsPerTick <= MaximumNormalSecondsPerTick;
            bool fastPassed =
                fastSecondsPerTick <= MaximumFastSecondsPerTick &&
                ratio <= MaximumFastToNormalRatio;
            if (!normalPassed)
            {
                pendingFailure =
                    $"Normal cadence measured {normalSecondsPerTick:F6}s/tick outside " +
                    $"[{MinimumNormalSecondsPerTick:F3},{MaximumNormalSecondsPerTick:F3}].";
            }
            else if (!fastPassed)
            {
                pendingFailure =
                    $"Fast cadence measured {fastSecondsPerTick:F6}s/tick and ratio " +
                    $"{ratio:F3}; expected <= {MaximumFastSecondsPerTick:F3}s/tick " +
                    $"and <= {MaximumFastToNormalRatio:F2} of Normal.";
            }

            bool passed = string.IsNullOrEmpty(pendingFailure);
            Finish(
                passed,
                passed
                    ? "Synthetic InputSystem F1/F2/F5 production path and real 33/3ms Host cadence passed."
                    : pendingFailure);
        }

        private static void ResetMeasurement()
        {
            measurementFirstTick = -1;
            measurementLastTick = driver.CurrentTickIndex;
            measurementFirstTimestamp = 0;
            measurementLastTimestamp = 0;
        }

        private static void BeginStage(ProbeStage nextStage)
        {
            stage = nextStage;
            stageStartedTimestamp = Stopwatch.GetTimestamp();
            stageFrameCount = 0;
        }

        private static void QueueKeyboardState(params Key[] pressedKeys)
        {
            pendingKeyboardState = pressedKeys ?? Array.Empty<Key>();
            keyboardStatePending = true;
        }

        internal static void InjectPendingKeyboardStateFromPlayerUpdate()
        {
            if (!keyboardStatePending || keyboard == null)
                return;

            var state = new KeyboardState(pendingKeyboardState);
            InputState.Change(keyboard, state, InputUpdateType.Dynamic);
            keyboard.MakeCurrent();
            keyboardStateInjectionCount++;
            lastInjectedKeyboardState = string.Join(",", pendingKeyboardState);
            keyboardStatePending = false;
            AppendTrace("input-injected:" + lastInjectedKeyboardState);
        }

        private static void InstallPlayerLoopInjection()
        {
            originalPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopSystem modified = originalPlayerLoop;
            if (!InsertBeforeScriptRunBehaviourUpdate(ref modified))
            {
                throw new InvalidOperationException(
                    "Could not locate Update.ScriptRunBehaviourUpdate in the PlayerLoop.");
            }
            PlayerLoop.SetPlayerLoop(modified);
            playerLoopInjectionInstalled = true;
        }

        private static bool InsertBeforeScriptRunBehaviourUpdate(
            ref PlayerLoopSystem system)
        {
            PlayerLoopSystem[] children = system.subSystemList;
            if (children == null)
                return false;

            for (int index = 0; index < children.Length; index++)
            {
                if (children[index].type == typeof(Update.ScriptRunBehaviourUpdate))
                {
                    var replacement = new PlayerLoopSystem[children.Length + 1];
                    Array.Copy(children, 0, replacement, 0, index);
                    replacement[index] = new PlayerLoopSystem
                    {
                        type = typeof(BattleHostControlPlayerLoopInjectionMarker),
                        updateDelegate = InjectPendingKeyboardStateFromPlayerUpdate,
                    };
                    Array.Copy(
                        children,
                        index,
                        replacement,
                        index + 1,
                        children.Length - index);
                    system.subSystemList = replacement;
                    return true;
                }

                PlayerLoopSystem child = children[index];
                if (InsertBeforeScriptRunBehaviourUpdate(ref child))
                {
                    children[index] = child;
                    system.subSystemList = children;
                    return true;
                }
            }

            return false;
        }

        private static void AppendTrace(string note)
        {
            if (driver == null)
                return;
            Trace.Add(new TraceRow
            {
                elapsedSeconds = ElapsedSeconds(startedTimestamp),
                stage = stage.ToString(),
                note = note,
                tick = driver.CurrentTickIndex,
                paused = driver.IsPaused,
                fastMode = driver.IsFastMode,
                activeIntervalSeconds = driver.ActiveHostIntervalSeconds,
                accumulatorSeconds = driver.RemainingAccumulatorTime,
                workerActive = driver.DedicatedSimulationWorkerActiveForDiagnostics,
                workerInFlight = driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                probeKeyboardDeviceId = keyboard?.deviceId ?? -1,
                driverKeyboardDeviceId =
                    driver.HostControlPhysicalKeyboardDeviceIdForDiagnostics,
                driverPhysicalSampleCount =
                    driver.HostControlPhysicalSampleCountForDiagnostics,
                driverF1Pressed =
                    driver.HostControlPhysicalF1PressedForDiagnostics,
                driverF2Pressed =
                    driver.HostControlPhysicalF2PressedForDiagnostics,
                driverF5Pressed =
                    driver.HostControlPhysicalF5PressedForDiagnostics,
                driverPhysicalEdgeCount =
                    driver.HostControlPhysicalEdgeCountForDiagnostics,
                driverAppliedCommandCount =
                    driver.HostControlAppliedCommandCountForDiagnostics,
                driverLastPhysicalEdges =
                    driver.HostControlLastPhysicalEdgesForDiagnostics.ToString(),
                driverLastAppliedCommands =
                    driver.HostControlLastAppliedCommandsForDiagnostics.ToString(),
                driverAppliedPausedBefore =
                    driver.HostControlLastAppliedPausedBeforeForDiagnostics,
                driverAppliedPausedAfter =
                    driver.HostControlLastAppliedPausedAfterForDiagnostics,
                driverSetPausedCallCount =
                    driver.SetPausedCallCountForDiagnostics,
                driverSetPausedLastValue =
                    driver.SetPausedLastValueForDiagnostics,
                keyboardEnabled = keyboard != null && keyboard.enabled,
                f1Pressed = keyboard != null && keyboard.f1Key.isPressed,
                f1PressedThisFrame =
                    keyboard != null && keyboard.f1Key.wasPressedThisFrame,
                f2Pressed = keyboard != null && keyboard.f2Key.isPressed,
                f2PressedThisFrame =
                    keyboard != null && keyboard.f2Key.wasPressedThisFrame,
                f5Pressed = keyboard != null && keyboard.f5Key.isPressed,
                f5PressedThisFrame =
                    keyboard != null && keyboard.f5Key.wasPressedThisFrame,
            });
        }

        private static void Fail(string message)
        {
            pendingFailure = message;
            Finish(false, message);
        }

        private static void Finish(bool passed, string message)
        {
            QueueKeyboardState();
            var report = new ProbeResult
            {
                schema = "ntsd28-b1-host-control-play-v1",
                status = passed ? "PASS" : "FAIL",
                message = message,
                scenePath = UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene().path,
                physicalInputSystemPath = false,
                productionInputSystemDevicePath = true,
                diagnosticCommandPathUsed = false,
                syntheticInputSystemKeyboard = true,
                unfocusedInputOverrideApplied = inputEnvironmentAdjusted,
                dedicatedWorkerActive = dedicatedWorkerActive,
                dedicatedWorkerIneligibilityReason =
                    dedicatedWorkerIneligibilityReason,
                dedicatedWorkerFailure = dedicatedWorkerFailure,
                dedicatedWorkerLastSubmissionFailure =
                    dedicatedWorkerLastSubmissionFailure,
                normalIntervalContractSeconds = 0.033,
                fastIntervalContractSeconds = 0.003,
                normalElapsedSeconds = normalElapsedSeconds,
                normalTickDelta = normalTickDelta,
                normalSecondsPerTick = normalSecondsPerTick,
                fastElapsedSeconds = fastElapsedSeconds,
                fastTickDelta = fastTickDelta,
                fastSecondsPerTick = fastSecondsPerTick,
                fastToNormalRatio = normalSecondsPerTick > 0.0
                    ? fastSecondsPerTick / normalSecondsPerTick
                    : 0.0,
                pausedSingleStepDelta = singleStepResultTick - singleStepStartTick,
                runningF2LatentStepObserved =
                    !runningF2DropVerified,
                maximumObservedTickJump = maximumObservedTickJump,
                keyboardStateInjectionCount = keyboardStateInjectionCount,
                lastInjectedKeyboardState = lastInjectedKeyboardState,
                trace = Trace.ToArray(),
            };
            WriteResult(report);
            UnityEngine.Debug.Log(
                $"[BattleHostControlPlayModeProbe] {report.status}: {message}; " +
                $"report={ResultPath()}");
            StopObservation();
        }

        private static void WriteImmediateFailure(string message)
        {
            WriteResult(new ProbeResult
            {
                schema = "ntsd28-b1-host-control-play-v1",
                status = "FAIL",
                message = message,
                scenePath = UnityEngine.SceneManagement.SceneManager
                    .GetActiveScene().path,
                trace = Array.Empty<TraceRow>(),
            });
            UnityEngine.Debug.LogError(
                $"[BattleHostControlPlayModeProbe] FAIL: {message}; report={ResultPath()}");
        }

        private static void WriteResult(ProbeResult report)
        {
            string path = ResultPath();
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static string ResultPath()
        {
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", ResultRelativePath));
        }

        private static double ElapsedSeconds(long started)
        {
            return ElapsedSeconds(started, Stopwatch.GetTimestamp());
        }

        private static double ElapsedSeconds(long started, long ended)
        {
            return (ended - started) / (double)Stopwatch.Frequency;
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            if (keyboard != null)
            {
                var releasedState = new KeyboardState(Array.Empty<Key>());
                InputState.Change(
                    keyboard,
                    releasedState,
                    InputUpdateType.Dynamic);
                InputSystem.RemoveDevice(keyboard);
            }
            keyboardStatePending = false;
            pendingKeyboardState = Array.Empty<Key>();
            if (playerLoopInjectionInstalled)
            {
                PlayerLoop.SetPlayerLoop(originalPlayerLoop);
                playerLoopInjectionInstalled = false;
            }
            if (inputEnvironmentAdjusted)
            {
                InputSystem.settings.backgroundBehavior =
                    previousBackgroundBehavior;
                InputSystem.settings.editorInputBehaviorInPlayMode =
                    previousEditorInputBehavior;
                Application.runInBackground = previousRunInBackground;
                inputEnvironmentAdjusted = false;
            }
            running = false;
            driver = null;
            keyboard = null;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string schema;
            public string status;
            public string message;
            public string scenePath;
            public bool physicalInputSystemPath;
            public bool productionInputSystemDevicePath;
            public bool diagnosticCommandPathUsed;
            public bool syntheticInputSystemKeyboard;
            public bool unfocusedInputOverrideApplied;
            public bool dedicatedWorkerActive;
            public string dedicatedWorkerIneligibilityReason;
            public string dedicatedWorkerFailure;
            public string dedicatedWorkerLastSubmissionFailure;
            public double normalIntervalContractSeconds;
            public double fastIntervalContractSeconds;
            public double normalElapsedSeconds;
            public int normalTickDelta;
            public double normalSecondsPerTick;
            public double fastElapsedSeconds;
            public int fastTickDelta;
            public double fastSecondsPerTick;
            public double fastToNormalRatio;
            public int pausedSingleStepDelta;
            public bool runningF2LatentStepObserved;
            public int maximumObservedTickJump;
            public int keyboardStateInjectionCount;
            public string lastInjectedKeyboardState;
            public TraceRow[] trace;
        }

        [Serializable]
        private sealed class TraceRow
        {
            public double elapsedSeconds;
            public string stage;
            public string note;
            public int tick;
            public bool paused;
            public bool fastMode;
            public float activeIntervalSeconds;
            public float accumulatorSeconds;
            public bool workerActive;
            public bool workerInFlight;
            public int probeKeyboardDeviceId;
            public int driverKeyboardDeviceId;
            public long driverPhysicalSampleCount;
            public bool driverF1Pressed;
            public bool driverF2Pressed;
            public bool driverF5Pressed;
            public long driverPhysicalEdgeCount;
            public long driverAppliedCommandCount;
            public string driverLastPhysicalEdges;
            public string driverLastAppliedCommands;
            public bool driverAppliedPausedBefore;
            public bool driverAppliedPausedAfter;
            public long driverSetPausedCallCount;
            public bool driverSetPausedLastValue;
            public bool keyboardEnabled;
            public bool f1Pressed;
            public bool f1PressedThisFrame;
            public bool f2Pressed;
            public bool f2PressedThisFrame;
            public bool f5Pressed;
            public bool f5PressedThisFrame;
        }

        private enum ProbeStage
        {
            MeasureNormal,
            WaitPause,
            VerifyPaused,
            WaitSingleStep,
            VerifySingleStepStable,
            WaitResumeForRunningF2,
            WaitResumeTick,
            WaitRunningF2Tick,
            ReleaseRunningF2,
            WaitPauseAfterRunningF2,
            VerifyRunningF2Dropped,
            WaitResumeForFast,
            WaitTickBeforeFast,
            WaitFastMode,
            MeasureFast,
            WaitNormalRestore,
        }
    }

    internal sealed class BattleHostControlPlayerLoopInjectionMarker
    {
    }
}
#endif
