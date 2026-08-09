using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public enum AiDecisionShadowMode
    {
        Disabled = 0,
        Shadow = 1,
        SharedShadow = 2,
    }

    public enum AiDecisionExecutionMode
    {
        Legacy = 0,
        IndexedCanonical = 1,
    }

    public enum AiUnifiedSnapshotShadowMode
    {
        Disabled = 0,
        Shadow = 1,
    }

    public enum AiUnifiedSnapshotExecutionMode
    {
        LegacySeparate = 0,
        UnifiedAuthority = 1,
    }

    public enum AiUnifiedSnapshotConsumer
    {
        None = 0,
        SoASensing = 1,
        IndexedDecision = 2,
    }

    public enum AiUnifiedSnapshotMismatchKind
    {
        None = 0,
        Epoch = 1,
        Capacity = 2,
        Included = 3,
        SpecialMembership = 4,
        Generation = 5,
        Identity = 6,
        Field = 7,
        BoundaryFlags = 8,
        IndexReadiness = 9,
        IndexCount = 10,
        IndexEntry = 11,
        SummaryEntry = 12,
        FallbackReference = 13,
        MoveModeProduct = 14,
        MutationWitness = 15,
    }

    public enum AiUnifiedSnapshotField
    {
        None = 0,
        InputHistoryGate = 1,
        ObjectId = 2,
        DataObjectType = 3,
        X = 4,
        Y = 5,
        Z = 6,
        Hp = 7,
        Hp3 = 8,
        HpMax = 9,
        Pp = 10,
        Team = 11,
        State = 12,
        Frame = 13,
        LinkState = 14,
        KillCount = 15,
        CachedTargetSlot = 16,
        CoordinateTargetX = 17,
        VxBits = 18,
        Facing = 19,
        TargetSlot = 20,
        HitStop = 21,
        SpecialIndex = 22,
        GroundRoleIndex = 23,
        AirRoleIndex = 24,
        GroundRoleTeamSummary = 25,
        AirRoleTeamSummary = 26,
        TeamSummary = 27,
        FallbackSlot = 28,
        MoveModePresent = 29,
        MoveModeEligible = 30,
        MoveModeGeneration = 31,
        MoveModeHp = 32,
        MoveModeX = 33,
        MoveModeZ = 34,
        MoveModeTopSlot = 35,
        MoveModeTopX = 36,
        MoveModeTopZ = 37,
        MoveModeSecondSlot = 38,
        MoveModeSecondX = 39,
        MoveModeSecondZ = 40,
        MoveModeValid = 41,
        WitnessEpoch = 42,
        WitnessOrdinal = 43,
        WitnessSlot = 44,
        WitnessGeneration = 45,
        WitnessStableId = 46,
        WitnessRoleRebuilt = 47,
        WitnessTeamRebuilt = 48,
        WitnessRoleVersion = 49,
        WitnessTeamVersion = 50,
        WitnessOldX = 51,
        WitnessNewX = 52,
        WitnessOldTeam = 53,
        WitnessNewTeam = 54,
        WitnessOldRoleFlags = 55,
        WitnessNewRoleFlags = 56,
        WitnessOldLiving = 57,
        WitnessNewLiving = 58,
        WitnessOldHp = 59,
        WitnessNewHp = 60,
    }

    public enum AiUnifiedSnapshotProductMutationKind
    {
        None = 0,
        FallbackReference = 1,
        MoveModeFirst10Hp = 2,
    }

    public enum AiUnifiedSnapshotExceptionStage
    {
        None = 0,
        Prepare = 1,
        Capture = 2,
        BuildIndexes = 3,
        Validate = 4,
        InitialSensingCompare = 5,
        InitialDecisionCompare = 6,
        Refresh = 7,
        RefreshCapture = 8,
        RefreshBuildIndexes = 9,
        RefreshCompare = 10,
    }

    public struct AiUnifiedSnapshotMismatch
    {
        public AiUnifiedSnapshotConsumer Consumer;
        public AiUnifiedSnapshotMismatchKind Kind;
        public AiUnifiedSnapshotField Field;
        public int Slot;
        public long ExpectedValue;
        public long ActualValue;
    }

    public enum AiDecisionShadowMismatchReason
    {
        None = 0,
        SnapshotUnavailable = 1,
        IdentityChanged = 2,
        Input = 3,
        WorldFlow = 4,
        RngState = 5,
        RngCalls = 6,
        RngOrder = 7,
        RngTraceOverflow = 8,
    }

    public enum AiDecisionShadowExceptionStage
    {
        None = 0,
        SharedBuild = 1,
        SharedPreflight = 2,
        KernelEvaluate = 3,
        SharedRefresh = 4,
    }

    public enum AiDecisionIndexedMismatchReason
    {
        None = 0,
        Availability = 1,
        Exit = 2,
        Input = 3,
        World = 4,
        Target = 5,
        BestDistance = 6,
        Flags = 7,
        HitStop = 8,
        RngState = 9,
        RngCalls = 10,
        RngOrder = 11,
        RngDrawCount = 12,
        RngTraceOverflow = 13,
        RngTrace = 14,
    }

    public partial class SimulationWorld
    {
        private struct AiUnifiedSnapshotMutationWitness
        {
            public ulong Epoch;
            public long Ordinal;
            public int Slot;
            public uint Generation;
            public int StableId;
            public bool RoleRebuilt;
            public bool TeamRebuilt;
            public long RoleVersion;
            public long TeamVersion;
            public int OldX;
            public int NewX;
            public int OldTeam;
            public int NewTeam;
            public int OldRoleFlags;
            public int NewRoleFlags;
            public bool OldLiving;
            public bool NewLiving;
            public int OldHp;
            public int NewHp;
        }

        private sealed class AiUnifiedSnapshotExecutionState
        {
            internal AiUnifiedSnapshotExecutionState(int capacity)
            {
                Rows = new AiSoASensingRows(capacity);
                SharedSnapshot = new AiDecisionSnapshot(Rows);
                IndexedSnapshot = new AiDecisionSnapshot(Rows);
                SoASensingBoundaryFlags = new int[capacity];
                DecisionBoundaryFlags = new int[capacity];
                FallbackSlots = new LF2Entity[capacity];
                MoveModeFirst10Present = new bool[10];
                MoveModeFirst10Eligible = new bool[10];
                MoveModeFirst10Generation = new uint[10];
                MoveModeFirst10Hp = new int[10];
                MoveModeFirst10X = new int[10];
                MoveModeFirst10Z = new int[10];
            }

            internal readonly AiSoASensingRows Rows;
            internal readonly AiDecisionSnapshot SharedSnapshot;
            internal readonly AiDecisionSnapshot IndexedSnapshot;
            internal readonly int[] SoASensingBoundaryFlags;
            internal readonly int[] DecisionBoundaryFlags;
            internal readonly LF2Entity[] FallbackSlots;
            internal readonly bool[] MoveModeFirst10Present;
            internal readonly bool[] MoveModeFirst10Eligible;
            internal readonly uint[] MoveModeFirst10Generation;
            internal readonly int[] MoveModeFirst10Hp;
            internal readonly int[] MoveModeFirst10X;
            internal readonly int[] MoveModeFirst10Z;
            internal ulong Epoch;
            internal int ExpectedCapacity;
            internal int MoveModeTopSlot;
            internal int MoveModeTopX;
            internal int MoveModeTopZ;
            internal int MoveModeSecondSlot;
            internal int MoveModeSecondX;
            internal int MoveModeSecondZ;
            internal bool MoveModeFirst10Valid;

            internal int Capacity => Rows.Capacity;

            internal void Reset(ulong epoch, int expectedCapacity)
            {
                Epoch = epoch;
                ExpectedCapacity = expectedCapacity;
                Rows.Reset(epoch);
                Array.Clear(SoASensingBoundaryFlags, 0, Capacity);
                Array.Clear(DecisionBoundaryFlags, 0, Capacity);
                Array.Clear(FallbackSlots, 0, Capacity);
                Array.Clear(MoveModeFirst10Present, 0, 10);
                Array.Clear(MoveModeFirst10Eligible, 0, 10);
                Array.Clear(MoveModeFirst10Generation, 0, 10);
                Array.Clear(MoveModeFirst10Hp, 0, 10);
                Array.Clear(MoveModeFirst10X, 0, 10);
                Array.Clear(MoveModeFirst10Z, 0, 10);
                MoveModeTopSlot = -1;
                MoveModeTopX = -1;
                MoveModeTopZ = 0;
                MoveModeSecondSlot = -1;
                MoveModeSecondX = -1;
                MoveModeSecondZ = 0;
                MoveModeFirst10Valid = false;
            }
        }

        private const ulong AiDecisionRngHashOffset = 1469598103934665603UL;
        private const ulong AiDecisionRngHashPrime = 1099511628211UL;

        private AiDecisionShadowMode aiDecisionShadowMode;
        private AiDecisionExecutionMode aiDecisionExecutionMode;
        private AiUnifiedSnapshotShadowMode aiUnifiedSnapshotShadowMode;
        private AiUnifiedSnapshotExecutionMode aiUnifiedSnapshotExecutionMode;
        private int aiDecisionIndexedCanonicalFullOracleSampleInterval;
        private AiDecisionSnapshot aiDecisionShadowSnapshot;
        private AiDecisionSnapshot aiDecisionSharedSnapshot;
        private AiDecisionSnapshot aiDecisionIndexedSnapshot;
        private AiSoASensingRows aiDecisionSharedRows;
        private AiDecisionSnapshot aiDecisionComparisonSnapshot;
        private ulong aiDecisionSharedPassEpoch;
        private bool aiDecisionSharedPassAvailable;
        private AiDecisionAvailability aiDecisionSharedPassUnavailableReason;
        private AiDecisionWitness aiDecisionShadowExpected;
        private LF2Entity aiDecisionShadowSelf;
        private bool aiDecisionShadowComparisonActive;
        private bool aiDecisionLegacyRngRecording;
        private readonly int[] aiDecisionLegacyRngModuli = new int[256];
        private readonly int[] aiDecisionLegacyRngRaw = new int[256];
        private readonly int[] aiDecisionLegacyRngValues = new int[256];
        private int aiDecisionLegacyRngCount;
        private bool aiDecisionLegacyRngOverflow;
        private ulong aiDecisionLegacyRngOrderHash = AiDecisionRngHashOffset;
        private Type aiDecisionShadowFirstExceptionType;
        private AiSoASensingRows aiUnifiedSnapshotRows;
        private int[] aiUnifiedSnapshotSoASensingBoundaryFlags;
        private int[] aiUnifiedSnapshotDecisionBoundaryFlags;
        private LF2Entity[] aiUnifiedSnapshotFallbackSlots;
        private bool[] aiUnifiedMoveModeFirst10Present = new bool[10];
        private bool[] aiUnifiedMoveModeFirst10Eligible = new bool[10];
        private uint[] aiUnifiedMoveModeFirst10Generation = new uint[10];
        private int[] aiUnifiedMoveModeFirst10Hp = new int[10];
        private int[] aiUnifiedMoveModeFirst10X = new int[10];
        private int[] aiUnifiedMoveModeFirst10Z = new int[10];
        private int aiUnifiedMoveModeTopSlot = -1;
        private int aiUnifiedMoveModeTopX = -1;
        private int aiUnifiedMoveModeTopZ;
        private int aiUnifiedMoveModeSecondSlot = -1;
        private int aiUnifiedMoveModeSecondX = -1;
        private int aiUnifiedMoveModeSecondZ;
        private bool aiUnifiedMoveModeFirst10Valid;
        private AiUnifiedSnapshotExecutionState aiUnifiedSnapshotPublishedState;
        private AiUnifiedSnapshotExecutionState aiUnifiedSnapshotScratchState;
        private AiSoASensingRows aiUnifiedSnapshotLegacySoARows;
        private AiSoASensingRows aiUnifiedSnapshotLegacyDecisionRows;
        private AiDecisionSnapshot aiUnifiedSnapshotLegacySharedSnapshot;
        private AiDecisionSnapshot aiUnifiedSnapshotLegacyIndexedSnapshot;
        private LF2Entity[] aiUnifiedSnapshotLegacyInputSlots;
        private bool[] aiUnifiedSnapshotLegacyMoveModeFirst10Present;
        private bool[] aiUnifiedSnapshotLegacyMoveModeFirst10Eligible;
        private uint[] aiUnifiedSnapshotLegacyMoveModeFirst10Generation;
        private int[] aiUnifiedSnapshotLegacyMoveModeFirst10Hp;
        private int[] aiUnifiedSnapshotLegacyMoveModeFirst10X;
        private int[] aiUnifiedSnapshotLegacyMoveModeFirst10Z;
        private bool aiUnifiedSnapshotExecutionCommittedThisPass;
        private bool aiUnifiedSnapshotExecutionConsumerStartedThisPass;
        private ulong aiUnifiedSnapshotPassEpoch;
        private bool aiUnifiedSnapshotPassAvailable;
        private bool aiUnifiedSnapshotPassFailureRecorded;
        private bool aiUnifiedSnapshotProductsComparedThisPass;
        private bool aiUnifiedSnapshotRefreshComparisonActive;
        private long aiSoASensingMutationWitnessOrdinal;
        private long aiSoASensingRoleIndexVersion;
        private long aiSoASensingTeamSummaryVersion;
        private AiUnifiedSnapshotMutationWitness aiSoASensingMutationWitness;
        private long aiDecisionMutationWitnessOrdinal;
        private long aiDecisionRoleIndexVersion;
        private long aiDecisionTeamSummaryVersion;
        private AiUnifiedSnapshotMutationWitness aiDecisionMutationWitness;
        private long aiUnifiedSnapshotMutationWitnessOrdinal;
        private long aiUnifiedSnapshotRoleIndexVersion;
        private long aiUnifiedSnapshotTeamSummaryVersion;
        private AiUnifiedSnapshotMutationWitness aiUnifiedSnapshotMutationWitness;
        private Type aiUnifiedSnapshotFirstExceptionType;
#if UNITY_INCLUDE_TESTS
        private long aiDecisionShadowBeginInvocationCountForTests;
        private long aiDecisionShadowCompleteInvocationCountForTests;
        private int aiDecisionSharedPreflightMutationKindForSelfCheck = -1;
        private int aiDecisionSharedPreflightMutationSlotForSelfCheck = -1;
        private int aiDecisionSharedPostLegacyMutationSlotForSelfCheck = -1;
        private int aiDecisionSharedPostLegacyMutationStateForSelfCheck;
        private AiDecisionShadowExceptionStage aiDecisionShadowExceptionStageForSelfCheck;
        private AiDecisionAvailability aiDecisionIndexedCanonicalPreCommitFailureForSelfCheck;
        private AiUnifiedSnapshotConsumer aiUnifiedSnapshotBoundaryMutationConsumerForSelfCheck;
        private int aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck = -1;
        private int aiUnifiedSnapshotBoundaryMutationXorForSelfCheck;
        private AiUnifiedSnapshotExceptionStage aiUnifiedSnapshotExceptionStageForSelfCheck;
        private AiUnifiedSnapshotConsumer aiUnifiedSnapshotWitnessMutationConsumerForSelfCheck;
        private AiUnifiedSnapshotProductMutationKind aiUnifiedSnapshotProductMutationKindForSelfCheck;
        private int aiUnifiedSnapshotProductMutationSlotForSelfCheck = -1;
        private int aiUnifiedSnapshotExecutionProbeObserverSlotAForSelfCheck = -1;
        private int aiUnifiedSnapshotExecutionProbeTargetSlotAForSelfCheck = -1;
        private int aiUnifiedSnapshotExecutionProbeStateAForSelfCheck = int.MinValue;
        private int aiUnifiedSnapshotExecutionProbeObserverSlotBForSelfCheck = -1;
        private int aiUnifiedSnapshotExecutionProbeTargetSlotBForSelfCheck = -1;
        private int aiUnifiedSnapshotExecutionProbeStateBForSelfCheck = int.MinValue;
#endif

        public AiDecisionShadowMode AiDecisionShadowMode
        {
            get => aiDecisionShadowMode;
            set
            {
                if (_ticking)
                    throw new InvalidOperationException(
                        "AI decision shadow mode cannot change while a simulation pass is running.");
                if (value != AiDecisionShadowMode.Disabled &&
                    value != AiDecisionShadowMode.Shadow &&
                    value != AiDecisionShadowMode.SharedShadow)
                    throw new ArgumentOutOfRangeException(nameof(value));
                aiDecisionShadowMode = value;
            }
        }

        public AiDecisionExecutionMode AiDecisionExecutionMode
        {
            get => aiDecisionExecutionMode;
            set
            {
                if (_ticking)
                    throw new InvalidOperationException(
                        "AI decision execution mode cannot change while a simulation pass is running.");
                if (value != AiDecisionExecutionMode.Legacy &&
                    value != AiDecisionExecutionMode.IndexedCanonical)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (aiUnifiedSnapshotExecutionMode ==
                        AiUnifiedSnapshotExecutionMode.UnifiedAuthority &&
                    value != AiDecisionExecutionMode.IndexedCanonical)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority requires IndexedCanonical.");
                }
                aiDecisionExecutionMode = value;
            }
        }

        public AiUnifiedSnapshotShadowMode AiUnifiedSnapshotShadowMode
        {
            get => aiUnifiedSnapshotShadowMode;
            set
            {
                if (_ticking)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot shadow mode cannot change while a simulation pass is running.");
                }
                if (value != AiUnifiedSnapshotShadowMode.Disabled &&
                    value != AiUnifiedSnapshotShadowMode.Shadow)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (value == AiUnifiedSnapshotShadowMode.Shadow &&
                    aiUnifiedSnapshotExecutionMode ==
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot shadow and authority execution are mutually exclusive.");
                }

                aiUnifiedSnapshotShadowMode = value;
                if (value == AiUnifiedSnapshotShadowMode.Disabled)
                    EndAiUnifiedSnapshotShadowPass();
            }
        }

        public AiUnifiedSnapshotExecutionMode AiUnifiedSnapshotExecutionMode
        {
            get => aiUnifiedSnapshotExecutionMode;
            set
            {
                if (_ticking)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot execution mode cannot change while a simulation pass is running.");
                }
                if (value != AiUnifiedSnapshotExecutionMode.LegacySeparate &&
                    value != AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (value == AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    if (aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Disabled)
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot shadow and authority execution are mutually exclusive.");
                    }
                    if (aiSensingMode != AiSensingMode.SoAAiSensing ||
                        aiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical)
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority requires SoAAiSensing and IndexedCanonical.");
                    }
                }

                if (aiUnifiedSnapshotExecutionMode == value)
                    return;
                aiUnifiedSnapshotExecutionMode = value;
                EndAiUnifiedSnapshotExecutionPass();
                if (value == AiUnifiedSnapshotExecutionMode.LegacySeparate)
                    RestoreAiUnifiedSnapshotLegacyConsumerBuffers();
            }
        }

        public int AiDecisionIndexedCanonicalFullOracleSampleInterval
        {
            get => aiDecisionIndexedCanonicalFullOracleSampleInterval;
            set
            {
                if (_ticking)
                    throw new InvalidOperationException(
                        "AI decision oracle sampling cannot change while a simulation pass is running.");
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                aiDecisionIndexedCanonicalFullOracleSampleInterval = value;
            }
        }

        private bool AiDecisionRequiresSharedRows =>
            aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow ||
            aiDecisionExecutionMode == AiDecisionExecutionMode.IndexedCanonical;

        public long AiDecisionShadowEligibleCountForDiagnostics { get; private set; }
        public long AiDecisionShadowAvailableCountForDiagnostics { get; private set; }
        public long AiDecisionShadowUnavailableCountForDiagnostics { get; private set; }
        public long AiDecisionShadowComparedCountForDiagnostics { get; private set; }
        public long AiDecisionShadowMismatchCountForDiagnostics { get; private set; }
        public long AiDecisionShadowCloneRngCallCountForDiagnostics { get; private set; }
        public long AiDecisionShadowRowVisitCountForDiagnostics { get; private set; }
        public long AiDecisionSharedBuildCountForDiagnostics { get; private set; }
        public long AiDecisionSharedRefreshCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedEligibleCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedAvailableCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedUnavailableCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedComparedCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedMismatchCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedFullRowVisitCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedRowVisitCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedCanonicalEligibleCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedCanonicalCommittedCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedCanonicalFallbackCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics { get; private set; }
        public long AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics { get; private set; }
        public AiDecisionAvailability AiDecisionShadowFirstUnavailableReasonForDiagnostics { get; private set; }
        public AiDecisionShadowMismatchReason AiDecisionShadowFirstMismatchReasonForDiagnostics { get; private set; }
        public AiDecisionShadowExceptionStage AiDecisionShadowFirstExceptionStageForDiagnostics { get; private set; }
        public AiDecisionIndexedMismatchReason AiDecisionIndexedFirstMismatchReasonForDiagnostics { get; private set; }
        public AiDecisionAvailability AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics { get; private set; }
        public AiDecisionIndexedMismatchReason AiDecisionIndexedCanonicalFirstOracleMismatchReasonForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowBuildCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowRefreshCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowSensingComparedCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowDecisionComparedCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowUnavailableCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowMismatchCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowDerivedComparisonEntryVisitCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionBuildCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionSlotVisitCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionRefreshCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionReadCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics { get; private set; }
        public long AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics { get; private set; }
        public AiUnifiedSnapshotMismatch AiUnifiedSnapshotShadowFirstMismatchForDiagnostics { get; private set; }
        public AiUnifiedSnapshotExceptionStage AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics { get; private set; }
        public Type AiUnifiedSnapshotShadowFirstExceptionTypeForDiagnostics =>
            aiUnifiedSnapshotFirstExceptionType;
        public AiUnifiedSnapshotExceptionStage AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics { get; private set; }
        public Type AiUnifiedSnapshotExecutionFirstFailureTypeForDiagnostics { get; private set; }
        public bool AiUnifiedSnapshotShadowRowsAllocatedForDiagnostics =>
            aiUnifiedSnapshotRows != null;
        public Type AiDecisionShadowFirstExceptionTypeForDiagnostics =>
            aiDecisionShadowFirstExceptionType;
        public AiDecisionWitness AiDecisionShadowLastExpectedForDiagnostics => aiDecisionShadowExpected;
