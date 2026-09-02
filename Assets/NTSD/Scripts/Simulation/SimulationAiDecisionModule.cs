using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Ecs;

using AiSoASensingRows =
    NTSD.Simulation.SimulationAiSensingModule.AiSoASensingRows;
using AiInputContext =
    NTSD.Simulation.SimulationAiInputModule.AiInputContext;
using AiSoANearestResult =
    NTSD.Simulation.SimulationAiSensingModule.AiSoANearestResult;
using AiSoASpecialResult =
    NTSD.Simulation.SimulationAiSensingModule.AiSoASpecialResult;

namespace NTSD.Simulation
{
    internal sealed class SimulationAiDecisionModule
    {
        private const int AiSoASpecialProximity = 1 << 0;
        private const int AiSoASpecialLeft = 1 << 1;
        private const int AiSoASpecialRight = 1 << 2;
        private const int AiSoASpecialUp = 1 << 3;
        private const int AiSoASpecialDown = 1 << 4;
        private const int AiSoASpecialGuard7A = 1 << 5;
        private const int AiSoASpecialGuard7B = 1 << 6;
        private const int AiSoASpecialForce7AGround = 1 << 7;
        private const int AiSoASpecialC8ThreatSeen = 1 << 8;
        private const int AiSoASpecialPostSelectionSeen = 1 << 9;
        private readonly RuntimeSlotTable runtimeSlots;
        private readonly SimulationAiInputModule input;
        private readonly SimulationAiSensingModule sensing;
        private AiUnifiedSnapshotMutationWitness soASensingMutationWitness;
        private AiUnifiedSnapshotMutationWitness decisionMutationWitness;
        private AiUnifiedSnapshotMutationWitness unifiedSnapshotMutationWitness;
        private AiDecisionRowContext decisionRowContext;

        internal struct AiUnifiedSnapshotMutationWitness
        {
            internal ulong Epoch;
            internal long Ordinal;
            internal int Slot;
            internal uint Generation;
            internal int StableId;
            internal bool RoleRebuilt;
            internal bool TeamRebuilt;
            internal long RoleVersion;
            internal long TeamVersion;
            internal int OldX;
            internal int NewX;
            internal int OldTeam;
            internal int NewTeam;
            internal int OldRoleFlags;
            internal int NewRoleFlags;
            internal bool OldLiving;
            internal bool NewLiving;
            internal int OldHp;
            internal int NewHp;
        }

        internal struct AiDecisionRowIdentity
        {
            internal LF2Entity Entity;
            internal int Slot;
            internal uint Generation;
            internal int StableId;
            internal bool Included;
        }

        internal struct AiDecisionRowContext
        {
            internal AiSoASensingRows Rows;
            internal LF2Entity[] Slots;
            internal ulong OccupancyEpoch;
            internal AiDecisionRowIdentity Self;
            internal AiDecisionRowIdentity Selected;
            internal AiDecisionRowIdentity Cached;
            internal bool Bound;
        }

        internal sealed class AiUnifiedSnapshotExecutionState
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

        internal SimulationAiDecisionModule(
            int runtimeSlotCapacity,
            RuntimeSlotTable runtimeSlots,
            SimulationAiInputModule input,
            SimulationAiSensingModule sensing)
        {
            this.runtimeSlots = runtimeSlots ??
                throw new ArgumentNullException(nameof(runtimeSlots));
            this.input = input ?? throw new ArgumentNullException(nameof(input));
            this.sensing = sensing ?? throw new ArgumentNullException(nameof(sensing));
        }

        internal AiDecisionShadowMode ShadowMode { get; set; }
        internal AiDecisionExecutionMode ExecutionMode { get; set; }
        internal AiUnifiedSnapshotShadowMode UnifiedShadowMode { get; set; }
        internal AiUnifiedSnapshotExecutionMode UnifiedExecutionMode { get; set; }
        internal int IndexedCanonicalFullOracleSampleInterval { get; set; }
        internal AiDecisionSnapshot LegacyFallbackSnapshot { get; set; }
        internal AiDecisionSnapshot ShadowSnapshot { get; set; }
        internal AiDecisionSnapshot SharedSnapshot { get; set; }
        internal AiDecisionSnapshot IndexedSnapshot { get; set; }
        internal AiSoASensingRows SharedRows { get; set; }
        internal AiDecisionSnapshot ComparisonSnapshot { get; set; }
        internal ulong SharedPassEpoch { get; set; }
        internal bool SharedPassAvailable { get; set; }
        internal AiDecisionAvailability SharedPassUnavailableReason { get; set; }
        internal AiDecisionWitness ShadowExpected { get; set; }
        internal LF2Entity ShadowSelf { get; set; }
        internal bool ShadowComparisonActive { get; set; }
        internal bool LegacyRngRecording { get; set; }
        internal int[] LegacyRngModuli { get; } = new int[256];
        internal int[] LegacyRngRaw { get; } = new int[256];
        internal int[] LegacyRngValues { get; } = new int[256];
        internal int LegacyRngCount { get; set; }
        internal bool LegacyRngOverflow { get; set; }
        internal ulong LegacyRngOrderHash { get; set; } = 1469598103934665603UL;
        internal int LegacyCharacterDecisionPosition { get; set; }
        internal int[] CharacterDecisionLegacyRngModuli { get; } = new int[256];
        internal int[] CharacterDecisionLegacyRngRaw { get; } = new int[256];
        internal int[] CharacterDecisionLegacyRngValues { get; } = new int[256];
        internal Type ShadowFirstExceptionType { get; set; }
        internal AiSoASensingRows UnifiedSnapshotRows { get; set; }
        internal int[] UnifiedSnapshotSoASensingBoundaryFlags { get; set; }
        internal int[] UnifiedSnapshotDecisionBoundaryFlags { get; set; }
        internal LF2Entity[] UnifiedSnapshotFallbackSlots { get; set; }
        internal bool[] UnifiedMoveModeFirst10Present { get; set; } = new bool[10];
        internal bool[] UnifiedMoveModeFirst10Eligible { get; set; } = new bool[10];
        internal uint[] UnifiedMoveModeFirst10Generation { get; set; } = new uint[10];
        internal int[] UnifiedMoveModeFirst10Hp { get; set; } = new int[10];
        internal int[] UnifiedMoveModeFirst10X { get; set; } = new int[10];
        internal int[] UnifiedMoveModeFirst10Z { get; set; } = new int[10];
        internal int UnifiedMoveModeTopSlot { get; set; } = -1;
        internal int UnifiedMoveModeTopX { get; set; } = -1;
        internal int UnifiedMoveModeTopZ { get; set; }
        internal int UnifiedMoveModeSecondSlot { get; set; } = -1;
        internal int UnifiedMoveModeSecondX { get; set; } = -1;
        internal int UnifiedMoveModeSecondZ { get; set; }
        internal bool UnifiedMoveModeFirst10Valid { get; set; }
        internal AiUnifiedSnapshotExecutionState UnifiedSnapshotPublishedState { get; set; }
        internal AiUnifiedSnapshotExecutionState UnifiedSnapshotScratchState { get; set; }
        internal AiUnifiedSnapshotExecutionState UnifiedSnapshotStandbyState { get; set; }
        internal AiSoASensingRows UnifiedSnapshotLegacySoARows { get; set; }
        internal AiSoASensingRows UnifiedSnapshotLegacyDecisionRows { get; set; }
        internal AiDecisionSnapshot UnifiedSnapshotLegacySharedSnapshot { get; set; }
        internal AiDecisionSnapshot UnifiedSnapshotLegacyIndexedSnapshot { get; set; }
        internal LF2Entity[] UnifiedSnapshotLegacyInputSlots { get; set; }
        internal bool[] UnifiedSnapshotLegacyMoveModeFirst10Present { get; set; }
        internal bool[] UnifiedSnapshotLegacyMoveModeFirst10Eligible { get; set; }
        internal uint[] UnifiedSnapshotLegacyMoveModeFirst10Generation { get; set; }
        internal int[] UnifiedSnapshotLegacyMoveModeFirst10Hp { get; set; }
        internal int[] UnifiedSnapshotLegacyMoveModeFirst10X { get; set; }
        internal int[] UnifiedSnapshotLegacyMoveModeFirst10Z { get; set; }
        internal bool UnifiedSnapshotExecutionCommittedThisPass { get; set; }
        internal bool UnifiedSnapshotExecutionConsumerStartedThisPass { get; set; }
        internal bool UnifiedSnapshotNoPendingRefreshSkip { get; set; } = true;
        internal ulong UnifiedSnapshotPassEpoch { get; set; }
        internal bool UnifiedSnapshotPassAvailable { get; set; }
        internal bool UnifiedSnapshotPassFailureRecorded { get; set; }
        internal bool UnifiedSnapshotProductsComparedThisPass { get; set; }
        internal bool UnifiedSnapshotRefreshComparisonActive { get; set; }
        internal long SoASensingMutationWitnessOrdinal { get; set; }
        internal long SoASensingRoleIndexVersion { get; set; }
        internal long SoASensingTeamSummaryVersion { get; set; }
        internal ref AiUnifiedSnapshotMutationWitness SoASensingMutationWitness =>
            ref soASensingMutationWitness;
        internal long DecisionMutationWitnessOrdinal { get; set; }
        internal long DecisionRoleIndexVersion { get; set; }
        internal long DecisionTeamSummaryVersion { get; set; }
        internal ref AiUnifiedSnapshotMutationWitness DecisionMutationWitness =>
            ref decisionMutationWitness;
        internal long UnifiedSnapshotMutationWitnessOrdinal { get; set; }
        internal long UnifiedSnapshotRoleIndexVersion { get; set; }
        internal long UnifiedSnapshotTeamSummaryVersion { get; set; }
        internal ref AiUnifiedSnapshotMutationWitness UnifiedSnapshotMutationWitness =>
            ref unifiedSnapshotMutationWitness;
        internal Type UnifiedSnapshotFirstExceptionType { get; set; }
        internal long IndexedEligibleCount { get; set; }
        internal long IndexedAvailableCount { get; set; }
        internal long IndexedUnavailableCount { get; set; }
        internal long IndexedComparedCount { get; set; }
        internal long IndexedMismatchCount { get; set; }
        internal long IndexedFullRowVisitCount { get; set; }
        internal long IndexedRowVisitCount { get; set; }
        internal AiDecisionIndexedMismatchReason IndexedFirstMismatchReason { get; set; }
        internal long ShadowEligibleCount { get; set; }
        internal long ShadowAvailableCount { get; set; }
        internal long ShadowUnavailableCount { get; set; }
        internal long ShadowComparedCount { get; set; }
        internal long ShadowMismatchCount { get; set; }
        internal long ShadowCloneRngCallCount { get; set; }
        internal long ShadowRowVisitCount { get; set; }
        internal long SharedBuildCount { get; set; }
        internal long SharedRefreshCount { get; set; }
        internal long IndexedCanonicalEligibleCount { get; set; }
        internal long IndexedCanonicalCommittedCount { get; set; }
        internal long IndexedCanonicalFallbackCount { get; set; }
        internal long IndexedCanonicalFullOracleSampleCount { get; set; }
        internal long IndexedCanonicalFullOracleMismatchCount { get; set; }
        internal AiDecisionAvailability IndexedCanonicalFirstFallbackReason { get; set; }
        internal AiDecisionIndexedMismatchReason IndexedCanonicalFirstOracleMismatchReason
        {
            get;
            set;
        }
        internal AiDecisionAvailability ShadowFirstUnavailableReason { get; set; }
        internal AiDecisionShadowMismatchReason ShadowFirstMismatchReason { get; set; }
        internal AiDecisionShadowExceptionStage ShadowFirstExceptionStage { get; set; }
        internal bool DecisionRemainderEnabled { get; set; }
        internal bool DecisionRemainderUseRowsForCurrentInput { get; set; }
        internal bool DecisionRemainderAttemptedForCurrentInput { get; set; }
        internal bool DecisionRemainderRandomBoundaryPassed { get; set; }
        internal bool DecisionRemainderForceBeforeRandomFailure { get; set; }
        internal bool DecisionRemainderForceAfterRandomFailure { get; set; }
        internal int DecisionRemainderMutationKind { get; set; }
        internal bool DecisionRemainderMutationAfterRandom { get; set; }
        internal bool DecisionRemainderHardFailureRecorded { get; set; }
        internal ref AiDecisionRowContext DecisionRowContext => ref decisionRowContext;
        internal int DecisionRemainderEligibleAttemptCount { get; set; }
        internal int DecisionRemainderAppliedCount { get; set; }
        internal int DecisionRemainderFallbackCount { get; set; }
        internal int DecisionRemainderPreRandomFailureCount { get; set; }
        internal int DecisionRemainderPostRandomFailureCount { get; set; }
        internal int DecisionRemainderHardFailureCount { get; set; }
        internal int DecisionRemainderContextBindCount { get; set; }
        internal int DecisionRemainderGatewayValidationCount { get; set; }
        internal long DecisionRemainderRowVisitCount { get; set; }
        internal long UnifiedShadowBuildCount { get; set; }
        internal long UnifiedShadowSlotVisitCount { get; set; }
        internal long UnifiedShadowRefreshCount { get; set; }
        internal long UnifiedShadowSensingComparedCount { get; set; }
        internal long UnifiedShadowDecisionComparedCount { get; set; }
        internal long UnifiedShadowUnavailableCount { get; set; }
        internal long UnifiedShadowMismatchCount { get; set; }
        internal long UnifiedShadowDistinctBoundaryEncodingRowCount { get; set; }
        internal long UnifiedShadowFullComparisonSlotVisitCount { get; set; }
        internal long UnifiedShadowRefreshComparisonSlotVisitCount { get; set; }
        internal long UnifiedShadowDerivedComparisonEntryVisitCount { get; set; }
        internal long UnifiedShadowMutationWitnessComparedCount { get; set; }
        internal long UnifiedShadowRefreshDerivedFullLoopEntryVisitCount { get; set; }
        internal AiUnifiedSnapshotMismatch UnifiedShadowFirstMismatch { get; set; }
        internal AiUnifiedSnapshotExceptionStage UnifiedShadowFirstExceptionStage { get; set; }
        internal long UnifiedExecutionBuildCount { get; set; }
        internal long UnifiedExecutionRollForwardCount { get; set; }
        internal long UnifiedExecutionRollForwardDirtySlotCount { get; set; }
        internal long UnifiedExecutionSlotVisitCount { get; set; }
        internal long UnifiedExecutionCanonicalInitialCaptureCount { get; set; }
        internal long UnifiedExecutionRefreshCount { get; set; }
        internal long UnifiedExecutionNoPendingRefreshSkipCount { get; set; }
        internal long UnifiedExecutionIncrementalValidationCount { get; set; }
        internal long UnifiedExecutionReadCount { get; set; }
        internal long UnifiedExecutionCommittedPassCount { get; set; }
        internal long UnifiedExecutionPreCommitFailureCount { get; set; }
        internal long UnifiedExecutionPreCommitFallbackCount { get; set; }
        internal long UnifiedExecutionPostCommitHardBreachCount { get; set; }
        internal AiUnifiedSnapshotExceptionStage UnifiedExecutionFirstFailureStage { get; set; }
        internal Type UnifiedExecutionFirstFailureType { get; set; }
#if UNITY_INCLUDE_TESTS
        internal long ShadowBeginInvocationCountForTests { get; set; }
        internal long ShadowCompleteInvocationCountForTests { get; set; }
        private int sharedPreflightMutationKindForSelfCheck = -1;
        private int sharedPreflightMutationSlotForSelfCheck = -1;
        private int sharedPostLegacyMutationSlotForSelfCheck = -1;
        private int sharedPostLegacyMutationStateForSelfCheck;
        private AiDecisionShadowExceptionStage shadowExceptionStageForSelfCheck;
        private AiUnifiedSnapshotExceptionStage unifiedSnapshotExceptionStageForSelfCheck;
        internal AiDecisionAvailability IndexedCanonicalPreCommitFailureForSelfCheck
        {
            get;
            set;
        }
        internal int UnifiedExecutionProbeObserverSlotAForSelfCheck { get; set; } = -1;
        internal int UnifiedExecutionProbeTargetSlotAForSelfCheck { get; set; } = -1;
        internal int UnifiedExecutionProbeStateAForSelfCheck { get; set; } = int.MinValue;
        internal int UnifiedExecutionProbeObserverSlotBForSelfCheck { get; set; } = -1;
        internal int UnifiedExecutionProbeTargetSlotBForSelfCheck { get; set; } = -1;
        internal int UnifiedExecutionProbeStateBForSelfCheck { get; set; } = int.MinValue;
        internal AiUnifiedSnapshotConsumer UnifiedBoundaryMutationConsumerForSelfCheck
        {
            get;
            set;
        }
        internal int UnifiedBoundaryMutationSlotForSelfCheck { get; set; } = -1;
        internal int UnifiedBoundaryMutationXorForSelfCheck { get; set; }
        internal AiUnifiedSnapshotConsumer UnifiedWitnessMutationConsumerForSelfCheck
        {
            get;
            set;
        }
        internal AiUnifiedSnapshotProductMutationKind UnifiedProductMutationKindForSelfCheck
        {
            get;
            set;
        }
        internal int UnifiedProductMutationSlotForSelfCheck { get; set; } = -1;
#endif

#if UNITY_INCLUDE_TESTS
        internal void SetShadowExceptionStageForSelfCheck(
            AiDecisionShadowExceptionStage stage)
        {
            shadowExceptionStageForSelfCheck = stage;
        }

        internal void ThrowShadowExceptionForSelfCheck(
            AiDecisionShadowExceptionStage stage)
        {
            if (shadowExceptionStageForSelfCheck != stage)
                return;

            shadowExceptionStageForSelfCheck = AiDecisionShadowExceptionStage.None;
            throw new AiDecisionShadowSelfCheckException();
        }

        internal void SetSharedPreflightMutationForSelfCheck(
            int mutationKind,
            int slot)
        {
            sharedPreflightMutationKindForSelfCheck = mutationKind;
            sharedPreflightMutationSlotForSelfCheck = slot;
        }

        internal void ApplySharedPreflightMutationForSelfCheck()
        {
            int mutationKind = sharedPreflightMutationKindForSelfCheck;
            int slot = sharedPreflightMutationSlotForSelfCheck;
            sharedPreflightMutationKindForSelfCheck = -1;
            sharedPreflightMutationSlotForSelfCheck = -1;
            if (mutationKind < 0 ||
                SharedRows == null ||
                slot < 0 ||
                slot >= SharedRows.Capacity)
            {
                return;
            }

            switch (mutationKind)
            {
                case 0:
                    SharedRows.CapturedOccupancyEpoch =
                        SharedPassEpoch == ulong.MaxValue
                            ? 1UL
                            : SharedPassEpoch + 1UL;
                    break;
                case 1:
                    SharedRows.Generation[slot]++;
                    break;
                case 2:
                    SharedRows.Identity[slot]++;
                    break;
                case 3:
                    SharedRows.Included[slot] = false;
                    break;
            }
        }

        internal void SetSharedPostLegacyStateMutationForSelfCheck(
            int slot,
            int state)
        {
            sharedPostLegacyMutationSlotForSelfCheck = slot;
            sharedPostLegacyMutationStateForSelfCheck = state;
        }

        internal void ApplySharedPostLegacyMutationForSelfCheck(LF2Entity entity)
        {
            if (entity?.Runtime == null ||
                entity.Runtime.SlotIndex != sharedPostLegacyMutationSlotForSelfCheck)
            {
                return;
            }

            sharedPostLegacyMutationSlotForSelfCheck = -1;
            if (entity.Frame?.D != null)
                entity.Frame.D.state = sharedPostLegacyMutationStateForSelfCheck;
        }

        internal void SetUnifiedSnapshotExceptionForSelfCheck(
            AiUnifiedSnapshotExceptionStage stage)
        {
            unifiedSnapshotExceptionStageForSelfCheck = stage;
        }

        internal void ThrowUnifiedSnapshotExceptionForSelfCheck(
            AiUnifiedSnapshotExceptionStage stage)
        {
            if (unifiedSnapshotExceptionStageForSelfCheck != stage)
                return;

            unifiedSnapshotExceptionStageForSelfCheck =
                AiUnifiedSnapshotExceptionStage.None;
            throw new InvalidOperationException(
                "Injected unified AI snapshot observer exception.");
        }

        private sealed class AiDecisionShadowSelfCheckException : Exception
        {
        }
#endif

        internal void EvaluateIndexedShadow(
            AiDecisionSnapshot fullSnapshot,
            AiDecisionWitness fullWitness,
            bool fullAvailable)
        {
            IndexedEligibleCount++;
            if (IndexedSnapshot == null ||
                !ReferenceEquals(IndexedSnapshot.Rows, fullSnapshot.Rows))
            {
                IndexedUnavailableCount++;
                IndexedComparedCount++;
                RecordIndexedMismatch(AiDecisionIndexedMismatchReason.Availability);
                return;
            }

            IndexedSnapshot.CopyOwnedFrom(fullSnapshot);
            AiDecisionWitness indexedWitness = default;
            bool indexedAvailable = AiDecisionKernel.TryEvaluate(
                IndexedSnapshot,
                AiDecisionEvaluationPolicy.Indexed,
                ref indexedWitness);
            if (indexedAvailable)
                IndexedAvailableCount++;
            else
                IndexedUnavailableCount++;

            IndexedComparedCount++;
            IndexedFullRowVisitCount += fullWitness.RowVisits;
            IndexedRowVisitCount += indexedWitness.RowVisits;
            AiDecisionIndexedMismatchReason reason = CompareIndexedWitnesses(
                fullSnapshot,
                fullWitness,
                fullAvailable,
                IndexedSnapshot,
                indexedWitness,
                indexedAvailable);
            RecordIndexedMismatch(reason);
        }

        internal void RecordIndexedMismatch(AiDecisionIndexedMismatchReason reason)
        {
            if (reason == AiDecisionIndexedMismatchReason.None)
                return;

            IndexedMismatchCount++;
            if (IndexedFirstMismatchReason == AiDecisionIndexedMismatchReason.None)
                IndexedFirstMismatchReason = reason;
        }

        internal void PrepareCapacity(int capacity)
        {
            if (ShadowSnapshot == null ||
                ShadowSnapshot.Rows.Capacity != capacity)
            {
                ShadowSnapshot = new AiDecisionSnapshot(capacity);
            }
            if (LegacyFallbackSnapshot == null ||
                LegacyFallbackSnapshot.Rows.Capacity != capacity)
            {
                LegacyFallbackSnapshot = new AiDecisionSnapshot(capacity);
            }
            if (SharedRows == null || SharedRows.Capacity != capacity)
                SharedRows = new AiSoASensingRows(capacity);
            if (SharedSnapshot == null ||
                !ReferenceEquals(SharedSnapshot.Rows, SharedRows))
            {
                SharedSnapshot = new AiDecisionSnapshot(SharedRows);
            }
            if (IndexedSnapshot == null ||
                !ReferenceEquals(IndexedSnapshot.Rows, SharedRows))
            {
                IndexedSnapshot = new AiDecisionSnapshot(SharedRows);
            }
        }

        internal void EnsureExecutionScratchCapacity(int capacity)
        {
            if (UnifiedSnapshotScratchState != null &&
                UnifiedSnapshotScratchState.Capacity == capacity &&
                !ReferenceEquals(
                    UnifiedSnapshotScratchState,
                    UnifiedSnapshotPublishedState))
            {
                return;
            }

            UnifiedSnapshotScratchState =
                new AiUnifiedSnapshotExecutionState(capacity);
        }

        internal void EnsureUnifiedShadowCapacity(int capacity)
        {
            if (UnifiedSnapshotRows != null &&
                UnifiedSnapshotRows.Capacity == capacity &&
                UnifiedSnapshotFallbackSlots != null &&
                UnifiedSnapshotFallbackSlots.Length == capacity)
            {
                return;
            }

            UnifiedSnapshotRows = new AiSoASensingRows(capacity);
            UnifiedSnapshotSoASensingBoundaryFlags = new int[capacity];
            UnifiedSnapshotDecisionBoundaryFlags = new int[capacity];
            UnifiedSnapshotFallbackSlots = new LF2Entity[capacity];
        }

        internal void PrepareLegacyConsumerBuffers(int capacity)
        {
            AiUnifiedSnapshotExecutionState published =
                UnifiedSnapshotPublishedState;
            if (sensing.Rows != null &&
                (published == null ||
                 !ReferenceEquals(sensing.Rows, published.Rows)))
            {
                UnifiedSnapshotLegacySoARows = sensing.Rows;
            }
            if (SharedRows != null &&
                (published == null ||
                 !ReferenceEquals(SharedRows, published.Rows)))
            {
                UnifiedSnapshotLegacyDecisionRows = SharedRows;
            }
            if (input.Slots != null &&
                (published == null ||
                 !ReferenceEquals(input.Slots, published.FallbackSlots)))
            {
                UnifiedSnapshotLegacyInputSlots = input.Slots;
                UnifiedSnapshotLegacyMoveModeFirst10Present =
                    input.MoveModeFirst10Present;
                UnifiedSnapshotLegacyMoveModeFirst10Eligible =
                    input.MoveModeFirst10Eligible;
                UnifiedSnapshotLegacyMoveModeFirst10Generation =
                    input.MoveModeFirst10Generation;
                UnifiedSnapshotLegacyMoveModeFirst10Hp = input.MoveModeFirst10Hp;
                UnifiedSnapshotLegacyMoveModeFirst10X = input.MoveModeFirst10X;
                UnifiedSnapshotLegacyMoveModeFirst10Z = input.MoveModeFirst10Z;
            }

            if (UnifiedSnapshotLegacySoARows == null ||
                UnifiedSnapshotLegacySoARows.Capacity != capacity)
            {
                UnifiedSnapshotLegacySoARows = new AiSoASensingRows(capacity);
            }
            if (UnifiedSnapshotLegacyDecisionRows == null ||
                UnifiedSnapshotLegacyDecisionRows.Capacity != capacity)
            {
                UnifiedSnapshotLegacyDecisionRows =
                    new AiSoASensingRows(capacity);
            }
            if (UnifiedSnapshotLegacySharedSnapshot == null ||
                !ReferenceEquals(
                    UnifiedSnapshotLegacySharedSnapshot.Rows,
                    UnifiedSnapshotLegacyDecisionRows))
            {
                UnifiedSnapshotLegacySharedSnapshot =
                    new AiDecisionSnapshot(UnifiedSnapshotLegacyDecisionRows);
            }
            if (UnifiedSnapshotLegacyIndexedSnapshot == null ||
                !ReferenceEquals(
                    UnifiedSnapshotLegacyIndexedSnapshot.Rows,
                    UnifiedSnapshotLegacyDecisionRows))
            {
                UnifiedSnapshotLegacyIndexedSnapshot =
                    new AiDecisionSnapshot(UnifiedSnapshotLegacyDecisionRows);
            }
            if (UnifiedSnapshotLegacyInputSlots == null ||
                UnifiedSnapshotLegacyInputSlots.Length != capacity)
            {
                UnifiedSnapshotLegacyInputSlots = new LF2Entity[capacity];
            }
            if (UnifiedSnapshotLegacyMoveModeFirst10Present == null ||
                UnifiedSnapshotLegacyMoveModeFirst10Present.Length != 10)
            {
                UnifiedSnapshotLegacyMoveModeFirst10Present = new bool[10];
                UnifiedSnapshotLegacyMoveModeFirst10Eligible = new bool[10];
                UnifiedSnapshotLegacyMoveModeFirst10Generation = new uint[10];
                UnifiedSnapshotLegacyMoveModeFirst10Hp = new int[10];
                UnifiedSnapshotLegacyMoveModeFirst10X = new int[10];
                UnifiedSnapshotLegacyMoveModeFirst10Z = new int[10];
            }
        }

