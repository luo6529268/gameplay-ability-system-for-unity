#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Load;
using NTSD.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28B11SourceCacheCallerEditorTests
    {
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private NTSD28B11AtomicPublicationEditorTests fixture;
        private readonly List<string> roots = new List<string>();
        private readonly Dictionary<FieldInfo, object> previousOwners = new Dictionary<FieldInfo, object>();
        private Dictionary<string, object> previousCache;
        private CharacterAnimtorManager Manager => (CharacterAnimtorManager)Field("manager");
        private object Field(string name) => fixture.GetType().GetField(name, PrivateInstance).GetValue(fixture);

        private void Bind<T>(T value) where T : Component
        {
            FieldInfo field = typeof(MMSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            if (!previousOwners.ContainsKey(field)) previousOwners.Add(field, field.GetValue(null));
            field.SetValue(null, value);
        }

        private static object Invoke(object target, string name, params object[] args)
        {
            MethodInfo method = target.GetType().GetMethod(name, PrivateInstance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null, "Required content caller API missing: " + name);
            try { return method.Invoke(target, args); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        [SetUp]
        public void SetUp()
        {
            fixture = new NTSD28B11AtomicPublicationEditorTests();
            fixture.SetUpUsingProductionDefault();
            previousCache = new Dictionary<string, object>(ContentCache);
            Bind(Manager);
            Bind((GameDataManager)Field("data"));
            Bind((CharacterUIResourceManager)Field("ui"));
        }

        [TearDown]
        public void TearDown()
        {
            string runId = (string)Field("fixtureRunId");
            try { fixture.TearDown(); }
            finally
            {
                foreach (var owner in previousOwners) owner.Key.SetValue(null, owner.Value);
                previousOwners.Clear();
                foreach (string key in ContentCache.Keys.Where(key =>
                    key.StartsWith("NTSD.LoganContent.", StringComparison.Ordinal) &&
                    roots.Any(root => key.Contains(root))).ToArray())
                    NTSD_ResourceLoader.Instance.RemoveCache(key);
                roots.Clear();
                bool restored = previousCache.All(pair => ContentCache.TryGetValue(pair.Key, out object value) &&
                    ReferenceEquals(value, pair.Value)) && ContentCache.Count == previousCache.Count;
                SaveAdmission("configured-cache-cleanup-" + runId + ".json", new
                {
                    test = TestContext.CurrentContext.Test.FullName,
                    baselineEntries = previousCache.Count, finalEntries = ContentCache.Count, restored
                });
                previousCache.Clear();
                Assert.That(restored, Is.True, "Only this fixture's source cache keys may be removed.");
            }
        }

        private LoganVisualContentCandidate Candidate(string name)
        {
            var fixtureCandidate = (LoganVisualContentCandidate)Invoke(fixture, "Candidate", name, true, true);
            var candidate = LoganVisualContentCandidate.Capture(fixtureCandidate.Catalog.Source,
                ProjectBattleModeConfig.LoadDefault().Capture());
            roots.Add(candidate.Catalog.Source.RuntimeRoot);
            return candidate;
        }

        private static FieldInfo RootField()
        {
            FieldInfo field = typeof(GameConfig).GetField("BattleContentRuntimeRoot");
            Assert.That(field, Is.Not.Null, "GameConfig needs the shared battle content source selector.");
            return field;
        }

        private void Select(LoganVisualContentCandidate candidate) => RootField().SetValue(GameConfig.Instance, candidate.Catalog.Source.RuntimeRoot);
        private UniTask<string> Prewarm() => (UniTask<string>)Invoke(Manager, "PrewarmConfiguredLoganContentAsync", new object[] { null });
        private UniTask<string> Validate() => (UniTask<string>)Invoke(Manager, "ValidateConfiguredContentForBattleAsync");

        private T NewOwnedComponent<T>() where T : Component
        {
            var go = new GameObject("SourceCaller_" + typeof(T).Name) { hideFlags = HideFlags.HideAndDontSave };
            go.SetActive(false);
            ((List<UnityEngine.Object>)Field("owned")).Add(go);
            return go.AddComponent<T>();
        }


        private static Dictionary<string, object> ContentCache => (Dictionary<string, object>)typeof(NTSD_ResourceLoader)
            .GetField("cache", PrivateInstance).GetValue(NTSD_ResourceLoader.Instance);

        private void SetFixtureField(string name, object value) =>
            fixture.GetType().GetField(name, PrivateInstance).SetValue(fixture, value);

        private static void SaveAdmission(string name, object value) =>
            typeof(NTSD28B11AtomicPublicationEditorTests).GetMethod("SaveResumeEvidence",
                BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new[] { name, value });

        private sealed class PublicationSnapshot
        {
            public string[] catalog;
            public Color32[][] pixels;
            public int[] spriteIds, sourceIds;
            public string sourceKey;
            public long cacheHits;
            public int operationObservations;
            public bool shared;
        }

        private PublicationSnapshot CapturePublication(LoganVisualContentCandidate candidate, bool shared)
        {
            Assert.That(Manager.PublishedVisualContentKey, Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(GameDataManager.TryGetInstance().PublishedVisualContentKey, Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(CharacterUIResourceManager.TryGetInstance().PublishedVisualContentKey, Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(Manager.PublishedLoganContentIdentity,
                Is.SameAs(GameDataManager.TryGetInstance().PublishedLoganContentIdentity));
            Assert.That(Manager.PublishedLoganContentIdentity.CatalogFingerprint,
                Is.EqualTo(candidate.ContentIdentity.CatalogFingerprint));
            Assert.That(GameDataManager.TryGetInstance().GetObjectsByType(0).Count, Is.EqualTo(2));
            var geometry = typeof(Editor.H08ResidentVisualMemoryDiagnostics).GetMethod(
                "Geometry", BindingFlags.Static | BindingFlags.NonPublic);
            var readback = typeof(NTSD28B11AtomicPublicationEditorTests).GetMethod(
                "ReadbackTexturePixels", BindingFlags.Static | BindingFlags.NonPublic);
            var entries = Manager.SpriteCatalog.Entries.Values
                .OrderBy(e => e.Key.VisualDataId).ThenBy(e => e.Key.EffectivePic).ToArray();
            Assert.That(entries.Length, Is.EqualTo(6));
            var catalog = new List<string>();
            var pixels = new List<Color32[]>();
            var texturePixels = new Dictionary<Texture2D, Color32[]>();
            Color32[] Read(Texture2D texture)
            {
                if (!texturePixels.TryGetValue(texture, out Color32[] result))
                {
                    result = (Color32[])readback.Invoke(null, new object[] { texture });
                    texturePixels.Add(texture, result);
                }
                return result;
            }
            foreach (var entry in entries)
            {
                Assert.That(entry.CentralBinding.IsValid, Is.True);
                Assert.That(entry.CentralBinding.Mode, Is.EqualTo(BattleSpriteCentralBindingMode.AtlasPageTexture2D));
                catalog.Add(entry.Key + "|" + entry.SourceSheetPath + "|" +
                    geometry.Invoke(null, new object[] { entry.LegacySprite }));
                pixels.Add(Read(entry.SharedTexture));
                var page = (Texture2D)entry.CentralBinding.Texture;
                Color32[] pagePixels = Read(page);
                Rect rect = entry.CentralBinding.AtlasContentPixelRect;
                int width = Mathf.RoundToInt(rect.width), height = Mathf.RoundToInt(rect.height);
                var crop = new Color32[width * height];
                for (int y = 0; y < height; y++)
                    Array.Copy(pagePixels, (Mathf.RoundToInt(rect.y) + y) * page.width +
                        Mathf.RoundToInt(rect.x), crop, y * width, width);
                pixels.Add(crop);
            }
            var ui = CharacterUIResourceManager.TryGetInstance();
            foreach (int id in new[] { 56, 57 })
            {
                Assert.That(Manager.GetCharacterConfig(id), Is.Not.Null);
                Assert.That(GameDataManager.TryGetInstance().GetObjectById(id), Is.Not.Null);
                catalog.Add(Manager.GetCharacterConfig(id).characterData.name);
                foreach (Sprite sprite in new[] { ui.GetHeadSprite(id), ui.GetSmallSprite(id) })
                {
                    Assert.That(sprite, Is.Not.Null);
                    catalog.Add((string)geometry.Invoke(null, new object[] { sprite }));
                    pixels.Add(Read(sprite.texture));
                }
            }
            Assert.That(Manager.SpriteCatalog.TryGet(56, 0, out var first), Is.True);
            Assert.That(Manager.SpriteCatalog.TryGet(57, 0, out var second), Is.True);
            Assert.That(first.LegacySprite, Is.Not.SameAs(second.LegacySprite));
            Assert.That(ReferenceEquals(first.SharedTexture, second.SharedTexture), Is.EqualTo(shared));
            return new PublicationSnapshot
            {
                catalog = catalog.ToArray(), pixels = pixels.ToArray(),
                spriteIds = entries.Select(e => e.LegacySprite.GetInstanceID()).ToArray(),
                sourceIds = entries.Select(e => e.SharedTexture.GetInstanceID()).ToArray(),
                sourceKey = Manager.PublishedVisualContentKey,
                cacheHits = Manager.ConfiguredCandidateCacheHitCount, shared = shared
            };
        }

        private async UniTask<PublicationSnapshot> PrewarmAdmission(LoganVisualContentCandidate candidate, bool shared)
        {
            SetFixtureField("observedGeneration", candidate.SourceCacheKey);
            SetFixtureField("observedSourceRoot", candidate.Catalog.Source.RuntimeRoot);
            int observations = 0;
            try
            {
                string key = await Manager.PrewarmConfiguredLoganContentAsync(_ =>
                {
                    Invoke(fixture, "ObserveOwners");
                    object operation = typeof(CharacterAnimtorManager).GetField("activeNativePrewarm", PrivateInstance)
                        .GetValue(Manager);
                    if (operation == null) return;
                    bool actual = (bool)operation.GetType().GetField("ReuseNativeBodySheets", PrivateInstance)
                        .GetValue(operation);
                    Assert.That(actual, Is.EqualTo(shared));
                    Assert.Throws<InvalidOperationException>(() =>
                        Manager.ReuseNativeBodySheetsForDiagnostics = !shared);
                    observations++;
                });
                Assert.That(key, Is.EqualTo(candidate.SourceCacheKey));
            }
            finally { Invoke(fixture, "ObserveOwners"); }
            Assert.That(observations, Is.GreaterThan(0), "Must observe the actual configured native operation.");
            Assert.That(await Validate(), Is.EqualTo(candidate.SourceCacheKey));
            var snapshot = CapturePublication(candidate, shared);
            snapshot.operationObservations = observations;
            return snapshot;
        }

        private void DestroyAdmissionOwner()
        {
            var destroyed = Manager;
            Invoke(fixture, "ObserveOwners");
            Invoke(fixture, "DetachFixtureUi");
            Assert.That(destroyed.runInEditMode, Is.True);
            UnityEngine.Object.DestroyImmediate(destroyed.gameObject);
            // Assert the real Unity callback before any explicit fallback cleanup.
            Invoke(fixture, "AssertOwnedResourcesReleased");
            Assert.Throws<InvalidOperationException>(() => destroyed.ReuseNativeBodySheetsForDiagnostics = true);
        }

        private void ReplaceAdmissionOwner(bool explicitOff)
        {
            var replacement = NewOwnedComponent<CharacterAnimtorManager>();
            Assert.That(replacement.ReuseNativeBodySheetsForDiagnostics, Is.True);
            if (explicitOff) replacement.ReuseNativeBodySheetsForDiagnostics = false;
            replacement.runInEditMode = true;
            replacement.gameObject.SetActive(true);
            replacement.gameObject.SetActive(false);
            SetFixtureField("manager", replacement);
            Bind(replacement);
        }

        [UnityTest]
        public IEnumerator NativeDefaultAdmission_ConfiguredEntryPreservesFallbackAndRebuildsOwners()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var raw = (LoganVisualContentCandidate)Invoke(fixture, "MultiDefinitionCandidate", "DefaultAdmission", true);
                var candidate = LoganVisualContentCandidate.Capture(raw.Catalog.Source,
                    ProjectBattleModeConfig.LoadDefault().Capture());
                roots.Add(candidate.Catalog.Source.RuntimeRoot);
                Select(candidate);
                Assert.That(Manager.ReuseNativeBodySheetsForDiagnostics, Is.True,
                    "The production-default fixture must not enable the candidate with a setter.");
                var normal = await PrewarmAdmission(candidate, true);
                var publication = Manager.SpriteCatalog;
                long cacheHits = Manager.ConfiguredCandidateCacheHitCount;
                int publications = 0;
                Manager.PrewarmCompleted += () => publications++;
                Assert.That(await Manager.PrewarmConfiguredLoganContentAsync(), Is.EqualTo(candidate.SourceCacheKey));
                Assert.That(Manager.SpriteCatalog, Is.SameAs(publication));
                Assert.That(publications, Is.Zero);
                Assert.That(Manager.ConfiguredCandidateCacheHitCount, Is.GreaterThan(cacheHits));
                CollectionAssert.AreEqual(normal.spriteIds, CapturePublication(candidate, true).spriteIds);

                DestroyAdmissionOwner();
                ReplaceAdmissionOwner(explicitOff: true);
                var fallback = await PrewarmAdmission(candidate, false);
                Assert.That(fallback.cacheHits, Is.GreaterThan(0));
                Assert.That(Manager.SpriteCatalog, Is.Not.SameAs(publication));
                publication = Manager.SpriteCatalog;
                DestroyAdmissionOwner();
                ReplaceAdmissionOwner(explicitOff: false);
                var restoredDefault = await PrewarmAdmission(candidate, true);
                Assert.That(restoredDefault.cacheHits, Is.GreaterThan(0));
                Assert.That(Manager.SpriteCatalog, Is.Not.SameAs(publication));
                foreach (var other in new[] { fallback, restoredDefault })
                {
                    CollectionAssert.AreEqual(normal.catalog, other.catalog);
                    Assert.That(other.pixels.Length, Is.EqualTo(normal.pixels.Length));
                    for (int i = 0; i < normal.pixels.Length; i++)
                        CollectionAssert.AreEqual(normal.pixels[i], other.pixels[i], "Processed source/atlas/UI pixels " + i);
                }
                SaveAdmission("configured-default-" + Field("fixtureRunId") + ".json", new
                {
                    entry = "public PrewarmConfiguredLoganContentAsync", productionDefaultSetterUsed = false,
                    root = candidate.Catalog.Source.RuntimeRoot, candidate.SourceCacheKey,
                    sameOwnerCacheReentryPreservedPublication = true, fullGeometryCatalogAndPixelsEqual = true,
                    naturalDestroyCheckedBeforeFallback = true,
                    phases = new[] { normal, fallback, restoredDefault }.Select(p => new
                    {
                        p.shared, p.sourceKey, p.cacheHits, p.operationObservations, p.spriteIds, p.sourceIds,
                        p.catalog, pixelSets = p.pixels.Length, comparedPixels = p.pixels.Sum(v => v.Length)
                    }).ToArray()
                });
            });
        }

        private LF2ObjectPool CreatePool()
        {
            var pool = NewOwnedComponent<LF2ObjectPool>();
            Bind(pool);
            pool.gameObject.SetActive(true);
            pool.gameObject.SetActive(false);
            if (!Application.isPlaying) Invoke(pool, "Awake");
            return pool;
        }

        private SelectRoleItem CreateIdleView(Sprite head)
        {
            var view = NewOwnedComponent<SelectRoleItem>();
            view.gameObject.hideFlags = HideFlags.HideInHierarchy;
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (view.gameObject.scene != scene) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(view.gameObject, scene);
            view.RoleIcon = NewOwnedComponent<UnityEngine.UI.Image>();
            view.RoleIcon.sprite = head;
            typeof(SelectRoleItem).GetField("selectedCharacterId", PrivateInstance).SetValue(view, 56);
            typeof(SelectRoleItem).GetField("state", PrivateInstance).SetValue(view, SelectRoleState.Idle);
            return view;
        }

        [UnityTest]
        public IEnumerator MenuCaller_PrewarmUsesSelectedSource_AndFailureCannotReuseReady()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("MenuSource");
                Select(candidate);
                GameConfig.Instance.BattleRuntimeProfileName = "DesktopExtended";
                GameConfig.Instance.DesktopInitialRuntimeSlotCapacity = 256;
                LF2ObjectPool pool = CreatePool();
                Bind(NewOwnedComponent<LF2ReferencePool>());
                var factory = NewOwnedComponent<LF2ObjectPointFactory>();
                Bind(factory);
                Invoke(factory, "Awake");
                var controller = NewOwnedComponent<LoadingPrewarmController>();
                string otherKey = "NTSD.AudioFile::E3_" + Guid.NewGuid().ToString("N");
                object otherPayload = new object();
                NTSD_ResourceLoader.Instance.CacheResult(otherKey, otherPayload);
                try
                {
                    await controller.PrewarmOnceAsync();
                    Assert.That(controller.IsPrewarmed, Is.True);
                    Assert.That(await Validate(), Is.EqualTo(candidate.SourceCacheKey));
                    Sprite head = CharacterUIResourceManager.TryGetInstance().GetHeadSprite(56);
                    await controller.PrewarmOnceAsync();
                    Assert.That(CharacterUIResourceManager.TryGetInstance().GetHeadSprite(56), Is.SameAs(head));
                    var view = CreateIdleView(head);
                    CharacterUIResourceManager.TryGetInstance().Clear();
                    Assert.That(controller.IsPrewarmed, Is.False, "Losing a published view must invalidate the caller's ready state.");
                    await controller.PrewarmOnceAsync();
                    Assert.That(controller.IsPrewarmed, Is.True);
                    Assert.That(CharacterUIResourceManager.TryGetInstance().GetHeadSprite(56), Is.Not.Null);
                    Assert.That(view.RoleIcon.sprite, Is.SameAs(CharacterUIResourceManager.TryGetInstance().GetHeadSprite(56)));
                    Assert.That(typeof(SelectRoleItem).GetField("state", PrivateInstance).GetValue(view), Is.EqualTo(SelectRoleState.Idle));
                    int capacity = pool.AvailableObjectCountForAcceptance;
                    RootField().SetValue(GameConfig.Instance, Path.Combine(candidate.Catalog.Source.RuntimeRoot, "missing"));
                    Assert.That(controller.IsPrewarmed, Is.False);
                    bool failed = false;
                    try { await controller.PrewarmOnceAsync(); } catch (Exception) { failed = true; }
                    Assert.That(failed, Is.True);
                    Assert.That(controller.IsPrewarmed, Is.False);
                    Assert.That(pool.AvailableObjectCountForAcceptance, Is.EqualTo(capacity));
                    Assert.That(NTSD_ResourceLoader.Instance.TryGetCache(otherKey, out object actual), Is.True);
                    Assert.That(actual, Is.SameAs(otherPayload));
                }
                finally { NTSD_ResourceLoader.Instance.RemoveCache(otherKey); }
            });
        }

        [UnityTest]
        public IEnumerator DirectCaller_LoadsTheSameConfiguredSource()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("DirectSource");
                Select(candidate);
                var bootstrap = NewOwnedComponent<BattleTestBootstrap>();
                await (UniTask)Invoke(bootstrap, "LoadCharacterDataAsync");
                Assert.That(await Validate(), Is.EqualTo(candidate.SourceCacheKey));
                Assert.That(Manager.GetCharacterConfig(56).characterData.name, Is.EqualTo("DirectSource"));
            });
        }

        [UnityTest]
        public IEnumerator DirectCaller_EmptyRootRejectsLegacyFallback()
        {
            return UniTask.ToCoroutine(async () =>
            {
                RootField().SetValue(GameConfig.Instance, string.Empty);
                var data = (GameDataManager)Field("data");
                Assert.That(data.IsLoaded(), Is.False);
                var bootstrap = NewOwnedComponent<BattleTestBootstrap>();
                InvalidOperationException failure = null;
                try
                {
                    await (UniTask)Invoke(bootstrap, "LoadCharacterDataAsync");
                }
                catch (InvalidOperationException error)
                {
                    failure = error;
                }
                Assert.That(failure, Is.Not.Null);
                StringAssert.Contains("Formal Logan battle content root is required", failure.Message);
                Assert.That(data.IsLoaded(), Is.False);
                Assert.That(Manager.PublishedVisualContentKey, Is.Null);
            });
        }

        [UnityTest]
        public IEnumerator InspectorRefresh_UsesSelectedFormalRoot()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("InspectorRefresh");
                Select(candidate);
                Invoke(Manager, "RefreshAllData");
                DateTime deadline = DateTime.UtcNow.AddSeconds(15);
                while (Manager.PublishedVisualContentKey != candidate.SourceCacheKey && DateTime.UtcNow < deadline)
                    await UniTask.Yield();
                Assert.That(Manager.PublishedVisualContentKey, Is.EqualTo(candidate.SourceCacheKey));
                Assert.That(Manager.GetCharacterConfig(56).characterData.name, Is.EqualTo("InspectorRefresh"));
                Assert.That(GameDataManager.TryGetInstance().GetObjectById(56), Is.Not.Null);
            });
        }

        [UnityTest]
        public IEnumerator SameSourceConcurrentRequests_ShareOnePublicationAndNotifyBothCallers()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("Shared");
                Select(candidate);
                int publications = 0, firstProgress = 0, secondProgress = 0;
                Manager.PrewarmCompleted += () => publications++;
                var first = (UniTask<string>)Invoke(Manager, "PrewarmConfiguredLoganContentAsync", (Action<string>)(_ => firstProgress++));
                var second = (UniTask<string>)Invoke(Manager, "PrewarmConfiguredLoganContentAsync", (Action<string>)(_ => secondProgress++));
                Assert.That(await first, Is.EqualTo(candidate.SourceCacheKey));
                Assert.That(await second, Is.EqualTo(candidate.SourceCacheKey));
                Assert.That(publications, Is.EqualTo(1));
                Assert.That(firstProgress, Is.GreaterThan(0));
                Assert.That(secondProgress, Is.GreaterThan(0));
            });
        }

        [UnityTest]
        public IEnumerator CancelDuringInputCapture_DoesNotPublishOrStoreLateCandidate()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("Cancelled");
                Select(candidate);
                UniTask<string> pending = Prewarm();
                Invoke(Manager, "CancelConfiguredContentPrewarm");
                bool cancelled = false;
                try { await pending; } catch (OperationCanceledException) { cancelled = true; }
                Assert.That(cancelled, Is.True);
                Assert.That(Manager.PublishedVisualContentKey, Is.Null);
                Assert.That(Manager.NativeStagedResourceCount, Is.Zero);
                Assert.That(NTSD_ResourceLoader.Instance.IsCached("NTSD.LoganContent.Root::" + candidate.Catalog.Source.RuntimeRoot), Is.False);
                Assert.That(await Prewarm(), Is.EqualTo(candidate.SourceCacheKey));
            });
        }

        [UnityTest]
        public IEnumerator ReplacedGlobalOwner_CannotPublishItsLateInputResult()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("SupersededOwner");
                Select(candidate);
                UniTask<string> pending = Prewarm();
                Bind(NewOwnedComponent<CharacterAnimtorManager>());
                bool cancelled = false;
                try { await pending; } catch (OperationCanceledException) { cancelled = true; }
                Assert.That(cancelled, Is.True);
                Assert.That(Manager.PublishedVisualContentKey, Is.Null);
                Assert.That(CharacterUIResourceManager.TryGetInstance().PublishedVisualContentKey, Is.Null);
                Assert.That(NTSD_ResourceLoader.Instance.IsCached("NTSD.LoganContent.Root::" + candidate.Catalog.Source.RuntimeRoot), Is.False);
            });
        }

        [UnityTest]
        public IEnumerator PoolWarmup_CannotResumeAnOldGenerationAfterShutdownAndReprepare()
        {
            return UniTask.ToCoroutine(async () =>
            {
                LF2ObjectPool pool = CreatePool();
                int target = pool.AvailableObjectCountForAcceptance + 10;
                var pending = (UniTask<bool>)Invoke(pool, "PrepareCapacityForContentAsync", target, 0, null);
                int createdBeforeCancel = pool.AvailableObjectCountForAcceptance;
                Assert.That(createdBeforeCancel, Is.LessThan(target));
                pool.BeginBattleShutdown();
                pool.BeginBattlePreparation();
                Assert.That(await pending, Is.False);
                Assert.That(pool.AvailableObjectCountForAcceptance, Is.EqualTo(createdBeforeCancel));
                Assert.That(await (UniTask<bool>)Invoke(pool, "PrepareCapacityForContentAsync", target, 0, null), Is.True);
                Assert.That(pool.AvailableObjectCountForAcceptance, Is.EqualTo(target));
            });
        }

        [UnityTest]
        public IEnumerator PoolWarmup_CallerCancellationStopsBeforeNextMaterialization()
        {
            return UniTask.ToCoroutine(async () =>
            {
                LF2ObjectPool pool = CreatePool();
                bool valid = true;
                int target = pool.AvailableObjectCountForAcceptance + 10;
                var pending = (UniTask<bool>)Invoke(pool, "PrepareCapacityForContentAsync", target, 0, (Func<bool>)(() => valid));
                int beforeCancel = pool.AvailableObjectCountForAcceptance;
                valid = false;
                Assert.That(await pending, Is.False);
                Assert.That(pool.AvailableObjectCountForAcceptance, Is.EqualTo(beforeCancel));
                Assert.That(beforeCancel, Is.LessThan(target));
            });
        }

        [Test]
        public void DefaultConfig_LeavesLegacySourceSelected()
        {
            Assert.That(RootField().GetValue(GameConfig.Instance), Is.EqualTo(string.Empty));
            Assert.That(Manager.PublishedVisualContentKey, Is.Null);
        }

        [Test]
        public void GenericCache_HitSkipsExecuteButStillDeliversCompletion()
        {
            string key = "NTSD.SourceCallerTest." + Guid.NewGuid().ToString("N");
            object payload = new object();
            var loader = NTSD_ResourceLoader.Instance;
            int executed = 0, completed = 0;
            loader.CacheResult(key, payload);
            try
            {
                var task = new NTSD_LoadTask
                {
                    CacheKey = key,
                    Execute = (value, service) => { executed++; return UniTask.CompletedTask; },
                    OnCompleted = value => { completed++; Assert.That(value.Result, Is.SameAs(payload)); }
                };
                loader.AddTask(task);
                Assert.That(executed, Is.Zero);
                Assert.That(completed, Is.EqualTo(1));
                Assert.That(task.Status, Is.EqualTo(NTSD_LoadTaskStatus.Completed));
            }
            finally { loader.RemoveCache(key); }
        }

        [Test]
        public void CandidatePublicationCopies_DoNotExposeMutableCacheTemplate()
        {
            var candidate = Candidate("Original");
            var first = (Dictionary<int, LF2CharacterDataWrapper>)Invoke(candidate, "CopyCharacterConfigs");
            first[56].characterData.name = "RuntimeMutation";
            var second = (Dictionary<int, LF2CharacterDataWrapper>)Invoke(candidate, "CopyCharacterConfigs");
            Assert.That(second[56].characterData.name, Is.EqualTo("Original"));
            Assert.That(second[56].characterData, Is.Not.SameAs(first[56].characterData));
        }

        [UnityTest]
        public IEnumerator NativeCacheHit_RebuildsDestroyedManagerPublication()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("Cached");
                Select(candidate);
                string key = await Prewarm();
                Assert.That(key, Is.EqualTo(candidate.SourceCacheKey));
                Assert.That(await Validate(), Is.EqualTo(key));
                Sprite oldHead = CharacterUIResourceManager.TryGetInstance().GetHeadSprite(56);
                SelectRoleItem view = CreateIdleView(oldHead);
                if (!Application.isPlaying) Invoke(Manager, "OnDestroy");
                UnityEngine.Object.DestroyImmediate(Manager.gameObject);
                Assert.That(oldHead == null, Is.True, "Previous manager's owned head must be destroyed.");
                var go = new GameObject("SourceCallerReplacement") { hideFlags = HideFlags.HideAndDontSave };
                go.SetActive(false);
                var replacement = go.AddComponent<CharacterAnimtorManager>();
                ((List<UnityEngine.Object>)Field("owned")).Add(go);
                fixture.GetType().GetField("manager", PrivateInstance).SetValue(fixture, replacement);
                go.SetActive(true);
                go.SetActive(false);
                Bind(replacement);
                Assert.That(await Prewarm(), Is.EqualTo(key));
                Assert.That(await Validate(), Is.EqualTo(key));
                Assert.That(CharacterUIResourceManager.TryGetInstance().GetHeadSprite(56), Is.Not.Null);
                Assert.That(view.RoleIcon.sprite, Is.SameAs(CharacterUIResourceManager.TryGetInstance().GetHeadSprite(56)));
                PropertyInfo hits = replacement.GetType().GetProperty("ConfiguredCandidateCacheHitCount");
                Assert.That(hits, Is.Not.Null);
                Assert.That(Convert.ToInt64(hits.GetValue(replacement)), Is.GreaterThan(0));
            });
        }

        [UnityTest]
        public IEnumerator SameIdInDifferentRoots_PublishesExpectedSource()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var first = Candidate("FirstRoot");
                var second = Candidate("SecondRoot");
                Select(first);
                Assert.That(await Prewarm(), Is.EqualTo(first.SourceCacheKey));
                Select(second);
                Assert.That(await Prewarm(), Is.EqualTo(second.SourceCacheKey));
                Assert.That(await Validate(), Is.EqualTo(second.SourceCacheKey));
                Assert.That(Manager.GetCharacterConfig(56).characterData.name, Is.EqualTo("SecondRoot"));
            });
        }

        [UnityTest]
        public IEnumerator ChangedPng_InvalidatesInputLocatorAndPublishedReady()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("ChangedImage");
                Select(candidate);
                string first = await Prewarm();
                var texture = new Texture2D(6, 2, TextureFormat.RGBA32, false);
                try
                {
                    texture.SetPixels(Enumerable.Repeat(Color.blue, 12).ToArray());
                    texture.Apply();
                    File.WriteAllBytes(Path.Combine(candidate.Catalog.Source.ImageRoot, "c/body.png"), texture.EncodeToPNG());
                }
                finally { UnityEngine.Object.DestroyImmediate(texture); }
                string second = await Prewarm();
                Assert.That(second, Is.Not.EqualTo(first));
                Assert.That(await Validate(), Is.EqualTo(second));
            });
        }

        [UnityTest]
        public IEnumerator InvalidRequestedRoot_PreservesOldPublicationButCannotValidateItAsReady()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("Valid");
                Select(candidate);
                string oldKey = await Prewarm();
                RootField().SetValue(GameConfig.Instance, Path.Combine(candidate.Catalog.Source.RuntimeRoot, "missing"));
                bool loadFailed = false, validationFailed = false;
                try { await Prewarm(); } catch (Exception) { loadFailed = true; }
                try { await Validate(); } catch (Exception) { validationFailed = true; }
                Assert.That(loadFailed, Is.True);
                Assert.That(validationFailed, Is.True);
                Assert.That(Manager.PublishedVisualContentKey, Is.EqualTo(oldKey));
                Assert.That(Manager.GetCharacterConfig(56).characterData.name, Is.EqualTo("Valid"));
            });
        }
    }

    [InitializeOnLoad]
    internal sealed class NTSD28Q07EmptyRootBattleEntryRequestRunner : ICallbacks
    {
        private const string RequestPrefix = "NTSD28-Q07-EmptyRootBattleEntry-";
        private static readonly string RequestDirectory = Path.Combine(
            Directory.GetParent(Application.dataPath).FullName, "Temp");
        private static readonly string[] FocusedTests =
        {
            "NTSD.Test.NTSD28B11SourceCacheCallerEditorTests.DirectCaller_EmptyRootRejectsLegacyFallback",
            "NTSD.Test.NTSD28B11SourceCacheCallerEditorTests.DirectCaller_LoadsTheSameConfiguredSource",
        };
        private static readonly StringBuilder FailureDetails = new StringBuilder(1024);
        private static NTSD28Q07EmptyRootBattleEntryRequestRunner activeCallbacks;
        private static TestRunnerApi activeApi;
        private static string activeResultPath;

        static NTSD28Q07EmptyRootBattleEntryRequestRunner()
        {
            EditorApplication.update += PollRequest;
        }

        private static void PollRequest()
        {
            if (activeCallbacks != null || EditorApplication.isCompiling ||
                EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode ||
                !Directory.Exists(RequestDirectory))
                return;

            foreach (string requestPath in Directory.GetFiles(RequestDirectory, RequestPrefix + "*.request"))
            {
                string resultPath = Path.ChangeExtension(requestPath, ".result");
                string startedPath = Path.ChangeExtension(requestPath, ".started");
                if (File.Exists(startedPath) || File.Exists(resultPath))
                    continue;

                File.WriteAllText(startedPath, DateTime.UtcNow.ToString("O"), new UTF8Encoding(false));
                activeResultPath = resultPath;
                activeCallbacks = new NTSD28Q07EmptyRootBattleEntryRequestRunner();
                try
                {
                    activeApi = ScriptableObject.CreateInstance<TestRunnerApi>();
                    activeApi.RegisterCallbacks(activeCallbacks);
                    activeApi.Execute(new ExecutionSettings(new Filter
                    {
                        testMode = TestMode.EditMode,
                        testNames = FocusedTests,
                    })
                    {
                        runSynchronously = false,
                    });
                }
                catch (Exception error)
                {
                    File.WriteAllText(resultPath, "runner_error=" + error + "\n", new UTF8Encoding(false));
                    if (activeApi != null)
                    {
                        activeApi.UnregisterCallbacks(activeCallbacks);
                        UnityEngine.Object.DestroyImmediate(activeApi);
                    }
                    activeApi = null;
                    activeCallbacks = null;
                    activeResultPath = null;
                }
                return;
            }
        }

        public void RunStarted(ITestAdaptor testsToRun)
        {
            FailureDetails.Clear();
        }

        public void RunFinished(ITestResultAdaptor result)
        {
            string report =
                $"state={result.ResultState}\n" +
                $"passed={result.PassCount}\n" +
                $"failed={result.FailCount}\n" +
                $"skipped={result.SkipCount}\n" +
                $"inconclusive={result.InconclusiveCount}\n" +
                $"message={result.Message}\n" + FailureDetails;
            File.WriteAllText(activeResultPath, report, new UTF8Encoding(false));
            activeApi.UnregisterCallbacks(this);
            UnityEngine.Object.DestroyImmediate(activeApi);
            activeApi = null;
            activeCallbacks = null;
            activeResultPath = null;
        }

        public void TestStarted(ITestAdaptor test)
        {
        }

        public void TestFinished(ITestResultAdaptor result)
        {
            if (result?.Test == null || result.Test.IsSuite || result.FailCount <= 0)
                return;
            FailureDetails.Append("--- failure ---\n");
            FailureDetails.Append("test=").Append(result.FullName).Append('\n');
            FailureDetails.Append("state=").Append(result.ResultState).Append('\n');
            FailureDetails.Append("message=").Append(result.Message).Append('\n');
            FailureDetails.Append("stack=").Append(result.StackTrace).Append('\n');
        }
    }
}
#endif
