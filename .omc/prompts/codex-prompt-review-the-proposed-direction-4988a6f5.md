---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "I:\\GitHub\\ZhiHu_MD\\knowledge_books\\prj_68c246da390142ca9eb02359636ea6a3\\topics\\tpc_15abb4a7a2314496818979f4a6810984\\artifacts\\revision-3\\topic.md"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Simulation\\SimulationTickDriver.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Simulation\\Input\\FrameInputSet.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\Scripts\\Simulation\\BattleParitySnapshot.cs"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\NTSD_Lockstep_Framework_Plan.md"
  - "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Assets\\NTSD\\NTSD_Lockstep_Risk_Assessment.md"
timestamp: "2026-07-27T00:15:17.359Z"
---

[BLOCKED] File 'I:\GitHub\ZhiHu_MD\knowledge_books\prj_68c246da390142ca9eb02359636ea6a3\topics\tpc_15abb4a7a2314496818979f4a6810984\artifacts\revision-3\topic.md' is outside the working directory. Only files within the project are allowed.

--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Simulation\SimulationTickDriver.cs ---
﻿using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Presentation;
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
        public const int LocalFreeRunMinCatchUpTicks = 4;

        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("单个 Unity 渲染帧最多追多少个逻辑帧。本地模式必须允许有限追帧，避免渲染帧率低于 30 FPS 时拖慢战斗时钟。")]
        public int maxCatchUpTicksPerFrame = LocalFreeRunMinCatchUpTicks;

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
            int minimumCatchUp = driveMode == SimulationDriveMode.LocalFreeRun
                ? LocalFreeRunMinCatchUpTicks
                : 1;
            if (maxCatchUpTicksPerFrame < minimumCatchUp)
                maxCatchUpTicksPerFrame = minimumCatchUp;
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
        private NTSD.Animation.BattleEntityOverlayRenderer _overlayRenderer;
        private BattlePresentationBackendMode _presentationBackendMode =
            BattlePresentationBackendMode.CentralOnly;

        private int _sparkRenderFrame = 0;
        private ISimulationFrameInputProvider _frameInputProvider = new LocalSimulationFrameInputProvider();
        private FrameInputSet _lastAppliedFrameInput = FrameInputSet.Empty(0);
        private BattleParityFrameSnapshot _lastFrameSnapshot;
        private IBattleChecksumSnapshot _lastChecksumSnapshot;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;
            lockstepSettings ??= new LockstepSimulationSettings();
            lockstepSettings.Normalize();

            CreateProductionWorld();

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
                bool buildPresentation = ShouldBuildPresentationForCatchUpTick(
                    lockstepSettings.driveMode,
                    lockstepSettings.requireInputFrameReady,
                    _timeAccumulator,
                    catchUpTicks,
                    lockstepSettings.maxCatchUpTicksPerFrame);
                StepOneTickInternal(nextTickIndex, buildPresentation);
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
            if (_overlayRenderer == null)
                _overlayRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.BattleEntityOverlayRenderer>();
            _overlayRenderer.RenderAll(_world);

            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                    _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
            }

            _sparkRenderer.RenderAll(_world);
            _world?.BattlePresentation.FinalizePublishedHitRecordCycle(_world);
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

        private bool StepOneTickInternal(int tickIndex, bool buildPresentation)
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
            _battleTickSystem?.RunReleaseTick(tickIndex, buildPresentation);
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
                _lastChecksumSnapshot = null;
                lastFrameChecksum = string.Empty;
                return;
            }

            _lastChecksumSnapshot = CaptureSupportedChecksumSnapshot(_world, tickIndex, frameInput);
            _lastFrameSnapshot = _lastChecksumSnapshot as BattleParityFrameSnapshot;
            lastFrameChecksum = _lastChecksumSnapshot?.OverallChecksum ?? string.Empty;
        }

        internal static bool SupportsAuthorityFrameChecksum(SimulationWorld world)
        {
            return world != null &&
                   world.RuntimeProfileForServices == BattleRuntimeProfile.Authority400 &&
                   world.MaxRuntimeSlotsForServices == SimulationWorld.AuthorityRuntimeSlotCapacity;
        }

        internal static BattleParityFrameSnapshot CaptureSupportedFrameSnapshot(
            SimulationWorld world,
            int tickIndex,
            FrameInputSet frameInput)
        {
            return SupportsAuthorityFrameChecksum(world)
                ? world.CaptureParityFrameSnapshot(tickIndex, frameInput)
                : null;
        }

        internal static bool SupportsFrameChecksum(SimulationWorld world)
        {
            if (world == null)
                return false;

            return SupportsAuthorityFrameChecksum(world) ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended;
        }

        internal static IBattleChecksumSnapshot CaptureSupportedChecksumSnapshot(
            SimulationWorld world,
            int tickIndex,
            FrameInputSet frameInput)
        {
            if (world == null)
                return null;

            if (SupportsAuthorityFrameChecksum(world))
                return world.CaptureParityFrameSnapshot(tickIndex, frameInput);

            return world.RuntimeProfileForServices == BattleRuntimeProfile.MobileExtended ||
                   world.RuntimeProfileForServices == BattleRuntimeProfile.DesktopExtended
                ? world.CaptureExtendedChecksumSnapshot(tickIndex, frameInput)
                : null;
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
        public IBattleChecksumSnapshot LastChecksumSnapshot => _lastChecksumSnapshot;
        public bool HasFrameChecksum => _lastChecksumSnapshot != null;
        public string LastFrameChecksum => lastFrameChecksum;
        public BattlePresentationBackendMode PresentationBackendMode => _presentationBackendMode;

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
            if (!EnsureRuntimeProfileFromSources())
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
            _world.Runtime?.ApplyBootstrapFromMatchConfig(config);
            _world.SetNeedClearInput(true);
            _world.RefreshStageRuntimeSnapshotFromScene();

            List<BattleStageCampaignData> stageCampaigns = BattleStageCampaignLoader.LoadFromFile(
                config?.stageCampaignFilePath);
            _world.ConfigureStageCampaigns(stageCampaigns, config?.stageSeriesId ?? 0, -1);

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
            return StepOneTick(ignorePaused, buildPresentation: true);
        }

        public bool StepOneTick(bool ignorePaused, bool buildPresentation)
        {
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(_tickIndex + 1, buildPresentation);
            RefreshInspectorState();
            return stepped;
        }

        public void UnbindWorld()
        {
            if (_world != null)
                BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            CreateProductionWorld();
            ResetDriverStateAfterWorldCreation();
        }

#if UNITY_EDITOR
        public bool TryConfigureEmptyDiagnosticWorld(
            BattleRuntimeWorldSettings settings,
            out string failureReason)
        {
            if (_world != null &&
                (_world.ObjectCount != 0 || _world.ClaimedRuntimeSlotCountForDiagnostics != 0))
            {
                failureReason =
                    "The diagnostic world can only be configured before entities are registered.";
                return false;
            }

            try
            {
                CreateProductionWorld(settings, _presentationBackendMode);
                ResetDriverStateAfterWorldCreation();
                failureReason = string.Empty;
                return true;
            }
            catch (System.Exception exception)
            {
                failureReason = exception.Message;
                return false;
            }
        }
