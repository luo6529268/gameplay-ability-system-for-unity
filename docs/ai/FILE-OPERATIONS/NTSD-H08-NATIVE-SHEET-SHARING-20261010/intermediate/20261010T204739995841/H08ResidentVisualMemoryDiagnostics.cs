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
using NTSD.Simulation.Presentation;
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


        private const string CandidateOutput = "artifacts/diagnostics/NTSD-H08-NATIVE-SHEET-SHARING-20261010";
        private const BindingFlags HiddenInstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        private static void SaveCandidate(string name, object value)
        {
            Directory.CreateDirectory(CandidateOutput);
            using var stream = new FileStream(Path.Combine(CandidateOutput, name), FileMode.CreateNew, FileAccess.Write);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, Formatting.Indented));
            stream.Write(bytes, 0, bytes.Length);
        }

        private sealed class StageSnapshot
        {
            public int declarations, sprites, textures, sourceRecords, created, failed;
            public long sourcePayloadBytes, textureMemoryBytes;
            public long[] textureMemory;
            public int[] textureIds;
            public string[] sourcePaths;
            public bool cleaned;
            public string[] geometry, catalog;
            [JsonIgnore] public Dictionary<string, Color32[]> pixels = new Dictionary<string, Color32[]>();
        }

        private static string Geometry(Sprite sprite)
        {
            if (sprite == null) return "null";
            var values = new List<float> { sprite.rect.x, sprite.rect.y, sprite.rect.width, sprite.rect.height,
                sprite.pivot.x, sprite.pivot.y, sprite.pixelsPerUnit,
                sprite.bounds.center.x, sprite.bounds.center.y, sprite.bounds.center.z,
                sprite.bounds.size.x, sprite.bounds.size.y, sprite.bounds.size.z };
            foreach (Vector2 value in sprite.vertices) { values.Add(value.x); values.Add(value.y); }
            foreach (Vector2 value in sprite.uv) { values.Add(value.x); values.Add(value.y); }
            return string.Join(",", values.Select(v => v.ToString("R", System.Globalization.CultureInfo.InvariantCulture)));
        }

        private static IList Groups(LoganVisualContentCandidate candidate, Reference[] references)
        {
            var files = references.Select(r => (r.id, r.file, r.owned)).ToList();
            return (IList)typeof(CharacterAnimtorManager).GetMethod("BuildNativeBodySheetGroups", HiddenStatic)
                .Invoke(null, new object[] { files, candidate });
        }

        private static async UniTask<StageSnapshot> Stage(LoganVisualContentCandidate candidate,
            Reference[] references, bool enabled, bool wrongHash = false, bool cancel = false, bool legacy = false)
        {
            var result = new StageSnapshot { declarations = references.Length };
            var textures = new HashSet<Texture2D>();
            var sprites = new HashSet<Sprite>();
            var sources = new List<BattleAtlasSourcePixels>();
            var clamps = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            var paths = new Dictionary<Sprite, string>();
            var staged = references.GroupBy(r => r.id).ToDictionary(g => g.Key,
                g => new List<Sprite>(new Sprite[g.Max(r => r.file.endFrame) + 1]));
            var single = typeof(CharacterAnimtorManager).GetMethod("ProcessAndCreateSpritesForCandidateAsync", HiddenStatic);
            var grouped = typeof(CharacterAnimtorManager).GetMethod("ProcessNativeBodySheetGroupAsync", HiddenStatic);
            try
            {
                if (enabled)
                {
                    foreach (object group in Groups(candidate, references))
                    {
                        if (wrongHash) group.GetType().GetField("ExpectedSha256", HiddenInstance).SetValue(group, new string('0', 64));
                        using var cpu = new SemaphoreSlim(0, 1);
                        using var upload = new SemaphoreSlim(1, 1);
                        var count = await (UniTask<(int created, int failed)>)grouped.Invoke(null, new object[] {
                            group, staged, sprites, textures, sources, clamps, paths, null, cpu, upload,
                            candidate.Catalog.Source, new Func<bool>(() => !cancel) });
                        result.created += count.created;
                        result.failed += count.failed;
                        Assert.That(cpu.CurrentCount, Is.EqualTo(1));
                        Assert.That(upload.CurrentCount, Is.EqualTo(1));
                    }
                }
                else
                {
                    foreach (Reference reference in references)
                    {
                        using var cpu = new SemaphoreSlim(0, 1);
                        using var upload = new SemaphoreSlim(1, 1);
                        int count = await (UniTask<int>)single.Invoke(null, new object[] { reference.id, reference.file,
                            reference.owned, staged, sprites, textures, sources, clamps, paths, null, cpu, upload,
                            legacy ? null : candidate.Catalog.Source,
                            wrongHash ? new string('0',64) : legacy ? null : candidate.GetImageSha256(reference.file.filePath),
                            legacy ? null : new Func<bool>(() => !cancel) });
                        if (count < 0) result.failed++; else result.created += count;
                        Assert.That(cpu.CurrentCount, Is.EqualTo(1));
                        Assert.That(upload.CurrentCount, Is.EqualTo(1));
                    }
                }
                result.textures = textures.Count;
                result.sprites = sprites.Count;
                result.sourceRecords = sources.Count;
                result.sourcePayloadBytes = sources.Sum(s => (long)s.Pixels.Length * 4);
                result.textureIds = textures.Select(t => t.GetInstanceID()).ToArray();
                result.sourcePaths = sources.Select(s => s.Path).ToArray();
                result.textureMemory = textures.Select(Profiler.GetRuntimeMemorySizeLong).ToArray();
                result.textureMemoryBytes = result.textureMemory.Sum();
                foreach (var source in sources)
                {
                    if (result.pixels.TryGetValue(source.Path, out Color32[] before))
                        CollectionAssert.AreEqual(before, source.Pixels);
                    else result.pixels.Add(source.Path, source.Pixels);
                }
                result.geometry = staged.OrderBy(p => p.Key).SelectMany(p =>
                    p.Value.Select((sprite, pic) => p.Key + "/" + pic + ":" + Geometry(sprite))).ToArray();
                if (result.failed == 0 && !cancel)
                {
                    var configs = references.GroupBy(r => r.id).ToDictionary(g => g.Key,
                        g => new LF2CharacterDataWrapper(g.Key, new LF2CharacterData { files = g.Select(r => r.file).ToList() }));
                    var catalog = CharacterAnimtorManager.BuildBattleSpriteCatalog(configs, staged, paths);
                    foreach (var entry in catalog.Entries.Values)
                        Assert.That(entry.SharedTexture, Is.SameAs(entry.LegacySprite.texture));
                    result.catalog = catalog.Entries.OrderBy(p => p.Key.ToString()).Select(p =>
                        p.Key + "|" + p.Value.SourceSheetPath + "|" + Geometry(p.Value.LegacySprite) + "|" +
                        p.Value.PixelRect.ToString("F6") + "|" + p.Value.Pivot.ToString("F6")).ToArray();
                }
            }
            finally
            {
                CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures);
                Assert.That(sprites.All(s => s == null) && textures.All(t => t == null), Is.True);
                Assert.That(CharacterAnimtorManager.DestroyStagedPresentation(sprites, textures), Is.Zero);
                result.cleaned = true;
            }
            return result;
        }

        private static void EqualStages(StageSnapshot off, StageSnapshot on)
        {
            Assert.That(off.failed, Is.Zero);
            Assert.That(on.failed, Is.Zero);
            Assert.That(on.created, Is.EqualTo(off.created));
            Assert.That(on.sprites, Is.EqualTo(off.sprites));
            CollectionAssert.AreEqual(off.geometry, on.geometry);
            CollectionAssert.AreEqual(off.catalog, on.catalog);
            CollectionAssert.AreEquivalent(off.pixels.Keys, on.pixels.Keys);
            foreach (var item in off.pixels) CollectionAssert.AreEqual(item.Value, on.pixels[item.Key]);
            Assert.That(off.cleaned && on.cleaned, Is.True);
        }

        [Test]
        public void NativeSheetReuse_CurrentInventoryGrouping()
        {
            var candidate = Capture();
            var references = References(candidate).ToArray();
            var frozen = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(Path.Combine(Output, "inventory.json")));
            Assert.That(candidate.VisualFingerprint, Is.EqualTo((string)frozen["VisualFingerprint"]));
            IList groups = Groups(candidate, references);
            Assert.That(groups.Count, Is.EqualTo(703));
            var rows = new List<object>();
            long repeated = 0;
            foreach (object group in groups)
            {
                var members = (List<(int id, SpriteFileInfo file, HashSet<int> owned)>)group.GetType()
                    .GetField("Members", HiddenInstance).GetValue(group);
                if (members.Count <= 1) continue;
                string path = Path.GetFullPath(members[0].file.filePath);
                string hash = (string)group.GetType().GetField("ExpectedSha256", HiddenInstance).GetValue(group);
                foreach (var item in members)
                {
                    Assert.That(BattleAtlasLayoutPlanner.NormalizePath(Path.GetFullPath(item.file.filePath)),
                        Is.EqualTo(BattleAtlasLayoutPlanner.NormalizePath(path)));
                    Assert.That(candidate.GetImageSha256(item.file.filePath), Is.EqualTo(hash));
                }
                var size = Dimensions(path);
                long bytes = (long)size.width * size.height * 4 * (members.Count - 1);
                repeated += bytes;
                rows.Add(new { path, expectedSha256 = hash, references = members.Count, repeatedRgba32Bytes = bytes,
                    declarations = members.Select(m => new { m.id, m.file.startFrame, m.file.endFrame, m.file.width,
                        m.file.height, m.file.row, m.file.col, ownedPics = m.owned.OrderBy(x => x).ToArray() }).ToArray() });
            }
            Assert.That(rows.Count, Is.EqualTo(56));
            Assert.That(repeated, Is.EqualTo(184792652));
            candidate.AssertInputsCurrent();
            SaveCandidate("inventory-reconciliation.json", new { status = "COLD_GROUPING_VERIFIED",
                candidate.VisualFingerprint, references = references.Length, groups = groups.Count,
                repeatedRgba32Estimate = repeated, rows });
        }

        [UnityTest, Explicit("Single bounded current-content memory witness after correctness and ownership gates")]
        public IEnumerator NativeSheetReuse_OneBoundedMemoryGroupSet()
        {
            return UniTask.ToCoroutine(async () =>
            {
                Directory.CreateDirectory(CandidateOutput);
                using (var marker = new FileStream(Path.Combine(CandidateOutput, "memory.started"), FileMode.CreateNew)) { }
                var records = new List<object>();
                string status = "IN_PROGRESS";
                string failure = null;
                try
                {
                    var candidate = Capture();
                    var groups = References(candidate).GroupBy(r => Path.GetFullPath(r.file.filePath), StringComparer.Ordinal).ToArray();
                    var five = groups.Where(g => g.Count() == 5).OrderByDescending(g => { var d = Dimensions(g.Key); return (long)d.width * d.height; }).First();
                    var crop = groups.Where(g => g.Count() > 1 && g.Key != five.Key &&
                        g.Select(r => (r.file.startFrame, r.file.endFrame, r.file.width, r.file.height)).Distinct().Count() > 1)
                        .OrderBy(g => { var d = Dimensions(g.Key); return (long)d.width * d.height; }).First();
                    var large = groups.Where(g => g.Count() == 2 && g.Key != crop.Key).OrderByDescending(g =>
                        { var d = Dimensions(g.Key); return (long)d.width * d.height; }).First();
                    var unique = groups.Where(g => g.Count() == 1).OrderBy(g =>
                        { var d = Dimensions(g.Key); return (long)d.width * d.height; }).First();
                    foreach (var group in new[] { five, crop, large, unique })
                    {
                        StageSnapshot off = await Stage(candidate, group.ToArray(), false);
                        StageSnapshot on = await Stage(candidate, group.ToArray(), true);
                        records.Add(new { path = group.Key, references = group.Count(), off, on });
                        EqualStages(off, on);
                        Assert.That(off.sourceRecords, Is.EqualTo(group.Count()));
                        Assert.That(on.sourceRecords, Is.EqualTo(1));
                        Assert.That(off.textures, Is.EqualTo(group.Count()));
                        Assert.That(on.textures, Is.EqualTo(1));
                        if (group.Count() > 1) Assert.That(on.textureMemoryBytes, Is.LessThan(off.textureMemoryBytes));
                        else Assert.That(on.textureMemoryBytes, Is.EqualTo(off.textureMemoryBytes));
                    }
                    candidate.AssertInputsCurrent();
                    status = "SCOPED_GROUP_RESOURCE_MEMORY_GAIN_VERIFIED_DEFAULT_OFF";
                }
                catch (Exception error) { status = "FAILED_STOPPED_DEFAULT_OFF"; failure = error.ToString(); throw; }
                finally { SaveCandidate("bounded-memory.json", new { status, failure, records,
                    scope = "Actual original/grouped native sheet workers; one OFF/ON per frozen group, source resources only; full coordinator/pixels/atlas tested separately; no whole battle/peak/device measurement." }); }
            });
        }


        private static LoganVisualContentCandidate BoundaryCandidate(string imageKind, bool separatePaths = false)
        {
            string root = Path.GetFullPath(Path.Combine("Temp", "NTSDH08Sharing", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(Path.Combine(root, "decoded_dat"));
            Directory.CreateDirectory(Path.Combine(root, "vfs", "c"));
            byte[] bytes;
            if (imageKind == "bmp")
            {
                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                writer.Write((byte)'B'); writer.Write((byte)'M'); writer.Write(54 + 8 * 8 * 3);
                writer.Write(0); writer.Write(54); writer.Write(40); writer.Write(8); writer.Write(8);
                writer.Write((short)1); writer.Write((short)24); writer.Write(0); writer.Write(8 * 8 * 3);
                writer.Write(0); writer.Write(0); writer.Write(0); writer.Write(0);
                for (int i = 0; i < 64; i++) { writer.Write((byte)(30 + i)); writer.Write((byte)80); writer.Write((byte)190); }
                bytes = stream.ToArray();
            }
            else if (imageKind == "empty") bytes = Array.Empty<byte>();
            else if (imageKind == "corrupt") bytes = new byte[] { 1, 2, 3, 4 };
            else
            {
                var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
                try
                {
                    var pixels = Enumerable.Range(0, 64).Select(i => new Color32((byte)(20 + i), (byte)(180 - i), 90, (byte)(100 + i))).ToArray();
                    texture.SetPixels32(pixels); texture.Apply();
                    bytes = texture.EncodeToPNG();
                }
                finally { UnityEngine.Object.DestroyImmediate(texture); }
            }
            string extension = imageKind == "bmp" ? ".bmp" : ".png";
            foreach (string path in new[] { "body", "other" }) File.WriteAllBytes(Path.Combine(root, "vfs", "c", path + extension), bytes);
            File.WriteAllText(Path.Combine(root, "catalog.csv"),
                "registry_section,registry_index,id,type,source_path,published_folder\nobject,0,56,0,a.dat,missing\nobject,1,57,0,b.dat,missing\n");
            for (int i = 0; i < 2; i++)
            {
                string path = i == 1 && separatePaths ? "other" : "body";
                File.WriteAllText(Path.Combine(root, "decoded_dat", i == 0 ? "a.dat" : "b.dat"),
                    "<bmp_begin>\nname: H08\nfile(0-3): c/" + path + extension +
                    (i == 0 ? " w: 3 h: 3 row: 2 col: 2\n" : " w: 7 h: 7 row: 1 col: 4\n") +
                    "<bmp_end>\n<frame> 0 standing\npic: 0 state: 0 wait: 1 next: 0\n<frame_end>\n");
            }
            return LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(root));
        }

        [UnityTest]
        public IEnumerator NativeSheetReuse_GroupWorkerBoundaries()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var rows = new List<object>();
                foreach (string kind in new[] { "png", "bmp" })
                {
                    var candidate = BoundaryCandidate(kind);
                    var refs = References(candidate).ToArray();
                    Assert.That(refs.Length, Is.EqualTo(2));
                    StageSnapshot off = await Stage(candidate, refs, false);
                    StageSnapshot on = await Stage(candidate, refs, true);
                    EqualStages(off, on);
                    Assert.That(on.textures, Is.EqualTo(off.textures - 1));
                    Assert.That(on.sourceRecords, Is.EqualTo(off.sourceRecords - 1));
                    rows.Add(new { kind, off, on });
                    if (kind == "bmp")
                    {
                        StageSnapshot legacy = await Stage(candidate, refs, false, legacy: true);
                        Assert.That(legacy.failed, Is.Zero);
                        CollectionAssert.AreEqual(legacy.geometry, off.geometry);
                        foreach (var pixels in legacy.pixels) CollectionAssert.AreEqual(pixels.Value, off.pixels[pixels.Key]);
                    }
                }
                var separate = BoundaryCandidate("png", true);
                var separateRefs = References(separate).ToArray();
                Assert.That(separate.GetImageSha256(separateRefs[0].file.filePath),
                    Is.EqualTo(separate.GetImageSha256(separateRefs[1].file.filePath)));
                Assert.That(Groups(separate, separateRefs).Count, Is.EqualTo(2), "Equal content must not merge distinct paths.");
                EqualStages(await Stage(separate, separateRefs, false), await Stage(separate, separateRefs, true));

                var valid = BoundaryCandidate("png");
                var validRefs = References(valid).ToArray();
                StageSnapshot cancelled = await Stage(valid, validRefs, true, cancel: true);
                Assert.That(cancelled.textures, Is.Zero);
                Assert.That(cancelled.created, Is.Zero);
                bool previousLogs = LogAssert.ignoreFailingMessages;
                try
                {
                    LogAssert.ignoreFailingMessages = true;
                    StageSnapshot hashFailure = await Stage(valid, validRefs, true, wrongHash: true);
                    Assert.That(hashFailure.failed, Is.EqualTo(2));
                    Assert.That(hashFailure.textures, Is.Zero);
                    rows.Add(new { kind = "hash-mismatch", result = hashFailure });
                    var invalid = new Reference { id = 58,
                        file = new SpriteFileInfo(validRefs[0].file.filePath, 0, 0, -1, 3, 1, 1),
                        owned = new HashSet<int> { 0 } };
                    StageSnapshot badPlan = await Stage(valid, new[] { validRefs[0], invalid }, true);
                    Assert.That(badPlan.failed, Is.EqualTo(1), "Only invalid declaration is a failed sheet.");
                    Assert.That(badPlan.textures, Is.Zero, "Do not materialize a partially invalid group.");
                    rows.Add(new { kind = "invalid-plan", result = badPlan });
                    foreach (string kind in new[] { "empty", "corrupt" })
                    {
                        var candidate = BoundaryCandidate(kind);
                        StageSnapshot failure = await Stage(candidate, References(candidate).ToArray(), true);
                        Assert.That(failure.failed, Is.EqualTo(2));
                        Assert.That(failure.textures, Is.Zero);
                        rows.Add(new { kind, result = failure });
                    }
                }
                finally { LogAssert.ignoreFailingMessages = previousLogs; }
                SaveCandidate("group-worker-boundaries.json", new { status = "GROUP_WORKER_BOUNDARIES_PASS",
                    scope = "Actual worker; original native coordinator, publication and pixel gates are separate.", rows });
            });
        }


        internal static void VerifySharedPublicationDraw(BattleSpriteCatalog catalog, string label)
        {
            const int size = 96;
            Type oracle = typeof(BattleCentralSameSamplePixelEditorTests);
            var makeQuad = oracle.GetMethod("MakeIndependentQuad", HiddenStatic);
            var trusted = typeof(NTSD.Animation.Rendering.Editor.BattleCatalogCentralResourceResolverEditorTests)
                .GetMethod("CreateTrustedCommandWithIdentity", HiddenStatic);
            Assert.That(catalog.TryGet(56, 0, out var first), Is.True);
            Assert.That(catalog.TryGet(57, 0, out var second), Is.True);
            int mode = (int)first.CentralBinding.Mode;
            using var fixture = new NTSD.Animation.Rendering.Editor.BattleCatalogCentralResourceResolverEditorTests.SegmentUvFixture(mode);
            fixture.Catalog = catalog; fixture.Configure();
            using var backend = new NTSD.Animation.Rendering.BattleDynamicMeshBackend();
            backend.PrepareCapacity(2);
            var frame = new NTSD.Simulation.Presentation.BattlePresentationFrame();
            var resources = new NTSD.Animation.Rendering.BattleCentralResolvedResource[2];
            var meshes = new Mesh[2];
            var target = new RenderTexture(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(size, size, TextureFormat.RGBA32, false, true);
            using var buffer = new UnityEngine.Rendering.CommandBuffer();
            var block = new MaterialPropertyBlock();
            var previous = RenderTexture.active;
            Color32 background = new Color32(11, 17, 23, 255);
            void Begin()
            {
                buffer.Clear(); buffer.SetRenderTarget(target); buffer.ClearRenderTarget(false, true, background);
                buffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(
                    0, 8 * NTSD.Animation.NTSDRenderSpace.UnitsPerPixelX,
                    0, 8 * NTSD.Animation.NTSDRenderSpace.UnitsPerPixelY, -1, 1));
            }
            Color32[] Read()
            {
                Graphics.ExecuteCommandBuffer(buffer); RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, size, size), 0, 0); readback.Apply(); return readback.GetPixels32();
            }
            void Draw(Mesh mesh, Material material, Texture texture, int submesh)
            {
                block.Clear(); block.SetTexture(texture is Texture2DArray ? "_MainTexArray" : "_MainTex", texture);
                buffer.DrawMesh(mesh, Matrix4x4.identity, material, submesh, 0, block);
            }
            try
            {
                target.Create();
                int index = 0;
                foreach (var entry in new[] { first, second })
                {
                    var color = index == 0 ? new Color32(230, 60, 80, 145) : new Color32(60, 210, 240, 180);
                    var command = new NTSD.Simulation.Presentation.BattleRenderCommand(
                        NTSD.Simulation.Presentation.BattleRenderCommandType.Entity,
                        new NTSD.Simulation.RuntimeEntityHandle(index + 1, 1), index + 1,
                        entry.Key.VisualDataId, entry.Key.EffectivePic, 0, index, 0, 0, index,
                        new Vector3((4 + index * .2f) * NTSD.Animation.NTSDRenderSpace.UnitsPerPixelX,
                            4 * NTSD.Animation.NTSDRenderSpace.UnitsPerPixelY, 0),
                        new Vector2(entry.PixelWidth, entry.PixelHeight), entry.Pivot, entry.NormalizedUv,
                        new BattleSpriteRenderState(color, false, false, SpriteMaskInteraction.None,
                            BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha),
                        new NTSD.Simulation.Presentation.BattleSpriteValueDescriptor(true, entry.LegacySprite != null,
                            entry.LegacySprite.GetInstanceID(), entry.SharedTexture.GetInstanceID(), 0, entry.PixelRect,
                            entry.Pivot, BattleVisualResourceKey.FromEntity(entry.Key)));
                    command = (NTSD.Simulation.Presentation.BattleRenderCommand)trusted.Invoke(null, new object[] { command, entry });
                    frame.AddCommand(command);
                    Assert.That(fixture.Resolver.ResolvePrepared(command, out resources[index]),
                        Is.EqualTo(NTSD.Animation.Rendering.BattleCentralResourceStatus.Resolved));
                    meshes[index] = (Mesh)makeQuad.Invoke(null, new object[] { command, resources[index] });
                    index++;
                }
                string frozen = NTSD.Animation.Rendering.Editor.BattleCatalogCentralResourceResolverEditorTests.CaptureFields(frame);
                Begin();
                for (int i = 0; i < 2; i++) Draw(meshes[i], resources[i].Material, resources[i].Texture, 0);
                Color32[] expected = Read();
                backend.Build(frame, fixture.Resolver);
                Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(2));
                Begin();
                for (int i = 0; i < backend.SegmentCount; i++)
                {
                    var segment = backend.GetSegment(i);
                    Draw(backend.GetChunkMesh(segment.ChunkIndex), segment.Material, segment.Texture, segment.SubMeshIndex);
                }
                Color32[] actual = Read();
                CollectionAssert.AreEqual(expected, actual, "Shared publication draw differs from independent quad oracle.");
                int content = expected.Count(p => !p.Equals(background));
                Assert.That(content, Is.GreaterThan(100));
                Begin();
                for (int i = 1; i >= 0; i--) Draw(meshes[i], resources[i].Material, resources[i].Texture, 0);
                Color32[] reversed = Read();
                int differences = expected.Where((p, i) => !p.Equals(reversed[i])).Count();
                Assert.That(differences, Is.GreaterThan(10), "Overlap must detect reversed consumption order.");
                Assert.That(NTSD.Animation.Rendering.Editor.BattleCatalogCentralResourceResolverEditorTests.CaptureFields(frame),
                    Is.EqualTo(frozen));
                var unresolved = new NTSD.Simulation.Presentation.BattlePresentationFrame();
                unresolved.AddCommand(new NTSD.Simulation.Presentation.BattleRenderCommand(
                    NTSD.Simulation.Presentation.BattleRenderCommandType.Entity, default, 999, 999, 0,
                    0, 0, 0, 0, 0, Vector3.zero, Vector2.one, Vector2.zero, new Rect(0, 0, 1, 1),
                    new BattleSpriteRenderState(Color.white, false, false, SpriteMaskInteraction.None,
                        BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha), default));
                backend.Build(unresolved, fixture.Resolver);
                Assert.That(backend.Diagnostics.UnresolvedCommandCount, Is.EqualTo(1));
                Assert.That(backend.SegmentCount, Is.Zero);
                SaveCandidate("overlap-" + label + ".json", new { status = "ACTUAL_DRAW_ORACLE_PASS", mode,
                    comparedPixels = actual.Length, contentPixels = content, reversedDifferentPixels = differences,
                    exactPixels = true, publicationUnchanged = true, unresolvedSegments = backend.SegmentCount });
            }
            finally
            {
                RenderTexture.active = previous;
                foreach (Mesh mesh in meshes) if (mesh != null) UnityEngine.Object.DestroyImmediate(mesh);
                target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(readback);
            }
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
