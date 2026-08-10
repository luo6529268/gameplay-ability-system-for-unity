using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
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
        private readonly BattleLockstepChecksumModule lockstepChecksumModule;
        private readonly SimulationDiagnosticsModule diagnosticsModule =
            new SimulationDiagnosticsModule();
        private readonly SimulationWorldMutationTracker runtimeMutationTracker =
            new SimulationWorldMutationTracker();
        private readonly SimulationWorldHooks runtimeHooks =
            new SimulationWorldHooks();

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
        public SimulationRuntimeCapacityModule RuntimeCapacity => runtimeCapacityModule;
        internal SimulationBattleBufferModule BattleBuffersForServices => battleBuffers;
        internal RuntimeCharacterConfigResolver RuntimeCharacterConfigs =>
            runtimeCharacterConfigs;
        internal StageSpawnTaskConfigurator StageSpawnTaskConfigurator =>
            stageSpawnTaskConfigurator;

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
            stageRenderModule.ClampCharacterZToStageBoundsAll();
        }

        public void ApplyPreFrameBoundsAll()
        {
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
            stageRenderModule.UpdateBattleResultsFlow();
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
            if (string.IsNullOrEmpty(soundId))
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
            queryAndLinkModule.ValidateHeldLinksAll(tickIndex);
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
