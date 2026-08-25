using System.Diagnostics;
using NTSD.App;
using NTSD.Simulation;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] private Material material;
        [SerializeField] private Material arrayMaterial;
        [SerializeField] private BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks;

        private BattleRenderPass pass;
        private BattleBottomOverlayPass bottomOverlayPass;

        public Material Material => material;
        public Material ArrayMaterial => arrayMaterial;
        public BattleCentralDrawMode DrawMode => drawMode;
        public RenderPassEvent InjectionPoint => RenderPassEvent.AfterRenderingTransparents;

        public void Configure(Material value, BattleCentralDrawMode mode)
        {
            material = value;
            drawMode = mode;
            Create();
        }

        public void Configure(Material fallbackValue, Material arrayValue, BattleCentralDrawMode mode)
        {
            material = fallbackValue;
            arrayMaterial = arrayValue;
            drawMode = mode;
            Create();
        }

        public override void Create()
        {
            pass ??= new BattleRenderPass();
            pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
            bottomOverlayPass ??= new BattleBottomOverlayPass();
            bottomOverlayPass.renderPassEvent =
                (RenderPassEvent)((int)RenderPassEvent.AfterRenderingTransparents + 1);
            BattleCentralRenderSystem.RegisterFeature(this, material, arrayMaterial, drawMode);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            BattleCentralRenderSystem.RecordFeatureCameraAvailability(
                this,
                renderer,
                renderingData.cameraData.camera,
                renderingData.cameraData.renderType);
            BattleCentralRenderSystem.MaterializeLatestPublishedFrameForCamera(
                this,
                renderingData.cameraData.camera,
                renderingData.cameraData.renderType);
            if (renderer == null)
            {
                return;
            }

            if (BattleCentralRenderSystem.TryAcquireSubmission(
                    renderingData.cameraData.camera,
                    renderingData.cameraData.renderType,
                    out BattleCentralSubmission.BattleCentralSubmissionLease submissionLease))
            {
                pass.Setup(submissionLease);
                renderer.EnqueuePass(pass);
            }

            if (BattleBackgroundBottomOverlayPresenter.TryGetDraw(
                    renderingData.cameraData.camera,
                    out Mesh overlayMesh,
                    out Material overlayMaterial,
                    out MaterialPropertyBlock overlayProperties))
            {
                bottomOverlayPass.Setup(
                    overlayMesh,
                    overlayMaterial,
                    overlayProperties);
                renderer.EnqueuePass(bottomOverlayPass);
            }
        }

        protected override void Dispose(bool disposing)
        {
            BattleCentralRenderSystem.UnregisterFeature(this);
            pass?.Dispose();
            pass = null;
            bottomOverlayPass?.Dispose();
            bottomOverlayPass = null;
        }

        private sealed class BattleBottomOverlayPass : ScriptableRenderPass
        {
            private Mesh mesh;
            private Material material;
            private MaterialPropertyBlock properties;

            public void Setup(
                Mesh valueMesh,
                Material valueMaterial,
                MaterialPropertyBlock valueProperties)
            {
                mesh = valueMesh;
                material = valueMaterial;
                properties = valueProperties;
            }

            public override void Execute(
                ScriptableRenderContext context,
                ref RenderingData renderingData)
            {
                if (mesh == null || material == null)
                    return;

                CommandBuffer commandBuffer = CommandBufferPool.Get(
                    "NTSD Battle Bottom Overlay");
                try
                {
                    commandBuffer.DrawMesh(
                        mesh,
                        Matrix4x4.identity,
                        material,
                        0,
                        0,
                        properties);
                    context.ExecuteCommandBuffer(commandBuffer);
                }
                finally
                {
                    CommandBufferPool.Release(commandBuffer);
                    mesh = null;
                    material = null;
                    properties = null;
                }
            }

            public void Dispose()
            {
                mesh = null;
                material = null;
                properties = null;
            }
        }

        private sealed class BattleRenderPass : ScriptableRenderPass
        {
            private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
            private static readonly int MainTexArrayId = Shader.PropertyToID("_MainTexArray");
            private static readonly ProfilerMarker ExecuteCommandBufferMarker =
                new ProfilerMarker("NTSD.BattlePresentation.ExecuteCommandBuffer");
            private readonly MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            private BattleCentralSubmission.BattleCentralSubmissionLease submissionLease;

            public void Setup(BattleCentralSubmission.BattleCentralSubmissionLease value)
            {
                submissionLease.Dispose();
                submissionLease = value;
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                BattleCentralSubmission.BattleCentralSubmissionLease lease = submissionLease;
                submissionLease = default;
                try
                {
                    BattleDynamicMeshBackend backend = lease.Backend;
                    if (backend == null || !BattleCentralRenderSystem.IsSubmissionLeaseCurrent(lease))
                        return;

                    CommandBuffer commandBuffer = CommandBufferPool.Get("NTSD Central Battle Rendering");
                    int drawCount = 0;
                    try
                    {
                        for (int index = 0; index < backend.SegmentCount; index++)
                        {
                            BattleCentralRenderSegment segment = backend.GetSegment(index);
                            if (segment.Material == null || segment.Texture == null)
                                continue;
                            propertyBlock.Clear();
                            if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray)
                                propertyBlock.SetTexture(MainTexArrayId, segment.Texture);
                            else
                                propertyBlock.SetTexture(MainTexId, segment.Texture);
                            commandBuffer.DrawMesh(
                                backend.GetChunkMesh(segment.ChunkIndex),
                                Matrix4x4.identity,
                                segment.Material,
                                segment.SubMeshIndex,
                                0,
                                propertyBlock);
                            drawCount++;
                        }
                        BattleTickDetailPhaseDiagnostics detailDiagnostics =
                            lease.Submission?.World?
                                .ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                        long executeStarted = detailDiagnostics != null
                            ? Stopwatch.GetTimestamp()
                            : 0;
                        try
                        {
                            using (ExecuteCommandBufferMarker.Auto())
                            {
                                context.ExecuteCommandBuffer(commandBuffer);
                            }
                        }
                        finally
                        {
                            if (detailDiagnostics != null)
                            {
                                detailDiagnostics.RecordDeferredPhaseElapsed(
                                    BattleTickDetailPhase.RenderExecuteCommandBuffer,
                                    Stopwatch.GetTimestamp() - executeStarted);
                            }
                        }
                    }
                    finally
                    {
                        CommandBufferPool.Release(commandBuffer);
                    }
                    BattleCentralRenderSystem.RecordSubmission(lease, drawCount);
                }
                finally
                {
                    lease.Dispose();
                }
            }

            public void Dispose()
            {
                submissionLease.Dispose();
                submissionLease = default;
            }
        }
    }
}
