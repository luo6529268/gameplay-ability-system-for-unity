using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.App;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Lockstep;
using NTSD.Simulation.Presentation;
using NTSD.Tools;
using Unity.Profiling;
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

        [Tooltip("显式追帧或吞吐诊断的单帧逻辑预算。普通 LocalFreeRun 始终每个 Unity Update 最多自动执行 1 tick。")]
        public int maxCatchUpTicksPerFrame = DefaultMaxTicksPerFrame;

        [Tooltip("最多保留多少个逻辑帧的时间积压，超过后丢弃外层积压但不改变单个逻辑帧步长。")]
        public int maxBacklogTicks = 8;

        [Tooltip("联机帧同步预留：本地输入写入未来第 N 帧。当前单机可保持 0。")]
        public int inputDelayTicks = 0;

        [Tooltip("联机帧同步预留：推进前是否要求该逻辑帧的输入已经准备好。")]
        public bool requireInputFrameReady = false;

        [Tooltip("在每个逻辑 tick 尾部生成无分配的 64 位战局校验值。")]
        public bool enableFrameChecksum = false;

        [Tooltip("诊断工具专用：同时生成会分配托管内存的完整 canonical/JSON 快照。正式战斗必须关闭。")]
        public bool captureFullFrameSnapshotForDiagnostics = false;

        public void Normalize()
        {
            if (maxCatchUpTicksPerFrame < 1)
                maxCatchUpTicksPerFrame = 1;
            if (maxBacklogTicks < maxCatchUpTicksPerFrame) maxBacklogTicks = maxCatchUpTicksPerFrame;
            if (inputDelayTicks < 0) inputDelayTicks = 0;
        }

        public bool DisableAllocatingDiagnosticsForFormalBattle()
        {
            bool changed = captureFullFrameSnapshotForDiagnostics;
            captureFullFrameSnapshotForDiagnostics = false;
            return changed;
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
        private const int MaximumLocalPlayerSlots = 8;
        private readonly SimulationPlayerInput[] capturedPlayers =
            new SimulationPlayerInput[MaximumLocalPlayerSlots];
        private readonly SimulationInputButtons[] previousButtonsBySlot =
            new SimulationInputButtons[MaximumLocalPlayerSlots];
        private readonly bool[] previousSlotActive = new bool[MaximumLocalPlayerSlots];
        private readonly bool[] currentSlotActive = new bool[MaximumLocalPlayerSlots];
        private readonly FrameInputSet capturedFrame = FrameInputSet.Empty(0);
        private SimulationWorld world;
        private bool canonicalFrameCaptured;

        public bool IsFrameInputReady(int tickIndex) => true;
        public FrameInputSet GetFrameInput(int tickIndex)
        {
            int playerCount = 0;
            canonicalFrameCaptured = world != null &&
                world.TryCaptureLocalFrameInput(
                    tickIndex,
                    capturedPlayers,
                    out playerCount);
            if (!canonicalFrameCaptured)
            {
                capturedFrame.ResetPreallocated(tickIndex, null);
                return capturedFrame;
            }

            System.Array.Clear(currentSlotActive, 0, currentSlotActive.Length);
            for (int index = 0; index < playerCount; index++)
            {
                SimulationPlayerInput captured = capturedPlayers[index];
                int playerSlot = captured.PlayerSlot;
                SimulationInputButtons previous =
                    (uint)playerSlot < (uint)previousButtonsBySlot.Length &&
                    previousSlotActive[playerSlot]
                        ? previousButtonsBySlot[playerSlot]
                        : SimulationInputButtons.None;
                SimulationInputButtons current = captured.Buttons;
                capturedPlayers[index] = new SimulationPlayerInput(
                    playerSlot,
                    current,
                    current & ~previous,
                    previous & ~current);

                if ((uint)playerSlot < (uint)previousButtonsBySlot.Length)
                {
                    previousButtonsBySlot[playerSlot] = current;
                    currentSlotActive[playerSlot] = true;
                }
            }

            for (int playerSlot = 0; playerSlot < previousSlotActive.Length; playerSlot++)
            {
                previousSlotActive[playerSlot] = currentSlotActive[playerSlot];
                if (!currentSlotActive[playerSlot])
                    previousButtonsBySlot[playerSlot] = SimulationInputButtons.None;
            }

            capturedFrame.ResetPreallocated(tickIndex, capturedPlayers, playerCount);
            return capturedFrame;
        }

        public void BeforeSimTick(int tickIndex)
        {
            if (canonicalFrameCaptured)
                world?.DiscardDirectLocalInputTick(tickIndex);
        }

        public void Reset()
        {
            capturedFrame.ResetPreallocated(0, null);
            System.Array.Clear(previousButtonsBySlot, 0, previousButtonsBySlot.Length);
            System.Array.Clear(previousSlotActive, 0, previousSlotActive.Length);
            System.Array.Clear(currentSlotActive, 0, currentSlotActive.Length);
            canonicalFrameCaptured = false;
        }

        internal void BindWorld(SimulationWorld nextWorld)
        {
            world = nextWorld;
            Reset();
        }
    }

    /// <summary>
    /// 战斗场景模拟时钟。
    /// 负责固定 30Hz 逻辑 tick，并把 C# 权威工程的 pass 顺序交给 NTSDBattleTickSystem。
    /// Unity 的 Update/LateUpdate 只作为外层驱动和表现刷新；战斗逻辑内部不能依赖 deltaTime。
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        private static readonly ProfilerMarker LatePresentationMarker =
            new ProfilerMarker("NTSD.BattlePresentation.LateUpdate");
        private static readonly ProfilerMarker PresentLatestFrameMarker =
            new ProfilerMarker("NTSD.BattlePresentation.PresentLatestFrame");
        private static readonly ProfilerMarker DispatchSoundsMarker =
            new ProfilerMarker("NTSD.BattlePresentation.DispatchSounds");
        private static readonly ProfilerMarker LegacySparkMarker =
            new ProfilerMarker("NTSD.BattlePresentation.LegacySparkMaterializer");
        private static readonly ProfilerMarker FinalizeHitRecordMarker =
            new ProfilerMarker("NTSD.BattlePresentation.FinalizeHitRecordCycle");

        [Tooltip("记录每个模拟 tick 的开始和结束。")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("启动时暂停，直到 BattleBootstrap 恢复模拟。")]
        [SerializeField] private bool startPaused = true;

        [Header("帧同步时钟")]
        [SerializeField] private LockstepSimulationSettings lockstepSettings = new LockstepSimulationSettings();

        [Header("单机 Simulation Worker")]
        [Tooltip("正式 CentralOnly 单机战斗完成预热后，将 BattleKernel 固定到专用线程；Unity 主线程只消费已发布表现。")]
        [SerializeField] private bool useDedicatedSimulationWorker = true;

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
        [SerializeField][MMReadOnly] private bool dedicatedSimulationWorkerActive;
        [SerializeField][MMReadOnly] private bool dedicatedSimulationWorkerTickInFlight;

        [Header("Sound Presentation Diagnostics")]
        [Tooltip("Diagnostic-only switch. Logical sound events and checksums are still recorded when presentation is suppressed.")]
        [SerializeField] private bool suppressSoundPresentationForDiagnostics = false;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;
        private ulong _lastFrameChecksumValue;
        private bool _hasFrameChecksum;
        private readonly OfflineLocalTickPolicy _offlineLocalTickPolicy =
            new OfflineLocalTickPolicy();
        private readonly ManualReplayTickPolicy _manualReplayTickPolicy =
            new ManualReplayTickPolicy();
        private readonly NetworkLockstepTickPolicy _networkLockstepTickPolicy =
            new NetworkLockstepTickPolicy();
        private SimulationTickHostPolicy _tickHostPolicy;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;
        private BattlePresentationBackendMode _presentationBackendMode =
            BattlePresentationBackendMode.CentralOnly;
        private BattleAiExecutionProfile _aiExecutionProfile =
            BattleAiExecutionProfile.LegacyCanonical;

        private int _sparkRenderFrame = 0;
        private readonly LocalSimulationFrameInputProvider _localFrameInputProvider =
            new LocalSimulationFrameInputProvider();
        private readonly FrameInputSet _emptyLastAppliedFrameInput =
            FrameInputSet.Empty(0);
        private readonly BattleFunctionKeyInputLatch _battleFunctionKeyInputLatch =
            new BattleFunctionKeyInputLatch();
        private ISimulationFrameInputProvider _frameInputProvider;
        private FrameInputSet _lastAppliedFrameInput;
        private BattleParityFrameSnapshot _lastFrameSnapshot;
        private IBattleChecksumSnapshot _lastChecksumSnapshot;
        private ISimulationSoundPresentationSink _soundPresentationSinkForDiagnostics;
        private readonly List<PendingSoundEvent> _publishedSoundEvents =
            new List<PendingSoundEvent>(256);
        private int _publishedSoundEventLimit = 256;
        private long _dispatchedSoundEventCount;
        private long _suppressedSoundEventCount;
        private long _rejectedPublishedSoundEventCount;
        private long _formalBattleDiagnosticsSuppressedCount;
        private long _rejectedLatePresentationComponentCreateCount;
        private readonly BattleRuntimeAllocationGate _allocationGate =
            new BattleRuntimeAllocationGate();
        private readonly BattleManagedMemoryBoundary _managedMemoryBoundary =
            new BattleManagedMemoryBoundary();
        private BattleManagedMemoryFrameBeginProbe _managedMemoryFrameBeginProbe;
        private BattleManagedMemoryFrameEndProbe _managedMemoryFrameEndProbe;
        private const int MaximumSimulationWorkerPlayerSlots = 8;
        private DedicatedBattleSimulationWorker _simulationWorker;
        private readonly SimulationPlayerInput[] _simulationWorkerSubmittedPlayers =
            new SimulationPlayerInput[MaximumSimulationWorkerPlayerSlots];
        private readonly FrameInputSet _simulationWorkerSubmittedFrameInput =
            FrameInputSet.Empty(0);
        private readonly SimulationPlayerInput[] _simulationWorkerCompletedPlayers =
            new SimulationPlayerInput[MaximumSimulationWorkerPlayerSlots];
        private readonly FrameInputSet _simulationWorkerCompletedFrameInput =
            FrameInputSet.Empty(0);
        private ISimulationFrameInputProvider _simulationWorkerSubmittedProvider;
        private bool _simulationWorkerTickInFlight;
        private bool _simulationWorkerPresentationAwaitingAcknowledgement;
        private int _simulationWorkerSubmittedTick;
        private long _simulationWorkerConsumedSequence;
        private long _simulationWorkerPendingAcknowledgementSequence;
        private long _simulationWorkerAcknowledgementSubmittedSequence;
        private bool _simulationWorkerFailureReported;
        private string _dedicatedSimulationWorkerIneligibilityReason = string.Empty;
        private string _dedicatedSimulationWorkerLastSubmissionFailureReason = string.Empty;
        private long _dedicatedSimulationWorkerLastExecutionElapsedTimestampTicks;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;
            lockstepSettings ??= new LockstepSimulationSettings();
            lockstepSettings.Normalize();
            _frameInputProvider ??= _localFrameInputProvider;
            SelectTickHostPolicy(resetSelectedPolicy: true);

            CreateProductionWorld();

            Log.Info($"[SimulationTickDriver] Awake. paused={paused}, World created");
        }

        private void Update()
        {
            _managedMemoryBoundary.BeginDriverUpdate();
            try
            {
                TryCompleteDedicatedSimulationWorkerPresentationConsumption();
                ConsumeDedicatedSimulationWorkerPublication();
                TryCompleteDedicatedSimulationWorkerPresentationConsumption();
                if (PauseForDedicatedSimulationWorkerFailure())
                {
                    RefreshInspectorState();
                    return;
                }

                if (paused || _world == null)
                {
                    RefreshInspectorState();
                    return;
                }

                CaptureBattleFunctionKeyEdges();

                SimulationTickHostPolicy policy = SelectTickHostPolicy(
                    resetSelectedPolicy: false);
                float elapsedSeconds = policy.UsesWallClock
                    ? (lockstepSettings.useUnscaledTime
                        ? Time.unscaledDeltaTime
                        : Time.deltaTime)
                    : 0f;
                policy.BeginUpdate(elapsedSeconds, lockstepSettings);
                _timeAccumulator = policy.Accumulator;

                int catchUpTicks = 0;
                while (policy.ShouldAttemptAutomaticTick(
                           catchUpTicks,
                           lockstepSettings))
                {
                    int nextTickIndex = _tickIndex + 1;
                    if (!CanAdvanceTick(nextTickIndex))
                        break;

                    bool buildPresentation =
                        policy.ShouldBuildPresentationForNextTick(
                        catchUpTicks,
                        lockstepSettings);
                    if (!StepOneTickInternal(nextTickIndex, buildPresentation))
                        break;

                    policy.CommitAutomaticTick();
                    _timeAccumulator = policy.Accumulator;
                    catchUpTicks++;
                }

                RefreshInspectorState();
            }
            finally
            {
                _managedMemoryBoundary.ObserveAfterDriverUpdate(_tickIndex);
            }
        }

        private void FixedUpdate()
        {
            // 帧同步逻辑不依赖 Unity FixedUpdate。Unity 物理循环只作为引擎外层回调存在。
        }

        private void LateUpdate()
        {
            using ProfilerMarker.AutoScope latePresentationScope =
                LatePresentationMarker.Auto();
            _managedMemoryBoundary.BeginPresentation();
            try
            {
                ConsumeDedicatedSimulationWorkerPublication();
                PauseForDedicatedSimulationWorkerFailure();
                if (_world == null)
                    return;

                using (PresentLatestFrameMarker.Auto())
                    _world.PresentLatestFrame(_tickIndex);
                using (DispatchSoundsMarker.Auto())
                    DispatchPublishedSounds();

                if (_sparkRenderer == null)
                {
                    _sparkRenderer = AppManager.Instance?.SparkRenderer;
                    if (_sparkRenderer == null)
                    {
                        if (_managedMemoryBoundary.BattleWindowOpen)
                        {
                            _rejectedLatePresentationComponentCreateCount++;
                            return;
                        }

                        _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
                    }
                }

                using (LegacySparkMarker.Auto())
                    _sparkRenderer.RenderAll(_world);
                if (_simulationWorker == null)
                {
                    using (FinalizeHitRecordMarker.Auto())
                        _world?.BattlePresentation.FinalizePublishedHitRecordCycle(_world);
                }
            }
            finally
            {
                AcknowledgeDedicatedSimulationWorkerPresentation();
                _managedMemoryBoundary.ObserveAfterPresentation(_tickIndex);
            }
        }

        private bool CanAdvanceTick(int tickIndex)
        {
            TryCompleteDedicatedSimulationWorkerPresentationConsumption();
            if (_simulationWorkerTickInFlight ||
                _simulationWorkerPresentationAwaitingAcknowledgement)
            {
                return false;
            }

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
            ApplyPendingBattleFunctionKeyCommandsForTick();
            if (ShouldSubmitToDedicatedSimulationWorker())
            {
                if (TrySubmitDedicatedSimulationWorkerTick(
                        frameInput,
                        buildPresentation,
                        provider))
                {
                    return true;
                }

                if (PauseForDedicatedSimulationWorkerFailure())
                    return false;
            }

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
            ApplyPendingBattleFunctionKeyCommandsForTick();
            _world.PrepareStageRuntimeSnapshotForTick(tickIndex);
            _managedMemoryBoundary.BeginTick();
            try
            {
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
                PublishPendingSoundsAfterChecksum();

                if (debugLogPerTick)
                    Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");

                return true;
            }
            finally
            {
                _managedMemoryBoundary.ObserveAfterTick(tickIndex);
            }
        }

        private void CaptureFrameChecksumIfNeeded(int tickIndex, FrameInputSet frameInput)
        {
            if (!lockstepSettings.enableFrameChecksum)
            {
                _lastFrameSnapshot = null;
                _lastChecksumSnapshot = null;
                lastFrameChecksum = string.Empty;
                _lastFrameChecksumValue = 0UL;
                _hasFrameChecksum = false;
                return;
            }

            _lastFrameChecksumValue = _world.CaptureRuntimeChecksum64(tickIndex, frameInput);
            _hasFrameChecksum = true;

            if (lockstepSettings.captureFullFrameSnapshotForDiagnostics)
            {
                _lastChecksumSnapshot = CaptureSupportedChecksumSnapshot(_world, tickIndex, frameInput);
                _lastFrameSnapshot = _lastChecksumSnapshot as BattleParityFrameSnapshot;
                lastFrameChecksum = _lastChecksumSnapshot?.OverallChecksum ?? string.Empty;
                return;
            }

            _lastFrameSnapshot = null;
            _lastChecksumSnapshot = null;
            lastFrameChecksum = string.Empty;
        }

        private void PublishPendingSoundsAfterChecksum()
        {
            IReadOnlyList<PendingSoundEvent> sounds = _world?.PendingSounds;
            int soundCount = sounds?.Count ?? 0;
            if (soundCount == 0)
                return;

            int available = _publishedSoundEventLimit - _publishedSoundEvents.Count;
            int publishCount = Mathf.Clamp(available, 0, soundCount);
            for (int index = 0; index < publishCount; index++)
                _publishedSoundEvents.Add(sounds[index]);
            if (publishCount < soundCount)
                _rejectedPublishedSoundEventCount += soundCount - publishCount;
        }

        private void DispatchPublishedSounds()
        {
            int soundCount = _publishedSoundEvents.Count;
            if (soundCount == 0)
                return;

            try
            {
                if (suppressSoundPresentationForDiagnostics)
                {
                    _suppressedSoundEventCount += soundCount;
                    return;
                }

                ISimulationSoundPresentationSink sink =
                    _soundPresentationSinkForDiagnostics ?? AppManager.Instance?.SoundPlayer;
                if (sink == null)
                    return;

                sink.PresentSounds(_publishedSoundEvents);
                _dispatchedSoundEventCount += soundCount;
            }
            finally
            {
                _publishedSoundEvents.Clear();
            }
        }

        private bool ShouldSubmitToDedicatedSimulationWorker()
        {
            TryCompleteDedicatedSimulationWorkerPresentationConsumption();
            return _simulationWorker != null &&
                   _simulationWorker.IsRunning &&
                   _simulationWorker.Failure == null &&
                   !_simulationWorkerTickInFlight &&
                   !_simulationWorkerPresentationAwaitingAcknowledgement &&
                   lockstepSettings.driveMode == SimulationDriveMode.LocalFreeRun &&
                   !lockstepSettings.requireInputFrameReady &&
                   !lockstepSettings.captureFullFrameSnapshotForDiagnostics;
        }

        private bool TrySubmitDedicatedSimulationWorkerTick(
            FrameInputSet frameInput,
            bool buildPresentation,
            ISimulationFrameInputProvider provider)
        {
            if (!ShouldSubmitToDedicatedSimulationWorker())
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "worker-is-not-ready-for-submission";
                return false;
            }
            if (frameInput == null)
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "frame-input-is-null";
                return false;
            }
            if (frameInput.Players == null)
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "frame-input-player-list-is-null";
                return false;
            }
            if (frameInput.Players.Count > MaximumSimulationWorkerPlayerSlots)
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "frame-input-player-count-exceeds-worker-capacity";
                return false;
            }

            int tickIndex = frameInput.TickIndex;
            _world.PrepareStageRuntimeSnapshotForTick(tickIndex);
            BattleSimulationStageSnapshot stage =
                BattleSimulationStageSnapshot.Capture(_world.Runtime?.Stage);
            if (!_simulationWorker.TrySubmit(
                    frameInput,
                    buildPresentation,
                    in stage))
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    _simulationWorker.Failure == null
                        ? "worker-input-queue-rejected-request"
                        : "worker-failed-before-request-enqueue";
                return false;
            }

            CopyFrameInput(
                frameInput,
                _simulationWorkerSubmittedFrameInput,
                _simulationWorkerSubmittedPlayers);
            _simulationWorkerSubmittedProvider = provider;
            _simulationWorkerSubmittedTick = tickIndex;
            _simulationWorkerTickInFlight = true;
            dedicatedSimulationWorkerTickInFlight = true;
            _dedicatedSimulationWorkerLastSubmissionFailureReason = string.Empty;
            if (debugLogPerTick)
            {
                Log.Info(
                    $"[SimulationTickDriver] ========== SimTick {tickIndex} SUBMITTED ==========");
            }
            return true;
        }

        private bool ConsumeDedicatedSimulationWorkerPublication()
        {
            if (_simulationWorker == null || !_simulationWorkerTickInFlight)
                return false;

            long consumedSequence = _simulationWorkerConsumedSequence;
            if (!_simulationWorker.TryReadLatest(
                    ref consumedSequence,
                    out BattleSimulationTickPublication publication))
            {
                return false;
            }

            if (publication.TickIndex != _simulationWorkerSubmittedTick)
            {
                paused = true;
                if (!_simulationWorkerFailureReported)
                {
                    _simulationWorkerFailureReported = true;
                    Debug.LogError(
                        "[SimulationTickDriver] Dedicated simulation worker published " +
                        $"tick {publication.TickIndex}, expected {_simulationWorkerSubmittedTick}. " +
                        "Simulation has been paused to avoid advancing a torn world.");
                }
                return false;
            }

            _simulationWorkerConsumedSequence = consumedSequence;
            _simulationWorkerPendingAcknowledgementSequence = consumedSequence;
            _simulationWorkerAcknowledgementSubmittedSequence = 0;
            CopyFrameInput(
                _simulationWorkerSubmittedFrameInput,
                _simulationWorkerCompletedFrameInput,
                _simulationWorkerCompletedPlayers);
            _lastAppliedFrameInput = _simulationWorkerCompletedFrameInput;
            _dedicatedSimulationWorkerLastExecutionElapsedTimestampTicks =
                publication.ExecutionElapsedTimestampTicks;
            _tickIndex = publication.TickIndex;
            _sparkRenderFrame = publication.TickIndex;
            if (_world.Runtime?.Flow != null)
                _world.Runtime.Flow.SparkRenderFrame = publication.TickIndex;

            _lastFrameSnapshot = null;
            _lastChecksumSnapshot = null;
            lastFrameChecksum = string.Empty;
            _hasFrameChecksum = publication.HasStateChecksum;
            _lastFrameChecksumValue = publication.HasStateChecksum
                ? publication.StateChecksum
                : 0UL;
            PublishPendingSoundsAfterChecksum();
            _simulationWorkerSubmittedProvider?.AfterSimTick(publication.TickIndex);
            _simulationWorkerSubmittedProvider = null;
            _simulationWorkerPresentationAwaitingAcknowledgement =
                publication.HasPresentationFrame;

            if (debugLogPerTick)
            {
                Log.Info(
                    $"[SimulationTickDriver] ========== SimTick {publication.TickIndex} PUBLISHED ==========");
            }

            if (!_simulationWorkerPresentationAwaitingAcknowledgement)
                AcknowledgeDedicatedSimulationWorkerPresentation();
            return true;
        }

        private void AcknowledgeDedicatedSimulationWorkerPresentation()
        {
            long sequence = _simulationWorkerPendingAcknowledgementSequence;
            if (_simulationWorker == null || sequence <= 0)
                return;

            if (_simulationWorkerAcknowledgementSubmittedSequence < sequence)
            {
                _simulationWorker.AcknowledgePresentationConsumed(sequence);
                _simulationWorkerAcknowledgementSubmittedSequence = sequence;
            }
            _simulationWorkerPresentationAwaitingAcknowledgement = false;
            TryCompleteDedicatedSimulationWorkerPresentationConsumption();
        }

        private bool TryCompleteDedicatedSimulationWorkerPresentationConsumption()
        {
            long sequence = _simulationWorkerPendingAcknowledgementSequence;
            if (_simulationWorker == null || sequence <= 0 ||
                _simulationWorkerAcknowledgementSubmittedSequence < sequence ||
                !_simulationWorker.IsPresentationConsumptionFinalized(sequence))
            {
                return false;
            }

            _simulationWorkerPendingAcknowledgementSequence = 0;
            _simulationWorkerAcknowledgementSubmittedSequence = 0;
            _simulationWorkerPresentationAwaitingAcknowledgement = false;
            _simulationWorkerTickInFlight = false;
            _simulationWorkerSubmittedTick = 0;
            dedicatedSimulationWorkerTickInFlight = false;
            return true;
        }

        private bool PauseForDedicatedSimulationWorkerFailure()
        {
            System.Exception failure = _simulationWorker?.Failure;
            if (failure == null)
                return false;

            paused = true;
            if (!_simulationWorkerFailureReported)
            {
                _simulationWorkerFailureReported = true;
                Debug.LogError(
                    "[SimulationTickDriver] Dedicated simulation worker failed. " +
                    "Simulation has been paused; the world will not fall back after a partial tick.\n" +
                    failure);
            }
            return true;
        }

        private static void CopyFrameInput(
            FrameInputSet source,
            FrameInputSet destination,
            SimulationPlayerInput[] destinationPlayers)
        {
            int playerCount = source?.Players?.Count ?? 0;
            if (destination == null || destinationPlayers == null ||
                playerCount > destinationPlayers.Length)
            {
                throw new System.InvalidOperationException(
                    "The preallocated simulation input copy is too small.");
            }

            for (int index = 0; index < playerCount; index++)
                destinationPlayers[index] = source.Players[index];
            destination.ResetPreallocated(
                source?.TickIndex ?? 0,
                destinationPlayers,
                playerCount);
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
        public FrameInputSet LastAppliedFrameInput =>
            _lastAppliedFrameInput ?? _emptyLastAppliedFrameInput;
        public BattleParityFrameSnapshot LastFrameSnapshot => _lastFrameSnapshot;
        public IBattleChecksumSnapshot LastChecksumSnapshot => _lastChecksumSnapshot;
        public bool HasFrameChecksum => _hasFrameChecksum;
        public ulong LastFrameChecksumValue => _lastFrameChecksumValue;
        public string LastFrameChecksum => lastFrameChecksum;
        public BattlePresentationBackendMode PresentationBackendMode => _presentationBackendMode;
        public BattleAiExecutionProfile AiExecutionProfile => _aiExecutionProfile;
        public bool SuppressSoundPresentationForDiagnostics =>
            suppressSoundPresentationForDiagnostics;
        public long DispatchedSoundEventCountForDiagnostics => _dispatchedSoundEventCount;
        public long SuppressedSoundEventCountForDiagnostics => _suppressedSoundEventCount;
        public long RejectedPublishedSoundEventCountForDiagnostics =>
            _rejectedPublishedSoundEventCount;
        public int PendingPublishedSoundEventCountForDiagnostics =>
            _publishedSoundEvents.Count;
        public long FormalBattleDiagnosticsSuppressedCount =>
            _formalBattleDiagnosticsSuppressedCount;
        public long RejectedLatePresentationComponentCreateCount =>
            _rejectedLatePresentationComponentCreateCount;
        public bool DedicatedSimulationWorkerActiveForDiagnostics =>
            _simulationWorker != null && _simulationWorker.IsRunning;
        public bool DedicatedSimulationWorkerTickInFlightForDiagnostics
        {
            get
            {
                TryCompleteDedicatedSimulationWorkerPresentationConsumption();
                return _simulationWorkerTickInFlight;
            }
        }
        public System.Exception DedicatedSimulationWorkerFailureForDiagnostics =>
            _simulationWorker?.Failure;
        public string DedicatedSimulationWorkerIneligibilityReasonForDiagnostics =>
            _dedicatedSimulationWorkerIneligibilityReason;
        public string DedicatedSimulationWorkerLastSubmissionFailureReasonForDiagnostics =>
            _dedicatedSimulationWorkerLastSubmissionFailureReason;
        public long DedicatedSimulationWorkerLastExecutionElapsedTimestampTicksForDiagnostics =>
            _dedicatedSimulationWorkerLastExecutionElapsedTimestampTicks;

        public float RemainingAccumulatorTime => _timeAccumulator;
        public float RenderAlpha => renderAlpha;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;
        public BattleRuntimeAllocationGate AllocationGate => _allocationGate;
        public BattleManagedMemoryBoundary ManagedMemoryBoundary =>
            _managedMemoryBoundary;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        /// <summary>
        /// Schedules exactly one paused LocalFreeRun tick through the production
        /// dedicated-worker path. This is intentionally separate from StepOneTick,
        /// whose explicit/manual contract remains synchronous and stops the worker.
        /// </summary>
        public bool TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
            bool buildPresentation = true)
        {
            if (!paused)
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "diagnostic-worker-step-requires-paused-driver";
                return false;
            }
            if (!ShouldSubmitToDedicatedSimulationWorker())
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "worker-is-not-ready-for-diagnostic-submission";
                return false;
            }

            int tickIndex = _tickIndex + 1;
            if (!CanAdvanceTick(tickIndex))
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "next-diagnostic-tick-is-not-advanceable";
                return false;
            }

            ISimulationFrameInputProvider provider = _frameInputProvider;
            if (provider == null)
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "frame-input-provider-is-null";
                return false;
            }

            FrameInputSet frameInput = provider.GetFrameInput(tickIndex);
            if (frameInput == null || frameInput.TickIndex != tickIndex)
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "frame-input-provider-returned-an-invalid-tick";
                return false;
            }

            provider.BeforeSimTick(tickIndex);
            bool submitted = TrySubmitDedicatedSimulationWorkerTick(
                frameInput,
                buildPresentation,
                provider);
            RefreshInspectorState();
            return submitted;
        }

        public void BeginBattleAllocationSeal()
        {
            if (_world == null)
                return;
            if (_allocationGate.IsSealed && _world.RuntimeCapacity.IsSealed)
                return;

            if (lockstepSettings.DisableAllocatingDiagnosticsForFormalBattle())
                _formalBattleDiagnosticsSuppressedCount++;
            if (debugLogPerTick)
            {
                debugLogPerTick = false;
                _formalBattleDiagnosticsSuppressedCount++;
            }

            int maximumBodyCount = 1;
            int maximumItrCount = 1;
            CharacterAnimtorManager animatorManager = CharacterAnimtorManager.Instance;
            GameDataManager dataManager = GameDataManager.TryGetInstance();
            IReadOnlyList<ObjectDefinition> definitions = dataManager?.GetAllObjects();
            if (animatorManager != null && definitions != null && definitions.Count > 0)
            {
                _world.UnsealRuntimeDataCatalog();
                _world.PrepareRuntimeDataCatalogForBattle(
                    definitions,
                    animatorManager.GetCharacterConfig,
                    animatorManager.CommonVisualCatalog?.IsSparkValid == true
                        ? BattleHitRecordLifecycleCatalog.Available
                        : BattleHitRecordLifecycleCatalog.Unavailable);
            }

            animatorManager?.GetMaximumBattleCollisionRectCounts(
                out maximumBodyCount,
                out maximumItrCount);

            _allocationGate.PrepareNonUnityCapacity(
                _world.MaxRuntimeSlotsForServices,
                _world);
            _world.RuntimeCapacity.PrepareForBattle();
            _world.BattleBuffersForServices.Prepare(
                _world.MaxRuntimeSlotsForServices,
                _world.ObjectCount);
            _world.PrepareBattleHotPathCapacity(
                maximumBodyCount,
                maximumItrCount);
            BattleCentralRenderSystem.ResetRuntime();
            PreparePresentationHotPathCapacity(_world.MaxRuntimeSlotsForServices);
            AppManager.Instance?.SoundPlayer?.PrepareBattlePresentationHotPath();
            _world.PrepareEnabledBattleDiagnosticsHotPath();
            _world.RuntimeCapacity.Seal();
            _allocationGate.Seal(_world);
            _world.SetLogicOnlyEntityMaterialization(
                _presentationBackendMode == BattlePresentationBackendMode.CentralOnly &&
                _world.RuntimeDataCatalog?.IsReady == true);
            StartDedicatedSimulationWorkerIfEligible();
            _managedMemoryBoundary.CompleteLoadingAndOpenBattleWindow();
        }

        private void StartDedicatedSimulationWorkerIfEligible()
        {
            StopDedicatedSimulationWorker(resetLogicOnlyMaterialization: false);
            _dedicatedSimulationWorkerIneligibilityReason =
                ResolveDedicatedSimulationWorkerIneligibilityReason();
            _dedicatedSimulationWorkerLastExecutionElapsedTimestampTicks = 0L;
            if (!string.IsNullOrEmpty(_dedicatedSimulationWorkerIneligibilityReason))
            {
                RefreshDedicatedSimulationWorkerInspectorState();
                return;
            }

            _world.SetLogicOnlyEntityMaterialization(true);
            var executor = new BattleWorldSimulationTickExecutor(
                _world,
                _battleTickSystem,
                lockstepSettings.enableFrameChecksum,
                _managedMemoryBoundary);
            _simulationWorker = new DedicatedBattleSimulationWorker(
                inputCapacity: 1,
                maximumPlayerCount: MaximumSimulationWorkerPlayerSlots,
                executor: executor);
            try
            {
                _simulationWorker.Start();
                _simulationWorkerFailureReported = false;
            }
            catch
            {
                _simulationWorker.Dispose();
                _simulationWorker = null;
                RefreshDedicatedSimulationWorkerInspectorState();
                throw;
            }
            _dedicatedSimulationWorkerIneligibilityReason = string.Empty;
            RefreshDedicatedSimulationWorkerInspectorState();
        }

        private string ResolveDedicatedSimulationWorkerIneligibilityReason()
        {
            if (!useDedicatedSimulationWorker)
                return "disabled-by-driver-configuration";
            if (_world == null)
                return "world-not-created";
            if (_battleTickSystem == null)
                return "battle-tick-system-not-created";
            if (_presentationBackendMode != BattlePresentationBackendMode.CentralOnly)
                return "presentation-backend-is-not-central-only";
            if (lockstepSettings.driveMode != SimulationDriveMode.LocalFreeRun)
                return "drive-mode-is-not-local-free-run";
            if (lockstepSettings.requireInputFrameReady)
                return "input-ready-gate-is-enabled";
            if (lockstepSettings.captureFullFrameSnapshotForDiagnostics)
                return "allocating-full-frame-snapshot-is-enabled";
            if (_world.ForceLegacyPerPassStageRefreshForDiagnostics)
                return "legacy-per-pass-stage-refresh-is-enabled";
            if (_world.RuntimeDataCatalog?.IsReady != true)
                return "runtime-data-catalog-is-not-ready";
            return string.Empty;
        }

        private void StopDedicatedSimulationWorker(
            bool resetLogicOnlyMaterialization = true)
        {
            DedicatedBattleSimulationWorker worker = _simulationWorker;
            if (worker != null)
            {
                worker.Stop();
                worker.Dispose();
            }

            _simulationWorker = null;
            _simulationWorkerSubmittedProvider = null;
            _simulationWorkerTickInFlight = false;
            _simulationWorkerPresentationAwaitingAcknowledgement = false;
            _simulationWorkerSubmittedTick = 0;
            _simulationWorkerConsumedSequence = 0;
            _simulationWorkerPendingAcknowledgementSequence = 0;
            _simulationWorkerAcknowledgementSubmittedSequence = 0;
            _simulationWorkerSubmittedFrameInput.ResetPreallocated(0, null);
            _simulationWorkerCompletedFrameInput.ResetPreallocated(0, null);
            dedicatedSimulationWorkerTickInFlight = false;
            if (resetLogicOnlyMaterialization && !_allocationGate.IsSealed)
                _world?.SetLogicOnlyEntityMaterialization(false);
            if (_world != null)
                _world.BattlePresentation.FinalizePublishedHitRecordCycle(_world);
            RefreshDedicatedSimulationWorkerInspectorState();
        }

        private void RefreshDedicatedSimulationWorkerInspectorState()
        {
            dedicatedSimulationWorkerActive =
                _simulationWorker != null && _simulationWorker.IsRunning;
            dedicatedSimulationWorkerTickInFlight = _simulationWorkerTickInFlight;
        }

        private void PreparePresentationHotPathCapacity(int entityCapacity)
        {
            if (_managedMemoryFrameBeginProbe == null)
            {
                _managedMemoryFrameBeginProbe =
                    gameObject.GetComponent<BattleManagedMemoryFrameBeginProbe>() ??
                    gameObject.AddComponent<BattleManagedMemoryFrameBeginProbe>();
            }
            if (_managedMemoryFrameEndProbe == null)
            {
                _managedMemoryFrameEndProbe =
                    gameObject.GetComponent<BattleManagedMemoryFrameEndProbe>() ??
                    gameObject.AddComponent<BattleManagedMemoryFrameEndProbe>();
            }
            _managedMemoryFrameBeginProbe.Bind(this, _managedMemoryBoundary);
            _managedMemoryFrameEndProbe.Bind(this, _managedMemoryBoundary);

            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                {
                    _sparkRenderer =
                        gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
                }
            }

            int normalizedEntityCapacity = Mathf.Max(0, entityCapacity);
            int presentationTicks = Mathf.Max(1, lockstepSettings.maxBacklogTicks);
            long desiredPublishedSoundCapacity = System.Math.Max(
                256L,
                (long)normalizedEntityCapacity * 16L * presentationTicks);
            _publishedSoundEventLimit = (int)System.Math.Min(
                1_048_576L,
                desiredPublishedSoundCapacity);
            if (_publishedSoundEvents.Capacity < _publishedSoundEventLimit)
                _publishedSoundEvents.Capacity = _publishedSoundEventLimit;
            _sparkRenderer.PrepareCapacity(
                checked(
                    normalizedEntityCapacity *
                    NTSD.Animation.LF2Objects.LF2Entity.MaxHitRecordSlots));
            BattleSpriteCatalog spriteCatalog =
                CharacterAnimtorManager.Instance?.SpriteCatalog ?? BattleSpriteCatalog.Empty;
            BattleCentralRenderSystem.PrepareBattleCapacity(
                normalizedEntityCapacity,
                BattlePresentationCoordinator.CalculateMaximumCommandCapacity(
                    normalizedEntityCapacity),
                spriteCatalog.Count);
        }

        public void EndBattleAllocationSeal()
        {
            StopDedicatedSimulationWorker(resetLogicOnlyMaterialization: false);
            _managedMemoryBoundary.CloseBattleWindow();
            _allocationGate.Unseal(_world);
            _world?.SetLogicOnlyEntityMaterialization(false);
            _world?.RuntimeCapacity.Unseal();
            _world?.UnsealRuntimeDataCatalog();
            _world?.Runtime?.EnsureStageSpawnBuffers().Unseal();
            BattleCentralRenderSystem.EndBattleCapacitySeal();
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

            StopDedicatedSimulationWorker();
            lockstepSettings = settings;
            lockstepSettings.Normalize();
            if (lockstepSettings.driveMode != SimulationDriveMode.LocalFreeRun)
                _battleFunctionKeyInputLatch.Clear();
            SelectTickHostPolicy(resetSelectedPolicy: true);
            _timeAccumulator = _tickHostPolicy.Accumulator;
            RefreshInspectorState();
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            _battleFunctionKeyInputLatch.Clear();
            EndBattleAllocationSeal();
            _publishedSoundEvents.Clear();
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
                matchState.StageIdx = config != null && config.backgroundId >= 0
                    ? config.backgroundId
                    : 0;
                matchState.RandomStage = 0;
                matchState.RuntimeStageCount =
                    GameDataManager.TryGetInstance()?.BackgroundCount ?? 0;
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

        private void CaptureBattleFunctionKeyEdges()
        {
            BattleMatchRuntimeState match = _world?.Runtime?.Match;
            _battleFunctionKeyInputLatch.CapturePhysicalEdges(
                GameConfig.Instance,
                match?.LocalGameModeId ?? 0,
                match?.BattleGameModeId ?? 1,
                lockstepSettings.driveMode);
        }

        private void ApplyPendingBattleFunctionKeyCommandsForTick()
        {
            if (_world == null ||
                !_battleFunctionKeyInputLatch.TryConsume(
                    out bool toggleInitializeStats,
                    out int mode2Request))
            {
                return;
            }

            if (toggleInitializeStats)
                _world.ToggleInitStatsRequest();
            if (mode2Request != 0)
                _world.SetMode2Request(mode2Request);
        }

        public void QueueBattleFunctionKeyCommandsForDiagnostics(
            BattleFunctionKeyCommand commands)
        {
            _battleFunctionKeyInputLatch.QueueForDiagnostics(commands);
        }

        public void SetFrameInputProvider(ISimulationFrameInputProvider provider)
        {
            StopDedicatedSimulationWorker();
            _frameInputProvider = provider ??
                (lockstepSettings.driveMode == SimulationDriveMode.LocalFreeRun &&
                 !lockstepSettings.requireInputFrameReady
                    ? _localFrameInputProvider
                    : null);
            _frameInputProvider?.Reset();
            ResetLastAppliedFrameInput(_tickIndex);
        }

        public BattleLockstepSession CreateStrictLockstepSession(
            LockstepSessionIdentity identity,
            int futureFrameCapacity,
            int journalCapacity,
            int snapshotIntervalTicks = 0,
            int snapshotCapacity = 0)
        {
            lockstepSettings.Normalize();
            return new BattleLockstepSession(
                this,
                identity,
                lockstepSettings.inputDelayTicks,
                futureFrameCapacity,
                journalCapacity,
                snapshotIntervalTicks: snapshotIntervalTicks,
                snapshotCapacity: snapshotCapacity);
        }

        public bool StepOneTick(
            FrameInputSet frameInput,
            bool ignorePaused = false,
            bool buildPresentation = true)
        {
            if (!ignorePaused && paused)
                return false;

            StopDedicatedSimulationWorker();
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

            StopDedicatedSimulationWorker();
            bool stepped = StepOneTickInternal(_tickIndex + 1, buildPresentation);
            RefreshInspectorState();
            return stepped;
        }

        public bool TryRestoreBattleStateSnapshot(
            LockstepSessionIdentity identity,
            BattleStateSnapshotBuffer snapshot,
            out BattleStateSnapshotRestoreFailure failure)
        {
            StopDedicatedSimulationWorker();
            if (_world == null)
            {
                failure = BattleStateSnapshotRestoreFailure.WorldConfigurationMismatch;
                return false;
            }
            if (!_world.TryRestoreBattleStateSnapshot(identity, snapshot, out failure))
            {
                return false;
            }

            _tickIndex = snapshot.CapturedTick;
            _sparkRenderFrame = snapshot.Core.Flow.SparkRenderFrame;
            _offlineLocalTickPolicy.Reset();
            _manualReplayTickPolicy.Reset();
            _networkLockstepTickPolicy.Reset();
            SelectTickHostPolicy(resetSelectedPolicy: false);
            _timeAccumulator = 0f;
            ResetLastAppliedFrameInput(_tickIndex);
            _lastFrameSnapshot = null;
            _lastChecksumSnapshot = null;
            lastFrameChecksum = string.Empty;
            _lastFrameChecksumValue = 0UL;
            _hasFrameChecksum = false;
            _publishedSoundEvents.Clear();
            RefreshInspectorState();
            return true;
        }

        public void UnbindWorld()
        {
            EndBattleAllocationSeal();
            _publishedSoundEvents.Clear();
            if (_world != null)
                BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = null;
            _localFrameInputProvider.BindWorld(null);
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            EndBattleAllocationSeal();
            CreateProductionWorld();
            ResetDriverStateAfterWorldCreation();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
            _offlineLocalTickPolicy.Reset();
            _manualReplayTickPolicy.Reset();
            _networkLockstepTickPolicy.Reset();
            SelectTickHostPolicy(resetSelectedPolicy: false);
            _timeAccumulator = 0f;
            _sparkRenderFrame = 0;
            ResetLastAppliedFrameInput(0);
            _lastFrameSnapshot = null;
            _lastChecksumSnapshot = null;
            lastFrameChecksum = string.Empty;
            _lastFrameChecksumValue = 0UL;
            _hasFrameChecksum = false;
            _dispatchedSoundEventCount = 0;
            _suppressedSoundEventCount = 0;
            _rejectedPublishedSoundEventCount = 0;
            _publishedSoundEvents.Clear();
            _formalBattleDiagnosticsSuppressedCount = 0;
            _rejectedLatePresentationComponentCreateCount = 0;
            _frameInputProvider?.Reset();
            RefreshInspectorState();
        }

#if UNITY_EDITOR
        internal void FlushPublishedSoundEventsForTesting()
        {
            DispatchPublishedSounds();
        }
#endif

        private SimulationTickHostPolicy SelectTickHostPolicy(
            bool resetSelectedPolicy)
        {
            SimulationTickHostPolicy selected;
            if (lockstepSettings.driveMode == SimulationDriveMode.Manual)
            {
                selected = _manualReplayTickPolicy;
            }
            else if (lockstepSettings.driveMode == SimulationDriveMode.LockstepBuffered ||
                     lockstepSettings.requireInputFrameReady)
            {
                selected = _networkLockstepTickPolicy;
            }
            else
            {
                selected = _offlineLocalTickPolicy;
            }

            if (!ReferenceEquals(_tickHostPolicy, selected))
            {
                _tickHostPolicy?.Reset();
                _tickHostPolicy = selected;
                resetSelectedPolicy = true;
            }

            if (resetSelectedPolicy)
                _tickHostPolicy.Reset();
            return _tickHostPolicy;
        }

        private void ResetLastAppliedFrameInput(int tickIndex)
        {
            _emptyLastAppliedFrameInput.ResetPreallocated(tickIndex, null);
            _lastAppliedFrameInput = _emptyLastAppliedFrameInput;
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
            StopDedicatedSimulationWorker();
            BattlePresentationBackendResolver.ValidateAvailable(presentationMode);
            var nextWorld = new SimulationWorld(
                settings.Profile,
                settings.InitialRuntimeSlotCapacity,
                settings.CollisionBroadphase);
            nextWorld.BindLogicReferencePool(LF2ReferencePool.Instance.SimulationCore);
            nextWorld.ConfigureAiExecutionProfile(aiExecutionProfile);
            nextWorld.SetBattlePresentationBackend(presentationMode);
            if (_world != null)
                BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = nextWorld;
            _localFrameInputProvider.BindWorld(_world);
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
            EndBattleAllocationSeal();
            BattleCentralRenderSystem.ResetRuntime();
            _world?.BattlePresentation.Reset();
            _world = null;
            _localFrameInputProvider.BindWorld(null);
            _battleTickSystem = null;
        }
    }
}
