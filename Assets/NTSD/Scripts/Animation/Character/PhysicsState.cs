using UnityEngine;
using NTSD.Simulation;  // Step D8: 引用 SimulationConstants

namespace NTSD.Animation
{
    /// <summary>
    /// 物理状态对象 - 对应 FLF 的 $.ps
    ///
    /// 这个类是 FLF (Little Fighter 2) 物理系统的核心数据结构。
    /// 对应 FLF mechanics.js:272-288 的 mech.prototype.create_metric()
    ///
    /// **关键概念**：
    /// - 这是"意图速度"存储，不是立即应用的位置变化
    /// - 速度在 ApplyDynamics() 中被应用到位置：ps.x += ps.vx
    /// - 摩擦力和重力也在 ApplyDynamics() 中应用
    /// </summary>
    public class PhysicsState
    {
        /// <summary>
        /// FLF 默认攻击框 Z 轴宽度（像素）
        /// 对应 I:\C++Test\NTSD\F.LF-master\LF\global.js:130 (GC.default.itr.zwidth = 12)
        /// </summary>
        public static readonly float FLF_DEFAULT_ITR_ZWIDTH = NTSDGlobal.Default.Itr.ZWidth;

        // ==================== 位置（FLF 世界坐标，像素）====================
        /// <summary>
        /// 水平位置（对应 Unity transform.position.x）
        /// 单位：像素
        /// </summary>
        public float x;

        /// <summary>
        /// 垂直位置（跳跃高度）
        /// P2: 现在表示相对 groundY 的跳跃位移（像素）
        /// - 0 = 地面（与 groundY 相同高度）
        /// - 负值 = 在空中（向上为负，对应 FLF 的 y < 0）
        /// 实际 worldY = groundY + ps.y / PIXELS_PER_UNIT
        /// 单位：像素
        /// </summary>
        public float y;

        /// <summary>
        /// 地面参考高度（Unity world space Y 坐标，单位：Unity 单位）
        /// P2: 起跳前记录的 transform.position.y
        /// 落地判定：ps.y <= 0 时落地（worldY 回到 groundY）
        /// 注意：这是绝对世界坐标，与 ps.y（相对位移）不同
        /// </summary>
        public float groundY = 0f;

        /// <summary>
        /// 深度位置（对应 Unity transform.position.y）
        /// 单位：像素
        /// </summary>
        public float z;

        // ==================== 速度（像素/帧，30fps）====================
        /// <summary>
        /// 水平速度
        /// 单位：像素/帧（30fps）
        /// </summary>
        public float vx;

        /// <summary>
        /// 垂直速度（跳跃）
        /// - 负值 = 向上
        /// - 正值 = 向下（重力加速）
        /// 单位：像素/帧（30fps）
        /// </summary>
        public float vy;

        /// <summary>
        /// 深度速度
        /// 单位：像素/帧（30fps）
        /// </summary>
        public float vz;

        // ==================== 屏幕坐标（只读，用于渲染）====================
        /// <summary>
        /// 屏幕 X 坐标（Sprite 左上角）
        /// 对应 FLF mechanics.js:340
        /// 计算公式：ps.dir === 'right' ? (ps.x - fD.centerx) : (ps.x + fD.centerx - sp.w)
        /// </summary>
        public float sx;

        /// <summary>
        /// 屏幕 Y 坐标（Sprite 左上角）
        /// 对应 FLF mechanics.js:341
        /// 计算公式：ps.y - fD.centery
        /// </summary>
        public float sy;

        /// <summary>
        /// 屏幕 Z 坐标（用于排序）
        /// 对应 FLF mechanics.js:342
        /// 计算公式：ps.z
        /// </summary>
        public float sz;

        // ==================== 其他状态 ====================
        /// <summary>
        /// 朝向
        /// - "left" = 向左
        /// - "right" = 向右
        /// </summary>
        public string dir = "right";

        /// <summary>
        /// 摩擦系数（每帧重置为 1）
        /// 对应 FLF livingobject.js:114
        /// 在 TU_Update 开始时调用 ResetFriction()
        /// </summary>
        public float fric = 1f;

