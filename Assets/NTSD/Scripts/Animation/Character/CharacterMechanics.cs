using System;
using UnityEngine;
using NTSD.Simulation;
using NTSD.Tools;

namespace NTSD.Animation
{
    /// <summary>
    /// 边界解算模式枚举
    /// 用于调试时输出更结构化的日志
    /// </summary>
    public enum BoundaryResolveMode
    {
        None,    // 无越界，Full 移动成功
        XOnly,   // X-only 回退成功
        ZOnly,   // Z-only 回退成功
        Stop     // 完全阻挡，速度归零
    }

    /// <summary>
    /// 物理计算输入上下文（readonly struct 减少 GC）
    /// 避免 CharacterMechanics 直接依赖 Animator/Mono
    /// </summary>
    public readonly struct CharacterMechanicsContext
    {
        public readonly PhysicsState ps;
        public readonly float mass;
        public readonly float minSpeed;
        public readonly float gravity;
        public readonly Func<Vector2, bool> isPointWalkable;
        public readonly Action<string> logWarning;

        public CharacterMechanicsContext(
            PhysicsState ps,
            float mass,
            float minSpeed,
            float gravity,
            Func<Vector2, bool> isPointWalkable,
            Action<string> logWarning = null)
        {
            this.ps = ps;
            this.mass = mass;
            this.minSpeed = minSpeed;
            this.gravity = gravity;
            this.isPointWalkable = isPointWalkable;
            this.logWarning = logWarning;
        }
    }

    /// <summary>
    /// 物理计算输出结果（readonly struct 减少 GC）
    /// </summary>
    public readonly struct MechanicsStepResult
    {
        public readonly bool landedThisTick;
        public readonly bool grounded;
        public readonly Vector2 groundPlanePos;
        public readonly float visualYOffset;
        public readonly BoundaryResolveMode boundaryMode;

        public MechanicsStepResult(
            bool landedThisTick,
            bool grounded,
            Vector2 groundPlanePos,
            float visualYOffset,
            BoundaryResolveMode boundaryMode)
        {
            this.landedThisTick = landedThisTick;
            this.grounded = grounded;
            this.groundPlanePos = groundPlanePos;
            this.visualYOffset = visualYOffset;
            this.boundaryMode = boundaryMode;
        }
    }

    /// <summary>
    /// 角色物理计算层 - 对应 FLF 的 mech 体系
    /// 
    /// 不继承 MonoBehaviour，不挂组件，只处理数据/运算。
    /// 对应 FLF mechanics.js 的 dynamics() 逻辑。
    /// 
    /// 职责（严格按 FLF 顺序）：
    /// 1. 应用速度到位置：ps.x += ps.vx, ps.z += ps.vz
    /// 2. 边界检测与修正（P3 full → X-only → Z-only → stop）
    /// 3. 垂直位移 + 地面修正 + 落地判定
    /// 4. 应用摩擦力（只在地面）
    /// 5. 应用重力（只在空中）
    /// </summary>
    public sealed class CharacterMechanics
    {
        // ==================== 可复用 Helper（对齐 FLF mechanics.js）====================

        /// <summary>
        /// 单位摩擦 - 对齐 FLF mechanics.js unit_friction()
        /// 每 tick 对 vx/vz 各减 1（带符号），低于 minSpeed 时归零
        /// </summary>
        public static void UnitFriction(PhysicsState ps)
        {
            if (ps == null) return;

            if (ps.vx != 0)
                ps.vx += (ps.vx > 0 ? -1f : 1f);

            if (ps.vz != 0)
                ps.vz += (ps.vz > 0 ? -1f : 1f);
        }

        /// <summary>
        /// 线性摩擦 - 对齐 FLF mechanics.js linear_friction(x, z)
        /// 对 vx/vz 各减去指定量（带符号），低于 minSpeed 时归零
        /// </summary>
        public static void LinearFriction(PhysicsState ps, float fricX, float fricZ)
        {
            if (ps == null) return;

            if (ps.vx != 0)
                ps.vx += (ps.vx > 0 ? -fricX : fricX);

            if (ps.vz != 0)
                ps.vz += (ps.vz > 0 ? -fricZ : fricZ);
        }

        /// <summary>
        /// 速度标量 - 对齐 FLF mechanics.js speed()
        /// 注意：FLF 默认只算 vx/vy（不含 vz），这里保持一致
        /// </summary>
        public static float SpeedXY(PhysicsState ps)
        {
            if (ps == null) return 0f;
            return Mathf.Sqrt(ps.vx * ps.vx + ps.vy * ps.vy);
        }

