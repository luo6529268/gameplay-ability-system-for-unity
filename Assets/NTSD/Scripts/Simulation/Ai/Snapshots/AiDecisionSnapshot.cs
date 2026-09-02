using System;

namespace NTSD.Simulation
{
    public enum AiDecisionAvailability
    {
        None = 0,
        Available = 1,
        SnapshotMissing = 2,
        SelfSlotInvalid = 3,
        SelfNotIncluded = 4,
        GenerationMismatch = 5,
        StableIdMismatch = 6,
        EpochMismatch = 7,
        IndexesNotReady = 8,
    }

    public enum AiDecisionEvaluationPolicy
    {
        FullScan = 0,
        Indexed = 1,
    }

    public enum AiDecisionOwnedInputMode
    {
        SnapshotCopy = 0,
        CanonicalStoreDirect = 1,
    }

    public enum AiDecisionExit
    {
        None = 0,
        InvalidSelf = 1,
        Coordinate = 2,
        NoTarget = 3,
        TargetMissing = 4,
        TargetState3000 = 5,
        HeldSpecial = 6,
        SpecialTarget = 7,
        AirTarget = 8,
        C8Target = 9,
        FirstDecision = 10,
        PredictedDecision = 11,
        HeldDecision = 12,
        Complete = 13,
    }

    public struct AiDecisionInputState
    {
        public int History0;
        public int History1;
        public int History2;
        public int History3;
        public int History4;
        public int History5;

        public byte CdAttack;
        public byte CdJump;
        public byte CdDefend;
        public byte CdDefendLock;
        public byte CdRight;
        public byte CdLeft;
        public byte CdUp;
        public byte CdDown;

        public byte ComboDra;
        public byte ComboDla;
        public byte ComboDua;
        public byte ComboDda;
        public byte ComboDrj;
        public byte ComboDlj;
        public byte ComboDuj;
        public byte ComboDdj;
        public byte ComboDja;

        public byte PrevUp;
        public byte PrevDown;
        public byte PrevLeft;
        public byte PrevRight;
        public byte PrevJump;
        public byte PrevDefend;
        public byte PrevAttack;

        public byte KeyUp;
        public byte KeyDown;
        public byte KeyLeft;
        public byte KeyRight;
        public byte KeyAttack;
        public byte KeyJump;
        public byte KeyDefend;

        public int Unk360;
        public int Unk3FC;
        public int Unk400;
        public int BoundaryFlags;

        public bool HasInputHistoryGate => History0 != 0;
        public bool HasBoundaryBlock => BoundaryFlags != 0;
    }

    public struct AiDecisionWorldState
    {
        public int Difficulty;
        public int AiPhaseGate;
        public int InputPhase;
        public int StageTargetX;
        public int StageZMin;
        public int StageZMax;

        public int FlowAiDifficulty;
        public int FlowRand3;
        public int FlowRand5;
        public int FlowRand15;
        public int FlowRand20;
        public int FlowMoveMode;
        public int FlowStageTargetX;
    }

    public sealed class AiDecisionSnapshot
    {
        public AiDecisionSnapshot(int capacity)
            : this(new AiSensingSnapshot(capacity))
        {
        }

        public AiDecisionSnapshot(AiSensingSnapshot rows)
        {
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));
            if (rows.Capacity < 1)
                throw new ArgumentOutOfRangeException(nameof(rows));
            Rows = rows;
            CharacterDecisionModule = new AiCharacterDecisionModule();
            RngTraceModuli = new int[256];
            RngTraceRaw = new int[256];
            RngTraceValues = new int[256];
        }

        public AiSensingSnapshot Rows { get; }
        internal AiCharacterDecisionModule CharacterDecisionModule { get; }
        public int SelfSlot;
        public uint SelfGeneration;
        public int SelfStableId;
        public ulong OccupancyEpoch;
        public AiDecisionInputState Input;
        public AiDecisionWorldState World;
        public uint RngState;
        public ulong RngCalls;
        public readonly int[] RngTraceModuli;
        public readonly int[] RngTraceRaw;
        public readonly int[] RngTraceValues;
        public int RngTraceCount;
        public bool RngTraceOverflow;

        public void Reset(ulong occupancyEpoch)
        {
            ResetSharedRows(occupancyEpoch);
            ResetOwned(occupancyEpoch);
        }

        public void ResetSharedRows(ulong occupancyEpoch)
        {
            Rows.Reset(occupancyEpoch);
        }

        public void ResetOwned(ulong occupancyEpoch)
        {
            SelfSlot = -1;
            SelfGeneration = 0;
            SelfStableId = 0;
            OccupancyEpoch = occupancyEpoch;
            Input = default;
            World = default;
            RngState = 0;
            RngCalls = 0;
            RngTraceCount = 0;
            RngTraceOverflow = false;
        }

        public void CopyOwnedFrom(AiDecisionSnapshot source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            SelfSlot = source.SelfSlot;
            SelfGeneration = source.SelfGeneration;
            SelfStableId = source.SelfStableId;
            OccupancyEpoch = source.OccupancyEpoch;
            Input = source.Input;
            World = source.World;
            RngState = source.RngState;
            RngCalls = source.RngCalls;
            RngTraceCount = 0;
            RngTraceOverflow = false;
        }
    }

    public struct AiDecisionWitness
    {
        public AiDecisionAvailability Availability;
        public AiDecisionExit Exit;
        public int SelfSlot;
        public uint SelfGeneration;
        public int SelfStableId;
        public ulong OccupancyEpoch;
        public AiDecisionInputState Input;
        public AiDecisionWorldState World;
        public int InitialSelectedSlot;
        public int CachedSelectedSlot;
        public int FinalSelectedSlot;
        public int InitialBestDistance;
        public int SpecialBestDistance;
        public int SpecialFlags;
        public int SelectedTargetHitStop;
        public int CharacterDecisionPosition;
        public uint RngState;
        public ulong RngCalls;
        public ulong RngOrderHash;
        public int RngDrawCount;
        public bool RngTraceOverflow;
        public int RowVisits;
    }
}
