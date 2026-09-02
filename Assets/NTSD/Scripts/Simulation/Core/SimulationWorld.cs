using System;
using System.Collections.Generic;
using System.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Presentation;
using NTSD.Simulation.Spatial;
using UnityEngine;

using AiGroundTeamPartition =
    NTSD.Simulation.SimulationAiInputModule.AiGroundTeamPartition;
using AiInputContext =
    NTSD.Simulation.SimulationAiInputModule.AiInputContext;
using AiNearestPointFilter =
    NTSD.Simulation.SimulationAiInputModule.AiNearestPointFilter;
using AiNearestSlotFacts =
    NTSD.Simulation.SimulationAiInputModule.AiNearestSlotFacts;
using AiNearestSnapshotStamp =
    NTSD.Simulation.SimulationAiInputModule.AiNearestSnapshotStamp;
using AiTeamHpSummary =
    NTSD.Simulation.SimulationAiInputModule.AiTeamHpSummary;
using AiSoANearestResult =
    NTSD.Simulation.SimulationAiSensingModule.AiSoANearestResult;
using AiSoASensingResult =
    NTSD.Simulation.SimulationAiSensingModule.AiSoASensingResult;
using AiSoASensingRows =
    NTSD.Simulation.SimulationAiSensingModule.AiSoASensingRows;
using AiSoASpecialResult =
    NTSD.Simulation.SimulationAiSensingModule.AiSoASpecialResult;
using AiUnifiedSnapshotExecutionState =
    NTSD.Simulation.SimulationAiDecisionModule.AiUnifiedSnapshotExecutionState;
using AiUnifiedSnapshotMutationWitness =
    NTSD.Simulation.SimulationAiDecisionModule.AiUnifiedSnapshotMutationWitness;
using AiDecisionRowContext =
    NTSD.Simulation.SimulationAiDecisionModule.AiDecisionRowContext;
using AiDecisionRowIdentity =
    NTSD.Simulation.SimulationAiDecisionModule.AiDecisionRowIdentity;

namespace NTSD.Simulation
{
    public readonly struct PendingSoundEvent
    {
        public PendingSoundEvent(string cue, int worldX, int tick)
        {
            Cue = cue;
            WorldX = worldX;
            Tick = tick;
        }

        public string Cue { get; }
        public int WorldX { get; }
        public int Tick { get; }
    }

    public interface ISimulationSoundPresentationSink
    {
        void PresentSounds(IReadOnlyList<PendingSoundEvent> sounds);
    }

    /// <summary>
    /// NTSD 战斗对象的确定性模拟调度器。普通 module 正在逐域接管状态与算法；
    /// 迁移期间主类仍保留兼容 façade 和尚未抽离的算法体，但不再使用 partial 共享实现。
    /// </summary>
    public class SimulationWorld
    {
        private readonly SimulationEntityTraversal entityTraversal;
        private readonly SimulationQueryAndLinkModule queryAndLinkModule;
        private readonly SimulationBattleBufferModule battleBuffers;
        private readonly SimulationRuntimeCapacityModule runtimeCapacityModule;
        private readonly SimulationFrameInputModule frameInputModule;
        private FrameInputSet currentAppliedFrameInput;
        private readonly SimulationRegistryModule registryModule;
        private readonly SimulationAiRuntime aiRuntime;
        private readonly SimulationPassPipeline passPipeline;
        private readonly StageSpawnTaskConfigurator stageSpawnTaskConfigurator;
        private readonly SimulationStageWaveModule stageWaveModule;
        private readonly SimulationStageRenderModule stageRenderModule;
        private readonly BattleParitySnapshotModule paritySnapshotModule;
        private readonly RuntimeCharacterConfigResolver runtimeCharacterConfigs;
        private readonly BattleRuntimeDataCatalog runtimeDataCatalog;
        private BattleLogicReferencePool logicReferencePool;
        private readonly BattleLogicEntityFactory logicEntityFactory;
        private readonly BattleLogicObjectPointRuntime logicObjectPointRuntime;
        private readonly BattleLockstepChecksumModule lockstepChecksumModule;
        private readonly BattleWorldCoreScalarSnapshotModule
            battleWorldCoreScalarSnapshotModule;
        private readonly BattleWorldRosterResultsSnapshotModule
            battleWorldRosterResultsSnapshotModule;
        private readonly BattleWorldStageSpawnSnapshotModule
            battleWorldStageSpawnSnapshotModule;
        private readonly BattleWorldRuntimeSlotSnapshotModule
            battleWorldRuntimeSlotSnapshotModule;
        private readonly BattleWorldEntityRuntimeSnapshotModule
            battleWorldEntityRuntimeSnapshotModule;
        private readonly BattleWorldEntityBaseShellSnapshotModule
            battleWorldEntityBaseShellSnapshotModule;
        private readonly BattleWorldLivingShellSnapshotModule
            battleWorldLivingShellSnapshotModule;
        private readonly BattleWorldCharacterShellSnapshotModule
            battleWorldCharacterShellSnapshotModule;
        private readonly BattleWorldWeaponShellSnapshotModule
            battleWorldWeaponShellSnapshotModule;
        private readonly BattleWorldSpecialOtherShellSnapshotModule
            battleWorldSpecialOtherShellSnapshotModule;
        private readonly BattleWorldPendingEventSnapshotModule
            battleWorldPendingEventSnapshotModule;
        private readonly BattleWorldRestSnapshotModule
            battleWorldRestSnapshotModule;
        private readonly BattleStateSnapshotRestoreModule
            battleStateSnapshotRestoreModule;
        private readonly BattleEcsShadowModule battleEcsShadowModule;
        private readonly BattleEcsCooldownPass battleEcsCooldownPass;
        private readonly BattleEcsCharacterStageZPass battleEcsCharacterStageZPass;
        private readonly BattleEcsCharacterPreFrameBoundsPass
            battleEcsCharacterPreFrameBoundsPass;
        private readonly BattleEcsFramePostProcessPass battleEcsFramePostProcessPass;
        private readonly BattleEcsPositiveLinkValidationPass
            battleEcsPositiveLinkValidationPass;
        private readonly BattleEcsCharacterFrameAdvancePass
            battleEcsCharacterFrameAdvancePass;
        private readonly BattleEcsCharacterRecoveryPass
            battleEcsCharacterRecoveryPass;
        private readonly BattleEcsCharacterFrameTickPass
            battleEcsCharacterFrameTickPass;
        private readonly BattleEcsCharacterInputPass
            battleEcsCharacterInputPass;
        private readonly BattleEcsCharacterPostFrameTailPass
            battleEcsCharacterPostFrameTailPass;
        private readonly BattleEcsHitExecutionPlan battleEcsHitExecutionPlan;
        private readonly BattleAiUnifiedRowPublisher battleAiUnifiedRowPublisher;
        private readonly BattleIdentityWriter battleIdentityWriter;
        private readonly BattleCharacterInputActionResolver battleCharacterInputActionResolver;
        private readonly BattleCharacterInputWriter battleCharacterInputWriter;
        private readonly BattleFrameMotionWriter battleFrameMotionWriter;
        private readonly BattleRelationLinkWriter battleRelationLinkWriter;
        private readonly BattleVitalWriter battleVitalWriter;
        private readonly BattleCharacterActionWriter battleCharacterActionWriter;
        private readonly BattleAiInputWriter battleAiInputWriter;
        private readonly BattleBoundaryWriter battleBoundaryWriter;
        private readonly BattleInteractionWriter battleInteractionWriter;
        private readonly BattleHeldObjectWriter battleHeldObjectWriter;
        private readonly BattleCpointWriter battleCpointWriter;
        private readonly BattleDamageWriter battleDamageWriter;
        private readonly BattleStructuralWriter battleStructuralWriter;
        private readonly BattleResultsReserveHostWriter battleResultsReserveHostWriter;
        private readonly BattleResultsOutcomeHostWriter battleResultsOutcomeHostWriter;
        private readonly BattleResultsWriter battleResultsWriter;
        private readonly CharacterMechanics characterMechanics;
        private readonly SimulationDiagnosticsModule diagnosticsModule =
            new SimulationDiagnosticsModule();
        private readonly SimulationWorldMutationTracker runtimeMutationTracker;
        private readonly SimulationWorldHooks runtimeHooks =
            new SimulationWorldHooks();
        private bool logicOnlyEntityMaterialization;
        private SimContext _context;

        public ILF2SceneQuery SceneQuery { get; private set; }
        public INTSDItrKindService ItrKindService { get; private set; }
        public DeterministicRng Rng { get; private set; }
        public BattleRuntimeState Runtime { get; private set; }
        public int[] KillStats => Runtime.KillStats;
        public int[] DamageStats => Runtime.DamageStats;

        public SimulationWorld()
            : this(BattleRuntimeProfile.Authority400, AuthorityRuntimeSlotCapacity)
        {
        }

        internal SimulationWorld(
            RuntimeCharacterConfigResolver characterConfigResolver)
            : this(
                BattleRuntimeProfile.Authority400,
                AuthorityRuntimeSlotCapacity,
                CollisionBroadphaseBackend.BruteForce,
                characterConfigResolver)
        {
        }

        public SimulationWorld(
            BattleRuntimeProfile runtimeProfile,
            int runtimeSlotCapacity,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce,
            RuntimeCharacterConfigResolver characterConfigResolver = null)
        {
            if (runtimeSlotCapacity < DynamicRuntimeSlotStart)
                throw new ArgumentOutOfRangeException(nameof(runtimeSlotCapacity),
                    "Runtime slot capacity must include the dynamic slot band.");
            if (runtimeProfile == BattleRuntimeProfile.Authority400 &&
                runtimeSlotCapacity != AuthorityRuntimeSlotCapacity)
            {
                throw new ArgumentException(
                    "Authority400 worlds must use exactly 400 runtime slots.",
                    nameof(runtimeSlotCapacity));
            }

            CollisionBroadphaseForServices = collisionBroadphase;
            int maxActiveRuntimeEntities = runtimeProfile == BattleRuntimeProfile.MobileExtended
                ? BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities
                : int.MaxValue;
            registryModule = new SimulationRegistryModule(
                this,
                runtimeSlotCapacity,
                20,
                DynamicRuntimeSlotStart,
                runtimeProfile,
                maxActiveRuntimeEntities);
            aiRuntime = new SimulationAiRuntime(
                this,
                runtimeSlotCapacity,
                registryModule.RuntimeSlots);
            passPipeline = new SimulationPassPipeline(this);
            battleEcsShadowModule = new BattleEcsShadowModule(
                this,
                new BattleEcsCapacityProfile(runtimeProfile, runtimeSlotCapacity));
            battleEcsCooldownPass = new BattleEcsCooldownPass(
                this,
                _runtimeSlots,
                _runtimeRestStore,
                runtimeSlotCapacity);
            battleBuffers = new SimulationBattleBufferModule(runtimeSlotCapacity);
            runtimeCapacityModule = new SimulationRuntimeCapacityModule(
                _runtimeSlots,
                _runtimeRestStore,
                battleBuffers,
                objectBucketRegistry);
            frameInputModule = new SimulationFrameInputModule(this);
            stageSpawnTaskConfigurator = new StageSpawnTaskConfigurator();
            runtimeCharacterConfigs =
                characterConfigResolver ?? new RuntimeCharacterConfigResolver();
            runtimeDataCatalog = new BattleRuntimeDataCatalog();
            logicReferencePool = new BattleLogicReferencePool();
            logicEntityFactory = new BattleLogicEntityFactory(this);
            logicObjectPointRuntime = new BattleLogicObjectPointRuntime(
                this,
                runtimeSlotCapacity);
            runtimeCharacterConfigs.BindRuntimeDataCatalog(runtimeDataCatalog);
            entityTraversal = new SimulationEntityTraversal(this, _runtimeSlots);
            queryAndLinkModule = new SimulationQueryAndLinkModule(this);
            lockstepChecksumModule = new BattleLockstepChecksumModule();
            battleWorldCoreScalarSnapshotModule =
                new BattleWorldCoreScalarSnapshotModule(this);
            battleWorldRosterResultsSnapshotModule =
                new BattleWorldRosterResultsSnapshotModule(this);
            battleWorldStageSpawnSnapshotModule =
                new BattleWorldStageSpawnSnapshotModule(this);
            battleWorldRuntimeSlotSnapshotModule =
                new BattleWorldRuntimeSlotSnapshotModule(this);
            battleWorldEntityRuntimeSnapshotModule =
                new BattleWorldEntityRuntimeSnapshotModule(this);
            battleWorldEntityBaseShellSnapshotModule =
                new BattleWorldEntityBaseShellSnapshotModule(this);
            battleWorldLivingShellSnapshotModule =
                new BattleWorldLivingShellSnapshotModule(this);
            battleWorldCharacterShellSnapshotModule =
                new BattleWorldCharacterShellSnapshotModule(this);
            battleWorldWeaponShellSnapshotModule =
                new BattleWorldWeaponShellSnapshotModule(this);
            battleWorldSpecialOtherShellSnapshotModule =
                new BattleWorldSpecialOtherShellSnapshotModule(this);
            battleWorldPendingEventSnapshotModule =
                new BattleWorldPendingEventSnapshotModule(this, battleBuffers);
            battleWorldRestSnapshotModule =
                new BattleWorldRestSnapshotModule(_runtimeRestStore);
            battleStateSnapshotRestoreModule =
                new BattleStateSnapshotRestoreModule(this);
            stageWaveModule = new SimulationStageWaveModule(this);
            stageRenderModule = new SimulationStageRenderModule(this);
            battleEcsCharacterStageZPass = new BattleEcsCharacterStageZPass(
                this,
                _runtimeSlots,
                runtimeSlotCapacity);
            battleEcsCharacterPreFrameBoundsPass =
                new BattleEcsCharacterPreFrameBoundsPass(
                    this,
                    _runtimeSlots);
            battleEcsFramePostProcessPass = new BattleEcsFramePostProcessPass(
                this,
                _runtimeSlots,
                runtimeSlotCapacity);
            battleEcsPositiveLinkValidationPass =
                new BattleEcsPositiveLinkValidationPass(
                    this,
                    _runtimeSlots,
                    runtimeSlotCapacity);
            battleEcsCharacterFrameAdvancePass =
                new BattleEcsCharacterFrameAdvancePass(this);
            battleEcsCharacterRecoveryPass =
                new BattleEcsCharacterRecoveryPass(this);
            battleEcsCharacterFrameTickPass =
                new BattleEcsCharacterFrameTickPass();
            battleEcsCharacterInputPass =
                new BattleEcsCharacterInputPass(this);
            battleEcsCharacterPostFrameTailPass =
                new BattleEcsCharacterPostFrameTailPass();
            battleEcsHitExecutionPlan = new BattleEcsHitExecutionPlan(
                this,
                runtimeSlotCapacity);
            battleAiUnifiedRowPublisher = new BattleAiUnifiedRowPublisher(
                runtimeSlotCapacity);
            battleIdentityWriter = new BattleIdentityWriter(runtimeSlotCapacity);
            battleCharacterInputActionResolver = new BattleCharacterInputActionResolver();
            battleCharacterInputWriter = new BattleCharacterInputWriter(
                this,
                runtimeSlotCapacity,
                battleAiUnifiedRowPublisher);
            battleFrameMotionWriter = new BattleFrameMotionWriter(
                runtimeSlotCapacity,
                battleAiUnifiedRowPublisher);
            battleRelationLinkWriter = new BattleRelationLinkWriter(
                runtimeSlotCapacity,
                battleAiUnifiedRowPublisher);
            battleVitalWriter = new BattleVitalWriter(
                runtimeSlotCapacity,
                battleAiUnifiedRowPublisher);
            runtimeMutationTracker = new SimulationWorldMutationTracker();
            battleCharacterActionWriter = new BattleCharacterActionWriter();
            battleAiInputWriter = new BattleAiInputWriter(
                this,
                battleCharacterInputWriter);
            battleBoundaryWriter = new BattleBoundaryWriter(
                battleCharacterInputWriter);
            battleInteractionWriter = new BattleInteractionWriter();
            battleHeldObjectWriter = new BattleHeldObjectWriter();
            battleCpointWriter = new BattleCpointWriter();
            battleDamageWriter = new BattleDamageWriter();
            battleStructuralWriter = new BattleStructuralWriter(this);
            battleResultsReserveHostWriter =
                new BattleResultsReserveHostWriter(this);
            battleResultsOutcomeHostWriter =
                new BattleResultsOutcomeHostWriter(this);
            battleResultsWriter = new BattleResultsWriter(this);
            characterMechanics = new CharacterMechanics();
            paritySnapshotModule = new BattleParitySnapshotModule(this);
            InitializeAiSoASensingRows(runtimeSlotCapacity);
            aiCharacterDecisionLegacyFallbackSnapshot =
                new AiDecisionSnapshot(runtimeSlotCapacity);
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this, collisionBroadphase);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
        }

        internal int ActiveDataObjectTypeCacheTick { get; private set; } = -1;
        public SimContext Context => _context;

        public int CurrentTickIndex => Runtime?.Flow?.CurrentTickIndex ?? 0;
        public int SparkRenderFrame => Runtime?.Flow?.SparkRenderFrame ?? 0;
        public int BattleGameModeId => Runtime?.Match?.BattleGameModeId ?? 0;
        public int LocalGameModeId => Runtime?.Match?.LocalGameModeId ?? 0;
        public int Difficulty => Runtime?.Match?.Difficulty ?? 2;
        public int BackgroundId => Runtime?.Match?.BackgroundId ?? -1;
        public int MatchSeed => Runtime?.Match?.Seed ?? 0;
        public int AiPhaseGate => Runtime?.Flow?.AiPhaseGate ?? 0;
        public int InputPhase => Runtime?.Flow?.InputPhase ?? 0;
        public int FrameMod12 => Runtime?.Flow?.FrameMod12 ?? 0;
        public int FrameToggle => Runtime?.Flow?.FrameToggle ?? 0;
        public int BattleExitCountdown => Runtime?.Flow?.BattleExitCountdown ?? 0;
        public int RouteOutRequest => Runtime?.Flow?.RouteOutRequest ?? 0;
        public int Mode2Request => Runtime?.Flow?.Mode2Request ?? 0;
        public int InitStatsRequest => Runtime?.Flow?.InitStatsRequest ?? 0;
        public bool NeedClearInput => Runtime?.Flow?.NeedClearInput ?? false;
        public BattleStageCampaignSet StageCampaigns => Runtime?.StageCampaigns ?? BattleStageCampaignSet.Empty;
        public BattleStageProgressionState StageProgression => Runtime?.StageProgression;
        public bool StageProgressionValid => Runtime?.StageProgressionValid ?? false;
        public int StageSpawnWaveApplied => Runtime?.StageSpawnWaveApplied ?? -1;
        public int StageSpawnWaveDeferredEntryApplied => Runtime?.StageSpawnWaveDeferredEntryApplied ?? -1;
        public int StageSpawnRuntimeWave => Runtime?.StageSpawnRuntimeWave ?? -1;
        public List<int> StageSpawnRuntimeTargetTotal => Runtime?.StageSpawnRuntimeTargetTotal;
        public List<int> StageSpawnRuntimeEntryCount => Runtime?.StageSpawnRuntimeEntryCount;
        public List<int> StageSpawnRuntimeSpawnedTotal => Runtime?.StageSpawnRuntimeSpawnedTotal;
        public List<int[]> StageSpawnRuntimeSlots => Runtime?.StageSpawnRuntimeSlots;

        public void SetAiPhaseGate(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.AiPhaseGate = value;
        }

        public void SetBattleExitCountdown(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.BattleExitCountdown = value;
        }

        public void SetRouteOutRequest(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.RouteOutRequest = value;
        }

        public void SetMode2Request(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.Mode2Request = value;
        }

