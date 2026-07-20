namespace NTSD.Simulation.Spatial
{
    public sealed class SpatialBroadphaseDiagnostics
    {
        public int RebuildCount { get; internal set; }
        public int IndexedCount { get; internal set; }
        public int FallbackCount { get; internal set; }
        public int BrutePairCount { get; internal set; }
        public int QuadtreePairCount { get; internal set; }
        public int AcceptedPairCount { get; internal set; }
        public int MismatchCount { get; internal set; }
        public long FirstMissingPair { get; internal set; } = -1;
        public long FirstExtraPair { get; internal set; } = -1;
        public long FirstAcceptedPairMissingFromTree { get; internal set; } = -1;

        internal void Begin(int indexedCount)
        {
            RebuildCount++;
            IndexedCount = indexedCount;
            FallbackCount = 0;
            BrutePairCount = 0;
            QuadtreePairCount = 0;
            AcceptedPairCount = 0;
            MismatchCount = 0;
            FirstMissingPair = -1;
            FirstExtraPair = -1;
            FirstAcceptedPairMissingFromTree = -1;
        }
    }
}
