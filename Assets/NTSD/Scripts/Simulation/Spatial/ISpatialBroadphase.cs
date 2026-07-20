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

    public interface ISpatialBroadphase
    {
        void Rebuild(IReadOnlyList<SpatialBroadphaseEntry> entries, in SpatialAabbXZ preferredRoot);

        void Query(in SpatialAabbXZ bounds, List<int> resultEntryIndices);
    }
}
