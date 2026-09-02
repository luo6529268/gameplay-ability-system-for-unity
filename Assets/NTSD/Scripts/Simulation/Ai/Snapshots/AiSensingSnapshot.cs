using System;

namespace NTSD.Simulation
{
    public struct AiSensingTeamSummary
    {
        public int Team;
        public int Count;
        public int MinHp;
        public int MinCount;
        public int SecondMinHp;
    }

    public struct AiSensingRoleTeamSummary
    {
        public int Team;
        public int Start;
        public int Count;
    }

    public class AiSensingSnapshot
    {
        public AiSensingSnapshot(int capacity)
        {
            Included = new bool[capacity];
            SpecialScanMember = new bool[capacity];
            SpecialSlots = new int[capacity];
            GroundRoleSlotsByX = new int[capacity];
            AirRoleSlotsByX = new int[capacity];
            GroundRoleTeamSummaries = new AiSensingRoleTeamSummary[capacity];
            AirRoleTeamSummaries = new AiSensingRoleTeamSummary[capacity];
            TeamSummaries = new AiSensingTeamSummary[capacity];
            InputHistoryGate = new bool[capacity];
            Generation = new uint[capacity];
            Identity = new int[capacity];
            ObjectId = new int[capacity];
            DataObjectType = new int[capacity];
            X = new int[capacity];
            Y = new int[capacity];
            Z = new int[capacity];
            Hp = new int[capacity];
            Hp3 = new int[capacity];
            HpMax = new int[capacity];
            Pp = new int[capacity];
            Team = new int[capacity];
            State = new int[capacity];
            Frame = new int[capacity];
            HitJ = new int[capacity];
            LinkState = new int[capacity];
            KillCount = new int[capacity];
            CachedTargetSlot = new int[capacity];
            CoordinateTargetX = new int[capacity];
            Vx = new double[capacity];
            Facing = new int[capacity];
            TargetSlot = new int[capacity];
            HitStop = new int[capacity];
            BoundaryFlags = new int[capacity];
        }

        public int Capacity => Included.Length;
        public ulong CapturedOccupancyEpoch;
        public readonly bool[] Included;
        public readonly bool[] SpecialScanMember;
        public readonly int[] SpecialSlots;
        public int SpecialSlotCount;
        public bool SpecialIndexReady;
        public readonly int[] GroundRoleSlotsByX;
        public int GroundRoleSlotCount;
        public readonly int[] AirRoleSlotsByX;
        public int AirRoleSlotCount;
        public readonly AiSensingRoleTeamSummary[] GroundRoleTeamSummaries;
        public int GroundRoleTeamSummaryCount;
        public readonly AiSensingRoleTeamSummary[] AirRoleTeamSummaries;
        public int AirRoleTeamSummaryCount;
        public bool RoleIndexesReady;
        public readonly AiSensingTeamSummary[] TeamSummaries;
        public int TeamSummaryCount;
        public bool TeamSummariesReady;
        public readonly bool[] InputHistoryGate;
        public readonly uint[] Generation;
        public readonly int[] Identity;
        public readonly int[] ObjectId;
        public readonly int[] DataObjectType;
        public readonly int[] X;
        public readonly int[] Y;
        public readonly int[] Z;
        public readonly int[] Hp;
        public readonly int[] Hp3;
        public readonly int[] HpMax;
        public readonly int[] Pp;
        public readonly int[] Team;
        public readonly int[] State;
        public readonly int[] Frame;
        public readonly int[] HitJ;
        public readonly int[] LinkState;
        public readonly int[] KillCount;
        public readonly int[] CachedTargetSlot;
        public readonly int[] CoordinateTargetX;
        public readonly double[] Vx;
        public readonly int[] Facing;
        public readonly int[] TargetSlot;
        public readonly int[] HitStop;
        public readonly int[] BoundaryFlags;

        public void Reset(ulong occupancyEpoch)
        {
            CapturedOccupancyEpoch = occupancyEpoch;
            Array.Clear(Included, 0, Capacity);
            Array.Clear(SpecialScanMember, 0, Capacity);
            Array.Clear(SpecialSlots, 0, Capacity);
            SpecialSlotCount = 0;
            SpecialIndexReady = false;
            GroundRoleSlotCount = 0;
            AirRoleSlotCount = 0;
            GroundRoleTeamSummaryCount = 0;
            AirRoleTeamSummaryCount = 0;
            RoleIndexesReady = false;
            TeamSummaryCount = 0;
            TeamSummariesReady = false;
        }

        protected void CopyTo(AiSensingSnapshot grown)
        {
            int count = Capacity;
            grown.CapturedOccupancyEpoch = CapturedOccupancyEpoch;
            Array.Copy(Included, grown.Included, count);
            Array.Copy(SpecialScanMember, grown.SpecialScanMember, count);
            Array.Copy(SpecialSlots, grown.SpecialSlots, SpecialSlotCount);
            grown.SpecialSlotCount = SpecialSlotCount;
            grown.SpecialIndexReady = SpecialIndexReady;
            Array.Copy(GroundRoleSlotsByX, grown.GroundRoleSlotsByX, GroundRoleSlotCount);
            grown.GroundRoleSlotCount = GroundRoleSlotCount;
            Array.Copy(AirRoleSlotsByX, grown.AirRoleSlotsByX, AirRoleSlotCount);
            grown.AirRoleSlotCount = AirRoleSlotCount;
            Array.Copy(GroundRoleTeamSummaries, grown.GroundRoleTeamSummaries, GroundRoleTeamSummaryCount);
            grown.GroundRoleTeamSummaryCount = GroundRoleTeamSummaryCount;
            Array.Copy(AirRoleTeamSummaries, grown.AirRoleTeamSummaries, AirRoleTeamSummaryCount);
            grown.AirRoleTeamSummaryCount = AirRoleTeamSummaryCount;
            grown.RoleIndexesReady = RoleIndexesReady;
            Array.Copy(TeamSummaries, grown.TeamSummaries, TeamSummaryCount);
            grown.TeamSummaryCount = TeamSummaryCount;
            grown.TeamSummariesReady = TeamSummariesReady;
            Array.Copy(InputHistoryGate, grown.InputHistoryGate, count);
            Array.Copy(Generation, grown.Generation, count);
            Array.Copy(Identity, grown.Identity, count);
            Array.Copy(ObjectId, grown.ObjectId, count);
            Array.Copy(DataObjectType, grown.DataObjectType, count);
            Array.Copy(X, grown.X, count);
            Array.Copy(Y, grown.Y, count);
            Array.Copy(Z, grown.Z, count);
            Array.Copy(Hp, grown.Hp, count);
            Array.Copy(Hp3, grown.Hp3, count);
            Array.Copy(HpMax, grown.HpMax, count);
            Array.Copy(Pp, grown.Pp, count);
            Array.Copy(Team, grown.Team, count);
            Array.Copy(State, grown.State, count);
            Array.Copy(Frame, grown.Frame, count);
            Array.Copy(HitJ, grown.HitJ, count);
            Array.Copy(LinkState, grown.LinkState, count);
            Array.Copy(KillCount, grown.KillCount, count);
            Array.Copy(CachedTargetSlot, grown.CachedTargetSlot, count);
            Array.Copy(CoordinateTargetX, grown.CoordinateTargetX, count);
            Array.Copy(Vx, grown.Vx, count);
            Array.Copy(Facing, grown.Facing, count);
            Array.Copy(TargetSlot, grown.TargetSlot, count);
            Array.Copy(HitStop, grown.HitStop, count);
            Array.Copy(BoundaryFlags, grown.BoundaryFlags, count);
        }
    }
}
