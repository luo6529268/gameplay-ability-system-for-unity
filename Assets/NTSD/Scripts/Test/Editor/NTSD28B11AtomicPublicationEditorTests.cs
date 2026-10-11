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


        private const string ResumeOutput = "artifacts/diagnostics/NTSD-H08-NATIVE-SHEET-SHARING-ADMISSION-20261010";
        private readonly List<OwnedResourceObservation> observedResources = new List<OwnedResourceObservation>(256);
        private readonly HashSet<int> preexistingResourceIds = new HashSet<int>();
        private string observedGeneration;
        private string observedSourceRoot;
        private string fixtureRunId;
        private object previousManagerInstance;
        private object previousDataInstance;
        private object previousUiInstance;

        private sealed class OwnedResourceObservation
        {
            public UnityEngine.Object Resource;
            public int InstanceId;
            public string Type, Name, OwnerPhase, Generation, SourceRoot;
        }

        private static FieldInfo SingletonField<T>() where T : Component
        {
            return typeof(MoreMountains.Tools.MMSingleton<T>).GetField(
                "_instance", BindingFlags.Static | BindingFlags.NonPublic);
        }

        private static object PrivateField(object target, string name)
        {
            return target.GetType().GetField(name,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(target);
        }

        private static void SaveResumeEvidence(string name, object value)
        {
            Directory.CreateDirectory(ResumeOutput);
            using (var stream = new FileStream(Path.Combine(ResumeOutput, name), FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(value, Newtonsoft.Json.Formatting.Indented));
        }

        private void ObserveSet(IEnumerable values, string phase)
        {
            foreach (object item in values)
            {
                var resource = item as UnityEngine.Object;
                if (resource == null || observedResources.Any(r => ReferenceEquals(r.Resource, resource)))
                    continue;
                Assert.That(preexistingResourceIds.Contains(resource.GetInstanceID()), Is.False,
                    "New publication must not own a preexisting native object.");
                Assert.That(ReferenceEquals(resource, shadowSprite) ||
                    (shadowSprite != null && ReferenceEquals(resource, shadowSprite.texture)), Is.False,
                    "Borrowed shadow resources cannot enter publication ownership.");
                observedResources.Add(new OwnedResourceObservation
                {
                    Resource = resource, InstanceId = resource.GetInstanceID(),
                    Type = resource.GetType().Name, Name = resource.name, OwnerPhase = phase,
                    Generation = observedGeneration, SourceRoot = observedSourceRoot
                });
            }
        }

        private void ObserveOwners()
        {
            if (ReferenceEquals(manager, null)) return;
            foreach (string field in new[] { "publishedOwnedSprites", "publishedOwnedResources",
                "legacyOwnedUiSprites", "legacyOwnedUiResources" })
                ObserveSet((IEnumerable)PrivateField(manager, field), field);
            foreach (object operation in (IEnumerable)PrivateField(manager, "nativePrewarmOperations"))
                foreach (string field in new[] { "Sprites", "Textures", "Resources" })
                    ObserveSet((IEnumerable)PrivateField(operation, field), "operation." + field);
            foreach (object publication in ((IDictionary)PrivateField(manager, "retiredSpritePublications")).Values)
                foreach (string field in new[] { "Sprites", "Resources" })
                    ObserveSet((IEnumerable)PrivateField(publication, field), "retired." + field);
        }

        private object[] ResourceEvidence()
        {
            return observedResources.Select(r => (object)new
            {
                r.InstanceId, r.Type, r.Name, r.OwnerPhase, r.Generation, r.SourceRoot,
                destroyed = r.Resource == null
            }).ToArray();
        }

        private void DetachFixtureUi()
        {
            foreach (var item in owned)
            {
                var go = item as GameObject;
                if (go == null) continue;
                var image = go.GetComponent<UnityEngine.UI.Image>();
                if (image != null) image.sprite = null;
            }
            if (ui != null) ui.Clear();
        }

        private void ExplicitOwnerCleanup()
        {
            if (ReferenceEquals(manager, null)) return;
            ObserveOwners();
            DetachFixtureUi();
            var method = typeof(CharacterAnimtorManager).GetMethod(
                "OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic);
            Invoke(method, manager);
            Invoke(method, manager);
        }

        private void AssertOwnedResourcesReleased()
        {
            Assert.That(manager.NativeStagedResourceCount, Is.Zero);
            Assert.That(manager.PendingRetiredSpritePublicationCount, Is.Zero);
            Assert.That(((IDictionary)PrivateField(manager, "spriteCatalogRendererBindings")).Count, Is.Zero);
            Assert.That((bool)PrivateField(manager, "spritePrewarmDisposed"), Is.True);
            Assert.That(manager.ReuseNativeBodySheetsForDiagnostics, Is.False);
            foreach (var resource in observedResources)
                Assert.That(resource.Resource == null, Is.True,
                    resource.Type + " " + resource.InstanceId + " owned by " + resource.OwnerPhase);
            Assert.That(shadowSprite != null && shadowSprite.texture != null, Is.True,
                "Publication cleanup must retain borrowed config resources.");
        }

        private static byte[] FixturePng()
        {
            return EncodePng(SharedBodyPixels(), 8, 4);
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOff_LoadedPublicationUsesUnityDestroyCallback()
        {
            return VerifyLoadedPublicationUnityDestroy(false);
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOn_LoadedPublicationUsesUnityDestroyCallback()
        {
            return VerifyLoadedPublicationUnityDestroy(true);
        }

        private IEnumerator VerifyLoadedPublicationUnityDestroy(bool enabled)
        {
            return UniTask.ToCoroutine(async () =>
            {
                manager.ReuseNativeBodySheetsForDiagnostics = enabled;
                GameConfig.Instance.BattleAtlasModeName = "TextureArray";
                var candidate = MultiDefinitionCandidate("NativeUnityDestroy" + enabled, heads: true);
                Assert.That(await Load(candidate), Is.True);
                Assert.That(PublishedEntry(56, 0).CentralBinding.Mode,
                    Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
                Assert.That(ReferenceEquals(PublishedEntry(56, 0).SharedTexture,
                    PublishedEntry(57, 0).SharedTexture), Is.EqualTo(enabled));
                Assert.That(observedResources.Any(r => r.Resource is Texture2DArray), Is.True);
                Assert.That(observedResources.Any(r => r.Resource is Sprite), Is.True);
                DetachFixtureUi();
                Assert.That(ui.LoadedCount, Is.Zero);
                Assert.That((bool)PrivateField(manager, "spritePrewarmDisposed"), Is.False);
                Assert.That(manager.runInEditMode, Is.True);
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
                // No reflection/SendMessage cleanup has run before these observations.
                bool callbackDisposed = (bool)PrivateField(manager, "spritePrewarmDisposed");
                bool flagAfterCallback = manager.ReuseNativeBodySheetsForDiagnostics;
                bool allReleasedByCallback = observedResources.All(r => r.Resource == null);
                SaveResumeEvidence("unity-destroy-" + enabled + "-" + fixtureRunId + ".json", new
                {
                    enabled, callbackDisposed, flagAfterCallback, allReleasedByCallback,
                    actualUnityEvent = true, runInEditMode = true, enteredPlay = false,
                    explicitlyInvokedBeforeObservation = false,
                    borrowedAlive = shadowSprite != null && shadowSprite.texture != null,
                    resources = ResourceEvidence()
                });
                Assert.That(callbackDisposed, Is.True, "Unity must invoke the original production callback.");
                Assert.That(flagAfterCallback, Is.False);
                Assert.That(allReleasedByCallback, Is.True);
                AssertOwnedResourcesReleased();
                ExplicitOwnerCleanup();
                AssertOwnedResourcesReleased();
            });
        }

        private GameObject NewObject(string name)
        {
            var go = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
            go.SetActive(false);
            owned.Add(go);
            return go;
        }

        [SetUp]
        public void SetUp() => SetUpFixture(explicitSheetReuseOff: true);

        internal void SetUpUsingProductionDefault() => SetUpFixture(explicitSheetReuseOff: false);

        private void SetUpFixture(bool explicitSheetReuseOff)
        {
            fixtureRunId = Guid.NewGuid().ToString("N");
            observedResources.Clear();
            preexistingResourceIds.Clear();
            foreach (var value in Resources.FindObjectsOfTypeAll<Texture>())
                preexistingResourceIds.Add(value.GetInstanceID());
            foreach (var value in Resources.FindObjectsOfTypeAll<Sprite>())
                preexistingResourceIds.Add(value.GetInstanceID());
            previousManagerInstance = SingletonField<CharacterAnimtorManager>().GetValue(null);
            previousDataInstance = SingletonField<GameDataManager>().GetValue(null);
            previousUiInstance = SingletonField<CharacterUIResourceManager>().GetValue(null);
            manager = NewObject("NativePublicationManager").AddComponent<CharacterAnimtorManager>();
            if (explicitSheetReuseOff) manager.ReuseNativeBodySheetsForDiagnostics = false;
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
                // Enable Unity callbacks on this fixture instance without changing the production class.
                manager.runInEditMode = true;
                manager.gameObject.SetActive(true);
                manager.gameObject.SetActive(false);
            }
            finally { singleton.SetValue(null, previousManager); }
        }

        [TearDown]
        public void TearDown()
        {

            bool released = false;
            object[] evidence = null;
            try
            {
                ExplicitOwnerCleanup();
                AssertOwnedResourcesReleased();
                released = true;
            }
            finally
            {
                evidence = ResourceEvidence();
                try
                {
                    if (manager != null) UnityEngine.Object.DestroyImmediate(manager.gameObject);
                    for (int i = owned.Count - 1; i >= 0; i--)
                        if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
                }
                finally
                {
                    owned.Clear();
                    typeof(GameConfig).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                        .SetValue(null, previousConfig);
                    SingletonField<CharacterAnimtorManager>().SetValue(null, previousManagerInstance);
                    SingletonField<GameDataManager>().SetValue(null, previousDataInstance);
                    SingletonField<CharacterUIResourceManager>().SetValue(null, previousUiInstance);
                    SaveResumeEvidence("owner-cleanup-" + fixtureRunId + ".json", new
                    {
                        test = TestContext.CurrentContext.Test.FullName,
                        released, resources = evidence, baselineNativeObjectCount = preexistingResourceIds.Count,
                        oldNativeResidue = "UNKNOWN_UNTOUCHED",
                        priorManagerRestored = ReferenceEquals(SingletonField<CharacterAnimtorManager>().GetValue(null), previousManagerInstance),
                        priorDataRestored = ReferenceEquals(SingletonField<GameDataManager>().GetValue(null), previousDataInstance),
                        priorUiRestored = ReferenceEquals(SingletonField<CharacterUIResourceManager>().GetValue(null), previousUiInstance),
                        configRestored = ReferenceEquals(GameConfig.Instance, previousConfig)
                    });
                }
            }
        }

        private LoganVisualContentCandidate Candidate(string name, bool heads = true,
            bool bodySheetPresent = true)
        {
            string root = Path.Combine(ProjectRoot, "Temp/NTSD28AtomicPublication", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/c"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"), "registry_section,registry_index,id,type,source_path,published_folder\nobject,7,56,0,a.dat,missing\n", new UTF8Encoding(false));
            string dat = "<bmp_begin>\nname: " + name + "\n" + (heads ? "head: c/head.png\nsmall: c/small.png\n" : "") + "file(20-19): c/body.png w: 5 h: 1 row: 1 col: 1\n<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n";
            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"), dat, new UTF8Encoding(false));
            byte[] png = FixturePng();
            foreach (string image in new[] { "body", "head", "small" })
            {
                if (image == "body" && !bodySheetPresent)
                    continue;
                File.WriteAllBytes(Path.Combine(root, "vfs/c/" + image + ".png"), png);
            }
            return LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
        }

        private static Color32[] SharedBodyPixels()
        {
            const int width = 8;
            const int height = 4;
            var row = new[]
            {
                new Color32(255, 0, 0, 255),
                new Color32(0, 0, 0, 0),
                new Color32(0, 0, 255, 255),
                new Color32(0, 0, 0, 0),
                new Color32(0, 255, 0, 255),
                new Color32(255, 255, 0, 128),
                new Color32(0, 0, 0, 0),
                new Color32(255, 255, 255, 255),
            };
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
                Array.Copy(row, 0, pixels, y * width, width);
            return pixels;
        }

        private static byte[] EncodePng(Color32[] pixels, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                return texture.EncodeToPNG();
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static string SharedDefinitionDat(string name, bool heads, string bodyDeclaration)
        {
            return "<bmp_begin>\nname: " + name + "\n" +
                (heads ? "head: c/head.png\nsmall: c/small.png\n" : string.Empty) +
                bodyDeclaration + "\n<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n";
        }

        private LoganVisualContentCandidate MultiDefinitionCandidate(string name, bool heads = false)
        {
            string root = Path.Combine(ProjectRoot, "Temp/NTSD28AtomicPublication", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(root, "vfs/c"));
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\n" +
                "object,7,56,0,a.dat,missing\n" +
                "object,8,57,0,b.dat,missing\n", new UTF8Encoding(false));

            File.WriteAllText(Path.Combine(root, "decoded_dat/a.dat"),
                SharedDefinitionDat(name + "_A", heads,
                    "file(20-19): c/body.png w: 2 h: 2 row: 2 col: 1"), new UTF8Encoding(false));
            File.WriteAllText(Path.Combine(root, "decoded_dat/b.dat"),
                SharedDefinitionDat(name + "_B", heads,
                    "file(20-19): c/body.png w: 3 h: 2 row: 2 col: 2"), new UTF8Encoding(false));

            byte[] bodyPng = EncodePng(SharedBodyPixels(), 8, 4);
            File.WriteAllBytes(Path.Combine(root, "vfs/c/body.png"), bodyPng);
            if (heads)
            {
                File.WriteAllBytes(Path.Combine(root, "vfs/c/head.png"), bodyPng);
                File.WriteAllBytes(Path.Combine(root, "vfs/c/small.png"), bodyPng);
            }
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

        private async UniTask<bool> Load(LoganVisualContentCandidate candidate, Action<string> progress = null)
        {
            ObserveOwners();
            observedGeneration = candidate.SourceCacheKey;
            observedSourceRoot = candidate.Catalog.Source.RuntimeRoot;
            var method = typeof(CharacterAnimtorManager).GetMethod("LoadLoganContentForOwnersAsync", BindingFlags.Instance | BindingFlags.NonPublic);
            try
            {
                return await (UniTask<bool>)Invoke(method, manager, candidate, data, ui,
                    new Action<string>(path =>
                    {
                        ObserveOwners();
                        try { progress?.Invoke(path); }
                        finally { ObserveOwners(); }
                    }));
            }
            finally { ObserveOwners(); }
        }

        private static string Key(object target) => (string)target.GetType().GetProperty("PublishedVisualContentKey").GetValue(target);
        private void Cancel()
        {
            Invoke(typeof(CharacterAnimtorManager).GetMethod("CancelNativeContentPrewarm"), manager);
            Invoke(typeof(CharacterAnimtorManager).GetMethod("ReleaseCancelledNativeContentStaging"), manager);
        }

        private void AssertPublished(LoganVisualContentCandidate candidate)
        {
            AssertPublished(candidate, new[] { 56 }, 1);
        }

        private void AssertPublished(LoganVisualContentCandidate candidate,
            IReadOnlyCollection<int> expectedIds, int expectedObjectCount)
        {
            Assert.That(manager.IsPrewarmCompleted, Is.True);
            Assert.That(Key(manager), Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(Key(data), Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(Key(ui), Is.EqualTo(candidate.SourceCacheKey));
            Assert.That(manager.PublishedLoganContentIdentity, Is.SameAs(candidate.ContentIdentity));
            Assert.That(data.PublishedLoganContentIdentity, Is.SameAs(candidate.ContentIdentity));
            var session = manager.PublishedLoganContentIdentity.CreateLocalValidationSessionIdentity(42, 424242, 99, new[] { 0 });
            Assert.That(session.CatalogFingerprint, Is.EqualTo(candidate.ContentIdentity.CatalogFingerprint));
            foreach (int id in expectedIds)
            {
                Assert.That(manager.GetCharacterConfig(id), Is.Not.Null);
                Assert.That(data.GetObjectById(id), Is.Not.Null);
            }
            Assert.That(data.GetObjectsByType(0).Count, Is.EqualTo(expectedObjectCount));
        }

        private void EnableNativeSheetReuse()
        {
            Assert.That(manager.ReuseNativeBodySheetsForDiagnostics, Is.False);
            manager.ReuseNativeBodySheetsForDiagnostics = true;
            Assert.That(manager.ReuseNativeBodySheetsForDiagnostics, Is.True);
        }

        private BattleSpriteEntry PublishedEntry(int visualDataId, int effectivePic)
        {
            Assert.That(manager.SpriteCatalog.TryGet(visualDataId, effectivePic, out BattleSpriteEntry entry), Is.True,
                $"Published sprite {visualDataId}:{effectivePic} is missing.");
            return entry;
        }

        private void AssertSharedDefinitionPublication(LoganVisualContentCandidate candidate)
        {
            AssertPublished(candidate, new[] { 56, 57 }, 2);
            BattleSpriteEntry first = PublishedEntry(56, 0);
            BattleSpriteEntry second = PublishedEntry(57, 0);
            BattleSpriteEntry secondFrame = PublishedEntry(57, 1);
            BattleSpriteEntry clamped = PublishedEntry(57, 2);
            Assert.That(first.SourceSheetPath, Is.EqualTo(second.SourceSheetPath));
            Assert.That(first.SharedTexture, Is.SameAs(second.SharedTexture));
            Assert.That(first.SharedTexture, Is.SameAs(secondFrame.SharedTexture));
            Assert.That(first.PixelRect.width, Is.EqualTo(2f));
            Assert.That(first.PixelRect.height, Is.EqualTo(2f));
            Assert.That(second.PixelRect.width, Is.EqualTo(3f));
            Assert.That(second.PixelRect.height, Is.EqualTo(2f));
            Assert.That(clamped.SourceSheetPath, Is.Not.EqualTo(first.SourceSheetPath));
            Assert.That(Path.GetExtension(clamped.SourceSheetPath), Is.EqualTo(".rgba"));
            Assert.That(clamped.SharedTexture, Is.Not.SameAs(first.SharedTexture));
            Assert.That(clamped.PixelRect, Is.EqualTo(new Rect(0f, 0f, 3f, 2f)));
        }

        private static Color32[] ReadbackTexturePixels(Texture2D source)
        {
            Assert.That(source, Is.Not.Null);
            var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false, false);
            RenderTexture previous = RenderTexture.active;
            RenderTexture readback = RenderTexture.GetTemporary(
                source.width, source.height, 0, RenderTextureFormat.ARGB32);
            try
            {
                Graphics.Blit(source, readback);
                RenderTexture.active = readback;
                copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                copy.Apply(false, false);
                return copy.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(readback);
                UnityEngine.Object.DestroyImmediate(copy);
            }
        }

        private static void AssertPixelsEqual(Color32[] expected, Color32[] actual, string label)
        {
            Assert.That(actual, Is.Not.Null, label);
            Assert.That(actual.Length, Is.EqualTo(expected.Length), label);
            for (int i = 0; i < expected.Length; i++)
                Assert.That(actual[i].Equals(expected[i]), Is.True, $"{label} pixel {i} differs.");
        }

        private static void AssertAtlasRectPixels(
            Color32[] pagePixels, int pageWidth, Rect atlasRect,
            Color32[] sourcePixels, int sourceWidth, Rect sourceRect, string label)
        {
            int width = Mathf.RoundToInt(sourceRect.width);
            int height = Mathf.RoundToInt(sourceRect.height);
            Assert.That(Mathf.RoundToInt(atlasRect.width), Is.EqualTo(width), label);
            Assert.That(Mathf.RoundToInt(atlasRect.height), Is.EqualTo(height), label);
            int atlasX = Mathf.RoundToInt(atlasRect.x);
            int atlasY = Mathf.RoundToInt(atlasRect.y);
            int sourceX = Mathf.RoundToInt(sourceRect.x);
            int sourceY = Mathf.RoundToInt(sourceRect.y);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color32 expected = sourcePixels[(sourceY + y) * sourceWidth + sourceX + x];
                    Color32 actual = pagePixels[(atlasY + y) * pageWidth + atlasX + x];
                    Assert.That(actual.Equals(expected), Is.True,
                        $"{label} pixel ({x},{y}) differs at atlas ({atlasX + x},{atlasY + y}).");
                }
            }
        }

        private static bool ValidateAtlasSourceSet(
            IEnumerable<BattleAtlasSourcePixels> sources, out string diagnostic)
        {
            var method = typeof(BattleAtlasResourceBuilder).GetMethod(
                "TryValidateSourceSet", BindingFlags.Static | BindingFlags.NonPublic);
            object[] args = { sources, null };
            bool result = (bool)Invoke(method, null, args);
            diagnostic = args[1] as string;
            return result;
        }

        private static void AssertCentralBindingMode(
            BattleSpriteEntry entry, BattleSpriteCentralBindingMode expected, string label)
        {
            Assert.That(entry.CentralBinding.Mode, Is.EqualTo(expected), label);
            Assert.That(entry.CentralBinding.IsValid, Is.True, label + " binding must be valid.");
        }


        [UnityTest]
        public IEnumerator NativeCoordinatorOn_MissingBodyPreservesOriginalPolicy()
        {
            EnableNativeSheetReuse();
            return MissingBodySheet_PublishesOtherViewsAndRestoresWhenFileAppears();
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOn_FailedGenerationKeepsOldPublication()
        {
            return UniTask.ToCoroutine(async () =>
            {
                EnableNativeSheetReuse();
                var first = MultiDefinitionCandidate("KeptAfterFailure", true);
                Assert.That(await Load(first), Is.True);
                Texture2D oldBody = PublishedEntry(56, 0).SharedTexture;
                var next = MultiDefinitionCandidate("RejectedChangedInput", true);
                string path = Path.Combine(next.Catalog.Source.ImageRoot, "c/body.png");
                string backupRoot = Path.Combine(ProjectRoot,
                    "docs/ai/FILE-OPERATIONS/NTSD-H08-NATIVE-SHEET-SHARING-20261010/generated-fixture-before");
                Directory.CreateDirectory(backupRoot);
                string backup = Path.Combine(backupRoot, Guid.NewGuid().ToString("N") + ".png");
                File.Copy(path, backup);
                File.WriteAllText(backup + ".path.txt", path);
                File.AppendAllText(path, "\n");
                bool rejected = false;
                try { await Load(next); }
                catch (InvalidDataException) { rejected = true; }
                Assert.That(rejected, Is.True);
                AssertSharedDefinitionPublication(first);
                Assert.That(PublishedEntry(56, 0).SharedTexture, Is.SameAs(oldBody));
                Assert.That(manager.NativeStagedResourceCount, Is.Zero);
                Cancel(); Cancel();
                await UniTask.Yield();
                AssertSharedDefinitionPublication(first);
            });
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOn_ArrayDrawMatchesSourceAndDestroyResets()
        {
            return UniTask.ToCoroutine(async () =>
            {
                EnableNativeSheetReuse();
                GameConfig.Instance.BattleAtlasModeName = "SourceTexture2D";
                var candidate = MultiDefinitionCandidate("NativeArrayControl");
                Assert.That(await Load(candidate), Is.True);
                Color32[] source = NTSD.Test.Editor.H08ResidentVisualMemoryDiagnostics.VerifySharedPublicationDraw(
                    manager.SpriteCatalog, "ArraySourceControl");
                GameConfig.Instance.BattleAtlasModeName = "TextureArray";
                Assert.That(await Load(candidate), Is.True);
                AssertSharedDefinitionPublication(candidate);
                Assert.That(PublishedEntry(56, 0).CentralBinding.Mode,
                    Is.EqualTo(BattleSpriteCentralBindingMode.AtlasTextureArray));
                Color32[] array = NTSD.Test.Editor.H08ResidentVisualMemoryDiagnostics.VerifySharedPublicationDraw(
                    manager.SpriteCatalog, "TextureArray");
                CollectionAssert.AreEqual(source, array, "Actual array publication draw must match actual source publication.");
                Texture2D sheet = PublishedEntry(56, 0).SharedTexture;
                Texture atlas = PublishedEntry(56, 0).CentralBinding.Texture;
                var priorManager = manager;
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
                Assert.That(priorManager.ReuseNativeBodySheetsForDiagnostics, Is.False);
                Assert.That(sheet == null && atlas == null, Is.True);
                typeof(CharacterAnimtorManager).GetMethod("OnDestroy",
                    BindingFlags.Instance | BindingFlags.NonPublic).Invoke(priorManager, null);
                Assert.That(priorManager.ReuseNativeBodySheetsForDiagnostics, Is.False);
            });
        }


        [Test]
        public void NativeSheetReuse_DestroyCallbackQualificationOnly()
        {
            var rows = new List<object>();
            foreach (bool enabled in new[] { false, true })
            {
                var go = NewObject("H08DestroyCallbackQualification");
                var subject = go.AddComponent<CharacterAnimtorManager>();
                var singleton = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                object previous = singleton.GetValue(null);
                var disposed = typeof(CharacterAnimtorManager).GetField(
                    "spritePrewarmDisposed", BindingFlags.Instance | BindingFlags.NonPublic);
                var destroy = typeof(CharacterAnimtorManager).GetMethod(
                    "OnDestroy", BindingFlags.Instance | BindingFlags.NonPublic);
                try
                {
                    go.SetActive(true);
                    go.SetActive(false);
                    singleton.SetValue(null, previous);
                    subject.ReuseNativeBodySheetsForDiagnostics = enabled;
                    Assert.That((bool)disposed.GetValue(subject), Is.False);
                    UnityEngine.Object.DestroyImmediate(go);
                    bool delivered = (bool)disposed.GetValue(subject);
                    bool flagAfterDestroy = subject.ReuseNativeBodySheetsForDiagnostics;
                    destroy.Invoke(subject, null);
                    destroy.Invoke(subject, null);
                    rows.Add(new
                    {
                        enabled,
                        callbackDeliveredInEditMode = delivered,
                        flagAfterDestroy,
                        explicitlyDisposed = (bool)disposed.GetValue(subject),
                        flagAfterExplicitCleanup = subject.ReuseNativeBodySheetsForDiagnostics,
                        stagedResourcesAfterCleanup = subject.NativeStagedResourceCount
                    });
                    Assert.That((bool)disposed.GetValue(subject), Is.True);
                    Assert.That(subject.ReuseNativeBodySheetsForDiagnostics, Is.False);
                    Assert.That(subject.NativeStagedResourceCount, Is.Zero);
                }
                finally
                {
                    destroy.Invoke(subject, null);
                    if (go != null) UnityEngine.Object.DestroyImmediate(go);
                    singleton.SetValue(null, previous);
                }
            }
            string output = Path.Combine(ProjectRoot,
                "artifacts/diagnostics/NTSD-H08-NATIVE-SHEET-SHARING-20261010/destroy-callback-qualification.json");
            using (var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(Newtonsoft.Json.JsonConvert.SerializeObject(new
                {
                    status = "DIAGNOSTIC_ONLY_NOT_LIFECYCLE_ADMISSION",
                    rows,
                    loadedPublication = false,
                    originalFailureRetained = true
                }, Newtonsoft.Json.Formatting.Indented));
        }

        [Test]
        public void NativeCoordinator_FirstDeclarationOwnershipKeepsFirstRange()
        {
            var files = new List<SpriteFileInfo>
            {
                new SpriteFileInfo("first.png", 0, 1, 2, 2, 2, 1),
                new SpriteFileInfo("later.png", 1, 2, 3, 2, 2, 2),
            };

            List<HashSet<int>> ownership =
                CharacterAnimtorManager.BuildFirstDeclaredSpriteOwnership(files);

            Assert.That(ownership, Has.Count.EqualTo(2));
            Assert.That(ownership[0], Is.EquivalentTo(new[] { 0, 1 }));
            Assert.That(ownership[1], Is.EquivalentTo(new[] { 2 }));
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOn_MultiDefinitionSource2DAndAtlasPixelsRemainEquivalent()
        {
            return UniTask.ToCoroutine(async () =>
            {
                EnableNativeSheetReuse();
                Color32[] expected = SharedBodyPixels();
                string previousMode = GameConfig.Instance.BattleAtlasModeName;
                try
                {
                    GameConfig.Instance.BattleAtlasModeName = "SourceTexture2D";
                    var sourceCandidate = MultiDefinitionCandidate("NativeSourcePixels");
                    Assert.That(await Load(sourceCandidate), Is.True);
                    AssertSharedDefinitionPublication(sourceCandidate);

                    NTSD.Test.Editor.H08ResidentVisualMemoryDiagnostics.VerifySharedPublicationDraw(manager.SpriteCatalog, "Source2D");
                    BattleSpriteEntry sourceFirst = PublishedEntry(56, 0);
                    BattleSpriteEntry sourceSecond = PublishedEntry(57, 0);
                    BattleSpriteEntry sourceClamped = PublishedEntry(57, 2);
                    AssertCentralBindingMode(
                        sourceFirst, BattleSpriteCentralBindingMode.SourceTexture2D, "Source2D first definition");
                    AssertCentralBindingMode(
                        sourceSecond, BattleSpriteCentralBindingMode.SourceTexture2D, "Source2D second definition");
                    AssertCentralBindingMode(
                        sourceClamped, BattleSpriteCentralBindingMode.SourceTexture2D, "Source2D clamped definition");
                    Assert.That(sourceFirst.CentralBinding.Texture, Is.SameAs(sourceFirst.SharedTexture));
                    AssertPixelsEqual(
                        expected,
                        ReadbackTexturePixels((Texture2D)sourceFirst.CentralBinding.Texture),
                        "Source2D shared body sheet");

                    Color32[] expectedClamped = CharacterAnimtorManager.BuildNativeClampedCellPixels(
                        expected, 8, 4, 0, -2, 3, 2);
                    AssertPixelsEqual(
                        expectedClamped,
                        ReadbackTexturePixels((Texture2D)sourceClamped.CentralBinding.Texture),
                        "Source2D native clamped cell");
                    Assert.That(expected[1].a, Is.EqualTo(0), "transparent overlap oracle must remain transparent");
                    Assert.That(expected[2].b, Is.EqualTo(255), "opaque overlap oracle must remain blue");
                    Assert.That(expected[5].a, Is.EqualTo(128), "partial-alpha oracle must remain partial alpha");

                    string bodyPath = sourceFirst.SourceSheetPath;
                    var sameSource = new BattleAtlasSourcePixels(
                        bodyPath, 8, 4, (Color32[])expected.Clone());
                    Assert.That(
                        ValidateAtlasSourceSet(new[] { sameSource, sameSource }, out string sameDiagnostic),
                        Is.True,
                        sameDiagnostic);

                    Color32[] conflictPixels = (Color32[])expected.Clone();
                    conflictPixels[0] = new Color32(254, 0, 0, 255);
                    Assert.That(
                        ValidateAtlasSourceSet(
                            new[]
                            {
                                sameSource,
                                new BattleAtlasSourcePixels(bodyPath, 8, 4, conflictPixels),
                            },
                            out string conflictDiagnostic),
                        Is.False);
                    Assert.That(conflictDiagnostic, Does.Contain("Conflicting decoded atlas source"));

                    GameConfig.Instance.BattleAtlasModeName = "OrderedPages";
                    var atlasCandidate = MultiDefinitionCandidate("NativeAtlasPixels");
                    Assert.That(await Load(atlasCandidate), Is.True);
                    AssertSharedDefinitionPublication(atlasCandidate);

                    NTSD.Test.Editor.H08ResidentVisualMemoryDiagnostics.VerifySharedPublicationDraw(manager.SpriteCatalog, "OrderedPages");
                    BattleSpriteEntry atlasFirst = PublishedEntry(56, 0);
                    BattleSpriteEntry atlasSecond = PublishedEntry(57, 0);
                    BattleSpriteEntry atlasSecondFrame = PublishedEntry(57, 1);
                    BattleSpriteEntry atlasClamped = PublishedEntry(57, 2);
                    AssertCentralBindingMode(
                        atlasFirst, BattleSpriteCentralBindingMode.AtlasPageTexture2D, "atlas first definition");
                    AssertCentralBindingMode(
                        atlasSecond, BattleSpriteCentralBindingMode.AtlasPageTexture2D, "atlas second definition");
                    AssertCentralBindingMode(
                        atlasSecondFrame, BattleSpriteCentralBindingMode.AtlasPageTexture2D,
                        "atlas second definition second frame");
                    AssertCentralBindingMode(
                        atlasClamped, BattleSpriteCentralBindingMode.AtlasPageTexture2D, "atlas clamped definition");
                    Assert.That(atlasFirst.CentralBinding.Texture, Is.SameAs(atlasSecond.CentralBinding.Texture));
                    Assert.That(atlasFirst.CentralBinding.Texture, Is.SameAs(atlasClamped.CentralBinding.Texture));

                    Texture2D page = atlasFirst.CentralBinding.Texture as Texture2D;
                    Assert.That(page, Is.Not.Null);
                    Color32[] pagePixels = ReadbackTexturePixels(page);
                    AssertAtlasRectPixels(
                        pagePixels, page.width, atlasFirst.CentralBinding.AtlasContentPixelRect,
                        expected, 8, atlasFirst.PixelRect, "atlas first definition");
                    AssertAtlasRectPixels(
                        pagePixels, page.width, atlasSecond.CentralBinding.AtlasContentPixelRect,
                        expected, 8, atlasSecond.PixelRect, "atlas second definition");
                    AssertAtlasRectPixels(
                        pagePixels, page.width, atlasSecondFrame.CentralBinding.AtlasContentPixelRect,
                        expected, 8, atlasSecondFrame.PixelRect, "atlas second definition second frame");
                    AssertAtlasRectPixels(
                        pagePixels, page.width, atlasClamped.CentralBinding.AtlasContentPixelRect,
                        expectedClamped, 3, atlasClamped.PixelRect, "atlas native clamped cell");
                }
                finally
                {
                    GameConfig.Instance.BattleAtlasModeName = previousMode;
                }
            });
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOn_CancelGenerationReleasesSharedStagingAndRetryPublishes()
        {
            return UniTask.ToCoroutine(async () =>
            {
                EnableNativeSheetReuse();
                var first = MultiDefinitionCandidate("NativeCancelKept", heads: true);
                Assert.That(await Load(first), Is.True);
                AssertSharedDefinitionPublication(first);
                Texture2D oldBody = PublishedEntry(56, 0).SharedTexture;
                Sprite oldHead = ui.GetHeadSprite(56);

                var next = MultiDefinitionCandidate("NativeCancelNext", heads: true);
                bool cancelled = false;
                bool setterRejected = false;
                bool loaded = await Load(next, path =>
                {
                    if (cancelled || Path.GetFileName(path) != "head.png")
                        return;
                    Assert.That(manager.NativeStagedResourceCount, Is.GreaterThan(0));
                    try { manager.ReuseNativeBodySheetsForDiagnostics = false; }
                    catch (InvalidOperationException) { setterRejected = true; }
                    cancelled = true;
                    Cancel();
                });

                Assert.That(cancelled, Is.True);
                Assert.That(setterRejected, Is.True, "cold reuse switch must reject during active generation");
                Assert.That(loaded, Is.False);
                AssertPublished(first, new[] { 56, 57 }, 2);
                Assert.That(PublishedEntry(56, 0).SharedTexture, Is.SameAs(oldBody));
                Assert.That(ui.GetHeadSprite(56), Is.SameAs(oldHead));
                Assert.That(manager.NativeStagedResourceCount, Is.Zero);

                Assert.That(await Load(next), Is.True);
                AssertSharedDefinitionPublication(next);
            });
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOn_LeaseRebindsUiAndPreservesBorrowedAssetsUntilSharedRetirement()
        {
            return UniTask.ToCoroutine(async () =>
            {
                EnableNativeSheetReuse();
                var first = MultiDefinitionCandidate("NativeLeaseFirst", heads: true);
                Assert.That(await Load(first), Is.True);
                AssertSharedDefinitionPublication(first);
                Texture2D oldBody = PublishedEntry(56, 0).SharedTexture;
                Sprite oldHead = ui.GetHeadSprite(56);
                Assert.That(oldHead, Is.Not.Null);

                var view = NewObject("NativeSharedLeaseRoleView").AddComponent<SelectRoleItem>();
                view.gameObject.hideFlags = HideFlags.HideInHierarchy;
                var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (view.gameObject.scene != scene)
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(view.gameObject, scene);
                view.RoleIcon = NewObject("NativeSharedLeaseRoleImage").AddComponent<UnityEngine.UI.Image>();
                view.RoleIcon.sprite = oldHead;
                typeof(SelectRoleItem).GetField("selectedCharacterId", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(view, 56);
                typeof(SelectRoleItem).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(view, SelectRoleState.SelectingCharacter);
                ui.SetCharacterUISprites(57, shadowSprite, null);

                var oldCatalog = manager.SpriteCatalog;
                var lease = manager.AcquireCentralCatalogLease(oldCatalog);
                try
                {
                    var second = MultiDefinitionCandidate("NativeLeaseSecond", heads: false);
                    Assert.That(await Load(second), Is.True);
                    AssertSharedDefinitionPublication(second);
                    Assert.That(ui.GetHeadSprite(56), Is.Null);
                    Assert.That(view.RoleIcon.sprite, Is.Null);
                    Assert.That(oldBody == null, Is.False, "catalog lease must keep shared body alive");
                    Assert.That(oldHead == null, Is.False, "catalog lease must keep published head alive");
                    Assert.That(shadowSprite == null, Is.False, "borrowed UI sprite must remain alive");
                }
                finally { lease.Dispose(); }

                Assert.That(lease.IsReleased, Is.True);
                Assert.That(oldBody == null, Is.True);
                Assert.That(oldHead == null, Is.True);
                Assert.That(shadowSprite == null, Is.False);
            });
        }

        [UnityTest]
        public IEnumerator NativeCoordinatorOn_DriverShutdownRecyclesGroupedUnpublishedImages()
        {
            return UniTask.ToCoroutine(async () =>
            {
                EnableNativeSheetReuse();
                var singleton = typeof(MoreMountains.Tools.MMSingleton<CharacterAnimtorManager>)
                    .GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
                object previous = singleton.GetValue(null);
                var driver = NewObject("NativeSharedShutdownDriver").AddComponent<SimulationTickDriver>();
                singleton.SetValue(null, manager);
                try
                {
                    bool stopped = false;
                    bool loaded = await Load(MultiDefinitionCandidate("NativeStoppedShared", heads: true), path =>
                    {
                        if (stopped || Path.GetFileName(path) != "head.png")
                            return;
                        stopped = true;
                        Assert.That(manager.NativeStagedResourceCount, Is.GreaterThan(0));
                        var report = driver.ShutdownBattleRuntime();
                        Assert.That(report.CompletedStage, Is.GreaterThanOrEqualTo(
                            BattleRuntimeShutdownStage.PresentationCleared));
                        Assert.That(manager.NativeStagedResourceCount, Is.Zero);
                        Assert.That(driver.CompleteBattleRuntimeShutdownAfterMapCleanup(true).Status,
                            Is.EqualTo(BattleRuntimeShutdownStatus.Completed));
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

        [UnityTest]
        public IEnumerator MissingBodySheet_PublishesOtherViewsAndRestoresWhenFileAppears()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var missing = Candidate("MissingBody", bodySheetPresent: false);
                string bodyPath = Path.Combine(missing.Catalog.Source.ImageRoot, "c/body.png");
                Assert.That(File.Exists(bodyPath), Is.False);
                Assert.That(await Load(missing), Is.True);
                AssertPublished(missing);
                Assert.That(manager.SpriteCatalog.TryGet(56, 0, out _), Is.False);
                Assert.That(ui.GetHeadSprite(56), Is.Not.Null);

                byte[] png = FixturePng();
                File.WriteAllBytes(bodyPath, png);
                Assert.Throws<InvalidDataException>(() => missing.AssertInputsCurrent());
                var restored = LoganVisualContentCandidate.Capture(
                    BattleContentSource.ForLoganRuntime(missing.Catalog.Source.RuntimeRoot));
                Assert.That(await Load(restored), Is.True);
                AssertPublished(restored);
                Assert.That(manager.SpriteCatalog.TryGet(56, 0, out _), Is.True);
                SaveResumeEvidence("missing-body-" + fixtureRunId + ".json", new
                {
                    enabled = manager.ReuseNativeBodySheetsForDiagnostics,
                    sourceRoot = missing.Catalog.Source.RuntimeRoot,
                    reachedLoader = true, missingPublishedOtherViews = true,
                    absentThenRestored = true, oldCandidateRejected = true,
                    restoredKey = restored.SourceCacheKey
                });
            });
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
