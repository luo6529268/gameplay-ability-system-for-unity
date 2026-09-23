using System;

using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleNegativeEnvironmentRecoveryWriter
    {
        internal static bool IsEligible(
            SimulationWorld world,
            LF2Entity victim)
        {
            return world != null &&
                   victim?.Runtime != null &&
                   victim.Health != null &&
                   ReferenceEquals(victim.RegisteredWorldForSimulation, world) &&
                   victim.GetCurrentDataObjectTypeForSimulation() ==
                       (int)LF2ObjectType.Character &&
                   victim.Runtime.EnvironmentState320 < 0 &&
                   world.NativeResourcePhase12 == 0;
        }

        internal static bool Apply(
            SimulationWorld world,
            LF2Entity victim)
        {
            if (!IsEligible(world, victim))
                return false;

            NTSDEntityRuntime runtime = victim.Runtime;
            int rawRule = world.Runtime?.NativeHitResourceRules?
                .NegativeEnvironmentDamage90 ??
                NTSD28HitResourceRulesRuntimeState
                    .DefaultNegativeEnvironmentDamage90;
            int ruleDamage = rawRule > 0
                ? rawRule
                : NTSD28HitResourceRulesRuntimeState
                    .DefaultNegativeEnvironmentDamage90;
            int actualDamage = runtime.IncomingDamageScale340 > 0
                ? 900 / runtime.IncomingDamageScale340
                : ruleDamage;
            NTSDEntityRuntime credit = ResolveCreditRuntime(
                world,
                runtime.CatchSourceSlot90);

            if (runtime.HP > 0 &&
                (long)runtime.HP - actualDamage <= 0L &&
                credit != null)
            {
                credit.KnockoutCount358 = unchecked(
                    credit.KnockoutCount358 + 1);
                world.RecordNativeKnockout(
                    runtime.SlotIndex,
                    runtime.ImpactSourceSlot164 >= 0
                        ? runtime.ImpactSourceSlot164
                        : 1000,
                    credit.SlotIndex);
            }

            runtime.HP = unchecked(runtime.HP - actualDamage);
            runtime.HPBound = unchecked(
                runtime.HPBound - (actualDamage / 3));
            runtime.InputHpConsumedTotal34C = unchecked(
                runtime.InputHpConsumedTotal34C + ruleDamage);
            if (credit != null)
            {
                credit.InputScoreTotal348 = unchecked(
                    credit.InputScoreTotal348 + actualDamage);
            }
            runtime.HP = Math.Max(runtime.HP, 0);
            runtime.HPBound = Math.Max(runtime.HPBound, 0);
            return true;
        }

        private static NTSDEntityRuntime ResolveCreditRuntime(
            SimulationWorld world,
            int encodedRootSlot)
        {
            if (encodedRootSlot < 0)
                return null;

            int rootSlot = encodedRootSlot >= 0x2000
                ? encodedRootSlot - 0x2000
                : encodedRootSlot;
            NTSDEntityRuntime credit = ResolveRuntime(world, rootSlot);
            for (int depth = 0; credit != null && depth < 2; depth++)
            {
                if (credit.OwnerSlotIndex < 0)
                    break;

                NTSDEntityRuntime next = ResolveRuntime(
                    world,
                    credit.OwnerSlotIndex);
                if (next == null)
                    break;
                credit = next;
            }
            return credit;
        }

        private static NTSDEntityRuntime ResolveRuntime(
            SimulationWorld world,
            int slot)
        {
            if (world == null ||
                !world.TryGetRuntimeSlotReadOnlyView(
                    slot,
                    out RuntimeSlotTable.ReadOnlySlotView view) ||
                !view.Claimed)
            {
                return null;
            }

            return view.Entity?.Runtime ?? view.RawRuntime;
        }
    }
}
