#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Animation.Rendering.Editor
{
    public sealed class BattleMeshUploadBaselineEditorTests
    {
        private const int WarmupBuilds = 64;
        private const int SampleBuilds = 1800;

        [TestCase(100, BattleCentralDrawMode.OrderedChunks, false, false)]
        [TestCase(100, BattleCentralDrawMode.OrderedChunks, true, false)]
        [TestCase(500, BattleCentralDrawMode.OrderedChunks, false, false)]
        [TestCase(500, BattleCentralDrawMode.OrderedChunks, true, false)]
        [TestCase(1000, BattleCentralDrawMode.OrderedChunks, false, false)]
        [TestCase(1000, BattleCentralDrawMode.OrderedChunks, true, false)]
        [TestCase(1000, BattleCentralDrawMode.StrictOrderedDraw, false, false)]
        [TestCase(1000, BattleCentralDrawMode.StrictOrderedDraw, true, false)]
        [TestCase(1000, BattleCentralDrawMode.OrderedChunks, false, true)]
        [TestCase(1000, BattleCentralDrawMode.OrderedChunks, true, true)]
        [TestCase(4097, BattleCentralDrawMode.OrderedChunks, false, false)]
        [TestCase(4097, BattleCentralDrawMode.OrderedChunks, true, false)]
        public void CurrentBackend_ControlledWarmUploadBaseline(
            int commandCount, BattleCentralDrawMode mode, bool changePosition, bool alternateVariant)
        {
            Assert.That(Application.isPlaying, Is.False, "This baseline is EditMode only.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            string scenePath = scene.path;
            bool sceneDirty = scene.isDirty;
            int sceneRoots = scene.rootCount;
            var first = CreateFrame(commandCount, 0f);
            var second = changePosition ? CreateFrame(commandCount, 0.25f) : first;
            var resolver = new ControlledResolver(alternateVariant);
            var samples = new long[SampleBuilds];
            int expectedChunks = (commandCount + BattleDynamicMeshBackend.QuadsPerChunk - 1) /
                BattleDynamicMeshBackend.QuadsPerChunk;
            int expectedSegments = mode == BattleCentralDrawMode.StrictOrderedDraw || alternateVariant
                ? commandCount : expectedChunks;

            using var backend = new BattleDynamicMeshBackend();
            backend.PrepareCapacity(commandCount);
            typeof(BattleDynamicMeshBackend).GetMethod("SealCapacity",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(backend, new object[] { commandCount });
            for (int index = 0; index < WarmupBuilds; index++)
                backend.Build((index & 1) == 0 ? first : second, resolver, mode);
            var meshes = new Mesh[expectedChunks];
            var meshIds = new int[expectedChunks];
            var strides = new int[expectedChunks];
            long expectedBytesPerBuild = 0;
            for (int chunk = 0; chunk < expectedChunks; chunk++)
            {
                meshes[chunk] = backend.GetChunkMesh(chunk);
                meshIds[chunk] = meshes[chunk].GetInstanceID();
                strides[chunk] = meshes[chunk].GetVertexBufferStride(0);
                expectedBytesPerBuild += (long)backend.GetChunkActiveQuadCount(chunk) *
                    BattleDynamicMeshBackend.VerticesPerQuad * strides[chunk];
            }

            long calls = 0;
            long vertices = 0;
            long bytes = 0;
            long resolved = 0;
            long growths = 0;
            long segments = 0;
            long activeChunks = 0;
            long resolverBefore = resolver.ResolveCalls;
            long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int index = 0; index < SampleBuilds; index++)
            {
                long startedAt = Stopwatch.GetTimestamp();
                backend.Build((index & 1) == 0 ? first : second, resolver, mode);
                samples[index] = Stopwatch.GetTimestamp() - startedAt;
                BattleCentralBuildDiagnostics diagnostics = backend.Diagnostics;
                calls += diagnostics.VertexUploadCallCount;
                vertices += diagnostics.UploadedVertexCount;
                bytes += diagnostics.UploadedVertexBytes;
                resolved += diagnostics.ResolvedCommandCount;
                growths += diagnostics.CapacityGrowthCount;
                segments += diagnostics.SegmentCount;
                activeChunks += diagnostics.ActiveChunkCount;
            }
            long allocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
            long resolverCalls = resolver.ResolveCalls - resolverBefore;
            long expectedVertices = (long)SampleBuilds * commandCount * BattleDynamicMeshBackend.VerticesPerQuad;

            Assert.That(resolverCalls, Is.EqualTo((long)SampleBuilds * commandCount));
            Assert.That(resolved, Is.EqualTo(resolverCalls));
            Assert.That(calls, Is.EqualTo((long)SampleBuilds * expectedChunks));
            Assert.That(vertices, Is.EqualTo(expectedVertices));
            Assert.That(bytes, Is.EqualTo((long)SampleBuilds * expectedBytesPerBuild));
            Assert.That(segments, Is.EqualTo((long)SampleBuilds * expectedSegments));
            Assert.That(activeChunks, Is.EqualTo((long)SampleBuilds * expectedChunks));
            Assert.That(growths, Is.Zero);
            Assert.That(allocatedBytes, Is.Zero, "Only prepared entity Mesh Build/upload and scalar sampling are measured.");
            for (int chunk = 0; chunk < expectedChunks; chunk++)
            {
                Assert.That(backend.GetChunkMesh(chunk), Is.SameAs(meshes[chunk]));
                Assert.That(backend.GetChunkMesh(chunk).GetInstanceID(), Is.EqualTo(meshIds[chunk]));
                Assert.That(backend.GetChunkMesh(chunk).GetVertexBufferStride(0), Is.EqualTo(strides[chunk]));
            }
            scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            Assert.That(scene.path, Is.EqualTo(scenePath));
            Assert.That(scene.isDirty, Is.EqualTo(sceneDirty));
            Assert.That(scene.rootCount, Is.EqualTo(sceneRoots));

            long totalTicks = 0;
            for (int index = 0; index < samples.Length; index++)
                totalTicks += samples[index];
            Array.Sort(samples);
            var report = new Dictionary<string, object>
            {
                ["schemaVersion"] = "ntsd-m03-controlled-mesh-backend-baseline-v1",
                ["scope"] = "editmode-controlled-entity-mesh-build-upload",
                ["observedAtUtc"] = DateTime.UtcNow.ToString("o"),
                ["unityVersion"] = Application.unityVersion,
                ["graphicsApi"] = SystemInfo.graphicsDeviceType.ToString(),
                ["graphicsDevice"] = SystemInfo.graphicsDeviceName,
                ["pipelineType"] = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline == null
                    ? "BuiltIn" : UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.GetType().FullName,
                ["scenePath"] = scenePath,
                ["drawMode"] = mode.ToString(),
                ["commandCount"] = commandCount,
                ["positionChangedBetweenBuilds"] = changePosition,
                ["alternateMaterialVariant"] = alternateVariant,
                ["warmupBuilds"] = WarmupBuilds,
                ["sampleBuilds"] = SampleBuilds,
                ["stopwatchFrequency"] = Stopwatch.Frequency,
                ["cpuBuildMsP50"] = MillisecondsAtRank(samples, 0.50),
                ["cpuBuildMsP95"] = MillisecondsAtRank(samples, 0.95),
                ["cpuBuildMsP99"] = MillisecondsAtRank(samples, 0.99),
                ["cpuBuildMsMax"] = samples[samples.Length - 1] * 1000.0 / Stopwatch.Frequency,
                ["cpuBuildMsMean"] = totalTicks * 1000.0 / Stopwatch.Frequency / SampleBuilds,
                ["actualVertexStrides"] = strides,
                ["meshInstanceIds"] = meshIds,
                ["vertexUploadCallCount"] = calls,
                ["uploadedVertexCount"] = vertices,
                ["uploadedVertexBytes"] = bytes,
                ["resolvedCommandCount"] = resolved,
                ["resolverCalls"] = resolverCalls,
                ["physicalSegmentCountSum"] = segments,
                ["activeChunkCountSum"] = activeChunks,
                ["capacityGrowthCount"] = growths,
                ["currentThreadManagedBytes"] = allocatedBytes,
                ["resourceScope"] = "synthetic-resolved-binding-null-texture-null-material-no-catalog-or-GPU-draw",
                ["limitations"] = "Actual backend Build/API return baseline, not natural publication, interpolation, real catalog, " +
                    "1000 entities/AI, RenderPass, CPU DrawMesh, GPU batch/traffic/timing, Player FPS or Android certification. " +
                    "Repeated geometry intentionally still invokes Build; queued same-sample gates are outside this scope. " +
                    "Warmup/setup, readback assertions and JSON export are outside the allocation/timing window. " +
                    "Scalar accumulation is included in allocation scope, but CPU elapsed brackets Build only with timestamp overhead. " +
                    "No speedup or hard frame-time threshold is asserted; no production counters are written.",
            };
            TestContext.Out.WriteLine("NTSD_M03_BACKEND_BASELINE " + BattleCanonicalJson.Serialize(report));
        }

        private static double MillisecondsAtRank(long[] sorted, double percentile)
        {
            int index = Math.Max(0, (int)Math.Ceiling(sorted.Length * percentile) - 1);
            return sorted[index] * 1000.0 / Stopwatch.Frequency;
        }

        private static BattlePresentationFrame CreateFrame(int count, float xOffset)
        {
            var frame = new BattlePresentationFrame();
            for (int index = 0; index < count; index++)
            {
                frame.AddCommand(new BattleRenderCommand(
                    BattleRenderCommandType.Entity, RuntimeEntityHandle.Invalid,
                    index, index, index, 0, index, index, 0, index,
                    new Vector3(index + xOffset, 0f, 0f),
                    Vector2.one, new Vector2(0.5f, 0.5f), new Rect(0f, 0f, 1f, 1f),
                    false, default));
            }
            return frame;
        }

        private sealed class ControlledResolver : IBattleCentralResourceResolver
        {
            private readonly bool alternateVariant;

            public ControlledResolver(bool alternateVariant)
            {
                this.alternateVariant = alternateVariant;
            }

            public long ResolveCalls { get; private set; }

            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                ResolveCalls++;
                resource = new BattleCentralResolvedResource(
                    null, null, new Rect(0f, 0f, 1f, 1f), Vector2.one,
                    new Vector2(0.5f, 0.5f), new Color32(255, 255, 255, 255),
                    alternateVariant ? command.VisualDataId & 1 : 0);
                return BattleCentralResourceStatus.Resolved;
            }
        }
    }
}
#endif
