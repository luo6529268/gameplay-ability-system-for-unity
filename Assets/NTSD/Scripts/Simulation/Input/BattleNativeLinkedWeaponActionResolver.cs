using NTSD.Animation;

namespace NTSD.Simulation
{
    internal enum BattleNativeLinkedWeaponActionField : byte
    {
        NormalAttack1,
        NormalAttack2,
        LightThrow,
        WeaponDrink,
        HeavyThrow,
        RunHeavyThrow,
        RunAttack,
        JumpAttack,
        SkyLightThrow,
    }

    internal static class BattleNativeLinkedWeaponActionResolver
    {
        internal const int NormalAttack1Fallback = 20;
        internal const int NormalAttack2Fallback = 25;
        internal const int LightThrowFallback = 45;
        internal const int WeaponDrinkFallback = 55;
        internal const int HeavyThrowFallback = 50;
        internal const int RunHeavyThrowFallback = 50;
        internal const int RunAttackFallback = 35;
        internal const int DashJumpAttackFallback = 40;
        internal const int AirJumpAttackFallback = 30;
        internal const int SkyLightThrowFallback = 52;

        // Alignment contract: NTSD28-B6-NTSDSPEC-COMPAT-WEAPON-ACTION-PRODUCTION-001.
        internal static int Resolve(LF2CharacterData linkedData,
            BattleNativeLinkedWeaponActionField field, int fallback)
        {
            if (linkedData == null)
                return fallback;

            int action = field switch
            {
                BattleNativeLinkedWeaponActionField.NormalAttack1 => linkedData.normal_attack1,
                BattleNativeLinkedWeaponActionField.NormalAttack2 => linkedData.normal_attack2,
                BattleNativeLinkedWeaponActionField.LightThrow => linkedData.light_throw,
                BattleNativeLinkedWeaponActionField.WeaponDrink => linkedData.weapon_drink,
                BattleNativeLinkedWeaponActionField.HeavyThrow => linkedData.heavy_throw,
                BattleNativeLinkedWeaponActionField.RunHeavyThrow => linkedData.run_heavy_throw,
                BattleNativeLinkedWeaponActionField.RunAttack => linkedData.run_attack,
                BattleNativeLinkedWeaponActionField.JumpAttack => linkedData.jump_attack,
                BattleNativeLinkedWeaponActionField.SkyLightThrow => linkedData.sky_light_throw,
                _ => 0,
            };
            return action == 0 ? fallback : action;
        }
    }
}
