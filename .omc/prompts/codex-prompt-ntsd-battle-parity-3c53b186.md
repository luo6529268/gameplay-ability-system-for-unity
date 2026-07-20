---
provider: "codex"
agent_role: "architect"
model: "gpt-5.6-sol"
files:
  - "J:\\QQFile\\NTSD2.4\\ntsd_release_C#\\src\\BattleCore\\Simulation\\SimulationTickDriver.cs"
  - "J:\\QQFile\\NTSD2.4\\ntsd_release_C#\\src\\BattleCore\\Simulation\\GameTick.cs"
  - "J:\\QQFile\\NTSD2.4\\ntsd_release_C#\\src\\BattleCore\\Input\\InputRuntime.cs"
  - "Assets\\NTSD\\Scripts\\Simulation\\SimulationTickDriver.cs"
  - "Assets\\NTSD\\Scripts\\Simulation\\SimulationWorld.FrameInput.partial.cs"
  - "Assets\\NTSD\\Scripts\\Simulation\\BattleRuntimeState.cs"
  - "Assets\\NTSD\\Scripts\\Simulation\\BattleParitySnapshot.cs"
  - "Assets\\NTSD\\Scripts\\Test\\Editor\\BattleParityTraceEditor.cs"
  - "Tools\\NTSDParity\\TraceCompareCommand.cs"
  - "Temp\\NTSDParity\\compare-v3-diagnostic-full-iter2.json"
timestamp: "2026-07-17T05:18:20.307Z"
---

[BLOCKED] File 'J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\SimulationTickDriver.cs' is outside the working directory. Only files within the project are allowed.

[BLOCKED] File 'J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Simulation\GameTick.cs' is outside the working directory. Only files within the project are allowed.

[BLOCKED] File 'J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Input\InputRuntime.cs' is outside the working directory. Only files within the project are allowed.

--- File: Assets\NTSD\Scripts\Simulation\SimulationTickDriver.cs ---
﻿using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Simulation
{
    public enum SimulationDriveMode
    {
        LocalFreeRun,
        LockstepBuffered,
        Manual
    }

    /// <summary>
    /// 战斗逻辑帧配置。
    /// 逻辑帧长度固定使用 SimulationConstants.SIM_DT；这里的配置只决定外层驱动、追帧和联机预留策略。
    /// </summary>
    [System.Serializable]
    public sealed class LockstepSimulationSettings
    {
        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("单个 Unity 渲染帧最多追多少个逻辑帧。正式 NTSD 以 30Hz 逐帧呈现，默认不在一个渲染帧内连续追多个逻辑帧。")]
        public int maxCatchUpTicksPerFrame = 1;

        [Tooltip("最多保留多少个逻辑帧的时间积压，超过后丢弃外层积压但不改变单个逻辑帧步长。")]
        public int maxBacklogTicks = 8;

        [Tooltip("联机帧同步预留：本地输入写入未来第 N 帧。当前单机可保持 0。")]
        public int inputDelayTicks = 0;

        [Tooltip("联机帧同步预留：推进前是否要求该逻辑帧的输入已经准备好。")]
        public bool requireInputFrameReady = false;

        [Tooltip("在每个逻辑 tick 尾部生成 canonical battle snapshot 和分域 checksum。")]
        public bool enableFrameChecksum = false;

        public void Normalize()
        {
            if (maxCatchUpTicksPerFrame < 1) maxCatchUpTicksPerFrame = 1;
            if (maxBacklogTicks < maxCatchUpTicksPerFrame) maxBacklogTicks = maxCatchUpTicksPerFrame;
            if (inputDelayTicks < 0) inputDelayTicks = 0;
        }
    }

    /// <summary>
    /// 逻辑帧输入源预留接口。
    /// 当前单机输入仍由角色自己的 SimInputBuffer 消费；后续联机可在这里接入输入收齐、预测、回滚和重放。
    /// </summary>
    public interface ISimulationFrameInputProvider
    {
        bool IsFrameInputReady(int tickIndex);
        FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
        void BeforeSimTick(int tickIndex) { }
        void AfterSimTick(int tickIndex) { }
        void Reset() { }
    }

    public sealed class LocalSimulationFrameInputProvider : ISimulationFrameInputProvider
    {
        public bool IsFrameInputReady(int tickIndex) => true;
        public FrameInputSet GetFrameInput(int tickIndex) => FrameInputSet.Empty(tickIndex);
    }

    /// <summary>
    /// 战斗场景模拟时钟。
    /// 负责固定 30Hz 逻辑 tick，并把 C# 权威工程的 pass 顺序交给 NTSDBattleTickSystem。
    /// Unity 的 Update/LateUpdate 只作为外层驱动和表现刷新；战斗逻辑内部不能依赖 deltaTime。
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        [Tooltip("记录每个模拟 tick 的开始和结束。")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("启动时暂停，直到 BattleBootstrap 恢复模拟。")]
        [SerializeField] private bool startPaused = true;

        [Header("帧同步时钟")]
        [SerializeField] private LockstepSimulationSettings lockstepSettings = new LockstepSimulationSettings();

        [Header("调试信息（只读）")]
        [SerializeField][MMReadOnly] private int currentTickIndex = 0;
        [SerializeField][MMReadOnly] private float timeAccumulator = 0f;
        [SerializeField][MMReadOnly] private int objectCount = 0;
        [SerializeField][MMReadOnly] private bool paused = true;
        [SerializeField][MMReadOnly] private float renderAlpha = 0f;
        [SerializeField][MMReadOnly] private int backlogTickCount = 0;
        [SerializeField][MMReadOnly] private string lastFrameChecksum = string.Empty;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;

        private int _sparkRenderFrame = 0;
        private ISimulationFrameInputProvider _frameInputProvider = new LocalSimulationFrameInputProvider();
        private FrameInputSet _lastAppliedFrameInput = FrameInputSet.Empty(0);
        private BattleParityFrameSnapshot _lastFrameSnapshot;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;
            lockstepSettings ??= new LockstepSimulationSettings();
            lockstepSettings.Normalize();

            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);

            Log.Info($"[SimulationTickDriver] Awake. paused={paused}, World created");
        }

        private void Update()
        {
            if (paused || _world == null || lockstepSettings.driveMode == SimulationDriveMode.Manual)
            {
                RefreshInspectorState();
                return;
            }

            float delta = lockstepSettings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            _timeAccumulator += delta;

            int maxBacklogTicks = Mathf.Max(lockstepSettings.maxBacklogTicks, lockstepSettings.maxCatchUpTicksPerFrame);
            float maxAccumulator = SimulationConstants.SIM_DT * maxBacklogTicks;
            if (_timeAccumulator > maxAccumulator)
                _timeAccumulator = maxAccumulator;

            int catchUpTicks = 0;
            while (_timeAccumulator >= SimulationConstants.SIM_DT &&
                   catchUpTicks < lockstepSettings.maxCatchUpTicksPerFrame)
            {
                int nextTickIndex = _tickIndex + 1;
                if (!CanAdvanceTick(nextTickIndex))
                    break;

                _timeAccumulator -= SimulationConstants.SIM_DT;
                StepOneTickInternal(nextTickIndex);
                catchUpTicks++;
            }

            RefreshInspectorState();
        }

        private void FixedUpdate()
        {
            // 帧同步逻辑不依赖 Unity FixedUpdate。Unity 物理循环只作为引擎外层回调存在。
        }

        private void LateUpdate()
        {
            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                    _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
            }

            _sparkRenderer.RenderAll(_world);
        }

        private bool CanAdvanceTick(int tickIndex)
        {
            if (lockstepSettings.driveMode != SimulationDriveMode.LockstepBuffered &&
                !lockstepSettings.requireInputFrameReady)
            {
                return true;
            }

            return _frameInputProvider == null || _frameInputProvider.IsFrameInputReady(tickIndex);
        }

        private bool StepOneTickInternal(int tickIndex)
        {
            if (_world == null || !CanAdvanceTick(tickIndex))
                return false;

            _tickIndex = tickIndex;
            _sparkRenderFrame = tickIndex;
            if (_world.Runtime?.Flow != null)
            {
                _world.Runtime.Flow.SparkRenderFrame = _sparkRenderFrame;
            }

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");

            _frameInputProvider?.BeforeSimTick(tickIndex);
            FrameInputSet frameInput = _frameInputProvider?.GetFrameInput(tickIndex) ??
                                       FrameInputSet.Empty(tickIndex);
            if (frameInput.TickIndex != tickIndex)
                frameInput = FrameInputSet.Empty(tickIndex);

            _lastAppliedFrameInput = frameInput;
            _world.ApplyFrameInputSet(frameInput);
            _battleTickSystem?.RunReleaseTick(tickIndex);
            CaptureFrameChecksumIfNeeded(tickIndex, frameInput);
            _frameInputProvider?.AfterSimTick(tickIndex);

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");

            return true;
        }

        private void CaptureFrameChecksumIfNeeded(int tickIndex, FrameInputSet frameInput)
        {
            if (!lockstepSettings.enableFrameChecksum)
            {
                _lastFrameSnapshot = null;
                lastFrameChecksum = string.Empty;
                return;
            }

            _lastFrameSnapshot = _world.CaptureParityFrameSnapshot(tickIndex, frameInput);
            lastFrameChecksum = _lastFrameSnapshot?.Hashes?.Overall ?? string.Empty;
        }

        private void RefreshInspectorState()
        {
            currentTickIndex = _tickIndex;
            timeAccumulator = _timeAccumulator;
            objectCount = _world?.ObjectCount ?? 0;
            renderAlpha = Mathf.Clamp01(_timeAccumulator / SimulationConstants.SIM_DT);
            backlogTickCount = Mathf.FloorToInt(_timeAccumulator / SimulationConstants.SIM_DT);
        }

        public SimulationWorld World => _world;
        public int SparkRenderFrame => _sparkRenderFrame;
        public int CurrentTickIndex => _tickIndex;
        public FrameInputSet LastAppliedFrameInput => _lastAppliedFrameInput;
        public BattleParityFrameSnapshot LastFrameSnapshot => _lastFrameSnapshot;
        public bool HasFrameChecksum => _lastFrameSnapshot != null;
        public string LastFrameChecksum => lastFrameChecksum;

        public float RemainingAccumulatorTime => _timeAccumulator;
        public float RenderAlpha => renderAlpha;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        public void ApplySettings(LockstepSimulationSettings settings)
        {
            if (settings == null)
                return;

            lockstepSettings = settings;
            lockstepSettings.Normalize();
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            if (_world == null)
                return;

            _world.ResetRuntimeState();

            BattleMatchRuntimeState matchState = _world.Runtime?.Match;
            if (matchState != null)
            {
                matchState.LocalGameModeId = config?.gameMode?.gameModeId ?? 0;
                matchState.BattleGameModeId = config?.gameMode?.battleGameModeId ?? 1;
                matchState.BackgroundId = config?.backgroundId ?? -1;
                matchState.Difficulty = config?.difficulty ?? 2;
                matchState.Seed = config?.seed ?? 0;
            }

            _world.Rng?.Seed((uint)(config?.seed ?? 0));
            _world.Runtime?.Roster?.ApplyMatchConfig(config);
            _world.SetNeedClearInput(true);
            _world.RefreshStageRuntimeSnapshotFromScene();

            List<BattleStageCampaignData> stageCampaigns = BattleStageCampaignLoader.LoadFromFile(
                config?.stageCampaignFilePath);
            _world.ConfigureStageCampaigns(stageCampaigns, config?.stageSeriesId ?? 0, -1);
            if (matchState != null &&
                (matchState.BattleGameModeId == 1 || matchState.BattleGameModeId == 2))
            {
                _world.StartInitialStageWave();
            }

            _world.SetAiPhaseGate(matchState != null && matchState.BattleGameModeId == 2 ? 1 : 0);
        }

        public void SetFrameInputProvider(ISimulationFrameInputProvider provider)
        {
            _frameInputProvider = provider ?? new LocalSimulationFrameInputProvider();
            _frameInputProvider.Reset();
            _lastAppliedFrameInput = FrameInputSet.Empty(_tickIndex);
        }

        public bool StepOneTick(bool ignorePaused = false)
        {
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(_tickIndex + 1);
            RefreshInspectorState();
            return stepped;
        }

        public void UnbindWorld()
        {
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);
            _tickIndex = 0;
            _timeAccumulator = 0f;
            _sparkRenderFrame = 0;
            _lastAppliedFrameInput = FrameInputSet.Empty(0);
            _lastFrameSnapshot = null;
            lastFrameChecksum = string.Empty;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

        protected override void OnSingletonDestroyed()
        {
            _world = null;
            _battleTickSystem = null;
        }
    }
}


--- File: Assets\NTSD\Scripts\Simulation\SimulationWorld.FrameInput.partial.cs ---
using NTSD.Animation.LF2Objects;
using NTSD.Input;

namespace NTSD.Simulation
{
    public partial class SimulationWorld
    {
        private static readonly (SimulationInputButtons button, FuncKeyMask key)[] FrameInputKeys =
        {
            (SimulationInputButtons.Right, FuncKeyMask.right),
            (SimulationInputButtons.Left, FuncKeyMask.left),
            (SimulationInputButtons.Up, FuncKeyMask.up),
            (SimulationInputButtons.Down, FuncKeyMask.down),
            (SimulationInputButtons.Attack, FuncKeyMask.att),
            (SimulationInputButtons.Jump, FuncKeyMask.jump),
            (SimulationInputButtons.Defend, FuncKeyMask.def),
        };

