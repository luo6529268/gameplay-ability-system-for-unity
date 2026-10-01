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
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07C044KisameNaturalBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C044KisameNaturalBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C044-KISAME-NATURAL-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.C044KisameNaturalBattlePlay";
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
            public int targetX;
        }

        [Serializable]
        private sealed class EntitySample
        {
            public int slot;
            public int oid;
            public int type;
            public int action;
            public int state;
            public int counter;
            public int x;
            public int y;
            public int z;
            public double vx;
            public double vy;
            public double vz;
            public int hp;
            public int frameDelay;
            public int linkState;
            public int caughtSlot;
            public int caughtDuration;
            public int holderSlot;
            public int inputHpConsumed;
            public int owner;
            public int team;
            public int environmentState320;
            public int reviveLives;
            public int reviveNextHp;
        }

        [Serializable]
        private sealed class Sample
        {
            public int relativeTick;
            public int globalTick;
            public uint crtState;
            public ulong crtCalls;
            public int customCounter;
            public int customIndex;
            public ulong customCalls;
            public uint lastCallSite;
            public List<EntitySample> entities = new List<EntitySample>();
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public int actorAction;
            public int targetX;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string contentRoot;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public int startTick;
            public int endTick;
            public int inputPhase;
            public int resourcePhase12Before;
            public int resourcePhase3Before;
            public ulong frameSequenceBefore;
            public int resourcePhase12AtStart;
            public int resourcePhase3AtStart;
            public ulong frameSequenceAtStart;
            public int difficultyBefore;
            public int difficultyEffective;
            public int battleMode;
            public int localMode;
            public int aiPhaseGate;
            public int primarySlot = -1;
            public int firstBirthRelativeTick = -1;
            public int initialY;
            public bool configuredBeforeStart;
            public bool sceneCleanAfter;
            public bool exitedPlay;
            public List<Sample> samples = new List<Sample>();
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

        private static string HashScene()
        {
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(PathInProject(BattleScene)))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
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
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Restore();
            if (report == null || report.phase != "STARTUP" || !EditorApplication.isPlaying ||
                report.configuredBeforeStart || scene.path != BattleScene) return;
            try
            {
                BattleTestBootstrap[] matches = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        value.gameObject.scene == scene && !EditorUtility.IsPersistent(value)).ToArray();
                Require(matches.Length == 1, "Expected one active BattleTestBootstrap in Play clone.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField("overrideCharacterIds",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap overrideCharacterIds is missing.");
                field.SetValue(matches[0], new[] { 17, 2 });
                report.configuredBeforeStart = true;
                Save();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null) return;
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
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() < TimeSpan.FromMinutes(10),
                    "Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void TryStart()
        {
            string requestFile = PathInProject(RequestPath);
            if (!File.Exists(requestFile)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(requestFile, JsonUtility.ToJson(request, true));
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.Length <= 80 &&
                request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid runId.");
            Require((request.targetX == 550 && request.runId == "kis17-a314-x550-natural-scene-01") ||
                (request.targetX == 1200 && request.runId == "kis17-a314-x1200-natural-scene-01"),
                "Unexpected controlled C044 request.");
            Require(!File.Exists(PathInProject(ResultRoot + request.runId + ".json")),
                "Refusing to overwrite an existing result.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode, "Editor must be in Edit Mode.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                StringComparison.OrdinalIgnoreCase), "Only the original project Editor is allowed.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires sole clean saved NTSD_Battle Scene.");
            report = new Report { runId = request.runId, phase = "STARTUP", status = "RUNNING",
                startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene(),
                initialY = 0,
                actorAction = 314,
                targetX = request.targetX };
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
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity first) &&
                first is LF2Character, "Kisame roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "Second Naruto roster entity is missing.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            Require(actor.ObjectId == 17 && target.ObjectId == 2,
                "Play clone roster is not formal OID17/2 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            SetInitialActor(actor, report.actorAction, 500, 500, 0);
            SetInitialActor(target, 0, report.targetX, 500, 0);
            actor.RelationTeam = 1;
            target.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            NTSD28NativeWorldClockState nativeClock = world.Runtime.NativeWorldClock;
            Require(nativeClock != null, "Native world clock is unavailable.");
            report.resourcePhase12Before = nativeClock.ResourcePhase12;
            report.resourcePhase3Before = nativeClock.ResourcePhase3;
            report.frameSequenceBefore = nativeClock.FrameSequence;
            nativeClock.Reset();
            report.resourcePhase12AtStart = nativeClock.ResourcePhase12;
            report.resourcePhase3AtStart = nativeClock.ResourcePhase3;
            report.frameSequenceAtStart = nativeClock.FrameSequence;
            report.difficultyBefore = world.Difficulty;
            world.Runtime.Match.Difficulty = 0;
            report.difficultyEffective = world.Difficulty;
            report.battleMode = world.BattleGameModeId;
            report.localMode = world.LocalGameModeId;
            report.aiPhaseGate = world.AiPhaseGate;
            world.NativeRandom.ResetFromSeed(682973786u);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.inputPhase = world.InputPhase;
            report.phase = "MEASURING";
            Save();
        }

        private static void SetInitialActor(LF2Character actor, int action, int sourceX, int hp, int initialY)
        {
            actor.Initialize(hp, 500);
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
            actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), initialY,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(actor, sourceX, 400);
            Require(actor.Frame.N == action && actor.Runtime.SourceRuleXInt == sourceX &&
                actor.Runtime.YInt == initialY && actor.Runtime.EnvironmentState320 == 0,
                "Initial action, Y, environment marker or source-rule position was not established.");
        }

        private static EntitySample Capture(LF2Entity entity, int slot)
        {
            return new EntitySample
            {
                slot = slot,
                oid = entity.ObjectId,
                type = entity.Runtime.EntityType,
                action = entity.Frame.N,
                state = entity.Frame.D.state,
                counter = entity.AttackingCounter,
                x = entity.Runtime.SourceRuleXInt,
                y = entity.Runtime.YInt,
                z = entity.Runtime.SourceRuleZInt,
                vx = entity.Runtime.Vx,
                vy = entity.Runtime.Vy,
                vz = entity.Runtime.Vz,
                hp = entity.Runtime.HP,
                frameDelay = entity.Runtime.FrameDelay,
                linkState = entity.Runtime.LinkState,
                caughtSlot = entity.Runtime.CaughtSlotIndex,
                caughtDuration = entity.Runtime.CaughtDuration,
                holderSlot = entity.Runtime.HolderStableId,
                inputHpConsumed = entity.Runtime.InputHpConsumedTotal34C,
                owner = entity.Runtime.OwnerSlotIndex,
                team = entity.RelationTeam,
                environmentState320 = entity.Runtime.EnvironmentState320,
                reviveLives = entity.Runtime.HP2Orig,
                reviveNextHp = entity.Runtime.RespawnCount
            };
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.samples.Count == 60) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var sample = new Sample
            {
                relativeTick = report.samples.Count + 1,
                globalTick = driver.CurrentTickIndex,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls,
                lastCallSite = rng.LastSynchronizedCallSite
            };
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null || slot > 1) continue;
                EntitySample captured = Capture(entity, slot);
                sample.entities.Add(captured);
            }
            report.samples.Add(sample);
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            report.status = report.samples.Count == 60 &&
                report.samples.All(value => value.entities.Count == 2)
                ? "CAPTURED" : "INCOMPLETE";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null) { Debug.LogError("[Q07 C044 Kisame natural Play] " + message); return; }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                EditorApplication.ExitPlaymode();
            else if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = true;
            report.sceneHashAfter = HashScene();
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                report.sceneHashAfter == report.sceneHashBefore;
            if (!report.sceneCleanAfter)
            {
                report.status = "FAIL";
                report.error += " Saved Battle Scene or active Scene state changed.";
            }
            report.phase = "DONE";
            Save();
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            actor = null;
            target = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
