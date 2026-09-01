using System;
using System.Collections.Generic;
using System.IO;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NTSD.Animation.Rendering
{
    [Serializable]
    public sealed class BattleCentralEditorPreviewActor
    {
        [SerializeField] private bool visible = true;
        [SerializeField] private bool resolveFromCharacterManager = true;
        [SerializeField] private int objectId;
        [SerializeField] private int frameId;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Texture2D sourceSheet;
        [SerializeField] private RectInt sourceRectPixels = new RectInt(0, 0, 79, 79);
        [SerializeField] private Vector2 sourcePivot = new Vector2(0.5f, 0f);
        [SerializeField] private Transform anchor;
        [SerializeField] private Vector3 localPivotPosition;
        [SerializeField] private bool flipX;
        [SerializeField] private Color32 tint = new Color32(255, 255, 255, 255);
        [SerializeField] private bool showHealthBar = true;
        [SerializeField] private int currentHealth = 100;
        [SerializeField] private int recoverableHealth = 100;
        [SerializeField][Min(1)] private int maximumHealth = 100;

        public bool Visible => visible;
        public bool ResolveFromCharacterManager => resolveFromCharacterManager;
        public int ObjectId => objectId;
        public int FrameId => frameId;
        public Sprite Sprite => sprite;
        public Texture2D SourceSheet => sourceSheet;
        public RectInt SourceRectPixels => sourceRectPixels;
        public Vector2 SourcePivot => sourcePivot;
        public Transform Anchor => anchor;
        public Vector3 LocalPivotPosition => localPivotPosition;
        public bool FlipX => flipX;
        public Color32 Tint => tint;
        public bool ShowHealthBar => showHealthBar;
        public int CurrentHealth => currentHealth;
        public int RecoverableHealth => recoverableHealth;
        public int MaximumHealth => maximumHealth;

        internal void ConfigureForSelfCheck(
            Sprite configuredSprite,
            Vector3 configuredLocalPivotPosition,
            int configuredCurrentHealth,
            int configuredRecoverableHealth,
            int configuredMaximumHealth)
        {
            visible = true;
            resolveFromCharacterManager = false;
            objectId = 0;
            frameId = 0;
            sprite = configuredSprite;
            sourceSheet = null;
            sourceRectPixels = new RectInt(0, 0, 79, 79);
            sourcePivot = new Vector2(0.5f, 0f);
            anchor = null;
            localPivotPosition = configuredLocalPivotPosition;
            flipX = false;
            tint = new Color32(255, 255, 255, 255);
            showHealthBar = true;
            currentHealth = configuredCurrentHealth;
            recoverableHealth = configuredRecoverableHealth;
            maximumHealth = configuredMaximumHealth;
        }

        internal void ConfigureTextureForSelfCheck(
            Texture2D configuredSourceSheet,
            RectInt configuredSourceRectPixels,
            Vector2 configuredSourcePivot,
            int configuredCurrentHealth,
            int configuredRecoverableHealth,
            int configuredMaximumHealth)
        {
            ConfigureForSelfCheck(
                null,
                Vector3.zero,
                configuredCurrentHealth,
                configuredRecoverableHealth,
                configuredMaximumHealth);
            sourceSheet = configuredSourceSheet;
            sourceRectPixels = configuredSourceRectPixels;
            sourcePivot = configuredSourcePivot;
        }
    }

#if UNITY_EDITOR
    internal readonly struct BattleCentralEditorPreviewLayout
    {
        public BattleCentralEditorPreviewLayout(
            Vector3 pivotWorldPosition,
            Bounds spriteBounds,
            bool hasHealthBar,
            Bounds healthBarBounds)
        {
            PivotWorldPosition = pivotWorldPosition;
            SpriteBounds = spriteBounds;
            HasHealthBar = hasHealthBar;
            HealthBarBounds = healthBarBounds;
        }

        public Vector3 PivotWorldPosition { get; }
        public Bounds SpriteBounds { get; }
        public bool HasHealthBar { get; }
        public Bounds HealthBarBounds { get; }
    }
#endif

    [ExecuteAlways]
    [DisallowMultipleComponent]
    [AddComponentMenu("NTSD/Battle Rendering/Battle Central Editor Preview")]
    public sealed class BattleCentralEditorPreview : MonoBehaviour
    {
        private const string DefaultMaterialPath =
            "Assets/NTSD/Materials/BattleCentralTransparent.mat";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int MainTexArrayId = Shader.PropertyToID("_MainTexArray");
        private static readonly List<BattleCentralEditorPreview> RegisteredPreviews =
            new List<BattleCentralEditorPreview>(2);
#if UNITY_EDITOR
        private static BattleCentralEditorPreview exclusiveValidationPreview;
#endif

        [Header("Camera Preview")]
        [SerializeField] private bool previewInSceneView = true;
        [SerializeField] private bool previewInGameView = true;
        [SerializeField] private Material material;

        [Header("Preview Actors")]
        [SerializeField] private List<BattleCentralEditorPreviewActor> actors =
            new List<BattleCentralEditorPreviewActor>
            {
                new BattleCentralEditorPreviewActor(),
            };

        [Header("Overhead Health Bars")]
        [SerializeField] private bool drawHealthBars = true;
        [SerializeField] private BattleHealthBarStyle healthBarStyle = default;

        private readonly BattlePresentationFrame previewFrame = new BattlePresentationFrame();
        private readonly BattleDynamicMeshBackend actorBackend = new BattleDynamicMeshBackend();
        private readonly BattleHealthBarBatchBackend healthBackend =
            new BattleHealthBarBatchBackend();
        private readonly PreviewResourceResolver resourceResolver = new PreviewResourceResolver();
        private BattleHealthBarInstance[] healthInstances =
            Array.Empty<BattleHealthBarInstance>();
        private Sprite[] resolvedSprites = Array.Empty<Sprite>();
        private OwnedSpriteCache[] ownedSpriteCaches = Array.Empty<OwnedSpriteCache>();
        private Material resolvedMaterial;
        private int builtSignature;
        private bool hasBuiltSignature;
        private bool resourcesDisposed;

        public int PreviewActorCount => actorBackend.Diagnostics.ResolvedCommandCount;
        public int PreviewHealthBarCount => healthBackend.ActiveBarCount;
        public int PreviewHealthQuadCount => healthBackend.ActiveQuadCount;

        private void Reset()
        {
            healthBarStyle = BattleHealthBarStyle.Default;
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
                return;
#if UNITY_EDITOR
            hideFlags |= HideFlags.DontSaveInBuild;
#endif
            if (!RegisteredPreviews.Contains(this))
                RegisteredPreviews.Add(this);
            BattleCentralRenderSystem.RefreshRuntimeHealthBarAuthoringSettings();
            InvalidateAndRepaint();
        }

        private void OnDisable()
        {
            RegisteredPreviews.Remove(this);
            RepaintEditorViews();
        }

        private void OnDestroy()
        {
            RegisteredPreviews.Remove(this);
            DisposeResources();
        }

        private void OnValidate()
        {
            if (healthBarStyle.WidthPixels <= 0f || healthBarStyle.HeightPixels <= 0f)
                healthBarStyle = BattleHealthBarStyle.Default;
            BattleCentralRenderSystem.RefreshRuntimeHealthBarAuthoringSettings();
            InvalidateAndRepaint();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void RefreshRuntimeHealthBarSettingsAfterSceneLoad()
        {
            BattleCentralRenderSystem.RefreshRuntimeHealthBarAuthoringSettings();
        }

        private void Update()
        {
            if (Application.isPlaying || !transform.hasChanged)
                return;
            transform.hasChanged = false;
            InvalidateAndRepaint();
        }

        internal static bool TryGetActiveForCamera(
            Camera camera,
            CameraRenderType renderType,
            out BattleCentralEditorPreview preview)
        {
            preview = null;
            if (Application.isPlaying || renderType != CameraRenderType.Base || camera == null)
                return false;

#if UNITY_EDITOR
            BattleCentralEditorPreview validationPreview = exclusiveValidationPreview;
            if (validationPreview != null)
            {
                if (!validationPreview.isActiveAndEnabled ||
                    !validationPreview.CanRenderCamera(camera) ||
                    !validationPreview.EnsureBuilt())
                {
                    return false;
                }

                preview = validationPreview;
                return true;
            }
#endif

            RemoveDeadRegistrations();
            BattleCentralEditorPreview candidate = null;
            for (int index = 0; index < RegisteredPreviews.Count; index++)
            {
                BattleCentralEditorPreview current = RegisteredPreviews[index];
                if (current == null || !current.isActiveAndEnabled)
                    continue;
                if (candidate != null)
                    return false;
                candidate = current;
            }

            if (candidate == null || !candidate.CanRenderCamera(camera) || !candidate.EnsureBuilt())
                return false;
            preview = candidate;
            return true;
        }

        internal static bool TryGetRuntimeHealthBarAuthoringSettings(
            out bool enabled,
            out BattleHealthBarStyle style)
        {
            enabled = true;
            style = BattleHealthBarStyle.Default;
#if UNITY_EDITOR
            if (exclusiveValidationPreview != null)
            {
                enabled = exclusiveValidationPreview.drawHealthBars;
                style = exclusiveValidationPreview.healthBarStyle.WidthPixels > 0f &&
                        exclusiveValidationPreview.healthBarStyle.HeightPixels > 0f
                    ? exclusiveValidationPreview.healthBarStyle.Normalized()
                    : BattleHealthBarStyle.Default;
                return true;
            }
#endif
            BattleCentralEditorPreview[] previews =
                Resources.FindObjectsOfTypeAll<BattleCentralEditorPreview>();
            BattleCentralEditorPreview candidate = null;
            bool candidateIsActive = false;
            int candidateId = int.MaxValue;
            for (int index = 0; index < previews.Length; index++)
            {
                BattleCentralEditorPreview preview = previews[index];
                if (preview == null || preview.gameObject == null ||
                    !preview.gameObject.scene.IsValid() || !preview.gameObject.scene.isLoaded ||
                    (preview.hideFlags & HideFlags.HideInHierarchy) != 0)
                {
                    continue;
                }

                bool isActive = preview.isActiveAndEnabled;
                int instanceId = preview.GetInstanceID();
                if (candidate == null || isActive && !candidateIsActive ||
                    isActive == candidateIsActive && instanceId < candidateId)
                {
                    candidate = preview;
                    candidateIsActive = isActive;
                    candidateId = instanceId;
                }
            }

            if (candidate == null)
                return false;

            enabled = candidate.drawHealthBars;
            style = candidate.healthBarStyle.WidthPixels > 0f &&
                    candidate.healthBarStyle.HeightPixels > 0f
                ? candidate.healthBarStyle.Normalized()
                : BattleHealthBarStyle.Default;
            return true;
        }

        internal int AppendDrawCommands(
            CommandBuffer commandBuffer,
            MaterialPropertyBlock propertyBlock)
        {
            if (commandBuffer == null)
                throw new ArgumentNullException(nameof(commandBuffer));
            if (propertyBlock == null)
                throw new ArgumentNullException(nameof(propertyBlock));
            if (Application.isPlaying || resolvedMaterial == null)
                return 0;

            int drawCount = 0;
            for (int segmentIndex = 0; segmentIndex < actorBackend.SegmentCount; segmentIndex++)
            {
                BattleCentralRenderSegment segment = actorBackend.GetSegment(segmentIndex);
                if (segment.Material == null || segment.Texture == null)
                    continue;
                propertyBlock.Clear();
                if (segment.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray)
                    propertyBlock.SetTexture(MainTexArrayId, segment.Texture);
                else
                    propertyBlock.SetTexture(MainTexId, segment.Texture);
                commandBuffer.DrawMesh(
                    actorBackend.GetChunkMesh(segment.ChunkIndex),
                    Matrix4x4.identity,
                    segment.Material,
                    segment.SubMeshIndex,
                    0,
                    propertyBlock);
                drawCount++;
            }

            if (drawHealthBars && healthBackend.ActiveBarCount > 0 && healthBackend.Mesh != null)
            {
                propertyBlock.Clear();
                propertyBlock.SetTexture(MainTexId, Texture2D.whiteTexture);
                commandBuffer.DrawMesh(
                    healthBackend.Mesh,
                    Matrix4x4.identity,
                    resolvedMaterial,
                    0,
                    0,
                    propertyBlock);
                drawCount++;
            }

            return drawCount;
        }

        internal bool CanRenderCameraForSelfCheck(
            Camera camera,
            CameraRenderType renderType,
            CameraType cameraType,
            bool isPlaying,
            Camera worldCamera)
        {
            if (isPlaying || renderType != CameraRenderType.Base || camera == null)
                return false;
            if (cameraType == CameraType.SceneView)
                return previewInSceneView;
            return previewInGameView && camera == worldCamera;
        }

        internal void ConfigureForSelfCheck(
            Material configuredMaterial,
            BattleCentralEditorPreviewActor actor,
            in BattleHealthBarStyle configuredStyle)
        {
            material = configuredMaterial;
            actors.Clear();
            actors.Add(actor);
            healthBarStyle = configuredStyle;
            drawHealthBars = true;
            InvalidateAndRepaint();
        }

        internal bool RebuildForSelfCheck()
        {
            hasBuiltSignature = false;
            return EnsureBuilt();
        }

        internal static IDisposable BeginExclusiveValidationForSelfCheck(
            BattleCentralEditorPreview preview)
        {
#if UNITY_EDITOR
            if (preview == null)
                throw new ArgumentNullException(nameof(preview));
            if (Application.isPlaying)
                throw new InvalidOperationException("Editor preview validation requires Edit Mode.");
            if (exclusiveValidationPreview != null)
            {
                throw new InvalidOperationException(
                    "Another central editor preview validation is already active.");
            }

            exclusiveValidationPreview = preview;
            return new ExclusiveValidationScope(preview);
#else
            throw new PlatformNotSupportedException(
                "Central editor preview validation is only available in the Unity Editor.");
#endif
        }

        internal BattleHealthBarBatchBackend HealthBackendForSelfCheck => healthBackend;

#if UNITY_EDITOR
        internal int EditorActorCount => actors?.Count ?? 0;

        internal BattleCentralEditorPreviewActor GetEditorActor(int actorIndex)
        {
            return actors != null && (uint)actorIndex < (uint)actors.Count
                ? actors[actorIndex]
                : null;
        }

        internal void RequestEditorPreviewRefresh()
        {
            InvalidateAndRepaint();
        }

        internal static RectInt ResolveTopLeftSourceRectForEditor(
            Texture2D sourceSheet,
            int preferredWidth,
            int preferredHeight)
        {
            if (sourceSheet == null || sourceSheet.width <= 0 || sourceSheet.height <= 0)
                return default;
            int width = Mathf.Clamp(preferredWidth, 1, sourceSheet.width);
            int height = Mathf.Clamp(preferredHeight, 1, sourceSheet.height);
            List<RuntimeSpriteProcessor.SpriteRectData> rects =
                RuntimeSpriteProcessor.BuildSpriteRectsFromTopLeft(
                    sourceSheet.width,
                    sourceSheet.height,
                    width,
                    height,
                    1,
                    1);
            if (rects.Count == 0)
                return new RectInt(0, Mathf.Max(0, sourceSheet.height - height), width, height);
            Rect rect = rects[0].Rect;
            return new RectInt(
                Mathf.RoundToInt(rect.x),
                Mathf.RoundToInt(rect.y),
                Mathf.RoundToInt(rect.width),
                Mathf.RoundToInt(rect.height));
        }

        internal bool TryGetEditorLayout(
            int actorIndex,
            out BattleCentralEditorPreviewLayout layout)
        {
            layout = default;
            if (!EnsureBuilt() || actors == null ||
                (uint)actorIndex >= (uint)actors.Count ||
                (uint)actorIndex >= (uint)resolvedSprites.Length)
            {
                return false;
            }

            BattleCentralEditorPreviewActor actor = actors[actorIndex];
            Sprite sprite = resolvedSprites[actorIndex];
            if (actor == null || !actor.Visible || sprite == null || sprite.texture == null)
                return false;

            Vector2 pixelSize = sprite.rect.size;
            if (pixelSize.x <= 0f || pixelSize.y <= 0f)
                return false;
            Vector2 pivot = new Vector2(
                sprite.pivot.x / pixelSize.x,
                sprite.pivot.y / pixelSize.y);
            Transform anchor = actor.Anchor != null ? actor.Anchor : transform;
            Vector3 position = anchor.TransformPoint(actor.LocalPivotPosition);
            float spriteWidth = pixelSize.x * NTSDRenderSpace.UnitsPerPixelX *
                                NTSDRenderSpace.BattleVisualScale;
            float spriteHeight = pixelSize.y * NTSDRenderSpace.UnitsPerPixelY *
                                 NTSDRenderSpace.BattleVisualScale;
            float spriteLeft = position.x - pivot.x * spriteWidth;
            float spriteBottom = position.y - pivot.y * spriteHeight;
            float spriteTop = spriteBottom + spriteHeight;
            float spriteCenterX = spriteLeft + spriteWidth * 0.5f;
            var spriteBounds = new Bounds(
                new Vector3(
                    spriteCenterX,
                    spriteBottom + spriteHeight * 0.5f,
                    position.z),
                new Vector3(spriteWidth, spriteHeight, 0.001f));

            bool hasHealthBar = drawHealthBars && actor.ShowHealthBar &&
                                actor.MaximumHealth > 0;
            Bounds healthBounds = default;
            if (hasHealthBar)
            {
                BattleHealthBarStyle style = healthBarStyle.Normalized();
                float barWidth = style.WidthPixels * NTSDRenderSpace.UnitsPerPixelX;
                float barHeight = style.HeightPixels * NTSDRenderSpace.UnitsPerPixelY;
                float stableHeightPixels = ResolveStableHealthAnchorHeightPixels(actor, sprite);
                float stableTop = position.y + stableHeightPixels *
                                  NTSDRenderSpace.UnitsPerPixelY *
                                  NTSDRenderSpace.BattleVisualScale;
                float barCenterX = position.x +
                                   style.OffsetPixels.x * NTSDRenderSpace.UnitsPerPixelX;
                float barBottom = stableTop +
                                  (style.HeadGapPixels + style.OffsetPixels.y) *
                                  NTSDRenderSpace.UnitsPerPixelY;
                healthBounds = new Bounds(
                    new Vector3(barCenterX, barBottom + barHeight * 0.5f, position.z),
                    new Vector3(barWidth, barHeight, 0.001f));
            }

            layout = new BattleCentralEditorPreviewLayout(
                position,
                spriteBounds,
                hasHealthBar,
                healthBounds);
            return true;
        }
#endif

        private bool CanRenderCamera(Camera camera)
        {
            if (camera.cameraType == CameraType.SceneView)
                return previewInSceneView;
            return previewInGameView && camera == NTSDRenderSpace.WorldCamera;
        }

        private bool EnsureBuilt()
        {
            if (resourcesDisposed || Application.isPlaying)
                return false;

            Material nextMaterial = ResolveMaterial();
            if (nextMaterial == null)
                return false;

            int signature = ComputeSignature(nextMaterial);
            if (!hasBuiltSignature || signature != builtSignature)
            {
                Rebuild(nextMaterial);
                builtSignature = signature;
                hasBuiltSignature = true;
            }

            return actorBackend.SegmentCount > 0 || healthBackend.ActiveBarCount > 0;
        }

        private void Rebuild(Material nextMaterial)
        {
            resolvedMaterial = nextMaterial;
            previewFrame.Reset(0);
            int actorCapacity = actors?.Count ?? 0;
            actorBackend.PrepareCapacity(actorCapacity);
            EnsureHealthInstanceCapacity(actorCapacity);
            EnsureResolvedSpriteCapacity(actorCapacity);
            EnsureOwnedSpriteCacheCapacity(actorCapacity);
            DisposeUnusedOwnedSpriteCaches(actorCapacity);
            int healthCount = 0;

            for (int actorIndex = 0; actorIndex < actorCapacity; actorIndex++)
            {
                BattleCentralEditorPreviewActor actor = actors[actorIndex];
                Sprite sprite = ResolveActorSprite(actor, actorIndex);
                resolvedSprites[actorIndex] = sprite;
                if (actor == null || !actor.Visible || sprite == null || sprite.texture == null)
                    continue;

                Vector2 pixelSize = sprite.rect.size;
                if (pixelSize.x <= 0f || pixelSize.y <= 0f)
                    continue;
                Vector2 pivot = new Vector2(
                    sprite.pivot.x / pixelSize.x,
                    sprite.pivot.y / pixelSize.y);
                Transform anchor = actor.Anchor != null ? actor.Anchor : transform;
                Vector3 position = anchor.TransformPoint(actor.LocalPivotPosition);
                previewFrame.AddCommand(new BattleRenderCommand(
                    BattleRenderCommandType.Entity,
                    RuntimeEntityHandle.Invalid,
                    actorIndex + 1,
                    actorIndex,
                    actorIndex,
                    Mathf.RoundToInt(position.z / NTSDRenderSpace.UnitsPerPixelY),
                    actorIndex,
                    actorIndex,
                    0,
                    actorIndex,
                    position,
                    pixelSize,
                    pivot,
                    NormalizedUv(sprite),
                    actor.FlipX,
                    default));

                if (!drawHealthBars || !actor.ShowHealthBar || actor.MaximumHealth <= 0)
                    continue;

                float stableHeightPixels = ResolveStableHealthAnchorHeightPixels(actor, sprite);
                float stableTop = position.y + stableHeightPixels *
                                  NTSDRenderSpace.UnitsPerPixelY *
                                  NTSDRenderSpace.BattleVisualScale;
                healthInstances[healthCount++] = new BattleHealthBarInstance(
                    new Vector2(position.x, stableTop),
                    position.z,
                    actor.CurrentHealth,
                    actor.RecoverableHealth,
                    actor.MaximumHealth);
            }

            resourceResolver.Configure(actors, resolvedSprites, nextMaterial);
            previewFrame.CommandsMaterialized = true;
            actorBackend.Build(previewFrame, resourceResolver, BattleCentralDrawMode.OrderedChunks);
            for (int chunkIndex = 0; chunkIndex < actorBackend.ActiveChunkCount; chunkIndex++)
                actorBackend.GetChunkMesh(chunkIndex).hideFlags = HideFlags.HideAndDontSave;
            healthBackend.Build(healthInstances, healthCount, healthBarStyle);
        }

        private static float ResolveStableHealthAnchorHeightPixels(
            BattleCentralEditorPreviewActor actor,
            Sprite sprite)
        {
            if (actor != null && actor.ResolveFromCharacterManager)
            {
                LF2CharacterData characterData =
                    CharacterAnimtorManager.Instance?.GetCharacterData(actor.ObjectId);
                if (characterData != null)
                    return BattleHealthBarAnchor.ResolveStableCharacterHeightPixels(characterData);
            }

            if (actor != null && actor.SourceSheet != null && actor.SourceRectPixels.height > 0)
                return actor.SourceRectPixels.height;
            if (sprite != null && sprite.rect.height > 0f)
                return sprite.rect.height;
            return BattleHealthBarAnchor.DefaultCharacterHeightPixels;
        }

        private int ComputeSignature(Material nextMaterial)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + nextMaterial.GetInstanceID();
                hash = hash * 31 + transform.localToWorldMatrix.GetHashCode();
                hash = hash * 31 + previewInSceneView.GetHashCode();
                hash = hash * 31 + previewInGameView.GetHashCode();
                hash = hash * 31 + drawHealthBars.GetHashCode();
                hash = hash * 31 + healthBarStyle.WidthPixels.GetHashCode();
                hash = hash * 31 + healthBarStyle.HeightPixels.GetHashCode();
                hash = hash * 31 + healthBarStyle.BorderPixels.GetHashCode();
                hash = hash * 31 + healthBarStyle.HeadGapPixels.GetHashCode();
                hash = hash * 31 + healthBarStyle.OffsetPixels.GetHashCode();
                hash = hash * 31 + healthBarStyle.BackgroundColor.GetHashCode();
                hash = hash * 31 + healthBarStyle.RecoverableColor.GetHashCode();
                hash = hash * 31 + healthBarStyle.CurrentColor.GetHashCode();

                int actorCount = actors?.Count ?? 0;
                hash = hash * 31 + actorCount;
                for (int index = 0; index < actorCount; index++)
                {
                    BattleCentralEditorPreviewActor actor = actors[index];
                    if (actor == null)
                    {
                        hash = hash * 31;
                        continue;
                    }

                    hash = hash * 31 + actor.Visible.GetHashCode();
                    hash = hash * 31 + actor.ResolveFromCharacterManager.GetHashCode();
                    hash = hash * 31 + actor.ObjectId;
                    hash = hash * 31 + actor.FrameId;
                    Sprite resolvedSprite = ResolveNonTextureActorSprite(actor);
                    hash = hash * 31 +
                           (resolvedSprite != null ? resolvedSprite.GetInstanceID() : 0);
                    hash = hash * 31 +
                           (actor.SourceSheet != null ? actor.SourceSheet.GetInstanceID() : 0);
                    hash = hash * 31 + actor.SourceRectPixels.GetHashCode();
                    hash = hash * 31 + actor.SourcePivot.GetHashCode();
                    hash = hash * 31 + actor.LocalPivotPosition.GetHashCode();
                    hash = hash * 31 + actor.FlipX.GetHashCode();
                    hash = hash * 31 + actor.Tint.GetHashCode();
                    hash = hash * 31 + actor.ShowHealthBar.GetHashCode();
                    hash = hash * 31 + actor.CurrentHealth;
                    hash = hash * 31 + actor.RecoverableHealth;
                    hash = hash * 31 + actor.MaximumHealth;
                    if (actor.Anchor != null)
                        hash = hash * 31 + actor.Anchor.localToWorldMatrix.GetHashCode();
                }

                return hash;
            }
        }

        private Material ResolveMaterial()
        {
            if (material != null)
                return material;
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Material>(DefaultMaterialPath);
#else
            return null;
#endif
        }

        private void EnsureHealthInstanceCapacity(int required)
        {
            if (required <= healthInstances.Length)
                return;
            int next = Mathf.NextPowerOfTwo(Mathf.Max(1, required));
            Array.Resize(ref healthInstances, next);
        }

        private void EnsureResolvedSpriteCapacity(int required)
        {
            if (required <= resolvedSprites.Length)
                return;
            int next = Mathf.NextPowerOfTwo(Mathf.Max(1, required));
            Array.Resize(ref resolvedSprites, next);
        }

        private void EnsureOwnedSpriteCacheCapacity(int required)
        {
            if (required <= ownedSpriteCaches.Length)
                return;
            int next = Mathf.NextPowerOfTwo(Mathf.Max(1, required));
            Array.Resize(ref ownedSpriteCaches, next);
        }

        private void DisposeUnusedOwnedSpriteCaches(int actorCount)
        {
            for (int index = actorCount; index < ownedSpriteCaches.Length; index++)
            {
                ownedSpriteCaches[index]?.Dispose();
                ownedSpriteCaches[index] = null;
            }
        }

        private Sprite ResolveActorSprite(
            BattleCentralEditorPreviewActor actor,
            int actorIndex)
        {
            Sprite resolved = ResolveNonTextureActorSprite(actor);
            if (resolved != null || actor == null || actor.SourceSheet == null)
            {
                ownedSpriteCaches[actorIndex]?.Dispose();
                ownedSpriteCaches[actorIndex] = null;
                return resolved;
            }

#if UNITY_EDITOR
            OwnedSpriteCache cache = ownedSpriteCaches[actorIndex];
            if (cache != null && cache.Matches(actor))
                return cache.Sprite;
            cache?.Dispose();
            cache = OwnedSpriteCache.Create(actor);
            ownedSpriteCaches[actorIndex] = cache;
            return cache?.Sprite;
#else
            return null;
#endif
        }

        private static Sprite ResolveNonTextureActorSprite(
            BattleCentralEditorPreviewActor actor)
        {
            if (actor == null)
                return null;
            if (actor.Sprite != null)
                return actor.Sprite;
            if (!actor.ResolveFromCharacterManager)
                return null;

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            LF2FrameData frame = manager?.GetFrameData(actor.ObjectId, actor.FrameId);
            List<Sprite> sprites = manager?.GetCharacterSpriteByID(actor.ObjectId);
            int pic = frame?.pic ?? -1;
            return sprites != null && (uint)pic < (uint)sprites.Count
                ? sprites[pic]
                : null;
        }

        private void InvalidateAndRepaint()
        {
            hasBuiltSignature = false;
            RepaintEditorViews();
        }

        private void DisposeResources()
        {
            if (resourcesDisposed)
                return;
            resourcesDisposed = true;
            actorBackend.Dispose();
            healthBackend.Dispose();
            DisposeUnusedOwnedSpriteCaches(0);
            previewFrame.ReleasePublicationBinding();
            resolvedMaterial = null;
        }

        private static Rect NormalizedUv(Sprite sprite)
        {
            Rect textureRect = sprite.textureRect;
            Texture2D texture = sprite.texture;
            return new Rect(
                textureRect.x / texture.width,
                textureRect.y / texture.height,
                textureRect.width / texture.width,
                textureRect.height / texture.height);
        }

        private static void RemoveDeadRegistrations()
        {
            for (int index = RegisteredPreviews.Count - 1; index >= 0; index--)
            {
                if (RegisteredPreviews[index] == null)
                    RegisteredPreviews.RemoveAt(index);
            }
        }

        private static void RepaintEditorViews()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                SceneView.RepaintAll();
#endif
        }

