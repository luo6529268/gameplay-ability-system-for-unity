using System;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering
{
    [Serializable]
    public struct BattleHealthBarStyle
    {
        [SerializeField][Min(1f)] private float widthPixels;
        [SerializeField][Min(1f)] private float heightPixels;
        [SerializeField][Min(0f)] private float borderPixels;
        [SerializeField][Min(0f)] private float headGapPixels;
        [SerializeField] private Vector2 offsetPixels;
        [SerializeField] private Color32 backgroundColor;
        [SerializeField] private Color32 recoverableColor;
        [SerializeField] private Color32 currentColor;

        public float WidthPixels => widthPixels;
        public float HeightPixels => heightPixels;
        public float BorderPixels => borderPixels;
        public float HeadGapPixels => headGapPixels;
        public Vector2 OffsetPixels => offsetPixels;
        public Color32 BackgroundColor => backgroundColor;
        public Color32 RecoverableColor => recoverableColor;
        public Color32 CurrentColor => currentColor;

        public static BattleHealthBarStyle Default => new BattleHealthBarStyle(
            60f,
            6f,
            1f,
            6f,
            Vector2.zero,
            new Color32(20, 20, 20, 230),
            new Color32(116, 24, 24, 255),
            new Color32(235, 48, 48, 255));

        public BattleHealthBarStyle(
            float widthPixels,
            float heightPixels,
            float borderPixels,
            float headGapPixels,
            Vector2 offsetPixels,
            Color32 backgroundColor,
            Color32 recoverableColor,
            Color32 currentColor)
        {
            this.widthPixels = widthPixels;
            this.heightPixels = heightPixels;
            this.borderPixels = borderPixels;
            this.headGapPixels = headGapPixels;
            this.offsetPixels = offsetPixels;
            this.backgroundColor = backgroundColor;
            this.recoverableColor = recoverableColor;
            this.currentColor = currentColor;
        }

        internal BattleHealthBarStyle Normalized()
        {
            float normalizedWidth = Mathf.Max(1f, widthPixels);
            float normalizedHeight = Mathf.Max(1f, heightPixels);
            float normalizedBorder = Mathf.Clamp(
                borderPixels,
                0f,
                Mathf.Min(normalizedWidth, normalizedHeight) * 0.5f);
            return new BattleHealthBarStyle(
                normalizedWidth,
                normalizedHeight,
                normalizedBorder,
                Mathf.Max(0f, headGapPixels),
                offsetPixels,
                backgroundColor,
                recoverableColor,
                currentColor);
        }
    }

    internal static class BattleHealthBarAnchor
    {
        internal const float DefaultCharacterHeightPixels = 79f;

        internal static float ResolveStableCharacterHeightPixels(
            LF2CharacterData characterData)
        {
            float height = 0f;
            if (characterData?.files != null)
            {
                for (int index = 0; index < characterData.files.Count; index++)
                {
                    SpriteFileInfo file = characterData.files[index];
                    if (file != null && file.height > height)
                        height = file.height;
                }
            }

            return height > 0f ? height : DefaultCharacterHeightPixels;
        }
    }

    public readonly struct BattleHealthBarInstance
    {
        public BattleHealthBarInstance(
            Vector2 spriteTopCenterWorld,
            float worldZ,
            int currentHealth,
            int recoverableHealth,
            int maximumHealth)
        {
            SpriteTopCenterWorld = spriteTopCenterWorld;
            WorldZ = worldZ;
            CurrentHealth = currentHealth;
            RecoverableHealth = recoverableHealth;
            MaximumHealth = maximumHealth;
        }

        public Vector2 SpriteTopCenterWorld { get; }
        public float WorldZ { get; }
        public int CurrentHealth { get; }
        public int RecoverableHealth { get; }
        public int MaximumHealth { get; }
    }

    public sealed class BattleHealthBarBatchBackend : IDisposable
    {
        public const int QuadsPerBar = 3;
        public const int VerticesPerBar = QuadsPerBar * BattleDynamicMeshBackend.VerticesPerQuad;
        public const int IndicesPerBar = QuadsPerBar * BattleDynamicMeshBackend.IndicesPerQuad;
        public const int MaximumBarsPerBatch =
            BattleDynamicMeshBackend.QuadsPerChunk / QuadsPerBar;

        private static readonly VertexAttributeDescriptor[] VertexLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1),
        };

        private HealthBarVertex[] vertices = Array.Empty<HealthBarVertex>();
        private ushort[] indices = Array.Empty<ushort>();
        private BattleHealthBarInstance[] runtimeInstances =
            Array.Empty<BattleHealthBarInstance>();
        private Mesh mesh;
        private int capacity;
        private bool disposed;

        public Mesh Mesh => mesh;
        public int ActiveBarCount { get; private set; }
        public int ActiveQuadCount => ActiveBarCount * QuadsPerBar;
        public int ActiveVertexCount => ActiveBarCount * VerticesPerBar;
        public int ActiveIndexCount => ActiveBarCount * IndicesPerBar;
        public int Capacity => capacity;
        public BattlePresentationFrame BuiltFrame { get; private set; }
        public int MutationVersion { get; private set; }

        public void PrepareCapacity(int requiredBars)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleHealthBarBatchBackend));
            if (requiredBars < 0 || requiredBars > MaximumBarsPerBatch)
                throw new ArgumentOutOfRangeException(nameof(requiredBars));
            if (requiredBars <= capacity)
                return;

            int nextCapacity = Mathf.NextPowerOfTwo(Mathf.Max(1, requiredBars));
            nextCapacity = Mathf.Min(nextCapacity, MaximumBarsPerBatch);
            if (nextCapacity < requiredBars)
                nextCapacity = MaximumBarsPerBatch;

            vertices = new HealthBarVertex[nextCapacity * VerticesPerBar];
            indices = new ushort[nextCapacity * IndicesPerBar];
            runtimeInstances = new BattleHealthBarInstance[nextCapacity];
            for (int quadIndex = 0; quadIndex < nextCapacity * QuadsPerBar; quadIndex++)
            {
                int vertex = quadIndex * BattleDynamicMeshBackend.VerticesPerQuad;
                int index = quadIndex * BattleDynamicMeshBackend.IndicesPerQuad;
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

        public void Build(
            BattleHealthBarInstance[] instances,
            int instanceCount,
            in BattleHealthBarStyle configuredStyle)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleHealthBarBatchBackend));
            if (instances == null)
                throw new ArgumentNullException(nameof(instances));
            if (instanceCount < 0 || instanceCount > instances.Length)
                throw new ArgumentOutOfRangeException(nameof(instanceCount));

            BuiltFrame = null;
            MutationVersion++;
            PrepareCapacity(instanceCount);
            BattleHealthBarStyle style = configuredStyle.Normalized();
            int writtenBars = 0;
            bool hasBounds = false;
            Vector3 minimum = default;
            Vector3 maximum = default;

            for (int sourceIndex = 0; sourceIndex < instanceCount; sourceIndex++)
            {
                BattleHealthBarInstance instance = instances[sourceIndex];
                if (instance.MaximumHealth <= 0)
                    continue;

                float maximumHealth = Mathf.Max(1, instance.MaximumHealth);
                float recoverableRatio = Mathf.Clamp01(instance.RecoverableHealth / maximumHealth);
                float currentRatio = Mathf.Clamp01(instance.CurrentHealth / maximumHealth);

                float width = style.WidthPixels * NTSDRenderSpace.UnitsPerPixelX;
                float height = style.HeightPixels * NTSDRenderSpace.UnitsPerPixelY;
                float borderX = style.BorderPixels * NTSDRenderSpace.UnitsPerPixelX;
                float borderY = style.BorderPixels * NTSDRenderSpace.UnitsPerPixelY;
                float centerX = instance.SpriteTopCenterWorld.x +
                                style.OffsetPixels.x * NTSDRenderSpace.UnitsPerPixelX;
                float bottom = instance.SpriteTopCenterWorld.y +
                               (style.HeadGapPixels + style.OffsetPixels.y) *
                               NTSDRenderSpace.UnitsPerPixelY;
                float top = bottom + height;
                float left = centerX - width * 0.5f;
                float right = left + width;
                float innerLeft = left + borderX;
                float innerRight = right - borderX;
                float innerBottom = bottom + borderY;
                float innerTop = top - borderY;
                float innerWidth = Mathf.Max(0f, innerRight - innerLeft);
                float recoverableRight = innerLeft + innerWidth * recoverableRatio;
                float currentRight = innerLeft + innerWidth * currentRatio;
                int firstQuad = writtenBars * QuadsPerBar;

                WriteQuad(
                    firstQuad,
                    left,
                    bottom,
                    right,
                    top,
                    instance.WorldZ,
                    style.BackgroundColor);
                WriteQuad(
                    firstQuad + 1,
                    innerLeft,
                    innerBottom,
                    recoverableRight,
                    innerTop,
                    instance.WorldZ,
                    style.RecoverableColor);
                WriteQuad(
                    firstQuad + 2,
                    innerLeft,
                    innerBottom,
                    currentRight,
                    innerTop,
                    instance.WorldZ,
                    style.CurrentColor);

                var barMinimum = new Vector3(left, bottom, instance.WorldZ);
                var barMaximum = new Vector3(right, top, instance.WorldZ);
                if (!hasBounds)
                {
                    minimum = barMinimum;
                    maximum = barMaximum;
                    hasBounds = true;
                }
                else
                {
                    minimum = Vector3.Min(minimum, barMinimum);
                    maximum = Vector3.Max(maximum, barMaximum);
                }

                writtenBars++;
            }

            ActiveBarCount = writtenBars;
            if (writtenBars == 0)
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
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
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
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            targetMesh.bounds = bounds;
        }

        public void BuildFromFrame(
            BattlePresentationFrame frame,
            in BattleHealthBarStyle configuredStyle,
            bool enabled)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleHealthBarBatchBackend));
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            int maximumCandidateCount = enabled
                ? Mathf.Min(frame.EntityCount, MaximumBarsPerBatch)
                : 0;
            PrepareCapacity(maximumCandidateCount);
            int count = 0;
            if (enabled)
            {
                for (int commandIndex = 0; commandIndex < frame.CommandCount; commandIndex++)
                {
                    BattleRenderCommand command = frame.GetCommand(commandIndex);
                    if (command.Type != BattleRenderCommandType.Entity ||
                        !command.ShowOverheadHealthBar || command.MaximumHealth <= 0)
                    {
                        continue;
                    }
                    if (count >= MaximumBarsPerBatch)
                    {
                        throw new InvalidOperationException(
                            $"Runtime overhead health bars exceed the single-batch limit " +
                            $"of {MaximumBarsPerBatch}.");
                    }
                    if (count >= capacity)
                        PrepareCapacity(count + 1);

                    float spriteCenterX;
                    float spriteTop;
                    if (command.HasStableHealthAnchor)
                    {
                        spriteCenterX = command.StableHealthAnchorWorld.x;
                        spriteTop = command.StableHealthAnchorWorld.y;
                    }
                    else
                    {
                        float spriteWidth = command.Size.x * NTSDRenderSpace.UnitsPerPixelX *
                                            NTSDRenderSpace.BattleVisualScale;
                        float spriteHeight = command.Size.y * NTSDRenderSpace.UnitsPerPixelY *
                                             NTSDRenderSpace.BattleVisualScale;
                        float spriteLeft = command.Position.x - command.Pivot.x * spriteWidth;
                        spriteTop = command.Position.y +
                                    (1f - command.Pivot.y) * spriteHeight;
                        spriteCenterX = spriteLeft + spriteWidth * 0.5f;
                    }
                    runtimeInstances[count++] = new BattleHealthBarInstance(
                        new Vector2(spriteCenterX, spriteTop),
                        command.Position.z,
                        command.CurrentHealth,
                        command.RecoverableHealth,
                        command.MaximumHealth);
                }
            }

            Build(runtimeInstances, count, configuredStyle);
            BuiltFrame = frame;
        }

        public void Clear()
        {
            if (disposed)
                return;
            BuiltFrame = null;
            MutationVersion++;
            ClearActiveMesh();
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            DestroyMesh();
            vertices = Array.Empty<HealthBarVertex>();
            indices = Array.Empty<ushort>();
            runtimeInstances = Array.Empty<BattleHealthBarInstance>();
            capacity = 0;
            ActiveBarCount = 0;
            BuiltFrame = null;
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

        private void RecreateMesh()
        {
            DestroyMesh();
            mesh = new Mesh
            {
                name = "NTSD Battle Health Bar Batch",
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
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            mesh.subMeshCount = 1;
            ClearActiveMesh();
        }

        private void ClearActiveMesh()
        {
            ActiveBarCount = 0;
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
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
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
            int quadIndex,
            float left,
            float bottom,
            float right,
            float top,
            float z,
            Color32 color)
        {
            int vertex = quadIndex * BattleDynamicMeshBackend.VerticesPerQuad;
            WriteVertex(ref vertices[vertex], left, bottom, z, 0f, 0f, color);
            WriteVertex(ref vertices[vertex + 1], left, top, z, 0f, 1f, color);
            WriteVertex(ref vertices[vertex + 2], right, bottom, z, 1f, 0f, color);
            WriteVertex(ref vertices[vertex + 3], right, top, z, 1f, 1f, color);
        }

        private static void WriteVertex(
            ref HealthBarVertex vertex,
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

        private struct HealthBarVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public float AtlasSlice;
        }
    }
}
