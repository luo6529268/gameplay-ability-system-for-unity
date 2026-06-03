namespace NTSD.Simulation
{
    /// <summary>
    /// Formal battle tick coordinator for the Unity NTSD runtime.
    /// The pass order is based on the C++ release project and keeps entity-specific behavior in
    /// LF2Entity subclasses while centralizing when those passes execute.
    /// </summary>
    public sealed class NTSDBattleTickSystem
    {
        private readonly SimulationWorld world;

        public NTSDBattleTickSystem(SimulationWorld world)
        {
            this.world = world;
        }

        public void RunReleaseTick(int tickIndex, int sparkRenderFrame)
        {
            if (world == null) return;

            TickCooldowns(tickIndex);
            ResolvePreInteractions(tickIndex);
            FrameAdvanceAll(tickIndex);
            RandomWeaponDrop(tickIndex);
            ResolvePostInteractions(tickIndex);
            FramePostProcess();
            EntityCollision(tickIndex);
            LateEntityUpdate(tickIndex);
            TickSparkTimers(sparkRenderFrame);
        }

        private void TickCooldowns(int tickIndex)
        {
            world.VrestTickAll(tickIndex);
        }

        private void ResolvePreInteractions(int tickIndex)
        {
            world.PreInteractionTickAll(tickIndex);
        }

        private void FrameAdvanceAll(int tickIndex)
        {
            world.SerialTickAll(tickIndex);
        }

        private void RandomWeaponDrop(int tickIndex)
        {
            world.RandomWeaponDropTickAll(tickIndex);
        }

        private void ResolvePostInteractions(int tickIndex)
        {
            world.PostInteractionTickAll(tickIndex);
        }

        private void FramePostProcess()
        {
            world.FramePostProcessAll();
        }

        private void EntityCollision(int tickIndex)
        {
            world.EntityCollisionTickAll(tickIndex);
        }

        private void LateEntityUpdate(int tickIndex)
        {
            world.LateTick(tickIndex);
        }

        private void TickSparkTimers(int sparkRenderFrame)
        {
            world.TickSparkTimers(sparkRenderFrame);
        }
    }
}
