#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NTSD.Animation.LF2Objects;
using NTSD.Input;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Explicit Editor-only probe for the real NTSD_Battle scene. It queues the
    /// physical combo sequences through the Input System and records the
    /// resulting fixed-tick combo/frame progression.
    /// </summary>
    public static class BattleComboPlayModeProbeEditor
    {
        private const string DownJumpMenuPath = "NTSD/验证/运行组合键PlayMode探针";
        private const string ForwardAttackMenuPath = "NTSD/验证/运行组合键PlayMode探针-防前攻";
        private const string DownJumpResultPath = "Temp/NTSD_R3_COMBO_PLAY.result.json";
        private const string ForwardAttackResultPath = "Temp/NTSD_R3_COMBO_PLAY.forward-attack.result.json";
        private const string DownJumpRequestPath =
            "Temp/NTSD_R3_COMBO_PLAY.request";
        private const string ForwardAttackRequestPath =
            "Temp/NTSD_R3_COMBO_PLAY.forward-attack.request";
        private const int ObservationTailTicks = 18;
        private const int TimeoutTicks = 90;
        private const int MaximumPressAttemptsPerStep = 8;
        private const int MinimumQueuedStateHoldTicks = 2;

        private static readonly List<TraceRow> Trace = new List<TraceRow>(128);
        private static readonly FieldInfo FirstPlayerField =
            typeof(BattleTestBootstrap).GetField(
                "firstPlayerLf2",
                BindingFlags.Instance | BindingFlags.NonPublic);
        private static LF2Character character;
        private static SimulationTickDriver driver;
        private static Keyboard keyboard;
        private static int startTick;
        private static int step1Tick;
        private static int step2Tick;
        private static int step3Tick;
        private static int releaseTick;
        private static int expectedTargetFrame;
        private static int baselineObjectCount;
        private static int peakObjectCount;
        private static int lastObservedTick;
        private static int lastInputPulseTick;
        private static int step1PressAttempts;
        private static int step2PressAttempts;
        private static int step3PressAttempts;
        private static bool step1Seen;
        private static bool step2Seen;
        private static bool targetFrameSeen;
        private static bool retryReleaseQueued;
        private static bool running;
        private static bool requestBatchActive;
        private static ProbeKind activeKind;
        private static Key secondPhysicalKey;
        private static Key thirdPhysicalKey;
        private static string comboLabel;
        private static string resultRelativePath;

        [InitializeOnLoadMethod]
        private static void RegisterRequestPoller()
        {
            EditorApplication.update -= PollRequest;
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || running)
            {
                return;
            }

            if (!HasPendingRequest() || !IsLiveBattleReady())
                return;

            string downJump = ProjectPath(DownJumpRequestPath);
            if (File.Exists(downJump))
            {
                if (!CanStartProbe(ProbeKind.DownJump))
                    return;
                requestBatchActive = true;
                File.Delete(downJump);
                DeletePriorResult(DownJumpResultPath);
                RunProbe(ProbeKind.DownJump);
                return;
            }

            string forwardAttack = ProjectPath(ForwardAttackRequestPath);
            if (File.Exists(forwardAttack))
            {
                if (!CanStartProbe(ProbeKind.ForwardAttack))
                    return;
                requestBatchActive = true;
                File.Delete(forwardAttack);
                DeletePriorResult(ForwardAttackResultPath);
                RunProbe(ProbeKind.ForwardAttack);
            }
        }

        [MenuItem(DownJumpMenuPath)]
        public static void RunFromMenu()
        {
            RunProbe(ProbeKind.DownJump);
        }

        [MenuItem(ForwardAttackMenuPath)]
        public static void RunForwardAttackFromMenu()
        {
            RunProbe(ProbeKind.ForwardAttack);
        }

        private static void RunProbe(ProbeKind kind)
        {
            activeKind = kind;
            resultRelativePath = kind == ProbeKind.DownJump
                ? DownJumpResultPath
                : ForwardAttackResultPath;
            StopObservation();
            if (!EditorApplication.isPlaying)
            {
                WriteFailure("Play Mode is not active.");
                return;
            }

            BattleTestBootstrap bootstrap = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            character = FirstPlayerField?.GetValue(bootstrap) as LF2Character;
            driver = SimulationTickDriver.Instance;
            if (bootstrap == null || character == null || driver?.World == null ||
                character.Controller?.InputBuffer == null || character.Runtime == null ||
                character.Frame?.D == null)
            {
                WriteFailure("The live battle player/input/runtime is not ready.");
                return;
            }

            keyboard = Keyboard.current;
            if (keyboard == null)
            {
                WriteFailure("Unity Input System has no current Keyboard device.");
                return;
            }

            bool facingLeft = string.Equals(
                character.Runtime.Dir,
                "left",
                StringComparison.OrdinalIgnoreCase);
            if (kind == ProbeKind.DownJump)
            {
                expectedTargetFrame = character.Frame.D.hit_Dj;
                secondPhysicalKey = Key.S;
                thirdPhysicalKey = Key.K;
                comboLabel = "DDJ";
            }
            else
            {
                expectedTargetFrame = character.Frame.D.hit_Fa;
                secondPhysicalKey = facingLeft ? Key.A : Key.D;
                thirdPhysicalKey = Key.J;
                comboLabel = facingLeft ? "DLA" : "DRA";
            }
            if (expectedTargetFrame == 0)
            {
                WriteFailure(
                    $"Current frame {character.Frame.N} has no authored target for {comboLabel}.");
                return;
            }

            int currentTick = driver.CurrentTickIndex;
            startTick = currentTick;
            step1Tick = -1;
            step2Tick = -1;
            step3Tick = -1;
            releaseTick = -1;
            baselineObjectCount = driver.World.ObjectCount;
            peakObjectCount = baselineObjectCount;
            lastObservedTick = currentTick;
            lastInputPulseTick = currentTick;
            step1PressAttempts = 1;
            step2PressAttempts = 0;
            step3PressAttempts = 0;
            step1Seen = false;
            step2Seen = false;
            targetFrameSeen = false;
            retryReleaseQueued = false;
            Trace.Clear();

            running = true;
            EditorApplication.update += Observe;
            // Alignment contract: R3-COMBO-001. Queue physical device states so
            // CharacterInputModule and canonical FrameInputSet remain in the path.
            QueueKeyboardState(Key.L);
            Debug.Log(
                $"[BattleComboPlayModeProbe] queued {activeKind} physical L at tick {startTick}; " +
                $"combo={comboLabel},target={expectedTargetFrame}.");
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || character == null || driver?.World == null)
            {
                Finish(false, "Play Mode or live character ended before observation completed.");
                return;
            }

            int tick = driver.CurrentTickIndex;
            if (tick <= lastObservedTick)
                return;

            lastObservedTick = tick;
            int frame = character.Frame?.N ?? -1;
            byte combo = ResolveComboValue();
            int objectCount = driver.World.ObjectCount;
            peakObjectCount = Math.Max(peakObjectCount, objectCount);
            Trace.Add(new TraceRow
            {
                tick = tick,
                frame = frame,
                comboValue = combo,
                comboDdj = character.Runtime.ComboDdj,
                comboDra = character.Runtime.ComboDra,
                comboDla = character.Runtime.ComboDla,
                exactDdj = character.Runtime.NativeInputProxy.ComboState[5],
                exactHorizontal = character.Runtime.NativeInputProxy.ComboState[0],
                cdDefend = character.Runtime.CdDefend,
                cdDown = character.Runtime.CdDown,
                cdJump = character.Runtime.CdJump,
                cdAttack = character.Runtime.CdAttack,
                cdRight = character.Runtime.CdRight,
                cdLeft = character.Runtime.CdLeft,
                objectCount = objectCount,
            });

            if (!step1Seen && combo == 1)
            {
                step1Seen = true;
                step1Tick = tick;
                QueueKeyboardState(secondPhysicalKey);
                step2PressAttempts = 1;
                lastInputPulseTick = tick;
                retryReleaseQueued = false;
            }
            else if (step1Seen && !step2Seen && IsDirectionStep(combo))
            {
                step2Seen = true;
                step2Tick = tick;
                QueueKeyboardState(thirdPhysicalKey);
                step3PressAttempts = 1;
                lastInputPulseTick = tick;
                retryReleaseQueued = false;
            }
            if (step2Seen && frame == expectedTargetFrame)
            {
                targetFrameSeen = true;
                if (step3Tick < 0)
                {
                    step3Tick = tick;
                    releaseTick = tick;
                    QueueKeyboardState();
                }
            }

            if (running && !RetryPendingPhysicalInput(tick, combo))
            {
                Finish(
                    false,
                    $"Physical {comboLabel} input did not reach canonical FrameInputSet " +
                    $"within {MaximumPressAttemptsPerStep} press attempts.");
                return;
            }

            if (targetFrameSeen && tick >= step3Tick + ObservationTailTicks)
            {
                bool passed = step1Seen && step2Seen;
                Finish(
                    passed,
                    passed
                        ? $"Live NTSD_Battle {comboLabel} reached the authored target frame."
                        : "The target frame was reached without observing both persisted combo steps.");
                return;
            }

            if (tick > startTick + TimeoutTicks)
            {
                Finish(false, $"Timed out before the authored {comboLabel} target frame was observed.");
            }
        }

        private static byte ResolveComboValue()
        {
            byte[] exact = character.Runtime.NativeInputProxy.ComboState;
            return activeKind == ProbeKind.DownJump ? exact[5] : exact[0];
        }

        private static bool IsDirectionStep(byte combo)
        {
            if (activeKind == ProbeKind.DownJump || comboLabel != "DLA")
                return combo == 2;
            return combo == 3;
        }

        private static bool RetryPendingPhysicalInput(int tick, byte combo)
        {
            if (!step1Seen)
            {
                return PulsePhysicalState(
                    tick,
                    Key.L,
                    ref step1PressAttempts);
            }

            if (!step2Seen)
            {
                return PulsePhysicalState(
                    tick,
                    secondPhysicalKey,
                    ref step2PressAttempts);
            }

            if (!targetFrameSeen && IsDirectionStep(combo))
            {
                return PulsePhysicalState(
                    tick,
                    thirdPhysicalKey,
                    ref step3PressAttempts);
            }

            return true;
        }

        private static bool PulsePhysicalState(
            int tick,
            Key pressedKey,
            ref int pressAttempts)
        {
            // NTSD 2.8 2tu polls human input every other logic tick. Keep both
            // pressed and released device states alive long enough to cross a poll.
            if (tick - lastInputPulseTick < MinimumQueuedStateHoldTicks)
                return true;

            if (!retryReleaseQueued)
            {
                if (pressAttempts >= MaximumPressAttemptsPerStep)
                    return false;
                QueueKeyboardState();
                retryReleaseQueued = true;
            }
            else
            {
                QueueKeyboardState(pressedKey);
                pressAttempts++;
                retryReleaseQueued = false;
            }

            lastInputPulseTick = tick;
            return true;
        }

        private static void QueueKeyboardState(params Key[] pressedKeys)
        {
            if (keyboard != null)
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(pressedKeys));
        }

        private static void Finish(bool passed, string message)
        {
            ProbeResult result = new ProbeResult
            {
                status = passed ? "PASS" : "FAIL",
                message = message,
                probeKind = activeKind.ToString(),
                comboLabel = comboLabel,
                secondPhysicalKey = secondPhysicalKey.ToString(),
                thirdPhysicalKey = thirdPhysicalKey.ToString(),
                expectedTargetFrame = expectedTargetFrame,
                step1Tick = step1Tick,
                step2Tick = step2Tick,
                step3Tick = step3Tick,
                step1Seen = step1Seen,
                step2Seen = step2Seen,
                targetFrameSeen = targetFrameSeen,
                step1PressAttempts = step1PressAttempts,
                step2PressAttempts = step2PressAttempts,
                step3PressAttempts = step3PressAttempts,
                baselineObjectCount = baselineObjectCount,
                peakObjectCount = peakObjectCount,
                trace = Trace.ToArray(),
            };
            File.WriteAllText(ResultPath(), JsonUtility.ToJson(result, true));
            Debug.Log($"[BattleComboPlayModeProbe] {result.status}: {message}");
            StopObservation();
            CompleteRequestBatchIfFinished();
        }

        private static void WriteFailure(string message)
        {
            ProbeResult result = new ProbeResult
            {
                status = "FAIL",
                message = message,
                probeKind = activeKind.ToString(),
                comboLabel = comboLabel,
                trace = Array.Empty<TraceRow>(),
            };
            File.WriteAllText(ResultPath(), JsonUtility.ToJson(result, true));
            Debug.LogError($"[BattleComboPlayModeProbe] FAIL: {message}");
            StopObservation();
            CompleteRequestBatchIfFinished();
        }

        private static string ResultPath()
        {
            return ProjectPath(resultRelativePath);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void CompleteRequestBatchIfFinished()
        {
            if (!requestBatchActive || HasPendingRequest())
                return;

            EditorApplication.delayCall += ExitPlayModeAfterRequestBatch;
        }

        private static void ExitPlayModeAfterRequestBatch()
        {
            if (!requestBatchActive || running || HasPendingRequest())
                return;

            requestBatchActive = false;
            if (EditorApplication.isPlaying)
                EditorApplication.isPlaying = false;
        }

        private static bool HasPendingRequest()
        {
            return File.Exists(ProjectPath(DownJumpRequestPath)) ||
                File.Exists(ProjectPath(ForwardAttackRequestPath));
        }

        private static bool IsLiveBattleReady()
        {
            BattleTestBootstrap bootstrap =
                UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            LF2Character player =
                FirstPlayerField?.GetValue(bootstrap) as LF2Character;
            SimulationTickDriver liveDriver = SimulationTickDriver.Instance;
            return bootstrap != null && player != null &&
                liveDriver?.World != null &&
                player.Controller?.InputBuffer != null &&
                player.Runtime != null && player.Frame?.D != null;
        }

        private static bool CanStartProbe(ProbeKind kind)
        {
            BattleTestBootstrap bootstrap =
                UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            LF2Character player =
                FirstPlayerField?.GetValue(bootstrap) as LF2Character;
            if (player?.Frame?.D == null)
                return false;

            return kind == ProbeKind.DownJump
                ? player.Frame.D.hit_Dj != 0
                : player.Frame.D.hit_Fa != 0;
        }

        private static void DeletePriorResult(string relativePath)
        {
            string path = ProjectPath(relativePath);
            if (File.Exists(path))
                File.Delete(path);
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            if (keyboard != null)
                QueueKeyboardState();
            keyboard = null;
            running = false;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status;
            public string message;
            public string probeKind;
            public string comboLabel;
            public string secondPhysicalKey;
            public string thirdPhysicalKey;
            public int expectedTargetFrame;
            public int step1Tick;
            public int step2Tick;
            public int step3Tick;
            public bool step1Seen;
            public bool step2Seen;
            public bool targetFrameSeen;
            public int step1PressAttempts;
            public int step2PressAttempts;
            public int step3PressAttempts;
            public int baselineObjectCount;
            public int peakObjectCount;
            public TraceRow[] trace;
        }

        [Serializable]
        private sealed class TraceRow
        {
            public int tick;
            public int frame;
            public int comboValue;
            public int comboDdj;
            public int comboDra;
            public int comboDla;
            public int exactDdj;
            public int exactHorizontal;
            public int cdDefend;
            public int cdDown;
            public int cdJump;
            public int cdAttack;
            public int cdRight;
            public int cdLeft;
            public int objectCount;
        }

        private enum ProbeKind
        {
            DownJump,
            ForwardAttack,
        }
    }
}
#endif
