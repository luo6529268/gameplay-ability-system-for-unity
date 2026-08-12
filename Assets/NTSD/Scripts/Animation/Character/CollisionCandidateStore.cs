using System;
using NTSD.Simulation;

namespace NTSD.Animation
{
    public enum CollisionCandidateStoreMismatchReason
    {
        None = 0,
        UnexpectedShadowException = 1,
        RuntimeCapacityInvalid = 2,
        AttackerSlotOutOfRange = 3,
        AttackerHandleNotCurrent = 4,
        DuplicateAttackerSlot = 5,
        CandidateListMissing = 6,
        CandidateCountExceedsMaximum = 7,
        TargetSlotOutOfRange = 8,
        TargetHandleNotCurrent = 9,
        AttackerRowMissing = 10,
        AttackerGenerationMismatch = 11,
        CandidateCountMismatch = 12,
        TargetSlotMismatch = 13,
        TargetHandleSnapshotMismatch = 14,
        BodyXMismatch = 15,
        ItrIndexMismatch = 16,
        RuntimeItrIdentityMismatch = 17,
        ZeroAttackerHpOnConsumeMismatch = 18,
        ReleaseHeavyHeldTargetOnConsumeMismatch = 19,
    }

    /// <summary>
    /// Lifetime diagnostics for candidate-store builds and sampled legacy-oracle
    /// comparisons. Store-only authority ticks build without a Dictionary/List row.
    /// </summary>
    public sealed class CollisionCandidateStoreDiagnostics
    {
        public long BuildTickCount { get; private set; }
        public long ComparedAttackerCount { get; private set; }
        public long ComparedCandidateCount { get; private set; }
        public long MismatchCount { get; private set; }
        public long InvalidCount { get; private set; }
        public CollisionCandidateStoreMismatchReason FirstMismatchReason { get; private set; }

        internal void RecordBuildTick()
        {
            BuildTickCount++;
        }

        internal void RecordComparedAttacker()
        {
            ComparedAttackerCount++;
        }

        internal void RecordComparedCandidate()
        {
            ComparedCandidateCount++;
        }

        internal void RecordMismatch(CollisionCandidateStoreMismatchReason reason)
        {
            MismatchCount++;
            RecordFirstReason(reason);
        }

        internal void RecordInvalid(CollisionCandidateStoreMismatchReason reason)
        {
            InvalidCount++;
            MismatchCount++;
            RecordFirstReason(reason);
        }

        private void RecordFirstReason(CollisionCandidateStoreMismatchReason reason)
        {
            if (FirstMismatchReason == CollisionCandidateStoreMismatchReason.None)
                FirstMismatchReason = reason;
        }
    }

    public enum CollisionCandidateStoreAuthorityFailureReason
    {
        None = 0,
        StoreNotComplete = 1,
        AttackerHandleNotCurrent = 2,
        AttackerRowMissing = 3,
        CandidateReadFailed = 4,
        StoreOnlyProducerUnavailable = 5,
    }

    /// <summary>
    /// Lifetime diagnostics for the opt-in store authority. Counters remain zero
    /// while authority is disabled. Legacy Lists exist only on sampled oracle ticks;
    /// interval-zero authority is store-only and fails closed on producer faults.
    /// </summary>
    public sealed class CollisionCandidateStoreAuthorityDiagnostics
    {
        public long RequestedTickCount { get; private set; }
        public long AppliedTickCount { get; private set; }
        public long LegacyFallbackTickCount { get; private set; }
        public long SampledOracleTickCount { get; private set; }
        public long StoreOnlyTickCount { get; private set; }
        public long LegacyListCreatedOrWrittenCount { get; private set; }
        public long StoreOnlyHardFailureCount { get; private set; }
        public long RangeReadCount { get; private set; }
        public long EntryReadCount { get; private set; }
        public long FailureCount { get; private set; }
        public CollisionCandidateStoreAuthorityFailureReason FirstFailureReason { get; private set; }

        internal void RecordRequestedTick()
        {
            RequestedTickCount++;
        }

        internal void RecordAppliedTick()
        {
            AppliedTickCount++;
        }

