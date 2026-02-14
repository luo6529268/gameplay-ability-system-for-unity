using System.Collections.Generic;
using NTSD.Animation.LF2Objects;

namespace NTSD.Animation
{
    /// <summary>
    /// FLF 风格场景查询接口（对应 FLF scene.js 的 query/intersect）
    /// 
    /// 当前实现：BruteForceSceneQuery（暴力遍历）
    /// 后续可替换为四叉树等空间分区实现
    /// </summary>
    public interface ILF2SceneQuery
    {
        /// <summary>
        /// 查询与 vol 碰撞的所有 LivingObject 的 body 体积（排除 exclude 自身）
        /// 对应 FLF scene.query(vol, $, {tag:'body'})
        /// </summary>
        List<LF2LivingObject> QueryBodies(in PhysicsState.FlfVolume vol, LF2LivingObject exclude);

        /// <summary>
        /// BlockingXZ 检测（对齐 FLF mech.blocking_xz）
        /// 预测下一步是否会被 kind:14 阻挡
        /// </summary>
        bool TestBlockingXZ(LF2LivingObject actor, float vxPx, float vzPx);

        /// <summary>注册阻挡障碍物</summary>
        void RegisterBlockingObstacle(LF2BlockingObstacle obstacle);

        /// <summary>反注册阻挡障碍物</summary>
        void UnregisterBlockingObstacle(LF2BlockingObstacle obstacle);
    }
}