        public void ToggleInitStatsRequest()
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.InitStatsRequest = Runtime.Flow.InitStatsRequest == 0 ? 1 : 0;
            Runtime.Flow.BattleExitCountdown = 0;
        }

        public void SetInitStatsRequest(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.InitStatsRequest = value != 0 ? 1 : 0;
        }

        public void SetNeedClearInput(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Flow ??= new BattleFlowRuntimeState();
            Runtime.Flow.NeedClearInput = value;
        }

        public void AdvanceBattleFlowTick(int tickIndex)
        {
            if (Runtime?.Flow == null)
                return;

            Runtime.Flow.CurrentTickIndex = tickIndex;
            Runtime.Flow.InputPhase = (Runtime.Flow.InputPhase + 1) & 1;
            Runtime.Flow.FrameMod12 = tickIndex % 12;
            Runtime.Flow.FrameToggle = 1 - Runtime.Flow.FrameToggle;
        }

        public void SetStageProgressionValid(bool value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageProgressionValid = value;
        }

        internal void SetStageCampaigns(BattleStageCampaignSet value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Runtime ??= new BattleRuntimeState();
            Runtime.StageCampaigns = value;
        }

        public void SetStageSpawnWaveApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveApplied = value;
        }

        public void SetStageSpawnWaveDeferredEntryApplied(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnWaveDeferredEntryApplied = value;
        }

        public void SetStageSpawnRuntimeWave(int value)
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.StageSpawnRuntimeWave = value;
        }

        public bool PpMode
        {
            get => Runtime?.Match?.PpMode ?? true;
            set
            {
                if (Runtime?.Match != null)
                    Runtime.Match.PpMode = value;
            }
        }

        public List<PendingSoundEvent> PendingSounds => battleBuffers.PendingSounds;
        public long QueuedSoundEventCountForDiagnostics { get; private set; }
        public BattleEcsCapacityProfile BattleEcsCapacityProfileForDiagnostics => battleEcsShadowModule.CapacityProfile;
        public BattleEcsShadowMode BattleEcsShadowModeForDiagnostics => battleEcsShadowModule.Mode;
        public BattleEcsShadowDiagnostics BattleEcsShadowDiagnosticsForDiagnostics => battleEcsShadowModule.Diagnostics;
        public BattleEcsCooldownPassMode BattleEcsCooldownPassModeForDiagnostics => battleEcsCooldownPass.Mode;
        public BattleEcsCooldownPassDiagnostics BattleEcsCooldownPassDiagnosticsForDiagnostics => battleEcsCooldownPass.Diagnostics;
        public BattleEcsCharacterStageZPassMode BattleEcsCharacterStageZPassModeForDiagnostics => battleEcsCharacterStageZPass.Mode;
        public BattleEcsCharacterStageZPassDiagnostics BattleEcsCharacterStageZPassDiagnosticsForDiagnostics => battleEcsCharacterStageZPass.Diagnostics;
        public BattleEcsCharacterPreFrameBoundsPassMode
            BattleEcsCharacterPreFrameBoundsPassModeForDiagnostics =>
                battleEcsCharacterPreFrameBoundsPass.Mode;
        public BattleEcsCharacterPreFrameBoundsPassDiagnostics
            BattleEcsCharacterPreFrameBoundsPassDiagnosticsForDiagnostics =>
                battleEcsCharacterPreFrameBoundsPass.Diagnostics;
        public BattleEcsFramePostProcessPassMode BattleEcsFramePostProcessPassModeForDiagnostics => battleEcsFramePostProcessPass.Mode;
        public BattleEcsFramePostProcessPassDiagnostics BattleEcsFramePostProcessPassDiagnosticsForDiagnostics => battleEcsFramePostProcessPass.Diagnostics;
        public BattleEcsPositiveLinkValidationPassMode
            BattleEcsPositiveLinkValidationPassModeForDiagnostics =>
                battleEcsPositiveLinkValidationPass.Mode;
        public BattleEcsPositiveLinkValidationPassDiagnostics
            BattleEcsPositiveLinkValidationPassDiagnosticsForDiagnostics =>
                battleEcsPositiveLinkValidationPass.Diagnostics;
        public BattleEcsCharacterFrameAdvancePassMode
            BattleEcsCharacterFrameAdvancePassModeForDiagnostics =>
                battleEcsCharacterFrameAdvancePass.Mode;
        public BattleEcsCharacterFrameAdvancePassDiagnostics
            BattleEcsCharacterFrameAdvancePassDiagnosticsForDiagnostics =>
                battleEcsCharacterFrameAdvancePass.Diagnostics;
        public BattleEcsCharacterRecoveryPassMode
            BattleEcsCharacterRecoveryPassModeForDiagnostics =>
                battleEcsCharacterRecoveryPass.Mode;
        public BattleEcsCharacterRecoveryPassDiagnostics
            BattleEcsCharacterRecoveryPassDiagnosticsForDiagnostics =>
                battleEcsCharacterRecoveryPass.Diagnostics;
        public BattleEcsCharacterFrameTickPassMode
            BattleEcsCharacterFrameTickPassModeForDiagnostics =>
                battleEcsCharacterFrameTickPass.Mode;
        public BattleEcsCharacterFrameTickPassDiagnostics
            BattleEcsCharacterFrameTickPassDiagnosticsForDiagnostics =>
                battleEcsCharacterFrameTickPass.Diagnostics;
        public BattleEcsCharacterInputPassMode
            BattleEcsCharacterInputPassModeForDiagnostics =>
                battleEcsCharacterInputPass.Mode;
        public BattleEcsCharacterInputPassDiagnostics
            BattleEcsCharacterInputPassDiagnosticsForDiagnostics =>
                battleEcsCharacterInputPass.Diagnostics;
        public BattleEcsCharacterPostFrameTailPassMode
            BattleEcsCharacterPostFrameTailPassModeForDiagnostics =>
                battleEcsCharacterPostFrameTailPass.Mode;
        public BattleEcsCharacterPostFrameTailPassDiagnostics
            BattleEcsCharacterPostFrameTailPassDiagnosticsForDiagnostics =>
                battleEcsCharacterPostFrameTailPass.Diagnostics;
        public BattleHitExecutionPlanMode
            BattleHitExecutionPlanModeForDiagnostics =>
                battleEcsHitExecutionPlan.Mode;
        public BattleHitExecutionPlanDiagnostics
            BattleHitExecutionPlanDiagnosticsForDiagnostics =>
                battleEcsHitExecutionPlan.Diagnostics;
        public SimulationRuntimeCapacityModule RuntimeCapacity => runtimeCapacityModule;
        internal SimulationBattleBufferModule BattleBuffersForServices => battleBuffers;
        internal SimulationObjectBucketRegistry ObjectBucketRegistryForSnapshotRestore => objectBucketRegistry;
        internal RuntimeCharacterConfigResolver RuntimeCharacterConfigs => runtimeCharacterConfigs;
        internal BattleRuntimeDataCatalog RuntimeDataCatalog => runtimeDataCatalog;
        internal BattleLogicReferencePool LogicReferencePool => logicReferencePool;

        internal IReadOnlyList<ObjectDefinition>
            GetRandomWeaponLoadedObjectsForModule()
        {
            if (runtimeDataCatalog.IsReady)
                return runtimeDataCatalog.ObjectDefinitions;

            _ = CharacterAnimtorManager.Instance;
            return GameDataManager.Instance?.GetAllObjects();
        }

        internal bool HasRandomWeaponCharacterConfigSourceForModule()
        {
            return runtimeDataCatalog.IsReady ||
                   CharacterAnimtorManager.Instance != null;
        }

        internal LF2CharacterDataWrapper
            ResolveRandomWeaponCharacterConfigForModule(int objectId)
        {
            return runtimeDataCatalog.IsReady
                ? runtimeDataCatalog.GetCharacterConfig(objectId)
                : CharacterAnimtorManager.Instance?.GetCharacterConfig(objectId);
        }

        internal LF2CharacterData ResolveRandomWeaponCharacterDataForModule(
            int objectId)
        {
            return runtimeDataCatalog.IsReady
                ? runtimeDataCatalog.GetCharacterData(objectId)
                : CharacterAnimtorManager.Instance?.GetCharacterData(objectId);
        }

        internal bool IsRuntimeSlotClaimedForRandomWeaponModule(int runtimeSlot) => _runtimeSlots.IsClaimed(runtimeSlot);
        internal BattleLogicEntityFactory LogicEntityFactory => logicEntityFactory;
        internal BattleLogicObjectPointRuntime LogicObjectPointRuntime => logicObjectPointRuntime;
        internal bool UsesLogicOnlyEntityMaterialization => logicOnlyEntityMaterialization;

        internal void BeginBattlePreparation()
        {
            battleStructuralWriter.BeginBattlePreparation();
            logicObjectPointRuntime.BeginBattlePreparation();
        }

        internal void BeginBattleShutdown()
        {
            battleStructuralWriter.BeginBattleShutdown();
            logicObjectPointRuntime.BeginBattleShutdown();
        }

        internal int DiscardPendingObjectPointTasks() => logicObjectPointRuntime.DiscardPendingTasks();

        internal ILF2ObjectPointFactory ResolveObjectPointFactoryForSimulation()
        {
            // The branch order is intentional: a worker-owned logic world must
            // never evaluate the Unity singleton fallback.
            return logicOnlyEntityMaterialization
                ? logicObjectPointRuntime
                : LF2ObjectPointFactory.Instance;
        }

        internal void SetLogicOnlyEntityMaterialization(bool enabled)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Entity materialization mode cannot change during a battle tick.");
            }
            logicOnlyEntityMaterialization = enabled;
        }

        internal void BindLogicReferencePool(BattleLogicReferencePool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));
            if (ObjectCount != 0 || ClaimedRuntimeSlotCountForServices != 0)
            {
                throw new InvalidOperationException(
                    "The simulation logic pool must be bound before entities register.");
            }
            logicReferencePool = pool;
        }

        internal void PrepareRuntimeDataCatalogForBattle(
            IReadOnlyList<ObjectDefinition> definitions,
            Func<int, LF2CharacterDataWrapper> configResolver,
            BattleHitRecordLifecycleCatalog hitRecordLifecycleCatalog = default)
        {
            runtimeDataCatalog.Prepare(
                definitions,
                configResolver,
                hitRecordLifecycleCatalog);
            runtimeDataCatalog.Seal();
        }

        internal void UnsealRuntimeDataCatalog() => runtimeDataCatalog.Unseal();
        internal StageSpawnTaskConfigurator StageSpawnTaskConfigurator => stageSpawnTaskConfigurator;
        internal BattleCharacterInputActionResolver CharacterInputActionResolver => battleCharacterInputActionResolver;
        internal BattleIdentityWriter IdentityWriter => battleIdentityWriter;
        internal BattleAiUnifiedRowPublisher AiUnifiedRowPublisherForServices => battleAiUnifiedRowPublisher;
        internal SimulationWorldMutationTracker RuntimeMutationTrackerForServices => runtimeMutationTracker;
        internal BattleCharacterInputWriter CharacterInputWriter => battleCharacterInputWriter;

        internal BattleWorldCoreScalarSnapshot CaptureWorldCoreScalarSnapshot(
            Lockstep.LockstepSessionIdentity identity)
        {
            return battleWorldCoreScalarSnapshotModule.Capture(identity);
        }

        internal bool TryCaptureWorldRosterResultsSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldRosterResultsSnapshotBuffer destination)
        {
            return battleWorldRosterResultsSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredStageSpawnSnapshotEntryCapacity => battleWorldStageSpawnSnapshotModule.RequiredEntryCapacity;

        internal bool TryCaptureWorldStageSpawnSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldStageSpawnSnapshotBuffer destination)
        {
            return battleWorldStageSpawnSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredRuntimeSlotSnapshotCapacity => battleWorldRuntimeSlotSnapshotModule.SlotCapacity;

        internal bool TryCaptureWorldRuntimeSlotSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldRuntimeSlotSnapshotBuffer destination)
        {
            return battleWorldRuntimeSlotSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredEntityRuntimeSnapshotCapacity => battleWorldEntityRuntimeSnapshotModule.SlotCapacity;

        internal bool TryCaptureWorldEntityRuntimeSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldEntityRuntimeSnapshotBuffer destination)
        {
            return battleWorldEntityRuntimeSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredEntityBaseShellSnapshotCapacity => battleWorldEntityBaseShellSnapshotModule.SlotCapacity;

        internal bool TryCaptureWorldEntityBaseShellSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldEntityBaseShellSnapshotBuffer destination)
        {
            return battleWorldEntityBaseShellSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredLivingShellSnapshotCapacity => battleWorldLivingShellSnapshotModule.SlotCapacity;

        internal bool TryCaptureWorldLivingShellSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldLivingShellSnapshotBuffer destination)
        {
            return battleWorldLivingShellSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredCharacterShellSnapshotCapacity => battleWorldCharacterShellSnapshotModule.SlotCapacity;

        internal bool TryCaptureWorldCharacterShellSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldCharacterShellSnapshotBuffer destination)
        {
            return battleWorldCharacterShellSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredWeaponShellSnapshotCapacity => battleWorldWeaponShellSnapshotModule.SlotCapacity;

        internal bool TryCaptureWorldWeaponShellSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldWeaponShellSnapshotBuffer destination)
        {
            return battleWorldWeaponShellSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal int RequiredSpecialOtherShellSnapshotCapacity => battleWorldSpecialOtherShellSnapshotModule.SlotCapacity;

        internal bool TryCaptureWorldSpecialOtherShellSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldSpecialOtherShellSnapshotBuffer destination)
        {
            return battleWorldSpecialOtherShellSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal BattleWorldPendingEventSnapshotBuffer
            CreateWorldPendingEventSnapshotBufferForBootstrap()
        {
            return battleWorldPendingEventSnapshotModule.CreateBufferForBootstrap();
        }

        internal bool TryCaptureWorldPendingEventSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldPendingEventSnapshotBuffer destination)
        {
            return battleWorldPendingEventSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal BattleWorldRestSnapshotBuffer
            CreateWorldRestSnapshotBufferForBootstrap()
        {
            return battleWorldRestSnapshotModule.CreateBufferForBootstrap();
        }

        internal bool TryCaptureWorldRestSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleWorldRestSnapshotBuffer destination)
        {
            return battleWorldRestSnapshotModule.TryCapture(
                identity,
                tick,
                destination);
        }

        internal BattleStateSnapshotBuffer CreateBattleStateSnapshotBufferForBootstrap()
        {
            return new BattleStateSnapshotBuffer(
                new BattleWorldRosterResultsSnapshotBuffer(),
                new BattleWorldStageSpawnSnapshotBuffer(
                    RequiredStageSpawnSnapshotEntryCapacity),
                new BattleWorldRuntimeSlotSnapshotBuffer(
                    RequiredRuntimeSlotSnapshotCapacity),
                new BattleWorldEntityRuntimeSnapshotBuffer(
                    RequiredEntityRuntimeSnapshotCapacity),
                new BattleWorldEntityBaseShellSnapshotBuffer(
                    RequiredEntityBaseShellSnapshotCapacity),
                new BattleWorldLivingShellSnapshotBuffer(
                    RequiredLivingShellSnapshotCapacity),
                new BattleWorldCharacterShellSnapshotBuffer(
                    RequiredCharacterShellSnapshotCapacity),
                new BattleWorldWeaponShellSnapshotBuffer(
                    RequiredWeaponShellSnapshotCapacity),
                new BattleWorldSpecialOtherShellSnapshotBuffer(
                    RequiredSpecialOtherShellSnapshotCapacity),
                CreateWorldPendingEventSnapshotBufferForBootstrap(),
                CreateWorldRestSnapshotBufferForBootstrap());
        }

        internal bool TryCaptureBattleStateSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            int tick,
            BattleStateSnapshotBuffer destination)
        {
            if (identity == null)
                throw new ArgumentNullException(nameof(identity));
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            destination.Invalidate();
            BattleWorldCoreScalarSnapshot core =
                CaptureWorldCoreScalarSnapshot(identity);
            if (!TryCaptureWorldRosterResultsSnapshot(
                    identity,
                    tick,
                    destination.RosterResults) ||
                !TryCaptureWorldStageSpawnSnapshot(
                    identity,
                    tick,
                    destination.StageSpawn) ||
                !TryCaptureWorldRuntimeSlotSnapshot(
                    identity,
                    tick,
                    destination.RuntimeSlots) ||
                !TryCaptureWorldEntityRuntimeSnapshot(
                    identity,
                    tick,
                    destination.EntityRuntime) ||
                !TryCaptureWorldEntityBaseShellSnapshot(
                    identity,
                    tick,
                    destination.EntityBaseShell) ||
                !TryCaptureWorldLivingShellSnapshot(
                    identity,
                    tick,
                    destination.LivingShell) ||
                !TryCaptureWorldCharacterShellSnapshot(
                    identity,
                    tick,
                    destination.CharacterShell) ||
                !TryCaptureWorldWeaponShellSnapshot(
                    identity,
                    tick,
                    destination.WeaponShell) ||
                !TryCaptureWorldSpecialOtherShellSnapshot(
                    identity,
                    tick,
                    destination.SpecialOtherShell) ||
                !TryCaptureWorldPendingEventSnapshot(
                    identity,
                    tick,
                    destination.PendingEvents) ||
                !TryCaptureWorldRestSnapshot(
                    identity,
                    tick,
                    destination.Rest))
            {
                return false;
            }

            return destination.TryPublish(core, identity, tick);
        }

        internal bool TryRestoreBattleStateSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            BattleStateSnapshotBuffer snapshot,
            out BattleStateSnapshotRestoreFailure failure)
        {
            return battleStateSnapshotRestoreModule.TryRestoreBattleStateSnapshot(
                identity,
                snapshot,
                out failure);
        }

        internal void RestoreSnapshotOwnerScalars(
            int releaseCameraX,
            int releaseCameraVelocity,
            int nextAutoStableId)
        {
            _cameraX = releaseCameraX;
            _cameraVel = releaseCameraVelocity;
            _nextAutoStableId = nextAutoStableId;
        }

        internal bool RebuildDerivedStateAfterSnapshotRestore()
        {
            (SceneQuery as BruteForceSceneQuery)?.ResetFormalSpatialBroadphase();
            battleAiUnifiedRowPublisher.EndPass();
            battleIdentityWriter.Reset();
            battleCharacterInputWriter.Reset();
            battleFrameMotionWriter.Reset();
            battleRelationLinkWriter.Reset();
            battleVitalWriter.Reset();

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                    continue;

                LF2Entity entity = view.Entity;
                RuntimeEntityHandle handle =
                    new RuntimeEntityHandle(runtimeSlot, view.Generation);
                entity.Runtime.BindWorldMutationTracker(runtimeMutationTracker);
                battleCharacterInputWriter.Bind(entity.Runtime, handle);
                battleIdentityWriter.Bind(entity, handle);
                battleFrameMotionWriter.Bind(entity, handle);
                battleRelationLinkWriter.Bind(entity.Runtime, handle);
                battleVitalWriter.Bind(entity.Runtime, handle);
                BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                    entity.Renderer,
                    handle);
            }

            battleEcsShadowModule.Reset();
            battleEcsCooldownPass.Reset();
            battleEcsCharacterStageZPass.Reset();
            battleEcsCharacterPreFrameBoundsPass.Reset();
            battleEcsFramePostProcessPass.Reset();
            battleEcsPositiveLinkValidationPass.Reset();
            battleEcsCharacterFrameAdvancePass.Reset();
            battleEcsCharacterRecoveryPass.Reset();
            battleEcsCharacterFrameTickPass.Reset();
            battleEcsCharacterInputPass.Reset();
            battleEcsCharacterPostFrameTailPass.Reset();
            battleEcsHitExecutionPlan.Reset();
            ResetAiAirSpatialIndex();
            InvalidateAiAirRoleSnapshot();
            ResetAiMoveModeFirst10Snapshot();
            ResetAiUnifiedMoveModeFirst10Snapshot();
            InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
            InvalidateAiUnifiedSnapshotShadowPass();
            pendingDestroyScanCacheValid = false;
            BattlePresentation.Reset();
            return true;
        }

        internal BattleFrameMotionWriter FrameMotionWriter => battleFrameMotionWriter;
        internal BattleRelationLinkWriter RelationLinkWriter => battleRelationLinkWriter;
        public int PositiveLinkIndexCountForDiagnostics => battleRelationLinkWriter.PositiveLinkCount;
        internal BattleVitalWriter VitalWriter => battleVitalWriter;
        internal BattleCharacterActionWriter CharacterActionWriter => battleCharacterActionWriter;
        internal BattleAiInputWriter AiInputWriter => battleAiInputWriter;
        internal BattleBoundaryWriter BoundaryWriter => battleBoundaryWriter;
        internal BattleInteractionWriter InteractionWriter => battleInteractionWriter;
        internal BattleHeldObjectWriter HeldObjectWriter => battleHeldObjectWriter;
        internal BattleCpointWriter CpointWriter => battleCpointWriter;
        internal BattleDamageWriter DamageWriter => battleDamageWriter;
        internal BattleStructuralWriter StructuralWriter => battleStructuralWriter;
        internal BattleEcsHitExecutionPlan HitExecutionPlanForInteractionModule => battleEcsHitExecutionPlan;
        internal BattleResultsWriter ResultsWriter => battleResultsWriter;
        internal CharacterMechanics CharacterMechanicsForServices => characterMechanics;
        public BattleStructuralWriterDiagnostics StructuralWriterDiagnosticsForDiagnostics => battleStructuralWriter.Diagnostics;

        public bool TryGetFrameMotionStateForDiagnostics(
            LF2Entity entity,
            out BattleFrameMotionStateView view)
        {
            return battleFrameMotionWriter.TryGetState(entity?.Runtime, out view);
        }

        public bool TryGetRelationLinkStateForDiagnostics(
            LF2Entity entity,
            out BattleRelationLinkStateView view)
        {
            return battleRelationLinkWriter.TryGetState(entity?.Runtime, out view);
        }

        public bool TryGetVitalStateForDiagnostics(
            LF2Entity entity,
            out BattleVitalStateView view)
        {
            return battleVitalWriter.TryGetState(entity?.Runtime, out view);
        }

        public void ConfigureBattleEcsShadowForDiagnostics(BattleEcsShadowMode mode)
        {
            if (_ticking)
                throw new System.InvalidOperationException(
                    "The ECS migration shadow cannot be reconfigured during a battle tick.");

            battleEcsShadowModule.SetMode(mode);
        }

        public void CaptureBattleEcsShadowForDiagnostics(int tickIndex)
        {
            if (_ticking)
                throw new System.InvalidOperationException(
                    "The ECS migration shadow cannot be captured during a battle tick.");

            battleEcsShadowModule.Capture(tickIndex);
        }

        public bool ValidateBattleEcsShadowForDiagnostics()
        {
            if (_ticking)
                throw new System.InvalidOperationException(
                    "The ECS migration shadow cannot be validated during a battle tick.");

            return battleEcsShadowModule.Validate();
        }

        public bool TryGetBattleEcsShadowEntityForDiagnostics(
            int slot,
            out BattleEcsShadowEntityView view)
        {
            return battleEcsShadowModule.TryGetEntityView(slot, out view);
        }

        public int FindNextBattleEcsActiveSlotForDiagnostics(int startSlot) => battleEcsShadowModule.FindNextActiveSlot(startSlot);

        internal void RefreshBattleEcsShadowAfterTick(int tickIndex) => battleEcsShadowModule.CaptureAndCompareNoThrow(tickIndex);

        public void ConfigureBattleEcsCooldownPassForDiagnostics(
            BattleEcsCooldownPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The cooldown canonical writer can only change at a reset boundary.");
            }

            battleEcsCooldownPass.SetMode(mode);
        }

        internal void RunBattleEcsCooldownPass(int tickIndex) => battleEcsCooldownPass.Execute(tickIndex);

        public void ConfigureBattleEcsCharacterStageZPassForDiagnostics(
            BattleEcsCharacterStageZPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The character stage-Z canonical writer can only change at a reset boundary.");
            }

            battleEcsCharacterStageZPass.SetMode(mode);
        }

        public void ConfigureBattleEcsCharacterPreFrameBoundsPassForDiagnostics(
            BattleEcsCharacterPreFrameBoundsPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The character PreFrame bounds writer can only change at a reset boundary.");
            }

            battleEcsCharacterPreFrameBoundsPass.SetMode(mode);
        }

        internal void RestoreBattleEcsCharacterPreFrameBoundsPassForDiagnostics(
            BattleEcsCharacterPreFrameBoundsPassMode mode)
        {
            if (_ticking || ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new System.InvalidOperationException(
                    "The character PreFrame bounds writer can only be restored after all runtime slots are released.");
            }

            battleEcsCharacterPreFrameBoundsPass.SetMode(mode);
        }

        internal void RunBattleEcsCharacterPreFrameBoundsPass() => battleEcsCharacterPreFrameBoundsPass.Execute();

        internal void RunLegacyPreFrameBoundsAll() => stageRenderModule.RunLegacyPreFrameBoundsAll();

        public void ConfigureBattleEcsFramePostProcessPassForDiagnostics(
            BattleEcsFramePostProcessPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The frame-postprocess canonical writer can only change at a reset boundary.");
            }

            battleEcsFramePostProcessPass.SetMode(mode);
        }

        internal void RunBattleEcsFramePostProcessPass() => battleEcsFramePostProcessPass.Execute();

        public void ConfigureBattleEcsPositiveLinkValidationPassForDiagnostics(
            BattleEcsPositiveLinkValidationPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The positive-link canonical writer can only change at a reset boundary.");
            }

            battleEcsPositiveLinkValidationPass.SetMode(mode);
        }

        public void ConfigureBattleEcsCharacterFrameAdvancePassForDiagnostics(
            BattleEcsCharacterFrameAdvancePassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The character FrameAdvance pass can only change at a reset boundary.");
            }

            battleEcsCharacterFrameAdvancePass.SetMode(mode);
        }

        public void ConfigureBattleEcsCharacterRecoveryPassForDiagnostics(
            BattleEcsCharacterRecoveryPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The character recovery pass can only change at a reset boundary.");
            }

            battleEcsCharacterRecoveryPass.SetMode(mode);
        }

        public void ConfigureBattleEcsCharacterFrameTickPassForDiagnostics(
            BattleEcsCharacterFrameTickPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The character FrameTick pass can only change at a reset boundary.");
            }

            battleEcsCharacterFrameTickPass.SetMode(mode);
        }

        public void ConfigureBattleEcsCharacterInputPassForDiagnostics(
            BattleEcsCharacterInputPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The character input pass can only change at a reset boundary.");
            }

            battleEcsCharacterInputPass.SetMode(mode);
        }

        internal void RestoreBattleEcsCharacterInputPassForDiagnostics(
            BattleEcsCharacterInputPassMode mode)
        {
            if (_ticking || ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new System.InvalidOperationException(
                    "The character input pass can only be restored after all runtime slots are released.");
            }

            battleEcsCharacterInputPass.SetMode(mode);
        }

        internal void RestoreBattleEcsCharacterFrameTickPassForDiagnostics(
            BattleEcsCharacterFrameTickPassMode mode)
        {
            if (_ticking || ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new System.InvalidOperationException(
                    "The character FrameTick pass can only be restored after all runtime slots are released.");
            }

            battleEcsCharacterFrameTickPass.SetMode(mode);
        }

        public void ConfigureBattleEcsCharacterPostFrameTailPassForDiagnostics(
            BattleEcsCharacterPostFrameTailPassMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The character post-frame tail pass can only change at a reset boundary.");
            }

            battleEcsCharacterPostFrameTailPass.SetMode(mode);
        }

        internal void RestoreBattleEcsCharacterPostFrameTailPassForDiagnostics(
            BattleEcsCharacterPostFrameTailPassMode mode)
        {
            if (_ticking || ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new System.InvalidOperationException(
                    "The character post-frame tail pass can only be restored after all runtime slots are released.");
            }

            battleEcsCharacterPostFrameTailPass.SetMode(mode);
        }

        internal void RestoreBattleEcsPositiveLinkValidationPassForDiagnostics(
            BattleEcsPositiveLinkValidationPassMode mode)
        {
            if (_ticking || ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new System.InvalidOperationException(
                    "The positive-link canonical writer can only be restored after all runtime slots are released.");
            }

            battleEcsPositiveLinkValidationPass.SetMode(mode);
        }

        public void ConfigureBattleHitExecutionPlanForDiagnostics(
            BattleHitExecutionPlanMode mode)
        {
            if (_ticking || CurrentTickIndex != 0)
            {
                throw new System.InvalidOperationException(
                    "The hit execution-plan shadow can only change at a reset boundary.");
            }

            battleEcsHitExecutionPlan.SetMode(mode);
        }

        internal void RestoreBattleHitExecutionPlanForDiagnostics(
            BattleHitExecutionPlanMode mode)
        {
            if (_ticking || ClaimedRuntimeSlotCountForDiagnostics != 0)
            {
                throw new System.InvalidOperationException(
                    "The hit execution plan can only be restored after all runtime slots are released.");
            }

            battleEcsHitExecutionPlan.SetMode(mode);
        }

        public bool TryGetBattleHitExecutionPlanEntryForDiagnostics(
            int index,
            out BattleHitExecutionPlanEntryView entry)
        {
            return battleEcsHitExecutionPlan.TryGetEntry(index, out entry);
        }

        internal void CaptureBattleHitExecutionPlanPass(
            int tickIndex,
            BattleHitExecutionPass pass,
            bool skipProvenEmptyBaseCharacters = false,
            bool passProvenEmpty = false)
        {
            battleEcsHitExecutionPlan.CapturePass(
                tickIndex,
                pass,
                skipProvenEmptyBaseCharacters,
                passProvenEmpty);
        }

        internal bool BeginBattleHitExecutionPlanLegacyObservation(
            int tickIndex,
            BattleHitExecutionPass pass)
        {
            return battleEcsHitExecutionPlan.BeginLegacyObservationPass(
                tickIndex,
                pass);
        }

        internal bool ShouldObserveBattleHitExecutionPlanLegacyCandidateRead => battleEcsHitExecutionPlan.ShouldObserveLegacyCandidateRead;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyPreprocess => battleEcsHitExecutionPlan.ShouldObserveLegacyPreprocess;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects => battleEcsHitExecutionPlan.ShouldObserveLegacyConsumeEffects;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyDisposition => battleEcsHitExecutionPlan.ShouldObserveLegacyDisposition;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyDispatch => battleEcsHitExecutionPlan.ShouldObserveLegacyDispatch;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyWriterEffect => battleEcsHitExecutionPlan.ShouldObserveLegacyWriterEffect;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyLifecycleEffect => battleEcsHitExecutionPlan.ShouldObserveLegacyLifecycleEffect;

        internal bool CanProjectBattleHitExecutionPlanLegacyWriterEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            return battleEcsHitExecutionPlan.CanProjectLegacyWriterEffect(
                attacker,
                target,
                resolvedItr,
                disposition);
        }

        internal bool CanProjectBattleHitExecutionPlanLegacyLifecycleEffect(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            return battleEcsHitExecutionPlan.CanProjectLegacyLifecycleEffect(
                attacker,
                target,
                resolvedItr,
                disposition);
        }

        internal void ObserveBattleHitExecutionPlanLegacyCandidateRead(
            RuntimeEntityHandle attackerHandle,
            int candidateOrdinal,
            in SceneQueryHit hit)
        {
            battleEcsHitExecutionPlan.ObserveLegacyCandidateRead(
                attackerHandle,
                candidateOrdinal,
                hit);
        }

        internal void EndBattleHitExecutionPlanLegacyObservation() => battleEcsHitExecutionPlan.EndLegacyObservationPass();

        internal void ObserveBattleHitExecutionPlanLegacyPreprocess(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            bool zeroAttackerHpOnConsume,
            bool releaseHeavyHeldTargetOnConsume)
        {
            battleEcsHitExecutionPlan.ObserveLegacyPreprocess(
                attacker,
                target,
                resolvedItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume);
        }

        internal void ObserveBattleHitExecutionPlanLegacyDisposition(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            battleEcsHitExecutionPlan.ObserveLegacyDisposition(
                attacker,
                target,
                resolvedItr,
                disposition);
        }

        internal void PrepareBattleHitExecutionPlanLegacyConsumeEffectsObservation(
            LF2Entity attacker,
            LF2Entity target)
        {
            battleEcsHitExecutionPlan.PrepareLegacyConsumeEffectsObservation(
                attacker,
                target);
        }

        internal void ObserveBattleHitExecutionPlanLegacyConsumeEffects(
            LF2Entity attacker,
            LF2Entity target)
        {
            battleEcsHitExecutionPlan.ObserveLegacyConsumeEffects(
                attacker,
                target);
        }

        internal void PrepareBattleHitExecutionPlanLegacyDispatchObservation(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr)
        {
            battleEcsHitExecutionPlan.PrepareLegacyDispatchObservation(
                attacker,
                target,
                resolvedItr);
        }

        internal void ObserveBattleHitExecutionPlanLegacyDispatch(
            LF2Entity attacker,
            bool dispatchSucceeded,
            bool terminatedRemainingCandidates)
        {
            battleEcsHitExecutionPlan.ObserveLegacyDispatch(
                attacker,
                dispatchSucceeded,
                terminatedRemainingCandidates);
        }

        internal void PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            battleEcsHitExecutionPlan.PrepareLegacyWriterEffectObservation(
                attacker,
                target,
                resolvedItr,
                disposition);
        }

        internal void ObserveBattleHitExecutionPlanLegacyWriterEffect(
            LF2Entity attacker,
            LF2Entity target)
        {
            battleEcsHitExecutionPlan.ObserveLegacyWriterEffect(attacker, target);
        }

        internal void PrepareBattleHitExecutionPlanLegacyLifecycleEffectObservation(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea resolvedItr,
            BattleHitCandidateDisposition disposition)
        {
            battleEcsHitExecutionPlan.PrepareLegacyLifecycleEffectObservation(
                attacker,
                target,
                resolvedItr,
                disposition);
        }

        internal void ObserveBattleHitExecutionPlanLegacyLifecycleEffect(
            LF2Entity attacker)
        {
            battleEcsHitExecutionPlan.ObserveLegacyLifecycleEffect(attacker);
        }

        internal const int PresentationShadowSubOrder = 0;
        internal const int PresentationEntitySubOrder = 1;
        internal const int PresentationReservedOverlaySubOrder = 2;
        internal const int PresentationHitRecordSubOrder = 3;
        private const int PresentationSubOrderCount = 4;

        internal const int LegacySpriteRendererMaxPresentationEntities =
            (short.MaxValue + 1) / PresentationSubOrderCount;

        public BattlePresentationCoordinator BattlePresentation => stageRenderModule.BattlePresentation;

        public BattlePixelFramePlan CurrentPixelFramePlan => stageRenderModule.CurrentPixelFramePlan;

        public int LateRendererUpdateInvocationCountForDiagnostics => stageRenderModule.LateRendererUpdateInvocationCountForDiagnostics;

        public long CentralOnlyRendererShellBypassCountForDiagnostics => stageRenderModule.CentralOnlyRendererShellBypassCountForDiagnostics;

        public int PresentationRenderOrderBuildCountForDiagnostics => stageRenderModule.PresentationRenderOrderBuildCountForDiagnostics;

        public int PresentationRenderOrderReusePublishCountForDiagnostics => stageRenderModule.PresentationRenderOrderReusePublishCountForDiagnostics;

        public int PresentationEntityScanAndSortCountForDiagnostics => stageRenderModule.PresentationEntityScanAndSortCountForDiagnostics;

        public bool SkipLateRendererUpdateForDiagnostics => stageRenderModule.SkipLateRendererUpdateForDiagnostics;

        public long SkippedLateRendererUpdateTickCountForDiagnostics => stageRenderModule.SkippedLateRendererUpdateTickCountForDiagnostics;

        public bool ConfigureSkipLateRendererUpdateForDiagnostics(
            bool requested,
            bool simulationOnly)
        {
            return stageRenderModule.ConfigureSkipLateRendererUpdateForDiagnostics(
                requested,
                simulationOnly);
        }

        public void RestoreSkipLateRendererUpdateForDiagnostics(bool previous) => stageRenderModule.RestoreSkipLateRendererUpdateForDiagnostics(previous);

        internal void PublishPixelFramePlan(BattlePixelFramePlan plan) => stageRenderModule.PublishPixelFramePlan(plan);

        public void SetBattlePresentationBackend(BattlePresentationBackendMode mode) => stageRenderModule.SetBattlePresentationBackend(mode);

        public void SetExplicitStageRuntimeSnapshotForTesting(
            int stageWidth,
            int zMin,
            int zMax,
            int perspectiveNear,
            int perspectiveFar)
        {
            stageRenderModule.SetExplicitStageRuntimeSnapshotForTesting(
                stageWidth,
                zMin,
                zMax,
                perspectiveNear,
                perspectiveFar);
        }

        public bool IsGroundPointWalkable(Vector2 pointXY) => stageRenderModule.IsGroundPointWalkable(pointXY);

        public void RefreshStageRuntimeSnapshotFromScene() => stageRenderModule.RefreshStageRuntimeSnapshotFromScene();

        public void PrepareStageRuntimeSnapshotForTick(int tickIndex) => stageRenderModule.PrepareStageRuntimeSnapshotForTick(tickIndex);

        public bool ConfigureLegacyPerPassStageRefreshForDiagnostics(bool requested)
        {
            return stageRenderModule.ConfigureLegacyPerPassStageRefreshForDiagnostics(
                requested);
        }

        public bool ForceLegacyPerPassStageRefreshForDiagnostics => stageRenderModule.ForceLegacyPerPassStageRefreshForDiagnostics;
        public long StageRuntimeSceneRefreshCountForDiagnostics => stageRenderModule.StageRuntimeSceneRefreshCountForDiagnostics;
        public long StageRuntimeHostPrepareCountForDiagnostics => stageRenderModule.StageRuntimeHostPrepareCountForDiagnostics;
        public long StageRuntimeHostReuseCountForDiagnostics => stageRenderModule.StageRuntimeHostReuseCountForDiagnostics;
        public long StageRuntimeLegacyPerPassRefreshCountForDiagnostics => stageRenderModule.StageRuntimeLegacyPerPassRefreshCountForDiagnostics;

        private static void ResolveUnityStageRuntime(
            out int stageWidth,
            out int zMin,
            out int zMax,
            out int perspectiveNear,
            out int perspectiveFar)
        {
            SimulationStageRenderModule.ResolveUnityStageRuntime(
                out stageWidth,
                out zMin,
                out zMax,
                out perspectiveNear,
                out perspectiveFar);
        }

        public void ClampCharacterZToStageBoundsAll()
        {
            stageRenderModule.PrepareStageRuntimeForKernelPass();
            battleEcsCharacterStageZPass.Execute();
        }

        internal void RunLegacyCharacterZStageBounds() => stageRenderModule.ClampCharacterZToStageBoundsAll();

        public void ApplyPreFrameBoundsAll()
        {
            stageRenderModule.PrepareStageRuntimeForKernelPass();
            stageRenderModule.ApplyPreFrameBoundsAll();
        }

        public void RenderDispatchAll(int tickIndex) => stageRenderModule.RenderDispatchAll(tickIndex);

        public void RenderDispatchAll(int tickIndex, bool buildPresentation) => stageRenderModule.RenderDispatchAll(tickIndex, buildPresentation);

        internal void CaptureSimulationWorkerPresentationFrame(int tickIndex) => stageRenderModule.CaptureSimulationWorkerPresentationFrame(tickIndex);

        internal void PresentLatestFrame(int tickIndex) => stageRenderModule.PresentLatestFrame(tickIndex);

        internal static bool RequiresLegacySpriteRendererCapacityGuard(
            BattlePixelFramePlan plan)
        {
            return SimulationStageRenderModule.RequiresLegacySpriteRendererCapacityGuard(plan);
        }

        internal void GetPresentationEntitiesNoAlloc(List<LF2Entity> destination) => stageRenderModule.GetPresentationEntitiesNoAlloc(destination);

        internal void RecordLegacyShadowProbe(LF2Entity entity, SpriteRenderer renderer) => stageRenderModule.RecordLegacyShadowProbe(entity, renderer);

        internal void RecordLegacyEntityProbe(LF2Entity entity, SpriteRenderer renderer) => stageRenderModule.RecordLegacyEntityProbe(entity, renderer);

        internal void RecordLegacyHitRecordProbe(
            LF2Entity entity,
            SpriteRenderer renderer,
            int hitRecordIndex)
        {
            stageRenderModule.RecordLegacyHitRecordProbe(
                entity,
                renderer,
                hitRecordIndex);
        }

        internal void BuildPresentationRenderOrder() => stageRenderModule.BuildPresentationRenderOrder();

        internal void PublishPresentationRenderOrderFromSortedEntities(
            IReadOnlyList<LF2Entity> sortedEntities,
            bool reusesCoordinatorSort = false)
        {
            stageRenderModule.PublishPresentationRenderOrderFromSortedEntities(
                sortedEntities,
                reusesCoordinatorSort);
        }

        internal void PublishPresentationRenderOrderFromFrame(
            BattlePresentationFrame frame,
            bool reusesCoordinatorSort = false)
        {
            stageRenderModule.PublishPresentationRenderOrderFromFrame(
                frame,
                reusesCoordinatorSort);
        }

        internal void RecordPresentationEntityScanAndSortForDiagnostics() => stageRenderModule.RecordPresentationEntityScanAndSortForDiagnostics();

        internal static void ValidateLegacySpriteRendererPresentationCapacity(
            int materializedEntityCount)
        {
            SimulationStageRenderModule.ValidateLegacySpriteRendererPresentationCapacity(
                materializedEntityCount);
        }

        internal int GetPresentationRenderSortingOrder(LF2Entity entity, int subOrder) => stageRenderModule.GetPresentationRenderSortingOrder(entity, subOrder);

        internal void ResetUnityFixedWorldRenderOffsets() => stageRenderModule.ResetUnityFixedWorldRenderOffsets();

        public void UpdateBattleResultsFlow() => battleResultsOutcomeHostWriter.UpdateSummaryActivation();

        internal bool TrySpawnBattleResultsReserveBeforeResults(
            BattleResultsRuntimeState results,
            int side)
        {
            return battleResultsReserveHostWriter.TrySpawnBeforeResults(results, side);
        }

        internal int GetBattleResultsReserveLiveCount(int side, int col) => battleResultsReserveHostWriter.GetLiveCount(side, col);

        internal int GetBattleResultsReserveMissingCount(int side, int col) => battleResultsReserveHostWriter.GetMissingCount(side, col);

        internal void RunActiveBattleResultsTick(FrameInputSet frameInput) => battleResultsWriter.RunActiveTick(frameInput);

        internal void ResetUnityFixedWorldCameraStateForModule()
        {
            _cameraX = 0;
            _cameraVel = 0;
        }

        internal void GetNonEntityRendererObjectsForModule(
            List<ISimObject> destination)
        {
            registryModule.GetNonEntityRendererObjects(destination);
        }

        public void CurrentWaveStageTickAll() => stageWaveModule.CurrentWaveStageTickAll();

        public bool ConfigureStageCampaigns(
            List<BattleStageCampaignData> campaigns,
            int stageSeriesIdx,
            int initialWaveIdx)
        {
            return stageWaveModule.ConfigureStageCampaigns(
                campaigns,
                stageSeriesIdx,
                initialWaveIdx);
        }

        public bool ConfigureStageCampaignValues(
            BattleStageCampaignSet campaigns,
            int stageSeriesIdx,
            int initialWaveIdx)
        {
            return stageWaveModule.ConfigureStageCampaignValues(
                campaigns,
                stageSeriesIdx,
                initialWaveIdx);
        }

        public bool StartInitialStageWave() => stageWaveModule.StartInitialStageWave();

        // Keep the diagnostic reflection surface on the main class while the
        // implementation and state ownership live in the stage-wave module.
        private int StageSpawnEntryFactor() => stageWaveModule.StageSpawnEntryFactor();

        private int SpawnStageImmediateEntrySlot(BattleStageSpawnData spawn)
        {
            return spawn == null
                ? -1
                : stageWaveModule.SpawnStageImmediateEntrySlot(spawn.ToValue());
        }

        internal int FindFirstFreeRuntimeSlotForModule(
            int startSlot,
            int endSlotExclusive)
        {
            return FindFirstFreeRuntimeSlot(startSlot, endSlotExclusive);
        }

        internal bool TrySpawnResultsReserveEntry(
            int oid,
            int side,
            int hp,
            int requiredRuntimeSlot)
        {
            return stageWaveModule.TrySpawnResultsReserveEntry(
                oid,
                side,
                hp,
                requiredRuntimeSlot);
        }

        internal static bool UsesStageCharacterInitSemantics(int dataObjectType) => SimulationStageWaveModule.UsesStageCharacterInitSemantics(dataObjectType);

        internal static void ApplyStageSpawnRuntimeContract(LF2Entity entity, int hp) => SimulationStageWaveModule.ApplyStageSpawnRuntimeContract(entity, hp);

        public BattleTickPhaseDiagnostics ActiveBattleTickPhaseDiagnosticsForDiagnostics => diagnosticsModule.ActiveBattleTickPhase;

        public BattleTickPhaseDiagnostics EnableBattleTickPhaseDiagnosticsForDiagnostics() => diagnosticsModule.EnableBattleTickPhase();

        public void DisableBattleTickPhaseDiagnosticsForDiagnostics() => diagnosticsModule.DisableBattleTickPhase();

        public bool BattleTickDetailPhaseDiagnosticsAllocatedForDiagnostics => diagnosticsModule.BattleTickDetailAllocated;

        public BattleTickDetailPhaseDiagnostics ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics => diagnosticsModule.ActiveBattleTickDetailPhase;

        public BattleTickDetailPhaseDiagnostics EnableBattleTickDetailPhaseDiagnosticsForDiagnostics() => diagnosticsModule.EnableBattleTickDetailPhase();

        public void DisableBattleTickDetailPhaseDiagnosticsForDiagnostics() => diagnosticsModule.DisableBattleTickDetailPhase();

        public bool BattleAiInputDetailDiagnosticsAllocatedForDiagnostics => diagnosticsModule.BattleAiInputDetailAllocated;

        public BattleAiInputDetailDiagnostics ActiveBattleAiInputDetailDiagnosticsForDiagnostics => diagnosticsModule.ActiveBattleAiInputDetail;

        public BattleAiInputDetailDiagnostics EnableBattleAiInputDetailDiagnosticsForDiagnostics() => diagnosticsModule.EnableBattleAiInputDetail();

        public void DisableBattleAiInputDetailDiagnosticsForDiagnostics() => diagnosticsModule.DisableBattleAiInputDetail();

        public bool BattlePresentationPhaseDiagnosticsAllocatedForDiagnostics => diagnosticsModule.BattlePresentationPhaseAllocated;

        public BattlePresentationPhaseDiagnostics
            ActiveBattlePresentationPhaseDiagnosticsForDiagnostics =>
                diagnosticsModule.ActiveBattlePresentationPhase;

        public BattlePresentationPhaseDiagnostics
            EnableBattlePresentationPhaseDiagnosticsForDiagnostics()
        {
            return diagnosticsModule.EnableBattlePresentationPhase();
        }

        public void DisableBattlePresentationPhaseDiagnosticsForDiagnostics() => diagnosticsModule.DisableBattlePresentationPhase();

        public ulong CaptureRuntimeChecksum64(int tickIndex, FrameInputSet frameInput) => lockstepChecksumModule.Capture(this, tickIndex, frameInput);

        public BattleParityFrameSnapshot CaptureParityFrameSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null,
            bool includeFullDomains = false,
            IReadOnlyList<BattleParityStructuralEvent> structuralEvents = null)
        {
            return paritySnapshotModule.CaptureParityFrameSnapshot(
                tickIndex,
                frameInput,
                includeFullDomains,
                structuralEvents);
        }

        public BattleExtendedChecksumSnapshot CaptureExtendedChecksumSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null)
        {
            return paritySnapshotModule.CaptureExtendedChecksumSnapshot(
                tickIndex,
                frameInput);
        }

        public BattleLockstepChecksumSnapshot CaptureLockstepChecksumSnapshot(
            int tickIndex,
            FrameInputSet frameInput = null,
            IReadOnlyList<BattleParityStructuralEvent> structuralEvents = null)
        {
            return paritySnapshotModule.CaptureLockstepChecksumSnapshot(
                tickIndex,
                frameInput,
                structuralEvents);
        }

        internal static string NormalizeTraceAssetCue(string value) => BattleParitySnapshotModule.NormalizeTraceAssetCue(value);

        internal void SetRuntimeCharacterConfigResolverForSelfCheck(
            System.Func<int, NTSD.Animation.LF2CharacterDataWrapper> resolver)
        {
            runtimeCharacterConfigs.SetOverrideForSelfCheck(resolver);
        }

        internal void SetRespawnEffectSpawnOverrideForSelfCheck(
            System.Func<SimulationWorld, LF2Entity, LF2Entity> spawnOverride)
        {
            runtimeHooks.RespawnEffectSpawnOverride = spawnOverride;
        }

