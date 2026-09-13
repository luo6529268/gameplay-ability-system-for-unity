#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.App;
using NTSD.Simulation;
using NTSD.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28B11AtomicPublicationEditorTests
    {
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private CharacterAnimtorManager manager;
        private GameDataManager data;
        private CharacterUIResourceManager ui;
        private GameConfig previousConfig;
        private Sprite shadowSprite;
        private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

        private GameObject NewObject(string name)
        {
            var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
            go.SetActive(false);
            owned.Add(go);
            return go;
        }

        [SetUp]
        public void SetUp()
        {
            manager = NewObject("NativePublicationManager").AddComponent<CharacterAnimtorManager>();
            data = NewObject("NativePublicationData").AddComponent<GameDataManager>();
            ui = NewObject("NativePublicationUI").AddComponent<CharacterUIResourceManager>();
            previousConfig = GameConfig.Instance;
            var config = ScriptableObject.CreateInstance<GameConfig>();
            owned.Add(config);
            config.BattleAtlasModeName = "OrderedPages";
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            texture.SetPixels32(new Color32[16]);
            texture.Apply();
            owned.Add(texture);
            shadowSprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
            owned.Add(shadowSprite);
            var material = new Material(Shader.Find("NTSD/BattleCentralTransparent"));
            owned.Add(material);
            var shadow = NewObject("NativePublicationShadow");
            shadow.AddComponent<BattleCommonShadowDescriptor>().ConfigureForSelfCheck(shadowSprite, material, Color.white);
            config.ShadowPrefab = shadow;
            typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, config);
            var singleton = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            object previousManager = singleton.GetValue(null);
            try
            {
                // OnDestroy is only delivered to GameObjects that were active.
                manager.gameObject.SetActive(true);
                manager.gameObject.SetActive(false);
            }
            finally { singleton.SetValue(null, previousManager); }
        }

        [TearDown]
        public void TearDown()
        {
            if (manager != null) UnityEngine.Object.DestroyImmediate(manager.gameObject);
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
            typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, previousConfig);
        }

        private LoganVisualContentCandidate Candidate(string name, bool heads = true)
        {
            string root = Path.Combine(ProjectRoot, "Temp/NTSD28AtomicPublication", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/c"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"), "registry_section,registry_index,id,type,source_path,published_folder\nobject,7,56,0,a.dat,missing\n", new UTF8Encoding(false));
            string dat = "<bmp_begin>\nname: " + name + "\n" + (heads ? "head: c/head.png\nsmall: c/small.png\n" : "") + "file(20-19): c/body.png w: 5 h: 1 row: 1 col: 1\n<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n";
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"), dat, new UTF8Encoding(false));
            byte[] png = File.ReadAllBytes(Path.Combine(ProjectRoot, "Temp/NTSD28PngAlpha/fixture.dat"));
            foreach (string image in new[] { "body", "head", "small" }) File.WriteAllBytes(Path.Combine(root, "vfs/c/" + image + ".png"), png);
            return LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
        }

        private static object Invoke(MethodInfo method, object target, params object[] args)
        {
            Assert.That(method, Is.Not.Null, "Atomic native publication API is missing.");
            try { return method.Invoke(target, args); }
            catch (TargetInvocationException error) when (error.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(error.InnerException).Throw();
                throw;
            }
        }

        private UniTask<bool> Load(LoganVisualContentCandidate candidate, Action<string> progress = null)
        {
            var method = typeof(CharacterAnimtorManager).GetMethod("LoadLoganContentForOwnersAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            return (UniTask<bool>)Invoke(method, manager, candidate, data, ui, progress);
        }

        private static string Key(object target) => (string)target.GetType().GetProperty("PublishedVisualContentKey").GetValue(target);
        private void Cancel()
        {
            Invoke(typeof(CharacterAnimtorManager).GetMethod("CancelNativeContentPrewarm"), manager);
            Invoke(typeof(CharacterAnimtorManager).GetMethod("ReleaseCancelledNativeContentStaging"), manager);
        }

        private void AssertPublished(LoganVisualContentCandidate candidate)
        {
            Assert.That(manager.IsPrewarmCompleted, Is.True);
            Assert.That(Key(manager), Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(Key(data), Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(Key(ui), Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(manager.PublishedLoganContentIdentity, Is.SameAs(candidate.ContentIdentity));
            Assert.That(data.PublishedLoganContentIdentity, Is.SameAs(candidate.ContentIdentity));
            var session = manager.PublishedLoganContentIdentity.CreateLocalValidationSessionIdentity(42, 424242, 99, new[] { 0 });
            Assert.That(session.CatalogFingerprint, Is.EqualTo(candidate.ContentIdentity.CatalogFingerprint));
            Assert.That(manager.GetCharacterConfig(56), Is.Not.Null);
            Assert.That(data.GetObjectById(56), Is.Not.Null);
            Assert.That(data.GetObjectsByType(0).Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator FullPrewarm_PublishesAllViewsBeforeEvent_AndRebindsBeforeRetirement()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var first = Candidate("First");
                int events = 0;
                manager.PrewarmCompleted += () => { events++; Assert.That(Key(manager), Is.EqualTo(Key(data))); Assert.That(Key(data), Is.EqualTo(Key(ui))); };
                Assert.That(await Load(first), Is.True);
                AssertPublished(first);
                Assert.That(events, Is.EqualTo(1));
                Assert.That(ui.GetHeadSprite(56), Is.Not.Null);
                Assert.That((int)Invoke(typeof(GameDataManager).GetMethod("GetObjectRegistryIndex"), data, 56), Is.EqualTo(7));

                var view = NewObject("NativePublicationRoleView").AddComponent<SelectRoleItem>();
                view.gameObject.hideFlags = HideFlags.HideInHierarchy;
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                Assert.That(UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(scene), Is.False);
                if (view.gameObject.scene != scene)
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(view.gameObject, scene);
                UnityEngine.Debug.Log("Role fixture scene: " + scene.name + ", loaded=" + scene.isLoaded);
                var iconObject = NewObject("NativePublicationRoleImage");
                view.RoleIcon = iconObject.AddComponent<UnityEngine.UI.Image>();
                typeof(SelectRoleItem).GetField("selectedCharacterId", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, 56);
                typeof(SelectRoleItem).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, SelectRoleState.SelectingCharacter);
                Sprite oldHead = ui.GetHeadSprite(56);
                view.RoleIcon.sprite = oldHead;
                var oldCatalog = manager.SpriteCatalog;
                var lease = manager.AcquireCentralCatalogLease(oldCatalog);
                try
                {
                    var second = Candidate("Second", false);
                    Assert.That(await Load(second), Is.True);
                    AssertPublished(second);
                    Assert.That(ui.GetHeadSprite(56), Is.Null);
                    Assert.That(view.RoleIcon.sprite, Is.Null, "Old Image reference must be cleared before old head retirement.");
                    Assert.That(oldHead != null, Is.True, "Old publication lease still owns its resources.");
                    Assert.That((int)typeof(SelectRoleItem).GetField("selectedCharacterId", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view), Is.EqualTo(56));
                    Assert.That((SelectRoleState)typeof(SelectRoleItem).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view), Is.EqualTo(SelectRoleState.SelectingCharacter));
                }
                finally { lease.Dispose(); }
                Assert.That(oldHead == null, Is.True);
                Assert.That(shadowSprite != null, Is.True, "Borrowed GameConfig resources cannot be retired.");
                Assert.That(events, Is.EqualTo(2));
            });
        }

        [UnityTest]
        public IEnumerator CancelAfterBodyStaging_PreservesPublishedSource_AndAllowsRetry()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var first = Candidate("Kept");
                Assert.That(await Load(first), Is.True);
                Sprite head = ui.GetHeadSprite(56);
                var next = Candidate("Cancelled");
                bool cancelled = false;
                bool loaded = await Load(next, path =>
                {
                    if (!cancelled && Path.GetFileName(path) == "head.png") { cancelled = true; Cancel(); }
                });
                Assert.That(cancelled, Is.True);
                Assert.That(loaded, Is.False);
                AssertPublished(first);
                Assert.That(ui.GetHeadSprite(56), Is.SameAs(head));
                Assert.That((int)manager.GetType().GetProperty("NativeStagedResourceCount").GetValue(manager), Is.Zero);
                Assert.That(await Load(next), Is.True);
                AssertPublished(next);
            });
        }

        [UnityTest]
        public IEnumerator LegacyOwnedUiImages_TransferAfterNativeRebind_WhileBorrowedAssetsSurvive()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Candidate("NativeWithoutHead", false);
                var load = typeof(CharacterAnimtorManager).GetMethod("LoadBMPAsSpriteAsync", BindingFlags.Instance | BindingFlags.NonPublic);
                Sprite previous = await (UniTask<Sprite>)Invoke(load, manager, Path.Combine(candidate.Catalog.Source.ImageRoot, "c/head.png"), "legacy_head");
                Assert.That(previous, Is.Not.Null);
                ui.SetCharacterUISprites(56, previous, null);
                ui.SetCharacterUISprites(57, shadowSprite, null);
                var view = NewObject("LegacyHeadRebindView").AddComponent<SelectRoleItem>();
                view.gameObject.hideFlags = HideFlags.HideInHierarchy;
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (view.gameObject.scene != scene) UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(view.gameObject, scene);
                view.RoleIcon = NewObject("LegacyHeadRebindImage").AddComponent<UnityEngine.UI.Image>();
                view.RoleIcon.sprite = previous;
                typeof(SelectRoleItem).GetField("selectedCharacterId", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, 56);
                typeof(SelectRoleItem).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(view, SelectRoleState.Idle);
                Assert.That(await Load(candidate), Is.True);
                Assert.That(view.RoleIcon.sprite, Is.Null);
                Assert.That(previous == null, Is.True, "Dynamically loaded legacy UI image must transfer to retirement ownership.");
                Assert.That(shadowSprite != null, Is.True, "An externally supplied UI sprite remains borrowed.");
            });
        }

        [UnityTest]
        public IEnumerator ChangedCandidate_RejectsWithoutTouchingPublishedViews()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var first = Candidate("Kept");
                Assert.That(await Load(first), Is.True);
                var next = Candidate("Stale");
                File.AppendAllText(next.Catalog.Entries[0].DatPath, "\n# source changed\n");
                Exception failure = null;
                try { await Load(next); } catch (Exception error) { failure = error; }
                Assert.That(failure, Is.TypeOf<InvalidDataException>());
                AssertPublished(first);
                Assert.That((int)manager.GetType().GetProperty("NativeStagedResourceCount").GetValue(manager), Is.Zero);
            });
        }

        [Test]
        public void NullCandidate_DoesNotCreatePublicationOwners()
        {
            var previousData = GameDataManager.TryGetInstance();
            var previousUI = CharacterUIResourceManager.TryGetInstance();
            Assert.Throws<ArgumentNullException>(() => manager.LoadLoganContentAsync(null));
            Assert.That(GameDataManager.TryGetInstance(), Is.SameAs(previousData));
            Assert.That(CharacterUIResourceManager.TryGetInstance(), Is.SameAs(previousUI));
        }

        [UnityTest]
        public IEnumerator DriverShutdown_CancelsAndRecyclesUnpublishedImagesInOrderedStages()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var singleton = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                object previous = singleton.GetValue(null);
                var driver = NewObject("NativePublicationShutdownDriver").AddComponent<SimulationTickDriver>();
                singleton.SetValue(null, manager);
                try
                {
                    bool stopped = false;
                    bool loaded = await Load(Candidate("Stopped"), path =>
                    {
                        if (stopped || Path.GetFileName(path) != "head.png") return;
                        stopped = true;
                        Assert.That(manager.NativeStagedResourceCount, Is.GreaterThan(0));
                        var report = driver.ShutdownBattleRuntime();
                        Assert.That(report.CompletedStage, Is.GreaterThanOrEqualTo(BattleRuntimeShutdownStage.PresentationCleared));
                        Assert.That(manager.NativeStagedResourceCount, Is.Zero);
                        Assert.That(driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true).Status, Is.EqualTo(BattleRuntimeShutdownStatus.Completed));
                    });
                    Assert.That(stopped, Is.True);
                    Assert.That(loaded, Is.False);
                    Assert.That(manager.IsPrewarmCompleted, Is.False);
                    Assert.That(ui.LoadedCount, Is.Zero);
                    Assert.That(driver.LifecycleState, Is.EqualTo(BattleRuntimeLifecycleState.Stopped));
                    Assert.That(manager.NativeStagedResourceCount, Is.Zero);
                }
                finally { singleton.SetValue(null, previous); }
            });
        }

        [Test]
        public void ExistingPrewarmTransactionSelfCheck_RemainsValid()
        {
            var method = typeof(NTSD.Test.BattleRuntimeSelfCheck).GetMethod("CheckBattleSpritePrewarmTransactionContracts", BindingFlags.Static | BindingFlags.NonPublic);
            var singleton = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
            object previous = singleton.GetValue(null);
            singleton.SetValue(null, manager);
            var commonField = typeof(CharacterAnimtorManager).GetField("<CommonVisualCatalog>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            object previousCommon = commonField.GetValue(manager);
            commonField.SetValue(manager, BattleCommonVisualCatalog.Build(GameConfig.Instance.ShadowPrefab));
            try { Invoke(method, null); }
            finally
            {
                commonField.SetValue(manager, previousCommon);
                singleton.SetValue(null, previous);
            }
        }

        [TestCase(BattleRuntimeLifecycleState.Running, 0, false, false, false)]
        [TestCase(BattleRuntimeLifecycleState.Stopping, 0, false, false, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 1, false, false, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, true, false, false)]
        [TestCase(BattleRuntimeLifecycleState.Uninitialized, 0, false, true, false)]
        [TestCase(BattleRuntimeLifecycleState.Preparing, 0, false, false, true)]
        [TestCase(BattleRuntimeLifecycleState.Stopped, 0, false, false, true)]
        public void NativePublicationBoundary_UsesLifecycleAndDataBinding(BattleRuntimeLifecycleState lifecycle, int objects, bool sealedData, bool appBlocked, bool expected)
        {
            var method = typeof(CharacterAnimtorManager).GetMethod("NativeContentBoundaryAllows", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That((bool)Invoke(method, null, lifecycle, objects, sealedData, appBlocked), Is.EqualTo(expected));
        }
    }
}
#endif
