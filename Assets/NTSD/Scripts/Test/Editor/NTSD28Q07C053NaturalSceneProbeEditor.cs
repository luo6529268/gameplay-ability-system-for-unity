#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07C053NaturalSceneProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C053NaturalScene.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C053-NATURAL-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.C053NaturalScene";
        private const string RunId = "ank610-jira500-natural-scene-02";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character anko;
        private static LF2Character jiraiya;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
        }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int globalTick;
            public int ankoAction;
            public int jiraiyaAction;
            public int ankoX;
            public int jiraiyaX;
            public int attackerCount;
            public int attackerAction50Count;
            public int attackerAction55Count;
            public int attackerSlot = -1;
            public int attackerAction = -1;
            public int attackerX;
            public int attackerY;
            public int attackerZ;
            public int childSlot = -1;
            public int childAction = -1;
            public int childWaitCounter = -1;
            public int childX;
            public int childY;
            public int childZ;
            public int childHp;
            public int victimRestFromAttacker;
            public uint crtState;
            public ulong crtCalls;
            public int customCounter;
            public int customIndex;
            public ulong customCalls;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public int startTick;
            public int endTick;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public int childBirthTick = -1;
            public int attackerBirthTick = -1;
            public int difficulty;
            public int battleMode;
            public List<TickRow> ticks = new List<TickRow>();
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
                field.SetValue(matches[0], new[] { 65, 702 });
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
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C053 natural Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
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
            Scene scene = SceneManager.GetActiveScene();
            if (scene.path != BattleScene || scene.isDirty || SceneManager.sceneCount != 1)
                return;
            string requestFile = PathInProject(RequestPath);
            if (!File.Exists(requestFile)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            if (request == null || !request.requested) return;
            Require(request.runId == RunId, "Unexpected C053 natural run ID.");
            Require(!File.Exists(PathInProject(ResultRoot + RunId + ".json")),
                "Refusing to overwrite an existing C053 natural result.");
            request.requested = false;
            File.WriteAllText(requestFile, JsonUtility.ToJson(request, true));
            report = new Report { runId = RunId, phase = "STARTUP", status = "RUNNING",
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
                first is LF2Character, "OID65 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "OID702 roster entity is missing.");
            anko = (LF2Character)first;
            jiraiya = (LF2Character)second;
            Require(anko.ObjectId == 65 && jiraiya.ObjectId == 702,
                "Play clone roster is not formal OID65/702 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            Require(FindEntity(808, out _) == null && FindEntity(875, out _) == null,
                "Natural child or attacker already occupies the World.");
            SetInitialCharacter(anko, 511, 610);
            SetInitialCharacter(jiraiya, 553, 500);
            anko.RelationTeam = 1;
            jiraiya.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            NTSD28NativeWorldClockState nativeClock = world.Runtime.NativeWorldClock;
            Require(nativeClock != null, "Native world clock is unavailable.");
            nativeClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            report.difficulty = world.Difficulty;
            report.battleMode = world.BattleGameModeId;
            Require(report.battleMode == 0 && report.difficulty == 0,
                "Controlled mode/difficulty does not match source mode0.");
            world.NativeRandom.ResetFromSeed(682973786u);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            Save();
        }

        private static void SetInitialCharacter(LF2Character character, int action, int sourceX)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(action);
            character.Runtime.MP = 500;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.HitStun = 0;
            character.AttackExempt = 0;
            character.ItrRest.Reset();
            character.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(character, sourceX, 400);
            Require(character.Frame.N == action && character.Runtime.SourceRuleXInt == sourceX,
                "Character initial action or source X differs.");
        }

        private static LF2Entity FindEntity(int oid, out int slot)
        {
            for (int index = 50; index < world.RuntimeSlotCapacityForDiagnostics; index++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(index);
                if (entity != null && entity.ObjectId == oid)
                {
                    slot = index;
                    return entity;
                }
            }
            slot = -1;
            return null;
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.ticks.Count == 8) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            LF2Entity child = FindEntity(808, out int childSlot);
            LF2Entity attacker = FindEntity(875, out int attackerSlot);
            int attackerCount = 0;
            int action50Count = 0;
            int action55Count = 0;
            for (int index = 50; index < world.RuntimeSlotCapacityForDiagnostics; index++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(index);
                if (entity == null || entity.ObjectId != 875) continue;
                attackerCount++;
                if (entity.Frame.N == 50) action50Count++;
                if (entity.Frame.N == 55) action55Count++;
            }
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var row = new TickRow
            {
                relativeTick = report.ticks.Count + 1,
                globalTick = driver.CurrentTickIndex,
                ankoAction = anko.Frame.N,
                jiraiyaAction = jiraiya.Frame.N,
                ankoX = anko.Runtime.SourceRuleXInt,
                jiraiyaX = jiraiya.Runtime.SourceRuleXInt,
                attackerCount = attackerCount,
                attackerAction50Count = action50Count,
                attackerAction55Count = action55Count,
                attackerSlot = attackerSlot,
                attackerAction = attacker?.Frame.N ?? -1,
                attackerX = attacker?.Runtime.SourceRuleXInt ?? 0,
                attackerY = attacker?.Runtime.YInt ?? 0,
                attackerZ = attacker?.Runtime.SourceRuleZInt ?? 0,
                childSlot = childSlot,
                childAction = child?.Frame.N ?? -1,
                childWaitCounter = child?.Trans.WaitCounter ?? -1,
                childX = child?.Runtime.SourceRuleXInt ?? 0,
                childY = child?.Runtime.YInt ?? 0,
                childZ = child?.Runtime.SourceRuleZInt ?? 0,
                childHp = child?.Runtime.HP ?? 0,
                victimRestFromAttacker = child == null || attacker == null ? 0 :
                    world.GetRawRestVrest(childSlot, attackerSlot),
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls
            };
            if (child != null && report.childBirthTick < 0)
                report.childBirthTick = row.relativeTick;
            if (attacker != null && report.attackerBirthTick < 0)
                report.attackerBirthTick = row.relativeTick;
            report.ticks.Add(row);
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            TickRow seventh = report.ticks.Single(value => value.relativeTick == 7);
            bool matched = report.childBirthTick == 1 && report.attackerBirthTick == 4 &&
                seventh.childAction == 156 && seventh.childHp == 475 &&
                seventh.victimRestFromAttacker > 0;
            report.status = matched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
            if (!matched)
                report.error = "Natural producer birth or tick7 child action/HP/rest differs from source.";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C053 natural Play] " + message);
                return;
            }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            Save();
            if (EditorApplication.isPlaying)
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
            anko = null;
            jiraiya = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