#if UNITY_INCLUDE_TESTS
        public void SetCharacterInputPassMutationOverrideForSelfCheck(
            System.Action<SimulationWorld, LF2Entity> mutationOverride)
        {
            runtimeHooks.CharacterInputPassMutationOverride = mutationOverride;
        }
#endif

        public void QueueSound(string soundId, int worldX)
        {
            if (string.IsNullOrWhiteSpace(soundId))
                return;

            if (battleBuffers.TryQueueSound(
                    new PendingSoundEvent(soundId, worldX, CurrentTickIndex)))
            {
                QueuedSoundEventCountForDiagnostics++;
            }
        }

        internal void BeginDataObjectTypeTickCache(int tickIndex) => ActiveDataObjectTypeCacheTick = tickIndex;

        internal void EndDataObjectTypeTickCache() => ActiveDataObjectTypeCacheTick = -1;

        public void ApplyFrameInputSet(FrameInputSet frameInput)
        {
            currentAppliedFrameInput = frameInput;
            frameInputModule.ApplyFrameInputSet(frameInput);
        }

        internal FrameInputSet CurrentAppliedFrameInputForResults => currentAppliedFrameInput;

        internal bool TryCaptureLocalFrameInput(
            int tickIndex,
            SimulationPlayerInput[] destination,
            out int playerCount)
        {
            return frameInputModule.TryCaptureLocalFrameInput(
                tickIndex,
                destination,
                out playerCount);
        }

        internal void DiscardDirectLocalInputTick(int tickIndex) => frameInputModule.DiscardDirectLocalInputTick(tickIndex);

        internal bool TryResolveRosterInputEntity(int playerSlot, out LF2Entity entity) => frameInputModule.TryResolveRosterInputEntity(playerSlot, out entity);

        internal bool TryResolveRosterEntity(
            int playerSlot,
            bool requireHuman,
            out LF2Entity entity)
        {
            return frameInputModule.TryResolveRosterEntity(
                playerSlot,
                requireHuman,
                out entity);
        }

        internal void RefreshActiveHumanRosterInputBindings() => frameInputModule.RefreshActiveHumanRosterInputBindings();

        internal bool IsBoundActiveHumanRosterInputEntity(LF2Entity entity) => frameInputModule.IsBoundActiveHumanRosterInputEntity(entity);

        internal bool ResetCooldownsForRuntimeSlot(
            int runtimeSlot,
            LF2Entity occupant)
        {
            return queryAndLinkModule.ResetCooldownsForRuntimeSlot(
                runtimeSlot,
                occupant);
        }

        internal bool TryResetAndBindStageSpawnCooldownsForRegistry(
            int runtimeSlot,
            LF2Entity occupant)
        {
            return queryAndLinkModule.TryResetAndBindStageSpawnCooldowns(
                runtimeSlot,
                occupant);
        }

        public void HeldObjectProcessAll(int tickIndex) => queryAndLinkModule.HeldObjectProcessAll(tickIndex);

        public void ValidateHeldLinksAll(int tickIndex) => battleEcsPositiveLinkValidationPass.Execute(tickIndex);

        internal void RunLegacyPositiveLinkValidation(int tickIndex) => queryAndLinkModule.RunLegacyPositiveLinkValidation(tickIndex);

        public LF2Entity FindEntityByRuntimeSlotForQuery(int runtimeSlot) => queryAndLinkModule.FindEntityByRuntimeSlotCurrent(runtimeSlot);

        public LF2Entity FindEntityByRuntimeSlotIncludingPending(int runtimeSlot)
        {
            return queryAndLinkModule.FindEntityByRuntimeSlotIncludingDormant(
                runtimeSlot);
        }

        internal LF2Entity FindEntityByRuntimeSlotIncludingDormant(int runtimeSlot)
        {
            return queryAndLinkModule.FindEntityByRuntimeSlotIncludingDormant(
                runtimeSlot);
        }

        private LF2Entity FindEntityByRuntimeSlotCurrent(int runtimeSlot) => queryAndLinkModule.FindEntityByRuntimeSlotCurrent(runtimeSlot);

        internal LF2Entity FindEntityByRuntimeSlotCurrentForLateModule(
            int runtimeSlot)
        {
            return FindEntityByRuntimeSlotCurrent(runtimeSlot);
        }

        public void GetAllLivingObjects(List<LF2LivingObject> destination) => queryAndLinkModule.GetAllLivingObjects(destination);

        public void GetAllEntities(List<LF2Entity> destination) => queryAndLinkModule.GetAllEntities(destination);

        private void GetActiveEntitiesByRuntimeSlot(List<LF2Entity> destination) => queryAndLinkModule.GetActiveEntitiesByRuntimeSlot(destination);

        internal void GetActiveEntitiesByRuntimeSlotForModule(
            List<LF2Entity> destination)
        {
            GetActiveEntitiesByRuntimeSlot(destination);
        }

        internal LF2Entity InvokeRespawnEffectSpawnOverrideForModule(
            LF2Entity entity)
        {
            return runtimeHooks.RespawnEffectSpawnOverride?.Invoke(this, entity);
        }

        private SimulationEntityTraversal.ActiveEntityEnumerable
            ActiveEntitiesByRuntimeSlot => entityTraversal.ActiveEntities;

        internal SimulationEntityTraversal.ActiveEntityEnumerable
            ActiveEntitiesByRuntimeSlotForModule => entityTraversal.ActiveEntities;

        private SimulationEntityTraversal.DeferredMutationScope
            BeginDeferredMutationEntityPass()
        {
            return entityTraversal.BeginDeferredMutation();
        }

        internal void BeginDeferredEntityMutationPass() => _ticking = true;

        internal void EndDeferredEntityMutationPass()
        {
            _ticking = false;
            FlushPendingUnregister();
            FlushPendingEntityDestroy();
        }

        /// <summary>
        /// Allocates capacity for the battle-only hot paths before the allocation gate
        /// is sealed. This is a migration seam: the caches still live in legacy
        /// historical source slices, while battle bootstrap owns the only production preparation
        /// boundary.
        /// </summary>
        internal void PrepareBattleHotPathCapacity(
            int maximumBodyCountPerEntity = 1,
            int maximumItrCountPerEntity = 1)
        {
            int entityCapacity = MaxRuntimeSlotsForServices;
            if (entityCapacity <= 0)
                return;

            objectBucketRegistry.PrepareCapacity(entityCapacity);

            Runtime?.EnsureStageSpawnBuffers().Prepare(
                Runtime.StageCampaigns,
                Runtime.StageSpawnRuntimeTargetTotal,
                Runtime.StageSpawnRuntimeEntryCount,
                Runtime.StageSpawnRuntimeSpawnedTotal,
                Runtime.StageSpawnRuntimeSlots);

            EnsureAiTeamHpSnapshotCapacity();
            EnsureListCapacity(aiInputSpatialEntries, entityCapacity);
            EnsureListCapacity(aiInputSpatialHandles, entityCapacity);
            EnsureListCapacity(aiInputSpatialSlots, entityCapacity);
            EnsureListCapacity(aiInputGroundSpatialEntries, entityCapacity);
            EnsureListCapacity(aiInputAirSpatialEntries, entityCapacity);
            EnsureListCapacity(aiSpecialScanSlots, entityCapacity);
            EnsureListCapacity(aiPhase1TargetSlots, entityCapacity);
            EnsureListCapacity(
                aiInputActiveGroundTeamPartitions,
                aiInputGroundTeamPartitionPool.Length);
            aiTeamHpSummaries.EnsureCapacity(entityCapacity);
            aiInputGroundTeamPartitions.EnsureCapacity(
                aiInputGroundTeamPartitionPool.Length);
            aiInputSpatialBroadphase.PrepareCapacity(entityCapacity);
            aiInputGroundSpatialBroadphase.PrepareCapacity(entityCapacity);
            aiInputAirSpatialBroadphase.PrepareCapacity(entityCapacity);
            PrepareAiGroundTeamPartitionCapacity(entityCapacity);

            passPipeline.PrepareCapacity(entityCapacity);

            int registeredCapacity = System.Math.Max(entityCapacity, ObjectCount);
            stageRenderModule.PrepareCapacity(entityCapacity, registeredCapacity);

            (SceneQuery as NTSD.Animation.BruteForceSceneQuery)?
                .PrepareBattleCapacity(
                    entityCapacity,
                    maximumBodyCountPerEntity,
                    maximumItrCountPerEntity);

            PrepareAiDecisionHotPathCapacity(entityCapacity);
        }

        internal void PrepareEnabledBattleDiagnosticsHotPath() => diagnosticsModule.PrepareEnabledProfilerMarkers();

        private void PrepareAiGroundTeamPartitionCapacity(int entityCapacity)
        {
            for (int index = 0;
                 index < aiInputGroundTeamPartitionPool.Length;
                 index++)
            {
                AiGroundTeamPartition partition =
                    aiInputGroundTeamPartitionPool[index];
                EnsureListCapacity(partition.Entries, entityCapacity);
                partition.Broadphase.PrepareCapacity(entityCapacity);
            }
        }

        private void PrepareAiDecisionHotPathCapacity(int capacity)
        {
            aiRuntime.Decision.PrepareCapacity(capacity);

            EnsureAiUnifiedSnapshotCapacity(capacity);
            PrepareAiUnifiedSnapshotLegacyConsumerBuffers(capacity);
            EnsureAiUnifiedSnapshotExecutionScratchCapacity(capacity);
            if (aiUnifiedSnapshotStandbyState == null ||
                aiUnifiedSnapshotStandbyState.Capacity != capacity ||
                object.ReferenceEquals(
                    aiUnifiedSnapshotStandbyState,
                    aiUnifiedSnapshotScratchState) ||
                object.ReferenceEquals(
                    aiUnifiedSnapshotStandbyState,
                    aiUnifiedSnapshotPublishedState))
            {
                aiUnifiedSnapshotStandbyState =
                    new AiUnifiedSnapshotExecutionState(capacity);
            }
        }

        private static void EnsureListCapacity<T>(List<T> values, int capacity)
        {
            if (values != null && values.Capacity < capacity)
                values.Capacity = capacity;
        }


        // Registry compatibility façade and root lifecycle orchestration.
        /// <summary>
        /// Compatibility lookup for diagnostics. Ordered traversal and bucket
        /// lifetime are owned by objectBucketRegistry.
        /// </summary>
        private Dictionary<int, SimulationObjectBucket> _buckets => registryModule.CompatibilityBuckets;
        /// <summary>给没有显式运行时 ID 的对象自动分配 StableId。</summary>
        private int _nextAutoStableId { get => registryModule.NextAutoStableId; set => registryModule.NextAutoStableId = value; }
        internal const int AuthorityRuntimeSlotCapacity =
            BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity;
        private const int DynamicRuntimeSlotStart = 50;
        private BattleRuntimeProfile activeRuntimeProfile => registryModule.RuntimeProfile;
        private RuntimeSlotTable _runtimeSlots => registryModule.RuntimeSlots;
        private RuntimeRestStore _runtimeRestStore => registryModule.RuntimeRestStore;
        private SimulationObjectBucketRegistry objectBucketRegistry => registryModule.ObjectBuckets;
        private int maxActiveRuntimeEntities => registryModule.MaxActiveRuntimeEntities;
        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private List<ISimObject> _pendingUnregister => battleBuffers.PendingUnregister;
        private List<LF2Entity> _pendingSlotReleasedDestroy => battleBuffers.PendingSlotReleasedDestroy;
        private Dictionary<ISimObject, int> structuralPendingUnregisterSlots => registryModule.StructuralPendingUnregisterSlots;
        private IBattleParityStructuralEventSink structuralEventSink => registryModule.StructuralEventSink;
        private int structuralEventCursorSlot { get => registryModule.StructuralEventCursorSlot; set => registryModule.StructuralEventCursorSlot = value; }
        private bool pendingDestroyScanCacheValid { get => registryModule.PendingDestroyScanCacheValid; set => registryModule.PendingDestroyScanCacheValid = value; }
        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking { get => registryModule.IsTicking; set => registryModule.IsTicking = value; }
        internal bool IsTickingForStructuralWriter => _ticking;
        internal bool IsTickingForModules => _ticking;
        private List<LF2Entity> _entityScratch => battleBuffers.EntityScratch;
        private int _cameraX { get => registryModule.CameraX; set => registryModule.CameraX = value; }
        private int _cameraVel { get => registryModule.CameraVelocity; set => registryModule.CameraVelocity = value; }

        public int ReleaseCameraX => _cameraX;
        internal int ReleaseCameraVelocityForServices => _cameraVel;
        internal int NextAutoStableIdForServices => _nextAutoStableId;
        internal bool IsUnityFixedWorldCameraStateClear => _cameraX == 0 && _cameraVel == 0;
        internal int RuntimeSlotCapacity => _runtimeSlots.LogicalCapacity;
        internal RuntimeSlotTable RuntimeSlotTableForModules => _runtimeSlots;
        internal int MaxRuntimeSlotsForServices => RuntimeSlotCapacity;
        internal int DynamicRuntimeSlotStartForServices => DynamicRuntimeSlotStart;
        internal BattleRuntimeProfile RuntimeProfileForServices => activeRuntimeProfile;
        internal CollisionBroadphaseBackend CollisionBroadphaseForServices { get; }
        internal int ClaimedRuntimeSlotCountForServices => _runtimeSlots.ClaimedCount;
        internal ulong RuntimeSlotOccupancyEpochForServices => _runtimeSlots.OccupancyEpoch;
        internal int PreInteractionRuntimeSlotLogicalCapacityForModule => _runtimeSlots.LogicalCapacity;
        internal int PreInteractionClaimedRuntimeSlotCountForModule => _runtimeSlots.ClaimedCount;
        internal ulong PreInteractionRuntimeSlotOccupancyEpochForModule => _runtimeSlots.OccupancyEpoch;
        internal long PreInteractionPendingDestroyEpochForModule => runtimeMutationTracker.PendingFlushDestroyEpoch;
        internal int PreInteractionPendingUnregisterCountForModule => _pendingUnregister.Count;
        internal int RuntimeSlotLogicalCapacityForEarlyFrameAdvance => _runtimeSlots.LogicalCapacity;
        internal ulong RuntimeSlotOccupancyEpochForEarlyFrameAdvance => _runtimeSlots.OccupancyEpoch;
        public BattleRuntimeProfile RuntimeProfileForDiagnostics => activeRuntimeProfile;
        public int RuntimeSlotCapacityForDiagnostics => _runtimeSlots.LogicalCapacity;
        public CollisionBroadphaseBackend CollisionBroadphaseForDiagnostics => CollisionBroadphaseForServices;
        public int ClaimedRuntimeSlotCountForDiagnostics => _runtimeSlots.ClaimedCount;
        public long PendingDestroyFullScanCount { get => registryModule.PendingDestroyFullScanCount; private set => registryModule.PendingDestroyFullScanCount = value; }
        public long PendingDestroySkipCount { get => registryModule.PendingDestroySkipCount; private set => registryModule.PendingDestroySkipCount = value; }
        public long PendingDestroyVisitedEntityCount { get => registryModule.PendingDestroyVisitedEntityCount; private set => registryModule.PendingDestroyVisitedEntityCount = value; }
        public long NullRegistrationRejectCountForDiagnostics { get => registryModule.NullRegistrationRejectCount; private set => registryModule.NullRegistrationRejectCount = value; }
        public long BucketCapacityRejectCountForDiagnostics { get => registryModule.BucketCapacityRejectCount; private set => registryModule.BucketCapacityRejectCount = value; }
        public long DuplicateRegistrationRejectCountForDiagnostics { get => registryModule.DuplicateRegistrationRejectCount; private set => registryModule.DuplicateRegistrationRejectCount = value; }
        public long RuntimeSlotCapacityRejectCountForDiagnostics { get => registryModule.RuntimeSlotCapacityRejectCount; private set => registryModule.RuntimeSlotCapacityRejectCount = value; }
        public long RuntimeRestBindRejectCountForDiagnostics { get => registryModule.RuntimeRestBindRejectCount; private set => registryModule.RuntimeRestBindRejectCount = value; }
        public long StableIdRegistrationRejectCountForDiagnostics { get => registryModule.StableIdRegistrationRejectCount; private set => registryModule.StableIdRegistrationRejectCount = value; }
        public long MissingUnregisterCountForDiagnostics { get => registryModule.MissingUnregisterCount; private set => registryModule.MissingUnregisterCount = value; }
        public long RuntimeSlotReleaseRejectCountForDiagnostics { get => registryModule.RuntimeSlotReleaseRejectCount; private set => registryModule.RuntimeSlotReleaseRejectCount = value; }
        public long RejectedVRestWriteCountForDiagnostics => _runtimeRestStore.RejectedVRestWriteCount;
        public long RejectedSoundEventCountForDiagnostics => battleBuffers.RejectedSoundEventCount;
        public bool ForceLegacyPendingDestroyScanForDiagnostics { get => registryModule.ForceLegacyPendingDestroyScan; set => registryModule.ForceLegacyPendingDestroyScan = value; }
        public bool EnableRegistryLifecycleLoggingForDiagnostics { get => registryModule.EnableRegistryLifecycleLogging; set => registryModule.EnableRegistryLifecycleLogging = value; }
        internal RuntimeRestStore RuntimeRestStoreForServices => _runtimeRestStore;
        internal RuntimeSlotTable RuntimeSlotsForServices => _runtimeSlots;
        internal IBattleParityStructuralEventSink StructuralEventSinkForServices => structuralEventSink;
        internal bool HasLateEntityStructuralEventSinkForModule => structuralEventSink != null;

        internal void BeginLateEntityStructuralEventContextForModule(int tickIndex)
        {
            SetStructuralEventContextForDiagnostics(
                tickIndex,
                "late-entity-update");
        }

        internal void EmitLateEntityStructuralScanForModule(
            int runtimeSlot,
            LF2Entity entity)
        {
            structuralEventCursorSlot = runtimeSlot;
            EmitStructuralEvent(
                "scan",
                runtimeSlot,
                0,
                RuntimeSlotCapacity,
                "active",
                "visited",
                StructuralSourceKind(entity),
                runtimeSlot);
        }

        internal void EndLateEntityStructuralEventContextForModule() => structuralEventCursorSlot = -1;

        public void SetStructuralEventSinkForDiagnostics(
            IBattleParityStructuralEventSink sink,
            int tick,
            string pass)
        {
            registryModule.SetStructuralEventSink(sink, tick, pass);
        }

        public void SetStructuralEventContextForDiagnostics(int tick, string pass) => registryModule.SetStructuralEventContext(tick, pass);

        public int FindFirstFreeRuntimeSlotForDiagnostics(
            int startSlot,
            int endSlotExclusive)
        {
            return FindFirstFreeRuntimeSlot(startSlot, endSlotExclusive);
        }

        private void EmitStructuralEvent(
            string action,
            int slot,
            int searchStart,
            int searchEndExclusive,
            string before,
            string after,
            string sourceKind,
            int actorSlot = -1)
        {
            registryModule.EmitStructuralEvent(
                action,
                slot,
                searchStart,
                searchEndExclusive,
                before,
                after,
                sourceKind,
                actorSlot);
        }

        private static string StructuralSourceKind(LF2Entity entity) => SimulationRegistryModule.StructuralSourceKind(entity);

        private bool ContainsRegisteredEntityStableId(int stableId) => registryModule.ContainsRegisteredEntityStableId(stableId);

        private int GetRuntimeSlotOrder(LF2Entity entity) => SimulationRegistryModule.GetRuntimeSlotOrder(entity);

        private int CompareRuntimeSlotOrder(LF2Entity a, LF2Entity b) => SimulationRegistryModule.CompareRuntimeSlotOrder(a, b);

        private void RefreshRuntimeSnapshot(ISimObject obj) => SimulationRegistryModule.RefreshRuntimeSnapshot(obj);

        internal void RefreshRuntimeSnapshotForModule(ISimObject obj) => RefreshRuntimeSnapshot(obj);

        internal BattleEcsCharacterRecoveryResult
            ExecuteLateCharacterRecoveryForModule(
                LF2Entity entity,
                int tickIndex)
        {
            return battleEcsCharacterRecoveryPass.Execute(entity, tickIndex);
        }

        internal bool TryExecuteLateCharacterFrameTickForModule(LF2Entity entity) => battleEcsCharacterFrameTickPass.TryExecute(entity);

        internal IBattleObjectPointStructuralMaterializer
            ResolveLateObjectPointStructuralMaterializerForModule()
        {
            return ResolveObjectPointFactoryForSimulation() as
                IBattleObjectPointStructuralMaterializer;
        }

        internal ObjectDefinition ResolveLateState9996ObjectDefinitionForModule(
            int objectId)
        {
            ObjectDefinition definition =
                runtimeDataCatalog.GetObjectDefinition(objectId);
            if (definition == null && !runtimeDataCatalog.IsSealedForBattle)
                definition = GameDataManager.Instance?.GetObjectById(objectId);
            return definition;
        }

        internal int GetRuntimeSlotOrderForLateModule(LF2Entity entity) => GetRuntimeSlotOrder(entity);

        internal void EndLateEntityMutationTickingForModule() => _ticking = false;

        internal void FlushLateEntityPendingMutationsForModule()
        {
            FlushPendingUnregister();
            FlushPendingEntityDestroy();
        }

        internal bool TryResolveRuntimeHandleForEarlyFrameAdvance(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        internal bool TryResolveRuntimeHandleForInteractionModule(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        internal LF2Entity GetCurrentRuntimeSlotOccupantForInteractionModule(
            int runtimeSlot)
        {
            return _runtimeSlots.GetCurrentOccupant(runtimeSlot);
        }

        internal void InvalidateAiUnifiedRowMembershipForModule() => battleAiUnifiedRowPublisher.InvalidateAfterRowMembershipChange();

        internal NTSDEntityRuntime GetRawRuntimeSlotState(int runtimeSlot) => registryModule.GetRawRuntimeSlotState(runtimeSlot);

        internal bool TryGetCurrentRuntimeHandle(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return registryModule.TryGetCurrentRuntimeHandle(
                runtimeSlot,
                expectedEntity,
                out handle);
        }

        internal bool TryResolveRuntimeHandle(RuntimeEntityHandle handle, out LF2Entity entity) => registryModule.TryResolveRuntimeHandle(handle, out entity);

        public bool TryGetCurrentRuntimeHandleForDiagnostics(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return registryModule.TryGetCurrentRuntimeHandle(
                runtimeSlot,
                expectedEntity,
                out handle);
        }

        public bool TryResolveRuntimeHandleForDiagnostics(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            return registryModule.TryResolveRuntimeHandle(handle, out entity);
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        /// <summary>
        /// Returns currently claimed, pass-active runtime entities by resolving a fresh
        /// generation-checked handle for every runtime slot. This intentionally does
        /// not use bucket/pass queries so diagnostic cleanup can find leaked entries.
        /// </summary>
        public void GetActiveRuntimeEntitySnapshotForDiagnostics(List<LF2Entity> dst) => registryModule.GetActiveRuntimeEntitySnapshot(dst);

        /// <summary>
        /// Forces the same delayed destroy release boundary used by simulation passes.
        /// It never resets the world and is only valid outside a running pass.
        /// </summary>
        public void FlushPendingDestroyForDiagnostics()
        {
            if (_ticking)
                throw new System.InvalidOperationException(
                    "Diagnostic destroy flushing cannot run while SimulationWorld is ticking.");

            ReleasePendingDestroySlots();
            FlushPendingUnregister();
            FlushPendingEntityDestroy();
            FlushPendingUnregister();
        }
#endif

        internal bool TryGetRuntimeSlotReadOnlyView(
            int runtimeSlot,
            out RuntimeSlotTable.ReadOnlySlotView view)
        {
            return registryModule.TryGetRuntimeSlotReadOnlyView(
                runtimeSlot,
                out view);
        }

        public bool TryGetRuntimeSlotReadOnlyViewForDiagnostics(
            int runtimeSlot,
            out RuntimeSlotTable.ReadOnlySlotView view)
        {
            return registryModule.TryGetRuntimeSlotReadOnlyView(
                runtimeSlot,
                out view);
        }

        public void ResetRuntimeState()
        {
            EnsureAiSensingModeAvailableBeforeTick();
            stageRenderModule.Reset();
            ResetRegisteredObjects();
            battleEcsShadowModule.Reset();
            battleEcsCooldownPass.Reset();
            battleEcsCharacterStageZPass.Reset();
            battleEcsCharacterPreFrameBoundsPass.Reset();
            battleEcsFramePostProcessPass.Reset();
            battleEcsPositiveLinkValidationPass.Reset();
            battleEcsCharacterFrameAdvancePass.Reset();
            battleEcsCharacterRecoveryPass.Reset();
            battleEcsCharacterFrameTickPass.Reset();
            battleEcsCharacterInputPass.Reset();
            battleEcsCharacterPostFrameTailPass.Reset();
            battleEcsHitExecutionPlan.Reset();

            Runtime ??= new BattleRuntimeState();
            Runtime.Reset();
            // Unity lockstep owns one deterministic stream per SimulationWorld. The
            // explicit reset seed is an adapter boundary: it makes a world reset
            // replayable without sharing RNG state between independent Unity worlds.
            // It must remain distinct from MatchConfig.seed, which is applied by the
            // simulation driver at the formal battle-bootstrap boundary.
            Rng?.Seed(0x4E545344u);
            PendingSounds.Clear();
            _cameraX = 0;
            _cameraVel = 0;
            _nextAutoStableId = 100;
        }

        internal bool TryShutdownAndClearLogicState(
            out int releasedLogicEntities,
            out string failureReason)
        {
            return registryModule.TryShutdownAndClearLogicState(
                _entityScratch,
                out releasedLogicEntities,
                out failureReason);
        }

        private void ResetRegisteredObjects()
        {
            registryModule.ResetRegisteredObjects(
                battleBuffers.RegisteredObjectResetSet,
                _pendingUnregister,
                _pendingSlotReleasedDestroy,
                _entityScratch);
        }

        public void Register(ISimObject obj) => battleStructuralWriter.Register(obj);

        internal void RegisterCoreFromStructuralWriter(ISimObject obj)
        {
            registryModule.RegisterCore(
                obj,
                _pendingUnregister,
                _pendingSlotReleasedDestroy);
        }

        public void Unregister(ISimObject obj) => battleStructuralWriter.Unregister(obj);

        internal void UnregisterCoreFromStructuralWriter(ISimObject obj) => registryModule.UnregisterCore(obj, _pendingUnregister);

        private void FlushPendingUnregister() => registryModule.FlushPendingUnregister(_pendingUnregister);

        private void FlushPendingEntityDestroy()
        {
            registryModule.FlushPendingEntityDestroy(
                _pendingSlotReleasedDestroy,
                _entityScratch);
        }

        private bool IsActiveForCurrentPass(ISimObject obj)
        {
            return SimulationRegistryModule.IsActiveForCurrentPass(
                obj,
                _pendingUnregister);
        }

        internal bool IsActiveForCurrentPassInternal(ISimObject obj) => IsActiveForCurrentPass(obj);

        internal bool HasUnityPresentationBindingsForDedicatedWorker() => registryModule.HasUnityPresentationBindings();

        private int FindFirstFreeRuntimeSlot(int startSlot, int endSlotExclusive)
        {
            return registryModule.FindFirstFreeRuntimeSlot(
                startSlot,
                endSlotExclusive);
        }

        internal void ReleasePendingDestroySlotsForRegistry() => ReleasePendingDestroySlots();

        internal bool TryGrowDesktopRuntimeSlotsForRegistry(long minimumCapacity) => TryGrowDesktopRuntimeSlots(minimumCapacity);

        private bool TryGrowDesktopRuntimeSlots(long minimumCapacity)
        {
            if (minimumCapacity <= RuntimeSlotCapacity)
                return true;
            if (!runtimeCapacityModule.TryAuthorizeGrowth())
                return false;
            if (activeRuntimeProfile != BattleRuntimeProfile.DesktopExtended ||
                minimumCapacity > int.MaxValue)
            {
                return false;
            }

            int normalizedCapacity;
            try
            {
                normalizedCapacity = BattleRuntimeProfilePolicy.NormalizeDesktopCapacity(
                    (int)minimumCapacity);
            }
            catch (System.ArgumentOutOfRangeException)
            {
                return false;
            }

            var grownAiInputSlots = new LF2Entity[normalizedCapacity];
            System.Array.Copy(aiInputSlots, grownAiInputSlots, aiInputSlots.Length);
            if (!_runtimeRestStore.GrowTo(normalizedCapacity) ||
                !_runtimeSlots.GrowTo(normalizedCapacity))
                return false;

            aiInputSlots = grownAiInputSlots;
            battleAiUnifiedRowPublisher.GrowTo(normalizedCapacity);
            battleIdentityWriter.GrowTo(normalizedCapacity);
            battleCharacterInputWriter.GrowTo(normalizedCapacity);
            battleFrameMotionWriter.GrowTo(normalizedCapacity);
            battleRelationLinkWriter.GrowTo(normalizedCapacity);
            battleVitalWriter.GrowTo(normalizedCapacity);
            GrowAiSoASensingRows(normalizedCapacity);
            return true;
        }

        private void ReleasePendingDestroySlots()
        {
            registryModule.ReleasePendingDestroySlots(
                _pendingSlotReleasedDestroy);
        }

        internal bool RestoreStageSpawnRestState(int runtimeSlot, LF2Entity entity)
        {
            return registryModule.RestoreStageSpawnRestState(
                runtimeSlot,
                entity);
        }

        internal int GetRawRestArest(int runtimeSlot) => registryModule.GetRawRestArest(runtimeSlot);

        internal int GetRawRestVrest(int victimSlot, int attackerSlot) => registryModule.GetRawRestVrest(victimSlot, attackerSlot);

        public int ObjectCount
        {
            get => registryModule.CountActiveObjects(_pendingUnregister);
        }

        // Battle pass compatibility façade pending PassPipeline extraction.
        internal int LastCollisionPairVRestEligibilityVisitCount { get; private set; }

        public bool ForceLegacyPreInteractionForDiagnostics { get => passPipeline.Interaction.ForceLegacyPreInteractionForDiagnostics; set => passPipeline.Interaction.ForceLegacyPreInteractionForDiagnostics = value; }
        public bool ForceLegacyPreInteractionCrossPassProofForDiagnostics
        {
            get => passPipeline.Interaction
                .ForceLegacyPreInteractionCrossPassProofForDiagnostics;
            set => passPipeline.Interaction
                .ForceLegacyPreInteractionCrossPassProofForDiagnostics = value;
        }
        public bool ForceLegacyPreInteractionParticipantFilteringForDiagnostics
        {
            get => passPipeline.Interaction
                .ForceLegacyPreInteractionParticipantFilteringForDiagnostics;
            set => passPipeline.Interaction
                .ForceLegacyPreInteractionParticipantFilteringForDiagnostics =
                value;
        }
        public bool ForceLegacyEmptyCharacterHitConsumeForDiagnostics
        {
            get => passPipeline.Interaction
                .ForceLegacyEmptyCharacterHitConsumeForDiagnostics;
            set => passPipeline.Interaction
                .ForceLegacyEmptyCharacterHitConsumeForDiagnostics = value;
        }
        public bool ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics
        {
            get => passPipeline.Interaction
                .ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics;
            set => passPipeline.Interaction
                .ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics =
                value;
        }
        public bool ForceLegacyEmptyObjectHitConsumeForDiagnostics
        {
            get => passPipeline.Interaction
                .ForceLegacyEmptyObjectHitConsumeForDiagnostics;
            set => passPipeline.Interaction
                .ForceLegacyEmptyObjectHitConsumeForDiagnostics = value;
        }
        public bool ForceLegacyLateTailNoOpForDiagnostics { get => passPipeline.LateEntityLifecycle.ForceLegacyTailNoOpForDiagnostics; set => passPipeline.LateEntityLifecycle.ForceLegacyTailNoOpForDiagnostics = value; }
        public bool ForceFullCharacterInputPostRefreshForDiagnostics { get; set; }
        public bool ForceFullAiUnifiedSnapshotRebuildForDiagnostics { get; set; }
        public bool ValidateIncrementalAiUnifiedRowForDiagnostics { get; set; }
        public long LastAiProjectionPublicationCountForDiagnostics =>
            battleCharacterInputWriter
                .LastAiProjectionPublicationCountForDiagnostics;
        public long LastAiProjectionPublicationSkipCountForDiagnostics =>
            battleCharacterInputWriter
                .LastAiProjectionPublicationSkipCountForDiagnostics;
        public int LastPreInteractionScannedCountForDiagnostics => passPipeline.Interaction.LastPreInteractionScannedCountForDiagnostics;
        public int LastPreInteractionExecutedCountForDiagnostics => passPipeline.Interaction.LastPreInteractionExecutedCountForDiagnostics;
        public int LastPreInteractionProofSkipCountForDiagnostics => passPipeline.Interaction.LastPreInteractionProofSkipCountForDiagnostics;
        public int LastPreInteractionSnapshotSkipCountForDiagnostics => passPipeline.Interaction.LastPreInteractionSnapshotSkipCountForDiagnostics;
        public int LastPreInteractionFailClosedCountForDiagnostics => passPipeline.Interaction.LastPreInteractionFailClosedCountForDiagnostics;
        public int LastPreInteractionCpointCheckProofSkipCountForDiagnostics =>
            passPipeline.Interaction
                .LastPreInteractionCpointCheckProofSkipCountForDiagnostics;
        public int LastPreInteractionMismatchTailProofSkipCountForDiagnostics =>
            passPipeline.Interaction
                .LastPreInteractionMismatchTailProofSkipCountForDiagnostics;
        public int LastPreInteractionHeldSyncProofSkipCountForDiagnostics =>
            passPipeline.Interaction
                .LastPreInteractionHeldSyncProofSkipCountForDiagnostics;
        public bool LastPreInteractionWholePassProofSucceededForDiagnostics =>
            passPipeline.Interaction
                .LastPreInteractionWholePassProofSucceededForDiagnostics;
        public int LastPreInteractionWholePassParticipantCountForDiagnostics =>
            passPipeline.Interaction
                .LastPreInteractionWholePassParticipantCountForDiagnostics;
        public bool LastPreInteractionCrossPassProofUsedForDiagnostics =>
            passPipeline.Interaction
                .LastPreInteractionCrossPassProofUsedForDiagnostics;
        public int LastEmptyCharacterHitConsumeSkipCountForDiagnostics =>
            passPipeline.Interaction
                .LastEmptyCharacterHitConsumeSkipCountForDiagnostics;
        public int LastCharacterHitConsumeExecutedCountForDiagnostics =>
            passPipeline.Interaction
                .LastCharacterHitConsumeExecutedCountForDiagnostics;
        public int LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics
        {
            get => passPipeline.Interaction
                .LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics;
        }
        public int LastCharacterRuntimeCandidateCountGateFallbackForDiagnostics
        {
            get => passPipeline.Interaction
                .LastCharacterRuntimeCandidateCountGateFallbackForDiagnostics;
        }
        public int LastEmptyObjectHitConsumeSkipCountForDiagnostics => passPipeline.Interaction.LastEmptyObjectHitConsumeSkipCountForDiagnostics;
        public int LastObjectHitConsumeExecutedCountForDiagnostics => passPipeline.Interaction.LastObjectHitConsumeExecutedCountForDiagnostics;
        public int LastLateTailNoOpSkipCountForDiagnostics => passPipeline.LateEntityLifecycle.LastTailNoOpSkipCountForDiagnostics;
        public int LastLateTailExecutedCountForDiagnostics => passPipeline.LateEntityLifecycle.LastTailExecutedCountForDiagnostics;
        public int LastLateOpointFactoryResolveCountForDiagnostics => passPipeline.LateEntityLifecycle.LastOpointFactoryResolveCountForDiagnostics;
        public int LastLateOpointFlushCountForDiagnostics => passPipeline.LateEntityLifecycle.LastOpointFlushCountForDiagnostics;
        public bool ForceLegacyLateCommonNoOpGatesForDiagnostics { get => passPipeline.LateEntityLifecycle.ForceLegacyCommonNoOpGatesForDiagnostics; set => passPipeline.LateEntityLifecycle.ForceLegacyCommonNoOpGatesForDiagnostics = value; }
        public bool ForceLegacyPostFrameRuntimeSnapshotForDiagnostics { get; set; }
        public int LastFramePostProcessRuntimeSnapshotSkipCountForDiagnostics { get; private set; }
        public int LastEntityPostFrameTailRuntimeSnapshotSkipCountForDiagnostics { get; private set; }
        public int LastLateStateSpecialNoOpSkipCountForDiagnostics => passPipeline.LateEntityLifecycle.LastStateSpecialNoOpSkipCountForDiagnostics;
        public int LastLateRecoveryNoOpSkipCountForDiagnostics => passPipeline.LateEntityLifecycle.LastRecoveryNoOpSkipCountForDiagnostics;
        public int LastLateDeathOpointNoOpSkipCountForDiagnostics => passPipeline.LateEntityLifecycle.LastDeathOpointNoOpSkipCountForDiagnostics;
        public int LastLateCleanupNoOpSkipCountForDiagnostics => passPipeline.LateEntityLifecycle.LastCleanupNoOpSkipCountForDiagnostics;
        public int LastCharacterInputProgressCommitCountForDiagnostics
        {
            get;
            private set;
        }
        public int LastCharacterInputProgressCommitSkipCountForDiagnostics
        {
            get;
            private set;
        }
        public bool ForceLegacyEarlyFrameAdvanceForDiagnostics { get => passPipeline.EarlyFrameAdvance.ForceLegacyForDiagnostics; set => passPipeline.EarlyFrameAdvance.ForceLegacyForDiagnostics = value; }
        public int LastEarlyTeleportRefreshCountForDiagnostics => passPipeline.EarlyFrameAdvance.LastTeleportRefreshCountForDiagnostics;
        public int LastEarlyTeleportSnapshotSkipCountForDiagnostics => passPipeline.EarlyFrameAdvance.LastTeleportSnapshotSkipCountForDiagnostics;
        public bool LastEarlyStateHandlePathUsedForDiagnostics => passPipeline.EarlyFrameAdvance.LastStateHandlePathUsedForDiagnostics;
        public int LastEarlyStateHandleFallbackCountForDiagnostics => passPipeline.EarlyFrameAdvance.LastStateHandleFallbackCountForDiagnostics;

        public void PostCooldownInputAll(int tickIndex)
        {
            PostCooldownHumanInputAll(tickIndex);
            CharacterInputAll(tickIndex);
        }

        public void FlushQueuedObjectPointTasks()
        {
            if (UsesLogicOnlyEntityMaterialization)
            {
                logicObjectPointRuntime.FlushTasks();
                return;
            }

            LF2ObjectPointFactory.Instance?.FlushTasks();
        }

        public void PostCooldownHumanInputAll(int tickIndex)
        {
            RefreshActiveHumanRosterInputBindings();
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (!IsBoundActiveHumanRosterInputEntity(entity) ||
                        !entity.TryGetSharedInputControllerForSimulation(out _))
                    {
                        continue;
                    }

                    entity.RunHumanInputPollPhase(tickIndex);
                    if (IsActiveForCurrentPass(entity))
                        RefreshRuntimeSnapshot(entity);
                }
            }
        }

        public void ClearBattleEntryInputAll()
        {
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    if (entity.GetCurrentDataObjectTypeForSimulation() !=
                        (int)LF2ObjectType.Character)
                    {
                        continue;
                    }

                    entity.ClearBattleEntryInputState();
                    if (IsActiveForCurrentPass(entity))
                        RefreshRuntimeSnapshot(entity);
                }
            }
        }

        public void AiInputAndComboAll(int tickIndex)
        {
            if (tickIndex <= 1)
                return;

            EnsureAiSensingModeAvailableBeforeTick();
            BuildAiInputSlotSnapshot();
            if (AiDecisionRequiresSharedRows &&
                !AiUnifiedSnapshotExecutionOwnsCurrentPass)
                PrepareAiDecisionSharedPass();
            CompleteAiUnifiedSnapshotShadowInitialComparison();
            try
            {
                using (BeginDeferredMutationEntityPass())
                {
                    foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                    {
                        if (!entity.AiControlled ||
                            entity.GetCurrentDataObjectTypeForSimulation() != 0)
                        {
                            continue;
                        }

                        BeginAiUnifiedSnapshotExecutionConsumer(entity);
                        entity.RunCharacterInputPhase(tickIndex);
#if UNITY_INCLUDE_TESTS
                        if (aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow)
                            aiRuntime.Decision.ApplySharedPostLegacyMutationForSelfCheck(entity);
#endif
                        if (IsActiveForCurrentPass(entity))
                            RefreshRuntimeSnapshot(entity);
                        if (AiUnifiedSnapshotExecutionOwnsCurrentPass)
                        {
                            RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput(entity);
                        }
                        else
                        {
                            if (AiDecisionRequiresSharedRows)
                                RefreshAiDecisionSharedRowAfterCharacterInput(entity);
                            if (aiSensingMode == AiSensingMode.SoAAiSensing)
                            {
                                ObserveAiCandidateCharacterInputMutation(entity);
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            }
                            else
                            {
                                ObserveAiTeamHpSummaryMutation(entity);
                            }
                            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing)
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            RefreshAiUnifiedSnapshotShadowRowAfterCharacterInput(entity);
                        }
                    }
                }
            }
            finally
            {
                if (AiDecisionRequiresSharedRows)
                    EndAiDecisionSharedPass();
                ClearAiInputSlotSnapshot();
            }
        }

        public void CharacterInputAll(int tickIndex)
        {
            if (tickIndex <= 1)
                return;

            LastCharacterInputProgressCommitCountForDiagnostics = 0;
            LastCharacterInputProgressCommitSkipCountForDiagnostics = 0;
            battleCharacterInputWriter.ResetAiProjectionPublicationDiagnostics();
            EnsureAiSensingModeAvailableBeforeTick();
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            BattleAiInputDetailDiagnostics aiDetailDiagnostics =
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            aiDetailDiagnostics?.BeginTick(tickIndex);
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.CharacterInputSnapshotBuild);
            BuildAiInputSlotSnapshot();
            if (AiDecisionRequiresSharedRows &&
                !AiUnifiedSnapshotExecutionOwnsCurrentPass)
                PrepareAiDecisionSharedPass();
            CompleteAiUnifiedSnapshotShadowInitialComparison();
            detailDiagnostics?.EndPhase(
                BattleTickDetailPhase.CharacterInputSnapshotBuild);
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.CharacterInputEntityInputPass);
            try
            {
                using (BeginDeferredMutationEntityPass())
                {
                    foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                    {
                        if (entity.GetCurrentDataObjectTypeForSimulation() !=
                            (int)LF2ObjectType.Character)
                        {
                            continue;
                        }

                        BeginAiUnifiedSnapshotExecutionConsumer(entity);
                        if (!battleEcsCharacterInputPass.TryExecute(entity, tickIndex))
                            entity.RunCharacterInputPhaseForKnownCharacterDat(tickIndex);
#if UNITY_INCLUDE_TESTS
                        if (aiDecisionShadowMode == AiDecisionShadowMode.SharedShadow)
                            aiRuntime.Decision.ApplySharedPostLegacyMutationForSelfCheck(entity);
                        runtimeHooks.CharacterInputPassMutationOverride?.Invoke(this, entity);
#endif
                        if (IsActiveForCurrentPass(entity))
                        {
                            aiDetailDiagnostics?.BeginPhase(
                                BattleAiInputDetailPhase.RefreshRuntimeSnapshot);
                            bool forceFullPostRefresh =
                                ForceFullCharacterInputPostRefreshForDiagnostics;
#if UNITY_INCLUDE_TESTS
                            forceFullPostRefresh |=
                                runtimeHooks.CharacterInputPassMutationOverride != null;
#endif
                            if (forceFullPostRefresh)
                                RefreshRuntimeSnapshot(entity);
                            else
                                entity.RefreshRuntimeSnapshotAfterCharacterInput();
                            aiDetailDiagnostics?.RecordRefresh();
                            aiDetailDiagnostics?.EndPhase(
                                BattleAiInputDetailPhase.RefreshRuntimeSnapshot);
                        }
                        if (AiUnifiedSnapshotExecutionOwnsCurrentPass)
                        {
                            aiDetailDiagnostics?.BeginPhase(
                                BattleAiInputDetailPhase.UnifiedSnapshotExecutionRowRefresh);
                            RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput(entity);
                            aiDetailDiagnostics?.EndPhase(
                                BattleAiInputDetailPhase.UnifiedSnapshotExecutionRowRefresh);
                        }
                        else
                        {
                            if (AiDecisionRequiresSharedRows)
                                RefreshAiDecisionSharedRowAfterCharacterInput(entity);
                            if (aiSensingMode == AiSensingMode.SoAAiSensing)
                            {
                                ObserveAiCandidateCharacterInputMutation(entity);
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            }
                            else
                            {
                                ObserveAiTeamHpSummaryMutation(entity);
                            }
                            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing)
                                RefreshAiSoASensingShadowRowAfterCharacterInput(entity);
                            RefreshAiUnifiedSnapshotShadowRowAfterCharacterInput(entity);
                        }
                    }
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputEntityInputPass);
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
                if (AiDecisionRequiresSharedRows)
                    EndAiDecisionSharedPass();
                ClearAiInputSlotSnapshot();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.CharacterInputSnapshotClear);
            }
        }

        internal void RecordCharacterInputProgressCommitForDiagnostics(bool committed)
        {
            if (committed)
                LastCharacterInputProgressCommitCountForDiagnostics++;
            else
                LastCharacterInputProgressCommitSkipCountForDiagnostics++;
        }

        public void Oid5152RuntimeMaintenanceAll(int tickIndex) => passPipeline.RunOid5152Maintenance(tickIndex);

        public void SerialTickAll(int tickIndex)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            using (BeginDeferredMutationEntityPass())
            {
                // C# authority GameTick scans active slots in ascending order and completes
                // one entity before advancing to the next slot. The dynamic scan lets a
                // flushed producer in a later slot participate this tick; a reused lower slot
                // waits until the next tick.
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    // Alignment contract R3-FRAME-001A: human poll and AI preparation write
                    // this tick's current keys before frame advance. C++ frame advance and
                    // late frame tick still consume them, so only their source-specific input
                    // producers or the battle-entry branch own any clear/roll boundary.
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.FrameAdvanceTransit);
                    if (!battleEcsCharacterFrameAdvancePass.TryExecute(
                            entity,
                            tickIndex))
                    {
                        entity.SimTransit(tickIndex);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.FrameAdvanceTransit);
                    if (!IsActiveForCurrentPass(entity))
                        continue;

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.FrameAdvanceEntityUpdate);
                    entity.SimTU(tickIndex);
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.FrameAdvanceEntityUpdate);
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.FrameAdvanceRuntimeSnapshot);
                    entity.RefreshRuntimeSnapshotAfterFrameAdvance();
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.FrameAdvanceRuntimeSnapshot);
                }

                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.FrameAdvanceState9998Cleanup);
                CleanupState9998Entities();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.FrameAdvanceState9998Cleanup);
            }
        }

        private void CleanupState9998Entities()
        {
            GetActiveEntitiesByRuntimeSlot(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                LF2FrameData frame = entity?.Frame?.D;
                if (frame == null || frame.state != 9998) continue;
                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        public void PostFrameAdvanceDeathCleanupAll(int tickIndex) => passPipeline.RunRespawn(tickIndex);

        public void EarlyFrameAdvanceSpecialsAll(int tickIndex) => passPipeline.RunEarlyFrameAdvance(tickIndex);

        public void FrameLogicBeforeAdvanceAll(int tickIndex)
        {
            using (BeginDeferredMutationEntityPass())
            {
                foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                {
                    LF2FrameData frame = entity.Frame?.D;
                    if (frame == null ||
                        frame.hit_Fa <= 0 ||
                        entity.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.Character)
                    {
                        continue;
                    }

                    entity.RunFrameLogicBeforeAdvance();
                    FlushQueuedObjectPointTasks();
                    if (!IsActiveForCurrentPass(entity))
                        continue;
                    RefreshRuntimeSnapshot(entity);
                }
            }
        }

        internal int FindFirstFreeFrameLogicRuntimeSlot() => FindFirstFreeRuntimeSlot(DynamicRuntimeSlotStart, RuntimeSlotCapacity);

        public void CaptureCollisionFrameSnapshotsAll()
        {
            BruteForceSceneQuery bruteForce = SceneQuery as BruteForceSceneQuery;
            int currentTick = CurrentTickIndex;
            bool completed = false;
            bruteForce?.BeginCollisionSnapshotRoleRoster(currentTick);
            try
            {
                using (BeginDeferredMutationEntityPass())
                {
                    foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
                    {
                        bruteForce?.ObserveCollisionSnapshotEntity(
                            entity,
                            currentTick);
                        if (entity.Runtime != null &&
                            entity.Runtime.SuppressCollisionCandidateUntilTick > 0 &&
                            currentTick <
                                entity.Runtime.SuppressCollisionCandidateUntilTick)
                        {
                            continue;
                        }

                        entity.CaptureCollisionFrameSnapshot();
                        entity.RefreshRuntimeSnapshotAfterCollisionSnapshot();
                        bruteForce?.ObserveCollisionSnapshotRole(
                            entity,
                            currentTick);
                    }
                }

                completed = true;
            }
            finally
            {
                bruteForce?.CompleteCollisionSnapshotRoleRoster(
                    currentTick,
                    completed);
            }
        }

        public void CollectCollisionCandidatesAll()
        {
            if (SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.CollectCollisionCandidates();
        }

        public void TickCollisionPairVRestAll()
        {
            _runtimeRestStore.BeginCollisionPairVRestEligibility();
            int visitedItems = 0;
            for (int bucketIndex = 0;
                 bucketIndex < objectBucketRegistry.OrderedCount;
                 bucketIndex++)
            {
                List<ISimObject> items =
                    objectBucketRegistry.GetOrderedBucket(bucketIndex).items;
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    visitedItems++;
                    if (items[itemIndex] is not LF2Entity entity ||
                        !IsActiveForCurrentPass(entity) ||
                        entity.FrameCache?.Wrapper?.characterData == null)
                    {
                        continue;
                    }

                    int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                    if (!_runtimeSlots.IsAddressable(runtimeSlot) ||
                        !object.ReferenceEquals(
                            _runtimeSlots.GetCurrentOccupant(runtimeSlot),
                            entity))
                    {
                        continue;
                    }

                    _runtimeRestStore.MarkCollisionPairVRestEligible(runtimeSlot);
                }
            }
            LastCollisionPairVRestEligibilityVisitCount = visitedItems;
            _runtimeRestStore.TickMarkedCollisionPairVRest();
        }

        public void EndCollisionCandidateConsumption() => passPipeline.EndCollisionCandidateConsumption();

        public void LateEntityUpdateAll(int tickIndex) => passPipeline.RunLateEntityLifecycle(tickIndex);

        internal void RunLateStateSpecialPreCollisionForSelfCheck(
            LF2Entity entity)
        {
            passPipeline.RunLateStateSpecialPreCollisionForSelfCheck(
                entity);
        }

        public BattleLateRuntimeSnapshotMode LateRuntimeSnapshotModeForDiagnostics
        {
            get => passPipeline.LateEntityLifecycle.RuntimeSnapshotModeForDiagnostics;
            set => passPipeline.LateEntityLifecycle.RuntimeSnapshotModeForDiagnostics =
                value;
        }

        internal void RefreshLateTransitionRuntimeSnapshot(LF2Entity entity) => passPipeline.RefreshLateTransitionRuntimeSnapshot(entity);

        public void EntityPostFrameTailAll(int tickIndex)
        {
            LastEntityPostFrameTailRuntimeSnapshotSkipCountForDiagnostics = 0;
            foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
            {
                if (entity.Health == null)
                    continue;

                // Alignment contract: R8-FUNCTIONKEYMODE-001. C++ applies
                // g_init_stats before heal/catch maintenance for every active entity.
                if (InitStatsRequest == 1)
                {
                    entity.Health.HP3 = 500;
                    entity.Health.HPBound = 500;
                    entity.Health.HP = 500;
                    entity.Health.PP = 500;
                }

                if (battleEcsCharacterPostFrameTailPass.TryExecute(entity))
                {
                    LastEntityPostFrameTailRuntimeSnapshotSkipCountForDiagnostics++;
                    continue;
                }

                if (entity.HealTimer / 1000 == 1 && entity.Health.HP > 0)
                {
                    entity.HealTimer--;
                    if (entity.HealTimer % 8 == 0)
                    {
                        if (entity.Health.HP < entity.Health.HPBound)
                        {
                            entity.Health.HP += 8;
                            if (entity.Health.HP > entity.Health.HPBound)
                                entity.Health.HP = entity.Health.HPBound;
                        }
                        else
                        {
                            entity.HealTimer = 0;
                        }
                    }

                    if (entity.HealTimer % 1000 == 0)
                        entity.HealTimer = 0;
                }

                if (entity.CatchTimer > 0 && entity.Health.HP > 0)
                {
                    entity.CatchTimer--;
                    if (entity.CatchTimer % 8 == 0 && entity.Health.HP < entity.Health.HPBound)
                    {
                        entity.Health.HP += 8;
                        if (entity.Health.HP > entity.Health.HPBound)
                        {
                            entity.Health.HP = entity.Health.HPBound;
                            entity.CatchTimer = 0;
                        }
                    }
                }

                LF2FrameData frame = entity.Frame?.D;
                if (frame != null && frame.state == 1700)
                    entity.HealTimer = 1100;

                entity.ClearHitCandidateCarriers();
                entity.Runtime.TransientMp = 0;
                entity.Runtime.TransientMp2 = 1000;
                entity.Runtime.TransientMp3 = 1000;
                entity.Runtime.TransientMp4 = 1000;
                if (ForceLegacyPostFrameRuntimeSnapshotForDiagnostics)
                {
                    RefreshRuntimeSnapshot(entity);
                }
                else if (!entity.RefreshRuntimeSnapshotAfterPostFrameMaintenance())
                {
                    LastEntityPostFrameTailRuntimeSnapshotSkipCountForDiagnostics++;
                }
            }

        }

        public void FramePostProcessAll()
        {
            LastFramePostProcessRuntimeSnapshotSkipCountForDiagnostics = 0;
            foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
            {
                if (entity.FrameDelay != 0)
                    continue;

                if (entity.HitCount > 0)
                {
                    double denom = entity.HitCount + 1.0;
                    entity.PS.vx = entity.KnockbackVx * 2.0 / denom;
                    entity.PS.vy = entity.KnockbackVy * 2.0 / denom;
                    entity.PS.vz = entity.KnockbackVz * 2.0 / denom;
                    entity.HitCount = 0;
                }
                entity.KnockbackVx = 0f;
                entity.KnockbackVy = 0f;
                entity.KnockbackVz = 0f;
                if (ForceLegacyPostFrameRuntimeSnapshotForDiagnostics)
                {
                    RefreshRuntimeSnapshot(entity);
                }
                else if (!entity.RefreshRuntimeSnapshotAfterPostFrameMaintenance())
                {
                    LastFramePostProcessRuntimeSnapshotSkipCountForDiagnostics++;
                }
            }
        }

        public void VrestTickAll(int tickIndex)
        {
            foreach (LF2Entity entity in ActiveEntitiesByRuntimeSlot)
            {
                entity.ItrRest?.TickArest();
                ClearAttackExemptIfCurrentFrameCannotHit(entity);
                RefreshRuntimeSnapshot(entity);
            }
        }

        private void ClearAttackExemptIfCurrentFrameCannotHit(LF2Entity entity)
        {
            if (entity == null || entity.AttackExempt <= 0)
                return;

            LF2CharacterData entityData = (entity as LF2LivingObject)?._FrameDataWrapper?.characterData
                ?? entity.FrameCache?.Wrapper?.characterData;
            if (entityData == null)
                return;

            LF2FrameData frame = entity.Frame?.D;
            bool clear = frame?.itrs == null || frame.itrs.Count == 0;
            if (!clear &&
                frame.state == LF2States.WeaponOnHand &&
                entity.Runtime != null)
            {
                int holderSlot = entity.Runtime.ResolveActiveHolderSlotIndex();
                LF2Entity holder = holderSlot >= 0
                    ? FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                LF2CharacterData holderData = (holder as LF2LivingObject)?._FrameDataWrapper?.characterData
                    ?? holder?.FrameCache?.Wrapper?.characterData;
                if (holder != null && holderData != null)
                {
                    LF2FrameData holderFrame = holder.Frame?.D;
                    clear = holderFrame == null ||
                            holderFrame.PrimaryWeaponPoint.Attacking == 0;
                }
            }

            if (clear)
                entity.AttackExempt = 0;
        }

        public void PostInteractionTickAll(int tickIndex) => passPipeline.RunPostInteraction(tickIndex);

        public void ObjectInteractionTickAll(int tickIndex) => passPipeline.RunObjectInteraction(tickIndex);

        public void PreInteractionTickAll(int tickIndex) => passPipeline.RunPreInteraction(tickIndex);

        public void RandomWeaponDropTickAll(int tickIndex) => passPipeline.RunRandomWeaponDrop(tickIndex);

        public void Mode2RandomWeaponDropTailAll(int tickIndex) => passPipeline.RunMode2RandomWeaponDropTail(tickIndex);

        internal void ClearFunctionKeyRequestsAfterPostFrameTail()
        {
            SetInitStatsRequest(0);
            SetMode2Request(0);
        }

        internal void ClearMode2RequestAfterPostFrameTail() => ClearFunctionKeyRequestsAfterPostFrameTail();

#if UNITY_INCLUDE_TESTS
        internal int[] CaptureLateRuntimeSnapshotBoundaryForSelfCheck(int mode)
        {
            return CaptureLateRuntimeSnapshotBoundaryForModeForSelfCheck(
                mode,
                (int)BattleLateRuntimeSnapshotMode.LegacyThree);
        }

        internal int[] CaptureLateRuntimeSnapshotBoundaryForModeForSelfCheck(
            int mode,
            int snapshotMode)
        {
            if (snapshotMode < (int)BattleLateRuntimeSnapshotMode.LegacyThree ||
                snapshotMode > (int)BattleLateRuntimeSnapshotMode.ConsolidatedFinal)
            {
                throw new System.ArgumentOutOfRangeException(nameof(snapshotMode));
            }

            LateRuntimeSnapshotModeForDiagnostics =
                (BattleLateRuntimeSnapshotMode)snapshotMode;
            LF2Entity entity;
            LateRuntimeSnapshotProbe probe = null;
            LateRuntimeSnapshotWeaponProbe weapon = null;
            if (mode == 3)
            {
                weapon = new LateRuntimeSnapshotWeaponProbe();
                weapon.BindData();
                entity = weapon;
            }
            else
            {
                probe = new LateRuntimeSnapshotProbe(
                    zeroHpDuringRecovery: mode == 0,
                    cleanupCompleted: mode == 2);
                entity = probe;
            }

            Register(entity);
            if (mode == 1)
                entity.Runtime.SuppressLateFrameTickUntilTick = 2;
            if (mode == 4 || mode == 5)
            {
                int exitFrame = mode == 4 ? 1100 : 1200;
                entity.WriteCurrentFrameId(exitFrame);
            }

            BattleTickDetailPhaseDiagnostics diagnostics =
                EnableBattleTickDetailPhaseDiagnosticsForDiagnostics();
            diagnostics.BeginTick(1);
            LateEntityUpdateAll(1);

            return new[]
            {
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.Recovery),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTickSuppressed),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.CleanupCompleted),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameTick),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.DeathOpoint),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.TailAndQueuedFlush),
                probe?.RecoveryCount ?? 0,
                probe?.FrameTickCount ?? 0,
                probe?.FrameTickObservedHp ?? 0,
                probe?.DeathOpointCount ?? 0,
                probe?.DeathOpointObservedHp ?? 0,
                probe?.CleanupCount ?? 0,
                probe?.TailCount ?? 0,
                ObjectCount,
                weapon?.PendingDestroyObserved == true ? 1 : 0,
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.FrameExit),
                (int)diagnostics.GetLastLateRuntimeSnapshotCallCount(
                    BattleLateRuntimeSnapshotStage.TransitionInternal),
                entity.Runtime?.Frame ?? -1,
            };
        }

        private sealed class LateRuntimeSnapshotProbe : LF2Entity
        {
            private readonly bool zeroHpDuringRecovery;
            private readonly bool cleanupCompleted;

            internal int RecoveryCount { get; private set; }
            internal int FrameTickCount { get; private set; }
            internal int FrameTickObservedHp { get; private set; }
            internal int DeathOpointCount { get; private set; }
            internal int DeathOpointObservedHp { get; private set; }
            internal int CleanupCount { get; private set; }
            internal int TailCount { get; private set; }
            public override LF2ObjectType ObjectTypeEnum =>
                LF2ObjectType.Character;
            internal override bool UsesDynamicRuntimeSlot() => true;

            internal LateRuntimeSnapshotProbe(
                bool zeroHpDuringRecovery,
                bool cleanupCompleted)
            {
                this.zeroHpDuringRecovery = zeroHpDuringRecovery;
                this.cleanupCompleted = cleanupCompleted;
                Name = "LateRuntimeSnapshotProbe";
                ObjectId = 1;
                Health = new LF2Health();
                Health.BindRuntime(Runtime);
                Health.HP = 100;
                Health.HPBound = 100;
                ItrRest = new LF2ItrRestTracker();
                PS.BindRuntime(Runtime);
                Trans = new FrameTransistor(this);
                var frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 1,
                    next = 0,
                    centerx = 0,
                    centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = (int)LF2ObjectType.Character,
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                WriteCurrentFrameId(0);
                Frame.PN = 0;
                Frame.Prev = 0;
                Runtime.PrevFrame2 = 0;
            }

            internal override void RunPreCollisionRecoveryPhase(int tickIndex)
            {
                RecoveryCount++;
                if (zeroHpDuringRecovery)
                    Health.HP = 0;
            }

            public override void SimFrameTick(int tickIndex)
            {
                FrameTickCount++;
                FrameTickObservedHp = Runtime.HP;
            }

            internal override void RunLateDeathOpointPreCleanupPhase()
            {
                DeathOpointCount++;
                DeathOpointObservedHp = Runtime.HP;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                CleanupCount++;
                return cleanupCompleted;
            }

            internal override void RunLateTailBeforePrevFrame()
            {
                TailCount++;
            }

            public override void Reset()
            {
            }

            public override void Init(
                LF2TaskBase task,
                LF2ObjectRenderer renderer)
            {
            }
        }

        private sealed class LateRuntimeSnapshotWeaponProbe : LF2Weapon
        {
            internal bool PendingDestroyObserved { get; private set; }

            internal void BindData()
            {
                Name = "LateRuntimeSnapshotDepletedWeapon";
                ObjectId = 100;
                SetWeaponType((int)LF2ObjectType.LightWeapon);
                PS.BindRuntime(Runtime);
                Health.BindRuntime(Runtime);
                ItrRest = new LF2ItrRestTracker();
                Trans = new FrameTransistor(this);
                var frame = new LF2FrameData
                {
                    frameId = 0,
                    state = 0,
                    wait = 100,
                    next = 0,
                    centerx = 0,
                    centery = 0,
                };
                FrameCache.Load(new LF2CharacterDataWrapper(
                    ObjectId,
                    new LF2CharacterData
                    {
                        name = Name,
                        type_sub = 100,
                        weapon_hp = 1,
                        weapon_broken_sound = "LateSnapshot_Depleted",
                        frames = new List<LF2FrameData> { frame },
                    }));
                Frame.D = frame;
                Frame.PN = 0;
                WriteCurrentFrameId(0);
                Frame.Prev = 0;
                Runtime.PrevFrame2 = 0;
                Health.HP = 1;
                Health.HPBound = 1;
                Runtime.WeaponFlightCounter = -1;
            }

            internal override bool TryRunLatePostOpointCleanupPhase()
            {
                bool completed = base.TryRunLatePostOpointCleanupPhase();
                PendingDestroyObserved |= Runtime.PendingFlushDestroy;
                return completed;
            }
        }
