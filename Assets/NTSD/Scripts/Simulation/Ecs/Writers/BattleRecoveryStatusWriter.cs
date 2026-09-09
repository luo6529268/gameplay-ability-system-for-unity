using System;

using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleRecoveryStatusWriter
    {
        internal static void ApplyHpRecovery(
            LF2Entity entity,
            bool periodHp,
            bool stepWaitGate)
        {
            if (!periodHp || stepWaitGate ||
                entity.Health.HP <= 0 ||
                entity.Health.HP >= entity.Health.HPBound)
            {
                return;
            }

            // Alignment contract: NTSD28-B5-RECOVERY-STATUS-CONSUMERS-NO-STATS-001.
            // C25c consumes these timers before their C25h decrement.
            if (entity.Runtime.WeakTimer12C > 0)
            {
                if (entity.Health.PP < entity.Health.HP3)
                    entity.Health.PP++;
                return;
            }

            entity.Health.HP += entity.Runtime.HpRegenDouble1AC > 0 ? 2 : 1;
        }

        internal static void ApplyMpRecovery(LF2Entity entity)
        {
            if (entity.Runtime.WeakTimer12C > 0)
                return;

            int hpForRate = Math.Min(
                entity.Health.HP,
                NTSDGlobal.Gameplay.PpRecoverCap);
            if (entity.ObjectId == 51 || entity.ObjectId == 52)
                hpForRate /= 2;

            int delta =
                ((NTSDGlobal.Gameplay.PpRecoverCap - hpForRate) /
                 NTSDGlobal.Gameplay.PpRecoverHpRateDivisor) + 1;
            if (entity.Runtime.MpRegenBonusTimer1A4 > 0)
                delta++;
            entity.Health.PP += delta;
        }
    }
}
