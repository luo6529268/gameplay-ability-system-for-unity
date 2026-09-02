using System;

namespace NTSD.Simulation.Lockstep
{
    /// <summary>
    /// Client-owned S0 seam for constructing and preparing one logic-only world.
    /// It preserves the current bootstrap contract but is not a shared formal factory.
    /// </summary>
    internal static class InProcessBattleWorldBootstrap
    {
        internal static SimulationWorld CreateWorldForBarrier(
            LockstepStartBarrier barrier)
        {
            if (barrier == null)
                throw new ArgumentNullException(nameof(barrier));

            BattleRuntimeWorldSettings settings = barrier.WorldSettings;
            return new SimulationWorld(
                settings.Profile,
                settings.InitialRuntimeSlotCapacity,
                settings.CollisionBroadphase);
        }

        internal static void PrepareWorldForHost(
            LockstepStartBarrier barrier,
            SimulationWorld world)
        {
            if (barrier == null)
                throw new ArgumentNullException(nameof(barrier));
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            if (world.ObjectCount != 0 ||
                world.ClaimedRuntimeSlotCountForServices != 0)
            {
                throw new ArgumentException(
                    "An S0 kernel host requires a fresh world before the start barrier.",
                    nameof(world));
            }
            if (world.RuntimeProfileForServices != barrier.WorldSettings.Profile ||
                world.MaxRuntimeSlotsForServices !=
                    barrier.WorldSettings.InitialRuntimeSlotCapacity ||
                world.CollisionBroadphaseForServices !=
                    barrier.WorldSettings.CollisionBroadphase)
            {
                throw new ArgumentException(
                    "The world does not match the immutable start barrier settings.",
                    nameof(world));
            }

            world.SetLogicOnlyEntityMaterialization(true);
            world.Rng.Seed(barrier.Identity.Seed);
            world.Runtime.Match.Seed = unchecked((int)barrier.Identity.Seed);

            BattleRosterRuntimeState roster = world.Runtime.Roster;
            roster.Reset();
            for (int index = 0; index < barrier.PlayerCount; index++)
            {
                int playerSlot = barrier.CanonicalPlayerSlots[index];
                BattleSlotRuntimeState slot = roster.Slots[playerSlot];
                slot.Active = true;
                slot.IsHuman = true;
                slot.Team = playerSlot + 1;
                slot.InputId = playerSlot + 1;
                roster.ActiveSlotCount++;
            }
        }
    }
}
