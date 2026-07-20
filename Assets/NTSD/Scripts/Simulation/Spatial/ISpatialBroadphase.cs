using System.Collections.Generic;

namespace NTSD.Simulation.Spatial
{
    public readonly struct SpatialBroadphaseEntry
    {
        public SpatialBroadphaseEntry(int runtimeSlot, int inputIndex, in SpatialAabbXZ bounds)
        {
            RuntimeSlot = runtimeSlot;
            InputIndex = inputIndex;
            Bounds = bounds;
        }

        public int RuntimeSlot { get; }
        public int InputIndex { get; }
        public SpatialAabbXZ Bounds { get; }
    }

    public readonly struct IncrementalSpatialEntry
    {
        public IncrementalSpatialEntry(RuntimeEntityHandle handle, in SpatialAabbXZ bounds)
        {
            Handle = handle;
            Bounds = bounds;
        }

        public RuntimeEntityHandle Handle { get; }
        public SpatialAabbXZ Bounds { get; }
    }

    public readonly struct SpatialSynchronizeResult
    {
        public SpatialSynchronizeResult(
            bool succeeded,
            bool fullRebuild,
            int insertedCount,
            int updatedInPlaceCount,
            int migratedCount,
            int removedCount,
            int indexedCount)
        {
            Succeeded = succeeded;
            FullRebuild = fullRebuild;
            InsertedCount = insertedCount;
            UpdatedInPlaceCount = updatedInPlaceCount;
            MigratedCount = migratedCount;
            RemovedCount = removedCount;
            IndexedCount = indexedCount;
        }

        public bool Succeeded { get; }
        public bool FullRebuild { get; }
        public int InsertedCount { get; }
        public int UpdatedInPlaceCount { get; }
        public int MigratedCount { get; }
        public int RemovedCount { get; }
        public int IndexedCount { get; }

        public static SpatialSynchronizeResult Failed =>
            new SpatialSynchronizeResult(false, false, 0, 0, 0, 0, 0);
    }

    public interface ISpatialBroadphase
    {
        void Rebuild(IReadOnlyList<SpatialBroadphaseEntry> entries, in SpatialAabbXZ preferredRoot);

        void Query(in SpatialAabbXZ bounds, List<int> resultEntryIndices);
    }

    public interface IIncrementalSpatialBroadphase
    {
        SpatialSynchronizeResult Synchronize(
            IReadOnlyList<IncrementalSpatialEntry> entries,
            in SpatialAabbXZ preferredRoot);

        void QueryHandles(in SpatialAabbXZ bounds, List<RuntimeEntityHandle> resultHandles);

        void ResetIncremental();
    }
}