#endif

        // AI input compatibility façade pending full Input module extraction.
        private LF2Entity[] aiInputSlots { get => aiRuntime.Input.Slots; set => aiRuntime.Input.Slots = value; }
        private LooseQuadtreeBroadphase aiInputSpatialBroadphase => aiRuntime.Input.SpatialBroadphase;
        private List<IncrementalSpatialEntry> aiInputSpatialEntries => aiRuntime.Input.SpatialEntries;
        private List<RuntimeEntityHandle> aiInputSpatialHandles => aiRuntime.Input.SpatialHandles;
        private List<int> aiInputSpatialSlots => aiRuntime.Input.SpatialSlots;
        private LooseQuadtreeBroadphase aiInputGroundSpatialBroadphase => aiRuntime.Input.GroundSpatialBroadphase;
        private List<IncrementalSpatialEntry> aiInputGroundSpatialEntries => aiRuntime.Input.GroundSpatialEntries;
        private Dictionary<int, AiGroundTeamPartition> aiInputGroundTeamPartitions => aiRuntime.Input.GroundTeamPartitions;
        private List<AiGroundTeamPartition> aiInputActiveGroundTeamPartitions => aiRuntime.Input.ActiveGroundTeamPartitions;
        private AiGroundTeamPartition[] aiInputGroundTeamPartitionPool => aiRuntime.Input.GroundTeamPartitionPool;
        private LooseQuadtreeBroadphase aiInputAirSpatialBroadphase => aiRuntime.Input.AirSpatialBroadphase;
        private List<IncrementalSpatialEntry> aiInputAirSpatialEntries => aiRuntime.Input.AirSpatialEntries;
        private List<int> aiSpecialScanSlots => aiRuntime.Input.SpecialScanSlots;
        private List<int> aiPhase1TargetSlots => aiRuntime.Input.Phase1TargetSlots;
        private bool aiPhase1TargetSlotsValid { get => aiRuntime.Input.Phase1TargetSlotsValid; set => aiRuntime.Input.Phase1TargetSlotsValid = value; }
        private int aiMoveModeTopSlot { get => aiRuntime.Input.MoveModeTopSlot; set => aiRuntime.Input.MoveModeTopSlot = value; }
        private int aiMoveModeSecondSlot { get => aiRuntime.Input.MoveModeSecondSlot; set => aiRuntime.Input.MoveModeSecondSlot = value; }
        private bool aiMoveModeFirst10Valid { get => aiRuntime.Input.MoveModeFirst10Valid; set => aiRuntime.Input.MoveModeFirst10Valid = value; }
        private Dictionary<int, AiTeamHpSummary> aiTeamHpSummaries => aiRuntime.Input.TeamHpSummaries;
        private int[] aiInputGroundXBySlot { get => aiRuntime.Input.GroundXBySlot; set => aiRuntime.Input.GroundXBySlot = value; }
        private int[] aiInputGroundZBySlot { get => aiRuntime.Input.GroundZBySlot; set => aiRuntime.Input.GroundZBySlot = value; }
        private uint[] aiInputGroundGenerationBySlot { get => aiRuntime.Input.GroundGenerationBySlot; set => aiRuntime.Input.GroundGenerationBySlot = value; }
        private AiNearestSlotFacts[] aiNearestFactsBySlot { get => aiRuntime.Input.NearestFactsBySlot; set => aiRuntime.Input.NearestFactsBySlot = value; }
        private uint aiNearestFactsActiveVersion { get => aiRuntime.Input.NearestFactsActiveVersion; set => aiRuntime.Input.NearestFactsActiveVersion = value; }
        private ulong aiInputSlotSnapshotOccupancyEpoch { get => aiRuntime.Input.SlotSnapshotOccupancyEpoch; set => aiRuntime.Input.SlotSnapshotOccupancyEpoch = value; }
        private bool aiInputSpatialReady { get => aiRuntime.Input.SpatialReady; set => aiRuntime.Input.SpatialReady = value; }
        private bool aiInputGroundSpatialReady { get => aiRuntime.Input.GroundSpatialReady; set => aiRuntime.Input.GroundSpatialReady = value; }
        private bool aiInputGroundTeamPartitionsValid { get => aiRuntime.Input.GroundTeamPartitionsValid; set => aiRuntime.Input.GroundTeamPartitionsValid = value; }
        private bool aiInputAirSpatialReady { get => aiRuntime.Input.AirSpatialReady; set => aiRuntime.Input.AirSpatialReady = value; }
        private int aiInputAirRoleCount { get => aiRuntime.Input.AirRoleCount; set => aiRuntime.Input.AirRoleCount = value; }
        private bool aiInputAirRoleCountValid { get => aiRuntime.Input.AirRoleCountValid; set => aiRuntime.Input.AirRoleCountValid = value; }
        private bool aiTeamHpSummaryValid { get => aiRuntime.Input.TeamHpSummaryValid; set => aiRuntime.Input.TeamHpSummaryValid = value; }

        // Diagnostic A/B switch. Production uses the compact slot list built from the same snapshot.
        internal bool ForceFullAiSpecialScanForDiagnostics { get => aiRuntime.Input.ForceFullSpecialScan; set => aiRuntime.Input.ForceFullSpecialScan = value; }
        internal bool ForceFullAiPhase1TargetScanForDiagnostics { get => aiRuntime.Input.ForceFullPhase1TargetScan; set => aiRuntime.Input.ForceFullPhase1TargetScan = value; }
        internal bool ForceFullAiSameTeamScanForDiagnostics { get => aiRuntime.Input.ForceFullSameTeamScan; set => aiRuntime.Input.ForceFullSameTeamScan = value; }
        internal bool ForceFullAiMoveModeScanForDiagnostics { get => aiRuntime.Input.ForceFullMoveModeScan; set => aiRuntime.Input.ForceFullMoveModeScan = value; }
        internal bool ForceFullAiNearestScanForDiagnostics { get => aiRuntime.Input.ForceFullNearestScan; set => aiRuntime.Input.ForceFullNearestScan = value; }
        internal bool ForceLegacyAiNearestQueryForDiagnostics { get => aiRuntime.Input.ForceLegacyNearestQuery; set => aiRuntime.Input.ForceLegacyNearestQuery = value; }
        public bool ForceLegacyAiNearestFilterForDiagnostics { get => aiRuntime.Input.ForceLegacyNearestFilter; set => aiRuntime.Input.ForceLegacyNearestFilter = value; }
        internal bool EnableAiNearestBestFirstShadowForDiagnostics { get => aiRuntime.Input.EnableNearestBestFirstShadow; set => aiRuntime.Input.EnableNearestBestFirstShadow = value; }
        public int AiSameTeamSummaryFallbackCountForDiagnostics { get => aiRuntime.Input.SameTeamSummaryFallbackCount; private set => aiRuntime.Input.SameTeamSummaryFallbackCount = value; }
        internal int AiNearestBestFirstShadowMismatchCountForDiagnostics { get => aiRuntime.Input.NearestBestFirstShadowMismatchCount; private set => aiRuntime.Input.NearestBestFirstShadowMismatchCount = value; }
        internal string AiNearestBestFirstFirstShadowMismatchForDiagnostics { get => aiRuntime.Input.NearestBestFirstFirstShadowMismatch; private set => aiRuntime.Input.NearestBestFirstFirstShadowMismatch = value; }
        internal int AiNearestAirPassCountForDiagnostics { get => aiRuntime.Input.NearestAirPassCount; private set => aiRuntime.Input.NearestAirPassCount = value; }

        internal void BuildAiInputSlotSnapshot() => aiRuntime.Input.BuildInputSlotSnapshot(this);

        internal bool AiInputUsesSoACandidateForModule => aiSensingMode == AiSensingMode.SoAAiSensing;

        internal bool TryPrepareUnifiedAiInputSnapshotForModule(
            BattleAiInputDetailDiagnostics diagnostics)
        {
            return aiUnifiedSnapshotExecutionMode ==
                       AiUnifiedSnapshotExecutionMode.UnifiedAuthority &&
                   TryPrepareAiUnifiedSnapshotExecutionPass(diagnostics);
        }

        internal void CaptureAiCandidateFusedSnapshotForInputModule(
            int runtimeCapacity,
            ulong occupancyEpoch)
        {
            CaptureAiSoACandidateFusedSnapshot(runtimeCapacity, occupancyEpoch);
        }

        internal void GetAllEntitiesForAiInputModule(List<LF2Entity> destination) => GetAllEntities(destination);

        internal void CaptureAiSensingShadowSnapshotForInputModule(
            ulong occupancyEpoch)
        {
            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing)
                CaptureAiSoASensingShadowSnapshot(occupancyEpoch);
        }

        internal void SynchronizeAiInputSpatialSnapshotForModule() => SynchronizeAiInputSpatialSnapshot();

        internal void ObserveAiSensingSnapshotBuildEpochForInputModule(
            ulong occupancyEpochBefore,
            ulong occupancyEpochAfter)
        {
            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing ||
                aiSensingMode == AiSensingMode.SoAAiSensing)
            {
                ObserveAiSoASensingSnapshotBuildEpoch(
                    occupancyEpochBefore,
                    occupancyEpochAfter);
            }
        }

        internal void PrepareAiUnifiedSnapshotShadowPassForInputModule(
            BattleAiInputDetailDiagnostics diagnostics)
        {
            PrepareAiUnifiedSnapshotShadowPass(diagnostics);
        }

        internal void ClearAiInputSlotSnapshot()
        {
            bool usedUnifiedAuthority =
                AiUnifiedSnapshotExecutionFallbackForbidden;
            bool preserveRollingSnapshot =
                aiUnifiedSnapshotExecutionMode ==
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority &&
                aiUnifiedSnapshotPublishedState != null &&
                battleAiUnifiedRowPublisher.Active;
            if (preserveRollingSnapshot)
                SuspendAiUnifiedSnapshotExecutionPass();
            else
                EndAiUnifiedSnapshotExecutionPass();
            if (usedUnifiedAuthority)
                RestoreAiUnifiedSnapshotLegacyConsumerBuffers();
            EndAiUnifiedSnapshotShadowPass();
            bool useSoACandidate = aiSensingMode == AiSensingMode.SoAAiSensing;
            if (aiSensingMode == AiSensingMode.SoAShadowAiSensing ||
                aiSensingMode == AiSensingMode.SoAAiSensing)
                ClearAiSoASensingShadowSnapshot();
            aiRuntime.Input.ClearSlotSnapshot(useSoACandidate);
        }


        // Candidate captures the shared first-ten move-mode snapshot in the fused
        // runtime-slot scan. In particular, do not populate Legacy team summaries,
        // special lists, phase-1 targets, nearest facts, or any quadtree state here.

        private void EnsureAiTeamHpSnapshotCapacity() => aiRuntime.Input.EnsureSnapshotCapacity();

        internal bool IsLivingCharacterDatForAiInputModule(LF2Entity entity) => IsLivingCharacterDat(entity);

        internal int GetAiTeamForInputModule(LF2Entity entity) => Team(entity);

        internal int GetAiHpForInputModule(LF2Entity entity) => Hp(entity);

        internal int GetAiXForInputModule(LF2Entity entity) => X(entity);

        internal int GetAiZForInputModule(LF2Entity entity) => Z(entity);

        internal int GetAiYForInputModule(LF2Entity entity) => Y(entity);

        internal bool IsCharacterDatForAiInputModule(LF2Entity entity) => IsCharacterDat(entity);

        internal int GetAiStateForInputModule(LF2Entity entity) => State(entity);

        internal int GetAiSlotForInputModule(LF2Entity entity) => Slot(entity);

        internal bool IsAirAiSpatialRoleForInputModule(LF2Entity entity) => IsAirAiSpatialRole(entity);

        internal bool IsGroundAiSpatialRoleForInputModule(LF2Entity entity) => IsGroundAiSpatialRole(entity);

        internal bool IsAirAiTargetCandidateForInputModule(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase)
        {
            return IsAirAiTargetCandidate(self, candidate, inputPhase);
        }

        internal bool IsGroundAiTargetCandidateForInputModule(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase)
        {
            return IsGroundAiTargetCandidate(self, candidate, inputPhase);
        }

        internal bool IsAirAiTargetCandidateForInputModule(
            LF2Entity self,
            int selfSlot,
            int selfTeam,
            AiNearestSlotFacts candidate,
            int inputPhase)
        {
            return IsAirAiTargetCandidate(
                self,
                selfSlot,
                selfTeam,
                candidate,
                inputPhase);
        }

        internal bool IsGroundAiTargetCandidateForInputModule(
            LF2Entity self,
            int selfSlot,
            int selfX,
            int selfTeam,
            AiNearestSlotFacts candidate,
            int inputPhase)
        {
            return IsGroundAiTargetCandidate(
                self,
                selfSlot,
                selfX,
                selfTeam,
                candidate,
                inputPhase);
        }





        private void ObserveAiTeamHpSummaryMutation(LF2Entity entity) => aiRuntime.Input.ObserveTeamHpSummaryMutation(this, entity);

        // Candidate owns nearest/special state in the SoA rows.  The sole Legacy-era
        // product still shared by CreateAiInputContext is the first-ten move-mode
        // snapshot, so do not touch facts, team summaries, phase-one lists, or trees.
        private void ObserveAiCandidateCharacterInputMutation(LF2Entity entity) => aiRuntime.Input.ObserveCandidateCharacterInputMutation(this, entity);

        private void ObserveAiAirSpatialRoleMutation(LF2Entity entity) => aiRuntime.Input.ObserveAirSpatialRoleMutation(this, entity);

        private void ResetAiAirSpatialIndex() => aiRuntime.Input.ResetAirSpatialIndex();

        private void InvalidateAiAirRoleSnapshot() => aiRuntime.Input.InvalidateAirRoleSnapshot();

        [System.Diagnostics.Conditional("UNITY_INCLUDE_TESTS")]

        [System.Diagnostics.Conditional("UNITY_INCLUDE_TESTS")]

        private void ObserveAiGroundSpatialRoleMutation(LF2Entity entity) => aiRuntime.Input.ObserveGroundSpatialRoleMutation(this, entity);





        private void InvalidateAiGroundTeamPartitions() => aiRuntime.Input.InvalidateGroundTeamPartitions();


        private bool TryGetAiSameTeamSummaryExcludingSelf(
            LF2Entity self,
            out int otherCount,
            out int otherMinHp)
        {
            if (self?.Runtime == null)
            {
                otherCount = 0;
                otherMinHp = int.MaxValue;
                return false;
            }

            int slot = Slot(self);
            int selfTeam = Team(self);
            int selfHp = Hp(self);
            return aiRuntime.Input.TryGetSameTeamSummaryExcludingSelf(
                self,
                slot,
                selfTeam,
                selfHp,
                out otherCount,
                out otherMinHp);
        }

        private void ScanAiSameTeamSummaryExcludingSelf(
            LF2Entity self,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            otherCount = 0;
            otherMinHp = int.MaxValue;
            for (int slot = 0; slot < aiInputSlots.Length; slot++)
            {
                LF2Entity teammate = AiAt(slot);
                if (teammate == null || teammate == self ||
                    !IsLivingCharacterDat(teammate) || Team(teammate) != selfTeam)
                {
                    continue;
                }

                int teammateHp = Hp(teammate);
                if (teammateHp < otherMinHp)
                    otherMinHp = teammateHp;
                otherCount++;
            }
        }

        internal bool ResolveAiSameTeamSummaryExcludingSelf(
            LF2Entity self,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            if (!ForceFullAiSameTeamScanForDiagnostics &&
                TryGetAiSameTeamSummaryExcludingSelf(self, out otherCount, out otherMinHp))
            {
                return true;
            }

            if (!ForceFullAiSameTeamScanForDiagnostics)
                AiSameTeamSummaryFallbackCountForDiagnostics++;
            ScanAiSameTeamSummaryExcludingSelf(self, selfTeam, out otherCount, out otherMinHp);
            return false;
        }

        private static bool IsAiSpecialScanObjectId(int objectId) => SimulationAiInputModule.IsSpecialScanObjectId(objectId);

        private void ResetAiMoveModeFirst10Snapshot() => aiRuntime.Input.ResetMoveModeFirst10Snapshot();




        private void SynchronizeAiInputSpatialSnapshot() => aiRuntime.Input.SynchronizeSpatialSnapshot(this);


        internal void PrepareAiInputBasic(LF2Entity self, int tickIndex) => aiRuntime.Decision.PrepareAiInputBasic(this, self, tickIndex);

        internal bool TryPrepareAiDecisionIndexedCanonicalForModule(
            LF2Entity self,
            int tickIndex)
        {
            return aiRuntime.Decision.TryPrepareIndexedCanonical(
                this,
                battleCharacterInputWriter,
                battleAiInputWriter,
                aiExecutionProfile,
                aiDecisionOwnedInputMode,
                self,
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics,
                CaptureAiDecisionWorldState(),
                Rng?.State ?? 0,
                Rng?.CallCount ?? 0,
                Rng != null && Runtime?.Flow != null);
        }

        internal bool BeginAiDecisionShadowComparisonForModule(
            LF2Entity self,
            int tickIndex)
        {
            return BeginAiDecisionShadowComparison(self, tickIndex);
        }

        internal void CompleteAiDecisionShadowComparisonForModule(
            bool comparisonStarted)
        {
            CompleteAiDecisionShadowComparison(comparisonStarted);
        }

        internal AiDecisionAvailability CaptureAiDecisionShadowSnapshotForModule(
            LF2Entity self,
            AiDecisionSnapshot snapshot)
        {
            return CaptureAiDecisionShadowSnapshot(self, snapshot);
        }

        internal AiDecisionAvailability CaptureAiDecisionSharedOwnedSnapshotForModule(
            LF2Entity self,
            AiDecisionSnapshot snapshot)
        {
            return aiRuntime.Decision.CaptureSharedOwnedSnapshot(
                battleCharacterInputWriter,
                aiExecutionProfile,
                aiDecisionOwnedInputMode,
                self,
                snapshot,
                CaptureAiDecisionWorldState(),
                Rng?.State ?? 0,
                Rng?.CallCount ?? 0);
        }

        internal void ThrowAiUnifiedSnapshotExecutionHardBreachForDecisionModule(
            AiUnifiedSnapshotExceptionStage stage,
            string message)
        {
            ThrowAiUnifiedSnapshotExecutionHardBreach(stage, message);
        }

        internal bool TryCaptureAiUnifiedAuthorityRowForDecisionModule(
            AiSoASensingRows rows,
            LF2Entity entity,
            int slot,
            uint generation,
            bool captureSpecialMembership,
            out int decisionBoundaryFlags)
        {
            return SimulationAiDecisionModule.TryCaptureUnifiedAuthorityRow(
                battleIdentityWriter,
                battleFrameMotionWriter,
                battleCharacterInputWriter,
                battleRelationLinkWriter,
                battleVitalWriter,
                rows,
                entity,
                slot,
                generation,
                captureSpecialMembership,
                out decisionBoundaryFlags);
        }

        internal void ActivateAiUnifiedSnapshotExecutionPassForDecisionModule(
            AiUnifiedSnapshotExecutionState candidate)
        {
            ActivateAiUnifiedSnapshotExecutionPass(candidate);
        }

        internal void CompareAiUnifiedSnapshotShadowForDecisionModule(
            AiUnifiedSnapshotConsumer consumer,
            bool fullComparison,
            int refreshSlot)
        {
            CompareAiUnifiedSnapshotShadow(consumer, fullComparison, refreshSlot);
        }

        internal bool HasCharacterInputPassMutationOverrideForDecisionModule
        {
            get
            {
#if UNITY_INCLUDE_TESTS
                return runtimeHooks.CharacterInputPassMutationOverride != null;
#else
                return false;
#endif
            }
        }

        internal void RecordAiDecisionShadowExceptionForModule(
            AiDecisionShadowExceptionStage stage,
            Exception exception)
        {
            RecordAiDecisionShadowException(stage, exception);
        }

        internal void InvalidateAiDecisionSharedPassForModule(
            AiDecisionAvailability reason)
        {
            InvalidateAiDecisionSharedPass(reason);
        }

        internal AiDecisionShadowMismatchReason CompareAiDecisionShadowResultForModule(
            LF2Entity self)
        {
            return CompareAiDecisionShadowResult(self);
        }

        internal static ulong ResolveAiInputDetailRngCallDelta(
            ulong before,
            ulong after)
        {
            return after >= before ? after - before : 0;
        }

        private bool TryBindAiSoADecisionRowContext(
            LF2Entity self,
            int selectedSlot,
            int cachedSlot,
            LF2Entity cached)
        {
            return aiRuntime.Decision.TryBindAiSoADecisionRowContext(
                this,
                self,
                selectedSlot,
                cachedSlot,
                cached);
        }

        private void TrackAiSoADecisionSelectedRow(int selectedSlot) => aiRuntime.Decision.TrackAiSoADecisionSelectedRow(selectedSlot);

        private bool TryGetAiSoADecisionRemainderRow(
            LF2Entity entity,
            out AiSoASensingRows rows,
            out int slot)
        {
            return aiRuntime.Decision.TryGetAiSoADecisionRemainderRow(
                entity,
                out rows,
                out slot);
        }

        private void LatchAiSoADecisionRemainderToLegacy() => aiRuntime.Decision.LatchAiSoADecisionRemainderToLegacy(this);

        private void CompleteAiSoADecisionRemainderInput() => aiRuntime.Decision.CompleteAiSoADecisionRemainderInput(this);

        internal int StageZMin => Runtime?.Stage?.ZMin ?? 180;
        internal int StageZMax => Runtime?.Stage?.ZMax ?? 350;
        internal int Rand(int modulus)
        {
            int random = Rng.NextRaw();
            int value = random % Math.Max(1, modulus);
            if (aiDecisionLegacyRngRecording)
                RecordAiDecisionShadowLegacyRng(modulus, random, value);
            if (aiSoADecisionRemainderUseRowsForCurrentInput &&
                !aiSoADecisionRemainderRandomBoundaryPassed)
            {
                aiSoADecisionRemainderRandomBoundaryPassed = true;
            }
            return value;
        }
        internal LF2Entity AiAt(int slot) => aiRuntime.Decision.AiAt(slot);
        internal int X(LF2Entity e) => aiRuntime.Decision.X(e);
        internal int Y(LF2Entity e) => aiRuntime.Decision.Y(e);
        internal int Z(LF2Entity e) => aiRuntime.Decision.Z(e);
        internal int Hp(LF2Entity e) => aiRuntime.Decision.Hp(e);
        internal int Hp3(LF2Entity e) => aiRuntime.Decision.Hp3(e);
        internal int HpMax(LF2Entity e) => aiRuntime.Decision.HpMax(e);
        internal int Pp(LF2Entity e) => aiRuntime.Decision.Pp(e);
        internal int Team(LF2Entity e) => aiRuntime.Decision.Team(e);
        internal int Slot(LF2Entity e) => aiRuntime.Decision.Slot(e);
        internal int Frame(LF2Entity e) => aiRuntime.Decision.Frame(e);
        private int HitJ(LF2Entity e) => aiRuntime.Decision.HitJ(e);
        internal int State(LF2Entity e) => aiRuntime.Decision.State(e);
        internal int Facing(LF2Entity e) => aiRuntime.Decision.Facing(e);
        internal int ObjectId(LF2Entity e) => aiRuntime.Decision.ObjectId(e);
        internal int LinkState(LF2Entity e) => aiRuntime.Decision.LinkState(e);
        internal int TargetSlot(LF2Entity e) => aiRuntime.Decision.TargetSlot(e);
        internal int HitStop(LF2Entity e) => aiRuntime.Decision.HitStop(e);
        internal double Vx(LF2Entity e) => aiRuntime.Decision.Vx(e);
        internal bool HasInputHistoryGate(LF2Entity e) => aiRuntime.Decision.HasInputHistoryGate(e);
        internal bool HasBoundaryBlock(LF2Entity e) => aiRuntime.Decision.HasBoundaryBlock(e);
        private static int Abs(int value) => Math.Abs(value);
        internal int Distance(LF2Entity a, LF2Entity b) => aiRuntime.Decision.Distance(a, b);
        internal bool IsCharacterDat(LF2Entity e) => aiRuntime.Decision.IsCharacterDat(e);
        internal bool IsLivingCharacterDat(LF2Entity e) => aiRuntime.Decision.IsLivingCharacterDat(e);

        internal int FindNearestAiTargetSlot(
            LF2Entity self,
            AiInputContext ai,
            out int bestDist,
            out bool sameZLane)
        {
            return aiRuntime.Input.FindNearestTargetSlot(
                this,
                self,
                ai,
                out bestDist,
                out sameZLane);
        }


        private AiNearestPointFilter CreateAiNearestPointFilter(
            LF2Entity self,
            int inputPhase,
            bool air)
        {
            return aiRuntime.Input.CreateNearestPointFilter(
                this,
                self,
                inputPhase,
                air);
        }

        private bool TryCreateAiNearestSnapshotStamp(
            out AiNearestSnapshotStamp stamp)
        {
            return aiRuntime.Input.TryCreateNearestSnapshotStamp(
                RuntimeSlotOccupancyEpochForServices,
                out stamp);
        }

        private bool IsAiNearestSnapshotStampCurrent(
            in AiNearestSnapshotStamp stamp)
        {
            return aiRuntime.Input.IsNearestSnapshotStampCurrent(
                stamp,
                RuntimeSlotOccupancyEpochForServices);
        }

        private bool TryFindNearestAiTargetSlotBestFirst(
            LF2Entity self,
            AiInputContext ai,
            out int selected,
            out int bestDist,
            out bool sameZLane,
            bool allowAirRoleFastPath = false)
        {
            return aiRuntime.Input.TryFindNearestTargetSlotBestFirst(
                this,
                self,
                ai,
                out selected,
                out bestDist,
                out sameZLane,
                allowAirRoleFastPath);
        }

        private bool TryFindNearestGroundInSingleAllowedTeamPartition(
            LF2Entity self,
            int inputPhase,
            ref AiNearestPointFilter filter,
            BattleAiInputDetailDiagnostics diagnostics,
            out bool handled,
            out RuntimeEntityHandle nearestHandle,
            out int nearestDistance,
            out int visitedRecords)
        {
            return aiRuntime.Input.TryFindNearestGroundInSingleAllowedTeamPartition(
                this,
                self,
                inputPhase,
                ref filter,
                diagnostics,
                out handled,
                out nearestHandle,
                out nearestDistance,
                out visitedRecords);
        }

        private static bool IsGroundTeamPartitionAllowed(
            int selfTeam,
            int candidateTeam,
            int inputPhase)
        {
            return SimulationAiInputModule.IsGroundTeamPartitionAllowed(
                selfTeam,
                candidateTeam,
                inputPhase);
        }

        private int CountAllowedGroundTeamPartitions(
            int selfTeam,
            int inputPhase,
            out AiGroundTeamPartition singlePartition)
        {
            return aiRuntime.Input.CountAllowedGroundTeamPartitions(
                selfTeam,
                inputPhase,
                out singlePartition);
        }

        private bool TryFindNearestAiTargetSlotSpatial(
            LF2Entity self,
            AiInputContext ai,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            return aiRuntime.Input.TryFindNearestTargetSlotSpatial(
                this,
                self,
                ai,
                out selected,
                out bestDist,
                out sameZLane);
        }

        internal int FindNearestAiTargetSlotBrute(
            LF2Entity self,
            AiInputContext ai,
            out int bestDist,
            out bool sameZLane)
        {
            return aiRuntime.Input.FindNearestTargetSlotBrute(
                this,
                self,
                ai,
                out bestDist,
                out sameZLane);
        }

        private int FindNearestGroundAiTargetSlotBrute(
            LF2Entity self,
            int inputPhase,
            out int bestDist)
        {
            return aiRuntime.Input.FindNearestGroundTargetSlotBrute(
                this,
                self,
                inputPhase,
                out bestDist);
        }


        private bool IsGroundAiTargetCandidate(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase)
        {
            return aiRuntime.Input.IsGroundTargetCandidate(
                this,
                self,
                candidate,
                inputPhase);
        }

        private static bool IsGroundAiTargetCandidate(
            LF2Entity self,
            int selfSlot,
            int selfX,
            int selfTeam,
            in AiNearestSlotFacts candidate,
            int inputPhase)
        {
            return SimulationAiInputModule.IsGroundTargetCandidate(
                self,
                selfSlot,
                selfX,
                selfTeam,
                candidate,
                inputPhase);
        }

        private bool IsAirAiTargetCandidate(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase)
        {
            return aiRuntime.Input.IsAirTargetCandidate(
                this,
                self,
                candidate,
                inputPhase);
        }

        private static bool IsAirAiTargetCandidate(
            LF2Entity self,
            int selfSlot,
            int selfTeam,
            in AiNearestSlotFacts candidate,
            int inputPhase)
        {
            return SimulationAiInputModule.IsAirTargetCandidate(
                self,
                selfSlot,
                selfTeam,
                candidate,
                inputPhase);
        }

        private bool IsAirAiSpatialRole(LF2Entity candidate) => aiRuntime.Input.IsAirSpatialRole(this, candidate);

        private bool IsGroundAiSpatialRole(LF2Entity candidate) => aiRuntime.Input.IsGroundSpatialRole(this, candidate);


        private static int SaturatingAdd(int value, int delta) => SimulationAiInputModule.SaturatingAdd(value, delta);

        internal bool AiNearestSpatialMatchesBruteForSelfCheck(LF2Entity self, int inputPhase) => aiRuntime.Input.AiNearestSpatialMatchesBruteForSelfCheck(self, inputPhase);

        internal bool AiSnapshotIndexProductsMatchLegacyForSelfCheck() => aiRuntime.Input.AiSnapshotIndexProductsMatchLegacyForSelfCheck();

        internal bool AiMoveModeSnapshotMatchesFullForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out bool snapshotValid,
            out int topSlot,
            out int secondSlot,
            out int snapshotMoveMode,
            out int fullMoveMode)
        {
            return aiRuntime.Input.AiMoveModeSnapshotMatchesFullForSelfCheck(self, inputPhase, out snapshotValid, out topSlot, out secondSlot, out snapshotMoveMode, out fullMoveMode);
        }

        internal bool AiMoveModeValueMutationFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int candidateHp,
            int candidateX,
            int candidateZ,
            out bool snapshotValid,
            out int fallbackMoveMode,
            out int fullMoveMode)
        {
            return aiRuntime.Input.AiMoveModeValueMutationFallsBackForSelfCheck(self, candidate, candidateHp, candidateX, candidateZ, out snapshotValid, out fallbackMoveMode, out fullMoveMode);
        }

        internal bool AiMoveModeIdentityMutationFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            LF2Entity replacement,
            out bool snapshotValid,
            out int fallbackMoveMode,
            out int fullMoveMode)
        {
            return aiRuntime.Input.AiMoveModeIdentityMutationFallsBackForSelfCheck(self, candidate, replacement, out snapshotValid, out fallbackMoveMode, out fullMoveMode);
        }

        internal long MeasureAiMoveModeSnapshotAllocationsForSelfCheck(
            LF2Entity self,
            int iterations)
        {
            return aiRuntime.Input.MeasureAiMoveModeSnapshotAllocationsForSelfCheck(self, iterations);
        }

        internal bool AiAirRoleMutationMatchesBruteForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int candidateY)
        {
            return aiRuntime.Input.AiAirRoleMutationMatchesBruteForSelfCheck(self, candidate, inputPhase, candidateY);
        }

        internal bool AiAirRoleCountMutationForSelfCheck(
            LF2Entity candidate,
            int airState,
            int airY,
            int groundState,
            int groundY,
            out int initialCount,
            out int airCount,
            out int groundCount)
        {
            return aiRuntime.Input.AiAirRoleCountMutationForSelfCheck(candidate, airState, airY, groundState, groundY, out initialCount, out airCount, out groundCount);
        }

        internal bool AiAirNullMutationInvalidatesCountForSelfCheck() => aiRuntime.Input.AiAirNullMutationInvalidatesCountForSelfCheck();

        internal bool AiAirInvalidCoordinateInvalidatesCountForSelfCheck(
            LF2Entity candidate,
            out int count,
            out bool valid)
        {
            return aiRuntime.Input.AiAirInvalidCoordinateInvalidatesCountForSelfCheck(candidate, out count, out valid);
        }

        internal bool AiAirFastPathMatchesOracleForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool invalidateAirSnapshot,
            out int snapshotCount,
            out bool snapshotValid,
            out int fastAirPassCount,
            out int oracleAirPassCount)
        {
            return aiRuntime.Input.AiAirFastPathMatchesOracleForSelfCheck(self, inputPhase, invalidateAirSnapshot, out snapshotCount, out snapshotValid, out fastAirPassCount, out oracleAirPassCount);
        }

        internal int AiAirExecutionModePassCountForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceFull,
            bool forceLegacy,
            bool shadow)
        {
            return aiRuntime.Input.AiAirExecutionModePassCountForSelfCheck(self, inputPhase, forceFull, forceLegacy, shadow);
        }

        internal long MeasureAiAirZeroFastPathAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            return aiRuntime.Input.MeasureAiAirZeroFastPathAllocationsForSelfCheck(self, inputPhase, iterations);
        }

        internal bool AiGroundNearestMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out int groundVisitedRecords,
            out int allVisitedRecords,
            out int groundIndexedCount,
            out int selectedSlot,
            out int selectedDistance)
        {
            return aiRuntime.Input.AiGroundNearestMatchesBruteForSelfCheck(self, inputPhase, out groundVisitedRecords, out allVisitedRecords, out groundIndexedCount, out selectedSlot, out selectedDistance);
        }

        internal bool AiGroundTeamPartitionMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out int allowedPartitionCount,
            out bool partitionHandled,
            out int partitionVisitedRecords,
            out int groundVisitedRecords,
            out int selectedSlot,
            out int selectedDistance)
        {
            return aiRuntime.Input.AiGroundTeamPartitionMatchesBruteForSelfCheck(self, inputPhase, out allowedPartitionCount, out partitionHandled, out partitionVisitedRecords, out groundVisitedRecords, out selectedSlot, out selectedDistance);
        }

        internal bool AiGroundTeamPartitionMutationFallbackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int candidateTeam,
            int candidateX)
        {
            return aiRuntime.Input.AiGroundTeamPartitionMutationFallbackForSelfCheck(self, candidate, inputPhase, candidateTeam, candidateX);
        }

        internal bool AiGroundTeamPartitionFaultFallbackForSelfCheck(
            LF2Entity self,
            int inputPhase)
        {
            return aiRuntime.Input.AiGroundTeamPartitionFaultFallbackForSelfCheck(self, inputPhase);
        }

        internal long MeasureAiGroundTeamPartitionAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            return aiRuntime.Input.MeasureAiGroundTeamPartitionAllocationsForSelfCheck(self, inputPhase, iterations);
        }

        internal bool AiGroundRoleMutationMatchesBruteForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int candidateX,
            int candidateY,
            int candidateZ,
            int candidateState,
            out int fullRebuildDelta,
            out int inPlaceUpdateDelta,
            out int migrationDelta)
        {
            return aiRuntime.Input.AiGroundRoleMutationMatchesBruteForSelfCheck(self, candidate, inputPhase, candidateX, candidateY, candidateZ, candidateState, out fullRebuildDelta, out inPlaceUpdateDelta, out migrationDelta);
        }

        internal int RunAiGroundNearestQueriesForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            return aiRuntime.Input.RunAiGroundNearestQueriesForSelfCheck(self, inputPhase, iterations);
        }

        internal void CaptureAiNearestFactsTargetForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceLiveFacts,
            bool forceBrute,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            aiRuntime.Input.CaptureAiNearestFactsTargetForSelfCheck(self, inputPhase, forceLiveFacts, forceBrute, out selected, out bestDist, out sameZLane);
        }

        internal bool AiNearestSnapshotStampRejectsMutationForSelfCheck(
            int mutationKind)
        {
            return aiRuntime.Input.AiNearestSnapshotStampRejectsMutationForSelfCheck(mutationKind);
        }

        internal bool AiNearestFactsValidationFallbackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            int invalidationKind,
            out bool fastAborted)
        {
            return aiRuntime.Input.AiNearestFactsValidationFallbackForSelfCheck(self, candidate, inputPhase, invalidationKind, out fastAborted);
        }

        internal bool AiNearestOccupancyMutationFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity transientEntity,
            int transientSlot,
            int inputPhase,
            bool releaseBeforeQuery,
            out bool epochChanged,
            out bool fastAborted)
        {
            return aiRuntime.Input.AiNearestOccupancyMutationFallsBackForSelfCheck(self, transientEntity, transientSlot, inputPhase, releaseBeforeQuery, out epochChanged, out fastAborted);
        }

        internal bool AiNearestOccupancyReuseFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            LF2Entity replacement,
            int inputPhase,
            out bool generationChanged,
            out bool fastAborted)
        {
            return aiRuntime.Input.AiNearestOccupancyReuseFallsBackForSelfCheck(self, candidate, replacement, inputPhase, out generationChanged, out fastAborted);
        }

        internal bool AiNearestGenerationMismatchFallsBackForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int inputPhase,
            out bool fastAborted)
        {
            return aiRuntime.Input.AiNearestGenerationMismatchFallsBackForSelfCheck(self, candidate, inputPhase, out fastAborted);
        }

        internal bool AiGroundFailClosedFallbackMatchesBruteForSelfCheck(
            LF2Entity self,
            int inputPhase)
        {
            return aiRuntime.Input.AiGroundFailClosedFallbackMatchesBruteForSelfCheck(self, inputPhase);
        }

        internal bool AiSpecialScanSlotsMatchForSelfCheck(IReadOnlyList<int> expectedSlots) => aiRuntime.Input.AiSpecialScanSlotsMatchForSelfCheck(expectedSlots);

        internal bool AiPhase1TargetSlotsMatchForSelfCheck(IReadOnlyList<int> expectedSlots) => aiRuntime.Input.AiPhase1TargetSlotsMatchForSelfCheck(expectedSlots);

        internal bool AiPhase1TeamMutationMatchesBruteForSelfCheck(
            LF2Entity self,
            LF2Entity candidate,
            int candidateTeam,
            out bool phase1ListValid,
            out int selectedSlot)
        {
            return aiRuntime.Input.AiPhase1TeamMutationMatchesBruteForSelfCheck(self, candidate, candidateTeam, out phase1ListValid, out selectedSlot);
        }

        internal void CaptureAiSameTeamDecisionForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceFullScan,
            out bool evaluated,
            out bool usedSummary,
            out int otherCount,
            out int otherMinHp,
            out bool force7AGround,
            out bool guard7A)
        {
            aiRuntime.Input.CaptureAiSameTeamDecisionForSelfCheck(self, inputPhase, forceFullScan, out evaluated, out usedSummary, out otherCount, out otherMinHp, out force7AGround, out guard7A);
        }

        internal void CaptureAiNearestTargetForSelfCheck(
            LF2Entity self,
            int inputPhase,
            bool forceFullPhase1Scan,
            out int selected,
            out int bestDist,
            out bool sameZLane)
        {
            aiRuntime.Input.CaptureAiNearestTargetForSelfCheck(self, inputPhase, forceFullPhase1Scan, out selected, out bestDist, out sameZLane);
        }

        internal string CaptureAiSpecialScanSlotsForSelfCheck() => aiRuntime.Input.CaptureAiSpecialScanSlotsForSelfCheck();
        internal void AiUpdateMoveModeScan(
            LF2Entity self,
            ref AiInputContext ai)
        {
            aiRuntime.Input.UpdateMoveModeScan(
                this,
                aiSensingMode,
                aiSoASensingRows,
                aiSoADecisionRemainderUseRowsForCurrentInput,
                self,
                ref ai);
        }





        // AI sensing compatibility façade pending full Sensing module extraction.
        private const int AiSoASpecialProximity = 1 << 0;
        private const int AiSoASpecialLeft = 1 << 1;
        private const int AiSoASpecialRight = 1 << 2;
        private const int AiSoASpecialUp = 1 << 3;
        private const int AiSoASpecialDown = 1 << 4;
        private const int AiSoASpecialGuard7A = 1 << 5;
        private const int AiSoASpecialGuard7B = 1 << 6;
        private const int AiSoASpecialForce7AGround = 1 << 7;
        private const int AiSoASpecialC8ThreatSeen = 1 << 8;
        private const int AiSoASpecialPostSelectionSeen = 1 << 9;

        private AiSensingMode aiSensingMode { get => aiRuntime.Sensing.Mode; set => aiRuntime.Sensing.Mode = value; }
        private AiSoASensingRows aiSoASensingRows { get => aiRuntime.Sensing.Rows; set => aiRuntime.Sensing.Rows = value; }
        private ulong aiSoASensingSnapshotEpoch { get => aiRuntime.Sensing.SnapshotEpoch; set => aiRuntime.Sensing.SnapshotEpoch = value; }
        private bool aiSoASensingSnapshotValid { get => aiRuntime.Sensing.SnapshotValid; set => aiRuntime.Sensing.SnapshotValid = value; }
        private bool aiSoASensingPassInvalidated { get => aiRuntime.Sensing.PassInvalidated; set => aiRuntime.Sensing.PassInvalidated = value; }
        private ref AiSoASensingResult aiSoASensingExpected => ref aiRuntime.Sensing.Expected;
        private BattleAiExecutionProfile aiExecutionProfile { get => aiRuntime.Sensing.ExecutionProfile; set => aiRuntime.Sensing.ExecutionProfile = value; }
        private AiDecisionOwnedInputMode aiDecisionOwnedInputMode { get => aiRuntime.Sensing.DecisionOwnedInputMode; set => aiRuntime.Sensing.DecisionOwnedInputMode = value; }
        private bool aiSoACandidateExecutionEnabled { get => aiRuntime.Sensing.CandidateExecutionEnabled; set => aiRuntime.Sensing.CandidateExecutionEnabled = value; }
        private bool aiSoACandidatePassLatchedToLegacy { get => aiRuntime.Sensing.CandidatePassLatchedToLegacy; set => aiRuntime.Sensing.CandidatePassLatchedToLegacy = value; }
        private bool aiSoACandidateForceNearestFailureForSelfCheck { get => aiRuntime.Sensing.CandidateForceNearestFailure; set => aiRuntime.Sensing.CandidateForceNearestFailure = value; }
        private bool aiSoACandidateForceSpecialFailureForSelfCheck { get => aiRuntime.Sensing.CandidateForceSpecialFailure; set => aiRuntime.Sensing.CandidateForceSpecialFailure = value; }
        private bool aiSoADecisionRemainderEnabledForSelfCheck { get => aiRuntime.Decision.DecisionRemainderEnabled; set => aiRuntime.Decision.DecisionRemainderEnabled = value; }
        private bool aiSoADecisionRemainderUseRowsForCurrentInput { get => aiRuntime.Decision.DecisionRemainderUseRowsForCurrentInput; set => aiRuntime.Decision.DecisionRemainderUseRowsForCurrentInput = value; }
        private bool aiSoADecisionRemainderAttemptedForCurrentInput { get => aiRuntime.Decision.DecisionRemainderAttemptedForCurrentInput; set => aiRuntime.Decision.DecisionRemainderAttemptedForCurrentInput = value; }
        private bool aiSoADecisionRemainderRandomBoundaryPassed { get => aiRuntime.Decision.DecisionRemainderRandomBoundaryPassed; set => aiRuntime.Decision.DecisionRemainderRandomBoundaryPassed = value; }
        private bool aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck { get => aiRuntime.Decision.DecisionRemainderForceBeforeRandomFailure; set => aiRuntime.Decision.DecisionRemainderForceBeforeRandomFailure = value; }
        private bool aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck { get => aiRuntime.Decision.DecisionRemainderForceAfterRandomFailure; set => aiRuntime.Decision.DecisionRemainderForceAfterRandomFailure = value; }
        private int aiSoADecisionRemainderMutationKindForSelfCheck { get => aiRuntime.Decision.DecisionRemainderMutationKind; set => aiRuntime.Decision.DecisionRemainderMutationKind = value; }
        private bool aiSoADecisionRemainderMutationAfterRandomForSelfCheck { get => aiRuntime.Decision.DecisionRemainderMutationAfterRandom; set => aiRuntime.Decision.DecisionRemainderMutationAfterRandom = value; }
        private bool aiSoADecisionRemainderHardFailureRecordedForCurrentInput { get => aiRuntime.Decision.DecisionRemainderHardFailureRecorded; set => aiRuntime.Decision.DecisionRemainderHardFailureRecorded = value; }
        private ref AiDecisionRowContext aiSoADecisionRowContext => ref aiRuntime.Decision.DecisionRowContext;

        public AiSensingMode AiSensingMode
        {
            get => aiSensingMode;
            set
            {
                if (_ticking)
                {
                    throw new InvalidOperationException(
                        "AI sensing mode cannot be changed while a simulation pass is running.");
                }
                if (aiUnifiedSnapshotExecutionMode ==
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority must be disabled before changing AI sensing mode.");
                }

                switch (value)
                {
                    case AiSensingMode.LegacyAiSensing:
                    case AiSensingMode.SoAShadowAiSensing:
                        aiSensingMode = value;
                        return;
                    case AiSensingMode.SoAAiSensing:
                        throw new NotSupportedException(
                            "SoAAiSensing is unavailable in AI sensing shadow v1.");
                    default:
                        throw new ArgumentOutOfRangeException(nameof(value));
                }
            }
        }

        public BattleAiExecutionProfile AiExecutionProfile => aiExecutionProfile;
        public AiDecisionOwnedInputMode AiDecisionOwnedInputModeForDiagnostics => aiDecisionOwnedInputMode;

        public void ConfigureAiDecisionOwnedInputModeForDiagnostics(
            AiDecisionOwnedInputMode mode)
        {
            if (_ticking || ObjectCount != 0 || ClaimedRuntimeSlotCountForServices != 0)
            {
                throw new InvalidOperationException(
                    "The AI owned-input mode must be configured before entities are registered.");
            }
            if (mode != AiDecisionOwnedInputMode.SnapshotCopy &&
                mode != AiDecisionOwnedInputMode.CanonicalStoreDirect)
            {
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            aiDecisionOwnedInputMode = mode;
        }

        public void ConfigureAiExecutionProfile(BattleAiExecutionProfile profile)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "The battle AI execution profile cannot change while a simulation pass is running.");
            }
            if (ObjectCount != 0 || ClaimedRuntimeSlotCountForServices != 0)
            {
                throw new InvalidOperationException(
                    "The battle AI execution profile must be configured before entities are registered.");
            }
            if (profile != BattleAiExecutionProfile.LegacyCanonical &&
                profile != BattleAiExecutionProfile.DataOrientedCanonical)
            {
                throw new ArgumentOutOfRangeException(nameof(profile), profile, null);
            }

            // Leave authority first. This is the only ordering that can never expose a
            // partially configured authority pass to the existing property guards.
            if (aiUnifiedSnapshotExecutionMode !=
                AiUnifiedSnapshotExecutionMode.LegacySeparate)
            {
                AiUnifiedSnapshotExecutionMode =
                    AiUnifiedSnapshotExecutionMode.LegacySeparate;
            }

            AiDecisionShadowMode = AiDecisionShadowMode.Disabled;
            AiUnifiedSnapshotShadowMode = AiUnifiedSnapshotShadowMode.Disabled;
            AiDecisionIndexedCanonicalFullOracleSampleInterval = 0;

            switch (profile)
            {
                case BattleAiExecutionProfile.LegacyCanonical:
                    SetAiSoACandidateExecutionEnabled(false);
                    AiDecisionExecutionMode = AiDecisionExecutionMode.Legacy;
                    break;
                case BattleAiExecutionProfile.DataOrientedCanonical:
                    SetAiSoACandidateExecutionEnabled(true);
                    AiDecisionExecutionMode = AiDecisionExecutionMode.IndexedCanonical;
                    AiUnifiedSnapshotExecutionMode =
                        AiUnifiedSnapshotExecutionMode.UnifiedAuthority;
                    break;
            }

            aiExecutionProfile = profile;
            EnsureAiExecutionProfileCoherent();
        }

        private void EnsureAiExecutionProfileCoherent()
        {
            bool coherent = aiExecutionProfile == BattleAiExecutionProfile.LegacyCanonical
                ? aiSensingMode == AiSensingMode.LegacyAiSensing &&
                  aiDecisionExecutionMode == AiDecisionExecutionMode.Legacy &&
                  aiUnifiedSnapshotExecutionMode ==
                  AiUnifiedSnapshotExecutionMode.LegacySeparate
                : aiSensingMode == AiSensingMode.SoAAiSensing &&
                  aiDecisionExecutionMode == AiDecisionExecutionMode.IndexedCanonical &&
                  aiUnifiedSnapshotExecutionMode ==
                  AiUnifiedSnapshotExecutionMode.UnifiedAuthority;
            if (!coherent)
            {
                throw new InvalidOperationException(
                    "The battle AI execution profile did not produce a coherent sensing/decision/authority configuration.");
            }
        }

        public int AiSoASensingShadowQueryCountForDiagnostics { get => aiRuntime.Sensing.ShadowQueryCount; private set => aiRuntime.Sensing.ShadowQueryCount = value; }
        public int AiSoASensingShadowInvalidationCountForDiagnostics { get => aiRuntime.Sensing.ShadowInvalidationCount; private set => aiRuntime.Sensing.ShadowInvalidationCount = value; }
        public int AiSoASensingShadowPurityMismatchCountForDiagnostics { get => aiRuntime.Sensing.ShadowPurityMismatchCount; private set => aiRuntime.Sensing.ShadowPurityMismatchCount = value; }
        public int AiSoASensingShadowInitialMismatchCountForDiagnostics { get => aiRuntime.Sensing.ShadowInitialMismatchCount; private set => aiRuntime.Sensing.ShadowInitialMismatchCount = value; }
        public int AiSoASensingShadowCachedMismatchCountForDiagnostics { get => aiRuntime.Sensing.ShadowCachedMismatchCount; private set => aiRuntime.Sensing.ShadowCachedMismatchCount = value; }
        public int AiSoASensingShadowPostSpecialMismatchCountForDiagnostics { get => aiRuntime.Sensing.ShadowPostSpecialMismatchCount; private set => aiRuntime.Sensing.ShadowPostSpecialMismatchCount = value; }
        public int AiSoASensingShadowMismatchMaskForDiagnostics { get => aiRuntime.Sensing.ShadowMismatchMask; private set => aiRuntime.Sensing.ShadowMismatchMask = value; }
        public int AiSoASensingShadowLastMismatchMaskForDiagnostics { get => aiRuntime.Sensing.ShadowLastMismatchMask; private set => aiRuntime.Sensing.ShadowLastMismatchMask = value; }
        public bool AiSoASensingShadowComparisonPublishedForDiagnostics { get => aiRuntime.Sensing.ShadowComparisonPublished; private set => aiRuntime.Sensing.ShadowComparisonPublished = value; }
        public AiSoASensingShadowMismatch AiSoASensingShadowFirstMismatchForDiagnostics { get => aiRuntime.Sensing.ShadowFirstMismatch; private set => aiRuntime.Sensing.ShadowFirstMismatch = value; }
        public int AiSoACandidateNearestQueryCountForDiagnostics { get => aiRuntime.Sensing.CandidateNearestQueryCount; private set => aiRuntime.Sensing.CandidateNearestQueryCount = value; }
        public int AiSoACandidateSpecialQueryCountForDiagnostics { get => aiRuntime.Sensing.CandidateSpecialQueryCount; private set => aiRuntime.Sensing.CandidateSpecialQueryCount = value; }
        public int AiSoACandidateEmptySpecialFastPathCountForDiagnostics { get => aiRuntime.Sensing.CandidateEmptySpecialFastPathCount; private set => aiRuntime.Sensing.CandidateEmptySpecialFastPathCount = value; }
        public long AiSoACandidateGroundXRowVisitCountForDiagnostics { get => aiRuntime.Sensing.CandidateGroundXRowVisitCount; private set => aiRuntime.Sensing.CandidateGroundXRowVisitCount = value; }
        public long AiSoACandidateAirXRowVisitCountForDiagnostics { get => aiRuntime.Sensing.CandidateAirXRowVisitCount; private set => aiRuntime.Sensing.CandidateAirXRowVisitCount = value; }
        public long AiSoACandidateSpecialSlotVisitCountForDiagnostics { get => aiRuntime.Sensing.CandidateSpecialSlotVisitCount; private set => aiRuntime.Sensing.CandidateSpecialSlotVisitCount = value; }
        public int AiSoACandidateLegacyNearestScanCountForDiagnostics { get => aiRuntime.Sensing.CandidateLegacyNearestScanCount; private set => aiRuntime.Sensing.CandidateLegacyNearestScanCount = value; }
        public int AiSoACandidateLegacySpecialScanCountForDiagnostics { get => aiRuntime.Sensing.CandidateLegacySpecialScanCount; private set => aiRuntime.Sensing.CandidateLegacySpecialScanCount = value; }
        public int AiSoACandidatePreRandomFailureCountForDiagnostics { get => aiRuntime.Sensing.CandidatePreRandomFailureCount; private set => aiRuntime.Sensing.CandidatePreRandomFailureCount = value; }
        public int AiSoACandidatePostRandomFailureCountForDiagnostics { get => aiRuntime.Sensing.CandidatePostRandomFailureCount; private set => aiRuntime.Sensing.CandidatePostRandomFailureCount = value; }
        public int AiSoACandidateFusedSnapshotBuildCountForDiagnostics { get => aiRuntime.Sensing.CandidateFusedSnapshotBuildCount; private set => aiRuntime.Sensing.CandidateFusedSnapshotBuildCount = value; }
        public long AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics { get => aiRuntime.Sensing.CandidateFusedSnapshotSlotVisitCount; private set => aiRuntime.Sensing.CandidateFusedSnapshotSlotVisitCount = value; }
        public int AiSoACandidateFusedSnapshotFailureCountForDiagnostics { get => aiRuntime.Sensing.CandidateFusedSnapshotFailureCount; private set => aiRuntime.Sensing.CandidateFusedSnapshotFailureCount = value; }
        public long AiSoACandidateSnapshotRefreshCountForDiagnostics { get => aiRuntime.Sensing.CandidateSnapshotRefreshCount; private set => aiRuntime.Sensing.CandidateSnapshotRefreshCount = value; }
        public int AiLegacyNearestFactsBuildCountForDiagnostics { get => aiRuntime.Input.LegacyNearestFactsBuildCount; private set => aiRuntime.Input.LegacyNearestFactsBuildCount = value; }
        public int AiLegacySnapshotIndexBuildCountForDiagnostics { get => aiRuntime.Input.LegacySnapshotIndexBuildCount; private set => aiRuntime.Input.LegacySnapshotIndexBuildCount = value; }
        public int AiLegacyQuadtreeSyncCountForDiagnostics { get => aiRuntime.Input.LegacyQuadtreeSyncCount; private set => aiRuntime.Input.LegacyQuadtreeSyncCount = value; }
        public int AiLegacySnapshotMutationCountForDiagnostics { get => aiRuntime.Input.LegacySnapshotMutationCount; private set => aiRuntime.Input.LegacySnapshotMutationCount = value; }
        public int AiSoADecisionRemainderEligibleAttemptCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderEligibleAttemptCount; private set => aiRuntime.Decision.DecisionRemainderEligibleAttemptCount = value; }
        public int AiSoADecisionRemainderAppliedCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderAppliedCount; private set => aiRuntime.Decision.DecisionRemainderAppliedCount = value; }
        public int AiSoADecisionRemainderFallbackCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderFallbackCount; private set => aiRuntime.Decision.DecisionRemainderFallbackCount = value; }
        public int AiSoADecisionRemainderPreRandomFailureCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderPreRandomFailureCount; private set => aiRuntime.Decision.DecisionRemainderPreRandomFailureCount = value; }
        public int AiSoADecisionRemainderPostRandomFailureCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderPostRandomFailureCount; private set => aiRuntime.Decision.DecisionRemainderPostRandomFailureCount = value; }
        public int AiSoADecisionRemainderHardFailureCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderHardFailureCount; private set => aiRuntime.Decision.DecisionRemainderHardFailureCount = value; }
        public int AiSoADecisionRemainderContextBindCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderContextBindCount; private set => aiRuntime.Decision.DecisionRemainderContextBindCount = value; }
        public int AiSoADecisionRemainderGatewayValidationCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderGatewayValidationCount; private set => aiRuntime.Decision.DecisionRemainderGatewayValidationCount = value; }
        public long AiSoADecisionRemainderRowVisitCountForDiagnostics { get => aiRuntime.Decision.DecisionRemainderRowVisitCount; private set => aiRuntime.Decision.DecisionRemainderRowVisitCount = value; }
        public bool AiSoADecisionRemainderEnabledForDiagnostics => aiSoADecisionRemainderEnabledForSelfCheck;

        public void ResetAiSoACandidateDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "AI sensing diagnostics cannot be reset while a simulation pass is running.");
            }

            AiSoACandidateNearestQueryCountForDiagnostics = 0;
            AiSoACandidateSpecialQueryCountForDiagnostics = 0;
            AiSoACandidateEmptySpecialFastPathCountForDiagnostics = 0;
            AiSoACandidateGroundXRowVisitCountForDiagnostics = 0;
            AiSoACandidateAirXRowVisitCountForDiagnostics = 0;
            AiSoACandidateSpecialSlotVisitCountForDiagnostics = 0;
            AiSoACandidateLegacyNearestScanCountForDiagnostics = 0;
            AiSoACandidateLegacySpecialScanCountForDiagnostics = 0;
            AiSoACandidatePreRandomFailureCountForDiagnostics = 0;
            AiSoACandidatePostRandomFailureCountForDiagnostics = 0;
            AiSoACandidateFusedSnapshotBuildCountForDiagnostics = 0;
            AiSoACandidateFusedSnapshotSlotVisitCountForDiagnostics = 0;
            AiSoACandidateFusedSnapshotFailureCountForDiagnostics = 0;
            AiSoACandidateSnapshotRefreshCountForDiagnostics = 0;
            AiLegacyNearestFactsBuildCountForDiagnostics = 0;
            AiLegacySnapshotIndexBuildCountForDiagnostics = 0;
            AiLegacyQuadtreeSyncCountForDiagnostics = 0;
            AiLegacySnapshotMutationCountForDiagnostics = 0;
            AiSoADecisionRemainderEligibleAttemptCountForDiagnostics = 0;
            AiSoADecisionRemainderAppliedCountForDiagnostics = 0;
            AiSoADecisionRemainderFallbackCountForDiagnostics = 0;
            AiSoADecisionRemainderPreRandomFailureCountForDiagnostics = 0;
            AiSoADecisionRemainderPostRandomFailureCountForDiagnostics = 0;
            AiSoADecisionRemainderHardFailureCountForDiagnostics = 0;
            AiSoADecisionRemainderContextBindCountForDiagnostics = 0;
            AiSoADecisionRemainderGatewayValidationCountForDiagnostics = 0;
            AiSoADecisionRemainderRowVisitCountForDiagnostics = 0;
        }

        internal void SetAiSoACandidateModeForSelfCheck(bool enabled)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            if (!enabled &&
                aiUnifiedSnapshotExecutionMode ==
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                throw new InvalidOperationException(
                    "Unified AI snapshot authority must be disabled before SoAAiSensing.");
            }
            SetAiSoACandidateExecutionEnabled(enabled);
        }

        private void SetAiSoACandidateExecutionEnabled(bool enabled)
        {
            aiSoACandidateExecutionEnabled = enabled;
            aiSensingMode = enabled
                ? AiSensingMode.SoAAiSensing
                : AiSensingMode.LegacyAiSensing;
            aiSoACandidatePassLatchedToLegacy = false;
            if (!enabled)
            {
                aiSoACandidateForceNearestFailureForSelfCheck = false;
                aiSoACandidateForceSpecialFailureForSelfCheck = false;
                aiSoADecisionRemainderEnabledForSelfCheck = false;
                aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderMutationKindForSelfCheck = 0;
                aiSoADecisionRemainderMutationAfterRandomForSelfCheck = false;
            }
        }

        internal void SetAiSoADecisionRemainderModeForSelfCheck(bool enabled)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            if (enabled && aiSensingMode != AiSensingMode.SoAAiSensing)
            {
                throw new InvalidOperationException(
                    "AI SoA decision remainder requires SoAAiSensing authority.");
            }

            aiSoADecisionRemainderEnabledForSelfCheck = enabled;
            aiSoADecisionRemainderUseRowsForCurrentInput = false;
            aiSoADecisionRemainderAttemptedForCurrentInput = false;
            aiSoADecisionRemainderRandomBoundaryPassed = false;
            aiSoADecisionRemainderHardFailureRecordedForCurrentInput = false;
            aiSoADecisionRowContext = default;
            if (!enabled)
            {
                aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck = false;
                aiSoADecisionRemainderMutationKindForSelfCheck = 0;
                aiSoADecisionRemainderMutationAfterRandomForSelfCheck = false;
            }
        }

        internal void SetAiSoADecisionRemainderFailureForSelfCheck(
            bool failBeforeRandom,
            bool failAfterRandom)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            aiSoADecisionRemainderForceBeforeRandomFailureForSelfCheck =
                failBeforeRandom;
            aiSoADecisionRemainderForceAfterRandomFailureForSelfCheck =
                failAfterRandom;
        }

        internal void SetAiSoADecisionRemainderMutationForSelfCheck(
            int mutationKind,
            bool afterRandom)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            if (mutationKind < 0 || mutationKind > 4)
                throw new ArgumentOutOfRangeException(nameof(mutationKind));

            aiSoADecisionRemainderMutationKindForSelfCheck = mutationKind;
            aiSoADecisionRemainderMutationAfterRandomForSelfCheck = afterRandom;
        }

        internal void SetAiSoACandidateFailureForSelfCheck(
            bool failNearest,
            bool failSpecial)
        {
            EnsureAiSoASensingSelfCheckCanRun();
            aiSoACandidateForceNearestFailureForSelfCheck = failNearest;
            aiSoACandidateForceSpecialFailureForSelfCheck = failSpecial;
        }

        public void ResetAiSoASensingShadowDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "AI sensing diagnostics cannot be reset while a simulation pass is running.");
            }

            AiSoASensingShadowQueryCountForDiagnostics = 0;
            AiSoASensingShadowInvalidationCountForDiagnostics = 0;
            AiSoASensingShadowPurityMismatchCountForDiagnostics = 0;
            AiSoASensingShadowInitialMismatchCountForDiagnostics = 0;
            AiSoASensingShadowCachedMismatchCountForDiagnostics = 0;
            AiSoASensingShadowPostSpecialMismatchCountForDiagnostics = 0;
            AiSoASensingShadowMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowLastMismatchMaskForDiagnostics = 0;
            AiSoASensingShadowComparisonPublishedForDiagnostics = false;
            AiSoASensingShadowFirstMismatchForDiagnostics = default;
        }


        private void InitializeAiSoASensingRows(int capacity) => aiRuntime.Sensing.InitializeRows(capacity);

        private void GrowAiSoASensingRows(int capacity) => aiRuntime.Sensing.GrowRows(capacity);

        internal void EnsureAiSensingModeAvailableBeforeTick()
        {
            if (aiUnifiedSnapshotExecutionMode ==
                AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
            {
                if (aiSensingMode != AiSensingMode.SoAAiSensing ||
                    aiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical ||
                    aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Disabled)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority requires SoAAiSensing, IndexedCanonical, and disabled unified shadow.");
                }
            }
            if (aiSensingMode == AiSensingMode.SoAAiSensing)
            {
                if (aiSoACandidateExecutionEnabled)
                    return;

                throw new NotSupportedException(
                    "SoAAiSensing requires the data-oriented production profile or an internal diagnostic test gate.");
            }

            if (aiSensingMode != AiSensingMode.LegacyAiSensing &&
                aiSensingMode != AiSensingMode.SoAShadowAiSensing)
            {
                throw new InvalidOperationException("Unknown AI sensing mode.");
            }
        }

        private void CaptureAiSoASensingShadowSnapshot(ulong expectedEpoch)
        {
            if (!aiRuntime.Sensing.TryBuildShadowSnapshot(
                    aiInputSlots,
                    expectedEpoch))
            {
                return;
            }

            BeginAiUnifiedSnapshotProductionMutationWitnessPass(
                AiUnifiedSnapshotConsumer.SoASensing,
                aiSoASensingSnapshotEpoch);
            aiRuntime.Sensing.CompleteSnapshotCapture();
        }

        private bool CaptureAiSoACandidateFusedSnapshot(
            int expectedCapacity,
            ulong expectedEpoch)
        {
            if (!aiRuntime.Sensing.TryBuildCandidateFusedSnapshot(
                    this,
                    aiRuntime.Input,
                    expectedCapacity,
                    expectedEpoch))
            {
                return false;
            }

            BeginAiUnifiedSnapshotProductionMutationWitnessPass(
                AiUnifiedSnapshotConsumer.SoASensing,
                aiSoASensingSnapshotEpoch);
            aiRuntime.Sensing.CompleteSnapshotCapture();
            return true;
        }




        private bool TryCaptureAiSoASensingRow(
            LF2Entity entity,
            int slot,
            uint generation,
            bool captureSpecialMembership)
        {
            return SimulationAiSensingModule.TryCaptureRow(
                aiSoASensingRows,
                entity,
                slot,
                generation,
                captureSpecialMembership);
        }

        private static bool TryCaptureAiSoASensingRow(
            AiSoASensingRows rows,
            LF2Entity entity,
            int slot,
            uint generation,
            bool captureSpecialMembership,
            bool useFreshRuntimeIdentity = false)
        {
            return SimulationAiSensingModule.TryCaptureRow(
                rows,
                entity,
                slot,
                generation,
                captureSpecialMembership,
                useFreshRuntimeIdentity);
        }



        private void ObserveAiSoASensingSnapshotBuildEpoch(
            ulong expectedEpoch,
            ulong observedEpoch)
        {
            aiRuntime.Sensing.ObserveSnapshotBuildEpoch(expectedEpoch, observedEpoch);
        }

        private void ClearAiSoASensingShadowSnapshot() => aiRuntime.Sensing.ClearShadowSnapshot();


        private bool ValidateAiSoASensingShadowSnapshot() => aiRuntime.Sensing.ValidateShadowSnapshot();

        private void RefreshAiSoASensingShadowRowAfterCharacterInput(LF2Entity entity)
        {
            if (!aiRuntime.Sensing.TryRefreshRowAfterCharacterInput(
                    entity,
                    out SimulationAiSensingModule.RowRefreshResult result))
            {
                return;
            }

            RecordAiUnifiedSnapshotProductionMutationWitness(
                AiUnifiedSnapshotConsumer.SoASensing,
                aiSoASensingSnapshotEpoch,
                result.Slot,
                result.Generation,
                result.Identity,
                result.RoleRebuilt,
                result.TeamRebuilt,
                result.PreviousX,
                result.CurrentX,
                result.PreviousTeam,
                result.CurrentTeam,
                PackAiUnifiedSnapshotRoleFlags(
                    result.WasGroundRole,
                    result.WasAirRole),
                PackAiUnifiedSnapshotRoleFlags(
                    result.IsGroundRole,
                    result.IsAirRole),
                result.WasLivingCharacter,
                result.IsLivingCharacter,
                result.PreviousHp,
                result.CurrentHp);
        }

        internal void BeginAiSoASensingShadowComparison(
            LF2Entity self,
            int tickIndex)
        {
            aiRuntime.Sensing.BeginShadowComparison(this, self, tickIndex);
        }
        internal void ContinueAiSoASensingShadowComparisonAfterCache(
            LF2Entity self,
            int tickIndex,
            bool cachedTargetEligible,
            bool cacheRandomCalled,
            int cacheRoll,
            uint rngStateBefore,
            ulong rngCallsBefore,
            uint rngStateAfter,
            ulong rngCallsAfter,
            int selectedSlot)
        {
            aiRuntime.Sensing.ContinueComparisonAfterCache(
                self,
                tickIndex,
                InputPhase,
                ForceFullAiSpecialScanForDiagnostics,
                cachedTargetEligible,
                cacheRandomCalled,
                cacheRoll,
                rngStateBefore,
                rngCallsBefore,
                rngStateAfter,
                rngCallsAfter,
                selectedSlot);
        }

        internal void CompareAiSoASensingInitial(
            LF2Entity self,
            int tickIndex,
            int selectedSlot,
            int bestDist,
            bool sameZLane)
        {
            aiRuntime.Sensing.CompareInitial(
                self,
                tickIndex,
                selectedSlot,
                bestDist,
                sameZLane);
        }

        internal void CompareAiSoASensingPostSpecial(
            LF2Entity self,
            int tickIndex,
            int selectedSlot,
            int specialBestDist,
            bool specialObjectProximity,
            bool specialLeft,
            bool specialRight,
            bool specialUp,
            bool specialDown,
            bool specialGuard7A,
            bool specialGuard7B,
            bool specialForce7AGround,
            bool specialC8ThreatSeen,
            bool specialPostSelectionSeen)
        {
            aiRuntime.Sensing.ComparePostSpecial(
                self,
                tickIndex,
                selectedSlot,
                specialBestDist,
                specialObjectProximity,
                specialLeft,
                specialRight,
                specialUp,
                specialDown,
                specialGuard7A,
                specialGuard7B,
                specialForce7AGround,
                specialC8ThreatSeen,
                specialPostSelectionSeen);
        }

        internal void CompleteAiSoASensingComparisonWithoutSpecial(
            LF2Entity self,
            int tickIndex)
        {
            aiRuntime.Sensing.CompleteComparisonWithoutSpecial(self, tickIndex);
        }




        private bool TryRunAiSoASensingShadowQuery(
            int selfSlot,
            int inputPhase,
            uint rngState,
            bool forceFullSpecialScan,
            out AiSoASensingResult result)
        {
            return aiRuntime.Sensing.TryRunShadowQuery(
                selfSlot,
                inputPhase,
                rngState,
                forceFullSpecialScan,
                out result);
        }




        internal bool TryRunAiSoACandidateNearest(
            LF2Entity self,
            int inputPhase,
            out AiSoANearestResult result)
        {
            return aiRuntime.Sensing.TryRunCandidateNearest(
                self,
                inputPhase,
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics,
                out result);
        }


        internal bool TryRunAiSoACandidateSpecial(
            LF2Entity self,
            int inputPhase,
            int selectedSlot,
            int nearestBestDist,
            bool sameZLane,
            out AiSoASpecialResult result)
        {
            return aiRuntime.Sensing.TryRunCandidateSpecial(
                self,
                inputPhase,
                selectedSlot,
                nearestBestDist,
                sameZLane,
                ForceFullAiSpecialScanForDiagnostics,
                ActiveBattleAiInputDetailDiagnosticsForDiagnostics,
                out result);
        }




        internal void LatchAiSoACandidateToLegacyBeforeRandom()
        {
            if (AiUnifiedSnapshotExecutionFallbackForbidden)
            {
                ThrowAiUnifiedSnapshotExecutionHardBreach(
                    AiUnifiedSnapshotExceptionStage.InitialSensingCompare,
                    "SoAAiSensing attempted pre-random fallback after unified snapshot commit.");
            }
            aiSoACandidatePassLatchedToLegacy = true;
            AiSoACandidatePreRandomFailureCountForDiagnostics++;
        }

        internal void LatchAiSoACandidateToLegacyAfterRandom()
        {
            if (AiUnifiedSnapshotExecutionFallbackForbidden)
            {
                ThrowAiUnifiedSnapshotExecutionHardBreach(
                    AiUnifiedSnapshotExceptionStage.InitialSensingCompare,
                    "SoAAiSensing attempted post-random fallback after unified snapshot commit.");
            }
            aiSoACandidatePassLatchedToLegacy = true;
            AiSoACandidatePostRandomFailureCountForDiagnostics++;
        }

        private static void GetAiSoASameTeamSummaryExcludingSelf(
            AiSoASensingRows rows,
            int selfSlot,
            int selfTeam,
            out int otherCount,
            out int otherMinHp)
        {
            SimulationAiSensingModule.GetSameTeamSummaryExcludingSelf(
                rows,
                selfSlot,
                selfTeam,
                out otherCount,
                out otherMinHp);
        }
        internal bool CaptureAiSoASensingNearestForSelfCheck(
            LF2Entity self,
            int inputPhase,
            out int selectedSlot,
            out int bestDist,
            out bool sameZLane)
        {
            return aiRuntime.Sensing.CaptureNearestForSelfCheck(
                this,
                self,
                inputPhase,
                out selectedSlot,
                out bestDist,
                out sameZLane);
        }

        internal long MeasureAiSoASensingShadowAllocationsForSelfCheck(
            LF2Entity self,
            int inputPhase,
            int iterations)
        {
            return aiRuntime.Sensing.MeasureShadowAllocationsForSelfCheck(
                this,
                self,
                inputPhase,
                iterations);
        }

        internal bool AiSoASensingEpochDriftInvalidatesForSelfCheck() => aiRuntime.Sensing.EpochDriftInvalidatesForSelfCheck(this);

        internal bool AiSoASensingGenerationDriftInvalidatesForSelfCheck() => aiRuntime.Sensing.GenerationDriftInvalidatesForSelfCheck(this);

        internal bool AiSoASensingIdentityDriftInvalidatesForSelfCheck() => aiRuntime.Sensing.IdentityDriftInvalidatesForSelfCheck(this);

        private void EnsureAiSoASensingSelfCheckCanRun()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "AI sensing self-checks cannot run during a simulation pass.");
            }
        }

        private const ulong AiDecisionRngHashOffset = 1469598103934665603UL;
        private const ulong AiDecisionRngHashPrime = 1099511628211UL;

        private AiDecisionShadowMode aiDecisionShadowMode { get => aiRuntime.Decision.ShadowMode; set => aiRuntime.Decision.ShadowMode = value; }
        private AiDecisionExecutionMode aiDecisionExecutionMode { get => aiRuntime.Decision.ExecutionMode; set => aiRuntime.Decision.ExecutionMode = value; }
        private AiUnifiedSnapshotShadowMode aiUnifiedSnapshotShadowMode { get => aiRuntime.Decision.UnifiedShadowMode; set => aiRuntime.Decision.UnifiedShadowMode = value; }
        private AiUnifiedSnapshotExecutionMode aiUnifiedSnapshotExecutionMode { get => aiRuntime.Decision.UnifiedExecutionMode; set => aiRuntime.Decision.UnifiedExecutionMode = value; }
        private int aiDecisionIndexedCanonicalFullOracleSampleInterval { get => aiRuntime.Decision.IndexedCanonicalFullOracleSampleInterval; set => aiRuntime.Decision.IndexedCanonicalFullOracleSampleInterval = value; }
        private AiDecisionSnapshot aiCharacterDecisionLegacyFallbackSnapshot { get => aiRuntime.Decision.LegacyFallbackSnapshot; set => aiRuntime.Decision.LegacyFallbackSnapshot = value; }
        private bool aiDecisionSharedPassAvailable { get => aiRuntime.Decision.SharedPassAvailable; set => aiRuntime.Decision.SharedPassAvailable = value; }
        private AiDecisionWitness aiDecisionShadowExpected { get => aiRuntime.Decision.ShadowExpected; set => aiRuntime.Decision.ShadowExpected = value; }
        private bool aiDecisionShadowComparisonActive { get => aiRuntime.Decision.ShadowComparisonActive; set => aiRuntime.Decision.ShadowComparisonActive = value; }
        private bool aiDecisionLegacyRngRecording { get => aiRuntime.Decision.LegacyRngRecording; set => aiRuntime.Decision.LegacyRngRecording = value; }
        private int aiDecisionLegacyRngCount { get => aiRuntime.Decision.LegacyRngCount; set => aiRuntime.Decision.LegacyRngCount = value; }
        private int aiDecisionLegacyCharacterDecisionPosition { get => aiRuntime.Decision.LegacyCharacterDecisionPosition; set => aiRuntime.Decision.LegacyCharacterDecisionPosition = value; }
        private Type aiDecisionShadowFirstExceptionType { get => aiRuntime.Decision.ShadowFirstExceptionType; set => aiRuntime.Decision.ShadowFirstExceptionType = value; }
        private AiSoASensingRows aiUnifiedSnapshotRows { get => aiRuntime.Decision.UnifiedSnapshotRows; set => aiRuntime.Decision.UnifiedSnapshotRows = value; }
        private int[] aiUnifiedMoveModeFirst10Hp { get => aiRuntime.Decision.UnifiedMoveModeFirst10Hp; set => aiRuntime.Decision.UnifiedMoveModeFirst10Hp = value; }
        private AiUnifiedSnapshotExecutionState aiUnifiedSnapshotPublishedState { get => aiRuntime.Decision.UnifiedSnapshotPublishedState; set => aiRuntime.Decision.UnifiedSnapshotPublishedState = value; }
        private AiUnifiedSnapshotExecutionState aiUnifiedSnapshotScratchState { get => aiRuntime.Decision.UnifiedSnapshotScratchState; set => aiRuntime.Decision.UnifiedSnapshotScratchState = value; }
        private AiUnifiedSnapshotExecutionState aiUnifiedSnapshotStandbyState { get => aiRuntime.Decision.UnifiedSnapshotStandbyState; set => aiRuntime.Decision.UnifiedSnapshotStandbyState = value; }
        private bool aiUnifiedSnapshotExecutionCommittedThisPass { get => aiRuntime.Decision.UnifiedSnapshotExecutionCommittedThisPass; set => aiRuntime.Decision.UnifiedSnapshotExecutionCommittedThisPass = value; }
        private bool aiUnifiedSnapshotExecutionConsumerStartedThisPass { get => aiRuntime.Decision.UnifiedSnapshotExecutionConsumerStartedThisPass; set => aiRuntime.Decision.UnifiedSnapshotExecutionConsumerStartedThisPass = value; }
        private bool aiUnifiedSnapshotNoPendingRefreshSkipForDiagnostics { get => aiRuntime.Decision.UnifiedSnapshotNoPendingRefreshSkip; set => aiRuntime.Decision.UnifiedSnapshotNoPendingRefreshSkip = value; }
        private ref AiUnifiedSnapshotMutationWitness aiSoASensingMutationWitness => ref aiRuntime.Decision.SoASensingMutationWitness;
        private ref AiUnifiedSnapshotMutationWitness aiDecisionMutationWitness => ref aiRuntime.Decision.DecisionMutationWitness;
        private ref AiUnifiedSnapshotMutationWitness aiUnifiedSnapshotMutationWitness => ref aiRuntime.Decision.UnifiedSnapshotMutationWitness;
        private Type aiUnifiedSnapshotFirstExceptionType { get => aiRuntime.Decision.UnifiedSnapshotFirstExceptionType; set => aiRuntime.Decision.UnifiedSnapshotFirstExceptionType = value; }
