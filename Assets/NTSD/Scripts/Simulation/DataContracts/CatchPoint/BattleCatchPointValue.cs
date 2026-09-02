using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Platform-independent immutable CPoint catalog value in C++ release
    /// field order. It is content identity, not mutable battle state.
    /// </summary>
    public readonly struct BattleCatchPointValue :
        IEquatable<BattleCatchPointValue>
    {
        public BattleCatchPointValue(
            int kind,
            int x,
            int y,
            int injury,
            int cover,
            int vaction,
            int aaction,
            int jaction,
            int daction,
            int throwVx,
            int throwVy,
            int hurtable,
            int decrease,
            int dirControl,
            int taction,
            int throwInjury,
            int throwVz,
            int frontHurtAct,
            int backHurtAct)
        {
            Kind = kind;
            X = x;
            Y = y;
            Injury = injury;
            Cover = cover;
            Vaction = vaction;
            Aaction = aaction;
            Jaction = jaction;
            Daction = daction;
            ThrowVx = throwVx;
            ThrowVy = throwVy;
            Hurtable = hurtable;
            Decrease = decrease;
            DirControl = dirControl;
            Taction = taction;
            ThrowInjury = throwInjury;
            ThrowVz = throwVz;
            FrontHurtAct = frontHurtAct;
            BackHurtAct = backHurtAct;
        }

        public int Kind { get; }
        public int X { get; }
        public int Y { get; }
        public int Injury { get; }
        public int Cover { get; }
        public int Vaction { get; }
        public int Aaction { get; }
        public int Jaction { get; }
        public int Daction { get; }
        public int ThrowVx { get; }
        public int ThrowVy { get; }
        public int Hurtable { get; }
        public int Decrease { get; }
        public int DirControl { get; }
        public int Taction { get; }
        public int ThrowInjury { get; }
        public int ThrowVz { get; }
        public int FrontHurtAct { get; }
        public int BackHurtAct { get; }

        public bool Equals(BattleCatchPointValue other)
        {
            return Kind == other.Kind &&
                   X == other.X &&
                   Y == other.Y &&
                   Injury == other.Injury &&
                   Cover == other.Cover &&
                   Vaction == other.Vaction &&
                   Aaction == other.Aaction &&
                   Jaction == other.Jaction &&
                   Daction == other.Daction &&
                   ThrowVx == other.ThrowVx &&
                   ThrowVy == other.ThrowVy &&
                   Hurtable == other.Hurtable &&
                   Decrease == other.Decrease &&
                   DirControl == other.DirControl &&
                   Taction == other.Taction &&
                   ThrowInjury == other.ThrowInjury &&
                   ThrowVz == other.ThrowVz &&
                   FrontHurtAct == other.FrontHurtAct &&
                   BackHurtAct == other.BackHurtAct;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleCatchPointValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Kind;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Injury;
                hash = hash * 31 + Cover;
                hash = hash * 31 + Vaction;
                hash = hash * 31 + Aaction;
                hash = hash * 31 + Jaction;
                hash = hash * 31 + Daction;
                hash = hash * 31 + ThrowVx;
                hash = hash * 31 + ThrowVy;
                hash = hash * 31 + Hurtable;
                hash = hash * 31 + Decrease;
                hash = hash * 31 + DirControl;
                hash = hash * 31 + Taction;
                hash = hash * 31 + ThrowInjury;
                hash = hash * 31 + ThrowVz;
                hash = hash * 31 + FrontHurtAct;
                hash = hash * 31 + BackHurtAct;
                return hash;
            }
        }

        public static bool operator ==(
            BattleCatchPointValue left,
            BattleCatchPointValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleCatchPointValue left,
            BattleCatchPointValue right)
        {
            return !left.Equals(right);
        }
    }
}
