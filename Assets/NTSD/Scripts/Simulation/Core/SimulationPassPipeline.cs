namespace NTSD.Simulation
{
    /// <summary>
    /// Composes behavior-domain pass modules behind the stable SimulationWorld
    /// scheduling façade. The external scheduler retains the authoritative order.
    /// </summary>
    internal sealed class SimulationPassPipeline
    {
        private readonly BattleOid5152RuntimeModule oid5152RuntimeModule;
        private readonly BattleRespawnModule respawnModule;
        private readonly BattleEarlyFrameAdvanceModule earlyFrameAdvanceModule;
        private readonly BattleLateEntityLifecycleModule lateEntityLifecycleModule;
        private readonly BattleInteractionPipeline interactionPipeline;
        private readonly BattleRandomWeaponDropModule randomWeaponDropModule;

        internal SimulationPassPipeline(SimulationWorld world)
        {
            oid5152RuntimeModule = new BattleOid5152RuntimeModule(world);
            respawnModule = new BattleRespawnModule(world);
            earlyFrameAdvanceModule = new BattleEarlyFrameAdvanceModule(world);
            lateEntityLifecycleModule =
                new BattleLateEntityLifecycleModule(world);
            interactionPipeline = new BattleInteractionPipeline(world);
            randomWeaponDropModule = new BattleRandomWeaponDropModule(world);
        }

        internal BattleEarlyFrameAdvanceModule EarlyFrameAdvance =>
            earlyFrameAdvanceModule;
        internal BattleLateEntityLifecycleModule LateEntityLifecycle =>
            lateEntityLifecycleModule;
        internal BattleInteractionPipeline Interaction => interactionPipeline;

        internal void PrepareCapacity(int entityCapacity)
        {
            earlyFrameAdvanceModule.PrepareCapacity(entityCapacity);
            lateEntityLifecycleModule.PrepareCapacity(entityCapacity);
            interactionPipeline.PrepareCapacity(entityCapacity);
        }

        internal void RunOid5152Maintenance(int tickIndex)
        {
            oid5152RuntimeModule.RunMaintenance(tickIndex);
        }

        internal void RunRespawn(int tickIndex)
        {
            respawnModule.RunPostFrameAdvanceDeathCleanup(tickIndex);
        }

        internal void RunEarlyFrameAdvance(int tickIndex)
        {
            earlyFrameAdvanceModule.Run(tickIndex);
        }

        internal void RunLateEntityLifecycle(int tickIndex)
        {
            lateEntityLifecycleModule.Run(tickIndex);
        }

        internal void RunLateStateSpecialPreCollisionForSelfCheck(
            NTSD.Animation.LF2Objects.LF2Entity entity)
        {
            lateEntityLifecycleModule.RunStateSpecialPreCollisionForSelfCheck(
                entity);
        }

        internal void RefreshLateTransitionRuntimeSnapshot(
            NTSD.Animation.LF2Objects.LF2Entity entity)
        {
            lateEntityLifecycleModule.RefreshTransitionRuntimeSnapshot(entity);
        }

        internal void EndCollisionCandidateConsumption()
        {
            interactionPipeline.EndCollisionCandidateConsumption();
        }

        internal void RunPostInteraction(int tickIndex)
        {
            interactionPipeline.RunPostInteraction(tickIndex);
        }

        internal void RunObjectInteraction(int tickIndex)
        {
            interactionPipeline.RunObjectInteraction(tickIndex);
        }

        internal void RunPreInteraction(int tickIndex)
        {
            interactionPipeline.RunPreInteraction(tickIndex);
        }

        internal void RunRandomWeaponDrop(int tickIndex)
        {
            randomWeaponDropModule.RunNormalDrop(tickIndex);
        }

        internal void RunMode2RandomWeaponDropTail(int tickIndex)
        {
            randomWeaponDropModule.RunMode2Tail(tickIndex);
        }
    }
}