#endif

        private void ResetDriverStateAfterWorldCreation()
        {
            _tickIndex = 0;
            _timeAccumulator = 0f;
            _sparkRenderFrame = 0;
            _lastAppliedFrameInput = FrameInputSet.Empty(0);
            _lastFrameSnapshot = null;
            _lastChecksumSnapshot = null;
            lastFrameChecksum = string.Empty;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

        private void CreateProductionWorld()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            CreateProductionWorld(settings, presentationMode);
        }

        private void CreateProductionWorld(
            BattleRuntimeWorldSettings settings,
            BattlePresentationBackendMode presentationMode)
        {
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            var nextWorld = new SimulationWorld(
                settings.Profile,
                settings.InitialRuntimeSlotCapacity,
                settings.CollisionBroadphase);
            nextWorld.SetBattlePresentationBackend(presentationMode);
            if (_world != null)
                BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = nextWorld;
            _presentationBackendMode = presentationMode;
            _battleTickSystem = new NTSDBattleTickSystem(_world);
        }

        internal bool EnsureRuntimeProfileFromSources()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            if (WorldMatchesRuntimeSettings(_world, settings))
            {
                _presentationBackendMode = presentationMode;
                _world.SetBattlePresentationBackend(presentationMode);
                return true;
            }

            if (_world != null &&
                (_world.ClaimedRuntimeSlotCountForServices > 0 || _world.ObjectCount > 0))
            {
                Debug.LogError(
                    $"[SimulationTickDriver] Runtime profile change rejected while entities are registered. " +
                    $"Current={_world.RuntimeProfileForServices}/{_world.MaxRuntimeSlotsForServices}, " +
                    $"Requested={settings.Profile}/{settings.InitialRuntimeSlotCapacity}");
                return false;
            }

            CreateProductionWorld(settings, presentationMode);
            return true;
        }

        public static bool IsFinalCatchUpTick(
            float remainingAccumulator,
            int ticksAlreadyExecuted,
            int maxCatchUpTicks)
        {
            return remainingAccumulator < SimulationConstants.SIM_DT ||
                   ticksAlreadyExecuted + 1 >= maxCatchUpTicks;
        }

        public static bool ShouldBuildPresentationForCatchUpTick(
            SimulationDriveMode driveMode,
            bool requireInputFrameReady,
            float remainingAccumulator,
            int ticksAlreadyExecuted,
            int maxCatchUpTicks)
        {
            return driveMode != SimulationDriveMode.LocalFreeRun ||
                   requireInputFrameReady ||
                   IsFinalCatchUpTick(
                       remainingAccumulator,
                       ticksAlreadyExecuted,
                       maxCatchUpTicks);
        }

        internal static bool WorldMatchesRuntimeSettings(
            SimulationWorld world,
            BattleRuntimeWorldSettings settings)
        {
            if (world == null || world.RuntimeProfileForServices != settings.Profile)
                return false;

            if (world.CollisionBroadphaseForServices != settings.CollisionBroadphase)
                return false;

            return world.MaxRuntimeSlotsForServices == settings.InitialRuntimeSlotCapacity ||
                   (settings.Profile == BattleRuntimeProfile.DesktopExtended &&
                    world.MaxRuntimeSlotsForServices > settings.InitialRuntimeSlotCapacity);
        }

        protected override void OnSingletonDestroyed()
        {
            BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = null;
            _battleTickSystem = null;
        }
    }
}


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Simulation\Input\FrameInputSet.cs ---
using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    [Flags]
    public enum SimulationInputButtons : byte
    {
        None = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
        Attack = 1 << 4,
        Jump = 1 << 5,
        Defend = 1 << 6,
    }

    [Serializable]
    public readonly struct SimulationPlayerInput
    {
        public SimulationPlayerInput(int playerSlot, SimulationInputButtons buttons)
        {
            PlayerSlot = playerSlot;
            Buttons = buttons;
        }

        public int PlayerSlot { get; }
        public SimulationInputButtons Buttons { get; }
    }

    [Serializable]
    public sealed class FrameInputSet
    {
        private static readonly IReadOnlyList<SimulationPlayerInput> NoPlayers =
            Array.Empty<SimulationPlayerInput>();

        public FrameInputSet(int tickIndex, IReadOnlyList<SimulationPlayerInput> players = null)
        {
            TickIndex = tickIndex;
            Players = players ?? NoPlayers;
        }

        public int TickIndex { get; }
        public IReadOnlyList<SimulationPlayerInput> Players { get; }

        public static FrameInputSet Empty(int tickIndex)
        {
            return new FrameInputSet(tickIndex);
        }

        internal static Dictionary<int, FrameInputSet> BuildDenseTraceTimeline(
            int ticks,
            IEnumerable<int> activeHumanPlayerSlots,
            IEnumerable<FrameInputSet> sparseFrames)
        {
            var orderedSlots = new List<int>();
            var heldButtons = new Dictionary<int, SimulationInputButtons>();
            if (activeHumanPlayerSlots != null)
            {
                foreach (int playerSlot in activeHumanPlayerSlots)
                {
                    if (heldButtons.ContainsKey(playerSlot))
                        continue;

                    heldButtons[playerSlot] = SimulationInputButtons.None;
                    orderedSlots.Add(playerSlot);
                }
            }
            orderedSlots.Sort();

            var updatesByTick = new Dictionary<int, List<SimulationPlayerInput>>();
            if (sparseFrames != null)
            {
                foreach (FrameInputSet frame in sparseFrames)
                {
                    if (frame == null || frame.TickIndex <= 0 || frame.TickIndex > ticks)
                        continue;
                    if (!updatesByTick.TryGetValue(frame.TickIndex, out List<SimulationPlayerInput> updates))
                    {
                        updates = new List<SimulationPlayerInput>();
                        updatesByTick[frame.TickIndex] = updates;
                    }

                    for (int i = 0; i < frame.Players.Count; i++)
                        updates.Add(frame.Players[i]);
                }
            }

            var result = new Dictionary<int, FrameInputSet>();
            for (int tick = 1; tick <= ticks; tick++)
            {
                if (updatesByTick.TryGetValue(tick, out List<SimulationPlayerInput> updates))
                {
                    for (int i = 0; i < updates.Count; i++)
                    {
                        SimulationPlayerInput update = updates[i];
                        if (heldButtons.ContainsKey(update.PlayerSlot))
                            heldButtons[update.PlayerSlot] = update.Buttons;
                    }
                }

                var players = new SimulationPlayerInput[orderedSlots.Count];
                for (int i = 0; i < orderedSlots.Count; i++)
                {
                    int playerSlot = orderedSlots[i];
                    players[i] = new SimulationPlayerInput(playerSlot, heldButtons[playerSlot]);
                }
                result[tick] = new FrameInputSet(tick, players);
            }
            return result;
        }
    }
}


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\Scripts\Simulation\BattleParitySnapshot.cs ---
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

    public interface IBattleChecksumSnapshot
    {
        string Schema { get; }
        int Tick { get; }
        int ObjectCount { get; }
        string OverallChecksum { get; }
        string ToJson();
    }

    public sealed class BattleParityFrameSnapshot : IBattleChecksumSnapshot
    {
        public const string SchemaId = "ntsd-battle-trace-v3";
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
        public string Schema => SchemaId;
        public string OverallChecksum => Hashes?.Overall ?? string.Empty;

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

    public sealed class BattleExtendedChecksumHashes
    {
        public string ARest;
        public string Events;
        public string Input;
        public string Metadata;
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
                ["metadata"] = Metadata,
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

    /// <summary>
    /// Capacity-aware checksum for Extended runtime profiles. This is deliberately
    /// independent from the frozen Authority400 v3 parity/trace representation.
    /// </summary>
    public sealed class BattleExtendedChecksumSnapshot : IBattleChecksumSnapshot
    {
        public const string SchemaId = "ntsd-unity-extended-battle-checksum-v1";

        internal object InputDomain;
        internal object MetadataDomain;
        internal object RngDomain;
        internal object WorldDomain;
        internal object SlotsDomain;
        internal object ARestDomain;
        internal object VRestDomain;
        internal object StatsDomain;
        internal object EventsDomain;

        public string Schema => SchemaId;
        public string Profile { get; internal set; }
        public int Tick { get; internal set; }
        public int LogicalCapacity { get; internal set; }
        public int ClaimedCount { get; internal set; }
        public int ObjectCount { get; internal set; }
        public BattleExtendedChecksumHashes Hashes { get; internal set; }
        public string OverallChecksum => Hashes?.Overall ?? string.Empty;

        public string ToJson()
        {
            return BattleCanonicalJson.Serialize(new SortedDictionary<string, object>(StringComparer.Ordinal)
            {
                ["aRest"] = ARestDomain,
                ["events"] = EventsDomain,
                ["hashes"] = Hashes.ToCanonicalObject(includeOverall: true),
                ["input"] = InputDomain,
                ["kind"] = "extended-checksum",
                ["metadata"] = MetadataDomain,
                ["rng"] = RngDomain,
                ["schema"] = Schema,
                ["slots"] = SlotsDomain,
                ["stats"] = StatsDomain,
                ["tick"] = Tick,
                ["vRest"] = VRestDomain,
                ["world"] = WorldDomain,
            });
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
            if (RuntimeProfileForServices != BattleRuntimeProfile.Authority400 ||
                RuntimeSlotCapacity != AuthorityRuntimeSlotCapacity)
            {
                throw new InvalidOperationException(
                    $"Parity snapshots require an Authority400 world ({AuthorityRuntimeSlotCapacity} slots); " +
                    $"actual profile is {RuntimeProfileForServices} with capacity {RuntimeSlotCapacity}.");
            }

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

        /// <summary>
        /// Captures the capacity-aware checksum used by MobileExtended and
        /// DesktopExtended worlds. It must not be used as a v3 parity trace.
        /// </summary>
        public BattleExtendedChecksumSnapshot CaptureExtendedChecksumSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null)
        {
            if (RuntimeProfileForServices != BattleRuntimeProfile.MobileExtended &&
                RuntimeProfileForServices != BattleRuntimeProfile.DesktopExtended)
            {
                throw new InvalidOperationException(
                    "Extended checksums require a MobileExtended or DesktopExtended world.");
            }

            int logicalCapacity = RuntimeSlotCapacity;
            object inputDomain = ProjectFrameInput(frameInput ?? FrameInputSet.Empty(tickIndex));
            object rngDomain = DictionaryOf(
                ("callCount", (object)(Rng?.CallCount ?? 0UL)),
                ("seed", Rng?.State ?? 0U));
            object worldDomain = ProjectWorldDomain();
            object metadataDomain = DictionaryOf(
                ("claimedCount", (object)_runtimeSlots.ClaimedCount),
                ("logicalCapacity", logicalCapacity),
                ("objectCount", ObjectCount),
                ("profile", RuntimeProfileForServices.ToString()),
                ("schema", BattleExtendedChecksumSnapshot.SchemaId),
                ("tick", tickIndex));
            object slotsDomain = ProjectExtendedRuntimeSlots(logicalCapacity);
            RuntimeRestStore.DiagnosticSnapshot restSnapshot =
                _runtimeRestStore.CaptureSparseSnapshot();
            object aRestDomain = ProjectExtendedARestDomain(restSnapshot);
            object vRestDomain = ProjectExtendedVRestDomain(restSnapshot);
            object statsDomain = DictionaryOf(
                ("damage", CloneArray(DamageStats)),
                ("kill", CloneArray(KillStats)));
            object eventsDomain = ProjectEventsDomain();

            var hashes = new BattleExtendedChecksumHashes
            {
                ARest = BattleCanonicalJson.Sha256(aRestDomain),
                Events = BattleCanonicalJson.Sha256(eventsDomain),
                Input = BattleCanonicalJson.Sha256(inputDomain),
                Metadata = BattleCanonicalJson.Sha256(metadataDomain),
                Rng = BattleCanonicalJson.Sha256(rngDomain),
                Slots = BattleCanonicalJson.Sha256(slotsDomain),
                Stats = BattleCanonicalJson.Sha256(statsDomain),
                VRest = BattleCanonicalJson.Sha256(vRestDomain),
                World = BattleCanonicalJson.Sha256(worldDomain),
            };
            hashes.Overall = BattleCanonicalJson.Sha256(
                hashes.ToCanonicalObject(includeOverall: false));

            return new BattleExtendedChecksumSnapshot
            {
                Profile = RuntimeProfileForServices.ToString(),
                Tick = tickIndex,
                LogicalCapacity = logicalCapacity,
                ClaimedCount = _runtimeSlots.ClaimedCount,
                ObjectCount = ObjectCount,
                Hashes = hashes,
                InputDomain = inputDomain,
                MetadataDomain = metadataDomain,
                RngDomain = rngDomain,
                WorldDomain = worldDomain,
                SlotsDomain = slotsDomain,
                ARestDomain = aRestDomain,
                VRestDomain = vRestDomain,
                StatsDomain = statsDomain,
                EventsDomain = eventsDomain,
            };
        }

        private object ProjectExtendedRuntimeSlots(int logicalCapacity)
        {
            var slots = new object[logicalCapacity];
            for (int runtimeSlot = 0; runtimeSlot < logicalCapacity; runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view = _runtimeSlots.GetReadOnlyView(runtimeSlot);
                LF2Entity entity = view.Entity;
                if (view.Claimed &&
                    (entity?.ItrRest == null ||
                     !entity.ItrRest.IsBoundTo(_runtimeRestStore, runtimeSlot)))
                {
                    throw new InvalidOperationException(
                        $"Extended checksum requires slot {runtimeSlot} to be bound to the current world's rest store.");
                }
                int? currentDataOid = entity?.FrameCache?.Wrapper != null
                    ? entity.FrameCache.Wrapper.characterId
                    : entity?.ObjectId;
                int stableId = entity?.Runtime?.StableId ?? view.RawRuntime?.StableId ?? 0;
                slots[runtimeSlot] = DictionaryOf(
                    ("claimed", (object)view.Claimed),
                    ("currentDataOid", currentDataOid),
                    ("generation", view.Generation),
                    ("runtime", entity == null
                        ? ProjectEntityRuntime(
                            null,
                            runtimeSlot,
                            false,
                            view.RawRuntime,
                            projectRawState: view.RawRuntime != null)
                        : ProjectEntityRuntime(entity, runtimeSlot, IsActiveForCurrentPass(entity))),
                    ("runtimeSlot", runtimeSlot),
                    ("stableId", stableId));
            }

            return DictionaryOf(
                ("encoding", "capacity-ordered-runtime-slots"),
                ("logicalCapacity", logicalCapacity),
                ("slots", slots));
        }

        private static object ProjectExtendedARestDomain(RuntimeRestStore.DiagnosticSnapshot snapshot)
        {
            var entries = new object[snapshot.ARestEntries.Count];
            for (int i = 0; i < entries.Length; i++)
            {
                RuntimeRestStore.ARestEntry entry = snapshot.ARestEntries[i];
                entries[i] = DictionaryOf(
                    ("slot", (object)entry.AttackerSlot),
                    ("value", entry.Value));
            }

            return DictionaryOf(
                ("encoding", "sparse-nonzero"),
                ("logicalCapacity", snapshot.LogicalCapacity),
                ("entries", entries));
        }

        private static object ProjectExtendedVRestDomain(RuntimeRestStore.DiagnosticSnapshot snapshot)
        {
            var entries = new object[snapshot.VRestEntries.Count];
            for (int i = 0; i < entries.Length; i++)
            {
                RuntimeRestStore.VRestEntry entry = snapshot.VRestEntries[i];
                entries[i] = DictionaryOf(
                    ("attackerSlot", (object)entry.AttackerSlot),
                    ("value", entry.Value),
                    ("victimSlot", entry.VictimSlot));
            }

            return DictionaryOf(
                ("encoding", "sparse-nonzero"),
                ("logicalCapacity", snapshot.LogicalCapacity),
                ("entries", entries));
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
            var result = new object[AuthorityRuntimeSlotCapacity];
            for (int runtimeSlot = 0; runtimeSlot < result.Length; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                result[runtimeSlot] = entity == null
                    ? ProjectDefaultRuntimeSlot(runtimeSlot, GetRawRuntimeSlotState(runtimeSlot))
                    : ProjectRuntimeSlot(entity, runtimeSlot);
            }
            return result;
        }

        private object ProjectDefaultRuntimeSlot(int runtimeSlot, NTSDEntityRuntime runtime = null)
        {
            return DictionaryOf(
                ("currentDataOid", null),
                ("runtime", ProjectEntityRuntime(null, runtimeSlot, false, runtime)),
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

        private object ProjectEntityRuntime(
            LF2Entity entity,
            int runtimeSlot,
            bool active,
            NTSDEntityRuntime runtimeOverride = null,
            bool projectRawState = false)
        {
            NTSDEntityRuntime runtime = entity?.Runtime ?? runtimeOverride;
            bool isDefault = entity == null && !projectRawState;
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
                ("category", isDefault ? 0 : category),
                ("charId", isDefault ? -1 : runtime.ObjectId),
                ("entityType", isDefault ? 0 : runtime.EntityType),
                ("objType", isDefault ? 0 : runtime.ObjType),
                ("ownerId", runtime?.OwnerStableId ?? -1),
                ("slot", runtimeSlot),
                ("team", isDefault ? 0 : runtime.Team),
                ("unk364", runtime?.RelationTeam ?? 0));

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
                ("frameWaitCounter", isDefault ? 0 : runtime.FrameWaitCounter),
                ("hitStateCount", isDefault ? 0 : runtime.HitStateCount),
                ("hitStop", isDefault ? 0 : runtime.HitStop),
                ("jumpInitPending", false),
                ("prevFrame", isDefault ? 0 : entity?.Frame?.Prev ?? 0),
                ("prevFrame2", isDefault ? 0 : runtime.PrevFrame2),
                ("suppressJumpInit", false),
                ("waitCounter", isDefault ? 0 : runtime.WaitCounter));

            object links = DictionaryOf(
                ("catcherIdx", isDefault ? -1 : runtime.CatcherSlotIndex),
                ("caughtDuration", isDefault ? 0 : runtime.CaughtDuration),
                ("caughtIdx", isDefault ? -1 : runtime.CaughtSlotIndex),
                ("escapeCounter", isDefault ? 0 : runtime.CatchingStateTU),
                ("grabbedTimer", 0),
                ("heldWeaponSlot", isDefault ? -1 : runtime.HeldWeaponStableId),
                ("holderCopy", isDefault ? 99 : runtime.HolderCopySlotIndex),
                ("holderIdx", isDefault ? -1 : runtime.HolderStableId),
                ("linkState", isDefault ? 0 : runtime.LinkState),
                ("pickerIdx", isDefault ? -1 : runtime.PickerStableId),
                ("pickupCount", isDefault ? 0 : runtime.PickupCount),
                ("releaseTick", runtime?.ReleaseTick ?? -1),
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
                ("blockBackZ", runtime?.ZBoundNegative == true ? 1 : 0),
                ("blockFwdZ", runtime?.ZBoundPositive == true ? 1 : 0),
                ("blockLeft", runtime?.XBoundNegative == true ? 1 : 0),
                ("blockRight", runtime?.XBoundPositive == true ? 1 : 0),
                ("catchTimer", runtime?.CatchTimer ?? 0),
                ("healTimer", isDefault ? 0 : runtime.HealTimer),
                ("hitConfirm", isDefault ? 0 : runtime.HitConfirmEa),
                ("hitConfirm2", isDefault ? 0 : runtime.HitConfirm2),
                ("unk318", runtime?.RenderPicOffset ?? 0),
                ("unk31C", runtime?.WeaponFlightCounter ?? 0),
                ("unk324", runtime?.TransformOriginalObjectId ?? -1),
                ("unk328", isDefault ? -1 : runtime.Unk328),
                ("unk32C", isDefault ? -1 : runtime.Unk32C),
                ("unk330", isDefault ? 0 : runtime.Unk330),
                ("unk334", isDefault ? 0 : runtime.Unk334),
                ("unk338", isDefault ? 0 : runtime.Unk338),
                ("unk33C", runtime?.TransformTargetObjectId ?? -1),
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
            BattleResultsRuntimeState results = battle.Results ?? new BattleResultsRuntimeState();
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
                ("humanInputPolledExternally", flow.HumanInputPolledExternally),
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
                    ("battleEndPhase", (object)results.BattleEndPhase),
                    ("hadBoth", results.HadBoth),
                    ("pendingHostAction", results.PendingHostAction),
                    ("pendingWinner", results.PendingWinner),
                    ("phase", results.Phase),
                    ("teamCount", results.TeamCount),
                    ("teamIds", CloneArray(results.TeamIds)),
                    ("timer", results.Timer),
                    ("winner", results.Winner))),
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
            for (int slot = 0; slot < AuthorityRuntimeSlotCapacity; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                int value = entity?.ItrRest?.Arest ?? GetRawRestArest(slot);
                if (value != 0)
                    entries.Add(DictionaryOf(("slot", (object)slot), ("value", value)));
            }
            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectVRestDomain()
        {
            var entries = new List<object>();
            var victims = new LF2Entity[AuthorityRuntimeSlotCapacity];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < AuthorityRuntimeSlotCapacity; first++)
            {
                for (int second = 0; second < AuthorityRuntimeSlotCapacity; second++)
                {
                    // v3 preserves the authority matrix byte order. Its historical labels
                    // call the first (actual victim) index attackerSlot.
                    int value = victims[first]?.ItrRest?.GetVrest(second) ??
                                GetRawRestVrest(first, second);
                    if (value == 0)
                        continue;
                    entries.Add(DictionaryOf(
                        ("attackerSlot", (object)first),
                        ("value", value),
                        ("victimSlot", second)));
                }
            }
            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
                ("encoding", "sparse-nonzero"),
                ("entries", entries.ToArray()));
        }

        private object ProjectFullARestDomain()
        {
            var values = new int[AuthorityRuntimeSlotCapacity];
            for (int slot = 0; slot < values.Length; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                values[slot] = entity?.ItrRest?.Arest ?? GetRawRestArest(slot);
            }

            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
                ("encoding", "full"),
                ("values", values));
        }

        private object ProjectFullVRestDomain()
        {
            var values = new int[AuthorityRuntimeSlotCapacity][];
            var victims = new LF2Entity[AuthorityRuntimeSlotCapacity];
            for (int victim = 0; victim < victims.Length; victim++)
                victims[victim] = FindEntityByRuntimeSlotIncludingDormant(victim);

            for (int first = 0; first < AuthorityRuntimeSlotCapacity; first++)
            {
                var row = new int[AuthorityRuntimeSlotCapacity];
                for (int second = 0; second < AuthorityRuntimeSlotCapacity; second++)
                {
                    row[second] = victims[first]?.ItrRest?.GetVrest(second) ??
                                  GetRawRestVrest(first, second);
                }
                values[first] = row;
            }

            return DictionaryOf(
                ("dimension", (object)AuthorityRuntimeSlotCapacity),
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
                    ("cue", (object)NormalizeTraceAssetCue(sound?.Cue)),
                    ("tick", sound?.Tick ?? 0),
                    ("worldX", sound?.WorldX ?? 0));
            }
            return DictionaryOf(("pendingSounds", (object)sounds));
        }

        internal static string NormalizeTraceAssetCue(string value)
        {
            string normalized = (value ?? string.Empty).Trim().Replace('\\', '/');
            while (normalized.StartsWith("./", StringComparison.Ordinal))
                normalized = normalized.Substring(2);
            while (normalized.IndexOf("//", StringComparison.Ordinal) >= 0)
                normalized = normalized.Replace("//", "/");

            normalized = normalized.ToLowerInvariant();
            int separator = normalized.LastIndexOf('/');
            string identifier = separator >= 0 ? normalized.Substring(separator + 1) : normalized;
            return identifier.StartsWith("snddata_", StringComparison.Ordinal)
                ? identifier.Substring("snddata_".Length)
                : identifier;
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


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_Lockstep_Framework_Plan.md ---
# NTSD 联机帧同步（Lockstep）框架布置方案（核对清单）

> 目标：联机时战斗核心按帧同步推进（只传输入、不传状态）；当前项目核心目录为 `Assets/NTSD`。
> 本文只讨论架构与目录布置，不涉及代码修改。

---

## 0. 当前已有的“帧同步核心资产”（你可逐条核对）

### 0.1 唯一时钟源（Fixed Tick）
- 文件：`Assets/NTSD/Scripts/Simulation/SimulationTickDriver.cs`
- 特点：
  - 在 `FixedUpdate` 里累积 `Time.fixedDeltaTime`
  - 按 `SimulationConstants.SIM_DT` 以固定频率（当前为 30Hz）循环驱动
  - 每次调用 `RunOneSimTick(tickIndex)`

### 0.2 确定性执行容器（Deterministic Order）
- 文件：`Assets/NTSD/Scripts/Simulation/SimulationWorld.cs`
- 特点：
  - `SortedDictionary<int, Bucket>` 按 `SimOrder` 升序遍历
  - 同一 `SimOrder` 内按 `StableId` 升序（lazy sort）
  - 提供 `TransitTickAll / TUTickAll / LateTick` 三段式执行

### 0.3 按 Tick 对齐的输入缓冲（Tick-aligned Input）
- 文件：`Assets/NTSD/Scripts/Simulation/Input/SimInputBuffer.cs`
- 特点：
  - `EnqueueForNextTick(...)`：本地输入写入“下一帧”（避免同帧竞态）
  - `EnqueueForTick(tick, ...)`：可用于联机输入注入/回放
  - `TryDequeueAll(tick, out events)`：每 tick 消费一次

结论：
- NTSD 已具备 lockstep 的三大基石：`固定tick` + `确定性顺序` + `输入按tick对齐`。
- 后续要补的是：`联机输入分发/延迟窗口/校验/快照/回滚` 等 glue。

---

## 1. 推荐分层架构（目录布置建议）

> 原则：**战斗逻辑权威在“确定性核心层”**；Unity 表现层只渲染结果；网络层只传输入。

### Layer A：Deterministic Core（确定性核心）
- 建议目录：`Assets/NTSD/Scripts/Simulation/Core/`
- 放置内容：
  - `SimulationWorld`、`ISimObject`、`ISimTickable`、`SimContext`（现有）
  - 未来新增：
    - `DeterministicRng`（统一随机）
    - `WorldStateHash`（每 tick hash）
    - `Snapshot`（快照数据结构）

**约束**：
- Core 层尽量不依赖 UnityEngine 的行为（可以暂时有 log，但不要依赖 Transform/Physics/Time）。

### Layer B：Tick Driver（Unity 桥接的时钟驱动层）
- 建议目录：`Assets/NTSD/Scripts/Simulation/Driver/`
- 放置内容：
  - `SimulationTickDriver`（现有）

**约束**：
- Driver 层可以依赖 Unity（因为它负责 FixedUpdate），但不做战斗逻辑。

### Layer C：Input（输入帧系统：本地/联机/回放共用）
- 建议目录：`Assets/NTSD/Scripts/Simulation/Input/`
- 放置内容：
  - `SimInputBuffer`、`SimInputEvent`（现有）
  - 建议新增输入来源适配：
    - `LocalInputSource`：Unity InputSystem 回调 → `EnqueueForNextTick`
    - `NetInputSource`：网络收到 tick 输入 → `EnqueueForTick`
    - `ReplayInputSource`：回放文件/内存记录 → `EnqueueForTick`

**约束**：
- Core 不直接读 Unity Input；只从 `SimInputBuffer.TryDequeueAll` 获取该 tick 的输入。

### Layer D：Lockstep Session（联机会话层）
- 建议目录：`Assets/NTSD/Scripts/Netcode/Lockstep/`
- 放置内容（未来必补）：
  - Session（对局状态机、当前 tick、是否等待输入）
  - FrameInput / PlayerInput（每帧输入包）
  - InputDelay / JitterBuffer（输入延迟窗口）
  - Checksum/Hash（校验）
  - SnapshotStore / Rollback（回滚重演）
  - Resync/Rejoin（断线重连）

### Layer E：Presentation（表现层）
- 建议目录：`Assets/NTSD/Scripts/Presentation/`
- 放置内容：
  - MonoBehaviour、Animator、VFX、UI、相机、音频等

**约束**：
- 表现层只“消费核心状态/事件”，不能反向修改核心权威状态。

---

## 2. Tick Pipeline（每 tick 固定顺序）

建议将每 tick 明确为：

1) `ConsumeInputs(tick)`
- 从 `SimInputBuffer.TryDequeueAll(tick)` 取出该帧输入
- 写入角色/对象的输入状态（例如 keyMask buffer）

