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

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public static BattleSpriteMaterialSemantic Classify(Material material)
        {
            if (material == null || material.shader == null)
                return BattleSpriteMaterialSemantic.Unsupported;

            string shaderName = material.shader.name;
            if (shaderName != BuiltInSpriteShaderName &&
                shaderName != CentralTextureShaderName &&
                shaderName != CentralArrayShaderName)
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            if (shaderName != BuiltInSpriteShaderName &&
                material.GetTag(AlphaContractTag, false, string.Empty) != PremultipliedAlphaContract)
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            if (!material.HasProperty(ColorId) || !IsWhite(material.GetColor(ColorId)) ||
                material.IsKeywordEnabled("PIXELSNAP_ON"))
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            return BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
        }

        public static bool IsDeclaredCentralMaterial(Material material, bool textureArray)
        {
            if (material == null || material.shader == null)
                return false;
            string expectedShader = textureArray
                ? CentralArrayShaderName
                : CentralTextureShaderName;
            return material.shader.name == expectedShader &&
                   Classify(material) == BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
        }

        private static bool IsWhite(Color color)
        {
            const float epsilon = 0.000001f;
            return Mathf.Abs(color.r - 1f) <= epsilon &&
                   Mathf.Abs(color.g - 1f) <= epsilon &&
                   Mathf.Abs(color.b - 1f) <= epsilon &&
                   Mathf.Abs(color.a - 1f) <= epsilon;
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
            int frameId = -1)
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
        public int OverlayUnsupportedCount { get; internal set; }
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

        public BattlePresentationHitRecordSnapshot GetHitRecord(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return hitRecords[index];
        }

        public BattleRenderCommand GetCommand(int index)
        {
            if ((uint)index >= (uint)CommandCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return commands[index];
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
                    OverlayUnsupportedCount = source.OverlayUnsupportedCount;
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
            OverlayUnsupportedCount = 0;
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

        internal void AddEntity(in BattlePresentationEntitySnapshot entity)
        {
            EnsureEntityCapacity(EntityCount + 1);
            entities[EntityCount++] = entity;
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

            internal CommandWriter(
                BattlePresentationFrame owner,
                BattleRenderCommand[] destination,
                int count)
            {
                this.owner = owner;
                this.destination = destination;
                this.count = count;
            }

            internal void AddUnchecked(in BattleRenderCommand command)
            {
                destination[count++] = command;
            }

            internal void Commit()
            {
                owner.CommandCount = count;
            }
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
        private static readonly Comparison<LF2Entity> EntityOrderComparison = CompareEntityOrder;
        private static readonly int ObjectSortingLayerId = SortingLayer.NameToID("Object");
        private static readonly ProfilerMarker SortEntitiesMarker =
            new ProfilerMarker("NTSD.BattlePresentation.SortEntities");
        private static readonly ProfilerMarker CaptureHitRecordsMarker =
            new ProfilerMarker("NTSD.BattlePresentation.CaptureHitRecords");
        private static readonly ProfilerMarker CaptureEntitiesMarker =
            new ProfilerMarker("NTSD.BattlePresentation.CaptureEntities");
        private static readonly ProfilerMarker BuildCommandsMarker =
            new ProfilerMarker("NTSD.BattlePresentation.BuildCommands");
        private readonly BattlePresentationFrame frameA = new BattlePresentationFrame();
        private readonly BattlePresentationFrame frameB = new BattlePresentationFrame();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleA =
            new BattleHitRecordPresentationCycle();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleB =
            new BattleHitRecordPresentationCycle();
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(128);
        private readonly BattleEntityOverlayGlyph[] overlayGlyphScratch =
            new BattleEntityOverlayGlyph[32];
        private readonly WordGlyphCommandTemplate[] wordGlyphCommandTemplates =
            new WordGlyphCommandTemplate[WordGlyphTemplateCount];
        private readonly int[] wordGlyphTemplateEpochs = new int[WordGlyphTemplateCount];
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
        private bool awaitingLegacyCompletion;

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
            if (world == null)
                return;

            entityScratch.Clear();
            try
            {
                BattleTickDetailPhaseDiagnostics detailDiagnostics =
                    world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderBeginFrameSortEntities);
                try
                {
                    using (SortEntitiesMarker.Auto())
                    {
                        world.GetPresentationEntitiesNoAlloc(entityScratch);
                        entityScratch.Sort(EntityOrderComparison);
                    }
                }
                finally
                {
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
                        manager);
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
                BattleSpriteMaterialContract.Classify(material));
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
                BattleSpriteMaterialContract.Classify(material));
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
                    !world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                {
                    continue;
                }

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
                    entity.GetRenderSortingOrder() - SimulationWorld.PresentationEntitySubOrder,
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
            CharacterAnimtorManager manager)
        {
            BattlePresentationFrame previousFrame = PublishedFrame;
            BattlePresentationFrame writeFrame = ReferenceEquals(previousFrame, frameA) ? frameB : frameA;
            CaptureAndBuild(
                world,
                sortedEntities,
                tickIndex,
                commonVisualCatalog,
                hitRecordCycle,
                writeFrame);
            if (RequiresPublicationBinding(writeFrame))
            {
                writeFrame.RetainPublicationBinding(
                    manager,
                    manager?.SpriteCatalog,
                    previousFrame);
            }

            Interlocked.Exchange(ref publishedFrame, writeFrame);
            previousFrame?.ReleasePublicationBinding();
        }

        private static bool RequiresPublicationBinding(BattlePresentationFrame frame)
        {
            if (frame == null)
                return false;

            for (int commandIndex = 0; commandIndex < frame.CommandCount; commandIndex++)
            {
                BattleSpriteValueDescriptor descriptor = frame.GetCommand(commandIndex).SpriteDescriptor;
                if (descriptor.HasLogicalResourceKey && descriptor.HasSprite)
                    return true;
            }

            return false;
        }

        private void CaptureAndBuild(
            SimulationWorld world,
            List<LF2Entity> sortedEntities,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle hitRecordCycle,
            BattlePresentationFrame frame)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderBeginFrameCaptureEntities);
            try
            {
                using (CaptureEntitiesMarker.Auto())
                {
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
                            !world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                        {
                            continue;
                        }

                        LF2FrameData currentFrame = entity.Frame?.D;
                        int visualDataId = LF2Entity.ResolveCurrentDataObjectId(entity);
                        int effectivePic = entity.GetRenderPicIndex();
                        bool hasCatalogKey = entity.TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry);
                        Sprite catalogSprite = entry?.LegacySprite;
                        Texture2D catalogTexture = entry?.SharedTexture;
                        var spriteDescriptor = new BattleSpriteValueDescriptor(
                            hasCatalogKey,
                            catalogSprite != null,
                            catalogSprite != null ? catalogSprite.GetInstanceID() : 0,
                            catalogTexture != null ? catalogTexture.GetInstanceID() : 0,
                            0,
                            entry?.PixelRect ?? Rect.zero,
                            entry?.Pivot ?? Vector2.zero,
                            hasCatalogKey,
                            hasCatalogKey ? entry.Key : default);
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
                        LF2Entity holder = world.FindEntityByRuntimeSlotForQuery(holderSlot);
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
                            entity.GetRenderSortingOrder() - SimulationWorld.PresentationEntitySubOrder,
                            runtime.HitStop,
                            currentFrame != null,
                            currentFrame?.state ?? -1,
                            runtime.LinkState,
                            runtime.HP2Orig,
                            runtime.RelationTeam,
                            entity.GetCurrentDataObjectTypeForSimulation(),
                            entity.GetRuntimeXInt(),
                            entity.GetRuntimeYInt(),
                            entity.GetDisplayZ(),
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
                            currentFrame?.frameId ?? -1));
                    }
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderBeginFrameCaptureEntities);
            }

            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderBeginFrameBuildCommands);
            try
            {
                using (BuildCommandsMarker.Auto())
                {
                    BuildCommands(frame);
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderBeginFrameBuildCommands);
            }
        }

        private void BuildCommands(BattlePresentationFrame frame)
        {
            int maximumCommandCount = checked(
                frame.CommandCount +
                checked(frame.EntityCount * MaximumCommandsPerEntityWithoutHitRecords) +
                frame.HitRecordCount);
            BattlePresentationFrame.CommandWriter writer =
                frame.BeginCommandWrite(Math.Max(16, maximumCommandCount));
            RefreshWordGlyphTemplateEpoch(frame.CommonVisualCatalog);
            for (int rank = 0; rank < frame.EntityCount; rank++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(rank);
                int baseOrder = entity.PresentationBaseOrder;
                int localSequence = 0;

                bool drawShadow = entity.ShadowVisible && entity.HasCurrentFrame &&
                                  entity.State != 3005 && entity.State != 9997 &&
                                  entity.LinkState >= 0 && entity.ObjectId != 223 &&
                                  entity.ObjectId != 224 && frame.CommonShadowBinding != null &&
                                  LF2ObjectRenderer.ShouldDrawShadowForHitStop(entity.HitStop);
                if (drawShadow)
                {
                    BattleCommonVisualBinding shadow = frame.CommonShadowBinding;
                    Vector3 shadowPosition = NTSDRenderSpace.ScreenPixelToWorld(
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
                        NTSDRenderSpace.SnapWorldPosition(shadowPosition),
                        shadow.PixelSize,
                        shadow.Pivot,
                        shadow.NormalizedUv,
                        shadow.RenderState,
                        new BattleSpriteValueDescriptor(
                            true,
                            true,
                            shadow.SpriteInstanceId,
                            shadow.TextureInstanceId,
                            shadow.MaterialInstanceId,
                            shadow.PixelRect,
                            shadow.Pivot,
                            BattleVisualResourceKey.CommonShadow)));
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
                    Vector3 entityPosition = NTSDRenderSpace.ScreenPixelToWorld(pivotPixels.x, pivotPixels.y, 0f);
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
                        entity.SpriteDescriptor));
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
                    if (BattleEntityOverlayLayout.TryBuild(
                            in overlayRuntimeSlot,
                            frame.SlotLabelChars,
                            frame.SlotLabelState,
                            overlayGlyphScratch,
                            out int overlayGlyphCount))
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

                            Vector3 glyphPosition = NTSDRenderSpace.ScreenPixelToWorld(
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

                for (int hitIndex = 0; hitIndex < entity.HitRecordCount; hitIndex++)
                {
                    BattlePresentationHitRecordSnapshot hit = frame.GetHitRecord(
                        entity.HitRecordStart + hitIndex);
                    if (!TryResolveSparkFrame(
                            hit.Age,
                            out int pic,
                            out Vector2 size,
                            out Rect pixelRect))
                        continue;
                    if (!frame.CommonVisualCatalog.TryGetSpark(pic, out BattleCommonVisualBinding spark))
                        continue;

                    Vector3 hitPosition = NTSDRenderSpace.ScreenPixelToWorld(
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
                            spark.Key)));
                }
            }
            writer.Commit();
        }

#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
        public void BuildCommandsForSelfCheck(BattlePresentationFrame frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            frame.CommandCount = 0;
            BuildCommands(frame);
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
                    SpriteDescriptor);
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

        private static int CompareEntityOrder(LF2Entity left, LF2Entity right)
        {
            int z = (left?.Runtime?.ZInt ?? int.MaxValue).CompareTo(right?.Runtime?.ZInt ?? int.MaxValue);
            return z != 0
                ? z
                : (left?.Runtime?.SlotIndex ?? int.MaxValue).CompareTo(right?.Runtime?.SlotIndex ?? int.MaxValue);
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
