#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class M12MatchImageSceneFlowEditorTests
    {
        private const string MenuScene = "Assets/NTSD/Scene/NTSD_Menu.unity";
        private const string BattleScene = "Assets/NTSD/Scene/NTSD_Battle.unity";
        private const string ConfigAsset = "Assets/NTSD/Config/GameConfig/GameConfig.asset";
        private const string Output = "artifacts/diagnostics/M12-match-image-residency-20261013/scene-flow-01";
        private const string Session = "NTSD.M12.MatchImageSceneFlow.";
        private static readonly int[] Roots = { 28, 18, 65, 23, 8, 10, 66, 27 };
        private const string CoverageInput = "artifacts/diagnostics/M12-full-image-coverage-20261014/scene-cases-03.json";
        private const string CoverageOutput = "artifacts/diagnostics/M12-full-image-coverage-20261014/scene-flow-01";
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        private sealed class SceneCase
        {
            public string Name;
            public int[] Roots;
            public string ExpectedPlanPath;
            public string ExpectedPlanSha256;
        }

        private static SceneCase[] Cases()
        {
            if (!SessionState.GetBool(Session + "full-coverage", false))
                return new[]
                {
                    new SceneCase { Name = "fixed-eight-0", Roots = Roots,
                        ExpectedPlanPath = "artifacts/diagnostics/M12-match-image-residency-20261013/selector-worker/main-selection-02/match.json" },
                    new SceneCase { Name = "fixed-eight-1", Roots = Roots,
                        ExpectedPlanPath = "artifacts/diagnostics/M12-match-image-residency-20261013/selector-worker/main-selection-02/match.json" }
                };
            Assert.That(Hash(CoverageInput), Is.EqualTo(SessionState.GetString(Session + "case-input-sha", "")), "Coverage cases changed across Play reload.");
            var cases = JObject.Parse(File.ReadAllText(CoverageInput))["cases"].ToObject<SceneCase[]>();
            Assert.That(cases.Length, Is.GreaterThanOrEqualTo(3));
            foreach (SceneCase item in cases)
            {
                Assert.That(item.Roots.Length, Is.InRange(1, 8));
                Assert.That(item.Roots.Distinct().Count(), Is.EqualTo(item.Roots.Length));
                Assert.That(Hash(item.ExpectedPlanPath), Is.EqualTo(item.ExpectedPlanSha256));
            }
            CollectionAssert.AreEqual(cases[0].Roots, cases[cases.Length - 1].Roots, "Final case must re-enter the first roster after a different roster.");
            Assert.That(cases.Any(item => !item.Roots.SequenceEqual(cases[0].Roots)), Is.True);
            return cases;
        }
        private static UniTaskCompletionSource uploadGate;

        private static string Hash(string path)
        {
            using var hash = SHA256.Create();
            return BitConverter.ToString(hash.ComputeHash(File.ReadAllBytes(path))).Replace("-", string.Empty);
        }

        private static T Field<T>(object owner, string name) =>
            (T)owner.GetType().GetField(name, Private).GetValue(owner);

        private static void Save(string phase, object value)
        {
            string output = SessionState.GetBool(Session + "full-coverage", false) ? CoverageOutput : Output;
            string directory = Path.Combine(output, SessionState.GetString(Session + "run", "missing"));
            Directory.CreateDirectory(directory);
            using var writer = new StreamWriter(new FileStream(Path.Combine(directory, phase + ".json"), FileMode.CreateNew));
            writer.Write(JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        private static MatchConfig Match(int[] roots)
        {
            var result = new MatchConfig { gameMode = new GameModeConfig { gameModeId = 1, battleGameModeId = 1 }, seed = 314159 };
            for (int index = 0; index < roots.Length; index++)
                result.players.Add(new PlayerSlotConfig { use = true, characterId = roots[index],
                    isHuman = index == 0, inputId = index + 1, team = index % 2 + 1, aiId = index + 1 });
            return result;
        }

        private static IEnumerable Until(Func<bool> predicate, float seconds, string label)
        {
            float deadline = Time.realtimeSinceStartup + seconds;
            while (!predicate())
            {
                Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), label);
                yield return null;
            }
        }

        private static int[] UiTextureIds(CharacterAnimtorManager manager)
        {
            var ui = CharacterUIResourceManager.TryGetInstance();
            return Field<Dictionary<int, LF2CharacterDataWrapper>>(manager, "TotalCharacterFrameConfig").Keys
                .SelectMany(id =>
                {
                    var value = ui.GetCharacterUISprites(id);
                    return new[] { value?.HeadSprite, value?.SmallSprite };
                }).Where(value => value != null).Select(value => value.texture.GetInstanceID()).Distinct().OrderBy(id => id).ToArray();
        }

        private static UnityEngine.Object[] BodyResources(CharacterAnimtorManager manager)
        {
            var uiIds = new HashSet<int>(UiTextureIds(manager));
            return Field<HashSet<UnityEngine.Object>>(manager, "publishedOwnedResources")
                .Where(value => value != null && !uiIds.Contains(value.GetInstanceID())).ToArray();
        }

        private static void Snapshot(string phase, CharacterAnimtorManager manager)
        {
            var driver = SimulationTickDriver.Instance;
            var owned = Field<HashSet<UnityEngine.Object>>(manager, "publishedOwnedResources").Where(value => value != null).ToArray();
            long textureBytes = owned.OfType<Texture>().Sum(value => Profiler.GetRuntimeMemorySizeLong(value));
            long processBytes;
            using (var process = System.Diagnostics.Process.GetCurrentProcess()) processBytes = process.PrivateMemorySize64;
            Save(phase, new
            {
                utc = DateTime.UtcNow.ToString("O"), phase, appState = AppManager.Instance.State.ToString(),
                lifecycle = driver?.LifecycleState.ToString(), driverInstance = driver != null ? driver.GetInstanceID() : 0,
                tick = driver?.CurrentTickIndex, worldObjects = driver?.World?.ObjectCount,
                claimedSlots = driver?.World?.ClaimedRuntimeSlotCountForServices,
                plan = manager.PublishedImageLoadPlan?.Phase.ToString(),
                planFingerprint = manager.PublishedImageLoadPlanFingerprint,
                definitionIds = manager.PublishedImageLoadPlan?.DefinitionIds,
                missingDependencies = manager.PublishedImageLoadPlan?.MissingDependencies,
                residentBodyDefinitions = manager.ResidentBodyDefinitionCount,
                selectedBodyDeclarations = manager.LastSelectedBodySheetDeclarationCount,
                allDatDefinitions = Field<Dictionary<int, LF2CharacterDataWrapper>>(manager, "TotalCharacterFrameConfig").Count,
                uiTextureIds = UiTextureIds(manager), ownedResourceIds = owned.Select(value => value.GetInstanceID()).ToArray(),
                ownedTextureRuntimeBytes = textureBytes,
                editorProcessPrivateBytes = processBytes > 0 ? (long?)processBytes : null,
                processMemoryStatus = processBytes > 0 ? "AVAILABLE" : "UNAVAILABLE_MONO_API",
                activeCase = SessionState.GetString(Session + "active-case", ""),
                bodyBindings = manager.SpriteCatalog.Entries.Values.Select(entry => new
                {
                    definitionId = entry.Key.VisualDataId, pic = entry.Key.EffectivePic, entry.SourceSheetPath,
                    textureId = entry.SharedTexture != null ? entry.SharedTexture.GetInstanceID() : 0,
                    width = entry.SharedTexture != null ? entry.SharedTexture.width : 0,
                    height = entry.SharedTexture != null ? entry.SharedTexture.height : 0,
                    format = entry.SharedTexture != null ? entry.SharedTexture.format.ToString() : "missing"
                }).ToArray(),
                astcSupported = SystemInfo.SupportsTextureFormat(TextureFormat.ASTC_4x4), maxTextureSize = SystemInfo.maxTextureSize,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                textureFormats = owned.OfType<Texture2D>().GroupBy(value => value.format.ToString())
                    .ToDictionary(group => group.Key, group => group.Count()),
                actualImagePreflight = manager.LastNativeImageMemoryBudget?.Describe(),
                atlasDiagnostic = manager.LastAtlasDiagnostic,
                pendingPrewarm = manager.HasPendingNativePrewarmWork,
                catalogLeases = Field<Dictionary<BattleSpriteCatalog, int>>(manager, "spriteCatalogRendererBindings").Values.Sum(),
                pools = Resources.FindObjectsOfTypeAll<LF2ObjectPool>().Where(pool => pool != null && pool.gameObject.scene.IsValid())
                    .Select(pool => new { pool.ActiveObjectCountForAcceptance, pool.ActiveSpriteCountForAcceptance,
                        quiesced = pool.IsQuiescedForDiagnostics }).ToArray(),
                boundary = "Actual original-project desktop Play lifecycle and owned Texture native object bytes; not Android/GPU pixel/all-moves/device or whole-process optimization parity."
            });
        }

        private static void MenuProgress(int index, CharacterAnimtorManager manager, LoadingPrewarmController loading, UniTask task)
        {
            object operation = Field<object>(manager, "activeNativePrewarm");
            Save("menu-progress-" + index, new
            {
                utc = DateTime.UtcNow.ToString("O"), frame = Time.frameCount, realtime = Time.realtimeSinceStartup,
                editorPaused = EditorApplication.isPaused, playing = EditorApplication.isPlaying, timeScale = Time.timeScale,
                uniTaskInjected = PlayerLoopHelper.IsInjectedUniTaskPlayerLoop(), PlayerLoopHelper.MainThreadId,
                currentThread = System.Threading.Thread.CurrentThread.ManagedThreadId, PlayerLoopHelper.IsMainThread,
                taskStatus = task.Status.ToString(), manager.IsPrewarmCompleted, manager.NativeStagedResourceCount,
                nativeWork = Field<int>(manager, "nativePrewarmWorkCount"), activeNativeOperation = operation != null,
                configuredRunning = Field<bool>(manager, "configuredPrewarmRunning"),
                plan = manager.PublishedImageLoadPlan?.Phase.ToString(),
                lastBudget = manager.LastNativeImageMemoryBudget?.Describe(),
                loaderIdle = NTSD.Load.NTSD_ResourceLoader.Instance.IsIdle(),
                loadingRunning = Field<bool>(loading, "prewarmRunning"),
                loadingCompleted = Field<bool>(loading, "prewarmCompleted"),
                loadingPendingText = Field<Queue<string>>(loading, "pendingTexts").Count
            });
        }

        [UnityTest]
        [Timeout(1200000)]
        public IEnumerator FixedEightMenuBattleExitReentryAndLoadingCancellation()
        {
            SessionState.SetBool(Session + "full-coverage", false);
            Assert.That(EditorApplication.isPlaying, Is.False);
            Assert.That(SceneManager.GetActiveScene().isDirty, Is.False);
            SessionState.SetInt(Session + "log-stack", (int)Application.GetStackTraceLogType(LogType.Log));
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            SessionState.SetString(Session + "run", Guid.NewGuid().ToString("N"));
            foreach (string path in new[] { MenuScene, BattleScene, ConfigAsset })
                SessionState.SetString(Session + path, Hash(path));
            EditorSceneManager.OpenScene(MenuScene);
            Assert.That(SceneManager.GetActiveScene().isDirty, Is.False);
            yield return new EnterPlayMode();
            foreach (object step in RunBattleCycles()) yield return step;
            yield return new ExitPlayMode();
            VerifyAfterPlay();
        }

        [UnityTest]
        [Timeout(2400000)]
        public IEnumerator FullCatalogRepresentativeMenuBattleBoundaries()
        {
            Assert.That(EditorApplication.isPlaying, Is.False);
            Assert.That(SceneManager.GetActiveScene().isDirty, Is.False);
            Assert.That(File.Exists(CoverageInput), Is.True, "Generate and review full-catalog cases before this test.");
            SessionState.SetBool(Session + "full-coverage", true);
            SessionState.SetString(Session + "case-input-sha", Hash(CoverageInput));
            Cases();
            SessionState.SetInt(Session + "log-stack", (int)Application.GetStackTraceLogType(LogType.Log));
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            SessionState.SetString(Session + "run", Guid.NewGuid().ToString("N"));
            foreach (string path in new[] { MenuScene, BattleScene, ConfigAsset })
                SessionState.SetString(Session + path, Hash(path));
            EditorSceneManager.OpenScene(MenuScene);
            Assert.That(SceneManager.GetActiveScene().isDirty, Is.False);
            yield return new EnterPlayMode();
            foreach (object step in RunBattleCycles()) yield return step;
            yield return new ExitPlayMode();
            VerifyAfterPlay();
        }

        private static IEnumerable RunBattleCycles()
        {
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);

            for (int frame = 0; frame < 12; frame++) yield return null;
            Save("play-startup", new { playing = EditorApplication.isPlaying, activeScene = SceneManager.GetActiveScene().path,
                rootCount = SceneManager.GetActiveScene().rootCount, menuPresent = MenuUIController.Instance != null,
                appPresent = AppManager.Instance != null });
            foreach (object step in Until(() => MenuUIController.Instance != null && AppManager.Instance != null, 30f, "Menu owners missing.")) yield return step;
            var app = AppManager.Instance;
            Assert.That(app, Is.Not.Null);
            MenuUIController.Instance.ShowLoading();
            var manager = CharacterAnimtorManager.Instance;
            Assert.That(manager, Is.Not.Null);
            var loading = UnityEngine.Object.FindObjectOfType<LoadingPrewarmController>(true);
            Assert.That(loading, Is.Not.Null);
            Save("menu-warm-start", new { managerInstance = manager.GetInstanceID(),
                configuredRoot = CharacterAnimtorManager.ConfiguredContentRoot, manager.UseMatchImageResidency,
                appState = app != null ? app.State.ToString() : "destroyed",
                currentAppPresent = AppManager.Instance != null });
            var menuWarm = loading.PrewarmOnceAsync().Preserve();
            float menuStart = Time.realtimeSinceStartup;
            float nextProgress = menuStart;
            int progressIndex = 0;
            while (menuWarm.Status == UniTaskStatus.Pending)
            {
                if (Time.realtimeSinceStartup >= nextProgress)
                {
                    MenuProgress(progressIndex++, manager, loading, menuWarm);
                    nextProgress = Time.realtimeSinceStartup + 10f;
                }
                Assert.That(Time.realtimeSinceStartup - menuStart, Is.LessThan(300f),
                    "Full menu DAT/UI/pool prewarm task did not complete.");
                yield return null;
            }
            menuWarm.GetAwaiter().GetResult();
            Assert.That(CharacterAnimtorManager.TryGetInstance(), Is.SameAs(manager));
            Assert.That(manager.IsPublishedContentUsable, Is.True);
            Assert.That(manager.PublishedImageLoadPlan.Phase, Is.EqualTo(BattleImageLoadPhase.Menu));
            Assert.That(manager.ResidentBodyDefinitionCount, Is.Zero);
            int[] uiIds = UiTextureIds(manager);
            Assert.That(uiIds.Length, Is.EqualTo(203));
            Snapshot("menu-ready", manager);

            SceneCase[] cases = Cases();
            SessionState.SetString(Session + "active-case", "cancel-" + cases[0].Name);
            bool blockedBeforeUpload = false;
            uploadGate = new UniTaskCompletionSource();
            manager.NativeBeforeUploadForDiagnostics = async () =>
            {
                blockedBeforeUpload = true;
                await uploadGate.Task;
            };
            app.SetMatchConfig(Match(cases[0].Roots));
            app.LoadBattleAdditive();
            foreach (object step in Until(() => blockedBeforeUpload || SimulationTickDriver.Instance?.LifecycleState == BattleRuntimeLifecycleState.Stopped,
                180f, "Cancellation did not reach the real body upload boundary.")) yield return step;
            Snapshot("cancel-before-upload", manager);
            Assert.That(blockedBeforeUpload, Is.True, manager.LastNativeImageMemoryBudget?.Describe());
            Assert.That(SimulationTickDriver.Instance.World.ObjectCount, Is.Zero);
            var cancelled = app.UnloadBattleAfterDrainAsync().Preserve();
            yield return null;
            uploadGate.TrySetResult();
            manager.NativeBeforeUploadForDiagnostics = null;
            foreach (object step in Until(() => cancelled.Status != UniTaskStatus.Pending, 45f, "Cancellation did not drain.")) yield return step;
            AsyncOperation cancelledUnload = cancelled.GetAwaiter().GetResult();
            Assert.That(cancelledUnload, Is.Not.Null);
            yield return cancelledUnload;
            foreach (object step in Until(() => app.State == AppFlowState.MenuMain && SimulationTickDriver.Instance == null, 30f, "Cancelled battle did not return to menu.")) yield return step;
            Assert.That(manager.PublishedImageLoadPlan.Phase, Is.EqualTo(BattleImageLoadPhase.Menu));
            Assert.That(manager.ResidentBodyDefinitionCount, Is.Zero);
            Assert.That(manager.HasPendingNativePrewarmWork, Is.False);
            CollectionAssert.AreEqual(uiIds, UiTextureIds(manager));
            Snapshot("cancel-returned-menu", manager);

            int previousDriverId = 0;
            for (int cycle = 0; cycle < cases.Length; cycle++)
            {
                SceneCase item = cases[cycle];
                SessionState.SetString(Session + "active-case", item.Name);
                app.SetMatchConfig(Match(item.Roots));
                app.LoadBattleAdditive();
                foreach (object step in Until(() => app.State == AppFlowState.BattleRunning ||
                    SimulationTickDriver.Instance?.LifecycleState == BattleRuntimeLifecycleState.Stopped,
                    300f, "Battle preparation did not complete.")) yield return step;
                Snapshot("cycle-" + cycle + "-prepared", manager);
                Assert.That(app.State, Is.EqualTo(AppFlowState.BattleRunning), manager.LastImageResidencyDiagnostic);
                var driver = SimulationTickDriver.Instance;
                Assert.That(driver, Is.Not.Null);
                Assert.That(driver.GetInstanceID(), Is.Not.EqualTo(previousDriverId));
                previousDriverId = driver.GetInstanceID();
                foreach (object step in Until(() => driver.CurrentTickIndex >= 3, 10f, "The original Battle Scene did not advance normal ticks.")) yield return step;
                app.SetBattlePaused(true);
                if (item.ExpectedPlanSha256 != null)
                    Assert.That(Hash(item.ExpectedPlanPath), Is.EqualTo(item.ExpectedPlanSha256));
                var expected = JObject.Parse(File.ReadAllText(item.ExpectedPlanPath));
                int expectedDeclarations = expected["bodySourceDeclarations"].Sum(value => value["files"].Count());
                Assert.That(manager.ResidentBodyDefinitionCount, Is.EqualTo(expected["definitionIds"].Count()));
                Assert.That(manager.LastSelectedBodySheetDeclarationCount, Is.EqualTo(expectedDeclarations));
                CollectionAssert.AreEqual(expected["definitionIds"].Values<int>(), manager.PublishedImageLoadPlan.DefinitionIds);
                Assert.That(manager.PublishedImageLoadPlanFingerprint, Is.EqualTo(expected["fingerprint"].Value<string>()));
                CollectionAssert.AreEqual(expected["missingDependencies"].Values<int>(), manager.PublishedImageLoadPlan.MissingDependencies);
                Assert.That(manager.SpriteCatalog.Entries.Values.All(entry => entry.SharedTexture != null &&
                    entry.SharedTexture.width <= SystemInfo.maxTextureSize && entry.SharedTexture.height <= SystemInfo.maxTextureSize), Is.True);
                CollectionAssert.AreEqual(uiIds, UiTextureIds(manager));
                foreach (int oid in item.Roots)
                    Assert.That(driver.World.FindEntityByRuntimeSlotForQuery(Array.IndexOf(item.Roots, oid))?.FrameCache?.Wrapper?.characterId, Is.EqualTo(oid));
                var oldBody = BodyResources(manager);
                Assert.That(oldBody.Length, Is.GreaterThan(0));
                Snapshot("cycle-" + cycle + "-running", manager);
                var unloading = app.UnloadBattleAfterDrainAsync().Preserve();
                foreach (object step in Until(() => unloading.Status != UniTaskStatus.Pending, 45f, "Battle shutdown did not drain.")) yield return step;
                AsyncOperation operation = unloading.GetAwaiter().GetResult();
                Assert.That(operation, Is.Not.Null);
                yield return operation;
                foreach (object step in Until(() => app.State == AppFlowState.MenuMain && SimulationTickDriver.Instance == null, 30f, "Battle exit did not complete.")) yield return step;
                for (int frame = 0; frame < 3; frame++) yield return null;
                Assert.That(manager.ResidentBodyDefinitionCount, Is.Zero);
                Assert.That(manager.PublishedImageLoadPlan.Phase, Is.EqualTo(BattleImageLoadPhase.Menu));
                Assert.That(oldBody.All(value => value == null), Is.True, "Old body resources survived completed unload.");
                CollectionAssert.AreEqual(uiIds, UiTextureIds(manager));
                Assert.That(Field<Dictionary<BattleSpriteCatalog, int>>(manager, "spriteCatalogRendererBindings").Values.Sum(), Is.Zero);
                Assert.That(manager.HasPendingNativePrewarmWork, Is.False);
                Assert.That(Resources.FindObjectsOfTypeAll<LF2ObjectPool>()
                    .Where(pool => pool != null && pool.gameObject.scene.IsValid())
                    .All(pool => pool.ActiveObjectCountForAcceptance == 0 &&
                        pool.ActiveSpriteCountForAcceptance == 0 && pool.IsQuiescedForDiagnostics), Is.True);
                Snapshot("cycle-" + cycle + "-exited", manager);
            }
            var remainingOwned = Field<HashSet<UnityEngine.Object>>(manager, "publishedOwnedResources");
            SessionState.SetString(Session + "owned-ids", JsonConvert.SerializeObject(remainingOwned.Where(value => value != null)
                .Select(value => value.GetInstanceID()).ToArray()));
        }

        private static void RestoreTestLogStack()
        {
            int previous = SessionState.GetInt(Session + "log-stack", -1);
            if (previous < 0) return;
            Application.SetStackTraceLogType(LogType.Log, (StackTraceLogType)previous);
            SessionState.EraseInt(Session + "log-stack");
        }

        private static void VerifyAfterPlay()
        {
            RestoreTestLogStack();

            var nativeIds = new HashSet<int>(JsonConvert.DeserializeObject<int[]>(SessionState.GetString(Session + "owned-ids", "[]")));
            Assert.That(Resources.FindObjectsOfTypeAll<Texture>().Any(value => nativeIds.Contains(value.GetInstanceID())), Is.False);
            foreach (string path in new[] { MenuScene, BattleScene, ConfigAsset })
                Assert.That(Hash(path), Is.EqualTo(SessionState.GetString(Session + path, "")), path + " disk bytes changed.");
            Assert.That(SceneManager.GetActiveScene().isDirty, Is.False);
            Save("complete", new { status = "PASS", cases = Cases(), cancellation = "drained", normalBattleCycles = Cases().Length,
                exitReentry = "PASS", sceneDirty = false, originalSceneAndConfigHashesUnchanged = true,
                ownedTexturesReleasedOnExitPlay = true, noP1 = true, memoryBoundary = "Desktop owned texture observation; ASTC phone budget is independently static." });
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            RestoreTestLogStack();
            uploadGate?.TrySetResult();
            if (EditorApplication.isPlaying)
            {
                var manager = CharacterAnimtorManager.TryGetInstance();
                if (manager != null) manager.NativeBeforeUploadForDiagnostics = null;
                var app = AppManager.Instance;
                if (app != null && SceneManager.GetSceneByName("NTSD_Battle").isLoaded)
                {
                    var unloading = app.UnloadBattleAfterDrainAsync().Preserve();
                    foreach (object step in Until(() => unloading.Status != UniTaskStatus.Pending, 45f, "Failed test cleanup did not drain.")) yield return step;
                    if (unloading.Status == UniTaskStatus.Succeeded)
                    {
                        var operation = unloading.GetAwaiter().GetResult();
                        if (operation != null) yield return operation;
                    }
                }
                yield return new ExitPlayMode();
            }
        }
    }
}
#endif
