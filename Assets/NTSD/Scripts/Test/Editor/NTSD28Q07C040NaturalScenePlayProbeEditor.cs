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
    internal static class NTSD28Q07C040NaturalScenePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C040NaturalScene.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C040-NATURAL-SCENE-001/";
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
        }

        [Serializable]
        private sealed class TickSample
        {
            public int relativeTick;
            public int globalTick;
            public int inputPhase;
            public int slot0Buttons;
            public int slot1Buttons;
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
            public string contentRoot;
            public int battleMode;
            public int difficulty;
            public int startTick;
            public int endTick;
            public double sourceToViewXOne;
            public double sourceToViewZOne;
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
            string path = PathInProject(ResultRoot + report.runId + ".json");
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
                field.SetValue(matches[0], new[] { 25, 75, 97 });
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
            string path = PathInProject(RequestPath);
            if (!File.Exists(path)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(path, JsonUtility.ToJson(request, true));
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 80 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require(request.mode == "preflight" || request.mode == "run", "Invalid request mode.");
            Require(!File.Exists(PathInProject(ResultRoot + request.runId + ".json")),
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
            int[] ids = { 25, 75, 97 };
            int[] sourceX = { 500, 540, 560 };
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
                SetInitialActor(actors[slot], sourceX[slot]);
                actors[slot].RelationTeam = teams[slot];
                world.Runtime.Roster.Slots[slot].Team = teams[slot];
            }
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null, "Native world clock unavailable.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            world.NativeRandom.ResetFromSeed(0u);
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

        private static void SetInitialActor(LF2Character actor, int sourceX)
        {
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
            actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(actor, sourceX, 400);
            Require(actor.Frame.N == 0 && actor.Runtime.SourceRuleXInt == sourceX &&
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
                nativeDefend = entity.Runtime.NativeInputProxy.Current[6]
            };
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.samples.Count == 40) { CompleteMeasurement(); return; }
            int tick = report.samples.Count + 1;
            int next = driver.CurrentTickIndex + 1;
            SimulationInputButtons kakuzu = tick == 19 || tick == 20
                ? FormalAttackButton
                : tick == 21 || tick == 22 ? SimulationInputButtons.Defend :
                    SimulationInputButtons.None;
            SimulationInputButtons bee = tick <= 2 || tick == 9 || tick == 10 ||
                tick == 17 || tick == 18 ? FormalAttackButton :
                    SimulationInputButtons.None;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, kakuzu),
                new SimulationPlayerInput(1, bee),
                new SimulationPlayerInput(2, SimulationInputButtons.None)
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
            Save();
        }

        private static void CompleteMeasurement()
        {
            report.status = report.samples.Count == 40 &&
                report.samples.All(value => value.entities.Count == 3)
                ? "CAPTURED" : "INCOMPLETE";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
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
