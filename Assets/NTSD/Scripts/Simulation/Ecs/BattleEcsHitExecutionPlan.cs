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
        Kind15Or16 = 7,
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
            long observedConsumeEffectsCount,
            long observedWriterEffectCount,
            long observedLifecycleEffectCount,
            long observedDispatchCount,
            long observedAbortTerminationCount,
            long skippedCandidateCountAfterAbort,
            long observationMismatchCount,
            long failureCount,
            ulong lastConsumeEffectsDifferenceMask,
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
            ObservedConsumeEffectsCount = observedConsumeEffectsCount;
            ObservedWriterEffectCount = observedWriterEffectCount;
            ObservedLifecycleEffectCount = observedLifecycleEffectCount;
            ObservedDispatchCount = observedDispatchCount;
            ObservedAbortTerminationCount = observedAbortTerminationCount;
            SkippedCandidateCountAfterAbort = skippedCandidateCountAfterAbort;
            ObservationMismatchCount = observationMismatchCount;
            FailureCount = failureCount;
            LastConsumeEffectsDifferenceMask = lastConsumeEffectsDifferenceMask;
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
        public long ObservedConsumeEffectsCount { get; }
        public long ObservedWriterEffectCount { get; }
        public long ObservedLifecycleEffectCount { get; }
        public long ObservedDispatchCount { get; }
        public long ObservedAbortTerminationCount { get; }
        public long SkippedCandidateCountAfterAbort { get; }
        public long ObservationMismatchCount { get; }
        public long FailureCount { get; }
        public ulong LastConsumeEffectsDifferenceMask { get; }
        public ulong LastWriterEffectDifferenceMask { get; }
        public ulong LastLifecycleEffectDifferenceMask { get; }
        public BattleHitExecutionPlanFailureReason FirstFailureReason { get; }
        public int FirstFailureAttackerSlot { get; }
        public int FirstFailureCandidateOrdinal { get; }
        public bool CurrentTickPlanValid { get; }
    }

    /// <summary>
    /// U5 read-only boundary for the authority hit loops. It freezes the exact
    /// attacker/candidate order consumed by the still-canonical object writer. The
    /// plan never resolves damage and never writes runtime state.
    /// </summary>
    internal sealed class BattleEcsHitExecutionPlan
    {
        private const int HitCandidateMaximum = 20;

        private readonly SimulationWorld world;
        private readonly Entry[] entries;
        private BattleHitExecutionPlanMode mode;
        private int capturedTick = -1;
        private int entryCount;
        private bool characterPassCaptured;
        private bool objectPassCaptured;
        private int characterEntryStart;
        private int characterEntryCount;
        private int objectEntryStart;
        private int objectEntryCount;
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
        private long observedConsumeEffectsCount;
        private long observedWriterEffectCount;
        private long observedLifecycleEffectCount;
        private long observedDispatchCount;
        private long observedAbortTerminationCount;
        private long skippedCandidateCountAfterAbort;
        private long observationMismatchCount;
        private long failureCount;
        private ulong lastConsumeEffectsDifferenceMask;
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
        }

        internal BattleHitExecutionPlanMode Mode => mode;
        internal int EntryCount => entryCount;
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
                observedConsumeEffectsCount,
                observedWriterEffectCount,
                observedLifecycleEffectCount,
                observedDispatchCount,
                observedAbortTerminationCount,
                skippedCandidateCountAfterAbort,
                observationMismatchCount,
                failureCount,
                lastConsumeEffectsDifferenceMask,
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
            observationPassActive = false;
            observationPass = default;
            observationEntryStart = 0;
            observationExpectedCount = 0;
            observationReadCount = 0;
            ResetPendingPreprocessExpectation();
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
            observedConsumeEffectsCount = 0;
            observedWriterEffectCount = 0;
            observedLifecycleEffectCount = 0;
            observedDispatchCount = 0;
            observedAbortTerminationCount = 0;
            skippedCandidateCountAfterAbort = 0;
            observationMismatchCount = 0;
            failureCount = 0;
            lastConsumeEffectsDifferenceMask = 0;
            lastWriterEffectDifferenceMask = 0;
            lastLifecycleEffectDifferenceMask = 0;
            firstFailureReason = BattleHitExecutionPlanFailureReason.None;
            firstFailureAttackerSlot = -1;
            firstFailureCandidateOrdinal = -1;
        }

        internal void CapturePass(int tickIndex, BattleHitExecutionPass pass)
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
                        recordedItr?.kind ?? sourceItr.kind,
                        Fingerprint(sourceItr),
                        Fingerprint(recordedItr),
                        hit.ZeroAttackerHpOnConsume,
                        hit.ReleaseHeavyHeldTargetOnConsume);
                    plannedCandidateCount++;
                }
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
            ResetConsumeEffectsObservation();
            ResetPendingWriterEffectExpectation();
            ResetPendingLifecycleEffectExpectation();
            ResetPendingDispatchExpectation();
            activeDispositionEntryIndex = -1;
            observationPassCount++;
            return true;
        }

        internal void ObserveLegacyCandidateRead(
            RuntimeEntityHandle attackerHandle,
            int candidateOrdinal,
            in SceneQueryHit hit)
        {
            if (!ShouldObserveLegacyCandidateRead)
                return;

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
                    hit.ReleaseHeavyHeldTargetOnConsume)
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
            WriterEffectSnapshot actual = CaptureWriterEffectSnapshot(
                attacker,
                target,
                pendingExpectedWriterEffectSnapshot.HeldTargetHandle.Slot);
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
            observationPassActive = false;
            observationPass = default;
            observationEntryStart = 0;
            observationExpectedCount = 0;
            observationReadCount = 0;
            ResetPendingPreprocessExpectation();
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
                   currentFrame.bodies[0].x > 1000 &&
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
                case 16:
                    return BattleHitCandidateDisposition.Kind15Or16;
                case 10:
                case 11:
                    return BattleHitCandidateDisposition.Kind10Or11;
                case 1:
                    return BattleHitCandidateDisposition.Kind1Grab;
                case 3:
                    return BattleHitCandidateDisposition.Kind3Grab;
                case 2:
                case 7:
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
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            int targetSlot = target?.Runtime?.SlotIndex ?? -1;
            int holderSlot = attacker?.HolderCopySlot ?? -1;
            LF2Entity holder = holderSlot >= 0
                ? world.FindEntityByRuntimeSlotForQuery(holderSlot)
                : null;
            int damageStatIndex = target?.Unk344 ?? -1;
            int heldTargetSlot = heldTargetSlotOverride >= 0
                ? heldTargetSlotOverride
                : target?.Runtime?.ResolveActiveHeldSlotIndex() ?? -1;
            LF2Entity heldTarget = heldTargetSlot >= 0
                ? world.FindEntityByRuntimeSlotForQuery(heldTargetSlot)
                : null;
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
                TargetHolderSlot = target?.Runtime?.HolderStableId ?? int.MinValue,
                TargetHolderCopySlot = target?.Runtime?.HolderCopySlotIndex ?? int.MinValue,
                TargetRelationTeam = target?.Runtime?.RelationTeam ?? int.MinValue,
                TargetWeaponFlightCounter = target?.Runtime?.WeaponFlightCounter ?? int.MinValue,
                TargetWeaponCount = target?.WeaponCount ?? int.MinValue,
                TargetFall = target?.Runtime?.Fall ?? int.MinValue,
                TargetHitConfirmCounter = target?.HitConfirmCounter ?? int.MinValue,
                TargetHitConfirm2 = target?.Runtime?.HitConfirm2 ?? int.MinValue,
                TargetHealTimer = target?.HealTimer ?? int.MinValue,
                TargetXBoundPositive = target?.Runtime?.XBoundPositive ?? false,
                TargetXBoundNegative = target?.Runtime?.XBoundNegative ?? false,
                TargetZBoundPositive = target?.Runtime?.ZBoundPositive ?? false,
                TargetZBoundNegative = target?.Runtime?.ZBoundNegative ?? false,
                HolderHandle = ResolveCurrentHandle(holderSlot, holder),
                HolderComboCountAtk = holder?.ComboCountAtk ?? int.MinValue,
                HolderKillStat = holder?.KillStat ?? int.MinValue,
                TargetHp = target?.Health?.HP ?? int.MinValue,
                TargetHpBound = target?.Health?.HPBound ?? int.MinValue,
                TargetPp = target?.Health?.PP ?? int.MinValue,
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
                        ref projection);

                case BattleHitCandidateDisposition.Kind3Grab:
                    return ProjectGrabWriterEffect(
                        attacker,
                        target,
                        resolvedItr,
                        resetAttackerPosition: true,
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
                    if (attacker.Frame == null)
                        return false;
                    projection.TargetHealTimer = resolvedItr.injury + 1000;
                    projection.AttackerFrame = resolvedItr.dvx;
                    projection.AttackerRuntimeFrame = resolvedItr.dvx;
                    projection.AttackerX = target.Runtime.X;
                    projection.AttackerZ = target.Runtime.Z + 1.0;
                    projection.AttackerXInt = target.Runtime.XInt;
                    projection.AttackerZInt = target.Runtime.ZInt + 1;
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
                    return ProjectKind10Or11WriterEffect(
                        attacker,
                        target,
                        resolvedItr.kind,
                        ref projection);

                case BattleHitCandidateDisposition.Kind15Or16:
                    return ProjectKind15Or16WriterEffect(
                        attacker,
                        target,
                        resolvedItr,
                        resolvedItr.kind,
                        ref projection);

                case BattleHitCandidateDisposition.Damage:
                    if (target.GetCurrentDataObjectTypeForSimulation() !=
                        (int)LF2ObjectType.Character)
                    {
                        if (target.GetCurrentDataObjectTypeForSimulation() ==
                            (int)LF2ObjectType.SpecialAttack)
                        {
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

                            return ProjectStandardType3DamageWriterEffect(
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

                    return LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                            attacker,
                            target,
                            resolvedItr)
                        ? ProjectAlternateCharacterDamageWriterEffect(
                            attacker,
                            target,
                            resolvedItr,
                            ref projection)
                        : ProjectStandardCharacterDamageWriterEffect(
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
                    return CanProjectType3D1IdentityDamageWriterEffect(
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

                return CanProjectStandardObjectDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr);
            }

            return LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker,
                    target,
                    resolvedItr)
                ? CanProjectAlternateCharacterDamageWriterEffect(
                    attacker,
                    target,
                    resolvedItr)
                : CanProjectStandardCharacterDamageWriterEffect(
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
                target is not LF2WeaponBase ||
                attacker.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
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
            if ((targetOid == 100 && !oid100Held) ||
                attacker.GetState() != LF2States.Standing ||
                attacker.Runtime.LinkState < 0 ||
                (!oid100Held && target.Runtime.LinkState != 0) ||
                target.Runtime.YInt < 0 ||
                (resolvedItr.effect != 0 && resolvedItr.effect != 4) ||
                resolvedItr.bdefend == 100)
            {
                return false;
            }

            if (targetType == (int)LF2ObjectType.HeavyWeapon)
            {
                if (resolvedItr.fall <= 40 && resolvedItr.effect != 4)
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
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound) ||
                !string.IsNullOrWhiteSpace(targetData.weapon_hit_sound) ||
                attacker.ItrRest == null || target.ItrRest == null)
            {
                return false;
            }

            int requiredSounds = targetType == (int)LF2ObjectType.Drink ? 0 : 1;
            if (oid100Held && resolvedItr.dvx != 0)
                requiredSounds++;
            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(requiredSounds) == true;
        }

        private static bool CanProjectStandardType3DamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 || target is not LF2SpecialAttack ||
                attacker.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            int previousState = target.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.Frame?.Prev2D?.state ??
                target.GetFrameDataById(target.Runtime.PrevFrame2)?.state ?? 0;
            if (attackerOid == 8 || attackerOid == 0xD1 || attackerOid == 0xD5 ||
                attacker.GetState() != LF2States.Standing ||
                attacker.Runtime.LinkState < 0 ||
                target.GetState() != LF2States.Standing ||
                target.Runtime.LinkState != 0 ||
                target.Runtime.CatcherSlotIndex >= 0 ||
                target.Runtime.YInt < 0 ||
                target.Health.HP <= 0 ||
                previousState == 13 || previous2State == 12 ||
                resolvedItr.bdefend == 100 ||
                !IsSupportedType3Effect(resolvedItr.effect))
            {
                return false;
            }

            int type3Frame = resolvedItr.effect == 2 || resolvedItr.effect == 20
                ? 20
                : 30;
            if (target.GetFrameDataById(type3Frame) == null)
                return false;
            if (resolvedItr.effect >= 6000 && resolvedItr.effect < 7000 &&
                target.GetFrameDataById(resolvedItr.effect - 6000) == null)
            {
                return false;
            }

            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (attackerData == null || targetData == null ||
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound) ||
                !string.IsNullOrWhiteSpace(targetData.weapon_hit_sound) ||
                attacker.ItrRest == null || target.ItrRest == null)
            {
                return false;
            }

            int requiredSounds = resolvedItr.effect == 23 ? 2 : 1;
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
                resolvedItr.kind != 0 || attacker is not LF2SpecialAttack ||
                target is not LF2SpecialAttack ||
                attacker.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.SpecialAttack)
            {
                return false;
            }

            int attackerState = attacker.GetState();
            int targetState = target.GetState();
            int previousState = target.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.Frame?.Prev2D?.state ??
                target.GetFrameDataById(target.Runtime.PrevFrame2)?.state ?? 0;
            if (targetState != LF2States.ObjectFlying ||
                attackerState != LF2States.ObjectFlying ||
                attacker.Runtime.LinkState < 0 ||
                target.Runtime.LinkState != 0 ||
                target.Runtime.CatcherSlotIndex >= 0 ||
                target.Runtime.YInt < 0 ||
                target.Health.HP <= 0 ||
                previousState == 13 || previous2State == 12 ||
                resolvedItr.effect != 0 || resolvedItr.bdefend == 100 ||
                attacker.GetFrameDataById(20) == null ||
                target.GetFrameDataById(20) == null)
            {
                return false;
            }

            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (attackerData == null || targetData == null ||
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound) ||
                !string.IsNullOrWhiteSpace(targetData.weapon_hit_sound) ||
                attacker.ItrRest == null || target.ItrRest == null)
            {
                return false;
            }

            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(1) == true;
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
            int previous2State = target.Frame?.Prev2D?.state ??
                target.GetFrameDataById(target.Runtime.PrevFrame2)?.state ?? 0;
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
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound) ||
                !string.IsNullOrWhiteSpace(targetData.weapon_hit_sound) ||
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

            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(1) == true;
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
            bool oid8Character = attackerOid == 8 &&
                                 attackerType == (int)LF2ObjectType.Character &&
                                 attacker.Runtime.LinkState >= 0;
            bool oidD5HeldSpecial = attackerOid == 0xD5 &&
                                    attacker is LF2SpecialAttack &&
                                    attackerType == (int)LF2ObjectType.SpecialAttack &&
                                    attacker.Runtime.LinkState < 0;
            if (!oid8Character && !oidD5HeldSpecial)
                return false;

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            int previousState = target.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.Frame?.Prev2D?.state ??
                target.GetFrameDataById(target.Runtime.PrevFrame2)?.state ?? 0;
            if (!IsKarasuType3Oid(targetOid) ||
                attacker.GetState() != LF2States.Standing ||
                target.GetState() != LF2States.Standing ||
                target.Runtime.LinkState != 0 ||
                target.Runtime.CatcherSlotIndex >= 0 ||
                target.Runtime.YInt < 0 ||
                target.Health.HP <= 0 ||
                previousState == 13 || previous2State == 12 ||
                resolvedItr.effect != 0 || resolvedItr.bdefend == 100 ||
                target.GetFrameDataById(30) == null ||
                attackerData == null || targetData == null ||
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound) ||
                !string.IsNullOrWhiteSpace(targetData.weapon_hit_sound) ||
                attacker.ItrRest == null || target.ItrRest == null ||
                !TryResolveActiveType3IdentitySource(target.Match, 0xD1, out LF2Entity source) ||
                source.GetFrameDataById(30) == null ||
                source.GetFrameDataById(30).state == LF2States.ObjectFlying ||
                source.GetFrameDataById(30).state == LF2States.ObjectExpanding ||
                target.ResolveRuntimeCharacterConfig(0xD1)?.characterData !=
                    LF2HitResolveRuntimeData.ResolveCharacterData(source))
            {
                return false;
            }

            if (oidD5HeldSpecial && ResolveActiveType3Holder(attacker) == null)
                return false;

            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(1) == true;
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

            projection.TargetHitConfirm2 = 1;
            projection.TargetWeaponFlightCounter -= resolvedItr.injury;
            projection.TargetRelationTeam = projection.AttackerRelationTeam;

            if (targetType != (int)LF2ObjectType.HeavyWeapon ||
                resolvedItr.fall > 40)
            {
                projection.TargetHitCount++;
            }

            projection.TargetFall = 0;
            if (projection.TargetVx > -5.0 &&
                projection.TargetVx < 5.0 &&
                resolvedItr.dvx == 0)
            {
                projection.TargetKnockbackVx +=
                    attacker.Dirh() > 0 ? 5.0 : -5.0;
            }
            else if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx +=
                    attacker.Dirh() > 0 ? resolvedItr.dvx : -resolvedItr.dvx;
            }

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            bool applyOid100KnockbackTail =
                targetOid == 100 && target.Runtime.LinkState < 0 &&
                resolvedItr.dvx != 0;
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

            if (targetType != (int)LF2ObjectType.Drink)
            {
                ProjectQueuedSound(
                    targetWorld,
                    ResolveDamageEffectCue(resolvedItr.effect),
                    attacker.Runtime.XInt,
                    ref projection);
            }
            if (applyOid100KnockbackTail)
            {
                ProjectQueuedSound(
                    targetWorld,
                    "SFX_039",
                    target.Runtime.XInt,
                    ref projection);
            }

            projection.TargetHitStateCount = 45;
            if (projection.AttackerFrameDelay >= 0)
                projection.AttackerFrameDelay = 3;
            projection.TargetFrameDelay = -3;
            int itrArest = resolvedItr.arest < 4 && resolvedItr.vrest == 0
                ? 4
                : resolvedItr.arest;
            projection.AttackerAttackExempt = itrArest;
            projection.AttackerItrArest = itrArest;
            if (resolvedItr.vrest > 0)
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest;

            if (targetType == (int)LF2ObjectType.HeavyWeapon)
            {
                projection.AttackerVrestAgainstAttacker =
                    resolvedItr.fall <= 40 && resolvedItr.effect != 4 ? 3 : 19;
                projection.TargetFacing = projection.AttackerFacing;
                int targetFrame = resolvedItr.fall <= 40 && resolvedItr.effect != 4
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

            return ProjectKind0HitRecord(
                       attacker,
                       target,
                       resolvedItr,
                       ref projection) &&
                   attackerSlot >= 0 && targetSlot >= 0;
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

            projection.TargetHitCount++;
            projection.TargetFall += resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);

            if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            projection.TargetHitStateCount = 45;
            if (projection.AttackerFrameDelay >= 0)
                projection.AttackerFrameDelay = 3;
            projection.TargetFrameDelay = -3;
            int itrArest = resolvedItr.arest < 4 && resolvedItr.vrest == 0
                ? 4
                : resolvedItr.arest;
            projection.AttackerAttackExempt = itrArest;
            projection.AttackerItrArest = itrArest;
            if (resolvedItr.vrest > 0)
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest;

            projection.TargetRelationTeam = projection.AttackerRelationTeam;
            projection.TargetHolderCopySlot = attacker.HolderCopySlot;
            projection.TargetHitConfirm2 = 1;
            projection.TargetAttackingCounter = 0;
            projection.TargetKnockbackVx = 0.0;
            projection.TargetKnockbackVy = 0.0;
            projection.TargetKnockbackVz = 0.0;
            projection.TargetVx = 0.0;
            projection.TargetVy = 0.0;
            projection.TargetVz = 0.0;
            int targetFrame = resolvedItr.effect == 2 || resolvedItr.effect == 20
                ? 20
                : 30;
            projection.TargetFrame = targetFrame;
            projection.TargetRuntimeFrame = targetFrame;

            if (resolvedItr.effect >= 5000 && resolvedItr.effect < 6000)
            {
                projection.TargetPp = Math.Max(
                    0,
                    projection.TargetPp - (resolvedItr.effect - 5000));
            }
            else if (resolvedItr.effect >= 6000 && resolvedItr.effect < 7000)
            {
                targetFrame = resolvedItr.effect - 6000;
                projection.TargetFrame = targetFrame;
                projection.TargetRuntimeFrame = targetFrame;
            }
            else if (resolvedItr.effect == 23)
            {
                ProjectQueuedSound(
                    targetWorld,
                    "SFX_068",
                    target.Runtime.XInt,
                    ref projection);
            }

            return ProjectKind0HitRecord(
                       attacker,
                       target,
                       resolvedItr,
                       ref projection) &&
                   attackerSlot >= 0 && targetSlot >= 0;
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

            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;

            projection.TargetHitCount++;
            projection.TargetFall += resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);

            if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            projection.TargetHitStateCount = 45;
            if (projection.AttackerFrameDelay >= 0)
                projection.AttackerFrameDelay = 3;
            projection.TargetFrameDelay = -3;
            int itrArest = resolvedItr.arest < 4 && resolvedItr.vrest == 0
                ? 4
                : resolvedItr.arest;
            projection.AttackerAttackExempt = itrArest;
            projection.AttackerItrArest = itrArest;
            if (resolvedItr.vrest > 0)
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest;

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

            return ProjectKind0HitRecord(
                       attacker,
                       target,
                       resolvedItr,
                       ref projection) &&
                   attackerSlot >= 0 && targetSlot >= 0;
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

            projection.TargetHitCount++;
            projection.TargetFall += resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);

            if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            projection.TargetHitStateCount = 45;
            if (projection.AttackerFrameDelay >= 0)
                projection.AttackerFrameDelay = 3;
            projection.TargetFrameDelay = -3;
            int itrArest = resolvedItr.arest < 4 && resolvedItr.vrest == 0
                ? 4
                : resolvedItr.arest;
            projection.AttackerAttackExempt = itrArest;
            projection.AttackerItrArest = itrArest;
            if (resolvedItr.vrest > 0)
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest;

            projection.TargetRelationTeam = projection.AttackerRelationTeam;
            projection.TargetHolderCopySlot = attacker.HolderCopySlot;
            projection.TargetHitConfirm2 = 1;
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

            projection.TargetHitCount++;
            projection.TargetFall += resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            ProjectQueuedSound(
                targetWorld,
                ResolveDamageEffectCue(resolvedItr.effect),
                attacker.Runtime.XInt,
                ref projection);

            if (resolvedItr.dvx != 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0
                    ? resolvedItr.dvx
                    : -resolvedItr.dvx;
            }

            projection.TargetHitStateCount = 45;
            if (projection.AttackerFrameDelay >= 0)
                projection.AttackerFrameDelay = 3;
            projection.TargetFrameDelay = -3;
            int itrArest = resolvedItr.arest < 4 && resolvedItr.vrest == 0
                ? 4
                : resolvedItr.arest;
            projection.AttackerAttackExempt = itrArest;
            projection.AttackerItrArest = itrArest;
            if (resolvedItr.vrest > 0)
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest;

            projection.TargetRelationTeam = relationSource.RelationTeam;
            projection.TargetHolderCopySlot = relationSource.HolderCopySlot;
            projection.TargetHitConfirm2 = 1;
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
            projection.TargetFrame = 30;
            projection.TargetRuntimeFrame = 30;
            projection.TargetPrevFrame = 30;
            projection.TargetWaitCounter = 30;

            return ProjectKind0HitRecord(
                       attacker,
                       target,
                       resolvedItr,
                       ref projection) &&
                   attackerSlot >= 0 && targetSlot >= 0;
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

        private static bool IsSupportedType3Effect(int effect)
        {
            // Authority collision collection rejects effect 20 against non-character DAT,
            // so it cannot reach a type-3 special-attack writer plan.
            return effect == 0 || effect == 2 || effect == 3 || effect == 5 ||
                   effect == 21 || effect == 22 || effect == 23 ||
                   effect == 30 ||
                   (effect >= 5000 && effect < 7000);
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
            // Effect 4 is rejected for character DAT by the authority collision collector.
            return effect == 0 || effect == 1 || effect == 2 || effect == 3 ||
                   effect == 5 || effect == 20 || effect == 21 ||
                   effect == 22 || effect == 23 || effect == 30;
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

            int attackerType = attacker.GetCurrentDataObjectTypeForSimulation();
            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            bool supportedAttacker =
                attackerType == (int)LF2ObjectType.Character ||
                (attackerType == (int)LF2ObjectType.SpecialAttack &&
                 (attackerOid == 0xC9 || attackerOid == 0xD6));
            if (!supportedAttacker)
                return false;

            int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
            if (targetOid == 300 || targetOid == 100 ||
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, target, resolvedItr))
            {
                return false;
            }

            int targetState = target.Frame?.D?.state ?? 0;
            if ((targetState == LF2States.Frozen && resolvedItr.effect == 30) ||
                ((targetState == LF2States.Burning || targetState == LF2States.FirenSpecific) &&
                 (resolvedItr.effect == 20 || resolvedItr.effect == 21)))
            {
                return false;
            }

            if (attacker.GetState() != LF2States.Standing ||
                attacker.Runtime.LinkState < 0 ||
                target.Runtime.LinkState != 0 ||
                target.Runtime.CatcherSlotIndex >= 0 ||
                target.Runtime.YInt < 0 ||
                !IsSupportedStandardCharacterDamageEffect(resolvedItr.effect) ||
                target.Health.HP <= 0)
            {
                return false;
            }

            LF2CharacterData attackerData = LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData = LF2HitResolveRuntimeData.ResolveCharacterData(target);
            if (attackerData == null || targetData == null ||
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound) ||
                !string.IsNullOrWhiteSpace(targetData.weapon_hit_sound))
            {
                return false;
            }

            int previousState = target.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = target.Frame?.Prev2D?.state ??
                target.GetFrameDataById(target.Runtime.PrevFrame2)?.state ?? 0;
            if (previousState == LF2States.Frozen || previous2State == LF2States.Falling)
                return false;

            int fallIncrement = resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            int projectedFall = target.FallCounter + fallIncrement;
            if (projectedFall < 0)
                return false;

            int requiredSounds = resolvedItr.effect == 1 ? 4 : 2;
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
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            int injury = resolvedItr.injury;

            if (projection.TargetHp > 0 &&
                injury >= projection.TargetHp &&
                target.KillCount == -1)
            {
                if (projection.HolderHandle.IsValid)
                    projection.HolderKillStat++;
                if (projection.TargetKillStat != int.MinValue)
                    projection.TargetKillStat++;
            }

            projection.TargetHp -= injury;
            projection.TargetHpBound -= injury / 3;
            projection.TargetComboCountVic += injury;
            if (target.KillCount == -1 && projection.HolderHandle.IsValid)
                projection.HolderComboCountAtk += injury;
            if (projection.TargetDamageStat != int.MinValue)
                projection.TargetDamageStat += injury;

            projection.TargetHitCount++;
            int fallIncrement = resolvedItr.fall != 0
                ? resolvedItr.fall
                : NTSDGlobal.Default.Fall.Value;
            int projectedFall = projection.TargetHp <= 0
                ? 80 + fallIncrement
                : projection.TargetFall + fallIncrement;
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
            }
            else if (projectedFall > 20)
            {
                projection.TargetFall = 40;
                int targetFrame = projection.TargetFacing != projection.AttackerFacing
                    ? LF2StandardFrames.Injured2
                    : LF2StandardFrames.Injured4;
                projection.TargetFrame = targetFrame;
                projection.TargetRuntimeFrame = targetFrame;
            }
            else if (projectedFall > 0)
            {
                projection.TargetFall = 20;
                projection.TargetFrame = LF2StandardFrames.Injured;
                projection.TargetRuntimeFrame = LF2StandardFrames.Injured;
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

            if (knockback &&
                projection.TargetVx > -5.0 &&
                projection.TargetVx < 5.0 &&
                resolvedItr.dvx == 0)
            {
                projection.TargetKnockbackVx +=
                    attacker.Dirh() > 0 ? 5.0 : -5.0;
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
            }

            projection.TargetHitStateCount = 45;
            if (projection.AttackerFrameDelay >= 0)
                projection.AttackerFrameDelay = 3;
            projection.TargetFrameDelay = -3;
            int itrArest = resolvedItr.arest < 4 && resolvedItr.vrest == 0
                ? 4
                : resolvedItr.arest;
            projection.AttackerAttackExempt = itrArest;
            projection.AttackerItrArest = itrArest;
            if (resolvedItr.vrest > 0)
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest;

            if (projection.TargetFall == 80)
                projection.TargetFall = 0;

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

        private static bool CanProjectAlternateCharacterDamageWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            if (attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || resolvedItr == null ||
                resolvedItr.kind != 0 ||
                attacker.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character ||
                !LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker,
                    target,
                    resolvedItr))
            {
                return false;
            }

            int injury = resolvedItr.injury;
            if (target.FallDamageDiv > 0)
                injury = injury * 100 / target.FallDamageDiv;
            int reducedInjury = injury / 10;
            if (attacker.GetState() != LF2States.Standing ||
                attacker.Runtime.LinkState < 0 ||
                target.Runtime.LinkState != 0 ||
                target.Runtime.YInt != 0 ||
                target.HitStateCount + resolvedItr.bdefend > 30 ||
                (target.Frame?.N ?? 0) == LF2StandardFrames.Defend ||
                resolvedItr.effect != 0)
            {
                return false;
            }

            return target.Match?.BattleBuffersForServices
                .CanQueueSoundsWithoutRejection(1) == true;
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
            int injury = resolvedItr.injury;
            if (target.FallDamageDiv > 0)
                injury = injury * 100 / target.FallDamageDiv;
            int reducedInjury = injury / 10;

            if (projection.TargetHp > 0 &&
                reducedInjury >= projection.TargetHp &&
                target.KillCount == -1)
            {
                if (projection.HolderHandle.IsValid)
                    projection.HolderKillStat++;
                if (projection.TargetKillStat != int.MinValue)
                    projection.TargetKillStat++;
            }

            projection.TargetHp -= reducedInjury;
            projection.TargetHpBound -= reducedInjury / 3;
            projection.TargetComboCountVic += reducedInjury;
            if (target.KillCount == -1 && projection.HolderHandle.IsValid)
                projection.HolderComboCountAtk += reducedInjury;
            if (projection.TargetDamageStat != int.MinValue)
                projection.TargetDamageStat += reducedInjury;

            if (projection.TargetHp <= 0)
                projection.TargetFall = 80;

            projection.TargetAttackingCounter = 0;
            projection.TargetHitStateCount += resolvedItr.bdefend;
            projection.TargetHitCount++;
            projection.AttackerFrameDelay = 3;
            projection.TargetFrameDelay = -5;

            if (projection.TargetFall == 80 &&
                projection.TargetVx < 3.0 &&
                projection.TargetVx > -3.0 &&
                resolvedItr.dvx == 0)
            {
                projection.TargetKnockbackVx += attacker.Dirh() > 0 ? 3.0 : -3.0;
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

            projection.AttackerAttackExempt =
                resolvedItr.arest < 4 && resolvedItr.vrest == 0
                    ? 4
                    : Math.Min(resolvedItr.arest, 12);
            if (resolvedItr.vrest > 0)
            {
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest > 4
                    ? Math.Min(resolvedItr.vrest, 12)
                    : 4;
            }

            string leadCue = target.ObjectId == 37 || target.ObjectId == 6
                ? "SFX_017"
                : "SFX_002";
            ProjectQueuedSound(
                targetWorld,
                leadCue,
                target.Runtime.XInt,
                ref projection);

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
                    attacker.Frame?.N ?? 0) ?? attacker.Frame?.D;
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

        private static void ProjectQueuedSound(
            SimulationWorld targetWorld,
            string cue,
            int worldX,
            ref WriterEffectSnapshot projection)
        {
            projection.PendingSoundCount++;
            projection.PendingSoundCue = cue;
            projection.PendingSoundWorldX = worldX;
            projection.PendingSoundTick = targetWorld.CurrentTickIndex;
            projection.QueuedSoundEventCount++;
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

        private static bool ProjectKind10Or11WriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            int kind,
            ref WriterEffectSnapshot projection)
        {
            if (kind != 10 && kind != 11)
                return false;
            if (kind == 11 && target.WeaponCount >= 0)
                return true;

            const double factor = 0.9345794392523364;
            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            if (targetType == (int)LF2ObjectType.Character)
            {
                projection.TargetWeaponCount = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
                if (target.KillCount == -1 &&
                    (target.Match?.CurrentTickIndex ?? 0) % 12 == 0 &&
                    !LF2HitResolveRuntimeData.IsStepWaitGate(target) &&
                    projection.HolderHandle.IsValid)
                {
                    projection.HolderComboCountAtk += 11;
                }

                if (projection.TargetDamageStat != int.MinValue)
                    projection.TargetDamageStat += 11;

                projection.TargetFrame = 182;
                projection.TargetRuntimeFrame = 182;
                ProjectScaledAirStep(factor, 3.0, ref projection);
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

                projection.TargetWeaponCount = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
                ProjectScaledAirStep(factor, 3.0, ref projection);
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

                projection.TargetWeaponCount = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
                ProjectScaledAirStep(factor, 2.3, ref projection);
            }

            return true;
        }

        private static bool ProjectKind15Or16WriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            int kind,
            ref WriterEffectSnapshot projection)
        {
            if (kind != 15 && kind != 16)
                return false;

            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            if (targetType == (int)LF2ObjectType.Character)
            {
                if (kind == 16)
                {
                    return ProjectKind16CharacterWriterEffect(
                        attacker,
                        target,
                        resolvedItr,
                        ref projection);
                }

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

        private static bool ProjectKind16CharacterWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            ref WriterEffectSnapshot projection)
        {
            SimulationWorld targetWorld = target.Match ?? attacker.Match;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if (targetWorld == null || target.Health == null ||
                attackerSlot < 0 || targetSlot < 0)
            {
                return false;
            }

            int adjustedInjury = resolvedItr.injury;
            if (target.FallDamageDiv > 0)
                adjustedInjury = resolvedItr.injury * 100 / target.FallDamageDiv;

            if (projection.TargetHp > 0 &&
                adjustedInjury >= projection.TargetHp &&
                target.KillCount == -1)
            {
                if (projection.HolderHandle.IsValid)
                    projection.HolderKillStat++;
                if (projection.TargetKillStat != int.MinValue)
                    projection.TargetKillStat++;
            }

            projection.TargetHp -= adjustedInjury;
            projection.TargetHpBound -= adjustedInjury / 3;
            projection.TargetComboCountVic += adjustedInjury;
            if (target.KillCount == -1 && projection.HolderHandle.IsValid)
                projection.HolderComboCountAtk += adjustedInjury;
            if (projection.TargetDamageStat != int.MinValue)
                projection.TargetDamageStat += adjustedInjury;

            projection.TargetFrame = LF2StandardFrames.MpDrain;
            projection.TargetRuntimeFrame = LF2StandardFrames.MpDrain;
            projection.TargetAttackingCounter = 0;
            if (resolvedItr.vrest > 0)
                projection.TargetVrestAgainstAttacker = resolvedItr.vrest;

            int heldTargetSlot = target.Runtime.ResolveActiveHeldSlotIndex();
            LF2Entity heldTarget = heldTargetSlot >= 0
                ? targetWorld.FindEntityByRuntimeSlotForQuery(heldTargetSlot)
                : null;
            if (target.Runtime.LinkState == 2 &&
                heldTarget?.Runtime != null &&
                heldTarget.Runtime.LinkState == -2 &&
                heldTarget.Runtime.IsActivelyHeldBySlot(targetSlot))
            {
                projection.AttackerVrestAgainstHeld = 45;
                projection.TargetVrestAgainstHeld = 30;
                projection.TargetLinkState = 0;
                projection.HeldTargetLinkState = 0;
                unchecked
                {
                    projection.RngState = projection.RngState * 0x343FDu + 0x269EC3u;
                }
                projection.RngCallCount++;
                int heldFrame = (int)((projection.RngState >> 16) & 0x7FFFu) % 6;
                projection.HeldTargetFrame = heldFrame;
                projection.HeldTargetRuntimeFrame = heldFrame;
                projection.HeldTargetVy = -1.0;
            }

            if (targetWorld.BattleBuffersForServices.CanQueueSoundWithoutRejection)
            {
                projection.PendingSoundCount++;
                projection.PendingSoundCue = "SFX_065";
                projection.PendingSoundWorldX = target.Runtime.XInt;
                projection.PendingSoundTick = targetWorld.CurrentTickIndex;
                projection.QueuedSoundEventCount++;
            }
            else
            {
                projection.RejectedSoundEventCount++;
            }

            return true;
        }

        private static void ProjectScaledAirStep(
            double factor,
            double vyStep,
            ref WriterEffectSnapshot projection)
        {
            projection.TargetKnockbackVx = projection.TargetVx * factor;
            projection.TargetVx = projection.TargetKnockbackVx;
            projection.TargetKnockbackVz = projection.TargetVz * factor;
            projection.TargetVz = projection.TargetKnockbackVz;
            ProjectAirStep(vyStep, ref projection);
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
            LF2FrameData attackerFrame = attacker.GetFrameDataById(catchingFrame);
            LF2FrameData targetFrame = target.GetFrameDataById(caughtFrame);

            int attackerWAct = attackerFrame?.cpoint?.x ?? 0;
            int targetWAct = targetFrame?.cpoint?.x ?? 0;
            int attackerCx = attackerFrame?.centerx ?? 0;
            int attackerCy = attackerFrame?.centery ?? 0;
            int targetCx = targetFrame?.centerx ?? 0;
            int targetCy = targetFrame?.centery ?? 0;
            int attackerFacing = projection.AttackerXInt > projection.TargetXInt ? 1 : 0;

            projection.AttackerVx = 0.0;
            projection.TargetVx = 0.0;
            projection.AttackerFacing = attackerFacing;
            projection.TargetFacing = 1 - attackerFacing;
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
            projection.AttackerCaughtDuration = 300;
            projection.TargetFall = 0;
            return true;
        }

        private static bool ProjectPickupWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            int kind,
            ref WriterEffectSnapshot projection)
        {
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if (attackerSlot < 0 || targetSlot < 0)
                return false;

            if (kind == 7)
            {
                if (projection.AttackerLinkState != 0)
                    return true;

                projection.AttackerLinkState = 1;
                projection.TargetLinkState = -1;
                int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
                int targetType = target.GetCurrentDataObjectTypeForSimulation();
                if (targetOid == 0x78 || targetOid == 0x7C)
                {
                    projection.AttackerLinkState = 101;
                }
                else if (targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    projection.AttackerLinkState = 4;
                    projection.TargetLinkState = -4;
                }
                else if (targetType == (int)LF2ObjectType.Drink)
                {
                    projection.AttackerLinkState = target.Health != null && target.Health.HP > 0 ? 6 : 4;
                    projection.TargetLinkState = -projection.AttackerLinkState;
                    if (target.Health == null || target.Health.HP <= 0)
                        projection.TargetWeaponFlightCounter = 0;
                }

                ProjectPickupLinkFields(attackerSlot, targetSlot, ref projection);
                return true;
            }

            if (kind != 2)
                return false;

            int dataType = target.GetCurrentDataObjectTypeForSimulation();
            if (dataType == (int)LF2ObjectType.LightWeapon)
            {
                projection.AttackerFrame = LF2StandardFrames.PickingLight;
                projection.AttackerRuntimeFrame = LF2StandardFrames.PickingLight;
                projection.AttackerLinkState = 1;
                projection.TargetLinkState = -1;
            }
            else if (dataType == (int)LF2ObjectType.ThrowWeapon)
            {
                projection.AttackerFrame = LF2StandardFrames.PickingLight;
                projection.AttackerRuntimeFrame = LF2StandardFrames.PickingLight;
                projection.AttackerLinkState = 4;
                projection.TargetLinkState = -4;
            }
            else if (dataType == (int)LF2ObjectType.Drink)
            {
                projection.AttackerFrame = LF2StandardFrames.PickingLight;
                projection.AttackerRuntimeFrame = LF2StandardFrames.PickingLight;
                projection.AttackerLinkState = target.Health != null && target.Health.HP > 0 ? 6 : 4;
                projection.TargetLinkState = -projection.AttackerLinkState;
                if (target.Health == null || target.Health.HP <= 0)
                    projection.TargetWeaponFlightCounter = 0;
            }
            else if (dataType == (int)LF2ObjectType.HeavyWeapon)
            {
                projection.AttackerFrame = LF2StandardFrames.PickingHeavy;
                projection.AttackerRuntimeFrame = LF2StandardFrames.PickingHeavy;
                projection.AttackerLinkState = 2;
                projection.TargetLinkState = -2;
            }
            else
            {
                return true;
            }

            ProjectPickupLinkFields(attackerSlot, targetSlot, ref projection);
            projection.AttackerAttackingCounter = 0;
            return true;
        }

        private static void ProjectPickupLinkFields(
            int attackerSlot,
            int targetSlot,
            ref WriterEffectSnapshot projection)
        {
            projection.TargetRelationTeam = projection.AttackerRelationTeam;
            projection.AttackerTargetSlot = targetSlot;
            projection.TargetHolderSlot = attackerSlot;
            projection.TargetHolderCopySlot = attackerSlot;
            projection.AttackerPickupCount++;
            projection.AttackerHeldWeaponSlot = targetSlot;
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

            if (projection.Kind == 4 && attacker.WeaponCount > 0)
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
                        holderFrame.wpoints != null && holderFrame.wpoints.Count > 0
                            ? holderFrame.wpoints[0].attacking
                            : 0;
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
            Add(ref hash, itr.dvx);
            Add(ref hash, itr.dvy);
            Add(ref hash, itr.dvz);
            Add(ref hash, itr.injury);
            Add(ref hash, itr.fall);
            Add(ref hash, itr.vaction);
            Add(ref hash, itr.arest);
            Add(ref hash, itr.vrest);
            Add(ref hash, itr.effect);
            Add(ref hash, itr.kill);
            Add(ref hash, itr.bdefend);
            Add(ref hash, itr.attacking);
            Add(ref hash, itr.throwvz);
            Add(ref hash, itr.respond);
            Add(ref hash, itr.pickingact);
            Add(ref hash, itr.pickedact);
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
            Add(ref hash, itr.Dvx);
            Add(ref hash, itr.Dvy);
            Add(ref hash, itr.Dvz);
            Add(ref hash, itr.Injury);
            Add(ref hash, itr.Fall);
            Add(ref hash, itr.Vaction);
            Add(ref hash, itr.Arest);
            Add(ref hash, itr.Vrest);
            Add(ref hash, itr.Effect);
            Add(ref hash, itr.Kill);
            Add(ref hash, itr.Bdefend);
            Add(ref hash, itr.Attacking);
            Add(ref hash, itr.ThrowVz);
            Add(ref hash, itr.Respond);
            Add(ref hash, itr.PickingAct);
            Add(ref hash, itr.PickedAct);
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
                expected.AttackerHp != actual.AttackerHp) mask |= 1UL << 19;
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
            if (expected.TargetCatcherSlot != actual.TargetCatcherSlot) mask |= 1UL << 31;
            if (expected.TargetHolderSlot != actual.TargetHolderSlot) mask |= 1UL << 32;
            if (expected.TargetHolderCopySlot != actual.TargetHolderCopySlot) mask |= 1UL << 33;
            if (expected.TargetRelationTeam != actual.TargetRelationTeam) mask |= 1UL << 34;
            if (expected.TargetWeaponFlightCounter != actual.TargetWeaponFlightCounter) mask |= 1UL << 35;
            if (expected.TargetFall != actual.TargetFall ||
                expected.TargetFrameDelay != actual.TargetFrameDelay) mask |= 1UL << 36;
            if (expected.TargetHitConfirmCounter != actual.TargetHitConfirmCounter ||
                expected.TargetHitConfirm2 != actual.TargetHitConfirm2 ||
                expected.TargetHitCount != actual.TargetHitCount ||
                expected.TargetHitStateCount != actual.TargetHitStateCount) mask |= 1UL << 37;
            if (expected.TargetHealTimer != actual.TargetHealTimer) mask |= 1UL << 38;
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
            if (expected.HolderHandle != actual.HolderHandle) mask |= 1UL << 49;
            if (expected.HolderComboCountAtk != actual.HolderComboCountAtk) mask |= 1UL << 50;
            if (expected.TargetDamageStat != actual.TargetDamageStat) mask |= 1UL << 51;
            if (expected.TargetHp != actual.TargetHp) mask |= 1UL << 52;
            if (expected.TargetHpBound != actual.TargetHpBound) mask |= 1UL << 53;
            if (expected.TargetPp != actual.TargetPp) mask |= 1UL << 53;
            if (expected.TargetComboCountVic != actual.TargetComboCountVic) mask |= 1UL << 54;
            if (expected.TargetAttackingCounter != actual.TargetAttackingCounter) mask |= 1UL << 55;
            if (expected.HolderKillStat != actual.HolderKillStat) mask |= 1UL << 56;
            if (expected.TargetKillStat != actual.TargetKillStat) mask |= 1UL << 57;
            if (expected.TargetVrestAgainstAttacker != actual.TargetVrestAgainstAttacker) mask |= 1UL << 58;
            if (expected.TargetVrestAgainstHeld != actual.TargetVrestAgainstHeld ||
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
            internal int TargetHolderSlot;
            internal int TargetHolderCopySlot;
            internal int TargetRelationTeam;
            internal int TargetWeaponFlightCounter;
            internal int TargetWeaponCount;
            internal int TargetFall;
            internal int TargetHitConfirmCounter;
            internal int TargetHitConfirm2;
            internal int TargetHealTimer;
            internal bool TargetXBoundPositive;
            internal bool TargetXBoundNegative;
            internal bool TargetZBoundPositive;
            internal bool TargetZBoundNegative;
            internal RuntimeEntityHandle HolderHandle;
            internal int HolderComboCountAtk;
            internal int HolderKillStat;
            internal int TargetHp;
            internal int TargetHpBound;
            internal int TargetPp;
            internal int TargetComboCountVic;
            internal int TargetAttackingCounter;
            internal int TargetFrameDelay;
            internal int TargetHitCount;
            internal int TargetHitStateCount;
            internal int TargetKillStat;
            internal int TargetDamageStat;
            internal int TargetVrestAgainstAttacker;
            internal int TargetVrestAgainstHeld;
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
                int itrKind,
                ulong sourceItrFingerprint,
                ulong recordedItrFingerprint,
                bool zeroAttackerHpOnConsume,
                bool releaseHeavyHeldTargetOnConsume)
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
            private RuntimeEntityHandle TargetHandleSnapshot { get; }
            internal int ItrIndex { get; }
            private int ItrKind { get; }
            private ulong SourceItrFingerprint { get; }
            internal ulong RecordedItrFingerprint { get; }
            internal bool ZeroAttackerHpOnConsume { get; }
            internal bool ReleaseHeavyHeldTargetOnConsume { get; }
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
                Dvx = source.dvx;
                Dvy = source.dvy;
                Dvz = source.dvz;
                Injury = source.injury;
                Fall = source.fall;
                Vaction = source.vaction;
                Arest = source.arest;
                Vrest = source.vrest;
                Effect = source.effect;
                Kill = source.kill;
                Bdefend = source.bdefend;
                Attacking = source.attacking;
                ThrowVz = source.throwvz;
                Respond = source.respond;
                PickingAct = source.pickingact;
                PickedAct = source.pickedact;
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
            internal int Dvx;
            internal int Dvy;
            internal int Dvz;
            internal int Injury;
            internal int Fall;
            internal int Vaction;
            internal int Arest;
            internal int Vrest;
            internal int Effect;
            internal int Kill;
            internal int Bdefend;
            internal int Attacking;
            internal int ThrowVz;
            internal int Respond;
            internal int PickingAct;
            internal int PickedAct;
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
                Attacking = replacement.attacking;
                CatchingAct = replacement.catchingact;
                CatchingAct2 = replacement.catchingact2;
                CaughtAct = replacement.caughtact;
                CaughtAct2 = replacement.caughtact2;
                Respond = replacement.respond;
                PickingAct = replacement.pickingact;
                PickedAct = replacement.pickedact;
                ThrowVx = replacement.throwvx;
                ThrowVy = replacement.throwvy;
                Zwidth = replacement.zwidth;
                ThrowVz = replacement.throwvz;
                ThrowInjury = replacement.throwinjury;
            }
        }
    }
}
