#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NTSD.Test.Editor
{
    /// <summary>
    /// Editor-only physical-input witness for the live NTSD_Battle movement chain.
    /// Alignment contract: R8-JOINTMOVE-PROBE-001.
    /// </summary>
    public static class BattlePhysicalMovementPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run Physical Movement Jump Landing Play Probe";
        private const string ResultPath =
            "Temp/NTSD_R8_PHYSICAL_MOVEMENT_PLAY.result.json";
        private const int NeutralTimeoutTicks = 180;
        private const int ProbeTimeoutTicks = 300;
        private const int RightHoldTicksBeforeJump = 3;
        private const int AirTicksBeforeRelease = 3;
        private const int MaximumPressAttemptsPerPhase = 8;

        private static readonly List<TraceRow> Trace = new List<TraceRow>(384);

        private static LF2Character character;
        private static CharacterInputModule inputModule;
        private static SimulationTickDriver driver;
        private static Keyboard keyboard;
        private static LF2CharacterData characterData;
        private static ProbePhase phase;
        private static int startTick;
        private static int lastObservedTick;
        private static int neutralTick;
        private static int rightEdgeTick;
        private static int jumpEdgeTick;
        private static int airborneTick;
        private static int releaseTick;
        private static int landingTick;
        private static int baselineObjectCount;
        private static int baselineXInt;
        private static int jumpStartXInt;
        private static int firstAirXInt;
        private static int lastInputPulseTick;
        private static int rightPressAttempts;
        private static int jumpPressAttempts;
        private static double firstAirVx;
        private static double firstAirVy;
        private static bool rightInputSeen;
        private static bool jumpInputSeen;
        private static bool airborneSeen;
        private static bool horizontalAirMotionSeen;
        private static bool landedSeen;
        private static bool moveActionEnabledAtStart;
        private static bool jumpActionEnabledAtStart;
        private static int keyboardDeviceId;
        private static bool retryReleaseQueued;
        private static bool running;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            if (!EditorApplication.isPlaying)
            {
                WriteFailure("Play Mode is not active.");
                return;
            }

            BattleTestBootstrap bootstrap = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            FieldInfo firstPlayerField = typeof(BattleTestBootstrap).GetField(
                "firstPlayerLf2",
                BindingFlags.Instance | BindingFlags.NonPublic);
            character = firstPlayerField?.GetValue(bootstrap) as LF2Character;
            inputModule = character?.Controller as CharacterInputModule;
            driver = SimulationTickDriver.Instance;
            keyboard = Keyboard.current;
            characterData = character?.FrameCache?.Wrapper?.characterData;
            if (bootstrap == null || character == null || driver?.World == null ||
                character.Runtime == null || character.Frame?.D == null ||
                character.Controller?.InputBuffer == null || inputModule == null || keyboard == null ||
                characterData == null)
            {
                WriteFailure("The live battle player, Input System, or character DAT is not ready.");
                return;
            }

            if (Math.Abs(characterData.jump_distance) < 0.0001f ||
                Math.Abs(characterData.jump_height) < 0.0001f)
            {
                WriteFailure(
                    $"Current DAT has no usable jump contract: " +
                    $"distance={characterData.jump_distance:R}, height={characterData.jump_height:R}.");
                return;
            }

            startTick = driver.CurrentTickIndex;
            lastObservedTick = startTick;
            neutralTick = -1;
            rightEdgeTick = -1;
            jumpEdgeTick = -1;
            airborneTick = -1;
            releaseTick = -1;
            landingTick = -1;
            baselineObjectCount = driver.World.ObjectCount;
            baselineXInt = character.Runtime.XInt;
            jumpStartXInt = baselineXInt;
            firstAirXInt = baselineXInt;
            lastInputPulseTick = startTick;
            rightPressAttempts = 0;
            jumpPressAttempts = 0;
            firstAirVx = 0.0;
            firstAirVy = 0.0;
            rightInputSeen = false;
            jumpInputSeen = false;
            airborneSeen = false;
            horizontalAirMotionSeen = false;
            landedSeen = false;
            moveActionEnabledAtStart = inputModule.MoveAction?.enabled == true;
            jumpActionEnabledAtStart = inputModule.JumpAction?.enabled == true;
            keyboardDeviceId = keyboard.deviceId;
            retryReleaseQueued = false;
            phase = ProbePhase.WaitingForNeutral;
            Trace.Clear();

            QueueKeyboardState();
            SimulationPlayerInput initialInput =
                ResolveFirstPlayerInput(driver.LastAppliedFrameInput);
            int initialState = character.Frame?.D?.state ?? int.MinValue;
            if (IsNeutralGround(initialInput, initialState, character.Runtime.YInt))
            {
                neutralTick = startTick;
                baselineXInt = character.Runtime.XInt;
                QueueKeyboardState(Key.D);
                rightPressAttempts = 1;
                phase = ProbePhase.RightQueued;
            }
            running = true;
            EditorApplication.update += Observe;
            Debug.Log(
                $"[BattlePhysicalMovementPlayModeProbe] waiting for a neutral ground frame at tick {startTick}; " +
                $"jumpDistance={characterData.jump_distance:R}, jumpHeight={characterData.jump_height:R}.");
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
            SimulationPlayerInput playerInput = ResolveFirstPlayerInput(driver.LastAppliedFrameInput);
            int frame = character.Frame?.N ?? -1;
            int state = character.Frame?.D?.state ?? int.MinValue;
            int xInt = character.Runtime.XInt;
            int yInt = character.Runtime.YInt;
            Trace.Add(new TraceRow
            {
                tick = tick,
                frameInputTick = driver.LastAppliedFrameInput?.TickIndex ?? -1,
                playerSlot = playerInput.PlayerSlot,
                heldButtons = (int)playerInput.Buttons,
                pressedButtons = (int)playerInput.PressedButtons,
                releasedButtons = (int)playerInput.ReleasedButtons,
                frame = frame,
                state = state,
                x = character.Runtime.X,
                y = character.Runtime.Y,
                z = character.Runtime.Z,
                xInt = xInt,
                yInt = yInt,
                zInt = character.Runtime.ZInt,
                vx = character.Runtime.Vx,
                vy = character.Runtime.Vy,
                vz = character.Runtime.Vz,
                dir = character.Runtime.Dir,
                keyRight = character.Runtime.KeyRight,
                keyLeft = character.Runtime.KeyLeft,
                keyJump = character.Runtime.KeyJump,
                keyDefend = character.Runtime.KeyDefend,
                prevRight = character.Runtime.PrevRight,
                prevJump = character.Runtime.PrevJump,
                prevDefend = character.Runtime.PrevDefend,
                cdRight = character.Runtime.CdRight,
                cdJump = character.Runtime.CdJump,
                objectCount = driver.World.ObjectCount,
                moveInputX = inputModule.CurrentMoveInput.x,
                moveInputY = inputModule.CurrentMoveInput.y,
                phase = phase.ToString(),
            });

            switch (phase)
            {
                case ProbePhase.WaitingForNeutral:
                    if (IsNeutralGround(playerInput, state, yInt))
                    {
                        neutralTick = tick;
                        baselineXInt = xInt;
                        QueueKeyboardState(Key.D);
                        rightPressAttempts = 1;
                        lastInputPulseTick = tick;
                        retryReleaseQueued = false;
                        phase = ProbePhase.RightQueued;
                    }
                    else if (tick > startTick + NeutralTimeoutTicks)
                    {
                        Finish(false, "Timed out waiting for a neutral ground frame.");
                    }
                    break;

                case ProbePhase.RightQueued:
                    if (HasButton(playerInput.PressedButtons, SimulationInputButtons.Right) &&
                        HasButton(playerInput.Buttons, SimulationInputButtons.Right) &&
                        character.Runtime.KeyRight == 1 && character.Runtime.CdRight > 0)
                    {
                        rightInputSeen = true;
                        rightEdgeTick = tick;
                        retryReleaseQueued = false;
                        phase = ProbePhase.RightHeld;
                    }
                    else if (!PulseRightState(tick))
                    {
                        Finish(
                            false,
                            $"Physical D did not reach canonical FrameInputSet within " +
                            $"{MaximumPressAttemptsPerPhase} press attempts.");
                    }
                    break;

                case ProbePhase.RightHeld:
                    if (tick >= rightEdgeTick + RightHoldTicksBeforeJump)
                    {
                        jumpStartXInt = xInt;
                        QueueKeyboardState(Key.D, Key.K);
                        jumpPressAttempts = 1;
                        lastInputPulseTick = tick;
                        retryReleaseQueued = false;
                        phase = ProbePhase.JumpQueued;
                    }
                    break;

                case ProbePhase.JumpQueued:
                    // Physical K is the Unity JumpAction, but the preserved NTSD crossed
                    // canonical contract carries it in the Defend bit before KeyDefend/CdJump.
                    if (HasButton(playerInput.PressedButtons, SimulationInputButtons.Defend) &&
                        HasButton(playerInput.Buttons, SimulationInputButtons.Right) &&
                        character.Runtime.KeyDefend == 1 && character.Runtime.CdJump > 0)
                    {
                        jumpInputSeen = true;
                        jumpEdgeTick = tick;
                        QueueKeyboardState(Key.D);
                        retryReleaseQueued = false;
                        phase = ProbePhase.WaitingForAirborne;
                    }
                    else if (!PulseJumpState(tick))
                    {
                        Finish(
                            false,
                            $"Physical K did not reach canonical FrameInputSet within " +
                            $"{MaximumPressAttemptsPerPhase} press attempts.");
                    }
                    break;

                case ProbePhase.WaitingForAirborne:
                    if (yInt < 0)
                    {
                        airborneSeen = true;
                        airborneTick = tick;
                        firstAirXInt = xInt;
                        firstAirVx = character.Runtime.Vx;
                        firstAirVy = character.Runtime.Vy;
                        horizontalAirMotionSeen =
                            character.Runtime.Vx > 0.0001 &&
                            string.Equals(character.Runtime.Dir, "right", StringComparison.OrdinalIgnoreCase);
                        phase = ProbePhase.Airborne;
                    }
                    break;

                case ProbePhase.Airborne:
                    if (xInt > firstAirXInt)
                        horizontalAirMotionSeen = true;
                    if (tick >= airborneTick + AirTicksBeforeRelease)
                    {
                        releaseTick = tick;
                        QueueKeyboardState();
                        phase = ProbePhase.WaitingForLanding;
                    }
                    break;

                case ProbePhase.WaitingForLanding:
                    if (airborneSeen && yInt == 0 && Math.Abs(character.Runtime.Y) < 0.0001)
                    {
                        landedSeen = true;
                        landingTick = tick;
                        bool passed = rightInputSeen && jumpInputSeen && airborneSeen &&
                                      horizontalAirMotionSeen && xInt > jumpStartXInt &&
                                      driver.World.ObjectCount == baselineObjectCount;
                        Finish(
                            passed,
                            passed
                                ? "Physical D/K reached airborne horizontal movement and returned to ground."
                                : "The character landed, but one or more input/motion/cleanup checkpoints failed.");
                    }
                    break;
            }

            if (running && tick > startTick + ProbeTimeoutTicks)
                Finish(false, $"Timed out in phase {phase}.");
        }

        private static bool IsNeutralGround(
            SimulationPlayerInput playerInput,
            int state,
            int yInt)
        {
            return yInt == 0 &&
                   (state == LF2States.Standing || state == LF2States.Walking) &&
                   playerInput.Buttons == SimulationInputButtons.None &&
                   character.Runtime.KeyRight == 0 &&
                   character.Runtime.KeyLeft == 0 &&
                   character.Runtime.KeyJump == 0 &&
                   character.Runtime.KeyDefend == 0;
        }

        private static bool HasButton(
            SimulationInputButtons value,
            SimulationInputButtons button)
        {
            return (value & button) != 0;
        }

        private static bool PulseRightState(int tick)
        {
            if (tick <= lastInputPulseTick)
                return true;

            if (!retryReleaseQueued)
            {
                if (rightPressAttempts >= MaximumPressAttemptsPerPhase)
                    return false;
                QueueKeyboardState();
                retryReleaseQueued = true;
            }
            else
            {
                QueueKeyboardState(Key.D);
                rightPressAttempts++;
                retryReleaseQueued = false;
            }

            lastInputPulseTick = tick;
            return true;
        }

        private static bool PulseJumpState(int tick)
        {
            if (tick <= lastInputPulseTick)
                return true;

            if (!retryReleaseQueued)
            {
                if (jumpPressAttempts >= MaximumPressAttemptsPerPhase)
                    return false;
                QueueKeyboardState(Key.D);
                retryReleaseQueued = true;
            }
            else
            {
                QueueKeyboardState(Key.D, Key.K);
                jumpPressAttempts++;
                retryReleaseQueued = false;
            }

            lastInputPulseTick = tick;
            return true;
        }

        private static SimulationPlayerInput ResolveFirstPlayerInput(FrameInputSet frameInput)
        {
            if (frameInput?.Players == null || frameInput.Players.Count == 0)
                return default;

            for (int i = 0; i < frameInput.Players.Count; i++)
            {
                SimulationPlayerInput input = frameInput.Players[i];
                if (input.PlayerSlot == 0)
                    return input;
            }

            return frameInput.Players[0];
        }

        private static void QueueKeyboardState(params Key[] pressedKeys)
        {
            if (keyboard != null)
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(pressedKeys));
        }

        private static void Finish(bool passed, string message)
        {
            ProbeResult result = BuildResult(passed ? "PASS" : "FAIL", message);
            WriteResult(result);
            if (passed)
                Debug.Log($"[BattlePhysicalMovementPlayModeProbe] PASS: {message}");
            else
                Debug.LogError($"[BattlePhysicalMovementPlayModeProbe] FAIL: {message}");
            StopObservation();
        }

        private static void WriteFailure(string message)
        {
            ProbeResult result = BuildResult("FAIL", message);
            WriteResult(result);
            Debug.LogError($"[BattlePhysicalMovementPlayModeProbe] FAIL: {message}");
            StopObservation();
        }

        private static ProbeResult BuildResult(string status, string message)
        {
            return new ProbeResult
            {
                status = status,
                message = message,
                startTick = startTick,
                neutralTick = neutralTick,
                rightEdgeTick = rightEdgeTick,
                jumpEdgeTick = jumpEdgeTick,
                airborneTick = airborneTick,
                releaseTick = releaseTick,
                landingTick = landingTick,
                expectedJumpDistance = characterData?.jump_distance ?? 0f,
                expectedJumpHeight = characterData?.jump_height ?? 0f,
                baselineXInt = baselineXInt,
                jumpStartXInt = jumpStartXInt,
                firstAirXInt = firstAirXInt,
                finalXInt = character?.Runtime?.XInt ?? baselineXInt,
                firstAirVx = firstAirVx,
                firstAirVy = firstAirVy,
                rightInputSeen = rightInputSeen,
                jumpInputSeen = jumpInputSeen,
                airborneSeen = airborneSeen,
                horizontalAirMotionSeen = horizontalAirMotionSeen,
                landedSeen = landedSeen,
                moveActionEnabledAtStart = moveActionEnabledAtStart,
                jumpActionEnabledAtStart = jumpActionEnabledAtStart,
                keyboardDeviceId = keyboardDeviceId,
                rightPressAttempts = rightPressAttempts,
                jumpPressAttempts = jumpPressAttempts,
                baselineObjectCount = baselineObjectCount,
                finalObjectCount = driver?.World?.ObjectCount ?? -1,
                trace = Trace.ToArray(),
            };
        }

        private static void WriteResult(ProbeResult result)
        {
            string path = Path.GetFullPath(
                Path.Combine(Application.dataPath, "..", ResultPath));
            File.WriteAllText(path, JsonUtility.ToJson(result, true));
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            if (keyboard != null)
                QueueKeyboardState();
            keyboard = null;
            character = null;
            inputModule = null;
            characterData = null;
            driver = null;
            running = false;
        }

        [Serializable]
        private sealed class ProbeResult
        {
            public string status;
            public string message;
            public int startTick;
            public int neutralTick;
            public int rightEdgeTick;
            public int jumpEdgeTick;
            public int airborneTick;
            public int releaseTick;
            public int landingTick;
            public float expectedJumpDistance;
            public float expectedJumpHeight;
            public int baselineXInt;
            public int jumpStartXInt;
            public int firstAirXInt;
            public int finalXInt;
            public double firstAirVx;
            public double firstAirVy;
            public bool rightInputSeen;
            public bool jumpInputSeen;
            public bool airborneSeen;
            public bool horizontalAirMotionSeen;
            public bool landedSeen;
            public bool moveActionEnabledAtStart;
            public bool jumpActionEnabledAtStart;
            public int keyboardDeviceId;
            public int rightPressAttempts;
            public int jumpPressAttempts;
            public int baselineObjectCount;
            public int finalObjectCount;
            public TraceRow[] trace;
        }

        [Serializable]
        private sealed class TraceRow
        {
            public int tick;
            public int frameInputTick;
            public int playerSlot;
            public int heldButtons;
            public int pressedButtons;
            public int releasedButtons;
            public int frame;
            public int state;
            public double x;
            public double y;
            public double z;
            public int xInt;
            public int yInt;
            public int zInt;
            public double vx;
            public double vy;
            public double vz;
            public string dir;
            public int keyRight;
            public int keyLeft;
            public int keyJump;
            public int keyDefend;
            public int prevRight;
            public int prevJump;
            public int prevDefend;
            public int cdRight;
            public int cdJump;
            public int objectCount;
            public float moveInputX;
            public float moveInputY;
            public string phase;
        }

        private enum ProbePhase
        {
            WaitingForNeutral,
            RightQueued,
            RightHeld,
            JumpQueued,
            WaitingForAirborne,
            Airborne,
            WaitingForLanding,
        }
    }
}
#endif
