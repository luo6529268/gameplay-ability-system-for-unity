#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test
{
    public sealed class BattleVertexUploadDiagnosticsEditorTests
    {
        [TestCase(1)]
        [TestCase(17)]
        [TestCase(4096)]
        [TestCase(4097)]
        public void Build_CountsOnlyActiveVertexPrefixUsingActualMeshStride(int count)
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(count);
            backend.Build(Frame(count), new Resolver());

            AssertStatistics(backend, RequiredChunks(count), (long)count * 4);
            Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(count));
            Assert.That(backend.Diagnostics.SegmentCount, Is.EqualTo(RequiredChunks(count)));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EmptyOrNullFrame_HasNoVertexUpload(bool useNull)
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(17);
            backend.Build(Frame(17), new Resolver());
            backend.Build(useNull ? null : Frame(0), new Resolver());

            AssertStatistics(backend, 0, 0);
            Assert.That(backend.ActiveChunkCount, Is.Zero);
        }

        [TestCase(BattleCentralDrawMode.OrderedChunks)]
        [TestCase(BattleCentralDrawMode.StrictOrderedDraw)]
        public void DrawMode_ChangesSegmentsNotVertexUploadCalls(BattleCentralDrawMode mode)
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(17);
            backend.Build(Frame(17), new Resolver(), mode);

            AssertStatistics(backend, 1, 68);
            Assert.That(backend.SegmentCount,
                Is.EqualTo(mode == BattleCentralDrawMode.StrictOrderedDraw ? 17 : 1));
        }

        [Test]
        public void SameFrameRepeatedBuild_IsCountedAgainWithoutAccumulating()
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(17);
            var frame = Frame(17);
            var resolver = new Resolver();
            backend.Build(frame, resolver);
            Statistics first = Read(backend);

            for (int iteration = 0; iteration < 4; iteration++)
            {
                backend.Build(frame, resolver);
                Assert.That(Read(backend), Is.EqualTo(first),
                    "A fresh per-Build count is not proof of dirty-chunk skipping.");
            }
        }

        [Test]
        public void ShrinkAndClear_ResetWithoutCountingInertTailMetadata()
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(4097);
            backend.Build(Frame(4097), new Resolver(), BattleCentralDrawMode.StrictOrderedDraw);
            backend.Build(Frame(1), new Resolver());
            AssertStatistics(backend, 1, 4);

            backend.Clear();
            AssertStatistics(backend, 0, 0);
            backend.Build(Frame(17), new Resolver());
            AssertStatistics(backend, 1, 68);
        }

        [Test]
        public void PrepareAndNativeMeshRecovery_DoNotCountIndexBufferOrReservation()
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(4097);
            AssertStatistics(backend, 0, 0);
            backend.Build(Frame(17), new Resolver());
            Statistics before = Read(backend);
            UnityEngine.Object.DestroyImmediate(backend.GetChunkMesh(0));

            backend.PrepareCapacity(4097);

            Assert.That(Read(backend), Is.EqualTo(before),
                "Preparation does not start a new Build diagnostics scope.");
            backend.Build(Frame(17), new Resolver());
            AssertStatistics(backend, 1, 68);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UnresolvedCommands_CountOnlyResolvedPayload(bool rejectAll)
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(17);
            backend.Build(Frame(17), new Resolver(rejectAll, true));
            int resolved = rejectAll ? 0 : 9;

            AssertStatistics(backend, resolved == 0 ? 0 : 1, (long)resolved * 4);
            Assert.That(backend.Diagnostics.ResolvedCommandCount, Is.EqualTo(resolved));
            Assert.That(backend.Diagnostics.UnresolvedCommandCount, Is.EqualTo(17 - resolved));
        }

        [Test]
        public void SealedCapacityReject_BeforeBuildKeepsPriorDiagnostics()
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(17);
            typeof(BattleDynamicMeshBackend).GetMethod("SealCapacity",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(backend, new object[] { 17 });
            backend.Build(Frame(17), new Resolver());
            Statistics before = Read(backend);
            Mesh mesh = backend.GetChunkMesh(0);
            Bounds bounds = mesh.bounds;

            Assert.Throws<InvalidOperationException>(() => backend.Build(Frame(18), new Resolver()));

            Assert.That(Read(backend), Is.EqualTo(before));
            Assert.That(backend.GetChunkMesh(0), Is.SameAs(mesh));
            Assert.That(mesh.bounds, Is.EqualTo(bounds));
            Assert.That(backend.Diagnostics.SourceCommandCount, Is.EqualTo(17));
        }

        [TestCase(0)]
        [TestCase(4096)]
        public void UploadFailure_ReportsOnlyAlreadyCompletedApiCalls(int invalidCommandIndex)
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(4097);
            backend.Build(Frame(1), new Resolver());

            Assert.Throws<ArgumentException>(() =>
                backend.Build(Frame(4097, invalidCommandIndex), new Resolver()));

            int completedQuads = invalidCommandIndex == 0 ? 0 : 4096;
            long expectedVertices = (long)completedQuads * 4;
            Statistics stats = Read(backend);
            Assert.That(stats.Calls, Is.EqualTo(completedQuads == 0 ? 0 : 1));
            Assert.That(stats.Vertices, Is.EqualTo(expectedVertices));
            long stride = completedQuads == 0 ? 0 : backend.GetChunkMesh(0).GetVertexBufferStride(0);
            Assert.That(stats.Bytes, Is.EqualTo(expectedVertices * stride));
            Assert.That(backend.Diagnostics.SourceCommandCount, Is.EqualTo(4097),
                "An entered, failed Build scope is not the previous successful scope.");
        }

        [TestCase(BattleCentralDrawMode.OrderedChunks)]
        [TestCase(BattleCentralDrawMode.StrictOrderedDraw)]
        public void WarmedBuildWithCounters_AllocatesNoManagedMemory(BattleCentralDrawMode mode)
        {
            using var backend = new BattleDynamicMeshBackend();
            Read(backend);
            backend.PrepareCapacity(4097);
            var dense = Frame(4097);
            var sparse = Frame(17);
            var empty = Frame(0);
            var resolver = new Resolver();
            for (int iteration = 0; iteration < 4; iteration++)
            {
                backend.Build(dense, resolver, mode);
                backend.Build(sparse, resolver, mode);
                backend.Build(empty, resolver, mode);
            }
            GC.GetAllocatedBytesForCurrentThread();

            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < 16; iteration++)
            {
                backend.Build(sparse, resolver, mode);
                backend.Build(empty, resolver, mode);
                backend.Build(dense, resolver, mode);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(allocated, Is.Zero, "Only warmed fixture Build/Upload is in this scope.");
            AssertStatistics(backend, 2, 4097L * 4);
            Assert.That(backend.Diagnostics.CapacityGrowthCount, Is.Zero);
        }

        private static void AssertStatistics(BattleDynamicMeshBackend backend, int calls, long vertices)
        {
            Statistics stats = Read(backend);
            long bytes = 0;
            for (int chunkIndex = 0; chunkIndex < backend.ActiveChunkCount; chunkIndex++)
            {
                bytes += (long)backend.GetChunkActiveQuadCount(chunkIndex) * 4 *
                         backend.GetChunkMesh(chunkIndex).GetVertexBufferStride(0);
            }
            Assert.That(stats.Calls, Is.EqualTo(calls));
            Assert.That(stats.Vertices, Is.EqualTo(vertices));
            Assert.That(stats.Bytes, Is.EqualTo(bytes));
        }

        private static Statistics Read(BattleDynamicMeshBackend backend)
        {
            return new Statistics(
                ReadProperty<int>(backend.Diagnostics, "VertexUploadCallCount"),
                ReadProperty<long>(backend.Diagnostics, "UploadedVertexCount"),
                ReadProperty<long>(backend.Diagnostics, "UploadedVertexBytes"));
        }

        private static T ReadProperty<T>(BattleCentralBuildDiagnostics diagnostics, string name)
        {
            PropertyInfo property = typeof(BattleCentralBuildDiagnostics).GetProperty(name);
            Assert.That(property, Is.Not.Null, name + " must directly describe completed Mesh API payload.");
            Assert.That(property.PropertyType, Is.EqualTo(typeof(T)));
            Assert.That(property.GetSetMethod(), Is.Null, "Consumers must not rewrite build counters.");
            return (T)property.GetValue(diagnostics);
        }

        private static int RequiredChunks(int count)
        {
            return (count + BattleDynamicMeshBackend.QuadsPerChunk - 1) /
                   BattleDynamicMeshBackend.QuadsPerChunk;
        }

        private static BattlePresentationFrame Frame(int count, int invalidCommandIndex = -1)
        {
            var frame = new BattlePresentationFrame();
            for (int index = 0; index < count; index++)
            {
                frame.AddCommand(new BattleRenderCommand(
                    BattleRenderCommandType.Entity, RuntimeEntityHandle.Invalid,
                    index, 0, index, 0, index, index, 0, index,
                    new Vector3(index == invalidCommandIndex ? float.NaN : index, 0f, 0f),
                    Vector2.one, new Vector2(0.5f, 0.5f),
                    new Rect(0f, 0f, 1f, 1f), false, default));
            }
            return frame;
        }

        private readonly struct Statistics
        {
            public Statistics(int calls, long vertices, long bytes)
            {
                Calls = calls;
                Vertices = vertices;
                Bytes = bytes;
            }

            public int Calls { get; }
            public long Vertices { get; }
            public long Bytes { get; }
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            private readonly bool rejectAll;
            private readonly bool rejectOdd;

            public Resolver(bool rejectAll = false, bool rejectOdd = false)
            {
                this.rejectAll = rejectAll;
                this.rejectOdd = rejectOdd;
            }

            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(null, null, new Rect(0f, 0f, 1f, 1f),
                    Vector2.one, new Vector2(0.5f, 0.5f), Color.white, 0);
                return rejectAll || (rejectOdd && (int)command.Position.x % 2 != 0)
                    ? BattleCentralResourceStatus.UnresolvedVisual
                    : BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif
