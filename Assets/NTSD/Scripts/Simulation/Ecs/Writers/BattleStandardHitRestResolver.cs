using System;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleStandardHitRestResult
    {
        internal BattleStandardHitRestResult(
            int attackerHold,
            int targetHold,
            int arest,
            bool shouldWriteVrest,
            int vrest)
        {
            AttackerHold = attackerHold;
            TargetHold = targetHold;
            Arest = arest;
            ShouldWriteVrest = shouldWriteVrest;
            Vrest = vrest;
        }

        public int AttackerHold { get; }
        public int TargetHold { get; }
        public int Arest { get; }
        public bool ShouldWriteVrest { get; }
        public int Vrest { get; }
    }

    internal static class BattleStandardHitRestResolver
    {
        internal static BattleStandardHitRestResult Resolve(
            int currentAttackerHold,
            int currentTargetHold,
            int recover,
            int attackerDefinitionEffect,
            int targetDefinitionEffect,
            int arest,
            int vrest,
            int timingReduction)
        {
            int reduction = Math.Max(
                NTSD28StandardHitRestRuntimeState
                    .DefaultTimingReduction4A9FF4,
                Math.Min(
                    NTSD28StandardHitRestRuntimeState
                        .MaximumTimingReduction4A9FF4,
                    timingReduction));
            int attackerHold = currentAttackerHold;
            if (currentAttackerHold >= 0 && recover != 1 && recover != 3 &&
                attackerDefinitionEffect != 2 &&
                attackerDefinitionEffect != 3)
            {
                attackerHold = Math.Max(0, 3 - reduction);
            }

            int targetHold = currentTargetHold;
            if (recover != 2 && recover != 3 &&
                targetDefinitionEffect != 2 &&
                targetDefinitionEffect != 4)
            {
                targetHold = Math.Min(0, -3 + reduction);
            }

            int resolvedArest;
            if (arest < 4 && vrest == 0)
            {
                resolvedArest = 4;
            }
            else
            {
                resolvedArest = arest > 1
                    ? Math.Max(1, arest - reduction)
                    : arest;
            }

            bool shouldWriteVrest = vrest > 0;
            int resolvedVrest = 0;
            if (shouldWriteVrest)
            {
                int nativeByte = unchecked((byte)vrest);
                resolvedVrest = nativeByte > 1
                    ? Math.Max(1, nativeByte - reduction)
                    : nativeByte;
            }

            return new BattleStandardHitRestResult(
                attackerHold,
                targetHold,
                resolvedArest,
                shouldWriteVrest,
                resolvedVrest);
        }
    }
}
