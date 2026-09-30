#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07ChiyoNaturalBattlePlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_ChiyoNaturalBattlePlay.request.json";
        private const string ResultRoot = "artifacts/diagnostics/NTSD28-Q07-CHIYO-CONTROL-NATURAL-PLAY-001/";
        private const string SessionKey = "NTSD.Q07.ChiyoNaturalBattlePlay";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character chiyo;
        private static LF2Character opponent;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int dynamicUpdates;
        private static int queuedAtUpdate;
        private static readonly MethodInfo DynamicInputUpdate = typeof(InputSystem).GetMethod(
            "Update", BindingFlags.Static | BindingFlags.NonPublic, null,
            new[] { typeof(InputUpdateType) }, null);
        private static bool closing;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public bool directCanonical;
            public bool rngAudit;
            public bool aiTick20Audit;
            public bool slotAllocationAudit;
        }

        [Serializable]
        private sealed class SlotViewSample
        {
            public int slot, entityObjectId = -1;
            public long generation, allocationEpoch;
            public bool available, claimed;
        }

        [Serializable]
        private sealed class StructuralEventSample
        {
            public int tick, cursorSlot, actorSlot, slot, searchStart, searchEndExclusive;
            public int occupantObjectId = -1;
            public long lifecycleEpoch;
            public string pass, action, before, after, sourceKind;
        }

        private sealed class StructuralEventRecorder : IBattleParityStructuralEventSink
        {
            private readonly SimulationWorld observedWorld;
            private readonly List<StructuralEventSample> events;

            public StructuralEventRecorder(
                SimulationWorld observedWorld,
                List<StructuralEventSample> events)
            {
                this.observedWorld = observedWorld;
                this.events = events;
            }

            public void Record(BattleParityStructuralEvent value)
            {
                int occupantObjectId = -1;
                if (value.Slot >= 0 &&
                    observedWorld.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                        value.Slot, out RuntimeSlotTable.ReadOnlySlotView view))
                {
                    occupantObjectId = view.Entity?.ObjectId ?? -1;
                }
                events.Add(new StructuralEventSample
                {
                    tick = value.Tick,
                    cursorSlot = value.CursorSlot,
                    actorSlot = value.ActorSlot,
                    slot = value.Slot,
                    searchStart = value.SearchStart,
                    searchEndExclusive = value.SearchEndExclusive,
                    lifecycleEpoch = (long)value.LifecycleEpoch,
                    pass = value.Pass,
                    action = value.Action,
                    before = value.Before,
                    after = value.After,
                    sourceKind = value.SourceKind,
                    occupantObjectId = occupantObjectId,
                });
            }
        }

        [Serializable]
        private sealed class AiEntitySample
        {
            public int matches, slot = -1, action = -1, targetSlot = -1;
            public int sourceX, sourceZ, physicalX, physicalZ, relationTeam;
            public bool aiControlled;
        }

        [Serializable]
        private sealed class DynamicEntitySample
        {
            public int relativeTick, slot, objectId, action;
            public bool aiControlled;
        }

        [Serializable]
        private sealed class RandomCallSample
        {
            public string origin;
            public int callSite, upperBound, result, counterAfter, indexAfter;
            public long totalCalls;
        }

        private sealed class RandomCallRecorder : INTSD28NativeRandomCallObserver
        {
            private readonly string origin;
            private readonly List<RandomCallSample> calls;

            public RandomCallRecorder(string origin, List<RandomCallSample> calls)
            {
                this.origin = origin;
                this.calls = calls;
            }

            public void OnCrtNext(NTSD28NativeCrtCall call) { }

            public void OnSynchronizedNext(NTSD28NativeSynchronizedCall call)
            {
                calls.Add(new RandomCallSample
                {
                    origin = origin,
                    callSite = (int)call.CallSite,
                    upperBound = call.UpperBound,
                    result = call.Result,
                    counterAfter = call.CounterAfter,
                    indexAfter = call.IndexAfter,
                    totalCalls = (long)call.TotalCalls,
                });
            }
        }

        [Serializable]
        private sealed class TickSample
        {
            public int relativeTick, tick, inputPhase, chiyoAction, chiyoMp, puppetAction = -1;
            public int oid419Count, oid854Count;
            public string physicalKeys, canonicalButtons, canonicalPressed, canonicalReleased;
            public int chiyoLinkStateBefore, chiyoKind6OverrideBefore;
            public long synchronizedCallsBefore, synchronizedCallsAfter;
            public int synchronizedIndexBefore, synchronizedIndexAfter;
            public int synchronizedCounterBefore, synchronizedCounterAfter;
            public int lastSynchronizedCallSiteAfter, standingAttackRngResult = -1;
            public bool standingAttackRngObserved;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId, startedUtc, releaseStartedUtc, phase, status, error, firstDifference;
            public string sceneHashBefore, sceneHashAfter, contentRoot, puppet407CandidateOrdinals;
            public int startTick, endTick, initialInputPhase, keyboardDeviceId, first419Tick = -1;
            public int first854Tick = -1, firstChiyo420Tick = -1, firstPuppet400Tick = -1;
            public int firstPuppet407Tick = -1, remainingWorldObjects = -1;
            public int remainingRuntimeSlots = -1, remainingPoolBorrowers = -1;
            public bool configuredBeforeStart, candidate407Checked, ordinal1HasGeometry;
            public bool ordinal1InCandidates, stopped, detached, neutralKeyboardObserved;
            public bool exitedPlay, sceneCleanAfter;
            public bool directCanonical;
            public bool rngAudit;
            public bool aiTick20Audit;
            public bool slotAllocationAudit;
            public SlotViewSample tick18Slot50Before, tick18Slot51Before;
            public SlotViewSample tick18Slot50After, tick18Slot51After;
            public List<StructuralEventSample> tick18StructuralEvents = new List<StructuralEventSample>();
            public AiEntitySample tick19Oid850, tick20Oid850;
            public List<DynamicEntitySample> earlyDynamicEntities = new List<DynamicEntitySample>();
            public List<RandomCallSample> tick20DirectCalls = new List<RandomCallSample>();
            public List<RandomCallSample> tick20AcceptedAiCalls = new List<RandomCallSample>();
            public List<TickSample> samples = new List<TickSample>();
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
            EditorApplication.playModeStateChanged -= OnPlayMode;
            EditorApplication.playModeStateChanged += OnPlayMode;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            InputSystem.onAfterUpdate -= OnInputUpdate;
            InputSystem.onAfterUpdate += OnInputUpdate;
        }

        private static void OnInputUpdate()
        {
            if (InputState.currentUpdateType == InputUpdateType.Dynamic)
                dynamicUpdates++;
        }

        private static void RestoreSession()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, "");
            if (string.IsNullOrEmpty(saved)) return;
            report = JsonUtility.FromJson<Report>(saved);
            if (report.phase == "INPUT" || report.phase == "NEUTRAL")
                Fail("Domain reload interrupted the measured input sequence.");
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RestoreSession();
            if (report == null || report.phase != "STARTUP" || scene.path != BattleScene ||
                !EditorApplication.isPlaying || report.configuredBeforeStart)
                return;
            try
            {
                BattleTestBootstrap[] matches = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        value.gameObject.scene == scene && !EditorUtility.IsPersistent(value)).ToArray();
                Require(matches.Length == 1, "Expected one active BattleTestBootstrap in Play clone.");
                SetBootstrapField(matches[0], "overrideCharacterIds", new[] { 8, 2 });
                SetBootstrapField(matches[0], "forceWalkingMode", false);
                SetBootstrapField(matches[0], "forceRunningMode", false);
                report.configuredBeforeStart = true;
                SaveSession();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void SetBootstrapField(BattleTestBootstrap bootstrap, string name, object value)
        {
            FieldInfo field = typeof(BattleTestBootstrap).GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Require(field != null, "Missing bootstrap field " + name);
            field.SetValue(bootstrap, value);
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            RestoreSession();
            if (report == null) return;
            if (state == PlayModeStateChange.EnteredPlayMode && report.phase == "STARTUP" &&
                !report.configuredBeforeStart)
                Fail("Play clone was not configured before bootstrap Start.");
            if (state == PlayModeStateChange.ExitingPlayMode && report.phase != "EXITING" &&
                report.phase != "RELEASING")
                Fail("Play exited before the probe completed.");
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
                FinishAfterExit();
        }

        private static void Poll()
        {
            if (closing || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                RestoreSession();
                if (report == null)
                {
                    TryStartRequest();
                    return;
                }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) FinishAfterExit();
                    return;
                }
                if (report.phase == "CLEANUP_BLOCKED") return;
                if (report.phase == "RELEASING")
                {
                    Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
                    bool neutral = keyboard != null && !keyboard.jKey.isPressed &&
                        !keyboard.kKey.isPressed && !keyboard.lKey.isPressed && !keyboard.wKey.isPressed;
                    if (!neutral && DateTime.UtcNow - DateTime.Parse(report.releaseStartedUtc,
                            null, System.Globalization.DateTimeStyles.RoundtripKind) < TimeSpan.FromSeconds(5))
                    {
                        EditorApplication.QueuePlayerLoopUpdate();
                        return;
                    }
                    report.neutralKeyboardObserved = neutral;
                    if (!neutral) FailAfterCleanup("Keyboard did not reach neutral before Play exit.");
                    report.phase = "EXITING";
                    SaveSession();
                    EditorApplication.ExitPlaymode();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc,
                    null, System.Globalization.DateTimeStyles.RoundtripKind) < TimeSpan.FromMinutes(6),
                    "Probe startup/input timeout.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP")
                {
                    WaitForRoster();
                    return;
                }
                Require(driver != null && ReferenceEquals(driver.World, world), "Scene world changed.");
                Require(driver.IsPaused && !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                    "Driver pause/worker boundary changed.");
                Require(driver.CurrentTickIndex == report.endTick, "Unobserved logic tick while paused.");
                if (dynamicUpdates <= queuedAtUpdate) return;
                if (report.phase == "NEUTRAL")
                {
                    if (report.directCanonical && world.InputPhase != 0)
                    {
                        int nextTick = driver.CurrentTickIndex + 1;
                        var neutral = new FrameInputSet(nextTick, new[]
                        {
                            new SimulationPlayerInput(0, SimulationInputButtons.None),
                            new SimulationPlayerInput(1, SimulationInputButtons.None),
                        });
                        Require(driver.StepOneTick(neutral, ignorePaused: true,
                            buildPresentation: true), "Phase alignment neutral tick was rejected.");
                        report.startTick = report.endTick = driver.CurrentTickIndex;
                        report.initialInputPhase = world.InputPhase;
                    }
                    InitializeActor(chiyo, 150, 500);
                    InitializeActor(opponent, 500, 1200);
                    report.phase = "INPUT";
                    QueueForNextTick();
                    return;
                }
                AdvanceAndObserve();
            }
            catch (Exception error)
            {
                if (report != null) Fail(error.ToString());
                else Debug.LogError("[Q07 Chiyo natural Play] " + error);
            }
        }

        private static void TryStartRequest()
        {
            if (!File.Exists(RequestPath)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(RequestPath, JsonUtility.ToJson(request, true));
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 100 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require(!File.Exists(ResultRoot + request.runId + ".json"), "Refusing to overwrite runId.");
            Require(!request.rngAudit || request.directCanonical,
                "RNG audit requires the existing direct canonical route.");
            Require(!request.aiTick20Audit || (request.directCanonical && request.rngAudit),
                "Tick20 AI audit requires direct canonical input and RNG audit.");
            Require(!request.slotAllocationAudit || request.aiTick20Audit,
                "Slot allocation audit requires the existing tick20 AI audit.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Request requires Edit Mode.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase), "Only the original project Editor is allowed.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires the sole clean saved NTSD_Battle Scene.");
            report = new Report { runId = request.runId, directCanonical = request.directCanonical,
                rngAudit = request.rngAudit,
                aiTick20Audit = request.aiTick20Audit,
                slotAllocationAudit = request.slotAllocationAudit,
                startedUtc = DateTime.UtcNow.ToString("O"),
                phase = "STARTUP", status = "RUNNING", sceneHashBefore = HashScene() };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart, "Play clone was not configured before Start.");
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5) return;
            Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null, "Dedicated worker failed.");
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                stableUpdates = 0;
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 4) return;
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity p1) && p1 is LF2Character,
                "P1 roster is unavailable.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity p2) && p2 is LF2Character,
                "P2 roster is unavailable.");
            chiyo = (LF2Character)p1;
            opponent = (LF2Character)p2;
            Require(chiyo.ObjectId == 8 && opponent.ObjectId == 2 &&
                !chiyo.AiControlled && !opponent.AiControlled, "Expected human Chiyo/Naruto roster.");
            CharacterInputModule input = chiyo.Controller as CharacterInputModule;
            Require(input?.MoveAction?.enabled == true && input.AttackAction?.enabled == true &&
                input.JumpAction?.enabled == true && input.DefendAction?.enabled == true &&
                input.AttackAction.actionMap.name == "Player_1", "P1 physical action map unavailable.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime", "Wrong formal content root.");
            if (!report.directCanonical)
            {
                Require(Keyboard.current != null, "Keyboard.current unavailable.");
                report.keyboardDeviceId = Keyboard.current.deviceId;
            }
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.initialInputPhase = world.InputPhase;
            report.phase = "NEUTRAL";
            if (report.directCanonical)
            {
                queuedAtUpdate = dynamicUpdates - 1;
                EditorApplication.QueuePlayerLoopUpdate();
            }
            else QueueKeys();
            SaveSession();
        }

        private static void InitializeActor(LF2Character character, int hp, int x)
        {
            character.ImmediateFrame(0);
            character.Initialize(hp, character.Runtime.PPMax);
            character.Runtime.MP = 500;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.Runtime.SetPosition(x, 0, 650);
            AppManager.SyncParticipantBirthPosition(character, x, 650);
        }

        private static SimulationInputButtons ExpectedButtons(int index)
        {
            // The existing input carrier crosses Unity action bits into NTSD J/K/L semantics.
            if (index < 2 || index == 55 || index == 56) return SimulationInputButtons.Attack;
            if (index < 4) return SimulationInputButtons.Up;
            if (index < 6) return SimulationInputButtons.Defend;
            if (index == 53 || index == 54) return SimulationInputButtons.Jump;
            return SimulationInputButtons.None;
        }

        private static Key[] PhysicalKeys(int index)
        {
            if (index < 2 || index == 55 || index == 56) return new[] { Key.L };
            if (index < 4) return new[] { Key.W };
            if (index < 6) return new[] { Key.K };
            if (index == 53 || index == 54) return new[] { Key.J };
            return Array.Empty<Key>();
        }

        private static void QueueForNextTick()
        {
            if (report.directCanonical)
            {
                queuedAtUpdate = dynamicUpdates - 1;
                SaveSession();
                EditorApplication.QueuePlayerLoopUpdate();
                return;
            }
            QueueKeys(PhysicalKeys(report.samples.Count));
            SaveSession();
        }

        private static void QueueKeys(params Key[] keys)
        {
            Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
            Require(keyboard != null, "Original keyboard device disappeared.");
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            queuedAtUpdate = dynamicUpdates;
            EditorApplication.QueuePlayerLoopUpdate();
        }

        private static void AdvanceAndObserve()
        {
            int index = report.samples.Count;
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity p1) && ReferenceEquals(p1, chiyo) &&
                world.TryResolveRosterInputEntity(1, out LF2Entity p2) && ReferenceEquals(p2, opponent),
                "Roster identity changed.");
            int linkStateBefore = report.rngAudit ? chiyo.Runtime.LinkState : 0;
            int kind6OverrideBefore = report.rngAudit ? chiyo.Runtime.HitConfirmEa : 0;
            NTSD28NativeRandomScalarState randomBefore = report.rngAudit ?
                world.NativeRandom.CaptureScalarState() : default;
            bool captureTick18Slots = report.slotAllocationAudit && index == 17;
            bool captureTick20Calls = report.aiTick20Audit && index == 19;
            bool structuralAttached = false;
            if (captureTick18Slots)
            {
                report.tick18Slot50Before = CaptureSlotView(50);
                report.tick18Slot51Before = CaptureSlotView(51);
            }
            try
            {
                if (captureTick18Slots)
                {
                    Require(world.StructuralEventSinkForServices == null,
                        "Another structural event observer is already attached.");
                    world.SetStructuralEventSinkForDiagnostics(
                        new StructuralEventRecorder(world, report.tick18StructuralEvents),
                        driver.CurrentTickIndex + 1, "Q07-tick18-slot-allocation");
                    structuralAttached = true;
                }
                if (captureTick20Calls)
                {
                    world.NativeRandom.SetDiagnosticCallObserver(
                        new RandomCallRecorder("direct", report.tick20DirectCalls));
                    world.SetAcceptedAiRandomTraceObserverForDiagnostics(
                        new RandomCallRecorder("acceptedAi", report.tick20AcceptedAiCalls));
                }
                if (report.directCanonical)
                {
                    int nextTick = driver.CurrentTickIndex + 1;
                    SimulationInputButtons buttons = ExpectedButtons(index);
                    SimulationInputButtons previous = index == 0 ?
                        SimulationInputButtons.None : ExpectedButtons(index - 1);
                    var requested = new FrameInputSet(nextTick, new[]
                    {
                        new SimulationPlayerInput(0, buttons,
                            buttons & ~previous, previous & ~buttons),
                        new SimulationPlayerInput(1, SimulationInputButtons.None),
                    });
                    Require(driver.StepOneTick(requested, ignorePaused: true,
                        buildPresentation: true), "Full Driver rejected direct canonical tick.");
                }
                else
                {
                    QueueKeys(PhysicalKeys(index));
                    Require(DynamicInputUpdate != null, "Typed Dynamic Input System update is unavailable.");
                    DynamicInputUpdate.Invoke(null, new object[] { InputUpdateType.Dynamic });
                    Require(driver.StepOneTick(ignorePaused: true,
                        buildPresentation: true), "Full Driver rejected physical tick.");
                }
            }
            finally
            {
                if (structuralAttached)
                    world.SetStructuralEventSinkForDiagnostics(null,
                        driver.CurrentTickIndex, "Q07-tick18-slot-allocation-end");
                if (captureTick20Calls)
                {
                    world.SetAcceptedAiRandomTraceObserverForDiagnostics(null);
                    world.NativeRandom.SetDiagnosticCallObserver(null);
                }
            }
            NTSD28NativeRandomScalarState randomAfter = report.rngAudit ?
                world.NativeRandom.CaptureScalarState() : default;
            if (captureTick18Slots)
            {
                report.tick18Slot50After = CaptureSlotView(50);
                report.tick18Slot51After = CaptureSlotView(51);
            }
            int tick = driver.CurrentTickIndex;
            Require(tick == report.endTick + 1, "Skipped or unobserved logic tick.");
            report.endTick = tick;
            FrameInputSet frame = driver.LastAppliedFrameInput;
            SimulationPlayerInput input = default;
            SimulationPlayerInput other = default;
            bool foundInput = false;
            bool foundOther = false;
            foreach (SimulationPlayerInput player in frame.Players)
            {
                if (player.PlayerSlot == 0) { input = player; foundInput = true; }
                if (player.PlayerSlot == 1) { other = player; foundOther = true; }
            }
            Require(foundInput && foundOther, "Missing P1/P2 sampled frame input.");
            var sample = new TickSample { relativeTick = index + 1, tick = tick,
                inputPhase = world.InputPhase, chiyoAction = chiyo.Frame.N, chiyoMp = chiyo.Runtime.MP,
                physicalKeys = report.directCanonical ? "DirectCanonical" :
                    string.Join("+", PhysicalKeys(index)),
                canonicalButtons = input.Buttons.ToString(),
                canonicalPressed = input.PressedButtons.ToString(),
                canonicalReleased = input.ReleasedButtons.ToString() };
            if (report.rngAudit)
            {
                sample.chiyoLinkStateBefore = linkStateBefore;
                sample.chiyoKind6OverrideBefore = kind6OverrideBefore;
                sample.synchronizedCallsBefore = (long)randomBefore.SynchronizedCalls;
                sample.synchronizedCallsAfter = (long)randomAfter.SynchronizedCalls;
                sample.synchronizedIndexBefore = randomBefore.SynchronizedIndex;
                sample.synchronizedIndexAfter = randomAfter.SynchronizedIndex;
                sample.synchronizedCounterBefore = randomBefore.SynchronizedCounter;
                sample.synchronizedCounterAfter = randomAfter.SynchronizedCounter;
                sample.lastSynchronizedCallSiteAfter =
                    (int)randomAfter.LastSynchronizedCallSite;
                if (randomAfter.LastSynchronizedCallSite == 0x82u)
                {
                    NTSD28SynchronizedRandomState synchronized =
                        world.NativeRandom.CaptureSynchronizedState();
                    sample.standingAttackRngObserved = true;
                    sample.standingAttackRngResult = (int)(
                        ((uint)synchronized.Table[synchronized.Index] +
                         (uint)synchronized.Counter) % 2u);
                }
            }
            SimulationInputButtons expected = ExpectedButtons(index);
            SimulationInputButtons prior = index == 0 ? SimulationInputButtons.None : ExpectedButtons(index - 1);
            if (frame.TickIndex != tick || input.Buttons != expected ||
                input.PressedButtons != (expected & ~prior) ||
                input.ReleasedButtons != (prior & ~expected) ||
                other.Buttons != SimulationInputButtons.None)
                FirstDifference("Physical/canonical input mismatch at relative tick " + (index + 1));
            LF2Entity puppet = null;
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null) continue;
                if (report.aiTick20Audit && index >= 16 && index <= 19 && slot >= 50)
                    report.earlyDynamicEntities.Add(new DynamicEntitySample
                    {
                        relativeTick = index + 1,
                        slot = slot,
                        objectId = entity.ObjectId,
                        action = entity.Frame.N,
                        aiControlled = entity.AiControlled,
                    });
                if (entity.ObjectId == 419) sample.oid419Count++;
                if (entity.ObjectId != 854) continue;
                sample.oid854Count++;
                if (puppet == null) puppet = entity;
            }
            if (puppet != null) sample.puppetAction = puppet.Frame.N;
            if (report.aiTick20Audit && index == 18)
                report.tick19Oid850 = CaptureAiEntity(850);
            if (report.aiTick20Audit && index == 19)
                report.tick20Oid850 = CaptureAiEntity(850);
            report.samples.Add(sample);
            if (report.first419Tick < 0 && sample.oid419Count > 0) report.first419Tick = tick;
            if (report.first854Tick < 0 && sample.oid854Count > 0) report.first854Tick = tick;
            if (report.firstChiyo420Tick < 0 && sample.chiyoAction == 420) report.firstChiyo420Tick = tick;
            if (report.firstPuppet400Tick < 0 && sample.puppetAction == 400) report.firstPuppet400Tick = tick;
            if (report.firstPuppet407Tick < 0 && sample.puppetAction == 407)
            {
                report.firstPuppet407Tick = tick;
                InspectFrame407Candidates(puppet);
            }
            SaveSession();
            if (report.aiTick20Audit && report.samples.Count == 20)
            {
                if (report.tick19Oid850 == null || report.tick20Oid850 == null ||
                    report.tick19Oid850.matches != 1 || report.tick20Oid850.matches != 1)
                    FirstDifference("OID850 was not unique at relative tick19/20.");
                report.status = string.IsNullOrEmpty(report.firstDifference)
                    ? "PASS_SCOPED_TICK20_AI_AUDIT" : "OBSERVED_DIFFERENCE";
                Close();
                return;
            }
            if (report.samples.Count < 120)
            {
                QueueForNextTick();
                return;
            }
            if (report.firstPuppet407Tick < 0) FirstDifference("Puppet did not naturally reach frame407 in 120 measured ticks.");
            if (!report.candidate407Checked) FirstDifference("Natural frame407 candidate row was not checked.");
            report.status = string.IsNullOrEmpty(report.firstDifference)
                ? "PASS_SCOPED_NATURAL_CHAIN" : "OBSERVED_DIFFERENCE";
            Close();
        }

        private static AiEntitySample CaptureAiEntity(int objectId)
        {
            var sample = new AiEntitySample();
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null || entity.ObjectId != objectId) continue;
                sample.matches++;
                if (sample.matches != 1) continue;
                sample.slot = slot;
                sample.action = entity.Frame.N;
                sample.targetSlot = entity.ObjectAiTargetSlot3F8;
                sample.sourceX = entity.Runtime.SourceRuleXInt;
                sample.sourceZ = entity.Runtime.SourceRuleZInt;
                sample.physicalX = (int)entity.Runtime.X;
                sample.physicalZ = (int)entity.Runtime.Z;
                sample.relationTeam = entity.RelationTeam;
                sample.aiControlled = entity.AiControlled;
            }
            return sample;
        }

        private static SlotViewSample CaptureSlotView(int slot)
        {
            var sample = new SlotViewSample { slot = slot };
            if (!world.TryGetRuntimeSlotReadOnlyViewForDiagnostics(
                    slot, out RuntimeSlotTable.ReadOnlySlotView view))
                return sample;
            sample.available = true;
            sample.claimed = view.Claimed;
            sample.generation = view.Generation;
            sample.allocationEpoch = (long)view.AllocationEpoch;
            sample.entityObjectId = view.Entity?.ObjectId ?? -1;
            return sample;
        }

        private static void InspectFrame407Candidates(LF2Entity puppet)
        {
            Require(puppet != null && puppet.Frame.D.itrs.Count >= 2, "Natural puppet407 has fewer than two authored ITRs.");
            Require(puppet.Frame.D.itrs[1].kind == 100100, "Natural puppet407 ordinal1 is not kind100100.");
            report.ordinal1HasGeometry = puppet.Frame.D.itrs[1].hasGeometry;
            BruteForceSceneQuery query = world.SceneQuery as BruteForceSceneQuery;
            Require(query != null, "Production scene query is not BruteForceSceneQuery.");
            try
            {
                world.CaptureCollisionFrameSnapshotsAll();
                world.CollectCollisionCandidatesAll();
                Require(query.TryGetCollisionCandidateSequence(puppet, out List<SceneQueryHit> candidates),
                    "Natural puppet407 candidate sequence unavailable.");
                report.puppet407CandidateOrdinals = string.Join(",", candidates.Select(value => value.ItrIndex));
                report.ordinal1InCandidates = candidates.Any(value => value.ItrIndex == 1);
                report.candidate407Checked = true;
                if (report.ordinal1HasGeometry || report.ordinal1InCandidates)
                    FirstDifference("Control-only authored ordinal1 entered geometry/candidate route.");
            }
            finally { world.EndCollisionCandidateConsumption(); }
        }

        private static void FirstDifference(string message)
        {
            if (string.IsNullOrEmpty(report.firstDifference)) report.firstDifference = message;
        }

        private static void Fail(string error)
        {
            report.status = "FAIL";
            report.error += error + "\n";
            FirstDifference(error);
            Close();
        }

        private static void FailAfterCleanup(string error)
        {
            report.status = "FAIL";
            report.error += error + "\n";
            FirstDifference(error);
        }

        private static void Close()
        {
            if (closing || report.phase == "EXITING") return;
            closing = true;
            try
            {
                driver = SimulationTickDriver.Instance;
                if (EditorApplication.isPlaying && driver != null)
                {
                    BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                    bool mapCleared = true;
                    if (shutdown.RuntimeStagesCompleted)
                    {
                        foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                        {
                            if (bootstrap == null || EditorUtility.IsPersistent(bootstrap) ||
                                !bootstrap.gameObject.scene.IsValid()) continue;
                            bootstrap.DisablePresentation();
                            mapCleared &= bootstrap.IsRuntimeMapCleared;
                        }
                        shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                    }
                    report.stopped = shutdown.IsComplete;
                    report.remainingWorldObjects = shutdown.RemainingWorldObjects;
                    report.remainingRuntimeSlots = shutdown.RemainingRuntimeSlots;
                    report.remainingPoolBorrowers = shutdown.RemainingPoolBorrowers;
                    report.detached = driver.World == null;
                    Require(report.stopped && report.detached && report.remainingWorldObjects == 0 &&
                        report.remainingRuntimeSlots == 0 && report.remainingPoolBorrowers == 0,
                        "Ordered shutdown did not reach zero-residue postconditions: " + shutdown.FailureReason);
                }
            }
            catch (Exception error) { FailAfterCleanup("Cleanup: " + error); }
            finally
            {
                try
                {
                    if (!report.directCanonical)
                    {
                        Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
                        if (keyboard != null) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                    }
                    else report.neutralKeyboardObserved = true;
                }
                catch (Exception error) { FailAfterCleanup("Keyboard release: " + error); }
                bool blocked = EditorApplication.isPlaying && driver != null && !report.stopped;
                report.phase = blocked ? "CLEANUP_BLOCKED" :
                    EditorApplication.isPlaying && !report.directCanonical ? "RELEASING" : "EXITING";
                report.releaseStartedUtc = DateTime.UtcNow.ToString("O");
                SaveSession();
                closing = false;
                EditorApplication.QueuePlayerLoopUpdate();
                if (report.directCanonical && EditorApplication.isPlaying && !blocked)
                    EditorApplication.ExitPlaymode();
                if (blocked)
                {
                    WriteReport(ResultRoot + report.runId + "-shutdown-blocked.json");
                    Debug.LogError("[Q07 Chiyo natural Play] Ordered shutdown blocked; Play retained.");
                }
                EditorApplication.delayCall += () =>
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode && report?.phase == "EXITING")
                        FinishAfterExit();
                };
            }
        }

        private static void FinishAfterExit()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = true;
            report.sceneHashAfter = HashScene();
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty;
            if (!report.neutralKeyboardObserved) FailAfterCleanup("Keyboard was not confirmed neutral after Play.");
            if (!report.sceneCleanAfter || report.sceneHashBefore != report.sceneHashAfter)
                FailAfterCleanup("Saved Battle Scene hash or clean state changed.");
            string path = ResultRoot + report.runId + ".json";
            WriteReport(path);
            Debug.Log("[Q07 Chiyo natural Play] " + report.status + ": " + path);
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            chiyo = opponent = null;
            stableTick = -1;
            stableUpdates = 0;
        }

        private static void SaveSession() => SessionState.SetString(SessionKey, JsonUtility.ToJson(report));

        private static void WriteReport(string path)
        {
            Directory.CreateDirectory(ResultRoot);
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream)) writer.Write(JsonUtility.ToJson(report, true));
        }

        private static string HashScene()
        {
            using (var hash = System.Security.Cryptography.SHA256.Create())
            using (var stream = File.OpenRead(BattleScene))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
