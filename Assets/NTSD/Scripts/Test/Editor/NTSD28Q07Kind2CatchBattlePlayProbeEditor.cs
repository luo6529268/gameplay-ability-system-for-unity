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
    internal static class NTSD28Q07Kind2CatchBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_Kind2CatchBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C022-C029-KIND2-SCENE-PLAY-001/";
        private const string SessionKey = "NTSD.Q07.Kind2CatchBattlePlay";
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
        }

        [Serializable]
        private sealed class EntitySample
        {
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
            public int catchTarget;
            public int catchSource;
            public int catchTimeout;
            public int motionHold;
            public int interaction;
            public int cpointKind;
        }

        [Serializable]
        private sealed class Sample
        {
            public int relativeTick;
            public int globalTick;
            public EntitySample actor;
            public EntitySample target;
            public int targetJump;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
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
            public int firstCatchRelativeTick = -1;
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
                field.SetValue(matches[0], new[] { 52, 52 });
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
                startedUtc = DateTime.UtcNow.ToString("O"), sceneHashBefore = HashScene() };
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
                first is LF2Character, "First Naruto one-tail roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "Second Naruto one-tail roster entity is missing.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            Require(actor.ObjectId == 52 && target.ObjectId == 52,
                "Play clone roster is not formal OID52 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            SetInitialActor(actor, 255, 500, 500);
            SetInitialActor(target, 0, 650, 500);
            actor.RelationTeam = 1;
            target.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            world.NativeRandom.ResetFromSeed(682973786u);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.inputPhase = world.InputPhase;
            report.phase = "MEASURING";
            Save();
        }

        private static void SetInitialActor(LF2Character actor, int action, int sourceX, int hp)
        {
            actor.Initialize(hp, 500);
            actor.ImmediateFrame(action);
            actor.Runtime.MP = 500;
            actor.Runtime.PP = 500;
            actor.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(actor.Runtime);
            actor.SwitchDir("right");
            actor.Runtime.Vx = actor.Runtime.Vy = actor.Runtime.Vz = 0;
            actor.HitStun = 0;
            actor.AttackExempt = 0;
            actor.ItrRest.Reset();
            actor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(actor, sourceX, 400);
            Require(actor.Frame.N == action && actor.Runtime.SourceRuleXInt == sourceX,
                "Initial action or source-rule position was not established.");
        }

        private static EntitySample Capture(LF2Character entity)
        {
            return new EntitySample
            {
                action = entity.Frame.N,
                state = entity.Frame.D.state,
                counter = entity.AttackingCounter,
                x = entity.Runtime.SourceRuleXInt,
                y = entity.Runtime.YInt,
                z = entity.Runtime.SourceRuleZInt,
                vx = entity.Runtime.Vx,
                vy = entity.Runtime.Vy,
                vz = entity.Runtime.Vz,
                hp = entity.Health.HP,
                catchTarget = entity.CaughtSlotIndex,
                catchSource = entity.Runtime.CatchSourceSlot90,
                catchTimeout = entity.Runtime.CaughtDuration,
                motionHold = entity.FrameDelay,
                interaction = entity.Runtime.LinkState,
                cpointKind = entity.Frame.D.HasPrimaryCatchPoint
                    ? entity.Frame.D.PrimaryCatchPoint.Kind : -1
            };
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.samples.Count == 32) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            int relativeTick = report.samples.Count + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, relativeTick <= 4
                    ? SimulationInputButtons.Defend : SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            var sample = new Sample
            {
                relativeTick = relativeTick,
                globalTick = driver.CurrentTickIndex,
                actor = Capture(actor),
                target = Capture(target),
                targetJump = target.Runtime.NativeInputProxy.Current[5]
            };
            if (sample.actor.catchTarget == 1 && sample.target.catchSource == 0 &&
                report.firstCatchRelativeTick < 0)
                report.firstCatchRelativeTick = relativeTick;
            report.samples.Add(sample);
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            EntitySample caught = report.samples[8].target;
            EntitySample held = report.samples[9].target;
            report.status = report.firstCatchRelativeTick == 9 &&
                caught.action == 130 && caught.state == 1700 && caught.cpointKind == 2 &&
                caught.counter == 1 && Math.Abs(caught.vy) > 1 && held.catchSource == 0 &&
                held.counter == caught.counter && Math.Abs(held.vy - caught.vy) < 1e-8 &&
                held.motionHold == 0 && held.interaction == 0 ? "PASS" : "DIFFERENCE";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null) { Debug.LogError("[Q07 Kind2 catch Play] " + message); return; }
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