#if UNITY_INCLUDE_TESTS
        private long aiDecisionShadowBeginInvocationCountForTests { get => aiRuntime.Decision.ShadowBeginInvocationCountForTests; set => aiRuntime.Decision.ShadowBeginInvocationCountForTests = value; }
        private long aiDecisionShadowCompleteInvocationCountForTests { get => aiRuntime.Decision.ShadowCompleteInvocationCountForTests; set => aiRuntime.Decision.ShadowCompleteInvocationCountForTests = value; }
        private AiDecisionAvailability aiDecisionIndexedCanonicalPreCommitFailureForSelfCheck { get => aiRuntime.Decision.IndexedCanonicalPreCommitFailureForSelfCheck; set => aiRuntime.Decision.IndexedCanonicalPreCommitFailureForSelfCheck = value; }
        private AiUnifiedSnapshotConsumer aiUnifiedSnapshotBoundaryMutationConsumerForSelfCheck { get => aiRuntime.Decision.UnifiedBoundaryMutationConsumerForSelfCheck; set => aiRuntime.Decision.UnifiedBoundaryMutationConsumerForSelfCheck = value; }
        private int aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck { get => aiRuntime.Decision.UnifiedBoundaryMutationSlotForSelfCheck; set => aiRuntime.Decision.UnifiedBoundaryMutationSlotForSelfCheck = value; }
        private int aiUnifiedSnapshotBoundaryMutationXorForSelfCheck { get => aiRuntime.Decision.UnifiedBoundaryMutationXorForSelfCheck; set => aiRuntime.Decision.UnifiedBoundaryMutationXorForSelfCheck = value; }
        private AiUnifiedSnapshotConsumer aiUnifiedSnapshotWitnessMutationConsumerForSelfCheck { get => aiRuntime.Decision.UnifiedWitnessMutationConsumerForSelfCheck; set => aiRuntime.Decision.UnifiedWitnessMutationConsumerForSelfCheck = value; }
        private AiUnifiedSnapshotProductMutationKind aiUnifiedSnapshotProductMutationKindForSelfCheck { get => aiRuntime.Decision.UnifiedProductMutationKindForSelfCheck; set => aiRuntime.Decision.UnifiedProductMutationKindForSelfCheck = value; }
        private int aiUnifiedSnapshotProductMutationSlotForSelfCheck { get => aiRuntime.Decision.UnifiedProductMutationSlotForSelfCheck; set => aiRuntime.Decision.UnifiedProductMutationSlotForSelfCheck = value; }
        private int aiUnifiedSnapshotExecutionProbeObserverSlotAForSelfCheck { get => aiRuntime.Decision.UnifiedExecutionProbeObserverSlotAForSelfCheck; set => aiRuntime.Decision.UnifiedExecutionProbeObserverSlotAForSelfCheck = value; }
        private int aiUnifiedSnapshotExecutionProbeTargetSlotAForSelfCheck { get => aiRuntime.Decision.UnifiedExecutionProbeTargetSlotAForSelfCheck; set => aiRuntime.Decision.UnifiedExecutionProbeTargetSlotAForSelfCheck = value; }
        private int aiUnifiedSnapshotExecutionProbeStateAForSelfCheck { get => aiRuntime.Decision.UnifiedExecutionProbeStateAForSelfCheck; set => aiRuntime.Decision.UnifiedExecutionProbeStateAForSelfCheck = value; }
        private int aiUnifiedSnapshotExecutionProbeObserverSlotBForSelfCheck { get => aiRuntime.Decision.UnifiedExecutionProbeObserverSlotBForSelfCheck; set => aiRuntime.Decision.UnifiedExecutionProbeObserverSlotBForSelfCheck = value; }
        private int aiUnifiedSnapshotExecutionProbeTargetSlotBForSelfCheck { get => aiRuntime.Decision.UnifiedExecutionProbeTargetSlotBForSelfCheck; set => aiRuntime.Decision.UnifiedExecutionProbeTargetSlotBForSelfCheck = value; }
        private int aiUnifiedSnapshotExecutionProbeStateBForSelfCheck { get => aiRuntime.Decision.UnifiedExecutionProbeStateBForSelfCheck; set => aiRuntime.Decision.UnifiedExecutionProbeStateBForSelfCheck = value; }
