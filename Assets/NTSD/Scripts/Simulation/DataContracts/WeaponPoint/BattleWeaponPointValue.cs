using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Platform-independent immutable WPoint content in C++ release field
    /// order. Runtime holder/link/entity mutations stay outside this value.
    /// </summary>
    public readonly struct BattleWeaponPointValue :
        IEquatable<BattleWeaponPointValue>
    {
        public BattleWeaponPointValue(
            int kind,
            int x,
            int y,
            int attacking,
            int cover,
            int weaponAct,
            int dvx,
            int dvy,
            int dvz)
        {
            Kind = kind;
            X = x;
            Y = y;
            Attacking = attacking;
            Cover = cover;
            WeaponAct = weaponAct;
            Dvx = dvx;
            Dvy = dvy;
            Dvz = dvz;
        }

        public int Kind { get; }
        public int X { get; }
        public int Y { get; }
        public int Attacking { get; }
        public int Cover { get; }
        public int WeaponAct { get; }
        public int Dvx { get; }
        public int Dvy { get; }
        public int Dvz { get; }

        public bool Equals(BattleWeaponPointValue other)
        {
            return Kind == other.Kind &&
                   X == other.X &&
                   Y == other.Y &&
                   Attacking == other.Attacking &&
                   Cover == other.Cover &&
                   WeaponAct == other.WeaponAct &&
                   Dvx == other.Dvx &&
                   Dvy == other.Dvy &&
                   Dvz == other.Dvz;
        }

        public override bool Equals(object obj)
        {
            return obj is BattleWeaponPointValue other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + Kind;
                hash = hash * 31 + X;
                hash = hash * 31 + Y;
                hash = hash * 31 + Attacking;
                hash = hash * 31 + Cover;
                hash = hash * 31 + WeaponAct;
                hash = hash * 31 + Dvx;
                hash = hash * 31 + Dvy;
                hash = hash * 31 + Dvz;
                return hash;
            }
        }

        public static bool operator ==(
            BattleWeaponPointValue left,
            BattleWeaponPointValue right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(
            BattleWeaponPointValue left,
            BattleWeaponPointValue right)
        {
            return !left.Equals(right);
        }
    }
}
