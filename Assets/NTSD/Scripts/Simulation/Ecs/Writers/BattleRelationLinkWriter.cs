namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// World-owned lifecycle and read boundary for the low-frequency relation
    /// and link projection used by same-tick AI.
    /// </summary>
    internal sealed class BattleRelationLinkWriter
    {
        private readonly BattleRelationLinkStore store;

        internal BattleRelationLinkWriter(
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            store = new BattleRelationLinkStore(capacity, unifiedRowPublisher);
        }

        internal void Bind(NTSDEntityRuntime runtime, RuntimeEntityHandle handle)
        {
            store.Bind(runtime, handle);
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            store.Release(handle);
        }

        internal void Reset()
        {
            store.Reset();
        }

        internal void GrowTo(int capacity)
        {
            store.GrowTo(capacity);
        }

        internal bool TryCaptureAiProjection(
            NTSDEntityRuntime runtime,
            out BattleRelationLinkAiProjection projection)
        {
            return store.TryCaptureAiProjection(runtime, out projection);
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleRelationLinkAiProjection projection)
        {
            return store.TryCaptureAiProjection(handle, out projection);
        }

        internal bool TryGetState(
            NTSDEntityRuntime runtime,
            out BattleRelationLinkStateView view)
        {
            return store.TryGetState(runtime, out view);
        }

        internal int PositiveLinkCount => store.PositiveLinkCount;

        internal int FindNextPositiveLinkSlot(int startSlot)
        {
            return store.FindNextPositiveLinkSlot(startSlot);
        }

        internal bool TryGetPositiveLinkHandle(
            int slot,
            out RuntimeEntityHandle handle)
        {
            return store.TryGetPositiveLinkHandle(slot, out handle);
        }
    }
}
