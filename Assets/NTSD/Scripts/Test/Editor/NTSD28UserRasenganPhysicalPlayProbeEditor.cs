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
            NaturalProbe.Cancel();
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

        [MenuItem("NTSD/Battle Diagnostics/User/Run Naruto Natural Combo First 253 J Probe")]
        public static void RunNaturalFirst253FromMenu()
        {
            NaturalProbe.Start(0);
        }

        [MenuItem("NTSD/Battle Diagnostics/User/Run Naruto Natural Combo Second 253 J Probe")]
        public static void RunNaturalSecond253FromMenu()
        {
            NaturalProbe.Start(1);
        }

        [MenuItem("NTSD/Battle Diagnostics/User/Run Naruto Natural Combo After 254 J Probe")]
        public static void RunNaturalAfter254FromMenu()
        {
            NaturalProbe.Start(2);
        }

        // Alignment contract: NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001.
        // Device states are consumed by the normal InputSystem/player/tick path.
        private sealed class NaturalProbe
        {
            private static NaturalProbe active;
            private readonly List<object> trace = new List<object>(120);
            private readonly List<object> events = new List<object>(12);
            private readonly string outputPath;
            private readonly int timing;
            private readonly double started = EditorApplication.timeSinceStartup;
            private LF2Character actor;
            private CharacterInputModule input;
            private SimulationTickDriver tickDriver;
            private Keyboard device;
            private Key forward;
            private int initialTick;
            private int observedTick;
            private int initialMp;
            private int initialRuntimeMp;
            private int initialMpConsumed;
            private int initialHp;
            private int initialHpLost;
            private int initialStableId;
            private int initialInputPhase;
            private NTSDEntityRuntime initialRuntime;
            private BattleTestBootstrap owner;
            private SimulationWorld initialWorld;
            private bool raisedMp;
            private int step;
            private int defendTick = -1;
            private int forwardTick = -1;
            private int skillTick = -1;
            private int firstWindowTick = -1;
            private int secondWindowTick = -1;
            private int afterWindowTick = -1;
            private int queuedAttackTick = -1;
            private int frameInputAttackTick = -1;
            private int frameInputAttackPhase = -1;
            private int nativeAttackSampleTick = -1;
            private int nativeAttackSamplePhase = -1;
            private int nativeAttackEdgeTick = -1;
            private int convertedTick = -1;
            private bool saw240;
            private bool saw241;
            private bool releasedAttack;
            private int lastPulseTick;
            private int pulseAttempts = 1;
            private bool releasePulse;
            private bool finished;
            private bool controlledTickStepping;
            private bool initialPaused;

            private NaturalProbe(int selectedTiming)
            {
                timing = selectedTiming;
                string label = timing == 0 ? "first253" : timing == 1 ? "second253" : "after254";
                outputPath = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory,
                    "Temp/diagnostics/NTSD28-Q07-RASENGAN-NATURAL-COMBO-PLAY-001",
                    "natural-" + label + "-" + DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff") +
                    "-" + Guid.NewGuid().ToString("N") + ".json"));
            }

            public static void Cancel()
            {
                if (active != null)
                    active.Complete(false, "Superseded by another physical input probe.");
            }

            public static void Start(int selectedTiming)
            {
                Cancel();
                if (running)
                    Finish(false, "Superseded by a natural-start physical input probe.");
                active = new NaturalProbe(selectedTiming);
                active.Begin();
            }

            private void Begin()
            {
                try
                {
                    device = Keyboard.current;
                    if (!EditorApplication.isPlaying)
                        throw new InvalidOperationException("Play Mode is not active.");
                    owner = UnityEngine.Object.FindObjectOfType<BattleTestBootstrap>();
                    actor = typeof(BattleTestBootstrap).GetField("firstPlayerLf2",
                        BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner) as LF2Character;
                    input = actor?.Controller as CharacterInputModule;
                    tickDriver = SimulationTickDriver.Instance;
                    if (actor?.ObjectId != 2 || tickDriver?.World == null || device == null ||
                        tickDriver.Settings.driveMode != SimulationDriveMode.LocalFreeRun ||
                        tickDriver.World.OneTuInput || actor.Runtime.HP <= 0 || actor.Runtime.HitStop != 0 ||
                        actor.Health == null || actor.Health.PP < 200 ||
                        input?.AttackAction?.enabled != true ||
                        actor.FrameCache?.GetNativeFrameDataById(actor.Frame.N)?.State != 0 ||
                        actor.Runtime.NativeInputProxy.ComboState[1] != 0)
                        throw new InvalidOperationException(
                            "Requires live standing healthy Naruto OID2, no HitStop, idle combo[1], OneTuInput=false and enabled physical input. " +
                            "Observed: actor=" + (actor != null) +
                            ", oid=" + (actor?.ObjectId.ToString() ?? "null") +
                            ", world=" + (tickDriver?.World != null) +
                            ", device=" + (device != null) +
                            ", driveMode=" + (tickDriver?.Settings.driveMode.ToString() ?? "null") +
                            ", oneTu=" + (tickDriver?.World?.OneTuInput.ToString() ?? "null") +
                            ", hp=" + (actor?.Runtime?.HP.ToString() ?? "null") +
                            ", pp=" + (actor?.Health?.PP.ToString() ?? "null") +
                            ", hitStop=" + (actor?.Runtime?.HitStop.ToString() ?? "null") +
                            ", attackActionEnabled=" + (input?.AttackAction?.enabled.ToString() ?? "null") +
                            ", frame=" + (actor?.Frame.N.ToString() ?? "null") +
                            ", frameState=" + (actor?.FrameCache?.GetNativeFrameDataById(actor.Frame.N)?.State.ToString() ?? "null") +
                            ", combo1=" + (actor?.Runtime?.NativeInputProxy.ComboState[1].ToString() ?? "null"));
                    initialTick = observedTick = tickDriver.CurrentTickIndex;
                    initialMp = actor.Health.PP;
                    initialRuntimeMp = actor.Runtime.MP;
                    initialMpConsumed = actor.Runtime.InputMpConsumedTotal350;
                    initialRuntime = actor.Runtime;
                    initialStableId = actor.Runtime.StableId;
                    initialHp = actor.Runtime.HP;
                    initialHpLost = actor.Runtime.HPLost;
                    initialWorld = tickDriver.World;
                    initialInputPhase = initialWorld.InputPhase;
                    initialPaused = tickDriver.IsPaused;
                    tickDriver.SetPaused(true);
                    controlledTickStepping = true;
                    forward = actor.Runtime.IsFacingLeft ? Key.A : Key.D;
                    EditorApplication.update += ObserveNatural;
                    EditorApplication.playModeStateChanged += OnStateChanged;
                    AssemblyReloadEvents.beforeAssemblyReload += OnReload;
                    Queue("initial-neutral");
                    Queue("defend-after-completed-tick", Key.L);
                    lastPulseTick = initialTick;
                }
                catch (Exception exception)
                {
                    Complete(false, "Natural probe setup failed: " + exception);
                }
            }

            private void Queue(string phase, params Key[] keys)
            {
                events.Add(new
                {
                    tickBeforeQueue = tickDriver?.CurrentTickIndex ?? -1,
                    observedCompletedTick = observedTick,
                    phase,
                    keys = string.Join(",", keys),
                    elapsedSeconds = EditorApplication.timeSinceStartup - started,
                    inputUpdate = "queued-for-normal-InputSystem-update",
                    oneTuInput = tickDriver?.World?.OneTuInput,
                    inputPhase = tickDriver?.World?.InputPhase,
                });
                if (device != null)
                    InputSystem.QueueStateEvent(device, new KeyboardState(keys));
            }

            private void ObserveNatural()
            {
                if (finished)
                    return;
                try
                {
                    if (!EditorApplication.isPlaying || tickDriver?.World == null || actor?.Runtime == null)
                        throw new InvalidOperationException("Play Mode or live Naruto World ended.");
                    if (EditorApplication.timeSinceStartup - started > 20)
                        throw new TimeoutException("No complete natural input sequence within 20 seconds.");
                    int tick = tickDriver.CurrentTickIndex;
                    if (controlledTickStepping && tick == observedTick)
                    {
                        if (!tickDriver.StepOneTick(ignorePaused: true))
                            throw new InvalidOperationException("Controlled single tick could not consume the normal local input provider.");
                        tick = tickDriver.CurrentTickIndex;
                    }
                    if (tick <= observedTick)
                        return;
                    int gap = tick - observedTick;
                    observedTick = tick;
                    int action = actor.Frame.N;
                    int frameState = actor.FrameCache?.GetNativeFrameDataById(action)?.State ?? -1;
                    int combo = actor.Runtime.NativeInputProxy.ComboState[1];
                    SimulationPlayerInput applied = GetPlayerInput(tickDriver.LastAppliedFrameInput);
                    bool attack = (applied.Buttons & SimulationInputButtons.Jump) != 0;
                    if (queuedAttackTick >= 0 && tick > queuedAttackTick)
                    {
                        if (attack && frameInputAttackTick < 0)
                        {
                            frameInputAttackTick = tick;
                            frameInputAttackPhase = tickDriver.World.InputPhase;
                        }
                        if (actor.Runtime.NativeInputProxy.Current[4] != 0)
                        {
                            if (nativeAttackSampleTick < 0)
                            {
                                nativeAttackSampleTick = tick;
                                nativeAttackSamplePhase = tickDriver.World.InputPhase;
                            }
                            if (actor.Runtime.NativeInputProxy.Previous[4] == 0 && nativeAttackEdgeTick < 0)
                                nativeAttackEdgeTick = tick;
                        }
                    }
                    if (action == 301 && convertedTick < 0)
                        convertedTick = tick;
                    saw240 |= action == 240;
                    saw241 |= action == 241;
                    trace.Add(new
                    {
                        tick, gap, action, frameState, pic = actor.GetRenderPicIndex(), combo,
                        stableId = actor.Runtime.StableId,
                        hp = actor.Runtime.HP, hpLost = actor.Runtime.HPLost,
                        hitStop = actor.Runtime.HitStop,
                        oneTuInput = tickDriver.World.OneTuInput,
                        inputPhase = tickDriver.World.InputPhase,
                        mp = actor.Health.PP,
                        runtimeMp = actor.Runtime.MP,
                        inputMpConsumedTotal350 = actor.Runtime.InputMpConsumedTotal350,
                        inputLocalResourceEnabled49D034 = actor.Runtime.InputLocalResourceEnabled49D034,
                        inputHeld = (int)applied.Buttons,
                        inputPressed = (int)applied.PressedButtons,
                        inputReleased = (int)applied.ReleasedButtons,
                        proxyCurrent = (byte[])actor.Runtime.NativeInputProxy.Current.Clone(),
                        proxyPrevious = (byte[])actor.Runtime.NativeInputProxy.Previous.Clone(),
                        proxyEdgeWindow = (byte[])actor.Runtime.NativeInputProxy.EdgeWindow.Clone(),
                        phase = step,
                        elapsedSeconds = EditorApplication.timeSinceStartup - started,
                    });
                    if (gap != 1)
                        throw new InvalidOperationException("A completed logic tick was missed; exact natural window observation is invalid.");
                    if (!ReferenceEquals(initialWorld, tickDriver.World) || tickDriver.World.OneTuInput ||
                        !ReferenceEquals(initialRuntime, actor.Runtime) || actor.Runtime.StableId != initialStableId ||
                        actor.ObjectId != 2 || owner == null ||
                        !ReferenceEquals(actor, typeof(BattleTestBootstrap).GetField("firstPlayerLf2",
                            BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(owner)))
                        throw new InvalidOperationException("Actor/World identity or 2tu input mode changed; observation is invalid.");
                    if (actor.Runtime.HP <= 0 || actor.Runtime.HP < initialHp ||
                        actor.Runtime.HPLost != initialHpLost || actor.Runtime.HitStop != 0)
                        throw new InvalidOperationException("Damage, death or HitStop interrupted the actor; observation is invalid.");
                    if ((timing == 1 || timing == 2) && afterWindowTick >= 0 && action != 301 &&
                        !IsUninterruptedPostWindowFrame(action, frameState))
                        throw new InvalidOperationException("Post254 actor left the authored standing/normal attack tail; negative observation is invalid.");
                    if (step == 0 && combo == 1)
                    {
                        defendTick = tick;
                        step = 1;
                        Queue("forward-after-observed-combo1", forward);
                        ResetPulse(tick);
                    }
                    else if (step == 1 && combo == (forward == Key.A ? 3 : 2))
                    {
                        forwardTick = tick;
                        step = 2;
                        Queue("jump-after-observed-direction-combo", Key.K);
                        ResetPulse(tick);
                    }
                    else if (step == 2 && (action == 240 || action == 241))
                    {
                        skillTick = tick;
                        step = 3;
                        Queue("release-after-natural-skill-entry");
                    }
                    else if (step < 3 && tick - lastPulseTick >= 2)
                    {
                        if (!releasePulse && pulseAttempts >= 8)
                            throw new InvalidOperationException("Physical combo step exhausted eight input pulses.");
                        releasePulse = !releasePulse;
                        if (releasePulse)
                            Queue("combo-step-release-retry");
                        else
                        {
                            pulseAttempts++;
                            Queue("combo-step-press-retry", step == 0 ? Key.L : step == 1 ? forward : Key.K);
                        }
                        lastPulseTick = tick;
                    }
                    if (step == 3 && action == 253)
                    {
                        if (firstWindowTick < 0)
                            firstWindowTick = tick;
                        else if (secondWindowTick < 0)
                            secondWindowTick = tick;
                    }
                    if (step == 3 && firstWindowTick >= 0 && action == 254 && afterWindowTick < 0)
                        afterWindowTick = tick;
                    bool queuePoint = timing == 0 ? firstWindowTick == tick :
                        timing == 1 ? secondWindowTick == tick : afterWindowTick == tick;
                    if (queuedAttackTick < 0 && queuePoint)
                    {
                        queuedAttackTick = tick;
                        Queue("attack-after-selected-observed-window", Key.J);
                    }
                    if (convertedTick >= 0)
                    {
                        bool sampledInSecond253 = timing == 1 &&
                            frameInputAttackTick > secondWindowTick && frameInputAttackPhase == 0 &&
                            nativeAttackSampleTick == frameInputAttackTick;
                        Complete((timing == 0 || sampledInSecond253) && skillTick >= 0 &&
                            frameInputAttackTick >= 0 && nativeAttackSampleTick >= 0,
                            "Observed action301; FrameInputSet J tick=" + frameInputAttackTick +
                            "/phase=" + frameInputAttackPhase + "; native J sample tick=" +
                            nativeAttackSampleTick + "/phase=" + nativeAttackSamplePhase + ".");
                        return;
                    }
                    if (queuedAttackTick >= 0 && tick >= queuedAttackTick + 2 && !releasedAttack)
                    {
                        releasedAttack = true;
                        Queue("release-attack-after-two-ticks");
                    }
                    if (queuedAttackTick >= 0 && tick >= queuedAttackTick + 12)
                    {
                        bool expectedSecond253Miss = timing == 1 && firstWindowTick >= 0 &&
                            secondWindowTick == queuedAttackTick && afterWindowTick == frameInputAttackTick &&
                            frameInputAttackPhase == 1 && nativeAttackSampleTick > frameInputAttackTick &&
                            nativeAttackSamplePhase == 0;
                        Complete((timing == 2 || expectedSecond253Miss) &&
                            frameInputAttackTick >= 0 && nativeAttackSampleTick >= 0 && skillTick >= 0,
                            "Observed uninterrupted 12 ticks after J queue without action301; FrameInputSet J tick=" +
                            frameInputAttackTick + "/phase=" + frameInputAttackPhase +
                            "; native J sample tick=" + nativeAttackSampleTick + "/phase=" + nativeAttackSamplePhase +
                            "; formal 2tu second-253 miss=" + expectedSecond253Miss + ".");
                        return;
                    }
                    if (tick >= initialTick + 120)
                        throw new TimeoutException("Natural combo/window sequence exceeded 120 ticks.");
                }
                catch (Exception exception)
                {
                    Complete(false, "Natural probe failed: " + exception);
                }
            }

            private static bool IsUninterruptedPostWindowFrame(int action, int state)
            {
                // Formal c/nar/nar.dat: 254 -> standing; J selects 20/25 -> 46..49 -> standing.
                return (action == 254 && state == 3) ||
                    (action >= 0 && action <= 3 && state == 0) ||
                    ((action == 20 || action == 25 || (action >= 46 && action <= 49)) && state == 15);
            }

            private void ResetPulse(int tick)
            {
                pulseAttempts = 1;
                releasePulse = false;
                lastPulseTick = tick;
            }

            private void OnStateChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                    Complete(false, "Play Mode exited before natural probe completion.");
            }

            private void OnReload()
            {
                Complete(false, "Assembly reload interrupted natural probe.");
            }

            private void Complete(bool passed, string message)
            {
                if (finished)
                    return;
                finished = true;
                EditorApplication.update -= ObserveNatural;
                EditorApplication.playModeStateChanged -= OnStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= OnReload;
                try
                {
                    Queue("final-key-release");
                    if (device != null)
                        InputSystem.Update();
                }
                catch (Exception exception)
                {
                    passed = false;
                    message += " Key release failed: " + exception.Message;
                }
                if (controlledTickStepping && tickDriver != null)
                    tickDriver.SetPaused(initialPaused);
                active = null;
                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                using (var stream = new FileStream(outputPath, FileMode.CreateNew, FileAccess.Write))
                using (var writer = new StreamWriter(stream))
                    writer.Write(JsonConvert.SerializeObject(new
                    {
                        status = passed ? "PASS" : "FAIL", message, timing,
                        initialTick, initialMp, initialRuntimeMp, initialMpConsumed,
                        raisedMp, forward = forward.ToString(),
                        initialStableId, initialHp, initialHpLost, initialInputPhase,
                        requiredOneTuInput = false, controlledTickStepping, initialPaused,
                        defendTick, forwardTick, skillTick, saw240, saw241,
                        firstWindowTick, secondWindowTick, afterWindowTick,
                        queuedAttackTick, frameInputAttackTick, frameInputAttackPhase,
                        nativeAttackSampleTick, nativeAttackSamplePhase, nativeAttackEdgeTick, convertedTick,
                        canonicalAttackButton = "Jump (physical J via existing action mapping)",
                        evidenceScope = "Synthetic physical device events and one controlled local-provider tick per Editor update in original Editor; not normal wall-clock cadence, human keyboard, or formal EXE pixel proof.",
                        queuedEvents = events, rows = trace,
                    }, Formatting.Indented));
                if (passed)
                    Debug.Log("[RasenganNaturalPlayProbe] PASS: " + outputPath);
                else
                    Debug.LogError("[RasenganNaturalPlayProbe] FAIL: " + message + " Result: " + outputPath);
            }
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
