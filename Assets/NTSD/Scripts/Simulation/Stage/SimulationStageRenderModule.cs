using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Extensions;
using NTSD.LevelEditor;
using NTSD.Simulation.Presentation;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Simulation
{
    internal sealed class SimulationStageRenderModule
    {
        private const int PresentationSubOrderCount = 4;
        private const int PresentationShadowSubOrder =
            SimulationWorld.PresentationShadowSubOrder;
        private const int PresentationEntitySubOrder =
            SimulationWorld.PresentationEntitySubOrder;
        private const int PresentationHitRecordSubOrder =
            SimulationWorld.PresentationHitRecordSubOrder;
        private const int LegacySpriteRendererMaxPresentationEntities =
            SimulationWorld.LegacySpriteRendererMaxPresentationEntities;
        private readonly SimulationWorld world;

        private PresentationRenderOrder[] _presentationRenderOrders =
            new PresentationRenderOrder[128];
        private int[] _presentationRenderOrderEpochs = new int[128];
        private int _presentationRenderOrderEpoch = 1;
        private int _presentationRenderOrderCount;
        private static readonly System.Comparison<LF2Entity> PresentationOrderComparison =
            ComparePresentationRenderOrder;
        private readonly List<LF2Entity> _presentationRenderScratch = new List<LF2Entity>(128);
        private readonly List<ISimObject> _rendererSnapshotScratch = new List<ISimObject>(128);
        private readonly BattlePresentationCoordinator _battlePresentation =
            new BattlePresentationCoordinator();
        private BattlePixelFramePlan _currentPixelFramePlan;
        private bool skipLateRendererUpdateForDiagnostics;
        private bool forceLegacyPerPassStageRefreshForDiagnostics;
        private int preparedStageRuntimeTick = int.MinValue;

        internal SimulationStageRenderModule(SimulationWorld world)
        {
            this.world = world ?? throw new System.ArgumentNullException(nameof(world));
        }

        internal void Reset()
        {
            _battlePresentation.Reset();
            _currentPixelFramePlan = default;
            _presentationRenderScratch.Clear();
            _rendererSnapshotScratch.Clear();
            _presentationRenderOrderCount = 0;
            _hasExplicitStageRuntimeSnapshot = false;
            skipLateRendererUpdateForDiagnostics = false;
            forceLegacyPerPassStageRefreshForDiagnostics = false;
            preparedStageRuntimeTick = int.MinValue;
            StageRuntimeSceneRefreshCountForDiagnostics = 0;
            StageRuntimeHostPrepareCountForDiagnostics = 0;
            StageRuntimeHostReuseCountForDiagnostics = 0;
            StageRuntimeLegacyPerPassRefreshCountForDiagnostics = 0;
        }

        internal void PrepareCapacity(int entityCapacity, int registeredCapacity)
        {
            if (_presentationRenderScratch.Capacity < entityCapacity)
                _presentationRenderScratch.Capacity = entityCapacity;
            if (_rendererSnapshotScratch.Capacity < registeredCapacity)
                _rendererSnapshotScratch.Capacity = registeredCapacity;
            EnsurePresentationRenderOrderCapacity(entityCapacity);
            _battlePresentation.PrepareCapacity(entityCapacity);
        }

        internal BattlePresentationCoordinator BattlePresentation => _battlePresentation;
        internal BattlePixelFramePlan CurrentPixelFramePlan => _currentPixelFramePlan;
        internal int LateRendererUpdateInvocationCountForDiagnostics { get; private set; }
        internal long CentralOnlyRendererShellBypassCountForDiagnostics { get; private set; }
        internal int PresentationRenderOrderBuildCountForDiagnostics { get; private set; }
        internal int PresentationRenderOrderReusePublishCountForDiagnostics { get; private set; }
        internal int PresentationEntityScanAndSortCountForDiagnostics { get; private set; }
        internal bool SkipLateRendererUpdateForDiagnostics =>
            skipLateRendererUpdateForDiagnostics;
        internal long SkippedLateRendererUpdateTickCountForDiagnostics { get; private set; }
        internal bool ForceLegacyPerPassStageRefreshForDiagnostics =>
            forceLegacyPerPassStageRefreshForDiagnostics;
        internal long StageRuntimeSceneRefreshCountForDiagnostics { get; private set; }
        internal long StageRuntimeHostPrepareCountForDiagnostics { get; private set; }
        internal long StageRuntimeHostReuseCountForDiagnostics { get; private set; }
        internal long StageRuntimeLegacyPerPassRefreshCountForDiagnostics { get; private set; }

        public bool ConfigureSkipLateRendererUpdateForDiagnostics(
            bool requested,
            bool simulationOnly)
        {
            if (requested && !simulationOnly)
            {
                throw new System.InvalidOperationException(
                    "SkipLateRendererUpdate may only be enabled for a simulation-only diagnostic run.");
            }

            bool previous = skipLateRendererUpdateForDiagnostics;
            skipLateRendererUpdateForDiagnostics = requested;
            return previous;
        }

        public void RestoreSkipLateRendererUpdateForDiagnostics(bool previous)
        {
            skipLateRendererUpdateForDiagnostics = previous;
        }

        internal bool ConfigureLegacyPerPassStageRefreshForDiagnostics(bool requested)
        {
            bool previous = forceLegacyPerPassStageRefreshForDiagnostics;
            forceLegacyPerPassStageRefreshForDiagnostics = requested;
            preparedStageRuntimeTick = int.MinValue;
            return previous;
        }

        internal void PrepareStageRuntimeSnapshotForTick(int tickIndex)
        {
            if (forceLegacyPerPassStageRefreshForDiagnostics)
                return;
            if (preparedStageRuntimeTick == tickIndex)
            {
                StageRuntimeHostReuseCountForDiagnostics++;
                return;
            }

            RefreshStageRuntimeSnapshotFromScene();
            preparedStageRuntimeTick = tickIndex;
            StageRuntimeHostPrepareCountForDiagnostics++;
        }

        internal void PrepareStageRuntimeForKernelPass()
        {
            if (!forceLegacyPerPassStageRefreshForDiagnostics)
                return;

            RefreshStageRuntimeSnapshotFromScene();
            StageRuntimeLegacyPerPassRefreshCountForDiagnostics++;
        }

        internal void PublishPixelFramePlan(BattlePixelFramePlan plan)
        {
            _currentPixelFramePlan = plan;
        }

        public void SetBattlePresentationBackend(BattlePresentationBackendMode mode)
        {
            _battlePresentation.SetMode(mode);
            RefreshLegacyRendererSuppressionForBackend(mode);
        }

        private void RefreshLegacyRendererSuppressionForBackend(
            BattlePresentationBackendMode mode)
        {
            bool suppressLegacyRenderer =
                mode == BattlePresentationBackendMode.CentralOnly;
            List<ISimObject> renderers = BuildRendererSnapshot();
            for (int index = 0; index < renderers.Count; index++)
            {
                if (renderers[index] is LF2ObjectRenderer renderer)
                    renderer.RefreshLegacyRendererSuppression(suppressLegacyRenderer);
            }
        }

        private readonly struct PresentationRenderOrder
        {
            public PresentationRenderOrder(RuntimeEntityHandle handle, int rank)
            {
                Handle = handle;
                Rank = rank;
            }

            public RuntimeEntityHandle Handle { get; }
            public int Rank { get; }
        }

        private bool _hasExplicitStageRuntimeSnapshot;

        public void SetExplicitStageRuntimeSnapshotForTesting(
            int stageWidth,
            int zMin,
            int zMax,
            int perspectiveNear,
            int perspectiveFar)
        {
            world.Runtime?.Stage?.SetSceneSnapshot(
                stageWidth,
                zMin,
                zMax,
                perspectiveNear,
                perspectiveFar);
            _hasExplicitStageRuntimeSnapshot = true;
            preparedStageRuntimeTick = int.MinValue;
        }

        internal static void ResolveUnityStageRuntime(
            out int stageWidth,
            out int zMin,
            out int zMax,
            out int perspectiveNear,
            out int perspectiveFar)
        {
            var cfg = NTSD.App.GameConfig.Instance;
            stageWidth = cfg != null ? Mathf.Max(cfg.BattleStageWidthPx, NTSDRenderSpace.SourceScreenWidth) : 800;
            zMin = cfg != null ? cfg.BattleStageZMinPx : 180;
            zMax = cfg != null ? Mathf.Max(cfg.BattleStageZMaxPx, zMin + 1) : 350;
            perspectiveNear = cfg != null ? cfg.BattlePerspectiveNear : 0;
            perspectiveFar = cfg != null ? cfg.BattlePerspectiveFar : 0;

            BoundaryWallManager manager = BoundaryWallManager.Instance;
            if (manager != null && manager.TryGetBattleStageRuntime(out int boundaryStageWidth, out int boundaryZMin, out int boundaryZMax))
            {
                stageWidth = boundaryStageWidth;
                zMin = boundaryZMin;
                zMax = boundaryZMax;
            }
        }

        public bool IsGroundPointWalkable(Vector2 pointXY)
        {
            BoundaryWallManager manager = BoundaryWallManager.Instance;
            if (manager == null)
                return true;

            return manager.IsPointWalkable(pointXY);
        }

        public void RefreshStageRuntimeSnapshotFromScene()
        {
            if (_hasExplicitStageRuntimeSnapshot)
                return;

            ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out int perspectiveNear, out int perspectiveFar);
            world.Runtime?.Stage?.SetSceneSnapshot(
                stageWidth,
                zMin,
                zMax,
                perspectiveNear,
                perspectiveFar);
            StageRuntimeSceneRefreshCountForDiagnostics++;
        }

        public void ClampCharacterZToStageBoundsAll()
        {
            float zMin = world.Runtime?.Stage?.ZMin ?? 180;
            float zMax = world.Runtime?.Stage?.ZMax ?? 350;
            if (zMax < zMin)
                return;

            foreach (LF2Entity entity in world.ActiveEntitiesByRuntimeSlotForModule)
            {
                if (!entity.IsStageBoundedCharacter() || entity.PS == null)
                    continue;

                if (entity.PS.z > zMax) entity.PS.z = zMax;
                if (entity.PS.z < zMin) entity.PS.z = zMin;
                entity.Runtime.ZInt = (int)entity.Runtime.Z;
                entity.RefreshRuntimeSnapshot();
            }
        }

        public void ApplyPreFrameBoundsAll()
        {
            world.RunBattleEcsCharacterPreFrameBoundsPass();
            ResetUnityFixedWorldRenderOffsets();
        }

        internal void RunLegacyPreFrameBoundsAll()
        {
            int stageWidthPx = world.Runtime?.Stage?.StageWidthPx ?? 800;
            int baseStageWidthPx = world.Runtime?.Stage?.BaseStageWidthPx ?? 800;
            int xMaxOverride = world.Runtime?.Stage?.XMaxOverride ?? 0;
            int stageZMin = world.Runtime?.Stage?.ZMin ?? 180;
            int stageZMax = world.Runtime?.Stage?.ZMax ?? 350;

            float zMin = stageZMin;
            float zMax = stageZMax;
            float baseStageWidth = baseStageWidthPx;
            if (zMax < zMin || baseStageWidth <= 0f)
                return;

            foreach (LF2Entity entity in world.ActiveEntitiesByRuntimeSlotForModule)
            {
                if (entity.PS == null)
                    continue;

                entity.ApplyPreFrameZBounds(zMin, zMax);

                bool destroyed = entity.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride);
                if (!destroyed)
                    entity.RefreshRuntimeSnapshot();
            }

        }

        public void RenderDispatchAll(int tickIndex)
        {
            RenderDispatchAll(tickIndex, buildPresentation: true);
        }

        public void RenderDispatchAll(int tickIndex, bool buildPresentation)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            bool publishPresentation =
                buildPresentation ||
                _battlePresentation.Mode != BattlePresentationBackendMode.CentralOnly;
            if (publishPresentation)
            {
                BattlePresentationPhaseDiagnostics presentationDiagnostics =
                    world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
                presentationDiagnostics?.BeginTick(tickIndex);
                if (_battlePresentation.Mode != BattlePresentationBackendMode.CentralOnly)
                {
                    detailDiagnostics?.BeginPhase(BattleTickDetailPhase.RenderPresentationOrder);
                    BuildPresentationRenderOrder();
                    detailDiagnostics?.EndPhase(BattleTickDetailPhase.RenderPresentationOrder);
                }

                detailDiagnostics?.BeginPhase(BattleTickDetailPhase.RenderBeginFrame);
                presentationDiagnostics?.BeginPhase(
                    BattlePresentationPhase.BeginFrameTotal);
                try
                {
                    _battlePresentation.BeginFrame(world, tickIndex);
                }
                finally
                {
                    presentationDiagnostics?.EndPhase(
                        BattlePresentationPhase.BeginFrameTotal);
                    detailDiagnostics?.EndPhase(BattleTickDetailPhase.RenderBeginFrame);
                }

                presentationDiagnostics?.BeginPhase(
                    BattlePresentationPhase.QueueLatestPublishedFrame);
                try
                {
                    BattleCentralRenderSystem.QueueLatestPublishedFrame(world);
                }
                finally
                {
                    presentationDiagnostics?.EndPhase(
                        BattlePresentationPhase.QueueLatestPublishedFrame);
                }
            }

            if (!Application.isPlaying || Application.isBatchMode)
                PresentLatestFrame(tickIndex);
        }

        internal void CaptureSimulationWorkerPresentationFrame(int tickIndex)
        {
            _battlePresentation.BeginSimulationWorkerFrame(world, tickIndex);
        }

        internal void PresentLatestFrame(int tickIndex)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            BattleCentralRenderSystem.FlushLatestPublishedFrame(world);
            if (!BattleCentralRenderSystem.ShouldSuppressLegacyMaterializers(world))
            {
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard);
                ValidateLegacySpriteRendererPresentationCapacity(
                    _presentationRenderOrderCount);
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderPrepareFrameAndLegacyCapacityGuard);
            }

            detailDiagnostics?.BeginPhase(BattleTickDetailPhase.RenderLateRendererUpdate);
            if (SkipLateRendererUpdateForDiagnostics)
                SkippedLateRendererUpdateTickCountForDiagnostics++;
            else
                LateRendererUpdateAll(tickIndex);
            detailDiagnostics?.EndPhase(BattleTickDetailPhase.RenderLateRendererUpdate);
        }

        internal static bool RequiresLegacySpriteRendererCapacityGuard(BattlePixelFramePlan plan)
        {
            return !plan.SuppressesLegacyMaterializers;
        }

        internal void GetPresentationEntitiesNoAlloc(List<LF2Entity> destination)
        {
            if (destination == null)
                return;

            destination.Clear();
            for (int slot = 0; slot < world.RuntimeSlotCapacityForDiagnostics; slot++)
            {
                LF2Entity entity = world.FindEntityByRuntimeSlotIncludingDormant(slot);
                if (entity != null && world.IsActiveForCurrentPassInternal(entity))
                    destination.Add(entity);
            }
        }

        internal void RecordLegacyShadowProbe(LF2Entity entity, SpriteRenderer renderer)
        {
            if (!_battlePresentation.IsCapturingLegacyProbes || entity?.Runtime == null ||
                renderer == null || !renderer.enabled)
            {
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (!world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                return;

            BattleCommonVisualBinding shadowBinding =
                CharacterAnimtorManager.Instance?.CommonVisualCatalog?.Shadow;
            bool matchesCommonShadow = shadowBinding != null &&
                                       shadowBinding.MatchesSprite(renderer.sprite);
            BattleSpriteValueDescriptor descriptor = CaptureRendererDescriptor(
                renderer,
                out BattleSpriteRenderState renderState,
                matchesCommonShadow,
                BattleVisualResourceKey.CommonShadow);

            _battlePresentation.RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.Shadow,
                handle,
                entity.Runtime.StableId,
                -1,
                -1,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                0,
                renderer.transform.position,
                CaptureRendererSpriteSize(renderer),
                renderState,
                descriptor));
        }

        internal void RecordLegacyEntityProbe(LF2Entity entity, SpriteRenderer renderer)
        {
            if (!_battlePresentation.IsCapturingLegacyProbes || entity?.Runtime == null ||
                renderer == null || !renderer.enabled)
            {
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (!world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                return;

            BattleSpriteValueDescriptor descriptor = CaptureRendererDescriptor(
                renderer,
                out BattleSpriteRenderState renderState,
                true,
                BattleVisualResourceKey.FromEntity(new BattleSpriteKey(
                    LF2Entity.ResolveCurrentDataObjectId(entity),
                    entity.GetRenderPicIndex())));
            int visualDataId = descriptor.HasLogicalResourceKey &&
                               descriptor.LogicalResourceKey.IsEntitySprite
                ? descriptor.LogicalResourceKey.EntitySpriteKey.VisualDataId
                : -1;
            int effectivePic = descriptor.HasLogicalResourceKey &&
                               descriptor.LogicalResourceKey.IsEntitySprite
                ? descriptor.LogicalResourceKey.EntitySpriteKey.EffectivePic
                : -1;

            _battlePresentation.RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.Entity,
                handle,
                entity.Runtime.StableId,
                visualDataId,
                effectivePic,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                0,
                renderer.transform.position,
                CaptureRendererSpriteSize(renderer),
                renderState,
                descriptor));
        }

        internal void RecordLegacyHitRecordProbe(
            LF2Entity entity,
            SpriteRenderer renderer,
            int hitRecordIndex)
        {
            if (!_battlePresentation.IsCapturingLegacyProbes || entity?.Runtime == null ||
                renderer == null || !renderer.enabled)
            {
                return;
            }

            int slot = entity.Runtime.SlotIndex;
            if (!world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                return;

            BattleVisualResourceKey sparkKey = default;
            bool hasSparkKey = CharacterAnimtorManager.Instance?.CommonVisualCatalog?.TryGetSparkKey(
                renderer.sprite,
                out sparkKey) == true;
            BattleSpriteValueDescriptor descriptor = CaptureRendererDescriptor(
                renderer,
                out BattleSpriteRenderState renderState,
                hasSparkKey,
                sparkKey);

            _battlePresentation.RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.HitRecord,
                handle,
                entity.Runtime.StableId,
                -1,
                -1,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                hitRecordIndex,
                renderer.transform.position,
                CaptureRendererSpriteSize(renderer),
                renderState,
                descriptor));
        }

        private static Vector2 CaptureRendererSpriteSize(SpriteRenderer renderer)
        {
            Sprite sprite = renderer != null ? renderer.sprite : null;
            return sprite != null ? sprite.rect.size : Vector2.zero;
        }

        private static BattleSpriteValueDescriptor CaptureRendererDescriptor(
            SpriteRenderer renderer,
            out BattleSpriteRenderState renderState,
            bool hasPreferredKey = false,
            BattleVisualResourceKey preferredKey = default)
        {
            Sprite sprite = renderer != null ? renderer.sprite : null;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = Vector2.zero;
            if (sprite != null && rect.width > 0f && rect.height > 0f)
            {
                pivot = new Vector2(
                    sprite.pivot.x / rect.width,
                    sprite.pivot.y / rect.height);
            }

            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer != null ? renderer.sharedMaterial : null;
            BattleSpriteCatalog catalog = CharacterAnimtorManager.Instance?.SpriteCatalog ??
                                          BattleSpriteCatalog.Empty;
            BattleVisualResourceKey logicalResourceKey = default;
            bool hasLogicalResourceKey;
            if (hasPreferredKey &&
                (preferredKey.Kind == BattleVisualResourceKind.CommonShadow || preferredKey.IsCommonSpark))
            {
                logicalResourceKey = preferredKey;
                hasLogicalResourceKey = true;
            }
            else
            {
                BattleSpriteKey preferredEntityKey = preferredKey.EntitySpriteKey;
                bool foundEntityKey = hasPreferredKey && preferredKey.IsEntitySprite
                    ? catalog.TryGetKey(sprite, preferredEntityKey, out BattleSpriteKey entityKey)
                    : catalog.TryGetKey(sprite, out entityKey);
                logicalResourceKey = foundEntityKey
                    ? BattleVisualResourceKey.FromEntity(entityKey)
                    : default;
                hasLogicalResourceKey = foundEntityKey;
            }
            renderState = renderer != null
                ? new BattleSpriteRenderState(
                    renderer.color,
                    renderer.flipX,
                    renderer.flipY,
                    renderer.maskInteraction,
                    BattleSpriteMaterialContract.Classify(material))
                : default;
            return hasLogicalResourceKey
                ? new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    logicalResourceKey)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
        }

        /// <summary>
        /// Publishes a dense Unity presentation order from the release renderer's
        /// active (ZInt, runtime slot) ordering. This is intentionally not part of
        /// runtime state, checksums, or collision behavior.
        /// </summary>
        internal void BuildPresentationRenderOrder()
        {
            PresentationRenderOrderBuildCountForDiagnostics++;
            PresentationEntityScanAndSortCountForDiagnostics++;
            BattlePresentationPhaseDiagnostics presentationDiagnostics =
                world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
            presentationDiagnostics?.BeginPhase(
                BattlePresentationPhase.RenderOrderCollectAndSort);
            try
            {
                GetPresentationEntitiesNoAlloc(_presentationRenderScratch);
                _presentationRenderScratch.Sort(PresentationOrderComparison);
            }
            finally
            {
                presentationDiagnostics?.EndPhase(
                    BattlePresentationPhase.RenderOrderCollectAndSort);
            }
            PublishPresentationRenderOrderFromSortedEntities(_presentationRenderScratch);
            _presentationRenderScratch.Clear();
        }

        /// <summary>
        /// Publishes the dense Unity presentation order from a list already sorted by
        /// (ZInt, runtime slot). CentralOnly reuses the coordinator's capture list to
        /// avoid a second registry traversal and sort for the same presentation frame.
        /// </summary>
        internal void PublishPresentationRenderOrderFromSortedEntities(
            IReadOnlyList<LF2Entity> sortedEntities,
            bool reusesCoordinatorSort = false)
        {
            BattlePresentationPhaseDiagnostics presentationDiagnostics =
                world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
            presentationDiagnostics?.BeginPhase(
                BattlePresentationPhase.RenderOrderRankMapFill);
            try
            {
            if (reusesCoordinatorSort)
                PresentationRenderOrderReusePublishCountForDiagnostics++;
            AdvancePresentationRenderOrderEpoch();
            _presentationRenderOrderCount = 0;
            if (sortedEntities == null)
                return;

            EnsurePresentationRenderOrderCapacity(world.RuntimeSlotCapacityForDiagnostics);

            int rank = 0;
            for (int i = 0; i < sortedEntities.Count; i++)
            {
                LF2Entity entity = sortedEntities[i];
                int slot = entity?.Runtime?.SlotIndex ?? -1;
                if (entity == null || slot < 0 ||
                    !world.TryGetCurrentRuntimeHandle(
                        slot,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    continue;
                }

                EnsurePresentationRenderOrderCapacity(slot + 1);
                _presentationRenderOrders[slot] = new PresentationRenderOrder(handle, rank);
                _presentationRenderOrderEpochs[slot] = _presentationRenderOrderEpoch;
                _presentationRenderOrderCount++;
                rank++;
            }
            }
            finally
            {
                presentationDiagnostics?.EndPhase(
                    BattlePresentationPhase.RenderOrderRankMapFill);
            }
        }

        internal void PublishPresentationRenderOrderFromFrame(
            BattlePresentationFrame frame,
            bool reusesCoordinatorSort = false)
        {
            BattlePresentationPhaseDiagnostics presentationDiagnostics =
                world.ActiveBattlePresentationPhaseDiagnosticsForDiagnostics;
            presentationDiagnostics?.BeginPhase(
                BattlePresentationPhase.RenderOrderRankMapFill);
            try
            {
            if (reusesCoordinatorSort)
            {
                PresentationRenderOrderReusePublishCountForDiagnostics++;
                PresentationEntityScanAndSortCountForDiagnostics++;
            }
            AdvancePresentationRenderOrderEpoch();
            _presentationRenderOrderCount = 0;
            if (frame == null)
                return;

            EnsurePresentationRenderOrderCapacity(world.RuntimeSlotCapacityForDiagnostics);
            for (int rank = 0; rank < frame.EntityCount; rank++)
            {
                ref readonly BattlePresentationEntitySnapshot entity =
                    ref frame.GetEntityRef(rank);
                int slot = entity.RuntimeSlot;
                if (slot < 0)
                    continue;

                EnsurePresentationRenderOrderCapacity(slot + 1);
                _presentationRenderOrders[slot] =
                    new PresentationRenderOrder(entity.Handle, rank);
                _presentationRenderOrderEpochs[slot] = _presentationRenderOrderEpoch;
                _presentationRenderOrderCount++;
            }
            }
            finally
            {
                presentationDiagnostics?.EndPhase(
                    BattlePresentationPhase.RenderOrderRankMapFill);
            }
        }

        private void AdvancePresentationRenderOrderEpoch()
        {
            if (_presentationRenderOrderEpoch == int.MaxValue)
            {
                System.Array.Clear(
                    _presentationRenderOrderEpochs,
                    0,
                    _presentationRenderOrderEpochs.Length);
                _presentationRenderOrderEpoch = 1;
                return;
            }

            _presentationRenderOrderEpoch++;
        }

        private void EnsurePresentationRenderOrderCapacity(int required)
        {
            if (required <= _presentationRenderOrders.Length)
                return;

            int capacity = _presentationRenderOrders.Length;
            while (capacity < required)
                capacity = checked(capacity * 2);
            System.Array.Resize(ref _presentationRenderOrders, capacity);
            System.Array.Resize(ref _presentationRenderOrderEpochs, capacity);
        }

        internal void RecordPresentationEntityScanAndSortForDiagnostics()
        {
            PresentationEntityScanAndSortCountForDiagnostics++;
        }

        internal static void ValidateLegacySpriteRendererPresentationCapacity(
            int materializedEntityCount)
        {
            if (materializedEntityCount <= LegacySpriteRendererMaxPresentationEntities)
                return;

            throw new System.InvalidOperationException(
                "Legacy SpriteRenderer presentation supports at most " +
                LegacySpriteRendererMaxPresentationEntities +
                " materialized battle entities because it reserves four sorting orders per entity. " +
                "Use the central battle renderer before exceeding this temporary legacy limit.");
        }

        internal int GetPresentationRenderSortingOrder(LF2Entity entity, int subOrder)
        {
            int slot = entity?.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 &&
                slot < _presentationRenderOrders.Length &&
                _presentationRenderOrderEpochs[slot] == _presentationRenderOrderEpoch &&
                TryGetPublishedPresentationRenderOrder(slot, out PresentationRenderOrder published) &&
                world.TryResolveRuntimeHandle(published.Handle, out LF2Entity current) &&
                ReferenceEquals(current, entity))
            {
                return checked(published.Rank * PresentationSubOrderCount +
                               Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder));
            }

            // ForceRefreshPresentation can run before the normal render pass. Build
            // the same active map on demand rather than deriving a Unity order from a
            // sparse runtime slot. An unregistered/stale entity remains isolated at
            // its requested sub-order until it is published by a later render pass.
            if (entity != null && world.IsActiveForCurrentPassInternal(entity))
            {
                BuildPresentationRenderOrder();
                slot = entity.Runtime?.SlotIndex ?? -1;
                if (slot >= 0 &&
                    slot < _presentationRenderOrders.Length &&
                    _presentationRenderOrderEpochs[slot] == _presentationRenderOrderEpoch &&
                    TryGetPublishedPresentationRenderOrder(slot, out published) &&
                    world.TryResolveRuntimeHandle(published.Handle, out current) &&
                    ReferenceEquals(current, entity))
                {
                    return checked(published.Rank * PresentationSubOrderCount +
                                   Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder));
                }
            }

            return Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder);
        }

        private bool TryGetPublishedPresentationRenderOrder(
            int slot,
            out PresentationRenderOrder order)
        {
            if (slot >= 0 &&
                slot < _presentationRenderOrders.Length &&
                _presentationRenderOrderEpochs[slot] == _presentationRenderOrderEpoch)
            {
                order = _presentationRenderOrders[slot];
                return true;
            }

            order = default;
            return false;
        }

        private static int ComparePresentationRenderOrder(LF2Entity left, LF2Entity right)
        {
            int zComparison = (left?.GetRenderZInt() ?? int.MaxValue)
                .CompareTo(right?.GetRenderZInt() ?? int.MaxValue);
            if (zComparison != 0)
                return zComparison;

            int leftSlot = left?.Runtime?.SlotIndex ?? int.MaxValue;
            int rightSlot = right?.Runtime?.SlotIndex ?? int.MaxValue;
            int slotComparison = leftSlot.CompareTo(rightSlot);
            if (slotComparison != 0)
                return slotComparison;

            return (left?.StableId ?? int.MaxValue).CompareTo(right?.StableId ?? int.MaxValue);
        }

        internal void ResetUnityFixedWorldRenderOffsets()
        {
            // Unity battle scenes use fixed world coordinates. Keep entity, shadow,
            // and spark presentation independent from character-driven camera math.
            world.ResetUnityFixedWorldCameraStateForModule();
            world.GetAllEntities(_presentationRenderScratch);
            for (int i = 0; i < _presentationRenderScratch.Count; i++)
            {
                LF2Entity entity = _presentationRenderScratch[i];
                if (entity?.Runtime == null)
                    continue;

                entity.Runtime.RenderOffsetX = 0f;
            }

            _presentationRenderScratch.Clear();
        }

        private List<ISimObject> BuildRendererSnapshot()
        {
            world.GetNonEntityRendererObjectsForModule(_rendererSnapshotScratch);
            return _rendererSnapshotScratch;
        }

        private void LateRendererUpdateAll(int tickIndex)
        {
            if (_battlePresentation.Mode == BattlePresentationBackendMode.CentralOnly)
            {
                // CentralOnly captures the required entity visibility, frame, facing,
                // position, shadow and local-offset facts directly into the published
                // frame. Re-running every LF2ObjectRenderer only rewrites the same
                // managed presentation state and never contributes a central command.
                CentralOnlyRendererShellBypassCountForDiagnostics++;
                return;
            }

            LateRendererUpdateInvocationCountForDiagnostics++;
            var snapshot = BuildRendererSnapshot();
            for (int i = 0; i < snapshot.Count; i++)
            {
                ISimObject obj = snapshot[i];
                if (obj == null || !world.IsActiveForCurrentPassInternal(obj))
                    continue;

                obj.SimLateTick(tickIndex);
            }
        }

    }
}

