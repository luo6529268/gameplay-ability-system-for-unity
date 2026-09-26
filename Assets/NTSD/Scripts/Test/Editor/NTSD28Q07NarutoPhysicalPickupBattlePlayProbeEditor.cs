#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_NarutoPhysicalPickupBattlePlay.request.json";
        private const string ResultRoot = "artifacts/diagnostics/NTSD28-Q07-NARUTO-PHYSICAL-PICKUP-PLAY-001/";
        private const string SessionKey = "NTSD.Q07.NarutoPhysicalPickupBattlePlay";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character target;
        private static LF2Weapon weapon;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int dynamicUpdates;
        private static int queuedAtUpdate;
        private static int appliedDynamicUpdate;
        private static bool dynamicKeyboardJ, dynamicAttackPressed;
        private static string dynamicAttackControl;
        private static readonly MethodInfo DynamicInputUpdate = typeof(InputSystem).GetMethod(
            "Update", BindingFlags.Static | BindingFlags.NonPublic, null,
            new[] { typeof(InputUpdateType) }, null);
        private static bool closing;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public int weaponX;
        }

        [Serializable]
        private sealed class EntitySample
        {
            public int slot, stableId, oid, frame, state, pic, hp, pp, ppMax, link, childSlot, holderSlot;
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
            public bool keyboardJBeforeTick, attackEnabledBeforeTick, attackMapEnabledBeforeTick, attackPressedBeforeTick;
            public string attackControlBeforeTick, attackControlsBeforeTick, inputUpdateMode;
            public int appliedDynamicUpdate;
            public bool dynamicKeyboardJ, dynamicAttackPressed;
            public string dynamicAttackControl;
            public InputSample p1, p2;
            public EntitySample actor, target, weapon;
            public int worldObjects, runtimeSlots;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId, startedUtc, phase, status, error, firstDifference, releaseStartedUtc;
            public int weaponX;
            public string evidenceScope = "Original Editor Battle Scene; temporary formal ground OID120; physical keyboard events; paused full Driver host steps using production P1 input provider. Not LocalFreeRun cadence or formal root-EXE same-state/GPU parity.";
            public string inputContract = "Serialized Player_1 physical J=Attack action -> canonical Jump; physical K=Jump action -> canonical Defend (existing crossed carrier).";
            public bool bootstrapConfiguredBeforeStart, stopped, worldDetached, exitedPlay, sceneCleanAfter, neutralKeyboardObserved;
            public string contentRoot, sceneHashBefore, sceneHashAfter, lifecycleAfter, shutdownStatus, shutdownStage, shutdownFailure;
            public string sequencePhase = "PICKUP_ATTACK", queuedKeys = "Neutral";
            public bool jumpQueued, airAttackQueued, spriteCatalogFound, centralPlanValid;
            public int airAttackTicks, firstPickupTick = -1, firstStandingTick = -1, firstAirborneTick = -1, firstAction30Tick = -1;
            public int startTick, endTick, initialInputPhase, battleMode, keyboardDeviceId, weaponSlot = -1, publicationTick, matchingCommands;
            public int remainingWorldObjects = -1, remainingRuntimeSlots = -1, remainingPoolBorrowers = -1;
            public EntitySample initialActor, initialTarget, initialWeapon;
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
            {
                dynamicUpdates++;
                if (report?.phase == "INPUT" && dynamicUpdates == appliedDynamicUpdate)
                {
                    Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
                    CharacterInputModule input = actor?.Controller as CharacterInputModule;
                    dynamicKeyboardJ = keyboard?.jKey.isPressed == true;
                    dynamicAttackPressed = input?.AttackAction?.ReadValue<float>() > 0.5f;
                    dynamicAttackControl = input?.AttackAction?.activeControl?.path;
                }
            }
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
                SetBootstrapField(matches[0], "overrideCharacterIds", new[] { 2, 7 });
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
                if (report.phase == "NEUTRAL")
                {
                    if (dynamicUpdates <= queuedAtUpdate) return;
                    InitializeActor(actor, 200);
                    InitializeActor(target, 1200);
                    CreateGroundWeapon();
                    report.initialActor = Capture(actor);
                    report.initialTarget = Capture(target);
                    report.initialWeapon = Capture(weapon);
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
                else Debug.LogError("[Q07 Naruto physical pickup Play] " + error);
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
            report = new Report { runId = request.runId, weaponX = request.weaponX,
                startedUtc = DateTime.UtcNow.ToString("O"), phase = "STARTUP", status = "RUNNING" };
            Require(request.weaponX == 190 || request.weaponX == 800, "weaponX must be 190 or 800.");
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
            Require(actor != target && actor.ObjectId == 2 && target.ObjectId == 7 &&
                !actor.AiControlled && !target.AiControlled, "Expected normal human Naruto and opponent.");
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
            character.Runtime.MP = 500;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            int z = world.Runtime.Stage.ZMin + 50;
            character.Runtime.SetPosition(x, 0, z);
            AppManager.SyncParticipantBirthPosition(character, x, z);
        }

        private static void CreateGroundWeapon()
        {
            var data = world.RuntimeCharacterConfigs.Resolve(120);
            Require(data?.characterData != null, "Formal OID120 weapon DAT is unavailable.");
            report.weaponSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
            Require(report.weaponSlot >= 50, "No free weapon runtime slot.");
            weapon = new LF2Weapon();
            weapon.ObjectId = 120;
            weapon.Name = "Q07NarutoPhysicalPickupWeapon";
            weapon.SetWeaponType(1);
            weapon.FrameCache.Load(data);
            weapon.SetRequiredRuntimeSlot(report.weaponSlot);
            world.Register(weapon);
            weapon.ImmediateFrame(data.characterData.frames.First(
                frame => frame.state == LF2States.WeaponOnGround).frameId);
            weapon.Health.HP = 100;
            weapon.Runtime.SetPosition(report.weaponX, 0, world.Runtime.Stage.ZMin + 50);
            weapon.Runtime.SyncIntegerPosition();
        }

        private static void QueueForNextTick()
        {
            int index = report.samples.Count;
            if (index < 2)
            {
                report.queuedKeys = "J";
                report.sequencePhase = "PICKUP_ATTACK";
            }
            else if (report.weaponX == 190 && report.firstPickupTick >= 0 &&
                     !report.jumpQueued && actor.Frame.N == 0)
            {
                report.jumpQueued = true;
                report.queuedKeys = "K";
                report.sequencePhase = "JUMP";
            }
            else if (report.weaponX == 190 && report.jumpQueued && !report.airAttackQueued &&
                     actor.Frame.D?.state == LF2States.Jump && actor.Runtime.YInt < 0)
            {
                report.airAttackQueued = true;
                report.queuedKeys = "J";
                report.sequencePhase = "AIR_ATTACK";
            }
            else if (report.airAttackQueued && report.airAttackTicks < 2)
            {
                report.queuedKeys = "J";
                report.sequencePhase = "AIR_ATTACK";
            }
            else
            {
                report.queuedKeys = "Neutral";
                report.sequencePhase = report.weaponX == 800 ? "FAR_CONTROL" :
                    report.jumpQueued ? "OBSERVE" : "WAIT_STANDING";
            }
            appliedDynamicUpdate = 0;
            dynamicKeyboardJ = dynamicAttackPressed = false;
            dynamicAttackControl = null;
            EditorApplication.QueuePlayerLoopUpdate();
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
            string keys = report.queuedKeys;
            if (keys == "J") QueueKeys(Key.J);
            else if (keys == "K") QueueKeys(Key.K);
            else QueueKeys();
            Require(DynamicInputUpdate != null, "Typed Dynamic Input System update is unavailable.");
            appliedDynamicUpdate = dynamicUpdates + 1;
            DynamicInputUpdate.Invoke(null, new object[] { InputUpdateType.Dynamic });
            Keyboard beforeKeyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
            CharacterInputModule beforeInput = actor.Controller as CharacterInputModule;
            bool keyboardJBeforeTick = beforeKeyboard?.jKey.isPressed == true;
            bool attackEnabledBeforeTick = beforeInput?.AttackAction?.enabled == true;
            bool attackMapEnabledBeforeTick = beforeInput?.AttackAction?.actionMap?.enabled == true;
            bool attackPressedBeforeTick = beforeInput?.AttackAction?.ReadValue<float>() > 0.5f;
            string attackControlBeforeTick = beforeInput?.AttackAction?.activeControl?.path;
            string attackControlsBeforeTick = beforeInput?.AttackAction == null ? null :
                string.Join(",", beforeInput.AttackAction.controls.Select(control =>
                    control.path + "#" + control.device.deviceId));
            Require(driver.StepOneTick(ignorePaused: true, buildPresentation: true), "Full Driver rejected tick.");
            int tick = driver.CurrentTickIndex;
            Require(tick == report.endTick + 1, "Skipped/unobserved completed logic tick.");
            report.endTick = tick;
            FrameInputSet frame = driver.LastAppliedFrameInput;
            Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
            CharacterInputModule input = actor.Controller as CharacterInputModule;
            var sample = new TickSample { relativeTick = index + 1, tick = tick,
                canonicalTick = frame.TickIndex, inputPhase = world.InputPhase,
                physicalKeys = keys,
                keyboardJPressed = keyboard?.jKey.isPressed == true,
                keyboardKPressed = keyboard?.kKey.isPressed == true,
                attackActionPressed = input?.AttackAction?.ReadValue<float>() > 0.5f,
                jumpActionPressed = input?.JumpAction?.ReadValue<float>() > 0.5f,
                attackActiveControl = input?.AttackAction?.activeControl?.path,
                jumpActiveControl = input?.JumpAction?.activeControl?.path,
                keyboardJBeforeTick = keyboardJBeforeTick,
                attackEnabledBeforeTick = attackEnabledBeforeTick,
                attackMapEnabledBeforeTick = attackMapEnabledBeforeTick,
                attackPressedBeforeTick = attackPressedBeforeTick,
                attackControlBeforeTick = attackControlBeforeTick,
                attackControlsBeforeTick = attackControlsBeforeTick,
                inputUpdateMode = InputSystem.settings.updateMode.ToString(),
                appliedDynamicUpdate = appliedDynamicUpdate,
                dynamicKeyboardJ = dynamicKeyboardJ,
                dynamicAttackPressed = dynamicAttackPressed,
                dynamicAttackControl = dynamicAttackControl,
                p1 = CaptureInput(frame, 0), p2 = CaptureInput(frame, 1),
                actor = Capture(actor), target = Capture(target), weapon = Capture(weapon),
                worldObjects = world.ObjectCount, runtimeSlots = world.ClaimedRuntimeSlotCountForDiagnostics };
            report.samples.Add(sample);
            string previousKeys = index == 0 ? "Neutral" : report.samples[index - 1].physicalKeys;
            SimulationInputButtons expected = Canonical(keys);
            SimulationInputButtons expectedPressed = keys == previousKeys ?
                SimulationInputButtons.None : expected;
            SimulationInputButtons expectedReleased = keys == previousKeys ?
                SimulationInputButtons.None : Canonical(previousKeys);
            if (frame.TickIndex != tick || !sample.p1.present || !sample.p2.present ||
                sample.p1.buttons != expected.ToString() || sample.p1.pressed != expectedPressed.ToString() ||
                sample.p1.released != expectedReleased.ToString() || sample.p2.buttons != SimulationInputButtons.None.ToString() ||
                sample.p2.pressed != SimulationInputButtons.None.ToString() || sample.p2.released != SimulationInputButtons.None.ToString())
                FirstDifference("Physical/canonical input sequence mismatch at relative tick " + (index + 1));
            bool held = actor.GetHeldWeapon() == weapon &&
                actor.Runtime.LinkState % 100 == 1 && weapon.Runtime.LinkState == -1 &&
                actor.Runtime.TargetSlotIndex == weapon.Runtime.SlotIndex &&
                weapon.Runtime.HolderStableId == actor.Runtime.SlotIndex;
            if (report.firstPickupTick < 0 && held) report.firstPickupTick = tick;
            if (report.firstPickupTick >= 0 && report.firstStandingTick < 0 &&
                actor.Frame.N == 0) report.firstStandingTick = tick;
            if (report.jumpQueued && report.firstAirborneTick < 0 &&
                actor.Frame.D?.state == LF2States.Jump && actor.Runtime.YInt < 0)
                report.firstAirborneTick = tick;
            if (report.airAttackQueued && keys == "J") report.airAttackTicks++;
            if (report.firstPickupTick >= 0 && report.firstAction30Tick < 0 &&
                actor.Frame.N == 30 && actor.Frame.D?.pic == 97)
            {
                report.firstAction30Tick = tick;
                CapturePublication();
            }
            SaveSession();
            bool done = !string.IsNullOrEmpty(report.firstDifference) && report.samples.Count >= 4 ||
                (report.weaponX == 190
                ? report.firstAction30Tick >= 0 && tick >= report.firstAction30Tick + 3
                : report.samples.Count >= 30);
            if (!done && report.samples.Count < 50)
            {
                QueueForNextTick();
                return;
            }
            bool outcome = report.weaponX == 190
                ? report.firstPickupTick >= 0 && report.firstStandingTick >= 0 &&
                    report.firstAirborneTick >= 0 && report.firstAction30Tick >= 0 &&
                    report.spriteCatalogFound && report.centralPlanValid && report.matchingCommands == 1
                : report.firstPickupTick < 0 && actor.GetHeldWeapon() != weapon &&
                    actor.Runtime.LinkState % 100 != 1;
            if (!outcome) FirstDifference("Observed outcome differs from natural pickup/held-air or far-control expectation.");
            report.status = string.IsNullOrEmpty(report.firstDifference) ? "PASS_SCOPED_PHYSICAL_CHAIN" : "OBSERVED_DIFFERENCE";
            Close();
        }

        private static SimulationInputButtons Canonical(string keys)
        {
            if (keys == "J") return SimulationInputButtons.Jump;
            if (keys == "K") return SimulationInputButtons.Defend;
            return SimulationInputButtons.None;
        }

        private static void CapturePublication()
        {
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            int visualDataId = LF2Entity.ResolveCurrentDataObjectId(actor);
            int effectivePic = actor.GetRenderPicIndex();
            report.spriteCatalogFound = manager != null &&
                manager.TryGetSpriteEntry(visualDataId, effectivePic, out BattleSpriteEntry entry) &&
                entry != null && entry.CentralBinding.IsValid;
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
            report.publicationTick = plan.SimulationTick;
            report.centralPlanValid = plan.IsValid && !plan.IsStale &&
                plan.Owner == BattlePixelFrameOwner.Central &&
                plan.SimulationTick == driver.CurrentTickIndex &&
                plan.CapturedFrame?.CommandsMaterialized == true;
            if (!report.centralPlanValid) return;
            BattlePresentationFrame presentation = plan.CapturedFrame;
            for (int commandIndex = 0; commandIndex < presentation.CommandCount; commandIndex++)
            {
                BattleRenderCommand command = presentation.GetCommand(commandIndex);
                if (command.Type == BattleRenderCommandType.Entity &&
                    command.StableId == actor.Runtime.StableId &&
                    command.VisualDataId == 2 && command.EffectivePic == 97)
                    report.matchingCommands++;
            }
        }

        private static EntitySample Capture(LF2Entity entity)
        {
            if (entity == null) return null;
            return new EntitySample { slot = entity.Runtime.SlotIndex, stableId = entity.Runtime.StableId,
                oid = entity.ObjectId, frame = entity.Frame.N,
                state = entity.Frame.D == null ? -1 : (int)entity.Frame.D.state,
                pic = entity.Frame.D == null ? -1 : entity.Frame.D.pic,
                hp = entity.Runtime.HP, pp = entity.Runtime.PP,
                ppMax = entity.Runtime.PPMax, facing = entity.Runtime.Dir,
                x = entity.Runtime.X, y = entity.Runtime.Y, z = entity.Runtime.Z,
                sourceX = entity.Runtime.SourceRuleX, sourceZ = entity.Runtime.SourceRuleZ,
                link = entity.Runtime.LinkState, childSlot = entity.Runtime.TargetSlotIndex,
                holderSlot = entity.Runtime.HolderStableId };
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
                    Debug.LogError("[Q07 Naruto physical pickup Play] Ordered shutdown blocked; Play retained. Evidence in SessionState " + SessionKey);
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
            Debug.Log("[Q07 Naruto physical pickup Play] " + report.status + ": " + path);
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
