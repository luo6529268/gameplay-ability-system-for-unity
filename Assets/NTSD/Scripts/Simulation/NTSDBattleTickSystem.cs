namespace NTSD.Simulation
{
    /// <summary>
    /// Unity NTSD 战斗 tick 调度器。
    /// pass 顺序以 C++ release 工程为基准；实体专属行为留在 LF2Entity 子类中，
    /// 本类只负责集中维护这些 pass 的执行时机。
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
            ProcessHeldObjects(tickIndex);
            FrameAdvanceAll(tickIndex);
            ResolvePostInteractions(tickIndex);
            RandomWeaponDrop(tickIndex);
            ResolvePreInteractions(tickIndex);
            ProcessHeldObjects(tickIndex);
            FramePostProcess();
            LateEntityUpdate(tickIndex);
        }

        private void TickCooldowns(int tickIndex)
        {
            world.VrestTickAll(tickIndex);
        }

        private void ProcessHeldObjects(int tickIndex)
        {
            world.HeldObjectProcessAll(tickIndex);
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

        private void LateEntityUpdate(int tickIndex)
        {
            world.LateEntityUpdateAll(tickIndex);
        }

    }
}
