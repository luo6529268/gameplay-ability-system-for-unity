#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
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
    internal static class NTSD28Q07C053DoubleUjBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C053DoubleUjBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C053-DOUBLE-UJ-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.C053DoubleUjBattlePlay";
        private const string RunId = "o702-t700-a620-b620-scene-01";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character actor;
        private static LF2Character target;
        private static LF2Entity firstAttacker;
        private static LF2Entity secondAttacker;
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
            public int childSlot = -1;
            public int childAction = -1;
            public int childLatch = -1;
            public int childX;
            public int childY;
            public int childZ;
            public int firstAttackerAction;
            public int secondAttackerAction;
            public int firstVictimRest;
            public int secondVictimRest;
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
            public int firstAttackerSlot = -1;
            public int secondAttackerSlot = -1;
            public int childBirthTick = -1;
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
                field.SetValue(matches[0], new[] { 702, 2 });
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
                    TimeSpan.FromMinutes(10), "C053 Battle Play probe timed out.");
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
            Require(request.runId == RunId, "Unexpected C053 run ID.");
            Require(!File.Exists(PathInProject(ResultRoot + RunId + ".json")),
                "Refusing to overwrite an existing C053 result.");
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
                first is LF2Character, "OID702 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) &&
                second is LF2Character, "OID2 roster entity is missing.");
            actor = (LF2Character)first;
            target = (LF2Character)second;
            Require(actor.ObjectId == 702 && target.ObjectId == 2,
                "Play clone roster is not formal OID702/2 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            Require(world.FindEntityByRuntimeSlotForQuery(2) == null &&
                world.FindEntityByRuntimeSlotForQuery(3) == null,
                "Controlled source slots 2/3 are already occupied.");
            SetInitialCharacter(actor, 553, 500);
            SetInitialCharacter(target, 0, 700);
            actor.RelationTeam = 1;
            target.RelationTeam = 2;
            world.Runtime.Roster.Slots[0].Team = 1;
            world.Runtime.Roster.Slots[1].Team = 2;
            firstAttacker = CreateFormalAttacker(2, 620);
            secondAttacker = CreateFormalAttacker(3, 620);
            report.firstAttackerSlot = firstAttacker.Runtime.SlotIndex;
            report.secondAttackerSlot = secondAttacker.Runtime.SlotIndex;
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

        private static LF2Entity CreateFormalAttacker(int slot, int sourceX)
        {
            var task = new OPointCreateTask
            {
                targetWorld = world,
                requiredRuntimeSlot = slot,
                dir = "right",
                team = 2,
                preserveActionZero = true,
                skipPostInitZOffset = true,
                useDirectRuntimePosition = true,
                directX = world.SpatialProjection.SourceToViewX(sourceX),
                directY = 0,
                directZ = world.SpatialProjection.SourceToViewZ(401),
                useSourceRulePosition = true,
                sourceRuleX = sourceX,
                sourceRuleZ = 401,
                useDirectVelocity = true,
                opoint = new ObjectPoint { oid = 875, kind = 1, action = 55, facing = 0 }
            };
            LF2Entity entity = world.LogicEntityFactory.Create(task, out var failure);
            Require(entity != null, "Formal OID875 creation rejected: " + failure);
            Require(entity.ObjectId == 875 && entity.Runtime.SlotIndex == slot &&
                entity.Frame.N == 55 && entity.Runtime.SourceRuleXInt == sourceX &&
                entity.Runtime.SourceRuleZInt == 401,
                "Formal OID875 slot/action/source position differs.");
            entity.RelationTeam = 2;
            entity.Runtime.HP = 500;
            entity.Runtime.MP = 500;
            entity.Runtime.PP = 500;
            entity.Runtime.Vx = entity.Runtime.Vy = entity.Runtime.Vz = 0;
            return entity;
        }

        private static LF2Entity FindChild(out int slot)
        {
            for (int index = 50; index < world.RuntimeSlotCapacityForDiagnostics; index++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(index);
                if (entity != null && entity.ObjectId == 808)
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
            LF2Entity child = FindChild(out int childSlot);
            NTSD28NativeRandomScalarState rng = world.NativeRandom.CaptureScalarState();
            var row = new TickRow
            {
                relativeTick = report.ticks.Count + 1,
                globalTick = driver.CurrentTickIndex,
                childSlot = childSlot,
                childAction = child?.Frame.N ?? -1,
                childLatch = child?.Trans.WaitCounter ?? -1,
                childX = child?.Runtime.SourceRuleXInt ?? 0,
                childY = child?.Runtime.YInt ?? 0,
                childZ = child?.Runtime.SourceRuleZInt ?? 0,
                firstAttackerAction = firstAttacker.Frame.N,
                secondAttackerAction = secondAttacker.Frame.N,
                firstVictimRest = child == null ? 0 : world.GetRawRestVrest(childSlot,
                    firstAttacker.Runtime.SlotIndex),
                secondVictimRest = child == null ? 0 : world.GetRawRestVrest(childSlot,
                    secondAttacker.Runtime.SlotIndex),
                crtState = rng.CrtState,
                crtCalls = rng.CrtCalls,
                customCounter = rng.SynchronizedCounter,
                customIndex = rng.SynchronizedIndex,
                customCalls = rng.SynchronizedCalls
            };
            if (child != null && report.childBirthTick < 0)
                report.childBirthTick = row.relativeTick;
            report.ticks.Add(row);
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            TickRow sixth = report.ticks.Single(value => value.relativeTick == 6);
            TickRow seventh = report.ticks.Single(value => value.relativeTick == 7);
            bool matched = report.childBirthTick == 1 &&
                sixth.childAction == 153 &&
                seventh.childAction == 156 &&
                seventh.firstVictimRest > 0 && seventh.secondVictimRest > 0;
            report.status = matched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
            if (!matched)
                report.error = "OID808 birth/tick6 action/tick7 double-hit action-rest differs from source.";
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C053 double Uj Play] " + message);
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
            actor = null;
            target = null;
            firstAttacker = null;
            secondAttacker = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
