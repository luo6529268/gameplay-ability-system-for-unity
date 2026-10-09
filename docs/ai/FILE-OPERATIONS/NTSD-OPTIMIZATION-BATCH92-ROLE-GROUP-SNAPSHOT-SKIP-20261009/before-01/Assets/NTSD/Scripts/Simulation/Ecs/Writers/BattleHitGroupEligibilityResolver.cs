using NTSD.Animation;

namespace NTSD.Simulation.Ecs
{
    internal enum BattleHitGroupEligibilityReason : byte
    {
        InvalidSnapshot = 0,
        OriginalKindBypass = 1,
        TargetCurrentStateBypass = 2,
        FreezeColumnOverride = 3,
        ZeroAttackerGroup = 4,
        OrdinaryDifferentGroup = 5,
        State190SameGroup = 6,
        ModeOrStateEffectException = 7,
        TargetObjectTypeException = 8,
        OpposingFacingType3Exception = 9,
        Rejected = 10,
    }

    internal readonly struct BattleHitGroupEligibilityResult
    {
        internal BattleHitGroupEligibilityResult(
            bool accepted,
            BattleHitGroupEligibilityReason reason)
        {
            Accepted = accepted;
            Reason = reason;
        }

        public bool Accepted { get; }
        public BattleHitGroupEligibilityReason Reason { get; }
    }

    internal static class BattleHitGroupEligibilityResolver
    {
        private const int FreezeColumnObjectId = 212;

        internal static BattleHitGroupEligibilityResult Resolve(
            int originalKind,
            int effect,
            in BattleHitCandidatePairSnapshot pair,
            int activeModeHitGroupGate18)
        {
            if (!pair.Valid)
            {
                return Rejected(
                    BattleHitGroupEligibilityReason.InvalidSnapshot);
            }

            if (originalKind == 4 || originalKind == 8 || originalKind == 50)
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.OriginalKindBypass);
            }

            if (pair.TargetCurrentState == 10 || pair.TargetCurrentState == 13)
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.TargetCurrentStateBypass);
            }

            if (HasFreezeColumnOverride(in pair))
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.FreezeColumnOverride);
            }

            if (pair.AttackerBattleGroup == 0)
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.ZeroAttackerGroup);
            }

            bool equalGroup =
                pair.AttackerBattleGroup == pair.TargetBattleGroup;
            if (pair.AttackerCurrentState == 190)
            {
                if (equalGroup)
                {
                    return Accepted(
                        BattleHitGroupEligibilityReason.State190SameGroup);
                }
            }
            else if (!equalGroup)
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.OrdinaryDifferentGroup);
            }

            bool modeException = activeModeHitGroupGate18 == 1 ||
                                 activeModeHitGroupGate18 == 3;
            if ((modeException ||
                 pair.AttackerCurrentState == 18 ||
                 pair.AttackerCurrentState == 180) &&
                effect != 21 &&
                effect != 22)
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.ModeOrStateEffectException);
            }

            if (pair.TargetObjectType == 1 ||
                pair.TargetObjectType == 2 ||
                pair.TargetObjectType == 4 ||
                pair.TargetObjectType == 6)
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.TargetObjectTypeException);
            }

            if (pair.AttackerObjectType == 0 &&
                pair.TargetObjectType == 3 &&
                pair.AttackerFacing != pair.TargetFacing)
            {
                return Accepted(
                    BattleHitGroupEligibilityReason.OpposingFacingType3Exception);
            }

            return Rejected(BattleHitGroupEligibilityReason.Rejected);
        }

        internal static bool AcceptsSubstitutedKind5Character(
            in BattleHitCandidatePairSnapshot pair)
        {
            if (!pair.Valid || !pair.LinkedHolderPresent)
                return false;

            return pair.LinkedHolderBattleGroup == 0 ||
                   pair.LinkedHolderBattleGroup != pair.TargetBattleGroup ||
                   HasFreezeColumnOverride(in pair);
        }

        private static bool HasFreezeColumnOverride(
            in BattleHitCandidatePairSnapshot pair)
        {
            return pair.TargetObjectId == FreezeColumnObjectId &&
                   (pair.AttackerObjectId != pair.TargetObjectId ||
                    (pair.TargetAction % 10 == 5 &&
                     pair.AttackerAction % 10 == 0));
        }

        private static BattleHitGroupEligibilityResult Accepted(
            BattleHitGroupEligibilityReason reason)
        {
            return new BattleHitGroupEligibilityResult(true, reason);
        }

        private static BattleHitGroupEligibilityResult Rejected(
            BattleHitGroupEligibilityReason reason)
        {
            return new BattleHitGroupEligibilityResult(false, reason);
        }
    }
}
