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
            BattleCentralRenderSystem.RegisterFeature(this, material, arrayMaterial, drawMode);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            BattleCentralRenderSystem.RecordFeatureCameraAvailability(
                this,
                renderer,
                renderingData.cameraData.camera,
                renderingData.cameraData.renderType);
            if (renderer == null ||
                !BattleCentralRenderSystem.TryAcquireSubmission(
                    renderingData.cameraData.camera,
                    renderingData.cameraData.renderType,
                    out BattleCentralSubmission.BattleCentralSubmissionLease submissionLease))
            {
                return;
            }

            pass.Setup(submissionLease);
            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            BattleCentralRenderSystem.UnregisterFeature(this);
            pass?.Dispose();
            pass = null;
        }

        private sealed class BattleRenderPass : ScriptableRenderPass
        {
            private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
            private static readonly int MainTexArrayId = Shader.PropertyToID("_MainTexArray");
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
                        context.ExecuteCommandBuffer(commandBuffer);
                    }
                    finally
                    {
                        CommandBufferPool.Release(commandBuffer);
                    }
                    BattleCentralRenderSystem.RecordSubmission(drawCount);
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
