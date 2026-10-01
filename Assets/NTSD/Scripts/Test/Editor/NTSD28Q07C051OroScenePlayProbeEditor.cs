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
    internal static class NTSD28Q07C051OroScenePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string GameConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ModeAsset = "Assets/NTSD/Resources/ProjectBattleModeConfig.asset";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string ArmorRequest = "Temp/NTSD28_Q07_C051ArmorScenePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C051-ORO-SCENE-PLAY-001/";
        private const string ArmorResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C051-ARMOR-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.C051OroScenePlay";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character target;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int globalTick;
            public int actorAction;
            public int targetAction;
            public int targetHp;
            public int targetMp;
            public int targetArmorHp;
            public int targetSourceX;
            public double targetVx;
            public int childCount;
            public int childSlot = -1;
            public int childAction = -1;
            public int childSourceX;
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
            public bool faceRight;
            public bool armorCase;
            public int targetOid;
            public int targetSourceX;
            public int startTick;
            public int endTick;
            public int childBirthTick = -1;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
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

        [MenuItem("Tools/NTSD/2.8 Alignment/Q07 C051 Oro Scene Right")]
        private static void StartRight() => Start(true, 550, "right-x550-v1");

        [MenuItem("Tools/NTSD/2.8 Alignment/Q07 C051 Oro Scene Left")]
        private static void StartLeft() => Start(false, 350, "left-x350-v1");

        [Serializable]
        private sealed class ArmorRequestPayload
        {
            public bool requested;
            public int targetOid;
            public string runId;
        }

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string ResultPath(string runId, bool armorCase) =>
            (armorCase ? ArmorResultRoot : ResultRoot) + runId + ".json";

        private static string HashFile(string relative)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(ProjectPath(relative)))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "");
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

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }

        private static void Start(bool faceRight, int targetX, string runId,
            int targetOid = 2, bool armorCase = false)
        {
            RestoreSession();
            Require(report == null && !EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Editor is busy or another C051 probe is active.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Open one clean original Battle Scene before C051 Play.");
            Require(!File.Exists(ProjectPath(ResultPath(runId, armorCase))),
                "Refusing to overwrite an existing C051 result.");
            report = new Report
            {
                runId = runId,
                status = "RUNNING",
                phase = "STARTUP",
                startedUtc = DateTime.UtcNow.ToString("O"),
                faceRight = faceRight,
                armorCase = armorCase,
                targetOid = targetOid,
                targetSourceX = targetX,
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
                field.SetValue(matches[0], new[] { report.armorCase ? 78 : 20,
                    report.targetOid });
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

        private static void Poll()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            try
            {
                RestoreSession();
                if (report == null)
                {
                    PollArmorRequest();
                    return;
                }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C051 Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected C051 probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void PollArmorRequest()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            string path = ProjectPath(ArmorRequest);
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path, Encoding.UTF8);
            ArmorRequestPayload request = JsonUtility.FromJson<ArmorRequestPayload>(json);
            Require(request != null && request.requested &&
                (request.targetOid == 97 || request.targetOid == 2) &&
                request.runId == (request.targetOid == 97 ?
                    "armor97-x550-scene-v1" : "control2-x550-scene-v1"),
                "Unexpected C051 armor Scene request payload; leaving file untouched.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "C051 armor Scene request requires one clean Battle Scene.");
            Require(!File.Exists(ProjectPath(ResultPath(request.runId, true))),
                "Refusing to overwrite an existing C051 armor Scene result.");
            File.Delete(path);
            Start(true, 550, request.runId, request.targetOid, true);
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
                first is LF2Character, "OID20 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "OID2 roster entity is missing.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            Require(actor.ObjectId == (report.armorCase ? 78 : 20) &&
                target.ObjectId == report.targetOid,
                "Play clone roster differs from the requested formal pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == ContentRoot, "Play World did not use formal content.");
            SetInitialCharacter(actor, report.armorCase ? 466 : 288, 500,
                report.faceRight);
            SetInitialCharacter(target, 0, report.targetSourceX,
                report.armorCase ? false : report.faceRight);
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

        private static void SetInitialCharacter(LF2Character character, int action,
            int sourceX, bool faceRight)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(action);
            character.Runtime.MP = 500;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir(faceRight ? "right" : "left");
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

        private static LF2Entity FindChild(out int slot, out int count)
        {
            LF2Entity first = null;
            slot = -1;
            count = 0;
            int expectedOid = report.armorCase ? 447 : 888;
            for (int index = 50; index < world.RuntimeSlotCapacityForDiagnostics; index++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(index);
                if (entity != null && entity.ObjectId == expectedOid)
                {
                    count++;
                    if (first != null) continue;
                    slot = index;
                    first = entity;
                }
            }
            return first;
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is unstable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.ticks.Count == 12) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            LF2Entity child = FindChild(out int childSlot, out int childCount);
            var row = new TickRow
            {
                relativeTick = report.ticks.Count + 1,
                globalTick = driver.CurrentTickIndex,
                actorAction = actor.Frame.N,
                targetAction = target.Frame.N,
                targetHp = target.Health.HP,
                targetMp = target.Runtime.MP,
                targetArmorHp = target.Runtime.RuntimeArmorHp118,
                targetSourceX = target.Runtime.SourceRuleXInt,
                targetVx = target.Runtime.Vx,
                childCount = childCount,
                childSlot = childSlot,
                childAction = child?.Frame.N ?? -1,
                childSourceX = child?.Runtime.SourceRuleXInt ?? 0
            };
            if (child != null && report.childBirthTick < 0)
                report.childBirthTick = row.relativeTick;
            report.ticks.Add(row);
            report.endTick = driver.CurrentTickIndex;
            SaveSession();
        }

        private static void CompleteMeasurement()
        {
            if (report.armorCase)
            {
                TickRow third = report.ticks.Single(value => value.relativeTick == 3);
                bool armorTarget = report.targetOid == 97;
                bool armorMatched = report.childBirthTick == 2 &&
                    third.childCount > 0 &&
                    third.targetAction == (armorTarget ? 0 : 180) &&
                    third.targetHp == (armorTarget ? 495 : 450);
                report.status = armorMatched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
                if (!armorMatched)
                    report.error = "OID447 birth or tick3 armor/control action-HP differs.";
                report.phase = "EXITING";
                SaveSession();
                EditorApplication.ExitPlaymode();
                return;
            }
            TickRow eighth = report.ticks.Single(value => value.relativeTick == 8);
            TickRow ninth = report.ticks.Single(value => value.relativeTick == 9);
            TickRow twelfth = report.ticks.Single(value => value.relativeTick == 12);
            double expectedVx = report.faceRight ? -10 : 10;
            bool matched = report.childBirthTick == 4 && eighth.childAction == 40 &&
                ninth.targetAction == 180 && ninth.targetHp == 435 &&
                twelfth.targetAction == 180 && twelfth.targetHp == 435 &&
                twelfth.targetVx == expectedVx;
            report.status = matched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
            if (!matched)
                report.error = "OID888 birth/tick8 action/tick9 target action-HP/tick12 Vx differs.";
            report.phase = "EXITING";
            SaveSession();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C051 Oro Scene Play] " + message);
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
            string path = ProjectPath(ResultPath(report.runId, report.armorCase));
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
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
