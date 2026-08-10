using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns runtime-slot queries and held-object link maintenance for one match.
    /// The world keeps the public compatibility surface and delegates to this
    /// instance instead of spreading the implementation across partial types.
    /// </summary>
    internal sealed class SimulationQueryAndLinkModule
    {
        private readonly SimulationWorld world;

        internal SimulationQueryAndLinkModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal bool ResetCooldownsForRuntimeSlot(
            int runtimeSlot,
            LF2Entity occupant)
        {
            RuntimeSlotTable runtimeSlots = world.RuntimeSlotsForServices;
            if (runtimeSlot < 0 || runtimeSlot >= runtimeSlots.LogicalCapacity)
                return false;

            RuntimeRestStore restStore = world.RuntimeRestStoreForServices;
            if (!restStore.ResetSlot(runtimeSlot))
                return false;
            occupant?.ItrRest?.Reset();
            return occupant?.ItrRest == null ||
                   occupant.ItrRest.Bind(restStore, runtimeSlot, false);
        }

        internal void HeldObjectProcessAll(int tickIndex)
        {
            RuntimeSlotTable runtimeSlots = world.RuntimeSlotsForServices;
            for (int runtimeSlot = 0;
                 runtimeSlot < runtimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                LF2Entity held = runtimeSlots.GetCurrentOccupant(runtimeSlot);
                if (!world.IsActiveForCurrentPassInternal(held) ||
                    held.Runtime.LinkState >= 0)
                {
                    continue;
                }

                LF2Entity holder = FindEntityByRuntimeSlotCurrent(
                    held.Runtime.HolderStableId);
                if (holder == null ||
                    holder.Runtime.TargetSlotIndex != GetRuntimeSlotOrder(held))
                {
                    held.Runtime.LinkState = 0;
                    held.Runtime.HolderStableId = -1;
                    held.RefreshRuntimeSnapshot();
                    continue;
                }

                LF2FrameData holderFrame = holder.Frame?.D;
                WeaponPoint wpoint = holderFrame?.wpoints != null &&
                                     holderFrame.wpoints.Count > 0
                    ? holderFrame.wpoints[0]
                    : world.BattleBuffersForServices.DefaultHeldObjectWeaponPoint;

                if (!LF2HeldObjectRuntime.RunStep12(
                        holder,
                        held,
                        wpoint,
                        out WeaponActResult actResult))
                {
                    continue;
                }

                WeaponAttackResult attackResult = actResult.AttackResult;
                if (attackResult.HitUid != 0 && attackResult.ARest > 0 &&
                    holder.ItrRest != null)
                {
                    holder.ItrRest.Arest = attackResult.ARest;
                }

                holder.RefreshRuntimeSnapshot();
                held.RefreshRuntimeSnapshot();
            }
        }

        internal void ValidateHeldLinksAll(int tickIndex)
        {
            IBattleParityStructuralEventSink eventSink =
                world.StructuralEventSinkForServices;
            if (eventSink != null)
                world.SetStructuralEventContextForDiagnostics(
                    tickIndex,
                    "positive-link-validation");

            RuntimeSlotTable runtimeSlots = world.RuntimeSlotsForServices;
            for (int runtimeSlot = 0;
                 runtimeSlot < runtimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                LF2Entity holder = runtimeSlots.GetCurrentOccupant(runtimeSlot);
                if (!world.IsActiveForCurrentPassInternal(holder))
                    continue;

                int holderSlot = GetRuntimeSlotOrder(holder);
                if (holderSlot < 0 ||
                    holderSlot >= runtimeSlots.LogicalCapacity ||
                    holder.Runtime.LinkState <= 0)
                {
                    continue;
                }

                int targetRuntimeSlot = holder.Runtime.TargetSlotIndex;
                LF2Entity target = targetRuntimeSlot >= 0 &&
                                   targetRuntimeSlot < runtimeSlots.LogicalCapacity
                    ? FindEntityByRuntimeSlotCurrent(targetRuntimeSlot)
                    : null;
                bool targetActive = target != null;
                int observedHolderSlot = targetActive
                    ? target.Runtime.HolderStableId
                    : -1;
                int beforeLinkState = 0;
                int beforeTargetSlot = -1;
                int beforeHeldWeaponSlot = -1;
                int targetBeforeLinkState = 0;
                if (eventSink != null)
                {
                    beforeLinkState = holder.Runtime.LinkState;
                    beforeTargetSlot = holder.Runtime.TargetSlotIndex;
                    beforeHeldWeaponSlot = holder.Runtime.HeldWeaponStableId;
                    targetBeforeLinkState = targetActive
                        ? target.Runtime.LinkState
                        : 0;
                }

                bool valid = targetActive && observedHolderSlot == holderSlot;
                if (!valid)
                {
                    holder.Runtime.LinkState = 0;
                    holder.Runtime.TargetSlotIndex = -1;
                    holder.Runtime.HeldWeaponStableId = -1;
                    holder.RefreshRuntimeSnapshot();
                }

                if (eventSink == null)
                    continue;

                int targetAfterHolderSlot = targetActive
                    ? target.Runtime.HolderStableId
                    : -1;
                int targetAfterLinkState = targetActive
                    ? target.Runtime.LinkState
                    : 0;
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
                    Reason = valid
                        ? "reciprocal"
                        : targetActive
                            ? "holder-mismatch"
                            : "target-inactive",
                    TargetBeforeHolderSlot = observedHolderSlot,
                    TargetBeforeLinkState = targetBeforeLinkState,
                    TargetAfterHolderSlot = targetAfterHolderSlot,
                    TargetAfterLinkState = targetAfterLinkState,
                });
            }
        }

        internal LF2Entity FindEntityByRuntimeSlotCurrent(int runtimeSlot)
        {
            LF2Entity entity =
                world.RuntimeSlotsForServices.GetCurrentOccupant(runtimeSlot);
            return world.IsActiveForCurrentPassInternal(entity) ? entity : null;
        }

        internal LF2Entity FindEntityByRuntimeSlotIncludingDormant(int runtimeSlot)
        {
            return world.RuntimeSlotsForServices.GetCurrentOccupant(runtimeSlot);
        }

        internal void GetAllLivingObjects(List<LF2LivingObject> destination)
        {
            if (destination == null)
                return;
            destination.Clear();

            RuntimeSlotTable runtimeSlots = world.RuntimeSlotsForServices;
            for (int runtimeSlot = 0;
                 runtimeSlot < runtimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                if (runtimeSlots.GetCurrentOccupant(runtimeSlot) is
                        LF2LivingObject living &&
                    world.IsActiveForCurrentPassInternal(living))
                {
                    destination.Add(living);
                }
            }
        }

        internal void GetAllEntities(List<LF2Entity> destination)
        {
            if (destination == null)
                return;
            destination.Clear();

            RuntimeSlotTable runtimeSlots = world.RuntimeSlotsForServices;
            for (int runtimeSlot = 0;
                 runtimeSlot < runtimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                LF2Entity entity = runtimeSlots.GetCurrentOccupant(runtimeSlot);
                if (world.IsActiveForCurrentPassInternal(entity))
                    destination.Add(entity);
            }
        }

        internal void GetActiveEntitiesByRuntimeSlot(List<LF2Entity> destination)
        {
            GetAllEntities(destination);
        }

        private static int GetRuntimeSlotOrder(LF2Entity entity)
        {
            if (entity == null)
                return int.MaxValue;
            int slot = entity.Runtime?.SlotIndex ?? -1;
            return slot >= 0 ? slot : entity.StableId;
        }
    }
}
