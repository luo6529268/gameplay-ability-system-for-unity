using System;
using System.Collections.Generic;
using NTSD.App;

namespace NTSD.Simulation
{
    [Serializable]
    public sealed class BattleStageSpawnData
    {
        public int Id = -1;
        public int Act;
        public int Hp;
        public int Times = 1;
        public int X;
        public int Y;
        public double Ratio;
        public int Join;

        public BattleStageSpawnValue ToValue()
        {
            return new BattleStageSpawnValue(
                Id,
                Act,
                Hp,
                Times,
                X,
                Y,
                Ratio,
                Join);
        }
    }

    [Serializable]
    public sealed class BattleStagePhaseData
    {
        public int Bound;
        public List<BattleStageSpawnData> Spawns = new List<BattleStageSpawnData>();
    }

    [Serializable]
    public sealed class BattleStageCampaignData
    {
        public int Id = -1;
        public string Comment = string.Empty;
        public List<BattleStagePhaseData> Phases = new List<BattleStagePhaseData>();
    }

    [Serializable]
    public sealed class BattleResultsRuntimeState
    {
        public const int HostActionNone = 0;
        public const int HostActionRematch = 1;
        public const int HostActionBootstrapDirect = 2;

        private static readonly int[] InitialRow1 =
        {
            30, 30, 10, 10, 10, 7, 7, 3, 3, 15, 15,
            30, 30, 10, 10, 10, 7, 7, 3, 3, 15, 15,
        };

        private static readonly int[] InitialRow2 =
        {
            10, 10, 5, 5, 5, 3, 3, 1, 1, 3, 3,
            10, 10, 5, 5, 5, 3, 3, 1, 1, 3, 3,
        };

        private static readonly int[] SourceTotal =
        {
            20, 20, 8, 8, 8, 2, 2, 3, 2, 10, 10,
            42, 42, 2, 8, 0, 0, 0, 3, 0, 10, 10,
            0, 20, 12, 12, 0, 0, 8, 3, 0, 10, 10,
            20, 0, 4, 0, 10, 8, 0, 3, 6, 10, 10,
            0, 0, 0, 0, 0, 9, 8, 3, 8, 10, 10,
        };

        private static readonly int[] SourceHp =
        {
            7, 7, 4, 4, 4, 1, 1, 1, 1, 3, 3,
            20, 20, 1, 4, 0, 0, 0, 1, 0, 3, 3,
            0, 10, 6, 6, 0, 0, 4, 1, 1, 3, 3,
            10, 0, 2, 0, 5, 4, 0, 1, 3, 3, 3,
            0, 0, 0, 0, 0, 6, 4, 1, 4, 3, 3,
        };

        public int Phase;
        public int Cursor = 6;
        public int SettingsCursor = 2;
        public int TableCursor = 10;
        public int TableSide;
        public int ResultSubcursor = 2;
        public int[] ResultMultiplier = { 100, 100 };
        public int[,] ResultRow1Values = new int[2, 11];
        public int[,] ResultRow2Values = new int[2, 11];
        public int[,] ResultCommittedTotal = new int[2, 11];
        public int[,] ResultCommittedHp = new int[2, 11];
        public int[] ResultSelectedTroop = { -1, -1 };
        public int[] ResultSelectedIcon = { -1, -1 };
        public int[] ResultTableTop = { -1, -1 };
        public int[] ResultTableBottom = { -1, -1 };
        public int ResultTableSavedTop = -1;
        public int ResultTableSavedBottom = -1;
        public int[,] ResultBackupRow1Values = new int[2, 11];
        public int[,] ResultBackupRow2Values = new int[2, 11];
        public int Timer;
        public int Winner = -1;
        public bool HadBoth;
        public int BattleEndPhase;
        public int PendingWinner = -2;
        public int TeamCount;
        public int[] TeamIds = { -1, -1 };
        public int PendingHostAction;

        public bool IsActive => Phase >= 200;

        public void Reset()
        {
            EnsureBuffers();
            Phase = 0;
            Cursor = 6;
            SettingsCursor = 2;
            TableCursor = 10;
            TableSide = 0;
            Timer = 0;
            Winner = -1;
            PendingHostAction = HostActionNone;
            ResetResultTableState();
            ResetLiveGuard();
        }

