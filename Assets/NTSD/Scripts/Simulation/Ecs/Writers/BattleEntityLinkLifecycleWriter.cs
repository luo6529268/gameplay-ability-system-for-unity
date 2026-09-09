using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleEntityLinkLifecycleWriter
    {
        internal static void ClearReferencesToReleasedSlot(
            RuntimeSlotTable runtimeSlots,
            int releasedSlot)
        {
            if (runtimeSlots == null ||
                releasedSlot < 0 ||
                releasedSlot >= runtimeSlots.LogicalCapacity)
            {
                return;
            }

            for (int slot = 0; slot < runtimeSlots.LogicalCapacity; slot++)
            {
                LF2Entity entity = runtimeSlots.GetCurrentOccupant(slot);
                NTSDEntityRuntime runtime = entity?.Runtime;
                if (runtime == null)
                    continue;

                // Alignment contract:
                // NTSD28-B6-ENTITY-LINK-LIFECYCLE-CLEANUP-PRODUCTION-001.
                if (runtime.TargetSlotIndex == releasedSlot)
                {
                    runtime.TargetSlotIndex = 0;
                    runtime.LinkState = 0;
                    runtime.HeldWeaponStableId = -1;
                    runtime.ThrowFrameGuard = -1;
                    if (entity is LF2Character holder)
                        holder.HeldWeaponReferenceInternal = null;
                }

                if (runtime.LinkState != 0 &&
                    runtime.HolderStableId == releasedSlot)
                {
                    runtime.LinkState = 0;
                    runtime.HolderStableId = 0;
                }

                bool catchTargetReleased =
                    runtime.CaughtSlotIndex == releasedSlot;
                if (catchTargetReleased)
                {
                    runtime.CaughtSlotIndex = -1;
                    runtime.CaughtDuration = 0;
                }

                int catchSourceSlot = runtime.CatchSourceSlot90 >= 0x2000
                    ? runtime.CatchSourceSlot90 - 0x2000
                    : runtime.CatchSourceSlot90;
                bool catchSourceReleased = catchSourceSlot == releasedSlot;
                if (catchSourceReleased)
                {
                    runtime.CatchSourceSlot90 = -1;
                    runtime.CaughtDuration = 0;
                    if (runtime.CatcherSlotIndex == releasedSlot)
                        runtime.CatcherSlotIndex = -1;
                }

                if ((catchTargetReleased || catchSourceReleased) &&
                    entity is LF2LivingObject living)
                {
                    living.Catching = null;
                }
            }
        }
    }
}
