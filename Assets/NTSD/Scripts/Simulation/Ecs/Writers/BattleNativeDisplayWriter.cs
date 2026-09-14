namespace NTSD.Simulation.Ecs
{
    internal static class BattleNativeDisplayWriter
    {
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