        public void PrepareForBattleRematch()
        {
            EnsureBuffers();
            Phase = 0;
            Cursor = 6;
            SettingsCursor = 2;
            TableCursor = 10;
            TableSide = 0;
            ResultSubcursor = 2;
            Timer = 0;
            Winner = -1;
            PendingHostAction = HostActionNone;
            ResetLiveGuard();
        }

        public void ResetLiveGuard()
        {
            EnsureTeamIds();
            HadBoth = false;
            BattleEndPhase = 0;
            PendingWinner = -2;
            TeamCount = 0;
            TeamIds[0] = -1;
            TeamIds[1] = -1;
        }

        public void ResetResultTableState()
        {
            EnsureBuffers();
            ResultSubcursor = 2;
            ResultMultiplier[0] = 100;
            ResultMultiplier[1] = 100;
            ResultSelectedTroop[0] = -1;
            ResultSelectedTroop[1] = -1;
            ResultSelectedIcon[0] = -1;
            ResultSelectedIcon[1] = -1;
            ResultTableTop[0] = -1;
            ResultTableTop[1] = -1;
            ResultTableBottom[0] = -1;
            ResultTableBottom[1] = -1;
            ResultTableSavedTop = -1;
            ResultTableSavedBottom = -1;
            Array.Clear(ResultCommittedTotal, 0, ResultCommittedTotal.Length);
            Array.Clear(ResultCommittedHp, 0, ResultCommittedHp.Length);
            Array.Clear(ResultBackupRow1Values, 0, ResultBackupRow1Values.Length);
            Array.Clear(ResultBackupRow2Values, 0, ResultBackupRow2Values.Length);
            for (int side = 0; side < 2; side++)
            {
                for (int col = 0; col < 11; col++)
                {
                    int index = side * 11 + col;
                    ResultRow1Values[side, col] = InitialRow1[index];
                    ResultRow2Values[side, col] = InitialRow2[index];
                }
            }
        }

        public void CommitResultTableValues()
        {
            for (int side = 0; side < 2; side++)
            {
                for (int col = 0; col < 11; col++)
                {
                    int hp = ResultRow1Values[side, col];
                    ResultCommittedTotal[side, col] = hp > 0
                        ? hp + ResultRow2Values[side, col]
                        : 0;
                    ResultCommittedHp[side, col] = hp;
                }
            }
        }

        public int FallDamageDivForTeam(int team)
        {
            if (team == 0)
                return 1;
            if (team == 1 || team == 2)
                return ResultMultiplier[team - 1];
            return 0;
        }

        public void ApplyPhase210TableAttack(int editCursor)
        {
            int side = TableSide;
            if (side < 0 || side >= 2 || editCursor < 0 || editCursor >= 10)
                return;

            if (editCursor >= 5)
            {
                if (ResultTableTop[side] == -1)
                    ResultTableTop[side] = 0;
                int top = ResultTableTop[side];
                int bottom = editCursor - 5;
                ResultSelectedIcon[side] = top;
                ResultSelectedTroop[side] = editCursor - 4;
                ResultTableBottom[side] = bottom;
                RecomputePhase210Rows(side, bottom, top + 1);
                return;
            }

            if (editCursor > 0 && editCursor < 4)
            {
                ResultTableTop[side] = editCursor - 1;
                if (ResultTableBottom[side] == -1)
                    ResultTableBottom[side] = 0;
                int bottom = ResultTableBottom[side];
                RecomputePhase210Rows(side, bottom, editCursor);
                ResultSelectedTroop[side] = bottom + 1;
                ResultSelectedIcon[side] = editCursor - 1;
            }
            else if (editCursor == 0)
            {
                ResultTableTop[side] = -1;
                ResultTableBottom[side] = -1;
                for (int col = 0; col < 11; col++)
                {
                    ResultRow1Values[side, col] = 0;
                    ResultRow2Values[side, col] = 0;
                }
                ResultSelectedTroop[side] = 0;
            }
            else if (editCursor == 4)
            {
                ResultTableTop[side] = -1;
                ResultTableBottom[side] = -1;
                for (int col = 0; col < 11; col++)
                {
                    ResultRow1Values[side, col] = 2;
                    ResultRow2Values[side, col] = 30;
                }
                ResultRow1Values[side, 7] = 1;
                ResultRow1Values[side, 9] = 3;
                ResultRow2Values[side, 7] = 15;
                ResultRow1Values[side, 10] = 3;
                ResultSelectedTroop[side] = 6;
            }
        }