#if UNITY_EDITOR
        private sealed class ExclusiveValidationScope : IDisposable
        {
            private BattleCentralEditorPreview preview;

            public ExclusiveValidationScope(BattleCentralEditorPreview value)
            {
                preview = value;
            }

            public void Dispose()
            {
                BattleCentralEditorPreview current = preview;
                preview = null;
                if (exclusiveValidationPreview == current)
                    exclusiveValidationPreview = null;
            }
        }
#endif

        private sealed class PreviewResourceResolver : IBattleCentralResourceResolver
        {
            private List<BattleCentralEditorPreviewActor> configuredActors;
            private Sprite[] configuredSprites;
            private Material configuredMaterial;

            public void Configure(
                List<BattleCentralEditorPreviewActor> actors,
                Sprite[] sprites,
                Material material)
            {
                configuredActors = actors;
                configuredSprites = sprites;
                configuredMaterial = material;
            }

            public BattleCentralResourceStatus Resolve(
                in BattleRenderCommand command,
                out BattleCentralResolvedResource resource)
            {
                resource = default;
                int actorIndex = command.VisualDataId;
                if (configuredActors == null || (uint)actorIndex >= (uint)configuredActors.Count)
                    return BattleCentralResourceStatus.UnresolvedVisual;
                BattleCentralEditorPreviewActor actor = configuredActors[actorIndex];
                Sprite sprite = configuredSprites != null &&
                                (uint)actorIndex < (uint)configuredSprites.Length
                    ? configuredSprites[actorIndex]
                    : null;
                if (sprite == null || sprite.texture == null || configuredMaterial == null)
                    return BattleCentralResourceStatus.UnresolvedVisual;

                Vector2 pixelSize = sprite.rect.size;
                if (pixelSize.x <= 0f || pixelSize.y <= 0f)
                    return BattleCentralResourceStatus.UnresolvedVisual;
                Vector2 pivot = new Vector2(
                    sprite.pivot.x / pixelSize.x,
                    sprite.pivot.y / pixelSize.y);
                resource = new BattleCentralResolvedResource(
                    sprite.texture,
                    configuredMaterial,
                    NormalizedUv(sprite),
                    pixelSize,
                    pivot,
                    actor.Tint);
                return BattleCentralResourceStatus.Resolved;
            }
        }

        private sealed class OwnedSpriteCache : IDisposable
        {
            private readonly int sourceInstanceId;
            private readonly RectInt sourceRectPixels;
            private readonly Vector2 sourcePivot;
            private Texture2D texture;
            private Sprite sprite;

            private OwnedSpriteCache(
                int sourceInstanceId,
                RectInt sourceRectPixels,
                Vector2 sourcePivot)
            {
                this.sourceInstanceId = sourceInstanceId;
                this.sourceRectPixels = sourceRectPixels;
                this.sourcePivot = sourcePivot;
            }

            public Sprite Sprite => sprite;

            public bool Matches(BattleCentralEditorPreviewActor actor)
            {
                return actor != null && actor.SourceSheet != null &&
                       sourceInstanceId == actor.SourceSheet.GetInstanceID() &&
                       sourceRectPixels.Equals(actor.SourceRectPixels) &&
                       sourcePivot == actor.SourcePivot;
            }

#if UNITY_EDITOR
            public static OwnedSpriteCache Create(BattleCentralEditorPreviewActor actor)
            {
                if (actor?.SourceSheet == null)
                    return null;
                var cache = new OwnedSpriteCache(
                    actor.SourceSheet.GetInstanceID(),
                    actor.SourceRectPixels,
                    actor.SourcePivot);
                try
                {
                    string assetPath = AssetDatabase.GetAssetPath(actor.SourceSheet);
                    if (string.IsNullOrEmpty(assetPath))
                        return cache;
                    string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                    string absolutePath = Path.GetFullPath(Path.Combine(projectRoot, assetPath));
                    BMPLoader.BmpData bmpData = BMPLoader.LoadBmpData(absolutePath);
                    if (bmpData?.Pixels == null || bmpData.Width <= 0 || bmpData.Height <= 0)
                        return cache;

                    var sourcePixels = new Color32[bmpData.Pixels.Length];
                    for (int index = 0; index < sourcePixels.Length; index++)
                        sourcePixels[index] = bmpData.Pixels[index];
                    Color32[] processedPixels =
                        RuntimeSpriteProcessor.ProcessSheetPixelsFast(sourcePixels);
                    if (processedPixels == null ||
                        processedPixels.Length != bmpData.Width * bmpData.Height)
                    {
                        return cache;
                    }

                    cache.texture = new Texture2D(
                        bmpData.Width,
                        bmpData.Height,
                        TextureFormat.RGBA32,
                        false)
                    {
                        name = actor.SourceSheet.name + " Editor Preview",
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        hideFlags = HideFlags.HideAndDontSave,
                    };
                    cache.texture.SetPixels32(processedPixels);
                    cache.texture.Apply(false, true);

                    RectInt configuredRect = actor.SourceRectPixels;
                    int width = configuredRect.width > 0
                        ? configuredRect.width
                        : bmpData.Width;
                    int height = configuredRect.height > 0
                        ? configuredRect.height
                        : bmpData.Height;
                    int x = Mathf.Clamp(configuredRect.x, 0, bmpData.Width - 1);
                    int y = Mathf.Clamp(configuredRect.y, 0, bmpData.Height - 1);
                    width = Mathf.Clamp(width, 1, bmpData.Width - x);
                    height = Mathf.Clamp(height, 1, bmpData.Height - y);
                    cache.sprite = UnityEngine.Sprite.Create(
                        cache.texture,
                        new Rect(x, y, width, height),
                        new Vector2(
                            Mathf.Clamp01(actor.SourcePivot.x),
                            Mathf.Clamp01(actor.SourcePivot.y)),
                        100f,
                        0,
                        SpriteMeshType.FullRect);
                    cache.sprite.name = actor.SourceSheet.name + " Editor Preview Frame";
                    cache.sprite.hideFlags = HideFlags.HideAndDontSave;
                    return cache;
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(
                        $"[BattleCentralEditorPreview] Failed to decode preview sheet " +
                        $"'{actor.SourceSheet.name}': {exception.Message}");
                    cache.Dispose();
                    return new OwnedSpriteCache(
                        actor.SourceSheet.GetInstanceID(),
                        actor.SourceRectPixels,
                        actor.SourcePivot);
                }
            }
#endif

            public void Dispose()
            {
                Sprite targetSprite = sprite;
                Texture2D targetTexture = texture;
                sprite = null;
                texture = null;
                if (targetSprite != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(targetSprite);
                    else
                        UnityEngine.Object.DestroyImmediate(targetSprite);
                }
                if (targetTexture != null)
                {
                    if (Application.isPlaying)
                        UnityEngine.Object.Destroy(targetTexture);
                    else
                        UnityEngine.Object.DestroyImmediate(targetTexture);
                }
            }
        }
    }
}
