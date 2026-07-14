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
    /// SimulationWorld 的实体查询、链路校验和 runtime slot 遍历工具。
    /// </summary>
    public partial class SimulationWorld
    {
        private bool TryGetNextEntityAfterRuntimeSlot(int slotCursor, out LF2Entity nextEntity, out int nextSlot)
        {
            nextEntity = null;
            nextSlot = int.MaxValue;

            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return false;

            foreach (int key in bucketKeys)
            {
                if (!_buckets.TryGetValue(key, out Bucket bucket)) continue;
                bucket.EnsureSorted(GetRuntimeStableId);

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is not LF2Entity entity)
                        continue;
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    int slot = GetRuntimeSlotOrder(entity);
                    if (slot <= slotCursor || slot >= nextSlot)
                        continue;

                    nextSlot = slot;
                    nextEntity = entity;
                }
            }

            return nextEntity != null;
        }

        private void ForEachEntityByRuntimeSlot(System.Action<LF2Entity> action)
        {
            if (action == null) return;

            int slotCursor = -1;
            int safety = 0;
            while (TryGetNextEntityAfterRuntimeSlot(slotCursor, out LF2Entity entity, out int runtimeSlot))
            {
                slotCursor = runtimeSlot;
                if (++safety > 10000)
                {
                    Debug.LogError("[SimulationWorld] runtime slot scan safety break");
                    break;
                }

                if (entity == null || !IsActiveForCurrentPass(entity))
                    continue;

                action(entity);
            }
        }

        public void HeldObjectProcessAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(held =>
            {
                if (held == null || held.Runtime.LinkState >= 0) return;

                LF2Character holder = FindEntityByRuntimeSlotCurrent(held.Runtime.HolderStableId) as LF2Character;
                if (holder == null || holder.Runtime.TargetSlotIndex != GetRuntimeSlotOrder(held))
                {
                    held.Runtime.LinkState = 0;
                    RefreshRuntimeSnapshot(held);
                    return;
                }

                LF2FrameData holderFrame = holder.Frame?.D;
                WeaponPoint wpoint = holderFrame?.wpoints != null && holderFrame.wpoints.Count > 0
                    ? holderFrame.wpoints[0]
                    : new WeaponPoint();

                if (!holder.ReleaseHeldObjectByWPoint(held, wpoint, out var actResult))
                    return;

                if (actResult.NeedsKind3Drop)
                {
                    var dropPoint = new WeaponPoint
                    {
                        kind = 3,
                        x = wpoint.x,
                        y = wpoint.y,
                        weaponact = wpoint.weaponact,
                        cover = wpoint.cover
                    };
                    holder.ReleaseHeldObjectByWPoint(held, dropPoint, out actResult);
                }

                var attackResult = actResult.AttackResult;
                if (attackResult != null && attackResult.HitUid != 0 && attackResult.ARest > 0)
                    holder.ItrRest.Arest = attackResult.ARest;

                RefreshRuntimeSnapshot(holder);
                RefreshRuntimeSnapshot(held);
            });
        }

        public void ValidateHeldLinksAll(int tickIndex)
        {
            ForEachEntityByRuntimeSlot(holder =>
            {
                int holderSlot = GetRuntimeSlotOrder(holder);
                if (holderSlot < 0 || holderSlot >= MaxRuntimeSlots)
                    return;

                if (holder.Runtime.LinkState <= 0)
                    return;

                int targetRuntimeSlot = holder.Runtime.TargetSlotIndex;
                LF2Entity target = targetRuntimeSlot >= 0 && targetRuntimeSlot < MaxRuntimeSlots
                    ? FindEntityByRuntimeSlotCurrent(targetRuntimeSlot)
                    : null;
                if (target == null || target.Runtime.HolderStableId != holderSlot)
                {
                    holder.Runtime.LinkState = 0;
                    RefreshRuntimeSnapshot(holder);
                }
            });
        }

        public LF2Entity FindEntityByRuntimeSlotForQuery(int runtimeSlot)
        {
            return FindEntityByRuntimeSlotCurrent(runtimeSlot);
        }

        public LF2Entity FindEntityByRuntimeSlotIncludingPending(int runtimeSlot)
        {
            return FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
        }

        internal LF2Entity FindEntityByRuntimeSlotIncludingDormant(int runtimeSlot)
        {
            if (runtimeSlot < 0)
                return null;

            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null)
                return null;

            foreach (int key in bucketKeys)
            {
                if (!_buckets.TryGetValue(key, out Bucket bucket))
                    continue;

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity entity &&
                        GetRuntimeSlotOrder(entity) == runtimeSlot)
                    {
                        return entity;
                    }
                }
            }

            return null;
        }

        private LF2Entity FindEntityByRuntimeSlotCurrent(int runtimeSlot)
        {
            if (runtimeSlot < 0) return null;

            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return null;

            foreach (int key in bucketKeys)
            {
                if (!_buckets.TryGetValue(key, out Bucket bucket)) continue;
                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity entity &&
                        IsActiveForCurrentPass(entity) &&
                        GetRuntimeSlotOrder(entity) == runtimeSlot)
                    {
                        return entity;
                    }
                }
            }

            return null;
        }

        public void GetAllLivingObjects(List<LF2LivingObject> dst)
        {
            if (dst == null) return;
            dst.Clear();

            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                bucket.EnsureSorted(GetRuntimeStableId);

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2LivingObject living && IsActiveForCurrentPass(living))
                    {
                        dst.Add(living);
                    }
                }
            }

            dst.Sort(CompareRuntimeSlotOrder);
        }

        public void GetAllEntities(List<LF2Entity> dst)
        {
            if (dst == null) return;
            dst.Clear();

            var bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null) return;

            foreach (int simOrder in bucketKeys)
            {
                if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                bucket.EnsureSorted(GetRuntimeStableId);

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity entity && IsActiveForCurrentPass(entity))
                    {
                        dst.Add(entity);
                    }
                }
            }

            dst.Sort(CompareRuntimeSlotOrder);
        }

        private void GetActiveEntitiesByRuntimeSlot(List<LF2Entity> dst)
        {
            if (dst == null) return;
            dst.Clear();

            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity != null && IsActiveForCurrentPass(entity))
                    dst.Add(entity);
            });
        }
    }
}
