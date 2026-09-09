namespace NTSD.Simulation.Ecs
{
    internal enum BattleOrdinaryDefenseDecisionKind : byte
    {
        Inactive = 0,
        Applies = 1,
        Bypassed = 2,
    }

    internal readonly struct BattleOrdinaryDefenseResult
    {
        internal BattleOrdinaryDefenseResult(
            BattleOrdinaryDefenseDecisionKind decision,
            bool usedTwoWayDefenseObjectId)
        {
            Decision = decision;
            UsedTwoWayDefenseObjectId = usedTwoWayDefenseObjectId;
        }

        public BattleOrdinaryDefenseDecisionKind Decision { get; }
        public bool UsedTwoWayDefenseObjectId { get; }
    }

    internal static class BattleOrdinaryDefenseResolver
    {
        internal const int NativeTwoWayDefenseObjectId = 822;

        internal static BattleOrdinaryDefenseResult Resolve(
            int interactionKind,
            int interactionEffect,
            int interactionSpark,
            int interactionDbdefend,
            int interactionDvx,
            int attackerFacing,
            int defenderFacing,
            int defenderState,
            int defenderHp,
            int attackerObjectId)
        {
            if (interactionKind != 0 || interactionEffect >= 61 ||
                (defenderState != 7 &&
                 defenderState != 70 &&
                 defenderState != 75) ||
                defenderHp <= 0)
            {
                return new BattleOrdinaryDefenseResult(
                    BattleOrdinaryDefenseDecisionKind.Inactive,
                    false);
            }

            if (defenderState != 7 ||
                attackerFacing != defenderFacing ||
                (interactionSpark & 1) != 0 ||
                interactionDbdefend == 1 ||
                interactionDvx < 0)
            {
                return new BattleOrdinaryDefenseResult(
                    BattleOrdinaryDefenseDecisionKind.Applies,
                    false);
            }

            if (attackerObjectId == NativeTwoWayDefenseObjectId)
            {
                return new BattleOrdinaryDefenseResult(
                    BattleOrdinaryDefenseDecisionKind.Applies,
                    true);
            }

            return new BattleOrdinaryDefenseResult(
                BattleOrdinaryDefenseDecisionKind.Bypassed,
                false);
        }
    }
}