#endif

        public AiDecisionShadowMode AiDecisionShadowMode
        {
            get => aiDecisionShadowMode;
            set
            {
                if (_ticking)
                    throw new InvalidOperationException(
                        "AI decision shadow mode cannot change while a simulation pass is running.");
                if (value != AiDecisionShadowMode.Disabled &&
                    value != AiDecisionShadowMode.Shadow &&
                    value != AiDecisionShadowMode.SharedShadow)
                    throw new ArgumentOutOfRangeException(nameof(value));
                aiDecisionShadowMode = value;
            }
        }

        public AiDecisionExecutionMode AiDecisionExecutionMode
        {
            get => aiDecisionExecutionMode;
            set
            {
                if (_ticking)
                    throw new InvalidOperationException(
                        "AI decision execution mode cannot change while a simulation pass is running.");
                if (value != AiDecisionExecutionMode.Legacy &&
                    value != AiDecisionExecutionMode.IndexedCanonical)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (aiUnifiedSnapshotExecutionMode ==
                        AiUnifiedSnapshotExecutionMode.UnifiedAuthority &&
                    value != AiDecisionExecutionMode.IndexedCanonical)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot authority requires IndexedCanonical.");
                }
                aiDecisionExecutionMode = value;
            }
        }

        public AiUnifiedSnapshotShadowMode AiUnifiedSnapshotShadowMode
        {
            get => aiUnifiedSnapshotShadowMode;
            set
            {
                if (_ticking)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot shadow mode cannot change while a simulation pass is running.");
                }
                if (value != AiUnifiedSnapshotShadowMode.Disabled &&
                    value != AiUnifiedSnapshotShadowMode.Shadow)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (value == AiUnifiedSnapshotShadowMode.Shadow &&
                    aiUnifiedSnapshotExecutionMode ==
                    AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot shadow and authority execution are mutually exclusive.");
                }

                aiUnifiedSnapshotShadowMode = value;
                if (value == AiUnifiedSnapshotShadowMode.Disabled)
                    EndAiUnifiedSnapshotShadowPass();
            }
        }

        public AiUnifiedSnapshotExecutionMode AiUnifiedSnapshotExecutionMode
        {
            get => aiUnifiedSnapshotExecutionMode;
            set
            {
                if (_ticking)
                {
                    throw new InvalidOperationException(
                        "Unified AI snapshot execution mode cannot change while a simulation pass is running.");
                }
                if (value != AiUnifiedSnapshotExecutionMode.LegacySeparate &&
                    value != AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                if (value == AiUnifiedSnapshotExecutionMode.UnifiedAuthority)
                {
                    if (aiUnifiedSnapshotShadowMode != AiUnifiedSnapshotShadowMode.Disabled)
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot shadow and authority execution are mutually exclusive.");
                    }
                    if (aiSensingMode != AiSensingMode.SoAAiSensing ||
                        aiDecisionExecutionMode != AiDecisionExecutionMode.IndexedCanonical)
                    {
                        throw new InvalidOperationException(
                            "Unified AI snapshot authority requires SoAAiSensing and IndexedCanonical.");
                    }
                }

                if (aiUnifiedSnapshotExecutionMode == value)
                    return;
                aiUnifiedSnapshotExecutionMode = value;
                EndAiUnifiedSnapshotExecutionPass();
                if (value == AiUnifiedSnapshotExecutionMode.LegacySeparate)
                    RestoreAiUnifiedSnapshotLegacyConsumerBuffers();
            }
        }

        public bool AiUnifiedSnapshotNoPendingRefreshSkipForDiagnostics
        {
            get => aiUnifiedSnapshotNoPendingRefreshSkipForDiagnostics;
            set
            {
                if (_ticking)
                {
                    throw new InvalidOperationException(
                        "Unified AI no-pending refresh mode cannot change while a simulation pass is running.");
                }

                aiUnifiedSnapshotNoPendingRefreshSkipForDiagnostics = value;
            }
        }

        public int AiDecisionIndexedCanonicalFullOracleSampleInterval
        {
            get => aiDecisionIndexedCanonicalFullOracleSampleInterval;
            set
            {
                if (_ticking)
                    throw new InvalidOperationException(
                        "AI decision oracle sampling cannot change while a simulation pass is running.");
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));
                aiDecisionIndexedCanonicalFullOracleSampleInterval = value;
            }
        }

        private bool AiDecisionRequiresSharedRows => true;

        public long AiDecisionShadowEligibleCountForDiagnostics { get => aiRuntime.Decision.ShadowEligibleCount; private set => aiRuntime.Decision.ShadowEligibleCount = value; }
        public long AiDecisionShadowAvailableCountForDiagnostics { get => aiRuntime.Decision.ShadowAvailableCount; private set => aiRuntime.Decision.ShadowAvailableCount = value; }
        public long AiDecisionShadowUnavailableCountForDiagnostics { get => aiRuntime.Decision.ShadowUnavailableCount; private set => aiRuntime.Decision.ShadowUnavailableCount = value; }
        public long AiDecisionShadowComparedCountForDiagnostics { get => aiRuntime.Decision.ShadowComparedCount; private set => aiRuntime.Decision.ShadowComparedCount = value; }
        public long AiDecisionShadowMismatchCountForDiagnostics { get => aiRuntime.Decision.ShadowMismatchCount; private set => aiRuntime.Decision.ShadowMismatchCount = value; }
        public long AiDecisionShadowCloneRngCallCountForDiagnostics { get => aiRuntime.Decision.ShadowCloneRngCallCount; private set => aiRuntime.Decision.ShadowCloneRngCallCount = value; }
        public long AiDecisionShadowRowVisitCountForDiagnostics { get => aiRuntime.Decision.ShadowRowVisitCount; private set => aiRuntime.Decision.ShadowRowVisitCount = value; }
        public long AiDecisionSharedBuildCountForDiagnostics { get => aiRuntime.Decision.SharedBuildCount; private set => aiRuntime.Decision.SharedBuildCount = value; }
        public long AiDecisionSharedRefreshCountForDiagnostics { get => aiRuntime.Decision.SharedRefreshCount; private set => aiRuntime.Decision.SharedRefreshCount = value; }
        public long AiDecisionIndexedEligibleCountForDiagnostics { get => aiRuntime.Decision.IndexedEligibleCount; private set => aiRuntime.Decision.IndexedEligibleCount = value; }
        public long AiDecisionIndexedAvailableCountForDiagnostics { get => aiRuntime.Decision.IndexedAvailableCount; private set => aiRuntime.Decision.IndexedAvailableCount = value; }
        public long AiDecisionIndexedUnavailableCountForDiagnostics { get => aiRuntime.Decision.IndexedUnavailableCount; private set => aiRuntime.Decision.IndexedUnavailableCount = value; }
        public long AiDecisionIndexedComparedCountForDiagnostics { get => aiRuntime.Decision.IndexedComparedCount; private set => aiRuntime.Decision.IndexedComparedCount = value; }
        public long AiDecisionIndexedMismatchCountForDiagnostics { get => aiRuntime.Decision.IndexedMismatchCount; private set => aiRuntime.Decision.IndexedMismatchCount = value; }
        public long AiDecisionIndexedFullRowVisitCountForDiagnostics { get => aiRuntime.Decision.IndexedFullRowVisitCount; private set => aiRuntime.Decision.IndexedFullRowVisitCount = value; }
        public long AiDecisionIndexedRowVisitCountForDiagnostics { get => aiRuntime.Decision.IndexedRowVisitCount; private set => aiRuntime.Decision.IndexedRowVisitCount = value; }
        public long AiDecisionIndexedCanonicalEligibleCountForDiagnostics { get => aiRuntime.Decision.IndexedCanonicalEligibleCount; private set => aiRuntime.Decision.IndexedCanonicalEligibleCount = value; }
        public long AiDecisionIndexedCanonicalCommittedCountForDiagnostics { get => aiRuntime.Decision.IndexedCanonicalCommittedCount; private set => aiRuntime.Decision.IndexedCanonicalCommittedCount = value; }
        public long AiDecisionIndexedCanonicalFallbackCountForDiagnostics { get => aiRuntime.Decision.IndexedCanonicalFallbackCount; private set => aiRuntime.Decision.IndexedCanonicalFallbackCount = value; }
        public long AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics { get => aiRuntime.Decision.IndexedCanonicalFullOracleSampleCount; private set => aiRuntime.Decision.IndexedCanonicalFullOracleSampleCount = value; }
        public long AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics { get => aiRuntime.Decision.IndexedCanonicalFullOracleMismatchCount; private set => aiRuntime.Decision.IndexedCanonicalFullOracleMismatchCount = value; }
        public AiDecisionAvailability AiDecisionShadowFirstUnavailableReasonForDiagnostics { get => aiRuntime.Decision.ShadowFirstUnavailableReason; private set => aiRuntime.Decision.ShadowFirstUnavailableReason = value; }
        public AiDecisionShadowMismatchReason AiDecisionShadowFirstMismatchReasonForDiagnostics { get => aiRuntime.Decision.ShadowFirstMismatchReason; private set => aiRuntime.Decision.ShadowFirstMismatchReason = value; }
        public AiDecisionShadowExceptionStage AiDecisionShadowFirstExceptionStageForDiagnostics { get => aiRuntime.Decision.ShadowFirstExceptionStage; private set => aiRuntime.Decision.ShadowFirstExceptionStage = value; }
        public AiDecisionIndexedMismatchReason AiDecisionIndexedFirstMismatchReasonForDiagnostics { get => aiRuntime.Decision.IndexedFirstMismatchReason; private set => aiRuntime.Decision.IndexedFirstMismatchReason = value; }
        public AiDecisionAvailability AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics { get => aiRuntime.Decision.IndexedCanonicalFirstFallbackReason; private set => aiRuntime.Decision.IndexedCanonicalFirstFallbackReason = value; }
        public AiDecisionIndexedMismatchReason AiDecisionIndexedCanonicalFirstOracleMismatchReasonForDiagnostics { get => aiRuntime.Decision.IndexedCanonicalFirstOracleMismatchReason; private set => aiRuntime.Decision.IndexedCanonicalFirstOracleMismatchReason = value; }
        public long AiUnifiedSnapshotShadowBuildCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowBuildCount; private set => aiRuntime.Decision.UnifiedShadowBuildCount = value; }
        public long AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowSlotVisitCount; private set => aiRuntime.Decision.UnifiedShadowSlotVisitCount = value; }
        public long AiUnifiedSnapshotShadowRefreshCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowRefreshCount; private set => aiRuntime.Decision.UnifiedShadowRefreshCount = value; }
        public long AiUnifiedSnapshotShadowSensingComparedCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowSensingComparedCount; private set => aiRuntime.Decision.UnifiedShadowSensingComparedCount = value; }
        public long AiUnifiedSnapshotShadowDecisionComparedCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowDecisionComparedCount; private set => aiRuntime.Decision.UnifiedShadowDecisionComparedCount = value; }
        public long AiUnifiedSnapshotShadowUnavailableCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowUnavailableCount; private set => aiRuntime.Decision.UnifiedShadowUnavailableCount = value; }
        public long AiUnifiedSnapshotShadowMismatchCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowMismatchCount; private set => aiRuntime.Decision.UnifiedShadowMismatchCount = value; }
        public long AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowDistinctBoundaryEncodingRowCount; private set => aiRuntime.Decision.UnifiedShadowDistinctBoundaryEncodingRowCount = value; }
        public long AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowFullComparisonSlotVisitCount; private set => aiRuntime.Decision.UnifiedShadowFullComparisonSlotVisitCount = value; }
        public long AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowRefreshComparisonSlotVisitCount; private set => aiRuntime.Decision.UnifiedShadowRefreshComparisonSlotVisitCount = value; }
        public long AiUnifiedSnapshotShadowDerivedComparisonEntryVisitCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowDerivedComparisonEntryVisitCount; private set => aiRuntime.Decision.UnifiedShadowDerivedComparisonEntryVisitCount = value; }
        public long AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowMutationWitnessComparedCount; private set => aiRuntime.Decision.UnifiedShadowMutationWitnessComparedCount = value; }
        public long AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics { get => aiRuntime.Decision.UnifiedShadowRefreshDerivedFullLoopEntryVisitCount; private set => aiRuntime.Decision.UnifiedShadowRefreshDerivedFullLoopEntryVisitCount = value; }
        public long AiUnifiedSnapshotExecutionBuildCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionBuildCount; private set => aiRuntime.Decision.UnifiedExecutionBuildCount = value; }
        public long AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionRollForwardCount; private set => aiRuntime.Decision.UnifiedExecutionRollForwardCount = value; }
        public long AiUnifiedSnapshotExecutionRollForwardDirtySlotCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionRollForwardDirtySlotCount; private set => aiRuntime.Decision.UnifiedExecutionRollForwardDirtySlotCount = value; }
        public long AiUnifiedSnapshotExecutionSlotVisitCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionSlotVisitCount; private set => aiRuntime.Decision.UnifiedExecutionSlotVisitCount = value; }
        public long AiUnifiedSnapshotExecutionCanonicalInitialCaptureCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionCanonicalInitialCaptureCount; private set => aiRuntime.Decision.UnifiedExecutionCanonicalInitialCaptureCount = value; }
        public long AiUnifiedSnapshotExecutionRefreshCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionRefreshCount; private set => aiRuntime.Decision.UnifiedExecutionRefreshCount = value; }
        public long AiUnifiedSnapshotExecutionNoPendingRefreshSkipCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionNoPendingRefreshSkipCount; private set => aiRuntime.Decision.UnifiedExecutionNoPendingRefreshSkipCount = value; }
        public long AiUnifiedSnapshotExecutionIncrementalValidationCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionIncrementalValidationCount; private set => aiRuntime.Decision.UnifiedExecutionIncrementalValidationCount = value; }
        public long AiUnifiedSnapshotExecutionReadCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionReadCount; private set => aiRuntime.Decision.UnifiedExecutionReadCount = value; }
        public long AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionCommittedPassCount; private set => aiRuntime.Decision.UnifiedExecutionCommittedPassCount = value; }
        public long AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionPreCommitFailureCount; private set => aiRuntime.Decision.UnifiedExecutionPreCommitFailureCount = value; }
        public long AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionPreCommitFallbackCount; private set => aiRuntime.Decision.UnifiedExecutionPreCommitFallbackCount = value; }
        public long AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionPostCommitHardBreachCount; private set => aiRuntime.Decision.UnifiedExecutionPostCommitHardBreachCount = value; }
        public AiUnifiedSnapshotMismatch AiUnifiedSnapshotShadowFirstMismatchForDiagnostics { get => aiRuntime.Decision.UnifiedShadowFirstMismatch; private set => aiRuntime.Decision.UnifiedShadowFirstMismatch = value; }
        public AiUnifiedSnapshotExceptionStage AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics { get => aiRuntime.Decision.UnifiedShadowFirstExceptionStage; private set => aiRuntime.Decision.UnifiedShadowFirstExceptionStage = value; }
        public AiUnifiedSnapshotExceptionStage AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionFirstFailureStage; private set => aiRuntime.Decision.UnifiedExecutionFirstFailureStage = value; }
        public Type AiUnifiedSnapshotExecutionFirstFailureTypeForDiagnostics { get => aiRuntime.Decision.UnifiedExecutionFirstFailureType; private set => aiRuntime.Decision.UnifiedExecutionFirstFailureType = value; }
        public Type AiUnifiedSnapshotShadowFirstExceptionTypeForDiagnostics => aiUnifiedSnapshotFirstExceptionType;
        public bool AiUnifiedSnapshotShadowRowsAllocatedForDiagnostics => aiUnifiedSnapshotRows != null;
        public Type AiDecisionShadowFirstExceptionTypeForDiagnostics => aiDecisionShadowFirstExceptionType;
        public AiDecisionWitness AiDecisionShadowLastExpectedForDiagnostics => aiDecisionShadowExpected;
