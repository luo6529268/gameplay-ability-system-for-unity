using System;
using UnityEngine;
using NTSD.Simulation;
using NTSD.Tools;

namespace NTSD.Animation
{
    /// <summary>本 tick 的移动是如何被边界锁或 Unity 边界约束处理的。</summary>
    public enum BoundaryResolveMode
    {
        None,
        XOnly,
        ZOnly,
        Stop
    }

    /// <summary>
    /// 正式版物理步骤的输入上下文。
    /// 用结构体传入，避免 CharacterMechanics 直接依赖 MonoBehaviour 或 Animator 状态。
    /// </summary>
    public readonly struct CharacterMechanicsContext
    {
        public readonly PhysicsState ps;
        public readonly LF2FrameData frameData;
        public readonly float spriteWidthPx;
        public readonly float mass;
        public readonly float minSpeed;
        public readonly float gravity;
        public readonly Func<Vector2, bool> isPointWalkable;
        public readonly Action<string> logWarning;

        public CharacterMechanicsContext(
            PhysicsState ps,
            LF2FrameData frameData,
            float spriteWidthPx,
            float mass,
            float minSpeed,
            float gravity,
            Func<Vector2, bool> isPointWalkable,
            Action<string> logWarning = null)
        {
            this.ps = ps;
            this.frameData = frameData;
            this.spriteWidthPx = spriteWidthPx;
            this.mass = mass;
            this.minSpeed = minSpeed;
            this.gravity = gravity;
            this.isPointWalkable = isPointWalkable;
            this.logWarning = logWarning;
        }
    }

    /// <summary>物理步骤输出，用于调用方更新 Unity Transform 和调试状态。</summary>
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
    /// 对齐正式版逻辑的角色和简单 LF2 对象物理计算层。
    /// 角色移动会消费碰撞/边界逻辑设置的分轴边界标志，然后处理垂直位移，
    /// 最后根据地面或空中状态应用摩擦或重力。
    /// </summary>
    public sealed class CharacterMechanics
    {
        /// <summary>检查对象地面平面点在 Unity 边界中是否可走。</summary>
        private static bool IsMovementWalkable(
            Func<Vector2, bool> isPointWalkable,
            Vector2 center)
        {
            if (isPointWalkable == null) return true;
            return isPointWalkable(center);
        }

        /// <summary>对 x/z 速度应用 1 单位带符号摩擦。</summary>
        public static void UnitFriction(PhysicsState ps)
        {
            if (ps == null) return;

            if (ps.vx != 0)
                ps.vx += (ps.vx > 0 ? -1f : 1f);

            if (ps.vz != 0)
                ps.vz += (ps.vz > 0 ? -1f : 1f);
        }

        /// <summary>对 x/z 速度应用调用方指定的带符号摩擦。</summary>
        public static void LinearFriction(PhysicsState ps, float fricX, float fricZ)
        {
            if (ps == null) return;

            if (ps.vx != 0)
                ps.vx += (ps.vx > 0 ? -fricX : fricX);

            if (ps.vz != 0)
                ps.vz += (ps.vz > 0 ? -fricZ : fricZ);
        }

        /// <summary>返回旧动画分支使用的 x/y 速度标量。</summary>
        public static float SpeedXY(PhysicsState ps)
        {
            if (ps == null) return 0f;
            return Mathf.Sqrt(ps.vx * ps.vx + ps.vy * ps.vy);
        }

