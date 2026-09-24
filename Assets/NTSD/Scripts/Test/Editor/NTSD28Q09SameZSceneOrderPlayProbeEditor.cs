#if UNITY_EDITOR
using System;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28Q09SameZSceneOrderPlayProbeEditor
    {
        private const string RequestPath = "Temp/NTSD28_Q09_SameZSceneOrder.request";
        private const string ResultPath = "Temp/NTSD28_Q09_SameZSceneOrder.result.json";
        private static bool running;
        private static bool requestMode;
        private static bool pauseCaptured;
        private static bool previousPaused;
        private static double deadline;
        private static SimulationTickDriver driver;
        private static SimulationWorld world;
        private static string waitingFor;

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Update;
            EditorApplication.update += Update;
        }

        [MenuItem("NTSD/Battle Diagnostics/Q09/Run Same Z Scene Order Play Probe")]
        public static void Run()
        {
            if (running)
                return;
            Start(false);
        }

        private static void Start(bool fromRequest)
        {
            running = true;
            requestMode = fromRequest;
            pauseCaptured = false;
            driver = null;
            world = null;
            waitingFor = "original Battle Scene Play";
            deadline = EditorApplication.timeSinceStartup + 180.0;
        }

        private static void Update()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (!running)
            {
                string path = ProjectPath(RequestPath);
                if (!File.Exists(path))
                    return;
                Start(true);
            }

            try
            {
                Require(EditorApplication.timeSinceStartup < deadline,
                    "Timed out waiting for " + waitingFor + ".");
                if (!EditorApplication.isPlaying)
                {
                    Require(!pauseCaptured, "Play ended while awaiting worker completion.");
                    Require(requestMode, "Menu probe requires existing Battle Scene Play.");
                    Scene battle = SceneManager.GetSceneByName("NTSD_Battle");
                    Require(battle.IsValid() && battle.isLoaded,
                        "Original NTSD_Battle scene must already be loaded.");
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                        EditorApplication.EnterPlaymode();
                    return;
                }

                Scene scene = SceneManager.GetSceneByName("NTSD_Battle");
                Require(scene.IsValid() && scene.isLoaded, "NTSD_Battle scene is not loaded.");
                if (requestMode && File.Exists(ProjectPath(RequestPath)))
                    File.Delete(ProjectPath(RequestPath));
                if (!pauseCaptured)
                {
                    waitingFor = "production driver World and tick >= 5";
                    driver = SimulationTickDriver.Instance;
                    world = driver?.World;
                    if (world == null || driver.CurrentTickIndex < 5)
                        return;
                    Require(driver.PresentationBackendMode == BattlePresentationBackendMode.CentralOnly,
                        "Production presentation backend must be CentralOnly.");
                    previousPaused = driver.IsPaused;
                    pauseCaptured = true;
                    driver.SetPaused(true);
                    waitingFor = "paused dedicated worker completion";
                    deadline = EditorApplication.timeSinceStartup + 5.0;
                }
                Require(driver != null && ReferenceEquals(driver.World, world),
                    "Production World changed while waiting for worker completion.");
                if (driver.DedicatedSimulationWorkerTickInFlightForDiagnostics)
                    return;
                Require(driver.DedicatedSimulationWorkerFailureForDiagnostics == null,
                    "Dedicated worker reported a failure.");
                Execute();
            }
            catch (Exception exception)
            {
                Finish(new Report
                {
                    status = "FAIL",
                    message = exception.ToString(),
                    driverAvailableAtFailure = driver != null,
                    worldAvailableAtFailure = driver?.World != null,
                    tickAtFailure = driver?.CurrentTickIndex ?? -1,
                });
            }
        }

        private static void Execute()
        {
            var report = new Report
            {
                status = "FAIL",
                tick = driver.CurrentTickIndex,
                baselineObjects = world.ObjectCount,
                baselineClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics,
                backend = driver.PresentationBackendMode.ToString(),
                scenePath = SceneManager.GetSceneByName("NTSD_Battle").path,
            };
            PresentationFixture low = null;
            PresentationFixture high = null;
            try
            {
                int lowSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(50, 1000);
                Require(lowSlot >= 50, "No free lower fixture slot in [50, 1000).");
                low = new PresentationFixture();
                low.SetRequiredRuntimeSlot(lowSlot);
                world.Register(low);
                int highSlot = world.FindFirstFreeRuntimeSlotForDiagnostics(lowSlot + 1, 1000);
                Require(highSlot > lowSlot, "No distinct higher fixture slot is available.");
                high = new PresentationFixture();
                high.SetRequiredRuntimeSlot(highSlot);
                world.Register(high);
                Require(world.TryGetCurrentRuntimeHandleForDiagnostics(lowSlot, low, out var lowHandle)
                    && lowHandle.IsValid, "Lower fixture has no live runtime handle.");
                Require(world.TryGetCurrentRuntimeHandleForDiagnostics(highSlot, high, out var highHandle)
                    && highHandle.IsValid, "Higher fixture has no live runtime handle.");
                report.lowerSlot = lowSlot;
                report.higherSlot = highSlot;
                report.lowerHandle = $"slot={lowHandle.Slot},generation={lowHandle.Generation}";
                report.higherHandle = $"slot={highHandle.Slot},generation={highHandle.Generation}";
                report.lowerStableId = low.StableId;
                report.higherStableId = high.StableId;
                report.z = low.Runtime.ZInt;
                Require(high.Runtime.ZInt == report.z, "Fixture depths differ.");
                world.RenderDispatchAll(report.tick);
                BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
                Require(frame != null, "Production dispatch did not publish a frame.");
                world.BattlePresentation.MaterializePresentationOrder(world, frame);
                Require(frame.PresentationOrderMaterialized, "Published painter order is not materialized.");
                report.entityCount = frame.EntityCount;
                report.publishedCommandCount = frame.CommandCount;
                report.lowerRank = -1;
                report.higherRank = -1;
                for (int index = 0; index < frame.EntityCount; index++)
                {
                    BattlePresentationEntitySnapshot entry = frame.GetEntity(index);
                    if (entry.Handle.Equals(lowHandle))
                        report.lowerRank = index;
                    if (entry.Handle.Equals(highHandle))
                        report.higherRank = index;
                }
                report.lowerSortingOrder = low.GetRenderSortingOrder();
                report.higherSortingOrder = high.GetRenderSortingOrder();
                Require(report.higherRank >= 0 && report.lowerRank > report.higherRank,
                    "Published order did not paint higher physical slot before lower at equal Z.");
                Require(report.lowerSortingOrder == report.lowerRank * 4 + 1 &&
                    report.higherSortingOrder == report.higherRank * 4 + 1,
                    "Production sorting orders differ from materialized painter ranks.");
                Require(driver.CurrentTickIndex == report.tick,
                    "Logic tick advanced during paused publication witness.");
                report.status = "PASS";
                report.message = "Original Battle World equal-Z publication and runtime rank agree. " +
                    "Logic-only fixtures have no sprite; GPU submission and overlap pixels are not witnessed.";
            }
            catch (Exception exception)
            {
                report.message = exception.ToString();
            }
            finally
            {
                try
                {
                    if (high != null)
                        world.Unregister(high);
                    if (low != null)
                        world.Unregister(low);
                    world.RenderDispatchAll(report.tick);
                    report.finalObjects = world.ObjectCount;
                    report.finalClaimedSlots = world.ClaimedRuntimeSlotCountForDiagnostics;
                    report.cleanupPassed = report.finalObjects == report.baselineObjects &&
                        report.finalClaimedSlots == report.baselineClaimedSlots;
                    Require(report.cleanupPassed, "Cleanup did not restore object/slot baselines.");
                }
                catch (Exception exception)
                {
                    report.status = "FAIL";
                    report.message += " Cleanup: " + exception;
                }
                Finish(report);
            }
        }

        private static void Finish(Report report)
        {
            bool exit = requestMode;
            try
            {
                if (exit && File.Exists(ProjectPath(RequestPath)))
                    File.Delete(ProjectPath(RequestPath));
                if (pauseCaptured && driver != null)
                {
                    driver.SetPaused(previousPaused);
                    report.pauseRestored = driver.IsPaused == previousPaused;
                }
                string path = ProjectPath(ResultPath);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonUtility.ToJson(report, true));
                Debug.Log("[NTSD28Q09SameZSceneOrder] " + report.status + ": " + report.message);
            }
            finally
            {
                running = false;
                pauseCaptured = false;
                driver = null;
                world = null;
                if (exit && EditorApplication.isPlaying)
                    EditorApplication.delayCall += () => EditorApplication.ExitPlaymode();
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static string ProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private sealed class PresentationFixture : LF2Entity
        {
            internal PresentationFixture()
            {
                ObjectId = 10909;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 1;
                Health.HPBound = 1;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                Frame.D = new LF2FrameData { frameId = 0, state = 3005, pic = 999, wait = 1000000, next = 0 };
                Runtime.LinkState = 0;
                Runtime.SetPosition(0, 0, 240);
                Runtime.SyncIntegerPosition();
                RefreshRuntimeSnapshot();
            }

            public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Other;
            public override int GetCurrentDataObjectTypeForSimulation() => (int)LF2ObjectType.Other;
            public override void Reset() { }
            public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer) { }
        }

        [Serializable]
        private sealed class Report
        {
            public string status, message, backend, scenePath, lowerHandle, higherHandle;
            public int tick, lowerSlot, higherSlot, lowerStableId, higherStableId, z;
            public int lowerRank, higherRank, lowerSortingOrder, higherSortingOrder;
            public int entityCount, publishedCommandCount, baselineObjects, finalObjects;
            public int baselineClaimedSlots, finalClaimedSlots;
            public bool cleanupPassed, pauseRestored;
            public bool fixtureGpuSubmissionWitnessed = false;
            public bool driverAvailableAtFailure, worldAvailableAtFailure;
            public int tickAtFailure;
        }
    }
}
#endif
