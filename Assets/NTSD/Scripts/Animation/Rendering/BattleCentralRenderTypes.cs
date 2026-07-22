using System;
using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation.Rendering
{
    public enum BattleCentralDrawMode : byte
    {
        OrderedChunks = 0,
        StrictOrderedDraw = 1,
        SingleMeshDiagnosticOnly = 2,
    }

    public enum BattleCentralResourceStatus : byte
    {
        Resolved = 0,
        UnresolvedVisual = 1,
        UnsupportedCategory = 2,
        UnsupportedRenderState = 3,
    }

    public readonly struct BattleCentralResolvedResource
    {
        public BattleCentralResolvedResource(
            Texture texture,
            Material material,
            Rect normalizedUv,
            Vector2 pixelSize,
            Vector2 pivot,
            Color32 color,
            int materialVariant = 0,
            int atlasSlice = 0,
            BattleSpriteCentralBindingMode bindingMode = BattleSpriteCentralBindingMode.SourceTexture2D)
        {
            Texture = texture;
            Material = material;
            NormalizedUv = normalizedUv;
            PixelSize = pixelSize;
            Pivot = pivot;
            Color = color;
            MaterialVariant = materialVariant;
            AtlasSlice = atlasSlice;
            BindingMode = bindingMode;
        }

        public Texture Texture { get; }
        public Material Material { get; }
        public Rect NormalizedUv { get; }
        public Vector2 PixelSize { get; }
        public Vector2 Pivot { get; }
        public Color32 Color { get; }
        public int MaterialVariant { get; }
        public int AtlasSlice { get; }
        public BattleSpriteCentralBindingMode BindingMode { get; }

        internal bool HasDrawableResource => Texture != null && Material != null;
    }

    public interface IBattleCentralResourceResolver
    {
        BattleCentralResourceStatus Resolve(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource);
    }

    public readonly struct BattleCentralRenderSegment
    {
        public BattleCentralRenderSegment(
            int chunkIndex,
            int subMeshIndex,
            int firstCommandIndex,
            int commandCount,
            int firstQuad,
            int quadCount,
            Texture texture,
            Material material,
            int materialVariant,
            int atlasSlice,
            BattleSpriteCentralBindingMode bindingMode = BattleSpriteCentralBindingMode.SourceTexture2D)
        {
            ChunkIndex = chunkIndex;
            SubMeshIndex = subMeshIndex;
            FirstCommandIndex = firstCommandIndex;
            CommandCount = commandCount;
            FirstQuad = firstQuad;
            QuadCount = quadCount;
            Texture = texture;
            Material = material;
            MaterialVariant = materialVariant;
            AtlasSlice = atlasSlice;
            BindingMode = bindingMode;
        }

        public int ChunkIndex { get; }
        public int SubMeshIndex { get; }
        public int FirstCommandIndex { get; }
        public int CommandCount { get; }
        public int FirstQuad { get; }
        public int QuadCount { get; }
        public Texture Texture { get; }
        public Material Material { get; }
        public int MaterialVariant { get; }
        public int AtlasSlice { get; }
        public BattleSpriteCentralBindingMode BindingMode { get; }
    }

    public sealed class BattleCentralBuildDiagnostics
    {
        public int TickIndex { get; internal set; }
        public int SourceCommandCount { get; internal set; }
        public int ResolvedCommandCount { get; internal set; }
        public int UnresolvedCommandCount { get; internal set; }
        public int UnsupportedCategoryCount { get; internal set; }
        public int UnsupportedRenderStateCount { get; internal set; }
        public int FirstUnresolvedCommandIndex { get; internal set; } = -1;
        public BattleRenderCommandType FirstUnresolvedCommandType { get; internal set; }
        public int ActiveChunkCount { get; internal set; }
        public int SegmentCount { get; internal set; }
        public int CapacityGrowthCount { get; internal set; }
        public BattleCentralDrawMode DrawMode { get; internal set; }

        internal void Reset(int tickIndex, int sourceCommandCount, BattleCentralDrawMode drawMode)
        {
            TickIndex = tickIndex;
            SourceCommandCount = sourceCommandCount;
            ResolvedCommandCount = 0;
            UnresolvedCommandCount = 0;
            UnsupportedCategoryCount = 0;
            UnsupportedRenderStateCount = 0;
            FirstUnresolvedCommandIndex = -1;
            FirstUnresolvedCommandType = default;
            ActiveChunkCount = 0;
            SegmentCount = 0;
            DrawMode = drawMode;
        }
    }

    public sealed class BattleCatalogCentralResourceResolver : IBattleCentralResourceResolver
    {
        private BattleSpriteCatalog catalog = BattleSpriteCatalog.Empty;
        private BattleCommonVisualCatalog commonVisualCatalog = BattleCommonVisualCatalog.Empty;
        private Material fallbackMaterial;
        private Material arrayMaterial;

        public void Configure(BattleSpriteCatalog value, Material sharedMaterial)
        {
            Configure(value, BattleCommonVisualCatalog.Empty, sharedMaterial, sharedMaterial);
        }

        public void Configure(
            BattleSpriteCatalog value,
            Material sharedFallbackMaterial,
            Material sharedArrayMaterial)
        {
            Configure(
                value,
                BattleCommonVisualCatalog.Empty,
                sharedFallbackMaterial,
                sharedArrayMaterial);
        }

        public void Configure(
            BattleSpriteCatalog value,
            BattleCommonVisualCatalog commonVisuals,
            Material sharedFallbackMaterial,
            Material sharedArrayMaterial)
        {
            catalog = value ?? BattleSpriteCatalog.Empty;
            commonVisualCatalog = commonVisuals ?? BattleCommonVisualCatalog.Empty;
            fallbackMaterial = sharedFallbackMaterial;
            arrayMaterial = sharedArrayMaterial;
        }

        public BattleCentralResourceStatus Resolve(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            if (command.Type == BattleRenderCommandType.Shadow)
                return ResolveCommonShadow(command, out resource);

            if (command.Type == BattleRenderCommandType.HitRecord)
                return ResolveCommonSpark(command, out resource);

            if (command.Type == BattleRenderCommandType.OverlayGlyph)
                return ResolveCommonWordGlyph(command, out resource);

            if (command.Type != BattleRenderCommandType.Entity)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            if (!command.RenderState.IsSupported)
            {
                resource = default;
                return BattleCentralResourceStatus.UnsupportedRenderState;
            }

            if (!command.SpriteDescriptor.HasLogicalResourceKey ||
                !command.SpriteDescriptor.LogicalResourceKey.IsEntitySprite ||
                !catalog.TryGet(
                    command.SpriteDescriptor.LogicalResourceKey.EntitySpriteKey,
                    out BattleSpriteEntry entry) ||
                entry.Key.VisualDataId != command.VisualDataId ||
                entry.Key.EffectivePic != command.EffectivePic)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            BattleSpriteCentralBinding binding = entry.CentralBinding;
            Material material = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray
                ? arrayMaterial
                : fallbackMaterial;
            bool expectsArray = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray;
            if (!binding.IsValid ||
                !BattleSpriteMaterialContract.IsDeclaredCentralMaterial(material, expectsArray))
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            resource = new BattleCentralResolvedResource(
                binding.Texture,
                material,
                binding.NormalizedUv,
                new Vector2(entry.PixelWidth, entry.PixelHeight),
                entry.Pivot,
                command.Color,
                (int)command.RenderState.MaterialSemantic,
                binding.AtlasSlice,
                binding.Mode);
            return BattleCentralResourceStatus.Resolved;
        }

        private BattleCentralResourceStatus ResolveCommonWordGlyph(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = default;
            if (!command.RenderState.IsSupported)
                return BattleCentralResourceStatus.UnsupportedRenderState;
            if (!command.SpriteDescriptor.HasLogicalResourceKey ||
                !command.SpriteDescriptor.LogicalResourceKey.IsCommonWordGlyph ||
                command.VisualDataId != command.SpriteDescriptor.LogicalResourceKey.CommonWordSheetIndex ||
                command.EffectivePic != command.SpriteDescriptor.LogicalResourceKey.CommonWordCharCode ||
                !commonVisualCatalog.TryGetWordGlyph(
                    command.VisualDataId,
                    command.EffectivePic,
                    out BattleCommonVisualBinding binding) ||
                binding.Key != command.SpriteDescriptor.LogicalResourceKey ||
                command.SpriteDescriptor.SpriteInstanceId != binding.SpriteInstanceId ||
                command.SpriteDescriptor.TextureInstanceId != binding.TextureInstanceId ||
                command.SpriteDescriptor.MaterialInstanceId != binding.MaterialInstanceId ||
                command.SpriteDescriptor.PixelRect != binding.PixelRect ||
                command.SpriteDescriptor.PivotNormalized != binding.Pivot ||
                command.Size != binding.PixelSize ||
                command.RenderState.MaterialSemantic != binding.RenderState.MaterialSemantic ||
                command.RenderState.MaskInteraction != binding.RenderState.MaskInteraction ||
                !BattleSpriteMaterialContract.IsDeclaredCentralMaterial(fallbackMaterial, false))
            {
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            resource = new BattleCentralResolvedResource(
                binding.Texture,
                fallbackMaterial,
                binding.NormalizedUv,
                binding.PixelSize,
                binding.Pivot,
                command.Color,
                (int)command.RenderState.MaterialSemantic,
                0,
                BattleSpriteCentralBindingMode.SourceTexture2D);
            return BattleCentralResourceStatus.Resolved;
        }

        private BattleCentralResourceStatus ResolveCommonShadow(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            BattleCommonVisualBinding binding = commonVisualCatalog.Shadow;
            if (!command.RenderState.IsSupported)
            {
                resource = default;
                return BattleCentralResourceStatus.UnsupportedRenderState;
            }
            if (binding == null ||
                !command.SpriteDescriptor.HasLogicalResourceKey ||
                command.SpriteDescriptor.LogicalResourceKey != BattleVisualResourceKey.CommonShadow ||
                command.SpriteDescriptor.SpriteInstanceId != binding.SpriteInstanceId ||
                command.SpriteDescriptor.TextureInstanceId != binding.TextureInstanceId ||
                command.SpriteDescriptor.MaterialInstanceId != binding.MaterialInstanceId ||
                command.SpriteDescriptor.PixelRect != binding.PixelRect ||
                command.SpriteDescriptor.PivotNormalized != binding.Pivot ||
                !BattleSpriteMaterialContract.IsDeclaredCentralMaterial(fallbackMaterial, false))
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            resource = new BattleCentralResolvedResource(
                binding.Texture,
                fallbackMaterial,
                binding.NormalizedUv,
                binding.PixelSize,
                binding.Pivot,
                command.Color,
                (int)command.RenderState.MaterialSemantic,
                0,
                BattleSpriteCentralBindingMode.SourceTexture2D);
            return BattleCentralResourceStatus.Resolved;
        }

        private BattleCentralResourceStatus ResolveCommonSpark(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = default;
            if (!command.RenderState.IsSupported)
                return BattleCentralResourceStatus.UnsupportedRenderState;
            if (!command.SpriteDescriptor.HasLogicalResourceKey ||
                !command.SpriteDescriptor.LogicalResourceKey.IsCommonSpark ||
                command.EffectivePic != command.SpriteDescriptor.LogicalResourceKey.CommonSparkPic ||
                !commonVisualCatalog.TryGetSpark(command.EffectivePic, out BattleCommonVisualBinding binding) ||
                binding.Key != command.SpriteDescriptor.LogicalResourceKey ||
                command.SpriteDescriptor.SpriteInstanceId != binding.SpriteInstanceId ||
                command.SpriteDescriptor.TextureInstanceId != binding.TextureInstanceId ||
                command.SpriteDescriptor.PixelRect != binding.PixelRect ||
                command.SpriteDescriptor.PivotNormalized != binding.Pivot ||
                command.Size != binding.PixelSize ||
                command.RenderState.MaterialSemantic != binding.RenderState.MaterialSemantic ||
                command.RenderState.MaskInteraction != binding.RenderState.MaskInteraction ||
                !BattleSpriteMaterialContract.IsDeclaredCentralMaterial(fallbackMaterial, false))
            {
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            resource = new BattleCentralResolvedResource(
                binding.Texture,
                fallbackMaterial,
                binding.NormalizedUv,
                binding.PixelSize,
                binding.Pivot,
                command.Color,
                (int)command.RenderState.MaterialSemantic,
                0,
                BattleSpriteCentralBindingMode.SourceTexture2D);
            return BattleCentralResourceStatus.Resolved;
        }
    }
}
