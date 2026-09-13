#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Cysharp.Threading.Tasks;
using MoreMountains.Tools;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Load;
using NTSD.UI;
using NUnit.Framework;
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
            fixture.SetUp();
            Bind(Manager);
            Bind((GameDataManager)Field("data"));
            Bind((CharacterUIResourceManager)Field("ui"));
        }

        [TearDown]
        public void TearDown()
        {
            if (!Application.isPlaying && Manager != null) Invoke(Manager, "OnDestroy");
            fixture.TearDown();
            foreach (var owner in previousOwners) owner.Key.SetValue(null, owner.Value);
            previousOwners.Clear();
            var cache = (Dictionary<string, object>)typeof(NTSD_ResourceLoader)
                .GetField("cache", PrivateInstance).GetValue(NTSD_ResourceLoader.Instance);
            foreach (string key in cache.Keys.Where(key => key.StartsWith("NTSD.LoganContent.", StringComparison.Ordinal) &&
                roots.Any(root => key.Contains(root))).ToArray()) NTSD_ResourceLoader.Instance.RemoveCache(key);
            roots.Clear();
        }

        private LoganVisualContentCandidate Candidate(string name)
        {
            var candidate = (LoganVisualContentCandidate)Invoke(fixture, "Candidate", name, true);
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
}
#endif
