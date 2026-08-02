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
        internal bool ResetCooldownsForRuntimeSlot(int runtimeSlot, LF2Entity occupant)
        {
            if (runtimeSlot < 0 || runtimeSlot >= RuntimeSlotCapacity)
                return false;

            if (!_runtimeRestStore.ResetSlot(runtimeSlot))
                return false;
            occupant?.ItrRest?.Reset();
            return occupant?.ItrRest == null ||
                   occupant.ItrRest.Bind(_runtimeRestStore, runtimeSlot, false);
        }

        private void ForEachEntityByRuntimeSlot(System.Action<LF2Entity> action)
        {
            if (action == null) return;

            // This is a live ascending scan: a newborn above the cursor joins this pass,
            // while a recycled lower slot waits for the next pass, matching authority order.
            for (int runtimeSlot = 0; runtimeSlot < _runtimeSlots.LogicalCapacity; runtimeSlot++)
            {
                LF2Entity entity = _runtimeSlots.GetCurrentOccupant(runtimeSlot);
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

                LF2Entity holder = FindEntityByRuntimeSlotCurrent(held.Runtime.HolderStableId);
                if (holder == null || holder.Runtime.TargetSlotIndex != GetRuntimeSlotOrder(held))
                {
                    held.Runtime.LinkState = 0;
                    held.Runtime.HolderStableId = -1;
                    RefreshRuntimeSnapshot(held);
                    return;
                }

                LF2FrameData holderFrame = holder.Frame?.D;
                WeaponPoint wpoint = holderFrame?.wpoints != null && holderFrame.wpoints.Count > 0
                    ? holderFrame.wpoints[0]
                    : new WeaponPoint();

                if (!LF2HeldObjectRuntime.RunStep12(holder, held, wpoint, out var actResult))
                    return;

                var attackResult = actResult.AttackResult;
                if (attackResult != null && attackResult.HitUid != 0 && attackResult.ARest > 0)
                {
                    if (holder.ItrRest != null)
                        holder.ItrRest.Arest = attackResult.ARest;
                }

                RefreshRuntimeSnapshot(holder);
                RefreshRuntimeSnapshot(held);
            });
        }

        public void ValidateHeldLinksAll(int tickIndex)
        {
            IBattleParityStructuralEventSink eventSink = structuralEventSink;
            if (eventSink != null)
                SetStructuralEventContextForDiagnostics(tickIndex, "positive-link-validation");

            ForEachEntityByRuntimeSlot(holder =>
            {
                int holderSlot = GetRuntimeSlotOrder(holder);
                if (holderSlot < 0 || holderSlot >= RuntimeSlotCapacity)
                    return;

                if (holder.Runtime.LinkState <= 0)
                    return;

                int targetRuntimeSlot = holder.Runtime.TargetSlotIndex;
                LF2Entity target = targetRuntimeSlot >= 0 && targetRuntimeSlot < RuntimeSlotCapacity
                    ? FindEntityByRuntimeSlotCurrent(targetRuntimeSlot)
                    : null;
                bool targetActive = target != null;
                int observedHolderSlot = targetActive ? target.Runtime.HolderStableId : -1;
                int beforeLinkState = 0;
                int beforeTargetSlot = -1;
                int beforeHeldWeaponSlot = -1;
                int targetBeforeLinkState = 0;
                if (eventSink != null)
                {
                    // These fields exist solely for the structural witness. Keep their
                    // reads out of the normal positive-link validation hot path.
                    beforeLinkState = holder.Runtime.LinkState;
                    beforeTargetSlot = holder.Runtime.TargetSlotIndex;
                    beforeHeldWeaponSlot = holder.Runtime.HeldWeaponStableId;
                    targetBeforeLinkState = targetActive ? target.Runtime.LinkState : 0;
                }
                bool valid = targetActive && observedHolderSlot == holderSlot;
                if (!valid)
                {
                    holder.Runtime.LinkState = 0;
                    holder.Runtime.TargetSlotIndex = -1;
                    holder.Runtime.HeldWeaponStableId = -1;
                    RefreshRuntimeSnapshot(holder);
                }

                if (eventSink != null)
                {
                    int targetAfterHolderSlot = targetActive ? target.Runtime.HolderStableId : -1;
                    int targetAfterLinkState = targetActive ? target.Runtime.LinkState : 0;
                    eventSink.Record(new BattleParityStructuralEvent
                    {
                        Tick = tickIndex,
                        Pass = "positive-link-validation",
                        Action = "link-validation",
                        CursorSlot = holderSlot,
                        ActorSlot = holderSlot,
                        Slot = holderSlot,
                        Before = $"{beforeLinkState}/{beforeTargetSlot}/{beforeHeldWeaponSlot}",
                        After = $"{holder.Runtime.LinkState}/{holder.Runtime.TargetSlotIndex}/{holder.Runtime.HeldWeaponStableId}",
                        SourceKind = "positive-link",
                        BeforeLinkState = beforeLinkState,
                        BeforeTargetSlot = beforeTargetSlot,
                        BeforeHeldWeaponSlot = beforeHeldWeaponSlot,
                        AfterLinkState = holder.Runtime.LinkState,
                        AfterTargetSlot = holder.Runtime.TargetSlotIndex,
                        AfterHeldWeaponSlot = holder.Runtime.HeldWeaponStableId,
                        TargetActive = targetActive,
                        ObservedHolderSlot = observedHolderSlot,
                        Outcome = valid ? "kept" : "cleared",
                        Reason = valid ? "reciprocal" : targetActive ? "holder-mismatch" : "target-inactive",
                        TargetBeforeHolderSlot = observedHolderSlot,
                        TargetBeforeLinkState = targetBeforeLinkState,
                        TargetAfterHolderSlot = targetAfterHolderSlot,
                        TargetAfterLinkState = targetAfterLinkState,
                    });
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
            return _runtimeSlots.GetCurrentOccupant(runtimeSlot);
        }

        private LF2Entity FindEntityByRuntimeSlotCurrent(int runtimeSlot)
        {
            LF2Entity entity = _runtimeSlots.GetCurrentOccupant(runtimeSlot);
            return IsActiveForCurrentPass(entity) ? entity : null;
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

            // Every registered LF2Entity owns one runtime slot. Scanning the slot
            // table therefore produces the same ascending runtime order without a
            // bucket-key snapshot, LINQ sort, or temporary enumerator.
            for (int runtimeSlot = 0;
                 runtimeSlot < _runtimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                LF2Entity entity = _runtimeSlots.GetCurrentOccupant(runtimeSlot);
                if (entity != null && IsActiveForCurrentPass(entity))
                    dst.Add(entity);
            }
        }

        private void GetActiveEntitiesByRuntimeSlot(List<LF2Entity> dst)
        {
            if (dst == null) return;
            dst.Clear();

            for (int runtimeSlot = 0;
                 runtimeSlot < _runtimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                LF2Entity entity = _runtimeSlots.GetCurrentOccupant(runtimeSlot);
                if (entity != null && IsActiveForCurrentPass(entity))
                    dst.Add(entity);
            }
        }
    }
}
