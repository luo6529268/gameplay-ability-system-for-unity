#if DEVELOPMENT_BUILD && !UNITY_EDITOR
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
using NTSD.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NTSD.Test
{
    internal sealed class NTSD28Q07WindowsPlayerRuntimeProbe : MonoBehaviour
    {
        [Serializable]
        private sealed class Report
        {
            public string status;
            public string message;
            public string configuredRoot;
            public string selectionMode;
            public string serializedRootBefore;
            public string sourceKey;
            public string fingerprint;
            public string shutdownStage;
            public int worldObjects;
            public int trackedResources;
            public int resourceSurvivors;
            public int activePoolBorrowers;
            public bool samePublishedKeys;
            public bool sceneConfigMatched;
            public bool stayedStoppedAfterTwoFrames;
        }

        private const string FormalRoot = "Assets/NTSD/Content/LoganRuntime";
        private const string FormalFingerprint =
            "FD18D668B9D4EF0FAD4EE3D8056F98754049B3F25FB6927EC562C3F60B008147";
        private static string reportPath;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            string[] args = Environment.GetCommandLineArgs();
            bool useSerializedRoot = args.Contains("-ntsd-q07-serialized-root-probe");
            if (!useSerializedRoot && !args.Contains("-ntsd-q07-player-probe")) return;
            reportPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                "q07-player-runtime.json");
            var config = GameConfig.Instance;
            var bootstrap = FindObjectOfType<BattleTestBootstrap>(true);
            var sceneConfig = bootstrap == null ? null :
                (GameConfig)typeof(BattleTestBootstrap)
                    .GetField("gameConfig", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(bootstrap);
            if (config == null) config = sceneConfig;
            if (config == null)
                config = Resources.FindObjectsOfTypeAll<GameConfig>().FirstOrDefault();
            string serializedRootBefore = config == null ? string.Empty : config.BattleContentRuntimeRoot;
            if (config != null)
            {
                if (!useSerializedRoot) config.BattleContentRuntimeRoot = FormalRoot;
                if (GameConfig.Instance == null) GameConfig.Instance = config;
            }

            var go = new GameObject("NTSD28_Q07_WindowsPlayerRuntimeProbe");
            DontDestroyOnLoad(go);
            var probe = go.AddComponent<NTSD28Q07WindowsPlayerRuntimeProbe>();
            probe.config = config;
            probe.useSerializedRoot = useSerializedRoot;
            probe.serializedRootBefore = serializedRootBefore;
            probe.sceneConfigMatched = ReferenceEquals(config, sceneConfig);
        }

        private GameConfig config;
        private bool useSerializedRoot;
        private string serializedRootBefore;
        private bool sceneConfigMatched;

        private async void Start()
        {
            var report = new Report
            {
                status = "RUNNING",
                configuredRoot = config == null ? string.Empty : config.BattleContentRuntimeRoot,
                selectionMode = useSerializedRoot ? "serialized" : "injected",
                serializedRootBefore = serializedRootBefore,
                sceneConfigMatched = sceneConfigMatched,
            };
            try
            {
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                Require(config != null && GameConfig.Instance == config &&
                    config.BattleContentRuntimeRoot == FormalRoot,
                    "The Development Player did not select the formal sidecar before battle Start.");
                if (useSerializedRoot)
                    Require(sceneConfigMatched && serializedRootBefore == FormalRoot,
                        "The scene's serialized GameConfig did not supply the formal root.");

                SimulationTickDriver driver = null;
                float deadline = Time.realtimeSinceStartup + 480f;
                while (Time.realtimeSinceStartup < deadline)
                {
                    driver = SimulationTickDriver.Instance;
                    if (driver != null && driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                        driver.World?.ObjectCount > 0)
                        break;
                    await UniTask.Yield();
                }
                Require(driver != null && driver.LifecycleState == BattleRuntimeLifecycleState.Running &&
                    driver.World?.ObjectCount > 0, "Formal Player battle never reached a Running World.");
                report.worldObjects = driver.World.ObjectCount;

                var manager = CharacterAnimtorManager.TryGetInstance();
                Require(manager != null, "Formal Player content manager is missing.");
                report.sourceKey = await manager.ValidateConfiguredContentForBattleAsync();
                report.fingerprint = manager.PublishedLoganContentIdentity?.SemanticFingerprint;
                report.samePublishedKeys = !string.IsNullOrEmpty(report.sourceKey) &&
                    GameDataManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey &&
                    CharacterUIResourceManager.TryGetInstance()?.PublishedVisualContentKey == report.sourceKey;
                Require(report.fingerprint == FormalFingerprint && report.samePublishedKeys,
                    "The built Player did not publish the formal content to all three owners.");
                foreach (int id in new[] { 0, 50, 52 })
                {
                    var data = manager.GetCharacterConfig(id)?.characterData;
                    Require(data != null && !string.IsNullOrEmpty(data.name),
                        "Formal Player object definition is missing: " + id);
                }

                var resources = new HashSet<UnityEngine.Object>();
                foreach (string fieldName in new[] { "publishedOwnedSprites", "publishedOwnedResources" })
                    foreach (object value in (IEnumerable)typeof(CharacterAnimtorManager)
                        .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(manager))
                        resources.Add((UnityEngine.Object)value);
                report.trackedResources = resources.Count;
                Require(report.trackedResources > 0, "Formal Player publication owns no decoded resources.");

                var pool = LF2ObjectPool.TryGetInstance();
                var app = AppManager.Instance;
                Require(app != null, "Formal Player AppManager is missing.");
                bool shutdownSucceeded = app.TryShutdownBattleRuntimeBeforeSceneDestroy(
                    out BattleRuntimeShutdownReport shutdown);
                Require(shutdownSucceeded, "Formal Player ordered shutdown failed.");
                report.shutdownStage = shutdown.CompletedStage.ToString();
                await UniTask.NextFrame();
                await UniTask.NextFrame();
                report.stayedStoppedAfterTwoFrames = driver.LifecycleState == BattleRuntimeLifecycleState.Stopped &&
                    driver.World == null;
                Require(report.stayedStoppedAfterTwoFrames, "Player battle restarted after shutdown.");
                Require(pool != null, "Player battle pool is missing.");
                report.activePoolBorrowers = pool.ActiveObjectCountForAcceptance + pool.ActiveSpriteCountForAcceptance;
                Require(report.activePoolBorrowers == 0 && pool.IsQuiescedForDiagnostics,
                    "Player battle pool retained active borrowers.");

                if (SceneManager.sceneCount == 1) SceneManager.CreateScene("Q07_PlayerHost");
                AsyncOperation unload = app.UnloadBattle();
                Require(unload != null, "Player battle unload failed.");
                await unload.ToUniTask();
                await UniTask.NextFrame();
                report.resourceSurvivors = resources.Count(value => value != null);
                Require(report.resourceSurvivors == 0, "Player formal publication resources survived unload.");
                report.status = "PASS";
                report.message = "Built Windows Player selected formal sidecar, ran battle and closed cleanly.";
            }
            catch (Exception error)
            {
                report.status = "FAIL";
                report.message = error.ToString();
            }
            finally
            {
                File.WriteAllText(reportPath, JsonUtility.ToJson(report, true));
                Application.Quit(report.status == "PASS" ? 0 : 1);
            }
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
#endif
