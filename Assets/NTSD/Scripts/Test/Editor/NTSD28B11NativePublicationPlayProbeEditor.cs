#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28B11NativePublicationPlayProbeEditor
    {
        [Serializable] private sealed class Request { public bool requested; public bool waitForRunning; public bool appBootstrap; public string runId; }
        [Serializable] private sealed class Report
        {
            public string runId, status, message, initialScene, initialLifecycle, shutdownStatus;
            public string shutdownStage, shutdownFailure;
            public string bootstrapMode;
            public int[] appCharacterIds;
            public int initialObjectCount, checksCompleted, trackedObjects, survivors;
            public int remainingWorldObjects, remainingRuntimeSlots, remainingPoolBorrowers;
            public int actualPoolBorrowersAfterShutdown;
            public bool remainedStoppedAfterTwoFrames;
            public bool activeBoundaryRejected;
            public string[] checks;
        }

        private static bool running;
        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(ProjectRoot, "Temp/NTSD28_B11_SourceAtomicPublication.request.json");

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath)) return;
            Request request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
            if (request == null || !request.requested) return;
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode) EditorApplication.EnterPlaymode();
                return;
            }
            request.requested = false;
            File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
            running = true;
            Run(request).Forget();
        }

        private static async UniTask Run(Request request)
        {
            var report = new Report { runId = request.runId, status = "RUNNING", initialScene = SceneManager.GetActiveScene().path };
            string resultPath = null;
            try
            {
                Require(!string.IsNullOrEmpty(request.runId) && request.runId.All(ch => char.IsLetterOrDigit(ch) || ch == '-'), "Invalid play probe run ID.");
                resultPath = Path.Combine(ProjectRoot, "artifacts/diagnostics/NTSD28-B11-SOURCE-ATOMIC-PUBLICATION-001", request.runId + ".json");
                File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
                report.bootstrapMode = request.appBootstrap ? "AppInitializeBattleAsync" : "DirectBattleTestBootstrap";
                if (request.appBootstrap)
                {
                    BattleTestBootstrap.SuppressEntityCreationForProductionStress = true;
                    for (int i = 0; i < 1800 && !BattleTestBootstrap.ProductionStressServicesReady; i++)
                        await UniTask.Yield();
                    Require(BattleTestBootstrap.ProductionStressServicesReady &&
                        SimulationTickDriver.Instance?.World?.ObjectCount == 0,
                        "App probe needs prepared resources and a World without direct-bootstrap entities.");
                    report.appCharacterIds = GameDataManager.TryGetInstance().GetObjectsByType(0)
                        .Where(value => CharacterAnimtorManager.TryGetInstance().GetCharacterConfig(value.id) != null)
                        .Take(2).Select(value => value.id).ToArray();
                    Require(report.appCharacterIds.Length == 2, "Two loaded App test characters are required.");
                    var match = new MatchConfig { seed = 2833 };
                    for (int i = 0; i < report.appCharacterIds.Length; i++)
                        match.players.Add(new PlayerSlotConfig { use = true, isHuman = true,
                            characterId = report.appCharacterIds[i], team = i + 1, inputId = i + 1 });
                    AppManager.Instance.SetMatchConfig(match);
                    typeof(AppManager).GetMethod("InitializeBattleAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(AppManager.Instance, new object[] { SceneManager.GetActiveScene() });
                }
                SimulationTickDriver driver = null;
                for (int i = 0; i < 1800; i++)
                {
                    driver = SimulationTickDriver.Instance;
                    if (driver != null && driver.World != null && driver.World.ObjectCount > 0 &&
                        (!request.waitForRunning || driver.LifecycleState == BattleRuntimeLifecycleState.Running) &&
                        CharacterAnimtorManager.TryGetInstance()?.IsPrewarmCompleted == true) break;
                    await UniTask.Yield();
                }
                Require(driver != null && driver.World != null && driver.World.ObjectCount > 0, "A real active battle world was not observed.");
                if (request.waitForRunning) Require(driver.LifecycleState == BattleRuntimeLifecycleState.Running, "A fully running battle was not observed.");
                report.initialLifecycle = driver.LifecycleState.ToString();
                report.initialObjectCount = driver.World.ObjectCount;

                var factory = new NTSD28B11AtomicPublicationEditorTests();
                var make = factory.GetType().GetMethod("Candidate", BindingFlags.Instance | BindingFlags.NonPublic);
                var candidate = (LoganVisualContentCandidate)make.Invoke(factory, new object[] { "Gate", false });
                try { await CharacterAnimtorManager.TryGetInstance().LoadLoganContentAsync(candidate); }
                catch (InvalidOperationException error) { report.activeBoundaryRejected = error.Message.Contains("active battle boundary"); }
                Require(report.activeBoundaryRejected, "Public native publication did not reject the real active battle.");

                var app = AppManager.Instance;
                LF2ObjectPool actualPool = LF2ObjectPool.TryGetInstance();
                if (app != null)
                {
                    bool stopped = app.TryShutdownBattleRuntimeBeforeSceneDestroy(out BattleRuntimeShutdownReport shutdown);
                    RecordShutdown(report, shutdown);
                    Require(stopped, "Existing App ordered shutdown did not complete: " + shutdown.FailureReason);
                    await VerifyStoppedContinuation(report, driver, actualPool);
                    if (SceneManager.sceneCount == 1) SceneManager.CreateScene("NTSD28_E2_PlayProbeHost");
                    AsyncOperation unload = app.UnloadBattle();
                    Require(unload != null, "Existing App battle unload did not start.");
                    await unload.ToUniTask();
                }
                else
                {
                    driver.ShutdownBattleRuntime();
                    var bootstraps = UnityEngine.Object.FindObjectsOfType<BattleBootstrap>(true);
                    foreach (BattleBootstrap bootstrap in bootstraps) bootstrap.DisablePresentation();
                    BattleRuntimeShutdownReport shutdown = driver.CompleteBattleRuntimeShutdownAfterMapCleanup(bootstraps.All(value => value.IsRuntimeMapCleared));
                    Require(shutdown.IsComplete, "Existing Driver and map owners did not complete shutdown.");
                    RecordShutdown(report, shutdown);
                    await VerifyStoppedContinuation(report, driver, actualPool);
                }
                report.checks = new[]
                {
                    "FullPrewarm_PublishesAllViewsBeforeEvent_AndRebindsBeforeRetirement",
                    "CancelAfterBodyStaging_PreservesPublishedSource_AndAllowsRetry",
                    "DriverShutdown_CancelsAndRecyclesUnpublishedImagesInOrderedStages",
                    "LegacyOwnedUiImages_TransferAfterNativeRebind_WhileBorrowedAssetsSurvive"
                };
                foreach (string check in report.checks)
                {
                    var fixture = new NTSD28B11AtomicPublicationEditorTests();
                    var tracked = new List<UnityEngine.Object>();
                    try
                    {
                        fixture.SetUp();
                        IEnumerator routine = (IEnumerator)fixture.GetType().GetMethod(check).Invoke(fixture, null);
                        while (routine.MoveNext()) await UniTask.Yield();
                        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                        tracked.AddRange((IEnumerable<UnityEngine.Object>)fixture.GetType().GetField("owned", flags).GetValue(fixture));
                        var owner = (CharacterAnimtorManager)fixture.GetType().GetField("manager", flags).GetValue(fixture);
                        foreach (string name in new[] { "publishedOwnedSprites", "publishedOwnedResources", "legacyOwnedUiSprites", "legacyOwnedUiResources" })
                            tracked.AddRange(((IEnumerable)typeof(CharacterAnimtorManager).GetField(name, flags).GetValue(owner)).Cast<UnityEngine.Object>());
                        report.checksCompleted++;
                    }
                    finally { fixture.TearDown(); }
                    report.trackedObjects += tracked.Count;
                    report.survivors += tracked.Count(value => value != null);
                    Require(report.survivors == 0, "Fixture-owned Unity resources survived teardown.");
                    File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
                }
                report.status = "PASS";
                report.message = "Real Play native publication, rollback, ordered cancellation and owned-resource teardown passed.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.message = error.ToString();
            }
            finally
            {
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
                if (resultPath != null) File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
                running = false;
                EditorApplication.delayCall += () => { if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); };
            }
        }

        private static void Require(bool value, string message)
        {
            if (!value) throw new InvalidOperationException(message);
        }

        private static async UniTask VerifyStoppedContinuation(Report report, SimulationTickDriver driver, LF2ObjectPool pool)
        {
            // Keep the stopped Scene alive so a late bootstrap continuation must reject itself.
            await UniTask.NextFrame();
            await UniTask.NextFrame();
            Require(pool != null, "Actual pool owner was not captured by the Play probe.");
            report.actualPoolBorrowersAfterShutdown = pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
            report.remainedStoppedAfterTwoFrames = driver != null &&
                driver.LifecycleState == BattleRuntimeLifecycleState.Stopped && driver.World == null;
            Require(report.remainedStoppedAfterTwoFrames, "A late bootstrap continuation restarted the stopped battle.");
            Require(report.actualPoolBorrowersAfterShutdown == 0 && pool.IsQuiescedForDiagnostics &&
                !pool.AcceptingRequestsForDiagnostics, "Actual pool still has borrowers or reopened after shutdown.");
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.TryGetInstance();
            Require(factory != null && factory.PendingTaskCountForDiagnostics == 0 &&
                !factory.AcceptingSpawnRequestsForDiagnostics, "Actual spawn owner was not drained and closed.");
        }

        private static void RecordShutdown(Report report, BattleRuntimeShutdownReport shutdown)
        {
            report.shutdownStatus = shutdown.Status.ToString();
            report.shutdownStage = shutdown.CompletedStage.ToString();
            report.shutdownFailure = shutdown.FailureReason;
            report.remainingWorldObjects = shutdown.RemainingWorldObjects;
            report.remainingRuntimeSlots = shutdown.RemainingRuntimeSlots;
            report.remainingPoolBorrowers = shutdown.RemainingPoolBorrowers;
        }
    }
}
#endif
