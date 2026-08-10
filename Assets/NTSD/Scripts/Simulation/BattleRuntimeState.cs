using System;
using System.Collections.Generic;
using NTSD.App;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// 对齐 C++ GameWorld 的战斗配置快照。
    /// 这里只保存 battle runtime 需要长期持有的配置真相，不混 UI 光标或场景对象引用。
    /// </summary>
    [Serializable]
    public sealed class BattleMatchRuntimeState
    {
        public int LocalGameModeId;
        public int BattleGameModeId;
        public int BackgroundId = -1;
        public int Difficulty = 2;
        public int Seed;
        public bool PpMode = true;

        public void Reset()
        {
            LocalGameModeId = 0;
            BattleGameModeId = 0;
            BackgroundId = -1;
            Difficulty = 2;
            Seed = 0;
            PpMode = true;
        }
    }

    /// <summary>
    /// 对齐 C++ GameWorld 里的 stage / boundary 运行态。
    /// Unity 场景对象只是来源；真正运行时以这里的快照为准。
    /// </summary>
    [Serializable]
    public sealed class BattleStageRuntimeState
    {
        public int BaseStageWidthPx = 800;
        public int StageWidthPx = 800;
        public int ZMin = 180;
        public int ZMax = 350;
        public int PerspectiveNear;
        public int PerspectiveFar;
        public int BoundLeft;
        public int BoundRight = 800;
        public int XMaxOverride;
        public int CameraMaxOverride;

        public void Reset()
        {
            BaseStageWidthPx = 800;
            StageWidthPx = 800;
            ZMin = 180;
            ZMax = 350;
            PerspectiveNear = 0;
            PerspectiveFar = 0;
            BoundLeft = 0;
            BoundRight = 800;
            XMaxOverride = 0;
            CameraMaxOverride = 0;
        }

        public void SetSceneSnapshot(int stageWidthPx, int zMin, int zMax, int perspectiveNear, int perspectiveFar)
        {
            BaseStageWidthPx = Mathf.Max(stageWidthPx, 1);
            ZMin = zMin;
            ZMax = Mathf.Max(zMax, zMin + 1);
            PerspectiveNear = perspectiveNear;
            PerspectiveFar = perspectiveFar;
            RebuildActiveStageBounds();
        }

        public void ApplyPhaseBound(int bound)
        {
            if (bound > 0)
            {
                XMaxOverride = Mathf.Max(bound, 1);
                CameraMaxOverride = XMaxOverride - 794;
            }
            else
            {
                XMaxOverride = 0;
                CameraMaxOverride = 0;
            }

            RebuildActiveStageBounds();
        }

        public void ClearPhaseBound()
        {
            XMaxOverride = 0;
            CameraMaxOverride = 0;
            RebuildActiveStageBounds();
        }

        private void RebuildActiveStageBounds()
        {
            StageWidthPx = XMaxOverride > 0
                ? Mathf.Max(XMaxOverride, 1)
                : Mathf.Max(BaseStageWidthPx, 1);
        }
    }

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
    public sealed class BattleStageProgressionState
    {
        public int StageSeriesIdx;
        public int WaveIdx = -1;
        public int Round;
        public int RoundMax;

        public void Reset()
        {
            StageSeriesIdx = 0;
            WaveIdx = -1;
            Round = 0;
            RoundMax = 0;
        }
    }

    /// <summary>
    /// 对齐 C++ battle slot / reserve 前置编排信息。
    /// 当前先落主 slot 信息；reserve/result 细节后续继续迁移到这里。
    /// </summary>
    [Serializable]
    public sealed class BattleSlotRuntimeState
    {
        public bool Active;
        public bool IsHuman;
        public int CharacterId = -1;
        public int Team;
        public int InputId;
        public int AiId = -1;
        public int RuntimeSlotIndex = -1;
        public int StableId = -1;

        public void Reset()
        {
            Active = false;
            IsHuman = false;
            CharacterId = -1;
            Team = 0;
            InputId = 0;
            AiId = -1;
            RuntimeSlotIndex = -1;
            StableId = -1;
        }
    }

    [Serializable]
    public sealed class BattleRosterRuntimeState
    {
        public BattleSlotRuntimeState[] Slots = CreateSlots();
        public int ActiveSlotCount;

        private static BattleSlotRuntimeState[] CreateSlots()
        {
            var slots = new BattleSlotRuntimeState[8];
            for (int i = 0; i < slots.Length; i++)
                slots[i] = new BattleSlotRuntimeState();
            return slots;
        }

        public void Reset()
        {
            if (Slots == null || Slots.Length != 8)
                Slots = CreateSlots();

            for (int i = 0; i < Slots.Length; i++)
                Slots[i].Reset();

            ActiveSlotCount = 0;
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            Reset();
            if (config?.players == null)
                return;

            int slotCount = Math.Min(config.players.Count, Slots.Length);
            for (int i = 0; i < slotCount; i++)
            {
                PlayerSlotConfig player = config.players[i];
                if (player == null || !player.use)
                    continue;

                BattleSlotRuntimeState slot = Slots[i];
                slot.Active = true;
                slot.IsHuman = player.isHuman;
                slot.CharacterId = player.characterId;
                slot.Team = ResolveBattleTeam(player.team, i);
                slot.InputId = ResolveInputId(player.inputId, i);
                slot.AiId = player.aiId;
                ActiveSlotCount++;
            }
        }

        public static int ResolveBattleTeam(int configuredTeam, int originalSlotIndex)
        {
            if (configuredTeam == GameConfig.TeamIndependent)
                return 10 + originalSlotIndex;

            return configuredTeam > 0 ? configuredTeam : originalSlotIndex + 1;
        }

        public static int ResolveInputId(int configuredInputId, int originalSlotIndex)
        {
            return configuredInputId > 0 ? configuredInputId : originalSlotIndex + 1;
        }
    }

    [Serializable]
    public sealed class BattleSlotLabelRuntimeState
    {
        public const int SlotCount = 10;
        public const int CharacterCapacity = 12;

        public char[,] BattleSlotLabels = new char[SlotCount, CharacterCapacity];
        public int[] BattleSlotLabelState = new int[SlotCount];

        public void Reset()
        {
            Array.Clear(BattleSlotLabels, 0, BattleSlotLabels.Length);
            Array.Clear(BattleSlotLabelState, 0, BattleSlotLabelState.Length);
        }

        public void ApplyBootstrapFromMatchConfig(MatchConfig config)
        {
            Reset();
            if (config?.players == null)
                return;

            int slotCount = Math.Min(config.players.Count, 4);
            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                PlayerSlotConfig player = config.players[slotIndex];
                if (player == null || !player.use)
                    continue;

                BattleSlotLabels[slotIndex, 0] = (char)('1' + slotIndex);
                BattleSlotLabelState[slotIndex] = slotIndex + 1;
            }
        }
    }

    /// <summary>
    /// 对齐 C++ GameWorld / battle globals 的流程态。
    /// 这里只收全局 tick / gate / route 标记，不混表现层字段。
    /// </summary>
    [Serializable]
    public sealed class BattleFlowRuntimeState
    {
        public int CurrentTickIndex;
        public int SparkRenderFrame;
        public int AiPhaseGate;
        public int InputPhase;
        public int FrameMod12;
        public int FrameToggle;
        public int AiDifficulty;
        public int AiRand3;
        public int AiRand5;
        public int AiRand15;
        public int AiRand20;
        public int AiMoveMode;
        public int AiStageTargetX;
        public int BattleExitCountdown;
        public int RouteOutRequest;
        public int Mode2Request;
        public int BattleStepMode;
        public int BattleStepGate;
        public int DjaGuardGlobal44F224;
        public bool HumanInputPolledExternally;
        public bool NeedClearInput;

        public void Reset()
        {
            CurrentTickIndex = 0;
            SparkRenderFrame = 0;
            AiPhaseGate = 0;
            InputPhase = 0;
            FrameMod12 = 0;
            FrameToggle = 0;
            AiDifficulty = 0;
            AiRand3 = 0;
            AiRand5 = 0;
            AiRand15 = 0;
            AiRand20 = 0;
            AiMoveMode = 0;
            AiStageTargetX = 0;
            BattleExitCountdown = 0;
            RouteOutRequest = 0;
            Mode2Request = 0;
            BattleStepMode = 0;
            BattleStepGate = 0;
            DjaGuardGlobal44F224 = 0;
            HumanInputPolledExternally = false;
            NeedClearInput = false;
        }
    }

    [Serializable]
    public sealed class BattleResultsRuntimeState
    {
        public int Phase;
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
            Phase = 0;
            Timer = 0;
            Winner = -1;
            HadBoth = false;
            BattleEndPhase = 0;
            PendingWinner = -2;
            TeamCount = 0;
            EnsureTeamIds();
            TeamIds[0] = -1;
            TeamIds[1] = -1;
            PendingHostAction = 0;
        }

        public void ActivateSummary(int winner, int teamCount, int team0, int team1)
        {
            Phase = 200;
            Timer = 0;
            Winner = winner;
            TeamCount = teamCount;
            EnsureTeamIds();
            TeamIds[0] = team0;
            TeamIds[1] = team1;
        }

        public void EnsureTeamIds()
        {
            if (TeamIds == null || TeamIds.Length != 2)
                TeamIds = new[] { -1, -1 };
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
        public List<BattleStageCampaignData> StageCampaigns = new List<BattleStageCampaignData>();
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

        public void Reset()
        {
            Match?.Reset();
            Stage?.Reset();
            StageProgression?.Reset();
            StageProgressionValid = StageCampaigns != null && StageCampaigns.Count > 0;
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
    }
}
