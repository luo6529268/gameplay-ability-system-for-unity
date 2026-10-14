#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Animation.Rendering.Editor;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Test
{
    // Alignment contract: NTSD-M12-SPRITE-VISUAL-EQUIVALENCE-001. Diagnostic only.
    public sealed class M12SpriteVisualEquivalenceEditorTests
    {
        private const int Size = 256;
        private const string Evidence = "artifacts/diagnostics/M12-sprite-visual-equivalence-20261013";
        private const string Content = "Assets/NTSD/Content/LoganRuntime";
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private Material material;
        private string output;

        private sealed class Part
        {
            internal Texture2D Texture;
            internal Rect Source, Destination;
        }

        private sealed class Sample
        {
            internal string Name;
            internal Vector2 Canvas;
            internal Part Original;
            internal List<Part> Derived;
        }

        [SetUp]
        public void SetUp()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                Assert.Inconclusive("A real graphics device is required.");
            Assert.That(UnityEditor.EditorApplication.isPlaying, Is.False);
            var shader = Shader.Find("NTSD/BattleCentralTransparent");
            Assert.That(shader != null && shader.isSupported, Is.True);
            material = Track(new Material(shader) { hideFlags = HideFlags.HideAndDontSave });
            output = TestContext.CurrentContext.Test.Name.StartsWith("TrimmedPages")
                ? Path.GetFullPath(Path.Combine("artifacts/diagnostics/M12-transparent-trim-20261013/gpu", Guid.NewGuid().ToString("N")))
                : TestContext.CurrentContext.Test.Name.StartsWith("ExactDuplicateFrame")
                ? Path.GetFullPath(Path.Combine("artifacts/diagnostics/M12-exact-frame-dedup-20261013/gpu", Guid.NewGuid().ToString("N")))
                : Path.GetFullPath(Path.Combine(Evidence, TestContext.CurrentContext.Test.Name.StartsWith("Mobile1080P") ? "mobile-1080p-01/gpu" : "gpu", Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(output);
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }

        private T Track<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }

        private static string Hash(byte[] bytes)
        {
            using (var sha = SHA256.Create())
                return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant();
        }

        private Texture2D Texture(string path, bool repeat = false)
        {
            path = Path.GetFullPath(path);
            var data = BMPLoader.LoadVerifiedNativeImageData(path, Hash(File.ReadAllBytes(path)));
            Assert.That(data.Width <= SystemInfo.maxTextureSize && data.Height <= SystemInfo.maxTextureSize, Is.True);
            var texture = Track(new Texture2D(data.Width, data.Height, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave,
                filterMode = FilterMode.Point,
                wrapMode = repeat ? TextureWrapMode.Repeat : TextureWrapMode.Clamp
            });
            texture.SetPixels32(data.PngPixels);
            texture.Apply(false, true);
            return texture;
        }

        private static Part Whole(Texture2D texture, Rect source, Vector2 canvas) =>
            new Part { Texture = texture, Source = source, Destination = new Rect(Vector2.zero, canvas) };

        private static Rect JsonRect(JToken rect) => new Rect((int)rect["x"], (int)rect["y"],
            (int)rect["width"], (int)rect["height"]);

        private static Part PagePart(Texture2D texture, JToken chunk, int local, Vector2 canvas)
        {
            int index = local - (int)chunk["firstLocalPic"];
            return new Part { Texture = texture, Source = JsonRect(chunk["rects"][index]),
                Destination = chunk["contentRects"] == null ? new Rect(Vector2.zero, canvas)
                    : JsonRect(chunk["contentRects"][index]) };
        }

        private void Save(string name, byte[] bytes)
        {
            using (var stream = new FileStream(Path.Combine(output, name), FileMode.CreateNew))
                stream.Write(bytes, 0, bytes.Length);
        }

        private void SaveJson(string name, object data) =>
            Save(name, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, Formatting.Indented)));

        [Test]
        public void ExistingPages_OffscreenShaderMatchesOriginal()
        {
            var manifest = JObject.Parse(File.ReadAllText(Path.Combine(Content, "derived_sprite_pages/manifest.json")));
            var samples = new List<Sample>();
            foreach (var selection in new[] { ("c/dan/a/win2.png", 4, "win2"), ("c/sasu/a/Kirin.png", 10, "kirin"), ("c/sui/sui3.png", 7, "sui") })
            {
                var source = manifest["sources"].Single(item => (string)item["path"] == selection.Item1);
                int w = (int)source["frameWidth"], h = (int)source["frameHeight"], columns = (int)source["columns"];
                int local = selection.Item2;
                var original = Texture(Path.Combine(Content, "vfs", selection.Item1));
                var rect = new Rect(local % columns * (w + 1), original.height - (local / columns + 1) * (h + 1) + 1, w, h);
                var chunk = source["chunks"].Single(item => local >= (int)item["firstLocalPic"] &&
                    local < (int)item["firstLocalPic"] + (int)item["count"]);
                var page = manifest["pages"].Single(item => (string)item["id"] == (string)chunk["page"]);
                var r = chunk["rects"][local - (int)chunk["firstLocalPic"]];
                var derived = Texture(Path.Combine(Content, "derived_sprite_pages", (string)page["path"]));
                var canvas = new Vector2(w, h);
                samples.Add(new Sample { Name = selection.Item3, Canvas = canvas,
                    Original = Whole(original, rect, canvas),
                    Derived = new List<Part> { PagePart(derived, chunk, local, canvas) } });
            }
            Compare("existing-pages", samples);
        }

        [Test]
        public void IsolatedPrototypes_OffscreenShaderMatchesOriginal()
        {
            var samples = new List<Sample>();
            var white = Texture(Path.Combine(Content, "vfs/a/cla/fla.png"));
            var tinyWhite = Texture(Path.Combine(Evidence, "cpu/prototypes/white-1x1.png"));
            var whiteCanvas = new Vector2(3000, 800);
            samples.Add(new Sample { Name = "solid-white", Canvas = whiteCanvas,
                Original = Whole(white, new Rect(0, 0, 3000, 800), whiteCanvas),
                Derived = new List<Part> { Whole(tinyWhite, new Rect(0, 0, 1, 1), whiteCanvas) } });

            var stripe = Texture(Path.Combine(Content, "vfs/a/blac/blac.png"));
            var tinyStripe = Texture(Path.Combine(Evidence, "cpu/prototypes/blac-stripe-2x1.png"), true);
            var stripeCanvas = new Vector2(3000, 1300);
            samples.Add(new Sample { Name = "repeat-stripe", Canvas = stripeCanvas,
                Original = Whole(stripe, new Rect(0, 0, 3000, 1300), stripeCanvas),
                Derived = new List<Part> { Whole(tinyStripe, new Rect(0, 0, 3000, 1300), stripeCanvas) } });

            foreach (var selection in new[] { ("d0", "s/0/d0.png", 17), ("kirin", "c/sasu/a/Kirin.png", 10) })
            {
                var mapping = JObject.Parse(File.ReadAllText(Path.Combine(Evidence, "cpu/patches", selection.Item1, "mapping.json")));
                var geometry = mapping["sourceGeometry"];
                int w = (int)geometry["frameWidth"], h = (int)geometry["frameHeight"];
                int columns = (int)geometry["columns"], local = selection.Item3;
                var frame = mapping["frames"][local];
                Assert.That((bool)frame["emptyPatch"], Is.False);
                var rr = frame["rect"];
                int x = (int)rr[0], top = (int)rr[1], pw = (int)rr[2], ph = (int)rr[3];
                int y = h - top - ph;
                var canvas = new Vector2(w, h);
                var original = Texture(Path.Combine(Content, "vfs", selection.Item2));
                var baseTexture = Texture(Path.Combine(Evidence, "cpu/prototypes", selection.Item1 + "-base.png"));
                var patch = Texture(Path.Combine(Evidence, "cpu/patches", selection.Item1, "patch-" + local.ToString("000") + ".png"));
                var pieces = new List<Part>();
                foreach (var area in new[] { new Rect(0,0,x,h), new Rect(x+pw,0,w-x-pw,h),
                    new Rect(x,0,pw,y), new Rect(x,y+ph,pw,h-y-ph) })
                    if (area.width > 0 && area.height > 0)
                        pieces.Add(new Part { Texture = baseTexture, Source = area, Destination = area });
                pieces.Add(new Part { Texture = patch, Source = new Rect(0, 0, pw, ph), Destination = new Rect(x, y, pw, ph) });
                samples.Add(new Sample { Name = selection.Item1 + "-nonoverlap-patch", Canvas = canvas,
                    Original = Whole(original, new Rect(local % columns * (w + 1),
                        original.height - (local / columns + 1) * (h + 1) + 1, w, h), canvas), Derived = pieces });
            }
            Compare("isolated-prototypes", samples);
        }

        private void Compare(string category, List<Sample> samples)
        {
            SaveJson("planned-outputs.json", new { category, output,
                files = samples.Select(sample => sample.Name + ".png").Concat(new[] { "summary.json", "planned-outputs.json" }).ToArray() });
            var results = new List<object>();
            int changedCases = 0;
            long totalNonBackground = 0;
            foreach (var sample in samples)
            {
                Color32[] selectedA = null, selectedB = null;
                long worst = -1;
                foreach (bool flip in new[] { false, true })
                foreach (float alpha in new[] { 1f, 0.5f, 0.2f })
                foreach (float phase in new[] { 0f, 0.375f })
                {
                    var a = Render(sample.Canvas, new[] { sample.Original }, flip, alpha, phase);
                    var b = Render(sample.Canvas, sample.Derived, flip, alpha, phase);
                    long changed = 0, nonBackground = 0;
                    int max = 0;
                    for (int i = 0; i < a.Length; i++)
                    {
                        int d = Math.Max(Math.Max(Math.Abs(a[i].r - b[i].r), Math.Abs(a[i].g - b[i].g)),
                            Math.Max(Math.Abs(a[i].b - b[i].b), Math.Abs(a[i].a - b[i].a)));
                        if (d != 0) changed++;
                        max = Math.Max(max, d);
                        if (a[i].r != 32 || a[i].g != 48 || a[i].b != 64) nonBackground++;
                    }
                    if (changed > 0) changedCases++;
                    totalNonBackground += nonBackground;
                    results.Add(new { sample = sample.Name, flip, alpha, phase, changedPixels = changed, maxAbsDiff = max, nonBackground });
                    if (changed > worst) { worst = changed; selectedA = a; selectedB = b; }
                }
                SaveBoard(sample.Name + ".png", selectedA, selectedB);
            }
            SaveJson("summary.json", new { category, status = changedCases == 0 ? "GPU_AB_EQUAL" : "GPU_DIFFERENCES_DETECTED",
                cases = results.Count, changedCases, totalNonBackground, imageSize = Size, shader = material.shader.name,
                gpu = SystemInfo.graphicsDeviceName, graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                colorSpace = QualitySettings.activeColorSpace.ToString(), originalScene = "EditMode temporary test scene",
                productionChanged = false, testMode = "offscreen command-buffer only, not Battle Scene/Play",
                samples = results });
            Assert.That(totalNonBackground, Is.GreaterThan(0), "A completely blank draw is not proof.");
            Assert.That(changedCases, Is.Zero, "Strict GPU difference found; see " + output);
        }

        private Color32[] Render(Vector2 canvas, IEnumerable<Part> parts, bool flip, float alpha, float phase,
            int width = Size, int height = Size, float? explicitScale = null)
        {
            var meshes = new List<Mesh>();
            try
            {
                float scale = explicitScale ?? Math.Min((width - 12f) / canvas.x, (height - 12f) / canvas.y);
                var origin = new Vector2((width - canvas.x * scale) * .5f + phase, (height - canvas.y * scale) * .5f + phase);
                Action<CommandBuffer> enqueue = cb =>
                {
                    cb.ClearRenderTarget(false, true, new Color(32 / 255f, 48 / 255f, 64 / 255f, 1));
                    foreach (var part in parts)
                    {
                        var d = part.Destination;
                        var points = new[] { new Vector2(d.xMin,d.yMin), new Vector2(d.xMax,d.yMin),
                            new Vector2(d.xMax,d.yMax), new Vector2(d.xMin,d.yMax) };
                        var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
                        meshes.Add(mesh);
                        mesh.vertices = points.Select(p => new Vector3(origin.x + (flip ? canvas.x - p.x : p.x) * scale,
                            origin.y + p.y * scale, 0)).ToArray();
                        var s = part.Source;
                        float tw = part.Texture.width, th = part.Texture.height;
                        mesh.uv = new[] { new Vector2(s.xMin/tw,s.yMin/th), new Vector2(s.xMax/tw,s.yMin/th),
                            new Vector2(s.xMax/tw,s.yMax/th), new Vector2(s.xMin/tw,s.yMax/th) };
                        mesh.colors = Enumerable.Repeat(new Color(1, 1, 1, alpha), 4).ToArray();
                        var bounds = new Vector4((s.xMin+.5f)/tw,(s.yMin+.5f)/th,(s.xMax-.5f)/tw,(s.yMax-.5f)/th);
                        mesh.SetUVs(2, Enumerable.Repeat(bounds,4).ToList());
                        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
                        var block = new MaterialPropertyBlock();
                        block.SetTexture("_MainTex", part.Texture);
                        cb.DrawMesh(mesh, Matrix4x4.identity, material, 0, 0, block);
                    }
                };
                if (width != Size || height != Size || explicitScale.HasValue)
                    return ReadMobileCommandBuffer(width, height, enqueue);
                var method = typeof(BattleRenderingAcceptanceHarness).GetMethod("RenderCommandBuffer",
                    BindingFlags.Static | BindingFlags.NonPublic, null,
                    new[] { typeof(int), typeof(Vector2), typeof(float), typeof(Action<CommandBuffer>) }, null);
                Assert.That(method, Is.Not.Null);
                object image = method.Invoke(null, new object[] { Size, new Vector2(Size*.5f,Size*.5f), Size*.5f, enqueue });
                return (Color32[])image.GetType().GetProperty("Pixels").GetValue(image);
            }
            finally
            {
                foreach (var mesh in meshes) UnityEngine.Object.DestroyImmediate(mesh);
            }
        }



        [Test]
        public void ExactDuplicateFramePageMatchesPreviousPageOnGpu()
        {
            const int width = 1920, height = 1080;
            const float scale = 1.40625f;
            const string dedupEvidence = "artifacts/diagnostics/M12-exact-frame-dedup-20261013";
            var oldManifest = JObject.Parse(File.ReadAllText(Path.Combine(dedupEvidence, "before",
                Content, "derived_sprite_pages/manifest.json")));
            // Preserve this historical dedup regression against its accepted before-trim snapshot.
            var currentManifest = JObject.Parse(File.ReadAllText(Path.Combine(
                "artifacts/diagnostics/M12-transparent-trim-20261013/before", Content, "derived_sprite_pages/manifest.json")));
            var oldSource = oldManifest["sources"].Single(x => (string)x["path"] == "c/sui/sui3.png");
            var currentSource = currentManifest["sources"].Single(x => (string)x["path"] == "c/sui/sui3.png");
            var oldChunk = oldSource["chunks"].Single(x => (int)x["firstLocalPic"] == 18);
            var currentChunk = currentSource["chunks"].Single(x => (int)x["firstLocalPic"] == 18);
            var oldPage = oldManifest["pages"].Single(x => (string)x["id"] == (string)oldChunk["page"]);
            var currentPage = currentManifest["pages"].Single(x => (string)x["id"] == (string)currentChunk["page"]);
            Assert.That((bool)currentChunk["allowExactFrameAliases"], Is.True);
            Assert.That((int)currentPage["width"], Is.EqualTo(1144));
            Assert.That((int)currentPage["height"], Is.EqualTo(1144));
            string oldPath = Path.Combine(Content, "derived_sprite_pages", (string)oldPage["path"]);
            string currentPath = Path.Combine(Content, "derived_sprite_pages", (string)currentPage["path"]);
            Assert.That(Hash(File.ReadAllBytes(oldPath)), Is.EqualTo((string)oldPage["sha256"]));
            Assert.That(Hash(File.ReadAllBytes(currentPath)), Is.EqualTo((string)currentPage["sha256"]));
            var original = Texture(oldPath);
            var derived = Texture(currentPath);
            int w = (int)oldSource["frameWidth"], h = (int)oldSource["frameHeight"];
            var canvas = new Vector2(w, h);
            var rows = new List<MobileRow>();
            SaveJson("planned-outputs.json", new { files = new[] { "summary.json", "planned-outputs.json" },
                pattern = "local-<18..24>-<original/derived/difference>.png",
                comparison = "User-accepted previous1716page versus deduplicated1144page at1080p" });
            for (int local = 18; local < 25; local++)
            {
                var oldRect = oldChunk["rects"][local - 18];
                var currentRect = currentChunk["rects"][local - 18];
                var aPart = Whole(original, new Rect((int)oldRect["x"], (int)oldRect["y"], w, h), canvas);
                var bPart = Whole(derived, new Rect((int)currentRect["x"], (int)currentRect["y"], w, h), canvas);
                Color32[] worstA = null, worstB = null;
                int worst = -1;
                foreach (bool flip in new[] { false, true })
                foreach (float alpha in new[] { 1f, .5f, .2f })
                foreach (float phase in new[] { 0f, .375f })
                {
                    var a = Render(canvas, new[] { aPart }, flip, alpha, phase, width, height, scale);
                    var b = Render(canvas, new[] { bPart }, flip, alpha, phase, width, height, scale);
                    var row = MeasureMobile(a, b, "sui-" + local, "exact-frame-dedup", local,
                        width, height, scale, phase, flip, alpha);
                    rows.Add(row);
                    if (row.changedPixels > worst) { worst = row.changedPixels; worstA = a; worstB = b; }
                }
                SaveMobilePair("local-" + local, worstA, worstB, width, height, true);
            }
            SaveJson("summary.json", new { status = rows.All(x => x.changedPixels == 0) ? "GPU_EQUAL" : "GPU_DIFFERENCES",
                width, height, scale, cases = rows.Count, changedCases = rows.Count(x => x.changedPixels != 0),
                gpu = SystemInfo.graphicsDeviceName, graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                rows, sourceChanged = false, shaderChanged = false });
            Assert.That(rows.Sum(x => (long)x.visiblePixels), Is.GreaterThan(0));
            Assert.That(rows.Count, Is.EqualTo(84));
            Assert.That(rows.Count(x => x.changedPixels != 0), Is.Zero, "See " + output);
        }

        private Color32[] RenderProductionEntry(Vector2 canvas, Part part, bool flipX, bool flipY,
            float alpha, float phase, float scale)
        {
            var sprite = Sprite.Create(part.Texture, part.Source,
                new Vector2((canvas.x * .5f - part.Destination.x) / part.Destination.width,
                    -part.Destination.y / part.Destination.height), 100f, 0, SpriteMeshType.FullRect);
            try
            {
                var builder = new BattleSpriteCatalogBuilder();
                builder.Add(71, 0, "trim-gpu", part.Texture, part.Source, new Vector2(.5f, 0), sprite,
                    canvas, part.Destination);
                var catalog = builder.Publish();
                Assert.That(catalog.TryGet(71, 0, out var entry), Is.True);
                var resolver = new BattleCatalogCentralResourceResolver();
                resolver.Configure(catalog, material);
                var frame = new BattlePresentationFrame();
                frame.Reset(1);
                frame.AddCommand(M12SpriteTrimEditorTests.Command(entry, flipX, flipY,
                    new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255))));
                using var backend = new BattleDynamicMeshBackend();
                backend.PrepareCapacity(1);
                backend.Build(frame, resolver);
                Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(1));
                Assert.That(backend.GetChunkActiveQuadCount(0), Is.EqualTo(1));
                var matrix = Matrix4x4.TRS(new Vector3(960 + phase, (1080 - canvas.y * scale) * .5f + phase, 0),
                    Quaternion.identity, new Vector3(scale / (NTSDRenderSpace.UnitsPerPixelX * NTSDRenderSpace.BattleVisualScale),
                        scale / (NTSDRenderSpace.UnitsPerPixelY * NTSDRenderSpace.BattleVisualScale), 1));
                return ReadMobileCommandBuffer(1920, 1080, cb =>
                {
                    cb.ClearRenderTarget(false, true, new Color(32 / 255f, 48 / 255f, 64 / 255f, 1));
                    for (int index = 0; index < backend.SegmentCount; index++)
                    {
                        var segment = backend.GetSegment(index);
                        var properties = new MaterialPropertyBlock();
                        properties.SetTexture("_MainTex", segment.Texture);
                        cb.DrawMesh(backend.GetChunkMesh(segment.ChunkIndex), matrix, segment.Material,
                            segment.SubMeshIndex, 0, properties);
                    }
                });
            }
            finally { UnityEngine.Object.DestroyImmediate(sprite); }
        }

        [Test]
        public void TrimmedPagesCaptureAllFramesThroughProductionBackend1080P()
        {
            Assert.That(DetectPipeline(), Is.EqualTo("URP"));
            const float scale = 1.40625f;
            var previous = JObject.Parse(File.ReadAllText(Path.Combine(
                "artifacts/diagnostics/M12-transparent-trim-20261013/before", Content, "derived_sprite_pages/manifest.json")));
            var current = JObject.Parse(File.ReadAllText(Path.Combine(Content, "derived_sprite_pages/manifest.json")));
            var cache = new Dictionary<string, Texture2D>();
            Func<string, Texture2D> texture = p =>
            {
                if (!cache.TryGetValue(p, out var item)) cache[p] = item = Texture(p);
                return item;
            };
            Func<JObject, string, int, Part> part = (manifest, sourcePath, pic) =>
            {
                var source = manifest["sources"].Single(x => (string)x["path"] == sourcePath);
                var chunk = source["chunks"].Single(x => pic >= (int)x["firstLocalPic"] && pic < (int)x["firstLocalPic"] + (int)x["count"]);
                var page = manifest["pages"].Single(x => (string)x["id"] == (string)chunk["page"]);
                string file = Path.Combine(Content, "derived_sprite_pages", (string)page["path"]);
                Assert.That(Hash(File.ReadAllBytes(file)), Is.EqualTo((string)page["sha256"]));
                return PagePart(texture(file), chunk, pic, new Vector2((int)source["frameWidth"], (int)source["frameHeight"]));
            };
            var rows = new List<MobileRow>();
            SaveJson("planned-outputs.json", new { files = new[] { "summary.json", "planned-outputs.json", "negative-control.json" },
                patterns = new[] { "<group>-<pic>-original.png", "<group>-<pic>-derived.png", "<group>-<pic>-difference.png" },
                comparison = "Accepted full-canvas pages versus transparent trim; actual catalog/resolver/dynamic mesh shader path" });
            foreach (string path in new[] { "c/sasu/a/Kirin.png", "c/sui/sui3.png" })
            {
                var source = current["sources"].Single(x => (string)x["path"] == path);
                var canvas = new Vector2((int)source["frameWidth"], (int)source["frameHeight"]);
                string group = path.Contains("Kirin") ? "kirin" : "sui";
                for (int pic = 0; pic < (int)source["columns"] * (int)source["rows"]; pic++)
                {
                    Part aPart = part(previous, path, pic), bPart = part(current, path, pic);
                    int worst = -1;
                    Color32[] worstA = null, worstB = null;
                    foreach (bool flip in new[] { false, true })
                    foreach (float phase in new[] { 0f, .375f })
                    {
                        var a = RenderProductionEntry(canvas, aPart, flip, false, 1f, phase, scale);
                        var b = RenderProductionEntry(canvas, bPart, flip, false, 1f, phase, scale);
                        var row = MeasureMobile(a, b, group + "-" + pic, "trim-primary", pic, 1920, 1080, scale, phase, flip, 1f);
                        rows.Add(row);
                        if (row.changedPixels > worst) { worst = row.changedPixels; worstA = a; worstB = b; }
                    }
                    SaveMobilePair(group + "-" + pic.ToString("000"), worstA, worstB, 1920, 1080, true);
                }
                foreach (int pic in group == "kirin" ? new[] { 11, 22 } : new[] { 3, 21 })
                foreach (bool flip in new[] { false, true })
                foreach (float stressScale in new[] { scale, 2f })
                {
                    var a = RenderProductionEntry(canvas, part(previous, path, pic), flip, false, .5f, .5f, stressScale);
                    var b = RenderProductionEntry(canvas, part(current, path, pic), flip, false, .5f, .5f, stressScale);
                    rows.Add(MeasureMobile(a, b, group + "-" + pic, "trim-alpha-stress", pic, 1920, 1080, stressScale, .5f, flip, .5f));
                    SaveMobilePair(group + "-stress-" + pic + "-" + flip + "-" + stressScale, a, b, 1920, 1080, true);
                }
            }
            var controlPart = part(current, "c/sui/sui3.png", 0);
            var shifted = new Part { Texture = controlPart.Texture, Source = controlPart.Source,
                Destination = new Rect(controlPart.Destination.x + 1, controlPart.Destination.y,
                    controlPart.Destination.width, controlPart.Destination.height) };
            var good = RenderProductionEntry(new Vector2(567, 567), controlPart, false, false, 1f, .375f, scale);
            var bad = RenderProductionEntry(new Vector2(567, 567), shifted, false, false, 1f, .375f, scale);
            var control = MeasureMobile(good, bad, "offset-1px", "negative-control", 0, 1920, 1080, scale, .375f, false, 1);
            SaveJson("negative-control.json", control);
            SaveJson("summary.json", new { status = rows.All(x => x.changedPixels == 0) ? "GPU_EQUAL" : "GPU_DIFFERENCES_FOR_VISUAL_REVIEW",
                cases = rows.Count, changedCases = rows.Count(x => x.changedPixels != 0), rows,
                width = 1920, height = 1080, primaryScale = scale, gpu = SystemInfo.graphicsDeviceName,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(), sourceChanged = false, shaderChanged = false,
                note = "Primary220pairs plus16alpha/scale stress pairs; no physical phone or natural skill animation claim" });
            Assert.That(rows.Count, Is.EqualTo(236));
            Assert.That(rows.Sum(x => (long)x.visiblePixels), Is.GreaterThan(0));
            Assert.That(control.changedPixels, Is.GreaterThan(0), "The comparison must detect an incorrect1px crop offset.");
        }

        private sealed class MobileRow
        {
            public string sample, group;
            public int localPic, width, height, changedPixels, visiblePixels, highContrastPixels, maxAbsDiff;
            public bool flip;
            public float scale, phase, alpha;
            public double meanRgbDiff, changedVisibleRatio;
        }

        private string DetectPipeline()
        {
            var asset = GraphicsSettings.currentRenderPipeline;
            if (asset == null) return "BuiltIn";
            string name = asset.GetType().FullName ?? "";
            return name.Contains("Universal") ? "URP" : name.Contains("HighDefinition") ? "HDRP" : name;
        }

        [Test]
        public void Mobile1080P_CaptureWorstDifferences()
        {
            string pipeline = DetectPipeline();
            Assert.That(pipeline, Is.EqualTo("URP"));
            const int width = 1920, height = 1080;
            const float primaryScale = 1.40625f; // 1080 / (2 * 5.76 * 100) * 1.5; static Scene proxy only.
            var cache = new Dictionary<string, Texture2D>();
            Func<string, Texture2D> texture = p =>
            {
                if (!cache.TryGetValue(p, out var item)) cache[p] = item = Texture(p);
                return item;
            };
            var samples = new Dictionary<string, Sample>();
            var labels = new Dictionary<string, (string Group, int Pic)>();
            var manifest = JObject.Parse(File.ReadAllText(Path.Combine(Content, "derived_sprite_pages/manifest.json")));
            foreach (var path in new[] { "c/dan/a/win2.png", "c/sasu/a/Kirin.png", "c/sui/sui3.png" })
            {
                var source = manifest["sources"].Single(x => (string)x["path"] == path);
                int w = (int)source["frameWidth"], h = (int)source["frameHeight"], columns = (int)source["columns"];
                string group = path.Contains("Kirin") ? "kirin-page" : path.Contains("win2") ? "win2-page" : "sui-page";
                var original = texture(Path.Combine(Content, "vfs", path));
                for (int pic = 0; pic < (int)source["rows"] * columns; pic++)
                {
                    var chunk = source["chunks"].Single(x => pic >= (int)x["firstLocalPic"] && pic < (int)x["firstLocalPic"] + (int)x["count"]);
                    var page = manifest["pages"].Single(x => (string)x["id"] == (string)chunk["page"]);
                    var r = chunk["rects"][pic - (int)chunk["firstLocalPic"]];
                    var derived = texture(Path.Combine(Content, "derived_sprite_pages", (string)page["path"]));
                    var canvas = new Vector2(w, h);
                    string name = group + "-" + pic.ToString("000");
                    samples.Add(name, new Sample { Name = name, Canvas = canvas,
                        Original = Whole(original, new Rect(pic % columns * (w + 1), original.height - (pic / columns + 1) * (h + 1) + 1, w, h), canvas),
                        Derived = new List<Part> { PagePart(derived, chunk, pic, canvas) } });
                    labels.Add(name, (group, pic));
                }
            }
            foreach (var item in new[] { ("kirin", "c/sasu/a/Kirin.png"), ("d0", "s/0/d0.png") })
            {
                var mapping = JObject.Parse(File.ReadAllText(Path.Combine(Evidence, "cpu/patches", item.Item1, "mapping.json")));
                var geometry = mapping["sourceGeometry"];
                int w = (int)geometry["frameWidth"], h = (int)geometry["frameHeight"], columns = (int)geometry["columns"];
                var original = texture(Path.Combine(Content, "vfs", item.Item2));
                var baseTexture = texture(Path.Combine(Evidence, "cpu/prototypes", item.Item1 + "-base.png"));
                foreach (var frame in mapping["frames"])
                {
                    int pic = (int)frame["localPic"];
                    if (item.Item1 == "d0" && pic != 17) continue;
                    var canvas = new Vector2(w, h);
                    var pieces = new List<Part>();
                    if ((bool)frame["emptyPatch"])
                        pieces.Add(Whole(baseTexture, new Rect(0, 0, w, h), canvas));
                    else
                    {
                        var r = frame["rect"];
                        int x = (int)r[0], y = h - (int)r[1] - (int)r[3], pw = (int)r[2], ph = (int)r[3];
                        foreach (var area in new[] { new Rect(0,0,x,h), new Rect(x+pw,0,w-x-pw,h),
                            new Rect(x,0,pw,y), new Rect(x,y+ph,pw,h-y-ph) })
                            if (area.width > 0 && area.height > 0)
                                pieces.Add(new Part { Texture = baseTexture, Source = area, Destination = area });
                        pieces.Add(new Part { Texture = texture(Path.Combine(Evidence, "cpu/patches", item.Item1, "patch-" + pic.ToString("000") + ".png")),
                            Source = new Rect(0, 0, pw, ph), Destination = new Rect(x, y, pw, ph) });
                    }
                    string group = item.Item1 + "-patch", name = group + "-" + pic.ToString("000");
                    samples.Add(name, new Sample { Name = name, Canvas = canvas,
                        Original = Whole(original, new Rect(pic % columns * (w + 1), original.height - (pic / columns + 1) * (h + 1) + 1, w, h), canvas),
                        Derived = pieces });
                    labels.Add(name, (group, pic));
                }
            }
            var rows = new List<MobileRow>();
            var worst = new Dictionary<string, MobileRow>();
            SaveJson("planned-outputs.json", new { root = output, files = new[] { "summary.json", "scan.json", "motion.json", "planned-outputs.json", "progress.jsonl" },
                patterns = new[] { "worst-<group>-original.png", "worst-<group>-derived.png", "worst-<group>-difference.png", "motion-<group>-<phase>-original.png", "motion-<group>-<phase>-derived.png" },
                scope = "1920x1080 desktop offscreen proxy, not Android/phone; no Scene or camera edits" });
            using (var stream = new FileStream(Path.Combine(output, "progress.jsonl"), FileMode.CreateNew))
            using (var progress = new StreamWriter(stream) { AutoFlush = true })
            {
                foreach (var sample in samples.Values)
                {
                    foreach (bool flip in new[] { false, true })
                    foreach (float phase in new[] { 0f, .375f })
                    {
                        var a = Render(sample.Canvas, new[] { sample.Original }, flip, 1f, phase, width, height, primaryScale);
                        var b = Render(sample.Canvas, sample.Derived, flip, 1f, phase, width, height, primaryScale);
                        var label = labels[sample.Name];
                        var row = MeasureMobile(a, b, sample.Name, label.Group, label.Pic, width, height, primaryScale, phase, flip, 1f);
                        rows.Add(row);
                        if (!worst.TryGetValue(label.Group, out var previous) || row.meanRgbDiff > previous.meanRgbDiff)
                        {
                            worst[label.Group] = row;
                        }
                    }
                    progress.WriteLine(JsonConvert.SerializeObject(new { phase = "scan", sample = sample.Name, compared = rows.Count }));
                }
                SaveJson("scan.json", rows);
                foreach (var pair in worst)
                {
                    var row = pair.Value;
                    var sample = samples[row.sample];
                    var a = Render(sample.Canvas, new[] { sample.Original }, row.flip, row.alpha, row.phase, width, height, row.scale);
                    var b = Render(sample.Canvas, sample.Derived, row.flip, row.alpha, row.phase, width, height, row.scale);
                    SaveMobilePair("worst-" + pair.Key, a, b, width, height, true);
                }
                var motion = new List<MobileRow>();
                foreach (var group in new[] { "kirin-page", "win2-page", "kirin-patch" })
                {
                    var selected = worst[group];
                    var sample = samples[selected.sample];
                    foreach (float scale in new[] { .9375f, primaryScale, 2f })
                    for (int step = 0; step < 16; step++)
                    {
                        float phase = step / 16f;
                        var a = Render(sample.Canvas, new[] { sample.Original }, selected.flip, 1f, phase, width, height, scale);
                        var b = Render(sample.Canvas, sample.Derived, selected.flip, 1f, phase, width, height, scale);
                        motion.Add(MeasureMobile(a, b, sample.Name, group, selected.localPic, width, height, scale, phase, selected.flip, 1f));
                        if (scale == primaryScale)
                            SaveMobilePair("motion-" + group + "-" + step.ToString("00"), a, b, width, height, false);
                    }
                    progress.WriteLine(JsonConvert.SerializeObject(new { phase = "motion", sample = sample.Name, compared = motion.Count }));
                }
                SaveJson("motion.json", motion);
                SaveJson("summary.json", new { status = "CAPTURE_COMPLETE_REQUIRES_VISUAL_REVIEW", pipeline,
                    gpu = SystemInfo.graphicsDeviceName, graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                    colorSpace = QualitySettings.activeColorSpace.ToString(), width, height, primaryScale,
                    distinctPageFrames = 63, totalSampleFrames = samples.Count, scanCases = rows.Count,
                    scanChangedCases = rows.Count(x => x.changedPixels != 0), motionCases = motion.Count,
                    motionChangedCases = motion.Count(x => x.changedPixels != 0),
                    worstByGroup = worst.Values, sourceScaleFormula = "height/(2*SceneOrtho5.76*PPU100)*BattleVisualScale1.5; runtime background framing can override camera",
                    proxyLimit = "D3D11 render at1080p, not phone hardware; synthetic diagonal subpixel phases, not actual DAT sequence/movement",
                    productionChanged = false });
                Assert.That(rows.Count, Is.EqualTo(samples.Count * 4));
                Assert.That(rows.Sum(x => (long)x.visiblePixels), Is.GreaterThan(0));
            }
        }


        [Test]
        public void Mobile1080P_CaptureSelectedComparisons()
        {
            Assert.That(DetectPipeline(), Is.EqualTo("URP"));
            string requestPath = Path.Combine(Evidence, "mobile-1080p-01/selected-capture-request.json");
            var request = JObject.Parse(File.ReadAllText(requestPath));
            var cache = new Dictionary<string, Texture2D>();
            Func<string, Texture2D> texture = p =>
            {
                if (!cache.TryGetValue(p, out var item)) cache[p] = item = Texture(p);
                return item;
            };
            var manifest = JObject.Parse(File.ReadAllText(Path.Combine(Content, "derived_sprite_pages/manifest.json")));
            var rows = new List<MobileRow>();
            var files = new List<object>();
            SaveJson("planned-outputs.json", new { requestPath, requestSha256 = Hash(File.ReadAllBytes(requestPath)),
                patterns = new[] { "<label>-original.png", "<label>-derived.png", "<label>-difference.png", "summary.json", "planned-outputs.json" } });
            foreach (var target in request["cases"])
            {
                int pic = (int)target["localPic"];
                string group = (string)target["group"], label = (string)target["label"];
                string sourcePath = group.StartsWith("kirin") ? "c/sasu/a/Kirin.png" :
                    group.StartsWith("win2") ? "c/dan/a/win2.png" : "s/0/d0.png";
                var original = texture(Path.Combine(Content, "vfs", sourcePath));
                int w, h, columns;
                var parts = new List<Part>();
                if (group.EndsWith("-page"))
                {
                    var source = manifest["sources"].Single(x => (string)x["path"] == sourcePath);
                    w = (int)source["frameWidth"]; h = (int)source["frameHeight"]; columns = (int)source["columns"];
                    var chunk = source["chunks"].Single(x => pic >= (int)x["firstLocalPic"] && pic < (int)x["firstLocalPic"] + (int)x["count"]);
                    var page = manifest["pages"].Single(x => (string)x["id"] == (string)chunk["page"]);
                    var rect = chunk["rects"][pic - (int)chunk["firstLocalPic"]];
                    parts.Add(PagePart(texture(Path.Combine(Content, "derived_sprite_pages", (string)page["path"])),
                        chunk, pic, new Vector2(w, h)));
                }
                else
                {
                    string key = group.StartsWith("kirin") ? "kirin" : "d0";
                    var mapping = JObject.Parse(File.ReadAllText(Path.Combine(Evidence, "cpu/patches", key, "mapping.json")));
                    var geometry = mapping["sourceGeometry"];
                    w = (int)geometry["frameWidth"]; h = (int)geometry["frameHeight"]; columns = (int)geometry["columns"];
                    var frame = mapping["frames"][pic];
                    var baseTexture = texture(Path.Combine(Evidence, "cpu/prototypes", key + "-base.png"));
                    Assert.That((bool)frame["emptyPatch"], Is.False);
                    var r = frame["rect"];
                    int x = (int)r[0], y = h - (int)r[1] - (int)r[3], pw = (int)r[2], ph = (int)r[3];
                    foreach (var area in new[] { new Rect(0,0,x,h), new Rect(x+pw,0,w-x-pw,h),
                        new Rect(x,0,pw,y), new Rect(x,y+ph,pw,h-y-ph) })
                        if (area.width > 0 && area.height > 0)
                            parts.Add(new Part { Texture = baseTexture, Source = area, Destination = area });
                    parts.Add(new Part { Texture = texture(Path.Combine(Evidence, "cpu/patches", key, "patch-" + pic.ToString("000") + ".png")),
                        Source = new Rect(0, 0, pw, ph), Destination = new Rect(x, y, pw, ph) });
                }
                var canvas = new Vector2(w, h);
                var sourcePart = Whole(original, new Rect(pic % columns * (w + 1), original.height - (pic / columns + 1) * (h + 1) + 1, w, h), canvas);
                int width = (int)target["width"], height = (int)target["height"];
                float phase = (float)target["phase"], scale = (float)target["scale"], alpha = (float)target["alpha"];
                bool flip = (bool)target["flip"];
                var a = Render(canvas, new[] { sourcePart }, flip, alpha, phase, width, height, scale);
                var b = Render(canvas, parts, flip, alpha, phase, width, height, scale);
                var row = MeasureMobile(a, b, (string)target["sample"], group, pic, width, height, scale, phase, flip, alpha);
                Assert.That(row.visiblePixels, Is.GreaterThan(0), "Empty selected capture " + label);
                rows.Add(row);
                SaveMobilePair(label, a, b, width, height, true);
                files.Add(new { label, row, original = label + "-original.png", derived = label + "-derived.png", difference = label + "-difference.png" });
            }
            SaveJson("summary.json", new { status = "SELECTED_CAPTURE_COMPLETE_REQUIRES_VISUAL_REVIEW", pipeline = DetectPipeline(),
                gpu = SystemInfo.graphicsDeviceName, graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                cases = rows.Count, changedCases = rows.Count(x => x.changedPixels != 0), files,
                note = "Original31 selections from previous scan/motion; exact same Shader/Point/Gamma. Not mobile hardware or gameplay animation." });
            Assert.That(rows.Count, Is.EqualTo(request["cases"].Count()));
        }

        private static MobileRow MeasureMobile(Color32[] a, Color32[] b, string name, string group, int pic,
            int width, int height, float scale, float phase, bool flip, float alpha)
        {
            var row = new MobileRow { sample = name, group = group, localPic = pic, width = width, height = height,
                scale = scale, phase = phase, flip = flip, alpha = alpha };
            long sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                int dr = Math.Abs(a[i].r - b[i].r), dg = Math.Abs(a[i].g - b[i].g), db = Math.Abs(a[i].b - b[i].b);
                int delta = Math.Max(Math.Max(dr, dg), Math.Max(db, Math.Abs(a[i].a - b[i].a)));
                if (delta > 0) row.changedPixels++;
                if (delta > 16) row.highContrastPixels++;
                if (a[i].r != 32 || a[i].g != 48 || a[i].b != 64 ||
                    b[i].r != 32 || b[i].g != 48 || b[i].b != 64) row.visiblePixels++;
                row.maxAbsDiff = Math.Max(row.maxAbsDiff, delta);
                sum += dr + dg + db;
            }
            row.meanRgbDiff = sum / (double)(a.Length * 3);
            row.changedVisibleRatio = row.visiblePixels == 0 ? 0 : row.changedPixels / (double)row.visiblePixels;
            return row;
        }

        private void SaveMobilePair(string prefix, Color32[] a, Color32[] b, int width, int height, bool difference)
        {
            SaveMobileImage(prefix + "-original.png", a, width, height);
            SaveMobileImage(prefix + "-derived.png", b, width, height);
            if (!difference) return;
            var pixels = new Color32[a.Length];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = new Color32((byte)Math.Min(255, Math.Abs(a[i].r - b[i].r) * 8),
                    (byte)Math.Min(255, Math.Abs(a[i].g - b[i].g) * 8),
                    (byte)Math.Min(255, Math.Abs(a[i].b - b[i].b) * 8), 255);
            SaveMobileImage(prefix + "-difference.png", pixels, width, height);
        }

        private void SaveMobileImage(string name, Color32[] pixels, int width, int height)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
            try { texture.SetPixels32(pixels); texture.Apply(); Save(name, texture.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static Color32[] ReadMobileCommandBuffer(int width, int height, Action<CommandBuffer> enqueue)
        {
            var target = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            target.Create();
            var cb = new CommandBuffer { name = "M12 Mobile1080p diagnostic" };
            var previous = RenderTexture.active;
            Texture2D readback = null;
            try
            {
                cb.SetRenderTarget(target);
                cb.SetViewport(new Rect(0, 0, width, height));
                cb.SetViewProjectionMatrices(Matrix4x4.Translate(new Vector3(-width * .5f, -height * .5f, 0)),
                    GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-width * .5f, width * .5f, -height * .5f, height * .5f, -10, 10), true));
                enqueue(cb);
                Graphics.ExecuteCommandBuffer(cb);
                RenderTexture.active = target;
                readback = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
                readback.ReadPixels(new Rect(0, 0, width, height), 0, 0, false);
                return readback.GetPixels32();
            }
            finally
            {
                RenderTexture.active = previous;
                cb.Release();
                if (readback != null) UnityEngine.Object.DestroyImmediate(readback);
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private void SaveBoard(string name, Color32[] original, Color32[] derived)
        {
            var pixels = new Color32[Size * Size * 3];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    int i = y * Size + x, row = y * Size * 3;
                    pixels[row + x] = original[i];
                    pixels[row + Size + x] = derived[i];
                    pixels[row + Size * 2 + x] = new Color32(
                        (byte)Math.Min(255,Math.Abs(original[i].r-derived[i].r)*8),
                        (byte)Math.Min(255,Math.Abs(original[i].g-derived[i].g)*8),
                        (byte)Math.Min(255,Math.Abs(original[i].b-derived[i].b)*8),255);
                }
            var texture = new Texture2D(Size * 3, Size, TextureFormat.RGBA32, false, true);
            try { texture.SetPixels32(pixels); texture.Apply(); Save(name, texture.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
    }
}
#endif