2) `Transit`（状态迁移/边界处理）
- 对应：`SimulationWorld.TransitTickAll(tick)`

3) `TU / Sim`（核心逻辑与物理推进）
- 对应：`SimulationWorld.TUTickAll(tick)`

4) `Late`（清理与事件汇总）
- 对应：`SimulationWorld.LateTick(tick)`

5) （可选）`Hash/Record`
- 计算本 tick `WorldStateHash`
- 录制模式保存 `(tick, inputs, hash)`

---

## 3. 联机帧同步“必须补齐”的模块清单（从易到难）

### 3.1 Session（对局会话与 tick 控制）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Session/`
- 职责：
  - 管理对局状态（匹配/加载/战斗/结束）
  - 持有当前 tick、输入延迟窗口、是否允许推进

### 3.2 FrameInput / PlayerInput（每帧输入包）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Input/`
- 职责：
  - 网络传输单位：只传输入，不传世界状态
  - 将输入展开后注入：`SimInputBuffer.EnqueueForTick(tick, key, down)`

### 3.3 输入延迟窗口（InputDelay / JitterBuffer）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Input/`
- 职责：
  - 把网络抖动“吸收”在 buffer 里
  - 常见策略：延迟 N tick 执行（比如 2~6）
  - 若某 tick 输入缺失：选择等待 / 使用空输入 / 触发回滚（见 3.6）

