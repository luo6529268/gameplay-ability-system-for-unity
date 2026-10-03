#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    [InitializeOnLoad]
    internal static class NTSD28Q07D024PlatformBattlePlayProbeEditor
    {
        private const string ScenePath = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string ContentRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string RequestPath = "Temp/NTSD28_Q07_D024_PlatformBattle.request.json";
        private const string OutputRoot =
            "artifacts/diagnostics/NTSD28-336B44-Q07-D024-PLATFORM-SCENE-001";
        private static bool pauseCaptured;
        private static bool wasPaused;
        private static int stableTick = -1;
        private static int stableUpdates;

        [Serializable]
        private sealed class Request
        {
            public bool requested;
            public bool running;
            public string runId;
            public long startedUtcTicks;
        }

        [Serializable]
        private sealed class Row
        {
            public int tick;
            public int sourceAction;
            public int sourceX;
            public int sourceY;
            public int targetAction;
            public int targetX;
            public int targetY;
            public int targetCollisionReference;
            public int targetPlatformSlot;
            public int targetShadowOffset;
            public double targetSourcePreciseX;
            public double targetViewPreciseX;
            public double targetExpectedViewX;
            public double targetViewResidualX;
        }

        [Serializable]
        private sealed class Report
        {
            public string status;
            public string error;
            public string runId;
            public string sceneHashBefore;
            public string sceneHashAfter;
            public string contentRoot;
            public int startTick;
            public int endTick;
            public int sourceSlot;
            public int targetSlot;
            public int objectsBefore;
            public int objectsAfter;
            public int slotsBefore;
            public int slotsAfter;
            public int borrowersBefore;
            public int borrowersAfter;
            public bool fixtureUnregistered;
            public double maxViewResidualX;
            public Row[] rows;
        }

        static NTSD28Q07D024PlatformBattlePlayProbeEditor()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            string path = ProjectPath(RequestPath);
            if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
                !File.Exists(path))
                return;
            Request request;
            try
            {
                request = JsonUtility.FromJson<Request>(File.ReadAllText(path));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || (!request.requested && !request.running))
                return;
            if (request.requested && !request.running)
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                    return;
                Scene scene = SceneManager.GetActiveScene();
                if (!ValidRunId(request.runId) || scene.path != ScenePath ||
                    scene.isDirty || File.Exists(ResultPath(request.runId)))
                {
                    Finish(request, new Report
                    {
                        status = "FAIL",
                        error = "Invalid run ID, result collision or unsaved Battle Scene."
                    });
                    return;
                }
                request.requested = false;
                request.running = true;
                request.startedUtcTicks = DateTime.UtcNow.Ticks;
                File.WriteAllText(path, JsonUtility.ToJson(request));
                EditorApplication.EnterPlaymode();
                return;
            }
            if (!request.running || !EditorApplication.isPlaying)
                return;
            var report = new Report
            {
                runId = request.runId,
                contentRoot = GameConfig.Instance?.BattleContentRuntimeRoot
            };
            try
            {
                Require(DateTime.UtcNow - new DateTime(request.startedUtcTicks,
                    DateTimeKind.Utc) < TimeSpan.FromMinutes(3),
                    "Battle Scene Play startup timed out.");
                SimulationTickDriver driver = SimulationTickDriver.Instance;
                SimulationWorld world = driver?.World;
                if (world == null || driver.CurrentTickIndex < 5 ||
                    !world.IsBattleSnapshotBoundaryReady)
                    return;
                Require(SceneManager.GetActiveScene().path == ScenePath &&
                    report.contentRoot == ContentRoot,
                    "Battle Scene or formal content root changed.");
                Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                    "Dedicated worker failed.");
                if (!pauseCaptured)
                {
                    wasPaused = driver.IsPaused;
                    driver.SetPaused(true);
                    pauseCaptured = true;
                    return;
                }
                if (!driver.IsPaused ||
                    driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                    return;
                if (stableTick != driver.CurrentTickIndex)
                {
                    stableTick = driver.CurrentTickIndex;
                    stableUpdates = 0;
                    return;
                }
                if (++stableUpdates < 4)
                    return;
                Run(request, driver, world, report);
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
                Finish(request, report);
            }
        }

        private static void Run(Request request, SimulationTickDriver driver,
            SimulationWorld world, Report report)
        {
            LF2Character source = null;
            LF2Character target = null;
            LF2ObjectPool pool = LF2ObjectPool.Instance;
            report.sceneHashBefore = HashFile(ProjectPath(ScenePath));
            report.objectsBefore = world.ObjectCount;
            report.slotsBefore = world.ClaimedRuntimeSlotCountForDiagnostics;
            report.borrowersBefore = pool.ActiveObjectCountForAcceptance;
            try
            {
                var sourceData = world.RuntimeCharacterConfigs.Resolve(56);
                var targetData = world.RuntimeCharacterConfigs.Resolve(2);
                Require(sourceData?.characterData != null &&
                    targetData?.characterData != null,
                    "Formal OID56/OID2 definitions are unavailable.");
                int sourceSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                int targetSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(
                    sourceSlot + 1, 1000);
                Require(sourceSlot >= 50 && targetSlot > sourceSlot,
                    "Two diagnostic runtime slots are required.");
                report.sourceSlot = sourceSlot;
                report.targetSlot = targetSlot;
                source = Create(world, sourceData, 56, sourceSlot, 1,
                    200, 0, 400, 182);
                target = Create(world, targetData, 2, targetSlot, 2,
                    205, -5, 400, 0);
                Require(source.Frame.D?.itrs != null &&
                    source.Frame.D.itrs.Exists(itr => itr.kind == 50) &&
                    source.Frame.N == 182 && source.Runtime.SourceRuleXInt == 200 &&
                    target.Runtime.SourceRuleXInt == 205 &&
                    target.Runtime.YInt == -5,
                    "Formal initial frame or source coordinates differ.");
                var rows = new List<Row>(11);
                int originTick = driver.CurrentTickIndex;
                report.startTick = originTick;
                rows.Add(Capture(0, source, target, sourceSlot, world));
                for (int tick = 1; tick <= 10; tick++)
                {
                    Require(driver.StepOneTick(ignorePaused: true,
                        buildPresentation: true),
                        "Production Driver rejected tick " + tick + ".");
                    Require(driver.CurrentTickIndex == originTick + tick,
                        "Driver tick index did not advance once.");
                    rows.Add(Capture(tick, source, target, sourceSlot, world));
                }
                report.endTick = driver.CurrentTickIndex;
                report.rows = rows.ToArray();
                foreach (Row row in report.rows)
                    report.maxViewResidualX = Math.Max(report.maxViewResidualX,
                        Math.Abs(row.targetViewResidualX));
                Require(report.rows.Length == 11 &&
                    report.rows[1].targetPlatformSlot == 1 &&
                    report.rows[10].targetPlatformSlot == 1,
                    "Formal platform link did not persist through ten ticks.");
                Require(report.maxViewResidualX < 1.0,
                    "Target physical X escaped source-to-view pixel tolerance.");
                report.status = "PASS_CONTROLLED_SCENE";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.error = error.ToString();
            }
            finally
            {
                if (target?.RegisteredWorldForSimulation == world)
                    world.Unregister(target);
                if (source?.RegisteredWorldForSimulation == world)
                    world.Unregister(source);
                report.fixtureUnregistered =
                    (target == null || target.RegisteredWorldForSimulation == null) &&
                    (source == null || source.RegisteredWorldForSimulation == null);
                report.objectsAfter = world.ObjectCount;
                report.slotsAfter = world.ClaimedRuntimeSlotCountForDiagnostics;
                report.borrowersAfter = pool.ActiveObjectCountForAcceptance;
                report.sceneHashAfter = HashFile(ProjectPath(ScenePath));
                if (!report.fixtureUnregistered ||
                    report.objectsAfter != report.objectsBefore ||
                    report.slotsAfter != report.slotsBefore ||
                    report.borrowersAfter != report.borrowersBefore ||
                    report.sceneHashAfter != report.sceneHashBefore)
                {
                    report.status = "FAIL";
                    report.error += " Cleanup counts or Battle Scene SHA changed.";
                }
                driver.SetPaused(wasPaused);
                Finish(request, report);
            }
        }

        private static LF2Character Create(SimulationWorld world,
            LF2CharacterDataWrapper data, int oid, int slot, int team,
            int x, int y, int z, int action)
        {
            var entity = new LF2Character();
            entity.ModuleInitialize();
            entity.ObjectId = oid;
            entity.Name = "Q07D024Platform" + oid;
            entity.FrameCache.Load(data);
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.ImmediateFrame(action);
            entity.Frame.Prev = action;
            entity.Frame.Prev2 = action;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Runtime.PrevFrame2 = action;
            entity.Initialize(500, 500);
            entity.AiControlled = false;
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Runtime.SetPosition(world.SpatialProjection.SourceToViewX(x),
                y, world.SpatialProjection.SourceToViewZ(z));
            entity.Runtime.SetSourceRulePosition(x, z);
            entity.Runtime.SyncSourceRuleIntegerPosition();
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.NativePreviousY104 = y;
            entity.RefreshRuntimeSnapshot();
            return entity;
        }

        private static Row Capture(int tick, LF2Character source,
            LF2Character target, int sourceSlot, SimulationWorld world)
        {
            double expected = world.SpatialProjection.SourceToViewX(
                target.Runtime.SourceRuleX);
            return new Row
            {
                tick = tick,
                sourceAction = source.Frame.N,
                sourceX = source.Runtime.SourceRuleXInt,
                sourceY = source.Runtime.YInt,
                targetAction = target.Frame.N,
                targetX = target.Runtime.SourceRuleXInt,
                targetY = target.Runtime.YInt,
                targetCollisionReference = target.Runtime.CollisionYReference,
                targetPlatformSlot = target.Runtime.PlatformSourceSlotF4 == sourceSlot
                    ? 1 : target.Runtime.PlatformSourceSlotF4,
                targetShadowOffset = target.Runtime.RenderShadowOffset10C,
                targetSourcePreciseX = target.Runtime.SourceRuleX,
                targetViewPreciseX = target.Runtime.X,
                targetExpectedViewX = expected,
                targetViewResidualX = target.Runtime.X - expected
            };
        }

        private static void Finish(Request request, Report report)
        {
            if (ValidRunId(request.runId))
            {
                string output = ResultPath(request.runId);
                Directory.CreateDirectory(Path.GetDirectoryName(output));
                if (!File.Exists(output))
                    File.WriteAllText(output, JsonUtility.ToJson(report, true));
            }
            request.requested = false;
            request.running = false;
            File.WriteAllText(ProjectPath(RequestPath), JsonUtility.ToJson(request));
            pauseCaptured = false;
            stableTick = -1;
            stableUpdates = 0;
            if (EditorApplication.isPlaying)
                EditorApplication.ExitPlaymode();
        }

        private static bool ValidRunId(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length > 80)
                return false;
            foreach (char item in value)
                if (!char.IsLetterOrDigit(item) && item != '-' && item != '_')
                    return false;
            return true;
        }

        private static string ResultPath(string runId) =>
            ProjectPath(OutputRoot + "/" + runId + ".json");

        private static string ProjectPath(string relative) => Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", relative));

        private static string HashFile(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(stream))
                    .Replace("-", string.Empty);
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }
    }
}
#endif