#if UNITY_INCLUDE_TESTS
        public long AiDecisionShadowBeginInvocationCountForTests =>
            aiDecisionShadowBeginInvocationCountForTests;
        public long AiDecisionShadowCompleteInvocationCountForTests =>
            aiDecisionShadowCompleteInvocationCountForTests;
        public bool AiDecisionShadowComparisonActiveForTests => aiDecisionShadowComparisonActive;
        public bool AiDecisionLegacyRngRecordingForTests => aiDecisionLegacyRngRecording;
        public int AiDecisionLegacyRngCountForTests => aiDecisionLegacyRngCount;
        public bool AiDecisionSharedPassAvailableForTests => aiDecisionSharedPassAvailable;
        public ulong AiUnifiedSnapshotExecutionPublishedEpochForTests =>
            aiUnifiedSnapshotPublishedState?.Epoch ?? 0;
        public bool AiUnifiedSnapshotExecutionPublishedEpochIsCurrentForTests =>
            aiUnifiedSnapshotPublishedState != null &&
            aiUnifiedSnapshotPublishedState.Epoch == RuntimeSlotOccupancyEpochForServices;
        public int AiUnifiedSnapshotExecutionPublishedCapacityForTests =>
            aiUnifiedSnapshotPublishedState?.Capacity ?? 0;
        public int AiUnifiedSnapshotExecutionPublishedSpecialSlotCountForTests =>
            aiUnifiedSnapshotPublishedState?.Rows.SpecialSlotCount ?? 0;
        public int AiUnifiedSnapshotExecutionPublishedGroundRoleCountForTests =>
            aiUnifiedSnapshotPublishedState?.Rows.GroundRoleSlotCount ?? 0;
        public int AiUnifiedSnapshotExecutionPublishedTeamSummaryCountForTests =>
            aiUnifiedSnapshotPublishedState?.Rows.TeamSummaryCount ?? 0;
        public bool AiUnifiedSnapshotExecutionPublishedFirst10ValidForTests =>
            aiUnifiedSnapshotPublishedState?.MoveModeFirst10Valid == true;
        public int AiUnifiedSnapshotExecutionProbeStateAForTests =>
            aiUnifiedSnapshotExecutionProbeStateAForSelfCheck;
        public int AiUnifiedSnapshotExecutionProbeStateBForTests =>
            aiUnifiedSnapshotExecutionProbeStateBForSelfCheck;
#endif

        public void ResetAiDecisionShadowDiagnostics()
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "AI decision shadow diagnostics cannot be reset while a simulation pass is running.");
            AiDecisionShadowEligibleCountForDiagnostics = 0;
            AiDecisionShadowAvailableCountForDiagnostics = 0;
            AiDecisionShadowUnavailableCountForDiagnostics = 0;
            AiDecisionShadowComparedCountForDiagnostics = 0;
            AiDecisionShadowMismatchCountForDiagnostics = 0;
            AiDecisionShadowCloneRngCallCountForDiagnostics = 0;
            AiDecisionShadowRowVisitCountForDiagnostics = 0;
            AiDecisionSharedBuildCountForDiagnostics = 0;
            AiDecisionSharedRefreshCountForDiagnostics = 0;
            AiDecisionIndexedEligibleCountForDiagnostics = 0;
            AiDecisionIndexedAvailableCountForDiagnostics = 0;
            AiDecisionIndexedUnavailableCountForDiagnostics = 0;
            AiDecisionIndexedComparedCountForDiagnostics = 0;
            AiDecisionIndexedMismatchCountForDiagnostics = 0;
            AiDecisionIndexedFullRowVisitCountForDiagnostics = 0;
            AiDecisionIndexedRowVisitCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalEligibleCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalCommittedCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalFallbackCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics = 0;
            AiDecisionShadowFirstUnavailableReasonForDiagnostics = AiDecisionAvailability.None;
            AiDecisionShadowFirstMismatchReasonForDiagnostics = AiDecisionShadowMismatchReason.None;
            AiDecisionShadowFirstExceptionStageForDiagnostics =
                AiDecisionShadowExceptionStage.None;
            AiDecisionIndexedFirstMismatchReasonForDiagnostics =
                AiDecisionIndexedMismatchReason.None;
            AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics =
                AiDecisionAvailability.None;
            AiDecisionIndexedCanonicalFirstOracleMismatchReasonForDiagnostics =
                AiDecisionIndexedMismatchReason.None;
            aiDecisionShadowFirstExceptionType = null;
            aiDecisionShadowExpected = default;
            aiDecisionSharedPassAvailable = false;
            aiDecisionSharedPassUnavailableReason = AiDecisionAvailability.None;
            aiDecisionComparisonSnapshot = null;
#if UNITY_INCLUDE_TESTS
            aiDecisionShadowBeginInvocationCountForTests = 0;
            aiDecisionShadowCompleteInvocationCountForTests = 0;
