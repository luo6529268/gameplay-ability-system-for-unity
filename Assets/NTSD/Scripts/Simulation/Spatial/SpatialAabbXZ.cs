using System;

namespace NTSD.Simulation.Spatial
{
    public readonly struct SpatialAabbXZ : IEquatable<SpatialAabbXZ>
    {
        public SpatialAabbXZ(int minX, int minZ, int maxX, int maxZ)
        {
            MinX = minX;
            MinZ = minZ;
            MaxX = maxX;
            MaxZ = maxZ;
        }

        public int MinX { get; }
        public int MinZ { get; }
        public int MaxX { get; }
        public int MaxZ { get; }
        public bool IsValid => MinX < MaxX && MinZ < MaxZ;

        public bool Overlaps(in SpatialAabbXZ other)
        {
            return IsValid && other.IsValid &&
                   MinX < other.MaxX && MaxX > other.MinX &&
                   MinZ < other.MaxZ && MaxZ > other.MinZ;
        }

        public bool Contains(in SpatialAabbXZ other)
        {
            return IsValid && other.IsValid &&
                   MinX <= other.MinX && MaxX >= other.MaxX &&
                   MinZ <= other.MinZ && MaxZ >= other.MaxZ;
        }

        public static SpatialAabbXZ Normalize(int x1, int z1, int x2, int z2)
        {
            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minZ = Math.Min(z1, z2);
            int maxZ = Math.Max(z1, z2);
            return new SpatialAabbXZ(minX, minZ, maxX, maxZ);
        }

        public bool Equals(SpatialAabbXZ other)
        {
            return MinX == other.MinX && MinZ == other.MinZ &&
                   MaxX == other.MaxX && MaxZ == other.MaxZ;
        }

        public override bool Equals(object obj) => obj is SpatialAabbXZ other && Equals(other);

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = MinX;
                hash = (hash * 397) ^ MinZ;
                hash = (hash * 397) ^ MaxX;
                return (hash * 397) ^ MaxZ;
            }
        }
    }
}
