using System;

namespace NTSD.Simulation
{
    /// <summary>
    /// Immutable native OPoint content. All 24 values participate in identity;
    /// runtime spawn decoding adapts this value into a task-local DTO.
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
            int facing,
            int z = 0,
            int dvz = 0,
            int hp = 0,
            int mp = 0,
            int team = 0,
            int reserve = 0,
            int effect = 0,
            int pic = 0,
            int centerX = 0,
            int centerY = 0,
            int centerZ = 0,
            int frameA = 0,
            int attacking = 0,
            int join = 0,
            int joinReserve = 0,
            int joinPic = 0)
        {
            Kind = kind;
            X = x;
            Y = y;
            Action = action;
            Dvx = dvx;
            Dvy = dvy;
            Oid = oid;
            Facing = facing;
            Z = z;
            Dvz = dvz;
            Hp = hp;
            Mp = mp;
            Team = team;
            Reserve = reserve;
            Effect = effect;
            Pic = pic;
            CenterX = centerX;
            CenterY = centerY;
            CenterZ = centerZ;
            FrameA = frameA;
            Attacking = attacking;
            Join = join;
            JoinReserve = joinReserve;
            JoinPic = joinPic;
        }

        public int Kind { get; }
        public int X { get; }
        public int Y { get; }
        public int Action { get; }
        public int Dvx { get; }
        public int Dvy { get; }
        public int Oid { get; }
        public int Facing { get; }
        public int Z { get; }
        public int Dvz { get; }
        public int Hp { get; }
        public int Mp { get; }
        public int Team { get; }
        public int Reserve { get; }
        public int Effect { get; }
        public int Pic { get; }
        public int CenterX { get; }
        public int CenterY { get; }
        public int CenterZ { get; }
        public int FrameA { get; }
        public int Attacking { get; }
        public int Join { get; }
        public int JoinReserve { get; }
        public int JoinPic { get; }

        public bool Equals(BattleObjectPointValue other)
        {
            return Kind == other.Kind &&
                   X == other.X &&
                   Y == other.Y &&
                   Action == other.Action &&
                   Dvx == other.Dvx &&
                   Dvy == other.Dvy &&
                   Oid == other.Oid &&
                   Facing == other.Facing &&
                   Z == other.Z &&
                   Dvz == other.Dvz &&
                   Hp == other.Hp &&
                   Mp == other.Mp &&
                   Team == other.Team &&
                   Reserve == other.Reserve &&
                   Effect == other.Effect &&
                   Pic == other.Pic &&
                   CenterX == other.CenterX &&
                   CenterY == other.CenterY &&
                   CenterZ == other.CenterZ &&
                   FrameA == other.FrameA &&
                   Attacking == other.Attacking &&
                   Join == other.Join &&
                   JoinReserve == other.JoinReserve &&
                   JoinPic == other.JoinPic;
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
                hash = hash * 31 + Z;
                hash = hash * 31 + Dvz;
                hash = hash * 31 + Hp;
                hash = hash * 31 + Mp;
                hash = hash * 31 + Team;
                hash = hash * 31 + Reserve;
                hash = hash * 31 + Effect;
                hash = hash * 31 + Pic;
                hash = hash * 31 + CenterX;
                hash = hash * 31 + CenterY;
                hash = hash * 31 + CenterZ;
                hash = hash * 31 + FrameA;
                hash = hash * 31 + Attacking;
                hash = hash * 31 + Join;
                hash = hash * 31 + JoinReserve;
                hash = hash * 31 + JoinPic;
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