        internal void RestoreLegacyConsumerBuffers()
        {
            int capacity = runtimeSlots.LogicalCapacity;
            if (capacity <= 0)
                return;

            PrepareLegacyConsumerBuffers(capacity);
            sensing.Rows = UnifiedSnapshotLegacySoARows;
            SharedRows = UnifiedSnapshotLegacyDecisionRows;
            SharedSnapshot = UnifiedSnapshotLegacySharedSnapshot;
            IndexedSnapshot = UnifiedSnapshotLegacyIndexedSnapshot;
            input.Slots = UnifiedSnapshotLegacyInputSlots;
            input.MoveModeFirst10Present =
                UnifiedSnapshotLegacyMoveModeFirst10Present;
            input.MoveModeFirst10Eligible =
                UnifiedSnapshotLegacyMoveModeFirst10Eligible;
            input.MoveModeFirst10Generation =
                UnifiedSnapshotLegacyMoveModeFirst10Generation;
            input.MoveModeFirst10Hp = UnifiedSnapshotLegacyMoveModeFirst10Hp;
            input.MoveModeFirst10X = UnifiedSnapshotLegacyMoveModeFirst10X;
            input.MoveModeFirst10Z = UnifiedSnapshotLegacyMoveModeFirst10Z;
            input.MoveModeTopSlot = -1;
            input.MoveModeTopX = -1;
            input.MoveModeTopZ = 0;
            input.MoveModeSecondSlot = -1;
            input.MoveModeSecondX = -1;
            input.MoveModeSecondZ = 0;
            input.MoveModeFirst10Valid = false;
        }

        internal void ResetUnifiedMoveModeFirst10Snapshot()
        {
            Array.Clear(
                UnifiedMoveModeFirst10Present,
                0,
                UnifiedMoveModeFirst10Present.Length);
            Array.Clear(
                UnifiedMoveModeFirst10Eligible,
                0,
                UnifiedMoveModeFirst10Eligible.Length);
            Array.Clear(
                UnifiedMoveModeFirst10Generation,
                0,
                UnifiedMoveModeFirst10Generation.Length);
            Array.Clear(UnifiedMoveModeFirst10Hp, 0, UnifiedMoveModeFirst10Hp.Length);
            Array.Clear(UnifiedMoveModeFirst10X, 0, UnifiedMoveModeFirst10X.Length);
            Array.Clear(UnifiedMoveModeFirst10Z, 0, UnifiedMoveModeFirst10Z.Length);
            UnifiedMoveModeTopSlot = -1;
            UnifiedMoveModeTopX = -1;
            UnifiedMoveModeTopZ = 0;
            UnifiedMoveModeSecondSlot = -1;
            UnifiedMoveModeSecondX = -1;
            UnifiedMoveModeSecondZ = 0;
            UnifiedMoveModeFirst10Valid = false;
        }

        internal void RecordLegacyRng(int modulus, int raw, int value)
        {
            int index = LegacyRngCount;
            if (index < LegacyRngModuli.Length)
            {
                LegacyRngModuli[index] = modulus;
                LegacyRngRaw[index] = raw;
                LegacyRngValues[index] = value;
            }
            else
            {
                LegacyRngOverflow = true;
            }

            unchecked
            {
                LegacyRngOrderHash ^= (uint)modulus;
                LegacyRngOrderHash *= 1099511628211UL;
                LegacyRngOrderHash ^= (uint)raw;
                LegacyRngOrderHash *= 1099511628211UL;
                LegacyRngOrderHash ^= (uint)value;
                LegacyRngOrderHash *= 1099511628211UL;
            }
            LegacyRngCount++;
        }

        internal AiDecisionShadowMismatchReason CompareShadowResult(
            AiDecisionInputState actualInput,
            AiDecisionWorldState actualWorld,
            uint rngState,
            ulong rngCalls,
            int characterDecisionPosition)
        {
            AiDecisionSnapshot comparisonSnapshot = ComparisonSnapshot;
            if (comparisonSnapshot == null)
                return AiDecisionShadowMismatchReason.SnapshotUnavailable;
            if (!InputEquals(ShadowExpected.Input, actualInput))
                return AiDecisionShadowMismatchReason.Input;
            if (!WorldEquals(ShadowExpected.World, actualWorld))
                return AiDecisionShadowMismatchReason.WorldFlow;
            if (rngState != ShadowExpected.RngState)
                return AiDecisionShadowMismatchReason.RngState;
            if (characterDecisionPosition !=
                ShadowExpected.CharacterDecisionPosition)
            {
                return AiDecisionShadowMismatchReason.CharacterDecisionPosition;
            }
            if (rngCalls != ShadowExpected.RngCalls)
                return AiDecisionShadowMismatchReason.RngCalls;
            if (LegacyRngOverflow || ShadowExpected.RngTraceOverflow)
                return AiDecisionShadowMismatchReason.RngTraceOverflow;
            if (LegacyRngCount != ShadowExpected.RngDrawCount ||
                LegacyRngOrderHash != ShadowExpected.RngOrderHash)
            {
                return AiDecisionShadowMismatchReason.RngOrder;
            }
            for (int index = 0; index < LegacyRngCount; index++)
            {
                if (LegacyRngModuli[index] != comparisonSnapshot.RngTraceModuli[index] ||
                    LegacyRngRaw[index] != comparisonSnapshot.RngTraceRaw[index] ||
                    LegacyRngValues[index] != comparisonSnapshot.RngTraceValues[index])
                {
                    return AiDecisionShadowMismatchReason.RngOrder;
                }
            }
            return AiDecisionShadowMismatchReason.None;
        }

        internal static bool TryCaptureInputState(
            NTSDEntityRuntime runtime,
            out AiDecisionInputState inputState)
        {
            inputState = default;
            int[] history = runtime?.InputHistory;
            if (runtime == null || history == null || history.Length != 6)
                return false;

            inputState.History0 = history[0];
            inputState.History1 = history[1];
            inputState.History2 = history[2];
            inputState.History3 = history[3];
            inputState.History4 = history[4];
            inputState.History5 = history[5];
            inputState.CdAttack = runtime.CdAttack;
            inputState.CdJump = runtime.CdJump;
            inputState.CdDefend = runtime.CdDefend;
            inputState.CdDefendLock = runtime.CdDefendLock;
            inputState.CdRight = runtime.CdRight;
            inputState.CdLeft = runtime.CdLeft;
            inputState.CdUp = runtime.CdUp;
            inputState.CdDown = runtime.CdDown;
            inputState.ComboDra = runtime.ComboDra;
            inputState.ComboDla = runtime.ComboDla;
            inputState.ComboDua = runtime.ComboDua;
            inputState.ComboDda = runtime.ComboDda;
            inputState.ComboDrj = runtime.ComboDrj;
            inputState.ComboDlj = runtime.ComboDlj;
            inputState.ComboDuj = runtime.ComboDuj;
            inputState.ComboDdj = runtime.ComboDdj;
            inputState.ComboDja = runtime.ComboDja;
            inputState.PrevUp = runtime.PrevUp;
            inputState.PrevDown = runtime.PrevDown;
            inputState.PrevLeft = runtime.PrevLeft;
            inputState.PrevRight = runtime.PrevRight;
            inputState.PrevJump = runtime.PrevJump;
            inputState.PrevDefend = runtime.PrevDefend;
            inputState.PrevAttack = runtime.PrevAttack;
            inputState.KeyUp = runtime.KeyUp;
            inputState.KeyDown = runtime.KeyDown;
            inputState.KeyLeft = runtime.KeyLeft;
            inputState.KeyRight = runtime.KeyRight;
            inputState.KeyAttack = runtime.KeyAttack;
            inputState.KeyJump = runtime.KeyJump;
            inputState.KeyDefend = runtime.KeyDefend;
            inputState.Unk360 = runtime.Unk360;
            inputState.Unk3FC = runtime.Unk3FC;
            inputState.Unk400 = runtime.Unk400;
            inputState.BoundaryFlags = CaptureBoundaryFlags(runtime);
            return true;
        }

        internal static int CaptureBoundaryFlags(NTSDEntityRuntime runtime)
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

        internal void BeginProductionMutationWitnessPass(
            AiUnifiedSnapshotConsumer consumer,
            ulong epoch)
        {
            if (UnifiedShadowMode != AiUnifiedSnapshotShadowMode.Shadow)
                return;

            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    SoASensingMutationWitnessOrdinal = 0;
                    SoASensingRoleIndexVersion = 1;
                    SoASensingTeamSummaryVersion = 1;
                    soASensingMutationWitness = default;
                    soASensingMutationWitness.Epoch = epoch;
                    break;
                case AiUnifiedSnapshotConsumer.IndexedDecision:
                    DecisionMutationWitnessOrdinal = 0;
                    DecisionRoleIndexVersion = 1;
                    DecisionTeamSummaryVersion = 1;
                    decisionMutationWitness = default;
                    decisionMutationWitness.Epoch = epoch;
                    break;
            }
        }

