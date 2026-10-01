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
    internal static class NTSD28Q07C052PositiveRestBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string GameConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ModeAsset = "Assets/NTSD/Resources/ProjectBattleModeConfig.asset";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q07_C052PositiveRestBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C052-UNITY-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.C052PositiveRestBattlePlay";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character first;
        private static LF2Character second;
        private static LF2Entity attacker;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public int secondX;
            public int initialFirstAction;
            public string runId;
        }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int globalTick;
            public int attackerAction;
            public int firstAction;
            public int firstHp;
            public int firstRest;
            public int secondAction;
            public int secondHp;
            public int secondRest;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public int secondX;
            public int initialFirstAction;
            public string status;
            public string phase;
            public string error;
            public string startedUtc;
            public string contentRoot;
            public int attackerSlot = -1;
            public int firstSlot = -1;
            public int secondSlot = -1;
            public int startTick;
            public int endTick;
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

        private static string ProjectPath(string relative) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", relative));

        private static string HashFile(string relative)
        {
            using (SHA256 hash = SHA256.Create())
            using (FileStream stream = File.OpenRead(ProjectPath(relative)))
                return BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
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
                if (report == null) { TryStart(); return; }
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C052 Battle Play probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected C052 probe phase.");
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
            string path = ProjectPath(RequestPath);
            if (!File.Exists(path)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(path, Encoding.UTF8));
            if (request == null || !request.requested) return;
            Require(((request.initialFirstAction == 0 &&
                        (request.secondX == 530 || request.secondX == 650) &&
                        (request.runId == "x" + request.secondX + "-scene-v1" ||
                         request.runId == "x" + request.secondX + "-scene-v2")) ||
                     (request.initialFirstAction == 203 && request.secondX == 530 &&
                      (request.runId == "zero-rest-scene-v1" ||
                       request.runId == "zero-rest-scene-v2"))),
                "Unexpected C052 request; leaving it untouched.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "C052 requires one clean original Battle Scene.");
            Require(!File.Exists(ProjectPath(ResultRoot + request.runId + ".json")),
                "Refusing to overwrite an existing C052 result.");
            request.requested = false;
            File.WriteAllText(path, JsonUtility.ToJson(request, true));
            report = new Report
            {
                runId = request.runId,
                secondX = request.secondX,
                initialFirstAction = request.initialFirstAction,
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
            if (report == null || report.phase != "STARTUP" ||
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
                field.SetValue(matches[0], new[] { 2, 2 });
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
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity firstEntity) &&
                firstEntity is LF2Character, "First OID2 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity secondEntity) &&
                secondEntity is LF2Character, "Second OID2 roster entity is missing.");
            first = (LF2Character)firstEntity;
            second = (LF2Character)secondEntity;
            Require(first.ObjectId == 2 && second.ObjectId == 2,
                "Play clone roster differs from formal OID2 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == ContentRoot, "Play World did not use formal content.");
            SetInitialCharacter(first, 500, report.initialFirstAction);
            SetInitialCharacter(second, report.secondX, 0);
            first.RelationTeam = second.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 2;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null, "Native world clock is missing.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            Require(world.BattleGameModeId == 0 && world.Difficulty == 0,
                "Battle mode or difficulty differs from formal mode0.");
            world.NativeRandom.ResetFromSeed(682973786u);
            attacker = CreateFormalAttacker(2, 500);
            report.attackerSlot = attacker.Runtime.SlotIndex;
            report.firstSlot = first.Runtime.SlotIndex;
            report.secondSlot = second.Runtime.SlotIndex;
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            SaveSession();
        }

        private static void SetInitialCharacter(LF2Character character, int sourceX, int action)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(action);
            character.Runtime.MP = 500;
            character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("left");
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

        private static LF2Entity CreateFormalAttacker(int slot, int sourceX)
        {
            var task = new OPointCreateTask
            {
                targetWorld = world,
                requiredRuntimeSlot = slot,
                dir = "right",
                team = 1,
                preserveActionZero = true,
                skipPostInitZOffset = true,
                useDirectRuntimePosition = true,
                directX = world.SpatialProjection.SourceToViewX(sourceX),
                directY = 0,
                directZ = world.SpatialProjection.SourceToViewZ(400),
                useSourceRulePosition = true,
                sourceRuleX = sourceX,
                sourceRuleZ = 400,
                useDirectVelocity = true,
                opoint = new ObjectPoint { oid = 211, kind = 1, action = 161, facing = 0 }
            };
            LF2Entity entity = world.LogicEntityFactory.Create(task, out var failure);
            Require(entity != null, "Formal OID211 creation rejected: " + failure);
            Require(entity.ObjectId == 211 && entity.Runtime.SlotIndex == slot &&
                entity.Frame.N == 161 && entity.Runtime.SourceRuleXInt == sourceX,
                "Formal OID211 slot/action/source position differs.");
            entity.RelationTeam = 1;
            entity.Runtime.HP = entity.Runtime.MP = entity.Runtime.PP = 500;
            entity.Runtime.Vx = entity.Runtime.Vy = entity.Runtime.Vz = 0;
            return entity;
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
                attackerAction = attacker.Frame.N,
                firstAction = first.Frame.N,
                firstHp = first.Health.HP,
                firstRest = world.GetRawRestVrest(first.Runtime.SlotIndex,
                    attacker.Runtime.SlotIndex),
                secondAction = second.Frame.N,
                secondHp = second.Health.HP,
                secondRest = world.GetRawRestVrest(second.Runtime.SlotIndex,
                    attacker.Runtime.SlotIndex)
            });
            report.endTick = driver.CurrentTickIndex;
            SaveSession();
        }

        private static void CompleteMeasurement()
        {
            TickRow firstTick = report.ticks.Single(value => value.relativeTick == 1);
            bool matched = report.initialFirstAction == 203
                ? firstTick.firstHp == 500 && firstTick.firstRest == 0 &&
                  firstTick.secondRest == 0
                : firstTick.firstHp == 420 && firstTick.firstAction == 203 &&
                  firstTick.firstRest > 0 &&
                  (report.secondX == 530
                      ? firstTick.secondHp == 420 && firstTick.secondRest > 0
                      : firstTick.secondHp == 500 && firstTick.secondRest == 0);
            report.status = matched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
            if (!matched)
                report.error = "OID211 effect21 target action/HP/rest differs; inspect tick rows and initial geometry.";
            report.phase = "EXITING";
            SaveSession();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C052 Scene Play] " + message);
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
            first = null;
            second = null;
            attacker = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
