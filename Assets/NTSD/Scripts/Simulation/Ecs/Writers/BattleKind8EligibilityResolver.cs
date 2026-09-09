namespace NTSD.Simulation.Ecs
{
    internal enum BattleKind8EligibilityKind : byte
    {
        Accepted = 0,
        TargetTypeRejected = 1,
        RelationRejected = 2,
    }

    internal readonly struct BattleKind8EligibilityResult
    {
        internal BattleKind8EligibilityResult(BattleKind8EligibilityKind kind)
        {
            Kind = kind;
        }

        public BattleKind8EligibilityKind Kind { get; }
        public bool Accepted => Kind == BattleKind8EligibilityKind.Accepted;
    }

    internal static class BattleKind8EligibilityResolver
    {
        internal static BattleKind8EligibilityResult Resolve(
            int targetTypeSelector,
            int relationSelector,
            int targetObjectType,
            int attackerBattleGroup,
            int targetBattleGroup,
            int attackerOwnerSlot,
            int targetOwnerSlot,
            int battleModeContext)
        {
            bool typeMatches;
            if (targetTypeSelector < 7)
            {
                typeMatches = targetObjectType == targetTypeSelector;
            }
            else if (targetTypeSelector == 7)
            {
                typeMatches = targetObjectType == 1 ||
                              targetObjectType == 2 ||
                              targetObjectType == 4 ||
                              targetObjectType == 6;
            }
            else
            {
                typeMatches = targetTypeSelector == 8;
            }

            if (!typeMatches)
            {
                return new BattleKind8EligibilityResult(
                    BattleKind8EligibilityKind.TargetTypeRejected);
            }

            if (relationSelector < 0 || relationSelector > 4)
            {
                return new BattleKind8EligibilityResult(
                    BattleKind8EligibilityKind.RelationRejected);
            }
            if (relationSelector == 0)
            {
                return new BattleKind8EligibilityResult(
                    BattleKind8EligibilityKind.Accepted);
            }

            bool sameGroup = attackerBattleGroup == targetBattleGroup;
            bool relationMatches = relationSelector == 2
                ? !sameGroup
                : sameGroup;
            if (relationMatches && relationSelector >= 3)
                relationMatches = attackerOwnerSlot == targetOwnerSlot;
            if (relationMatches && relationSelector == 4)
                relationMatches = attackerOwnerSlot == battleModeContext;

            return new BattleKind8EligibilityResult(
                relationMatches
                    ? BattleKind8EligibilityKind.Accepted
                    : BattleKind8EligibilityKind.RelationRejected);
        }
    }
}