#endif
        }

        public void ResetAiUnifiedSnapshotShadowDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Unified AI snapshot diagnostics cannot be reset while a simulation pass is running.");
            }

            AiUnifiedSnapshotShadowBuildCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowRefreshCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowSensingComparedCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowDecisionComparedCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowUnavailableCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowMismatchCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowDerivedComparisonEntryVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowFirstMismatchForDiagnostics = default;
            AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics =
                AiUnifiedSnapshotExceptionStage.None;
            aiUnifiedSnapshotFirstExceptionType = null;
        }

        public void ResetAiUnifiedSnapshotExecutionDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Unified AI snapshot execution diagnostics cannot be reset while a simulation pass is running.");
            }

            AiUnifiedSnapshotExecutionBuildCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionRefreshCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionReadCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics =
                AiUnifiedSnapshotExceptionStage.None;
            AiUnifiedSnapshotExecutionFirstFailureTypeForDiagnostics = null;
        }

        private bool BeginAiDecisionShadowComparison(LF2Entity self, int tickIndex)
        {
#if UNITY_INCLUDE_TESTS
            aiDecisionShadowBeginInvocationCountForTests++;
#endif
            if (aiDecisionShadowMode != AiDecisionShadowMode.Shadow &&
                aiDecisionShadowMode != AiDecisionShadowMode.SharedShadow)
                return false;

            AiDecisionShadowEligibleCountForDiagnostics++;
            aiDecisionShadowComparisonActive = false;
            aiDecisionLegacyRngRecording = false;
            aiDecisionShadowSelf = null;
            aiDecisionComparisonSnapshot = null;

            AiDecisionSnapshot snapshot;
            AiDecisionAvailability captureAvailability;
            if (aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow)
            {
                if (!aiDecisionSharedPassAvailable || aiDecisionSharedSnapshot == null)
                {
                    RecordAiDecisionShadowUnavailable(
                        aiDecisionSharedPassUnavailableReason == AiDecisionAvailability.None
                            ? AiDecisionAvailability.SnapshotMissing
                            : aiDecisionSharedPassUnavailableReason);
                    return false;
                }

                snapshot = aiDecisionSharedSnapshot;
                captureAvailability = CaptureAiDecisionSharedOwnedSnapshot(self, snapshot);
            }
            else
            {
                int capacity = RuntimeSlotCapacity;
                if (capacity <= 0)
                {
                    RecordAiDecisionShadowUnavailable(AiDecisionAvailability.SnapshotMissing);
                    return false;
                }
                if (aiDecisionShadowSnapshot == null ||
                    aiDecisionShadowSnapshot.Rows.Capacity != capacity)
                {
                    aiDecisionShadowSnapshot = new AiDecisionSnapshot(capacity);
                }

                snapshot = aiDecisionShadowSnapshot;
                captureAvailability = CaptureAiDecisionShadowSnapshot(self, snapshot);
            }
            if (captureAvailability != AiDecisionAvailability.Available)
            {
                RecordAiDecisionShadowUnavailable(captureAvailability);
                return false;
            }

            AiDecisionWitness expected = default;
            try
            {
#if UNITY_INCLUDE_TESTS
                ThrowAiDecisionShadowExceptionForSelfCheck(
                    AiDecisionShadowExceptionStage.KernelEvaluate);
#endif
                bool fullAvailable = AiDecisionKernel.TryEvaluate(
                    snapshot,
                    AiDecisionEvaluationPolicy.FullScan,
                    ref expected);
                if (aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow)
                {
                    EvaluateAiDecisionIndexedShadow(snapshot, expected, fullAvailable);
                }
                if (!fullAvailable)
                {
                    RecordAiDecisionShadowUnavailable(expected.Availability);
                    return false;
                }
            }
            catch (Exception exception)
            {
                RecordAiDecisionShadowException(
                    AiDecisionShadowExceptionStage.KernelEvaluate,
                    exception);
                if (aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow)
                {
                    InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
                }
                RecordAiDecisionShadowUnavailable(AiDecisionAvailability.SnapshotMissing);
                return false;
            }

            aiDecisionComparisonSnapshot = snapshot;
            aiDecisionShadowExpected = expected;
            aiDecisionShadowSelf = self;
            aiDecisionShadowComparisonActive = true;
            AiDecisionShadowAvailableCountForDiagnostics++;
            AiDecisionShadowCloneRngCallCountForDiagnostics += expected.RngDrawCount;
            AiDecisionShadowRowVisitCountForDiagnostics += expected.RowVisits;
            aiDecisionLegacyRngCount = 0;
            aiDecisionLegacyRngOverflow = false;
            aiDecisionLegacyRngOrderHash = AiDecisionRngHashOffset;
            aiDecisionLegacyRngRecording = true;
            return true;
        }

        private void CompleteAiDecisionShadowComparison(bool comparisonStarted)
        {
#if UNITY_INCLUDE_TESTS
            aiDecisionShadowCompleteInvocationCountForTests++;
#endif
            aiDecisionLegacyRngRecording = false;
            if (!comparisonStarted || !aiDecisionShadowComparisonActive)
                return;

            aiDecisionShadowComparisonActive = false;
            LF2Entity self = aiDecisionShadowSelf;
            aiDecisionShadowSelf = null;
            if (self?.Runtime == null ||
                RuntimeSlotOccupancyEpochForServices != aiDecisionShadowExpected.OccupancyEpoch ||
                !TryGetCurrentRuntimeHandle(
                    aiDecisionShadowExpected.SelfSlot,
                    self,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != aiDecisionShadowExpected.SelfGeneration ||
                self.Runtime.StableId != aiDecisionShadowExpected.SelfStableId)
            {
                aiDecisionComparisonSnapshot = null;
                RecordAiDecisionShadowUnavailable(AiDecisionAvailability.EpochMismatch);
                return;
            }

            AiDecisionShadowComparedCountForDiagnostics++;
            AiDecisionShadowMismatchReason reason = CompareAiDecisionShadowResult(self);
            aiDecisionComparisonSnapshot = null;
            if (reason == AiDecisionShadowMismatchReason.None)
                return;
            AiDecisionShadowMismatchCountForDiagnostics++;
            if (AiDecisionShadowFirstMismatchReasonForDiagnostics == AiDecisionShadowMismatchReason.None)
                AiDecisionShadowFirstMismatchReasonForDiagnostics = reason;
        }

        private void EvaluateAiDecisionIndexedShadow(
            AiDecisionSnapshot fullSnapshot,
            AiDecisionWitness fullWitness,
            bool fullAvailable)
        {
            AiDecisionIndexedEligibleCountForDiagnostics++;
            if (aiDecisionIndexedSnapshot == null ||
                !ReferenceEquals(aiDecisionIndexedSnapshot.Rows, fullSnapshot.Rows))
            {
                AiDecisionIndexedUnavailableCountForDiagnostics++;
                AiDecisionIndexedComparedCountForDiagnostics++;
                RecordAiDecisionIndexedMismatch(AiDecisionIndexedMismatchReason.Availability);
                return;
            }

            aiDecisionIndexedSnapshot.CopyOwnedFrom(fullSnapshot);
            AiDecisionWitness indexedWitness = default;
            bool indexedAvailable = AiDecisionKernel.TryEvaluate(
                aiDecisionIndexedSnapshot,
                AiDecisionEvaluationPolicy.Indexed,
                ref indexedWitness);
            if (indexedAvailable)
                AiDecisionIndexedAvailableCountForDiagnostics++;
            else
                AiDecisionIndexedUnavailableCountForDiagnostics++;

            AiDecisionIndexedComparedCountForDiagnostics++;
            AiDecisionIndexedFullRowVisitCountForDiagnostics += fullWitness.RowVisits;
            AiDecisionIndexedRowVisitCountForDiagnostics += indexedWitness.RowVisits;
            AiDecisionIndexedMismatchReason reason = CompareAiDecisionIndexedWitnesses(
                fullSnapshot,
                fullWitness,
                fullAvailable,
                aiDecisionIndexedSnapshot,
                indexedWitness,
                indexedAvailable);
            RecordAiDecisionIndexedMismatch(reason);
        }

        private void RecordAiDecisionIndexedMismatch(AiDecisionIndexedMismatchReason reason)
        {
            if (reason == AiDecisionIndexedMismatchReason.None)
                return;
            AiDecisionIndexedMismatchCountForDiagnostics++;
            if (AiDecisionIndexedFirstMismatchReasonForDiagnostics ==
                AiDecisionIndexedMismatchReason.None)
            {
                AiDecisionIndexedFirstMismatchReasonForDiagnostics = reason;
            }
        }

        private static AiDecisionIndexedMismatchReason CompareAiDecisionIndexedWitnesses(
            AiDecisionSnapshot fullSnapshot,
            AiDecisionWitness full,
            bool fullAvailable,
            AiDecisionSnapshot indexedSnapshot,
            AiDecisionWitness indexed,
            bool indexedAvailable)
        {
            if (fullAvailable != indexedAvailable || full.Availability != indexed.Availability)
                return AiDecisionIndexedMismatchReason.Availability;
            if (full.Exit != indexed.Exit)
                return AiDecisionIndexedMismatchReason.Exit;
            if (!AiDecisionInputEquals(full.Input, indexed.Input))
                return AiDecisionIndexedMismatchReason.Input;
            if (!AiDecisionWorldEquals(full.World, indexed.World))
                return AiDecisionIndexedMismatchReason.World;
            if (full.InitialSelectedSlot != indexed.InitialSelectedSlot ||
                full.CachedSelectedSlot != indexed.CachedSelectedSlot ||
                full.FinalSelectedSlot != indexed.FinalSelectedSlot)
            {
                return AiDecisionIndexedMismatchReason.Target;
            }
            if (full.InitialBestDistance != indexed.InitialBestDistance ||
                full.SpecialBestDistance != indexed.SpecialBestDistance)
            {
                return AiDecisionIndexedMismatchReason.BestDistance;
            }
            if (full.SpecialFlags != indexed.SpecialFlags)
                return AiDecisionIndexedMismatchReason.Flags;
            if (full.SelectedTargetHitStop != indexed.SelectedTargetHitStop)
                return AiDecisionIndexedMismatchReason.HitStop;
            if (full.RngState != indexed.RngState)
                return AiDecisionIndexedMismatchReason.RngState;
            if (full.RngCalls != indexed.RngCalls)
                return AiDecisionIndexedMismatchReason.RngCalls;
            if (full.RngOrderHash != indexed.RngOrderHash)
                return AiDecisionIndexedMismatchReason.RngOrder;
            if (full.RngDrawCount != indexed.RngDrawCount)
                return AiDecisionIndexedMismatchReason.RngDrawCount;
            if (full.RngTraceOverflow != indexed.RngTraceOverflow)
                return AiDecisionIndexedMismatchReason.RngTraceOverflow;
            int traceCount = Math.Min(full.RngDrawCount, fullSnapshot.RngTraceModuli.Length);
            for (int index = 0; index < traceCount; index++)
            {
                if (fullSnapshot.RngTraceModuli[index] != indexedSnapshot.RngTraceModuli[index] ||
                    fullSnapshot.RngTraceRaw[index] != indexedSnapshot.RngTraceRaw[index] ||
                    fullSnapshot.RngTraceValues[index] != indexedSnapshot.RngTraceValues[index])
                {
                    return AiDecisionIndexedMismatchReason.RngTrace;
                }
            }
            return AiDecisionIndexedMismatchReason.None;
        }

        private bool TryPrepareAiDecisionIndexedCanonical(LF2Entity self, int tickIndex)
        {
            AiDecisionIndexedCanonicalEligibleCountForDiagnostics++;
            if (!aiDecisionSharedPassAvailable ||
                aiDecisionIndexedSnapshot == null ||
                aiDecisionSharedSnapshot == null)
            {
                RecordAiDecisionIndexedCanonicalFallback(
                    aiDecisionSharedPassUnavailableReason == AiDecisionAvailability.None
                        ? AiDecisionAvailability.SnapshotMissing
                        : aiDecisionSharedPassUnavailableReason);
                return false;
            }

            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalCapture);
            AiDecisionAvailability captureAvailability;
            try
            {
                captureAvailability =
                    CaptureAiDecisionSharedOwnedSnapshot(self, aiDecisionIndexedSnapshot);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalCapture);
            }
            if (captureAvailability != AiDecisionAvailability.Available)
            {
                RecordAiDecisionIndexedCanonicalFallback(captureAvailability);
                return false;
            }

            AiDecisionWitness indexedWitness = default;
            bool indexedAvailable;
            long ordinal = AiDecisionIndexedCanonicalEligibleCountForDiagnostics - 1L;
            int sampleInterval = aiDecisionIndexedCanonicalFullOracleSampleInterval;
            bool captureOracleTrace = sampleInterval > 0 && ordinal % sampleInterval == 0;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalKernel);
            try
            {
                indexedAvailable = AiDecisionKernel.TryEvaluate(
                    aiDecisionIndexedSnapshot,
                    AiDecisionEvaluationPolicy.Indexed,
                    captureOracleTrace,
                    ref indexedWitness);
            }
            catch (Exception exception)
            {
                RecordAiDecisionShadowException(
                    AiDecisionShadowExceptionStage.KernelEvaluate,
                    exception);
                InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
                RecordAiDecisionIndexedCanonicalFallback(AiDecisionAvailability.SnapshotMissing);
                return false;
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalKernel);
            }
            if (!indexedAvailable)
            {
                RecordAiDecisionIndexedCanonicalFallback(indexedWitness.Availability);
                return false;
            }

            if (captureOracleTrace)
            {
                AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics++;
                aiDecisionSharedSnapshot.CopyOwnedFrom(aiDecisionIndexedSnapshot);
                AiDecisionWitness fullWitness = default;
                bool fullAvailable = AiDecisionKernel.TryEvaluate(
                    aiDecisionSharedSnapshot,
                    AiDecisionEvaluationPolicy.FullScan,
                    ref fullWitness);
                AiDecisionIndexedMismatchReason mismatch =
                    CompareAiDecisionIndexedWitnesses(
                        aiDecisionSharedSnapshot,
                        fullWitness,
                        fullAvailable,
                        aiDecisionIndexedSnapshot,
                        indexedWitness,
                        indexedAvailable);
                if (mismatch != AiDecisionIndexedMismatchReason.None)
                {
                    AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics++;
                    if (AiDecisionIndexedCanonicalFirstOracleMismatchReasonForDiagnostics ==
                        AiDecisionIndexedMismatchReason.None)
                    {
                        AiDecisionIndexedCanonicalFirstOracleMismatchReasonForDiagnostics =
                            mismatch;
                    }
                    RecordAiDecisionIndexedCanonicalFallback(
                        fullWitness.Availability == AiDecisionAvailability.Available
                            ? AiDecisionAvailability.SnapshotMissing
                            : fullWitness.Availability);
                    return false;
                }
            }

            diagnostics?.BeginPhase(
                BattleAiInputDetailPhase.IndexedCanonicalCommitValidation);
            AiDecisionAvailability commitAvailability;
            try
            {
                commitAvailability =
                    ValidateAiDecisionIndexedCanonicalCommit(
                        self,
                        aiDecisionIndexedSnapshot,
                        indexedWitness);
            }
            finally
            {
                diagnostics?.EndPhase(
                    BattleAiInputDetailPhase.IndexedCanonicalCommitValidation);
            }
            if (commitAvailability != AiDecisionAvailability.Available)
            {
                RecordAiDecisionIndexedCanonicalFallback(commitAvailability);
                return false;
            }

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalCommitApply);
            try
            {
                ApplyAiDecisionIndexedCanonicalCommit(self.Runtime, indexedWitness);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalCommitApply);
            }
            AiDecisionIndexedCanonicalCommittedCountForDiagnostics++;
            return true;
        }

        private AiDecisionAvailability ValidateAiDecisionIndexedCanonicalCommit(
            LF2Entity self,
            AiDecisionSnapshot snapshot,
            AiDecisionWitness witness)
        {
#if UNITY_INCLUDE_TESTS
            if (aiDecisionIndexedCanonicalPreCommitFailureForSelfCheck !=
                AiDecisionAvailability.None)
            {
                AiDecisionAvailability failure =
                    aiDecisionIndexedCanonicalPreCommitFailureForSelfCheck;
                aiDecisionIndexedCanonicalPreCommitFailureForSelfCheck =
                    AiDecisionAvailability.None;
                return failure;
            }
#endif
            AiSoASensingRows rows = aiDecisionSharedRows;
            if (!aiDecisionSharedPassAvailable || rows == null ||
                !ReferenceEquals(snapshot.Rows, rows) ||
                !AiSensingKernel.AreIndexesReady(rows) ||
                RuntimeSlotOccupancyEpochForServices != snapshot.OccupancyEpoch ||
                rows.CapturedOccupancyEpoch != snapshot.OccupancyEpoch)
            {
                return AiDecisionAvailability.EpochMismatch;
            }
            int selfSlot = snapshot.SelfSlot;
            if (self?.Runtime == null || self.Runtime.SlotIndex != selfSlot ||
                !TryGetCurrentRuntimeHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle selfHandle) ||
                selfHandle.Generation != snapshot.SelfGeneration ||
                rows.Generation[selfSlot] != snapshot.SelfGeneration)
            {
                return AiDecisionAvailability.GenerationMismatch;
            }
            if (self.Runtime.StableId != snapshot.SelfStableId ||
                rows.Identity[selfSlot] != snapshot.SelfStableId)
            {
                return AiDecisionAvailability.StableIdMismatch;
            }
            // IndexedCanonical evaluates a fully owned value snapshot and does not
            // call back into the world before this commit gate. Re-reading and
            // comparing every input/world/RNG field here duplicated the capture for
            // every AI. Occupancy, self identity and selected-handle validation still
            // guard every mutable reference consumed by the commit.
            if (Rng == null || Runtime?.Flow == null)
            {
                return AiDecisionAvailability.SnapshotMissing;
            }

            int selectedSlot = witness.FinalSelectedSlot;
            if (selectedSlot >= 0 && selectedSlot < rows.Capacity &&
                rows.Included[selectedSlot])
            {
                RuntimeEntityHandle selectedHandle = new RuntimeEntityHandle(
                    selectedSlot,
                    rows.Generation[selectedSlot]);
                if (!TryResolveRuntimeHandle(
                        selectedHandle,
                        out LF2Entity selected) ||
                    selected?.Runtime == null ||
                    selected.Runtime.StableId != rows.Identity[selectedSlot])
                {
                    return AiDecisionAvailability.GenerationMismatch;
                }
            }
            return AiDecisionAvailability.Available;
        }

        private void ApplyAiDecisionIndexedCanonicalCommit(
            NTSDEntityRuntime runtime,
            AiDecisionWitness witness)
        {
            AiDecisionInputState input = witness.Input;
            int[] history = runtime.InputHistory;
            history[0] = input.History0;
            history[1] = input.History1;
            history[2] = input.History2;
            history[3] = input.History3;
            history[4] = input.History4;
            history[5] = input.History5;
            runtime.CdAttack = input.CdAttack;
            runtime.CdJump = input.CdJump;
            runtime.CdDefend = input.CdDefend;
            runtime.CdDefendLock = input.CdDefendLock;
            runtime.CdRight = input.CdRight;
            runtime.CdLeft = input.CdLeft;
            runtime.CdUp = input.CdUp;
            runtime.CdDown = input.CdDown;
            runtime.ComboDra = input.ComboDra;
            runtime.ComboDla = input.ComboDla;
            runtime.ComboDua = input.ComboDua;
            runtime.ComboDda = input.ComboDda;
            runtime.ComboDrj = input.ComboDrj;
            runtime.ComboDlj = input.ComboDlj;
            runtime.ComboDuj = input.ComboDuj;
            runtime.ComboDdj = input.ComboDdj;
            runtime.ComboDja = input.ComboDja;
            runtime.PrevUp = input.PrevUp;
            runtime.PrevDown = input.PrevDown;
            runtime.PrevLeft = input.PrevLeft;
            runtime.PrevRight = input.PrevRight;
            runtime.PrevJump = input.PrevJump;
            runtime.PrevDefend = input.PrevDefend;
            runtime.PrevAttack = input.PrevAttack;
            runtime.KeyUp = input.KeyUp;
            runtime.KeyDown = input.KeyDown;
            runtime.KeyLeft = input.KeyLeft;
            runtime.KeyRight = input.KeyRight;
            runtime.KeyAttack = input.KeyAttack;
            runtime.KeyJump = input.KeyJump;
            runtime.KeyDefend = input.KeyDefend;
            runtime.Unk360 = input.Unk360;
            runtime.Unk3FC = input.Unk3FC;
            runtime.Unk400 = input.Unk400;

            AiDecisionWorldState world = witness.World;
            BattleFlowRuntimeState flow = Runtime.Flow;
            flow.AiDifficulty = world.FlowAiDifficulty;
            flow.AiRand3 = world.FlowRand3;
            flow.AiRand5 = world.FlowRand5;
            flow.AiRand15 = world.FlowRand15;
            flow.AiRand20 = world.FlowRand20;
            flow.AiMoveMode = world.FlowMoveMode;
            flow.AiStageTargetX = world.FlowStageTargetX;
            Rng.RestoreState(witness.RngState, witness.RngCalls);
        }

        private void RecordAiDecisionIndexedCanonicalFallback(
            AiDecisionAvailability reason)
        {
            if (AiUnifiedSnapshotExecutionFallbackForbidden)
            {
                ThrowAiUnifiedSnapshotExecutionHardBreach(
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare,
                    "IndexedCanonical attempted fallback after unified snapshot commit.");
            }
            AiDecisionIndexedCanonicalFallbackCountForDiagnostics++;
            if (AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics ==
                AiDecisionAvailability.None)
            {
                AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics =
                    reason == AiDecisionAvailability.None
                        ? AiDecisionAvailability.SnapshotMissing
                        : reason;
            }
        }

        private AiDecisionAvailability CaptureAiDecisionShadowSnapshot(
            LF2Entity self,
            AiDecisionSnapshot snapshot)
        {
            if (self?.Runtime == null)
                return AiDecisionAvailability.SelfNotIncluded;

            ulong epoch = RuntimeSlotOccupancyEpochForServices;
            snapshot.Reset(epoch);
            int capacity = snapshot.Rows.Capacity;
            if (capacity != RuntimeSlotCapacity)
                return AiDecisionAvailability.SnapshotMissing;

            AiSensingSnapshot rows = snapshot.Rows;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    view.RuntimeSlot != slot)
                    return AiDecisionAvailability.SnapshotMissing;
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return AiDecisionAvailability.SnapshotMissing;
                    continue;
                }

                LF2Entity entity = view.Entity;
                NTSDEntityRuntime runtime = entity?.Runtime;
                if (view.Generation == 0 ||
                    entity == null ||
                    runtime == null ||
                    runtime.SlotIndex != slot)
                    return AiDecisionAvailability.GenerationMismatch;
                if (!IsActiveForCurrentPass(entity))
                    continue;

                rows.Included[slot] = true;
                rows.Generation[slot] = view.Generation;
                rows.Identity[slot] = runtime.StableId;
                rows.ObjectId[slot] = entity.ObjectId;
                rows.DataObjectType[slot] = entity.GetCurrentDataObjectTypeForSimulation();
                rows.X[slot] = runtime.XInt;
                rows.Y[slot] = runtime.YInt;
                rows.Z[slot] = runtime.ZInt;
                rows.Hp[slot] = runtime.HP;
                rows.Hp3[slot] = runtime.HP3;
                rows.HpMax[slot] = runtime.HPBound;
                rows.Pp[slot] = runtime.PP;
                rows.Team[slot] = runtime.RelationTeam;
                rows.State[slot] = entity.GetState();
                rows.Frame[slot] = runtime.Frame;
                rows.LinkState[slot] = runtime.LinkState;
                rows.KillCount[slot] = runtime.KillCount;
                rows.CachedTargetSlot[slot] = runtime.Unk360;
                rows.CoordinateTargetX[slot] = runtime.Unk3FC;
                rows.Vx[slot] = runtime.Vx;
                rows.Facing[slot] = runtime.Dir == "left" ? 1 : 0;
                rows.TargetSlot[slot] = runtime.TargetSlotIndex;
                rows.HitStop[slot] = runtime.HitStop;
                rows.BoundaryFlags[slot] = CaptureAiDecisionBoundaryFlags(runtime);
                rows.InputHistoryGate[slot] =
                    runtime.InputHistory != null &&
                    runtime.InputHistory.Length == 6 &&
                    runtime.InputHistory[0] != 0;
            }

            if (RuntimeSlotOccupancyEpochForServices != epoch ||
                RuntimeSlotCapacity != capacity)
                return AiDecisionAvailability.EpochMismatch;
            int selfSlot = self.Runtime.SlotIndex;
            if (selfSlot < 0 || selfSlot >= capacity)
                return AiDecisionAvailability.SelfSlotInvalid;
            if (!rows.Included[selfSlot])
                return AiDecisionAvailability.SelfNotIncluded;
            if (!TryGetCurrentRuntimeHandle(selfSlot, self, out RuntimeEntityHandle selfHandle))
                return AiDecisionAvailability.GenerationMismatch;
            if (rows.Generation[selfSlot] != selfHandle.Generation)
                return AiDecisionAvailability.GenerationMismatch;
            if (rows.Identity[selfSlot] != self.Runtime.StableId)
                return AiDecisionAvailability.StableIdMismatch;
            if (!TryCaptureAiDecisionInputState(self.Runtime, out snapshot.Input))
                return AiDecisionAvailability.SnapshotMissing;

            snapshot.SelfSlot = selfSlot;
            snapshot.SelfGeneration = selfHandle.Generation;
            snapshot.SelfStableId = self.Runtime.StableId;
            snapshot.OccupancyEpoch = epoch;
            snapshot.World = CaptureAiDecisionWorldState();
            snapshot.RngState = Rng?.State ?? 0;
            snapshot.RngCalls = Rng?.CallCount ?? 0;
            return AiDecisionAvailability.Available;
        }

        private void PrepareAiDecisionSharedPass()
        {
            if (!AiDecisionRequiresSharedRows)
                return;

            AiDecisionSharedBuildCountForDiagnostics++;
            aiDecisionSharedPassAvailable = false;
            aiDecisionSharedPassUnavailableReason = AiDecisionAvailability.None;
            aiDecisionSharedPassEpoch = RuntimeSlotOccupancyEpochForServices;
            AiDecisionShadowExceptionStage exceptionStage =
                AiDecisionShadowExceptionStage.SharedBuild;
            try
            {
#if UNITY_INCLUDE_TESTS
                ThrowAiDecisionShadowExceptionForSelfCheck(exceptionStage);
#endif
                int capacity = RuntimeSlotCapacity;
                if (capacity <= 0)
                {
                    InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
                    return;
                }

                if (aiDecisionSharedRows == null ||
                    aiDecisionSharedRows.Capacity != capacity)
                {
                    aiDecisionSharedRows = new AiSoASensingRows(capacity);
                    aiDecisionSharedSnapshot = new AiDecisionSnapshot(aiDecisionSharedRows);
                    aiDecisionIndexedSnapshot = new AiDecisionSnapshot(aiDecisionSharedRows);
                }
                else if (aiDecisionSharedSnapshot == null ||
                         !ReferenceEquals(aiDecisionSharedSnapshot.Rows, aiDecisionSharedRows))
                {
                    aiDecisionSharedSnapshot = new AiDecisionSnapshot(aiDecisionSharedRows);
                }
                if (aiDecisionIndexedSnapshot == null ||
                    !ReferenceEquals(aiDecisionIndexedSnapshot.Rows, aiDecisionSharedRows))
                {
                    aiDecisionIndexedSnapshot = new AiDecisionSnapshot(aiDecisionSharedRows);
                }

                aiDecisionSharedSnapshot.ResetSharedRows(aiDecisionSharedPassEpoch);
                AiDecisionAvailability buildAvailability =
                    CaptureAiDecisionSharedRows(capacity, aiDecisionSharedPassEpoch);
                if (buildAvailability != AiDecisionAvailability.Available)
                {
                    InvalidateAiDecisionSharedPass(buildAvailability);
                    return;
                }

                aiDecisionSharedRows.SpecialIndexReady = true;
                BuildAiSoASensingRoleIndexes(aiDecisionSharedRows);
                BuildAiSoASensingTeamSummaries(aiDecisionSharedRows);
                exceptionStage = AiDecisionShadowExceptionStage.SharedPreflight;
#if UNITY_INCLUDE_TESTS
                ThrowAiDecisionShadowExceptionForSelfCheck(exceptionStage);
                ApplyAiDecisionSharedPreflightMutationForSelfCheck();
#endif
                AiDecisionAvailability preflightAvailability =
                    ValidateAiDecisionSharedPassPreflight(capacity, aiDecisionSharedPassEpoch);
                if (preflightAvailability != AiDecisionAvailability.Available)
                {
                    InvalidateAiDecisionSharedPass(preflightAvailability);
                    return;
                }

                BeginAiUnifiedSnapshotProductionMutationWitnessPass(
                    AiUnifiedSnapshotConsumer.IndexedDecision,
                    aiDecisionSharedPassEpoch);
                aiDecisionSharedPassAvailable = true;
            }
            catch (Exception exception)
            {
                RecordAiDecisionShadowException(exceptionStage, exception);
                InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
            }
        }

        private AiDecisionAvailability CaptureAiDecisionSharedRows(
            int capacity,
            ulong occupancyEpoch)
        {
            AiSoASensingRows rows = aiDecisionSharedRows;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    view.RuntimeSlot != slot)
                {
                    return AiDecisionAvailability.SnapshotMissing;
                }
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return AiDecisionAvailability.SnapshotMissing;
                    continue;
                }

                LF2Entity entity = view.Entity;
                NTSDEntityRuntime runtime = entity?.Runtime;
                if (view.Generation == 0 ||
                    entity == null ||
                    runtime == null ||
                    runtime.SlotIndex != slot)
                {
                    return AiDecisionAvailability.GenerationMismatch;
                }
                if (!IsActiveForCurrentPass(entity))
                    continue;
                if (!TryCaptureAiSoASensingRow(
                        rows,
                        entity,
                        slot,
                        view.Generation,
                        true))
                {
                    return AiDecisionAvailability.SnapshotMissing;
                }
                rows.BoundaryFlags[slot] = CaptureAiDecisionBoundaryFlags(runtime);
            }

            return RuntimeSlotOccupancyEpochForServices == occupancyEpoch &&
                   RuntimeSlotCapacity == capacity
                ? AiDecisionAvailability.Available
                : AiDecisionAvailability.EpochMismatch;
        }

        private AiDecisionAvailability ValidateAiDecisionSharedPassPreflight(
            int capacity,
            ulong occupancyEpoch)
        {
            AiSoASensingRows rows = aiDecisionSharedRows;
            if (rows == null ||
                rows.Capacity != capacity ||
                RuntimeSlotCapacity != capacity ||
                RuntimeSlotOccupancyEpochForServices != occupancyEpoch ||
                rows.CapturedOccupancyEpoch != occupancyEpoch)
            {
                return AiDecisionAvailability.EpochMismatch;
            }
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    view.RuntimeSlot != slot)
                {
                    return AiDecisionAvailability.SnapshotMissing;
                }
                if (!view.Claimed)
                {
                    if (view.Entity != null || rows.Included[slot])
                        return AiDecisionAvailability.SnapshotMissing;
                    continue;
                }

                LF2Entity entity = view.Entity;
                NTSDEntityRuntime runtime = entity?.Runtime;
                if (view.Generation == 0 ||
                    entity == null ||
                    runtime == null ||
                    runtime.SlotIndex != slot)
                {
                    return AiDecisionAvailability.GenerationMismatch;
                }

                bool shouldBeIncluded = IsActiveForCurrentPass(entity);
                if (shouldBeIncluded != rows.Included[slot])
                {
                    return shouldBeIncluded
                        ? AiDecisionAvailability.SelfNotIncluded
                        : AiDecisionAvailability.SnapshotMissing;
                }
                if (!shouldBeIncluded)
                    continue;
                if (rows.Generation[slot] != view.Generation)
                    return AiDecisionAvailability.GenerationMismatch;
                if (rows.Identity[slot] != runtime.StableId)
                    return AiDecisionAvailability.StableIdMismatch;
            }

            if (!AiSensingKernel.ValidateIndexedContract(rows))
                return AiDecisionAvailability.IndexesNotReady;
            return AiDecisionAvailability.Available;
        }

        private AiDecisionAvailability CaptureAiDecisionSharedOwnedSnapshot(
            LF2Entity self,
            AiDecisionSnapshot snapshot)
        {
            if (!aiDecisionSharedPassAvailable ||
                self?.Runtime == null ||
                snapshot == null ||
                !ReferenceEquals(snapshot.Rows, aiDecisionSharedRows))
            {
                return AiDecisionAvailability.SnapshotMissing;
            }

            ulong epoch = aiDecisionSharedPassEpoch;
            AiSoASensingRows rows = aiDecisionSharedRows;
            if (RuntimeSlotOccupancyEpochForServices != epoch ||
                rows.CapturedOccupancyEpoch != epoch)
            {
                InvalidateAiDecisionSharedPass(AiDecisionAvailability.EpochMismatch);
                return AiDecisionAvailability.EpochMismatch;
            }

            int selfSlot = self.Runtime.SlotIndex;
            if (selfSlot < 0 || selfSlot >= rows.Capacity)
            {
                return ResetRejectedAiDecisionSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.SelfSlotInvalid);
            }
            if (!rows.Included[selfSlot])
            {
                return ResetRejectedAiDecisionSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.SelfNotIncluded);
            }
            if (!TryGetCurrentRuntimeHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle selfHandle) ||
                rows.Generation[selfSlot] != selfHandle.Generation)
            {
                return ResetRejectedAiDecisionSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.GenerationMismatch);
            }
            if (rows.Identity[selfSlot] != self.Runtime.StableId)
            {
                return ResetRejectedAiDecisionSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.StableIdMismatch);
            }
            if (!TryCaptureAiDecisionInputState(self.Runtime, out snapshot.Input))
            {
                return ResetRejectedAiDecisionSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.SnapshotMissing);
            }

            snapshot.SelfSlot = selfSlot;
            snapshot.SelfGeneration = selfHandle.Generation;
            snapshot.SelfStableId = self.Runtime.StableId;
            snapshot.OccupancyEpoch = epoch;
            snapshot.World = CaptureAiDecisionWorldState();
            snapshot.RngState = Rng?.State ?? 0;
            snapshot.RngCalls = Rng?.CallCount ?? 0;
            snapshot.RngTraceCount = 0;
            snapshot.RngTraceOverflow = false;
            return AiDecisionAvailability.Available;
        }

        private static AiDecisionAvailability ResetRejectedAiDecisionSnapshot(
            AiDecisionSnapshot snapshot,
            ulong epoch,
            AiDecisionAvailability availability)
        {
            snapshot.ResetOwned(epoch);
            return availability;
        }

        private void RefreshAiDecisionSharedRowAfterCharacterInput(LF2Entity entity)
        {
            if (!AiDecisionRequiresSharedRows ||
                !aiDecisionSharedPassAvailable)
            {
                return;
            }

            try
            {
#if UNITY_INCLUDE_TESTS
                ThrowAiDecisionShadowExceptionForSelfCheck(
                    AiDecisionShadowExceptionStage.SharedRefresh);
#endif
                if (RuntimeSlotOccupancyEpochForServices != aiDecisionSharedPassEpoch ||
                    entity?.Runtime == null)
                {
                    InvalidateAiDecisionSharedPass(AiDecisionAvailability.EpochMismatch);
                    return;
                }

                int slot = entity.Runtime.SlotIndex;
                AiSoASensingRows rows = aiDecisionSharedRows;
                if (slot < 0 ||
                    slot >= rows.Capacity ||
                    !rows.Included[slot] ||
                    !TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                    handle.Generation != rows.Generation[slot])
                {
                    InvalidateAiDecisionSharedPass(AiDecisionAvailability.GenerationMismatch);
                    return;
                }
                if (entity.Runtime.StableId != rows.Identity[slot])
                {
                    InvalidateAiDecisionSharedPass(AiDecisionAvailability.StableIdMismatch);
                    return;
                }

                int previousX = rows.X[slot];
                int previousTeam = rows.Team[slot];
                int previousHp = rows.Hp[slot];
                int previousObjectId = rows.ObjectId[slot];
                bool previousSpecialMember = rows.SpecialScanMember[slot];
                bool wasGroundRole = IsGroundAiSoARoleMember(rows, slot);
                bool wasAirRole = IsAirAiSoARoleMember(rows, slot);
                bool wasLivingCharacter = IsLivingCharacterAiSoARow(rows, slot);
                if (!TryCaptureAiSoASensingRow(
                        rows,
                        entity,
                        slot,
                        handle.Generation,
                        false,
                        true))
                {
                    InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
                    return;
                }
                rows.BoundaryFlags[slot] = CaptureAiDecisionBoundaryFlags(entity.Runtime);
                bool currentSpecialMember =
                    slot >= 20 && IsAiSpecialScanObjectId(rows.ObjectId[slot]);
                if (previousObjectId != rows.ObjectId[slot] ||
                    previousSpecialMember != currentSpecialMember)
                {
                    InvalidateAiDecisionSharedPass(
                        AiDecisionAvailability.SnapshotMissing);
                    return;
                }

                AiDecisionSharedRefreshCountForDiagnostics++;
                bool isGroundRole = IsGroundAiSoARoleMember(rows, slot);
                bool isAirRole = IsAirAiSoARoleMember(rows, slot);
                bool roleRebuilt = previousX != rows.X[slot] ||
                                   previousTeam != rows.Team[slot] ||
                                   wasGroundRole != isGroundRole ||
                                   wasAirRole != isAirRole;
                if (roleRebuilt)
                {
                    BuildAiSoASensingRoleIndexes(rows);
                }

                bool isLivingCharacter = IsLivingCharacterAiSoARow(rows, slot);
                bool teamRebuilt = wasLivingCharacter != isLivingCharacter ||
                                   previousTeam != rows.Team[slot] ||
                                   previousHp != rows.Hp[slot];
                if (teamRebuilt)
                {
                    BuildAiSoASensingTeamSummaries(rows);
                }
                RecordAiUnifiedSnapshotProductionMutationWitness(
                    AiUnifiedSnapshotConsumer.IndexedDecision,
                    aiDecisionSharedPassEpoch,
                    slot,
                    handle.Generation,
                    entity.Runtime.StableId,
                    roleRebuilt,
                    teamRebuilt,
                    previousX,
                    rows.X[slot],
                    previousTeam,
                    rows.Team[slot],
                    PackAiUnifiedSnapshotRoleFlags(wasGroundRole, wasAirRole),
                    PackAiUnifiedSnapshotRoleFlags(isGroundRole, isAirRole),
                    wasLivingCharacter,
                    isLivingCharacter,
                    previousHp,
                    rows.Hp[slot]);
            }
            catch (Exception exception)
            {
                RecordAiDecisionShadowException(
                    AiDecisionShadowExceptionStage.SharedRefresh,
                    exception);
                InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
            }
        }

        private void EndAiDecisionSharedPass()
        {
            if (!AiDecisionRequiresSharedRows)
                return;
            aiDecisionSharedPassAvailable = false;
            aiDecisionSharedPassEpoch = 0;
            aiDecisionComparisonSnapshot = null;
        }

        private void InvalidateAiDecisionSharedPass(AiDecisionAvailability reason)
        {
            aiDecisionSharedPassAvailable = false;
            if (aiDecisionSharedPassUnavailableReason == AiDecisionAvailability.None)
            {
                aiDecisionSharedPassUnavailableReason = reason == AiDecisionAvailability.None
                    ? AiDecisionAvailability.SnapshotMissing
                    : reason;
            }
        }

        private void RecordAiDecisionShadowException(
            AiDecisionShadowExceptionStage stage,
            Exception exception)
        {
            if (stage == AiDecisionShadowExceptionStage.None ||
                exception == null ||
                AiDecisionShadowFirstExceptionStageForDiagnostics !=
                AiDecisionShadowExceptionStage.None)
            {
                return;
            }

            AiDecisionShadowFirstExceptionStageForDiagnostics = stage;
            aiDecisionShadowFirstExceptionType = exception.GetType();
        }