        public void ApplyFrameInputSet(FrameInputSet frameInput)
        {
            if (frameInput?.Players == null || frameInput.Players.Count == 0)
                return;

            for (int i = 0; i < frameInput.Players.Count; i++)
            {
                SimulationPlayerInput playerInput = frameInput.Players[i];
                if (!TryResolveRosterInputEntity(playerInput.PlayerSlot, out LF2Entity entity) ||
                    entity.AiControlled ||
                    !entity.TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
                {
                    continue;
                }

                // The frame packet is a complete held-state snapshot. Queue every key so an
                // authoritative replay packet is applied after any local callback queued for
                // the same tick; NTSDInputStateModule derives the press/release edges once.
                for (int keyIndex = 0; keyIndex < FrameInputKeys.Length; keyIndex++)
                {
                    (SimulationInputButtons button, FuncKeyMask key) mapping = FrameInputKeys[keyIndex];
                    bool down = (playerInput.Buttons & mapping.button) != 0;
                    controller.InputBuffer.EnqueueForTick(frameInput.TickIndex, mapping.key, down);
                }
            }
        }

        internal bool TryResolveRosterInputEntity(int playerSlot, out LF2Entity entity)
        {
            return TryResolveRosterEntity(playerSlot, requireHuman: true, out entity);
        }

        internal bool TryResolveRosterEntity(int playerSlot, bool requireHuman, out LF2Entity entity)
        {
            entity = null;
            BattleRosterRuntimeState roster = Runtime?.Roster;
            if (roster?.Slots == null || playerSlot < 0 || playerSlot >= roster.Slots.Length)
                return false;

            BattleSlotRuntimeState rosterSlot = roster.Slots[playerSlot];
            if (rosterSlot == null || !rosterSlot.Active || (requireHuman && !rosterSlot.IsHuman))
                return false;

            entity = ResolveRosterSlotEntity(rosterSlot.RuntimeSlotIndex, rosterSlot);
            if (entity == null && rosterSlot.StableId >= 0)
                entity = FindRosterEntityByStableId(rosterSlot.StableId, rosterSlot);

            if (entity == null)
                entity = ResolveRosterSlotEntity(playerSlot, rosterSlot);

            if (entity == null)
            {
                for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
                {
                    LF2Entity candidate = ResolveRosterSlotEntity(runtimeSlot, rosterSlot);
                    if (candidate == null || IsRuntimeSlotBoundToOtherRosterPlayer(runtimeSlot, playerSlot))
                        continue;

                    entity = candidate;
                    break;
                }
            }

            if (entity == null)
                return false;

            rosterSlot.RuntimeSlotIndex = entity.Runtime.SlotIndex;
            rosterSlot.StableId = entity.Runtime.StableId;
            return true;
        }

        private LF2Entity ResolveRosterSlotEntity(int runtimeSlot, BattleSlotRuntimeState rosterSlot)
        {
            if (runtimeSlot < 0 || runtimeSlot >= MaxRuntimeSlots)
                return null;

            LF2Entity candidate = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
            return RosterEntityMatches(candidate, rosterSlot) ? candidate : null;
        }

        private LF2Entity FindRosterEntityByStableId(int stableId, BattleSlotRuntimeState rosterSlot)
        {
            for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
            {
                LF2Entity candidate = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (candidate?.Runtime?.StableId == stableId && RosterEntityMatches(candidate, rosterSlot))
                    return candidate;
            }

            return null;
        }

        private bool IsRuntimeSlotBoundToOtherRosterPlayer(int runtimeSlot, int playerSlot)
        {
            BattleSlotRuntimeState[] rosterSlots = Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return false;

            for (int i = 0; i < rosterSlots.Length; i++)
            {
                if (i != playerSlot && rosterSlots[i]?.Active == true &&
                    rosterSlots[i].RuntimeSlotIndex == runtimeSlot)
                {
                    return true;
                }
            }

            return false;
        }

        private bool RosterEntityMatches(LF2Entity candidate, BattleSlotRuntimeState rosterSlot)
        {
            if (candidate?.Runtime == null || !IsActiveForCurrentPass(candidate) ||
                candidate.AiControlled == rosterSlot.IsHuman)
                return false;
            if (candidate.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return false;
            if (rosterSlot.CharacterId >= 0 && candidate.ObjectId != rosterSlot.CharacterId)
                return false;
            return candidate.Team == rosterSlot.Team;
        }
    }
}


--- File: Assets\NTSD\Scripts\Simulation\BattleRuntimeState.cs ---
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
            NeedClearInput = false;
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
        public BattleRosterRuntimeState Roster = new BattleRosterRuntimeState();
        public BattleFlowRuntimeState Flow = new BattleFlowRuntimeState();
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
            StageSpawnRuntimeSlots?.Clear();
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


--- File: Assets\NTSD\Scripts\Simulation\BattleParitySnapshot.cs ---
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public sealed class BattleParityHashes
    {
        public string ARest;
        public string Events;
        public string Input;
        public string Overall;
        public string Rng;
        public string Slots;
        public string Stats;
        public string VRest;
        public string World;

        internal SortedDictionary<string, object> ToCanonicalObject(bool includeOverall)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARest,
                ["events"] = Events,
                ["input"] = Input,
                ["rng"] = Rng,
                ["slots"] = Slots,
                ["stats"] = Stats,
                ["vRest"] = VRest,
                ["world"] = World,
            };
            if (includeOverall)
                result["overall"] = Overall;
            return result;
        }
    }

    public sealed class BattleParityFrameSnapshot
    {
        internal object InputDomain;
        internal object RngDomain;
        internal object WorldDomain;
        internal object[] AllSlotsDomain;
        internal object[] CompactSlotsDomain;
        internal string[] SlotCommitments;
        internal object ARestDomain;
        internal object VRestDomain;
        internal object FullARestDomain;
        internal object FullVRestDomain;
        internal object StatsDomain;
        internal object EventsDomain;

        public int Tick { get; internal set; }
        public int ObjectCount { get; internal set; }
        public BattleParityHashes Hashes { get; internal set; }

        public string ToJson()
        {
            return ToJson(full: false);
        }

        public string ToJson(bool full)
        {
            var tick = new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = full && FullARestDomain != null ? FullARestDomain : ARestDomain,
                ["events"] = EventsDomain,
                ["hashes"] = Hashes.ToCanonicalObject(includeOverall: true),
                ["input"] = InputDomain,
                ["kind"] = "tick",
                ["objectCount"] = ObjectCount,
                ["rng"] = RngDomain,
                ["slots"] = full ? AllSlotsDomain : CompactSlotsDomain,
                ["slotCommitments"] = SlotCommitments,
                ["stats"] = StatsDomain,
                ["tick"] = Tick,
                ["vRest"] = full && FullVRestDomain != null ? FullVRestDomain : VRestDomain,
                ["world"] = WorldDomain,
            };
            return BattleCanonicalJson.Serialize(tick);
        }
    }

    public static class BattleCanonicalJson
    {
        public static string Serialize(object value)
        {
            var builder = new StringBuilder(4096);
            WriteValue(builder, value);
            return builder.ToString();
        }

        public static string Sha256(object value)
        {
            byte[] payload = Encoding.UTF8.GetBytes(Serialize(value));
            using SHA256 sha = SHA256.Create();
            byte[] digest = sha.ComputeHash(payload);
            var builder = new StringBuilder(digest.Length * 2);
            for (int i = 0; i < digest.Length; i++)
                builder.Append(digest[i].ToString("x2", CultureInfo.InvariantCulture));
            return builder.ToString();
        }

        private static void WriteValue(StringBuilder builder, object value)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            switch (value)
            {
                case string text:
                    WriteString(builder, text);
                    return;
                case char character:
                    WriteString(builder, character.ToString());
                    return;
                case bool boolean:
                    builder.Append(boolean ? "true" : "false");
                    return;
                case byte byteValue:
                    builder.Append(byteValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case sbyte signedByteValue:
                    builder.Append(signedByteValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case short shortValue:
                    builder.Append(shortValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case ushort unsignedShortValue:
                    builder.Append(unsignedShortValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case int intValue:
                    builder.Append(intValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case uint unsignedIntValue:
                    builder.Append(unsignedIntValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case long longValue:
                    builder.Append(longValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case ulong unsignedLongValue:
                    builder.Append(unsignedLongValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case float floatValue:
                    WriteFloatingPoint(builder, floatValue);
                    return;
                case double doubleValue:
                    WriteFloatingPoint(builder, doubleValue);
                    return;
                case decimal decimalValue:
                    builder.Append(decimalValue.ToString(CultureInfo.InvariantCulture));
                    return;
                case IDictionary dictionary:
                    WriteDictionary(builder, dictionary);
                    return;
                case IEnumerable enumerable:
                    WriteArray(builder, enumerable);
                    return;
            }

            if (value.GetType().IsEnum)
            {
                builder.Append(Convert.ToInt64(value, CultureInfo.InvariantCulture));
                return;
            }

            throw new InvalidOperationException(
                $"Unsupported canonical JSON value type: {value.GetType().FullName}");
        }

        private static void WriteDictionary(StringBuilder builder, IDictionary dictionary)
        {
            var keys = new List<string>(dictionary.Count);
            foreach (object key in dictionary.Keys)
                keys.Add(Convert.ToString(key, CultureInfo.InvariantCulture));
            keys.Sort(StringComparer.Ordinal);

            builder.Append('{');
            for (int i = 0; i < keys.Count; i++)
            {
                if (i > 0)
                    builder.Append(',');
                string key = keys[i];
                WriteString(builder, key);
                builder.Append(':');
                WriteValue(builder, dictionary[key]);
            }
            builder.Append('}');
        }

        private static void WriteArray(StringBuilder builder, IEnumerable values)
        {
            builder.Append('[');
            bool first = true;
            foreach (object value in values)
            {
                if (!first)
                    builder.Append(',');
                first = false;
                WriteValue(builder, value);
            }
            builder.Append(']');
        }

        private static void WriteFloatingPoint(StringBuilder builder, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value))
                throw new InvalidOperationException("Canonical battle snapshots cannot contain NaN or Infinity.");
            builder.Append(value.ToString("R", CultureInfo.InvariantCulture));
        }

        private static void WriteString(StringBuilder builder, string value)
        {
            builder.Append('"');
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];
                switch (c)
                {
                    case '"': builder.Append("\\\""); break;
                    case '\\': builder.Append("\\\\"); break;
                    case '\b': builder.Append("\\b"); break;
                    case '\f': builder.Append("\\f"); break;
                    case '\n': builder.Append("\\n"); break;
                    case '\r': builder.Append("\\r"); break;
                    case '\t': builder.Append("\\t"); break;
                    default:
                        if (c < 0x20 || c > 0x7E)
                            builder.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                        else
                            builder.Append(c);
                        break;
                }
            }
            builder.Append('"');
        }
    }

    public partial class SimulationWorld
    {
        public BattleParityFrameSnapshot CaptureParityFrameSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null,
            bool includeFullDomains = false)
        {
            object inputDomain = ProjectFrameInput(frameInput ?? FrameInputSet.Empty(tickIndex));
            object rngDomain = DictionaryOf(
                ("callCount", (object)(Rng?.CallCount ?? 0UL)),
                ("seed", Rng?.State ?? 0U));
            object worldDomain = ProjectWorldDomain();
            object[] allSlots = ProjectAllRuntimeSlots();
            var slotCommitments = new string[allSlots.Length];
            for (int slot = 0; slot < allSlots.Length; slot++)
                slotCommitments[slot] = BattleCanonicalJson.Sha256(allSlots[slot]);
            object slotCommitmentDomain = DictionaryOf(
                ("commitments", (object)slotCommitments),
                ("count", allSlots.Length));
            object aRestDomain = ProjectARestDomain();
            object vRestDomain = ProjectVRestDomain();
            object statsDomain = DictionaryOf(
                ("damage", CloneArray(DamageStats)),
                ("kill", CloneArray(KillStats)));
            object eventsDomain = ProjectEventsDomain();

            var hashes = new BattleParityHashes
            {
                ARest = BattleCanonicalJson.Sha256(aRestDomain),
                Events = BattleCanonicalJson.Sha256(eventsDomain),
                Input = BattleCanonicalJson.Sha256(inputDomain),
                Rng = BattleCanonicalJson.Sha256(rngDomain),
                Slots = BattleCanonicalJson.Sha256(slotCommitmentDomain),
                Stats = BattleCanonicalJson.Sha256(statsDomain),
                VRest = BattleCanonicalJson.Sha256(vRestDomain),
                World = BattleCanonicalJson.Sha256(worldDomain),
            };
            hashes.Overall = BattleCanonicalJson.Sha256(hashes.ToCanonicalObject(includeOverall: false));

            var compactSlots = new List<object>();
            for (int slot = 0; slot < allSlots.Length; slot++)
            {
                object baseline = ProjectDefaultRuntimeSlot(slot);
                if (!string.Equals(
                        BattleCanonicalJson.Sha256(allSlots[slot]),
                        BattleCanonicalJson.Sha256(baseline),
                        StringComparison.Ordinal))
                {
                    compactSlots.Add(allSlots[slot]);
                }
            }

            return new BattleParityFrameSnapshot
            {
                Tick = tickIndex,
                ObjectCount = ObjectCount,
                Hashes = hashes,
                InputDomain = inputDomain,
                RngDomain = rngDomain,
                WorldDomain = worldDomain,
                AllSlotsDomain = allSlots,
                CompactSlotsDomain = compactSlots.ToArray(),
                SlotCommitments = slotCommitments,
                ARestDomain = aRestDomain,
                VRestDomain = vRestDomain,
                FullARestDomain = includeFullDomains ? ProjectFullARestDomain() : null,
                FullVRestDomain = includeFullDomains ? ProjectFullVRestDomain() : null,
                StatsDomain = statsDomain,
                EventsDomain = eventsDomain,
            };
        }

        private object ProjectFrameInput(FrameInputSet frameInput)
        {
            var players = new object[frameInput.Players?.Count ?? 0];
            for (int i = 0; i < players.Length; i++)
            {
                SimulationPlayerInput player = frameInput.Players[i];
                players[i] = DictionaryOf(
                    ("buttons", (object)(byte)player.Buttons),
                    ("playerSlot", player.PlayerSlot));
            }
            return DictionaryOf(("players", (object)players), ("tickIndex", frameInput.TickIndex));
        }

        private object[] ProjectAllRuntimeSlots()
        {
            var result = new object[MaxRuntimeSlots];
            for (int runtimeSlot = 0; runtimeSlot < result.Length; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                result[runtimeSlot] = entity == null
                    ? ProjectDefaultRuntimeSlot(runtimeSlot)
                    : ProjectRuntimeSlot(entity, runtimeSlot);
            }
            return result;
        }

        private object ProjectDefaultRuntimeSlot(int runtimeSlot)
        {
            return DictionaryOf(
                ("currentDataOid", null),
                ("runtime", ProjectEntityRuntime(null, runtimeSlot, false)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectRuntimeSlot(LF2Entity entity, int runtimeSlot)
        {
            bool active = IsActiveForCurrentPass(entity);
            int? currentDataOid = entity.FrameCache?.Wrapper != null
                ? entity.FrameCache.Wrapper.characterId
                : entity.ObjectId;
            return DictionaryOf(
                ("currentDataOid", (object)currentDataOid),
                ("runtime", ProjectEntityRuntime(entity, runtimeSlot, active)),
                ("runtimeSlot", runtimeSlot));
        }

        private object ProjectEntityRuntime(LF2Entity entity, int runtimeSlot, bool active)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            bool isDefault = runtime == null;
            int[] hitRecordDamage = new int[LF2Entity.MaxHitRecordSlots];
            int[] hitRecordX = new int[LF2Entity.MaxHitRecordSlots];
            int[] hitRecordZ = new int[LF2Entity.MaxHitRecordSlots];
            if (entity != null)
            {
                for (int i = 0; i < hitRecordDamage.Length; i++)
                {
                    hitRecordDamage[i] = entity.GetHitRecordAge(i);
                    hitRecordX[i] = entity.GetHitRecordX(i);
                    hitRecordZ[i] = entity.GetHitRecordZ(i);
                }
            }

            int currentDataType = entity?.GetCurrentDataObjectTypeForSimulation() ?? -1;
            int category = ResolveTraceCategory(currentDataType);
            object identity = DictionaryOf(
                ("active", active),
                ("aiControlled", runtime?.AiControlled ?? false),
                ("category", isDefault ? 3 : category),
                ("charId", isDefault ? -1 : runtime.ObjectId),
                ("entityType", isDefault ? 0 : runtime.EntityType),
                ("objType", isDefault ? 0 : runtime.ObjType),
                ("ownerId", isDefault ? -1 : runtime.OwnerSlotIndex),
                ("slot", runtimeSlot),
                ("team", isDefault ? 0 : runtime.Team),
                ("unk364", isDefault ? 0 : (runtime.RelationTeam != 0 ? runtime.RelationTeam : runtime.Team)));

            object transform = DictionaryOf(
                ("facing", (object)(isDefault || runtime.Dir == "right" ? 0 : 1)),
                ("renderOffsetX", isDefault ? 0 : (int)runtime.RenderOffsetX),
                ("type3VisualZOffset", isDefault ? 0.0 : runtime.Type3VisualZOffset),
                ("x", isDefault ? 0.0 : runtime.X),
                ("xInt", isDefault ? 0 : runtime.XInt),
                ("y", isDefault ? 0.0 : runtime.Y),
                ("yInt", isDefault ? 0 : runtime.YInt),
                ("z", isDefault ? 0.0 : runtime.Z),
                ("zInt", isDefault ? 0 : runtime.ZInt));

            object motion = DictionaryOf(
                ("fall", isDefault ? 0 : runtime.Fall),
                ("hitCount", isDefault ? 0 : runtime.HitCount),
                ("knockbackVx", isDefault ? 0.1 : runtime.KnockbackVx),
                ("knockbackVy", isDefault ? 0.1 : runtime.KnockbackVy),
                ("knockbackVz", isDefault ? 0.1 : runtime.KnockbackVz),
                ("vx", isDefault ? 0.0 : runtime.Vx),
                ("vy", isDefault ? 0.0 : runtime.Vy),
                ("vz", isDefault ? 0.0 : runtime.Vz));

            object frame = DictionaryOf(
                ("animCounter", isDefault ? 0 : runtime.AnimCounter),
                ("animSub", isDefault ? 0 : runtime.AnimSub),
                ("attacking", isDefault ? 0 : runtime.AttackingCounter),
                ("frame", isDefault ? 0 : runtime.Frame),
                ("frameDelay", isDefault ? 0 : runtime.FrameDelay),
                ("frameWaitCounter", isDefault ? 0 : runtime.WaitCounter),
                ("hitStateCount", isDefault ? 0 : runtime.HitStateCount),
                ("hitStop", isDefault ? 0 : runtime.HitStop),
                ("jumpInitPending", false),
                ("prevFrame", isDefault ? 0 : entity.Frame?.Prev ?? 0),
                ("prevFrame2", isDefault ? 0 : runtime.PrevFrame2),
                ("suppressJumpInit", false),
                ("waitCounter", isDefault ? 0 : runtime.WaitCounter));

            object links = DictionaryOf(
                ("catcherIdx", isDefault ? -1 : runtime.CatcherSlotIndex),
                ("caughtDuration", isDefault ? 0 : runtime.CaughtDuration),
                ("caughtIdx", isDefault ? -1 : runtime.CaughtSlotIndex),
                ("escapeCounter", isDefault ? 0 : runtime.CatchingStateTU),
                ("grabbedTimer", isDefault ? 0 : runtime.GrabbedBy),
                ("heldWeaponSlot", isDefault ? -1 : runtime.HeldWeaponStableId),
                ("holderCopy", isDefault ? 99 : runtime.HolderCopySlotIndex),
                ("holderIdx", isDefault ? -1 : runtime.HolderStableId),
                ("linkState", isDefault ? 0 : runtime.LinkState),
                ("pickerIdx", isDefault ? -1 : runtime.PickerStableId),
                ("pickupCount", 0),
                ("releaseTick", -1),
                ("stuckVictimSlot", -1),
                ("targetIdx", isDefault ? -1 : runtime.TargetSlotIndex),
                ("throwFrameGuard", isDefault ? -1 : runtime.ThrowFrameGuard));

            object transient = DictionaryOf(
                ("hitCandidateItrIndices", (object)new sbyte[20]),
                ("hitCandidateSlots", new int[20]),
                ("mp", isDefault ? 0 : runtime.TransientMp),
                ("mp2", isDefault ? 1000 : runtime.TransientMp2),
                ("mp3", isDefault ? 1000 : runtime.TransientMp3),
                ("mp4", isDefault ? 1000 : runtime.TransientMp4));

            object stats = DictionaryOf(
                ("comboCountAtk", isDefault ? 0 : runtime.ComboCountAtk),
                ("comboCountVic", isDefault ? 0 : runtime.ComboCountVic),
                ("fallDamageDiv", isDefault ? 0 : runtime.FallDamageDiv),
                ("hp", isDefault ? 500 : runtime.HP),
                ("hp3", isDefault ? 500 : runtime.HP3),
                ("hpMax", isDefault ? 500 : runtime.HPBound),
                ("killCount", isDefault ? -1 : runtime.KillCount),
                ("killStat", isDefault ? 0 : runtime.KillStat),
                ("pp", isDefault ? 500 : runtime.PP),
                ("respawnCount", isDefault ? 0 : runtime.RespawnCount),
                ("spawnerSlot", isDefault ? -1 : runtime.SpawnerSlotIndex),
                ("unk344", isDefault ? 0 : runtime.Unk344),
                ("weaponCount", isDefault ? 0 : runtime.WeaponCount));

            object input = DictionaryOf(
                ("cdAttack", (object)(runtime?.CdAttack ?? 0)),
                ("cdDefend", runtime?.CdDefend ?? 0),
                ("cdDefendLock", runtime?.CdDefendLock ?? 0),
                ("cdDown", runtime?.CdDown ?? 0),
                ("cdJump", runtime?.CdJump ?? 0),
                ("cdLeft", runtime?.CdLeft ?? 0),
                ("cdRight", runtime?.CdRight ?? 0),
                ("cdUp", runtime?.CdUp ?? 0),
                ("comboDda", runtime?.ComboDda ?? 0),
                ("comboDdj", runtime?.ComboDdj ?? 0),
                ("comboDja", runtime?.ComboDja ?? 0),
                ("comboDla", runtime?.ComboDla ?? 0),
                ("comboDlj", runtime?.ComboDlj ?? 0),
                ("comboDra", runtime?.ComboDra ?? 0),
                ("comboDrj", runtime?.ComboDrj ?? 0),
                ("comboDua", runtime?.ComboDua ?? 0),
                ("comboDuj", runtime?.ComboDuj ?? 0),
                ("inputHistory", isDefault ? new int[6] : CloneArray(runtime.InputHistory)),
                ("keyAttack", runtime?.KeyAttack ?? 0),
                ("keyDefend", runtime?.KeyDefend ?? 0),
                ("keyDown", runtime?.KeyDown ?? 0),
                ("keyJump", runtime?.KeyJump ?? 0),
                ("keyLeft", runtime?.KeyLeft ?? 0),
                ("keyRight", runtime?.KeyRight ?? 0),
                ("keyUp", runtime?.KeyUp ?? 0),
                ("prevAttack", runtime?.PrevAttack ?? 0),
                ("prevDefend", runtime?.PrevDefend ?? 0),
                ("prevDown", runtime?.PrevDown ?? 0),
                ("prevJump", runtime?.PrevJump ?? 0),
                ("prevLeft", runtime?.PrevLeft ?? 0),
                ("prevRight", runtime?.PrevRight ?? 0),
                ("prevUp", runtime?.PrevUp ?? 0));

            object presentation = DictionaryOf(
                ("blink", isDefault ? 0 : runtime.Blink),
                ("hitRecordCount", entity?.HitRecordCount ?? 0),
                ("hitRecordDamage", hitRecordDamage),
                ("hitRecordX", hitRecordX),
                ("hitRecordZ", hitRecordZ),
                ("hp2Orig", isDefault ? 0 : runtime.HP2Orig),
                ("hpOrig", isDefault ? 0 : runtime.HPOrig),
                ("ppDisplay", isDefault ? 0 : runtime.PpDisplay));

            object residual = DictionaryOf(
                ("abortRemainingHitPairs", false),
                ("attackExempt", isDefault ? 0 : runtime.AttackExempt),
                ("blockBackZ", 0),
                ("blockFwdZ", 0),
                ("blockLeft", 0),
                ("blockRight", 0),
                ("catchTimer", isDefault ? 0 : runtime.CatchTimer),
                ("healTimer", isDefault ? 0 : runtime.HealTimer),
                ("hitConfirm", isDefault ? 0 : runtime.HitConfirmEa),
                ("hitConfirm2", isDefault ? 0 : runtime.HitConfirm2),
                ("unk318", 0),
                ("unk31C", 0),
                ("unk324", -1),
                ("unk328", isDefault ? -1 : runtime.Unk328),
                ("unk32C", isDefault ? -1 : runtime.Unk32C),
                ("unk330", isDefault ? 0 : runtime.Unk330),
                ("unk334", isDefault ? 0 : runtime.Unk334),
                ("unk338", isDefault ? 0 : runtime.Unk338),
                ("unk33C", -1),
                ("unk360", isDefault ? -1 : runtime.Unk360),
                ("unk3FC", isDefault ? -1000 : runtime.Unk3FC),
                ("unk400", isDefault ? -1000 : runtime.Unk400),
                ("weaponState", isDefault ? 0 : runtime.WeaponState));

            return DictionaryOf(
                ("frame", frame),
                ("identity", identity),
                ("input", input),
                ("links", links),
                ("motion", motion),
                ("presentation", presentation),
                ("residual", residual),
                ("stats", stats),
                ("transform", transform),
                ("transient", transient));
        }

        private object ProjectWorldDomain()
        {
            BattleRuntimeState battle = Runtime ?? new BattleRuntimeState();
            BattleMatchRuntimeState match = battle.Match ?? new BattleMatchRuntimeState();
            BattleStageRuntimeState stage = battle.Stage ?? new BattleStageRuntimeState();
            BattleFlowRuntimeState flow = battle.Flow ?? new BattleFlowRuntimeState();
            BattleRosterRuntimeState roster = battle.Roster ?? new BattleRosterRuntimeState();
            BattleStageProgressionState progression = battle.StageProgression ?? new BattleStageProgressionState();

            int slotCount = roster.Slots?.Length ?? 0;
            var battleSlotEntity = FilledArray(8, -1);
            var battleSlotOid = FilledArray(8, -1);
            var battleSlotState = new int[8];
            var battleSlotTeam = FilledArray(8, 1);
            var rosterSlots = new object[8];
            for (int i = 0; i < rosterSlots.Length; i++)
            {
                BattleSlotRuntimeState slot = i < slotCount ? roster.Slots[i] : null;
                bool active = slot?.Active ?? false;
                int oid = active ? slot.CharacterId : -1;
                int entitySlot = active ? slot.RuntimeSlotIndex : -1;
                int team = active ? slot.Team : 1;
                battleSlotEntity[i] = entitySlot;
                battleSlotOid[i] = oid;
                battleSlotState[i] = active ? 3 : 0;
                battleSlotTeam[i] = team;
                rosterSlots[i] = DictionaryOf(
                    ("active", (object)active),
                    ("ai", active && !slot.IsHuman),
                    ("entitySlot", entitySlot),
                    ("oid", oid),
                    ("state", battleSlotState[i]),
                    ("team", team));
            }

            object runtimeDomain = DictionaryOf(
                ("flow", DictionaryOf(
                    ("aiPhaseGate", (object)flow.AiPhaseGate),
                    ("battlePauseOverlay", 0),
                    ("battleStepEarlyReturned", 0),
                    ("battleStepFlag", 0),
                    ("battleStepGate", flow.BattleStepGate),
                    ("battleStepMode", flow.BattleStepMode),
                    ("frameMod12", flow.FrameMod12),
                    ("frameToggle", flow.FrameToggle),
                    ("gameTick", flow.CurrentTickIndex),
                    ("inputPhase", flow.InputPhase),
                    ("needClearInput", flow.NeedClearInput),
                    ("paused", false))),
                ("match", DictionaryOf(
                    ("difficulty", (object)match.Difficulty),
                    ("gameMode", match.BattleGameModeId),
                    ("randomStage", match.BackgroundId),
                    ("seed", match.Seed),
                    ("stageIdx", progression.StageSeriesIdx))),
                ("roster", DictionaryOf(
                    ("activeSlotCount", (object)roster.ActiveSlotCount),
                    ("slots", rosterSlots))),
                ("stage", DictionaryOf(
                    ("boundLeft", (object)0),
                    ("boundRight", stage.BoundRight),
                    ("cameraMaxOverride", stage.CameraMaxOverride),
                    ("cameraVel", _cameraVel),
                    ("cameraX", _cameraX),
                    ("width", stage.StageWidthPx),
                    ("xMaxOverride", stage.XMaxOverride),
                    ("zMax", stage.ZMax),
                    ("zMin", stage.ZMin))));

            return DictionaryOf(
                ("aiDifficulty", (object)flow.AiDifficulty),
                ("aiMoveMode", flow.AiMoveMode),
                ("aiPhaseGate", flow.AiPhaseGate),
                ("aiRand15", flow.AiRand15),
                ("aiRand20", flow.AiRand20),
                ("aiRand3", flow.AiRand3),
                ("aiRand5", flow.AiRand5),
                ("aiStageTargetX", flow.AiStageTargetX),
                ("battlePauseOverlay", 0),
                ("battleSlotCount", roster.ActiveSlotCount),
                ("battleSlotEntity", battleSlotEntity),
                ("battleSlotOid", battleSlotOid),
                ("battleSlotState", battleSlotState),
                ("battleSlotTeam", battleSlotTeam),
                ("battleStepEarlyReturned", 0),
                ("battleStepFlag449048", 0),
                ("battleStepGate44905C", flow.BattleStepGate),
                ("battleStepMode", flow.BattleStepMode),
                ("boundLeft", 0),
                ("boundRight", stage.BoundRight),
                ("cameraMaxOverride", stage.CameraMaxOverride),
                ("cameraVel", _cameraVel),
                ("cameraX", _cameraX),
                ("difficulty", match.Difficulty),
                ("djaGuardGlobal44F224", flow.DjaGuardGlobal44F224),
                ("f8Pressed", false),
                ("frameMod12", flow.FrameMod12),
                ("frameToggle", flow.FrameToggle),
                ("gameMode", match.BattleGameModeId),
                ("gameMode2", match.LocalGameModeId),
                ("gameTick", flow.CurrentTickIndex),
                ("humanInputPolledExternally", false),
                ("initStats", 0),
                ("inputPhase", flow.InputPhase),
                ("needClearInput", flow.NeedClearInput),
                ("objectCount", ObjectCount),
                ("paused", false),
                ("ppMode", PpMode),
                ("randomStage", match.BackgroundId),
                ("reserveCommittedHp", ZeroMatrix(2, 11)),
                ("reserveCommittedTotal", ZeroMatrix(2, 11)),
                ("reserveLiveCount", ZeroMatrix(2, 11)),
                ("reserveMissingCount", ZeroMatrix(2, 11)),
                ("reserveOidTable", new[] { 30, 31, 33, 34, 39, 32, 35, 36, 37, 122, 123 }),
                ("reserveOwnerValid", false),
                ("results", DictionaryOf(
                    ("battleEndPhase", (object)0),
                    ("hadBoth", false),
                    ("pendingHostAction", 0),
                    ("pendingWinner", -2),
                    ("phase", 0),
                    ("teamCount", 0),
                    ("teamIds", new[] { -1, -1 }),
                    ("timer", 0),
                    ("winner", -1))),
                ("runtime", runtimeDomain),
                ("stageAiInputCarrier", 0),
                ("stageIdx", progression.StageSeriesIdx),
                ("stageProgression", DictionaryOf(
                    ("round", (object)progression.Round),
                    ("roundMax", progression.RoundMax),
                    ("stageSeriesIdx", progression.StageSeriesIdx),
                    ("waveIdx", progression.WaveIdx))),
                ("stageProgressionValid", battle.StageProgressionValid),
                ("stageSpawnRuntimeEntryCount", CloneList(battle.StageSpawnRuntimeEntryCount)),
                ("stageSpawnRuntimeSlots", CloneNestedList(battle.StageSpawnRuntimeSlots)),
                ("stageSpawnRuntimeSpawnedTotal", CloneList(battle.StageSpawnRuntimeSpawnedTotal)),
                ("stageSpawnRuntimeTargetTotal", CloneList(battle.StageSpawnRuntimeTargetTotal)),
                ("stageSpawnRuntimeWave", battle.StageSpawnRuntimeWave),
                ("stageSpawnWaveApplied", battle.StageSpawnWaveApplied),
                ("stageSpawnWaveDeferredEntryApplied", battle.StageSpawnWaveDeferredEntryApplied),
                ("xMaxOverride", stage.XMaxOverride));
        }

        private object ProjectARestDomain()
        {
            var entries = new List<object>();
            for (int slot = 0; slot < MaxRuntimeSlots; slot++)
            {
                int value = FindEntityByRuntimeSlotIncludingDormant(slot)?.ItrRest?.Arest ?? 0;
                if (value != 0)
                    entries.Add(DictionaryOf(("slot", (object)slot), ("value", value)));
            }
            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectVRestDomain()
        {
            var entries = new List<object>();
            var victims = new LF2Entity[MaxRuntimeSlots];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < MaxRuntimeSlots; first++)
            {
                for (int second = 0; second < MaxRuntimeSlots; second++)
                {
                    // v3 preserves the authority matrix byte order. Its historical labels
                    // call the first (actual victim) index attackerSlot.
                    int value = victims[first]?.ItrRest?.GetVrest(second) ?? 0;
                    if (value == 0)
                        continue;
                    entries.Add(DictionaryOf(
                        ("attackerSlot", (object)first),
                        ("value", value),
                        ("victimSlot", second)));
                }
            }
            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectFullARestDomain()
        {
            var values = new int[MaxRuntimeSlots];
            for (int slot = 0; slot < values.Length; slot++)
                values[slot] = FindEntityByRuntimeSlotIncludingDormant(slot)?.ItrRest?.Arest ?? 0;

            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "full"),
                ("values", values));
        }

        private object ProjectFullVRestDomain()
        {
            var values = new int[MaxRuntimeSlots][];
            var victims = new LF2Entity[MaxRuntimeSlots];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < MaxRuntimeSlots; first++)
            {
                var row = new int[MaxRuntimeSlots];
                for (int second = 0; second < MaxRuntimeSlots; second++)
                    row[second] = victims[first]?.ItrRest?.GetVrest(second) ?? 0;
                values[first] = row;
            }

            return DictionaryOf(
                ("dimension", (object)MaxRuntimeSlots),
                ("encoding", "full-row-major"),
                ("values", values));
        }

        private object ProjectEventsDomain()
        {
            var sounds = new object[PendingSounds?.Count ?? 0];
            for (int i = 0; i < sounds.Length; i++)
            {
                PendingSoundEvent sound = PendingSounds[i];
                sounds[i] = DictionaryOf(
                    ("cue", (object)(sound?.Cue ?? string.Empty)),
                    ("tick", sound?.Tick ?? 0),
                    ("worldX", sound?.WorldX ?? 0));
            }
            return DictionaryOf(("pendingSounds", (object)sounds));
        }

        private static int ResolveTraceCategory(int dataType)
        {
            return dataType switch
            {
                0 => 0,
                1 or 2 or 4 or 6 => 1,
                3 => 2,
                _ => 3,
            };
        }

        private static int[] CloneArray(int[] values)
        {
            return values == null ? Array.Empty<int>() : (int[])values.Clone();
        }

        private static int[] FilledArray(int count, int value)
        {
            var result = new int[count];
            if (value != 0)
            {
                for (int i = 0; i < result.Length; i++)
                    result[i] = value;
            }
            return result;
        }

        private static object[] ZeroMatrix(int rows, int columns)
        {
            var result = new object[rows];
            for (int i = 0; i < rows; i++)
                result[i] = new int[columns];
            return result;
        }

        private static int[] CloneList(List<int> values)
        {
            return values == null ? Array.Empty<int>() : values.ToArray();
        }

        private static object[] CloneNestedList(List<int[]> values)
        {
            if (values == null)
                return Array.Empty<object>();
            var result = new object[values.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = CloneArray(values[i]);
            return result;
        }

        private static SortedDictionary<string, object> DictionaryOf(
            params (string key, object value)[] values)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
            for (int i = 0; i < values.Length; i++)
                result[values[i].key] = values[i].value;
            return result;
        }
    }
}


--- File: Assets\NTSD\Scripts\Test\Editor\BattleParityTraceEditor.cs ---
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Tools;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace NTSD.EditorTools
{
    [InitializeOnLoad]
    public static class BattleParityTraceEditor
    {
        private const string RequestFile = "Temp/NTSDParity/unity-trace.request.json";
        private const string ResultFile = "Temp/NTSDParity/unity-trace.result";
        private const string DefaultScenario = "Tools/NTSDParity/scenario.sample.json";
        private const string DefaultOutput = "Temp/NTSDParity/unity-trace-final.jsonl";
        private const string ProductionDataFixture = "production";
        private const string AuthorityDiagnosticDataFixture = "authority-dat-diagnostic";
        private const string DatPassword = "odBearBecauseHeIsVeryGoodSiuHungIsAGo";

        private static bool requestRunInProgress;

        static BattleParityTraceEditor()
        {
            EditorApplication.update += PollRequest;
        }

        [MenuItem("Tools/NTSD/Battle Parity/Run Sample Trace")]
        public static void RunSampleTrace()
        {
            RunAndWriteResult(
                DefaultScenario,
                DefaultOutput,
                "compact",
                ProductionDataFixture,
                exitBatchMode: false);
        }

        public static void RunFromCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();
            string scenarioPath = ReadArgument(args, "-ntsdParityScenario") ?? DefaultScenario;
            string outputPath = ReadArgument(args, "-ntsdParityOutput") ?? DefaultOutput;
            string detail = ReadArgument(args, "-ntsdParityDetail") ?? "compact";
            string dataFixture = ReadArgument(args, "-ntsdParityDataFixture") ?? ProductionDataFixture;
            RunAndWriteResult(
                scenarioPath,
                outputPath,
                detail,
                dataFixture,
                exitBatchMode: Application.isBatchMode);
        }

        private static void PollRequest()
        {
            if (requestRunInProgress || EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;

            string requestPath = ProjectPath(RequestFile);
            if (!File.Exists(requestPath))
                return;

            requestRunInProgress = true;
            try
            {
                string json = File.ReadAllText(requestPath, Encoding.UTF8);
                TraceRequest request = JsonUtility.FromJson<TraceRequest>(json) ?? new TraceRequest();
                string scenarioPath = string.IsNullOrWhiteSpace(request.scenarioPath)
                    ? DefaultScenario
                    : request.scenarioPath;
                string outputPath = string.IsNullOrWhiteSpace(request.outputPath)
                    ? DefaultOutput
                    : request.outputPath;
                string detail = string.IsNullOrWhiteSpace(request.detail) ? "compact" : request.detail;
                string dataFixture = string.IsNullOrWhiteSpace(request.dataFixture)
                    ? ProductionDataFixture
                    : request.dataFixture;
                RunAndWriteResult(scenarioPath, outputPath, detail, dataFixture, exitBatchMode: false);
            }
            finally
            {
                try
                {
                    File.Delete(requestPath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[BattleParityTraceEditor] Failed to delete request: {ex.Message}");
                }
                requestRunInProgress = false;
            }
        }

        private static void RunAndWriteResult(
            string scenarioPath,
            string outputPath,
            string detail,
            string dataFixture,
            bool exitBatchMode)
        {
            string resultPath = ProjectPath(ResultFile);
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath) ?? ProjectPath("Temp"));
            try
            {
                string resolvedOutput = RunScenario(scenarioPath, outputPath, detail, dataFixture);
                File.WriteAllText(resultPath, $"PASS{Environment.NewLine}{resolvedOutput}", new UTF8Encoding(false));
                Debug.Log($"[BattleParityTraceEditor] Trace written: {resolvedOutput}");
                if (exitBatchMode)
                    EditorApplication.Exit(0);
            }
            catch (Exception ex)
            {
                File.WriteAllText(resultPath, $"FAIL{Environment.NewLine}{ex}", new UTF8Encoding(false));
                Debug.LogError($"[BattleParityTraceEditor] Trace failed: {ex}");
                if (exitBatchMode)
                    EditorApplication.Exit(1);
            }
        }

        private static string RunScenario(
            string scenarioPath,
            string outputPath,
            string detail,
            string dataFixture)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Battle parity trace must run outside Play Mode.");
            if (detail != "compact" && detail != "full")
                throw new ArgumentException("Trace detail must be 'compact' or 'full'.", nameof(detail));
            if (dataFixture != ProductionDataFixture && dataFixture != AuthorityDiagnosticDataFixture)
                throw new ArgumentException(
                    $"Data fixture must be '{ProductionDataFixture}' or '{AuthorityDiagnosticDataFixture}'.",
                    nameof(dataFixture));

            string resolvedScenarioPath = ProjectPath(scenarioPath);
            string resolvedOutputPath = ProjectPath(outputPath);
            BattleTraceScenario scenario = JsonUtility.FromJson<BattleTraceScenario>(
                File.ReadAllText(resolvedScenarioPath, Encoding.UTF8));
            ValidateScenario(scenario);

            string indexPath = Path.Combine(Path.GetFullPath(scenario.gameRoot), "data", "data.txt");
            if (!File.Exists(indexPath))
                throw new FileNotFoundException("Authority data index not found.", indexPath);

            Directory.CreateDirectory(Path.GetDirectoryName(resolvedOutputPath) ?? ProjectPath("Temp"));
            string manifestSha256 = ResolveBattleLogicManifestSha256(
                scenario.gameRoot,
                indexPath,
                dataFixture);

            using var driverScope = new TemporarySimulationDriverScope();
            using var dataScope = new ExternalDatScope(scenario.gameRoot, indexPath);
            SimulationTickDriver driver = driverScope.Driver;
            SimulationWorld world = driver.World;
            ConfigureWorldAndRoster(world, dataScope, scenario);

            var provider = new ScenarioFrameInputProvider(scenario.inputs);
            driver.ApplySettings(new LockstepSimulationSettings
            {
                driveMode = SimulationDriveMode.Manual,
                enableFrameChecksum = true,
            });
            driver.SetFrameInputProvider(provider);
            driver.SetPaused(false);

            using var writer = new StreamWriter(resolvedOutputPath, false, new UTF8Encoding(false));
            writer.WriteLine(BuildHeaderJson(
                resolvedScenarioPath,
                scenario,
                dataScope.LoadedCount,
                manifestSha256,
                world,
                detail,
                dataFixture));

            bool full = detail == "full";
            for (int tick = 1; tick <= scenario.ticks; tick++)
            {
                if (!driver.StepOneTick(ignorePaused: true))
                    throw new InvalidOperationException($"Unity simulation did not advance tick {tick}.");

                BattleParityFrameSnapshot snapshot = full
                    ? world.CaptureParityFrameSnapshot(tick, provider.GetFrameInput(tick), includeFullDomains: true)
                    : driver.LastFrameSnapshot;
                if (snapshot == null || snapshot.Tick != tick)
                    throw new InvalidOperationException($"Unity parity snapshot missing for tick {tick}.");
                writer.WriteLine(snapshot.ToJson(full));
                writer.Flush();
            }

            return resolvedOutputPath;
        }

        private static void ConfigureWorldAndRoster(
            SimulationWorld world,
            ExternalDatScope dataScope,
            BattleTraceScenario scenario)
        {
            world.ResetRuntimeState();
            world.Rng.Seed(unchecked((uint)scenario.seed));

            BattleRuntimeState runtime = world.Runtime;
            runtime.Match.LocalGameModeId = 0;
            runtime.Match.BattleGameModeId = scenario.mode;
            runtime.Match.BackgroundId = scenario.randomStage;
            runtime.Match.Difficulty = scenario.difficulty;
            runtime.Match.Seed = 0;
            runtime.StageProgression.StageSeriesIdx = scenario.stage;
            runtime.StageProgression.WaveIdx = -1;
            runtime.StageProgression.Round = 0;
            runtime.StageProgression.RoundMax = 0;
            runtime.StageProgressionValid = false;
            runtime.Flow.AiPhaseGate = scenario.mode == 2 ? 1 : 0;

            ResolveBackgroundBounds(scenario.gameRoot, dataScope.DataManager, scenario.stage,
                out int stageWidth, out int zMin, out int zMax);
            world.SetExplicitStageRuntimeSnapshotForTesting(stageWidth, zMin, zMax, 0, 0);
            world.SetNeedClearInput(true);

            runtime.Roster.Reset();
            BattleTraceSlot[] slots = scenario.slots ?? Array.Empty<BattleTraceSlot>();
            foreach (BattleTraceSlot source in slots.OrderBy(value => value.playerSlot))
            {
                if (!source.active)
                    continue;
                if (!dataScope.Configs.TryGetValue(source.oid, out LF2CharacterDataWrapper wrapper))
                    throw new InvalidOperationException($"Scenario oid {source.oid} was not loaded from gameRoot.");

                int battleTeam = source.team == 0 ? 10 + source.playerSlot : source.team;
                BattleSlotRuntimeState rosterSlot = runtime.Roster.Slots[source.playerSlot];
                rosterSlot.Active = true;
                rosterSlot.IsHuman = !source.ai;
                rosterSlot.CharacterId = source.oid;
                rosterSlot.Team = battleTeam;
                rosterSlot.InputId = source.playerSlot;
                rosterSlot.AiId = source.ai ? source.playerSlot : -1;

                int xRange = stageWidth / 2;
                int x = stageWidth / 4 + (xRange > 0 ? world.Rng.NextRaw() % xRange : 0);
                int zRange = zMax - zMin;
                int z = (zRange > 0 ? world.Rng.NextRaw() % zRange : 0) + zMin;
                LF2Character character = CreateCharacter(world, wrapper, source, battleTeam, x, z);
                rosterSlot.RuntimeSlotIndex = character.Runtime.SlotIndex;
                rosterSlot.StableId = character.Runtime.StableId;
                runtime.Roster.ActiveSlotCount++;
            }
        }

        private static LF2Character CreateCharacter(
            SimulationWorld world,
            LF2CharacterDataWrapper wrapper,
            BattleTraceSlot slot,
            int battleTeam,
            int x,
            int z)
        {
            var character = new LF2Character();
            character.ModuleInitialize();
            character.ObjectId = slot.oid;
            character.Runtime.StableId = world.AllocateStableId();
            character.Runtime.X = x;
            character.Runtime.Y = 0.0;
            character.Runtime.Z = z;
            character.Runtime.XInt = x;
            character.Runtime.YInt = 0;
            character.Runtime.ZInt = z;
            character.ModuleBind(wrapper, slot.oid);
            if (character.Match != world)
                throw new InvalidOperationException($"Scenario oid {slot.oid} did not register into runner world.");

            character.Initialize(500, 500);
            character.Team = battleTeam;
            character.RelationTeam = battleTeam;
            character.AiControlled = slot.ai;
            character.RespawnCount = 0;
            character.HitStun = 75;
            character.Runtime.X = x;
            character.Runtime.Y = 0.0;
            character.Runtime.Z = z;
            character.Runtime.XInt = x;
            character.Runtime.YInt = 0;
            character.Runtime.ZInt = z;
            character.Runtime.Vx = 0.1;
            character.Runtime.Vy = 0.0;
            character.Runtime.Vz = 0.1;
            character.RefreshRuntimeSnapshot();
            return character;
        }

        private static void ResolveBackgroundBounds(
            string gameRoot,
            GameDataManager dataManager,
            int stage,
            out int width,
            out int zMin,
            out int zMax)
        {
            width = 800;
            zMin = 180;
            zMax = 350;
            BackgroundDefinition background = dataManager.GetBackgroundById(stage);
            if (background == null || string.IsNullOrWhiteSpace(background.file))
                return;

            string path = ResolveGameAssetPath(gameRoot, background.file);
            path = Path.ChangeExtension(path, ".dat");
            if (!File.Exists(path))
                return;

            string text = Lf2DatDecryptor.DecryptFile(path, DatPassword);
            Match widthMatch = Regex.Match(text ?? string.Empty, @"\bwidth\s*:\s*(-?\d+)", RegexOptions.IgnoreCase);
            Match zMatch = Regex.Match(text ?? string.Empty, @"\bzboundary\s*:\s*(-?\d+)\s+(-?\d+)", RegexOptions.IgnoreCase);
            if (widthMatch.Success)
                width = int.Parse(widthMatch.Groups[1].Value, CultureInfo.InvariantCulture);
            if (zMatch.Success)
            {
                zMin = int.Parse(zMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                zMax = int.Parse(zMatch.Groups[2].Value, CultureInfo.InvariantCulture);
            }
        }

        private static string BuildHeaderJson(
            string scenarioPath,
            BattleTraceScenario scenario,
            int loadedChars,
            string manifestSha256,
            SimulationWorld world,
            string detail,
            string dataFixture)
        {
            object header = DictionaryOf(
                ("buttonMask", (object)DictionaryOf(
                    ("attack", (object)(int)SimulationInputButtons.Attack),
                    ("defend", (int)SimulationInputButtons.Defend),
                    ("down", (int)SimulationInputButtons.Down),
                    ("jump", (int)SimulationInputButtons.Jump),
                    ("left", (int)SimulationInputButtons.Left),
                    ("right", (int)SimulationInputButtons.Right),
                    ("up", (int)SimulationInputButtons.Up))),
                ("dataFixture", dataFixture),
                ("detail", detail),
                ("expectedTicks", scenario.ticks),
                ("kind", "header"),
                ("loadedChars", loadedChars),
                ("manifest", DictionaryOf(
                    ("battleLogicSha256", (object)manifestSha256),
                    ("domain", "battle-logic"),
                    ("schema", "ntsd-resolved-dat-manifest-v2"))),
                ("maxRuntimeSlots", 400),
                ("rngAfterBootstrap", DictionaryOf(
                    ("callCount", (object)world.Rng.CallCount),
                    ("seed", world.Rng.State))),
                ("scenario", ProjectScenario(scenario)),
                ("scenarioName", Path.GetFileName(scenarioPath)),
                ("schema", "ntsd-battle-trace-v3"),
                ("stageFixture", DictionaryOf(
                    ("campaignCount", (object)0),
                    ("loaded", false),
                    ("name", null),
                    ("sha256", null))));
            return BattleCanonicalJson.Serialize(header);
        }

        private static object ProjectScenario(BattleTraceScenario scenario)
        {
            object[] slots = (scenario.slots ?? Array.Empty<BattleTraceSlot>())
                .Select(slot => DictionaryOf(
                    ("active", (object)slot.active),
                    ("ai", slot.ai),
                    ("oid", slot.oid),
                    ("playerSlot", slot.playerSlot),
                    ("team", slot.team)))
                .Cast<object>()
                .ToArray();
            object[] inputs = (scenario.inputs ?? Array.Empty<BattleTraceTickInput>())
                .Select(input => DictionaryOf(
                    ("players", (object)(input.players ?? Array.Empty<BattleTracePlayerInput>())
                        .Select(player => DictionaryOf(
                            ("buttonMask", (object)player.buttonMask),
                            ("playerSlot", player.playerSlot)))
                        .Cast<object>()
                        .ToArray()),
                    ("tick", input.tick)))
                .Cast<object>()
                .ToArray();
            return DictionaryOf(
                ("difficulty", (object)scenario.difficulty),
                ("inputs", inputs),
                ("mode", scenario.mode),
                ("randomStage", scenario.randomStage),
                ("seed", scenario.seed),
                ("slots", slots),
                ("stage", scenario.stage),
                ("ticks", scenario.ticks));
        }

        private static string ResolveBattleLogicManifestSha256(
            string gameRoot,
            string authorityIndex,
            string dataFixture)
        {
            string projectRoot = ProjectPath(string.Empty);
            string toolProject = Path.Combine(projectRoot, "Tools", "NTSDParity", "NTSDParity.csproj");
            string reportPath = Path.Combine(projectRoot, "Temp", "NTSDParity", "unity-runner-data-audit.json");
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? projectRoot);

            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = string.Join(" ", new[]
                {
                    "run",
                    "--project", Quote(toolProject),
                    "--", "data-audit",
                    "--authority-root", Quote(Path.GetFullPath(gameRoot)),
                    "--authority-index", Quote(authorityIndex),
                    "--unity-root", Quote(projectRoot),
                    "--output", Quote(reportPath),
                }),
                WorkingDirectory = projectRoot,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };
            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Failed to start NTSDParity data audit.");
            string stdout = process.StandardOutput.ReadToEnd();
            string stderr = process.StandardError.ReadToEnd();
            if (!process.WaitForExit(180000) || process.ExitCode != 0)
                throw new InvalidOperationException(
                    $"NTSDParity data audit failed. exit={process.ExitCode}\n{stdout}\n{stderr}");

            DataAuditEnvelope report = JsonUtility.FromJson<DataAuditEnvelope>(
                File.ReadAllText(reportPath, Encoding.UTF8));
            string manifest = dataFixture == AuthorityDiagnosticDataFixture
                ? report?.manifest?.authorityBattleLogicSha256
                : report?.manifest?.unityBattleLogicSha256;
            if (string.IsNullOrWhiteSpace(manifest))
                throw new InvalidDataException("Unity battle-logic manifest missing from data audit report.");
            return manifest;
        }

        private static void ValidateScenario(BattleTraceScenario scenario)
        {
            if (scenario == null)
                throw new InvalidDataException("Scenario JSON deserialized to null.");
            if (scenario.ticks <= 0)
                throw new ArgumentException("Scenario ticks must be positive.");
            if (string.IsNullOrWhiteSpace(scenario.gameRoot))
                throw new ArgumentException("Scenario gameRoot is required.");
            if (!string.IsNullOrWhiteSpace(scenario.stageFixture))
                throw new NotSupportedException("Explicit stage fixtures are not implemented by the Unity runner yet.");

            foreach (BattleTraceSlot slot in scenario.slots ?? Array.Empty<BattleTraceSlot>())
            {
                if (slot.playerSlot < 0 || slot.playerSlot >= 8)
                    throw new ArgumentOutOfRangeException(nameof(slot.playerSlot), slot.playerSlot, "Player slot must be 0..7.");
            }
            foreach (BattleTraceTickInput input in scenario.inputs ?? Array.Empty<BattleTraceTickInput>())
            {
                if (input.tick <= 0 || input.tick > scenario.ticks)
                    throw new ArgumentOutOfRangeException(nameof(input.tick), input.tick, "Input tick is outside scenario range.");
            }
        }

        private static SortedDictionary<string, object> DictionaryOf(params (string Key, object Value)[] items)
        {
            var result = new SortedDictionary<string, object>(StringComparer.Ordinal);
            foreach ((string key, object value) in items)
                result[key] = value;
            return result;
        }

        private static string ResolveGameAssetPath(string gameRoot, string indexedPath)
        {
            if (Path.IsPathRooted(indexedPath))
                return Path.GetFullPath(indexedPath);
            string normalized = indexedPath.Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar);
            return Path.GetFullPath(Path.Combine(gameRoot, normalized));
        }

        private static string ProjectPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return Path.GetFullPath(Directory.GetCurrentDirectory());
            return Path.IsPathRooted(path)
                ? Path.GetFullPath(path)
                : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), path));
        }

        private static string ReadArgument(string[] args, string name)
        {
            for (int i = 0; i + 1 < args.Length; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                    return args[i + 1];
            }
            return null;
        }

        private static string Quote(string value)
        {
            return $"\"{value.Replace("\"", "\\\"")}\"";
        }

        private sealed class ScenarioFrameInputProvider : ISimulationFrameInputProvider
        {
            private readonly Dictionary<int, FrameInputSet> inputs;

            public ScenarioFrameInputProvider(IEnumerable<BattleTraceTickInput> source)
            {
                inputs = (source ?? Array.Empty<BattleTraceTickInput>())
                    .GroupBy(item => item.tick)
                    .ToDictionary(
                        group => group.Key,
                        group => new FrameInputSet(
                            group.Key,
                            group.SelectMany(item => item.players ?? Array.Empty<BattleTracePlayerInput>())
                                .Select(player => new SimulationPlayerInput(
                                    player.playerSlot,
                                    (SimulationInputButtons)player.buttonMask))
                                .ToArray()));
            }

            public bool IsFrameInputReady(int tickIndex) => true;

            public FrameInputSet GetFrameInput(int tickIndex)
            {
                return inputs.TryGetValue(tickIndex, out FrameInputSet input)
                    ? input
                    : FrameInputSet.Empty(tickIndex);
            }
        }

        private sealed class TemporarySimulationDriverScope : IDisposable
        {
            private static readonly PropertyInfo InstanceProperty =
                typeof(SingletonBehaviour<SimulationTickDriver>).GetProperty(
                    "Instance",
                    BindingFlags.Public | BindingFlags.Static);

            private readonly SimulationTickDriver previousInstance;
            private readonly GameObject host;

            public TemporarySimulationDriverScope()
            {
                previousInstance = SimulationTickDriver.Instance;
                host = new GameObject("__NTSD_BattleParityTraceDriver")
                {
                    hideFlags = HideFlags.HideAndDontSave,
                };
                Driver = host.AddComponent<SimulationTickDriver>();
                SetInstance(Driver);
                Driver.RecreateWorld();
            }

            public SimulationTickDriver Driver { get; }

            public void Dispose()
            {
                SetInstance(null);
                if (host != null)
                    UnityEngine.Object.DestroyImmediate(host);
                SetInstance(previousInstance);
            }

            private static void SetInstance(SimulationTickDriver value)
            {
                MethodInfo setter = InstanceProperty?.GetSetMethod(nonPublic: true);
                if (setter == null)
                    throw new MissingMethodException("SimulationTickDriver singleton setter was not found.");
                setter.Invoke(null, new object[] { value });
            }
        }

        private sealed class ExternalDatScope : IDisposable
        {
            private readonly FieldInfo objectLookupField;
            private readonly FieldInfo cachedConfigField;
            private readonly FieldInfo frameConfigField;
            private readonly object originalObjectLookup;
            private readonly object originalCachedConfig;
            private readonly object originalFrameConfig;

            public ExternalDatScope(string gameRoot, string indexPath)
            {
                DataManager = GameDataManager.Instance
                    ?? throw new InvalidOperationException("GameDataManager singleton is unavailable.");
                CharacterAnimtorManager animationManager = CharacterAnimtorManager.Instance
                    ?? throw new InvalidOperationException("CharacterAnimtorManager singleton is unavailable.");

                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                objectLookupField = typeof(GameDataManager).GetField("objectLookup", flags);
                cachedConfigField = typeof(GameDataManager).GetField("cachedConfig", flags);
                frameConfigField = typeof(CharacterAnimtorManager).GetField("TotalCharacterFrameConfig", flags);
                if (objectLookupField == null || cachedConfigField == null || frameConfigField == null)
                    throw new MissingFieldException("External DAT cache fields were not found.");

                originalObjectLookup = objectLookupField.GetValue(DataManager);
                originalCachedConfig = cachedConfigField.GetValue(DataManager);
                originalFrameConfig = frameConfigField.GetValue(animationManager);

                objectLookupField.SetValue(DataManager, null);
                cachedConfigField.SetValue(DataManager, null);
                DataManager.LoadDataFile(indexPath);

                Configs = LoadAllConfigs(gameRoot, DataManager, animationManager);
                frameConfigField.SetValue(animationManager, Configs);
                LoadedCount = Configs.Count;
            }

            public GameDataManager DataManager { get; }
            public Dictionary<int, LF2CharacterDataWrapper> Configs { get; }
            public int LoadedCount { get; }

            public void Dispose()
            {
                CharacterAnimtorManager animationManager = CharacterAnimtorManager.Instance;
                if (animationManager != null)
                    frameConfigField.SetValue(animationManager, originalFrameConfig);
                objectLookupField.SetValue(DataManager, originalObjectLookup);
                cachedConfigField.SetValue(DataManager, originalCachedConfig);
            }

            private static Dictionary<int, LF2CharacterDataWrapper> LoadAllConfigs(
                string gameRoot,
                GameDataManager dataManager,
                CharacterAnimtorManager animationManager)
            {
                MethodInfo buildMethod = typeof(CharacterAnimtorManager).GetMethod(
                    "BuildCharacterDataFromDat",
                    BindingFlags.Instance | BindingFlags.NonPublic);
                if (buildMethod == null)
                    throw new MissingMethodException("CharacterAnimtorManager.BuildCharacterDataFromDat was not found.");

                var result = new Dictionary<int, LF2CharacterDataWrapper>();
                foreach (ObjectDefinition definition in dataManager.GetAllObjects().OrderBy(value => value.id))
                {
                    string datPath = Path.ChangeExtension(
                        ResolveGameAssetPath(gameRoot, definition.file),
                        ".dat");
                    if (!File.Exists(datPath))
                        continue;

                    string datText = Lf2DatDecryptor.DecryptFile(datPath, DatPassword);
                    Lf2DatFile datFile = new Lf2DatParserV2().Parse(datText, datPath);
                    if (datFile == null || datFile.Frames == null || datFile.Frames.Count == 0)
                        continue;

                    LF2CharacterData data = buildMethod.Invoke(
                        animationManager,
                        new object[] { datFile, Path.GetDirectoryName(datPath) }) as LF2CharacterData;
                    if (data == null)
                        continue;
                    if (data.type_sub == 0)
                        data.type_sub = definition.id;
                    result[definition.id] = new LF2CharacterDataWrapper(definition.id, data);
                }
                return result;
            }
        }

        [Serializable]
        private sealed class TraceRequest
        {
            public string scenarioPath;
            public string outputPath;
            public string detail = "compact";
            public string dataFixture = ProductionDataFixture;
        }

        [Serializable]
        private sealed class BattleTraceScenario
        {
            public int seed;
            public string gameRoot;
            public int mode = 1;
            public int difficulty = 1;
            public int stage;
            public int randomStage;
            public string stageFixture;
            public int ticks;
            public BattleTraceSlot[] slots;
            public BattleTraceTickInput[] inputs;
        }

        [Serializable]
        private sealed class BattleTraceSlot
        {
            public int playerSlot;
            public int oid;
            public int team = 1;
            public bool active;
            public bool ai;
        }

        [Serializable]
        private sealed class BattleTraceTickInput
        {
            public int tick;
            public BattleTracePlayerInput[] players;
        }

        [Serializable]
        private sealed class BattleTracePlayerInput
        {
            public int playerSlot;
            public int buttonMask;
        }

        [Serializable]
        private sealed class DataAuditEnvelope
        {
            public DataAuditManifest manifest;
        }

        [Serializable]
        private sealed class DataAuditManifest
        {
            public string authorityBattleLogicSha256;
            public string unityBattleLogicSha256;
        }
    }
}


