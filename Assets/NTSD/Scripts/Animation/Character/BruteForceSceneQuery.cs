using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation
{
    /// <summary>
    /// Brute-force scene query over all runtime entities.
    /// </summary>
    public class BruteForceSceneQuery : ILF2SceneQuery
    {
        public static bool AttackTraceEnabled;
        public static int AttackTraceAttackerId = -1;

        private readonly SimulationWorld _world;
        private readonly List<LF2Entity> _tmpResult = new List<LF2Entity>(16);
        private readonly List<LF2Entity> _tmpItrResult = new List<LF2Entity>(16);
        private readonly List<LF2Entity> _tmpAllObjects = new List<LF2Entity>(32);

        public BruteForceSceneQuery(SimulationWorld world)
        {
            _world = world;
        }

        public List<LF2Entity> QueryBodies(in PhysicsState.BattleVolume vol, LF2Entity exclude)
        {
            _tmpResult.Clear();
            _world.GetAllEntities(_tmpAllObjects);
            bool traceThisQuery = AttackTraceEnabled && exclude != null && exclude.StableId == AttackTraceAttackerId;

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity target = _tmpAllObjects[i];
                if (target == exclude) continue;
                if (target.PS == null || target.Frame?.D == null) continue;

                float dx = (exclude?.PS != null && target.PS != null) ? (target.PS.x - exclude.PS.x) : 0f;
                float dz = (exclude?.PS != null && target.PS != null) ? (target.PS.z - exclude.PS.z) : 0f;
                bool traceTarget = traceThisQuery && Mathf.Abs(dx) <= 220f && Mathf.Abs(dz) <= 120f;

                float spriteWidthPx = target.GetSpriteWidthPxForCollision();
                if (spriteWidthPx <= 0f)
                {
                    if (traceTarget)
                        Debug.LogError($"[AttackTrace][QueryBodiesSkipWidth] attacker={exclude.StableId} target={target.StableId} width={spriteWidthPx}");
                    continue;
                }

                var bodyVolumes = target.PS.GetBodyVolumes(
                    target.Frame.D.bodies,
                    target.Frame.D.centerx,
                    target.Frame.D.centery,
                    spriteWidthPx
                );

                if (traceTarget)
                {
                    Debug.LogError($"[AttackTrace][QueryBodiesTarget] attacker={exclude.StableId} target={target.StableId} frame={target.Frame.N} state={target.Frame.D.state} pic={target.Frame.D.pic} dx={dx} dz={dz} bodyCount={bodyVolumes.Count}");
                }

                for (int b = 0; b < bodyVolumes.Count; b++)
                {
                    bool intersects = CollisionUtil.Intersect(vol, bodyVolumes[b]);
                    if (traceTarget)
                    {
                        Debug.LogError($"[AttackTrace][QueryBodiesIntersect] attacker={exclude.StableId} target={target.StableId} bodyIndex={b} intersects={intersects} itrVol=({vol.x},{vol.y},{vol.z}; vx={vol.vx},vy={vol.vy},w={vol.w},h={vol.h},zw={vol.zwidth}) bodyVol=({bodyVolumes[b].x},{bodyVolumes[b].y},{bodyVolumes[b].z}; vx={bodyVolumes[b].vx},vy={bodyVolumes[b].vy},w={bodyVolumes[b].w},h={bodyVolumes[b].h},zw={bodyVolumes[b].zwidth})");
                    }
                    if (intersects)
                    {
                        if (traceTarget)
                            Debug.LogError($"[AttackTrace][QueryBodiesAdd] attacker={exclude.StableId} target={target.StableId}");
                        _tmpResult.Add(target);
                        break;
                    }
                }
            }

            return _tmpResult;
        }

        public List<LF2Entity> QueryItrs(in PhysicsState.BattleVolume vol, LF2Entity exclude, int itrKind, int excludeTeam = 0)
        {
            _tmpItrResult.Clear();
            _world.GetAllEntities(_tmpAllObjects);

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity target = _tmpAllObjects[i];
                if (target == exclude) continue;
                if (target.PS == null || target.Frame?.D == null) continue;
                if (excludeTeam != 0 && target.Team == excludeTeam) continue;

                var itrs = target.Frame.D.itrs;
                if (itrs == null || itrs.Count == 0) continue;

                float spriteWidthPx = target.GetSpriteWidthPxForCollision();
                if (spriteWidthPx <= 0f) continue;

                for (int j = 0; j < itrs.Count; j++)
                {
                    if (!MatchesKindAlias(itrs[j].kind, itrKind)) continue;

                    var itrVol = target.PS.GetItrVolume(itrs[j], target.Frame.D.centerx, target.Frame.D.centery, spriteWidthPx);
                    if (CollisionUtil.Intersect(vol, itrVol))
                    {
                        _tmpItrResult.Add(target);
                        break;
                    }
                }
            }

            return _tmpItrResult;
        }

        private bool MatchesKindAlias(int kind, int targetKind)
        {
            return _world?.ItrKindService?.MatchesKindAlias(kind, targetKind)
                   ?? NTSDItrKindService.MatchesKindAliasValue(kind, targetKind);
        }

    }

    /// <summary>
    /// Collision utility methods with no gameplay ownership.
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
