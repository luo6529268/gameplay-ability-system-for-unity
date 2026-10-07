#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
#endif

namespace NTSD.Test.Editor
{
    internal static class BattleProductionCatalogReplayEditor
    {
        private const int Warmup = 64;
        private const int Samples = 1800;
        private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly int MainTex = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexArray = Shader.PropertyToID("_MainTexArray");
        private static readonly string InvalidWindowParameter = "targetCameras";
        private static readonly string InvalidWindowMessage = "Invalid bounded camera window.";

        [Serializable]
        private sealed class Result
        {
            public string status = "RUNNING";
            public string error;
            public string scope = "Repeated already-sampled body/shadow commands; duplicate handles, EntityCount=0, no World entities or AI. " +
                "Production catalog resolver and active Foot/Health geometry; Graphics.ExecuteCommandBuffer, not production RenderPass. " +
                "CPU timings, not GPU draw/batch/time/FPS or Android certification. No Q06 sorting/interpolation oracle or raw GPU buffer fence proof.";
            public int sourceTick;
            public int sourceEntityCount;
            public int sourceCommandCount;
            public Template bodyTemplate;
            public Template shadowTemplate;
            public Case[] cases;
        }

        [Serializable]
        private sealed class Template
        {
            public int sourceCommandIndex;
            public Vector3 position;
            public Vector2 size;
            public Vector2 pivot;
            public Rect uv;
            public Color32 tint;
            public bool flipX;
            public bool flipY;
            public bool footEnabled;
            public bool healthEnabled;

            public Template(in BattleRenderCommand command, int index)
            {
                sourceCommandIndex = index;
                position = command.Position;
                size = command.Size;
                pivot = command.Pivot;
                uv = command.NormalizedUv;
                tint = command.Color;
                flipX = command.FlipX;
                flipY = command.FlipY;
                footEnabled = command.ShowSelfFootMarker;
                healthEnabled = command.ShowOverheadHealthBar;
            }
        }

        [Serializable]
        private sealed class Case
        {
            public int repeatedBodies;
            public int commandCount;
            public int warmup = Warmup;
            public int samples;
            public long allocatedBytes;
            public int nonzeroAllocationSamples;
            public int capacityGrowth;
            public int chunks;
            public int segments;
            public int footMarkers;
            public int healthBars;
            public int footCapacity;
            public int healthCapacity;
            public int sourceTextureSegments;
            public int atlasPageSegments;
            public int textureArraySegments;
            public int cpuDrawMeshCommands;
            public int graphicsExecuteCalls;
            public int resolverNoOpHits;
            public int resolverSealedCacheSkips;
            public int resolverBindingGeneration;
            public long uploadedVertexBytes;
            public double meanCpuMs;
            public double maxCpuMs;
            public int[] textureIdentities;
            public int[] materialIdentities;
            public int[] meshIdentities;
            public string[] bindingModes;
        }

        internal static bool IsNextCameraFrame(int previous, int current) => current > previous;

        internal static bool PrepareCameraWindowValidation()
        {
            return !string.IsNullOrEmpty(InvalidWindowParameter) && !string.IsNullOrEmpty(InvalidWindowMessage);
        }

        internal static bool IsWindowComplete(int targetCameras, int count, int startTick, int endTick,
            int requiredTicks, int capacity)
        {
            if (capacity <= 0 || targetCameras < 0 || targetCameras > capacity || count < 0 || count > capacity ||
                endTick < startTick || requiredTicks < 0)
                throw new ArgumentOutOfRangeException(InvalidWindowParameter, InvalidWindowMessage);
            return targetCameras > 0 ? count >= targetCameras : endTick - startTick >= requiredTicks;
        }

