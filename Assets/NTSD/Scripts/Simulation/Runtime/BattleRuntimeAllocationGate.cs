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
        private LF2ObjectPointFactory capturedFactory;
        private LF2ObjectPool capturedObjectPool;

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

            capturedFactory = LF2ObjectPointFactory.Instance;
            capturedObjectPool = LF2ObjectPool.Instance;
            capturedFactory?.PrepareTaskQueueCapacity(requestedRuntimeCapacity);
            world?.LogicObjectPointRuntime?.PrepareTaskQueueCapacity(
                requestedRuntimeCapacity);
        }

        internal void Seal(SimulationWorld world)
        {
            if (isSealed)
                return;

            world?.LogicReferencePool?.SealBattleCapacity();
            world?.LogicObjectPointRuntime?.SealBattleTaskCapacity();
            capturedObjectPool ??= LF2ObjectPool.Instance;
            capturedFactory ??= LF2ObjectPointFactory.Instance;
            capturedObjectPool?.SealBattleCapacity();
            capturedFactory?.SealBattleTaskCapacity();
            isSealed = true;
        }

        internal void Unseal(SimulationWorld world)
        {
            if (!isSealed)
            {
                capturedFactory = null;
                capturedObjectPool = null;
                return;
            }

            (capturedFactory != null
                ? capturedFactory
                : LF2ObjectPointFactory.TryGetInstance())?.UnsealBattleTaskCapacity();
            (capturedObjectPool != null
                ? capturedObjectPool
                : LF2ObjectPool.TryGetInstance())?.UnsealBattleCapacity();
            world?.LogicObjectPointRuntime?.UnsealBattleTaskCapacity();
            world?.LogicReferencePool?.UnsealBattleCapacity();
            capturedFactory = null;
            capturedObjectPool = null;
            isSealed = false;
        }
    }
}
