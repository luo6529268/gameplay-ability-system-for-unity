using System;
using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// Platform-independent immutable Bdy geometry in C++ release field order.
    /// Direction, frame-center offsets and full-height classification stay
    /// external runtime concerns.
    /// </summary>
    public readonly struct BattleBodyBoxValue :
        IEquatable<BattleBodyBoxValue>
    {
        private readonly bool geometryMissing;

        public BattleBodyBoxValue(int x, int y, int w, int h, int zWidth = 0, bool hasGeometry = true)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
            ZWidth = zWidth;
            geometryMissing = !hasGeometry;
        }

        public int X { get; }
        public int Y { get; }
        public int W { get; }
        public int H { get; }
        public int ZWidth { get; }

        // Legacy default is an explicit zero box; native missing geometry is constructed explicitly.
        public bool HasGeometry => !geometryMissing;

        public bool Equals(BattleBodyBoxValue other)
        {
            return X == other.X &&
                   Y == other.Y &&
                   W == other.W &&
                   H == other.H &&
                   ZWidth == other.ZWidth &&
                   HasGeometry == other.HasGeometry;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleBodyBoxValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + W;
                hash = hash * 31 + H;
                hash = hash * 31 + ZWidth;
                hash = hash * 31 + (HasGeometry ? 1 : 0);
                return hash;
            }
        }

        public static bool operator ==(
            BattleBodyBoxValue left,
            BattleBodyBoxValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleBodyBoxValue left,
            BattleBodyBoxValue right)
        {
            return !left.Equals(right);
        }

        public static implicit operator BattleBodyBoxValue(BodyBox source)
        {
            return BattleBodyBoxValueAdapter.FromLegacy(source);
        }
    }
}

