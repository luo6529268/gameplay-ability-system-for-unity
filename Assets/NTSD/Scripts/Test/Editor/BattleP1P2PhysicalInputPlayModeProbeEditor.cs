#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
    /// Editor-only physical P1/P2 witness. It only queues keyboard device states and observes
    /// the production action callback -> FrameInputSet -> roster -> runtime path.
    /// Alignment contract: R8-P2INPUT-001.
    /// </summary>
    public static class BattleP1P2PhysicalInputPlayModeProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/R8/Run P1 P2 Physical Input Play Probe";
        private const string TwoHumanFixtureMenuPath =
            "NTSD/Battle Diagnostics/R8/Run P1 P2 Physical Input Play Probe (Two-Human Fixture)";
        private const string ResultPath =
            "Temp/NTSD_R8_WP01G_R06_P1P2_PhysicalInput.result.json";
        private const string AutoRunSessionKey =
            "NTSD.R8.WP01G.R06.P1P2PhysicalInput.AutoRun";
        private const int NeutralTimeoutTicks = 180;
        private const int CaseTimeoutTicks = 48;
        private const int MaximumPressAttempts = 8;
        private const double FixtureStartupTimeoutSeconds = 90d;

        private static readonly InputCase[] Cases =
        {
            new InputCase("P1_D_Right", Key.D, 0, SimulationInputButtons.Right),
            new InputCase("P1_J_Jump", Key.J, 0, SimulationInputButtons.Jump),
            new InputCase("P1_K_Defend", Key.K, 0, SimulationInputButtons.Defend),
            new InputCase("P1_L_Attack", Key.L, 0, SimulationInputButtons.Attack),
            new InputCase("P2_Up", Key.UpArrow, 1, SimulationInputButtons.Up),
            new InputCase("P2_Down", Key.DownArrow, 1, SimulationInputButtons.Down),
            new InputCase("P2_Left", Key.LeftArrow, 1, SimulationInputButtons.Left),
            new InputCase("P2_Right", Key.RightArrow, 1, SimulationInputButtons.Right),
            new InputCase("P2_Numpad1_Jump", Key.Numpad1, 1, SimulationInputButtons.Jump),
            new InputCase("P2_Numpad2_Defend", Key.Numpad2, 1, SimulationInputButtons.Defend),
            new InputCase("P2_Numpad3_Attack", Key.Numpad3, 1, SimulationInputButtons.Attack),
        };

        private static readonly List<CaseResult> CaseResults = new List<CaseResult>(Cases.Length);

        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character player1;
        private static LF2Character player2;
        private static CharacterInputModule input1;
        private static CharacterInputModule input2;
        private static Keyboard keyboard;
        private static ProbePhase phase;
        private static int startTick;
        private static int lastObservedTick;
        private static int phaseStartTick;
        private static int lastPulseTick;
        private static int caseIndex;
        private static int pressAttempts;
        private static int player1RuntimeSlot;
        private static int player2RuntimeSlot;
        private static int player1StableId;
        private static int player2StableId;
        private static int keyboardDeviceId;
        private static bool retryReleaseQueued;
        private static bool running;
        private static double fixtureStartupDeadline;

        [InitializeOnLoadMethod]
        private static void RegisterFixtureLifecycle()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(TwoHumanFixtureMenuPath)]
        public static void RunWithTwoHumanFixture()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                WriteFailure("The two-human fixture must be started from Edit Mode.");
                return;
            }

            SessionState.SetBool(AutoRunSessionKey, true);
            EditorApplication.EnterPlaymode();
        }

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            StopObservation();
            if (!EditorApplication.isPlaying)
            {
                WriteFailure("Play Mode is not active.");
                return;
            }

            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            keyboard = Keyboard.current;
            if (driver == null || world == null || keyboard == null)
            {
                WriteFailure("SimulationTickDriver, world, or Keyboard.current is unavailable.");
                return;
            }

            if (!world.TryResolveRosterInputEntity(0, out LF2Entity player1Entity) ||
                !world.TryResolveRosterInputEntity(1, out LF2Entity player2Entity) ||
                player1Entity is not LF2Character player1Character ||
                player2Entity is not LF2Character player2Character ||
                ReferenceEquals(player1Character, player2Character))
            {
                WriteFailure("The production roster does not expose two distinct active human entities in player slots 0 and 1.");
                return;
            }

            player1 = player1Character;
            player2 = player2Character;

            input1 = player1.Controller as CharacterInputModule;
            input2 = player2.Controller as CharacterInputModule;
            if (!HasCompleteInputMap(input1, "Player_1") || !HasCompleteInputMap(input2, "Player_2"))
            {
                WriteFailure("P1/P2 CharacterInputModule action maps are not fully bound and enabled.");
                return;
            }

            player1RuntimeSlot = player1.Runtime.SlotIndex;
            player2RuntimeSlot = player2.Runtime.SlotIndex;
            player1StableId = player1.Runtime.StableId;
            player2StableId = player2.Runtime.StableId;
            keyboardDeviceId = keyboard.deviceId;
            startTick = driver.CurrentTickIndex;
            lastObservedTick = startTick;
            phaseStartTick = startTick;
            lastPulseTick = startTick;
            caseIndex = 0;
            pressAttempts = 0;
            retryReleaseQueued = false;
            phase = ProbePhase.WaitingForNeutral;
            CaseResults.Clear();
            QueueKeyboardState();

            running = true;
            EditorApplication.update += Observe;
            Debug.Log($"[BattleP1P2PhysicalInputPlayModeProbe] started at tick {startTick}.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!SessionState.GetBool(AutoRunSessionKey, false))
                return;

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (!TryConfigureTwoHumanPlayClone(out string error))
                {
                    SessionState.EraseBool(AutoRunSessionKey);
                    WriteFailure(error);
                    return;
                }

                fixtureStartupDeadline = EditorApplication.timeSinceStartup + FixtureStartupTimeoutSeconds;
                EditorApplication.update -= WaitForTwoHumanRoster;
                EditorApplication.update += WaitForTwoHumanRoster;
                return;
            }

            if (state == PlayModeStateChange.ExitingPlayMode ||
                state == PlayModeStateChange.EnteredEditMode)
            {
                StopFixtureWait();
                SessionState.EraseBool(AutoRunSessionKey);
            }
        }

        private static bool TryConfigureTwoHumanPlayClone(out string error)
        {
            NTSD.Test.BattleTestBootstrap[] bootstraps =
                Resources.FindObjectsOfTypeAll<NTSD.Test.BattleTestBootstrap>();
            NTSD.Test.BattleTestBootstrap bootstrap = null;
            for (int index = 0; index < bootstraps.Length; index++)
            {
                NTSD.Test.BattleTestBootstrap candidate = bootstraps[index];
                if (candidate != null && candidate.isActiveAndEnabled &&
                    candidate.gameObject.scene.IsValid() && candidate.gameObject.scene.isLoaded)
                {
                    bootstrap = candidate;
                    break;
                }
            }

            if (bootstrap == null)
            {
                error = "BattleTestBootstrap is unavailable in the active Play Mode scene.";
                return false;
            }

            FieldInfo overrideField = typeof(NTSD.Test.BattleTestBootstrap).GetField(
                "overrideCharacterIds",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (overrideField == null)
            {
                error = "BattleTestBootstrap.overrideCharacterIds could not be located.";
                return false;
            }

            int[] configuredIds = overrideField.GetValue(bootstrap) as int[];
            int characterId = configuredIds != null && configuredIds.Length > 0 && configuredIds[0] >= 0
                ? configuredIds[0]
                : 0;
            overrideField.SetValue(bootstrap, new[] { characterId, characterId });
            error = string.Empty;
            Debug.Log(
                $"[BattleP1P2PhysicalInputPlayModeProbe] configured unsaved Play clone for two human entities with character {characterId}.");
            return true;
        }

        private static void WaitForTwoHumanRoster()
        {
            if (!EditorApplication.isPlaying)
            {
                StopFixtureWait();
                SessionState.EraseBool(AutoRunSessionKey);
                WriteFailure("Play Mode ended before the two-human fixture became ready.");
                return;
            }

            SimulationWorld currentWorld = SimulationTickDriver.Instance?.World;
            if (currentWorld != null &&
                currentWorld.TryResolveRosterInputEntity(0, out LF2Entity first) &&
                currentWorld.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                first is LF2Character && second is LF2Character && !ReferenceEquals(first, second))
            {
                StopFixtureWait();
                SessionState.EraseBool(AutoRunSessionKey);
                RunFromMenu();
                return;
            }

            if (EditorApplication.timeSinceStartup <= fixtureStartupDeadline)
                return;

            StopFixtureWait();
            SessionState.EraseBool(AutoRunSessionKey);
            WriteFailure("Timed out waiting for BattleTestBootstrap to publish two active human roster entities.");
        }

        private static void StopFixtureWait()
        {
            EditorApplication.update -= WaitForTwoHumanRoster;
            fixtureStartupDeadline = 0d;
        }

        private static bool HasCompleteInputMap(CharacterInputModule input, string expectedMap)
        {
            return input != null && input.MoveAction?.enabled == true &&
                   input.AttackAction?.enabled == true && input.JumpAction?.enabled == true &&
                   input.DefendAction?.enabled == true &&
                   string.Equals(input.MoveAction.actionMap?.name, expectedMap, StringComparison.Ordinal) &&
                   ReferenceEquals(input.MoveAction.actionMap, input.AttackAction.actionMap) &&
                   ReferenceEquals(input.MoveAction.actionMap, input.JumpAction.actionMap) &&
                   ReferenceEquals(input.MoveAction.actionMap, input.DefendAction.actionMap);
        }

        private static void Observe()
        {
            if (!running)
                return;
            if (!EditorApplication.isPlaying || driver?.World == null || player1?.Runtime == null ||
                player2?.Runtime == null || keyboard == null)
            {
                Finish(false, "Play Mode or the production P1/P2 runtime ended before the probe completed.");
                return;
            }

            int tick = driver.CurrentTickIndex;
            if (tick <= lastObservedTick)
                return;
            lastObservedTick = tick;

            if (!RosterBindingsRemainCurrent())
            {
                Finish(false, "P1/P2 roster runtime-slot or stable-id binding changed during the probe.");
                return;
            }

            bool hasP1 = TryFindPlayerInput(driver.LastAppliedFrameInput, 0, out SimulationPlayerInput p1Input);
            bool hasP2 = TryFindPlayerInput(driver.LastAppliedFrameInput, 1, out SimulationPlayerInput p2Input);
            if (!hasP1 || !hasP2)
            {
                if (tick > startTick + NeutralTimeoutTicks)
                    Finish(false, "The production FrameInputSet did not contain both human player slots.");
                return;
            }

            switch (phase)
            {
                case ProbePhase.WaitingForNeutral:
                    if (p1Input.Buttons == SimulationInputButtons.None &&
                        p2Input.Buttons == SimulationInputButtons.None &&
                        AllRuntimeKeysReleased(player1) && AllRuntimeKeysReleased(player2))
                    {
                        BeginCurrentCase(tick);
                    }
                    else if (tick > startTick + NeutralTimeoutTicks)
                    {
                        Finish(false, "Timed out waiting for neutral P1/P2 input.");
                    }
                    break;

                case ProbePhase.WaitingForPress:
                    ObservePress(tick, p1Input, p2Input);
                    break;

                case ProbePhase.WaitingForRelease:
                    ObserveRelease(tick, p1Input, p2Input);
                    break;
            }
        }

        private static void BeginCurrentCase(int tick)
        {
            if (caseIndex >= Cases.Length)
            {
                Finish(true, string.Empty);
                return;
            }

            pressAttempts = 1;
            retryReleaseQueued = false;
            phaseStartTick = tick;
            lastPulseTick = tick;
            QueueKeyboardState(Cases[caseIndex].PhysicalKey);
            phase = ProbePhase.WaitingForPress;
        }

        private static void ObservePress(
            int tick,
            SimulationPlayerInput p1Input,
            SimulationPlayerInput p2Input)
        {
            InputCase current = Cases[caseIndex];
            SimulationPlayerInput targetInput = current.PlayerSlot == 0 ? p1Input : p2Input;
            SimulationPlayerInput otherInput = current.PlayerSlot == 0 ? p2Input : p1Input;
            LF2Entity target = current.PlayerSlot == 0 ? player1 : player2;
            LF2Entity other = current.PlayerSlot == 0 ? player2 : player1;
            bool pressed = HasButton(targetInput.PressedButtons, current.CanonicalButton) &&
                           HasButton(targetInput.Buttons, current.CanonicalButton) &&
                           !HasButton(otherInput.Buttons, current.CanonicalButton) &&
                           !HasButton(otherInput.PressedButtons, current.CanonicalButton) &&
                           GetRuntimeKey(target, current.CanonicalButton) == 1 &&
                           GetRuntimeKey(other, current.CanonicalButton) == 0;
            if (pressed)
            {
                CaseResults.Add(new CaseResult
                {
                    name = current.Name,
                    playerSlot = current.PlayerSlot,
                    physicalKey = current.PhysicalKey.ToString(),
                    canonicalButton = current.CanonicalButton.ToString(),
                    pressTick = tick,
                    releaseTick = -1,
                    pressAttempts = pressAttempts,
                    pressed = true,
                    released = false,
                    noCross = true,
                });
                QueueKeyboardState();
                lastPulseTick = tick;
                phaseStartTick = tick;
                phase = ProbePhase.WaitingForRelease;
                return;
            }

            if (tick > phaseStartTick + CaseTimeoutTicks)
            {
                Finish(false, $"{current.Name} did not reach its canonical press/runtime key contract.");
                return;
            }

            PulseCurrentCase(tick);
        }

        private static void ObserveRelease(
            int tick,
            SimulationPlayerInput p1Input,
            SimulationPlayerInput p2Input)
        {
            InputCase current = Cases[caseIndex];
            SimulationPlayerInput targetInput = current.PlayerSlot == 0 ? p1Input : p2Input;
            SimulationPlayerInput otherInput = current.PlayerSlot == 0 ? p2Input : p1Input;
            LF2Entity target = current.PlayerSlot == 0 ? player1 : player2;
            LF2Entity other = current.PlayerSlot == 0 ? player2 : player1;
            bool released = HasButton(targetInput.ReleasedButtons, current.CanonicalButton) &&
                            !HasButton(targetInput.Buttons, current.CanonicalButton) &&
                            !HasButton(otherInput.Buttons, current.CanonicalButton) &&
                            !HasButton(otherInput.ReleasedButtons, current.CanonicalButton) &&
                            GetRuntimeKey(target, current.CanonicalButton) == 0 &&
                            GetRuntimeKey(other, current.CanonicalButton) == 0;
            if (released)
            {
                CaseResult result = CaseResults[CaseResults.Count - 1];
                result.releaseTick = tick;
                result.released = true;
                CaseResults[CaseResults.Count - 1] = result;
                caseIndex++;
                phase = ProbePhase.WaitingForNeutral;
                phaseStartTick = tick;
                return;
            }

            if (tick > phaseStartTick + CaseTimeoutTicks)
            {
                Finish(false, $"{current.Name} did not reach its canonical release/runtime key contract.");
                return;
            }

            QueueKeyboardState();
        }

        private static void PulseCurrentCase(int tick)
        {
            if (tick <= lastPulseTick + 1)
                return;

            if (!retryReleaseQueued)
            {
                QueueKeyboardState();
                retryReleaseQueued = true;
                lastPulseTick = tick;
                return;
            }

            if (pressAttempts >= MaximumPressAttempts)
            {
                Finish(false, $"{Cases[caseIndex].Name} exceeded {MaximumPressAttempts} physical press attempts.");
                return;
            }

            QueueKeyboardState(Cases[caseIndex].PhysicalKey);
            pressAttempts++;
            retryReleaseQueued = false;
            lastPulseTick = tick;
        }

        private static bool RosterBindingsRemainCurrent()
        {
            return world.TryResolveRosterInputEntity(0, out LF2Entity currentP1) &&
                   world.TryResolveRosterInputEntity(1, out LF2Entity currentP2) &&
                   ReferenceEquals(currentP1, player1) && ReferenceEquals(currentP2, player2) &&
                   player1.Runtime.SlotIndex == player1RuntimeSlot &&
                   player2.Runtime.SlotIndex == player2RuntimeSlot &&
                   player1.Runtime.StableId == player1StableId &&
                   player2.Runtime.StableId == player2StableId;
        }

        private static bool TryFindPlayerInput(
            FrameInputSet frame,
            int playerSlot,
            out SimulationPlayerInput playerInput)
        {
            if (frame?.Players != null)
            {
                for (int index = 0; index < frame.Players.Count; index++)
                {
                    if (frame.Players[index].PlayerSlot == playerSlot)
                    {
                        playerInput = frame.Players[index];
                        return true;
                    }
                }
            }

            playerInput = default;
            return false;
        }

        private static bool AllRuntimeKeysReleased(LF2Entity entity)
        {
            return entity.Runtime.KeyRight == 0 && entity.Runtime.KeyLeft == 0 &&
                   entity.Runtime.KeyUp == 0 && entity.Runtime.KeyDown == 0 &&
                   entity.Runtime.KeyAttack == 0 && entity.Runtime.KeyJump == 0 &&
                   entity.Runtime.KeyDefend == 0;
        }

        private static int GetRuntimeKey(LF2Entity entity, SimulationInputButtons button)
        {
            if (button == SimulationInputButtons.Right) return entity.Runtime.KeyRight;
            if (button == SimulationInputButtons.Left) return entity.Runtime.KeyLeft;
            if (button == SimulationInputButtons.Up) return entity.Runtime.KeyUp;
            if (button == SimulationInputButtons.Down) return entity.Runtime.KeyDown;
            if (button == SimulationInputButtons.Attack) return entity.Runtime.KeyAttack;
            if (button == SimulationInputButtons.Jump) return entity.Runtime.KeyJump;
            if (button == SimulationInputButtons.Defend) return entity.Runtime.KeyDefend;
            return -1;
        }

        private static bool HasButton(SimulationInputButtons buttons, SimulationInputButtons expected)
        {
            return (buttons & expected) != 0;
        }

        private static void QueueKeyboardState(params Key[] pressedKeys)
        {
            if (keyboard == null)
                return;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(pressedKeys));
        }

        private static void Finish(bool success, string error)
        {
            QueueKeyboardState();
            int endTick = driver?.CurrentTickIndex ?? -1;
            bool bindingsRestored = player1?.Runtime != null && player2?.Runtime != null &&
                                    RosterBindingsRemainCurrent();
            var report = new ProbeReport
            {
                success = success && bindingsRestored && CaseResults.Count == Cases.Length,
                error = error,
                startTick = startTick,
                endTick = endTick,
                keyboardDeviceId = keyboardDeviceId,
                player1RuntimeSlot = player1RuntimeSlot,
                player2RuntimeSlot = player2RuntimeSlot,
                player1StableId = player1StableId,
                player2StableId = player2StableId,
                caseCount = CaseResults.Count,
                expectedCaseCount = Cases.Length,
                rosterBindingsRestored = bindingsRestored,
                cases = CaseResults.ToArray(),
            };
            if (!bindingsRestored && string.IsNullOrEmpty(report.error))
                report.error = "P1/P2 roster bindings were not preserved.";
            if (CaseResults.Count != Cases.Length && string.IsNullOrEmpty(report.error))
                report.error = $"Only {CaseResults.Count}/{Cases.Length} input cases completed.";

            Directory.CreateDirectory("Temp");
            File.WriteAllText(ResultPath, JsonUtility.ToJson(report, true));
            StopObservation();
            if (report.success)
                Debug.Log($"[BattleP1P2PhysicalInputPlayModeProbe] PASS: {ResultPath}");
            else
                Debug.LogError($"[BattleP1P2PhysicalInputPlayModeProbe] FAIL: {report.error}; report={ResultPath}");
        }

        private static void WriteFailure(string error)
        {
            var report = new ProbeReport
            {
                success = false,
                error = error,
                startTick = -1,
                endTick = -1,
                caseCount = 0,
                expectedCaseCount = Cases.Length,
                rosterBindingsRestored = false,
                cases = Array.Empty<CaseResult>(),
            };
            Directory.CreateDirectory("Temp");
            File.WriteAllText(ResultPath, JsonUtility.ToJson(report, true));
            Debug.LogError($"[BattleP1P2PhysicalInputPlayModeProbe] FAIL: {error}; report={ResultPath}");
        }

        private static void StopObservation()
        {
            EditorApplication.update -= Observe;
            running = false;
            driver = null;
            world = null;
            player1 = null;
            player2 = null;
            input1 = null;
            input2 = null;
            keyboard = null;
            phase = ProbePhase.WaitingForNeutral;
        }

        private readonly struct InputCase
        {
            public InputCase(
                string name,
                Key physicalKey,
                int playerSlot,
                SimulationInputButtons canonicalButton)
            {
                Name = name;
                PhysicalKey = physicalKey;
                PlayerSlot = playerSlot;
                CanonicalButton = canonicalButton;
            }

            public string Name { get; }
            public Key PhysicalKey { get; }
            public int PlayerSlot { get; }
            public SimulationInputButtons CanonicalButton { get; }
        }

        private enum ProbePhase
        {
            WaitingForNeutral,
            WaitingForPress,
            WaitingForRelease,
        }

        [Serializable]
        private struct CaseResult
        {
            public string name;
            public int playerSlot;
            public string physicalKey;
            public string canonicalButton;
            public int pressTick;
            public int releaseTick;
            public int pressAttempts;
            public bool pressed;
            public bool released;
            public bool noCross;
        }

        [Serializable]
        private sealed class ProbeReport
        {
            public bool success;
            public string error;
            public int startTick;
            public int endTick;
            public int keyboardDeviceId;
            public int player1RuntimeSlot;
            public int player2RuntimeSlot;
            public int player1StableId;
            public int player2StableId;
            public int caseCount;
            public int expectedCaseCount;
            public bool rosterBindingsRestored;
            public CaseResult[] cases;
        }
    }
}
#endif
