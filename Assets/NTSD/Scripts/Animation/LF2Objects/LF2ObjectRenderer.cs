using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;
using MoreMountains.Tools;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象渲染器，负责把逻辑层实体的当前帧、朝向和 C++ 像素坐标同步到 Unity SpriteRenderer。
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class LF2ObjectRenderer : MonoBehaviour, ISimObject
    {
        // ========== 组件引用 ==========
        private SpriteRenderer _spriteRenderer;
        private SpriteRenderer _shadowRenderer;
        private Transform _visualTransform;

        // ========== 逻辑层引用 ==========
        private LF2Entity _logicObject;

        // 渲染帧计数器，对齐 C++ release 的 dword_449098。
        private int _renderFrameCount = 0;

        // 缓存稳定 ID，AllocateStableId 只调用一次。
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
                    _stableId = SimulationTickDriver.Instance?.World?.AllocateStableId() ?? GetInstanceID();
                }

                return _stableId;
            }
        }

        // ========== 生命周期 ==========

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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

            int firstPresentationTick = _logicObject.Runtime?.FirstPresentationTick ?? 0;
            if (tickIndex < firstPresentationTick)
            {
                HidePresentation();
                return;
            }

            UpdateSprite();
            UpdatePosition();
            _logicObject.ReleaseForcedRuntimeIntPositionAfterFirstPresentation(tickIndex);
            ApplyVisualShake();
        }

        /// <summary>
        /// opoint 刚生成对象时，逻辑帧和表现对象需要在同一个模拟时刻完成同步。
        /// </summary>
        public void ForceRefreshPresentation()
        {
            if (_logicObject == null) return;
            int currentTick = _logicObject?.Match?.CurrentTickIndex ?? 0;
            int firstPresentationTick = _logicObject.Runtime?.FirstPresentationTick ?? 0;
            if (currentTick < firstPresentationTick)
            {
                HidePresentation();
                return;
            }

            UpdateSprite();
            UpdatePosition();
            _logicObject.ReleaseForcedRuntimeIntPositionAfterFirstPresentation(currentTick);
        }

        // ========== 核心方法 ==========

        /// <summary>
        /// 设置逻辑对象并初始化对应的 Sprite 资源。
        /// </summary>
        public void SetLogicObject(ILF2Object logicObject, LF2TaskBase task)
        {
            RestorePooledVisualState();
            _logicObject = logicObject as LF2Entity;
            _renderFrameCount = 0;
            _logicObject?.Init(task, this);

            List<Sprite> sprites = null;
            int startFrame = 0;
            if (_logicObject != null)
            {
                CharacterAnimtorManager.Instance?.TryGetSprites(_logicObject.ObjectId, out sprites);
                startFrame = CharacterAnimtorManager.Instance?.GetStartFrame(_logicObject.ObjectId) ?? 0;
            }
            if (sprites != null)
                _logicObject?.Sprite?.Initialize(_spriteRenderer, sprites, startFrame);
            _logicObject?.Sprite?.InitializeShadow(_shadowRenderer);
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

            if (_spriteRenderer != null)
            {
                _spriteRenderer.enabled = true;
                _spriteRenderer.color = Color.white;
                _spriteRenderer.flipX = false;
            }
        }

        private void HidePresentation()
        {
            _logicObject?.Sprite?.SetPresentationSuppressed(true);
            _logicObject?.Sprite?.Hide();
            _logicObject?.Sprite?.HideShadow();
        }

        public void SetShadowRenderer(SpriteRenderer shadowRenderer)
        {
            _shadowRenderer = shadowRenderer;
            _logicObject?.Sprite?.InitializeShadow(shadowRenderer);
            _logicObject?.SetShadowRenderer(shadowRenderer);
        }

        /// <summary>
        /// 重置状态，归还对象池前调用。
        /// </summary>
        public void ResetState()
        {
            _logicObject?.UnregisterFromWorld();
            _logicObject?.Reset();
            _logicObject = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 按当前帧 pic 和运行时方向刷新 Unity SpriteRenderer。
        /// </summary>
        private void UpdateSprite()
        {
            if (_logicObject == null) return;
            var frame = _logicObject.Frame?.D;
            if (frame == null)
            {
                // C++ 侧 frame 已经切到 1000/无效帧时，不应继续保留上一张图。
                _logicObject.Sprite?.Hide();
                _logicObject.Sprite?.HideShadow();
                return;
            }
            if (_logicObject.Sprite == null) return;
            if (!_logicObject.Sprite.HasRenderer) return;
            _logicObject.Sprite.SetPresentationSuppressed(false);
            _logicObject.Sprite.ShowPic(_logicObject.GetRenderPicIndex());
            var ps = _logicObject.PS;
            if (ps != null)
                _logicObject.Sprite.SwitchLR(ps.dir);
        }

        /// <summary>
        /// 同步 Transform 位置。
        /// C++ release draw_entity 使用绘制矩形：
        /// 朝右 dst.x = x - centerx，朝左 dst.x = x - (frame_w - centerx)，dst.y = z + y - centery。
        /// Unity 运行时 Sprite 的 pivot 是底部中心，因此这里把 C++ 绘制矩形换算为底部中心点。
        /// </summary>
        private void UpdatePosition()
        {
            if (_logicObject == null) return;
            var ps = _logicObject.PS;
            if (ps == null) return;

            ApplyCppDrawEntityPosition(ps);
            _logicObject.Sprite?.SetZ(_logicObject.GetDisplayZ() + ps.zz);

            // 阴影按 C++ 逻辑坐标 x/z 独立更新，不跟随图片 pivot。
            _logicObject.UpdateShadow(_renderFrameCount);
        }

        private void ApplyCppDrawEntityPosition(PhysicsState ps)
        {
            var frame = _logicObject.Frame?.D;

            float spriteWidth = _logicObject.GetSpriteWidthPxForRender();
            float spriteHeight = _logicObject.GetSpriteHeightPxForRender();
            float centerx = frame?.centerx ?? 0f;
            float centery = frame?.centery ?? 0f;

            // C++ release draw_entity 使用的是 x_int / y_int / z_int。
            // 这里不能直接吃 Unity 侧的浮点逻辑坐标，否则同一实体在出生后续拍、
            // 摩擦衰减和 type=3/oid=999 这类路径上会出现和正式版不一致的像素漂移。
            var runtime = _logicObject.Runtime;
            int drawX = _logicObject.GetRuntimeXInt() + Mathf.RoundToInt(_logicObject.GetRenderOffsetX());
            int drawY = _logicObject.GetRuntimeYInt();
            int drawDisplayZ = _logicObject.GetRenderZInt();

            float drawLeft = ps.dir == "left"
                ? drawX - (spriteWidth - centerx)
                : drawX - centerx;
            float drawTop = drawDisplayZ + drawY - centery;
            float pivotX = drawLeft + spriteWidth * 0.5f;
            float pivotScreenY = drawTop + spriteHeight;

            Transform rootTransform = transform.parent != null ? transform.parent : transform;
            rootTransform.localScale = NTSDRenderSpace.RenderScale;
            Vector3 worldPos = NTSDRenderSpace.ScreenPixelToWorld(pivotX, pivotScreenY, rootTransform.position.z);
            rootTransform.position = NTSDRenderSpace.SnapWorldPosition(worldPos);

            if (_visualTransform != null)
                _visualTransform.localPosition = Vector3.zero;
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
            return _spriteRenderer.sprite.textureRect.width;
        }
    }
}
