#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Test
{
    public sealed class BattleNativeMeshPrewarmEditorTests
    {
        private const BindingFlags InternalInstance = BindingFlags.Instance | BindingFlags.NonPublic;

        [TestCase(1)]
        [TestCase(17)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void Prepare_RestoresEveryRequiredLostNativeMeshWithoutBuilding(int commandCapacity)
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(commandCapacity);
            int requiredChunks = RequiredChunks(commandCapacity);
            var descriptorStorage = new object[requiredChunks];
            for (int index = 0; index < requiredChunks; index++)
            {
                descriptorStorage[index] = ChunkField(backend, index, "subMeshDescriptors");
                UnityEngine.Object.DestroyImmediate(StoredMesh(backend, index));
            }
            object chunkStorage = Field(backend, "chunks");
            object segmentStorage = Field(backend, "segments");
            int mutationVersion = (int)Field(backend, "mutationVersion");

            backend.PrepareCapacity(commandCapacity);

            for (int index = 0; index < requiredChunks; index++)
            {
                Mesh mesh = StoredMesh(backend, index);
                Assert.That(mesh != null, Is.True, "Inspect the stored mesh, not its lazy-creating getter.");
                Assert.That(ChunkField(backend, index, "subMeshDescriptors"), Is.SameAs(descriptorStorage[index]));
                Assert.That(mesh.vertexCount, Is.EqualTo(BattleDynamicMeshBackend.VerticesPerChunk));
                Assert.That(mesh.GetVertexBufferStride(0), Is.EqualTo(44));
                Assert.That(mesh.indexFormat, Is.EqualTo(IndexFormat.UInt16));
                Assert.That(mesh.subMeshCount, Is.EqualTo(1));
                Assert.That(mesh.GetSubMesh(0).indexCount, Is.Zero);
                Assert.That(mesh.bounds.size, Is.EqualTo(Vector3.zero));
            }
            Assert.That(Field(backend, "chunks"), Is.SameAs(chunkStorage));
            Assert.That(Field(backend, "segments"), Is.SameAs(segmentStorage));
            Assert.That(Field(backend, "mutationVersion"), Is.EqualTo(mutationVersion));
            Assert.That(backend.ActiveChunkCount, Is.Zero);
            Assert.That(backend.SegmentCount, Is.Zero);
            Assert.That(Field(backend, "builtFrame"), Is.Null);
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
        }

        [Test]
        public void Prepare_ReusesLivingMeshAndRecoversOnlyLostRequiredChunk()
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(4097);
            Mesh first = StoredMesh(backend, 0);
            Mesh second = StoredMesh(backend, 1);
            backend.PrepareCapacity(4097);
            Assert.That(StoredMesh(backend, 0), Is.SameAs(first));
            Assert.That(StoredMesh(backend, 1), Is.SameAs(second));

            UnityEngine.Object.DestroyImmediate(second);
            backend.PrepareCapacity(1);
            Assert.That(StoredMesh(backend, 0), Is.SameAs(first));
            Assert.That(StoredMesh(backend, 1) == null, Is.True,
                "A smaller reservation must not eagerly recreate an unused higher chunk.");

            backend.PrepareCapacity(4097);
            Assert.That(StoredMesh(backend, 0), Is.SameAs(first));
            Assert.That(StoredMesh(backend, 1) != null, Is.True);
            Assert.That(StoredMesh(backend, 1), Is.Not.SameAs(second));
        }

        [Test]
        public void Prepare_ZeroCapacityDoesNotAllocateOrRecoverMesh()
        {
            using var emptyBackend = new BattleDynamicMeshBackend();
            emptyBackend.PrepareCapacity(0);
            Assert.That(((Array)Field(emptyBackend, "chunks")).GetValue(0), Is.Null);

            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(1);
            UnityEngine.Object.DestroyImmediate(StoredMesh(backend, 0));
            backend.PrepareCapacity(0);
            Assert.That(StoredMesh(backend, 0) == null, Is.True);
        }

        [Test]
        public void Prepare_SealedBackendRejectsBeforeNativeRecovery()
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(1);
            Invoke(backend, "SealCapacity", 1);
            UnityEngine.Object.DestroyImmediate(StoredMesh(backend, 0));
            Assert.Throws<InvalidOperationException>(() => backend.PrepareCapacity(1));
            Assert.That(StoredMesh(backend, 0) == null, Is.True);
        }

        [Test]
        public void Prepare_UnsealedBackendCanRecoverAndSealAgain()
        {
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(17);
            Invoke(backend, "SealCapacity", 17);
            UnityEngine.Object.DestroyImmediate(StoredMesh(backend, 0));
            Invoke(backend, "UnsealCapacity");
            backend.PrepareCapacity(17);
            Assert.That(StoredMesh(backend, 0) != null, Is.True);
            Invoke(backend, "SealCapacity", 17);
            var frame = Frame(17);
            backend.Build(frame, new Resolver(), BattleCentralDrawMode.StrictOrderedDraw);
            Assert.That(backend.SegmentCount, Is.EqualTo(17));
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
        }

        [TestCase(17, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(17, BattleCentralDrawMode.StrictOrderedDraw)]
        [TestCase(4097, BattleCentralDrawMode.OrderedChunks)]
        [TestCase(4097, BattleCentralDrawMode.StrictOrderedDraw)]
        public void Build_AfterRecoveryReusesMeshAndStorageAcrossFirstAndTailUploads(
            int commandCapacity, BattleCentralDrawMode drawMode)
        {
            using var backend = new BattleDynamicMeshBackend();
            var dense = Frame(commandCapacity);
            var sparse = Frame(1);
            var empty = Frame(0);
            var resolver = new Resolver();
            backend.PrepareCapacity(commandCapacity);
            // Warm generic Mesh APIs before recreating the native mesh; no post-recovery Build is excluded.
            backend.Build(dense, resolver, drawMode);
            for (int index = 0; index < RequiredChunks(commandCapacity); index++)
                UnityEngine.Object.DestroyImmediate(StoredMesh(backend, index));

            backend.PrepareCapacity(commandCapacity);
            int requiredChunks = RequiredChunks(commandCapacity);
            var meshes = new Mesh[requiredChunks];
            var descriptors = new object[requiredChunks];
            for (int index = 0; index < requiredChunks; index++)
            {
                meshes[index] = StoredMesh(backend, index);
                Assert.That(meshes[index] != null, Is.True);
                descriptors[index] = ChunkField(backend, index, "subMeshDescriptors");
            }
            object chunks = Field(backend, "chunks");
            object segments = Field(backend, "segments");
            Invoke(backend, "SealCapacity", commandCapacity);

            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            backend.Build(dense, resolver, drawMode);
            for (int round = 0; round < 8; round++)
            {
                backend.Build(sparse, resolver, drawMode);
                backend.Build(empty, resolver, drawMode);
                backend.Build(dense, resolver, drawMode);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero, "Build/resolve/write/mesh Upload only; not full central submission.");
            Assert.That(backend.ActiveChunkCount, Is.EqualTo(requiredChunks));
            Assert.That(backend.SegmentCount, Is.EqualTo(
                drawMode == BattleCentralDrawMode.StrictOrderedDraw ? commandCapacity : requiredChunks));
            Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(commandCapacity));
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
            Assert.That(Field(backend, "chunks"), Is.SameAs(chunks));
            Assert.That(Field(backend, "segments"), Is.SameAs(segments));
            for (int index = 0; index < requiredChunks; index++)
            {
                Assert.That(StoredMesh(backend, index), Is.SameAs(meshes[index]));
                Assert.That(ChunkField(backend, index, "subMeshDescriptors"), Is.SameAs(descriptors[index]));
                int quadCount = Math.Min(BattleDynamicMeshBackend.QuadsPerChunk,
                    commandCapacity - index * BattleDynamicMeshBackend.QuadsPerChunk);
                Assert.That(backend.GetChunkActiveQuadCount(index), Is.EqualTo(quadCount));
                Mesh mesh = meshes[index];
                int activeSegments = drawMode == BattleCentralDrawMode.StrictOrderedDraw ? quadCount : 1;
                for (int subMesh = 0; subMesh < activeSegments; subMesh++)
                {
                    SubMeshDescriptor descriptor = mesh.GetSubMesh(subMesh);
                    int segmentQuads = drawMode == BattleCentralDrawMode.StrictOrderedDraw ? 1 : quadCount;
                    Assert.That(descriptor.indexCount, Is.EqualTo(segmentQuads * BattleDynamicMeshBackend.IndicesPerQuad));
                    Assert.That(descriptor.indexStart, Is.EqualTo(
                        drawMode == BattleCentralDrawMode.StrictOrderedDraw
                            ? subMesh * BattleDynamicMeshBackend.IndicesPerQuad : 0));
                }
            }
        }

        private static int RequiredChunks(int commandCapacity)
        {
            return (commandCapacity + BattleDynamicMeshBackend.QuadsPerChunk - 1) /
                   BattleDynamicMeshBackend.QuadsPerChunk;
        }

        private static BattlePresentationFrame Frame(int count)
        {
            var frame = new BattlePresentationFrame();
            for (int index = 0; index < count; index++)
            {
                frame.AddCommand(new BattleRenderCommand(
                    BattleRenderCommandType.Entity, RuntimeEntityHandle.Invalid,
                    index, 0, index, 0, index, index, 0, index,
                    new Vector3(index, 0f, 0f), Vector2.one, new Vector2(0.5f, 0.5f),
                    new Rect(0f, 0f, 1f, 1f), false, default));
            }
            return frame;
        }

        private static Mesh StoredMesh(BattleDynamicMeshBackend backend, int index)
        {
            return (Mesh)ChunkField(backend, index, "mesh");
        }

        private static object ChunkField(BattleDynamicMeshBackend backend, int index, string name)
        {
            object chunk = ((Array)Field(backend, "chunks")).GetValue(index);
            return Field(chunk, name);
        }

        private static object Field(object target, string name)
        {
            return target.GetType().GetField(name, InternalInstance).GetValue(target);
        }

        private static void Invoke(object target, string name, params object[] arguments)
        {
            target.GetType().GetMethod(name, InternalInstance).Invoke(target, arguments);
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(null, null, new Rect(0f, 0f, 1f, 1f),
                    Vector2.one, new Vector2(0.5f, 0.5f), Color.white, 0);
                return BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif
