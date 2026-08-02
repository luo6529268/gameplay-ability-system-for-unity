using System;
using System.Collections.Generic;
using NTSD.Simulation;
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

    public enum BattleCentralEntityDiagnosticReason : byte
    {
        None = 0,
        InvalidRuntimeHandle = 1,
        GenerationMismatch = 2,
        MissingSnapshotEntity = 3,
        PresentationVisibilityFalse = 4,
        CommandSuppressed = 5,
        MissingCatalogKey = 6,
        MissingTextureOrMaterial = 7,
        InvalidCentralBinding = 8,
        UnsupportedRenderState = 9,
        UnresolvedResource = 10,
        NotSubmitted = 11,
        StalePlan = 12,
        BackendMutationMismatch = 13,
    }

    public readonly struct BattleCentralEntityDiagnostic
    {
        internal BattleCentralEntityDiagnostic(
            BattleCentralEntityDiagnosticReason reason,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType,
            in BattlePresentationEntitySnapshot snapshot,
            bool hasSnapshot,
            in BattleRenderCommand command,
            bool hasCommand,
            in BattleCentralResolvedResource resource,
            bool hasResolvedResource,
            int commandIndex,
            int segmentIndex,
            int chunkIndex,
            bool submitted)
        {
            Reason = reason;
            Handle = handle;
            CommandType = commandType;
            StableId = hasSnapshot ? snapshot.StableId : 0;
            ObjectId = hasSnapshot ? snapshot.ObjectId : -1;
            CurrentDatObjectId = hasSnapshot ? snapshot.CurrentDatObjectId : -1;
            EffectivePic = hasSnapshot ? snapshot.EffectivePic : -1;
            FrameId = hasSnapshot ? snapshot.FrameId : -1;
            EntityVisible = hasSnapshot && snapshot.EntityVisible;
            ShadowVisible = hasSnapshot && snapshot.ShadowVisible;
            PresentationBaseOrder = hasSnapshot ? snapshot.PresentationBaseOrder : -1;
            HasLogicalResourceKey = hasCommand && command.SpriteDescriptor.HasLogicalResourceKey;
            LogicalResourceKey = HasLogicalResourceKey
                ? command.SpriteDescriptor.LogicalResourceKey
                : default;
            BindingMode = hasResolvedResource
                ? resource.BindingMode
                : BattleSpriteCentralBindingMode.SourceTexture2D;
            AtlasSlice = hasResolvedResource ? resource.AtlasSlice : -1;
            AtlasPageIndex = hasResolvedResource ? resource.AtlasPageIndex : -1;
            NormalizedUv = hasResolvedResource
                ? resource.NormalizedUv
                : hasCommand ? command.NormalizedUv : default;
            Pivot = hasResolvedResource ? resource.Pivot : hasCommand ? command.Pivot : default;
            Position = hasCommand ? command.Position : default;
            FlipX = hasCommand && command.FlipX;
            FlipY = hasCommand && command.FlipY;
            Color = hasCommand ? command.Color : default;
            SortOrder = hasCommand ? command.SortOrder : -1;
            LocalSequence = hasCommand ? command.LocalSequence : -1;
            CommandIndex = commandIndex;
            SegmentIndex = segmentIndex;
            ChunkIndex = chunkIndex;
            Submitted = submitted;
            HasSnapshot = hasSnapshot;
            HasCommand = hasCommand;
            HasResolvedResource = hasResolvedResource;
        }

        public BattleCentralEntityDiagnosticReason Reason { get; }
        public RuntimeEntityHandle Handle { get; }
        public BattleRenderCommandType CommandType { get; }
        public int StableId { get; }
        public int ObjectId { get; }
        public int CurrentDatObjectId { get; }
        public int EffectivePic { get; }
        public int FrameId { get; }
        public bool EntityVisible { get; }
        public bool ShadowVisible { get; }
        public int PresentationBaseOrder { get; }
        public bool HasLogicalResourceKey { get; }
        public BattleVisualResourceKey LogicalResourceKey { get; }
        public BattleSpriteCentralBindingMode BindingMode { get; }
        public int AtlasSlice { get; }
        public int AtlasPageIndex { get; }
        public Rect NormalizedUv { get; }
        public Vector2 Pivot { get; }
        public Vector3 Position { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }
        public Color32 Color { get; }
        public int SortOrder { get; }
        public int LocalSequence { get; }
        public int CommandIndex { get; }
        public int SegmentIndex { get; }
        public int ChunkIndex { get; }
        public bool Submitted { get; }
        public bool HasSnapshot { get; }
        public bool HasCommand { get; }
        public bool HasResolvedResource { get; }
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
            BattleSpriteCentralBindingMode bindingMode = BattleSpriteCentralBindingMode.SourceTexture2D,
            int atlasPageIndex = -1)
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
            AtlasPageIndex = atlasPageIndex;
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
        public int AtlasPageIndex { get; }

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
            BattleSpriteCentralBindingMode bindingMode = BattleSpriteCentralBindingMode.SourceTexture2D,
            int atlasPageIndex = -1)
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
            AtlasPageIndex = bindingMode == BattleSpriteCentralBindingMode.AtlasPageTexture2D
                ? atlasPageIndex
                : -1;
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
        public int AtlasPageIndex { get; }
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
        public BattleCentralResourceStatus FirstUnresolvedStatus { get; internal set; }
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
            FirstUnresolvedStatus = BattleCentralResourceStatus.Resolved;
            ActiveChunkCount = 0;
            SegmentCount = 0;
            DrawMode = drawMode;
        }
    }

    public sealed class BattleCatalogCentralResourceResolver : IBattleCentralResourceResolver
    {
        private const int InitialEntityTemplateCapacity = 128;

        private readonly Dictionary<BattleSpriteKey, BattleCentralResourceTemplate> entityTemplates =
            new Dictionary<BattleSpriteKey, BattleCentralResourceTemplate>(InitialEntityTemplateCapacity);
        private readonly BattleCentralResourceTemplate[] sparkTemplates =
            new BattleCentralResourceTemplate[BattleCommonVisualCatalog.SparkFrameCount];
        private readonly int[] initializedSparkTemplateSlots =
            new int[BattleCommonVisualCatalog.SparkFrameCount];
        private readonly BattleCentralResourceTemplate[][] wordTemplates = CreateWordTemplateCache();
        private readonly int[] initializedWordTemplateSlots =
            new int[BattleCommonVisualCatalog.WordSheetCount *
                    BattleCommonVisualCatalog.WordGlyphsPerSheet];

        private BattleSpriteCatalog catalog = BattleSpriteCatalog.Empty;
        private BattleCommonVisualCatalog commonVisualCatalog = BattleCommonVisualCatalog.Empty;
        private Material fallbackMaterial;
        private Material arrayMaterial;
        private Material configuredFallbackMaterial;
        private Material configuredArrayMaterial;
        private bool fallbackMaterialContractValid;
        private bool arrayMaterialContractValid;
        private BattleCentralResourceTemplate shadowTemplate;
        private int initializedSparkTemplateCount;
        private int initializedWordTemplateCount;
        private bool hasConfiguration;

        public int ConfigureCalls { get; private set; }
        public int NoOpHits { get; private set; }
        public int TemplateClears { get; private set; }
        public int BindingGeneration { get; private set; }
        public int DestroyedResourceInvalidations { get; private set; }

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
            ConfigureCalls++;
            BattleSpriteCatalog nextCatalog = value ?? BattleSpriteCatalog.Empty;
            BattleCommonVisualCatalog nextCommonVisualCatalog =
                commonVisuals ?? BattleCommonVisualCatalog.Empty;
            bool nextFallbackMaterialContractValid =
                BattleSpriteMaterialContract.IsDeclaredCentralMaterial(
                    sharedFallbackMaterial,
                    false);
            bool nextArrayMaterialContractValid =
                BattleSpriteMaterialContract.IsDeclaredCentralMaterial(
                    sharedArrayMaterial,
                    true);
            bool hasDestroyedMaterial =
                IsDestroyedUnityObject(sharedFallbackMaterial) ||
                IsDestroyedUnityObject(sharedArrayMaterial);
            if (hasConfiguration &&
                !hasDestroyedMaterial &&
                ReferenceEquals(catalog, nextCatalog) &&
                ReferenceEquals(commonVisualCatalog, nextCommonVisualCatalog) &&
                ReferenceEquals(fallbackMaterial, sharedFallbackMaterial) &&
                ReferenceEquals(arrayMaterial, sharedArrayMaterial) &&
                fallbackMaterialContractValid == nextFallbackMaterialContractValid &&
                arrayMaterialContractValid == nextArrayMaterialContractValid)
            {
                NoOpHits++;
                return;
            }

            ClearTemplates();
            catalog = nextCatalog;
            commonVisualCatalog = nextCommonVisualCatalog;
            fallbackMaterial = sharedFallbackMaterial;
            arrayMaterial = sharedArrayMaterial;
            configuredFallbackMaterial = fallbackMaterial;
            configuredArrayMaterial = arrayMaterial;
            fallbackMaterialContractValid = nextFallbackMaterialContractValid;
            arrayMaterialContractValid = nextArrayMaterialContractValid;
            hasConfiguration = true;
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
                !command.SpriteDescriptor.LogicalResourceKey.IsEntitySprite)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            BattleSpriteKey key = command.SpriteDescriptor.LogicalResourceKey.EntitySpriteKey;
            bool hasTemplate = entityTemplates.TryGetValue(
                key,
                out BattleCentralResourceTemplate template);
            bool matchesTrustedIdentity =
                hasTemplate && template.MatchesTrustedIdentity(command);
            if (hasTemplate && !matchesTrustedIdentity &&
                template.HasDestroyedResource)
            {
                InvalidateDestroyedResourceGeneration();
                template = default;
                matchesTrustedIdentity = false;
            }
            if (!template.IsInitialized ||
                (!matchesTrustedIdentity &&
                 !template.MatchesConfiguredMaterial(fallbackMaterial, arrayMaterial)))
            {
                template = BuildEntityTemplate(key);
                entityTemplates[key] = template;
                matchesTrustedIdentity = template.MatchesTrustedIdentity(command);
            }

            return matchesTrustedIdentity
                ? template.ResolveTrusted(command, out resource)
                : template.Resolve(command, out resource);
        }

        private BattleCentralResourceStatus ResolveCommonWordGlyph(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            if (!command.RenderState.IsSupported)
            {
                resource = default;
                return BattleCentralResourceStatus.UnsupportedRenderState;
            }
            if (!command.SpriteDescriptor.HasLogicalResourceKey ||
                !command.SpriteDescriptor.LogicalResourceKey.IsCommonWordGlyph)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            BattleVisualResourceKey key = command.SpriteDescriptor.LogicalResourceKey;
            int sheetIndex = key.CommonWordSheetIndex;
            int charCode = key.CommonWordCharCode;
            if (sheetIndex < 0 || sheetIndex >= BattleCommonVisualCatalog.WordSheetCount ||
                charCode < 0 || charCode >= BattleCommonVisualCatalog.WordGlyphsPerSheet)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            BattleCentralResourceTemplate template = wordTemplates[sheetIndex][charCode];
            bool matchesTrustedIdentity = template.MatchesTrustedIdentity(command);
            if (!matchesTrustedIdentity &&
                template.HasDestroyedResource)
            {
                InvalidateDestroyedResourceGeneration();
                template = default;
                matchesTrustedIdentity = false;
            }
            if (!template.IsInitialized ||
                (!matchesTrustedIdentity &&
                 !template.MatchesConfiguredMaterial(fallbackMaterial, arrayMaterial)))
            {
                template = BuildCommonWordTemplate(sheetIndex, charCode);
                if (!wordTemplates[sheetIndex][charCode].IsInitialized)
                {
                    initializedWordTemplateSlots[initializedWordTemplateCount++] =
                        sheetIndex * BattleCommonVisualCatalog.WordGlyphsPerSheet + charCode;
                }
                wordTemplates[sheetIndex][charCode] = template;
                matchesTrustedIdentity = template.MatchesTrustedIdentity(command);
            }

            return matchesTrustedIdentity
                ? template.ResolveTrusted(command, out resource)
                : template.Resolve(command, out resource);
        }

        private BattleCentralResourceStatus ResolveCommonShadow(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            if (!command.RenderState.IsSupported)
            {
                resource = default;
                return BattleCentralResourceStatus.UnsupportedRenderState;
            }
            if (!command.SpriteDescriptor.HasLogicalResourceKey ||
                command.SpriteDescriptor.LogicalResourceKey != BattleVisualResourceKey.CommonShadow)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            bool matchesTrustedIdentity = shadowTemplate.MatchesTrustedIdentity(command);
            if (!matchesTrustedIdentity &&
                shadowTemplate.HasDestroyedResource)
            {
                InvalidateDestroyedResourceGeneration();
                shadowTemplate = default;
                matchesTrustedIdentity = false;
            }
            if (!shadowTemplate.IsInitialized ||
                (!matchesTrustedIdentity &&
                 !shadowTemplate.MatchesConfiguredMaterial(fallbackMaterial, arrayMaterial)))
            {
                shadowTemplate = BuildCommonTemplate(
                    BattleRenderCommandType.Shadow,
                    -1,
                    -1,
                    commonVisualCatalog.Shadow);
                matchesTrustedIdentity = shadowTemplate.MatchesTrustedIdentity(command);
            }

            return matchesTrustedIdentity
                ? shadowTemplate.ResolveTrusted(command, out resource)
                : shadowTemplate.Resolve(command, out resource);
        }

        private BattleCentralResourceStatus ResolveCommonSpark(
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            if (!command.RenderState.IsSupported)
            {
                resource = default;
                return BattleCentralResourceStatus.UnsupportedRenderState;
            }
            if (!command.SpriteDescriptor.HasLogicalResourceKey ||
                !command.SpriteDescriptor.LogicalResourceKey.IsCommonSpark)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            int pic = command.SpriteDescriptor.LogicalResourceKey.CommonSparkPic;
            if (pic < 0 || pic >= BattleCommonVisualCatalog.SparkFrameCount)
            {
                resource = default;
                return BattleCentralResourceStatus.UnresolvedVisual;
            }

            BattleCentralResourceTemplate template = sparkTemplates[pic];
            bool matchesTrustedIdentity = template.MatchesTrustedIdentity(command);
            if (!matchesTrustedIdentity &&
                template.HasDestroyedResource)
            {
                InvalidateDestroyedResourceGeneration();
                template = default;
                matchesTrustedIdentity = false;
            }
            if (!template.IsInitialized ||
                (!matchesTrustedIdentity &&
                 !template.MatchesConfiguredMaterial(fallbackMaterial, arrayMaterial)))
            {
                commonVisualCatalog.TryGetSpark(pic, out BattleCommonVisualBinding binding);
                template = BuildCommonTemplate(
                    BattleRenderCommandType.HitRecord,
                    -1,
                    pic,
                    binding);
                if (!sparkTemplates[pic].IsInitialized)
                    initializedSparkTemplateSlots[initializedSparkTemplateCount++] = pic;
                sparkTemplates[pic] = template;
                matchesTrustedIdentity = template.MatchesTrustedIdentity(command);
            }

            return matchesTrustedIdentity
                ? template.ResolveTrusted(command, out resource)
                : template.Resolve(command, out resource);
        }

        private BattleCentralResourceTemplate BuildEntityTemplate(BattleSpriteKey key)
        {
            if (!catalog.TryGet(key, out BattleSpriteEntry entry) || entry == null)
                return BattleCentralResourceTemplate.Unresolved;

            BattleCentralResourceSignature signature =
                BattleCentralResourceSignature.FromEntity(entry);
            return BuildTemplate(signature, entry.CentralBinding);
        }

        private BattleCentralResourceTemplate BuildCommonWordTemplate(int sheetIndex, int charCode)
        {
            commonVisualCatalog.TryGetWordGlyph(
                sheetIndex,
                charCode,
                out BattleCommonVisualBinding binding);
            return BuildCommonTemplate(
                BattleRenderCommandType.OverlayGlyph,
                sheetIndex,
                charCode,
                binding);
        }

        private BattleCentralResourceTemplate BuildCommonTemplate(
            BattleRenderCommandType commandType,
            int visualDataId,
            int effectivePic,
            BattleCommonVisualBinding binding)
        {
            if (binding == null)
                return BattleCentralResourceTemplate.Unresolved;

            BattleCentralResourceSignature signature =
                BattleCentralResourceSignature.FromCommon(
                    commandType,
                    visualDataId,
                    effectivePic,
                    binding);
            return BuildTemplate(signature, binding.CentralBinding);
        }

        private BattleCentralResourceTemplate BuildTemplate(
            in BattleCentralResourceSignature signature,
            in BattleSpriteCentralBinding binding)
        {
            bool expectsArray =
                binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray;
            Material material = expectsArray ? arrayMaterial : fallbackMaterial;
            bool materialContractValid = IsConfiguredMaterialContractValid(material, expectsArray);
            if (!binding.IsValid || !materialContractValid)
            {
                return BattleCentralResourceTemplate.UnresolvedWithSignature(
                    signature,
                    material,
                    binding.Mode);
            }

            return BattleCentralResourceTemplate.Resolved(
                signature,
                binding.Texture,
                material,
                binding.NormalizedUv,
                binding.AtlasSlice,
                binding.Mode,
                binding.AtlasPageIndex);
        }

        private bool IsConfiguredMaterialContractValid(Material material, bool expectsArray)
        {
            if (expectsArray && ReferenceEquals(material, configuredArrayMaterial))
                return arrayMaterialContractValid;
            if (!expectsArray && ReferenceEquals(material, configuredFallbackMaterial))
                return fallbackMaterialContractValid;
            return BattleSpriteMaterialContract.IsDeclaredCentralMaterial(material, expectsArray);
        }

        private void InvalidateDestroyedResourceGeneration()
        {
            DestroyedResourceInvalidations++;
            ClearTemplates();
        }

        private static bool IsDestroyedUnityObject(UnityEngine.Object value)
        {
            return !ReferenceEquals(value, null) && value == null;
        }

        private void AdvanceBindingGeneration()
        {
            BindingGeneration = BindingGeneration == int.MaxValue
                ? 1
                : BindingGeneration + 1;
        }

        private void ClearTemplates()
        {
            TemplateClears++;
            AdvanceBindingGeneration();
            entityTemplates.Clear();
            shadowTemplate = default;

            for (int index = 0; index < initializedSparkTemplateCount; index++)
                sparkTemplates[initializedSparkTemplateSlots[index]] = default;
            initializedSparkTemplateCount = 0;

            for (int index = 0; index < initializedWordTemplateCount; index++)
            {
                int slot = initializedWordTemplateSlots[index];
                int sheetIndex = slot / BattleCommonVisualCatalog.WordGlyphsPerSheet;
                int charCode = slot % BattleCommonVisualCatalog.WordGlyphsPerSheet;
                wordTemplates[sheetIndex][charCode] = default;
            }
            initializedWordTemplateCount = 0;
        }

        private static BattleCentralResourceTemplate[][] CreateWordTemplateCache()
        {
            var templates = new BattleCentralResourceTemplate[
                BattleCommonVisualCatalog.WordSheetCount][];
            for (int sheetIndex = 0;
                 sheetIndex < BattleCommonVisualCatalog.WordSheetCount;
                 sheetIndex++)
            {
                templates[sheetIndex] = new BattleCentralResourceTemplate[
                    BattleCommonVisualCatalog.WordGlyphsPerSheet];
            }
            return templates;
        }

        private readonly struct BattleCentralResourceTemplate
        {
            private BattleCentralResourceTemplate(
                BattleCentralResourceStatus status,
                in BattleCentralResourceSignature signature,
                Texture texture,
                Material material,
                Rect normalizedUv,
                int atlasSlice,
                BattleSpriteCentralBindingMode bindingMode,
                int atlasPageIndex,
                bool tracksMaterial)
            {
                IsInitialized = true;
                Status = status;
                Signature = signature;
                Texture = texture;
                Material = material;
                NormalizedUv = normalizedUv;
                AtlasSlice = atlasSlice;
                BindingMode = bindingMode;
                AtlasPageIndex = atlasPageIndex;
                TracksMaterial = tracksMaterial;
            }

            public static BattleCentralResourceTemplate Unresolved { get; } =
                new BattleCentralResourceTemplate(
                    BattleCentralResourceStatus.UnresolvedVisual,
                    default,
                    null,
                    null,
                    default,
                    0,
                    BattleSpriteCentralBindingMode.SourceTexture2D,
                    -1,
                    false);

            public bool IsInitialized { get; }
            public bool HasDestroyedResource =>
                IsInitialized &&
                Status == BattleCentralResourceStatus.Resolved &&
                (IsDestroyedUnityObject(Texture) || IsDestroyedUnityObject(Material));
            private BattleCentralResourceStatus Status { get; }
            private BattleCentralResourceSignature Signature { get; }
            private Texture Texture { get; }
            private Material Material { get; }
            private Rect NormalizedUv { get; }
            private int AtlasSlice { get; }
            private BattleSpriteCentralBindingMode BindingMode { get; }
            private int AtlasPageIndex { get; }
            private bool TracksMaterial { get; }

            public static BattleCentralResourceTemplate UnresolvedWithSignature(
                in BattleCentralResourceSignature signature,
                Material material,
                BattleSpriteCentralBindingMode bindingMode)
            {
                return new BattleCentralResourceTemplate(
                    BattleCentralResourceStatus.UnresolvedVisual,
                    signature,
                    null,
                    material,
                    default,
                    0,
                    bindingMode,
                    -1,
                    true);
            }

            public static BattleCentralResourceTemplate Resolved(
                in BattleCentralResourceSignature signature,
                Texture texture,
                Material material,
                Rect normalizedUv,
                int atlasSlice,
                BattleSpriteCentralBindingMode bindingMode,
                int atlasPageIndex)
            {
                return new BattleCentralResourceTemplate(
                    BattleCentralResourceStatus.Resolved,
                    signature,
                    texture,
                    material,
                    normalizedUv,
                    atlasSlice,
                    bindingMode,
                    atlasPageIndex,
                    true);
            }

            public bool MatchesConfiguredMaterial(
                Material currentFallbackMaterial,
                Material currentArrayMaterial)
            {
                if (!TracksMaterial)
                    return true;
                Material currentMaterial =
                    BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray
                        ? currentArrayMaterial
                        : currentFallbackMaterial;
                return ReferenceEquals(Material, currentMaterial);
            }

            public bool MatchesTrustedIdentity(in BattleRenderCommand command)
            {
                return IsInitialized &&
                       Signature.MatchesTrustedIdentity(command);
            }

            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                if (Status != BattleCentralResourceStatus.Resolved)
                {
                    resource = default;
                    return Status;
                }
                if (!Signature.Matches(command))
                {
                    resource = default;
                    return BattleCentralResourceStatus.UnresolvedVisual;
                }

                return ResolveTrusted(command, out resource);
            }

            public BattleCentralResourceStatus ResolveTrusted(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                if (Status != BattleCentralResourceStatus.Resolved)
                {
                    resource = default;
                    return Status;
                }

                resource = new BattleCentralResolvedResource(
                    Texture,
                    Material,
                    NormalizedUv,
                    Signature.PixelSize,
                    Signature.Pivot,
                    command.Color,
                    (int)Signature.MaterialSemantic,
                    AtlasSlice,
                    BindingMode,
                    AtlasPageIndex);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private readonly struct BattleCentralResourceSignature
        {
            private BattleCentralResourceSignature(
                BattleRenderCommandType commandType,
                int visualDataId,
                int effectivePic,
                bool requiresSprite,
                bool hasSprite,
                int spriteInstanceId,
                int textureInstanceId,
                int materialInstanceId,
                Rect pixelRect,
                Vector2 pivot,
                Rect normalizedUv,
                Vector2 pixelSize,
                BattleVisualResourceKey logicalResourceKey,
                SpriteMaskInteraction maskInteraction,
                BattleSpriteMaterialSemantic materialSemantic,
                bool isSupported,
                object trustedResourceIdentity)
            {
                CommandType = commandType;
                VisualDataId = visualDataId;
                EffectivePic = effectivePic;
                RequiresSprite = requiresSprite;
                HasSprite = hasSprite;
                SpriteInstanceId = spriteInstanceId;
                TextureInstanceId = textureInstanceId;
                MaterialInstanceId = materialInstanceId;
                PixelRect = pixelRect;
                Pivot = pivot;
                NormalizedUv = normalizedUv;
                PixelSize = pixelSize;
                LogicalResourceKey = logicalResourceKey;
                MaskInteraction = maskInteraction;
                MaterialSemantic = materialSemantic;
                IsSupported = isSupported;
                TrustedResourceIdentity = trustedResourceIdentity;
            }

            public Vector2 Pivot { get; }
            public Vector2 PixelSize { get; }
            public BattleSpriteMaterialSemantic MaterialSemantic { get; }
            private BattleRenderCommandType CommandType { get; }
            private int VisualDataId { get; }
            private int EffectivePic { get; }
            private bool RequiresSprite { get; }
            private bool HasSprite { get; }
            private int SpriteInstanceId { get; }
            private int TextureInstanceId { get; }
            private int MaterialInstanceId { get; }
            private Rect PixelRect { get; }
            private Rect NormalizedUv { get; }
            private BattleVisualResourceKey LogicalResourceKey { get; }
            private SpriteMaskInteraction MaskInteraction { get; }
            private bool IsSupported { get; }
            private object TrustedResourceIdentity { get; }

            public static BattleCentralResourceSignature FromEntity(BattleSpriteEntry entry)
            {
                Sprite sprite = entry.LegacySprite;
                Texture2D texture = entry.SharedTexture;
                BattleSpriteRenderState renderState = BattleSpriteRenderState.Default();
                return new BattleCentralResourceSignature(
                    BattleRenderCommandType.Entity,
                    entry.Key.VisualDataId,
                    entry.Key.EffectivePic,
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    0,
                    entry.PixelRect,
                    entry.Pivot,
                    entry.NormalizedUv,
                    new Vector2(entry.PixelWidth, entry.PixelHeight),
                    BattleVisualResourceKey.FromEntity(entry.Key),
                    renderState.MaskInteraction,
                    renderState.MaterialSemantic,
                    renderState.IsSupported,
                    entry);
            }

            public static BattleCentralResourceSignature FromCommon(
                BattleRenderCommandType commandType,
                int visualDataId,
                int effectivePic,
                BattleCommonVisualBinding binding)
            {
                return new BattleCentralResourceSignature(
                    commandType,
                    visualDataId,
                    effectivePic,
                    true,
                    binding.Sprite != null,
                    binding.SpriteInstanceId,
                    binding.TextureInstanceId,
                    binding.MaterialInstanceId,
                    binding.PixelRect,
                    binding.Pivot,
                    binding.NormalizedUv,
                    binding.PixelSize,
                    binding.Key,
                    binding.RenderState.MaskInteraction,
                    binding.RenderState.MaterialSemantic,
                    binding.RenderState.IsSupported,
                    binding);
            }

            public bool Matches(in BattleRenderCommand command)
            {
                if (MatchesTrustedIdentity(command))
                    return true;

                BattleSpriteValueDescriptor descriptor = command.SpriteDescriptor;
                return command.Type == CommandType &&
                       command.VisualDataId == VisualDataId &&
                       command.EffectivePic == EffectivePic &&
                       descriptor.RequiresSprite == RequiresSprite &&
                       descriptor.HasSprite == HasSprite &&
                       descriptor.HasLogicalResourceKey &&
                       descriptor.LogicalResourceKey == LogicalResourceKey &&
                       descriptor.SpriteInstanceId == SpriteInstanceId &&
                       descriptor.TextureInstanceId == TextureInstanceId &&
                       descriptor.MaterialInstanceId == MaterialInstanceId &&
                       descriptor.PixelRect == PixelRect &&
                       descriptor.PivotNormalized == Pivot &&
                       command.Pivot == Pivot &&
                       command.NormalizedUv == NormalizedUv &&
                       command.Size == PixelSize &&
                       command.RenderState.MaskInteraction == MaskInteraction &&
                       command.RenderState.MaterialSemantic == MaterialSemantic &&
                       command.RenderState.IsSupported == IsSupported;
            }

            public bool MatchesTrustedIdentity(in BattleRenderCommand command)
            {
                return TrustedResourceIdentity != null &&
                       ReferenceEquals(
                           TrustedResourceIdentity,
                           command.TrustedResourceIdentity);
            }
        }
    }
}
