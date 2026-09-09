using System;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleReducedHitRestResult
    {
        internal BattleReducedHitRestResult(
            int attackerHold,
            int targetHold,
            bool clearTargetFrameCounter,
            int arest,
            bool shouldWriteVrest,
            int vrest)
        {
            AttackerHold = attackerHold;
            TargetHold = targetHold;
            ClearTargetFrameCounter = clearTargetFrameCounter;
            Arest = arest;
            ShouldWriteVrest = shouldWriteVrest;
            Vrest = vrest;
        }

        public int AttackerHold { get; }
        public int TargetHold { get; }
        public bool ClearTargetFrameCounter { get; }
        public int Arest { get; }
        public bool ShouldWriteVrest { get; }
        public int Vrest { get; }
    }

    internal static class BattleReducedHitRestResolver
    {
        internal static BattleReducedHitRestResult Resolve(
            int currentAttackerHold,
            int currentTargetHold,
            int attackerDefinitionEffect,
            int targetDefinitionEffect,
            bool hasSelectedArmor,
            int selectedArmorDelay,
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
            int targetHold = currentTargetHold;
            bool clearTargetFrameCounter = false;
            if (!hasSelectedArmor || selectedArmorDelay == -1)
            {
                if (targetDefinitionEffect != 2 &&
                    targetDefinitionEffect != 4)
                {
                    targetHold = Math.Min(0, -5 + reduction);
                }

                if (attackerDefinitionEffect != 2 &&
                    attackerDefinitionEffect != 3)
                {
                    attackerHold = Math.Max(0, 3 - reduction);
                }
            }
            else
            {
                int packedDelay = selectedArmorDelay;
                targetHold -= packedDelay % 100;
                packedDelay /= 100;
                attackerHold += packedDelay % 100;
                packedDelay /= 100;
                clearTargetFrameCounter = packedDelay % 100 == 0;
            }

            int resolvedArest = arest < 4 && vrest == 0
                ? 4
                : Math.Min(arest, 12);
            bool shouldWriteVrest = vrest > 0;
            int resolvedVrest = 0;
            if (shouldWriteVrest)
            {
                int nativeByte = unchecked((byte)vrest);
                resolvedVrest = nativeByte <= 4
                    ? 4
                    : Math.Min(nativeByte, 12);
            }

            return new BattleReducedHitRestResult(
                attackerHold,
                targetHold,
                clearTargetFrameCounter,
                resolvedArest,
                shouldWriteVrest,
                resolvedVrest);
        }
    }
}
