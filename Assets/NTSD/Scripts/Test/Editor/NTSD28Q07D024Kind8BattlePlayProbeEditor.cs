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
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07D024Kind8BattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string RequestGlob = "NTSD28_Q07_D024Kind8Battle.request*.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-D024-KIND8-SCENE-001/";
        private const string DirectMotionResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-D024-REPEATED-DIRECT-MOTION-001/scene/";
        private const string SessionKey = "NTSD.Q07.D024Kind8Battle";
        private const int SourceZ = 400;
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
        private static readonly LF2Character[] actors = new LF2Character[2];
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string mode;
            public string runId;
            public bool editorIdleConfirmed;
            public int targetX;
            public string caseName;
            public int startZ;
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
            public int objectType;
            public int action;
            public int counter;
            public int sourceX;
            public int sourceY;
            public int sourceZ;
            public double sourceRuleZ;
            public double viewX;
            public double viewZ;
            public int hp;
            public int team;
        }

        [Serializable]
        private sealed class TickSample
        {
            public int relativeTick;
            public int globalTick;
            public int inputPhase;
            public uint crtState;
            public ulong crtCalls;
            public int customCounter;
            public int customIndex;
            public ulong customCalls;
            public List<EntitySample> entities = new List<EntitySample>();
        }

        [Serializable]
        private sealed class Report
        {
            public int revision;
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
            public int targetX;
            public string caseName;
            public int startZ;
            public bool openedBattle;
            public bool ownsPlay;
            public bool configuredBeforeStart;
            public bool enteredPlay;
            public bool exitedPlay;
            public bool noLiveDriverWorldAfterExit;
            public bool protectedHashesStable;
            public string contentRoot;
            public int battleMode;
            public int difficulty;
            public int startTick;
            public int endTick;
            public double sourceToViewZOne;
            public bool projectedZAssertion;
            public double measuredViewDeltaZ;
            public double maxProjectionError;
            public bool orderedShutdownComplete;
            public string shutdownStatus;
            public string shutdownStage;
            public string shutdownFailure;
            public int remainingWorldObjects = -1;
            public int remainingRuntimeSlots = -1;
            public int remainingPoolBorrowers = -1;
            public int remainingActivePoolObjects = -1;
            public int remainingActivePoolSprites = -1;
            public bool poolQuiesced;
            public bool worldDetached;
            public string resetContract = "Two human roster actors, HP/MP/PP, action, velocity, source/view position, owner/team/link and input history; Flow FrameToggle/InputPhase=0; NativeWorldClock.Reset; NativeRandom seed 0x28A55A5A; difficulty0. Global Driver tick is retained.";
            public string evidenceLimit = "Production Driver and simulation projection in original Battle Scene. No physical keyboard or Game View pixel capture.";
            public List<FileHash> before = new List<FileHash>();
            public List<FileHash> after = new List<FileHash>();
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
            string directory = PathInProject(ResultRootForCase(report.caseName) + report.runId);
            Directory.CreateDirectory(directory);
            string path = Path.Combine(directory, (++report.revision).ToString("D5") + ".json");
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(report, true));
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
        }

        private static string ResultRootForCase(string caseName) =>
            caseName == "direct-motion" ? DirectMotionResultRoot : ResultRoot;

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
            if (report == null || report.mode != "run" || report.phase != "STARTUP" ||
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
                field.SetValue(matches[0], report.caseName == "direct-motion" ?
                    new[] { 92, 2 } : new[] { 7, 2 });
                report.configuredBeforeStart = true;
                Save();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null || report.mode != "run") return;
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
                if (report.enteredPlay && !EditorApplication.isPlayingOrWillChangePlaymode &&
                    (report.phase == "STARTUP" || report.phase == "MEASURING"))
                {
                    Fail("Editor left Play Mode before the kind8 measurement completed.");
                    Finish();
                    return;
                }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                if (report.phase == "CLEANUP_BLOCKED") return;
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "Kind8 Battle Scene probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void TryStart()
        {
            Request request = null;
            foreach (string path in Directory.GetFiles(PathInProject("Temp"), RequestGlob)
                .OrderByDescending(File.GetLastWriteTimeUtc))
            {
                Request candidate = JsonUtility.FromJson<Request>(File.ReadAllText(path));
                if (candidate == null || !candidate.requested ||
                    string.IsNullOrEmpty(candidate.runId) ||
                    Directory.Exists(PathInProject(ResultRootForCase(candidate.caseName) +
                        candidate.runId))) continue;
                request = candidate;
                break;
            }
            if (request == null) return;
            if (SessionState.GetString(SessionKey + ".ConsumedRun", "") == request.runId) return;
            SessionState.SetString(SessionKey + ".ConsumedRun", request.runId);
            Require(request.runId.Length <= 80 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require(request.mode == "preflight" || request.mode == "run", "Invalid request mode.");
            Require(string.IsNullOrEmpty(request.caseName) ||
                request.caseName == "direct-motion", "Invalid diagnostic case.");
            Require(request.caseName == "direct-motion" ? request.targetX == 1200 :
                request.targetX == 480 || request.targetX == 1200,
                "Only the declared near/far target X values are allowed.");
            Require(request.caseName != "direct-motion" ||
                request.startZ == 380 || request.startZ == 400,
                "Only declared direct-motion start Z values are allowed.");
            Require(!Directory.Exists(PathInProject(ResultRootForCase(request.caseName) +
                request.runId)),
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
                targetX = request.targetX,
                caseName = request.caseName,
                startZ = request.startZ,
                initialScene = scene.path,
                initialSceneDirty = scene.isDirty,
                editorPlaying = EditorApplication.isPlayingOrWillChangePlaymode,
                editorCompiling = EditorApplication.isCompiling,
                editorUpdating = EditorApplication.isUpdating,
                sceneCount = SceneManager.sceneCount,
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
            Require(request.editorIdleConfirmed, "Run requires current explicit Editor idle confirmation.");
            Require(!report.editorPlaying && !scene.isDirty && SceneManager.sceneCount == 1 &&
                (scene.path == MenuScene || scene.path == BattleScene),
                "Run requires one clean saved Menu or Battle Scene in Edit Mode.");
            Require(report.before.Count == ProtectedPaths.Length,
                "Protected file baseline is incomplete.");
            report.phase = "OPENING";
            Save();
            if (scene.path == MenuScene)
            {
                EditorSceneManager.OpenScene(BattleScene, OpenSceneMode.Single);
                report.openedBattle = true;
                Save();
            }
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
            report.ownsPlay = true;
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
            bool directMotion = report.caseName == "direct-motion";
            int sourceZ = directMotion ? report.startZ : SourceZ;
            int[] ids = { directMotion ? 92 : 7, 2 };
            int[] sourceX = { 500, report.targetX };
            int[] teams = { 1, 2 };
            for (int slot = 0; slot < actors.Length; slot++)
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
            for (int slot = 0; slot < actors.Length; slot++)
            {
                LF2Character actor = actors[slot];
                actor.Initialize(500, 500);
                actor.ImmediateFrame(0);
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
                actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX[slot]), 0,
                    world.SpatialProjection.SourceToViewZ(sourceZ));
                AppManager.SyncParticipantBirthPosition(actor, sourceX[slot], sourceZ);
                int initialAction = slot == 0 ? (directMotion ? 580 : 24) : 0;
                actor.ImmediateFrame(initialAction);
                actor.AiControlled = false;
                actor.Team = actor.RelationTeam = teams[slot];
                actor.Runtime.OwnerSlotIndex = slot;
                actor.Runtime.LinkState = 0;
                actor.Runtime.HolderStableId = 0;
                actor.Runtime.TargetSlotIndex = 0;
                world.Runtime.Roster.Slots[slot].Team = teams[slot];
                Require(actor.Runtime.SourceRuleXInt == sourceX[slot] &&
                    actor.Runtime.YInt == 0 && actor.Runtime.SourceRuleZInt == sourceZ &&
                    actor.Frame.N == initialAction,
                    "Formal initial actor state mismatch at slot " + slot);
            }
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null, "Native world clock unavailable.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            world.NativeRandom.ResetFromSeed(0x28A55A5Au);
            report.battleMode = world.BattleGameModeId;
            report.difficulty = world.Difficulty;
            Require(report.battleMode == 0 && report.difficulty == 0,
                "Unexpected battle mode or difficulty.");
            report.sourceToViewZOne = world.SpatialProjection.SourceDeltaToViewZ(1);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.samples.Add(CaptureTick(0));
            report.phase = "MEASURING";
            Save();
        }

        private static TickSample CaptureTick(int tick)
        {
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var sample = new TickSample
            {
                relativeTick = tick,
                globalTick = driver.CurrentTickIndex,
                inputPhase = world.InputPhase,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls
            };
            for (int slot = 0; slot < actors.Length; slot++)
            {
                LF2Entity occupant = world.FindEntityByRuntimeSlotForQuery(slot);
                Require(ReferenceEquals(occupant, actors[slot]),
                    "Roster actor replaced or retired at slot " + slot);
                LF2Character actor = actors[slot];
                sample.entities.Add(new EntitySample
                {
                    slot = slot,
                    oid = actor.ObjectId,
                    objectType = actor.GetCurrentDataObjectTypeForSimulation(),
                    action = actor.Frame.N,
                    counter = actor.AttackingCounter,
                    sourceX = actor.Runtime.SourceRuleXInt,
                    sourceY = actor.Runtime.YInt,
                    sourceZ = actor.Runtime.SourceRuleZInt,
                    sourceRuleZ = actor.Runtime.SourceRuleZ,
                    viewX = actor.Runtime.X,
                    viewZ = actor.Runtime.Z,
                    hp = actor.Runtime.HP,
                    team = actor.RelationTeam
                });
            }
            return sample;
        }

        private static void MeasureOneTick()
        {
            Require(driver != null && ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.samples.Count == (report.caseName == "direct-motion" ? 25 : 13))
            { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            Require(driver.CurrentTickIndex == next, "Driver did not advance exactly one tick.");
            report.samples.Add(CaptureTick(report.samples.Count));
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            if (report.caseName == "direct-motion")
            {
                report.maxProjectionError = report.samples.Max(sample =>
                    Math.Abs(sample.entities[0].viewZ -
                        world.SpatialProjection.SourceToViewZ(sample.entities[0].sourceRuleZ)));
                report.projectedZAssertion = report.samples.Count == 25 &&
                    report.samples.All(sample => sample.entities.Count == 2 &&
                        sample.entities[0].oid == 92 && sample.entities[0].action == 580 &&
                        sample.entities[0].sourceZ == report.startZ + 4 * sample.relativeTick &&
                        sample.entities[1].oid == 2 &&
                        sample.entities[1].sourceZ == report.startZ) &&
                    report.maxProjectionError < 1.0;
                report.status = report.projectedZAssertion ? "CAPTURED" : "MISMATCH";
                if (!report.projectedZAssertion)
                    report.error = "Repeated frame580 source/view Z projection mismatch.";
                Save();
                if (!CaptureOrderedShutdown()) return;
                report.phase = "EXITING";
                Save();
                EditorApplication.ExitPlaymode();
                return;
            }
            TickSample first = report.samples[1];
            EntitySample lee = first.entities[0];
            EntitySample naruto = first.entities[1];
            report.measuredViewDeltaZ = lee.viewZ - naruto.viewZ;
            int expectedDelta = report.targetX == 480 ? 1 : 0;
            report.projectedZAssertion = lee.sourceZ - naruto.sourceZ == expectedDelta &&
                Math.Abs(report.measuredViewDeltaZ -
                    expectedDelta * report.sourceToViewZOne) <= 1.0;
            report.status = report.samples.Count == 13 &&
                report.samples.All(value => value.entities.Count == 2) &&
                report.projectedZAssertion ? "CAPTURED" : "MISMATCH";
            if (!report.projectedZAssertion)
                report.error = "Tick1 source/view Z relation does not match the near/far projection contract.";
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
                    foreach (BattleBootstrap bootstrap in Resources.FindObjectsOfTypeAll<BattleBootstrap>())
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
                report.remainingActivePoolObjects = pool?.ActiveObjectCountForAcceptance ?? 0;
                report.remainingActivePoolSprites = pool?.ActiveSpriteCountForAcceptance ?? 0;
                report.poolQuiesced = pool == null || pool.IsQuiescedForDiagnostics;
                report.worldDetached = driver.World == null;
                report.orderedShutdownComplete = shutdown.IsComplete &&
                    shutdown.CompletedStage == BattleRuntimeShutdownStage.RuntimeMapCleared &&
                    report.worldDetached && report.poolQuiesced &&
                    report.remainingWorldObjects == 0 && report.remainingRuntimeSlots == 0 &&
                    report.remainingPoolBorrowers == 0 && report.remainingActivePoolObjects == 0 &&
                    report.remainingActivePoolSprites == 0;
                if (!report.orderedShutdownComplete)
                    report.error = "Ordered shutdown did not reach zero-residue postconditions.";
            }
            catch (Exception error) { report.error = "Ordered shutdown exception: " + error; }
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
            if (report == null) { Debug.LogError("[Q07 D024 kind8 Scene] " + message); return; }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (report.ownsPlay && EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = report.enteredPlay;
            report.noLiveDriverWorldAfterExit = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                .All(value => value == null || value.World == null);
            if (report.enteredPlay && !report.noLiveDriverWorldAfterExit)
            {
                report.status = "FAIL";
                report.error += " Live Driver World remains after exit.";
            }
            Scene scene = SceneManager.GetActiveScene();
            bool battleClean = !report.enteredPlay ||
                (scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1);
            report.after = HashProtectedFiles();
            report.protectedHashesStable = HashesMatch();
            if (!battleClean || !report.protectedHashesStable)
            {
                report.status = "FAIL";
                report.error += " Battle Scene or protected disk file changed.";
            }
            if (report.openedBattle && !scene.isDirty && battleClean && report.initialScene == MenuScene)
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
            report.after = HashProtectedFiles();
            report.protectedHashesStable = HashesMatch();
            if (!report.protectedHashesStable)
            {
                report.status = "FAIL";
                report.error += " Protected file changed during restoration.";
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