#if UNITY_INCLUDE_TESTS
        public long AiDecisionShadowBeginInvocationCountForTests => aiDecisionShadowBeginInvocationCountForTests;
        public long AiDecisionShadowCompleteInvocationCountForTests => aiDecisionShadowCompleteInvocationCountForTests;
        public bool AiDecisionShadowComparisonActiveForTests => aiDecisionShadowComparisonActive;
        public bool AiDecisionLegacyRngRecordingForTests => aiDecisionLegacyRngRecording;
        public int AiDecisionLegacyRngCountForTests => aiDecisionLegacyRngCount;
        public bool AiDecisionSharedPassAvailableForTests => aiDecisionSharedPassAvailable;
        public ulong AiUnifiedSnapshotExecutionPublishedEpochForTests => aiUnifiedSnapshotPublishedState?.Epoch ?? 0;
        public bool AiUnifiedSnapshotExecutionPublishedEpochIsCurrentForTests =>
            aiUnifiedSnapshotPublishedState != null &&
            aiUnifiedSnapshotPublishedState.Epoch == RuntimeSlotOccupancyEpochForServices;
        public int AiUnifiedSnapshotExecutionPublishedCapacityForTests => aiUnifiedSnapshotPublishedState?.Capacity ?? 0;
        public int AiUnifiedSnapshotExecutionPublishedSpecialSlotCountForTests => aiUnifiedSnapshotPublishedState?.Rows.SpecialSlotCount ?? 0;
        public int AiUnifiedSnapshotExecutionPublishedGroundRoleCountForTests => aiUnifiedSnapshotPublishedState?.Rows.GroundRoleSlotCount ?? 0;
        public int AiUnifiedSnapshotExecutionPublishedTeamSummaryCountForTests => aiUnifiedSnapshotPublishedState?.Rows.TeamSummaryCount ?? 0;
        public bool AiUnifiedSnapshotExecutionPublishedFirst10ValidForTests => aiUnifiedSnapshotPublishedState?.MoveModeFirst10Valid == true;
        public int AiUnifiedSnapshotExecutionProbeStateAForTests => aiUnifiedSnapshotExecutionProbeStateAForSelfCheck;
        public int AiUnifiedSnapshotExecutionProbeStateBForTests => aiUnifiedSnapshotExecutionProbeStateBForSelfCheck;
