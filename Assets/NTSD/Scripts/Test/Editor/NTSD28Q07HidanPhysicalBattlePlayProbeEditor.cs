#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    internal static class NTSD28Q07HidanPhysicalBattlePlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_HidanPhysicalBattlePlay.request.json";
        private const string ResultRoot = "artifacts/diagnostics/NTSD28-Q07-HIDAN-PHYSICAL-BATTLE-PLAY-001/";
        private const string SessionKey = "NTSD.Q07.HidanPhysicalBattlePlay";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character target;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int dynamicUpdates;
        private static int queuedAtUpdate;
        private static bool closing;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public int targetX;
        }

        [Serializable]
        private sealed class EntitySample
        {
            public int slot, stableId, oid, frame, hp, pp, ppMax, caughtSlot, catchSourceSlot;
            public string facing;
            public double x, y, z, sourceX, sourceZ;
        }

        [Serializable]
        private sealed class InputSample
        {
            public bool present;
            public string buttons, pressed, released;
        }

        [Serializable]
        private sealed class TickSample
        {
            public int relativeTick, tick, canonicalTick, inputPhase;
            public string physicalKeys;
            public bool keyboardJPressed, keyboardKPressed, attackActionPressed, jumpActionPressed;
            public string attackActiveControl, jumpActiveControl;
            public InputSample p1, p2;
            public EntitySample actor, target;
            public int worldObjects, runtimeSlots;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId, startedUtc, phase, status, error, firstDifference, releaseStartedUtc;
            public int targetX;
            public string evidenceScope = "Original Editor Battle Scene; physical keyboard events; paused full Driver host steps using production input provider. Not LocalFreeRun wall-clock cadence or formal tick-zero same-world parity (project Z650 vs source Z350).";
            public string inputContract = "Serialized Player_1 physical J=Attack action -> canonical Jump; physical K=Jump action -> canonical Defend (existing crossed carrier).";
            public bool bootstrapConfiguredBeforeStart, stopped, worldDetached, exitedPlay, sceneCleanAfter, neutralKeyboardObserved;
            public string contentRoot, sceneHashBefore, sceneHashAfter, lifecycleAfter, shutdownStatus, shutdownStage, shutdownFailure;
            public int startTick, endTick, initialInputPhase, battleMode, keyboardDeviceId, firstCatchTick = -1, firstInjuryTick = -1, firstResourceTick = -1;
            public int firstActorCostTick = -1, firstTargetResourceIncreaseTick = -1;
            public int firstActorPostCatchIncreaseTick = -1, actorPostCatchIncrease, targetResourceIncrease;
            public int remainingWorldObjects = -1, remainingRuntimeSlots = -1, remainingPoolBorrowers = -1;
            public EntitySample initialActor, initialTarget;
            public List<EntitySample> initialWorld = new List<EntitySample>();
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
                Fail("Domain reload interrupted the measured sequence; no restart or parity inference.");
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RestoreSession();
            if (report == null || report.phase != "STARTUP" || scene.path != BattleScene ||
                !EditorApplication.isPlaying || report.bootstrapConfiguredBeforeStart)
                return;
            try
            {
                // sceneLoaded runs after Awake/OnEnable and before the clone's Start.
                BattleTestBootstrap[] matches = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        value.gameObject.scene == scene && !EditorUtility.IsPersistent(value)).ToArray();
                Require(matches.Length == 1, "Expected one active BattleTestBootstrap in Play clone.");
                SetBootstrapField(matches[0], "overrideCharacterIds", new[] { 24, 24 });
                SetBootstrapField(matches[0], "forceWalkingMode", false);
                SetBootstrapField(matches[0], "forceRunningMode", false);
                report.bootstrapConfiguredBeforeStart = true;
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
                !report.bootstrapConfiguredBeforeStart)
                Fail("sceneLoaded callback did not configure the clone before Start; no late bootstrap override attempted.");
            if (state == PlayModeStateChange.ExitingPlayMode && report.phase != "EXITING" && report.phase != "RELEASING")
                Fail("Play was exited before the probe completed.");
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
                    Keyboard releasedKeyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
                    if (releasedKeyboard != null && (releasedKeyboard.jKey.isPressed || releasedKeyboard.kKey.isPressed))
                    {
                        if (DateTime.UtcNow - DateTime.Parse(report.releaseStartedUtc,
                            null, System.Globalization.DateTimeStyles.RoundtripKind) < TimeSpan.FromSeconds(5))
                        {
                            EditorApplication.QueuePlayerLoopUpdate();
                            return;
                        }
                        report.status = "FAIL";
                        report.error += "Neutral keyboard was not observed within five seconds.\n";
                    }
                    report.neutralKeyboardObserved = releasedKeyboard != null &&
                        !releasedKeyboard.jKey.isPressed && !releasedKeyboard.kKey.isPressed;
                    if (!report.neutralKeyboardObserved)
                    {
                        report.status = "FAIL";
                        report.error += "Neutral keyboard state was not confirmed before Play exit.\n";
                    }
                    report.phase = "EXITING";
                    SaveSession();
                    EditorApplication.ExitPlaymode();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc,
                    null, System.Globalization.DateTimeStyles.RoundtripKind) < TimeSpan.FromMinutes(5),
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
                    InitializeActor(actor, 500);
                    InitializeActor(target, report.targetX);
                    report.initialActor = Capture(actor);
                    report.initialTarget = Capture(target);
                    for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
                    {
                        LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                        if (entity != null) report.initialWorld.Add(Capture(entity));
                    }
                    report.phase = "INPUT";
                    QueueForNextTick();
                    return;
                }
                AdvanceAndObserve();
            }
            catch (Exception error)
            {
                if (report != null) Fail(error.ToString());
                else Debug.LogError("[Q07 Hidan physical Play] " + error);
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
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Request must begin in Edit Mode.");
            report = new Report { runId = request.runId, targetX = request.targetX,
                startedUtc = DateTime.UtcNow.ToString("O"), phase = "STARTUP", status = "RUNNING" };
            Require(request.targetX == 580 || request.targetX == 1200, "targetX must be 580 or 1200.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase), "Only original project Editor is allowed.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires the sole clean saved NTSD_Battle Scene.");
            report.sceneHashBefore = HashScene();
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        private static void WaitForRoster()
        {
            Require(report.bootstrapConfiguredBeforeStart, "Play clone was not configured before Start.");
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
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity p1) &&
                p1 is LF2Character, "P1 normal roster is unavailable.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity p2) &&
                p2 is LF2Character, "P2 normal roster is unavailable.");
            actor = (LF2Character)p1;
            target = (LF2Character)p2;
            Require(actor != target && actor.ObjectId == 24 && target.ObjectId == 24 &&
                !actor.AiControlled && !target.AiControlled, "Expected two normal human Hidan actors.");
            Require(HasMap(actor, "Player_1") && HasMap(target, "Player_2"), "Player maps unavailable.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime", "Wrong formal content root.");
            Require(Keyboard.current != null, "Keyboard.current is unavailable.");
            report.keyboardDeviceId = Keyboard.current.deviceId;
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.initialInputPhase = world.InputPhase;
            report.battleMode = world.BattleGameModeId;
            report.phase = "NEUTRAL";
            QueueKeys();
            SaveSession();
        }

        private static bool HasMap(LF2Character character, string name)
        {
            CharacterInputModule input = character.Controller as CharacterInputModule;
            return input?.MoveAction?.enabled == true && input.AttackAction?.enabled == true &&
                input.JumpAction?.enabled == true && input.DefendAction?.enabled == true &&
                input.AttackAction.actionMap.name == name && input.JumpAction.actionMap.name == name;
        }

        private static void InitializeActor(LF2Character character, int x)
        {
            character.ImmediateFrame(0);
            character.Initialize(500, character.Runtime.PPMax);
            character.Runtime.MP = 300;
            character.Runtime.PP = 300;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.Runtime.SetPosition(x, 0, 650);
            AppManager.SyncParticipantBirthPosition(character, x, 650);
        }

        private static void QueueForNextTick()
        {
            int index = report.samples.Count;
            if (index < 2) QueueKeys(Key.J);
            else if (index < 4) QueueKeys(Key.K);
            else QueueKeys();
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
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity p1) && ReferenceEquals(p1, actor) &&
                world.TryResolveRosterInputEntity(1, out LF2Entity p2) && ReferenceEquals(p2, target),
                "Roster identity changed.");
            Require(driver.StepOneTick(ignorePaused: true, buildPresentation: true), "Full Driver rejected tick.");
            int tick = driver.CurrentTickIndex;
            Require(tick == report.endTick + 1, "Skipped/unobserved completed logic tick.");
            report.endTick = tick;
            FrameInputSet frame = driver.LastAppliedFrameInput;
            Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
            CharacterInputModule input = actor.Controller as CharacterInputModule;
            var sample = new TickSample { relativeTick = index + 1, tick = tick,
                canonicalTick = frame.TickIndex, inputPhase = world.InputPhase,
                physicalKeys = index < 2 ? "J (Attack action)" : index < 4 ? "K (Jump action)" : "Neutral",
                keyboardJPressed = keyboard?.jKey.isPressed == true,
                keyboardKPressed = keyboard?.kKey.isPressed == true,
                attackActionPressed = input?.AttackAction?.ReadValue<float>() > 0.5f,
                jumpActionPressed = input?.JumpAction?.ReadValue<float>() > 0.5f,
                attackActiveControl = input?.AttackAction?.activeControl?.path,
                jumpActiveControl = input?.JumpAction?.activeControl?.path,
                p1 = CaptureInput(frame, 0), p2 = CaptureInput(frame, 1), actor = Capture(actor), target = Capture(target),
                worldObjects = world.ObjectCount, runtimeSlots = world.ClaimedRuntimeSlotCountForDiagnostics };
            report.samples.Add(sample);
            SimulationInputButtons expected = index < 2 ? SimulationInputButtons.Jump :
                index < 4 ? SimulationInputButtons.Defend : SimulationInputButtons.None;
            SimulationInputButtons expectedPressed = index == 0 ? SimulationInputButtons.Jump :
                index == 2 ? SimulationInputButtons.Defend : SimulationInputButtons.None;
            SimulationInputButtons expectedReleased = index == 2 ? SimulationInputButtons.Jump :
                index == 4 ? SimulationInputButtons.Defend : SimulationInputButtons.None;
            if (frame.TickIndex != tick || !sample.p1.present || !sample.p2.present ||
                sample.p1.buttons != expected.ToString() || sample.p1.pressed != expectedPressed.ToString() ||
                sample.p1.released != expectedReleased.ToString() || sample.p2.buttons != SimulationInputButtons.None.ToString() ||
                sample.p2.pressed != SimulationInputButtons.None.ToString() || sample.p2.released != SimulationInputButtons.None.ToString())
                FirstDifference("Physical/canonical input sequence mismatch at relative tick " + (index + 1));
            if (report.firstCatchTick < 0 && actor.Runtime.CaughtSlotIndex == target.Runtime.SlotIndex &&
                target.Runtime.CatchSourceSlot90 == actor.Runtime.SlotIndex) report.firstCatchTick = tick;
            if (report.firstInjuryTick < 0 && target.Runtime.HP < report.initialTarget.hp) report.firstInjuryTick = tick;
            if (report.firstResourceTick < 0 && actor.Runtime.PP != report.initialActor.pp) report.firstResourceTick = tick;
            if (report.firstActorCostTick < 0 && actor.Runtime.PP < report.initialActor.pp) report.firstActorCostTick = tick;
            if (report.firstCatchTick >= 0)
            {
                int previousActorPp = index == 0 ? report.initialActor.pp : report.samples[index - 1].actor.pp;
                if (actor.Runtime.PP > previousActorPp)
                {
                    if (report.firstActorPostCatchIncreaseTick < 0) report.firstActorPostCatchIncreaseTick = tick;
                    report.actorPostCatchIncrease += actor.Runtime.PP - previousActorPp;
                }
                if (target.Runtime.PP > report.initialTarget.pp)
                {
                    if (report.firstTargetResourceIncreaseTick < 0) report.firstTargetResourceIncreaseTick = tick;
                    report.targetResourceIncrease = Math.Max(report.targetResourceIncrease,
                        target.Runtime.PP - report.initialTarget.pp);
                }
            }
            SaveSession();
            if (report.samples.Count < 40)
            {
                QueueForNextTick();
                return;
            }
            bool outcome = report.targetX == 580
                ? report.firstCatchTick >= 0 && report.firstInjuryTick >= 0 &&
                    report.firstTargetResourceIncreaseTick >= 0 && report.firstActorPostCatchIncreaseTick >= 0
                : report.firstCatchTick < 0;
            if (!outcome) FirstDifference("Observed outcome differs from selected natural catch/injury/resource expectation.");
            report.status = string.IsNullOrEmpty(report.firstDifference) ? "PASS_SCOPED_PHYSICAL_CHAIN" : "OBSERVED_DIFFERENCE";
            Close();
        }

        private static EntitySample Capture(LF2Entity entity)
        {
            return new EntitySample { slot = entity.Runtime.SlotIndex, stableId = entity.Runtime.StableId,
                oid = entity.ObjectId, frame = entity.Frame.N, hp = entity.Runtime.HP, pp = entity.Runtime.PP,
                ppMax = entity.Runtime.PPMax, facing = entity.Runtime.Dir,
                x = entity.Runtime.X, y = entity.Runtime.Y, z = entity.Runtime.Z,
                sourceX = entity.Runtime.SourceRuleX, sourceZ = entity.Runtime.SourceRuleZ,
                caughtSlot = entity.Runtime.CaughtSlotIndex, catchSourceSlot = entity.Runtime.CatchSourceSlot90 };
        }

        private static InputSample CaptureInput(FrameInputSet frame, int slot)
        {
            foreach (SimulationPlayerInput input in frame.Players)
                if (input.PlayerSlot == slot)
                    return new InputSample { present = true, buttons = input.Buttons.ToString(),
                        pressed = input.PressedButtons.ToString(), released = input.ReleasedButtons.ToString() };
            return new InputSample();
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
                    report.shutdownStatus = shutdown.Status.ToString();
                    report.shutdownStage = shutdown.CompletedStage.ToString();
                    report.shutdownFailure = shutdown.FailureReason;
                    report.remainingWorldObjects = shutdown.RemainingWorldObjects;
                    report.remainingRuntimeSlots = shutdown.RemainingRuntimeSlots;
                    report.remainingPoolBorrowers = shutdown.RemainingPoolBorrowers;
                    report.lifecycleAfter = driver.LifecycleState.ToString();
                    report.worldDetached = driver.World == null;
                    Require(report.stopped && report.worldDetached && report.remainingWorldObjects == 0 &&
                        report.remainingRuntimeSlots == 0 && report.remainingPoolBorrowers == 0,
                        "Ordered shutdown did not reach zero-residue postconditions.");
                }
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error += "Cleanup: " + error + "\n";
            }
            finally
            {
                try
                {
                    Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
                    if (keyboard != null) InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                }
                catch (Exception error)
                {
                    report.status = "FAIL";
                    report.error += "Keyboard release: " + error + "\n";
                }
                bool hardGateFailed = EditorApplication.isPlaying && driver != null && !report.stopped;
                report.phase = hardGateFailed ? "CLEANUP_BLOCKED" : EditorApplication.isPlaying ? "RELEASING" : "EXITING";
                report.releaseStartedUtc = DateTime.UtcNow.ToString("O");
                SaveSession();
                closing = false;
                EditorApplication.QueuePlayerLoopUpdate();
                if (hardGateFailed)
                {
                    WriteReport(ResultRoot + report.runId + "-shutdown-blocked.json");
                    Debug.LogError("[Q07 Hidan physical Play] Ordered shutdown blocked; Play retained. Evidence in SessionState " + SessionKey);
                }
                EditorApplication.delayCall += () =>
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode && report?.phase == "EXITING") FinishAfterExit();
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
            if (!report.neutralKeyboardObserved)
            {
                report.status = "FAIL";
                report.error += "Neutral keyboard state was not confirmed after Play exit.\n";
            }
            if (!report.sceneCleanAfter || report.sceneHashBefore != report.sceneHashAfter)
            {
                report.status = "FAIL";
                report.error += "Saved Scene hash or clean state changed after Play.\n";
            }
            string path = ResultRoot + report.runId + ".json";
            WriteReport(path);
            Debug.Log("[Q07 Hidan physical Play] " + report.status + ": " + path);
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            actor = target = null;
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

        private static void Require(bool condition, string error)
        {
            if (!condition) throw new InvalidOperationException(error);
        }
    }
}
#endif
