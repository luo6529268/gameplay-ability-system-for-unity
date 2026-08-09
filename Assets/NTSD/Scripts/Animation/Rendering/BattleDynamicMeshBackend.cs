using System;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleDynamicMeshBackend : IDisposable
    {
        public const int QuadsPerChunk = 4096;
        public const int VerticesPerQuad = 4;
        public const int IndicesPerQuad = 6;
        public const int VerticesPerChunk = QuadsPerChunk * VerticesPerQuad;
        public const int IndicesPerChunk = QuadsPerChunk * IndicesPerQuad;
        public const int MaxUInt16VertexIndex = VerticesPerChunk - 1;

        private readonly BattleCentralBuildDiagnostics diagnostics = new BattleCentralBuildDiagnostics();
        private BattleMeshChunk[] chunks = new BattleMeshChunk[1];
        private BattleCentralRenderSegment[] segments = new BattleCentralRenderSegment[16];
        private SegmentBoundsAccumulator[] segmentBounds =
            new SegmentBoundsAccumulator[16];
        private int activeChunkCount;
        private int dirtyChunkCount;
        private int segmentCount;
        private int mutationVersion;
        private bool disposed;
        private BattlePresentationFrame builtFrame;
#if UNITY_EDITOR
        private int lastInactiveChunkClearCount;
#endif

        public BattleCentralBuildDiagnostics Diagnostics => diagnostics;
        public int ActiveChunkCount => activeChunkCount;
        public int SegmentCount => segmentCount;
        public int AllocatedChunkCount => chunks.Length;
        internal int MutationVersion => mutationVersion;
        internal BattlePresentationFrame BuiltFrame => builtFrame;

        public Mesh GetChunkMesh(int index)
        {
            if ((uint)index >= (uint)activeChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return chunks[index].Mesh;
        }

        public int GetChunkActiveQuadCount(int index)
        {
            if ((uint)index >= (uint)activeChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return chunks[index].ActiveQuadCount;
        }

        internal ushort GetChunkIndexTemplateValue(int chunkIndex, int index)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetIndexTemplateValue(index);
        }

        internal float GetChunkVertexAtlasSlice(int chunkIndex, int vertexIndex)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetVertexAtlasSlice(vertexIndex);
        }

        internal Color32 GetChunkVertexColor(int chunkIndex, int vertexIndex)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetVertexColor(vertexIndex);
        }

        internal Vector2 GetChunkVertexUv(int chunkIndex, int vertexIndex)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetVertexUv(vertexIndex);
        }

        public BattleCentralRenderSegment GetSegment(int index)
        {
            if ((uint)index >= (uint)segmentCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return segments[index];
        }

        public void Build(
            BattlePresentationFrame frame,
            IBattleCentralResourceResolver resolver,
            BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks,
            BattleTickDetailPhaseDiagnostics detailDiagnostics = null)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleDynamicMeshBackend));
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            mutationVersion++;
            builtFrame = frame;
#if UNITY_EDITOR
            lastInactiveChunkClearCount = 0;
