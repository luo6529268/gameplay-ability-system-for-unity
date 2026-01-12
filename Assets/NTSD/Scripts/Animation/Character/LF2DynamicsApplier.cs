using System;
using NTSD.Simulation;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// 物理运算桥接器：对齐 FLF 的 mech.dynamics() + blocking_xz()，并将结果写回 Unity 组件。
    ///
    /// 设计目标：
    /// - 将“物理运算相关逻辑”从 LF2CharacterAnimator 中剥离（便于后续扩展/替换实现）。
    /// - 保持每 tick 无额外分配（复用已有委托/缓存）。
    /// </summary>
    public static class LF2DynamicsApplier
    {
        public static void Apply(
            LF2CharacterAnimator animator,
            CharacterMechanics mechanics,
            float mass,
            Func<Vector2, bool> isPointWalkable,
            Action<string> logWarning,
            bool debugCollisionLog,
            Transform groundTransform,
            Vector3 baseLocalPosition)
        {
            if (animator == null) return;
            if (animator.unitActions == null || animator.ps == null || mechanics == null) return;
            if (groundTransform == null) return;

            float blockedMoveScale = LF2CollisionSystem.BlockingXZ(animator) ? 0.1f : 1f;

            bool hasStageBounds = false;
            LF2StageBoundsPx stageBoundsPx = default;
            var boundsProvider = NTSD.LevelEditor.BoundaryWallManager.Instance;
            if (boundsProvider != null && boundsProvider.TryGetStageBoundsPx(out stageBoundsPx))
            {
                hasStageBounds = true;
            }

            var ctx = new CharacterMechanicsContext(
                ps: animator.ps,
                frameData: animator.CurrentFrame,
                spriteWidthPx: animator.GetSpriteWidthPxForCollision(),
                hasStageBounds: hasStageBounds,
                stageBoundsPx: stageBoundsPx,
                mass: mass,
                minSpeed: NTSDGlobal.Gameplay.MinSpeed,
                gravity: NTSDGlobal.Gameplay.Gravity,
                blockedMoveScale: blockedMoveScale,
                isPointWalkable: isPointWalkable,
                logWarning: logWarning
            );

            var result = mechanics.Step(ctx);

            if (debugCollisionLog && result.boundaryMode != BoundaryResolveMode.None)
            {
                Tools.Log.Info("[Boundary] ResolveMode={0}", result.boundaryMode);
            }

            // ground plane（Unity X/Y）写回
            groundTransform.position = new Vector3(
                result.groundPlanePos.x,
                result.groundPlanePos.y,
                groundTransform.position.z
            );

            // 视觉高度偏移（Unity local Y）
            animator.transform.localPosition = baseLocalPosition + new Vector3(0f, result.visualYOffset, 0f);

            // BeatEmUp / 排序 / 影子用数据
            animator.unitActions.yForce = 0f;
            animator.unitActions.isGrounded = result.grounded;
            animator.unitActions.groundPos = groundTransform.position.y;
        }
    }
}
