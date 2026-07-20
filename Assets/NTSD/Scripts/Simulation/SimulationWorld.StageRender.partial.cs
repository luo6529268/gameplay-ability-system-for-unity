using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的关卡边界、摄像机和渲染相关 pass。
    /// </summary>
    public partial class SimulationWorld
    {
        private bool _hasExplicitStageRuntimeSnapshot;

        public void SetExplicitStageRuntimeSnapshotForTesting(
            int stageWidth,
            int zMin,
            int zMax,
            int perspectiveNear,
            int perspectiveFar)
        {
            Runtime?.Stage?.SetSceneSnapshot(stageWidth, zMin, zMax, perspectiveNear, perspectiveFar);
            _hasExplicitStageRuntimeSnapshot = true;
        }

        private static void ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out int perspectiveNear, out int perspectiveFar)
        {
            var cfg = NTSD.App.GameConfig.Instance;
            stageWidth = cfg != null ? Mathf.Max(cfg.BattleStageWidthPx, NTSDRenderSpace.SourceScreenWidth) : 800;
            zMin = cfg != null ? cfg.BattleStageZMinPx : 180;
            zMax = cfg != null ? Mathf.Max(cfg.BattleStageZMaxPx, zMin + 1) : 350;
            perspectiveNear = cfg != null ? cfg.BattlePerspectiveNear : 0;
            perspectiveFar = cfg != null ? cfg.BattlePerspectiveFar : 0;

            BoundaryWallManager manager = BoundaryWallManager.Instance;
            if (manager != null && manager.TryGetBattleStageRuntime(out int boundaryStageWidth, out int boundaryZMin, out int boundaryZMax))
            {
                stageWidth = boundaryStageWidth;
                zMin = boundaryZMin;
                zMax = boundaryZMax;
            }
        }

        public bool IsGroundPointWalkable(Vector2 pointXY)
        {
            BoundaryWallManager manager = BoundaryWallManager.Instance;
            if (manager == null)
                return true;

            return manager.IsPointWalkable(pointXY);
        }

        public void RefreshStageRuntimeSnapshotFromScene()
        {
            if (_hasExplicitStageRuntimeSnapshot)
                return;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out int perspectiveNear, out int perspectiveFar);
            Runtime?.Stage?.SetSceneSnapshot(stageWidth, zMin, zMax, perspectiveNear, perspectiveFar);
        }

        public void ClampCharacterZToStageBoundsAll()
        {
            RefreshStageRuntimeSnapshotFromScene();
            float zMin = Runtime?.Stage?.ZMin ?? 180;
            float zMax = Runtime?.Stage?.ZMax ?? 350;
            if (zMax < zMin)
                return;

            ForEachEntityByRuntimeSlot(entity =>
            {
                if (!entity.IsStageBoundedCharacter() || entity.PS == null)
                    return;

                if (entity.PS.z > zMax) entity.PS.z = zMax;
                if (entity.PS.z < zMin) entity.PS.z = zMin;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void ApplyPreFrameBoundsAll()
        {
            RefreshStageRuntimeSnapshotFromScene();
            int stageWidthPx = Runtime?.Stage?.StageWidthPx ?? 800;
            int baseStageWidthPx = Runtime?.Stage?.BaseStageWidthPx ?? 800;
            int xMaxOverride = Runtime?.Stage?.XMaxOverride ?? 0;
            int stageZMin = Runtime?.Stage?.ZMin ?? 180;
            int stageZMax = Runtime?.Stage?.ZMax ?? 350;

            float zMin = stageZMin;
            float zMax = stageZMax;
            float baseStageWidth = baseStageWidthPx;
            if (zMax < zMin || baseStageWidth <= 0f)
                return;

            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity == null || entity.PS == null)
                    return;

                entity.ApplyPreFrameZBounds(zMin, zMax);

                bool destroyed = entity.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride);
                if (!destroyed)
                    RefreshRuntimeSnapshot(entity);
            });

            ResetUnityFixedWorldRenderOffsets();
        }

        public void RenderDispatchAll(int tickIndex)
        {
            LateRendererUpdateAll(tickIndex);
        }

        internal void ResetUnityFixedWorldRenderOffsets()
        {
            // Unity battle scenes use fixed world coordinates. Keep entity, shadow,
            // and spark presentation independent from character-driven camera math.
            _cameraX = 0;
            _cameraVel = 0;
            GetAllEntities(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity?.Runtime == null)
                    continue;

                entity.Runtime.RenderOffsetX = 0f;
            }

            _entityScratch.Clear();
        }

        private List<ISimObject> BuildRendererSnapshot()
        {
            var snapshot = new List<ISimObject>();
            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return snapshot;

            foreach (int key in bucketKeys)
            {
                if (!_buckets.TryGetValue(key, out Bucket bucket)) continue;
                bucket.EnsureSorted(GetRuntimeStableId);
                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity) continue;
                    if (bucket.items[i] is LF2ObjectRenderer)
                        snapshot.Add(bucket.items[i]);
                }
            }

            snapshot.Sort((a, b) => GetRuntimeStableId(a).CompareTo(GetRuntimeStableId(b)));
            return snapshot;
        }

        private void LateRendererUpdateAll(int tickIndex)
        {
            var snapshot = BuildRendererSnapshot();
            for (int i = 0; i < snapshot.Count; i++)
            {
                ISimObject obj = snapshot[i];
                if (obj == null || !IsActiveForCurrentPass(obj))
                    continue;

                obj.SimLateTick(tickIndex);
            }
        }

        public void UpdateBattleResultsFlow()
        {
            BattleRuntimeState battle = Runtime;
            if (battle?.Match?.BattleGameModeId != 1)
                return;

            battle.Results ??= new BattleResultsRuntimeState();
            BattleResultsRuntimeState results = battle.Results;
            if (results.IsActive)
                return;

            BattleSlotRuntimeState[] rosterSlots = battle.Roster?.Slots;
            if (rosterSlots == null)
                return;

            int[] teamIds = { -1, -1 };
            int[] alive = new int[2];
            int teamCount = 0;
            int slotCount = rosterSlots.Length < 8 ? rosterSlots.Length : 8;

            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[slotIndex];
                if (rosterSlot == null)
                    continue;

                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(rosterSlot.RuntimeSlotIndex);
                if (entity == null ||
                    entity.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                {
                    continue;
                }

                // Authority GameTick keeps a fixed 0..7 slot in the result scan and
                // skips only when the slot state is dormant and the bound entity is
                // inactive. An active entity must remain eligible even when its
                // roster metadata has already been marked inactive.
                if (!rosterSlot.Active && !IsActiveForCurrentPass(entity))
                    continue;

                int team = entity.RelationTeam != 0 ? entity.RelationTeam : rosterSlot.Team;
                if (team == 0)
                    continue;

                int bucket = -1;
                for (int i = 0; i < teamCount; i++)
                {
                    if (teamIds[i] == team)
                    {
                        bucket = i;
                        break;
                    }
                }

                if (bucket < 0 && teamCount < teamIds.Length)
                {
                    bucket = teamCount;
                    teamIds[teamCount++] = team;
                }

                if (bucket >= 0 && IsActiveForCurrentPass(entity) && entity.Health != null && entity.Health.HP > 0)
                    alive[bucket]++;
            }

            if (alive[0] > 0 && alive[1] > 0)
                results.HadBoth = true;

            if (!results.HadBoth || teamCount < 2)
                return;

            results.EnsureTeamIds();
            if (alive[0] > 0 && alive[1] > 0)
            {
                results.BattleEndPhase = 0;
                results.PendingWinner = -2;
                results.TeamCount = teamCount;
                results.TeamIds[0] = teamIds[0];
                results.TeamIds[1] = teamIds[1];
                return;
            }

            int decidedWinner = alive[0] > 0 ? 0 : alive[1] > 0 ? 1 : -1;
            if (results.BattleEndPhase == 0)
            {
                results.BattleEndPhase = 1;
                results.PendingWinner = decidedWinner;
            }
            else
            {
                results.BattleEndPhase++;
            }

            results.TeamCount = teamCount;
            results.TeamIds[0] = teamIds[0];
            results.TeamIds[1] = teamIds[1];

            if (results.BattleEndPhase >= 11)
                results.ActivateSummary(results.PendingWinner, teamCount, teamIds[0], teamIds[1]);
        }
    }
}
