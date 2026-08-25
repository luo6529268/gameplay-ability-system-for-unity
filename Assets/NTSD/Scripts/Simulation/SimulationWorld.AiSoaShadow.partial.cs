using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public enum AiSensingMode
    {
        LegacyAiSensing = 0,
        SoAShadowAiSensing = 1,
        SoAAiSensing = 2,
    }

    public enum AiSoASensingShadowMismatchKind
    {
        None = 0,
        ShadowPurity = 1,
        InitialNearest = 2,
        CachedSelection = 3,
        PostSpecialSelection = 4,
    }

    public struct AiSoASensingShadowMismatch
    {
        public AiSoASensingShadowMismatchKind Kind;
        public int SelfSlot;
        public int ExpectedSelection;
        public int ActualSelection;
        public int ExpectedValue;
        public int ActualValue;
        public int ExpectedFlags;
        public int ActualFlags;
    }

    public partial class SimulationWorld
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

        private AiSensingMode aiSensingMode;
        private AiSoASensingRows aiSoASensingRows;
        private ulong aiSoASensingSnapshotEpoch;
        private bool aiSoASensingSnapshotValid;
        private bool aiSoASensingPassInvalidated;
        private bool aiSoASensingComparisonPending;
        private AiSoASensingResult aiSoASensingExpected;
        private int aiSoASensingPendingMismatchMask;
        private AiSoASensingShadowMismatch aiSoASensingPendingFirstMismatch;
        private BattleAiExecutionProfile aiExecutionProfile =
            BattleAiExecutionProfile.LegacyCanonical;
        private AiDecisionOwnedInputMode aiDecisionOwnedInputMode =
            AiDecisionOwnedInputMode.SnapshotCopy;
        private bool aiSoACandidateExecutionEnabled;
        private bool aiSoACandidatePassLatchedToLegacy;
        private bool aiSoACandidateForceNearestFailureForSelfCheck;
        private bool aiSoACandidateForceSpecialFailureForSelfCheck;
        private bool aiSoADecisionRemainderEnabledForSelfCheck;
        private bool aiSoADecisionRemainderUseRowsForCurrentInput;
        private bool aiSoADecisionRemainderAttemptedForCurrentInput;
        private bool aiSoADecisionRemainderRandomBoundaryPassed;
        private bool aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck;
        private bool aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck;
        private int aiSoADecisionRemainderMutationKindForSelfCheck;
        private bool aiSoADecisionRemainderMutationAfterRandomForSelfCheck;
        private bool aiSoADecisionRemainderHardFailureRecordedForCurrentInput;
        private AiDecisionRowContext aiSoADecisionRowContext;

        public AiSensingMode AiSensingMode
        {
            get => aiSensingMode;
            set
            {
                if (_ticking)
                {
                    throw new InvalidOperationException(
                        "AI sensing mode cannot be changed while a simulation pass is running.");
                }
                if (aiUnifiedSnapshotExecutionMode ==
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority must be disabled before changing AI sensing mode.");
                }

                switch (value)
                {
                    case AiSensingMode.LegacyAiSensing:
                    case AiSensingMode.SoAShadowAiSensing:
                        aiSensingMode = value;
                        return;
                    case AiSensingMode.SoAAiSensing:
                        throw new NotSupportedException(
                            "SoAAiSensing is unavailable in AI sensing shadow v1.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public BattleAiExecutionProfile AiExecutionProfile => aiExecutionProfile;
        public AiDecisionOwnedInputMode AiDecisionOwnedInputModeForDiagnostics =>
            aiDecisionOwnedInputMode;

        public void ConfigureAiDecisionOwnedInputModeForDiagnostics(
            AiDecisionOwnedInputMode mode)
        {
            if (_ticking || ObjectCount != 0 || ClaimedRuntimeSlotCountForServices != 0)
            {
                throw new InvalidOperationException(
                    "The AI owned-input mode must be configured before entities are registered.");
            }
            if (mode != AiDecisionOwnedInputMode.SnapshotCopy &&
                mode != AiDecisionOwnedInputMode.CanonicalStoreDirect)
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            aiDecisionOwnedInputMode = mode;
        }

        public void ConfigureAiExecutionProfile(BattleAiExecutionProfile profile)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "The battle AI execution profile cannot change while a simulation pass is running.");
            }
            if (ObjectCount != 0 || ClaimedRuntimeSlotCountForServices != 0)
            {
                throw new InvalidOperationException(
                    "The battle AI execution profile must be configured before entities are registered.");
            }
            if (profile != BattleAiExecutionProfile.LegacyCanonical &&
                profile != BattleAiExecutionProfile.DataOrientedCanonical)
            {
                throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }

            // Leave authority first. This is the only ordering that can never expose a
            // partially configured authority pass to the existing property guards.
            if (aiUnifiedSnapshotExecutionMode !=
                AiUnifiedSnapshotExecutionMode.LegacySeparate)
            {
                AiUnifiedSnapshotExecutionMode =
                    AiUnifiedSnapshotExecutionMode.LegacySeparate;
            }

            AiDecisionShadowMode = AiDecisionShadowMode.Disabled;
            AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Disabled;
            AiDecisionIndexedCanonicalFullOracleSampleInterval = 0;

            switch (profile)
            {
                case BattleAiExecutionProfile.LegacyCanonical:
                    SetAiSoACandidateExecutionEnabled(false);
                    AiDecisionExecutionMode = AiDecisionExecutionMode.Legacy;
                    break;
                case BattleAiExecutionProfile.DataOrientedCanonical:
                    SetAiSoACandidateExecutionEnabled(true);
                    AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
                    AiUnifiedSnapshotExecutionMode =
                        AiUnifiedSnapshotExecutionMode.UnifiedAuthority;
                    break;
            }

            aiExecutionProfile = profile;
            EnsureAiExecutionProfileCoherent();
        }

        private void EnsureAiExecutionProfileCoherent()
        {
            bool coherent = aiExecutionProfile == BattleAiExecutionProfile.LegacyCanonical
                ? aiSensingMode == AiSensingMode.LegacyAiSensing &&
                  aiDecisionExecutionMode == AiDecisionExecutionMode.Legacy &&
                  aiUnifiedSnapshotExecutionMode ==
                  AiUnifiedSnapshotExecutionMode.LegacySeparate
                : aiSensingMode == AiSensingMode.SoAAiSensing &&
                  aiDecisionExecutionMode == AiDecisionExecutionMode.IndexedCanonical &&
                  aiUnifiedSnapshotExecutionMode ==
                  AiUnifiedSnapshotExecutionMode.UnifiedAuthority;
            if (!coherent)
            {
                throw new InvalidOperationException(
                    "The battle AI execution profile did not produce a coherent sensing/decision/authority configuration.");
            }
        }

        public int AiSoASensingShadowQueryCountForDiagnostics { get; private set; }
        public int AiSoASensingShadowInvalidationCountForDiagnostics { get; private set; }
        public int AiSoASensingShadowPurityMismatchCountForDiagnostics { get; private set; }
        public int AiSoASensingShadowInitialMismatchCountForDiagnostics { get; private set; }
        public int AiSoASensingShadowCachedMismatchCountForDiagnostics { get; private set; }
        public int AiSoASensingShadowPostSpecialMismatchCountForDiagnostics { get; private set; }
        public int AiSoASensingShadowMismatchMaskForDiagnostics { get; private set; }
        public int AiSoASensingShadowLastMismatchMaskForDiagnostics { get; private set; }
        public bool AiSoASensingShadowComparisonPublishedForDiagnostics { get; private set; }
        public AiSoASensingShadowMismatch AiSoASensingShadowFirstMismatchForDiagnostics { get; private set; }
        public int AiSoACandidateNearestQueryCountForDiagnostics { get; private set; }
        public int AiSoACandidateSpecialQueryCountForDiagnostics { get; private set; }
        public int AiSoACandidateEmptySpecialFastPathCountForDiagnostics { get; private set; }
        public long AiSoACandidateGroundXRowVisitCountForDiagnostics { get; private set; }
        public long AiSoACandidateAirXRowVisitCountForDiagnostics { get; private set; }
        public long AiSoACandidateSpecialSlotVisitCountForDiagnostics { get; private set; }
        public int AiSoACandidateLegacyNearestScanCountForDiagnostics { get; private set; }
        public int AiSoACandidateLegacySpecialScanCountForDiagnostics { get; private set; }
        public int AiSoACandidatePreRandomFailureCountForDiagnostics { get; private set; }
        public int AiSoACandidatePostRandomFailureCountForDiagnostics { get; private set; }
        public int AiSoACandidateFusedSnapshotBuildCountForDiagnostics { get; private set; }
        public long AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics { get; private set; }
        public int AiSoACandidateFusedSnapshotFailureCountForDiagnostics { get; private set; }
        public long AiSoACandidateSnapshotRefreshCountForDiagnostics { get; private set; }
        public int AiLegacyNearestFactsBuildCountForDiagnostics { get; private set; }
        public int AiLegacySnapshotIndexBuildCountForDiagnostics { get; private set; }
        public int AiLegacyQuadtreeSyncCountForDiagnostics { get; private set; }
        public int AiLegacySnapshotMutationCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderEligibleAttemptCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderAppliedCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderFallbackCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderPreRandomFailureCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderPostRandomFailureCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderHardFailureCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderContextBindCountForDiagnostics { get; private set; }
        public int AiSoADecisionRemainderGatewayValidationCountForDiagnostics { get; private set; }
        public long AiSoADecisionRemainderRowVisitCountForDiagnostics { get; private set; }
        public bool AiSoADecisionRemainderEnabledForDiagnostics =>
            aiSoADecisionRemainderEnabledForSelfCheck;

        public void ResetAiSoACandidateDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "AI sensing diagnostics cannot be reset while a simulation pass is running.");
            }

            AiSoACandidateNearestQueryCountForDiagnostics = 0;
            AiSoACandidateSpecialQueryCountForDiagnostics = 0;
            AiSoACandidateEmptySpecialFastPathCountForDiagnostics = 0;
            AiSoACandidateGroundXRowVisitCountForDiagnostics = 0;
            AiSoACandidateAirXRowVisitCountForDiagnostics = 0;
            AiSoACandidateSpecialSlotVisitCountForDiagnostics = 0;
            AiSoACandidateLegacyNearestScanCountForDiagnostics = 0;
            AiSoACandidateLegacySpecialScanCountForDiagnostics = 0;
            AiSoACandidatePreRandomFailureCountForDiagnostics = 0;
            AiSoACandidatePostRandomFailureCountForDiagnostics = 0;
            AiSoACandidateFusedSnapshotBuildCountForDiagnostics = 0;
            AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics = 0;
            AiSoACandidateFusedSnapshotFailureCountForDiagnostics = 0;
            AiSoACandidateSnapshotRefreshCountForDiagnostics = 0;
            AiLegacyNearestFactsBuildCountForDiagnostics = 0;
            AiLegacySnapshotIndexBuildCountForDiagnostics = 0;
            AiLegacyQuadtreeSyncCountForDiagnostics = 0;
            AiLegacySnapshotMutationCountForDiagnostics = 0;
            AiSoADecisionRemainderEligibleAttemptCountForDiagnostics = 0;
            AiSoADecisionRemainderAppliedCountForDiagnostics = 0;
            AiSoADecisionRemainderFallbackCountForDiagnostics = 0;
            AiSoADecisionRemainderPreRandomFailureCountForDiagnostics = 0;
            AiSoADecisionRemainderPostRandomFailureCountForDiagnostics = 0;
            AiSoADecisionRemainderHardFailureCountForDiagnostics = 0;
            AiSoADecisionRemainderContextBindCountForDiagnostics = 0;
            AiSoADecisionRemainderGatewayValidationCountForDiagnostics = 0;
            AiSoADecisionRemainderRowVisitCountForDiagnostics = 0;
        }

        internal void SetAiSoACandidateModeForSelfCheck(bool enabled)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            if (!enabled &&
                aiUnifiedSnapshotExecutionMode ==
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                throw new InvalidOperationException(
                    "Unified AI snapshot authority must be disabled before SoAAiSensing.");
            }
            SetAiSoACandidateExecutionEnabled(enabled);
        }

        private void SetAiSoACandidateExecutionEnabled(bool enabled)
        {
            aiSoACandidateExecutionEnabled = enabled;
            aiSensingMode = enabled
                ? AiSensingMode.SoAAiSensing
                : AiSensingMode.LegacyAiSensing;
            aiSoACandidatePassLatchedToLegacy = false;
            if (!enabled)
            {
                aiSoACandidateForceNearestFailureForSelfCheck = false;
                aiSoACandidateForceSpecialFailureForSelfCheck = false;
                aiSoADecisionRemainderEnabledForSelfCheck = false;
                aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderMutationKindForSelfCheck = 0;
                aiSoADecisionRemainderMutationAfterRandomForSelfCheck = false;
            }
        }

        internal void SetAiSoADecisionRemainderModeForSelfCheck(bool enabled)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            if (enabled && aiSensingMode != AiSensingMode.SoAAiSensing)
            {
                throw new InvalidOperationException(
                    "AI SoA decision remainder requires SoAAiSensing authority.");
            }

            aiSoADecisionRemainderEnabledForSelfCheck = enabled;
            aiSoADecisionRemainderUseRowsForCurrentInput = false;
            aiSoADecisionRemainderAttemptedForCurrentInput = false;
            aiSoADecisionRemainderRandomBoundaryPassed = false;
            aiSoADecisionRemainderHardFailureRecordedForCurrentInput = false;
            aiSoADecisionRowContext = default;
            if (!enabled)
            {
                aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderMutationKindForSelfCheck = 0;
                aiSoADecisionRemainderMutationAfterRandomForSelfCheck = false;
            }
        }

        internal void SetAiSoADecisionRemainderFailureForSelfCheck(
            bool failBeforeRandom,
            bool failAfterRandom)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck =
                failBeforeRandom;
            aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck =
                failAfterRandom;
        }

        internal void SetAiSoADecisionRemainderMutationForSelfCheck(
            int mutationKind,
            bool afterRandom)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            if (mutationKind < 0 || mutationKind > 4)
                throw new ArgumentOutOfRangeException(nameof(mutationKind));

            aiSoADecisionRemainderMutationKindForSelfCheck = mutationKind;
            aiSoADecisionRemainderMutationAfterRandomForSelfCheck = afterRandom;
        }

        internal void SetAiSoACandidateFailureForSelfCheck(
            bool failNearest,
            bool failSpecial)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            aiSoACandidateForceNearestFailureForSelfCheck = failNearest;
            aiSoACandidateForceSpecialFailureForSelfCheck = failSpecial;
        }

        public void ResetAiSoASensingShadowDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "AI sensing diagnostics cannot be reset while a simulation pass is running.");
            }

            AiSoASensingShadowQueryCountForDiagnostics = 0;
            AiSoASensingShadowInvalidationCountForDiagnostics = 0;
            AiSoASensingShadowPurityMismatchCountForDiagnostics = 0;
            AiSoASensingShadowInitialMismatchCountForDiagnostics = 0;
            AiSoASensingShadowCachedMismatchCountForDiagnostics = 0;
            AiSoASensingShadowPostSpecialMismatchCountForDiagnostics = 0;
            AiSoASensingShadowMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowComparisonPublishedForDiagnostics = false;
            AiSoASensingShadowFirstMismatchForDiagnostics = default;
        }

        private sealed class AiSoASensingRows : AiSensingSnapshot
        {
            internal AiSoASensingRows(int capacity)
                : base(capacity)
            {
            }

            internal AiSoASensingRows GrowTo(int capacity)
            {
                var grown = new AiSoASensingRows(capacity);
                CopyTo(grown);
                return grown;
            }
        }

        private struct AiDecisionRowIdentity
        {
            public LF2Entity Entity;
            public int Slot;
            public uint Generation;
            public int StableId;
            public bool Included;
        }

        private struct AiDecisionRowContext
        {
            public AiSoASensingRows Rows;
            public LF2Entity[] Slots;
            public ulong OccupancyEpoch;
            public AiDecisionRowIdentity Self;
            public AiDecisionRowIdentity Selected;
            public AiDecisionRowIdentity Cached;
            public bool Bound;
        }

        private struct AiSoASensingResult
        {
            public int TickIndex;
            public int SelfSlot;
            public uint SelfGeneration;
            public int SelfIdentity;
            public int InitialSelectedSlot;
            public int InitialBestDist;
            public bool InitialSameZLane;
            public bool CachedTargetEligible;
            public bool CacheRandomExpected;
            public int CacheRoll;
            public uint CacheRngStateBefore;
            public uint CacheRngStateAfter;
            public ulong CacheRngCallsBefore;
            public ulong CacheRngCallsAfter;
            public int CachedSelectedSlot;
            public int PostSpecialSelectedSlot;
            public int SpecialBestDist;
            public int SpecialFlags;
        }

        private struct AiSoANearestResult
        {
            public int SelectedSlot;
            public int BestDist;
            public bool SameZLane;
            public ulong SnapshotEpoch;
            public uint SelectedGeneration;
            public int SelectedIdentity;
        }

        private struct AiSoASpecialResult
        {
            public int SelectedSlot;
            public int BestDist;
            public bool SameZLane;
            public ulong SnapshotEpoch;
            public uint SelectedGeneration;
            public int SelectedIdentity;
            public int Flags;
        }

        private void InitializeAiSoASensingRows(int capacity)
        {
            aiSoASensingRows = new AiSoASensingRows(capacity);
        }

        private void GrowAiSoASensingRows(int capacity)
        {
            if (aiSoASensingRows == null)
            {
                InitializeAiSoASensingRows(capacity);
            }
            else if (capacity > aiSoASensingRows.Capacity)
            {
                aiSoASensingRows = aiSoASensingRows.GrowTo(capacity);
            }

            if (aiSoASensingSnapshotValid)
                InvalidateAiSoASensingShadowPass();
        }

        private void EnsureAiSensingModeAvailableBeforeTick()
        {
            if (aiUnifiedSnapshotExecutionMode ==
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                if (aiSensingMode != AiSensingMode.SoAAiSensing ||
                    aiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical ||
                    aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Disabled)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority requires SoAAiSensing, IndexedCanonical, and disabled unified shadow.");
                }
            }
            if (aiSensingMode == AiSensingMode.SoAAiSensing)
            {
                if (aiSoACandidateExecutionEnabled)
                    return;

                throw new NotSupportedException(
                    "SoAAiSensing requires the data-oriented production profile or an internal diagnostic test gate.");
            }

            if (aiSensingMode != AiSensingMode.LegacyAiSensing &&
                aiSensingMode != AiSensingMode.SoAShadowAiSensing)
            {
                throw new InvalidOperationException("Unknown AI sensing mode.");
            }
        }

        private void CaptureAiSoASensingShadowSnapshot(ulong expectedEpoch)
        {
            aiSoASensingSnapshotValid = false;
            aiSoASensingPassInvalidated = false;
            aiSoACandidatePassLatchedToLegacy = false;
            aiSoASensingComparisonPending = false;
            aiSoASensingPendingMismatchMask = 0;
            aiSoASensingPendingFirstMismatch = default;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowComparisonPublishedForDiagnostics = false;
            aiSoASensingSnapshotEpoch = expectedEpoch;
            aiSoASensingRows.CapturedOccupancyEpoch = expectedEpoch;
            Array.Clear(aiSoASensingRows.Included, 0, aiSoASensingRows.Capacity);
            Array.Clear(aiSoASensingRows.SpecialScanMember, 0, aiSoASensingRows.Capacity);
            aiSoASensingRows.SpecialSlotCount = 0;
            aiSoASensingRows.SpecialIndexReady = false;
            aiSoASensingRows.GroundRoleSlotCount = 0;
            aiSoASensingRows.AirRoleSlotCount = 0;
            aiSoASensingRows.GroundRoleTeamSummaryCount = 0;
            aiSoASensingRows.AirRoleTeamSummaryCount = 0;
            aiSoASensingRows.RoleIndexesReady = false;
            aiSoASensingRows.TeamSummaryCount = 0;
            aiSoASensingRows.TeamSummariesReady = false;

            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                LF2Entity entity = aiInputSlots[slot];
                if (entity == null)
                    continue;

                if (!TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                    !TryCaptureAiSoASensingRow(entity, slot, handle.Generation, true))
                {
                    InvalidateAiSoASensingShadowPass();
                    return;
                }
            }

            if (RuntimeSlotOccupancyEpochForServices != expectedEpoch)
            {
                InvalidateAiSoASensingShadowPass();
                return;
            }

            aiSoASensingRows.SpecialIndexReady = true;
            BuildAiSoASensingRoleIndexes(aiSoASensingRows);
            BuildAiSoASensingTeamSummaries(aiSoASensingRows);
            BeginAiUnifiedSnapshotProductionMutationWitnessPass(
                AiUnifiedSnapshotConsumer.SoASensing,
                aiSoASensingSnapshotEpoch);
            aiSoASensingSnapshotValid = true;
        }

        private bool CaptureAiSoACandidateFusedSnapshot(
            int expectedCapacity,
            ulong expectedEpoch)
        {
            AiSoACandidateFusedSnapshotBuildCountForDiagnostics++;
            aiSoASensingSnapshotValid = false;
            aiSoASensingPassInvalidated = false;
            aiSoACandidatePassLatchedToLegacy = false;
            aiSoASensingComparisonPending = false;
            aiSoASensingPendingMismatchMask = 0;
            aiSoASensingPendingFirstMismatch = default;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowComparisonPublishedForDiagnostics = false;
            aiSoASensingSnapshotEpoch = expectedEpoch;
            if (aiSoASensingRows != null)
                aiSoASensingRows.CapturedOccupancyEpoch = expectedEpoch;

            LF2Entity[] slots = aiInputSlots;
            AiSoASensingRows rows = aiSoASensingRows;
            bool capacityProven =
                expectedCapacity > 0 &&
                slots != null &&
                rows != null &&
                expectedCapacity == slots.Length &&
                expectedCapacity == rows.Capacity;
            bool soaProven = capacityProven;
            bool moveModeFirst10Proven = capacityProven;

            if (slots != null)
                Array.Clear(slots, 0, slots.Length);
            ResetAiMoveModeFirst10Snapshot();
            if (rows != null)
            {
                Array.Clear(rows.Included, 0, rows.Capacity);
                Array.Clear(rows.SpecialScanMember, 0, rows.Capacity);
                Array.Clear(rows.SpecialSlots, 0, rows.Capacity);
                rows.SpecialSlotCount = 0;
                rows.SpecialIndexReady = false;
                rows.GroundRoleSlotCount = 0;
                rows.AirRoleSlotCount = 0;
                rows.GroundRoleTeamSummaryCount = 0;
                rows.AirRoleTeamSummaryCount = 0;
                rows.RoleIndexesReady = false;
                rows.TeamSummaryCount = 0;
                rows.TeamSummariesReady = false;
            }

            for (int slot = 0; slot < expectedCapacity; slot++)
            {
                AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics++;

                if (!TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    view.RuntimeSlot != slot)
                {
                    soaProven = false;
                    if (slot < aiMoveModeFirst10Present.Length)
                        moveModeFirst10Proven = false;
                    continue;
                }

                if (!view.Claimed)
                {
                    // Released slots may retain a non-zero generation, but never an
                    // entity reference. A ghost occupant is structural corruption:
                    // fail the SoA proof while continuing the fixed-capacity scan so
                    // every other valid claimed slot remains available to fallback.
                    if (view.Entity != null)
                        soaProven = false;
                    continue;
                }

                LF2Entity entity = view.Entity;
                NTSDEntityRuntime runtime = entity?.Runtime;
                if (view.Generation == 0 ||
                    entity == null ||
                    runtime == null ||
                    runtime.SlotIndex != slot)
                {
                    soaProven = false;
                    if (slot < aiMoveModeFirst10Present.Length)
                        moveModeFirst10Proven = false;
                    continue;
                }

                // Dormant, pending-destroy, and queued-unregister occupants are valid
                // slot-table entries that are simply inactive for this pass.
                if (!IsActiveForCurrentPass(entity))
                    continue;

                if (slots == null || slot >= slots.Length || rows == null)
                {
                    soaProven = false;
                    if (slot < aiMoveModeFirst10Present.Length)
                        moveModeFirst10Proven = false;
                    continue;
                }

                slots[slot] = entity;
                if (slot < aiMoveModeFirst10Present.Length)
                {
                    CaptureAiMoveModeFirst10Candidate(
                        slot,
                        entity,
                        true,
                        new RuntimeEntityHandle(slot, view.Generation));
                }

                if (!TryCaptureAiSoASensingRow(
                        rows,
                        entity,
                        slot,
                        view.Generation,
                        true))
                {
                    soaProven = false;
                }
            }

            bool finalStructureProven =
                RuntimeSlotCapacity == expectedCapacity &&
                ReferenceEquals(aiInputSlots, slots) &&
                slots != null &&
                slots.Length == expectedCapacity &&
                ReferenceEquals(aiSoASensingRows, rows) &&
                rows != null &&
                rows.Capacity == expectedCapacity;
            bool epochProven =
                RuntimeSlotOccupancyEpochForServices == expectedEpoch;
            aiMoveModeFirst10Valid =
                moveModeFirst10Proven &&
                finalStructureProven &&
                epochProven;

            if (!soaProven || !finalStructureProven || !epochProven)
            {
                AiSoACandidateFusedSnapshotFailureCountForDiagnostics++;
                InvalidateAiSoASensingShadowPass();
                return false;
            }

            rows.SpecialIndexReady = true;
            BuildAiSoASensingRoleIndexes(rows);
            BuildAiSoASensingTeamSummaries(rows);
            BeginAiUnifiedSnapshotProductionMutationWitnessPass(
                AiUnifiedSnapshotConsumer.SoASensing,
                aiSoASensingSnapshotEpoch);
            aiSoASensingSnapshotValid = true;
            return true;
        }

        private static void BuildAiSoASensingRoleIndexes(AiSoASensingRows rows)
        {
            int groundCount = 0;
            int airCount = 0;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (IsGroundAiSoARoleMember(rows, slot))
                    rows.GroundRoleSlotsByX[groundCount++] = slot;
                if (IsAirAiSoARoleMember(rows, slot))
                    rows.AirRoleSlotsByX[airCount++] = slot;
            }

            rows.GroundRoleSlotCount = groundCount;
            rows.AirRoleSlotCount = airCount;
            if (groundCount > 1)
            {
                SortAiSoARoleSlotsByTeamThenXThenSlot(
                    rows,
                    rows.GroundRoleSlotsByX,
                    0,
                    groundCount - 1);
            }
            if (airCount > 1)
            {
                SortAiSoARoleSlotsByTeamThenXThenSlot(
                    rows,
                    rows.AirRoleSlotsByX,
                    0,
                    airCount - 1);
            }

            rows.GroundRoleTeamSummaryCount = BuildAiSoARoleTeamSpans(
                rows,
                rows.GroundRoleSlotsByX,
                groundCount,
                rows.GroundRoleTeamSummaries);
            rows.AirRoleTeamSummaryCount = BuildAiSoARoleTeamSpans(
                rows,
                rows.AirRoleSlotsByX,
                airCount,
                rows.AirRoleTeamSummaries);
            rows.RoleIndexesReady = true;
        }

        private static int BuildAiSoARoleTeamSpans(
            AiSoASensingRows rows,
            int[] slots,
            int slotCount,
            AiSensingRoleTeamSummary[] summaries)
        {
            int summaryCount = 0;
            int index = 0;
            while (index < slotCount)
            {
                int start = index;
                int team = rows.Team[slots[index++]];
                while (index < slotCount && rows.Team[slots[index]] == team)
                    index++;

                summaries[summaryCount++] = new AiSensingRoleTeamSummary
                {
                    Team = team,
                    Start = start,
                    Count = index - start,
                };
            }

            return summaryCount;
        }

        private static void SortAiSoARoleSlotsByTeamThenXThenSlot(
            AiSoASensingRows rows,
            int[] slots,
            int left,
            int right)
        {
            while (left < right)
            {
                int lower = left;
                int upper = right;
                int pivotSlot = slots[left + ((right - left) >> 1)];
                while (lower <= upper)
                {
                    while (CompareAiSoARoleSlots(
                               rows,
                               slots[lower],
                               pivotSlot) < 0)
                        lower++;
                    while (CompareAiSoARoleSlots(
                               rows,
                               slots[upper],
                               pivotSlot) > 0)
                        upper--;

                    if (lower > upper)
                        continue;

                    int swap = slots[lower];
                    slots[lower++] = slots[upper];
                    slots[upper--] = swap;
                }

                // Recurse into the smaller partition so the call stack remains O(log N)
                // even when the captured slots arrive in a pathological order.
                if (upper - left < right - lower)
                {
                    if (left < upper)
                    {
                        SortAiSoARoleSlotsByTeamThenXThenSlot(
                            rows,
                            slots,
                            left,
                            upper);
                    }
                    left = lower;
                }
                else
                {
                    if (lower < right)
                    {
                        SortAiSoARoleSlotsByTeamThenXThenSlot(
                            rows,
                            slots,
                            lower,
                            right);
                    }
                    right = upper;
                }
            }
        }

        private static int CompareAiSoARoleSlots(
            AiSoASensingRows rows,
            int firstSlot,
            int secondSlot)
        {
            int teamComparison = rows.Team[firstSlot].CompareTo(rows.Team[secondSlot]);
            if (teamComparison != 0)
                return teamComparison;

            int xComparison = rows.X[firstSlot].CompareTo(rows.X[secondSlot]);
            return xComparison != 0 ? xComparison : firstSlot.CompareTo(secondSlot);
        }

        private static void BuildAiSoASensingTeamSummaries(AiSoASensingRows rows)
        {
            rows.TeamSummaryCount = 0;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!IsLivingCharacterAiSoARow(rows, slot))
                    continue;

                int summaryIndex = FindAiSoATeamSummaryIndex(rows, rows.Team[slot]);
                if (summaryIndex < 0)
                {
                    summaryIndex = rows.TeamSummaryCount++;
                    rows.TeamSummaries[summaryIndex] = new AiSensingTeamSummary
                    {
                        Team = rows.Team[slot],
                        MinHp = int.MaxValue,
                        SecondMinHp = int.MaxValue,
                    };
                }

                AiSensingTeamSummary summary = rows.TeamSummaries[summaryIndex];
                int hp = rows.Hp[slot];
                summary.Count++;
                if (hp < summary.MinHp)
                {
                    summary.SecondMinHp = summary.MinHp;
                    summary.MinHp = hp;
                    summary.MinCount = 1;
                }
                else if (hp == summary.MinHp)
                {
                    summary.MinCount++;
                }
                else if (hp < summary.SecondMinHp)
                {
                    summary.SecondMinHp = hp;
                }

                rows.TeamSummaries[summaryIndex] = summary;
            }
            rows.TeamSummariesReady = true;
        }

        private static int FindAiSoATeamSummaryIndex(AiSoASensingRows rows, int team)
        {
            for (int index = 0; index < rows.TeamSummaryCount; index++)
            {
                if (rows.TeamSummaries[index].Team == team)
                    return index;
            }

            return -1;
        }

        private bool TryCaptureAiSoASensingRow(
            LF2Entity entity,
            int slot,
            uint generation,
            bool captureSpecialMembership)
        {
            return TryCaptureAiSoASensingRow(
                aiSoASensingRows,
                entity,
                slot,
                generation,
                captureSpecialMembership);
        }

        private static bool TryCaptureAiSoASensingRow(
            AiSoASensingRows rows,
            LF2Entity entity,
            int slot,
            uint generation,
            bool captureSpecialMembership,
            bool useFreshRuntimeIdentity = false)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            if (rows == null ||
                runtime == null ||
                generation == 0 ||
                slot < 0 ||
                slot >= rows.Capacity ||
                runtime.SlotIndex != slot)
            {
                return false;
            }

            int objectId = useFreshRuntimeIdentity
                ? runtime.ObjectId
                : entity.ObjectId;
            int dataObjectType = useFreshRuntimeIdentity
                ? runtime.EntityType
                : entity.GetCurrentDataObjectTypeForSimulation();

            rows.Included[slot] = true;
            if (captureSpecialMembership)
            {
                bool specialScanMember =
                    slot >= 20 && IsAiSpecialScanObjectId(objectId);
                rows.SpecialScanMember[slot] = specialScanMember;
                if (specialScanMember)
                    rows.SpecialSlots[rows.SpecialSlotCount++] = slot;
            }
            int[] inputHistory = runtime.InputHistory;
            rows.InputHistoryGate[slot] =
                inputHistory != null &&
                inputHistory.Length == 6 &&
                inputHistory[0] != 0;
            rows.Generation[slot] = generation;
            rows.Identity[slot] = runtime.StableId;
            rows.ObjectId[slot] = objectId;
            rows.DataObjectType[slot] = dataObjectType;
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
            rows.HitJ[slot] = CaptureAiCurrentFrameHitJ(entity, runtime.Frame);
            rows.LinkState[slot] = runtime.LinkState;
            rows.KillCount[slot] = runtime.KillCount;
            rows.CachedTargetSlot[slot] = runtime.Unk360;
            rows.CoordinateTargetX[slot] = runtime.Unk3FC;
            rows.Vx[slot] = runtime.Vx;
            rows.Facing[slot] = runtime.Dir == "left" ? 1 : 0;
            rows.TargetSlot[slot] = runtime.TargetSlotIndex;
            rows.HitStop[slot] = runtime.HitStop;
            rows.BoundaryFlags[slot] = CaptureAiSoASensingBoundaryFlags(runtime);
            return true;
        }

        private static int CaptureAiCurrentFrameHitJ(
            LF2Entity entity,
            int currentFrame)
        {
            return entity?.GetFrameDataById(currentFrame)?.hit_j ?? 0;
        }

        private static int CaptureAiSoASensingBoundaryFlags(NTSDEntityRuntime runtime)
        {
            return (runtime.ZBoundNegative ? 1 : 0) |
                   (runtime.ZBoundPositive ? 1 << 1 : 0) |
                   (runtime.XBoundNegative ? 1 << 2 : 0) |
                   (runtime.XBoundPositive ? 1 << 3 : 0);
        }

        private void ObserveAiSoASensingSnapshotBuildEpoch(
            ulong expectedEpoch,
            ulong observedEpoch)
        {
            if (aiSoASensingSnapshotValid &&
                (expectedEpoch != aiSoASensingSnapshotEpoch || observedEpoch != expectedEpoch))
            {
                InvalidateAiSoASensingShadowPass();
            }
        }

        private void ClearAiSoASensingShadowSnapshot()
        {
            aiSoASensingSnapshotValid = false;
            aiSoASensingPassInvalidated = false;
            aiSoASensingComparisonPending = false;
            aiSoASensingPendingMismatchMask = 0;
            aiSoASensingPendingFirstMismatch = default;
            aiSoASensingSnapshotEpoch = 0;
            if (aiSoASensingRows != null)
                aiSoASensingRows.CapturedOccupancyEpoch = 0;
            aiSoACandidatePassLatchedToLegacy = false;
        }

        private void InvalidateAiSoASensingShadowPass()
        {
            aiSoASensingSnapshotValid = false;
            aiSoASensingComparisonPending = false;
            aiSoASensingPendingMismatchMask = 0;
            aiSoASensingPendingFirstMismatch = default;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowComparisonPublishedForDiagnostics = false;
            if (aiSoASensingPassInvalidated)
                return;

            aiSoASensingPassInvalidated = true;
            AiSoASensingShadowInvalidationCountForDiagnostics++;
        }

        private bool ValidateAiSoASensingShadowSnapshot()
        {
            if (!aiSoASensingSnapshotValid || aiSoASensingPassInvalidated)
                return false;

            if (RuntimeSlotOccupancyEpochForServices != aiSoASensingSnapshotEpoch)
            {
                InvalidateAiSoASensingShadowPass();
                return false;
            }

            AiSoASensingRows rows = aiSoASensingRows;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!rows.Included[slot])
                    continue;

                if (!TryGetRuntimeSlotReadOnlyView(
                        slot,
                        out RuntimeSlotTable.ReadOnlySlotView view) ||
                    !view.Claimed ||
                    view.Generation != rows.Generation[slot] ||
                    view.Entity?.Runtime == null ||
                    view.Entity.Runtime.SlotIndex != slot ||
                    view.Entity.Runtime.StableId != rows.Identity[slot])
                {
                    InvalidateAiSoASensingShadowPass();
                    return false;
                }
            }

            return true;
        }

        private void RefreshAiSoASensingShadowRowAfterCharacterInput(LF2Entity entity)
        {
            if (!aiSoASensingSnapshotValid || aiSoASensingPassInvalidated)
                return;

            if (RuntimeSlotOccupancyEpochForServices != aiSoASensingSnapshotEpoch ||
                entity?.Runtime == null)
            {
                InvalidateAiSoASensingShadowPass();
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            AiSoASensingRows rows = aiSoASensingRows;
            if (slot < 0 ||
                slot >= rows.Capacity ||
                !rows.Included[slot] ||
                !TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                handle.Generation != rows.Generation[slot] ||
                entity.Runtime.StableId != rows.Identity[slot])
            {
                InvalidateAiSoASensingShadowPass();
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
            if (!TryCaptureAiSoASensingRow(entity, slot, handle.Generation, false))
            {
                InvalidateAiSoASensingShadowPass();
                return;
            }

            bool currentSpecialMember =
                slot >= 20 && IsAiSpecialScanObjectId(rows.ObjectId[slot]);
            if (previousObjectId != rows.ObjectId[slot] ||
                previousSpecialMember != currentSpecialMember)
            {
                InvalidateAiSoASensingShadowPass();
                return;
            }

            bool isGroundRole = IsGroundAiSoARoleMember(rows, slot);
            bool isAirRole = IsAirAiSoARoleMember(rows, slot);
            bool groundRoleChanged = wasGroundRole != isGroundRole;
            bool airRoleChanged = wasAirRole != isAirRole;
            bool roleRebuilt = previousX != rows.X[slot] ||
                               previousTeam != rows.Team[slot] ||
                               groundRoleChanged ||
                               airRoleChanged;
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
                AiUnifiedSnapshotConsumer.SoASensing,
                aiSoASensingSnapshotEpoch,
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
            if (aiSensingMode == AiSensingMode.SoAAiSensing)
                AiSoACandidateSnapshotRefreshCountForDiagnostics++;
        }

        private void BeginAiSoASensingShadowComparison(LF2Entity self, int tickIndex)
        {
            aiSoASensingComparisonPending = false;
            aiSoASensingPendingMismatchMask = 0;
            aiSoASensingPendingFirstMismatch = default;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowComparisonPublishedForDiagnostics = false;
            if (self?.Runtime == null ||
                self.Runtime.HP <= 0 ||
                self.Runtime.Unk3FC > -1000 ||
                !ValidateAiSoASensingShadowSnapshot())
            {
                return;
            }

            int selfSlot = self.Runtime.SlotIndex;
            if (selfSlot < 0 ||
                selfSlot >= aiSoASensingRows.Capacity ||
                !aiSoASensingRows.Included[selfSlot] ||
                aiSoASensingRows.Identity[selfSlot] != self.Runtime.StableId)
            {
                InvalidateAiSoASensingShadowPass();
                return;
            }

            if (!TryGetCurrentRuntimeHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle selfHandle) ||
                selfHandle.Generation != aiSoASensingRows.Generation[selfSlot])
            {
                InvalidateAiSoASensingShadowPass();
                return;
            }

            uint rngStateBefore = Rng?.State ?? 0;
            ulong rngCallsBefore = Rng?.CallCount ?? 0;
            ulong inputSignatureBefore = CaptureAiNearestInputSignature(self.Runtime);
            bool succeeded = TryQueryAiSoANearest(
                selfSlot,
                InputPhase,
                out AiSoANearestResult nearest);
            uint rngStateAfter = Rng?.State ?? 0;
            ulong rngCallsAfter = Rng?.CallCount ?? 0;
            ulong inputSignatureAfter = CaptureAiNearestInputSignature(self.Runtime);

            if (rngStateBefore != rngStateAfter ||
                rngCallsBefore != rngCallsAfter ||
                inputSignatureBefore != inputSignatureAfter)
            {
                RecordAiSoASensingPendingMismatch(
                    AiSoASensingShadowMismatchKind.ShadowPurity,
                    selfSlot,
                    unchecked((int)rngStateBefore),
                    unchecked((int)rngStateAfter),
                    unchecked((int)rngCallsBefore),
                    unchecked((int)rngCallsAfter),
                    unchecked((int)inputSignatureBefore),
                    unchecked((int)inputSignatureAfter));
            }

            if (!succeeded)
                return;

            aiSoASensingExpected = default;
            aiSoASensingExpected.SelfSlot = selfSlot;
            aiSoASensingExpected.InitialSelectedSlot = nearest.SelectedSlot;
            aiSoASensingExpected.InitialBestDist = nearest.BestDist;
            aiSoASensingExpected.InitialSameZLane = nearest.SameZLane;
            aiSoASensingExpected.CacheRngStateBefore = rngStateBefore;
            aiSoASensingExpected.CacheRngCallsBefore = rngCallsBefore;
            int savedTargetSlot = aiSoASensingRows.CachedTargetSlot[selfSlot];
            bool cachedTargetEligible =
                IsLivingCharacterAiSoARow(aiSoASensingRows, savedTargetSlot);
            aiSoASensingExpected.CachedTargetEligible = cachedTargetEligible;
            aiSoASensingExpected.CacheRandomExpected = cachedTargetEligible;
            uint predictedRngState = rngStateBefore;
            ulong predictedRngCalls = rngCallsBefore;
            int predictedCacheRoll = 0;
            int predictedCachedSelection = nearest.SelectedSlot;
            if (cachedTargetEligible)
            {
                predictedCacheRoll =
                    NextAiSoALocalRandom(ref predictedRngState) % 30;
                predictedRngCalls++;
                if (predictedCacheRoll > 0)
                    predictedCachedSelection = savedTargetSlot;
            }
            aiSoASensingExpected.CacheRoll = predictedCacheRoll;
            aiSoASensingExpected.CacheRngStateAfter = predictedRngState;
            aiSoASensingExpected.CacheRngCallsAfter = predictedRngCalls;
            aiSoASensingExpected.CachedSelectedSlot = predictedCachedSelection;
            aiSoASensingExpected.PostSpecialSelectedSlot = predictedCachedSelection;
            aiSoASensingExpected.SpecialBestDist = 10000;
            aiSoASensingExpected.TickIndex = tickIndex;
            aiSoASensingExpected.SelfGeneration = selfHandle.Generation;
            aiSoASensingExpected.SelfIdentity = self.Runtime.StableId;
            AiSoASensingShadowQueryCountForDiagnostics++;
            aiSoASensingComparisonPending = true;
        }

        private void ContinueAiSoASensingShadowComparisonAfterCache(
            LF2Entity self,
            int tickIndex,
            bool cachedTargetEligible,
            bool cacheRandomCalled,
            int cacheRoll,
            uint rngStateBefore,
            ulong rngCallsBefore,
            uint rngStateAfter,
            ulong rngCallsAfter,
            int selectedSlot)
        {
            if (!IsAiSoASensingComparisonCurrent(self, tickIndex))
                return;

            bool cacheMismatch =
                cachedTargetEligible != aiSoASensingExpected.CachedTargetEligible ||
                cacheRandomCalled != aiSoASensingExpected.CacheRandomExpected ||
                (cacheRandomCalled && cacheRoll != aiSoASensingExpected.CacheRoll) ||
                rngStateBefore != aiSoASensingExpected.CacheRngStateBefore ||
                rngCallsBefore != aiSoASensingExpected.CacheRngCallsBefore ||
                rngStateAfter != aiSoASensingExpected.CacheRngStateAfter ||
                rngCallsAfter != aiSoASensingExpected.CacheRngCallsAfter ||
                selectedSlot != aiSoASensingExpected.CachedSelectedSlot;
            if (cacheMismatch)
            {
                RecordAiSoASensingPendingMismatch(
                    AiSoASensingShadowMismatchKind.CachedSelection,
                    aiSoASensingExpected.SelfSlot,
                    aiSoASensingExpected.CachedSelectedSlot,
                    selectedSlot,
                    aiSoASensingExpected.CacheRoll,
                    cacheRoll,
                    PackAiSoACacheFlags(
                        aiSoASensingExpected.CachedTargetEligible,
                        aiSoASensingExpected.CacheRandomExpected),
                    PackAiSoACacheFlags(cachedTargetEligible, cacheRandomCalled));
            }

            int expectedSelectedSlot = aiSoASensingExpected.CachedSelectedSlot;
            if (expectedSelectedSlot < 0)
                return;

            if (!TryQueryAiSoASpecial(
                    aiSoASensingExpected.SelfSlot,
                    InputPhase,
                    expectedSelectedSlot,
                    aiSoASensingExpected.InitialBestDist,
                    aiSoASensingExpected.InitialSameZLane,
                    ForceFullAiSpecialScanForDiagnostics,
                    out AiSoASpecialResult special))
            {
                InvalidateAiSoASensingShadowPass();
                return;
            }

            aiSoASensingExpected.PostSpecialSelectedSlot = special.SelectedSlot;
            aiSoASensingExpected.SpecialBestDist = special.BestDist;
            aiSoASensingExpected.SpecialFlags = special.Flags;
        }

        private void CompareAiSoASensingInitial(
            LF2Entity self,
            int tickIndex,
            int selectedSlot,
            int bestDist,
            bool sameZLane)
        {
            if (!IsAiSoASensingComparisonCurrent(self, tickIndex))
                return;

            if (selectedSlot == aiSoASensingExpected.InitialSelectedSlot &&
                bestDist == aiSoASensingExpected.InitialBestDist &&
                sameZLane == aiSoASensingExpected.InitialSameZLane)
            {
                return;
            }

            RecordAiSoASensingPendingMismatch(
                AiSoASensingShadowMismatchKind.InitialNearest,
                aiSoASensingExpected.SelfSlot,
                aiSoASensingExpected.InitialSelectedSlot,
                selectedSlot,
                aiSoASensingExpected.InitialBestDist,
                bestDist,
                aiSoASensingExpected.InitialSameZLane ? 1 : 0,
                sameZLane ? 1 : 0);
        }

        private void CompareAiSoASensingPostSpecial(
            LF2Entity self,
            int tickIndex,
            int selectedSlot,
            int specialBestDist,
            bool specialObjectProximity,
            bool specialLeft,
            bool specialRight,
            bool specialUp,
            bool specialDown,
            bool specialGuard7A,
            bool specialGuard7B,
            bool specialForce7AGround,
            bool specialC8ThreatSeen,
            bool specialPostSelectionSeen)
        {
            if (!IsAiSoASensingComparisonCurrent(self, tickIndex))
                return;

            int flags = PackAiSoASpecialFlags(
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
            if (selectedSlot != aiSoASensingExpected.PostSpecialSelectedSlot ||
                specialBestDist != aiSoASensingExpected.SpecialBestDist ||
                flags != aiSoASensingExpected.SpecialFlags)
            {
                RecordAiSoASensingPendingMismatch(
                    AiSoASensingShadowMismatchKind.PostSpecialSelection,
                    aiSoASensingExpected.SelfSlot,
                    aiSoASensingExpected.PostSpecialSelectedSlot,
                    selectedSlot,
                    aiSoASensingExpected.SpecialBestDist,
                    specialBestDist,
                    aiSoASensingExpected.SpecialFlags,
                    flags);
            }

            PublishAiSoASensingComparison();
        }

        private void CompleteAiSoASensingComparisonWithoutSpecial(
            LF2Entity self,
            int tickIndex)
        {
            if (IsAiSoASensingComparisonCurrent(self, tickIndex))
                PublishAiSoASensingComparison();
        }

        private bool IsAiSoASensingComparisonCurrent(
            LF2Entity self,
            int tickIndex)
        {
            if (!aiSoASensingComparisonPending ||
                !aiSoASensingSnapshotValid ||
                aiSoASensingPassInvalidated ||
                self?.Runtime == null)
            {
                aiSoASensingComparisonPending = false;
                return false;
            }

            int selfSlot = self.Runtime.SlotIndex;
            if (tickIndex != aiSoASensingExpected.TickIndex ||
                selfSlot != aiSoASensingExpected.SelfSlot ||
                RuntimeSlotOccupancyEpochForServices != aiSoASensingSnapshotEpoch ||
                self.Runtime.StableId != aiSoASensingExpected.SelfIdentity ||
                !TryGetCurrentRuntimeHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != aiSoASensingExpected.SelfGeneration)
            {
                InvalidateAiSoASensingShadowPass();
                return false;
            }

            return true;
        }

        private void RecordAiSoASensingPendingMismatch(
            AiSoASensingShadowMismatchKind kind,
            int selfSlot,
            int expectedSelection,
            int actualSelection,
            int expectedValue,
            int actualValue,
            int expectedFlags,
            int actualFlags)
        {
            int mismatchBit = 1 << ((int)kind - 1);
            aiSoASensingPendingMismatchMask |= mismatchBit;
            if (aiSoASensingPendingFirstMismatch.Kind !=
                AiSoASensingShadowMismatchKind.None)
            {
                return;
            }

            aiSoASensingPendingFirstMismatch =
                new AiSoASensingShadowMismatch
                {
                    Kind = kind,
                    SelfSlot = selfSlot,
                    ExpectedSelection = expectedSelection,
                    ActualSelection = actualSelection,
                    ExpectedValue = expectedValue,
                    ActualValue = actualValue,
                    ExpectedFlags = expectedFlags,
                    ActualFlags = actualFlags,
                };
        }

        private void PublishAiSoASensingComparison()
        {
            int mismatchMask = aiSoASensingPendingMismatchMask;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.ShadowPurity - 1))) != 0)
                AiSoASensingShadowPurityMismatchCountForDiagnostics++;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.InitialNearest - 1))) != 0)
                AiSoASensingShadowInitialMismatchCountForDiagnostics++;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.CachedSelection - 1))) != 0)
                AiSoASensingShadowCachedMismatchCountForDiagnostics++;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.PostSpecialSelection - 1))) != 0)
                AiSoASensingShadowPostSpecialMismatchCountForDiagnostics++;

            AiSoASensingShadowMismatchMaskForDiagnostics |= mismatchMask;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = mismatchMask;
            if (AiSoASensingShadowFirstMismatchForDiagnostics.Kind ==
                    AiSoASensingShadowMismatchKind.None &&
                aiSoASensingPendingFirstMismatch.Kind !=
                    AiSoASensingShadowMismatchKind.None)
            {
                AiSoASensingShadowFirstMismatchForDiagnostics =
                    aiSoASensingPendingFirstMismatch;
            }

            AiSoASensingShadowComparisonPublishedForDiagnostics = true;
            aiSoASensingComparisonPending = false;
            aiSoASensingPendingMismatchMask = 0;
            aiSoASensingPendingFirstMismatch = default;
        }

        private bool TryRunAiSoASensingShadowQuery(
            int selfSlot,
            int inputPhase,
            uint rngState,
            bool forceFullSpecialScan,
            out AiSoASensingResult result)
        {
            result = default;
            if (!TryQueryAiSoANearest(selfSlot, inputPhase, out AiSoANearestResult nearest))
                return false;

            result.SelfSlot = selfSlot;
            result.InitialSelectedSlot = nearest.SelectedSlot;
            result.InitialBestDist = nearest.BestDist;
            result.InitialSameZLane = nearest.SameZLane;

            AiSoASensingRows rows = aiSoASensingRows;
            int selectedSlot = nearest.SelectedSlot;
            int savedTargetSlot = rows.CachedTargetSlot[selfSlot];
            if (IsLivingCharacterAiSoARow(rows, savedTargetSlot) &&
                NextAiSoALocalRandom(ref rngState) % 30 > 0)
            {
                selectedSlot = savedTargetSlot;
            }
            result.CachedSelectedSlot = selectedSlot;
            result.PostSpecialSelectedSlot = selectedSlot;
            result.SpecialBestDist = 10000;
            if (selectedSlot < 0)
                return true;

            if (!TryQueryAiSoASpecial(
                    selfSlot,
                    inputPhase,
                    selectedSlot,
                    nearest.BestDist,
                    nearest.SameZLane,
                    forceFullSpecialScan,
                    out AiSoASpecialResult special))
            {
                return false;
            }

            result.PostSpecialSelectedSlot = special.SelectedSlot;
            result.SpecialBestDist = special.BestDist;
            result.SpecialFlags = special.Flags;
            return true;
        }

        private bool TryQueryAiSoANearest(
            int selfSlot,
            int inputPhase,
            out AiSoANearestResult result)
        {
            result = default;
            if (!AiSensingKernel.TryFindNearest(
                    aiSoASensingRows,
                    selfSlot,
                    inputPhase,
                    out AiSensingNearestResult kernelResult))
                return false;

            result.SelectedSlot = kernelResult.SelectedSlot;
            result.BestDist = kernelResult.BestDist;
            result.SameZLane = kernelResult.SameZLane;
            result.SnapshotEpoch = kernelResult.CapturedOccupancyEpoch;
            result.SelectedGeneration = kernelResult.SelectedGeneration;
            result.SelectedIdentity = kernelResult.SelectedIdentity;
            AiSoACandidateGroundXRowVisitCountForDiagnostics += kernelResult.GroundRowVisits;
            AiSoACandidateAirXRowVisitCountForDiagnostics += kernelResult.AirRowVisits;
            return true;
        }

        private bool TryQueryAiSoASpecial(
            int selfSlot,
            int inputPhase,
            int initialSelectedSlot,
            int nearestBestDist,
            bool sameZLane,
            bool forceFullSpecialScan,
            out AiSoASpecialResult result)
        {
            result = default;
            if (!AiSensingKernel.TryScanSpecial(
                    aiSoASensingRows,
                    selfSlot,
                    inputPhase,
                    initialSelectedSlot,
                    nearestBestDist,
                    sameZLane,
                    forceFullSpecialScan,
                    out AiSensingSpecialResult kernelResult))
            {
                return false;
            }

            result.SelectedSlot = kernelResult.SelectedSlot;
            result.BestDist = kernelResult.BestDist;
            result.SameZLane = kernelResult.SameZLane;
            result.SnapshotEpoch = kernelResult.CapturedOccupancyEpoch;
            result.SelectedGeneration = kernelResult.SelectedGeneration;
            result.SelectedIdentity = kernelResult.SelectedIdentity;
            result.Flags = kernelResult.Flags;
            AiSoACandidateSpecialSlotVisitCountForDiagnostics += kernelResult.SlotVisits;
            return true;
        }

        private bool TryQueryAiSoASpecialPreviousImplementation(
            int selfSlot,
            int inputPhase,
            int initialSelectedSlot,
            int nearestBestDist,
            bool sameZLane,
            bool forceFullSpecialScan,
            out AiSoASpecialResult result)
        {
            result = default;
            AiSoASensingRows rows = aiSoASensingRows;
            if (rows == null ||
                selfSlot < 0 ||
                selfSlot >= rows.Capacity ||
                !rows.Included[selfSlot] ||
                initialSelectedSlot < 0 ||
                initialSelectedSlot >= rows.Capacity ||
                !rows.Included[initialSelectedSlot])
            {
                return false;
            }

            int selectedSlot = initialSelectedSlot;
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

            int selfTeam = rows.Team[selfSlot];
            if ((inputPhase == 1 || inputPhase == 4) && selfTeam != 5)
            {
                specialForce7AGround = true;
                if (rows.Hp[selfSlot] > (4 * rows.Hp3[selfSlot]) / 5 ||
                    rows.Hp[selfSlot] > rows.Hp3[selfSlot] - 130)
                {
                    specialForce7AGround = false;
                }
                if (rows.Hp[selfSlot] > 430 ||
                    rows.Hp[selfSlot] > rows.Hp3[selfSlot] - 130)
                {
                    specialGuard7A = true;
                }

                GetAiSoASameTeamSummaryExcludingSelf(
                    rows,
                    selfSlot,
                    selfTeam,
                    out int sameTeamCount,
                    out int sameTeamMinHp);
                if (sameTeamMinHp < rows.Hp[selfSlot])
                    specialForce7AGround = false;
                if (sameTeamMinHp < rows.Hp[selfSlot] - 200)
                    specialGuard7A = true;
                if (sameTeamCount == 0)
                    specialForce7AGround = false;
            }

            if (rows.KillCount[selfSlot] > -1)
            {
                specialGuard7A = true;
                specialGuard7B = true;
            }
            if (rows.Pp[selfSlot] > 250)
                specialGuard7B = true;
            if (inputPhase == 1 && selfTeam == 1)
                specialGuard7B = true;
            if (selfSlot >= 20 && inputPhase == 4)
                specialGuard7B = true;

            int scanCount = forceFullSpecialScan
                ? rows.Capacity - 20
                : rows.SpecialSlotCount;
            for (int scanIndex = 0; scanIndex < scanCount; scanIndex++)
            {
                int slot = forceFullSpecialScan
                    ? scanIndex + 20
                    : rows.SpecialSlots[scanIndex];
                AiSoACandidateSpecialSlotVisitCountForDiagnostics++;
                if (!rows.Included[slot])
                    continue;

                int objectId = rows.ObjectId[slot];
                int state = rows.State[slot];
                if (objectId == 0xC8)
                {
                    int frameGroup = rows.Frame[slot] / 10;
                    bool threat = frameGroup == 6 && rows.Team[slot] != selfTeam;
                    if (!threat && frameGroup == 5)
                    {
                        bool lowHpWindow =
                            (rows.Hp[selfSlot] >= rows.Hp3[selfSlot] - 70 ||
                             rows.Hp[selfSlot] >= rows.Hp3[selfSlot] - 200) &&
                            (rows.Hp[selfSlot] >= (3 * rows.Hp3[selfSlot]) / 5 ||
                             rows.Hp[selfSlot] < rows.Hp3[selfSlot] - 200);
                        threat = (rows.ObjectId[selfSlot] == 2 ||
                                  rows.ObjectId[selfSlot] == 34) &&
                                 lowHpWindow && rows.Team[slot] == selfTeam;
                    }
                    if (threat)
                        specialC8ThreatSeen = true;
                    if (threat &&
                        Abs(rows.Z[slot] - rows.Z[selfSlot]) < 25 &&
                        Abs(rows.X[slot] - rows.X[selfSlot]) < 150)
                    {
                        specialObjectProximity = true;
                        if (Abs(rows.Z[slot] - rows.Z[selfSlot]) < 20)
                        {
                            if (Abs(rows.X[slot] - rows.X[selfSlot]) < 180)
                            {
                                if (rows.Z[slot] <= rows.Z[selfSlot])
                                    specialUp = true;
                                else
                                    specialDown = true;
                            }
                            if (rows.X[slot] <= rows.X[selfSlot])
                                specialLeft = true;
                            else
                                specialRight = true;
                        }
                    }
                }

                if ((objectId == 0xD3 && state == 0x12) ||
                    (objectId == 0xD4 && rows.Frame[slot] >= 150 && rows.Frame[slot] <= 170))
                {
                    if (Abs(rows.X[slot] - rows.X[selfSlot]) < 80)
                    {
                        if (rows.Z[slot] > rows.Z[selfSlot] + 20)
                            specialDown = true;
                        else if (rows.Z[slot] < rows.Z[selfSlot] - 20)
                            specialUp = true;
                    }
                    if (Abs(rows.Z[slot] - rows.Z[selfSlot]) < 20)
                    {
                        if (rows.X[slot] > rows.X[selfSlot] + 100)
                            specialRight = true;
                        else if (rows.X[slot] < rows.X[selfSlot] - 100)
                            specialLeft = true;
                    }
                }

                if (!specialPostSelectionSeen &&
                    !specialC8ThreatSeen &&
                    !sameZLane &&
                    rows.LinkState[selfSlot] == 0)
                {
                    int dist = AiSoADistance(rows, selfSlot, slot);
                    bool objectIdCandidate = objectId / 100 == 1 || objectId == 0xD5;
                    bool guarded =
                        (objectId == 0x7A && specialGuard7A) ||
                        (objectId == 0x7B && specialGuard7B) ||
                        (rows.InputHistoryGate[selfSlot] && objectId != 0x7A);
                    if (dist < 2 * nearestBestDist &&
                        dist < specialBestDist &&
                        objectIdCandidate &&
                        !guarded &&
                        rows.LinkState[slot] == 0 &&
                        (state == 0x3EC || state == 0x7D4))
                    {
                        selectedSlot = slot;
                        specialBestDist = dist;
                    }
                }

                if (objectId == 0xC8 &&
                    rows.Frame[slot] / 10 == 5 &&
                    Abs(rows.X[slot] - rows.X[selfSlot]) < 300 &&
                    Abs(rows.Z[slot] - rows.Z[selfSlot]) < 90 &&
                    rows.Team[slot] == selfTeam)
                {
                    bool pressure =
                        (rows.Hp[selfSlot] < rows.HpMax[selfSlot] - 70 &&
                         rows.Hp[selfSlot] < 140) ||
                        (rows.Hp[selfSlot] < (3 * rows.HpMax[selfSlot]) / 5 &&
                         rows.Hp[selfSlot] >= 140);
                    if (pressure)
                        selectedSlot = slot;
                    specialPostSelectionSeen = true;
                }

                if (specialForce7AGround &&
                    objectId == 0x7A &&
                    state == 0x3EC &&
                    rows.LinkState[selfSlot] == 0)
                {
                    selectedSlot = slot;
                    specialPostSelectionSeen = true;
                }
            }

            if (specialC8ThreatSeen)
                selectedSlot = selectedBeforeSpecialScan;
            result.SelectedSlot = selectedSlot;
            result.BestDist = specialBestDist;
            result.SameZLane = sameZLane;
            result.SnapshotEpoch = aiSoASensingSnapshotEpoch;
            CaptureAiSoASelectedIdentity(rows, selectedSlot, out result.SelectedGeneration, out result.SelectedIdentity);
            result.Flags = PackAiSoASpecialFlags(
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
            return true;
        }

        private static void CaptureAiSoASelectedIdentity(
            AiSoASensingRows rows,
            int selectedSlot,
            out uint generation,
            out int identity)
        {
            if (selectedSlot >= 0 &&
                selectedSlot < rows.Capacity &&
                rows.Included[selectedSlot])
            {
                generation = rows.Generation[selectedSlot];
                identity = rows.Identity[selectedSlot];
                return;
            }

            generation = 0;
            identity = 0;
        }

        private bool TryRunAiSoACandidateNearest(
            LF2Entity self,
            int inputPhase,
            out AiSoANearestResult result)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (diagnostics != null)
            {
                diagnostics.BeginPhase(BattleAiInputDetailPhase.CandidateNearest);
                try
                {
                    return TryRunAiSoACandidateNearestCore(self, inputPhase, out result);
                }
                finally
                {
                    diagnostics.EndPhase(BattleAiInputDetailPhase.CandidateNearest);
                }
            }

            return TryRunAiSoACandidateNearestCore(self, inputPhase, out result);
        }

        private bool TryRunAiSoACandidateNearestCore(
            LF2Entity self,
            int inputPhase,
            out AiSoANearestResult result)
        {
            result = default;
            if (aiSoACandidatePassLatchedToLegacy ||
                aiSoACandidateForceNearestFailureForSelfCheck ||
                !ValidateAiSoACandidateSelfHandle(self, out int selfSlot))
            {
                return false;
            }

            ulong epochBefore = RuntimeSlotOccupancyEpochForServices;
            if (!TryQueryAiSoANearest(selfSlot, inputPhase, out result) ||
                result.SnapshotEpoch != epochBefore ||
                RuntimeSlotOccupancyEpochForServices != epochBefore ||
                !ValidateAiSoACandidateSelfHandle(self, out int validatedSelfSlot) ||
                validatedSelfSlot != selfSlot ||
                !ValidateAiSoACandidateSelectedHandle(
                    result.SelectedSlot,
                    result.SelectedGeneration,
                    result.SelectedIdentity))
            {
                result = default;
                return false;
            }

            AiSoACandidateNearestQueryCountForDiagnostics++;
            return true;
        }

        private bool TryRunAiSoACandidateSpecial(
            LF2Entity self,
            int inputPhase,
            int selectedSlot,
            int nearestBestDist,
            bool sameZLane,
            out AiSoASpecialResult result)
        {
            BattleAiInputDetailDiagnostics diagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            if (diagnostics != null)
            {
                diagnostics.BeginPhase(BattleAiInputDetailPhase.CandidateSpecial);
                try
                {
                    return TryRunAiSoACandidateSpecialCore(
                        self,
                        inputPhase,
                        selectedSlot,
                        nearestBestDist,
                        sameZLane,
                        out result);
                }
                finally
                {
                    diagnostics.EndPhase(BattleAiInputDetailPhase.CandidateSpecial);
                }
            }

            return TryRunAiSoACandidateSpecialCore(
                self,
                inputPhase,
                selectedSlot,
                nearestBestDist,
                sameZLane,
                out result);
        }

        private bool TryRunAiSoACandidateSpecialCore(
            LF2Entity self,
            int inputPhase,
            int selectedSlot,
            int nearestBestDist,
            bool sameZLane,
            out AiSoASpecialResult result)
        {
            result = default;
            if (aiSoACandidatePassLatchedToLegacy ||
                aiSoACandidateForceSpecialFailureForSelfCheck ||
                !ValidateAiSoACandidateSelfHandle(self, out int selfSlot))
            {
                return false;
            }

            ulong epochBefore = RuntimeSlotOccupancyEpochForServices;
            bool usedEmptySpecialFastPath =
                !ForceFullAiSpecialScanForDiagnostics &&
                aiSoASensingRows.SpecialSlotCount == 0;
            bool querySucceeded;
            if (usedEmptySpecialFastPath)
            {
                // With no special rows, guard values can only affect the absent scan.
                // Gameplay after the scan consumes proximity/direction flags, which are
                // necessarily false.  SoAShadow still calls TryQueryAiSoASpecial directly
                // and therefore retains the complete guard/flag comparison contract.
                querySucceeded = TryBuildAiSoAEmptySpecialResult(
                    selectedSlot,
                    sameZLane,
                    out result);
            }
            else
            {
                querySucceeded = TryQueryAiSoASpecial(
                    selfSlot,
                    inputPhase,
                    selectedSlot,
                    nearestBestDist,
                    sameZLane,
                    ForceFullAiSpecialScanForDiagnostics,
                    out result);
            }

            if (!querySucceeded ||
                result.SnapshotEpoch != epochBefore ||
                RuntimeSlotOccupancyEpochForServices != epochBefore ||
                !ValidateAiSoACandidateSelfHandle(self, out int validatedSelfSlot) ||
                validatedSelfSlot != selfSlot ||
                !ValidateAiSoACandidateSelectedHandle(
                    result.SelectedSlot,
                    result.SelectedGeneration,
                    result.SelectedIdentity))
            {
                result = default;
                return false;
            }

            if (usedEmptySpecialFastPath)
                AiSoACandidateEmptySpecialFastPathCountForDiagnostics++;
            AiSoACandidateSpecialQueryCountForDiagnostics++;
            return true;
        }

        private bool TryBuildAiSoAEmptySpecialResult(
            int selectedSlot,
            bool sameZLane,
            out AiSoASpecialResult result)
        {
            result = default;
            AiSoASensingRows rows = aiSoASensingRows;
            if (rows == null ||
                selectedSlot < 0 ||
                selectedSlot >= rows.Capacity ||
                !rows.Included[selectedSlot])
            {
                return false;
            }

            result.SelectedSlot = selectedSlot;
            result.BestDist = 10000;
            result.SameZLane = sameZLane;
            result.SnapshotEpoch = aiSoASensingSnapshotEpoch;
            CaptureAiSoASelectedIdentity(
                rows,
                selectedSlot,
                out result.SelectedGeneration,
                out result.SelectedIdentity);
            result.Flags = 0;
            return true;
        }

        private bool ValidateAiSoACandidateSelfHandle(
            LF2Entity self,
            out int selfSlot)
        {
            selfSlot = self?.Runtime?.SlotIndex ?? -1;
            return aiSoACandidateExecutionEnabled &&
                   aiSoASensingSnapshotValid &&
                   !aiSoASensingPassInvalidated &&
                   RuntimeSlotOccupancyEpochForServices == aiSoASensingSnapshotEpoch &&
                   selfSlot >= 0 &&
                   selfSlot < aiSoASensingRows.Capacity &&
                   aiSoASensingRows.Included[selfSlot] &&
                   self.Runtime.StableId == aiSoASensingRows.Identity[selfSlot] &&
                   TryGetCurrentRuntimeHandle(
                       selfSlot,
                       self,
                       out RuntimeEntityHandle handle) &&
                   handle.Generation == aiSoASensingRows.Generation[selfSlot];
        }

        private bool ValidateAiSoACandidateSelectedHandle(
            int selectedSlot,
            uint selectedGeneration,
            int selectedIdentity)
        {
            if (selectedSlot < 0)
                return selectedGeneration == 0 && selectedIdentity == 0;

            if (selectedSlot >= aiSoASensingRows.Capacity ||
                !aiSoASensingRows.Included[selectedSlot] ||
                aiSoASensingRows.Generation[selectedSlot] != selectedGeneration ||
                aiSoASensingRows.Identity[selectedSlot] != selectedIdentity ||
                !TryGetRuntimeSlotReadOnlyView(
                    selectedSlot,
                    out RuntimeSlotTable.ReadOnlySlotView view))
            {
                return false;
            }

            return view.Claimed &&
                   view.Generation == selectedGeneration &&
                   view.Entity?.Runtime != null &&
                   view.Entity.Runtime.SlotIndex == selectedSlot &&
                   view.Entity.Runtime.StableId == selectedIdentity;
        }

        private void LatchAiSoACandidateToLegacyBeforeRandom()
        {
            if (AiUnifiedSnapshotExecutionFallbackForbidden)
            {
                ThrowAiUnifiedSnapshotExecutionHardBreach(
                    AiUnifiedSnapshotExceptionStage.InitialSensingCompare,
                    "SoAAiSensing attempted pre-random fallback after unified snapshot commit.");
            }
            aiSoACandidatePassLatchedToLegacy = true;
            AiSoACandidatePreRandomFailureCountForDiagnostics++;
        }

        private void LatchAiSoACandidateToLegacyAfterRandom()
        {
            if (AiUnifiedSnapshotExecutionFallbackForbidden)
            {
                ThrowAiUnifiedSnapshotExecutionHardBreach(
                    AiUnifiedSnapshotExceptionStage.InitialSensingCompare,
                    "SoAAiSensing attempted post-random fallback after unified snapshot commit.");
            }
            aiSoACandidatePassLatchedToLegacy = true;
            AiSoACandidatePostRandomFailureCountForDiagnostics++;
        }

        private int FindNearestGroundAiSoASensingSlot(
            AiSoASensingRows rows,
            int selfSlot,
            int inputPhase,
            out int bestDist)
        {
            int selectedSlot = -1;
            bestDist = 10000;
            int[] roleSlots = rows.GroundRoleSlotsByX;
            int selfTeam = rows.Team[selfSlot];
            int selfX = rows.X[selfSlot];
            for (int summaryIndex = 0;
                 summaryIndex < rows.GroundRoleTeamSummaryCount;
                 summaryIndex++)
            {
                AiSensingRoleTeamSummary summary =
                    rows.GroundRoleTeamSummaries[summaryIndex];
                if (summary.Count <= 0 ||
                    !TeamCandidateAllowed(selfTeam, summary.Team, inputPhase))
                {
                    continue;
                }

                int spanEnd = summary.Start + summary.Count;
                int left = FindAiSoARoleLowerBound(
                    rows,
                    roleSlots,
                    summary.Start,
                    summary.Count,
                    selfX) - 1;
                int right = left + 1;
                while (left >= summary.Start || right < spanEnd)
                {
                    int leftDx = left >= summary.Start
                        ? Abs(rows.X[roleSlots[left]] - selfX)
                        : int.MaxValue;
                    int rightDx = right < spanEnd
                        ? Abs(rows.X[roleSlots[right]] - selfX)
                        : int.MaxValue;
                    if (leftDx > bestDist && rightDx > bestDist)
                        break;

                    int slot;
                    if (leftDx <= rightDx)
                    {
                        slot = roleSlots[left--];
                    }
                    else
                    {
                        slot = roleSlots[right++];
                    }

                    AiSoACandidateGroundXRowVisitCountForDiagnostics++;

                    if (IsGroundAiSoATargetCandidate(rows, selfSlot, slot, inputPhase))
                    {
                        int distance = AiSoADistance(rows, selfSlot, slot);
                        if (IsAiSoABetterCandidate(
                                distance,
                                slot,
                                bestDist,
                                selectedSlot))
                        {
                            bestDist = distance;
                            selectedSlot = slot;
                        }
                    }
                }
            }

            return selectedSlot;
        }

        private int FindNearestAirAiSoASensingSlot(
            AiSoASensingRows rows,
            int selfSlot,
            int inputPhase)
        {
            int selectedSlot = -1;
            int bestDist = 10000;
            int[] roleSlots = rows.AirRoleSlotsByX;
            int selfTeam = rows.Team[selfSlot];
            int selfX = rows.X[selfSlot];
            for (int summaryIndex = 0;
                 summaryIndex < rows.AirRoleTeamSummaryCount;
                 summaryIndex++)
            {
                AiSensingRoleTeamSummary summary =
                    rows.AirRoleTeamSummaries[summaryIndex];
                if (summary.Count <= 0 ||
                    !TeamCandidateAllowed(selfTeam, summary.Team, inputPhase))
                {
                    continue;
                }

                int spanEnd = summary.Start + summary.Count;
                int left = FindAiSoARoleLowerBound(
                    rows,
                    roleSlots,
                    summary.Start,
                    summary.Count,
                    selfX) - 1;
                int right = left + 1;
                while (left >= summary.Start || right < spanEnd)
                {
                    int leftDx = left >= summary.Start
                        ? Abs(rows.X[roleSlots[left]] - selfX)
                        : int.MaxValue;
                    int rightDx = right < spanEnd
                        ? Abs(rows.X[roleSlots[right]] - selfX)
                        : int.MaxValue;
                    int maximumRelevantDx = bestDist < 249 ? bestDist : 249;
                    if (leftDx > maximumRelevantDx && rightDx > maximumRelevantDx)
                        break;

                    int slot;
                    if (leftDx <= rightDx)
                    {
                        slot = roleSlots[left--];
                    }
                    else
                    {
                        slot = roleSlots[right++];
                    }

                    AiSoACandidateAirXRowVisitCountForDiagnostics++;

                    if (IsAirAiSoATargetCandidate(rows, selfSlot, slot, inputPhase) &&
                        Abs(rows.Z[slot] - rows.Z[selfSlot]) < 40 &&
                        Abs(rows.X[slot] - selfX) < 250)
                    {
                        int distance = AiSoADistance(rows, selfSlot, slot);
                        if (IsAiSoABetterCandidate(
                                distance,
                                slot,
                                bestDist,
                                selectedSlot))
                        {
                            bestDist = distance;
                            selectedSlot = slot;
                        }
                    }
                }
            }

            return selectedSlot;
        }

        private static int FindAiSoARoleLowerBound(
            AiSoASensingRows rows,
            int[] roleSlots,
            int start,
            int count,
            int x)
        {
            int lower = start;
            int upper = start + count;
            while (lower < upper)
            {
                int middle = lower + ((upper - lower) >> 1);
                if (rows.X[roleSlots[middle]] < x)
                    lower = middle + 1;
                else
                    upper = middle;
            }

            return lower;
        }

        private static bool IsGroundAiSoARoleMember(AiSoASensingRows rows, int slot)
        {
            if (slot < 0 || slot >= rows.Capacity || !rows.Included[slot])
                return false;

            int state = rows.State[slot];
            return rows.Hp[slot] > 0 &&
                   state != 14 &&
                   Abs(rows.Y[slot]) <= 2 &&
                   (rows.DataObjectType[slot] == 0 || state == 3000);
        }

        private static bool IsAirAiSoARoleMember(AiSoASensingRows rows, int slot)
        {
            return slot >= 0 &&
                   slot < rows.Capacity &&
                   rows.Included[slot] &&
                   rows.Hp[slot] > 0 &&
                   (rows.State[slot] == 14 || Abs(rows.Y[slot]) > 2);
        }

        private static bool IsGroundAiSoATargetCandidate(
            AiSoASensingRows rows,
            int selfSlot,
            int candidateSlot,
            int inputPhase)
        {
            if (candidateSlot == selfSlot || !rows.Included[candidateSlot])
                return false;

            int state = rows.State[candidateSlot];
            if (rows.DataObjectType[candidateSlot] != 0)
            {
                if (state != 3000)
                    return false;
                if (rows.X[candidateSlot] > rows.X[selfSlot])
                {
                    if (!(rows.Vx[candidateSlot] < 0.001))
                        return false;
                }
                else if (rows.X[candidateSlot] < rows.X[selfSlot])
                {
                    if (!(rows.Vx[candidateSlot] > 0.001))
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return TeamCandidateAllowed(
                       rows.Team[selfSlot],
                       rows.Team[candidateSlot],
                       inputPhase) &&
                   rows.Hp[candidateSlot] > 0 &&
                   state != 14 &&
                   Abs(rows.Y[candidateSlot]) <= 2;
        }

        private static bool IsAirAiSoATargetCandidate(
            AiSoASensingRows rows,
            int selfSlot,
            int candidateSlot,
            int inputPhase)
        {
            return candidateSlot != selfSlot &&
                   rows.Included[candidateSlot] &&
                   TeamCandidateAllowed(
                       rows.Team[selfSlot],
                       rows.Team[candidateSlot],
                       inputPhase) &&
                   rows.Hp[candidateSlot] > 0 &&
                   (rows.State[candidateSlot] == 14 || Abs(rows.Y[candidateSlot]) > 2);
        }

        private static bool IsLivingCharacterAiSoARow(
            AiSoASensingRows rows,
            int slot)
        {
            return slot >= 0 &&
                   slot < rows.Capacity &&
                   rows.Included[slot] &&
                   rows.DataObjectType[slot] == 0 &&
                   rows.Hp[slot] > 0;
        }

        private static void GetAiSoASameTeamSummaryExcludingSelf(
            AiSoASensingRows rows,
            int selfSlot,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            otherCount = 0;
            otherMinHp = int.MaxValue;
            int summaryIndex = FindAiSoATeamSummaryIndex(rows, selfTeam);
            if (summaryIndex < 0)
                return;

            AiSensingTeamSummary summary = rows.TeamSummaries[summaryIndex];
            otherCount = summary.Count;
            if (!IsLivingCharacterAiSoARow(rows, selfSlot))
            {
                otherMinHp = summary.MinHp;
                return;
            }

            otherCount--;
            if (otherCount <= 0)
            {
                otherCount = 0;
                return;
            }

            otherMinHp = rows.Hp[selfSlot] == summary.MinHp && summary.MinCount == 1
                ? summary.SecondMinHp
                : summary.MinHp;
        }

        private static int AiSoADistance(
            AiSoASensingRows rows,
            int firstSlot,
            int secondSlot)
        {
            return Abs(rows.X[secondSlot] - rows.X[firstSlot]) +
                   Abs(rows.Z[secondSlot] - rows.Z[firstSlot]);
        }

        private static bool IsAiSoABetterCandidate(
            int candidateDistance,
            int candidateSlot,
            int bestDistance,
            int selectedSlot)
        {
            return candidateDistance < bestDistance ||
                   (candidateDistance == bestDistance &&
                    selectedSlot >= 0 &&
                    candidateSlot < selectedSlot);
        }

        private static int NextAiSoALocalRandom(ref uint state)
        {
            unchecked
            {
                state = state * 0x343FDu + 0x269EC3u;
            }
            return (int)((state >> 16) & 0x7FFFu);
        }

        private static int PackAiSoASpecialFlags(
            bool specialObjectProximity,
            bool specialLeft,
            bool specialRight,
            bool specialUp,
            bool specialDown,
            bool specialGuard7A,
            bool specialGuard7B,
            bool specialForce7AGround,
            bool specialC8ThreatSeen,
            bool specialPostSelectionSeen)
        {
            int flags = 0;
            if (specialObjectProximity) flags |= AiSoASpecialProximity;
            if (specialLeft) flags |= AiSoASpecialLeft;
            if (specialRight) flags |= AiSoASpecialRight;
            if (specialUp) flags |= AiSoASpecialUp;
            if (specialDown) flags |= AiSoASpecialDown;
            if (specialGuard7A) flags |= AiSoASpecialGuard7A;
            if (specialGuard7B) flags |= AiSoASpecialGuard7B;
            if (specialForce7AGround) flags |= AiSoASpecialForce7AGround;
            if (specialC8ThreatSeen) flags |= AiSoASpecialC8ThreatSeen;
            if (specialPostSelectionSeen) flags |= AiSoASpecialPostSelectionSeen;
            return flags;
        }

        private static int PackAiSoACacheFlags(
            bool cachedTargetEligible,
            bool cacheRandomCalled)
        {
            int flags = 0;
            if (cachedTargetEligible) flags |= 1 << 0;
            if (cacheRandomCalled) flags |= 1 << 1;
            return flags;
        }

        internal bool CaptureAiSoASensingNearestForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out int selectedSlot,
            out int bestDist,
            out bool sameZLane)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            AiSensingMode previousMode = aiSensingMode;
            bool snapshotBuilt = false;
            try
            {
                aiSensingMode = AiSensingMode.SoAShadowAiSensing;
                BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                if (!ValidateAiSoASensingShadowSnapshot() ||
                    self?.Runtime == null ||
                    !TryRunAiSoASensingShadowQuery(
                        self.Runtime.SlotIndex,
                        inputPhase,
                        Rng?.State ?? 0,
                        ForceFullAiSpecialScanForDiagnostics,
                        out AiSoASensingResult result))
                {
                    selectedSlot = -1;
                    bestDist = 10000;
                    sameZLane = false;
                    return false;
                }

                selectedSlot = result.InitialSelectedSlot;
                bestDist = result.InitialBestDist;
                sameZLane = result.InitialSameZLane;
                return true;
            }
            finally
            {
                CompleteAiSoASensingSelfCheck(previousMode, snapshotBuilt);
            }
        }

        internal long MeasureAiSoASensingShadowAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            AiSensingMode previousMode = aiSensingMode;
            bool snapshotBuilt = false;
            try
            {
                aiSensingMode = AiSensingMode.SoAShadowAiSensing;
                BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                if (!ValidateAiSoASensingShadowSnapshot() || self?.Runtime == null)
                    return -1;
                int selfSlot = self.Runtime.SlotIndex;
                uint rngState = Rng?.State ?? 0;
                for (int index = 0; index < 16; index++)
                {
                    if (!TryRunAiSoASensingShadowQuery(
                            selfSlot,
                            inputPhase,
                            rngState,
                            ForceFullAiSpecialScanForDiagnostics,
                            out _))
                    {
                        return -1;
                    }
                }

                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int index = 0; index < iterations; index++)
                {
                    if (!TryRunAiSoASensingShadowQuery(
                            selfSlot,
                            inputPhase,
                            rngState,
                            ForceFullAiSpecialScanForDiagnostics,
                            out _))
                    {
                        return -1;
                    }
                }
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                CompleteAiSoASensingSelfCheck(previousMode, snapshotBuilt);
            }
        }

        internal bool AiSoASensingEpochDriftInvalidatesForSelfCheck()
        {
            EnsureAiSoASensingSelfCheckCanRun();
            AiSensingMode previousMode = aiSensingMode;
            bool snapshotBuilt = false;
            try
            {
                aiSensingMode = AiSensingMode.SoAShadowAiSensing;
                BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                aiSoASensingSnapshotEpoch++;
                return !ValidateAiSoASensingShadowSnapshot() && aiSoASensingPassInvalidated;
            }
            finally
            {
                CompleteAiSoASensingSelfCheck(previousMode, snapshotBuilt);
            }
        }

        internal bool AiSoASensingGenerationDriftInvalidatesForSelfCheck()
        {
            EnsureAiSoASensingSelfCheckCanRun();
            AiSensingMode previousMode = aiSensingMode;
            bool snapshotBuilt = false;
            try
            {
                aiSensingMode = AiSensingMode.SoAShadowAiSensing;
                BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                for (int slot = 0; slot < aiSoASensingRows.Capacity; slot++)
                {
                    if (!aiSoASensingRows.Included[slot])
                        continue;
                    aiSoASensingRows.Generation[slot]++;
                    return !ValidateAiSoASensingShadowSnapshot() && aiSoASensingPassInvalidated;
                }
                return false;
            }
            finally
            {
                CompleteAiSoASensingSelfCheck(previousMode, snapshotBuilt);
            }
        }

        internal bool AiSoASensingIdentityDriftInvalidatesForSelfCheck()
        {
            EnsureAiSoASensingSelfCheckCanRun();
            AiSensingMode previousMode = aiSensingMode;
            bool snapshotBuilt = false;
            try
            {
                aiSensingMode = AiSensingMode.SoAShadowAiSensing;
                BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                for (int slot = 0; slot < aiSoASensingRows.Capacity; slot++)
                {
                    if (!aiSoASensingRows.Included[slot])
                        continue;
                    aiSoASensingRows.Identity[slot]++;
                    return !ValidateAiSoASensingShadowSnapshot() && aiSoASensingPassInvalidated;
                }
                return false;
            }
            finally
            {
                CompleteAiSoASensingSelfCheck(previousMode, snapshotBuilt);
            }
        }

        private void EnsureAiSoASensingSelfCheckCanRun()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "AI sensing self-checks cannot run during a simulation pass.");
            }
        }

        private void CompleteAiSoASensingSelfCheck(
            AiSensingMode previousMode,
            bool snapshotBuilt)
        {
            try
            {
                if (snapshotBuilt)
                    ClearAiInputSlotSnapshot();
            }
            finally
            {
                aiSensingMode = previousMode;
            }
        }
    }
}
