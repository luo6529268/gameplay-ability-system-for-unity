#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using NTSD.Animation.LF2Objects;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace NTSD.Test.Editor
{
    public static class NTSD28UserRasenganPhysicalPlayProbeEditor
    {
        private const string MenuPath =
            "NTSD/Battle Diagnostics/User/Run Naruto Rasengan Physical J Window Probe";
        private const string ResultPath =
            "Temp/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/rasengan-physical-play.json";
        private const string LateResultPath =
            "Temp/diagnostics/NTSD28-USER-NARUTO-RUN-ATTACK-WINDOW-001/rasengan-physical-late-play.json";
        private const string LateMenuPath =
            "NTSD/Battle Diagnostics/User/Run Naruto Rasengan Late Physical J Probe";

        private static readonly List<TickRow> Rows = new List<TickRow>(48);
        private static BattleTestBootstrap bootstrap;
        private static LF2Character character;
        private static CharacterInputModule inputModule;
        private static SimulationTickDriver driver;
        private static Keyboard keyboard;
        private static int startTick;
        private static int lastTick;
        private static int first253Tick;
        private static int queuedJAfterTick;
        private static int canonicalJTick;
        private static int action301Tick;
        private static double startSeconds;
        private static double first253Seconds;
        private static double queuedJSeconds;
        private static bool running;
        private static bool pressOnSecond253;

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            RunProbe(false);
        }

        [MenuItem(LateMenuPath)]
        public static void RunLateFromMenu()
        {
            RunProbe(true);
        }

        private static void RunProbe(bool latePress)
        {
            StopObservation();
            Rows.Clear();
            pressOnSecond253 = latePress;
            if (!EditorApplication.isPlaying)
            {
                WriteResult(false, "Play Mode is not active.");
                return;
            }

            bootstrap = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
            FieldInfo playerField = typeof(BattleTestBootstrap).GetField(
                "firstPlayerLf2", BindingFlags.Instance | BindingFlags.NonPublic);
            character = bootstrap == null
                ? null
                : playerField?.GetValue(bootstrap) as LF2Character;
            inputModule = character?.Controller as CharacterInputModule;
            driver = SimulationTickDriver.Instance;
            keyboard = Keyboard.current;
            if (character?.ObjectId != 2 || driver?.World == null ||
                inputModule?.AttackAction?.enabled != true || keyboard == null ||
                !character.FrameCache.HasFrame(241) ||
                !character.FrameCache.HasFrame(253))
            {
                WriteResult(false,
                    "Naruto OID 2, live World, physical AttackAction, or authored frames are unavailable.");
                return;
            }

            try
            {
                QueueKeyboardState();
                character.Runtime.MP = 200;
                character.Runtime.PP = 200;
                character.ImmediateFrame(241);
                character.Frame.PN = 241;
                character.Frame.Prev = 241;
                character.Frame.Prev2 = 241;
                character.Frame.Prev2D = character.Frame.D;
                character.Runtime.PrevFrame2 = 241;
                character.Trans.SyncWaitCounterFrame(241);
                startTick = driver.CurrentTickIndex;
                lastTick = startTick;
                first253Tick = -1;
                queuedJAfterTick = -1;
                canonicalJTick = -1;
                action301Tick = -1;
                startSeconds = EditorApplication.timeSinceStartup;
                first253Seconds = -1;
                queuedJSeconds = -1;
                running = true;
                EditorApplication.update += Observe;
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
            }
            catch (Exception exception)
            {
                Finish(false, "Probe setup failed: " + exception);
            }
        }

        private static void Observe()
        {
            if (!running)
                return;
            try
            {
                if (!EditorApplication.isPlaying || driver?.World == null ||
                    character?.Runtime == null)
                {
                    Finish(false, "Play Mode or Naruto World ended.");
                    return;
                }

                int tick = driver.CurrentTickIndex;
                if (tick <= lastTick)
                    return;
                int gap = tick - lastTick;
                lastTick = tick;
                SimulationPlayerInput player = GetPlayerInput(
                    driver.LastAppliedFrameInput);
                int action = character.Frame?.N ?? -1;
                int inputHeld = (int)player.Buttons;
                int inputPressed = (int)player.PressedButtons;
                bool canonicalJ =
                    (player.Buttons & SimulationInputButtons.Jump) != 0;
                if (canonicalJ && canonicalJTick < 0)
                    canonicalJTick = tick;
                if (action == 301 && action301Tick < 0)
                    action301Tick = tick;

                Rows.Add(new TickRow
                {
                    tick = tick,
                    elapsedSeconds = EditorApplication.timeSinceStartup - startSeconds,
                    gap = gap,
                    action = action,
                    counter = character.Runtime.AttackingCounter,
                    mp = character.Runtime.MP,
                    inputHeld = inputHeld,
                    inputPressed = inputPressed,
                    inputReleased = (int)player.ReleasedButtons,
                    keyJump = character.Runtime.KeyJump,
                    cdAttack = character.Runtime.CdAttack,
                    physicalAttackActionEnabled = inputModule.AttackAction?.enabled == true,
                });

                if (gap > 1 && (first253Tick >= 0 || action >= 251))
                {
                    Finish(false, "Editor observation skipped a logic tick near the attack window.");
                    return;
                }
                if (first253Tick < 0 && action == 253)
                {
                    first253Tick = tick;
                    first253Seconds = EditorApplication.timeSinceStartup;
                    if (!pressOnSecond253)
                        QueuePhysicalAttackAfterTick(tick);
                    return;
                }
                if (pressOnSecond253 && first253Tick >= 0 &&
                    queuedJAfterTick < 0 && action == 253 &&
                    tick > first253Tick)
                {
                    QueuePhysicalAttackAfterTick(tick);
                    return;
                }
                if (action301Tick >= 0)
                {
                    Finish(canonicalJTick >= 0,
                        canonicalJTick >= 0
                            ? "Physical J reached FrameInputSet and action 301."
                            : "Action 301 occurred without a captured physical J button.");
                    return;
                }
                if (queuedJAfterTick >= 0 && tick >= queuedJAfterTick + 2)
                    QueueKeyboardState();
                if (first253Tick >= 0 && action != 253 && action != 254 &&
                    tick >= first253Tick + 3)
                {
                    Finish(false, "The action-253 window ended without action 301.");
                    return;
                }
                if (tick >= startTick + 45)
                    Finish(false, "Timed out before the physical attack transition.");
            }
            catch (Exception exception)
            {
                Finish(false, "Probe observation failed: " + exception);
            }
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingPlayMode && running)
                Finish(false, "Play Mode exited before the attack window completed.");
        }

        private static void QueuePhysicalAttackAfterTick(int tick)
        {
            QueueKeyboardState(Key.J);
            queuedJAfterTick = tick;
            queuedJSeconds = EditorApplication.timeSinceStartup;
        }

        private static SimulationPlayerInput GetPlayerInput(FrameInputSet frame)
        {
            if (frame?.Players == null)
                return default;
            for (int index = 0; index < frame.Players.Count; index++)
            {
                SimulationPlayerInput value = frame.Players[index];
                if (value.PlayerSlot == 0)
                    return value;
            }
            return default;
        }

        private static void QueueKeyboardState(params Key[] keys)
        {
            if (keyboard == null)
                return;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }

        private static void Finish(bool passed, string message)
        {
            StopObservation();
            try
            {
                QueueKeyboardState();
            }
            catch (Exception exception)
            {
                passed = false;
                message += " Key release failed: " + exception.Message;
            }
            WriteResult(passed, message);
            if (passed)
                Debug.Log("[RasenganPhysicalPlayProbe] PASS: " + message);
            else
                Debug.LogError("[RasenganPhysicalPlayProbe] FAIL: " + message);
        }

        private static void StopObservation()
        {
            running = false;
            EditorApplication.update -= Observe;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        }

        private static void WriteResult(bool passed, string message)
        {
            string path = Path.GetFullPath(Path.Combine(
                Environment.CurrentDirectory,
                pressOnSecond253 ? LateResultPath : ResultPath));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonConvert.SerializeObject(new
            {
                status = passed ? "PASS" : "FAIL",
                message,
                pressOnSecond253,
                startTick,
                first253Tick,
                queuedJAfterTick,
                canonicalJTick,
                action301Tick,
                startSeconds,
                first253Seconds,
                queuedJSeconds,
                rows = Rows,
            }, Formatting.Indented));
        }

        private sealed class TickRow
        {
            public int tick;
            public double elapsedSeconds;
            public int gap;
            public int action;
            public int counter;
            public int mp;
            public int inputHeld;
            public int inputPressed;
            public int inputReleased;
            public byte keyJump;
            public byte cdAttack;
            public bool physicalAttackActionEnabled;
        }
    }
}
#endif
