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
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    // Alignment contract: NTSD-M12-D0-BASE-PATCH-001.
    public sealed class D0SpriteCompositeEditorTests
    {
        private const string Evidence = "artifacts/diagnostics/M12-d0-base-patch-20261013";
        private const string Content = "Assets/NTSD/Content/LoganRuntime";
        private readonly List<UnityEngine.Object> owned = new List<UnityEngine.Object>();
        private readonly HashSet<UnityEngine.Object> replacementResources = new HashSet<UnityEngine.Object>();
        private BattleSpriteCatalog original, replacement;
        private string output;
        private Material normalMaterial, spriteMaterial;
        private T Track<T>(T value) where T : UnityEngine.Object { owned.Add(value); return value; }
        private static string Hash(byte[] bytes) { using var s = SHA256.Create(); return BitConverter.ToString(s.ComputeHash(bytes)).Replace("-", "").ToLowerInvariant(); }
        private static Rect R(JToken r) => new Rect((int)r["x"], (int)r["y"], (int)r["width"], (int)r["height"]);
        private void Save(string name, object value)
        {
            byte[] bytes = value as byte[] ?? Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value, Formatting.Indented));
            using var stream = new FileStream(Path.Combine(output, name), FileMode.CreateNew);
            stream.Write(bytes, 0, bytes.Length);
        }
        [SetUp]
        public void SetUp()
        {
            Assert.That(UnityEditor.EditorApplication.isPlaying, Is.False);
            output = Path.GetFullPath(Path.Combine(Evidence, "tests", TestContext.CurrentContext.Test.MethodName + "-" + Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(output);
            normalMaterial = Track(new Material(Shader.Find("NTSD/BattleCentralTransparent")));
            spriteMaterial = Track(new Material(Shader.Find("Sprites/Default")));
        }
        [TearDown]
        public void TearDown()
        {
            CharacterAnimtorManager.DestroyStagedPresentation(null, replacementResources);
            replacementResources.Clear();
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            owned.Clear();
        }
        private Texture2D Texture(string file, string expected)
        {
            Assert.That(Hash(File.ReadAllBytes(file)), Is.EqualTo(expected));
            var data = BMPLoader.LoadVerifiedNativeImageData(Path.GetFullPath(file), expected);
            var texture = Track(new Texture2D(data.Width, data.Height, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp });
            texture.SetPixels32(data.PngPixels); texture.Apply(false, true); return texture;
        }
        private void Candidate()
        {
            var selection = JObject.Parse(File.ReadAllText(Path.Combine(Evidence, "candidate-selection.json")));
            string directory = (string)selection["directory"];
            byte[] raw = File.ReadAllBytes(Path.Combine(directory, "manifest.json"));
            Assert.That(Hash(raw), Is.EqualTo((string)selection["manifestSha256"]));
            var manifest = JObject.Parse(Encoding.UTF8.GetString(raw));
            var source = manifest["sources"].Single(s => (string)s["path"] == "s/0/d0.png");
            var chunk = source["chunks"].Single();
            var page = manifest["pages"].Single(p => (string)p["id"] == (string)chunk["page"]);
            Texture2D native = Texture(Path.Combine(Content, "vfs/s/0/d0.png"), (string)source["sha256"]);
            Texture2D packed = Texture(Path.Combine(directory, (string)page["path"]), (string)page["sha256"]);
            var patches = chunk["replacementPatches"].Select(p => p.Type == JTokenType.Null ? default :
                new BattleSpriteReplacementFrame.Patch(R(p["source"]), R(p["destination"]))).ToArray();
            var frames = BattleSpriteReplacementFrame.Create(packed, R(chunk["rects"][0]), patches, replacementResources);
            var a = new BattleSpriteCatalogBuilder(); var b = new BattleSpriteCatalogBuilder();
            for (int i = 0; i < 34; i++)
            {
                Rect nr = new Rect(i % 2 * 311, native.height - i / 2 * 126 - 125, 310, 125);
                Sprite ns = Track(Sprite.Create(native, nr, new Vector2(.5f, 0), 100f, 0, SpriteMeshType.FullRect));
                Rect pr = R(chunk["rects"][i]);
                Sprite ps = Track(Sprite.Create(packed, pr, new Vector2(.5f, 0), 100f, 0, SpriteMeshType.FullRect));
                a.Add(299, 30 + i, "native", native, nr, new Vector2(.5f, 0), ns);
                b.Add(299, 30 + i, "replacement", packed, pr, new Vector2(.5f, 0), ps);
                b.SetReplacementFrame(299, 30 + i, frames[i]);
            }
            original = a.Publish(); replacement = b.Publish();
            Save("inputs.json", new { manifestSha256 = Hash(raw), directory, sourceSha256 = (string)source["sha256"],
                pageSha256 = (string)page["sha256"], frames = frames.Length, packedBytes = (long)packed.width * packed.height * 4 });
        }
        private static Color32[] Read(Action<CommandBuffer> draws)
        {
            var rt = new RenderTexture(1920, 1080, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
                { filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
            rt.Create(); var cb = new CommandBuffer(); var previous = RenderTexture.active; Texture2D read = null;
            try
            {
                cb.SetRenderTarget(rt); cb.SetViewport(new Rect(0, 0, 1920, 1080));
                cb.SetViewProjectionMatrices(Matrix4x4.Translate(new Vector3(-960, -540, 0)),
                    GL.GetGPUProjectionMatrix(Matrix4x4.Ortho(-960, 960, -540, 540, -10, 10), true));
                cb.ClearRenderTarget(false, true, new Color(32 / 255f, 48 / 255f, 64 / 255f, 1)); draws(cb);
                Graphics.ExecuteCommandBuffer(cb); RenderTexture.active = rt;
                read = new Texture2D(1920, 1080, TextureFormat.RGBA32, false, true);
                read.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0, false); return read.GetPixels32();
            }
            finally { RenderTexture.active = previous; cb.Release(); if (read != null) UnityEngine.Object.DestroyImmediate(read); rt.Release(); UnityEngine.Object.DestroyImmediate(rt); }
        }
        private Color32[] Render(BattleSpriteCatalog catalog, int pic, bool flip, float alpha, float phase, float scale, bool legacy)
        {
            Assert.That(catalog.TryGet(299, pic, out var entry), Is.True);
            byte a = (byte)Mathf.RoundToInt(alpha * 255);
            if (legacy)
            {
                var go = new GameObject("D0 isolated legacy renderer") { hideFlags = HideFlags.HideAndDontSave };
                var module = new LF2Sprite();
                try
                {
                    var renderer = go.AddComponent<SpriteRenderer>(); renderer.sharedMaterial = spriteMaterial;
                    module.Initialize(renderer, null, 0, catalog, 299); module.ShowPic(pic); module.SwitchLR(flip ? "left" : "right");
                    renderer.color = new Color32(255, 255, 255, a);
                    go.transform.position = new Vector3(960 + phase, (1080 - entry.PixelHeight * scale) * .5f + phase, 0);
                    go.transform.localScale = new Vector3(scale * 100, scale * 100, 1);
                    return Read(cb => cb.DrawRenderer(renderer, renderer.sharedMaterial));
                }
                finally { module.Reset(); UnityEngine.Object.DestroyImmediate(go); }
            }
            var frame = new BattlePresentationFrame(); frame.Reset(1);
            frame.AddCommand(M12SpriteTrimEditorTests.Command(entry, flip, false, new Color32(255, 255, 255, a)));
            var resolver = new BattleCatalogCentralResourceResolver(); resolver.Configure(catalog, normalMaterial);
            using var backend = new BattleDynamicMeshBackend(); backend.PrepareCapacity(1); backend.Build(frame, resolver);
            Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(1));
            Assert.That(backend.GetChunkActiveQuadCount(0), Is.EqualTo(1));
            var matrix = Matrix4x4.TRS(new Vector3(960 + phase, (1080 - entry.PixelHeight * scale) * .5f + phase, 0), Quaternion.identity,
                new Vector3(scale / (NTSDRenderSpace.UnitsPerPixelX * NTSDRenderSpace.BattleVisualScale),
                    scale / (NTSDRenderSpace.UnitsPerPixelY * NTSDRenderSpace.BattleVisualScale), 1));
            return Read(cb => { for (int i = 0; i < backend.SegmentCount; i++) { var s = backend.GetSegment(i); var props = new MaterialPropertyBlock();
                props.SetTexture("_MainTex", s.Texture); cb.DrawMesh(backend.GetChunkMesh(s.ChunkIndex), matrix, s.Material, s.SubMeshIndex, 0, props); } });
        }
        private sealed class Row
        {
            public int localPic, changedPixels, visiblePixels, maxDifference; public bool flip, legacy; public float alpha, phase, scale;
        }
        private static Row Measure(Color32[] a, Color32[] b, int pic, bool flip, float alpha, float phase, float scale, bool legacy)
        {
            var row = new Row { localPic = pic, flip = flip, alpha = alpha, phase = phase, scale = scale, legacy = legacy };
            for (int i = 0; i < a.Length; i++)
            {
                int d = Math.Max(Math.Max(Math.Abs(a[i].r-b[i].r), Math.Abs(a[i].g-b[i].g)), Math.Max(Math.Abs(a[i].b-b[i].b), Math.Abs(a[i].a-b[i].a)));
                if (d != 0) row.changedPixels++; row.maxDifference = Math.Max(row.maxDifference, d);
                if (a[i].r != 32 || a[i].g != 48 || a[i].b != 64 || b[i].r != 32 || b[i].g != 48 || b[i].b != 64) row.visiblePixels++;
            }
            return row;
        }
        private void Image(string name, Color32[] pixels)
        {
            var texture = new Texture2D(1920, 1080, TextureFormat.RGBA32, false, true);
            try { texture.SetPixels32(pixels); texture.Apply(); Save(name, texture.EncodeToPNG()); }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }
        [Test]
        public void Candidate1080pAllFramesCentralAndLegacy()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) Assert.Inconclusive("Real D3D11 graphics required.");
            Candidate(); var rows = new List<Row>();
            Save("planned-outputs.json", new { files = new[] { "summary.json", "inputs.json", "planned-outputs.json" },
                patterns = new[] { "<central/legacy>-<local>-original.png", "<central/legacy>-<local>-derived.png" }, noP1 = true });
            foreach (bool legacy in new[] { false, true })
            for (int pic = 0; pic < 34; pic++)
            {
                int worst = -1; Color32[] worstA = null, worstB = null;
                foreach (bool flip in new[] { false, true })
                foreach (float alpha in legacy ? new[] { 1f, .5f } : new[] { 1f, .5f, .2f })
                foreach (float phase in legacy ? new[] { .375f } : new[] { 0f, .375f })
                {
                    var a = Render(original, 30 + pic, flip, alpha, phase, 1.40625f, legacy);
                    var b = Render(replacement, 30 + pic, flip, alpha, phase, 1.40625f, legacy);
                    var row = Measure(a, b, pic, flip, alpha, phase, 1.40625f, legacy); rows.Add(row);
                    if (row.changedPixels > worst) { worst = row.changedPixels; worstA = a; worstB = b; }
                }
                string prefix = (legacy ? "legacy-" : "central-") + pic.ToString("000");
                Image(prefix + "-original.png", worstA); Image(prefix + "-derived.png", worstB);
            }
            foreach (int pic in new[] { 1, 17, 33 }) foreach (bool flip in new[] { false, true })
            {
                var a = Render(original, 30 + pic, flip, .5f, .5f, 2f, false);
                var b = Render(replacement, 30 + pic, flip, .5f, .5f, 2f, false);
                rows.Add(Measure(a, b, pic, flip, .5f, .5f, 2f, false));
                string prefix = "stress-" + pic + "-" + flip;
                Image(prefix + "-original.png", a); Image(prefix + "-derived.png", b);
            }
            Save("summary.json", new { cases = rows.Count, changedCases = rows.Count(r => r.changedPixels > 0), rows,
                gpu = SystemInfo.graphicsDeviceName, graphicsApi = SystemInfo.graphicsDeviceType.ToString(), physicalPhone = false,
                admission = "Main visual review required; capturing is not automatic equality acceptance" });
            Assert.That(rows.Count, Is.EqualTo(550)); Assert.That(rows.Sum(r => (long)r.visiblePixels), Is.GreaterThan(0));
        }
        [Test]
        public void SharedMaterialKeepsThirtyFourFramesInOneBatchAndRestoresLegacyMaterial()
        {
            Candidate(); var frame = new BattlePresentationFrame(); frame.Reset(1);
            for (int i = 0; i < 34; i++) { replacement.TryGet(299, 30 + i, out var entry);
                Assert.That(entry.WithCentralBinding(entry.CentralBinding).ReplacementFrame, Is.SameAs(entry.ReplacementFrame));
                frame.AddCommand(M12SpriteTrimEditorTests.Command(entry, false, false, new Color32(255,255,255,255))); }
            var resolver = new BattleCatalogCentralResourceResolver(); resolver.Configure(replacement, normalMaterial);
            using var backend = new BattleDynamicMeshBackend(); backend.PrepareCapacity(34); backend.Build(frame, resolver);
            Assert.That(backend.SegmentCount, Is.EqualTo(1)); Assert.That(backend.GetChunkActiveQuadCount(0), Is.EqualTo(34));
            Assert.That(backend.GetChunkMesh(0).GetVertexBufferStride(0), Is.EqualTo(44));
            for (int i = 0; i < 34; i++) Assert.That(backend.GetChunkVertexAtlasSlice(0, i * 4), Is.EqualTo(i));
            var go = Track(new GameObject("D0 reset fixture")); var renderer = go.AddComponent<SpriteRenderer>(); renderer.sharedMaterial = spriteMaterial;
            var mixedEntries = replacement.Entries.ToDictionary(p => p.Key, p => p.Value);
            original.TryGet(299, 30, out var normalEntry);
            mixedEntries.Add(new BattleSpriteKey(299, 64), new BattleSpriteEntry(new BattleSpriteKey(299, 64),
                normalEntry.SourceSheetPath, normalEntry.SharedTexture, normalEntry.PixelRect, normalEntry.Pivot, normalEntry.LegacySprite));
            var mixed = new BattleSpriteCatalog(mixedEntries);
            var module = new LF2Sprite(); module.Initialize(renderer, null, 0, mixed, 299); module.ShowPic(47);
            replacement.TryGet(299, 47, out var selected); Assert.That(renderer.sharedMaterial, Is.SameAs(selected.ReplacementFrame.LegacyMaterial));
            module.ShowPic(64); Assert.That(renderer.sharedMaterial, Is.SameAs(spriteMaterial));
            Assert.That(renderer.sprite, Is.SameAs(normalEntry.LegacySprite));
            module.ShowPic(47); module.ShowPic(999); Assert.That(renderer.sharedMaterial, Is.SameAs(spriteMaterial));
            module.ShowPic(63); module.Reset(); Assert.That(renderer.sharedMaterial, Is.SameAs(spriteMaterial));
        }
        [Test]
        public void TransparentPatchErasesBaseAndWrongNoPatchControlIsDetected()
        {
            var texture = Track(new Texture2D(8,8,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point });
            var pixels = new Color32[64]; for(int y=1;y<5;y++) for(int x=1;x<5;x++) pixels[y*8+x]=new Color32(255,255,255,255);
            texture.SetPixels32(pixels);texture.Apply(false,true);
            var patches = new[] { default(BattleSpriteReplacementFrame.Patch), new BattleSpriteReplacementFrame.Patch(new Rect(6,1,1,1),new Rect(1,1,1,1)) };
            var frames = BattleSpriteReplacementFrame.Create(texture,new Rect(1,1,4,4),patches,replacementResources);
            var builder = new BattleSpriteCatalogBuilder();
            for(int i=0;i<2;i++) { var sprite=Track(Sprite.Create(texture,new Rect(1,1,4,4),new Vector2(.5f,0),100));
                builder.Add(299,30+i,"erase",texture,sprite.rect,new Vector2(.5f,0),sprite);builder.SetReplacementFrame(299,30+i,frames[i]); }
            var catalog=builder.Publish();var a=Render(catalog,30,false,1,0,2,false);var b=Render(catalog,31,false,1,0,2,false);
            var row=Measure(a,b,1,false,1,0,2,false);Assert.That(row.changedPixels,Is.EqualTo(4));
            for (int i = 0; i < a.Length; i++)
                if (!a[i].Equals(b[i])) Assert.That(b[i], Is.EqualTo(new Color32(32,48,64,255)));
            Save("negative-control.json",row);
        }
        [UnityTest]
        public IEnumerator DeployedD0LoadsOnePageAndOwnsMaterials()
        {
            return UniTask.ToCoroutine(async () =>
            {
                var candidate=LoganVisualContentCandidate.Capture(BattleContentSource.ForLoganRuntime(Path.GetFullPath(Content)),NTSD.App.ProjectBattleModeConfig.LoadDefault().Capture());
                var configs=candidate.CopyCharacterConfigs();var config=configs[299];var file=config.characterData.files.Single(f=>f.filePath.Replace((char)92,'/').EndsWith("/s/0/d0.png"));
                Assert.That(file.startFrame,Is.EqualTo(30));Assert.That(file.endFrame,Is.EqualTo(63));
                var files=new List<(int characterId,SpriteFileInfo fileInfo,HashSet<int> ownedEffectivePics)> { (299,file,new HashSet<int>(Enumerable.Range(30,34))) };
                const BindingFlags hidden=BindingFlags.Static|BindingFlags.NonPublic;
                object Call(string method,params object[] args)=>typeof(CharacterAnimtorManager).GetMethod(method,hidden).Invoke(null,args);
                var groups=(IList)Call("BuildNativeBodySheetGroupsWithReuse",files,candidate,true);Assert.That(groups.Count,Is.EqualTo(1));
                var byId=new Dictionary<int,List<Sprite>> { [299]=Enumerable.Repeat<Sprite>(null,64).ToList() };var sprites=new HashSet<Sprite>();var textures=new HashSet<Texture2D>();var atlas=new List<BattleAtlasSourcePixels>();var paths=new Dictionary<Sprite,string>();
                var type=typeof(CharacterAnimtorManager).GetNestedType("NativePrewarmOperation",BindingFlags.NonPublic);object operation=Activator.CreateInstance(type,true);const BindingFlags fields=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
                var metrics=new CharacterAnimtorManager.NativeDecodeAdmission(134217728,268435456);type.GetField("Candidate",fields).SetValue(operation,candidate);type.GetField("ReuseNativeBodySheets",fields).SetValue(operation,true);type.GetField("Metrics",fields).SetValue(operation,metrics);
                using var cpu=new SemaphoreSlim(1,1);using var upload=new SemaphoreSlim(1,1);
                try
                {
                    await cpu.WaitAsync();var result=await (UniTask<(int created,int failed)>)Call("ProcessNativeBodySheetGroupAsync",groups[0],byId,sprites,textures,atlas,new Dictionary<string,Sprite>(),paths,null,cpu,upload,candidate.Catalog.Source,new Func<bool>(()=>true),operation,null);
                    Assert.That(result.created,Is.EqualTo(34));Assert.That(result.failed,Is.Zero);
                    var catalog=CharacterAnimtorManager.BuildBattleSpriteCatalogWithReplacements(new Dictionary<int,LF2CharacterDataWrapper>{{299,config}},byId,paths,candidate.DerivedPages,replacementResources);
                    Assert.That(catalog.Count,Is.EqualTo(34));Assert.That(textures.Count,Is.EqualTo(1));Assert.That(replacementResources.Count,Is.EqualTo(35));Assert.That(atlas.Count,Is.Zero);
                    for(int pic=30;pic<64;pic++){Assert.That(catalog.TryGet(299,pic,out var entry),Is.True);Assert.That(entry.PixelWidth,Is.EqualTo(310));Assert.That(entry.PixelHeight,Is.EqualTo(125));Assert.That(entry.ReplacementFrame.Index,Is.EqualTo(pic-30));}
                    Assert.That(metrics.PendingPixels+metrics.RetainedPixels+metrics.Active,Is.Zero);candidate.AssertInputsCurrent();
                    Save("loaded.json",new { frames=catalog.Count,textures=textures.Count,materials=replacementResources.Count,rgbaBytes=textures.Sum(t=>(long)t.width*t.height*4) });
                }
                finally { CharacterAnimtorManager.DestroyStagedPresentation(sprites,textures); }
            });
        }
    }
}
#endif
