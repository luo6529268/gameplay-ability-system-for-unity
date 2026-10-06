#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.IO;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28SpriteRedLineEditorTests
    {
        private const string SourcePath = "Assets/NTSD/Content/LoganRuntime/vfs/c/nar/nar.png";
        private const string OutputRoot = "artifacts/diagnostics/NTSD28-BATTLE-SPRITE-RED-LINE-20261006/";

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void NarutoStandingSourceRectExcludesAuthoredGutter(int pic)
        {
            BMPLoader.BmpData data = BMPLoader.LoadBmpData(SourcePath);
            Rect rect = SourceRect(data, pic);
            Assert.That(rect.size, Is.EqualTo(new Vector2(79, 79)));
            int gutter = 0;
            for (int y = (int)rect.yMin; y < rect.yMax; y++)
                for (int x = (int)rect.xMin; x < rect.xMax; x++)
                    if (IsGutter(data.Pixels[y * data.Width + x])) gutter++;
            Assert.That(gutter, Is.Zero, "Authored standing crop contains the source grid.");
            Assert.That(IsGutter(data.Pixels[((int)rect.yMin - 1) * data.Width + (int)rect.xMin]), Is.True);
        }

        [TestCase(0, false, false)]
        [TestCase(1, false, false)]
        [TestCase(2, false, false)]
        [TestCase(0, true, false)]
        [TestCase(2, true, true)]
        public void NativeStandingGpuDoesNotSampleNeighbouringGutter(int mode, bool flipX, bool flipY)
        {
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(GraphicsDeviceType.Null));
            BMPLoader.BmpData data = BMPLoader.LoadBmpData(SourcePath);
            Rect rect = SourceRect(data, 0);
            var source = new Texture2D(data.Width, data.Height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            Texture texture = source;
            Material material = null;
            var target = new RenderTexture(128, 160, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(128, 160, TextureFormat.RGBA32, false, true);
            var commands = new CommandBuffer();
            RenderTexture previous = RenderTexture.active;
            using var backend = new BattleDynamicMeshBackend();
            try
            {
                source.SetPixels(data.Pixels);
                source.Apply(false, false);
                if (mode != 0)
                {
                    var pagePixels = new Color32[2048 * 2048];
                    for (int y = 0; y < data.Height; y++)
                        for (int x = 0; x < data.Width; x++)
                            pagePixels[(y + 4) * 2048 + x + 4] = data.Pixels[y * data.Width + x];
                    rect.position += new Vector2(4, 4);
                    if (mode == 1)
                    {
                        var page = new Texture2D(2048, 2048, TextureFormat.RGBA32, false)
                        { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                        texture = page;
                        page.SetPixels32(pagePixels);
                        page.Apply(false, false);
                    }
                    else
                    {
                        Assert.That(SystemInfo.supports2DArrayTextures, Is.True);
                        var array = new Texture2DArray(2048, 2048, 1, TextureFormat.RGBA32, false)
                        { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                        texture = array;
                        array.SetPixels32(pagePixels, 0);
                        array.Apply(false, false);
                    }
                }
                Shader shader = Shader.Find(mode == 2 ? "NTSD/BattleCentralTransparentArray" : "NTSD/BattleCentralTransparent");
                Assert.That(shader, Is.Not.Null);
                material = new Material(shader);
                material.SetTexture(mode == 2 ? "_MainTexArray" : "_MainTex", texture);
                material.SetColor("_Color", Color.white);
                Rect uv = new Rect(rect.x / texture.width, rect.y / texture.height,
                    rect.width / texture.width, rect.height / texture.height);
                var resolver = new Resolver(new BattleCentralResolvedResource(texture, material, uv,
                    rect.size, new Vector2(0.5f, 0), new Color32(255, 255, 255, 255),
                    bindingMode: mode == 2 ? BattleSpriteCentralBindingMode.AtlasTextureArray :
                        mode == 1 ? BattleSpriteCentralBindingMode.AtlasPageTexture2D : BattleSpriteCentralBindingMode.SourceTexture2D));
                target.Create();
                int worstGutter = 0;
                for (int offset = 0; offset <= 20; offset++)
                {
                    var frame = new BattlePresentationFrame();
                    frame.Reset(offset + 1);
                    frame.AddCommand(new BattleRenderCommand(BattleRenderCommandType.Entity,
                        RuntimeEntityHandle.Invalid, 0, 2, 0, 0, 0, 0, 0, 0,
                        new Vector3(64 * NTSDRenderSpace.UnitsPerPixelX,
                            (16 + offset * 0.05f) * NTSDRenderSpace.UnitsPerPixelY, 0),
                        rect.size, new Vector2(0.5f, 0), uv,
                        new BattleSpriteRenderState(new Color32(255, 255, 255, 255), flipX, flipY,
                            SpriteMaskInteraction.None, BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha), default));
                    backend.Build(frame, resolver);
                    commands.Clear();
                    commands.SetRenderTarget(target);
                    commands.ClearRenderTarget(false, true, new Color32(32, 64, 96, 255));
                    commands.SetViewProjectionMatrices(Matrix4x4.identity,
                        Matrix4x4.Ortho(0, 128 * NTSDRenderSpace.UnitsPerPixelX,
                            0, 160 * NTSDRenderSpace.UnitsPerPixelY, -1, 1));
                    commands.DrawMesh(backend.GetChunkMesh(0), Matrix4x4.identity, material, 0, 0);
                    Graphics.ExecuteCommandBuffer(commands);
                    RenderTexture.active = target;
                    readback.ReadPixels(new Rect(0, 0, 128, 160), 0, 0);
                    readback.Apply();
                    Color32[] pixels = readback.GetPixels32();
                    int gutter = 0;
                    int content = 0;
                    foreach (Color32 pixel in pixels)
                    {
                        if (IsGutter(pixel)) gutter++;
                        if (pixel.r > 200 && pixel.g > 180 && pixel.b < 80) content++;
                    }
                    Assert.That(content, Is.GreaterThan(20), "GPU did not draw Naruto; cannot accept an empty output.");
                    worstGutter = Math.Max(worstGutter, gutter);
                    if (offset == 0 || offset == 10 || gutter > 0)
                        SavePng("gpu-fixed-mode" + mode + "-flip" + flipX + flipY + "-offset" + offset + "-" + Guid.NewGuid().ToString("N") + ".png", readback.EncodeToPNG());
                }
                TestContext.WriteLine("GPU mode=" + mode + " worst gutter=" + worstGutter);
                Assert.That(worstGutter, Is.Zero, "GPU sampled the red grid outside the declared crop.");
            }
            finally
            {
                RenderTexture.active = previous;
                commands.Release();
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(readback);
                UnityEngine.Object.DestroyImmediate(material);
                if (texture != source) UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(source);
            }
        }

        private static Rect SourceRect(BMPLoader.BmpData data, int pic)
        {
            return CharacterAnimtorManager.BuildIndexedSpriteRects(
                new SpriteFileInfo(SourcePath, 0, 199, 79, 79, 10, 20), data.Width, data.Height, 20, 10)[pic].Value;
        }

        [Test]
        public void SingleTexelCropPreservesAuthoredRedAndPartialAlpha()
        {
            var texture = new Texture2D(3, 3, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var material = new Material(Shader.Find("NTSD/BattleCentralTransparent"));
            var target = new RenderTexture(128, 32, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(128, 32, TextureFormat.RGBA32, false, true);
            var buffer = new CommandBuffer();
            RenderTexture previous = RenderTexture.active;
            using var backend = new BattleDynamicMeshBackend();
            try
            {
                var pixels = new Color32[9];
                for (int index = 0; index < pixels.Length; index++) pixels[index] = new Color32(0, 255, 0, 255);
                pixels[4] = new Color32(201, 47, 0, 128);
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
                material.mainTexture = texture;
                material.color = Color.white;
                Rect uv = new Rect(1f / 3f, 1f / 3f, 1f / 3f, 1f / 3f);
                var resource = new BattleCentralResolvedResource(texture, material, uv,
                    Vector2.one, new Vector2(0.5f, 0), new Color32(255, 255, 255, 255));
                var frame = new BattlePresentationFrame();
                frame.Reset(1);
                frame.AddCommand(new BattleRenderCommand(BattleRenderCommandType.Entity,
                    RuntimeEntityHandle.Invalid, 0, 2, 0, 0, 0, 0, 0, 0,
                    new Vector3(64 * NTSDRenderSpace.UnitsPerPixelX, 16 * NTSDRenderSpace.UnitsPerPixelY, 0),
                    Vector2.one, new Vector2(0.5f, 0), uv, true, default));
                backend.Build(frame, new Resolver(resource));
                target.Create();
                buffer.SetRenderTarget(target);
                buffer.ClearRenderTarget(false, true, new Color32(32, 64, 96, 255));
                buffer.SetViewProjectionMatrices(Matrix4x4.identity,
                    Matrix4x4.Ortho(0, 128 * NTSDRenderSpace.UnitsPerPixelX,
                        0, 32 * NTSDRenderSpace.UnitsPerPixelY, -1, 1));
                buffer.DrawMesh(backend.GetChunkMesh(0), Matrix4x4.identity, material, 0, 0);
                Graphics.ExecuteCommandBuffer(buffer);
                RenderTexture.active = target;
                readback.ReadPixels(new Rect(0, 0, 128, 32), 0, 0);
                readback.Apply();
                Color32 actual = readback.GetPixels32()[16 * 128 + 64];
                float alpha = 128f / 255f;
                Assert.That(actual.r, Is.EqualTo(Mathf.RoundToInt(201 * alpha + 32 * (1 - alpha))).Within(2));
                Assert.That(actual.g, Is.EqualTo(Mathf.RoundToInt(47 * alpha + 64 * (1 - alpha))).Within(2));
                Assert.That(actual.b, Is.EqualTo(Mathf.RoundToInt(96 * (1 - alpha))).Within(2));
            }
            finally
            {
                RenderTexture.active = previous;
                buffer.Release();
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(readback);
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static bool IsGutter(Color32 pixel)
        {
            return pixel.a > 240 && Math.Abs(pixel.r - 201) <= 4 &&
                Math.Abs(pixel.g - 47) <= 4 && pixel.b < 4;
        }

        private static void SavePng(string name, byte[] bytes)
        {
            using var stream = new FileStream(OutputRoot + name, FileMode.CreateNew, FileAccess.Write);
            stream.Write(bytes, 0, bytes.Length);
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            private readonly BattleCentralResolvedResource resource;
            internal Resolver(BattleCentralResolvedResource resource) { this.resource = resource; }
            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command,
                out BattleCentralResolvedResource resolved)
            {
                resolved = resource;
                return BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif
