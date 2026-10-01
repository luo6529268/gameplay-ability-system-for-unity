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
    internal static class NTSD28Q07C052HayatePhysicalBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string GameConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ModeAsset = "Assets/NTSD/Resources/ProjectBattleModeConfig.asset";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q07_C052HayatePhysicalBattlePlay-v3.request.json";
        private const string ResultPath = "artifacts/diagnostics/NTSD28-336B44-Q07-C052-HAYATE-PHYSICAL-SCENE-001/hayate-physical-scene-v3.json";
        private const string SessionKey = "NTSD.Q07.C052HayatePhysicalBattlePlay.v3";
        private const string RunId = "hayate-physical-scene-v3";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character first;
        private static LF2Character second;
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
            public int tick;
            public int globalTick;
            public int submittedInputMask;
            public int actorAction;
            public int actorX;
            public int actorY;
            public int actorZ;
            public int actorMp;
            public int actorPp;
            public int actorMpConsumed;
            public int firstAction;
            public int firstHp;
            public int firstX;
            public int secondAction;
            public int secondHp;
            public int secondX;
            public int count417;
            public int first417Slot = -1;
            public int first417Action = -1;
            public int first417X;
            public int first417Y;
            public int first417Z;
            public int count211;
            public int first211Slot = -1;
            public int first211Action = -1;
            public int first211X;
            public int first211Y;
            public int first211Z;
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
                    TimeSpan.FromMinutes(10), "C052 Hayate Scene probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected Hayate probe phase.");
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
            Require(request.runId == RunId, "Unexpected Hayate probe request.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Hayate probe requires one clean original Battle Scene.");
            Require(!File.Exists(ProjectPath(ResultPath)), "Refusing to overwrite Hayate result.");
            request.requested = false;
            File.WriteAllText(requestPath, JsonUtility.ToJson(request, true));
            report = new Report
            {
                runId = RunId,
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
                Require(matches.Length == 1, "Expected one BattleTestBootstrap in Play clone.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField("overrideCharacterIds",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap overrideCharacterIds is missing.");
                field.SetValue(matches[0], new[] { 73, 2, 2 });
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
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity actorEntity) &&
                actorEntity is LF2Character, "Hayate roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity firstEntity) &&
                firstEntity is LF2Character, "First Naruto roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(2, out LF2Entity secondEntity) &&
                secondEntity is LF2Character, "Second Naruto roster entity is missing.");
            actor = (LF2Character)actorEntity;
            first = (LF2Character)firstEntity;
            second = (LF2Character)secondEntity;
            Require(actor.ObjectId == 73 && first.ObjectId == 2 && second.ObjectId == 2,
                "Play clone roster differs from formal OID73/2/2.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == ContentRoot, "Play World did not use formal content.");
            SetInitialCharacter(actor, 500, 0, "right");
            SetInitialCharacter(first, 589, 0, "left");
            SetInitialCharacter(second, 619, 0, "left");
            actor.RelationTeam = 1;
            first.RelationTeam = second.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            world.Runtime.Roster.Slots[2].Team = 2;
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

        private static void SetInitialCharacter(LF2Character character, int sourceX,
            int action, string direction)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(action);
            character.Runtime.MP = character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir(direction);
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

        private static void CaptureChild(TickRow row, int oid)
        {
            for (int slot = 50; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity == null || entity.ObjectId != oid) continue;
                if (oid == 417)
                {
                    row.count417++;
                    if (row.first417Slot >= 0) continue;
                    row.first417Slot = slot;
                    row.first417Action = entity.Frame.N;
                    row.first417X = entity.Runtime.SourceRuleXInt;
                    row.first417Y = entity.Runtime.YInt;
                    row.first417Z = entity.Runtime.SourceRuleZInt;
                }
                else
                {
                    row.count211++;
                    if (row.first211Slot >= 0) continue;
                    row.first211Slot = slot;
                    row.first211Action = entity.Frame.N;
                    row.first211X = entity.Runtime.SourceRuleXInt;
                    row.first211Y = entity.Runtime.YInt;
                    row.first211Z = entity.Runtime.SourceRuleZInt;
                }
            }
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is unstable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.ticks.Count == 36) { CompleteMeasurement(); return; }
            int next = driver.CurrentTickIndex + 1;
            int relativeTick = report.ticks.Count + 1;
            // The packet uses legacy buffer keys: physical Jump/Defend/Attack
            // enter as Defend/Attack/Jump before native input projection.
            SimulationInputButtons actorButtons = relativeTick <= 4
                ? SimulationInputButtons.Defend
                : relativeTick >= 8 && relativeTick <= 10
                    ? SimulationInputButtons.Attack | SimulationInputButtons.Right |
                      SimulationInputButtons.Jump
                    : SimulationInputButtons.None;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, actorButtons),
                new SimulationPlayerInput(1, SimulationInputButtons.None),
                new SimulationPlayerInput(2, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            var row = new TickRow
            {
                tick = relativeTick,
                globalTick = driver.CurrentTickIndex,
                submittedInputMask = (int)actorButtons,
                actorAction = actor.Frame.N,
                actorX = actor.Runtime.SourceRuleXInt,
                actorY = actor.Runtime.YInt,
                actorZ = actor.Runtime.SourceRuleZInt,
                actorMp = actor.Runtime.MP,
                actorPp = actor.Runtime.PP,
                actorMpConsumed = actor.Runtime.InputMpConsumedTotal350,
                firstAction = first.Frame.N,
                firstHp = first.Health.HP,
                firstX = first.Runtime.SourceRuleXInt,
                secondAction = second.Frame.N,
                secondHp = second.Health.HP,
                secondX = second.Runtime.SourceRuleXInt
            };
            CaptureChild(row, 417);
            CaptureChild(row, 211);
            report.ticks.Add(row);
            report.endTick = driver.CurrentTickIndex;
            SaveSession();
        }

        private static void CompleteMeasurement()
        {
            report.status = "MEASURED_COMPARE_PENDING";
            report.phase = "EXITING";
            SaveSession();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C052 Hayate Scene] " + message);
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
            string path = ProjectPath(ResultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write,
                FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(report, true));
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            actor = first = second = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
