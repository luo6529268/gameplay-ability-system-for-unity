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
    internal static class NTSD28Q07C053TobiNaturalBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string GameConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string ModeAsset = "Assets/NTSD/Resources/ProjectBattleModeConfig.asset";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string ResultPath =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001/tobi-jump-natural-01.json";
        private const string ResidualResultPath =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001/tobi-jump-natural-01-postplay.json";
        private const string SessionKey = "NTSD.Q07.C053TobiNaturalScene.01";
        private const string MenuPath = "NTSD/Validation/Q07/C053 Tobi Natural Scene Play Probe";
        private const string ResidualMenuPath =
            "NTSD/Validation/Q07/C053 Tobi Post Play Residual";
        private const int TargetTicks = 14;

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character tobi;
        private static LF2Character opponent;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class TickRow
        {
            public int tick;
            public int globalTick;
            public int submittedLegacyButtons;
            public int inputPhase;
            public int tobiAction;
            public int tobiSourceX;
            public int tobiY;
            public int tobiSourceZ;
            public int opponentAction;
            public int childCount;
            public int childSlot = -1;
            public int childAction = -1;
            public int childSourceX;
            public int childY;
            public int childSourceZ;
            public uint crtState;
            public ulong crtCalls;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId = "tobi-jump-natural-01";
            public string status = "RUNNING";
            public string phase = "STARTUP";
            public string error = string.Empty;
            public string startedUtc;
            public string contentRoot;
            public int startTick;
            public int endTick;
            public int childBirthTick = -1;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public int liveDriversAfter;
            public int livePoolsAfter;
            public string battleHashBefore;
            public string menuHashBefore;
            public string gameConfigHashBefore;
            public string modeHashBefore;
            public string battleHashAfter;
            public string menuHashAfter;
            public string gameConfigHashAfter;
            public string modeHashAfter;
            public List<TickRow> ticks = new List<TickRow>();
        }

        [Serializable]
        private sealed class ResidualReport
        {
            public string status;
            public string sceneHashInPlayResult;
            public string sceneHashNow;
            public bool sceneDirty;
            public int sceneRootCount;
            public int sceneDriverCount;
            public int sceneDriverWithWorldCount;
            public int scenePoolCount;
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

        [MenuItem(MenuPath)]
        private static void StartFromMenu()
        {
            Require(report == null &&
                string.IsNullOrEmpty(SessionState.GetString(SessionKey, string.Empty)),
                "A Tobi natural Scene probe is already active.");
            Require(!EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Original Editor must be idle and outside Play.");
            Require(string.Equals(Path.GetFullPath(Application.dataPath).Replace('\\', '/'),
                    "I:/GitHub/Unity_GAS/gameplay-ability-system-for-unity/Assets",
                    StringComparison.OrdinalIgnoreCase),
                "The Tobi probe must run in the original Unity project.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && !scene.isDirty && SceneManager.sceneCount == 1,
                "Expected one clean original Battle Scene.");
            Require(!File.Exists(ProjectPath(ResultPath)), "Refusing to overwrite result.");
            report = new Report
            {
                startedUtc = DateTime.UtcNow.ToString("O"),
                battleHashBefore = Hash(BattleScene),
                menuHashBefore = Hash(MenuScene),
                gameConfigHashBefore = Hash(GameConfigAsset),
                modeHashBefore = Hash(ModeAsset)
            };
            SaveSession();
            EditorApplication.EnterPlaymode();
        }

        [MenuItem(ResidualMenuPath)]
        private static void CheckPostPlayResidual()
        {
            Require(!EditorApplication.isPlayingOrWillChangePlaymode &&
                !EditorApplication.isCompiling && !EditorApplication.isUpdating,
                "Post-Play residual check requires an idle EditMode Editor.");
            Scene scene = SceneManager.GetActiveScene();
            Require(scene.path == BattleScene && SceneManager.sceneCount == 1,
                "Post-Play residual check requires the original Battle Scene.");
            string resultPath = ProjectPath(ResultPath);
            Require(File.Exists(resultPath), "The original Tobi Play result is missing.");
            string residualPath = ProjectPath(ResidualResultPath);
            Require(!File.Exists(residualPath), "Refusing to overwrite residual result.");
            Report prior = JsonUtility.FromJson<Report>(File.ReadAllText(resultPath));
            Require(prior != null && prior.phase == "DONE" && prior.exitedPlay,
                "The original Tobi Play result is not complete.");

            SimulationTickDriver[] drivers =
                Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                    .Where(value => value != null && !EditorUtility.IsPersistent(value) &&
                        value.gameObject.scene == scene).ToArray();
            LF2ObjectPool[] pools = Resources.FindObjectsOfTypeAll<LF2ObjectPool>()
                .Where(value => value != null && !EditorUtility.IsPersistent(value) &&
                    value.gameObject.scene == scene).ToArray();
            var residual = new ResidualReport
            {
                sceneHashInPlayResult = prior.battleHashAfter,
                sceneHashNow = Hash(BattleScene),
                sceneDirty = scene.isDirty,
                sceneRootCount = scene.rootCount,
                sceneDriverCount = drivers.Length,
                sceneDriverWithWorldCount = drivers.Count(value => value.World != null),
                scenePoolCount = pools.Length
            };
            residual.status = prior.sceneCleanAfter && !residual.sceneDirty &&
                residual.sceneHashInPlayResult == residual.sceneHashNow &&
                residual.sceneDriverCount == 1 &&
                residual.sceneDriverWithWorldCount == 0 &&
                residual.scenePoolCount == 0
                    ? "SCOPED_PASS" : "INCONCLUSIVE";
            Directory.CreateDirectory(Path.GetDirectoryName(residualPath));
            using (var stream = new FileStream(residualPath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(residual, true));
        }

        private static string ProjectPath(string path) =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", path));

        private static string Hash(string path)
        {
            using (SHA256 sha = SHA256.Create())
            using (FileStream stream = File.OpenRead(ProjectPath(path)))
                return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", string.Empty);
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
            string saved = SessionState.GetString(SessionKey, string.Empty);
            if (!string.IsNullOrEmpty(saved))
                report = JsonUtility.FromJson<Report>(saved);
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            RestoreSession();
            if (report == null || report.phase != "STARTUP" ||
                !EditorApplication.isPlaying || report.configuredBeforeStart ||
                scene.path != BattleScene) return;
            try
            {
                BattleTestBootstrap[] bootstraps =
                    Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                        .Where(value => value != null && value.isActiveAndEnabled &&
                            value.gameObject.scene == scene && !EditorUtility.IsPersistent(value))
                        .ToArray();
                Require(bootstraps.Length == 1, "Expected one active BattleTestBootstrap.");
                FieldInfo field = typeof(BattleTestBootstrap).GetField(
                    "overrideCharacterIds", BindingFlags.Instance | BindingFlags.NonPublic);
                Require(field != null, "BattleTestBootstrap roster override is unavailable.");
                field.SetValue(bootstraps[0], new[] { 0, 2 });
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
            RestoreSession();
            if (report == null) return;
            try
            {
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "Tobi natural Scene probe timed out.");
                if (!EditorApplication.isPlaying) return;
                if (report.phase == "STARTUP") { WaitForRoster(); return; }
                Require(report.phase == "MEASURING", "Unexpected probe phase.");
                MeasureOneTick();
            }
            catch (Exception error) { Fail(error.ToString()); }
        }

        private static void WaitForRoster()
        {
            Require(report.configuredBeforeStart,
                "Play clone was not configured before Start.");
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
                first is LF2Character, "Tobi roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "Opponent roster entity is missing.");
            tobi = (LF2Character)first;
            opponent = (LF2Character)second;
            Require(tobi.ObjectId == 0 && opponent.ObjectId == 2,
                "Play clone roster differs from formal OID0/2 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == ContentRoot,
                "Play World did not use formal character content.");
            SetInitialCharacter(tobi, 600, 0, 1);
            SetInitialCharacter(opponent, 1100, 0, 2);
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null,
                "Native world clock is unavailable.");
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
            int action, int team)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(action);
            character.Runtime.MP = character.Runtime.PP = 500;
            character.ClearBattleEntryInputState();
            NTSD28NativeComboStateMachine.InitializeNativeHistory(character.Runtime);
            character.SwitchDir("right");
            character.Runtime.Vx = character.Runtime.Vy = character.Runtime.Vz = 0;
            character.Runtime.HP2Orig = 1;
            character.Runtime.RespawnCount = 0;
            character.HitStun = 0;
            character.AttackExempt = 0;
            character.ItrRest.Reset();
            character.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(sourceX), 0,
                world.SpatialProjection.SourceToViewZ(400));
            AppManager.SyncParticipantBirthPosition(character, sourceX, 400);
            character.RelationTeam = team;
            world.Runtime.Roster.Slots[team - 1].Team = team;
            Require(character.Frame.N == action &&
                character.Runtime.SourceRuleXInt == sourceX &&
                character.Runtime.YInt == 0 &&
                character.Runtime.SourceRuleZInt == 400,
                "Initial action or source-rule position differs.");
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is unstable.");
            Require(driver.CurrentTickIndex == report.endTick,
                "An unobserved tick advanced while paused.");
            if (report.ticks.Count == TargetTicks)
            {
                report.status = "MEASURED_COMPARE_PENDING";
                report.phase = "EXITING";
                SaveSession();
                EditorApplication.ExitPlaymode();
                return;
            }

            int tick = report.ticks.Count + 1;
            int next = driver.CurrentTickIndex + 1;
            // Alignment contract: NTSD28-336B44-Q07-C053-TOBI-NATURAL-SCENE-001.
            // Legacy packet keys map to physical Jump/Defend/Attack as Defend/Attack/Jump.
            SimulationInputButtons buttons = tick <= 4
                ? SimulationInputButtons.Defend
                : tick >= 8 && tick <= 10
                    ? SimulationInputButtons.Attack | SimulationInputButtons.Right |
                      SimulationInputButtons.Jump
                    : SimulationInputButtons.None;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, buttons),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected tick " + next);

            int childCount = 0;
            int childSlot = -1;
            LF2Entity child = null;
            for (int slot = 2; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                if (candidate == null || candidate.ObjectId != 251) continue;
                childCount++;
                if (child != null) continue;
                child = candidate;
                childSlot = slot;
            }
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var row = new TickRow
            {
                tick = tick,
                globalTick = driver.CurrentTickIndex,
                submittedLegacyButtons = (int)buttons,
                inputPhase = world.InputPhase,
                tobiAction = tobi.Frame.N,
                tobiSourceX = tobi.Runtime.SourceRuleXInt,
                tobiY = tobi.Runtime.YInt,
                tobiSourceZ = tobi.Runtime.SourceRuleZInt,
                opponentAction = opponent.Frame.N,
                childCount = childCount,
                childSlot = childSlot,
                childAction = child?.Frame.N ?? -1,
                childSourceX = child?.Runtime.SourceRuleXInt ?? 0,
                childY = child?.Runtime.YInt ?? 0,
                childSourceZ = child?.Runtime.SourceRuleZInt ?? 0,
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls
            };
            if (child != null && report.childBirthTick < 0)
                report.childBirthTick = tick;
            report.ticks.Add(row);
            report.endTick = driver.CurrentTickIndex;
            SaveSession();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07/C053 Tobi natural Scene] " + message);
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
            report.battleHashAfter = Hash(BattleScene);
            report.menuHashAfter = Hash(MenuScene);
            report.gameConfigHashAfter = Hash(GameConfigAsset);
            report.modeHashAfter = Hash(ModeAsset);
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                SceneManager.sceneCount == 1 &&
                report.battleHashAfter == report.battleHashBefore &&
                report.menuHashAfter == report.menuHashBefore &&
                report.gameConfigHashAfter == report.gameConfigHashBefore &&
                report.modeHashAfter == report.modeHashBefore;
            report.liveDriversAfter = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                .Count(value => value != null && !EditorUtility.IsPersistent(value) &&
                    value.gameObject.scene.IsValid());
            report.livePoolsAfter = Resources.FindObjectsOfTypeAll<LF2ObjectPool>()
                .Count(value => value != null && !EditorUtility.IsPersistent(value) &&
                    value.gameObject.scene.IsValid());
            if (!report.sceneCleanAfter && report.status == "MEASURED_COMPARE_PENDING")
                report.status = "MEASURED_SCENE_CHANGED";
            report.phase = "DONE";
            string resultPath = ProjectPath(ResultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
            using (var stream = new FileStream(resultPath, FileMode.CreateNew,
                FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
                writer.Write(JsonUtility.ToJson(report, true));
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            tobi = opponent = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
