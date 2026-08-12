using System;
using System.Collections.Generic;
using System.Threading;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using Unity.Profiling;
using UnityEngine;

namespace NTSD.Simulation.Presentation
{
    public enum BattleRenderCommandType : byte
    {
        Shadow = 0,
        Entity = 1,
        OverlayGlyph = 2,
        HitRecord = 3,
    }

    public enum BattlePresentationDifferenceKind : byte
    {
        None = 0,
        ExpectedMissing = 1,
        UnexpectedLegacy = 2,
        Category = 3,
        Identity = 4,
        Visual = 5,
        Position = 6,
        Size = 7,
        Flip = 8,
        SortOrder = 9,
        Color = 10,
        RenderState = 11,
        ResourceKey = 12,
    }

    public enum BattleOverlayParityState : byte
    {
        None = 0,
        AuthorityExpectedButLegacyMissing = 1,
    }

    public enum BattlePresentationParityStatus : byte
    {
        None = 0,
        PendingLegacyFrame = 1,
        Complete = 2,
        IncompleteLegacyFrame = 3,
    }

    public enum BattleSpriteMaterialSemantic : byte
    {
        Unsupported = 0,
        PremultipliedSpriteAlpha = 1,
    }

    public readonly struct BattleSpriteRenderState
    {
        public BattleSpriteRenderState(
            Color32 color,
            bool flipX,
            bool flipY,
            SpriteMaskInteraction maskInteraction,
            BattleSpriteMaterialSemantic materialSemantic)
        {
            Color = color;
            FlipX = flipX;
            FlipY = flipY;
            MaskInteraction = maskInteraction;
            MaterialSemantic = materialSemantic;
        }

        public Color32 Color { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }
        public SpriteMaskInteraction MaskInteraction { get; }
        public BattleSpriteMaterialSemantic MaterialSemantic { get; }
        public bool IsSupported =>
            MaskInteraction == SpriteMaskInteraction.None &&
            MaterialSemantic == BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;

        public static BattleSpriteRenderState Default(bool flipX = false)
        {
            return new BattleSpriteRenderState(
                new Color32(255, 255, 255, 255),
                flipX,
                false,
                SpriteMaskInteraction.None,
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
        }
    }

    public static class BattleSpriteMaterialContract
    {
        public const string BuiltInSpriteShaderName = "Sprites/Default";
        public const string CentralTextureShaderName = "NTSD/BattleCentralTransparent";
        public const string CentralArrayShaderName = "NTSD/BattleCentralTransparentArray";
        public const string AlphaContractTag = "NTSDAlphaContract";
        public const string PremultipliedAlphaContract = "PremultipliedSpriteAlpha";

        private static readonly BattleSpriteMaterialClassifier Classifier =
            new BattleSpriteMaterialClassifier();

        public static BattleSpriteMaterialSemantic Classify(Material material)
        {
            return Classifier.Classify(material);
        }

        public static bool IsDeclaredCentralMaterial(Material material, bool textureArray)
        {
            return Classifier.IsDeclaredCentralMaterial(material, textureArray);
        }
    }

    public readonly struct BattleSpriteValueDescriptor
    {
        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized)
            : this(
                requiresSprite,
                hasSprite,
                spriteInstanceId,
                textureInstanceId,
                materialInstanceId,
                pixelRect,
                pivotNormalized,
                false,
                default(BattleSpriteKey))
        {
        }

        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized,
            bool hasLogicalResourceKey,
            BattleSpriteKey logicalResourceKey)
        {
            RequiresSprite = requiresSprite;
            HasSprite = hasSprite;
            SpriteInstanceId = spriteInstanceId;
            TextureInstanceId = textureInstanceId;
            MaterialInstanceId = materialInstanceId;
            PixelRect = pixelRect;
            PivotNormalized = pivotNormalized;
            HasLogicalResourceKey = hasLogicalResourceKey;
            LogicalResourceKey = BattleVisualResourceKey.FromEntity(logicalResourceKey);
        }

        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized,
            BattleVisualResourceKey logicalResourceKey)
        {
            RequiresSprite = requiresSprite;
            HasSprite = hasSprite;
            SpriteInstanceId = spriteInstanceId;
            TextureInstanceId = textureInstanceId;
            MaterialInstanceId = materialInstanceId;
            PixelRect = pixelRect;
            PivotNormalized = pivotNormalized;
            HasLogicalResourceKey = true;
            LogicalResourceKey = logicalResourceKey;
        }

        public bool RequiresSprite { get; }
        public bool HasSprite { get; }
        public int SpriteInstanceId { get; }
        public int TextureInstanceId { get; }
        public int MaterialInstanceId { get; }
        public Rect PixelRect { get; }
        public Vector2 PivotNormalized { get; }
        public bool HasLogicalResourceKey { get; }
        public BattleVisualResourceKey LogicalResourceKey { get; }
    }

    public readonly struct BattlePresentationHitRecordSnapshot
    {
        public BattlePresentationHitRecordSnapshot(int age, int anchorX, int anchorZ)
        {
            Age = age;
            AnchorX = anchorX;
            AnchorZ = anchorZ;
        }

        public int Age { get; }
        public int AnchorX { get; }
        public int AnchorZ { get; }
    }

    public readonly struct BattleHitRecordOwnerSnapshot
    {
        public BattleHitRecordOwnerSnapshot(
            RuntimeEntityHandle handle,
            int stableId,
            int zInt,
            int runtimeSlot,
            int presentationBaseOrder,
            float renderOffsetX,
            int cameraX,
            int hitRecordStart,
            int hitRecordCount)
        {
            Handle = handle;
            StableId = stableId;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            PresentationBaseOrder = presentationBaseOrder;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            HitRecordStart = hitRecordStart;
            HitRecordCount = hitRecordCount;
        }

        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int PresentationBaseOrder { get; }
        public float RenderOffsetX { get; }
        public int CameraX { get; }
        public int HitRecordStart { get; }
        public int HitRecordCount { get; }
    }

    public sealed class BattleHitRecordPresentationCycle
    {
        private BattleHitRecordOwnerSnapshot[] owners = new BattleHitRecordOwnerSnapshot[16];
        private BattlePresentationHitRecordSnapshot[] hitRecords =
            new BattlePresentationHitRecordSnapshot[16];
        private CharacterAnimtorManager bindingManager;
        private BattleSpriteCatalog boundCatalog = BattleSpriteCatalog.Empty;

        public int CycleId { get; private set; }
        public int TickIndex { get; private set; }
        public int OwnerCount { get; private set; }
        public int HitRecordCount { get; private set; }
        public BattleCommonVisualCatalog CommonVisualCatalog { get; private set; } =
            BattleCommonVisualCatalog.Empty;
        public bool HasValidSparkPublication => CommonVisualCatalog.IsSparkValid;

        public BattleHitRecordOwnerSnapshot GetOwner(int index)
        {
            if ((uint)index >= (uint)OwnerCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return owners[index];
        }

        public BattlePresentationHitRecordSnapshot GetHitRecord(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return hitRecords[index];
        }

        internal void Reset(
            int cycleId,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog)
        {
            ReleasePublicationBinding();
            CycleId = cycleId;
            TickIndex = tickIndex;
            OwnerCount = 0;
            HitRecordCount = 0;
            CommonVisualCatalog = commonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
        }

        internal void AddOwner(in BattleHitRecordOwnerSnapshot owner)
        {
            EnsureCapacity(ref owners, OwnerCount + 1);
            owners[OwnerCount++] = owner;
        }

        internal void AddHitRecord(in BattlePresentationHitRecordSnapshot hitRecord)
        {
            EnsureCapacity(ref hitRecords, HitRecordCount + 1);
            hitRecords[HitRecordCount++] = hitRecord;
        }

        internal void PrepareCapacity(int ownerCapacity, int hitRecordCapacity)
        {
            EnsureCapacity(ref owners, ownerCapacity);
            EnsureCapacity(ref hitRecords, hitRecordCapacity);
        }

        internal void RetainPublicationBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog,
            BattleHitRecordPresentationCycle previousCycle)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (manager == null || ReferenceEquals(nextCatalog, BattleSpriteCatalog.Empty))
                return;

            if (previousCycle != null &&
                previousCycle.bindingManager == manager &&
                ReferenceEquals(previousCycle.boundCatalog, nextCatalog))
            {
                bindingManager = previousCycle.bindingManager;
                boundCatalog = previousCycle.boundCatalog;
                previousCycle.bindingManager = null;
                previousCycle.boundCatalog = BattleSpriteCatalog.Empty;
                return;
            }

            manager.RegisterRendererCatalogBinding(nextCatalog);
            bindingManager = manager;
            boundCatalog = nextCatalog;
        }

        internal void ReleasePublicationBinding()
        {
            CharacterAnimtorManager manager = bindingManager;
            BattleSpriteCatalog catalog = boundCatalog;
            bindingManager = null;
            boundCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        private static void EnsureCapacity<T>(ref T[] array, int required)
        {
            if (required <= array.Length)
                return;
            int next = array.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref array, next);
        }
    }

    public readonly struct BattlePresentationEntitySnapshot
    {
        public BattlePresentationEntitySnapshot(
            RuntimeEntityHandle handle,
            int stableId,
            int objectId,
            int currentDatObjectId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int presentationBaseOrder,
            int hitStop,
            bool hasCurrentFrame,
            int state,
            int linkState,
            int hp2Orig,
            int relationTeam,
            int currentDatObjType,
            int xInt,
            int yInt,
            float displayZ,
            float renderOffsetX,
            int cameraX,
            int frameDelay,
            float centerX,
            float centerY,
            float pixelWidth,
            float pixelHeight,
            Vector2 heldVisualAttachmentOffsetPixels,
            Rect normalizedUv,
            Vector2 pivot,
            bool flipX,
            bool hasCatalogKey,
            BattleSpriteValueDescriptor spriteDescriptor,
            int hitRecordStart,
            int hitRecordCount,
            bool entityVisible = true,
            bool shadowVisible = true,
            Vector2 localOffsetPixels = default(Vector2),
            int frameId = -1,
            object trustedResourceIdentity = null)
        {
            Handle = handle;
            StableId = stableId;
            ObjectId = objectId;
            CurrentDatObjectId = currentDatObjectId;
            EffectivePic = effectivePic;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            PresentationBaseOrder = presentationBaseOrder;
            HitStop = hitStop;
            HasCurrentFrame = hasCurrentFrame;
            State = state;
            LinkState = linkState;
            HP2Orig = hp2Orig;
            RelationTeam = relationTeam;
            CurrentDatObjType = currentDatObjType;
            XInt = xInt;
            YInt = yInt;
            DisplayZ = displayZ;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            FrameDelay = frameDelay;
            CenterX = centerX;
            CenterY = centerY;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            HeldVisualAttachmentOffsetPixels = heldVisualAttachmentOffsetPixels;
            NormalizedUv = normalizedUv;
            Pivot = pivot;
            FlipX = flipX;
            HasCatalogKey = hasCatalogKey;
            SpriteDescriptor = spriteDescriptor;
            HitRecordStart = hitRecordStart;
            HitRecordCount = hitRecordCount;
            EntityVisible = entityVisible;
            ShadowVisible = shadowVisible;
            LocalOffsetPixels = localOffsetPixels;
            FrameId = frameId;
            TrustedResourceIdentity = trustedResourceIdentity;
        }

        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int ObjectId { get; }
        public int CurrentDatObjectId { get; }
        public int VisualDataId => CurrentDatObjectId;
        public int EffectivePic { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int PresentationBaseOrder { get; }
        public int HitStop { get; }
        public bool HasCurrentFrame { get; }
        public int State { get; }
        public int LinkState { get; }
        public int HP2Orig { get; }
        public int RelationTeam { get; }
        public int CurrentDatObjType { get; }
        public int XInt { get; }
        public int YInt { get; }
        public float DisplayZ { get; }
        public float RenderOffsetX { get; }
        public int CameraX { get; }
        public int FrameDelay { get; }
        public float CenterX { get; }
        public float CenterY { get; }
        public float PixelWidth { get; }
        public float PixelHeight { get; }
        public Vector2 HeldVisualAttachmentOffsetPixels { get; }
        public Rect NormalizedUv { get; }
        public Vector2 Pivot { get; }
        public bool FlipX { get; }
        public bool HasCatalogKey { get; }
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
        public int HitRecordStart { get; }
        public int HitRecordCount { get; }
        public bool EntityVisible { get; }
        public bool ShadowVisible { get; }
        public Vector2 LocalOffsetPixels { get; }
        public int FrameId { get; }
        internal object TrustedResourceIdentity { get; }

        internal BattlePresentationEntitySnapshot WithResolvedSprite(
            float pixelWidth,
            float pixelHeight,
            Rect normalizedUv,
            Vector2 pivot,
            bool hasCatalogKey,
            BattleSpriteValueDescriptor spriteDescriptor,
            object trustedResourceIdentity)
        {
            return new BattlePresentationEntitySnapshot(
                Handle,
                StableId,
                ObjectId,
                CurrentDatObjectId,
                EffectivePic,
                ZInt,
                RuntimeSlot,
                PresentationBaseOrder,
                HitStop,
                HasCurrentFrame,
                State,
                LinkState,
                HP2Orig,
                RelationTeam,
                CurrentDatObjType,
                XInt,
                YInt,
                DisplayZ,
                RenderOffsetX,
                CameraX,
                FrameDelay,
                CenterX,
                CenterY,
                pixelWidth,
                pixelHeight,
                HeldVisualAttachmentOffsetPixels,
                normalizedUv,
                pivot,
                FlipX,
                hasCatalogKey,
                spriteDescriptor,
                HitRecordStart,
                HitRecordCount,
                EntityVisible,
                ShadowVisible,
                LocalOffsetPixels,
                FrameId,
                trustedResourceIdentity);
        }

        internal BattlePresentationEntitySnapshot WithPresentationBaseOrder(
            int presentationBaseOrder)
        {
            return new BattlePresentationEntitySnapshot(
                Handle,
                StableId,
                ObjectId,
                CurrentDatObjectId,
                EffectivePic,
                ZInt,
                RuntimeSlot,
                presentationBaseOrder,
                HitStop,
                HasCurrentFrame,
                State,
                LinkState,
                HP2Orig,
                RelationTeam,
                CurrentDatObjType,
                XInt,
                YInt,
                DisplayZ,
                RenderOffsetX,
                CameraX,
                FrameDelay,
                CenterX,
                CenterY,
                PixelWidth,
                PixelHeight,
                HeldVisualAttachmentOffsetPixels,
                NormalizedUv,
                Pivot,
                FlipX,
                HasCatalogKey,
                SpriteDescriptor,
                HitRecordStart,
                HitRecordCount,
                EntityVisible,
                ShadowVisible,
                LocalOffsetPixels,
                FrameId,
                TrustedResourceIdentity);
        }

    }

    public readonly struct BattleRenderCommand
    {
        public BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            bool flipX,
            BattleSpriteValueDescriptor spriteDescriptor)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                zInt,
                runtimeSlot,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                pivot,
                normalizedUv,
                BattleSpriteRenderState.Default(flipX),
                spriteDescriptor)
        {
        }

        internal BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            bool flipX,
            BattleSpriteValueDescriptor spriteDescriptor,
            object trustedResourceIdentity)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                zInt,
                runtimeSlot,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                pivot,
                normalizedUv,
                BattleSpriteRenderState.Default(flipX),
                spriteDescriptor,
                trustedResourceIdentity)
        {
        }

        public BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            BattleSpriteRenderState renderState,
            BattleSpriteValueDescriptor spriteDescriptor)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                zInt,
                runtimeSlot,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                pivot,
                normalizedUv,
                renderState,
                spriteDescriptor,
                null)
        {
        }

        internal BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            BattleSpriteRenderState renderState,
            BattleSpriteValueDescriptor spriteDescriptor,
            object trustedResourceIdentity)
        {
            Type = type;
            Handle = handle;
            StableId = stableId;
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            SortOrder = sortOrder;
            SortingLayerId = sortingLayerId;
            LocalSequence = localSequence;
            Position = position;
            Size = size;
            Pivot = pivot;
            NormalizedUv = normalizedUv;
            RenderState = renderState;
            SpriteDescriptor = spriteDescriptor;
            TrustedResourceIdentity = trustedResourceIdentity;
        }

        public BattleRenderCommandType Type { get; }
        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int VisualDataId { get; }
        public int EffectivePic { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int SortOrder { get; }
        public int SortingLayerId { get; }
        public int LocalSequence { get; }
        public Vector3 Position { get; }
        public Vector2 Size { get; }
        public Vector2 Pivot { get; }
        public Rect NormalizedUv { get; }
        public BattleSpriteRenderState RenderState { get; }
        public Color32 Color => RenderState.Color;
        public bool FlipX => RenderState.FlipX;
        public bool FlipY => RenderState.FlipY;
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
        internal object TrustedResourceIdentity { get; }
    }

    public sealed class BattlePresentationFrame
    {
        private static readonly ProfilerMarker FrozenFrameCopyMarker =
            new ProfilerMarker("NTSD.BattlePresentation.FrozenFrameCopy");
        private BattlePresentationEntitySnapshot[] entities = new BattlePresentationEntitySnapshot[16];
        private BattlePresentationHitRecordSnapshot[] hitRecords = new BattlePresentationHitRecordSnapshot[16];
        private BattleRenderCommand[] commands = new BattleRenderCommand[64];
        private readonly char[,] slotLabelChars = new char[10, 12];
        private readonly int[] slotLabelState = new int[10];
        private CharacterAnimtorManager bindingManager;
        private BattleSpriteCatalog boundCatalog = BattleSpriteCatalog.Empty;

        public int TickIndex { get; internal set; }
        public int EntityCount { get; internal set; }
        public int HitRecordCount { get; internal set; }
        public int CommandCount { get; internal set; }
        public bool PresentationOrderMaterialized { get; internal set; }
        public bool CommandsMaterialized { get; internal set; }
        public int OverlayUnsupportedCount { get; internal set; }
        internal bool RequiresCatalogPublicationBinding { get; set; }
        public BattleCommonVisualCatalog CommonVisualCatalog { get; internal set; } =
            BattleCommonVisualCatalog.Empty;
        public BattleCommonVisualBinding CommonShadowBinding { get; internal set; }
        public string CommonShadowDiagnostic { get; internal set; } = string.Empty;
        public int EntityCapacity => entities.Length;
        public int HitRecordCapacity => hitRecords.Length;
        public int CommandCapacity => commands.Length;
        internal char[,] SlotLabelChars => slotLabelChars;
        internal int[] SlotLabelState => slotLabelState;
        public BattleSpriteCatalog BoundCatalogForAcceptance => boundCatalog;
        internal BattleSpriteCatalog BoundCatalog => boundCatalog;

        public BattlePresentationEntitySnapshot GetEntity(int index)
        {
            if ((uint)index >= (uint)EntityCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return entities[index];
        }

        internal ref readonly BattlePresentationEntitySnapshot GetEntityRef(int index)
        {
            if ((uint)index >= (uint)EntityCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref entities[index];
        }

        public BattlePresentationHitRecordSnapshot GetHitRecord(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return hitRecords[index];
        }

        internal ref readonly BattlePresentationHitRecordSnapshot GetHitRecordRef(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref hitRecords[index];
        }

        public BattleRenderCommand GetCommand(int index)
        {
            if ((uint)index >= (uint)CommandCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return commands[index];
        }

        internal ref readonly BattleRenderCommand GetCommandRef(int index)
        {
            if ((uint)index >= (uint)CommandCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return ref commands[index];
        }

        internal void CopyFrom(
            BattlePresentationFrame source,
            BattleTickDetailPhaseDiagnostics detailDiagnostics = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (ReferenceEquals(this, source))
                return;

            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy);
            try
            {
                using (FrozenFrameCopyMarker.Auto())
                {
                    ReleasePublicationBinding();
                    EnsureEntityCapacity(source.EntityCount);
                    EnsureHitRecordCapacity(source.HitRecordCount);
                    EnsureCommandCapacity(source.CommandCount);
                    Array.Copy(source.entities, entities, source.EntityCount);
                    Array.Copy(source.hitRecords, hitRecords, source.HitRecordCount);
                    Array.Copy(source.commands, commands, source.CommandCount);
                    Array.Copy(source.slotLabelChars, slotLabelChars, source.slotLabelChars.Length);
                    Array.Copy(source.slotLabelState, slotLabelState, source.slotLabelState.Length);

                    TickIndex = source.TickIndex;
                    EntityCount = source.EntityCount;
                    HitRecordCount = source.HitRecordCount;
                    CommandCount = source.CommandCount;
                    PresentationOrderMaterialized = source.PresentationOrderMaterialized;
                    CommandsMaterialized = source.CommandsMaterialized;
                    OverlayUnsupportedCount = source.OverlayUnsupportedCount;
                    RequiresCatalogPublicationBinding =
                        source.RequiresCatalogPublicationBinding;
                    CommonVisualCatalog = source.CommonVisualCatalog;
                    CommonShadowBinding = source.CommonShadowBinding;
                    CommonShadowDiagnostic = source.CommonShadowDiagnostic;
                    // Submission catalog binding owns resource lifetime for frozen copies.
                    boundCatalog = source.boundCatalog;
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy);
            }
        }

        internal void Reset(
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog = null)
        {
            ReleasePublicationBinding();
            TickIndex = tickIndex;
            EntityCount = 0;
            HitRecordCount = 0;
            CommandCount = 0;
            PresentationOrderMaterialized = false;
            CommandsMaterialized = false;
            OverlayUnsupportedCount = 0;
            RequiresCatalogPublicationBinding = false;
            Array.Clear(slotLabelChars, 0, slotLabelChars.Length);
            Array.Clear(slotLabelState, 0, slotLabelState.Length);
            CommonVisualCatalog = commonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
            CommonShadowBinding = commonVisualCatalog?.Shadow;
            CommonShadowDiagnostic = commonVisualCatalog?.Diagnostic ??
                                     BattleCommonVisualCatalog.Empty.Diagnostic;
        }

        internal void RetainPublicationBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog,
            BattlePresentationFrame previousFrame)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (manager == null || ReferenceEquals(nextCatalog, BattleSpriteCatalog.Empty))
                return;

            if (previousFrame != null &&
                previousFrame.bindingManager == manager &&
                ReferenceEquals(previousFrame.boundCatalog, nextCatalog))
            {
                bindingManager = previousFrame.bindingManager;
                boundCatalog = previousFrame.boundCatalog;
                previousFrame.bindingManager = null;
                previousFrame.boundCatalog = BattleSpriteCatalog.Empty;
                return;
            }

            manager.RegisterRendererCatalogBinding(nextCatalog);
            bindingManager = manager;
            boundCatalog = nextCatalog;
        }

        internal void ReleasePublicationBinding()
        {
            CharacterAnimtorManager manager = bindingManager;
            BattleSpriteCatalog catalog = boundCatalog;
            bindingManager = null;
            boundCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        internal void EnsureEntityCapacity(int required) => EnsureCapacity(ref entities, required);
        internal void EnsureHitRecordCapacity(int required) => EnsureCapacity(ref hitRecords, required);
        internal void EnsureCommandCapacity(int required) => EnsureCapacity(ref commands, required);

        internal void PrepareCapacity(
            int entityCapacity,
            int hitRecordCapacity,
            int commandCapacity)
        {
            EnsureEntityCapacity(entityCapacity);
            EnsureHitRecordCapacity(hitRecordCapacity);
            EnsureCommandCapacity(commandCapacity);
        }

        internal void AddEntity(in BattlePresentationEntitySnapshot entity)
        {
            EnsureEntityCapacity(EntityCount + 1);
            entities[EntityCount++] = entity;
        }

        internal void SetEntity(
            int index,
            in BattlePresentationEntitySnapshot entity)
        {
            if ((uint)index >= (uint)EntityCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            entities[index] = entity;
        }

        internal void SortEntities(
            IComparer<BattlePresentationEntitySnapshot> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            if (EntityCount > 1)
                Array.Sort(entities, 0, EntityCount, comparer);
        }

        internal void AddHitRecord(in BattlePresentationHitRecordSnapshot hitRecord)
        {
            EnsureHitRecordCapacity(HitRecordCount + 1);
            hitRecords[HitRecordCount++] = hitRecord;
        }

        internal void AddCommand(in BattleRenderCommand command)
        {
            EnsureCommandCapacity(CommandCount + 1);
            commands[CommandCount++] = command;
            if (CommandRequiresCatalogPublicationBinding(in command))
                RequiresCatalogPublicationBinding = true;
        }

        internal CommandWriter BeginCommandWrite(int maximumCommandCount)
        {
            if (maximumCommandCount < CommandCount)
                throw new ArgumentOutOfRangeException(nameof(maximumCommandCount));

            EnsureCommandCapacity(maximumCommandCount);
            return new CommandWriter(this, commands, CommandCount);
        }

        internal struct CommandWriter
        {
            private readonly BattlePresentationFrame owner;
            private readonly BattleRenderCommand[] destination;
            private int count;
            private bool requiresCatalogPublicationBinding;

            internal CommandWriter(
                BattlePresentationFrame owner,
                BattleRenderCommand[] destination,
                int count)
            {
                this.owner = owner;
                this.destination = destination;
                this.count = count;
                requiresCatalogPublicationBinding =
                    owner.RequiresCatalogPublicationBinding;
            }

            internal void AddUnchecked(in BattleRenderCommand command)
            {
                destination[count++] = command;
                if (CommandRequiresCatalogPublicationBinding(in command))
                    requiresCatalogPublicationBinding = true;
            }

            internal void Commit()
            {
                owner.CommandCount = count;
                owner.RequiresCatalogPublicationBinding =
                    requiresCatalogPublicationBinding;
            }
        }

        private static bool CommandRequiresCatalogPublicationBinding(
            in BattleRenderCommand command)
        {
            return command.SpriteDescriptor.HasLogicalResourceKey &&
                   command.SpriteDescriptor.HasSprite;
        }

        private static void EnsureCapacity<T>(ref T[] array, int required)
        {
            if (required <= array.Length)
                return;

            int next = array.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref array, next);
        }
    }

    public readonly struct LegacyPresentationProbe
    {
        public LegacyPresentationProbe(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            bool flipX,
            BattleSpriteValueDescriptor spriteDescriptor)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                BattleSpriteRenderState.Default(flipX),
                spriteDescriptor)
        {
        }

        public LegacyPresentationProbe(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            BattleSpriteRenderState renderState,
            BattleSpriteValueDescriptor spriteDescriptor)
        {
            Type = type;
            Handle = handle;
            StableId = stableId;
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
            SortOrder = sortOrder;
            SortingLayerId = sortingLayerId;
            LocalSequence = localSequence;
            Position = position;
            Size = size;
            RenderState = renderState;
            SpriteDescriptor = spriteDescriptor;
        }

        public BattleRenderCommandType Type { get; }
        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int VisualDataId { get; }
        public int EffectivePic { get; }
        public int SortOrder { get; }
        public int SortingLayerId { get; }
        public int LocalSequence { get; }
        public Vector3 Position { get; }
        public Vector2 Size { get; }
        public BattleSpriteRenderState RenderState { get; }
        public Color32 Color => RenderState.Color;
        public bool FlipX => RenderState.FlipX;
        public bool FlipY => RenderState.FlipY;
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
    }

    public sealed class BattlePresentationParityDiagnostics
    {
        public BattlePresentationParityStatus Status { get; internal set; }
        public int TickIndex { get; internal set; }
        public int ExpectedCount { get; internal set; }
        public int ActualCount { get; internal set; }
        public int DifferenceCount { get; internal set; }
        public int FirstDifferenceIndex { get; internal set; } = -1;
        public BattlePresentationDifferenceKind FirstDifferenceKind { get; internal set; }
        public BattleOverlayParityState OverlayState { get; internal set; }
        public int OverlayUnsupportedCount { get; internal set; }
        public int IncompleteLegacyFrameCount { get; internal set; }
        public int FirstIncompleteLegacyTick { get; internal set; } = -1;
        public int LastIncompleteLegacyTick { get; internal set; } = -1;
        public int CompletedLegacyFrameCount { get; internal set; }
        public bool HasFirstExpectedCommand { get; internal set; }
        public BattleRenderCommand FirstExpectedCommand { get; internal set; }
        public bool HasFirstActualProbe { get; internal set; }
        public LegacyPresentationProbe FirstActualProbe { get; internal set; }
    }

    public sealed class BattlePresentationCoordinator
    {
        private const int MaximumCommandsPerEntityWithoutHitRecords =
            2 + BattleEntityOverlayLayout.MaximumGlyphCount;
        private const int WordGlyphTemplateCount =
            BattleCommonVisualCatalog.WordSheetCount *
            BattleCommonVisualCatalog.WordGlyphsPerSheet;
        private static readonly int ObjectSortingLayerId = SortingLayer.NameToID("Object");
        private static readonly ProfilerMarker SortEntitiesMarker =
            new ProfilerMarker("NTSD.BattlePresentation.SortEntities");
        private static readonly ProfilerMarker CaptureHitRecordsMarker =
            new ProfilerMarker("NTSD.BattlePresentation.CaptureHitRecords");
        private static readonly ProfilerMarker CaptureEntitiesMarker =
            new ProfilerMarker("NTSD.BattlePresentation.CaptureEntities");
        private static readonly ProfilerMarker BuildCommandsMarker =
            new ProfilerMarker("NTSD.BattlePresentation.BuildCommands");
        private static readonly IComparer<BattlePresentationEntitySnapshot>
            PresentationSnapshotComparer =
                Comparer<BattlePresentationEntitySnapshot>.Create(
                    ComparePresentationSnapshots);
        private readonly BattleSpriteMaterialClassifier materialClassifier =
            new BattleSpriteMaterialClassifier();
        private readonly BattlePresentationFrame frameA = new BattlePresentationFrame();
        private readonly BattlePresentationFrame frameB = new BattlePresentationFrame();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleA =
            new BattleHitRecordPresentationCycle();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleB =
            new BattleHitRecordPresentationCycle();
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(128);
        private LF2Entity[] entitySortSource = new LF2Entity[128];
        private LF2Entity[] entitySortDestination = new LF2Entity[128];
        private uint[] entitySortKeySource = new uint[128];
        private uint[] entitySortKeyDestination = new uint[128];
        private RuntimeEntityHandle[] entityHandleCache =
            new RuntimeEntityHandle[128];
        private int[] entityHandleCacheEpochs = new int[128];
        private readonly int[] entitySortBuckets = new int[256];
        private readonly BattleEntityOverlayGlyph[] overlayGlyphScratch =
            new BattleEntityOverlayGlyph[32];
        private readonly Dictionary<BattleSpriteKey, SpriteCaptureCacheEntry>
            spriteCaptureCache =
                new Dictionary<BattleSpriteKey, SpriteCaptureCacheEntry>(64);
        private readonly WordGlyphCommandTemplate[] wordGlyphCommandTemplates =
            new WordGlyphCommandTemplate[WordGlyphTemplateCount];
        private readonly int[] wordGlyphTemplateEpochs = new int[WordGlyphTemplateCount];
        private readonly WordGlyphCommandTemplate[] comLabelCommandTemplates =
            new WordGlyphCommandTemplate[BattleCommonVisualCatalog.WordSheetCount];
        private readonly bool[] hasComLabelCommandTemplate =
            new bool[BattleCommonVisualCatalog.WordSheetCount];
        private LegacyPresentationProbe[] legacyProbes = new LegacyPresentationProbe[64];
        private BattleCommonVisualCatalog wordGlyphTemplateCatalog;
        private int wordGlyphTemplateEpoch = 1;
        private BattlePresentationFrame publishedFrame;
        private BattleHitRecordPresentationCycle publishedHitRecordCycle;
        private BattlePresentationBackendMode mode;
        private int nextHitRecordCycleId;
        private int finalizedHitRecordCycleId;
        private int legacyProbeCount;
        private int probeSequence;
        private int entityHandleCacheEpoch = 1;
        private bool awaitingLegacyCompletion;

        private readonly struct SpriteCaptureCacheEntry
        {
            public SpriteCaptureCacheEntry(
                bool hasCatalogKey,
                BattleSpriteEntry entry,
                in BattleSpriteValueDescriptor descriptor)
            {
                HasCatalogKey = hasCatalogKey;
                Entry = entry;
                Descriptor = descriptor;
            }

            public bool HasCatalogKey { get; }
            public BattleSpriteEntry Entry { get; }
            public BattleSpriteValueDescriptor Descriptor { get; }
        }

        public BattlePresentationCoordinator()
        {
            mode = BattlePresentationBackendMode.LegacyOnly;
            Diagnostics = new BattlePresentationParityDiagnostics();
        }

        public BattlePresentationBackendMode Mode => mode;
        public BattlePresentationFrame PublishedFrame => Volatile.Read(ref publishedFrame);
        public BattleHitRecordPresentationCycle PublishedHitRecordCycle =>
            Volatile.Read(ref publishedHitRecordCycle);
        public BattlePresentationParityDiagnostics Diagnostics { get; }
        public bool IsCapturingLegacyProbes => awaitingLegacyCompletion;
        internal int LastHitRecordOwnerLookupCount { get; private set; }

        public void PrepareCapacity(int entityCapacity)
        {
            if (entityCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(entityCapacity));

            int hitRecordCapacity = checked(
                entityCapacity * LF2Entity.MaxHitRecordSlots);
            int commandCapacity = CalculateMaximumCommandCapacity(entityCapacity);

            if (entityScratch.Capacity < entityCapacity)
                entityScratch.Capacity = entityCapacity;
            EnsureEntitySortCapacity(entityCapacity);
            EnsureEntityHandleCacheCapacity(entityCapacity);
            spriteCaptureCache.EnsureCapacity(entityCapacity);

            frameA.PrepareCapacity(
                entityCapacity,
                hitRecordCapacity,
                commandCapacity);
            frameB.PrepareCapacity(
                entityCapacity,
                hitRecordCapacity,
                commandCapacity);
            hitRecordCycleA.PrepareCapacity(entityCapacity, hitRecordCapacity);
            hitRecordCycleB.PrepareCapacity(entityCapacity, hitRecordCapacity);
        }

        internal static int CalculateMaximumCommandCapacity(int entityCapacity)
        {
            if (entityCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(entityCapacity));

            return checked(
                entityCapacity * MaximumCommandsPerEntityWithoutHitRecords +
                entityCapacity * LF2Entity.MaxHitRecordSlots);
        }

        public void SetMode(BattlePresentationBackendMode value)
        {
            BattlePresentationBackendResolver.ValidateAvailable(value);
            mode = value;
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                if (awaitingLegacyCompletion)
                    RecordIncompleteLegacyFrame();
                awaitingLegacyCompletion = false;
                legacyProbeCount = 0;
            }
        }

        public void BeginFrame(SimulationWorld world, int tickIndex)
        {
            AdvanceEntityHandleCacheEpoch();
            if (world == null)
                return;

            entityScratch.Clear();
            try
            {
                BattleTickDetailPhaseDiagnostics detailDiagnostics =
                    world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                BattlePresentationPhaseDiagnostics presentationDiagnostics =
                    world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderBeginFrameSortEntities);
                if (mode != BattlePresentationBackendMode.CentralOnly)
                {
                    presentationDiagnostics?.BeginPhase(
                        BattlePresentationPhase.BeginFrameSortEntities);
                }
                try
                {
                    using (SortEntitiesMarker.Auto())
                    {
                        world.GetPresentationEntitiesNoAlloc(entityScratch);
                        if (mode != BattlePresentationBackendMode.CentralOnly)
                            SortEntitiesByZPreservingSlotOrder(entityScratch);
                        EnsureEntityHandleCacheCapacity(entityScratch.Count);
                        if (mode != BattlePresentationBackendMode.CentralOnly)
                            world.RecordPresentationEntityScanAndSortForDiagnostics();
                    }
                }
                finally
                {
                    if (mode != BattlePresentationBackendMode.CentralOnly)
                    {
                        presentationDiagnostics?.EndPhase(
                            BattlePresentationPhase.BeginFrameSortEntities);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderBeginFrameSortEntities);
                }

                CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
                BattleCommonVisualCatalog commonVisualCatalog =
                    manager?.CommonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
                BattleHitRecordPresentationCycle previousCycle = PublishedHitRecordCycle;
                BattleHitRecordPresentationCycle writeCycle =
                    ReferenceEquals(previousCycle, hitRecordCycleA)
                        ? hitRecordCycleB
                        : hitRecordCycleA;
                int cycleId = nextHitRecordCycleId == int.MaxValue ? 1 : nextHitRecordCycleId + 1;
                nextHitRecordCycleId = cycleId;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderBeginFrameCaptureHitRecords);
                presentationDiagnostics?.BeginPhase(
                    BattlePresentationPhase.BeginFrameCaptureHitRecords);
                try
                {
                    using (CaptureHitRecordsMarker.Auto())
                    {
                        CaptureHitRecordCycle(
                            world,
                            entityScratch,
                            tickIndex,
                            cycleId,
                            commonVisualCatalog,
                            writeCycle);
                    }
                }
                finally
                {
                    presentationDiagnostics?.EndPhase(
                        BattlePresentationPhase.BeginFrameCaptureHitRecords);
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderBeginFrameCaptureHitRecords);
                }
                if (writeCycle.HitRecordCount > 0 && commonVisualCatalog.IsSparkValid)
                {
                    writeCycle.RetainPublicationBinding(
                        manager,
                        manager?.SpriteCatalog,
                        previousCycle);
                }
                Interlocked.Exchange(ref publishedHitRecordCycle, writeCycle);
                previousCycle?.ReleasePublicationBinding();

                // Legacy overlays consume the same immutable command snapshot, but the
                // central renderer still refuses to build or submit geometry in this mode.
                if (mode == BattlePresentationBackendMode.LegacyOnly)
                {
                    CaptureBuildAndPublishFrame(
                        world,
                        entityScratch,
                        tickIndex,
                        commonVisualCatalog,
                        writeCycle,
                        manager);
                    return;
                }

                if (mode == BattlePresentationBackendMode.CentralOnly)
                {
                    CaptureBuildAndPublishFrame(
                        world,
                        entityScratch,
                        tickIndex,
                        commonVisualCatalog,
                        writeCycle,
                        manager,
                        buildCommands: false);
                    awaitingLegacyCompletion = false;
                    legacyProbeCount = 0;
                    return;
                }

                if (mode != BattlePresentationBackendMode.CentralShadowBuild)
                    return;

                if (awaitingLegacyCompletion)
                    RecordIncompleteLegacyFrame();

                CaptureBuildAndPublishFrame(
                    world,
                    entityScratch,
                    tickIndex,
                    commonVisualCatalog,
                    writeCycle,
                    manager);
                legacyProbeCount = 0;
                probeSequence = 0;
                awaitingLegacyCompletion = true;
                Diagnostics.Status = BattlePresentationParityStatus.PendingLegacyFrame;
                Diagnostics.TickIndex = tickIndex;
            }
            finally
            {
                entityScratch.Clear();
            }
        }

        public bool FinalizePublishedHitRecordCycle(SimulationWorld world)
        {
            BattleHitRecordPresentationCycle cycle = PublishedHitRecordCycle;
            if (world == null || cycle == null || cycle.CycleId == finalizedHitRecordCycleId)
                return false;

            finalizedHitRecordCycleId = cycle.CycleId;
            if (!cycle.HasValidSparkPublication)
                return false;

            bool changed = false;
            try
            {
                for (int ownerIndex = 0; ownerIndex < cycle.OwnerCount; ownerIndex++)
                {
                    BattleHitRecordOwnerSnapshot owner = cycle.GetOwner(ownerIndex);
                    if (!world.TryResolveRuntimeHandle(owner.Handle, out LF2Entity entity) ||
                        entity == null || entity.HitRecordCount != owner.HitRecordCount)
                    {
                        continue;
                    }

                    bool sampleMatches = true;
                    for (int hitIndex = 0; hitIndex < owner.HitRecordCount; hitIndex++)
                    {
                        BattlePresentationHitRecordSnapshot hit = cycle.GetHitRecord(
                            owner.HitRecordStart + hitIndex);
                        if (entity.GetHitRecordAge(hitIndex) != hit.Age)
                        {
                            sampleMatches = false;
                            break;
                        }
                    }
                    if (!sampleMatches)
                        continue;

                    for (int hitIndex = 0; hitIndex < owner.HitRecordCount; hitIndex++)
                    {
                        BattlePresentationHitRecordSnapshot hit = cycle.GetHitRecord(
                            owner.HitRecordStart + hitIndex);
                        if (BattleCommonVisualCatalog.TryResolveSparkAge(hit.Age, out _))
                        {
                            entity.AdvanceHitRecordFromPresentation(hitIndex, hit.Age);
                            changed = true;
                        }
                        else if (hitIndex == owner.HitRecordCount - 1)
                        {
                            changed |= entity.RemoveHitRecordTailFromPresentation(
                                hitIndex,
                                owner.HitRecordCount,
                                hit.Age);
                        }
                    }
                }
            }
            finally
            {
                cycle.ReleasePublicationBinding();
            }

            return changed;
        }

        public void ReleaseResources()
        {
            frameA.ReleasePublicationBinding();
            frameB.ReleasePublicationBinding();
            hitRecordCycleA.ReleasePublicationBinding();
            hitRecordCycleB.ReleasePublicationBinding();
        }

        public void Reset()
        {
            ReleaseResources();
            Interlocked.Exchange(ref publishedFrame, null);
            Interlocked.Exchange(ref publishedHitRecordCycle, null);
            entityScratch.Clear();
            nextHitRecordCycleId = 0;
            finalizedHitRecordCycleId = 0;
            legacyProbeCount = 0;
            probeSequence = 0;
            awaitingLegacyCompletion = false;
            LastHitRecordOwnerLookupCount = 0;
            AdvanceEntityHandleCacheEpoch();
            Diagnostics.Status = BattlePresentationParityStatus.None;
            Diagnostics.TickIndex = 0;
            Diagnostics.ExpectedCount = 0;
            Diagnostics.ActualCount = 0;
            Diagnostics.DifferenceCount = 0;
            Diagnostics.FirstDifferenceIndex = -1;
            Diagnostics.FirstDifferenceKind = BattlePresentationDifferenceKind.None;
            Diagnostics.OverlayState = BattleOverlayParityState.None;
            Diagnostics.OverlayUnsupportedCount = 0;
            Diagnostics.IncompleteLegacyFrameCount = 0;
            Diagnostics.FirstIncompleteLegacyTick = -1;
            Diagnostics.LastIncompleteLegacyTick = -1;
            Diagnostics.CompletedLegacyFrameCount = 0;
            Diagnostics.HasFirstExpectedCommand = false;
            Diagnostics.FirstExpectedCommand = default;
            Diagnostics.HasFirstActualProbe = false;
            Diagnostics.FirstActualProbe = default;
        }

        public void CompleteLegacyFrame()
        {
            if (!awaitingLegacyCompletion)
                return;

            awaitingLegacyCompletion = false;
            ComparePublishedFrameToLegacyProbes();
            Diagnostics.Status = BattlePresentationParityStatus.Complete;
            Diagnostics.CompletedLegacyFrameCount++;
        }

        private void RecordIncompleteLegacyFrame()
        {
            int incompleteTick = PublishedFrame?.TickIndex ?? Diagnostics.TickIndex;
            Diagnostics.Status = BattlePresentationParityStatus.IncompleteLegacyFrame;
            Diagnostics.IncompleteLegacyFrameCount++;
            if (Diagnostics.FirstIncompleteLegacyTick < 0)
                Diagnostics.FirstIncompleteLegacyTick = incompleteTick;
            Diagnostics.LastIncompleteLegacyTick = incompleteTick;
            awaitingLegacyCompletion = false;
            legacyProbeCount = 0;
        }

        internal void RecordLegacyProbe(in LegacyPresentationProbe probe)
        {
            if (!awaitingLegacyCompletion)
                return;

            EnsureLegacyProbeCapacity(legacyProbeCount + 1);
            legacyProbes[legacyProbeCount++] = new LegacyPresentationProbe(
                probe.Type,
                probe.Handle,
                probe.StableId,
                probe.VisualDataId,
                probe.EffectivePic,
                probe.SortOrder,
                probe.SortingLayerId,
                probeSequence++,
                probe.Position,
                probe.Size,
                probe.RenderState,
                probe.SpriteDescriptor);
        }

        internal void RecordLegacyHitRecordProbe(
            in BattleHitRecordOwnerSnapshot owner,
            SpriteRenderer renderer,
            int hitRecordIndex,
            BattleCommonVisualBinding binding)
        {
            if (!awaitingLegacyCompletion || renderer == null || !renderer.enabled)
                return;

            Sprite sprite = renderer.sprite;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = sprite != null && rect.width > 0f && rect.height > 0f
                ? new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height)
                : Vector2.zero;
            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer.sharedMaterial;
            var renderState = new BattleSpriteRenderState(
                renderer.color,
                renderer.flipX,
                renderer.flipY,
                renderer.maskInteraction,
                materialClassifier.Classify(material));
            bool matchesPublishedBinding = binding?.MatchesSprite(sprite) == true;
            BattleSpriteValueDescriptor descriptor = matchesPublishedBinding
                ? new BattleSpriteValueDescriptor(
                    true,
                    true,
                    sprite.GetInstanceID(),
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    binding.Key)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
            RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.HitRecord,
                owner.Handle,
                owner.StableId,
                -1,
                -1,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                hitRecordIndex,
                renderer.transform.position,
                sprite != null ? sprite.rect.size : Vector2.zero,
                renderState,
                descriptor));
        }

        internal void RecordLegacyOverlayProbe(
            in BattleRenderCommand command,
            SpriteRenderer renderer,
            BattleCommonVisualBinding binding)
        {
            if (!awaitingLegacyCompletion || renderer == null || !renderer.enabled)
                return;

            Sprite sprite = renderer.sprite;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = sprite != null && rect.width > 0f && rect.height > 0f
                ? new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height)
                : Vector2.zero;
            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer.sharedMaterial;
            var renderState = new BattleSpriteRenderState(
                renderer.color,
                renderer.flipX,
                renderer.flipY,
                renderer.maskInteraction,
                materialClassifier.Classify(material));
            bool matchesPublishedBinding = binding != null &&
                                         binding.Key == command.SpriteDescriptor.LogicalResourceKey &&
                                         binding.MatchesSprite(sprite);
            BattleSpriteValueDescriptor descriptor = matchesPublishedBinding
                ? new BattleSpriteValueDescriptor(
                    true,
                    true,
                    sprite.GetInstanceID(),
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    binding.Key)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
            RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.OverlayGlyph,
                command.Handle,
                command.StableId,
                command.VisualDataId,
                command.EffectivePic,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                command.LocalSequence,
                renderer.transform.position,
                sprite != null ? sprite.rect.size : Vector2.zero,
                renderState,
                descriptor));
        }

        internal void ResetLegacyProbesForSelfCheck()
        {
            if (!awaitingLegacyCompletion)
                return;
            legacyProbeCount = 0;
            probeSequence = 0;
        }

        private void CaptureHitRecordCycle(
            SimulationWorld world,
            List<LF2Entity> sortedEntities,
            int tickIndex,
            int cycleId,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle cycle)
        {
            cycle.Reset(cycleId, tickIndex, commonVisualCatalog);
            for (int index = 0; index < sortedEntities.Count; index++)
            {
                LF2Entity entity = sortedEntities[index];
                NTSDEntityRuntime runtime = entity?.Runtime;
                int slot = runtime?.SlotIndex ?? -1;
                if (entity == null || runtime == null || slot < 0 ||
                    runtime.OidMergeDormant || runtime.PendingFlushDestroy ||
                    tickIndex < runtime.FirstPresentationTick ||
                    !world.TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    continue;
                }

                entityHandleCache[index] = handle;
                entityHandleCacheEpochs[index] = entityHandleCacheEpoch;

                int sampledCount = Math.Min(entity.HitRecordCount, LF2Entity.MaxHitRecordSlots);
                if (sampledCount <= 0)
                    continue;
                int hitRecordStart = cycle.HitRecordCount;
                for (int hitIndex = 0; hitIndex < sampledCount; hitIndex++)
                {
                    cycle.AddHitRecord(new BattlePresentationHitRecordSnapshot(
                        entity.GetHitRecordAge(hitIndex),
                        entity.GetHitRecordX(hitIndex),
                        entity.GetHitRecordZ(hitIndex)));
                }
                cycle.AddOwner(new BattleHitRecordOwnerSnapshot(
                    handle,
                    runtime.StableId,
                    runtime.ZInt,
                    slot,
                    mode == BattlePresentationBackendMode.CentralOnly
                        ? checked(index * 4)
                        : entity.GetRenderSortingOrder() -
                          SimulationWorld.PresentationEntitySubOrder,
                    entity.GetRenderOffsetX(),
                    world.ReleaseCameraX,
                    hitRecordStart,
                    sampledCount));
            }
        }

        private void CaptureBuildAndPublishFrame(
            SimulationWorld world,
            List<LF2Entity> sortedEntities,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle hitRecordCycle,
            CharacterAnimtorManager manager,
            bool buildCommands = true)
        {
            BattlePresentationFrame previousFrame = PublishedFrame;
            BattlePresentationFrame writeFrame = ReferenceEquals(previousFrame, frameA) ? frameB : frameA;
            CaptureAndBuild(
                world,
                sortedEntities,
                tickIndex,
                commonVisualCatalog,
                hitRecordCycle,
                writeFrame,
                manager,
                buildCommands);
            if (!buildCommands && writeFrame.EntityCount > 0 &&
                manager != null &&
                !ReferenceEquals(manager.SpriteCatalog, BattleSpriteCatalog.Empty))
            {
                // The logic publication stores only logical sprite keys. Retain the
                // catalog generation so the presentation host can resolve those keys
                // after the tick without observing a newer loading generation.
                writeFrame.RequiresCatalogPublicationBinding = true;
            }
            BattlePresentationPhaseDiagnostics presentationDiagnostics =
                world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
            presentationDiagnostics?.BeginPhase(
                BattlePresentationPhase.RequiresPublicationBinding);
            try
            {
                if (writeFrame.RequiresCatalogPublicationBinding)
                {
                    writeFrame.RetainPublicationBinding(
                        manager,
                        manager?.SpriteCatalog,
                        previousFrame);
                }
            }
            finally
            {
                presentationDiagnostics?.EndPhase(
                    BattlePresentationPhase.RequiresPublicationBinding);
            }

            presentationDiagnostics?.BeginPhase(
                BattlePresentationPhase.PublishSwapAndRelease);
            try
            {
                Interlocked.Exchange(ref publishedFrame, writeFrame);
                previousFrame?.ReleasePublicationBinding();
            }
            finally
            {
                presentationDiagnostics?.EndPhase(
                    BattlePresentationPhase.PublishSwapAndRelease);
            }
        }

        private void CaptureAndBuild(
            SimulationWorld world,
            List<LF2Entity> sortedEntities,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle hitRecordCycle,
            BattlePresentationFrame frame,
            CharacterAnimtorManager manager,
            bool buildCommands)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            BattlePresentationPhaseDiagnostics presentationDiagnostics =
                world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderBeginFrameCaptureEntities);
            presentationDiagnostics?.BeginPhase(
                BattlePresentationPhase.BeginFrameCaptureEntities);
            try
            {
                using (CaptureEntitiesMarker.Auto())
                {
                    spriteCaptureCache.Clear();
                    frame.Reset(tickIndex, commonVisualCatalog);
                    Array.Copy(
                        world.Runtime.SlotLabels.BattleSlotLabels,
                        frame.SlotLabelChars,
                        frame.SlotLabelChars.Length);
                    Array.Copy(
                        world.Runtime.SlotLabels.BattleSlotLabelState,
                        frame.SlotLabelState,
                        frame.SlotLabelState.Length);
                    frame.EnsureEntityCapacity(sortedEntities.Count);
                    int hitRecordOwnerCursor = 0;
                    LastHitRecordOwnerLookupCount = 0;

                    for (int i = 0; i < sortedEntities.Count; i++)
                    {
                        LF2Entity entity = sortedEntities[i];
                        NTSDEntityRuntime runtime = entity?.Runtime;
                        int slot = runtime?.SlotIndex ?? -1;
                        if (entity == null || runtime == null || slot < 0 ||
                            runtime.OidMergeDormant || runtime.PendingFlushDestroy ||
                            tickIndex < runtime.FirstPresentationTick ||
                            entityHandleCacheEpochs[i] != entityHandleCacheEpoch)
                        {
                            continue;
                        }

                        RuntimeEntityHandle handle = entityHandleCache[i];

                        LF2FrameData currentFrame = entity.Frame?.D;
                        int visualDataId = LF2Entity.ResolveCurrentDataObjectId(entity);
                        int effectivePic = entity.GetRenderPicIndex();
                        BattleSpriteEntry entry = null;
                        bool hasCatalogKey = false;
                        BattleSpriteValueDescriptor spriteDescriptor = default;
                        if (buildCommands)
                        {
                            ResolveSpriteCapture(
                                manager,
                                visualDataId,
                                effectivePic,
                                out entry,
                                out hasCatalogKey,
                                out spriteDescriptor);
                        }
                        int currentDataType = entity.GetCurrentDataObjectTypeForSimulation();
                        int hitRecordStart = frame.HitRecordCount;
                        int sourceHitRecordCount = 0;
                        BattleHitRecordOwnerSnapshot hitRecordOwner = default;
                        if (hitRecordOwnerCursor < hitRecordCycle.OwnerCount)
                        {
                            LastHitRecordOwnerLookupCount++;
                            BattleHitRecordOwnerSnapshot candidate =
                                hitRecordCycle.GetOwner(hitRecordOwnerCursor);
                            if (candidate.Handle.Equals(handle))
                            {
                                hitRecordOwner = candidate;
                                sourceHitRecordCount = candidate.HitRecordCount;
                                hitRecordOwnerCursor++;
                            }
                        }
                        frame.EnsureHitRecordCapacity(frame.HitRecordCount + sourceHitRecordCount);
                        for (int hitIndex = 0; hitIndex < sourceHitRecordCount; hitIndex++)
                        {
                            frame.AddHitRecord(hitRecordCycle.GetHitRecord(
                                hitRecordOwner.HitRecordStart + hitIndex));
                        }

                        int holderSlot = runtime.HolderStableId;
                        LF2Entity holder = holderSlot >= 0
                            ? world.FindEntityByRuntimeSlotForQuery(holderSlot)
                            : null;
                        Vector2 heldVisualAttachmentOffsetPixels =
                            LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels(
                                runtime,
                                currentFrame,
                                holder,
                                NTSDRenderSpace.BattleVisualScale);
                        LF2Sprite entitySprite = entity.Sprite;
                        bool entityVisible = entitySprite?.EntityVisible ?? true;
                        bool shadowVisible = entitySprite?.ShadowVisible ?? true;
                        Vector2 localOffsetPixels = entitySprite?.LocalOffsetPixels ?? Vector2.zero;

                        frame.AddEntity(new BattlePresentationEntitySnapshot(
                            handle,
                            runtime.StableId,
                            entity.ObjectId,
                            visualDataId,
                            effectivePic,
                            runtime.ZInt,
                            slot,
                            buildCommands
                                ? entity.GetRenderSortingOrder() -
                                  SimulationWorld.PresentationEntitySubOrder
                                : checked(i * 4),
                            runtime.HitStop,
                            currentFrame != null,
                            currentFrame?.state ?? -1,
                            runtime.LinkState,
                            runtime.HP2Orig,
                            runtime.RelationTeam,
                            currentDataType,
                            entity.GetRuntimeXInt(),
                            entity.GetRuntimeYInt(),
                            entity.GetDisplayZForCurrentDataType(currentDataType),
                            entity.GetRenderOffsetX(),
                            world.ReleaseCameraX,
                            entity.FrameDelay,
                            currentFrame?.centerx ?? 0f,
                            currentFrame?.centery ?? 0f,
                            entry?.PixelWidth ?? 0f,
                            entry?.PixelHeight ?? 0f,
                            heldVisualAttachmentOffsetPixels,
                            entry?.NormalizedUv ?? Rect.zero,
                            entry?.Pivot ?? new Vector2(0.5f, 0f),
                            string.Equals(runtime.Dir, "left", StringComparison.Ordinal),
                            hasCatalogKey,
                            spriteDescriptor,
                            hitRecordStart,
                            sourceHitRecordCount,
                            entityVisible,
                            shadowVisible,
                            localOffsetPixels,
                            currentFrame?.frameId ?? -1,
                            hasCatalogKey ? entry : null));
                        if (buildCommands && hasCatalogKey && spriteDescriptor.HasSprite)
                            frame.RequiresCatalogPublicationBinding = true;
                    }
                }
            }
            finally
            {
                spriteCaptureCache.Clear();
                presentationDiagnostics?.EndPhase(
                    BattlePresentationPhase.BeginFrameCaptureEntities);
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderBeginFrameCaptureEntities);
            }

            if (buildCommands)
            {
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderBeginFrameBuildCommands);
                presentationDiagnostics?.BeginPhase(
                    BattlePresentationPhase.BeginFrameBuildCommands);
                try
                {
                    using (BuildCommandsMarker.Auto())
                    {
                        BuildCommands(frame, detailDiagnostics);
                    }
                }
                finally
                {
                    presentationDiagnostics?.EndPhase(
                        BattlePresentationPhase.BeginFrameBuildCommands);
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderBeginFrameBuildCommands);
                }
            }
        }

        internal void MaterializeCommands(
            BattlePresentationFrame frame,
            BattleTickDetailPhaseDiagnostics detailDiagnostics)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (frame.CommandsMaterialized)
                return;

            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderBeginFrameBuildCommands);
            try
            {
                using (BuildCommandsMarker.Auto())
                {
                    frame.CommandCount = 0;
                    frame.RequiresCatalogPublicationBinding = false;
                    ResolveDeferredSpriteCaptures(frame);
                    BuildCommands(frame, detailDiagnostics);
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderBeginFrameBuildCommands);
            }
        }

        internal void MaterializePresentationOrder(
            SimulationWorld world,
            BattlePresentationFrame frame)
        {
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));
            if (frame.PresentationOrderMaterialized)
                return;

            BattlePresentationPhaseDiagnostics presentationDiagnostics =
                world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
            presentationDiagnostics?.BeginPhase(
                BattlePresentationPhase.BeginFrameSortEntities);
            try
            {
                frame.SortEntities(PresentationSnapshotComparer);
                for (int index = 0; index < frame.EntityCount; index++)
                {
                    ref readonly BattlePresentationEntitySnapshot source =
                        ref frame.GetEntityRef(index);
                    BattlePresentationEntitySnapshot ordered =
                        source.WithPresentationBaseOrder(checked(index * 4));
                    frame.SetEntity(index, in ordered);
                }
                frame.PresentationOrderMaterialized = true;
                world.PublishPresentationRenderOrderFromFrame(
                    frame,
                    reusesCoordinatorSort: true);
            }
            finally
            {
                presentationDiagnostics?.EndPhase(
                    BattlePresentationPhase.BeginFrameSortEntities);
            }
        }

        private static int ComparePresentationSnapshots(
            BattlePresentationEntitySnapshot left,
            BattlePresentationEntitySnapshot right)
        {
            int zComparison = left.ZInt.CompareTo(right.ZInt);
            if (zComparison != 0)
                return zComparison;

            int slotComparison = left.RuntimeSlot.CompareTo(right.RuntimeSlot);
            if (slotComparison != 0)
                return slotComparison;

            return left.StableId.CompareTo(right.StableId);
        }

        private void ResolveSpriteCapture(
            CharacterAnimtorManager manager,
            int visualDataId,
            int effectivePic,
            out BattleSpriteEntry entry,
            out bool hasCatalogKey,
            out BattleSpriteValueDescriptor descriptor)
        {
            ResolveSpriteCapture(
                manager?.SpriteCatalog,
                visualDataId,
                effectivePic,
                out entry,
                out hasCatalogKey,
                out descriptor);
        }

        private void ResolveDeferredSpriteCaptures(BattlePresentationFrame frame)
        {
            spriteCaptureCache.Clear();
            try
            {
                for (int index = 0; index < frame.EntityCount; index++)
                {
                    ref readonly BattlePresentationEntitySnapshot source =
                        ref frame.GetEntityRef(index);
                    ResolveSpriteCapture(
                        frame.BoundCatalog,
                        source.VisualDataId,
                        source.EffectivePic,
                        out BattleSpriteEntry entry,
                        out bool hasCatalogKey,
                        out BattleSpriteValueDescriptor descriptor);
                    BattlePresentationEntitySnapshot resolved =
                        source.WithResolvedSprite(
                            entry?.PixelWidth ?? 0f,
                            entry?.PixelHeight ?? 0f,
                            entry?.NormalizedUv ?? Rect.zero,
                            entry?.Pivot ?? new Vector2(0.5f, 0f),
                            hasCatalogKey,
                            descriptor,
                            hasCatalogKey ? entry : null);
                    frame.SetEntity(index, in resolved);
                }
            }
            finally
            {
                spriteCaptureCache.Clear();
            }
        }

        private void ResolveSpriteCapture(
            BattleSpriteCatalog catalog,
            int visualDataId,
            int effectivePic,
            out BattleSpriteEntry entry,
            out bool hasCatalogKey,
            out BattleSpriteValueDescriptor descriptor)
        {
            if (catalog == null || effectivePic < 0 || effectivePic == 999)
            {
                entry = null;
                hasCatalogKey = false;
                descriptor = default;
                return;
            }

            var key = new BattleSpriteKey(visualDataId, effectivePic);
            if (spriteCaptureCache.TryGetValue(key, out SpriteCaptureCacheEntry cached))
            {
                entry = cached.Entry;
                hasCatalogKey = cached.HasCatalogKey;
                descriptor = cached.Descriptor;
                return;
            }

            hasCatalogKey = catalog.TryGet(visualDataId, effectivePic, out entry);
            Sprite catalogSprite = entry?.LegacySprite;
            Texture2D catalogTexture = entry?.SharedTexture;
            descriptor = new BattleSpriteValueDescriptor(
                hasCatalogKey,
                catalogSprite != null,
                catalogSprite != null ? catalogSprite.GetInstanceID() : 0,
                catalogTexture != null ? catalogTexture.GetInstanceID() : 0,
                0,
                entry?.PixelRect ?? Rect.zero,
                entry?.Pivot ?? Vector2.zero,
                hasCatalogKey,
                hasCatalogKey ? entry.Key : default);
            spriteCaptureCache.Add(
                key,
                new SpriteCaptureCacheEntry(
                    hasCatalogKey,
                    entry,
                    in descriptor));
        }

        private void BuildCommands(
            BattlePresentationFrame frame,
            BattleTickDetailPhaseDiagnostics detailDiagnostics)
        {
            int maximumCommandCount = checked(
                frame.CommandCount +
                checked(frame.EntityCount * MaximumCommandsPerEntityWithoutHitRecords) +
                frame.HitRecordCount);
            BattlePresentationFrame.CommandWriter writer =
                frame.BeginCommandWrite(Math.Max(16, maximumCommandCount));
            RefreshWordGlyphTemplateEpoch(frame.CommonVisualCatalog);
            for (int sheetIndex = 0;
                 sheetIndex < BattleCommonVisualCatalog.WordSheetCount;
                 sheetIndex++)
            {
                BattleCommonVisualBinding binding = null;
                bool hasBinding =
                    mode == BattlePresentationBackendMode.CentralOnly &&
                    frame.CommonVisualCatalog != null &&
                    frame.CommonVisualCatalog.TryGetComLabel(
                        sheetIndex,
                        out binding);
                hasComLabelCommandTemplate[sheetIndex] = hasBinding;
                comLabelCommandTemplates[sheetIndex] = hasBinding
                    ? new WordGlyphCommandTemplate(sheetIndex, 'C', binding)
                    : default;
            }
            bool hasSpecialComComposite =
                hasComLabelCommandTemplate[BattleCommonVisualCatalog.SpecialComSheetIndex];
            WordGlyphCommandTemplate specialComComposite =
                comLabelCommandTemplates[BattleCommonVisualCatalog.SpecialComSheetIndex];

            WordGlyphCommandTemplate specialComC = default;
            WordGlyphCommandTemplate specialComO = default;
            WordGlyphCommandTemplate specialComM = default;
            bool hasSpecialComC = false;
            bool hasSpecialComO = false;
            bool hasSpecialComM = false;
            if (!hasSpecialComComposite)
            {
                hasSpecialComC = TryGetWordGlyphCommandTemplate(
                    frame.CommonVisualCatalog,
                    5,
                    'C',
                    out specialComC);
                hasSpecialComO = TryGetWordGlyphCommandTemplate(
                    frame.CommonVisualCatalog,
                    5,
                    'o',
                    out specialComO);
                hasSpecialComM = TryGetWordGlyphCommandTemplate(
                    frame.CommonVisualCatalog,
                    5,
                    'm',
                    out specialComM);
            }
            NTSDRenderSpace.ViewportTransformSnapshot viewportTransform =
                NTSDRenderSpace.CaptureViewportTransform();
            BattleCommonVisualBinding commonShadow = frame.CommonShadowBinding;
            bool hasCommonShadow = commonShadow != null;
            BattleSpriteValueDescriptor commonShadowDescriptor = hasCommonShadow
                ? new BattleSpriteValueDescriptor(
                    true,
                    true,
                    commonShadow.SpriteInstanceId,
                    commonShadow.TextureInstanceId,
                    commonShadow.MaterialInstanceId,
                    commonShadow.PixelRect,
                    commonShadow.Pivot,
                    BattleVisualResourceKey.CommonShadow)
                : default;
            bool collectDetailTimings = detailDiagnostics != null;
            long shadowElapsedTicks = 0;
            long entityElapsedTicks = 0;
            long overlayElapsedTicks = 0;
            long hitRecordElapsedTicks = 0;
            for (int rank = 0; rank < frame.EntityCount; rank++)
            {
                ref readonly BattlePresentationEntitySnapshot entity =
                    ref frame.GetEntityRef(rank);
                int baseOrder = entity.PresentationBaseOrder;
                int localSequence = 0;

                long sectionStartedAt = collectDetailTimings
                    ? System.Diagnostics.Stopwatch.GetTimestamp()
                    : 0;
                bool drawShadow = entity.ShadowVisible && entity.HasCurrentFrame &&
                                  entity.State != 3005 && entity.State != 9997 &&
                                  entity.LinkState >= 0 && entity.ObjectId != 223 &&
                                  entity.ObjectId != 224 && hasCommonShadow &&
                                  LF2ObjectRenderer.ShouldDrawShadowForHitStop(entity.HitStop);
                if (drawShadow)
                {
                    Vector3 shadowPosition = viewportTransform.ScreenPixelToWorld(
                        entity.XInt + (int)entity.RenderOffsetX - entity.CameraX,
                        entity.ZInt,
                        0f);
                    writer.AddUnchecked(new BattleRenderCommand(
                        BattleRenderCommandType.Shadow,
                        entity.Handle,
                        entity.StableId,
                        -1,
                        -1,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder,
                        ObjectSortingLayerId,
                        localSequence++,
                        shadowPosition,
                        commonShadow.PixelSize,
                        commonShadow.Pivot,
                        commonShadow.NormalizedUv,
                        commonShadow.RenderState,
                        commonShadowDescriptor,
                        commonShadow));
                }
                if (collectDetailTimings)
                {
                    long sectionCompletedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                    shadowElapsedTicks += sectionCompletedAt - sectionStartedAt;
                    sectionStartedAt = sectionCompletedAt;
                }

                bool drawEntity = entity.EntityVisible && entity.State >= 0 &&
                                  entity.EffectivePic != 999 &&
                                  entity.HasCatalogKey &&
                                  LF2ObjectRenderer.ShouldDrawEntityForHitStop(entity.HitStop);
                if (drawEntity)
                {
                    Vector2 pivotPixels = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                        entity.XInt,
                        entity.YInt,
                        entity.DisplayZ,
                        entity.RenderOffsetX,
                        entity.CameraX,
                        entity.FrameDelay,
                        frame.TickIndex,
                        entity.FlipX,
                        entity.PixelWidth,
                        entity.PixelHeight,
                        entity.CenterX,
                        entity.CenterY,
                        NTSDRenderSpace.BattleVisualScale);
                    pivotPixels += entity.HeldVisualAttachmentOffsetPixels;
                    pivotPixels += entity.LocalOffsetPixels * NTSDRenderSpace.BattleVisualScale;
                    Vector3 entityPosition = viewportTransform.ScreenPixelToWorld(
                        pivotPixels.x,
                        pivotPixels.y,
                        0f);
                    writer.AddUnchecked(new BattleRenderCommand(
                        BattleRenderCommandType.Entity,
                        entity.Handle,
                        entity.StableId,
                        entity.VisualDataId,
                        entity.EffectivePic,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder + 1,
                        ObjectSortingLayerId,
                        localSequence++,
                        entityPosition,
                        new Vector2(entity.PixelWidth, entity.PixelHeight),
                        entity.Pivot,
                        entity.NormalizedUv,
                        entity.FlipX,
                        entity.SpriteDescriptor,
                        entity.TrustedResourceIdentity));
                }
                if (collectDetailTimings)
                {
                    long sectionCompletedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                    entityElapsedTicks += sectionCompletedAt - sectionStartedAt;
                    sectionStartedAt = sectionCompletedAt;
                }

                if (entity.HasCurrentFrame)
                {
                    var overlayRuntimeSlot = new BattleEntityOverlayRuntimeSlot(
                        entity.RuntimeSlot,
                        entity.HP2Orig,
                        entity.RelationTeam,
                        entity.CurrentDatObjType,
                        entity.CurrentDatObjectId,
                        entity.HitStop,
                        entity.XInt,
                        entity.YInt,
                        entity.ZInt,
                        (int)entity.RenderOffsetX,
                        entity.CameraX,
                        (int)entity.CenterY);
                    if (BattleEntityOverlayLayout.TryGetSpecialComLayout(
                            in overlayRuntimeSlot,
                            out int specialComX,
                            out int specialComY,
                            out _))
                    {
                        if (hasSpecialComComposite)
                        {
                            Vector3 glyphPosition = viewportTransform.ScreenPixelToWorld(
                                specialComX,
                                specialComY,
                                0f);
                            writer.AddUnchecked(specialComComposite.CreateCommand(
                                entity.Handle,
                                entity.StableId,
                                entity.ZInt,
                                entity.RuntimeSlot,
                                baseOrder + 2,
                                ObjectSortingLayerId,
                                localSequence++,
                                glyphPosition));
                            localSequence += 2;
                        }
                        else if (hasSpecialComC)
                        {
                            Vector3 glyphPosition = viewportTransform.ScreenPixelToWorld(
                                specialComX,
                                specialComY,
                                0f);
                            writer.AddUnchecked(specialComC.CreateCommand(
                                entity.Handle,
                                entity.StableId,
                                entity.ZInt,
                                entity.RuntimeSlot,
                                baseOrder + 2,
                                ObjectSortingLayerId,
                                localSequence++,
                                glyphPosition));
                        }
                        if (!hasSpecialComComposite && hasSpecialComO)
                        {
                            Vector3 glyphPosition = viewportTransform.ScreenPixelToWorld(
                                specialComX + BattleEntityOverlayLayout.GlyphAdvance,
                                specialComY,
                                0f);
                            writer.AddUnchecked(specialComO.CreateCommand(
                                entity.Handle,
                                entity.StableId,
                                entity.ZInt,
                                entity.RuntimeSlot,
                                baseOrder + 2,
                                ObjectSortingLayerId,
                                localSequence++,
                                glyphPosition));
                        }
                        if (!hasSpecialComComposite && hasSpecialComM)
                        {
                            Vector3 glyphPosition = viewportTransform.ScreenPixelToWorld(
                                specialComX + BattleEntityOverlayLayout.GlyphAdvance * 2,
                                specialComY,
                                0f);
                            writer.AddUnchecked(specialComM.CreateCommand(
                                entity.Handle,
                                entity.StableId,
                                entity.ZInt,
                                entity.RuntimeSlot,
                                baseOrder + 2,
                                ObjectSortingLayerId,
                                localSequence++,
                                glyphPosition));
                        }
                    }
                    else if (BattleEntityOverlayLayout.TryBuild(
                            in overlayRuntimeSlot,
                            frame.SlotLabelChars,
                            frame.SlotLabelState,
                            overlayGlyphScratch,
                            out int overlayGlyphCount))
                    {
                        if (TryGetComCompositeSheet(
                                overlayGlyphScratch,
                                overlayGlyphCount,
                                out int comSheetIndex) &&
                            hasComLabelCommandTemplate[comSheetIndex])
                        {
                            BattleEntityOverlayGlyph firstGlyph = overlayGlyphScratch[0];
                            Vector3 glyphPosition = viewportTransform.ScreenPixelToWorld(
                                firstGlyph.PixelX,
                                firstGlyph.PixelY,
                                0f);
                            writer.AddUnchecked(comLabelCommandTemplates[comSheetIndex].CreateCommand(
                                entity.Handle,
                                entity.StableId,
                                entity.ZInt,
                                entity.RuntimeSlot,
                                baseOrder + 2,
                                ObjectSortingLayerId,
                                localSequence,
                                glyphPosition));
                            localSequence += 3;
                        }
                        else
                        {
                            for (int glyphIndex = 0; glyphIndex < overlayGlyphCount; glyphIndex++)
                            {
                                BattleEntityOverlayGlyph glyph = overlayGlyphScratch[glyphIndex];
                                if (!TryGetWordGlyphCommandTemplate(
                                        frame.CommonVisualCatalog,
                                        glyph.SheetIndex,
                                        glyph.CharCode,
                                        out WordGlyphCommandTemplate template))
                                {
                                    continue;
                                }

                                Vector3 glyphPosition = viewportTransform.ScreenPixelToWorld(
                                    glyph.PixelX,
                                    glyph.PixelY,
                                    0f);
                                writer.AddUnchecked(template.CreateCommand(
                                    entity.Handle,
                                    entity.StableId,
                                    entity.ZInt,
                                    entity.RuntimeSlot,
                                    baseOrder + 2,
                                    ObjectSortingLayerId,
                                    localSequence++,
                                    glyphPosition));
                            }
                        }
                    }
                }
                if (collectDetailTimings)
                {
                    long sectionCompletedAt = System.Diagnostics.Stopwatch.GetTimestamp();
                    overlayElapsedTicks += sectionCompletedAt - sectionStartedAt;
                    sectionStartedAt = sectionCompletedAt;
                }

                for (int hitIndex = 0; hitIndex < entity.HitRecordCount; hitIndex++)
                {
                    ref readonly BattlePresentationHitRecordSnapshot hit =
                        ref frame.GetHitRecordRef(entity.HitRecordStart + hitIndex);
                    if (!TryResolveSparkFrame(
                            hit.Age,
                            out int pic,
                            out Vector2 size,
                            out Rect pixelRect))
                        continue;
                    if (!frame.CommonVisualCatalog.TryGetSpark(pic, out BattleCommonVisualBinding spark))
                        continue;

                    Vector3 hitPosition = viewportTransform.ScreenPixelToWorld(
                        hit.AnchorX + entity.RenderOffsetX - entity.CameraX,
                        hit.AnchorZ,
                        0f);
                    writer.AddUnchecked(new BattleRenderCommand(
                        BattleRenderCommandType.HitRecord,
                        entity.Handle,
                        entity.StableId,
                        -1,
                        pic,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder + 3,
                        ObjectSortingLayerId,
                        hitIndex,
                        hitPosition,
                        spark.PixelSize,
                        spark.Pivot,
                        spark.NormalizedUv,
                        spark.RenderState,
                        new BattleSpriteValueDescriptor(
                            true,
                            true,
                            spark.SpriteInstanceId,
                            spark.TextureInstanceId,
                            spark.MaterialInstanceId,
                            spark.PixelRect,
                            spark.Pivot,
                            spark.Key),
                        spark));
                }
                if (collectDetailTimings)
                {
                    hitRecordElapsedTicks +=
                        System.Diagnostics.Stopwatch.GetTimestamp() - sectionStartedAt;
                }
            }
            writer.Commit();
            frame.CommandsMaterialized = true;
            if (collectDetailTimings)
            {
                detailDiagnostics.RecordPhaseElapsed(
                    BattleTickDetailPhase.RenderBuildCommandsShadow,
                    shadowElapsedTicks);
                detailDiagnostics.RecordPhaseElapsed(
                    BattleTickDetailPhase.RenderBuildCommandsEntity,
                    entityElapsedTicks);
                detailDiagnostics.RecordPhaseElapsed(
                    BattleTickDetailPhase.RenderBuildCommandsOverlay,
                    overlayElapsedTicks);
                detailDiagnostics.RecordPhaseElapsed(
                    BattleTickDetailPhase.RenderBuildCommandsHitRecord,
                    hitRecordElapsedTicks);
            }
        }