        internal void RecordLegacyFallbackTick()
        {
            LegacyFallbackTickCount++;
        }

        internal void RecordSampledOracleTick()
        {
            SampledOracleTickCount++;
        }

        internal void RecordStoreOnlyTick()
        {
            StoreOnlyTickCount++;
        }

        internal void RecordLegacyListCreatedOrWritten()
        {
            LegacyListCreatedOrWrittenCount++;
        }

        internal void RecordStoreOnlyHardFailure()
        {
            StoreOnlyHardFailureCount++;
        }

        internal void RecordRangeRead()
        {
            RangeReadCount++;
        }

        internal void RecordEntryRead()
        {
            EntryReadCount++;
        }

        internal void RecordFailure(CollisionCandidateStoreAuthorityFailureReason reason)
        {
            FailureCount++;
            if (FirstFailureReason == CollisionCandidateStoreAuthorityFailureReason.None)
                FirstFailureReason = reason;
        }
    }

    /// <summary>
    /// Fixed-slab candidate entry. TargetHandle is sampled-oracle diagnostic
    /// generation snapshot only: current gameplay resolves a collected hit by
    /// TargetSlot, so target generation must never gate candidate consumption.
    /// RuntimeItr retains the collected source-itr metadata; consumers still resolve
    /// current pair-dependent itr state at consume time.
    /// </summary>
    public readonly struct CollisionCandidateStoreEntry
    {
        public CollisionCandidateStoreEntry(
            int targetSlot,
            RuntimeEntityHandle targetHandle,
            int bodyX,
            int itrIndex,
            InteractionArea runtimeItr,
            bool zeroAttackerHpOnConsume,
            bool releaseHeavyHeldTargetOnConsume)
        {
            TargetSlot = targetSlot;
            TargetHandle = targetHandle;
            BodyX = bodyX;
            ItrIndex = itrIndex;
            RuntimeItr = runtimeItr;
            ZeroAttackerHpOnConsume = zeroAttackerHpOnConsume;
            ReleaseHeavyHeldTargetOnConsume = releaseHeavyHeldTargetOnConsume;
        }

        public int TargetSlot { get; }
        public RuntimeEntityHandle TargetHandle { get; }
        public int BodyX { get; }
        public int ItrIndex { get; }
        public InteractionArea RuntimeItr { get; }
        public bool ZeroAttackerHpOnConsume { get; }
        public bool ReleaseHeavyHeldTargetOnConsume { get; }
    }

    /// <summary>
    /// Query-owned candidate store. In StoreAuthority mode this is the independent
    /// producer; sampled legacy lists are only an oracle. Each attacker runtime slot
    /// owns one row of HitCandidateMax entries isolated by attacker generation.
    /// </summary>
    internal sealed class CollisionCandidateStore
    {
        internal const int HitCandidateMax = 20;

        private readonly CollisionCandidateStoreDiagnostics diagnostics;
        private CollisionCandidateStoreEntry[] entries;
        private int[] counts;
        private uint[] attackerGenerations;
        private uint[] rowEpochs;
        private int[] nonEmptyAttackerSlots;
        private int nonEmptyAttackerCount;
        private uint currentEpoch;
        private bool building;
        private bool visible;

        internal CollisionCandidateStore(
            CollisionCandidateStoreDiagnostics diagnostics)
        {
            this.diagnostics = diagnostics ??
                               throw new ArgumentNullException(nameof(diagnostics));
            entries = Array.Empty<CollisionCandidateStoreEntry>();
            counts = Array.Empty<int>();
            attackerGenerations = Array.Empty<uint>();
            rowEpochs = Array.Empty<uint>();
            nonEmptyAttackerSlots = Array.Empty<int>();
        }

        internal int RuntimeCapacity => counts.Length;
        internal bool IsBuilding => building;
        internal bool IsVisible => visible;

        internal bool PrepareCapacity(int runtimeCapacity)
        {
            if (building || visible)
                return false;
            return runtimeCapacity > 0 && EnsureCapacity(runtimeCapacity);
        }

        internal void AbortBuild()
        {
            building = false;
            visible = false;
        }

