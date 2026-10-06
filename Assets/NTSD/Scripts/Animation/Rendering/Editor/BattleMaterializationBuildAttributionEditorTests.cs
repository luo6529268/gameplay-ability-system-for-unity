#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleMaterializationBuildAttributionEditorTests
    {
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic;
        private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly string[] GroupNames =
        {
            "MaterializationBuilds", "PublicationChangedBuilds", "AlphaChangedBuilds", "RepeatedSampleBuilds",
        };
        private static readonly string[] CounterNames =
        {
            "BuildInvocationCount", "AdmittedBuildCount", "CompletedBuildCount", "RejectedBeforeBuildCount",
            "FailedBuildCount", "VertexUploadCallCount", "UploadedVertexCount", "UploadedVertexBytes",
        };

        private delegate void BuildAction(BattleDynamicMeshBackend backend, BattlePresentationFrame frame,
            IBattleCentralResourceResolver resolver, BattleCentralDrawMode mode, BattleCentralDisplaySampleKind kind,
            BattleTickDetailPhaseDiagnostics detail, BattlePresentationPhaseDiagnostics presentation);

        [SetUp]
        public void SetUp()
        {
            BattleCentralRenderSystem.ResetRuntime();
        }

        [TearDown]
        public void TearDown()
        {
            BattleCentralRenderSystem.ResetRuntime();
        }

        [TestCase(BattleCentralDisplaySampleKind.PublicationChanged)]
        [TestCase(BattleCentralDisplaySampleKind.AlphaChanged)]
        [TestCase(BattleCentralDisplaySampleKind.Repeated)]
        public void RealBuild_AssignsOnlyItsCategory_UsingActualMeshStride(BattleCentralDisplaySampleKind kind)
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(17);
            build(backend, Frame(17), new Resolver(), BattleCentralDrawMode.OrderedChunks, kind, null, null);
            long bytes = 68L * backend.GetChunkMesh(0).GetVertexBufferStride(0);
            AssertCounts(Group("MaterializationBuilds"), 1, 1, 1, 0, 0, 1, 68, bytes);
            AssertCounts(Group(GroupName(kind)), 1, 1, 1, 0, 0, 1, 68, bytes);
            foreach (string name in GroupNames)
            {
                if (name != "MaterializationBuilds" && name != GroupName(kind))
                    AssertCounts(Group(name), 0, 0, 0, 0, 0, 0, 0, 0);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void EmptyOrNullBuild_IsCompletedWithoutVertexPayload(bool useNull)
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(1);
            build(backend, useNull ? null : Frame(0), new Resolver(), BattleCentralDrawMode.OrderedChunks,
                BattleCentralDisplaySampleKind.AlphaChanged, null, null);
            AssertCounts(Group("MaterializationBuilds"), 1, 1, 1, 0, 0, 0, 0, 0);
        }

        [Test]
        public void MixedCategories_TotalIsTheirSum_AndRepeatedBuildStillUploads()
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(17);
            var frame = Frame(17);
            var resolver = new Resolver();
            foreach (BattleCentralDisplaySampleKind kind in new[]
            {
                BattleCentralDisplaySampleKind.PublicationChanged,
                BattleCentralDisplaySampleKind.AlphaChanged, BattleCentralDisplaySampleKind.Repeated,
            })
                build(backend, frame, resolver, BattleCentralDrawMode.OrderedChunks, kind, null, null);
            long bytes = 204L * backend.GetChunkMesh(0).GetVertexBufferStride(0);
            AssertCounts(Group("MaterializationBuilds"), 3, 3, 3, 0, 0, 3, 204, bytes);
            foreach (string counter in CounterNames)
            {
                long sum = Count(Group("PublicationChangedBuilds"), counter) +
                    Count(Group("AlphaChangedBuilds"), counter) + Count(Group("RepeatedSampleBuilds"), counter);
                Assert.That(Count(Group("MaterializationBuilds"), counter), Is.EqualTo(sum), counter);
            }
            Assert.That(backend.Diagnostics.VertexUploadCallCount, Is.EqualTo(1),
                "Backend is per-Build; these groups are cumulative. No dirty-chunk optimization is claimed.");
        }

        [TestCase(BattleCentralDrawMode.OrderedChunks)]
        [TestCase(BattleCentralDrawMode.StrictOrderedDraw)]
        public void DrawMode_ChangesSegments_NotTheVertexPayloadAttribution(BattleCentralDrawMode mode)
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(17);
            build(backend, Frame(17), new Resolver(), mode, BattleCentralDisplaySampleKind.PublicationChanged, null, null);
            Assert.That(backend.SegmentCount, Is.EqualTo(mode == BattleCentralDrawMode.StrictOrderedDraw ? 17 : 1));
            Assert.That(Count(Group("MaterializationBuilds"), "VertexUploadCallCount"), Is.EqualTo(1));
            Assert.That(Count(Group("MaterializationBuilds"), "UploadedVertexCount"), Is.EqualTo(68));
        }

        [Test]
        public void SealedCapacityReject_DoesNotCountThePriorBuildPayloadAgain()
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(17);
            typeof(BattleDynamicMeshBackend).GetMethod("SealCapacity", InstanceFlags)
                .Invoke(backend, new object[] { 17 });
            build(backend, Frame(17), new Resolver(), BattleCentralDrawMode.OrderedChunks,
                BattleCentralDisplaySampleKind.PublicationChanged, null, null);
            Assert.Throws<InvalidOperationException>(() => build(backend, Frame(18), new Resolver(),
                BattleCentralDrawMode.OrderedChunks, BattleCentralDisplaySampleKind.AlphaChanged, null, null));
            AssertCounts(Group("AlphaChangedBuilds"), 1, 0, 0, 1, 0, 0, 0, 0);
            Assert.That(backend.Diagnostics.VertexUploadCallCount, Is.EqualTo(1), "Old diagnostics are intentionally retained.");
            Assert.That(Count(Group("MaterializationBuilds"), "VertexUploadCallCount"), Is.EqualTo(1));
        }

        [Test]
        public void NullResolverReject_DoesNotCountStaleDiagnostics()
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(1);
            build(backend, Frame(1), new Resolver(), BattleCentralDrawMode.OrderedChunks,
                BattleCentralDisplaySampleKind.PublicationChanged, null, null);
            Assert.Throws<ArgumentNullException>(() => build(backend, Frame(1), null,
                BattleCentralDrawMode.OrderedChunks, BattleCentralDisplaySampleKind.AlphaChanged, null, null));
            AssertCounts(Group("AlphaChangedBuilds"), 1, 0, 0, 1, 0, 0, 0, 0);
            Assert.That(Count(Group("MaterializationBuilds"), "UploadedVertexCount"), Is.EqualTo(4));
        }

        [Test]
        public void DisposedBackend_IsRejectedBeforeBuild()
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.Dispose();
            Assert.Throws<ObjectDisposedException>(() => build(backend, Frame(1), new Resolver(),
                BattleCentralDrawMode.OrderedChunks, BattleCentralDisplaySampleKind.Repeated, null, null));
            AssertCounts(Group("MaterializationBuilds"), 1, 0, 0, 1, 0, 0, 0, 0);
        }

        [TestCase(0)]
        [TestCase(4096)]
        public void UploadFailure_FreezesCompletedApisBeforeOuterClear(int invalidIndex)
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(4097);
            build(backend, Frame(1), new Resolver(), BattleCentralDrawMode.OrderedChunks,
                BattleCentralDisplaySampleKind.PublicationChanged, null, null);
            Assert.Throws<ArgumentException>(() => build(backend, Frame(4097, invalidIndex), new Resolver(),
                BattleCentralDrawMode.OrderedChunks, BattleCentralDisplaySampleKind.AlphaChanged, null, null));
            long calls = invalidIndex == 0 ? 0 : 1;
            long vertices = invalidIndex == 0 ? 0 : 16384;
            long bytes = vertices == 0 ? 0 : vertices * backend.GetChunkMesh(0).GetVertexBufferStride(0);
            backend.Clear();
            Assert.That(backend.Diagnostics.VertexUploadCallCount, Is.Zero);
            AssertCounts(Group("AlphaChangedBuilds"), 1, 1, 0, 0, 1, calls, vertices, bytes);
            Assert.That(Count(Group("MaterializationBuilds"), "VertexUploadCallCount"), Is.EqualTo(1 + calls),
                "Completed API payload survives cleanup, not a license to publish a partial submission.");
        }

        [Test]
        public void ResolverException_IsAnAdmittedFailedBuildWithNoUpload()
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(1);
            Assert.Throws<InvalidOperationException>(() => build(backend, Frame(1), new Resolver(throws: true),
                BattleCentralDrawMode.OrderedChunks, BattleCentralDisplaySampleKind.Repeated, null, null));
            AssertCounts(Group("MaterializationBuilds"), 1, 1, 0, 0, 1, 0, 0, 0);
        }

        [Test]
        public void UnresolvedCommands_CanCompleteBuildWithoutPixels()
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(17);
            build(backend, Frame(17), new Resolver(reject: true), BattleCentralDrawMode.OrderedChunks,
                BattleCentralDisplaySampleKind.Repeated, null, null);
            Assert.That(backend.ActiveChunkCount, Is.Zero);
            AssertCounts(Group("MaterializationBuilds"), 1, 1, 1, 0, 0, 0, 0, 0);
        }

        [Test]
        public void NoneContext_StillBuildsButDoesNotEnterProductionAttribution()
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(1);
            build(backend, Frame(1), new Resolver(), BattleCentralDrawMode.OrderedChunks,
                BattleCentralDisplaySampleKind.None, null, null);
            Assert.That(backend.Diagnostics.VertexUploadCallCount, Is.EqualTo(1));
            AssertCounts(Group("MaterializationBuilds"), 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void ScalarAggregation_PreservesLongPayload_AndIgnoresRejectedStalePayload()
        {
            var record = RecordMethod();
            var payload = new BattleCentralBuildDiagnostics
            {
                VertexUploadCallCount = 2,
                UploadedVertexCount = (long)int.MaxValue + 64,
                UploadedVertexBytes = (long)int.MaxValue + 128,
            };
            record(BattleCentralDisplaySampleKind.PublicationChanged, true, true, payload);
            record(BattleCentralDisplaySampleKind.AlphaChanged, false, false, payload);
            AssertCounts(Group("PublicationChangedBuilds"), 1, 1, 1, 0, 0, 2,
                (long)int.MaxValue + 64, (long)int.MaxValue + 128);
            AssertCounts(Group("AlphaChangedBuilds"), 1, 0, 0, 1, 0, 0, 0, 0);
            Assert.That(Count(Group("MaterializationBuilds"), "UploadedVertexBytes"), Is.EqualTo((long)int.MaxValue + 128));
        }

        [Test]
        public void PerFrameReset_PreservesGroups_RuntimeResetClearsAndReusesThem()
        {
            var record = RecordMethod();
            object group = Group("MaterializationBuilds");
            record(BattleCentralDisplaySampleKind.PublicationChanged, true, true, new BattleCentralBuildDiagnostics());
            typeof(BattleCentralRenderSystem).GetMethod("ResetPerFrameDiagnostics", StaticFlags)
                .Invoke(null, new object[] { BattlePresentationBackendMode.CentralOnly, false });
            Assert.That(Count(group, "CompletedBuildCount"), Is.EqualTo(1));
            BattleCentralRenderSystem.ResetRuntime();
            Assert.That(Group("MaterializationBuilds"), Is.SameAs(group));
            foreach (string name in GroupNames)
                foreach (string counter in CounterNames)
                    Assert.That(Count(Group(name), counter), Is.Zero, name + "." + counter);
        }

        [Test]
        public void ForcedCachedPlan_HasPrepareAttemptsButNoBackendBuild()
        {
            Group("MaterializationBuilds");
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.CentralOnly);
            world.BattlePresentation.BeginFrame(world, 10);
            BattleCentralRenderSystem.QueueLatestPublishedFrameForSelfCheck(world);
            var plan = new BattlePixelFramePlan(world, world.BattlePresentation.PublishedFrame,
                BattlePresentationBackendMode.CentralOnly, BattlePixelFrameOwner.Central,
                10, 10, 100, false, "synthetic cached-plan gate only", null);
            world.PublishPixelFramePlan(plan);
            SetStatic("publishedPlanWorld", world);
            SetStatic("publishedPlanGeneration", 100);
            SetStatic("lastBuiltDisplayAlpha", 1.0);
            Assert.That(BattleCentralRenderSystem.FlushLatestPublishedFrame(world).Generation, Is.EqualTo(100));
            Assert.That(BattleCentralRenderSystem.FlushLatestPublishedFrame(world).Generation, Is.EqualTo(100));
            Assert.That(BattleCentralRenderSystem.Diagnostics.MaterializationAttemptCount, Is.EqualTo(2));
            AssertCounts(Group("MaterializationBuilds"), 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void LegacyPrepare_DoesNotEnterProductionBuildAttribution()
        {
            Group("MaterializationBuilds");
            var world = new SimulationWorld();
            world.SetBattlePresentationBackend(BattlePresentationBackendMode.LegacyOnly);
            world.BattlePresentation.BeginFrame(world, 10);
            Assert.That(BattleCentralRenderSystem.FlushLatestPublishedFrame(world).IsValid, Is.True);
            AssertCounts(Group("MaterializationBuilds"), 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void PreparedScalarAggregation_AllocatesNoManagedMemory()
        {
            var record = RecordMethod();
            var payload = new BattleCentralBuildDiagnostics { VertexUploadCallCount = 1, UploadedVertexCount = 4, UploadedVertexBytes = 176 };
            for (int index = 0; index < 64; index++)
                record(BattleCentralDisplaySampleKind.PublicationChanged, true, true, payload);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 512; index++)
                record(BattleCentralDisplaySampleKind.AlphaChanged, true, true, payload);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero, "Only the new cached scalar helper is measured.");
        }

        [TestCase(BattleCentralDrawMode.OrderedChunks)]
        [TestCase(BattleCentralDrawMode.StrictOrderedDraw)]
        public void WarmedAttributionWrapperAndBuild_AllocateNoManagedMemory(BattleCentralDrawMode mode)
        {
            BuildAction build = BuildMethod();
            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(17);
            var dense = Frame(17);
            var sparse = Frame(1);
            var empty = Frame(0);
            var resolver = new Resolver();
            for (int index = 0; index < 8; index++)
            {
                build(backend, dense, resolver, mode, BattleCentralDisplaySampleKind.PublicationChanged, null, null);
                build(backend, sparse, resolver, mode, BattleCentralDisplaySampleKind.AlphaChanged, null, null);
                build(backend, empty, resolver, mode, BattleCentralDisplaySampleKind.Repeated, null, null);
            }
            BattleCentralRenderSystem.ResetRuntime();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < 48; index++)
            {
                build(backend, dense, resolver, mode, BattleCentralDisplaySampleKind.PublicationChanged, null, null);
                build(backend, sparse, resolver, mode, BattleCentralDisplaySampleKind.AlphaChanged, null, null);
                build(backend, empty, resolver, mode, BattleCentralDisplaySampleKind.Repeated, null, null);
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Assert.That(allocated, Is.Zero, "Warmed fixture wrapper + entity Mesh Build/Upload only, not the full render path.");
            Assert.That(Count(Group("MaterializationBuilds"), "BuildInvocationCount"), Is.EqualTo(144));
            Assert.That(Count(Group("MaterializationBuilds"), "VertexUploadCallCount"), Is.EqualTo(96));
            Assert.That(Count(Group("MaterializationBuilds"), "UploadedVertexCount"), Is.EqualTo(3456));
        }

        private static BuildAction BuildMethod()
        {
            MethodInfo method = typeof(BattleCentralRenderSystem).GetMethod("BuildMeshForMaterialization", StaticFlags);
            Assert.That(method, Is.Not.Null, "Attributed Build wrapper is missing.");
            return (BuildAction)Delegate.CreateDelegate(typeof(BuildAction), method);
        }

        private static Action<BattleCentralDisplaySampleKind, bool, bool, BattleCentralBuildDiagnostics> RecordMethod()
        {
            MethodInfo method = typeof(BattleCentralRuntimeDiagnostics).GetMethod("RecordBackendBuild", InstanceFlags);
            Assert.That(method, Is.Not.Null, "Build attribution aggregation is missing.");
            return (Action<BattleCentralDisplaySampleKind, bool, bool, BattleCentralBuildDiagnostics>)Delegate.CreateDelegate(
                typeof(Action<BattleCentralDisplaySampleKind, bool, bool, BattleCentralBuildDiagnostics>),
                BattleCentralRenderSystem.Diagnostics, method);
        }

        private static object Group(string name)
        {
            PropertyInfo property = typeof(BattleCentralRuntimeDiagnostics).GetProperty(name, InstanceFlags);
            Assert.That(property, Is.Not.Null, "Missing attribution group: " + name);
            Assert.That(property.GetSetMethod(), Is.Null);
            object group = property.GetValue(BattleCentralRenderSystem.Diagnostics);
            Assert.That(group, Is.Not.Null);
            return group;
        }

        private static long Count(object group, string name)
        {
            PropertyInfo property = group.GetType().GetProperty(name, InstanceFlags);
            Assert.That(property, Is.Not.Null, name);
            Assert.That(property.PropertyType, Is.EqualTo(typeof(long)));
            Assert.That(property.GetSetMethod(), Is.Null);
            return (long)property.GetValue(group);
        }

        private static void AssertCounts(object group, params long[] expected)
        {
            for (int index = 0; index < CounterNames.Length; index++)
                Assert.That(Count(group, CounterNames[index]), Is.EqualTo(expected[index]), CounterNames[index]);
        }

        private static string GroupName(BattleCentralDisplaySampleKind kind)
        {
            return kind == BattleCentralDisplaySampleKind.PublicationChanged ? "PublicationChangedBuilds"
                : kind == BattleCentralDisplaySampleKind.AlphaChanged ? "AlphaChangedBuilds" : "RepeatedSampleBuilds";
        }

        private static void SetStatic(string name, object value)
        {
            typeof(BattleCentralRenderSystem).GetField(name, StaticFlags).SetValue(null, value);
        }

        private static BattlePresentationFrame Frame(int count, int invalidIndex = -1)
        {
            var frame = new BattlePresentationFrame();
            for (int index = 0; index < count; index++)
            {
                frame.AddCommand(new BattleRenderCommand(BattleRenderCommandType.Entity, RuntimeEntityHandle.Invalid,
                    index, 0, index, 0, index, index, 0, index,
                    new Vector3(index == invalidIndex ? float.NaN : index, 0f, 0f),
                    Vector2.one, new Vector2(0.5f, 0.5f), new Rect(0f, 0f, 1f, 1f), false, default));
            }
            return frame;
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            private readonly bool reject;
            private readonly bool throws;

            public Resolver(bool reject = false, bool throws = false)
            {
                this.reject = reject;
                this.throws = throws;
            }

            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                if (throws)
                    throw new InvalidOperationException("fixture resolve failure");
                resource = new BattleCentralResolvedResource(null, null, new Rect(0f, 0f, 1f, 1f),
                    Vector2.one, new Vector2(0.5f, 0.5f), Color.white, 0);
                return reject ? BattleCentralResourceStatus.UnresolvedVisual : BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif
