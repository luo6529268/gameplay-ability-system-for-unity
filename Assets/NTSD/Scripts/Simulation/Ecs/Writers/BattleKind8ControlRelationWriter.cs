using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    internal static class BattleKind8ControlRelationWriter
    {
        internal static bool TryApply(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            if (world == null ||
                attacker?.Runtime == null ||
                attacker.Frame == null ||
                target?.Runtime == null ||
                target.Health == null ||
                interaction == null ||
                interaction.kind != 8)
            {
                return false;
            }

            BattleKind8EligibilityResult eligibility =
                BattleKind8EligibilityResolver.Resolve(
                    interaction.bdefend,
                    interaction.respond,
                    target.GetCurrentDataObjectTypeForSimulation(),
                    attacker.RelationTeam,
                    target.RelationTeam,
                    attacker.Runtime.OwnerSlotIndex,
                    target.Runtime.OwnerSlotIndex,
                    world.BattleGameModeId);
            if (!eligibility.Accepted)
                return false;

            if (interaction.injury != 0)
                target.HealTimer = unchecked(interaction.injury + 1000);

            int caughtAct = FirstOrZero(interaction.caughtact);
            if (caughtAct != 0)
                target.Health.PP = unchecked(target.Health.PP + caughtAct);

            if (interaction.dvx != 999)
            {
                attacker.DirectWriteRawFramePreserveWaitCounter(
                    interaction.dvx);
            }

            int syncMode = NormalizeSyncMode(interaction.dvy);
            if (syncMode == -1)
                return true;

            if (syncMode != 1)
                attacker.Runtime.X = target.Runtime.X;
            if (syncMode != 0)
                attacker.Runtime.Y = target.Runtime.Y;
            attacker.Runtime.Z = target.Runtime.Z + 1.0;
            return true;
        }

        internal static int NormalizeSyncMode(int value)
        {
            return value > 2 || value < -1 ? 0 : value;
        }

        internal static int FirstOrZero(int[] values)
        {
            return values != null && values.Length > 0 ? values[0] : 0;
        }
    }
}

