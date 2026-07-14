using System;
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

        public void Reset()
        {
            LocalGameModeId = 0;
            BattleGameModeId = 0;
            BackgroundId = -1;
            Difficulty = 2;
            Seed = 0;
        }
    }

    /// <summary>
    /// 对齐 C++ GameWorld 里的 stage / boundary 运行态。
    /// Unity 场景对象只是来源；真正运行时以这里的快照为准。
    /// </summary>
    [Serializable]
    public sealed class BattleStageRuntimeState
    {
        public int StageWidthPx = 800;
        public int ZMin = 180;
        public int ZMax = 350;
        public int PerspectiveNear;
        public int PerspectiveFar;

        public void Reset()
        {
            StageWidthPx = 800;
            ZMin = 180;
            ZMax = 350;
            PerspectiveNear = 0;
            PerspectiveFar = 0;
        }

        public void Set(int stageWidthPx, int zMin, int zMax, int perspectiveNear, int perspectiveFar)
        {
            StageWidthPx = Mathf.Max(stageWidthPx, 1);
            ZMin = zMin;
            ZMax = Mathf.Max(zMax, zMin + 1);
            PerspectiveNear = perspectiveNear;
            PerspectiveFar = perspectiveFar;
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

            int writeIndex = 0;
            for (int i = 0; i < config.players.Count && writeIndex < Slots.Length; i++)
            {
                PlayerSlotConfig player = config.players[i];
                if (player == null || !player.use)
                    continue;

                BattleSlotRuntimeState slot = Slots[writeIndex];
                slot.Active = true;
                slot.IsHuman = player.isHuman;
                slot.CharacterId = player.characterId;
                slot.Team = player.team;
                slot.InputId = player.inputId;
                slot.AiId = player.aiId;
                writeIndex++;
            }

            ActiveSlotCount = writeIndex;
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
        public int BattleExitCountdown;
        public int RouteOutRequest;
        public int Mode2Request;
        public int BattleStepMode;
        public int BattleStepGate;
        public int DjaGuardGlobal44F224;

        public void Reset()
        {
            CurrentTickIndex = 0;
            SparkRenderFrame = 0;
            AiPhaseGate = 0;
            BattleExitCountdown = 0;
            RouteOutRequest = 0;
            Mode2Request = 0;
            BattleStepMode = 0;
            BattleStepGate = 0;
            DjaGuardGlobal44F224 = 0;
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
        public BattleRosterRuntimeState Roster = new BattleRosterRuntimeState();
        public BattleFlowRuntimeState Flow = new BattleFlowRuntimeState();
        public int[] KillStats = new int[BattleStatSlotCount];
        public int[] DamageStats = new int[BattleStatSlotCount];

        public void Reset()
        {
            Match?.Reset();
            Stage?.Reset();
            Roster?.Reset();
            Flow?.Reset();
            ResetStatArray(ref KillStats);
            ResetStatArray(ref DamageStats);
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
