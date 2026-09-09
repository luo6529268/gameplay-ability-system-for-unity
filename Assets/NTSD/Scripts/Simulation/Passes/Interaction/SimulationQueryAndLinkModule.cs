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
        private int heldTraceTick = int.MinValue;
        private int heldTraceOccurrence;

        internal SimulationQueryAndLinkModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal long HeldInvalidReciprocalFailureCountForDiagnostics
        {
            get;
            private set;
        }

        internal int LastHeldInvalidReciprocalFailureCountForDiagnostics
        {
            get;
            private set;
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

        internal bool TryResetAndBindStageSpawnCooldowns(
            int runtimeSlot,
            LF2Entity occupant)
        {
            RuntimeSlotTable runtimeSlots = world.RuntimeSlotsForServices;
            if (runtimeSlot < 0 || runtimeSlot >= runtimeSlots.LogicalCapacity)
                return false;

            RuntimeRestStore restStore = world.RuntimeRestStoreForServices;
            if (occupant == null)
                return false;
            if (occupant.ItrRest == null)
                return restStore.ResetSlot(runtimeSlot);

            return occupant.ItrRest.TryResetAndBind(restStore, runtimeSlot);
        }

        internal void HeldObjectProcessAll(int tickIndex)
        {
            LastHeldInvalidReciprocalFailureCountForDiagnostics = 0;
            if (world.StructuralEventSinkForServices != null)
            {
                if (heldTraceTick != tickIndex)
                {
                    heldTraceTick = tickIndex;
                    heldTraceOccurrence = 0;
                }
                heldTraceOccurrence++;
            }
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

                int heldSlot = GetRuntimeSlotOrder(held);
                int holderSlot = held.Runtime.HolderStableId;
                bool holderSlotInRange =
                    holderSlot >= 0 &&
                    holderSlot < runtimeSlots.LogicalCapacity;
                LF2Entity holder = holderSlotInRange
                    ? FindEntityByRuntimeSlotCurrent(holderSlot)
                    : null;
                if (holder == null ||
                    holder.Runtime.TargetSlotIndex != heldSlot)
                {
                    // Alignment contract:
                    // NTSD28-B6-HELD-INVALID-RECIPROCAL-PRESERVE-PRODUCTION-001.
                    HeldInvalidReciprocalFailureCountForDiagnostics++;
                    LastHeldInvalidReciprocalFailureCountForDiagnostics++;
                    RecordInvalidNegativeHeldRelation(
                        tickIndex,
                        heldSlot,
                        holderSlot,
                        holderSlotInRange,
                        held,
                        holder);
                    continue;
                }

                LF2FrameData holderFrame = holder.Frame?.D;
                BattleWeaponPointValue wpoint = holderFrame != null
                    ? holderFrame.PrimaryWeaponPoint
                    : world.BattleBuffersForServices.DefaultHeldObjectWeaponPoint;

                if (!world.HeldObjectWriter.RunStep12(
                        holder,
                        held,
                        wpoint,
                        out WeaponActResult actResult))
                {
                    continue;
                }

                if (actResult.TerminalDespawnRequested)
                {
                    IBattleParityStructuralEventSink sink = world.StructuralEventSinkForServices;
                    if (sink != null)
                    {
                        string pass = heldTraceOccurrence == 1 ? "held-refill:C09" :
                            heldTraceOccurrence == 2 ? "held-refill:C20" : "held-refill:additional";
                        world.SetStructuralEventContextForDiagnostics(tickIndex, pass);
                        sink.Record(new BattleParityStructuralEvent
                        {
                            Tick = tickIndex,
                            Pass = pass,
                            Action = "held-terminal",
                            CursorSlot = heldSlot,
                            ActorSlot = holderSlot,
                            Slot = heldSlot,
                            Before = "active",
                            After = "terminal-requested",
                            LifecycleEpoch = runtimeSlots.GetAllocationEpoch(heldSlot),
                            SourceKind = "held-object",
                            Reason = "weaponact>=1000",
                        });
                    }

                    // Alignment contract: NTSD28-B6-WPOINT-TERMINAL-STRUCTURAL-PRODUCTION-001.
                    world.TryGetCurrentRuntimeHandle(holderSlot, holder, out RuntimeEntityHandle holderHandle);
                    world.StructuralWriter.Free(held);
                    if (world.TryResolveRuntimeHandle(holderHandle, out LF2Entity liveHolder))
                        liveHolder.RefreshRuntimeSnapshot();
                    continue;
                }

                if (actResult.UnsupportedWeaponAction)
                {
                    IBattleParityStructuralEventSink sink = world.StructuralEventSinkForServices;
                    if (sink != null)
                    {
                        string pass = heldTraceOccurrence == 1 ? "held-refill:C09" :
                            heldTraceOccurrence == 2 ? "held-refill:C20" : "held-refill:additional";
                        world.SetStructuralEventContextForDiagnostics(tickIndex, pass);
                        sink.Record(new BattleParityStructuralEvent
                        {
                            Tick = tickIndex,
                            Pass = pass,
                            Action = "held-unsupported-action",
                            CursorSlot = heldSlot,
                            ActorSlot = holderSlot,
                            Slot = heldSlot,
                            Before = wpoint.WeaponAct.ToString(),
                            After = held.Frame.N.ToString(),
                            LifecycleEpoch = runtimeSlots.GetAllocationEpoch(heldSlot),
                            SourceKind = "held-object",
                            Reason = "child-frame-missing",
                            Outcome = "preserved-continue",
                        });
                    }
                    holder.RefreshRuntimeSnapshot();
                    held.RefreshRuntimeSnapshot();
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

        private void RecordInvalidNegativeHeldRelation(
            int tickIndex,
            int heldSlot,
            int holderSlot,
            bool holderSlotInRange,
            LF2Entity held,
            LF2Entity holder)
        {
            IBattleParityStructuralEventSink eventSink =
                world.StructuralEventSinkForServices;
            if (eventSink == null)
                return;

            world.SetStructuralEventContextForDiagnostics(
                tickIndex,
                "negative-held-validation");
            int holderTargetSlot = holder?.Runtime?.TargetSlotIndex ?? -1;
            int linkState = held.Runtime.LinkState;
            int heldWeaponSlot = held.Runtime.HeldWeaponStableId;
            string relation =
                $"{linkState}/{holderSlot}/{holderTargetSlot}/{heldWeaponSlot}";
            eventSink.Record(new BattleParityStructuralEvent
            {
                Tick = tickIndex,
                Pass = "negative-held-validation",
                Action = "link-validation",
                CursorSlot = heldSlot,
                ActorSlot = heldSlot,
                Slot = heldSlot,
                Before = relation,
                After = relation,
                LifecycleEpoch = world.RuntimeSlotsForServices
                    .GetAllocationEpoch(heldSlot),
                SourceKind = "negative-held",
                BeforeLinkState = linkState,
                BeforeTargetSlot = holderSlot,
                BeforeHeldWeaponSlot = heldWeaponSlot,
                AfterLinkState = linkState,
                AfterTargetSlot = holderSlot,
                AfterHeldWeaponSlot = heldWeaponSlot,
                TargetActive = holder != null,
                ObservedHolderSlot = holderTargetSlot,
                Outcome = "preserved",
                Reason = !holderSlotInRange
                    ? "parent-out-of-range"
                    : holder == null
                        ? "parent-inactive"
                        : "reciprocal-mismatch",
                TargetBeforeHolderSlot = holderTargetSlot,
                TargetBeforeLinkState = holder?.Runtime?.LinkState ?? 0,
                TargetAfterHolderSlot = holderTargetSlot,
                TargetAfterLinkState = holder?.Runtime?.LinkState ?? 0,
            });
        }

        internal void RunLegacyPositiveLinkValidation(int tickIndex)
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
