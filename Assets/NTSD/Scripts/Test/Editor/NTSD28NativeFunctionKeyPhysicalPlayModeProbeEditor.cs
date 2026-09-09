#if UNITY_EDITOR
using System;
using System.IO;

using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NTSD.Test.Editor
{
    public static class NTSD28NativeFunctionKeyPhysicalPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/NTSD28/B2/Run Function-Key Physical Play Probe";
        private const string RequestRelativePath =
            "Temp/NTSD28FunctionKeyPhysicalPlay.request";
        private const string ResultRelativePath =
            "Temp/NTSD28FunctionKeyPhysicalPlay.result.json";
        private const int TimeoutEditorUpdates = 1800;

        private static SimulationTickDriver driver;
        private static NTSD28NativeFunctionKeySessionState state;
        private static Keyboard keyboard;
        private static ProbeReport report;
        private static CarrierBaseline baseline;
        private static ProbePhase phase;
        private static int targetFrame;
        private static int editorUpdates;
        private static bool previousPaused;
        private static bool running;

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

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
            keyboard = Keyboard.current;
            if (driver?.World?.Runtime?.FunctionKeys == null ||
                driver.World.RuntimeDataCatalog?.IsReady != true ||
                keyboard == null)
            {
                WriteImmediateFailure(
                    "Production driver, world, runtime catalog, function-key carrier, or Keyboard is not ready.");
                return;
            }

            if (driver.LifecycleState == BattleRuntimeLifecycleState.Preparing)
                driver.SetPaused(false);
            if (driver.LifecycleState != BattleRuntimeLifecycleState.Running)
            {
                WriteImmediateFailure(
                    "Production driver did not enter Running before the physical probe.");
                return;
            }

            state = driver.World.Runtime.FunctionKeys;
            baseline = CarrierBaseline.Capture(state);
            previousPaused = driver.IsPaused;
            driver.SetPaused(true);
            state.ResetForBattle();
            driver.ConsumeNativeFunctionKeyLeaveBattleRequestForDiagnostics();
            driver.ConsumeNativeFunctionKeyMaintenanceCommandForDiagnostics();
            report = new ProbeReport
            {
                status = "RUNNING",
                startTick = driver.CurrentTickIndex,
                keyboardDeviceId = keyboard.deviceId,
                lifecycle = driver.LifecycleState.ToString(),
            };
            editorUpdates = 0;
            running = true;
            QueueKeys();
            AdvanceTo(ProbePhase.InitialReleaseObserved);
            EditorApplication.update -= Observe;
            EditorApplication.update += Observe;
        }

        private static void PollRequest()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || running)
            {
                return;
            }

            string requestPath = ProjectPath(RequestRelativePath);
            if (!File.Exists(requestPath))
                return;

            SimulationTickDriver current = SimulationTickDriver.Instance;
            if (current?.World?.Runtime?.FunctionKeys == null ||
                current.World.RuntimeDataCatalog?.IsReady != true ||
                Keyboard.current == null)
            {
                return;
            }

            File.Delete(requestPath);
            string resultPath = ProjectPath(ResultRelativePath);
            if (File.Exists(resultPath))
                File.Delete(resultPath);
            RunFromMenu();
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver == null ||
                driver.World == null || keyboard == null)
            {
                Fail("Play Mode, driver, world, or Keyboard ended before completion.");
                return;
            }

            editorUpdates++;
            if (editorUpdates > TimeoutEditorUpdates)
            {
                Fail($"Timed out in phase {phase}.");
                return;
            }
            if (Time.frameCount < targetFrame)
                return;

            try
            {
                switch (phase)
                {
                    case ProbePhase.InitialReleaseObserved:
                        QueueKeys(Key.F6);
                        AdvanceTo(ProbePhase.F6Pressed);
                        break;
                    case ProbePhase.F6Pressed:
                        QueueKeys();
                        AdvanceTo(ProbePhase.F6Released);
                        break;
                    case ProbePhase.F6Released:
                        StepAndRequireAccepted(0x10);
                        Require(state.F6EventCount == 1 &&
                                !state.HitResourceEnabled,
                            "Physical F6 did not toggle hit-resource exactly once.");
                        Require(BattleTestBootstrap.NativeFunctionKeysOwnBattle(driver),
                            "BattleTestBootstrap did not yield formal F6/F7 ownership.");
                        report.f6Passed = true;
                        state.ResetForBattle();
                        QueueKeys(Key.F7);
                        AdvanceTo(ProbePhase.F7Pressed);
                        break;
                    case ProbePhase.F7Pressed:
                        QueueKeys();
                        AdvanceTo(ProbePhase.F7Released);
                        break;
                    case ProbePhase.F7Released:
                        StepAndRequireAccepted(0x20);
                        Require(state.F7EventCount == 1 && state.PendingFullMp,
                            "Physical F7 did not publish the native full-MP handoff.");
                        Require(driver.World.InitStatsRequest == 0,
                            "Physical F7 leaked into the legacy all-stats request.");
                        report.f7Passed = true;
                        state.ResetForBattle();
                        QueueKeys(Key.F8, Key.F9);
                        AdvanceTo(ProbePhase.F8F9Pressed);
                        break;
                    case ProbePhase.F8F9Pressed:
                        QueueKeys();
                        AdvanceTo(ProbePhase.F8F9Released);
                        break;
                    case ProbePhase.F8F9Released:
                        StepAndRequireAccepted(0xC0);
                        Require(state.F8EventCount == 1 &&
                                state.F9EventCount == 1 &&
                                state.PendingObjectCommand ==
                                    NTSD28NativeFunctionKeyPendingObjectCommand.TerminateObjects,
                            "Physical same-window F8/F9 did not preserve fixed F9 priority.");
                        Require(driver.World.Mode2Request == 0,
                            "Physical F8/F9 leaked into the legacy mode2 request.");
                        report.f8F9Passed = true;
                        state.ResetForBattle();
                        QueueKeys(Key.F3, Key.F6);
                        AdvanceTo(ProbePhase.F3F6Pressed);
                        break;
                    case ProbePhase.F3F6Pressed:
                        QueueKeys();
                        AdvanceTo(ProbePhase.F3F6Released);
                        break;
                    case ProbePhase.F3F6Released:
                        StepAndRequireAccepted(0x04);
                        Require(state.LockState == 2 && state.F6EventCount == 0,
                            "Physical F3 did not lock the later same-window F6 command.");
                        report.f3LockPassed = true;
                        state.ResetForBattle();
                        QueueKeys(Key.F10);
                        AdvanceTo(ProbePhase.F10Pressed);
                        break;
                    case ProbePhase.F10Pressed:
                        QueueKeys();
                        AdvanceTo(ProbePhase.F10Released);
                        break;
                    case ProbePhase.F10Released:
                        StepAndRequireAccepted(0x00);
                        Require(!driver.NativeFunctionKeyLeaveBattleRequestedForDiagnostics &&
                                driver.NativeFunctionKeyMaintenanceCommandForDiagnostics ==
                                    NTSD28NativeFunctionKeyMaintenanceCommand.None,
                            "Plain physical F10 produced a non-native handoff.");
                        report.f10NoActionPassed = true;
                        QueueKeys(Key.F11, Key.F12);
                        AdvanceTo(ProbePhase.F11F12Pressed);
                        break;
                    case ProbePhase.F11F12Pressed:
                        Require(
                            driver.NativeFunctionKeyContinuousHostCommandForDiagnostics ==
                                NTSD28NativeFunctionKeyHostCommand.VolumeUp,
                            "Physical F11+F12 did not preserve F12 continuous priority.");
                        report.f11F12HeldPassed = true;
                        QueueKeys();
                        AdvanceTo(ProbePhase.F11F12Released);
                        break;
                    case ProbePhase.F11F12Released:
                        Require(
                            driver.NativeFunctionKeyContinuousHostCommandForDiagnostics ==
                                NTSD28NativeFunctionKeyHostCommand.None,
                            "Physical F11/F12 release did not clear continuous handoff.");
                        report.f11F12ReleasePassed = true;
                        QueueKeys(Key.F4);
                        AdvanceTo(ProbePhase.F4Pressed);
                        break;
                    case ProbePhase.F4Pressed:
                        Require(driver.NativeFunctionKeyLeaveBattleRequestedForDiagnostics,
                            "Physical F4 did not publish the deferred leave handoff.");
                        QueueKeys();
                        AdvanceTo(ProbePhase.F4Released);
                        break;
                    case ProbePhase.F4Released:
                        Require(
                            driver.ConsumeNativeFunctionKeyLeaveBattleRequestForDiagnostics(),
                            "Physical F4 leave handoff was lost before its owner consumed it.");
                        report.f4HandoffPassed = true;
                        QueueKeys(Key.LeftCtrl, Key.F10);
                        AdvanceTo(ProbePhase.ControlF10Pressed);
                        break;
                    case ProbePhase.ControlF10Pressed:
                        Require(
                            driver.NativeFunctionKeyMaintenanceCommandForDiagnostics ==
                                NTSD28NativeFunctionKeyMaintenanceCommand.DiscardPendingRecording,
                            "Physical Ctrl+F10 did not publish recording maintenance.");
                        QueueKeys();
                        AdvanceTo(ProbePhase.ControlF10Released);
                        break;
                    case ProbePhase.ControlF10Released:
                        Require(
                            driver.ConsumeNativeFunctionKeyMaintenanceCommandForDiagnostics() ==
                                NTSD28NativeFunctionKeyMaintenanceCommand.DiscardPendingRecording,
                            "Physical Ctrl+F10 maintenance handoff was lost.");
                        report.controlF10Passed = true;
                        FinishSuccess();
                        break;
                }
            }
            catch (Exception exception)
            {
                Fail(exception.ToString());
            }
        }

        private static void StepAndRequireAccepted(byte expectedAccepted)
        {
            int before = driver.CurrentTickIndex;
            Require(driver.StepOneTick(true, false),
                $"Production manual tick was rejected in phase {phase}.");
            Require(driver.CurrentTickIndex == before + 1,
                $"Production manual tick advanced {driver.CurrentTickIndex - before} ticks.");
            Require(state.LastAcceptedEventByte == expectedAccepted,
                $"Accepted event byte was 0x{state.LastAcceptedEventByte:X2}, " +
                $"expected 0x{expectedAccepted:X2} in phase {phase}.");
        }

        private static void QueueKeys(params Key[] keys)
        {
            if (keyboard == null)
                throw new InvalidOperationException("Keyboard disappeared during probe.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        }

        private static void AdvanceTo(ProbePhase next)
        {
            phase = next;
            targetFrame = Time.frameCount + 2;
        }

        private static void FinishSuccess()
        {
            report.status = "PASS";
            report.endTick = driver.CurrentTickIndex;
            CleanupAndWrite();
        }

        private static void Fail(string message)
        {
            report ??= new ProbeReport();
            report.status = "FAIL";
            report.error = message;
            report.endTick = driver != null ? driver.CurrentTickIndex : -1;
            CleanupAndWrite();
        }

        private static void CleanupAndWrite()
        {
            try
            {
                if (keyboard != null)
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                if (state != null)
                    baseline.Restore(state);
                if (driver != null)
                {
                    driver.ConsumeNativeFunctionKeyLeaveBattleRequestForDiagnostics();
                    driver.ConsumeNativeFunctionKeyMaintenanceCommandForDiagnostics();
                    driver.SetPaused(previousPaused);
                }
            }
            catch (Exception cleanupError)
            {
                report.status = "FAIL";
                report.error = string.IsNullOrEmpty(report.error)
                    ? cleanupError.ToString()
                    : report.error + "\nCLEANUP: " + cleanupError;
            }

            string resultPath = ProjectPath(ResultRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
            StopObservation();
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            running = false;
            driver = null;
            state = null;
            keyboard = null;
            phase = ProbePhase.None;
        }

        private static void WriteImmediateFailure(string message)
        {
            var immediate = new ProbeReport
            {
                status = "FAIL",
                error = message,
                endTick = -1,
            };
            string resultPath = ProjectPath(ResultRelativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            File.WriteAllText(resultPath, JsonUtility.ToJson(immediate, true));
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(
                Application.dataPath,
                "..",
                relativePath));
        }

        private enum ProbePhase
        {
            None,
            InitialReleaseObserved,
            F6Pressed,
            F6Released,
            F7Pressed,
            F7Released,
            F8F9Pressed,
            F8F9Released,
            F3F6Pressed,
            F3F6Released,
            F10Pressed,
            F10Released,
            F11F12Pressed,
            F11F12Released,
            F4Pressed,
            F4Released,
            ControlF10Pressed,
            ControlF10Released,
        }

        private struct CarrierBaseline
        {
            internal int lockState;
            internal bool hitResourceEnabled;
            internal uint f6;
            internal uint f7;
            internal uint f8;
            internal uint f9;
            internal bool pendingFullMp;
            internal NTSD28NativeFunctionKeyPendingObjectCommand pendingObject;
            internal byte queued;
            internal byte accepted;

            internal static CarrierBaseline Capture(
                NTSD28NativeFunctionKeySessionState source)
            {
                return new CarrierBaseline
                {
                    lockState = source.LockState,
                    hitResourceEnabled = source.HitResourceEnabled,
                    f6 = source.F6EventCount,
                    f7 = source.F7EventCount,
                    f8 = source.F8EventCount,
                    f9 = source.F9EventCount,
                    pendingFullMp = source.PendingFullMp,
                    pendingObject = source.PendingObjectCommand,
                    queued = source.QueuedEventByte,
                    accepted = source.LastAcceptedEventByte,
                };
            }

            internal void Restore(NTSD28NativeFunctionKeySessionState destination)
            {
                destination.RestoreForSnapshot(
                    lockState,
                    hitResourceEnabled,
                    f6,
                    f7,
                    f8,
                    f9,
                    pendingFullMp,
                    pendingObject,
                    queued,
                    accepted);
            }
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public string status;
            public string error;
            public string lifecycle;
            public int keyboardDeviceId;
            public int startTick;
            public int endTick;
            public bool f6Passed;
            public bool f7Passed;
            public bool f8F9Passed;
            public bool f3LockPassed;
            public bool f10NoActionPassed;
            public bool f11F12HeldPassed;
            public bool f11F12ReleasePassed;
            public bool f4HandoffPassed;
            public bool controlF10Passed;
        }
    }
}
#endif
