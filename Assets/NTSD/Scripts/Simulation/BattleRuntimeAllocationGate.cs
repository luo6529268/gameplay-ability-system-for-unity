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

        public void PrepareNonUnityCapacity(int requestedRuntimeCapacity)
        {
            if (isSealed || requestedRuntimeCapacity <= 0)
                return;

            reservedRuntimeCapacity = requestedRuntimeCapacity;

            LF2ReferencePool referencePool = LF2ReferencePool.Instance;
            if (referencePool != null)
            {
                referencePool.PrepareObjectCapacity(
                    LF2ObjectType.Character,
                    requestedRuntimeCapacity);
                referencePool.PrewarmTasks<OPointCreateTask>(requestedRuntimeCapacity);
                referencePool.PrewarmTasks<OPointCreateMultipleTask>(requestedRuntimeCapacity);
            }

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            factory?.PrepareTaskQueueCapacity(requestedRuntimeCapacity);
        }

        public void Seal()
        {
            if (isSealed)
                return;

            LF2ReferencePool.Instance?.SealBattleCapacity();
            LF2ObjectPool.Instance?.SealBattleCapacity();
            LF2ObjectPointFactory.Instance?.SealBattleTaskCapacity();
            isSealed = true;
        }

        public void Unseal()
        {
            if (!isSealed)
                return;

            LF2ObjectPointFactory.Instance?.UnsealBattleTaskCapacity();
            LF2ObjectPool.Instance?.UnsealBattleCapacity();
            LF2ReferencePool.Instance?.UnsealBattleCapacity();
            isSealed = false;
        }
    }
}
