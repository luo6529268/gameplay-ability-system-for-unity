using System;
using System.Runtime.CompilerServices;

namespace NTSD.Simulation.Ecs
{
    /// <summary>
    /// Stages canonical store mutations and publishes their final values at the
    /// post-CharacterInput boundary. This preserves the authority visibility point
    /// while avoiding a full projection copy for unchanged fields.
    /// </summary>
    internal sealed class BattleAiUnifiedRowPublisher
    {
        private const ulong InputHistoryGateBit = 1UL << 0;
        private const ulong XBit = 1UL << 1;
        private const ulong YBit = 1UL << 2;
        private const ulong ZBit = 1UL << 3;
        private const ulong HpBit = 1UL << 4;
        private const ulong Hp3Bit = 1UL << 5;
        private const ulong HpMaxBit = 1UL << 6;
        private const ulong PpBit = 1UL << 7;
        private const ulong TeamBit = 1UL << 8;
        private const ulong StateBit = 1UL << 9;
        private const ulong FrameBit = 1UL << 10;
        private const ulong LinkStateBit = 1UL << 11;
        private const ulong KillCountBit = 1UL << 12;
        private const ulong CachedTargetSlotBit = 1UL << 13;
        private const ulong CoordinateTargetXBit = 1UL << 14;
        private const ulong VxBit = 1UL << 15;
        private const ulong FacingBit = 1UL << 16;
        private const ulong TargetSlotBit = 1UL << 17;
        private const ulong HitStopBit = 1UL << 18;
        private const ulong DecisionBoundaryFlagsBit = 1UL << 19;
        private const ulong HitJBit = 1UL << 20;

        private bool active;
        private ulong epoch;
        private uint stamp;
        private bool[] included;
        private uint[] generations;
        private int[] dataObjectType;
        private bool[] inputHistoryGate;
        private int[] x;
        private int[] y;
        private int[] z;
        private int[] hp;
        private int[] hp3;
        private int[] hpMax;
        private int[] pp;
        private int[] team;
        private int[] state;
        private int[] frame;
        private int[] hitJ;
        private int[] linkState;
        private int[] killCount;
        private int[] cachedTargetSlot;
        private int[] coordinateTargetX;
        private double[] vx;
        private int[] facing;
        private int[] targetSlot;
        private int[] hitStop;
        private int[] rowSensingBoundaryFlags;
        private int[] publishedSensingBoundaryFlags;
        private int[] decisionBoundaryFlags;

        private uint[] pendingStamp;
        private ulong[] pendingMask;
        private bool[] pendingInputHistoryGate;
        private int[] pendingX;
        private int[] pendingY;
        private int[] pendingZ;
        private int[] pendingHp;
        private int[] pendingHp3;
        private int[] pendingHpMax;
        private int[] pendingPp;
        private int[] pendingTeam;
        private int[] pendingState;
        private int[] pendingFrame;
        private int[] pendingHitJ;
        private int[] pendingLinkState;
        private int[] pendingKillCount;
        private int[] pendingCachedTargetSlot;
        private int[] pendingCoordinateTargetX;
        private double[] pendingVx;
        private int[] pendingFacing;
        private int[] pendingTargetSlot;
        private int[] pendingHitStop;
        private int[] pendingDecisionBoundaryFlags;
        private int[] pendingSlots;
        private int pendingSlotCount;

        internal BattleAiUnifiedRowPublisher(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            AllocatePending(capacity);
        }

        internal bool Active => active;
        internal ulong Epoch => active ? epoch : 0;
        internal int PendingSlotCount => active ? pendingSlotCount : 0;

