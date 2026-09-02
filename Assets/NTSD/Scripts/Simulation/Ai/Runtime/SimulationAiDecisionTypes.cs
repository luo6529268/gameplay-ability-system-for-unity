using System;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Ecs;

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
        HitJ = 61,
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
        CharacterDecisionPosition = 9,
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
        CharacterDecisionPosition = 15,
    }

}


