namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// World-owned lifecycle and read boundary for vitality state used by
    /// same-tick AI.
    /// </summary>
    internal sealed class BattleVitalWriter
    {
        private readonly BattleVitalStore store;

        internal BattleVitalWriter(
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            store = new BattleVitalStore(capacity, unifiedRowPublisher);
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
            out BattleVitalAiProjection projection)
        {
            return store.TryCaptureAiProjection(runtime, out projection);
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleVitalAiProjection projection)
        {
            return store.TryCaptureAiProjection(handle, out projection);
        }

        internal bool TryGetState(
            NTSDEntityRuntime runtime,
            out BattleVitalStateView view)
        {
            return store.TryGetState(runtime, out view);
        }
    }
}
