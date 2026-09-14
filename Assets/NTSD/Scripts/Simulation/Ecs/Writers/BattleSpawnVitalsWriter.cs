using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleSpawnVitalsWriter
    {
        internal static void Apply(LF2Entity entity, ObjectPoint point)
        {
            if (entity?.Health == null || entity.Runtime == null)
                return;

            // Alignment contract: NTSD28-Q06-OPOINT-SPAWN-VITALS-TRANSACTION-001.
            bool lowVitals = point.oid == 5 || point.oid == 52;
            int hp = point.hp > 0 ? point.hp : lowVitals ? 10 : 500;
            int mp = point.mp > 0 ? point.mp : lowVitals ? 5 : 500;
            var data = entity.FrameCache?.Wrapper?.characterData;
            var stats = data?.NativeMetadata?.Stats;
            int hpPercent = stats?.Int32OrDefault("ohp", 0) ?? 0;
            int mpPercent = stats?.Int32OrDefault("omp", 0) ?? 0;
            if (hpPercent > 0)
                hp = unchecked((int)((long)hp * hpPercent / 100));
            if (mpPercent > 0)
                mp = unchecked((int)((long)mp * mpPercent / 100));

            entity.Health.HP = hp;
            entity.Health.HPBound = hp;
            entity.Health.HP3 = hp;
            entity.Health.PP = mp;
            entity.Health.MaxMP = stats?.Int32OrDefault("max_mp", mp) ?? mp;

            NTSDEntityRuntime runtime = entity.Runtime;
            // Alignment contract: NTSD28-Q06-OPOINT-WEAPON-HP-BIRTH-001.
            runtime.WeaponFlightCounter = data?.NativeMetadata?.Bmp.Int32OrDefault("weapon_hp", 0) ?? data?.weapon_hp ?? 0;
            // Alignment contract: NTSD28-Q06-LATE-OPOINT-DEPTH-AND-LIVES-001.
            runtime.HP2Orig = 1;
            runtime.HPOrig = 0;
            runtime.RespawnCount = 0;
            runtime.DisplayCurrentHp200 = hp;
            runtime.DisplayEffectiveMaxHp208 = hp;
            runtime.DisplayScore1F0 = 0;
            runtime.DisplayDamageTotal1F8 = 0;
            runtime.DisplayScoreStep1F4 = 0;
            runtime.DisplayDamageStep1FC = 0;
            runtime.DisplayCurrentHpStep204 = 0;
            runtime.DisplayEffectiveMaxHpStep20C = 0;
        }
    }
}
