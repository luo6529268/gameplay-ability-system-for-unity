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
using NTSD.Simulation.Ecs;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07C043FusionHeldBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C043FusionHeldBattlePlay.v3.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C043-FUSION-HELD-SCENE-001/";
        private const string SessionKey = "NTSD.Q07.C043FusionHeldBattlePlay";

        private static readonly string[] ProtectedPaths =
        {
            BattleScene,
            "Assets/NTSD/Scene/NTSD_Menu.unity",
            "Assets/NTSD/Config/GameConfig/GameConfig.asset",
            "Assets/NTSD/Resources/ProjectBattleModeConfig.asset"
        };

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character primary;
        private static LF2Character partner;
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
            public bool inputRight;
            public int leeOid;
            public int leeAction;
            public int leeState;
            public int leeSourceX;
            public int leeLink;
            public bool chiPresent;
            public int chiAction;
            public int chiState;
            public int chiSourceX;
            public int chiSourceZ;
            public double chiPhysicalX;
            public double chiPhysicalZ;
            public bool chiSourceInitialized;
            public int chiLink;
            public int chiChild;
            public int childSlot;
            public int childOid;
            public int childAction;
            public int childSourceX;
            public int childSourceZ;
            public double childPhysicalX;
            public double childPhysicalZ;
            public bool childSourceInitialized;
            public int childLink;
            public int childParent;
            public long invalidRelationDelta;
            public int lastInvalidRelationCount;
            public int referencePoolActive;
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
            public string[] protectedPaths;
            public string[] hashesBefore;
            public string[] hashesAfter;
            public bool configuredBeforeStart;
            public bool exitedPlay;
            public bool sceneCleanAfter;
            public int startTick;
            public int endTick;
            public int poolActiveBefore;
            public int poolActiveBeforeExit;
            public int poolActiveAfterExit = -1;
            public int shutdownStageAfterExit = -1;
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

        private static string[] HashProtected()
        {
            var hashes = new string[ProtectedPaths.Length];
            for (int index = 0; index < hashes.Length; index++)
            {
                using (SHA256 hash = SHA256.Create())
                using (FileStream stream = File.OpenRead(PathInProject(ProtectedPaths[index])))
                    hashes[index] = BitConverter.ToString(hash.ComputeHash(stream)).Replace("-", "");
            }
            return hashes;
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
            if (!string.IsNullOrEmpty(saved)) report = JsonUtility.FromJson<Report>(saved);
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
                field.SetValue(matches[0], new[] { 7, 8 });
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
                    TimeSpan.FromMinutes(10), "C043 Battle Play probe timed out.");
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
            if (scene.path != BattleScene || scene.isDirty || SceneManager.sceneCount != 1) return;
            string requestFile = PathInProject(RequestPath);
            if (!File.Exists(requestFile)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(requestFile));
            if (request == null || !request.requested) return;
            Require(request.runId == "late-double-tap-scene-03" ||
                request.runId == "late-double-tap-d024-spatial-04", "Unexpected C043 run ID.");
            Require(!File.Exists(PathInProject(ResultRoot + request.runId + ".json")),
                "Refusing to overwrite an existing C043 result.");
            request.requested = false;
            File.WriteAllText(requestFile, JsonUtility.ToJson(request, true));
            report = new Report
            {
                runId = request.runId, phase = "STARTUP", status = "RUNNING",
                startedUtc = DateTime.UtcNow.ToString("O"),
                protectedPaths = ProtectedPaths,
                hashesBefore = HashProtected()
            };
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
            Require(world.TryResolveRosterInputEntity(0, out LF2Entity first) && first is LF2Character,
                "OID7 roster entity is missing.");
            Require(world.TryResolveRosterInputEntity(1, out LF2Entity second) && second is LF2Character,
                "OID8 roster entity is missing.");
            primary = (LF2Character)first;
            partner = (LF2Character)second;
            Require(primary.ObjectId == 7 && partner.ObjectId == 8,
                "Play clone roster is not formal OID7/8 pair.");
            report.contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
            Require(report.contentRoot == "Assets/NTSD/Content/LoganRuntime",
                "Play World did not use staged formal content.");
            SetInitialCharacter(primary, 0, 304);
            SetInitialCharacter(partner, 256, 300);
            primary.AiControlled = partner.AiControlled = false;
            partner.SwitchDir("left");
            primary.RelationTeam = partner.RelationTeam = 1;
            world.Runtime.Roster.Slots[0].Team = world.Runtime.Roster.Slots[1].Team = 1;
            world.Runtime.Flow.FrameToggle = 0;
            world.Runtime.Flow.InputPhase = 0;
            Require(world.Runtime.NativeWorldClock != null, "Native world clock is unavailable.");
            world.Runtime.NativeWorldClock.Reset();
            world.Runtime.Match.Difficulty = 0;
            Require(world.BattleGameModeId == 0 && world.Difficulty == 0,
                "Controlled mode/difficulty does not match source mode0.");
            world.NativeRandom.ResetFromSeed(682973786u);
            report.poolActiveBefore = world.LogicReferencePool.ActiveCount;
            report.startTick = report.endTick = driver.CurrentTickIndex;
            report.phase = "MEASURING";
            Save();
        }

        private static void SetInitialCharacter(LF2Character character, int action, int sourceX)
        {
            character.Initialize(100, 500);
            character.ImmediateFrame(action);
            character.Runtime.HP = 100;
            character.Runtime.MP = character.Runtime.PP = 500;
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

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.ticks.Count == 60) { CompleteMeasurement(); return; }
            int tick = report.ticks.Count + 1;
            bool right = (tick >= 23 && tick <= 24) || tick >= 27;
            SimulationInputButtons buttons = right ? SimulationInputButtons.Right :
                SimulationInputButtons.None;
            long invalidBefore = world.HeldInvalidReciprocalFailureCountForDiagnostics;
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, buttons),
                new SimulationPlayerInput(1, buttons)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            LF2Entity child = null;
            int childSlot = -1;
            for (int slot = 20; slot < 100; slot++)
            {
                LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                if (candidate == null || candidate.ObjectId != 420) continue;
                Require(child == null, "More than one active OID420 child.");
                child = candidate;
                childSlot = slot;
            }
            bool chiPresent = !partner.Runtime.OidMergeDormant;
            report.ticks.Add(new TickRow
            {
                tick = tick,
                globalTick = driver.CurrentTickIndex,
                inputRight = right,
                leeOid = primary.ObjectId,
                leeAction = primary.Frame.N,
                leeState = primary.Frame.D?.State ?? -1,
                leeSourceX = primary.Runtime.SourceRuleXInt,
                leeLink = primary.Runtime.LinkState,
                chiPresent = chiPresent,
                chiAction = chiPresent ? partner.Frame.N : -1,
                chiState = chiPresent ? partner.Frame.D?.State ?? -1 : -1,
                chiSourceX = chiPresent ? partner.Runtime.SourceRuleXInt : -1,
                chiSourceZ = chiPresent ? partner.Runtime.SourceRuleZInt : -1,
                chiPhysicalX = chiPresent ? partner.Runtime.X : 0,
                chiPhysicalZ = chiPresent ? partner.Runtime.Z : 0,
                chiSourceInitialized = chiPresent && partner.Runtime.SourceRulePositionInitialized,
                chiLink = chiPresent ? partner.Runtime.LinkState : 0,
                chiChild = chiPresent ? partner.Runtime.TargetSlotIndex : -1,
                childSlot = childSlot,
                childOid = child?.ObjectId ?? -1,
                childAction = child?.Frame.N ?? -1,
                childSourceX = child?.Runtime.SourceRuleXInt ?? 0,
                childSourceZ = child?.Runtime.SourceRuleZInt ?? 0,
                childPhysicalX = child?.Runtime.X ?? 0,
                childPhysicalZ = child?.Runtime.Z ?? 0,
                childSourceInitialized = child?.Runtime.SourceRulePositionInitialized == true,
                childLink = child?.Runtime.LinkState ?? 0,
                childParent = child?.Runtime.HolderStableId ?? -1,
                invalidRelationDelta = world.HeldInvalidReciprocalFailureCountForDiagnostics -
                    invalidBefore,
                lastInvalidRelationCount = world.LastHeldInvalidReciprocalFailureCountForDiagnostics,
                referencePoolActive = world.LogicReferencePool.ActiveCount
            });
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            TickRow birth = report.ticks[5];
            TickRow fusion = report.ticks[27];
            TickRow next = report.ticks[28];
            bool matched = birth.childOid == 420 && birth.childSlot == 50 &&
                birth.childLink == -1 && birth.childParent == 1 &&
                fusion.leeOid == 51 && fusion.leeAction == 290 && !fusion.chiPresent &&
                fusion.childOid == 420 && fusion.childLink == 0 && fusion.childParent == 1 &&
                fusion.invalidRelationDelta == 1 && next.invalidRelationDelta == 0;
            report.status = matched ? "NATURAL_GATE_PASS" : "FIRST_DIFFERENCE";
            if (!matched) report.error = "One of birth, fusion, invalid relation tail, or no-repeat gates differs.";
            report.poolActiveBeforeExit = world.LogicReferencePool.ActiveCount;
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C043 natural fusion Play] " + message);
                return;
            }
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
            if (world?.LogicReferencePool != null)
                report.poolActiveAfterExit = world.LogicReferencePool.ActiveCount;
            if (driver != null)
                report.shutdownStageAfterExit = (int)driver.ShutdownStageForDiagnostics;
            if (!report.sceneCleanAfter ||
                (report.poolActiveAfterExit >= 0 && report.poolActiveAfterExit != 0) ||
                (report.shutdownStageAfterExit >= 0 &&
                    report.shutdownStageAfterExit != (int)BattleRuntimeShutdownStage.RuntimeMapCleared))
            {
                report.status = "FAIL";
                report.error += " Observed Scene, reference pool, or shutdown postcondition failed.";
            }
            report.phase = "DONE";
            Save();
            SessionState.EraseString(SessionKey);
            report = null;
            driver = null;
            world = null;
            primary = null;
            partner = null;
            stableTick = -1;
            stableUpdates = 0;
        }
    }
}
#endif
