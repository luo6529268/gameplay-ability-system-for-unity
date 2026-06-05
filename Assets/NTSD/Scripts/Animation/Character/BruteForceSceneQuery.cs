using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;

namespace NTSD.Animation
{
    /// <summary>
    /// 遍历全部运行时实体的战斗场景查询器。
    /// </summary>
    public class BruteForceSceneQuery : ILF2SceneQuery
    {
        private readonly SimulationWorld _world;
        private readonly List<LF2Entity> _tmpResult = new List<LF2Entity>(16);
        private readonly List<LF2Entity> _tmpAllObjects = new List<LF2Entity>(32);

        public BruteForceSceneQuery(SimulationWorld world)
        {
            _world = world;
        }

        public List<LF2Entity> QueryBodies(in PhysicsState.BattleVolume vol, LF2Entity exclude)
        {
            _tmpResult.Clear();
            _world.GetAllEntities(_tmpAllObjects);

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity target = _tmpAllObjects[i];
                if (target == exclude) continue;
                if (target.PS == null || target.Frame?.D == null) continue;

                float spriteWidthPx = target.GetSpriteWidthPxForCollision();
                if (spriteWidthPx <= 0f)
                {
                    continue;
                }

                var bodyVolumes = target.PS.GetBodyVolumes(
                    target.Frame.D.bodies,
                    target.Frame.D.centerx,
                    target.Frame.D.centery,
                    spriteWidthPx
                );

                for (int b = 0; b < bodyVolumes.Count; b++)
                {
                    bool intersects = CollisionUtil.Intersect(vol, bodyVolumes[b]);
                    if (intersects)
                    {
                        _tmpResult.Add(target);
                        break;
                    }
                }
            }

            return _tmpResult;
        }

    }

    /// <summary>
    /// 纯碰撞工具函数，不持有战斗逻辑状态。
    /// </summary>
    public static class CollisionUtil
    {
        public static bool Intersect(in PhysicsState.BattleVolume a, in PhysicsState.BattleVolume b)
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
