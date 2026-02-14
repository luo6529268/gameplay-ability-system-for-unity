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
        public readonly LF2FrameData frameData;
        public readonly float spriteWidthPx;
        public readonly bool hasStageBounds;
        public readonly LF2StageBoundsPx stageBoundsPx;
        public readonly float mass;
        public readonly float minSpeed;
        public readonly float gravity;
        public readonly float blockedMoveScale;
        public readonly Func<Vector2, bool> isPointWalkable;
        public readonly Action<string> logWarning;

        public CharacterMechanicsContext(
            PhysicsState ps,
            LF2FrameData frameData,
            float spriteWidthPx,
            bool hasStageBounds,
            LF2StageBoundsPx stageBoundsPx,
            float mass,
            float minSpeed,
            float gravity,
            float blockedMoveScale,
            Func<Vector2, bool> isPointWalkable,
            Action<string> logWarning = null)
        {
            this.ps = ps;
            this.frameData = frameData;
            this.spriteWidthPx = spriteWidthPx;
            this.hasStageBounds = hasStageBounds;
            this.stageBoundsPx = stageBoundsPx;
            this.mass = mass;
            this.minSpeed = minSpeed;
            this.gravity = gravity;
            this.blockedMoveScale = blockedMoveScale;
            this.isPointWalkable = isPointWalkable;
            this.logWarning = logWarning;
        }
    }

    /// <summary>
    /// 物理计算输出结果（readonly struct 减少 GC）
    /// </summary>
    public readonly struct MechanicsStepResult
    {
        public readonly bool grounded;
        public readonly Vector2 groundPlanePos;
        public readonly float visualYOffset;
        public readonly BoundaryResolveMode boundaryMode;

        public MechanicsStepResult(
            bool grounded,
            Vector2 groundPlanePos,
            float visualYOffset,
            BoundaryResolveMode boundaryMode)
        {
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
    /// 2. 边界/阻挡处理（对齐 FLF 的“blocking_xz -> *0.1 位移”风格）
    /// 3. 垂直位移 + 地面修正 + 落地判定
    /// 4. 应用摩擦力（只在地面）
    /// 5. 应用重力（只在空中）
    /// </summary>
    public sealed class CharacterMechanics
    {
        private static float GetDefaultFootRadiusWorld()
        {
            // FLF 默认 itr.zwidth = 12 像素（global.js: GC.default.itr.zwidth）。
            // 这里取半宽作为“近似体积”采样半径，并转换到 Unity ground plane 的 world 单位。
            // FLF 默认 itr.zwidth = 12 像素（global.js: GC.default.itr.zwidth）
            // 这里取半宽作为“近似体积”采样半径，并转换到 Unity ground plane 的 world 单位。
            return (NTSDGlobal.Default.Itr.ZWidth / SimulationConstants.PIXELS_PER_UNIT) * 0.5f;
        }

        /// <summary>
        /// 使用“多点采样”近似 FLF blocking_xz() 的体积阻挡判定。
        /// 数据来源仍然是项目的可视化可走区（isPointWalkable），但阻挡判定方式更接近 FLF 的“体积查询”。
        /// </summary>
        private static bool IsFootprintWalkable(
            Func<Vector2, bool> isPointWalkable,
            Vector2 center,
            float radiusWorld,
            Vector2 moveDirWorld)
        {
            if (isPointWalkable == null) return true;
            if (radiusWorld <= 0f) return isPointWalkable(center);

            // 基础 5 点采样：中心 + 十字
            if (!isPointWalkable(center)) return false;
            if (!isPointWalkable(new Vector2(center.x + radiusWorld, center.y))) return false;
            if (!isPointWalkable(new Vector2(center.x - radiusWorld, center.y))) return false;
            if (!isPointWalkable(new Vector2(center.x, center.y + radiusWorld))) return false;
            if (!isPointWalkable(new Vector2(center.x, center.y - radiusWorld))) return false;

            // 按移动方向前探一点，减少边界处“单点通过、体积穿出”的漏判
            if (moveDirWorld.sqrMagnitude > 1e-6f)
            {
                moveDirWorld.Normalize();
                if (!isPointWalkable(center + moveDirWorld * radiusWorld)) return false;
            }

            return true;
        }
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
                float footprintRadiusWorld = GetDefaultFootRadiusWorld();
                if (!IsFootprintWalkable(isPointWalkable, footPoint, footprintRadiusWorld, Vector2.zero))
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
                return new MechanicsStepResult(true, Vector2.zero, 0f, BoundaryResolveMode.None);
            }

            BoundaryResolveMode boundaryMode = BoundaryResolveMode.None;
            float footprintRadiusWorld = GetDefaultFootRadiusWorld();
            Vector2 moveDirWorld = new Vector2(ps.vx, ps.vz) / SimulationConstants.PIXELS_PER_UNIT;

            // ==================== 1. 水平位移 + 边界回退（FLF Line 326-327）====================
            float oldX = ps.x;
            float oldZ = ps.z;

            // 尝试 full 移动
            ps.x += ps.vx * ctx.blockedMoveScale;
            ps.z += ps.vz * ctx.blockedMoveScale;

            // 对齐 FLF mechanics.js dynamics(): x/z clamp（非回滚）
            if (ctx.hasStageBounds)
            {
                var b = ctx.stageBoundsPx;
                bool clampedX = false;
                bool clampedZ = false;

                if (b.floorXBound)
                {
                    if (ps.x < b.xMinPx) { ps.x = b.xMinPx; clampedX = true; }
                    else if (ps.x > b.xMaxPx) { ps.x = b.xMaxPx; clampedX = true; }
                }

                if (ps.z < b.zMinPx) { ps.z = b.zMinPx; clampedZ = true; }
                else if (ps.z > b.zMaxPx) { ps.z = b.zMaxPx; clampedZ = true; }

                if (clampedX && clampedZ) boundaryMode = BoundaryResolveMode.Stop;
                else if (clampedX) boundaryMode = BoundaryResolveMode.XOnly;
                else if (clampedZ) boundaryMode = BoundaryResolveMode.ZOnly;
            }

            // P3: 检测是否越出边界
            else if (ctx.isPointWalkable != null)
            {
                Vector2 footPoint = ps.GetGroundPoint2D();

                if (!IsFootprintWalkable(ctx.isPointWalkable, footPoint, footprintRadiusWorld, moveDirWorld))
                {
                    // Full 移动不可走：对齐 FLF 的阻挡处理，改为缩小位移（*0.1）
                    ctx.logWarning?.Invoke("[Boundary] Out of walkable area, keep position");
                    ps.x = oldX;
                    ps.z = oldZ;
                    boundaryMode = BoundaryResolveMode.Stop;

                    footPoint = ps.GetGroundPoint2D();
                    if (!IsFootprintWalkable(ctx.isPointWalkable, footPoint, footprintRadiusWorld, moveDirWorld))
                    {
                        // 缩小位移后仍不可走：本帧保持原位（不清 vx/vz）
                        ctx.logWarning?.Invoke("[Boundary] Out of walkable area, keep position");
                        ps.x = oldX;
                        ps.z = oldZ;
                        boundaryMode = BoundaryResolveMode.Stop;

                        footPoint = ps.GetGroundPoint2D();
                        if (!IsFootprintWalkable(ctx.isPointWalkable, footPoint, footprintRadiusWorld, moveDirWorld))
                        {
                            // 保留旧的“二次确认”分支：保持原位
                            ctx.logWarning?.Invoke("[Boundary] Blocked: keep position");
                            ps.x = oldX;
                            ps.z = oldZ;
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

            // ps.sx/ps.sy/ps.sz 由 UpdateSpriteOrigin() 统一计算（对齐 FLF mechanics.js:349-351）

            if (ps.y > 0)  // 不允许低于地面
            {
                ps.y = 0;
                // ps.sy 将在 UpdateSpriteOrigin() 中统一计算
            }

            // ==================== 3. 地面摩擦（FLF Line 368-375）====================
            if (ctx.frameData != null && ctx.spriteWidthPx > 0f)
            {
                ps.UpdateSpriteOrigin(ctx.frameData.centerx, ctx.frameData.centery, ctx.spriteWidthPx);
            }

            if (ps.y == 0 && ctx.mass > 0f)
            {
                // X轴摩擦
                if (ps.vx != 0)
                    ps.vx += (ps.vx > 0 ? -1 : 1) * ps.fric;
                
                // Z轴摩擦
                if (ps.vz != 0)
                    ps.vz += (ps.vz > 0 ? -1 : 1) * ps.fric;
                
                if(ps.vx != 0 && ps.vx > -NTSDGlobal.Gameplay.MinSpeed && ps.vx < NTSDGlobal.Gameplay.MinSpeed)
                    ps.vx = 0;
                if (ps.vz != 0 && ps.vz > -NTSDGlobal.Gameplay.MinSpeed && ps.vz < NTSDGlobal.Gameplay.MinSpeed)
                    ps.vz = 0;
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
                grounded,
                groundPlanePos,
                visualYOffset,
                boundaryMode
            );
        }

        /// <summary>
        /// 简化版物理动力学（用于武器/特效等非角色对象）
        /// 对齐 FLF mechanics.js dynamics() 的核心逻辑
        /// </summary>
        public static void Dynamics(PhysicsState ps, float mass = 1f)
        {
            if (ps == null) return;

            // 1. 水平位移
            ps.x += ps.vx;
            ps.z += ps.vz;

            // 2. 垂直位移
            ps.y += ps.vy;

            // 3. 地面修正
            if (ps.y > 0)
            {
                ps.y = 0;
            }

            // 4. 地面摩擦
            if (ps.y == 0 && mass > 0f)
            {
                if (ps.vx != 0)
                    ps.vx += (ps.vx > 0 ? -1 : 1) * ps.fric;
                if (ps.vz != 0)
                    ps.vz += (ps.vz > 0 ? -1 : 1) * ps.fric;

                if (ps.vx != 0 && ps.vx > -NTSDGlobal.Gameplay.MinSpeed && ps.vx < NTSDGlobal.Gameplay.MinSpeed)
                    ps.vx = 0;
                if (ps.vz != 0 && ps.vz > -NTSDGlobal.Gameplay.MinSpeed && ps.vz < NTSDGlobal.Gameplay.MinSpeed)
                    ps.vz = 0;
            }

            // 5. 空中重力
            if (ps.y < 0)
            {
                ps.vy += mass * NTSDGlobal.Gameplay.Gravity;
            }
        }
    }
}
