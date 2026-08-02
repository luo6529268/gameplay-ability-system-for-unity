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

        private sealed class AiSoASensingRows
        {
            internal AiSoASensingRows(int capacity)
            {
                Included = new bool[capacity];
                SpecialScanMember = new bool[capacity];
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
                LinkState = new int[capacity];
                KillCount = new int[capacity];
                CachedTargetSlot = new int[capacity];
                CoordinateTargetX = new int[capacity];
                Vx = new double[capacity];
            }

            internal int Capacity => Included.Length;
            internal readonly bool[] Included;
            internal readonly bool[] SpecialScanMember;
            internal readonly bool[] InputHistoryGate;
            internal readonly uint[] Generation;
            internal readonly int[] Identity;
            internal readonly int[] ObjectId;
            internal readonly int[] DataObjectType;
            internal readonly int[] X;
            internal readonly int[] Y;
            internal readonly int[] Z;
            internal readonly int[] Hp;
            internal readonly int[] Hp3;
            internal readonly int[] HpMax;
            internal readonly int[] Pp;
            internal readonly int[] Team;
            internal readonly int[] State;
            internal readonly int[] Frame;
            internal readonly int[] LinkState;
            internal readonly int[] KillCount;
            internal readonly int[] CachedTargetSlot;
            internal readonly int[] CoordinateTargetX;
            internal readonly double[] Vx;

            internal AiSoASensingRows GrowTo(int capacity)
            {
                var grown = new AiSoASensingRows(capacity);
                int count = Capacity;
                Array.Copy(Included, grown.Included, count);
                Array.Copy(SpecialScanMember, grown.SpecialScanMember, count);
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
                Array.Copy(LinkState, grown.LinkState, count);
                Array.Copy(KillCount, grown.KillCount, count);
                Array.Copy(CachedTargetSlot, grown.CachedTargetSlot, count);
                Array.Copy(CoordinateTargetX, grown.CoordinateTargetX, count);
                Array.Copy(Vx, grown.Vx, count);
                return grown;
            }
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
            public int CachedSelectedSlot;
            public int PostSpecialSelectedSlot;
            public int SpecialBestDist;
            public int SpecialFlags;
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
            if (aiSensingMode == AiSensingMode.SoAAiSensing)
            {
                throw new NotSupportedException(
                    "SoAAiSensing is unavailable in AI sensing shadow v1.");
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
            aiSoASensingComparisonPending = false;
            aiSoASensingPendingMismatchMask = 0;
            aiSoASensingPendingFirstMismatch = default;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowComparisonPublishedForDiagnostics = false;
            aiSoASensingSnapshotEpoch = expectedEpoch;
            Array.Clear(aiSoASensingRows.Included, 0, aiSoASensingRows.Capacity);
            Array.Clear(aiSoASensingRows.SpecialScanMember, 0, aiSoASensingRows.Capacity);

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

            aiSoASensingSnapshotValid = true;
        }

        private bool TryCaptureAiSoASensingRow(
            LF2Entity entity,
            int slot,
            uint generation,
            bool captureSpecialMembership)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            if (runtime == null ||
                generation == 0 ||
                slot < 0 ||
                slot >= aiSoASensingRows.Capacity ||
                runtime.SlotIndex != slot)
            {
                return false;
            }

            AiSoASensingRows rows = aiSoASensingRows;
            rows.Included[slot] = true;
            if (captureSpecialMembership)
                rows.SpecialScanMember[slot] = slot >= 20 && IsAiSpecialScanObjectId(entity.ObjectId);
            rows.InputHistoryGate[slot] = runtime.HasInputHistoryGate();
            rows.Generation[slot] = generation;
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
            return true;
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
            if (slot < 0 ||
                slot >= aiSoASensingRows.Capacity ||
                !aiSoASensingRows.Included[slot] ||
                !TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle handle) ||
                handle.Generation != aiSoASensingRows.Generation[slot] ||
                entity.Runtime.StableId != aiSoASensingRows.Identity[slot] ||
                !TryCaptureAiSoASensingRow(entity, slot, handle.Generation, false))
            {
                InvalidateAiSoASensingShadowPass();
            }
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
            bool succeeded = TryRunAiSoASensingShadowQuery(
                selfSlot,
                InputPhase,
                rngStateBefore,
                ForceFullAiSpecialScanForDiagnostics,
                out aiSoASensingExpected);
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

            aiSoASensingExpected.TickIndex = tickIndex;
            aiSoASensingExpected.SelfGeneration = selfHandle.Generation;
            aiSoASensingExpected.SelfIdentity = self.Runtime.StableId;
            AiSoASensingShadowQueryCountForDiagnostics++;
            aiSoASensingComparisonPending = true;
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

        private void CompareAiSoASensingCachedSelection(
            LF2Entity self,
            int tickIndex,
            int selectedSlot)
        {
            if (!IsAiSoASensingComparisonCurrent(self, tickIndex) ||
                selectedSlot == aiSoASensingExpected.CachedSelectedSlot)
            {
                return;
            }

            RecordAiSoASensingPendingMismatch(
                AiSoASensingShadowMismatchKind.CachedSelection,
                aiSoASensingExpected.SelfSlot,
                aiSoASensingExpected.CachedSelectedSlot,
                selectedSlot,
                0,
                0,
                0,
                0);
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
            AiSoASensingRows rows = aiSoASensingRows;
            if (rows == null ||
                selfSlot < 0 ||
                selfSlot >= rows.Capacity ||
                !rows.Included[selfSlot] ||
                rows.Hp[selfSlot] <= 0 ||
                rows.CoordinateTargetX[selfSlot] > -1000)
            {
                return false;
            }

            result.SelfSlot = selfSlot;
            int selectedSlot = FindNearestGroundAiSoASensingSlot(
                rows,
                selfSlot,
                inputPhase,
                out int bestDist);
            bool sameZLane = selectedSlot >= 0 &&
                             Abs(rows.Z[selectedSlot] - rows.Z[selfSlot]) < 15;
            if (rows.State[selfSlot] != 9)
            {
                int bestAirDist = 10000;
                int airSelectedSlot = -1;
                for (int slot = 0; slot < rows.Capacity; slot++)
                {
                    if (!IsAirAiSoATargetCandidate(rows, selfSlot, slot, inputPhase))
                        continue;

                    int dist = AiSoADistance(rows, selfSlot, slot);
                    if (!IsAiSoABetterCandidate(dist, slot, bestAirDist, airSelectedSlot) ||
                        Abs(rows.Z[slot] - rows.Z[selfSlot]) >= 40 ||
                        Abs(rows.X[slot] - rows.X[selfSlot]) >= 250)
                    {
                        continue;
                    }

                    bestAirDist = dist;
                    airSelectedSlot = slot;
                }

                if (airSelectedSlot >= 0)
                    selectedSlot = airSelectedSlot;
            }

            result.InitialSelectedSlot = selectedSlot;
            result.InitialBestDist = bestDist;
            result.InitialSameZLane = sameZLane;

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

                ScanAiSoASameTeamSummaryExcludingSelf(
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

            for (int slot = 20; slot < rows.Capacity; slot++)
            {
                if (!rows.Included[slot] ||
                    (!forceFullSpecialScan && !rows.SpecialScanMember[slot]))
                {
                    continue;
                }

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
                    if (dist < 2 * bestDist &&
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
            result.PostSpecialSelectedSlot = selectedSlot;
            result.SpecialBestDist = specialBestDist;
            result.SpecialFlags = PackAiSoASpecialFlags(
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

        private static int FindNearestGroundAiSoASensingSlot(
            AiSoASensingRows rows,
            int selfSlot,
            int inputPhase,
            out int bestDist)
        {
            int selectedSlot = -1;
            bestDist = 10000;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (!IsGroundAiSoATargetCandidate(rows, selfSlot, slot, inputPhase))
                    continue;

                int distance = AiSoADistance(rows, selfSlot, slot);
                if (IsAiSoABetterCandidate(distance, slot, bestDist, selectedSlot))
                {
                    bestDist = distance;
                    selectedSlot = slot;
                }
            }
            return selectedSlot;
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

        private static void ScanAiSoASameTeamSummaryExcludingSelf(
            AiSoASensingRows rows,
            int selfSlot,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            otherCount = 0;
            otherMinHp = int.MaxValue;
            for (int slot = 0; slot < rows.Capacity; slot++)
            {
                if (slot == selfSlot ||
                    !IsLivingCharacterAiSoARow(rows, slot) ||
                    rows.Team[slot] != selfTeam)
                {
                    continue;
                }

                if (rows.Hp[slot] < otherMinHp)
                    otherMinHp = rows.Hp[slot];
                otherCount++;
            }
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