        internal void GrowTo(int capacity)
        {
            if (capacity <= pendingStamp.Length)
                return;

            Array.Resize(ref pendingStamp, capacity);
            Array.Resize(ref pendingMask, capacity);
            Array.Resize(ref pendingInputHistoryGate, capacity);
            Array.Resize(ref pendingX, capacity);
            Array.Resize(ref pendingY, capacity);
            Array.Resize(ref pendingZ, capacity);
            Array.Resize(ref pendingHp, capacity);
            Array.Resize(ref pendingHp3, capacity);
            Array.Resize(ref pendingHpMax, capacity);
            Array.Resize(ref pendingPp, capacity);
            Array.Resize(ref pendingTeam, capacity);
            Array.Resize(ref pendingState, capacity);
            Array.Resize(ref pendingFrame, capacity);
            Array.Resize(ref pendingHitJ, capacity);
            Array.Resize(ref pendingLinkState, capacity);
            Array.Resize(ref pendingKillCount, capacity);
            Array.Resize(ref pendingCachedTargetSlot, capacity);
            Array.Resize(ref pendingCoordinateTargetX, capacity);
            Array.Resize(ref pendingVx, capacity);
            Array.Resize(ref pendingFacing, capacity);
            Array.Resize(ref pendingTargetSlot, capacity);
            Array.Resize(ref pendingHitStop, capacity);
            Array.Resize(ref pendingDecisionBoundaryFlags, capacity);
            Array.Resize(ref pendingSlots, capacity);
        }

        internal void BeginPass(
            ulong occupancyEpoch,
            bool[] includedRows,
            uint[] rowGenerations,
            int[] rowDataObjectType,
            bool[] rowInputHistoryGate,
            int[] rowX,
            int[] rowY,
            int[] rowZ,
            int[] rowHp,
            int[] rowHp3,
            int[] rowHpMax,
            int[] rowPp,
            int[] rowTeam,
            int[] rowState,
            int[] rowFrame,
            int[] rowHitJ,
            int[] rowLinkState,
            int[] rowKillCount,
            int[] rowCachedTargetSlot,
            int[] rowCoordinateTargetX,
            double[] rowVx,
            int[] rowFacing,
            int[] rowTargetSlot,
            int[] rowHitStop,
            int[] rowSensingBoundaryFlags,
            int[] publishedSensingBoundaryFlags,
            int[] publishedDecisionBoundaryFlags)
        {
            int capacity = includedRows?.Length ?? 0;
            if (occupancyEpoch == 0 ||
                capacity == 0 ||
                rowGenerations?.Length != capacity ||
                rowDataObjectType?.Length != capacity ||
                rowInputHistoryGate?.Length != capacity ||
                rowX?.Length != capacity ||
                rowY?.Length != capacity ||
                rowZ?.Length != capacity ||
                rowHp?.Length != capacity ||
                rowHp3?.Length != capacity ||
                rowHpMax?.Length != capacity ||
                rowPp?.Length != capacity ||
                rowTeam?.Length != capacity ||
                rowState?.Length != capacity ||
                rowFrame?.Length != capacity ||
                rowHitJ?.Length != capacity ||
                rowLinkState?.Length != capacity ||
                rowKillCount?.Length != capacity ||
                rowCachedTargetSlot?.Length != capacity ||
                rowCoordinateTargetX?.Length != capacity ||
                rowVx?.Length != capacity ||
                rowFacing?.Length != capacity ||
                rowTargetSlot?.Length != capacity ||
                rowHitStop?.Length != capacity ||
                rowSensingBoundaryFlags?.Length != capacity ||
                publishedSensingBoundaryFlags?.Length != capacity ||
                publishedDecisionBoundaryFlags?.Length != capacity)
            {
                throw new InvalidOperationException(
                    "Unified AI row publisher requires one complete committed row set.");
            }

            GrowTo(capacity);
            stamp++;
            if (stamp == 0)
            {
                Array.Clear(pendingStamp, 0, pendingStamp.Length);
                stamp = 1;
            }
            pendingSlotCount = 0;

            epoch = occupancyEpoch;
            included = includedRows;
            generations = rowGenerations;
            dataObjectType = rowDataObjectType;
            inputHistoryGate = rowInputHistoryGate;
            x = rowX;
            y = rowY;
            z = rowZ;
            hp = rowHp;
            hp3 = rowHp3;
            hpMax = rowHpMax;
            pp = rowPp;
            team = rowTeam;
            state = rowState;
            frame = rowFrame;
            hitJ = rowHitJ;
            linkState = rowLinkState;
            killCount = rowKillCount;
            cachedTargetSlot = rowCachedTargetSlot;
            coordinateTargetX = rowCoordinateTargetX;
            vx = rowVx;
            facing = rowFacing;
            targetSlot = rowTargetSlot;
            hitStop = rowHitStop;
            this.rowSensingBoundaryFlags = rowSensingBoundaryFlags;
            this.publishedSensingBoundaryFlags = publishedSensingBoundaryFlags;
            decisionBoundaryFlags = publishedDecisionBoundaryFlags;
            active = true;
        }

