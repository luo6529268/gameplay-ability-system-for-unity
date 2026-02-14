using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 对齐 FLF 的 itr:kind:14（blocking）概念：用于阻挡角色在 X/Z（Unity X/Y ground plane）上的移动。
    /// 
    /// 注意：
    /// - 不依赖 Unity Collider（避免大量实体时的物理开销，方便未来迁移 ECS）。
    /// - 仅提供一个数据驱动的阻挡 AABB（用于查询），不参与 Unity 物理模拟。
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class LF2BlockingObstacle : MonoBehaviour
    {
        [Header("DAT Frame Data (Source of ITR)")]
        [SerializeField] private LF2FrameData _frameData;

        [Header("Sprite Metrics (Pixels)")]
        [Tooltip("用于 mirror 计算（dir==left 时 localX = spriteWidth - itr.x - itr.w）。后续可从资源/配置自动读取。")]
        [SerializeField] private float _spriteWidthPx = 0f;

        [Header("Facing")]
        [SerializeField] private bool _facingLeft = false;

        /// <summary>
        /// 动态生成对象的对外初始化入口：实例化后赋值即可。
        /// 不做资源读取，不做解析，仅缓存必要字段用于 blocking_xz 查询。
        /// </summary>
        public void Configure(LF2FrameData frameData, float spriteWidthPx, bool facingLeft)
        {
            _frameData = frameData;
            _spriteWidthPx = spriteWidthPx;
            _facingLeft = facingLeft;
        }

        public void SetFrameData(LF2FrameData frameData) => _frameData = frameData;
        public void SetSpriteWidthPx(float spriteWidthPx) => _spriteWidthPx = spriteWidthPx;
        public void SetFacingLeft(bool facingLeft) => _facingLeft = facingLeft;

        public bool IsConfigured => IsValidForBlocking();

        private void OnEnable()
        {
            SimulationTickDriver.Instance?.World?.SceneQuery?.RegisterBlockingObstacle(this);
        }

        private void OnDisable()
        {
            SimulationTickDriver.Instance?.World?.SceneQuery?.UnregisterBlockingObstacle(this);
        }

        internal bool IsValidForBlocking()
        {
            return _frameData != null && _frameData.itrs != null && _frameData.itrs.Count > 0 && _spriteWidthPx > 0f;
        }

        internal int FillItr14Volumes(System.Collections.Generic.List<PhysicsState.FlfVolume> dst)
        {
            if (dst == null) return 0;
            dst.Clear();

            if (!IsValidForBlocking()) return 0;

            // 将 Unity ground plane (X/Y) 映射为 FLF (x/z) 像素坐标
            float xPx = transform.position.x * SimulationConstants.PIXELS_PER_UNIT;
            float zPx = transform.position.y * SimulationConstants.PIXELS_PER_UNIT;

            // 对齐 FLF mechanics.js:set_pos 计算 sx/sy/sz（只需要用于生成 volume 的 origin）
            float sx = !_facingLeft
                ? (xPx - _frameData.centerx)
                : (xPx + _frameData.centerx - _spriteWidthPx);
            float sy = 0f - _frameData.centery;
            float sz = zPx;

            for (int i = 0; i < _frameData.itrs.Count; i++)
            {
                var itr = _frameData.itrs[i];
                if (itr == null) continue;
                if (itr.kind != 14) continue; // FLF: tag 'itr:14' = blocking

                float localX = itr.x;
                if (_facingLeft)
                {
                    localX = _spriteWidthPx - itr.x - itr.w;
                }

                dst.Add(new PhysicsState.FlfVolume(
                    sx, sy, sz,
                    localX, itr.y,
                    itr.w, itr.h,
                    itr.zwidth
                ));
            }

            return dst.Count;
        }

        private void OnDrawGizmosSelected()
        {
            // 编辑器可视化：以 itr:14 的近似包围盒显示（不参与运行时逻辑）
            if (_frameData == null) return;
            if (_frameData.itrs == null || _frameData.itrs.Count == 0) return;
            if (_spriteWidthPx <= 0f) return;

            float xPx = transform.position.x * SimulationConstants.PIXELS_PER_UNIT;
            float zPx = transform.position.y * SimulationConstants.PIXELS_PER_UNIT;

            float sx = !_facingLeft
                ? (xPx - _frameData.centerx)
                : (xPx + _frameData.centerx - _spriteWidthPx);
            float sy = 0f - _frameData.centery;
            float sz = zPx;

            for (int i = 0; i < _frameData.itrs.Count; i++)
            {
                var itr = _frameData.itrs[i];
                if (itr == null || itr.kind != 14) continue;

                float localX = itr.x;
                if (_facingLeft) localX = _spriteWidthPx - itr.x - itr.w;

                // FlfVolume 的矩形是 (x+vx, y+vy, w, h)，这里转换到 Unity world (X/Y)
                float leftPx = sx + localX;
                float topPx = sy + itr.y;
                float wPx = itr.w;
                float hPx = itr.h;

                Vector3 center = new Vector3(
                    (leftPx + wPx * 0.5f) / SimulationConstants.PIXELS_PER_UNIT,
                    (sz + (topPx + hPx * 0.5f)) / SimulationConstants.PIXELS_PER_UNIT,
                    0f
                );
                Vector3 size = new Vector3(
                    wPx / SimulationConstants.PIXELS_PER_UNIT,
                    hPx / SimulationConstants.PIXELS_PER_UNIT,
                    0f
                );

                Gizmos.color = new Color(1f, 0.35f, 0.15f, 0.25f);
                Gizmos.DrawCube(center, size);
                Gizmos.color = new Color(1f, 0.35f, 0.15f, 0.9f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
