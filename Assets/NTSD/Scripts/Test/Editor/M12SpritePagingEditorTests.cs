#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    // Alignment contract: NTSD-M12-DAT-SPRITE-PAGING-PILOT-001.
    public sealed class M12SpritePagingEditorTests
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private string root;
        private BattleContentSource source;
        private JObject manifest;
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();

        private static string Hash(byte[] bytes)
        {
            using (var hash = SHA256.Create())
                return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private string NewFile(string relative, byte[] bytes)
        {
            string file = Path.Combine(root, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(file));
            using (var stream = new FileStream(file, FileMode.CreateNew))
                stream.Write(bytes, 0, bytes.Length);
            return file;
        }

        private void ReplaceOwned(string file, byte[] bytes)
        {
            Assert.That(Path.GetFullPath(file).StartsWith(root + Path.DirectorySeparatorChar), Is.True);
            string suffix = Guid.NewGuid().ToString("N");
            string backup = file + "." + suffix + ".before";
            File.Copy(file, backup, false);
            NewFile("operation-" + suffix + ".json", Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new
            {
                file, backup, before = Hash(File.ReadAllBytes(file)), after = Hash(bytes),
                reason = "Same-test-owned negative fixture; retain preimage"
            })));
            File.WriteAllBytes(file, bytes);
        }

        private static byte[] Png(int width, int height, Color32[] pixels)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels32(pixels);
                texture.Apply();
                return texture.EncodeToPNG();
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        [SetUp]
        public void SetUp()
        {
            root = Path.GetFullPath(Path.Combine("Temp", "M12Pg", Guid.NewGuid().ToString("N")));
            source = BattleContentSource.ForLoganRuntime(root);
            var pixels = new Color32[24];
            for (int y = 0; y < 2; y++)
                for (int frame = 0; frame < 2; frame++)
                    for (int x = 0; x < 2; x++)
                        pixels[y * 12 + frame * 3 + x] = new Color32(
                            (byte)(20 + frame * 70 + x * 30), (byte)(60 + y * 60), 130, (byte)(100 + x * 100));
            byte[] original = Png(12, 2, pixels);
            var pagePixels = new Color32[32];
            for (int frame = 0; frame < 2; frame++)
                for (int y = 0; y < 4; y++)
                    for (int x = 0; x < 4; x++)
                        pagePixels[y * 8 + frame * 4 + x] =
                            pixels[Mathf.Clamp(y - 1, 0, 1) * 12 + frame * 3 + Mathf.Clamp(x - 1, 0, 1)];
            byte[] page = Png(8, 4, pagePixels);
            NewFile("derived_sprite_pages/pages/p.png", page);
            var sources = new JArray();
            for (int i = 0; i < 2; i++)
            {
                string image = "c/" + (i == 0 ? "a" : "b") + ".png";
                string dat = "c/" + (i == 0 ? "a" : "b") + ".dat";
                NewFile("vfs/" + image, original);
                byte[] datBytes = Encoding.UTF8.GetBytes(
                    "<bmp_begin>\nname: Paging\nfile(99-100): " + image +
                    " w: 2 h: 2 row: 2 col: 1\n<bmp_end>\n" +
                    "<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n");
                NewFile("decoded_dat/" + dat, datBytes);
                sources.Add(JObject.FromObject(new
                {
                    path = image, sha256 = Hash(original), width = 12, height = 2,
                    frameWidth = 2, frameHeight = 2, columns = 2, rows = 1,
                    bindings = new[] { new { oid = 100 + i, datPath = dat, datSha256 = Hash(datBytes), firstPic = 0, lastPic = 1 } },
                    chunks = new[] { new { page = "p", firstLocalPic = 0, count = 2, columns = 2, rows = 1,
                        rects = new[] { new { x = 1, y = 1, width = 2, height = 2 }, new { x = 5, y = 1, width = 2, height = 2 } } } }
                }));
            }
            NewFile("catalog.csv", Encoding.UTF8.GetBytes(
                "registry_section,registry_index,id,type,source_path,published_folder\n" +
                "object,0,100,0,c/a.dat,missing\nobject,1,101,0,c/b.dat,missing\n"));
            manifest = new JObject
            {
                ["version"] = 1, ["generatorVersion"] = "NTSD_DAT_PAGES_V1",
                ["pages"] = JArray.FromObject(new[] { new { id = "p", path = "pages/p.png", sha256 = Hash(page), width = 8, height = 4 } }),
                ["sources"] = sources
            };
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
            // Fixtures intentionally remain as operation evidence; no file deletion.
        }

        private void PublishManifest() =>
            NewFile(BattleDerivedSpritePages.RelativeManifestPath, Encoding.UTF8.GetBytes(manifest.ToString()));

        private LoganVisualContentCandidate Capture() => LoganVisualContentCandidate.Capture(source);

        private static object Invoke(string method, params object[] args) =>
            typeof(CharacterAnimtorManager).GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);

        private static object Operation(LoganVisualContentCandidate candidate, bool reuse,
            CharacterAnimtorManager.NativeDecodeAdmission metrics, Action<string> decode = null)
        {
            Type type = typeof(CharacterAnimtorManager).GetNestedType("NativePrewarmOperation", BindingFlags.NonPublic);
            object operation = Activator.CreateInstance(type, true);
            type.GetField("Candidate", Fields).SetValue(operation, candidate);
            type.GetField("ReuseNativeBodySheets", Fields).SetValue(operation, reuse);
            type.GetField("Metrics", Fields).SetValue(operation, metrics);
            type.GetField("DecodeStarted", Fields).SetValue(operation, decode);
            return operation;
        }

        private static List<(int characterId, SpriteFileInfo fileInfo, HashSet<int> ownedEffectivePics)> Declarations(
            Dictionary<int, LF2CharacterDataWrapper> configs) =>
            configs.Values.Select(config => (config.characterId, config.characterData.files.Single(),
                new HashSet<int> { 0, 1 })).ToList();

        [TestCase("offset")]
        [TestCase("size")]
        [TestCase("length")]
        public void InvalidTrimGeometryIsRejected(string kind)
        {
            var chunk = manifest["sources"][0]["chunks"][0];
            var content = new JArray(
                JObject.FromObject(new { x = 0, y = 0, width = 2, height = 2 }),
                JObject.FromObject(new { x = 0, y = 0, width = 2, height = 2 }));
            chunk["contentRects"] = content;
            if (kind == "offset") content[0]["x"] = 1;
            if (kind == "size") content[0]["width"] = 1;
            if (kind == "length") content.RemoveAt(1);
            PublishManifest();
            Assert.Throws<InvalidDataException>(() => Capture());
        }

        [TestCase("length")]
        [TestCase("offset")]
        [TestCase("source-overlap")]
        [TestCase("size")]
        public void InvalidReplacementPatchGeometryIsRejected(string kind)
        {
            var chunk = manifest["sources"][0]["chunks"][0];
            chunk["allowExactFrameAliases"] = true;
            chunk["rects"][1] = chunk["rects"][0].DeepClone();
            var patch = JObject.FromObject(new { source = new { x = 5, y = 1, width = 1, height = 1 },
                destination = new { x = 0, y = 0, width = 1, height = 1 } });
            var patches = new JArray(JValue.CreateNull(), patch);
            chunk["replacementPatches"] = patches;
            if (kind == "length") patches.RemoveAt(0);
            if (kind == "offset") patch["destination"]["x"] = 2;
            if (kind == "source-overlap") patch["source"]["x"] = 1;
            if (kind == "size") patch["source"]["width"] = 2;
            PublishManifest();
            Assert.Throws<InvalidDataException>(() => Capture());
        }

        [Test]
        public void OptionalManifestChangesOnlyVisualIdentityAndUsesNativeCumulativePics()
        {
            var before = Capture();
            Assert.That(before.DerivedPages, Is.Null);
            PublishManifest();
            var after = Capture();
            Assert.That(after.ContentIdentity.SemanticFingerprint, Is.EqualTo(before.ContentIdentity.SemanticFingerprint));
            Assert.That(after.VisualFingerprint, Is.Not.EqualTo(before.VisualFingerprint));
            Assert.That(after.Images.Count, Is.EqualTo(3));
            Assert.That(after.DerivedPages.Pages.Count, Is.EqualTo(1));
            var configs = after.CopyCharacterConfigs();
            Assert.That(configs[100].characterData.files.Single().startFrame, Is.Zero);
            Assert.That(configs[100].characterData.files.Single().endFrame, Is.EqualTo(1));
            after.AssertInputsCurrent();
            Assert.Throws<InvalidDataException>(() => before.AssertInputsCurrent());
        }


        private void EnableFixtureFrameAliases()
        {
            var pixels = new Color32[24];
            for (int y = 0; y < 2; y++)
                for (int frame = 0; frame < 2; frame++)
                    for (int x = 0; x < 2; x++)
                        pixels[y * 12 + frame * 3 + x] = new Color32(
                            (byte)(20 + x * 30), (byte)(60 + y * 60), 130, (byte)(100 + x * 100));
            byte[] identicalFrames = Png(12, 2, pixels);
            foreach (var mapped in manifest["sources"])
            {
                ReplaceOwned(Path.Combine(root, "vfs", (string)mapped["path"]), identicalFrames);
                mapped["sha256"] = Hash(identicalFrames);
                mapped["chunks"][0]["allowExactFrameAliases"] = true;
                mapped["chunks"][0]["rects"][1]["x"] = 1;
            }
        }

        [Test]
        public void ExplicitExactFrameAliasesShareOneDecodeGroup()
        {
            EnableFixtureFrameAliases();
            PublishManifest();
            var candidate = Capture();
            Assert.That(candidate.DerivedPages.TryGetSource(Path.Combine(root, "vfs/c/a.png"), out var mapped), Is.True);
            Assert.That(mapped.Chunks.Single().Rects[0], Is.EqualTo(mapped.Chunks.Single().Rects[1]));
            var configs = candidate.CopyCharacterConfigs();
            var groups = (IList)Invoke("BuildNativeBodySheetGroupsWithReuse", Declarations(configs), candidate, true);
            Assert.That(groups.Count, Is.EqualTo(1));
            candidate.AssertInputsCurrent();
        }

        [Test]
        public void ExactAliasOptInStillRejectsPartiallyOverlappingRectangles()
        {
            EnableFixtureFrameAliases();
            manifest["sources"][0]["chunks"][0]["rects"][1]["x"] = 2;
            PublishManifest();
            Assert.Throws<InvalidDataException>(() => Capture());
        }

        [TestCase("traversal")]
        [TestCase("gap")]
        [TestCase("overlap")]
        [TestCase("authored-range")]
        [TestCase("source-hash")]
        public void InvalidManifestFailsBeforeDecode(string error)
        {
            switch (error)
            {
                case "traversal": manifest["pages"][0]["path"] = "../outside.png"; break;
                case "gap": manifest["sources"][0]["chunks"][0]["firstLocalPic"] = 1; break;
                case "overlap": manifest["sources"][0]["chunks"][0]["rects"][1]["x"] = 1; break;
                case "authored-range": manifest["sources"][0]["bindings"][0]["firstPic"] = 99; break;
                case "source-hash": manifest["sources"][0]["sha256"] = new string('0', 64); break;
            }
            PublishManifest();
            Assert.Throws<InvalidDataException>(() => Capture());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ChangedPageOrManifestRejectsCapturedCandidate(bool page)
        {
            PublishManifest();
            var candidate = Capture();
            string file = Path.Combine(root, page ? "derived_sprite_pages/pages/p.png" : BattleDerivedSpritePages.RelativeManifestPath);
            byte[] bytes = File.ReadAllBytes(file);
            if (page) bytes[bytes.Length - 1] ^= 1;
            else bytes = Encoding.UTF8.GetBytes(manifest.ToString(Formatting.None));
            ReplaceOwned(file, bytes);
            Assert.Throws<InvalidDataException>(() => candidate.AssertInputsCurrent());
        }

        [TestCase(true, 128)]
        [TestCase(false, 256)]
        public void BudgetCountsActualPagesWithoutRetainedPixelsOrAtlas(bool reuse, long expected)
        {
            PublishManifest();
            var candidate = Capture();
            var configs = candidate.CopyCharacterConfigs();
            var operation = Operation(candidate, reuse, null);
            var budget = (BattleImageMemoryBudget)Invoke("BuildNativeImageMemoryBudget",
                operation, Declarations(configs), configs, 0L, -1L, 8);
            Assert.That(budget.SourceTextureBytes, Is.EqualTo(expected));
            Assert.That(budget.RetainedPixelBytes + budget.ReadableTextureBytes + budget.ReservedAtlasBytes, Is.Zero);
            var error = Assert.Throws<TargetInvocationException>(() => Invoke("BuildNativeImageMemoryBudget",
                operation, Declarations(configs), configs, 0L, -1L, 4));
            Assert.That(error.InnerException, Is.TypeOf<InvalidDataException>());
        }

        [UnityTest]
        public IEnumerator RealPageDecodeSharesTexturePreservesKeysAndMatchRebind()
        {
            return UniTask.ToCoroutine(async () =>
            {
                PublishManifest();
                var candidate = Capture();
                var configs = candidate.CopyCharacterConfigs();
                var declarations = Declarations(configs);
                var groups = (IList)Invoke("BuildNativeBodySheetGroupsWithReuse", declarations, candidate, true);
                Assert.That(groups.Count, Is.EqualTo(1));
                Assert.That(((IList)Invoke("BuildNativeBodySheetGroups", declarations, candidate)).Count, Is.EqualTo(1));
                Assert.That(((IList)Invoke("BuildNativeBodySheetGroupsWithReuse", declarations, candidate, false)).Count, Is.EqualTo(2));
                var sprites = new HashSet<Sprite>();
                var textures = new HashSet<Texture2D>();
                var atlas = new List<BattleAtlasSourcePixels>();
                var overrides = new Dictionary<Sprite, string>();
                var byId = new Dictionary<int, List<Sprite>>
                {
                    [100] = new List<Sprite> { null, null }, [101] = new List<Sprite> { null, null }
                };
                var metrics = new CharacterAnimtorManager.NativeDecodeAdmission(1000000, 1000000);
                int decodes = 0;
                var operation = Operation(candidate, true, metrics, file =>
                {
                    Assert.That(file.Replace('\\', '/').EndsWith("/derived_sprite_pages/pages/p.png"), Is.True);
                    Interlocked.Increment(ref decodes);
                });
                using (var cpu = new SemaphoreSlim(0, 1))
                using (var upload = new SemaphoreSlim(1, 1))
                {
                    try
                    {
                        var result = await (UniTask<(int created, int failed)>)Invoke("ProcessNativeBodySheetGroupAsync",
                            groups[0], byId, sprites, textures, atlas, new Dictionary<string, Sprite>(), overrides,
                            null, cpu, upload, source, new Func<bool>(() => true), operation, null);
                        Assert.That(result.created, Is.EqualTo(4));
                        Assert.That(result.failed, Is.Zero);
                        Assert.That(decodes, Is.EqualTo(1));
                        Assert.That(textures.Count, Is.EqualTo(1));
                        Assert.That(textures.Single().isReadable, Is.False);
                        Assert.That(atlas.Count, Is.Zero);
                        Assert.That(overrides.Count, Is.EqualTo(4));
                        var catalog = CharacterAnimtorManager.BuildBattleSpriteCatalog(configs, byId, overrides);
                        Assert.That(catalog.Count, Is.EqualTo(4));
                        foreach (var pair in catalog.Entries)
                        {
                            Assert.That(pair.Key.EffectivePic, Is.InRange(0, 1));
                            Assert.That(pair.Value.PixelRect, Is.EqualTo(new Rect(1 + 4 * pair.Key.EffectivePic, 1, 2, 2)));
                            Assert.That(pair.Value.LegacySprite.pivot, Is.EqualTo(new Vector2(1, 0)));
                        }
                        VerifyRebind(catalog, candidate);
                        Assert.That(metrics.PendingPixels + metrics.RetainedPixels + metrics.Active, Is.Zero);
                        Assert.That(cpu.CurrentCount, Is.EqualTo(1));
                        Assert.That(upload.CurrentCount, Is.EqualTo(1));
                    }
                    finally { CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures); }
                }
            });
        }

        private T Track<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }

        private void VerifyRebind(BattleSpriteCatalog catalog, LoganVisualContentCandidate candidate)
        {
            var material = Track(new Material(Shader.Find(BattleSpriteMaterialContract.CentralTextureShaderName)));
            var shadowTexture = Track(new Texture2D(8, 8, TextureFormat.RGBA32, false));
            var shadow = Track(Sprite.Create(shadowTexture, new Rect(0, 0, 8, 8), Vector2.one * 0.5f));
            var prefab = Track(new GameObject("M12PagingShadow"));
            var descriptor = prefab.AddComponent<BattleCommonShadowDescriptor>();
            typeof(BattleCommonShadowDescriptor).GetMethod("ConfigureForSelfCheck", Fields).Invoke(descriptor,
                new object[] { shadow, material, Color.white, false, false, SpriteMaskInteraction.None });
            var sparkTexture = Track(new Texture2D(510, 256, TextureFormat.RGBA32, false));
            var sparks = new Sprite[BattleCommonVisualCatalog.SparkFrameCount];
            for (int i = 0; i < sparks.Length; i++)
                sparks[i] = Track(Sprite.Create(sparkTexture, BattleCommonVisualCatalog.GetSparkPixelRect(i),
                    BattleCommonVisualCatalog.GetSparkPivotNormalized(i)));
            var common = BattleCommonVisualCatalog.Build(prefab).WithSpark(sparkTexture, sparks);
            var capabilities = new BattleRenderingDeviceCapabilities(
                "test", "test", "test", true, 4096, 256, true, false, 256L * 1024 * 1024);
            bool success = CharacterAnimtorManager.TryBuildMatchCentralAtlasPublication(
                catalog, common, new[] { 100, 101 }, capabilities, null, Array.Empty<string>(),
                out var rebound, out var boundCommon, out var resources, out string diagnostic, out _, out _,
                (texture, file) => candidate.DerivedPages.IsPagePath(file) ? null : texture.GetPixels32());
            if (resources != null) foreach (var resource in resources) owned.Add(resource);
            Assert.That(success, Is.True, diagnostic);
            Assert.That(boundCommon.IsRuntimeReady, Is.True);
            foreach (var pair in catalog.Entries)
            {
                var entry = rebound.Entries[pair.Key];
                Assert.That(entry.CentralBinding.Mode, Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
                Assert.That(entry.SharedTexture, Is.SameAs(pair.Value.SharedTexture));
                Assert.That(entry.PixelRect, Is.EqualTo(pair.Value.PixelRect));
                Assert.That(entry.Pivot, Is.EqualTo(pair.Value.Pivot));
            }
        }

        [UnityTest]
        public IEnumerator CancelAfterDecodeReleasesSlotsWithoutMaterializingTexture()
        {
            return UniTask.ToCoroutine(async () =>
            {
                PublishManifest();
                var candidate = Capture();
                var groups = (IList)Invoke("BuildNativeBodySheetGroupsWithReuse", Declarations(candidate.CopyCharacterConfigs()), candidate, true);
                var sprites = new HashSet<Sprite>();
                var textures = new HashSet<Texture2D>();
                var metrics = new CharacterAnimtorManager.NativeDecodeAdmission(1000000, 1000000);
                int live = 1;
                var operation = Operation(candidate, true, metrics, _ => Interlocked.Exchange(ref live, 0));
                using (var cpu = new SemaphoreSlim(0, 1))
                using (var upload = new SemaphoreSlim(1, 1))
                {
                    var result = await (UniTask<(int created, int failed)>)Invoke("ProcessNativeBodySheetGroupAsync",
                        groups[0], new Dictionary<int, List<Sprite>>(), sprites, textures,
                        new List<BattleAtlasSourcePixels>(), new Dictionary<string, Sprite>(),
                        new Dictionary<Sprite, string>(), null, cpu, upload, source,
                        new Func<bool>(() => Volatile.Read(ref live) == 1), operation, null);
                    Assert.That(result.created + result.failed + sprites.Count + textures.Count, Is.Zero);
                    Assert.That(metrics.PendingPixels + metrics.RetainedPixels + metrics.Active, Is.Zero);
                    Assert.That(cpu.CurrentCount, Is.EqualTo(1));
                    Assert.That(upload.CurrentCount, Is.EqualTo(1));
                }
            });
        }

        [UnityTest]
        public IEnumerator DeployedPagesDecodeAndPublishAllEightyEightKeys()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var actual = LoganVisualContentCandidate.Capture(
                    BattleContentSource.ForLoganRuntime(Path.GetFullPath("Assets/NTSD/Content/LoganRuntime")),
                    NTSD.App.ProjectBattleModeConfig.LoadDefault().Capture());
                var configs = actual.CopyCharacterConfigs();
                var declarations = new List<(int characterId, SpriteFileInfo fileInfo, HashSet<int> ownedEffectivePics)>();
                foreach (var config in configs.Values)
                    foreach (var file in config.characterData.files)
                        if (actual.DerivedPages.TryGetSource(file.filePath, out var mapped) &&
                            mapped.Chunks.All(chunk => chunk.ReplacementPatches == null))
                            declarations.Add((config.characterId, file,
                                new HashSet<int>(Enumerable.Range(file.startFrame, file.endFrame - file.startFrame + 1))));
                Assert.That(declarations.Count, Is.EqualTo(4));
                var selectedConfigs = declarations.ToDictionary(item => item.characterId, item => configs[item.characterId]);
                var byId = declarations.ToDictionary(item => item.characterId,
                    item => Enumerable.Repeat<Sprite>(null, item.fileInfo.endFrame + 1).ToList());
                var groups = (IList)Invoke("BuildNativeBodySheetGroupsWithReuse", declarations, actual, true);
                Assert.That(groups.Count, Is.EqualTo(18));
                var sprites = new HashSet<Sprite>();
                var textures = new HashSet<Texture2D>();
                var atlas = new List<BattleAtlasSourcePixels>();
                var overrides = new Dictionary<Sprite, string>();
                var metrics = new CharacterAnimtorManager.NativeDecodeAdmission(134217728, 268435456);
                int decodes = 0;
                var operation = Operation(actual, true, metrics, _ => Interlocked.Increment(ref decodes));
                using (var cpu = new SemaphoreSlim(1, 1))
                using (var upload = new SemaphoreSlim(1, 1))
                {
                    try
                    {
                        int created = 0;
                        foreach (object group in groups)
                        {
                            await cpu.WaitAsync();
                            var result = await (UniTask<(int created, int failed)>)Invoke("ProcessNativeBodySheetGroupAsync",
                                group, byId, sprites, textures, atlas, new Dictionary<string, Sprite>(), overrides,
                                null, cpu, upload, actual.Catalog.Source, new Func<bool>(() => true), operation, null);
                            Assert.That(result.failed, Is.Zero);
                            created += result.created;
                        }
                        var catalog = CharacterAnimtorManager.BuildBattleSpriteCatalog(selectedConfigs, byId, overrides);
                        Assert.That(created, Is.EqualTo(88));
                        Assert.That(catalog.Count, Is.EqualTo(88));
                        Assert.That(decodes, Is.EqualTo(18));
                        // Alignment contract: NTSD-M12-EXACT-FRAME-DEDUP-001. Same pixels, keys and draw count.
                        foreach (int oid in new[] { 74, 317 })
                        {
                            Sprite canonical = byId[oid][171];
                            Assert.That(canonical.rect.width, Is.EqualTo(567));
                            Assert.That(canonical.rect.height, Is.EqualTo(567));
                            for (int pic = 172; pic <= 174; pic++)
                            {
                                Assert.That(byId[oid][pic].texture, Is.SameAs(canonical.texture));
                                Assert.That(byId[oid][pic].rect, Is.EqualTo(canonical.rect));
                                Assert.That(byId[oid][pic].pivot, Is.EqualTo(canonical.pivot));
                            }
                        }
                        var selectedPages = declarations.SelectMany(declaration =>
                        {
                            actual.DerivedPages.TryGetSource(declaration.fileInfo.filePath, out var mappedSource);
                            return mappedSource.Chunks.Select(chunk => chunk.Page);
                        }).Distinct();
                        long expectedBytes = selectedPages.Sum(page => (long)page.Width * page.Height * 4);
                        Assert.That(expectedBytes, Is.EqualTo(112077376L));
                        Assert.That(textures.Sum(texture => (long)texture.width * texture.height * 4), Is.EqualTo(expectedBytes));

                        Assert.That(textures.Count, Is.EqualTo(18));
                        Assert.That(textures.All(texture => !texture.isReadable && texture.width <= 2048 && texture.height <= 2048), Is.True);
                        Assert.That(atlas.Count, Is.Zero);
                        foreach (var declaration in declarations)
                            foreach (int pic in declaration.ownedEffectivePics)
                            {
                                Assert.That(catalog.TryGet(declaration.characterId, pic, out var entry), Is.True);
                                Assert.That(entry.PixelWidth, Is.EqualTo(declaration.fileInfo.width));
                                Assert.That(entry.PixelHeight, Is.EqualTo(declaration.fileInfo.height));
                                Assert.That(entry.Pivot, Is.EqualTo(new Vector2(0.5f, 0)));
                                Assert.That(actual.DerivedPages.IsPagePath(entry.SourceSheetPath), Is.True);
                                actual.DerivedPages.TryGetSource(declaration.fileInfo.filePath, out var mappedSource);
                                int local = pic - declaration.fileInfo.startFrame;
                                var mappedChunk = mappedSource.Chunks.Single(chunk =>
                                    local >= chunk.FirstLocalPic && local < chunk.FirstLocalPic + chunk.Count);
                                int inChunk = local - mappedChunk.FirstLocalPic;
                                Rect expectedContent = mappedChunk.ContentRects[inChunk];
                                Assert.That(entry.ContentRect, Is.EqualTo(expectedContent));
                                Assert.That(entry.PixelRect, Is.EqualTo(mappedChunk.Rects[inChunk]));
                                Assert.That(entry.LegacySprite.pivot.x,
                                    Is.EqualTo(entry.PixelWidth * .5f - expectedContent.x).Within(.001f));
                                Assert.That(entry.LegacySprite.pivot.y, Is.EqualTo(-expectedContent.y).Within(.001f));
                            }
                        Assert.That(metrics.PendingPixels + metrics.RetainedPixels + metrics.Active, Is.Zero);
                        Assert.That(cpu.CurrentCount + upload.CurrentCount, Is.EqualTo(2));
                        actual.AssertInputsCurrent();
                    }
                    finally { CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures); }
                }
            });
        }

        [Test]
        public void DeployedPagesRetainNativeKirinRange()
        {
            var actual = LoganVisualContentCandidate.Capture(
                BattleContentSource.ForLoganRuntime(Path.GetFullPath("Assets/NTSD/Content/LoganRuntime")),
                NTSD.App.ProjectBattleModeConfig.LoadDefault().Capture());
            Assert.That(actual.DerivedPages.Pages.Count(page => actual.DerivedPages.TryGetSource(
                Path.GetFullPath("Assets/NTSD/Content/LoganRuntime/vfs/s/0/d0.png"), out var d0)
                ? d0.Chunks.All(chunk => chunk.Page != page) : true), Is.EqualTo(18));
            Assert.That(actual.DerivedPages.Pages.All(page => page.Width <= 2048 && page.Height <= 2048), Is.True);
            var configs = actual.CopyCharacterConfigs();
            var kirin = configs[806].characterData.files.Single(file => file.filePath.Replace('\\', '/').EndsWith("/c/sasu/a/Kirin.png"));
            Assert.That(kirin.startFrame, Is.EqualTo(38));
            Assert.That(kirin.endFrame, Is.EqualTo(67));
            actual.DerivedPages.TryGetSource(kirin.filePath, out var mapped);
            Assert.That(mapped.Chunks.Sum(chunk => chunk.Count), Is.EqualTo(30));
            actual.AssertInputsCurrent();
        }
    }
}
#endif
