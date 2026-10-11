#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        private const string BindingReuseOutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH67-RENDER-TEXTURE-BINDING-REUSE-20261008";
        private delegate int SegmentAppender(CommandBuffer buffer, BattleDynamicMeshBackend backend,
            MaterialPropertyBlock block, bool reuse, out int prepared, out int reused);

        [Test]
        public void SegmentTextureBindingReuse_DefaultOff()
        {
            PropertyInfo property = typeof(BattleRenderFeature).GetProperty("EnableSegmentTextureBindingReuseForDiagnostics");
            Assert.That(property, Is.Not.Null, "Candidate opt-in contract is missing.");
            Assert.That(property.GetValue(null), Is.EqualTo(false));
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void SegmentTextureBindingReuse_FirstAndRepeatedPreparation(int mode)
        {
            SegmentAppender append = GetSegmentAppender();
            using var fixture = new BindingFixture(mode, mode);
            var block = new MaterialPropertyBlock();
            try
            {
                foreach (bool reuse in new[] { false, true })
                {
                    fixture.Buffer.Clear();
                    block.SetColor("_PreviousOwnerColor", Color.red);
                    block.SetTexture(mode == 2 ? "_MainTex" : "_MainTexArray", fixture.Textures[0]);
                    int draws = append(fixture.Buffer, fixture.Backend, block, reuse, out int prepared, out int reused);
                    Assert.That(draws, Is.EqualTo(4));
                    Assert.That(prepared, Is.EqualTo(reuse ? 1 : 4));
                    Assert.That(reused, Is.EqualTo(reuse ? 3 : 0));
                    AssertBinding(block, mode, fixture.Textures[0]);
                    Assert.That(block.GetColor("_PreviousOwnerColor"), Is.EqualTo(Color.clear));
                }
            }
            finally
            {
                fixture.Buffer.Clear();
            }
        }

        [TestCase(0, 1)]
        [TestCase(0, 2)]
        [TestCase(1, 0)]
        [TestCase(1, 2)]
        [TestCase(2, 0)]
        [TestCase(2, 1)]
        public void SegmentTextureBindingReuse_EveryBindingModeTransition(int firstMode, int lastMode)
        {
            SegmentAppender append = GetSegmentAppender();
            using var fixture = new BindingFixture(firstMode, lastMode);
            var block = new MaterialPropertyBlock();
            Assert.That(append(fixture.Buffer, fixture.Backend, block, true, out int prepared, out int reused), Is.EqualTo(4));
            Assert.That(prepared, Is.EqualTo(2));
            Assert.That(reused, Is.EqualTo(2));
            AssertBinding(block, lastMode, fixture.Textures[1]);
            if (firstMode != 2 && lastMode != 2)
                Assert.That(fixture.Textures[0], Is.SameAs(fixture.Textures[1]), "Mode alone must invalidate preparation.");
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void SegmentTextureBindingReuse_ActualTextureIdentityChange(int mode)
        {
            SegmentAppender append = GetSegmentAppender();
            using var fixture = new BindingFixture(mode, mode, true);
            Assert.That(fixture.Textures[0], Is.Not.SameAs(fixture.Textures[1]));
            Assert.That(fixture.Textures[0].width, Is.EqualTo(fixture.Textures[1].width));
            var block = new MaterialPropertyBlock();
            Assert.That(append(fixture.Buffer, fixture.Backend, block, true, out int prepared, out int reused), Is.EqualTo(4));
            Assert.That(prepared, Is.EqualTo(2));
            Assert.That(reused, Is.EqualTo(2));
            AssertBinding(block, mode, fixture.Textures[1]);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void SegmentTextureBindingReuse_NewCallStartsUnprepared(int mode)
        {
            SegmentAppender append = GetSegmentAppender();
            using var fixture = new BindingFixture(mode, mode);
            using var nextBuffer = new CommandBuffer();
            var block = new MaterialPropertyBlock();
            foreach (CommandBuffer buffer in new[] { fixture.Buffer, nextBuffer })
            {
                block.SetColor("_PreviousOwnerColor", Color.red);
                block.SetTexture(mode == 2 ? "_MainTex" : "_MainTexArray", fixture.Textures[0]);
                Assert.That(append(buffer, fixture.Backend, block, true, out int prepared, out int reused), Is.EqualTo(4));
                Assert.That(prepared, Is.EqualTo(1));
                Assert.That(reused, Is.EqualTo(3));
                AssertBinding(block, mode, fixture.Textures[0]);
                Assert.That(block.GetColor("_PreviousOwnerColor"), Is.EqualTo(Color.clear));
            }
        }

        [Test]
        public void SegmentTextureBindingReuse_InvalidSegmentsKeepOriginalSkip()
        {
            SegmentAppender append = GetSegmentAppender();
            using var fixture = new BindingFixture(0, 0);
            FieldInfo field = typeof(BattleDynamicMeshBackend).GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.That(field, Is.Not.Null);
            var segments = (BattleCentralRenderSegment[])field.GetValue(fixture.Backend);
            for (int index = 1; index <= 2; index++)
            {
                BattleCentralRenderSegment segment = segments[index];
                segments[index] = new BattleCentralRenderSegment(segment.ChunkIndex, segment.SubMeshIndex,
                    segment.FirstCommandIndex, segment.CommandCount, segment.FirstQuad, segment.QuadCount,
                    index == 1 ? null : segment.Texture, index == 2 ? null : segment.Material,
                    segment.MaterialVariant, segment.AtlasSlice, segment.BindingMode, segment.AtlasPageIndex);
            }
            var block = new MaterialPropertyBlock();
            foreach (bool reuse in new[] { false, true })
            {
                fixture.Buffer.Clear();
                Assert.That(append(fixture.Buffer, fixture.Backend, block, reuse, out int prepared, out int reused), Is.EqualTo(2));
                Assert.That(prepared, Is.EqualTo(reuse ? 1 : 2));
                Assert.That(reused, Is.EqualTo(reuse ? 1 : 0));
                AssertBinding(block, 0, fixture.Textures[0]);
            }
        }

        [Test]
        public void SegmentTextureBindingReuse_EmptyBackendDoesNotPrepare()
        {
            SegmentAppender append = GetSegmentAppender();
            using var backend = new BattleDynamicMeshBackend();
            using var buffer = new CommandBuffer();
            var block = new MaterialPropertyBlock();
            block.SetColor("_PreviousOwnerColor", Color.red);
            Assert.That(append(buffer, backend, block, true, out int prepared, out int reused), Is.Zero);
            Assert.That(prepared, Is.Zero);
            Assert.That(reused, Is.Zero);
            Assert.That(buffer.sizeInBytes, Is.Zero);
            Assert.That(block.GetColor("_PreviousOwnerColor"), Is.EqualTo(Color.red));
        }

        [Test]
        public void SegmentTextureBindingReuse_FootBodyHealthPropertySlots()
        {
            SegmentAppender append = GetSegmentAppender();
            using var fixture = new BindingFixture(2, 2);
            Texture footTexture = MakeTexture(0);
            var block = new MaterialPropertyBlock();
            try
            {
                block.Clear();
                block.SetTexture("_MainTex", footTexture);
                Assert.That(append(fixture.Buffer, fixture.Backend, block, true, out int prepared, out int reused), Is.EqualTo(4));
                Assert.That(prepared, Is.EqualTo(1));
                Assert.That(reused, Is.EqualTo(3));
                AssertBinding(block, 2, fixture.Textures[0]);
                block.Clear();
                block.SetTexture("_MainTex", footTexture);
                AssertBinding(block, 0, footTexture);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(footTexture);
            }
        }

        [TestCase(0, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(1, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(2, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(0, BattleCentralDrawMode.StrictOrderedDraw)]
        [TestCase(1, BattleCentralDrawMode.StrictOrderedDraw)]
        [TestCase(2, BattleCentralDrawMode.StrictOrderedDraw)]
        public void SegmentTextureBindingReuse_PixelsMatchIndependentQuadOracle(int mode, BattleCentralDrawMode drawMode)
        {
            SegmentAppender append = GetSegmentAppender();
            Assert.That(GraphicsSettings.currentRenderPipeline, Is.Not.Null);
            Assert.That(GraphicsSettings.currentRenderPipeline.GetType().FullName, Does.Contain("Universal"));
            Assert.That(SystemInfo.graphicsDeviceType, Is.Not.EqualTo(GraphicsDeviceType.Null));
            string runRoot = Path.Combine(BindingReuseOutputRoot, "pixels-" + mode + "-" + (int)drawMode + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(runRoot);
            var target = new RenderTexture(Width, Height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear)
            { hideFlags = HideFlags.HideAndDontSave };
            var readback = new Texture2D(Width, Height, TextureFormat.RGBA32, false, true)
            { hideFlags = HideFlags.HideAndDontSave };
            RenderTexture previousTarget = RenderTexture.active;
            try
            {
                target.Create();
                foreach (bool distinctTexture in new[] { false, true })
                {
                    using var fixture = new BindingFixture(mode, mode, distinctTexture, drawMode);
                    BattlePresentationFrame published = MakePublication();
                    var captured = new BattlePresentationFrame();
                    captured.CopyFrom(published);
                    var display = new BattlePresentationDisplayMotion();
                    display.Prepare(captured, 0.5, 1.5, 2.0);
                    display.ApplyToCapturedCommands(captured);
                    AssertIndependentSample(published, captured, 0.5);
                    fixture.Backend.Build(captured, fixture.Resolver, drawMode);
                    int expectedSegments = drawMode == BattleCentralDrawMode.StrictOrderedDraw ? 4 : 2;
                    Assert.That(fixture.Backend.SegmentCount, Is.EqualTo(expectedSegments));
                    var block = new MaterialPropertyBlock();
                    Color32[][] candidates = new Color32[2][];
                    for (int side = 0; side < 2; side++)
                    {
                        Begin(fixture.Buffer, target);
                        Assert.That(append(fixture.Buffer, fixture.Backend, block, side == 1,
                            out int prepared, out int reused), Is.EqualTo(expectedSegments));
                        Assert.That(prepared, Is.EqualTo(side == 0 ? expectedSegments : distinctTexture ? 2 : 1));
                        Assert.That(prepared + reused, Is.EqualTo(expectedSegments));
                        candidates[side] = Read(fixture.Buffer, target, readback);
                        WriteNew(Path.Combine(runRoot, (distinctTexture ? "changed-" : "same-") + side + ".png"), readback.EncodeToPNG());
                    }
                    var meshes = new Mesh[4];
                    try
                    {
                        for (int index = 0; index < meshes.Length; index++)
                        {
                            BattleRenderCommand command = captured.GetCommand(index);
                            fixture.Resolver.Resolve(command, out BattleCentralResolvedResource resource);
                            meshes[index] = MakeIndependentQuad(command, resource);
                        }
                        RecordIndependentBindingQuads(fixture, captured, meshes, target, block, false);
                        Color32[] expected = Read(fixture.Buffer, target, readback);
                        WriteNew(Path.Combine(runRoot, (distinctTexture ? "changed-" : "same-") + "reference.png"), readback.EncodeToPNG());
                        int content = 0;
                        int maximumError = 0;
                        for (int index = 0; index < expected.Length; index++)
                        {
                            if (Error(expected[index], Background) > 4)
                                content++;
                            maximumError = Math.Max(maximumError, Math.Max(Error(candidates[0][index], expected[index]),
                                Error(candidates[1][index], expected[index])));
                        }
                        Assert.That(content, Is.GreaterThan(100), "Empty pixels do not qualify.");
                        Assert.That(maximumError, Is.Zero, "OFF/ON must both match independent quads.");
                        RecordIndependentBindingQuads(fixture, captured, meshes, target, block, true);
                        Color32[] reversed = Read(fixture.Buffer, target, readback);
                        int different = 0;
                        for (int index = 0; index < expected.Length; index++)
                            if (Error(expected[index], reversed[index]) > 2)
                                different++;
                        Assert.That(different, Is.GreaterThan(20), "Painter-order negative control must differ.");
                        WriteNew(Path.Combine(runRoot, (distinctTexture ? "changed-" : "same-") + "reverse.png"), readback.EncodeToPNG());
                        TestContext.WriteLine("BINDING_REUSE_PIXELS mode=" + mode + " drawMode=" + drawMode +
                            " distinct=" + distinctTexture + " segments=" + expectedSegments + " content=" + content +
                            " maximumChannelError=" + maximumError + " reverseDifferent=" + different);
                    }
                    finally
                    {
                        foreach (Mesh mesh in meshes)
                            UnityEngine.Object.DestroyImmediate(mesh);
                    }
                }
            }
            finally
            {
                RenderTexture.active = previousTarget;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
                UnityEngine.Object.DestroyImmediate(readback);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SegmentTextureBindingReuse_Cost_CompleteCommandRecording(bool alternating)
        {
            SegmentAppender append = GetSegmentAppender();
            using var fixture = new BindingFixture(0, 0, true);
            const int count = 2000;
            var frame = new BattlePresentationFrame();
            for (int index = 0; index < count; index++)
            {
                frame.AddCommand(new BattleRenderCommand(BattleRenderCommandType.Entity,
                    new RuntimeEntityHandle(3, 1), 5, 2, 0, 0, 3, 1, index, index,
                    Position(index % 4), new Vector2(20, 16), new Vector2(0.5f, 0.5f),
                    new Rect(0.25f, 0.25f, 0.5f, 0.5f),
                    new BattleSpriteRenderState(new Color32(255, 255, 255, 255), false, false,
                        SpriteMaskInteraction.None, BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha), default));
            }
            fixture.Backend.PrepareCapacity(count);
            fixture.Backend.Build(frame, new CostBindingResolver(fixture.Textures, fixture.Materials[0], alternating),
                BattleCentralDrawMode.StrictOrderedDraw);
            Assert.That(fixture.Backend.SegmentCount, Is.EqualTo(count));
            var block = new MaterialPropertyBlock();
            var samples = new double[2][] { new double[8], new double[8] };
            var timer = new System.Diagnostics.Stopwatch();
            int recordedBytes = 0;
            for (int round = 0; round < 12; round++)
            {
                for (int order = 0; order < 2; order++)
                {
                    int side = (round + order) & 1;
                    fixture.Buffer.Clear();
                    timer.Restart();
                    int draws = append(fixture.Buffer, fixture.Backend, block, side == 1, out int prepared, out int reused);
                    timer.Stop();
                    Assert.That(draws, Is.EqualTo(count));
                    Assert.That(prepared, Is.EqualTo(side == 1 && !alternating ? 1 : count));
                    Assert.That(reused, Is.EqualTo(side == 1 && !alternating ? count - 1 : 0));
                    if (recordedBytes == 0)
                        recordedBytes = fixture.Buffer.sizeInBytes;
                    Assert.That(fixture.Buffer.sizeInBytes, Is.EqualTo(recordedBytes));
                    if (round >= 4)
                        samples[side][round - 4] = timer.Elapsed.TotalMilliseconds;
                }
            }
            double offMean = 0;
            double onMean = 0;
            foreach (double sample in samples[0])
                offMean += sample / 8;
            foreach (double sample in samples[1])
                onMean += sample / 8;
            TestContext.WriteLine("SEGMENT_TEXTURE_BINDING_REUSE_COST pattern=" + (alternating ? "alternating" : "constant") +
                " segments=2000 drawsPerSide=2000 bytes=" + recordedBytes + " offMeanMs=" + offMean.ToString("R", System.Globalization.CultureInfo.InvariantCulture) +
                " onMeanMs=" + onMean.ToString("R", System.Globalization.CultureInfo.InvariantCulture) +
                " offRawMs=" + string.Join(",", Array.ConvertAll(samples[0], value => value.ToString("R", System.Globalization.CultureInfo.InvariantCulture))) +
                " onRawMs=" + string.Join(",", Array.ConvertAll(samples[1], value => value.ToString("R", System.Globalization.CultureInfo.InvariantCulture))) +
                " warmupPerSide=4 samplesPerSide=8 scope=command_recording_fixture_not_AI_FPS_or_GPU gc=UNKNOWN");
        }

        private static SegmentAppender GetSegmentAppender()
        {
            MethodInfo method = typeof(BattleRenderFeature).GetMethod("AppendSegmentDrawCommands", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null, "Candidate segment appender contract is missing.");
            return (SegmentAppender)Delegate.CreateDelegate(typeof(SegmentAppender), method);
        }

        private static void AssertBinding(MaterialPropertyBlock block, int mode, Texture texture)
        {
            Assert.That(block.GetTexture(mode == 2 ? "_MainTexArray" : "_MainTex"), Is.SameAs(texture));
            Assert.That(block.GetTexture(mode == 2 ? "_MainTex" : "_MainTexArray"), Is.Null);
        }

        private static void RecordIndependentBindingQuads(BindingFixture fixture, BattlePresentationFrame frame,
            Mesh[] meshes, RenderTexture target, MaterialPropertyBlock block, bool reverse)
        {
            Begin(fixture.Buffer, target);
            for (int sequence = 0; sequence < meshes.Length; sequence++)
            {
                int index = reverse ? meshes.Length - 1 - sequence : sequence;
                fixture.Resolver.Resolve(frame.GetCommand(index), out BattleCentralResolvedResource resource);
                block.Clear();
                block.SetTexture(resource.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ? "_MainTexArray" : "_MainTex", resource.Texture);
                fixture.Buffer.DrawMesh(meshes[index], Matrix4x4.identity, resource.Material, 0, 0, block);
            }
        }

        private sealed class BindingFixture : IDisposable
        {
            internal readonly Texture[] Textures = new Texture[2];
            internal readonly Material[] Materials = new Material[2];
            internal readonly BattleDynamicMeshBackend Backend = new BattleDynamicMeshBackend();
            internal readonly CommandBuffer Buffer = new CommandBuffer();
            internal readonly BindingResolver Resolver;

            internal BindingFixture(int firstMode, int lastMode, bool distinctTexture = false,
                BattleCentralDrawMode drawMode = BattleCentralDrawMode.StrictOrderedDraw)
            {
                try
                {
                    Textures[0] = MakeTexture(firstMode);
                    Textures[1] = !distinctTexture && (firstMode == 2) == (lastMode == 2) ? Textures[0] : MakeTexture(lastMode);
                    for (int index = 0; index < 2; index++)
                    {
                        int mode = index == 0 ? firstMode : lastMode;
                        Shader shader = Shader.Find(mode == 2 ? "NTSD/BattleCentralTransparentArray" : "NTSD/BattleCentralTransparent");
                        Assert.That(shader, Is.Not.Null);
                        Materials[index] = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                        Materials[index].SetColor("_Color", index == 0 ? Color.white : new Color(0.85f, 0.95f, 0.75f, 0.9f));
                    }
                    if (distinctTexture)
                        RecolorBindingTexture(Textures[1]);
                    Resolver = new BindingResolver(Textures, Materials, firstMode, lastMode);
                    Backend.Build(MakePublication(), Resolver, drawMode);
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public void Dispose()
            {
                Buffer.Release();
                Backend.Dispose();
                foreach (Material value in Materials)
                    UnityEngine.Object.DestroyImmediate(value);
                if (!ReferenceEquals(Textures[0], Textures[1]))
                    UnityEngine.Object.DestroyImmediate(Textures[1]);
                UnityEngine.Object.DestroyImmediate(Textures[0]);
            }
        }

        private static void RecolorBindingTexture(Texture texture)
        {
            int slices = texture is Texture2DArray array ? array.depth : 1;
            for (int slice = 0; slice < slices; slice++)
            {
                Color32[] pixels = texture is Texture2DArray sourceArray ? sourceArray.GetPixels32(slice) : ((Texture2D)texture).GetPixels32();
                for (int index = 0; index < pixels.Length; index++)
                {
                    Color32 pixel = pixels[index];
                    pixels[index] = new Color32(pixel.g, pixel.b, pixel.r, pixel.a);
                }
                if (texture is Texture2DArray destinationArray)
                    destinationArray.SetPixels32(pixels, slice);
                else
                    ((Texture2D)texture).SetPixels32(pixels);
            }
            if (texture is Texture2DArray changedArray)
                changedArray.Apply(false, false);
            else
                ((Texture2D)texture).Apply(false, false);
        }

        private sealed class BindingResolver : IBattleCentralResourceResolver
        {
            private readonly Texture[] textures;
            private readonly Material[] materials;
            private readonly int firstMode;
            private readonly int lastMode;

            internal BindingResolver(Texture[] textures, Material[] materials, int firstMode, int lastMode)
            {
                this.textures = textures;
                this.materials = materials;
                this.firstMode = firstMode;
                this.lastMode = lastMode;
            }

            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                int group = command.LocalSequence / 2;
                int mode = group == 0 ? firstMode : lastMode;
                resource = new BattleCentralResolvedResource(textures[group], materials[group], command.NormalizedUv,
                    command.Size, command.Pivot, command.Color, materialVariant: group, atlasSlice: mode == 2 ? group : 0,
                    bindingMode: mode == 2 ? BattleSpriteCentralBindingMode.AtlasTextureArray :
                        mode == 1 ? BattleSpriteCentralBindingMode.AtlasPageTexture2D : BattleSpriteCentralBindingMode.SourceTexture2D);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private sealed class CostBindingResolver : IBattleCentralResourceResolver
        {
            private readonly Texture[] textures;
            private readonly Material material;
            private readonly bool alternating;

            internal CostBindingResolver(Texture[] textures, Material material, bool alternating)
            {
                this.textures = textures;
                this.material = material;
                this.alternating = alternating;
            }

            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(textures[alternating ? command.LocalSequence & 1 : 0], material,
                    command.NormalizedUv, command.Size, command.Pivot, command.Color);
                return BattleCentralResourceStatus.Resolved;
            }
        }

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