#if UNITY_INCLUDE_TESTS
        public void SetAiDecisionShadowExceptionStageForSelfCheck(
            AiDecisionShadowExceptionStage stage)
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "Cannot arm AI decision exception injection while ticking.");
            if (stage != AiDecisionShadowExceptionStage.SharedBuild &&
                stage != AiDecisionShadowExceptionStage.SharedPreflight &&
                stage != AiDecisionShadowExceptionStage.KernelEvaluate &&
                stage != AiDecisionShadowExceptionStage.SharedRefresh)
            {
                throw new ArgumentOutOfRangeException(nameof(stage));
            }
            aiDecisionShadowExceptionStageForSelfCheck = stage;
        }

        private void ThrowAiDecisionShadowExceptionForSelfCheck(
            AiDecisionShadowExceptionStage stage)
        {
            if (aiDecisionShadowExceptionStageForSelfCheck != stage)
                return;
            aiDecisionShadowExceptionStageForSelfCheck = AiDecisionShadowExceptionStage.None;
            throw new AiDecisionShadowSelfCheckException();
        }

        private sealed class AiDecisionShadowSelfCheckException : Exception
        {
        }

        public void SetAiDecisionSharedPreflightMutationForSelfCheck(
            int mutationKind,
            int slot)
        {
            if (_ticking)
                throw new InvalidOperationException("Cannot arm AI decision preflight mutation while ticking.");
            if (mutationKind < 0 || mutationKind > 3)
                throw new ArgumentOutOfRangeException(nameof(mutationKind));
            aiDecisionSharedPreflightMutationKindForSelfCheck = mutationKind;
            aiDecisionSharedPreflightMutationSlotForSelfCheck = slot;
        }

        public void SetAiDecisionIndexedCanonicalPreCommitFailureForSelfCheck(
            AiDecisionAvailability reason)
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "Cannot arm AI decision canonical commit failure while ticking.");
            if (reason == AiDecisionAvailability.None ||
                reason == AiDecisionAvailability.Available)
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }
            aiDecisionIndexedCanonicalPreCommitFailureForSelfCheck = reason;
        }

        public void SetAiDecisionSharedPostLegacyStateMutationForSelfCheck(
            int slot,
            int state)
        {
            if (_ticking)
                throw new InvalidOperationException("Cannot arm AI decision row mutation while ticking.");
            aiDecisionSharedPostLegacyMutationSlotForSelfCheck = slot;
            aiDecisionSharedPostLegacyMutationStateForSelfCheck = state;
        }

        private void ApplyAiDecisionSharedPreflightMutationForSelfCheck()
        {
            int mutationKind = aiDecisionSharedPreflightMutationKindForSelfCheck;
            int slot = aiDecisionSharedPreflightMutationSlotForSelfCheck;
            aiDecisionSharedPreflightMutationKindForSelfCheck = -1;
            aiDecisionSharedPreflightMutationSlotForSelfCheck = -1;
            if (mutationKind < 0 ||
                aiDecisionSharedRows == null ||
                slot < 0 ||
                slot >= aiDecisionSharedRows.Capacity)
            {
                return;
            }

            switch (mutationKind)
            {
                case 0:
                    aiDecisionSharedRows.CapturedOccupancyEpoch =
                        aiDecisionSharedPassEpoch == ulong.MaxValue
                            ? 1UL
                            : aiDecisionSharedPassEpoch + 1UL;
                    break;
                case 1:
                    aiDecisionSharedRows.Generation[slot]++;
                    break;
                case 2:
                    aiDecisionSharedRows.Identity[slot]++;
                    break;
                case 3:
                    aiDecisionSharedRows.Included[slot] = false;
                    break;
            }
        }

        private void ApplyAiDecisionSharedPostLegacyMutationForSelfCheck(LF2Entity entity)
        {
            if (entity?.Runtime == null ||
                entity.Runtime.SlotIndex != aiDecisionSharedPostLegacyMutationSlotForSelfCheck)
            {
                return;
            }

            aiDecisionSharedPostLegacyMutationSlotForSelfCheck = -1;
            if (entity.Frame?.D != null)
                entity.Frame.D.state = aiDecisionSharedPostLegacyMutationStateForSelfCheck;
        }