        /// <summary>
        /// Z 轴偏移（用于渲染层级调整）
        /// 例如：抓取其他角色时，调整被抓者的渲染层级
        /// </summary>
        public float zz = 0f;

        // ==================== 常量统一来源（Step D8）====================
        // ⚠️ 所有常量现在统一引用 SimulationConstants
        // - SIM_TICK_RATE = 30Hz（FLF 帧率）
        // - PIXELS_PER_UNIT = 100（Unity PPU 设置）
        // - 不再需要 FRAMERATE_SCALE（30/30 = 1.0，FLF 数据直接使用）

        // ==================== 公共方法 ====================

        /// <summary>
        /// 更新 Sprite 原点（屏幕坐标系）
        /// 对齐 FLF mechanics.js: ps.sx/ps.sy/ps.sz 的计算。
        /// </summary>
        public void UpdateSpriteOrigin(int centerx, int centery, float spriteWidthPx)
        {
            sx = dir == "right"
                ? (x - centerx)
                : (x + centerx - spriteWidthPx);

            sy = y - centery;
            sz = z;
        }

        // 获取水平方向
        public int Dirh()
        {
            return (dir == "left" ? -1 : 1);
        }

        /// <summary>
        /// FLF scene.query 所需的 volume 格式（见 LF/scene.js）。
        /// 注意：volume 的实际矩形为 (x+vx, y+vy, w, h)，深度区间为 [z-zwidth, z+zwidth]。
        /// </summary>
        public readonly struct FlfVolume
        {
            public readonly float x;
            public readonly float y;
            public readonly float z;
            public readonly float vx;
            public readonly float vy;
            public readonly float w;
            public readonly float h;
            public readonly float zwidth;

            public FlfVolume(float x, float y, float z, float vx, float vy, float w, float h, float zwidth)
            {
                this.x = x;
                this.y = y;
                this.z = z;
                this.vx = vx;
                this.vy = vy;
                this.w = w;
                this.h = h;
                this.zwidth = zwidth;
            }
        }

        /// <summary>
        /// 生成当前帧的 bdy 体积列表（严格对齐 FLF mechanics.js: mech.body_body()）。
        /// - bodies 缺失时返回 1 个 (w/h=0,zwidth=0) 的点体积（与 FLF 一致）
        /// - 朝左时按 sp.w 镜像 x：vx = sp.w - O.x - O.w
        /// - offset 用于预测体积（对齐 FLF mech.body(offset) 的行为：偏移加在 sprite origin 上）
        /// </summary>
        public System.Collections.Generic.List<FlfVolume> GetBodyVolumes(
            System.Collections.Generic.List<BodyBox> bodies,
            int centerx,
            int centery,
            float spriteWidthPx,
            float offsetX = 0f,
            float offsetY = 0f,
            float offsetZ = 0f)
        {
            UpdateSpriteOrigin(centerx, centery, spriteWidthPx);
            float originX = sx + offsetX;
            float originY = sy + offsetY;
            float originZ = sz + offsetZ;

            var result = new System.Collections.Generic.List<FlfVolume>();

            if (bodies == null || bodies.Count == 0)
            {
                result.Add(new FlfVolume(originX, originY, originZ, 0f, 0f, 0f, 0f, 0f));
                return result;
            }

            bool facingLeft = dir == "left";
            foreach (var body in bodies)
            {
                float localX = body.x;
                if (facingLeft)
                {
                    localX = spriteWidthPx - body.x - body.w;
                }

                result.Add(new FlfVolume(
                    originX, originY, originZ,
                    localX, body.y,
                    body.w, body.h,
                    FLF_DEFAULT_ITR_ZWIDTH
                ));
            }

            return result;
        }

