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

        // ========== 逻辑层引用 ==========
        private LF2LivingObject _logicObject;

        // 渲染帧计数器（对应反汇编 dword_449098，每渲染帧递增）
        private int _renderFrameCount = 0;

        // ========== 公开属性 ==========
        public ILF2Object LogicObject => _logicObject;

        // ========== ISimObject 实现 ==========

        /// <summary>
        /// 渲染层固定 SimOrder=100（在所有逻辑之后）
        /// </summary>
        public int SimOrder => SimOrderConstants.Renderer;

        public int StableId => SimulationTickDriver.Instance?.World?.AllocateStableId() ?? 0;

        // ========== 生命周期 ==========

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            UnityEngine.Debug.Log($"[Renderer] Awake: {gameObject.name}");
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

        public void SimLateTick(int tickIndex) { }

        private void LateUpdate()
        {
            if (_logicObject == null) return;
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
            _logicObject = logicObject as LF2LivingObject;
            _logicObject?.Init(task, this);

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            List<Sprite> sprites = null;
            if (_logicObject != null)
                CharacterAnimtorManager.Instance?.TryGetSprites(_logicObject.ObjectId, out sprites);
            _logicObject?.Sprite?.Initialize(sr, sprites);
            _logicObject?.Sprite?.InitializeShadow(_shadowRenderer);
        }

        public void SetShadowRenderer(SpriteRenderer shadowRenderer)
        {
            _shadowRenderer = shadowRenderer;
            _logicObject?.Sprite?.InitializeShadow(shadowRenderer);
        }

        /// <summary>
        /// 重置状态（归还对象池前调用）
        /// </summary>
        public void ResetState()
        {
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

            _logicObject.Sprite?.ShowPic(frame.pic);
            var ps = _logicObject.PS;
            if (ps != null)
                _logicObject.Sprite?.SwitchLR(ps.dir);

            // 阴影显隐：对齐反汇编 RenderDispatch 0x0041D1C9-0x0041D20B
            // 条件：state != 3005 && state != 9997 && entity_type != 223/224 && y > -70 && renderFrame%4 < 2
            if (_shadowRenderer != null)
            {
                var ps2 = _logicObject.PS;
                bool hideShadow = frame.state == 3005
                                || frame.state == 9997
                                || (ps2 != null && ps2.y < -70f)
                                || (_logicObject.Effect?.Blink == true && (_renderFrameCount % 4) >= 2);
                _shadowRenderer.enabled = !hideShadow;
            }
        }

        /// <summary>
        /// 同步 Transform 位置（从 PS 像素坐标转换到 Unity 世界坐标）
        /// FLF 坐标系：x → Unity X，z(深度) → Unity Y（地面），y(跳跃，负数向上) → Unity Y 偏移
        /// </summary>
        private void UpdatePosition()
        {
            if (_logicObject == null) return;
            var ps = _logicObject.PS;
            if (ps == null) return;

            const float ppu = 100f;

            // 反汇编 RenderDispatch：
            // screen_x = entity.x - frame.centerx（sprite 左上角）
            // screen_y = entity.z + entity.y - frame.centery（地面深度 + 跳跃高度，ps.y 负数向上）
            var frame = _logicObject.Frame?.D;
            float cx = frame?.centerx ?? 0f;
            float cy = frame?.centery ?? 0f;

            float worldX = (ps.x - cx) / ppu;
            float worldY = (ps.z - ps.y - cy) / ppu;
            transform.position = new Vector3(worldX, worldY, transform.position.z);

            _logicObject.Sprite?.SetZ(ps.z + ps.zz);

            if (_shadowRenderer != null)
            {
                var st = _shadowRenderer.transform;
                // shadow pivot=(0.5,0.5)，sprite pivot=(0.5,0)
                // shadow 中心对齐 sprite 底部中心 = (worldX + cx/ppu, worldY)
                float shadowWorldX = worldX + cx / ppu;
                float shadowWorldY = worldY;
                st.position = new Vector3(shadowWorldX, shadowWorldY, st.position.z);
            }
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
