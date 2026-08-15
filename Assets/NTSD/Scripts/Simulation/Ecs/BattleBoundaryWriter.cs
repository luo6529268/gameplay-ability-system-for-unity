using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Owns directional block publication and keeps the canonical character
    /// input row synchronized when physics consumes the one-tick flags.
    /// </summary>
    internal sealed class BattleBoundaryWriter
    {
        private readonly BattleCharacterInputWriter characterInputWriter;

        internal BattleBoundaryWriter(
            BattleCharacterInputWriter characterInputWriter)
        {
            this.characterInputWriter = characterInputWriter;
        }

        internal bool TryApplyKind14DirectionalBlock(
            LF2Entity attacker,
            LF2Entity victim)
        {
            if (attacker?.Runtime == null || victim?.Runtime == null)
                return false;

            NTSDEntityRuntime runtime = victim.Runtime;
            int attackerX = attacker.Runtime.XInt;
            int attackerZ = attacker.Runtime.ZInt;
            int victimX = runtime.XInt;
            int victimZ = runtime.ZInt;

            if (attackerX > victimX + 5 &&
                (runtime.Vx > 0.0 || victim.KnockbackVx > 0.0))
            {
                runtime.XBoundPositive = true;
            }
            else if (attackerX < victimX - 5 &&
                     (runtime.Vx < 0.0 || victim.KnockbackVx < 0.0))
            {
                runtime.XBoundNegative = true;
            }

            if (attackerZ > victimZ + 2 &&
                (runtime.Vz > 0.0 || victim.KnockbackVz > 0.0))
            {
                runtime.ZBoundPositive = true;
            }
            else if (attackerZ < victimZ - 2 &&
                     (runtime.Vz < 0.0 || victim.KnockbackVz < 0.0))
            {
                runtime.ZBoundNegative = true;
            }

            characterInputWriter.SyncBoundaryFlagsFromRuntime(runtime);
            return true;
        }

        internal void SyncConsumedFlags(NTSDEntityRuntime runtime)
        {
            characterInputWriter.SyncBoundaryFlagsFromRuntime(runtime);
        }
    }
}
