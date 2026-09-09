using System.Collections.Generic;

using NTSD.Animation;

namespace NTSD.Simulation.Ecs
{
    internal enum BattleType1ArmorMatchDecisionKind : byte
    {
        Unresolved = 0,
        Applies = 1,
        Bypassed = 2,
        CandidateRejected = 3,
    }

    internal enum BattleType1ArmorMatchReason : byte
    {
        None = 0,
        UnsupportedArmorType = 1,
        KindAbsent = 2,
        FrontOnlyBackHit = 3,
        BackOnlyFrontHit = 4,
        RecoveryDelayExceedsRatio = 5,
        BdefendThreshold = 6,
        FallThreshold = 7,
        InjuryThreshold = 8,
        EffectListed = 9,
        AttackerObjectIdListed = 10,
        SystemInvalidStatesUnavailable = 11,
        OutsideActiveSet = 12,
    }

    internal readonly struct BattleType1ArmorMatchResult
    {
        internal BattleType1ArmorMatchResult(
            BattleType1ArmorMatchDecisionKind decision,
            BattleType1ArmorMatchReason reason)
        {
            Decision = decision;
            Reason = reason;
        }

        public BattleType1ArmorMatchDecisionKind Decision { get; }
        public BattleType1ArmorMatchReason Reason { get; }
    }

    internal static class BattleType1ArmorMatchResolver
    {
        internal static BattleType1ArmorMatchResult Resolve(
            LF2ArmorData armor,
            InteractionArea interaction,
            int attackerObjectId,
            int attackerFacing,
            int defenderFacing,
            int defenderAction,
            int defenderState,
            int defenderArmorDelay,
            int effectiveInjury,
            bool systemRulesAvailable)
        {
            if (armor == null || armor.type != 1)
            {
                return Unresolved(
                    BattleType1ArmorMatchReason.UnsupportedArmorType);
            }

            if (interaction == null)
                interaction = EmptyInteraction.Instance;

            if (interaction.kind != 0 &&
                !Contains(armor.kinds, interaction.kind))
            {
                return new BattleType1ArmorMatchResult(
                    BattleType1ArmorMatchDecisionKind.CandidateRejected,
                    BattleType1ArmorMatchReason.KindAbsent);
            }

            if (armor.facing == 1 && attackerFacing == defenderFacing)
                return Bypassed(BattleType1ArmorMatchReason.FrontOnlyBackHit);
            if (armor.facing == 2 && attackerFacing != defenderFacing)
                return Bypassed(BattleType1ArmorMatchReason.BackOnlyFrontHit);
            if (armor.ratio < defenderArmorDelay)
            {
                return Bypassed(
                    BattleType1ArmorMatchReason.RecoveryDelayExceedsRatio);
            }
            if (interaction.bdefend == 100 &&
                (armor.bdefend == -1 ||
                 armor.bdefend <= interaction.bdefend))
            {
                return Bypassed(BattleType1ArmorMatchReason.BdefendThreshold);
            }
            if (armor.fall != -1 && armor.fall < interaction.fall)
                return Bypassed(BattleType1ArmorMatchReason.FallThreshold);
            if (armor.injury != -1 && armor.injury < effectiveInjury)
                return Bypassed(BattleType1ArmorMatchReason.InjuryThreshold);
            if (Contains(armor.effects, interaction.effect))
                return Bypassed(BattleType1ArmorMatchReason.EffectListed);
            if (Contains(armor.ids, attackerObjectId))
            {
                return Bypassed(
                    BattleType1ArmorMatchReason.AttackerObjectIdListed);
            }

            bool activeState = ActionInRanges(
                armor.frame_ranges,
                defenderAction);
            if (!activeState && armor.states != null && armor.states.Count > 0)
            {
                activeState = Contains(armor.states, defenderState);
            }
            else if (!activeState &&
                     (armor.states == null || armor.states.Count == 0))
            {
                if (!systemRulesAvailable)
                {
                    return Unresolved(
                        BattleType1ArmorMatchReason
                            .SystemInvalidStatesUnavailable);
                }
                activeState = !IsNative2833InvalidState(defenderState);
            }

            if (!activeState)
                return Bypassed(BattleType1ArmorMatchReason.OutsideActiveSet);
            return new BattleType1ArmorMatchResult(
                BattleType1ArmorMatchDecisionKind.Applies,
                BattleType1ArmorMatchReason.None);
        }

        private static bool Contains(List<int> values, int needle)
        {
            if (values == null)
                return false;
            for (int index = 0; index < values.Count; index++)
            {
                if (values[index] == needle)
                    return true;
            }
            return false;
        }

        private static bool ActionInRanges(
            List<LF2ArmorFrameRange> ranges,
            int action)
        {
            if (ranges == null)
                return false;
            for (int index = 0; index < ranges.Count; index++)
            {
                LF2ArmorFrameRange range = ranges[index];
                if (range.first <= action && action <= range.last)
                    return true;
            }
            return false;
        }

        private static bool IsNative2833InvalidState(int state)
        {
            switch (state)
            {
                case 8:
                case 11:
                case 12:
                case 13:
                case 14:
                case 16:
                case 18:
                    return true;
                default:
                    return false;
            }
        }

        private static BattleType1ArmorMatchResult Bypassed(
            BattleType1ArmorMatchReason reason)
        {
            return new BattleType1ArmorMatchResult(
                BattleType1ArmorMatchDecisionKind.Bypassed,
                reason);
        }

        private static BattleType1ArmorMatchResult Unresolved(
            BattleType1ArmorMatchReason reason)
        {
            return new BattleType1ArmorMatchResult(
                BattleType1ArmorMatchDecisionKind.Unresolved,
                reason);
        }

        private static class EmptyInteraction
        {
            internal static readonly InteractionArea Instance =
                new InteractionArea();
        }
    }
}
