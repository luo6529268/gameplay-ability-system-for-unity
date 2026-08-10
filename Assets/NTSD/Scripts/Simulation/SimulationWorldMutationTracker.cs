using System.Threading;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns mutation epochs that invalidate caches for one simulation world.
    /// Runtimes notify this tracker without allocating delegates or sharing state
    /// with another battle.
    /// </summary>
    internal sealed class SimulationWorldMutationTracker
    {
        private long pendingFlushDestroyEpoch;

        internal long PendingFlushDestroyEpoch =>
            Volatile.Read(ref pendingFlushDestroyEpoch);

        internal void NotifyPendingFlushDestroyMutation()
        {
            Interlocked.Increment(ref pendingFlushDestroyEpoch);
        }
    }
}
