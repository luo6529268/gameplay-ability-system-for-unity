using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Lockstep;
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
        public const int DefaultMaxTicksPerFrame = 1;

        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("单个 Unity 渲染帧最多执行多少个逻辑帧。单机默认 1；大于 1 仅用于显式追帧或吞吐诊断。")]
        public int maxCatchUpTicksPerFrame = DefaultMaxTicksPerFrame;

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
            if (maxCatchUpTicksPerFrame < 1)
                maxCatchUpTicksPerFrame = 1;
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
        FrameInputSet GetFrameInput(int tickIndex);
        void BeforeSimTick(int tickIndex) { }
        void AfterSimTick(int tickIndex) { }
        void Reset() { }
    }

    public sealed class LocalSimulationFrameInputProvider : ISimulationFrameInputProvider
    {
        private readonly FrameInputSet emptyFrame = FrameInputSet.Empty(0);

        public bool IsFrameInputReady(int tickIndex) => true;
        public FrameInputSet GetFrameInput(int tickIndex)
        {
            emptyFrame.ResetPreallocated(tickIndex, null);
            return emptyFrame;
        }

        public void Reset()
        {
            emptyFrame.ResetPreallocated(0, null);
        }
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
        [SerializeField][MMReadOnly] private string effectiveAiExecutionProfile =
            nameof(BattleAiExecutionProfile.LegacyCanonical);

        [Header("Sound Presentation Diagnostics")]
        [Tooltip("Diagnostic-only switch. Logical sound events and checksums are still recorded when presentation is suppressed.")]
        [SerializeField] private bool suppressSoundPresentationForDiagnostics = false;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;
        private NTSD.Animation.BattleEntityOverlayRenderer _overlayRenderer;
        private BattlePresentationBackendMode _presentationBackendMode =
            BattlePresentationBackendMode.CentralOnly;
        private BattleAiExecutionProfile _aiExecutionProfile =
            BattleAiExecutionProfile.LegacyCanonical;

        private int _sparkRenderFrame = 0;
        private ISimulationFrameInputProvider _frameInputProvider = new LocalSimulationFrameInputProvider();
        private FrameInputSet _lastAppliedFrameInput = FrameInputSet.Empty(0);
        private BattleParityFrameSnapshot _lastFrameSnapshot;
        private IBattleChecksumSnapshot _lastChecksumSnapshot;
        private ISimulationSoundPresentationSink _soundPresentationSinkForDiagnostics;
        private long _dispatchedSoundEventCount;
        private long _suppressedSoundEventCount;

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
            if (_world == null)
                return;

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

            return _frameInputProvider != null &&
                   !(_frameInputProvider is LocalSimulationFrameInputProvider) &&
                   _frameInputProvider.IsFrameInputReady(tickIndex);
        }

        private bool StepOneTickInternal(int tickIndex, bool buildPresentation)
        {
            if (_world == null || !CanAdvanceTick(tickIndex))
                return false;

            ISimulationFrameInputProvider provider = _frameInputProvider;
            if (provider == null)
                return false;

            FrameInputSet frameInput = provider.GetFrameInput(tickIndex);
            if (frameInput == null || frameInput.TickIndex != tickIndex)
                return false;

            provider.BeforeSimTick(tickIndex);
            bool stepped = StepOneTickInternal(frameInput, buildPresentation);
            if (stepped)
                provider.AfterSimTick(tickIndex);
            return stepped;
        }

        private bool StepOneTickInternal(FrameInputSet frameInput, bool buildPresentation)
        {
            if (_world == null || frameInput == null || frameInput.TickIndex != _tickIndex + 1)
                return false;

            int tickIndex = frameInput.TickIndex;
            _tickIndex = tickIndex;
            _sparkRenderFrame = tickIndex;
            if (_world.Runtime?.Flow != null)
            {
                _world.Runtime.Flow.SparkRenderFrame = _sparkRenderFrame;
            }

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");

            _lastAppliedFrameInput = frameInput;
            _world.ApplyFrameInputSet(frameInput);
            _battleTickSystem?.RunReleaseTick(tickIndex, buildPresentation);
            CaptureFrameChecksumIfNeeded(tickIndex, frameInput);
            DispatchPendingSoundsAfterChecksum();

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

        private void DispatchPendingSoundsAfterChecksum()
        {
            IReadOnlyList<PendingSoundEvent> sounds = _world?.PendingSounds;
            int soundCount = sounds?.Count ?? 0;
            if (soundCount == 0)
                return;

            if (suppressSoundPresentationForDiagnostics)
            {
                _suppressedSoundEventCount += soundCount;
                return;
            }

            ISimulationSoundPresentationSink sink =
                _soundPresentationSinkForDiagnostics ?? AppManager.Instance?.SoundPlayer;
            if (sink == null)
                return;

            sink.PresentSounds(sounds);
            _dispatchedSoundEventCount += soundCount;
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
        public BattleAiExecutionProfile AiExecutionProfile => _aiExecutionProfile;
        public bool SuppressSoundPresentationForDiagnostics =>
            suppressSoundPresentationForDiagnostics;
        public long DispatchedSoundEventCountForDiagnostics => _dispatchedSoundEventCount;
        public long SuppressedSoundEventCountForDiagnostics => _suppressedSoundEventCount;

        public float RemainingAccumulatorTime => _timeAccumulator;
        public float RenderAlpha => renderAlpha;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        public void SetSoundPresentationSuppressedForDiagnostics(bool value)
        {
            suppressSoundPresentationForDiagnostics = value;
        }

        public void SetSoundPresentationSinkForDiagnostics(
            ISimulationSoundPresentationSink sink)
        {
            _soundPresentationSinkForDiagnostics = sink;
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
            if (!EnsureProductionConfigurationFromSources())
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
            _frameInputProvider = provider ??
                (lockstepSettings.driveMode == SimulationDriveMode.LocalFreeRun &&
                 !lockstepSettings.requireInputFrameReady
                    ? new LocalSimulationFrameInputProvider()
                    : null);
            _frameInputProvider?.Reset();
            _lastAppliedFrameInput = FrameInputSet.Empty(_tickIndex);
        }

        public BattleLockstepSession CreateStrictLockstepSession(
            LockstepSessionIdentity identity,
            int futureFrameCapacity,
            int journalCapacity)
        {
            lockstepSettings.Normalize();
            return new BattleLockstepSession(
                this,
                identity,
                lockstepSettings.inputDelayTicks,
                futureFrameCapacity,
                journalCapacity);
        }

        public bool StepOneTick(
            FrameInputSet frameInput,
            bool ignorePaused = false,
            bool buildPresentation = true)
        {
            if (!ignorePaused && paused)
                return false;

            bool stepped = StepOneTickInternal(frameInput, buildPresentation);
            RefreshInspectorState();
            return stepped;
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
            return TryConfigureEmptyDiagnosticWorld(
                settings,
                BattleAiExecutionProfile.LegacyCanonical,
                out failureReason);
        }

        public bool TryConfigureEmptyDiagnosticWorld(
            BattleRuntimeWorldSettings settings,
            BattleAiExecutionProfile aiExecutionProfile,
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
                CreateProductionWorld(
                    settings,
                    _presentationBackendMode,
                    aiExecutionProfile);
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
            _dispatchedSoundEventCount = 0;
            _suppressedSoundEventCount = 0;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

        private void CreateProductionWorld()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            BattleAiExecutionProfile aiExecutionProfile =
                BattleAiExecutionProfileProductionSource.Resolve(GameConfig.Instance);
            CreateProductionWorld(settings, presentationMode, aiExecutionProfile);
        }

        private void CreateProductionWorld(
            BattleRuntimeWorldSettings settings,
            BattlePresentationBackendMode presentationMode,
            BattleAiExecutionProfile aiExecutionProfile)
        {
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            var nextWorld = new SimulationWorld(
                settings.Profile,
                settings.InitialRuntimeSlotCapacity,
                settings.CollisionBroadphase);
            nextWorld.ConfigureAiExecutionProfile(aiExecutionProfile);
            nextWorld.SetBattlePresentationBackend(presentationMode);
            if (_world != null)
                BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = nextWorld;
            _presentationBackendMode = presentationMode;
            _aiExecutionProfile = aiExecutionProfile;
            effectiveAiExecutionProfile = aiExecutionProfile.ToString();
            _battleTickSystem = new NTSDBattleTickSystem(_world);
        }

        internal bool EnsureProductionConfigurationFromSources()
        {
            BattleRuntimeWorldSettings settings = BattleRuntimeProfileProductionSource.Resolve(
                GameConfig.Instance);
            BattlePresentationBackendMode presentationMode =
                BattlePresentationBackendResolver.Resolve(GameConfig.Instance);
            BattleAiExecutionProfile aiExecutionProfile =
                BattleAiExecutionProfileProductionSource.Resolve(GameConfig.Instance);
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            if (WorldMatchesRuntimeSettings(_world, settings, aiExecutionProfile))
            {
                _presentationBackendMode = presentationMode;
                _aiExecutionProfile = aiExecutionProfile;
                effectiveAiExecutionProfile = aiExecutionProfile.ToString();
                _world.SetBattlePresentationBackend(presentationMode);
                return true;
            }

            if (_world != null &&
                (_world.ClaimedRuntimeSlotCountForServices > 0 || _world.ObjectCount > 0))
            {
                Debug.LogError(
                    $"[SimulationTickDriver] Runtime profile change rejected while entities are registered. " +
                    $"Current={_world.RuntimeProfileForServices}/{_world.MaxRuntimeSlotsForServices}/" +
                    $"{_world.AiExecutionProfile}, Requested={settings.Profile}/" +
                    $"{settings.InitialRuntimeSlotCapacity}/{aiExecutionProfile}");
                return false;
            }

            CreateProductionWorld(settings, presentationMode, aiExecutionProfile);
            return true;
        }

        internal bool EnsureRuntimeProfileFromSources()
        {
            return EnsureProductionConfigurationFromSources();
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
            return WorldMatchesRuntimeSettings(
                world,
                settings,
                BattleAiExecutionProfile.LegacyCanonical);
        }

        internal static bool WorldMatchesRuntimeSettings(
            SimulationWorld world,
            BattleRuntimeWorldSettings settings,
            BattleAiExecutionProfile aiExecutionProfile)
        {
            if (world == null || world.RuntimeProfileForServices != settings.Profile)
                return false;

            if (world.AiExecutionProfile != aiExecutionProfile)
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
