#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using NTSD.Animation;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NTSD.Test.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace NTSD.Test
{
    public sealed class BattleRealTextureSubmissionEditorTests
    {
        private const int WarmupSamples = 64;
        private const int SampleCount = 1800;
        private const string OutputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH19-REAL-TEXTURE-SUBMISSION-20261007";
        private const string FirstTexturePath = "Assets/NTSD/Sprite/Character/MingRen/naruto_0.bmp";
        private const string SecondTexturePath = "Assets/NTSD/Sprite/Character/Zuozhu/sasuke_0.bmp";
        private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly Matrix4x4 Projection = Matrix4x4.Ortho(-2f, 2f, -2f, 2f, -1f, 1f);

        private delegate bool AcquireLease(BattleCentralSubmission submission,
            out BattleCentralSubmission.BattleCentralSubmissionLease lease);

        [Serializable]
        private sealed class Result
        {
            public string status = "FAIL";
            public string error = "";
            public string observedAtUtc;
            public string unityVersion;
            public string graphicsApi;
            public string graphicsDevice;
            public string pipeline;
            public string drawMode;
            public bool alternatingTextures;
            public int commandCount;
            public int activeAuxiliariesPerSample;
            public string outputRoot = OutputRoot;
            public int warmupSamples = WarmupSamples;
            public int sampleCount = SampleCount;
            public int physicalSegmentsPerSample;
            public int activeChunksPerSample;
            public int vertexStride;
            public long managedAllocatedBytes;
            public string allocationCounter = "GC.GetAllocatedBytesForCurrentThread / RAW_UNCALIBRATED";
            public string gcAllocUnit;
            public int gcAllocCapacity;
            public BattleScopedGcAllocationRecorder.CalibrationResult gcAllocBefore;
            public BattleScopedGcAllocationRecorder.CalibrationResult gcAllocAfter;
            public BattleScopedGcAllocationRecorder.ScopeResult gcAllocScope;
            public long capacityGrowth;
            public long vertexUploadCalls;
            public long uploadedVertexBytes;
            public long cpuDrawMeshCommands;
            public int graphicsExecuteCalls;
            public int slotsObserved;
            public int remainingCpuLeases;
            public bool sourceUnchanged;
            public bool sceneUnchanged;
            public string firstTextureIdentity;
            public string secondTextureIdentity;
            public string firstTextureSha256;
            public string secondTextureSha256;
            public string firstTextureMetaSha256;
            public string secondTextureMetaSha256;
            public double cpuBridgeMsMean;
            public double cpuBridgeMsP50;
            public double cpuBridgeMsP95;
            public double cpuBridgeMsMax;
            public double captureAndMotionMsMean;
            public double buildAndUploadMsMean;
            public double recordingMsMean;
            public double executeApiMsMean;
            public string limitations =
                "Prepared Editor CPU bridge with preordered synthetic publications and real Unity imported Texture2D assets. " +
                "Not production DAT decoding/catalog resolution, Q06 sorting, natural publication, 1000 AI, physical display frames, " +
                "ScriptableRenderContext/production RenderPass, active Foot/Health, PlayerLoop/other-thread/native allocation, GPU completion/batch, " +
                "FPS, Android certification or an optimization A/B. CPU API return may include driver wait; lease0/Execute return is not a GPU fence. " +
                "Asset loading, reflection/delegate creation, warmup, assertions, sorting and export are outside sampling.";
        }

        [TestCase(100, BattleCentralDrawMode.OrderedChunks, false)]
        [TestCase(500, BattleCentralDrawMode.OrderedChunks, false)]
        [TestCase(1000, BattleCentralDrawMode.OrderedChunks, false)]
        [TestCase(1000, BattleCentralDrawMode.StrictOrderedDraw, false)]
        [TestCase(1000, BattleCentralDrawMode.OrderedChunks, true)]
        [TestCase(4097, BattleCentralDrawMode.OrderedChunks, false)]
        public void RealTextures_PreparedCaptureMotionUploadRecordExecuteLease_ZeroGc(
            int commandCount, BattleCentralDrawMode mode, bool alternatingTextures)
        {
            Run(commandCount, mode, alternatingTextures, 0);
        }

        [TestCase(1000, BattleCentralDrawMode.StrictOrderedDraw, 1000)]
        [TestCase(4097, BattleCentralDrawMode.OrderedChunks, 17)]
        public void ActiveAuxiliaries_FullPreparedCpuChain_TwoSlotsZeroGc(
            int commandCount, BattleCentralDrawMode mode, int auxiliaryCount)
        {
            Run(commandCount, mode, false, auxiliaryCount);
        }

        private static void Run(int commandCount, BattleCentralDrawMode mode,
            bool alternatingTextures, int auxiliaryCount)
        {
            Scene scene = SceneManager.GetActiveScene();
            string scenePath = scene.path;
            bool sceneDirty = scene.isDirty;
            int sceneRoots = scene.rootCount;
            RenderTexture previousTarget = RenderTexture.active;
            var result = new Result
            {
                observedAtUtc = DateTime.UtcNow.ToString("o"),
                unityVersion = Application.unityVersion,
                graphicsApi = SystemInfo.graphicsDeviceType.ToString(),
                graphicsDevice = SystemInfo.graphicsDeviceName,
                pipeline = GraphicsSettings.currentRenderPipeline == null ? "BuiltIn" :
                    GraphicsSettings.currentRenderPipeline.GetType().FullName,
                drawMode = mode.ToString(),
                alternatingTextures = alternatingTextures,
                commandCount = commandCount,
                activeAuxiliariesPerSample = auxiliaryCount,
                firstTextureSha256 = Sha256(FirstTexturePath),
                secondTextureSha256 = Sha256(SecondTexturePath),
                firstTextureMetaSha256 = Sha256(FirstTexturePath + ".meta"),
                secondTextureMetaSha256 = Sha256(SecondTexturePath + ".meta"),
            };
            Slot[] slots = null;
            Sprite auxiliarySprite = null;
            Material material = null;
            RenderTexture target = null;
            BattleScopedGcAllocationRecorder allocationRecorder = null;
            try
            {
                Texture2D firstTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(FirstTexturePath);
                Texture2D secondTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(SecondTexturePath);
                Assert.That(firstTexture, Is.Not.Null, "Existing texture importer must be ready; do not rewrite the asset.");
                Assert.That(secondTexture, Is.Not.Null);
                Assert.That(firstTexture, Is.Not.SameAs(secondTexture));
                result.firstTextureIdentity = Identity(firstTexture);
                result.secondTextureIdentity = Identity(secondTexture);
                Shader shader = Shader.Find("NTSD/BattleCentralTransparent");
                Assert.That(shader, Is.Not.Null);
                material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                target = new RenderTexture(16, 16, 0, RenderTextureFormat.ARGB32)
                {
                    hideFlags = HideFlags.HideAndDontSave,
                    filterMode = FilterMode.Point,
                    antiAliasing = 1,
                };
                target.Create();
                if (auxiliaryCount > 0)
                {
                    result.outputRoot = "artifacts/diagnostics/NTSD-OPTIMIZATION-BATCH27-CALIBRATED-GC-20261007/cpu-bridge";
                    result.limitations = result.limitations.Replace("active Foot/Health, ", "") +
                        " Active auxiliaries use a temporary sprite from the borrowed source texture, not production GameConfig authoring.";
                    auxiliarySprite = Sprite.Create(firstTexture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 1f);
                }
                var source = MakePublication(commandCount, auxiliaryCount);
                var resolver = new RealTextureResolver(firstTexture, secondTexture, material,
                    alternatingTextures, commandCount);
                var motion = new BattlePresentationDisplayMotion();
                motion.PrepareCapacity(commandCount + 1);
                typeof(BattlePresentationDisplayMotion).GetMethod("SealCapacity", PrivateInstance)
                    .Invoke(motion, new object[] { commandCount + 1 });
                slots = new[] { new Slot(commandCount), new Slot(commandCount) };
                var block = new MaterialPropertyBlock();
                var sampleTicks = new long[SampleCount];
                long captureTicks = 0;
                long buildTicks = 0;
                long recordTicks = 0;
                long executeTicks = 0;
                for (int index = 0; index < WarmupSamples; index++)
                    Execute(slots[index & 1], source, motion, resolver, mode, target, block,
                        index, out _, out _, out _, out _, auxiliarySprite, material);
                Mesh firstMesh = slots[0].Backend.GetChunkMesh(0);
                Mesh secondMesh = slots[1].Backend.GetChunkMesh(0);
                int expectedChunks = (commandCount + BattleDynamicMeshBackend.QuadsPerChunk - 1) /
                    BattleDynamicMeshBackend.QuadsPerChunk;
                int expectedSegments = ExpectedSegments(commandCount, mode, alternatingTextures);
                int stride = firstMesh.GetVertexBufferStride(0);
                result.vertexStride = stride;
                result.physicalSegmentsPerSample = expectedSegments;
                result.activeChunksPerSample = expectedChunks;
                Assert.That(stride, Is.EqualTo(secondMesh.GetVertexBufferStride(0)));
                Vector3 originalPosition = source.GetCommand(0).Position;
                double originalX = source.GetMotionState(0).PreciseX;
                if (auxiliaryCount > 0)
                {
                    allocationRecorder = new BattleScopedGcAllocationRecorder();
                    result.allocationCounter = "Current-thread GC.Alloc events; legacy byte count raw/uncalibrated";
                    result.gcAllocUnit = allocationRecorder.Unit;
                    result.gcAllocCapacity = allocationRecorder.Capacity;
                    result.gcAllocBefore = allocationRecorder.Calibrate();
                    Assert.That(result.gcAllocBefore.passed, Is.True, "Pre-scope positive/negative calibration failed.");
                    if (!allocationRecorder.Begin())
                        throw new InvalidOperationException("The calibrated GC.Alloc recorder could not start.");
                }
                long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
                try
                {
                    for (int index = 0; index < SampleCount; index++)
                    {
                        Slot slot = slots[index & 1];
                        long started = Stopwatch.GetTimestamp();
                        int draws = Execute(slot, source, motion, resolver, mode, target, block,
                            WarmupSamples + index, out long capture, out long build, out long record, out long execute,
                            auxiliarySprite, material);
                        sampleTicks[index] = Stopwatch.GetTimestamp() - started;
                        captureTicks += capture;
                        buildTicks += build;
                        recordTicks += record;
                        executeTicks += execute;
                        result.cpuDrawMeshCommands += draws;
                        result.graphicsExecuteCalls++;
                        result.capacityGrowth += slot.Backend.Diagnostics.CapacityGrowthCount;
                        result.vertexUploadCalls += slot.Backend.Diagnostics.VertexUploadCallCount;
                        result.uploadedVertexBytes += slot.Backend.Diagnostics.UploadedVertexBytes;
                    }
                    result.managedAllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - allocatedBefore;
                }
                finally
                {
                    if (allocationRecorder != null)
                        result.gcAllocScope = allocationRecorder.End();
                }
                if (allocationRecorder != null)
                {
                    result.gcAllocAfter = allocationRecorder.Calibrate();
                }
                result.slotsObserved = 2;
                result.remainingCpuLeases = slots[0].Submission.ReadLeaseCount + slots[1].Submission.ReadLeaseCount;
                result.sourceUnchanged = source.GetCommand(0).Position == originalPosition &&
                    source.GetMotionState(0).PreciseX == originalX && source.TickIndex == 101;
                result.sceneUnchanged = scene.path == scenePath && scene.isDirty == sceneDirty && scene.rootCount == sceneRoots;
                long totalTicks = 0;
                for (int index = 0; index < sampleTicks.Length; index++)
                    totalTicks += sampleTicks[index];
                Array.Sort(sampleTicks);
                result.cpuBridgeMsMean = MeanMs(totalTicks);
                result.cpuBridgeMsP50 = RankMs(sampleTicks, 0.50);
                result.cpuBridgeMsP95 = RankMs(sampleTicks, 0.95);
                result.cpuBridgeMsMax = sampleTicks[SampleCount - 1] * 1000.0 / Stopwatch.Frequency;
                result.captureAndMotionMsMean = MeanMs(captureTicks);
                result.buildAndUploadMsMean = MeanMs(buildTicks);
                result.recordingMsMean = MeanMs(recordTicks);
                result.executeApiMsMean = MeanMs(executeTicks);
                if (allocationRecorder != null)
                {
                    Assert.That(BattleScopedGcAllocationRecorder.HasZeroEvents(result.gcAllocScope,
                        result.gcAllocBefore, result.gcAllocAfter), Is.True,
                        "The full prepared CPU bridge, timing and scalar sampling must have calibrated zero GC.Alloc events.");
                }
                else
                {
                    Assert.That(result.managedAllocatedBytes, Is.Zero, "Legacy raw byte-count regression, not a calibrated certificate.");
                }
                Assert.That(result.capacityGrowth, Is.Zero);
                Assert.That(result.cpuDrawMeshCommands,
                    Is.EqualTo((long)SampleCount * (expectedSegments + (auxiliaryCount > 0 ? 2 : 0))));
                Assert.That(result.graphicsExecuteCalls, Is.EqualTo(SampleCount));
                Assert.That(result.vertexUploadCalls, Is.EqualTo((long)SampleCount * expectedChunks));
                Assert.That(result.uploadedVertexBytes, Is.EqualTo((long)SampleCount * commandCount * 4 * stride));
                Assert.That(result.remainingCpuLeases, Is.Zero);
                Assert.That(result.sourceUnchanged, Is.True);
                Assert.That(result.sceneUnchanged, Is.True);
                Assert.That(slots[0].Backend.GetChunkMesh(0), Is.SameAs(firstMesh));
                Assert.That(slots[1].Backend.GetChunkMesh(0), Is.SameAs(secondMesh));
                foreach (Slot slot in slots)
                {
                    Assert.That(slot.Foot.ActiveMarkerCount, Is.EqualTo(auxiliaryCount));
                    Assert.That(slot.Health.ActiveBarCount, Is.EqualTo(auxiliaryCount));
                    Assert.That(slot.Backend.SegmentCount, Is.EqualTo(expectedSegments));
                    Assert.That(slot.Backend.ActiveChunkCount, Is.EqualTo(expectedChunks));
                    Assert.That(slot.Backend.Diagnostics.UnresolvedCommandCount, Is.Zero);
                    Assert.That(slot.LastCapture, Is.Not.SameAs(source));
                    Assert.That(slot.LastCapture.CommandCount, Is.EqualTo(commandCount));
                    Assert.That(slot.LastCapture.GetCommand(0).Handle, Is.EqualTo(source.GetCommand(0).Handle));
                    for (int index = 0; index < expectedSegments; index++)
                    {
                        BattleCentralRenderSegment segment = slot.Backend.GetSegment(index);
                        Assert.That(segment.Texture, Is.Not.Null);
                        Assert.That(segment.Material, Is.SameAs(material));
                        Assert.That(segment.BindingMode, Is.EqualTo(BattleSpriteCentralBindingMode.SourceTexture2D));
                    }
                }
                Assert.That(slots[0].LastCapture, Is.Not.SameAs(slots[1].LastCapture));
                result.status = "PASS";
            }
            catch (Exception exception)
            {
                result.error = exception.ToString();
                throw;
            }
            finally
            {
                allocationRecorder?.Dispose();
                RenderTexture.active = previousTarget;
                if (slots != null)
                    foreach (Slot slot in slots)
                        slot.Dispose();
                if (material != null)
                    UnityEngine.Object.DestroyImmediate(material);
                if (auxiliarySprite != null)
                    UnityEngine.Object.DestroyImmediate(auxiliarySprite);
                if (target != null)
                {
                    target.Release();
                    UnityEngine.Object.DestroyImmediate(target);
                }
                Save(result);
            }
        }

        [Test]
        public void PreparedSlot_OverflowAndHeldLeaseRejectWholeCapture()
        {
            using (var slot = new Slot(2))
            {
                BattlePresentationFrame source = MakePublication(2);
                BattlePresentationFrame good = slot.Capture(source);
                Assert.Throws<InvalidOperationException>(() => slot.Capture(MakePublication(3)));
                Assert.That(good.CommandCount, Is.EqualTo(2));
                // No GPU or catalog resource is needed for this CPU slot contract.
                slot.Backend.Build(good, new RealTextureResolver(null, null, null, false, 2));
                slot.Foot.BuildFromFrame(good, null, BattleFootMarkerStyle.Default, false);
                slot.Health.BuildFromFrame(good, BattleHealthBarStyle.Default, false);
                slot.Publish(good, 1);
                Assert.That(slot.Acquire(out var lease), Is.True);
                try
                {
                    slot.Retire();
                    Assert.That(slot.Submission.ReadLeaseCount, Is.EqualTo(1));
                    Assert.Throws<InvalidOperationException>(() => slot.Capture(source));
                    Assert.That(slot.Submission.Generation, Is.EqualTo(1));
                }
                finally
                {
                    lease.Dispose();
                }
                Assert.That(slot.Submission.ReadLeaseCount, Is.Zero);
                Assert.That(slot.Capture(source).CommandCount, Is.EqualTo(2));
            }
        }

        private static int Execute(Slot slot, BattlePresentationFrame source, BattlePresentationDisplayMotion motion,
            RealTextureResolver resolver, BattleCentralDrawMode mode, RenderTexture target,
            MaterialPropertyBlock block, int sampleId, out long captureTicks, out long buildTicks,
            out long recordTicks, out long executeTicks, Sprite auxiliarySprite = null, Material auxiliaryMaterial = null)
        {
            long started = Stopwatch.GetTimestamp();
            BattlePresentationFrame captured = slot.Capture(source);
            motion.Prepare(captured, (sampleId % 5) * 0.25, 1.5, 2.0);
            motion.ApplyToCapturedCommands(captured);
            captureTicks = Stopwatch.GetTimestamp() - started;
            started = Stopwatch.GetTimestamp();
            slot.Backend.Build(captured, resolver, mode);
            slot.Foot.BuildFromFrame(captured, auxiliarySprite, BattleFootMarkerStyle.Default, auxiliarySprite != null);
            slot.Health.BuildFromFrame(captured, BattleHealthBarStyle.Default, auxiliarySprite != null);
            buildTicks = Stopwatch.GetTimestamp() - started;
            slot.LastCapture = captured;
            slot.Publish(captured, sampleId + 1);
            if (!slot.Acquire(out var lease))
                throw new InvalidOperationException("Prepared submission lease acquisition failed.");
            CommandBuffer buffer = null;
            try
            {
                started = Stopwatch.GetTimestamp();
                buffer = CommandBufferPool.Get("NTSD Batch19 Real Texture CPU Bridge");
                buffer.SetRenderTarget(target);
                buffer.ClearRenderTarget(false, true, Color.clear);
                buffer.SetViewProjectionMatrices(Matrix4x4.identity, Projection);
                int draws = 0;
                if (auxiliarySprite != null)
                {
                    block.Clear();
                    block.SetTexture(MainTexId, slot.Foot.Texture);
                    buffer.DrawMesh(slot.Foot.Mesh, Matrix4x4.identity, auxiliaryMaterial, 0, 0, block);
                    draws++;
                }
                for (int index = 0; index < slot.Backend.SegmentCount; index++)
                {
                    BattleCentralRenderSegment segment = slot.Backend.GetSegment(index);
                    if (segment.Material == null || segment.Texture == null)
                        throw new InvalidOperationException("Sample contains an invalid drawable binding.");
                    block.Clear();
                    block.SetTexture(MainTexId, segment.Texture);
                    buffer.DrawMesh(slot.Backend.GetChunkMesh(segment.ChunkIndex), Matrix4x4.identity,
                        segment.Material, segment.SubMeshIndex, 0, block);
                    draws++;
                }
                if (auxiliarySprite != null)
                {
                    block.Clear();
                    block.SetTexture(MainTexId, Texture2D.whiteTexture);
                    buffer.DrawMesh(slot.Health.Mesh, Matrix4x4.identity, auxiliaryMaterial, 0, 0, block);
                    draws++;
                }
                recordTicks = Stopwatch.GetTimestamp() - started;
                started = Stopwatch.GetTimestamp();
                Graphics.ExecuteCommandBuffer(buffer);
                executeTicks = Stopwatch.GetTimestamp() - started;
                if (!slot.Record(sampleId + 1, captured.TickIndex, draws))
                    throw new InvalidOperationException("Submission generation did not accept executed CPU draw count.");
                return draws;
            }
            finally
            {
                if (buffer != null)
                    CommandBufferPool.Release(buffer);
                lease.Dispose();
                slot.Retire();
            }
        }

        private static BattlePresentationFrame MakePublication(int count, int auxiliaryCount = 0)
        {
            var prior = new BattlePresentationFrame { TickIndex = 100 };
            var frame = new BattlePresentationFrame { TickIndex = 101 };
            for (int index = 0; index < count; index++)
            {
                prior.AddMotionState(Motion(index, 100));
                frame.AddMotionState(Motion(index, 104));
                frame.AddCommand(new BattleRenderCommand(BattleRenderCommandType.Entity,
                    new RuntimeEntityHandle(index + 1, 1), index + 1, 2, 0, 0, index + 1, 1, index, index,
                    new Vector3(-1f + (index % 32) * 0.06f, -1f + ((index / 32) % 32) * 0.06f, index * 0.000001f),
                    new Vector2(64, 64), new Vector2(0.5f, 0.5f), new Rect(0f, 0f, 1f, 1f),
                    new BattleSpriteRenderState(new Color32(255, 220, 200, 200), (index & 1) != 0, false,
                        SpriteMaskInteraction.None, BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha),
                    default, showOverheadHealthBar: index < auxiliaryCount,
                    currentHealth: 40, recoverableHealth: 80, maximumHealth: 100,
                    stableHealthAnchorWorld: new Vector2(0, 1), hasStableHealthAnchor: true,
                    stableFootAnchorWorld: Vector2.zero, hasStableFootAnchor: true,
                    showSelfFootMarker: index < auxiliaryCount, motionAnchor: BattlePresentationMotionAnchor.Body));
            }
            frame.CopyPreviousMotionStatesFrom(prior);
            return frame;
        }

        private static BattlePresentationMotionState Motion(int index, double x)
        {
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.SetSourceRulePosition(x, 100);
            runtime.X = x * 1.5;
            runtime.Z = 200;
            return new BattlePresentationMotionState(new RuntimeEntityHandle(index + 1, 1), 2, runtime);
        }

        private static int ExpectedSegments(int count, BattleCentralDrawMode mode, bool alternating)
        {
            if (mode == BattleCentralDrawMode.StrictOrderedDraw || alternating)
                return count;
            // Two contiguous texture identities, plus any chunk boundary inside either run.
            int middle = count / 2;
            return (middle + 4095) / 4096 + ((count - 1) / 4096 - middle / 4096 + 1);
        }

        private static string Identity(Texture2D texture)
        {
            return texture.GetInstanceID() + ":" + texture.width + "x" + texture.height + ":" + texture.format;
        }

        private static string Sha256(string path)
        {
            using (SHA256 algorithm = SHA256.Create())
            using (FileStream stream = File.OpenRead(path))
                return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "");
        }

        private static double MeanMs(long ticks)
        {
            return ticks * 1000.0 / Stopwatch.Frequency / SampleCount;
        }

        private static double RankMs(long[] ticks, double rank)
        {
            return ticks[(int)Math.Ceiling(rank * ticks.Length) - 1] * 1000.0 / Stopwatch.Frequency;
        }

        private static void Save(Result result)
        {
            string run = Guid.NewGuid().ToString("N");
            string path = Path.Combine(result.outputRoot, "run-" + run);
            Directory.CreateDirectory(path);
            using (var stream = new FileStream(Path.Combine(path, "result.json"), FileMode.CreateNew, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
                writer.Write(JsonUtility.ToJson(result, true));
            TestContext.Out.WriteLine("NTSD_BATCH19_RESULT " + path + "/result.json status=" + result.status);
        }

        private sealed class RealTextureResolver : IBattleCentralResourceResolver
        {
            private readonly Texture first;
            private readonly Texture second;
            private readonly Material material;
            private readonly bool alternating;
            private readonly int middle;

            public RealTextureResolver(Texture first, Texture second, Material material, bool alternating, int count)
            {
                this.first = first;
                this.second = second;
                this.material = material;
                this.alternating = alternating;
                middle = count / 2;
            }

            public BattleCentralResourceStatus Resolve(in BattleRenderCommand command, out BattleCentralResolvedResource resource)
            {
                Texture texture = (alternating ? (command.LocalSequence & 1) != 0 : command.LocalSequence >= middle)
                    ? second : first;
                resource = new BattleCentralResolvedResource(texture, material, command.NormalizedUv,
                    command.Size, command.Pivot, new Color32(255, 220, 200, 200));
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private sealed class Slot : IDisposable
        {
            public readonly BattleDynamicMeshBackend Backend = new BattleDynamicMeshBackend();
            public readonly BattleFootMarkerBatchBackend Foot = new BattleFootMarkerBatchBackend();
            public readonly BattleHealthBarBatchBackend Health = new BattleHealthBarBatchBackend();
            public readonly BattleCentralSubmission Submission;
            public BattlePresentationFrame LastCapture;
            private readonly Func<BattleCentralSubmission, BattlePresentationFrame, BattleTickDetailPhaseDiagnostics,
                BattlePresentationFrame> capture;
            private readonly Action<BattleCentralSubmission, SimulationWorld, BattlePresentationFrame, int, int,
                CharacterAnimtorManager, BattleSpriteCatalog> publish;
            private readonly AcquireLease acquire;
            private readonly Action<BattleCentralSubmission> retire;
            private readonly Func<BattleCentralSubmission, int, int, int, bool> record;

            public Slot(int count)
            {
                ConstructorInfo constructor = typeof(BattleCentralSubmission).GetConstructor(PrivateInstance, null,
                    new[] { typeof(BattleDynamicMeshBackend), typeof(BattleFootMarkerBatchBackend), typeof(BattleHealthBarBatchBackend) }, null);
                Submission = (BattleCentralSubmission)constructor.Invoke(new object[] { Backend, Foot, Health });
                typeof(BattleCentralSubmission).GetMethod("PrepareCapacity", PrivateInstance)
                    .Invoke(Submission, new object[] { count + 1, 0, count });
                Backend.PrepareCapacity(count);
                typeof(BattleDynamicMeshBackend).GetMethod("SealCapacity", PrivateInstance)
                    .Invoke(Backend, new object[] { count });
                typeof(BattleCentralSubmission).GetMethod("SealCapacity", PrivateInstance).Invoke(Submission, null);
                capture = Bind<Func<BattleCentralSubmission, BattlePresentationFrame, BattleTickDetailPhaseDiagnostics,
                    BattlePresentationFrame>>("CaptureFrame");
                publish = Bind<Action<BattleCentralSubmission, SimulationWorld, BattlePresentationFrame, int, int,
                    CharacterAnimtorManager, BattleSpriteCatalog>>("Publish");
                acquire = Bind<AcquireLease>("TryAcquire");
                retire = Bind<Action<BattleCentralSubmission>>("Retire");
                record = Bind<Func<BattleCentralSubmission, int, int, int, bool>>("TryRecordExecutedDraws");
            }

            public BattlePresentationFrame Capture(BattlePresentationFrame source) => capture(Submission, source, null);
            public void Publish(BattlePresentationFrame frame, int generation) =>
                publish(Submission, null, frame, frame.TickIndex, generation, null, null);
            public bool Acquire(out BattleCentralSubmission.BattleCentralSubmissionLease lease) => acquire(Submission, out lease);
            public bool Record(int generation, int tick, int draws) => record(Submission, generation, tick, draws);
            public void Retire() => retire(Submission);

            public void Dispose()
            {
                Retire();
                Backend.Dispose();
                Foot.Dispose();
                Health.Dispose();
            }

            private static T Bind<T>(string method) where T : Delegate
            {
                return (T)Delegate.CreateDelegate(typeof(T),
                    typeof(BattleCentralSubmission).GetMethod(method, PrivateInstance));
            }
        }
    }
}
#endif