        internal void RecordProductionMutationWitness(
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
            if (UnifiedShadowMode != AiUnifiedSnapshotShadowMode.Shadow)
                return;

            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    if (roleRebuilt)
                        SoASensingRoleIndexVersion++;
                    if (teamRebuilt)
                        SoASensingTeamSummaryVersion++;
                    SoASensingMutationWitnessOrdinal++;
                    soASensingMutationWitness = CreateMutationWitness(
                        epoch,
                        SoASensingMutationWitnessOrdinal,
                        slot,
                        generation,
                        stableId,
                        roleRebuilt,
                        teamRebuilt,
                        SoASensingRoleIndexVersion,
                        SoASensingTeamSummaryVersion,
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
                        DecisionRoleIndexVersion++;
                    if (teamRebuilt)
                        DecisionTeamSummaryVersion++;
                    DecisionMutationWitnessOrdinal++;
                    decisionMutationWitness = CreateMutationWitness(
                        epoch,
                        DecisionMutationWitnessOrdinal,
                        slot,
                        generation,
                        stableId,
                        roleRebuilt,
                        teamRebuilt,
                        DecisionRoleIndexVersion,
                        DecisionTeamSummaryVersion,
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

        internal void RecordUnifiedMutationWitness(
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
                UnifiedSnapshotRoleIndexVersion++;
            if (teamRebuilt)
                UnifiedSnapshotTeamSummaryVersion++;
            UnifiedSnapshotMutationWitnessOrdinal++;
            unifiedSnapshotMutationWitness = CreateMutationWitness(
                epoch,
                UnifiedSnapshotMutationWitnessOrdinal,
                slot,
                generation,
                stableId,
                roleRebuilt,
                teamRebuilt,
                UnifiedSnapshotRoleIndexVersion,
                UnifiedSnapshotTeamSummaryVersion,
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

        internal void CommitExecutionCandidate(
            AiUnifiedSnapshotExecutionState candidate)
        {
            AiUnifiedSnapshotExecutionState previous =
                UnifiedSnapshotPublishedState;
            UnifiedSnapshotPublishedState = candidate;
            if (previous != null)
            {
                UnifiedSnapshotScratchState = previous;
            }
            else
            {
                UnifiedSnapshotScratchState = UnifiedSnapshotStandbyState;
                UnifiedSnapshotStandbyState = null;
            }
        }

        internal void EndExecutionPassState()
        {
            UnifiedSnapshotExecutionCommittedThisPass = false;
            UnifiedSnapshotExecutionConsumerStartedThisPass = false;
        }

        internal bool TryPrepareUnifiedExecutionPass(
            SimulationWorld world,
            BattleAiUnifiedRowPublisher rowPublisher,
            bool forceFullRebuild)
        {
            if (UnifiedExecutionMode !=
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                return false;
            }

            UnifiedExecutionBuildCount++;
            if (!forceFullRebuild &&
                TryRollForwardUnifiedExecutionPass(world, rowPublisher))
            {
                return true;
            }

            rowPublisher.EndPass();
            EndExecutionPassState();
            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Prepare;
            try
            {
                if (sensing.Mode != AiSensingMode.SoAAiSensing ||
                    ExecutionMode != AiDecisionExecutionMode.IndexedCanonical ||
                    UnifiedShadowMode != AiUnifiedSnapshotShadowMode.Disabled)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority configuration changed before its build.");
                }

                int capacity = runtimeSlots.LogicalCapacity;
                ulong epoch = runtimeSlots.OccupancyEpoch;
                if (capacity <= 0)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority requires positive runtime slot capacity.");
                }

                PrepareLegacyConsumerBuffers(capacity);
                EnsureExecutionScratchCapacity(capacity);
                AiUnifiedSnapshotExecutionState candidate =
                    UnifiedSnapshotScratchState;
                candidate.Reset(epoch, capacity);
                AiSoASensingRows rows = candidate.Rows;

                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                stage = AiUnifiedSnapshotExceptionStage.Capture;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                for (int slot = 0; slot < capacity; slot++)
                {
                    UnifiedExecutionSlotVisitCount++;
                    if (!runtimeSlots.IsAddressable(slot))
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority could not read the runtime slot table.");
                    }
                    RuntimeSlotTable.ReadOnlySlotView view =
                        runtimeSlots.GetReadOnlyView(slot);
                    if (view.RuntimeSlot != slot)
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
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;

                    candidate.FallbackSlots[slot] = entity;
                    if (!world.TryCaptureAiUnifiedAuthorityRowForDecisionModule(
                            rows,
                            entity,
                            slot,
                            view.Generation,
                            true,
                            out int decisionFlags))
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority could not capture an active row.");
                    }
                    UnifiedExecutionCanonicalInitialCaptureCount++;
                    if (slot < candidate.MoveModeFirst10Present.Length)
                    {
                        CaptureUnifiedMoveModeScratchCandidate(
                            candidate,
                            rows,
                            slot,
                            view.Generation);
                    }

                    candidate.SoASensingBoundaryFlags[slot] =
                        rows.BoundaryFlags[slot];
                    candidate.DecisionBoundaryFlags[slot] = decisionFlags;
                }

                stage = AiUnifiedSnapshotExceptionStage.BuildIndexes;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                rows.SpecialIndexReady = true;
                SimulationAiSensingModule.BuildRoleIndexes(rows);
                SimulationAiSensingModule.BuildTeamSummaries(rows);

                stage = AiUnifiedSnapshotExceptionStage.Validate;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                candidate.MoveModeFirst10Valid = true;
                if (!ValidateExecutionPreCommit(candidate, capacity, epoch))
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority pre-commit validation failed.");
                }

                CommitExecutionCandidate(candidate);
                world.ActivateAiUnifiedSnapshotExecutionPassForDecisionModule(candidate);
                stage = AiUnifiedSnapshotExceptionStage.InitialSensingCompare;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                return true;
            }
            catch (Exception exception)
            {
                if (UnifiedSnapshotExecutionCommittedThisPass ||
                    UnifiedSnapshotExecutionConsumerStartedThisPass)
                {
                    RecordUnifiedExecutionFailure(stage, exception, true);
                    UnifiedSnapshotExecutionCommittedThisPass = false;
                    sensing.PassInvalidated = true;
                    InvalidateSharedPass(AiDecisionAvailability.SnapshotMissing);
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority failed after publication; same-tick fallback is forbidden.",
                        exception);
                }
                RecordUnifiedExecutionFailure(stage, exception, false);
                UnifiedExecutionPreCommitFailureCount++;
                UnifiedExecutionPreCommitFallbackCount++;
                RestoreLegacyConsumerBuffers();
                return false;
            }
        }

        internal bool TryRollForwardUnifiedExecutionPass(
            SimulationWorld world,
            BattleAiUnifiedRowPublisher rowPublisher)
        {
            AiUnifiedSnapshotExecutionState published =
                UnifiedSnapshotPublishedState;
            int capacity = runtimeSlots.LogicalCapacity;
            ulong epoch = runtimeSlots.OccupancyEpoch;
            if (!rowPublisher.Active ||
                published == null ||
                capacity <= 0 ||
                published.Capacity != capacity ||
                published.Epoch != epoch ||
                rowPublisher.Epoch != epoch)
            {
                return false;
            }

            try
            {
                AiSoASensingRows rows = published.Rows;
                int dirtySlotCount = rowPublisher.PendingSlotCount;
                bool roleProductsChanged = false;
                bool teamProductsChanged = false;
                for (int index = 0; index < dirtySlotCount; index++)
                {
                    int slot = rowPublisher.GetPendingSlot(index);
                    if ((uint)slot >= (uint)capacity ||
                        !rows.Included[slot] ||
                        rows.Generation[slot] == 0 ||
                        !rowPublisher.TryCommitPending(
                            slot,
                            rows.Generation[slot],
                            out bool slotRoleProductsChanged,
                            out bool slotTeamProductsChanged))
                    {
                        throw new InvalidOperationException(
                            "Unified AI rolling snapshot observed a stale dirty slot.");
                    }

                    roleProductsChanged |= slotRoleProductsChanged;
                    teamProductsChanged |= slotTeamProductsChanged;
                }

                if (roleProductsChanged)
                    SimulationAiSensingModule.BuildRoleIndexes(rows);
                if (teamProductsChanged)
                    SimulationAiSensingModule.BuildTeamSummaries(rows);

                RebuildUnifiedMoveModeFirst10Product(published);
                if (world.ValidateIncrementalAiUnifiedRowForDiagnostics &&
                    !ValidateExecutionPreCommit(published, capacity, epoch))
                {
                    throw new InvalidOperationException(
                        "Unified AI rolling snapshot validation failed.");
                }

                world.ActivateAiUnifiedSnapshotExecutionPassForDecisionModule(published);
                UnifiedExecutionRollForwardCount++;
                UnifiedExecutionRollForwardDirtySlotCount += dirtySlotCount;
                return true;
            }
            catch (Exception exception)
            {
                RecordUnifiedExecutionFailure(
                    AiUnifiedSnapshotExceptionStage.Prepare,
                    exception,
                    false);
                UnifiedExecutionPreCommitFailureCount++;
                UnifiedExecutionPreCommitFallbackCount++;
                return false;
            }
        }

        internal void RecordUnifiedExecutionFailure(
            AiUnifiedSnapshotExceptionStage stage,
            Exception exception,
            bool postCommit)
        {
            if (UnifiedExecutionFirstFailureStage ==
                AiUnifiedSnapshotExceptionStage.None)
            {
                UnifiedExecutionFirstFailureStage = stage;
                UnifiedExecutionFirstFailureType = exception?.GetType();
            }
            if (postCommit)
                UnifiedExecutionPostCommitHardBreachCount++;
        }

        internal void ActivateUnifiedExecutionPass(
            BattleAiUnifiedRowPublisher rowPublisher,
            AiUnifiedSnapshotExecutionState candidate)
        {
            sensing.Rows = candidate.Rows;
            SharedRows = candidate.Rows;
            SharedSnapshot = candidate.SharedSnapshot;
            IndexedSnapshot = candidate.IndexedSnapshot;
            input.Slots = candidate.FallbackSlots;
            input.MoveModeFirst10Present = candidate.MoveModeFirst10Present;
            input.MoveModeFirst10Eligible = candidate.MoveModeFirst10Eligible;
            input.MoveModeFirst10Generation = candidate.MoveModeFirst10Generation;
            input.MoveModeFirst10Hp = candidate.MoveModeFirst10Hp;
            input.MoveModeFirst10X = candidate.MoveModeFirst10X;
            input.MoveModeFirst10Z = candidate.MoveModeFirst10Z;
            input.MoveModeTopSlot = candidate.MoveModeTopSlot;
            input.MoveModeTopX = candidate.MoveModeTopX;
            input.MoveModeTopZ = candidate.MoveModeTopZ;
            input.MoveModeSecondSlot = candidate.MoveModeSecondSlot;
            input.MoveModeSecondX = candidate.MoveModeSecondX;
            input.MoveModeSecondZ = candidate.MoveModeSecondZ;
            input.MoveModeFirst10Valid = candidate.MoveModeFirst10Valid;

            AiSoASensingRows rows = candidate.Rows;
            rowPublisher.BeginPass(
                candidate.Epoch,
                rows.Included,
                rows.Generation,
                rows.DataObjectType,
                rows.InputHistoryGate,
                rows.X,
                rows.Y,
                rows.Z,
                rows.Hp,
                rows.Hp3,
                rows.HpMax,
                rows.Pp,
                rows.Team,
                rows.State,
                rows.Frame,
                rows.HitJ,
                rows.LinkState,
                rows.KillCount,
                rows.CachedTargetSlot,
                rows.CoordinateTargetX,
                rows.Vx,
                rows.Facing,
                rows.TargetSlot,
                rows.HitStop,
                rows.BoundaryFlags,
                candidate.SoASensingBoundaryFlags,
                candidate.DecisionBoundaryFlags);

            UnifiedSnapshotExecutionCommittedThisPass = true;
            UnifiedSnapshotExecutionConsumerStartedThisPass = false;
            sensing.SnapshotEpoch = candidate.Epoch;
            sensing.SnapshotValid = true;
            sensing.PassInvalidated = false;
            sensing.CandidatePassLatchedToLegacy = false;
            SharedPassEpoch = candidate.Epoch;
            SharedPassUnavailableReason = AiDecisionAvailability.None;
            SharedPassAvailable = true;
            input.SlotSnapshotOccupancyEpoch = candidate.Epoch;
            UnifiedExecutionCommittedPassCount++;
        }

        internal bool ValidateUnifiedExecutionState(
            SimulationWorld world,
            AiUnifiedSnapshotExecutionState candidate,
            int capacity,
            ulong epoch)
        {
            if (candidate == null ||
                candidate.ExpectedCapacity != capacity ||
                candidate.Capacity != capacity ||
                candidate.Epoch != epoch ||
                candidate.Rows.CapturedOccupancyEpoch != epoch ||
                runtimeSlots.LogicalCapacity != capacity ||
                runtimeSlots.OccupancyEpoch != epoch ||
                !AiSensingKernel.ValidateIndexedContract(candidate.Rows))
            {
                return false;
            }

            AiSoASensingRows rows = candidate.Rows;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!runtimeSlots.IsAddressable(slot))
                    return false;
                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                if (view.RuntimeSlot != slot)
                    return false;

                LF2Entity entity = view.Entity;
                bool shouldBeIncluded =
                    view.Claimed &&
                    entity != null &&
                    world.IsActiveForCurrentPassInternal(entity);
                if (rows.Included[slot] != shouldBeIncluded ||
                    !ReferenceEquals(
                        candidate.FallbackSlots[slot],
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
                        (slot >= 20 &&
                         SimulationAiInputModule.IsSpecialScanObjectId(
                             rows.ObjectId[slot])) ||
                    candidate.SoASensingBoundaryFlags[slot] !=
                        SimulationAiSensingModule.CaptureBoundaryFlags(runtime) ||
                    candidate.DecisionBoundaryFlags[slot] !=
                        CaptureBoundaryFlags(runtime))
                {
                    return false;
                }

                if (slot >= candidate.MoveModeFirst10Present.Length)
                    continue;
                bool eligible = world.IsLivingCharacterDat(entity);
                if (!candidate.MoveModeFirst10Present[slot] ||
                    candidate.MoveModeFirst10Generation[slot] != view.Generation ||
                    candidate.MoveModeFirst10Hp[slot] != world.Hp(entity) ||
                    candidate.MoveModeFirst10Eligible[slot] != eligible ||
                    eligible &&
                    (candidate.MoveModeFirst10X[slot] != world.X(entity) ||
                     candidate.MoveModeFirst10Z[slot] != world.Z(entity)))
                {
                    return false;
                }
            }

            return candidate.MoveModeFirst10Valid;
        }

        internal static bool TryCaptureUnifiedAuthorityRow(
            BattleIdentityWriter identityWriter,
            BattleFrameMotionWriter frameMotionWriter,
            BattleCharacterInputWriter characterInputWriter,
            BattleRelationLinkWriter relationLinkWriter,
            BattleVitalWriter vitalWriter,
            AiSoASensingRows rows,
            LF2Entity entity,
            int slot,
            uint generation,
            bool captureSpecialMembership,
            out int decisionBoundaryFlags)
        {
            decisionBoundaryFlags = 0;
            NTSDEntityRuntime runtime = entity?.Runtime;
            var handle = new RuntimeEntityHandle(slot, generation);
            if (rows == null ||
                runtime == null ||
                !handle.IsValid ||
                slot < 0 ||
                slot >= rows.Capacity ||
                runtime.SlotIndex != slot ||
                !identityWriter.TryCaptureAiProjection(
                    handle,
                    out BattleIdentityAiProjection identity) ||
                !frameMotionWriter.TryCaptureAiProjection(
                    handle,
                    out BattleFrameMotionAiProjection frameMotion) ||
                !characterInputWriter.TryCaptureAiProjection(
                    handle,
                    out BattleCharacterInputAiProjection inputProjection) ||
                !relationLinkWriter.TryCaptureAiProjection(
                    handle,
                    out BattleRelationLinkAiProjection relationLink) ||
                !vitalWriter.TryCaptureAiProjection(
                    handle,
                    out BattleVitalAiProjection vital))
            {
                return false;
            }

            int objectId = identity.ObjectId;
            rows.Included[slot] = true;
            if (captureSpecialMembership)
            {
                bool specialScanMember =
                    slot >= 20 && SimulationAiInputModule.IsSpecialScanObjectId(objectId);
                rows.SpecialScanMember[slot] = specialScanMember;
                if (specialScanMember)
                    rows.SpecialSlots[rows.SpecialSlotCount++] = slot;
            }

            rows.InputHistoryGate[slot] = inputProjection.InputHistoryGate;
            rows.Generation[slot] = generation;
            rows.Identity[slot] = identity.StableId;
            rows.ObjectId[slot] = objectId;
            rows.DataObjectType[slot] = identity.DataObjectType;
            rows.X[slot] = frameMotion.X;
            rows.Y[slot] = frameMotion.Y;
            rows.Z[slot] = frameMotion.Z;
            rows.Hp[slot] = vital.Hp;
            rows.Hp3[slot] = vital.Hp3;
            rows.HpMax[slot] = vital.HpBound;
            rows.Pp[slot] = vital.Pp;
            rows.Team[slot] = relationLink.RelationTeam;
            rows.State[slot] = frameMotion.State;
            rows.Frame[slot] = frameMotion.Frame;
            rows.HitJ[slot] = frameMotion.HitJ;
            rows.LinkState[slot] = relationLink.LinkState;
            rows.KillCount[slot] = relationLink.KillCount;
            rows.CachedTargetSlot[slot] = inputProjection.CachedTargetSlot;
            rows.CoordinateTargetX[slot] = inputProjection.CoordinateTargetX;
            rows.Vx[slot] = frameMotion.Vx;
            rows.Facing[slot] = frameMotion.Facing;
            rows.TargetSlot[slot] = relationLink.TargetSlot;
            rows.HitStop[slot] = frameMotion.HitStop;
            decisionBoundaryFlags = inputProjection.DecisionBoundaryFlags;
            rows.BoundaryFlags[slot] =
                BattleAiUnifiedRowPublisher.ToSensingBoundaryFlags(
                    decisionBoundaryFlags);
            return true;
        }

        internal void ObserveUnifiedExecutionMoveModeFirst10Mutation(
            BattleIdentityWriter identityWriter,
            AiUnifiedSnapshotExecutionState published,
            LF2Entity entity,
            int slot,
            uint generation)
        {
            if (!published.MoveModeFirst10Valid)
                return;
            AiSoASensingRows rows = published.Rows;
            if (entity == null || rows == null)
            {
                published.MoveModeFirst10Valid = false;
                input.MoveModeFirst10Valid = false;
                return;
            }

            if (slot < 0 || slot >= published.MoveModeFirst10Present.Length)
                return;

            if (!published.MoveModeFirst10Present[slot] ||
                !ReferenceEquals(published.FallbackSlots[slot], entity) ||
                rows.Generation[slot] != generation ||
                generation != published.MoveModeFirst10Generation[slot] ||
                rows.Hp[slot] != published.MoveModeFirst10Hp[slot] ||
                !identityWriter.TryCaptureAiProjection(
                    new RuntimeEntityHandle(slot, generation),
                    out BattleIdentityAiProjection identity))
            {
                published.MoveModeFirst10Valid = false;
                input.MoveModeFirst10Valid = false;
                return;
            }

            bool eligible = identity.DataObjectType ==
                                (int)LF2ObjectType.Character &&
                            rows.Hp[slot] > 0;
            if (eligible != published.MoveModeFirst10Eligible[slot] ||
                eligible &&
                (rows.X[slot] != published.MoveModeFirst10X[slot] ||
                 rows.Z[slot] != published.MoveModeFirst10Z[slot]))
            {
                published.MoveModeFirst10Valid = false;
                input.MoveModeFirst10Valid = false;
            }
        }

        internal void BeginUnifiedExecutionConsumer(LF2Entity entity)
        {
            if (!UnifiedSnapshotExecutionCommittedThisPass || entity == null)
                return;

            UnifiedSnapshotExecutionConsumerStartedThisPass = true;
            if (!entity.AiControlled)
                return;

            UnifiedExecutionReadCount++;
#if UNITY_INCLUDE_TESTS
            int observerSlot = entity.Runtime?.SlotIndex ?? -1;
            AiUnifiedSnapshotExecutionState published =
                UnifiedSnapshotPublishedState;
            if (published != null &&
                observerSlot == UnifiedExecutionProbeObserverSlotAForSelfCheck)
            {
                int target = UnifiedExecutionProbeTargetSlotAForSelfCheck;
                UnifiedExecutionProbeStateAForSelfCheck =
                    target >= 0 && target < published.Capacity &&
                    published.Rows.Included[target]
                        ? published.Rows.State[target]
                        : int.MinValue;
            }
            if (published != null &&
                observerSlot == UnifiedExecutionProbeObserverSlotBForSelfCheck)
            {
                int target = UnifiedExecutionProbeTargetSlotBForSelfCheck;
                UnifiedExecutionProbeStateBForSelfCheck =
                    target >= 0 && target < published.Capacity &&
                    published.Rows.Included[target]
                        ? published.Rows.State[target]
                        : int.MinValue;
            }
#endif
        }

        internal void PrepareUnifiedShadowPass(
            SimulationWorld world,
            BattleAiInputDetailDiagnostics diagnostics)
        {
            if (UnifiedShadowMode != AiUnifiedSnapshotShadowMode.Shadow)
                return;

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Prepare;
            try
            {
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                PrepareUnifiedShadowPassCore(world, diagnostics, ref stage);
            }
            catch (Exception exception)
            {
                RecordUnifiedShadowException(stage, exception);
                InvalidateUnifiedShadowPass();
            }
        }

        internal void CompleteUnifiedShadowInitialComparison(SimulationWorld world)
        {
            if (UnifiedSnapshotExecutionCommittedThisPass)
            {
                AiUnifiedSnapshotExceptionStage executionStage =
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare;
                try
                {
                    ThrowUnifiedSnapshotExceptionForSelfCheck(executionStage);
                }
                catch (Exception exception)
                {
                    RecordUnifiedExecutionFailure(executionStage, exception, true);
                    UnifiedSnapshotExecutionCommittedThisPass = false;
                    sensing.PassInvalidated = true;
                    InvalidateSharedPass(AiDecisionAvailability.SnapshotMissing);
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority failed at the initial decision boundary; same-tick fallback is forbidden.",
                        exception);
                }
                return;
            }
            if (UnifiedShadowMode != AiUnifiedSnapshotShadowMode.Shadow)
                return;

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.InitialDecisionCompare;
            try
            {
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                world.CompareAiUnifiedSnapshotShadowForDecisionModule(
                    AiUnifiedSnapshotConsumer.IndexedDecision,
                    true,
                    -1);
            }
            catch (Exception exception)
            {
                RecordUnifiedShadowException(stage, exception);
                InvalidateUnifiedShadowPass();
            }
        }

        internal void RefreshUnifiedExecutionRowAfterCharacterInput(
            SimulationWorld world,
            BattleAiUnifiedRowPublisher rowPublisher,
            BattleIdentityWriter identityWriter,
            BattleFrameMotionWriter frameMotionWriter,
            BattleCharacterInputWriter characterInputWriter,
            BattleRelationLinkWriter relationLinkWriter,
            BattleVitalWriter vitalWriter,
            LF2Entity entity,
            bool forceFullPostRefresh,
            bool validateIncrementalRow)
        {
            if (!UnifiedSnapshotExecutionCommittedThisPass)
                return;

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Refresh;
            try
            {
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                AiUnifiedSnapshotExecutionState published =
                    UnifiedSnapshotPublishedState;
                if (published == null)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority has no published state.");
                }
                if (runtimeSlots.OccupancyEpoch != published.Epoch ||
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
                    published.FallbackSlots[slot] != entity ||
                    rows.Generation[slot] == 0 ||
                    rows.Identity[slot] != entity.Runtime.StableId)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority row identity changed after commit.");
                }

                uint generation = rows.Generation[slot];
                stage = AiUnifiedSnapshotExceptionStage.RefreshCapture;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                if (UnifiedSnapshotNoPendingRefreshSkip &&
                    !validateIncrementalRow &&
                    !forceFullPostRefresh &&
                    slot >= published.MoveModeFirst10Present.Length &&
                    !rowPublisher.HasPendingValues(slot, generation))
                {
                    UnifiedExecutionNoPendingRefreshSkipCount++;
                    UnifiedExecutionRefreshCount++;
                    return;
                }

                bool roleProductsChanged;
                bool teamProductsChanged;
                bool captured;
                if (forceFullPostRefresh)
                {
                    int previousX = rows.X[slot];
                    int previousTeam = rows.Team[slot];
                    int previousHp = rows.Hp[slot];
                    int previousObjectId = rows.ObjectId[slot];
                    bool previousSpecialMember = rows.SpecialScanMember[slot];
                    bool wasGroundRole =
                        SimulationAiSensingModule.IsGroundRoleMember(rows, slot);
                    bool wasAirRole =
                        SimulationAiSensingModule.IsAirRoleMember(rows, slot);
                    bool wasLivingCharacter =
                        SimulationAiSensingModule.IsLivingCharacterRow(rows, slot);
                    captured = SimulationAiSensingModule.TryCaptureRow(
                        rows,
                        entity,
                        slot,
                        generation,
                        false,
                        true);
                    bool currentSpecialMember =
                        slot >= 20 &&
                        SimulationAiInputModule.IsSpecialScanObjectId(
                            rows.ObjectId[slot]);
                    if (previousObjectId != rows.ObjectId[slot] ||
                        previousSpecialMember != currentSpecialMember)
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority special membership changed after commit.");
                    }

                    bool isGroundRole =
                        SimulationAiSensingModule.IsGroundRoleMember(rows, slot);
                    bool isAirRole =
                        SimulationAiSensingModule.IsAirRoleMember(rows, slot);
                    roleProductsChanged = previousX != rows.X[slot] ||
                                          previousTeam != rows.Team[slot] ||
                                          wasGroundRole != isGroundRole ||
                                          wasAirRole != isAirRole;
                    bool isLivingCharacter =
                        SimulationAiSensingModule.IsLivingCharacterRow(rows, slot);
                    teamProductsChanged =
                        wasLivingCharacter != isLivingCharacter ||
                        previousTeam != rows.Team[slot] ||
                        previousHp != rows.Hp[slot];

                    if (!rowPublisher.TryDiscardPending(slot, generation))
                        captured = false;

                    int sensingFlags =
                        SimulationAiSensingModule.CaptureBoundaryFlags(entity.Runtime);
                    int decisionFlags = CaptureBoundaryFlags(entity.Runtime);
                    rows.BoundaryFlags[slot] = sensingFlags;
                    published.SoASensingBoundaryFlags[slot] = sensingFlags;
                    published.DecisionBoundaryFlags[slot] = decisionFlags;
                }
                else
                {
                    captured = rowPublisher.TryCommitPending(
                        slot,
                        generation,
                        out roleProductsChanged,
                        out teamProductsChanged);
                    if (captured && validateIncrementalRow)
                    {
                        captured = ValidateUnifiedExecutionRowAfterCharacterInput(
                            identityWriter,
                            frameMotionWriter,
                            characterInputWriter,
                            relationLinkWriter,
                            vitalWriter,
                            published,
                            rows,
                            entity,
                            slot,
                            generation);
                        UnifiedExecutionIncrementalValidationCount++;
                    }
                }
                if (!captured)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority could not refresh a committed row.");
                }

                bool expectedSpecialMember =
                    slot >= 20 &&
                    SimulationAiInputModule.IsSpecialScanObjectId(rows.ObjectId[slot]);
                if (rows.SpecialScanMember[slot] != expectedSpecialMember)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority special membership changed after commit.");
                }

                stage = AiUnifiedSnapshotExceptionStage.RefreshBuildIndexes;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                if (roleProductsChanged)
                    SimulationAiSensingModule.BuildRoleIndexes(rows);
                if (teamProductsChanged)
                    SimulationAiSensingModule.BuildTeamSummaries(rows);

                ObserveUnifiedExecutionMoveModeFirst10Mutation(
                    identityWriter,
                    published,
                    entity,
                    slot,
                    generation);
                UnifiedExecutionRefreshCount++;
            }
            catch (Exception exception)
            {
                RecordUnifiedExecutionFailure(stage, exception, true);
                UnifiedSnapshotExecutionCommittedThisPass = false;
                sensing.PassInvalidated = true;
                InvalidateSharedPass(AiDecisionAvailability.SnapshotMissing);
                throw new InvalidOperationException(
                    "Unified AI snapshot authority hard breach after commit; same-tick fallback is forbidden.",
                    exception);
            }
        }

        private bool ValidateUnifiedExecutionRowAfterCharacterInput(
            BattleIdentityWriter identityWriter,
            BattleFrameMotionWriter frameMotionWriter,
            BattleCharacterInputWriter characterInputWriter,
            BattleRelationLinkWriter relationLinkWriter,
            BattleVitalWriter vitalWriter,
            AiUnifiedSnapshotExecutionState published,
            AiSoASensingRows rows,
            LF2Entity entity,
            int slot,
            uint generation)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            if (published == null ||
                rows == null ||
                runtime == null ||
                generation == 0 ||
                slot < 0 ||
                slot >= rows.Capacity ||
                runtime.SlotIndex != slot)
            {
                return false;
            }

            if (!frameMotionWriter.TryCaptureAiProjection(
                    runtime,
                    out BattleFrameMotionAiProjection frameMotion))
            {
                return false;
            }

            if (!runtimeSlots.TryGetCurrentHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != generation ||
                !identityWriter.TryCaptureAiProjection(
                    handle,
                    out BattleIdentityAiProjection identity))
            {
                return false;
            }

            if (!characterInputWriter.TryCaptureAiProjection(
                    runtime,
                    out BattleCharacterInputAiProjection inputProjection) ||
                !relationLinkWriter.TryCaptureAiProjection(
                    runtime,
                    out BattleRelationLinkAiProjection relationLink) ||
                !vitalWriter.TryCaptureAiProjection(
                    runtime,
                    out BattleVitalAiProjection vital))
            {
                return false;
            }

            return rows.Identity[slot] == identity.StableId &&
                   rows.ObjectId[slot] == identity.ObjectId &&
                   rows.DataObjectType[slot] == identity.DataObjectType &&
                   rows.InputHistoryGate[slot] == inputProjection.InputHistoryGate &&
                   rows.X[slot] == frameMotion.X &&
                   rows.Y[slot] == frameMotion.Y &&
                   rows.Z[slot] == frameMotion.Z &&
                   rows.Hp[slot] == vital.Hp &&
                   rows.Hp3[slot] == vital.Hp3 &&
                   rows.HpMax[slot] == vital.HpBound &&
                   rows.Pp[slot] == vital.Pp &&
                   rows.Team[slot] == relationLink.RelationTeam &&
                   rows.State[slot] == frameMotion.State &&
                   rows.Frame[slot] == frameMotion.Frame &&
                   rows.HitJ[slot] == frameMotion.HitJ &&
                   rows.LinkState[slot] == relationLink.LinkState &&
                   rows.KillCount[slot] == relationLink.KillCount &&
                   rows.CachedTargetSlot[slot] == inputProjection.CachedTargetSlot &&
                   rows.CoordinateTargetX[slot] == inputProjection.CoordinateTargetX &&
                   rows.Vx[slot].Equals(frameMotion.Vx) &&
                   rows.Facing[slot] == frameMotion.Facing &&
                   rows.TargetSlot[slot] == relationLink.TargetSlot &&
                   rows.HitStop[slot] == frameMotion.HitStop &&
                   rows.BoundaryFlags[slot] ==
                       BattleAiUnifiedRowPublisher.ToSensingBoundaryFlags(
                           inputProjection.DecisionBoundaryFlags) &&
                   published.SoASensingBoundaryFlags[slot] ==
                       rows.BoundaryFlags[slot] &&
                   published.DecisionBoundaryFlags[slot] ==
                       inputProjection.DecisionBoundaryFlags;
        }

        internal void RefreshUnifiedShadowRowAfterCharacterInput(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (UnifiedShadowMode != AiUnifiedSnapshotShadowMode.Shadow ||
                !UnifiedSnapshotPassAvailable)
            {
                return;
            }

            AiUnifiedSnapshotExceptionStage stage =
                AiUnifiedSnapshotExceptionStage.Refresh;
            try
            {
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
                RefreshUnifiedShadowRowAfterCharacterInputCore(
                    world,
                    entity,
                    ref stage);
            }
            catch (Exception exception)
            {
                RecordUnifiedShadowException(stage, exception);
                InvalidateUnifiedShadowPass();
            }
        }

        private void RefreshUnifiedShadowRowAfterCharacterInputCore(
            SimulationWorld world,
            LF2Entity entity,
            ref AiUnifiedSnapshotExceptionStage stage)
        {
            if (runtimeSlots.OccupancyEpoch != UnifiedSnapshotPassEpoch ||
                entity?.Runtime == null)
            {
                InvalidateUnifiedShadowPass();
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            AiSoASensingRows rows = UnifiedSnapshotRows;
            if (slot < 0 ||
                slot >= rows.Capacity ||
                !rows.Included[slot] ||
                !runtimeSlots.TryGetCurrentHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                rows.Generation[slot] != handle.Generation ||
                rows.Identity[slot] != entity.Runtime.StableId)
            {
                InvalidateUnifiedShadowPass();
                return;
            }

            int previousX = rows.X[slot];
            int previousTeam = rows.Team[slot];
            int previousHp = rows.Hp[slot];
            int previousObjectId = rows.ObjectId[slot];
            bool previousSpecialMember = rows.SpecialScanMember[slot];
            bool wasGroundRole =
                SimulationAiSensingModule.IsGroundRoleMember(rows, slot);
            bool wasAirRole =
                SimulationAiSensingModule.IsAirRoleMember(rows, slot);
            bool wasLivingCharacter =
                SimulationAiSensingModule.IsLivingCharacterRow(rows, slot);
            stage = AiUnifiedSnapshotExceptionStage.RefreshCapture;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
            if (!SimulationAiSensingModule.TryCaptureRow(
                    rows,
                    entity,
                    slot,
                    handle.Generation,
                    false))
            {
                InvalidateUnifiedShadowPass();
                return;
            }

            bool currentSpecialMember =
                slot >= 20 &&
                SimulationAiInputModule.IsSpecialScanObjectId(rows.ObjectId[slot]);
            if (previousObjectId != rows.ObjectId[slot] ||
                previousSpecialMember != currentSpecialMember)
            {
                InvalidateUnifiedShadowPass();
                return;
            }

            int sensingFlags =
                SimulationAiSensingModule.CaptureBoundaryFlags(entity.Runtime);
            int decisionFlags = CaptureBoundaryFlags(entity.Runtime);
            UnifiedSnapshotSoASensingBoundaryFlags[slot] = sensingFlags;
            UnifiedSnapshotDecisionBoundaryFlags[slot] = decisionFlags;
            if (sensingFlags != decisionFlags)
                UnifiedShadowDistinctBoundaryEncodingRowCount++;

            bool isGroundRole =
                SimulationAiSensingModule.IsGroundRoleMember(rows, slot);
            bool isAirRole =
                SimulationAiSensingModule.IsAirRoleMember(rows, slot);
            bool roleProductsChanged = previousX != rows.X[slot] ||
                                       previousTeam != rows.Team[slot] ||
                                       wasGroundRole != isGroundRole ||
                                       wasAirRole != isAirRole;
            bool isLivingCharacter =
                SimulationAiSensingModule.IsLivingCharacterRow(rows, slot);
            bool teamProductsChanged = wasLivingCharacter != isLivingCharacter ||
                                       previousTeam != rows.Team[slot] ||
                                       previousHp != rows.Hp[slot];
            stage = AiUnifiedSnapshotExceptionStage.RefreshBuildIndexes;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
            if (roleProductsChanged)
                SimulationAiSensingModule.BuildRoleIndexes(rows);
            if (teamProductsChanged)
                SimulationAiSensingModule.BuildTeamSummaries(rows);

            RecordUnifiedMutationWitness(
                UnifiedSnapshotPassEpoch,
                slot,
                handle.Generation,
                entity.Runtime.StableId,
                roleProductsChanged,
                teamProductsChanged,
                previousX,
                rows.X[slot],
                previousTeam,
                rows.Team[slot],
                PackRoleFlags(wasGroundRole, wasAirRole),
                PackRoleFlags(isGroundRole, isAirRole),
                wasLivingCharacter,
                isLivingCharacter,
                previousHp,
                rows.Hp[slot]);
            ObserveUnifiedMoveModeFirst10Mutation(entity);
            UnifiedShadowRefreshCount++;
            stage = AiUnifiedSnapshotExceptionStage.RefreshCompare;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
            world.CompareAiUnifiedSnapshotShadowForDecisionModule(
                AiUnifiedSnapshotConsumer.SoASensing,
                false,
                slot);
            world.CompareAiUnifiedSnapshotShadowForDecisionModule(
                AiUnifiedSnapshotConsumer.IndexedDecision,
                false,
                slot);
        }

        private void ObserveUnifiedMoveModeFirst10Mutation(LF2Entity entity)
        {
            if (!UnifiedMoveModeFirst10Valid)
                return;
            if (entity?.Runtime == null)
            {
                UnifiedMoveModeFirst10Valid = false;
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (slot < 0 || slot >= UnifiedMoveModeFirst10Present.Length)
            {
                for (int index = 0;
                     index < UnifiedMoveModeFirst10Present.Length;
                     index++)
                {
                    if (ReferenceEquals(UnifiedSnapshotFallbackSlots[index], entity))
                    {
                        UnifiedMoveModeFirst10Valid = false;
                        break;
                    }
                }
                return;
            }

            if (!UnifiedMoveModeFirst10Present[slot] ||
                !ReferenceEquals(UnifiedSnapshotFallbackSlots[slot], entity) ||
                !runtimeSlots.TryGetCurrentHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != UnifiedMoveModeFirst10Generation[slot] ||
                entity.Runtime.HP != UnifiedMoveModeFirst10Hp[slot])
            {
                UnifiedMoveModeFirst10Valid = false;
                return;
            }

            bool eligible = entity.GetCurrentDataObjectTypeForSimulation() ==
                                (int)LF2ObjectType.Character &&
                            entity.Runtime.HP > 0;
            if (eligible != UnifiedMoveModeFirst10Eligible[slot] ||
                eligible &&
                (entity.Runtime.XInt != UnifiedMoveModeFirst10X[slot] ||
                 entity.Runtime.ZInt != UnifiedMoveModeFirst10Z[slot]))
            {
                UnifiedMoveModeFirst10Valid = false;
            }
        }

        private void PrepareUnifiedShadowPassCore(
            SimulationWorld world,
            BattleAiInputDetailDiagnostics diagnostics,
            ref AiUnifiedSnapshotExceptionStage stage)
        {
            UnifiedShadowBuildCount++;
            UnifiedSnapshotPassAvailable = false;
            UnifiedSnapshotPassFailureRecorded = false;
            UnifiedSnapshotProductsComparedThisPass = false;
            UnifiedSnapshotPassEpoch = runtimeSlots.OccupancyEpoch;
            int capacity = runtimeSlots.LogicalCapacity;
            if (capacity <= 0)
            {
                InvalidateUnifiedShadowPass();
                return;
            }

            EnsureUnifiedShadowCapacity(capacity);
            AiSoASensingRows rows = UnifiedSnapshotRows;
            rows.Reset(UnifiedSnapshotPassEpoch);
            Array.Clear(UnifiedSnapshotSoASensingBoundaryFlags, 0, capacity);
            Array.Clear(UnifiedSnapshotDecisionBoundaryFlags, 0, capacity);
            Array.Clear(UnifiedSnapshotFallbackSlots, 0, capacity);
            ResetUnifiedMoveModeFirst10Snapshot();

            bool captureAvailable = true;
            int visitedSlots = 0;
            stage = AiUnifiedSnapshotExceptionStage.Capture;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
            diagnostics?.BeginPhase(
                BattleAiInputDetailPhase.SnapshotUnifiedDuplicateCapture);
            try
            {
                for (int slot = 0; slot < capacity; slot++)
                {
                    visitedSlots++;
                    UnifiedShadowSlotVisitCount++;
                    if (!runtimeSlots.IsAddressable(slot))
                    {
                        captureAvailable = false;
                        break;
                    }
                    RuntimeSlotTable.ReadOnlySlotView view =
                        runtimeSlots.GetReadOnlyView(slot);
                    if (view.RuntimeSlot != slot)
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
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;
                    UnifiedSnapshotFallbackSlots[slot] = entity;
                    if (slot < UnifiedMoveModeFirst10Present.Length)
                    {
                        CaptureUnifiedMoveModeFirst10Candidate(
                            slot,
                            entity,
                            view.Generation);
                    }
                    if (!SimulationAiSensingModule.TryCaptureRow(
                            rows,
                            entity,
                            slot,
                            view.Generation,
                            true))
                    {
                        captureAvailable = false;
                        break;
                    }

                    int sensingFlags =
                        SimulationAiSensingModule.CaptureBoundaryFlags(runtime);
                    int decisionFlags = CaptureBoundaryFlags(runtime);
                    UnifiedSnapshotSoASensingBoundaryFlags[slot] = sensingFlags;
                    UnifiedSnapshotDecisionBoundaryFlags[slot] = decisionFlags;
                    if (sensingFlags != decisionFlags)
                        UnifiedShadowDistinctBoundaryEncodingRowCount++;
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
                runtimeSlots.LogicalCapacity != capacity ||
                runtimeSlots.OccupancyEpoch != UnifiedSnapshotPassEpoch)
            {
                InvalidateUnifiedShadowPass();
                return;
            }
            UnifiedMoveModeFirst10Valid = true;

            stage = AiUnifiedSnapshotExceptionStage.BuildIndexes;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
            diagnostics?.BeginPhase(
                BattleAiInputDetailPhase.SnapshotUnifiedDuplicateIndexBuild);
            try
            {
                rows.SpecialIndexReady = true;
                SimulationAiSensingModule.BuildRoleIndexes(rows);
                SimulationAiSensingModule.BuildTeamSummaries(rows);
            }
            finally
            {
                diagnostics?.EndPhase(
                    BattleAiInputDetailPhase.SnapshotUnifiedDuplicateIndexBuild);
            }
            stage = AiUnifiedSnapshotExceptionStage.Validate;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
            if (!AiSensingKernel.ValidateIndexedContract(rows))
            {
                InvalidateUnifiedShadowPass();
                return;
            }

            UnifiedSnapshotMutationWitnessOrdinal = 0;
            UnifiedSnapshotRoleIndexVersion = 1;
            UnifiedSnapshotTeamSummaryVersion = 1;
            UnifiedSnapshotMutationWitness = default;
            UnifiedSnapshotPassAvailable = true;
            stage = AiUnifiedSnapshotExceptionStage.InitialSensingCompare;
                ThrowUnifiedSnapshotExceptionForSelfCheck(stage);
            world.CompareAiUnifiedSnapshotShadowForDecisionModule(
                AiUnifiedSnapshotConsumer.SoASensing,
                true,
                -1);
        }

        internal void EndUnifiedShadowPass()
        {
            UnifiedSnapshotPassAvailable = false;
            UnifiedSnapshotPassFailureRecorded = false;
            UnifiedSnapshotProductsComparedThisPass = false;
            UnifiedSnapshotRefreshComparisonActive = false;
            UnifiedSnapshotPassEpoch = 0;
        }

        internal void InvalidateUnifiedShadowPass()
        {
            if (!UnifiedSnapshotPassFailureRecorded)
            {
                UnifiedShadowUnavailableCount++;
                UnifiedSnapshotPassFailureRecorded = true;
            }
            UnifiedSnapshotPassAvailable = false;
        }

        internal void RecordUnifiedShadowException(
            AiUnifiedSnapshotExceptionStage stage,
            Exception exception)
        {
            if (UnifiedShadowFirstExceptionStage !=
                AiUnifiedSnapshotExceptionStage.None)
            {
                return;
            }
            UnifiedShadowFirstExceptionStage = stage;
            UnifiedSnapshotFirstExceptionType = exception?.GetType();
        }

        private void CaptureUnifiedMoveModeFirst10Candidate(
            int slot,
            LF2Entity entity,
            uint generation)
        {
            UnifiedMoveModeFirst10Present[slot] = true;
            UnifiedMoveModeFirst10Generation[slot] = generation;
            UnifiedMoveModeFirst10Hp[slot] = entity.Runtime?.HP ?? 0;
            bool eligible = entity.GetCurrentDataObjectTypeForSimulation() ==
                                (int)LF2ObjectType.Character &&
                            UnifiedMoveModeFirst10Hp[slot] > 0;
            UnifiedMoveModeFirst10Eligible[slot] = eligible;
            if (!eligible)
                return;

            int x = entity.Runtime.XInt;
            int z = entity.Runtime.ZInt;
            UnifiedMoveModeFirst10X[slot] = x;
            UnifiedMoveModeFirst10Z[slot] = z;
            if (x <= -1)
                return;

            if (UnifiedMoveModeTopSlot < 0 || x > UnifiedMoveModeTopX)
            {
                UnifiedMoveModeSecondSlot = UnifiedMoveModeTopSlot;
                UnifiedMoveModeSecondX = UnifiedMoveModeTopX;
                UnifiedMoveModeSecondZ = UnifiedMoveModeTopZ;
                UnifiedMoveModeTopSlot = slot;
                UnifiedMoveModeTopX = x;
                UnifiedMoveModeTopZ = z;
                return;
            }
            if (UnifiedMoveModeSecondSlot < 0 || x > UnifiedMoveModeSecondX)
            {
                UnifiedMoveModeSecondSlot = slot;
                UnifiedMoveModeSecondX = x;
                UnifiedMoveModeSecondZ = z;
            }
        }

        internal void RebuildUnifiedMoveModeFirst10Product(
            AiUnifiedSnapshotExecutionState candidate)
        {
            Array.Clear(candidate.MoveModeFirst10Present, 0, 10);
            Array.Clear(candidate.MoveModeFirst10Eligible, 0, 10);
            Array.Clear(candidate.MoveModeFirst10Generation, 0, 10);
            Array.Clear(candidate.MoveModeFirst10Hp, 0, 10);
            Array.Clear(candidate.MoveModeFirst10X, 0, 10);
            Array.Clear(candidate.MoveModeFirst10Z, 0, 10);
            candidate.MoveModeTopSlot = -1;
            candidate.MoveModeTopX = -1;
            candidate.MoveModeTopZ = 0;
            candidate.MoveModeSecondSlot = -1;
            candidate.MoveModeSecondX = -1;
            candidate.MoveModeSecondZ = 0;

            AiSoASensingRows rows = candidate.Rows;
            int limit = rows.Capacity < 10 ? rows.Capacity : 10;
            for (int slot = 0; slot < limit; slot++)
            {
                if (!rows.Included[slot])
                    continue;
                CaptureUnifiedMoveModeScratchCandidate(
                    candidate,
                    rows,
                    slot,
                    rows.Generation[slot]);
            }
            candidate.MoveModeFirst10Valid = true;
        }

        internal static void CaptureUnifiedMoveModeScratchCandidate(
            AiUnifiedSnapshotExecutionState candidate,
            AiSoASensingRows rows,
            int slot,
            uint generation)
        {
            candidate.MoveModeFirst10Present[slot] = true;
            candidate.MoveModeFirst10Generation[slot] = generation;
            int hp = rows.Hp[slot];
            candidate.MoveModeFirst10Hp[slot] = hp;
            bool eligible = rows.DataObjectType[slot] ==
                                (int)LF2ObjectType.Character &&
                            hp > 0;
            candidate.MoveModeFirst10Eligible[slot] = eligible;
            if (!eligible)
                return;

            int x = rows.X[slot];
            int z = rows.Z[slot];
            candidate.MoveModeFirst10X[slot] = x;
            candidate.MoveModeFirst10Z[slot] = z;
            if (x <= -1)
                return;
            if (candidate.MoveModeTopSlot < 0 || x > candidate.MoveModeTopX)
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

        internal static int PackRoleFlags(bool ground, bool air)
        {
            return (ground ? 1 : 0) | (air ? 1 << 1 : 0);
        }

        internal bool ValidateExecutionPreCommit(
            AiUnifiedSnapshotExecutionState candidate,
            int capacity,
            ulong epoch)
        {
            return candidate != null &&
                   candidate.ExpectedCapacity == capacity &&
                   candidate.Capacity == capacity &&
                   candidate.Epoch == epoch &&
                   candidate.Rows != null &&
                   candidate.Rows.CapturedOccupancyEpoch == epoch &&
                   candidate.FallbackSlots != null &&
                   candidate.FallbackSlots.Length == capacity &&
                   candidate.SoASensingBoundaryFlags != null &&
                   candidate.SoASensingBoundaryFlags.Length == capacity &&
                   candidate.DecisionBoundaryFlags != null &&
                   candidate.DecisionBoundaryFlags.Length == capacity &&
                   runtimeSlots.LogicalCapacity == capacity &&
                   runtimeSlots.OccupancyEpoch == epoch &&
                   candidate.MoveModeFirst10Valid &&
                   AiSensingKernel.AreIndexesReady(candidate.Rows);
        }

        internal static AiDecisionAvailability ResetRejectedSnapshot(
            AiDecisionSnapshot snapshot,
            ulong epoch,
            AiDecisionAvailability availability)
        {
            snapshot.ResetOwned(epoch);
            return availability;
        }

        internal static bool MatchUnifiedSnapshotValue(
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
            return SetUnifiedSnapshotMismatch(
                consumer,
                kind,
                field,
                slot,
                expected,
                actual,
                ref mismatch);
        }

        internal static bool SetUnifiedSnapshotMismatch(
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

        internal AiDecisionAvailability CaptureShadowSnapshot(
            SimulationWorld world,
            BattleCharacterInputWriter characterInputWriter,
            BattleAiExecutionProfile executionProfile,
            LF2Entity self,
            AiDecisionSnapshot snapshot,
            AiDecisionWorldState worldState,
            uint rngState,
            ulong rngCalls)
        {
            if (self?.Runtime == null)
                return AiDecisionAvailability.SelfNotIncluded;

            ulong epoch = runtimeSlots.OccupancyEpoch;
            snapshot.Reset(epoch);
            int capacity = snapshot.Rows.Capacity;
            if (capacity != runtimeSlots.LogicalCapacity)
                return AiDecisionAvailability.SnapshotMissing;

            AiSensingSnapshot rows = snapshot.Rows;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!runtimeSlots.IsAddressable(slot))
                    return AiDecisionAvailability.SnapshotMissing;

                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                if (view.RuntimeSlot != slot)
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
                {
                    return AiDecisionAvailability.GenerationMismatch;
                }
                if (!world.IsActiveForCurrentPassInternal(entity))
                    continue;

                rows.Included[slot] = true;
                rows.Generation[slot] = view.Generation;
                rows.Identity[slot] = runtime.StableId;
                rows.ObjectId[slot] = entity.ObjectId;
                rows.DataObjectType[slot] =
                    entity.GetCurrentDataObjectTypeForSimulation();
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
                rows.HitJ[slot] =
                    SimulationAiSensingModule.CaptureCurrentFrameHitJ(
                        entity,
                        runtime.Frame);
                rows.LinkState[slot] = runtime.LinkState;
                rows.KillCount[slot] = runtime.KillCount;
                rows.CachedTargetSlot[slot] = runtime.Unk360;
                rows.CoordinateTargetX[slot] = runtime.Unk3FC;
                rows.Vx[slot] = runtime.Vx;
                rows.Facing[slot] = runtime.Dir == "left" ? 1 : 0;
                rows.TargetSlot[slot] = runtime.TargetSlotIndex;
                rows.HitStop[slot] = runtime.HitStop;
                rows.BoundaryFlags[slot] = CaptureBoundaryFlags(runtime);
                rows.InputHistoryGate[slot] =
                    runtime.InputHistory != null &&
                    runtime.InputHistory.Length == 6 &&
                    runtime.InputHistory[0] != 0;
            }

            if (runtimeSlots.OccupancyEpoch != epoch ||
                runtimeSlots.LogicalCapacity != capacity)
            {
                return AiDecisionAvailability.EpochMismatch;
            }
            int selfSlot = self.Runtime.SlotIndex;
            if (selfSlot < 0 || selfSlot >= capacity)
                return AiDecisionAvailability.SelfSlotInvalid;
            if (!rows.Included[selfSlot])
                return AiDecisionAvailability.SelfNotIncluded;
            if (!runtimeSlots.TryGetCurrentHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle selfHandle))
            {
                return AiDecisionAvailability.GenerationMismatch;
            }
            if (rows.Generation[selfSlot] != selfHandle.Generation)
                return AiDecisionAvailability.GenerationMismatch;
            if (rows.Identity[selfSlot] != self.Runtime.StableId)
                return AiDecisionAvailability.StableIdMismatch;

            bool inputCaptured = executionProfile ==
                    BattleAiExecutionProfile.DataOrientedCanonical
                ? characterInputWriter.TryCaptureCanonicalState(
                    self.Runtime,
                    out snapshot.Input)
                : TryCaptureInputState(self.Runtime, out snapshot.Input);
            if (!inputCaptured)
                return AiDecisionAvailability.SnapshotMissing;

            snapshot.SelfSlot = selfSlot;
            snapshot.SelfGeneration = selfHandle.Generation;
            snapshot.SelfStableId = self.Runtime.StableId;
            snapshot.OccupancyEpoch = epoch;
            snapshot.World = worldState;
            snapshot.RngState = rngState;
            snapshot.RngCalls = rngCalls;
            return AiDecisionAvailability.Available;
        }

        internal AiDecisionAvailability CaptureSharedRows(
            SimulationWorld world,
            int capacity,
            ulong occupancyEpoch)
        {
            AiSoASensingRows rows = SharedRows;
            for (int slot = 0; slot < capacity; slot++)
            {
                if (!runtimeSlots.IsAddressable(slot))
                    return AiDecisionAvailability.SnapshotMissing;

                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                if (view.RuntimeSlot != slot)
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
                {
                    return AiDecisionAvailability.GenerationMismatch;
                }
                if (!world.IsActiveForCurrentPassInternal(entity))
                    continue;
                if (!SimulationAiSensingModule.TryCaptureRow(
                        rows,
                        entity,
                        slot,
                        view.Generation,
                        true))
                {
                    return AiDecisionAvailability.SnapshotMissing;
                }
                rows.BoundaryFlags[slot] = CaptureBoundaryFlags(runtime);
            }

            return runtimeSlots.OccupancyEpoch == occupancyEpoch &&
                   runtimeSlots.LogicalCapacity == capacity
                ? AiDecisionAvailability.Available
                : AiDecisionAvailability.EpochMismatch;
        }

        internal AiDecisionAvailability ValidateSharedPassPreflight(
            SimulationWorld world,
            int capacity,
            ulong occupancyEpoch)
        {
            AiSoASensingRows rows = SharedRows;
            if (rows == null ||
                rows.Capacity != capacity ||
                runtimeSlots.LogicalCapacity != capacity ||
                runtimeSlots.OccupancyEpoch != occupancyEpoch ||
                rows.CapturedOccupancyEpoch != occupancyEpoch)
            {
                return AiDecisionAvailability.EpochMismatch;
            }

            for (int slot = 0; slot < capacity; slot++)
            {
                if (!runtimeSlots.IsAddressable(slot))
                    return AiDecisionAvailability.SnapshotMissing;

                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                if (view.RuntimeSlot != slot)
                    return AiDecisionAvailability.SnapshotMissing;
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

                bool shouldBeIncluded = world.IsActiveForCurrentPassInternal(entity);
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

            return AiSensingKernel.ValidateIndexedContract(rows)
                ? AiDecisionAvailability.Available
                : AiDecisionAvailability.IndexesNotReady;
        }

        internal AiDecisionAvailability CaptureSharedOwnedSnapshot(
            BattleCharacterInputWriter inputWriter,
            BattleAiExecutionProfile executionProfile,
            AiDecisionOwnedInputMode ownedInputMode,
            LF2Entity self,
            AiDecisionSnapshot snapshot,
            in AiDecisionWorldState worldState,
            uint rngState,
            ulong rngCalls)
        {
            AiUnifiedSnapshotExecutionState unifiedState =
                UnifiedSnapshotPublishedState;
            if (UnifiedSnapshotExecutionCommittedThisPass &&
                unifiedState != null &&
                snapshot != null &&
                ReferenceEquals(snapshot, unifiedState.IndexedSnapshot) &&
                ReferenceEquals(snapshot.Rows, unifiedState.Rows))
            {
                NTSDEntityRuntime runtime = self?.Runtime;
                int slot = runtime?.SlotIndex ?? -1;
                AiSoASensingRows unifiedRows = unifiedState.Rows;
                if ((uint)slot >= (uint)unifiedRows.Capacity ||
                    unifiedState.FallbackSlots[slot] != self ||
                    unifiedRows.Generation[slot] == 0)
                {
                    return ResetRejectedSnapshot(
                        snapshot,
                        unifiedState.Epoch,
                        AiDecisionAvailability.GenerationMismatch);
                }

                bool directCanonicalInput =
                    executionProfile == BattleAiExecutionProfile.DataOrientedCanonical &&
                    ownedInputMode == AiDecisionOwnedInputMode.CanonicalStoreDirect;
                if (directCanonicalInput
                        ? !inputWriter.CanEvaluateCanonicalDecision(runtime)
                        : executionProfile ==
                              BattleAiExecutionProfile.DataOrientedCanonical
                            ? !inputWriter.TryCaptureCanonicalState(
                                runtime,
                                out snapshot.Input)
                            : !TryCaptureInputState(runtime, out snapshot.Input))
                {
                    return ResetRejectedSnapshot(
                        snapshot,
                        unifiedState.Epoch,
                        AiDecisionAvailability.SnapshotMissing);
                }

                PopulateOwnedSnapshot(
                    snapshot,
                    slot,
                    unifiedRows.Generation[slot],
                    unifiedRows.Identity[slot],
                    unifiedState.Epoch,
                    worldState,
                    rngState,
                    rngCalls);
                return AiDecisionAvailability.Available;
            }

            if (!SharedPassAvailable ||
                self?.Runtime == null ||
                snapshot == null ||
                !ReferenceEquals(snapshot.Rows, SharedRows))
            {
                return AiDecisionAvailability.SnapshotMissing;
            }

            ulong epoch = SharedPassEpoch;
            AiSoASensingRows rows = SharedRows;
            if (runtimeSlots.OccupancyEpoch != epoch ||
                rows.CapturedOccupancyEpoch != epoch)
            {
                InvalidateSharedPass(AiDecisionAvailability.EpochMismatch);
                return AiDecisionAvailability.EpochMismatch;
            }

            int selfSlot = self.Runtime.SlotIndex;
            if (selfSlot < 0 || selfSlot >= rows.Capacity)
            {
                return ResetRejectedSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.SelfSlotInvalid);
            }
            if (!rows.Included[selfSlot])
            {
                return ResetRejectedSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.SelfNotIncluded);
            }

            uint selfGeneration;
            bool unifiedFastCapture =
                UnifiedSnapshotExecutionCommittedThisPass &&
                UnifiedSnapshotPublishedState != null &&
                ReferenceEquals(UnifiedSnapshotPublishedState.Rows, rows);
            if (unifiedFastCapture)
            {
                selfGeneration = rows.Generation[selfSlot];
                if (selfGeneration == 0 ||
                    UnifiedSnapshotPublishedState.FallbackSlots[selfSlot] != self)
                {
                    return ResetRejectedSnapshot(
                        snapshot,
                        epoch,
                        AiDecisionAvailability.GenerationMismatch);
                }
            }
            else if (!runtimeSlots.TryGetCurrentHandle(
                         selfSlot,
                         self,
                         out RuntimeEntityHandle selfHandle) ||
                     rows.Generation[selfSlot] != selfHandle.Generation)
            {
                return ResetRejectedSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.GenerationMismatch);
            }
            else
            {
                selfGeneration = selfHandle.Generation;
            }

            if (rows.Identity[selfSlot] != self.Runtime.StableId)
            {
                return ResetRejectedSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.StableIdMismatch);
            }
            bool inputCaptured = executionProfile ==
                    BattleAiExecutionProfile.DataOrientedCanonical
                ? inputWriter.TryCaptureCanonicalState(
                    self.Runtime,
                    out snapshot.Input)
                : TryCaptureInputState(self.Runtime, out snapshot.Input);
            if (!inputCaptured)
            {
                return ResetRejectedSnapshot(
                    snapshot,
                    epoch,
                    AiDecisionAvailability.SnapshotMissing);
            }

            PopulateOwnedSnapshot(
                snapshot,
                selfSlot,
                selfGeneration,
                self.Runtime.StableId,
                epoch,
                worldState,
                rngState,
                rngCalls);
            return AiDecisionAvailability.Available;
        }

        internal void RefreshSharedRowAfterCharacterInput(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (!SharedPassAvailable)
                return;

            try
            {
                ThrowShadowExceptionForSelfCheck(
                    AiDecisionShadowExceptionStage.SharedRefresh);
                if (runtimeSlots.OccupancyEpoch != SharedPassEpoch ||
                    entity?.Runtime == null)
                {
                    InvalidateSharedPass(AiDecisionAvailability.EpochMismatch);
                    return;
                }

                int slot = entity.Runtime.SlotIndex;
                AiSoASensingRows rows = SharedRows;
                if (slot < 0 ||
                    slot >= rows.Capacity ||
                    !rows.Included[slot] ||
                    !runtimeSlots.TryGetCurrentHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                    handle.Generation != rows.Generation[slot])
                {
                    InvalidateSharedPass(AiDecisionAvailability.GenerationMismatch);
                    return;
                }
                if (entity.Runtime.StableId != rows.Identity[slot])
                {
                    InvalidateSharedPass(AiDecisionAvailability.StableIdMismatch);
                    return;
                }

                int previousX = rows.X[slot];
                int previousTeam = rows.Team[slot];
                int previousHp = rows.Hp[slot];
                int previousObjectId = rows.ObjectId[slot];
                bool previousSpecialMember = rows.SpecialScanMember[slot];
                bool wasGroundRole =
                    SimulationAiSensingModule.IsGroundRoleMember(rows, slot);
                bool wasAirRole =
                    SimulationAiSensingModule.IsAirRoleMember(rows, slot);
                bool wasLivingCharacter =
                    SimulationAiSensingModule.IsLivingCharacterRow(rows, slot);
                if (!SimulationAiSensingModule.TryCaptureRow(
                        rows,
                        entity,
                        slot,
                        handle.Generation,
                        false,
                        true))
                {
                    InvalidateSharedPass(AiDecisionAvailability.SnapshotMissing);
                    return;
                }
                rows.BoundaryFlags[slot] = CaptureBoundaryFlags(entity.Runtime);
                bool currentSpecialMember =
                    slot >= 20 &&
                    SimulationAiInputModule.IsSpecialScanObjectId(rows.ObjectId[slot]);
                if (previousObjectId != rows.ObjectId[slot] ||
                    previousSpecialMember != currentSpecialMember)
                {
                    InvalidateSharedPass(AiDecisionAvailability.SnapshotMissing);
                    return;
                }

                SharedRefreshCount++;
                bool isGroundRole =
                    SimulationAiSensingModule.IsGroundRoleMember(rows, slot);
                bool isAirRole =
                    SimulationAiSensingModule.IsAirRoleMember(rows, slot);
                bool roleRebuilt = previousX != rows.X[slot] ||
                                   previousTeam != rows.Team[slot] ||
                                   wasGroundRole != isGroundRole ||
                                   wasAirRole != isAirRole;
                if (roleRebuilt)
                    SimulationAiSensingModule.BuildRoleIndexes(rows);

                bool isLivingCharacter =
                    SimulationAiSensingModule.IsLivingCharacterRow(rows, slot);
                bool teamRebuilt = wasLivingCharacter != isLivingCharacter ||
                                   previousTeam != rows.Team[slot] ||
                                   previousHp != rows.Hp[slot];
                if (teamRebuilt)
                    SimulationAiSensingModule.BuildTeamSummaries(rows);

                RecordProductionMutationWitness(
                    AiUnifiedSnapshotConsumer.IndexedDecision,
                    SharedPassEpoch,
                    slot,
                    handle.Generation,
                    entity.Runtime.StableId,
                    roleRebuilt,
                    teamRebuilt,
                    previousX,
                    rows.X[slot],
                    previousTeam,
                    rows.Team[slot],
                    PackRoleFlags(wasGroundRole, wasAirRole),
                    PackRoleFlags(isGroundRole, isAirRole),
                    wasLivingCharacter,
                    isLivingCharacter,
                    previousHp,
                    rows.Hp[slot]);
            }
            catch (Exception exception)
            {
                RecordShadowException(
                    AiDecisionShadowExceptionStage.SharedRefresh,
                    exception);
                InvalidateSharedPass(AiDecisionAvailability.SnapshotMissing);
            }
        }

        internal bool TryPrepareIndexedCanonical(
            SimulationWorld world,
            BattleCharacterInputWriter inputWriter,
            BattleAiInputWriter aiInputWriter,
            BattleAiExecutionProfile executionProfile,
            AiDecisionOwnedInputMode ownedInputMode,
            LF2Entity self,
            BattleAiInputDetailDiagnostics diagnostics,
            in AiDecisionWorldState worldState,
            uint rngState,
            ulong rngCalls,
            bool hasRuntimeFlow)
        {
            IndexedCanonicalEligibleCount++;
            if (!SharedPassAvailable || IndexedSnapshot == null || SharedSnapshot == null)
            {
                RecordIndexedCanonicalFallback(
                    world,
                    SharedPassUnavailableReason == AiDecisionAvailability.None
                        ? AiDecisionAvailability.SnapshotMissing
                        : SharedPassUnavailableReason);
                return false;
            }

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalCapture);
            AiDecisionAvailability captureAvailability;
            try
            {
                captureAvailability = CaptureSharedOwnedSnapshot(
                    inputWriter,
                    executionProfile,
                    ownedInputMode,
                    self,
                    IndexedSnapshot,
                    worldState,
                    rngState,
                    rngCalls);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalCapture);
            }
            if (captureAvailability != AiDecisionAvailability.Available)
            {
                RecordIndexedCanonicalFallback(world, captureAvailability);
                return false;
            }

            AiDecisionWitness indexedWitness = default;
            bool indexedAvailable;
            long ordinal = IndexedCanonicalEligibleCount - 1L;
            int sampleInterval = IndexedCanonicalFullOracleSampleInterval;
            bool captureOracleTrace = sampleInterval > 0 && ordinal % sampleInterval == 0;
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalKernel);
            try
            {
                if (ownedInputMode == AiDecisionOwnedInputMode.CanonicalStoreDirect &&
                    executionProfile == BattleAiExecutionProfile.DataOrientedCanonical)
                {
                    indexedAvailable = inputWriter.TryEvaluateCanonicalDecision(
                        self.Runtime,
                        IndexedSnapshot,
                        AiDecisionEvaluationPolicy.Indexed,
                        captureOracleTrace,
                        diagnostics,
                        ref indexedWitness);
                }
                else
                {
                    indexedAvailable = AiDecisionKernel.TryEvaluate(
                        IndexedSnapshot,
                        AiDecisionEvaluationPolicy.Indexed,
                        captureOracleTrace,
                        diagnostics,
                        ref indexedWitness);
                }
            }
            catch (Exception exception)
            {
                RecordShadowException(AiDecisionShadowExceptionStage.KernelEvaluate, exception);
                InvalidateSharedPass(AiDecisionAvailability.SnapshotMissing);
                RecordIndexedCanonicalFallback(
                    world,
                    AiDecisionAvailability.SnapshotMissing);
                return false;
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalKernel);
            }
            if (!indexedAvailable)
            {
                RecordIndexedCanonicalFallback(world, indexedWitness.Availability);
                return false;
            }

            if (captureOracleTrace)
            {
                IndexedCanonicalFullOracleSampleCount++;
                SharedSnapshot.CopyOwnedFrom(IndexedSnapshot);
                if (ownedInputMode == AiDecisionOwnedInputMode.CanonicalStoreDirect &&
                    !inputWriter.TryCaptureCanonicalState(self.Runtime, out SharedSnapshot.Input))
                {
                    RecordIndexedCanonicalFallback(
                        world,
                        AiDecisionAvailability.SnapshotMissing);
                    return false;
                }

                AiDecisionWitness fullWitness = default;
                bool fullAvailable = AiDecisionKernel.TryEvaluate(
                    SharedSnapshot,
                    AiDecisionEvaluationPolicy.FullScan,
                    ref fullWitness);
                AiDecisionIndexedMismatchReason mismatch = CompareIndexedWitnesses(
                    SharedSnapshot,
                    fullWitness,
                    fullAvailable,
                    IndexedSnapshot,
                    indexedWitness,
                    indexedAvailable);
                if (mismatch != AiDecisionIndexedMismatchReason.None)
                {
                    IndexedCanonicalFullOracleMismatchCount++;
                    if (IndexedCanonicalFirstOracleMismatchReason ==
                        AiDecisionIndexedMismatchReason.None)
                    {
                        IndexedCanonicalFirstOracleMismatchReason = mismatch;
                    }
                    RecordIndexedCanonicalFallback(
                        world,
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
                commitAvailability = ValidateIndexedCanonicalCommit(
                    self,
                    IndexedSnapshot,
                    indexedWitness,
                    hasRuntimeFlow);
            }
            finally
            {
                diagnostics?.EndPhase(
                    BattleAiInputDetailPhase.IndexedCanonicalCommitValidation);
            }
            if (commitAvailability != AiDecisionAvailability.Available)
            {
                RecordIndexedCanonicalFallback(world, commitAvailability);
                return false;
            }

            diagnostics?.BeginPhase(BattleAiInputDetailPhase.IndexedCanonicalCommitApply);
            try
            {
                aiInputWriter.CommitIndexedCanonicalDecision(self.Runtime, indexedWitness);
            }
            finally
            {
                diagnostics?.EndPhase(BattleAiInputDetailPhase.IndexedCanonicalCommitApply);
            }
            IndexedCanonicalCommittedCount++;
            return true;
        }

        private AiDecisionAvailability ValidateIndexedCanonicalCommit(
            LF2Entity self,
            AiDecisionSnapshot snapshot,
            in AiDecisionWitness witness,
            bool hasRuntimeFlow)
        {
#if UNITY_INCLUDE_TESTS
            if (IndexedCanonicalPreCommitFailureForSelfCheck !=
                AiDecisionAvailability.None)
            {
                AiDecisionAvailability failure =
                    IndexedCanonicalPreCommitFailureForSelfCheck;
                IndexedCanonicalPreCommitFailureForSelfCheck =
                    AiDecisionAvailability.None;
                return failure;
            }
#endif
            if (UnifiedSnapshotExecutionCommittedThisPass &&
                UnifiedSnapshotPublishedState != null &&
                ReferenceEquals(snapshot, UnifiedSnapshotPublishedState.IndexedSnapshot) &&
                ReferenceEquals(snapshot.Rows, UnifiedSnapshotPublishedState.Rows) &&
                self?.Runtime != null &&
                hasRuntimeFlow)
            {
                return AiDecisionAvailability.Available;
            }

            AiSoASensingRows rows = SharedRows;
            if (!SharedPassAvailable || rows == null ||
                !ReferenceEquals(snapshot.Rows, rows) ||
                !AiSensingKernel.AreIndexesReady(rows) ||
                runtimeSlots.OccupancyEpoch != snapshot.OccupancyEpoch ||
                rows.CapturedOccupancyEpoch != snapshot.OccupancyEpoch)
            {
                return AiDecisionAvailability.EpochMismatch;
            }

            int selfSlot = snapshot.SelfSlot;
            bool unifiedFastValidation =
                UnifiedSnapshotExecutionCommittedThisPass &&
                UnifiedSnapshotPublishedState != null &&
                ReferenceEquals(UnifiedSnapshotPublishedState.Rows, rows);
            if (self?.Runtime == null ||
                selfSlot < 0 ||
                selfSlot >= rows.Capacity ||
                self.Runtime.SlotIndex != selfSlot ||
                rows.Generation[selfSlot] != snapshot.SelfGeneration)
            {
                return AiDecisionAvailability.GenerationMismatch;
            }
            if (unifiedFastValidation)
            {
                if (UnifiedSnapshotPublishedState.FallbackSlots[selfSlot] != self)
                    return AiDecisionAvailability.GenerationMismatch;
            }
            else if (!runtimeSlots.TryGetCurrentHandle(
                         selfSlot,
                         self,
                         out RuntimeEntityHandle selfHandle) ||
                     selfHandle.Generation != snapshot.SelfGeneration)
            {
                return AiDecisionAvailability.GenerationMismatch;
            }
            if (self.Runtime.StableId != snapshot.SelfStableId ||
                rows.Identity[selfSlot] != snapshot.SelfStableId)
            {
                return AiDecisionAvailability.StableIdMismatch;
            }
            if (!hasRuntimeFlow)
                return AiDecisionAvailability.SnapshotMissing;

            int selectedSlot = witness.FinalSelectedSlot;
            if (selectedSlot >= 0 &&
                selectedSlot < rows.Capacity &&
                rows.Included[selectedSlot])
            {
                LF2Entity selected;
                if (unifiedFastValidation)
                {
                    selected = UnifiedSnapshotPublishedState.FallbackSlots[selectedSlot];
                }
                else
                {
                    RuntimeEntityHandle selectedHandle = new RuntimeEntityHandle(
                        selectedSlot,
                        rows.Generation[selectedSlot]);
                    if (!runtimeSlots.TryResolve(selectedHandle, out selected))
                        return AiDecisionAvailability.GenerationMismatch;
                }

                if (selected?.Runtime == null ||
                    selected.Runtime.StableId != rows.Identity[selectedSlot])
                {
                    return AiDecisionAvailability.GenerationMismatch;
                }
            }
            return AiDecisionAvailability.Available;
        }

        private void RecordIndexedCanonicalFallback(
            SimulationWorld world,
            AiDecisionAvailability reason)
        {
            if (UnifiedSnapshotExecutionCommittedThisPass ||
                UnifiedSnapshotExecutionConsumerStartedThisPass)
            {
                world.ThrowAiUnifiedSnapshotExecutionHardBreachForDecisionModule(
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare,
                    "IndexedCanonical attempted fallback after unified snapshot commit.");
            }
            IndexedCanonicalFallbackCount++;
            if (IndexedCanonicalFirstFallbackReason == AiDecisionAvailability.None)
            {
                IndexedCanonicalFirstFallbackReason =
                    reason == AiDecisionAvailability.None
                        ? AiDecisionAvailability.SnapshotMissing
                        : reason;
            }
        }

        private static void PopulateOwnedSnapshot(
            AiDecisionSnapshot snapshot,
            int selfSlot,
            uint selfGeneration,
            int selfStableId,
            ulong epoch,
            in AiDecisionWorldState worldState,
            uint rngState,
            ulong rngCalls)
        {
            snapshot.SelfSlot = selfSlot;
            snapshot.SelfGeneration = selfGeneration;
            snapshot.SelfStableId = selfStableId;
            snapshot.OccupancyEpoch = epoch;
            snapshot.World = worldState;
            snapshot.RngState = rngState;
            snapshot.RngCalls = rngCalls;
            snapshot.RngTraceCount = 0;
            snapshot.RngTraceOverflow = false;
        }

        internal void CompareUnifiedSnapshotShadow(
            AiUnifiedSnapshotConsumer consumer,
            bool fullComparison,
            int refreshSlot)
        {
            if (!UnifiedSnapshotPassAvailable)
                return;

            AiSoASensingRows productionRows;
            int[] unifiedBoundaryFlags;
            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    if (sensing.Mode != AiSensingMode.SoAShadowAiSensing &&
                        sensing.Mode != AiSensingMode.SoAAiSensing)
                    {
                        return;
                    }
                    if (!sensing.SnapshotValid || sensing.PassInvalidated)
                    {
                        InvalidateUnifiedShadowPass();
                        return;
                    }
                    productionRows = sensing.Rows;
                    unifiedBoundaryFlags = UnifiedSnapshotSoASensingBoundaryFlags;
                    break;
                case AiUnifiedSnapshotConsumer.IndexedDecision:
                    if (!SharedPassAvailable)
                    {
                        InvalidateUnifiedShadowPass();
                        return;
                    }
                    productionRows = SharedRows;
                    unifiedBoundaryFlags = UnifiedSnapshotDecisionBoundaryFlags;
                    break;
                default:
                    return;
            }

            int mutatedSlot = -1;
            int originalBoundaryFlags = 0;
#if UNITY_INCLUDE_TESTS
            if (UnifiedBoundaryMutationConsumerForSelfCheck == consumer &&
                UnifiedBoundaryMutationSlotForSelfCheck >= 0 &&
                UnifiedBoundaryMutationSlotForSelfCheck < productionRows.Capacity)
            {
                mutatedSlot = UnifiedBoundaryMutationSlotForSelfCheck;
                originalBoundaryFlags = productionRows.BoundaryFlags[mutatedSlot];
                productionRows.BoundaryFlags[mutatedSlot] ^=
                    UnifiedBoundaryMutationXorForSelfCheck;
                UnifiedBoundaryMutationConsumerForSelfCheck =
                    AiUnifiedSnapshotConsumer.None;
                UnifiedBoundaryMutationSlotForSelfCheck = -1;
                UnifiedBoundaryMutationXorForSelfCheck = 0;
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
                    UnifiedSnapshotRefreshComparisonActive = true;
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
                        UnifiedSnapshotRefreshComparisonActive = false;
                    }
                }
            }
            finally
            {
                if (mutatedSlot >= 0)
                    productionRows.BoundaryFlags[mutatedSlot] = originalBoundaryFlags;
            }

            if (consumer == AiUnifiedSnapshotConsumer.SoASensing)
                UnifiedShadowSensingComparedCount++;
            else
                UnifiedShadowDecisionComparedCount++;
            if (matches)
                return;

            UnifiedShadowMismatchCount++;
            if (UnifiedShadowFirstMismatch.Kind ==
                AiUnifiedSnapshotMismatchKind.None)
            {
                UnifiedShadowFirstMismatch = mismatch;
            }
        }



        private bool TryCompareAiUnifiedSnapshotRows(
            AiSoASensingRows production,
            int[] unifiedBoundaryFlags,
            AiUnifiedSnapshotConsumer consumer,
            out AiUnifiedSnapshotMismatch mismatch)
        {
            mismatch = default;
            AiSoASensingRows unified = UnifiedSnapshotRows;
            if (production == null || unified == null || unifiedBoundaryFlags == null)
            {
                return SetUnifiedSnapshotMismatch(
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Capacity,
                    AiUnifiedSnapshotField.None,
                    -1,
                    production?.Capacity ?? -1,
                    unified?.Capacity ?? -1,
                    ref mismatch);
            }
            if (!MatchUnifiedSnapshotValue(
                    unchecked((long)production.CapturedOccupancyEpoch),
                    unchecked((long)unified.CapturedOccupancyEpoch),
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Epoch,
                    AiUnifiedSnapshotField.None,
                    -1,
                    ref mismatch) ||
                !MatchUnifiedSnapshotValue(
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
                UnifiedShadowFullComparisonSlotVisitCount++;
                if (!MatchUnifiedSnapshotValue(
                        production.Included[slot] ? 1 : 0,
                        unified.Included[slot] ? 1 : 0,
                        consumer,
                        AiUnifiedSnapshotMismatchKind.Included,
                        AiUnifiedSnapshotField.None,
                        slot,
                        ref mismatch) ||
                    !MatchUnifiedSnapshotValue(
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

                if (!MatchUnifiedSnapshotValue(production.Generation[slot], unified.Generation[slot], consumer, AiUnifiedSnapshotMismatchKind.Generation, AiUnifiedSnapshotField.None, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Identity[slot], unified.Identity[slot], consumer, AiUnifiedSnapshotMismatchKind.Identity, AiUnifiedSnapshotField.None, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.InputHistoryGate[slot] ? 1 : 0, unified.InputHistoryGate[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.InputHistoryGate, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.ObjectId[slot], unified.ObjectId[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.ObjectId, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.DataObjectType[slot], unified.DataObjectType[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.DataObjectType, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.X[slot], unified.X[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.X, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Y[slot], unified.Y[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Y, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Z[slot], unified.Z[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Z, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Hp[slot], unified.Hp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Hp3[slot], unified.Hp3[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp3, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.HpMax[slot], unified.HpMax[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HpMax, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Pp[slot], unified.Pp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Pp, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Team[slot], unified.Team[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Team, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.State[slot], unified.State[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.State, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Frame[slot], unified.Frame[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Frame, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.HitJ[slot], unified.HitJ[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HitJ, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.LinkState[slot], unified.LinkState[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.LinkState, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.KillCount[slot], unified.KillCount[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.KillCount, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.CachedTargetSlot[slot], unified.CachedTargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CachedTargetSlot, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.CoordinateTargetX[slot], unified.CoordinateTargetX[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CoordinateTargetX, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(BitConverter.DoubleToInt64Bits(production.Vx[slot]), BitConverter.DoubleToInt64Bits(unified.Vx[slot]), consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.VxBits, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.Facing[slot], unified.Facing[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Facing, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.TargetSlot[slot], unified.TargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.TargetSlot, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.HitStop[slot], unified.HitStop[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HitStop, slot, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.BoundaryFlags[slot], unifiedBoundaryFlags[slot], consumer, AiUnifiedSnapshotMismatchKind.BoundaryFlags, AiUnifiedSnapshotField.None, slot, ref mismatch))
                {
                    return false;
                }
            }

            if (!TryCompareUnifiedSnapshotIndexes(
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
            if (UnifiedSnapshotProductsComparedThisPass)
                return true;

            bool productsMatch = TryCompareUnifiedSnapshotCandidateProducts(
                consumer,
                true,
                -1,
                ref mismatch);
            if (productsMatch)
                UnifiedSnapshotProductsComparedThisPass = true;
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
            AiSoASensingRows unified = UnifiedSnapshotRows;
            if (production == null || unified == null || unifiedBoundaryFlags == null)
            {
                return SetUnifiedSnapshotMismatch(
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Capacity,
                    AiUnifiedSnapshotField.None,
                    -1,
                    production?.Capacity ?? -1,
                    unified?.Capacity ?? -1,
                    ref mismatch);
            }
            if (!MatchUnifiedSnapshotValue(
                    unchecked((long)production.CapturedOccupancyEpoch),
                    unchecked((long)unified.CapturedOccupancyEpoch),
                    consumer,
                    AiUnifiedSnapshotMismatchKind.Epoch,
                    AiUnifiedSnapshotField.None,
                    -1,
                    ref mismatch) ||
                !MatchUnifiedSnapshotValue(
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

            UnifiedShadowRefreshComparisonSlotVisitCount++;
            if (!TryCompareUnifiedSnapshotRow(
                    production,
                    unified,
                    unifiedBoundaryFlags,
                    consumer,
                    slot,
                    ref mismatch))
            {
                return false;
            }

            if (!TryCompareUnifiedSnapshotCandidateProducts(
                    consumer,
                    false,
                    slot,
                    ref mismatch))
            {
                return false;
            }

            return TryCompareUnifiedSnapshotMutationWitness(
                consumer,
                ref mismatch);
        }

        private static bool TryCompareUnifiedSnapshotRow(
            AiSoASensingRows production,
            AiSoASensingRows unified,
            int[] unifiedBoundaryFlags,
            AiUnifiedSnapshotConsumer consumer,
            int slot,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            if (!MatchUnifiedSnapshotValue(production.Included[slot] ? 1 : 0, unified.Included[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.Included, AiUnifiedSnapshotField.None, slot, ref mismatch) ||
                !MatchUnifiedSnapshotValue(production.SpecialScanMember[slot] ? 1 : 0, unified.SpecialScanMember[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.SpecialMembership, AiUnifiedSnapshotField.None, slot, ref mismatch))
            {
                return false;
            }
            if (!production.Included[slot])
                return true;

            return MatchUnifiedSnapshotValue(production.Generation[slot], unified.Generation[slot], consumer, AiUnifiedSnapshotMismatchKind.Generation, AiUnifiedSnapshotField.None, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Identity[slot], unified.Identity[slot], consumer, AiUnifiedSnapshotMismatchKind.Identity, AiUnifiedSnapshotField.None, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.InputHistoryGate[slot] ? 1 : 0, unified.InputHistoryGate[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.InputHistoryGate, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.ObjectId[slot], unified.ObjectId[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.ObjectId, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.DataObjectType[slot], unified.DataObjectType[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.DataObjectType, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.X[slot], unified.X[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.X, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Y[slot], unified.Y[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Y, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Z[slot], unified.Z[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Z, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Hp[slot], unified.Hp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Hp3[slot], unified.Hp3[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Hp3, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.HpMax[slot], unified.HpMax[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HpMax, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Pp[slot], unified.Pp[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Pp, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Team[slot], unified.Team[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Team, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.State[slot], unified.State[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.State, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Frame[slot], unified.Frame[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Frame, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.HitJ[slot], unified.HitJ[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HitJ, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.LinkState[slot], unified.LinkState[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.LinkState, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.KillCount[slot], unified.KillCount[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.KillCount, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.CachedTargetSlot[slot], unified.CachedTargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CachedTargetSlot, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.CoordinateTargetX[slot], unified.CoordinateTargetX[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.CoordinateTargetX, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(BitConverter.DoubleToInt64Bits(production.Vx[slot]), BitConverter.DoubleToInt64Bits(unified.Vx[slot]), consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.VxBits, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Facing[slot], unified.Facing[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.Facing, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.TargetSlot[slot], unified.TargetSlot[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.TargetSlot, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.HitStop[slot], unified.HitStop[slot], consumer, AiUnifiedSnapshotMismatchKind.Field, AiUnifiedSnapshotField.HitStop, slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.BoundaryFlags[slot], unifiedBoundaryFlags[slot], consumer, AiUnifiedSnapshotMismatchKind.BoundaryFlags, AiUnifiedSnapshotField.None, slot, ref mismatch);
        }



        private bool TryCompareUnifiedSnapshotCandidateProducts(
            AiUnifiedSnapshotConsumer consumer,
            bool fullComparison,
            int refreshSlot,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            int firstSlot = fullComparison ? 0 : refreshSlot;
            int lastSlot = fullComparison
                ? UnifiedSnapshotFallbackSlots.Length - 1
                : refreshSlot;
            for (int slot = firstSlot; slot <= lastSlot; slot++)
            {
#if UNITY_INCLUDE_TESTS
                if (UnifiedProductMutationKindForSelfCheck ==
                        AiUnifiedSnapshotProductMutationKind.FallbackReference &&
                    UnifiedProductMutationSlotForSelfCheck == slot)
                {
                    UnifiedProductMutationKindForSelfCheck =
                        AiUnifiedSnapshotProductMutationKind.None;
                    UnifiedProductMutationSlotForSelfCheck = -1;
                    return SetUnifiedSnapshotMismatch(
                        consumer,
                        AiUnifiedSnapshotMismatchKind.FallbackReference,
                        AiUnifiedSnapshotField.FallbackSlot,
                        slot,
                        input.Slots[slot]?.Runtime?.StableId ?? 0,
                        -1,
                        ref mismatch);
                }
#endif
                if (!ReferenceEquals(input.Slots[slot], UnifiedSnapshotFallbackSlots[slot]))
                {
                    return SetUnifiedSnapshotMismatch(
                        consumer,
                        AiUnifiedSnapshotMismatchKind.FallbackReference,
                        AiUnifiedSnapshotField.FallbackSlot,
                        slot,
                        input.Slots[slot]?.Runtime?.StableId ?? 0,
                        UnifiedSnapshotFallbackSlots[slot]?.Runtime?.StableId ?? 0,
                        ref mismatch);
                }
            }

            if (!MatchUnifiedSnapshotValue(input.MoveModeFirst10Valid ? 1 : 0, UnifiedMoveModeFirst10Valid ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeValid, -1, ref mismatch))
                return false;
            if (!input.MoveModeFirst10Valid)
                return true;

            int firstMoveSlot = fullComparison ? 0 : refreshSlot;
            int lastMoveSlot = fullComparison
                ? input.MoveModeFirst10Present.Length - 1
                : refreshSlot;
            if (firstMoveSlot < input.MoveModeFirst10Present.Length)
            {
                for (int slot = firstMoveSlot; slot <= lastMoveSlot; slot++)
                {
#if UNITY_INCLUDE_TESTS
                    if (UnifiedProductMutationKindForSelfCheck ==
                            AiUnifiedSnapshotProductMutationKind.MoveModeFirst10Hp &&
                        UnifiedProductMutationSlotForSelfCheck == slot)
                    {
                        UnifiedProductMutationKindForSelfCheck =
                            AiUnifiedSnapshotProductMutationKind.None;
                        UnifiedProductMutationSlotForSelfCheck = -1;
                        return SetUnifiedSnapshotMismatch(
                            consumer,
                            AiUnifiedSnapshotMismatchKind.MoveModeProduct,
                            AiUnifiedSnapshotField.MoveModeHp,
                            slot,
                            input.MoveModeFirst10Hp[slot],
                            UnifiedMoveModeFirst10Hp[slot] + 1L,
                            ref mismatch);
                    }
#endif
                    if (!MatchUnifiedSnapshotValue(input.MoveModeFirst10Present[slot] ? 1 : 0, UnifiedMoveModeFirst10Present[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModePresent, slot, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(input.MoveModeFirst10Eligible[slot] ? 1 : 0, UnifiedMoveModeFirst10Eligible[slot] ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeEligible, slot, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(input.MoveModeFirst10Generation[slot], UnifiedMoveModeFirst10Generation[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeGeneration, slot, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(input.MoveModeFirst10Hp[slot], UnifiedMoveModeFirst10Hp[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeHp, slot, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(input.MoveModeFirst10X[slot], UnifiedMoveModeFirst10X[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeX, slot, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(input.MoveModeFirst10Z[slot], UnifiedMoveModeFirst10Z[slot], consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeZ, slot, ref mismatch))
                    {
                        return false;
                    }
                }
            }

            return MatchUnifiedSnapshotValue(input.MoveModeTopSlot, UnifiedMoveModeTopSlot, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeTopSlot, -1, ref mismatch) &&
                   MatchUnifiedSnapshotValue(input.MoveModeTopX, UnifiedMoveModeTopX, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeTopX, -1, ref mismatch) &&
                   MatchUnifiedSnapshotValue(input.MoveModeTopZ, UnifiedMoveModeTopZ, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeTopZ, -1, ref mismatch) &&
                   MatchUnifiedSnapshotValue(input.MoveModeSecondSlot, UnifiedMoveModeSecondSlot, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeSecondSlot, -1, ref mismatch) &&
                   MatchUnifiedSnapshotValue(input.MoveModeSecondX, UnifiedMoveModeSecondX, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeSecondX, -1, ref mismatch) &&
                   MatchUnifiedSnapshotValue(input.MoveModeSecondZ, UnifiedMoveModeSecondZ, consumer, AiUnifiedSnapshotMismatchKind.MoveModeProduct, AiUnifiedSnapshotField.MoveModeSecondZ, -1, ref mismatch);
        }



        private bool TryCompareUnifiedSnapshotMutationWitness(
            AiUnifiedSnapshotConsumer consumer,
            ref AiUnifiedSnapshotMismatch mismatch)
        {
            AiUnifiedSnapshotMutationWitness production;
            switch (consumer)
            {
                case AiUnifiedSnapshotConsumer.SoASensing:
                    production = SoASensingMutationWitness;
                    break;
                case AiUnifiedSnapshotConsumer.IndexedDecision:
                    production = DecisionMutationWitness;
                    break;
                default:
                    return false;
            }

            AiUnifiedSnapshotMutationWitness unified =
                UnifiedSnapshotMutationWitness;
#if UNITY_INCLUDE_TESTS
            if (UnifiedWitnessMutationConsumerForSelfCheck == consumer)
            {
                unified.Ordinal++;
                UnifiedWitnessMutationConsumerForSelfCheck =
                    AiUnifiedSnapshotConsumer.None;
            }
#endif
            UnifiedShadowMutationWitnessComparedCount++;
            return MatchUnifiedSnapshotValue(unchecked((long)production.Epoch), unchecked((long)unified.Epoch), consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessEpoch, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Ordinal, unified.Ordinal, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOrdinal, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Slot, unified.Slot, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessSlot, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.Generation, unified.Generation, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessGeneration, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.StableId, unified.StableId, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessStableId, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.RoleRebuilt ? 1 : 0, unified.RoleRebuilt ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessRoleRebuilt, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.TeamRebuilt ? 1 : 0, unified.TeamRebuilt ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessTeamRebuilt, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.RoleVersion, unified.RoleVersion, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessRoleVersion, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.TeamVersion, unified.TeamVersion, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessTeamVersion, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.OldX, unified.OldX, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldX, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.NewX, unified.NewX, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewX, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.OldTeam, unified.OldTeam, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldTeam, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.NewTeam, unified.NewTeam, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewTeam, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.OldRoleFlags, unified.OldRoleFlags, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldRoleFlags, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.NewRoleFlags, unified.NewRoleFlags, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewRoleFlags, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.OldLiving ? 1 : 0, unified.OldLiving ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldLiving, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.NewLiving ? 1 : 0, unified.NewLiving ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewLiving, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.OldHp, unified.OldHp, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessOldHp, production.Slot, ref mismatch) &&
                   MatchUnifiedSnapshotValue(production.NewHp, unified.NewHp, consumer, AiUnifiedSnapshotMismatchKind.MutationWitness, AiUnifiedSnapshotField.WitnessNewHp, production.Slot, ref mismatch);
        }



        private bool TryCompareUnifiedSnapshotIndexes(
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
                RecordUnifiedSnapshotDerivedComparisonEntries(2);
                if (!MatchUnifiedSnapshotValue(production.SpecialIndexReady ? 1 : 0, unified.SpecialIndexReady ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.IndexReadiness, AiUnifiedSnapshotField.SpecialIndex, -1, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.SpecialSlotCount, unified.SpecialSlotCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.SpecialIndex, -1, ref mismatch))
                {
                    return false;
                }
                for (int index = 0; index < production.SpecialSlotCount; index++)
                {
                    RecordUnifiedSnapshotDerivedComparisonEntries(1);
                    if (!MatchUnifiedSnapshotValue(production.SpecialSlots[index], unified.SpecialSlots[index], consumer, AiUnifiedSnapshotMismatchKind.IndexEntry, AiUnifiedSnapshotField.SpecialIndex, index, ref mismatch))
                        return false;
                }
            }

            if (compareRoleIndexes)
            {
                RecordUnifiedSnapshotDerivedComparisonEntries(5);
                if (!MatchUnifiedSnapshotValue(production.RoleIndexesReady ? 1 : 0, unified.RoleIndexesReady ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.IndexReadiness, AiUnifiedSnapshotField.GroundRoleIndex, -1, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.GroundRoleSlotCount, unified.GroundRoleSlotCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.GroundRoleIndex, -1, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.AirRoleSlotCount, unified.AirRoleSlotCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.AirRoleIndex, -1, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.GroundRoleTeamSummaryCount, unified.GroundRoleTeamSummaryCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.GroundRoleTeamSummary, -1, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.AirRoleTeamSummaryCount, unified.AirRoleTeamSummaryCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.AirRoleTeamSummary, -1, ref mismatch))
                {
                    return false;
                }
                for (int index = 0; index < production.GroundRoleSlotCount; index++)
                {
                    RecordUnifiedSnapshotDerivedComparisonEntries(1);
                    if (!MatchUnifiedSnapshotValue(production.GroundRoleSlotsByX[index], unified.GroundRoleSlotsByX[index], consumer, AiUnifiedSnapshotMismatchKind.IndexEntry, AiUnifiedSnapshotField.GroundRoleIndex, index, ref mismatch))
                        return false;
                }
                for (int index = 0; index < production.AirRoleSlotCount; index++)
                {
                    RecordUnifiedSnapshotDerivedComparisonEntries(1);
                    if (!MatchUnifiedSnapshotValue(production.AirRoleSlotsByX[index], unified.AirRoleSlotsByX[index], consumer, AiUnifiedSnapshotMismatchKind.IndexEntry, AiUnifiedSnapshotField.AirRoleIndex, index, ref mismatch))
                        return false;
                }
                for (int index = 0; index < production.GroundRoleTeamSummaryCount; index++)
                {
                    RecordUnifiedSnapshotDerivedComparisonEntries(1);
                    AiSensingRoleTeamSummary expected = production.GroundRoleTeamSummaries[index];
                    AiSensingRoleTeamSummary actual = unified.GroundRoleTeamSummaries[index];
                    if (!MatchUnifiedSnapshotValue(expected.Team, actual.Team, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.GroundRoleTeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.Start, actual.Start, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.GroundRoleTeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.Count, actual.Count, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.GroundRoleTeamSummary, index, ref mismatch))
                    {
                        return false;
                    }
                }
                for (int index = 0; index < production.AirRoleTeamSummaryCount; index++)
                {
                    RecordUnifiedSnapshotDerivedComparisonEntries(1);
                    AiSensingRoleTeamSummary expected = production.AirRoleTeamSummaries[index];
                    AiSensingRoleTeamSummary actual = unified.AirRoleTeamSummaries[index];
                    if (!MatchUnifiedSnapshotValue(expected.Team, actual.Team, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.AirRoleTeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.Start, actual.Start, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.AirRoleTeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.Count, actual.Count, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.AirRoleTeamSummary, index, ref mismatch))
                    {
                        return false;
                    }
                }
            }

            if (compareTeamSummaries)
            {
                RecordUnifiedSnapshotDerivedComparisonEntries(2);
                if (!MatchUnifiedSnapshotValue(production.TeamSummariesReady ? 1 : 0, unified.TeamSummariesReady ? 1 : 0, consumer, AiUnifiedSnapshotMismatchKind.IndexReadiness, AiUnifiedSnapshotField.TeamSummary, -1, ref mismatch) ||
                    !MatchUnifiedSnapshotValue(production.TeamSummaryCount, unified.TeamSummaryCount, consumer, AiUnifiedSnapshotMismatchKind.IndexCount, AiUnifiedSnapshotField.TeamSummary, -1, ref mismatch))
                {
                    return false;
                }
                for (int index = 0; index < production.TeamSummaryCount; index++)
                {
                    RecordUnifiedSnapshotDerivedComparisonEntries(1);
                    AiSensingTeamSummary expected = production.TeamSummaries[index];
                    AiSensingTeamSummary actual = unified.TeamSummaries[index];
                    if (!MatchUnifiedSnapshotValue(expected.Team, actual.Team, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.Count, actual.Count, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.MinHp, actual.MinHp, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.MinCount, actual.MinCount, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch) ||
                        !MatchUnifiedSnapshotValue(expected.SecondMinHp, actual.SecondMinHp, consumer, AiUnifiedSnapshotMismatchKind.SummaryEntry, AiUnifiedSnapshotField.TeamSummary, index, ref mismatch))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void RecordUnifiedSnapshotDerivedComparisonEntries(long count)
        {
            UnifiedShadowDerivedComparisonEntryVisitCount += count;
            if (UnifiedSnapshotRefreshComparisonActive)
            {
                UnifiedShadowRefreshDerivedFullLoopEntryVisitCount +=
                    count;
            }
        }



        private LF2Entity AtInputSlot(int slot)
        {
            return slot >= 0 && slot < input.Slots.Length
                ? input.Slots[slot]
                : null;
        }

        private bool TryGetRuntimeSlotReadOnlyView(
            int slot,
            out RuntimeSlotTable.ReadOnlySlotView view)
        {
            if (!runtimeSlots.IsAddressable(slot))
            {
                view = default;
                return false;
            }
            view = runtimeSlots.GetReadOnlyView(slot);
            return true;
        }


        internal bool TryBindAiSoADecisionRowContext(
            SimulationWorld world,
            LF2Entity self,
            int selectedSlot,
            int cachedSlot,
            LF2Entity cached)
        {
            DecisionRemainderAttemptedForCurrentInput = true;
            AiSoASensingRows rows = sensing.Rows;
            var context = new AiDecisionRowContext
            {
                Rows = rows,
                Slots = input.Slots,
                OccupancyEpoch = sensing.SnapshotEpoch,
            };
            bool captured = TryCaptureAiSoADecisionRowIdentity(
                                rows,
                                self?.Runtime?.SlotIndex ?? -1,
                                self,
                                out context.Self) &&
                            TryCaptureAiSoADecisionRowIdentity(
                                rows,
                                selectedSlot,
                                AtInputSlot(selectedSlot),
                                out context.Selected) &&
                            TryCaptureAiSoADecisionRowIdentity(
                                rows,
                                cachedSlot,
                                cached,
                                out context.Cached);
            if (!captured ||
                !ValidateAiSoADecisionRowContext(ref context, terminalGuard: false))
            {
                LatchAiSoADecisionRemainderToLegacy(world);
                return false;
            }

            context.Bound = true;
            DecisionRowContext = context;
            DecisionRemainderUseRowsForCurrentInput = true;
            DecisionRemainderContextBindCount++;
            return true;
        }

        private static bool TryCaptureAiSoADecisionRowIdentity(
            AiSoASensingRows rows,
            int slot,
            LF2Entity entity,
            out AiDecisionRowIdentity identity)
        {
            identity = new AiDecisionRowIdentity
            {
                Entity = entity,
                Slot = slot,
            };
            if (rows == null)
                return false;
            if (slot < 0 || slot >= rows.Capacity)
                return entity == null;

            identity.Included = rows.Included[slot];
            identity.Generation = rows.Generation[slot];
            identity.StableId = rows.Identity[slot];
            return true;
        }

        private bool ValidateAiSoADecisionRowContext(
            ref AiDecisionRowContext context,
            bool terminalGuard)
        {
            DecisionRemainderGatewayValidationCount++;
            bool mutationActive =
                DecisionRemainderMutationKind != 0 &&
                DecisionRemainderMutationAfterRandom == terminalGuard &&
                (!terminalGuard || DecisionRemainderRandomBoundaryPassed);
            if ((terminalGuard &&
                 DecisionRemainderRandomBoundaryPassed &&
                 DecisionRemainderForceAfterRandomFailure) ||
                (DecisionRemainderForceBeforeRandomFailure &&
                 !terminalGuard) ||
                (mutationActive &&
                 DecisionRemainderMutationKind == 1) ||
                !DecisionRemainderEnabled ||
                sensing.Mode != AiSensingMode.SoAAiSensing ||
                sensing.CandidatePassLatchedToLegacy ||
                !sensing.SnapshotValid ||
                sensing.PassInvalidated ||
                context.Rows == null ||
                context.Slots == null ||
                !ReferenceEquals(context.Rows, sensing.Rows) ||
                !ReferenceEquals(context.Slots, input.Slots) ||
                context.Rows.Capacity != context.Slots.Length ||
                runtimeSlots.LogicalCapacity != context.Rows.Capacity ||
                context.OccupancyEpoch != sensing.SnapshotEpoch ||
                input.SlotSnapshotOccupancyEpoch != context.OccupancyEpoch ||
                runtimeSlots.OccupancyEpoch != context.OccupancyEpoch)
            {
                return false;
            }

            return ValidateAiSoADecisionRowIdentity(
                       ref context,
                       context.Self,
                       mutationActive &&
                       DecisionRemainderMutationKind == 2) &&
                   ValidateAiSoADecisionRowIdentity(
                       ref context,
                       context.Selected,
                       mutationActive &&
                       DecisionRemainderMutationKind == 3) &&
                   ValidateAiSoADecisionRowIdentity(
                       ref context,
                       context.Cached,
                       mutationActive &&
                       DecisionRemainderMutationKind == 4);
        }

        private bool ValidateAiSoADecisionRowIdentity(
            ref AiDecisionRowContext context,
            AiDecisionRowIdentity identity,
            bool forceMismatch)
        {
            DecisionRemainderRowVisitCount++;
            if (forceMismatch)
                return false;
            if (identity.Slot < 0 || identity.Slot >= context.Rows.Capacity)
                return identity.Entity == null && !identity.Included;
            if (!identity.Included)
            {
                return identity.Entity == null &&
                       context.Slots[identity.Slot] == null &&
                       TryGetRuntimeSlotReadOnlyView(
                           identity.Slot,
                           out RuntimeSlotTable.ReadOnlySlotView emptyView) &&
                       emptyView.RuntimeSlot == identity.Slot &&
                       !emptyView.Claimed &&
                       emptyView.Entity == null;
            }

            return identity.Entity?.Runtime != null &&
                   ReferenceEquals(context.Slots[identity.Slot], identity.Entity) &&
                   identity.Entity.Runtime.SlotIndex == identity.Slot &&
                   identity.Entity.Runtime.StableId == identity.StableId &&
                   runtimeSlots.TryGetCurrentHandle(
                       identity.Slot,
                       identity.Entity,
                       out RuntimeEntityHandle handle) &&
                   handle.Generation == identity.Generation;
        }

        internal void TrackAiSoADecisionSelectedRow(int selectedSlot)
        {
            if (!DecisionRemainderUseRowsForCurrentInput)
                return;

            TryCaptureAiSoADecisionRowIdentity(
                DecisionRowContext.Rows,
                selectedSlot,
                AtInputSlot(selectedSlot),
                out DecisionRowContext.Selected);
        }

        internal bool TryGetAiSoADecisionRemainderRow(
            LF2Entity entity,
            out AiSoASensingRows rows,
            out int slot)
        {
            // The slot snapshot and SoA rows are published at one occupancy epoch.
            // Registry mutations are deferred for this pass, and the lease is guarded
            // at bind/end, so property reads must not repeat global row validation.
            rows = null;
            slot = -1;
            if (!DecisionRemainderUseRowsForCurrentInput ||
                !DecisionRowContext.Bound)
            {
                return false;
            }

            rows = DecisionRowContext.Rows;
            if (entity?.Runtime == null || rows == null)
                return false;

            slot = entity.Runtime.SlotIndex;
            return slot >= 0 &&
                   slot < rows.Capacity &&
                   rows.Included[slot];
        }

        internal LF2Entity AiAt(int slot)
        {
            return AtInputSlot(slot);
        }

        internal int X(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.X[slot]
                : entity.Runtime.XInt;

        internal int Y(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Y[slot]
                : entity.Runtime.YInt;

        internal int Z(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Z[slot]
                : entity.Runtime.ZInt;

        internal int Hp(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Hp[slot]
                : entity.Runtime.HP;

        internal int Hp3(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Hp3[slot]
                : entity.Runtime.HP3;

        internal int HpMax(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.HpMax[slot]
                : entity.Runtime.HPBound;

        internal int Pp(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Pp[slot]
                : entity.Runtime.PP;

        internal int Team(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Team[slot]
                : entity.Runtime.RelationTeam;

        internal int Slot(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out _, out int slot)
                ? slot
                : entity.Runtime.SlotIndex;

        internal int Frame(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Frame[slot]
                : entity.Runtime.Frame;

        internal int HitJ(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.HitJ[slot]
                : SimulationAiSensingModule.CaptureCurrentFrameHitJ(
                    entity,
                    entity.Runtime.Frame);

        internal int State(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.State[slot]
                : entity.GetState();

        internal int Facing(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Facing[slot]
                : entity.Runtime.Dir == "left" ? 1 : 0;

        internal int ObjectId(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.ObjectId[slot]
                : entity.ObjectId;

        internal int LinkState(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.LinkState[slot]
                : entity.Runtime.LinkState;

        internal int TargetSlot(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.TargetSlot[slot]
                : entity.Runtime.TargetSlotIndex;

        internal int HitStop(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.HitStop[slot]
                : entity.Runtime.HitStop;

        internal double Vx(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.Vx[slot]
                : entity.Runtime.Vx;

        internal bool HasInputHistoryGate(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.InputHistoryGate[slot]
                : entity.Runtime.HasInputHistoryGate();

        internal bool HasBoundaryBlock(LF2Entity entity) =>
            TryGetAiSoADecisionRemainderRow(entity, out AiSoASensingRows rows, out int slot)
                ? rows.BoundaryFlags[slot] != 0
                : entity.Runtime.ZBoundNegative ||
                  entity.Runtime.ZBoundPositive ||
                  entity.Runtime.XBoundNegative ||
                  entity.Runtime.XBoundPositive;

        internal int Distance(LF2Entity a, LF2Entity b) =>
            Math.Abs(X(b) - X(a)) + Math.Abs(Z(b) - Z(a));

        internal bool IsCharacterDat(LF2Entity entity) =>
            entity != null &&
            (TryGetAiSoADecisionRemainderRow(
                 entity,
                 out AiSoASensingRows rows,
                 out int slot)
                ? rows.DataObjectType[slot]
                : entity.GetCurrentDataObjectTypeForSimulation()) == 0;

        internal bool IsLivingCharacterDat(LF2Entity entity) =>
            IsCharacterDat(entity) && Hp(entity) > 0;

        internal void LatchAiSoADecisionRemainderToLegacy(SimulationWorld world)
        {
            if (!DecisionRemainderAttemptedForCurrentInput)
                return;
            if ((UnifiedSnapshotExecutionCommittedThisPass || UnifiedSnapshotExecutionConsumerStartedThisPass))
            {
                world.ThrowAiUnifiedSnapshotExecutionHardBreachForDecisionModule(
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare,
                    "AI decision remainder attempted fallback after unified snapshot commit.");
            }

            if (DecisionRemainderRandomBoundaryPassed)
            {
                RecordAiSoADecisionRemainderHardFailure(world);
                return;
            }

            DecisionRemainderAttemptedForCurrentInput = false;
            DecisionRemainderUseRowsForCurrentInput = false;
            DecisionRowContext = default;
            DecisionRemainderFallbackCount++;
            DecisionRemainderPreRandomFailureCount++;
        }

        private void RecordAiSoADecisionRemainderHardFailure(SimulationWorld world)
        {
            if (DecisionRemainderHardFailureRecorded)
                return;

            DecisionRemainderHardFailureRecorded = true;
            DecisionRemainderPostRandomFailureCount++;
            DecisionRemainderHardFailureCount++;
            if ((UnifiedSnapshotExecutionCommittedThisPass || UnifiedSnapshotExecutionConsumerStartedThisPass))
            {
                world.ThrowAiUnifiedSnapshotExecutionHardBreachForDecisionModule(
                    AiUnifiedSnapshotExceptionStage.InitialDecisionCompare,
                    "AI decision remainder failed after the random boundary under unified snapshot authority.");
            }
        }

        internal void CompleteAiSoADecisionRemainderInput(SimulationWorld world)
        {
            if (DecisionRemainderAttemptedForCurrentInput &&
                DecisionRemainderUseRowsForCurrentInput)
            {
                if (!ValidateAiSoADecisionRowContext(
                        ref DecisionRowContext,
                        terminalGuard: true))
                {
                    if (DecisionRemainderRandomBoundaryPassed)
                    {
                        RecordAiSoADecisionRemainderHardFailure(world);
                    }
                    else
                    {
                        DecisionRemainderFallbackCount++;
                        DecisionRemainderPreRandomFailureCount++;
                    }
                }
                else if (!DecisionRemainderHardFailureRecorded)
                {
                    DecisionRemainderAppliedCount++;
                }
            }

            DecisionRemainderAttemptedForCurrentInput = false;
            DecisionRemainderUseRowsForCurrentInput = false;
            DecisionRemainderRandomBoundaryPassed = false;
            DecisionRemainderHardFailureRecorded = false;
            DecisionRowContext = default;
        }



        internal void PrepareAiInputBasicLegacyCore(
            SimulationWorld world,
            LF2Entity self,
            int tickIndex)
        {
            LegacyCharacterDecisionPosition = 0;
            DecisionRemainderUseRowsForCurrentInput = false;
            DecisionRemainderAttemptedForCurrentInput = false;
            DecisionRemainderRandomBoundaryPassed = false;
            DecisionRemainderHardFailureRecorded = false;
            DecisionRowContext = default;
            bool compareSoAShadow = sensing.Mode == AiSensingMode.SoAShadowAiSensing;
            bool useSoACandidate = sensing.Mode == AiSensingMode.SoAAiSensing;
            if (compareSoAShadow)
                world.BeginAiSoASensingShadowComparison(self, tickIndex);
            else if (useSoACandidate)
                world.EnsureAiSensingModeAvailableBeforeTick();

            // Alignment contract R3-AI-LIFE-001: the C++ caller reaches AI input
            // before death/respawn cleanup, including an active zero-HP character DAT.
            if (self?.Runtime == null)
                return;

            NTSDEntityRuntime input = self.Runtime;
            BattleAiInputDetailDiagnostics decisionDiagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (input.Unk3FC > -1000)
            {
                RollAndClearAiKeys(world,input);
                MoveTowardCoordinate(world,self, CreateCoordinateAiInputContext(world));
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: false,
                    postSpecialRngCallsBefore: 0);
                return;
            }

            ulong contextRngCallsBefore = decisionDiagnostics != null
                ? world.Rng?.CallCount ?? 0
                : 0;
            decisionDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.ContextMoveMode);
            decisionDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.ContextMoveMode);
            AiInputContext ai = CreateAiInputContext(world, self, tickIndex);
            decisionDiagnostics?.EndPhase(
                BattleAiInputDetailPhase.ContextMoveMode);
            if (decisionDiagnostics != null)
            {
                decisionDiagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.ContextMoveMode,
                    SimulationWorld.ResolveAiInputDetailRngCallDelta(
                        contextRngCallsBefore,
                        world.Rng?.CallCount ?? 0));
            }

            int selectedSlot;
            int bestDist;
            bool sameZLane;
            bool candidateUsesLegacyForThisInput =
                useSoACandidate && sensing.CandidatePassLatchedToLegacy;
            if (useSoACandidate && !candidateUsesLegacyForThisInput)
            {
                if (world.TryRunAiSoACandidateNearest(
                        self,
                        ai.InputPhase,
                        out AiSoANearestResult nearest))
                {
                    selectedSlot = nearest.SelectedSlot;
                    bestDist = nearest.BestDist;
                    sameZLane = nearest.SameZLane;
                }
                else
                {
                    world.LatchAiSoACandidateToLegacyBeforeRandom();
                    candidateUsesLegacyForThisInput = true;
                    sensing.CandidateLegacyNearestScanCount++;
                    selectedSlot = world.FindNearestAiTargetSlotBrute(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
            }
            else
            {
                if (useSoACandidate)
                {
                    sensing.CandidateLegacyNearestScanCount++;
                    selectedSlot = world.FindNearestAiTargetSlotBrute(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
                else
                {
                    selectedSlot = world.FindNearestAiTargetSlot(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
            }
            if (compareSoAShadow)
                world.CompareAiSoASensingInitial(self, tickIndex, selectedSlot, bestDist, sameZLane);
            decisionDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.CachedTargetRetention);
            decisionDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.CachedTargetRetention);
            int savedTargetSlot = input.Unk360;
            LF2Entity cached = world.AiAt(savedTargetSlot);
            decisionDiagnostics?.RecordPhaseSlotVisits(
                BattleAiInputDetailPhase.CachedTargetRetention,
                1);
            bool decisionRemainderRequested =
                DecisionRemainderEnabled &&
                sensing.Mode == AiSensingMode.SoAAiSensing;
            if (decisionRemainderRequested)
                DecisionRemainderEligibleAttemptCount++;
            if (decisionRemainderRequested &&
                !TryBindAiSoADecisionRowContext(world, self,
                    selectedSlot,
                    savedTargetSlot,
                    cached))
            {
                if (!candidateUsesLegacyForThisInput)
                {
                    candidateUsesLegacyForThisInput = true;
                    sensing.CandidateLegacyNearestScanCount++;
                    selectedSlot = world.FindNearestAiTargetSlotBrute(
                        self,
                        ai,
                        out bestDist,
                        out sameZLane);
                }
            }
            uint cacheRngStateBefore = world.Rng?.State ?? 0;
            ulong cacheRngCallsBefore = world.Rng?.CallCount ?? 0;
            bool cachedTargetEligible = world.IsLivingCharacterDat(cached);
            bool cacheRandomCalled = false;
            int cacheRoll = 0;
            if (cachedTargetEligible)
            {
                cacheRandomCalled = true;
                cacheRoll = world.Rand(30);
                if (cacheRoll > 0)
                    selectedSlot = savedTargetSlot;
                else
                    input.Unk360 = selectedSlot;
            }
            else
            {
                input.Unk360 = selectedSlot;
            }
            uint cacheRngStateAfter = world.Rng?.State ?? 0;
            ulong cacheRngCallsAfter = world.Rng?.CallCount ?? 0;
            if (compareSoAShadow)
            {
                world.ContinueAiSoASensingShadowComparisonAfterCache(
                    self,
                    tickIndex,
                    cachedTargetEligible,
                    cacheRandomCalled,
                    cacheRoll,
                    cacheRngStateBefore,
                    cacheRngCallsBefore,
                    cacheRngStateAfter,
                    cacheRngCallsAfter,
                    selectedSlot);
            }
            decisionDiagnostics?.EndPhase(
                BattleAiInputDetailPhase.CachedTargetRetention);
            if (decisionDiagnostics != null)
            {
                decisionDiagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.CachedTargetRetention,
                    SimulationWorld.ResolveAiInputDetailRngCallDelta(
                        cacheRngCallsBefore,
                        cacheRngCallsAfter));
            }

            if (selectedSlot < 0)
            {
                if (compareSoAShadow)
                    world.CompleteAiSoASensingComparisonWithoutSpecial(self, tickIndex);
                RollAndClearAiKeys(world,input);
                AiPostNoTargetFallback(world,self, cached, ai);
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: false,
                    postSpecialRngCallsBefore: 0);
                return;
            }

            int selectedBeforeSpecialScan = selectedSlot;
            bool specialObjectProximity = false;
            bool specialLeft = false;
            bool specialRight = false;
            bool specialUp = false;
            bool specialDown = false;
            bool specialGuard7A = false;
            bool specialGuard7B = false;
            bool specialForce7AGround = false;
            bool specialC8ThreatSeen = false;
            bool specialPostSelectionSeen = false;
            int specialBestDist = 10000;

            bool runLegacySpecialScan = !useSoACandidate || candidateUsesLegacyForThisInput;
            if (useSoACandidate && !runLegacySpecialScan)
            {
                if (world.TryRunAiSoACandidateSpecial(
                        self,
                        ai.InputPhase,
                        selectedSlot,
                        bestDist,
                        sameZLane,
                        out AiSoASpecialResult special))
                {
                    selectedSlot = special.SelectedSlot;
                    specialBestDist = special.BestDist;
                    int flags = special.Flags;
                    specialObjectProximity = (flags & AiSoASpecialProximity) != 0;
                    specialLeft = (flags & AiSoASpecialLeft) != 0;
                    specialRight = (flags & AiSoASpecialRight) != 0;
                    specialUp = (flags & AiSoASpecialUp) != 0;
                    specialDown = (flags & AiSoASpecialDown) != 0;
                    specialGuard7A = (flags & AiSoASpecialGuard7A) != 0;
                    specialGuard7B = (flags & AiSoASpecialGuard7B) != 0;
                    specialForce7AGround = (flags & AiSoASpecialForce7AGround) != 0;
                    specialC8ThreatSeen = (flags & AiSoASpecialC8ThreatSeen) != 0;
                    specialPostSelectionSeen =
                        (flags & AiSoASpecialPostSelectionSeen) != 0;
                }
                else
                {
                    world.LatchAiSoACandidateToLegacyAfterRandom();
                    LatchAiSoADecisionRemainderToLegacy(world);
                    runLegacySpecialScan =
                        !DecisionRemainderUseRowsForCurrentInput;
                }
            }

            if (runLegacySpecialScan)
            {
                if (useSoACandidate)
                    sensing.CandidateLegacySpecialScanCount++;

                if (ai.InputPhase == 1 || ai.InputPhase == 4)
                {
                    int selfTeam = world.Team(self);
                    if (selfTeam != 5)
                    {
                        specialForce7AGround = true;
                        if (world.Hp(self) > (4 * world.Hp3(self)) / 5 || world.Hp(self) > world.Hp3(self) - 130)
                            specialForce7AGround = false;
                        if (world.Hp(self) > 430 || world.Hp(self) > world.Hp3(self) - 130)
                            specialGuard7A = true;

                        world.ResolveAiSameTeamSummaryExcludingSelf(
                            self,
                            selfTeam,
                            out int sameTeamCount,
                            out int sameTeamMinHp);
                        if (sameTeamMinHp < world.Hp(self)) specialForce7AGround = false;
                        if (sameTeamMinHp < world.Hp(self) - 200) specialGuard7A = true;
                        if (sameTeamCount == 0) specialForce7AGround = false;
                    }
                }

                if (self.Runtime.KillCount > -1) { specialGuard7A = true; specialGuard7B = true; }
                if (world.Pp(self) > 250) specialGuard7B = true;
                if (ai.InputPhase == 1 && world.Team(self) == 1) specialGuard7B = true;
                if (world.Slot(self) >= 20 && ai.InputPhase == 4) specialGuard7B = true;

                // Candidate does not build the Legacy compact special list.  Once a
                // Candidate query fails, retain the authoritative slot order through
                // the full 20..capacity scan rather than rebuilding Legacy products.
                bool fullLegacySpecialScan =
                    world.ForceFullAiSpecialScanForDiagnostics || useSoACandidate;
                int specialScanCount = fullLegacySpecialScan
                    ? this.input.Slots.Length - 20
                    : this.input.SpecialScanSlots.Count;
                for (int specialScanIndex = 0; specialScanIndex < specialScanCount; specialScanIndex++)
                {
                    int i = fullLegacySpecialScan
                        ? specialScanIndex + 20
                        : this.input.SpecialScanSlots[specialScanIndex];
                    LF2Entity obj = world.AiAt(i);
                    if (obj == null) continue;
                    int objOid = obj.ObjectId;
                    int objState = world.State(obj);
                    if (objOid == 0xC8)
                    {
                        int frameGroup = world.Frame(obj) / 10;
                        bool threat = frameGroup == 6 && world.Team(obj) != world.Team(self);
                        if (!threat && frameGroup == 5)
                        {
                            bool lowHpWindow = (world.Hp(self) >= world.Hp3(self) - 70 || world.Hp(self) >= world.Hp3(self) - 200) &&
                                               (world.Hp(self) >= (3 * world.Hp3(self)) / 5 || world.Hp(self) < world.Hp3(self) - 200);
                            threat = (world.ObjectId(self) == 2 || world.ObjectId(self) == 34) && lowHpWindow && world.Team(obj) == world.Team(self);
                        }
                        if (threat) specialC8ThreatSeen = true;
                        if (threat && Math.Abs(world.Z(obj) - world.Z(self)) < 25 && Math.Abs(world.X(obj) - world.X(self)) < 150)
                        {
                            specialObjectProximity = true;
                            if (Math.Abs(world.Z(obj) - world.Z(self)) < 20)
                            {
                                if (Math.Abs(world.X(obj) - world.X(self)) < 180)
                                {
                                    if (world.Z(obj) <= world.Z(self)) specialUp = true; else specialDown = true;
                                }
                                if (world.X(obj) <= world.X(self)) specialLeft = true; else specialRight = true;
                            }
                        }
                    }

                    if ((objOid == 0xD3 && objState == 0x12) || (objOid == 0xD4 && world.Frame(obj) >= 150 && world.Frame(obj) <= 170))
                    {
                        if (Math.Abs(world.X(obj) - world.X(self)) < 80)
                        {
                            if (world.Z(obj) > world.Z(self) + 20) specialDown = true;
                            else if (world.Z(obj) < world.Z(self) - 20) specialUp = true;
                        }
                        if (Math.Abs(world.Z(obj) - world.Z(self)) < 20)
                        {
                            if (world.X(obj) > world.X(self) + 100) specialRight = true;
                            else if (world.X(obj) < world.X(self) - 100) specialLeft = true;
                        }
                    }

                    if (!specialPostSelectionSeen && !specialC8ThreatSeen && !sameZLane && input.LinkState == 0)
                    {
                        int dist = world.Distance(self, obj);
                        bool oidCandidate = objOid / 100 == 1 || objOid == 0xD5;
                        bool guarded = (objOid == 0x7A && specialGuard7A) || (objOid == 0x7B && specialGuard7B) ||
                                       (input.HasInputHistoryGate() && objOid != 0x7A);
                        if (dist < 2 * bestDist && dist < specialBestDist && oidCandidate && !guarded &&
                            obj.Runtime.LinkState == 0 && (objState == 0x3EC || objState == 0x7D4))
                        {
                            selectedSlot = i;
                            specialBestDist = dist;
                        }
                    }

                    if (objOid == 0xC8 && world.Frame(obj) / 10 == 5 && Math.Abs(world.X(obj) - world.X(self)) < 300 &&
                        Math.Abs(world.Z(obj) - world.Z(self)) < 90 && world.Team(obj) == world.Team(self))
                    {
                        bool pressure = (world.Hp(self) < world.HpMax(self) - 70 && world.Hp(self) < 140) ||
                                        (world.Hp(self) < (3 * world.HpMax(self)) / 5 && world.Hp(self) >= 140);
                        if (pressure) selectedSlot = i;
                        specialPostSelectionSeen = true;
                    }

                    if (specialForce7AGround && objOid == 0x7A && objState == 0x3EC && input.LinkState == 0)
                    {
                        selectedSlot = i;
                        specialPostSelectionSeen = true;
                    }
                }

                if (specialC8ThreatSeen) selectedSlot = selectedBeforeSpecialScan;
            }
            if (compareSoAShadow)
            {
                world.CompareAiSoASensingPostSpecial(
                    self,
                    tickIndex,
                    selectedSlot,
                    specialBestDist,
                    specialObjectProximity,
                    specialLeft,
                    specialRight,
                    specialUp,
                    specialDown,
                    specialGuard7A,
                    specialGuard7B,
                    specialForce7AGround,
                    specialC8ThreatSeen,
                    specialPostSelectionSeen);
            }
            TrackAiSoADecisionSelectedRow(selectedSlot);
            ulong postSpecialRngCallsBefore = decisionDiagnostics != null
                ? world.Rng?.CallCount ?? 0
                : 0;
            decisionDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.PostSpecialMainDecision);
            decisionDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.PostSpecialMainDecision);
            input.Unk360 = selectedSlot;
            RollAndClearAiKeys(world,input);
            LF2Entity target = world.AiAt(selectedSlot);
            decisionDiagnostics?.RecordPhaseSlotVisits(
                BattleAiInputDetailPhase.PostSpecialMainDecision,
                1);
            if (target == null)
            {
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }
            int selfState = world.State(self);
            int targetState = world.State(target);
            int targetOid = world.ObjectId(target);

            if (world.X(target) > world.X(self) && world.Facing(self) == 1) input.KeyRight = 1;
            if (world.X(target) < world.X(self) && world.Facing(self) == 0) input.KeyLeft = 1;
            if (selfState == 2) { if (world.Facing(self) == 1) input.KeyRight = 1; else input.KeyLeft = 1; }

            int blockRoll = world.Rand(ai.Rand5 + 8);
            if (blockRoll == 0 && world.HasBoundaryBlock(self))
            { input.PrevJump = 0; input.KeyJump = 1; }

            if (AiPreUpdateTarget3000SideEffect(world,self, target, selfState, targetState, ai))
            {
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (world.HasInputHistoryGate(self) && world.LinkState(self) > 0)
            {
                LF2Entity held = world.AiAt(world.TargetSlot(self));
                decisionDiagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.PostSpecialMainDecision,
                    1);
                if (held != null && (world.ObjectId(held) == 0x7A || world.ObjectId(held) == 0x7B))
                {
                    input.PrevJump = 0;
                    input.KeyJump = 1;
                    ApplyAiInputEdgesWithDiagnostics(world,
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
            }

            bool coordinateAllowsSpecial = !world.HasInputHistoryGate(self) || AiPostCacheCoordinateAllowsSpecial(world,self);
            if (coordinateAllowsSpecial && (targetState == 0x3EC || targetState == 0x7D4))
            {
                if (world.HasInputHistoryGate(self) && (Math.Abs(world.Z(self) - world.Z(target)) > 150 || Math.Abs(world.X(self) - world.X(target)) > 240) &&
                    targetOid != 0x7A && targetOid != 0x7B)
                {
                    ApplyAiInputEdgesWithDiagnostics(world,
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
                MoveTowardTarget(world,self, target, ai, selfState);
                if (Math.Abs(world.Z(target) - world.Z(self)) <= 3 && Math.Abs(world.X(target) - world.X(self)) <= 6) { input.PrevJump = 0; input.KeyJump = 1; }
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (targetState == 14 || Math.Abs(world.Y(target)) > 2)
            {
                if (world.X(target) > ai.StageTargetX - 30)
                {
                    input.KeyLeft = 1;
                    input.PrevLeft = 0;
                    ApplyAiInputEdgesWithDiagnostics(world,
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
                if (world.X(target) < 30)
                {
                    input.KeyRight = 1;
                    input.PrevRight = 0;
                    ApplyAiInputEdgesWithDiagnostics(world,
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
                if (Math.Abs(world.Z(target) - world.Z(self)) <= 45 || Math.Abs(world.X(target) - world.X(self)) <= 350)
                {
                    if (world.X(target) > world.X(self)) { input.KeyLeft = 1; if (world.Rand(ai.Rand20 + 35) == 0) input.PrevLeft = 0; }
                    else { input.KeyRight = 1; if (world.Rand(ai.Rand20 + 35) == 0) input.PrevRight = 0; }
                    if (world.Z(target) < world.Z(self) || world.Z(target) < world.StageZMin + 10) input.KeyDown = 1; else input.KeyUp = 1;
                }
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            bool c8Allowed = (world.HasInputHistoryGate(self) && (Math.Abs(world.Z(self) - world.Z(target)) > 150 || Math.Abs(world.X(self) - world.X(target)) > 240)) ||
                             (targetState != 14 && Math.Abs(world.Y(target)) <= 2);
            if (c8Allowed && targetOid == 0xC8)
            {
                if (world.X(target) > world.X(self) + 7) input.KeyRight = 1; else if (world.X(target) < world.X(self) - 7) input.KeyLeft = 1;
                if (world.Z(target) > world.Z(self) + 2) input.KeyDown = 1; else if (world.Z(target) < world.Z(self) - 2) input.KeyUp = 1;
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (world.Rand(ai.Rand5 + 1) == 0)
            {
                int characterDecisionPosition = 0;
                if (AiUpdateFirstDecision(world,self, target, bestDist, specialObjectProximity))
                {
                    characterDecisionPosition = 1;
                }
                else if (AiUpdateTeammateGuardDecision(world,self, ai, bestDist, sameZLane))
                {
                    characterDecisionPosition = 2;
                }
                else if (AiUpdateOid1ComboDecision(world,self, target, targetState))
                {
                    characterDecisionPosition = 3;
                }
                else if (AiUpdateCloseOid1Decision(world,self, target))
                {
                    characterDecisionPosition = 4;
                }
                else if (AiUpdateOid4ComboDecision(world,self, target))
                {
                    characterDecisionPosition = 5;
                }
                else if (AiUpdateOid5ComboDecision(world,self, target))
                {
                    characterDecisionPosition = 6;
                }
                else if (RunLegacyAiCharacterDecisionModule(
                             world,
                             self,
                             target,
                             targetState,
                             bestDist,
                             sameZLane,
                             ai,
                             out int matchedPosition))
                {
                    characterDecisionPosition = matchedPosition;
                }

                if (characterDecisionPosition != 0)
                {
                    LegacyCharacterDecisionPosition = characterDecisionPosition;
                    ApplyAiInputEdgesWithDiagnostics(world,
                        input,
                        decisionDiagnostics,
                        completePostSpecialMainDecision: true,
                        postSpecialRngCallsBefore);
                    return;
                }
            }

            bool closeOrFree = !world.HasInputHistoryGate(self) || (Math.Abs(world.Z(self) - world.Z(target)) <= 150 && Math.Abs(world.X(self) - world.X(target)) <= 240);
            int selfOid = world.ObjectId(self);
            bool widePath = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if (!widePath)
            {
                bool targetPressure = world.Hp(target) > world.Hp(self) * 2 || (world.Hp(self) <= 100 && world.Hp3(self) > 100);
                widePath = targetPressure && ai.InputPhase == 1 && world.IsCharacterDat(target) && world.Slot(self) >= 20 && world.Team(self) != 5;
            }

            if (closeOrFree)
            {
                if ((specialRight || ai.MoveMode == 1) && selfState == 2 && world.Facing(self) == 0) input.KeyLeft = 1;
                if (specialLeft && selfState == 2 && world.Facing(self) == 1) input.KeyRight = 1;
                int threshold = widePath ? 170 : 60;
                int near = widePath ? 150 : 0;
                if (selfState != 19)
                {
                    if ((world.X(target) > world.X(self) + threshold || ((world.X(target) > world.X(self) + near || (selfState == 7 && world.X(target) > world.X(self))) && world.Facing(self) == 1)) &&
                        !specialRight && ((widePath && ai.MoveMode == 0) || (!widePath && (ai.MoveMode == 0 || world.Facing(self) == 1))))
                    { input.KeyRight = 1; if (world.Rand(ai.Rand20 + 35) == 0) input.PrevRight = 0; }
                    if ((world.X(target) < world.X(self) - threshold || ((world.X(target) < world.X(self) - near || (selfState == 7 && world.X(target) < world.X(self))) && world.Facing(self) == 0)) && !specialLeft)
                    { input.KeyLeft = 1; if (world.Rand(ai.Rand20 + 35) == 0) input.PrevLeft = 0; }
                    if (((world.Z(target) > world.Z(self) + 3 && !specialObjectProximity) || ((specialRight || specialLeft) && specialUp)) && !specialDown) input.KeyDown = 1;
                    if (((world.Z(target) < world.Z(self) - 3 && !specialObjectProximity) || ((specialRight || specialLeft) && specialDown)) && !specialUp) input.KeyUp = 1;
                }
            }

            if (world.LinkState(self) > 0 && !AiProcessHelper(world,self, target, ai, selfState, targetState, sameZLane, specialObjectProximity))
            {
                ApplyAiInputEdgesWithDiagnostics(world,
                    input,
                    decisionDiagnostics,
                    completePostSpecialMainDecision: true,
                    postSpecialRngCallsBefore);
                return;
            }

            if (world.Rand(ai.Difficulty * 7 + 10) == 0 && (targetState == 3 || targetState / 100 == 3) &&
                Math.Abs(world.Z(target) - world.Z(self)) < 9 && ((world.Facing(target) == 0 && world.X(target) < world.X(self)) || (world.Facing(target) == 1 && world.X(target) > world.X(self))))
                input.KeyAttack = 1;
            if (closeOrFree && world.Rand(2 * (ai.Rand5 + 10)) < 3 && world.Rand(20) < 3 && targetState != 14) input.KeyDefend = 1;
            bool selfGroup = selfOid == 0x12 || selfOid == 5 || selfOid == 0x1F;
            if ((!selfGroup || targetState == 16) && Math.Abs(world.X(target) - 2 * (int)world.Vx(self) - world.X(self)) < 50 &&
                Math.Abs(world.Z(target) - world.Z(self)) < 5 && world.Rand(ai.Rand3 + 3) == 0 && targetState != 14) input.KeyJump = 1;

            AiProcessSubCallerPrewrite(world,self, target, ai, selfState, targetState);
            AiProcessSubLabel435PressurePrewrite(world,self, target, ai, selfState, targetState);
            AiProcessSubHelper(world,self, target, ai, targetState, specialLeft, specialRight);
            ApplyAiInputEdgesWithDiagnostics(world,
                input,
                decisionDiagnostics,
                completePostSpecialMainDecision: true,
                postSpecialRngCallsBefore);
        }



        private bool RunLegacyAiCharacterDecisionModule(SimulationWorld world,
            LF2Entity self,
            LF2Entity target,
            int targetState,
            int nearestTargetDistance,
            bool sameZLane,
            in AiInputContext ai,
            out int matchedPosition)
        {
            matchedPosition = 0;
            if (self?.Runtime == null || target?.Runtime == null)
                return false;

            AiDecisionSnapshot snapshot = null;
            AiDecisionAvailability availability = AiDecisionAvailability.SnapshotMissing;
            if (SharedPassAvailable && SharedSnapshot != null)
            {
                availability = world.CaptureAiDecisionSharedOwnedSnapshotForModule(
                    self,
                    SharedSnapshot);
                if (availability == AiDecisionAvailability.Available)
                    snapshot = SharedSnapshot;
            }

            if (snapshot == null)
            {
                snapshot = LegacyFallbackSnapshot;
                if (snapshot == null || snapshot.Rows.Capacity != world.RuntimeSlotCapacity)
                {
                    throw new InvalidOperationException(
                        "Legacy AI character-decision fallback snapshot is not prepared for the current runtime capacity.");
                }

                availability = world.CaptureAiDecisionShadowSnapshotForModule(self, snapshot);
            }

            if (availability != AiDecisionAvailability.Available)
            {
                throw new InvalidOperationException(
                    $"Legacy AI character-decision snapshot capture failed after the decision RNG boundary: {availability}.");
            }

            AiSensingSnapshot rows = snapshot.Rows;
            int selfSlot = self.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if ((uint)selfSlot >= (uint)rows.Capacity ||
                (uint)targetSlot >= (uint)rows.Capacity ||
                !rows.Included[selfSlot] ||
                !rows.Included[targetSlot])
            {
                throw new InvalidOperationException(
                    "Legacy AI character-decision snapshot does not contain the current self/target pair.");
            }

            AiDecisionInputState decisionInput = snapshot.Input;
            bool captureTrace = LegacyRngRecording;
            var random = new AiDecisionRandomStream(
                world.Rng.State,
                world.Rng.CallCount,
                captureTrace,
                captureTrace ? CharacterDecisionLegacyRngModuli : null,
                captureTrace ? CharacterDecisionLegacyRngRaw : null,
                captureTrace ? CharacterDecisionLegacyRngValues : null);
            var context = new AiCharacterDecisionContext(
                ai.MoveMode,
                ai.StageTargetX);

            bool matched = snapshot.CharacterDecisionModule.TryEvaluatePositions7Through39(
                rows,
                selfSlot,
                targetSlot,
                targetState,
                nearestTargetDistance,
                sameZLane,
                in context,
                ref decisionInput,
                ref random,
                out matchedPosition,
                out _);

            world.Rng.RestoreState(random.State, random.Calls);
            if (random.DrawCount > 0 && DecisionRemainderUseRowsForCurrentInput)
                DecisionRemainderRandomBoundaryPassed = true;

            if (captureTrace)
            {
                int capturedDrawCount = Math.Min(
                    random.DrawCount,
                    CharacterDecisionLegacyRngModuli.Length);
                for (int index = 0; index < capturedDrawCount; index++)
                {
                    RecordLegacyRng(
                        CharacterDecisionLegacyRngModuli[index],
                        CharacterDecisionLegacyRngRaw[index],
                        CharacterDecisionLegacyRngValues[index]);
                }
                if (random.TraceOverflow || random.DrawCount > capturedDrawCount)
                    LegacyRngOverflow = true;
            }

            world.CharacterInputWriter.CommitAiDecisionState(
                self.Runtime,
                decisionInput);
            return matched;
        }

        private void ApplyAiInputEdgesWithDiagnostics(SimulationWorld world,
            NTSDEntityRuntime input,
            BattleAiInputDetailDiagnostics diagnostics,
            bool completePostSpecialMainDecision,
            ulong postSpecialRngCallsBefore)
        {
            diagnostics?.RecordPhaseCall(BattleAiInputDetailPhase.InputEdges);
            diagnostics?.BeginPhase(BattleAiInputDetailPhase.InputEdges);
            world.CharacterInputWriter.ApplyInputEdges(input);
            CompleteAiSoADecisionRemainderInput(world);
            diagnostics?.EndPhase(BattleAiInputDetailPhase.InputEdges);
            if (!completePostSpecialMainDecision || diagnostics == null)
                return;

            diagnostics.RecordPhaseRngCalls(
                BattleAiInputDetailPhase.PostSpecialMainDecision,
                SimulationWorld.ResolveAiInputDetailRngCallDelta(
                    postSpecialRngCallsBefore,
                    world.Rng?.CallCount ?? 0));
            diagnostics.EndPhase(
                BattleAiInputDetailPhase.PostSpecialMainDecision);
        }

        private AiInputContext CreateAiInputContext(
            SimulationWorld world,
            LF2Entity self,
            int tickIndex)
        {
            int inputPhase = world.InputPhase;
            int difficulty = world.Difficulty;
            bool forceZero = world.AiPhaseGate == 1;
            if (!forceZero && inputPhase == 1 && world.Team(self) != 5)
                forceZero = world.Slot(self) < 20 || world.ObjectId(self) < 30;
            if (forceZero || difficulty < 0) difficulty = 0;
            AiInputContext ai = new AiInputContext
            {
                Difficulty = difficulty,
                Rand3 = difficulty * 3,
                Rand5 = difficulty * 5,
                Rand15 = difficulty * 15,
                Rand20 = difficulty * 20,
                InputPhase = inputPhase,
                StageTargetX = world.Runtime?.Stage?.XMaxOverride > 0 ? world.Runtime.Stage.XMaxOverride : (world.Runtime?.Stage?.StageWidthPx ?? 800),
            };
            world.AiUpdateMoveModeScan(self, ref ai);
            if (world.Runtime?.Flow != null)
            {
                world.Runtime.Flow.AiDifficulty = ai.Difficulty;
                world.Runtime.Flow.AiRand3 = ai.Rand3;
                world.Runtime.Flow.AiRand5 = ai.Rand5;
                world.Runtime.Flow.AiRand15 = ai.Rand15;
                world.Runtime.Flow.AiRand20 = ai.Rand20;
                world.Runtime.Flow.AiMoveMode = ai.MoveMode;
                world.Runtime.Flow.AiStageTargetX = ai.StageTargetX;
            }
            return ai;
        }

        private AiInputContext CreateCoordinateAiInputContext(SimulationWorld world)
        {
            BattleFlowRuntimeState flow = world.Runtime?.Flow;
            return new AiInputContext
            {
                Difficulty = flow?.AiDifficulty ?? 0,
                Rand3 = flow?.AiRand3 ?? 0,
                Rand5 = flow?.AiRand5 ?? 0,
                Rand15 = flow?.AiRand15 ?? 0,
                Rand20 = flow?.AiRand20 ?? 0,
                MoveMode = flow?.AiMoveMode ?? 0,
                StageTargetX = flow?.AiStageTargetX ?? (world.Runtime?.Stage?.StageWidthPx ?? 800),
                InputPhase = world.InputPhase,
            };
        }



        private void AiPostNoTargetFallback(SimulationWorld world, LF2Entity self, LF2Entity savedTarget, AiInputContext ai)
        {
            if (savedTarget != null)
            {
                bool close = !world.HasInputHistoryGate(self) || (Math.Abs(world.Z(self) - world.Z(savedTarget)) <= 150 && Math.Abs(world.X(self) - world.X(savedTarget)) <= 240);
                if (close && ai.MoveMode == 1) self.Runtime.KeyLeft = 1;
            }
            if ((world.ObjectId(self) == 7 && world.Frame(self) >= 255 && world.Frame(self) <= 261) ||
                (world.ObjectId(self) == 9 && world.Frame(self) >= 280 && world.Frame(self) <= 290) ||
                (world.ObjectId(self) == 32 && world.Frame(self) >= 240 && world.Frame(self) <= 245))
                self.Runtime.KeyAttack = 1;
        }

        private void RollAndClearAiKeys(SimulationWorld world, NTSDEntityRuntime input)
        {
            world.CharacterInputWriter.RollAndClearCurrentKeys(input);
        }

        private void MoveTowardCoordinate(SimulationWorld world, LF2Entity self, AiInputContext ai)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (input.Unk3FC <= -1000 || input.Unk400 <= -1000) return;
            if (world.X(self) > input.Unk3FC + 6)
            {
                input.KeyLeft = 1;
                if (world.X(self) > input.Unk3FC + 250 && world.Rand(ai.Rand3 + 3) == 0) input.PrevLeft = 0;
                if (world.X(self) < input.Unk3FC + 100 && world.State(self) == 2 && world.Facing(self) == 1) input.KeyRight = 1;
            }
            else if (world.X(self) < input.Unk3FC - 6)
            {
                input.KeyRight = 1;
                if (world.X(self) < input.Unk3FC - 250 && world.Rand(ai.Rand3 + 3) == 0) input.PrevRight = 0;
                if (world.X(self) > input.Unk3FC - 100 && world.State(self) == 2 && world.Facing(self) == 0) input.KeyLeft = 1;
            }
            if (world.Z(self) < input.Unk400 - 3) input.KeyDown = 1;
            else if (world.Z(self) > input.Unk400 + 3) input.KeyUp = 1;
            if (input.XBoundPositive || input.XBoundNegative) { input.PrevJump = 0; input.KeyJump = 1; }
            if (Math.Abs(input.Unk400 - world.Z(self)) <= 90 && Math.Abs(input.Unk3FC - world.X(self)) <= 90)
            { input.Unk3FC = -1000; input.Unk400 = -1000; }
        }

        private void MoveTowardTarget(SimulationWorld world, LF2Entity self, LF2Entity target, AiInputContext ai, int selfState)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (world.X(self) > world.X(target) + 6)
            {
                input.KeyLeft = 1;
                if (world.X(self) > world.X(target) + 250 && world.Rand(ai.Rand3 + 3) == 0) input.PrevLeft = 0;
                if (world.X(self) < world.X(target) + 100 && selfState == 2 && world.Facing(self) == 1) input.KeyRight = 1;
            }
            else if (world.X(self) < world.X(target) - 6)
            {
                if (ai.MoveMode == 0) input.KeyRight = 1;
                if (world.X(self) < world.X(target) - 250 && world.Rand(ai.Rand3 + 3) == 0 && ai.MoveMode == 0) input.PrevRight = 0;
                if (world.X(self) > world.X(target) - 100 && selfState == 2 && world.Facing(self) == 0) input.KeyLeft = 1;
            }
            if (world.Z(self) < world.Z(target) - 3) input.KeyDown = 1;
            else if (world.Z(self) > world.Z(target) + 3) input.KeyUp = 1;
        }

        private bool AiPostCacheCoordinateAllowsSpecial(SimulationWorld world, LF2Entity self)
        {
            NTSDEntityRuntime r = self.Runtime;
            if (r.Unk3FC <= -1000) return true;
            if (Math.Abs(r.Unk400 - world.Z(self)) > 90 || Math.Abs(r.Unk3FC - world.X(self)) > 90) return false;
            r.Unk3FC = -1000; r.Unk400 = -1000;
            return true;
        }

        private bool AiPreUpdateTarget3000SideEffect(SimulationWorld world, LF2Entity self, LF2Entity target, int selfState, int targetState, AiInputContext ai)
        {
            if (targetState != 3000) return false;
            bool randomGate = ai.Rand3 <= 0 || world.Rand(ai.Rand3) == 0;
            if (selfState != 7 && randomGate &&
                ((world.X(target) > world.X(self) && world.X(target) < world.X(self) + 200 && world.Vx(target) < 0.0) ||
                 (world.X(target) < world.X(self) && world.X(target) > world.X(self) - 200 && world.Vx(target) > 0.0)))
            { self.Runtime.PrevAttack = 0; self.Runtime.KeyAttack = 1; }
            if (world.X(target) > world.X(self) && world.Facing(self) == 1) self.Runtime.KeyRight = 1;
            if (world.X(target) < world.X(self) && world.Facing(self) == 0) self.Runtime.KeyLeft = 1;
            return true;
        }

        private bool AiUpdateFirstDecision(SimulationWorld world, LF2Entity self, LF2Entity target, int nearestTargetDist, bool specialObjectProximity)
        {
            int oid = world.ObjectId(self);
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (world.Rand(10) == 0 && world.Pp(self) > 85 &&
                ((world.Hp(self) < world.HpMax(self) - 70 && world.Hp(self) < 450) || (world.Hp(self) < (3 * world.HpMax(self)) / 5 && world.Hp(self) >= 140)))
            { self.Runtime.ComboDdj = 3; return true; }
            if (nearestTargetDist < 10000 && world.Rand(30) == 0 && world.Pp(self) > 250) { self.Runtime.ComboDua = 3; return true; }
            int targetOid = world.ObjectId(target);
            bool split = targetOid == 2 || targetOid == 9 || targetOid == 10 || targetOid == 11 || targetOid == 33 || targetOid == 34;
            int maxDx = split ? 500 : 250;
            int targetPpMin = split ? 220 : 170;
            if (world.Rand(15) == 0 && Math.Abs(world.X(target) - world.X(self)) > 100 && Math.Abs(world.X(target) - world.X(self)) < maxDx &&
                Math.Abs(world.Z(target) - world.Z(self)) < 30 && world.Pp(self) > 100 && world.Pp(target) > targetPpMin && !specialObjectProximity)
            { if (world.X(target) <= world.X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3; return true; }
            return false;
        }

        private bool AiUpdateTeammateGuardDecision(SimulationWorld world, LF2Entity self, AiInputContext ai, int nearestTargetDist, bool sameZLane)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (diagnostics == null)
            {
                return AiUpdateTeammateGuardDecisionCore(world,
                    self,
                    ai,
                    nearestTargetDist,
                    sameZLane,
                    null);
            }

            diagnostics.RecordPhaseCall(
                BattleAiInputDetailPhase.Teammate20Scan);
            ulong rngCallsBefore = world.Rng?.CallCount ?? 0;
            diagnostics.BeginPhase(BattleAiInputDetailPhase.Teammate20Scan);
            try
            {
                return AiUpdateTeammateGuardDecisionCore(world,
                    self,
                    ai,
                    nearestTargetDist,
                    sameZLane,
                    diagnostics);
            }
            finally
            {
                diagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.Teammate20Scan,
                    SimulationWorld.ResolveAiInputDetailRngCallDelta(
                        rngCallsBefore,
                        world.Rng?.CallCount ?? 0));
                diagnostics.EndPhase(BattleAiInputDetailPhase.Teammate20Scan);
            }
        }

        private bool AiUpdateTeammateGuardDecisionCore(SimulationWorld world, LF2Entity self,
            AiInputContext ai,
            int nearestTargetDist,
            bool sameZLane,
            BattleAiInputDetailDiagnostics diagnostics)
        {
            int oid = world.ObjectId(self);
            if (oid != 1 && oid != 2 && oid != 4 && oid != 5 && oid != 21) return false;
            if (self.Runtime.LinkState != 0 && world.Frame(self) >= 9) return false;
            bool hpWindow = (world.Hp(self) >= world.HpMax(self) - 70 || world.Hp(self) >= 140) &&
                            (world.Hp(self) >= (3 * world.HpMax(self)) / 5 || world.Hp(self) < 140);
            if (!hpWindow || sameZLane) return false;
            if (DecisionRemainderUseRowsForCurrentInput)
            {
                return AiUpdateTeammateGuardDecisionRows(world,
                    self,
                    nearestTargetDist,
                    diagnostics);
            }
            return AiUpdateTeammateGuardDecisionLegacy(world,
                self,
                nearestTargetDist,
                diagnostics,
                0);
        }

        private bool AiUpdateTeammateGuardDecisionLegacy(SimulationWorld world, LF2Entity self,
            int nearestTargetDist,
            BattleAiInputDetailDiagnostics diagnostics,
            int startSlot)
        {
            for (int i = startSlot; i < 20; i++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Teammate20Scan,
                    1);
                LF2Entity cand = world.AiAt(i);
                if (cand == null || cand == self || world.Team(cand) == 0 || world.Team(cand) != world.Team(self) ||
                    Math.Abs(world.X(cand) - world.X(self)) >= 250 || Math.Abs(world.Z(cand) - world.Z(self)) >= 60 || world.Pp(self) <= 350)
                    continue;
                bool lowHp = (world.Hp(cand) < world.HpMax(cand) - 90 && world.Hp(cand) < 140) ||
                             (world.Hp(cand) < (3 * world.HpMax(cand)) / 5 && world.Hp(cand) >= 140);
                if (!lowHp || world.Hp(cand) <= 0 || world.Distance(self, cand) >= nearestTargetDist / 3) continue;
                if (world.X(cand) > world.X(self) && world.Facing(self) == 1 && Math.Abs(world.X(cand) - world.X(self)) >= 5)
                { self.Runtime.KeyRight = 1; self.Runtime.KeyLeft = 0; return true; }
                if (world.X(cand) < world.X(self) && world.Facing(self) != 1 && Math.Abs(world.X(cand) - world.X(self)) >= 5)
                { self.Runtime.KeyRight = 0; self.Runtime.KeyLeft = 1; return true; }
                self.Runtime.ComboDuj = 3; return true;
            }
            return false;
        }

        private bool AiUpdateTeammateGuardDecisionRows(SimulationWorld world, LF2Entity self,
            int nearestTargetDist,
            BattleAiInputDetailDiagnostics diagnostics)
        {
            AiSoASensingRows rows = sensing.Rows;
            int selfSlot = world.Slot(self);
            int selfTeam = world.Team(self);
            int selfX = world.X(self);
            int selfZ = world.Z(self);
            int count = Math.Min(20, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Teammate20Scan,
                    1);
                if (!rows.Included[slot] ||
                    slot == selfSlot ||
                    rows.Team[slot] == 0 ||
                    rows.Team[slot] != selfTeam ||
                    Math.Abs(rows.X[slot] - selfX) >= 250 ||
                    Math.Abs(rows.Z[slot] - selfZ) >= 60 ||
                    world.Pp(self) <= 350)
                {
                    continue;
                }

                int hp = rows.Hp[slot];
                int hpMax = rows.HpMax[slot];
                bool lowHp = (hp < hpMax - 90 && hp < 140) ||
                             (hp < (3 * hpMax) / 5 && hp >= 140);
                int distance = Math.Abs(rows.X[slot] - selfX) +
                               Math.Abs(rows.Z[slot] - selfZ);
                if (!lowHp || hp <= 0 || distance >= nearestTargetDist / 3)
                    continue;

                int deltaX = rows.X[slot] - selfX;
                if (deltaX > 0 && world.Facing(self) == 1 && Math.Abs(deltaX) >= 5)
                {
                    self.Runtime.KeyRight = 1;
                    self.Runtime.KeyLeft = 0;
                    return true;
                }
                if (deltaX < 0 && world.Facing(self) != 1 && Math.Abs(deltaX) >= 5)
                {
                    self.Runtime.KeyRight = 0;
                    self.Runtime.KeyLeft = 1;
                    return true;
                }

                self.Runtime.ComboDuj = 3;
                return true;
            }

            return false;
        }

        private bool AiUpdateOid1ComboDecision(SimulationWorld world, LF2Entity self, LF2Entity target, int targetState)
        {
            int oid = world.ObjectId(self);
            if (oid != 1 && oid != 21 && oid != 17) return false;
            int dx = Math.Abs(world.X(target) - world.X(self));
            int dz = Math.Abs(world.Z(target) - world.Z(self));
            if (world.Frame(self) >= 260 && world.Frame(self) <= 289 && dx < 100 && dz < 7) return false;
            if (world.Rand(7) == 0 && dx < 150 && dz < 8 && world.Pp(self) > 150 &&
                ((world.Rand(10) == 0 && targetState != 3) || (world.Rand(3) > 0 && (targetState == 16 || targetState == 8 || targetState == 11))))
            { if (world.X(target) > world.X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            if (world.Rand(7) == 0 && dx < 100 && dz < 7 && world.Pp(self) > 75)
            {
                if (world.Pp(self) <= 150 || ((world.Rand(10) != 0 || targetState == 3) && (world.Rand(3) <= 0 || targetState != 16)))
                { self.Runtime.ComboDda = 3; return true; }
                if (world.X(target) <= world.X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3;
                return true;
            }
            return false;
        }

        private bool AiUpdateCloseOid1Decision(SimulationWorld world, LF2Entity self, LF2Entity target)
        {
            int oid = world.ObjectId(self);
            if (oid != 1 && oid != 21 && oid != 17) return false;
            if (world.Frame(self) < 260 || world.Frame(self) > 289 || Math.Abs(world.X(target) - world.X(self)) >= 100 || Math.Abs(world.Z(target) - world.Z(self)) >= 7) return false;
            if ((world.Y(target) == 0 && world.Y(self) == 0 && world.Rand(3) == 0) || (world.Y(target) < 0 && world.Y(self) < 0 && world.Rand(7) == 0))
            { self.Runtime.KeyJump = 1; self.Runtime.PrevJump = 0; return true; }
            if ((world.Y(target) >= 0 || world.Rand(5) != 0) && world.Rand(30) != 0) return true;
            bool targetRight = world.X(target) > world.X(self);
            bool targetLeft = world.X(target) < world.X(self);
            if ((targetRight && world.Facing(self) == 0) || (targetLeft && world.Facing(self) == 1)) self.Runtime.KeyDefend = 1;
            self.Runtime.PrevDefend = 0;
            return true;
        }

        private bool AiUpdateOid4ComboDecision(SimulationWorld world, LF2Entity self, LF2Entity target)
        {
            int oid = world.ObjectId(self);
            if (oid != 4 && oid != 10 && oid != 19) return false;
            int dx = Math.Abs(world.X(target) - world.X(self));
            int dz = Math.Abs(world.Z(target) - world.Z(self));
            if (world.Pp(self) > 360 && dx < 100 && dz < 70 && world.Rand(world.Hp(self) / 5 + 10) == 0)
            { self.Runtime.ComboDuj = 3; return true; }
            if (world.Rand(45) == 0 && dx > 100 && dx < 550 && dz < 20 && world.Pp(self) > 170)
            { if (world.X(target) <= world.X(self)) self.Runtime.ComboDlj = 3; else self.Runtime.ComboDrj = 3; return true; }
            if (world.Rand(30) == 0 && world.Pp(self) > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                bool facing = (world.Facing(self) == 0 && world.X(self) < world.X(target)) || (world.Facing(self) == 1 && world.X(self) > world.X(target));
                if (facing) { self.Runtime.ComboDja = 3; return true; }
            }
            return false;
        }

        private bool AiUpdateOid5ComboDecision(SimulationWorld world, LF2Entity self, LF2Entity target)
        {
            int oid = world.ObjectId(self);
            if (oid != 5 && oid != 19) return false;
            int dx = Math.Abs(world.X(target) - world.X(self));
            int dz = Math.Abs(world.Z(target) - world.Z(self));
            if (world.Pp(self) > 450 && dx > 100 && dz > 50 && world.Rand(3) == 0)
            { if (world.Rand(2) != 0) self.Runtime.ComboDdj = 3; else self.Runtime.ComboDuj = 3; return true; }
            if (world.Pp(self) > 70 && dx > 100 && dx < 160 && dz < 8 && world.Rand(10) == 0)
            { if (world.X(target) > world.X(self)) self.Runtime.ComboDrj = 3; else self.Runtime.ComboDlj = 3; return true; }
            if (world.Rand(30) == 0 && world.Pp(self) > 200 && dx > 100 && dx < 160 && dz < 55)
            {
                if (world.Facing(self) == 0 && world.X(self) < world.X(target)) { self.Runtime.ComboDra = 3; return true; }
                if (world.Facing(self) == 1 && world.X(self) > world.X(target)) { self.Runtime.ComboDla = 3; return true; }
            }
            return false;
        }

        private static bool AiProcessSubOidGroup(int oid) => oid <= 29 || oid == 33 || oid == 34;
        private static bool AiSpecialOidForSubGate(int oid) => oid == 18 || oid == 5 || oid == 31 || oid == 36;

        private void AiProcessSubHelper(SimulationWorld world, LF2Entity self, LF2Entity target, AiInputContext ai, int targetState, bool specialLeft, bool specialRight)
        {
            NTSDEntityRuntime input = self.Runtime;
            int oid = world.ObjectId(self);
            int predictedTargetX = world.X(target) + 2 * (int)world.Vx(target);
            if (world.Pp(self) < 150) input.ComboDja = 3;
            if (Math.Abs(world.X(target) - 2 * (int)world.Vx(self) - world.X(self)) < 80 && Math.Abs(world.Z(target) - world.Z(self)) < 5 &&
                world.Rand(ai.Rand3 + 3) == 0 && targetState != 14) input.KeyJump = 1;
            if ((specialLeft && world.X(target) > world.X(self)) || (specialRight && world.X(target) < world.X(self))) return;
            if (world.Rand(ai.Rand3 + 1) != 0) return;
            int predictedDelta = Math.Abs(predictedTargetX - world.X(self));
            if (AiProcessSubOidGroup(oid) && predictedDelta > 100 && predictedDelta < 900 && Math.Abs(world.Z(target) - world.Z(self)) < 5 &&
                world.Rand(ai.Rand3 + 10) == 0 && targetState != 14) input.KeyAttack = 1;
            bool facing = (world.Facing(self) == 0 && world.X(target) > world.X(self)) || (world.Facing(self) == 1 && world.X(target) < world.X(self));
            if (AiProcessSubOidGroup(oid) && predictedDelta > 90 && facing && (world.Frame(self) == 110 || world.Frame(self) >= 235) &&
                Math.Abs(world.Z(target) - world.Z(self)) < 13 && targetState != 14)
            {
                input.PrevRight = input.PrevLeft = input.PrevJump = 0;
                if (world.X(target) <= world.X(self)) input.KeyLeft = 1; else input.KeyRight = 1;
                if (oid != 34 || world.Rand(2) != 0) input.KeyJump = 1; else input.KeyDefend = 1;
            }
            if (oid == 1 && predictedDelta > 100 && predictedDelta < 300 && Math.Abs(world.Z(target) - world.Z(self)) < 5 &&
                world.Rand(ai.Rand5 + 10) == 0 && targetState != 14) input.KeyAttack = 1;
            if (oid == 1 && predictedDelta > 90 && facing && (world.Frame(self) == 110 || world.Frame(self) >= 235) &&
                Math.Abs(world.Z(target) - world.Z(self)) < 7 && targetState != 14)
            {
                input.PrevRight = input.PrevLeft = input.PrevJump = 0;
                if (world.X(target) <= world.X(self)) input.KeyLeft = 1; else input.KeyRight = 1;
                input.KeyJump = 1;
            }
        }

        private void AiProcessSubCallerPrewrite(SimulationWorld world, LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState)
        {
            NTSDEntityRuntime input = self.Runtime;
            bool specialOid = AiSpecialOidForSubGate(world.ObjectId(self));
            if (world.LinkState(self) == 0 && targetState == 16 && specialOid &&
                Math.Abs(world.X(target) - 2 * (int)input.Vx - world.X(self)) < 350 && Math.Abs(world.Z(target) - world.Z(self)) < 5 && world.Rand(ai.Rand3 + 3) == 0)
            {
                if ((world.X(target) > world.X(self) && world.Facing(self) == 0) || (world.X(target) <= world.X(self) && world.Facing(self) == 1)) input.KeyJump = 1;
            }
            if (world.LinkState(self) != 0 || targetState == 16 || !specialOid) return;
            bool closeTrigger = world.X(target) - world.X(self) < 100 && Math.Abs(world.Z(target) - world.Z(self)) < 80 && world.Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger && selfState != 7)
            {
                if (Math.Abs(world.X(target) - 2 * (int)input.Vx - world.X(self)) < 300 && Math.Abs(world.Z(target) - world.Z(self)) < 5 &&
                    world.Rand(ai.Rand3 + 3) == 0 && targetState != 14 &&
                    ((world.X(target) > world.X(self) && world.Facing(self) == 0) || (world.X(target) <= world.X(self) && world.Facing(self) == 1))) input.KeyJump = 1;
            }
            else if (selfState != 7)
            {
                bool closeWindow = !world.HasInputHistoryGate(self) || (Math.Abs(world.Z(self) - world.Z(target)) <= 150 && Math.Abs(world.X(self) - world.X(target)) <= 240);
                ApplyPressureRetreat(world, self, target, ai, closeWindow);
                if (closeWindow && world.Rand(17) == 0) input.KeyDefend = 1;
            }
        }

        private void AiProcessSubLabel435PressurePrewrite(SimulationWorld world, LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState)
        {
            NTSDEntityRuntime input = self.Runtime;
            bool specialOid = AiSpecialOidForSubGate(world.ObjectId(self));
            if (targetState != 16 && specialOid && world.LinkState(self) == 0) return;
            bool pressure = world.Hp(target) > world.Hp(self) * 2 || (world.Hp(self) <= 100 && world.Hp3(self) > 100);
            if (!pressure || ai.InputPhase != 1 || !world.IsCharacterDat(target) || world.Slot(self) < 20 || world.Team(self) == 5) return;
            bool closeTrigger = world.X(target) - world.X(self) < 100 && Math.Abs(world.Z(target) - world.Z(self)) < 80 && world.Rand(ai.Rand3 + 2) == 0;
            if (!closeTrigger || selfState == 7) return;
            bool closeWindow = !world.HasInputHistoryGate(self) || (Math.Abs(world.Z(self) - world.Z(target)) <= 150 && Math.Abs(world.X(self) - world.X(target)) <= 240);
            ApplyPressureRetreat(world, self, target, ai, closeWindow);
            if (closeWindow && world.Rand(17) == 0) input.KeyDefend = 1;
        }

        private void ApplyPressureRetreat(SimulationWorld world, LF2Entity self, LF2Entity target, AiInputContext ai, bool closeWindow)
        {
            if (!closeWindow) return;
            if ((world.X(target) < 250 || world.X(target) < world.X(self)) && world.X(target) <= ai.StageTargetX - 250)
            { self.Runtime.KeyRight = 1; self.Runtime.PrevRight = 0; }
            else if (world.X(target) > ai.StageTargetX - 250 || world.X(target) > world.X(self))
            { self.Runtime.KeyLeft = 1; self.Runtime.PrevLeft = 0; }
        }

        private bool AiProcessHelper(SimulationWorld world, LF2Entity self, LF2Entity target, AiInputContext ai, int selfState, int targetState, bool sameZLane, bool specialObjectProximity)
        {
            NTSDEntityRuntime input = self.Runtime;
            if (world.Rand(ai.Rand3 + 1) > 0) return false;
            int heldSlot = world.TargetSlot(self);
            if (heldSlot < 0 || heldSlot >= this.input.Slots.Length) return true;
            LF2Entity held = world.AiAt(heldSlot);
            int heldOid = held != null ? world.ObjectId(held) : -1;
            bool lineCover = false;
            BattleAiInputDetailDiagnostics heldScanDiagnostics =
                world.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            heldScanDiagnostics?.RecordPhaseCall(
                BattleAiInputDetailPhase.Held20Scan);
            ulong heldScanRngCallsBefore = heldScanDiagnostics != null
                ? world.Rng?.CallCount ?? 0
                : 0;
            heldScanDiagnostics?.BeginPhase(
                BattleAiInputDetailPhase.Held20Scan);
            if (DecisionRemainderUseRowsForCurrentInput)
            {
                lineCover = HasAiSoADecisionHeldLineCoverRows(
                    world,
                    self,
                    target,
                    heldScanDiagnostics);
            }
            else
            {
                lineCover = HasAiSoADecisionHeldLineCoverLegacy(
                    world,
                    self,
                    target,
                    heldScanDiagnostics,
                    0,
                    false);
            }
            heldScanDiagnostics?.EndPhase(
                BattleAiInputDetailPhase.Held20Scan);
            if (heldScanDiagnostics != null)
            {
                heldScanDiagnostics.RecordPhaseRngCalls(
                    BattleAiInputDetailPhase.Held20Scan,
                    SimulationWorld.ResolveAiInputDetailRngCallDelta(
                        heldScanRngCallsBefore,
                        world.Rng?.CallCount ?? 0));
            }
            if (selfState == 2 && world.Rand(ai.Rand3 + 5) == 0)
            { if (lineCover) input.KeyDefend = 1; else input.KeyJump = 1; }

            int vxTwice = 2 * (int)world.Vx(self);
            if (heldOid == 100 || heldOid == 101 || heldOid == 120 || heldOid == 121 || heldOid == 124)
            {
                if (Math.Abs(world.X(target) - vxTwice - world.X(self)) < 10000 && Math.Abs(world.Z(target) - world.Z(self)) < 6 && world.Rand(ai.Rand3 + 3) == 0 && targetState != 14)
                    input.KeyJump = 1;
                if (heldOid == 124 && world.Rand(ai.Rand15 + 30) == 0) input.KeyJump = 1;
                if (world.Rand(ai.Rand3 + 5) == 0)
                {
                    bool close = !world.HasInputHistoryGate(self) || (Math.Abs(world.Z(self) - world.Z(target)) <= 150 && Math.Abs(world.X(self) - world.X(target)) <= 240);
                    if (close && Math.Abs(world.X(target) - world.X(self)) < 600 && Math.Abs(world.Z(target) - world.Z(self)) < 20)
                    {
                        if (world.X(target) > world.X(self) && ai.MoveMode == 0) { input.KeyRight = 1; input.PrevRight = 0; }
                        if (world.X(target) < world.X(self)) { input.KeyLeft = 1; input.PrevLeft = 0; }
                    }
                }
            }
            if ((heldOid == 150 || heldOid == 151) && !lineCover && Math.Abs(world.X(target) - vxTwice - world.X(self)) < 5000 &&
                Math.Abs(world.Z(target) - world.Z(self)) < 10 && world.Rand(ai.Rand5 + 7) == 0 && targetState != 14) input.KeyJump = 1;
            if (heldOid != 122 && heldOid != 123) return true;

            world.CharacterInputWriter.ClearCurrentKeys(input);
            if (selfState == 17 && sameZLane && !specialObjectProximity && world.HitStop(self) != 0)
            { input.KeyAttack = 1; return false; }
            if (world.HasInputHistoryGate(self) && (Math.Abs(world.Z(self) - world.Z(target)) > 150 || Math.Abs(world.X(self) - world.X(target)) > 240)) return false;
            if (world.Z(target) < world.StageZMin + 30) input.KeyDown = 1;
            else if (world.Z(target) < world.StageZMax - 30) input.KeyUp = 1;
            else if (world.Z(target) > world.Z(self)) input.KeyUp = 1;
            else input.KeyDown = 1;

            if (world.X(target) < 400 && world.X(self) < 200)
            {
                input.KeyRight = 1;
                if (world.Rand(ai.Rand3 + 7) == 0) input.PrevRight = 0;
                if (world.Rand(ai.Rand3 + 5) == 0 && selfState == 2) input.KeyDefend = 1;
                return false;
            }
            if (world.X(target) > ai.StageTargetX - 400 && world.X(self) > ai.StageTargetX - 200)
            {
                input.KeyLeft = 1;
                if (world.Rand(ai.Rand3 + 7) == 0) input.PrevLeft = 0;
                if (world.Rand(ai.Rand3 + 5) == 0 && selfState == 2) input.KeyDefend = 1;
                return false;
            }
            if (Math.Abs(world.X(target) - world.X(self)) < 350 && Math.Abs(world.Z(target) - world.Z(self)) < 70)
            {
                if (world.X(target) > world.X(self)) { input.KeyLeft = 1; if (world.Rand(ai.Rand3 + 4) == 0) input.PrevLeft = 0; }
                if (world.X(target) <= world.X(self)) { input.KeyRight = 1; if (world.Rand(ai.Rand3 + 4) == 0) input.PrevRight = 0; }
                return false;
            }
            if (selfState == 2)
            { if (world.Facing(self) == 0) input.KeyLeft = 1; if (world.Facing(self) == 1) input.KeyRight = 1; return false; }
            if (world.Rand(5) != 0) return false;
            if (specialObjectProximity || (world.ObjectId(self) != 2 && world.ObjectId(self) != 34) || world.Pp(self) <= 150 || world.Rand(ai.Rand3 + 3) <= 0)
            { input.KeyJump = 1; return false; }
            if (world.X(target) > world.X(self)) input.ComboDrj = 3; else input.ComboDlj = 3;
            return true;
        }



        private bool HasAiSoADecisionHeldLineCoverRows(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity target,
            BattleAiInputDetailDiagnostics diagnostics)
        {
            AiSoASensingRows rows = sensing.Rows;
            int selfSlot = world.Slot(self);
            int selfTeam = world.Team(self);
            int selfX = world.X(self);
            int selfZ = world.Z(self);
            int targetX = world.X(target);
            int targetTeam = world.Team(target);
            bool lineCover = false;
            int count = Math.Min(20, rows.Capacity);
            for (int slot = 0; slot < count; slot++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Held20Scan,
                    1);
                if (!rows.Included[slot] ||
                    slot == selfSlot ||
                    rows.Team[slot] == 0 ||
                    targetTeam != selfTeam ||
                    rows.Hp[slot] <= 0 ||
                    rows.State[slot] == 14 ||
                    Math.Abs(rows.Y[slot]) > 2)
                {
                    continue;
                }

                int candidateX = rows.X[slot];
                if (Math.Abs(rows.Z[slot] - selfZ) < 15 &&
                    ((selfX < candidateX && candidateX < targetX) ||
                     (targetX < candidateX && candidateX < selfX)))
                {
                    lineCover = true;
                }
            }
            return lineCover;
        }

        private bool HasAiSoADecisionHeldLineCoverLegacy(
            SimulationWorld world,
            LF2Entity self,
            LF2Entity target,
            BattleAiInputDetailDiagnostics diagnostics,
            int startSlot,
            bool lineCover)
        {
            for (int slot = startSlot; slot < 20; slot++)
            {
                diagnostics?.RecordPhaseSlotVisits(
                    BattleAiInputDetailPhase.Held20Scan,
                    1);
                LF2Entity candidate = world.AiAt(slot);
                if (candidate == null ||
                    candidate == self ||
                    world.Team(candidate) == 0 ||
                    world.Team(target) != world.Team(self) ||
                    world.Hp(candidate) <= 0 ||
                    world.State(candidate) == 14 ||
                    Math.Abs(world.Y(candidate)) > 2)
                {
                    continue;
                }

                if (Math.Abs(world.Z(candidate) - world.Z(self)) < 15 &&
                    ((world.X(self) < world.X(candidate) &&
                      world.X(candidate) < world.X(target)) ||
                     (world.X(target) < world.X(candidate) &&
                      world.X(candidate) < world.X(self))))
                {
                    lineCover = true;
                }
            }
            return lineCover;
        }

        internal void PrepareAiInputBasic(
            SimulationWorld world,
            LF2Entity self,
            int tickIndex)
        {
            if (ExecutionMode == AiDecisionExecutionMode.IndexedCanonical)
            {
                if (!world.TryPrepareAiDecisionIndexedCanonicalForModule(
                        self,
                        tickIndex))
                {
                    PrepareAiInputBasicLegacyCore(world, self, tickIndex);
                }
                return;
            }

            if (ShadowMode == AiDecisionShadowMode.Disabled)
            {
                PrepareAiInputBasicLegacyCore(world, self, tickIndex);
                return;
            }

            bool comparisonStarted =
                world.BeginAiDecisionShadowComparisonForModule(self, tickIndex);
            try
            {
                PrepareAiInputBasicLegacyCore(world, self, tickIndex);
            }
            finally
            {
                world.CompleteAiDecisionShadowComparisonForModule(
                    comparisonStarted);
            }
        }

        internal bool BeginShadowComparison(
            SimulationWorld world,
            LF2Entity self,
            int tickIndex)
        {
#if UNITY_INCLUDE_TESTS
            ShadowBeginInvocationCountForTests++;
#endif
            if (ShadowMode != AiDecisionShadowMode.Shadow &&
                ShadowMode != AiDecisionShadowMode.SharedShadow)
            {
                return false;
            }

            ShadowEligibleCount++;
            ShadowComparisonActive = false;
            LegacyRngRecording = false;
            ShadowSelf = null;
            ComparisonSnapshot = null;

            AiDecisionSnapshot snapshot;
            AiDecisionAvailability captureAvailability;
            if (ShadowMode == AiDecisionShadowMode.SharedShadow)
            {
                if (!SharedPassAvailable || SharedSnapshot == null)
                {
                    RecordShadowUnavailable(
                        SharedPassUnavailableReason == AiDecisionAvailability.None
                            ? AiDecisionAvailability.SnapshotMissing
                            : SharedPassUnavailableReason);
                    return false;
                }

                snapshot = SharedSnapshot;
                captureAvailability =
                    world.CaptureAiDecisionSharedOwnedSnapshotForModule(
                        self,
                        snapshot);
            }
            else
            {
                int capacity = runtimeSlots.LogicalCapacity;
                if (capacity <= 0)
                {
                    RecordShadowUnavailable(AiDecisionAvailability.SnapshotMissing);
                    return false;
                }
                if (ShadowSnapshot == null ||
                    ShadowSnapshot.Rows.Capacity != capacity)
                {
                    ShadowSnapshot = new AiDecisionSnapshot(capacity);
                }

                snapshot = ShadowSnapshot;
                captureAvailability =
                    world.CaptureAiDecisionShadowSnapshotForModule(
                        self,
                        snapshot);
            }
            if (captureAvailability != AiDecisionAvailability.Available)
            {
                RecordShadowUnavailable(captureAvailability);
                return false;
            }

            AiDecisionWitness expected = default;
            try
            {
#if UNITY_INCLUDE_TESTS
                ThrowShadowExceptionForSelfCheck(
                    AiDecisionShadowExceptionStage.KernelEvaluate);
#endif
                bool fullAvailable = AiDecisionKernel.TryEvaluate(
                    snapshot,
                    AiDecisionEvaluationPolicy.FullScan,
                    ref expected);
                if (ShadowMode == AiDecisionShadowMode.SharedShadow)
                    EvaluateIndexedShadow(snapshot, expected, fullAvailable);
                if (!fullAvailable)
                {
                    RecordShadowUnavailable(expected.Availability);
                    return false;
                }
            }
            catch (Exception exception)
            {
                world.RecordAiDecisionShadowExceptionForModule(
                    AiDecisionShadowExceptionStage.KernelEvaluate,
                    exception);
                if (ShadowMode == AiDecisionShadowMode.SharedShadow)
                {
                    world.InvalidateAiDecisionSharedPassForModule(
                        AiDecisionAvailability.SnapshotMissing);
                }
                RecordShadowUnavailable(AiDecisionAvailability.SnapshotMissing);
                return false;
            }

            ComparisonSnapshot = snapshot;
            ShadowExpected = expected;
            ShadowSelf = self;
            ShadowComparisonActive = true;
            ShadowAvailableCount++;
            ShadowCloneRngCallCount += expected.RngDrawCount;
            ShadowRowVisitCount += expected.RowVisits;
            LegacyRngCount = 0;
            LegacyRngOverflow = false;
            LegacyRngOrderHash = 1469598103934665603UL;
            LegacyCharacterDecisionPosition = 0;
            LegacyRngRecording = true;
            return true;
        }

        internal void CompleteShadowComparison(
            SimulationWorld world,
            bool comparisonStarted)
        {
#if UNITY_INCLUDE_TESTS
            ShadowCompleteInvocationCountForTests++;
#endif
            LegacyRngRecording = false;
            if (!comparisonStarted || !ShadowComparisonActive)
                return;

            ShadowComparisonActive = false;
            LF2Entity self = ShadowSelf;
            ShadowSelf = null;
            if (self?.Runtime == null ||
                runtimeSlots.OccupancyEpoch != ShadowExpected.OccupancyEpoch ||
                !runtimeSlots.TryGetCurrentHandle(
                    ShadowExpected.SelfSlot,
                    self,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != ShadowExpected.SelfGeneration ||
                self.Runtime.StableId != ShadowExpected.SelfStableId)
            {
                ComparisonSnapshot = null;
                RecordShadowUnavailable(AiDecisionAvailability.EpochMismatch);
                return;
            }

            ShadowComparedCount++;
            AiDecisionShadowMismatchReason reason =
                world.CompareAiDecisionShadowResultForModule(self);
            ComparisonSnapshot = null;
            if (reason == AiDecisionShadowMismatchReason.None)
                return;

            ShadowMismatchCount++;
            if (ShadowFirstMismatchReason == AiDecisionShadowMismatchReason.None)
                ShadowFirstMismatchReason = reason;
        }

        internal void RecordShadowUnavailable(AiDecisionAvailability reason)
        {
            ShadowUnavailableCount++;
            if (ShadowFirstUnavailableReason == AiDecisionAvailability.None)
                ShadowFirstUnavailableReason = reason;
        }

        internal void PrepareSharedPass(SimulationWorld world)
        {
            SharedBuildCount++;
            SharedPassAvailable = false;
            SharedPassUnavailableReason = AiDecisionAvailability.None;
            SharedPassEpoch = runtimeSlots.OccupancyEpoch;
            AiDecisionShadowExceptionStage exceptionStage =
                AiDecisionShadowExceptionStage.SharedBuild;
            try
            {
#if UNITY_INCLUDE_TESTS
                ThrowShadowExceptionForSelfCheck(exceptionStage);
#endif
                int capacity = runtimeSlots.LogicalCapacity;
                if (capacity <= 0)
                {
                    world.InvalidateAiDecisionSharedPassForModule(
                        AiDecisionAvailability.SnapshotMissing);
                    return;
                }

                PrepareCapacity(capacity);
                SharedSnapshot.ResetSharedRows(SharedPassEpoch);
                AiDecisionAvailability buildAvailability =
                    CaptureSharedRows(world, capacity, SharedPassEpoch);
                if (buildAvailability != AiDecisionAvailability.Available)
                {
                    world.InvalidateAiDecisionSharedPassForModule(
                        buildAvailability);
                    return;
                }

                SharedRows.SpecialIndexReady = true;
                SimulationAiSensingModule.BuildRoleIndexes(SharedRows);
                SimulationAiSensingModule.BuildTeamSummaries(SharedRows);
                exceptionStage = AiDecisionShadowExceptionStage.SharedPreflight;
#if UNITY_INCLUDE_TESTS
                ThrowShadowExceptionForSelfCheck(exceptionStage);
                ApplySharedPreflightMutationForSelfCheck();
#endif
                AiDecisionAvailability preflightAvailability =
                    ValidateSharedPassPreflight(
                        world,
                        capacity,
                        SharedPassEpoch);
                if (preflightAvailability != AiDecisionAvailability.Available)
                {
                    world.InvalidateAiDecisionSharedPassForModule(
                        preflightAvailability);
                    return;
                }

                BeginProductionMutationWitnessPass(
                    AiUnifiedSnapshotConsumer.IndexedDecision,
                    SharedPassEpoch);
                SharedPassAvailable = true;
            }
            catch (Exception exception)
            {
                world.RecordAiDecisionShadowExceptionForModule(
                    exceptionStage,
                    exception);
                world.InvalidateAiDecisionSharedPassForModule(
                    AiDecisionAvailability.SnapshotMissing);
            }
        }

        internal void EndSharedPass()
        {
            SharedPassAvailable = false;
            SharedPassEpoch = 0;
            ComparisonSnapshot = null;
        }

        internal void InvalidateSharedPass(AiDecisionAvailability reason)
        {
            SharedPassAvailable = false;
            if (SharedPassUnavailableReason == AiDecisionAvailability.None)
            {
                SharedPassUnavailableReason = reason == AiDecisionAvailability.None
                    ? AiDecisionAvailability.SnapshotMissing
                    : reason;
            }
        }

        internal void RecordShadowException(
            AiDecisionShadowExceptionStage stage,
            Exception exception)
        {
            if (stage == AiDecisionShadowExceptionStage.None ||
                exception == null ||
                ShadowFirstExceptionStage != AiDecisionShadowExceptionStage.None)
            {
                return;
            }

            ShadowFirstExceptionStage = stage;
            ShadowFirstExceptionType = exception.GetType();
        }

        internal void ResetDecisionDiagnostics()
        {
            ShadowEligibleCount = 0;
            ShadowAvailableCount = 0;
            ShadowUnavailableCount = 0;
            ShadowComparedCount = 0;
            ShadowMismatchCount = 0;
            ShadowCloneRngCallCount = 0;
            ShadowRowVisitCount = 0;
            SharedBuildCount = 0;
            SharedRefreshCount = 0;
            IndexedEligibleCount = 0;
            IndexedAvailableCount = 0;
            IndexedUnavailableCount = 0;
            IndexedComparedCount = 0;
            IndexedMismatchCount = 0;
            IndexedFullRowVisitCount = 0;
            IndexedRowVisitCount = 0;
            ShadowFirstUnavailableReason = AiDecisionAvailability.None;
            ShadowFirstMismatchReason = AiDecisionShadowMismatchReason.None;
            ShadowFirstExceptionStage = AiDecisionShadowExceptionStage.None;
            IndexedFirstMismatchReason = AiDecisionIndexedMismatchReason.None;
            ShadowFirstExceptionType = null;
            ShadowExpected = default;
            SharedPassAvailable = false;
            SharedPassUnavailableReason = AiDecisionAvailability.None;
            ComparisonSnapshot = null;
#if UNITY_INCLUDE_TESTS
            ShadowBeginInvocationCountForTests = 0;
            ShadowCompleteInvocationCountForTests = 0;
#endif
        }

        private static AiUnifiedSnapshotMutationWitness CreateMutationWitness(
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

        internal static AiDecisionIndexedMismatchReason CompareIndexedWitnesses(
            AiDecisionSnapshot fullSnapshot,
            AiDecisionWitness full,
            bool fullAvailable,
            AiDecisionSnapshot indexedSnapshot,
            AiDecisionWitness indexed,
            bool indexedAvailable)
        {
            if (fullAvailable != indexedAvailable ||
                full.Availability != indexed.Availability)
            {
                return AiDecisionIndexedMismatchReason.Availability;
            }
            if (full.Exit != indexed.Exit)
                return AiDecisionIndexedMismatchReason.Exit;
            if (full.CharacterDecisionPosition != indexed.CharacterDecisionPosition)
                return AiDecisionIndexedMismatchReason.CharacterDecisionPosition;
            if (!InputEquals(full.Input, indexed.Input))
                return AiDecisionIndexedMismatchReason.Input;
            if (!WorldEquals(full.World, indexed.World))
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

            int traceCount = Math.Min(
                full.RngDrawCount,
                fullSnapshot.RngTraceModuli.Length);
            for (int index = 0; index < traceCount; index++)
            {
                if (fullSnapshot.RngTraceModuli[index] !=
                        indexedSnapshot.RngTraceModuli[index] ||
                    fullSnapshot.RngTraceRaw[index] !=
                        indexedSnapshot.RngTraceRaw[index] ||
                    fullSnapshot.RngTraceValues[index] !=
                        indexedSnapshot.RngTraceValues[index])
                {
                    return AiDecisionIndexedMismatchReason.RngTrace;
                }
            }
            return AiDecisionIndexedMismatchReason.None;
        }

        internal static bool WorldEquals(
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

        internal static bool InputEquals(
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
