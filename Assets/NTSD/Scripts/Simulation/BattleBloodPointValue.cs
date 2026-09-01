using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Platform-independent immutable BPoint catalog value in C++ release
    /// field order. It is catalog identity, not battle world state.
    /// </summary>
    public readonly struct BattleBloodPointValue :
        IEquatable<BattleBloodPointValue>
    {
        public BattleBloodPointValue(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public bool Equals(BattleBloodPointValue other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleBloodPointValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                return hash;
            }
        }

        public static bool operator ==(
            BattleBloodPointValue left,
            BattleBloodPointValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleBloodPointValue left,
            BattleBloodPointValue right)
        {
            return !left.Equals(right);
        }
    }
}