#endif

        private AiDecisionWorldState CaptureAiDecisionWorldState()
        {
            BattleFlowRuntimeState flow = Runtime?.Flow;
            return new AiDecisionWorldState
            {
                Difficulty = Difficulty,
                AiPhaseGate = AiPhaseGate,
                InputPhase = InputPhase,
                StageTargetX = Runtime?.Stage?.XMaxOverride > 0
                    ? Runtime.Stage.XMaxOverride
                    : Runtime?.Stage?.StageWidthPx ?? 800,
                StageZMin = Runtime?.Stage?.ZMin ?? 180,
                StageZMax = Runtime?.Stage?.ZMax ?? 350,
                FlowAiDifficulty = flow?.AiDifficulty ?? 0,
                FlowRand3 = flow?.AiRand3 ?? 0,
                FlowRand5 = flow?.AiRand5 ?? 0,
                FlowRand15 = flow?.AiRand15 ?? 0,
                FlowRand20 = flow?.AiRand20 ?? 0,
                FlowMoveMode = flow?.AiMoveMode ?? 0,
                FlowStageTargetX = flow?.AiStageTargetX ??
                    (Runtime?.Stage?.StageWidthPx ?? 800),
            };
        }

        private static bool TryCaptureAiDecisionInputState(
            NTSDEntityRuntime runtime,
            out AiDecisionInputState input)
        {
            input = default;
            int[] history = runtime?.InputHistory;
            if (runtime == null || history == null || history.Length != 6)
                return false;
            input.History0 = history[0];
            input.History1 = history[1];
            input.History2 = history[2];
            input.History3 = history[3];
            input.History4 = history[4];
            input.History5 = history[5];
            input.CdAttack = runtime.CdAttack;
            input.CdJump = runtime.CdJump;
            input.CdDefend = runtime.CdDefend;
            input.CdDefendLock = runtime.CdDefendLock;
            input.CdRight = runtime.CdRight;
            input.CdLeft = runtime.CdLeft;
            input.CdUp = runtime.CdUp;
            input.CdDown = runtime.CdDown;
            input.ComboDra = runtime.ComboDra;
            input.ComboDla = runtime.ComboDla;
            input.ComboDua = runtime.ComboDua;
            input.ComboDda = runtime.ComboDda;
            input.ComboDrj = runtime.ComboDrj;
            input.ComboDlj = runtime.ComboDlj;
            input.ComboDuj = runtime.ComboDuj;
            input.ComboDdj = runtime.ComboDdj;
            input.ComboDja = runtime.ComboDja;
            input.PrevUp = runtime.PrevUp;
            input.PrevDown = runtime.PrevDown;
            input.PrevLeft = runtime.PrevLeft;
            input.PrevRight = runtime.PrevRight;
            input.PrevJump = runtime.PrevJump;
            input.PrevDefend = runtime.PrevDefend;
            input.PrevAttack = runtime.PrevAttack;
            input.KeyUp = runtime.KeyUp;
            input.KeyDown = runtime.KeyDown;
            input.KeyLeft = runtime.KeyLeft;
            input.KeyRight = runtime.KeyRight;
            input.KeyAttack = runtime.KeyAttack;
            input.KeyJump = runtime.KeyJump;
            input.KeyDefend = runtime.KeyDefend;
            input.Unk360 = runtime.Unk360;
            input.Unk3FC = runtime.Unk3FC;
            input.Unk400 = runtime.Unk400;
            input.BoundaryFlags = CaptureAiDecisionBoundaryFlags(runtime);
            return true;
        }

        private static int CaptureAiDecisionBoundaryFlags(NTSDEntityRuntime runtime)
        {
            int flags = 0;
            if (runtime.XBoundPositive)
                flags |= 1;
            if (runtime.XBoundNegative)
                flags |= 2;
            if (runtime.ZBoundPositive)
                flags |= 4;
            if (runtime.ZBoundNegative)
                flags |= 8;
            return flags;
        }

        private void RecordAiDecisionShadowLegacyRng(int modulus, int raw, int value)
        {
            int index = aiDecisionLegacyRngCount;
            if (index < aiDecisionLegacyRngModuli.Length)
            {
                aiDecisionLegacyRngModuli[index] = modulus;
                aiDecisionLegacyRngRaw[index] = raw;
                aiDecisionLegacyRngValues[index] = value;
            }
            else
            {
                aiDecisionLegacyRngOverflow = true;
            }
            unchecked
            {
                aiDecisionLegacyRngOrderHash ^= (uint)modulus;
                aiDecisionLegacyRngOrderHash *= AiDecisionRngHashPrime;
                aiDecisionLegacyRngOrderHash ^= (uint)raw;
                aiDecisionLegacyRngOrderHash *= AiDecisionRngHashPrime;
                aiDecisionLegacyRngOrderHash ^= (uint)value;
                aiDecisionLegacyRngOrderHash *= AiDecisionRngHashPrime;
            }
            aiDecisionLegacyRngCount++;
        }

        private AiDecisionShadowMismatchReason CompareAiDecisionShadowResult(LF2Entity self)
        {
            AiDecisionSnapshot comparisonSnapshot = aiDecisionComparisonSnapshot;
            if (comparisonSnapshot == null)
                return AiDecisionShadowMismatchReason.SnapshotUnavailable;
            if (!TryCaptureAiDecisionInputState(self.Runtime, out AiDecisionInputState actualInput) ||
                !AiDecisionInputEquals(aiDecisionShadowExpected.Input, actualInput))
                return AiDecisionShadowMismatchReason.Input;
            AiDecisionWorldState actualWorld = CaptureAiDecisionWorldState();
            if (!AiDecisionWorldEquals(aiDecisionShadowExpected.World, actualWorld))
                return AiDecisionShadowMismatchReason.WorldFlow;
            if ((Rng?.State ?? 0) != aiDecisionShadowExpected.RngState)
                return AiDecisionShadowMismatchReason.RngState;
            if ((Rng?.CallCount ?? 0) != aiDecisionShadowExpected.RngCalls)
                return AiDecisionShadowMismatchReason.RngCalls;
            if (aiDecisionLegacyRngOverflow || aiDecisionShadowExpected.RngTraceOverflow)
                return AiDecisionShadowMismatchReason.RngTraceOverflow;
            if (aiDecisionLegacyRngCount != aiDecisionShadowExpected.RngDrawCount ||
                aiDecisionLegacyRngOrderHash != aiDecisionShadowExpected.RngOrderHash)
                return AiDecisionShadowMismatchReason.RngOrder;
            for (int index = 0; index < aiDecisionLegacyRngCount; index++)
            {
                if (aiDecisionLegacyRngModuli[index] != comparisonSnapshot.RngTraceModuli[index] ||
                    aiDecisionLegacyRngRaw[index] != comparisonSnapshot.RngTraceRaw[index] ||
                    aiDecisionLegacyRngValues[index] != comparisonSnapshot.RngTraceValues[index])
                    return AiDecisionShadowMismatchReason.RngOrder;
            }
            return AiDecisionShadowMismatchReason.None;
        }

        private void RecordAiDecisionShadowUnavailable(AiDecisionAvailability reason)
        {
            AiDecisionShadowUnavailableCountForDiagnostics++;
            if (AiDecisionShadowFirstUnavailableReasonForDiagnostics == AiDecisionAvailability.None)
                AiDecisionShadowFirstUnavailableReasonForDiagnostics = reason;
        }

        private static bool AiDecisionWorldEquals(
            AiDecisionWorldState expected,
            AiDecisionWorldState actual)
        {
            return expected.Difficulty == actual.Difficulty &&
                   expected.AiPhaseGate == actual.AiPhaseGate &&
                   expected.InputPhase == actual.InputPhase &&
                   expected.StageTargetX == actual.StageTargetX &&
                   expected.StageZMin == actual.StageZMin &&
                   expected.StageZMax == actual.StageZMax &&
                   expected.FlowAiDifficulty == actual.FlowAiDifficulty &&
                   expected.FlowRand3 == actual.FlowRand3 &&
                   expected.FlowRand5 == actual.FlowRand5 &&
                   expected.FlowRand15 == actual.FlowRand15 &&
                   expected.FlowRand20 == actual.FlowRand20 &&
                   expected.FlowMoveMode == actual.FlowMoveMode &&
                   expected.FlowStageTargetX == actual.FlowStageTargetX;
        }

        private bool TryPrepareAiUnifiedSnapshotExecutionPass(
            BattleAiInputDetailDiagnostics diagnostics)
        {
            if (aiUnifiedSnapshotExecutionMode !=
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                return false;
            }

            EndAiUnifiedSnapshotExecutionPass();
            AiUnifiedSnapshotExecutionBuildCountForDiagnostics++;
            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Prepare;
            try
            {
                if (aiSensingMode != AiSensingMode.SoAAiSensing ||
                    aiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical ||
                    aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Disabled)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority configuration changed before its build.");
                }

                int capacity = RuntimeSlotCapacity;
                ulong epoch = RuntimeSlotOccupancyEpochForServices;
                if (capacity <= 0)
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority requires positive runtime slot capacity.");

                PrepareAiUnifiedSnapshotLegacyConsumerBuffers(capacity);
                EnsureAiUnifiedSnapshotExecutionScratchCapacity(capacity);
                AiUnifiedSnapshotExecutionState candidate =
                    aiUnifiedSnapshotScratchState;
                candidate.Reset(epoch, capacity);
                AiSoASensingRows rows = candidate.Rows;

                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                stage = AiUnifiedSnapshotExceptionStage.Capture;
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                for (int slot = 0; slot < capacity; slot++)
                {
                    AiUnifiedSnapshotExecutionSlotVisitCountForDiagnostics++;
                    if (!TryGetRuntimeSlotReadOnlyView(
                            slot,
                            out RuntimeSlotTable.ReadOnlySlotView view) ||
                        view.RuntimeSlot != slot)
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority could not read the runtime slot table.");
                    }
                    if (!view.Claimed)
                    {
                        if (view.Entity != null)
                        {
                            throw new InvalidOperationException(
                                "Unified AI snapshot authority observed a ghost slot occupant.");
                        }
                        continue;
                    }

                    LF2Entity entity = view.Entity;
                    NTSDEntityRuntime runtime = entity?.Runtime;
                    if (view.Generation == 0 ||
                        entity == null ||
                        runtime == null ||
                        runtime.SlotIndex != slot)
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority observed an invalid generation or runtime slot.");
                    }
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    candidate.FallbackSlots[slot] = entity;
                    if (slot < candidate.MoveModeFirst10Present.Length)
                    {
                        CaptureAiUnifiedMoveModeScratchCandidate(
                            candidate,
                            slot,
                            entity,
                            view.Generation);
                    }
                    if (!TryCaptureAiSoASensingRow(
                            rows,
                            entity,
                            slot,
                            view.Generation,
                            true))
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority could not capture an active row.");
                    }

                    int sensingFlags = CaptureAiSoASensingBoundaryFlags(runtime);
                    int decisionFlags = CaptureAiDecisionBoundaryFlags(runtime);
                    rows.BoundaryFlags[slot] = sensingFlags;
                    candidate.SoASensingBoundaryFlags[slot] = sensingFlags;
                    candidate.DecisionBoundaryFlags[slot] = decisionFlags;
                }

                stage = AiUnifiedSnapshotExceptionStage.BuildIndexes;
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                rows.SpecialIndexReady = true;
                BuildAiSoASensingRoleIndexes(rows);
                BuildAiSoASensingTeamSummaries(rows);

                stage = AiUnifiedSnapshotExceptionStage.Validate;
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                candidate.MoveModeFirst10Valid = true;
                if (!ValidateAiUnifiedSnapshotExecutionState(
                        candidate,
                        capacity,
                        epoch))
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority pre-commit validation failed.");
                }

                CommitAiUnifiedSnapshotExecutionPass(candidate);
                stage = AiUnifiedSnapshotExceptionStage.InitialSensingCompare;
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                return true;
            }
            catch (Exception exception)
            {
                if (AiUnifiedSnapshotExecutionFallbackForbidden)
                {
                    RecordAiUnifiedSnapshotExecutionFailure(stage, exception, true);
                    aiUnifiedSnapshotExecutionCommittedThisPass = false;
                    aiSoASensingPassInvalidated = true;
                    InvalidateAiDecisionSharedPass(
                        AiDecisionAvailability.SnapshotMissing);
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority failed after publication; same-tick fallback is forbidden.",
                        exception);
                }
                RecordAiUnifiedSnapshotExecutionFailure(stage, exception, false);
                AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics++;
                AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics++;
                RestoreAiUnifiedSnapshotLegacyConsumerBuffers();
                return false;
            }
        }

        private void EnsureAiUnifiedSnapshotExecutionScratchCapacity(int capacity)
        {
            if (aiUnifiedSnapshotScratchState != null &&
                aiUnifiedSnapshotScratchState.Capacity == capacity &&
                !ReferenceEquals(
                    aiUnifiedSnapshotScratchState,
                    aiUnifiedSnapshotPublishedState))
            {
                return;
            }

            aiUnifiedSnapshotScratchState =
                new AiUnifiedSnapshotExecutionState(capacity);
        }

        private void CommitAiUnifiedSnapshotExecutionPass(
            AiUnifiedSnapshotExecutionState candidate)
        {
            AiUnifiedSnapshotExecutionState previous =
                aiUnifiedSnapshotPublishedState;
            aiUnifiedSnapshotPublishedState = candidate;
            aiUnifiedSnapshotScratchState = previous;

            aiSoASensingRows = candidate.Rows;
            aiDecisionSharedRows = candidate.Rows;
            aiDecisionSharedSnapshot = candidate.SharedSnapshot;
            aiDecisionIndexedSnapshot = candidate.IndexedSnapshot;
            aiInputSlots = candidate.FallbackSlots;
            aiMoveModeFirst10Present = candidate.MoveModeFirst10Present;
            aiMoveModeFirst10Eligible = candidate.MoveModeFirst10Eligible;
            aiMoveModeFirst10Generation = candidate.MoveModeFirst10Generation;
            aiMoveModeFirst10Hp = candidate.MoveModeFirst10Hp;
            aiMoveModeFirst10X = candidate.MoveModeFirst10X;
            aiMoveModeFirst10Z = candidate.MoveModeFirst10Z;
            aiMoveModeTopSlot = candidate.MoveModeTopSlot;
            aiMoveModeTopX = candidate.MoveModeTopX;
            aiMoveModeTopZ = candidate.MoveModeTopZ;
            aiMoveModeSecondSlot = candidate.MoveModeSecondSlot;
            aiMoveModeSecondX = candidate.MoveModeSecondX;
            aiMoveModeSecondZ = candidate.MoveModeSecondZ;
            aiMoveModeFirst10Valid = candidate.MoveModeFirst10Valid;

            aiUnifiedSnapshotExecutionCommittedThisPass = true;
            aiUnifiedSnapshotExecutionConsumerStartedThisPass = false;
            aiSoASensingSnapshotEpoch = candidate.Epoch;
            aiSoASensingSnapshotValid = true;
            aiSoASensingPassInvalidated = false;
            aiSoACandidatePassLatchedToLegacy = false;
            aiDecisionSharedPassEpoch = candidate.Epoch;
            aiDecisionSharedPassUnavailableReason = AiDecisionAvailability.None;
            aiDecisionSharedPassAvailable = true;
            aiInputSlotSnapshotOccupancyEpoch = candidate.Epoch;
            AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics++;
        }

        private void CaptureAiUnifiedMoveModeScratchCandidate(
            AiUnifiedSnapshotExecutionState candidate,
            int slot,
            LF2Entity entity,
            uint generation)
        {
            candidate.MoveModeFirst10Present[slot] = true;
            candidate.MoveModeFirst10Generation[slot] = generation;
            candidate.MoveModeFirst10Hp[slot] = Hp(entity);
            bool eligible = IsLivingCharacterDat(entity);
            candidate.MoveModeFirst10Eligible[slot] = eligible;
            if (!eligible)
                return;

            int x = X(entity);
            int z = Z(entity);
            candidate.MoveModeFirst10X[slot] = x;
            candidate.MoveModeFirst10Z[slot] = z;
            if (x <= -1)
                return;
            if (candidate.MoveModeTopSlot < 0 ||
                x > candidate.MoveModeTopX)
            {
                candidate.MoveModeSecondSlot = candidate.MoveModeTopSlot;
                candidate.MoveModeSecondX = candidate.MoveModeTopX;
                candidate.MoveModeSecondZ = candidate.MoveModeTopZ;
                candidate.MoveModeTopSlot = slot;
                candidate.MoveModeTopX = x;
                candidate.MoveModeTopZ = z;
                return;
            }
            if (candidate.MoveModeSecondSlot < 0 ||
                x > candidate.MoveModeSecondX)
            {
                candidate.MoveModeSecondSlot = slot;
                candidate.MoveModeSecondX = x;
                candidate.MoveModeSecondZ = z;
            }
        }

        private void ObserveAiUnifiedSnapshotExecutionMoveModeFirst10Mutation(
            AiUnifiedSnapshotExecutionState published,
            LF2Entity entity)
        {
            if (!published.MoveModeFirst10Valid)
                return;
            if (entity?.Runtime == null)
            {
                published.MoveModeFirst10Valid = false;
                aiMoveModeFirst10Valid = false;
                return;
            }

            int slot = Slot(entity);
            if (slot < 0 || slot >= published.MoveModeFirst10Present.Length)
            {
                for (int index = 0;
                     index < published.MoveModeFirst10Present.Length;
                     index++)
                {
                    if (ReferenceEquals(published.FallbackSlots[index], entity))
                    {
                        published.MoveModeFirst10Valid = false;
                        aiMoveModeFirst10Valid = false;
                        break;
                    }
                }
                return;
            }

            if (!published.MoveModeFirst10Present[slot] ||
                !ReferenceEquals(published.FallbackSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != published.MoveModeFirst10Generation[slot] ||
                Hp(entity) != published.MoveModeFirst10Hp[slot])
            {
                published.MoveModeFirst10Valid = false;
                aiMoveModeFirst10Valid = false;
                return;
            }

            bool eligible = IsLivingCharacterDat(entity);
            if (eligible != published.MoveModeFirst10Eligible[slot] ||
                eligible &&
                (X(entity) != published.MoveModeFirst10X[slot] ||
                 Z(entity) != published.MoveModeFirst10Z[slot]))
            {
                published.MoveModeFirst10Valid = false;
                aiMoveModeFirst10Valid = false;
            }
        }

        private bool ValidateAiUnifiedSnapshotExecutionState(
            AiUnifiedSnapshotExecutionState candidate,
            int capacity,
            ulong epoch)
        {
            if (candidate == null ||
                candidate.ExpectedCapacity != capacity ||
                candidate.Capacity != capacity ||
                candidate.Epoch != epoch ||
                candidate.Rows.CapturedOccupancyEpoch != epoch ||
                RuntimeSlotCapacity != capacity ||
                RuntimeSlotOccupancyEpochForServices != epoch ||
                !AiSensingKernel.ValidateIndexedContract(candidate.Rows))
            {
                return false;
            }

            AiSoASensingRows rows = candidate.Rows;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    view.RuntimeSlot != slot)
                {
                    return false;
                }

                LF2Entity entity = view.Entity;
                bool shouldBeIncluded =
                    view.Claimed && entity != null && IsActiveForCurrentPass(entity);
                if (rows.Included[slot] != shouldBeIncluded ||
                    !ReferenceEquals(candidate.FallbackSlots[slot],
                        shouldBeIncluded ? entity : null))
                {
                    return false;
                }
                if (!shouldBeIncluded)
                {
                    if (!view.Claimed && entity != null)
                        return false;
                    continue;
                }

                NTSDEntityRuntime runtime = entity.Runtime;
                if (view.Generation == 0 ||
                    runtime == null ||
                    runtime.SlotIndex != slot ||
                    rows.Generation[slot] != view.Generation ||
                    rows.Identity[slot] != runtime.StableId ||
                    rows.SpecialScanMember[slot] !=
                        (slot >= 20 && IsAiSpecialScanObjectId(rows.ObjectId[slot])) ||
                    candidate.SoASensingBoundaryFlags[slot] !=
                        CaptureAiSoASensingBoundaryFlags(runtime) ||
                    candidate.DecisionBoundaryFlags[slot] !=
                        CaptureAiDecisionBoundaryFlags(runtime))
                {
                    return false;
                }

                if (slot >= candidate.MoveModeFirst10Present.Length)
                    continue;
                bool eligible = IsLivingCharacterDat(entity);
                if (!candidate.MoveModeFirst10Present[slot] ||
                    candidate.MoveModeFirst10Generation[slot] != view.Generation ||
                    candidate.MoveModeFirst10Hp[slot] != Hp(entity) ||
                    candidate.MoveModeFirst10Eligible[slot] != eligible ||
                    eligible &&
                    (candidate.MoveModeFirst10X[slot] != X(entity) ||
                     candidate.MoveModeFirst10Z[slot] != Z(entity)))
                {
                    return false;
                }
            }

            return candidate.MoveModeFirst10Valid;
        }

        private void PrepareAiUnifiedSnapshotLegacyConsumerBuffers(int capacity)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            if (aiSoASensingRows != null &&
                (published == null ||
                 !ReferenceEquals(aiSoASensingRows, published.Rows)))
            {
                aiUnifiedSnapshotLegacySoARows = aiSoASensingRows;
            }
            if (aiDecisionSharedRows != null &&
                (published == null ||
                 !ReferenceEquals(aiDecisionSharedRows, published.Rows)))
            {
                aiUnifiedSnapshotLegacyDecisionRows = aiDecisionSharedRows;
            }
            if (aiInputSlots != null &&
                (published == null ||
                 !ReferenceEquals(aiInputSlots, published.FallbackSlots)))
            {
                aiUnifiedSnapshotLegacyInputSlots = aiInputSlots;
                aiUnifiedSnapshotLegacyMoveModeFirst10Present =
                    aiMoveModeFirst10Present;
                aiUnifiedSnapshotLegacyMoveModeFirst10Eligible =
                    aiMoveModeFirst10Eligible;
                aiUnifiedSnapshotLegacyMoveModeFirst10Generation =
                    aiMoveModeFirst10Generation;
                aiUnifiedSnapshotLegacyMoveModeFirst10Hp = aiMoveModeFirst10Hp;
                aiUnifiedSnapshotLegacyMoveModeFirst10X = aiMoveModeFirst10X;
                aiUnifiedSnapshotLegacyMoveModeFirst10Z = aiMoveModeFirst10Z;
            }

            if (aiUnifiedSnapshotLegacySoARows == null ||
                aiUnifiedSnapshotLegacySoARows.Capacity != capacity)
            {
                aiUnifiedSnapshotLegacySoARows = new AiSoASensingRows(capacity);
            }
            if (aiUnifiedSnapshotLegacyDecisionRows == null ||
                aiUnifiedSnapshotLegacyDecisionRows.Capacity != capacity)
            {
                aiUnifiedSnapshotLegacyDecisionRows =
                    new AiSoASensingRows(capacity);
            }
            if (aiUnifiedSnapshotLegacySharedSnapshot == null ||
                !ReferenceEquals(
                    aiUnifiedSnapshotLegacySharedSnapshot.Rows,
                    aiUnifiedSnapshotLegacyDecisionRows))
            {
                aiUnifiedSnapshotLegacySharedSnapshot =
                    new AiDecisionSnapshot(aiUnifiedSnapshotLegacyDecisionRows);
            }
            if (aiUnifiedSnapshotLegacyIndexedSnapshot == null ||
                !ReferenceEquals(
                    aiUnifiedSnapshotLegacyIndexedSnapshot.Rows,
                    aiUnifiedSnapshotLegacyDecisionRows))
            {
                aiUnifiedSnapshotLegacyIndexedSnapshot =
                    new AiDecisionSnapshot(aiUnifiedSnapshotLegacyDecisionRows);
            }
            if (aiUnifiedSnapshotLegacyInputSlots == null ||
                aiUnifiedSnapshotLegacyInputSlots.Length != capacity)
            {
                aiUnifiedSnapshotLegacyInputSlots = new LF2Entity[capacity];
            }
            if (aiUnifiedSnapshotLegacyMoveModeFirst10Present == null ||
                aiUnifiedSnapshotLegacyMoveModeFirst10Present.Length != 10)
            {
                aiUnifiedSnapshotLegacyMoveModeFirst10Present = new bool[10];
                aiUnifiedSnapshotLegacyMoveModeFirst10Eligible = new bool[10];
                aiUnifiedSnapshotLegacyMoveModeFirst10Generation = new uint[10];
                aiUnifiedSnapshotLegacyMoveModeFirst10Hp = new int[10];
                aiUnifiedSnapshotLegacyMoveModeFirst10X = new int[10];
                aiUnifiedSnapshotLegacyMoveModeFirst10Z = new int[10];
            }
        }

        private void RestoreAiUnifiedSnapshotLegacyConsumerBuffers()
        {
            int capacity = RuntimeSlotCapacity;
            if (capacity <= 0)
                return;
            PrepareAiUnifiedSnapshotLegacyConsumerBuffers(capacity);
            aiSoASensingRows = aiUnifiedSnapshotLegacySoARows;
            aiDecisionSharedRows = aiUnifiedSnapshotLegacyDecisionRows;
            aiDecisionSharedSnapshot = aiUnifiedSnapshotLegacySharedSnapshot;
            aiDecisionIndexedSnapshot = aiUnifiedSnapshotLegacyIndexedSnapshot;
            aiInputSlots = aiUnifiedSnapshotLegacyInputSlots;
            aiMoveModeFirst10Present =
                aiUnifiedSnapshotLegacyMoveModeFirst10Present;
            aiMoveModeFirst10Eligible =
                aiUnifiedSnapshotLegacyMoveModeFirst10Eligible;
            aiMoveModeFirst10Generation =
                aiUnifiedSnapshotLegacyMoveModeFirst10Generation;
            aiMoveModeFirst10Hp = aiUnifiedSnapshotLegacyMoveModeFirst10Hp;
            aiMoveModeFirst10X = aiUnifiedSnapshotLegacyMoveModeFirst10X;
            aiMoveModeFirst10Z = aiUnifiedSnapshotLegacyMoveModeFirst10Z;
            aiMoveModeTopSlot = -1;
            aiMoveModeTopX = -1;
            aiMoveModeTopZ = 0;
            aiMoveModeSecondSlot = -1;
            aiMoveModeSecondX = -1;
            aiMoveModeSecondZ = 0;
            aiMoveModeFirst10Valid = false;
        }

        private void EndAiUnifiedSnapshotExecutionPass()
        {
            aiUnifiedSnapshotExecutionCommittedThisPass = false;
            aiUnifiedSnapshotExecutionConsumerStartedThisPass = false;
        }

        private void RecordAiUnifiedSnapshotExecutionFailure(
            AiUnifiedSnapshotExceptionStage stage,
            Exception exception,
            bool postCommit)
        {
            if (AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics ==
                AiUnifiedSnapshotExceptionStage.None)
            {
                AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics = stage;
                AiUnifiedSnapshotExecutionFirstFailureTypeForDiagnostics =
                    exception?.GetType();
            }
            if (postCommit)
                AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics++;
        }

        private void ThrowAiUnifiedSnapshotExecutionHardBreach(
            AiUnifiedSnapshotExceptionStage stage,
            string message)
        {
            var exception = new InvalidOperationException(message);
            RecordAiUnifiedSnapshotExecutionFailure(stage, exception, true);
            aiUnifiedSnapshotExecutionCommittedThisPass = false;
            aiSoASensingPassInvalidated = true;
            InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
            throw exception;
        }

        private void BeginAiUnifiedSnapshotExecutionConsumer(LF2Entity entity)
        {
            if (!aiUnifiedSnapshotExecutionCommittedThisPass || entity == null)
                return;

            aiUnifiedSnapshotExecutionConsumerStartedThisPass = true;
            if (entity.AiControlled)
            {
                AiUnifiedSnapshotExecutionReadCountForDiagnostics++;
#if UNITY_INCLUDE_TESTS
                int observerSlot = entity.Runtime?.SlotIndex ?? -1;
                AiUnifiedSnapshotExecutionState published =
                    aiUnifiedSnapshotPublishedState;
                if (published != null &&
                    observerSlot ==
                        aiUnifiedSnapshotExecutionProbeObserverSlotAForSelfCheck)
                {
                    int target =
                        aiUnifiedSnapshotExecutionProbeTargetSlotAForSelfCheck;
                    aiUnifiedSnapshotExecutionProbeStateAForSelfCheck =
                        target >= 0 && target < published.Capacity &&
                        published.Rows.Included[target]
                            ? published.Rows.State[target]
                            : int.MinValue;
                }
                if (published != null &&
                    observerSlot ==
                        aiUnifiedSnapshotExecutionProbeObserverSlotBForSelfCheck)
                {
                    int target =
                        aiUnifiedSnapshotExecutionProbeTargetSlotBForSelfCheck;
                    aiUnifiedSnapshotExecutionProbeStateBForSelfCheck =
                        target >= 0 && target < published.Capacity &&
                        published.Rows.Included[target]
                            ? published.Rows.State[target]
                            : int.MinValue;
                }
#endif
            }
        }

        private bool AiUnifiedSnapshotExecutionOwnsCurrentPass =>
            aiUnifiedSnapshotExecutionCommittedThisPass;

        private bool AiUnifiedSnapshotExecutionFallbackForbidden =>
            aiUnifiedSnapshotExecutionCommittedThisPass ||
            aiUnifiedSnapshotExecutionConsumerStartedThisPass;

        private void PrepareAiUnifiedSnapshotShadowPass(
            BattleAiInputDetailDiagnostics diagnostics)
        {
            if (aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Shadow)
                return;

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Prepare;
            try
            {
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                PrepareAiUnifiedSnapshotShadowPassCore(diagnostics, ref stage);
            }
            catch (Exception exception)
            {
                RecordAiUnifiedSnapshotShadowException(stage, exception);
                InvalidateAiUnifiedSnapshotShadowPass();
            }
        }

        private void PrepareAiUnifiedSnapshotShadowPassCore(
            BattleAiInputDetailDiagnostics diagnostics,
            ref AiUnifiedSnapshotExceptionStage stage)
        {

            AiUnifiedSnapshotShadowBuildCountForDiagnostics++;
            aiUnifiedSnapshotPassAvailable = false;
            aiUnifiedSnapshotPassFailureRecorded = false;
            aiUnifiedSnapshotProductsComparedThisPass = false;
            aiUnifiedSnapshotPassEpoch = RuntimeSlotOccupancyEpochForServices;
            int capacity = RuntimeSlotCapacity;
            if (capacity <= 0)
            {
                InvalidateAiUnifiedSnapshotShadowPass();
                return;
            }

            EnsureAiUnifiedSnapshotCapacity(capacity);
            AiSoASensingRows rows = aiUnifiedSnapshotRows;
            rows.Reset(aiUnifiedSnapshotPassEpoch);
            Array.Clear(aiUnifiedSnapshotSoASensingBoundaryFlags, 0, capacity);
            Array.Clear(aiUnifiedSnapshotDecisionBoundaryFlags, 0, capacity);
            Array.Clear(aiUnifiedSnapshotFallbackSlots, 0, capacity);
            ResetAiUnifiedMoveModeFirst10Snapshot();

            bool captureAvailable = true;
            int visitedSlots = 0;
            stage = AiUnifiedSnapshotExceptionStage.Capture;
            ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
            diagnostics?.BeginPhase(
                BattleAiInputDetailPhase.SnapshotUnifiedDuplicateCapture);
            try
            {
                for (int slot = 0; slot < capacity; slot++)
                {
                    visitedSlots++;
                    AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics++;
                    if (!TryGetRuntimeSlotReadOnlyView(
                            slot,
                            out RuntimeSlotTable.ReadOnlySlotView view) ||
                        view.RuntimeSlot != slot)
                    {
                        captureAvailable = false;
                        break;
                    }
                    if (!view.Claimed)
                    {
                        if (view.Entity != null)
                        {
                            captureAvailable = false;
                            break;
                        }
                        continue;
                    }

                    LF2Entity entity = view.Entity;
                    NTSDEntityRuntime runtime = entity?.Runtime;
                    if (view.Generation == 0 ||
                        entity == null ||
                        runtime == null ||
                        runtime.SlotIndex != slot)
                    {
                        captureAvailable = false;
                        break;
                    }
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    aiUnifiedSnapshotFallbackSlots[slot] = entity;
                    if (slot < aiUnifiedMoveModeFirst10Present.Length)
                    {
                        CaptureAiUnifiedMoveModeFirst10Candidate(
                            slot,
                            entity,
                            view.Generation);
                    }
                    if (!TryCaptureAiSoASensingRow(
                            rows,
                            entity,
                            slot,
                            view.Generation,
                            true))
                    {
                        captureAvailable = false;
                        break;
                    }

                    int sensingFlags = CaptureAiSoASensingBoundaryFlags(runtime);
                    int decisionFlags = CaptureAiDecisionBoundaryFlags(runtime);
                    aiUnifiedSnapshotSoASensingBoundaryFlags[slot] = sensingFlags;
                    aiUnifiedSnapshotDecisionBoundaryFlags[slot] = decisionFlags;
                    if (sensingFlags != decisionFlags)
                    {
                        AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics++;
                    }
                }
            }
            finally
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateCapture,
                    visitedSlots);
                diagnostics?.EndPhase(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateCapture);
            }

            if (!captureAvailable ||
                RuntimeSlotCapacity != capacity ||
                RuntimeSlotOccupancyEpochForServices != aiUnifiedSnapshotPassEpoch)
            {
                InvalidateAiUnifiedSnapshotShadowPass();
                return;
            }
            aiUnifiedMoveModeFirst10Valid = true;

            stage = AiUnifiedSnapshotExceptionStage.BuildIndexes;
            ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
            diagnostics?.BeginPhase(
                BattleAiInputDetailPhase.SnapshotUnifiedDuplicateIndexBuild);
            try
            {
                rows.SpecialIndexReady = true;
                BuildAiSoASensingRoleIndexes(rows);
                BuildAiSoASensingTeamSummaries(rows);
            }
            finally
            {
                diagnostics?.EndPhase(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateIndexBuild);
            }
            stage = AiUnifiedSnapshotExceptionStage.Validate;
            ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
            if (!AiSensingKernel.ValidateIndexedContract(rows))
            {
                InvalidateAiUnifiedSnapshotShadowPass();
                return;
            }

            aiUnifiedSnapshotMutationWitnessOrdinal = 0;
            aiUnifiedSnapshotRoleIndexVersion = 1;
            aiUnifiedSnapshotTeamSummaryVersion = 1;
            aiUnifiedSnapshotMutationWitness = default;
            aiUnifiedSnapshotPassAvailable = true;
            stage = AiUnifiedSnapshotExceptionStage.InitialSensingCompare;
            ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
            CompareAiUnifiedSnapshotShadow(
                AiUnifiedSnapshotConsumer.SoASensing,
                true,
                -1);
        }

        private void EnsureAiUnifiedSnapshotCapacity(int capacity)
        {
            if (aiUnifiedSnapshotRows != null &&
                aiUnifiedSnapshotRows.Capacity == capacity &&
                aiUnifiedSnapshotFallbackSlots != null &&
                aiUnifiedSnapshotFallbackSlots.Length == capacity)
            {
                return;
            }

            aiUnifiedSnapshotRows = new AiSoASensingRows(capacity);
            aiUnifiedSnapshotSoASensingBoundaryFlags = new int[capacity];
            aiUnifiedSnapshotDecisionBoundaryFlags = new int[capacity];
            aiUnifiedSnapshotFallbackSlots = new LF2Entity[capacity];
        }

        private void CompleteAiUnifiedSnapshotShadowInitialComparison()
        {
            if (aiUnifiedSnapshotExecutionCommittedThisPass)
            {
                AiUnifiedSnapshotExceptionStage executionStage =
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare;
                try
                {
                    ThrowAiUnifiedSnapshotExceptionForSelfCheck(executionStage);
                }
                catch (Exception exception)
                {
                    RecordAiUnifiedSnapshotExecutionFailure(
                        executionStage,
                        exception,
                        true);
                    aiUnifiedSnapshotExecutionCommittedThisPass = false;
                    aiSoASensingPassInvalidated = true;
                    InvalidateAiDecisionSharedPass(
                        AiDecisionAvailability.SnapshotMissing);
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority failed at the initial decision boundary; same-tick fallback is forbidden.",
                        exception);
                }
                return;
            }
            if (aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Shadow ||
                !AiDecisionRequiresSharedRows)
            {
                return;
            }

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.InitialDecisionCompare;
            try
            {
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                CompareAiUnifiedSnapshotShadow(
                    AiUnifiedSnapshotConsumer.IndexedDecision,
                    true,
                    -1);
            }
            catch (Exception exception)
            {
                RecordAiUnifiedSnapshotShadowException(stage, exception);
                InvalidateAiUnifiedSnapshotShadowPass();
            }
        }

        private void RefreshAiUnifiedSnapshotShadowRowAfterCharacterInput(LF2Entity entity)
        {
            if (aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Shadow ||
                !aiUnifiedSnapshotPassAvailable)
            {
                return;
            }

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Refresh;
            try
            {
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                RefreshAiUnifiedSnapshotShadowRowAfterCharacterInputCore(
                    entity,
                    ref stage);
            }
            catch (Exception exception)
            {
                RecordAiUnifiedSnapshotShadowException(stage, exception);
                InvalidateAiUnifiedSnapshotShadowPass();
            }
        }

        private void RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput(
            LF2Entity entity)
        {
            if (!aiUnifiedSnapshotExecutionCommittedThisPass)
                return;

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Refresh;
            try
            {
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                AiUnifiedSnapshotExecutionState published =
                    aiUnifiedSnapshotPublishedState;
                if (published == null)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority has no published state.");
                }
                if (RuntimeSlotOccupancyEpochForServices !=
                        published.Epoch ||
                    entity?.Runtime == null)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority epoch changed after commit.");
                }

                int slot = entity.Runtime.SlotIndex;
                AiSoASensingRows rows = published.Rows;
                if (slot < 0 ||
                    slot >= rows.Capacity ||
                    !rows.Included[slot] ||
                    !TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                    rows.Generation[slot] != handle.Generation ||
                    rows.Identity[slot] != entity.Runtime.StableId)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority row identity changed after commit.");
                }

                int previousX = rows.X[slot];
                int previousTeam = rows.Team[slot];
                int previousHp = rows.Hp[slot];
                int previousObjectId = rows.ObjectId[slot];
                bool previousSpecialMember = rows.SpecialScanMember[slot];
                bool wasGroundRole = IsGroundAiSoARoleMember(rows, slot);
                bool wasAirRole = IsAirAiSoARoleMember(rows, slot);
                bool wasLivingCharacter = IsLivingCharacterAiSoARow(rows, slot);
                stage = AiUnifiedSnapshotExceptionStage.RefreshCapture;
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                if (!TryCaptureAiSoASensingRow(
                        rows,
                        entity,
                        slot,
                        handle.Generation,
                        false,
                        true))
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority could not refresh a committed row.");
                }

                bool currentSpecialMember =
                    slot >= 20 && IsAiSpecialScanObjectId(rows.ObjectId[slot]);
                if (previousObjectId != rows.ObjectId[slot] ||
                    previousSpecialMember != currentSpecialMember)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority special membership changed after commit.");
                }

                int sensingFlags = CaptureAiSoASensingBoundaryFlags(entity.Runtime);
                int decisionFlags = CaptureAiDecisionBoundaryFlags(entity.Runtime);
                rows.BoundaryFlags[slot] = sensingFlags;
                published.SoASensingBoundaryFlags[slot] = sensingFlags;
                published.DecisionBoundaryFlags[slot] = decisionFlags;

                bool isGroundRole = IsGroundAiSoARoleMember(rows, slot);
                bool isAirRole = IsAirAiSoARoleMember(rows, slot);
                bool roleProductsChanged = previousX != rows.X[slot] ||
                                           previousTeam != rows.Team[slot] ||
                                           wasGroundRole != isGroundRole ||
                                           wasAirRole != isAirRole;
                bool isLivingCharacter = IsLivingCharacterAiSoARow(rows, slot);
                bool teamProductsChanged =
                    wasLivingCharacter != isLivingCharacter ||
                    previousTeam != rows.Team[slot] ||
                    previousHp != rows.Hp[slot];
                stage = AiUnifiedSnapshotExceptionStage.RefreshBuildIndexes;
                ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
                if (roleProductsChanged)
                    BuildAiSoASensingRoleIndexes(rows);
                if (teamProductsChanged)
                    BuildAiSoASensingTeamSummaries(rows);

                RecordAiUnifiedSnapshotMutationWitness(
                    published.Epoch,
                    slot,
                    handle.Generation,
                    entity.Runtime.StableId,
                    roleProductsChanged,
                    teamProductsChanged,
                    previousX,
                    rows.X[slot],
                    previousTeam,
                    rows.Team[slot],
                    PackAiUnifiedSnapshotRoleFlags(wasGroundRole, wasAirRole),
                    PackAiUnifiedSnapshotRoleFlags(isGroundRole, isAirRole),
                    wasLivingCharacter,
                    isLivingCharacter,
                    previousHp,
                    rows.Hp[slot]);
                ObserveAiUnifiedSnapshotExecutionMoveModeFirst10Mutation(
                    published,
                    entity);
                AiUnifiedSnapshotExecutionRefreshCountForDiagnostics++;
            }
            catch (Exception exception)
            {
                RecordAiUnifiedSnapshotExecutionFailure(stage, exception, true);
                aiUnifiedSnapshotExecutionCommittedThisPass = false;
                aiSoASensingPassInvalidated = true;
                InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
                throw new InvalidOperationException(
                    "Unified AI snapshot authority hard breach after commit; same-tick fallback is forbidden.",
                    exception);
            }
        }

        private void RefreshAiUnifiedSnapshotShadowRowAfterCharacterInputCore(
            LF2Entity entity,
            ref AiUnifiedSnapshotExceptionStage stage)
        {
            if (RuntimeSlotOccupancyEpochForServices != aiUnifiedSnapshotPassEpoch ||
                entity?.Runtime == null)
            {
                InvalidateAiUnifiedSnapshotShadowPass();
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            AiSoASensingRows rows = aiUnifiedSnapshotRows;
            if (slot < 0 ||
                slot >= rows.Capacity ||
                !rows.Included[slot] ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                rows.Generation[slot] != handle.Generation ||
                rows.Identity[slot] != entity.Runtime.StableId)
            {
                InvalidateAiUnifiedSnapshotShadowPass();
                return;
            }

            int previousX = rows.X[slot];
            int previousTeam = rows.Team[slot];
            int previousHp = rows.Hp[slot];
            int previousObjectId = rows.ObjectId[slot];
            bool previousSpecialMember = rows.SpecialScanMember[slot];
            bool wasGroundRole = IsGroundAiSoARoleMember(rows, slot);
            bool wasAirRole = IsAirAiSoARoleMember(rows, slot);
            bool wasLivingCharacter = IsLivingCharacterAiSoARow(rows, slot);
            stage = AiUnifiedSnapshotExceptionStage.RefreshCapture;
            ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
            if (!TryCaptureAiSoASensingRow(
                    rows,
                    entity,
                    slot,
                    handle.Generation,
                    false))
            {
                InvalidateAiUnifiedSnapshotShadowPass();
                return;
            }

            bool currentSpecialMember =
                slot >= 20 && IsAiSpecialScanObjectId(rows.ObjectId[slot]);
            if (previousObjectId != rows.ObjectId[slot] ||
                previousSpecialMember != currentSpecialMember)
            {
                InvalidateAiUnifiedSnapshotShadowPass();
                return;
            }

            int sensingFlags = CaptureAiSoASensingBoundaryFlags(entity.Runtime);
            int decisionFlags = CaptureAiDecisionBoundaryFlags(entity.Runtime);
            aiUnifiedSnapshotSoASensingBoundaryFlags[slot] = sensingFlags;
            aiUnifiedSnapshotDecisionBoundaryFlags[slot] = decisionFlags;
            if (sensingFlags != decisionFlags)
            {
                AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics++;
            }

            bool isGroundRole = IsGroundAiSoARoleMember(rows, slot);
            bool isAirRole = IsAirAiSoARoleMember(rows, slot);
            bool roleProductsChanged = previousX != rows.X[slot] ||
                                       previousTeam != rows.Team[slot] ||
                                       wasGroundRole != isGroundRole ||
                                       wasAirRole != isAirRole;
            bool isLivingCharacter = IsLivingCharacterAiSoARow(rows, slot);
            bool teamProductsChanged = wasLivingCharacter != isLivingCharacter ||
                                       previousTeam != rows.Team[slot] ||
                                       previousHp != rows.Hp[slot];
            stage = AiUnifiedSnapshotExceptionStage.RefreshBuildIndexes;
            ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
            if (roleProductsChanged)
            {
                BuildAiSoASensingRoleIndexes(rows);
            }

            if (teamProductsChanged)
            {
                BuildAiSoASensingTeamSummaries(rows);
            }

            RecordAiUnifiedSnapshotMutationWitness(
                aiUnifiedSnapshotPassEpoch,
                slot,
                handle.Generation,
                entity.Runtime.StableId,
                roleProductsChanged,
                teamProductsChanged,
                previousX,
                rows.X[slot],
                previousTeam,
                rows.Team[slot],
                PackAiUnifiedSnapshotRoleFlags(wasGroundRole, wasAirRole),
                PackAiUnifiedSnapshotRoleFlags(isGroundRole, isAirRole),
                wasLivingCharacter,
                isLivingCharacter,
                previousHp,
                rows.Hp[slot]);
            ObserveAiUnifiedMoveModeFirst10Mutation(entity);
            AiUnifiedSnapshotShadowRefreshCountForDiagnostics++;
            stage = AiUnifiedSnapshotExceptionStage.RefreshCompare;
            ThrowAiUnifiedSnapshotExceptionForSelfCheck(stage);
            CompareAiUnifiedSnapshotShadow(
                AiUnifiedSnapshotConsumer.SoASensing,
                false,
                slot);
            CompareAiUnifiedSnapshotShadow(
                AiUnifiedSnapshotConsumer.IndexedDecision,
                false,
                slot);
        }

        private void EndAiUnifiedSnapshotShadowPass()
        {
            aiUnifiedSnapshotPassAvailable = false;
            aiUnifiedSnapshotPassFailureRecorded = false;
            aiUnifiedSnapshotProductsComparedThisPass = false;
            aiUnifiedSnapshotRefreshComparisonActive = false;
            aiUnifiedSnapshotPassEpoch = 0;
        }

        private void InvalidateAiUnifiedSnapshotShadowPass()
        {
            if (!aiUnifiedSnapshotPassFailureRecorded)
            {
                AiUnifiedSnapshotShadowUnavailableCountForDiagnostics++;
                aiUnifiedSnapshotPassFailureRecorded = true;
            }
            aiUnifiedSnapshotPassAvailable = false;
        }

        private void CompareAiUnifiedSnapshotShadow(
            AiUnifiedSnapshotConsumer consumer,
            bool fullComparison,
            int refreshSlot)
        {
            if (!aiUnifiedSnapshotPassAvailable)
                return;

            AiSoASensingRows productionRows;
            int[] unifiedBoundaryFlags;
            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    if (aiSensingMode != AiSensingMode.SoAShadowAiSensing &&
                        aiSensingMode != AiSensingMode.SoAAiSensing)
                    {
                        return;
                    }
                    if (!aiSoASensingSnapshotValid || aiSoASensingPassInvalidated)
                    {
                        InvalidateAiUnifiedSnapshotShadowPass();
                        return;
                    }
                    productionRows = aiSoASensingRows;
                    unifiedBoundaryFlags = aiUnifiedSnapshotSoASensingBoundaryFlags;
                    break;
                case AiUnifiedSnapshotConsumer.IndexedDecision:
                    if (!AiDecisionRequiresSharedRows)
                        return;
                    if (!aiDecisionSharedPassAvailable)
                    {
                        InvalidateAiUnifiedSnapshotShadowPass();
                        return;
                    }
                    productionRows = aiDecisionSharedRows;
                    unifiedBoundaryFlags = aiUnifiedSnapshotDecisionBoundaryFlags;
                    break;
                default:
                    return;
            }

            int mutatedSlot = -1;
            int originalBoundaryFlags = 0;
#if UNITY_INCLUDE_TESTS
            if (aiUnifiedSnapshotBoundaryMutationConsumerForSelfCheck == consumer &&
                aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck >= 0 &&
                aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck < productionRows.Capacity)
            {
                mutatedSlot = aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck;
                originalBoundaryFlags = productionRows.BoundaryFlags[mutatedSlot];
                productionRows.BoundaryFlags[mutatedSlot] ^=
                    aiUnifiedSnapshotBoundaryMutationXorForSelfCheck;
                aiUnifiedSnapshotBoundaryMutationConsumerForSelfCheck =
                    AiUnifiedSnapshotConsumer.None;
                aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck = -1;
                aiUnifiedSnapshotBoundaryMutationXorForSelfCheck = 0;
            }
#endif

            bool matches;
            AiUnifiedSnapshotMismatch mismatch;
            try
            {
                if (fullComparison)
                {
                    matches = TryCompareAiUnifiedSnapshotRows(
                        productionRows,
                        unifiedBoundaryFlags,
                        consumer,
                        out mismatch);
                }
                else
                {
                    aiUnifiedSnapshotRefreshComparisonActive = true;
                    try
                    {
                        matches = TryCompareAiUnifiedSnapshotRefresh(
                            productionRows,
                            unifiedBoundaryFlags,
                            consumer,
                            refreshSlot,
                            out mismatch);
                    }
                    finally
                    {
                        aiUnifiedSnapshotRefreshComparisonActive = false;
                    }
                }
            }
            finally
            {
                if (mutatedSlot >= 0)
                    productionRows.BoundaryFlags[mutatedSlot] = originalBoundaryFlags;
            }

            if (consumer == AiUnifiedSnapshotConsumer.SoASensing)
                AiUnifiedSnapshotShadowSensingComparedCountForDiagnostics++;
            else
                AiUnifiedSnapshotShadowDecisionComparedCountForDiagnostics++;
            if (matches)
                return;

            AiUnifiedSnapshotShadowMismatchCountForDiagnostics++;
            if (AiUnifiedSnapshotShadowFirstMismatchForDiagnostics.Kind ==
                AiUnifiedSnapshotMismatchKind.None)
            {
                AiUnifiedSnapshotShadowFirstMismatchForDiagnostics = mismatch;
            }
        }

        private bool TryCompareAiUnifiedSnapshotRows(
            AiSoASensingRows production,
            int[] unifiedBoundaryFlags,
            AiUnifiedSnapshotConsumer consumer,
            out AiUnifiedSnapshotMismatch mismatch)
        {
            mismatch = default;
            AiSoASensingRows unified = aiUnifiedSnapshotRows;
            if (production == null || unified == null || unifiedBoundaryFlags == null)
            {
                return SetAiUnifiedSnapshotMismatch(
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Capacity,
                    AiUnifiedSnapshotField.None,
                    -1,
                    production?.Capacity ?? -1,
                    unified?.Capacity ?? -1,
                    ref mismatch);
            }
            if (!MatchAiUnifiedSnapshotValue(
                    unchecked((long)production.CapturedOccupancyEpoch),
                    unchecked((long)unified.CapturedOccupancyEpoch),
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Epoch,
                    AiUnifiedSnapshotField.None,
                    -1,
                    ref mismatch) ||
                !MatchAiUnifiedSnapshotValue(
                    production.Capacity,
                    unified.Capacity,
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Capacity,
                    AiUnifiedSnapshotField.None,
                    -1,
                    ref mismatch))
            {
                return false;
            }

            int capacity = production.Capacity;
            for (int slot = 0; slot < capacity; slot++)
            {
                AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics++;
                if (!MatchAiUnifiedSnapshotValue(
                        production.Included[slot] ? 1 : 0,
                        unified.Included[slot] ? 1 : 0,
                        consumer,
                        AiUnifiedSnapshotMismatchKind.Included,
                        AiUnifiedSnapshotField.None,
                        slot,
                        ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(
                        production.SpecialScanMember[slot] ? 1 : 0,
                        unified.SpecialScanMember[slot] ? 1 : 0,
                        consumer,
                        AiUnifiedSnapshotMismatchKind.SpecialMembership,
                        AiUnifiedSnapshotField.None,
                        slot,
                        ref mismatch))
                {
                    return false;
                }
                if (!production.Included[slot])
                    continue;

                if (!MatchAiUnifiedSnapshotValue(production.Generation[slot], unified.Generation[slot], consumer, AiUnifiedSnapshotMismatchKind.Generation, AiUnifiedSnapshotField.None, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Identity[slot], unified.Identity[slot], consumer, AiUnifiedSnapshotMismatchKind.Identity, AiUnifiedSnapshotField.None, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.InputHistoryGate[slot] ? 1 : 0, unified.InputHistoryGate[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.InputHistoryGate, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.ObjectId[slot], unified.ObjectId[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.ObjectId, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.DataObjectType[slot], unified.DataObjectType[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.DataObjectType, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.X[slot], unified.X[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.X, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Y[slot], unified.Y[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Y, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Z[slot], unified.Z[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Z, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Hp[slot], unified.Hp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Hp3[slot], unified.Hp3[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp3, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.HpMax[slot], unified.HpMax[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HpMax, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Pp[slot], unified.Pp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Pp, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Team[slot], unified.Team[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Team, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.State[slot], unified.State[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.State, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Frame[slot], unified.Frame[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Frame, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.LinkState[slot], unified.LinkState[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.LinkState, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.KillCount[slot], unified.KillCount[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.KillCount, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.CachedTargetSlot[slot], unified.CachedTargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CachedTargetSlot, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.CoordinateTargetX[slot], unified.CoordinateTargetX[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CoordinateTargetX, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(BitConverter.DoubleToInt64Bits(production.Vx[slot]), BitConverter.DoubleToInt64Bits(unified.Vx[slot]), consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.VxBits, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.Facing[slot], unified.Facing[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Facing, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.TargetSlot[slot], unified.TargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.TargetSlot, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.HitStop[slot], unified.HitStop[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HitStop, slot, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.BoundaryFlags[slot], unifiedBoundaryFlags[slot], consumer, AiUnifiedSnapshotMismatchKind.BoundaryFlags, AiUnifiedSnapshotField.None, slot, ref mismatch))
                {
                    return false;
                }
            }

            if (!TryCompareAiUnifiedSnapshotIndexes(
                    production,
                    unified,
                    consumer,
                    true,
                    true,
                    true,
                    ref mismatch))
            {
                return false;
            }
            if (aiUnifiedSnapshotProductsComparedThisPass)
                return true;

            bool productsMatch = TryCompareAiUnifiedSnapshotCandidateProducts(
                consumer,
                true,
                -1,
                ref mismatch);
            if (productsMatch)
                aiUnifiedSnapshotProductsComparedThisPass = true;
            return productsMatch;
        }

        private bool TryCompareAiUnifiedSnapshotRefresh(
            AiSoASensingRows production,
            int[] unifiedBoundaryFlags,
            AiUnifiedSnapshotConsumer consumer,
            int slot,
            out AiUnifiedSnapshotMismatch mismatch)
        {
            mismatch = default;
            AiSoASensingRows unified = aiUnifiedSnapshotRows;
            if (production == null || unified == null || unifiedBoundaryFlags == null)
            {
                return SetAiUnifiedSnapshotMismatch(
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Capacity,
                    AiUnifiedSnapshotField.None,
                    -1,
                    production?.Capacity ?? -1,
                    unified?.Capacity ?? -1,
                    ref mismatch);
            }
            if (!MatchAiUnifiedSnapshotValue(
                    unchecked((long)production.CapturedOccupancyEpoch),
                    unchecked((long)unified.CapturedOccupancyEpoch),
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Epoch,
                    AiUnifiedSnapshotField.None,
                    -1,
                    ref mismatch) ||
                !MatchAiUnifiedSnapshotValue(
                    production.Capacity,
                    unified.Capacity,
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Capacity,
                    AiUnifiedSnapshotField.None,
                    -1,
                    ref mismatch) ||
                slot < 0 ||
                slot >= production.Capacity)
            {
                return false;
            }

            AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics++;
            if (!TryCompareAiUnifiedSnapshotRow(
                    production,
                    unified,
                    unifiedBoundaryFlags,
                    consumer,
                    slot,
                    ref mismatch))
            {
                return false;
            }

            if (!TryCompareAiUnifiedSnapshotCandidateProducts(
                    consumer,
                    false,
                    slot,
                    ref mismatch))
            {
                return false;
            }

            return TryCompareAiUnifiedSnapshotMutationWitness(
                consumer,
                ref mismatch);
        }

        private static bool TryCompareAiUnifiedSnapshotRow(
            AiSoASensingRows production,
            AiSoASensingRows unified,
            int[] unifiedBoundaryFlags,
            AiUnifiedSnapshotConsumer consumer,
            int slot,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            if (!MatchAiUnifiedSnapshotValue(production.Included[slot] ? 1 : 0, unified.Included[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.Included, AiUnifiedSnapshotField.None, slot, ref mismatch) ||
                !MatchAiUnifiedSnapshotValue(production.SpecialScanMember[slot] ? 1 : 0, unified.SpecialScanMember[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.SpecialMembership, AiUnifiedSnapshotField.None, slot, ref mismatch))
            {
                return false;
            }
            if (!production.Included[slot])
                return true;

            return MatchAiUnifiedSnapshotValue(production.Generation[slot], unified.Generation[slot], consumer, AiUnifiedSnapshotMismatchKind.Generation, AiUnifiedSnapshotField.None, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Identity[slot], unified.Identity[slot], consumer, AiUnifiedSnapshotMismatchKind.Identity, AiUnifiedSnapshotField.None, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.InputHistoryGate[slot] ? 1 : 0, unified.InputHistoryGate[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.InputHistoryGate, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.ObjectId[slot], unified.ObjectId[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.ObjectId, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.DataObjectType[slot], unified.DataObjectType[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.DataObjectType, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.X[slot], unified.X[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.X, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Y[slot], unified.Y[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Y, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Z[slot], unified.Z[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Z, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Hp[slot], unified.Hp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Hp3[slot], unified.Hp3[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp3, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.HpMax[slot], unified.HpMax[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HpMax, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Pp[slot], unified.Pp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Pp, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Team[slot], unified.Team[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Team, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.State[slot], unified.State[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.State, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Frame[slot], unified.Frame[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Frame, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.LinkState[slot], unified.LinkState[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.LinkState, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.KillCount[slot], unified.KillCount[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.KillCount, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.CachedTargetSlot[slot], unified.CachedTargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CachedTargetSlot, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.CoordinateTargetX[slot], unified.CoordinateTargetX[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CoordinateTargetX, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(BitConverter.DoubleToInt64Bits(production.Vx[slot]), BitConverter.DoubleToInt64Bits(unified.Vx[slot]), consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.VxBits, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Facing[slot], unified.Facing[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Facing, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.TargetSlot[slot], unified.TargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.TargetSlot, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.HitStop[slot], unified.HitStop[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HitStop, slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.BoundaryFlags[slot], unifiedBoundaryFlags[slot], consumer, AiUnifiedSnapshotMismatchKind.BoundaryFlags, AiUnifiedSnapshotField.None, slot, ref mismatch);
        }

        private bool TryCompareAiUnifiedSnapshotCandidateProducts(
            AiUnifiedSnapshotConsumer consumer,
            bool fullComparison,
            int refreshSlot,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            int firstSlot = fullComparison ? 0 : refreshSlot;
            int lastSlot = fullComparison
                ? aiUnifiedSnapshotFallbackSlots.Length - 1
                : refreshSlot;
            for (int slot = firstSlot; slot <= lastSlot; slot++)
            {
#if UNITY_INCLUDE_TESTS
                if (aiUnifiedSnapshotProductMutationKindForSelfCheck ==
                        AiUnifiedSnapshotProductMutationKind.FallbackReference &&
                    aiUnifiedSnapshotProductMutationSlotForSelfCheck == slot)
                {
                    aiUnifiedSnapshotProductMutationKindForSelfCheck =
                        AiUnifiedSnapshotProductMutationKind.None;
                    aiUnifiedSnapshotProductMutationSlotForSelfCheck = -1;
                    return SetAiUnifiedSnapshotMismatch(
                        consumer,
                        AiUnifiedSnapshotMismatchKind.FallbackReference,
                        AiUnifiedSnapshotField.FallbackSlot,
                        slot,
                        aiInputSlots[slot]?.Runtime?.StableId ?? 0,
                        -1,
                        ref mismatch);
                }
#endif
                if (!ReferenceEquals(aiInputSlots[slot], aiUnifiedSnapshotFallbackSlots[slot]))
                {
                    return SetAiUnifiedSnapshotMismatch(
                        consumer,
                        AiUnifiedSnapshotMismatchKind.FallbackReference,
                        AiUnifiedSnapshotField.FallbackSlot,
                        slot,
                        aiInputSlots[slot]?.Runtime?.StableId ?? 0,
                        aiUnifiedSnapshotFallbackSlots[slot]?.Runtime?.StableId ?? 0,
                        ref mismatch);
                }
            }

            if (!MatchAiUnifiedSnapshotValue(aiMoveModeFirst10Valid ? 1 : 0, aiUnifiedMoveModeFirst10Valid ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeValid, -1, ref mismatch))
                return false;
            if (!aiMoveModeFirst10Valid)
                return true;

            int firstMoveSlot = fullComparison ? 0 : refreshSlot;
            int lastMoveSlot = fullComparison
                ? aiMoveModeFirst10Present.Length - 1
                : refreshSlot;
            if (firstMoveSlot < aiMoveModeFirst10Present.Length)
            {
                for (int slot = firstMoveSlot; slot <= lastMoveSlot; slot++)
                {
#if UNITY_INCLUDE_TESTS
                    if (aiUnifiedSnapshotProductMutationKindForSelfCheck ==
                            AiUnifiedSnapshotProductMutationKind.MoveModeFirst10Hp &&
                        aiUnifiedSnapshotProductMutationSlotForSelfCheck == slot)
                    {
                        aiUnifiedSnapshotProductMutationKindForSelfCheck =
                            AiUnifiedSnapshotProductMutationKind.None;
                        aiUnifiedSnapshotProductMutationSlotForSelfCheck = -1;
                        return SetAiUnifiedSnapshotMismatch(
                            consumer,
                            AiUnifiedSnapshotMismatchKind.MoveModeProduct,
                            AiUnifiedSnapshotField.MoveModeHp,
                            slot,
                            aiMoveModeFirst10Hp[slot],
                            aiUnifiedMoveModeFirst10Hp[slot] + 1L,
                            ref mismatch);
                    }
#endif
                    if (!MatchAiUnifiedSnapshotValue(aiMoveModeFirst10Present[slot] ? 1 : 0, aiUnifiedMoveModeFirst10Present[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModePresent, slot, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(aiMoveModeFirst10Eligible[slot] ? 1 : 0, aiUnifiedMoveModeFirst10Eligible[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeEligible, slot, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(aiMoveModeFirst10Generation[slot], aiUnifiedMoveModeFirst10Generation[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeGeneration, slot, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(aiMoveModeFirst10Hp[slot], aiUnifiedMoveModeFirst10Hp[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeHp, slot, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(aiMoveModeFirst10X[slot], aiUnifiedMoveModeFirst10X[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeX, slot, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(aiMoveModeFirst10Z[slot], aiUnifiedMoveModeFirst10Z[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeZ, slot, ref mismatch))
                    {
                        return false;
                    }
                }
            }

            return MatchAiUnifiedSnapshotValue(aiMoveModeTopSlot, aiUnifiedMoveModeTopSlot, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeTopSlot, -1, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(aiMoveModeTopX, aiUnifiedMoveModeTopX, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeTopX, -1, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(aiMoveModeTopZ, aiUnifiedMoveModeTopZ, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeTopZ, -1, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(aiMoveModeSecondSlot, aiUnifiedMoveModeSecondSlot, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeSecondSlot, -1, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(aiMoveModeSecondX, aiUnifiedMoveModeSecondX, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeSecondX, -1, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(aiMoveModeSecondZ, aiUnifiedMoveModeSecondZ, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeSecondZ, -1, ref mismatch);
        }

        private void BeginAiUnifiedSnapshotProductionMutationWitnessPass(
            AiUnifiedSnapshotConsumer consumer,
            ulong epoch)
        {
            if (aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Shadow)
                return;

            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    aiSoASensingMutationWitnessOrdinal = 0;
                    aiSoASensingRoleIndexVersion = 1;
                    aiSoASensingTeamSummaryVersion = 1;
                    aiSoASensingMutationWitness = default;
                    aiSoASensingMutationWitness.Epoch = epoch;
                    break;
                case AiUnifiedSnapshotConsumer.IndexedDecision:
                    aiDecisionMutationWitnessOrdinal = 0;
                    aiDecisionRoleIndexVersion = 1;
                    aiDecisionTeamSummaryVersion = 1;
                    aiDecisionMutationWitness = default;
                    aiDecisionMutationWitness.Epoch = epoch;
                    break;
            }
        }

        private void RecordAiUnifiedSnapshotProductionMutationWitness(
            AiUnifiedSnapshotConsumer consumer,
            ulong epoch,
            int slot,
            uint generation,
            int stableId,
            bool roleRebuilt,
            bool teamRebuilt,
            int oldX,
            int newX,
            int oldTeam,
            int newTeam,
            int oldRoleFlags,
            int newRoleFlags,
            bool oldLiving,
            bool newLiving,
            int oldHp,
            int newHp)
        {
            if (aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Shadow)
                return;

            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    if (roleRebuilt)
                        aiSoASensingRoleIndexVersion++;
                    if (teamRebuilt)
                        aiSoASensingTeamSummaryVersion++;
                    aiSoASensingMutationWitnessOrdinal++;
                    aiSoASensingMutationWitness = CreateAiUnifiedSnapshotMutationWitness(
                        epoch,
                        aiSoASensingMutationWitnessOrdinal,
                        slot,
                        generation,
                        stableId,
                        roleRebuilt,
                        teamRebuilt,
                        aiSoASensingRoleIndexVersion,
                        aiSoASensingTeamSummaryVersion,
                        oldX,
                        newX,
                        oldTeam,
                        newTeam,
                        oldRoleFlags,
                        newRoleFlags,
                        oldLiving,
                        newLiving,
                        oldHp,
                        newHp);
                    break;
                case AiUnifiedSnapshotConsumer.IndexedDecision:
                    if (roleRebuilt)
                        aiDecisionRoleIndexVersion++;
                    if (teamRebuilt)
                        aiDecisionTeamSummaryVersion++;
                    aiDecisionMutationWitnessOrdinal++;
                    aiDecisionMutationWitness = CreateAiUnifiedSnapshotMutationWitness(
                        epoch,
                        aiDecisionMutationWitnessOrdinal,
                        slot,
                        generation,
                        stableId,
                        roleRebuilt,
                        teamRebuilt,
                        aiDecisionRoleIndexVersion,
                        aiDecisionTeamSummaryVersion,
                        oldX,
                        newX,
                        oldTeam,
                        newTeam,
                        oldRoleFlags,
                        newRoleFlags,
                        oldLiving,
                        newLiving,
                        oldHp,
                        newHp);
                    break;
            }
        }

        private void RecordAiUnifiedSnapshotMutationWitness(
            ulong epoch,
            int slot,
            uint generation,
            int stableId,
            bool roleRebuilt,
            bool teamRebuilt,
            int oldX,
            int newX,
            int oldTeam,
            int newTeam,
            int oldRoleFlags,
            int newRoleFlags,
            bool oldLiving,
            bool newLiving,
            int oldHp,
            int newHp)
        {
            if (roleRebuilt)
                aiUnifiedSnapshotRoleIndexVersion++;
            if (teamRebuilt)
                aiUnifiedSnapshotTeamSummaryVersion++;
            aiUnifiedSnapshotMutationWitnessOrdinal++;
            aiUnifiedSnapshotMutationWitness = CreateAiUnifiedSnapshotMutationWitness(
                epoch,
                aiUnifiedSnapshotMutationWitnessOrdinal,
                slot,
                generation,
                stableId,
                roleRebuilt,
                teamRebuilt,
                aiUnifiedSnapshotRoleIndexVersion,
                aiUnifiedSnapshotTeamSummaryVersion,
                oldX,
                newX,
                oldTeam,
                newTeam,
                oldRoleFlags,
                newRoleFlags,
                oldLiving,
                newLiving,
                oldHp,
                newHp);
        }

        private static AiUnifiedSnapshotMutationWitness
            CreateAiUnifiedSnapshotMutationWitness(
                ulong epoch,
                long ordinal,
                int slot,
                uint generation,
                int stableId,
                bool roleRebuilt,
                bool teamRebuilt,
                long roleVersion,
                long teamVersion,
                int oldX,
                int newX,
                int oldTeam,
                int newTeam,
                int oldRoleFlags,
                int newRoleFlags,
                bool oldLiving,
                bool newLiving,
                int oldHp,
                int newHp)
        {
            return new AiUnifiedSnapshotMutationWitness
            {
                Epoch = epoch,
                Ordinal = ordinal,
                Slot = slot,
                Generation = generation,
                StableId = stableId,
                RoleRebuilt = roleRebuilt,
                TeamRebuilt = teamRebuilt,
                RoleVersion = roleVersion,
                TeamVersion = teamVersion,
                OldX = oldX,
                NewX = newX,
                OldTeam = oldTeam,
                NewTeam = newTeam,
                OldRoleFlags = oldRoleFlags,
                NewRoleFlags = newRoleFlags,
                OldLiving = oldLiving,
                NewLiving = newLiving,
                OldHp = oldHp,
                NewHp = newHp,
            };
        }

        private bool TryCompareAiUnifiedSnapshotMutationWitness(
            AiUnifiedSnapshotConsumer consumer,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            AiUnifiedSnapshotMutationWitness production;
            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    production = aiSoASensingMutationWitness;
                    break;
                case AiUnifiedSnapshotConsumer.IndexedDecision:
                    production = aiDecisionMutationWitness;
                    break;
                default:
                    return false;
            }

            AiUnifiedSnapshotMutationWitness unified =
                aiUnifiedSnapshotMutationWitness;
#if UNITY_INCLUDE_TESTS
            if (aiUnifiedSnapshotWitnessMutationConsumerForSelfCheck == consumer)
            {
                unified.Ordinal++;
                aiUnifiedSnapshotWitnessMutationConsumerForSelfCheck =
                    AiUnifiedSnapshotConsumer.None;
            }
#endif
            AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics++;
            return MatchAiUnifiedSnapshotValue(unchecked((long)production.Epoch), unchecked((long)unified.Epoch), consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessEpoch, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Ordinal, unified.Ordinal, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOrdinal, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Slot, unified.Slot, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessSlot, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.Generation, unified.Generation, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessGeneration, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.StableId, unified.StableId, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessStableId, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.RoleRebuilt ? 1 : 0, unified.RoleRebuilt ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessRoleRebuilt, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.TeamRebuilt ? 1 : 0, unified.TeamRebuilt ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessTeamRebuilt, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.RoleVersion, unified.RoleVersion, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessRoleVersion, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.TeamVersion, unified.TeamVersion, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessTeamVersion, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.OldX, unified.OldX, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldX, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.NewX, unified.NewX, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewX, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.OldTeam, unified.OldTeam, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldTeam, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.NewTeam, unified.NewTeam, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewTeam, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.OldRoleFlags, unified.OldRoleFlags, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldRoleFlags, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.NewRoleFlags, unified.NewRoleFlags, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewRoleFlags, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.OldLiving ? 1 : 0, unified.OldLiving ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldLiving, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.NewLiving ? 1 : 0, unified.NewLiving ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewLiving, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.OldHp, unified.OldHp, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldHp, production.Slot, ref mismatch) &&
                   MatchAiUnifiedSnapshotValue(production.NewHp, unified.NewHp, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewHp, production.Slot, ref mismatch);
        }

        private static int PackAiUnifiedSnapshotRoleFlags(
            bool ground,
            bool air)
        {
            return (ground ? 1 : 0) | (air ? 1 << 1 : 0);
        }

        private bool TryCompareAiUnifiedSnapshotIndexes(
            AiSoASensingRows production,
            AiSoASensingRows unified,
            AiUnifiedSnapshotConsumer consumer,
            bool compareSpecialIndex,
            bool compareRoleIndexes,
            bool compareTeamSummaries,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            if (compareSpecialIndex)
            {
                RecordAiUnifiedSnapshotDerivedComparisonEntries(2);
                if (!MatchAiUnifiedSnapshotValue(production.SpecialIndexReady ? 1 : 0, unified.SpecialIndexReady ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.IndexReadiness, AiUnifiedSnapshotField.SpecialIndex, -1, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.SpecialSlotCount, unified.SpecialSlotCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.SpecialIndex, -1, ref mismatch))
                {
                    return false;
                }
                for (int index = 0; index < production.SpecialSlotCount; index++)
                {
                    RecordAiUnifiedSnapshotDerivedComparisonEntries(1);
                    if (!MatchAiUnifiedSnapshotValue(production.SpecialSlots[index], unified.SpecialSlots[index], consumer, AiUnifiedSnapshotMismatchKind.IndexEntry, AiUnifiedSnapshotField.SpecialIndex, index, ref mismatch))
                        return false;
                }
            }

            if (compareRoleIndexes)
            {
                RecordAiUnifiedSnapshotDerivedComparisonEntries(5);
                if (!MatchAiUnifiedSnapshotValue(production.RoleIndexesReady ? 1 : 0, unified.RoleIndexesReady ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.IndexReadiness, AiUnifiedSnapshotField.GroundRoleIndex, -1, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.GroundRoleSlotCount, unified.GroundRoleSlotCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.GroundRoleIndex, -1, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.AirRoleSlotCount, unified.AirRoleSlotCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.AirRoleIndex, -1, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.GroundRoleTeamSummaryCount, unified.GroundRoleTeamSummaryCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.GroundRoleTeamSummary, -1, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.AirRoleTeamSummaryCount, unified.AirRoleTeamSummaryCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.AirRoleTeamSummary, -1, ref mismatch))
                {
                    return false;
                }
                for (int index = 0; index < production.GroundRoleSlotCount; index++)
                {
                    RecordAiUnifiedSnapshotDerivedComparisonEntries(1);
                    if (!MatchAiUnifiedSnapshotValue(production.GroundRoleSlotsByX[index], unified.GroundRoleSlotsByX[index], consumer, AiUnifiedSnapshotMismatchKind.IndexEntry, AiUnifiedSnapshotField.GroundRoleIndex, index, ref mismatch))
                        return false;
                }
                for (int index = 0; index < production.AirRoleSlotCount; index++)
                {
                    RecordAiUnifiedSnapshotDerivedComparisonEntries(1);
                    if (!MatchAiUnifiedSnapshotValue(production.AirRoleSlotsByX[index], unified.AirRoleSlotsByX[index], consumer, AiUnifiedSnapshotMismatchKind.IndexEntry, AiUnifiedSnapshotField.AirRoleIndex, index, ref mismatch))
                        return false;
                }
                for (int index = 0; index < production.GroundRoleTeamSummaryCount; index++)
                {
                    RecordAiUnifiedSnapshotDerivedComparisonEntries(1);
                    AiSensingRoleTeamSummary expected = production.GroundRoleTeamSummaries[index];
                    AiSensingRoleTeamSummary actual = unified.GroundRoleTeamSummaries[index];
                    if (!MatchAiUnifiedSnapshotValue(expected.Team, actual.Team, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.GroundRoleTeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.Start, actual.Start, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.GroundRoleTeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.Count, actual.Count, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.GroundRoleTeamSummary, index, ref mismatch))
                    {
                        return false;
                    }
                }
                for (int index = 0; index < production.AirRoleTeamSummaryCount; index++)
                {
                    RecordAiUnifiedSnapshotDerivedComparisonEntries(1);
                    AiSensingRoleTeamSummary expected = production.AirRoleTeamSummaries[index];
                    AiSensingRoleTeamSummary actual = unified.AirRoleTeamSummaries[index];
                    if (!MatchAiUnifiedSnapshotValue(expected.Team, actual.Team, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.AirRoleTeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.Start, actual.Start, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.AirRoleTeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.Count, actual.Count, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.AirRoleTeamSummary, index, ref mismatch))
                    {
                        return false;
                    }
                }
            }

            if (compareTeamSummaries)
            {
                RecordAiUnifiedSnapshotDerivedComparisonEntries(2);
                if (!MatchAiUnifiedSnapshotValue(production.TeamSummariesReady ? 1 : 0, unified.TeamSummariesReady ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.IndexReadiness, AiUnifiedSnapshotField.TeamSummary, -1, ref mismatch) ||
                    !MatchAiUnifiedSnapshotValue(production.TeamSummaryCount, unified.TeamSummaryCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.TeamSummary, -1, ref mismatch))
                {
                    return false;
                }
                for (int index = 0; index < production.TeamSummaryCount; index++)
                {
                    RecordAiUnifiedSnapshotDerivedComparisonEntries(1);
                    AiSensingTeamSummary expected = production.TeamSummaries[index];
                    AiSensingTeamSummary actual = unified.TeamSummaries[index];
                    if (!MatchAiUnifiedSnapshotValue(expected.Team, actual.Team, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.Count, actual.Count, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.MinHp, actual.MinHp, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.MinCount, actual.MinCount, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchAiUnifiedSnapshotValue(expected.SecondMinHp, actual.SecondMinHp, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void RecordAiUnifiedSnapshotDerivedComparisonEntries(long count)
        {
            AiUnifiedSnapshotShadowDerivedComparisonEntryVisitCountForDiagnostics += count;
            if (aiUnifiedSnapshotRefreshComparisonActive)
            {
                AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics +=
                    count;
            }
        }

        private void ResetAiUnifiedMoveModeFirst10Snapshot()
        {
            Array.Clear(aiUnifiedMoveModeFirst10Present, 0, aiUnifiedMoveModeFirst10Present.Length);
            Array.Clear(aiUnifiedMoveModeFirst10Eligible, 0, aiUnifiedMoveModeFirst10Eligible.Length);
            Array.Clear(aiUnifiedMoveModeFirst10Generation, 0, aiUnifiedMoveModeFirst10Generation.Length);
            Array.Clear(aiUnifiedMoveModeFirst10Hp, 0, aiUnifiedMoveModeFirst10Hp.Length);
            Array.Clear(aiUnifiedMoveModeFirst10X, 0, aiUnifiedMoveModeFirst10X.Length);
            Array.Clear(aiUnifiedMoveModeFirst10Z, 0, aiUnifiedMoveModeFirst10Z.Length);
            aiUnifiedMoveModeTopSlot = -1;
            aiUnifiedMoveModeTopX = -1;
            aiUnifiedMoveModeTopZ = 0;
            aiUnifiedMoveModeSecondSlot = -1;
            aiUnifiedMoveModeSecondX = -1;
            aiUnifiedMoveModeSecondZ = 0;
            aiUnifiedMoveModeFirst10Valid = false;
        }

        private void CaptureAiUnifiedMoveModeFirst10Candidate(
            int slot,
            LF2Entity entity,
            uint generation)
        {
            aiUnifiedMoveModeFirst10Present[slot] = true;
            aiUnifiedMoveModeFirst10Generation[slot] = generation;
            aiUnifiedMoveModeFirst10Hp[slot] = Hp(entity);
            bool eligible = IsLivingCharacterDat(entity);
            aiUnifiedMoveModeFirst10Eligible[slot] = eligible;
            if (!eligible)
                return;

            int x = X(entity);
            int z = Z(entity);
            aiUnifiedMoveModeFirst10X[slot] = x;
            aiUnifiedMoveModeFirst10Z[slot] = z;
            if (x <= -1)
                return;

            if (aiUnifiedMoveModeTopSlot < 0 || x > aiUnifiedMoveModeTopX)
            {
                aiUnifiedMoveModeSecondSlot = aiUnifiedMoveModeTopSlot;
                aiUnifiedMoveModeSecondX = aiUnifiedMoveModeTopX;
                aiUnifiedMoveModeSecondZ = aiUnifiedMoveModeTopZ;
                aiUnifiedMoveModeTopSlot = slot;
                aiUnifiedMoveModeTopX = x;
                aiUnifiedMoveModeTopZ = z;
                return;
            }

            if (aiUnifiedMoveModeSecondSlot < 0 || x > aiUnifiedMoveModeSecondX)
            {
                aiUnifiedMoveModeSecondSlot = slot;
                aiUnifiedMoveModeSecondX = x;
                aiUnifiedMoveModeSecondZ = z;
            }
        }

        private void ObserveAiUnifiedMoveModeFirst10Mutation(LF2Entity entity)
        {
            if (!aiUnifiedMoveModeFirst10Valid)
                return;
            if (entity?.Runtime == null)
            {
                aiUnifiedMoveModeFirst10Valid = false;
                return;
            }

            int slot = Slot(entity);
            if (slot < 0 || slot >= aiUnifiedMoveModeFirst10Present.Length)
            {
                for (int index = 0; index < aiUnifiedMoveModeFirst10Present.Length; index++)
                {
                    if (ReferenceEquals(aiUnifiedSnapshotFallbackSlots[index], entity))
                    {
                        aiUnifiedMoveModeFirst10Valid = false;
                        break;
                    }
                }
                return;
            }

            if (!aiUnifiedMoveModeFirst10Present[slot] ||
                !ReferenceEquals(aiUnifiedSnapshotFallbackSlots[slot], entity) ||
                !TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle) ||
                handle.Generation != aiUnifiedMoveModeFirst10Generation[slot] ||
                Hp(entity) != aiUnifiedMoveModeFirst10Hp[slot])
            {
                aiUnifiedMoveModeFirst10Valid = false;
                return;
            }

            bool eligible = IsLivingCharacterDat(entity);
            if (eligible != aiUnifiedMoveModeFirst10Eligible[slot] ||
                eligible &&
                (X(entity) != aiUnifiedMoveModeFirst10X[slot] ||
                 Z(entity) != aiUnifiedMoveModeFirst10Z[slot]))
            {
                aiUnifiedMoveModeFirst10Valid = false;
            }
        }

        private void RecordAiUnifiedSnapshotShadowException(
            AiUnifiedSnapshotExceptionStage stage,
            Exception exception)
        {
            if (AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics !=
                AiUnifiedSnapshotExceptionStage.None)
            {
                return;
            }

            AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics = stage;
            aiUnifiedSnapshotFirstExceptionType = exception?.GetType();
        }

        private void ThrowAiUnifiedSnapshotExceptionForSelfCheck(
            AiUnifiedSnapshotExceptionStage stage)
        {
#if UNITY_INCLUDE_TESTS
            if (aiUnifiedSnapshotExceptionStageForSelfCheck != stage)
                return;
            aiUnifiedSnapshotExceptionStageForSelfCheck =
                AiUnifiedSnapshotExceptionStage.None;
            throw new InvalidOperationException(
                "Injected unified AI snapshot observer exception.");
#endif
        }

        private static bool MatchAiUnifiedSnapshotValue(
            long expected,
            long actual,
            AiUnifiedSnapshotConsumer consumer,
            AiUnifiedSnapshotMismatchKind kind,
            AiUnifiedSnapshotField field,
            int slot,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            if (expected == actual)
                return true;
            return SetAiUnifiedSnapshotMismatch(
                consumer,
                kind,
                field,
                slot,
                expected,
                actual,
                ref mismatch);
        }

        private static bool SetAiUnifiedSnapshotMismatch(
            AiUnifiedSnapshotConsumer consumer,
            AiUnifiedSnapshotMismatchKind kind,
            AiUnifiedSnapshotField field,
            int slot,
            long expected,
            long actual,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            mismatch = new AiUnifiedSnapshotMismatch
            {
                Consumer = consumer,
                Kind = kind,
                Field = field,
                Slot = slot,
                ExpectedValue = expected,
                ActualValue = actual,
            };
            return false;
        }

#if UNITY_INCLUDE_TESTS
        public void SetAiUnifiedSnapshotWitnessMutationForSelfCheck(
            AiUnifiedSnapshotConsumer consumer)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI witness mutation while ticking.");
            }
            if (consumer != AiUnifiedSnapshotConsumer.SoASensing &&
                consumer != AiUnifiedSnapshotConsumer.IndexedDecision)
            {
                throw new ArgumentOutOfRangeException(nameof(consumer));
            }
            aiUnifiedSnapshotWitnessMutationConsumerForSelfCheck = consumer;
        }

        public void SetAiUnifiedSnapshotProductMutationForSelfCheck(
            AiUnifiedSnapshotProductMutationKind kind,
            int slot)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI product mutation while ticking.");
            }
            if (kind != AiUnifiedSnapshotProductMutationKind.FallbackReference &&
                kind != AiUnifiedSnapshotProductMutationKind.MoveModeFirst10Hp)
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }
            if (slot < 0 ||
                kind == AiUnifiedSnapshotProductMutationKind.MoveModeFirst10Hp &&
                slot >= aiUnifiedMoveModeFirst10Hp.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }
            aiUnifiedSnapshotProductMutationKindForSelfCheck = kind;
            aiUnifiedSnapshotProductMutationSlotForSelfCheck = slot;
        }

        public void SetAiUnifiedSnapshotExceptionForSelfCheck(
            AiUnifiedSnapshotExceptionStage stage)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI snapshot exception while ticking.");
            }
            if (stage == AiUnifiedSnapshotExceptionStage.None)
                throw new ArgumentOutOfRangeException(nameof(stage));
            aiUnifiedSnapshotExceptionStageForSelfCheck = stage;
        }

        public void SetAiUnifiedSnapshotExecutionFailureForSelfCheck(
            AiUnifiedSnapshotExceptionStage stage)
        {
            SetAiUnifiedSnapshotExceptionForSelfCheck(stage);
        }

        public void SetAiUnifiedSnapshotExecutionVisibilityProbeForSelfCheck(
            int observerSlotA,
            int targetSlotA,
            int observerSlotB,
            int targetSlotB)
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "Cannot arm unified AI visibility probes while ticking.");
            aiUnifiedSnapshotExecutionProbeObserverSlotAForSelfCheck = observerSlotA;
            aiUnifiedSnapshotExecutionProbeTargetSlotAForSelfCheck = targetSlotA;
            aiUnifiedSnapshotExecutionProbeStateAForSelfCheck = int.MinValue;
            aiUnifiedSnapshotExecutionProbeObserverSlotBForSelfCheck = observerSlotB;
            aiUnifiedSnapshotExecutionProbeTargetSlotBForSelfCheck = targetSlotB;
            aiUnifiedSnapshotExecutionProbeStateBForSelfCheck = int.MinValue;
        }

        public bool ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck()
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null &&
                   ValidateAiUnifiedSnapshotExecutionState(
                       published,
                       published.ExpectedCapacity,
                       published.Epoch);
        }

        public int GetAiUnifiedSnapshotExecutionPublishedGenerationForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? unchecked((int)published.Rows.Generation[slot])
                : 0;
        }

        public int GetAiUnifiedSnapshotExecutionPublishedStableIdForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? published.Rows.Identity[slot]
                : 0;
        }

        public int GetAiUnifiedSnapshotExecutionPublishedSensingBoundaryForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? published.SoASensingBoundaryFlags[slot]
                : 0;
        }

        public int GetAiUnifiedSnapshotExecutionPublishedDecisionBoundaryForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? published.DecisionBoundaryFlags[slot]
                : 0;
        }

        public bool IsAiUnifiedSnapshotExecutionPublishedFallbackForSelfCheck(
            int slot,
            LF2Entity entity)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null &&
                   slot >= 0 &&
                   slot < published.Capacity &&
                   ReferenceEquals(published.FallbackSlots[slot], entity);
        }

        public bool IsAiUnifiedSnapshotExecutionPublishedFirst10PresentForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null &&
                   slot >= 0 &&
                   slot < published.MoveModeFirst10Present.Length &&
                   published.MoveModeFirst10Present[slot];
        }

        public void SetAiUnifiedSnapshotBoundaryMutationForSelfCheck(
            AiUnifiedSnapshotConsumer consumer,
            int slot,
            int xorMask)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI snapshot mutation while ticking.");
            }
            if (consumer != AiUnifiedSnapshotConsumer.SoASensing &&
                consumer != AiUnifiedSnapshotConsumer.IndexedDecision)
            {
                throw new ArgumentOutOfRangeException(nameof(consumer));
            }
            if (slot < 0)
                throw new ArgumentOutOfRangeException(nameof(slot));
            if (xorMask == 0)
                throw new ArgumentOutOfRangeException(nameof(xorMask));

            aiUnifiedSnapshotBoundaryMutationConsumerForSelfCheck = consumer;
            aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck = slot;
            aiUnifiedSnapshotBoundaryMutationXorForSelfCheck = xorMask;
        }
#endif

        private static bool AiDecisionInputEquals(
            AiDecisionInputState expected,
            AiDecisionInputState actual)
        {
            return expected.History0 == actual.History0 &&
                   expected.History1 == actual.History1 &&
                   expected.History2 == actual.History2 &&
                   expected.History3 == actual.History3 &&
                   expected.History4 == actual.History4 &&
                   expected.History5 == actual.History5 &&
                   expected.CdAttack == actual.CdAttack &&
                   expected.CdJump == actual.CdJump &&
                   expected.CdDefend == actual.CdDefend &&
                   expected.CdDefendLock == actual.CdDefendLock &&
                   expected.CdRight == actual.CdRight &&
                   expected.CdLeft == actual.CdLeft &&
                   expected.CdUp == actual.CdUp &&
                   expected.CdDown == actual.CdDown &&
                   expected.ComboDra == actual.ComboDra &&
                   expected.ComboDla == actual.ComboDla &&
                   expected.ComboDua == actual.ComboDua &&
                   expected.ComboDda == actual.ComboDda &&
                   expected.ComboDrj == actual.ComboDrj &&
                   expected.ComboDlj == actual.ComboDlj &&
                   expected.ComboDuj == actual.ComboDuj &&
                   expected.ComboDdj == actual.ComboDdj &&
                   expected.ComboDja == actual.ComboDja &&
                   expected.PrevUp == actual.PrevUp &&
                   expected.PrevDown == actual.PrevDown &&
                   expected.PrevLeft == actual.PrevLeft &&
                   expected.PrevRight == actual.PrevRight &&
                   expected.PrevJump == actual.PrevJump &&
                   expected.PrevDefend == actual.PrevDefend &&
                   expected.PrevAttack == actual.PrevAttack &&
                   expected.KeyUp == actual.KeyUp &&
                   expected.KeyDown == actual.KeyDown &&
                   expected.KeyLeft == actual.KeyLeft &&
                   expected.KeyRight == actual.KeyRight &&
                   expected.KeyAttack == actual.KeyAttack &&
                   expected.KeyJump == actual.KeyJump &&
                   expected.KeyDefend == actual.KeyDefend &&
                   expected.Unk360 == actual.Unk360 &&
                   expected.Unk3FC == actual.Unk3FC &&
                   expected.Unk400 == actual.Unk400 &&
                   expected.BoundaryFlags == actual.BoundaryFlags;
        }
    }
}