        internal void EndPass()
        {
            active = false;
            epoch = 0;
            pendingSlotCount = 0;
        }

        internal void InvalidateAfterOccupancyChange()
        {
            InvalidateAfterRowMembershipChange();
        }

        internal void InvalidateAfterRowMembershipChange()
        {
            if (!active)
                return;

            EndPass();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int GetPendingSlot(int index)
        {
            if (!active || (uint)index >= (uint)pendingSlotCount)
                return -1;
            return pendingSlots[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishFrameMotion(
            int slot,
            uint generation,
            RuntimeFrameMotionField field,
            int value)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);

            switch (field)
            {
                case RuntimeFrameMotionField.XInt:
                    pendingX[slot] = value;
                    pendingMask[slot] |= XBit;
                    break;
                case RuntimeFrameMotionField.YInt:
                    pendingY[slot] = value;
                    pendingMask[slot] |= YBit;
                    break;
                case RuntimeFrameMotionField.ZInt:
                    pendingZ[slot] = value;
                    pendingMask[slot] |= ZBit;
                    break;
                case RuntimeFrameMotionField.Frame:
                    pendingFrame[slot] = value;
                    pendingMask[slot] |= FrameBit;
                    break;
                case RuntimeFrameMotionField.State:
                    pendingState[slot] = value;
                    pendingMask[slot] |= StateBit;
                    break;
                case RuntimeFrameMotionField.HitStop:
                    pendingHitStop[slot] = value;
                    pendingMask[slot] |= HitStopBit;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishFrameMotion(
            int slot,
            uint generation,
            RuntimeFrameMotionField field,
            double value)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            if (field != RuntimeFrameMotionField.Vx)
                return;
            BeginPending(slot);
            pendingVx[slot] = value;
            pendingMask[slot] |= VxBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishFacing(int slot, uint generation, int value)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);
            pendingFacing[slot] = value;
            pendingMask[slot] |= FacingBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishHitJ(int slot, uint generation, int value)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);
            pendingHitJ[slot] = value;
            pendingMask[slot] |= HitJBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishIntegerPosition(
            int slot,
            uint generation,
            int nextX,
            int nextY,
            int nextZ)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);
            pendingX[slot] = nextX;
            pendingY[slot] = nextY;
            pendingZ[slot] = nextZ;
            pendingMask[slot] |= XBit | YBit | ZBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishRelationLink(
            int slot,
            uint generation,
            RuntimeRelationLinkField field,
            int value)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);

            switch (field)
            {
                case RuntimeRelationLinkField.RelationTeam:
                    pendingTeam[slot] = value;
                    pendingMask[slot] |= TeamBit;
                    break;
                case RuntimeRelationLinkField.LinkState:
                    pendingLinkState[slot] = value;
                    pendingMask[slot] |= LinkStateBit;
                    break;
                case RuntimeRelationLinkField.KillCount:
                    pendingKillCount[slot] = value;
                    pendingMask[slot] |= KillCountBit;
                    break;
                case RuntimeRelationLinkField.TargetSlot:
                    pendingTargetSlot[slot] = value;
                    pendingMask[slot] |= TargetSlotBit;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishVital(
            int slot,
            uint generation,
            RuntimeVitalField field,
            int value)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);