        /// <summary>
        /// 生成当前帧的 itr 体积列表（严格对齐 FLF mechanics.js: mech.volume + character.js pre/post_interaction 用法）。
        /// - itrs 缺失时返回空列表（与 FLF 一致：没有 itr 就不会产生交互）
        /// - 朝左时按 sp.w 镜像 x：vx = sp.w - O.x - O.w
        /// - zwidth：FLF 在 character.js pre/post_interaction 中会把 vol.zwidth 设为 0（表示只看目标 body 的 zwidth 容差）
        /// </summary>
        public System.Collections.Generic.List<FlfVolume> GetItrVolumes(
            System.Collections.Generic.List<InteractionArea> itrs,
            int centerx,
            int centery,
            float spriteWidthPx,
            float itrZWidthPx = 0f,
            float offsetX = 0f,
            float offsetY = 0f,
            float offsetZ = 0f)
        {
            UpdateSpriteOrigin(centerx, centery, spriteWidthPx);
            float originX = sx + offsetX;
            float originY = sy + offsetY;
            float originZ = sz + offsetZ;

            var result = new System.Collections.Generic.List<FlfVolume>();
            if (itrs == null || itrs.Count == 0)
                return result;

            bool facingLeft = dir == "left";
            foreach (var itr in itrs)
            {
                float localX = itr.x;
                if (facingLeft)
                {
                    localX = spriteWidthPx - itr.x - itr.w;
                }

                result.Add(new FlfVolume(
                    originX, originY, originZ,
                    localX, itr.y,
                    itr.w, itr.h,
                    itrZWidthPx
                ));
            }

            return result;
        }

        /// <summary>
        /// 重置摩擦力（每个 TU 开始时调用）
        /// 对应 FLF livingobject.js:114
        /// </summary>
        public void ResetFriction()
        {
            fric = 1f;
        }

        /// <summary>
        /// 将 FLF 速度（像素/帧）转换为 Unity 速度（单位/秒）
        ///
        /// Step D8: 使用 SimulationConstants 统一常量
        /// - 速度值 vx/vz 直接来自 FLF 数据（30fps 语义）
        /// - 不再需要 FRAMERATE_SCALE（30Hz SimTick = FLF 帧率）
        ///
        /// 转换公式：
        /// - ps.vx = 4 像素/TU（30fps）
        /// - Unity: vx * (30Hz / 100 PPU) = 4 * 0.3 = 1.2 单位/秒
        /// - 转换系数 = SIM_TICK_RATE / PIXELS_PER_UNIT = 30 / 100 = 0.3
        /// </summary>
        public Vector2 ToUnityVelocity()
        {
            float conversion = SimulationConstants.SIM_TICK_RATE / SimulationConstants.PIXELS_PER_UNIT;  // = 0.3
            return new Vector2(vx * conversion, vz * conversion);
        }

        /// <summary>
        /// 从 Unity 速度（单位/秒）设置 FLF 速度（像素/帧）
        ///
        /// Step D8: 使用 SimulationConstants 统一常量
        ///
        /// 转换公式：
        /// - Unity: velocity = 1.2 单位/秒
        /// - FLF: vx = 1.2 × (100 PPU / 30Hz) = 4 像素/TU
        /// - 转换系数 = PIXELS_PER_UNIT / SIM_TICK_RATE = 100 / 30 ≈ 3.333
        /// </summary>
        public void FromUnityVelocity(Vector2 unityVel)
        {
            float conversion = SimulationConstants.PIXELS_PER_UNIT / SimulationConstants.SIM_TICK_RATE;  // ≈ 3.333
            vx = unityVel.x * conversion;
            vz = unityVel.y * conversion;
        }

        /// <summary>
        /// 将 FLF 位置（像素）转换为 Unity 位置（单位）
        /// Step D8: 使用 SimulationConstants.PIXELS_PER_UNIT
        /// P2: 新坐标映射 - 从 2D 改为 3D（X/Z 地面平面，Y 跳跃高度）
        /// </summary>
        public Vector3 ToUnityPosition()
        {
            // P2: 新坐标映射（地面平面 = X/Z，跳跃高度 = Y）
            return new Vector3(
                x / SimulationConstants.PIXELS_PER_UNIT,
                z / SimulationConstants.PIXELS_PER_UNIT,
                0f
            );
        }

