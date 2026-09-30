#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Animation.LF2Objects;
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
    internal static class NTSD28Q07HidanPhysicalBattlePlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q07_HidanPhysicalBattlePlay.request.json";
        private const string Frame430RequestPath =
            "Temp/NTSD28_Q09_P20_HidanFrame430BattlePlay.request.json";
        private const string MissingBodyRequestPath =
            "Temp/NTSD28_Q09_P20_HidanMissingBodyBattlePlay.request.json";
        private const string MissingBodyContentRoot =
            "Temp/NTSD28Q09P20MissingHid6UnityRoot20260929";
        private const string GameConfigAssetPath =
            "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ResultRoot = "artifacts/diagnostics/NTSD28-Q07-HIDAN-PHYSICAL-BATTLE-PLAY-001/";
        private const string Frame430ResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-BATTLE-COMMAND-001/";
        private const string Frame430PixelResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-SCENE-PIXEL-001/";
        private const string Frame430GameViewResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P20-HIDAN-GAMEVIEW-001/";
        private const string MissingBodyResultRoot =
            "artifacts/diagnostics/NTSD28-Q09-P20-MISSING-SHEET-ROOT-20260929/";
        private const int PixelWidth = 1333;
        private const int PixelHeight = 730;
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexArrayId = Shader.PropertyToID("_MainTexArray");
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
            public bool frame430;
            public bool captureFrame430Pixels;
            public bool captureComposedGameView;
            public bool missingBodySheet;
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
            public long synchronizedCallsBefore, synchronizedCallsAfter;
            public int synchronizedCounterBefore, synchronizedCounterAfter;
            public int synchronizedIndexBefore, synchronizedIndexAfter;
            public int lastSynchronizedCallSiteBefore, lastSynchronizedCallSiteAfter;
            public bool standingAttackRngObserved;
            public int standingAttackRngResult = -1;
            public bool keyboardJPressed, keyboardKPressed, keyboardLPressed, keyboardDPressed;
            public bool attackActionPressed, jumpActionPressed;
            public string attackActiveControl, jumpActiveControl;
            public InputSample p1, p2;
            public EntitySample actor, target;
            public int worldObjects, runtimeSlots;
            public bool centralFrameValid, actorBodyCommand, targetBodyCommand, worldCameraEnabled;
            public int actorRenderPic = -1, actorSnapshotPic = -1, actorCommandPic = -1;
            public int actorCommandVisualDataId = -1, centralCommandCount, centralCommandTick = -1;
            public bool actorCatalogEntry, actorLegacySprite, actorCentralBindingValid;
            public string actorSourceSheetPath, actorSourceRect, actorCentralBindingMode;
            public float actorSourceRectX, actorSourceRectY, actorSourceRectWidth, actorSourceRectHeight;
            public string combatChecksumBeforeRender, combatChecksumAfterRender;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId, startedUtc, phase, status, error, firstDifference, releaseStartedUtc;
            public int targetX;
            public bool frame430;
            public bool captureFrame430Pixels;
            public bool captureComposedGameView;
            public bool missingBodySheet;
            public bool diagnosticConfigRestored;
            public string gameViewPng, gameViewChecksumBefore, gameViewChecksumAfter;
            public int gameViewTickBefore = -1, gameViewTickAfter = -1;
            public int gameViewWidth, gameViewHeight;
            public string sceneCentralPng, scratchBodyOnPng, scratchBodyOffPng;
            public string pixelChecksumBefore, pixelChecksumAfter;
            public int pixelTickBefore = -1, pixelTickAfter = -1;
            public int bodyDeltaPixelCount, sceneMatchesBodyOnPixelCount, sceneMatchesBodyOffPixelCount;
            public bool pixelCameraRestored;
            public string evidenceScope = "Original Editor Battle Scene; physical keyboard events; paused full Driver host steps using production input provider. Not LocalFreeRun wall-clock cadence or formal tick-zero same-world parity (project Z650 vs source Z350).";
            public string inputContract = "Serialized Player_1 physical J=Attack action -> canonical Jump; physical K=Jump action -> canonical Defend (existing crossed carrier).";
            public bool bootstrapConfiguredBeforeStart, stopped, worldDetached, exitedPlay, sceneCleanAfter, neutralKeyboardObserved;
            public string contentRoot, sceneHashBefore, sceneHashAfter, lifecycleAfter, shutdownStatus, shutdownStage, shutdownFailure;
            public int startTick, endTick, initialInputPhase, battleMode, keyboardDeviceId, firstCatchTick = -1, firstInjuryTick = -1, firstResourceTick = -1;
            public int firstActorCostTick = -1, firstTargetResourceIncreaseTick = -1;
            public int firstActorPostCatchIncreaseTick = -1, actorPostCatchIncrease, targetResourceIncrease;
            public int firstFrame430RelativeTick = -1, firstFrame430AbsoluteTick = -1;
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

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ConfigureMissingBodyRootBeforeSceneLoad()
        {
            RestoreSession();
            if (report == null || report.phase != "STARTUP" || !report.missingBodySheet)
                return;
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigAssetPath);
            Require(source != null && AssetDatabase.Contains(source),
                "Saved GameConfig Asset is unavailable for missing-body probe.");
            Require(GameConfig.Instance == null || ReferenceEquals(GameConfig.Instance, source),
                "A foreign GameConfig singleton is active before missing-body Play.");
            foreach (GameConfig loaded in Resources.FindObjectsOfTypeAll<GameConfig>())
                Require(loaded == null || AssetDatabase.Contains(loaded),
                    "A non-asset GameConfig already exists before missing-body Play.");

            GameConfig clone = UnityEngine.Object.Instantiate(source);
            clone.name = "GameConfig-Q09P20MissingBodyClone";
            clone.hideFlags = HideFlags.DontSave;
            clone.BattleContentRuntimeRoot = MissingBodyContentRoot;
            SetGameConfigInstance(null);
            GameConfig.Instance = clone;
        }

        private static bool IsMissingBodyConfigClone(GameConfig candidate)
        {
            return candidate != null && !AssetDatabase.Contains(candidate) &&
                candidate.name == "GameConfig-Q09P20MissingBodyClone" &&
                (candidate.hideFlags & HideFlags.DontSave) == HideFlags.DontSave &&
                candidate.BattleContentRuntimeRoot == MissingBodyContentRoot;
        }

        private static void SetGameConfigInstance(GameConfig value)
        {
            FieldInfo field = typeof(GameConfig).GetField("_instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require(field != null, "GameConfig singleton backing field changed.");
            field.SetValue(null, value);
        }

        private static void RestoreMissingBodyConfigAfterPlay()
        {
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigAssetPath);
            Require(source != null && AssetDatabase.Contains(source),
                "Saved GameConfig Asset is unavailable after missing-body Play.");
            GameConfig current = GameConfig.Instance;
            Require(current == null || ReferenceEquals(current, source) ||
                IsMissingBodyConfigClone(current),
                "An unrelated GameConfig singleton replaced the diagnostic clone.");
            GameConfig[] loaded = Resources.FindObjectsOfTypeAll<GameConfig>();
            foreach (GameConfig candidate in loaded)
                Require(candidate == null || AssetDatabase.Contains(candidate) ||
                    IsMissingBodyConfigClone(candidate),
                    "An unrelated non-asset GameConfig is loaded after diagnostic Play.");
            SetGameConfigInstance(null);
            foreach (GameConfig candidate in loaded)
                if (IsMissingBodyConfigClone(candidate))
                    UnityEngine.Object.DestroyImmediate(candidate);
            GameConfig.Instance = source;
            report.diagnosticConfigRestored = ReferenceEquals(GameConfig.Instance, source);
            Require(report.diagnosticConfigRestored,
                "Saved GameConfig Asset was not restored after missing-body Play.");
        }

        private static void RestoreSession()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, "");
            if (string.IsNullOrEmpty(saved)) return;
            report = JsonUtility.FromJson<Report>(saved);
            if (report.phase == "INPUT" || report.phase == "NEUTRAL" ||
                report.phase == "GAMEVIEW_PENDING")
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
                if (report.missingBodySheet)
                    Require(IsMissingBodyConfigClone(GameConfig.Instance),
                        "Missing-body diagnostic config was not selected before Battle Scene load.");
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
                    if (releasedKeyboard != null && (releasedKeyboard.jKey.isPressed ||
                        releasedKeyboard.kKey.isPressed ||
                        (report.frame430 && (releasedKeyboard.lKey.isPressed ||
                            releasedKeyboard.dKey.isPressed))))
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
                        !releasedKeyboard.jKey.isPressed && !releasedKeyboard.kKey.isPressed &&
                        (!report.frame430 || (!releasedKeyboard.lKey.isPressed &&
                            !releasedKeyboard.dKey.isPressed));
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
                if (report.phase == "GAMEVIEW_PENDING")
                {
                    CompleteComposedGameViewCapture();
                    return;
                }
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
            string requestPath = MissingBodyRequestPath;
            Request request = File.Exists(requestPath)
                ? JsonUtility.FromJson<Request>(File.ReadAllText(requestPath)) : null;
            if (request == null || !request.requested)
            {
                requestPath = Frame430RequestPath;
                request = File.Exists(requestPath)
                    ? JsonUtility.FromJson<Request>(File.ReadAllText(requestPath)) : null;
                if (request == null || !request.requested)
                {
                    requestPath = RequestPath;
                    if (!File.Exists(requestPath)) return;
                    request = JsonUtility.FromJson<Request>(File.ReadAllText(requestPath));
                }
            }
            if (request == null || !request.requested) return;
            Require(requestPath != MissingBodyRequestPath ||
                request.missingBodySheet && request.frame430 &&
                !request.captureFrame430Pixels && !request.captureComposedGameView,
                "Dedicated missing-body request requires frame430 without pixel capture.");
            Require(requestPath != Frame430RequestPath || request.frame430,
                "Dedicated frame430 request must opt in to frame430 mode.");
            Require(!request.missingBodySheet || requestPath == MissingBodyRequestPath,
                "Missing-body mode requires the dedicated request path.");
            Require(!request.captureFrame430Pixels || request.frame430,
                "Frame430 pixel capture requires frame430 mode.");
            Require(!request.captureComposedGameView ||
                request.frame430 && !request.captureFrame430Pixels,
                "Composed Game View capture requires frame430 mode without isolated pixel capture.");
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 100 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require(!File.Exists(ReportRoot(request.frame430, request.captureFrame430Pixels,
                request.captureComposedGameView, request.missingBodySheet) +
                request.runId + ".json"), "Refusing to overwrite runId.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Request must begin in Edit Mode.");
            if (request.missingBodySheet) PreflightMissingBodyRoot();
            request.requested = false;
            File.WriteAllText(requestPath, JsonUtility.ToJson(request, true));
            report = new Report { runId = request.runId, targetX = request.targetX,
                frame430 = request.frame430,
                captureFrame430Pixels = request.captureFrame430Pixels,
                captureComposedGameView = request.captureComposedGameView,
                missingBodySheet = request.missingBodySheet,
                startedUtc = DateTime.UtcNow.ToString("O"), phase = "STARTUP", status = "RUNNING" };
            Require(request.frame430 ? request.targetX == 1200 :
                request.targetX == 580 || request.targetX == 1200,
                "targetX is outside the selected diagnostic fixture.");
            if (request.frame430)
            {
                report.evidenceScope = request.missingBodySheet
                    ? "Original Battle Scene physical input/full Driver, isolated root missing only hid6; selected body-command omission and other content only, not formal same-world or GUI pixel parity."
                    : request.captureComposedGameView
                    ? "Original Battle Scene physical input, complete Driver, natural frame430 composed Unity Game View; not formal root GUI GPU or same-world pixel parity."
                    : request.captureFrame430Pixels
                        ? "Original Battle Scene physical input, complete Driver, isolated World Camera central pixels and same-frozen-command scratch A/B; not full Game View composition or formal root GPU parity."
                        : "Original Battle Scene physical input and complete Driver; natural frame430 central command observation only, not formal GPU pixel parity.";
                report.inputContract = "Physical J1-2, K9-10, L+D+J13-14; " +
                    "the existing crossed carrier maps these to attack, jump, defend+forward+attack.";
            }
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

        private static void PreflightMissingBodyRoot()
        {
            GameConfig source = AssetDatabase.LoadAssetAtPath<GameConfig>(GameConfigAssetPath);
            Require(source != null && AssetDatabase.Contains(source) &&
                source.BattleContentRuntimeRoot == "Assets/NTSD/Content/LoganRuntime",
                "The saved formal GameConfig Asset changed.");
            Require(GameConfig.Instance == null || ReferenceEquals(GameConfig.Instance, source),
                "A foreign GameConfig singleton is active before the missing-body request.");
            foreach (GameConfig loaded in Resources.FindObjectsOfTypeAll<GameConfig>())
                Require(loaded == null || AssetDatabase.Contains(loaded),
                    "A non-asset GameConfig is loaded before the missing-body request.");

            string root = Path.GetFullPath(Path.Combine(Application.dataPath, "..",
                MissingBodyContentRoot));
            string hid = Path.Combine(root, "vfs", "c", "hid");
            Require(File.Exists(Path.Combine(root, "catalog.csv")) &&
                Directory.Exists(Path.Combine(root, "decoded_dat")) &&
                Directory.Exists(hid) &&
                Directory.GetFiles(hid, "*.png").Length == 9 &&
                !File.Exists(Path.Combine(hid, "hid6.png")),
                "The isolated missing-hid6 root is incomplete or changed.");
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
            Require(report.contentRoot == (report.missingBodySheet
                    ? MissingBodyContentRoot : "Assets/NTSD/Content/LoganRuntime"),
                "Wrong formal content root.");
            if (report.missingBodySheet)
                Require(IsMissingBodyConfigClone(GameConfig.Instance) &&
                    CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                    "Missing-body Battle content was not published from the diagnostic clone.");
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
            if (report.frame430 && (index == 12 || index == 13))
                QueueKeys(Key.L, Key.D, Key.J);
            else if (index < 2) QueueKeys(Key.J);
            else if (report.frame430 ? index == 8 || index == 9 : index < 4)
                QueueKeys(Key.K);
            else QueueKeys();
            SaveSession();
        }

        private static SimulationInputButtons ExpectedFrame430Buttons(int index)
        {
            if (index < 0) return SimulationInputButtons.None;
            if (index < 2) return SimulationInputButtons.Jump;
            if (index == 8 || index == 9) return SimulationInputButtons.Defend;
            if (index == 12 || index == 13)
                return SimulationInputButtons.Attack | SimulationInputButtons.Right |
                       SimulationInputButtons.Jump;
            return SimulationInputButtons.None;
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
            NTSD28NativeRandomScalarState randomBefore = world.NativeRandom.CaptureScalarState();
            Require(driver.StepOneTick(ignorePaused: true, buildPresentation: true), "Full Driver rejected tick.");
            NTSD28NativeRandomScalarState randomAfter = world.NativeRandom.CaptureScalarState();
            int tick = driver.CurrentTickIndex;
            Require(tick == report.endTick + 1, "Skipped/unobserved completed logic tick.");
            report.endTick = tick;
            FrameInputSet frame = driver.LastAppliedFrameInput;
            Keyboard keyboard = InputSystem.GetDeviceById(report.keyboardDeviceId) as Keyboard;
            CharacterInputModule input = actor.Controller as CharacterInputModule;
            var sample = new TickSample { relativeTick = index + 1, tick = tick,
                canonicalTick = frame.TickIndex, inputPhase = world.InputPhase,
                synchronizedCallsBefore = (long)randomBefore.SynchronizedCalls,
                synchronizedCallsAfter = (long)randomAfter.SynchronizedCalls,
                synchronizedCounterBefore = randomBefore.SynchronizedCounter,
                synchronizedCounterAfter = randomAfter.SynchronizedCounter,
                synchronizedIndexBefore = randomBefore.SynchronizedIndex,
                synchronizedIndexAfter = randomAfter.SynchronizedIndex,
                lastSynchronizedCallSiteBefore = (int)randomBefore.LastSynchronizedCallSite,
                lastSynchronizedCallSiteAfter = (int)randomAfter.LastSynchronizedCallSite,
                physicalKeys = report.frame430
                    ? index < 2 ? "J" : index == 8 || index == 9 ? "K" :
                      index == 12 || index == 13 ? "L+D+J" : "Neutral"
                    : index < 2 ? "J (Attack action)" : index < 4 ? "K (Jump action)" : "Neutral",
                keyboardJPressed = keyboard?.jKey.isPressed == true,
                keyboardKPressed = keyboard?.kKey.isPressed == true,
                keyboardLPressed = keyboard?.lKey.isPressed == true,
                keyboardDPressed = keyboard?.dKey.isPressed == true,
                attackActionPressed = input?.AttackAction?.ReadValue<float>() > 0.5f,
                jumpActionPressed = input?.JumpAction?.ReadValue<float>() > 0.5f,
                attackActiveControl = input?.AttackAction?.activeControl?.path,
                jumpActiveControl = input?.JumpAction?.activeControl?.path,
                p1 = CaptureInput(frame, 0), p2 = CaptureInput(frame, 1), actor = Capture(actor), target = Capture(target),
                worldObjects = world.ObjectCount, runtimeSlots = world.ClaimedRuntimeSlotCountForDiagnostics };
            if (randomAfter.SynchronizedCalls == randomBefore.SynchronizedCalls + 1UL &&
                randomAfter.LastSynchronizedCallSite == 0x82u)
            {
                NTSD28SynchronizedRandomState synchronized =
                    world.NativeRandom.CaptureSynchronizedState();
                sample.standingAttackRngObserved = true;
                sample.standingAttackRngResult = (int)(
                    ((uint)synchronized.Table[synchronized.Index] +
                     (uint)synchronized.Counter) % 2u);
            }
            if (report.frame430 && sample.actor.frame == 430)
            {
                if (report.firstFrame430RelativeTick < 0)
                {
                    report.firstFrame430RelativeTick = sample.relativeTick;
                    report.firstFrame430AbsoluteTick = tick;
                }
                sample.actorRenderPic = actor.GetRenderPicIndex();
                Camera camera = NTSDRenderSpace.WorldCamera;
                sample.worldCameraEnabled = camera != null && camera.isActiveAndEnabled;
                sample.combatChecksumBeforeRender = world.CaptureParityFrameSnapshot(tick).OverallChecksum;
                BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(world);
                BattlePresentationFrame presentation = plan.CapturedFrame;
                sample.centralFrameValid = plan.IsValid && !plan.IsStale && presentation != null &&
                    presentation.CommandsMaterialized && presentation.TickIndex == world.CurrentTickIndex;
                if (sample.centralFrameValid)
                {
                    sample.centralCommandTick = presentation.TickIndex;
                    sample.centralCommandCount = presentation.CommandCount;
                    for (int entityIndex = 0; entityIndex < presentation.EntityCount; entityIndex++)
                    {
                        BattlePresentationEntitySnapshot entity = presentation.GetEntity(entityIndex);
                        if (entity.RuntimeSlot == actor.Runtime.SlotIndex)
                            sample.actorSnapshotPic = entity.EffectivePic;
                    }
                    for (int commandIndex = 0; commandIndex < presentation.CommandCount; commandIndex++)
                    {
                        BattleRenderCommand command = presentation.GetCommand(commandIndex);
                        if (command.Type != BattleRenderCommandType.Entity) continue;
                        if (command.RuntimeSlot == actor.Runtime.SlotIndex)
                        {
                            sample.actorBodyCommand = true;
                            sample.actorCommandPic = command.EffectivePic;
                            sample.actorCommandVisualDataId = command.VisualDataId;
                            if (presentation.BoundCatalogForAcceptance.TryGet(
                                    command.VisualDataId, command.EffectivePic,
                                    out BattleSpriteEntry entry))
                            {
                                sample.actorCatalogEntry = true;
                                sample.actorSourceSheetPath = entry.SourceSheetPath;
                                sample.actorSourceRect = entry.PixelRect.ToString();
                                sample.actorSourceRectX = entry.PixelRect.x;
                                sample.actorSourceRectY = entry.PixelRect.y;
                                sample.actorSourceRectWidth = entry.PixelRect.width;
                                sample.actorSourceRectHeight = entry.PixelRect.height;
                                sample.actorLegacySprite = entry.LegacySprite != null;
                                sample.actorCentralBindingValid = entry.CentralBinding.IsValid;
                                sample.actorCentralBindingMode = entry.CentralBinding.Mode.ToString();
                            }
                        }
                        if (command.RuntimeSlot == target.Runtime.SlotIndex) sample.targetBodyCommand = true;
                    }
                    if (report.captureFrame430Pixels &&
                        sample.relativeTick == report.firstFrame430RelativeTick)
                        CaptureFrame430Pixels(camera, presentation);
                }
                sample.combatChecksumAfterRender = world.CaptureParityFrameSnapshot(tick).OverallChecksum;
            }
            report.samples.Add(sample);
            SimulationInputButtons expected = report.frame430
                ? ExpectedFrame430Buttons(index)
                : index < 2 ? SimulationInputButtons.Jump :
                  index < 4 ? SimulationInputButtons.Defend : SimulationInputButtons.None;
            SimulationInputButtons expectedPressed = report.frame430
                ? expected & ~ExpectedFrame430Buttons(index - 1)
                : index == 0 ? SimulationInputButtons.Jump :
                  index == 2 ? SimulationInputButtons.Defend : SimulationInputButtons.None;
            SimulationInputButtons expectedReleased = report.frame430
                ? ExpectedFrame430Buttons(index - 1) & ~expected
                : index == 2 ? SimulationInputButtons.Jump :
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
            if (report.captureComposedGameView &&
                sample.relativeTick == report.firstFrame430RelativeTick)
            {
                RequestComposedGameViewCapture();
                return;
            }
            SaveSession();
            if (report.samples.Count < (report.frame430 ? 30 : 40))
            {
                QueueForNextTick();
                return;
            }
            if (report.frame430)
            {
                FinishFrame430();
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
                    WriteReport(ReportRoot(report.frame430, report.captureFrame430Pixels,
                        report.captureComposedGameView, report.missingBodySheet) +
                        report.runId + "-shutdown-blocked.json");
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
            if (report.missingBodySheet && !report.diagnosticConfigRestored)
            {
                try { RestoreMissingBodyConfigAfterPlay(); }
                catch (Exception error)
                {
                    report.status = "FAIL";
                    report.error += "Config restoration: " + error + "\n";
                }
            }
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
            string path = ReportRoot(report.frame430, report.captureFrame430Pixels,
                report.captureComposedGameView, report.missingBodySheet) + report.runId + ".json";
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

        private static string ReportRoot(bool frame430, bool captureFrame430Pixels,
            bool captureComposedGameView, bool missingBodySheet) =>
            missingBodySheet ? MissingBodyResultRoot :
            captureComposedGameView ? Frame430GameViewResultRoot :
            captureFrame430Pixels ? Frame430PixelResultRoot :
            frame430 ? Frame430ResultRoot : ResultRoot;

        private static void RequestComposedGameViewCapture()
        {
            Require(report.frame430 && report.samples.Count == report.firstFrame430RelativeTick,
                "Game View capture is not on the first natural frame430 tick.");
            Require(Screen.width > 0 && Screen.height > 0,
                "Game View dimensions are unavailable.");
            report.gameViewTickBefore = world.CurrentTickIndex;
            report.gameViewChecksumBefore = world.CaptureParityFrameSnapshot(
                report.gameViewTickBefore).OverallChecksum;
            report.gameViewPng = Frame430GameViewResultRoot + report.runId + "-gameview.png";
            string path = Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, report.gameViewPng));
            Require(!File.Exists(path), "Refusing to overwrite Game View image.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            report.phase = "GAMEVIEW_PENDING";
            SaveSession();
            ScreenCapture.CaptureScreenshot(path);
            EditorApplication.QueuePlayerLoopUpdate();
        }

        private static void CompleteComposedGameViewCapture()
        {
            string path = Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, report.gameViewPng));
            if (!File.Exists(path) || new FileInfo(path).Length < 24) return;
            var image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                if (!ImageConversion.LoadImage(image, bytes, false)) return;
                report.gameViewWidth = image.width;
                report.gameViewHeight = image.height;
            }
            catch (IOException)
            {
                return;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(image);
            }
            Require(report.gameViewWidth > 0 && report.gameViewHeight > 0,
                "Composed Game View image is empty.");
            report.gameViewTickAfter = world.CurrentTickIndex;
            report.gameViewChecksumAfter = world.CaptureParityFrameSnapshot(
                report.gameViewTickAfter).OverallChecksum;
            Require(report.gameViewTickBefore == report.gameViewTickAfter &&
                    report.gameViewChecksumBefore == report.gameViewChecksumAfter,
                "Composed Game View capture changed the combat World.");
            FinishFrame430();
        }

        private static void FinishFrame430()
        {
            TickSample frame430 = report.samples.FirstOrDefault(value =>
                value.relativeTick == report.firstFrame430RelativeTick);
            if (report.missingBodySheet)
            {
                if (frame430 == null || frame430.actor.frame != 430 ||
                    frame430.actorRenderPic != 119 || frame430.actorSnapshotPic != 119 ||
                    !frame430.centralFrameValid || !frame430.worldCameraEnabled ||
                    frame430.actorBodyCommand || frame430.actorCatalogEntry ||
                    !frame430.targetBodyCommand ||
                    frame430.centralCommandTick != frame430.tick ||
                    frame430.combatChecksumBeforeRender != frame430.combatChecksumAfterRender)
                    FirstDifference("Missing hid6 frame430 did not preserve logic/other body while omitting its body command.");
                report.status = string.IsNullOrEmpty(report.firstDifference)
                    ? "PASS_SCOPED_MISSING_BODY_FRAME430" : "OBSERVED_DIFFERENCE";
                Close();
                return;
            }
            if (frame430 == null ||
                frame430.actor.frame != 430 || frame430.actorRenderPic != 119 ||
                !frame430.centralFrameValid || !frame430.worldCameraEnabled ||
                !frame430.actorBodyCommand || frame430.actorCommandPic != 119 ||
                frame430.actorCommandVisualDataId != 24 ||
                !frame430.actorCatalogEntry || !frame430.actorLegacySprite ||
                !frame430.actorCentralBindingValid ||
                string.IsNullOrEmpty(frame430.actorSourceSheetPath) ||
                !frame430.actorSourceSheetPath.Replace('\\', '/').EndsWith(
                    "/c/hid/hid6.png", StringComparison.OrdinalIgnoreCase) ||
                Mathf.Abs(frame430.actorSourceRectX - 722f) > 0.01f ||
                Mathf.Abs(frame430.actorSourceRectY - 871f) > 0.01f ||
                Mathf.Abs(frame430.actorSourceRectWidth - 360f) > 0.01f ||
                Mathf.Abs(frame430.actorSourceRectHeight - 289f) > 0.01f ||
                !frame430.targetBodyCommand ||
                frame430.centralCommandTick != frame430.tick ||
                frame430.combatChecksumBeforeRender != frame430.combatChecksumAfterRender)
            {
                FirstDifference("Natural frame430 central command/catalog/camera differs from formal hid6 body projection.");
            }
            report.status = string.IsNullOrEmpty(report.firstDifference)
                ? report.captureComposedGameView
                    ? "CAPTURED_SCOPED_FRAME430_GAMEVIEW"
                    : report.captureFrame430Pixels
                        ? "CAPTURED_SCOPED_FRAME430_PIXEL" : "PASS_SCOPED_FRAME430_COMMAND"
                : "OBSERVED_DIFFERENCE";
            Close();
        }

        private static void CaptureFrame430Pixels(Camera camera, BattlePresentationFrame source)
        {
            Require(camera != null && camera.isActiveAndEnabled,
                "Frame430 pixel capture requires the active World Camera.");
            report.pixelTickBefore = world.CurrentTickIndex;
            report.pixelChecksumBefore = world.CaptureParityFrameSnapshot(
                report.pixelTickBefore).OverallChecksum;

            int actorCommandIndex = -1;
            for (int index = 0; index < source.CommandCount; index++)
            {
                BattleRenderCommand command = source.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Entity ||
                    command.RuntimeSlot != actor.Runtime.SlotIndex)
                    continue;
                Require(actorCommandIndex < 0,
                    "Frame430 actor has more than one body command.");
                actorCommandIndex = index;
            }
            Require(actorCommandIndex >= 0,
                "Frame430 actor body command is missing.");

            MethodInfo addCommand = typeof(BattlePresentationFrame).GetMethod(
                "AddCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            Require(addCommand != null,
                "Frozen frame diagnostic command writer is unavailable.");
            var bodyOnFrame = new BattlePresentationFrame();
            var bodyOffFrame = new BattlePresentationFrame();
            for (int index = 0; index < source.CommandCount; index++)
            {
                BattleRenderCommand command = source.GetCommand(index);
                addCommand.Invoke(bodyOnFrame, new object[] { command });
                if (index != actorCommandIndex)
                    addCommand.Invoke(bodyOffFrame, new object[] { command });
            }
            Require(bodyOnFrame.CommandCount == bodyOffFrame.CommandCount + 1,
                "Diagnostic copy did not remove exactly the actor body command.");

            Material material = BattleCentralRenderSystem.RegisteredFeatureMaterialForAcceptance;
            Material arrayMaterial =
                BattleCentralRenderSystem.RegisteredFeatureArrayMaterialForAcceptance;
            Require(material != null && arrayMaterial != null,
                "Central production materials are unavailable.");
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.Configure(source.BoundCatalogForAcceptance,
                source.CommonVisualCatalog, material, arrayMaterial);
            FieldInfo drawModeField = typeof(BattleCentralRenderSystem).GetField(
                "drawMode", BindingFlags.Static | BindingFlags.NonPublic);
            Require(drawModeField != null,
                "Central production draw mode is unavailable.");
            var drawMode = (BattleCentralDrawMode)drawModeField.GetValue(null);
            using var bodyOnBackend = new BattleDynamicMeshBackend();
            using var bodyOffBackend = new BattleDynamicMeshBackend();
            bodyOnBackend.Build(bodyOnFrame, resolver, drawMode);
            bodyOffBackend.Build(bodyOffFrame, resolver, drawMode);
            Require(bodyOnBackend.Diagnostics.ResolvedCommandCount ==
                    bodyOffBackend.Diagnostics.ResolvedCommandCount + 1 &&
                    bodyOnBackend.SegmentCount > 0,
                "Frame430 body did not resolve to one central draw command.");

            Color32[] scene = CaptureWorldCameraCentral(camera,
                out Matrix4x4 view, out Matrix4x4 projection,
                out report.sceneCentralPng);
            Require(report.pixelCameraRestored,
                "World Camera state was not restored after capture.");
            Color32[] bodyOn = RenderFrozenCommands(bodyOnBackend, view, projection,
                "body-on", out report.scratchBodyOnPng);
            Color32[] bodyOff = RenderFrozenCommands(bodyOffBackend, view, projection,
                "body-off", out report.scratchBodyOffPng);
            Require(scene.Length == PixelWidth * PixelHeight &&
                    bodyOn.Length == scene.Length && bodyOff.Length == scene.Length,
                "Frame430 GPU images have incompatible dimensions.");
            for (int index = 0; index < scene.Length; index++)
            {
                Color32 on = bodyOn[index];
                Color32 off = bodyOff[index];
                if (Near(on, off, 2))
                    continue;
                report.bodyDeltaPixelCount++;
                if (Near(scene[index], on, 8))
                    report.sceneMatchesBodyOnPixelCount++;
                if (Near(scene[index], off, 8))
                    report.sceneMatchesBodyOffPixelCount++;
            }
            report.pixelTickAfter = world.CurrentTickIndex;
            report.pixelChecksumAfter = world.CaptureParityFrameSnapshot(
                report.pixelTickAfter).OverallChecksum;
            Require(report.bodyDeltaPixelCount > 0 &&
                    report.pixelTickBefore == report.pixelTickAfter &&
                    report.pixelChecksumBefore == report.pixelChecksumAfter,
                "Frame430 GPU capture lacks body pixels or changed combat World.");
        }

        private static Color32[] CaptureWorldCameraCentral(Camera camera,
            out Matrix4x4 view, out Matrix4x4 projection,
            out string relativePath)
        {
            int cullingMask = camera.cullingMask;
            CameraClearFlags clearFlags = camera.clearFlags;
            Color backgroundColor = camera.backgroundColor;
            bool allowHdr = camera.allowHDR;
            bool allowMsaa = camera.allowMSAA;
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            var target = new RenderTexture(PixelWidth, PixelHeight, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            view = Matrix4x4.identity;
            projection = Matrix4x4.identity;
            relativePath = null;
            try
            {
                target.Create();
                camera.cullingMask = 0;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = Color.white;
                camera.allowHDR = false;
                camera.allowMSAA = false;
                camera.targetTexture = target;
                view = camera.worldToCameraMatrix;
                projection = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
                camera.Render();
                return ReadAndSave(target, "scene-camera", out relativePath);
            }
            finally
            {
                camera.cullingMask = cullingMask;
                camera.clearFlags = clearFlags;
                camera.backgroundColor = backgroundColor;
                camera.allowHDR = allowHdr;
                camera.allowMSAA = allowMsaa;
                camera.targetTexture = previousTarget;
                RenderTexture.active = previousActive;
                report.pixelCameraRestored = camera.cullingMask == cullingMask &&
                    camera.clearFlags == clearFlags &&
                    camera.backgroundColor == backgroundColor &&
                    camera.allowHDR == allowHdr && camera.allowMSAA == allowMsaa &&
                    camera.targetTexture == previousTarget &&
                    RenderTexture.active == previousActive;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Color32[] RenderFrozenCommands(BattleDynamicMeshBackend backend,
            Matrix4x4 view, Matrix4x4 projection, string suffix,
            out string relativePath)
        {
            var target = new RenderTexture(PixelWidth, PixelHeight, 0,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var commands = new CommandBuffer { name = "Q09 Hidan Frame430 " + suffix };
            relativePath = null;
            try
            {
                target.Create();
                commands.SetRenderTarget(target);
                commands.SetViewport(new Rect(0, 0, PixelWidth, PixelHeight));
                commands.ClearRenderTarget(false, true, Color.white);
                commands.SetViewProjectionMatrices(view, projection);
                var properties = new MaterialPropertyBlock();
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    Require(segment.Material != null && segment.Texture != null,
                        "Central segment has no material or texture.");
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
                return ReadAndSave(target, suffix, out relativePath);
            }
            finally
            {
                commands.Release();
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static Color32[] ReadAndSave(RenderTexture target, string suffix,
            out string relativePath)
        {
            relativePath = Frame430PixelResultRoot + report.runId + "-" + suffix + ".png";
            string path = Path.GetFullPath(Path.Combine(
                Directory.GetParent(Application.dataPath).FullName, relativePath));
            Require(!File.Exists(path), "Refusing to overwrite frame430 GPU image.");
            RenderTexture previousActive = RenderTexture.active;
            Texture2D readback = null;
            try
            {
                RenderTexture.active = target;
                readback = new Texture2D(PixelWidth, PixelHeight,
                    TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0, 0, PixelWidth, PixelHeight),
                    0, 0, false);
                readback.Apply(false, false);
                Color32[] pixels = readback.GetPixels32();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                using (var stream = new FileStream(path, FileMode.CreateNew,
                    FileAccess.Write))
                {
                    byte[] png = readback.EncodeToPNG();
                    stream.Write(png, 0, png.Length);
                }
                return pixels;
            }
            finally
            {
                RenderTexture.active = previousActive;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
            }
        }

        private static bool Near(Color32 left, Color32 right, int tolerance) =>
            Math.Abs(left.r - right.r) <= tolerance &&
            Math.Abs(left.g - right.g) <= tolerance &&
            Math.Abs(left.b - right.b) <= tolerance &&
            Math.Abs(left.a - right.a) <= tolerance;

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
