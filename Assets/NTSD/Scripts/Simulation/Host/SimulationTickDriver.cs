using System.Collections.Generic;
using System;
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
using UnityEngine.InputSystem;

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
    /// LocalFreeRun普通Host使用精确33ms；Manual/Lockstep只由显式frame推进。
    /// </summary>
    [System.Serializable]
    public sealed class LockstepSimulationSettings
    {
        public const int DefaultMaxTicksPerFrame = 1;

        [Tooltip("本地单机直接按时间推进；联机模式会等待指定逻辑帧输入就绪；手动模式只允许外部 StepOneTick 推进。")]
        public SimulationDriveMode driveMode = SimulationDriveMode.LocalFreeRun;

        [Tooltip("使用 unscaledDeltaTime 驱动外层逻辑时钟，避免 Time.timeScale 影响帧同步规则。")]
        public bool useUnscaledTime = true;

        [Tooltip("显式追帧或吞吐诊断的单帧逻辑预算。普通LocalFreeRun每个Unity Update最多排空2个active intervals。")]
        public int maxCatchUpTicksPerFrame = DefaultMaxTicksPerFrame;

        [Tooltip("显式追帧/worker诊断积压预算。普通LocalFreeRun wall-clock debt固定最多2个当前cadence interval，且每Update最多执行2tick。")]
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
        private readonly FrameInputSet capturedFrame =
            FrameInputSetPreallocation.CreateReusable();
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
    /// 负责NTSD 2.8 Host cadence逻辑tick，并把pass顺序交给NTSDBattleTickSystem。
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
        private static readonly ProfilerMarker AcknowledgeHitRecordMarker =
            new ProfilerMarker("NTSD.BattlePresentation.AcknowledgeHitRecordCycle");

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
        [SerializeField][MMReadOnly] private BattleRuntimeLifecycleState lifecycleState =
            BattleRuntimeLifecycleState.Uninitialized;
        private int preparationGeneration;
        private bool battleRuntimeServicesPrepared;
        [SerializeField][MMReadOnly] private BattleRuntimeShutdownStage shutdownStage =
            BattleRuntimeShutdownStage.None;
        [SerializeField][MMReadOnly] private float renderAlpha = 0f;
        [SerializeField][MMReadOnly] private int backlogTickCount = 0;
        [SerializeField][MMReadOnly] private bool fastMode = false;
        [SerializeField][MMReadOnly] private bool hostSingleStepPending = false;
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
            FrameInputSetPreallocation.CreateReusable();
        private readonly BattleFunctionKeyInputLatch _battleFunctionKeyInputLatch =
            new BattleFunctionKeyInputLatch();
        private readonly NTSD28NativeFunctionKeyPhysicalLatch
            _nativeFunctionKeyPhysicalLatch =
                new NTSD28NativeFunctionKeyPhysicalLatch();
        private readonly SimulationHostControlPhysicalEdgeLatch
            _hostControlPhysicalEdgeLatch =
                new SimulationHostControlPhysicalEdgeLatch();
        private SimulationHostControlCommand _pendingHostControlCommands;
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
        private readonly BattleRuntimeShutdownDiagnostics _shutdownDiagnostics =
            new BattleRuntimeShutdownDiagnostics();
        private LF2ObjectPointFactory _battleObjectPointFactory;
        private LF2ObjectPool _battleObjectPool;
        private readonly BattleManagedMemoryBoundary _managedMemoryBoundary =
            new BattleManagedMemoryBoundary();
        private BattleManagedMemoryFrameBeginProbe _managedMemoryFrameBeginProbe;
        private BattleManagedMemoryFrameEndProbe _managedMemoryFrameEndProbe;
        private const int MaximumSimulationWorkerPlayerSlots = 8;
        private DedicatedBattleSimulationWorker _simulationWorker;
        private readonly SimulationPlayerInput[] _simulationWorkerSubmittedPlayers =
            new SimulationPlayerInput[MaximumSimulationWorkerPlayerSlots];
        private readonly FrameInputSet _simulationWorkerSubmittedFrameInput =
            FrameInputSetPreallocation.CreateReusable();
        private readonly SimulationPlayerInput[] _simulationWorkerCompletedPlayers =
            new SimulationPlayerInput[MaximumSimulationWorkerPlayerSlots];
        private readonly FrameInputSet _simulationWorkerCompletedFrameInput =
            FrameInputSetPreallocation.CreateReusable();
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
        private string _hostControlLastFailureReason = string.Empty;
        private long _hostControlPhysicalSampleCount;
        private int _hostControlPhysicalKeyboardDeviceId = -1;
        private bool _hostControlPhysicalF1Pressed;
        private bool _hostControlPhysicalF2Pressed;
        private bool _hostControlPhysicalF5Pressed;
        private SimulationHostControlCommand _hostControlLastPhysicalEdges;
        private long _hostControlPhysicalEdgeCount;
        private long _hostControlAppliedCommandCount;
        private SimulationHostControlCommand _hostControlLastAppliedCommands;
        private bool _hostControlLastAppliedPausedBefore;
        private bool _hostControlLastAppliedPausedAfter;
        private byte _pendingNativeFunctionKeySessionEventByte;
        private bool _nativeFunctionKeyLeaveBattleRequested;
        private NTSD28NativeFunctionKeyMaintenanceCommand
            _nativeFunctionKeyMaintenanceCommand;
        private NTSD28NativeFunctionKeyHostCommand
            _nativeFunctionKeyContinuousHostCommand;
        private long _setPausedCallCount;
        private bool _setPausedLastValue;
        private long _dedicatedSimulationWorkerLastExecutionElapsedTimestampTicks;

        protected override void OnSingletonAwake()
        {
            EnterPreparingState();
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
                if (lifecycleState != BattleRuntimeLifecycleState.Running)
                {
                    RefreshInspectorState();
                    return;
                }

                TryCompleteDedicatedSimulationWorkerPresentationConsumption();
                ConsumeDedicatedSimulationWorkerPublication();
                TryCompleteDedicatedSimulationWorkerPresentationConsumption();
                if (PauseForDedicatedSimulationWorkerFailure())
                {
                    RefreshInspectorState();
                    return;
                }

                CaptureHostControlEdges();
                CaptureBattleFunctionKeyEdges();
                ApplyPendingHostControlCommands();

                if (paused || _world == null)
                {
                    if (paused)
                    {
                        ResetLocalHostDebt();
                        TryAdvancePendingPausedHostSingleStep();
                    }
                    RefreshInspectorState();
                    return;
                }

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
                if (lifecycleState != BattleRuntimeLifecycleState.Running)
                    return;

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
                    using (AcknowledgeHitRecordMarker.Auto())
                        _world?.BattlePresentation.AcknowledgePublishedHitRecordCycle();
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
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
                return false;

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
            SimulationWorld snapshotWorld = _world;
            snapshotWorld?.EnterSnapshotTickBoundary();
            try
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
                bool functionKeysDispatched = false;
                if (ShouldSubmitToDedicatedSimulationWorker())
                {
                    ApplyPendingBattleFunctionKeyCommandsForTick();
                    functionKeysDispatched = true;
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

                bool stepped = StepOneTickInternal(
                    frameInput,
                    buildPresentation,
                    !functionKeysDispatched);
                if (stepped)
                    provider.AfterSimTick(tickIndex);
                return stepped;
            }
            finally
            {
                snapshotWorld?.ExitSnapshotTickBoundary();
            }
        }

        private bool StepOneTickInternal(
            FrameInputSet frameInput,
            bool buildPresentation,
            bool applyPendingFunctionKeys = true)
        {
            SimulationWorld snapshotWorld = _world;
            snapshotWorld?.EnterSnapshotTickBoundary();
            try
            {
                if (_world == null || frameInput == null || frameInput.TickIndex != _tickIndex + 1)
                    return false;

                int tickIndex = frameInput.TickIndex;
                if (applyPendingFunctionKeys)
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
            finally
            {
                snapshotWorld?.ExitSnapshotTickBoundary();
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
            float activeInterval = ActiveHostIntervalSeconds;
            renderAlpha = Mathf.Clamp01(_timeAccumulator / activeInterval);
            backlogTickCount = Mathf.FloorToInt(_timeAccumulator / activeInterval);
            fastMode = _offlineLocalTickPolicy.CadenceMode ==
                SimulationHostCadenceMode.Fast;
            hostSingleStepPending =
                (_pendingHostControlCommands &
                 SimulationHostControlCommand.SingleStep) != 0;
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
        public bool IsFastMode =>
            _offlineLocalTickPolicy.CadenceMode == SimulationHostCadenceMode.Fast;
        public float ActiveHostIntervalSeconds =>
            lockstepSettings?.driveMode == SimulationDriveMode.LocalFreeRun
                ? _offlineLocalTickPolicy.ActiveIntervalSeconds
                : SimulationConstants.SIM_DT;
        public string HostControlLastFailureReasonForDiagnostics =>
            _hostControlLastFailureReason;
        public long HostControlPhysicalSampleCountForDiagnostics =>
            _hostControlPhysicalSampleCount;
        public int HostControlPhysicalKeyboardDeviceIdForDiagnostics =>
            _hostControlPhysicalKeyboardDeviceId;
        public bool HostControlPhysicalF1PressedForDiagnostics =>
            _hostControlPhysicalF1Pressed;
        public bool HostControlPhysicalF2PressedForDiagnostics =>
            _hostControlPhysicalF2Pressed;
        public bool HostControlPhysicalF5PressedForDiagnostics =>
            _hostControlPhysicalF5Pressed;
        internal SimulationHostControlCommand
            HostControlLastPhysicalEdgesForDiagnostics =>
                _hostControlLastPhysicalEdges;
        public long HostControlPhysicalEdgeCountForDiagnostics =>
            _hostControlPhysicalEdgeCount;
        public long HostControlAppliedCommandCountForDiagnostics =>
            _hostControlAppliedCommandCount;
        internal SimulationHostControlCommand
            HostControlLastAppliedCommandsForDiagnostics =>
                _hostControlLastAppliedCommands;
        public bool HostControlLastAppliedPausedBeforeForDiagnostics =>
            _hostControlLastAppliedPausedBefore;
        public bool HostControlLastAppliedPausedAfterForDiagnostics =>
            _hostControlLastAppliedPausedAfter;
        internal bool NativeFunctionKeyLeaveBattleRequestedForDiagnostics =>
            _nativeFunctionKeyLeaveBattleRequested;
        internal NTSD28NativeFunctionKeyMaintenanceCommand
            NativeFunctionKeyMaintenanceCommandForDiagnostics =>
                _nativeFunctionKeyMaintenanceCommand;
        internal NTSD28NativeFunctionKeyHostCommand
            NativeFunctionKeyContinuousHostCommandForDiagnostics =>
                _nativeFunctionKeyContinuousHostCommand;
        public long SetPausedCallCountForDiagnostics => _setPausedCallCount;
        public bool SetPausedLastValueForDiagnostics => _setPausedLastValue;
        public LockstepSimulationSettings Settings => lockstepSettings;

        public bool IsPaused => paused;
        public BattleRuntimeLifecycleState LifecycleState => lifecycleState;
        public BattleRuntimeShutdownStage ShutdownStageForDiagnostics => shutdownStage;
        public BattleRuntimeAllocationGate AllocationGate => _allocationGate;
        public BattleManagedMemoryBoundary ManagedMemoryBoundary =>
            _managedMemoryBoundary;

        public void SetPaused(bool value)
        {
            if (!value &&
                (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                 lifecycleState == BattleRuntimeLifecycleState.Stopped))
            {
                return;
            }

            _setPausedCallCount++;
            _setPausedLastValue = value;
            paused = value;
            if (value)
            {
                _pendingHostControlCommands &=
                    ~SimulationHostControlCommand.SingleStep;
                ResetLocalHostDebt();
            }
            if (!value && lifecycleState == BattleRuntimeLifecycleState.Preparing)
                lifecycleState = BattleRuntimeLifecycleState.Running;
        }

        /// <summary>
        /// Schedules exactly one paused LocalFreeRun tick through the production
        /// dedicated-worker path. This is intentionally separate from StepOneTick,
        /// whose explicit/manual contract remains synchronous and stops the worker.
        /// </summary>
        public bool TryScheduleDedicatedSimulationWorkerTickForDiagnostics(
            bool buildPresentation = true)
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
            {
                _dedicatedSimulationWorkerLastSubmissionFailureReason =
                    "battle-runtime-is-stopping-or-stopped";
                return false;
            }

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
            ApplyPendingBattleFunctionKeyCommandsForTick();
            bool submitted = TrySubmitDedicatedSimulationWorkerTick(
                frameInput,
                buildPresentation,
                provider);
            RefreshInspectorState();
            return submitted;
        }

        internal int PreparationGeneration => preparationGeneration;

        internal bool IsBattlePreparationCurrent(int generation, SimulationWorld expectedWorld)
        {
            return lifecycleState == BattleRuntimeLifecycleState.Preparing &&
                preparationGeneration == generation && _world != null &&
                ReferenceEquals(_world, expectedWorld);
        }

        internal void PrepareBattleRuntimeServices()
        {
            if (lifecycleState != BattleRuntimeLifecycleState.Preparing || _world == null)
                throw new InvalidOperationException("Battle services require a live preparing World.");

            LF2ObjectPool pool = LF2ObjectPool.TryGetInstance();
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.TryGetInstance();
            if (battleRuntimeServicesPrepared)
            {
                if (!ReferenceEquals(pool, _battleObjectPool) ||
                    !ReferenceEquals(factory, _battleObjectPointFactory) || pool == null || factory == null)
                    throw new InvalidOperationException("Prepared battle service owners were replaced.");
                return;
            }
            if (_world.ObjectCount != 0 || _world.ClaimedRuntimeSlotCountForServices != 0 ||
                _world.RuntimeCapacity.IsSealed ||
                (pool != null && (pool.ActiveObjectCountForAcceptance != 0 || pool.ActiveSpriteCountForAcceptance != 0)) ||
                (factory != null && factory.PendingTaskCountForDiagnostics != 0))
                throw new InvalidOperationException("Battle services must be owned before the first entity or queued spawn.");

            // Alignment contract: NTSD28-PREPARING-SHUTDOWN-OWNER-CAPTURE-001
            // Capture each owner before birth so Preparing shutdown uses the same services.
            _battleObjectPointFactory = LF2ObjectPointFactory.Instance;
            _battleObjectPool = LF2ObjectPool.Instance;
            _battleObjectPointFactory.BeginBattlePreparation();
            _battleObjectPool.BeginBattlePreparation();
            battleRuntimeServicesPrepared = true;
        }

        public void BeginBattleAllocationSeal()
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping)
                return;
            if (lifecycleState == BattleRuntimeLifecycleState.Stopped)
                EnterPreparingState();
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

            _world.BeginBattlePreparation();
            _battleObjectPointFactory = LF2ObjectPointFactory.Instance;
            _battleObjectPool = LF2ObjectPool.Instance;
            _battleObjectPointFactory?.BeginBattlePreparation();
            _battleObjectPool?.BeginBattlePreparation();

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
            if (_world.HasUnityPresentationBindingsForDedicatedWorker())
                return "unity-presentation-bindings-are-still-attached";
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
                _world.BattlePresentation.AcknowledgePublishedHitRecordCycle();
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
            EndBattleAllocationSealAfterWorkerStopped();
        }

        private void EndBattleAllocationSealAfterWorkerStopped()
        {
            _managedMemoryBoundary.CloseBattleWindow();
            _allocationGate.Unseal(_world);
            _world?.SetLogicOnlyEntityMaterialization(false);
            _world?.RuntimeCapacity.Unseal();
            _world?.UnsealRuntimeDataCatalog();
            _world?.Runtime?.EnsureStageSpawnBuffers().Unseal();
            BattleCentralRenderSystem.EndBattleCapacitySeal();
        }

        public BattleRuntimeShutdownReport ShutdownBattleRuntime()
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopped)
                return BuildShutdownReport(BattleRuntimeShutdownStatus.AlreadyStopped);
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping)
            {
                return string.IsNullOrEmpty(_shutdownDiagnostics.FailureReason)
                    ? BuildShutdownReport(
                        _shutdownDiagnostics.CompletedStage >=
                            BattleRuntimeShutdownStage.ObjectPoolQuiesced
                            ? BattleRuntimeShutdownStatus.AwaitingRuntimeMapCleanup
                            : BattleRuntimeShutdownStatus.AlreadyStopping)
                    : BuildShutdownReport(BattleRuntimeShutdownStatus.Failed);
            }

            lifecycleState = BattleRuntimeLifecycleState.Stopping;
            CharacterAnimtorManager.TryGetInstance()?.CancelConfiguredContentPrewarm();
            CharacterAnimtorManager.TryGetInstance()?.CancelNativeContentPrewarm();
            paused = true;
            _battleFunctionKeyInputLatch.Clear();
            ClearNativeFunctionKeyRoutingState();
            _hostControlPhysicalEdgeLatch.Clear();
            _pendingHostControlCommands = SimulationHostControlCommand.None;
            ResetLocalHostDebt();
            _frameInputProvider?.Reset();
            _localFrameInputProvider.Reset();
            _shutdownDiagnostics.Reset();
            CompleteShutdownStage(BattleRuntimeShutdownStage.TickAndInputClosed);

            try
            {
                StopDedicatedSimulationWorker(resetLogicOnlyMaterialization: false);
                if (_simulationWorker != null || _simulationWorkerTickInFlight ||
                    _simulationWorkerPresentationAwaitingAcknowledgement)
                {
                    return FailShutdown(
                        BattleRuntimeShutdownStage.TickAndInputClosed,
                        "dedicated-simulation-worker-did-not-stop");
                }
                CompleteShutdownStage(BattleRuntimeShutdownStage.WorkerStopped);

                _world?.BeginBattleShutdown();
                _battleObjectPointFactory?.BeginBattleShutdown(_world);
                _battleObjectPool?.BeginBattleShutdown();
                CompleteShutdownStage(BattleRuntimeShutdownStage.SpawnIntakeClosed);

                EndBattleAllocationSealAfterWorkerStopped();
                CompleteShutdownStage(BattleRuntimeShutdownStage.AllocationUnsealed);

                _publishedSoundEvents.Clear();
                CharacterAnimtorManager.TryGetInstance()?.ReleaseCancelledNativeContentStaging();
                BattleCentralRenderSystem.ResetRuntime();
                _world?.BattlePresentation.Reset();
                CompleteShutdownStage(BattleRuntimeShutdownStage.PresentationCleared);

                _shutdownDiagnostics.DiscardedObjectPointTasks +=
                    _world?.DiscardPendingObjectPointTasks() ?? 0;
                _shutdownDiagnostics.DiscardedObjectPointTasks +=
                    _battleObjectPointFactory?.DiscardPendingTasks() ?? 0;
                CompleteShutdownStage(
                    BattleRuntimeShutdownStage.PendingObjectPointTasksDiscarded);

                if (_battleObjectPool != null)
                {
                    if (!_battleObjectPool.ReleaseAllActiveForShutdown(
                            out int returnedRenderers,
                            out int returnedSpriteRenderers,
                            out string poolReleaseFailure))
                    {
                        return FailShutdown(
                            BattleRuntimeShutdownStage.PendingObjectPointTasksDiscarded,
                            poolReleaseFailure);
                    }
                    _shutdownDiagnostics.ReturnedRenderers += returnedRenderers;
                    _shutdownDiagnostics.ReturnedSpriteRenderers +=
                        returnedSpriteRenderers;
                }
                else if (_world?.HasUnityPresentationBindingsForDedicatedWorker() == true)
                {
                    return FailShutdown(
                        BattleRuntimeShutdownStage.PendingObjectPointTasksDiscarded,
                        "unity-renderers-remained-without-an-object-pool-owner");
                }
                CompleteShutdownStage(BattleRuntimeShutdownStage.RenderersReturned);

                if (_world != null &&
                    !_world.TryShutdownAndClearLogicState(
                        out _,
                        out string worldCleanupFailure))
                {
                    return FailShutdown(
                        BattleRuntimeShutdownStage.RenderersReturned,
                        worldCleanupFailure);
                }
                CompleteShutdownStage(BattleRuntimeShutdownStage.WorldLogicCleared);

                _world?.BindSnapshotHostBusyObserver(null);
                _world = null;
                _localFrameInputProvider.BindWorld(null);
                _battleTickSystem = null;
                ResetLastAppliedFrameInput(_tickIndex);
                CompleteShutdownStage(BattleRuntimeShutdownStage.WorldUnbound);

                if (_battleObjectPool != null &&
                    !_battleObjectPool.CompleteBattleQuiesce(
                        out string poolQuiesceFailure))
                {
                    return FailShutdown(
                        BattleRuntimeShutdownStage.WorldUnbound,
                        poolQuiesceFailure);
                }
                CompleteShutdownStage(BattleRuntimeShutdownStage.ObjectPoolQuiesced);
                return BuildShutdownReport(
                    BattleRuntimeShutdownStatus.AwaitingRuntimeMapCleanup);
            }
            catch (Exception exception)
            {
                return FailShutdown(
                    _shutdownDiagnostics.CompletedStage,
                    exception.GetType().Name + ": " + exception.Message);
            }
        }

        public BattleRuntimeShutdownReport CompleteBattleRuntimeShutdownAfterMapCleanup(
            bool runtimeMapCleared)
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopped)
                return BuildShutdownReport(BattleRuntimeShutdownStatus.AlreadyStopped);
            if (lifecycleState != BattleRuntimeLifecycleState.Stopping ||
                _shutdownDiagnostics.CompletedStage <
                    BattleRuntimeShutdownStage.ObjectPoolQuiesced)
            {
                return FailShutdown(
                    _shutdownDiagnostics.CompletedStage,
                    "runtime-stages-1-through-10-have-not-completed");
            }
            if (!runtimeMapCleared)
            {
                return FailShutdown(
                    BattleRuntimeShutdownStage.ObjectPoolQuiesced,
                    "runtime-map-carrier-has-not-been-cleared");
            }

            CompleteShutdownStage(BattleRuntimeShutdownStage.RuntimeMapCleared);
            _shutdownDiagnostics.ClearFailure();
            lifecycleState = BattleRuntimeLifecycleState.Stopped;
            _battleObjectPointFactory = null;
            _battleObjectPool = null;
            return BuildShutdownReport(BattleRuntimeShutdownStatus.Completed);
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
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
            {
                return;
            }

            StopDedicatedSimulationWorker();
            lockstepSettings = settings;
            lockstepSettings.Normalize();
            if (lockstepSettings.driveMode != SimulationDriveMode.LocalFreeRun)
            {
                _battleFunctionKeyInputLatch.Clear();
                ClearNativeFunctionKeyRoutingState();
                _hostControlPhysicalEdgeLatch.Clear();
                _pendingHostControlCommands = SimulationHostControlCommand.None;
            }
            SelectTickHostPolicy(resetSelectedPolicy: true);
            _timeAccumulator = _tickHostPolicy.Accumulator;
            RefreshInspectorState();
        }

        public void ApplyMatchConfig(MatchConfig config)
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping)
                throw new InvalidOperationException(
                    "A battle match cannot be prepared while runtime shutdown is in progress.");

            EnterPreparingState();
            _battleFunctionKeyInputLatch.Clear();
            ClearNativeFunctionKeyRoutingState();
            _hostControlPhysicalEdgeLatch.Clear();
            _pendingHostControlCommands = SimulationHostControlCommand.None;
            ResetLocalHostDebt();
            EndBattleAllocationSeal();
            _publishedSoundEvents.Clear();
            if (!EnsureProductionConfigurationFromSources())
                return;

            _world.ResetRuntimeState();
            _world.BeginBattlePreparation();
            _battleObjectPointFactory = LF2ObjectPointFactory.TryGetInstance();
            _battleObjectPool = LF2ObjectPool.TryGetInstance();
            _battleObjectPointFactory?.BeginBattlePreparation();
            _battleObjectPool?.BeginBattlePreparation();

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
            _world.NativeRandom?.ResetForDirectBattle(
                (uint)(config?.seed ?? 0));
            _world.SetOneTuInputForBattle(config?.oneTuInput ?? false);
            _world.Runtime?.Roster?.ApplyMatchConfig(config);
            _world.Runtime?.ApplyBootstrapFromMatchConfig(config);
            _world.SetNeedClearInput(true);
            _world.RefreshStageRuntimeSnapshotFromScene();

            List<BattleStageCampaignData> stageCampaigns = BattleStageCampaignLoader.LoadFromFile(
                config?.stageCampaignFilePath);
            if (!_world.ConfigureStageCampaigns(
                    stageCampaigns,
                    config?.stageSeriesId ?? 0,
                    -1))
            {
                throw new InvalidOperationException(
                    "Stage campaign projection failed before battle bootstrap.");
            }

            _world.SetAiPhaseGate(matchState != null && matchState.BattleGameModeId == 2 ? 1 : 0);
        }

        private SimulationHostControlCommand RouteNativeHostControlEdges(
            SimulationHostControlCommand physicalEdges)
        {
            SimulationHostControlCommand routed = SimulationHostControlCommand.None;
            NTSD28NativeFunctionKeyRouteContext context =
                BuildNativeFunctionKeyRouteContext();
            if ((physicalEdges & SimulationHostControlCommand.TogglePause) != 0)
            {
                routed |= MapNativeHostCommand(
                    NTSD28NativeFunctionKeyRouter.Route(
                        NTSD28NativeFunctionKey.F1,
                        false,
                        NTSD28NativeFunctionKeyModifiers.None,
                        context));
            }
            if ((physicalEdges & SimulationHostControlCommand.SingleStep) != 0)
            {
                routed |= MapNativeHostCommand(
                    NTSD28NativeFunctionKeyRouter.Route(
                        NTSD28NativeFunctionKey.F2,
                        false,
                        NTSD28NativeFunctionKeyModifiers.None,
                        context));
            }
            if ((physicalEdges & SimulationHostControlCommand.ToggleFastMode) != 0)
            {
                routed |= MapNativeHostCommand(
                    NTSD28NativeFunctionKeyRouter.Route(
                        NTSD28NativeFunctionKey.F5,
                        false,
                        NTSD28NativeFunctionKeyModifiers.None,
                        context));
            }
            return routed;
        }

        private static SimulationHostControlCommand MapNativeHostCommand(
            NTSD28NativeFunctionKeyRouteResult route)
        {
            if (route.Disposition !=
                NTSD28NativeFunctionKeyDisposition.HostCommand)
            {
                return SimulationHostControlCommand.None;
            }

            switch (route.HostCommand)
            {
                case NTSD28NativeFunctionKeyHostCommand.TogglePause:
                    return SimulationHostControlCommand.TogglePause;
                case NTSD28NativeFunctionKeyHostCommand.SingleStep:
                    return SimulationHostControlCommand.SingleStep;
                case NTSD28NativeFunctionKeyHostCommand.ToggleFastMode:
                    return SimulationHostControlCommand.ToggleFastMode;
                default:
                    return SimulationHostControlCommand.None;
            }
        }

        private NTSD28NativeFunctionKeyRouteContext
            BuildNativeFunctionKeyRouteContext()
        {
            bool battleActive =
                lifecycleState == BattleRuntimeLifecycleState.Running &&
                _world != null;
            bool f6F9Locked =
                _world?.Runtime?.FunctionKeys?.LockState == 2;
            return new NTSD28NativeFunctionKeyRouteContext(
                battleActive,
                true,
                f6F9Locked,
                true);
        }

        private NTSD28NativeFunctionKeySessionContext
            BuildNativeFunctionKeySessionContext()
        {
            BattleRuntimeState runtime = _world?.Runtime;
            bool battleActive =
                lifecycleState == BattleRuntimeLifecycleState.Running &&
                runtime != null;
            bool mainStateAllows = battleActive &&
                (runtime.Match?.LocalGameModeId ?? 0) == 0 &&
                runtime.Results?.IsActive != true;
            return new NTSD28NativeFunctionKeySessionContext(
                battleActive,
                mainStateAllows,
                true);
        }

        private void CollectNativeFunctionKeyRoute(
            NTSD28NativeFunctionKeyRouteResult route)
        {
            switch (route.Disposition)
            {
                case NTSD28NativeFunctionKeyDisposition.HostCommand:
                    if (route.HostCommand ==
                        NTSD28NativeFunctionKeyHostCommand.LeaveBattle)
                    {
                        _nativeFunctionKeyLeaveBattleRequested = true;
                    }
                    else
                    {
                        _pendingHostControlCommands |=
                            MapNativeHostCommand(route);
                    }
                    break;
                case NTSD28NativeFunctionKeyDisposition.SessionCommand:
                    _pendingNativeFunctionKeySessionEventByte = (byte)(
                        _pendingNativeFunctionKeySessionEventByte |
                        NTSD28NativeFunctionKeySessionState.EventMask(
                            route.SessionCommand));
                    break;
                case NTSD28NativeFunctionKeyDisposition.MaintenanceCommand:
                    _nativeFunctionKeyMaintenanceCommand =
                        route.MaintenanceCommand;
                    break;
                case NTSD28NativeFunctionKeyDisposition.ContinuousHostCommand:
                    _nativeFunctionKeyContinuousHostCommand = route.HostCommand;
                    break;
            }
        }

        private void ClearNativeFunctionKeyRoutingState()
        {
            _nativeFunctionKeyPhysicalLatch.Clear();
            _pendingNativeFunctionKeySessionEventByte = 0;
            _nativeFunctionKeyLeaveBattleRequested = false;
            _nativeFunctionKeyMaintenanceCommand =
                NTSD28NativeFunctionKeyMaintenanceCommand.None;
            _nativeFunctionKeyContinuousHostCommand =
                NTSD28NativeFunctionKeyHostCommand.None;
        }

        private void CaptureHostControlEdges()
        {
            if (lockstepSettings.driveMode != SimulationDriveMode.LocalFreeRun)
            {
                _pendingHostControlCommands = SimulationHostControlCommand.None;
                _hostControlPhysicalEdgeLatch.Clear();
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                _hostControlPhysicalEdgeLatch.Clear();
                _hostControlPhysicalKeyboardDeviceId = -1;
                _hostControlPhysicalF1Pressed = false;
                _hostControlPhysicalF2Pressed = false;
                _hostControlPhysicalF5Pressed = false;
                _hostControlLastPhysicalEdges =
                    SimulationHostControlCommand.None;
                return;
            }

            _hostControlPhysicalSampleCount++;
            _hostControlPhysicalKeyboardDeviceId = keyboard.deviceId;
            _hostControlPhysicalF1Pressed = keyboard.f1Key.isPressed;
            _hostControlPhysicalF2Pressed = keyboard.f2Key.isPressed;
            _hostControlPhysicalF5Pressed = keyboard.f5Key.isPressed;
            SimulationHostControlCommand physicalEdges =
                _hostControlPhysicalEdgeLatch.Capture(
                    _hostControlPhysicalF1Pressed,
                    _hostControlPhysicalF2Pressed,
                    _hostControlPhysicalF5Pressed);
            _hostControlLastPhysicalEdges =
                RouteNativeHostControlEdges(physicalEdges);
            if (_hostControlLastPhysicalEdges !=
                SimulationHostControlCommand.None)
            {
                _hostControlPhysicalEdgeCount++;
            }
            _pendingHostControlCommands |= _hostControlLastPhysicalEdges;
        }

        private void ApplyPendingHostControlCommands()
        {
            if (_pendingHostControlCommands == SimulationHostControlCommand.None ||
                lockstepSettings.driveMode != SimulationDriveMode.LocalFreeRun)
            {
                return;
            }

            SimulationHostControlCommand commands = _pendingHostControlCommands;
            _pendingHostControlCommands = SimulationHostControlCommand.None;
            _hostControlAppliedCommandCount++;
            _hostControlLastAppliedCommands = commands;
            _hostControlLastAppliedPausedBefore = paused;
            SimulationHostControlTransition transition =
                SimulationHostControl.Apply(
                    commands,
                    new SimulationHostControlState(
                        paused,
                        _offlineLocalTickPolicy.CadenceMode));
            paused = transition.State.Paused;
            _hostControlLastAppliedPausedAfter = paused;
            if (transition.CadenceChanged)
            {
                _offlineLocalTickPolicy.SetCadenceMode(
                    transition.State.CadenceMode);
                _timeAccumulator = _offlineLocalTickPolicy.Accumulator;
            }

            if (transition.RequestSingleStep)
            {
                _pendingHostControlCommands |=
                    SimulationHostControlCommand.SingleStep;
            }
            if (!paused)
            {
                _pendingHostControlCommands &=
                    ~SimulationHostControlCommand.SingleStep;
            }
            else
            {
                ResetLocalHostDebt();
            }
        }

        private bool TryAdvancePendingPausedHostSingleStep()
        {
            if ((_pendingHostControlCommands &
                 SimulationHostControlCommand.SingleStep) == 0)
            {
                _hostControlLastFailureReason = "no-pending-single-step";
                return false;
            }
            if (!paused || lockstepSettings.driveMode != SimulationDriveMode.LocalFreeRun ||
                lifecycleState != BattleRuntimeLifecycleState.Running)
            {
                _hostControlLastFailureReason =
                    "host-state-does-not-admit-single-step";
                _pendingHostControlCommands &=
                    ~SimulationHostControlCommand.SingleStep;
                return false;
            }

            int nextTick = _tickIndex + 1;
            if (_world == null)
            {
                _hostControlLastFailureReason = "world-is-null";
                return false;
            }
            if (_frameInputProvider == null)
            {
                _hostControlLastFailureReason = "frame-input-provider-is-null";
                return false;
            }
            if (!CanAdvanceTick(nextTick))
            {
                _hostControlLastFailureReason = "next-tick-is-not-advanceable";
                return false;
            }

            bool advanced = StepOneTickInternal(
                nextTick,
                buildPresentation: true);
            if (advanced)
            {
                _hostControlLastFailureReason = string.Empty;
                _pendingHostControlCommands &=
                    ~SimulationHostControlCommand.SingleStep;
            }
            else
            {
                _hostControlLastFailureReason =
                    "production-tick-entry-rejected-single-step";
            }
            ResetLocalHostDebt();
            return advanced;
        }

        private void ResetLocalHostDebt()
        {
            _offlineLocalTickPolicy.Reset();
            _timeAccumulator = 0f;
        }

        internal void QueueHostControlCommandsForDiagnostics(
            SimulationHostControlCommand commands)
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped ||
                lockstepSettings.driveMode != SimulationDriveMode.LocalFreeRun)
            {
                return;
            }

            _pendingHostControlCommands |= commands;
        }

        internal bool ProcessHostControlCommandsForDiagnostics()
        {
            ApplyPendingHostControlCommands();
            bool advanced = false;
            if (paused)
            {
                ResetLocalHostDebt();
                advanced = TryAdvancePendingPausedHostSingleStep();
            }
            RefreshInspectorState();
            return advanced;
        }

        private void CaptureBattleFunctionKeyEdges()
        {
            _nativeFunctionKeyPhysicalLatch.CapturePhysicalEdges(
                lockstepSettings.driveMode,
                BuildNativeFunctionKeyRouteContext());
            _nativeFunctionKeyContinuousHostCommand =
                _nativeFunctionKeyPhysicalLatch.CurrentContinuousHostCommand;
            if (!_nativeFunctionKeyPhysicalLatch.TryConsumeOneShotHandoff(
                    out byte sessionEventByte,
                    out bool leaveBattle,
                    out NTSD28NativeFunctionKeyMaintenanceCommand maintenanceCommand))
            {
                return;
            }

            _pendingNativeFunctionKeySessionEventByte = (byte)(
                _pendingNativeFunctionKeySessionEventByte | sessionEventByte);
            _nativeFunctionKeyLeaveBattleRequested |= leaveBattle;
            if (maintenanceCommand !=
                NTSD28NativeFunctionKeyMaintenanceCommand.None)
            {
                _nativeFunctionKeyMaintenanceCommand = maintenanceCommand;
            }
        }

        private void ApplyPendingBattleFunctionKeyCommandsForTick()
        {
            if (_world?.Runtime?.FunctionKeys == null)
                return;

            NTSD28NativeFunctionKeySessionState nativeState =
                _world.Runtime.FunctionKeys;
            bool previousHitResourceEnabled = nativeState.HitResourceEnabled;
            nativeState.QueueEventByte(_pendingNativeFunctionKeySessionEventByte);
            _pendingNativeFunctionKeySessionEventByte = 0;
            nativeState.Dispatch(BuildNativeFunctionKeySessionContext());
            if (nativeState.HitResourceEnabled != previousHitResourceEnabled)
            {
                _world.ProjectNativeHitResourceGateToActiveEntities(
                    nativeState.HitResourceEnabled);
            }

            // The R8 path remains explicit diagnostics only until B8 removes its
            // historical all-stats/mode2 effects. Production physical keys never
            // enter this latch after NTSD28-B2-FUNCTION-KEY-PRODUCTION-INTEGRATION-001.
            if (!_battleFunctionKeyInputLatch.TryConsume(
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
            if (lifecycleState != BattleRuntimeLifecycleState.Running)
                return;
            _battleFunctionKeyInputLatch.QueueForDiagnostics(commands);
        }

        internal NTSD28NativeFunctionKeyRouteResult
            QueueNativeFunctionKeyForDiagnostics(
                NTSD28NativeFunctionKey key,
                bool repeated = false,
                bool control = false)
        {
            NTSD28NativeFunctionKeyRouteResult route =
                NTSD28NativeFunctionKeyRouter.Route(
                    key,
                    repeated,
                    new NTSD28NativeFunctionKeyModifiers(control),
                    BuildNativeFunctionKeyRouteContext());
            CollectNativeFunctionKeyRoute(route);
            return route;
        }

        internal bool ConsumeNativeFunctionKeyLeaveBattleRequestForDiagnostics()
        {
            bool requested = _nativeFunctionKeyLeaveBattleRequested;
            _nativeFunctionKeyLeaveBattleRequested = false;
            return requested;
        }

        internal NTSD28NativeFunctionKeyMaintenanceCommand
            ConsumeNativeFunctionKeyMaintenanceCommandForDiagnostics()
        {
            NTSD28NativeFunctionKeyMaintenanceCommand command =
                _nativeFunctionKeyMaintenanceCommand;
            _nativeFunctionKeyMaintenanceCommand =
                NTSD28NativeFunctionKeyMaintenanceCommand.None;
            return command;
        }

        public void SetFrameInputProvider(ISimulationFrameInputProvider provider)
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
            {
                return;
            }
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
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
                return false;
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
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
                return false;
            if (!ignorePaused && paused)
                return false;

            StopDedicatedSimulationWorker();
            bool stepped = StepOneTickInternal(_tickIndex + 1, buildPresentation);
            RefreshInspectorState();
            return stepped;
        }

        private bool IsSnapshotHostBusy(SimulationWorld observedWorld)
        {
            if (!ReferenceEquals(_world, observedWorld) || _simulationWorkerTickInFlight ||
                _simulationWorkerPresentationAwaitingAcknowledgement ||
                lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
                return true;
            LF2ObjectPointFactory owner = _battleObjectPointFactory ?? LF2ObjectPointFactory.TryGetInstance();
            return owner != null && owner.HasPendingTasksForSnapshot(observedWorld, _world);
        }

        public bool TryRestoreBattleStateSnapshot(
            LockstepSessionIdentity identity,
            BattleStateSnapshotBuffer snapshot,
            out BattleStateSnapshotRestoreFailure failure)
        {
            if (lifecycleState == BattleRuntimeLifecycleState.Stopping ||
                lifecycleState == BattleRuntimeLifecycleState.Stopped)
            {
                failure = BattleStateSnapshotRestoreFailure.WorldConfigurationMismatch;
                return false;
            }
            if (_world == null)
            {
                failure = BattleStateSnapshotRestoreFailure.WorldConfigurationMismatch;
                return false;
            }
            if (!_world.IsBattleSnapshotBoundaryReady)
            {
                failure = BattleStateSnapshotRestoreFailure.WorldBusy;
                return false;
            }
            StopDedicatedSimulationWorker();
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
            BattleRuntimeShutdownReport report = ShutdownBattleRuntime();
            if (report.Status != BattleRuntimeShutdownStatus.Failed)
                CompleteBattleRuntimeShutdownAfterMapCleanup(true);
        }

        public void RecreateWorld()
        {
            BattleRuntimeShutdownReport shutdown = ShutdownBattleRuntime();
            if (shutdown.Status == BattleRuntimeShutdownStatus.Failed)
                throw new InvalidOperationException(shutdown.FailureReason);
            CompleteBattleRuntimeShutdownAfterMapCleanup(true);
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
            EnterPreparingState();
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
            _world?.BindSnapshotHostBusyObserver(null);
            _world = nextWorld;
            _world.BindSnapshotHostBusyObserver(IsSnapshotHostBusy);
            _localFrameInputProvider.BindWorld(_world);
            _presentationBackendMode = presentationMode;
            _aiExecutionProfile = aiExecutionProfile;
            effectiveAiExecutionProfile = aiExecutionProfile.ToString();
            _battleTickSystem = new NTSDBattleTickSystem(_world);
        }

        private void EnterPreparingState()
        {
            preparationGeneration = unchecked(preparationGeneration + 1);
            battleRuntimeServicesPrepared = false;
            lifecycleState = BattleRuntimeLifecycleState.Preparing;
            paused = true;
            _shutdownDiagnostics.Reset();
            shutdownStage = BattleRuntimeShutdownStage.None;
            _battleObjectPointFactory = null;
            _battleObjectPool = null;
        }

        private void CompleteShutdownStage(BattleRuntimeShutdownStage stage)
        {
            _shutdownDiagnostics.Complete(stage);
            shutdownStage = _shutdownDiagnostics.CompletedStage;
        }

        private BattleRuntimeShutdownReport FailShutdown(
            BattleRuntimeShutdownStage completedStage,
            string failureReason)
        {
            _shutdownDiagnostics.Fail(completedStage, failureReason);
            shutdownStage = _shutdownDiagnostics.CompletedStage;
            lifecycleState = BattleRuntimeLifecycleState.Stopping;
            paused = true;
            return BuildShutdownReport(BattleRuntimeShutdownStatus.Failed);
        }

        private BattleRuntimeShutdownReport BuildShutdownReport(
            BattleRuntimeShutdownStatus status)
        {
            return new BattleRuntimeShutdownReport(
                status,
                _shutdownDiagnostics.CompletedStage,
                _shutdownDiagnostics.FailureReason,
                _shutdownDiagnostics.DiscardedObjectPointTasks,
                _shutdownDiagnostics.ReturnedRenderers,
                _shutdownDiagnostics.ReturnedSpriteRenderers,
                _world?.ObjectCount ?? 0,
                _world?.ClaimedRuntimeSlotCountForDiagnostics ?? 0,
                (_battleObjectPool?.ActiveObjectCountForAcceptance ?? 0) +
                (_battleObjectPool?.ActiveSpriteCountForAcceptance ?? 0));
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
            BattleRuntimeShutdownReport report = ShutdownBattleRuntime();
            if (report.RuntimeStagesCompleted)
                CompleteBattleRuntimeShutdownAfterMapCleanup(true);
        }
    }
}
