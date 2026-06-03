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
        public readonly float mass;
        public readonly float minSpeed;
        public readonly float gravity;
        public readonly float blockedMoveScale;
        public readonly Func<Vector2, bool> isPointWalkable;
        public readonly Func<Vector2, float, bool> isNearConcaveVertex;
        public readonly Action<string> logWarning;

        public CharacterMechanicsContext(
            PhysicsState ps,
            LF2FrameData frameData,
            float spriteWidthPx,
            float mass,
            float minSpeed,
            float gravity,
            float blockedMoveScale,
            Func<Vector2, bool> isPointWalkable,
            Func<Vector2, float, bool> isNearConcaveVertex = null,
            Action<string> logWarning = null)
        {
            this.ps = ps;
            this.frameData = frameData;
            this.spriteWidthPx = spriteWidthPx;
            this.mass = mass;
            this.minSpeed = minSpeed;
            this.gravity = gravity;
            this.blockedMoveScale = blockedMoveScale;
            this.isPointWalkable = isPointWalkable;
            this.isNearConcaveVertex = isNearConcaveVertex;
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
        /// <summary>
        /// 使用脚底中心点做可走性判断。
        /// 边缘容差由 BoundaryWall 统一处理，避免顶点处前探点落到边界外导致卡住。
        /// </summary>
        private static bool IsMovementWalkable(
            Func<Vector2, bool> isPointWalkable,
            Vector2 center)
        {
            if (isPointWalkable == null) return true;
            return isPointWalkable(center);
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
                if (!IsMovementWalkable(isPointWalkable, footPoint))
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
            // ==================== 1. 水平位移 + 边界回退（FLF Line 326-327）====================
            float oldX = ps.x;
            float oldZ = ps.z;

            // 尝试 full 移动
            float moveScale = ctx.blockedMoveScale;
            ps.x += ps.vx * moveScale;
            ps.z += ps.vz * moveScale;

            // P3: 检测是否越出边界
            if (ctx.isPointWalkable != null)
            {
                Vector2 footPoint = ps.GetGroundPoint2D();

                if (!IsMovementWalkable(ctx.isPointWalkable, footPoint))
                {
                    // Full 移动被拦截：尝试 wall sliding（分轴移动）
                    bool slid = false;

                    // 尝试只移动 X
                    if (!slid && ps.vx != 0)
                    {
                        ps.x = oldX + ps.vx * moveScale;
                        ps.z = oldZ;
                        footPoint = ps.GetGroundPoint2D();
                        if (IsMovementWalkable(ctx.isPointWalkable, footPoint))
                        {
                            boundaryMode = BoundaryResolveMode.XOnly;
                            slid = true;
                        }
                    }

                    // 尝试只移动 Z
                    if (!slid && ps.vz != 0)
                    {
                        ps.x = oldX;
                        ps.z = oldZ + ps.vz * moveScale;
                        footPoint = ps.GetGroundPoint2D();
                        if (IsMovementWalkable(ctx.isPointWalkable, footPoint))
                        {
                            boundaryMode = BoundaryResolveMode.XOnly;
                            slid = true;
                        }
                    }

                    // 凹角绕行：只在凹角顶点附近才做 nudge，直边/垂直边直接停住
                    // 用当前位置检测（角色靠近凹角时触发），半径 1.0 世界单位
                    bool nearConcaveVertex = false;
                    if (ctx.isNearConcaveVertex != null)
                    {
                        Vector2 currentFoot = new Vector2(
                            oldX / SimulationConstants.PIXELS_PER_UNIT,
                            oldZ / SimulationConstants.PIXELS_PER_UNIT);
                        nearConcaveVertex = ctx.isNearConcaveVertex(currentFoot, 1.0f);
                    }

                    if (!slid && ps.vx != 0 && nearConcaveVertex)
                    {
                        // 先尝试 X + Z 偏移
                        float[] nudges = { 10f, -10f, 20f, -20f, 30f, -30f };
                        foreach (float nz in nudges)
                        {
                            ps.x = oldX + ps.vx * moveScale;
                            ps.z = oldZ + nz;
                            footPoint = ps.GetGroundPoint2D();
                            if (IsMovementWalkable(ctx.isPointWalkable, footPoint))
                            {
                                boundaryMode = BoundaryResolveMode.XOnly;
                                slid = true;
                                break;
                            }
                        }

                        // 若 X+nudge 全部失败，尝试纯 Z 移动让角色先绕开凹角
                        if (!slid)
                        {
                            float[] zOnly = { 10f, -10f, 20f, -20f };
                            foreach (float nz in zOnly)
                            {
                                ps.x = oldX;
                                ps.z = oldZ + nz;
                                footPoint = ps.GetGroundPoint2D();
                                if (IsMovementWalkable(ctx.isPointWalkable, footPoint))
                                {
                                    boundaryMode = BoundaryResolveMode.XOnly;
                                    slid = true;
                                    break;
                                }
                            }
                        }
                    }

                    // Z 方向有速度但被拦截时，尝试加小幅 X 偏移绕过凹角
                    if (!slid && ps.vz != 0 && nearConcaveVertex)
                    {
                        float[] nudges = { 10f, -10f, 20f, -20f, 30f, -30f };
                        foreach (float nx in nudges)
                        {
                            ps.x = oldX + nx;
                            ps.z = oldZ + ps.vz * moveScale;
                            footPoint = ps.GetGroundPoint2D();
                            if (IsMovementWalkable(ctx.isPointWalkable, footPoint))
                            {
                                boundaryMode = BoundaryResolveMode.XOnly;
                                slid = true;
                                break;
                            }
                        }
                    }

                    if (!slid)
                    {
                        // 所有方向都被拦截：保持原位
                        ps.x = oldX;
                        ps.z = oldZ;
                        boundaryMode = BoundaryResolveMode.Stop;
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

        /// <summary>
        /// 武器专用物理，严格对齐反汇编 Entity_FrameAdvance 0x4162EB-0x4164BD 执行顺序：
        ///   1. x += vx（边界锁定）
        ///   2. z += vz（边界锁定）
        ///   3. 边界标志清零
        ///   4. 地面摩擦（y+=vy 之前，用旧 y 判断，反汇编 0x4163A1: cmp [esi+14h],0; jl skip）
        ///   5. y += vy；钳制 y <= 0
        ///   6. 若新 y < -0.0001（仍在空中）：vy += gravityToAdd（反汇编 0x4164BD: test ah,1; jz landed）
        /// gravityToAdd 由调用方按 type/type_sub 计算并传入（type=3 传 0）。
        /// </summary>
        public static void WeaponDynamics(PhysicsState ps, float gravityToAdd)
        {
            if (ps == null) return;

            // x 位移（[+3F0h]/[+3F4h] 边界锁定）
            if (ps.vx > 0 && !ps.xBoundPositive)
                ps.x += ps.vx;
            else if (ps.vx < 0 && !ps.xBoundNegative)
                ps.x += ps.vx;

            // z 位移（[+3E8h]/[+3ECh] 边界锁定）
            if (ps.vz > 0 && !ps.zBoundPositive)
                ps.z += ps.vz;
            else if (ps.vz < 0 && !ps.zBoundNegative)
                ps.z += ps.vz;

            // 清零边界锁定标志（反汇编 0x416365-0x416377）
            ps.zBoundPositive = false;
            ps.zBoundNegative = false;
            ps.xBoundPositive = false;
            ps.xBoundNegative = false;

            // 地面摩擦：用旧 y（y+=vy 之前）判断（反汇编 0x4163A1: cmp [esi+14h],ebp; jl skip）
            // 反汇编 0x4163BD: fsub dbl_4432B0=1.0，固定减 1.0，不用 ps.fric
            if (ps.y >= 0)
            {
                if (ps.vx > 0.0001f)
                {
                    ps.vx -= 1.0f;
                    if (ps.vx < 0.0001f) ps.vx = 0f;
                }
                else if (ps.vx < -0.0001f)
                {
                    ps.vx += 1.0f;
                    if (ps.vx > -0.0001f) ps.vx = 0f;
                }
                if (ps.vz > 0.0001f)
                {
                    ps.vz -= 1.0f;
                    if (ps.vz < 0.0001f) ps.vz = 0f;
                }
                else if (ps.vz < -0.0001f)
                {
                    ps.vz += 1.0f;
                    if (ps.vz > -0.0001f) ps.vz = 0f;
                }
            }

            // y 位移（反汇编 0x4164AC-0x4164B5）
            ps.y += ps.vy;
            if (ps.y > 0) ps.y = 0;

            // 重力：仅在新 y < -0.0001（仍在空中）时累加（反汇编 0x4164BD: test ah,1; jz loc_4166CE）
            if (ps.y < -0.0001f)
                ps.vy += gravityToAdd;
        }
    }
}