        public void SnapshotPhase210Table()
        {
            for (int side = 0; side < 2; side++)
            {
                for (int col = 0; col < 11; col++)
                {
                    ResultBackupRow1Values[side, col] = ResultRow1Values[side, col];
                    ResultBackupRow2Values[side, col] = ResultRow2Values[side, col];
                }
            }
            ResultTableSavedTop = ResultTableTop[TableSide];
            ResultTableSavedBottom = ResultTableBottom[TableSide];
        }

        public void RestorePhase210Table()
        {
            for (int side = 0; side < 2; side++)
            {
                for (int col = 0; col < 11; col++)
                {
                    ResultRow1Values[side, col] = ResultBackupRow1Values[side, col];
                    ResultRow2Values[side, col] = ResultBackupRow2Values[side, col];
                }
            }
            ResultTableTop[TableSide] = ResultTableSavedTop;
            ResultTableBottom[TableSide] = ResultTableSavedBottom;
        }

        public void ActivateSummary(int winner, int teamCount, int team0, int team1)
        {
            Phase = 200;
            Cursor = 6;
            SettingsCursor = 2;
            TableCursor = 10;
            TableSide = 0;
            Timer = 0;
            Winner = winner;
            TeamCount = teamCount;
            EnsureTeamIds();
            TeamIds[0] = team0;
            TeamIds[1] = team1;
            // Alignment contract: CLIENT-CPP-RESULTS-ACTIVATION-RESET-ALIGNMENT-001.
            // C++ resets the Results table before clearing the live guard on scene entry.
            ResetResultTableState();
            ResetLiveGuard();
        }

        public void EnsureTeamIds()
        {
            if (TeamIds == null || TeamIds.Length != 2)
                TeamIds = new[] { -1, -1 };
        }

        private void RecomputePhase210Rows(int side, int sourceRow, int factor)
        {
            if (side < 0 || side >= 2 || sourceRow < 0 || sourceRow >= 5)
                return;

            int sourceBase = sourceRow * 11;
            for (int col = 0; col < 11; col++)
            {
                int sourceIndex = sourceBase + col;
                int total = factor * SourceTotal[sourceIndex] / 3;
                int hp = factor * SourceHp[sourceIndex] / 3;
                if (hp < 1 && total > 0)
                    hp = 1;
                ResultRow1Values[side, col] = hp;
                ResultRow2Values[side, col] = total - hp;
                if (ResultRow2Values[side, col] < 0)
                    ResultRow2Values[side, col] = 0;
            }
        }

        private void EnsureBuffers()
        {
            EnsureTeamIds();
            if (ResultMultiplier == null || ResultMultiplier.Length != 2)
                ResultMultiplier = new[] { 100, 100 };
            if (ResultSelectedTroop == null || ResultSelectedTroop.Length != 2)
                ResultSelectedTroop = new[] { -1, -1 };
            if (ResultSelectedIcon == null || ResultSelectedIcon.Length != 2)
                ResultSelectedIcon = new[] { -1, -1 };
            if (ResultTableTop == null || ResultTableTop.Length != 2)
                ResultTableTop = new[] { -1, -1 };
            if (ResultTableBottom == null || ResultTableBottom.Length != 2)
                ResultTableBottom = new[] { -1, -1 };
            EnsureMatrix(ref ResultRow1Values);
            EnsureMatrix(ref ResultRow2Values);
            EnsureMatrix(ref ResultCommittedTotal);
            EnsureMatrix(ref ResultCommittedHp);
            EnsureMatrix(ref ResultBackupRow1Values);
            EnsureMatrix(ref ResultBackupRow2Values);
        }

