using System;

using NTSD.App;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation.Ecs
{
    public enum BattleHitExecutionPlanMode : byte
    {
        Disabled = 0,
        ShadowCapture = 1,
        ShadowCompare = 2,
        DataOriented = 3,
    }

    public enum BattleHitExecutionPass : byte
    {
        Character = 1,
        Object = 2,
    }

    public enum BattleHitCandidateDisposition : byte
    {
        None = 0,
        RejectedByConsumeGate = 1,
        Oid300Redirect = 2,
        Damage = 3,
        HitConfirm = 4,
        Kind8 = 5,
        Kind14 = 6,
        Kind15 = 7,
        Kind10Or11 = 8,
        Kind1Grab = 9,
        Kind3Grab = 10,
        Pickup = 11,
        Unsupported = 12,
    }

    public enum BattleHitExecutionPlanFailureReason : byte
    {
        None = 0,
        CandidateSourceUnavailable = 1,
        AttackerHandleNotCurrent = 2,
        AttackerFrameUnavailable = 3,
        CandidateCountMismatch = 4,
        CandidateReadFailed = 5,
        ItrIndexOutOfRange = 6,
        CapacityExceeded = 7,
        DuplicatePassCapture = 8,
        ObservationPassUnavailable = 9,
        ObservationEntryUnexpected = 10,
        ObservationEntryMismatch = 11,
        ObservationEntryMissing = 12,
        ObservationPreprocessUnexpected = 13,
        ObservationPreprocessMismatch = 14,
        ObservationPreprocessMissing = 15,
        ObservationConsumeEffectsUnexpected = 16,
        ObservationConsumeEffectsPreStateMismatch = 17,
        ObservationConsumeEffectsMismatch = 18,
        ObservationConsumeEffectsMissing = 19,
        ObservationDispatchUnexpected = 20,
        ObservationDispatchMismatch = 21,
        ObservationDispatchMissing = 22,
        ObservationDispositionUnexpected = 23,
        ObservationDispositionMismatch = 24,
        ObservationDispositionMissing = 25,
        ObservationWriterEffectUnexpected = 26,
        ObservationWriterEffectPreStateMismatch = 27,
        ObservationWriterEffectMismatch = 28,
        ObservationWriterEffectMissing = 29,
        ObservationLifecycleEffectUnexpected = 30,
        ObservationLifecycleEffectPreStateMismatch = 31,
        ObservationLifecycleEffectMismatch = 32,
        ObservationLifecycleEffectMissing = 33,
        ObservationFirstBodyResponseUnexpected = 34,
        ObservationFirstBodyResponsePreStateMismatch = 35,
        ObservationFirstBodyResponseMismatch = 36,
        ObservationFirstBodyResponseMissing = 37,
    }

    public readonly struct BattleHitExecutionPlanEntryView
    {
        internal BattleHitExecutionPlanEntryView(
            BattleHitExecutionPass pass,
            RuntimeEntityHandle attackerHandle,
            int attackerStableId,
            int attackerPrevFrame2,
            int candidateOrdinal,
            int targetSlot,
            RuntimeEntityHandle targetHandleSnapshot,
            int itrIndex,
            int itrKind,
            ulong sourceItrFingerprint,
            ulong recordedItrFingerprint,
            bool zeroAttackerHpOnConsume,
            bool releaseHeavyHeldTargetOnConsume,
            bool preprocessObserved,
            ulong expectedResolvedItrFingerprint,
            ulong observedResolvedItrFingerprint,
            bool expectedZeroAttackerHpAfterPreprocess,
            bool observedZeroAttackerHpAfterPreprocess,
            bool expectedReleaseHeavyHeldTargetAfterPreprocess,
            bool observedReleaseHeavyHeldTargetAfterPreprocess,
            int expectedResolvedItrKind,
            int observedResolvedItrKind,
            bool dispositionObserved,
            BattleHitCandidateDisposition expectedDisposition,
            BattleHitCandidateDisposition observedDisposition,
            bool firstBodyResponseAttemptObserved,
            bool expectedFirstBodyResponseEligible,
            bool observedFirstBodyResponseEligible,
            BattleFirstBodyResponseKind expectedFirstBodyResponseKind,
            BattleFirstBodyResponseKind observedFirstBodyResponseKind,
            bool consumeEffectsObserved,
            ulong expectedConsumeEffectsFingerprint,
            ulong observedConsumeEffectsFingerprint,
            uint expectedRngStateAfterConsume,
            uint observedRngStateAfterConsume,
            ulong expectedRngCallCountAfterConsume,
            ulong observedRngCallCountAfterConsume)
        {
            Pass = pass;
            AttackerHandle = attackerHandle;
            AttackerStableId = attackerStableId;
            AttackerPrevFrame2 = attackerPrevFrame2;
            CandidateOrdinal = candidateOrdinal;
            TargetSlot = targetSlot;
            TargetHandleSnapshot = targetHandleSnapshot;
            ItrIndex = itrIndex;
            ItrKind = itrKind;
            SourceItrFingerprint = sourceItrFingerprint;
            RecordedItrFingerprint = recordedItrFingerprint;
            ZeroAttackerHpOnConsume = zeroAttackerHpOnConsume;
            ReleaseHeavyHeldTargetOnConsume = releaseHeavyHeldTargetOnConsume;
            PreprocessObserved = preprocessObserved;
            ExpectedResolvedItrFingerprint = expectedResolvedItrFingerprint;
            ObservedResolvedItrFingerprint = observedResolvedItrFingerprint;
            ExpectedZeroAttackerHpAfterPreprocess =
                expectedZeroAttackerHpAfterPreprocess;
            ObservedZeroAttackerHpAfterPreprocess =
                observedZeroAttackerHpAfterPreprocess;
            ExpectedReleaseHeavyHeldTargetAfterPreprocess =
                expectedReleaseHeavyHeldTargetAfterPreprocess;
            ObservedReleaseHeavyHeldTargetAfterPreprocess =
                observedReleaseHeavyHeldTargetAfterPreprocess;
            ExpectedResolvedItrKind = expectedResolvedItrKind;
            ObservedResolvedItrKind = observedResolvedItrKind;
            DispositionObserved = dispositionObserved;
            ExpectedDisposition = expectedDisposition;
            ObservedDisposition = observedDisposition;
            FirstBodyResponseAttemptObserved = firstBodyResponseAttemptObserved;
            ExpectedFirstBodyResponseEligible =
                expectedFirstBodyResponseEligible;
            ObservedFirstBodyResponseEligible =
                observedFirstBodyResponseEligible;
            ExpectedFirstBodyResponseKind = expectedFirstBodyResponseKind;
            ObservedFirstBodyResponseKind = observedFirstBodyResponseKind;
            ConsumeEffectsObserved = consumeEffectsObserved;
            ExpectedConsumeEffectsFingerprint = expectedConsumeEffectsFingerprint;
            ObservedConsumeEffectsFingerprint = observedConsumeEffectsFingerprint;
            ExpectedRngStateAfterConsume = expectedRngStateAfterConsume;
            ObservedRngStateAfterConsume = observedRngStateAfterConsume;
            ExpectedRngCallCountAfterConsume = expectedRngCallCountAfterConsume;
            ObservedRngCallCountAfterConsume = observedRngCallCountAfterConsume;
        }

        public BattleHitExecutionPass Pass { get; }
        public RuntimeEntityHandle AttackerHandle { get; }
        public int AttackerStableId { get; }
        public int AttackerPrevFrame2 { get; }
        public int CandidateOrdinal { get; }
        public int TargetSlot { get; }

        /// <summary>
        /// Diagnostic identity observed while the plan is captured. Candidate
        /// consumption is still authoritative by TargetSlot; this handle must not
        /// become a gameplay gate because the C# authority has fixed slots only.
        /// </summary>
        public RuntimeEntityHandle TargetHandleSnapshot { get; }

        public int ItrIndex { get; }
        public int ItrKind { get; }
        public ulong SourceItrFingerprint { get; }
        public ulong RecordedItrFingerprint { get; }
        public bool ZeroAttackerHpOnConsume { get; }
        public bool ReleaseHeavyHeldTargetOnConsume { get; }
        public bool PreprocessObserved { get; }
        public ulong ExpectedResolvedItrFingerprint { get; }
        public ulong ObservedResolvedItrFingerprint { get; }
        public bool ExpectedZeroAttackerHpAfterPreprocess { get; }
        public bool ObservedZeroAttackerHpAfterPreprocess { get; }
        public bool ExpectedReleaseHeavyHeldTargetAfterPreprocess { get; }
        public bool ObservedReleaseHeavyHeldTargetAfterPreprocess { get; }
        public int ExpectedResolvedItrKind { get; }
        public int ObservedResolvedItrKind { get; }
        public bool DispositionObserved { get; }
        public BattleHitCandidateDisposition ExpectedDisposition { get; }
        public BattleHitCandidateDisposition ObservedDisposition { get; }
        public bool FirstBodyResponseAttemptObserved { get; }
        public bool ExpectedFirstBodyResponseEligible { get; }
        public bool ObservedFirstBodyResponseEligible { get; }
        public BattleFirstBodyResponseKind ExpectedFirstBodyResponseKind { get; }
        public BattleFirstBodyResponseKind ObservedFirstBodyResponseKind { get; }
        public bool ConsumeEffectsObserved { get; }
        public ulong ExpectedConsumeEffectsFingerprint { get; }
        public ulong ObservedConsumeEffectsFingerprint { get; }
        public uint ExpectedRngStateAfterConsume { get; }
        public uint ObservedRngStateAfterConsume { get; }
        public ulong ExpectedRngCallCountAfterConsume { get; }
        public ulong ObservedRngCallCountAfterConsume { get; }
    }

    public readonly struct BattleHitExecutionPlanDiagnostics
    {
        internal BattleHitExecutionPlanDiagnostics(
            BattleHitExecutionPlanMode mode,
            int capturedTick,
            long characterPassCaptureCount,
            long objectPassCaptureCount,
            long attackerVisitCount,
            long plannedAttackerCount,
            long plannedCandidateCount,
            long observationPassCount,
            long observedCandidateCount,
            long observedPreprocessCount,
            long observedDispositionCount,
            long observedFirstBodyResponseAttemptCount,
            long observedConsumeEffectsCount,
            long observedWriterEffectCount,
            long observedLifecycleEffectCount,
            long observedDispatchCount,
            long observedAbortTerminationCount,
            long skippedCandidateCountAfterAbort,
            long observationMismatchCount,
            long failureCount,
            ulong lastConsumeEffectsDifferenceMask,
            ulong lastFirstBodyResponseDifferenceMask,
            ulong lastWriterEffectDifferenceMask,
            ulong lastLifecycleEffectDifferenceMask,
            BattleHitExecutionPlanFailureReason firstFailureReason,
            int firstFailureAttackerSlot,
            int firstFailureCandidateOrdinal,
            bool currentTickPlanValid)
        {
            Mode = mode;
            CapturedTick = capturedTick;
            CharacterPassCaptureCount = characterPassCaptureCount;
            ObjectPassCaptureCount = objectPassCaptureCount;
            AttackerVisitCount = attackerVisitCount;
            PlannedAttackerCount = plannedAttackerCount;
            PlannedCandidateCount = plannedCandidateCount;
            ObservationPassCount = observationPassCount;
            ObservedCandidateCount = observedCandidateCount;
            ObservedPreprocessCount = observedPreprocessCount;
            ObservedDispositionCount = observedDispositionCount;
            ObservedFirstBodyResponseAttemptCount =
                observedFirstBodyResponseAttemptCount;
            ObservedConsumeEffectsCount = observedConsumeEffectsCount;
            ObservedWriterEffectCount = observedWriterEffectCount;
            ObservedLifecycleEffectCount = observedLifecycleEffectCount;
            ObservedDispatchCount = observedDispatchCount;
            ObservedAbortTerminationCount = observedAbortTerminationCount;
            SkippedCandidateCountAfterAbort = skippedCandidateCountAfterAbort;
            ObservationMismatchCount = observationMismatchCount;
            FailureCount = failureCount;
            LastConsumeEffectsDifferenceMask = lastConsumeEffectsDifferenceMask;
            LastFirstBodyResponseDifferenceMask =
                lastFirstBodyResponseDifferenceMask;
            LastWriterEffectDifferenceMask = lastWriterEffectDifferenceMask;
            LastLifecycleEffectDifferenceMask = lastLifecycleEffectDifferenceMask;
            FirstFailureReason = firstFailureReason;
            FirstFailureAttackerSlot = firstFailureAttackerSlot;
            FirstFailureCandidateOrdinal = firstFailureCandidateOrdinal;
            CurrentTickPlanValid = currentTickPlanValid;
        }

        public BattleHitExecutionPlanMode Mode { get; }
        public int CapturedTick { get; }
        public long CharacterPassCaptureCount { get; }
        public long ObjectPassCaptureCount { get; }
        public long AttackerVisitCount { get; }
        public long PlannedAttackerCount { get; }
        public long PlannedCandidateCount { get; }
        public long ObservationPassCount { get; }
        public long ObservedCandidateCount { get; }
        public long ObservedPreprocessCount { get; }
        public long ObservedDispositionCount { get; }
        public long ObservedFirstBodyResponseAttemptCount { get; }
        public long ObservedConsumeEffectsCount { get; }
        public long ObservedWriterEffectCount { get; }
        public long ObservedLifecycleEffectCount { get; }
        public long ObservedDispatchCount { get; }
        public long ObservedAbortTerminationCount { get; }
        public long SkippedCandidateCountAfterAbort { get; }
        public long ObservationMismatchCount { get; }
        public long FailureCount { get; }
        public ulong LastConsumeEffectsDifferenceMask { get; }
        public ulong LastFirstBodyResponseDifferenceMask { get; }
        public ulong LastWriterEffectDifferenceMask { get; }
        public ulong LastLifecycleEffectDifferenceMask { get; }
        public BattleHitExecutionPlanFailureReason FirstFailureReason { get; }
        public int FirstFailureAttackerSlot { get; }
        public int FirstFailureCandidateOrdinal { get; }
        public bool CurrentTickPlanValid { get; }
    }

    /// <summary>
    /// U5 boundary for the authority hit loops. It freezes the exact participant
    /// and candidate order consumed by the canonical writer. Shadow modes remain
    /// read-only; DataOriented only changes pass scheduling and still reuses the
    /// same resolver writer so battle rules are not duplicated.
    /// </summary>
    internal sealed class BattleEcsHitExecutionPlan
    {
        private const int HitCandidateMaximum = 20;

        private readonly SimulationWorld world;
        private readonly Entry[] entries;
        private readonly RuntimeEntityHandle[] characterParticipants;
        private readonly RuntimeEntityHandle[] objectParticipants;
        private readonly int[] characterParticipantEntryStarts;
        private readonly int[] characterParticipantEntryCounts;
        private readonly int[] objectParticipantEntryStarts;
        private readonly int[] objectParticipantEntryCounts;
        private readonly CollisionCandidateRange[] characterParticipantCandidateRanges;
        private readonly CollisionCandidateRange[] objectParticipantCandidateRanges;
        private BattleHitExecutionPlanMode mode;
        private int capturedTick = -1;
        private int entryCount;
        private bool characterPassCaptured;
        private bool objectPassCaptured;
        private int characterEntryStart;
        private int characterEntryCount;
        private int objectEntryStart;
        private int objectEntryCount;
        private int characterParticipantCount;
        private int objectParticipantCount;
        private bool observationPassActive;
        private BattleHitExecutionPass observationPass;
        private int observationEntryStart;
        private int observationExpectedCount;
        private int observationReadCount;
        private int pendingPreprocessEntryIndex = -1;
        private bool pendingPreprocessExpected;
        private ulong pendingExpectedResolvedItrFingerprint;
        private bool pendingExpectedZeroAttackerHp;
        private bool pendingExpectedReleaseHeavyHeldTarget;
        private int activePreprocessEntryIndex = -1;
        private int activeDispositionEntryIndex = -1;
        private int pendingFirstBodyResponseEntryIndex = -1;
        private bool pendingFirstBodyResponseExpected;
        private FirstBodyResponseAttemptSnapshot
            pendingExpectedFirstBodyResponseSnapshot;
        private int pendingConsumeEffectsEntryIndex = -1;
        private bool pendingConsumeEffectsExpected;
        private int pendingConsumeEffectsHeldTargetSlot = -1;
        private ulong pendingExpectedConsumeEffectsFingerprint;
        private ConsumeEffectsSnapshot pendingExpectedConsumeEffectsSnapshot;
        private int pendingDispatchEntryIndex = -1;
        private bool pendingDispatchExpected;
        private bool pendingExpectedAbortAfterSuccessfulDispatch;
        private int pendingWriterEffectEntryIndex = -1;
        private bool pendingWriterEffectExpected;
        private WriterEffectSnapshot pendingExpectedWriterEffectSnapshot;
        private int pendingLifecycleEffectEntryIndex = -1;
        private bool pendingLifecycleEffectExpected;
        private LifecycleEffectSnapshot pendingExpectedLifecycleEffectSnapshot;
        private bool currentTickPlanValid;
        private long characterPassCaptureCount;
        private long objectPassCaptureCount;
        private long attackerVisitCount;
        private long plannedAttackerCount;
        private long plannedCandidateCount;
        private long observationPassCount;
        private long observedCandidateCount;
        private long observedPreprocessCount;
        private long observedDispositionCount;
        private long observedFirstBodyResponseAttemptCount;
        private long observedConsumeEffectsCount;
        private long observedWriterEffectCount;
        private long observedLifecycleEffectCount;
        private long observedDispatchCount;
        private long observedAbortTerminationCount;
        private long skippedCandidateCountAfterAbort;
        private long observationMismatchCount;
        private long failureCount;
        private ulong lastConsumeEffectsDifferenceMask;
        private ulong lastFirstBodyResponseDifferenceMask;
        private ulong lastWriterEffectDifferenceMask;
        private ulong lastLifecycleEffectDifferenceMask;
        private BattleHitExecutionPlanFailureReason firstFailureReason;
        private int firstFailureAttackerSlot = -1;
        private int firstFailureCandidateOrdinal = -1;

        internal BattleEcsHitExecutionPlan(
            SimulationWorld world,
            int runtimeSlotCapacity)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            if (runtimeSlotCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(runtimeSlotCapacity));

            entries = new Entry[checked(runtimeSlotCapacity * HitCandidateMaximum)];
            characterParticipants = new RuntimeEntityHandle[runtimeSlotCapacity];
            objectParticipants = new RuntimeEntityHandle[runtimeSlotCapacity];
            characterParticipantEntryStarts = new int[runtimeSlotCapacity];
            characterParticipantEntryCounts = new int[runtimeSlotCapacity];
            objectParticipantEntryStarts = new int[runtimeSlotCapacity];
            objectParticipantEntryCounts = new int[runtimeSlotCapacity];
            characterParticipantCandidateRanges =
                new CollisionCandidateRange[runtimeSlotCapacity];
            objectParticipantCandidateRanges =
                new CollisionCandidateRange[runtimeSlotCapacity];
        }

        internal BattleHitExecutionPlanMode Mode => mode;
        internal int EntryCount => entryCount;
        internal bool UsesDataOrientedScheduling =>
            mode == BattleHitExecutionPlanMode.DataOriented;
        internal bool ShouldObserveLegacyCandidateRead =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;
        internal bool ShouldObserveLegacyPreprocess =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;
        internal bool ShouldObserveLegacyConsumeEffects =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;
        internal bool ShouldObserveLegacyDisposition =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;
        internal bool ShouldObserveLegacyFirstBodyResponseAttempt =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;
        internal bool ShouldObserveLegacyDispatch =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;
        internal bool ShouldObserveLegacyWriterEffect =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;
        internal bool ShouldObserveLegacyLifecycleEffect =>
            mode == BattleHitExecutionPlanMode.ShadowCompare &&
            observationPassActive;

        internal bool CanProjectLegacyWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            if (!ShouldObserveLegacyWriterEffect)
                return false;

            if (disposition != BattleHitCandidateDisposition.Damage)
                return true;

            if (IsOid201CharacterHitLifecycle(attacker, target, resolvedItr))
                return false;

            if (activePreprocessEntryIndex < 0 ||
                activePreprocessEntryIndex >= entryCount ||
                entries[activePreprocessEntryIndex]
                    .ExpectedReleaseHeavyHeldTargetAfterPreprocess)
            {
                return false;
            }

            return CanProjectDamageWriterEffect(
                attacker,
                target,
                resolvedItr);
        }

        internal bool CanProjectLegacyLifecycleEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            return ShouldObserveLegacyLifecycleEffect &&
                   disposition == BattleHitCandidateDisposition.Damage &&
                   activePreprocessEntryIndex >= 0 &&
                   activePreprocessEntryIndex < entryCount &&
                   !entries[activePreprocessEntryIndex]
                       .ExpectedReleaseHeavyHeldTargetAfterPreprocess &&
                   IsOid201CharacterHitLifecycle(attacker, target, resolvedItr) &&
                   CanProjectStandardCharacterDamageWriterEffect(
                       attacker,
                       target,
                       resolvedItr);
        }

        internal BattleHitExecutionPlanDiagnostics Diagnostics =>
            new BattleHitExecutionPlanDiagnostics(
                mode,
                capturedTick,
                characterPassCaptureCount,
                objectPassCaptureCount,
                attackerVisitCount,
                plannedAttackerCount,
                plannedCandidateCount,
                observationPassCount,
                observedCandidateCount,
                observedPreprocessCount,
                observedDispositionCount,
                observedFirstBodyResponseAttemptCount,
                observedConsumeEffectsCount,
                observedWriterEffectCount,
                observedLifecycleEffectCount,
                observedDispatchCount,
                observedAbortTerminationCount,
                skippedCandidateCountAfterAbort,
                observationMismatchCount,
                failureCount,
                lastConsumeEffectsDifferenceMask,
                lastFirstBodyResponseDifferenceMask,
                lastWriterEffectDifferenceMask,
                lastLifecycleEffectDifferenceMask,
                firstFailureReason,
                firstFailureAttackerSlot,
                firstFailureCandidateOrdinal,
                currentTickPlanValid);

        internal void SetMode(BattleHitExecutionPlanMode requestedMode)
        {
            mode = requestedMode;
            Reset();
        }

        internal void Reset()
        {
            capturedTick = -1;
            entryCount = 0;
            characterPassCaptured = false;
            objectPassCaptured = false;
            characterEntryStart = 0;
            characterEntryCount = 0;
            objectEntryStart = 0;
            objectEntryCount = 0;
            characterParticipantCount = 0;
            objectParticipantCount = 0;
            observationPassActive = false;
            observationPass = default;
            observationEntryStart = 0;
            observationExpectedCount = 0;
            observationReadCount = 0;
            ResetPendingPreprocessExpectation();
            ResetPendingFirstBodyResponseExpectation();
            ResetConsumeEffectsObservation();
            ResetPendingWriterEffectExpectation();
            ResetPendingLifecycleEffectExpectation();
            activeDispositionEntryIndex = -1;
            currentTickPlanValid = false;
            characterPassCaptureCount = 0;
            objectPassCaptureCount = 0;
            attackerVisitCount = 0;
            plannedAttackerCount = 0;
            plannedCandidateCount = 0;
            observationPassCount = 0;
            observedCandidateCount = 0;
            observedPreprocessCount = 0;
            observedDispositionCount = 0;
            observedFirstBodyResponseAttemptCount = 0;
            observedConsumeEffectsCount = 0;
            observedWriterEffectCount = 0;
            observedLifecycleEffectCount = 0;
            observedDispatchCount = 0;
            observedAbortTerminationCount = 0;
            skippedCandidateCountAfterAbort = 0;
            observationMismatchCount = 0;
            failureCount = 0;
            lastConsumeEffectsDifferenceMask = 0;
            lastFirstBodyResponseDifferenceMask = 0;
            lastWriterEffectDifferenceMask = 0;
            lastLifecycleEffectDifferenceMask = 0;
            firstFailureReason = BattleHitExecutionPlanFailureReason.None;
            firstFailureAttackerSlot = -1;
            firstFailureCandidateOrdinal = -1;
        }

        internal void CapturePass(
            int tickIndex,
            BattleHitExecutionPass pass,
            bool skipProvenEmptyBaseCharacters = false,
            bool passProvenEmpty = false)
        {
            if (mode == BattleHitExecutionPlanMode.Disabled)
                return;

            BeginTickIfNeeded(tickIndex);
            if (!TryMarkPassCaptured(pass))
            {
                RecordFailure(
                    BattleHitExecutionPlanFailureReason.DuplicatePassCapture,
                    -1,
                    -1);
                return;
            }

            int passEntryStart = entryCount;
            if (pass == BattleHitExecutionPass.Character)
                characterPassCaptureCount++;
            else
                objectPassCaptureCount++;

            if (UsesDataOrientedScheduling)
            {
                CaptureDataOrientedPass(
                    tickIndex,
                    pass,
                    skipProvenEmptyBaseCharacters,
                    passProvenEmpty);
                return;
            }

            foreach (LF2Entity attacker in world.ActiveEntitiesByRuntimeSlotForModule)
            {
                attackerVisitCount++;
                if (!Participates(attacker, tickIndex, pass))
                    continue;

                int attackerSlot = attacker.Runtime.SlotIndex;
                if (!world.TryGetCurrentRuntimeHandle(
                        attackerSlot,
                        attacker,
                        out RuntimeEntityHandle attackerHandle))
                {
                    RecordFailure(
                        BattleHitExecutionPlanFailureReason.AttackerHandleNotCurrent,
                        attackerSlot,
                        -1);
                    continue;
                }

                RuntimeEntityHandle[] participants = pass == BattleHitExecutionPass.Character
                    ? characterParticipants
                    : objectParticipants;
                int participantIndex = pass == BattleHitExecutionPass.Character
                    ? characterParticipantCount++
                    : objectParticipantCount++;
                participants[participantIndex] = attackerHandle;
                int[] participantEntryStarts =
                    pass == BattleHitExecutionPass.Character
                        ? characterParticipantEntryStarts
                        : objectParticipantEntryStarts;
                int[] participantEntryCounts =
                    pass == BattleHitExecutionPass.Character
                        ? characterParticipantEntryCounts
                        : objectParticipantEntryCounts;
                int participantEntryStart = entryCount;
                participantEntryStarts[participantIndex] = participantEntryStart;
                participantEntryCounts[participantIndex] = 0;

                if (!world.SceneQuery.TryGetCollisionCandidateRange(
                        attacker,
                        out CollisionCandidateRange candidates))
                {
                    RecordFailure(
                        BattleHitExecutionPlanFailureReason.CandidateSourceUnavailable,
                        attackerSlot,
                        -1);
                    continue;
                }

                int candidateCount = candidates.Count;
                if (candidateCount <= 0)
                    continue;
                if (candidateCount > HitCandidateMaximum ||
                    attacker.Runtime.HitCandidateCount != candidateCount)
                {
                    RecordFailure(
                        BattleHitExecutionPlanFailureReason.CandidateCountMismatch,
                        attackerSlot,
                        -1);
                    continue;
                }

                LF2FrameData frame = attacker.GetCollisionFrameData();
                if (frame?.itrs == null)
                {
                    RecordFailure(
                        BattleHitExecutionPlanFailureReason.AttackerFrameUnavailable,
                        attackerSlot,
                        -1);
                    continue;
                }

                plannedAttackerCount++;
                for (int candidateOrdinal = 0;
                     candidateOrdinal < candidateCount;
                     candidateOrdinal++)
                {
                    if (!candidates.TryGet(candidateOrdinal, out SceneQueryHit hit))
                    {
                        RecordFailure(
                            BattleHitExecutionPlanFailureReason.CandidateReadFailed,
                            attackerSlot,
                            candidateOrdinal);
                        continue;
                    }

                    int itrIndex = hit.ItrIndex;
                    if (itrIndex < 0 || itrIndex >= frame.itrs.Count)
                    {
                        RecordFailure(
                            BattleHitExecutionPlanFailureReason.ItrIndexOutOfRange,
                            attackerSlot,
                            candidateOrdinal);
                        continue;
                    }

                    if (entryCount >= entries.Length)
                    {
                        RecordFailure(
                            BattleHitExecutionPlanFailureReason.CapacityExceeded,
                            attackerSlot,
                            candidateOrdinal);
                        return;
                    }

                    RuntimeEntityHandle targetHandle = RuntimeEntityHandle.Invalid;
                    LF2Entity target = world.FindEntityByRuntimeSlotForQuery(hit.TargetSlot);
                    if (target != null)
                    {
                        world.TryGetCurrentRuntimeHandle(
                            hit.TargetSlot,
                            target,
                            out targetHandle);
                    }

                    InteractionArea sourceItr = frame.itrs[itrIndex];
                    InteractionArea recordedItr = hit.RuntimeItr;
                    entries[entryCount++] = new Entry(
                        pass,
                        attackerHandle,
                        attacker.Runtime.StableId,
                        attacker.Runtime.PrevFrame2,
                        candidateOrdinal,
                        hit.TargetSlot,
                        targetHandle,
                        itrIndex,
                        hit.BodyX,
                        recordedItr?.kind ?? sourceItr.kind,
                        Fingerprint(sourceItr),
                        Fingerprint(recordedItr),
                        hit.ZeroAttackerHpOnConsume,
                        hit.ReleaseHeavyHeldTargetOnConsume,
                        hit.PairSnapshot);
                    plannedCandidateCount++;
                }

                participantEntryCounts[participantIndex] =
                    entryCount - participantEntryStart;
            }

            int passEntryCount = entryCount - passEntryStart;
            if (pass == BattleHitExecutionPass.Character)
            {
                characterEntryStart = passEntryStart;
                characterEntryCount = passEntryCount;
            }
            else
            {
                objectEntryStart = passEntryStart;
                objectEntryCount = passEntryCount;
            }
        }

        private void CaptureDataOrientedPass(
            int tickIndex,
            BattleHitExecutionPass pass,
            bool skipProvenEmptyBaseCharacters,
            bool passProvenEmpty)
        {
            if (passProvenEmpty)
            {
                if (pass == BattleHitExecutionPass.Character)
                {
                    characterEntryStart = 0;
                    characterEntryCount = 0;
                }
                else
                {
                    objectEntryStart = 0;
                    objectEntryCount = 0;
                }
                return;
            }

            RuntimeEntityHandle[] participants =
                pass == BattleHitExecutionPass.Character
                    ? characterParticipants
                    : objectParticipants;
            int[] participantEntryStarts =
                pass == BattleHitExecutionPass.Character
                    ? characterParticipantEntryStarts
                    : objectParticipantEntryStarts;
            int[] participantEntryCounts =
                pass == BattleHitExecutionPass.Character
                    ? characterParticipantEntryCounts
                    : objectParticipantEntryCounts;
            CollisionCandidateRange[] participantCandidateRanges =
                pass == BattleHitExecutionPass.Character
                    ? characterParticipantCandidateRanges
                    : objectParticipantCandidateRanges;

            foreach (LF2Entity attacker in world.ActiveEntitiesByRuntimeSlotForModule)
            {
                attackerVisitCount++;
                if (!Participates(attacker, tickIndex, pass))
                    continue;

                if (pass == BattleHitExecutionPass.Character &&
                    skipProvenEmptyBaseCharacters &&
                    attacker.GetType() == typeof(LF2Character) &&
                    attacker.Runtime.HitCandidateCount == 0 &&
                    attacker.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
                {
                    continue;
                }

                int attackerSlot = attacker.Runtime.SlotIndex;
                if (!world.TryGetCurrentRuntimeHandle(
                        attackerSlot,
                        attacker,
                        out RuntimeEntityHandle attackerHandle))
                {
                    RecordFailure(
                        BattleHitExecutionPlanFailureReason.AttackerHandleNotCurrent,
                        attackerSlot,
                        -1);
                    continue;
                }

                if (!world.SceneQuery.TryGetCollisionCandidateRange(
                        attacker,
                        out CollisionCandidateRange candidates))
                {
                    RecordFailure(
                        BattleHitExecutionPlanFailureReason.CandidateSourceUnavailable,
                        attackerSlot,
                        -1);
                    continue;
                }

                int candidateCount = candidates.Count;
                if (candidateCount < 0 ||
                    candidateCount > HitCandidateMaximum ||
                    attacker.Runtime.HitCandidateCount != candidateCount)
                {
                    RecordFailure(
                        BattleHitExecutionPlanFailureReason.CandidateCountMismatch,
                        attackerSlot,
                        -1);
                    continue;
                }

                if (candidateCount > 0)
                {
                    LF2FrameData frame = attacker.GetCollisionFrameData();
                    if (frame?.itrs == null)
                    {
                        RecordFailure(
                            BattleHitExecutionPlanFailureReason.AttackerFrameUnavailable,
                            attackerSlot,
                            -1);
                        continue;
                    }
                    plannedAttackerCount++;
                    plannedCandidateCount += candidateCount;
                }

                int participantIndex = pass == BattleHitExecutionPass.Character
                    ? characterParticipantCount++
                    : objectParticipantCount++;
                participants[participantIndex] = attackerHandle;
                participantEntryStarts[participantIndex] = 0;
                participantEntryCounts[participantIndex] = candidateCount;
                participantCandidateRanges[participantIndex] = candidates;
            }
        }

        internal bool BeginLegacyObservationPass(
            int tickIndex,
            BattleHitExecutionPass pass)
        {
            if (mode != BattleHitExecutionPlanMode.ShadowCompare)
                return false;

            bool passCaptured = pass == BattleHitExecutionPass.Character
                ? characterPassCaptured
                : objectPassCaptured;
            if (capturedTick != tickIndex || !passCaptured || observationPassActive)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationPassUnavailable,
                    -1,
                    -1);
                return false;
            }

            observationPassActive = true;
            observationPass = pass;
            observationEntryStart = pass == BattleHitExecutionPass.Character
                ? characterEntryStart
                : objectEntryStart;
            observationExpectedCount = pass == BattleHitExecutionPass.Character
                ? characterEntryCount
                : objectEntryCount;
            observationReadCount = 0;
            ResetPendingPreprocessExpectation();
            ResetPendingFirstBodyResponseExpectation();
            ResetConsumeEffectsObservation();
            ResetPendingWriterEffectExpectation();
            ResetPendingLifecycleEffectExpectation();
            ResetPendingDispatchExpectation();
            activeDispositionEntryIndex = -1;
            observationPassCount++;
            return true;
        }

        internal bool TryValidateDataOrientedPass(
            int tickIndex,
            BattleHitExecutionPass pass)
        {
            if (!UsesDataOrientedScheduling || capturedTick != tickIndex ||
                !currentTickPlanValid)
            {
                return false;
            }

            bool captured = pass == BattleHitExecutionPass.Character
                ? characterPassCaptured
                : objectPassCaptured;
            if (!captured)
                return false;

            RuntimeEntityHandle[] participants = pass == BattleHitExecutionPass.Character
                ? characterParticipants
                : objectParticipants;
            CollisionCandidateRange[] participantCandidateRanges =
                pass == BattleHitExecutionPass.Character
                    ? characterParticipantCandidateRanges
                    : objectParticipantCandidateRanges;
            int count = pass == BattleHitExecutionPass.Character
                ? characterParticipantCount
                : objectParticipantCount;
            for (int index = 0; index < count; index++)
            {
                if (!world.TryResolveRuntimeHandle(
                        participants[index],
                        out LF2Entity entity) ||
                    !Participates(entity, tickIndex, pass) ||
                    !entity.TryGetBattleHitCandidateConsumer(pass, out _))
                {
                    return false;
                }

                int[] participantEntryCounts =
                    pass == BattleHitExecutionPass.Character
                        ? characterParticipantEntryCounts
                        : objectParticipantEntryCounts;
                int candidateCount = participantEntryCounts[index];
                if (candidateCount < 0 ||
                    candidateCount > HitCandidateMaximum ||
                    participantCandidateRanges[index].Count != candidateCount)
                    return false;

                if (candidateCount > 0)
                {
                    LF2FrameData frame = entity.GetCollisionFrameData();
                    if (frame?.itrs == null)
                        return false;
                }
            }

            return true;
        }

        internal int GetDataOrientedParticipantCount(BattleHitExecutionPass pass)
        {
            return pass == BattleHitExecutionPass.Character
                ? characterParticipantCount
                : objectParticipantCount;
        }

        internal bool TryGetDataOrientedParticipant(
            BattleHitExecutionPass pass,
            int index,
            out LF2Entity entity,
            out CollisionCandidateRange candidates)
        {
            RuntimeEntityHandle[] participants = pass == BattleHitExecutionPass.Character
                ? characterParticipants
                : objectParticipants;
            int count = pass == BattleHitExecutionPass.Character
                ? characterParticipantCount
                : objectParticipantCount;
            if ((uint)index >= (uint)count)
            {
                entity = null;
                candidates = default;
                return false;
            }

            CollisionCandidateRange[] participantCandidateRanges =
                pass == BattleHitExecutionPass.Character
                    ? characterParticipantCandidateRanges
                    : objectParticipantCandidateRanges;
            candidates = participantCandidateRanges[index];
            return world.TryResolveRuntimeHandle(participants[index], out entity);
        }

        internal void ObserveLegacyCandidateRead(
            RuntimeEntityHandle attackerHandle,
            int candidateOrdinal,
            in SceneQueryHit hit)
        {
            if (!ShouldObserveLegacyCandidateRead)
                return;

            CompletePendingFirstBodyResponseExpectation();
            CompletePendingWriterEffectExpectation();
            CompletePendingLifecycleEffectExpectation();
            CompletePendingDispatchExpectation();
            CompletePendingConsumeEffectsExpectation();
            CompleteActiveDispositionExpectation();
            activePreprocessEntryIndex = -1;
            CompletePendingPreprocessExpectation();

            int expectedOffset = observationReadCount;
            observationReadCount++;
            observedCandidateCount++;
            if (expectedOffset >= observationExpectedCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationEntryUnexpected,
                    attackerHandle.Slot,
                    candidateOrdinal);
                return;
            }

            Entry expected = entries[observationEntryStart + expectedOffset];
            if (expected.Pass != observationPass ||
                expected.AttackerHandle != attackerHandle ||
                expected.CandidateOrdinal != candidateOrdinal ||
                expected.TargetSlot != hit.TargetSlot ||
                expected.ItrIndex != hit.ItrIndex ||
                expected.RecordedItrFingerprint != Fingerprint(hit.RuntimeItr) ||
                expected.ZeroAttackerHpOnConsume != hit.ZeroAttackerHpOnConsume ||
                expected.ReleaseHeavyHeldTargetOnConsume !=
                    hit.ReleaseHeavyHeldTargetOnConsume ||
                expected.PairSnapshot != hit.PairSnapshot)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationEntryMismatch,
                    attackerHandle.Slot,
                    candidateOrdinal);
                return;
            }

            PreparePreprocessExpectation(
                observationEntryStart + expectedOffset,
                in expected,
                in hit);
            activeDispositionEntryIndex = observationEntryStart + expectedOffset;
        }

        internal void ObserveLegacyPreprocess(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            bool zeroAttackerHpOnConsume,
            bool releaseHeavyHeldTargetOnConsume)
        {
            if (!ShouldObserveLegacyPreprocess)
                return;

            if (!pendingPreprocessExpected || pendingPreprocessEntryIndex < 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationPreprocessUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = pendingPreprocessEntryIndex;
            Entry expected = entries[entryIndex];
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            ulong actualFingerprint = Fingerprint(resolvedItr);
            observedPreprocessCount++;

            expected.RecordObservedPreprocess(
                actualFingerprint,
                resolvedItr?.kind ?? int.MinValue,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
            entries[entryIndex] = expected;

            bool attackerCurrent = attacker != null &&
                world.TryGetCurrentRuntimeHandle(
                    attackerSlot,
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                attackerHandle == expected.AttackerHandle;
            if (!attackerCurrent ||
                targetSlot != expected.TargetSlot ||
                actualFingerprint != pendingExpectedResolvedItrFingerprint ||
                zeroAttackerHpOnConsume != pendingExpectedZeroAttackerHp ||
                releaseHeavyHeldTargetOnConsume !=
                    pendingExpectedReleaseHeavyHeldTarget)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationPreprocessMismatch,
                    attackerSlot,
                    expected.CandidateOrdinal);
            }

            activePreprocessEntryIndex = entryIndex;
            ResetPendingPreprocessExpectation();
        }

        internal void ObserveLegacyDisposition(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition observedDisposition)
        {
            if (!ShouldObserveLegacyDisposition)
                return;

            if (activeDispositionEntryIndex < 0 ||
                activeDispositionEntryIndex >= entryCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationDispositionUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = activeDispositionEntryIndex;
            Entry entry = entries[entryIndex];
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            BattleHitCandidateDisposition expectedDisposition =
                ProjectExpectedDisposition(attacker, target, entry.ExpectedResolvedItrKind);
            bool attackerCurrent = attacker != null &&
                world.TryGetCurrentRuntimeHandle(
                    attackerSlot,
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                attackerHandle == entry.AttackerHandle;
            bool observationMatches = attackerCurrent &&
                targetSlot == entry.TargetSlot &&
                resolvedItr != null &&
                Fingerprint(resolvedItr) == entry.ObservedResolvedItrFingerprint &&
                entry.PreprocessObserved &&
                expectedDisposition == observedDisposition;

            observedDispositionCount++;
            entry.RecordDisposition(expectedDisposition, observedDisposition);
            entries[entryIndex] = entry;
            if (!observationMatches)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationDispositionMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            activeDispositionEntryIndex = -1;
        }

        internal void PrepareLegacyFirstBodyResponseAttemptObservation(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            if (!ShouldObserveLegacyFirstBodyResponseAttempt)
                return;

            CompletePendingFirstBodyResponseExpectation();
            if (activePreprocessEntryIndex < 0 ||
                activePreprocessEntryIndex >= entryCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason
                        .ObservationFirstBodyResponseUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = activePreprocessEntryIndex;
            Entry entry = entries[entryIndex];
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            bool attackerCurrent = attacker != null &&
                world.TryGetCurrentRuntimeHandle(
                    attackerSlot,
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                attackerHandle == entry.AttackerHandle;
            bool identityMatches = attackerCurrent &&
                targetSlot == entry.TargetSlot &&
                resolvedItr != null &&
                resolvedItr.kind == 0 &&
                Fingerprint(resolvedItr) == entry.ObservedResolvedItrFingerprint &&
                entry.DispositionObserved &&
                entry.ExpectedDisposition == disposition &&
                BattleFirstBodyResponseWriter
                    .IsUnarmoredContinuationDisposition(disposition);
            if (!identityMatches)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason
                        .ObservationFirstBodyResponsePreStateMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            FirstBodyResponseAttemptSnapshot projection =
                CaptureFirstBodyResponseAttemptSnapshot(attacker, target);
            if (!ProjectFirstBodyResponseAttempt(
                    attacker,
                    target,
                    resolvedItr,
                    ref projection))
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason
                        .ObservationFirstBodyResponsePreStateMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            pendingFirstBodyResponseEntryIndex = entryIndex;
            pendingFirstBodyResponseExpected = true;
            pendingExpectedFirstBodyResponseSnapshot = projection;
            entry.RecordExpectedFirstBodyResponseAttempt(
                projection.AttemptEligible,
                projection.ResponseKind);
            entries[entryIndex] = entry;
        }

        internal void ObserveLegacyFirstBodyResponseAttempt(
            LF2Entity attacker,
            LF2Entity target,
            in BattleFirstBodyResponseAttemptResult attempt)
        {
            if (!ShouldObserveLegacyFirstBodyResponseAttempt)
                return;

            if (!pendingFirstBodyResponseExpected ||
                pendingFirstBodyResponseEntryIndex < 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason
                        .ObservationFirstBodyResponseUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = pendingFirstBodyResponseEntryIndex;
            Entry entry = entries[entryIndex];
            FirstBodyResponseAttemptSnapshot actual =
                CaptureFirstBodyResponseAttemptSnapshot(attacker, target);
            actual.AttemptEligible = attempt.Eligible;
            actual.ResponseKind = attempt.Response.Kind;
            ulong differenceMask = DifferenceMask(
                in pendingExpectedFirstBodyResponseSnapshot,
                in actual);
            lastFirstBodyResponseDifferenceMask = differenceMask;
            observedFirstBodyResponseAttemptCount++;
            entry.RecordObservedFirstBodyResponseAttempt(
                attempt.Eligible,
                attempt.Response.Kind);
            entries[entryIndex] = entry;
            if (differenceMask != 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason
                        .ObservationFirstBodyResponseMismatch,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    entry.CandidateOrdinal);
            }

            if (attempt.Applied)
            {
                observedAbortTerminationCount++;
                SkipRemainingEntriesForAttacker(entry.AttackerHandle);
            }

            ResetPendingFirstBodyResponseExpectation();
        }

        internal void PrepareLegacyConsumeEffectsObservation(
            LF2Entity attacker,
            LF2Entity target)
        {
            if (!ShouldObserveLegacyConsumeEffects)
                return;

            CompletePendingConsumeEffectsExpectation();
            if (activePreprocessEntryIndex < 0 ||
                activePreprocessEntryIndex >= entryCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationConsumeEffectsUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = activePreprocessEntryIndex;
            Entry entry = entries[entryIndex];
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            bool attackerCurrent = attacker != null &&
                world.TryGetCurrentRuntimeHandle(
                    attackerSlot,
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                attackerHandle == entry.AttackerHandle;
            if (!attackerCurrent || targetSlot != entry.TargetSlot)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationConsumeEffectsPreStateMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            int heldTargetSlot = target?.Runtime?.ResolveActiveHeldSlotIndex() ?? -1;
            ConsumeEffectsSnapshot expected = CaptureConsumeEffectsSnapshot(
                attacker,
                target,
                heldTargetSlot);
            bool preStateValid = ProjectConsumeEffects(
                attacker,
                target,
                entry,
                ref expected);
            if (!preStateValid)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationConsumeEffectsPreStateMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            pendingConsumeEffectsEntryIndex = entryIndex;
            pendingConsumeEffectsExpected = true;
            pendingConsumeEffectsHeldTargetSlot = heldTargetSlot;
            pendingExpectedConsumeEffectsSnapshot = expected;
            pendingExpectedConsumeEffectsFingerprint = Fingerprint(in expected);
            entry.RecordExpectedConsumeEffects(
                pendingExpectedConsumeEffectsFingerprint,
                expected.RngState,
                expected.RngCallCount);
            entries[entryIndex] = entry;
        }

        internal void ObserveLegacyConsumeEffects(
            LF2Entity attacker,
            LF2Entity target)
        {
            if (!ShouldObserveLegacyConsumeEffects)
                return;

            if (!pendingConsumeEffectsExpected ||
                pendingConsumeEffectsEntryIndex < 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationConsumeEffectsUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = pendingConsumeEffectsEntryIndex;
            Entry entry = entries[entryIndex];
            ConsumeEffectsSnapshot actual = CaptureConsumeEffectsSnapshot(
                attacker,
                target,
                pendingConsumeEffectsHeldTargetSlot);
            ulong actualFingerprint = Fingerprint(in actual);
            observedConsumeEffectsCount++;
            entry.RecordObservedConsumeEffects(
                actualFingerprint,
                actual.RngState,
                actual.RngCallCount);
            entries[entryIndex] = entry;

            ulong differenceMask = DifferenceMask(
                in pendingExpectedConsumeEffectsSnapshot,
                in actual);
            lastConsumeEffectsDifferenceMask = differenceMask;
            if (differenceMask != 0 ||
                actualFingerprint != pendingExpectedConsumeEffectsFingerprint)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationConsumeEffectsMismatch,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    entry.CandidateOrdinal);
            }

            ResetPendingConsumeEffectsExpectation();
        }

        internal void PrepareLegacyDispatchObservation(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (!ShouldObserveLegacyDispatch)
                return;

            CompletePendingDispatchExpectation();
            if (activePreprocessEntryIndex < 0 ||
                activePreprocessEntryIndex >= entryCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationDispatchUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = activePreprocessEntryIndex;
            Entry entry = entries[entryIndex];
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            bool attackerCurrent = attacker != null &&
                world.TryGetCurrentRuntimeHandle(
                    attackerSlot,
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                attackerHandle == entry.AttackerHandle;
            if (!attackerCurrent ||
                targetSlot != entry.TargetSlot ||
                resolvedItr == null ||
                Fingerprint(resolvedItr) != entry.ObservedResolvedItrFingerprint)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationDispatchMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            pendingDispatchEntryIndex = entryIndex;
            pendingDispatchExpected = true;
            pendingExpectedAbortAfterSuccessfulDispatch =
                ProjectAbortAfterSuccessfulDispatch(target, resolvedItr?.kind ?? int.MinValue);
        }

        internal void PrepareLegacyWriterEffectObservation(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            if (!ShouldObserveLegacyWriterEffect)
                return;

            CompletePendingWriterEffectExpectation();
            if (activePreprocessEntryIndex < 0 ||
                activePreprocessEntryIndex >= entryCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationWriterEffectUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = activePreprocessEntryIndex;
            Entry entry = entries[entryIndex];
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            bool attackerCurrent = attacker != null &&
                world.TryGetCurrentRuntimeHandle(
                    attackerSlot,
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                attackerHandle == entry.AttackerHandle;
            bool identityMatches = attackerCurrent &&
                targetSlot == entry.TargetSlot &&
                resolvedItr != null &&
                Fingerprint(resolvedItr) == entry.ObservedResolvedItrFingerprint &&
                entry.DispositionObserved &&
                entry.ExpectedDisposition == disposition;
            if (!identityMatches)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationWriterEffectPreStateMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            WriterEffectSnapshot projection = CaptureWriterEffectSnapshot(attacker, target);
            if (!ProjectWriterEffect(
                    attacker,
                    target,
                    resolvedItr,
                    disposition,
                    ref projection))
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationWriterEffectPreStateMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            pendingWriterEffectEntryIndex = entryIndex;
            pendingWriterEffectExpected = true;
            pendingExpectedWriterEffectSnapshot = projection;
        }

        internal void ObserveLegacyWriterEffect(
            LF2Entity attacker,
            LF2Entity target)
        {
            if (!ShouldObserveLegacyWriterEffect)
                return;

            if (!pendingWriterEffectExpected || pendingWriterEffectEntryIndex < 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationWriterEffectUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            Entry entry = entries[pendingWriterEffectEntryIndex];
            WriterEffectSnapshot actual =
                CaptureWriterEffectSnapshotWithCreditOverride(
                attacker,
                target,
                pendingExpectedWriterEffectSnapshot.HeldTargetHandle.Slot,
                pendingExpectedWriterEffectSnapshot.StandardCreditHandle.Slot);
            ulong differenceMask = DifferenceMask(
                in pendingExpectedWriterEffectSnapshot,
                in actual);
            lastWriterEffectDifferenceMask = differenceMask;
            observedWriterEffectCount++;
            if (differenceMask != 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationWriterEffectMismatch,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    entry.CandidateOrdinal);
            }

            ResetPendingWriterEffectExpectation();
        }

        internal void PrepareLegacyLifecycleEffectObservation(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            if (!ShouldObserveLegacyLifecycleEffect)
                return;

            CompletePendingLifecycleEffectExpectation();
            if (activePreprocessEntryIndex < 0 ||
                activePreprocessEntryIndex >= entryCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationLifecycleEffectUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            int entryIndex = activePreprocessEntryIndex;
            Entry entry = entries[entryIndex];
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            bool attackerCurrent = attacker != null &&
                world.TryGetCurrentRuntimeHandle(
                    attackerSlot,
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                attackerHandle == entry.AttackerHandle;
            bool identityMatches = attackerCurrent &&
                targetSlot == entry.TargetSlot &&
                resolvedItr != null &&
                Fingerprint(resolvedItr) == entry.ObservedResolvedItrFingerprint &&
                entry.DispositionObserved &&
                entry.ExpectedDisposition == disposition &&
                CanProjectLegacyLifecycleEffect(
                    attacker,
                    target,
                    resolvedItr,
                    disposition);
            if (!identityMatches ||
                !world.TryGetRuntimeSlotReadOnlyView(
                    attackerSlot,
                    out RuntimeSlotTable.ReadOnlySlotView beforeView) ||
                !beforeView.Claimed ||
                beforeView.Generation != entry.AttackerHandle.Generation ||
                !ReferenceEquals(beforeView.Entity, attacker))
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationLifecycleEffectPreStateMismatch,
                    attackerSlot,
                    entry.CandidateOrdinal);
            }

            pendingLifecycleEffectEntryIndex = entryIndex;
            pendingLifecycleEffectExpected = true;
            pendingExpectedLifecycleEffectSnapshot = new LifecycleEffectSnapshot
            {
                AttackerHandle = entry.AttackerHandle,
                SlotClaimed = false,
                SlotGeneration = NextGeneration(entry.AttackerHandle.Generation),
                SlotOccupant = null,
                AttackerRuntimeSlot = -1,
            };
        }

        internal void ObserveLegacyLifecycleEffect(LF2Entity attacker)
        {
            if (!ShouldObserveLegacyLifecycleEffect)
                return;

            if (!pendingLifecycleEffectExpected || pendingLifecycleEffectEntryIndex < 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationLifecycleEffectUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            Entry entry = entries[pendingLifecycleEffectEntryIndex];
            int slot = pendingExpectedLifecycleEffectSnapshot.AttackerHandle.Slot;
            RuntimeSlotTable.ReadOnlySlotView view =
                world.RuntimeSlotTableForModules.GetReadOnlyView(slot);
            var actual = new LifecycleEffectSnapshot
            {
                AttackerHandle = pendingExpectedLifecycleEffectSnapshot.AttackerHandle,
                SlotClaimed = view.Claimed,
                SlotGeneration = view.Generation,
                SlotOccupant = view.Entity,
                AttackerRuntimeSlot = attacker?.Runtime?.SlotIndex ?? int.MinValue,
            };
            ulong differenceMask = DifferenceMask(
                in pendingExpectedLifecycleEffectSnapshot,
                in actual);
            lastLifecycleEffectDifferenceMask = differenceMask;
            observedLifecycleEffectCount++;
            if (differenceMask != 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationLifecycleEffectMismatch,
                    slot,
                    entry.CandidateOrdinal);
            }

            ResetPendingLifecycleEffectExpectation();
        }

        internal void ObserveLegacyDispatch(
            LF2Entity attacker,
            bool dispatchSucceeded,
            bool terminatedRemainingCandidates)
        {
            if (!ShouldObserveLegacyDispatch)
                return;

            if (!pendingDispatchExpected || pendingDispatchEntryIndex < 0)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationDispatchUnexpected,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    -1);
                return;
            }

            Entry entry = entries[pendingDispatchEntryIndex];
            bool expectedTermination =
                pendingExpectedAbortAfterSuccessfulDispatch && dispatchSucceeded;
            observedDispatchCount++;
            if (terminatedRemainingCandidates != expectedTermination)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationDispatchMismatch,
                    attacker?.Runtime?.SlotIndex ?? -1,
                    entry.CandidateOrdinal);
            }

            if (terminatedRemainingCandidates)
            {
                observedAbortTerminationCount++;
                SkipRemainingEntriesForAttacker(entry.AttackerHandle);
            }

            ResetPendingDispatchExpectation();
        }

        internal void EndLegacyObservationPass()
        {
            if (!observationPassActive)
                return;

            CompletePendingWriterEffectExpectation();
            CompletePendingLifecycleEffectExpectation();
            CompletePendingDispatchExpectation();
            CompletePendingFirstBodyResponseExpectation();
            CompletePendingConsumeEffectsExpectation();
            CompleteActiveDispositionExpectation();
            CompletePendingPreprocessExpectation();

            if (observationReadCount != observationExpectedCount)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationEntryMissing,
                    -1,
                    observationReadCount);
            }

            observationPassActive = false;
            observationPass = default;
            observationEntryStart = 0;
            observationExpectedCount = 0;
            observationReadCount = 0;
            ResetPendingPreprocessExpectation();
            ResetPendingFirstBodyResponseExpectation();
            ResetConsumeEffectsObservation();
            ResetPendingWriterEffectExpectation();
            ResetPendingLifecycleEffectExpectation();
            ResetPendingDispatchExpectation();
            activeDispositionEntryIndex = -1;
        }

        internal bool TryGetEntry(
            int index,
            out BattleHitExecutionPlanEntryView view)
        {
            if ((uint)index >= (uint)entryCount)
            {
                view = default;
                return false;
            }

            view = entries[index].ToView();
            return true;
        }

        private void BeginTickIfNeeded(int tickIndex)
        {
            if (capturedTick == tickIndex)
                return;

            capturedTick = tickIndex;
            entryCount = 0;
            characterPassCaptured = false;
            objectPassCaptured = false;
            characterEntryStart = 0;
            characterEntryCount = 0;
            objectEntryStart = 0;
            objectEntryCount = 0;
            characterParticipantCount = 0;
            objectParticipantCount = 0;
            observationPassActive = false;
            observationPass = default;
            observationEntryStart = 0;
            observationExpectedCount = 0;
            observationReadCount = 0;
            ResetPendingPreprocessExpectation();
            ResetPendingFirstBodyResponseExpectation();
            ResetConsumeEffectsObservation();
            ResetPendingWriterEffectExpectation();
            ResetPendingLifecycleEffectExpectation();
            ResetPendingDispatchExpectation();
            activeDispositionEntryIndex = -1;
            currentTickPlanValid = true;
        }

        private bool TryMarkPassCaptured(BattleHitExecutionPass pass)
        {
            if (pass == BattleHitExecutionPass.Character)
            {
                if (characterPassCaptured)
                    return false;
                characterPassCaptured = true;
                return true;
            }

            if (objectPassCaptured)
                return false;
            objectPassCaptured = true;
            return true;
        }

        private static bool Participates(
            LF2Entity attacker,
            int tickIndex,
            BattleHitExecutionPass pass)
        {
            if (attacker?.Runtime == null)
                return false;

            if (pass == BattleHitExecutionPass.Character)
            {
                return attacker.SupportsPostInteractionPhase() &&
                       tickIndex >= attacker.Runtime.SuppressPostInteractionUntilTick;
            }

            return attacker.SupportsObjectInteractionPhase() &&
                   tickIndex >= attacker.Runtime.SuppressObjectInteractionUntilTick;
        }

        private void RecordFailure(
            BattleHitExecutionPlanFailureReason reason,
            int attackerSlot,
            int candidateOrdinal)
        {
            currentTickPlanValid = false;
            failureCount++;
            if (firstFailureReason != BattleHitExecutionPlanFailureReason.None)
                return;

            firstFailureReason = reason;
            firstFailureAttackerSlot = attackerSlot;
            firstFailureCandidateOrdinal = candidateOrdinal;
        }

        private void RecordObservationFailure(
            BattleHitExecutionPlanFailureReason reason,
            int attackerSlot,
            int candidateOrdinal)
        {
            observationMismatchCount++;
            RecordFailure(reason, attackerSlot, candidateOrdinal);
        }

        private void PreparePreprocessExpectation(
            int entryIndex,
            in Entry expected,
            in SceneQueryHit hit)
        {
            if (!world.TryResolveRuntimeHandle(
                    expected.AttackerHandle,
                    out LF2Entity attacker))
            {
                return;
            }

            LF2FrameData frame = attacker.GetCollisionFrameData();
            if (frame?.itrs == null ||
                expected.ItrIndex < 0 ||
                expected.ItrIndex >= frame.itrs.Count)
            {
                return;
            }

            LF2Entity target = world.FindEntityByRuntimeSlotForQuery(hit.TargetSlot);
            if (target?.Runtime == null)
                return;

            InteractionArea sourceItr = frame.itrs[expected.ItrIndex];
            if (sourceItr == null)
                return;

            ItrProjection projection = new ItrProjection(sourceItr);
            ProjectRuntimeItr(
                attacker,
                target,
                ref projection,
                out bool zeroAttackerHp,
                out bool releaseHeavyHeldTarget);

            pendingPreprocessEntryIndex = entryIndex;
            pendingPreprocessExpected = true;
            pendingExpectedResolvedItrFingerprint = Fingerprint(in projection);
            pendingExpectedZeroAttackerHp = zeroAttackerHp;
            pendingExpectedReleaseHeavyHeldTarget = releaseHeavyHeldTarget;

            Entry mutableEntry = entries[entryIndex];
            mutableEntry.RecordExpectedPreprocess(
                pendingExpectedResolvedItrFingerprint,
                projection.Kind,
                zeroAttackerHp,
                releaseHeavyHeldTarget);
            entries[entryIndex] = mutableEntry;
        }

        private void CompletePendingPreprocessExpectation()
        {
            if (!pendingPreprocessExpected)
                return;

            Entry expected = entries[pendingPreprocessEntryIndex];
            RecordObservationFailure(
                BattleHitExecutionPlanFailureReason.ObservationPreprocessMissing,
                expected.AttackerHandle.Slot,
                expected.CandidateOrdinal);
            ResetPendingPreprocessExpectation();
        }

        private void ResetPendingPreprocessExpectation()
        {
            pendingPreprocessEntryIndex = -1;
            pendingPreprocessExpected = false;
            pendingExpectedResolvedItrFingerprint = 0;
            pendingExpectedZeroAttackerHp = false;
            pendingExpectedReleaseHeavyHeldTarget = false;
        }

        private void CompletePendingFirstBodyResponseExpectation()
        {
            if (!pendingFirstBodyResponseExpected)
                return;

            Entry expected = entries[pendingFirstBodyResponseEntryIndex];
            RecordObservationFailure(
                BattleHitExecutionPlanFailureReason
                    .ObservationFirstBodyResponseMissing,
                expected.AttackerHandle.Slot,
                expected.CandidateOrdinal);
            ResetPendingFirstBodyResponseExpectation();
        }

        private void ResetPendingFirstBodyResponseExpectation()
        {
            pendingFirstBodyResponseEntryIndex = -1;
            pendingFirstBodyResponseExpected = false;
            pendingExpectedFirstBodyResponseSnapshot = default;
        }

        private void CompletePendingConsumeEffectsExpectation()
        {
            if (!pendingConsumeEffectsExpected)
                return;

            Entry expected = entries[pendingConsumeEffectsEntryIndex];
            RecordObservationFailure(
                BattleHitExecutionPlanFailureReason.ObservationConsumeEffectsMissing,
                expected.AttackerHandle.Slot,
                expected.CandidateOrdinal);
            ResetPendingConsumeEffectsExpectation();
        }

        private void ResetPendingConsumeEffectsExpectation()
        {
            pendingConsumeEffectsEntryIndex = -1;
            pendingConsumeEffectsExpected = false;
            pendingConsumeEffectsHeldTargetSlot = -1;
            pendingExpectedConsumeEffectsFingerprint = 0;
            pendingExpectedConsumeEffectsSnapshot = default;
        }

        private void ResetConsumeEffectsObservation()
        {
            activePreprocessEntryIndex = -1;
            ResetPendingConsumeEffectsExpectation();
        }

        private void CompleteActiveDispositionExpectation()
        {
            if (activeDispositionEntryIndex < 0 ||
                activeDispositionEntryIndex >= entryCount)
            {
                activeDispositionEntryIndex = -1;
                return;
            }

            Entry entry = entries[activeDispositionEntryIndex];
            if (currentTickPlanValid &&
                entry.PreprocessObserved &&
                !entry.DispositionObserved)
            {
                RecordObservationFailure(
                    BattleHitExecutionPlanFailureReason.ObservationDispositionMissing,
                    entry.AttackerHandle.Slot,
                    entry.CandidateOrdinal);
            }

            activeDispositionEntryIndex = -1;
        }

        private void CompletePendingDispatchExpectation()
        {
            if (!pendingDispatchExpected)
                return;

            Entry expected = entries[pendingDispatchEntryIndex];
            RecordObservationFailure(
                BattleHitExecutionPlanFailureReason.ObservationDispatchMissing,
                expected.AttackerHandle.Slot,
                expected.CandidateOrdinal);
            ResetPendingDispatchExpectation();
        }

        private void ResetPendingDispatchExpectation()
        {
            pendingDispatchEntryIndex = -1;
            pendingDispatchExpected = false;
            pendingExpectedAbortAfterSuccessfulDispatch = false;
        }

        private void CompletePendingWriterEffectExpectation()
        {
            if (!pendingWriterEffectExpected)
                return;

            Entry expected = entries[pendingWriterEffectEntryIndex];
            RecordObservationFailure(
                BattleHitExecutionPlanFailureReason.ObservationWriterEffectMissing,
                expected.AttackerHandle.Slot,
                expected.CandidateOrdinal);
            ResetPendingWriterEffectExpectation();
        }

        private void ResetPendingWriterEffectExpectation()
        {
            pendingWriterEffectEntryIndex = -1;
            pendingWriterEffectExpected = false;
            pendingExpectedWriterEffectSnapshot = default;
        }

        private void CompletePendingLifecycleEffectExpectation()
        {
            if (!pendingLifecycleEffectExpected)
                return;

            Entry expected = entries[pendingLifecycleEffectEntryIndex];
            RecordObservationFailure(
                BattleHitExecutionPlanFailureReason.ObservationLifecycleEffectMissing,
                expected.AttackerHandle.Slot,
                expected.CandidateOrdinal);
            ResetPendingLifecycleEffectExpectation();
        }

        private void ResetPendingLifecycleEffectExpectation()
        {
            pendingLifecycleEffectEntryIndex = -1;
            pendingLifecycleEffectExpected = false;
            pendingExpectedLifecycleEffectSnapshot = default;
        }

        private static bool ProjectAbortAfterSuccessfulDispatch(
            LF2Entity target,
            int resolvedKind)
        {
            if (target == null || resolvedKind != 0)
                return false;

            int currentOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            if (currentOid != 300)
                return false;

            int currentFrameId = target.Frame?.N ?? 0;
            LF2FrameData currentFrame = target.GetFrameDataById(currentFrameId);
            LF2FrameData futureFrame = target.GetFrameDataById(currentFrameId + 6);
            return currentFrame?.bodies != null &&
                   currentFrame.bodies.Count > 0 &&
                   currentFrame.bodies[0].X > 1000 &&
                   futureFrame?.bodies != null &&
                   futureFrame.bodies.Count > 0;
        }

        private static BattleHitCandidateDisposition ProjectExpectedDisposition(
            LF2Entity attacker,
            LF2Entity target,
            int resolvedKind)
        {
            if (!CanConsumeProjectedCandidate(attacker, target))
                return BattleHitCandidateDisposition.RejectedByConsumeGate;

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            if (resolvedKind == 0 && targetOid == 300)
                return BattleHitCandidateDisposition.Oid300Redirect;

            switch (resolvedKind)
            {
                case 0:
                case 9:
                    return BattleHitCandidateDisposition.Damage;
                case 6:
                    return BattleHitCandidateDisposition.HitConfirm;
                case 8:
                    return BattleHitCandidateDisposition.Kind8;
                case 14:
                    return BattleHitCandidateDisposition.Kind14;
                case 15:
                    return BattleHitCandidateDisposition.Kind15;
                case 10:
                case 11:
                case 17:
                case 18:
                    return BattleHitCandidateDisposition.Kind10Or11;
                case 1:
                    return BattleHitCandidateDisposition.Kind1Grab;
                case 3:
                    return BattleHitCandidateDisposition.Kind3Grab;
                case 2:
                    return BattleHitCandidateDisposition.Pickup;
                default:
                    return BattleHitCandidateDisposition.Unsupported;
            }
        }

        private static bool CanConsumeProjectedCandidate(
            LF2Entity attacker,
            LF2Entity target)
        {
            if (attacker?.Runtime == null || target?.Runtime == null || target == attacker)
                return false;
            if (target.Runtime.PendingFlushDestroy || target.FrameCache == null)
                return false;

            int attackerSlot = attacker.Runtime.SlotIndex;
            return attackerSlot < 0 || target.ItrVrestTest(attackerSlot, true);
        }

        private void SkipRemainingEntriesForAttacker(RuntimeEntityHandle attackerHandle)
        {
            while (observationReadCount < observationExpectedCount)
            {
                int entryIndex = observationEntryStart + observationReadCount;
                if (entries[entryIndex].AttackerHandle != attackerHandle)
                    break;

                observationReadCount++;
                skippedCandidateCountAfterAbort++;
            }
        }

        private FirstBodyResponseAttemptSnapshot
            CaptureFirstBodyResponseAttemptSnapshot(
                LF2Entity attacker,
                LF2Entity target)
        {
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            NTSD28NativeRandomScalarState random =
                world.NativeRandom?.CaptureScalarState() ?? default;
            return new FirstBodyResponseAttemptSnapshot
            {
                AttackerHandle = ResolveCurrentHandle(attackerSlot, attacker),
                TargetHandle = ResolveCurrentHandle(targetSlot, target),
                AttemptEligible = false,
                ResponseKind = BattleFirstBodyResponseKind.None,
                AttackerFrame = attacker?.Frame?.N ?? int.MinValue,
                AttackerRuntimeFrame = attacker?.Runtime?.Frame ?? int.MinValue,
                AttackerFrameCounter =
                    attacker?.Runtime?.FrameWaitCounter ?? int.MinValue,
                AttackerGroup = attacker?.Runtime?.RelationTeam ?? int.MinValue,
                AttackerFrameDelay = attacker?.Runtime?.FrameDelay ?? int.MinValue,
                AttackerInputScore =
                    attacker?.Runtime?.InputScoreTotal348 ?? int.MinValue,
                TargetFrame = target?.Frame?.N ?? int.MinValue,
                TargetRuntimeFrame = target?.Runtime?.Frame ?? int.MinValue,
                TargetFrameCounter =
                    target?.Runtime?.FrameWaitCounter ?? int.MinValue,
                TargetGroup = target?.Runtime?.RelationTeam ?? int.MinValue,
                TargetFrameDelay = target?.Runtime?.FrameDelay ?? int.MinValue,
                TargetHp = target?.Health?.HP ?? int.MinValue,
                TargetHpBound = target?.Health?.HPBound ?? int.MinValue,
                TargetRuntimeArmorHp =
                    target?.Runtime?.RuntimeArmorHp118 ?? int.MinValue,
                TargetInputHpConsumed =
                    target?.Runtime?.InputHpConsumedTotal34C ?? int.MinValue,
                RngCrtState = random.CrtState,
                RngCrtCalls = random.CrtCalls,
                RngTableSeed = random.TableSeed,
                RngCounter = random.SynchronizedCounter,
                RngIndex = random.SynchronizedIndex,
                RngCalls = random.SynchronizedCalls,
                RngLastCallSite = random.LastSynchronizedCallSite,
                RngTableHash = random.SynchronizedTableHash,
                RngGeneration = random.SynchronizedGeneration,
            };
        }

        private bool ProjectFirstBodyResponseAttempt(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction,
            ref FirstBodyResponseAttemptSnapshot projection)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || interaction == null ||
                world.NativeRandom == null)
            {
                return false;
            }

            BattleFirstBodyResponseAttemptResult attempt =
                BattleFirstBodyResponseWriter.ResolveAttempt(
                    world,
                    attacker,
                    target,
                    interaction,
                    false,
                    0);
            projection.AttemptEligible = attempt.Eligible;
            projection.ResponseKind = attempt.Response.Kind;
            if (!attempt.Eligible)
                return true;

            if (attempt.Response.NeedsRoll)
            {
                NTSD28SynchronizedRandomCursor cursor =
                    world.NativeRandom.CaptureSynchronizedCursor();
                int roll = cursor.Next(
                    (uint)attempt.Response.Chance,
                    100);
                attempt = BattleFirstBodyResponseWriter.ResolveAttempt(
                    world,
                    attacker,
                    target,
                    interaction,
                    true,
                    roll);
                projection.RngCounter = cursor.Counter;
                projection.RngIndex = cursor.Index;
                projection.RngCalls = cursor.Calls;
                projection.RngLastCallSite = cursor.LastCallSite;
            }

            projection.AttemptEligible = attempt.Eligible;
            projection.ResponseKind = attempt.Response.Kind;
            if (!attempt.Applied)
                return true;

            BattleFirstBodyResponseResult response = attempt.Response;
            if (attempt.BrokenArmorBeforeResponse)
                projection.TargetRuntimeArmorHp = -1;
            if (response.WriteTargetGroup)
                projection.TargetGroup = response.TargetGroup;
            if (response.WriteTargetAction)
            {
                projection.TargetFrame = response.TargetAction;
                projection.TargetRuntimeFrame = response.TargetAction;
                if (response.ResetTargetFrameCounter)
                    projection.TargetFrameCounter = 0;
            }
            if (response.WriteAttackerAction)
            {
                projection.AttackerFrame = response.AttackerAction;
                projection.AttackerRuntimeFrame = response.AttackerAction;
                if (response.ResetAttackerFrameCounter)
                    projection.AttackerFrameCounter = 0;
            }
            if (response.ApplyHold)
            {
                projection.AttackerFrameDelay = 3;
                projection.TargetFrameDelay = -3;
            }
            if (response.ApplyManualDamage)
            {
                int injury = response.ManualDamage;
                projection.TargetHp = Math.Max(
                    0,
                    unchecked(projection.TargetHp - injury));
                projection.TargetInputHpConsumed = unchecked(
                    projection.TargetInputHpConsumed + injury);
                projection.AttackerInputScore = unchecked(
                    projection.AttackerInputScore + injury);
            }

            return true;
        }

        private ConsumeEffectsSnapshot CaptureConsumeEffectsSnapshot(
            LF2Entity attacker,
            LF2Entity target,
            int heldTargetSlot)
        {
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            RuntimeEntityHandle attackerHandle = ResolveCurrentHandle(
                attackerSlot,
                attacker);
            RuntimeEntityHandle targetHandle = ResolveCurrentHandle(
                targetSlot,
                target);
            LF2Entity heldTarget = heldTargetSlot >= 0
                ? world.FindEntityByRuntimeSlotForQuery(heldTargetSlot)
                : null;
            RuntimeEntityHandle heldTargetHandle = ResolveCurrentHandle(
                heldTargetSlot,
                heldTarget);

            return new ConsumeEffectsSnapshot
            {
                AttackerHandle = attackerHandle,
                AttackerHp = attacker?.Health?.HP ?? int.MinValue,
                TargetHandle = targetHandle,
                TargetLinkState = target?.Runtime?.LinkState ?? int.MinValue,
                TargetTargetSlot = target?.Runtime?.TargetSlotIndex ?? int.MinValue,
                TargetVrestAgainstAttacker =
                    targetSlot >= 0 && attackerSlot >= 0
                        ? world.GetRawRestVrest(targetSlot, attackerSlot)
                        : int.MinValue,
                TargetVrestAgainstHeld =
                    targetSlot >= 0 && heldTargetSlot >= 0
                        ? world.GetRawRestVrest(targetSlot, heldTargetSlot)
                        : int.MinValue,
                HeldTargetHandle = heldTargetHandle,
                HeldTargetLinkState = heldTarget?.Runtime?.LinkState ?? int.MinValue,
                HeldTargetHolderSlot = heldTarget?.Runtime?.HolderStableId ?? int.MinValue,
                HeldTargetFrame = heldTarget?.Frame?.N ?? int.MinValue,
                HeldTargetVy = heldTarget?.Runtime?.Vy ?? double.NaN,
                RngState = world.Rng.State,
                RngCallCount = world.Rng.CallCount,
            };
        }

        private WriterEffectSnapshot CaptureWriterEffectSnapshot(
            LF2Entity attacker,
            LF2Entity target,
            int heldTargetSlotOverride = -1)
        {
            return CaptureWriterEffectSnapshotWithCreditOverride(
                attacker,
                target,
                heldTargetSlotOverride,
                int.MinValue);
        }

        private WriterEffectSnapshot CaptureWriterEffectSnapshotWithCreditOverride(
            LF2Entity attacker,
            LF2Entity target,
            int heldTargetSlotOverride,
            int standardCreditSlotOverride = int.MinValue)
        {
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            int holderSlot = -1;
            LF2Entity holder = null;
            int activeHolderSlot = attacker?.Runtime?.ResolveActiveHolderSlotIndex() ?? -1;
            LF2Entity activeHolder = activeHolderSlot >= 0
                ? world.FindEntityByRuntimeSlotForQuery(activeHolderSlot)
                : null;
            int damageStatIndex = target?.Unk344 ?? -1;
            int heldTargetSlot = heldTargetSlotOverride >= 0
                ? heldTargetSlotOverride
                : target?.Runtime?.ResolveActiveHeldSlotIndex() ?? -1;
            LF2Entity heldTarget = heldTargetSlot >= 0
                ? world.FindEntityByRuntimeSlotForQuery(heldTargetSlot)
                : null;
            LF2Entity standardCredit = standardCreditSlotOverride != int.MinValue
                ? (standardCreditSlotOverride >= 0
                    ? world.FindEntityByRuntimeSlotForQuery(
                        standardCreditSlotOverride)
                    : null)
                : BattleDamageWriter.ResolveNativeStandardHitCredit(
                    world,
                    attacker);
            LF2Entity hitRecordOwner = null;
            if (attacker?.Runtime != null && target?.Runtime != null)
            {
                hitRecordOwner = attacker.Runtime.ZInt > target.Runtime.ZInt ||
                                 (attacker.Runtime.ZInt == target.Runtime.ZInt &&
                                  attackerSlot > targetSlot)
                    ? attacker
                    : target;
            }
            int hitRecordCount = hitRecordOwner?.HitRecordCount ?? int.MinValue;
            int lastHitRecordIndex = hitRecordCount > 0 ? hitRecordCount - 1 : -1;
            int pendingSoundCount = world.PendingSounds?.Count ?? 0;
            PendingSoundEvent lastPendingSound = pendingSoundCount > 0
                ? world.PendingSounds[pendingSoundCount - 1]
                : default;
            return new WriterEffectSnapshot
            {
                AttackerHandle = ResolveCurrentHandle(attackerSlot, attacker),
                AttackerFrame = attacker?.Frame?.N ?? int.MinValue,
                AttackerRuntimeFrame = attacker?.Runtime?.Frame ?? int.MinValue,
                AttackerX = attacker?.Runtime?.X ?? double.NaN,
                AttackerY = attacker?.Runtime?.Y ?? double.NaN,
                AttackerZ = attacker?.Runtime?.Z ?? double.NaN,
                AttackerXInt = attacker?.Runtime?.XInt ?? int.MinValue,
                AttackerYInt = attacker?.Runtime?.YInt ?? int.MinValue,
                AttackerZInt = attacker?.Runtime?.ZInt ?? int.MinValue,
                AttackerVx = attacker?.Runtime?.Vx ?? double.NaN,
                AttackerVy = attacker?.Runtime?.Vy ?? double.NaN,
                AttackerVz = attacker?.Runtime?.Vz ?? double.NaN,
                AttackerKnockbackVx = attacker?.KnockbackVx ?? double.NaN,
                AttackerKnockbackVy = attacker?.KnockbackVy ?? double.NaN,
                AttackerKnockbackVz = attacker?.KnockbackVz ?? double.NaN,
                AttackerFacing = attacker?.Dirh() < 0 ? 1 : 0,
                AttackerRelationTeam = attacker?.Runtime?.RelationTeam ?? int.MinValue,
                AttackerLinkState = attacker?.Runtime?.LinkState ?? int.MinValue,
                AttackerTargetSlot = attacker?.Runtime?.TargetSlotIndex ?? int.MinValue,
                AttackerCaughtSlot = attacker?.Runtime?.CaughtSlotIndex ?? int.MinValue,
                AttackerCaughtDuration = attacker?.Runtime?.CaughtDuration ?? int.MinValue,
                AttackerHeldWeaponSlot = attacker?.Runtime?.HeldWeaponStableId ?? int.MinValue,
                AttackerPickupCount = attacker?.Runtime?.PickupCount ?? int.MinValue,
                AttackerAttackingCounter = attacker?.Runtime?.AttackingCounter ?? int.MinValue,
                AttackerFrameDelay = attacker?.FrameDelay ?? int.MinValue,
                AttackerAttackExempt = attacker?.AttackExempt ?? int.MinValue,
                AttackerItrArest = attacker?.ItrRest?.Arest ?? int.MinValue,
                AttackerHp = attacker?.Health?.HP ?? int.MinValue,
                AttackerKind4SourceCount =
                    attacker?.Runtime?.Kind4SourceCount92 ?? int.MinValue,
                StandardCreditHandle = ResolveCurrentHandle(
                    standardCredit?.Runtime?.SlotIndex ?? -1,
                    standardCredit),
                StandardCreditInputScore =
                    standardCredit?.Runtime?.InputScoreTotal348 ?? int.MinValue,
                StandardCreditKnockoutCount =
                    standardCredit?.Runtime?.KnockoutCount358 ?? int.MinValue,
                TargetHandle = ResolveCurrentHandle(targetSlot, target),
                TargetFrame = target?.Frame?.N ?? int.MinValue,
                TargetRuntimeFrame = target?.Runtime?.Frame ?? int.MinValue,
                TargetPrevFrame = target?.Frame?.Prev ?? int.MinValue,
                TargetWaitCounter = target?.Trans?.WaitCounter ?? int.MinValue,
                TargetObjectId = target?.ObjectId ?? int.MinValue,
                TargetDataObjectId = target?.FrameCache?.Wrapper?.characterId ?? int.MinValue,
                TargetDataObjectType = target?.GetCurrentDataObjectTypeForSimulation() ?? int.MinValue,
                TargetX = target?.Runtime?.X ?? double.NaN,
                TargetY = target?.Runtime?.Y ?? double.NaN,
                TargetZ = target?.Runtime?.Z ?? double.NaN,
                TargetXInt = target?.Runtime?.XInt ?? int.MinValue,
                TargetYInt = target?.Runtime?.YInt ?? int.MinValue,
                TargetZInt = target?.Runtime?.ZInt ?? int.MinValue,
                TargetVx = target?.Runtime?.Vx ?? double.NaN,
                TargetVy = target?.Runtime?.Vy ?? double.NaN,
                TargetVz = target?.Runtime?.Vz ?? double.NaN,
                TargetKnockbackVx = target?.KnockbackVx ?? double.NaN,
                TargetKnockbackVy = target?.KnockbackVy ?? double.NaN,
                TargetKnockbackVz = target?.KnockbackVz ?? double.NaN,
                TargetFacing = target?.Dirh() < 0 ? 1 : 0,
                TargetLinkState = target?.Runtime?.LinkState ?? int.MinValue,
                TargetTargetSlot = target?.Runtime?.TargetSlotIndex ?? int.MinValue,
                TargetCatcherSlot = target?.Runtime?.CatcherSlotIndex ?? int.MinValue,
                TargetCatchSourceSlot90 =
                    target?.Runtime?.CatchSourceSlot90 ?? int.MinValue,
                TargetEnvironmentState320 = target?.Runtime?.EnvironmentState320 ?? int.MinValue,
                TargetImpactSourceSlot164 = target?.Runtime?.ImpactSourceSlot164 ?? int.MinValue,
                TargetHolderSlot = target?.Runtime?.HolderStableId ?? int.MinValue,
                TargetHolderCopySlot = target?.Runtime?.HolderCopySlotIndex ?? int.MinValue,
                TargetOwnerSlot = target?.Runtime?.OwnerSlotIndex ?? int.MinValue,
                TargetRelationTeam = target?.Runtime?.RelationTeam ?? int.MinValue,
                TargetWeaponFlightCounter = target?.Runtime?.WeaponFlightCounter ?? int.MinValue,
                TargetWeaponCount = target?.WeaponCount ?? int.MinValue,
                TargetFall = target?.Runtime?.Fall ?? int.MinValue,
                TargetHitConfirmCounter = target?.HitConfirmCounter ?? int.MinValue,
                TargetHitConfirm2 = target?.Runtime?.HitConfirm2 ?? int.MinValue,
                TargetSpecialHitLatch0EB = target?.Runtime?.SpecialHitLatch0EB ?? false,
                TargetAnimCounter = target?.Runtime?.AnimCounter ?? int.MinValue,
                TargetHealTimer = target?.HealTimer ?? int.MinValue,
                TargetXBoundPositive = target?.Runtime?.XBoundPositive ?? false,
                TargetXBoundNegative = target?.Runtime?.XBoundNegative ?? false,
                TargetZBoundPositive = target?.Runtime?.ZBoundPositive ?? false,
                TargetZBoundNegative = target?.Runtime?.ZBoundNegative ?? false,
                HolderHandle = ResolveCurrentHandle(holderSlot, holder),
                HolderComboCountAtk = holder?.ComboCountAtk ?? int.MinValue,
                HolderKillStat = holder?.KillStat ?? int.MinValue,
                ActiveHolderHandle = ResolveCurrentHandle(activeHolderSlot, activeHolder),
                ActiveHolderFrameDelay = activeHolder?.FrameDelay ?? int.MinValue,
                TargetHp = target?.Health?.HP ?? int.MinValue,
                TargetHpBound = target?.Health?.HPBound ?? int.MinValue,
                TargetPp = target?.Health?.PP ?? int.MinValue,
                TargetRuntimeArmorHp =
                    target?.Runtime?.RuntimeArmorHp118 ?? int.MinValue,
                TargetInputHpConsumedTotal =
                    target?.Runtime?.InputHpConsumedTotal34C ?? int.MinValue,
                TargetInputMpConsumedTotal =
                    target?.Runtime?.InputMpConsumedTotal350 ?? int.MinValue,
                TargetComboCountVic = target?.ComboCountVic ?? int.MinValue,
                TargetAttackingCounter = target?.AttackingCounter ?? int.MinValue,
                TargetFrameDelay = target?.FrameDelay ?? int.MinValue,
                TargetHitCount = target?.HitCount ?? int.MinValue,
                TargetHitStateCount = target?.HitStateCount ?? int.MinValue,
                TargetKillStat = world.KillStats != null &&
                                 damageStatIndex > 0 &&
                                 damageStatIndex < world.KillStats.Length
                    ? world.KillStats[damageStatIndex]
                    : int.MinValue,
                TargetDamageStat = world.DamageStats != null &&
                                   damageStatIndex > 0 &&
                                   damageStatIndex < world.DamageStats.Length
                    ? world.DamageStats[damageStatIndex]
                    : int.MinValue,
                TargetVrestAgainstAttacker = targetSlot >= 0 && attackerSlot >= 0
                    ? world.GetRawRestVrest(targetSlot, attackerSlot)
                    : int.MinValue,
                TargetVrestAgainstHeld = targetSlot >= 0 && heldTargetSlot >= 0
                    ? world.GetRawRestVrest(targetSlot, heldTargetSlot)
                    : int.MinValue,
                HeldTargetVrestAgainstAttacker = heldTargetSlot >= 0 && attackerSlot >= 0
                    ? world.GetRawRestVrest(heldTargetSlot, attackerSlot)
                    : int.MinValue,
                AttackerVrestAgainstHeld = attackerSlot >= 0 && heldTargetSlot >= 0
                    ? world.GetRawRestVrest(attackerSlot, heldTargetSlot)
                    : int.MinValue,
                AttackerVrestAgainstAttacker = attackerSlot >= 0
                    ? world.GetRawRestVrest(attackerSlot, attackerSlot)
                    : int.MinValue,
                HeldTargetHandle = ResolveCurrentHandle(heldTargetSlot, heldTarget),
                HeldTargetLinkState = heldTarget?.Runtime?.LinkState ?? int.MinValue,
                HeldTargetHolderSlot = heldTarget?.Runtime?.HolderStableId ?? int.MinValue,
                HeldTargetFrame = heldTarget?.Frame?.N ?? int.MinValue,
                HeldTargetRuntimeFrame = heldTarget?.Runtime?.Frame ?? int.MinValue,
                HeldTargetVy = heldTarget?.Runtime?.Vy ?? double.NaN,
                HitRecordOwnerHandle = ResolveCurrentHandle(
                    hitRecordOwner?.Runtime?.SlotIndex ?? -1,
                    hitRecordOwner),
                HitRecordCount = hitRecordCount,
                HitRecordDamage = lastHitRecordIndex >= 0
                    ? hitRecordOwner.GetHitRecordAge(lastHitRecordIndex)
                    : int.MinValue,
                HitRecordX = lastHitRecordIndex >= 0
                    ? hitRecordOwner.GetHitRecordX(lastHitRecordIndex)
                    : int.MinValue,
                HitRecordZ = lastHitRecordIndex >= 0
                    ? hitRecordOwner.GetHitRecordZ(lastHitRecordIndex)
                    : int.MinValue,
                RngState = world.Rng?.State ?? 0,
                RngCallCount = world.Rng?.CallCount ?? 0,
                PendingSoundCount = pendingSoundCount,
                PendingSoundFingerprint = FingerprintPendingSounds(world),
                PendingSoundCue = pendingSoundCount > 0 ? lastPendingSound.Cue : null,
                PendingSoundWorldX = pendingSoundCount > 0 ? lastPendingSound.WorldX : int.MinValue,
                PendingSoundTick = pendingSoundCount > 0 ? lastPendingSound.Tick : int.MinValue,
                QueuedSoundEventCount = world.QueuedSoundEventCountForDiagnostics,
                RejectedSoundEventCount = world.BattleBuffersForServices.RejectedSoundEventCount,
            };
        }

        private static bool ProjectWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition,
            ref WriterEffectSnapshot projection)
        {
            if (resolvedItr == null || attacker?.Runtime == null || target?.Runtime == null)
                return false;

            switch (disposition)
            {
                case BattleHitCandidateDisposition.Kind1Grab:
                    return ProjectGrabWriterEffect(
                        attacker,
                        target,
                        resolvedItr,
                        resetAttackerPosition: false,
                        strictKind3: false,
                        ref projection);

                case BattleHitCandidateDisposition.Kind3Grab:
                    return ProjectGrabWriterEffect(
                        attacker,
                        target,
                        resolvedItr,
                        resetAttackerPosition: true,
                        strictKind3: true,
                        ref projection);

                case BattleHitCandidateDisposition.Pickup:
                    return ProjectPickupWriterEffect(
                        attacker,
                        target,
                        resolvedItr.kind,
                        ref projection);

                case BattleHitCandidateDisposition.HitConfirm:
                    projection.TargetHitConfirmCounter = 3;
                    return true;

                case BattleHitCandidateDisposition.Kind8:
                    if (attacker.Frame == null || target.Health == null)
                        return false;
                    SimulationWorld kind8World = target.Match ?? attacker.Match;
                    if (kind8World == null ||
                        !BattleKind8EligibilityResolver.Resolve(
                            resolvedItr.bdefend,
                            resolvedItr.respond,
                            target.GetCurrentDataObjectTypeForSimulation(),
                            attacker.RelationTeam,
                            target.RelationTeam,
                            attacker.Runtime.OwnerSlotIndex,
                            target.Runtime.OwnerSlotIndex,
                            kind8World.BattleGameModeId).Accepted)
                    {
                        return false;
                    }
                    if (resolvedItr.injury != 0)
                    {
                        projection.TargetHealTimer = unchecked(
                            resolvedItr.injury + 1000);
                    }
                    int kind8CaughtAct =
                        BattleKind8ControlRelationWriter.FirstOrZero(
                            resolvedItr.caughtact);
                    if (kind8CaughtAct != 0)
                    {
                        projection.TargetPp = unchecked(
                            projection.TargetPp + kind8CaughtAct);
                    }
                    if (resolvedItr.dvx != 999)
                    {
                        projection.AttackerFrame = resolvedItr.dvx;
                        projection.AttackerRuntimeFrame = resolvedItr.dvx;
                    }
                    int kind8SyncMode =
                        BattleKind8ControlRelationWriter.NormalizeSyncMode(
                            resolvedItr.dvy);
                    if (kind8SyncMode != -1)
                    {
                        if (kind8SyncMode != 1)
                            projection.AttackerX = target.Runtime.X;
                        if (kind8SyncMode != 0)
                            projection.AttackerY = target.Runtime.Y;
                        projection.AttackerZ = target.Runtime.Z + 1.0;
                    }
                    return true;

                case BattleHitCandidateDisposition.Kind14:
                    if (attacker.Runtime.XInt > target.Runtime.XInt + 5 &&
                        (target.Runtime.Vx > 0.0 || target.KnockbackVx > 0.0))
                    {
                        projection.TargetXBoundPositive = true;
                    }
                    else if (attacker.Runtime.XInt < target.Runtime.XInt - 5 &&
                             (target.Runtime.Vx < 0.0 || target.KnockbackVx < 0.0))
                    {
                        projection.TargetXBoundNegative = true;
                    }

                    if (attacker.Runtime.ZInt > target.Runtime.ZInt + 2 &&
                        (target.Runtime.Vz > 0.0 || target.KnockbackVz > 0.0))
                    {
                        projection.TargetZBoundPositive = true;
                    }
                    else if (attacker.Runtime.ZInt < target.Runtime.ZInt - 2 &&
                             (target.Runtime.Vz < 0.0 || target.KnockbackVz < 0.0))
                    {
                        projection.TargetZBoundNegative = true;
                    }
                    return true;

                case BattleHitCandidateDisposition.Kind10Or11:
                    return ProjectNativeImpactWriterEffect(
                        attacker,
                        target,
                        resolvedItr,
                        ref projection);

                case BattleHitCandidateDisposition.Kind15:
                    return ProjectKind15WriterEffect(
                        attacker,
                        target,
                        ref projection);

                case BattleHitCandidateDisposition.Damage:
                    if (target.GetCurrentDataObjectTypeForSimulation() !=
                        (int)LF2ObjectType.Character)
                    {
                        if (target.GetCurrentDataObjectTypeForSimulation() ==
                            (int)LF2ObjectType.SpecialAttack)
                        {
                            if (CanProjectType3Kind9DamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr))
                            {
                                return ProjectType3Kind9DamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr,
                                    ref projection);
                            }

                            if (CanProjectType3StateSyncDamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr))
                            {
                                return ProjectType3StateSyncDamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr,
                                    ref projection);
                            }

                            if (BattleDamageWriter
                                    .IsNativeLockedKindTransformCandidate(
                                        attacker,
                                        target) &&
                                CanProjectStandardType3DamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr))
                            {
                                return ProjectStandardType3DamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr,
                                    ref projection);
                            }

                            if (CanProjectType3D1IdentityDamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr))
                            {
                                return ProjectType3D1IdentityDamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr,
                                    ref projection);
                            }

                            if (CanProjectType3ActiveD1IdentityDamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr))
                            {
                                return ProjectType3ActiveD1IdentityDamageWriterEffect(
                                    attacker,
                                    target,
                                    resolvedItr,
                                    ref projection);
                            }

                            return ProjectStandardType3DamageWriterEffect(
                                attacker,
                                target,
                                resolvedItr,
                                ref projection);
                        }

                        if (CanProjectNonConvertedKind9ObjectDamageWriterEffect(
                                attacker,
                                target,
                                resolvedItr))
                        {
                            return ProjectNonConvertedKind9ObjectDamageWriterEffect(
                                attacker,
                                target,
                                resolvedItr,
                                ref projection);
                        }

                        return ProjectStandardObjectDamageWriterEffect(
                            attacker,
                            target,
                            resolvedItr,
                            ref projection);
                    }

                    BattleOrdinaryCharacterDamageRoute route =
                        BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                            target.Match ?? attacker.Match,
                            attacker,
                            target,
                            resolvedItr);
                    return route.UsesReducedHit
                        ? ProjectAlternateCharacterDamageWriterEffect(
                            attacker,
                            target,
                            resolvedItr,
                            ref projection)
                        : route.UsesUnarmoredHit &&
                          ProjectStandardCharacterDamageWriterEffect(
                            attacker,
                            target,
                            resolvedItr,
                            ref projection);

                default:
                    return false;
            }
        }

        private static bool CanProjectDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (target?.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                if (target.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.SpecialAttack)
                {
                    return CanProjectType3Kind9DamageWriterEffect(
                               attacker,
                               target,
                               resolvedItr) ||
                           (BattleDamageWriter
                                .IsNativeLockedKindTransformCandidate(
                                    attacker,
                                    target) &&
                            CanProjectStandardType3DamageWriterEffect(
                                attacker,
                                target,
                                resolvedItr)) ||
                           CanProjectType3D1IdentityDamageWriterEffect(
                               attacker,
                               target,
                               resolvedItr) ||
                           CanProjectType3ActiveD1IdentityDamageWriterEffect(
                               attacker,
                               target,
                               resolvedItr) ||
                           CanProjectType3StateSyncDamageWriterEffect(
                               attacker,
                               target,
                               resolvedItr) ||
                           CanProjectStandardType3DamageWriterEffect(
                               attacker,
                               target,
                               resolvedItr);
                }

                return CanProjectNonConvertedKind9ObjectDamageWriterEffect(
                           attacker,
                           target,
                           resolvedItr) ||
                       CanProjectStandardObjectDamageWriterEffect(
                           attacker,
                           target,
                           resolvedItr);
            }

            BattleOrdinaryCharacterDamageRoute route =
                BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                    target.Match ?? attacker.Match,
                    attacker,
                    target,
                    resolvedItr);
            return route.UsesReducedHit
                ? CanProjectAlternateCharacterDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr)
                : route.UsesUnarmoredHit &&
                  CanProjectStandardCharacterDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr);
        }

        private static bool CanProjectStandardObjectDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                resolvedItr == null || resolvedItr.kind != 0 ||
                target is not LF2WeaponBase)
            {
                return false;
            }

            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            if (targetType != (int)LF2ObjectType.LightWeapon &&
                targetType != (int)LF2ObjectType.HeavyWeapon &&
                targetType != (int)LF2ObjectType.ThrowWeapon &&
                targetType != (int)LF2ObjectType.Drink)
            {
                return false;
            }

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            bool oid100Held =
                targetOid == 100 && target.Runtime.LinkState < 0;
            bool skipOid100KnockbackTail =
                target.Runtime.Vx > -5.0 &&
                target.Runtime.Vx < 5.0 &&
                resolvedItr.dvx == 0;
            bool applyOid100KnockbackTail =
                oid100Held && !skipOid100KnockbackTail;
            int attackerState = attacker.GetState();
            bool attackerState2000 = attackerState == LF2States.HeavyWeaponInSky;
            bool attackerState1002 = attackerState == LF2States.WeaponThrowing;
            bool attackerState3000 = attackerState == LF2States.ProjectileFlying;
            if (attackerState1002 && !HasFramesInRange(attacker, 0, 16))
                return false;
            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            if (attackerState3000 && attackerOid == 0xD1 &&
                (IsKarasuType3Oid(targetOid) ||
                 (targetOid == 0xD1 && target.Frame.N == 40)))
            {
                return false;
            }

            if (targetType == (int)LF2ObjectType.HeavyWeapon)
            {
                if (resolvedItr.fall <= 40 &&
                    target.Runtime.YInt >= 0 &&
                    resolvedItr.effect != 4)
                {
                    if (target.GetFrameDataById(20) == null)
                        return false;
                }
                else if (!HasFramesInRange(target, 0, 6))
                {
                    return false;
                }
            }
            else if (!HasFramesInRange(target, 0, 16))
            {
                return false;
            }

            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (attackerData == null || targetData == null ||
                attacker.ItrRest == null || target.ItrRest == null)
            {
                return false;
            }

            int requiredSounds = targetType == (int)LF2ObjectType.Drink ? 0 : 1;
            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.SpecialAttack &&
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound))
            {
                requiredSounds++;
            }
            if (!string.IsNullOrWhiteSpace(targetData.weapon_hit_sound))
                requiredSounds++;
            if (applyOid100KnockbackTail)
                requiredSounds++;
            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static bool CanProjectNonConvertedKind9ObjectDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                resolvedItr == null || resolvedItr.kind != 9 ||
                target is not LF2WeaponBase ||
                target.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character ||
                target.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int targetState = target.GetState();
            if (targetState == LF2States.WeaponThrowing ||
                targetState == LF2States.HeavyWeaponInSky)
            {
                return false;
            }

            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(1) == true;
        }

        private static bool CanProjectStandardType3DamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 || target is not LF2SpecialAttack ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int targetState = target.GetState();
            int attackerState = attacker.GetState();
            bool attackerState3000 = attackerState == LF2States.ProjectileFlying;
            bool attackerState1002 = attackerState == LF2States.WeaponThrowing;
            if (targetState == LF2States.ObjectFlying ||
                !IsSupportedType3Effect(resolvedItr.effect))
            {
                return false;
            }

            if (attackerState1002 && !HasFramesInRange(attacker, 0, 16))
                return false;

            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            if (attackerState3000 && attackerOid == 0xD1 &&
                (IsKarasuType3Oid(targetOid) ||
                 (targetOid == 0xD1 && target.Frame.N == 40)))
            {
                return false;
            }

            int attackerType = attacker.GetCurrentDataObjectTypeForSimulation();
            bool kindTransformCandidate =
                BattleDamageWriter.IsNativeLockedKindTransformCandidate(
                    attacker,
                    target);
            int type3Frame;
            if (kindTransformCandidate)
            {
                type3Frame = 40;
                if (attacker.FrameCache?.Wrapper == null ||
                    attacker.FrameCache.HasFrame(type3Frame) != true)
                {
                    return false;
                }
            }
            else
            {
                LF2FrameData responseFrame = target.Frame?.D;
                if (responseFrame == null)
                    return false;
                bool ordinaryFjPath =
                    (attackerType == (int)LF2ObjectType.Character ||
                     attacker.Runtime.LinkState < 0) &&
                    resolvedItr.effect != 2 &&
                    resolvedItr.effect != 20;
                type3Frame = ordinaryFjPath
                    ? responseFrame.hit_Fj
                    : responseFrame.hit_Uj;
                if (type3Frame == 0)
                    type3Frame = ordinaryFjPath ? 30 : 20;
            }
            if (!kindTransformCandidate &&
                target.GetFrameDataById(type3Frame) == null)
                return false;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (attackerData == null || targetData == null ||
                attacker.ItrRest == null || target.ItrRest == null)
            {
                return false;
            }

            int requiredSounds = 1;
            if (attackerType == (int)LF2ObjectType.SpecialAttack &&
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound))
            {
                requiredSounds++;
            }
            if (!string.IsNullOrWhiteSpace(targetData.weapon_hit_sound))
                requiredSounds++;
            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static bool CanProjectType3Kind9DamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                resolvedItr == null || resolvedItr.kind != 9 ||
                target is not LF2SpecialAttack ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int targetState = target.GetState();
            if (targetState == LF2States.WeaponThrowing ||
                targetState == LF2States.HeavyWeaponInSky)
            {
                return false;
            }

            int targetFrame = targetState == LF2States.ObjectFlying ? 40 : 30;
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (target.GetFrameDataById(targetFrame) == null ||
                targetData == null)
            {
                return false;
            }

            int requiredSounds = string.IsNullOrWhiteSpace(targetData.weapon_broken_sound)
                ? 1
                : 2;
            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static bool CanProjectType3StateSyncDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int attackerState = attacker.GetState();
            int targetState = target.GetState();
            bool matchingPair =
                (targetState == LF2States.ObjectFlying &&
                 attackerState == LF2States.ObjectFlying) ||
                (targetState == LF2States.ObjectExpanding &&
                 attackerState == LF2States.ObjectExpanding);
            if (!matchingPair)
            {
                return false;
            }

            int attackerResetAction = ResolveType3PairResetAction(
                attacker,
                attacker.Trans?.WaitCounter ?? attacker.Runtime.WaitCounter);
            int targetResetAction = ResolveType3PairResetAction(
                target,
                target.Trans?.WaitCounter ?? target.Runtime.WaitCounter);
            if (attacker.GetFrameDataById(attackerResetAction) == null ||
                target.GetFrameDataById(targetResetAction) == null)
            {
                return false;
            }

            return attacker.ItrRest != null && target.ItrRest != null;
        }

        private static bool CanProjectType3D1IdentityDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 || attacker is not LF2SpecialAttack ||
                target is not LF2SpecialAttack ||
                attacker.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            LF2CharacterDataWrapper replacement = target.ResolveRuntimeCharacterConfig(attackerOid);
            int previousState = target.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.GetFrameDataById(
                    target.Runtime.PrevFrame2)?.state ??
                target.Frame?.Prev2D?.state ?? 0;
            if (attackerOid != 0xD1 || !IsKarasuType3Oid(targetOid) ||
                target.GetState() != LF2States.Standing ||
                attacker.Runtime.LinkState < 0 ||
                target.Runtime.LinkState != 0 ||
                target.Runtime.CatcherSlotIndex >= 0 ||
                target.Runtime.YInt < 0 ||
                target.Health.HP <= 0 ||
                previousState == 13 || previous2State == 12 ||
                resolvedItr.effect != 0 || resolvedItr.bdefend == 100 ||
                attackerData == null || targetData == null ||
                replacement?.characterData == null ||
                !ReferenceEquals(replacement.characterData, attackerData) ||
                attacker.GetFrameDataById(40) == null ||
                attacker.ItrRest == null || target.ItrRest == null)
            {
                return false;
            }

            int attackerState = attacker.GetState();
            int replacementState = attacker.GetFrameDataById(40).state;
            bool plainIdentity = attackerState == LF2States.Standing &&
                                 replacementState != LF2States.ObjectFlying &&
                                 replacementState != LF2States.ObjectExpanding;
            bool expandingStateSync = attackerState == LF2States.ObjectExpanding &&
                                      replacementState == LF2States.ObjectExpanding &&
                                      attacker.GetFrameDataById(20) != null;
            if (!plainIdentity && !expandingStateSync)
                return false;

            int requiredSounds = 1;
            if (!string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound))
                requiredSounds++;
            if (!string.IsNullOrWhiteSpace(targetData.weapon_hit_sound))
                requiredSounds++;
            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static bool CanProjectType3ActiveD1IdentityDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 || target is not LF2SpecialAttack ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            int attackerType = attacker.GetCurrentDataObjectTypeForSimulation();
            bool oidD5HeldSpecial = attackerOid == 0xD5 &&
                                     attacker is LF2SpecialAttack &&
                                     attackerType == (int)LF2ObjectType.SpecialAttack &&
                                     attacker.Runtime.LinkState < 0;
            if (!oidD5HeldSpecial)
                return false;

            bool replacementUsesFrame20 =
                (resolvedItr.effect == 2 || resolvedItr.effect == 20);

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            int targetState = target.GetState();
            bool supportedTargetState = targetState == LF2States.Standing ||
                                        (resolvedItr.effect == 20 &&
                                         (targetState == LF2States.WeaponThrowing ||
                                          targetState == LF2States.HeavyWeaponInSky));
            int previousState = target.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.GetFrameDataById(
                    target.Runtime.PrevFrame2)?.state ??
                target.Frame?.Prev2D?.state ?? 0;
            if (!IsKarasuType3Oid(targetOid) ||
                 attacker.GetState() != LF2States.Standing ||
                 !supportedTargetState ||
                 target.Runtime.LinkState != 0 ||
                 target.Runtime.CatcherSlotIndex >= 0 ||
                 target.Runtime.YInt < 0 ||
                 target.Health.HP <= 0 ||
                 previousState == 13 || previous2State == 12 ||
                 resolvedItr.bdefend == 100 ||
                 attackerData == null || targetData == null ||
                 attacker.ItrRest == null || target.ItrRest == null ||
                 !TryResolveActiveType3IdentitySource(target.Match, 0xD1, out LF2Entity source) ||
                 target.ResolveRuntimeCharacterConfig(0xD1)?.characterData !=
                     LF2HitResolveRuntimeData.ResolveCharacterData(source))
            {
                return false;
            }

            int replacementFrame = replacementUsesFrame20 ? 20 : 30;
            LF2FrameData targetReplacementFrame = target.GetFrameDataById(replacementFrame);
            LF2FrameData sourceReplacementFrame = source.GetFrameDataById(replacementFrame);
            if (targetReplacementFrame == null || sourceReplacementFrame == null ||
                sourceReplacementFrame.state == LF2States.ObjectFlying ||
                sourceReplacementFrame.state == LF2States.ObjectExpanding)
            {
                return false;
            }

            int replacementDataType = source.GetCurrentDataObjectTypeForSimulation();
            bool characterDat = replacementDataType == (int)LF2ObjectType.Character;
            if (characterDat)
            {
                bool burningEffect = resolvedItr.effect == 2 ||
                                     resolvedItr.effect == 21 ||
                                     resolvedItr.effect == 22 ||
                                     (resolvedItr.effect == 20 &&
                                      sourceReplacementFrame.state != 18);
                bool freezeEffect = (resolvedItr.effect == 3 || resolvedItr.effect == 30) &&
                                    sourceReplacementFrame.state != 13;
                if (burningEffect && source.GetFrameDataById(203) == null)
                    return false;
                if (freezeEffect && source.GetFrameDataById(200) == null)
                    return false;
            }

            if (resolvedItr.effect >= 6000 && resolvedItr.effect < 7000 &&
                source.GetFrameDataById(resolvedItr.effect - 6000) == null)
            {
                return false;
            }

            if (ResolveActiveType3Holder(attacker) == null)
                return false;

            int requiredSounds = 1;
            if (attackerType == (int)LF2ObjectType.SpecialAttack &&
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound))
            {
                requiredSounds++;
            }
            if (!string.IsNullOrWhiteSpace(targetData.weapon_hit_sound))
                requiredSounds++;
            if (resolvedItr.effect == 23 ||
                (characterDat &&
                 (((resolvedItr.effect == 3 || resolvedItr.effect == 30) &&
                   sourceReplacementFrame.state != 13) ||
                  resolvedItr.effect == 2 || resolvedItr.effect == 21 ||
                  resolvedItr.effect == 22 ||
                  (resolvedItr.effect == 20 && sourceReplacementFrame.state != 18))))
            {
                requiredSounds++;
            }

            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static void ProjectNativeStandardHitRest(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction,
            ref WriterEffectSnapshot projection)
        {
            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            int timingReduction = targetWorld?.Runtime?.NativeStandardHitRest?.
                TimingReduction4A9FF4 ??
                NTSD28StandardHitRestRuntimeState
                    .DefaultTimingReduction4A9FF4;
            BattleStandardHitRestResult result =
                BattleStandardHitRestResolver.Resolve(
                    projection.AttackerFrameDelay,
                    projection.TargetFrameDelay,
                    interaction.recover,
                    attackerData?.definition_effect ?? 0,
                    targetData?.definition_effect ?? 0,
                    interaction.arest,
                    interaction.vrest,
                    timingReduction);

            projection.AttackerFrameDelay = result.AttackerHold;
            projection.TargetFrameDelay = result.TargetHold;
            projection.AttackerAttackExempt = result.Arest;
            projection.AttackerItrArest = result.Arest;
            if (result.ShouldWriteVrest)
                projection.TargetVrestAgainstAttacker = result.Vrest;
        }

        private static void ProjectNativeReducedHitRest(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction,
            ref WriterEffectSnapshot projection,
            LF2ArmorData selectedArmor = null)
        {
            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            int timingReduction = targetWorld?.Runtime?.NativeStandardHitRest?.
                TimingReduction4A9FF4 ??
                NTSD28StandardHitRestRuntimeState
                    .DefaultTimingReduction4A9FF4;
            BattleReducedHitRestResult result =
                BattleReducedHitRestResolver.Resolve(
                    projection.AttackerFrameDelay,
                    projection.TargetFrameDelay,
                    attackerData?.definition_effect ?? 0,
                    targetData?.definition_effect ?? 0,
                    selectedArmor != null,
                    selectedArmor?.delay ?? -1,
                    interaction.arest,
                    interaction.vrest,
                    timingReduction);

            projection.AttackerFrameDelay = result.AttackerHold;
            projection.TargetFrameDelay = result.TargetHold;
            projection.AttackerAttackExempt = result.Arest;
            projection.AttackerItrArest = result.Arest;
            if (result.ShouldWriteVrest)
                projection.TargetVrestAgainstAttacker = result.Vrest;
        }

        private static bool ProjectStandardObjectDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectStandardObjectDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;

            ProjectWeaponNormalVitalAndStatWrites(
                attacker,
                target,
                resolvedItr.injury,
                ref projection);
            projection.TargetHitConfirm2 = 1;
            int durabilityInjury = BattleDamageWriter.ResolveNativeAttackingInjury(
                targetWorld,
                attacker,
                resolvedItr.injury);
            projection.TargetWeaponFlightCounter = resolvedItr.bdefend == 100
                ? -1
                : projection.TargetWeaponFlightCounter - durabilityInjury;
            projection.TargetRelationTeam = projection.AttackerRelationTeam;

            if (targetType != (int)LF2ObjectType.HeavyWeapon ||
                resolvedItr.fall > 40)
            {
                projection.TargetHitCount++;
            }

            projection.TargetFall = 0;
            bool attackerState2000 = attacker.GetState() == LF2States.HeavyWeaponInSky;
            if (projection.TargetVx > -5.0 &&
                projection.TargetVx < 5.0 &&
                resolvedItr.dvx == 0)
            {
                projection.TargetKnockbackVx += attackerState2000
                    ? 5.0
                    : (attacker.Dirh() > 0 ? 5.0 : -5.0);
            }
            else if (attackerState2000 && resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx +=
                    projection.AttackerXInt < projection.TargetXInt
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
            }
            else if (targetType == (int)LF2ObjectType.ThrowWeapon ||
                     targetType == (int)LF2ObjectType.Drink)
            {
                double scaled = Math.Abs(projection.TargetVx) * 0.55;
                if (resolvedItr.dvx > scaled)
                {
                    projection.TargetKnockbackVx += attacker.Dirh() > 0
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
                }
                else if (attacker.Dirh() > 0)
                {
                    if (projection.TargetKnockbackVx > 0.0)
                        projection.TargetKnockbackVx += resolvedItr.dvx;
                    else if (projection.TargetVx < 0.0)
                        projection.TargetKnockbackVx = -scaled;
                }
                else
                {
                    if (projection.TargetKnockbackVx < 0.0)
                        projection.TargetKnockbackVx -= resolvedItr.dvx;
                    else if (projection.TargetVx > 0.0)
                        projection.TargetKnockbackVx = -scaled;
                }
            }
            else if (resolvedItr.effect == 22 || resolvedItr.effect == 23)
            {
                projection.TargetKnockbackVx +=
                    projection.TargetXInt <= projection.AttackerXInt
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
            }
            else if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx +=
                    attacker.Dirh() > 0 ? resolvedItr.dvx : -resolvedItr.dvx;
            }

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            bool applyOid100KnockbackTail =
                targetOid == 100 && target.Runtime.LinkState < 0 &&
                !(projection.TargetVx > -5.0 &&
                  projection.TargetVx < 5.0 &&
                  resolvedItr.dvx == 0);
            if (applyOid100KnockbackTail)
            {
                projection.TargetKnockbackVx *= 2.5;
                if (projection.TargetKnockbackVx > 0.0 &&
                    projection.TargetKnockbackVx < 10.0)
                {
                    projection.TargetKnockbackVx = 10.0;
                }
                else if (projection.TargetKnockbackVx < 0.0 &&
                         projection.TargetKnockbackVx > -10.0)
                {
                    projection.TargetKnockbackVx = -10.0;
                }
            }

            if (targetType != (int)LF2ObjectType.HeavyWeapon ||
                resolvedItr.fall > 40)
            {
                projection.TargetKnockbackVy +=
                    resolvedItr.dvy != 0 ? resolvedItr.dvy : -7.0;
                if ((int)(projection.TargetKnockbackVy + projection.TargetYInt) > 0)
                    projection.TargetKnockbackVy = 12.0;
            }

            if (projection.TargetLinkState > 0 &&
                HasAuthorityHeldTargetRelation(target))
            {
                projection.HeldTargetVrestAgainstAttacker = 45;
                projection.TargetVrestAgainstHeld = 30;
            }

            if (targetType != (int)LF2ObjectType.Drink)
            {
                ProjectQueuedSound(
                    targetWorld,
                    ResolveDamageEffectCue(resolvedItr.effect),
                    attacker.Runtime.XInt,
                    ref projection);
            }
            ProjectStandardHurtCustomSounds(
                targetWorld,
                attacker,
                target,
                ref projection);
            if (applyOid100KnockbackTail)
            {
                ProjectQueuedSound(
                    targetWorld,
                    "SFX_039",
                    target.Runtime.XInt,
                    ref projection);
            }

            int attackerState = attacker.GetState();
            ProjectNativeType3AttackerPostHitAction(attacker, ref projection);

            projection.TargetHitStateCount = 45;
            ProjectNativeStandardHitRest(
                attacker,
                target,
                resolvedItr,
                ref projection);
            ProjectActiveHolderFrameDelay(ref projection);

            if (attackerState == LF2States.WeaponThrowing)
            {
                int attackerFrame = ProjectBattleRandInt(ref projection, 16);
                projection.AttackerFrame = attackerFrame;
                projection.AttackerRuntimeFrame = attackerFrame;
                projection.AttackerVx = projection.TargetKnockbackVx * -0.5;
                projection.AttackerVy = -4.0;
                if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.ThrowWeapon &&
                    targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    projection.AttackerKnockbackVx = -projection.TargetKnockbackVx;
                }
            }

            if (targetType == (int)LF2ObjectType.HeavyWeapon)
            {
                projection.AttackerVrestAgainstAttacker =
                    resolvedItr.fall <= 40 && resolvedItr.effect != 4 ? 3 : 19;
                projection.TargetFacing = projection.AttackerFacing;
                int targetFrame = resolvedItr.fall <= 40 &&
                                  projection.TargetYInt >= 0 &&
                                  resolvedItr.effect != 4
                    ? 20
                    : ProjectBattleRandInt(ref projection, 6);
                projection.TargetFrame = targetFrame;
                projection.TargetRuntimeFrame = targetFrame;
            }
            else
            {
                if (targetType == (int)LF2ObjectType.ThrowWeapon ||
                    targetType == (int)LF2ObjectType.Drink)
                {
                    projection.AttackerVrestAgainstAttacker = 30;
                }

                int targetFrame = ProjectBattleRandInt(ref projection, 16);
                projection.TargetFrame = targetFrame;
                projection.TargetRuntimeFrame = targetFrame;
            }

            ProjectNativeEffectActionOverride(
                attacker,
                target,
                resolvedItr,
                ref projection);
            return ProjectKind0HitRecord(
                       attacker,
                       target,
                       resolvedItr,
                       ref projection) &&
                   attackerSlot >= 0 && targetSlot >= 0;
        }

        private static bool ProjectNonConvertedKind9ObjectDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectNonConvertedKind9ObjectDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);
            return true;
        }

        private static bool ProjectStandardType3DamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectStandardType3DamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;

            ProjectType3NormalVitalAndStatWrites(
                attacker,
                target,
                resolvedItr.injury,
                ref projection);
            if (projection.TargetHp <= 0 || resolvedItr.effect == 4)
                projection.TargetFall = 80;

            projection.TargetHitCount++;
            projection.TargetFall += resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            int previousState = target.GetFrameDataById(
                    target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.GetFrameDataById(
                    target.Runtime.PrevFrame2)?.state ??
                target.Frame?.Prev2D?.state ?? 0;
            if (previousState == LF2States.Frozen ||
                previous2State == LF2States.Falling)
            {
                projection.TargetFall = 80;
            }
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);
            ProjectStandardHurtCustomSounds(
                targetWorld,
                attacker,
                target,
                ref projection);

            bool attackerState2000 =
                attacker.GetState() == LF2States.HeavyWeaponInSky;
            if (attackerState2000 && resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx +=
                    projection.AttackerXInt < projection.TargetXInt
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
            }
            else if (resolvedItr.effect == 22 || resolvedItr.effect == 23)
            {
                projection.TargetKnockbackVx +=
                    projection.TargetXInt <= projection.AttackerXInt
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
            }
            else if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            int attackerState = attacker.GetState();
            ProjectNativeType3AttackerPostHitAction(attacker, ref projection);

            projection.TargetHitStateCount = 45;
            ProjectNativeStandardHitRest(
                attacker,
                target,
                resolvedItr,
                ref projection);

            if (attackerState == LF2States.WeaponThrowing)
            {
                int attackerFrame = ProjectBattleRandInt(ref projection, 16);
                projection.AttackerFrame = attackerFrame;
                projection.AttackerRuntimeFrame = attackerFrame;
                projection.AttackerVx = projection.TargetKnockbackVx * -0.5;
                projection.AttackerVy = -4.0;
            }

            int targetFrame;
            bool type3PairReset = false;
            LF2Entity targetPairDefinitionSource = target;
            int targetPairActionLatch =
                target.Trans?.WaitCounter ?? target.Runtime.WaitCounter;
            bool kindTransformCandidate =
                BattleDamageWriter.IsNativeLockedKindTransformCandidate(
                    attacker,
                    target);
            if (kindTransformCandidate)
            {
                targetFrame = 40;
                projection.TargetRelationTeam = attacker.RelationTeam;
                projection.TargetOwnerSlot = attacker.Runtime.OwnerSlotIndex;
                projection.TargetSpecialHitLatch0EB = true;
                projection.TargetAttackingCounter = 0;
                projection.TargetKnockbackVx = 0.0;
                projection.TargetKnockbackVy = 0.0;
                projection.TargetKnockbackVz = 0.0;
                projection.TargetObjectId =
                    LF2Entity.ResolveCurrentDataObjectId(attacker);
                projection.TargetDataObjectId = projection.TargetObjectId;
                projection.TargetDataObjectType =
                    attacker.GetCurrentDataObjectTypeForSimulation();
                projection.TargetPrevFrame = targetFrame;
                projection.TargetWaitCounter = targetFrame;
                targetPairDefinitionSource = attacker;
                targetPairActionLatch = targetFrame;
                int transformedState = attacker.GetFrameDataById(targetFrame)?.state ?? 0;
                type3PairReset =
                    (transformedState == LF2States.ObjectFlying &&
                     attackerState == LF2States.ObjectFlying) ||
                    (transformedState == LF2States.ObjectExpanding &&
                     attackerState == LF2States.ObjectExpanding);
            }
            else
            {
                LF2Entity relationSource = attacker;
                int relationSourceSlot = attacker.Runtime.SlotIndex;
                if (attacker.Runtime.LinkState < 0)
                {
                    LF2Entity activeParent = ResolveActiveType3Holder(attacker);
                    if (activeParent?.Runtime != null)
                    {
                        relationSource = activeParent;
                        relationSourceSlot = activeParent.Runtime.SlotIndex;
                    }
                }

                projection.TargetRelationTeam = relationSource.RelationTeam;
                projection.TargetOwnerSlot = relationSource.Runtime.OwnerSlotIndex;
                projection.TargetAnimCounter = relationSourceSlot;
                projection.TargetSpecialHitLatch0EB = true;
                projection.TargetKnockbackVx = 0.0;
                projection.TargetKnockbackVy = 0.0;
                projection.TargetKnockbackVz = 0.0;
                LF2FrameData responseFrame = target.Frame?.D;
                bool ordinaryFjPath =
                    (attacker.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.Character ||
                     attacker.Runtime.LinkState < 0) &&
                    resolvedItr.effect != 2 &&
                    resolvedItr.effect != 20;
                targetFrame = ordinaryFjPath
                    ? responseFrame?.hit_Fj ?? 0
                    : responseFrame?.hit_Uj ?? 0;
                if (targetFrame == 0)
                    targetFrame = ordinaryFjPath ? 30 : 20;
                projection.TargetAttackingCounter = 0;
                int responseState = target.GetFrameDataById(targetFrame)?.state ?? 0;
                type3PairReset =
                    (responseState == LF2States.ObjectFlying &&
                     attackerState == LF2States.ObjectFlying) ||
                    (responseState == LF2States.ObjectExpanding &&
                     attackerState == LF2States.ObjectExpanding);
            }
            projection.TargetFrame = targetFrame;
            projection.TargetRuntimeFrame = targetFrame;
            ProjectNativeType3MotionHoldRelease(ref projection);
            if (type3PairReset)
            {
                int targetResetAction = ResolveType3PairResetAction(
                    targetPairDefinitionSource,
                    targetPairActionLatch);
                int attackerResetAction = ResolveType3PairResetAction(
                    attacker,
                    attacker.Trans?.WaitCounter ?? attacker.Runtime.WaitCounter);
                projection.TargetFrame = targetResetAction;
                projection.TargetRuntimeFrame = targetResetAction;
                projection.TargetAttackingCounter = 0;
                projection.TargetKnockbackVx = 0.0;
                projection.TargetKnockbackVy = 0.0;
                projection.TargetKnockbackVz = 0.0;
                projection.AttackerFrame = attackerResetAction;
                projection.AttackerRuntimeFrame = attackerResetAction;
                projection.AttackerAttackingCounter = 0;
                projection.AttackerKnockbackVx = 0.0;
                projection.AttackerKnockbackVy = 0.0;
                projection.AttackerKnockbackVz = 0.0;
            }
            if (projection.TargetFall == 80)
                projection.TargetFall = 0;

            ProjectNativeEffectActionOverride(
                attacker,
                target,
                resolvedItr,
                ref projection);
            return ProjectKind0HitRecord(
                       attacker,
                       target,
                       resolvedItr,
                       ref projection) &&
                   attackerSlot >= 0 && targetSlot >= 0;
        }

        private static bool ProjectType3Kind9DamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectType3Kind9DamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (!string.IsNullOrWhiteSpace(targetData?.weapon_broken_sound))
            {
                ProjectQueuedSound(
                    targetWorld,
                    targetData.weapon_broken_sound,
                    target.Runtime.XInt,
                    ref projection);
            }
            projection.AttackerFrameDelay = -3;
            projection.TargetSpecialHitLatch0EB = true;

            if (target.GetState() == LF2States.ObjectFlying)
            {
                projection.TargetFrame = 40;
                projection.TargetRuntimeFrame = 40;
                return true;
            }

            projection.TargetRelationTeam = projection.AttackerRelationTeam;
            projection.TargetFrame = 30;
            projection.TargetRuntimeFrame = 30;
            projection.TargetAttackingCounter = 0;
            projection.TargetKnockbackVx = 0.0;
            projection.TargetKnockbackVy = 0.0;
            projection.TargetKnockbackVz = 0.0;
            projection.TargetVx = 0.0;
            projection.TargetVy = 0.0;
            projection.TargetVz = 0.0;
            projection.TargetAnimCounter = attacker.Runtime.SlotIndex;
            return true;
        }

        private static bool ProjectType3StateSyncDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectType3StateSyncDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            ProjectNativeStandardHitRest(
                attacker,
                target,
                resolvedItr,
                ref projection);

            int targetResetAction = ResolveType3PairResetAction(
                target,
                target.Trans?.WaitCounter ?? target.Runtime.WaitCounter);
            int attackerResetAction = ResolveType3PairResetAction(
                attacker,
                attacker.Trans?.WaitCounter ?? attacker.Runtime.WaitCounter);
            projection.TargetFrame = targetResetAction;
            projection.TargetRuntimeFrame = targetResetAction;
            projection.TargetAttackingCounter = 0;
            projection.TargetKnockbackVx = 0.0;
            projection.TargetKnockbackVy = 0.0;
            projection.TargetKnockbackVz = 0.0;

            projection.AttackerFrame = attackerResetAction;
            projection.AttackerRuntimeFrame = attackerResetAction;
            projection.AttackerAttackingCounter = 0;
            projection.AttackerKnockbackVx = 0.0;
            projection.AttackerKnockbackVy = 0.0;
            projection.AttackerKnockbackVz = 0.0;
            ProjectNativeType3MotionHoldRelease(ref projection);

            return true;
        }

        private static bool ProjectType3D1IdentityDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectType3D1IdentityDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);

            ProjectType3NormalVitalAndStatWrites(
                attacker,
                target,
                resolvedItr.injury,
                ref projection);
            projection.TargetHitCount++;
            projection.TargetFall += resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);
            ProjectStandardHurtCustomSounds(
                targetWorld,
                attacker,
                target,
                ref projection);

            if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            projection.TargetHitStateCount = 45;
            ProjectNativeStandardHitRest(
                attacker,
                target,
                resolvedItr,
                ref projection);

            projection.TargetRelationTeam = projection.AttackerRelationTeam;
            projection.TargetSpecialHitLatch0EB = true;
            projection.TargetAttackingCounter = 0;
            projection.TargetKnockbackVx = 0.0;
            projection.TargetKnockbackVy = 0.0;
            projection.TargetKnockbackVz = 0.0;
            projection.TargetVx = 0.0;
            projection.TargetVy = 0.0;
            projection.TargetVz = 0.0;
            projection.TargetObjectId = 0xD1;
            projection.TargetDataObjectId = 0xD1;
            projection.TargetDataObjectType =
                attacker.GetCurrentDataObjectTypeForSimulation();
            projection.TargetWeaponCount = attackerData?.weapon_hp ?? 0;
            projection.TargetFrame = 40;
            projection.TargetRuntimeFrame = 40;
            projection.TargetWaitCounter = 40;
            projection.TargetPrevFrame = 40;

            if (attacker.GetState() == LF2States.ObjectExpanding &&
                attacker.GetFrameDataById(40)?.state == LF2States.ObjectExpanding)
            {
                projection.TargetFrame = 20;
                projection.TargetRuntimeFrame = 20;
                projection.TargetAttackingCounter = 0;
                projection.TargetKnockbackVx = 0.0;
                projection.TargetKnockbackVy = 0.0;
                projection.TargetKnockbackVz = 0.0;
                projection.TargetVx = 0.0;
                projection.TargetVy = 0.0;
                projection.TargetVz = 0.0;

                projection.AttackerFrame = 20;
                projection.AttackerRuntimeFrame = 20;
                projection.AttackerAttackingCounter = 0;
                projection.AttackerKnockbackVx = 0.0;
                projection.AttackerKnockbackVy = 0.0;
                projection.AttackerKnockbackVz = 0.0;
                projection.AttackerVx = 0.0;
                projection.AttackerVy = 0.0;
                projection.AttackerVz = 0.0;
                if (projection.AttackerFrameDelay > 0)
                    projection.AttackerFrameDelay = -projection.AttackerFrameDelay;
            }

            return ProjectKind0HitRecord(
                       attacker,
                       target,
                       resolvedItr,
                       ref projection) &&
                   attackerSlot >= 0 && targetSlot >= 0;
        }

        private static bool ProjectType3ActiveD1IdentityDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectType3ActiveD1IdentityDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr) ||
                !TryResolveActiveType3IdentitySource(target.Match, 0xD1, out LF2Entity source))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            LF2CharacterData sourceData =
                LF2HitResolveRuntimeData.ResolveCharacterData(source);
            LF2Entity relationSource = attacker.Runtime.LinkState < 0
                ? ResolveActiveType3Holder(attacker)
                : attacker;
            if (relationSource == null)
                return false;

            bool replacementUsesFrame20 =
                attacker.Runtime.LinkState < 0 &&
                (resolvedItr.effect == 2 || resolvedItr.effect == 20);
            int replacementFrame = replacementUsesFrame20 ? 20 : 30;
            LF2FrameData sourceReplacementFrame = source.GetFrameDataById(replacementFrame);
            if (sourceReplacementFrame == null)
                return false;

            ProjectType3NormalVitalAndStatWrites(
                attacker,
                target,
                resolvedItr.injury,
                ref projection);
            projection.TargetHitCount++;
            projection.TargetFall += resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);
            ProjectStandardHurtCustomSounds(
                targetWorld,
                attacker,
                target,
                ref projection);

            if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            projection.TargetHitStateCount = 45;
            ProjectNativeStandardHitRest(
                attacker,
                target,
                resolvedItr,
                ref projection);

            projection.TargetRelationTeam = relationSource.RelationTeam;
            projection.TargetSpecialHitLatch0EB = true;
            projection.TargetAttackingCounter = 0;
            projection.TargetKnockbackVx = 0.0;
            projection.TargetKnockbackVy = 0.0;
            projection.TargetKnockbackVz = 0.0;
            projection.TargetVx = 0.0;
            projection.TargetVy = 0.0;
            projection.TargetVz = 0.0;
            projection.TargetObjectId = 0xD1;
            projection.TargetDataObjectId = 0xD1;
            projection.TargetDataObjectType =
                source.GetCurrentDataObjectTypeForSimulation();
            projection.TargetWeaponCount = sourceData?.weapon_hp ?? 0;
            projection.TargetFrame = replacementFrame;
            projection.TargetRuntimeFrame = replacementFrame;
            projection.TargetPrevFrame = replacementFrame;
            projection.TargetWaitCounter = replacementFrame;
            ProjectActiveHolderFrameDelay(ref projection);

            bool characterDat = projection.TargetDataObjectType ==
                                (int)LF2ObjectType.Character;
            if ((resolvedItr.effect == 3 || resolvedItr.effect == 30) &&
                characterDat && sourceReplacementFrame.state != 13)
            {
                projection.TargetFrame = 200;
                projection.TargetRuntimeFrame = 200;
                projection.TargetAttackingCounter = 0;
                ProjectQueuedSound(
                    targetWorld,
                    "SFX_065",
                    target.Runtime.XInt,
                    ref projection);
            }
            else if (resolvedItr.effect >= 5000 && resolvedItr.effect < 6000)
            {
                projection.TargetPp = Math.Max(
                    0,
                    projection.TargetPp - (resolvedItr.effect - 5000));
            }
            else if (resolvedItr.effect >= 6000 && resolvedItr.effect < 7000)
            {
                int effectFrame = resolvedItr.effect - 6000;
                projection.TargetFrame = effectFrame;
                projection.TargetRuntimeFrame = effectFrame;
            }
            else if (characterDat &&
                     (resolvedItr.effect == 2 ||
                      resolvedItr.effect == 21 ||
                      resolvedItr.effect == 22 ||
                      (resolvedItr.effect == 20 && sourceReplacementFrame.state != 18)))
            {
                projection.TargetFrame = 203;
                projection.TargetRuntimeFrame = 203;
                projection.TargetAttackingCounter = 0;
                projection.TargetFacing = projection.TargetKnockbackVx < 0.0 ? 0 : 1;
                ProjectQueuedSound(
                    targetWorld,
                    "SFX_068",
                    target.Runtime.XInt,
                    ref projection);
            }
            else if (resolvedItr.effect == 23)
            {
                ProjectQueuedSound(
                    targetWorld,
                    "SFX_068",
                    target.Runtime.XInt,
                    ref projection);
            }

            ProjectNativeEffectActionOverride(
                attacker,
                target,
                resolvedItr,
                ref projection);
            return true;
        }

        private static bool TryResolveActiveType3IdentitySource(
            SimulationWorld sourceWorld,
            int objectId,
            out LF2Entity source)
        {
            source = null;
            if (sourceWorld == null)
                return false;

            for (int slot = 0; slot < sourceWorld.MaxRuntimeSlotsForServices; slot++)
            {
                LF2Entity candidate = sourceWorld.FindEntityByRuntimeSlotForQuery(slot);
                if (candidate == null || candidate.ObjectId != objectId)
                    continue;

                source = candidate;
                return true;
            }

            return false;
        }

        private static LF2Entity ResolveActiveType3Holder(LF2Entity entity)
        {
            if (entity?.Match == null || entity.Runtime == null)
                return null;

            int holderSlot = entity.Runtime.HolderStableId;
            if (holderSlot < 0 || holderSlot >= entity.Match.MaxRuntimeSlotsForServices)
                return null;

            return entity.Match.FindEntityByRuntimeSlotForQuery(holderSlot);
        }

        private static bool IsKarasuType3Oid(int oid)
        {
            return oid == 0xC8 || oid == 0xCB || oid == 0xCD || oid == 0xCE ||
                   oid == 0xCF || oid == 0xD7 || oid == 0xD8;
        }

        private static void ProjectType3NormalVitalAndStatWrites(
            LF2Entity attacker,
            LF2Entity target,
            int rawInjury,
            ref WriterEffectSnapshot projection)
        {
            int effectiveInjury = BattleDamageWriter
                .ResolveNativeUnarmoredHpInjury(
                    rawInjury,
                    target.Runtime.IncomingDamageScale340,
                    attacker.Runtime.WeakTimer12C);
            projection.TargetHp -= effectiveInjury;
            projection.TargetHpBound -= effectiveInjury / 3;
            projection.TargetInputHpConsumedTotal = unchecked(
                projection.TargetInputHpConsumedTotal + effectiveInjury);
            ProjectNativeStandardHitCreditAndConsume(
                target,
                effectiveInjury,
                ref projection);
        }

        private static void ProjectWeaponNormalVitalAndStatWrites(
            LF2Entity attacker,
            LF2Entity target,
            int rawInjury,
            ref WriterEffectSnapshot projection)
        {
            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            bool normalVitalWeapon =
                targetType == (int)LF2ObjectType.LightWeapon ||
                targetType == (int)LF2ObjectType.HeavyWeapon ||
                targetType == (int)LF2ObjectType.ThrowWeapon;
            if (!normalVitalWeapon)
                return;

            int effectiveInjury = BattleDamageWriter
                .ResolveNativeUnarmoredHpInjury(
                    rawInjury,
                    target.Runtime.IncomingDamageScale340,
                    attacker.Runtime.WeakTimer12C);

            projection.TargetHp -= effectiveInjury;
            projection.TargetHpBound -= effectiveInjury / 3;
            projection.TargetInputHpConsumedTotal = unchecked(
                projection.TargetInputHpConsumedTotal + effectiveInjury);
            ProjectNativeStandardHitCreditAndConsume(
                target,
                effectiveInjury,
                ref projection);
        }

        private static void ProjectNativeStandardHitCreditAndConsume(
            LF2Entity target,
            int effectiveInjury,
            ref WriterEffectSnapshot projection)
        {
            if (projection.TargetDataObjectType ==
                    (int)LF2ObjectType.Character &&
                target?.Runtime?.OrdinaryCreditGate2F4 == -1 &&
                projection.StandardCreditHandle.IsValid)
            {
                projection.StandardCreditInputScore = unchecked(
                    projection.StandardCreditInputScore + effectiveInjury);
            }

            if ((projection.AttackerKind4SourceCount & 0xFFFF) != 0)
                projection.AttackerKind4SourceCount--;
        }

        private static bool IsSupportedType3Effect(int effect)
        {
            // Ordinary kind 0 rejects effect 20 for non-character DAT, but original
            // kind 9 can be converted to kind 0 after collision collection. Therefore
            // every integer is valid at the resolved writer boundary.
            return true;
        }

        private static string ResolveDamageEffectCue(int effect)
        {
            return effect switch
            {
                1 => "SFX_002",
                2 => "SFX_006",
                3 => "SFX_010",
                4 => "SFX_011",
                5 => "SFX_004",
                _ => "SFX_001",
            };
        }

        private static bool IsSupportedStandardCharacterDamageEffect(int effect)
        {
            // Ordinary kind 0 rejects effect 4 for character DAT, but original kind 9
            // is converted after collection, so the resolved writer must support it.
            return true;
        }

        private static bool HasFramesInRange(
            LF2Entity entity,
            int firstInclusive,
            int lastExclusive)
        {
            for (int frameId = firstInclusive; frameId < lastExclusive; frameId++)
            {
                if (entity.GetFrameDataById(frameId) == null)
                    return false;
            }

            return true;
        }

        private static void ProjectActiveHolderFrameDelay(
            ref WriterEffectSnapshot projection)
        {
            if (projection.AttackerLinkState < 0 &&
                projection.ActiveHolderHandle.IsValid)
            {
                projection.ActiveHolderFrameDelay = projection.AttackerFrameDelay;
            }
        }

        private static int ResolveType3PairResetAction(
            LF2Entity definitionSource,
            int actionLatch)
        {
            int action = definitionSource?.GetFrameDataById(actionLatch)?.hit_Uj ?? 0;
            return action == 0 ? 20 : action;
        }

        private static void ProjectNativeType3MotionHoldRelease(
            ref WriterEffectSnapshot projection)
        {
            if (projection.AttackerLinkState < 0)
            {
                if (!projection.ActiveHolderHandle.IsValid)
                    return;

                projection.ActiveHolderFrameDelay = projection.AttackerFrameDelay;
                if (projection.ActiveHolderFrameDelay > 0)
                    projection.ActiveHolderFrameDelay = -projection.ActiveHolderFrameDelay;
                return;
            }

            if (projection.AttackerFrameDelay > 0)
                projection.AttackerFrameDelay = -projection.AttackerFrameDelay;
        }

        private static bool HasAuthorityHeldTargetRelation(LF2Entity holder)
        {
            if (holder?.Runtime == null || holder.Runtime.LinkState <= 0)
                return false;

            int holderSlot = holder.Runtime.SlotIndex;
            int heldTargetSlot = holder.Runtime.ResolveActiveHeldSlotIndex();
            LF2Entity heldTarget = heldTargetSlot >= 0
                ? holder.Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot)
                : null;
            return holderSlot >= 0 &&
                   heldTarget?.Runtime != null &&
                   heldTarget.Runtime.HolderStableId == holderSlot;
        }

        private static bool CanProjectStandardCharacterDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                return false;
            }

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            BattleOrdinaryCharacterDamageRoute route =
                BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                    target.Match ?? attacker.Match,
                    attacker,
                    target,
                    resolvedItr);
            if (targetOid == 300 || !route.UsesUnarmoredHit)
            {
                return false;
            }

            bool attackerState1002 =
                attacker.GetState() == LF2States.WeaponThrowing;
            if (!IsSupportedStandardCharacterDamageEffect(resolvedItr.effect))
            {
                return false;
            }

            if (attackerState1002 && !HasFramesInRange(attacker, 0, 16))
                return false;

            LF2CharacterData attackerData = LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData = LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (attackerData == null || targetData == null)
            {
                return false;
            }

            int attackerType = attacker.GetCurrentDataObjectTypeForSimulation();
            int requiredSounds = resolvedItr.effect == 1 ? 4 : 2;
            if (attackerType == (int)LF2ObjectType.SpecialAttack &&
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound))
            {
                requiredSounds++;
            }
            if (targetOid == 100 && target.Runtime.LinkState < 0)
                requiredSounds++;
            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static bool IsOid201CharacterHitLifecycle(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null || resolvedItr == null ||
                resolvedItr.kind != 0 ||
                attacker.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                return false;
            }

            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            return attackerOid == 0xC9;
        }

        private static bool ProjectStandardCharacterDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectStandardCharacterDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            BattleOrdinaryCharacterDamageRoute route =
                BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                    targetWorld,
                    attacker,
                    target,
                    resolvedItr);
            LF2ArmorData brokenArmor = route.Kind ==
                BattleOrdinaryCharacterDamageRouteKind
                    .UnarmoredType1BrokenFallback
                ? route.Armor
                : null;
            if (brokenArmor != null)
                projection.TargetRuntimeArmorHp = -1;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            int injury = BattleDamageWriter.ResolveNativeUnarmoredHpInjury(
                resolvedItr.injury,
                target.Runtime.IncomingDamageScale340,
                attacker.Runtime.WeakTimer12C);

            if (projection.TargetHp > 0 &&
                injury >= projection.TargetHp &&
                target.Runtime.OrdinaryCreditGate2F4 == -1)
            {
                if (projection.StandardCreditHandle.IsValid)
                    projection.StandardCreditKnockoutCount++;
                if (projection.HolderHandle.IsValid)
                    projection.HolderKillStat++;
                if (projection.TargetKillStat != int.MinValue)
                    projection.TargetKillStat++;
            }

            projection.TargetHp -= injury;
            projection.TargetHpBound -= injury / 3;
            projection.TargetInputHpConsumedTotal = unchecked(
                projection.TargetInputHpConsumedTotal + injury);
            ProjectNativeStandardHitCreditAndConsume(
                target,
                injury,
                ref projection);
            projection.TargetComboCountVic += injury;
            if (target.Runtime.OrdinaryCreditGate2F4 == -1 &&
                projection.HolderHandle.IsValid)
                projection.HolderComboCountAtk += injury;
            if (projection.TargetDamageStat != int.MinValue)
                projection.TargetDamageStat += injury;

            projection.TargetHitCount++;
            int fallIncrement = resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            int previousState = target.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.GetFrameDataById(
                    target.Runtime.PrevFrame2)?.state ??
                target.Frame?.Prev2D?.state ?? 0;
            int projectedFall = projection.TargetFall;
            if (projection.TargetHp <= 0 || resolvedItr.effect == 4)
                projectedFall = 80;
            projectedFall += fallIncrement;
            if (previousState == LF2States.Frozen ||
                previous2State == LF2States.Falling)
            {
                projectedFall = 80;
            }
            bool targetInAir = projection.TargetYInt < 0;
            bool knockback = projectedFall > 60;
            if (knockback)
            {
                projection.TargetFall = 80;
            }
            else if (projectedFall > 40)
            {
                projection.TargetFall = 60;
                projection.TargetFrame = LF2StandardFrames.Injured6;
                projection.TargetRuntimeFrame = LF2StandardFrames.Injured6;
                if (targetInAir)
                {
                    projection.TargetFall = 80;
                    knockback = true;
                }
            }
            else if (projectedFall > 20)
            {
                projection.TargetFall = 40;
                int targetFrame = projection.TargetFacing != projection.AttackerFacing
                    ? LF2StandardFrames.Injured2
                    : LF2StandardFrames.Injured4;
                projection.TargetFrame = targetFrame;
                projection.TargetRuntimeFrame = targetFrame;
                if (targetInAir)
                {
                    projection.TargetFall = 80;
                    knockback = true;
                }
            }
            else if (projectedFall > 0)
            {
                projection.TargetFall = 20;
                int targetFrame = targetInAir
                    ? (projection.TargetFacing != projection.AttackerFacing
                        ? LF2StandardFrames.Injured2
                        : LF2StandardFrames.Injured4)
                    : LF2StandardFrames.Injured;
                projection.TargetFrame = targetFrame;
                projection.TargetRuntimeFrame = targetFrame;
            }
            else
            {
                projection.TargetFall = projectedFall;
            }

            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);
            ProjectStandardHurtCustomSounds(
                targetWorld,
                attacker,
                target,
                ref projection);
            ProjectQueuedSound(
                targetWorld,
                knockback ? "SFX_006" : "SFX_001",
                knockback ? target.Runtime.XInt : attacker.Runtime.XInt,
                ref projection);

            if (resolvedItr.effect == 1)
            {
                ProjectQueuedSound(
                    targetWorld,
                    knockback ? "SFX_033" : "SFX_032",
                    target.Runtime.XInt,
                    ref projection);
                ProjectQueuedSound(
                    targetWorld,
                    knockback ? "SFX_006" : "SFX_001",
                    knockback ? target.Runtime.XInt : attacker.Runtime.XInt,
                    ref projection);
            }

            bool attackerState2000 = attacker.GetState() == LF2States.HeavyWeaponInSky;
            if (knockback &&
                projection.TargetVx > -5.0 &&
                projection.TargetVx < 5.0 &&
                resolvedItr.dvx == 0)
            {
                projection.TargetKnockbackVx += attackerState2000
                    ? 5.0
                    : (attacker.Dirh() > 0 ? 5.0 : -5.0);
            }
            else if (attackerState2000 && resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx +=
                    projection.AttackerXInt < projection.TargetXInt
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
            }
            else if (resolvedItr.effect == 22 || resolvedItr.effect == 23)
            {
                projection.TargetKnockbackVx +=
                    projection.TargetXInt <= projection.AttackerXInt
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
            }
            else if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            bool applyOid100KnockbackTail =
                targetOid == 100 &&
                projection.TargetLinkState < 0 &&
                !(knockback &&
                  projection.TargetVx > -5.0 &&
                  projection.TargetVx < 5.0 &&
                  resolvedItr.dvx == 0);
            if (applyOid100KnockbackTail)
            {
                projection.TargetKnockbackVx *= 2.5;
                ProjectQueuedSound(
                    targetWorld,
                    "SFX_039",
                    target.Runtime.XInt,
                    ref projection);
                if (projection.TargetKnockbackVx > 0.0 &&
                    projection.TargetKnockbackVx < 10.0)
                {
                    projection.TargetKnockbackVx = 10.0;
                }
                else if (projection.TargetKnockbackVx < 0.0 &&
                         projection.TargetKnockbackVx > -10.0)
                {
                    projection.TargetKnockbackVx = -10.0;
                }
            }

            if (brokenArmor != null &&
                projection.TargetRuntimeArmorHp == -1)
            {
                if (brokenArmor.action != 0)
                {
                    projection.TargetFrame = brokenArmor.action;
                    projection.TargetRuntimeFrame = brokenArmor.action;
                }
                projection.TargetRuntimeArmorHp++;
            }

            if (knockback)
            {
                projection.TargetKnockbackVy +=
                    resolvedItr.dvy != 0 ? resolvedItr.dvy : -7.0;
                if ((int)(projection.TargetKnockbackVy + projection.TargetYInt) > 0)
                    projection.TargetKnockbackVy = 12.0;

                int targetFrame = projection.TargetFacing == 0
                    ? (projection.TargetKnockbackVx <= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack)
                    : (projection.TargetKnockbackVx >= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack);
                projection.TargetFrame = targetFrame;
                projection.TargetRuntimeFrame = targetFrame;

                if (projection.TargetLinkState > 0 &&
                    HasAuthorityHeldTargetRelation(target))
                {
                    projection.HeldTargetVrestAgainstAttacker = 45;
                    projection.TargetVrestAgainstHeld = 30;
                }
            }

            ProjectNativeType3AttackerPostHitAction(attacker, ref projection);

            projection.TargetHitStateCount = 45;
            ProjectNativeStandardHitRest(
                attacker,
                target,
                resolvedItr,
                ref projection);

            ProjectActiveHolderFrameDelay(ref projection);
            if (projection.TargetFall == 80)
                projection.TargetFall = 0;

            if (attacker.GetState() == LF2States.WeaponThrowing)
            {
                int attackerFrame = ProjectBattleRandInt(ref projection, 16);
                projection.AttackerFrame = attackerFrame;
                projection.AttackerRuntimeFrame = attackerFrame;
                projection.AttackerVx = projection.TargetKnockbackVx * -0.5;
                projection.AttackerVy = -4.0;
                if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.ThrowWeapon &&
                    target.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.ThrowWeapon)
                {
                    projection.AttackerKnockbackVx = -projection.TargetKnockbackVx;
                }
            }

            ProjectNativeEffectActionOverride(
                attacker,
                target,
                resolvedItr,
                ref projection);
            ProjectNativeKind0PostEffectAction(
                target,
                resolvedItr,
                ref projection);
            bool projectedHitRecord = ProjectKind0HitRecord(
                attacker,
                target,
                resolvedItr,
                ref projection);
            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.SpecialAttack &&
                attackerOid == 0xD6)
            {
                projection.AttackerHp = 0;
            }

            return projectedHitRecord && attackerSlot >= 0 && targetSlot >= 0;
        }

        private static void ProjectNativeEffectActionOverride(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            BattleDamageWriter.NativeEffectActionOverrideDecision decision;
            if (BattleDamageWriter.IsNativeLockedKindTransformCandidate(
                    attacker,
                    target) &&
                projection.TargetDataObjectId ==
                    LF2Entity.ResolveCurrentDataObjectId(attacker))
            {
                LF2CharacterData projectedData =
                    LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
                decision = BattleDamageWriter
                    .ResolveNativeEffectActionOverrideForProjectedTarget(
                        attacker,
                        resolvedItr,
                        projection.TargetHp,
                        projection.TargetDataObjectType,
                        projectedData?.property ?? 0,
                        attacker.GetFrameDataById(projection.TargetWaitCounter),
                        attacker.GetFrameDataById(projection.TargetPrevFrame));
            }
            else
            {
                decision = BattleDamageWriter.ResolveNativeEffectActionOverride(
                    attacker,
                    target,
                    resolvedItr,
                    projection.TargetHp);
            }
            if (decision.AttackerAction > 0)
            {
                projection.AttackerFrame = decision.AttackerAction;
                projection.AttackerRuntimeFrame = decision.AttackerAction;
            }
            if (decision.TargetAction > 0)
            {
                projection.TargetFrame = decision.TargetAction;
                projection.TargetRuntimeFrame = decision.TargetAction;
            }
        }

        private static void ProjectNativeKind0PostEffectAction(
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            BattleDamageWriter.NativeKind0PostEffectActionDecision decision =
                BattleDamageWriter.ResolveNativeKind0PostEffectAction(
                    target,
                    resolvedItr,
                    projection.TargetKnockbackVx);
            if (decision.Action <= 0)
                return;

            projection.TargetFrame = decision.Action;
            projection.TargetRuntimeFrame = decision.Action;
            projection.TargetAttackingCounter = 0;
            if (decision.Facing >= 0)
                projection.TargetFacing = decision.Facing;
        }

        private static bool CanProjectAlternateCharacterDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                return false;
            }

            BattleOrdinaryCharacterDamageRoute route =
                BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                    target.Match ?? attacker.Match,
                    attacker,
                    target,
                    resolvedItr);
            if (!route.UsesReducedHit)
                return false;

            bool attackerState1002 =
                attacker.GetState() == LF2States.WeaponThrowing;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (attackerData == null || targetData == null ||
                target.ItrRest == null)
            {
                return false;
            }

            if (attackerState1002 && !HasFramesInRange(attacker, 0, 16))
                return false;
            int requiredSounds = attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.SpecialAttack
                ? (string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound) ? 0 : 1)
                : 1;
            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static bool ProjectAlternateCharacterDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (!CanProjectAlternateCharacterDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            BattleOrdinaryCharacterDamageRoute route =
                BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                    targetWorld,
                    attacker,
                    target,
                    resolvedItr);
            LF2ArmorData selectedArmor = route.Kind ==
                BattleOrdinaryCharacterDamageRouteKind.ReducedType1Armor
                ? route.Armor
                : null;
            BattleReducedHitDamageResult damage =
                BattleReducedHitDamageResolver.Resolve(
                    resolvedItr.injury,
                    selectedArmor != null,
                    selectedArmor?.type ?? 0,
                    selectedArmor?.decrease ?? 0,
                    selectedArmor?.mp ?? 0,
                    selectedArmor?.hp ?? 0,
                    target.Runtime.IncomingDamageScale340);
            if (!damage.Supported)
                return false;
            int reducedInjury = damage.HpDamage;

            if (projection.TargetHp > 0 &&
                reducedInjury >= projection.TargetHp &&
                target.Runtime.OrdinaryCreditGate2F4 == -1)
            {
                if (projection.StandardCreditHandle.IsValid)
                    projection.StandardCreditKnockoutCount++;
                if (projection.HolderHandle.IsValid)
                    projection.HolderKillStat++;
                if (projection.TargetKillStat != int.MinValue)
                    projection.TargetKillStat++;
            }

            projection.TargetHp -= reducedInjury;
            projection.TargetHpBound -= reducedInjury / 3;
            projection.TargetPp -= damage.MpDamage;
            projection.TargetInputHpConsumedTotal = unchecked(
                projection.TargetInputHpConsumedTotal + reducedInjury);
            projection.TargetInputMpConsumedTotal = unchecked(
                projection.TargetInputMpConsumedTotal + damage.MpDamage);
            projection.TargetRuntimeArmorHp = unchecked(
                projection.TargetRuntimeArmorHp +
                damage.RuntimeArmorHpDelta);
            ProjectNativeStandardHitCreditAndConsume(
                target,
                reducedInjury,
                ref projection);
            projection.TargetComboCountVic += reducedInjury;
            if (target.Runtime.OrdinaryCreditGate2F4 == -1 &&
                projection.HolderHandle.IsValid)
                projection.HolderComboCountAtk += reducedInjury;
            if (projection.TargetDamageStat != int.MinValue)
                projection.TargetDamageStat += reducedInjury;

            if (projection.TargetHp <= 0)
                projection.TargetFall = 80;

            projection.TargetAttackingCounter = 0;
            if (projection.TargetRuntimeArmorHp <= 0)
                projection.TargetHitStateCount += resolvedItr.bdefend;
            projection.TargetHitCount++;
            ProjectNativeReducedHitRest(
                attacker,
                target,
                resolvedItr,
                ref projection,
                selectedArmor);
            ProjectActiveHolderFrameDelay(ref projection);

            bool attackerState2000 = attacker.GetState() == LF2States.HeavyWeaponInSky;
            if (projection.TargetYInt == 0)
            {
                int previous2State = target.GetFrameDataById(
                        target.Runtime.PrevFrame2)?.state ??
                    target.Frame?.Prev2D?.state ?? 0;
                int actionThreshold = selectedArmor != null
                    ? Math.Max(selectedArmor.ratio, 30)
                    : 30;
                if (projection.TargetHitStateCount > actionThreshold &&
                    previous2State == LF2States.Defending)
                {
                    projection.TargetFrame = LF2StandardFrames.DefendBroken;
                    projection.TargetRuntimeFrame = LF2StandardFrames.DefendBroken;
                }
                else if (projection.TargetFrame == LF2StandardFrames.Defend)
                {
                    projection.TargetFrame = LF2StandardFrames.Defend1;
                    projection.TargetRuntimeFrame = LF2StandardFrames.Defend1;
                }

                if (projection.TargetFall == 80 &&
                    projection.TargetVx < 3.0 &&
                    projection.TargetVx > -3.0 &&
                    resolvedItr.dvx == 0)
                {
                    projection.TargetKnockbackVx += attackerState2000
                        ? (projection.AttackerXInt < projection.TargetXInt ? 6.0 : -6.0)
                        : (attacker.Dirh() > 0 ? 3.0 : -3.0);
                }
                else if (attackerState2000)
                {
                    projection.TargetKnockbackVx +=
                        projection.AttackerXInt < projection.TargetXInt
                            ? resolvedItr.dvx
                            : -resolvedItr.dvx;
                }
                else if (resolvedItr.effect == 22 || resolvedItr.effect == 23)
                {
                    projection.TargetKnockbackVx +=
                        projection.TargetXInt <= projection.AttackerXInt
                            ? resolvedItr.dvx
                            : -resolvedItr.dvx;
                }
                else
                {
                    int halfDvx = resolvedItr.dvx / 2;
                    projection.TargetKnockbackVx += attacker.Dirh() > 0
                        ? halfDvx
                        : -halfDvx;
                }
            }
            else if (projection.TargetFall == 80 &&
                     projection.TargetVx < 6.0 &&
                     projection.TargetVx > -6.0 &&
                     resolvedItr.dvx < 6)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0 ? 6.0 : -6.0;
            }
            else if (resolvedItr.effect == 22 || resolvedItr.effect == 23)
            {
                projection.TargetKnockbackVx +=
                    projection.TargetXInt <= projection.AttackerXInt
                        ? resolvedItr.dvx
                        : -resolvedItr.dvx;
            }
            else
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.SpecialAttack)
            {
                if (!string.IsNullOrWhiteSpace(attackerData?.weapon_broken_sound))
                {
                    ProjectQueuedSound(
                        targetWorld,
                        attackerData.weapon_broken_sound,
                        attacker.Runtime.XInt,
                        ref projection);
                }
            }
            else
            {
                string leadCue = target.ObjectId == 37 || target.ObjectId == 6
                    ? "SFX_017"
                    : "SFX_002";
                ProjectQueuedSound(
                    targetWorld,
                    leadCue,
                    target.Runtime.XInt,
                    ref projection);
            }

            if (attacker.GetState() == LF2States.WeaponThrowing)
            {
                int attackerFrame = ProjectBattleRandInt(ref projection, 16);
                projection.AttackerFrame = attackerFrame;
                projection.AttackerRuntimeFrame = attackerFrame;
                projection.AttackerVx = projection.TargetKnockbackVx * -0.5;
                projection.AttackerVy = -4.0;
                projection.AttackerVz *= -0.6666666666666666;
            }

            if (attackerState2000)
            {
                if (BattleDamageWriter.ShouldDampenNativeReducedState2000(
                        projection.AttackerX,
                        projection.TargetX,
                        projection.AttackerVx))
                {
                    projection.AttackerVx /= 2.5;
                    projection.AttackerVz /= 2.5;
                }
            }

            ProjectNativeType3AttackerPostHitAction(attacker, ref projection);

            return ProjectKind0HitRecord(
                attacker,
                target,
                resolvedItr,
                ref projection);
        }

        private static bool ProjectKind0HitRecord(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            if (projection.HitRecordCount < LF2Entity.MaxHitRecordSlots)
            {
                LF2FrameData attackerFrame = attacker.GetFrameDataById(
                    projection.AttackerFrame) ?? attacker.Frame?.D;
                if (attackerFrame == null)
                    return false;

                int hitX;
                if (attacker.Dirh() > 0)
                {
                    hitX = attacker.Runtime.XInt - attackerFrame.centerx +
                        resolvedItr.x + resolvedItr.w;
                    if (hitX > target.Runtime.XInt)
                        hitX = target.Runtime.XInt;
                }
                else
                {
                    hitX = attacker.Runtime.XInt + attackerFrame.centerx -
                        resolvedItr.x - resolvedItr.w;
                    if (hitX < target.Runtime.XInt)
                        hitX = target.Runtime.XInt;
                }

                int hitYOffset = attacker.Runtime.YInt + (resolvedItr.h / 2) +
                    resolvedItr.y - attackerFrame.centery;
                int lowerY = target.Runtime.YInt - attackerFrame.centery;
                if (hitYOffset < lowerY)
                    hitYOffset = (lowerY + hitYOffset) >> 1;
                else if (hitYOffset > target.Runtime.YInt)
                    hitYOffset = (target.Runtime.YInt + hitYOffset) >> 1;

                projection.HitRecordCount++;
                int sparkPhase = resolvedItr.effect == 1 ? 1 : 0;
                projection.HitRecordDamage = resolvedItr.fall > 60
                    ? sparkPhase * 20
                    : sparkPhase * 20 + 10;
                projection.HitRecordZ = attacker.Runtime.ZInt + hitYOffset +
                    ProjectBattleRandInt(ref projection, 9) - 4;
                projection.HitRecordX = hitX +
                    ProjectBattleRandInt(ref projection, 9) - 4;
            }

            return true;
        }

        private static void ProjectNativeType3AttackerPostHitAction(
            LF2Entity attacker,
            ref WriterEffectSnapshot projection)
        {
            BattleDamageWriter.NativeType3AttackerPostHitActionDecision decision =
                BattleDamageWriter.ResolveNativeType3AttackerPostHitAction(attacker);
            if (!decision.Applies)
                return;

            projection.AttackerFrame = decision.Action;
            projection.AttackerRuntimeFrame = decision.Action;
            projection.AttackerAttackingCounter = 0;
            projection.AttackerVx = 0.0;
            if (decision.HasSelectedFrame)
                projection.AttackerVz = decision.SelectedFrameDvx;
        }

        private static void ProjectQueuedSound(
            SimulationWorld targetWorld,
            string cue,
            int worldX,
            ref WriterEffectSnapshot projection)
        {
            AddPendingSoundFingerprint(
                ref projection.PendingSoundFingerprint,
                cue,
                worldX,
                targetWorld.CurrentTickIndex);
            projection.PendingSoundCount++;
            projection.PendingSoundCue = cue;
            projection.PendingSoundWorldX = worldX;
            projection.PendingSoundTick = targetWorld.CurrentTickIndex;
            projection.QueuedSoundEventCount++;
        }

        private static void ProjectStandardHurtCustomSounds(
            SimulationWorld targetWorld,
            LF2Entity attacker,
            LF2Entity target,
            ref WriterEffectSnapshot projection)
        {
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.SpecialAttack &&
                !string.IsNullOrWhiteSpace(attackerData?.weapon_broken_sound))
            {
                ProjectQueuedSound(
                    targetWorld,
                    attackerData.weapon_broken_sound,
                    attacker.Runtime.XInt,
                    ref projection);
            }

            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (target.GetCurrentDataObjectTypeForSimulation() >
                    (int)LF2ObjectType.Character &&
                !string.IsNullOrWhiteSpace(targetData?.weapon_hit_sound))
            {
                ProjectQueuedSound(
                    targetWorld,
                    targetData.weapon_hit_sound,
                    target.Runtime.XInt,
                    ref projection);
            }
        }

        private static int ProjectBattleRandInt(
            ref WriterEffectSnapshot projection,
            int exclusiveMaximum)
        {
            unchecked
            {
                projection.RngState = projection.RngState * 0x343FDu + 0x269EC3u;
            }
            projection.RngCallCount++;
            return (int)((projection.RngState >> 16) & 0x7FFFu) % exclusiveMaximum;
        }

        private static bool ProjectNativeImpactWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            ref WriterEffectSnapshot projection)
        {
            BattleNativeImpactPlan plan = BattleDamageWriter.CreateNativeImpactPlan(
                target.Match ?? attacker.Match, attacker, target, itr);
            for (int i = 0; i < plan.OperationCount; i++)
            {
                BattleNativeImpactOperation op = plan.GetOperation(i);
                switch (op.Kind)
                {
                    case BattleNativeImpactWriteKind.Environment: projection.TargetEnvironmentState320 = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.CatchSource: projection.TargetCatchSourceSlot90 = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.ImpactSource: projection.TargetImpactSourceSlot164 = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.Action:
                        projection.TargetFrame = (int)op.Value;
                        projection.TargetRuntimeFrame = (int)op.Value;
                        break;
                    case BattleNativeImpactWriteKind.Vx: projection.TargetVx = op.Value; break;
                    case BattleNativeImpactWriteKind.Vz: projection.TargetVz = op.Value; break;
                    case BattleNativeImpactWriteKind.PendingX: projection.TargetKnockbackVx = op.Value; break;
                    case BattleNativeImpactWriteKind.PendingZ: projection.TargetKnockbackVz = op.Value; break;
                    case BattleNativeImpactWriteKind.YInt: projection.TargetYInt = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.Y: projection.TargetY = op.Value; break;
                    case BattleNativeImpactWriteKind.Vy: projection.TargetVy = op.Value; break;
                    case BattleNativeImpactWriteKind.PendingY: projection.TargetKnockbackVy = op.Value; break;
                }
            }
            return true;
        }

        private static bool ProjectKind15WriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            ref WriterEffectSnapshot projection)
        {
            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            if (targetType == (int)LF2ObjectType.Character)
            {
                ProjectWhirlwindMovement(attacker, 3.0, ref projection);
                return true;
            }

            bool lightLike = targetType == (int)LF2ObjectType.LightWeapon ||
                             targetType == (int)LF2ObjectType.ThrowWeapon ||
                             targetType == (int)LF2ObjectType.Drink;
            if (lightLike)
            {
                int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
                if (targetOid == 0xC9 || targetOid == 0xCA)
                    return true;

                if (target.GetFrameDataById(projection.TargetFrame)?.state !=
                    LF2States.WeaponInSky)
                {
                    projection.TargetFrame = 0;
                    projection.TargetRuntimeFrame = 0;
                }

                ProjectWhirlwindMovement(attacker, 3.0, ref projection);
                return true;
            }

            if (targetType == (int)LF2ObjectType.HeavyWeapon)
            {
                if (target.GetFrameDataById(projection.TargetFrame)?.state !=
                    LF2States.HeavyWeaponInSky)
                {
                    projection.TargetFrame = 0;
                    projection.TargetRuntimeFrame = 0;
                }

                ProjectWhirlwindMovement(attacker, 2.3, ref projection);
            }

            return true;
        }


        private static void ProjectWhirlwindMovement(
            LF2Entity attacker,
            double vyStep,
            ref WriterEffectSnapshot projection)
        {
            projection.TargetKnockbackVx = projection.TargetVx +
                (projection.TargetXInt > attacker.Runtime.XInt ? -1.0 : 1.0);
            projection.TargetVx = projection.TargetKnockbackVx;
            projection.TargetKnockbackVz = projection.TargetVz +
                (projection.TargetZInt > attacker.Runtime.ZInt ? -0.5 : 0.5);
            projection.TargetVz = projection.TargetKnockbackVz;
            ProjectAirStep(vyStep, ref projection);
        }

        private static void ProjectAirStep(
            double vyStep,
            ref WriterEffectSnapshot projection)
        {
            if (projection.TargetYInt >= -2)
            {
                projection.TargetYInt = -2;
                projection.TargetY = -2.0;
                projection.TargetVy = -6.0;
            }

            if (projection.TargetVy > -6.0)
            {
                projection.TargetVy -= vyStep;
                projection.TargetKnockbackVy = projection.TargetVy;
            }
        }

        private static bool ProjectGrabWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            bool resetAttackerPosition,
            bool strictKind3,
            ref WriterEffectSnapshot projection)
        {
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if (attackerSlot < 0 || targetSlot < 0)
                return false;

            int catchingFrame = resolvedItr.catchingact != null &&
                                resolvedItr.catchingact.Length > 0
                ? resolvedItr.catchingact[0]
                : 0;
            int caughtFrame = resolvedItr.caughtact != null &&
                              resolvedItr.caughtact.Length > 0
                ? resolvedItr.caughtact[0]
                : 0;
            int attackerFacing = projection.AttackerXInt > projection.TargetXInt ? 1 : 0;
            int targetFacing = 1 - attackerFacing;
            if (strictKind3)
            {
                if (catchingFrame < 0)
                {
                    catchingFrame = -catchingFrame;
                    attackerFacing = 1 - attackerFacing;
                }
                if (caughtFrame < 0)
                {
                    caughtFrame = -caughtFrame;
                    targetFacing = 1 - targetFacing;
                }

                if (attacker.FrameCache == null || target.FrameCache == null ||
                    !attacker.FrameCache.HasFrame(catchingFrame) ||
                    !target.FrameCache.HasFrame(caughtFrame))
                {
                    // The writer returns unsupported, but that is still a fully
                    // projectable no-op effect for shadow comparison.
                    return true;
                }
            }

            LF2FrameData attackerFrame = attacker.GetFrameDataById(catchingFrame);
            LF2FrameData targetFrame = target.GetFrameDataById(caughtFrame);

            int attackerWAct = attackerFrame?.PrimaryCatchPoint.X ?? 0;
            int targetWAct = targetFrame?.PrimaryCatchPoint.X ?? 0;
            int attackerCx = attackerFrame?.centerx ?? 0;
            int attackerCy = attackerFrame?.centery ?? 0;
            int targetCx = targetFrame?.centerx ?? 0;
            int targetCy = targetFrame?.centery ?? 0;

            projection.AttackerVx = 0.0;
            projection.TargetVx = 0.0;
            projection.AttackerFacing = attackerFacing;
            projection.TargetFacing = targetFacing;
            projection.AttackerFrame = catchingFrame;
            projection.AttackerRuntimeFrame = catchingFrame;
            projection.TargetFrame = caughtFrame;
            projection.TargetRuntimeFrame = caughtFrame;

            if (resetAttackerPosition)
            {
                projection.AttackerX = projection.AttackerXInt;
                projection.AttackerY = projection.AttackerYInt;
            }

            projection.TargetX = attackerFacing == 0
                ? projection.AttackerXInt - attackerCx - targetCx + attackerWAct + targetWAct
                : attackerCx + targetCx + projection.AttackerXInt - attackerWAct - targetWAct;
            projection.TargetY = targetCy - attackerCy + projection.AttackerYInt;

            double lerp = (projection.TargetXInt - projection.TargetX) * 0.5;
            projection.TargetX += lerp;
            projection.AttackerX += lerp;
            projection.TargetXInt = (int)projection.TargetX;
            projection.AttackerXInt = (int)projection.AttackerX;
            projection.AttackerCaughtSlot = targetSlot;
            projection.TargetCatcherSlot = attackerSlot;
            if (strictKind3)
                projection.TargetCatchSourceSlot90 = attackerSlot;
            projection.AttackerCaughtDuration =
                strictKind3 && resolvedItr.respond != 0
                    ? resolvedItr.respond
                    : 300;
            projection.TargetFall = 0;
            return true;
        }

        private static bool ProjectPickupWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            int kind,
            ref WriterEffectSnapshot projection)
        {
            if (kind != 2)
                return false;

            LF2FrameData targetFrame = target.GetFrameDataById(projection.TargetFrame);
            var input = new BattlePickupTransactionInput(
                projection.TargetDataObjectType,
                projection.TargetDataObjectId != int.MinValue
                    ? projection.TargetDataObjectId
                    : projection.TargetObjectId,
                target.Health != null ? projection.TargetHp : 0,
                projection.TargetWeaponFlightCounter,
                attacker.Runtime.SlotIndex,
                target.Runtime.SlotIndex,
                projection.AttackerRelationTeam,
                projection.AttackerLinkState,
                projection.AttackerPickupCount,
                projection.AttackerFrame,
                projection.AttackerAttackingCounter,
                targetFrame != null,
                targetFrame?.PrimaryWeaponPoint.WeaponAct ?? 0);
            BattlePickupTransactionPlan plan = BattlePickupTransactionPlan.Create(input, BattleLockedPickupWeaponThrowRules.Locked);
            if (!plan.Applied)
                return false;

            for (int i = 0; i < plan.OperationCount; i++)
            {
                BattlePickupWriteOperation operation = plan.GetOperation(i);
                switch (operation.Kind)
                {
                    case BattlePickupWriteKind.SetTargetWeaponHp:
                        projection.TargetWeaponFlightCounter = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderRelationCount:
                        projection.AttackerPickupCount = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderRelation:
                        projection.AttackerLinkState = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetRelation:
                        projection.TargetLinkState = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderLinkedChildSlot:
                        projection.AttackerTargetSlot = operation.Value;
                        projection.AttackerHeldWeaponSlot = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetLinkedParentSlot:
                        projection.TargetHolderSlot = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetOwnerSlot:
                        projection.TargetOwnerSlot = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetTargetBattleGroup:
                        projection.TargetRelationTeam = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderAction:
                        projection.AttackerFrame = operation.Value;
                        projection.AttackerRuntimeFrame = operation.Value;
                        break;
                    case BattlePickupWriteKind.SetHolderFrameCounter:
                        projection.AttackerAttackingCounter = operation.Value;
                        break;
                }
            }

            return true;
        }

        private RuntimeEntityHandle ResolveCurrentHandle(
            int slot,
            LF2Entity entity)
        {
            return slot >= 0 &&
                   entity != null &&
                   world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle)
                ? handle
                : RuntimeEntityHandle.Invalid;
        }

        private bool ProjectConsumeEffects(
            LF2Entity attacker,
            LF2Entity target,
            in Entry entry,
            ref ConsumeEffectsSnapshot projection)
        {
            bool valid = true;
            // Consume effects are driven by the current-pair preprocess result, not
            // by the raw flags frozen when collision candidates were collected.
            // The authority re-resolves kind/link state immediately before consume.
            if (entry.ExpectedZeroAttackerHpAfterPreprocess)
            {
                if (attacker?.Health == null)
                    valid = false;
                projection.AttackerHp = 0;
            }

            if (!entry.ExpectedReleaseHeavyHeldTargetAfterPreprocess)
                return valid;

            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            int heldTargetSlot = projection.HeldTargetHandle.Slot;
            LF2Entity heldTarget = heldTargetSlot >= 0
                ? world.FindEntityByRuntimeSlotForQuery(heldTargetSlot)
                : null;
            bool relationValid =
                attackerSlot >= 0 &&
                targetSlot >= 0 &&
                target?.Runtime != null &&
                target.Runtime.LinkState == 2 &&
                target.Runtime.TargetSlotIndex == heldTargetSlot &&
                target.ItrRest != null &&
                heldTarget?.Runtime != null &&
                heldTarget.Runtime.LinkState == -2 &&
                heldTarget.Runtime.HolderStableId == targetSlot &&
                projection.HeldTargetHandle.IsValid;
            if (!relationValid)
                valid = false;

            projection.TargetVrestAgainstAttacker =
                attackerSlot == heldTargetSlot ? 30 : 45;
            projection.TargetVrestAgainstHeld = 30;
            projection.TargetLinkState = 0;
            projection.HeldTargetLinkState = 0;
            unchecked
            {
                projection.RngState =
                    projection.RngState * 0x343FDu + 0x269EC3u;
            }
            projection.RngCallCount++;
            projection.HeldTargetFrame =
                (int)((projection.RngState >> 16) & 0x7FFFu) % 6;
            projection.HeldTargetVy = -1.0;
            return valid;
        }

        private static void ProjectRuntimeItr(
            LF2Entity attacker,
            LF2Entity target,
            ref ItrProjection projection,
            out bool zeroAttackerHpOnConsume,
            out bool releaseHeavyHeldTargetOnConsume)
        {
            zeroAttackerHpOnConsume = false;
            releaseHeavyHeldTargetOnConsume = false;

            if (projection.Kind == 4 &&
                attacker.Runtime != null &&
                attacker.Runtime.EnvironmentState320 > 0)
            {
                projection.Kind = 0;
                bool facingRight = attacker.Dirh() > 0;
                double vx = attacker.Runtime?.Vx ?? 0.0;
                if ((vx > 0.0 && !facingRight) ||
                    (vx < 0.0 && facingRight))
                {
                    projection.Dvx = -projection.Dvx;
                }
            }

            if (target.Runtime != null &&
                target.Runtime.LinkState == 2 &&
                projection.Kind == 0)
            {
                int heldTargetSlot = target.Runtime.TargetSlotIndex;
                LF2Entity heldTarget = heldTargetSlot >= 0
                    ? target.Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot)
                    : null;
                if (heldTarget?.Runtime != null &&
                    heldTarget.Runtime.HolderStableId == target.Runtime.SlotIndex &&
                    heldTarget.Runtime.LinkState == -2)
                {
                    releaseHeavyHeldTargetOnConsume = true;
                }
            }

            if (projection.Kind == 5 && attacker.Runtime.LinkState < 0)
            {
                int holderSlot = attacker.ResolveReleaseNeutralHolderSlotOrImplicitZero();
                LF2Entity holder = holderSlot >= 0
                    ? attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                LF2FrameData holderFrame = holder?.GetCollisionFrameData();
                int attackerSlot = attacker.Runtime?.SlotIndex ?? -1;
                if (holder?.Runtime != null &&
                    holderFrame != null &&
                    holder.Runtime.TargetSlotIndex == attackerSlot)
                {
                    int attackingItrIndex =
                        holderFrame.PrimaryWeaponPoint.Attacking;
                    int targetSlot = target.Runtime?.SlotIndex ?? -1;
                    if (attackingItrIndex > 0 &&
                        holderSlot != targetSlot &&
                        holderFrame.itrs != null &&
                        attackingItrIndex < holderFrame.itrs.Count)
                    {
                        projection.ApplyKind5Replacement(
                            holderFrame.itrs[attackingItrIndex]);
                    }
                }
            }

            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            if (targetType == (int)LF2ObjectType.HeavyWeapon)
            {
                projection.Dvx /= 2;
                projection.Dvy /= 2;
            }

            if (projection.Kind != 9)
                return;

            if (targetType == (int)LF2ObjectType.Character)
            {
                projection.Kind = 0;
                zeroAttackerHpOnConsume = true;
                return;
            }

            int targetState = target.Frame?.D?.state ?? 0;
            if (targetState == 1002 || targetState == 2000)
                projection.Kind = 0;
        }

        private static ulong Fingerprint(InteractionArea itr)
        {
            if (itr == null)
                return 0;

            const ulong offset = 1469598103934665603UL;
            ulong hash = offset;
            Add(ref hash, itr.kind);
            Add(ref hash, itr.x);
            Add(ref hash, itr.y);
            Add(ref hash, itr.w);
            Add(ref hash, itr.h);
            Add(ref hash, itr.zwidth);
            Add(ref hash, itr.z);
            Add(ref hash, itr.hasGeometry ? 1 : 0);
            Add(ref hash, itr.dvx);
            Add(ref hash, itr.dvy);
            Add(ref hash, itr.dvz);
            Add(ref hash, itr.injury);
            Add(ref hash, itr.drain);
            Add(ref hash, itr.sound);
            Add(ref hash, itr.cover);
            Add(ref hash, itr.fall);
            Add(ref hash, itr.vaction);
            Add(ref hash, itr.arest);
            Add(ref hash, itr.vrest);
            Add(ref hash, itr.effect);
            Add(ref hash, itr.spark);
            Add(ref hash, itr.recover);
            Add(ref hash, itr.dbdefend);
            Add(ref hash, itr.kill);
            Add(ref hash, itr.bdefend);
            Add(ref hash, itr.attacking);
            Add(ref hash, itr.throwvz);
            Add(ref hash, itr.respond);
            Add(ref hash, itr.pickingact);
            Add(ref hash, itr.pickedact);
            Add(ref hash, itr.delay);
            Add(ref hash, itr.poison);
            Add(ref hash, itr.confus);
            Add(ref hash, itr.weak);
            Add(ref hash, itr.manacle);
            Add(ref hash, itr.join);
            Add(ref hash, itr.mimic);
            Add(ref hash, itr.bound);
            Add(ref hash, itr.facing);
            Add(ref hash, itr.dx);
            Add(ref hash, itr.dy);
            Add(ref hash, itr.dz);
            Add(ref hash, itr.gain);
            Add(ref hash, itr.throwvx);
            Add(ref hash, itr.throwvy);
            Add(ref hash, itr.throwinjury);
            Add(ref hash, itr.catchingact);
            Add(ref hash, itr.caughtact);
            Add(ref hash, itr.catchingact2);
            Add(ref hash, itr.caughtact2);
            return hash;
        }

        private static ulong Fingerprint(in ItrProjection itr)
        {
            const ulong offset = 1469598103934665603UL;
            ulong hash = offset;
            Add(ref hash, itr.Kind);
            Add(ref hash, itr.X);
            Add(ref hash, itr.Y);
            Add(ref hash, itr.W);
            Add(ref hash, itr.H);
            Add(ref hash, itr.Zwidth);
            Add(ref hash, itr.Z);
            Add(ref hash, itr.HasGeometry ? 1 : 0);
            Add(ref hash, itr.Dvx);
            Add(ref hash, itr.Dvy);
            Add(ref hash, itr.Dvz);
            Add(ref hash, itr.Injury);
            Add(ref hash, itr.Drain);
            Add(ref hash, itr.Sound);
            Add(ref hash, itr.Cover);
            Add(ref hash, itr.Fall);
            Add(ref hash, itr.Vaction);
            Add(ref hash, itr.Arest);
            Add(ref hash, itr.Vrest);
            Add(ref hash, itr.Effect);
            Add(ref hash, itr.Spark);
            Add(ref hash, itr.Recover);
            Add(ref hash, itr.Dbdefend);
            Add(ref hash, itr.Kill);
            Add(ref hash, itr.Bdefend);
            Add(ref hash, itr.Attacking);
            Add(ref hash, itr.ThrowVz);
            Add(ref hash, itr.Respond);
            Add(ref hash, itr.PickingAct);
            Add(ref hash, itr.PickedAct);
            Add(ref hash, itr.Delay);
            Add(ref hash, itr.Poison);
            Add(ref hash, itr.Confus);
            Add(ref hash, itr.Weak);
            Add(ref hash, itr.Manacle);
            Add(ref hash, itr.Join);
            Add(ref hash, itr.Mimic);
            Add(ref hash, itr.Bound);
            Add(ref hash, itr.Facing);
            Add(ref hash, itr.Dx);
            Add(ref hash, itr.Dy);
            Add(ref hash, itr.Dz);
            Add(ref hash, itr.Gain);
            Add(ref hash, itr.ThrowVx);
            Add(ref hash, itr.ThrowVy);
            Add(ref hash, itr.ThrowInjury);
            Add(ref hash, itr.CatchingAct);
            Add(ref hash, itr.CaughtAct);
            Add(ref hash, itr.CatchingAct2);
            Add(ref hash, itr.CaughtAct2);
            return hash;
        }

        private static ulong Fingerprint(in ConsumeEffectsSnapshot snapshot)
        {
            const ulong offset = 1469598103934665603UL;
            ulong hash = offset;
            Add(ref hash, snapshot.AttackerHandle.Slot);
            Add(ref hash, snapshot.AttackerHandle.Generation);
            Add(ref hash, snapshot.AttackerHp);
            Add(ref hash, snapshot.TargetHandle.Slot);
            Add(ref hash, snapshot.TargetHandle.Generation);
            Add(ref hash, snapshot.TargetLinkState);
            Add(ref hash, snapshot.TargetTargetSlot);
            Add(ref hash, snapshot.TargetVrestAgainstAttacker);
            Add(ref hash, snapshot.TargetVrestAgainstHeld);
            Add(ref hash, snapshot.HeldTargetHandle.Slot);
            Add(ref hash, snapshot.HeldTargetHandle.Generation);
            Add(ref hash, snapshot.HeldTargetLinkState);
            Add(ref hash, snapshot.HeldTargetHolderSlot);
            Add(ref hash, snapshot.HeldTargetFrame);
            Add(ref hash, BitConverter.DoubleToInt64Bits(snapshot.HeldTargetVy));
            Add(ref hash, snapshot.RngState);
            Add(ref hash, snapshot.RngCallCount);
            return hash;
        }

        private static ulong DifferenceMask(
            in FirstBodyResponseAttemptSnapshot expected,
            in FirstBodyResponseAttemptSnapshot actual)
        {
            ulong mask = 0;
            if (expected.AttackerHandle != actual.AttackerHandle) mask |= 1UL << 0;
            if (expected.TargetHandle != actual.TargetHandle) mask |= 1UL << 1;
            if (expected.AttemptEligible != actual.AttemptEligible ||
                expected.ResponseKind != actual.ResponseKind) mask |= 1UL << 2;
            if (expected.AttackerFrame != actual.AttackerFrame ||
                expected.AttackerRuntimeFrame != actual.AttackerRuntimeFrame ||
                expected.AttackerFrameCounter != actual.AttackerFrameCounter) mask |= 1UL << 3;
            if (expected.AttackerGroup != actual.AttackerGroup ||
                expected.AttackerFrameDelay != actual.AttackerFrameDelay) mask |= 1UL << 4;
            if (expected.AttackerInputScore != actual.AttackerInputScore) mask |= 1UL << 5;
            if (expected.TargetFrame != actual.TargetFrame ||
                expected.TargetRuntimeFrame != actual.TargetRuntimeFrame ||
                expected.TargetFrameCounter != actual.TargetFrameCounter) mask |= 1UL << 6;
            if (expected.TargetGroup != actual.TargetGroup ||
                expected.TargetFrameDelay != actual.TargetFrameDelay) mask |= 1UL << 7;
            if (expected.TargetHp != actual.TargetHp ||
                expected.TargetHpBound != actual.TargetHpBound ||
                expected.TargetRuntimeArmorHp != actual.TargetRuntimeArmorHp ||
                expected.TargetInputHpConsumed != actual.TargetInputHpConsumed) mask |= 1UL << 8;
            if (expected.RngCrtState != actual.RngCrtState ||
                expected.RngCrtCalls != actual.RngCrtCalls) mask |= 1UL << 9;
            if (expected.RngTableSeed != actual.RngTableSeed ||
                expected.RngTableHash != actual.RngTableHash ||
                expected.RngGeneration != actual.RngGeneration) mask |= 1UL << 10;
            if (expected.RngCounter != actual.RngCounter ||
                expected.RngIndex != actual.RngIndex ||
                expected.RngCalls != actual.RngCalls ||
                expected.RngLastCallSite != actual.RngLastCallSite) mask |= 1UL << 11;
            return mask;
        }

        private static ulong DifferenceMask(
            in ConsumeEffectsSnapshot expected,
            in ConsumeEffectsSnapshot actual)
        {
            ulong mask = 0;
            if (expected.AttackerHandle != actual.AttackerHandle) mask |= 1UL << 0;
            if (expected.AttackerHp != actual.AttackerHp) mask |= 1UL << 1;
            if (expected.TargetHandle != actual.TargetHandle) mask |= 1UL << 2;
            if (expected.TargetLinkState != actual.TargetLinkState) mask |= 1UL << 3;
            if (expected.TargetTargetSlot != actual.TargetTargetSlot) mask |= 1UL << 4;
            if (expected.TargetVrestAgainstAttacker != actual.TargetVrestAgainstAttacker) mask |= 1UL << 5;
            if (expected.TargetVrestAgainstHeld != actual.TargetVrestAgainstHeld) mask |= 1UL << 6;
            if (expected.HeldTargetHandle != actual.HeldTargetHandle) mask |= 1UL << 7;
            if (expected.HeldTargetLinkState != actual.HeldTargetLinkState) mask |= 1UL << 8;
            if (expected.HeldTargetHolderSlot != actual.HeldTargetHolderSlot) mask |= 1UL << 9;
            if (expected.HeldTargetFrame != actual.HeldTargetFrame) mask |= 1UL << 10;
            if (BitConverter.DoubleToInt64Bits(expected.HeldTargetVy) !=
                BitConverter.DoubleToInt64Bits(actual.HeldTargetVy)) mask |= 1UL << 11;
            if (expected.RngState != actual.RngState) mask |= 1UL << 12;
            if (expected.RngCallCount != actual.RngCallCount) mask |= 1UL << 13;
            return mask;
        }

        private static ulong DifferenceMask(
            in WriterEffectSnapshot expected,
            in WriterEffectSnapshot actual)
        {
            ulong mask = 0;
            if (expected.AttackerHandle != actual.AttackerHandle) mask |= 1UL << 0;
            if (expected.TargetHandle != actual.TargetHandle) mask |= 1UL << 1;
            if (expected.AttackerFrame != actual.AttackerFrame) mask |= 1UL << 2;
            if (expected.AttackerRuntimeFrame != actual.AttackerRuntimeFrame) mask |= 1UL << 3;
            if (BitConverter.DoubleToInt64Bits(expected.AttackerX) !=
                BitConverter.DoubleToInt64Bits(actual.AttackerX)) mask |= 1UL << 4;
            if (BitConverter.DoubleToInt64Bits(expected.AttackerY) !=
                BitConverter.DoubleToInt64Bits(actual.AttackerY)) mask |= 1UL << 5;
            if (BitConverter.DoubleToInt64Bits(expected.AttackerZ) !=
                BitConverter.DoubleToInt64Bits(actual.AttackerZ)) mask |= 1UL << 6;
            if (expected.AttackerXInt != actual.AttackerXInt) mask |= 1UL << 7;
            if (expected.AttackerYInt != actual.AttackerYInt) mask |= 1UL << 8;
            if (expected.AttackerZInt != actual.AttackerZInt) mask |= 1UL << 9;
            if (BitConverter.DoubleToInt64Bits(expected.AttackerVx) !=
                    BitConverter.DoubleToInt64Bits(actual.AttackerVx) ||
                BitConverter.DoubleToInt64Bits(expected.AttackerVy) !=
                    BitConverter.DoubleToInt64Bits(actual.AttackerVy) ||
                BitConverter.DoubleToInt64Bits(expected.AttackerVz) !=
                    BitConverter.DoubleToInt64Bits(actual.AttackerVz) ||
                BitConverter.DoubleToInt64Bits(expected.AttackerKnockbackVx) !=
                    BitConverter.DoubleToInt64Bits(actual.AttackerKnockbackVx) ||
                BitConverter.DoubleToInt64Bits(expected.AttackerKnockbackVy) !=
                    BitConverter.DoubleToInt64Bits(actual.AttackerKnockbackVy) ||
                BitConverter.DoubleToInt64Bits(expected.AttackerKnockbackVz) !=
                    BitConverter.DoubleToInt64Bits(actual.AttackerKnockbackVz)) mask |= 1UL << 10;
            if (expected.AttackerFacing != actual.AttackerFacing) mask |= 1UL << 11;
            if (expected.AttackerRelationTeam != actual.AttackerRelationTeam) mask |= 1UL << 12;
            if (expected.AttackerLinkState != actual.AttackerLinkState) mask |= 1UL << 13;
            if (expected.AttackerTargetSlot != actual.AttackerTargetSlot) mask |= 1UL << 14;
            if (expected.AttackerCaughtSlot != actual.AttackerCaughtSlot) mask |= 1UL << 15;
            if (expected.AttackerCaughtDuration != actual.AttackerCaughtDuration) mask |= 1UL << 16;
            if (expected.AttackerHeldWeaponSlot != actual.AttackerHeldWeaponSlot) mask |= 1UL << 17;
            if (expected.AttackerPickupCount != actual.AttackerPickupCount) mask |= 1UL << 18;
            if (expected.AttackerAttackingCounter != actual.AttackerAttackingCounter ||
                expected.AttackerFrameDelay != actual.AttackerFrameDelay ||
                expected.AttackerAttackExempt != actual.AttackerAttackExempt ||
                expected.AttackerItrArest != actual.AttackerItrArest ||
                expected.AttackerHp != actual.AttackerHp ||
                expected.AttackerKind4SourceCount !=
                    actual.AttackerKind4SourceCount) mask |= 1UL << 19;
            if (expected.TargetFrame != actual.TargetFrame ||
                expected.TargetPrevFrame != actual.TargetPrevFrame ||
                expected.TargetWaitCounter != actual.TargetWaitCounter) mask |= 1UL << 20;
            if (expected.TargetRuntimeFrame != actual.TargetRuntimeFrame) mask |= 1UL << 21;
            if (BitConverter.DoubleToInt64Bits(expected.TargetX) !=
                BitConverter.DoubleToInt64Bits(actual.TargetX)) mask |= 1UL << 22;
            if (BitConverter.DoubleToInt64Bits(expected.TargetY) !=
                BitConverter.DoubleToInt64Bits(actual.TargetY)) mask |= 1UL << 23;
            if (BitConverter.DoubleToInt64Bits(expected.TargetZ) !=
                BitConverter.DoubleToInt64Bits(actual.TargetZ)) mask |= 1UL << 24;
            if (expected.TargetXInt != actual.TargetXInt) mask |= 1UL << 25;
            if (expected.TargetYInt != actual.TargetYInt) mask |= 1UL << 26;
            if (expected.TargetZInt != actual.TargetZInt) mask |= 1UL << 27;
            if (BitConverter.DoubleToInt64Bits(expected.TargetVx) !=
                BitConverter.DoubleToInt64Bits(actual.TargetVx)) mask |= 1UL << 28;
            if (expected.TargetFacing != actual.TargetFacing) mask |= 1UL << 29;
            if (expected.TargetLinkState != actual.TargetLinkState) mask |= 1UL << 30;
            if (expected.TargetCatcherSlot != actual.TargetCatcherSlot ||
                expected.TargetCatchSourceSlot90 !=
                    actual.TargetCatchSourceSlot90 ||
                expected.TargetImpactSourceSlot164 != actual.TargetImpactSourceSlot164) mask |= 1UL << 31;
            if (expected.TargetHolderSlot != actual.TargetHolderSlot) mask |= 1UL << 32;
            if (expected.TargetHolderCopySlot != actual.TargetHolderCopySlot) mask |= 1UL << 33;
            if (expected.TargetRelationTeam != actual.TargetRelationTeam ||
                expected.TargetOwnerSlot != actual.TargetOwnerSlot) mask |= 1UL << 34;
            if (expected.TargetWeaponFlightCounter != actual.TargetWeaponFlightCounter) mask |= 1UL << 35;
            if (expected.TargetFall != actual.TargetFall ||
                expected.TargetFrameDelay != actual.TargetFrameDelay) mask |= 1UL << 36;
            if (expected.TargetHitConfirmCounter != actual.TargetHitConfirmCounter ||
                expected.TargetHitConfirm2 != actual.TargetHitConfirm2 ||
                expected.TargetSpecialHitLatch0EB != actual.TargetSpecialHitLatch0EB ||
                expected.TargetAnimCounter != actual.TargetAnimCounter ||
                expected.TargetHitCount != actual.TargetHitCount ||
                expected.TargetHitStateCount != actual.TargetHitStateCount) mask |= 1UL << 37;
            if (expected.TargetHealTimer != actual.TargetHealTimer ||
                expected.TargetEnvironmentState320 != actual.TargetEnvironmentState320) mask |= 1UL << 38;
            if (expected.TargetXBoundPositive != actual.TargetXBoundPositive) mask |= 1UL << 39;
            if (expected.TargetXBoundNegative != actual.TargetXBoundNegative) mask |= 1UL << 40;
            if (expected.TargetZBoundPositive != actual.TargetZBoundPositive) mask |= 1UL << 41;
            if (expected.TargetZBoundNegative != actual.TargetZBoundNegative) mask |= 1UL << 42;
            if (BitConverter.DoubleToInt64Bits(expected.TargetVy) !=
                BitConverter.DoubleToInt64Bits(actual.TargetVy)) mask |= 1UL << 43;
            if (BitConverter.DoubleToInt64Bits(expected.TargetVz) !=
                BitConverter.DoubleToInt64Bits(actual.TargetVz)) mask |= 1UL << 44;
            if (BitConverter.DoubleToInt64Bits(expected.TargetKnockbackVx) !=
                BitConverter.DoubleToInt64Bits(actual.TargetKnockbackVx)) mask |= 1UL << 45;
            if (BitConverter.DoubleToInt64Bits(expected.TargetKnockbackVy) !=
                BitConverter.DoubleToInt64Bits(actual.TargetKnockbackVy)) mask |= 1UL << 46;
            if (BitConverter.DoubleToInt64Bits(expected.TargetKnockbackVz) !=
                BitConverter.DoubleToInt64Bits(actual.TargetKnockbackVz)) mask |= 1UL << 47;
            if (expected.TargetWeaponCount != actual.TargetWeaponCount ||
                expected.TargetObjectId != actual.TargetObjectId ||
                expected.TargetDataObjectId != actual.TargetDataObjectId ||
                expected.TargetDataObjectType != actual.TargetDataObjectType) mask |= 1UL << 48;
            if (expected.HolderHandle != actual.HolderHandle ||
                expected.ActiveHolderHandle != actual.ActiveHolderHandle ||
                expected.ActiveHolderFrameDelay != actual.ActiveHolderFrameDelay)
            {
                mask |= 1UL << 49;
            }
            if (expected.HolderComboCountAtk != actual.HolderComboCountAtk ||
                expected.StandardCreditHandle != actual.StandardCreditHandle ||
                expected.StandardCreditInputScore !=
                    actual.StandardCreditInputScore ||
                expected.StandardCreditKnockoutCount !=
                    actual.StandardCreditKnockoutCount) mask |= 1UL << 50;
            if (expected.TargetDamageStat != actual.TargetDamageStat) mask |= 1UL << 51;
            if (expected.TargetHp != actual.TargetHp) mask |= 1UL << 52;
            if (expected.TargetHpBound != actual.TargetHpBound) mask |= 1UL << 53;
            if (expected.TargetPp != actual.TargetPp) mask |= 1UL << 53;
            if (expected.TargetRuntimeArmorHp != actual.TargetRuntimeArmorHp ||
                expected.TargetInputHpConsumedTotal != actual.TargetInputHpConsumedTotal ||
                expected.TargetInputMpConsumedTotal != actual.TargetInputMpConsumedTotal)
            {
                mask |= 1UL << 53;
            }
            if (expected.TargetComboCountVic != actual.TargetComboCountVic) mask |= 1UL << 54;
            if (expected.TargetAttackingCounter != actual.TargetAttackingCounter) mask |= 1UL << 55;
            if (expected.HolderKillStat != actual.HolderKillStat) mask |= 1UL << 56;
            if (expected.TargetKillStat != actual.TargetKillStat) mask |= 1UL << 57;
            if (expected.TargetVrestAgainstAttacker != actual.TargetVrestAgainstAttacker) mask |= 1UL << 58;
            if (expected.TargetVrestAgainstHeld != actual.TargetVrestAgainstHeld ||
                expected.HeldTargetVrestAgainstAttacker !=
                    actual.HeldTargetVrestAgainstAttacker ||
                expected.AttackerVrestAgainstHeld != actual.AttackerVrestAgainstHeld ||
                expected.AttackerVrestAgainstAttacker != actual.AttackerVrestAgainstAttacker) mask |= 1UL << 59;
            if (expected.TargetTargetSlot != actual.TargetTargetSlot ||
                expected.HeldTargetHandle != actual.HeldTargetHandle ||
                expected.HeldTargetLinkState != actual.HeldTargetLinkState ||
                expected.HeldTargetHolderSlot != actual.HeldTargetHolderSlot) mask |= 1UL << 60;
            if (expected.HeldTargetFrame != actual.HeldTargetFrame ||
                expected.HeldTargetRuntimeFrame != actual.HeldTargetRuntimeFrame ||
                BitConverter.DoubleToInt64Bits(expected.HeldTargetVy) !=
                BitConverter.DoubleToInt64Bits(actual.HeldTargetVy)) mask |= 1UL << 61;
            if (expected.RngState != actual.RngState ||
                expected.RngCallCount != actual.RngCallCount ||
                expected.HitRecordOwnerHandle != actual.HitRecordOwnerHandle ||
                expected.HitRecordCount != actual.HitRecordCount ||
                expected.HitRecordDamage != actual.HitRecordDamage ||
                expected.HitRecordX != actual.HitRecordX ||
                expected.HitRecordZ != actual.HitRecordZ) mask |= 1UL << 62;
            if (expected.PendingSoundCount != actual.PendingSoundCount ||
                expected.PendingSoundFingerprint != actual.PendingSoundFingerprint ||
                !string.Equals(expected.PendingSoundCue, actual.PendingSoundCue, StringComparison.Ordinal) ||
                expected.PendingSoundWorldX != actual.PendingSoundWorldX ||
                expected.PendingSoundTick != actual.PendingSoundTick ||
                expected.QueuedSoundEventCount != actual.QueuedSoundEventCount ||
                expected.RejectedSoundEventCount != actual.RejectedSoundEventCount) mask |= 1UL << 63;
            return mask;
        }

        private static ulong DifferenceMask(
            in LifecycleEffectSnapshot expected,
            in LifecycleEffectSnapshot actual)
        {
            ulong mask = 0;
            if (expected.AttackerHandle != actual.AttackerHandle) mask |= 1UL << 0;
            if (expected.SlotClaimed != actual.SlotClaimed) mask |= 1UL << 1;
            if (expected.SlotGeneration != actual.SlotGeneration) mask |= 1UL << 2;
            if (!ReferenceEquals(expected.SlotOccupant, actual.SlotOccupant)) mask |= 1UL << 3;
            if (expected.AttackerRuntimeSlot != actual.AttackerRuntimeSlot) mask |= 1UL << 4;
            return mask;
        }

        private static uint NextGeneration(uint generation)
        {
            generation++;
            return generation == 0 ? 1u : generation;
        }

        private static void Add(ref ulong hash, int value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 1099511628211UL;
            }
        }

        private static void Add(ref ulong hash, uint value)
        {
            unchecked
            {
                hash ^= value;
                hash *= 1099511628211UL;
            }
        }

        private static void Add(ref ulong hash, long value)
        {
            Add(ref hash, unchecked((ulong)value));
        }

        private static void Add(ref ulong hash, ulong value)
        {
            unchecked
            {
                hash ^= (uint)value;
                hash *= 1099511628211UL;
                hash ^= (uint)(value >> 32);
                hash *= 1099511628211UL;
            }
        }

        private static void Add(ref ulong hash, int[] values)
        {
            if (values == null)
            {
                Add(ref hash, -1);
                return;
            }

            Add(ref hash, values.Length);
            for (int index = 0; index < values.Length; index++)
                Add(ref hash, values[index]);
        }

        private static ulong FingerprintPendingSounds(SimulationWorld sourceWorld)
        {
            const ulong offset = 1469598103934665603UL;
            ulong hash = offset;
            if (sourceWorld?.PendingSounds == null)
                return hash;

            for (int index = 0; index < sourceWorld.PendingSounds.Count; index++)
            {
                PendingSoundEvent sound = sourceWorld.PendingSounds[index];
                AddPendingSoundFingerprint(
                    ref hash,
                    sound.Cue,
                    sound.WorldX,
                    sound.Tick);
            }

            return hash;
        }

        private static void AddPendingSoundFingerprint(
            ref ulong hash,
            string cue,
            int worldX,
            int tick)
        {
            if (cue == null)
            {
                Add(ref hash, -1);
            }
            else
            {
                Add(ref hash, cue.Length);
                for (int index = 0; index < cue.Length; index++)
                    Add(ref hash, cue[index]);
            }

            Add(ref hash, worldX);
            Add(ref hash, tick);
        }

        private struct FirstBodyResponseAttemptSnapshot
        {
            internal RuntimeEntityHandle AttackerHandle;
            internal RuntimeEntityHandle TargetHandle;
            internal bool AttemptEligible;
            internal BattleFirstBodyResponseKind ResponseKind;
            internal int AttackerFrame;
            internal int AttackerRuntimeFrame;
            internal int AttackerFrameCounter;
            internal int AttackerGroup;
            internal int AttackerFrameDelay;
            internal int AttackerInputScore;
            internal int TargetFrame;
            internal int TargetRuntimeFrame;
            internal int TargetFrameCounter;
            internal int TargetGroup;
            internal int TargetFrameDelay;
            internal int TargetHp;
            internal int TargetHpBound;
            internal int TargetRuntimeArmorHp;
            internal int TargetInputHpConsumed;
            internal uint RngCrtState;
            internal ulong RngCrtCalls;
            internal uint RngTableSeed;
            internal int RngCounter;
            internal int RngIndex;
            internal ulong RngCalls;
            internal uint RngLastCallSite;
            internal ulong RngTableHash;
            internal uint RngGeneration;
        }

        private struct ConsumeEffectsSnapshot
        {
            internal RuntimeEntityHandle AttackerHandle;
            internal int AttackerHp;
            internal RuntimeEntityHandle TargetHandle;
            internal int TargetLinkState;
            internal int TargetTargetSlot;
            internal int TargetVrestAgainstAttacker;
            internal int TargetVrestAgainstHeld;
            internal RuntimeEntityHandle HeldTargetHandle;
            internal int HeldTargetLinkState;
            internal int HeldTargetHolderSlot;
            internal int HeldTargetFrame;
            internal double HeldTargetVy;
            internal uint RngState;
            internal ulong RngCallCount;
        }

        private struct LifecycleEffectSnapshot
        {
            internal RuntimeEntityHandle AttackerHandle;
            internal bool SlotClaimed;
            internal uint SlotGeneration;
            internal LF2Entity SlotOccupant;
            internal int AttackerRuntimeSlot;
        }

        private struct WriterEffectSnapshot
        {
            internal RuntimeEntityHandle AttackerHandle;
            internal int AttackerFrame;
            internal int AttackerRuntimeFrame;
            internal double AttackerX;
            internal double AttackerY;
            internal double AttackerZ;
            internal int AttackerXInt;
            internal int AttackerYInt;
            internal int AttackerZInt;
            internal double AttackerVx;
            internal double AttackerVy;
            internal double AttackerVz;
            internal double AttackerKnockbackVx;
            internal double AttackerKnockbackVy;
            internal double AttackerKnockbackVz;
            internal int AttackerFacing;
            internal int AttackerRelationTeam;
            internal int AttackerLinkState;
            internal int AttackerTargetSlot;
            internal int AttackerCaughtSlot;
            internal int AttackerCaughtDuration;
            internal int AttackerHeldWeaponSlot;
            internal int AttackerPickupCount;
            internal int AttackerAttackingCounter;
            internal int AttackerFrameDelay;
            internal int AttackerAttackExempt;
            internal int AttackerItrArest;
            internal int AttackerHp;
            internal int AttackerKind4SourceCount;
            internal RuntimeEntityHandle StandardCreditHandle;
            internal int StandardCreditInputScore;
            internal int StandardCreditKnockoutCount;
            internal RuntimeEntityHandle TargetHandle;
            internal int TargetFrame;
            internal int TargetRuntimeFrame;
            internal int TargetPrevFrame;
            internal int TargetWaitCounter;
            internal int TargetObjectId;
            internal int TargetDataObjectId;
            internal int TargetDataObjectType;
            internal double TargetX;
            internal double TargetY;
            internal double TargetZ;
            internal int TargetXInt;
            internal int TargetYInt;
            internal int TargetZInt;
            internal double TargetVx;
            internal double TargetVy;
            internal double TargetVz;
            internal double TargetKnockbackVx;
            internal double TargetKnockbackVy;
            internal double TargetKnockbackVz;
            internal int TargetFacing;
            internal int TargetLinkState;
            internal int TargetCatcherSlot;
            internal int TargetCatchSourceSlot90;
            internal int TargetEnvironmentState320;
            internal int TargetImpactSourceSlot164;
            internal int TargetHolderSlot;
            internal int TargetHolderCopySlot;
            internal int TargetOwnerSlot;
            internal int TargetRelationTeam;
            internal int TargetWeaponFlightCounter;
            internal int TargetWeaponCount;
            internal int TargetFall;
            internal int TargetHitConfirmCounter;
            internal int TargetHitConfirm2;
            internal bool TargetSpecialHitLatch0EB;
            internal int TargetAnimCounter;
            internal int TargetHealTimer;
            internal bool TargetXBoundPositive;
            internal bool TargetXBoundNegative;
            internal bool TargetZBoundPositive;
            internal bool TargetZBoundNegative;
            internal RuntimeEntityHandle HolderHandle;
            internal int HolderComboCountAtk;
            internal int HolderKillStat;
            internal RuntimeEntityHandle ActiveHolderHandle;
            internal int ActiveHolderFrameDelay;
            internal int TargetHp;
            internal int TargetHpBound;
            internal int TargetPp;
            internal int TargetRuntimeArmorHp;
            internal int TargetInputHpConsumedTotal;
            internal int TargetInputMpConsumedTotal;
            internal int TargetComboCountVic;
            internal int TargetAttackingCounter;
            internal int TargetFrameDelay;
            internal int TargetHitCount;
            internal int TargetHitStateCount;
            internal int TargetKillStat;
            internal int TargetDamageStat;
            internal int TargetVrestAgainstAttacker;
            internal int TargetVrestAgainstHeld;
            internal int HeldTargetVrestAgainstAttacker;
            internal int AttackerVrestAgainstHeld;
            internal int AttackerVrestAgainstAttacker;
            internal int TargetTargetSlot;
            internal RuntimeEntityHandle HeldTargetHandle;
            internal int HeldTargetLinkState;
            internal int HeldTargetHolderSlot;
            internal int HeldTargetFrame;
            internal int HeldTargetRuntimeFrame;
            internal double HeldTargetVy;
            internal RuntimeEntityHandle HitRecordOwnerHandle;
            internal int HitRecordCount;
            internal int HitRecordDamage;
            internal int HitRecordX;
            internal int HitRecordZ;
            internal uint RngState;
            internal ulong RngCallCount;
            internal int PendingSoundCount;
            internal ulong PendingSoundFingerprint;
            internal string PendingSoundCue;
            internal int PendingSoundWorldX;
            internal int PendingSoundTick;
            internal long QueuedSoundEventCount;
            internal long RejectedSoundEventCount;
        }

        private struct Entry
        {
            internal Entry(
                BattleHitExecutionPass pass,
                RuntimeEntityHandle attackerHandle,
                int attackerStableId,
                int attackerPrevFrame2,
                int candidateOrdinal,
                int targetSlot,
                RuntimeEntityHandle targetHandleSnapshot,
                int itrIndex,
                int bodyX,
                int itrKind,
                ulong sourceItrFingerprint,
                ulong recordedItrFingerprint,
                bool zeroAttackerHpOnConsume,
                bool releaseHeavyHeldTargetOnConsume,
                BattleHitCandidatePairSnapshot pairSnapshot)
            {
                Pass = pass;
                AttackerHandle = attackerHandle;
                AttackerStableId = attackerStableId;
                AttackerPrevFrame2 = attackerPrevFrame2;
                CandidateOrdinal = candidateOrdinal;
                TargetSlot = targetSlot;
                TargetHandleSnapshot = targetHandleSnapshot;
                ItrIndex = itrIndex;
                BodyX = bodyX;
                ItrKind = itrKind;
                SourceItrFingerprint = sourceItrFingerprint;
                RecordedItrFingerprint = recordedItrFingerprint;
                ZeroAttackerHpOnConsume = zeroAttackerHpOnConsume;
                ReleaseHeavyHeldTargetOnConsume = releaseHeavyHeldTargetOnConsume;
                PairSnapshot = pairSnapshot;
                PreprocessObserved = false;
                ExpectedResolvedItrFingerprint = 0;
                ObservedResolvedItrFingerprint = 0;
                ExpectedZeroAttackerHpAfterPreprocess = false;
                ObservedZeroAttackerHpAfterPreprocess = false;
                ExpectedReleaseHeavyHeldTargetAfterPreprocess = false;
                ObservedReleaseHeavyHeldTargetAfterPreprocess = false;
                ExpectedResolvedItrKind = int.MinValue;
                ObservedResolvedItrKind = int.MinValue;
                DispositionObserved = false;
                ExpectedDisposition = BattleHitCandidateDisposition.None;
                ObservedDisposition = BattleHitCandidateDisposition.None;
                FirstBodyResponseAttemptObserved = false;
                ExpectedFirstBodyResponseEligible = false;
                ObservedFirstBodyResponseEligible = false;
                ExpectedFirstBodyResponseKind =
                    BattleFirstBodyResponseKind.None;
                ObservedFirstBodyResponseKind =
                    BattleFirstBodyResponseKind.None;
                ConsumeEffectsObserved = false;
                ExpectedConsumeEffectsFingerprint = 0;
                ObservedConsumeEffectsFingerprint = 0;
                ExpectedRngStateAfterConsume = 0;
                ObservedRngStateAfterConsume = 0;
                ExpectedRngCallCountAfterConsume = 0;
                ObservedRngCallCountAfterConsume = 0;
            }

            internal BattleHitExecutionPass Pass { get; }
            internal RuntimeEntityHandle AttackerHandle { get; }
            private int AttackerStableId { get; }
            private int AttackerPrevFrame2 { get; }
            internal int CandidateOrdinal { get; }
            internal int TargetSlot { get; }
            internal RuntimeEntityHandle TargetHandleSnapshot { get; }
            internal int ItrIndex { get; }
            internal int BodyX { get; }
            private int ItrKind { get; }
            internal ulong SourceItrFingerprint { get; }
            internal ulong RecordedItrFingerprint { get; }
            internal bool ZeroAttackerHpOnConsume { get; }
            internal bool ReleaseHeavyHeldTargetOnConsume { get; }
            internal BattleHitCandidatePairSnapshot PairSnapshot { get; }
            internal bool PreprocessObserved { get; private set; }
            private ulong ExpectedResolvedItrFingerprint { get; set; }
            internal ulong ObservedResolvedItrFingerprint { get; private set; }
            internal bool ExpectedZeroAttackerHpAfterPreprocess { get; private set; }
            private bool ObservedZeroAttackerHpAfterPreprocess { get; set; }
            internal bool ExpectedReleaseHeavyHeldTargetAfterPreprocess { get; private set; }
            private bool ObservedReleaseHeavyHeldTargetAfterPreprocess { get; set; }
            internal int ExpectedResolvedItrKind { get; private set; }
            private int ObservedResolvedItrKind { get; set; }
            internal bool DispositionObserved { get; private set; }
            internal BattleHitCandidateDisposition ExpectedDisposition { get; private set; }
            private BattleHitCandidateDisposition ObservedDisposition { get; set; }
            private bool FirstBodyResponseAttemptObserved { get; set; }
            private bool ExpectedFirstBodyResponseEligible { get; set; }
            private bool ObservedFirstBodyResponseEligible { get; set; }
            private BattleFirstBodyResponseKind ExpectedFirstBodyResponseKind
            {
                get;
                set;
            }
            private BattleFirstBodyResponseKind ObservedFirstBodyResponseKind
            {
                get;
                set;
            }
            private bool ConsumeEffectsObserved { get; set; }
            private ulong ExpectedConsumeEffectsFingerprint { get; set; }
            private ulong ObservedConsumeEffectsFingerprint { get; set; }
            private uint ExpectedRngStateAfterConsume { get; set; }
            private uint ObservedRngStateAfterConsume { get; set; }
            private ulong ExpectedRngCallCountAfterConsume { get; set; }
            private ulong ObservedRngCallCountAfterConsume { get; set; }

            internal void RecordExpectedPreprocess(
                ulong resolvedItrFingerprint,
                int resolvedItrKind,
                bool zeroAttackerHp,
                bool releaseHeavyHeldTarget)
            {
                ExpectedResolvedItrFingerprint = resolvedItrFingerprint;
                ExpectedResolvedItrKind = resolvedItrKind;
                ExpectedZeroAttackerHpAfterPreprocess = zeroAttackerHp;
                ExpectedReleaseHeavyHeldTargetAfterPreprocess =
                    releaseHeavyHeldTarget;
            }

            internal void RecordObservedPreprocess(
                ulong resolvedItrFingerprint,
                int resolvedItrKind,
                bool zeroAttackerHp,
                bool releaseHeavyHeldTarget)
            {
                PreprocessObserved = true;
                ObservedResolvedItrFingerprint = resolvedItrFingerprint;
                ObservedResolvedItrKind = resolvedItrKind;
                ObservedZeroAttackerHpAfterPreprocess = zeroAttackerHp;
                ObservedReleaseHeavyHeldTargetAfterPreprocess =
                    releaseHeavyHeldTarget;
            }

            internal void RecordDisposition(
                BattleHitCandidateDisposition expected,
                BattleHitCandidateDisposition observed)
            {
                DispositionObserved = true;
                ExpectedDisposition = expected;
                ObservedDisposition = observed;
            }

            internal void RecordExpectedConsumeEffects(
                ulong fingerprint,
                uint rngState,
                ulong rngCallCount)
            {
                ExpectedConsumeEffectsFingerprint = fingerprint;
                ExpectedRngStateAfterConsume = rngState;
                ExpectedRngCallCountAfterConsume = rngCallCount;
            }

            internal void RecordExpectedFirstBodyResponseAttempt(
                bool eligible,
                BattleFirstBodyResponseKind kind)
            {
                ExpectedFirstBodyResponseEligible = eligible;
                ExpectedFirstBodyResponseKind = kind;
            }

            internal void RecordObservedFirstBodyResponseAttempt(
                bool eligible,
                BattleFirstBodyResponseKind kind)
            {
                FirstBodyResponseAttemptObserved = true;
                ObservedFirstBodyResponseEligible = eligible;
                ObservedFirstBodyResponseKind = kind;
            }

            internal void RecordObservedConsumeEffects(
                ulong fingerprint,
                uint rngState,
                ulong rngCallCount)
            {
                ConsumeEffectsObserved = true;
                ObservedConsumeEffectsFingerprint = fingerprint;
                ObservedRngStateAfterConsume = rngState;
                ObservedRngCallCountAfterConsume = rngCallCount;
            }

            internal BattleHitExecutionPlanEntryView ToView()
            {
                return new BattleHitExecutionPlanEntryView(
                    Pass,
                    AttackerHandle,
                    AttackerStableId,
                    AttackerPrevFrame2,
                    CandidateOrdinal,
                    TargetSlot,
                    TargetHandleSnapshot,
                    ItrIndex,
                    ItrKind,
                    SourceItrFingerprint,
                    RecordedItrFingerprint,
                    ZeroAttackerHpOnConsume,
                    ReleaseHeavyHeldTargetOnConsume,
                    PreprocessObserved,
                    ExpectedResolvedItrFingerprint,
                    ObservedResolvedItrFingerprint,
                    ExpectedZeroAttackerHpAfterPreprocess,
                    ObservedZeroAttackerHpAfterPreprocess,
                    ExpectedReleaseHeavyHeldTargetAfterPreprocess,
                    ObservedReleaseHeavyHeldTargetAfterPreprocess,
                    ExpectedResolvedItrKind,
                    ObservedResolvedItrKind,
                    DispositionObserved,
                    ExpectedDisposition,
                    ObservedDisposition,
                    FirstBodyResponseAttemptObserved,
                    ExpectedFirstBodyResponseEligible,
                    ObservedFirstBodyResponseEligible,
                    ExpectedFirstBodyResponseKind,
                    ObservedFirstBodyResponseKind,
                    ConsumeEffectsObserved,
                    ExpectedConsumeEffectsFingerprint,
                    ObservedConsumeEffectsFingerprint,
                    ExpectedRngStateAfterConsume,
                    ObservedRngStateAfterConsume,
                    ExpectedRngCallCountAfterConsume,
                    ObservedRngCallCountAfterConsume);
            }
        }

        private struct ItrProjection
        {
            internal ItrProjection(InteractionArea source)
            {
                Kind = source.kind;
                X = source.x;
                Y = source.y;
                W = source.w;
                H = source.h;
                Zwidth = source.zwidth;
                Z = source.z;
                HasGeometry = source.hasGeometry;
                Dvx = source.dvx;
                Dvy = source.dvy;
                Dvz = source.dvz;
                Injury = source.injury;
                Drain = source.drain;
                Sound = source.sound;
                Cover = source.cover;
                Fall = source.fall;
                Vaction = source.vaction;
                Arest = source.arest;
                Vrest = source.vrest;
                Effect = source.effect;
                Spark = source.spark;
                Recover = source.recover;
                Dbdefend = source.dbdefend;
                Kill = source.kill;
                Bdefend = source.bdefend;
                Attacking = source.attacking;
                ThrowVz = source.throwvz;
                Respond = source.respond;
                PickingAct = source.pickingact;
                PickedAct = source.pickedact;
                Delay = source.delay;
                Poison = source.poison;
                Confus = source.confus;
                Weak = source.weak;
                Manacle = source.manacle;
                Join = source.join;
                Mimic = source.mimic;
                Bound = source.bound;
                Facing = source.facing;
                Dx = source.dx;
                Dy = source.dy;
                Dz = source.dz;
                Gain = source.gain;
                ThrowVx = source.throwvx;
                ThrowVy = source.throwvy;
                ThrowInjury = source.throwinjury;
                CatchingAct = source.catchingact;
                CaughtAct = source.caughtact;
                CatchingAct2 = source.catchingact2;
                CaughtAct2 = source.caughtact2;
            }

            internal int Kind;
            internal int X;
            internal int Y;
            internal int W;
            internal int H;
            internal int Zwidth;
            internal int Z;
            internal bool HasGeometry;
            internal int Dvx;
            internal int Dvy;
            internal int Dvz;
            internal int Injury;
            internal int Drain;
            internal int Sound;
            internal int Cover;
            internal int Fall;
            internal int Vaction;
            internal int Arest;
            internal int Vrest;
            internal int Effect;
            internal int Spark;
            internal int Recover;
            internal int Dbdefend;
            internal int Kill;
            internal int Bdefend;
            internal int Attacking;
            internal int ThrowVz;
            internal int Respond;
            internal int PickingAct;
            internal int PickedAct;
            internal int Delay;
            internal int Poison;
            internal int Confus;
            internal int Weak;
            internal int Manacle;
            internal int Join;
            internal int Mimic;
            internal int Bound;
            internal int Facing;
            internal int Dx;
            internal int Dy;
            internal int Dz;
            internal int Gain;
            internal int ThrowVx;
            internal int ThrowVy;
            internal int ThrowInjury;
            internal int[] CatchingAct;
            internal int[] CaughtAct;
            internal int[] CatchingAct2;
            internal int[] CaughtAct2;

            internal void ApplyKind5Replacement(InteractionArea replacement)
            {
                Kind = 0;
                Dvx = replacement.dvx;
                Dvy = replacement.dvy;
                Fall = replacement.fall;
                Bdefend = replacement.bdefend;
                Injury = replacement.injury;
                Arest = replacement.arest;
                Vrest = replacement.vrest;
                Effect = replacement.effect;
                Spark = replacement.spark;
                Dbdefend = replacement.dbdefend;
                Attacking = replacement.attacking;
                CatchingAct = replacement.catchingact;
                CatchingAct2 = replacement.catchingact2;
                CaughtAct = replacement.caughtact;
                CaughtAct2 = replacement.caughtact2;
                Respond = replacement.respond;
                PickingAct = replacement.pickingact;
                PickedAct = replacement.pickedact;
                Delay = replacement.delay;
                Poison = replacement.poison;
                Confus = replacement.confus;
                Weak = replacement.weak;
                Manacle = replacement.manacle;
                Join = replacement.join;
                Mimic = replacement.mimic;
                Bound = replacement.bound;
                Facing = replacement.facing;
                Dx = replacement.dx;
                Dy = replacement.dy;
                Dz = replacement.dz;
                Gain = replacement.gain;
                ThrowVx = replacement.throwvx;
                ThrowVy = replacement.throwvy;
                Zwidth = replacement.zwidth;
                ThrowVz = replacement.throwvz;
                ThrowInjury = replacement.throwinjury;
            }
        }
    }
}
