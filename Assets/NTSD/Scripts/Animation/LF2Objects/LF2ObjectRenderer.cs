using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2 对象渲染器（MonoBehaviour 层）
    /// 职责：
    /// 1. 每帧更新 SpriteRenderer 的 sprite（SimLateTick）
    /// 2. 每帧同步 Transform 位置（从 Animator.ps）
    /// 3. 持有逻辑层对象引用（但不负责其生命周期管理）
    ///
    /// 生命周期：固定 SimOrder=100，只参与 LateTick 阶段（渲染更新）
    /// 逻辑层对象（LF2SpecialAttack 等）在 SimOrder=20/30/40 的阶段执行逻辑
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

        // 渲染帧计数器（对应反汇编 dword_449098，每渲染帧递增）
        private int _renderFrameCount = 0;

        // 缓存稳定ID（AllocateStableId 只调用一次）
        private int _stableId = 0;

        // ========== 公开属性 ==========
        public ILF2Object LogicObject => _logicObject;

        // ========== ISimObject 实现 ==========

        /// <summary>
        /// 渲染层固定 SimOrder=100（在所有逻辑之后）
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

        public void OnAdded(SimContext ctx) { }

        public void OnRemoved(SimContext ctx) { }

        public void SimTransit(int tickIndex) { }

        public void SimTU(int tickIndex) { }

        public void SimLateTick(int tickIndex)
        {
            if (_logicObject == null) return;
            if (!(_logicObject is LF2Character) && _renderFrameCount < 3)
                UnityEngine.Debug.Log($"[SimLateTick] oid={_logicObject.ObjectId} tick={tickIndex}");
            UpdateSprite();
            UpdatePosition();
            ApplyVisualShake();
        }

        // ========== 核心方法 ==========

        /// <summary>
        /// 设置逻辑对象并初始化
        /// 由 LF2ObjectFactory 调用
        /// </summary>
        public void SetLogicObject(ILF2Object logicObject, LF2TaskBase task)
        {
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
        }

        public void SetShadowRenderer(SpriteRenderer shadowRenderer)
        {
            _shadowRenderer = shadowRenderer;
            _logicObject?.Sprite?.InitializeShadow(shadowRenderer);
            _logicObject?.SetShadowRenderer(shadowRenderer);
        }

        /// <summary>
        /// 重置状态（归还对象池前调用）
        /// </summary>
        public void ResetState()
        {
            _logicObject?.UnregisterFromWorld();
            _logicObject?.Reset();
            _logicObject = null;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// 更新 sprite（从 CurrentFrame.pic 和 PS.dir）
        /// 对应 FLF sp.show_pic / sp.switch_lr
        /// </summary>
        private void UpdateSprite()
        {
            if (_logicObject == null) return;
            var frame = _logicObject.Frame?.D;
            if (frame == null) return;
            if (_logicObject.Sprite == null) return;
            if (!_logicObject.Sprite.HasRenderer) return;
            _logicObject.Sprite.ShowPic(frame.pic);
            var ps = _logicObject.PS;
            if (ps != null)
                _logicObject.Sprite.SwitchLR(ps.dir);
        }

        /// <summary>
        /// 同步 Transform 位置（从 PS 像素坐标转换到 Unity 世界坐标）
        /// 严格对齐 Sibling 结构：
        /// 1. Root (EntityObject): 负责地面坐标 px, pz
        /// 2. Model (EntityModel): 负责视觉高度 py 和中心点偏移 cx, cy
        /// </summary>
        private void UpdatePosition()
        {
            if (_logicObject == null) return;
            var ps = _logicObject.PS;
            if (ps == null) return;

            const float ppu = 100f;

            var frame = _logicObject.Frame?.D;
            float cx = frame?.centerx ?? 0f;
            float cy = frame?.centery ?? 0f;

            // 1. Root 节点始终保持在地面 (px, pz)
            Transform rootTransform = transform.parent != null ? transform.parent : transform;
            if (_logicObject is LF2Character)
            {
                Vector2 groundPlanePos = ps.GetGroundPoint2D();
                rootTransform.position = new Vector3(
                    Mathf.Round(groundPlanePos.x * SimulationConstants.PIXELS_PER_UNIT) / SimulationConstants.PIXELS_PER_UNIT,
                    Mathf.Round(groundPlanePos.y * SimulationConstants.PIXELS_PER_UNIT) / SimulationConstants.PIXELS_PER_UNIT,
                    rootTransform.position.z
                );

                float visualYOffset = -ps.y / SimulationConstants.PIXELS_PER_UNIT;
                float snappedVisualY = Mathf.Round(visualYOffset * SimulationConstants.PIXELS_PER_UNIT) / SimulationConstants.PIXELS_PER_UNIT;
                if (_visualTransform != null)
                {
                    _visualTransform.localPosition = new Vector3(0f, snappedVisualY, 0f);
                }

                if (_renderFrameCount < 2)
                    UnityEngine.Debug.Log($"[Char] px={ps.x:F0} pz={ps.z:F0} worldX={groundPlanePos.x:F2} worldY={groundPlanePos.y:F2} localY={snappedVisualY:F2} finalY={groundPlanePos.y + snappedVisualY:F2}");
            }
            else
            {
                float worldX = ps.x / ppu;
                float worldY = (ps.z - ps.y) / ppu;
                rootTransform.position = new Vector3(worldX, worldY, rootTransform.position.z);

                if (_visualTransform != null)
                {
                    _visualTransform.localPosition = new Vector3(-cx / ppu, -cy / ppu, 0);
                }

                if (_renderFrameCount < 2)
                {
                    string pname = transform.parent != null ? transform.parent.name : "null";
                    string gpname = (transform.parent != null && transform.parent.parent != null) ? transform.parent.parent.name : "null";
                    UnityEngine.Debug.Log($"[Pos] oid={_logicObject.ObjectId} px={ps.x:F0} py={ps.y:F0} pz={ps.z:F0} rootY={worldY:F2} localY={(-cy/ppu):F2} finalY={worldY + (-cy/ppu):F2}");
                }
            }
            _logicObject.Sprite?.SetZ(ps.z + ps.zz);

            // 阴影由 LF2Entity.UpdateShadow() 处理（保持在 Root 的本地零点）
            _logicObject.UpdateShadow(_renderFrameCount);
        }

        private void ApplyVisualShake()
        {
            _renderFrameCount++;
        }

        // ========== 辅助方法 ==========

        /// <summary>
        /// 获取当前 sprite 宽度（用于 make_point）
        /// </summary>
        public float GetSpriteWidth()
        {
            return _spriteRenderer.sprite.textureRect.width;
        }
    }
}