### 3.4 序列化与网络协议（只传输入）
- 目录：`Assets/NTSD/Scripts/Netcode/Transport/`
- 必要消息类型（概念层面）：
  - Join/Match/StartBattle(seed,startTick)
  - InputUpstream(playerId,tick,input)
  - InputBroadcast(tick,allPlayersInputs)
  - Ping/Pong

### 3.5 确定性 RNG（种子下发）
- 目录：`Assets/NTSD/Scripts/Simulation/Core/Determinism/`
- 职责：
  - 全端使用相同 seed 与相同消费顺序
  - 禁止使用 UnityEngine.Random 作为战斗权威

### 3.6 Checksum / WorldStateHash（一致性校验）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Checksum/`
- 职责：
  - 每 tick/每 N tick 计算 hash
  - 客户端上报或服务器广播权威 hash
  - 发现不一致后记录并触发诊断

### 3.7 Snapshot + Rollback（回滚重演，强烈建议）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Rollback/`
- 职责：
  - 保存最近 N tick 快照（回滚窗口）
  - 输入迟到时回滚到 tickX 重演 tickX..current

### 3.8 Resync / Rejoin（断线重连）
- 目录：`Assets/NTSD/Scripts/Netcode/Lockstep/Resync/`
- 职责：
  - 服务器下发：最新快照 + 最近若干帧输入
  - 客户端恢复并追帧

