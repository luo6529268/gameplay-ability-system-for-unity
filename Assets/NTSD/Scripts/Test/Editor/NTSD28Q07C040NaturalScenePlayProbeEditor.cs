#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
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
    internal static class NTSD28Q07C040NaturalScenePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C040NaturalScene.request.json";
        private const string PlatformRequestPath =
            "Temp/NTSD28_Q07_D024_PlatformNaturalScene.request.json";
        private const string ViewRequestGlob = "NTSD28_Q09_C040GameView.request*.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C040-NATURAL-SCENE-001/";
        private const string ViewResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q09-C040-GAMEVIEW-WITNESS-001/";
        private const string PlatformResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-NATURAL-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.C040NaturalScene";
        // FrameInputSet uses legacy physical-action names; its Jump bit reaches
        // the formal attack slot through the existing native input bridge.
        private const SimulationInputButtons FormalAttackButton =
            SimulationInputButtons.Jump;
        private static readonly string[] ProtectedPaths =
        {
            BattleScene,
            MenuScene,
            "Assets/NTSD/Config/GameConfig/GameConfig.asset",
            "Assets/NTSD/Resources/ProjectBattleModeConfig.asset"
        };

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static readonly LF2Character[] actors = new LF2Character[3];
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string mode;
            public string runId;
        }

        [Serializable]
        private sealed class FileHash
        {
            public string path;
            public string sha256;
        }

        [Serializable]
        private sealed class EntitySample
        {
            public int slot;
            public int oid;
            public int action;
            public int state;
            public int counter;
            public int sourceX;
            public int sourceY;
            public int sourceZ;
            public double sourceRuleX;
            public double sourceRuleZ;
            public double viewX;
            public double viewY;
            public double viewZ;
            public int hp;
            public int hold;
            public int catchTarget;
            public int catchSource;
            public int team;
            public bool aiControlled;
            public byte legacyAttack;
            public byte legacyJump;
            public byte legacyDefend;
            public byte nativeAttack;
            public byte nativeJump;
            public byte nativeDefend;
            public int platformYReference;
            public int platformSourceSlot;
            public int platformShadowOffset;
        }

        [Serializable]
        private sealed class TickSample
        {
            public int relativeTick;
            public int globalTick;
            public int inputPhase;
            public int slot0Buttons;
            public int slot1Buttons;
            public int slot2Buttons;
            public uint crtState;
            public ulong crtCalls;
            public int customCounter;
            public int customIndex;
            public ulong customCalls;
            public List<EntitySample> entities = new List<EntitySample>();
        }

        [Serializable]
        private sealed class ViewCapture
        {
            public int relativeTick;
            public int globalTick;
            public string path;
            public string sha256;
            public int width;
            public int height;
            public int fileBytes;
            public int publishedFrameTick;
            public int publishedHitRecordCount;
            public bool sparkResourceAvailable;
            public int sparkCommandCount;
            public List<SparkCommandSample> sparkCommands = new List<SparkCommandSample>();
        }

        [Serializable]
        private sealed class SparkCommandSample
        {
            public int stableId;
            public int pic;
            public int sortOrder;
            public float x;
            public float y;
            public float z;
            public float width;
            public float height;
        }

        [Serializable]
        private sealed class ShadowSample
        {
            public int relativeTick;
            public int globalTick;
            public int publishedTick;
            public int planTick;
            public int sourceOffset;
            public int commandCount;
            public double displayAlpha;
            public double viewHeightOffset;
            public double expectedViewHeightOffset;
            public double residual;
        }

        private static bool IsShadowProjectionProbe => report != null &&
            report.mode == "platform" &&
            report.runId.StartsWith("d024-shadow-height-", StringComparison.Ordinal);

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string mode;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string initialScene;
            public string finalScene;
            public bool initialSceneDirty;
            public bool finalSceneDirty;
            public bool editorPlaying;
            public bool editorCompiling;
            public bool editorUpdating;
            public int sceneCount;
            public bool configuredBeforeStart;
            public bool enteredPlay;
            public bool exitedPlay;
            public bool protectedHashesStable;
            public bool orderedShutdownComplete;
            public bool worldDetached;
            public bool poolQuiesced;
            public string shutdownStatus;
            public string shutdownStage;
            public string shutdownFailure;
            public int remainingWorldObjects = -1;
            public int remainingRuntimeSlots = -1;
            public int remainingPoolBorrowers = -1;
            public int remainingActivePoolObjects = -1;
            public int remainingActivePoolSprites = -1;
            public string contentRoot;
            public int battleMode;
            public int difficulty;
            public int startTick;
            public int endTick;
            public double sourceToViewXOne;
            public double sourceToViewZOne;
            public bool captureGameView;
            public int pendingViewTick;
            public string pendingViewPath;
            public string pendingViewStartedUtc;
            public ViewCapture view;
            public List<FileHash> before = new List<FileHash>();
            public List<FileHash> after = new List<FileHash>();
            public List<TickSample> samples = new List<TickSample>();
            public List<ShadowSample> shadowSamples = new List<ShadowSample>();
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

        private static string PathInProject(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static List<FileHash> HashProtectedFiles()
        {
            var result = new List<FileHash>(ProtectedPaths.Length);
            foreach (string relative in ProtectedPaths)
            {
                using (SHA256 hash = SHA256.Create())
                using (FileStream stream = File.OpenRead(PathInProject(relative)))
                {
                    result.Add(new FileHash
                    {
                        path = relative,
                        sha256 = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "")
                    });
                }
            }
            return result;
        }

        private static void Save()
        {
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
            string root = report.mode == "platform" ? PlatformResultRoot :
                report.captureGameView ? ViewResultRoot : ResultRoot;
            string path = PathInProject(root + report.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static void Restore()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, "");
            if (!string.IsNullOrEmpty(saved)) report = JsonUtility.FromJson<Report>(saved);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null ||
                (report.mode != "run" && report.mode != "triad" &&
                    report.mode != "platform" &&
                    report.mode != "view") ||
                report.phase != "STARTUP" ||
                !EditorApplication.isPlaying || report.configuredBeforeStart ||
                scene.path != BattleScene) return;
            try
            {
                BattleTestBootstrap[] matches = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        value.gameObject.scene == scene && !EditorUtility.IsPersistent(value)).ToArray();
                Require(matches.Length == 1, "Expected one active BattleTestBootstrap in Play clone.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField("overrideCharacterIds",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap overrideCharacterIds is missing.");
                field.SetValue(matches[0], report.mode == "platform"
                    ? new[] { 36, 56, 2 } : report.mode == "triad"
                    ? new[] { 21, 75, 97 } : new[] { 25, 75, 97 });
                report.configuredBeforeStart = true;
                Save();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null ||
                (report.mode != "run" && report.mode != "triad" &&
                    report.mode != "platform" &&
                    report.mode != "view")) return;
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                report.enteredPlay = true;
                Save();
            }
            if (state == PlayModeStateChange.EnteredPlayMode &&
                report.phase == "STARTUP" && !report.configuredBeforeStart)
                Fail("Play clone was not configured before bootstrap Start.");
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
                Finish();
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                Restore();
                if (report == null) { TryStart(); return; }
                if (report.phase == "OPENING") { EnterBattlePlay(); return; }
                if (report.phase == "WAITING_VIEW") { WaitForGameView(); return; }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C040 natural Scene probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void TryStart()
        {
            string viewPath = Directory.GetFiles(PathInProject("Temp"), ViewRequestGlob)
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .FirstOrDefault(candidate =>
                    JsonUtility.FromJson<Request>(File.ReadAllText(candidate))?.requested == true);
            string platformPath = PathInProject(PlatformRequestPath);
            if (!File.Exists(platformPath) ||
                JsonUtility.FromJson<Request>(File.ReadAllText(platformPath))?.requested != true)
                platformPath = null;
            string path = viewPath ?? platformPath ?? PathInProject(RequestPath);
            if (!File.Exists(path)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(path, JsonUtility.ToJson(request, true));
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 80 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require(request.mode == "preflight" || request.mode == "run" ||
                request.mode == "triad" || request.mode == "view" ||
                request.mode == "platform",
                "Invalid request mode.");
            Require(request.mode != "view" || path == viewPath,
                "Game View mode requires its independent request path.");
            Require(request.mode != "platform" || path == platformPath,
                "Platform mode requires its independent request path.");
            string root = request.mode == "platform" ? PlatformResultRoot :
                request.mode == "view" ? ViewResultRoot : ResultRoot;
            Require(!File.Exists(PathInProject(root + request.runId + ".json")),
                "Refusing to overwrite existing result.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase), "Only original project Editor is allowed.");
            Scene scene = SceneManager.GetActiveScene();
            report = new Report
            {
                runId = request.runId,
                mode = request.mode,
                status = "RUNNING",
                phase = "PREFLIGHT",
                startedUtc = DateTime.UtcNow.ToString("O"),
                initialScene = scene.path,
                initialSceneDirty = scene.isDirty,
                editorPlaying = EditorApplication.isPlayingOrWillChangePlaymode,
                editorCompiling = EditorApplication.isCompiling,
                editorUpdating = EditorApplication.isUpdating,
                sceneCount = SceneManager.sceneCount,
                captureGameView = request.mode == "view",
                before = HashProtectedFiles()
            };
            Save();
            if (request.mode == "preflight")
            {
                report.status = "CAPTURED";
                report.phase = "DONE";
                report.finalScene = scene.path;
                report.finalSceneDirty = scene.isDirty;
                report.after = HashProtectedFiles();
                report.protectedHashesStable = HashesMatch();
                Save();
                Clear();
                return;
            }
            Require(!report.editorPlaying && !scene.isDirty && SceneManager.sceneCount == 1 &&
                (scene.path == MenuScene || scene.path == BattleScene),
                "Run requires one clean saved Menu or Battle Scene in Edit Mode.");
            Require(report.before != null && report.before.Count == ProtectedPaths.Length,
                "Protected file baseline is incomplete.");
            report.phase = "OPENING";
            Save();
            if (scene.path == MenuScene)
                EditorSceneManager.OpenScene(BattleScene, OpenSceneMode.Single);
        }

        private static void EnterBattlePlay()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode,
                "Editor entered Play before Scene preflight completed.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Expected sole clean saved Battle Scene.");
            Require(HashesMatch(HashProtectedFiles(), report.before),
                "Protected disk file changed before Battle Play.");
            report.phase = "STARTUP";
            Save();
            EditorApplication.EnterPlaymode();
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart, "Play clone was not configured before Start.");
            driver = SimulationTickDriver.Instance;
            world = driver?.World;
            if (world == null || driver.CurrentTickIndex < 5) return;
            if (!driver.IsPaused) { driver.SetPaused(true); return; }
            if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics) return;
            if (stableTick != driver.CurrentTickIndex)
            {
                stableTick = driver.CurrentTickIndex;
                stableUpdates = 0;
                return;
            }
            if (++stableUpdates < 3) return;
            bool triad = report.mode == "triad";
            bool platform = report.mode == "platform";
            int[] ids = platform ? new[] { 36, 56, 2 } :
                triad ? new[] { 21, 75, 97 } : new[] { 25, 75, 97 };
            int[] sourceX = platform ? new[] { 100, 200, 185 } :
                triad ? new[] { 500, 620, 640 } :
                new[] { 500, 540, 560 };
            int[] actions = triad ? new[] { 415, 73, 0 } :
                new[] { 0, 0, 0 };
            int[] teams = { 1, 2, 1 };
            for (int slot = 0; slot < 3; slot++)
            {
                Require(world.TryResolveRosterInputEntity(slot, out LF2Entity entity) &&
                    entity is LF2Character, "Expected character roster slot " + slot);
                actors[slot] = (LF2Character)entity;
                Require(actors[slot].ObjectId == ids[slot],
                    "Incorrect formal character in slot " + slot);
            }
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            for (int slot = 0; slot < 3; slot++)
            {
                SetInitialActor(actors[slot], sourceX[slot], actions[slot]);
                actors[slot].RelationTeam = teams[slot];
                world.Runtime.Roster.Slots[slot].Team = teams[slot];
            }
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null, "Native world clock unavailable.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            world.NativeRandom.ResetFromSeed(platform ? 0x28A55A5Au :
                triad ? 682973786u : 0u);
            report.battleMode = world.BattleGameModeId;
            report.difficulty = world.Difficulty;
            Require(report.battleMode == 0 && report.difficulty == 0,
                "Unexpected battle mode or difficulty.");
            report.sourceToViewXOne = world.SpatialProjection.SourceDeltaToViewX(1);
            report.sourceToViewZOne = world.SpatialProjection.SourceDeltaToViewZ(1);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            Save();
        }

        private static void SetInitialActor(LF2Character actor, int sourceX,
            int action)
        {
            actor.Initialize(500, 500);
            actor.ImmediateFrame(action);
            actor.Runtime.MP = 500;
            actor.Runtime.PP = 500;
            actor.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(actor.Runtime);
            actor.SwitchDir("right");
            actor.Runtime.Vx = actor.Runtime.Vy = actor.Runtime.Vz = 0;
            actor.Runtime.HP2Orig = 1;
            actor.Runtime.RespawnCount = 0;
            actor.HitStun = 0;
            actor.AttackExempt = 0;
            actor.ItrRest.Reset();
            actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(actor, sourceX, 400);
            Require(actor.Frame.N == action &&
                actor.Runtime.SourceRuleXInt == sourceX &&
                actor.Runtime.YInt == 0 && actor.Runtime.SourceRuleZInt == 400,
                "Initial action or source-rule position was not established.");
        }

        private static EntitySample Capture(LF2Character entity, int slot)
        {
            return new EntitySample
            {
                slot = slot,
                oid = entity.ObjectId,
                action = entity.Frame.N,
                state = entity.Frame.D.state,
                counter = entity.AttackingCounter,
                sourceX = entity.Runtime.SourceRuleXInt,
                sourceY = entity.Runtime.YInt,
                sourceZ = entity.Runtime.SourceRuleZInt,
                sourceRuleX = entity.Runtime.SourceRuleX,
                sourceRuleZ = entity.Runtime.SourceRuleZ,
                viewX = entity.Runtime.X,
                viewY = entity.Runtime.Y,
                viewZ = entity.Runtime.Z,
                hp = entity.Runtime.HP,
                hold = entity.Runtime.FrameDelay,
                catchTarget = entity.Runtime.CaughtSlotIndex,
                catchSource = entity.Runtime.CatchSourceSlot90,
                team = entity.RelationTeam,
                aiControlled = entity.AiControlled,
                legacyAttack = entity.Runtime.KeyAttack,
                legacyJump = entity.Runtime.KeyJump,
                legacyDefend = entity.Runtime.KeyDefend,
                nativeAttack = entity.Runtime.NativeInputProxy.Current[4],
                nativeJump = entity.Runtime.NativeInputProxy.Current[5],
                nativeDefend = entity.Runtime.NativeInputProxy.Current[6],
                platformYReference = entity.Runtime.CollisionYReference,
                platformSourceSlot = entity.Runtime.PlatformSourceSlotF4,
                platformShadowOffset = entity.Runtime.RenderShadowOffset10C
            };
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            int targetTicks = IsShadowProjectionProbe ? 31 : report.mode == "platform" ? 96 :
                report.mode == "triad" ? 16 : 40;
            if (report.samples.Count == targetTicks)
            {
                CompleteMeasurement();
                return;
            }
            int tick = report.samples.Count + 1;
            int next = driver.CurrentTickIndex + 1;
            SimulationInputButtons kakuzu = report.mode == "platform"
                ? tick <= 2 ? SimulationInputButtons.Attack :
                    tick <= 4 ? SimulationInputButtons.Up :
                    tick <= 6 ? FormalAttackButton : SimulationInputButtons.None
                : report.mode == "triad"
                ? SimulationInputButtons.None : tick == 19 || tick == 20
                ? FormalAttackButton
                : tick == 21 || tick == 22 ? SimulationInputButtons.Defend :
                    SimulationInputButtons.None;
            SimulationInputButtons bee = report.mode == "platform" ||
                report.mode == "triad"
                ? SimulationInputButtons.None : tick <= 2 || tick == 9 || tick == 10 ||
                tick == 17 || tick == 18 ? FormalAttackButton :
                    SimulationInputButtons.None;
            SimulationInputButtons target = report.mode == "platform" &&
                (tick == 9 || tick == 10) ? SimulationInputButtons.Defend :
                SimulationInputButtons.None;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, kakuzu),
                new SimulationPlayerInput(1, bee),
                new SimulationPlayerInput(2, target)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var sample = new TickSample
            {
                relativeTick = tick,
                globalTick = driver.CurrentTickIndex,
                inputPhase = world.InputPhase,
                slot0Buttons = (int)kakuzu,
                slot1Buttons = (int)bee,
                slot2Buttons = (int)target,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls
            };
            for (int slot = 0; slot < 3; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                Require(ReferenceEquals(entity, actors[slot]),
                    "Roster entity changed at slot " + slot);
                sample.entities.Add(Capture(actors[slot], slot));
            }
            report.samples.Add(sample);
            report.endTick = driver.CurrentTickIndex;
            if (IsShadowProjectionProbe && tick >= 29 && tick <= 31)
                CapturePlatformShadowPresentation(tick);
            if (report.captureGameView && tick == 25)
            {
                string relativePath = ViewResultRoot + report.runId + "/game-view-tick25.png";
                string output = PathInProject(relativePath);
                Require(!File.Exists(output), "Refusing to overwrite C040 Game View capture.");
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                report.pendingViewTick = tick;
                report.pendingViewPath = relativePath;
                report.pendingViewStartedUtc = DateTime.UtcNow.ToString("O");
                report.phase = "WAITING_VIEW";
                ScreenCapture.CaptureScreenshot(output);
            }
            Save();
        }

        private static void CapturePlatformShadowPresentation(int relativeTick)
        {
            NTSD.Animation.Rendering.BattlePixelFramePlan plan;
            double alpha;
            int renderFps = world.BattlePresentationRenderFps;
            float interval = world.BattlePresentationLogicIntervalSeconds;
            try
            {
                world.ConfigureBattlePresentationDisplayPolicy(30, interval);
                plan = NTSD.Animation.Rendering.BattleCentralRenderSystem.PrepareFrame(world);
                alpha = NTSD.Animation.Rendering.BattleCentralRenderSystem
                    .LastResolvedDisplayAlphaForWorld(world);
            }
            finally
            {
                world.ConfigureBattlePresentationDisplayPolicy(renderFps, interval);
            }
            BattlePresentationFrame frame = plan.CapturedFrame;
            Require(plan.IsValid && !plan.IsStale && plan.SimulationTick == report.endTick &&
                frame != null && frame.TickIndex == report.endTick && frame.CommandsMaterialized &&
                world.BattlePresentation.PublishedFrame?.TickIndex == report.endTick &&
                Math.Abs(alpha - 1.0) < 1e-6,
                "Platform shadow commands are not from the current complete tick.");
            int sourceOffset = actors[2].Runtime.RenderShadowOffset10C;
            int expectedSourceOffset = relativeTick == 29 ? -50 : relativeTick == 30 ? -58 : -64;
            Require(sourceOffset == expectedSourceOffset,
                "Existing natural platform source-height sequence changed.");
            var sample = new ShadowSample
            {
                relativeTick = relativeTick,
                globalTick = report.endTick,
                publishedTick = world.BattlePresentation.PublishedFrame.TickIndex,
                planTick = plan.SimulationTick,
                sourceOffset = sourceOffset,
                displayAlpha = alpha,
                expectedViewHeightOffset = sourceOffset * 1152.0 / 730.0
            };
            NTSDRenderSpace.ViewportTransformSnapshot viewport =
                NTSDRenderSpace.CaptureViewportTransform();
            Vector3 ground = viewport.ScreenPixelToWorld(
                actors[2].GetRuntimeXInt() + (int)actors[2].GetRenderOffsetX() - world.ReleaseCameraX,
                actors[2].GetRenderZInt(), 0f);
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type != BattleRenderCommandType.Shadow ||
                    command.RuntimeSlot != 2 || command.StableId != actors[2].StableId)
                    continue;
                sample.commandCount++;
                sample.viewHeightOffset = -(command.Position.y - ground.y) / viewport.UnitsPerPixelY;
            }
            sample.residual = sample.viewHeightOffset - sample.expectedViewHeightOffset;
            report.shadowSamples.Add(sample);
            Require(sample.commandCount == 1 && Math.Abs(sample.residual) < 0.002,
                "Natural platform shadow did not consume the shared height projection.");
        }

        private static void WaitForGameView()
        {
            Require(report.captureGameView && report.pendingViewTick == 25 &&
                driver.IsPaused && driver.CurrentTickIndex == report.endTick &&
                report.samples.Count == 25,
                "C040 screenshot wait advanced the production simulation.");
            Require(DateTime.UtcNow -
                DateTime.Parse(report.pendingViewStartedUtc).ToUniversalTime() <
                TimeSpan.FromSeconds(90), "C040 Game View screenshot timed out.");
            string output = PathInProject(report.pendingViewPath);
            if (!File.Exists(output) || new FileInfo(output).Length < 32) return;
            byte[] bytes;
            try { bytes = File.ReadAllBytes(output); }
            catch (IOException) { return; }
            Require(bytes.Length >= 32 && bytes[0] == 137 && bytes[1] == 80 &&
                bytes[2] == 78 && bytes[3] == 71 && bytes[4] == 13 &&
                bytes[5] == 10 && bytes[6] == 26 && bytes[7] == 10,
                "C040 Game View capture is not a PNG.");
            int width = bytes[16] << 24 | bytes[17] << 16 | bytes[18] << 8 | bytes[19];
            int height = bytes[20] << 24 | bytes[21] << 16 | bytes[22] << 8 | bytes[23];
            Require(width > 0 && height > 0,
                "C040 Game View dimensions are invalid.");
            var plan = world.CurrentPixelFramePlan;
            BattlePresentationFrame frame = plan.CapturedFrame;
            if (!plan.IsValid || plan.SimulationTick != report.endTick ||
                frame == null || frame.TickIndex != report.endTick ||
                !frame.CommandsMaterialized) return;
            var sparkCommands = new List<SparkCommandSample>();
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand command = frame.GetCommand(index);
                if (command.Type != BattleRenderCommandType.HitRecord) continue;
                sparkCommands.Add(new SparkCommandSample
                {
                    stableId = command.StableId,
                    pic = command.EffectivePic,
                    sortOrder = command.SortOrder,
                    x = command.Position.x,
                    y = command.Position.y,
                    z = command.Position.z,
                    width = command.Size.x,
                    height = command.Size.y
                });
            }
            using (SHA256 hash = SHA256.Create())
            {
                report.view = new ViewCapture
                {
                    relativeTick = 25,
                    globalTick = report.endTick,
                    path = report.pendingViewPath,
                    sha256 = BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", ""),
                    width = width,
                    height = height,
                    fileBytes = bytes.Length,
                    publishedFrameTick = frame.TickIndex,
                    publishedHitRecordCount = frame.HitRecordCount,
                    sparkResourceAvailable = frame.CommonVisualCatalog?.IsSparkValid == true,
                    sparkCommandCount = sparkCommands.Count,
                    sparkCommands = sparkCommands
                };
            }
            report.pendingViewTick = 0;
            report.pendingViewPath = null;
            report.pendingViewStartedUtc = null;
            report.phase = "MEASURING";
            Save();
        }

        private static void CompleteMeasurement()
        {
            report.status = report.samples.Count ==
                (IsShadowProjectionProbe ? 31 : report.mode == "platform" ? 96 :
                    report.mode == "triad" ? 16 : 40) &&
                report.samples.All(value => value.entities.Count == 3) &&
                (!report.captureGameView || report.view != null) &&
                (!IsShadowProjectionProbe || report.shadowSamples.Count == 3)
                ? "CAPTURED" : "INCOMPLETE";
            Save();
            if (!CaptureOrderedShutdown()) return;
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static bool CaptureOrderedShutdown()
        {
            try
            {
                LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in
                        Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap) ||
                            !bootstrap.gameObject.scene.IsValid()) continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }
                report.shutdownStatus = shutdown.Status.ToString();
                report.shutdownStage = shutdown.CompletedStage.ToString();
                report.shutdownFailure = shutdown.FailureReason;
                report.remainingWorldObjects = shutdown.RemainingWorldObjects;
                report.remainingRuntimeSlots = shutdown.RemainingRuntimeSlots;
                report.remainingPoolBorrowers = shutdown.RemainingPoolBorrowers;
                report.remainingActivePoolObjects =
                    pool?.ActiveObjectCountForAcceptance ?? 0;
                report.remainingActivePoolSprites =
                    pool?.ActiveSpriteCountForAcceptance ?? 0;
                report.poolQuiesced = pool == null || pool.IsQuiescedForDiagnostics;
                report.worldDetached = driver.World == null;
                report.orderedShutdownComplete = shutdown.IsComplete &&
                    shutdown.CompletedStage == BattleRuntimeShutdownStage.RuntimeMapCleared &&
                    report.worldDetached && report.poolQuiesced &&
                    report.remainingWorldObjects == 0 &&
                    report.remainingRuntimeSlots == 0 &&
                    report.remainingPoolBorrowers == 0 &&
                    report.remainingActivePoolObjects == 0 &&
                    report.remainingActivePoolSprites == 0;
                if (!report.orderedShutdownComplete)
                    report.error = "Ordered shutdown did not reach zero-residue postconditions.";
            }
            catch (Exception error)
            {
                report.error = "Ordered shutdown exception: " + error;
            }
            if (report.orderedShutdownComplete) return true;
            report.status = "FAIL";
            report.phase = "CLEANUP_BLOCKED";
            Save();
            return false;
        }

        private static bool HashesMatch() => HashesMatch(report.after, report.before);

        private static bool HashesMatch(List<FileHash> current, List<FileHash> baseline)
        {
            return current != null && baseline != null &&
                current.Count == ProtectedPaths.Length &&
                baseline.Count == ProtectedPaths.Length &&
                current.Zip(baseline, (left, right) =>
                    left.path == right.path && left.sha256 == right.sha256).All(equal => equal);
        }

        private static void Fail(string message)
        {
            if (report == null) { Debug.LogError("[Q07 C040 natural Scene] " + message); return; }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = report.enteredPlay;
            Scene scene = SceneManager.GetActiveScene();
            bool battleClean = !report.enteredPlay ||
                (scene.path == BattleScene && !scene.isDirty &&
                    SceneManager.sceneCount == 1);
            report.after = HashProtectedFiles();
            report.protectedHashesStable = HashesMatch();
            if (!battleClean || !report.protectedHashesStable)
            {
                report.status = "FAIL";
                report.error += " Battle Scene or protected disk file changed.";
            }
            if (report.enteredPlay && battleClean && report.initialScene == MenuScene)
                EditorSceneManager.OpenScene(MenuScene, OpenSceneMode.Single);
            scene = SceneManager.GetActiveScene();
            report.finalScene = scene.path;
            report.finalSceneDirty = scene.isDirty;
            if (report.finalScene != report.initialScene ||
                (report.enteredPlay && scene.isDirty))
            {
                report.status = "FAIL";
                report.error += " Initial clean Editor Scene was not restored.";
            }
            report.phase = "DONE";
            Save();
            Clear();
        }

        private static void Clear()
        {
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            for (int slot = 0; slot < actors.Length; slot++) actors[slot] = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
