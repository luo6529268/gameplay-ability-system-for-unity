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
using NTSD.Simulation.Ecs;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    internal static class NTSD28Q07C056FusionHoldBattlePlayProbeEditor
    {
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string RequestPath = "Temp/NTSD28_Q07_C056FusionHoldBattlePlay.request.json";
        private const string ResultRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-C056-FUSION-SCENE-PLAY-001/";
        private const string SessionKey = "NTSD.Q07.C056FusionHoldBattlePlay";
        private const string ShutdownRunId = "fusion-hold-c0-shutdown-20261004-01";
        private const string ShutdownRequestSessionKey = SessionKey + ".ShutdownRunId";

        private static Report report;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static LF2Character primary;
        private static LF2Character partner;
        private static int stableTick = -1;
        private static int stableUpdates;
        private static int shutdownSaveSequence;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public string runId;
            public int initialCounter;
        }

        [Serializable]
        private sealed class TickRow
        {
            public int relativeTick;
            public int globalTick;
            public int primaryOid;
            public int action;
            public int counter;
            public int hold;
            public int actionLatch;
            public int tickActionSnapshot;
            public int sourceX;
            public int sourceZ;
            public int partnerAction;
            public int partnerHp;
            public int partnerTeam;
            public int partnerSourceX;
            public int partnerSourceZ;
            public bool partnerAiControlled;
            public int partnerFusionTimer;
            public int primaryFusionTimer;
            public bool partnerDormant;
            public long spawnDelta;
            public int lastSpawnOid;
            public int activePuppetCount;
            public int referencePoolActive;
        }

        [Serializable]
        private sealed class Report
        {
            public string runId;
            public int initialCounter;
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
            public int poolActiveBefore;
            public int poolActiveBeforeExit;
            public int poolActiveAfterExit = -1;
            public int shutdownStageAfterExit = -1;
            public int shutdownTriggerTick = -1;
            public int shutdownActivePuppetCount = -1;
            public string shutdownStatus;
            public string shutdownStage;
            public string shutdownFailure;
            public int remainingWorldObjects = -1;
            public int remainingRuntimeSlots = -1;
            public int remainingPoolBorrowers = -1;
            public int remainingActivePoolObjects = -1;
            public int remainingActivePoolSprites = -1;
            public bool poolQuiesced;
            public bool worldDetached;
            public bool orderedShutdownComplete;
            public bool noLiveWorldAfterExit;
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
            if (IsShutdownRun())
            {
                string directory = PathInProject(ResultRoot + report.runId);
                Directory.CreateDirectory(directory);
                string snapshotPath;
                do
                {
                    snapshotPath = Path.Combine(directory,
                        "snapshot-" + shutdownSaveSequence.ToString("D4") + ".json");
                    shutdownSaveSequence++;
                }
                while (File.Exists(snapshotPath));

                using (FileStream stream = new FileStream(
                    snapshotPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (StreamWriter writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(report, true));
                return;
            }

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

        private static bool IsShutdownRun()
        {
            return report != null && report.initialCounter == 0 &&
                string.Equals(report.runId, ShutdownRunId, StringComparison.Ordinal);
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
                if (report.phase == "CLEANUP_BLOCKED") return;
                if (report.phase == "EXITING")
                {
                    if (!EditorApplication.isPlayingOrWillChangePlaymode) Finish();
                    return;
                }
                Require(DateTime.UtcNow - DateTime.Parse(report.startedUtc).ToUniversalTime() <
                    TimeSpan.FromMinutes(10), "C056 Battle Play probe timed out.");
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
            bool shutdownRun = request.initialCounter == 0 &&
                string.Equals(request.runId, ShutdownRunId, StringComparison.Ordinal);
            if (shutdownRun)
            {
                if (Directory.Exists(PathInProject(ResultRoot + request.runId))) return;
                if (string.Equals(SessionState.GetString(ShutdownRequestSessionKey, ""),
                    request.runId, StringComparison.Ordinal)) return;
                SessionState.SetString(ShutdownRequestSessionKey, request.runId);
            }
            else
            {
                Require(request.initialCounter == 7 || request.initialCounter == 0,
                    "Only the source counter7/counter0 controls are allowed.");
                Require(request.runId == "fusion-hold-c" + request.initialCounter + "-scene-02",
                    "Unexpected C056 run ID.");
                Require(!File.Exists(PathInProject(ResultRoot + request.runId + ".json")),
                    "Refusing to overwrite an existing C056 result.");
                request.requested = false;
                File.WriteAllText(requestFile, JsonUtility.ToJson(request, true));
            }

            shutdownSaveSequence = 0;
            report = new Report
            {
                runId = request.runId, initialCounter = request.initialCounter,
                phase = "STARTUP", status = "RUNNING", startedUtc = DateTime.UtcNow.ToString("O"),
                sceneHashBefore = HashScene()
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
            SetInitialCharacter(primary, 304);
            SetInitialCharacter(partner, 300);
            primary.AiControlled = partner.AiControlled = false;
            partner.SwitchDir("left");
            primary.RelationTeam = partner.RelationTeam = 1;
            world.Runtime.Roster.Slots[0].Team = world.Runtime.Roster.Slots[1].Team = 1;
            primary.AttackingCounter = report.initialCounter;
            primary.FrameDelay = 3;
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

        private static void SetInitialCharacter(LF2Character character, int sourceX)
        {
            character.Initialize(500, 500);
            character.ImmediateFrame(9);
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
                world.SpatialProjection.SourceToViewZ(600));
            AppManager.SyncParticipantBirthPosition(character, sourceX, 600);
            Require(character.Frame.N == 9 && character.Runtime.SourceRuleXInt == sourceX,
                "Character initial action or source X differs.");
        }

        private static int CountPuppets()
        {
            int count = 0;
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity != null && entity.ObjectId == 213) count++;
            }
            return count;
        }

        private static void MeasureOneTick()
        {
            Require(ReferenceEquals(driver.World, world) && driver.IsPaused &&
                !driver.DedicatedSimulationWorkerTickInFlightForDiagnostics,
                "Production World changed or tick boundary is not stable.");
            Require(driver.CurrentTickIndex == report.endTick, "Unobserved tick while paused.");
            if (report.ticks.Count == 3) { CompleteMeasurement(); return; }
            long spawnBefore = world.StructuralWriterDiagnosticsForDiagnostics.SpawnCount;
            int next = driver.CurrentTickIndex + 1;
            var input = new FrameInputSet(next, new[]
            {
                new SimulationPlayerInput(0, SimulationInputButtons.None),
                new SimulationPlayerInput(1, SimulationInputButtons.None)
            });
            Require(driver.StepOneTick(input, ignorePaused: true, buildPresentation: true),
                "Production Driver rejected complete tick " + next);
            BattleStructuralWriterDiagnostics structural = world.StructuralWriterDiagnosticsForDiagnostics;
            report.ticks.Add(new TickRow
            {
                relativeTick = report.ticks.Count + 1,
                globalTick = driver.CurrentTickIndex,
                primaryOid = primary.ObjectId,
                action = primary.Frame.N,
                counter = primary.AttackingCounter,
                hold = primary.FrameDelay,
                actionLatch = primary.Trans.WaitCounter,
                tickActionSnapshot = primary.Runtime.PrevFrame2,
                sourceX = primary.Runtime.SourceRuleXInt,
                sourceZ = primary.Runtime.SourceRuleZInt,
                partnerAction = partner.Frame.N,
                partnerHp = partner.Health.HP,
                partnerTeam = partner.RelationTeam,
                partnerSourceX = partner.Runtime.SourceRuleXInt,
                partnerSourceZ = partner.Runtime.SourceRuleZInt,
                partnerAiControlled = partner.AiControlled,
                partnerFusionTimer = partner.Runtime.Unk338,
                primaryFusionTimer = primary.Runtime.Unk338,
                partnerDormant = partner.Runtime.OidMergeDormant,
                spawnDelta = structural.SpawnCount - spawnBefore,
                lastSpawnOid = structural.LastOid,
                activePuppetCount = CountPuppets(),
                referencePoolActive = world.LogicReferencePool.ActiveCount
            });
            report.endTick = driver.CurrentTickIndex;
            Save();
        }

        private static void CompleteMeasurement()
        {
            long[] expected = report.initialCounter == 7 ? new long[] { 0, 0, 0 } :
                new long[] { 1, 1, 0 };
            bool matched = report.ticks.Count == 3 && report.ticks[0].primaryOid == 51 &&
                report.ticks[0].action == 290 && report.ticks[0].partnerDormant &&
                report.ticks[0].counter == report.initialCounter && report.ticks[0].hold == 2 &&
                report.ticks.Select((row, index) => row.spawnDelta == expected[index]).All(value => value);
            report.status = matched ? "SCOPED_PASS" : "FIRST_DIFFERENCE";
            if (!matched) report.error = "Fusion action/counter/hold or per-tick structural spawn differs from source.";

            if (IsShutdownRun())
            {
                Require(report.ticks[2].activePuppetCount == 2 && CountPuppets() == 2,
                    "Shutdown witness requires two live OID213 entities after the third full tick.");
                report.shutdownTriggerTick = report.endTick;
                report.shutdownActivePuppetCount = report.ticks[2].activePuppetCount;
                report.poolActiveBeforeExit = world.LogicReferencePool.ActiveCount;
                if (!CaptureOrderedShutdown()) return;
                report.phase = "EXITING";
                Save();
                EditorApplication.ExitPlaymode();
                return;
            }

            report.poolActiveBeforeExit = world.LogicReferencePool.ActiveCount;
            report.phase = "EXITING";
            Save();
            EditorApplication.ExitPlaymode();
        }

        private static bool CaptureOrderedShutdown()
        {
            try
            {
                LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                BattleRuntimeShutdownReport shutdown = driver.ShutdownBattleRuntime();
                bool mapCleared = true;
                if (shutdown.RuntimeStagesCompleted)
                {
                    foreach (BattleBootstrap bootstrap in
                        Resources.FindObjectsOfTypeAll<BattleBootstrap>())
                    {
                        if (bootstrap == null || EditorUtility.IsPersistent(bootstrap) ||
                            !bootstrap.gameObject.scene.IsValid()) continue;
                        bootstrap.DisablePresentation();
                        mapCleared &= bootstrap.IsRuntimeMapCleared;
                    }
                    shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(mapCleared);
                }

                report.shutdownStatus = shutdown.Status.ToString();
                report.shutdownStage = shutdown.CompletedStage.ToString();
                report.shutdownFailure = shutdown.FailureReason;
                report.remainingWorldObjects = shutdown.RemainingWorldObjects;
                report.remainingRuntimeSlots = shutdown.RemainingRuntimeSlots;
                report.remainingPoolBorrowers = shutdown.RemainingPoolBorrowers;
                report.remainingActivePoolObjects =
                    pool?.ActiveObjectCountForAcceptance ?? 0;
                report.remainingActivePoolSprites =
                    pool?.ActiveSpriteCountForAcceptance ?? 0;
                report.poolQuiesced = pool == null || pool.IsQuiescedForDiagnostics;
                report.worldDetached = driver.World == null;
                report.orderedShutdownComplete = shutdown.IsComplete &&
                    shutdown.CompletedStage == BattleRuntimeShutdownStage.RuntimeMapCleared &&
                    report.worldDetached && report.poolQuiesced &&
                    report.remainingWorldObjects == 0 &&
                    report.remainingRuntimeSlots == 0 &&
                    report.remainingPoolBorrowers == 0 &&
                    report.remainingActivePoolObjects == 0 &&
                    report.remainingActivePoolSprites == 0;
                if (!report.orderedShutdownComplete)
                    report.error = "Ordered shutdown did not reach zero-residue postconditions.";
            }
            catch (Exception error)
            {
                report.error = "Ordered shutdown exception: " + error;
            }

            if (report.orderedShutdownComplete) return true;
            report.status = "FAIL";
            report.phase = "CLEANUP_BLOCKED";
            Save();
            return false;
        }

        private static void Fail(string message)
        {
            if (report == null)
            {
                Debug.LogError("[Q07 C056 fusion Play] " + message);
                return;
            }
            if (IsShutdownRun())
            {
                report.status = "CLEANUP_BLOCKED";
                report.error = message;
                report.phase = "CLEANUP_BLOCKED";
                Save();
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
            report.sceneHashAfter = HashScene();
            Scene scene = SceneManager.GetActiveScene();
            report.sceneCleanAfter = scene.path == BattleScene && !scene.isDirty &&
                report.sceneHashAfter == report.sceneHashBefore;
            if (IsShutdownRun())
            {
                report.noLiveWorldAfterExit = Resources.FindObjectsOfTypeAll<SimulationTickDriver>()
                    .All(value => value == null || value.World == null);
                if (!report.noLiveWorldAfterExit)
                {
                    report.status = "FAIL";
                    report.error += " Live Driver World remains after exit.";
                }
            }
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
            if (IsShutdownRun()) SessionState.EraseString(ShutdownRequestSessionKey);
            report = null;
            driver = null;
            world = null;
            primary = null;
            partner = null;
            stableTick = -1;
            stableUpdates = 0;
            shutdownSaveSequence = 0;
        }
    }
}
#endif
