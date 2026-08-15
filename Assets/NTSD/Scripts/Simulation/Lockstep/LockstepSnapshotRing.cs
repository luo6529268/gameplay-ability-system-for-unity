using System;

namespace NTSD.Simulation.Lockstep
{
    /// <summary>
    /// Fixed-capacity history of periodic complete battle snapshots. Every cell and
    /// the staging buffer are allocated during bootstrap; successful captures swap
    /// the staging buffer into the ring so a failed capture cannot corrupt history.
    /// </summary>
    public sealed class LockstepSnapshotRing
    {
        private readonly LockstepSessionIdentity identity;
        private readonly SimulationWorld world;
        private readonly BattleStateSnapshotBuffer[] snapshots;
        private readonly int[] ticks;
        private readonly int intervalTicks;

        private BattleStateSnapshotBuffer staging;
        private int count;
        private int nextWriteIndex;
        private int nextCaptureTick;

        internal LockstepSnapshotRing(
            LockstepSessionIdentity identity,
            SimulationWorld world,
            int intervalTicks,
            int capacity)
        {
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            if (intervalTicks <= 0)
                throw new ArgumentOutOfRangeException(nameof(intervalTicks));
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            this.intervalTicks = intervalTicks;
            snapshots = new BattleStateSnapshotBuffer[capacity];
            ticks = new int[capacity];
            for (int index = 0; index < capacity; index++)
                snapshots[index] = world.CreateBattleStateSnapshotBufferForBootstrap();
            staging = world.CreateBattleStateSnapshotBufferForBootstrap();
            Reset();
        }

        public int Count => count;
        public int Capacity => snapshots.Length;
        public int IntervalTicks => intervalTicks;
        public int ProtocolSchemaVersion => identity.SchemaVersion;
        public ulong IdentityFingerprint => identity.IdentityFingerprint;
        public int NextCaptureTick => nextCaptureTick;
        public int LatestTick => count == 0
            ? 0
            : ticks[nextWriteIndex == 0 ? snapshots.Length - 1 : nextWriteIndex - 1];
        public int EarliestTick
        {
            get
            {
                if (count == 0)
                    return 0;
                int oldestIndex = count == snapshots.Length ? nextWriteIndex : 0;
                return ticks[oldestIndex];
            }
        }

        public bool ShouldCapture(int tickIndex)
        {
            return tickIndex == nextCaptureTick;
        }

        internal bool TryCaptureNext(
            int tickIndex,
            out LockstepProtocolReason reason)
        {
            if (tickIndex != nextCaptureTick)
            {
                reason = LockstepProtocolReason.WrongFrameTick;
                return false;
            }
            if (!world.TryCaptureBattleStateSnapshot(identity, tickIndex, staging))
            {
                reason = LockstepProtocolReason.SnapshotCaptureFailed;
                return false;
            }

            int writeIndex = nextWriteIndex;
            BattleStateSnapshotBuffer previous = snapshots[writeIndex];
            snapshots[writeIndex] = staging;
            staging = previous;
            ticks[writeIndex] = tickIndex;

            nextWriteIndex++;
            if (nextWriteIndex == snapshots.Length)
                nextWriteIndex = 0;
            if (count < snapshots.Length)
                count++;
            nextCaptureTick = CheckedNextCaptureTick(tickIndex);
            reason = LockstepProtocolReason.None;
            return true;
        }

        public bool TryGet(int tickIndex, out BattleStateSnapshotBuffer snapshot)
        {
            if (count == 0 ||
                tickIndex < EarliestTick ||
                tickIndex > LatestTick)
            {
                snapshot = null;
                return false;
            }

            int distance = LatestTick - tickIndex;
            if (distance % intervalTicks != 0)
            {
                snapshot = null;
                return false;
            }

            int slotDistance = distance / intervalTicks;
            if (slotDistance >= count)
            {
                snapshot = null;
                return false;
            }

            int latestIndex = nextWriteIndex == 0
                ? snapshots.Length - 1
                : nextWriteIndex - 1;
            int physicalIndex = latestIndex - slotDistance;
            if (physicalIndex < 0)
                physicalIndex += snapshots.Length;
            if (ticks[physicalIndex] != tickIndex ||
                !snapshots[physicalIndex].IsValid)
            {
                snapshot = null;
                return false;
            }

            snapshot = snapshots[physicalIndex];
            return true;
        }

        public void Reset(int consumedTick = 0)
        {
            if (consumedTick < 0)
                throw new ArgumentOutOfRangeException(nameof(consumedTick));

            count = 0;
            nextWriteIndex = 0;
            nextCaptureTick = CheckedNextCaptureTick(consumedTick);
        }

        private int CheckedNextCaptureTick(int tickIndex)
        {
            if (tickIndex > int.MaxValue - intervalTicks)
                throw new InvalidOperationException("The snapshot tick range is exhausted.");
            return tickIndex + intervalTicks;
        }
    }
}
