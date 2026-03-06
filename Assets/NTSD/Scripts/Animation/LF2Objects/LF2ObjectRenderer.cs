using UnityEngine;
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

        // ========== 逻辑层引用 ==========
        // 所有实际对象都继承 LF2LivingObject，用其作为字段类型以访问 PS/Frame/Sprite
        private LF2LivingObject _logicObject;

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
            UpdateSprite();
            UpdatePosition();
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
            // Wire SpriteRenderer into the Sprite module so direction flips work.
            // Sprites list is null until a sprite-loading system is built; ShowPic() will no-op.
            _logicObject?.Sprite?.Initialize(_spriteRenderer, null);
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
            float worldX = ps.x / ppu;
            float worldY = ps.z / ppu - ps.y / ppu;  // FLF y 为负时对象在空中，偏移为正
            transform.position = new Vector3(worldX, worldY, transform.position.z);
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
