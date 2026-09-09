namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleReducedHitDamageResult
    {
        internal BattleReducedHitDamageResult(
            bool supported,
            int hpDamage,
            int mpDamage,
            int runtimeArmorHpDelta)
        {
            Supported = supported;
            HpDamage = hpDamage;
            MpDamage = mpDamage;
            RuntimeArmorHpDelta = runtimeArmorHpDelta;
        }

        public bool Supported { get; }
        public int HpDamage { get; }
        public int MpDamage { get; }
        public int RuntimeArmorHpDelta { get; }
    }

    internal static class BattleReducedHitDamageResolver
    {
        internal static BattleReducedHitDamageResult Resolve(
            int baseInjury,
            bool hasSelectedArmor,
            int armorType,
            int armorDecrease,
            int armorMp,
            int armorHp,
            int targetDamageScale)
        {
            int hpDamage = 0;
            int mpDamage = 0;
            int runtimeArmorHpDelta = 0;
            if (!hasSelectedArmor || armorType == 4)
            {
                hpDamage = baseInjury / 10;
            }
            else
            {
                if (armorType == 3)
                {
                    return new BattleReducedHitDamageResult(
                        false,
                        0,
                        0,
                        0);
                }

                if (armorHp > 0)
                {
                    runtimeArmorHpDelta = unchecked(-baseInjury);
                }

                int reduced = armorDecrease <= 0
                    ? unchecked((int)(-(long)armorDecrease))
                    : unchecked((int)(((long)baseInjury * armorDecrease) / 100));
                if (armorMp == 0)
                {
                    hpDamage = reduced;
                }
                else
                {
                    mpDamage = armorMp <= 0
                        ? unchecked((int)(-(long)armorMp))
                        : unchecked((int)(((long)armorMp * reduced) / 100));
                }
            }

            if (targetDamageScale > 0)
            {
                int scaledProduct = unchecked(
                    (int)((uint)hpDamage * 100u));
                hpDamage = scaledProduct / targetDamageScale;
            }

            return new BattleReducedHitDamageResult(
                true,
                hpDamage,
                mpDamage,
                runtimeArmorHpDelta);
        }
    }
}