---

## 4. 推荐落地里程碑（你可逐条核对）

1) 单机可回放闭环
- 录制每 tick 输入序列 → 重放 → 每 tick hash 一致

2) 本机双端联调（同机跑 server/client）
- 协议跑通 + 输入注入跑通

3) 真联机 + inputDelay（先不回滚）
- 输入必须提前 N tick 到达，否则等待（会卡，但实现简单）

4) 回滚重演
- 解决迟到输入/丢包导致的等待

5) 断线重连 + 完整校验链路

---

## 5. 当前讨论中的“关键约束提醒”（避免踩坑）

- 战斗核心若要严格 lockstep：
  - 不要让 Unity Physics 作为权威
  - 不要让 Unity Time/Animator 驱动数值逻辑
  - 随机必须统一来源与消费顺序
  - 执行顺序必须完全确定（你现在的 SimOrder/StableId 是正确方向）

---

## 6. 你核对时建议重点关注的 6 个问题

1) 你的战斗核心是否还能有地方直接读 Unity Input？（如果有，需要逐步收敛到 SimInputBuffer）
2) 物理/碰撞是否依赖 Unity Physics2D/3D？（若是，lockstep 风险高）
3) `StableId` 的分配规则是否能做到“跨端一致”？（联机时一般由服务器分配并广播）
4) `SimOrder` 是否覆盖所有对象类型，且不会动态变化导致顺序漂移？
5) 随机是否存在多处来源（UnityEngine.Random / System.Random / 自定义）？（需要统一）
6) 状态是否可快照（至少：位置/速度/状态机/关键计数器/输入缓冲窗口/RNG状态）？