        internal bool BeginBuild(int runtimeCapacity)
        {
            AbortBuild();
            if (runtimeCapacity <= 0)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.RuntimeCapacityInvalid);
                return false;
            }

            if (!EnsureCapacity(runtimeCapacity))
                return false;

            AdvanceEpoch();
            nonEmptyAttackerCount = 0;
            building = true;
            diagnostics.RecordBuildTick();
            return true;
        }

        internal bool CompleteBuild()
        {
            if (!building)
                return false;

            building = false;
            visible = true;
            return true;
        }

        internal void EndTickVisibility()
        {
            AbortBuild();
        }

        internal bool TryBeginAttacker(RuntimeEntityHandle attackerHandle)
        {
            if (!building ||
                !attackerHandle.IsValid ||
                attackerHandle.Slot < 0 ||
                attackerHandle.Slot >= counts.Length)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.AttackerSlotOutOfRange);
                return false;
            }

            int slot = attackerHandle.Slot;
            if (rowEpochs[slot] == currentEpoch)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.DuplicateAttackerSlot);
                return false;
            }

            rowEpochs[slot] = currentEpoch;
            attackerGenerations[slot] = attackerHandle.Generation;
            counts[slot] = 0;
            return true;
        }

        internal bool TryWriteAt(
            RuntimeEntityHandle attackerHandle,
            int candidateIndex,
            in CollisionCandidateStoreEntry entry)
        {
            int slot = attackerHandle.Slot;
            if (!building ||
                !attackerHandle.IsValid ||
                slot < 0 ||
                slot >= counts.Length ||
                rowEpochs[slot] != currentEpoch ||
                attackerGenerations[slot] != attackerHandle.Generation)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.AttackerGenerationMismatch);
                return false;
            }

            int count = counts[slot];
            if (candidateIndex < 0 ||
                candidateIndex >= HitCandidateMax ||
                candidateIndex > count)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.CandidateCountExceedsMaximum);
                return false;
            }

            entries[slot * HitCandidateMax + candidateIndex] = entry;
            if (candidateIndex == count)
            {
                if (count == 0)
                    nonEmptyAttackerSlots[nonEmptyAttackerCount++] = slot;
                counts[slot] = count + 1;
            }
            return true;
        }

        internal bool TryReplaceSingle(
            RuntimeEntityHandle attackerHandle,
            in CollisionCandidateStoreEntry entry)
        {
            int slot = attackerHandle.Slot;
            if (!building ||
                !attackerHandle.IsValid ||
                slot < 0 ||
                slot >= counts.Length ||
                rowEpochs[slot] != currentEpoch ||
                attackerGenerations[slot] != attackerHandle.Generation)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.AttackerGenerationMismatch);
                return false;
            }

            entries[slot * HitCandidateMax] = entry;
            if (counts[slot] == 0)
                nonEmptyAttackerSlots[nonEmptyAttackerCount++] = slot;
            counts[slot] = 1;
            return true;
        }

        internal int VisibleNonEmptyAttackerCount =>
            visible && !building ? nonEmptyAttackerCount : 0;

        internal bool TryGetVisibleNonEmptyAttackerHandle(
            int index,
            out RuntimeEntityHandle attackerHandle)
        {
            attackerHandle = RuntimeEntityHandle.Invalid;
            if (!visible || building || index < 0 || index >= nonEmptyAttackerCount)
                return false;

            int slot = nonEmptyAttackerSlots[index];
            if (slot < 0 ||
                slot >= counts.Length ||
                rowEpochs[slot] != currentEpoch ||
                counts[slot] <= 0)
            {
                return false;
            }

            attackerHandle = new RuntimeEntityHandle(
                slot,
                attackerGenerations[slot]);
            return attackerHandle.IsValid;
        }

        internal bool TryGetBuildingAttackerRowForCompare(
            RuntimeEntityHandle attackerHandle,
            out int count)
        {
            return TryGetAttackerRow(
                attackerHandle,
                requireBuilding: true,
                out count);
        }

        internal bool TryGetBuildingCandidateForCompare(
            RuntimeEntityHandle attackerHandle,
            int candidateIndex,
            out CollisionCandidateStoreEntry entry)
        {
            return TryGetCandidate(
                attackerHandle,
                candidateIndex,
                requireBuilding: true,
                out entry);
        }

        internal bool TryGetVisibleAttackerRow(
            RuntimeEntityHandle attackerHandle,
            out int count)
        {
            return TryGetAttackerRow(
                attackerHandle,
                requireBuilding: false,
                out count);
        }

        internal bool TryGetVisibleCandidate(
            RuntimeEntityHandle attackerHandle,
            int candidateIndex,
            out CollisionCandidateStoreEntry entry)
        {
            return TryGetCandidate(
                attackerHandle,
                candidateIndex,
                requireBuilding: false,
                out entry);
        }

        internal bool TryGetProducerAttackerRow(
            RuntimeEntityHandle attackerHandle,
            out int count)
        {
            count = 0;
            int slot = attackerHandle.Slot;
            if (!attackerHandle.IsValid ||
                currentEpoch == 0 ||
                slot < 0 ||
                slot >= counts.Length ||
                rowEpochs[slot] != currentEpoch ||
                attackerGenerations[slot] != attackerHandle.Generation)
            {
                return false;
            }

            count = counts[slot];
            return true;
        }

        internal void RecordMismatch(CollisionCandidateStoreMismatchReason reason)
        {
            diagnostics.RecordMismatch(reason);
        }

        internal void RecordInvalid(CollisionCandidateStoreMismatchReason reason)
        {
            diagnostics.RecordInvalid(reason);
        }

        internal void RecordComparedAttacker()
        {
            diagnostics.RecordComparedAttacker();
        }

        internal void RecordComparedCandidate()
        {
            diagnostics.RecordComparedCandidate();
        }

        private bool EnsureCapacity(int runtimeCapacity)
        {
            if (runtimeCapacity <= counts.Length)
                return true;

            try
            {
                int entryCapacity = checked(runtimeCapacity * HitCandidateMax);
                var grownEntries = new CollisionCandidateStoreEntry[entryCapacity];
                var grownCounts = new int[runtimeCapacity];
                var grownAttackerGenerations = new uint[runtimeCapacity];
                var grownRowEpochs = new uint[runtimeCapacity];
                var grownNonEmptyAttackerSlots = new int[runtimeCapacity];
                entries = grownEntries;
                counts = grownCounts;
                attackerGenerations = grownAttackerGenerations;
                rowEpochs = grownRowEpochs;
                nonEmptyAttackerSlots = grownNonEmptyAttackerSlots;
                nonEmptyAttackerCount = 0;
                currentEpoch = 0;
                return true;
            }
            catch (OverflowException)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.RuntimeCapacityInvalid);
                AbortBuild();
                return false;
            }
            catch (OutOfMemoryException)
            {
                diagnostics.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.RuntimeCapacityInvalid);
                AbortBuild();
                return false;
            }
        }

        private void AdvanceEpoch()
        {
            currentEpoch++;
            if (currentEpoch != 0)
                return;

            Array.Clear(rowEpochs, 0, rowEpochs.Length);
            currentEpoch = 1;
        }

        private bool TryGetAttackerRow(
            RuntimeEntityHandle attackerHandle,
            bool requireBuilding,
            out int count)
        {
            count = 0;
            int slot = attackerHandle.Slot;
            bool readable = requireBuilding
                ? building && !visible
                : visible && !building;
            if (!readable ||
                !attackerHandle.IsValid ||
                slot < 0 ||
                slot >= counts.Length ||
                rowEpochs[slot] != currentEpoch ||
                attackerGenerations[slot] != attackerHandle.Generation)
            {
                return false;
            }

            count = counts[slot];
            return true;
        }

        private bool TryGetCandidate(
            RuntimeEntityHandle attackerHandle,
            int candidateIndex,
            bool requireBuilding,
            out CollisionCandidateStoreEntry entry)
        {
            entry = default;
            if (!TryGetAttackerRow(
                    attackerHandle,
                    requireBuilding,
                    out int count) ||
                candidateIndex < 0 ||
                candidateIndex >= count)
            {
                return false;
            }

            entry = entries[attackerHandle.Slot * HitCandidateMax + candidateIndex];
            return true;
        }
    }
}
