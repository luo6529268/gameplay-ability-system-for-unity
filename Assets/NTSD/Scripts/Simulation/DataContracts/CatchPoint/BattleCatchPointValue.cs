using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Platform-independent immutable CPoint catalog value in C++ release
    /// 27-field content contract. Float identity preserves binary32 bits.
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
            float throwVx,
            float throwVy,
            int hurtable,
            int decrease,
            int dirControl,
            int taction,
            int throwInjury,
            float throwVz,
            int frontHurtAct,
            int backHurtAct,
            int faction = 0,
            int baction = 0,
            int uzaction = 0,
            int dzaction = 0,
            int z = 0,
            int recover = 0,
            int drain = 0,
            int gain = 0)
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
            Faction = faction;
            Baction = baction;
            Uzaction = uzaction;
            Dzaction = dzaction;
            Z = z;
            Recover = recover;
            Drain = drain;
            Gain = gain;
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
        public float ThrowVx { get; }
        public float ThrowVy { get; }
        public int Hurtable { get; }
        public int Decrease { get; }
        public int DirControl { get; }
        public int Taction { get; }
        public int ThrowInjury { get; }
        public float ThrowVz { get; }
        public int FrontHurtAct { get; }
        public int BackHurtAct { get; }
        public int Faction { get; }
        public int Baction { get; }
        public int Uzaction { get; }
        public int Dzaction { get; }
        public int Z { get; }
        public int Recover { get; }
        public int Drain { get; }
        public int Gain { get; }

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
                   BitConverter.SingleToInt32Bits(ThrowVx) == BitConverter.SingleToInt32Bits(other.ThrowVx) &&
                   BitConverter.SingleToInt32Bits(ThrowVy) == BitConverter.SingleToInt32Bits(other.ThrowVy) &&
                   Hurtable == other.Hurtable &&
                   Decrease == other.Decrease &&
                   DirControl == other.DirControl &&
                   Taction == other.Taction &&
                   ThrowInjury == other.ThrowInjury &&
                   BitConverter.SingleToInt32Bits(ThrowVz) == BitConverter.SingleToInt32Bits(other.ThrowVz) &&
                   FrontHurtAct == other.FrontHurtAct &&
                   BackHurtAct == other.BackHurtAct &&
                   Faction == other.Faction &&
                   Baction == other.Baction &&
                   Uzaction == other.Uzaction &&
                   Dzaction == other.Dzaction &&
                   Z == other.Z &&
                   Recover == other.Recover &&
                   Drain == other.Drain &&
                   Gain == other.Gain;
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
                hash = hash * 31 + BitConverter.SingleToInt32Bits(ThrowVx);
                hash = hash * 31 + BitConverter.SingleToInt32Bits(ThrowVy);
                hash = hash * 31 + Hurtable;
                hash = hash * 31 + Decrease;
                hash = hash * 31 + DirControl;
                hash = hash * 31 + Taction;
                hash = hash * 31 + ThrowInjury;
                hash = hash * 31 + BitConverter.SingleToInt32Bits(ThrowVz);
                hash = hash * 31 + FrontHurtAct;
                hash = hash * 31 + BackHurtAct;
                hash = hash * 31 + Faction;
                hash = hash * 31 + Baction;
                hash = hash * 31 + Uzaction;
                hash = hash * 31 + Dzaction;
                hash = hash * 31 + Z;
                hash = hash * 31 + Recover;
                hash = hash * 31 + Drain;
                hash = hash * 31 + Gain;
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