        /// <summary>
        /// 设置脚底点位置 + 边界约束 - 对齐 FLF mechanics.js set_pos(x, y, z)
        /// </summary>
        public static void SetPos(PhysicsState ps, float x, float y, float z, Func<Vector2, bool> isPointWalkable)
        {
            if (ps == null) return;

            // 保存旧值用于回滚
            float oldX = ps.x;
            float oldY = ps.y;
            float oldZ = ps.z;

            ps.x = x;
            ps.y = y;
            ps.z = z;

            // 边界约束：越界则回滚
            if (isPointWalkable != null)
            {
                Vector2 footPoint = ps.GetGroundPoint2D();
                if (!isPointWalkable(footPoint))
                {
                    ps.x = oldX;
                    ps.y = oldY;
                    ps.z = oldZ;
                }
            }
        }

        // ==================== 主入口 ====================
        /// <summary>
        /// 每 tick 执行一次的物理计算入口
        /// 等价于 FLF mech.dynamics()
        /// </summary>
        public MechanicsStepResult Step(in CharacterMechanicsContext ctx)
        {
            var ps = ctx.ps;
            if (ps == null)
            {
                return new MechanicsStepResult(false, true, Vector2.zero, 0f, BoundaryResolveMode.None);
            }

            BoundaryResolveMode boundaryMode = BoundaryResolveMode.None;
            bool landedThisTick = false;

            // ==================== 1. 水平位移 + 边界回退（FLF Line 326-327）====================
            float oldX = ps.x;
            float oldZ = ps.z;

            // 尝试 full 移动
            ps.x += ps.vx;
            ps.z += ps.vz;

            // P3: 检测是否越出边界
            if (ctx.isPointWalkable != null)
            {
                Vector2 footPoint = ps.GetGroundPoint2D();

                if (!ctx.isPointWalkable(footPoint))
                {
                    // Full 移动越界，尝试 X-only
                    ctx.logWarning?.Invoke("[Boundary] Full 越界，尝试 X-only");
                    ps.x = oldX + ps.vx;
                    ps.z = oldZ;

                    footPoint = ps.GetGroundPoint2D();
                    if (!ctx.isPointWalkable(footPoint))
                    {
                        // X-only 也越界，尝试 Z-only
                        ctx.logWarning?.Invoke("[Boundary] X-only 越界，尝试 Z-only");
                        ps.x = oldX;
                        ps.z = oldZ + ps.vz;
                        boundaryMode = BoundaryResolveMode.ZOnly;

                        footPoint = ps.GetGroundPoint2D();
                        if (!ctx.isPointWalkable(footPoint))
                        {
                            // Z-only 也越界，stop
                            ctx.logWarning?.Invoke("[Boundary] Z-only 越界，Stop");
                            ps.x = oldX;
                            ps.z = oldZ;
                            ps.vx = 0;
                            ps.vz = 0;
                            boundaryMode = BoundaryResolveMode.Stop;
                        }
                    }
                    else
                    {
                        boundaryMode = BoundaryResolveMode.XOnly;
                    }
                }
            }

            // ==================== 2. 垂直位移 + 地面修正（FLF Line 347-354）====================
            ps.y += ps.vy;

            if (ps.y > 0)  // 不允许低于地面
            {
                ps.y = 0;
                if (ps.vy > 0)  // 只在向下运动时触发落地
                {
                    ps.vy = 0;
                    landedThisTick = true;
                }
            }

            // ==================== 3. 地面摩擦（FLF Line 368-375）====================
            if (ps.y == 0 && ctx.mass > 0f)
            {
                // X轴摩擦
                if (ps.vx != 0)
                {
                    ps.vx += (ps.vx > 0 ? -1 : 1) * ps.fric;
                    if (Mathf.Abs(ps.vx) < ctx.minSpeed) ps.vx = 0;
                }

                // Z轴摩擦
                if (ps.vz != 0)
                {
                    ps.vz += (ps.vz > 0 ? -1 : 1) * ps.fric;
                    if (Mathf.Abs(ps.vz) < ctx.minSpeed) ps.vz = 0;
                }
            }

            // ==================== 4. 空中重力（FLF Line 377）====================
            if (ps.y < 0)
            {
                ps.vy += ctx.mass * ctx.gravity;
            }

            // ==================== 5. 计算输出结果 ====================
            Vector2 groundPlanePos = ps.GetGroundPoint2D();
            float visualYOffset = (-ps.y) / SimulationConstants.PIXELS_PER_UNIT;
            bool grounded = (ps.y == 0);

            return new MechanicsStepResult(
                landedThisTick,
                grounded,
                groundPlanePos,
                visualYOffset,
                boundaryMode
            );
        }
    }
}
