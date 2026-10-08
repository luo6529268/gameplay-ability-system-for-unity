#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
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
    public sealed class BattleCentralSameSamplePixelEditorTests
    {
        private const int Width = 96;
        private const int Height = 96;
        private const string OutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH17-SAME-SAMPLE-PIXELS-20261007";
        private static readonly Color32 Background = new Color32(19, 29, 43, 255);

        [Serializable]
        private sealed class Result
        {
            public int bindingMode;
            public int drawMode;
            public int comparedSamples;
            public int comparedPixels;
            public int maximumChannelError;
            public int reverseOrderDifferentPixels;
            public int minimumContentPixels = int.MaxValue;
            public string status;
        }

        [TestCase(0, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(1, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(2, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(0, BattleCentralDrawMode.StrictOrderedDraw)]
        [TestCase(1, BattleCentralDrawMode.StrictOrderedDraw)]
        [TestCase(2, BattleCentralDrawMode.StrictOrderedDraw)]
        public void InterpolatedOverlap_MatchesIndependentQuadOracle(int mode, BattleCentralDrawMode drawMode)
        {
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(GraphicsDeviceType.Null));
            if (mode == 2)
                Assert.That(SystemInfo.supports2DArrayTextures, Is.True);
            string runRoot = Path.Combine(OutputRoot, "run-" + mode + "-" + (int)drawMode + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(runRoot);
            Texture texture = null;
            Material material = null;
            var target = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            var readback = new Texture2D(Width, Height, TextureFormat.RGBA32, false, true);
            var buffer = new CommandBuffer();
            RenderTexture previousTarget = RenderTexture.active;
            var result = new Result { bindingMode = mode, drawMode = (int)drawMode };
            using var backend = new BattleDynamicMeshBackend();
            try
            {
                texture = MakeTexture(mode);
                Shader shader = Shader.Find(mode == 2 ? "NTSD/BattleCentralTransparentArray" : "NTSD/BattleCentralTransparent");
                Assert.That(shader, Is.Not.Null);
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                material.SetTexture(mode == 2 ? "_MainTexArray" : "_MainTex", texture);
                material.SetColor("_Color", Color.white);
                var resolver = new Resolver(texture, material, mode);
                target.Create();
                var published = MakePublication();
                foreach (double alpha in new[] { 0.0, 0.25, 0.5, 0.75, 1.0 })
                {
                    var captured = new BattlePresentationFrame();
                    captured.CopyFrom(published);
                    var display = new BattlePresentationDisplayMotion();
                    display.Prepare(captured, alpha, 1.5, 2.0);
                    display.ApplyToCapturedCommands(captured);
                    AssertIndependentSample(published, captured, alpha);
                    // First Build changes ranges; second exercises stable metadata. Shrink then recover below.
                    backend.Build(captured, resolver, drawMode);
                    Mesh identity = backend.GetChunkMesh(0);
                    backend.Build(captured, resolver, drawMode);
                    Compare(captured, resolver, backend, target, readback, buffer, result, runRoot, alpha, "stable");
                    var prefix = new BattlePresentationFrame();
                    prefix.Reset(captured.TickIndex);
                    prefix.AddCommand(captured.GetCommand(0));
                    prefix.AddCommand(captured.GetCommand(1));
                    backend.Build(prefix, resolver, drawMode);
                    Compare(prefix, resolver, backend, target, readback, buffer, result, runRoot, alpha, "shrink");
                    backend.Build(captured, resolver, drawMode);
                    Assert.That(backend.GetChunkMesh(0), Is.SameAs(identity));
                    Compare(captured, resolver, backend, target, readback, buffer, result, runRoot, alpha, "recover");
                }
                result.status = "PASS";
                WriteNew(Path.Combine(runRoot, "result.json"), System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(result, true)));
                TestContext.WriteLine(JsonUtility.ToJson(result));
            }
            finally
            {
                RenderTexture.active = previousTarget;
                buffer.Release();
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(readback);
                UnityEngine.Object.DestroyImmediate(material);
                UnityEngine.Object.DestroyImmediate(texture);
            }
        }

        private static void Compare(BattlePresentationFrame frame, Resolver resolver,
            BattleDynamicMeshBackend backend, RenderTexture target, Texture2D readback, CommandBuffer buffer,
            Result result, string runRoot, double alpha, string phase)
        {
            int expectedSegments = result.drawMode == (int)BattleCentralDrawMode.StrictOrderedDraw ? frame.CommandCount : frame.CommandCount / 2;
            Assert.That(backend.SegmentCount, Is.EqualTo(expectedSegments));
            Begin(buffer, target);
            for (int index = 0; index < backend.SegmentCount; index++)
            {
                BattleCentralRenderSegment segment = backend.GetSegment(index);
                Assert.That(segment.FirstCommandIndex, Is.EqualTo(result.drawMode == (int)BattleCentralDrawMode.StrictOrderedDraw ? index : index * 2));
                buffer.DrawMesh(backend.GetChunkMesh(segment.ChunkIndex), Matrix4x4.identity,
                    segment.Material, segment.SubMeshIndex, 0);
            }
            Color32[] actual = Read(buffer, target, readback);
            if (alpha == 0.5 && phase == "stable")
                WriteNew(Path.Combine(runRoot, "actual.png"), readback.EncodeToPNG());
            var meshes = new Mesh[frame.CommandCount];
            try
            {
                for (int index = 0; index < frame.CommandCount; index++)
                {
                    BattleRenderCommand command = frame.GetCommand(index);
                    resolver.Resolve(command, out BattleCentralResolvedResource resource);
                    meshes[index] = MakeIndependentQuad(command, resource);
                }
                Begin(buffer, target);
                for (int index = 0; index < meshes.Length; index++)
                    buffer.DrawMesh(meshes[index], Matrix4x4.identity, resolver.Material, 0, 0);
                Color32[] expected = Read(buffer, target, readback);
                if (alpha == 0.5 && phase == "stable")
                    WriteNew(Path.Combine(runRoot, "reference.png"), readback.EncodeToPNG());
                int content = 0;
                int maxError = 0;
                for (int index = 0; index < actual.Length; index++)
                {
                    maxError = Math.Max(maxError, Error(actual[index], expected[index]));
                    if (Error(expected[index], Background) > 4)
                        content++;
                }
                Assert.That(content, Is.GreaterThan(100), "Empty output is not acceptance.");
                Assert.That(maxError, Is.Zero, "Same-sample backend differs from independent per-quad oracle.");
                result.maximumChannelError = Math.Max(result.maximumChannelError, maxError);
                result.minimumContentPixels = Math.Min(result.minimumContentPixels, content);
                result.comparedSamples++;
                result.comparedPixels += actual.Length;
                if (alpha == 0.5 && phase == "stable")
                {
                    Begin(buffer, target);
                    for (int index = meshes.Length - 1; index >= 0; index--)
                        buffer.DrawMesh(meshes[index], Matrix4x4.identity, resolver.Material, 0, 0);
                    Color32[] reverse = Read(buffer, target, readback);
                    int different = 0;
                    for (int index = 0; index < reverse.Length; index++)
                        if (Error(reverse[index], expected[index]) > 2)
                            different++;
                    Assert.That(different, Is.GreaterThan(20), "Fixture is not sensitive to painter order.");
                    result.reverseOrderDifferentPixels = different;
                    WriteNew(Path.Combine(runRoot, "reverse-order-negative-control.png"), readback.EncodeToPNG());
                }
            }
            finally
            {
                foreach (Mesh mesh in meshes)
                    UnityEngine.Object.DestroyImmediate(mesh);
            }
        }

        private static void AssertIndependentSample(BattlePresentationFrame published, BattlePresentationFrame captured, double alpha)
        {
            double dx = (Math.Round(100 + 20 * alpha, MidpointRounding.AwayFromZero) - 120) * 1.5;
            double dy = -(Math.Round(100 + 10 * alpha, MidpointRounding.AwayFromZero) - 110) * 2
                - (Math.Round(10 * alpha, MidpointRounding.AwayFromZero) - 10);
            for (int index = 0; index < published.CommandCount; index++)
            {
                BattleRenderCommand source = published.GetCommand(index);
                BattleRenderCommand sample = captured.GetCommand(index);
                Vector3 original = Position(index);
                Assert.That(source.Position, Is.EqualTo(original), "Publication was mutated.");
                Assert.That(sample.Position.x, Is.EqualTo(original.x + (float)dx * NTSDRenderSpace.UnitsPerPixelX).Within(1e-6));
                Assert.That(sample.Position.y, Is.EqualTo(original.y + (float)dy * NTSDRenderSpace.UnitsPerPixelY).Within(1e-6));
                Assert.That(sample.FlipX, Is.EqualTo(source.FlipX));
                Assert.That(sample.FlipY, Is.EqualTo(source.FlipY));
                Assert.That(sample.Color, Is.EqualTo(source.Color));
                Assert.That(sample.LocalSequence, Is.EqualTo(source.LocalSequence));
                Assert.That(sample.Handle, Is.EqualTo(source.Handle));
            }
            Assert.That(published.TickIndex, Is.EqualTo(11));
            Assert.That(published.GetMotionState(0).PreciseX, Is.EqualTo(120));
        }

        private static BattlePresentationFrame MakePublication()
        {
            var previous = new BattlePresentationFrame { TickIndex = 10 };
            previous.AddMotionState(Motion(100, 0, 100));
            var published = new BattlePresentationFrame { TickIndex = 11 };
            published.AddMotionState(Motion(120, 10, 110));
            published.CopyPreviousMotionStatesFrom(previous);
            for (int index = 0; index < 4; index++)
            {
                Color32 color = new Color32((byte)(255 - index * 21), (byte)(210 + index * 12), (byte)(180 + index * 20), (byte)(160 + index * 20));
                published.AddCommand(new BattleRenderCommand(BattleRenderCommandType.Entity,
                    new RuntimeEntityHandle(3, 1), 5, 2, 0, 0, 3, 1, index, index,
                    Position(index), new Vector2(20, 16), new Vector2(0.5f, 0.5f),
                    new Rect(0.25f, 0.25f, 0.5f, 0.5f),
                    new BattleSpriteRenderState(color, (index & 1) != 0, (index & 2) != 0,
                        SpriteMaskInteraction.None, BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha),
                    default, motionAnchor: BattlePresentationMotionAnchor.Body));
            }
            return published;
        }

        private static BattlePresentationMotionState Motion(double x, double y, double z)
        {
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.SetSourceRulePosition(x, z);
            runtime.X = x * 1.5;
            runtime.Y = y;
            runtime.Z = z * 2.0;
            return new BattlePresentationMotionState(new RuntimeEntityHandle(3, 1), 2, runtime);
        }

        private static Vector3 Position(int index)
        {
            return new Vector3((48 + index * 3.25f) * NTSDRenderSpace.UnitsPerPixelX,
                (30 + index * 2.5f) * NTSDRenderSpace.UnitsPerPixelY, index * 0.001f);
        }

        private static Texture MakeTexture(int mode)
        {
            var pixels = new Color32[64];
            for (int index = 0; index < pixels.Length; index++)
                pixels[index] = new Color32(201, 47, 0, 255);
            for (int y = 2; y < 6; y++)
                for (int x = 2; x < 6; x++)
                    pixels[y * 8 + x] = new Color32((byte)(30 + x * 29), (byte)(40 + y * 27),
                        (byte)(230 - x * 17), (byte)(80 + (x + y) * 12));
            if (mode == 2)
            {
                var array = new Texture2DArray(8, 8, 2, TextureFormat.RGBA32, false)
                { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                array.SetPixels32(pixels, 0);
                for (int index = 0; index < pixels.Length; index++)
                {
                    Color32 pixel = pixels[index];
                    pixels[index] = new Color32(pixel.b, pixel.r, pixel.g, pixel.a);
                }
                array.SetPixels32(pixels, 1);
                array.Apply(false, false);
                return array;
            }
            var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static Mesh MakeIndependentQuad(in BattleRenderCommand command, in BattleCentralResolvedResource resource)
        {
            float width = resource.PixelSize.x * NTSDRenderSpace.UnitsPerPixelX * NTSDRenderSpace.BattleVisualScale;
            float height = resource.PixelSize.y * NTSDRenderSpace.UnitsPerPixelY * NTSDRenderSpace.BattleVisualScale;
            float left = command.Position.x - resource.Pivot.x * width;
            float bottom = command.Position.y - resource.Pivot.y * height;
            float z = command.Position.z;
            Rect rect = resource.NormalizedUv;
            float u0 = command.FlipX ? rect.xMax : rect.xMin;
            float u1 = command.FlipX ? rect.xMin : rect.xMax;
            float v0 = command.FlipY ? rect.yMax : rect.yMin;
            float v1 = command.FlipY ? rect.yMin : rect.yMax;
            var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
            mesh.vertices = new[]
            {
                new Vector3(left, bottom, z), new Vector3(left, bottom + height, z),
                new Vector3(left + width, bottom, z), new Vector3(left + width, bottom + height, z)
            };
            mesh.uv = new[] { new Vector2(u0, v0), new Vector2(u0, v1), new Vector2(u1, v0), new Vector2(u1, v1) };
            mesh.colors32 = new[] { resource.Color, resource.Color, resource.Color, resource.Color };
            var slices = new List<Vector2>();
            var sampleBounds = new List<Vector4>();
            var bounds = new Vector4(rect.xMin + 0.5f / resource.Texture.width, rect.yMin + 0.5f / resource.Texture.height,
                rect.xMax - 0.5f / resource.Texture.width, rect.yMax - 0.5f / resource.Texture.height);
            for (int index = 0; index < 4; index++)
            {
                slices.Add(new Vector2(resource.AtlasSlice, 0));
                sampleBounds.Add(bounds);
            }
            mesh.SetUVs(1, slices);
            mesh.SetUVs(2, sampleBounds);
            mesh.triangles = new[] { 0, 1, 2, 2, 1, 3 };
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void Begin(CommandBuffer buffer, RenderTexture target)
        {
            buffer.Clear();
            buffer.SetRenderTarget(target);
            buffer.ClearRenderTarget(false, true, Background);
            buffer.SetViewProjectionMatrices(Matrix4x4.identity,
                Matrix4x4.Ortho(0, Width * NTSDRenderSpace.UnitsPerPixelX,
                    0, Height * NTSDRenderSpace.UnitsPerPixelY, -1, 1));
        }

        private static Color32[] Read(CommandBuffer buffer, RenderTexture target, Texture2D readback)
        {
            Graphics.ExecuteCommandBuffer(buffer);
            RenderTexture.active = target;
            readback.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
            readback.Apply();
            return readback.GetPixels32();
        }

        private static int Error(Color32 left, Color32 right)
        {
            return Math.Max(Math.Max(Math.Abs(left.r - right.r), Math.Abs(left.g - right.g)),
                Math.Max(Math.Abs(left.b - right.b), Math.Abs(left.a - right.a)));
        }

        private static void WriteNew(string path, byte[] bytes)
        {
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write);
            stream.Write(bytes, 0, bytes.Length);
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            private readonly Texture texture;
            private readonly int mode;
            internal Material Material { get; }
            internal Resolver(Texture texture, Material material, int mode)
            {
                this.texture = texture;
                Material = material;
                this.mode = mode;
            }

            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                int group = command.LocalSequence / 2;
                resource = new BattleCentralResolvedResource(texture, Material, command.NormalizedUv,
                    command.Size, command.Pivot, command.Color, materialVariant: group, atlasSlice: mode == 2 ? group : 0,
                    bindingMode: mode == 2 ? BattleSpriteCentralBindingMode.AtlasTextureArray :
                        mode == 1 ? BattleSpriteCentralBindingMode.AtlasPageTexture2D : BattleSpriteCentralBindingMode.SourceTexture2D);
                return BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif

