#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q08C009ReturningGroupBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q08_C009ReturningGroupBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q08-C009-RETURNING-GROUP-001/";
        private const string SessionKey = "NTSD.Q08.C009ReturningGroupBattlePlay";
        private static readonly string[] ProtectedPaths =
        {
            BattleScene,
            "Assets/NTSD/Scene/NTSD_Menu.unity",
            "Assets/NTSD/Config/GameConfig/GameConfig.asset",
            "Assets/NTSD/Resources/ProjectBattleModeConfig.asset"
        };

        [Serializable] private sealed class Request { public bool requested; public string runId; }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int globalTick;
            public int timer;
            public int outputTimer;
            public ulong groupMask;
            public int emitterAction;
            public int group2CharacterCount;
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
            public string[] hashesBefore;
            public string[] hashesAfter;
            public int startTick;
            public int endTick;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public List<TickRow> rows = new List<TickRow>();
        }

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character survivor;
        private static LF2Entity emitter;
        private static int stableTick = -1;
        private static int stableUpdates;

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

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string Hash(string relative)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(ProjectPath(relative)))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static string[] HashProtected() => ProtectedPaths.Select(Hash).ToArray();

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }

        private static void Save()
        {
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));
            string path = ProjectPath(ResultRoot + report.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, JsonUtility.ToJson(report, true));
        }

        private static void Restore()
        {
            if (report != null) return;
            string stored = SessionState.GetString(SessionKey, "");
            if (!string.IsNullOrEmpty(stored)) report = JsonUtility.FromJson<Report>(stored);
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
                field.SetValue(matches[0], new[] { 56 });
                report.configuredBeforeStart = true;
                Save();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            Restore();
            if (report == null) return;
            if (state == PlayModeStateChange.EnteredPlayMode && report.phase == "STARTUP" &&
                !report.configuredBeforeStart) Fail("Play clone was not configured before bootstrap Start.");
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING") Finish();
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
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C009 Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected C009 probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void TryStart()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                !string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                    "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                    StringComparison.OrdinalIgnoreCase)) return;
            string requestPath = ProjectPath(RequestPath);
            if (!File.Exists(requestPath)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(requestPath, Encoding.UTF8));
            if (request == null || !request.requested) return;
            request.requested = false;
            File.WriteAllText(requestPath, JsonUtility.ToJson(request, true));
            Require(request.runId == "c009-oid304-return-scene-v1", "Unexpected C009 runId.");
            Require(!File.Exists(ProjectPath(ResultRoot + request.runId + ".json")),
                "Refusing to overwrite existing C009 Scene result.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Requires the sole clean saved Battle Scene.");
            report = new Report { runId = request.runId, status = "RUNNING", phase = "STARTUP",
                startedUtc = DateTime.UtcNow.ToString("O"), hashesBefore = HashProtected() };
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
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity entity) &&
                entity is LF2Character, "OID56 roster character is missing.");
            survivor = (LF2Character)entity;
            Require(survivor.ObjectId == 56, "Play roster is not formal OID56.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World is not using staged formal content.");
            survivor.Initialize(500, 500);
            survivor.ImmediateFrame(0);
            survivor.ClearBattleEntryInputState();
            survivor.SwitchDir("right");
            survivor.Runtime.Vx = survivor.Runtime.Vy = survivor.Runtime.Vz = 0;
            survivor.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(500), 0,
                world.SpatialProjection.SourceToViewZ(650));
            AppManager.SyncParticipantBirthPosition(survivor, 500, 650);
            survivor.Team = survivor.RelationTeam = 1;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Match.Difficulty = 0;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Results.ResetNativeResultFlow();
            world.NativeRandom.ResetFromSeed(682973786u);
            emitter = SpawnEmitter();
            Require(emitter.ObjectId == 304 && emitter.Runtime.EntityType == 3 &&
                emitter.RelationTeam == 2 && emitter.Frame.N == 11,
                "Formal OID304/type3/action11/team2 was not established.");
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            Save();
        }

        private static LF2Entity SpawnEmitter()
        {
            Require(world.FindEntityByRuntimeSlotForQuery(50) == null,
                "Required OID304 slot50 is occupied.");
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint { kind = 1, oid = 304, action = 11, facing = 0 };
            task.targetWorld = world;
            task.requiredRuntimeSlot = 50;
            task.team = 2;
            task.relationTeam = 2;
            task.useExplicitRelationIdentity = true;
            task.ownerEntityIndex = 1;
            task.dir = "right";
            task.preserveActionZero = true;
            task.skipPostInitZOffset = true;
            task.useDirectRuntimePosition = true;
            task.directX = world.SpatialProjection.SourceToViewX(900);
            task.directY = 0;
            task.directZ = world.SpatialProjection.SourceToViewZ(650);
            task.useExplicitInitialVitals = true;
            task.initialHp = 500;
            task.initialMp = 500;
            LF2Entity result;
            try { result = LF2ObjectPointFactory.Instance.CreateObjectImmediate(task); }
            finally { LF2ReferencePool.Instance.Recycle(task); }
            Require(result != null && result.Runtime.SlotIndex == 50,
                "Production object factory did not spawn OID304 at slot50.");
            result.Team = result.RelationTeam = 2;
            result.ImmediateFrame(11);
            result.FrameDelay = 0;
            result.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(900), 0,
                world.SpatialProjection.SourceToViewZ(650));
            result.Runtime.SyncIntegerPosition();
            result.RefreshRuntimeSnapshot();
            return result;
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production Driver tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.rows.Count == 12) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected tick " + next);
            BattleResultsRuntimeState results = world.Runtime.Results;
            int children = 0;
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity != null && entity.ObjectId == 56 && entity.Runtime.EntityType == 0 &&
                    entity.RelationTeam == 2 && entity.Runtime.HP > 0) children++;
            }
            report.rows.Add(new TickRow
            {
                relativeTick = report.rows.Count + 1,
                globalTick = driver.CurrentTickIndex,
                timer = results.NativeResultTimer,
                outputTimer = results.NativeResultOutputTimer,
                groupMask = results.NativeLivingGroupMask,
                emitterAction = emitter == null ? -1 : emitter.Frame.N,
                group2CharacterCount = children
            });
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            bool pass = report.rows.Count == 12;
            for (int index = 0; index < report.rows.Count; index++)
            {
                TickRow row = report.rows[index];
                int expectedTimer = index < 2 ? index + 1 : 2;
                ulong expectedMask = index < 2 ? 1UL << 1 : (1UL << 1) | (1UL << 2);
                pass &= row.timer == expectedTimer && row.outputTimer == expectedTimer &&
                    row.groupMask == expectedMask &&
                    row.group2CharacterCount == (index == 0 ? 0 : 1);
            }
            report.status = pass ? "PASS" : "DIFFERENCE";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null) { Debug.LogError("[Q08 C009 Battle Play] " + message); return; }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = true;
            report.hashesAfter = HashProtected();
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                report.hashesBefore.SequenceEqual(report.hashesAfter);
            if (!report.sceneCleanAfter)
            {
                report.status = "FAIL";
                report.error += " Saved Battle Scene or protected asset changed.";
            }
            report.phase = "DONE";
            Save();
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            survivor = null;
            emitter = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
