using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the battle allocation boundary. Loading and match assembly may grow
    /// capacity; after sealing, covered pools fail closed and only increment
    /// numeric diagnostics instead of allocating or logging in the hot path.
    /// </summary>
    public sealed class BattleRuntimeAllocationGate
    {
        private bool isSealed;
        private int reservedRuntimeCapacity;

        public bool IsSealed => isSealed;
        public int ReservedRuntimeCapacity => reservedRuntimeCapacity;

        internal void PrepareNonUnityCapacity(
            int requestedRuntimeCapacity,
            SimulationWorld world)
        {
            if (isSealed || requestedRuntimeCapacity <= 0)
                return;

            reservedRuntimeCapacity = requestedRuntimeCapacity;
            BattleLogicReferencePool logicReferencePool =
                world?.LogicReferencePool;

            if (logicReferencePool != null)
            {
                logicReferencePool.PrepareBattleEntityShellCapacity(
                    requestedRuntimeCapacity);
                logicReferencePool.PrewarmTasks<OPointCreateTask>(requestedRuntimeCapacity);
                logicReferencePool.PrewarmTasks<OPointCreateMultipleTask>(requestedRuntimeCapacity);
            }

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            factory?.PrepareTaskQueueCapacity(requestedRuntimeCapacity);
            world?.LogicObjectPointRuntime?.PrepareTaskQueueCapacity(
                requestedRuntimeCapacity);
        }

        internal void Seal(SimulationWorld world)
        {
            if (isSealed)
                return;

            world?.LogicReferencePool?.SealBattleCapacity();
            world?.LogicObjectPointRuntime?.SealBattleTaskCapacity();
            LF2ObjectPool.Instance?.SealBattleCapacity();
            LF2ObjectPointFactory.Instance?.SealBattleTaskCapacity();
            isSealed = true;
        }

        internal void Unseal(SimulationWorld world)
        {
            if (!isSealed)
                return;

            LF2ObjectPointFactory.Instance?.UnsealBattleTaskCapacity();
            LF2ObjectPool.Instance?.UnsealBattleCapacity();
            world?.LogicObjectPointRuntime?.UnsealBattleTaskCapacity();
            world?.LogicReferencePool?.UnsealBattleCapacity();
            isSealed = false;
        }
    }
}