        /// <summary>
        /// 从 Unity 位置（单位）设置 FLF 位置（像素）
        /// Step D8: 使用 SimulationConstants.PIXELS_PER_UNIT
        /// P2: 新坐标映射 - Unity Z 对应 FLF z，Unity Y 用于跳跃高度
        /// </summary>
        public void FromUnityPosition(Vector3 unityPos)
        {
            x = unityPos.x * SimulationConstants.PIXELS_PER_UNIT;          // Unity X → FLF x
            z = unityPos.y * SimulationConstants.PIXELS_PER_UNIT;
            groundY = 0f;
            y = 0;                                                          // P2: 初始化为地面（相对位移为 0）
            // 注意：跳跃位移 y 由物理系统管理，这里只记录 groundY 参考
        }

        // ==================== P3: 地面 Footprint 支持 ====================

        /// <summary>
        /// 地面平面坐标（Unity X/Y）：用于边界与排序。
        /// </summary>
        public Vector2 GetGroundPoint2D()
        {
            return new Vector2(
                x / SimulationConstants.PIXELS_PER_UNIT,
                z / SimulationConstants.PIXELS_PER_UNIT
            );
        }

        public int GetDir() 
        {
            return dir == "left" ? -1 : 1;
        }

        /// <summary>
        /// 获取角色在地面平面的 footprint Rect（用于边界检测）
        /// P3: 从 BodyBox 计算 Unity world space 的 X/Y 平面矩形
        ///
        /// 参数说明：
        /// - bodies: 当前帧的 BodyBox 列表（来自 LF2FrameData.bodies）
        /// - centerx/centery: 当前帧的中心点偏移（来自 LF2FrameData）
        ///
        /// 返回值：Unity world space Rect (x=世界X, y=世界Y, width, height)
        /// ⚠️ 项目规范：Rect.x = worldX, Rect.y = worldY（地面 Y 坐标，不是跳跃高度）
        /// ⚠️ ps.z 映射到 world Y（地面平面），ps.y 用于跳跃高度
        /// </summary>
        public Rect GetFootprintRect(System.Collections.Generic.List<BodyBox> bodies, int centerx, int centery)
        {
            if (bodies == null || bodies.Count == 0)
            {
                // FLF: no bdy -> w/h=0 点体积（不要自造默认矩形）
                float worldX = x / SimulationConstants.PIXELS_PER_UNIT;
                float worldY = z / SimulationConstants.PIXELS_PER_UNIT; // ⚠️ ps.z → world Y
                return new Rect(worldX, worldY, 0f, 0f);
            }

            // P3 Phase 1: 使用第一个 BodyBox 作为 footprint（简化实现）
            // 未来可以扩展：kind=0 表示地面碰撞盒，kind=1/2 表示攻击判定
            var body = bodies[0];

            // BodyBox 坐标系：相对于角色 centerx/centery 的偏移（像素）
            // 需要转换为 Unity world space (X/Y 平面)
            // ⚠️ 项目规范：ps.x → world X, ps.z → world Y
            // FLF 镜像规则：朝左时 body.x 需要按帧宽度镜像；在 world left 公式里 sp.w 会抵消，等价写法：
            // right: left = ps.x - centerx + body.x
            // left : left = ps.x + centerx - body.x - body.w
            float bodyLeftPx = dir == "left"
                ? (x + centerx - body.x - body.w)
                : (x - centerx + body.x);
            float bodyWorldX = bodyLeftPx / SimulationConstants.PIXELS_PER_UNIT;
            float bodyWorldY = (z + body.y - centery) / SimulationConstants.PIXELS_PER_UNIT; // ⚠️ ps.z → world Y
            float bodyWidth = body.w / SimulationConstants.PIXELS_PER_UNIT;
            float bodyHeight = body.h / SimulationConstants.PIXELS_PER_UNIT;

            // Unity Rect: (x, y) 是左下角坐标
            // ⚠️ Rect.x = worldX, Rect.y = worldY（地面 Y 坐标，不是跳跃高度）
            return new Rect(bodyWorldX, bodyWorldY, bodyWidth, bodyHeight);
        }
    }
}
