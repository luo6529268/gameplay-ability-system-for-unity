using System;
using UnityEngine;
using NTSD.Simulation;
using NTSD.Tools;

namespace NTSD.Animation
{
    /// <summary>本 tick 的移动是如何被边界锁或 Unity 边界约束处理的。</summary>
    /// <summary>
    /// 本 tick 的移动最终被哪一类边界规则拦住。
    /// </summary>
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
    /// <summary>
    /// 一次物理步进所需的输入上下文。
    ///
    /// 这样做的目的，是让物理计算依赖“明确传入的数据”，
    /// 而不是直接耦合到 MonoBehaviour、Animator 或场景对象。
    /// </summary>
    public readonly struct CharacterMechanicsContext
    {
        public readonly NTSDEntityRuntime Runtime;
        public readonly LF2FrameData frameData;
        public readonly float spriteWidthPx;
        public readonly float mass;
        public readonly float minSpeed;
        public readonly double gravity; // P0-f-2a: double sim gravity
        public readonly Func<Vector2, bool> isPointWalkable;
        public readonly Action<string> logWarning;

        public CharacterMechanicsContext(
            NTSDEntityRuntime runtime,
            LF2FrameData frameData,
            float spriteWidthPx,
            float mass,
            float minSpeed,
            double gravity,
            Func<Vector2, bool> isPointWalkable,
            Action<string> logWarning = null)
        {
            Runtime = runtime;
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
    /// <summary>
    /// 一次物理步进后的结果摘要。
    ///
    /// 调用方会拿它去更新表现层、判断是否落地，以及记录边界处理结果。
    /// </summary>
    public readonly struct MechanicsStepResult
    {
        public readonly bool grounded;
        public readonly Vector2 groundPlanePos;
        public readonly float visualYOffset;
        public readonly BoundaryResolveMode boundaryMode;
        public readonly bool landed;
        public readonly float verticalVelocityBeforeLanding;

        public MechanicsStepResult(
            bool grounded,
            Vector2 groundPlanePos,
            float visualYOffset,
            BoundaryResolveMode boundaryMode,
            bool landed = false,
            float verticalVelocityBeforeLanding = 0f)
        {
            this.grounded = grounded;
            this.groundPlanePos = groundPlanePos;
            this.visualYOffset = visualYOffset;
            this.boundaryMode = boundaryMode;
            this.landed = landed;
            this.verticalVelocityBeforeLanding = verticalVelocityBeforeLanding;
        }
    }

    /// <summary>
    /// 对齐正式版逻辑的角色和简单 LF2 对象物理计算层。
    /// 角色移动会消费碰撞/边界逻辑设置的分轴边界标志，然后处理垂直位移，
    /// 最后根据地面或空中状态应用摩擦或重力。
    /// </summary>
    /// <summary>
    /// 角色/通用战斗对象的基础物理计算器。
    ///
    /// 这里处理的是非常底层的移动学规则：
    /// x/z 位移、边界阻挡、y 轴落地、地面摩擦、空中重力。
    ///
    /// 更高层的“切什么帧、是否进入受击、是否生成特效”不在这里处理。
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
        public static void UnitFriction(NTSDEntityRuntime runtime)
        {
            if (runtime == null) return;

            runtime.Vx = ApplyReleaseUnitFriction(runtime.Vx);
            runtime.Vz = ApplyReleaseUnitFriction(runtime.Vz);
        }

        /// <summary>
        /// C# 基准工程 Physics.ApplyUnitFriction：速度每 tick 向 0 收敛 1，越过 0 时直接归零。
        /// </summary>
        private static double ApplyReleaseUnitFriction(double velocity) // P0-f: double sim velocity
        {
            if (velocity > 0.0001f)
            {
                velocity -= 1.0f;
                if (velocity < 0.0001f)
                    velocity = 0.0f;
                return velocity;
            }

            if (velocity < -0.0001f)
            {
                velocity += 1.0f;
                if (velocity > 0.0001f)
                    velocity = 0.0f;
            }

            return velocity;
        }

        /// <summary>
        /// 执行一次角色物理 tick。
        /// 正式版边界标志只会阻止与速度方向匹配的轴向移动，并在本 tick 后清零。
        /// </summary>
        // 执行一次角色物理 tick。
        // 这是理解“位置为什么这么变”的核心入口。
        public MechanicsStepResult Step(in CharacterMechanicsContext ctx)
        {
            var runtime = ctx.Runtime;
            if (runtime == null)
            {
                return new MechanicsStepResult(true, Vector2.zero, 0f, BoundaryResolveMode.None);
            }

            BoundaryResolveMode boundaryMode = BoundaryResolveMode.None;
            double oldX = runtime.X;
            double oldZ = runtime.Z;
            int groundedSnapshotY = runtime.YInt;
            bool startedGrounded = groundedSnapshotY >= 0;

            // C++ 正式版保留速度，只在本 tick 跳过被阻挡轴的位移。
            bool blockedX = (runtime.Vx > 0f && runtime.XBoundPositive) || (runtime.Vx < 0f && runtime.XBoundNegative);
            bool blockedZ = (runtime.Vz > 0f && runtime.ZBoundPositive) || (runtime.Vz < 0f && runtime.ZBoundNegative);

            if (!blockedX) runtime.X += runtime.Vx;
            if (!blockedZ) runtime.Z += runtime.Vz;

            if (blockedX && blockedZ) boundaryMode = BoundaryResolveMode.Stop;
            else if (blockedX) boundaryMode = BoundaryResolveMode.ZOnly;
            else if (blockedZ) boundaryMode = BoundaryResolveMode.XOnly;

            runtime.ClearBounds();

            // Unity 可走边界适配：正式版的场景边界不属于 Entity 物理本体。
            if (ctx.isPointWalkable != null &&
                !IsMovementWalkable(ctx.isPointWalkable, NTSDRenderSpace.GroundPixelToWorld((float)runtime.X, (float)runtime.Z)))
            {
                runtime.X = oldX;
                runtime.Z = oldZ;
                boundaryMode = BoundaryResolveMode.Stop;
            }

            // 垂直轴：y == 0 表示地面，y < 0 表示空中。
            if (startedGrounded && ctx.mass > 0f)
            {
                UnitFriction(runtime);
            }

            float vyBeforeVerticalMove = (float)runtime.Vy;
            runtime.Y += runtime.Vy;

            bool landed = runtime.Y > 0.0001f;

            if (runtime.Y > 0)
            {
                runtime.Y = 0;
            }

            if (ctx.frameData != null && ctx.spriteWidthPx > 0f)
            {
                runtime.UpdateSpriteOrigin(ctx.frameData.centerx, ctx.frameData.centery, ctx.spriteWidthPx);
            }

            if (runtime.Y < 0)
            {
                runtime.Vy += ctx.gravity;
            }



            Vector2 groundPlanePos = NTSDRenderSpace.GroundPixelToWorld((float)runtime.X, (float)runtime.Z);
            float visualYOffset = (float)((-runtime.Y) / SimulationConstants.PIXELS_PER_UNIT);
            bool grounded = (runtime.Y == 0);

            return new MechanicsStepResult(
                grounded,
                groundPlanePos,
                visualYOffset,
                boundaryMode,
                landed,
                vyBeforeVerticalMove
            );
        }

        /// <summary>
        /// 武器专用的正式版动力学。
        /// 边界标志会限制 x/z 位移并随后清零；地面摩擦在 y += vy 之前应用；
        /// 只有 y 位移后仍处于空中时才追加重力。
        /// </summary>
        // 武器的基础动力学与角色不同，所以单独走这个入口。
        public static bool WeaponDynamics(NTSDEntityRuntime runtime, double gravityToAdd, out double oldVy) // P0-f-2a: double gravity; P0-f-2b B1: out double oldVy (no float truncation)
        {
            oldVy = 0.0;
            if (runtime == null) return false;

            if (runtime.Vx > 0 && !runtime.XBoundPositive)
                runtime.X += runtime.Vx;
            else if (runtime.Vx < 0 && !runtime.XBoundNegative)
                runtime.X += runtime.Vx;

            if (runtime.Vz > 0 && !runtime.ZBoundPositive)
                runtime.Z += runtime.Vz;
            else if (runtime.Vz < 0 && !runtime.ZBoundNegative)
                runtime.Z += runtime.Vz;

            runtime.ClearBounds();

            int groundedSnapshotY = runtime.YInt;
            bool startedGrounded = groundedSnapshotY >= 0;
            if (startedGrounded)
            {
                // release non-character friction uses the cached pre-vertical integer grounded snapshot.
                // ordinary LF2OtherObject landing ticks must not receive an extra post-landing unit-friction pass.
                UnitFriction(runtime);
            }

            oldVy = runtime.Vy; // P0-f-2b B1: no (float) truncation — double landing Vy snapshot
            runtime.Y += runtime.Vy;
            bool landed = runtime.Y > 0.0001f && oldVy > 0.0001; // oldVy now double (baseline Vy>0.0001 double); Y-side threshold unchanged (Y not in B1 oldVy/Vy/Vx scope, already green)
            if (runtime.Y > 0) runtime.Y = 0;

            if (runtime.Y < -0.0001f)
                runtime.Vy += gravityToAdd;

            return landed;
        }
    }
}
