using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Animation
{
    /// <summary>
    /// 暴力遍历场景查询实现
    /// 遍历 SimulationWorld 中所有 LF2LivingObject 做碰撞检测
    /// 后续可替换为四叉树等空间分区实现
    /// </summary>
    public class BruteForceSceneQuery : ILF2SceneQuery
    {
        private readonly SimulationWorld _world;
        private readonly List<LF2BlockingObstacle> _blockingObstacles = new List<LF2BlockingObstacle>(64);

        // 复用列表，减少 GC
        private readonly List<LF2LivingObject> _tmpResult = new List<LF2LivingObject>(16);
        private readonly List<LF2LivingObject> _tmpItrResult = new List<LF2LivingObject>(16);
        private readonly List<LF2LivingObject> _tmpAllObjects = new List<LF2LivingObject>(32);
        private readonly List<PhysicsState.FlfVolume> _tmpActorBodies = new List<PhysicsState.FlfVolume>(8);
        private readonly List<PhysicsState.FlfVolume> _tmpItr14 = new List<PhysicsState.FlfVolume>(8);
        private readonly List<PhysicsState.FlfVolume> _tmpTargetBodies = new List<PhysicsState.FlfVolume>(8);

        public BruteForceSceneQuery(SimulationWorld world)
        {
            _world = world;
        }

        // ==================== ILF2SceneQuery ====================

        public List<LF2LivingObject> QueryBodies(in PhysicsState.FlfVolume vol, LF2LivingObject exclude)
        {
            _tmpResult.Clear();
            _world.GetAllLivingObjects(_tmpAllObjects);

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2LivingObject target = _tmpAllObjects[i];
                if (target == exclude) continue;
                if (target.PS == null || target.Frame.D == null) continue;

                float spriteWidthPx = target.GetSpriteWidthPxForCollision();
                if (spriteWidthPx <= 0f) continue;

                target.PS.FillBodyVolumes(
                    _tmpTargetBodies,
                    target.Frame.D.bodies,
                    target.Frame.D.centerx,
                    target.Frame.D.centery,
                    spriteWidthPx,
                    NTSDGlobal.Default.Itr.ZWidth
                );

                for (int b = 0; b < _tmpTargetBodies.Count; b++)
                {
                    if (CollisionUtil.Intersect(vol, _tmpTargetBodies[b]))
                    {
                        _tmpResult.Add(target);
                        break; // 同一个 target 只加一次
                    }
                }
            }

            return _tmpResult;
        }

        public List<LF2LivingObject> QueryItrs(in PhysicsState.FlfVolume vol, LF2LivingObject exclude, int itrKind, int excludeTeam = 0)
        {
            _tmpItrResult.Clear();
            _world.GetAllLivingObjects(_tmpAllObjects);

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2LivingObject target = _tmpAllObjects[i];
                if (target == exclude) continue;
                if (target.PS == null || target.Frame?.D == null) continue;
                if (excludeTeam != 0 && target.Team == excludeTeam) continue;

                var itrs = target.Frame.D.itrs;
                if (itrs == null || itrs.Count == 0) continue;

                float spriteWidthPx = target.GetSpriteWidthPxForCollision();
                if (spriteWidthPx <= 0f) continue;

                bool matched = false;
                for (int j = 0; j < itrs.Count; j++)
                {
                    if (!MatchItrKind(itrs[j].kind, itrKind)) continue;

                    var itrVol = target.PS.GetItrVolume(itrs[j], target.Frame.D.centerx, target.Frame.D.centery, spriteWidthPx);
                    if (CollisionUtil.Intersect(vol, itrVol))
                    {
                        _tmpItrResult.Add(target);
                        matched = true;
                        break;
                    }
                }
            }

            return _tmpItrResult;
        }

        // 对应 FLF global.js GC.match_itr_kind
        private static readonly Dictionary<int, int[]> ItrTypeMap = new Dictionary<int, int[]>
        {
            { 2,  new[] { 2, 1, 4, 21, 5 } },
            { 1,  new[] { 1, 21, 17 } },
            { 4,  new[] { 4, 10, 19 } },
            { 5,  new[] { 5, 19 } },
            { 6,  new[] { 6, 18 } },
            { 7,  new[] { 7, 4, 10 } },
            { 9,  new[] { 9, 2 } },
            { 10, new[] { 10, 1 } },
            { 32, new[] { 32, 19 } },
            { 33, new[] { 33, 19, 16 } },
            { 34, new[] { 34, 10, 5, 14 } },
            { 36, new[] { 36, 16 } },
            { 39, new[] { 39, 10 } },
            { 50, new[] { 50, 4, 18, 7, 21, 5, 14, 17 } },
            { 51, new[] { 51, 2, 18, 7 } },
            { 52, new[] { 52, 1, 2, 21 } },
        };

        private static bool MatchItrKind(int itrKind, int targetKind)
        {
            if (ItrTypeMap.TryGetValue(targetKind, out var types))
            {
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i] == itrKind) return true;
                }
                return false;
            }
            return itrKind == targetKind;
        }

        public bool TestBlockingXZ(LF2LivingObject actor, float vxPx, float vzPx)
        {
            if (actor == null || actor.PS == null) return false;
            if (_blockingObstacles.Count == 0) return false;

            var frame = actor.Frame.D;
            if (frame == null) return false;

            float spriteWidthPx = actor.GetSpriteWidthPxForCollision();
            if (spriteWidthPx <= 0f) return false;

            // 对齐 FLF mech.blocking_xz():
            // 用当前 frame 的 body 体积，带 offset(vx,vz) 预测下一步位置
            // 并将 body.zwidth 置为 0（FLF: body[i].zwidth = 0）
            actor.PS.FillBodyVolumes(
                _tmpActorBodies,
                frame.bodies,
                frame.centerx,
                frame.centery,
                spriteWidthPx,
                zwidthPx: 0f,
                offsetX: vxPx,
                offsetY: 0f,
                offsetZ: vzPx
            );

            if (_tmpActorBodies.Count == 0) return false;

            for (int i = _blockingObstacles.Count - 1; i >= 0; i--)
            {
                var obs = _blockingObstacles[i];
                if (obs == null || !obs.isActiveAndEnabled)
                {
                    _blockingObstacles.RemoveAt(i);
                    continue;
                }

                int count = obs.FillItr14Volumes(_tmpItr14);
                if (count <= 0) continue;

                for (int b = 0; b < _tmpActorBodies.Count; b++)
                {
                    var body = _tmpActorBodies[b];
                    for (int k = 0; k < _tmpItr14.Count; k++)
                    {
                        if (CollisionUtil.Intersect(body, _tmpItr14[k]))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void RegisterBlockingObstacle(LF2BlockingObstacle obstacle)
        {
            if (obstacle == null) return;
            if (!_blockingObstacles.Contains(obstacle))
                _blockingObstacles.Add(obstacle);
        }

        public void UnregisterBlockingObstacle(LF2BlockingObstacle obstacle)
        {
            if (obstacle == null) return;
            _blockingObstacles.Remove(obstacle);
        }
    }

    /// <summary>
    /// 碰撞工具方法（纯几何，无业务语义）
    /// </summary>
    public static class CollisionUtil
    {
        /// <summary>
        /// 对齐 FLF scene.js: intersect()
        /// rect_flat + zwidth 区间检测
        /// </summary>
        public static bool Intersect(in PhysicsState.FlfVolume a, in PhysicsState.FlfVolume b)
        {
            float aLeft = a.x + a.vx;
            float aTop = a.y + a.vy;
            float aRight = aLeft + a.w;
            float aBottom = aTop + a.h;

            float bLeft = b.x + b.vx;
            float bTop = b.y + b.vy;
            float bRight = bLeft + b.w;
            float bBottom = bTop + b.h;

            if (aBottom < bTop) return false;
            if (aTop > bBottom) return false;
            if (aRight < bLeft) return false;
            if (aLeft > bRight) return false;

            float aZMin = a.z - a.zwidth;
            float aZMax = a.z + a.zwidth;
            float bZMin = b.z - b.zwidth;
            float bZMax = b.z + b.zwidth;

            if (aZMax < bZMin) return false;
            if (aZMin > bZMax) return false;

            return true;
        }
    }
}
