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
using UnityEngine.TestTools;

namespace NTSD.Test.Editor
{
    [Category("Goal19_M1")]
    public sealed class BattleDynamicMeshSubmeshGrowthEditorTests
    {
        private Evidence evidence;
        private string phase;

        [SetUp]
        public void SetUp()
        {
            phase = File.Exists("Temp/Goal19_M1_Phase.txt") ? File.ReadAllText("Temp/Goal19_M1_Phase.txt").Trim() : "RED";
            evidence = new Evidence { test = TestContext.CurrentContext.Test.Name, device = SystemInfo.graphicsDeviceType.ToString() };
            Application.logMessageReceived += CaptureLog;
        }

        [TearDown]
        public void TearDown()
        {
            Application.logMessageReceived -= CaptureLog;
            string name = System.Text.RegularExpressions.Regex.Replace(evidence.test, "[^a-zA-Z0-9_-]", "_");
            File.WriteAllText("Temp/Goal19_M1_" + phase + "_" + name + ".json", JsonUtility.ToJson(evidence, true));
        }

        private void CaptureLog(string message, string stack, LogType type)
        {
            evidence.logs.Add(type + ": " + message + "\n" + stack);
        }

        [TestCase(0)]
        [TestCase(1)]
        public void FirstGrowth_FromEmptyOrSingleToMany(int warmCount)
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver();
            if (warmCount != 0) Build(backend, frame, resolver, warmCount);
            Build(backend, frame, resolver, 32);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void WarmGrowth_ShrinkActivePrefixAndRegrow()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver();
            foreach (int count in new[] {1,32,48,3,65,2,65}) Build(backend, frame, resolver, count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void CrossChunk_EmptyFrameThenRestoreAndGrow()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver();
            foreach (int count in new[] {4095,4097,0,4099}) Build(backend, frame, resolver, count);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void DegenerateFiniteQuad_HasFiniteDescriptors()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver { zeroExtent = true };
            Build(backend, frame, resolver, 8, zeroSize: true);
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void NonFiniteInput_IsRejectedBeforeNativeUpload(float value)
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver();
            FrameAccess.Reset(frame, 1);
            FrameAccess.AddCommand(frame, Command(0, new Vector3(value, 0, 0), Vector2.one));
            Assert.Throws<ArgumentException>(() => backend.Build(frame, resolver), "nonfinite input must not reach native mesh mutation");
            LogAssert.NoUnexpectedReceived();
        }

