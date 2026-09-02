using System;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering
{
    internal static class BattleFootMarkerSizing
    {
        internal static float ResolveStableCharacterScale(float stableCharacterHeightPixels)
        {
            return stableCharacterHeightPixels > 0f
                ? stableCharacterHeightPixels /
                  BattleHealthBarAnchor.DefaultCharacterHeightPixels
                : 1f;
        }
    }

    public sealed class BattleFootMarkerBatchBackend : IDisposable
    {
        public const int MaximumMarkersPerBatch = BattleDynamicMeshBackend.QuadsPerChunk;
        public const int VerticesPerMarker = BattleDynamicMeshBackend.VerticesPerQuad;
        public const int IndicesPerMarker = BattleDynamicMeshBackend.IndicesPerQuad;

        private static readonly VertexAttributeDescriptor[] VertexLayout =
        {
            new VertexAttributeDescriptor(
                VertexAttribute.Position,
                VertexAttributeFormat.Float32,
                3),
            new VertexAttributeDescriptor(
                VertexAttribute.Color,
                VertexAttributeFormat.UNorm8,
                4),
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0,
                VertexAttributeFormat.Float32,
                2),
            new VertexAttributeDescriptor(
                VertexAttribute.TexCoord1,
                VertexAttributeFormat.Float32,
                1),
        };

        private FootMarkerVertex[] vertices = Array.Empty<FootMarkerVertex>();
        private ushort[] indices = Array.Empty<ushort>();
        private Mesh mesh;
        private int capacity;
        private bool disposed;

        public Mesh Mesh => mesh;
        public Texture Texture { get; private set; }
        public int ActiveMarkerCount { get; private set; }
        public int ActiveQuadCount => ActiveMarkerCount;
        public int ActiveVertexCount => ActiveMarkerCount * VerticesPerMarker;
        public int ActiveIndexCount => ActiveMarkerCount * IndicesPerMarker;
        public int Capacity => capacity;
        public BattlePresentationFrame BuiltFrame { get; private set; }
        public int MutationVersion { get; private set; }

        public void PrepareCapacity(int requiredMarkers)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleFootMarkerBatchBackend));
            if (requiredMarkers < 0 || requiredMarkers > MaximumMarkersPerBatch)
                throw new ArgumentOutOfRangeException(nameof(requiredMarkers));
            if (requiredMarkers <= capacity)
                return;

            int nextCapacity = Mathf.NextPowerOfTwo(Mathf.Max(1, requiredMarkers));
            nextCapacity = Mathf.Min(nextCapacity, MaximumMarkersPerBatch);
            if (nextCapacity < requiredMarkers)
                nextCapacity = MaximumMarkersPerBatch;

            vertices = new FootMarkerVertex[nextCapacity * VerticesPerMarker];
            indices = new ushort[nextCapacity * IndicesPerMarker];
            for (int markerIndex = 0; markerIndex < nextCapacity; markerIndex++)
            {
                int vertex = markerIndex * VerticesPerMarker;
                int index = markerIndex * IndicesPerMarker;
                indices[index] = (ushort)vertex;
                indices[index + 1] = (ushort)(vertex + 1);
                indices[index + 2] = (ushort)(vertex + 2);
                indices[index + 3] = (ushort)(vertex + 2);
                indices[index + 4] = (ushort)(vertex + 1);
                indices[index + 5] = (ushort)(vertex + 3);
            }

            capacity = nextCapacity;
            RecreateMesh();
        }

        public void BuildFromFrame(
            BattlePresentationFrame frame,
            Sprite sprite,
            in BattleFootMarkerStyle configuredStyle,
            bool enabled)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleFootMarkerBatchBackend));
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            MutationVersion++;
            BuiltFrame = frame;
            bool canDraw = enabled && sprite != null && sprite.texture != null;
            Texture = canDraw ? sprite.texture : null;
            if (!canDraw)
            {
                ClearActiveMesh();
                return;
            }

            int maximumCandidateCount = Mathf.Min(
                frame.EntityCount > 0 ? frame.EntityCount : frame.CommandCount,
                MaximumMarkersPerBatch);
            PrepareCapacity(maximumCandidateCount);
            BattleFootMarkerStyle style = configuredStyle.Normalized();
            Rect uv = ResolveNormalizedUv(sprite);
            float offsetX = style.OffsetPixels.x * NTSDRenderSpace.UnitsPerPixelX;
            float offsetY = style.OffsetPixels.y * NTSDRenderSpace.UnitsPerPixelY;
            int writtenMarkers = 0;
            bool hasBounds = false;
            Vector3 minimum = default;
            Vector3 maximum = default;

            for (int commandIndex = 0; commandIndex < frame.CommandCount; commandIndex++)
            {
                BattleRenderCommand command = frame.GetCommand(commandIndex);
                if (command.Type != BattleRenderCommandType.Entity ||
                    !command.ShowSelfFootMarker)
                {
                    continue;
                }
                if (writtenMarkers >= MaximumMarkersPerBatch)
                {
                    throw new InvalidOperationException(
                        $"Runtime FootSelf markers exceed the single-batch limit " +
                        $"of {MaximumMarkersPerBatch}.");
                }
                if (writtenMarkers >= capacity)
                    PrepareCapacity(writtenMarkers + 1);

                Vector2 anchor = command.HasStableFootAnchor
                    ? command.StableFootAnchorWorld
                    : new Vector2(command.Position.x, command.Position.y);
                float markerScale = command.FootMarkerScale;
                float width = style.WidthPixels * markerScale *
                              NTSDRenderSpace.UnitsPerPixelX;
                float height = style.HeightPixels * markerScale *
                               NTSDRenderSpace.UnitsPerPixelY;
                float centerX = anchor.x + offsetX;
                float centerY = anchor.y + offsetY;
                float left = centerX - width * 0.5f;
                float right = centerX + width * 0.5f;
                float bottom = centerY - height * 0.5f;
                float top = centerY + height * 0.5f;
                WriteQuad(
                    writtenMarkers,
                    left,
                    bottom,
                    right,
                    top,
                    command.Position.z,
                    uv,
                    style.Tint);

                var markerMinimum = new Vector3(left, bottom, command.Position.z);
                var markerMaximum = new Vector3(right, top, command.Position.z);
                if (!hasBounds)
                {
                    minimum = markerMinimum;
                    maximum = markerMaximum;
                    hasBounds = true;
                }
                else
                {
                    minimum = Vector3.Min(minimum, markerMinimum);
                    maximum = Vector3.Max(maximum, markerMaximum);
                }

                writtenMarkers++;
            }

            ActiveMarkerCount = writtenMarkers;
            if (writtenMarkers == 0)
            {
                ClearActiveMesh();
                return;
            }

            Mesh targetMesh = mesh;
            targetMesh.SetVertexBufferData(
                vertices,
                0,
                0,
                ActiveVertexCount,
                0,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            var bounds = new Bounds();
            bounds.SetMinMax(minimum, maximum);
            targetMesh.SetSubMesh(
                0,
                new SubMeshDescriptor(0, ActiveIndexCount, MeshTopology.Triangles)
                {
                    baseVertex = 0,
                    firstVertex = 0,
                    vertexCount = ActiveVertexCount,
                    bounds = bounds,
                },
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            targetMesh.bounds = bounds;
        }

        public void Clear()
        {
            if (disposed)
                return;
            MutationVersion++;
            BuiltFrame = null;
            Texture = null;
            ClearActiveMesh();
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            DestroyMesh();
            vertices = Array.Empty<FootMarkerVertex>();
            indices = Array.Empty<ushort>();
            capacity = 0;
            ActiveMarkerCount = 0;
            BuiltFrame = null;
            Texture = null;
        }

        internal Vector3 GetVertexPosition(int index)
        {
            if ((uint)index >= (uint)ActiveVertexCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return vertices[index].Position;
        }

        internal Color32 GetVertexColor(int index)
        {
            if ((uint)index >= (uint)ActiveVertexCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return vertices[index].Color;
        }

        internal Vector2 GetVertexUv(int index)
        {
            if ((uint)index >= (uint)ActiveVertexCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return vertices[index].Uv;
        }

        private void RecreateMesh()
        {
            DestroyMesh();
            mesh = new Mesh
            {
                name = "NTSD Battle Foot Marker Batch",
                indexFormat = IndexFormat.UInt16,
                hideFlags = HideFlags.HideAndDontSave,
            };
            mesh.MarkDynamic();
            mesh.SetVertexBufferParams(vertices.Length, VertexLayout);
            mesh.SetIndexBufferParams(indices.Length, IndexFormat.UInt16);
            mesh.SetIndexBufferData(
                indices,
                0,
                0,
                indices.Length,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            mesh.subMeshCount = 1;
            ClearActiveMesh();
        }

        private void ClearActiveMesh()
        {
            ActiveMarkerCount = 0;
            if (mesh == null)
                return;
            mesh.SetSubMesh(
                0,
                new SubMeshDescriptor(0, 0, MeshTopology.Triangles)
                {
                    baseVertex = 0,
                    firstVertex = 0,
                    vertexCount = 0,
                    bounds = new Bounds(Vector3.zero, Vector3.zero),
                },
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            mesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
        }

        private void DestroyMesh()
        {
            Mesh target = mesh;
            mesh = null;
            if (target == null)
                return;
            if (Application.isPlaying)
                UnityEngine.Object.Destroy(target);
            else
                UnityEngine.Object.DestroyImmediate(target);
        }

        private void WriteQuad(
            int markerIndex,
            float left,
            float bottom,
            float right,
            float top,
            float z,
            Rect uv,
            Color32 color)
        {
            int vertex = markerIndex * VerticesPerMarker;
            WriteVertex(ref vertices[vertex], left, bottom, z, uv.xMin, uv.yMin, color);
            WriteVertex(ref vertices[vertex + 1], left, top, z, uv.xMin, uv.yMax, color);
            WriteVertex(ref vertices[vertex + 2], right, bottom, z, uv.xMax, uv.yMin, color);
            WriteVertex(ref vertices[vertex + 3], right, top, z, uv.xMax, uv.yMax, color);
        }

        private static void WriteVertex(
            ref FootMarkerVertex vertex,
            float x,
            float y,
            float z,
            float u,
            float v,
            Color32 color)
        {
            vertex.Position = new Vector3(x, y, z);
            vertex.Color = color;
            vertex.Uv = new Vector2(u, v);
            vertex.AtlasSlice = 0f;
        }

        private static Rect ResolveNormalizedUv(Sprite sprite)
        {
            Rect rect = sprite.textureRect;
            Texture2D texture = sprite.texture;
            return new Rect(
                rect.x / texture.width,
                rect.y / texture.height,
                rect.width / texture.width,
                rect.height / texture.height);
        }

        private struct FootMarkerVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public float AtlasSlice;
        }
    }
}
