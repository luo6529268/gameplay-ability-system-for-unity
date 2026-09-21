#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test.Editor
{
    public static class NTSD28B11SourceCallerPlayProbeEditor
    {
        [Serializable] private sealed class Request
        {
            public bool requested;
            public bool formalStaged;
            public bool useSerializedRoot;
            public string runId, mode, root, configAssetPath;
            public int[] sourceIds;
        }
        [Serializable] private sealed class Report
        {
            public string runId, mode, status, message, sourceKey, shutdownStage;
            public string failurePublishedKey, failureConfiguredRoot, failureLifecycle;
            public int[] sourceIds;
            public int worldObjects, trackedResources, resourceSurvivors, menuPreviousOwnerSurvivors, actualPoolBorrowers;
            public long candidateCacheHits;
            public bool menuReady, samePublishedKeys, stayedStoppedAfterTwoFrames;
            public bool serializedRootSelected;
            public string rootFromAsset;
        }

        private static bool running;
        private static string Root => Directory.GetParent(Application.dataPath).FullName;
        private static string RequestPath => Path.Combine(Root, "Temp/NTSD28_B11_SourceCaller.request.json");
        private static string Q07RequestPath => Path.Combine(Root, "Temp/NTSD28_Q07_StagedCaller.request.json");

        private static string ActiveRequestPath()
        {
            if (File.Exists(Q07RequestPath))
            {
                var q07 = JsonUtility.FromJson<Request>(File.ReadAllText(Q07RequestPath));
                if (q07 != null && q07.requested) return Q07RequestPath;
            }
            return RequestPath;
        }

        [InitializeOnLoadMethod]
        private static void Register()
        {
            EditorApplication.update -= Poll;
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (running || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
            string requestPath;
            Request request;
            try
            {
                requestPath = ActiveRequestPath();
                if (!File.Exists(requestPath)) return;
                request = JsonUtility.FromJson<Request>(File.ReadAllText(requestPath));
            }
            catch (IOException)
            {
                return;
            }
            if (request == null || !request.requested) return;
            if (!EditorApplication.isPlaying)
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    PrepareRequest(request);
                    File.WriteAllText(requestPath, JsonUtility.ToJson(request));
                    EditorApplication.EnterPlaymode();
                }
                return;
            }
            if (NTSD28B11SourceCallerPlaySetup.AttemptedRunId != request.runId) return;
            request.requested = false;
            File.WriteAllText(requestPath, JsonUtility.ToJson(request));
            running = true;
            Run(request).Forget();
        }

        private static async UniTask Run(Request request)
        {
            var report = new Report { runId = request.runId, mode = request.mode, status = "RUNNING" };
            string resultPath = null;
            GameConfig previousConfig = NTSD28B11SourceCallerPlaySetup.PreviousConfig;
            GameConfig ownedConfig = NTSD28B11SourceCallerPlaySetup.OwnedConfig;
            HashSet<UnityEngine.Object> menuPreviousOwnerResources = null;
            try
            {
                Require(!string.IsNullOrEmpty(request.runId) && request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid run ID.");
                Require(request.mode == "direct" || request.mode == "app" || request.mode == "menu", "Unknown caller mode.");
                resultPath = Path.Combine(Root, request.formalStaged
                    ? "artifacts/diagnostics/NTSD28-Q07-CONTENT-MIGRATION-READINESS-001"
                    : "artifacts/diagnostics/NTSD28-B11-SOURCE-CACHE-CALLER-PRODUCTION-001", request.runId + ".json");
                File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
                Require(string.IsNullOrEmpty(NTSD28B11SourceCallerPlaySetup.Failure) && ownedConfig != null &&
                    ReferenceEquals(GameConfig.Instance, ownedConfig), "Before-scene source setup failed: " + NTSD28B11SourceCallerPlaySetup.Failure);
                if (request.useSerializedRoot)
                {
                    var serializedConfig = AssetDatabase.LoadAssetAtPath<GameConfig>(request.configAssetPath);
                    report.rootFromAsset = serializedConfig?.BattleContentRuntimeRoot;
                    report.serializedRootSelected = request.mode == "menu" && request.formalStaged &&
                        request.configAssetPath == "Assets/NTSD/Config/GameConfig/GameConfig.asset" &&
                        report.rootFromAsset == request.root &&
                        ownedConfig.BattleContentRuntimeRoot == report.rootFromAsset;
                    Require(report.serializedRootSelected,
                        "The menu caller did not use the production GameConfig serialized root.");
                }
                report.sourceIds = request.sourceIds;

                if (request.mode != "direct")
                {
                    if (request.formalStaged)
                    {
                        float deadline = Time.realtimeSinceStartup + 240f;
                        while (!BattleTestBootstrap.ProductionStressServicesReady && Time.realtimeSinceStartup < deadline)
                            await UniTask.Yield();
                    }
                    else
                    {
                        for (int i = 0; i < 1800 && !BattleTestBootstrap.ProductionStressServicesReady; i++)
                            await UniTask.Yield();
                    }
                    Require(BattleTestBootstrap.ProductionStressServicesReady && SimulationTickDriver.Instance?.World?.ObjectCount == 0,
                        "The direct bootstrap must prepare native resources without creating actors for App/menu checks.");
                    if (request.mode == "menu")
                    {
                        var previousManager = CharacterAnimtorManager.TryGetInstance();
                        Require(previousManager != null, "A prewarmed owner is required for cache rehydration.");
                        if (request.formalStaged)
                            menuPreviousOwnerResources = CaptureOwnedResources(previousManager);
                        UnityEngine.Object.DestroyImmediate(previousManager.gameObject);
                        if (menuPreviousOwnerResources != null)
                        {
                            report.menuPreviousOwnerSurvivors = menuPreviousOwnerResources.Count(value => value != null);
                            Require(report.menuPreviousOwnerSurvivors == 0,
                                "The previous formal menu prewarm owner left publication resources alive.");
                        }
                        var go = new GameObject("E3_MenuPrewarmProbe");
                        go.SetActive(false);
                        var loading = go.AddComponent<LoadingPrewarmController>();
                        typeof(LoadingPrewarmController).GetField("runOnStart", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(loading, false);
                        go.SetActive(true);
                        await loading.PrewarmOnceAsync();
                        report.menuReady = loading.IsPrewarmed;
                        Require(report.menuReady, "The actual menu prewarm caller did not finish.");
                    }
                    var match = new MatchConfig { seed = 2833 };
                    for (int i = 0; i < 2; i++)
                        match.players.Add(new PlayerSlotConfig { use = true, isHuman = true,
                            characterId = report.sourceIds[i], team = i + 1, inputId = i + 1 });
                    AppManager.Instance.SetMatchConfig(match);
                    typeof(AppManager).GetMethod("InitializeBattleAsync", BindingFlags.Instance | BindingFlags.NonPublic)
                        .Invoke(AppManager.Instance, new object[] { SceneManager.GetActiveScene() });
                }

                SimulationTickDriver driver = null;
                for (int i = 0; i < 1800; i++)
                {
                    driver = SimulationTickDriver.Instance;
                    if (driver != null && driver.LifecycleState == BattleRuntimeLifecycleState.Running && driver.World?.ObjectCount > 0) break;
                    await UniTask.Yield();
                }
                Require(driver != null && driver.LifecycleState == BattleRuntimeLifecycleState.Running && driver.World?.ObjectCount > 0,
                    "The selected production caller did not reach a running native-content battle.");
                report.worldObjects = driver.World.ObjectCount;
                CharacterAnimtorManager manager = CharacterAnimtorManager.TryGetInstance();
                report.sourceKey = await manager.ValidateConfiguredContentForBattleAsync();
                report.samePublishedKeys = !string.IsNullOrEmpty(report.sourceKey) &&
                    GameDataManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey &&
                    CharacterUIResourceManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey;
                Require(report.samePublishedKeys, "Production owners disagree about the selected content.");
                foreach (int id in report.sourceIds)
                {
                    var data = manager.GetCharacterConfig(id)?.characterData;
                    if (request.formalStaged)
                        Require(data != null && !string.IsNullOrEmpty(data.name) && !data.name.StartsWith("E3_probe_", StringComparison.Ordinal),
                            "A formal staged definition did not reach the native caller: " + id);
                    else
                        Require(data?.name == "E3_probe_" + id, "A legacy definition reached the native caller.");
                }
                if (request.formalStaged)
                    Require(manager.PublishedLoganContentIdentity?.SemanticFingerprint ==
                        "FD18D668B9D4EF0FAD4EE3D8056F98754049B3F25FB6927EC562C3F60B008147",
                        "The production caller did not publish the formal staged content identity.");
                report.candidateCacheHits = manager.ConfiguredCandidateCacheHitCount;
                if (request.mode == "menu") Require(report.candidateCacheHits > 0, "Menu owner rehydration did not reuse the candidate input cache.");

                var resources = CaptureOwnedResources(manager);
                report.trackedResources = resources.Count;
                LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
                Require(AppManager.Instance.TryShutdownBattleRuntimeBeforeSceneDestroy(out BattleRuntimeShutdownReport shutdown), shutdown.FailureReason);
                report.shutdownStage = shutdown.CompletedStage.ToString();
                await UniTask.NextFrame();
                await UniTask.NextFrame();
                report.stayedStoppedAfterTwoFrames = driver.LifecycleState == BattleRuntimeLifecycleState.Stopped && driver.World == null;
                Require(report.stayedStoppedAfterTwoFrames, "A stopped caller restarted the battle.");
                Require(pool != null, "The actual pool owner is missing.");
                report.actualPoolBorrowers = pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
                Require(report.actualPoolBorrowers == 0 && pool.IsQuiescedForDiagnostics, "Battle borrowers remained after shutdown.");
                if (SceneManager.sceneCount == 1) SceneManager.CreateScene("E3_SourceCallerHost");
                AsyncOperation unload = AppManager.Instance.UnloadBattle();
                Require(unload != null, "Battle unload failed.");
                await unload.ToUniTask();
                await UniTask.NextFrame();
                report.resourceSurvivors = resources.Count(value => value != null);
                Require(report.resourceSurvivors == 0, "Native publication resources survived owner unload.");
                report.status = "PASS";
                report.message = request.formalStaged
                    ? "Actual formal-staged content caller, World preparation and ordered unload passed."
                    : "Actual configured-content caller, World preparation and ordered unload passed with a legal isolated source.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.message = error.ToString();
                report.failurePublishedKey = CharacterAnimtorManager.TryGetInstance()?.PublishedVisualContentKey;
                report.failureConfiguredRoot = GameConfig.Instance?.BattleContentRuntimeRoot;
                report.failureLifecycle = SimulationTickDriver.Instance?.LifecycleState.ToString();
            }
            finally
            {
                BattleTestBootstrap.SuppressEntityCreationForProductionStress = false;
                try
                {
                    if (SimulationTickDriver.Instance != null &&
                        SimulationTickDriver.Instance.LifecycleState != BattleRuntimeLifecycleState.Stopped)
                        AppManager.Instance?.TryShutdownBattleRuntimeBeforeSceneDestroy(out _);
                }
                catch (Exception cleanupError)
                {
                    report.status = "FAIL";
                    report.message += "\nProbe cleanup failed: " + cleanupError;
                }
                if (ReferenceEquals(GameConfig.Instance, ownedConfig))
                    typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, previousConfig);
                if (ownedConfig != null) UnityEngine.Object.DestroyImmediate(ownedConfig);
                if (resultPath != null) File.WriteAllText(resultPath, JsonUtility.ToJson(report, true));
                running = false;
                EditorApplication.delayCall += () => { if (EditorApplication.isPlaying) EditorApplication.ExitPlaymode(); };
            }
        }

        private static void PrepareRequest(Request request)
        {
            Require(!string.IsNullOrEmpty(request.runId) && request.runId.All(c => char.IsLetterOrDigit(c) || c == '-'), "Invalid run ID.");
            var template = request.useSerializedRoot
                ? AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/NTSD/Config/GameConfig/GameConfig.asset")
                : Resources.FindObjectsOfTypeAll<GameConfig>().FirstOrDefault(EditorUtility.IsPersistent);
            Require(template != null, "A current GameConfig asset is required before Play.");
            request.configAssetPath = AssetDatabase.GetAssetPath(template);
            if (request.useSerializedRoot)
            {
                Require(request.mode == "menu" && request.formalStaged &&
                    template.BattleContentRuntimeRoot == "Assets/NTSD/Content/LoganRuntime",
                    "The production GameConfig does not select formal content for menu prewarm.");
                request.sourceIds = new[] { 0, 50, 52 };
                request.root = template.BattleContentRuntimeRoot;
                return;
            }
            if (request.formalStaged)
            {
                request.sourceIds = new[] { 0, 50, 52 };
                request.root = "Assets/NTSD/Content/LoganRuntime";
                return;
            }
            var ids = new HashSet<int> { 0, 50, 52 };
            foreach (var bootstrap in Resources.FindObjectsOfTypeAll<BattleTestBootstrap>()
                .Where(value => value.gameObject.scene.IsValid() && value.gameObject.scene.isLoaded))
            {
                var overrides = (int[])typeof(BattleTestBootstrap).GetField("overrideCharacterIds", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bootstrap);
                if (overrides != null) foreach (int id in overrides) if (id >= 0) ids.Add(id);
            }
            request.sourceIds = ids.OrderBy(value => value).ToArray();
            request.root = CreateSource(request.runId, request.sourceIds);
        }

        private static HashSet<UnityEngine.Object> CaptureOwnedResources(CharacterAnimtorManager manager)
        {
            var resources = new HashSet<UnityEngine.Object>();
            foreach (string name in new[] { "publishedOwnedSprites", "publishedOwnedResources" })
                foreach (var value in (IEnumerable)typeof(CharacterAnimtorManager)
                    .GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager))
                    resources.Add((UnityEngine.Object)value);
            return resources;
        }

        private static string CreateSource(string runId, int[] ids)
        {
            string root = Path.Combine(Root, "Temp/E3SourceCaller", runId);
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/c"));
            var catalog = new StringBuilder("registry_section,registry_index,id,type,source_path,published_folder\n");
            for (int i = 0; i < ids.Length; i++)
            {
                string file = "probe" + ids[i] + ".dat";
                catalog.AppendLine("object," + i + "," + ids[i] + ",0," + file + ",missing");
                File.WriteAllText(Path.Combine(root, "decoded_dat", file), "<bmp_begin>\nname: E3_probe_" + ids[i] +
                    "\nhead: c/head.png\nsmall: c/small.png\nfile(0-0): c/body.png w: 5 h: 1 row: 1 col: 1\n<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n");
            }
            File.WriteAllText(Path.Combine(root, "catalog.csv"), catalog.ToString());
            byte[] pixels = File.ReadAllBytes(Path.Combine(Root, "Temp/NTSD28PngAlpha/fixture.dat"));
            foreach (string name in new[] { "body", "head", "small" }) File.WriteAllBytes(Path.Combine(root, "vfs/c", name + ".png"), pixels);
            return root;
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