        private static void EnsureMatrix(ref int[,] values)
        {
            if (values == null || values.GetLength(0) != 2 || values.GetLength(1) != 11)
                values = new int[2, 11];
        }
    }

    /// <summary>
    /// Unity 侧的战斗唯一运行态根节点。
    /// 让 SimulationWorld 对齐 C++ GameWorld 的“职责中心”，但避免重新长成一个巨型类。
    /// </summary>
    [Serializable]
    public sealed class BattleRuntimeState
    {
        private const int BattleStatSlotCount = 3;

        public BattleMatchRuntimeState Match = new BattleMatchRuntimeState();
        public BattleStageRuntimeState Stage = new BattleStageRuntimeState();
        [NonSerialized]
        public BattleStageCampaignSet StageCampaigns = BattleStageCampaignSet.Empty;
        public BattleStageProgressionState StageProgression = new BattleStageProgressionState();
        public bool StageProgressionValid;
        public int StageSpawnWaveApplied = -1;
        public int StageSpawnWaveDeferredEntryApplied = -1;
        public int StageSpawnRuntimeWave = -1;
        public List<int> StageSpawnRuntimeTargetTotal = new List<int>();
        public List<int> StageSpawnRuntimeEntryCount = new List<int>();
        public List<int> StageSpawnRuntimeSpawnedTotal = new List<int>();
        public List<int[]> StageSpawnRuntimeSlots = new List<int[]>();
        [NonSerialized]
        public StageSpawnRuntimeBufferPool StageSpawnBuffers =
            new StageSpawnRuntimeBufferPool();
        public BattleRosterRuntimeState Roster = new BattleRosterRuntimeState();
        public BattleFlowRuntimeState Flow = new BattleFlowRuntimeState();
        public BattleResultsRuntimeState Results = new BattleResultsRuntimeState();
        public BattleSlotLabelRuntimeState SlotLabels = new BattleSlotLabelRuntimeState();
        public int[] KillStats = new int[BattleStatSlotCount];
        public int[] DamageStats = new int[BattleStatSlotCount];
        public bool ReserveOwnerValid;
        public int[,] ReserveCommittedTotal = new int[2, 11];
        public int[,] ReserveCommittedHp = new int[2, 11];

        public void Reset()
        {
            Match?.Reset();
            Stage?.Reset();
            StageProgression?.Reset();
            StageCampaigns ??= BattleStageCampaignSet.Empty;
            StageProgressionValid = StageCampaigns.Count > 0;
            StageSpawnWaveApplied = -1;
            StageSpawnWaveDeferredEntryApplied = -1;
            StageSpawnRuntimeWave = -1;
            StageSpawnRuntimeTargetTotal?.Clear();
            StageSpawnRuntimeEntryCount?.Clear();
            StageSpawnRuntimeSpawnedTotal?.Clear();
            EnsureStageSpawnBuffers();
            StageSpawnBuffers.Recycle(StageSpawnRuntimeSlots);
            Roster?.Reset();
            Flow?.Reset();
            Results?.Reset();
            SlotLabels?.Reset();
            ResetStatArray(ref KillStats);
            ResetStatArray(ref DamageStats);
            ReserveOwnerValid = false;
            ResetReserveMatrix(ref ReserveCommittedTotal);
            ResetReserveMatrix(ref ReserveCommittedHp);
        }

        public void ApplyBootstrapFromMatchConfig(MatchConfig config)
        {
            SlotLabels?.ApplyBootstrapFromMatchConfig(config);
        }

        public StageSpawnRuntimeBufferPool EnsureStageSpawnBuffers()
        {
            StageSpawnBuffers ??= new StageSpawnRuntimeBufferPool();
            return StageSpawnBuffers;
        }

        private static void ResetStatArray(ref int[] stats)
        {
            if (stats == null || stats.Length != BattleStatSlotCount)
            {
                stats = new int[BattleStatSlotCount];
                return;
            }

            Array.Clear(stats, 0, stats.Length);
        }

        private static void ResetReserveMatrix(ref int[,] values)
        {
            if (values == null || values.GetLength(0) != 2 || values.GetLength(1) != 11)
            {
                values = new int[2, 11];
                return;
            }

            Array.Clear(values, 0, values.Length);
        }
    }
}
