using NTSD.Animation;

namespace NTSD.Simulation.Ecs
{
    internal enum BattleType1ArmorActivationReason : byte
    {
        None = 0,
        InsufficientMp = 1,
        MissingRuntimeArmorHp = 2,
        ArmorHpExhausted = 3,
    }

    internal readonly struct BattleType1ArmorActivationResult
    {
        internal BattleType1ArmorActivationResult(
            bool available,
            int mpCost,
            bool armorHpBroken,
            bool hasNextArmorHp,
            int nextArmorHp,
            BattleType1ArmorActivationReason reason)
        {
            Available = available;
            MpCost = mpCost;
            ArmorHpBroken = armorHpBroken;
            HasNextArmorHp = hasNextArmorHp;
            NextArmorHp = nextArmorHp;
            Reason = reason;
        }

        public bool Available { get; }
        public int MpCost { get; }
        public bool ArmorHpBroken { get; }
        public bool HasNextArmorHp { get; }
        public int NextArmorHp { get; }
        public BattleType1ArmorActivationReason Reason { get; }
    }

    internal static class BattleType1ArmorActivationResolver
    {
        internal static BattleType1ArmorActivationResult Resolve(
            LF2ArmorData armor,
            int baseInjury,
            int effectiveInjury,
            int currentMp,
            bool hasRuntimeArmorHp,
            int runtimeArmorHp)
        {
            if (armor.mp != 0)
            {
                int reduced = AbsoluteOrScaled(baseInjury, armor.decrease);
                int mpCost = AbsoluteOrScaled(reduced, armor.mp);
                if (mpCost == 0)
                    mpCost = 1;
                if (currentMp < mpCost)
                {
                    return new BattleType1ArmorActivationResult(
                        false,
                        mpCost,
                        false,
                        false,
                        0,
                        BattleType1ArmorActivationReason.InsufficientMp);
                }

                return Available(mpCost);
            }

            if (armor.hp != 0)
            {
                if (!hasRuntimeArmorHp)
                {
                    return new BattleType1ArmorActivationResult(
                        false,
                        0,
                        false,
                        false,
                        0,
                        BattleType1ArmorActivationReason.MissingRuntimeArmorHp);
                }
                if (runtimeArmorHp <= effectiveInjury)
                {
                    return new BattleType1ArmorActivationResult(
                        false,
                        0,
                        true,
                        true,
                        -1,
                        BattleType1ArmorActivationReason.ArmorHpExhausted);
                }
            }

            return Available(0);
        }

        private static int AbsoluteOrScaled(int baseValue, int field)
        {
            if (field <= 0)
                return unchecked((int)(-(long)field));
            return unchecked((int)(((long)baseValue * field) / 100));
        }

        private static BattleType1ArmorActivationResult Available(int mpCost)
        {
            return new BattleType1ArmorActivationResult(
                true,
                mpCost,
                false,
                false,
                0,
                BattleType1ArmorActivationReason.None);
        }
    }
}