--- File: I:\GitHub\Unity_GAS\gameplay-ability-system-for-unity\Assets\NTSD\NTSD_Lockstep_Risk_Assessment.md ---
# NTSD 帧同步（Lockstep）风险评估清单

> 评估目标：识别现有代码中可能影响"严格 PVP 帧同步"确定性的风险点  
> 评估范围：`Assets/NTSD/Scripts/` 目录  
> 评估时间：2026-03-02

---

## 风险等级定义

- **🔴 Critical（必须修复）**：直接影响战斗权威状态，跨端必定不一致
- **🟠 High（应该修复）**：某些条件下会导致不同步，PVP 模式必须处理
- **🟡 Medium（可延后）**：表现层或非权威逻辑，可以暂时隔离
- **🟢 Low（无需修改）**：已经符合确定性要求或在 Simulation 层外

---

## 1. 随机数来源（Random）

### 🔴 Critical - 战斗逻辑使用 UnityEngine.Random

**文件位置**：
- `Scripts/Animation/LF2Objects/LF2Character.cs`
  - Line 985: `UnityEngine.Random.value < 0.5f` 选择武器攻击动作
  - Line 1013: `UnityEngine.Random.value < 0.5f` 选择挥拳动画
  - Line 1320: `UnityEngine.Random.value < 0.15f` AI 决策概率