        [TestCase("CheckHeldPresentationGeometryContracts")]
        [TestCase("CheckBattlePresentationShadowBuildContracts")]
        [TestCase("CheckHitRecordPresentationLifecycleContracts")]
        public void OriginalSelfCheckGrowthSites_NoEngineAssert(string method)
        {
            MethodInfo target = typeof(BattleRuntimeSelfCheck).GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(target, Is.Not.Null);
            Type singletonScope = typeof(BattleRuntimeSelfCheck).GetNestedType("TemporarySingletonSceneObjectScope", BindingFlags.NonPublic);
            Type shadowScope = typeof(BattleRuntimeSelfCheck).GetNestedType("TemporaryCommonShadowVisualConfig", BindingFlags.NonPublic);
            using var singletons = (IDisposable)Activator.CreateInstance(singletonScope, true);
            using var shadows = (IDisposable)Activator.CreateInstance(shadowScope,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                new object[] {CharacterAnimtorManager.Instance}, null);
            target.Invoke(null, null);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void WarmStableBuild_AllocatesZeroManagedBytes()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver();
            Build(backend, frame, resolver, 32);
            backend.Build(frame, resolver);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 128; i++) backend.Build(frame, resolver);
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            evidence.allocatedBytes = bytes;
            Assert.That(bytes, Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void EmptyRecovery_RangeChangeDoesNotRepublishCachedActiveTail()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver();
            Build(backend, frame, resolver, 32);
            Build(backend, frame, resolver, 0);
            Build(backend, frame, resolver, 1);
            FrameAccess.Reset(frame, 4);
            for (int i = 0; i < 8; i++)
                FrameAccess.AddCommand(frame, new BattleRenderCommand(BattleRenderCommandType.Entity,
                    RuntimeEntityHandle.Invalid, i, i < 4 ? 0 : 1, i, 0, i, i, 0, i,
                    new Vector3(i,0,0), Vector2.one, new Vector2(0.5f,0.5f), new Rect(0,0,1,1), false, default));
            backend.Build(frame, resolver);
            Mesh mesh = backend.GetChunkMesh(0);
            Assert.That(mesh.subMeshCount, Is.EqualTo(32));
            Assert.That(backend.SegmentCount, Is.EqualTo(2));
            Assert.That(mesh.GetSubMesh(0).indexCount, Is.EqualTo(24));
            Assert.That(mesh.GetSubMesh(1).indexStart, Is.EqualTo(24));
            Assert.That(mesh.GetSubMesh(1).indexCount, Is.EqualTo(24));
            for (int i = 2; i < 32; i++)
            {
                Assert.That(mesh.GetSubMesh(i).indexCount, Is.Zero);
                Assert.That(mesh.GetSubMesh(i).vertexCount, Is.Zero);
            }
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void StableSmallPrefixAfterLargeHighWater_ReusesStorageWithoutAllocation()
        {
            using var backend = new BattleDynamicMeshBackend();
            var frame = new BattlePresentationFrame(); var resolver = new Resolver();
            Build(backend, frame, resolver, 4096);
            Build(backend, frame, resolver, 1);
            backend.Build(frame, resolver);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            long started = System.Diagnostics.Stopwatch.GetTimestamp();
            for (int i = 0; i < 256; i++) backend.Build(frame, resolver);
            long elapsed = System.Diagnostics.Stopwatch.GetTimestamp() - started;
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            evidence.allocatedBytes = bytes;
            evidence.averageMicroseconds = elapsed * 1000000.0 / System.Diagnostics.Stopwatch.Frequency / 256;
            Assert.That(bytes, Is.Zero);
            Assert.That(backend.GetChunkMesh(0).subMeshCount, Is.EqualTo(4096));
            Assert.That(backend.SegmentCount, Is.EqualTo(1));
            LogAssert.NoUnexpectedReceived();
        }

        private void Build(BattleDynamicMeshBackend backend, BattlePresentationFrame frame, Resolver resolver,
            int count, bool zeroSize = false)
        {
            int previousChunks = backend.ActiveChunkCount;
            int oldPhysical = previousChunks > 0 ? backend.GetChunkMesh(0).subMeshCount : -1;
            FrameAccess.Reset(frame, evidence.frames.Count + 1);
            for (int i = 0; i < count; i++)
                FrameAccess.AddCommand(frame, Command(i, new Vector3(i % 97, i % 11, 0), zeroSize ? Vector2.zero : Vector2.one));
            backend.Build(frame, resolver);
            var report = new FrameEvidence { commandCount = count, previousPhysicalFirstChunk = oldPhysical,
                activeChunks = backend.ActiveChunkCount, segments = backend.SegmentCount };
            evidence.frames.Add(report);
            for (int chunk = 0; chunk < backend.ActiveChunkCount; chunk++)
            {
                Mesh mesh = backend.GetChunkMesh(chunk);
                int active = backend.GetChunkActiveQuadCount(chunk) * 4;
                Vector3[] positions = mesh.vertices;
                var chunkReport = new ChunkEvidence { chunk = chunk, physicalCount = mesh.subMeshCount,
                    activeVertices = active, nativeBounds = mesh.bounds };
                report.chunks.Add(chunkReport);
                Assert.That(Finite(mesh.bounds), Is.True, "native bounds");
                Bounds cpu = new Bounds(positions[0], Vector3.zero);
                for (int i = 0; i < active; i++)
                {
                    Assert.That(Finite(positions[i]), Is.True, "active vertex");
                    cpu.Encapsulate(positions[i]);
                }
                chunkReport.cpuBounds = cpu;
                Assert.That(Vector3.Distance(cpu.center, mesh.bounds.center), Is.LessThan(0.0001f));
                Assert.That(Vector3.Distance(cpu.size, mesh.bounds.size), Is.LessThan(0.0001f));
                int covered = 0;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    SubMeshDescriptor d = mesh.GetSubMesh(sub);
                    chunkReport.descriptors.Add(new DescriptorEvidence { indexStart = d.indexStart,
                        indexCount = d.indexCount, firstVertex = d.firstVertex, vertexCount = d.vertexCount, bounds = d.bounds });
                    Assert.That(Finite(d.bounds), Is.True, "descriptor bounds");
                    Assert.That(d.indexStart + d.indexCount, Is.LessThanOrEqualTo(BattleDynamicMeshBackend.IndicesPerChunk));
                    Assert.That(d.firstVertex + d.vertexCount, Is.LessThanOrEqualTo(active));
                    Assert.That(d.baseVertex, Is.Zero);
                    Assert.That(d.indexCount, Is.EqualTo(d.vertexCount / 4 * 6));
                    if (d.indexCount > 0)
                    {
                        covered++;
                        int[] indices = mesh.GetIndices(sub);
                        foreach (int index in indices) Assert.That(index, Is.InRange(d.firstVertex, d.firstVertex + d.vertexCount - 1));
                    }
                    else Assert.That(d.vertexCount, Is.Zero);
                }
                chunkReport.desiredActiveCount = covered;
                Assert.That(covered, Is.EqualTo(active / 4));
            }
        }

        private static bool Finite(Vector3 v) => !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
            float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
        private static bool Finite(Bounds b) => Finite(b.center) && Finite(b.extents);

        private static BattleRenderCommand Command(int i, Vector3 position, Vector2 size)
        {
            return new BattleRenderCommand(BattleRenderCommandType.Entity, RuntimeEntityHandle.Invalid,
                i, i & 1, i, 0, i, i, 0, i, position, size, new Vector2(0.5f,0.5f), new Rect(0,0,1,1), false, default);
        }

        private sealed class Resolver : IBattleCentralResourceResolver
        {
            internal bool zeroExtent;
            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                resource = new BattleCentralResolvedResource(null, null, new Rect(0,0,1,1),
                    zeroExtent ? Vector2.zero : Vector2.one, new Vector2(0.5f,0.5f), Color.white, command.VisualDataId);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        [Serializable] private sealed class Evidence
        {
            public string test, device;
            public long allocatedBytes;
            public double averageMicroseconds;
            public List<string> logs = new List<string>();
            public List<FrameEvidence> frames = new List<FrameEvidence>();
        }
        [Serializable] private sealed class FrameEvidence
        {
            public int commandCount, previousPhysicalFirstChunk, activeChunks, segments;
            public List<ChunkEvidence> chunks = new List<ChunkEvidence>();
        }
        [Serializable] private sealed class ChunkEvidence
        {
            public int chunk, physicalCount, desiredActiveCount, activeVertices;
            public Bounds nativeBounds, cpuBounds;
            public List<DescriptorEvidence> descriptors = new List<DescriptorEvidence>();
        }
        [Serializable] private sealed class DescriptorEvidence
        {
            public int indexStart, indexCount, firstVertex, vertexCount;
            public Bounds bounds;
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

    }
}
#endif
