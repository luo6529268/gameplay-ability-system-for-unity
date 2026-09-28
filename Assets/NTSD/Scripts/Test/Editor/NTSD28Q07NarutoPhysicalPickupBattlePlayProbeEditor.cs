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
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07NarutoPhysicalPickupBattlePlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_NarutoPhysicalPickupBattlePlay.request.json";
        private const string ResultRoot = "artifacts/diagnostics/NTSD28-Q07-NARUTO-PHYSICAL-PICKUP-PLAY-001/";
        private const string HeldCommandRequestPath = "Temp/NTSD28_Q09_P02_HeldCommand.request.json";
        private const string HeldCommandResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P02-NATURAL-HELD-COMMAND-001/";
        private const string HeldPixelResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P02-NATURAL-HELD-PIXEL-001/";
        private const string HeldUnsaturatedResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P02-HELD-UNSATURATED-PHASE-001/";
        private const string HeldAttributionResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P02-HELD-TARGET-PIXEL-ATTRIBUTION-001/";
        private const int PixelCaptureWidth = 960;
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexArrayId = Shader.PropertyToID("_MainTexArray");
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
            public bool captureHeldCommand;
            public bool captureHeldPixels;
            public bool captureHeldUnsaturated;
            public bool captureHeldAttribution;
        }

        [Serializable]
        private sealed class PixelEvidence
        {
            public string imagePath, checksum;
            public int tick, generation, commandCount, weaponCommandCount;
            public int publishedTick, previousMotionTick, clockBeginCallbackCount;
            public int x, y, width, height, targetNonWhite, exclusiveArea, exclusiveNonWhite;
            public int submissionCountBefore, submissionCountAfter;
            public double alpha, alphaBeforeCamera;
            public float commandX, commandY;
        }

        private sealed class PixelCapture
        {
            public PixelEvidence Evidence;
            public Color32[] Pixels;
            public RectInt WeaponBounds;
            public List<RectInt> OtherBounds;
            public AttributionEvidence Attribution;
            public bool[] AttributedMask;
        }

        [Serializable]
        private sealed class AttributionEvidence
        {
            public string allImagePath, withoutImagePath, checksum;
            public int commandCount, withoutCommandCount;
            public int resolvedCount, withoutResolvedCount, targetPixels;
            public int x, y, width, height;
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
            public bool captureHeldCommand, heldResourceResolved, heldSubmitted, worldCameraActive;
            public bool captureHeldPixels, pixelSourceBirthInitialized;
            public bool captureHeldUnsaturated;
            public PixelEvidence earlyPixel, laterPixel;
            public int changedExclusivePixels;
            public bool captureHeldAttribution;
            public AttributionEvidence earlyAttribution, laterAttribution;
            public int changedAttributedPixels;
            public bool focusPolicyAdjusted, focusPolicyRestored;
            public int originalBackgroundBehavior, originalEditorInputBehavior;
            public int airAttackTicks, firstPickupTick = -1, firstStandingTick = -1, firstAirborneTick = -1, firstAction30Tick = -1;
            public int startTick, endTick, initialInputPhase, battleMode, keyboardDeviceId, weaponSlot = -1, publicationTick, matchingCommands;
            public int heldCommandCount, heldCommandIndex = -1, heldSegmentIndex = -1;
            public int cameraSubmissionCountBefore, cameraSubmissionCountAfter;
            public int heldEffectivePic = -1, heldVisualDataId = -1;
            public float heldCommandX, heldCommandY;
            public string heldReasonBefore, heldReasonAfter, checksumBeforeCamera, checksumAfterCamera;
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
                    RestoreFocusPolicy();
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
            string requestPath = HeldCommandRequestPath;
            Request request = File.Exists(requestPath)
                ? JsonUtility.FromJson<Request>(File.ReadAllText(requestPath))
                : null;
            if (request?.requested != true)
            {
                requestPath = RequestPath;
                if (!File.Exists(requestPath)) return;
                request = JsonUtility.FromJson<Request>(File.ReadAllText(requestPath));
            }
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(requestPath, JsonUtility.ToJson(request, true));
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 100 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require((requestPath == HeldCommandRequestPath) == request.captureHeldCommand,
                "Held-command request must use its dedicated request path.");
            Require(!request.captureHeldPixels || request.captureHeldCommand,
                "Held-pixel witness requires the held-command request.");
            Require(!request.captureHeldUnsaturated ||
                    request.captureHeldPixels && request.captureHeldCommand,
                "Unsaturated witness requires held-pixel and held-command flags.");
            Require(!request.captureHeldAttribution ||
                    request.captureHeldUnsaturated && request.captureHeldPixels &&
                    request.captureHeldCommand,
                "Attribution witness requires unsaturated held-pixel flags.");
            string resultRoot = request.captureHeldAttribution
                ? HeldAttributionResultRoot : request.captureHeldUnsaturated
                ? HeldUnsaturatedResultRoot : request.captureHeldPixels ? HeldPixelResultRoot :
                request.captureHeldCommand ? HeldCommandResultRoot : ResultRoot;
            Require(!File.Exists(resultRoot + request.runId + ".json"), "Refusing to overwrite runId.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Request must begin in Edit Mode.");
            report = new Report { runId = request.runId, weaponX = request.weaponX,
                captureHeldCommand = request.captureHeldCommand,
                captureHeldPixels = request.captureHeldPixels,
                captureHeldUnsaturated = request.captureHeldUnsaturated,
                captureHeldAttribution = request.captureHeldAttribution,
                startedUtc = DateTime.UtcNow.ToString("O"), phase = "STARTUP", status = "RUNNING" };
            Require(request.weaponX == 190 || request.weaponX == 800, "weaponX must be 190 or 800.");
            Require(!request.captureHeldCommand || request.weaponX == 190,
                "Held-command witness requires the natural near pickup case.");
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
            if (report.captureHeldCommand)
            {
                InputSettings settings = InputSystem.settings;
                report.originalBackgroundBehavior = (int)settings.backgroundBehavior;
                report.originalEditorInputBehavior = (int)settings.editorInputBehaviorInPlayMode;
                report.focusPolicyAdjusted = true;
                SaveSession();
                settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                settings.editorInputBehaviorInPlayMode =
                    InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            }
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
            int z = world.Runtime.Stage.ZMin + 50;
            weapon.Runtime.SetPosition(report.weaponX, 0, z);
            weapon.Runtime.SyncIntegerPosition();
            if (report.captureHeldPixels)
            {
                weapon.Runtime.SetSourceRulePosition(report.weaponX, z);
                weapon.Runtime.SyncSourceRuleIntegerPosition();
                report.pixelSourceBirthInitialized = weapon.Runtime.SourceRulePositionInitialized;
            }
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
            if (report.captureHeldCommand)
                CaptureHeldCommand(presentation);
        }

        private static void CaptureHeldCommand(BattlePresentationFrame presentation)
        {
            Require(ReferenceEquals(actor.GetHeldWeapon(), weapon),
                "The weapon was not held at the central publication tick.");
            for (int index = 0; index < presentation.CommandCount; index++)
            {
                BattleRenderCommand command = presentation.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Entity ||
                    command.RuntimeSlot != report.weaponSlot)
                    continue;
                report.heldCommandCount++;
                report.heldCommandIndex = index;
                report.heldCommandX = command.Position.x;
                report.heldCommandY = command.Position.y;
            }
            BattleCentralEntityDiagnostic before =
                BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(
                    world, report.weaponSlot);
            report.heldReasonBefore = before.Reason.ToString();
            report.heldResourceResolved = before.HasResolvedResource;
            report.heldSegmentIndex = before.SegmentIndex;
            report.heldEffectivePic = before.EffectivePic;
            report.heldVisualDataId = before.CurrentDatObjectId;
            Require(report.heldCommandCount == 1 && before.HasSnapshot &&
                    before.HasCommand && before.HasResolvedResource &&
                    before.CommandIndex == report.heldCommandIndex &&
                    before.SegmentIndex >= 0 && before.ObjectId == 120 &&
                    before.CurrentDatObjectId == LF2Entity.ResolveCurrentDataObjectId(weapon) &&
                    before.EffectivePic == weapon.GetRenderPicIndex(),
                "Held weapon central command/resource mismatch: " + report.heldReasonBefore);

            Camera camera = NTSDRenderSpace.WorldCamera;
            report.worldCameraActive = camera != null && camera.enabled &&
                camera.gameObject.activeInHierarchy;
            Require(report.worldCameraActive, "The active World camera is unavailable.");
            int tick = driver.CurrentTickIndex;
            report.checksumBeforeCamera = world.CaptureParityFrameSnapshot(tick).OverallChecksum;
            report.cameraSubmissionCountBefore = BattleCentralRenderSystem.Diagnostics.SubmissionCount;
            BattleCentralEntityDiagnostic capturedAfter = default;
            if (report.captureHeldPixels)
            {
                Require(report.pixelSourceBirthInitialized &&
                        weapon.Runtime.SourceRulePositionInitialized,
                    "The held weapon source-rule position was not initialized.");
                if (report.captureHeldUnsaturated)
                {
                    using (var clock = new ControlledDisplayClock(world))
                    {
                        clock.SetFraction(0.20);
                        PixelCapture early = CaptureHeldPixelFrame(
                            camera, "early", clock, 0.20);
                        report.earlyPixel = early.Evidence;
                        clock.SetFraction(0.70);
                        PixelCapture later = CaptureHeldPixelFrame(
                            camera, "later", clock, 0.70);
                        report.laterPixel = later.Evidence;
                        report.changedExclusivePixels =
                            CountChangedExclusivePixels(early, later);
                        if (report.captureHeldAttribution)
                        {
                            report.earlyAttribution = early.Attribution;
                            report.laterAttribution = later.Attribution;
                            report.changedAttributedPixels =
                                CountChangedAttributedPixels(early, later);
                        }
                        report.cameraSubmissionCountAfter =
                            BattleCentralRenderSystem.Diagnostics.SubmissionCount;
                        capturedAfter = BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(
                            world, report.weaponSlot);
                    }
                    Require(report.earlyPixel.alpha > 0.0 &&
                            report.earlyPixel.alpha < report.laterPixel.alpha &&
                            report.laterPixel.alpha < 1.0 &&
                            report.earlyPixel.commandX != report.laterPixel.commandX,
                        "Controlled held-weapon display phases were not both unsaturated or command X was static.");
                    if (report.captureHeldAttribution)
                        Require(report.earlyAttribution.targetPixels > 0 &&
                                report.laterAttribution.targetPixels > 0 &&
                                report.changedAttributedPixels > 0,
                            "The held weapon central GPU contribution did not change between display phases.");
                }
                else
                {
                    PixelCapture early = CaptureHeldPixelFrame(camera, "early");
                    report.earlyPixel = early.Evidence;
                    BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
                    PixelCapture later = CaptureHeldPixelFrame(camera, "later");
                    report.laterPixel = later.Evidence;
                    report.changedExclusivePixels = CountChangedExclusivePixels(early, later);
                    Require(later.Evidence.alpha > early.Evidence.alpha &&
                            later.Evidence.commandX != early.Evidence.commandX &&
                            (early.Evidence.exclusiveNonWhite > 0 ||
                             later.Evidence.exclusiveNonWhite > 0) &&
                            report.changedExclusivePixels > 0,
                        "Held weapon camera pixels did not show attributable display motion.");
                }
            }
            else
            {
                camera.Render();
            }
            if (!report.captureHeldUnsaturated)
            {
                report.cameraSubmissionCountAfter =
                    BattleCentralRenderSystem.Diagnostics.SubmissionCount;
                capturedAfter = BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(
                    world, report.weaponSlot);
            }
            BattleCentralEntityDiagnostic after = capturedAfter;
            report.heldReasonAfter = after.Reason.ToString();
            report.heldSubmitted = after.Submitted;
            report.checksumAfterCamera = world.CaptureParityFrameSnapshot(tick).OverallChecksum;
            Require(driver.CurrentTickIndex == tick &&
                    world.CurrentPixelFramePlan.DisplayTick == tick &&
                    report.cameraSubmissionCountAfter > report.cameraSubmissionCountBefore &&
                    after.Handle == before.Handle && after.HasCommand &&
                    after.HasResolvedResource && after.Submitted &&
                    after.Reason == BattleCentralEntityDiagnosticReason.None &&
                    report.checksumAfterCamera == report.checksumBeforeCamera,
                "Held weapon camera submission/checksum mismatch: " + report.heldReasonAfter);
        }

        private static PixelCapture CaptureHeldPixelFrame(Camera camera, string phase,
            ControlledDisplayClock controlledClock = null,
            double controlledFraction = 0.0)
        {
            int tick = driver.CurrentTickIndex;
            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            Require(plan.IsValid && plan.Owner == BattlePixelFrameOwner.Central &&
                    plan.DisplayTick == tick && plan.CapturedFrame?.CommandsMaterialized == true,
                "Central pixel plan is unavailable at " + phase);
            int height = Mathf.Max(1, Mathf.RoundToInt(PixelCaptureWidth /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            var targetTexture = new RenderTexture(PixelCaptureWidth, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            RenderTexture previousActive = RenderTexture.active;
            CameraState savedCamera = new CameraState(camera);
            Texture2D readback = null;
            Action<ScriptableRenderContext, Camera> beginCamera = null;
            int clockBeginCallbackCount = 0;
            try
            {
                targetTexture.Create();
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.white;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = targetTexture;
                int submissionBefore = BattleCentralRenderSystem.Diagnostics.SubmissionCount;
                string checksumBefore = world.CaptureParityFrameSnapshot(tick).OverallChecksum;
                double alphaBeforeCamera =
                    BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world);
                if (controlledClock != null)
                {
                    beginCamera = (context, renderingCamera) =>
                    {
                        if (!ReferenceEquals(renderingCamera, camera)) return;
                        clockBeginCallbackCount++;
                        controlledClock.SetStartOnly(controlledFraction);
                    };
                    RenderPipelineManager.beginCameraRendering += beginCamera;
                }
                camera.Render();
                if (controlledClock != null)
                    Require(clockBeginCallbackCount > 0,
                        "The World camera begin-render clock callback did not run at " + phase);
                int submissionAfter = BattleCentralRenderSystem.Diagnostics.SubmissionCount;
                BattlePixelFramePlan renderedPlan = world.CurrentPixelFramePlan;
                Require(renderedPlan.IsValid &&
                        ReferenceEquals(renderedPlan.World, plan.World) &&
                        renderedPlan.Owner == BattlePixelFrameOwner.Central &&
                        renderedPlan.DisplayTick == tick &&
                        renderedPlan.CapturedFrame?.CommandsMaterialized == true &&
                        submissionAfter > submissionBefore,
                    "World camera did not render the frozen central frame at " + phase);
                double alpha = BattleCentralRenderSystem.LastResolvedDisplayAlphaForWorld(world);
                BattlePresentationFrame frame = renderedPlan.CapturedFrame;
                BattleRenderCommand weaponCommand = default;
                int matching = 0;
                int weaponCommandIndex = -1;
                var otherBounds = new List<RectInt>(frame.CommandCount);
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    if (command.Type == BattleRenderCommandType.Entity &&
                        command.RuntimeSlot == report.weaponSlot &&
                        command.StableId == weapon.Runtime.StableId)
                    {
                        matching++;
                        weaponCommand = command;
                        weaponCommandIndex = index;
                    }
                    else
                    {
                        RectInt bounds = ProjectCommandBounds(camera, command,
                            PixelCaptureWidth, height);
                        if (bounds.width > 0 && bounds.height > 0)
                            otherBounds.Add(bounds);
                    }
                }
                Require(matching == 1, "Weapon pixel command count changed at " + phase);
                BattleCentralEntityDiagnostic diagnostic =
                    BattleCentralRenderSystem.CaptureEntityDiagnosticBySlot(
                        world, report.weaponSlot);
                Require(diagnostic.HasResolvedResource && diagnostic.Submitted &&
                        diagnostic.Reason == BattleCentralEntityDiagnosticReason.None,
                    "Weapon pixel resource/submission refused at " + phase +
                    ": " + diagnostic.Reason);
                RectInt weaponBounds = ProjectCommandBounds(camera, weaponCommand,
                    PixelCaptureWidth, height);
                Require(weaponBounds.width > 0 && weaponBounds.height > 0,
                    "Weapon projected outside the camera at " + phase);
                RenderTexture.active = targetTexture;
                readback = new Texture2D(PixelCaptureWidth, height,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0f, 0f, PixelCaptureWidth, height),
                    0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                var evidence = new PixelEvidence
                {
                    tick = tick,
                    generation = renderedPlan.Generation,
                    publishedTick = frame.TickIndex,
                    previousMotionTick = frame.PreviousMotionTickIndex,
                    clockBeginCallbackCount = clockBeginCallbackCount,
                    commandCount = frame.CommandCount,
                    weaponCommandCount = matching,
                    alpha = alpha,
                    alphaBeforeCamera = alphaBeforeCamera,
                    commandX = weaponCommand.Position.x,
                    commandY = weaponCommand.Position.y,
                    x = weaponBounds.x,
                    y = weaponBounds.y,
                    width = weaponBounds.width,
                    height = weaponBounds.height,
                    submissionCountBefore = submissionBefore,
                    submissionCountAfter = submissionAfter,
                    checksum = checksumBefore,
                };
                for (int y = weaponBounds.yMin; y < weaponBounds.yMax; y++)
                for (int x = weaponBounds.xMin; x < weaponBounds.xMax; x++)
                {
                    bool nonWhite = IsNonWhite(pixels[y * PixelCaptureWidth + x]);
                    if (nonWhite) evidence.targetNonWhite++;
                    if (OverlapsAny(otherBounds, x, y)) continue;
                    evidence.exclusiveArea++;
                    if (nonWhite) evidence.exclusiveNonWhite++;
                }
                AttributionEvidence attribution = null;
                bool[] attributedMask = null;
                if (report.captureHeldAttribution)
                    attribution = CaptureTargetAttribution(camera, frame,
                        weaponCommandIndex, phase, height, checksumBefore,
                        out attributedMask);
                string imagePath = GetResultRoot() + report.runId + "-" + phase + ".png";
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                using (var stream = new FileStream(imagePath, FileMode.CreateNew,
                    FileAccess.Write))
                {
                    byte[] png = readback.EncodeToPNG();
                    stream.Write(png, 0, png.Length);
                }
                evidence.imagePath = imagePath;
                Require(world.CaptureParityFrameSnapshot(tick).OverallChecksum ==
                        checksumBefore && driver.CurrentTickIndex == tick,
                    "Camera pixel readback changed the frozen World at " + phase);
                return new PixelCapture
                {
                    Evidence = evidence,
                    Pixels = pixels,
                    WeaponBounds = weaponBounds,
                    OtherBounds = otherBounds,
                    Attribution = attribution,
                    AttributedMask = attributedMask,
                };
            }
            finally
            {
                if (beginCamera != null)
                    RenderPipelineManager.beginCameraRendering -= beginCamera;
                RenderTexture.active = previousActive;
                savedCamera.Restore(camera);
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                targetTexture.Release();
                UnityEngine.Object.DestroyImmediate(targetTexture);
            }
        }

        private static AttributionEvidence CaptureTargetAttribution(Camera camera,
            BattlePresentationFrame source, int targetIndex, string phase,
            int height, string expectedChecksum, out bool[] attributedMask)
        {
            Require(source != null && source.CommandsMaterialized &&
                    targetIndex >= 0 && targetIndex < source.CommandCount,
                "The frozen held-weapon command frame is unavailable.");
            MethodInfo addCommand = typeof(BattlePresentationFrame).GetMethod(
                "AddCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(addCommand != null, "The temporary command writer is unavailable.");
            var all = new BattlePresentationFrame();
            var withoutTarget = new BattlePresentationFrame();
            for (int index = 0; index < source.CommandCount; index++)
            {
                BattleRenderCommand command = source.GetCommand(index);
                addCommand.Invoke(all, new object[] { command });
                if (index != targetIndex)
                    addCommand.Invoke(withoutTarget, new object[] { command });
            }
            Require(all.CommandCount == source.CommandCount &&
                    withoutTarget.CommandCount == all.CommandCount - 1,
                "The temporary frames did not remove exactly one weapon command.");

            Material material = BattleCentralRenderSystem.RegisteredFeatureMaterialForAcceptance;
            Material arrayMaterial =
                BattleCentralRenderSystem.RegisteredFeatureArrayMaterialForAcceptance;
            Require(material != null && arrayMaterial != null,
                "The production central materials are unavailable.");
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.Configure(source.BoundCatalogForAcceptance,
                source.CommonVisualCatalog, material, arrayMaterial);
            FieldInfo drawModeField = typeof(BattleCentralRenderSystem).GetField(
                "drawMode", BindingFlags.Static | BindingFlags.NonPublic);
            Require(drawModeField != null, "The production central draw mode is unavailable.");
            var drawMode = (BattleCentralDrawMode)drawModeField.GetValue(null);
            using var allBackend = new BattleDynamicMeshBackend();
            using var withoutBackend = new BattleDynamicMeshBackend();
            allBackend.Build(all, resolver, drawMode);
            withoutBackend.Build(withoutTarget, resolver, drawMode);
            var evidence = new AttributionEvidence
            {
                checksum = expectedChecksum,
                commandCount = all.CommandCount,
                withoutCommandCount = withoutTarget.CommandCount,
                resolvedCount = allBackend.Diagnostics.ResolvedCommandCount,
                withoutResolvedCount = withoutBackend.Diagnostics.ResolvedCommandCount,
            };
            Require(evidence.resolvedCount == evidence.withoutResolvedCount + 1 &&
                    allBackend.SegmentCount > 0 && withoutBackend.SegmentCount > 0,
                "The removed held-weapon command did not resolve to exactly one central quad.");
            Color32[] allPixels = RenderTemporaryHeldBackend(camera, allBackend,
                phase + "-all-central", height, out evidence.allImagePath);
            Color32[] withoutPixels = RenderTemporaryHeldBackend(camera, withoutBackend,
                phase + "-without-weapon", height, out evidence.withoutImagePath);
            Require(allPixels.Length == withoutPixels.Length &&
                    allPixels.Length == PixelCaptureWidth * height,
                "Target-attribution GPU captures have incompatible sizes.");
            attributedMask = new bool[allPixels.Length];
            int minX = PixelCaptureWidth, minY = height, maxX = -1, maxY = -1;
            for (int index = 0; index < allPixels.Length; index++)
            {
                if (!PixelsDiffer(allPixels[index], withoutPixels[index])) continue;
                attributedMask[index] = true;
                evidence.targetPixels++;
                int x = index % PixelCaptureWidth;
                int y = index / PixelCaptureWidth;
                minX = Math.Min(minX, x);
                minY = Math.Min(minY, y);
                maxX = Math.Max(maxX, x);
                maxY = Math.Max(maxY, y);
            }
            if (evidence.targetPixels > 0)
            {
                evidence.x = minX;
                evidence.y = minY;
                evidence.width = maxX - minX + 1;
                evidence.height = maxY - minY + 1;
            }
            Require(driver.CurrentTickIndex == source.TickIndex &&
                    world.CaptureParityFrameSnapshot(source.TickIndex).OverallChecksum ==
                    expectedChecksum,
                "The temporary central GPU comparison changed the combat World.");
            return evidence;
        }

        private static Color32[] RenderTemporaryHeldBackend(Camera camera,
            BattleDynamicMeshBackend backend, string imageName, int height,
            out string imagePath)
        {
            var targetTexture = new RenderTexture(PixelCaptureWidth, height, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var commands = new CommandBuffer { name = "Q09 Held Target Difference " + imageName };
            RenderTexture previousActive = RenderTexture.active;
            Texture2D readback = null;
            imagePath = GetResultRoot() + report.runId + "-" + imageName + ".png";
            try
            {
                targetTexture.Create();
                commands.SetRenderTarget(targetTexture);
                commands.SetViewport(new Rect(0, 0, PixelCaptureWidth, height));
                commands.ClearRenderTarget(false, true, Color.white);
                commands.SetViewProjectionMatrices(camera.worldToCameraMatrix,
                    GL.GetGPUProjectionMatrix(camera.projectionMatrix, true));
                var properties = new MaterialPropertyBlock();
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    Require(segment.Material != null && segment.Texture != null,
                        "A held-target central segment has no material or texture.");
                    properties.Clear();
                    properties.SetTexture(
                        segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray
                            ? MainTexArrayId : MainTexId,
                        segment.Texture);
                    commands.DrawMesh(backend.GetChunkMesh(segment.ChunkIndex),
                        Matrix4x4.identity, segment.Material,
                        segment.SubMeshIndex, 0, properties);
                }
                Graphics.ExecuteCommandBuffer(commands);
                RenderTexture.active = targetTexture;
                readback = new Texture2D(PixelCaptureWidth, height,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0, 0, PixelCaptureWidth, height),
                    0, 0, false);
                readback.Apply(false, false);
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));
                using (var stream = new FileStream(imagePath, FileMode.CreateNew,
                    FileAccess.Write))
                {
                    byte[] png = readback.EncodeToPNG();
                    stream.Write(png, 0, png.Length);
                }
                return readback.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previousActive;
                commands.Release();
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                targetTexture.Release();
                UnityEngine.Object.DestroyImmediate(targetTexture);
            }
        }

        private static bool PixelsDiffer(Color32 first, Color32 second) =>
            Math.Abs(first.r - second.r) > 2 ||
            Math.Abs(first.g - second.g) > 2 ||
            Math.Abs(first.b - second.b) > 2 ||
            Math.Abs(first.a - second.a) > 2;

        private static int CountChangedAttributedPixels(PixelCapture early,
            PixelCapture later)
        {
            Require(early.AttributedMask != null && later.AttributedMask != null &&
                    early.AttributedMask.Length == later.AttributedMask.Length,
                "The two weapon attribution masks are unavailable.");
            int changed = 0;
            for (int index = 0; index < early.AttributedMask.Length; index++)
                if (early.AttributedMask[index] != later.AttributedMask[index]) changed++;
            return changed;
        }

        private static int CountChangedExclusivePixels(PixelCapture early,
            PixelCapture later)
        {
            int left = Math.Min(early.WeaponBounds.xMin, later.WeaponBounds.xMin);
            int right = Math.Max(early.WeaponBounds.xMax, later.WeaponBounds.xMax);
            int bottom = Math.Min(early.WeaponBounds.yMin, later.WeaponBounds.yMin);
            int top = Math.Max(early.WeaponBounds.yMax, later.WeaponBounds.yMax);
            int changed = 0;
            for (int y = bottom; y < top; y++)
            for (int x = left; x < right; x++)
            {
                if (OverlapsAny(early.OtherBounds, x, y) ||
                    OverlapsAny(later.OtherBounds, x, y))
                    continue;
                Color32 first = early.Pixels[y * PixelCaptureWidth + x];
                Color32 second = later.Pixels[y * PixelCaptureWidth + x];
                if (first.r != second.r || first.g != second.g ||
                    first.b != second.b || first.a != second.a)
                    changed++;
            }
            return changed;
        }

        private static bool IsNonWhite(Color32 pixel) =>
            pixel.r < 250 || pixel.g < 250 || pixel.b < 250;

        private static bool OverlapsAny(List<RectInt> bounds, int x, int y)
        {
            foreach (RectInt rect in bounds)
            {
                if (x >= rect.xMin && x < rect.xMax &&
                    y >= rect.yMin && y < rect.yMax)
                    return true;
            }
            return false;
        }

        private static RectInt ProjectCommandBounds(Camera camera,
            BattleRenderCommand command, int width, int height)
        {
            float worldWidth = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                NTSDRenderSpace.BattleVisualScale;
            float worldHeight = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - command.Pivot.x * worldWidth;
            float bottom = command.Position.y - command.Pivot.y * worldHeight;
            Vector3 lower = camera.WorldToViewportPoint(
                new Vector3(left, bottom, command.Position.z));
            Vector3 upper = camera.WorldToViewportPoint(new Vector3(
                left + worldWidth, bottom + worldHeight, command.Position.z));
            int x0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.x, upper.x) * width), 0, width);
            int x1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.x, upper.x) * width), 0, width);
            int y0 = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(lower.y, upper.y) * height), 0, height);
            int y1 = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(lower.y, upper.y) * height), 0, height);
            return new RectInt(x0, y0, x1 - x0, y1 - y0);
        }

        private readonly struct CameraState
        {
            private readonly int cullingMask;
            private readonly CameraClearFlags clearFlags;
            private readonly Color backgroundColor;
            private readonly bool allowHdr;
            private readonly bool allowMsaa;
            private readonly RenderTexture targetTexture;

            public CameraState(Camera camera)
            {
                cullingMask = camera.cullingMask;
                clearFlags = camera.clearFlags;
                backgroundColor = camera.backgroundColor;
                allowHdr = camera.allowHDR;
                allowMsaa = camera.allowMSAA;
                targetTexture = camera.targetTexture;
            }

            public void Restore(Camera camera)
            {
                camera.targetTexture = targetTexture;
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = backgroundColor;
                camera.allowHDR = allowHdr;
                camera.allowMSAA = allowMsaa;
            }
        }

        private sealed class ControlledDisplayClock : IDisposable
        {
            private const BindingFlags StaticPrivate = BindingFlags.Static |
                BindingFlags.NonPublic;
            private readonly SimulationWorld targetWorld;
            private readonly FieldInfo clockWorldField;
            private readonly FieldInfo clockVersionField;
            private readonly FieldInfo clockStartField;
            private readonly object previousWorld;
            private readonly object previousVersion;
            private readonly object previousStart;
            private readonly int publicationVersion;

            public ControlledDisplayClock(SimulationWorld world)
            {
                targetWorld = world;
                Type type = typeof(BattleCentralRenderSystem);
                clockWorldField = type.GetField("displayClockWorld", StaticPrivate);
                clockVersionField = type.GetField("displayClockPublicationVersion", StaticPrivate);
                clockStartField = type.GetField("displayClockStartedAt", StaticPrivate);
                FieldInfo pendingField = type.GetField("pendingPublicationVersion", StaticPrivate);
                Require(clockWorldField != null && clockVersionField != null &&
                        clockStartField != null && pendingField != null,
                    "Central display clock fields are unavailable.");
                previousWorld = clockWorldField.GetValue(null);
                previousVersion = clockVersionField.GetValue(null);
                previousStart = clockStartField.GetValue(null);
                publicationVersion = (int)pendingField.GetValue(null);
                Require(ReferenceEquals(previousWorld, world) &&
                        (int)previousVersion == publicationVersion &&
                        world.BattlePresentationLogicIntervalSeconds > 0.0,
                    "The current published-frame display clock is unavailable.");
            }

            public void SetFraction(double fraction)
            {
                SetStartOnly(fraction);
                BattleCentralRenderSystem.FlushLatestPublishedFrame(targetWorld);
            }

            public void SetStartOnly(double fraction)
            {
                clockWorldField.SetValue(null, targetWorld);
                clockVersionField.SetValue(null, publicationVersion);
                clockStartField.SetValue(null,
                    Time.realtimeSinceStartupAsDouble -
                    fraction * targetWorld.BattlePresentationLogicIntervalSeconds);
            }

            public void Dispose()
            {
                clockWorldField.SetValue(null, previousWorld);
                clockVersionField.SetValue(null, previousVersion);
                clockStartField.SetValue(null, previousStart);
                BattleCentralRenderSystem.FlushLatestPublishedFrame(targetWorld);
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

        private static void RestoreFocusPolicy()
        {
            if (report?.focusPolicyAdjusted != true || report.focusPolicyRestored)
                return;
            InputSettings settings = InputSystem.settings;
            settings.editorInputBehaviorInPlayMode =
                (InputSettings.EditorInputBehaviorInPlayMode)report.originalEditorInputBehavior;
            settings.backgroundBehavior =
                (InputSettings.BackgroundBehavior)report.originalBackgroundBehavior;
            report.focusPolicyRestored =
                (int)settings.editorInputBehaviorInPlayMode == report.originalEditorInputBehavior &&
                (int)settings.backgroundBehavior == report.originalBackgroundBehavior;
            Require(report.focusPolicyRestored, "Input focus policy was not restored.");
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
                if (hardGateFailed)
                    RestoreFocusPolicy();
                report.phase = hardGateFailed ? "CLEANUP_BLOCKED" : EditorApplication.isPlaying ? "RELEASING" : "EXITING";
                report.releaseStartedUtc = DateTime.UtcNow.ToString("O");
                SaveSession();
                closing = false;
                EditorApplication.QueuePlayerLoopUpdate();
                if (hardGateFailed)
                {
                    WriteReport(GetResultRoot() + report.runId + "-shutdown-blocked.json");
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
            string path = GetResultRoot() + report.runId + ".json";
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

        private static string GetResultRoot() =>
            report?.captureHeldAttribution == true ? HeldAttributionResultRoot :
            report?.captureHeldUnsaturated == true ? HeldUnsaturatedResultRoot :
            report?.captureHeldPixels == true ? HeldPixelResultRoot :
            report?.captureHeldCommand == true ? HeldCommandResultRoot : ResultRoot;

        private static void WriteReport(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
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
