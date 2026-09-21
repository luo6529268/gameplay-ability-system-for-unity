namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeDisplayWriter
    {
        internal static void InitializeBirth(NTSDEntityRuntime runtime, int hp)
        {
            if (runtime == null)
                return;

            // Alignment contract: NTSD28-Q06-ORDINARY-STAGE-DISPLAY-BIRTH-001.
            runtime.DisplayScore1F0 = 0;
            runtime.DisplayScoreStep1F4 = 0;
            runtime.DisplayDamageTotal1F8 = 0;
            runtime.DisplayDamageStep1FC = 0;
            runtime.DisplayCurrentHp200 = hp;
            runtime.DisplayCurrentHpStep204 = 0;
            runtime.DisplayEffectiveMaxHp208 = hp;
            runtime.DisplayEffectiveMaxHpStep20C = 0;
        }

        internal static void Advance(NTSDEntityRuntime runtime)
        {
            if (runtime == null)
                return;

            // Alignment contract: NTSD28-Q06-NATIVE-DISPLAY-PROGRESSION-001.
            runtime.DisplayScore1F0 = runtime.DisplayScore1F0 < runtime.InputScoreTotal348
                ? unchecked(runtime.DisplayScore1F0 + runtime.DisplayScoreStep1F4)
                : runtime.InputScoreTotal348;
            runtime.DisplayDamageTotal1F8 = runtime.DisplayDamageTotal1F8 < runtime.InputHpConsumedTotal34C
                ? unchecked(runtime.DisplayDamageTotal1F8 + runtime.DisplayDamageStep1FC)
                : runtime.InputHpConsumedTotal34C;
            runtime.DisplayCurrentHp200 = runtime.DisplayCurrentHp200 > runtime.HP
                ? unchecked(runtime.DisplayCurrentHp200 - runtime.DisplayCurrentHpStep204)
                : runtime.HP;
            runtime.DisplayEffectiveMaxHp208 = runtime.DisplayEffectiveMaxHp208 > runtime.HPBound
                ? unchecked(runtime.DisplayEffectiveMaxHp208 - runtime.DisplayEffectiveMaxHpStep20C)
                : runtime.HPBound;
        }
    }
}