        /// <summary>设置 NTSD 位置；如果 Unity 可走检测拒绝该点，则回滚到旧位置。</summary>
        public static void SetPos(PhysicsState ps, float x, float y, float z, Func<Vector2, bool> isPointWalkable)
        {
            if (ps == null) return;

            float oldX = ps.x;
            float oldY = ps.y;
            float oldZ = ps.z;

            ps.x = x;
            ps.y = y;
            ps.z = z;

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

        /// <summary>
        /// 执行一次角色物理 tick。
        /// 正式版边界标志只会阻止与速度方向匹配的轴向移动，并在本 tick 后清零。
        /// </summary>
        public MechanicsStepResult Step(in CharacterMechanicsContext ctx)
        {
            var ps = ctx.ps;
            if (ps == null)
            {
                return new MechanicsStepResult(true, Vector2.zero, 0f, BoundaryResolveMode.None);
            }

            BoundaryResolveMode boundaryMode = BoundaryResolveMode.None;
            float oldX = ps.x;
            float oldZ = ps.z;

            // C++ 正式版保留速度，只在本 tick 跳过被阻挡轴的位移。
            bool blockedX = (ps.vx > 0f && ps.xBoundPositive) || (ps.vx < 0f && ps.xBoundNegative);
            bool blockedZ = (ps.vz > 0f && ps.zBoundPositive) || (ps.vz < 0f && ps.zBoundNegative);

            if (!blockedX) ps.x += ps.vx;
            if (!blockedZ) ps.z += ps.vz;

            if (blockedX && blockedZ) boundaryMode = BoundaryResolveMode.Stop;
            else if (blockedX) boundaryMode = BoundaryResolveMode.ZOnly;
            else if (blockedZ) boundaryMode = BoundaryResolveMode.XOnly;

            ps.xBoundPositive = ps.xBoundNegative = false;
            ps.zBoundPositive = ps.zBoundNegative = false;

            // Unity 可走边界适配：正式版的场景边界不属于 Entity 物理本体。
            if (ctx.isPointWalkable != null && !IsMovementWalkable(ctx.isPointWalkable, ps.GetGroundPoint2D()))
            {
                ps.x = oldX;
                ps.z = oldZ;
                boundaryMode = BoundaryResolveMode.Stop;
            }

            // 垂直轴：y == 0 表示地面，y < 0 表示空中。
            ps.y += ps.vy;


            if (ps.y > 0)
            {
                ps.y = 0;
            }

            if (ctx.frameData != null && ctx.spriteWidthPx > 0f)
            {
                ps.UpdateSpriteOrigin(ctx.frameData.centerx, ctx.frameData.centery, ctx.spriteWidthPx);
            }

            // 地面摩擦在位移之后、空中重力之前应用。
            if (ps.y == 0 && ctx.mass > 0f)
            {
                if (ps.vx != 0)
                    ps.vx += (ps.vx > 0 ? -1 : 1) * ps.fric;
                
                if (ps.vz != 0)
                    ps.vz += (ps.vz > 0 ? -1 : 1) * ps.fric;
                
                if(ps.vx != 0 && ps.vx > -NTSDGlobal.Gameplay.MinSpeed && ps.vx < NTSDGlobal.Gameplay.MinSpeed)
                    ps.vx = 0;
                if (ps.vz != 0 && ps.vz > -NTSDGlobal.Gameplay.MinSpeed && ps.vz < NTSDGlobal.Gameplay.MinSpeed)
                    ps.vz = 0;
            }

            if (ps.y < 0)
            {
                ps.vy += ctx.mass * ctx.gravity;
            }



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
        /// 非角色对象共用的简化动力学路径，适用于不需要正式版分轴边界标志的对象。
        /// </summary>
        public static void Dynamics(PhysicsState ps, float mass = 1f)
        {
            if (ps == null) return;

            ps.x += ps.vx;
            ps.z += ps.vz;

            ps.y += ps.vy;

            if (ps.y > 0)
            {
                ps.y = 0;
            }

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

            if (ps.y < 0)
            {
                ps.vy += mass * NTSDGlobal.Gameplay.Gravity;
            }
        }

        /// <summary>
        /// 武器专用的正式版动力学。
        /// 边界标志会限制 x/z 位移并随后清零；地面摩擦在 y += vy 之前应用；
        /// 只有 y 位移后仍处于空中时才追加重力。
        /// </summary>
        public static void WeaponDynamics(PhysicsState ps, float gravityToAdd)
        {
            if (ps == null) return;

            if (ps.vx > 0 && !ps.xBoundPositive)
                ps.x += ps.vx;
            else if (ps.vx < 0 && !ps.xBoundNegative)
                ps.x += ps.vx;

            if (ps.vz > 0 && !ps.zBoundPositive)
                ps.z += ps.vz;
            else if (ps.vz < 0 && !ps.zBoundNegative)
                ps.z += ps.vz;

            ps.zBoundPositive = false;
            ps.zBoundNegative = false;
            ps.xBoundPositive = false;
            ps.xBoundNegative = false;

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

            ps.y += ps.vy;
            if (ps.y > 0) ps.y = 0;

            if (ps.y < -0.0001f)
                ps.vy += gravityToAdd;
        }
    }
}
