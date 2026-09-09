using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeComboOrdinaryProducer
    {
        internal static bool TryApply(
            SimulationWorld world,
            int attackerSlot,
            int targetSlot)
        {
            NTSD28NativeComboRuntimeState combo = world?.Runtime?.NativeCombo;
            if (combo == null || !combo.RecordPresent || combo.Bound != 1)
                return false;

            LF2Entity target = world.FindEntityByRuntimeSlotForQuery(targetSlot);
            if (target?.Runtime == null ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                return false;
            }

            int sourceSlot = combo.Facing == 1 ? attackerSlot : targetSlot;
            LF2Entity source = world.FindEntityByRuntimeSlotForQuery(sourceSlot);
            if (source?.Runtime == null)
                return false;

            LF2Entity credited = source;
            if (source.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                credited = world.FindEntityByRuntimeSlotForQuery(
                    source.Runtime.OwnerSlotIndex);
            }

            if (credited?.Runtime == null)
                return false;

            credited.Runtime.NativeComboHitCount1E0++;
            // Alignment contract:
            // NTSD28-B5-NATIVE-COMBO-ORDINARY-PRODUCER-001.
            // C24 commits FrameSequence later in this same process tick.
            credited.Runtime.NativeComboHitLastTick1E4 =
                world.NativeFrameSequence + 1UL;
            return true;
        }
    }
}
