#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using NTSD.Animation;
using NTSD.App;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    public sealed class H08ResidentVisualMemoryDiagnostics
    {
        [Test]
        public void NativeSheetReuse_ColdSwitchDefaultsOff()
        {
            var property = typeof(CharacterAnimtorManager).GetProperty("ReuseNativeBodySheetsForDiagnostics",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(property, Is.Not.Null, "Native per-publication sharing candidate is absent.");
            var go = new GameObject("H08ColdSwitch") { hideFlags = HideFlags.HideAndDontSave };
            go.SetActive(false);
            try
            {
                var manager = go.AddComponent<CharacterAnimtorManager>();
                Assert.That(property.GetValue(manager), Is.False);
                property.SetValue(manager, true);
                Assert.That(property.GetValue(manager), Is.True);
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
        }

        private const string Output = "artifacts/diagnostics/NTSD-H08-RESIDENT-VISUAL-MEMORY-REVIEW-20261010";
        private const BindingFlags HiddenStatic = BindingFlags.NonPublic | BindingFlags.Static;

        private sealed class Reference
        {
            public int id;
            public SpriteFileInfo file;
            public HashSet<int> owned;
        }

        private static LoganVisualContentCandidate Capture()
        {
            var config = UnityEditor.AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/NTSD/Config/GameConfig/GameConfig.asset");
            Assert.That(config, Is.Not.Null);
            return LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(
                Path.GetFullPath(config.BattleContentRuntimeRoot)), ProjectBattleModeConfig.LoadDefault().Capture());
        }

        private static List<Reference> References(LoganVisualContentCandidate candidate)
        {
            var configs = CharacterAnimtorManager.BuildCharacterFrameConfigsFromCatalog(candidate.Catalog);
            var result = new List<Reference>();
            foreach (var pair in configs)
            {
                var files = pair.Value.characterData.files;
                var owned = CharacterAnimtorManager.BuildFirstDeclaredSpriteOwnership(files);
                for (int i = 0; i < files.Count; i++)
                    if (owned[i].Count > 0 && !candidate.IsMissingBodySheet(files[i].filePath))
                        result.Add(new Reference { id = pair.Key, file = files[i], owned = owned[i] });
            }
            return result;
        }

        private static (int width, int height) Dimensions(string path)
        {
            using var input = File.OpenRead(path);
            var bytes = new byte[26];
            Assert.That(input.Read(bytes, 0, bytes.Length), Is.EqualTo(bytes.Length));
            if (bytes[0] == 137 && bytes[1] == 80 && bytes[2] == 78 && bytes[3] == 71)
            {
                int Read(int i) => (bytes[i] << 24) | (bytes[i + 1] << 16) | (bytes[i + 2] << 8) | bytes[i + 3];
                return (Read(16), Read(20));
            }
            Assert.That(bytes[0], Is.EqualTo((byte)'B'), path);
            Assert.That(bytes[1], Is.EqualTo((byte)'M'), path);
            return (BitConverter.ToInt32(bytes, 18), Math.Abs(BitConverter.ToInt32(bytes, 22)));
        }

        private static void Save(string file, object value)
        {
            Directory.CreateDirectory(Output);
            string path = Path.Combine(Output, file);
            Assert.That(File.Exists(path), Is.False, "Preserve existing evidence.");
            File.WriteAllText(path, JsonConvert.SerializeObject(value, Formatting.Indented));
        }

        [Test]
        public void CurrentContent_LiveSheetReferenceInventory()
        {
            var candidate = Capture();
            var references = References(candidate);
            var rows = references.GroupBy(r => Path.GetFullPath(r.file.filePath), StringComparer.Ordinal)
                .Select(g =>
                {
                    var size = Dimensions(g.Key);
                    long bytes = checked((long)size.width * size.height * 4);
                    return new { path = g.Key, references = g.Count(), width = size.width, height = size.height,
                        rgba32Bytes = bytes, repeatedRgba32Bytes = bytes * (g.Count() - 1),
                        owners = g.Select(r => new { r.id, r.file.startFrame, r.file.endFrame,
                            r.file.width, r.file.height, ownedPics = r.owned.Count }).ToArray() };
                }).OrderByDescending(r => r.repeatedRgba32Bytes).ToArray();
            candidate.AssertInputsCurrent();
            Save("inventory.json", new { status = "SOURCE_INVENTORY_VERIFIED", runtimeRoot = candidate.Catalog.Source.RuntimeRoot,
                candidate.VisualFingerprint, definitionCount = candidate.Catalog.Entries.Count,
                liveSheetReferences = references.Count, uniquePaths = rows.Length,
                repeatedPaths = rows.Count(r => r.references > 1),
                referenceRgba32Bytes = rows.Sum(r => r.rgba32Bytes * r.references),
                uniqueRgba32Bytes = rows.Sum(r => r.rgba32Bytes),
                repeatedRgba32Bytes = rows.Sum(r => r.repeatedRgba32Bytes),
                scope = "Header-based RGBA32 source payload estimates only; excludes native overhead, atlas, clamp, UI, audio and decode peak; not battle memory measurement.", rows });
        }

        [UnityTest]
        public IEnumerator DuplicateSheet_RealProductionStageWitness()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate = Capture();
                var group = References(candidate).GroupBy(r => Path.GetFullPath(r.file.filePath), StringComparer.Ordinal)
                    .Where(g => g.Count() > 1).OrderByDescending(g =>
                    { var d = Dimensions(g.Key); return (long)d.width * d.height * (g.Count() - 1); }).First();
                var selected = group.Take(2).ToArray();
                var textures = new HashSet<Texture2D>();
                var sprites = new HashSet<Sprite>();
                var atlas = new List<BattleAtlasSourcePixels>();
                var clamped = new Dictionary<string, Sprite>(StringComparer.Ordinal);
                var paths = new Dictionary<Sprite, string>();
                var sheetIds = new List<int>();
                var memory = new List<long>();
                var spriteCounts = new List<int>();
                var loader = typeof(CharacterAnimtorManager).GetMethod("ProcessAndCreateSpritesForCandidateAsync", HiddenStatic);
                Assert.That(loader, Is.Not.Null);
                try
                {
                    foreach (var reference in selected)
                    {
                        var staged = new Dictionary<int, List<Sprite>>
                        { [reference.id] = new List<Sprite>(new Sprite[reference.file.endFrame + 1]) };
                        int before = atlas.Count;
                        using var cpu = new SemaphoreSlim(0, 1);
                        using var upload = new SemaphoreSlim(1, 1);
                        var oldTextures = new HashSet<Texture2D>(textures);
                        var pending = (UniTask<int>)loader.Invoke(null, new object[] { reference.id, reference.file,
                            reference.owned, staged, sprites, textures, atlas, clamped, paths,
                            null, cpu, upload, candidate.Catalog.Source,
                            candidate.GetImageSha256(reference.file.filePath), new Func<bool>(() => true) });
                        int count = await pending;
                        Assert.That(count, Is.GreaterThan(0));
                        Assert.That(cpu.CurrentCount, Is.EqualTo(1));
                        Assert.That(upload.CurrentCount, Is.EqualTo(1));
                        Assert.That(atlas[before].Path, Is.EqualTo(group.Key));
                        var size = Dimensions(group.Key);
                        var texture = textures.Except(oldTextures).Single(t => t.width == size.width && t.height == size.height);
                        Assert.That(texture.isReadable, Is.False);
                        sheetIds.Add(texture.GetInstanceID());
                        memory.Add(Profiler.GetRuntimeMemorySizeLong(texture));
                        spriteCounts.Add(count);
                    }
                    Assert.That(sheetIds[0], Is.Not.EqualTo(sheetIds[1]));
                    var sheets = atlas.Where(a => a.Path == group.Key).ToArray();
                    Assert.That(sheets.Length, Is.EqualTo(2));
                    Assert.That(sheets[0].Pixels, Is.EqualTo(sheets[1].Pixels));
                    Assert.That(ReferenceEquals(sheets[0].Pixels, sheets[1].Pixels), Is.False);
                    candidate.AssertInputsCurrent();
                    Save("duplicate-witness.json", new { status = "REAL_PRODUCTION_DUPLICATION_CONFIRMED",
                        path = group.Key, selectedOwners = selected.Select(r => r.id).ToArray(), sheetIds,
                        profilerRuntimeMemoryBytes = memory, spriteCounts,
                        rgba32BytesPerSheet = (long)sheets[0].Width * sheets[0].Height * 4,
                        separateProcessedPixelArrays = true, exactPixelsEqual = true, sourceReadable = false,
                        scope = "Two isolated real production sheet loads, not whole battle memory or optimized candidate. Profiler values reported independently from payload estimates." });
                }
                finally
                {
                    foreach (var sprite in sprites) UnityEngine.Object.DestroyImmediate(sprite);
                    foreach (var texture in textures) UnityEngine.Object.DestroyImmediate(texture);
                    Assert.That(sprites.All(s => s == null), Is.True);
                    Assert.That(textures.All(t => t == null), Is.True);
                }
            });
        }

        [Test]
        public void SharedSheet_DistinctSpriteGeometryAndOriginalRetirement()
        {
            var textures = new HashSet<Texture2D>();
            var sprites = new HashSet<Sprite>();
            try
            {
                var texture = new Texture2D(32, 16, TextureFormat.RGBA32, false);
                textures.Add(texture);
                texture.SetPixels32(new Color32[512]);
                texture.Apply(false, true);
                var first = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0), 100f, 0, SpriteMeshType.FullRect);
                var second = Sprite.Create(texture, new Rect(9, 1, 12, 13), new Vector2(0.25f, 0.75f), 100f, 0, SpriteMeshType.FullRect);
                sprites.Add(first);
                sprites.Add(second);
                Assert.That(first, Is.Not.SameAs(second));
                Assert.That(first.texture, Is.SameAs(second.texture));
                Assert.That(second.rect, Is.EqualTo(new Rect(9, 1, 12, 13)));
                Assert.That(second.pivot, Is.EqualTo(new Vector2(3, 9.75f)));
                long nativeBytes = Profiler.GetRuntimeMemorySizeLong(texture);
                Assert.That(nativeBytes, Is.GreaterThan(0));
                Assert.That(textures.Add(second.texture), Is.False, "Existing ownership set must deduplicate shared texture.");
                int destroyed = CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures);
                Assert.That(destroyed, Is.EqualTo(3), "Two distinct sprites, one shared texture.");
                Assert.That(first == null && second == null && texture == null, Is.True);
                Assert.That(CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures), Is.Zero);
                Save("shared-owner-witness.json", new { status = "EXISTING_OWNER_HELPER_COMPATIBLE",
                    distinctSprites = 2, sharedTextures = 1, destroyed, nativeBytes,
                    scope = "Isolated geometry and existing destruction helper, not cancellation/lease/full publication candidate acceptance." });
            }
            finally
            {
                foreach (var sprite in sprites) if (sprite != null) UnityEngine.Object.DestroyImmediate(sprite);
                foreach (var texture in textures) if (texture != null) UnityEngine.Object.DestroyImmediate(texture);
            }
        }
    }
}
#endif
