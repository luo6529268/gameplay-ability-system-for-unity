#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q09P08SameStateBattlePlayProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScenePath = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q09_P08SameState.request.json";
        private const string RegressionRequestPath = "Temp/NTSD28_Q07_HumanButtonRegression.request.json";
        private const string IdleRequestPath = "Temp/NTSD28_Q09_IdleTick14.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q09-P08-ROOT-UNITY-SAME-STATE-001/";
        private const string IdleResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q09-IDLE-TICK14-GAMEVIEW-001/";
        private const string SessionKey = "NTSD.Q09.P08SameState";
        private const string ConsumedKey = "NTSD.Q09.P08SameState.Consumed";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character target;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public string scenario;
            public long expiresUtcTicks;
        }

        [Serializable]
        private sealed class Row
        {
            public int relativeTick;
            public int globalTick;
            public int submittedP1Buttons;
            public int actorAction;
            public int actorHp;
            public int actorMp;
            public int actorSourceX;
            public int actorSourceZ;
            public int actorViewX;
            public int targetAction;
            public int targetMp;
            public int targetSourceX;
            public int targetSourceZ;
            public int targetViewX;
            public int targetHp;
            public int targetBaseHp;
            public int targetBpointCount;
            public int bodyCommandIndex = -1;
            public int bleedCommandIndex = -1;
            public int bleedCommandCount;
            public int publishedTick = -1;
            public uint crtState;
            public ulong crtCalls;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string phase;
            public string error;
            public long startedUtcTicks;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public bool sceneCleanAfter;
            public bool exitedPlay;
            public bool configuredBeforeStart;
            public bool idleTick14;
            public bool returnToMenu;
            public bool returnedToMenu;
            public string menuHashBefore;
            public string menuHashAfter;
            public string contentRoot;
            public int startTick;
            public int endTick;
            public int battleMode;
            public int difficulty;
            public int initialInputPhase;
            public int initialActorAction;
            public int initialActorSourceX;
            public int initialActorSourceZ;
            public int initialActorHp;
            public int initialActorMp;
            public int initialTargetAction;
            public int initialTargetSourceX;
            public int initialTargetSourceZ;
            public int initialTargetHp;
            public int initialTargetMp;
            public int initialTargetBaseHp;
            public uint initialCrtState;
            public ulong initialCrtCalls;
            public ulong initialTableHash;
            public int tick22MarkWidth;
            public int tick22MarkHeight;
            public float tick22MarkWorldX;
            public float tick22MarkWorldY;
            public float tick22MarkScreenX;
            public float tick22MarkScreenY;
            public string screenshot;
            public int screenshotWidth;
            public int screenshotHeight;
            public int screenshotTickAfter;
            public int publishedTick = -1;
            public int planTick = -1;
            public int screenWidth;
            public int screenHeight;
            public float cameraOrthographicSize;
            public float cameraAspect;
            public int cameraPixelWidth;
            public int cameraPixelHeight;
            public bool cameraTargetRestored;
            public List<Row> rows = new List<Row>();
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
        }

        private static void Restore()
        {
            if (report != null)
                return;
            string saved = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void Persist()
        {
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null || report.phase != "STARTUP" ||
                !EditorApplication.isPlaying || report.configuredBeforeStart ||
                scene.path != ScenePath)
                return;
            try
            {
                BattleTestBootstrap bootstrap = null;
                int count = 0;
                foreach (BattleTestBootstrap candidate in
                         Resources.FindObjectsOfTypeAll<BattleTestBootstrap>())
                {
                    if (candidate == null || !candidate.isActiveAndEnabled ||
                        candidate.gameObject.scene != scene ||
                        EditorUtility.IsPersistent(candidate))
                        continue;
                    bootstrap = candidate;
                    count++;
                }
                Require(count == 1, "Expected one active BattleTestBootstrap.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField(
                    "overrideCharacterIds", BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap override field is unavailable.");
                field.SetValue(bootstrap, new[] { 2, 9 });
                report.configuredBeforeStart = true;
                Persist();
            }
            catch (Exception exception)
            {
                Fail(exception.ToString());
            }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null)
                return;
            if (state == PlayModeStateChange.EnteredPlayMode &&
                report.phase == "STARTUP" && !report.configuredBeforeStart)
                Fail("Play clone was not configured before BattleTestBootstrap.Start.");
            if (state == PlayModeStateChange.EnteredEditMode &&
                report.phase == "EXITING")
                Finish();
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            try
            {
                Restore();
                if (report == null)
                {
                    TryStart();
                    return;
                }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        Finish();
                    return;
                }
                Require(DateTime.UtcNow - new DateTime(report.startedUtcTicks,
                    DateTimeKind.Utc) < TimeSpan.FromMinutes(8),
                    "Original Battle Scene Play timed out.");
                if (report.phase == "OPENING_SCENE")
                {
                    if (EditorApplication.isPlayingOrWillChangePlaymode)
                        return;
                    Scene scene = SceneManager.GetActiveScene();
                    Require(scene.path == ScenePath && !scene.isDirty &&
                            SceneManager.sceneCount == 1,
                        "The original Battle Scene did not open cleanly.");
                    report.sceneHashBefore = HashScene();
                    report.phase = "STARTUP";
                    Persist();
                    EditorApplication.EnterPlaymode();
                    return;
                }
                if (!EditorApplication.isPlaying)
                    return;
                if (report.phase == "STARTUP")
                    WaitForRoster();
                else if (report.phase == "MEASURING")
                    MeasureOneTick();
                else if (report.phase == "SCREENSHOT_WAIT")
                    WaitForIdleScreenshot();
                else
                    Fail("Unexpected probe phase: " + report.phase);
            }
            catch (Exception exception)
            {
                Fail(exception.ToString());
            }
        }

        private static void TryStart()
        {
            string idleRequestFile = ProjectPath(IdleRequestPath);
            string regressionRequestFile = ProjectPath(RegressionRequestPath);
            Request idleRequest = File.Exists(idleRequestFile)
                ? JsonUtility.FromJson<Request>(File.ReadAllText(idleRequestFile)) : null;
            bool activeIdleRequest = idleRequest != null && idleRequest.requested &&
                idleRequest.scenario == "idleTick14" &&
                idleRequest.expiresUtcTicks > DateTime.UtcNow.Ticks &&
                !string.IsNullOrEmpty(idleRequest.runId) &&
                !File.Exists(ResultPath(idleRequest.runId));
            if (activeIdleRequest && EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            string requestFile = activeIdleRequest
                ? idleRequestFile : File.Exists(regressionRequestFile)
                    ? regressionRequestFile : ProjectPath(RequestPath);
            if (!File.Exists(requestFile))
                return;
            Request request = activeIdleRequest
                ? idleRequest : JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            bool idleTick14 = requestFile == idleRequestFile;
            if (request == null || !request.requested ||
                string.IsNullOrEmpty(request.runId) ||
                SessionState.GetString(ConsumedKey, string.Empty) == request.runId ||
                File.Exists(ResultPath(request.runId)))
                return;
            if (idleTick14 && (request.scenario != "idleTick14" ||
                               request.expiresUtcTicks <= DateTime.UtcNow.Ticks))
                return;
            Require(request.runId.Length <= 80 &&
                request.runId.StartsWith(idleTick14
                    ? "idle-tick14-336b44-" : "ita-equal-hp-336b44-",
                    StringComparison.Ordinal),
                "Unexpected P-08 runId.");
            foreach (char character in request.runId)
                Require(char.IsLetterOrDigit(character) || character == '-',
                    "Invalid P-08 runId character.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Editor must be idle in Edit Mode.");
            Scene scene = SceneManager.GetActiveScene();
            if (idleTick14 && scene.path == MenuScenePath &&
                !scene.isDirty && SceneManager.sceneCount == 1)
            {
                report = new Report
                {
                    runId = request.runId,
                    status = "RUNNING",
                    phase = "OPENING_SCENE",
                    startedUtcTicks = DateTime.UtcNow.Ticks,
                    idleTick14 = true,
                    returnToMenu = true,
                    menuHashBefore = HashPath(MenuScenePath),
                };
                SessionState.SetString(ConsumedKey, request.runId);
                Persist();
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
                return;
            }
            if (idleTick14 && (scene.path != ScenePath || scene.isDirty ||
                               SceneManager.sceneCount != 1))
                return;
            Require(scene.path == ScenePath && !scene.isDirty &&
                SceneManager.sceneCount == 1,
                "A single clean saved NTSD_Battle Scene is required.");
            report = new Report
            {
                runId = request.runId,
                status = "RUNNING",
                phase = "STARTUP",
                startedUtcTicks = DateTime.UtcNow.Ticks,
                sceneHashBefore = HashScene(),
                idleTick14 = idleTick14,
            };
            SessionState.SetString(ConsumedKey, request.runId);
            Persist();
            EditorApplication.EnterPlaymode();
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart,
                "Play clone roster was not configured before Start.");
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5 ||
                !world.IsBattleSnapshotBoundaryReady)
                return;
            if (!driver.IsPaused)
            {
                driver.SetPaused(true);
                return;
            }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 3)
                return;
            Require(!driver.DedicatedSimulationWorkerActiveForDiagnostics,
                "The selected inline Driver is required for this paired diagnostic.");
            Require(driver.PresentationBackendMode ==
                    BattlePresentationBackendMode.CentralOnly,
                "CentralOnly production backend is required.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == ContentRoot &&
                    CharacterAnimtorManager.TryGetInstance()?.PublishedLoganContentIdentity != null,
                "Formal LoganRuntime content is not published.");
            LF2Entity second = null;
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity first) &&
                    first is LF2Character && first.ObjectId == 2 &&
                    world.TryResolveRosterInputEntity(1, out second) &&
                    second is LF2Character && second.ObjectId == 9,
                "The Play clone roster is not Naruto OID2 / Ita OID9.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            if (report.idleTick14)
            {
                SetInitialActor(actor, 500, 500, false, 400, 200);
                SetInitialActor(target, 620, 500, true, 400, 200);
            }
            else
            {
                SetInitialActor(actor, 500, 500, false);
                SetInitialActor(target, 540, 30, true);
            }
            actor.Team = actor.RelationTeam = 1;
            target.Team = target.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            world.Runtime.Match.Difficulty = 0;
            world.Runtime.NativeWorldClock.Reset();
            world.NativeRandom.ResetFromSeed(0u);
            if (report.idleTick14)
                world.NativeRandom.SynchronizedNext(0x004021E0u, 1);
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.battleMode = world.BattleGameModeId;
            report.difficulty = world.Difficulty;
            report.initialInputPhase = world.InputPhase;
            report.initialActorAction = actor.Frame.N;
            report.initialActorSourceX = actor.Runtime.SourceRuleXInt;
            report.initialActorSourceZ = actor.Runtime.SourceRuleZInt;
            report.initialActorHp = actor.Runtime.HP;
            report.initialActorMp = actor.Runtime.MP;
            report.initialTargetAction = target.Frame.N;
            report.initialTargetSourceX = target.Runtime.SourceRuleXInt;
            report.initialTargetSourceZ = target.Runtime.SourceRuleZInt;
            report.initialTargetHp = target.Runtime.HP;
            report.initialTargetMp = target.Runtime.MP;
            report.initialTargetBaseHp = target.Runtime.HP3;
            report.initialCrtState = rng.CrtState;
            report.initialCrtCalls = rng.CrtCalls;
            report.initialTableHash = rng.SynchronizedTableHash;
            bool initialTupleMatches = report.battleMode == 0 && report.difficulty == 0 &&
                report.initialActorAction == 0 && report.initialTargetAction == 0 &&
                report.initialActorSourceX == 500 && report.initialActorHp == 500 &&
                report.initialCrtState == 3374725112u && report.initialCrtCalls == 3000UL;
            if (report.idleTick14)
            {
                initialTupleMatches &= report.initialTargetSourceX == 620 &&
                    report.initialActorSourceZ == 400 && report.initialTargetSourceZ == 400 &&
                    report.initialActorMp == 200 && report.initialTargetHp == 500 &&
                    report.initialTargetBaseHp == 500 && report.initialTargetMp == 200;
            }
            else
            {
                initialTupleMatches &= report.initialTargetSourceX == 540 &&
                    report.initialActorSourceZ == 650 && report.initialTargetSourceZ == 650 &&
                    report.initialTargetHp == 30 && report.initialTargetBaseHp == 30;
            }
            Require(initialTupleMatches,
                "The selected formal root local initial tuple was not restored.");
            report.phase = "MEASURING";
            Persist();
        }

        private static void SetInitialActor(
            LF2Character character, int sourceX, int hp, bool faceLeft,
            int sourceZ = 650, int mp = 500)
        {
            character.Initialize(hp, mp);
            character.ImmediateFrame(0);
            character.Runtime.MP = mp;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir(faceLeft ? "left" : "right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.HitStun = 0;
            character.AttackExempt = 0;
            character.ItrRest.Reset();
            character.Runtime.SetPosition(
                world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(sourceZ));
            AppManager.SyncParticipantBirthPosition(character, sourceX, sourceZ);
            Require(character.Runtime.SourceRuleXInt == sourceX &&
                    character.Runtime.SourceRuleZInt == sourceZ &&
                    character.Runtime.YInt == 0 &&
                    character.Runtime.HP == hp && character.Runtime.HP3 == hp &&
                    character.Runtime.MP == mp,
                "Character source position or health was not restored.");
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                    !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics &&
                    driver.CurrentTickIndex == report.endTick,
                "Production World changed or the paused tick boundary is unstable.");
            if (report.rows.Count == (report.idleTick14 ? 14 : 22))
            {
                Complete();
                return;
            }
            int relativeTick = report.rows.Count + 1;
            int next = driver.CurrentTickIndex + 1;
            SimulationInputButtons p1 = !report.idleTick14 && relativeTick <= 2
                ? SimulationInputButtons.Jump : SimulationInputButtons.None;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, p1),
                new SimulationPlayerInput(1, SimulationInputButtons.None),
            });
            Require(driver.StepOneTick(input, ignorePaused: true,
                buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var row = new Row
            {
                relativeTick = relativeTick,
                globalTick = driver.CurrentTickIndex,
                submittedP1Buttons = (int)p1,
                actorAction = actor.Frame.N,
                actorHp = actor.Runtime.HP,
                actorMp = actor.Runtime.MP,
                actorSourceX = actor.Runtime.SourceRuleXInt,
                actorSourceZ = actor.Runtime.SourceRuleZInt,
                actorViewX = actor.Runtime.XInt,
                targetAction = target.Frame.N,
                targetMp = target.Runtime.MP,
                targetSourceX = target.Runtime.SourceRuleXInt,
                targetSourceZ = target.Runtime.SourceRuleZInt,
                targetViewX = target.Runtime.XInt,
                targetHp = target.Runtime.HP,
                targetBaseHp = target.Runtime.HP3,
                targetBpointCount = target.Frame.D?.BloodPoints?.Count ?? 0,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
            };
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            row.publishedTick = published?.TickIndex ?? -1;
            BattlePixelFramePlan plan = published?.TickIndex == driver.CurrentTickIndex
                ? BattleCentralRenderSystem.PrepareFrame(world) : default;
            BattlePresentationFrame commands = plan.IsValid && !plan.IsStale
                ? plan.CapturedFrame : null;
            if (commands != null && commands.CommandsMaterialized)
            {
                for (int index = 0; index < commands.CommandCount; index++)
                {
                    BattleRenderCommand command = commands.GetCommand(index);
                    if (command.RuntimeSlot != 1)
                        continue;
                    if (command.Type == BattleRenderCommandType.Entity)
                        row.bodyCommandIndex = index;
                    if (command.Type != BattleRenderCommandType.BleedMark)
                        continue;
                    row.bleedCommandCount++;
                    row.bleedCommandIndex = index;
                    if (relativeTick == 22)
                    {
                        report.tick22MarkWidth = Mathf.RoundToInt(command.Size.x);
                        report.tick22MarkHeight = Mathf.RoundToInt(command.Size.y);
                        report.tick22MarkWorldX = command.Position.x;
                        report.tick22MarkWorldY = command.Position.y;
                    }
                }
            }
            report.rows.Add(row);
            report.endTick = driver.CurrentTickIndex;
            Persist();
        }

        private static void Complete()
        {
            if (report.idleTick14)
            {
                CaptureIdleGameView();
                return;
            }
            Row last = report.rows[21];
            bool reached = report.rows[7].targetHp == 10 &&
                last.targetAction == 0 && last.targetHp == 10 &&
                last.targetBaseHp == 30 && last.bleedCommandCount == 1 &&
                last.bleedCommandIndex > last.bodyCommandIndex &&
                report.tick22MarkWidth == 1 && report.tick22MarkHeight == 3;
            if (reached)
            {
                CaptureCamera();
                report.status = "LOCAL_INITIAL_TUPLE_MARK_REACHED";
            }
            else
            {
                report.status = "SELECTED_TRACE_OR_MARK_DIFFERENCE";
            }
            report.phase = "EXITING";
            Persist();
            EditorApplication.ExitPlaymode();
        }

        private static void CaptureIdleGameView()
        {
            Require(driver.IsPaused && report.rows.Count == 14 &&
                    driver.CurrentTickIndex == report.endTick,
                "The idle tick14 screenshot boundary is unstable.");
            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            BattlePresentationFrame published = world.BattlePresentation.PublishedFrame;
            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            report.publishedTick = published?.TickIndex ?? -1;
            report.planTick = plan.CapturedFrame?.TickIndex ?? -1;
            Require(report.publishedTick == report.endTick &&
                    report.planTick == report.endTick,
                "The idle Game View does not have the same published logic tick.");
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.isActiveAndEnabled,
                "The original Battle Scene world camera is unavailable.");
            report.cameraOrthographicSize = camera.orthographicSize;
            report.cameraAspect = camera.aspect;
            report.cameraPixelWidth = camera.pixelWidth;
            report.cameraPixelHeight = camera.pixelHeight;
            report.screenWidth = Screen.width;
            report.screenHeight = Screen.height;
            Require(report.screenWidth > 0 && report.screenHeight > 0,
                "The composite Game View dimensions are unavailable.");
            report.screenshot = IdleResultRoot + report.runId + ".png";
            string path = ProjectPath(report.screenshot);
            Require(!File.Exists(path), "Refusing to overwrite the idle screenshot.");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            ScreenCapture.CaptureScreenshot(path);
            report.phase = "SCREENSHOT_WAIT";
            Persist();
        }

        private static void WaitForIdleScreenshot()
        {
            string path = ProjectPath(report.screenshot);
            if (!File.Exists(path) || new FileInfo(path).Length < 24)
                return;
            var image = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(image, File.ReadAllBytes(path), false))
                    return;
                report.screenshotWidth = image.width;
                report.screenshotHeight = image.height;
            }
            catch (IOException)
            {
                return;
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(image);
            }
            report.screenshotTickAfter = driver.CurrentTickIndex;
            Require(driver.IsPaused && report.screenshotTickAfter == report.endTick,
                "The logic tick changed during the composite screenshot.");
            Row last = report.rows[13];
            bool logicMatchesFormalTitle = last.actorAction == 3 &&
                last.targetAction == 3 && last.actorHp == 500 &&
                last.targetHp == 500 && last.actorMp == 200 &&
                last.targetMp == 200 && last.actorSourceX == 500 &&
                last.targetSourceX == 620 && last.actorSourceZ == 400 &&
                last.targetSourceZ == 400;
            report.status = logicMatchesFormalTitle
                ? "CAPTURED_FORMAL_TITLE_FIELDS_MATCH"
                : "CAPTURED_LOGIC_TITLE_FIRST_DIFFERENCE";
            report.phase = "EXITING";
            Persist();
            EditorApplication.ExitPlaymode();
        }

        private static void CaptureCamera()
        {
            Camera camera = NTSDRenderSpace.WorldCamera;
            Require(camera != null && camera.isActiveAndEnabled,
                "The original world camera is unavailable.");
            int width = 1920;
            int height = Mathf.Max(1, Mathf.RoundToInt(width /
                (camera.aspect > 0f ? camera.aspect : 16f / 9f)));
            var targetTexture = new RenderTexture(width, height, 24,
                RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.HideAndDontSave,
            };
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            Texture2D readback = null;
            try
            {
                targetTexture.Create();
                camera.targetTexture = targetTexture;
                camera.Render();
                RenderTexture.active = targetTexture;
                readback = new Texture2D(width, height, TextureFormat.RGBA32,
                    false, true);
                readback.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                readback.Apply(false, false);
                report.screenshot = ResultRoot + report.runId + ".png";
                string path = ProjectPath(report.screenshot);
                Require(!File.Exists(path), "Refusing to overwrite the screenshot.");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, readback.EncodeToPNG());
                Vector3 screen = camera.WorldToScreenPoint(new Vector3(
                    report.tick22MarkWorldX, report.tick22MarkWorldY, 0));
                report.tick22MarkScreenX = screen.x;
                report.tick22MarkScreenY = screen.y;
            }
            finally
            {
                RenderTexture.active = previousActive;
                camera.targetTexture = previousTarget;
                report.cameraTargetRestored = camera.targetTexture == previousTarget;
                if (readback != null)
                    UnityEngine.Object.DestroyImmediate(readback);
                targetTexture.Release();
                UnityEngine.Object.DestroyImmediate(targetTexture);
            }
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q09 P-08 same-state probe] " + message);
                return;
            }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Persist();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
            else if (!EditorApplication.isPlayingOrWillChangePlaymode)
                Finish();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            report.exitedPlay = true;
            report.sceneHashAfter = HashScene();
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == ScenePath && !scene.isDirty &&
                report.sceneHashAfter == report.sceneHashBefore;
            if (!report.sceneCleanAfter)
            {
                report.status = "FAIL";
                report.error += " Saved Battle Scene or active Scene changed.";
            }
            if (report.returnToMenu && report.sceneCleanAfter)
            {
                try
                {
                    EditorSceneManager.OpenScene(MenuScenePath, OpenSceneMode.Single);
                    Scene menu = SceneManager.GetActiveScene();
                    report.menuHashAfter = HashPath(MenuScenePath);
                    report.returnedToMenu = menu.path == MenuScenePath &&
                        !menu.isDirty && report.menuHashAfter == report.menuHashBefore;
                    if (!report.returnedToMenu)
                    {
                        report.status = "FAIL";
                        report.error += " Original Menu Scene did not return cleanly.";
                    }
                }
                catch (Exception exception)
                {
                    report.status = "FAIL";
                    report.error += " Menu Scene restore failed: " + exception;
                }
            }
            report.phase = "DONE";
            string path = ResultPath(report.runId);
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(report, true));
            }
            else
            {
                Debug.LogError("[Q09 P-08 same-state probe] Refusing result overwrite: " + path);
            }
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            actor = null;
            target = null;
            stableTick = -1;
            stableUpdates = 0;
        }

        private static string ResultPath(string runId) =>
            ProjectPath((runId.StartsWith("idle-tick14-336b44-",
                StringComparison.Ordinal) ? IdleResultRoot : ResultRoot) + runId + ".json");

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string HashScene() => HashPath(ScenePath);

        private static string HashPath(string relativePath)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(ProjectPath(relativePath)))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
#endif
