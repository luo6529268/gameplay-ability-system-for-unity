using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Ecs;
using NTSD.Simulation.Presentation;
using UnityEngine;

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
    /// NTSD 战斗对象的确定性模拟调度器。各职责由主类持有的普通 module 实例实现；
    /// 现存 partial 仅作为待迁移的历史边界，不再新增。
    /// </summary>
    public partial class SimulationWorld
    {
        private readonly SimulationEntityTraversal entityTraversal;
        private readonly SimulationQueryAndLinkModule queryAndLinkModule;
        private readonly SimulationRandomWeaponDropBuffer randomWeaponDropBuffer;
        private readonly SimulationBattleBufferModule battleBuffers;
        private readonly SimulationRuntimeCapacityModule runtimeCapacityModule;
        private readonly SimulationFrameInputModule frameInputModule;
        private readonly SimulationObjectBucketRegistry objectBucketRegistry;
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
        private readonly BattleResultsWriter battleResultsWriter;
        private readonly CharacterMechanics characterMechanics;
        private readonly SimulationDiagnosticsModule diagnosticsModule =
            new SimulationDiagnosticsModule();
        private readonly SimulationWorldMutationTracker runtimeMutationTracker;
        private readonly SimulationWorldHooks runtimeHooks =
            new SimulationWorldHooks();
        private bool logicOnlyEntityMaterialization;

        internal int ActiveDataObjectTypeCacheTick { get; private set; } = -1;

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
        public BattleEcsCapacityProfile BattleEcsCapacityProfileForDiagnostics =>
            battleEcsShadowModule.CapacityProfile;
        public BattleEcsShadowMode BattleEcsShadowModeForDiagnostics =>
            battleEcsShadowModule.Mode;
        public BattleEcsShadowDiagnostics BattleEcsShadowDiagnosticsForDiagnostics =>
            battleEcsShadowModule.Diagnostics;
        public BattleEcsCooldownPassMode BattleEcsCooldownPassModeForDiagnostics =>
            battleEcsCooldownPass.Mode;
        public BattleEcsCooldownPassDiagnostics BattleEcsCooldownPassDiagnosticsForDiagnostics =>
            battleEcsCooldownPass.Diagnostics;
        public BattleEcsCharacterStageZPassMode BattleEcsCharacterStageZPassModeForDiagnostics =>
            battleEcsCharacterStageZPass.Mode;
        public BattleEcsCharacterStageZPassDiagnostics BattleEcsCharacterStageZPassDiagnosticsForDiagnostics =>
            battleEcsCharacterStageZPass.Diagnostics;
        public BattleEcsCharacterPreFrameBoundsPassMode
            BattleEcsCharacterPreFrameBoundsPassModeForDiagnostics =>
                battleEcsCharacterPreFrameBoundsPass.Mode;
        public BattleEcsCharacterPreFrameBoundsPassDiagnostics
            BattleEcsCharacterPreFrameBoundsPassDiagnosticsForDiagnostics =>
                battleEcsCharacterPreFrameBoundsPass.Diagnostics;
        public BattleEcsFramePostProcessPassMode BattleEcsFramePostProcessPassModeForDiagnostics =>
            battleEcsFramePostProcessPass.Mode;
        public BattleEcsFramePostProcessPassDiagnostics BattleEcsFramePostProcessPassDiagnosticsForDiagnostics =>
            battleEcsFramePostProcessPass.Diagnostics;
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
        internal SimulationObjectBucketRegistry ObjectBucketRegistryForSnapshotRestore =>
            objectBucketRegistry;
        internal RuntimeCharacterConfigResolver RuntimeCharacterConfigs =>
            runtimeCharacterConfigs;
        internal BattleRuntimeDataCatalog RuntimeDataCatalog => runtimeDataCatalog;
        internal BattleLogicReferencePool LogicReferencePool => logicReferencePool;
        internal BattleLogicEntityFactory LogicEntityFactory => logicEntityFactory;
        internal BattleLogicObjectPointRuntime LogicObjectPointRuntime =>
            logicObjectPointRuntime;
        internal bool UsesLogicOnlyEntityMaterialization =>
            logicOnlyEntityMaterialization;

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

        internal void UnsealRuntimeDataCatalog()
        {
            runtimeDataCatalog.Unseal();
        }
        internal StageSpawnTaskConfigurator StageSpawnTaskConfigurator =>
            stageSpawnTaskConfigurator;
        internal BattleCharacterInputActionResolver CharacterInputActionResolver =>
            battleCharacterInputActionResolver;
        internal BattleIdentityWriter IdentityWriter => battleIdentityWriter;
        internal BattleCharacterInputWriter CharacterInputWriter =>
            battleCharacterInputWriter;

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

        internal int RequiredStageSpawnSnapshotEntryCapacity =>
            battleWorldStageSpawnSnapshotModule.RequiredEntryCapacity;

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

        internal int RequiredRuntimeSlotSnapshotCapacity =>
            battleWorldRuntimeSlotSnapshotModule.SlotCapacity;

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

        internal int RequiredEntityRuntimeSnapshotCapacity =>
            battleWorldEntityRuntimeSnapshotModule.SlotCapacity;

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

        internal int RequiredEntityBaseShellSnapshotCapacity =>
            battleWorldEntityBaseShellSnapshotModule.SlotCapacity;

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

        internal int RequiredLivingShellSnapshotCapacity =>
            battleWorldLivingShellSnapshotModule.SlotCapacity;

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

        internal int RequiredCharacterShellSnapshotCapacity =>
            battleWorldCharacterShellSnapshotModule.SlotCapacity;

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

        internal int RequiredWeaponShellSnapshotCapacity =>
            battleWorldWeaponShellSnapshotModule.SlotCapacity;

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

        internal int RequiredSpecialOtherShellSnapshotCapacity =>
            battleWorldSpecialOtherShellSnapshotModule.SlotCapacity;

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
                battleFrameMotionWriter.Bind(entity.Runtime, handle);
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

        internal BattleFrameMotionWriter FrameMotionWriter =>
            battleFrameMotionWriter;
        internal BattleRelationLinkWriter RelationLinkWriter =>
            battleRelationLinkWriter;
        public int PositiveLinkIndexCountForDiagnostics =>
            battleRelationLinkWriter.PositiveLinkCount;
        internal BattleVitalWriter VitalWriter => battleVitalWriter;
        internal BattleCharacterActionWriter CharacterActionWriter =>
            battleCharacterActionWriter;
        internal BattleAiInputWriter AiInputWriter => battleAiInputWriter;
        internal BattleBoundaryWriter BoundaryWriter => battleBoundaryWriter;
        internal BattleInteractionWriter InteractionWriter => battleInteractionWriter;
        internal BattleHeldObjectWriter HeldObjectWriter => battleHeldObjectWriter;
        internal BattleCpointWriter CpointWriter => battleCpointWriter;
        internal BattleDamageWriter DamageWriter => battleDamageWriter;
        internal BattleStructuralWriter StructuralWriter => battleStructuralWriter;
        internal BattleResultsWriter ResultsWriter => battleResultsWriter;
        internal CharacterMechanics CharacterMechanicsForServices =>
            characterMechanics;
        public BattleStructuralWriterDiagnostics StructuralWriterDiagnosticsForDiagnostics =>
            battleStructuralWriter.Diagnostics;

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

        public int FindNextBattleEcsActiveSlotForDiagnostics(int startSlot)
        {
            return battleEcsShadowModule.FindNextActiveSlot(startSlot);
        }

        internal void RefreshBattleEcsShadowAfterTick(int tickIndex)
        {
            battleEcsShadowModule.CaptureAndCompareNoThrow(tickIndex);
        }

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

        internal void RunBattleEcsCooldownPass(int tickIndex)
        {
            battleEcsCooldownPass.Execute(tickIndex);
        }

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

        internal void RunBattleEcsCharacterPreFrameBoundsPass()
        {
            battleEcsCharacterPreFrameBoundsPass.Execute();
        }

        internal void RunLegacyPreFrameBoundsAll()
        {
            stageRenderModule.RunLegacyPreFrameBoundsAll();
        }

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

        internal void RunBattleEcsFramePostProcessPass()
        {
            battleEcsFramePostProcessPass.Execute();
        }

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

        internal bool ShouldObserveBattleHitExecutionPlanLegacyCandidateRead =>
            battleEcsHitExecutionPlan.ShouldObserveLegacyCandidateRead;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyPreprocess =>
            battleEcsHitExecutionPlan.ShouldObserveLegacyPreprocess;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects =>
            battleEcsHitExecutionPlan.ShouldObserveLegacyConsumeEffects;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyDisposition =>
            battleEcsHitExecutionPlan.ShouldObserveLegacyDisposition;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyDispatch =>
            battleEcsHitExecutionPlan.ShouldObserveLegacyDispatch;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyWriterEffect =>
            battleEcsHitExecutionPlan.ShouldObserveLegacyWriterEffect;

        internal bool ShouldObserveBattleHitExecutionPlanLegacyLifecycleEffect =>
            battleEcsHitExecutionPlan.ShouldObserveLegacyLifecycleEffect;

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

        internal void EndBattleHitExecutionPlanLegacyObservation()
        {
            battleEcsHitExecutionPlan.EndLegacyObservationPass();
        }

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

        public BattlePresentationCoordinator BattlePresentation =>
            stageRenderModule.BattlePresentation;

        public BattlePixelFramePlan CurrentPixelFramePlan =>
            stageRenderModule.CurrentPixelFramePlan;

        public int LateRendererUpdateInvocationCountForDiagnostics =>
            stageRenderModule.LateRendererUpdateInvocationCountForDiagnostics;

        public long CentralOnlyRendererShellBypassCountForDiagnostics =>
            stageRenderModule.CentralOnlyRendererShellBypassCountForDiagnostics;

        public int PresentationRenderOrderBuildCountForDiagnostics =>
            stageRenderModule.PresentationRenderOrderBuildCountForDiagnostics;

        public int PresentationRenderOrderReusePublishCountForDiagnostics =>
            stageRenderModule.PresentationRenderOrderReusePublishCountForDiagnostics;

        public int PresentationEntityScanAndSortCountForDiagnostics =>
            stageRenderModule.PresentationEntityScanAndSortCountForDiagnostics;

        public bool SkipLateRendererUpdateForDiagnostics =>
            stageRenderModule.SkipLateRendererUpdateForDiagnostics;

        public long SkippedLateRendererUpdateTickCountForDiagnostics =>
            stageRenderModule.SkippedLateRendererUpdateTickCountForDiagnostics;

        public bool ConfigureSkipLateRendererUpdateForDiagnostics(
            bool requested,
            bool simulationOnly)
        {
            return stageRenderModule.ConfigureSkipLateRendererUpdateForDiagnostics(
                requested,
                simulationOnly);
        }

        public void RestoreSkipLateRendererUpdateForDiagnostics(bool previous)
        {
            stageRenderModule.RestoreSkipLateRendererUpdateForDiagnostics(previous);
        }

        internal void PublishPixelFramePlan(BattlePixelFramePlan plan)
        {
            stageRenderModule.PublishPixelFramePlan(plan);
        }

        public void SetBattlePresentationBackend(BattlePresentationBackendMode mode)
        {
            stageRenderModule.SetBattlePresentationBackend(mode);
        }

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

        public bool IsGroundPointWalkable(Vector2 pointXY)
        {
            return stageRenderModule.IsGroundPointWalkable(pointXY);
        }

        public void RefreshStageRuntimeSnapshotFromScene()
        {
            stageRenderModule.RefreshStageRuntimeSnapshotFromScene();
        }

        public void PrepareStageRuntimeSnapshotForTick(int tickIndex)
        {
            stageRenderModule.PrepareStageRuntimeSnapshotForTick(tickIndex);
        }

        public bool ConfigureLegacyPerPassStageRefreshForDiagnostics(bool requested)
        {
            return stageRenderModule.ConfigureLegacyPerPassStageRefreshForDiagnostics(
                requested);
        }

        public bool ForceLegacyPerPassStageRefreshForDiagnostics =>
            stageRenderModule.ForceLegacyPerPassStageRefreshForDiagnostics;
        public long StageRuntimeSceneRefreshCountForDiagnostics =>
            stageRenderModule.StageRuntimeSceneRefreshCountForDiagnostics;
        public long StageRuntimeHostPrepareCountForDiagnostics =>
            stageRenderModule.StageRuntimeHostPrepareCountForDiagnostics;
        public long StageRuntimeHostReuseCountForDiagnostics =>
            stageRenderModule.StageRuntimeHostReuseCountForDiagnostics;
        public long StageRuntimeLegacyPerPassRefreshCountForDiagnostics =>
            stageRenderModule.StageRuntimeLegacyPerPassRefreshCountForDiagnostics;

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

        internal void RunLegacyCharacterZStageBounds()
        {
            stageRenderModule.ClampCharacterZToStageBoundsAll();
        }

        public void ApplyPreFrameBoundsAll()
        {
            stageRenderModule.PrepareStageRuntimeForKernelPass();
            stageRenderModule.ApplyPreFrameBoundsAll();
        }

        public void RenderDispatchAll(int tickIndex)
        {
            stageRenderModule.RenderDispatchAll(tickIndex);
        }

        public void RenderDispatchAll(int tickIndex, bool buildPresentation)
        {
            stageRenderModule.RenderDispatchAll(tickIndex, buildPresentation);
        }

        internal void CaptureSimulationWorkerPresentationFrame(int tickIndex)
        {
            stageRenderModule.CaptureSimulationWorkerPresentationFrame(tickIndex);
        }

        internal void PresentLatestFrame(int tickIndex)
        {
            stageRenderModule.PresentLatestFrame(tickIndex);
        }

        internal static bool RequiresLegacySpriteRendererCapacityGuard(
            BattlePixelFramePlan plan)
        {
            return SimulationStageRenderModule.RequiresLegacySpriteRendererCapacityGuard(plan);
        }

        internal void GetPresentationEntitiesNoAlloc(List<LF2Entity> destination)
        {
            stageRenderModule.GetPresentationEntitiesNoAlloc(destination);
        }

        internal void RecordLegacyShadowProbe(LF2Entity entity, SpriteRenderer renderer)
        {
            stageRenderModule.RecordLegacyShadowProbe(entity, renderer);
        }

        internal void RecordLegacyEntityProbe(LF2Entity entity, SpriteRenderer renderer)
        {
            stageRenderModule.RecordLegacyEntityProbe(entity, renderer);
        }

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

        internal void BuildPresentationRenderOrder()
        {
            stageRenderModule.BuildPresentationRenderOrder();
        }

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

        internal void RecordPresentationEntityScanAndSortForDiagnostics()
        {
            stageRenderModule.RecordPresentationEntityScanAndSortForDiagnostics();
        }

        internal static void ValidateLegacySpriteRendererPresentationCapacity(
            int materializedEntityCount)
        {
            SimulationStageRenderModule.ValidateLegacySpriteRendererPresentationCapacity(
                materializedEntityCount);
        }

        internal int GetPresentationRenderSortingOrder(LF2Entity entity, int subOrder)
        {
            return stageRenderModule.GetPresentationRenderSortingOrder(entity, subOrder);
        }

        internal void ResetUnityFixedWorldRenderOffsets()
        {
            stageRenderModule.ResetUnityFixedWorldRenderOffsets();
        }

        public void UpdateBattleResultsFlow()
        {
            battleResultsWriter.UpdateSummaryActivation();
        }

        internal void RunActiveBattleResultsTick()
        {
            battleResultsWriter.RunActiveTick();
        }

        internal void ResetUnityFixedWorldCameraStateForModule()
        {
            _cameraX = 0;
            _cameraVel = 0;
        }

        internal void GetNonEntityRendererObjectsForModule(
            List<ISimObject> destination)
        {
            destination.Clear();
            if (!_buckets.TryGetValue(
                    SimOrderConstants.Renderer,
                    out SimulationObjectBucket bucket))
            {
                return;
            }

            bucket.EnsureSorted(runtimeStableIdComparer);
            for (int i = 0; i < bucket.items.Count; i++)
            {
                if (bucket.items[i] is LF2Entity)
                    continue;
                if (bucket.items[i] is LF2ObjectRenderer)
                    destination.Add(bucket.items[i]);
            }
        }

        public void CurrentWaveStageTickAll()
        {
            stageWaveModule.CurrentWaveStageTickAll();
        }

        public void ConfigureStageCampaigns(
            List<BattleStageCampaignData> campaigns,
            int stageSeriesIdx,
            int initialWaveIdx)
        {
            stageWaveModule.ConfigureStageCampaigns(
                campaigns,
                stageSeriesIdx,
                initialWaveIdx);
        }

        public bool StartInitialStageWave()
        {
            return stageWaveModule.StartInitialStageWave();
        }

        // Keep the diagnostic reflection surface on the main class while the
        // implementation and state ownership live in the stage-wave module.
        private int StageSpawnEntryFactor()
        {
            return stageWaveModule.StageSpawnEntryFactor();
        }

        private int SpawnStageImmediateEntrySlot(BattleStageSpawnData spawn)
        {
            return stageWaveModule.SpawnStageImmediateEntrySlot(spawn);
        }

        internal int FindFirstFreeRuntimeSlotForModule(
            int startSlot,
            int endSlotExclusive)
        {
            return FindFirstFreeRuntimeSlot(startSlot, endSlotExclusive);
        }

        internal static bool UsesStageCharacterInitSemantics(int dataObjectType)
        {
            return SimulationStageWaveModule.UsesStageCharacterInitSemantics(dataObjectType);
        }

        internal static void ApplyStageSpawnRuntimeContract(LF2Entity entity, int hp)
        {
            SimulationStageWaveModule.ApplyStageSpawnRuntimeContract(entity, hp);
        }

        public BattleTickPhaseDiagnostics ActiveBattleTickPhaseDiagnosticsForDiagnostics =>
            diagnosticsModule.ActiveBattleTickPhase;

        public BattleTickPhaseDiagnostics EnableBattleTickPhaseDiagnosticsForDiagnostics()
        {
            return diagnosticsModule.EnableBattleTickPhase();
        }

        public void DisableBattleTickPhaseDiagnosticsForDiagnostics()
        {
            diagnosticsModule.DisableBattleTickPhase();
        }

        public bool BattleTickDetailPhaseDiagnosticsAllocatedForDiagnostics =>
            diagnosticsModule.BattleTickDetailAllocated;

        public BattleTickDetailPhaseDiagnostics ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics =>
            diagnosticsModule.ActiveBattleTickDetailPhase;

        public BattleTickDetailPhaseDiagnostics EnableBattleTickDetailPhaseDiagnosticsForDiagnostics()
        {
            return diagnosticsModule.EnableBattleTickDetailPhase();
        }

        public void DisableBattleTickDetailPhaseDiagnosticsForDiagnostics()
        {
            diagnosticsModule.DisableBattleTickDetailPhase();
        }

        public bool BattleAiInputDetailDiagnosticsAllocatedForDiagnostics =>
            diagnosticsModule.BattleAiInputDetailAllocated;

        public BattleAiInputDetailDiagnostics ActiveBattleAiInputDetailDiagnosticsForDiagnostics =>
            diagnosticsModule.ActiveBattleAiInputDetail;

        public BattleAiInputDetailDiagnostics EnableBattleAiInputDetailDiagnosticsForDiagnostics()
        {
            return diagnosticsModule.EnableBattleAiInputDetail();
        }

        public void DisableBattleAiInputDetailDiagnosticsForDiagnostics()
        {
            diagnosticsModule.DisableBattleAiInputDetail();
        }

        public bool BattlePresentationPhaseDiagnosticsAllocatedForDiagnostics =>
            diagnosticsModule.BattlePresentationPhaseAllocated;

        public BattlePresentationPhaseDiagnostics
            ActiveBattlePresentationPhaseDiagnosticsForDiagnostics =>
                diagnosticsModule.ActiveBattlePresentationPhase;

        public BattlePresentationPhaseDiagnostics
            EnableBattlePresentationPhaseDiagnosticsForDiagnostics()
        {
            return diagnosticsModule.EnableBattlePresentationPhase();
        }

        public void DisableBattlePresentationPhaseDiagnosticsForDiagnostics()
        {
            diagnosticsModule.DisableBattlePresentationPhase();
        }

        public ulong CaptureRuntimeChecksum64(int tickIndex, FrameInputSet frameInput)
        {
            return lockstepChecksumModule.Capture(this, tickIndex, frameInput);
        }

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

        internal static string NormalizeTraceAssetCue(string value)
        {
            return BattleParitySnapshotModule.NormalizeTraceAssetCue(value);
        }

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

        internal void BeginDataObjectTypeTickCache(int tickIndex)
        {
            ActiveDataObjectTypeCacheTick = tickIndex;
        }

        internal void EndDataObjectTypeTickCache()
        {
            ActiveDataObjectTypeCacheTick = -1;
        }

        public void ApplyFrameInputSet(FrameInputSet frameInput)
        {
            frameInputModule.ApplyFrameInputSet(frameInput);
        }

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

        internal void DiscardDirectLocalInputTick(int tickIndex)
        {
            frameInputModule.DiscardDirectLocalInputTick(tickIndex);
        }

        internal bool TryResolveRosterInputEntity(int playerSlot, out LF2Entity entity)
        {
            return frameInputModule.TryResolveRosterInputEntity(playerSlot, out entity);
        }

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

        internal void RefreshActiveHumanRosterInputBindings()
        {
            frameInputModule.RefreshActiveHumanRosterInputBindings();
        }

        internal bool IsBoundActiveHumanRosterInputEntity(LF2Entity entity)
        {
            return frameInputModule.IsBoundActiveHumanRosterInputEntity(entity);
        }

        internal bool ResetCooldownsForRuntimeSlot(
            int runtimeSlot,
            LF2Entity occupant)
        {
            return queryAndLinkModule.ResetCooldownsForRuntimeSlot(
                runtimeSlot,
                occupant);
        }

        public void HeldObjectProcessAll(int tickIndex)
        {
            queryAndLinkModule.HeldObjectProcessAll(tickIndex);
        }

        public void ValidateHeldLinksAll(int tickIndex)
        {
            battleEcsPositiveLinkValidationPass.Execute(tickIndex);
        }

        internal void RunLegacyPositiveLinkValidation(int tickIndex)
        {
            queryAndLinkModule.RunLegacyPositiveLinkValidation(tickIndex);
        }

        public LF2Entity FindEntityByRuntimeSlotForQuery(int runtimeSlot)
        {
            return queryAndLinkModule.FindEntityByRuntimeSlotCurrent(runtimeSlot);
        }

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

        private LF2Entity FindEntityByRuntimeSlotCurrent(int runtimeSlot)
        {
            return queryAndLinkModule.FindEntityByRuntimeSlotCurrent(runtimeSlot);
        }

        public void GetAllLivingObjects(List<LF2LivingObject> destination)
        {
            queryAndLinkModule.GetAllLivingObjects(destination);
        }

        public void GetAllEntities(List<LF2Entity> destination)
        {
            queryAndLinkModule.GetAllEntities(destination);
        }

        private void GetActiveEntitiesByRuntimeSlot(List<LF2Entity> destination)
        {
            queryAndLinkModule.GetActiveEntitiesByRuntimeSlot(destination);
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

        internal void BeginDeferredEntityMutationPass()
        {
            _ticking = true;
        }

        internal void EndDeferredEntityMutationPass()
        {
            _ticking = false;
            FlushPendingUnregister();
            FlushPendingEntityDestroy();
        }

        /// <summary>
        /// Allocates capacity for the battle-only hot paths before the allocation gate
        /// is sealed. This is a migration seam: the caches still live in legacy
        /// partial files, while battle bootstrap owns the only production preparation
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

            EnsureListCapacity(earlyState500Handles, entityCapacity);
            EnsureListCapacity(earlyState501Handles, entityCapacity);

            int registeredCapacity = System.Math.Max(entityCapacity, ObjectCount);
            stageRenderModule.PrepareCapacity(entityCapacity, registeredCapacity);

            (SceneQuery as NTSD.Animation.BruteForceSceneQuery)?
                .PrepareBattleCapacity(
                    entityCapacity,
                    maximumBodyCountPerEntity,
                    maximumItrCountPerEntity);

            PrepareAiDecisionHotPathCapacity(entityCapacity);
        }

        internal void PrepareEnabledBattleDiagnosticsHotPath()
        {
            diagnosticsModule.PrepareEnabledProfilerMarkers();
        }

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
            if (aiDecisionShadowSnapshot == null ||
                aiDecisionShadowSnapshot.Rows.Capacity != capacity)
            {
                aiDecisionShadowSnapshot = new AiDecisionSnapshot(capacity);
            }

            if (aiDecisionSharedRows == null ||
                aiDecisionSharedRows.Capacity != capacity)
            {
                aiDecisionSharedRows = new AiSoASensingRows(capacity);
            }
            if (aiDecisionSharedSnapshot == null ||
                !object.ReferenceEquals(aiDecisionSharedSnapshot.Rows, aiDecisionSharedRows))
            {
                aiDecisionSharedSnapshot = new AiDecisionSnapshot(aiDecisionSharedRows);
            }
            if (aiDecisionIndexedSnapshot == null ||
                !object.ReferenceEquals(aiDecisionIndexedSnapshot.Rows, aiDecisionSharedRows))
            {
                aiDecisionIndexedSnapshot = new AiDecisionSnapshot(aiDecisionSharedRows);
            }

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

    }
}