            switch (field)
            {
                case RuntimeVitalField.Hp:
                    pendingHp[slot] = value;
                    pendingMask[slot] |= HpBit;
                    break;
                case RuntimeVitalField.HpBound:
                    pendingHpMax[slot] = value;
                    pendingMask[slot] |= HpMaxBit;
                    break;
                case RuntimeVitalField.Hp3:
                    pendingHp3[slot] = value;
                    pendingMask[slot] |= Hp3Bit;
                    break;
                case RuntimeVitalField.Pp:
                    pendingPp[slot] = value;
                    pendingMask[slot] |= PpBit;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishInputProjection(
            int slot,
            uint generation,
            bool historyGate,
            int cachedTarget,
            int coordinateX)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);
            pendingInputHistoryGate[slot] = historyGate;
            pendingCachedTargetSlot[slot] = cachedTarget;
            pendingCoordinateTargetX[slot] = coordinateX;
            pendingMask[slot] |=
                InputHistoryGateBit | CachedTargetSlotBit | CoordinateTargetXBit;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void PublishDecisionBoundaryFlags(
            int slot,
            uint generation,
            int flags)
        {
            if (!active)
                return;
            ValidateRow(slot, generation);
            BeginPending(slot);
            pendingDecisionBoundaryFlags[slot] = flags;
            pendingMask[slot] |= DecisionBoundaryFlagsBit;
        }

