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
    public sealed class BattleDynamicMeshBackendSubMeshEditorTests
    {
        [Test]
        public void Upload_PreservesDescriptorsAndPhysicalHighWaterAcrossTailTransitions()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new DescriptorResolver();

            BuildFrame(frame, 1, 1, false);
            backend.Build(frame, resolver);
            Mesh mesh = backend.GetChunkMesh(0);
            Assert.That(mesh.subMeshCount, Is.EqualTo(1));
            AssertActiveDescriptor(mesh, 0, 0, 1);

            BuildFrame(frame, 2, 32, true);
            backend.Build(frame, resolver);
            Assert.That(mesh.subMeshCount, Is.EqualTo(32));
            AssertActivePrefix(mesh, 32);

            BuildFrame(frame, 3, 1, false);
            backend.Build(frame, resolver);
            Assert.That(mesh.subMeshCount, Is.EqualTo(32));
            AssertActiveDescriptor(mesh, 0, 0, 1);
            AssertInertTail(mesh, 1);

            BuildFrame(frame, 4, 33, true);
            backend.Build(frame, resolver);
            Assert.That(mesh.subMeshCount, Is.EqualTo(33));
            AssertActivePrefix(mesh, 33);

            BuildFrame(frame, 5, 1, false);
            backend.Build(frame, resolver);
            Assert.That(mesh.subMeshCount, Is.EqualTo(33));
            AssertActiveDescriptor(mesh, 0, 0, 1);
            AssertInertTail(mesh, 1);

            BuildFrame(frame, 6, 0, false);
            backend.Build(frame, resolver);
            Assert.That(backend.ActiveChunkCount, Is.Zero);
            Assert.That(mesh.subMeshCount, Is.EqualTo(33));
            AssertInertTail(mesh, 0);
            Assert.That(mesh.bounds.size, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void Upload_HandlesPhysicalSubMeshCountBelowPreviousActiveCount()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new DescriptorResolver();

            BuildFrame(frame, 1, 1, false);
            backend.Build(frame, resolver);
            Mesh mesh = backend.GetChunkMesh(0);
            Assert.That(mesh.subMeshCount, Is.EqualTo(1));

            // Do not physically shrink an initialized Unity mesh: Unity validates
            // removed native descriptors and reports an invalid MinMaxAABB before
            // Upload can execute. Simulate only the stale managed high-water that
            // the production clamp must tolerate while preserving a valid mesh.
            BackendAccess.SetChunkActiveSubMeshCount(backend, 0, 32);
            BuildFrame(frame, 2, 1, false);
            Assert.DoesNotThrow(() => backend.Build(frame, resolver));
            Assert.That(mesh.subMeshCount, Is.EqualTo(1));
            AssertActiveDescriptor(mesh, 0, 0, 1);

            BackendAccess.SetChunkActiveSubMeshCount(backend, 0, 32);
            BuildFrame(frame, 3, 0, false);
            Assert.DoesNotThrow(() => backend.Build(frame, resolver));
            Assert.That(mesh.subMeshCount, Is.EqualTo(1));
            AssertInertTail(mesh, 0);
        }

        [Test]
        public void Upload_PreservesChunkRangesAndRecoversDestroyedMesh()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new DescriptorResolver();

            int[] counts = { 4095, 4096, 4097 };
            for (int index = 0; index < counts.Length; index++)
            {
                int commandCount = counts[index];
                BuildFrame(frame, index + 1, commandCount, false);
                backend.Build(frame, resolver);

                int expectedChunkCount = (commandCount + BattleDynamicMeshBackend.QuadsPerChunk - 1) /
                                         BattleDynamicMeshBackend.QuadsPerChunk;
                Assert.That(backend.ActiveChunkCount, Is.EqualTo(expectedChunkCount));
                for (int chunkIndex = 0; chunkIndex < expectedChunkCount; chunkIndex++)
                {
                    int firstQuad = chunkIndex * BattleDynamicMeshBackend.QuadsPerChunk;
                    int quadCount = Math.Min(
                        BattleDynamicMeshBackend.QuadsPerChunk,
                        commandCount - firstQuad);
                    Mesh mesh = backend.GetChunkMesh(chunkIndex);
                    Assert.That(mesh.subMeshCount, Is.EqualTo(1));
                    AssertActiveDescriptor(mesh, 0, 0, quadCount);
                }
            }

            Mesh firstMesh = backend.GetChunkMesh(0);
            Mesh secondMesh = backend.GetChunkMesh(1);
            AssertIndexBufferStateForDevice(firstMesh, BattleDynamicMeshBackend.IndicesPerChunk);
            AssertIndexBufferStateForDevice(secondMesh, BattleDynamicMeshBackend.IndicesPerQuad);

            BuildFrame(frame, 9, 0, false);
            backend.Build(frame, resolver);
            Assert.That(backend.ActiveChunkCount, Is.Zero);
            AssertInertTail(firstMesh, 0);
            AssertInertTail(secondMesh, 0);

            BuildFrame(frame, 10, 4097, false);
            backend.Build(frame, resolver);
            AssertIndexBufferStateForDevice(firstMesh, BattleDynamicMeshBackend.IndicesPerChunk);
            AssertIndexBufferStateForDevice(secondMesh, BattleDynamicMeshBackend.IndicesPerQuad);
            UnityEngine.Object.DestroyImmediate(firstMesh);

            BuildFrame(frame, 11, 4097, false);
            backend.Build(frame, resolver);
            Mesh recoveredMesh = backend.GetChunkMesh(0);
            Assert.That(firstMesh == null, Is.True);
            Assert.That(recoveredMesh, Is.Not.Null);
            Assert.That(recoveredMesh, Is.Not.SameAs(firstMesh));
            Assert.That(recoveredMesh.subMeshCount, Is.EqualTo(1));
            AssertActiveDescriptor(recoveredMesh, 0, 0, 4096);
            Assert.That(secondMesh.subMeshCount, Is.EqualTo(1));
            AssertActiveDescriptor(secondMesh, 0, 0, 1);

            BuildFrame(frame, 11, 0, false);
            backend.Build(frame, resolver);
            Assert.That(recoveredMesh.subMeshCount, Is.EqualTo(1));
            Assert.That(secondMesh.subMeshCount, Is.EqualTo(1));
            AssertInertTail(recoveredMesh, 0);
            AssertInertTail(secondMesh, 0);
        }

        [TestCase(BattleCentralDrawMode.OrderedChunks)]
        [TestCase(BattleCentralDrawMode.StrictOrderedDraw)]
        public void Upload_StableMultipleDescriptorsMoveAndClearCachedTail(
            BattleCentralDrawMode drawMode)
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new DescriptorResolver();
            const int commandCount = 64;
            backend.PrepareCapacity(commandCount);

            BuildFrame(frame, 1, commandCount, true);
            backend.Build(frame, resolver, drawMode);
            Mesh mesh = backend.GetChunkMesh(0);
            int meshId = mesh.GetInstanceID();
            Bounds previousBounds = mesh.bounds;
            var previousDescriptors = new SubMeshDescriptor[commandCount];
            for (int index = 0; index < commandCount; index++)
                previousDescriptors[index] = mesh.GetSubMesh(index);
            Vector3[] previousVertices = mesh.vertices;
            Vector2[] previousUvs = mesh.uv;
            Color32[] previousColors = mesh.colors32;

            const float positionOffset = 100f;
            Vector3 displacement = new Vector3(positionOffset, -positionOffset, 0f);
            BuildFrame(frame, 2, commandCount, true, positionOffset);
            backend.Build(frame, resolver, drawMode);
            Assert.That(backend.GetChunkMesh(0), Is.SameAs(mesh));
            Assert.That(mesh.GetInstanceID(), Is.EqualTo(meshId));
            AssertVectorNear(mesh.bounds.center, previousBounds.center + displacement);
            AssertVectorNear(mesh.bounds.size, previousBounds.size);
            Assert.That(backend.SegmentCount, Is.EqualTo(commandCount));
            Vector3[] movedVertices = mesh.vertices;
            Assert.That(mesh.uv, Is.EqualTo(previousUvs));
            Assert.That(mesh.colors32, Is.EqualTo(previousColors));
            for (int index = 0; index < commandCount; index++)
            {
                SubMeshDescriptor previous = previousDescriptors[index];
                SubMeshDescriptor current = mesh.GetSubMesh(index);
                Assert.That(current.indexStart, Is.EqualTo(previous.indexStart));
                Assert.That(current.indexCount, Is.EqualTo(previous.indexCount));
                Assert.That(current.firstVertex, Is.EqualTo(previous.firstVertex));
                Assert.That(current.vertexCount, Is.EqualTo(previous.vertexCount));
                Assert.That(current.baseVertex, Is.EqualTo(previous.baseVertex));
                Assert.That(current.topology, Is.EqualTo(previous.topology));
                AssertVectorNear(current.bounds.center, previous.bounds.center + displacement);
                AssertVectorNear(current.bounds.size, previous.bounds.size);
                BattleCentralRenderSegment segment = backend.GetSegment(index);
                Assert.That(segment.ChunkIndex, Is.Zero);
                Assert.That(segment.SubMeshIndex, Is.EqualTo(index));
                Assert.That(segment.FirstCommandIndex, Is.EqualTo(index));
                Assert.That(segment.FirstQuad, Is.EqualTo(index));
                Assert.That(segment.QuadCount, Is.EqualTo(1));
                for (int corner = 0; corner < BattleDynamicMeshBackend.VerticesPerQuad; corner++)
                {
                    int vertex = index * BattleDynamicMeshBackend.VerticesPerQuad + corner;
                    AssertVectorNear(movedVertices[vertex], previousVertices[vertex] + displacement);
                }
            }

            BuildFrame(frame, 3, 2, true);
            backend.Build(frame, resolver, drawMode);
            AssertActivePrefix(mesh, 2);
            AssertInertTail(mesh, 2);
            Assert.That(mesh.subMeshCount, Is.EqualTo(commandCount));
            BuildFrame(frame, 4, 0, false);
            backend.Build(frame, resolver, drawMode);
            AssertInertTail(mesh, 0);
            BuildFrame(frame, 5, 3, true);
            backend.Build(frame, resolver, drawMode);
            AssertActivePrefix(mesh, 3);
            AssertInertTail(mesh, 3);
            BuildFrame(frame, 6, 3, true, positionOffset);
            backend.Build(frame, resolver, drawMode);
            AssertActivePrefix(mesh, 3);
            AssertInertTail(mesh, 3);
            BuildFrame(frame, 7, 1, false);
            backend.Build(frame, resolver, drawMode);
            AssertActivePrefix(mesh, 1);
            AssertInertTail(mesh, 1);
            Assert.That(mesh.subMeshCount, Is.EqualTo(commandCount));
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
            Assert.That(backend.Diagnostics.VertexUploadCallCount, Is.EqualTo(1));
            Assert.That(backend.Diagnostics.UploadedVertexBytes,
                Is.EqualTo(BattleDynamicMeshBackend.VerticesPerQuad * mesh.GetVertexBufferStride(0)));
        }

        [Test]
        public void Upload_StableSmallPrefixAfterHighWaterAllocatesZeroManagedBytes()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame();
            var resolver = new DescriptorResolver();
            backend.PrepareCapacity(64);
            BuildFrame(frame, 1, 64, true);
            backend.Build(frame, resolver);
            Mesh mesh = backend.GetChunkMesh(0);
            BuildFrame(frame, 2, 2, true);
            for (int warmup = 0; warmup < 8; warmup++)
                backend.Build(frame, resolver);

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 64; iteration++)
                backend.Build(frame, resolver);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero);
            Assert.That(backend.GetChunkMesh(0), Is.SameAs(mesh));
            Assert.That(mesh.subMeshCount, Is.EqualTo(64));
            AssertActivePrefix(mesh, 2);
            AssertInertTail(mesh, 2);
            Assert.That(backend.SegmentCount, Is.EqualTo(2));
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
        }

        private static void AssertVectorNear(Vector3 actual, Vector3 expected)
        {
            // Translation changes float rounding at the new coordinate magnitude.
            const float epsilon = 0.00002f;
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(epsilon));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(epsilon));
            Assert.That(actual.z, Is.EqualTo(expected.z).Within(epsilon));
        }

        private static void BuildFrame(
            BattlePresentationFrame frame,
            int tickIndex,
            int commandCount,
            bool alternating,
            float positionOffset = 0f)
        {
            FrameAccess.Reset(frame, tickIndex);
            for (int index = 0; index < commandCount; index++)
            {
                FrameAccess.AddCommand(frame, new BattleRenderCommand(
                    BattleRenderCommandType.Entity,
                    RuntimeEntityHandle.Invalid,
                    index,
                    alternating ? index & 1 : 0,
                    index,
                    0,
                    index,
                    index,
                    0,
                    index,
                    new Vector3(index + positionOffset, -positionOffset, 0f),
                    Vector2.one,
                    new Vector2(0.5f, 0.5f),
                    new Rect(0f, 0f, 1f, 1f),
                    false,
                    default));
            }
        }

        private static void AssertActivePrefix(Mesh mesh, int subMeshCount)
        {
            for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
                AssertActiveDescriptor(mesh, subMeshIndex, subMeshIndex, 1);
        }

        private static void AssertActiveDescriptor(Mesh mesh, int subMeshIndex, int firstQuad, int quadCount)
        {
            SubMeshDescriptor descriptor = mesh.GetSubMesh(subMeshIndex);
            Assert.That((int)mesh.GetIndexStart(subMeshIndex), Is.EqualTo(firstQuad * BattleDynamicMeshBackend.IndicesPerQuad));
            Assert.That((int)mesh.GetIndexCount(subMeshIndex), Is.EqualTo(quadCount * BattleDynamicMeshBackend.IndicesPerQuad));
            Assert.That(descriptor.baseVertex, Is.Zero);
            Assert.That(descriptor.firstVertex, Is.EqualTo(firstQuad * BattleDynamicMeshBackend.VerticesPerQuad));
            Assert.That(descriptor.vertexCount, Is.EqualTo(quadCount * BattleDynamicMeshBackend.VerticesPerQuad));
            Assert.That(descriptor.bounds.size, Is.Not.EqualTo(Vector3.zero));
        }

        private static void AssertInertTail(Mesh mesh, int firstInertSubMesh)
        {
            for (int subMeshIndex = firstInertSubMesh; subMeshIndex < mesh.subMeshCount; subMeshIndex++)
            {
                SubMeshDescriptor descriptor = mesh.GetSubMesh(subMeshIndex);
                Assert.That(mesh.GetIndexCount(subMeshIndex), Is.Zero);
                Assert.That(descriptor.vertexCount, Is.Zero);
                Assert.That(descriptor.bounds.size, Is.EqualTo(Vector3.zero));
            }
        }

        private static void AssertIndexBufferStateForDevice(Mesh mesh, int expectedActiveIndexCount)
        {
            Assert.That(mesh.GetIndexCount(0), Is.EqualTo(expectedActiveIndexCount));
            using GraphicsBuffer indexBuffer = mesh.GetIndexBuffer();
            Assert.That(indexBuffer, Is.Not.Null);
            Assert.That(indexBuffer.count, Is.EqualTo(BattleDynamicMeshBackend.IndicesPerChunk));
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                Assert.That(mesh.GetNativeIndexBufferPtr(), Is.EqualTo(IntPtr.Zero));
            else
                Assert.That(mesh.GetNativeIndexBufferPtr(), Is.Not.EqualTo(IntPtr.Zero));
        }

        private sealed class DescriptorResolver : IBattleCentralResourceResolver
        {
            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(
                    null,
                    null,
                    new Rect(0f, 0f, 1f, 1f),
                    Vector2.one,
                    new Vector2(0.5f, 0.5f),
                    Color.white,
                    command.VisualDataId);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private static class FrameAccess
        {
            private delegate void ResetDelegate(BattlePresentationFrame frame, int tickIndex, BattleCommonVisualCatalog commonVisualCatalog);
            private delegate void AddCommandDelegate(BattlePresentationFrame frame, in BattleRenderCommand command);

            private static readonly ResetDelegate ResetMethod = (ResetDelegate)typeof(BattlePresentationFrame)
                .GetMethod(
                    "Reset",
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    new[] { typeof(int), typeof(BattleCommonVisualCatalog) },
                    null)
                .CreateDelegate(typeof(ResetDelegate));

            private static readonly AddCommandDelegate AddCommandMethod = (AddCommandDelegate)typeof(BattlePresentationFrame)
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

            public static void AddCommand(BattlePresentationFrame frame, in BattleRenderCommand command)
            {
                AddCommandMethod(frame, command);
            }
        }

        private static class BackendAccess
        {
            private static readonly FieldInfo ChunksField = typeof(BattleDynamicMeshBackend)
                .GetField("chunks", BindingFlags.Instance | BindingFlags.NonPublic);

            public static void SetChunkActiveSubMeshCount(
                BattleDynamicMeshBackend backend,
                int chunkIndex,
                int activeSubMeshCount)
            {
                var chunks = (Array)ChunksField.GetValue(backend);
                object chunk = chunks.GetValue(chunkIndex);
                FieldInfo activeCountField = chunk.GetType().GetField(
                    "activeSubMeshCount",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                activeCountField.SetValue(chunk, activeSubMeshCount);
            }
        }
    }
}
#endif