        internal static void Run(BattlePixelFramePlan plan, string output)
        {
            var result = new Result();
            try
            {
                Require(plan.UsesCentralPixels && !plan.IsStale && plan.CapturedFrame != null,
                    "Requires a current actual materialized production plan.");
                BattlePresentationFrame source = plan.CapturedFrame;
                Require(source.BoundCatalog != null && !ReferenceEquals(source.BoundCatalog, BattleSpriteCatalog.Empty),
                    "Actual bound production catalog is missing.");
                result.sourceTick = source.TickIndex;
                result.sourceEntityCount = source.EntityCount;
                result.sourceCommandCount = source.CommandCount;
                var commandsBefore = new BattleRenderCommand[source.CommandCount];
                BattleRenderCommand body = default, shadow = default;
                bool bodyFound = false, shadowFound = false;
                for (int index = 0; index < source.CommandCount; index++)
                {
                    BattleRenderCommand command = source.GetCommand(index);
                    commandsBefore[index] = command;
                    if (!bodyFound && command.Type == BattleRenderCommandType.Entity &&
                        command.ShowSelfFootMarker && command.ShowOverheadHealthBar && command.MaximumHealth > 0)
                    {
                        body = command;
                        result.bodyTemplate = new Template(command, index);
                        bodyFound = true;
                    }
                    if (!shadowFound && command.Type == BattleRenderCommandType.Shadow)
                    {
                        shadow = command;
                        result.shadowTemplate = new Template(command, index);
                        shadowFound = true;
                    }
                }
                Require(bodyFound && shadowFound, "No actual body with both auxiliary flags and shadow template.");
                Material material = BattleCentralRenderSystem.RegisteredFeatureMaterialForAcceptance;
                Material arrayMaterial = BattleCentralRenderSystem.RegisteredFeatureArrayMaterialForAcceptance;
                Sprite footSprite = ReadField<Sprite>("runtimeFootMarkerSprite");
                BattleFootMarkerStyle footStyle = ReadField<BattleFootMarkerStyle>("runtimeFootMarkerStyle");
                BattleHealthBarStyle healthStyle = ReadField<BattleHealthBarStyle>("runtimeHealthBarStyle");
                BattleCentralDrawMode mode = ReadField<BattleCentralDrawMode>("drawMode");
                Require(material != null && footSprite != null && footSprite.texture != null,
                    "Actual production auxiliary material/foot sprite missing.");
                result.cases = new Case[3];
                int[] counts = { 100, 500, 1000 };
                for (int index = 0; index < counts.Length; index++)
                {
                    result.cases[index] = new Case { repeatedBodies = counts[index], commandCount = counts[index] * 2 };
                    RunCase(result.cases[index], source, body, shadow,
                        material, arrayMaterial, footSprite, footStyle, healthStyle, mode);
                }
                Require(source.CommandCount == result.sourceCommandCount &&
                    source.EntityCount == result.sourceEntityCount && ReferenceEquals(plan.CapturedFrame, source),
                    "Replay mutated the production source frame.");
                for (int index = 0; index < commandsBefore.Length; index++)
                    Require(commandsBefore[index].Equals(source.GetCommand(index)), "Replay mutated a production source command.");
                result.status = "PASS";
            }
            catch (Exception exception)
            {
                result.status = "FAIL";
                result.error = exception.ToString();
                throw;
            }
            finally
            {
                byte[] bytes = System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(result, true));
                using var stream = new FileStream(output, FileMode.CreateNew, FileAccess.Write);
                stream.Write(bytes, 0, bytes.Length);
            }
        }

