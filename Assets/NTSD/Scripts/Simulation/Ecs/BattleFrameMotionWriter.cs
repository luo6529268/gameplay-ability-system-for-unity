using System;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// World-owned lifecycle boundary for the same-tick AI frame/motion projection.
    /// Existing entity code keeps its authority ordering while the required fields
    /// are published to the generation-owned store at their original write points.
    /// </summary>
    internal sealed class BattleFrameMotionWriter
    {
        private readonly BattleFrameMotionStore store;

        internal BattleFrameMotionWriter(
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            store = new BattleFrameMotionStore(capacity, unifiedRowPublisher);
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
            out BattleFrameMotionAiProjection projection)
        {
            return store.TryCaptureAiProjection(runtime, out projection);
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleFrameMotionAiProjection projection)
        {
            return store.TryCaptureAiProjection(handle, out projection);
        }

        internal bool TryGetState(
            NTSDEntityRuntime runtime,
            out BattleFrameMotionStateView view)
        {
            return store.TryGetState(runtime, out view);
        }

    }
}