#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
        public void BuildCommandsForSelfCheck(BattlePresentationFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.CommandCount = 0;
            frame.RequiresCatalogPublicationBinding = false;
            BuildCommands(frame, null);
        }

        public bool TryCreateWordGlyphCommandForSelfCheck(
            BattleCommonVisualCatalog catalog,
            int sheetIndex,
            int charCode,
            RuntimeEntityHandle handle,
            int stableId,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int localSequence,
            Vector3 position,
            out BattleRenderCommand command)
        {
            RefreshWordGlyphTemplateEpoch(catalog);
            if (!TryGetWordGlyphCommandTemplate(
                    catalog,
                    sheetIndex,
                    charCode,
                    out WordGlyphCommandTemplate template))
            {
                command = default;
                return false;
            }

            command = template.CreateCommand(
                handle,
                stableId,
                zInt,
                runtimeSlot,
                sortOrder,
                ObjectSortingLayerId,
                localSequence,
                position);
            return true;
        }
#endif

        private void RefreshWordGlyphTemplateEpoch(BattleCommonVisualCatalog catalog)
        {
            catalog ??= BattleCommonVisualCatalog.Empty;
            if (ReferenceEquals(wordGlyphTemplateCatalog, catalog))
                return;

            wordGlyphTemplateCatalog = catalog;
            wordGlyphTemplateEpoch = unchecked(wordGlyphTemplateEpoch + 1);
            if (wordGlyphTemplateEpoch > 0)
                return;

            Array.Clear(wordGlyphTemplateEpochs, 0, wordGlyphTemplateEpochs.Length);
            wordGlyphTemplateEpoch = 1;
        }

        private bool TryGetWordGlyphCommandTemplate(
            BattleCommonVisualCatalog catalog,
            int sheetIndex,
            int charCode,
            out WordGlyphCommandTemplate template)
        {
            if ((uint)sheetIndex >= BattleCommonVisualCatalog.WordSheetCount ||
                (uint)charCode >= BattleCommonVisualCatalog.WordGlyphsPerSheet)
            {
                template = default;
                return false;
            }

            int templateIndex =
                sheetIndex * BattleCommonVisualCatalog.WordGlyphsPerSheet + charCode;
            if (wordGlyphTemplateEpochs[templateIndex] != wordGlyphTemplateEpoch)
            {
                wordGlyphTemplateEpochs[templateIndex] = wordGlyphTemplateEpoch;
                wordGlyphCommandTemplates[templateIndex] =
                    catalog != null &&
                    catalog.TryGetWordGlyph(
                        sheetIndex,
                        charCode,
                        out BattleCommonVisualBinding binding)
                        ? new WordGlyphCommandTemplate(sheetIndex, charCode, binding)
                        : default;
            }

            template = wordGlyphCommandTemplates[templateIndex];
            return template.HasBinding;
        }

        private static bool TryGetComCompositeSheet(
            BattleEntityOverlayGlyph[] glyphs,
            int glyphCount,
            out int sheetIndex)
        {
            sheetIndex = -1;
            if (glyphs == null || glyphCount != 3)
                return false;

            BattleEntityOverlayGlyph first = glyphs[0];
            BattleEntityOverlayGlyph second = glyphs[1];
            BattleEntityOverlayGlyph third = glyphs[2];
            if (first.Type != BattleEntityOverlayGlyphType.Label ||
                second.Type != BattleEntityOverlayGlyphType.Label ||
                third.Type != BattleEntityOverlayGlyphType.Label ||
                first.CharCode != 'C' ||
                second.CharCode != 'o' ||
                third.CharCode != 'm' ||
                first.SheetIndex != second.SheetIndex ||
                first.SheetIndex != third.SheetIndex ||
                (uint)first.SheetIndex >= BattleCommonVisualCatalog.WordSheetCount ||
                first.PixelY != second.PixelY ||
                first.PixelY != third.PixelY ||
                second.PixelX != first.PixelX + BattleEntityOverlayLayout.GlyphAdvance ||
                third.PixelX != first.PixelX + BattleEntityOverlayLayout.GlyphAdvance * 2)
            {
                return false;
            }

            sheetIndex = first.SheetIndex;
            return true;
        }

        internal static bool TryResolveSparkFrame(
            int age,
            out int pic,
            out Vector2 size,
            out Rect pixelRect)
        {
            if (!BattleCommonVisualCatalog.TryResolveSparkAge(age, out pic))
            {
                size = Vector2.zero;
                pixelRect = Rect.zero;
                return false;
            }

            pixelRect = BattleCommonVisualCatalog.GetSparkPixelRect(pic);
            size = pixelRect.size;
            return true;
        }

        internal static Rect GetAuthoritySparkPixelRect(int pic)
        {
            return BattleCommonVisualCatalog.GetSparkPixelRect(pic);
        }

        internal static Vector2 GetAuthoritySparkPivotNormalized(int pic)
        {
            return BattleCommonVisualCatalog.GetSparkPivotNormalized(pic);
        }

        private readonly struct WordGlyphCommandTemplate
        {
            internal WordGlyphCommandTemplate(
                int sheetIndex,
                int charCode,
                BattleCommonVisualBinding binding)
            {
                HasBinding = true;
                SheetIndex = sheetIndex;
                CharCode = charCode;
                Size = binding.PixelSize;
                Pivot = binding.Pivot;
                NormalizedUv = binding.NormalizedUv;
                RenderState = binding.RenderState;
                TrustedResourceIdentity = binding;
                Binding = binding;
                SpriteDescriptor = new BattleSpriteValueDescriptor(
                    true,
                    true,
                    binding.SpriteInstanceId,
                    binding.TextureInstanceId,
                    binding.MaterialInstanceId,
                    binding.PixelRect,
                    binding.Pivot,
                    binding.Key);
            }

            internal bool HasBinding { get; }
            private int SheetIndex { get; }
            private int CharCode { get; }
            private Vector2 Size { get; }
            private Vector2 Pivot { get; }
            private Rect NormalizedUv { get; }
            private BattleSpriteRenderState RenderState { get; }
            private BattleSpriteValueDescriptor SpriteDescriptor { get; }
            private object TrustedResourceIdentity { get; }
            private BattleCommonVisualBinding Binding { get; }

            internal BattleRenderCommand CreateCommand(
                RuntimeEntityHandle handle,
                int stableId,
                int zInt,
                int runtimeSlot,
                int sortOrder,
                int sortingLayerId,
                int localSequence,
                Vector3 position)
            {
                return new BattleRenderCommand(
                    BattleRenderCommandType.OverlayGlyph,
                    handle,
                    stableId,
                    SheetIndex,
                    CharCode,
                    zInt,
                    runtimeSlot,
                    sortOrder,
                    sortingLayerId,
                    localSequence,
                    position,
                    Size,
                    Pivot,
                    NormalizedUv,
                    RenderState,
                    SpriteDescriptor,
                    TrustedResourceIdentity);
            }

        }

        private void ComparePublishedFrameToLegacyProbes()
        {
            BattlePresentationFrame frame = PublishedFrame;
            Diagnostics.TickIndex = frame?.TickIndex ?? 0;
            Diagnostics.ExpectedCount = 0;
            Diagnostics.ActualCount = legacyProbeCount;
            Diagnostics.DifferenceCount = 0;
            Diagnostics.FirstDifferenceIndex = -1;
            Diagnostics.FirstDifferenceKind = BattlePresentationDifferenceKind.None;
            Diagnostics.HasFirstExpectedCommand = false;
            Diagnostics.FirstExpectedCommand = default;
            Diagnostics.HasFirstActualProbe = false;
            Diagnostics.FirstActualProbe = default;
            Diagnostics.OverlayUnsupportedCount = frame?.OverlayUnsupportedCount ?? 0;
            Diagnostics.OverlayState = BattleOverlayParityState.None;
            if (frame == null)
                return;

            SortLegacyProbes();
            int expectedIndex = 0;
            int actualIndex = 0;
            while (true)
            {
                bool hasExpected = expectedIndex < frame.CommandCount;
                bool hasActual = actualIndex < legacyProbeCount;
                if (!hasExpected && !hasActual)
                    break;

                int comparisonIndex = Diagnostics.ExpectedCount;
                if (!hasExpected)
                {
                    RegisterDifference(
                        comparisonIndex,
                        BattlePresentationDifferenceKind.UnexpectedLegacy,
                        default,
                        false,
                        legacyProbes[actualIndex],
                        true);
                    actualIndex++;
                    continue;
                }
                Diagnostics.ExpectedCount++;
                if (!hasActual)
                {
                    RegisterDifference(
                        comparisonIndex,
                        BattlePresentationDifferenceKind.ExpectedMissing,
                        frame.GetCommand(expectedIndex),
                        true,
                        default,
                        false);
                    expectedIndex++;
                    continue;
                }

                BattleRenderCommand expected = frame.GetCommand(expectedIndex++);
                LegacyPresentationProbe actual = legacyProbes[actualIndex++];
                BattlePresentationDifferenceKind difference = Compare(expected, actual);
                if (difference != BattlePresentationDifferenceKind.None)
                {
                    RegisterDifference(
                        comparisonIndex,
                        difference,
                        expected,
                        true,
                        actual,
                        true);
                }
            }
        }

        private static BattlePresentationDifferenceKind Compare(
            in BattleRenderCommand expected,
            in LegacyPresentationProbe actual)
        {
            if (expected.Type != actual.Type)
                return BattlePresentationDifferenceKind.Category;
            if (expected.Handle != actual.Handle || expected.StableId != actual.StableId)
                return BattlePresentationDifferenceKind.Identity;
            if (expected.SpriteDescriptor.RequiresSprite && !actual.SpriteDescriptor.HasSprite)
                return BattlePresentationDifferenceKind.Visual;
            if (expected.SpriteDescriptor.HasLogicalResourceKey &&
                (!actual.SpriteDescriptor.HasLogicalResourceKey ||
                 expected.SpriteDescriptor.LogicalResourceKey != actual.SpriteDescriptor.LogicalResourceKey))
            {
                return BattlePresentationDifferenceKind.ResourceKey;
            }
            Rect expectedRect = expected.SpriteDescriptor.PixelRect;
            Rect actualRect = actual.SpriteDescriptor.PixelRect;
            if (expectedRect.width > 0f && expectedRect.height > 0f &&
                ((expectedRect.position - actualRect.position).sqrMagnitude > 0.000001f ||
                 (expectedRect.size - actualRect.size).sqrMagnitude > 0.000001f))
            {
                return BattlePresentationDifferenceKind.Visual;
            }
            if (expectedRect.width > 0f && expectedRect.height > 0f &&
                (expected.SpriteDescriptor.PivotNormalized -
                 actual.SpriteDescriptor.PivotNormalized).sqrMagnitude > 0.000001f)
            {
                return BattlePresentationDifferenceKind.Visual;
            }
            if (expected.SortOrder != actual.SortOrder)
                return BattlePresentationDifferenceKind.SortOrder;
            if (expected.SortingLayerId != actual.SortingLayerId)
                return BattlePresentationDifferenceKind.SortOrder;
            if ((expected.Position - actual.Position).sqrMagnitude > 0.000001f)
                return BattlePresentationDifferenceKind.Position;
            if (expected.Size.sqrMagnitude > 0.000001f &&
                (expected.Size - actual.Size).sqrMagnitude > 0.000001f)
                return BattlePresentationDifferenceKind.Size;
            if (!expected.RenderState.IsSupported || !actual.RenderState.IsSupported ||
                expected.RenderState.MaterialSemantic != actual.RenderState.MaterialSemantic ||
                expected.RenderState.MaskInteraction != actual.RenderState.MaskInteraction)
            {
                return BattlePresentationDifferenceKind.RenderState;
            }
            if (!expected.Color.Equals(actual.Color))
                return BattlePresentationDifferenceKind.Color;
            if (expected.FlipX != actual.FlipX || expected.FlipY != actual.FlipY)
                return BattlePresentationDifferenceKind.Flip;
            return BattlePresentationDifferenceKind.None;
        }

        internal static BattlePresentationDifferenceKind CompareForSelfCheck(
            in BattleRenderCommand expected,
            in LegacyPresentationProbe actual)
        {
            return Compare(expected, actual);
        }

        private static bool HasOverlayGlyphCommands(BattlePresentationFrame frame)
        {
            if (frame == null)
                return false;

            for (int index = 0; index < frame.CommandCount; index++)
            {
                if (frame.GetCommand(index).Type == BattleRenderCommandType.OverlayGlyph)
                    return true;
            }

            return false;
        }

        private void RegisterDifference(
            int index,
            BattlePresentationDifferenceKind kind,
            in BattleRenderCommand expected,
            bool hasExpected,
            in LegacyPresentationProbe actual,
            bool hasActual)
        {
            Diagnostics.DifferenceCount++;
            if (Diagnostics.FirstDifferenceIndex >= 0)
                return;
            Diagnostics.FirstDifferenceIndex = index;
            Diagnostics.FirstDifferenceKind = kind;
            Diagnostics.HasFirstExpectedCommand = hasExpected;
            Diagnostics.FirstExpectedCommand = expected;
            Diagnostics.HasFirstActualProbe = hasActual;
            Diagnostics.FirstActualProbe = actual;
        }

        private void SortLegacyProbes()
        {
            for (int i = 1; i < legacyProbeCount; i++)
            {
                LegacyPresentationProbe current = legacyProbes[i];
                int j = i - 1;
                while (j >= 0 && CompareProbeOrder(current, legacyProbes[j]) < 0)
                {
                    legacyProbes[j + 1] = legacyProbes[j];
                    j--;
                }
                legacyProbes[j + 1] = current;
            }
        }

        private static int CompareProbeOrder(in LegacyPresentationProbe left, in LegacyPresentationProbe right)
        {
            int order = left.SortOrder.CompareTo(right.SortOrder);
            return order != 0 ? order : left.LocalSequence.CompareTo(right.LocalSequence);
        }

        private void SortEntitiesByZPreservingSlotOrder(List<LF2Entity> entities)
        {
            int count = entities?.Count ?? 0;
            if (count < 2)
                return;

            EnsureEntitySortCapacity(count);
            for (int index = 0; index < count; index++)
            {
                LF2Entity entity = entities[index];
                entitySortSource[index] = entity;
                int zInt = entity?.Runtime?.ZInt ?? int.MaxValue;
                entitySortKeySource[index] = unchecked((uint)(zInt ^ int.MinValue));
            }

            LF2Entity[] sourceEntities = entitySortSource;
            LF2Entity[] destinationEntities = entitySortDestination;
            uint[] sourceKeys = entitySortKeySource;
            uint[] destinationKeys = entitySortKeyDestination;
            for (int shift = 0; shift < 32; shift += 8)
            {
                Array.Clear(entitySortBuckets, 0, entitySortBuckets.Length);
                for (int index = 0; index < count; index++)
                {
                    int bucket = (int)((sourceKeys[index] >> shift) & 0xFF);
                    entitySortBuckets[bucket]++;
                }

                int offset = 0;
                for (int bucket = 0; bucket < entitySortBuckets.Length; bucket++)
                {
                    int bucketCount = entitySortBuckets[bucket];
                    entitySortBuckets[bucket] = offset;
                    offset += bucketCount;
                }

                for (int index = 0; index < count; index++)
                {
                    uint key = sourceKeys[index];
                    int bucket = (int)((key >> shift) & 0xFF);
                    int destination = entitySortBuckets[bucket]++;
                    destinationEntities[destination] = sourceEntities[index];
                    destinationKeys[destination] = key;
                }

                (sourceEntities, destinationEntities) =
                    (destinationEntities, sourceEntities);
                (sourceKeys, destinationKeys) = (destinationKeys, sourceKeys);
            }

            for (int index = 0; index < count; index++)
                entities[index] = sourceEntities[index];
        }

        private void EnsureEntitySortCapacity(int required)
        {
            if (required <= entitySortSource.Length)
                return;

            int capacity = entitySortSource.Length;
            while (capacity < required)
                capacity = checked(capacity * 2);
            Array.Resize(ref entitySortSource, capacity);
            Array.Resize(ref entitySortDestination, capacity);
            Array.Resize(ref entitySortKeySource, capacity);
            Array.Resize(ref entitySortKeyDestination, capacity);
        }

        private void EnsureEntityHandleCacheCapacity(int required)
        {
            if (required <= entityHandleCache.Length)
                return;

            int capacity = entityHandleCache.Length;
            while (capacity < required)
                capacity = checked(capacity * 2);
            Array.Resize(ref entityHandleCache, capacity);
            Array.Resize(ref entityHandleCacheEpochs, capacity);
        }

        private void AdvanceEntityHandleCacheEpoch()
        {
            if (entityHandleCacheEpoch == int.MaxValue)
            {
                Array.Clear(
                    entityHandleCacheEpochs,
                    0,
                    entityHandleCacheEpochs.Length);
                entityHandleCacheEpoch = 1;
                return;
            }

            entityHandleCacheEpoch++;
        }

        private void EnsureLegacyProbeCapacity(int required)
        {
            if (required <= legacyProbes.Length)
                return;
            int next = legacyProbes.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref legacyProbes, next);
        }
    }
}