--- File: Tools\NTSDParity\TraceCompareCommand.cs ---
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace NTSDParity;

internal static class TraceCompareCommand
{
    internal const string TraceSchema = "ntsd-battle-trace-v3";
    internal const int RuntimeSlotCount = 400;

    private static readonly string[] DomainNames =
    [
        "input",
        "rng",
        "world",
        "slots",
        "aRest",
        "vRest",
        "stats",
        "events",
    ];

    private static readonly SortedDictionary<string, int> ExpectedButtonMask = new(StringComparer.Ordinal)
    {
        ["right"] = 1,
        ["left"] = 2,
        ["up"] = 4,
        ["down"] = 8,
        ["attack"] = 16,
        ["jump"] = 32,
        ["defend"] = 64,
    };

    public static int Run(string[] args)
    {
        CommandLine cli = CommandLine.Parse(args);
        string authorityPath = Path.GetFullPath(cli.Require("--authority"));
        string unityPath = Path.GetFullPath(cli.Require("--unity"));
        string outputPath = RepositoryPaths.ResolveOutput(cli.Require("--output"));
        bool fullFieldDiff = string.Equals(cli.Get("--detail") ?? "hashes", "full", StringComparison.Ordinal);
        if ((cli.Get("--detail") ?? "hashes") is not ("hashes" or "full"))
            throw new ArgumentException("--detail must be 'hashes' or 'full'.");

        using StreamReader authority = new(authorityPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        using StreamReader unity = new(unityPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        TraceCompareReport report = Compare(
            authority,
            unity,
            Path.GetFileName(authorityPath),
            Path.GetFileName(unityPath),
            fullFieldDiff);

        File.WriteAllText(outputPath, JsonSerializer.Serialize(report, JsonProjection.SerializerOptions), new UTF8Encoding(false));
        Console.WriteLine(outputPath);
        Console.WriteLine(
            $"status={report.Status} certificate={report.CertificateEligible} " +
            $"ticksCompared={report.TicksCompared} firstDifferenceTick={report.FirstDifference?.Tick}");
        return report.Status.StartsWith("equal", StringComparison.Ordinal) ? 0 : 1;
    }

    internal static TraceCompareTestResult CompareTextForTest(string authority, string unity)
    {
        using StringReader authorityReader = new(authority);
        using StringReader unityReader = new(unity);
        TraceCompareReport report = Compare(authorityReader, unityReader, "authority", "unity", fullFieldDiff: true);
        return new TraceCompareTestResult(report.Status, report.FirstDifference?.Reason);
    }

    private static TraceCompareReport Compare(
        TextReader authorityReader,
        TextReader unityReader,
        string authorityName,
        string unityName,
        bool fullFieldDiff)
    {
        TraceCompareReport report = new()
        {
            Schema = "ntsd-streaming-trace-compare-v2",
            Authority = authorityName,
            Unity = unityName,
        };

        string? authorityHeaderLine = ReadNextLine(authorityReader);
        string? unityHeaderLine = ReadNextLine(unityReader);
        if (authorityHeaderLine is null || unityHeaderLine is null)
            return Fail(report, "header", 0, "missing-header", authorityHeaderLine, unityHeaderLine, fullFieldDiff);

        HeaderContract authorityHeader;
        HeaderContract unityHeader;
        try
        {
            authorityHeader = ValidateHeader(authorityHeaderLine, "authority");
            unityHeader = ValidateHeader(unityHeaderLine, "unity");
        }
        catch (Exception ex)
        {
            return Fail(report, "header", 0, "invalid-header: " + ex.Message, authorityHeaderLine, unityHeaderLine, fullFieldDiff);
        }

        report.AuthorityManifestSha256 = authorityHeader.Manifest;
        report.UnityManifestSha256 = unityHeader.Manifest;
        report.AuthorityDetail = authorityHeader.Detail;
        report.UnityDetail = unityHeader.Detail;
        report.ExpectedTicks = authorityHeader.ExpectedTicks;

        string? headerMismatch = CompareHeaders(authorityHeader, unityHeader);
        if (headerMismatch is not null)
            return Fail(report, "header", 0, headerMismatch, authorityHeaderLine, unityHeaderLine, fullFieldDiff);

        for (int expectedTick = 1; expectedTick <= authorityHeader.ExpectedTicks; expectedTick++)
        {
            string? authorityLine = ReadNextLine(authorityReader);
            string? unityLine = ReadNextLine(unityReader);
            if (authorityLine is null || unityLine is null)
                return Fail(report, "stream", expectedTick, "missing-required-tick", authorityLine, unityLine, fullFieldDiff);

            ValidatedTick authorityTick;
            ValidatedTick unityTick;
            try
            {
                authorityTick = ValidateTick(authorityLine, authorityHeader, expectedTick, "authority");
            }
            catch (Exception ex)
            {
                return Fail(report, "authority", expectedTick, "invalid-tick: " + ex.Message, authorityLine, unityLine, fullFieldDiff);
            }
            try
            {
                unityTick = ValidateTick(unityLine, unityHeader, expectedTick, "unity");
            }
            catch (Exception ex)
            {
                return Fail(report, "unity", expectedTick, "invalid-tick: " + ex.Message, authorityLine, unityLine, fullFieldDiff);
            }

            foreach (string domain in DomainNames.Append("overall"))
            {
                string authorityHash = authorityTick.Hashes[domain];
                string unityHash = unityTick.Hashes[domain];
                if (!string.Equals(authorityHash, unityHash, StringComparison.Ordinal))
                    return Fail(report, domain, expectedTick, "domain-mismatch", authorityLine, unityLine, fullFieldDiff);
            }

            foreach (string domain in DomainNames.Where(value => value != "slots"))
            {
                if (!JsonNode.DeepEquals(authorityTick.CanonicalDomains[domain], unityTick.CanonicalDomains[domain]))
                    return Fail(report, domain, expectedTick, "domain-body-mismatch", authorityLine, unityLine, fullFieldDiff);
            }
            if (authorityHeader.Detail == "full" && unityHeader.Detail == "full" &&
                !JsonNode.DeepEquals(authorityTick.OpenedSlotBodies, unityTick.OpenedSlotBodies))
            {
                return Fail(report, "slots", expectedTick, "slot-body-mismatch", authorityLine, unityLine, fullFieldDiff);
            }

            report.TicksCompared++;
        }

        string? authorityExtra = ReadNextLine(authorityReader);
        string? unityExtra = ReadNextLine(unityReader);
        if (authorityExtra is not null || unityExtra is not null)
            return Fail(report, "stream", authorityHeader.ExpectedTicks + 1, "unexpected-extra-tick", authorityExtra, unityExtra, fullFieldDiff);

        report.CertificateEligible = authorityHeader.Detail == "full" && unityHeader.Detail == "full";
        report.Status = report.CertificateEligible ? "equal" : "equal-commitments";
        return report;
    }

    private static HeaderContract ValidateHeader(string line, string producer)
    {
        JsonObject header = ParseObject(line, producer + " header");
        RequireString(header, "kind", "header");
        RequireString(header, "schema", TraceSchema);

        int expectedTicks = RequireInt(header, "expectedTicks");
        if (expectedTicks <= 0)
            throw new InvalidDataException("expectedTicks must be positive");
        int maxRuntimeSlots = RequireInt(header, "maxRuntimeSlots");
        if (maxRuntimeSlots != RuntimeSlotCount)
            throw new InvalidDataException($"maxRuntimeSlots must be {RuntimeSlotCount}");
        int loadedChars = RequireInt(header, "loadedChars");
        if (loadedChars <= 0)
            throw new InvalidDataException("loadedChars must be positive");

        string detail = RequireString(header, "detail");
        if (detail is not ("compact" or "full"))
            throw new InvalidDataException("detail must be compact or full");

        JsonObject scenario = RequireObject(header, "scenario");
        if (RequireInt(scenario, "ticks") != expectedTicks)
            throw new InvalidDataException("scenario.ticks does not match expectedTicks");

        JsonObject manifest = RequireObject(header, "manifest");
        RequireString(manifest, "schema", "ntsd-resolved-dat-manifest-v2");
        RequireString(manifest, "domain", "battle-logic");
        string manifestHash = RequireHash(manifest, "battleLogicSha256");

        JsonObject buttonMask = RequireObject(header, "buttonMask");
        if (!JsonNode.DeepEquals(CanonicalJson.Canonicalize(buttonMask), CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(ExpectedButtonMask))))
            throw new InvalidDataException("buttonMask does not match the v3 contract");

        JsonObject rng = RequireObject(header, "rngAfterBootstrap");
        _ = RequireUInt(rng, "seed");
        if (RequireLong(rng, "callCount") < 0)
            throw new InvalidDataException("rngAfterBootstrap.callCount must be nonnegative");

        JsonObject stageFixture = RequireObject(header, "stageFixture");
        bool fixtureLoaded = RequireBool(stageFixture, "loaded");
        int fixtureCampaignCount = RequireInt(stageFixture, "campaignCount");
        if (fixtureCampaignCount < 0)
            throw new InvalidDataException("stageFixture.campaignCount must be nonnegative");
        if (fixtureLoaded)
            _ = RequireHash(stageFixture, "sha256");

        return new HeaderContract
        {
            ExpectedTicks = expectedTicks,
            Detail = detail,
            LoadedChars = loadedChars,
            Manifest = manifestHash,
            ScenarioHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(scenario)),
            StageFixtureHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(stageFixture)),
            ButtonMaskHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(buttonMask)),
            BootstrapRngHash = CanonicalJson.Sha256(CanonicalJson.Canonicalize(rng)),
        };
    }

    private static string? CompareHeaders(HeaderContract authority, HeaderContract unity)
    {
        if (authority.ExpectedTicks != unity.ExpectedTicks)
            return "expectedTicks";
        if (authority.LoadedChars != unity.LoadedChars)
            return "loadedChars";
        if (!string.Equals(authority.Manifest, unity.Manifest, StringComparison.Ordinal))
            return "manifest";
        if (!string.Equals(authority.ScenarioHash, unity.ScenarioHash, StringComparison.Ordinal))
            return "scenario";
        if (!string.Equals(authority.StageFixtureHash, unity.StageFixtureHash, StringComparison.Ordinal))
            return "stageFixture";
        if (!string.Equals(authority.ButtonMaskHash, unity.ButtonMaskHash, StringComparison.Ordinal))
            return "buttonMask";
        if (!string.Equals(authority.BootstrapRngHash, unity.BootstrapRngHash, StringComparison.Ordinal))
            return "rngAfterBootstrap";
        return null;
    }

    private static ValidatedTick ValidateTick(
        string line,
        HeaderContract header,
        int expectedTick,
        string producer)
    {
        JsonObject tick = ParseObject(line, producer + " tick");
        RequireString(tick, "kind", "tick");
        int actualTick = RequireInt(tick, "tick");
        if (actualTick != expectedTick)
            throw new InvalidDataException($"tick index {actualTick} is not expected contiguous tick {expectedTick}");

        JsonObject worldBody = RequireObject(tick, "world");
        int topLevelObjectCount = RequireInt(tick, "objectCount");
        if (RequireInt(worldBody, "objectCount") != topLevelObjectCount)
            throw new InvalidDataException("top-level objectCount does not match world.objectCount");

        Dictionary<string, JsonNode?> domains = new(StringComparer.Ordinal)
        {
            ["input"] = CanonicalJson.Canonicalize(RequireNode(tick, "input")),
            ["rng"] = CanonicalJson.Canonicalize(RequireNode(tick, "rng")),
            ["world"] = CanonicalJson.Canonicalize(worldBody),
            ["aRest"] = NormalizeARest(RequireObject(tick, "aRest")),
            ["vRest"] = NormalizeVRest(RequireObject(tick, "vRest")),
            ["stats"] = CanonicalJson.Canonicalize(RequireNode(tick, "stats")),
            ["events"] = CanonicalJson.Canonicalize(RequireNode(tick, "events")),
        };
        SlotValidation slots = ValidateSlots(tick, header.Detail);
        domains["slots"] = slots.CommitmentDomain;

        SortedDictionary<string, string> computed = new(StringComparer.Ordinal);
        foreach (string domain in DomainNames)
            computed[domain] = CanonicalJson.Sha256(domains[domain]);
        computed["overall"] = CanonicalJson.Sha256(computed);

        JsonObject reported = RequireObject(tick, "hashes");
        foreach (string domain in DomainNames.Append("overall"))
        {
            string reportedHash = RequireHash(reported, domain);
            if (!string.Equals(reportedHash, computed[domain], StringComparison.Ordinal))
            {
                throw new InvalidDataException(
                    $"{domain} body hash mismatch (reported {reportedHash}, computed {computed[domain]})");
            }
        }

        return new ValidatedTick(computed, domains, slots.OpenedBodies);
    }

    private static SlotValidation ValidateSlots(JsonObject tick, string detail)
    {
        JsonArray commitmentsNode = RequireArray(tick, "slotCommitments");
        if (commitmentsNode.Count != RuntimeSlotCount)
            throw new InvalidDataException($"slotCommitments must contain {RuntimeSlotCount} entries");

        string[] commitments = new string[RuntimeSlotCount];
        for (int slot = 0; slot < RuntimeSlotCount; slot++)
        {
            commitments[slot] = commitmentsNode[slot]?.GetValue<string>()
                ?? throw new InvalidDataException($"slot commitment {slot} is not a string");
            ValidateHash(commitments[slot], $"slot commitment {slot}");
        }

        JsonArray slots = RequireArray(tick, "slots");
        bool[] opened = new bool[RuntimeSlotCount];
        JsonNode?[] openedBodies = new JsonNode?[RuntimeSlotCount];
        foreach (JsonNode? slotNode in slots)
        {
            if (slotNode is not JsonObject slotObject)
                throw new InvalidDataException("slot body must be an object");
            int slot = RequireInt(slotObject, "runtimeSlot");
            if (slot < 0 || slot >= RuntimeSlotCount || opened[slot])
                throw new InvalidDataException($"invalid or duplicate runtime slot {slot}");
            opened[slot] = true;
            JsonNode? canonicalBody = CanonicalJson.Canonicalize(slotObject);
            openedBodies[slot] = canonicalBody;
            string bodyHash = CanonicalJson.Sha256(canonicalBody);
            if (!string.Equals(bodyHash, commitments[slot], StringComparison.Ordinal))
                throw new InvalidDataException($"slot {slot} body does not match its commitment");
        }

        if (detail == "full" && (slots.Count != RuntimeSlotCount || opened.Any(value => !value)))
            throw new InvalidDataException("full trace must open all 400 runtime slot commitments");

        JsonNode commitmentDomain = CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["count"] = RuntimeSlotCount,
            ["commitments"] = commitments,
        }))!;
        JsonNode openedDomain = CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(openedBodies))!;
        return new SlotValidation(commitmentDomain, openedDomain);
    }

    private static JsonNode NormalizeARest(JsonObject source)
    {
        if (RequireInt(source, "dimension") != RuntimeSlotCount)
            throw new InvalidDataException("aRest dimension must be 400");
        string encoding = RequireString(source, "encoding");
        SortedDictionary<int, int> values = new();
        if (encoding == "sparse-nonzero")
        {
            foreach (JsonNode? node in RequireArray(source, "entries"))
            {
                JsonObject entry = node as JsonObject ?? throw new InvalidDataException("aRest entry must be an object");
                int slot = RequireInt(entry, "slot");
                int value = RequireInt(entry, "value");
                if (slot < 0 || slot >= RuntimeSlotCount || value == 0 || !values.TryAdd(slot, value))
                    throw new InvalidDataException("invalid aRest sparse entry");
            }
        }
        else if (encoding == "full")
        {
            JsonArray full = RequireArray(source, "values");
            if (full.Count != RuntimeSlotCount)
                throw new InvalidDataException("aRest full values must contain 400 entries");
            for (int slot = 0; slot < RuntimeSlotCount; slot++)
            {
                int value = full[slot]?.GetValue<int>() ?? throw new InvalidDataException("invalid aRest value");
                if (value != 0)
                    values.Add(slot, value);
            }
        }
        else
        {
            throw new InvalidDataException("unsupported aRest encoding");
        }

        return CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = RuntimeSlotCount,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = values.Select(pair => new { slot = pair.Key, value = pair.Value }).ToArray(),
        }))!;
    }

    private static JsonNode NormalizeVRest(JsonObject source)
    {
        if (RequireInt(source, "dimension") != RuntimeSlotCount)
            throw new InvalidDataException("vRest dimension must be 400");
        string encoding = RequireString(source, "encoding");
        SortedDictionary<(int First, int Second), int> values = new();
        if (encoding == "sparse-nonzero")
        {
            foreach (JsonNode? node in RequireArray(source, "entries"))
            {
                JsonObject entry = node as JsonObject ?? throw new InvalidDataException("vRest entry must be an object");
                int first = RequireInt(entry, "attackerSlot");
                int second = RequireInt(entry, "victimSlot");
                int value = RequireInt(entry, "value");
                if (first < 0 || first >= RuntimeSlotCount || second < 0 || second >= RuntimeSlotCount ||
                    value == 0 || !values.TryAdd((first, second), value))
                {
                    throw new InvalidDataException("invalid vRest sparse entry");
                }
            }
        }
        else if (encoding == "full-row-major")
        {
            JsonArray rows = RequireArray(source, "values");
            if (rows.Count != RuntimeSlotCount)
                throw new InvalidDataException("vRest full matrix must contain 400 rows");
            for (int first = 0; first < RuntimeSlotCount; first++)
            {
                JsonArray row = rows[first] as JsonArray ?? throw new InvalidDataException("vRest row must be an array");
                if (row.Count != RuntimeSlotCount)
                    throw new InvalidDataException("vRest full row must contain 400 entries");
                for (int second = 0; second < RuntimeSlotCount; second++)
                {
                    int value = row[second]?.GetValue<int>() ?? throw new InvalidDataException("invalid vRest value");
                    if (value != 0)
                        values.Add((first, second), value);
                }
            }
        }
        else
        {
            throw new InvalidDataException("unsupported vRest encoding");
        }

        return CanonicalJson.Canonicalize(JsonSerializer.SerializeToNode(new SortedDictionary<string, object?>(StringComparer.Ordinal)
        {
            ["dimension"] = RuntimeSlotCount,
            ["encoding"] = "sparse-nonzero",
            ["entries"] = values.Select(pair => new
            {
                attackerSlot = pair.Key.First,
                victimSlot = pair.Key.Second,
                value = pair.Value,
            }).ToArray(),
        }))!;
    }

    private static TraceCompareReport Fail(
        TraceCompareReport report,
        string domain,
        int tick,
        string reason,
        string? authorityLine,
        string? unityLine,
        bool fullFieldDiff)
    {
        report.Status = "different";
        report.CertificateEligible = false;
        report.FirstDifference = new TraceDifference { Tick = tick, Domain = domain, Reason = reason };
        if (fullFieldDiff && authorityLine is not null && unityLine is not null)
        {
            try
            {
                CanonicalJson.CompareNodes(JsonNode.Parse(authorityLine), JsonNode.Parse(unityLine), "$", report.FirstDifference.Fields, 512);
                report.FirstDifference.FieldDiffTruncated = report.FirstDifference.Fields.Count >= 512;
            }
            catch (JsonException)
            {
                // The contract error already identifies malformed JSON.
            }
        }
        return report;
    }

    private static JsonObject ParseObject(string json, string description)
        => JsonNode.Parse(json) as JsonObject ?? throw new InvalidDataException(description + " must be a JSON object");

    private static JsonNode RequireNode(JsonObject owner, string property)
        => owner[property] ?? throw new InvalidDataException($"missing property '{property}'");

    private static JsonObject RequireObject(JsonObject owner, string property)
        => owner[property] as JsonObject ?? throw new InvalidDataException($"property '{property}' must be an object");

    private static JsonArray RequireArray(JsonObject owner, string property)
        => owner[property] as JsonArray ?? throw new InvalidDataException($"property '{property}' must be an array");

    private static string RequireString(JsonObject owner, string property)
        => owner[property]?.GetValue<string>() ?? throw new InvalidDataException($"property '{property}' must be a string");

    private static string RequireString(JsonObject owner, string property, string expected)
    {
        string actual = RequireString(owner, property);
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidDataException($"property '{property}' must be '{expected}'");
        return actual;
    }

    private static int RequireInt(JsonObject owner, string property)
        => owner[property]?.GetValue<int>() ?? throw new InvalidDataException($"property '{property}' must be an integer");

    private static uint RequireUInt(JsonObject owner, string property)
        => owner[property]?.GetValue<uint>() ?? throw new InvalidDataException($"property '{property}' must be an unsigned integer");

    private static long RequireLong(JsonObject owner, string property)
        => owner[property]?.GetValue<long>() ?? throw new InvalidDataException($"property '{property}' must be an integer");

    private static bool RequireBool(JsonObject owner, string property)
        => owner[property]?.GetValue<bool>() ?? throw new InvalidDataException($"property '{property}' must be a boolean");

    private static string RequireHash(JsonObject owner, string property)
    {
        string hash = RequireString(owner, property);
        ValidateHash(hash, property);
        return hash;
    }

    private static void ValidateHash(string hash, string description)
    {
        if (hash.Length != 64 || hash.Any(value => !char.IsAsciiHexDigit(value)) ||
            !string.Equals(hash, hash.ToLowerInvariant(), StringComparison.Ordinal))
        {
            throw new InvalidDataException(description + " must be a lowercase SHA-256 hex digest");
        }
    }

    private static string? ReadNextLine(TextReader reader)
    {
        while (reader.ReadLine() is { } line)
        {
            if (!string.IsNullOrWhiteSpace(line))
                return line;
        }
        return null;
    }

    private sealed class HeaderContract
    {
        public int ExpectedTicks { get; set; }
        public int LoadedChars { get; set; }
        public string Detail { get; set; } = string.Empty;
        public string Manifest { get; set; } = string.Empty;
        public string ScenarioHash { get; set; } = string.Empty;
        public string StageFixtureHash { get; set; } = string.Empty;
        public string ButtonMaskHash { get; set; } = string.Empty;
        public string BootstrapRngHash { get; set; } = string.Empty;
    }

    private sealed record ValidatedTick(
        SortedDictionary<string, string> Hashes,
        Dictionary<string, JsonNode?> CanonicalDomains,
        JsonNode? OpenedSlotBodies);

    private sealed record SlotValidation(JsonNode CommitmentDomain, JsonNode OpenedBodies);

    private sealed class TraceCompareReport
    {
        public string Schema { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool CertificateEligible { get; set; }
        public string Authority { get; set; } = string.Empty;
        public string Unity { get; set; } = string.Empty;
        public string? AuthorityManifestSha256 { get; set; }
        public string? UnityManifestSha256 { get; set; }
        public string? AuthorityDetail { get; set; }
        public string? UnityDetail { get; set; }
        public int ExpectedTicks { get; set; }
        public int TicksCompared { get; set; }
        public TraceDifference? FirstDifference { get; set; }
    }

    private sealed class TraceDifference
    {
        public int Tick { get; set; }
        public string Domain { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public bool FieldDiffTruncated { get; set; }
        public List<FieldDifference> Fields { get; set; } = [];
    }
}

internal sealed record TraceCompareTestResult(string Status, string? Reason);


--- File: Temp\NTSDParity\compare-v3-diagnostic-full-iter2.json ---
{
  "schema": "ntsd-streaming-trace-compare-v2",
  "status": "different",
  "certificateEligible": false,
  "authority": "authority-v3-full-final.jsonl",
  "unity": "unity-trace-v3-diagnostic-full-iter2.jsonl",
  "authorityManifestSha256": "41c088d2a746800752d448c6b0e5e32d4d00c8214447787f9924c90b42a80375",
  "unityManifestSha256": "41c088d2a746800752d448c6b0e5e32d4d00c8214447787f9924c90b42a80375",
  "authorityDetail": "full",
  "unityDetail": "full",
  "expectedTicks": 6,
  "ticksCompared": 1,
  "firstDifference": {
    "tick": 2,
    "domain": "world",
    "reason": "domain-mismatch",
    "fieldDiffTruncated": false,
    "fields": [
      {
        "path": "$.hashes.overall",
        "authority": "\u00225b2c5235bd4deda85f1f6e561e28c4c4b35292a0f026647adaa00070ea1eb264\u0022",
        "unity": "\u0022359922c725b4908d4aea2d45c335b0074511670009ee80889b3c9f8c4f2f46ec\u0022"
      },
      {
        "path": "$.hashes.slots",
        "authority": "\u00226f99f8822fa61bd538950c6fcf13d5c032ffd9e484c2c0b335b130353650444d\u0022",
        "unity": "\u0022fb139c0b528ca5efde004ebe020dfb83858872fbd88ec3c297b2604c8fb5394e\u0022"
      },
      {
        "path": "$.hashes.world",
        "authority": "\u002248d1e635b4f6c9b7d17cec5e91f2cf9409f123052d9b458d21281c43214fb3a4\u0022",
        "unity": "\u0022d7eb47085873ca8361bf98df61edab3c8ff5cf89a2c705919bc959b3792bbf68\u0022"
      },
      {
        "path": "$.slotCommitments[0]",
        "authority": "\u00227556d850665d16d699d0e214e80d8cf9096c68b17de21acc1ca18d7d2ce30d4b\u0022",
        "unity": "\u002285ca02f380b3c12043ceeb45c4d4523f163e9ebca713b8e186796a18ff3e4a7f\u0022"
      },
      {
        "path": "$.slots[0].runtime.frame.animCounter",
        "authority": "0",
        "unity": "1"
      },
      {
        "path": "$.slots[0].runtime.frame.animSub",
        "authority": "0",
        "unity": "-10"
      },
      {
        "path": "$.slots[0].runtime.frame.frame",
        "authority": "0",
        "unity": "5"
      },
      {
        "path": "$.slots[0].runtime.frame.frameWaitCounter",
        "authority": "0",
        "unity": "5"
      },
      {
        "path": "$.slots[0].runtime.frame.prevFrame",
        "authority": "0",
        "unity": "5"
      },
      {
        "path": "$.slots[0].runtime.frame.prevFrame2",
        "authority": "0",
        "unity": "5"
      },
      {
        "path": "$.slots[0].runtime.frame.waitCounter",
        "authority": "0",
        "unity": "5"
      },
      {
        "path": "$.slots[0].runtime.input.cdLeft",
        "authority": "4",
        "unity": "5"
      },
      {
        "path": "$.slots[0].runtime.input.prevLeft",
        "authority": "1",
        "unity": "0"
      },
      {
        "path": "$.slots[0].runtime.motion.vx",
        "authority": "0",
        "unity": "-3"
      },
      {
        "path": "$.slots[0].runtime.transform.facing",
        "authority": "0",
        "unity": "1"
      },
      {
        "path": "$.slots[0].runtime.transform.x",
        "authority": "569.1",
        "unity": "565"
      },
      {
        "path": "$.slots[0].runtime.transform.xInt",
        "authority": "569",
        "unity": "565"
      },
      {
        "path": "$.world.cameraVel",
        "authority": "1",
        "unity": "0"
      },
      {
        "path": "$.world.cameraX",
        "authority": "1",
        "unity": "0"
      },
      {
        "path": "$.world.results.hadBoth",
        "authority": "true",
        "unity": "false"
      },
      {
        "path": "$.world.results.teamCount",
        "authority": "2",
        "unity": "0"
      },
      {
        "path": "$.world.results.teamIds[0]",
        "authority": "1",
        "unity": "-1"
      },
      {
        "path": "$.world.results.teamIds[1]",
        "authority": "2",
        "unity": "-1"
      },
      {
        "path": "$.world.runtime.stage.cameraVel",
        "authority": "1",
        "unity": "0"
      },
      {
        "path": "$.world.runtime.stage.cameraX",
        "authority": "1",
        "unity": "0"
      }
    ]
  }
}

[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# NTSD battle parity architecture review

Review the current full per-tick parity effort between the authoritative C# project and Unity.

Authoritative project: `J:\QQFile\NTSD2.4\ntsd_release_C#`
Unity project: current working directory.

Focus on the current tick-2 first difference and the proposed fixes:

1. Authority `SimulationTickDriver.ApplyFrameInput` invokes `InputRuntime.PollHumanInput` before `GameTick.Run`; `PollHumanInput` rolls previous state, writes held state, ticks cooldowns, then applies edges. Unity currently queues a complete frame input set before the tick and consumes it in the post-cooldown human input phase. Determine the correct Unity production contract that preserves LocalFreeRun, LockstepBuffered, Manual/replay, and the existing combo input path without double-consuming edges.
2. Authority world camera fields change, while Unity intentionally uses a fixed-world camera and must not restore player-driven world/camera movement. Review whether a named comparison profile may normalize only cameraX/cameraVel while retaining all entity/world combat fields.
3. Authority `UpdateBattleResultsFlow` writes HadBoth, TeamCount, TeamIds, BattleEndPhase, and PendingWinner during normal combat. Unity trace currently emits constants. Determine the minimum real Unity battle-results runtime required for combat simulation parity while excluding results UI/menu behavior.
4. Inspect the v3 trace/hash/manifest boundary for any way a diagnostic mode could be mistaken for a production certificate. Confirm that production DAT manifest mismatch remains a hard rejection.
5. Identify likely regressions or missing focused tests, with exact file references where possible.

Do not modify files. Produce an evidence-based review with prioritized findings and a clear PASS/FAIL gate for the proposed iteration.
