using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    internal static class BattleNativeComboExpiryModule
    {
        internal static int Expire(SimulationWorld world)
        {
            NTSD28NativeComboRuntimeState combo = world?.Runtime?.NativeCombo;
            if (combo == null || !combo.RecordPresent || combo.Respond < 0)
                return 0;

            ulong battleTick = world.NativeFrameSequence;
            ulong respond = (ulong)combo.Respond;
            int expired = 0;
            for (int slot = 0; slot < world.MaxRuntimeSlotsForServices; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotForQuery(slot);
                if (entity?.Runtime == null ||
                    entity.Runtime.NativeComboHitCount1E0 <= 0)
                {
                    continue;
                }

                ulong lastTick = entity.Runtime.NativeComboHitLastTick1E4;
                if (lastTick > battleTick || battleTick - lastTick < respond)
                    continue;

                entity.Runtime.NativeComboHitCount1E0 = 0;
                expired++;
            }

            return expired;
        }
    }
}
