namespace NTSD.Simulation
{
    /// <summary>
    /// Owns match-scoped runtime capacity. Capacity may be prepared while loading,
    /// but production tick code must not grow managed storage after the seal closes.
    /// </summary>
    public sealed class SimulationRuntimeCapacityModule
    {
        private readonly RuntimeSlotTable runtimeSlots;
        private readonly RuntimeRestStore runtimeRestStore;
        private readonly SimulationBattleBufferModule battleBuffers;
        private readonly SimulationObjectBucketRegistry objectBucketRegistry;
        private bool isSealed;

        public SimulationRuntimeCapacityModule(
            RuntimeSlotTable runtimeSlots,
            RuntimeRestStore runtimeRestStore,
            SimulationBattleBufferModule battleBuffers = null)
            : this(runtimeSlots, runtimeRestStore, battleBuffers, null)
        {
        }

        internal SimulationRuntimeCapacityModule(
            RuntimeSlotTable runtimeSlots,
            RuntimeRestStore runtimeRestStore,
            SimulationBattleBufferModule battleBuffers,
            SimulationObjectBucketRegistry objectBucketRegistry = null)
        {
            this.runtimeSlots = runtimeSlots;
            this.runtimeRestStore = runtimeRestStore;
            this.battleBuffers = battleBuffers;
            this.objectBucketRegistry = objectBucketRegistry;
        }

        public bool IsSealed => isSealed;
        public long RejectedGrowthCount { get; private set; }

        public void PrepareForBattle()
        {
            if (isSealed)
                return;

            runtimeSlots.PrepareAllPages();
            runtimeRestStore.PrepareForBattle();
        }

        public void Seal()
        {
            if (isSealed)
                return;

            PrepareForBattle();
            runtimeRestStore.SealCapacity();
            battleBuffers?.Seal();
            objectBucketRegistry?.SealCapacity();
            isSealed = true;
        }

        public void Unseal()
        {
            if (!isSealed)
                return;

            battleBuffers?.Unseal();
            objectBucketRegistry?.UnsealCapacity();
            runtimeRestStore.UnsealCapacity();
            isSealed = false;
        }

        public bool TryAuthorizeGrowth()
        {
            if (!isSealed)
                return true;

            RejectedGrowthCount++;
            return false;
        }
    }
}