**问题**：
- `UnityEngine.Random` 是全局静态随机源，无法保证跨端一致
- 每次调用会改变内部状态，调用顺序/次数不同会导致后续结果不同
- 这些随机调用直接影响战斗动作选择（权威逻辑）

**影响范围**：
- 角色攻击动作选择（60 vs 65 帧）
- 武器攻击类型选择
- AI 行为决策

**修复方案**：
1. 创建 `DeterministicRng` 类（基于固定种子的 System.Random 或自定义实现）
2. 在 `SimContext` 或 `SimulationWorld` 中持有唯一实例
3. 所有战斗逻辑改为：`context.Rng.NextFloat() < 0.5f`
4. 联机时服务器下发种子，所有端从同一种子开始

**预估工作量**：中等
- 创建 RNG 类：1-2 小时
- 替换所有调用点：需要全局搜索 `UnityEngine.Random`，逐个改为 context 注入

---

## 2. 时间依赖（Time）

### 🟢 Low - Simulation 层已无 Time 依赖

**扫描结果**：
- `Scripts/Simulation/` 目录下**未发现** `Time.time` / `Time.deltaTime` 使用
- 所有 Simulation 逻辑已经通过 `SimulationTickDriver` 的固定 tick 驱动

**发现的 Time 使用均在表现层/UI**：
- `Scripts/UI/SelectRoleItem.cs:309` - UI 闪烁动画
- `Scripts/UI/CharacterSelectionController.cs:247` - 倒计时 UI
- `Scripts/Test/ProCamera2DTestPanel.cs` - 测试工具
- `Scripts/Animation/LF2ObjectPool.cs:134,150,164` - 对象池过期清理（非权威）

**结论**：✅ 当前 Simulation 核心已正确隔离时间依赖，无需修改

---

## 3. 物理引擎依赖（Unity Physics）

### 🟢 Low - 已使用自定义碰撞检测

**扫描结果**：
- `Scripts/Simulation/` 和 `Scripts/Animation/` 核心战斗逻辑**未使用** Unity Physics/Rigidbody 作为权威
- 发现的 Physics 引用均为：
  - `Scripts/GAS/` - GAS 框架（非 NTSD 核心）
  - `Scripts/Test/` - 测试工具

**当前实现**：
- 碰撞检测：`BruteForceSceneQuery` 实现自定义 AABB 碰撞
- 物理状态：`PhysicsState` 类手动管理位置/速度/摩擦
- 位移推进：在 `SimTick` 中手动计算 `ps.x += ps.vx`

**优势**：
- 完全确定性（纯数学计算）
- 跨平台一致
- 可序列化/可回滚

**后续优化方向**（非风险项）：
- 将 `BruteForceSceneQuery` 替换为四叉树/空间哈希（性能优化，不影响确定性）

**结论**：✅ 已经是正确路线，无需修改

---

## 4. 集合遍历顺序（Dictionary/HashSet）

### 🟢 Low - 已使用 SortedDictionary

**扫描结果**：
- `SimulationWorld.cs` 使用 `SortedDictionary<int, Bucket>` 按 SimOrder 排序
- Bucket 内使用 `List<ISimObject>` + `OrderBy(obj => obj.StableId)` lazy sort

**当前实现**：
```csharp
private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();
// ...
bucket.items = items.OrderBy(obj => obj.StableId).ToList();
```

**结论**：✅ 已经保证确定性顺序，无需修改

**注意事项**（未来扩展时）：
- 如果其他地方新增 `Dictionary` / `HashSet` 遍历，需要确保：
  - 仅用于查找（不遍历）
  - 或遍历后排序再使用

---

## 5. StableId 分配机制

### 🟠 High - 联机时需要服务器统一分配

**当前实现**：
- `SimulationWorld.AllocateStableId()` 本地递增（从 100 开始）
- 注释中已说明："多人模式：服务器会显式设置 StableId"

**问题**：
- 单机模式：本地分配没问题
- 联机模式：如果各端独立分配，同一对象在不同端可能得到不同 StableId → 执行顺序不一致

**修复方案**：
1. 联机时禁用本地 `AllocateStableId()`
2. 服务器创建对象时分配 StableId 并广播
3. 客户端收到创建消息时使用服务器指定的 StableId

**预估工作量**：中等
- 需要在网络层实现"创建对象"消息（包含 StableId）
- 修改对象创建流程，区分单机/联机模式

---

## 6. 输入来源

### 🟡 Medium - 需要确认输入收集点是否已收敛

**当前架构**：
- ✅ `SimInputBuffer` 提供 tick 对齐的输入缓冲
- ✅ `EnqueueForNextTick` / `EnqueueForTick` 接口完善

**需要核实的点**（未在本次扫描中完全覆盖）：
1. 是否所有战斗对象都从 `SimInputBuffer.TryDequeueAll(tick)` 消费输入？
2. 是否还有地方在 `Update()` / `FixedUpdate()` 中直接读 Unity Input？

**建议行动**：
- 搜索所有 `Input.GetKey` / `Input.GetButton` / InputSystem 回调
- 确认它们只写入 `SimInputBuffer`，不直接驱动战斗逻辑

**预估工作量**：小到中等（取决于发现的直接输入点数量）

---

## 7. 快照与回滚能力

### 🟠 High - 当前不支持快照，需要设计

**当前状态**：
- ❌ 未发现 Snapshot / Serialize / Rollback 相关代码
- ✅ 核心状态集中在 `SimulationWorld` / `PhysicsState` / `LF2LivingObject`

**需要快照的关键状态**（初步清单）：
1. **SimulationWorld**
   - `_nextAutoStableId`（StableId 计数器）
   - 所有注册对象的引用列表
2. **每个 LF2LivingObject**
   - `PhysicsState`（位置/速度/朝向/摩擦）
   - `LF2FrameInfo`（当前帧号/动画状态）
   - `LF2Health`（HP/MP）
   - `LF2EffectState`（buff/debuff 状态）
   - 输入缓冲窗口（如果有）
3. **RNG 状态**（未来添加后）
   - 当前种子/内部状态

**不需要快照的内容**：
- 表现层：Animator / SpriteRenderer / VFX
- 资源引用：FrameData / CharacterData（从配置重建）
- UI 状态

