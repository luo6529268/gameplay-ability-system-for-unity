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
using NTSD.App;
using NTSD.Game;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07C050VerticalSkipScenePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string GameConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ModeAsset = "Assets/NTSD/Resources/ProjectBattleModeConfig.asset";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q07_C050VerticalScenePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C050-SCENE-PLAY-001/";
        private const string SessionKey = "NTSD.Q07.C050VerticalScenePlay";

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
            public int targetX;
            public string runId;
        }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int globalTick;
            public int actorAction;
            public int actorHp;
            public double actorVy;
            public int actorMotionHold;
            public int targetAction;
            public int targetHp;
            public double targetVy;
            public int targetMotionHold;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public int targetX;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string contentRoot;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public int startTick;
            public int endTick;
            public string menuHashBefore;
            public string battleHashBefore;
            public string gameConfigHashBefore;
            public string modeHashBefore;
            public string menuHashAfter;
            public string battleHashAfter;
            public string gameConfigHashAfter;
            public string modeHashAfter;
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

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string HashFile(string relative)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(ProjectPath(relative)))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void SaveSession() =>
            SessionState.SetString(SessionKey, JsonUtility.ToJson(report));

        private static void RestoreSession()
        {
            if (report != null) return;
            string saved = SessionState.GetString(SessionKey, "");
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                RestoreSession();
                if (report == null) { PollRequest(); return; }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C050 Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected C050 probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void PollRequest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string path = ProjectPath(RequestPath);
            if (!File.Exists(path)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(path, Encoding.UTF8));
            Require(request != null && request.requested &&
                (request.targetX == 520 || request.targetX == 1200) &&
                request.runId == (request.targetX == 520 ?
                    "x520-scene-v1" : "x1200-scene-v1"),
                "Unexpected C050 Scene request; leaving it untouched.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "C050 Scene request requires one clean original Battle Scene.");
            Require(!File.Exists(ProjectPath(ResultRoot + request.runId + ".json")),
                "Refusing to overwrite an existing C050 result.");
            File.Delete(path);
            Start(request);
        }

        private static void Start(Request request)
        {
            Require(report == null && !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Editor is busy or another C050 probe is active.");
            report = new Report
            {
                runId = request.runId,
                targetX = request.targetX,
                status = "RUNNING",
                phase = "STARTUP",
                startedUtc = DateTime.UtcNow.ToString("O"),
                menuHashBefore = HashFile(MenuScene),
                battleHashBefore = HashFile(BattleScene),
                gameConfigHashBefore = HashFile(GameConfigAsset),
                modeHashBefore = HashFile(ModeAsset)
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RestoreSession();
            if (report == null || report.phase != "STARTUP" || !EditorApplication.isPlaying ||
                report.configuredBeforeStart || scene.path != BattleScene) return;
            try
            {
                BattleTestBootstrap[] matches = Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                    .Where(value => value != null && value.isActiveAndEnabled &&
                        value.gameObject.scene == scene && !EditorUtility.IsPersistent(value)).ToArray();
                Require(matches.Length == 1, "Expected one BattleTestBootstrap in Play clone.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField("overrideCharacterIds",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap overrideCharacterIds is missing.");
                field.SetValue(matches[0], new[] { 24, 56 });
                report.configuredBeforeStart = true;
                SaveSession();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void OnPlayMode(PlayModeStateChange state)
        {
            RestoreSession();
            if (report == null) return;
            if (state == PlayModeStateChange.EnteredPlayMode &&
                report.phase == "STARTUP" && !report.configuredBeforeStart)
                Fail("Play clone was not configured before bootstrap Start.");
            if (state == PlayModeStateChange.EnteredEditMode && report.phase == "EXITING")
                Finish();
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
                first is LF2Character, "OID24 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "OID56 roster entity is missing.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            Require(actor.ObjectId == 24 && target.ObjectId == 56,
                "Play clone roster is not formal OID24/56 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == ContentRoot, "Play World did not use formal content.");
            SetInitialCharacter(actor, 37, 500);
            SetInitialCharacter(target, 259, report.targetX);
            actor.RelationTeam = 1;
            target.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null, "Native world clock is missing.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            Require(world.BattleGameModeId == 0 && world.Difficulty == 0,
                "Battle mode or difficulty differs from formal mode0.");
            world.NativeRandom.ResetFromSeed(682973786u);
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            SaveSession();
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
                "Initial action or source X differs.");
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is unstable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.ticks.Count == 3) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            report.ticks.Add(new TickRow
            {
                relativeTick = report.ticks.Count + 1,
                globalTick = driver.CurrentTickIndex,
                actorAction = actor.Frame.N,
                actorHp = actor.Health.HP,
                actorVy = actor.Runtime.Vy,
                actorMotionHold = actor.Runtime.FrameDelay,
                targetAction = target.Frame.N,
                targetHp = target.Health.HP,
                targetVy = target.Runtime.Vy,
                targetMotionHold = target.Runtime.FrameDelay
            });
            report.endTick = driver.CurrentTickIndex;
            SaveSession();
        }

        private static void CompleteMeasurement()
        {
            TickRow second = report.ticks.Single(value => value.relativeTick == 2);
            TickRow third = report.ticks.Single(value => value.relativeTick == 3);
            bool matched = report.targetX == 520
                ? second.targetAction == 259 && second.targetHp == 465 &&
                  second.targetVy == 0 && third.targetAction == 259 &&
                  third.targetHp == 465
                : second.targetHp == 500 && third.targetHp == 500;
            report.status = matched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
            if (!matched)
                report.error = "C050 near hit or far control differs; inspect all tick rows.";
            report.phase = "EXITING";
            SaveSession();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C050 Scene Play] " + message);
                return;
            }
            report.status = "FAIL";
            report.error = message;
            report.phase = "EXITING";
            SaveSession();
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static void Finish()
        {
            if (report == null || EditorApplication.isPlayingOrWillChangePlaymode) return;
            report.exitedPlay = true;
            report.menuHashAfter = HashFile(MenuScene);
            report.battleHashAfter = HashFile(BattleScene);
            report.gameConfigHashAfter = HashFile(GameConfigAsset);
            report.modeHashAfter = HashFile(ModeAsset);
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                report.menuHashAfter == report.menuHashBefore &&
                report.battleHashAfter == report.battleHashBefore &&
                report.gameConfigHashAfter == report.gameConfigHashBefore &&
                report.modeHashAfter == report.modeHashBefore;
            if (!report.sceneCleanAfter)
            {
                report.status = "FAIL";
                report.error += " Scene or protected Asset changed after Play.";
            }
            report.phase = "DONE";
            string path = ProjectPath(ResultRoot + report.runId + ".json");
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
                FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(report, true));
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
