using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleEcsCharacterRecoveryPassMode : byte
    {
        Legacy = 0,
        DataOriented = 1,
    }

    internal enum BattleEcsCharacterRecoveryResult : byte
    {
        CompatibilityFallback = 0,
        ProvenNoOp = 1,
        Executed = 2,
    }

    public readonly struct BattleEcsCharacterRecoveryPassDiagnostics
    {
        internal BattleEcsCharacterRecoveryPassDiagnostics(
            BattleEcsCharacterRecoveryPassMode mode,
            long runCount,
            long exactCharacterCount,
            long provenNoOpCount,
            long compatibilityFallbackCount)
        {
            Mode = mode;
            RunCount = runCount;
            ExactCharacterCount = exactCharacterCount;
            ProvenNoOpCount = provenNoOpCount;
            CompatibilityFallbackCount = compatibilityFallbackCount;
        }

        public BattleEcsCharacterRecoveryPassMode Mode { get; }
        public long RunCount { get; }
        public long ExactCharacterCount { get; }
        public long ProvenNoOpCount { get; }
        public long CompatibilityFallbackCount { get; }
    }

    /// <summary>
    /// Owns the authority character regeneration segment that runs immediately
    /// before the late frame tick. Derived compatibility shells retain the
    /// virtual entity implementation.
    /// </summary>
    internal sealed class BattleEcsCharacterRecoveryPass
    {
        private readonly SimulationWorld world;
        private BattleEcsCharacterRecoveryPassMode mode =
            BattleEcsCharacterRecoveryPassMode.DataOriented;
        private long runCount;
        private long exactCharacterCount;
        private long provenNoOpCount;
        private long compatibilityFallbackCount;

        internal BattleEcsCharacterRecoveryPass(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal BattleEcsCharacterRecoveryPassMode Mode => mode;

        internal BattleEcsCharacterRecoveryPassDiagnostics Diagnostics =>
            new BattleEcsCharacterRecoveryPassDiagnostics(
                mode,
                runCount,
                exactCharacterCount,
                provenNoOpCount,
                compatibilityFallbackCount);

        internal void SetMode(BattleEcsCharacterRecoveryPassMode requestedMode)
        {
            mode = requestedMode;
            ResetDiagnostics();
        }

        internal void Reset()
        {
            ResetDiagnostics();
        }

        internal BattleEcsCharacterRecoveryResult Execute(
            LF2Entity entity,
            int tickIndex)
        {
            runCount++;
            if (mode == BattleEcsCharacterRecoveryPassMode.Legacy ||
                entity == null ||
                entity.GetType() != typeof(LF2Character) ||
                entity.Health == null ||
                entity.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                compatibilityFallbackCount++;
                return BattleEcsCharacterRecoveryResult.CompatibilityFallback;
            }

            exactCharacterCount++;
            bool periodHp =
                tickIndex % NTSDGlobal.Gameplay.HpRecoverPeriod == 0;
            bool periodPp =
                tickIndex % NTSDGlobal.Gameplay.PpRecoverPeriod == 0;
            if (!periodHp && !periodPp)
            {
                provenNoOpCount++;
                return BattleEcsCharacterRecoveryResult.ProvenNoOp;
            }

            ApplyAuthorityRecovery(entity, periodHp, periodPp);
            return BattleEcsCharacterRecoveryResult.Executed;
        }

        private void ApplyAuthorityRecovery(
            LF2Entity entity,
            bool periodHp,
            bool periodPp)
        {
            BattleFlowRuntimeState flow = world.Runtime?.Flow;
            bool stepWaitGate =
                flow != null &&
                flow.BattleStepMode == 1 &&
                flow.BattleStepGate != 1;

            if (entity.Health.HP > 0 &&
                entity.Health.HP < entity.Health.HPBound &&
                periodHp &&
                !stepWaitGate)
            {
                entity.Health.HP++;
            }

            if (entity.WeaponCount < 0 && periodHp && !stepWaitGate)
            {
                int injury = NTSDGlobal.Gameplay.NegativeWeaponCountInjury;
                if (entity.FallDamageDiv > 0)
                {
                    injury = NTSDGlobal.Gameplay.NegativeWeaponCountScaledInjury /
                             entity.FallDamageDiv;
                }

                entity.Health.HP -= injury;
                entity.Health.HPBound -=
                    injury / NTSDGlobal.Gameplay.NegativeWeaponCountHpBoundDivisor;
                if (entity.Health.HP < 0)
                    entity.Health.HP = 0;
                if (entity.Health.HPBound < 0)
                    entity.Health.HPBound = 0;
                entity.ComboCountVic += 9;
            }

            if (!periodPp ||
                (entity.KillCount != -1 &&
                 entity.Health.PP >= NTSDGlobal.Gameplay.PpRecoverLowLimit) ||
                entity.Health.PP >= NTSDGlobal.Gameplay.PpRecoverCap ||
                entity.HitStun < 0 ||
                stepWaitGate)
            {
                return;
            }

            int hpForRate = Math.Min(
                entity.Health.HP,
                NTSDGlobal.Gameplay.PpRecoverCap);
            if (entity.ObjectId == 51 || entity.ObjectId == 52)
                hpForRate /= 2;

            entity.Health.PP +=
                ((NTSDGlobal.Gameplay.PpRecoverCap - hpForRate) /
                 NTSDGlobal.Gameplay.PpRecoverHpRateDivisor) + 1;
        }

        private void ResetDiagnostics()
        {
            runCount = 0;
            exactCharacterCount = 0;
            provenNoOpCount = 0;
            compatibilityFallbackCount = 0;
        }
    }
}