**修复方案**：
1. 为核心类添加 `Serialize()` / `Deserialize()` 方法
2. 创建 `SnapshotStore` 保存最近 N tick 快照（例如 60-180 tick）
3. 实现 `RollbackManager.RestoreSnapshot(tick)` + 重演逻辑

**预估工作量**：大
- 设计序列化格式：1-2 天
- 实现所有核心类的序列化：3-5 天
- 测试回滚正确性：2-3 天

---

## 8. 浮点数确定性

### 🟡 Medium - 当前使用 float，需要评估跨平台一致性

**当前实现**：
- `PhysicsState` 所有字段均为 `float`
- 速度/位置计算使用 `Mathf` / 标准浮点运算

**风险评估**：
- **低风险场景**：同平台（都是 Windows x64 / 都是 Android ARM64）
- **中风险场景**：跨平台（PC vs 移动端）
- **高风险场景**：复杂物理模拟 + 长时间累积误差

**当前 NTSD 的情况**：
- 物理相对简单（2D 横版，无复杂刚体碰撞）
- 每 tick 重新设置速度（不是累积型物理）
- 有摩擦/重力但计算简单

**建议策略**（按优先级）：
1. **短期**：先用 float + 严格校验（每 N tick 对比 hash）
   - 如果发现不一致，记录并分析是否是浮点问题
2. **中期**：如果确认有浮点漂移，考虑：
   - 使用 fixed point 库（例如 FixMath.NET）
   - 或限制浮点运算（避免除法/三角函数，使用查表）
3. **长期**：如果要支持严格跨平台 PVP，最终可能需要 fixed point

**预估工作量**（如果需要迁移到 fixed point）：大
- 替换所有 float → FixedPoint：1-2 周
- 测试所有战斗逻辑：1-2 周

---

## 9. Transform 同步

### 🟢 Low - Transform 仅用于表现，不影响权威状态

**当前实现**：
- 权威位置存储在 `PhysicsState.x/y/z`（像素单位）
- `Transform.position` 从 `PhysicsState` 单向同步（表现层）

**代码证据**：
```csharp
// LF2Character.cs:2572
_CharacterHub.transform.position = new Vector3(
    PS.x / SimulationConstants.PIXELS_PER_UNIT,
    PS.y / SimulationConstants.PIXELS_PER_UNIT + groundY,
    _CharacterHub.transform.position.z
);
```

**结论**：✅ 正确的单向数据流（Sim → Presentation），无需修改

---

## 总结：改动规模评估

### 必须修复（联机前）
1. **🔴 随机数统一**：中等工作量（1-3 天）
2. **🟠 StableId 联机分配**：中等工作量（2-3 天）

### 应该修复（严格 PVP）
3. **🟠 快照与回滚**：大工作量（1-2 周）

### 可延后评估
4. **🟡 输入收敛检查**：小到中等（1-2 天）
5. **🟡 浮点确定性**：视测试结果决定（可能 0 天，也可能 2-4 周）

### 无需修改（已符合要求）
- ✅ 时间依赖（已隔离）
- ✅ 物理引擎（已自定义）
- ✅ 集合遍历（已排序）
- ✅ Transform 同步（已单向）

---

## 推荐实施路线

### Phase 1：单机可回放（1 周）
- 统一随机数源
- 实现输入录制/回放
- 每 tick 计算 hash 并记录

### Phase 2：本机双端验证（1 周）
- 实现 StableId 服务器分配
- 实现基础网络协议（输入上行/广播）
- 本机跑 server + client 验证一致性

### Phase 3：真联机 + inputDelay（1 周）
- 实现输入延迟窗口
- 缺帧等待策略
- 网络测试

### Phase 4：回滚与断线重连（2-3 周）
- 实现快照系统
- 实现回滚重演
- 实现断线重连

### Phase 5：跨平台验证（视情况）
- 如果发现浮点不一致，考虑 fixed point 迁移

---

## 附录：扫描命令记录

```bash
# 随机数
grep -rn "UnityEngine.Random\|System.Random\|new Random(" Scripts/

# 时间依赖
grep -rn "Time.time\|Time.deltaTime\|DateTime.Now" Scripts/

# 物理引擎
grep -rn "Physics\.\|Physics2D\.\|Rigidbody\|Collider" Scripts/

# 不稳定集合
grep -rn "Dictionary<\|HashSet<" Scripts/
```


[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

Review the proposed direction for converting the Unity NTSD battle runtime into a production frame-lockstep architecture.

Read the supplied frame-sync article and inspect the actual repository. The unique authority for battle behavior is:
J:\QQFile\NTSD2.4\ntsd_release_C#

Current verified facts:
- The Unity project already has a fixed 30 Hz SimulationTickDriver, LocalFreeRun/LockstepBuffered/Manual modes, FrameInputSet, a deterministic LCG RNG, runtime-slot ordering/generation handles, and Authority/Extended checksum capture.
- It has no production transport, room/session server, wire protocol, network frame provider, input history, full restorable world snapshot, rollback, rejoin, replay runtime, or network simulator.
- Static audit found likely lockstep blockers:
  1. asynchronous sprite/catalog readiness can affect collision/body-hit eligibility;
  2. stage bounds are refreshed from Unity Transform/float scene state during battle ticks;
  3. a presentation pass writes a runtime position-hold flag back into logic;
  4. production defaults to LocalFreeRun and local SimInputBuffer;
  5. authoritative motion uses double/float without a cross-platform numeric contract;
  6. hidden float hit accumulators are outside the canonical runtime/checksum;
  7. loose-quadtree AI traversal can inherit unordered incremental-record order;
  8. renderer and logical entities share one auto StableId sequence;
  9. GameObject pool retention depends on Time.time/asynchronous prewarm;
  10. world reset invokes object cleanup through HashSet enumeration.
- The C# authority itself uses many double operations, so a blind global Fix64 conversion can change observable behavior. Parity with the authority must remain a gate.
- The existing Assets/NTSD/NTSD_Lockstep_Framework_Plan.md and NTSD_Lockstep_Risk_Assessment.md are historical and contain stale facts (for example FixedUpdate driving logic and unresolved battle UnityEngine.Random).
- The prior 1000-AI stress work shows the current object-oriented full workload is over the 33.3 ms tick budget; frame sync does not solve that performance limit.

Provide an architecture review with:
1. the correct target module boundaries (pure simulation, protocol, server/session, Unity client adapter, presentation, replay/diagnostics);
2. the safest migration order that preserves existing battle behavior;
3. whether strict delayed lockstep without rollback should be the first playable milestone;
4. how to handle numeric determinism without prematurely breaking C# parity;
5. snapshot/checksum/input-history contracts needed for replay, resync, and eventual rollback;
6. what must be proven before selecting or integrating a transport;
7. acceptance gates for Windows Editor/Player and Android IL2CPP;
8. any corrections to the assumptions above.

Do not edit production source. Write a concise but concrete architecture report to the requested output file.