        private static void RunCase(Case result, BattlePresentationFrame source, BattleRenderCommand body,
            BattleRenderCommand shadow, Material material, Material arrayMaterial, Sprite footSprite,
            BattleFootMarkerStyle footStyle, BattleHealthBarStyle healthStyle, BattleCentralDrawMode mode)
        {
            // Alignment contract: NTSD-OPT-M03-PRODUCTION-CATALOG-020. Never installs this command fixture in the World.
            var frame = new BattlePresentationFrame { TickIndex = source.TickIndex };
            for (int index = 0; index < result.repeatedBodies; index++)
            {
                frame.AddCommand(shadow);
                frame.AddCommand(body);
            }
            var resolver = new BattleCatalogCentralResourceResolver();
            resolver.PrepareCapacity(128, 128);
            resolver.Configure(source.BoundCatalog, source.CommonVisualCatalog, material, arrayMaterial);
            resolver.SealCapacity();
            using var backend = new BattleDynamicMeshBackend();
            using var foot = new BattleFootMarkerBatchBackend();
            using var health = new BattleHealthBarBatchBackend();
            backend.PrepareCapacity(result.commandCount);
            foot.PrepareCapacity(result.repeatedBodies);
            health.PrepareCapacity(result.repeatedBodies);
            Seal(backend, result.commandCount);
            Seal(foot, result.repeatedBodies);
            Seal(health, result.repeatedBodies);
            var block = new MaterialPropertyBlock();
            var target = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGB32)
                { hideFlags = HideFlags.HideAndDontSave };
            RenderTexture previous = RenderTexture.active;
            try
            {
                Require(target.Create(), "Replay render target creation failed.");
                for (int index = 0; index < Warmup; index++)
                    Execute(frame, source, resolver, backend, foot, health, material, arrayMaterial, footSprite,
                        footStyle, healthStyle, mode, target, block);
                result.meshIdentities = new int[backend.ActiveChunkCount];
                for (int index = 0; index < result.meshIdentities.Length; index++)
                    result.meshIdentities[index] = backend.GetChunkMesh(index).GetInstanceID();
                int footCapacity = foot.Capacity, healthCapacity = health.Capacity;
                long elapsedTicks = 0, maximumTicks = 0;
                for (int index = 0; index < Samples; index++)
                {
                    long allocationStart = GC.GetAllocatedBytesForCurrentThread();
                    long started = Stopwatch.GetTimestamp();
                    int draws = Execute(frame, source, resolver, backend, foot, health, material, arrayMaterial,
                        footSprite, footStyle, healthStyle, mode, target, block);
                    long elapsed = Stopwatch.GetTimestamp() - started;
                    long allocated = GC.GetAllocatedBytesForCurrentThread() - allocationStart;
                    result.allocatedBytes += allocated;
                    if (allocated != 0) result.nonzeroAllocationSamples++;
                    elapsedTicks += elapsed;
                    maximumTicks = Math.Max(maximumTicks, elapsed);
                    result.samples++;
                    result.cpuDrawMeshCommands += draws;
                    result.graphicsExecuteCalls++;
                    result.capacityGrowth += backend.Diagnostics.CapacityGrowthCount;
                    Require(backend.Diagnostics.UnresolvedCommandCount == 0 &&
                        backend.Diagnostics.ResolvedCommandCount == result.commandCount &&
                        foot.ActiveMarkerCount == result.repeatedBodies && health.ActiveBarCount == result.repeatedBodies &&
                        foot.Capacity == footCapacity && health.Capacity == healthCapacity,
                        "Replay capacity/resource/active auxiliary coverage changed.");
                    Require(backend.ActiveChunkCount == result.meshIdentities.Length,
                        "Replay allocated an additional mesh chunk.");
                    for (int meshIndex = 0; meshIndex < result.meshIdentities.Length; meshIndex++)
                        Require(backend.GetChunkMesh(meshIndex).GetInstanceID() == result.meshIdentities[meshIndex] &&
                            backend.GetChunkMesh(meshIndex).GetVertexBufferStride(0) == 44,
                            "Replay mesh identity/layout changed.");
                }
                result.chunks = backend.ActiveChunkCount;
                result.segments = backend.SegmentCount;
                result.footMarkers = foot.ActiveMarkerCount;
                result.healthBars = health.ActiveBarCount;
                result.footCapacity = footCapacity;
                result.healthCapacity = healthCapacity;
                result.resolverNoOpHits = resolver.NoOpHits;
                result.resolverSealedCacheSkips = resolver.SealedCapacityCacheSkips;
                result.resolverBindingGeneration = resolver.BindingGeneration;
                result.uploadedVertexBytes = backend.Diagnostics.UploadedVertexBytes;
                result.meanCpuMs = elapsedTicks * 1000.0 / Stopwatch.Frequency / Samples;
                result.maxCpuMs = maximumTicks * 1000.0 / Stopwatch.Frequency;
                result.textureIdentities = new int[backend.SegmentCount];
                result.materialIdentities = new int[backend.SegmentCount];
                result.bindingModes = new string[backend.SegmentCount];
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    result.textureIdentities[index] = segment.Texture.GetInstanceID();
                    result.materialIdentities[index] = segment.Material.GetInstanceID();
                    result.bindingModes[index] = segment.BindingMode.ToString();
                    if (segment.BindingMode == BattleSpriteCentralBindingMode.SourceTexture2D) result.sourceTextureSegments++;
                    if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasPageTexture2D) result.atlasPageSegments++;
                    if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray) result.textureArraySegments++;
                }
                Require(result.allocatedBytes == 0 && result.nonzeroAllocationSamples == 0 && result.capacityGrowth == 0 &&
                    resolver.SealedCapacityCacheSkips == 0, "Replay current-thread 0B/no-growth/cache seal failed.");
            }
            finally
            {
                RenderTexture.active = previous;
                target.Release();
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        private static int Execute(BattlePresentationFrame frame, BattlePresentationFrame source,
            BattleCatalogCentralResourceResolver resolver, BattleDynamicMeshBackend backend,
            BattleFootMarkerBatchBackend foot, BattleHealthBarBatchBackend health, Material material,
            Material arrayMaterial, Sprite footSprite, BattleFootMarkerStyle footStyle, BattleHealthBarStyle healthStyle,
            BattleCentralDrawMode mode, RenderTexture target, MaterialPropertyBlock block)
        {
            resolver.Configure(source.BoundCatalog, source.CommonVisualCatalog, material, arrayMaterial);
            backend.Build(frame, resolver, mode);
            foot.BuildFromFrame(frame, footSprite, footStyle, true);
            health.BuildFromFrame(frame, healthStyle, true);
            CommandBuffer buffer = CommandBufferPool.Get("NTSD Batch20 Production Catalog CPU Replay");
            try
            {
                buffer.SetRenderTarget(target);
                buffer.ClearRenderTarget(false, true, Color.clear);
                buffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.Ortho(-20, 20, -20, 20, -20, 20));
                block.Clear();
                block.SetTexture(MainTex, foot.Texture);
                buffer.DrawMesh(foot.Mesh, Matrix4x4.identity, material, 0, 0, block);
                for (int index = 0; index < backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = backend.GetSegment(index);
                    Require(segment.Texture != null && segment.Material != null, "Invalid actual catalog binding.");
                    block.Clear();
                    block.SetTexture(segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ? MainTexArray : MainTex,
                        segment.Texture);
                    buffer.DrawMesh(backend.GetChunkMesh(segment.ChunkIndex), Matrix4x4.identity,
                        segment.Material, segment.SubMeshIndex, 0, block);
                }
                block.Clear();
                block.SetTexture(MainTex, Texture2D.whiteTexture);
                buffer.DrawMesh(health.Mesh, Matrix4x4.identity, material, 0, 0, block);
                Graphics.ExecuteCommandBuffer(buffer);
                return backend.SegmentCount + 2;
            }
            finally
            {
                CommandBufferPool.Release(buffer);
            }
        }

        private static T ReadField<T>(string name) =>
            (T)typeof(BattleCentralRenderSystem).GetField(name, PrivateStatic).GetValue(null);

        private static void Seal(object backend, int count) =>
            backend.GetType().GetMethod("SealCapacity", PrivateInstance).Invoke(backend, new object[] { count });

        private static void Require(bool condition, string reason)
        {
            if (!condition) throw new InvalidOperationException(reason);
        }
    }

