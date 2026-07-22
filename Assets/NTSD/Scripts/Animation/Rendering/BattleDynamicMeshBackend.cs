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
        private int activeChunkCount;
        private int segmentCount;
        private int mutationVersion;
        private bool disposed;

        public BattleCentralBuildDiagnostics Diagnostics => diagnostics;
        public int ActiveChunkCount => activeChunkCount;
        public int SegmentCount => segmentCount;
        public int AllocatedChunkCount => chunks.Length;
        internal int MutationVersion => mutationVersion;

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
            BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleDynamicMeshBackend));
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            mutationVersion++;

            int commandCount = frame?.CommandCount ?? 0;
            diagnostics.Reset(frame?.TickIndex ?? 0, commandCount, drawMode);
            segmentCount = 0;
            int resolvedCount = 0;
            int lastChunkIndex = -1;
            int lastSegmentIndex = -1;

            for (int commandIndex = 0; commandIndex < commandCount; commandIndex++)
            {
                BattleRenderCommand command = frame.GetCommand(commandIndex);
                BattleCentralResourceStatus status = resolver.Resolve(command, out BattleCentralResolvedResource resource);
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
                    }
                    // An unresolved command still occupies an authoritative position
                    // in the P3 stream. Never batch resolved commands across it.
                    lastSegmentIndex = -1;
                    lastChunkIndex = -1;
                    continue;
                }

                int chunkIndex = resolvedCount / QuadsPerChunk;
                int quadIndex = resolvedCount % QuadsPerChunk;
                EnsureChunk(chunkIndex);
                BattleMeshChunk chunk = chunks[chunkIndex];
                chunk.WriteQuad(quadIndex, command, resource);

                bool strict = drawMode == BattleCentralDrawMode.StrictOrderedDraw;
                bool canAppend = !strict && lastSegmentIndex >= 0 && lastChunkIndex == chunkIndex &&
                                 IsCompatible(segments[lastSegmentIndex], resource) &&
                                 segments[lastSegmentIndex].FirstQuad + segments[lastSegmentIndex].QuadCount == quadIndex;
                if (canAppend)
                {
                    BattleCentralRenderSegment previous = segments[lastSegmentIndex];
                    segments[lastSegmentIndex] = new BattleCentralRenderSegment(
                        previous.ChunkIndex,
                        previous.SubMeshIndex,
                        previous.FirstCommandIndex,
                        commandIndex - previous.FirstCommandIndex + 1,
                        previous.FirstQuad,
                        previous.QuadCount + 1,
                        previous.Texture,
                        previous.Material,
                        previous.MaterialVariant,
                        previous.AtlasSlice,
                        previous.BindingMode);
                }
                else
                {
                    EnsureSegmentCapacity(segmentCount + 1);
                    int subMeshIndex = chunk.PendingSegmentCount;
                    chunk.PendingSegmentCount++;
                    segments[segmentCount] = new BattleCentralRenderSegment(
                        chunkIndex,
                        subMeshIndex,
                        commandIndex,
                        1,
                        quadIndex,
                        1,
                        resource.Texture,
                        resource.Material,
                        resource.MaterialVariant,
                        resource.AtlasSlice,
                        resource.BindingMode);
                    lastSegmentIndex = segmentCount++;
                    lastChunkIndex = chunkIndex;
                }

                resolvedCount++;
            }

            activeChunkCount = resolvedCount == 0 ? 0 : (resolvedCount + QuadsPerChunk - 1) / QuadsPerChunk;
            int segmentCursor = 0;
            for (int chunkIndex = 0; chunkIndex < activeChunkCount; chunkIndex++)
            {
                BattleMeshChunk chunk = chunks[chunkIndex];
                int activeQuads = Math.Min(QuadsPerChunk, resolvedCount - chunkIndex * QuadsPerChunk);
                chunk.Upload(chunkIndex, activeQuads, segments, ref segmentCursor, segmentCount);
            }
            for (int chunkIndex = activeChunkCount; chunkIndex < chunks.Length; chunkIndex++)
                chunks[chunkIndex]?.ClearActive();

            diagnostics.ResolvedCommandCount = resolvedCount;
            diagnostics.ActiveChunkCount = activeChunkCount;
            diagnostics.SegmentCount = segmentCount;
        }

        public void Clear()
        {
            mutationVersion++;
            segmentCount = 0;
            activeChunkCount = 0;
            for (int i = 0; i < chunks.Length; i++)
                chunks[i]?.ClearActive();
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
            activeChunkCount = 0;
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
        }

        private void EnsureSegmentCapacity(int required)
        {
            if (required <= segments.Length)
                return;
            int next = segments.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref segments, next);
            diagnostics.CapacityGrowthCount++;
        }

        private static bool IsCompatible(
            in BattleCentralRenderSegment segment,
            in BattleCentralResolvedResource resource)
        {
            return segment.Texture == resource.Texture &&
                   segment.Material == resource.Material &&
                   segment.MaterialVariant == resource.MaterialVariant &&
                   segment.BindingMode == resource.BindingMode &&
                   (resource.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ||
                    segment.AtlasSlice == resource.AtlasSlice);
        }

        private struct BattleQuadVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public float AtlasSlice;
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
            private bool hasBounds;
            private Vector3 boundsMin;
            private Vector3 boundsMax;

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
                in BattleCentralResolvedResource resource)
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

                Encapsulate(new Vector3(left, bottom, z));
                Encapsulate(new Vector3(right, top, z));
            }

            public void Upload(
                int chunkIndex,
                int activeQuads,
                BattleCentralRenderSegment[] allSegments,
                ref int segmentCursor,
                int totalSegments)
            {
                Mesh targetMesh = EnsureMesh();
                ActiveQuadCount = activeQuads;
                int activeVertices = activeQuads * VerticesPerQuad;
                if (activeVertices > 0)
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

                targetMesh.subMeshCount = PendingSegmentCount;
                while (segmentCursor < totalSegments &&
                       allSegments[segmentCursor].ChunkIndex == chunkIndex)
                {
                    BattleCentralRenderSegment segment = allSegments[segmentCursor];
                    targetMesh.SetSubMesh(
                        segment.SubMeshIndex,
                        new SubMeshDescriptor(
                            segment.FirstQuad * IndicesPerQuad,
                            segment.QuadCount * IndicesPerQuad,
                            MeshTopology.Triangles)
                        {
                            baseVertex = 0,
                            firstVertex = segment.FirstQuad * VerticesPerQuad,
                            vertexCount = segment.QuadCount * VerticesPerQuad,
                            bounds = CurrentBounds(),
                        },
                        MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                        MeshUpdateFlags.DontNotifyMeshUsers);
                    segmentCursor++;
                    if (segmentCursor >= totalSegments ||
                        allSegments[segmentCursor].ChunkIndex != segment.ChunkIndex)
                    {
                        break;
                    }
                }
                targetMesh.bounds = CurrentBounds();
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
                    hasBounds = false;
                    return;
                }
                // Unity 2022.3 releases the native index buffer when subMeshCount
                // reaches zero. Keep one inert submesh so the immutable UInt16
                // template survives empty frames and can be reused on the next build.
                SetInertSubmesh(targetMesh);
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
                SetInertSubmesh(createdMesh);
                return createdMesh;
            }

            private static void SetInertSubmesh(Mesh targetMesh)
            {
                targetMesh.subMeshCount = 1;
                targetMesh.SetSubMesh(
                    0,
                    new SubMeshDescriptor(0, 0, MeshTopology.Triangles)
                    {
                        baseVertex = 0,
                        firstVertex = 0,
                        vertexCount = 0,
                        bounds = new Bounds(Vector3.zero, Vector3.zero),
                    },
                    MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers);
                targetMesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
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

            private void Encapsulate(Vector3 position)
            {
                if (!hasBounds)
                {
                    boundsMin = position;
                    boundsMax = position;
                    hasBounds = true;
                    return;
                }
                boundsMin = Vector3.Min(boundsMin, position);
                boundsMax = Vector3.Max(boundsMax, position);
            }

            private Bounds CurrentBounds()
            {
                if (!hasBounds)
                    return new Bounds(Vector3.zero, Vector3.zero);
                var bounds = new Bounds();
                bounds.SetMinMax(boundsMin, boundsMax);
                return bounds;
            }
        }
    }
}
