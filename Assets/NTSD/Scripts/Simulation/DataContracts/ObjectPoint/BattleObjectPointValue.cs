using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Platform-independent immutable OPoint content in C++ release semantic
    /// order. Runtime spawn decoding adapts this value into a task-local DTO.
    /// </summary>
    public readonly struct BattleObjectPointValue :
        IEquatable<BattleObjectPointValue>
    {
        public BattleObjectPointValue(
            int kind,
            int x,
            int y,
            int action,
            int dvx,
            int dvy,
            int oid,
            int facing)
        {
            Kind = kind;
            X = x;
            Y = y;
            Action = action;
            Dvx = dvx;
            Dvy = dvy;
            Oid = oid;
            Facing = facing;
        }

        public int Kind { get; }
        public int X { get; }
        public int Y { get; }
        public int Action { get; }
        public int Dvx { get; }
        public int Dvy { get; }
        public int Oid { get; }
        public int Facing { get; }

        public bool Equals(BattleObjectPointValue other)
        {
            return Kind == other.Kind &&
                   X == other.X &&
                   Y == other.Y &&
                   Action == other.Action &&
                   Dvx == other.Dvx &&
                   Dvy == other.Dvy &&
                   Oid == other.Oid &&
                   Facing == other.Facing;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleObjectPointValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Kind;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Action;
                hash = hash * 31 + Dvx;
                hash = hash * 31 + Dvy;
                hash = hash * 31 + Oid;
                hash = hash * 31 + Facing;
                return hash;
            }
        }

        public static bool operator ==(
            BattleObjectPointValue left,
            BattleObjectPointValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleObjectPointValue left,
            BattleObjectPointValue right)
        {
            return !left.Equals(right);
        }
    }
}
