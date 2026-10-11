#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using NTSD.Animation;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleCatalogCentralResourceResolverEditorTests
    {

        internal const string SegmentUvOutput = "artifacts/diagnostics/NTSD-M03-SEGMENT-UV-DIMENSION-REUSE-20261010";

        internal static void SetUvReuse(BattleDynamicMeshBackend backend, bool enabled, bool count = false)
        {
            backend.SegmentUvDimensionReuseEnabled = enabled;
            typeof(BattleDynamicMeshBackend).GetField("countUvDimensionReads",
                BindingFlags.Instance | BindingFlags.NonPublic).SetValue(backend, count);
        }

        internal static string CaptureFields(object value)
        {
            var output = new System.Text.StringBuilder();
            AppendFields(output, value, true);
            return output.ToString();
        }

        private static void AppendFields(System.Text.StringBuilder output, object value, bool expand)
        {
            if (value == null) { output.Append("null;"); return; }
            Type type = value.GetType();
            if (value is float f) { output.Append(BitConverter.SingleToInt32Bits(f)).Append(';'); return; }
            if (value is double d) { output.Append(BitConverter.DoubleToInt64Bits(d)).Append(';'); return; }
            if (type.IsPrimitive || type.IsEnum || value is string)
            { output.Append(value).Append(';'); return; }
            if (value is UnityEngine.Object unity)
            { output.Append(unity == null ? 0 : unity.GetInstanceID()).Append(';'); return; }
            if (value is Array array)
            {
                output.Append(array.Length).Append('[');
                foreach (object item in array) AppendFields(output, item, false);
                output.Append(']'); return;
            }
            if (!type.IsValueType && !expand)
            { output.Append(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(value)).Append(';'); return; }
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                output.Append(field.Name).Append(':');
                AppendFields(output, field.GetValue(value), false);
            }
        }

        internal static string CaptureBackend(BattleDynamicMeshBackend backend)
        {
            var output = new System.Text.StringBuilder(CaptureFields(backend.Diagnostics));
            output.Append(backend.AllocatedChunkCount);
            for (int i = 0; i < backend.SegmentCount; i++) output.Append(CaptureFields(backend.GetSegment(i)));
            for (int i = 0; i < backend.ActiveChunkCount; i++)
            {
                Mesh mesh = backend.GetChunkMesh(i);
                int vertices = backend.GetChunkActiveQuadCount(i) * 4;
                Assert.That(mesh.GetVertexBufferStride(0), Is.EqualTo(44));
                using (var data = Mesh.AcquireReadOnlyMeshData(mesh))
                {
                    var bytes = data[0].GetVertexData<byte>(0);
                    for (int j = 0; j < vertices * 44; j++) output.Append(bytes[j]).Append(',');
                    var indices = data[0].GetIndexData<ushort>();
                    for (int j = 0; j < vertices / 4 * 6; j++) output.Append(indices[j]).Append(',');
                }
                output.Append(CaptureFields(mesh.bounds)).Append(mesh.indexFormat).Append(mesh.subMeshCount);
                for (int j = 0; j < mesh.subMeshCount; j++) output.Append(CaptureFields(mesh.GetSubMesh(j)));
            }
            return output.ToString();
        }

        internal sealed class SegmentUvFixture : IDisposable
        {
            private readonly ResolverFixture[] owners = new ResolverFixture[3];
            internal Texture2DArray ArrayTexture;
            internal BattleSpriteCatalog Catalog;
            internal readonly BattleCatalogCentralResourceResolver Resolver = new BattleCatalogCentralResourceResolver();
            internal Material Material => Mode == 1 ? owners[0].ValidArrayMaterial : owners[0].Valid2DMaterial;
            internal Texture2D TextureAt(int index) => owners[index].SourceTexture;
            internal readonly int Mode;
            private readonly bool pageBreaks;

            internal SegmentUvFixture(int mode, bool pageBreaks = false)
            {
                Mode = mode;
                this.pageBreaks = pageBreaks;
                try
                {
                    for (int i = 0; i < owners.Length; i++)
                    {
                        owners[i] = new ResolverFixture(false, "segment-uv-" + i);
                        Assert.That(owners[i].SourceTexture.Reinitialize(32, 32), Is.True);
                        Fill(owners[i].SourceTexture, i);
                    }
                    ArrayTexture = new Texture2DArray(32, 32, 2, TextureFormat.RGBA32, false)
                    { hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                    ArrayTexture.SetPixels32(owners[0].SourceTexture.GetPixels32(), 0);
                    ArrayTexture.SetPixels32(owners[1].SourceTexture.GetPixels32(), 1);
                    ArrayTexture.Apply(false, false);
                    Rebuild();
                }
                catch { Dispose(); throw; }
            }

            private static void Fill(Texture2D texture, int offset)
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                var pixels = new Color32[texture.width * texture.height];
                for (int i = 0; i < pixels.Length; i++)
                    pixels[i] = new Color32((byte)(30 + i % 140), (byte)(40 + (i * 3 + offset * 55) % 180),
                        (byte)(220 - i % 150), (byte)(100 + i % 150));
                texture.SetPixels32(pixels);
                texture.Apply(false, false);
            }

            internal void Rebuild()
            {
                var entries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>();
                for (int i = 0; i < 12; i++)
                {
                    Texture2D source = owners[i / 4].SourceTexture;
                    Rect pixels = new Rect(2 + i % 4, 2, 20, 16);
                    Texture bindingTexture = Mode == 1 ? (Texture)ArrayTexture : source;
                    Rect uv = new Rect(pixels.x / bindingTexture.width, pixels.y / bindingTexture.height,
                        pixels.width / bindingTexture.width, pixels.height / bindingTexture.height);
                    var key = new BattleSpriteKey(71, i);
                    var binding = new BattleSpriteCentralBinding((BattleSpriteCentralBindingMode)Mode,
                        bindingTexture, Mode == 1 ? i % 2 : 0, uv, pixels,
                        Mode == 1 ? i % 2 : Mode == 2 ? (pageBreaks ? i % 2 : 0) : -1);
                    entries.Add(key, new BattleSpriteEntry(key, "segment-uv-test", source, pixels,
                        new Vector2(0.5f, 0.5f), null, binding));
                }
                Catalog = new BattleSpriteCatalog(entries);
                Configure();
            }

            internal void Configure()
            {
                Resolver.Configure(Catalog, owners[0].Valid2DMaterial, owners[0].ValidArrayMaterial);
            }

            internal BattleRenderCommand Command(int pic, int sequence = 0, Vector3? position = null)
            {
                BattleRenderCommand source = owners[0].CreateCommand(Catalog, 71, pic,
                    new Color32((byte)(255 - sequence % 150), (byte)(120 + sequence % 120),
                        (byte)(160 + sequence % 90), (byte)(150 + sequence % 100)));
                var command = new BattleRenderCommand(source.Type, new RuntimeEntityHandle(3, 1), 5, 71,
                    pic, 0, 3, sequence, 0, sequence, position ?? new Vector3(sequence % 31 * 0.001f, 0.15f, sequence % 3 * 0.001f),
                    source.Size, source.Pivot, source.NormalizedUv,
                    new BattleSpriteRenderState(source.Color, (sequence & 1) != 0, (sequence & 2) != 0,
                        SpriteMaskInteraction.None, BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha),
                    source.SpriteDescriptor, motionAnchor: BattlePresentationMotionAnchor.Body);
                Catalog.TryGet(71, pic, out BattleSpriteEntry entry);
                return CreateTrustedCommandWithIdentity(command, entry);
            }

            internal BattlePresentationFrame Frame(int count, bool fragmented = false)
            {
                var frame = new BattlePresentationFrame();
                frame.Reset(77);
                for (int i = 0; i < count; i++) frame.AddCommand(Command(fragmented ? i % 3 * 4 : i % 4, i));
                return frame;
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(ArrayTexture);
                foreach (var owner in owners) owner?.Dispose();
            }
        }

        private static int CompareUvBuild(SegmentUvFixture fixture, BattlePresentationFrame frame,
            BattleCentralDrawMode mode = BattleCentralDrawMode.OrderedChunks)
        {
            string publication = CaptureFields(frame);
            using var off = new BattleDynamicMeshBackend();
            using var on = new BattleDynamicMeshBackend();
            off.PrepareCapacity(frame.CommandCount);
            on.PrepareCapacity(frame.CommandCount);
            SetUvReuse(off, false, true);
            SetUvReuse(on, true, true);
            off.Build(frame, fixture.Resolver, mode);
            on.Build(frame, fixture.Resolver, mode);
            Assert.That(CaptureBackend(on), Is.EqualTo(CaptureBackend(off)), "Full native mesh payload/metadata differs.");
            Assert.That(CaptureFields(frame), Is.EqualTo(publication), "Publication was mutated.");
            Assert.That(on.Diagnostics.CapacityGrowthCount, Is.Zero);
            return on.LastUvDimensionReadPairs;
        }


        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void SegmentUvReuse_PreparedFullPayloadAndCounts(int bindingMode)
        {
            using var fixture = new SegmentUvFixture(bindingMode);
            foreach (int count in new[] { 0, 1, 64, 4096, 4097 })
            {
                var frame = fixture.Frame(count);
                int expected = count == 0 ? 0 : count == 1 ? 1 : count == 4097 ? 3 : 2;
                Assert.That(CompareUvBuild(fixture, frame), Is.EqualTo(expected), "Getter pair count at " + count);
            }
            Assert.That(CompareUvBuild(fixture, fixture.Frame(15, true)), Is.EqualTo(bindingMode == 1 ? 2 : 15));
            Assert.That(CompareUvBuild(fixture, fixture.Frame(15), BattleCentralDrawMode.StrictOrderedDraw), Is.EqualTo(15));
            Assert.That(CompareUvBuild(fixture, fixture.Frame(15), BattleCentralDrawMode.SingleMeshDiagnosticOnly), Is.EqualTo(15));
        }

        [Test]
        public void SegmentUvReuse_PreparedBreaksAndDestroyedResource()
        {
            using var page = new SegmentUvFixture(2, true);
            Assert.That(CompareUvBuild(page, page.Frame(12)), Is.EqualTo(12), "Page identity must break segments.");
            using var fixture = new SegmentUvFixture(0);
            var broken = fixture.Frame(4);
            broken.AddCommand(default);
            for (int i = 0; i < 4; i++) broken.AddCommand(fixture.Command(i, i + 5));
            Assert.That(CompareUvBuild(fixture, broken), Is.EqualTo(4), "Unresolved must break reuse.");
            var entries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>();
            for (int i = 0; i < 12; i++)
            {
                fixture.Catalog.TryGet(71, i, out var entry);
                if (i == 2)
                    entry = entry.WithCentralBinding(new BattleSpriteCentralBinding(BattleSpriteCentralBindingMode.AtlasTextureArray,
                        fixture.ArrayTexture, 1, entry.NormalizedUv, entry.PixelRect, 1));
                entries.Add(entry.Key, entry);
            }
            fixture.Catalog = new BattleSpriteCatalog(entries);
            fixture.Configure();
            Assert.That(CompareUvBuild(fixture, fixture.Frame(12)), Is.EqualTo(10), "Texture/material/binding breaks.");
            UnityEngine.Object.DestroyImmediate(fixture.TextureAt(0));
            fixture.Configure();
            var invalid = fixture.Frame(12);
            Assert.That(CompareUvBuild(fixture, invalid), Is.GreaterThanOrEqualTo(0));
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(12);
            SetUvReuse(backend, true, true);
            backend.Build(invalid, fixture.Resolver);
            Assert.That(backend.Diagnostics.UnresolvedCommandCount, Is.GreaterThan(0));
        }

        [Test]
        public void SegmentUvReuse_BetweenBuildResizeReplaceShrinkAndNativeRecovery()
        {
            using var fixture = new SegmentUvFixture(0);
            using var replacement = new SegmentUvFixture(1);
            using var off = new BattleDynamicMeshBackend();
            using var on = new BattleDynamicMeshBackend();
            off.PrepareCapacity(4097);
            on.PrepareCapacity(4097);
            SetUvReuse(off, false, true);
            SetUvReuse(on, true, true);
            Action<BattlePresentationFrame, BattleCatalogCentralResourceResolver> compare = (frame, resolver) =>
            {
                string before = CaptureFields(frame);
                off.Build(frame, resolver);
                on.Build(frame, resolver);
                Assert.That(CaptureBackend(on), Is.EqualTo(CaptureBackend(off)));
                Assert.That(CaptureFields(frame), Is.EqualTo(before));
            };
            var large = fixture.Frame(4097);
            compare(large, fixture.Resolver);
            compare(fixture.Frame(1), fixture.Resolver);
            compare(fixture.Frame(0), fixture.Resolver);
            compare(fixture.Frame(32), fixture.Resolver);
            string original = CaptureBackend(on);
            Assert.That(fixture.TextureAt(0).Reinitialize(64, 48), Is.True);
            fixture.Configure();
            compare(fixture.Frame(32), fixture.Resolver);
            Assert.That(CaptureBackend(on), Is.Not.EqualTo(original), "New Build must read resized native texture.");
            fixture.Rebuild();
            compare(fixture.Frame(32), fixture.Resolver);
            compare(replacement.Frame(32), replacement.Resolver);
            UnityEngine.Object.DestroyImmediate(off.GetChunkMesh(0));
            UnityEngine.Object.DestroyImmediate(on.GetChunkMesh(0));
            off.PrepareCapacity(4097);
            on.PrepareCapacity(4097);
            compare(large, fixture.Resolver);
            off.Clear();
            on.Clear();
            Assert.That(on.SegmentUvDimensionReuseEnabled, Is.False);
            SetUvReuse(on, true, true);
            compare(replacement.Frame(32), replacement.Resolver);
        }

        [Test]
        public void SegmentUvReuse_OriginalCapacityAndNonfiniteRejection()
        {
            using var fixture = new SegmentUvFixture(0);
            foreach (bool enabled in new[] { false, true })
            {
                using var backend = new BattleDynamicMeshBackend();
                backend.PrepareCapacity(4);
                backend.SealCapacity(4);
                SetUvReuse(backend, enabled, true);
                backend.Build(fixture.Frame(4), fixture.Resolver);
                string before = CaptureBackend(backend);
                Assert.Throws<InvalidOperationException>(() => backend.Build(fixture.Frame(5), fixture.Resolver));
                Assert.That(CaptureBackend(backend), Is.EqualTo(before));
                var invalid = fixture.Frame(2);
                invalid.AddCommand(fixture.Command(0, 2, new Vector3(float.NaN, 0, 0)));
                Assert.Throws<ArgumentException>(() => backend.Build(invalid, fixture.Resolver));
                backend.Clear();
                backend.UnsealCapacity();
                SetUvReuse(backend, enabled, true);
                backend.Build(fixture.Frame(4), fixture.Resolver);
                Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(4));
                Assert.That(backend.LastUvDimensionReadPairs, Is.EqualTo(enabled ? 2 : 4));
            }
        }

        private sealed class UvFallbackResolver : IBattleCentralResourceResolver
        {
            internal Texture2D Texture;
            internal Material Material;
            internal bool Resize;
            internal bool Throw;
            internal Rect[] Uvs;
            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                if (Throw && command.LocalSequence == 2) throw new InvalidOperationException("owned resolver fault");
                if (Resize && command.LocalSequence == 1) Texture.Reinitialize(64, 48);
                Rect uv = Uvs == null ? new Rect(0.125f, 0.25f, 0.5f, 0.5f) : Uvs[command.LocalSequence % Uvs.Length];
                resource = new BattleCentralResolvedResource(Texture, Material, uv, command.Size, command.Pivot, command.Color);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        [Test]
        public void SegmentUvReuse_CustomFallbackInvalidUvsNullDestroyedResizeAndException()
        {
            using var fixture = new SegmentUvFixture(0);
            var resolver = new UvFallbackResolver { Texture = fixture.TextureAt(0), Material = fixture.Material };
            var frame = fixture.Frame(12);
            var uvs = new[] { new Rect(0, 0, 0, 1), new Rect(0, 0, 1, 1), new Rect(0, 0, -1, 1),
                new Rect(0, 0, 1, -1), new Rect(0, 0, float.NaN, 1), new Rect(0, 0, 0.00001f, 0.00002f) };
            for (int shape = 0; shape < 4; shape++)
            {
                resolver.Uvs = shape == 0 ? uvs : null;
                resolver.Resize = shape == 1;
                if (shape == 2) resolver.Texture = null;
                if (shape == 3)
                {
                    resolver.Texture = fixture.TextureAt(0);
                    UnityEngine.Object.DestroyImmediate(resolver.Texture);
                }
                using var off = new BattleDynamicMeshBackend();
                using var on = new BattleDynamicMeshBackend();
                off.PrepareCapacity(12);
                on.PrepareCapacity(12);
                SetUvReuse(off, false, true);
                SetUvReuse(on, true, true);
                if (resolver.Resize) resolver.Texture.Reinitialize(32, 32);
                off.Build(frame, resolver);
                if (resolver.Resize) resolver.Texture.Reinitialize(32, 32);
                on.Build(frame, resolver);
                Assert.That(CaptureBackend(on), Is.EqualTo(CaptureBackend(off)));
                Assert.That(on.LastUvDimensionReadPairs, Is.EqualTo(off.LastUvDimensionReadPairs));
                Assert.That(on.LastUvDimensionReadPairs, Is.EqualTo(shape == 0 ? 4 : shape == 1 ? 12 : 0));
            }
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(12);
            SetUvReuse(backend, true, true);
            resolver.Throw = true;
            Assert.Throws<InvalidOperationException>(() => backend.Build(frame, resolver));
            backend.Clear();
            SetUvReuse(backend, true, true);
            resolver.Throw = false;
            resolver.Texture = fixture.TextureAt(1);
            backend.Build(frame, resolver);
            Assert.That(backend.LastUvDimensionReadPairs, Is.EqualTo(12));
        }

        private static readonly FieldInfo FallbackMaterialField =
            typeof(BattleCatalogCentralResourceResolver).GetField(
                "fallbackMaterial",
                BindingFlags.Instance | BindingFlags.NonPublic);


        [Test]
        public void SegmentUvReuse_DefaultOffAndClearDisables()
        {
            PropertyInfo flag = typeof(BattleDynamicMeshBackend).GetProperty(
                "SegmentUvDimensionReuseEnabled", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(flag, Is.Not.Null, "The bounded candidate flag is not implemented.");
            using var backend = new BattleDynamicMeshBackend();
            Assert.That(flag.GetValue(backend), Is.False);
            flag.SetValue(backend, true);
            backend.Clear();
            Assert.That(flag.GetValue(backend), Is.False);
            flag.SetValue(backend, true);
            backend.Dispose();
            Assert.That(flag.GetValue(backend), Is.False);
        }

        [Test]
        public void ResolvePrepared_BleedMarkUsesOpaqueSolidResourceAndFailsClosed()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            var color = new Color32(255, 0, 0, 255);
            var state = new BattleSpriteRenderState(color, false, false,
                SpriteMaskInteraction.None,
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
            var descriptor = new BattleSpriteValueDescriptor(false, false, 0, 0, 0,
                new Rect(0f, 0f, 1f, 1f), new Vector2(0f, 1f),
                BattleVisualResourceKey.CommonSolid);
            BattleRenderCommand Create(BattleSpriteRenderState renderState,
                BattleSpriteValueDescriptor spriteDescriptor) =>
                new BattleRenderCommand(
                    BattleRenderCommandType.BleedMark,
                    new RuntimeEntityHandle(1, 1), 1, 9, 0, 180, 1, 102,
                    0, 1, Vector3.zero, new Vector2(1f, 3f),
                    new Vector2(0f, 1f), new Rect(0f, 0f, 1f, 1f),
                    renderState, spriteDescriptor);

            fixture.Resolver.Configure(BattleSpriteCatalog.Empty,
                BattleCommonVisualCatalog.Empty,
                fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            var command = Create(state, descriptor);
            Assert.That(fixture.Resolver.ResolvePrepared(command,
                out BattleCentralResolvedResource resource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(resource.Texture, Is.SameAs(Texture2D.whiteTexture));
            Assert.That(resource.Material, Is.SameAs(fixture.Valid2DMaterial));
            Assert.That(resource.Color, Is.EqualTo(color));
            Assert.That(resource.PixelSize, Is.EqualTo(Vector2.zero));

            var frame = new BattlePresentationFrame();
            MethodInfo resetFrame = typeof(BattlePresentationFrame).GetMethod(
                "Reset", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo addCommand = typeof(BattlePresentationFrame).GetMethod(
                "AddCommand", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(resetFrame, Is.Not.Null);
            Assert.That(addCommand, Is.Not.Null);
            resetFrame.Invoke(frame, new object[] { 1, BattleCommonVisualCatalog.Empty });
            addCommand.Invoke(frame, new object[] { command });
            using (var backend = new BattleDynamicMeshBackend())
            {
                backend.Build(frame, fixture.Resolver);
                Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(1));
                Assert.That(backend.GetChunkActiveQuadCount(0), Is.EqualTo(1));
                Assert.That(backend.GetChunkVertexColor(0, 0), Is.EqualTo(color));
                Bounds bounds = backend.GetChunkMesh(0).GetSubMesh(0).bounds;
                Assert.That(bounds.size.x, Is.EqualTo(
                    NTSDRenderSpace.UnitsPerPixelX *
                    NTSDRenderSpace.BattleVisualScale).Within(0.0001f));
                Assert.That(bounds.size.y, Is.EqualTo(
                    3f * NTSDRenderSpace.UnitsPerPixelY *
                    NTSDRenderSpace.BattleVisualScale).Within(0.0001f));
            }

            var wrongKey = new BattleSpriteValueDescriptor(false, false, 0, 0, 0,
                new Rect(0f, 0f, 1f, 1f), new Vector2(0f, 1f),
                BattleVisualResourceKey.CommonShadow);
            Assert.That(fixture.Resolver.ResolvePrepared(Create(state, wrongKey), out _),
                Is.EqualTo(BattleCentralResourceStatus.UnresolvedVisual));
            var masked = new BattleSpriteRenderState(color, false, false,
                SpriteMaskInteraction.VisibleInsideMask,
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
            Assert.That(fixture.Resolver.ResolvePrepared(Create(masked, descriptor), out _),
                Is.EqualTo(BattleCentralResourceStatus.UnsupportedRenderState));

            fixture.Resolver.Configure(BattleSpriteCatalog.Empty,
                BattleCommonVisualCatalog.Empty,
                fixture.Invalid2DMaterial, fixture.ValidArrayMaterial);
            Assert.That(fixture.Resolver.ResolvePrepared(command, out _),
                Is.Not.EqualTo(BattleCentralResourceStatus.Resolved));
        }

        [Test]
        public void Configure_ValidAndInvalid2DMaterials_PreserveResolverStatus()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            AssertResolved(fixture.Resolver, command);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Invalid2DMaterial, fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, command);
            AssertUnresolved(fixture.Resolver, command);
        }

        [Test]
        public void Configure_RevalidatesMaterialMutatedSinceThePreviousBuild()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            int clearsAfterValidConfiguration = fixture.Resolver.TemplateClears;

            fixture.Valid2DMaterial.SetColor("_Color", Color.black);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, command);
            Assert.That(fixture.Resolver.TemplateClears, Is.EqualTo(clearsAfterValidConfiguration + 1));

            fixture.Valid2DMaterial.SetColor("_Color", Color.white);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            Assert.That(fixture.Resolver.TemplateClears, Is.EqualTo(clearsAfterValidConfiguration + 2));
        }

        [Test]
        public void Configure_ValidAndInvalidArrayMaterials_PreserveResolverStatus()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            BattleRenderCommand command = fixture.CreateCommand(1);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            AssertResolved(fixture.Resolver, command);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.InvalidArrayMaterial);
            AssertUnresolved(fixture.Resolver, command);
            AssertUnresolved(fixture.Resolver, command);
        }

        [Test]
        public void Resolve_AllTemplateKinds_KeepColdAndCachedFieldsIdentical()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            using var common = new CommonFixture("cold-cached");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            BattleRenderCommand[] commands =
            {
                fixture.CreateCommand(0),
                fixture.CreateCommand(1),
                common.CreateShadowCommand(),
                common.CreateSparkCommand(),
                common.CreateWordCommand(),
            };

            for (int commandIndex = 0; commandIndex < commands.Length; commandIndex++)
            {
                BattleCentralResourceStatus coldStatus =
                    fixture.Resolver.Resolve(commands[commandIndex], out BattleCentralResolvedResource cold);
                BattleCentralResourceStatus cachedStatus =
                    fixture.Resolver.Resolve(commands[commandIndex], out BattleCentralResolvedResource cached);

                Assert.That(coldStatus, Is.EqualTo(BattleCentralResourceStatus.Resolved));
                Assert.That(cachedStatus, Is.EqualTo(coldStatus));
                AssertResourcesEqual(cold, cached);
            }
        }

        [Test]
        public void ResolvePrepared_WarmedTrustedResources_MatchStrictResolveForAllTemplateKinds()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            using var common = new CommonFixture("prepared-cached");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            BattleRenderCommand[] commands =
            {
                fixture.CreateCommand(0),
                fixture.CreateCommand(1),
                common.CreateShadowCommand(),
                common.CreateSparkCommand(),
                common.CreateWordCommand(),
            };

            for (int commandIndex = 0; commandIndex < commands.Length; commandIndex++)
            {
                Assert.That(
                    fixture.Resolver.Resolve(
                        commands[commandIndex],
                        out BattleCentralResolvedResource expected),
                    Is.EqualTo(BattleCentralResourceStatus.Resolved));
                Assert.That(
                    fixture.Resolver.ResolvePrepared(
                        commands[commandIndex],
                        out BattleCentralResolvedResource actual),
                    Is.EqualTo(BattleCentralResourceStatus.Resolved));
                AssertResourcesEqual(expected, actual);
            }
        }

        [Test]
        public void ResolvePrepared_RepeatedTrustedType_UsesHotResourceAndCurrentColor()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand first = fixture.CreateTrustedCommand(0, Color.white);
            BattleRenderCommand tinted = fixture.CreateTrustedCommand(0, Color.red);
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            Assert.That(
                fixture.Resolver.Resolve(first, out _),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            int trustedHitsBefore = fixture.Resolver.TrustedResourceCacheHits;
            Assert.That(
                fixture.Resolver.ResolvePrepared(first, out _),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                fixture.Resolver.TrustedResourceCacheHits,
                Is.EqualTo(trustedHitsBefore + 1));

            int hotHitsBefore = fixture.Resolver.PreparedHotResourceCacheHits;
            Assert.That(
                fixture.Resolver.ResolvePrepared(
                    tinted,
                    out BattleCentralResolvedResource resolved),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                fixture.Resolver.PreparedHotResourceCacheHits,
                Is.EqualTo(hotHitsBefore + 1));
            Assert.That(resolved.Color, Is.EqualTo((Color32)Color.red));
        }

        [Test]
        public void ResolvePrepared_DestroyedResourceDetectedByConfigure_FailsClosed()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            Assert.That(
                fixture.Resolver.ResolvePrepared(command, out _),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                fixture.Resolver.ResolvePrepared(command, out _),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            int hotHitsBeforeInvalidation =
                fixture.Resolver.PreparedHotResourceCacheHits;

            UnityEngine.Object.DestroyImmediate(fixture.SourceTexture);
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            Assert.That(
                fixture.Resolver.ResolvePrepared(command, out _),
                Is.EqualTo(BattleCentralResourceStatus.UnresolvedVisual));
            Assert.That(
                fixture.Resolver.PreparedHotResourceCacheHits,
                Is.EqualTo(hotHitsBeforeInvalidation));
            Assert.That(fixture.Resolver.DestroyedResourceInvalidations, Is.EqualTo(1));
        }

        [Test]
        public void ResolvePrepared_WarmedTrustedIdentity_StillValidatesCommandEnvelope()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand valid = fixture.CreateTrustedCommand(0, Color.white);
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            Assert.That(
                fixture.Resolver.ResolvePrepared(valid, out _),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));

            BattleRenderCommand unsupported = fixture.CreateMutatedTrustedCommand(
                0,
                SignatureMutation.MaterialSemantic);
            Assert.That(
                fixture.Resolver.ResolvePrepared(unsupported, out _),
                Is.EqualTo(BattleCentralResourceStatus.UnsupportedRenderState));

            BattleRenderCommand missingLogicalKey = fixture.CreateMutatedTrustedCommand(
                0,
                SignatureMutation.MissingLogicalKey);
            Assert.That(
                fixture.Resolver.ResolvePrepared(missingLogicalKey, out _),
                Is.EqualTo(BattleCentralResourceStatus.UnresolvedVisual));
        }

        [Test]
        public void Resolve_CommonSpecialCom_UsesPublishedBindingAndTrustedCache()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            using var common = new CommonFixture("special-com");
            BattleCommonVisualCatalog catalog = common.CreateCatalog(
                includeShadow: true,
                completeSparks: false,
                completeWords: false,
                includeSpecialCom: true);
            fixture.Resolver.Configure(
                fixture.Catalog,
                catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            BattleRenderCommand command = common.CreateSpecialComCommand();
            AssertResolved(fixture.Resolver, command);
            int cacheHitsBefore = fixture.Resolver.TrustedResourceCacheHits;
            AssertResolved(fixture.Resolver, command);
            Assert.That(fixture.Resolver.TrustedResourceCacheHits,
                Is.EqualTo(cacheHitsBefore + 1));
        }

        [Test]
        public void Configure_SameReferencesAndValidity_PreservesWarmedTemplateCaches()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            using var common = new CommonFixture("no-op");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            BattleRenderCommand[] commands =
            {
                fixture.CreateCommand(0),
                fixture.CreateCommand(1),
                common.CreateShadowCommand(),
                common.CreateSparkCommand(),
                common.CreateWordCommand(),
            };
            var expectedResources = new BattleCentralResolvedResource[commands.Length];
            for (int commandIndex = 0; commandIndex < commands.Length; commandIndex++)
            {
                Assert.That(
                    fixture.Resolver.Resolve(commands[commandIndex], out expectedResources[commandIndex]),
                    Is.EqualTo(BattleCentralResourceStatus.Resolved));
            }

            int clearsBeforeNoOp = fixture.Resolver.TemplateClears;
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            Assert.That(fixture.Resolver.ConfigureCalls, Is.EqualTo(2));
            Assert.That(fixture.Resolver.NoOpHits, Is.EqualTo(1));
            Assert.That(fixture.Resolver.TemplateClears, Is.EqualTo(clearsBeforeNoOp));
            for (int commandIndex = 0; commandIndex < commands.Length; commandIndex++)
            {
                Assert.That(
                    fixture.Resolver.Resolve(commands[commandIndex], out BattleCentralResolvedResource actual),
                    Is.EqualTo(BattleCentralResourceStatus.Resolved));
                AssertResourcesEqual(expectedResources[commandIndex], actual);
            }
        }

        [Test]
        public void Resolve_EntityExactSignatureMutations_FailClosedAfterWarmup()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            BattleRenderCommand command = fixture.CreateCommand(0);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);

            foreach (SignatureMutation mutation in Enum.GetValues(typeof(SignatureMutation)))
            {
                BattleRenderCommand mutated = MutateEntityCommand(command, mutation);
                BattleCentralResourceStatus expected =
                    mutation == SignatureMutation.MaterialSemantic ||
                    mutation == SignatureMutation.MaskInteraction
                        ? BattleCentralResourceStatus.UnsupportedRenderState
                        : BattleCentralResourceStatus.UnresolvedVisual;
                Assert.That(
                    fixture.Resolver.Resolve(mutated, out _),
                    Is.EqualTo(expected),
                    mutation.ToString());
            }
        }

        [Test]
        public void Resolve_MaterialOutsideConfigureReferences_UsesImmediateValidation()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);

            Assert.That(FallbackMaterialField, Is.Not.Null);
            AssertResolved(fixture.Resolver, command);
            FallbackMaterialField.SetValue(fixture.Resolver, fixture.Invalid2DMaterial);
            AssertUnresolved(fixture.Resolver, command);

            FallbackMaterialField.SetValue(fixture.Resolver, fixture.AlternateValid2DMaterial);
            AssertResolved(fixture.Resolver, command);
        }

        [Test]
        public void Resolve_SparkMaterialIdentityMutation_FailsClosedAfterWarmup()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            using var common = new CommonFixture("spark-material");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            BattleRenderCommand command = common.CreateSparkCommand();
            AssertResolved(fixture.Resolver, command);

            BattleSpriteValueDescriptor descriptor = command.SpriteDescriptor;
            var mutatedDescriptor = new BattleSpriteValueDescriptor(
                descriptor.RequiresSprite,
                descriptor.HasSprite,
                descriptor.SpriteInstanceId,
                descriptor.TextureInstanceId,
                descriptor.MaterialInstanceId + 1,
                descriptor.PixelRect,
                descriptor.PivotNormalized,
                descriptor.LogicalResourceKey);
            AssertUnresolved(fixture.Resolver, CloneCommand(command, mutatedDescriptor));
        }

        [Test]
        public void Resolve_CachedTemplate_InjectsCurrentCommandColor()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);

            Color32 firstColor = new Color32(17, 31, 47, 63);
            Color32 secondColor = new Color32(79, 97, 113, 131);
            Assert.That(
                fixture.Resolver.Resolve(
                    fixture.CreateCommand(0, firstColor),
                    out BattleCentralResolvedResource first),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                fixture.Resolver.Resolve(
                    fixture.CreateCommand(0, secondColor),
                    out BattleCentralResolvedResource second),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));

            Assert.That(first.Color, Is.EqualTo(firstColor));
            Assert.That(second.Color, Is.EqualTo(secondColor));
            Assert.That(ReferenceEquals(first.Texture, second.Texture), Is.True);
            Assert.That(ReferenceEquals(first.Material, second.Material), Is.True);
        }

        [Test]
        public void Resolve_TrustedIdentityCache_ReusesResourceAndKeepsPerCommandColor()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            Color32 firstColor = new Color32(17, 31, 47, 63);
            Color32 secondColor = new Color32(79, 97, 113, 131);
            BattleRenderCommand firstCommand = fixture.CreateTrustedCommand(0, firstColor);
            BattleRenderCommand secondCommand = fixture.CreateTrustedCommand(0, secondColor);

            Assert.That(
                fixture.Resolver.Resolve(firstCommand, out BattleCentralResolvedResource first),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(fixture.Resolver.TrustedResourceCacheMisses, Is.EqualTo(1));
            Assert.That(fixture.Resolver.TrustedResourceCacheHits, Is.EqualTo(0));

            Assert.That(
                fixture.Resolver.Resolve(secondCommand, out BattleCentralResolvedResource second),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(fixture.Resolver.TrustedResourceCacheHits, Is.EqualTo(1));
            Assert.That(first.Color, Is.EqualTo(firstColor));
            Assert.That(second.Color, Is.EqualTo(secondColor));
            Assert.That(ReferenceEquals(first.Texture, second.Texture), Is.True);
            Assert.That(ReferenceEquals(first.Material, second.Material), Is.True);
        }

        [Test]
        public void Configure_ReferenceChangesDropPriorCatalogCommonAndMaterialTemplates()
        {
            using var first = new ResolverFixture(includeArrayBinding: true, name: "epoch-a");
            using var second = new ResolverFixture(includeArrayBinding: true, name: "epoch-b");
            using var commonA = new CommonFixture("epoch-common-a");
            using var commonB = new CommonFixture("epoch-common-b");
            BattleCatalogCentralResourceResolver resolver = first.Resolver;
            BattleRenderCommand entityA = first.CreateCommand(0);
            BattleRenderCommand entityB = second.CreateCommand(0);
            BattleRenderCommand arrayB = second.CreateCommand(1);
            BattleRenderCommand shadowA = commonA.CreateShadowCommand();
            BattleRenderCommand shadowB = commonB.CreateShadowCommand();

            resolver.Configure(
                first.Catalog,
                commonA.Catalog,
                first.Valid2DMaterial,
                first.ValidArrayMaterial);
            AssertResolved(resolver, entityA);
            AssertResolved(resolver, shadowA);
            int expectedTemplateClears = resolver.TemplateClears;

            resolver.Configure(
                second.Catalog,
                commonA.Catalog,
                first.Valid2DMaterial,
                first.ValidArrayMaterial);
            Assert.That(resolver.TemplateClears, Is.EqualTo(++expectedTemplateClears));
            AssertUnresolved(resolver, entityA);
            Assert.That(
                resolver.Resolve(entityB, out BattleCentralResolvedResource entityBResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(ReferenceEquals(entityBResource.Texture, second.SourceTexture), Is.True);

            resolver.Configure(
                second.Catalog,
                commonB.Catalog,
                first.Valid2DMaterial,
                first.ValidArrayMaterial);
            Assert.That(resolver.TemplateClears, Is.EqualTo(++expectedTemplateClears));
            AssertUnresolved(resolver, shadowA);
            Assert.That(
                resolver.Resolve(shadowB, out BattleCentralResolvedResource shadowBResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(ReferenceEquals(shadowBResource.Texture, commonB.Texture), Is.True);

            resolver.Configure(
                second.Catalog,
                commonB.Catalog,
                first.AlternateValid2DMaterial,
                first.ValidArrayMaterial);
            Assert.That(resolver.TemplateClears, Is.EqualTo(++expectedTemplateClears));
            Assert.That(
                resolver.Resolve(entityB, out BattleCentralResolvedResource fallbackResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                ReferenceEquals(fallbackResource.Material, first.AlternateValid2DMaterial),
                Is.True);

            resolver.Configure(
                second.Catalog,
                commonB.Catalog,
                first.AlternateValid2DMaterial,
                first.AlternateValidArrayMaterial);
            Assert.That(resolver.TemplateClears, Is.EqualTo(++expectedTemplateClears));
            Assert.That(
                resolver.Resolve(arrayB, out BattleCentralResolvedResource arrayResource),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
            Assert.That(
                ReferenceEquals(arrayResource.Material, first.AlternateValidArrayMaterial),
                Is.True);

            resolver.Configure(null, null, null, null);
            Assert.That(resolver.TemplateClears, Is.EqualTo(++expectedTemplateClears));
            AssertUnresolved(resolver, entityB);
            AssertUnresolved(resolver, shadowB);
        }

        [Test]
        public void Configure_DestroyedMaterialNeverUsesTheNoOpCachePath()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand command = fixture.CreateCommand(0);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            AssertResolved(fixture.Resolver, command);
            int clearsBeforeDestroy = fixture.Resolver.TemplateClears;

            UnityEngine.Object.DestroyImmediate(fixture.Valid2DMaterial);
            Assert.That(fixture.Valid2DMaterial == null, Is.True);
            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);

            Assert.That(fixture.Resolver.TemplateClears, Is.EqualTo(clearsBeforeDestroy + 1));
            Assert.That(fixture.Resolver.NoOpHits, Is.Zero);
            AssertUnresolved(fixture.Resolver, command);

            fixture.Resolver.Configure(fixture.Catalog, fixture.Valid2DMaterial, fixture.ValidArrayMaterial);
            Assert.That(fixture.Resolver.TemplateClears, Is.EqualTo(clearsBeforeDestroy + 2));
            AssertUnresolved(fixture.Resolver, command);
        }

        [Test]
        public void CommonCatalog_ValidityFlags_PreserveExistingCompletenessSemantics()
        {
            using var common = new CommonFixture("validity");

            BattleCommonVisualCatalog complete = common.CreateCatalog(
                includeShadow: true,
                completeSparks: true,
                completeWords: true);
            BattleCommonVisualCatalog missingShadow = common.CreateCatalog(
                includeShadow: false,
                completeSparks: true,
                completeWords: true);
            BattleCommonVisualCatalog missingSpark = common.CreateCatalog(
                includeShadow: true,
                completeSparks: false,
                completeWords: true);
            BattleCommonVisualCatalog missingGlyph = common.CreateCatalog(
                includeShadow: true,
                completeSparks: true,
                completeWords: false);

            Assert.That(complete.IsShadowValid, Is.True);
            Assert.That(complete.IsSparkValid, Is.True);
            Assert.That(complete.IsWordsValid, Is.True);
            Assert.That(complete.IsComplete, Is.True);

            Assert.That(missingShadow.IsShadowValid, Is.False);
            Assert.That(missingShadow.IsSparkValid, Is.True);
            Assert.That(missingShadow.IsWordsValid, Is.True);
            Assert.That(missingShadow.IsComplete, Is.False);

            Assert.That(missingSpark.IsShadowValid, Is.True);
            Assert.That(missingSpark.IsSparkValid, Is.False);
            Assert.That(missingSpark.IsWordsValid, Is.True);
            Assert.That(missingSpark.IsComplete, Is.False);

            Assert.That(missingGlyph.IsShadowValid, Is.True);
            Assert.That(missingGlyph.IsSparkValid, Is.True);
            Assert.That(missingGlyph.IsWordsValid, Is.False);
            Assert.That(missingGlyph.IsComplete, Is.False);
        }

        [Test]
        public void Resolve_InvalidBindingAndMaterial_RemainCachedUnresolved()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            BattleRenderCommand validCommand = fixture.CreateCommand(0);
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Invalid2DMaterial,
                fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, validCommand);
            AssertUnresolved(fixture.Resolver, validCommand);

            BattleSpriteCatalog invalidCatalog = fixture.CreateInvalidBindingCatalog();
            BattleRenderCommand invalidBindingCommand =
                fixture.CreateCommand(invalidCatalog, ResolverFixture.InvalidBindingVisualDataId, 0);
            fixture.Resolver.Configure(
                invalidCatalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            AssertUnresolved(fixture.Resolver, invalidBindingCommand);
            AssertUnresolved(fixture.Resolver, invalidBindingCommand);
        }

        [Test]
        public void Resolve_WarmedEntityShadowSparkAndWordTemplates_AllocateZeroBytes()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            using var common = new CommonFixture("allocation");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            BattleRenderCommand entity = fixture.CreateCommand(0);
            BattleRenderCommand shadow = common.CreateShadowCommand();
            BattleRenderCommand spark = common.CreateSparkCommand();
            BattleRenderCommand word = common.CreateWordCommand();
            AssertResolved(fixture.Resolver, entity);
            AssertResolved(fixture.Resolver, shadow);
            AssertResolved(fixture.Resolver, spark);
            AssertResolved(fixture.Resolver, word);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 256; iteration++)
            {
                fixture.Resolver.Resolve(entity, out _);
                fixture.Resolver.Resolve(shadow, out _);
                fixture.Resolver.Resolve(spark, out _);
                fixture.Resolver.Resolve(word, out _);
            }
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
        }

        [Test]
        public void Resolve_ColdPreparedEntityTemplates_AllocateZeroBytesAfterCapacitySeal()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: true);
            BattleRenderCommand first = fixture.CreateCommand(0);
            BattleRenderCommand second = fixture.CreateCommand(1);

            var warmupResolver = new BattleCatalogCentralResourceResolver();
            warmupResolver.Configure(
                fixture.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            AssertResolved(warmupResolver, first);

            fixture.Resolver.PrepareCapacity(
                fixture.Catalog.Count,
                fixture.Catalog.Count);
            fixture.Resolver.SealCapacity();
            fixture.Resolver.Configure(
                fixture.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            AssertResolved(fixture.Resolver, first);
            AssertResolved(fixture.Resolver, second);
            long allocatedAfter = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(allocatedAfter - allocatedBefore, Is.Zero);
            Assert.That(fixture.Resolver.SealedCapacityCacheSkips, Is.Zero);
        }

        [Test]
        public void Resolve_DestroyedEntityAndCommonResourcesInvalidateTheWholeGeneration()
        {
            using var fixture = new ResolverFixture(includeArrayBinding: false);
            using var common = new CommonFixture("destroy-generation");
            fixture.Resolver.Configure(
                fixture.Catalog,
                common.Catalog,
                fixture.Valid2DMaterial,
                fixture.ValidArrayMaterial);
            BattleRenderCommand entity = fixture.CreateCommand(0);
            BattleRenderCommand shadow = common.CreateShadowCommand();
            AssertResolved(fixture.Resolver, entity);
            AssertResolved(fixture.Resolver, entity);
            int generationBeforeEntityDestroy = fixture.Resolver.BindingGeneration;

            UnityEngine.Object.DestroyImmediate(fixture.SourceTexture);
            Assert.That(fixture.SourceTexture == null, Is.True);
            AssertUnresolved(fixture.Resolver, entity);
            Assert.That(
                fixture.Resolver.BindingGeneration,
                Is.GreaterThan(generationBeforeEntityDestroy));
            Assert.That(fixture.Resolver.DestroyedResourceInvalidations, Is.EqualTo(1));

            AssertResolved(fixture.Resolver, shadow);
            int generationBeforeCommonDestroy = fixture.Resolver.BindingGeneration;
            UnityEngine.Object.DestroyImmediate(common.Texture);
            Assert.That(common.Texture == null, Is.True);
            AssertUnresolved(fixture.Resolver, shadow);
            Assert.That(
                fixture.Resolver.BindingGeneration,
                Is.GreaterThan(generationBeforeCommonDestroy));
            Assert.That(fixture.Resolver.DestroyedResourceInvalidations, Is.EqualTo(2));
        }

        private static void AssertResolved(
            BattleCatalogCentralResourceResolver resolver,
            in BattleRenderCommand command)
        {
            Assert.That(
                resolver.Resolve(command, out _),
                Is.EqualTo(BattleCentralResourceStatus.Resolved));
        }

        private static void AssertUnresolved(
            BattleCatalogCentralResourceResolver resolver,
            in BattleRenderCommand command)
        {
            Assert.That(
                resolver.Resolve(command, out _),
                Is.EqualTo(BattleCentralResourceStatus.UnresolvedVisual));
        }

        private static void AssertResourcesEqual(
            in BattleCentralResolvedResource expected,
            in BattleCentralResolvedResource actual)
        {
            Assert.That(ReferenceEquals(actual.Texture, expected.Texture), Is.True);
            Assert.That(ReferenceEquals(actual.Material, expected.Material), Is.True);
            Assert.That(actual.NormalizedUv, Is.EqualTo(expected.NormalizedUv));
            Assert.That(actual.PixelSize, Is.EqualTo(expected.PixelSize));
            Assert.That(actual.Pivot, Is.EqualTo(expected.Pivot));
            Assert.That(actual.Color, Is.EqualTo(expected.Color));
            Assert.That(actual.MaterialVariant, Is.EqualTo(expected.MaterialVariant));
            Assert.That(actual.AtlasSlice, Is.EqualTo(expected.AtlasSlice));
            Assert.That(actual.BindingMode, Is.EqualTo(expected.BindingMode));
            Assert.That(actual.AtlasPageIndex, Is.EqualTo(expected.AtlasPageIndex));
        }

        private static BattleRenderCommand MutateEntityCommand(
            in BattleRenderCommand command,
            SignatureMutation mutation)
        {
            BattleSpriteValueDescriptor source = command.SpriteDescriptor;
            bool requiresSprite = source.RequiresSprite;
            bool hasSprite = source.HasSprite;
            bool hasLogicalKey = source.HasLogicalResourceKey;
            int spriteInstanceId = source.SpriteInstanceId;
            int textureInstanceId = source.TextureInstanceId;
            int materialInstanceId = source.MaterialInstanceId;
            Rect pixelRect = source.PixelRect;
            Vector2 descriptorPivot = source.PivotNormalized;
            BattleVisualResourceKey logicalKey = source.LogicalResourceKey;
            int visualDataId = command.VisualDataId;
            int effectivePic = command.EffectivePic;
            Vector2 size = command.Size;
            Vector2 commandPivot = command.Pivot;
            Rect normalizedUv = command.NormalizedUv;
            BattleSpriteRenderState renderState = command.RenderState;

            switch (mutation)
            {
                case SignatureMutation.MissingLogicalKey:
                    hasLogicalKey = false;
                    break;
                case SignatureMutation.LogicalKey:
                    logicalKey = BattleVisualResourceKey.FromEntity(new BattleSpriteKey(17, 1));
                    break;
                case SignatureMutation.VisualDataId:
                    visualDataId++;
                    break;
                case SignatureMutation.EffectivePic:
                    effectivePic++;
                    break;
                case SignatureMutation.RequiresSprite:
                    requiresSprite = !requiresSprite;
                    break;
                case SignatureMutation.HasSprite:
                    hasSprite = !hasSprite;
                    break;
                case SignatureMutation.SpriteInstanceId:
                    spriteInstanceId++;
                    break;
                case SignatureMutation.TextureInstanceId:
                    textureInstanceId++;
                    break;
                case SignatureMutation.MaterialInstanceId:
                    materialInstanceId++;
                    break;
                case SignatureMutation.PixelRect:
                    pixelRect.x++;
                    break;
                case SignatureMutation.DescriptorPivot:
                    descriptorPivot.x += 0.125f;
                    break;
                case SignatureMutation.CommandPivot:
                    commandPivot.x += 0.125f;
                    break;
                case SignatureMutation.NormalizedUv:
                    normalizedUv.x += 0.125f;
                    break;
                case SignatureMutation.Size:
                    size.x++;
                    break;
                case SignatureMutation.MaterialSemantic:
                    renderState = new BattleSpriteRenderState(
                        renderState.Color,
                        renderState.FlipX,
                        renderState.FlipY,
                        renderState.MaskInteraction,
                        BattleSpriteMaterialSemantic.Unsupported);
                    break;
                case SignatureMutation.MaskInteraction:
                    renderState = new BattleSpriteRenderState(
                        renderState.Color,
                        renderState.FlipX,
                        renderState.FlipY,
                        SpriteMaskInteraction.VisibleInsideMask,
                        renderState.MaterialSemantic);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mutation), mutation, null);
            }

            BattleSpriteValueDescriptor descriptor = hasLogicalKey
                ? new BattleSpriteValueDescriptor(
                    requiresSprite,
                    hasSprite,
                    spriteInstanceId,
                    textureInstanceId,
                    materialInstanceId,
                    pixelRect,
                    descriptorPivot,
                    logicalKey)
                : new BattleSpriteValueDescriptor(
                    requiresSprite,
                    hasSprite,
                    spriteInstanceId,
                    textureInstanceId,
                    materialInstanceId,
                    pixelRect,
                    descriptorPivot);
            return CloneCommand(
                command,
                descriptor,
                visualDataId,
                effectivePic,
                size,
                commandPivot,
                normalizedUv,
                renderState);
        }

        private static BattleRenderCommand CloneCommand(
            in BattleRenderCommand command,
            BattleSpriteValueDescriptor descriptor,
            int? visualDataId = null,
            int? effectivePic = null,
            Vector2? size = null,
            Vector2? pivot = null,
            Rect? normalizedUv = null,
            BattleSpriteRenderState? renderState = null)
        {
            return new BattleRenderCommand(
                command.Type,
                command.Handle,
                command.StableId,
                visualDataId ?? command.VisualDataId,
                effectivePic ?? command.EffectivePic,
                command.ZInt,
                command.RuntimeSlot,
                command.SortOrder,
                command.SortingLayerId,
                command.LocalSequence,
                command.Position,
                size ?? command.Size,
                pivot ?? command.Pivot,
                normalizedUv ?? command.NormalizedUv,
                renderState ?? command.RenderState,
                descriptor);
        }

        private static BattleRenderCommand CreateTrustedCommandWithIdentity(
            in BattleRenderCommand source,
            object trustedResourceIdentity)
        {
            ConstructorInfo constructor = typeof(BattleRenderCommand).GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[]
                {
                    typeof(BattleRenderCommandType),
                    typeof(RuntimeEntityHandle),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(Vector3),
                    typeof(Vector2),
                    typeof(Vector2),
                    typeof(Rect),
                    typeof(BattleSpriteRenderState),
                    typeof(BattleSpriteValueDescriptor),
                    typeof(object),
                    typeof(bool),
                    typeof(int),
                    typeof(int),
                    typeof(int),
                    typeof(Vector2),
                    typeof(bool),
                    typeof(Vector2),
                    typeof(bool),
                    typeof(bool),
                    typeof(float),
                    typeof(BattlePresentationMotionAnchor),
                },
                null);
            Assert.That(constructor, Is.Not.Null);
            return (BattleRenderCommand)constructor.Invoke(
                new object[]
                {
                    source.Type,
                    source.Handle,
                    source.StableId,
                    source.VisualDataId,
                    source.EffectivePic,
                    source.ZInt,
                    source.RuntimeSlot,
                    source.SortOrder,
                    source.SortingLayerId,
                    source.LocalSequence,
                    source.Position,
                    source.Size,
                    source.Pivot,
                    source.NormalizedUv,
                    source.RenderState,
                    source.SpriteDescriptor,
                    trustedResourceIdentity,
                    source.ShowOverheadHealthBar,
                    source.CurrentHealth,
                    source.RecoverableHealth,
                    source.MaximumHealth,
                    source.StableHealthAnchorWorld,
                    source.HasStableHealthAnchor,
                    source.StableFootAnchorWorld,
                    source.HasStableFootAnchor,
                    source.ShowSelfFootMarker,
                    source.FootMarkerScale,
                    source.MotionAnchor,
                });
        }

        private enum SignatureMutation
        {
            MissingLogicalKey,
            LogicalKey,
            VisualDataId,
            EffectivePic,
            RequiresSprite,
            HasSprite,
            SpriteInstanceId,
            TextureInstanceId,
            MaterialInstanceId,
            PixelRect,
            DescriptorPivot,
            CommandPivot,
            NormalizedUv,
            Size,
            MaterialSemantic,
            MaskInteraction,
        }

        private sealed class ResolverFixture : IDisposable
        {
            public const int InvalidBindingVisualDataId = 18;

            private static readonly ConstructorInfo EntryWithCentralBindingConstructor =
                typeof(BattleSpriteEntry).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleSpriteKey),
                        typeof(string),
                        typeof(Texture2D),
                        typeof(Rect),
                        typeof(Vector2),
                        typeof(Sprite),
                        typeof(BattleSpriteCentralBinding),
                    },
                    null);
            private static readonly ConstructorInfo CatalogConstructor =
                typeof(BattleSpriteCatalog).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(IDictionary<BattleSpriteKey, BattleSpriteEntry>),
                    },
                    null);
            private readonly Texture2D texture;
            private readonly Texture2DArray arrayTexture;
            private readonly Sprite sourceSprite;
            private readonly Sprite arraySprite;

            public ResolverFixture(bool includeArrayBinding, string name = "resolver-fixture")
            {
                texture = new Texture2D(8, 8, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = $"{name}-texture",
                };
                arrayTexture = new Texture2DArray(8, 8, 1, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = $"{name}-array",
                };
                sourceSprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0f),
                    1f);
                sourceSprite.hideFlags = HideFlags.HideAndDontSave;
                sourceSprite.name = $"{name}-source-sprite";
                arraySprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0f),
                    1f);
                arraySprite.hideFlags = HideFlags.HideAndDontSave;
                arraySprite.name = $"{name}-array-sprite";
                Shader textureShader = Shader.Find(BattleSpriteMaterialContract.CentralTextureShaderName);
                Shader arrayShader = Shader.Find(BattleSpriteMaterialContract.CentralArrayShaderName);
                Assert.That(textureShader, Is.Not.Null);
                Assert.That(arrayShader, Is.Not.Null);

                Valid2DMaterial = CreateMaterial(textureShader);
                AlternateValid2DMaterial = CreateMaterial(textureShader);
                Invalid2DMaterial = CreateMaterial(arrayShader);
                ValidArrayMaterial = CreateMaterial(arrayShader);
                AlternateValidArrayMaterial = CreateMaterial(arrayShader);
                InvalidArrayMaterial = CreateMaterial(textureShader);
                Catalog = CreateCatalog(includeArrayBinding);
                Resolver = new BattleCatalogCentralResourceResolver();
            }

            public BattleSpriteCatalog Catalog { get; }
            public BattleCatalogCentralResourceResolver Resolver { get; }
            public Material Valid2DMaterial { get; }
            public Material AlternateValid2DMaterial { get; }
            public Material Invalid2DMaterial { get; }
            public Material ValidArrayMaterial { get; }
            public Material AlternateValidArrayMaterial { get; }
            public Material InvalidArrayMaterial { get; }
            public Texture2D SourceTexture => texture;

            public BattleRenderCommand CreateCommand(
                int effectivePic,
                Color32? color = null)
            {
                return CreateCommand(Catalog, 17, effectivePic, color);
            }

            public BattleRenderCommand CreateCommand(
                BattleSpriteCatalog sourceCatalog,
                int visualDataId,
                int effectivePic,
                Color32? color = null)
            {
                Assert.That(
                    sourceCatalog.TryGet(visualDataId, effectivePic, out BattleSpriteEntry entry),
                    Is.True);
                Color32 commandColor = color ?? new Color32(255, 255, 255, 255);
                var renderState = new BattleSpriteRenderState(
                    commandColor,
                    false,
                    false,
                    SpriteMaskInteraction.None,
                    BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
                return new BattleRenderCommand(
                    BattleRenderCommandType.Entity,
                    new RuntimeEntityHandle(effectivePic, 1),
                    effectivePic + 1,
                    visualDataId,
                    effectivePic,
                    0,
                    effectivePic,
                    0,
                    0,
                    effectivePic,
                    Vector3.zero,
                    new Vector2(entry.PixelWidth, entry.PixelHeight),
                    entry.Pivot,
                    entry.NormalizedUv,
                    renderState,
                    new BattleSpriteValueDescriptor(
                        true,
                        entry.LegacySprite != null,
                        entry.LegacySprite != null ? entry.LegacySprite.GetInstanceID() : 0,
                        entry.SharedTexture != null ? entry.SharedTexture.GetInstanceID() : 0,
                        0,
                        entry.PixelRect,
                        entry.Pivot,
                        BattleVisualResourceKey.FromEntity(entry.Key)));
            }

            public BattleRenderCommand CreateTrustedCommand(
                int effectivePic,
                Color32 color)
            {
                Assert.That(
                    Catalog.TryGet(17, effectivePic, out BattleSpriteEntry entry),
                    Is.True);
                return CreateTrustedCommand(
                    CreateCommand(effectivePic, color),
                    entry);
            }

            public BattleRenderCommand CreateMutatedTrustedCommand(
                int effectivePic,
                SignatureMutation mutation)
            {
                Assert.That(
                    Catalog.TryGet(17, effectivePic, out BattleSpriteEntry entry),
                    Is.True);
                BattleRenderCommand source = MutateEntityCommand(
                    CreateCommand(effectivePic),
                    mutation);
                return CreateTrustedCommand(source, entry);
            }

            private static BattleRenderCommand CreateTrustedCommand(
                in BattleRenderCommand source,
                BattleSpriteEntry entry)
            {
                return CreateTrustedCommandWithIdentity(source, entry);
            }

            public BattleSpriteCatalog CreateInvalidBindingCatalog()
            {
                Assert.That(EntryWithCentralBindingConstructor, Is.Not.Null);
                Assert.That(CatalogConstructor, Is.Not.Null);
                var key = new BattleSpriteKey(InvalidBindingVisualDataId, 0);
                var entry = (BattleSpriteEntry)EntryWithCentralBindingConstructor.Invoke(
                    new object[]
                    {
                        key,
                        "invalid-binding",
                        texture,
                        new Rect(0f, 0f, 8f, 8f),
                        new Vector2(0.5f, 0f),
                        sourceSprite,
                        default(BattleSpriteCentralBinding),
                    });
                var entries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>
                {
                    [key] = entry,
                };
                return (BattleSpriteCatalog)CatalogConstructor.Invoke(new object[] { entries });
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(sourceSprite);
                UnityEngine.Object.DestroyImmediate(arraySprite);
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(arrayTexture);
                UnityEngine.Object.DestroyImmediate(Valid2DMaterial);
                UnityEngine.Object.DestroyImmediate(AlternateValid2DMaterial);
                UnityEngine.Object.DestroyImmediate(Invalid2DMaterial);
                UnityEngine.Object.DestroyImmediate(ValidArrayMaterial);
                UnityEngine.Object.DestroyImmediate(AlternateValidArrayMaterial);
                UnityEngine.Object.DestroyImmediate(InvalidArrayMaterial);
            }

            private BattleSpriteCatalog CreateCatalog(bool includeArrayBinding)
            {
                var builder = new BattleSpriteCatalogBuilder();
                builder.Add(
                    17,
                    0,
                    "resolver-fixture",
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    sourceSprite);
                if (!includeArrayBinding)
                    return builder.Publish();

                builder.Add(
                    17,
                    1,
                    "resolver-fixture",
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    arraySprite);
                BattleSpriteCatalog sourceCatalog = builder.Publish();
                var bindings = new Dictionary<BattleSpriteKey, BattleSpriteCentralBinding>
                {
                    [new BattleSpriteKey(17, 0)] = new BattleSpriteCentralBinding(
                        BattleSpriteCentralBindingMode.SourceTexture2D,
                        texture,
                        0,
                        new Rect(0f, 0f, 1f, 1f),
                        new Rect(0f, 0f, 8f, 8f)),
                    [new BattleSpriteKey(17, 1)] = new BattleSpriteCentralBinding(
                        BattleSpriteCentralBindingMode.AtlasTextureArray,
                        arrayTexture,
                        0,
                        new Rect(0f, 0f, 1f, 1f),
                        new Rect(0f, 0f, 8f, 8f),
                        0),
                };
                MethodInfo withCentralBindings = typeof(BattleSpriteCatalog).GetMethod(
                    "WithCentralBindings",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.That(withCentralBindings, Is.Not.Null);
                return (BattleSpriteCatalog)withCentralBindings.Invoke(sourceCatalog, new object[] { bindings });
            }

            private static Material CreateMaterial(Shader shader)
            {
                var material = new Material(shader)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                material.SetColor("_Color", Color.white);
                return material;
            }
        }

        private sealed class CommonFixture : IDisposable
        {
            private const int SparkPic = 3;
            private const int WordSheetIndex = 2;
            private const int WordCharCode = 65;

            private static readonly ConstructorInfo BindingConstructor =
                typeof(BattleCommonVisualBinding).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleVisualResourceKey),
                        typeof(Sprite),
                        typeof(Texture2D),
                        typeof(Material),
                        typeof(Rect),
                        typeof(Rect),
                        typeof(Vector2),
                        typeof(Vector2),
                        typeof(BattleSpriteRenderState),
                        typeof(BattleSpriteCentralBinding),
                    },
                    null);
            private static readonly ConstructorInfo CatalogConstructor =
                typeof(BattleCommonVisualCatalog).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleCommonVisualBinding),
                        typeof(BattleCommonVisualBinding[]),
                        typeof(Texture2D[]),
                        typeof(BattleCommonVisualBinding[][]),
                        typeof(string),
                    },
                    null);
            private static readonly ConstructorInfo CatalogWithSpecialConstructor =
                typeof(BattleCommonVisualCatalog).GetConstructor(
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[]
                    {
                        typeof(BattleCommonVisualBinding),
                        typeof(BattleCommonVisualBinding[]),
                        typeof(Texture2D[]),
                        typeof(BattleCommonVisualBinding[][]),
                        typeof(BattleCommonVisualBinding),
                        typeof(string),
                    },
                    null);

            private readonly Texture2D sparkTexture;
            private readonly Texture2D wordTexture;
            private readonly Texture2D specialComTexture;
            private readonly Sprite shadowSprite;
            private readonly Sprite sparkSprite;
            private readonly Sprite wordSprite;
            private readonly Sprite specialComSprite;
            private readonly BattleCommonVisualBinding shadowBinding;
            private readonly BattleCommonVisualBinding sparkBinding;
            private readonly BattleCommonVisualBinding wordBinding;
            private readonly BattleCommonVisualBinding specialComBinding;

            public CommonFixture(string name)
            {
                Assert.That(BindingConstructor, Is.Not.Null);
                Assert.That(CatalogConstructor, Is.Not.Null);

                Texture = CreateTexture($"{name}-shadow");
                sparkTexture = CreateTexture($"{name}-spark");
                wordTexture = CreateTexture($"{name}-word");
                specialComTexture = CreateTexture($"{name}-special-com");
                shadowSprite = CreateSprite(Texture, $"{name}-shadow-sprite");
                sparkSprite = CreateSprite(sparkTexture, $"{name}-spark-sprite");
                wordSprite = CreateSprite(wordTexture, $"{name}-word-sprite");
                specialComSprite = CreateSprite(
                    specialComTexture,
                    $"{name}-special-com-sprite");
                shadowBinding = CreateBinding(
                    BattleVisualResourceKey.CommonShadow,
                    shadowSprite,
                    Texture);
                sparkBinding = CreateBinding(
                    BattleVisualResourceKey.CommonSpark(SparkPic),
                    sparkSprite,
                    sparkTexture);
                wordBinding = CreateBinding(
                    BattleVisualResourceKey.CommonWordGlyph(WordSheetIndex, WordCharCode),
                    wordSprite,
                    wordTexture);
                specialComBinding = CreateBinding(
                    BattleVisualResourceKey.CommonSpecialCom,
                    specialComSprite,
                    specialComTexture);

                Catalog = CreateCatalog(
                    includeShadow: true,
                    completeSparks: false,
                    completeWords: false);
            }

            public BattleCommonVisualCatalog Catalog { get; }
            public Texture2D Texture { get; }

            public BattleCommonVisualCatalog CreateCatalog(
                bool includeShadow,
                bool completeSparks,
                bool completeWords,
                bool includeSpecialCom = false)
            {
                var sparks = new BattleCommonVisualBinding[
                    BattleCommonVisualCatalog.SparkFrameCount];
                if (completeSparks)
                {
                    for (int pic = 0; pic < sparks.Length; pic++)
                        sparks[pic] = sparkBinding;
                }
                else
                {
                    sparks[SparkPic] = sparkBinding;
                }

                var wordTextures = new Texture2D[BattleCommonVisualCatalog.WordSheetCount];
                var words = new BattleCommonVisualBinding[
                    BattleCommonVisualCatalog.WordSheetCount][];
                for (int sheetIndex = 0;
                     sheetIndex < BattleCommonVisualCatalog.WordSheetCount;
                     sheetIndex++)
                {
                    if (completeWords)
                    {
                        wordTextures[sheetIndex] = wordTexture;
                        words[sheetIndex] = new BattleCommonVisualBinding[
                            BattleCommonVisualCatalog.WordGlyphsPerSheet];
                        for (int charCode = 0;
                             charCode < BattleCommonVisualCatalog.WordGlyphsPerSheet;
                             charCode++)
                        {
                            words[sheetIndex][charCode] = wordBinding;
                        }
                    }
                    else
                    {
                        words[sheetIndex] = sheetIndex == WordSheetIndex
                            ? new BattleCommonVisualBinding[
                                BattleCommonVisualCatalog.WordGlyphsPerSheet]
                            : Array.Empty<BattleCommonVisualBinding>();
                    }
                }
                if (!completeWords)
                {
                    wordTextures[WordSheetIndex] = wordTexture;
                    words[WordSheetIndex][WordCharCode] = wordBinding;
                }

                if (!includeSpecialCom)
                {
                    return (BattleCommonVisualCatalog)CatalogConstructor.Invoke(
                        new object[]
                        {
                            includeShadow ? shadowBinding : null,
                            sparks,
                            wordTextures,
                            words,
                            string.Empty,
                        });
                }

                Assert.That(CatalogWithSpecialConstructor, Is.Not.Null);
                return (BattleCommonVisualCatalog)CatalogWithSpecialConstructor.Invoke(
                    new object[]
                    {
                        includeShadow ? shadowBinding : null,
                        sparks,
                        wordTextures,
                        words,
                        specialComBinding,
                        string.Empty,
                    });
            }

            public BattleRenderCommand CreateShadowCommand()
            {
                return CreateCommand(
                    BattleRenderCommandType.Shadow,
                    -1,
                    -1,
                    shadowBinding);
            }

            public BattleRenderCommand CreateSparkCommand()
            {
                return CreateCommand(
                    BattleRenderCommandType.HitRecord,
                    -1,
                    SparkPic,
                    sparkBinding);
            }

            public BattleRenderCommand CreateWordCommand()
            {
                return CreateCommand(
                    BattleRenderCommandType.OverlayGlyph,
                    WordSheetIndex,
                    WordCharCode,
                    wordBinding);
            }

            public BattleRenderCommand CreateSpecialComCommand()
            {
                BattleRenderCommand source = CreateCommand(
                    BattleRenderCommandType.OverlayGlyph,
                    5,
                    'C',
                    specialComBinding);
                return CreateTrustedCommandWithIdentity(source, specialComBinding);
            }

            public void Dispose()
            {
                UnityEngine.Object.DestroyImmediate(shadowSprite);
                UnityEngine.Object.DestroyImmediate(sparkSprite);
                UnityEngine.Object.DestroyImmediate(wordSprite);
                UnityEngine.Object.DestroyImmediate(specialComSprite);
                UnityEngine.Object.DestroyImmediate(Texture);
                UnityEngine.Object.DestroyImmediate(sparkTexture);
                UnityEngine.Object.DestroyImmediate(wordTexture);
                UnityEngine.Object.DestroyImmediate(specialComTexture);
            }

            private static Texture2D CreateTexture(string name)
            {
                return new Texture2D(8, 8, TextureFormat.RGBA32, false)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    name = name,
                };
            }

            private static Sprite CreateSprite(Texture2D texture, string name)
            {
                Sprite sprite = Sprite.Create(
                    texture,
                    new Rect(0f, 0f, 8f, 8f),
                    new Vector2(0.5f, 0.5f),
                    1f);
                sprite.hideFlags = HideFlags.HideAndDontSave;
                sprite.name = name;
                return sprite;
            }

            private static BattleCommonVisualBinding CreateBinding(
                BattleVisualResourceKey key,
                Sprite sprite,
                Texture2D texture)
            {
                var pixelRect = new Rect(0f, 0f, 8f, 8f);
                var normalizedUv = new Rect(0f, 0f, 1f, 1f);
                var pivot = new Vector2(0.5f, 0.5f);
                var centralBinding = new BattleSpriteCentralBinding(
                    BattleSpriteCentralBindingMode.SourceTexture2D,
                    texture,
                    0,
                    normalizedUv,
                    pixelRect);
                return (BattleCommonVisualBinding)BindingConstructor.Invoke(
                    new object[]
                    {
                        key,
                        sprite,
                        texture,
                        null,
                        pixelRect,
                        normalizedUv,
                        pixelRect.size,
                        pivot,
                        BattleSpriteRenderState.Default(),
                        centralBinding,
                    });
            }

            private static BattleRenderCommand CreateCommand(
                BattleRenderCommandType type,
                int visualDataId,
                int effectivePic,
                BattleCommonVisualBinding binding)
            {
                return new BattleRenderCommand(
                    type,
                    RuntimeEntityHandle.Invalid,
                    1,
                    visualDataId,
                    effectivePic,
                    0,
                    0,
                    0,
                    0,
                    0,
                    Vector3.zero,
                    binding.PixelSize,
                    binding.Pivot,
                    binding.NormalizedUv,
                    binding.RenderState,
                    new BattleSpriteValueDescriptor(
                        true,
                        true,
                        binding.SpriteInstanceId,
                        binding.TextureInstanceId,
                        binding.MaterialInstanceId,
                        binding.PixelRect,
                        binding.Pivot,
                        binding.Key));
            }

        }
    }
}
#endif
