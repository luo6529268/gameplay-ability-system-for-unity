#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NTSD.Animation;
using NTSD.DatParser;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace NTSD.Test
{
    public sealed class NTSD28B11PngSheetAlphaEditorTests
    {
        private const string RuntimeRoot = @"J:\QQFile\NTSD2.8.3.3 zip\NTSD2.8.3.3\NTSD 2.8-Logan\resources\runtime";
        private static BattleContentSource NativeSource => BattleContentSource.ForLoganRuntime(RuntimeRoot);
        private static string FixturePath => Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Temp/NTSD28PngAlpha/fixture.dat");

        [OneTimeSetUp]
        public void CreateOwnedFixture()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FixturePath));
            var texture = new Texture2D(6, 2, TextureFormat.RGBA32, false);
            try
            {
                texture.SetPixels32(new[]
                {
                    new Color32(0,255,0,255), new Color32(0,255,0,255), new Color32(0,255,0,255),
                    new Color32(0,255,0,255), new Color32(0,255,0,255), new Color32(0,255,0,255),
                    new Color32(0,255,0,128), new Color32(0,255,0,241), new Color32(0,255,0,238),
                    new Color32(0,0,0,128), new Color32(160,80,40,0), new Color32(0,255,0,255)
                });
                texture.Apply();
                // A misleading extension verifies format identity comes from decoded bytes.
                File.WriteAllBytes(FixturePath, texture.EncodeToPNG());
            }
            finally { UnityEngine.Object.DestroyImmediate(texture); }
        }

        private static BMPLoader.BmpData LoadWorker(string path) => Task.Run(() => BMPLoader.LoadBmpData(path)).GetAwaiter().GetResult();

        private static Color32[] Prepare(BMPLoader.BmpData data, BattleContentSource source)
        {
            var method = typeof(CharacterAnimtorManager).GetMethod("PrepareBattleSheetPixels", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null, "Production sheet alpha policy is missing.");
            return (Color32[])method.Invoke(null, new object[] { data, source });
        }

        [Test]
        public void DecoderFormat_IsByteBased_OnWorkerAndMainThread()
        {
            var property = typeof(BMPLoader.BmpData).GetProperty("IsPng");
            Assert.That(property, Is.Not.Null, "Decode format must survive until the sheet consumer.");
            Assert.That(property.GetValue(LoadWorker(FixturePath)), Is.EqualTo(true));
            Assert.That(property.GetValue(BMPLoader.LoadBmpData(FixturePath)), Is.EqualTo(true));
            Assert.That(property.GetValue(new BMPLoader.BmpData()), Is.EqualTo(false));
        }

        [Test]
        public void NativePng_PreservesAllRgba_IncludingVisibleGreenAndBlack()
        {
            var data = LoadWorker(FixturePath);
            Color32[] actual = Prepare(data, NativeSource);
            for (int i = 0; i < actual.Length; i++)
                Assert.That(actual[i], Is.EqualTo((Color32)data.Pixels[i]), "pixel " + i);
        }

        [Test]
        public void LegacyPng_AndNonPngData_KeepExistingColorKeyAndGridRules()
        {
            var data = LoadWorker(FixturePath);
            var expected = new Color32[data.Pixels.Length];
            for (int i = 0; i < expected.Length; i++) expected[i] = data.Pixels[i];
            RuntimeSpriteProcessor.ProcessSheetPixelsFast(expected);
            RuntimeSpriteProcessor.ClearDetectedGridSeparatorAlpha(expected, data.Width, data.Height);
            Assert.That(Prepare(data, null), Is.EqualTo(expected));
            var bmp = new BMPLoader.BmpData { Width = data.Width, Height = data.Height, Pixels = data.Pixels };
            Assert.That(Prepare(bmp, NativeSource), Is.EqualTo(expected));
            var isolated = new BMPLoader.BmpData { Width = 2, Height = 1, Pixels = new[] { new Color(0,0,0,0.5f), new Color(1,0,0,0.5f) } };
            Assert.That(Prepare(isolated, NativeSource), Is.EqualTo(new[] { new Color32(0,0,0,0), new Color32(255,0,0,255) }));
        }

        [Test]
        public void ProductionStaging_HasExplicitSource_WithoutManagerLifecycle()
        {
            Assert.That(typeof(CharacterAnimtorManager).GetMethod("ProcessAndCreateSpritesAsync", BindingFlags.NonPublic | BindingFlags.Static), Is.Not.Null);
            Assert.That(typeof(CharacterAnimtorManager).GetMethod("LoadCharacterSpritesAsync", new[] { typeof(Action<string>) }), Is.Not.Null);
        }

        [UnityTest]
        public IEnumerator Fixture_ProductionStageCatalogAndGpu_PreserveNativeAlpha()
        {
            return UniTask.ToCoroutine(async () =>
                await VerifyProductionSheet(new SpriteFileInfo(FixturePath, 0, 0, 5, 1, 1, 1), true));
        }

        [UnityTest]
        public IEnumerator FormalNar_ProductionStageCatalogAndGpu_PreserveNativeAlpha()
        {
            return UniTask.ToCoroutine(async () =>
            {
                string path = NativeSource.ResolveDatPath("c/nar/nar.dat");
                var dat = new Lf2DatParserV2().ParseLoganContent(File.ReadAllText(path), path);
                var files = CharacterAnimtorManager.BuildSpriteFilesForSource(dat, Path.GetDirectoryName(path), NativeSource);
                await VerifyProductionSheet(files[0], false);
            });
        }

        private static async UniTask VerifyProductionSheet(SpriteFileInfo file, bool allFixturePixels)
        {
            var method = typeof(CharacterAnimtorManager).GetMethod("ProcessAndCreateSpritesAsync", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Real per-sheet staging must accept explicit source.");
            var raw = await UniTask.RunOnThreadPool(() => BMPLoader.LoadBmpData(file.filePath));
            var owned = new HashSet<int>();
            for (int i = file.startFrame; i <= file.endFrame; i++) owned.Add(i);
            var staged = new Dictionary<int, List<Sprite>> { [56] = new List<Sprite>(new Sprite[file.endFrame + 1]) };
            var sprites = new HashSet<Sprite>();
            var textures = new HashSet<Texture2D>();
            var atlas = new List<BattleAtlasSourcePixels>();
            using (var cpu = new SemaphoreSlim(0, 1))
            using (var upload = new SemaphoreSlim(1, 1))
            try
            {
                var task = (UniTask<int>)method.Invoke(null, new object[] { 56, file, owned, staged, sprites, textures, atlas, null, cpu, upload, NativeSource });
                Assert.That(await task, Is.GreaterThan(0));
                Assert.That(cpu.CurrentCount, Is.EqualTo(1));
                Assert.That(upload.CurrentCount, Is.EqualTo(1));
                Assert.That(atlas.Count, Is.EqualTo(1));
                for (int i = 0; i < raw.Pixels.Length; i++)
                    Assert.That(atlas[0].Pixels[i], Is.EqualTo((Color32)raw.Pixels[i]), "Atlas input pixel " + i);
                var data = new LF2CharacterData();
                data.files.Add(file);
                var catalog = CharacterAnimtorManager.BuildBattleSpriteCatalog(new Dictionary<int, LF2CharacterDataWrapper> { [56] = new LF2CharacterDataWrapper(56, data) }, staged);
                Assert.That(QualitySettings.activeColorSpace, Is.EqualTo(ColorSpace.Gamma), "This GPU certificate covers the current Gamma project.");
                Assert.That(GraphicsSettings.currentRenderPipeline.GetType().Name, Does.Contain("Universal"));
                int measured = 0;
                for (int pic = file.startFrame; pic <= file.endFrame && measured < 5; pic++)
                {
                    if (!catalog.TryGet(56, pic, out BattleSpriteEntry entry)) continue;
                    Rect rect = entry.PixelRect;
                    for (int y = (int)rect.y; y < rect.yMax && measured < 5; y++)
                    for (int x = (int)rect.x; x < rect.xMax && measured < 5; x++)
                    {
                        Color32 original = raw.Pixels[y * raw.Width + x];
                        if (!allFixturePixels && (original.a == 0 || original.a == 255)) continue;
                        AssertGpuPixel(entry.SharedTexture, x, y, original);
                        measured++;
                    }
                }
                Assert.That(measured, Is.EqualTo(5), "Need five actual visible source samples.");
                TestContext.WriteLine("Production sheet/catalog/GPU matched: " + file.filePath + " samples=" + measured + " backend=" + SystemInfo.graphicsDeviceType);
            }
            finally
            {
                foreach (var sprite in sprites) UnityEngine.Object.DestroyImmediate(sprite);
                foreach (var texture in textures) UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void AssertGpuPixel(Texture2D texture, int x, int y, Color32 original)
        {
            var shader = Shader.Find("NTSD/BattleCentralTransparent");
            Assert.That(shader, Is.Not.Null);
            var material = new Material(shader);
            var mesh = new Mesh();
            var target = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
            var command = new CommandBuffer();
            RenderTexture previous = RenderTexture.active;
            try
            {
                material.mainTexture = texture;
                material.color = Color.white;
                mesh.vertices = new[] { new Vector3(-1,-1,0), new Vector3(1,-1,0), new Vector3(1,1,0), new Vector3(-1,1,0) };
                var uv = new Vector2((x + 0.5f) / texture.width, (y + 0.5f) / texture.height);
                mesh.uv = new[] { uv, uv, uv, uv };
                mesh.colors32 = new[] { new Color32(255,255,255,255), new Color32(255,255,255,255), new Color32(255,255,255,255), new Color32(255,255,255,255) };
                mesh.triangles = new[] { 0,1,2,0,2,3 };
                target.Create();
                Color background = new Color32(32, 64, 96, 255);
                command.SetRenderTarget(target);
                command.ClearRenderTarget(false, true, background);
                command.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
                command.DrawMesh(mesh, Matrix4x4.identity, material);
                Graphics.ExecuteCommandBuffer(command);
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0,0,1,1), 0, 0);
                readback.Apply();
                Color32 actual = readback.GetPixels32()[0];
                float alpha = original.a / 255f;
                Assert.That((int)actual.r, Is.EqualTo(Mathf.RoundToInt(original.r * alpha + 32 * (1-alpha))).Within(2), "GPU red");
                Assert.That((int)actual.g, Is.EqualTo(Mathf.RoundToInt(original.g * alpha + 64 * (1-alpha))).Within(2), "GPU green");
                Assert.That((int)actual.b, Is.EqualTo(Mathf.RoundToInt(original.b * alpha + 96 * (1-alpha))).Within(2), "GPU blue");
                Assert.That((int)actual.a, Is.EqualTo(255).Within(1), "GPU alpha");
            }
            finally
            {
                RenderTexture.active = previous;
                command.Release();
                target.Release();
                UnityEngine.Object.DestroyImmediate(readback);
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(mesh);
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }
}
#endif
