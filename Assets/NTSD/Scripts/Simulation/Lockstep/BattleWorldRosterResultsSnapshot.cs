using System;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    public readonly struct BattleRosterSlotSnapshot
    {
        internal BattleRosterSlotSnapshot(BattleSlotRuntimeState state)
        {
            Active = state.Active;
            IsHuman = state.IsHuman;
            CharacterId = state.CharacterId;
            Team = state.Team;
            InputId = state.InputId;
            AiId = state.AiId;
            RuntimeSlotIndex = state.RuntimeSlotIndex;
            StableId = state.StableId;
        }

        public bool Active { get; }
        public bool IsHuman { get; }
        public int CharacterId { get; }
        public int Team { get; }
        public int InputId { get; }
        public int AiId { get; }
        public int RuntimeSlotIndex { get; }
        public int StableId { get; }
    }

    /// <summary>
    /// Reusable fixed-capacity storage for roster, result, slot-label and battle
    /// statistic state. The caller owns the buffer and decides when it may be
    /// overwritten. This remains an incremental U7 capture product; it is not a
    /// complete battle snapshot and therefore intentionally exposes no restore API.
    /// </summary>
    public sealed class BattleWorldRosterResultsSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;
        public const int RosterSlotCount = 8;
        public const int ResultSideCount = 2;
        public const int ResultColumnCount = 11;
        public const int BattleStatCount = 3;

        private readonly BattleRosterSlotSnapshot[] rosterSlots =
            new BattleRosterSlotSnapshot[RosterSlotCount];
        private readonly int[] resultTeamIds = new int[ResultSideCount];
        private readonly int[] resultMultiplier = new int[ResultSideCount];
        private readonly int[] resultSelectedTroop = new int[ResultSideCount];
        private readonly int[] resultSelectedIcon = new int[ResultSideCount];
        private readonly int[] resultTableTop = new int[ResultSideCount];
        private readonly int[] resultTableBottom = new int[ResultSideCount];
        private readonly int[] resultRow1Values =
            new int[ResultSideCount * ResultColumnCount];
        private readonly int[] resultRow2Values =
            new int[ResultSideCount * ResultColumnCount];
        private readonly int[] resultCommittedTotal =
            new int[ResultSideCount * ResultColumnCount];
        private readonly int[] resultCommittedHp =
            new int[ResultSideCount * ResultColumnCount];
        private readonly int[] resultBackupRow1Values =
            new int[ResultSideCount * ResultColumnCount];
        private readonly int[] resultBackupRow2Values =
            new int[ResultSideCount * ResultColumnCount];
        private readonly char[] slotLabels = new char[
            BattleSlotLabelRuntimeState.SlotCount *
            BattleSlotLabelRuntimeState.CharacterCapacity];
        private readonly int[] slotLabelStates =
            new int[BattleSlotLabelRuntimeState.SlotCount];
        private readonly int[] killStats = new int[BattleStatCount];
        private readonly int[] damageStats = new int[BattleStatCount];
        private readonly int[] reserveCommittedTotal =
            new int[ResultSideCount * ResultColumnCount];
        private readonly int[] reserveCommittedHp =
            new int[ResultSideCount * ResultColumnCount];

        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }
        public int ActiveRosterSlotCount { get; private set; }
        public int ResultsPhase { get; private set; }
        public int ResultsCursor { get; private set; }
        public int ResultsSettingsCursor { get; private set; }
        public int ResultsTableCursor { get; private set; }
        public int ResultsTableSide { get; private set; }
        public int ResultsSubcursor { get; private set; }
        public int ResultsTableSavedTop { get; private set; }
        public int ResultsTableSavedBottom { get; private set; }
        public int ResultsTimer { get; private set; }
        public int ResultsWinner { get; private set; }
        public bool ResultsHadBoth { get; private set; }
        public int ResultsBattleEndPhase { get; private set; }
        public int ResultsPendingWinner { get; private set; }
        public int ResultsTeamCount { get; private set; }
        public int ResultsPendingHostAction { get; private set; }
        public bool ReserveOwnerValid { get; private set; }

        public BattleRosterSlotSnapshot GetRosterSlot(int index)
        {
            ValidateIndex(index, RosterSlotCount, nameof(index));
            return rosterSlots[index];
        }

        public int GetResultTeamId(int side)
        {
            ValidateIndex(side, ResultSideCount, nameof(side));
            return resultTeamIds[side];
        }

        public int GetResultMultiplier(int side)
        {
            ValidateIndex(side, ResultSideCount, nameof(side));
            return resultMultiplier[side];
        }

        public int GetResultSelectedTroop(int side)
        {
            ValidateIndex(side, ResultSideCount, nameof(side));
            return resultSelectedTroop[side];
        }

        public int GetResultSelectedIcon(int side)
        {
            ValidateIndex(side, ResultSideCount, nameof(side));
            return resultSelectedIcon[side];
        }

        public int GetResultTableTop(int side)
        {
            ValidateIndex(side, ResultSideCount, nameof(side));
            return resultTableTop[side];
        }

        public int GetResultTableBottom(int side)
        {
            ValidateIndex(side, ResultSideCount, nameof(side));
            return resultTableBottom[side];
        }

        public int GetResultRow1Value(int side, int column)
        {
            return resultRow1Values[ResultIndex(side, column)];
        }

        public int GetResultRow2Value(int side, int column)
        {
            return resultRow2Values[ResultIndex(side, column)];
        }

        public int GetResultCommittedTotal(int side, int column)
        {
            return resultCommittedTotal[ResultIndex(side, column)];
        }

        public int GetResultCommittedHp(int side, int column)
        {
            return resultCommittedHp[ResultIndex(side, column)];
        }

        public int GetResultBackupRow1Value(int side, int column)
        {
            return resultBackupRow1Values[ResultIndex(side, column)];
        }

        public int GetResultBackupRow2Value(int side, int column)
        {
            return resultBackupRow2Values[ResultIndex(side, column)];
        }

        public char GetSlotLabel(int slot, int characterIndex)
        {
            ValidateIndex(slot, BattleSlotLabelRuntimeState.SlotCount, nameof(slot));
            ValidateIndex(
                characterIndex,
                BattleSlotLabelRuntimeState.CharacterCapacity,
                nameof(characterIndex));
            return slotLabels[
                slot * BattleSlotLabelRuntimeState.CharacterCapacity +
                characterIndex];
        }

        public int GetSlotLabelState(int slot)
        {
            ValidateIndex(slot, BattleSlotLabelRuntimeState.SlotCount, nameof(slot));
            return slotLabelStates[slot];
        }

        public int GetKillStat(int index)
        {
            ValidateIndex(index, BattleStatCount, nameof(index));
            return killStats[index];
        }

        public int GetDamageStat(int index)
        {
            ValidateIndex(index, BattleStatCount, nameof(index));
            return damageStats[index];
        }

        public int GetReserveCommittedTotal(int side, int column)
        {
            return reserveCommittedTotal[ResultIndex(side, column)];
        }

        public int GetReserveCommittedHp(int side, int column)
        {
            return reserveCommittedHp[ResultIndex(side, column)];
        }

        internal bool TryCapture(
            BattleRuntimeState runtime,
            LockstepSessionIdentity identity,
            int tick)
        {
            if (!HasCanonicalFixedBuffers(runtime))
            {
                return false;
            }

            BattleRosterRuntimeState roster = runtime.Roster;
            BattleResultsRuntimeState results = runtime.Results;
            BattleSlotLabelRuntimeState labels = runtime.SlotLabels;

            for (int index = 0; index < RosterSlotCount; index++)
            {
                rosterSlots[index] = new BattleRosterSlotSnapshot(roster.Slots[index]);
            }

            CopyArray(results.TeamIds, resultTeamIds);
            CopyArray(results.ResultMultiplier, resultMultiplier);
            CopyArray(results.ResultSelectedTroop, resultSelectedTroop);
            CopyArray(results.ResultSelectedIcon, resultSelectedIcon);
            CopyArray(results.ResultTableTop, resultTableTop);
            CopyArray(results.ResultTableBottom, resultTableBottom);
            CopyMatrix(results.ResultRow1Values, resultRow1Values);
            CopyMatrix(results.ResultRow2Values, resultRow2Values);
            CopyMatrix(results.ResultCommittedTotal, resultCommittedTotal);
            CopyMatrix(results.ResultCommittedHp, resultCommittedHp);
            CopyMatrix(results.ResultBackupRow1Values, resultBackupRow1Values);
            CopyMatrix(results.ResultBackupRow2Values, resultBackupRow2Values);
            CopyLabels(labels.BattleSlotLabels, slotLabels);
            CopyArray(labels.BattleSlotLabelState, slotLabelStates);
            CopyArray(runtime.KillStats, killStats);
            CopyArray(runtime.DamageStats, damageStats);
            CopyMatrix(runtime.ReserveCommittedTotal, reserveCommittedTotal);
            CopyMatrix(runtime.ReserveCommittedHp, reserveCommittedHp);

            SchemaVersion = CurrentSchemaVersion;
            ProtocolSchemaVersion = identity.SchemaVersion;
            IdentityFingerprint = identity.IdentityFingerprint;
            CapturedTick = tick;
            ActiveRosterSlotCount = roster.ActiveSlotCount;
            ResultsPhase = results.Phase;
            ResultsCursor = results.Cursor;
            ResultsSettingsCursor = results.SettingsCursor;
            ResultsTableCursor = results.TableCursor;
            ResultsTableSide = results.TableSide;
            ResultsSubcursor = results.ResultSubcursor;
            ResultsTableSavedTop = results.ResultTableSavedTop;
            ResultsTableSavedBottom = results.ResultTableSavedBottom;
            ResultsTimer = results.Timer;
            ResultsWinner = results.Winner;
            ResultsHadBoth = results.HadBoth;
            ResultsBattleEndPhase = results.BattleEndPhase;
            ResultsPendingWinner = results.PendingWinner;
            ResultsTeamCount = results.TeamCount;
            ResultsPendingHostAction = results.PendingHostAction;
            ReserveOwnerValid = runtime.ReserveOwnerValid;
            return true;
        }

        private static bool HasCanonicalFixedBuffers(BattleRuntimeState runtime)
        {
            if (runtime?.Roster?.Slots == null ||
                runtime.Roster.Slots.Length != RosterSlotCount ||
                runtime.Results == null ||
                runtime.SlotLabels == null)
            {
                return false;
            }

            for (int index = 0; index < RosterSlotCount; index++)
            {
                if (runtime.Roster.Slots[index] == null)
                {
                    return false;
                }
            }

            BattleResultsRuntimeState results = runtime.Results;
            return IsArray(results.TeamIds, ResultSideCount) &&
                   IsArray(results.ResultMultiplier, ResultSideCount) &&
                   IsArray(results.ResultSelectedTroop, ResultSideCount) &&
                   IsArray(results.ResultSelectedIcon, ResultSideCount) &&
                   IsArray(results.ResultTableTop, ResultSideCount) &&
                   IsArray(results.ResultTableBottom, ResultSideCount) &&
                   IsResultMatrix(results.ResultRow1Values) &&
                   IsResultMatrix(results.ResultRow2Values) &&
                   IsResultMatrix(results.ResultCommittedTotal) &&
                   IsResultMatrix(results.ResultCommittedHp) &&
                   IsResultMatrix(results.ResultBackupRow1Values) &&
                   IsResultMatrix(results.ResultBackupRow2Values) &&
                   IsLabelMatrix(runtime.SlotLabels.BattleSlotLabels) &&
                   IsArray(
                       runtime.SlotLabels.BattleSlotLabelState,
                       BattleSlotLabelRuntimeState.SlotCount) &&
                   IsArray(runtime.KillStats, BattleStatCount) &&
                   IsArray(runtime.DamageStats, BattleStatCount) &&
                   IsResultMatrix(runtime.ReserveCommittedTotal) &&
                   IsResultMatrix(runtime.ReserveCommittedHp);
        }

        private static bool IsArray(Array values, int length)
        {
            return values != null && values.Length == length;
        }

        private static bool IsResultMatrix(int[,] values)
        {
            return values != null &&
                   values.GetLength(0) == ResultSideCount &&
                   values.GetLength(1) == ResultColumnCount;
        }

        private static bool IsLabelMatrix(char[,] values)
        {
            return values != null &&
                   values.GetLength(0) == BattleSlotLabelRuntimeState.SlotCount &&
                   values.GetLength(1) ==
                   BattleSlotLabelRuntimeState.CharacterCapacity;
        }

        private static void CopyArray(int[] source, int[] destination)
        {
            for (int index = 0; index < destination.Length; index++)
            {
                destination[index] = source[index];
            }
        }

        private static void CopyMatrix(int[,] source, int[] destination)
        {
            for (int side = 0; side < ResultSideCount; side++)
            {
                for (int column = 0; column < ResultColumnCount; column++)
                {
                    destination[side * ResultColumnCount + column] =
                        source[side, column];
                }
            }
        }

        private static void CopyLabels(char[,] source, char[] destination)
        {
            for (int slot = 0; slot < BattleSlotLabelRuntimeState.SlotCount; slot++)
            {
                for (int characterIndex = 0;
                     characterIndex < BattleSlotLabelRuntimeState.CharacterCapacity;
                     characterIndex++)
                {
                    destination[
                        slot * BattleSlotLabelRuntimeState.CharacterCapacity +
                        characterIndex] = source[slot, characterIndex];
                }
            }
        }

        private static int ResultIndex(int side, int column)
        {
            ValidateIndex(side, ResultSideCount, nameof(side));
            ValidateIndex(column, ResultColumnCount, nameof(column));
            return side * ResultColumnCount + column;
        }

        private static void ValidateIndex(int index, int count, string parameterName)
        {
            if ((uint)index >= (uint)count)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }
        }
    }

    internal sealed class BattleWorldRosterResultsSnapshotModule
    {
        private readonly SimulationWorld world;

        internal BattleWorldRosterResultsSnapshotModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldRosterResultsSnapshotBuffer destination)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            return destination.TryCapture(world.Runtime, identity, tick);
        }
    }
}