        internal bool TryCommitPending(
            int slot,
            uint generation,
            out bool roleProductsChanged,
            out bool teamProductsChanged)
        {
            roleProductsChanged = false;
            teamProductsChanged = false;
            if (!active || !IsCurrentRow(slot, generation))
                return false;
            if (pendingStamp[slot] != stamp)
                return true;

            ulong mask = pendingMask[slot];
            if (mask == 0)
                return true;
            int oldX = x[slot];
            int oldHp = hp[slot];
            int oldTeam = team[slot];
            bool oldGround = IsGroundRole(slot, y[slot], hp[slot], state[slot]);
            bool oldAir = IsAirRole(y[slot], hp[slot], state[slot]);

            if ((mask & InputHistoryGateBit) != 0)
                inputHistoryGate[slot] = pendingInputHistoryGate[slot];
            if ((mask & XBit) != 0) x[slot] = pendingX[slot];
            if ((mask & YBit) != 0) y[slot] = pendingY[slot];
            if ((mask & ZBit) != 0) z[slot] = pendingZ[slot];
            if ((mask & HpBit) != 0) hp[slot] = pendingHp[slot];
            if ((mask & Hp3Bit) != 0) hp3[slot] = pendingHp3[slot];
            if ((mask & HpMaxBit) != 0) hpMax[slot] = pendingHpMax[slot];
            if ((mask & PpBit) != 0) pp[slot] = pendingPp[slot];
            if ((mask & TeamBit) != 0) team[slot] = pendingTeam[slot];
            if ((mask & StateBit) != 0) state[slot] = pendingState[slot];
            if ((mask & FrameBit) != 0) frame[slot] = pendingFrame[slot];
            if ((mask & HitJBit) != 0) hitJ[slot] = pendingHitJ[slot];
            if ((mask & LinkStateBit) != 0) linkState[slot] = pendingLinkState[slot];
            if ((mask & KillCountBit) != 0) killCount[slot] = pendingKillCount[slot];
            if ((mask & CachedTargetSlotBit) != 0)
                cachedTargetSlot[slot] = pendingCachedTargetSlot[slot];
            if ((mask & CoordinateTargetXBit) != 0)
                coordinateTargetX[slot] = pendingCoordinateTargetX[slot];
            if ((mask & VxBit) != 0) vx[slot] = pendingVx[slot];
            if ((mask & FacingBit) != 0) facing[slot] = pendingFacing[slot];
            if ((mask & TargetSlotBit) != 0) targetSlot[slot] = pendingTargetSlot[slot];
            if ((mask & HitStopBit) != 0) hitStop[slot] = pendingHitStop[slot];
            if ((mask & DecisionBoundaryFlagsBit) != 0)
            {
                int decisionFlags = pendingDecisionBoundaryFlags[slot];
                int sensingFlags = ToSensingBoundaryFlags(decisionFlags);
                rowSensingBoundaryFlags[slot] = sensingFlags;
                publishedSensingBoundaryFlags[slot] = sensingFlags;
                decisionBoundaryFlags[slot] = decisionFlags;
            }

            bool newGround = IsGroundRole(slot, y[slot], hp[slot], state[slot]);
            bool newAir = IsAirRole(y[slot], hp[slot], state[slot]);
            roleProductsChanged = oldX != x[slot] ||
                                  oldTeam != team[slot] ||
                                  oldGround != newGround ||
                                  oldAir != newAir;
            teamProductsChanged = oldTeam != team[slot] || oldHp != hp[slot];
            pendingMask[slot] = 0;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool HasPendingValues(int slot, uint generation)
        {
            return active &&
                   IsCurrentRow(slot, generation) &&
                   pendingStamp[slot] == stamp &&
                   pendingMask[slot] != 0;
        }

        internal bool TryDiscardPending(int slot, uint generation)
        {
            if (!active || !IsCurrentRow(slot, generation))
                return false;

            if (pendingStamp[slot] == stamp)
                pendingMask[slot] = 0;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BeginPending(int slot)
        {
            if (pendingStamp[slot] == stamp)
                return;
            pendingStamp[slot] = stamp;
            pendingMask[slot] = 0;
            pendingSlots[pendingSlotCount++] = slot;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateRow(int slot, uint generation)
        {
            if (!IsCurrentRow(slot, generation))
            {
                throw new InvalidOperationException(
                    "Unified AI row publisher observed a stale slot generation after commit.");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsCurrentRow(int slot, uint generation)
        {
            return generation != 0 &&
                   (uint)slot < (uint)included.Length &&
                   included[slot] &&
                   generations[slot] == generation;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsGroundRole(int slot, int rowY, int rowHp, int rowState)
        {
            return rowHp > 0 &&
                   rowState != 14 &&
                   Abs(rowY) <= 2 &&
                   (dataObjectType[slot] == 0 || rowState == 3000);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsAirRole(int rowY, int rowHp, int rowState)
        {
            return rowHp > 0 && (rowState == 14 || Abs(rowY) > 2);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int ToSensingBoundaryFlags(int decisionFlags)
        {
            return ((decisionFlags & 8) != 0 ? 1 : 0) |
                   ((decisionFlags & 4) != 0 ? 1 << 1 : 0) |
                   ((decisionFlags & 2) != 0 ? 1 << 2 : 0) |
                   ((decisionFlags & 1) != 0 ? 1 << 3 : 0);
        }

        private void AllocatePending(int capacity)
        {
            pendingStamp = new uint[capacity];
            pendingMask = new ulong[capacity];
            pendingInputHistoryGate = new bool[capacity];
            pendingX = new int[capacity];
            pendingY = new int[capacity];
            pendingZ = new int[capacity];
            pendingHp = new int[capacity];
            pendingHp3 = new int[capacity];
            pendingHpMax = new int[capacity];
            pendingPp = new int[capacity];
            pendingTeam = new int[capacity];
            pendingState = new int[capacity];
            pendingFrame = new int[capacity];
            pendingHitJ = new int[capacity];
            pendingLinkState = new int[capacity];
            pendingKillCount = new int[capacity];
            pendingCachedTargetSlot = new int[capacity];
            pendingCoordinateTargetX = new int[capacity];
            pendingVx = new double[capacity];
            pendingFacing = new int[capacity];
            pendingTargetSlot = new int[capacity];
            pendingHitStop = new int[capacity];
            pendingDecisionBoundaryFlags = new int[capacity];
            pendingSlots = new int[capacity];
        }
    }
}
