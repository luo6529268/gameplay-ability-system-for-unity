using System.Collections.Generic;
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
            BattlePresentationBackendMode.LegacyOnly;

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
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(_tickIndex + 1);
            RefreshInspectorState();
            return stepped;
        }

        public void UnbindWorld()
        {
            _world?.BattlePresentation.Reset();
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            CreateProductionWorld();
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
