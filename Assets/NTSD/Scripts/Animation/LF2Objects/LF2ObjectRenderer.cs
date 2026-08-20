using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Animation.Rendering;
using NTSD.Simulation;
using MoreMountains.Tools;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象渲染器，负责把逻辑层实体的当前帧、朝向和 C++ 像素坐标同步到 Unity SpriteRenderer。
    /// </summary>
    public class LF2ObjectRenderer : MonoBehaviour, ISimObject
    {
        // ========== 组件引用 ==========
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _shadowRenderer;
        private Transform _visualTransform;
        private Material _defaultSpriteSharedMaterial;
        private Material _defaultShadowSharedMaterial;

        // ========== 逻辑层引用 ==========
        private LF2Entity _logicObject;
        private int _boundSpriteObjectId = int.MinValue;
        private BattleSpriteCatalog _boundSpriteCatalog;
        private CharacterAnimtorManager _catalogBindingManager;

        // 渲染帧计数器，对齐 C++ release 的 dword_449098。
        private int _renderFrameCount = 0;

        // Renderer identity is presentation-only. It must never consume the
        // deterministic logic-entity StableId sequence owned by SimulationWorld.
        [SerializeField][MMReadOnly]private int _stableId = 0;

        // ========== 公开属性 ==========
        public ILF2Object LogicObject => _logicObject;

        // ========== ISimObject 实现 ==========

        /// <summary>
        /// 渲染层固定在所有逻辑对象之后执行。
        /// </summary>
        public int SimOrder => SimOrderConstants.Renderer;

        public int StableId
        {
            get
            {
                if (_stableId == 0)
                {
                    _stableId = GetInstanceID();
                }

                return _stableId;
            }
        }

        // ========== 生命周期 ==========

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _defaultSpriteSharedMaterial = ResolveBorrowedDefaultSharedMaterial(_spriteRenderer);
            NormalizeSpriteRendererState(_spriteRenderer, _defaultSpriteSharedMaterial);
            _visualTransform = this.transform;
        }

        private void OnEnable()
        {
            SimulationTickDriver.Instance?.World?.Register(this);
        }

        private void OnDisable()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
        }

        public void SimLateTick(int tickIndex)
        {
            if (_logicObject == null) return;

            bool suppressLegacyMaterializers =
                BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(_logicObject.Match);
            _logicObject.Sprite?.SetLegacyRendererSuppressed(suppressLegacyMaterializers);

            int firstPresentationTick = _logicObject.Runtime?.FirstPresentationTick ?? 0;
            bool presentationBlocked = _logicObject.Runtime?.OidMergeDormant == true ||
                                       tickIndex < firstPresentationTick;
            if (suppressLegacyMaterializers)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(presentationBlocked);
                if (!presentationBlocked)
                {
                    UpdateCentralManagedSpriteState();
                    _logicObject.UpdateShadowManagedState();
                }
                ApplyVisualShake();
                return;
            }

            if (_logicObject.Runtime?.OidMergeDormant == true)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }

            if (tickIndex < firstPresentationTick)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }

            _logicObject.Sprite?.SetPresentationSuppressed(false);
            UpdateSprite();
            UpdatePosition(tickIndex);
            _logicObject.Match?.RecordLegacyEntityProbe(_logicObject, _spriteRenderer);
            ApplyVisualShake();
        }

        /// <summary>
        /// opoint 刚生成对象时，逻辑帧和表现对象需要在同一个模拟时刻完成同步。
        /// </summary>
        public void ForceRefreshPresentation()
        {
            if (_logicObject == null) return;
            int currentTick = _logicObject?.Match?.CurrentTickIndex ?? 0;
            bool suppressLegacyMaterializers =
                BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(_logicObject.Match);
            _logicObject.Sprite?.SetLegacyRendererSuppressed(suppressLegacyMaterializers);

            int firstPresentationTick = _logicObject.Runtime?.FirstPresentationTick ?? 0;
            bool presentationBlocked = _logicObject.Runtime?.OidMergeDormant == true ||
                                       currentTick < firstPresentationTick;
            if (suppressLegacyMaterializers)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(presentationBlocked);
                if (!presentationBlocked)
                {
                    UpdateCentralManagedSpriteState();
                    _logicObject.UpdateShadowManagedState();
                }
                return;
            }

            if (_logicObject.Runtime?.OidMergeDormant == true)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }
            if (currentTick < firstPresentationTick)
            {
                _logicObject.Sprite?.SetPresentationSuppressed(true);
                return;
            }

            _logicObject.Sprite?.SetPresentationSuppressed(false);
            UpdateSprite();
            UpdatePosition(currentTick);
            _logicObject.Match?.RecordLegacyEntityProbe(_logicObject, _spriteRenderer);
        }

        // ========== 核心方法 ==========

        /// <summary>
        /// 设置逻辑对象并初始化对应的 Sprite 资源。
        /// </summary>
        public void SetLogicObject(ILF2Object logicObject, LF2TaskBase task)
        {
            ReleaseCatalogBinding();
            RestorePooledVisualState();
            _logicObject = logicObject as LF2Entity;
            _renderFrameCount = 0;
            _logicObject?.Init(task, this);
            RefreshLegacyRendererSuppression(
                BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(
                    _logicObject?.Match));
            BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                this,
                ResolveCurrentRuntimeHandle(_logicObject));

            List<Sprite> sprites = null;
            int startFrame = 0;
            int visualDataId = int.MinValue;
            CharacterAnimtorManager animatorManager = CharacterAnimtorManager.Instance;
            if (_logicObject != null)
            {
                visualDataId = LF2Entity.ResolveCurrentDataObjectId(_logicObject);
                animatorManager?.TryGetSprites(visualDataId, out sprites);
                startFrame = animatorManager?.GetStartFrame(visualDataId) ?? 0;
            }
            if (sprites != null)
            {
                _logicObject?.Sprite?.Initialize(
                    _spriteRenderer,
                    sprites,
                    startFrame,
                    animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty,
                    visualDataId);
                _boundSpriteObjectId = visualDataId;
                UpdateCatalogBinding(animatorManager, animatorManager?.SpriteCatalog);
            }
            else
            {
                _logicObject?.Sprite?.Initialize(
                    _spriteRenderer,
                    null,
                    0,
                    animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty,
                    visualDataId);
                _logicObject?.Sprite?.ClearCurrentSprite();
                _boundSpriteObjectId = int.MinValue;
                UpdateCatalogBinding(animatorManager, animatorManager?.SpriteCatalog);
            }
            _logicObject?.SetShadowRenderer(_shadowRenderer);

            // 新生成对象先压住表现。
            // C++ release 的 late opoint / transition smoke 不允许在创建当拍先露一帧，
            // 必须等 FirstPresentationTick 到达后再由 ForceRefresh/SimLateTick 放行。
            _logicObject?.Sprite?.SetPresentationSuppressed(true);

            var frame = _logicObject?.Frame?.D;
            if (frame != null && _logicObject.Sprite != null)
                _logicObject.Sprite.ShowPic(_logicObject.GetRenderPicIndex());
        }

        /// <summary>
        /// 对象池复用时恢复 Unity 渲染组件状态，避免上一轮 Hide/Reset 留下不可见状态。
        /// </summary>
        public void RestorePooledVisualState()
        {
            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_defaultSpriteSharedMaterial == null)
                _defaultSpriteSharedMaterial = ResolveBorrowedDefaultSharedMaterial(_spriteRenderer);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
                NormalizeSpriteRendererState(_spriteRenderer, _defaultSpriteSharedMaterial);
            }
            NormalizeSpriteRendererState(_shadowRenderer, _defaultShadowSharedMaterial);
        }

        public void SetShadowRenderer(SpriteRenderer shadowRenderer)
        {
            _shadowRenderer = shadowRenderer;
            _defaultShadowSharedMaterial = ResolveBorrowedDefaultSharedMaterial(shadowRenderer);
            if (_shadowRenderer != null)
                _shadowRenderer.sortingLayerName = "Object";
            _logicObject?.SetShadowRenderer(shadowRenderer);
        }

        internal void RefreshLegacyRendererSuppression(bool suppressed)
        {
            _logicObject?.Sprite?.SetLegacyRendererSuppressed(suppressed);
        }

        /// <summary>
        /// 重置状态，归还对象池前调用。
        /// </summary>
        public void ResetState()
        {
            BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(this);
            _logicObject?.Sprite?.Reset();
            _logicObject?.SetShadowRenderer(null);
            if (_defaultSpriteSharedMaterial == null)
                _defaultSpriteSharedMaterial = ResolveBorrowedDefaultSharedMaterial(_spriteRenderer);
            NormalizeSpriteRendererState(_spriteRenderer, _defaultSpriteSharedMaterial);
            NormalizeSpriteRendererState(_shadowRenderer, _defaultShadowSharedMaterial);
            ReleaseCatalogBinding();
            _logicObject?.UnregisterFromWorld();
            _logicObject?.Reset();
            _logicObject = null;
            _boundSpriteObjectId = int.MinValue;
            gameObject.SetActive(false);
        }

        private static RuntimeEntityHandle ResolveCurrentRuntimeHandle(LF2Entity logicObject)
        {
            if (logicObject == null || logicObject.Runtime?.SlotIndex < 0)
                return RuntimeEntityHandle.Invalid;

            SimulationWorld world = logicObject.Match;
            return world != null && world.TryGetCurrentRuntimeHandle(
                logicObject.Runtime.SlotIndex,
                logicObject,
                out RuntimeEntityHandle handle)
                ? handle
                : RuntimeEntityHandle.Invalid;
        }

        internal static void NormalizeSpriteRendererState(
            SpriteRenderer renderer,
            Material borrowedDefaultSharedMaterial)
        {
            if (renderer == null)
                return;

            renderer.color = Color.white;
            renderer.flipX = false;
            renderer.flipY = false;
            renderer.maskInteraction = SpriteMaskInteraction.None;
            if (borrowedDefaultSharedMaterial != null)
                renderer.sharedMaterial = borrowedDefaultSharedMaterial;
        }

        internal static Material ResolveBorrowedDefaultSharedMaterial(SpriteRenderer renderer)
        {
            Material sharedMaterial = renderer != null ? renderer.sharedMaterial : null;
            return sharedMaterial != null && sharedMaterial.shader != null &&
                   sharedMaterial.shader.name == "Sprites/Default"
                ? sharedMaterial
                : null;
        }

        /// <summary>
        /// 按当前帧 pic 和运行时方向刷新 Unity SpriteRenderer。
        /// </summary>
        private void UpdateSprite()
        {
            if (_logicObject == null) return;
            bool shouldDrawForHitStop = ShouldDrawEntityForHitStop(_logicObject.Runtime?.HitStop ?? 0);
            _logicObject.Sprite?.SetLegacyEntityVisible(shouldDrawForHitStop);
            if (!shouldDrawForHitStop)
            {
                // C# release DrawEntity hides the entity for the negative HitStop
                // threshold and four-tick blink phase. This only changes presentation;
                // the runtime entity continues advancing normally.
                return;
            }

            EnsureRuntimeIdentitySprites();
            var frame = _logicObject.Frame?.D;
            if (frame == null)
            {
                // C++ 侧 frame 已经切到 1000/无效帧时，不应继续保留上一张图。
                _logicObject.Sprite?.Hide();
                _logicObject.Sprite?.HideShadow();
                return;
            }
            if (_logicObject.Sprite == null) return;
            _logicObject.Sprite.ShowPic(_logicObject.GetRenderPicIndex());
            var ps = _logicObject.PS;
            if (ps != null)
                _logicObject.Sprite.SwitchLR(ps.dir);
        }

        private void UpdateCentralManagedSpriteState()
        {
            if (_logicObject == null)
                return;

            LF2Sprite sprite = _logicObject.Sprite;
            bool shouldDrawForHitStop =
                ShouldDrawEntityForHitStop(_logicObject.Runtime?.HitStop ?? 0);
            sprite?.SetLegacyEntityVisibleManagedOnly(shouldDrawForHitStop);
            if (!shouldDrawForHitStop)
                return;

            EnsureRuntimeIdentitySprites(managedOnly: true);
            LF2FrameData frame = _logicObject.Frame?.D;
            if (frame == null)
            {
                sprite?.ClearCurrentSpriteManagedOnly();
                sprite?.SetShadowVisibleManagedOnly(false);
                return;
            }
            if (sprite == null)
                return;

            sprite.ShowPicManagedOnly(_logicObject.GetRenderPicIndex());
            PhysicsState ps = _logicObject.PS;
            if (ps != null)
                sprite.SwitchLRManagedOnly(ps.dir);
        }

        internal static bool ShouldDrawEntityForHitStop(int hitStop)
        {
            return hitStop > -25 && (System.Math.Abs((long)hitStop) % 4) < 2;
        }

        internal static bool ShouldDrawShadowForHitStop(int hitStop)
        {
            return hitStop > -70 && (System.Math.Abs((long)hitStop) % 4) < 2;
        }

        private void EnsureRuntimeIdentitySprites(bool managedOnly = false)
        {
            if (_logicObject == null || _logicObject.Sprite == null)
                return;

            int visualDataId = LF2Entity.ResolveCurrentDataObjectId(_logicObject);
            CharacterAnimtorManager animatorManager = CharacterAnimtorManager.Instance;
            BattleSpriteCatalog currentCatalog = animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty;
            if (_boundSpriteObjectId == visualDataId &&
                ReferenceEquals(_boundSpriteCatalog, currentCatalog))
                return;

            if (animatorManager != null &&
                animatorManager.TryGetSprites(visualDataId, out List<Sprite> sprites) &&
                sprites != null)
            {
                int startFrame = animatorManager.GetStartFrame(visualDataId);
                if (managedOnly)
                {
                    _logicObject.Sprite.SetSpritesManagedOnly(sprites, startFrame);
                    _logicObject.Sprite.SetCatalogBindingManagedOnly(
                        animatorManager.SpriteCatalog,
                        visualDataId);
                }
                else
                {
                    _logicObject.Sprite.SetSprites(sprites, startFrame);
                    _logicObject.Sprite.SetCatalogBinding(
                        animatorManager.SpriteCatalog,
                        visualDataId);
                }
                _boundSpriteObjectId = visualDataId;
                UpdateCatalogBinding(animatorManager, animatorManager.SpriteCatalog);
                return;
            }

            // Never render the previous identity's catalog while the new one is still unavailable.
            BattleSpriteCatalog fallbackCatalog =
                animatorManager?.SpriteCatalog ?? BattleSpriteCatalog.Empty;
            if (managedOnly)
            {
                _logicObject.Sprite.SetSpritesManagedOnly(null);
                _logicObject.Sprite.SetCatalogBindingManagedOnly(
                    fallbackCatalog,
                    visualDataId);
            }
            else
            {
                _logicObject.Sprite.SetSprites(null);
                _logicObject.Sprite.SetCatalogBinding(
                    fallbackCatalog,
                    visualDataId);
            }
            _boundSpriteObjectId = int.MinValue;
            UpdateCatalogBinding(animatorManager, currentCatalog);
        }

        private void UpdateCatalogBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog)
        {
            if (ReferenceEquals(_catalogBindingManager, manager) &&
                ReferenceEquals(_boundSpriteCatalog, catalog))
                return;

            ReleaseCatalogBinding();
            _catalogBindingManager = manager;
            _boundSpriteCatalog = catalog;
            manager?.RegisterRendererCatalogBinding(catalog);
        }

        private void ReleaseCatalogBinding()
        {
            _catalogBindingManager?.UnregisterRendererCatalogBinding(_boundSpriteCatalog);
            _catalogBindingManager = null;
            _boundSpriteCatalog = null;
        }

        private void OnDestroy()
        {
            BattleCentralPresentationMountRegistry.RemoveOwnerRuntimeBinding(this);
            ReleaseCatalogBinding();
        }

        /// <summary>
        /// 同步 Transform 位置。
        /// C++ release draw_entity 使用绘制矩形：
        /// 朝右 dst.x = x - centerx，朝左 dst.x = x - (frame_w - centerx)，dst.y = z + y - centery。
        /// Unity 运行时 Sprite 的 pivot 是底部中心，因此这里把 C++ 绘制矩形换算为底部中心点。
        /// </summary>
        private void UpdatePosition(int tickIndex)
        {
            if (_logicObject == null) return;
            var ps = _logicObject.PS;
            if (ps == null) return;

            ApplyCppDrawEntityPosition(ps, tickIndex);
            _logicObject.Sprite?.SetZ(_logicObject.GetDisplayRenderSortingOrder(
                _logicObject.GetDisplayZ(), ps.zz));

            // 阴影按 C++ 逻辑坐标 x/z 独立更新，不跟随图片 pivot。
            _logicObject.UpdateShadow(_renderFrameCount);
        }

        private void RefreshLegacySortingMetadata()
        {
            if (_logicObject == null) return;
            var ps = _logicObject.PS;
            if (ps == null) return;

            _logicObject.Sprite?.SetZ(_logicObject.GetDisplayRenderSortingOrder(
                _logicObject.GetDisplayZ(), ps.zz));
        }

        private void ApplyCppDrawEntityPosition(PhysicsState ps, int tickIndex)
        {
            var frame = _logicObject.Frame?.D;

            float spriteWidth = _logicObject.GetSpriteWidthPxForRender();
            float spriteHeight = _logicObject.GetSpriteHeightPxForRender();
            float centerx = frame?.centerx ?? 0f;
            float centery = frame?.centery ?? 0f;

            // C++ release draw_entity 使用的是 x_int / y_int / z_int。
            // 这里不能直接吃 Unity 侧的浮点逻辑坐标，否则同一实体在出生后续拍、
            // 摩擦衰减和 type=3/oid=999 这类路径上会出现和正式版不一致的像素漂移。
            int cameraX = _logicObject.Match?.ReleaseCameraX ?? 0;
            Vector2 pivot = ComputeEntityBottomCenterPivotPixels(
                _logicObject.GetRuntimeXInt(),
                _logicObject.GetRuntimeYInt(),
                _logicObject.GetDisplayZ(),
                _logicObject.GetRenderOffsetX(),
                cameraX,
                _logicObject.FrameDelay,
                tickIndex,
                ps.dir == "left",
                spriteWidth,
                spriteHeight,
                centerx,
                centery,
                NTSDRenderSpace.BattleVisualScale);
            pivot += ResolveHeldVisualAttachmentOffsetPixels(frame);

            Transform rootTransform = transform.parent != null ? transform.parent : transform;
            rootTransform.localScale = NTSDRenderSpace.RenderScale;
            Vector3 worldPos = NTSDRenderSpace.ScreenPixelToPresentationWorld(
                pivot.x,
                pivot.y,
                rootTransform.position.z);
            rootTransform.position = worldPos;

            if (_visualTransform != null && _visualTransform != rootTransform)
                _visualTransform.localPosition = Vector3.zero;
        }

        internal static Vector2 ComputeEntityBottomCenterPivotPixels(
            int xInt,
            int yInt,
            float displayZ,
            float renderOffsetX,
            int cameraX,
            int frameDelay,
            int tickIndex,
            bool facingLeft,
            float spriteWidth,
            float spriteHeight,
            float centerx,
            float centery,
            float visualScale)
        {
            int extraX = frameDelay < 0 ? 6 * (tickIndex & 1) - 3 : 0;
            int screenX = xInt + (int)renderOffsetX - cameraX + extraX;
            int screenY = (int)displayZ + yInt;
            float pivotX = facingLeft
                ? screenX + visualScale * (centerx - spriteWidth * 0.5f)
                : screenX + visualScale * (spriteWidth * 0.5f - centerx);
            float pivotY = screenY + visualScale * (spriteHeight - centery);
            return new Vector2(pivotX, pivotY);
        }

        private Vector2 ResolveHeldVisualAttachmentOffsetPixels(LF2FrameData heldFrame)
        {
            NTSDEntityRuntime heldRuntime = _logicObject.Runtime;
            int holderSlot = heldRuntime?.HolderStableId ?? -1;
            LF2Entity holder = _logicObject.Match?.FindEntityByRuntimeSlotForQuery(holderSlot);
            return ResolveHeldVisualAttachmentOffsetPixels(
                heldRuntime,
                heldFrame,
                holder,
                NTSDRenderSpace.BattleVisualScale);
        }

        internal static Vector2 ResolveHeldVisualAttachmentOffsetPixels(
            NTSDEntityRuntime heldRuntime,
            LF2FrameData heldFrame,
            LF2Entity holder,
            float visualScale)
        {
            NTSDEntityRuntime holderRuntime = holder?.Runtime;
            LF2FrameData holderFrame = holder?.Frame?.D;
            if (heldRuntime == null || heldRuntime.LinkState >= 0 || heldRuntime.SlotIndex < 0 ||
                holderRuntime == null || holderRuntime.SlotIndex != heldRuntime.HolderStableId ||
                holderRuntime.TargetSlotIndex != heldRuntime.SlotIndex ||
                holderFrame?.wpoints == null || holderFrame.wpoints.Count == 0 ||
                heldFrame?.wpoints == null || heldFrame.wpoints.Count == 0)
            {
                return Vector2.zero;
            }

            WeaponPoint holderWPoint = holderFrame.wpoints[0];
            WeaponPoint heldWPoint = heldFrame.wpoints[0];
            if (holderWPoint == null || heldWPoint == null)
                return Vector2.zero;

            return ComputeHeldVisualAttachmentOffsetPixels(
                holderRuntime.Dir == "left",
                holderFrame.centerx,
                holderFrame.centery,
                holderWPoint.x,
                holderWPoint.y,
                heldFrame.centerx,
                heldFrame.centery,
                heldWPoint.x,
                heldWPoint.y,
                visualScale);
        }

        internal static Vector2 ComputeHeldVisualAttachmentOffsetPixels(
            bool facingLeft,
            float holderCenterX,
            float holderCenterY,
            float holderWPointX,
            float holderWPointY,
            float heldCenterX,
            float heldCenterY,
            float heldWPointX,
            float heldWPointY,
            float visualScale)
        {
            float scaleDelta = visualScale - 1f;
            float holderDeltaX = holderWPointX - holderCenterX;
            float heldDeltaX = heldWPointX - heldCenterX;
            float x = scaleDelta * (holderDeltaX - heldDeltaX);
            if (facingLeft)
                x = -x;

            float holderDeltaY = holderWPointY - holderCenterY;
            float heldDeltaY = heldWPointY - heldCenterY;
            float y = scaleDelta * (holderDeltaY - heldDeltaY);
            return new Vector2(x, y);
        }

        private void ApplyVisualShake()
        {
            _renderFrameCount++;
        }

        // ========== 辅助方法 ==========

        /// <summary>
        /// 获取当前 Sprite 宽度，单位为像素。
        /// </summary>
        public float GetSpriteWidth()
        {
            return _logicObject?.GetSpriteWidthPxForRender() ?? 0f;
        }
    }
}