#endif

            int commandCount = frame?.CommandCount ?? 0;
            diagnostics.Reset(frame?.TickIndex ?? 0, commandCount, drawMode);
            segmentCount = 0;
            int resolvedCount = 0;
            bool hasOpenSegment = false;
            int openSegmentIndex = -1;
            int openChunkIndex = -1;
            int openSubMeshIndex = -1;
            int openFirstCommandIndex = -1;
            int openFirstQuad = -1;
            int openQuadCount = 0;
            BattleCentralResolvedResource openSegmentResource = default;

            for (int commandIndex = 0; commandIndex < commandCount; commandIndex++)
            {
                ref readonly BattleRenderCommand command =
                    ref frame.GetCommandRef(commandIndex);
                BattleCentralResolvedResource resource;
                BattleCentralResourceStatus status;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
                try
                {
                    status = resolver.Resolve(command, out resource);
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
                }
                if (status != BattleCentralResourceStatus.Resolved)
                {
                    if (status == BattleCentralResourceStatus.UnsupportedCategory)
                        diagnostics.UnsupportedCategoryCount++;
                    else if (status == BattleCentralResourceStatus.UnsupportedRenderState)
                        diagnostics.UnsupportedRenderStateCount++;
                    else
                        diagnostics.UnresolvedCommandCount++;
                    if (diagnostics.FirstUnresolvedCommandIndex < 0)
                    {
                        diagnostics.FirstUnresolvedCommandIndex = commandIndex;
                        diagnostics.FirstUnresolvedCommandType = command.Type;
                        diagnostics.FirstUnresolvedStatus = status;
                    }
                    // An unresolved command still occupies an authoritative position
                    // in the P3 stream. Never batch resolved commands across it.
                    if (hasOpenSegment)
                    {
                        CommitSegment(
                            openSegmentIndex,
                            openChunkIndex,
                            openSubMeshIndex,
                            openFirstCommandIndex,
                            openFirstQuad,
                            openQuadCount,
                            openSegmentResource);
                        hasOpenSegment = false;
                    }
                    continue;
                }

                int chunkIndex = resolvedCount / QuadsPerChunk;
                int quadIndex = resolvedCount % QuadsPerChunk;
                EnsureChunk(chunkIndex);
                BattleMeshChunk chunk = chunks[chunkIndex];
                bool strict = drawMode == BattleCentralDrawMode.StrictOrderedDraw;
                bool canAppend = !strict && hasOpenSegment && openChunkIndex == chunkIndex &&
                                 IsCompatible(openSegmentResource, resource) &&
                                 openFirstQuad + openQuadCount == quadIndex;
                if (!canAppend)
                {
                    if (hasOpenSegment)
                    {
                        CommitSegment(
                            openSegmentIndex,
                            openChunkIndex,
                            openSubMeshIndex,
                            openFirstCommandIndex,
                            openFirstQuad,
                            openQuadCount,
                            openSegmentResource);
                    }
                    EnsureSegmentCapacity(segmentCount + 1);
                    openSegmentIndex = segmentCount++;
                    openChunkIndex = chunkIndex;
                    openSubMeshIndex = chunk.PendingSegmentCount++;
                    openFirstCommandIndex = commandIndex;
                    openFirstQuad = quadIndex;
                    openQuadCount = 0;
                    openSegmentResource = resource;
                    hasOpenSegment = true;
                }
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameWriteQuads);
                try
                {
                    chunk.WriteQuad(
                        quadIndex,
                        command,
                        resource,
                        out SegmentBoundsAccumulator quadBounds);
                    if (canAppend)
                        segmentBounds[openSegmentIndex].Encapsulate(quadBounds);
                    else
                        segmentBounds[openSegmentIndex] = quadBounds;
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameWriteQuads);
                }

                openQuadCount++;
                resolvedCount++;
            }

            if (hasOpenSegment)
            {
                CommitSegment(
                    openSegmentIndex,
                    openChunkIndex,
                    openSubMeshIndex,
                    openFirstCommandIndex,
                    openFirstQuad,
                    openQuadCount,
                    openSegmentResource);
            }

            activeChunkCount = resolvedCount == 0 ? 0 : (resolvedCount + QuadsPerChunk - 1) / QuadsPerChunk;
            int segmentCursor = 0;
            for (int chunkIndex = 0; chunkIndex < activeChunkCount; chunkIndex++)
            {
                BattleMeshChunk chunk = chunks[chunkIndex];
                int activeQuads = Math.Min(QuadsPerChunk, resolvedCount - chunkIndex * QuadsPerChunk);
                chunk.Upload(
                    chunkIndex,
                    activeQuads,
                    segments,
                    segmentBounds,
                    ref segmentCursor,
                    segmentCount,
                    detailDiagnostics);
            }
            for (int chunkIndex = activeChunkCount; chunkIndex < dirtyChunkCount; chunkIndex++)
            {
                chunks[chunkIndex]?.ClearActive();
#if UNITY_EDITOR
                lastInactiveChunkClearCount++;
#endif
            }
            dirtyChunkCount = activeChunkCount;

            diagnostics.ResolvedCommandCount = resolvedCount;
            diagnostics.ActiveChunkCount = activeChunkCount;
            diagnostics.SegmentCount = segmentCount;
        }

        public void Clear()
        {
            mutationVersion++;
            builtFrame = null;
            segmentCount = 0;
            activeChunkCount = 0;
            builtFrame = null;
            for (int i = 0; i < dirtyChunkCount; i++)
                chunks[i]?.ClearActive();
            dirtyChunkCount = 0;
            diagnostics.Reset(0, 0, BattleCentralDrawMode.OrderedChunks);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            for (int i = 0; i < chunks.Length; i++)
                chunks[i]?.Dispose();
            chunks = Array.Empty<BattleMeshChunk>();
            segments = Array.Empty<BattleCentralRenderSegment>();
            segmentBounds = Array.Empty<SegmentBoundsAccumulator>();
            activeChunkCount = 0;
            dirtyChunkCount = 0;
            segmentCount = 0;
        }

        private void EnsureChunk(int chunkIndex)
        {
            if (chunkIndex >= chunks.Length)
            {
                int next = chunks.Length;
                while (next <= chunkIndex)
                    next = checked(next * 2);
                Array.Resize(ref chunks, next);
                diagnostics.CapacityGrowthCount++;
            }
            if (chunks[chunkIndex] == null)
            {
                chunks[chunkIndex] = new BattleMeshChunk(chunkIndex);
                diagnostics.CapacityGrowthCount++;
            }
            if (dirtyChunkCount <= chunkIndex)
                dirtyChunkCount = chunkIndex + 1;
        }

        private void EnsureSegmentCapacity(int required)
        {
            if (required <= segments.Length)
                return;
            int next = segments.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref segments, next);
            Array.Resize(ref segmentBounds, next);
            diagnostics.CapacityGrowthCount++;
        }

        private void CommitSegment(
            int segmentIndex,
            int chunkIndex,
            int subMeshIndex,
            int firstCommandIndex,
            int firstQuad,
            int quadCount,
            in BattleCentralResolvedResource resource)
        {
            segments[segmentIndex] = new BattleCentralRenderSegment(
                chunkIndex,
                subMeshIndex,
                firstCommandIndex,
                quadCount,
                firstQuad,
                quadCount,
                resource.Texture,
                resource.Material,
                resource.MaterialVariant,
                resource.AtlasSlice,
                resource.BindingMode,
                resource.AtlasPageIndex);
        }

        private static bool IsCompatible(
            in BattleCentralResolvedResource current,
            in BattleCentralResolvedResource next)
        {
            return current.Texture == next.Texture &&
                   current.Material == next.Material &&
                   current.MaterialVariant == next.MaterialVariant &&
                   current.BindingMode == next.BindingMode &&
                   (next.BindingMode != BattleSpriteCentralBindingMode.AtlasPageTexture2D ||
                    current.AtlasPageIndex == next.AtlasPageIndex) &&
                   (next.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ||
                    current.AtlasSlice == next.AtlasSlice);
        }

        private struct BattleQuadVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public float AtlasSlice;
        }

        private struct SegmentBoundsAccumulator
        {
            private bool hasValue;
            private float minX;
            private float minY;
            private float minZ;
            private float maxX;
            private float maxY;
            private float maxZ;

            public void Set(
                float valueMinX,
                float valueMinY,
                float valueMinZ,
                float valueMaxX,
                float valueMaxY,
                float valueMaxZ)
            {
                minX = valueMinX;
                minY = valueMinY;
                minZ = valueMinZ;
                maxX = valueMaxX;
                maxY = valueMaxY;
                maxZ = valueMaxZ;
                hasValue = true;
            }

            public void Encapsulate(in SegmentBoundsAccumulator other)
            {
                if (!other.hasValue)
                    return;
                if (!hasValue)
                {
                    this = other;
                    return;
                }

                minX = Mathf.Min(minX, other.minX);
                minY = Mathf.Min(minY, other.minY);
                minZ = Mathf.Min(minZ, other.minZ);
                maxX = Mathf.Max(maxX, other.maxX);
                maxY = Mathf.Max(maxY, other.maxY);
                maxZ = Mathf.Max(maxZ, other.maxZ);
            }

            public Bounds ToBounds()
            {
                if (!hasValue)
                    return new Bounds(Vector3.zero, Vector3.zero);
                var bounds = new Bounds();
                bounds.SetMinMax(
                    new Vector3(minX, minY, minZ),
                    new Vector3(maxX, maxY, maxZ));
                return bounds;
            }
        }

        private sealed class BattleMeshChunk : IDisposable
        {
            private static readonly VertexAttributeDescriptor[] VertexLayout =
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1),
            };

            private readonly BattleQuadVertex[] vertices = new BattleQuadVertex[VerticesPerChunk];
            private readonly ushort[] indexTemplate = new ushort[IndicesPerChunk];
            private readonly int chunkIndex;
            private Mesh mesh;
            private int activeSubMeshCount;
            private bool hasBounds;
            private float boundsMinX;
            private float boundsMinY;
            private float boundsMinZ;
            private float boundsMaxX;
            private float boundsMaxY;
            private float boundsMaxZ;

            public BattleMeshChunk(int index)
            {
                chunkIndex = index;
                for (int quad = 0; quad < QuadsPerChunk; quad++)
                {
                    int vertex = quad * VerticesPerQuad;
                    int indexOffset = quad * IndicesPerQuad;
                    indexTemplate[indexOffset] = (ushort)vertex;
                    indexTemplate[indexOffset + 1] = (ushort)(vertex + 1);
                    indexTemplate[indexOffset + 2] = (ushort)(vertex + 2);
                    indexTemplate[indexOffset + 3] = (ushort)(vertex + 2);
                    indexTemplate[indexOffset + 4] = (ushort)(vertex + 1);
                    indexTemplate[indexOffset + 5] = (ushort)(vertex + 3);
                }
                mesh = CreateMesh();
                ClearActive();
            }

            public Mesh Mesh => EnsureMesh();
            public int ActiveQuadCount { get; private set; }
            public int PendingSegmentCount { get; set; }

            public ushort GetIndexTemplateValue(int index)
            {
                if ((uint)index >= (uint)indexTemplate.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return indexTemplate[index];
            }

            public float GetVertexAtlasSlice(int index)
            {
                if ((uint)index >= (uint)vertices.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return vertices[index].AtlasSlice;
            }

            public Color32 GetVertexColor(int index)
            {
                if ((uint)index >= (uint)vertices.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return vertices[index].Color;
            }

            public Vector2 GetVertexUv(int index)
            {
                if ((uint)index >= (uint)vertices.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return vertices[index].Uv;
            }

            public void WriteQuad(
                int quadIndex,
                in BattleRenderCommand command,
                in BattleCentralResolvedResource resource,
                out SegmentBoundsAccumulator quadBounds)
            {
                if ((uint)quadIndex >= QuadsPerChunk)
                    throw new ArgumentOutOfRangeException(nameof(quadIndex));

                Vector2 pixelSize = resource.PixelSize.sqrMagnitude > 0f ? resource.PixelSize : command.Size;
                Vector2 pivot = resource.Pivot;
                float width = pixelSize.x * NTSDRenderSpace.UnitsPerPixelX * NTSDRenderSpace.BattleVisualScale;
                float height = pixelSize.y * NTSDRenderSpace.UnitsPerPixelY * NTSDRenderSpace.BattleVisualScale;
                float left = command.Position.x - pivot.x * width;
                float right = left + width;
                float bottom = command.Position.y - pivot.y * height;
                float top = bottom + height;
                float z = command.Position.z;

                Rect uv = resource.NormalizedUv;
                float u0 = command.FlipX ? uv.xMax : uv.xMin;
                float u1 = command.FlipX ? uv.xMin : uv.xMax;
                float v0 = command.FlipY ? uv.yMax : uv.yMin;
                float v1 = command.FlipY ? uv.yMin : uv.yMax;
                int vertex = quadIndex * VerticesPerQuad;
                vertices[vertex] = CreateVertex(left, bottom, z, u0, v0, resource);
                vertices[vertex + 1] = CreateVertex(left, top, z, u0, v1, resource);
                vertices[vertex + 2] = CreateVertex(right, bottom, z, u1, v0, resource);
                vertices[vertex + 3] = CreateVertex(right, top, z, u1, v1, resource);

                float minX = Mathf.Min(left, right);
                float minY = Mathf.Min(bottom, top);
                float maxX = Mathf.Max(left, right);
                float maxY = Mathf.Max(bottom, top);
                quadBounds = default;
                quadBounds.Set(minX, minY, z, maxX, maxY, z);
                Encapsulate(minX, minY, z, maxX, maxY, z);
            }

            public void Upload(
                int chunkIndex,
                int activeQuads,
                BattleCentralRenderSegment[] allSegments,
                SegmentBoundsAccumulator[] allSegmentBounds,
                ref int segmentCursor,
                int totalSegments,
                BattleTickDetailPhaseDiagnostics detailDiagnostics)
            {
                Mesh targetMesh = EnsureMesh();
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                try
                {
                    int previousActiveSubMeshCount = activeSubMeshCount;
                    int desiredSubMeshCount = PendingSegmentCount;
                    int physicalSubMeshCount = targetMesh.subMeshCount;
                    if (desiredSubMeshCount > physicalSubMeshCount)
                    {
                        targetMesh.subMeshCount = desiredSubMeshCount;
                        // Unity does not guarantee safe default descriptors after native
                        // submesh growth, so reinitialize the complete physical range.
                        for (int subMeshIndex = 0; subMeshIndex < targetMesh.subMeshCount; subMeshIndex++)
                            SetInertSubmesh(targetMesh, subMeshIndex);
                    }
                    else
                    {
                        // Reset every descriptor that was active in the previous upload before
                        // rewriting this frame's active range. Keep the physical high-water;
                        // shrinking subMeshCount here forces Unity to rebuild native state.
                        int inertEnd = Math.Min(previousActiveSubMeshCount, physicalSubMeshCount);
                        for (int subMeshIndex = 0; subMeshIndex < inertEnd; subMeshIndex++)
                            SetInertSubmesh(targetMesh, subMeshIndex);
                    }
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                }

                ActiveQuadCount = activeQuads;
                int activeVertices = activeQuads * VerticesPerQuad;
                if (activeVertices > 0)
                {
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.RenderPrepareFrameSetVertexBufferData);
                    try
                    {
                        targetMesh.SetVertexBufferData(
                            vertices,
                            0,
                            0,
                            activeVertices,
                            0,
                            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                            MeshUpdateFlags.DontNotifyMeshUsers);
                    }
                    finally
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.RenderPrepareFrameSetVertexBufferData);
                    }
                }

                int desiredActiveSubMeshCount = PendingSegmentCount;
                Bounds currentBounds = CurrentBounds();
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                try
                {
                    for (int activeSubMeshIndex = 0;
                         activeSubMeshIndex < desiredActiveSubMeshCount;
                         activeSubMeshIndex++)
                    {
                        if (segmentCursor >= totalSegments ||
                            allSegments[segmentCursor].ChunkIndex != chunkIndex ||
                            allSegments[segmentCursor].SubMeshIndex != activeSubMeshIndex)
                        {
                            throw new InvalidOperationException(
                                "Chunk submesh descriptors must be contiguous and sequential.");
                        }

                        BattleCentralRenderSegment segment = allSegments[segmentCursor];
                        targetMesh.SetSubMesh(
                            activeSubMeshIndex,
                            new SubMeshDescriptor(
                                segment.FirstQuad * IndicesPerQuad,
                                segment.QuadCount * IndicesPerQuad,
                                MeshTopology.Triangles)
                            {
                                baseVertex = 0,
                                firstVertex = segment.FirstQuad * VerticesPerQuad,
                                vertexCount = segment.QuadCount * VerticesPerQuad,
                                bounds = allSegmentBounds[segmentCursor].ToBounds(),
                            },
                            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                            MeshUpdateFlags.DontNotifyMeshUsers);
                        segmentCursor++;
                    }
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                }
                activeSubMeshCount = desiredActiveSubMeshCount;
                targetMesh.bounds = currentBounds;
                PendingSegmentCount = 0;
                hasBounds = false;
            }

            public void ClearActive()
            {
                ActiveQuadCount = 0;
                PendingSegmentCount = 0;
                Mesh targetMesh = mesh;
                if (targetMesh == null)
                {
                    activeSubMeshCount = 0;
                    hasBounds = false;
                    return;
                }
                int physicalSubMeshCount = targetMesh.subMeshCount;
                int inertSubMeshCount = Math.Min(activeSubMeshCount, physicalSubMeshCount);
                for (int subMeshIndex = 0; subMeshIndex < inertSubMeshCount; subMeshIndex++)
                    SetInertSubmesh(targetMesh, subMeshIndex);
                activeSubMeshCount = 0;
                targetMesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
                hasBounds = false;
            }

            public void Dispose()
            {
                Mesh targetMesh = mesh;
                mesh = null;
                if (targetMesh == null)
                    return;
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(targetMesh);
                else
                    UnityEngine.Object.DestroyImmediate(targetMesh);
            }

            private Mesh EnsureMesh()
            {
                // With Enter Play Mode domain reload disabled, this managed chunk can
                // outlive the native Mesh Unity destroys on exiting Play Mode.
                if (mesh != null)
                    return mesh;

                activeSubMeshCount = 0;
                mesh = CreateMesh();
                return mesh;
            }

            private Mesh CreateMesh()
            {
                var createdMesh = new Mesh
                {
                    name = $"NTSD Battle Central Chunk {chunkIndex}",
                    indexFormat = IndexFormat.UInt16,
                };
                createdMesh.MarkDynamic();
                createdMesh.SetVertexBufferParams(VerticesPerChunk, VertexLayout);
                createdMesh.SetIndexBufferParams(IndicesPerChunk, IndexFormat.UInt16);
                createdMesh.SetIndexBufferData(
                    indexTemplate,
                    0,
                    0,
                    indexTemplate.Length,
                    MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers);
                createdMesh.subMeshCount = 1;
                SetInertSubmesh(createdMesh, 0);
                createdMesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
                return createdMesh;
            }

            private static void SetInertSubmesh(Mesh targetMesh, int subMeshIndex)
            {
                targetMesh.SetSubMesh(
                    subMeshIndex,
                    new SubMeshDescriptor(0, 0, MeshTopology.Triangles)
                    {
                        baseVertex = 0,
                        firstVertex = 0,
                        vertexCount = 0,
                        bounds = new Bounds(Vector3.zero, Vector3.zero),
                    },
                    MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers);
            }

            private static BattleQuadVertex CreateVertex(
                float x,
                float y,
                float z,
                float u,
                float v,
                in BattleCentralResolvedResource resource)
            {
                return new BattleQuadVertex
                {
                    Position = new Vector3(x, y, z),
                    Color = resource.Color,
                    Uv = new Vector2(u, v),
                    AtlasSlice = resource.AtlasSlice,
                };
            }

            private void Encapsulate(
                float minX,
                float minY,
                float minZ,
                float maxX,
                float maxY,
                float maxZ)
            {
                if (!hasBounds)
                {
                    boundsMinX = minX;
                    boundsMinY = minY;
                    boundsMinZ = minZ;
                    boundsMaxX = maxX;
                    boundsMaxY = maxY;
                    boundsMaxZ = maxZ;
                    hasBounds = true;
                    return;
                }
                boundsMinX = Mathf.Min(boundsMinX, minX);
                boundsMinY = Mathf.Min(boundsMinY, minY);
                boundsMinZ = Mathf.Min(boundsMinZ, minZ);
                boundsMaxX = Mathf.Max(boundsMaxX, maxX);
                boundsMaxY = Mathf.Max(boundsMaxY, maxY);
                boundsMaxZ = Mathf.Max(boundsMaxZ, maxZ);
            }

            private Bounds CurrentBounds()
            {
                if (!hasBounds)
                    return new Bounds(Vector3.zero, Vector3.zero);
                var bounds = new Bounds();
                bounds.SetMinMax(
                    new Vector3(boundsMinX, boundsMinY, boundsMinZ),
                    new Vector3(boundsMaxX, boundsMaxY, boundsMaxZ));
                return bounds;
            }

        }
    }
}