#if UNITY_INCLUDE_TESTS
    public sealed class BattleProductionCatalogReplayPolicyEditorTests
    {
        [Test]
        public void CameraTarget_IgnoresTickCompletionUntil1800Samples()
        {
            Assert.That(BattleProductionCatalogReplayEditor.IsWindowComplete(1800, 1799, 0, 10000, 96, 2048), Is.False);
            Assert.That(BattleProductionCatalogReplayEditor.IsWindowComplete(1800, 1800, 0, 1, 96, 2048), Is.True);
        }

        [Test]
        public void CameraIdentity_RejectsDuplicateAndBackwardFrames()
        {
            Assert.That(BattleProductionCatalogReplayEditor.IsNextCameraFrame(100, 100), Is.False);
            Assert.That(BattleProductionCatalogReplayEditor.IsNextCameraFrame(100, 99), Is.False);
            Assert.That(BattleProductionCatalogReplayEditor.IsNextCameraFrame(100, 101), Is.True);
        }

        [Test]
        public void LegacyWindow_RetainsTickCompletion()
        {
            Assert.That(BattleProductionCatalogReplayEditor.IsWindowComplete(0, 10, 8, 103, 96, 2048), Is.False);
            Assert.That(BattleProductionCatalogReplayEditor.IsWindowComplete(0, 10, 8, 104, 96, 2048), Is.True);
        }

        [TestCase(2049, 1, 0, 1, 96, 2048)]
        [TestCase(1800, 2049, 0, 1, 96, 2048)]
        [TestCase(1800, 1, 8, 7, 96, 2048)]
        [TestCase(-1, 1, 0, 1, 96, 2048)]
        public void InvalidWindow_FailsClosed(int target, int count, int start, int end, int ticks, int capacity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                BattleProductionCatalogReplayEditor.IsWindowComplete(target, count, start, end, ticks, capacity));
        }
    }
#endif
}
#endif
