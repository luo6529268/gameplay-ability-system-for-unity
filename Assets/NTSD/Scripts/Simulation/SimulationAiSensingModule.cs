using System;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    internal sealed class SimulationAiSensingModule
    {
        private readonly RuntimeSlotTable runtimeSlots;

        internal sealed class AiSoASensingRows : AiSensingSnapshot
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

        internal struct AiSoASensingResult
        {
            internal int TickIndex;
            internal int SelfSlot;
            internal uint SelfGeneration;
            internal int SelfIdentity;
            internal int InitialSelectedSlot;
            internal int InitialBestDist;
            internal bool InitialSameZLane;
            internal bool CachedTargetEligible;
            internal bool CacheRandomExpected;
            internal int CacheRoll;
            internal uint CacheRngStateBefore;
            internal uint CacheRngStateAfter;
            internal ulong CacheRngCallsBefore;
            internal ulong CacheRngCallsAfter;
            internal int CachedSelectedSlot;
            internal int PostSpecialSelectedSlot;
            internal int SpecialBestDist;
            internal int SpecialFlags;
        }

        internal struct AiSoANearestResult
        {
            internal int SelectedSlot;
            internal int BestDist;
            internal bool SameZLane;
            internal ulong SnapshotEpoch;
            internal uint SelectedGeneration;
            internal int SelectedIdentity;
        }

        internal struct AiSoASpecialResult
        {
            internal int SelectedSlot;
            internal int BestDist;
            internal bool SameZLane;
            internal ulong SnapshotEpoch;
            internal uint SelectedGeneration;
            internal int SelectedIdentity;
            internal int Flags;
        }

        internal struct RowRefreshResult
        {
            internal int Slot;
            internal uint Generation;
            internal int Identity;
            internal bool RoleRebuilt;
            internal bool TeamRebuilt;
            internal int PreviousX;
            internal int CurrentX;
            internal int PreviousTeam;
            internal int CurrentTeam;
            internal bool WasGroundRole;
            internal bool WasAirRole;
            internal bool IsGroundRole;
            internal bool IsAirRole;
            internal bool WasLivingCharacter;
            internal bool IsLivingCharacter;
            internal int PreviousHp;
            internal int CurrentHp;
        }

        private AiSoASensingResult expected;

        internal SimulationAiSensingModule(RuntimeSlotTable runtimeSlots)
        {
            this.runtimeSlots = runtimeSlots ??
                throw new ArgumentNullException(nameof(runtimeSlots));
        }

        internal AiSoASensingRows Rows { get; set; }
        internal ref AiSoASensingResult Expected => ref expected;
        internal AiSensingMode Mode { get; set; }
        internal ulong SnapshotEpoch { get; set; }
        internal bool SnapshotValid { get; set; }
        internal bool PassInvalidated { get; set; }
        internal bool ComparisonPending { get; set; }
        internal int PendingMismatchMask { get; set; }
        internal AiSoASensingShadowMismatch PendingFirstMismatch { get; set; }
        internal BattleAiExecutionProfile ExecutionProfile { get; set; } =
            BattleAiExecutionProfile.LegacyCanonical;
        internal AiDecisionOwnedInputMode DecisionOwnedInputMode { get; set; } =
            AiDecisionOwnedInputMode.SnapshotCopy;
        internal bool CandidateExecutionEnabled { get; set; }
        internal bool CandidatePassLatchedToLegacy { get; set; }
        internal bool CandidateForceNearestFailure { get; set; }
        internal bool CandidateForceSpecialFailure { get; set; }
        internal int ShadowQueryCount { get; set; }
        internal int ShadowInvalidationCount { get; set; }
        internal int ShadowPurityMismatchCount { get; set; }
        internal int ShadowInitialMismatchCount { get; set; }
        internal int ShadowCachedMismatchCount { get; set; }
        internal int ShadowPostSpecialMismatchCount { get; set; }
        internal int ShadowMismatchMask { get; set; }
        internal int ShadowLastMismatchMask { get; set; }
        internal bool ShadowComparisonPublished { get; set; }
        internal AiSoASensingShadowMismatch ShadowFirstMismatch { get; set; }
        internal int CandidateNearestQueryCount { get; set; }
        internal int CandidateSpecialQueryCount { get; set; }
        internal int CandidateEmptySpecialFastPathCount { get; set; }
        internal long CandidateGroundXRowVisitCount { get; set; }
        internal long CandidateAirXRowVisitCount { get; set; }
        internal long CandidateSpecialSlotVisitCount { get; set; }
        internal int CandidateLegacyNearestScanCount { get; set; }
        internal int CandidateLegacySpecialScanCount { get; set; }
        internal int CandidatePreRandomFailureCount { get; set; }
        internal int CandidatePostRandomFailureCount { get; set; }
        internal int CandidateFusedSnapshotBuildCount { get; set; }
        internal long CandidateFusedSnapshotSlotVisitCount { get; set; }
        internal int CandidateFusedSnapshotFailureCount { get; set; }
        internal long CandidateSnapshotRefreshCount { get; set; }

        internal void InitializeRows(int capacity)
        {
            Rows = new AiSoASensingRows(capacity);
        }

        internal void GrowRows(int capacity)
        {
            if (Rows == null)
            {
                InitializeRows(capacity);
            }
            else if (capacity > Rows.Capacity)
            {
                Rows = Rows.GrowTo(capacity);
            }

            if (SnapshotValid)
                InvalidateShadowPass();
        }

        internal void ObserveSnapshotBuildEpoch(
            ulong expectedEpoch,
            ulong observedEpoch)
        {
            if (SnapshotValid &&
                (expectedEpoch != SnapshotEpoch || observedEpoch != expectedEpoch))
            {
                InvalidateShadowPass();
            }
        }

        internal void ClearShadowSnapshot()
        {
            SnapshotValid = false;
            PassInvalidated = false;
            ComparisonPending = false;
            PendingMismatchMask = 0;
            PendingFirstMismatch = default;
            SnapshotEpoch = 0;
            if (Rows != null)
                Rows.CapturedOccupancyEpoch = 0;
            CandidatePassLatchedToLegacy = false;
        }

        internal void InvalidateShadowPass()
        {
            SnapshotValid = false;
            ComparisonPending = false;
            PendingMismatchMask = 0;
            PendingFirstMismatch = default;
            ShadowLastMismatchMask = 0;
            ShadowComparisonPublished = false;
            if (PassInvalidated)
                return;

            PassInvalidated = true;
            ShadowInvalidationCount++;
        }

        internal bool TryBuildShadowSnapshot(
            LF2Entity[] slots,
            ulong expectedEpoch)
        {
            SnapshotValid = false;
            PassInvalidated = false;
            CandidatePassLatchedToLegacy = false;
            ComparisonPending = false;
            PendingMismatchMask = 0;
            PendingFirstMismatch = default;
            ShadowLastMismatchMask = 0;
            ShadowComparisonPublished = false;
            SnapshotEpoch = expectedEpoch;
            Rows.CapturedOccupancyEpoch = expectedEpoch;
            Array.Clear(Rows.Included, 0, Rows.Capacity);
            Array.Clear(Rows.SpecialScanMember, 0, Rows.Capacity);
            Rows.SpecialSlotCount = 0;
            Rows.SpecialIndexReady = false;
            Rows.GroundRoleSlotCount = 0;
            Rows.AirRoleSlotCount = 0;
            Rows.GroundRoleTeamSummaryCount = 0;
            Rows.AirRoleTeamSummaryCount = 0;
            Rows.RoleIndexesReady = false;
            Rows.TeamSummaryCount = 0;
            Rows.TeamSummariesReady = false;

            for (int slot = 0; slot < slots.Length; slot++)
            {
                LF2Entity entity = slots[slot];
                if (entity == null)
                    continue;

                if (!runtimeSlots.TryGetCurrentHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle) ||
                    !TryCaptureRow(Rows, entity, slot, handle.Generation, true))
                {
                    InvalidateShadowPass();
                    return false;
                }
            }

            if (runtimeSlots.OccupancyEpoch != expectedEpoch)
            {
                InvalidateShadowPass();
                return false;
            }

            Rows.SpecialIndexReady = true;
            BuildRoleIndexes(Rows);
            BuildTeamSummaries(Rows);
            return true;
        }

        internal bool TryBuildCandidateFusedSnapshot(
            SimulationWorld world,
            SimulationAiInputModule input,
            int expectedCapacity,
            ulong expectedEpoch)
        {
            CandidateFusedSnapshotBuildCount++;
            SnapshotValid = false;
            PassInvalidated = false;
            CandidatePassLatchedToLegacy = false;
            ComparisonPending = false;
            PendingMismatchMask = 0;
            PendingFirstMismatch = default;
            ShadowLastMismatchMask = 0;
            ShadowComparisonPublished = false;
            SnapshotEpoch = expectedEpoch;
            if (Rows != null)
                Rows.CapturedOccupancyEpoch = expectedEpoch;

            LF2Entity[] slots = input.Slots;
            AiSoASensingRows rows = Rows;
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
            input.ResetMoveModeFirst10Snapshot();
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
                CandidateFusedSnapshotSlotVisitCount++;

                if (!runtimeSlots.IsAddressable(slot))
                {
                    soaProven = false;
                    if (slot < input.MoveModeFirst10Present.Length)
                        moveModeFirst10Proven = false;
                    continue;
                }

                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                if (view.RuntimeSlot != slot)
                {
                    soaProven = false;
                    if (slot < input.MoveModeFirst10Present.Length)
                        moveModeFirst10Proven = false;
                    continue;
                }

                if (!view.Claimed)
                {
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
                    if (slot < input.MoveModeFirst10Present.Length)
                        moveModeFirst10Proven = false;
                    continue;
                }

                if (!world.IsActiveForCurrentPassInternal(entity))
                    continue;

                if (slots == null || slot >= slots.Length || rows == null)
                {
                    soaProven = false;
                    if (slot < input.MoveModeFirst10Present.Length)
                        moveModeFirst10Proven = false;
                    continue;
                }

                slots[slot] = entity;
                if (slot < input.MoveModeFirst10Present.Length)
                {
                    input.CaptureMoveModeFirst10Candidate(
                        slot,
                        world.GetAiHpForInputModule(entity),
                        true,
                        new RuntimeEntityHandle(slot, view.Generation),
                        world.IsLivingCharacterDatForAiInputModule(entity),
                        world.GetAiXForInputModule(entity),
                        world.GetAiZForInputModule(entity));
                }

                if (!TryCaptureRow(
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
                runtimeSlots.LogicalCapacity == expectedCapacity &&
                ReferenceEquals(input.Slots, slots) &&
                slots != null &&
                slots.Length == expectedCapacity &&
                ReferenceEquals(Rows, rows) &&
                rows != null &&
                rows.Capacity == expectedCapacity;
            bool epochProven = runtimeSlots.OccupancyEpoch == expectedEpoch;
            input.MoveModeFirst10Valid =
                moveModeFirst10Proven &&
                finalStructureProven &&
                epochProven;

            if (!soaProven || !finalStructureProven || !epochProven)
            {
                CandidateFusedSnapshotFailureCount++;
                InvalidateShadowPass();
                return false;
            }

            rows.SpecialIndexReady = true;
            BuildRoleIndexes(rows);
            BuildTeamSummaries(rows);
            return true;
        }

        internal void CompleteSnapshotCapture()
        {
            SnapshotValid = true;
        }

        internal bool ValidateShadowSnapshot()
        {
            if (!SnapshotValid || PassInvalidated)
                return false;

            if (runtimeSlots.OccupancyEpoch != SnapshotEpoch)
            {
                InvalidateShadowPass();
                return false;
            }

            AiSoASensingRows rows = Rows;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!rows.Included[slot])
                    continue;

                if (!runtimeSlots.IsAddressable(slot))
                {
                    InvalidateShadowPass();
                    return false;
                }

                RuntimeSlotTable.ReadOnlySlotView view =
                    runtimeSlots.GetReadOnlyView(slot);
                if (!view.Claimed ||
                    view.Generation != rows.Generation[slot] ||
                    view.Entity?.Runtime == null ||
                    view.Entity.Runtime.SlotIndex != slot ||
                    view.Entity.Runtime.StableId != rows.Identity[slot])
                {
                    InvalidateShadowPass();
                    return false;
                }
            }

            return true;
        }

        internal bool TryRefreshRowAfterCharacterInput(
            LF2Entity entity,
            out RowRefreshResult result)
        {
            result = default;
            if (!SnapshotValid || PassInvalidated)
                return false;

            if (runtimeSlots.OccupancyEpoch != SnapshotEpoch ||
                entity?.Runtime == null)
            {
                InvalidateShadowPass();
                return false;
            }

            int slot = entity.Runtime.SlotIndex;
            AiSoASensingRows rows = Rows;
            if (slot < 0 ||
                slot >= rows.Capacity ||
                !rows.Included[slot] ||
                !runtimeSlots.TryGetCurrentHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != rows.Generation[slot] ||
                entity.Runtime.StableId != rows.Identity[slot])
            {
                InvalidateShadowPass();
                return false;
            }

            int previousX = rows.X[slot];
            int previousTeam = rows.Team[slot];
            int previousHp = rows.Hp[slot];
            int previousObjectId = rows.ObjectId[slot];
            bool previousSpecialMember = rows.SpecialScanMember[slot];
            bool wasGroundRole = IsGroundRoleMember(rows, slot);
            bool wasAirRole = IsAirRoleMember(rows, slot);
            bool wasLivingCharacter = IsLivingCharacterRow(rows, slot);
            if (!TryCaptureRow(entity: entity,
                    rows: rows,
                    slot: slot,
                    generation: handle.Generation,
                    captureSpecialMembership: false))
            {
                InvalidateShadowPass();
                return false;
            }

            bool currentSpecialMember =
                slot >= 20 &&
                SimulationAiInputModule.IsSpecialScanObjectId(rows.ObjectId[slot]);
            if (previousObjectId != rows.ObjectId[slot] ||
                previousSpecialMember != currentSpecialMember)
            {
                InvalidateShadowPass();
                return false;
            }

            bool isGroundRole = IsGroundRoleMember(rows, slot);
            bool isAirRole = IsAirRoleMember(rows, slot);
            bool roleRebuilt = previousX != rows.X[slot] ||
                               previousTeam != rows.Team[slot] ||
                               wasGroundRole != isGroundRole ||
                               wasAirRole != isAirRole;
            if (roleRebuilt)
                BuildRoleIndexes(rows);

            bool isLivingCharacter = IsLivingCharacterRow(rows, slot);
            bool teamRebuilt = wasLivingCharacter != isLivingCharacter ||
                               previousTeam != rows.Team[slot] ||
                               previousHp != rows.Hp[slot];
            if (teamRebuilt)
                BuildTeamSummaries(rows);

            result = new RowRefreshResult
            {
                Slot = slot,
                Generation = handle.Generation,
                Identity = entity.Runtime.StableId,
                RoleRebuilt = roleRebuilt,
                TeamRebuilt = teamRebuilt,
                PreviousX = previousX,
                CurrentX = rows.X[slot],
                PreviousTeam = previousTeam,
                CurrentTeam = rows.Team[slot],
                WasGroundRole = wasGroundRole,
                WasAirRole = wasAirRole,
                IsGroundRole = isGroundRole,
                IsAirRole = isAirRole,
                WasLivingCharacter = wasLivingCharacter,
                IsLivingCharacter = isLivingCharacter,
                PreviousHp = previousHp,
                CurrentHp = rows.Hp[slot],
            };
            if (Mode == AiSensingMode.SoAAiSensing)
                CandidateSnapshotRefreshCount++;
            return true;
        }

        internal bool TryRunShadowQuery(
            int selfSlot,
            int inputPhase,
            uint rngState,
            bool forceFullSpecialScan,
            out AiSoASensingResult result)
        {
            result = default;
            if (!TryQueryNearest(
                    selfSlot,
                    inputPhase,
                    out AiSoANearestResult nearest))
            {
                return false;
            }

            result.SelfSlot = selfSlot;
            result.InitialSelectedSlot = nearest.SelectedSlot;
            result.InitialBestDist = nearest.BestDist;
            result.InitialSameZLane = nearest.SameZLane;

            int selectedSlot = nearest.SelectedSlot;
            int savedTargetSlot = Rows.CachedTargetSlot[selfSlot];
            if (IsLivingCharacterRow(Rows, savedTargetSlot) &&
                NextLocalRandom(ref rngState) % 30 > 0)
            {
                selectedSlot = savedTargetSlot;
            }
            result.CachedSelectedSlot = selectedSlot;
            result.PostSpecialSelectedSlot = selectedSlot;
            result.SpecialBestDist = 10000;
            if (selectedSlot < 0)
                return true;

            if (!TryQuerySpecial(
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

        internal bool TryQueryNearest(
            int selfSlot,
            int inputPhase,
            out AiSoANearestResult result)
        {
            result = default;
            if (!AiSensingKernel.TryFindNearest(
                    Rows,
                    selfSlot,
                    inputPhase,
                    out AiSensingNearestResult kernelResult))
            {
                return false;
            }

            result.SelectedSlot = kernelResult.SelectedSlot;
            result.BestDist = kernelResult.BestDist;
            result.SameZLane = kernelResult.SameZLane;
            result.SnapshotEpoch = kernelResult.CapturedOccupancyEpoch;
            result.SelectedGeneration = kernelResult.SelectedGeneration;
            result.SelectedIdentity = kernelResult.SelectedIdentity;
            CandidateGroundXRowVisitCount += kernelResult.GroundRowVisits;
            CandidateAirXRowVisitCount += kernelResult.AirRowVisits;
            return true;
        }

        internal bool TryQuerySpecial(
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
                    Rows,
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
            CandidateSpecialSlotVisitCount += kernelResult.SlotVisits;
            return true;
        }

        internal bool TryBuildEmptySpecialResult(
            int selectedSlot,
            bool sameZLane,
            out AiSoASpecialResult result)
        {
            result = default;
            if (Rows == null ||
                selectedSlot < 0 ||
                selectedSlot >= Rows.Capacity ||
                !Rows.Included[selectedSlot])
            {
                return false;
            }

            result.SelectedSlot = selectedSlot;
            result.BestDist = 10000;
            result.SameZLane = sameZLane;
            result.SnapshotEpoch = SnapshotEpoch;
            CaptureSelectedIdentity(
                Rows,
                selectedSlot,
                out result.SelectedGeneration,
                out result.SelectedIdentity);
            result.Flags = 0;
            return true;
        }

        internal bool ValidateCandidateSelfHandle(
            LF2Entity self,
            out int selfSlot)
        {
            selfSlot = self?.Runtime?.SlotIndex ?? -1;
            return CandidateExecutionEnabled &&
                   SnapshotValid &&
                   !PassInvalidated &&
                   runtimeSlots.OccupancyEpoch == SnapshotEpoch &&
                   selfSlot >= 0 &&
                   selfSlot < Rows.Capacity &&
                   Rows.Included[selfSlot] &&
                   self.Runtime.StableId == Rows.Identity[selfSlot] &&
                   runtimeSlots.TryGetCurrentHandle(
                       selfSlot,
                       self,
                       out RuntimeEntityHandle handle) &&
                   handle.Generation == Rows.Generation[selfSlot];
        }

        internal bool ValidateCandidateSelectedHandle(
            int selectedSlot,
            uint selectedGeneration,
            int selectedIdentity)
        {
            if (selectedSlot < 0)
                return selectedGeneration == 0 && selectedIdentity == 0;

            if (selectedSlot >= Rows.Capacity ||
                !Rows.Included[selectedSlot] ||
                Rows.Generation[selectedSlot] != selectedGeneration ||
                Rows.Identity[selectedSlot] != selectedIdentity ||
                !runtimeSlots.IsAddressable(selectedSlot))
            {
                return false;
            }

            RuntimeSlotTable.ReadOnlySlotView view =
                runtimeSlots.GetReadOnlyView(selectedSlot);
            return view.Claimed &&
                   view.Generation == selectedGeneration &&
                   view.Entity?.Runtime != null &&
                   view.Entity.Runtime.SlotIndex == selectedSlot &&
                   view.Entity.Runtime.StableId == selectedIdentity;
        }

        internal void RecordPendingMismatch(
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
            PendingMismatchMask |= mismatchBit;
            if (PendingFirstMismatch.Kind != AiSoASensingShadowMismatchKind.None)
                return;

            PendingFirstMismatch = new AiSoASensingShadowMismatch
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

        internal void BeginShadowComparison(
            SimulationWorld world,
            LF2Entity self,
            int tickIndex)
        {
            ComparisonPending = false;
            PendingMismatchMask = 0;
            PendingFirstMismatch = default;
            ShadowLastMismatchMask = 0;
            ShadowComparisonPublished = false;
            if (self?.Runtime == null ||
                self.Runtime.HP <= 0 ||
                self.Runtime.Unk3FC > -1000 ||
                !ValidateShadowSnapshot())
            {
                return;
            }

            int selfSlot = self.Runtime.SlotIndex;
            if (selfSlot < 0 ||
                selfSlot >= Rows.Capacity ||
                !Rows.Included[selfSlot] ||
                Rows.Identity[selfSlot] != self.Runtime.StableId)
            {
                InvalidateShadowPass();
                return;
            }

            if (!runtimeSlots.TryGetCurrentHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle selfHandle) ||
                selfHandle.Generation != Rows.Generation[selfSlot])
            {
                InvalidateShadowPass();
                return;
            }

            uint rngStateBefore = world.Rng?.State ?? 0;
            ulong rngCallsBefore = world.Rng?.CallCount ?? 0;
            ulong inputSignatureBefore =
                SimulationAiInputModule.CaptureNearestInputSignature(self.Runtime);
            bool succeeded = TryQueryNearest(
                selfSlot,
                world.InputPhase,
                out AiSoANearestResult nearest);
            uint rngStateAfter = world.Rng?.State ?? 0;
            ulong rngCallsAfter = world.Rng?.CallCount ?? 0;
            ulong inputSignatureAfter =
                SimulationAiInputModule.CaptureNearestInputSignature(self.Runtime);

            if (rngStateBefore != rngStateAfter ||
                rngCallsBefore != rngCallsAfter ||
                inputSignatureBefore != inputSignatureAfter)
            {
                RecordPendingMismatch(
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

            expected = default;
            expected.SelfSlot = selfSlot;
            expected.InitialSelectedSlot = nearest.SelectedSlot;
            expected.InitialBestDist = nearest.BestDist;
            expected.InitialSameZLane = nearest.SameZLane;
            expected.CacheRngStateBefore = rngStateBefore;
            expected.CacheRngCallsBefore = rngCallsBefore;
            int savedTargetSlot = Rows.CachedTargetSlot[selfSlot];
            bool cachedTargetEligible =
                IsLivingCharacterRow(Rows, savedTargetSlot);
            expected.CachedTargetEligible = cachedTargetEligible;
            expected.CacheRandomExpected = cachedTargetEligible;
            uint predictedRngState = rngStateBefore;
            ulong predictedRngCalls = rngCallsBefore;
            int predictedCacheRoll = 0;
            int predictedCachedSelection = nearest.SelectedSlot;
            if (cachedTargetEligible)
            {
                predictedCacheRoll = NextLocalRandom(ref predictedRngState) % 30;
                predictedRngCalls++;
                if (predictedCacheRoll > 0)
                    predictedCachedSelection = savedTargetSlot;
            }
            expected.CacheRoll = predictedCacheRoll;
            expected.CacheRngStateAfter = predictedRngState;
            expected.CacheRngCallsAfter = predictedRngCalls;
            expected.CachedSelectedSlot = predictedCachedSelection;
            expected.PostSpecialSelectedSlot = predictedCachedSelection;
            expected.SpecialBestDist = 10000;
            expected.TickIndex = tickIndex;
            expected.SelfGeneration = selfHandle.Generation;
            expected.SelfIdentity = self.Runtime.StableId;
            ShadowQueryCount++;
            ComparisonPending = true;
        }

        internal bool CaptureNearestForSelfCheck(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            out int selectedSlot,
            out int bestDist,
            out bool sameZLane)
        {
            EnsureSelfCheckCanRun(world);
            AiSensingMode previousMode = Mode;
            bool snapshotBuilt = false;
            try
            {
                Mode = AiSensingMode.SoAShadowAiSensing;
                world.BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                if (!ValidateShadowSnapshot() ||
                    self?.Runtime == null ||
                    !TryRunShadowQuery(
                        self.Runtime.SlotIndex,
                        inputPhase,
                        world.Rng?.State ?? 0,
                        world.ForceFullAiSpecialScanForDiagnostics,
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
                CompleteSelfCheck(world, previousMode, snapshotBuilt);
            }
        }

        internal long MeasureShadowAllocationsForSelfCheck(
            SimulationWorld world,
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            EnsureSelfCheckCanRun(world);
            AiSensingMode previousMode = Mode;
            bool snapshotBuilt = false;
            try
            {
                Mode = AiSensingMode.SoAShadowAiSensing;
                world.BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                if (!ValidateShadowSnapshot() || self?.Runtime == null)
                    return -1;
                int selfSlot = self.Runtime.SlotIndex;
                uint rngState = world.Rng?.State ?? 0;
                for (int index = 0; index < 16; index++)
                {
                    if (!TryRunShadowQuery(
                            selfSlot,
                            inputPhase,
                            rngState,
                            world.ForceFullAiSpecialScanForDiagnostics,
                            out _))
                    {
                        return -1;
                    }
                }

                _ = GC.GetAllocatedBytesForCurrentThread();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int index = 0; index < iterations; index++)
                {
                    if (!TryRunShadowQuery(
                            selfSlot,
                            inputPhase,
                            rngState,
                            world.ForceFullAiSpecialScanForDiagnostics,
                            out _))
                    {
                        return -1;
                    }
                }
                return GC.GetAllocatedBytesForCurrentThread() - before;
            }
            finally
            {
                CompleteSelfCheck(world, previousMode, snapshotBuilt);
            }
        }

        internal bool EpochDriftInvalidatesForSelfCheck(SimulationWorld world)
        {
            EnsureSelfCheckCanRun(world);
            AiSensingMode previousMode = Mode;
            bool snapshotBuilt = false;
            try
            {
                Mode = AiSensingMode.SoAShadowAiSensing;
                world.BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                SnapshotEpoch++;
                return !ValidateShadowSnapshot() && PassInvalidated;
            }
            finally
            {
                CompleteSelfCheck(world, previousMode, snapshotBuilt);
            }
        }

        internal bool GenerationDriftInvalidatesForSelfCheck(
            SimulationWorld world)
        {
            EnsureSelfCheckCanRun(world);
            AiSensingMode previousMode = Mode;
            bool snapshotBuilt = false;
            try
            {
                Mode = AiSensingMode.SoAShadowAiSensing;
                world.BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                for (int slot = 0; slot < Rows.Capacity; slot++)
                {
                    if (!Rows.Included[slot])
                        continue;
                    Rows.Generation[slot]++;
                    return !ValidateShadowSnapshot() && PassInvalidated;
                }
                return false;
            }
            finally
            {
                CompleteSelfCheck(world, previousMode, snapshotBuilt);
            }
        }

        internal bool IdentityDriftInvalidatesForSelfCheck(
            SimulationWorld world)
        {
            EnsureSelfCheckCanRun(world);
            AiSensingMode previousMode = Mode;
            bool snapshotBuilt = false;
            try
            {
                Mode = AiSensingMode.SoAShadowAiSensing;
                world.BuildAiInputSlotSnapshot();
                snapshotBuilt = true;
                for (int slot = 0; slot < Rows.Capacity; slot++)
                {
                    if (!Rows.Included[slot])
                        continue;
                    Rows.Identity[slot]++;
                    return !ValidateShadowSnapshot() && PassInvalidated;
                }
                return false;
            }
            finally
            {
                CompleteSelfCheck(world, previousMode, snapshotBuilt);
            }
        }

        private static void EnsureSelfCheckCanRun(SimulationWorld world)
        {
            if (world.IsTickingForModules)
            {
                throw new InvalidOperationException(
                    "AI sensing self-checks cannot run during a simulation pass.");
            }
        }

        private void CompleteSelfCheck(
            SimulationWorld world,
            AiSensingMode previousMode,
            bool snapshotBuilt)
        {
            try
            {
                if (snapshotBuilt)
                    world.ClearAiInputSlotSnapshot();
            }
            finally
            {
                Mode = previousMode;
            }
        }

        internal void PublishComparison()
        {
            int mismatchMask = PendingMismatchMask;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.ShadowPurity - 1))) != 0)
                ShadowPurityMismatchCount++;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.InitialNearest - 1))) != 0)
                ShadowInitialMismatchCount++;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.CachedSelection - 1))) != 0)
                ShadowCachedMismatchCount++;
            if ((mismatchMask & (1 << ((int)AiSoASensingShadowMismatchKind.PostSpecialSelection - 1))) != 0)
                ShadowPostSpecialMismatchCount++;

            ShadowMismatchMask |= mismatchMask;
            ShadowLastMismatchMask = mismatchMask;
            if (ShadowFirstMismatch.Kind == AiSoASensingShadowMismatchKind.None &&
                PendingFirstMismatch.Kind != AiSoASensingShadowMismatchKind.None)
            {
                ShadowFirstMismatch = PendingFirstMismatch;
            }

            ShadowComparisonPublished = true;
            ComparisonPending = false;
            PendingMismatchMask = 0;
            PendingFirstMismatch = default;
        }

        internal bool IsComparisonCurrent(LF2Entity self, int tickIndex)
        {
            if (!ComparisonPending ||
                !SnapshotValid ||
                PassInvalidated ||
                self?.Runtime == null)
            {
                ComparisonPending = false;
                return false;
            }

            int selfSlot = self.Runtime.SlotIndex;
            if (tickIndex != expected.TickIndex ||
                selfSlot != expected.SelfSlot ||
                runtimeSlots.OccupancyEpoch != SnapshotEpoch ||
                self.Runtime.StableId != expected.SelfIdentity ||
                !runtimeSlots.TryGetCurrentHandle(
                    selfSlot,
                    self,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != expected.SelfGeneration)
            {
                InvalidateShadowPass();
                return false;
            }

            return true;
        }

        internal bool TryRunCandidateNearest(
            LF2Entity self,
            int inputPhase,
            BattleAiInputDetailDiagnostics diagnostics,
            out AiSoANearestResult result)
        {
            if (diagnostics != null)
            {
                diagnostics.BeginPhase(BattleAiInputDetailPhase.CandidateNearest);
                try
                {
                    return TryRunCandidateNearestCore(
                        self,
                        inputPhase,
                        out result);
                }
                finally
                {
                    diagnostics.EndPhase(BattleAiInputDetailPhase.CandidateNearest);
                }
            }

            return TryRunCandidateNearestCore(self, inputPhase, out result);
        }

        internal bool TryRunCandidateNearestCore(
            LF2Entity self,
            int inputPhase,
            out AiSoANearestResult result)
        {
            result = default;
            if (CandidatePassLatchedToLegacy ||
                CandidateForceNearestFailure ||
                !ValidateCandidateSelfHandle(self, out int selfSlot))
            {
                return false;
            }

            ulong epochBefore = runtimeSlots.OccupancyEpoch;
            if (!TryQueryNearest(selfSlot, inputPhase, out result) ||
                result.SnapshotEpoch != epochBefore ||
                runtimeSlots.OccupancyEpoch != epochBefore ||
                !ValidateCandidateSelfHandle(self, out int validatedSelfSlot) ||
                validatedSelfSlot != selfSlot ||
                !ValidateCandidateSelectedHandle(
                    result.SelectedSlot,
                    result.SelectedGeneration,
                    result.SelectedIdentity))
            {
                result = default;
                return false;
            }

            CandidateNearestQueryCount++;
            return true;
        }

        internal bool TryRunCandidateSpecial(
            LF2Entity self,
            int inputPhase,
            int selectedSlot,
            int nearestBestDist,
            bool sameZLane,
            bool forceFullSpecialScan,
            BattleAiInputDetailDiagnostics diagnostics,
            out AiSoASpecialResult result)
        {
            if (diagnostics != null)
            {
                diagnostics.BeginPhase(BattleAiInputDetailPhase.CandidateSpecial);
                try
                {
                    return TryRunCandidateSpecialCore(
                        self,
                        inputPhase,
                        selectedSlot,
                        nearestBestDist,
                        sameZLane,
                        forceFullSpecialScan,
                        out result);
                }
                finally
                {
                    diagnostics.EndPhase(BattleAiInputDetailPhase.CandidateSpecial);
                }
            }

            return TryRunCandidateSpecialCore(
                self,
                inputPhase,
                selectedSlot,
                nearestBestDist,
                sameZLane,
                forceFullSpecialScan,
                out result);
        }

        internal bool TryRunCandidateSpecialCore(
            LF2Entity self,
            int inputPhase,
            int selectedSlot,
            int nearestBestDist,
            bool sameZLane,
            bool forceFullSpecialScan,
            out AiSoASpecialResult result)
        {
            result = default;
            if (CandidatePassLatchedToLegacy ||
                CandidateForceSpecialFailure ||
                !ValidateCandidateSelfHandle(self, out int selfSlot))
            {
                return false;
            }

            ulong epochBefore = runtimeSlots.OccupancyEpoch;
            bool usedEmptySpecialFastPath =
                !forceFullSpecialScan && Rows.SpecialSlotCount == 0;
            bool querySucceeded = usedEmptySpecialFastPath
                ? TryBuildEmptySpecialResult(selectedSlot, sameZLane, out result)
                : TryQuerySpecial(
                    selfSlot,
                    inputPhase,
                    selectedSlot,
                    nearestBestDist,
                    sameZLane,
                    forceFullSpecialScan,
                    out result);

            if (!querySucceeded ||
                result.SnapshotEpoch != epochBefore ||
                runtimeSlots.OccupancyEpoch != epochBefore ||
                !ValidateCandidateSelfHandle(self, out int validatedSelfSlot) ||
                validatedSelfSlot != selfSlot ||
                !ValidateCandidateSelectedHandle(
                    result.SelectedSlot,
                    result.SelectedGeneration,
                    result.SelectedIdentity))
            {
                result = default;
                return false;
            }

            if (usedEmptySpecialFastPath)
                CandidateEmptySpecialFastPathCount++;
            CandidateSpecialQueryCount++;
            return true;
        }

        internal void ContinueComparisonAfterCache(
            LF2Entity self,
            int tickIndex,
            int inputPhase,
            bool forceFullSpecialScan,
            bool cachedTargetEligible,
            bool cacheRandomCalled,
            int cacheRoll,
            uint rngStateBefore,
            ulong rngCallsBefore,
            uint rngStateAfter,
            ulong rngCallsAfter,
            int selectedSlot)
        {
            if (!IsComparisonCurrent(self, tickIndex))
                return;

            bool cacheMismatch =
                cachedTargetEligible != expected.CachedTargetEligible ||
                cacheRandomCalled != expected.CacheRandomExpected ||
                (cacheRandomCalled && cacheRoll != expected.CacheRoll) ||
                rngStateBefore != expected.CacheRngStateBefore ||
                rngCallsBefore != expected.CacheRngCallsBefore ||
                rngStateAfter != expected.CacheRngStateAfter ||
                rngCallsAfter != expected.CacheRngCallsAfter ||
                selectedSlot != expected.CachedSelectedSlot;
            if (cacheMismatch)
            {
                RecordPendingMismatch(
                    AiSoASensingShadowMismatchKind.CachedSelection,
                    expected.SelfSlot,
                    expected.CachedSelectedSlot,
                    selectedSlot,
                    expected.CacheRoll,
                    cacheRoll,
                    PackCacheFlags(
                        expected.CachedTargetEligible,
                        expected.CacheRandomExpected),
                    PackCacheFlags(cachedTargetEligible, cacheRandomCalled));
            }

            int expectedSelectedSlot = expected.CachedSelectedSlot;
            if (expectedSelectedSlot < 0)
                return;

            if (!TryQuerySpecial(
                    expected.SelfSlot,
                    inputPhase,
                    expectedSelectedSlot,
                    expected.InitialBestDist,
                    expected.InitialSameZLane,
                    forceFullSpecialScan,
                    out AiSoASpecialResult special))
            {
                InvalidateShadowPass();
                return;
            }

            expected.PostSpecialSelectedSlot = special.SelectedSlot;
            expected.SpecialBestDist = special.BestDist;
            expected.SpecialFlags = special.Flags;
        }

        internal void CompareInitial(
            LF2Entity self,
            int tickIndex,
            int selectedSlot,
            int bestDist,
            bool sameZLane)
        {
            if (!IsComparisonCurrent(self, tickIndex))
                return;

            if (selectedSlot == expected.InitialSelectedSlot &&
                bestDist == expected.InitialBestDist &&
                sameZLane == expected.InitialSameZLane)
            {
                return;
            }

            RecordPendingMismatch(
                AiSoASensingShadowMismatchKind.InitialNearest,
                expected.SelfSlot,
                expected.InitialSelectedSlot,
                selectedSlot,
                expected.InitialBestDist,
                bestDist,
                expected.InitialSameZLane ? 1 : 0,
                sameZLane ? 1 : 0);
        }

        internal void ComparePostSpecial(
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
            if (!IsComparisonCurrent(self, tickIndex))
                return;

            int flags = PackSpecialFlags(
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
            if (selectedSlot != expected.PostSpecialSelectedSlot ||
                specialBestDist != expected.SpecialBestDist ||
                flags != expected.SpecialFlags)
            {
                RecordPendingMismatch(
                    AiSoASensingShadowMismatchKind.PostSpecialSelection,
                    expected.SelfSlot,
                    expected.PostSpecialSelectedSlot,
                    selectedSlot,
                    expected.SpecialBestDist,
                    specialBestDist,
                    expected.SpecialFlags,
                    flags);
            }

            PublishComparison();
        }

        internal void CompleteComparisonWithoutSpecial(
            LF2Entity self,
            int tickIndex)
        {
            if (IsComparisonCurrent(self, tickIndex))
                PublishComparison();
        }

        private static int PackCacheFlags(
            bool cachedTargetEligible,
            bool cacheRandomCalled)
        {
            int flags = 0;
            if (cachedTargetEligible) flags |= 1 << 0;
            if (cacheRandomCalled) flags |= 1 << 1;
            return flags;
        }

        private static int PackSpecialFlags(
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
            if (specialObjectProximity) flags |= 1 << 0;
            if (specialLeft) flags |= 1 << 1;
            if (specialRight) flags |= 1 << 2;
            if (specialUp) flags |= 1 << 3;
            if (specialDown) flags |= 1 << 4;
            if (specialGuard7A) flags |= 1 << 5;
            if (specialGuard7B) flags |= 1 << 6;
            if (specialForce7AGround) flags |= 1 << 7;
            if (specialC8ThreatSeen) flags |= 1 << 8;
            if (specialPostSelectionSeen) flags |= 1 << 9;
            return flags;
        }

        internal static void CaptureSelectedIdentity(
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

        private static int NextLocalRandom(ref uint state)
        {
            unchecked
            {
                state = state * 0x343FDu + 0x269EC3u;
            }
            return (int)((state >> 16) & 0x7FFFu);
        }

        internal static void BuildRoleIndexes(AiSoASensingRows rows)
        {
            int groundCount = 0;
            int airCount = 0;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (IsGroundRoleMember(rows, slot))
                    rows.GroundRoleSlotsByX[groundCount++] = slot;
                if (IsAirRoleMember(rows, slot))
                    rows.AirRoleSlotsByX[airCount++] = slot;
            }

            rows.GroundRoleSlotCount = groundCount;
            rows.AirRoleSlotCount = airCount;
            if (groundCount > 1)
            {
                SortRoleSlotsByTeamThenXThenSlot(
                    rows,
                    rows.GroundRoleSlotsByX,
                    0,
                    groundCount - 1);
            }
            if (airCount > 1)
            {
                SortRoleSlotsByTeamThenXThenSlot(
                    rows,
                    rows.AirRoleSlotsByX,
                    0,
                    airCount - 1);
            }

            rows.GroundRoleTeamSummaryCount = BuildRoleTeamSpans(
                rows,
                rows.GroundRoleSlotsByX,
                groundCount,
                rows.GroundRoleTeamSummaries);
            rows.AirRoleTeamSummaryCount = BuildRoleTeamSpans(
                rows,
                rows.AirRoleSlotsByX,
                airCount,
                rows.AirRoleTeamSummaries);
            rows.RoleIndexesReady = true;
        }

        internal static void BuildTeamSummaries(AiSoASensingRows rows)
        {
            rows.TeamSummaryCount = 0;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!IsLivingCharacterRow(rows, slot))
                    continue;

                int summaryIndex = FindTeamSummaryIndex(rows, rows.Team[slot]);
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

        internal static int FindTeamSummaryIndex(AiSoASensingRows rows, int team)
        {
            for (int index = 0; index < rows.TeamSummaryCount; index++)
            {
                if (rows.TeamSummaries[index].Team == team)
                    return index;
            }

            return -1;
        }

        internal static void GetSameTeamSummaryExcludingSelf(
            AiSoASensingRows rows,
            int selfSlot,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            otherCount = 0;
            otherMinHp = int.MaxValue;
            int summaryIndex = FindTeamSummaryIndex(rows, selfTeam);
            if (summaryIndex < 0)
                return;

            AiSensingTeamSummary summary = rows.TeamSummaries[summaryIndex];
            otherCount = summary.Count;
            if (!IsLivingCharacterRow(rows, selfSlot))
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

            otherMinHp = rows.Hp[selfSlot] == summary.MinHp &&
                         summary.MinCount == 1
                ? summary.SecondMinHp
                : summary.MinHp;
        }

        internal static bool TryCaptureRow(
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
                    slot >= 20 && SimulationAiInputModule.IsSpecialScanObjectId(objectId);
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
            rows.HitJ[slot] = CaptureCurrentFrameHitJ(entity, runtime.Frame);
            rows.LinkState[slot] = runtime.LinkState;
            rows.KillCount[slot] = runtime.KillCount;
            rows.CachedTargetSlot[slot] = runtime.Unk360;
            rows.CoordinateTargetX[slot] = runtime.Unk3FC;
            rows.Vx[slot] = runtime.Vx;
            rows.Facing[slot] = runtime.Dir == "left" ? 1 : 0;
            rows.TargetSlot[slot] = runtime.TargetSlotIndex;
            rows.HitStop[slot] = runtime.HitStop;
            rows.BoundaryFlags[slot] = CaptureBoundaryFlags(runtime);
            return true;
        }

        internal static int CaptureCurrentFrameHitJ(
            LF2Entity entity,
            int currentFrame)
        {
            return entity?.GetFrameDataById(currentFrame)?.hit_j ?? 0;
        }

        internal static int CaptureBoundaryFlags(NTSDEntityRuntime runtime)
        {
            return (runtime.ZBoundNegative ? 1 : 0) |
                   (runtime.ZBoundPositive ? 1 << 1 : 0) |
                   (runtime.XBoundNegative ? 1 << 2 : 0) |
                   (runtime.XBoundPositive ? 1 << 3 : 0);
        }

        internal static bool IsGroundRoleMember(AiSoASensingRows rows, int slot)
        {
            if (slot < 0 || slot >= rows.Capacity || !rows.Included[slot])
                return false;

            int state = rows.State[slot];
            return rows.Hp[slot] > 0 &&
                   state != 14 &&
                   Math.Abs(rows.Y[slot]) <= 2 &&
                   (rows.DataObjectType[slot] == 0 || state == 3000);
        }

        internal static bool IsAirRoleMember(AiSoASensingRows rows, int slot)
        {
            return slot >= 0 &&
                   slot < rows.Capacity &&
                   rows.Included[slot] &&
                   rows.Hp[slot] > 0 &&
                   (rows.State[slot] == 14 || Math.Abs(rows.Y[slot]) > 2);
        }

        internal static bool IsLivingCharacterRow(AiSoASensingRows rows, int slot)
        {
            return slot >= 0 &&
                   slot < rows.Capacity &&
                   rows.Included[slot] &&
                   rows.DataObjectType[slot] == 0 &&
                   rows.Hp[slot] > 0;
        }

        private static int BuildRoleTeamSpans(
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

        private static void SortRoleSlotsByTeamThenXThenSlot(
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
                    while (CompareRoleSlots(rows, slots[lower], pivotSlot) < 0)
                        lower++;
                    while (CompareRoleSlots(rows, slots[upper], pivotSlot) > 0)
                        upper--;

                    if (lower > upper)
                        continue;

                    int swap = slots[lower];
                    slots[lower++] = slots[upper];
                    slots[upper--] = swap;
                }

                if (upper - left < right - lower)
                {
                    if (left < upper)
                        SortRoleSlotsByTeamThenXThenSlot(rows, slots, left, upper);
                    left = lower;
                }
                else
                {
                    if (lower < right)
                        SortRoleSlotsByTeamThenXThenSlot(rows, slots, lower, right);
                    right = upper;
                }
            }
        }

        private static int CompareRoleSlots(
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
    }
}
