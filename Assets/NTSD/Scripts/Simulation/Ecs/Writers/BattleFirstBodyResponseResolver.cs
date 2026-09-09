namespace NTSD.Simulation.Ecs
{
    public enum BattleFirstBodyResponseKind : byte
    {
        None = 0,
        ActionRange = 1,
        EncodedNeedsRoll = 2,
        EncodedChanceRejected = 3,
        EncodedApplied = 4,
    }

    internal readonly struct BattleFirstBodyResponseResult
    {
        internal BattleFirstBodyResponseResult(
            BattleFirstBodyResponseKind kind,
            int firstBodyKind,
            int firstBodyRespond,
            int chance,
            int roll,
            bool usedProvidedRoll,
            int targetAction,
            bool writeTargetAction,
            bool resetTargetFrameCounter,
            int attackerAction,
            bool writeAttackerAction,
            bool resetAttackerFrameCounter,
            int effect,
            int targetGroup,
            bool writeTargetGroup,
            bool applyHold,
            int manualDamage,
            bool applyManualDamage)
        {
            Kind = kind;
            FirstBodyKind = firstBodyKind;
            FirstBodyRespond = firstBodyRespond;
            Chance = chance;
            Roll = roll;
            UsedProvidedRoll = usedProvidedRoll;
            TargetAction = targetAction;
            WriteTargetAction = writeTargetAction;
            ResetTargetFrameCounter = resetTargetFrameCounter;
            AttackerAction = attackerAction;
            WriteAttackerAction = writeAttackerAction;
            ResetAttackerFrameCounter = resetAttackerFrameCounter;
            Effect = effect;
            TargetGroup = targetGroup;
            WriteTargetGroup = writeTargetGroup;
            ApplyHold = applyHold;
            ManualDamage = manualDamage;
            ApplyManualDamage = applyManualDamage;
        }

        public BattleFirstBodyResponseKind Kind { get; }
        public bool Recognized => Kind != BattleFirstBodyResponseKind.None;
        public bool Applied =>
            Kind == BattleFirstBodyResponseKind.ActionRange ||
            Kind == BattleFirstBodyResponseKind.EncodedApplied;
        public bool NeedsRoll =>
            Kind == BattleFirstBodyResponseKind.EncodedNeedsRoll;
        public int FirstBodyKind { get; }
        public int FirstBodyRespond { get; }
        public int Chance { get; }
        public int Roll { get; }
        public bool UsedProvidedRoll { get; }
        public int TargetAction { get; }
        public bool WriteTargetAction { get; }
        public bool ResetTargetFrameCounter { get; }
        public int AttackerAction { get; }
        public bool WriteAttackerAction { get; }
        public bool ResetAttackerFrameCounter { get; }
        public int Effect { get; }
        public int TargetGroup { get; }
        public bool WriteTargetGroup { get; }
        public bool ApplyHold { get; }
        public int ManualDamage { get; }
        public bool ApplyManualDamage { get; }
    }

    internal static class BattleFirstBodyResponseResolver
    {
        private const int ActionRangeLower = 1000;
        private const int ActionRangeSplit = 1999;
        private const int ActionRangeUpperExclusive = 2999;
        private const int EncodedBase = 1000000000;
        private const int EncodedUpperExclusive = 1999999999;

        internal static BattleFirstBodyResponseResult Resolve(
            int firstBodyKind,
            int firstBodyRespond,
            int rawInjury,
            int attackerGroup,
            bool hasRoll,
            int roll)
        {
            if (firstBodyKind >= ActionRangeLower &&
                firstBodyKind < ActionRangeUpperExclusive)
            {
                int rangeTargetAction = firstBodyKind < ActionRangeSplit
                    ? firstBodyKind - 1000
                    : firstBodyKind - 2000;
                int targetGroup = firstBodyRespond == -1
                    ? attackerGroup
                    : firstBodyRespond == 0
                        ? 1
                        : firstBodyRespond;
                return new BattleFirstBodyResponseResult(
                    BattleFirstBodyResponseKind.ActionRange,
                    firstBodyKind,
                    firstBodyRespond,
                    0,
                    0,
                    false,
                    rangeTargetAction,
                    true,
                    false,
                    0,
                    false,
                    false,
                    0,
                    targetGroup,
                    true,
                    firstBodyKind < ActionRangeSplit,
                    0,
                    false);
            }

            if (firstBodyKind < EncodedBase ||
                firstBodyKind >= EncodedUpperExclusive)
            {
                return Empty(firstBodyKind, firstBodyRespond);
            }

            int payload = firstBodyKind - EncodedBase;
            int chance = payload / 10000000;
            payload %= 10000000;
            int targetAction = payload / 10000;
            payload %= 10000;
            int attackerAction = payload / 10;
            int effect = payload % 10;

            bool chanceUsesRoll = chance > 0 && chance < 100;
            if (chanceUsesRoll && !hasRoll)
            {
                return EncodedWithoutWrites(
                    BattleFirstBodyResponseKind.EncodedNeedsRoll,
                    firstBodyKind,
                    firstBodyRespond,
                    chance,
                    0,
                    false,
                    targetAction,
                    attackerAction,
                    effect);
            }

            if (chanceUsesRoll && roll >= chance)
            {
                return EncodedWithoutWrites(
                    BattleFirstBodyResponseKind.EncodedChanceRejected,
                    firstBodyKind,
                    firstBodyRespond,
                    chance,
                    roll,
                    true,
                    targetAction,
                    attackerAction,
                    effect);
            }

            bool writeTargetAction = targetAction < 999;
            bool writeAttackerAction = attackerAction < 999;
            bool writeTargetGroup =
                effect == 1 || effect == 3 || effect == 5 || effect == 7;
            bool applyHold = effect == 4 || effect == 5 ||
                             effect == 6 || effect == 7;
            bool applyManualDamage = effect == 2 || effect == 6;

            return new BattleFirstBodyResponseResult(
                BattleFirstBodyResponseKind.EncodedApplied,
                firstBodyKind,
                firstBodyRespond,
                chance,
                chanceUsesRoll ? roll : 0,
                chanceUsesRoll,
                targetAction,
                writeTargetAction,
                writeTargetAction,
                attackerAction,
                writeAttackerAction,
                writeAttackerAction,
                effect,
                writeTargetGroup ? attackerGroup : 0,
                writeTargetGroup,
                applyHold,
                applyManualDamage ? rawInjury : 0,
                applyManualDamage);
        }

        private static BattleFirstBodyResponseResult Empty(
            int firstBodyKind,
            int firstBodyRespond)
        {
            return new BattleFirstBodyResponseResult(
                BattleFirstBodyResponseKind.None,
                firstBodyKind,
                firstBodyRespond,
                0,
                0,
                false,
                0,
                false,
                false,
                0,
                false,
                false,
                0,
                0,
                false,
                false,
                0,
                false);
        }

        private static BattleFirstBodyResponseResult EncodedWithoutWrites(
            BattleFirstBodyResponseKind kind,
            int firstBodyKind,
            int firstBodyRespond,
            int chance,
            int roll,
            bool usedProvidedRoll,
            int targetAction,
            int attackerAction,
            int effect)
        {
            return new BattleFirstBodyResponseResult(
                kind,
                firstBodyKind,
                firstBodyRespond,
                chance,
                roll,
                usedProvidedRoll,
                targetAction,
                false,
                false,
                attackerAction,
                false,
                false,
                effect,
                0,
                false,
                false,
                0,
                false);
        }
    }
}