#endif

        public void ResetAiDecisionShadowDiagnostics()
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "AI decision shadow diagnostics cannot be reset while a simulation pass is running.");
            aiRuntime.Decision.ResetDecisionDiagnostics();
            AiDecisionIndexedCanonicalEligibleCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalCommittedCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalFallbackCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalFullOracleSampleCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalFullOracleMismatchCountForDiagnostics = 0;
            AiDecisionIndexedCanonicalFirstFallbackReasonForDiagnostics =
                AiDecisionAvailability.None;
            AiDecisionIndexedCanonicalFirstOracleMismatchReasonForDiagnostics =
                AiDecisionIndexedMismatchReason.None;
        }

        public void ResetAiUnifiedSnapshotShadowDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Unified AI snapshot diagnostics cannot be reset while a simulation pass is running.");
            }

            AiUnifiedSnapshotShadowBuildCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowRefreshCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowSensingComparedCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowDecisionComparedCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowUnavailableCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowMismatchCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowDistinctBoundaryEncodingRowCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowFullComparisonSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowRefreshComparisonSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowDerivedComparisonEntryVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowMutationWitnessComparedCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowRefreshDerivedFullLoopEntryVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotShadowFirstMismatchForDiagnostics = default;
            AiUnifiedSnapshotShadowFirstExceptionStageForDiagnostics =
                AiUnifiedSnapshotExceptionStage.None;
            aiUnifiedSnapshotFirstExceptionType = null;
        }

        public void ResetAiUnifiedSnapshotExecutionDiagnostics()
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Unified AI snapshot execution diagnostics cannot be reset while a simulation pass is running.");
            }

            AiUnifiedSnapshotExecutionBuildCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionRollForwardCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionRollForwardDirtySlotCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionSlotVisitCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionCanonicalInitialCaptureCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionRefreshCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionNoPendingRefreshSkipCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionIncrementalValidationCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionReadCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionCommittedPassCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionPreCommitFailureCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionPreCommitFallbackCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionPostCommitHardBreachCountForDiagnostics = 0;
            AiUnifiedSnapshotExecutionFirstFailureStageForDiagnostics =
                AiUnifiedSnapshotExceptionStage.None;
            AiUnifiedSnapshotExecutionFirstFailureTypeForDiagnostics = null;
        }

        private bool BeginAiDecisionShadowComparison(LF2Entity self, int tickIndex)
        {
            return aiRuntime.Decision.BeginShadowComparison(
                this,
                self,
                tickIndex);
        }

        private void CompleteAiDecisionShadowComparison(bool comparisonStarted)
        {
            aiRuntime.Decision.CompleteShadowComparison(
                this,
                comparisonStarted);
        }



        private AiDecisionAvailability CaptureAiDecisionShadowSnapshot(
            LF2Entity self,
            AiDecisionSnapshot snapshot)
        {
            return aiRuntime.Decision.CaptureShadowSnapshot(
                this,
                battleCharacterInputWriter,
                aiExecutionProfile,
                self,
                snapshot,
                CaptureAiDecisionWorldState(),
                Rng?.State ?? 0,
                Rng?.CallCount ?? 0);
        }

        private void PrepareAiDecisionSharedPass() => aiRuntime.Decision.PrepareSharedPass(this);




        private void RefreshAiDecisionSharedRowAfterCharacterInput(LF2Entity entity) => aiRuntime.Decision.RefreshSharedRowAfterCharacterInput(this, entity);

        private void EndAiDecisionSharedPass() => aiRuntime.Decision.EndSharedPass();

        private void InvalidateAiDecisionSharedPass(AiDecisionAvailability reason) => aiRuntime.Decision.InvalidateSharedPass(reason);

        private void RecordAiDecisionShadowException(
            AiDecisionShadowExceptionStage stage,
            Exception exception)
        {
            aiRuntime.Decision.RecordShadowException(stage, exception);
        }

#if UNITY_INCLUDE_TESTS
        public void SetAiDecisionShadowExceptionStageForSelfCheck(
            AiDecisionShadowExceptionStage stage)
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "Cannot arm AI decision exception injection while ticking.");
            if (stage != AiDecisionShadowExceptionStage.SharedBuild &&
                stage != AiDecisionShadowExceptionStage.SharedPreflight &&
                stage != AiDecisionShadowExceptionStage.KernelEvaluate &&
                stage != AiDecisionShadowExceptionStage.SharedRefresh)
            {
                throw new ArgumentOutOfRangeException(nameof(stage));
            }
            aiRuntime.Decision.SetShadowExceptionStageForSelfCheck(stage);
        }

        public void SetAiDecisionSharedPreflightMutationForSelfCheck(
            int mutationKind,
            int slot)
        {
            if (_ticking)
                throw new InvalidOperationException("Cannot arm AI decision preflight mutation while ticking.");
            if (mutationKind < 0 || mutationKind > 3)
                throw new ArgumentOutOfRangeException(nameof(mutationKind));
            aiRuntime.Decision.SetSharedPreflightMutationForSelfCheck(
                mutationKind,
                slot);
        }

        public void SetAiDecisionIndexedCanonicalPreCommitFailureForSelfCheck(
            AiDecisionAvailability reason)
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "Cannot arm AI decision canonical commit failure while ticking.");
            if (reason == AiDecisionAvailability.None ||
                reason == AiDecisionAvailability.Available)
            {
                throw new ArgumentOutOfRangeException(nameof(reason));
            }
            aiDecisionIndexedCanonicalPreCommitFailureForSelfCheck = reason;
        }

        public void SetAiDecisionSharedPostLegacyStateMutationForSelfCheck(
            int slot,
            int state)
        {
            if (_ticking)
                throw new InvalidOperationException("Cannot arm AI decision row mutation while ticking.");
            aiRuntime.Decision.SetSharedPostLegacyStateMutationForSelfCheck(
                slot,
                state);
        }

#endif

        private AiDecisionWorldState CaptureAiDecisionWorldState()
        {
            BattleFlowRuntimeState flow = Runtime?.Flow;
            return new AiDecisionWorldState
            {
                Difficulty = Difficulty,
                AiPhaseGate = AiPhaseGate,
                InputPhase = InputPhase,
                StageTargetX = Runtime?.Stage?.XMaxOverride > 0
                    ? Runtime.Stage.XMaxOverride
                    : Runtime?.Stage?.StageWidthPx ?? 800,
                StageZMin = Runtime?.Stage?.ZMin ?? 180,
                StageZMax = Runtime?.Stage?.ZMax ?? 350,
                FlowAiDifficulty = flow?.AiDifficulty ?? 0,
                FlowRand3 = flow?.AiRand3 ?? 0,
                FlowRand5 = flow?.AiRand5 ?? 0,
                FlowRand15 = flow?.AiRand15 ?? 0,
                FlowRand20 = flow?.AiRand20 ?? 0,
                FlowMoveMode = flow?.AiMoveMode ?? 0,
                FlowStageTargetX = flow?.AiStageTargetX ??
                    (Runtime?.Stage?.StageWidthPx ?? 800),
            };
        }


        private static bool TryCaptureAiDecisionInputState(
            NTSDEntityRuntime runtime,
            out AiDecisionInputState input)
        {
            return SimulationAiDecisionModule.TryCaptureInputState(
                runtime,
                out input);
        }


        private void RecordAiDecisionShadowLegacyRng(int modulus, int raw, int value) => aiRuntime.Decision.RecordLegacyRng(modulus, raw, value);

        private AiDecisionShadowMismatchReason CompareAiDecisionShadowResult(LF2Entity self)
        {
            if (!TryCaptureAiDecisionInputState(
                    self.Runtime,
                    out AiDecisionInputState actualInput))
            {
                return AiDecisionShadowMismatchReason.Input;
            }
            AiDecisionWorldState actualWorld = CaptureAiDecisionWorldState();
            return aiRuntime.Decision.CompareShadowResult(
                actualInput,
                actualWorld,
                Rng?.State ?? 0,
                Rng?.CallCount ?? 0,
                aiDecisionLegacyCharacterDecisionPosition);
        }


        private bool TryPrepareAiUnifiedSnapshotExecutionPass(
            BattleAiInputDetailDiagnostics diagnostics)
        {
            return aiRuntime.Decision.TryPrepareUnifiedExecutionPass(
                this,
                battleAiUnifiedRowPublisher,
                ForceFullAiUnifiedSnapshotRebuildForDiagnostics);
        }

        private void EnsureAiUnifiedSnapshotExecutionScratchCapacity(int capacity) => aiRuntime.Decision.EnsureExecutionScratchCapacity(capacity);

        private void ActivateAiUnifiedSnapshotExecutionPass(
            AiUnifiedSnapshotExecutionState candidate)
        {
            aiRuntime.Decision.ActivateUnifiedExecutionPass(
                battleAiUnifiedRowPublisher,
                candidate);
        }

        private bool ValidateAiUnifiedSnapshotExecutionState(
            AiUnifiedSnapshotExecutionState candidate,
            int capacity,
            ulong epoch)
        {
            return aiRuntime.Decision.ValidateUnifiedExecutionState(
                this,
                candidate,
                capacity,
                epoch);
        }

        private void PrepareAiUnifiedSnapshotLegacyConsumerBuffers(int capacity) => aiRuntime.Decision.PrepareLegacyConsumerBuffers(capacity);

        private void RestoreAiUnifiedSnapshotLegacyConsumerBuffers() => aiRuntime.Decision.RestoreLegacyConsumerBuffers();

        private void EndAiUnifiedSnapshotExecutionPass()
        {
            battleAiUnifiedRowPublisher.EndPass();
            aiRuntime.Decision.EndExecutionPassState();
        }

        private void SuspendAiUnifiedSnapshotExecutionPass() => aiRuntime.Decision.EndExecutionPassState();

        private void RecordAiUnifiedSnapshotExecutionFailure(
            AiUnifiedSnapshotExceptionStage stage,
            Exception exception,
            bool postCommit)
        {
            aiRuntime.Decision.RecordUnifiedExecutionFailure(
                stage,
                exception,
                postCommit);
        }

        private void ThrowAiUnifiedSnapshotExecutionHardBreach(
            AiUnifiedSnapshotExceptionStage stage,
            string message)
        {
            var exception = new InvalidOperationException(message);
            RecordAiUnifiedSnapshotExecutionFailure(stage, exception, true);
            aiUnifiedSnapshotExecutionCommittedThisPass = false;
            aiSoASensingPassInvalidated = true;
            InvalidateAiDecisionSharedPass(AiDecisionAvailability.SnapshotMissing);
            throw exception;
        }

        private void BeginAiUnifiedSnapshotExecutionConsumer(LF2Entity entity) => aiRuntime.Decision.BeginUnifiedExecutionConsumer(entity);

        private bool AiUnifiedSnapshotExecutionOwnsCurrentPass => aiUnifiedSnapshotExecutionCommittedThisPass;

        private bool AiUnifiedSnapshotExecutionFallbackForbidden =>
            aiUnifiedSnapshotExecutionCommittedThisPass ||
            aiUnifiedSnapshotExecutionConsumerStartedThisPass;

        private void PrepareAiUnifiedSnapshotShadowPass(
            BattleAiInputDetailDiagnostics diagnostics)
        {
            aiRuntime.Decision.PrepareUnifiedShadowPass(this, diagnostics);
        }

        private void EnsureAiUnifiedSnapshotCapacity(int capacity) => aiRuntime.Decision.EnsureUnifiedShadowCapacity(capacity);

        private void CompleteAiUnifiedSnapshotShadowInitialComparison() => aiRuntime.Decision.CompleteUnifiedShadowInitialComparison(this);

        private void RefreshAiUnifiedSnapshotShadowRowAfterCharacterInput(
            LF2Entity entity)
        {
            aiRuntime.Decision.RefreshUnifiedShadowRowAfterCharacterInput(
                this,
                entity);
        }

        private void RefreshAiUnifiedSnapshotExecutionRowAfterCharacterInput(
            LF2Entity entity)
        {
            aiRuntime.Decision.RefreshUnifiedExecutionRowAfterCharacterInput(
                this,
                battleAiUnifiedRowPublisher,
                battleIdentityWriter,
                battleFrameMotionWriter,
                battleCharacterInputWriter,
                battleRelationLinkWriter,
                battleVitalWriter,
                entity,
                ForceFullCharacterInputPostRefreshForDiagnostics ||
                    HasCharacterInputPassMutationOverrideForDecisionModule,
                ValidateIncrementalAiUnifiedRowForDiagnostics);
        }

        private void EndAiUnifiedSnapshotShadowPass() => aiRuntime.Decision.EndUnifiedShadowPass();

        private void InvalidateAiUnifiedSnapshotShadowPass() => aiRuntime.Decision.InvalidateUnifiedShadowPass();

        private void CompareAiUnifiedSnapshotShadow(
            AiUnifiedSnapshotConsumer consumer,
            bool fullComparison,
            int refreshSlot)
        {
            aiRuntime.Decision.CompareUnifiedSnapshotShadow(
                consumer,
                fullComparison,
                refreshSlot);
        }

        private void BeginAiUnifiedSnapshotProductionMutationWitnessPass(
            AiUnifiedSnapshotConsumer consumer,
            ulong epoch)
        {
            aiRuntime.Decision.BeginProductionMutationWitnessPass(
                consumer,
                epoch);
        }

        private void RecordAiUnifiedSnapshotProductionMutationWitness(
            AiUnifiedSnapshotConsumer consumer,
            ulong epoch,
            int slot,
            uint generation,
            int stableId,
            bool roleRebuilt,
            bool teamRebuilt,
            int oldX,
            int newX,
            int oldTeam,
            int newTeam,
            int oldRoleFlags,
            int newRoleFlags,
            bool oldLiving,
            bool newLiving,
            int oldHp,
            int newHp)
        {
            aiRuntime.Decision.RecordProductionMutationWitness(
                consumer,
                epoch,
                slot,
                generation,
                stableId,
                roleRebuilt,
                teamRebuilt,
                oldX,
                newX,
                oldTeam,
                newTeam,
                oldRoleFlags,
                newRoleFlags,
                oldLiving,
                newLiving,
                oldHp,
                newHp);
        }

        private static int PackAiUnifiedSnapshotRoleFlags(
            bool ground,
            bool air)
        {
            return SimulationAiDecisionModule.PackRoleFlags(ground, air);
        }

        private void ResetAiUnifiedMoveModeFirst10Snapshot() => aiRuntime.Decision.ResetUnifiedMoveModeFirst10Snapshot();

#if UNITY_INCLUDE_TESTS
        public void SetAiUnifiedSnapshotWitnessMutationForSelfCheck(
            AiUnifiedSnapshotConsumer consumer)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI witness mutation while ticking.");
            }
            if (consumer != AiUnifiedSnapshotConsumer.SoASensing &&
                consumer != AiUnifiedSnapshotConsumer.IndexedDecision)
            {
                throw new ArgumentOutOfRangeException(nameof(consumer));
            }
            aiUnifiedSnapshotWitnessMutationConsumerForSelfCheck = consumer;
        }

        public void SetAiUnifiedSnapshotProductMutationForSelfCheck(
            AiUnifiedSnapshotProductMutationKind kind,
            int slot)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI product mutation while ticking.");
            }
            if (kind != AiUnifiedSnapshotProductMutationKind.FallbackReference &&
                kind != AiUnifiedSnapshotProductMutationKind.MoveModeFirst10Hp)
            {
                throw new ArgumentOutOfRangeException(nameof(kind));
            }
            if (slot < 0 ||
                kind == AiUnifiedSnapshotProductMutationKind.MoveModeFirst10Hp &&
                slot >= aiUnifiedMoveModeFirst10Hp.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }
            aiUnifiedSnapshotProductMutationKindForSelfCheck = kind;
            aiUnifiedSnapshotProductMutationSlotForSelfCheck = slot;
        }

        public void SetAiUnifiedSnapshotExceptionForSelfCheck(
            AiUnifiedSnapshotExceptionStage stage)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI snapshot exception while ticking.");
            }
            if (stage == AiUnifiedSnapshotExceptionStage.None)
                throw new ArgumentOutOfRangeException(nameof(stage));
            aiRuntime.Decision.SetUnifiedSnapshotExceptionForSelfCheck(stage);
        }

        public void SetAiUnifiedSnapshotExecutionFailureForSelfCheck(
            AiUnifiedSnapshotExceptionStage stage)
        {
            SetAiUnifiedSnapshotExceptionForSelfCheck(stage);
        }

        public void SetAiUnifiedSnapshotExecutionVisibilityProbeForSelfCheck(
            int observerSlotA,
            int targetSlotA,
            int observerSlotB,
            int targetSlotB)
        {
            if (_ticking)
                throw new InvalidOperationException(
                    "Cannot arm unified AI visibility probes while ticking.");
            aiUnifiedSnapshotExecutionProbeObserverSlotAForSelfCheck = observerSlotA;
            aiUnifiedSnapshotExecutionProbeTargetSlotAForSelfCheck = targetSlotA;
            aiUnifiedSnapshotExecutionProbeStateAForSelfCheck = int.MinValue;
            aiUnifiedSnapshotExecutionProbeObserverSlotBForSelfCheck = observerSlotB;
            aiUnifiedSnapshotExecutionProbeTargetSlotBForSelfCheck = targetSlotB;
            aiUnifiedSnapshotExecutionProbeStateBForSelfCheck = int.MinValue;
        }

        public bool ValidateAiUnifiedSnapshotExecutionPublishedStateForSelfCheck()
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null &&
                   ValidateAiUnifiedSnapshotExecutionState(
                       published,
                       published.ExpectedCapacity,
                       published.Epoch);
        }

        public int GetAiUnifiedSnapshotExecutionPublishedGenerationForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? unchecked((int)published.Rows.Generation[slot])
                : 0;
        }

        public int GetAiUnifiedSnapshotExecutionPublishedStableIdForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? published.Rows.Identity[slot]
                : 0;
        }

        public int GetAiUnifiedSnapshotExecutionPublishedHitJForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? published.Rows.HitJ[slot]
                : 0;
        }

        public int GetAiUnifiedSnapshotExecutionPublishedSensingBoundaryForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? published.SoASensingBoundaryFlags[slot]
                : 0;
        }

        public int GetAiUnifiedSnapshotExecutionPublishedDecisionBoundaryForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null && slot >= 0 && slot < published.Capacity
                ? published.DecisionBoundaryFlags[slot]
                : 0;
        }

        public bool IsAiUnifiedSnapshotExecutionPublishedFallbackForSelfCheck(
            int slot,
            LF2Entity entity)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null &&
                   slot >= 0 &&
                   slot < published.Capacity &&
                   ReferenceEquals(published.FallbackSlots[slot], entity);
        }

        public bool IsAiUnifiedSnapshotExecutionPublishedFirst10PresentForSelfCheck(
            int slot)
        {
            AiUnifiedSnapshotExecutionState published =
                aiUnifiedSnapshotPublishedState;
            return published != null &&
                   slot >= 0 &&
                   slot < published.MoveModeFirst10Present.Length &&
                   published.MoveModeFirst10Present[slot];
        }

        public void SetAiUnifiedSnapshotBoundaryMutationForSelfCheck(
            AiUnifiedSnapshotConsumer consumer,
            int slot,
            int xorMask)
        {
            if (_ticking)
            {
                throw new InvalidOperationException(
                    "Cannot arm unified AI snapshot mutation while ticking.");
            }
            if (consumer != AiUnifiedSnapshotConsumer.SoASensing &&
                consumer != AiUnifiedSnapshotConsumer.IndexedDecision)
            {
                throw new ArgumentOutOfRangeException(nameof(consumer));
            }
            if (slot < 0)
                throw new ArgumentOutOfRangeException(nameof(slot));
            if (xorMask == 0)
                throw new ArgumentOutOfRangeException(nameof(xorMask));

            aiUnifiedSnapshotBoundaryMutationConsumerForSelfCheck = consumer;
            aiUnifiedSnapshotBoundaryMutationSlotForSelfCheck = slot;
            aiUnifiedSnapshotBoundaryMutationXorForSelfCheck = xorMask;
        }
#endif

    }

}
