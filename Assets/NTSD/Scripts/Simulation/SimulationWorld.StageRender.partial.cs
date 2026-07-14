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

            UpdateReleaseCameraAndRenderOffsets(stageWidthPx, stageZMin, stageZMax);
        }

        public void RenderDispatchAll(int tickIndex)
        {
            LateRendererUpdateAll(tickIndex);
        }

        private void UpdateReleaseCameraAndRenderOffsets(int stageWidth, int zMin, int zMax)
        {
            int maxCam = stageWidth - NTSDRenderSpace.SourceScreenWidth;

            if (maxCam > 0)
            {
                int sumX = 0;
                int count = 0;
                GetAllEntities(_entityScratch);

                for (int i = 0; i < _entityScratch.Count; i++)
                {
                    LF2Entity entity = _entityScratch[i];
                    if (entity == null || !entity.ShouldContributeToReleaseCamera() || entity.PS == null)
                        continue;

                    int slotIndex = entity.Runtime?.SlotIndex ?? -1;
                    if (slotIndex < 0 || slotIndex >= 8)
                        continue;

                    int state = entity.Frame?.D?.state ?? 0;
                    int xInt = entity.GetRuntimeXInt();
                    int facing = entity.PS.dir == "left" ? -1 : 1;
                    int px = state == 14 ? xInt : (xInt - facing * 260 + 130);
                    sumX += px;
                    count++;
                }

                if (count == 0)
                {
                    for (int i = 0; i < _entityScratch.Count; i++)
                    {
                        LF2Entity entity = _entityScratch[i];
                        if (entity == null || !entity.ShouldContributeToReleaseCamera() || entity.PS == null)
                            continue;

                        sumX += entity.GetRuntimeXInt();
                        count++;
                    }

                    if (count == 0)
                    {
                        sumX = 800;
                        count = 1;
                    }
                }

                _entityScratch.Clear();

                int target = count > 0 ? (sumX / count - 397) : 0;
                if (target < 0) target = 0;
                if (target > maxCam) target = maxCam;

                int diff = target - _cameraX;
                int step = diff / 14;
                _cameraVel = (step + _cameraVel * 6) / 7;
                if (_cameraVel == 0 && diff != 0)
                    _cameraVel = diff > 0 ? 1 : -1;

                _cameraX += _cameraVel;
                if (_cameraX < 0) _cameraX = 0;
                if (_cameraX > maxCam) _cameraX = maxCam;
            }
            else
            {
                _cameraX = 0;
                _cameraVel = 0;
            }

            UpdateRenderOffsets(stageWidth, zMin, zMax);
        }

        private void UpdateRenderOffsets(int stageWidth, int zMin, int zMax)
        {
            GetAllEntities(_entityScratch);

            int perspectiveNear = Runtime?.Stage?.PerspectiveNear ?? 0;
            int perspectiveFar = Runtime?.Stage?.PerspectiveFar ?? 0;
            bool hasPerspective = (perspectiveNear != 0 || perspectiveFar != 0) && zMax != zMin;
            int zRange = zMax - zMin;

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity == null)
                    continue;

                float renderOffset = 0f;
                if (hasPerspective)
                {
                    int zInt = entity.GetRenderZInt();
                    int xInt = entity.GetRuntimeXInt();
                    int cameraDelta = _cameraX - xInt + 400;
                    double weight =
                        ((double)(zInt - zMin) * perspectiveNear / zRange) +
                        ((double)(zMax - zInt) * perspectiveFar / zRange);
                    renderOffset = (float)(weight * cameraDelta * 0.0025);
                }

                entity.Runtime.RenderOffsetX = renderOffset;
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
    }
}
