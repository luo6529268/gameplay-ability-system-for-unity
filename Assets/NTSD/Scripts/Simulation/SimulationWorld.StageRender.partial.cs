using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using NTSD.Simulation.Presentation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 的关卡边界、摄像机和渲染相关 pass。
    /// </summary>
    public partial class SimulationWorld
    {
        // These are presentation-only Unity sorting sub-orders. P1 renders Shadow,
        // Entity, and HitRecord; Overlay remains a reserved P3 slot until it has a
        // production consumer.
        internal const int PresentationShadowSubOrder = 0;
        internal const int PresentationEntitySubOrder = 1;
        internal const int PresentationReservedOverlaySubOrder = 2;
        internal const int PresentationHitRecordSubOrder = 3;
        private const int PresentationSubOrderCount = 4;

        // Legacy SpriteRenderer sortingOrder is a signed 16-bit value. Reserving
        // four contiguous presentation positions per entity leaves 8192 published
        // entities before a positive sorting order would overflow. Central rendering
        // removes this temporary legacy-backend limit.
        internal const int LegacySpriteRendererMaxPresentationEntities =
            (short.MaxValue + 1) / PresentationSubOrderCount;

        private readonly Dictionary<LF2Entity, PresentationRenderOrder> _presentationRenderOrders =
            new Dictionary<LF2Entity, PresentationRenderOrder>();
        private static readonly System.Comparison<LF2Entity> PresentationOrderComparison =
            ComparePresentationRenderOrder;
        private readonly List<LF2Entity> _presentationRenderScratch = new List<LF2Entity>(128);
        private readonly List<ISimObject> _rendererSnapshotScratch = new List<ISimObject>(128);
        private static readonly System.Comparison<ISimObject> RendererStableIdComparison =
            CompareRendererStableId;
        private readonly BattlePresentationCoordinator _battlePresentation =
            new BattlePresentationCoordinator();
        private BattlePixelFramePlan _currentPixelFramePlan;

        public BattlePresentationCoordinator BattlePresentation => _battlePresentation;
        public BattlePixelFramePlan CurrentPixelFramePlan => _currentPixelFramePlan;

        internal void PublishPixelFramePlan(BattlePixelFramePlan plan)
        {
            _currentPixelFramePlan = plan;
        }

        public void SetBattlePresentationBackend(BattlePresentationBackendMode mode)
        {
            _battlePresentation.SetMode(mode);
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
            Runtime?.Stage?.SetSceneSnapshot(stageWidth, zMin, zMax, perspectiveNear, perspectiveFar);
            _hasExplicitStageRuntimeSnapshot = true;
        }

        private static void ResolveUnityStageRuntime(out int stageWidth, out int zMin, out int zMax, out int perspectiveNear, out int perspectiveFar)
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
            Runtime?.Stage?.SetSceneSnapshot(stageWidth, zMin, zMax, perspectiveNear, perspectiveFar);
        }

        public void ClampCharacterZToStageBoundsAll()
        {
            RefreshStageRuntimeSnapshotFromScene();
            float zMin = Runtime?.Stage?.ZMin ?? 180;
            float zMax = Runtime?.Stage?.ZMax ?? 350;
            if (zMax < zMin)
                return;

            ForEachEntityByRuntimeSlot(entity =>
            {
                if (!entity.IsStageBoundedCharacter() || entity.PS == null)
                    return;

                if (entity.PS.z > zMax) entity.PS.z = zMax;
                if (entity.PS.z < zMin) entity.PS.z = zMin;
                RefreshRuntimeSnapshot(entity);
            });
        }

        public void ApplyPreFrameBoundsAll()
        {
            RefreshStageRuntimeSnapshotFromScene();
            int stageWidthPx = Runtime?.Stage?.StageWidthPx ?? 800;
            int baseStageWidthPx = Runtime?.Stage?.BaseStageWidthPx ?? 800;
            int xMaxOverride = Runtime?.Stage?.XMaxOverride ?? 0;
            int stageZMin = Runtime?.Stage?.ZMin ?? 180;
            int stageZMax = Runtime?.Stage?.ZMax ?? 350;

            float zMin = stageZMin;
            float zMax = stageZMax;
            float baseStageWidth = baseStageWidthPx;
            if (zMax < zMin || baseStageWidth <= 0f)
                return;

            ForEachEntityByRuntimeSlot(entity =>
            {
                if (entity == null || entity.PS == null)
                    return;

                entity.ApplyPreFrameZBounds(zMin, zMax);

                bool destroyed = entity.ApplyPreFrameXBounds(baseStageWidth, xMaxOverride);
                if (!destroyed)
                    RefreshRuntimeSnapshot(entity);
            });

            ResetUnityFixedWorldRenderOffsets();
        }

        public void RenderDispatchAll(int tickIndex)
        {
            BuildPresentationRenderOrder();
            _battlePresentation.BeginFrame(this, tickIndex);
            BattlePixelFramePlan plan = BattleCentralRenderSystem.PrepareFrame(this);
            if (RequiresLegacySpriteRendererCapacityGuard(plan))
                ValidateLegacySpriteRendererPresentationCapacity(_presentationRenderOrders.Count);
            LateRendererUpdateAll(tickIndex);
        }

        internal static bool RequiresLegacySpriteRendererCapacityGuard(BattlePixelFramePlan plan)
        {
            return !plan.IsValid || plan.Owner == BattlePixelFrameOwner.Legacy;
        }

        internal void GetPresentationEntitiesNoAlloc(List<LF2Entity> destination)
        {
            if (destination == null)
                return;

            destination.Clear();
            for (int slot = 0; slot < RuntimeSlotCapacity; slot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(slot);
                if (entity != null && IsActiveForCurrentPassInternal(entity))
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
            if (!TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
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
            if (!TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
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
            if (!TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
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
            GetPresentationEntitiesNoAlloc(_presentationRenderScratch);
            _presentationRenderScratch.Sort(PresentationOrderComparison);
            _presentationRenderOrders.Clear();

            int rank = 0;
            for (int i = 0; i < _presentationRenderScratch.Count; i++)
            {
                LF2Entity entity = _presentationRenderScratch[i];
                int slot = entity?.Runtime?.SlotIndex ?? -1;
                if (entity == null || slot < 0 ||
                    !TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                {
                    continue;
                }

                _presentationRenderOrders[entity] = new PresentationRenderOrder(handle, rank);
                rank++;
            }

            _presentationRenderScratch.Clear();
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
            if (entity != null &&
                _presentationRenderOrders.TryGetValue(entity, out PresentationRenderOrder published) &&
                TryResolveRuntimeHandle(published.Handle, out LF2Entity current) &&
                ReferenceEquals(current, entity))
            {
                return checked(published.Rank * PresentationSubOrderCount +
                               Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder));
            }

            // ForceRefreshPresentation can run before the normal render pass. Build
            // the same active map on demand rather than deriving a Unity order from a
            // sparse runtime slot. An unregistered/stale entity remains isolated at
            // its requested sub-order until it is published by a later render pass.
            if (entity != null && IsActiveForCurrentPass(entity))
            {
                BuildPresentationRenderOrder();
                if (_presentationRenderOrders.TryGetValue(entity, out published) &&
                    TryResolveRuntimeHandle(published.Handle, out current) &&
                    ReferenceEquals(current, entity))
                {
                    return checked(published.Rank * PresentationSubOrderCount +
                                   Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder));
                }
            }

            return Mathf.Clamp(subOrder, PresentationShadowSubOrder, PresentationHitRecordSubOrder);
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
            _cameraX = 0;
            _cameraVel = 0;
            GetAllEntities(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity?.Runtime == null)
                    continue;

                entity.Runtime.RenderOffsetX = 0f;
            }

            _entityScratch.Clear();
        }

        private List<ISimObject> BuildRendererSnapshot()
        {
            _rendererSnapshotScratch.Clear();
            if (_buckets.TryGetValue(SimOrderConstants.Renderer, out Bucket bucket))
            {
                bucket.EnsureSorted(GetRuntimeStableId);
                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity) continue;
                    if (bucket.items[i] is LF2ObjectRenderer)
                        _rendererSnapshotScratch.Add(bucket.items[i]);
                }
            }

            _rendererSnapshotScratch.Sort(RendererStableIdComparison);
            return _rendererSnapshotScratch;
        }

        private static int CompareRendererStableId(ISimObject left, ISimObject right)
        {
            return (left?.StableId ?? int.MaxValue).CompareTo(
                right?.StableId ?? int.MaxValue);
        }

        private void LateRendererUpdateAll(int tickIndex)
        {
            var snapshot = BuildRendererSnapshot();
            for (int i = 0; i < snapshot.Count; i++)
            {
                ISimObject obj = snapshot[i];
                if (obj == null || !IsActiveForCurrentPass(obj))
                    continue;

                obj.SimLateTick(tickIndex);
            }
        }

        public void UpdateBattleResultsFlow()
        {
            BattleRuntimeState battle = Runtime;
            if (battle?.Match?.BattleGameModeId != 1)
                return;

            battle.Results ??= new BattleResultsRuntimeState();
            BattleResultsRuntimeState results = battle.Results;
            if (results.IsActive)
                return;

            BattleSlotRuntimeState[] rosterSlots = battle.Roster?.Slots;
            if (rosterSlots == null)
                return;

            int[] teamIds = { -1, -1 };
            int[] alive = new int[2];
            int teamCount = 0;
            int slotCount = rosterSlots.Length < 8 ? rosterSlots.Length : 8;

            for (int slotIndex = 0; slotIndex < slotCount; slotIndex++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[slotIndex];
                if (rosterSlot == null)
                    continue;

                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(rosterSlot.RuntimeSlotIndex);
                if (entity == null ||
                    entity.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                {
                    continue;
                }

                // Authority GameTick keeps a fixed 0..7 slot in the result scan and
                // skips only when the slot state is dormant and the bound entity is
                // inactive. An active entity must remain eligible even when its
                // roster metadata has already been marked inactive.
                if (!rosterSlot.Active && !IsActiveForCurrentPass(entity))
                    continue;

                int team = entity.RelationTeam != 0 ? entity.RelationTeam : rosterSlot.Team;
                if (team == 0)
                    continue;

                int bucket = -1;
                for (int i = 0; i < teamCount; i++)
                {
                    if (teamIds[i] == team)
                    {
                        bucket = i;
                        break;
                    }
                }

                if (bucket < 0 && teamCount < teamIds.Length)
                {
                    bucket = teamCount;
                    teamIds[teamCount++] = team;
                }

                if (bucket >= 0 && IsActiveForCurrentPass(entity) && entity.Health != null && entity.Health.HP > 0)
                    alive[bucket]++;
            }

            if (alive[0] > 0 && alive[1] > 0)
                results.HadBoth = true;

            if (!results.HadBoth || teamCount < 2)
                return;

            results.EnsureTeamIds();
            if (alive[0] > 0 && alive[1] > 0)
            {
                results.BattleEndPhase = 0;
                results.PendingWinner = -2;
                results.TeamCount = teamCount;
                results.TeamIds[0] = teamIds[0];
                results.TeamIds[1] = teamIds[1];
                return;
            }

            int decidedWinner = alive[0] > 0 ? 0 : alive[1] > 0 ? 1 : -1;
            if (results.BattleEndPhase == 0)
            {
                results.BattleEndPhase = 1;
                results.PendingWinner = decidedWinner;
            }
            else
            {
                results.BattleEndPhase++;
            }

            results.TeamCount = teamCount;
            results.TeamIds[0] = teamIds[0];
            results.TeamIds[1] = teamIds[1];

            if (results.BattleEndPhase >= 11)
                results.ActivateSummary(results.PendingWinner, teamCount, teamIds[0], teamIds[1]);
        }
    }
}
