#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleDynamicMeshBackendSegmentBoundsEditorTests
    {
        [Test]
        public void AccumulatedBounds_MatchLegacyVertexScanAcrossRandomSegments()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new VariantResolver();
            var random = new System.Random(0x51E6B0);

            FrameAccess.Reset(frame, 1);
            int variant = 0;
            int runRemaining = 0;
            for (int index = 0; index < 512; index++)
            {
                if (runRemaining <= 0)
                {
                    variant = random.Next(0, 8);
                    runRemaining = random.Next(1, 13);
                }
                runRemaining--;
                FrameAccess.AddCommand(frame, CreateCommand(
                    index,
                    variant,
                    new Vector3(
                        RandomRange(random, -700f, 700f),
                        RandomRange(random, -400f, 400f),
                        RandomRange(random, -20f, 20f)),
                    new Vector2(
                        RandomSignedExtent(random, 140f),
                        RandomSignedExtent(random, 180f)),
                    new Vector2(
                        RandomRange(random, -0.5f, 1.5f),
                        RandomRange(random, -0.5f, 1.5f))));
            }

            backend.Build(frame, resolver);
            Assert.That(backend.SegmentCount, Is.GreaterThan(1));
            AssertAllSegmentBoundsMatchLegacy(frame, backend);
        }

        [Test]
        public void AccumulatedBounds_ResetAcrossFramesChunksAndEmptyClear()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new VariantResolver();

            FrameAccess.Reset(frame, 1);
            for (int index = 0;
                 index < BattleDynamicMeshBackend.QuadsPerChunk + 1;
                 index++)
            {
                FrameAccess.AddCommand(frame, CreateCommand(
                    index,
                    index < BattleDynamicMeshBackend.QuadsPerChunk ? 0 : 1,
                    new Vector3(index * 0.25f, index % 17, index % 5),
                    new Vector2(4f + index % 3, 7f + index % 5),
                    new Vector2(0.5f, 0.25f)));
            }

            backend.Build(frame, resolver);
            Assert.That(backend.ActiveChunkCount, Is.EqualTo(2));
            AssertAllSegmentBoundsMatchLegacy(frame, backend);
            Mesh firstChunk = backend.GetChunkMesh(0);
            Mesh secondChunk = backend.GetChunkMesh(1);

            FrameAccess.Reset(frame, 2);
            FrameAccess.AddCommand(frame, CreateCommand(
                0,
                3,
                new Vector3(-900f, 350f, -8f),
                new Vector2(33f, 41f),
                new Vector2(1.25f, -0.25f)));
            FrameAccess.AddCommand(frame, CreateCommand(
                1,
                3,
                new Vector3(-850f, 290f, 12f),
                new Vector2(19f, 27f),
                new Vector2(-0.5f, 1.5f)));
            backend.Build(frame, resolver);
            Assert.That(backend.ActiveChunkCount, Is.EqualTo(1));
            Assert.That(backend.SegmentCount, Is.EqualTo(1));
            AssertAllSegmentBoundsMatchLegacy(frame, backend);
            Assert.That(secondChunk.bounds.size, Is.EqualTo(Vector3.zero));
            AssertInertDescriptors(secondChunk);

            FrameAccess.Reset(frame, 3);
            backend.Build(frame, resolver);
            Assert.That(backend.ActiveChunkCount, Is.Zero);
            Assert.That(firstChunk.bounds.size, Is.EqualTo(Vector3.zero));
            Assert.That(secondChunk.bounds.size, Is.EqualTo(Vector3.zero));
            AssertInertDescriptors(firstChunk);
            AssertInertDescriptors(secondChunk);

            FrameAccess.Reset(frame, 4);
            FrameAccess.AddCommand(frame, CreateCommand(
                0,
                7,
                new Vector3(25f, -40f, 3f),
                new Vector2(8f, 6f),
                new Vector2(0f, 0f)));
            backend.Build(frame, resolver);
            AssertAllSegmentBoundsMatchLegacy(frame, backend);
        }

        [Test]
        public void AccumulatedBounds_StrictModeKeepsOneExactBoundsPerQuad()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new VariantResolver();

            FrameAccess.Reset(frame, 1);
            for (int index = 0; index < 64; index++)
            {
                FrameAccess.AddCommand(frame, CreateCommand(
                    index,
                    0,
                    new Vector3(index * 2f, -index, index % 9),
                    new Vector2(index + 1f, index * 0.5f + 2f),
                    new Vector2(index % 3 * 0.5f, index % 5 * 0.25f)));
            }

            backend.Build(
                frame,
                resolver,
                BattleCentralDrawMode.StrictOrderedDraw);
            Assert.That(backend.SegmentCount, Is.EqualTo(frame.CommandCount));
            AssertAllSegmentBoundsMatchLegacy(frame, backend);
        }

        [Test]
        public void AccumulatedBounds_SteadyStateAddsNoManagedAllocation()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new VariantResolver();
            FrameAccess.Reset(frame, 1);
            for (int index = 0; index < 128; index++)
            {
                FrameAccess.AddCommand(frame, CreateCommand(
                    index,
                    index / 8,
                    new Vector3(index, index % 11, index % 7),
                    new Vector2(12f, 16f),
                    new Vector2(0.5f, 0.5f)));
            }

            for (int warmup = 0; warmup < 8; warmup++)
                backend.Build(frame, resolver);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 64; iteration++)
                backend.Build(frame, resolver);
            long after = GC.GetAllocatedBytesForCurrentThread();

            Assert.That(after - before, Is.EqualTo(0));
        }

        private static void AssertAllSegmentBoundsMatchLegacy(
            BattlePresentationFrame frame,
            BattleDynamicMeshBackend backend)
        {
            for (int segmentIndex = 0; segmentIndex < backend.SegmentCount; segmentIndex++)
            {
                BattleCentralRenderSegment segment = backend.GetSegment(segmentIndex);
                Mesh mesh = backend.GetChunkMesh(segment.ChunkIndex);
                Bounds actual = mesh.GetSubMesh(segment.SubMeshIndex).bounds;
                Bounds expected = CalculateLegacyBounds(frame, segment);
                AssertBoundsEqual(actual, expected, segmentIndex);
            }
        }

        private static Bounds CalculateLegacyBounds(
            BattlePresentationFrame frame,
            in BattleCentralRenderSegment segment)
        {
            bool hasBounds = false;
            Vector3 min = default;
            Vector3 max = default;
            int endCommand = segment.FirstCommandIndex + segment.CommandCount;
            for (int commandIndex = segment.FirstCommandIndex;
                 commandIndex < endCommand;
                 commandIndex++)
            {
                BattleRenderCommand command = frame.GetCommand(commandIndex);
                float width = command.Size.x *
                              NTSDRenderSpace.UnitsPerPixelX *
                              NTSDRenderSpace.BattleVisualScale;
                float height = command.Size.y *
                               NTSDRenderSpace.UnitsPerPixelY *
                               NTSDRenderSpace.BattleVisualScale;
                float left = command.Position.x - command.Pivot.x * width;
                float right = left + width;
                float bottom = command.Position.y - command.Pivot.y * height;
                float top = bottom + height;
                Vector3 firstCorner = new Vector3(left, bottom, command.Position.z);
                Vector3 secondCorner = new Vector3(right, top, command.Position.z);
                Vector3 quadMin = Vector3.Min(firstCorner, secondCorner);
                Vector3 quadMax = Vector3.Max(firstCorner, secondCorner);
                if (!hasBounds)
                {
                    min = quadMin;
                    max = quadMax;
                    hasBounds = true;
                }
                else
                {
                    min = Vector3.Min(min, quadMin);
                    max = Vector3.Max(max, quadMax);
                }
            }

            var result = new Bounds();
            if (hasBounds)
                result.SetMinMax(min, max);
            return result;
        }

        private static void AssertBoundsEqual(
            Bounds actual,
            Bounds expected,
            int segmentIndex)
        {
            const float epsilon = 0.00001f;
            Assert.That(actual.min.x, Is.EqualTo(expected.min.x).Within(epsilon),
                $"segment {segmentIndex} min.x");
            Assert.That(actual.min.y, Is.EqualTo(expected.min.y).Within(epsilon),
                $"segment {segmentIndex} min.y");
            Assert.That(actual.min.z, Is.EqualTo(expected.min.z).Within(epsilon),
                $"segment {segmentIndex} min.z");
            Assert.That(actual.max.x, Is.EqualTo(expected.max.x).Within(epsilon),
                $"segment {segmentIndex} max.x");
            Assert.That(actual.max.y, Is.EqualTo(expected.max.y).Within(epsilon),
                $"segment {segmentIndex} max.y");
            Assert.That(actual.max.z, Is.EqualTo(expected.max.z).Within(epsilon),
                $"segment {segmentIndex} max.z");
        }

        private static void AssertInertDescriptors(Mesh mesh)
        {
            for (int subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
            {
                SubMeshDescriptor descriptor = mesh.GetSubMesh(subMesh);
                Assert.That(mesh.GetIndexCount(subMesh), Is.Zero);
                Assert.That(descriptor.vertexCount, Is.Zero);
                Assert.That(descriptor.bounds.size, Is.EqualTo(Vector3.zero));
            }
        }

        private static BattleRenderCommand CreateCommand(
            int sequence,
            int variant,
            Vector3 position,
            Vector2 size,
            Vector2 pivot)
        {
            return new BattleRenderCommand(
                BattleRenderCommandType.Entity,
                RuntimeEntityHandle.Invalid,
                sequence,
                variant,
                sequence,
                0,
                sequence,
                sequence,
                0,
                sequence,
                position,
                size,
                pivot,
                new Rect(0f, 0f, 1f, 1f),
                false,
                default);
        }

        private static float RandomRange(System.Random random, float min, float max)
        {
            return min + (float)random.NextDouble() * (max - min);
        }

        private static float RandomSignedExtent(System.Random random, float max)
        {
            float magnitude = RandomRange(random, 0.25f, max);
            return random.Next(0, 2) == 0 ? magnitude : -magnitude;
        }

        private sealed class VariantResolver : IBattleCentralResourceResolver
        {
            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(
                    null,
                    null,
                    command.NormalizedUv,
                    command.Size,
                    command.Pivot,
                    Color.white,
                    command.VisualDataId);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private static class FrameAccess
        {
            private delegate void ResetDelegate(
                BattlePresentationFrame frame,
                int tickIndex,
                BattleCommonVisualCatalog commonVisualCatalog);
            private delegate void AddCommandDelegate(
                BattlePresentationFrame frame,
                in BattleRenderCommand command);

            private static readonly ResetDelegate ResetMethod =
                (ResetDelegate)typeof(BattlePresentationFrame)
                    .GetMethod(
                        "Reset",
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(int), typeof(BattleCommonVisualCatalog) },
                        null)
                    .CreateDelegate(typeof(ResetDelegate));

            private static readonly AddCommandDelegate AddCommandMethod =
                (AddCommandDelegate)typeof(BattlePresentationFrame)
                    .GetMethod(
                        "AddCommand",
                        BindingFlags.Instance | BindingFlags.NonPublic,
                        null,
                        new[] { typeof(BattleRenderCommand).MakeByRefType() },
                        null)
                    .CreateDelegate(typeof(AddCommandDelegate));

            public static void Reset(BattlePresentationFrame frame, int tickIndex)
            {
                ResetMethod(frame, tickIndex, null);
            }

            public static void AddCommand(
                BattlePresentationFrame frame,
                in BattleRenderCommand command)
            {
                AddCommandMethod(frame, command);
            }
        }
    }
}
#endif
